using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using NAudio.Wave;
using PlatinumSharp.Game;
using PlatinumSharp.Nitro;

namespace PlatinumSharp.Runner;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        string? romPath = args.FirstOrDefault(a => a.EndsWith(".nds", StringComparison.OrdinalIgnoreCase));
        if (romPath == null)
        {
            using OpenFileDialog dialog = new() { Filter = "Nintendo DS ROM (*.nds)|*.nds" };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return 1;
            }
            romPath = dialog.FileName;
        }
        string? screenshot = args.SkipWhile(a => a != "--screenshot").Skip(1).FirstOrDefault();
        int frames = int.TryParse(args.SkipWhile(a => a != "--frames").Skip(1).FirstOrDefault(), out int parsed) ? parsed : 0;
        GameAudio.CapturePath = args.SkipWhile(a => a != "--wav").Skip(1).FirstOrDefault();
        foreach (string press in args.SkipWhile(a => a != "--press").Skip(1).TakeWhile(a => !a.StartsWith("--")))
        {
            string[] parts = press.Split(':');
            GameForm.ScriptedKeys.Add((int.Parse(parts[0]), int.Parse(parts[1])));
        }
        foreach (string dump in args.SkipWhile(a => a != "--dump").Skip(1).TakeWhile(a => !a.StartsWith("--")))
        {
            GameForm.ScriptedDumps.Add(int.Parse(dump));
        }
        foreach (string redirect in args.Where(a => a.StartsWith("redirect=")))
        {
            string[] pair = redirect.Substring(9).Split('>');
            ApplicationTemplate.Redirects[Convert.ToUInt32(pair[0], 16)] = Convert.ToUInt32(pair[1], 16);
        }
        GameForm.Muted = args.Contains("--mute");
        GameForm.ResultPath = args.SkipWhile(a => a != "--result").Skip(1).FirstOrDefault();
        string? scriptPath = args.SkipWhile(a => a != "--script").Skip(1).FirstOrDefault();
        if (scriptPath != null)
        {
            GameForm.Script = new TestScript(scriptPath);
            string? stateName = args.SkipWhile(a => a != "--state").Skip(1).FirstOrDefault();
            GameForm.BackupPath = TestScript.PrepareBackup(stateName);
            if (stateName != null)
            {
                GameForm.Script.SkipToState(stateName);
            }
        }
        if (int.TryParse(args.SkipWhile(a => a != "--speed").Skip(1).FirstOrDefault(), out int speed) && speed > 0)
        {
            GameForm.Speed = speed;
        }
        using GameForm form = new(romPath, screenshot, frames);
        Application.Run(form);
        return 0;
    }
}

internal sealed class GameAudio : ISampleProvider
{
    public static string? CapturePath;
    public static string? Failure;
    private static readonly List<float> captured = new();

    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    public int Read(float[] buffer, int offset, int count)
    {
        try
        {
            Audio.Render(buffer, offset, count / 2);
        }
        catch (Exception ex)
        {
            Failure ??= ex.ToString();
            for (int i = 0; i < count; i++)
            {
                buffer[offset + i] = 0;
            }
        }
        if (CapturePath != null)
        {
            lock (captured)
            {
                for (int i = 0; i < count; i++)
                {
                    captured.Add(buffer[offset + i]);
                }
            }
        }
        return count;
    }

    public static void SaveCapture()
    {
        if (CapturePath == null)
        {
            return;
        }
        lock (captured)
        {
            PlatinumSharp.Nitro.Sound.SequencePlayer.SaveWav(CapturePath, captured.ToArray(), 44100);
            File.WriteAllText(CapturePath + ".txt", Audio.Describe() + Failure);
        }
    }
}

internal sealed class PanelButton : Button
{
    public static readonly Color NormalColor = Color.FromArgb(58, 58, 68);
    public static readonly Color PressedColor = Color.FromArgb(40, 120, 70);
    public static readonly Color RebindColor = Color.FromArgb(160, 110, 30);

    public PanelButton()
    {
        SetStyle(ControlStyles.Selectable, false);
        TabStop = false;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderColor = Color.FromArgb(110, 110, 124);
        FlatAppearance.MouseOverBackColor = Color.FromArgb(84, 84, 98);
        FlatAppearance.MouseDownBackColor = Color.FromArgb(100, 100, 118);
        BackColor = NormalColor;
        ForeColor = Color.White;
        UseVisualStyleBackColor = false;
    }
}

internal sealed class GameForm : Form
{
    private const int Zoom = 2;
    private const int PanelWidth = 460;
    private const int FrameMicroseconds = 16715;
    private const ushort PowerMainOnTop = 0x8000;
    private const int ScriptedHoldFrames = 4;
    private const string DumpRoot = @"E:\Poketest\dumps";
    private const int KeyLayoutTop = 46;
    private const int KeyLayoutHeight = 184;
    private const int NoRebind = -1;
    private const string ShotDirectory = @"E:\Poketest\tools\tests\shots";

    private static readonly string[] buttonNames = { "L", "R", "Up", "Left", "Right", "Down", "X", "Y", "A", "B", "Select", "Start" };
    private static readonly int[] buttonMasks = { Input.KeyL, Input.KeyR, Input.KeyUp, Input.KeyLeft, Input.KeyRight, Input.KeyDown, Input.KeyX, Input.KeyY, Input.KeyA, Input.KeyB, Input.KeySelect, Input.KeyStart };
    private static readonly Rectangle[] buttonBounds =
    {
        new(0, 0, 96, 36), new(344, 0, 96, 36),
        new(62, 44, 60, 40), new(0, 88, 60, 40), new(124, 88, 60, 40), new(62, 132, 60, 40),
        new(318, 44, 60, 40), new(256, 88, 60, 40), new(380, 88, 60, 40), new(318, 132, 60, 40),
        new(140, 144, 72, 36), new(216, 144, 72, 36),
    };
    private static readonly Keys[] defaultBindings = { Keys.Q, Keys.W, Keys.Up, Keys.Left, Keys.Right, Keys.Down, Keys.S, Keys.A, Keys.X, Keys.Z, Keys.Back, Keys.Enter };

    public static readonly List<(int Frame, int Keys)> ScriptedKeys = new();
    public static readonly List<int> ScriptedDumps = new();

    private static readonly string[] layerNames = { "bg0", "bg1", "bg2", "bg3", "obj" };

    private readonly System.Drawing.Bitmap frame = new(Rasterizer.Width, Rasterizer.Height * 2, PixelFormat.Format32bppArgb);
    private readonly int[] mainPixels = new int[Rasterizer.Width * Rasterizer.Height];
    private readonly int[] subPixels = new int[Rasterizer.Width * Rasterizer.Height];
    private readonly int[] presented = new int[Rasterizer.Width * Rasterizer.Height * 2];
    private readonly object presentGate = new();
    private readonly Thread gameThread;
    private readonly WaveOutEvent output = new();
    private readonly float[] mutedScratch = new float[735 * 2];
    public static bool Muted;
    public static int Speed = 1;
    public static TestScript? Script;
    public static string? ResultPath;
    public static string? BackupPath;
    private volatile string scriptStatus = "";
    private volatile string? shotRequest;
    private volatile int scriptedKeys;
    private int scriptEndRefreshes;
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly System.Windows.Forms.Timer refresh = new() { Interval = 8 };
    private readonly Label info = new();
    private readonly Panel infoPanel = new();
    private readonly PanelButton[] keyButtons = new PanelButton[buttonNames.Length];
    private readonly Keys[] bindings = (Keys[])defaultBindings.Clone();
    private readonly string bindingsPath = Path.Combine(AppContext.BaseDirectory, "keys.cfg");
    private int rebindIndex = NoRebind;
    private readonly PanelButton dumpButton = new();
    private readonly PanelButton pauseButton = new();
    private readonly PanelButton stepButton = new();
    private readonly string? screenshotPath;
    private readonly int frameLimit;
    private readonly NitroRom rom;
    private long nextFrame;
    private int frameCount;
    private int keys;
    private int refreshCount;
    private volatile bool closing;
    private volatile bool dumpRequested;
    private volatile bool paused;
    private volatile bool stepRequested;
    private volatile string? failure;
    private volatile string status = "";
    private volatile string lastDump = "";
    private bool failureShown;

    public GameForm(string romPath, string? screenshot, int frames)
    {
        screenshotPath = screenshot;
        frameLimit = frames;
        Text = "PlatinumSharp";
        ClientSize = new Size(Rasterizer.Width * Zoom + PanelWidth, Rasterizer.Height * 2 * Zoom);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        DoubleBuffered = true;
        KeyPreview = true;
        BackColor = Color.FromArgb(24, 24, 28);
        int panelLeft = Rasterizer.Width * Zoom + 10;
        dumpButton.Text = "Dump this moment";
        dumpButton.SetBounds(panelLeft, 8, 170, 30);
        dumpButton.Click += (_, _) => dumpRequested = true;
        pauseButton.Text = "Pause";
        pauseButton.SetBounds(panelLeft + 178, 8, 90, 30);
        pauseButton.Click += (_, _) =>
        {
            paused = !paused;
            pauseButton.Text = paused ? "Resume" : "Pause";
        };
        stepButton.Text = "Step frame";
        stepButton.SetBounds(panelLeft + 276, 8, 100, 30);
        stepButton.Click += (_, _) => stepRequested = true;
        LoadBindings();
        System.Drawing.Font keyFont = new("Segoe UI", 8f);
        for (int i = 0; i < keyButtons.Length; i++)
        {
            int index = i;
            PanelButton button = new();
            Rectangle bounds = buttonBounds[i];
            button.SetBounds(panelLeft + bounds.X, KeyLayoutTop + bounds.Y, bounds.Width, bounds.Height);
            button.Font = keyFont;
            button.Click += (_, _) => BeginRebind(index);
            keyButtons[i] = button;
            Controls.Add(button);
        }
        UpdateKeyButtons();
        int infoTop = KeyLayoutTop + KeyLayoutHeight + 8;
        infoPanel.SetBounds(panelLeft, infoTop, PanelWidth - 14, ClientSize.Height - infoTop - 6);
        infoPanel.AutoScroll = true;
        infoPanel.BackColor = BackColor;
        info.Location = new Point(0, 0);
        info.AutoSize = true;
        info.MaximumSize = new Size(PanelWidth - 40, 0);
        info.Font = new System.Drawing.Font("Consolas", 8.5f);
        info.ForeColor = Color.Gainsboro;
        info.BackColor = BackColor;
        infoPanel.Controls.Add(info);
        Controls.Add(dumpButton);
        Controls.Add(pauseButton);
        Controls.Add(stepButton);
        Controls.Add(infoPanel);
        rom = new NitroRom(romPath);
        RomData.Open(rom);
        CardBackup.Open(BackupPath ?? Path.ChangeExtension(romPath, ".sav"));
        Interrupts.WaitHook = OnWait;
        if (!Muted)
        {
            output.DesiredLatency = 200;
            output.NumberOfBuffers = 4;
            output.Init(new GameAudio());
            output.Play();
        }
        refresh.Tick += (_, _) => OnRefresh();
        refresh.Start();
        TestScript.Screenshot = name => shotRequest = name;
        gameThread = new Thread(RunGame) { IsBackground = true, Name = "Game" };
        gameThread.Start();
    }

    private void RunGame()
    {
        try
        {
            Main.NitroMain();
        }
        catch (ThreadInterruptedException)
        {
        }
        catch (Exception ex)
        {
            if (!closing)
            {
                failure = ex.Message + Environment.NewLine + ex.StackTrace;
                try
                {
                    WriteDump("failure");
                }
                catch (Exception)
                {
                }
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
        if (frameCount == 10 && ApplicationTemplate.Redirects.Count > 0)
        {
            TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(SaveData.SaveData_Ptr());
            if (TrainerInfo.TrainerInfo_HasNoName(trainerInfo))
            {
                TrainerInfo.TrainerInfo_SetName(trainerInfo, new ushort[] { 0x012B, 0x012C, 0x012D, 0xFFFF });
            }
        }
        UpdateStatus();
        if (dumpRequested || ScriptedDumps.Contains(frameCount))
        {
            dumpRequested = false;
            WriteDump("manual");
        }
        while (paused && !stepRequested && !closing)
        {
            if (dumpRequested)
            {
                dumpRequested = false;
                WriteDump("manual");
            }
            Thread.Sleep(10);
        }
        stepRequested = false;
        int scripted = 0;
        foreach ((int Frame, int Keys) entry in ScriptedKeys)
        {
            if (frameCount >= entry.Frame && frameCount < entry.Frame + ScriptedHoldFrames)
            {
                scripted |= entry.Keys;
            }
        }
        if (Script != null)
        {
            scripted |= Script.Update();
            scriptStatus = Script.Status;
        }
        scriptedKeys = scripted;
        Input.Keys = keys | scripted;
        long now = clock.ElapsedTicks * 1000000 / Stopwatch.Frequency;
        if (nextFrame < now - FrameMicroseconds * 4 || nextFrame > now + FrameMicroseconds * 4)
        {
            nextFrame = now;
        }
        nextFrame += FrameMicroseconds / Speed;
        long wait = nextFrame - now;
        if (Muted)
        {
            Audio.Render(mutedScratch, 0, mutedScratch.Length / 2);
        }
        if (wait > 1000 && (frameLimit == 0 || GameAudio.CapturePath != null || Muted))
        {
            Thread.Sleep((int)(wait / 1000));
        }
    }

    private void UpdateStatus()
    {
        if (frameCount % 6 != 0 && !paused)
        {
            return;
        }
        StringBuilder text = new();
        if (scriptStatus.Length > 0)
        {
            text.AppendLine(scriptStatus);
            text.AppendLine();
        }
        text.AppendLine($"frame {frameCount}   keys {Input.Keys:X3}" + (Speed > 1 ? $"   speed x{Speed}" : ""));
        text.Append(DebugInfo.DescribeApplication());
        text.AppendLine();
        text.Append(DebugInfo.DescribeVideo());
        text.AppendLine();
        text.Append(DebugInfo.DescribeData());
        status = text.ToString();
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

    private static void SavePixels(int[] pixels, int width, int height, string path)
    {
        using System.Drawing.Bitmap bitmap = new(width, height, PixelFormat.Format32bppArgb);
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        Marshal.Copy(pixels, 0, data.Scan0, width * height);
        bitmap.UnlockBits(data);
        bitmap.Save(path, ImageFormat.Png);
    }

    private void WriteDump(string reason)
    {
        string folder = Path.Combine(DumpRoot, $"frame{frameCount:D6}_{DateTime.Now:HHmmss}_{reason}");
        Directory.CreateDirectory(folder);
        int[] both;
        lock (presentGate)
        {
            both = (int[])presented.Clone();
        }
        SavePixels(both, Rasterizer.Width, Rasterizer.Height * 2, Path.Combine(folder, "screen.png"));
        int[] layer = new int[Rasterizer.Width * Rasterizer.Height];
        for (int display = 0; display < 2; display++)
        {
            uint control = Video.GetDisplayControl(display);
            for (int i = 0; i < layerNames.Length; i++)
            {
                uint bit = i < 4 ? 0x100u << i : 0x1000u;
                if ((control & bit) == 0)
                {
                    continue;
                }
                Rasterizer.LayerMask = 1 << i;
                Rasterizer.Render(display, layer);
                SavePixels(layer, Rasterizer.Width, Rasterizer.Height, Path.Combine(folder, $"{(display == 0 ? "main" : "sub")}_{layerNames[i]}.png"));
            }
        }
        Rasterizer.LayerMask = 0x1F;
        int[] scene = new int[Rasterizer.Width * Rasterizer.Height];
        for (int i = 0; i < scene.Length; i++)
        {
            int value = Render3D.Frame[i];
            scene[i] = ((value >> 15) & 0x1F) == 0 ? unchecked((int)0xFFFF00FF) : Rasterizer.ToArgb((ushort)(value & 0x7FFF));
        }
        SavePixels(scene, Rasterizer.Width, Rasterizer.Height, Path.Combine(folder, "3d.png"));
        StringBuilder text = new();
        text.AppendLine($"frame {frameCount} reason {reason} keys {Input.Keys:X3}");
        text.Append(DebugInfo.DescribeApplication());
        text.AppendLine();
        text.Append(DebugInfo.DescribeVideoFull());
        text.AppendLine();
        text.Append(Audio.Describe());
        text.AppendLine();
        text.Append(DebugInfo.DescribeData());
        if (failure != null)
        {
            text.AppendLine();
            text.AppendLine(failure);
        }
        File.WriteAllText(Path.Combine(folder, "info.txt"), text.ToString());
        lastDump = folder;
    }

    private void WriteResult()
    {
        if (ResultPath == null)
        {
            return;
        }
        StringBuilder text = new();
        text.AppendLine(failure != null ? "STOPPED at frame " + frameCount : "finished at frame " + frameCount);
        text.AppendLine(scriptStatus);
        if (failure != null)
        {
            text.AppendLine(failure);
            text.AppendLine("dump: " + lastDump);
        }
        else
        {
            text.Append(DebugInfo.DescribeApplication());
        }
        File.WriteAllText(ResultPath, text.ToString());
        if (failure == null)
        {
            using System.Drawing.Bitmap window = new(ClientSize.Width, ClientSize.Height);
            DrawToBitmap(window, new Rectangle(Point.Empty, ClientSize));
            window.Save(Path.ChangeExtension(ResultPath, ".png"), ImageFormat.Png);
        }
    }

    private void OnRefresh()
    {
        refreshCount++;
        if (failure != null && !failureShown)
        {
            failureShown = true;
            Text = "PlatinumSharp - stopped";
            info.Text = "STOPPED at frame " + frameCount + Environment.NewLine + failure + Environment.NewLine + Environment.NewLine + "dump: " + lastDump;
            if (frameLimit > 0)
            {
                File.WriteAllText((screenshotPath ?? Path.Combine(DumpRoot, "failure")) + ".txt", failure);
                Close();
            }
            return;
        }
        if (failure != null && Script != null)
        {
            scriptEndRefreshes++;
            if (scriptEndRefreshes > 400)
            {
                WriteResult();
                Close();
            }
            return;
        }
        BitmapData data = frame.LockBits(new Rectangle(0, 0, frame.Width, frame.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        lock (presentGate)
        {
            Marshal.Copy(presented, 0, data.Scan0, presented.Length);
        }
        frame.UnlockBits(data);
        Invalidate(new Rectangle(0, 0, Rasterizer.Width * Zoom, Rasterizer.Height * 2 * Zoom));
        if (refreshCount % 4 == 0)
        {
            UpdateKeyHighlights();
        }
        if (shotRequest != null)
        {
            string shotName = shotRequest;
            shotRequest = null;
            Directory.CreateDirectory(ShotDirectory);
            frame.Save(Path.Combine(ShotDirectory, shotName + ".png"), ImageFormat.Png);
        }
        if (failure == null && refreshCount % 8 == 0)
        {
            string text = status + (lastDump.Length > 0 ? Environment.NewLine + Environment.NewLine + "last dump: " + lastDump : "");
            if (info.Text != text)
            {
                info.Text = text;
            }
        }
        if (Script != null && Script.Finished && failure == null)
        {
            scriptEndRefreshes++;
            if (scriptEndRefreshes > 250)
            {
                WriteResult();
                Close();
                return;
            }
        }
        if (frameLimit > 0 && frameCount >= frameLimit)
        {
            if (screenshotPath != null)
            {
                frame.Save(screenshotPath, ImageFormat.Png);
                using System.Drawing.Bitmap window = new(ClientSize.Width, ClientSize.Height);
                DrawToBitmap(window, new Rectangle(Point.Empty, ClientSize));
                window.Save(Path.ChangeExtension(screenshotPath, ".window.png"), ImageFormat.Png);
            }
            GameAudio.SaveCapture();
            Close();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        e.Graphics.DrawImage(frame, new Rectangle(0, 0, Rasterizer.Width * Zoom, Rasterizer.Height * 2 * Zoom));
    }

    private void LoadBindings()
    {
        if (!File.Exists(bindingsPath))
        {
            return;
        }
        string[] lines = File.ReadAllLines(bindingsPath);
        for (int i = 0; i < bindings.Length && i < lines.Length; i++)
        {
            if (Enum.TryParse(lines[i], out Keys key))
            {
                bindings[i] = key;
            }
        }
    }

    private void SaveBindings()
    {
        try
        {
            File.WriteAllLines(bindingsPath, bindings.Select(b => b.ToString()));
        }
        catch (IOException)
        {
        }
    }

    private static string KeyName(Keys key)
    {
        switch (key)
        {
            case Keys.None:
                return "-";
            case Keys.Up:
                return "\u2191";
            case Keys.Down:
                return "\u2193";
            case Keys.Left:
                return "\u2190";
            case Keys.Right:
                return "\u2192";
            case Keys.Enter:
                return "Enter";
            case Keys.Back:
                return "Bksp";
        }
        return key.ToString();
    }

    private void UpdateKeyButtons()
    {
        for (int i = 0; i < keyButtons.Length; i++)
        {
            keyButtons[i].Text = i == rebindIndex ? buttonNames[i] + "\npress key" : buttonNames[i] + "\n[" + KeyName(bindings[i]) + "]";
        }
    }

    private void UpdateKeyHighlights()
    {
        int held = keys | scriptedKeys;
        for (int i = 0; i < keyButtons.Length; i++)
        {
            Color color = i == rebindIndex ? PanelButton.RebindColor : (held & buttonMasks[i]) != 0 ? PanelButton.PressedColor : PanelButton.NormalColor;
            if (keyButtons[i].BackColor != color)
            {
                keyButtons[i].BackColor = color;
            }
        }
    }

    private void BeginRebind(int index)
    {
        rebindIndex = rebindIndex == index ? NoRebind : index;
        keys = 0;
        UpdateKeyButtons();
        UpdateKeyHighlights();
    }

    private void FinishRebind(Keys key)
    {
        if (key != Keys.Escape)
        {
            for (int i = 0; i < bindings.Length; i++)
            {
                if (bindings[i] == key)
                {
                    bindings[i] = bindings[rebindIndex];
                }
            }
            bindings[rebindIndex] = key;
            SaveBindings();
        }
        rebindIndex = NoRebind;
        keys = 0;
        UpdateKeyButtons();
        UpdateKeyHighlights();
    }

    private int MapKey(Keys key)
    {
        if (key == Keys.None)
        {
            return 0;
        }
        for (int i = 0; i < bindings.Length; i++)
        {
            if (bindings[i] == key)
            {
                return buttonMasks[i];
            }
        }
        return 0;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        Keys code = keyData & Keys.KeyCode;
        if (rebindIndex != NoRebind)
        {
            if (code != Keys.ShiftKey && code != Keys.ControlKey && code != Keys.Menu)
            {
                FinishRebind(code);
            }
            return true;
        }
        if (MapKey(code) != 0)
        {
            keys |= MapKey(code);
            return true;
        }
        if (code == Keys.F5)
        {
            dumpRequested = true;
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (rebindIndex == NoRebind)
        {
            keys |= MapKey(e.KeyCode);
        }
        e.Handled = true;
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        keys &= ~MapKey(e.KeyCode);
        e.Handled = true;
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        Point position = infoPanel.AutoScrollPosition;
        infoPanel.AutoScrollPosition = new Point(0, Math.Max(0, -position.Y - e.Delta));
        base.OnMouseWheel(e);
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
        if (!Muted)
        {
            output.Stop();
        }
        output.Dispose();
        base.OnFormClosing(e);
    }
}
