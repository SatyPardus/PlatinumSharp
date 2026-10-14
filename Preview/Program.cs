using Bitmap = System.Drawing.Bitmap;
using Font = PlatinumSharp.Game.Font;
using Graphics = System.Drawing.Graphics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using NAudio.Wave;
using PlatinumSharp.Game;
using PlatinumSharp.Nitro;
using PlatinumSharp.Nitro.Sound;

namespace PlatinumSharp.Preview;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        bool selfTest = args.Contains("--selftest");
        string? romPath = args.FirstOrDefault(a => a.EndsWith(".nds", StringComparison.OrdinalIgnoreCase));
        if (selfTest)
        {
            return SelfTest(romPath!, args.SkipWhile(a => a != "--selftest").Skip(1).FirstOrDefault() ?? "preview.png");
        }
        ApplicationConfiguration.Initialize();
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
        if (screenshot != null)
        {
            using PreviewForm capture = new(romPath);
            capture.SaveScreenshot(screenshot);
            return 0;
        }
        Application.Run(new PreviewForm(romPath));
        return 0;
    }

    private static int SelfTest(string romPath, string output)
    {
        using NitroRom rom = new(romPath);
        RomData.Open(rom);
        SpriteSource sprites = new(rom);
        using Bitmap bitmap = sprites.Render(1, 0);
        bitmap.Save(output, ImageFormat.Png);
        int opaque = 0;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).A != 0)
                {
                    opaque++;
                }
            }
        }
        Console.WriteLine($"sprite: {bitmap.Width}x{bitmap.Height}, {opaque} opaque pixels, saved {output}");
        MusicSource music = new(rom);
        music.Select(music.DefaultIndex);
        float[] buffer = new float[44100 * 2 * 2];
        int read = music.Read(buffer, 0, buffer.Length);
        double energy = 0;
        for (int i = 0; i < read; i++)
        {
            energy += buffer[i] * buffer[i];
        }
        Console.WriteLine($"music: \"{music.Names[music.DefaultIndex]}\", {read / 2} frames, rms {Math.Sqrt(energy / Math.Max(1, read)):F4}");
        return opaque > 0 && energy > 0 ? 0 : 2;
    }
}

internal sealed class SpriteSource
{
    public const int FrameSize = 80;
    private const int RowBytes = 0x50;
    private const int MembersPerSpecies = 6;
    private const int FrontFemale = 2;
    private const int FrontMale = 3;
    private const int NormalPalette = 4;

    private readonly Narc archive;

    public SpriteSource(NitroRom rom)
    {
        string path = rom.Paths.First(p => p.EndsWith("pl_pokegra.narc", StringComparison.OrdinalIgnoreCase));
        archive = Narc.Open(rom, path);
    }

    public int SpeciesCount => archive.Count / MembersPerSpecies - 1;

    public Bitmap Render(int species, int frame)
    {
        int member = species * MembersPerSpecies + FrontMale;
        if (archive.GetSize(member) == 0)
        {
            member = species * MembersPerSpecies + FrontFemale;
        }
        Bitmap bitmap = new(FrameSize, FrameSize, PixelFormat.Format32bppArgb);
        if (archive.GetSize(member) == 0)
        {
            return bitmap;
        }
        G2d.GetUnpackedCharacterData(archive.ReadMember(member), out CharacterData? character);
        G2d.GetUnpackedPaletteData(archive.ReadMember(species * MembersPerSpecies + NormalPalette), out PaletteFile? palette);
        byte[] pixels = character!.RawData;
        PokemonSprite.PokemonSprite_DecryptPt(pixels);
        for (int y = 0; y < FrameSize; y++)
        {
            for (int x = 0; x < FrameSize; x++)
            {
                int sourceX = x + frame * FrameSize;
                byte packed = pixels[y * RowBytes + sourceX / 2];
                int index = (sourceX & 1) == 0 ? packed & 0xF : packed >> 4;
                if (index == 0)
                {
                    continue;
                }
                ushort color = palette!.Colors[index];
                int r = (color & 0x1F) * 255 / 31;
                int g = ((color >> 5) & 0x1F) * 255 / 31;
                int b = ((color >> 10) & 0x1F) * 255 / 31;
                bitmap.SetPixel(x, y, Color.FromArgb(255, r, g, b));
            }
        }
        return bitmap;
    }
}

internal sealed class MusicSource : ISampleProvider
{
    private readonly Sdat sdat;
    private readonly List<SequenceInfo> sequences;
    private readonly object gate = new();
    private SequencePlayer? player;
    private SequenceInfo? current;

    public MusicSource(NitroRom rom)
    {
        sdat = Sdat.Open(rom, rom.Paths.First(p => p.EndsWith(".sdat", StringComparison.OrdinalIgnoreCase)));
        sequences = sdat.Sequences.Where(s => s.Name.StartsWith("SEQ_", StringComparison.OrdinalIgnoreCase) && !s.Name.StartsWith("SEQ_SE", StringComparison.OrdinalIgnoreCase)).ToList();
        if (sequences.Count == 0)
        {
            sequences = sdat.Sequences.ToList();
        }
        Names = sequences.Select(s => s.Name).ToList();
        int town = Names.FindIndex(n => n.Contains("TOWN", StringComparison.OrdinalIgnoreCase));
        DefaultIndex = town >= 0 ? town : 0;
    }

    public List<string> Names { get; }

    public int DefaultIndex { get; }

    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    public void Select(int index)
    {
        lock (gate)
        {
            current = sequences[index];
            player = new SequencePlayer(sdat, current);
        }
    }

    public int Read(float[] buffer, int offset, int count)
    {
        lock (gate)
        {
            int frames = count / 2;
            int done = 0;
            int restarts = 0;
            while (done < frames && player != null)
            {
                int rendered = player.Render(buffer, offset + done * 2, frames - done);
                done += rendered;
                if (player.Finished || rendered == 0)
                {
                    if (++restarts > 2)
                    {
                        break;
                    }
                    player = new SequencePlayer(sdat, current!);
                }
            }
            Array.Clear(buffer, offset + done * 2, count - done * 2);
            return count;
        }
    }
}

internal sealed class SpriteStage
{
    private sealed record ObjectSet(string Name, string Archive, int Tiles, int Palette, int Cells, int Animations);

    private const int MaxSprites = 7;
    private const int RowX = 22;
    private const int RowY = 34;
    private const int RowSpacing = 36;
    private const int SpinnerX = 226;
    private const int SpinnerY = 92;
    private const uint ObjMappingMask = 0x00300010;

    private static readonly ObjectSet[] sets =
    {
        new("Code entry", "arc/codein_gra.narc", 0, 1, 2, 3),
        new("Slot machine", "data/slot.narc", 11, 10, 12, 13),
        new("Versus intro", "battle/graphic/vs_demo_gra.narc", 5, 8, 6, 7),
        new("Balance ball", "arc/balance_ball_gra.narc", 13, 14, 16, 15),
        new("Mime Jr. stage", "arc/manene.narc", 17, 18, 20, 19),
        new("Ghost gym", "arc/plgym_ghost.narc", 1, 4, 2, 3),
    };

    private readonly NitroRom rom;
    private readonly G2dRenderer renderer = new();
    private readonly SpriteList list;
    private readonly List<Sprite> sprites = new();
    private readonly ImageProxy imageProxy = new();
    private readonly ImagePaletteProxy paletteProxy = new();
    private Sprite? spinner;
    private int frame;

    public SpriteStage(NitroRom rom)
    {
        this.rom = rom;
        RenderOam.RenderOam_Init(0, 128, 0, 32, 0, 128, 0, 32, 0);
        list = SpriteUtil.SpriteList_InitRendering(MaxSprites + 1, renderer, 0);
    }

    public static IEnumerable<string> Names => sets.Select(s => s.Name);

    public string Status { get; private set; } = "";

    public void Load(int index)
    {
        ObjectSet set = sets[index];
        Sprite.SpriteList_DeleteAll(list);
        sprites.Clear();
        spinner = null;
        Narc archive = Narc.Open(rom, rom.Paths.First(p => p.EndsWith(set.Archive, StringComparison.OrdinalIgnoreCase)));
        if (!G2d.GetUnpackedCharacterData(archive.ReadMember(set.Tiles), out CharacterData? tiles)
            || !G2d.GetUnpackedPaletteData(archive.ReadMember(set.Palette), out PaletteFile? palette)
            || !G2d.GetUnpackedCellBank(archive.ReadMember(set.Cells), out CellDataBank? cells)
            || !G2d.GetUnpackedAnimBank(archive.ReadMember(set.Animations), out AnimBankData? animations))
        {
            Status = "resources missing";
            return;
        }
        Video.SetDisplayControl(0, (Video.GetDisplayControl(0) & ~ObjMappingMask) | (tiles!.MappingType & ObjMappingMask));
        imageProxy.Init();
        paletteProxy.Init();
        G2d.LoadImageMapping(tiles.MappingType == 0 ? G2d.Layout2D : G2d.Layout1D, tiles, 0, G2d.VramMain, imageProxy);
        G2d.LoadPalette(palette!, null, 0, G2d.VramMain, paletteProxy);
        SpriteResourcesHeader header = new()
        {
            ImageProxy = imageProxy,
            CharData = tiles,
            PaletteProxy = paletteProxy,
            CellBank = cells,
            CellAnimBank = animations,
        };
        Sprite? first = Add(header, RowX, RowY);
        if (first == null)
        {
            Status = "no sprite";
            return;
        }
        int count = (int)Sprite.Sprite_GetAnimCount(first);
        sprites.Add(first);
        for (int i = 1; i < Math.Min(count, MaxSprites); i++)
        {
            Sprite? sprite = Add(header, RowX + i * RowSpacing, RowY);
            if (sprite != null)
            {
                Sprite.Sprite_SetAnim(sprite, (uint)i);
                sprites.Add(sprite);
            }
        }
        spinner = Add(header, SpinnerX, SpinnerY);
        if (spinner != null)
        {
            Sprite.Sprite_SetAffineOverwriteMode(spinner, NnsG2d.AffineOverwriteDouble);
        }
        Status = count + " animations";
    }

    private Sprite? Add(SpriteResourcesHeader header, int x, int y)
    {
        SpriteListTemplate template = new()
        {
            List = list,
            ResourceData = header,
            Position = new VecFx32 { X = x << Fx32.Shift, Y = y << Fx32.Shift },
            Priority = 0,
            VramType = G2d.VramMain,
            HeapID = 0,
        };
        Sprite? sprite = Sprite.SpriteList_Add(template);
        if (sprite != null)
        {
            Sprite.Sprite_SetAnimateFlag(sprite, 1);
        }
        return sprite;
    }

    public void Tick()
    {
        frame++;
        if (spinner != null)
        {
            Sprite.Sprite_SetAffineZRotation(spinner, (ushort)(frame * 0x180));
            int scale = Fx32.One + Render3D.Sin((frame * 24) & 0xFFF) / 2;
            Sprite.Sprite_SetAffineScale(spinner, new VecFx32 { X = scale, Y = scale, Z = Fx32.One });
            if (frame % 240 == 0)
            {
                uint next = (Sprite.Sprite_GetActiveAnim(spinner) + 1) % Sprite.Sprite_GetAnimCount(spinner);
                Sprite.Sprite_SetAnim(spinner, next);
            }
        }
        foreach (Sprite sprite in sprites)
        {
            if (!Sprite.Sprite_IsAnimated(sprite) && frame % 90 == 0)
            {
                Sprite.Sprite_RestartAnim(sprite);
            }
        }
        Sprite.SpriteList_Update(list);
        RenderOam.RenderOam_Transfer();
    }
}

internal sealed class GameScreen
{
    private const int TextLayer = 1;
    private const ushort FrameTile = 1;
    private const byte FramePalette = 13;
    private const byte TextPalette = 14;
    private const ushort WindowTile = 0x40;
    private const byte White = 15;
    private const int SpriteX = 128;
    private const int SpriteY = 84;
    private const int AttributeX = 0;
    private const int AttributeY = 1;
    private const int AttributeRotation = 9;
    private const int AttributeScaleX = 0xC;
    private const int AttributeScaleY = 0xD;
    private const int FadeOut = 0;
    private const int FadeIn = 1;
    private const ushort EndOfString = 0xFFFF;
    private const ushort BackdropColor = 0x5A8B;
    private const int TypingDelay = 6;

    private readonly BgConfig bgConfig;
    private readonly Window window;
    private readonly PokemonAnimManager animManager;
    private readonly PokemonSprite sprite = new();
    private readonly int[] pixels = new int[Rasterizer.Width * Rasterizer.Height];
    private readonly Bitmap screen = new(Rasterizer.Width, Rasterizer.Height, PixelFormat.Format32bppArgb);
    private readonly Bitmap boxLayer = new(Rasterizer.Width, Rasterizer.Height, PixelFormat.Format32bppArgb);
    private GameString? speciesName;
    private GameString? moveName;
    private byte printerID = Text.MaxPrinters;
    private int lastSecond = -1;
    private int fadeStage;
    private bool animActive;

    public GameScreen()
    {
        SystemState system = Globals.gSystem;
        system.MainTaskManager = SysTaskManager.SysTaskManager_Init(32, new SysTaskManager());
        system.VBlankTaskManager = SysTaskManager.SysTaskManager_Init(32, new SysTaskManager());
        system.PostVBlankTaskManager = SysTaskManager.SysTaskManager_Init(32, new SysTaskManager());
        system.PrintTaskManager = SysTaskManager.SysTaskManager_Init(4, new SysTaskManager());
        Rtc.InitRTC();
        Bg.SetAllGraphicsModes(new GraphicsModes { DisplayMode = 1 });
        bgConfig = Bg.BgConfig_New(0);
        Bg.Bg_InitFromTemplate(bgConfig, TextLayer, new BgTemplate { BufferSize = 0x800, ScreenSize = 1, ScreenBase = 30 }, Bg.TypeStatic);
        Bg.Bg_ClearTilesRange(TextLayer, 0x20, 0, 0);
        Bg.Bg_MaskPalette(TextLayer, BackdropColor);
        Font.Fonts_Init();
        Font.Font_InitManager(0, 0);
        Font.Font_LoadTextPalette(0, TextPalette * 0x20, 0);
        RenderWindow.LoadMessageBoxGraphics(bgConfig, TextLayer, FrameTile, FramePalette, 0, 0);
        window = Bg.Window_New(0, 1)[0];
        Bg.Window_Add(bgConfig, window, TextLayer, 2, 19, 27, 4, TextPalette, WindowTile);
        TextGlyphDecoder.Text_GenerateHalfRowLookupTable(1, White, 2);
        animManager = PokemonAnimManager.PokemonAnimManager_New(0, 1, 0);
        ResetSprite();
        GXLayers.GXLayers_TurnBothDispOn();
    }

    public bool AnimationRunning => animActive && PokemonAnimManager.PokemonAnimManager_HasAnimCompleted(animManager, 0) == 0;

    private void ResetSprite()
    {
        PokemonSprite.PokemonSprite_SetAttribute(sprite, AttributeX, SpriteX);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, AttributeY, SpriteY);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, AttributeRotation, 0);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, AttributeScaleX, 0x100);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, AttributeScaleY, 0x100);
    }

    public void SetSpecies(int species)
    {
        speciesName = MessageUtil.MessageUtil_SpeciesName((uint)species, 0);
        moveName = MessageUtil.MessageUtil_MoveName((uint)species, 0);
        if (printerID < Text.MaxPrinters && Text.Text_IsPrinterActive(printerID) != 0)
        {
            Text.Text_RemovePrinter(printerID);
        }
        Bg.Window_FillTilemap(window, White);
        RenderWindow.Window_DrawMessageBox(window, FrameTile, FramePalette);
        Bg.Window_CopyToVRAM(window);
        printerID = (byte)Text.Text_AddPrinterWithParams(window, 0, speciesName, 4, 0, TypingDelay, null);
        Text.Text_AddPrinterWithParams(window, 0, moveName, 100, 16, Text.InstantDelay, null);
        lastSecond = -1;
    }

    public void PlayAnimation(int animation)
    {
        if (animActive)
        {
            PokemonAnimManager.PokemonAnimManager_DeleteAnim(animManager, 0);
            animManager.Anims[0].Clear();
        }
        ResetSprite();
        PokemonAnimManager.PokemonAnimManager_InitAnim(animManager, sprite, new PokemonAnimTemplate { Animation = (ushort)animation }, 0);
        animActive = true;
    }

    public void StartFade()
    {
        if (fadeStage == 0)
        {
            ScreenFadeControl.StartScreenFade(0, FadeOut, FadeOut, 0, 12, 1, 0);
            fadeStage = 1;
        }
    }

    private int DrawText(ushort[] characters, int x, int y)
    {
        for (int i = 0; i < characters.Length && characters[i] != EndOfString; i++)
        {
            TextGlyph glyph = Font.Font_TryLoadGlyph(0, characters[i]);
            Bg.Window_CopyGlyph(window, glyph.Pixels, glyph.Width, glyph.Height, x, y, 0);
            x += glyph.Width;
        }
        return x;
    }

    private void DrawNumber(int value, int x, int y)
    {
        ushort[] digits = new ushort[4];
        CharCode.CharCode_FromInt(digits, 0, value, 2, 2);
        digits[2] = EndOfString;
        DrawText(digits, x, y);
    }

    private void DrawClock()
    {
        Bg.Window_FillRectWithColor(window, White, 4, 16, 64, 16);
        TextGlyphDecoder.Text_GenerateHalfRowLookupTable(1, White, 2);
        Rtc.GetCurrentDateTime(out _, out RtcTime time);
        DrawNumber(time.Hour, 4, 16);
        DrawNumber(time.Minute, 24, 16);
        DrawNumber(time.Second, 44, 16);
        Bg.Window_CopyToVRAM(window);
    }

    public void Tick()
    {
        SysTaskManager.SysTaskManager_ExecuteTasks(Globals.gSystem.MainTaskManager!);
        SysTaskManager.SysTaskManager_ExecuteTasks(Globals.gSystem.PrintTaskManager!);
        ScreenFadeControl.ExecScreenFade();
        if (fadeStage == 1 && ScreenFadeControl.IsScreenFadeDone())
        {
            ScreenFadeControl.StartScreenFade(0, FadeIn, FadeIn, 0, 12, 1, 0);
            fadeStage = 2;
        }
        else if (fadeStage == 2 && ScreenFadeControl.IsScreenFadeDone())
        {
            fadeStage = 0;
        }
        for (int i = 0; i < 11; i++)
        {
            Rtc.UpdateRTC();
        }
        Rtc.GetCurrentDateTime(out _, out RtcTime time);
        if (time.Second != lastSecond)
        {
            lastSecond = time.Second;
            DrawClock();
        }
        Bg.Bg_RunScheduledUpdates(bgConfig);
    }

    private void CopyPixels(Bitmap target)
    {
        BitmapData data = target.LockBits(new Rectangle(0, 0, target.Width, target.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        System.Runtime.InteropServices.Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
        target.UnlockBits(data);
    }

    public Bitmap Render(Bitmap spriteFrame)
    {
        Rasterizer.RenderBackgrounds(0, pixels);
        Rasterizer.RenderObjects(0, pixels);
        CopyPixels(screen);
        CopyPixels(boxLayer);
        using Graphics graphics = Graphics.FromImage(screen);
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = PixelOffsetMode.Half;
        float scaleX = PokemonSprite.PokemonSprite_GetAttribute(sprite, AttributeScaleX) / 256f;
        float scaleY = PokemonSprite.PokemonSprite_GetAttribute(sprite, AttributeScaleY) / 256f;
        float angle = (ushort)PokemonSprite.PokemonSprite_GetAttribute(sprite, AttributeRotation) * 360f / 65536f;
        graphics.TranslateTransform(PokemonSprite.PokemonSprite_GetAttribute(sprite, AttributeX), PokemonSprite.PokemonSprite_GetAttribute(sprite, AttributeY));
        graphics.RotateTransform(angle);
        graphics.ScaleTransform(scaleX == 0 ? 1 : scaleX, scaleY == 0 ? 1 : scaleY);
        graphics.DrawImage(spriteFrame, -spriteFrame.Width / 2, -spriteFrame.Height / 2, spriteFrame.Width, spriteFrame.Height);
        graphics.ResetTransform();
        graphics.SetClip(new Rectangle(0, 136, Rasterizer.Width, 56));
        graphics.DrawImage(boxLayer, 0, 0, Rasterizer.Width, Rasterizer.Height);
        graphics.ResetClip();
        int brightness = Video.GetMasterBrightness(0);
        if (brightness != 0)
        {
            int alpha = Math.Min(255, Math.Abs(brightness) * 255 / 16);
            using SolidBrush brush = new(Color.FromArgb(alpha, brightness > 0 ? Color.White : Color.Black));
            graphics.FillRectangle(brush, 0, 0, screen.Width, screen.Height);
        }
        return screen;
    }
}

internal sealed class PreviewForm : Form
{
    private const int Zoom = 2;
    private const float StartVolume = 0.08f;
    private const int AnimationCount = 143;

    private readonly NitroRom rom;
    private readonly SpriteSource sprites;
    private readonly MusicSource music;
    private readonly GameScreen game;
    private readonly SpriteStage stage;
    private readonly ComboBox objects = new();
    private readonly Label objectStatus = new();
    private readonly WaveOutEvent output = new();
    private readonly PictureBox picture = new();
    private readonly NumericUpDown species = new();
    private readonly NumericUpDown animation = new();
    private readonly CheckBox autoPlay = new();
    private readonly ComboBox sequence = new();
    private readonly TrackBar volume = new();
    private readonly Label volumeLabel = new();
    private readonly System.Windows.Forms.Timer timer = new();
    private readonly Bitmap[] frames = new Bitmap[2];
    private int tick;
    private int idleFrames;

    public PreviewForm(string romPath)
    {
        rom = new NitroRom(romPath);
        RomData.Open(rom);
        sprites = new SpriteSource(rom);
        music = new MusicSource(rom);
        game = new GameScreen();
        stage = new SpriteStage(rom);

        Text = "PlatinumSharp preview";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ClientSize = new Size(Rasterizer.Width * Zoom + 24, Rasterizer.Height * Zoom + 184);

        picture.SetBounds(12, 12, Rasterizer.Width * Zoom, Rasterizer.Height * Zoom);
        picture.BackColor = Color.Black;
        picture.Paint += PaintScreen;
        Controls.Add(picture);

        int y = picture.Bottom + 10;
        Controls.Add(new Label { Text = "Sprite", AutoSize = true, Location = new Point(12, y + 3) });
        species.SetBounds(70, y, 70, 23);
        species.Minimum = 1;
        species.Maximum = Math.Max(1, sprites.SpeciesCount);
        species.ValueChanged += (_, _) => LoadSprite();
        Controls.Add(species);

        Controls.Add(new Label { Text = "Animation", AutoSize = true, Location = new Point(160, y + 3) });
        animation.SetBounds(228, y, 60, 23);
        animation.Minimum = 0;
        animation.Maximum = AnimationCount - 1;
        animation.Value = 1;
        Controls.Add(animation);
        Button play = new() { Text = "Play", Location = new Point(296, y - 1), Size = new Size(60, 25) };
        play.Click += (_, _) => game.PlayAnimation((int)animation.Value);
        Controls.Add(play);
        autoPlay.Text = "Cycle";
        autoPlay.Checked = true;
        autoPlay.AutoSize = true;
        autoPlay.Location = new Point(364, y + 2);
        Controls.Add(autoPlay);
        Button fade = new() { Text = "Fade", Location = new Point(440, y - 1), Size = new Size(60, 25) };
        fade.Click += (_, _) => game.StartFade();
        Controls.Add(fade);

        y += 34;
        Controls.Add(new Label { Text = "Objects", AutoSize = true, Location = new Point(12, y + 3) });
        objects.SetBounds(70, y, 200, 23);
        objects.DropDownStyle = ComboBoxStyle.DropDownList;
        objects.Items.AddRange(SpriteStage.Names.ToArray());
        objects.SelectedIndexChanged += (_, _) => LoadObjects();
        Controls.Add(objects);
        objectStatus.SetBounds(280, y + 3, 240, 20);
        Controls.Add(objectStatus);

        y += 34;
        Controls.Add(new Label { Text = "Music", AutoSize = true, Location = new Point(12, y + 3) });
        sequence.SetBounds(70, y, ClientSize.Width - 82, 23);
        sequence.DropDownStyle = ComboBoxStyle.DropDownList;
        sequence.Items.AddRange(music.Names.ToArray());
        sequence.SelectedIndexChanged += (_, _) => music.Select(sequence.SelectedIndex);
        Controls.Add(sequence);

        y += 32;
        Controls.Add(new Label { Text = "Volume", AutoSize = true, Location = new Point(12, y + 3) });
        volume.SetBounds(64, y - 4, ClientSize.Width - 130, 30);
        volume.Minimum = 0;
        volume.Maximum = 100;
        volume.TickFrequency = 10;
        volume.Value = (int)(StartVolume * 100);
        volume.ValueChanged += (_, _) => ApplyVolume();
        Controls.Add(volume);
        volumeLabel.SetBounds(ClientSize.Width - 60, y + 3, 50, 20);
        Controls.Add(volumeLabel);

        LoadSprite();
        objects.SelectedIndex = 0;
        sequence.SelectedIndex = music.DefaultIndex;
        output.Init(music);
        ApplyVolume();
        output.Play();
        game.PlayAnimation((int)animation.Value);

        timer.Interval = 16;
        timer.Tick += (_, _) => Step();
        timer.Start();
    }

    private void Step()
    {
        tick++;
        game.Tick();
        stage.Tick();
        if (!game.AnimationRunning)
        {
            idleFrames++;
            if (autoPlay.Checked && idleFrames > 40)
            {
                animation.Value = ((int)animation.Value + 1) % AnimationCount;
                game.PlayAnimation((int)animation.Value);
                idleFrames = 0;
            }
        }
        else
        {
            idleFrames = 0;
        }
        picture.Invalidate();
    }

    private void ApplyVolume()
    {
        output.Volume = volume.Value / 100f;
        volumeLabel.Text = volume.Value + "%";
    }

    private void LoadObjects()
    {
        stage.Load(objects.SelectedIndex);
        objectStatus.Text = stage.Status;
    }

    private void LoadSprite()
    {
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i]?.Dispose();
            frames[i] = sprites.Render((int)species.Value, i);
        }
        game.SetSpecies((int)species.Value);
        picture.Invalidate();
    }

    private void PaintScreen(object? sender, PaintEventArgs e)
    {
        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        e.Graphics.DrawImage(game.Render(frames[(tick / 30) % frames.Length]), 0, 0, picture.Width, picture.Height);
    }

    public void SaveScreenshot(string path)
    {
        for (int i = 0; i < 20; i++)
        {
            game.Tick();
            stage.Tick();
        }
        game.Render(frames[0]).Save(path, ImageFormat.Png);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        timer.Stop();
        output.Stop();
        output.Dispose();
        rom.Dispose();
        base.OnFormClosed(e);
    }
}
