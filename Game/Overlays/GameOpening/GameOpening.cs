using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed class OpeningIntroScene
{
    public byte State;
    public byte SubState;
    public byte Loaded;
    public byte BlendStep;
    public byte BlendTimer;
    public byte BlendDone;
    public object? BgConfig;
    public SysTask? Task;
    public OpeningEffects? Effects;
}

public sealed class OpeningData
{
    public int HeapID;
    public int Frame;
    public int Skipped;
    public BgConfig? BgConfig;
    public G3DPipelineBuffers? Pipeline;
    public uint SavedSeed;
    public readonly OpeningIntroScene Intro = new();
    public readonly OpeningWorldSceneData World = new();
    public readonly OpeningLegendSceneData Legend = new();
    public byte CanSkip;

    public OpeningStarters? Starters
    {
        get => World.Starters;
        set => World.Starters = value;
    }
}

public static class GameOpening
{
    private const int HeapID = 0x4C;
    private const int HeapSize = 0xA0000;
    private const int Overlay = 77;
    private const int TitleScreenOverlay = 0x4D;
    private const uint TitleScreenTemplate = 0x021D742C;
    private const int KeyA = 1;
    private const int KeyStart = 8;
    private const ushort White = 0x7FFF;
    private const int LastFrame = 0x97E;
    private const uint IntroBanks = 0x021D76F4;
    private const uint IntroGraphicsModes = 0x021D74CC;
    private const int BgTemplateSize = 0x1C;
    private const int GraphicsArchive = 0x80;
    private const int LogoArchive = 0x30;
    private const int PaletteMainBg = 0;
    private const int PaletteSubBg = 4;
    private const uint MainBlendRegister = 0;
    private const int BlendMax = 0x10;
    private const int IntroFadeInDelay = 6;
    private const int IntroFadeStep = 4;
    private const int IntroLogoFadeFrame = 0x73;
    private const int IntroCrossFadeFrame = 0x109;
    private const int IntroEndFrame = 0x1EA;
    private const int IntroBgm = 0x494;

    private static BgTemplate ReadBgTemplate(byte[] raw, int offset)
    {
        return new BgTemplate
        {
            X = Bin.U32(raw, offset),
            Y = Bin.U32(raw, offset + 4),
            BufferSize = Bin.U32(raw, offset + 8),
            BaseTile = Bin.U32(raw, offset + 0xC),
            ScreenSize = raw[offset + 0x10],
            ColorMode = raw[offset + 0x11],
            ScreenBase = raw[offset + 0x12],
            CharBase = raw[offset + 0x13],
            BgExtPltt = raw[offset + 0x14],
            Priority = raw[offset + 0x15],
            AreaOver = raw[offset + 0x16],
            Mosaic = (int)Bin.U32(raw, offset + 0x18),
        };
    }

    public static BgTemplate BgTemplateAt(uint address)
    {
        return ReadBgTemplate(RomData.Overlay(Overlay, address, BgTemplateSize), 0);
    }

    public static BgTemplate[] BgTemplatesAt(uint address, int count)
    {
        byte[] raw = RomData.Overlay(Overlay, address, BgTemplateSize * count);
        BgTemplate[] templates = new BgTemplate[count];
        for (int i = 0; i < count; i++)
        {
            templates[i] = ReadBgTemplate(raw, i * BgTemplateSize);
        }
        return templates;
    }

    public static GraphicsModes GraphicsModesAt(uint address)
    {
        byte[] raw = RomData.Overlay(Overlay, address, 0x10);
        return new GraphicsModes
        {
            DisplayMode = (int)Bin.U32(raw, 0),
            MainBgMode = (int)Bin.U32(raw, 4),
            SubBgMode = (int)Bin.U32(raw, 8),
            Bg0As2DOr3D = (int)Bin.U32(raw, 0xC),
        };
    }

    public static GXBanks BanksAt(uint address)
    {
        byte[] raw = RomData.Overlay(Overlay, address, 0x28);
        return new GXBanks
        {
            Bg = Bin.U32(raw, 0),
            BgExtPltt = Bin.U32(raw, 4),
            SubBg = Bin.U32(raw, 8),
            SubBgExtPltt = Bin.U32(raw, 0xC),
            Obj = Bin.U32(raw, 0x10),
            ObjExtPltt = Bin.U32(raw, 0x14),
            SubObj = Bin.U32(raw, 0x18),
            SubObjExtPltt = Bin.U32(raw, 0x1C),
            Tex = Bin.U32(raw, 0x20),
            TexPltt = Bin.U32(raw, 0x24),
        };
    }

    // OFFSET 0x021D25B0
    private static void GameOpening_SetIntroBanks()
    {
        GXLayers.GXLayers_SetBanks(BanksAt(IntroBanks));
    }

    // OFFSET 0x021D2610
    private static void GameOpening_InitIntroBgs(OpeningData data)
    {
        data.BgConfig = Bg.BgConfig_New(data.HeapID);
        Bg.SetAllGraphicsModes(GraphicsModesAt(IntroGraphicsModes));
        Bg.Bg_InitFromTemplate(data.BgConfig, 1, BgTemplateAt(0x021D7510), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 2, BgTemplateAt(0x021D752C), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 3, BgTemplateAt(0x021D7548), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 5, BgTemplateAt(0x021D75F0), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 6, BgTemplateAt(0x021D75D4), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 7, BgTemplateAt(0x021D7580), 0);
        Bg.Bg_MaskPalette(0, 0);
        Bg.Bg_MaskPalette(4, 0);
    }

    // OFFSET 0x021D2B30
    private static void GameOpening_IntroVBlank(object? data)
    {
        RenderOam.RenderOam_Transfer();
    }

    // OFFSET 0x021D2D08
    public static int GameOpening_Init(ApplicationManager appMan, ref int state)
    {
        BrightnessController.BrightnessController_ResetAllControllers();
        ScreenFadeControl.SetScreenColorBrightness(0, White);
        ScreenFadeControl.SetScreenColorBrightness(1, White);
        GameSystem.SetVBlankCallback(null, null);
        GameSystem.SetHBlankCallback(null, null);
        GXLayers.GXLayers_DisableEngineALayers();
        GXLayers.GXLayers_DisableEngineBLayers();
        GameSystem.SetAutorepeat(4, 8);
        Heap.Heap_Create(3, HeapID, HeapSize);
        OpeningData data = new();
        appMan.Data = data;
        data.HeapID = HeapID;
        data.Skipped = 0;
        data.CanSkip = 0;
        gSystem.WhichScreenIs3D = 0;
        GXLayers.GXLayers_SwapDisplay();
        data.SavedSeed = MathUtil.LCRNG_GetSeed();
        MathUtil.LCRNG_SetSeed(0);
        return 1;
    }

    // OFFSET 0x021D2D94
    public static int GameOpening_Main(ApplicationManager appMan, ref int state)
    {
        OpeningData data = (OpeningData)ApplicationManager.ApplicationManager_Data(appMan)!;
        if (data.CanSkip != 0)
        {
            if ((gSystem.PressedKeys & KeyA) != 0 || (gSystem.PressedKeys & KeyStart) != 0)
            {
                data.Skipped = 1;
                gSystem.ShowTitleScreenIntro = 0;
                ScreenFadeControl.SetScreenColorBrightness(0, 0);
                ScreenFadeControl.SetScreenColorBrightness(1, 0);
            }
        }
        switch (state)
        {
            case 0:
                state++;
                break;
            case 1:
                if (GameOpening_RunIntro(data))
                {
                    state++;
                }
                break;
            case 2:
                if (OpeningWorldScene.OpeningWorldScene_Run(data))
                {
                    state++;
                }
                break;
            case 3:
                if (OpeningWorldScene.OpeningLegendScene_Run(data))
                {
                    state++;
                }
                break;
            case 4:
                if (data.Frame >= LastFrame)
                {
                    return 1;
                }
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        if (data.Skipped != 0)
        {
            return 1;
        }
        data.Frame++;
        return 0;
    }

    // OFFSET 0x021D2E60
    public static int GameOpening_Exit(ApplicationManager appMan, ref int state)
    {
        OpeningData data = (OpeningData)ApplicationManager.ApplicationManager_Data(appMan)!;
        if (!ScreenFadeControl.IsScreenFadeDone())
        {
            ScreenFadeControl.FinishScreenFade();
        }
        MathUtil.LCRNG_SetSeed(data.SavedSeed);
        ApplicationManager.ApplicationManager_FreeData(appMan);
        Heap.Heap_Destroy(HeapID);
        Main.EnqueueApplication(TitleScreenOverlay, ApplicationTemplate.At(TitleScreenTemplate));
        return 1;
    }

    // OFFSET 0x021D2E9C
    private static bool GameOpening_RunIntro(OpeningData data)
    {
        OpeningIntroScene scene = data.Intro;
        if (data.Skipped != 0)
        {
            scene.State = 3;
        }
        switch (scene.State)
        {
            case 0:
                GameOpening_InitIntro(data);
                scene.State++;
                break;
            case 1:
                GameOpening_LoadIntroGraphics(scene);
                scene.State++;
                break;
            case 2:
                if (GameOpening_UpdateIntro(scene, data))
                {
                    scene.State++;
                }
                break;
            case 3:
                GameOpening_FreeIntro(data);
                return true;
        }
        return false;
    }

    // OFFSET 0x021D2F0C
    private static void GameOpening_InitIntro(OpeningData data)
    {
        GameOpening_SetIntroBanks();
        GameOpening_InitIntroBgs(data);
        data.Intro.BgConfig = data.BgConfig;
        data.Intro.Effects = OpeningEffects.OpeningEffects_New();
        GameSystem.SetVBlankCallback(GameOpening_IntroVBlank, data);
        data.Intro.Loaded = 1;
    }

    // OFFSET 0x021D2F38
    private static void GameOpening_LoadIntroGraphics(OpeningIntroScene scene)
    {
        object bgConfig = scene.BgConfig!;
        Graphics.Graphics_LoadTilesToBgLayer(GraphicsArchive, 0x10, bgConfig, 2, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayer(GraphicsArchive, 0x10, bgConfig, 6, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayer(GraphicsArchive, 0x11, bgConfig, 2, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayer(GraphicsArchive, 0x12, bgConfig, 6, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadPalette(GraphicsArchive, 0xF, PaletteMainBg, 0, 0, HeapID);
        Graphics.Graphics_LoadPalette(GraphicsArchive, 0xF, PaletteSubBg, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayer(LogoArchive, 0xF, bgConfig, 1, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayer(LogoArchive, 0xD, bgConfig, 1, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadPalette(LogoArchive, 0xE, PaletteMainBg, 0, 0x20, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayer(GraphicsArchive, 0x13, bgConfig, 3, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayer(GraphicsArchive, 0x13, bgConfig, 7, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayer(GraphicsArchive, 0x72, bgConfig, 5, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayer(GraphicsArchive, 0x71, bgConfig, 5, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadPalette(GraphicsArchive, 0x73, PaletteSubBg, 0, 0x20, HeapID);
        GXLayers.GXLayers_EngineAToggleLayers(4, 0);
        GXLayers.GXLayers_EngineBToggleLayers(4, 0);
        Video.SetBlendAlpha(0, 2, 8, 0, BlendMax);
        Video.SetBlendAlpha(1, 2, 8, 0, BlendMax);
        Interrupts.Wait(true, Interrupts.VBlank);
        ScreenFadeControl.ResetScreenMasterBrightness(0);
        ScreenFadeControl.ResetScreenMasterBrightness(1);
        GXLayers.GXLayers_TurnBothDispOn();
    }

    // OFFSET 0x021D30D0
    private static bool GameOpening_UpdateIntro(OpeningIntroScene scene, OpeningData data)
    {
        int frame = data.Frame;
        OpeningEffects.OpeningEffects_Update(scene.Effects!, frame);
        switch (scene.SubState)
        {
            case 0:
                scene.BlendTimer = 0;
                scene.BlendStep = 0;
                scene.BlendDone = 0;
                scene.Task = SysTask.SysTask_Start(GameOpening_FadeInLogoTask, data, 0);
                Sound.Sound_SetSceneAndPlayBGM(1, IntroBgm, 1);
                scene.SubState++;
                break;
            case 1:
                if (scene.BlendDone != 0 && frame >= IntroLogoFadeFrame)
                {
                    scene.BlendTimer = 0;
                    scene.BlendStep = BlendMax;
                    scene.BlendDone = 0;
                    scene.Task = SysTask.SysTask_Start(GameOpening_FadeOutLogoTask, data, 0);
                    scene.SubState++;
                }
                break;
            case 2:
                if (scene.BlendDone != 0 && frame >= IntroCrossFadeFrame)
                {
                    GXLayers.GXLayers_EngineAToggleLayers(2, 0);
                    GXLayers.GXLayers_EngineBToggleLayers(2, 0);
                    Video.SetBlendAlpha(0, 4, 8, 0, BlendMax);
                    Video.SetBlendAlpha(1, 4, 8, 0, BlendMax);
                    GXLayers.GXLayers_EngineAToggleLayers(4, 1);
                    GXLayers.GXLayers_EngineBToggleLayers(4, 1);
                    scene.BlendTimer = 0;
                    scene.BlendStep = 0;
                    scene.BlendDone = 0;
                    scene.Task = SysTask.SysTask_Start(GameOpening_FadeInTopTask, data, 0);
                    scene.SubState++;
                }
                break;
            case 3:
                if (scene.BlendDone != 0)
                {
                    scene.BlendTimer = 0;
                    scene.BlendStep = 0;
                    scene.BlendDone = 0;
                    scene.Task = SysTask.SysTask_Start(GameOpening_FadeInBottomTask, data, 0);
                    scene.SubState++;
                }
                break;
            case 4:
                if (scene.BlendDone != 0 && frame >= IntroEndFrame)
                {
                    ScreenFadeControl.StartScreenFade(2, 0, 0, 0, 0x12, 1, HeapID);
                    scene.SubState++;
                }
                break;
            case 5:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    return true;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x021D3234
    private static void GameOpening_FreeIntro(OpeningData data)
    {
        OpeningIntroScene scene = data.Intro;
        GameSystem.SetVBlankCallback(null, null);
        if (scene.Loaded != 0)
        {
            OpeningEffects.OpeningEffects_Free(scene.Effects!);
            Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 1);
            Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 2);
            Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 6);
            Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 3);
            Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 7);
            Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 5);
            data.BgConfig = null;
            scene.Loaded = 0;
        }
        if (scene.Task != null)
        {
            if (data.Skipped == 0)
            {
                ErrorHandling.GF_AssertFail();
            }
            SysTask.SysTask_Done(scene.Task);
            scene.Task = null;
        }
    }

    private static bool AdvanceBlend(OpeningIntroScene scene, SysTask task, int delay, int direction)
    {
        scene.BlendTimer++;
        if (scene.BlendTimer >= delay)
        {
            scene.BlendTimer = 0;
            scene.BlendStep = (byte)(scene.BlendStep + direction);
        }
        bool finished = direction > 0 ? scene.BlendStep >= BlendMax : scene.BlendStep == 0;
        if (finished)
        {
            SysTask.SysTask_Done(task);
            scene.Task = null;
            scene.BlendDone = 1;
        }
        return finished;
    }

    // OFFSET 0x021D32A4
    private static void GameOpening_FadeInLogoTask(SysTask task, object? param)
    {
        OpeningData data = (OpeningData)param!;
        AdvanceBlend(data.Intro, task, IntroFadeInDelay, 1);
        Video.SetBlendAlpha(0, 2, 8, data.Intro.BlendStep, BlendMax);
        Video.SetBlendAlpha(1, 2, 8, data.Intro.BlendStep, BlendMax);
    }

    // OFFSET 0x021D3300
    private static void GameOpening_FadeOutLogoTask(SysTask task, object? param)
    {
        OpeningData data = (OpeningData)param!;
        if (AdvanceBlend(data.Intro, task, IntroFadeStep, -1))
        {
            data.CanSkip = 1;
        }
        Video.SetBlendAlpha(0, 2, 8, data.Intro.BlendStep, BlendMax);
        Video.SetBlendAlpha(1, 2, 8, data.Intro.BlendStep, BlendMax);
    }

    // OFFSET 0x021D3360
    private static void GameOpening_FadeInTopTask(SysTask task, object? param)
    {
        OpeningData data = (OpeningData)param!;
        AdvanceBlend(data.Intro, task, IntroFadeStep, 1);
        Video.SetBlendAlpha(0, 4, 8, data.Intro.BlendStep, BlendMax);
    }

    // OFFSET 0x021D33A8
    private static void GameOpening_FadeInBottomTask(SysTask task, object? param)
    {
        OpeningData data = (OpeningData)param!;
        AdvanceBlend(data.Intro, task, IntroFadeStep, 1);
        Video.SetBlendAlpha(1, 4, 8, data.Intro.BlendStep, BlendMax);
    }
}
