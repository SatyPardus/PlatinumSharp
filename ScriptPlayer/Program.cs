using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json;
using NAudio.Wave;
using PlatinumSharp.Game;
using PlatinumSharp.Nitro;

namespace PlatinumSharp.ScriptPlayer;

internal static class Program
{
    public const string DefaultRom = @"E:\Poketest\Pokemon - Platinum Version (Europe).nds";
    public const string DataPath = @"E:\Poketest\tools\scriptdata.json";

    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        string? romPath = args.FirstOrDefault(a => a.EndsWith(".nds", StringComparison.OrdinalIgnoreCase));
        if (romPath == null && File.Exists(DefaultRom))
        {
            romPath = DefaultRom;
        }
        if (romPath == null)
        {
            using OpenFileDialog dialog = new() { Filter = "Nintendo DS ROM (*.nds)|*.nds" };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return 1;
            }
            romPath = dialog.FileName;
        }
        string dataPath = Path.Combine(AppContext.BaseDirectory, "scriptdata.json");
        if (!File.Exists(dataPath))
        {
            dataPath = DataPath;
        }
        if (!File.Exists(dataPath))
        {
            MessageBox.Show("scriptdata.json was not found. Run tools\\screxport.py first.", "Script Player");
            return 1;
        }
        ScriptData data = JsonSerializer.Deserialize<ScriptData>(File.ReadAllText(dataPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        string? state = args.SkipWhile(a => a != "--state").Skip(1).FirstOrDefault();
        if (state == null && !args.Contains("--nostate"))
        {
            state = PlayerForm.States().FirstOrDefault();
        }
        string? test = args.SkipWhile(a => a != "--test").Skip(1).FirstOrDefault();
        if (test != null)
        {
            string[] parts = test.Split(',');
            PlayerForm.TestScriptID = int.Parse(parts[0]);
            PlayerForm.TestObject = int.Parse(parts[1]);
            PlayerForm.TestSteps = int.Parse(parts[2]);
            PlayerForm.TestShot = parts[3];
        }
        using PlayerForm form = new(romPath, data, state, args.Contains("--mute"));
        Application.Run(form);
        return 0;
    }
}

internal sealed class ScriptData
{
    public List<MapInfo> Maps { get; set; } = new();
    public List<CommonInfo> Common { get; set; } = new();
    public Dictionary<string, PageInfo> Pages { get; set; } = new();
    public List<string> Commands { get; set; } = new();
}

internal sealed class MapInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int File { get; set; }
    public string Page { get; set; } = "";
    public List<EventInfo> Events { get; set; } = new();
    public List<int[]> Warps { get; set; } = new();
    public List<int[]> Objects { get; set; } = new();
    public int Scripts { get; set; }

    public override string ToString()
    {
        return $"{Id,3}  {Name}";
    }
}

internal sealed class EventInfo
{
    public int Script { get; set; }
    public int Object { get; set; }
    public string Label { get; set; } = "";
}

internal sealed class CommonInfo
{
    public int Base { get; set; }
    public int File { get; set; }
    public string Page { get; set; } = "";
}

internal sealed class PageInfo
{
    public List<int> Entries { get; set; } = new();
    public List<JsonElement[]> Lines { get; set; } = new();
}

internal sealed class ScriptChoice
{
    public int Script;
    public int Object = ScriptPlayback.NoObject;
    public string Label = "";

    public override string ToString()
    {
        return Label;
    }
}

internal sealed class ArrivalChoice
{
    public int Warp = -1;
    public int X;
    public int Z;
    public string Label = "";

    public override string ToString()
    {
        return Label;
    }
}

internal sealed class ListingLine
{
    public int Offset;
    public string Text = "";
    public int Kind;
}

internal sealed class GameAudio : ISampleProvider
{
    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    public int Read(float[] buffer, int offset, int count)
    {
        try
        {
            Audio.Render(buffer, offset, count / 2);
        }
        catch (Exception)
        {
            Array.Clear(buffer, offset, count);
        }
        return count;
    }
}

internal sealed class PlayerForm : Form
{
    private const int Zoom = 2;
    private const int PanelWidth = 640;
    private const int FrameMicroseconds = 16715;
    private const ushort PowerMainOnTop = 0x8000;
    private const int KindCommand = 0;
    private const int KindLabel = 1;
    private const int KindDialogue = 2;
    private const int NoLine = -1;

    private static readonly Color panelColor = Color.FromArgb(30, 32, 38);
    private static readonly Color fieldColor = Color.FromArgb(42, 45, 53);
    private static readonly Color inkColor = Color.FromArgb(226, 228, 233);
    private static readonly Color dimColor = Color.FromArgb(140, 146, 160);
    private static readonly Color labelColor = Color.FromArgb(255, 150, 130);
    private static readonly Color dialogueColor = Color.FromArgb(140, 214, 164);
    private static readonly Color currentColor = Color.FromArgb(30, 96, 120);
    private static readonly Color blockedColor = Color.FromArgb(150, 54, 44);

    private static readonly (Keys Key, int Mask, string Name)[] bindings =
    {
        (Keys.X, Input.KeyA, "A"), (Keys.Z, Input.KeyB, "B"), (Keys.S, Input.KeyX, "X"), (Keys.A, Input.KeyY, "Y"),
        (Keys.Q, Input.KeyL, "L"), (Keys.W, Input.KeyR, "R"), (Keys.Enter, Input.KeyStart, "Start"), (Keys.Back, Input.KeySelect, "Select"),
        (Keys.Up, Input.KeyUp, "Up"), (Keys.Down, Input.KeyDown, "Down"), (Keys.Left, Input.KeyLeft, "Left"), (Keys.Right, Input.KeyRight, "Right"),
    };

    private readonly ScriptData data;
    private readonly string romPath;
    private readonly bool muted;
    private readonly System.Drawing.Bitmap frame = new(Rasterizer.Width, Rasterizer.Height * 2, PixelFormat.Format32bppArgb);
    private readonly int[] mainPixels = new int[Rasterizer.Width * Rasterizer.Height];
    private readonly int[] subPixels = new int[Rasterizer.Width * Rasterizer.Height];
    private readonly int[] presented = new int[Rasterizer.Width * Rasterizer.Height * 2];
    private readonly object presentGate = new();
    private readonly float[] mutedScratch = new float[735 * 2];
    private readonly Thread gameThread;
    private readonly WaveOutEvent output = new();
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly System.Windows.Forms.Timer refresh = new() { Interval = 16 };
    private readonly ComboBox stateBox = new();
    private readonly TextBox mapFilter = new();
    private readonly ListBox mapList = new();
    private readonly ComboBox arrivalBox = new();
    private readonly Button warpButton = new();
    private readonly ListBox scriptList = new();
    private readonly Button runButton = new();
    private readonly Button pauseButton = new();
    private readonly Button stepButton = new();
    private readonly Button freezeButton = new();
    private readonly Button frameButton = new();
    private readonly ComboBox speedBox = new();
    private readonly ListBox listing = new();
    private readonly Label status = new();
    private readonly Label listingTitle = new();
    private readonly System.Drawing.Font mono = new("Consolas", 9f);
    private readonly Dictionary<int, MapInfo> mapsByID = new();
    private readonly Dictionary<int, int> lineByOffset = new();
    private List<ListingLine> lines = new();
    private string shownPage = "";
    private int shownMap = -1;
    private int currentLine = NoLine;
    private long nextFrame;
    private int frameCount;
    private int keys;
    private volatile int speed = 1;
    private volatile bool closing;
    private volatile bool frozen;
    private volatile bool frameStep;
    private volatile string? failure;
    private bool failureShown;
    private int testTicks;
    private int testStepsDone;
    public static int TestScriptID = -1;
    public static int TestObject = -1;
    public static int TestSteps;
    public static string? TestShot;

    public static List<string> States()
    {
        if (!Directory.Exists(TestScript.StateDirectory))
        {
            return new List<string>();
        }
        return new DirectoryInfo(TestScript.StateDirectory).GetFiles("*.sav").Where(f => !f.Name.StartsWith("_")).OrderByDescending(f => f.LastWriteTimeUtc).Select(f => Path.GetFileNameWithoutExtension(f.Name)).ToList();
    }

    public PlayerForm(string romPath, ScriptData data, string? state, bool muted)
    {
        this.romPath = romPath;
        this.data = data;
        this.muted = muted;
        foreach (MapInfo map in data.Maps)
        {
            mapsByID[map.Id] = map;
        }
        Text = "Platinum Script Player";
        ClientSize = new Size(Rasterizer.Width * Zoom + PanelWidth, Rasterizer.Height * 2 * Zoom);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        DoubleBuffered = true;
        KeyPreview = true;
        BackColor = panelColor;
        ForeColor = inkColor;
        Font = new System.Drawing.Font("Segoe UI", 9f);
        BuildPanel(state);
        NitroRom rom = new(romPath);
        RomData.Open(rom);
        CardBackup.Open(TestScript.PrepareBackup(state));
        ScriptPlayback.Enabled = true;
        Interrupts.WaitHook = OnWait;
        if (!muted)
        {
            output.DesiredLatency = 200;
            output.NumberOfBuffers = 4;
            output.Init(new GameAudio());
            output.Play();
        }
        refresh.Tick += (_, _) => OnRefresh();
        refresh.Start();
        gameThread = new Thread(RunGame) { IsBackground = true, Name = "Game" };
        gameThread.Start();
    }

    private Label Caption(string text, int x, int y)
    {
        Label label = new() { Text = text, AutoSize = true, ForeColor = dimColor, Location = new Point(x, y) };
        Controls.Add(label);
        return label;
    }

    private void Style(Control control)
    {
        control.BackColor = fieldColor;
        control.ForeColor = inkColor;
        if (control is Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(90, 96, 110);
        }
        if (control is ListBox list)
        {
            list.BorderStyle = BorderStyle.FixedSingle;
            list.IntegralHeight = false;
        }
        if (control is ComboBox combo)
        {
            combo.FlatStyle = FlatStyle.Flat;
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
        }
        Controls.Add(control);
    }

    private void BuildPanel(string? state)
    {
        int left = Rasterizer.Width * Zoom + 12;
        int width = PanelWidth - 24;
        int half = (width - 10) / 2;
        Caption("Story state (checkpoint save)", left, 8);
        stateBox.SetBounds(left, 26, half, 24);
        Style(stateBox);
        stateBox.Items.Add("(none - boot from title)");
        foreach (string name in States())
        {
            stateBox.Items.Add(name);
        }
        stateBox.SelectedIndex = state == null ? 0 : Math.Max(0, stateBox.Items.IndexOf(state));
        stateBox.SelectionChangeCommitted += (_, _) => Restart();
        Caption("Speed", left + half + 10, 8);
        speedBox.SetBounds(left + half + 10, 26, 80, 24);
        Style(speedBox);
        speedBox.Items.AddRange(new object[] { "1x", "2x", "4x", "8x" });
        speedBox.SelectedIndex = 0;
        speedBox.SelectionChangeCommitted += (_, _) =>
        {
            speed = 1 << speedBox.SelectedIndex;
            ActiveControl = null;
        };
        Caption("Map", left, 58);
        mapFilter.SetBounds(left, 76, half, 24);
        mapFilter.BorderStyle = BorderStyle.FixedSingle;
        mapFilter.PlaceholderText = "filter by name or number";
        Style(mapFilter);
        mapFilter.TextChanged += (_, _) => FillMaps();
        mapList.SetBounds(left, 102, half, 150);
        Style(mapList);
        mapList.SelectedIndexChanged += (_, _) => OnMapSelected();
        mapList.DoubleClick += (_, _) => Warp();
        Caption("Arrive at", left, 256);
        arrivalBox.SetBounds(left, 274, half - 96, 24);
        Style(arrivalBox);
        warpButton.Text = "Go to map";
        warpButton.SetBounds(left + half - 90, 273, 90, 26);
        Style(warpButton);
        warpButton.Click += (_, _) => Warp();
        Caption("Scripts on the current map", left + half + 10, 58);
        scriptList.SetBounds(left + half + 10, 76, half, 196);
        Style(scriptList);
        scriptList.HorizontalScrollbar = true;
        scriptList.SelectedIndexChanged += (_, _) => OnScriptSelected();
        scriptList.DoubleClick += (_, _) => RunSelected();
        runButton.Text = "Play script";
        runButton.SetBounds(left + half + 10, 273, 110, 26);
        Style(runButton);
        runButton.Click += (_, _) => RunSelected();
        pauseButton.Text = "Pause script";
        pauseButton.SetBounds(left, 308, 110, 28);
        Style(pauseButton);
        pauseButton.Click += (_, _) =>
        {
            ScriptPlayback.Paused = !ScriptPlayback.Paused;
            ActiveControl = null;
        };
        stepButton.Text = "Step command";
        stepButton.SetBounds(left + 116, 308, 120, 28);
        Style(stepButton);
        stepButton.Click += (_, _) =>
        {
            ScriptPlayback.Paused = true;
            ScriptPlayback.StepCommand();
            ActiveControl = null;
        };
        freezeButton.Text = "Freeze game";
        freezeButton.SetBounds(left + 242, 308, 110, 28);
        Style(freezeButton);
        freezeButton.Click += (_, _) =>
        {
            frozen = !frozen;
            ActiveControl = null;
        };
        frameButton.Text = "Step frame";
        frameButton.SetBounds(left + 358, 308, 100, 28);
        Style(frameButton);
        frameButton.Click += (_, _) =>
        {
            frozen = true;
            frameStep = true;
            ActiveControl = null;
        };
        listingTitle.SetBounds(left, 344, width, 18);
        listingTitle.ForeColor = dimColor;
        Controls.Add(listingTitle);
        listing.SetBounds(left, 364, width, ClientSize.Height - 364 - 96);
        Style(listing);
        listing.DrawMode = DrawMode.OwnerDrawFixed;
        listing.Font = mono;
        listing.ItemHeight = 16;
        listing.HorizontalScrollbar = true;
        listing.DrawItem += DrawListingLine;
        status.SetBounds(left, ClientSize.Height - 90, width, 84);
        status.Font = mono;
        status.ForeColor = inkColor;
        Controls.Add(status);
        FillMaps();
    }

    private void Restart()
    {
        List<string> arguments = new() { "\"" + romPath + "\"" };
        if (stateBox.SelectedIndex > 0)
        {
            arguments.Add("--state");
            arguments.Add(stateBox.SelectedItem!.ToString()!);
        }
        else
        {
            arguments.Add("--nostate");
        }
        if (muted)
        {
            arguments.Add("--mute");
        }
        Process.Start(Application.ExecutablePath, string.Join(' ', arguments));
        Close();
    }

    private void FillMaps()
    {
        string filter = mapFilter.Text.Trim();
        mapList.BeginUpdate();
        mapList.Items.Clear();
        foreach (MapInfo map in data.Maps)
        {
            if (filter.Length == 0 || map.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) || map.Id.ToString() == filter)
            {
                mapList.Items.Add(map);
            }
        }
        mapList.EndUpdate();
    }

    private void OnMapSelected()
    {
        arrivalBox.Items.Clear();
        if (mapList.SelectedItem is not MapInfo map)
        {
            return;
        }
        for (int i = 0; i < map.Warps.Count; i++)
        {
            arrivalBox.Items.Add(new ArrivalChoice { Warp = i, X = map.Warps[i][0], Z = map.Warps[i][1], Label = $"warp {i} at {map.Warps[i][0]},{map.Warps[i][1]}" });
        }
        foreach (int[] position in map.Objects)
        {
            arrivalBox.Items.Add(new ArrivalChoice { X = position[0], Z = position[1] + 1, Label = $"below object at {position[0]},{position[1]}" });
        }
        if (arrivalBox.Items.Count == 0)
        {
            arrivalBox.Items.Add(new ArrivalChoice { X = 8, Z = 8, Label = "tile 8,8" });
        }
        arrivalBox.SelectedIndex = 0;
    }

    private void Warp()
    {
        if (mapList.SelectedItem is MapInfo map && arrivalBox.SelectedItem is ArrivalChoice arrival)
        {
            ScriptPlayback.RequestWarp(map.Id, arrival.Warp, arrival.X, arrival.Z);
        }
        ActiveControl = null;
    }

    private void FillScripts(MapInfo map)
    {
        scriptList.BeginUpdate();
        scriptList.Items.Clear();
        foreach (EventInfo info in map.Events)
        {
            scriptList.Items.Add(new ScriptChoice { Script = info.Script, Object = info.Object, Label = $"{info.Script,5}  {info.Label}" });
        }
        for (int i = 1; i <= map.Scripts; i++)
        {
            scriptList.Items.Add(new ScriptChoice { Script = i, Label = $"{i,5}  script {i}" });
        }
        scriptList.EndUpdate();
    }

    private void OnScriptSelected()
    {
        if (scriptList.SelectedItem is not ScriptChoice choice || ScriptPlayback.ScriptActive)
        {
            return;
        }
        string label = $"script_{choice.Script}:";
        int index = lines.FindIndex(l => l.Kind == KindLabel && l.Text == label);
        if (index >= 0)
        {
            listing.TopIndex = index;
        }
    }

    private void RunSelected()
    {
        if (scriptList.SelectedItem is ScriptChoice choice)
        {
            ScriptPlayback.RequestScript(choice.Script, choice.Object);
        }
        ActiveControl = null;
    }

    private void ShowPage(string page, string title)
    {
        if (page == shownPage)
        {
            return;
        }
        shownPage = page;
        lines = new List<ListingLine>();
        lineByOffset.Clear();
        if (data.Pages.TryGetValue(page, out PageInfo? info))
        {
            foreach (JsonElement[] line in info.Lines)
            {
                ListingLine entry = new() { Offset = line[0].GetInt32(), Text = line[1].GetString() ?? "", Kind = line[2].GetInt32() };
                if (entry.Offset >= 0)
                {
                    lineByOffset[entry.Offset] = lines.Count;
                }
                lines.Add(entry);
            }
        }
        listingTitle.Text = title;
        currentLine = NoLine;
        listing.BeginUpdate();
        listing.Items.Clear();
        foreach (ListingLine line in lines)
        {
            listing.Items.Add(line);
        }
        listing.EndUpdate();
    }

    private string PageForFile(int file, out string title)
    {
        if (mapsByID.TryGetValue(ScriptPlayback.Map, out MapInfo? current) && current.File == file)
        {
            title = $"Script file {file} - {current.Name}";
            return current.Page;
        }
        CommonInfo? common = data.Common.FirstOrDefault(c => c.File == file);
        if (common != null)
        {
            title = $"Script file {file} - common scripts from ID {common.Base}";
            return common.Page;
        }
        MapInfo? other = data.Maps.FirstOrDefault(m => m.File == file);
        title = $"Script file {file}" + (other != null ? " - " + other.Name : "");
        return other?.Page ?? "";
    }

    private void DrawListingLine(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= lines.Count)
        {
            return;
        }
        ListingLine line = lines[e.Index];
        bool current = e.Index == currentLine;
        Color back = current ? (ScriptPlayback.BlockedCommand != ScriptPlayback.NoCommand ? blockedColor : currentColor) : fieldColor;
        using SolidBrush backBrush = new(back);
        e.Graphics.FillRectangle(backBrush, e.Bounds);
        Color ink = line.Kind == KindLabel ? labelColor : line.Kind == KindDialogue ? dialogueColor : line.Kind == KindCommand ? inkColor : dimColor;
        string text = (current ? "> " : "  ") + (line.Offset >= 0 ? line.Offset.ToString("X4") : "    ") + " " + line.Text;
        TextRenderer.DrawText(e.Graphics, text, mono, e.Bounds.Location, current ? Color.White : ink, TextFormatFlags.NoPrefix);
    }

    private void RunGame()
    {
        try
        {
            PlatinumSharp.Game.Main.NitroMain();
        }
        catch (ThreadInterruptedException)
        {
        }
        catch (Exception ex)
        {
            if (!closing)
            {
                failure = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }
    }

    private void OnWait(uint mask)
    {
        if (closing)
        {
            throw new ThreadInterruptedException();
        }
        if ((mask & Interrupts.VBlank) == 0)
        {
            return;
        }
        RenderFrame();
        frameCount++;
        ScriptPlayback.Update();
        while (frozen && !frameStep && !closing)
        {
            Thread.Sleep(10);
        }
        frameStep = false;
        Input.Keys = keys;
        long now = clock.ElapsedTicks * 1000000 / Stopwatch.Frequency;
        if (nextFrame < now - FrameMicroseconds * 4 || nextFrame > now + FrameMicroseconds * 4)
        {
            nextFrame = now;
        }
        nextFrame += FrameMicroseconds / speed;
        long wait = nextFrame - now;
        if (muted)
        {
            Audio.Render(mutedScratch, 0, mutedScratch.Length / 2);
        }
        if (wait > 1000)
        {
            Thread.Sleep((int)(wait / 1000));
        }
    }

    private void RenderFrame()
    {
        Rasterizer.LayerMask = 0x1F;
        Rasterizer.Render(0, mainPixels);
        Rasterizer.ApplyMasterBrightness(0, mainPixels);
        Rasterizer.Render(1, subPixels);
        Rasterizer.ApplyMasterBrightness(1, subPixels);
        bool mainOnTop = (Video.PowerControl & PowerMainOnTop) != 0;
        lock (presentGate)
        {
            Array.Copy(mainOnTop ? mainPixels : subPixels, 0, presented, 0, mainPixels.Length);
            Array.Copy(mainOnTop ? subPixels : mainPixels, 0, presented, mainPixels.Length, mainPixels.Length);
        }
    }

    private void OnRefresh()
    {
        if (failure != null && TestShot != null)
        {
            File.WriteAllText(TestShot + ".txt", failure);
            Close();
            return;
        }
        if (failure != null)
        {
            if (!failureShown)
            {
                failureShown = true;
                Text = "Platinum Script Player - game stopped";
                status.Text = "GAME STOPPED: " + failure.Split('\n')[0].Trim() + Environment.NewLine + "Pick a state above to restart.";
                status.ForeColor = labelColor;
            }
            return;
        }
        BitmapData bits = frame.LockBits(new Rectangle(0, 0, frame.Width, frame.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        lock (presentGate)
        {
            Marshal.Copy(presented, 0, bits.Scan0, presented.Length);
        }
        frame.UnlockBits(bits);
        Invalidate(new Rectangle(0, 0, Rasterizer.Width * Zoom, Rasterizer.Height * 2 * Zoom));
        UpdatePanel();
        RunTest();
    }

    private void RunTest()
    {
        if (TestShot == null)
        {
            return;
        }
        if (testTicks == 0)
        {
            if (!ScriptPlayback.FieldIdle)
            {
                return;
            }
            if (TestScriptID < 0)
            {
                ScriptPlayback.RequestWarp(-TestScriptID, TestObject, 0, 0);
            }
            else
            {
                ScriptPlayback.Paused = true;
                ScriptPlayback.RequestScript(TestScriptID, TestObject);
            }
        }
        testTicks++;
        if (testTicks % 30 == 0 && testStepsDone < TestSteps)
        {
            testStepsDone++;
            ScriptPlayback.StepCommand();
        }
        if (testTicks == 30 * (TestSteps + 4))
        {
            using System.Drawing.Bitmap window = new(ClientSize.Width, ClientSize.Height);
            DrawToBitmap(window, new Rectangle(Point.Empty, ClientSize));
            window.Save(TestShot, ImageFormat.Png);
            Close();
        }
    }

    private void UpdatePanel()
    {
        bool inField = ScriptPlayback.InField;
        int map = ScriptPlayback.Map;
        if (inField && map != shownMap && mapsByID.TryGetValue(map, out MapInfo? info))
        {
            shownMap = map;
            FillScripts(info);
        }
        int file = ScriptPlayback.CurrentFile;
        bool active = ScriptPlayback.ScriptActive;
        if (active && file != ScriptPlayback.NoFile)
        {
            ShowPage(PageForFile(file, out string title), title);
        }
        else if (inField && !active && mapsByID.TryGetValue(map, out MapInfo? current))
        {
            ShowPage(current.Page, $"Script file {current.File} - {current.Name}");
        }
        int line = NoLine;
        if (active && file != ScriptPlayback.NoFile && lineByOffset.TryGetValue(ScriptPlayback.CurrentOffset, out int found))
        {
            line = found;
        }
        if (line != currentLine)
        {
            int previous = currentLine;
            currentLine = line;
            if (previous >= 0 && previous < listing.Items.Count)
            {
                listing.Invalidate(listing.GetItemRectangle(previous));
            }
            if (line >= 0)
            {
                int visible = listing.ClientSize.Height / listing.ItemHeight;
                if (line < listing.TopIndex || line >= listing.TopIndex + visible - 2)
                {
                    listing.TopIndex = Math.Max(0, line - visible / 3);
                }
                listing.Invalidate(listing.GetItemRectangle(line));
            }
        }
        bool paused = ScriptPlayback.Paused;
        pauseButton.Text = paused ? "Resume script" : "Pause script";
        pauseButton.BackColor = paused ? currentColor : fieldColor;
        freezeButton.Text = frozen ? "Unfreeze game" : "Freeze game";
        freezeButton.BackColor = frozen ? currentColor : fieldColor;
        bool idle = ScriptPlayback.FieldIdle;
        runButton.Enabled = idle;
        warpButton.Enabled = idle;
        string where = inField ? $"map {map} {(mapsByID.TryGetValue(map, out MapInfo? m) ? m.Name : "")} at {ScriptPlayback.PlayerX},{ScriptPlayback.PlayerZ}" : "not in the field yet";
        string what;
        int blocked = ScriptPlayback.BlockedCommand;
        if (blocked != ScriptPlayback.NoCommand)
        {
            what = $"STOPPED: command {(blocked < data.Commands.Count ? data.Commands[blocked] : blocked.ToString())} is not ported yet";
        }
        else if (active)
        {
            what = $"script {ScriptPlayback.RunningScript} {(paused ? "paused" : "running")} - {ScriptPlayback.CommandsRun} commands run";
        }
        else
        {
            what = idle ? "idle - pick a script and press Play script" : "busy";
        }
        string said = ScriptPlayback.Dialogue.Replace(" / ", " ");
        if (said.Length > 150)
        {
            said = said.Substring(0, 150) + "...";
        }
        string text = where + Environment.NewLine + what + Environment.NewLine + (said.Length > 0 ? "says: " + said : "") + Environment.NewLine + "keys: X=A Z=B S=X A=Y Enter=Start arrows=move";
        if (status.Text != text)
        {
            status.Text = text;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        e.Graphics.DrawImage(frame, new Rectangle(0, 0, Rasterizer.Width * Zoom, Rasterizer.Height * 2 * Zoom));
    }

    private static int MapKey(Keys key)
    {
        foreach ((Keys Key, int Mask, string Name) binding in bindings)
        {
            if (binding.Key == key)
            {
                return binding.Mask;
            }
        }
        return 0;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (ActiveControl == mapFilter)
        {
            if (keyData == Keys.Escape || keyData == Keys.Enter)
            {
                ActiveControl = null;
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        int mask = MapKey(keyData & Keys.KeyCode);
        if (mask != 0)
        {
            keys |= mask;
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        keys &= ~MapKey(e.KeyCode);
        base.OnKeyUp(e);
    }

    private void UpdateTouch(MouseEventArgs e, bool down)
    {
        int x = e.X / Zoom;
        int y = e.Y / Zoom - Rasterizer.Height;
        if (down && x >= 0 && x < Rasterizer.Width && y >= 0 && y < Rasterizer.Height)
        {
            Input.TouchX = (ushort)x;
            Input.TouchY = (ushort)y;
            Input.Touching = true;
        }
        else
        {
            Input.Touching = false;
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        ActiveControl = null;
        UpdateTouch(e, true);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            UpdateTouch(e, true);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        UpdateTouch(e, false);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        closing = true;
        refresh.Stop();
        if (!muted)
        {
            output.Stop();
        }
        output.Dispose();
        base.OnFormClosing(e);
    }
}
