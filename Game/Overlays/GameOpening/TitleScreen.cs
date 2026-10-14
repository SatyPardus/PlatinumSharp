using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed class TitleScreenModels
{
    public int State;
    public readonly G3dRenderObj RenderObj = new();
    public G3dModel? Model;
    public byte[]? Resource;
    public byte[]? AnimData1;
    public byte[]? AnimData2;
    public G3dAnimObj? AnimObj1;
    public G3dAnimObj? AnimObj2;
    public readonly HeapAllocator Allocator = new();
    public readonly G3dAllocator AnimAllocator = new();
    public VecFx32 Position;
    public VecFx32 Scale;
    public VecFx32 Rotation;
    public Camera? MainCamera;
    public Camera? IntroCamera;
    public int AnimMode;
    public readonly Easy3DObject IntroObject = new();
    public readonly Easy3DAnim IntroAnim1 = new();
    public readonly Easy3DAnim IntroAnim2 = new();
    public readonly Easy3DModel IntroModel = new();
    public readonly Easy3DObject LoopObject = new();
    public readonly Easy3DAnim LoopAnim1 = new();
    public readonly Easy3DAnim LoopAnim2 = new();
    public readonly Easy3DModel LoopModel = new();
    public int IntroFrame;
    public int AngleSpeed;
    public int Angle;
    public int Reserved;
    public int FovSpeed;
    public byte IntroAnim1State;
    public byte IntroAnim2State;
}

public sealed class TitleScreenGraphics
{
    public int State;
    public readonly TitleScreenModels Models = new();
    public ushort Timer;
    public readonly Window Window = new();
    public VecFx32 Target;
    public VecFx32 Position;
    public int ShowPressStart;
    public VecFx32 PositionStart;
    public VecFx32 PositionEnd;
    public VecFx32 TargetStart;
    public VecFx32 TargetEnd;
    public VecFx16 Light0;
    public VecFx16 Light1;
    public ushort LightLevel;
    public ushort LightState;
    public int Reserved;
    public int IntroFinished;
    public int IntroPlaying;
    public int PanFrames;
    public int BobAngle;
}

public sealed class TitleScreenData
{
    public int HeapID;
    public BgConfig? BgConfig;
    public G3DPipelineBuffers? Pipeline;
    public readonly TitleScreenGraphics Graphics = new();
    public ushort NextApplication;
    public int StartDelay;
    public int IdleFrames;
    public int ExitFrames;
}

public static class TitleScreen
{
    private const int HeapID = 0x1E;
    private const int HeapSize = 0x40000;
    private const int Overlay = 77;
    private const int OpeningOverlay = 0x4D;
    private const uint OpeningTemplate = 0x021D788C;
    private const uint MainMenuTemplate = 0x020F8AB4;
    private const uint DeleteSaveTemplate = 0x020F8A48;
    private const uint BanksAddress = 0x021D7404;
    private const uint GraphicsModesAddress = 0x021D72F0;
    private const uint ScaleAddress = 0x021D72E4;
    private const uint IntroCameraAngleAddress = 0x021D72C8;
    private const uint IntroCameraOffsetAddress = 0x021D72D8;
    private const uint WindowTemplateAddress = 0x021D72D0;
    private const int TitleArchive = 0x30;
    private const int OpeningArchive = 0x80;
    private const int TitleBgm = 0x495;
    private const int SpeciesGiratina = 0x1E7;
    private const ushort White = 0x7FFF;
    private const int KeyA = 1;
    private const int KeyB = 2;
    private const int KeySelect = 4;
    private const int KeyStart = 8;
    private const int KeyUp = 0x40;
    private const int NextMainMenu = 1;
    private const int NextDeleteSave = 2;
    private const int NextOpening = 3;
    private const int IdleTimeout = 0x384;
    private const uint AllLayersMask = 0xFFFFE0FF;
    private const ushort Disp3DMask = 0xCFFF;
    private const ushort Disp3DAntiAlias = 0x10;
    private const ushort Disp3DAlphaBlend = 8;
    private const ushort Disp3DEdgeMarkingMask = 0xCFDF;
    private const int OffsetSetX = 0;
    private const int OffsetSetY = 3;
    private const int PaletteMainBg = 0;
    private const int PaletteSubBg = 4;
    private const int PaletteSubBgExt = 6;
    private const int PanDuration = 0x3C;

    private static VecFx32 VectorAt(uint address)
    {
        uint[] raw = RomData.OverlayU32(Overlay, address, 3);
        return new VecFx32 { X = (int)raw[0], Y = (int)raw[1], Z = (int)raw[2] };
    }

    // OFFSET 0x021D0D80
    public static int TitleScreen_Init(ApplicationManager appMan, ref int state)
    {
        ScreenFadeControl.SetScreenColorBrightness(0, 0);
        ScreenFadeControl.SetScreenColorBrightness(1, 0);
        Video.SetBlendControl(1, 0);
        Video.SetBlendControl(0, 0);
        GameSystem.SetVBlankCallback(null, null);
        GameSystem.SetHBlankCallback(null, null);
        GXLayers.GXLayers_DisableEngineALayers();
        GXLayers.GXLayers_DisableEngineBLayers();
        Video.SetDisplayControl(0, Video.GetDisplayControl(0) & AllLayersMask);
        Video.SetDisplayControl(1, Video.GetDisplayControl(1) & AllLayersMask);
        GameSystem.SetAutorepeat(4, 8);
        Heap.Heap_Create(3, HeapID, HeapSize);
        TitleScreenData data = new();
        appMan.Data = data;
        data.HeapID = HeapID;
        data.NextApplication = 0;
        data.IdleFrames = 0;
        TitleScreen_SetBanks();
        TitleScreen_InitBgs(data);
        TitleScreen_Init3D(data);
        GameSystem.SetVBlankCallback(TitleScreen_VBlank, data);
        GXLayers.GXLayers_TurnBothDispOn();
        return 1;
    }

    // OFFSET 0x021D0E3C
    public static int TitleScreen_Main(ApplicationManager appMan, ref int state)
    {
        TitleScreenData data = (TitleScreenData)ApplicationManager.ApplicationManager_Data(appMan)!;
        TitleScreenGraphics graphics = data.Graphics;
        switch (state)
        {
            case 0:
                if (TitleScreen_InitGraphics(graphics, data.BgConfig!, data.HeapID))
                {
                    graphics.State = 0;
                    if (gSystem.ShowTitleScreenIntro == 0)
                    {
                        data.StartDelay = 0x1E;
                        graphics.IntroFinished = 1;
                        graphics.IntroPlaying = 0;
                        state = 2;
                    }
                    else
                    {
                        data.StartDelay = 0;
                        gSystem.ShowTitleScreenIntro = 0;
                        graphics.IntroPlaying = 1;
                        state = 1;
                    }
                }
                break;
            case 1:
                if (TitleScreen_RunIntro(graphics, data.BgConfig!, data.HeapID))
                {
                    graphics.State = 0;
                    state = 2;
                }
                break;
            case 2:
                Sound.Sound_SetScene(0);
                Sound.Sound_SetSceneAndPlayBGM(1, TitleBgm, 1);
                state = 3;
                break;
            case 3:
                if (data.StartDelay != 0)
                {
                    data.StartDelay--;
                    graphics.ShowPressStart = 0;
                    TitleScreen_RunLoop(graphics, data.BgConfig!, data.HeapID);
                    break;
                }
                graphics.ShowPressStart = 1;
                TitleScreen_RunLoop(graphics, data.BgConfig!, data.HeapID);
                data.IdleFrames++;
                if ((gSystem.PressedKeys & KeyA) == KeyA || (gSystem.PressedKeys & KeyStart) == KeyStart)
                {
                    data.NextApplication = NextMainMenu;
                    SoundPlayback.Sound_FadeOutBGM(0, 0x3C);
                    SoundPlayback.Sound_PlayPokemonCry(SpeciesGiratina, 1);
                    TitleScreen_ShowPressedBg(data.BgConfig!, data.HeapID);
                    state = 4;
                    break;
                }
                if ((gSystem.HeldKeys & (KeyUp | KeySelect | KeyB)) == (KeyUp | KeySelect | KeyB))
                {
                    data.NextApplication = NextDeleteSave;
                    ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 6, 1, data.HeapID);
                    state = 6;
                    break;
                }
                if (data.IdleFrames > IdleTimeout)
                {
                    data.NextApplication = NextOpening;
                    gSystem.ShowTitleScreenIntro = 1;
                    GXLayers.GXLayers_EngineBToggleLayers(1, 0);
                    SoundPlayback.Sound_FadeOutBGM(0, 0x3C);
                    state = 5;
                }
                break;
            case 4:
                graphics.ShowPressStart = 0;
                TitleScreen_RunLoop(graphics, data.BgConfig!, data.HeapID);
                data.ExitFrames++;
                if (data.ExitFrames == 10)
                {
                    ScreenFadeControl.StartScreenFade(0, 0, 0, White, 6, 1, data.HeapID);
                }
                if (SoundPlayback.Sound_IsPokemonCryPlaying() == 0 && ScreenFadeControl.IsScreenFadeDone() && data.ExitFrames >= 10)
                {
                    SoundPlayback.Sound_StopBGM(TitleBgm, 0);
                    GXLayers.GXLayers_EngineAToggleLayers(1, 0);
                    GXLayers.GXLayers_EngineAToggleLayers(2, 0);
                    GXLayers.GXLayers_EngineAToggleLayers(4, 0);
                    GXLayers.GXLayers_EngineAToggleLayers(8, 0);
                    GXLayers.GXLayers_EngineBToggleLayers(1, 0);
                    GXLayers.GXLayers_EngineBToggleLayers(2, 0);
                    GXLayers.GXLayers_EngineBToggleLayers(4, 0);
                    GXLayers.GXLayers_EngineBToggleLayers(8, 0);
                    ScreenFadeControl.StartScreenFade(0, 1, 1, White, 6, 1, data.HeapID);
                    state = 6;
                }
                break;
            case 5:
                graphics.ShowPressStart = 0;
                TitleScreen_RunLoop(graphics, data.BgConfig!, data.HeapID);
                if (SoundPlayback.Sound_IsFadeActive() == 0)
                {
                    SoundPlayback.Sound_StopBGM(TitleBgm, 0);
                    ScreenFadeControl.StartScreenFade(0, 0, 0, White, 6, 1, data.HeapID);
                    state = 6;
                }
                break;
            case 6:
                if (ScreenFadeControl.IsScreenFadeDone() && TitleScreen_FreeGraphics(graphics, data.BgConfig!, data.HeapID))
                {
                    return 1;
                }
                break;
        }
        return 0;
    }

    // OFFSET 0x021D10FC
    public static int TitleScreen_Exit(ApplicationManager appMan, ref int state)
    {
        TitleScreenData data = (TitleScreenData)ApplicationManager.ApplicationManager_Data(appMan)!;
        int heapID = data.HeapID;
        int next = data.NextApplication;
        GameSystem.SetVBlankCallback(null, null);
        TitleScreen_Free3D(data);
        TitleScreen_FreeBgs(data);
        ApplicationManager.ApplicationManager_FreeData(appMan);
        Heap.Heap_Destroy(heapID);
        switch (next)
        {
            case NextDeleteSave:
                Main.EnqueueApplication(-1, ApplicationTemplate.At(DeleteSaveTemplate));
                break;
            case NextOpening:
                Sound.Sound_SetScene(0);
                Main.EnqueueApplication(OpeningOverlay, ApplicationTemplate.At(OpeningTemplate));
                break;
            default:
                Main.EnqueueApplication(-1, ApplicationTemplate.At(MainMenuTemplate));
                break;
        }
        return 1;
    }

    // OFFSET 0x021D1178
    private static void TitleScreen_VBlank(object? param)
    {
        TitleScreenData data = (TitleScreenData)param!;
        Bg.Bg_RunScheduledUpdates(data.BgConfig!);
    }

    // OFFSET 0x021D1184
    private static void TitleScreen_SetBanks()
    {
        GXLayers.GXLayers_SetBanks(GameOpening.BanksAt(BanksAddress));
    }

    // OFFSET 0x021D11A4
    private static bool TitleScreen_SkipPressed()
    {
        int pressed = gSystem.PressedKeys;
        return (pressed & KeyA) == KeyA || (pressed & KeyStart) == KeyStart || (pressed & KeySelect) == KeySelect;
    }

    // OFFSET 0x021D11CC
    private static void TitleScreen_Init3D(TitleScreenData data)
    {
        data.Pipeline = G3DPipeline.G3DPipeline_InitEx(data.HeapID, 0, 1, 0, 4, null);
        Video.SetBgControl(0, (ushort)((Video.GetBgControl(0) & ~3) | 1));
    }

    // OFFSET 0x021D11FC
    private static void TitleScreen_Free3D(TitleScreenData data)
    {
        G3DPipeline.G3DPipelineBuffers_Free(data.Pipeline!);
    }

    // OFFSET 0x021D1208
    private static void TitleScreen_LoadModels(TitleScreenModels models, int modelMember, int animMember, int heapID)
    {
        Heap.HeapExp_FndInitAllocator(models.Allocator, heapID, 4);
        models.Resource = NARC.NARC_AllocAndReadWholeMemberByIndexPair(TitleArchive, modelMember, heapID);
        models.AnimData1 = NARC.NARC_AllocAndReadWholeMemberByIndexPair(TitleArchive, animMember, heapID);
        models.AnimData2 = NARC.NARC_AllocAndReadWholeMemberByIndexPair(TitleArchive, 2, heapID);
        Easy3D.Easy3D_InitRenderObjFromResource(models.RenderObj, out models.Model, models.Resource);
        G3dAnim anim1 = G3d.GetAnimByIndex(models.AnimData1, 0)!;
        G3dAnim anim2 = G3d.GetAnimByIndex(models.AnimData2, 0)!;
        models.AnimObj1 = G3d.AllocAnimObj(models.AnimAllocator, anim1, models.Model!);
        models.AnimObj2 = G3d.AllocAnimObj(models.AnimAllocator, anim2, models.Model!);
        G3dTexture? texture = G3d.GetTexture(models.Resource);
        G3d.InitAnimObj(models.AnimObj1, anim1, models.Model!, texture);
        G3d.InitAnimObj(models.AnimObj2, anim2, models.Model!, texture);
        G3d.AddAnimObj(models.RenderObj, models.AnimObj1);
        G3d.AddAnimObj(models.RenderObj, models.AnimObj2);
        models.Position = new VecFx32 { X = 0, Y = 0, Z = 0 };
        models.Scale = VectorAt(ScaleAddress);
        models.Rotation = new VecFx32 { X = 0, Y = 0, Z = 0 };
        models.AnimMode = 0;
        TitleScreen_LoadIntroModels(models, heapID);
    }

    // OFFSET 0x021D1300
    private static void TitleScreen_LoadIntroModels(TitleScreenModels models, int heapID)
    {
        NARC narc = NARC.NARC_ctor(TitleArchive, heapID);
        Easy3DObject.Easy3DModel_LoadFrom(models.IntroModel, narc, 0x13, heapID);
        G3d.SetMaterialPolygonAttrMask(models.IntroModel.Model!, true, 0x1F0000);
        G3d.SetMaterialPolygonAttrMask(models.IntroModel.Model!, true, 0x3F000000);
        Easy3DObject.Easy3DAnim_LoadFrom(models.IntroAnim1, models.IntroModel, narc, 0x15, heapID, models.AnimAllocator);
        Easy3DObject.Easy3DAnim_SetFrame(models.IntroAnim1, 0);
        Easy3DObject.Easy3DAnim_LoadFrom(models.IntroAnim2, models.IntroModel, narc, 0x14, heapID, models.AnimAllocator);
        Easy3DObject.Easy3DAnim_SetFrame(models.IntroAnim2, 0);
        Easy3DObject.Easy3DObject_Init(models.IntroObject, models.IntroModel);
        Easy3DObject.Easy3DObject_SetPosition(models.IntroObject, 0, 0, 0);
        Easy3DObject.Easy3DObject_SetScale(models.IntroObject, Fx32.One, Fx32.One, Fx32.One);
        Easy3DObject.Easy3DObject_SetVisible(models.IntroObject, 1);
        Easy3DObject.Easy3DObject_AddAnim(models.IntroObject, models.IntroAnim1);
        Easy3DObject.Easy3DObject_AddAnim(models.IntroObject, models.IntroAnim2);
        Easy3DObject.Easy3DModel_LoadFrom(models.LoopModel, narc, 0x10, heapID);
        G3d.SetMaterialPolygonAttrMask(models.LoopModel.Model!, true, 0x1F0000);
        G3d.SetMaterialPolygonAttrMask(models.LoopModel.Model!, true, 0x3F000000);
        Easy3DObject.Easy3DAnim_LoadFrom(models.LoopAnim1, models.LoopModel, narc, 0x12, heapID, models.AnimAllocator);
        Easy3DObject.Easy3DAnim_SetFrame(models.LoopAnim1, 0);
        Easy3DObject.Easy3DAnim_LoadFrom(models.LoopAnim2, models.LoopModel, narc, 0x11, heapID, models.AnimAllocator);
        Easy3DObject.Easy3DAnim_SetFrame(models.LoopAnim2, 0);
        Easy3DObject.Easy3DObject_Init(models.LoopObject, models.LoopModel);
        Easy3DObject.Easy3DObject_SetPosition(models.LoopObject, 0, 0, 0);
        Easy3DObject.Easy3DObject_SetScale(models.LoopObject, Fx32.One, Fx32.One, Fx32.One);
        Easy3DObject.Easy3DObject_SetVisible(models.LoopObject, 1);
        Easy3DObject.Easy3DObject_AddAnim(models.LoopObject, models.LoopAnim1);
        Easy3DObject.Easy3DObject_AddAnim(models.LoopObject, models.LoopAnim2);
        NARC.NARC_dtor(narc);
        models.AngleSpeed = -0x12E;
        models.Angle = 0xE383;
        models.Reserved = 0x1000;
        models.FovSpeed = 0x7800;
    }

    // OFFSET 0x021D14E4
    private static void TitleScreen_FreeModels(TitleScreenModels models)
    {
        TitleScreen_FreeIntroModels(models);
        G3d.FreeAnimObj(models.AnimAllocator, models.AnimObj1!);
        G3d.FreeAnimObj(models.AnimAllocator, models.AnimObj2!);
        Heap.Heap_Free(models.AnimData1!);
        Heap.Heap_Free(models.AnimData2!);
        Heap.Heap_Free(models.Resource!);
    }

    // OFFSET 0x021D1514
    private static void TitleScreen_FreeIntroModels(TitleScreenModels models)
    {
        Easy3DObject.Easy3DModel_Release(models.IntroModel);
        Easy3DObject.Easy3DAnim_Release(models.IntroAnim1, models.AnimAllocator);
        Easy3DObject.Easy3DAnim_Release(models.IntroAnim2, models.AnimAllocator);
        Easy3DObject.Easy3DModel_Release(models.LoopModel);
        Easy3DObject.Easy3DAnim_Release(models.LoopAnim1, models.AnimAllocator);
        Easy3DObject.Easy3DAnim_Release(models.LoopAnim2, models.AnimAllocator);
    }

    // OFFSET 0x021D1568
    private static void TitleScreen_Draw(TitleScreenGraphics graphics, TitleScreenModels models)
    {
        if (graphics.IntroFinished == 0 && graphics.IntroPlaying == 1)
        {
            TitleScreen_Nop(models.IntroCamera!);
            Camera.Camera_ComputeProjectionMatrix(0, models.IntroCamera!);
            Camera.Camera_SetAsActive(models.IntroCamera!);
        }
        else
        {
            Camera.Camera_ComputeProjectionMatrix(0, models.MainCamera!);
            Camera.Camera_SetAsActive(models.MainCamera!);
        }
        graphics.BobAngle += 2;
        graphics.BobAngle %= 360;
        int sine = MathUtil.CalcSineDegrees_Wraparound((ushort)(graphics.BobAngle * 0xFFFF / 360));
        models.Position.Y -= (int)(sine * 0.3);
        switch (models.State)
        {
            case 0:
                return;
            case 1:
                G3SwapRequest.G3_ResetG3X();
                G3SwapRequest.G3_RequestSwapBuffers(1, 1);
                models.State = 0;
                return;
            case 2:
                break;
            default:
                return;
        }
        G3SwapRequest.G3_ResetG3X();
        Camera.Camera_ComputeViewMatrix();
        FxUtil.MTX_Rot33Vec(out MtxFx33 rotation, models.Rotation);
        if (graphics.IntroFinished == 0)
        {
            if (graphics.IntroPlaying == 1)
            {
                TitleScreen_DrawIntroModels(models);
            }
        }
        else
        {
            Easy3D.Easy3D_DrawRenderObj(models.RenderObj, models.Position, rotation, models.Scale);
        }
        switch (models.AnimMode)
        {
            case 0:
                models.AnimObj1!.Frame = 0;
                models.AnimObj2!.Frame = 0;
                break;
            case 1:
                if (models.AnimObj1!.Frame == 0)
                {
                    models.AnimMode = 0;
                    break;
                }
                goto case 2;
            case 2:
                models.AnimObj1!.Frame += Fx32.One;
                models.AnimObj2!.Frame += Fx32.One;
                if (models.AnimObj1.Frame == models.AnimObj1.ResAnm!.NumFrames << Fx32.Shift)
                {
                    models.AnimObj1.Frame = 0;
                }
                if (models.AnimObj2.Frame == models.AnimObj2.ResAnm!.NumFrames << Fx32.Shift)
                {
                    models.AnimObj2.Frame = 0;
                }
                break;
        }
        G3SwapRequest.G3_RequestSwapBuffers(1, 1);
    }

    // OFFSET 0x021D1704
    private static void TitleScreen_DrawIntroModels(TitleScreenModels models)
    {
        if (models.IntroAnim1State == 1)
        {
            if (Easy3DObject.Easy3DAnim_Update(models.IntroAnim1, Fx32.One))
            {
                models.IntroAnim1State = 2;
            }
        }
        if (models.IntroAnim2State == 1)
        {
            if (Easy3DObject.Easy3DAnim_Update(models.IntroAnim2, Fx32.One))
            {
                models.IntroAnim2State = 2;
            }
        }
        Easy3DObject.Easy3DAnim_UpdateLooped(models.LoopAnim1, Fx32.One);
        Easy3DObject.Easy3DAnim_UpdateLooped(models.LoopAnim2, Fx32.One);
        Render3D.PushMatrix();
        Easy3DObject.Easy3DObject_Draw(models.LoopObject);
        if (models.IntroAnim1State == 2 && models.IntroAnim2State == 2)
        {
            Easy3DObject.Easy3DObject_SetVisible(models.IntroObject, 0);
        }
        else
        {
            Easy3DObject.Easy3DObject_Draw(models.IntroObject);
        }
        Render3D.PopMatrix(1);
    }

    // OFFSET 0x021D17B4
    private static void TitleScreen_InitBgs(TitleScreenData data)
    {
        data.BgConfig = Bg.BgConfig_New(data.HeapID);
        Bg.SetAllGraphicsModes(GameOpening.GraphicsModesAt(GraphicsModesAddress));
        Bg.Bg_InitFromTemplate(data.BgConfig, 4, GameOpening.BgTemplateAt(0x021D7354), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 5, GameOpening.BgTemplateAt(0x021D7370), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 6, GameOpening.BgTemplateAt(0x021D738C), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 1, GameOpening.BgTemplateAt(0x021D73A8), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 3, GameOpening.BgTemplateAt(0x021D7300), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 7, GameOpening.BgTemplateAt(0x021D731C), 0);
        GXLayers.GXLayers_EngineAToggleLayers(1, 0);
        GXLayers.GXLayers_EngineAToggleLayers(2, 0);
        GXLayers.GXLayers_EngineAToggleLayers(4, 0);
        GXLayers.GXLayers_EngineAToggleLayers(8, 0);
        GXLayers.GXLayers_EngineBToggleLayers(1, 0);
        GXLayers.GXLayers_EngineBToggleLayers(2, 0);
        GXLayers.GXLayers_EngineBToggleLayers(4, 0);
        GXLayers.GXLayers_EngineBToggleLayers(8, 0);
        Bg.Bg_MaskPalette(0, 0);
        Bg.Bg_MaskPalette(4, 0);
    }

    // OFFSET 0x021D1908
    private static void TitleScreen_FreeBgs(TitleScreenData data)
    {
        GXLayers.GXLayers_EngineAToggleLayers(1, 0);
        GXLayers.GXLayers_EngineAToggleLayers(2, 0);
        GXLayers.GXLayers_EngineAToggleLayers(4, 0);
        GXLayers.GXLayers_EngineAToggleLayers(8, 0);
        GXLayers.GXLayers_EngineBToggleLayers(1, 0);
        GXLayers.GXLayers_EngineBToggleLayers(2, 0);
        GXLayers.GXLayers_EngineBToggleLayers(4, 0);
        GXLayers.GXLayers_EngineBToggleLayers(8, 0);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 4);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 5);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 6);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 1);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 3);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 7);
        data.BgConfig = null;
    }

    // OFFSET 0x021D1984
    private static void TitleScreen_UpdateIntroCamera(TitleScreenGraphics graphics, TitleScreenModels models)
    {
        VecFx32 delta = new() { X = 0, Y = 0, Z = 0 };
        if (graphics.IntroFinished == 1)
        {
            return;
        }
        if ((uint)models.IntroFrame < 0x3C)
        {
            delta.Z = -0xA00;
            Camera.Camera_Move(delta, models.IntroCamera!);
        }
        if (models.IntroFrame == 0x4B)
        {
            models.IntroAnim1State = 1;
            models.IntroAnim2State = 1;
        }
        if ((uint)models.IntroFrame >= 0xFA)
        {
            CameraAngle angle = Camera.Camera_GetAngle(models.IntroCamera!);
            angle.X = (ushort)models.Angle;
            Camera.Camera_SetAngleAroundTarget(angle, models.IntroCamera!);
            models.Angle += models.AngleSpeed;
            if (models.Angle < 0xC011)
            {
                models.Angle = 0xC011;
                Camera.Camera_AdjustFOV((ushort)(-(models.FovSpeed >> 8)), models.IntroCamera!);
                models.FovSpeed -= 0x280;
                if (models.FovSpeed < 0x1000)
                {
                    models.FovSpeed = 0x1000;
                }
            }
        }
        models.IntroFrame++;
    }

    // OFFSET 0x021D1A60
    private static bool TitleScreen_InitGraphics(TitleScreenGraphics graphics, BgConfig bgConfig, int heapID)
    {
        TitleScreenModels models = graphics.Models;
        TitleScreen_InitCameraPath(graphics);
        TitleScreen_LoadBgs(bgConfig, heapID, graphics);
        TitleScreen_LoadModels(models, 1, 0, heapID);
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DAntiAlias);
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DAlphaBlend);
        graphics.Target = graphics.TargetStart;
        graphics.Position = graphics.PositionStart;
        models.MainCamera = Camera.Camera_Alloc(heapID);
        Camera.Camera_InitWithTargetAndPosition(() => graphics.Target, graphics.Position, 0xB60, 0, false, models.MainCamera);
        Camera.Camera_SetClipping(0, 0x12C000, models.MainCamera);
        Camera.Camera_ComputeProjectionMatrix(0, models.MainCamera);
        Camera.Camera_SetAsActive(models.MainCamera);
        VecFx32 origin = new() { X = 0, Y = 0, Z = 0 };
        models.IntroCamera = Camera.Camera_Alloc(heapID);
        ushort[] rawAngle = RomData.OverlayU16(Overlay, IntroCameraAngleAddress, 4);
        CameraAngle angle = new() { X = rawAngle[0], Y = rawAngle[1], Z = rawAngle[2], Padding = rawAngle[3] };
        Camera.Camera_InitWithTarget(() => origin, 0xA0000, angle, 0xFA4, 0, false, models.IntroCamera);
        Camera.Camera_SetClipping(0, 0x12C000, models.IntroCamera);
        Camera.Camera_Move(VectorAt(IntroCameraOffsetAddress), models.IntroCamera);
        Camera.Camera_SetAsActive(models.IntroCamera);
        Render3D.SetGlobalLightVector(0, graphics.Light0.X, graphics.Light0.Y, graphics.Light0.Z);
        Render3D.SetGlobalLightColor(0, White);
        Render3D.SetGlobalLightVector(1, graphics.Light1.X, graphics.Light1.Y, graphics.Light1.Z);
        Render3D.SetGlobalLightColor(1, White);
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DAntiAlias);
        gSystem.WhichScreenIs3D = 1;
        GXLayers.GXLayers_SwapDisplay();
        models.State = 2;
        return true;
    }

    // OFFSET 0x021D1C10
    private static void TitleScreen_PanCamera(TitleScreenGraphics graphics)
    {
        graphics.Target.X += (graphics.TargetEnd.X - graphics.TargetStart.X) / PanDuration;
        graphics.Target.Y += (graphics.TargetEnd.Y - graphics.TargetStart.Y) / PanDuration;
        graphics.Target.Z += (graphics.TargetEnd.Z - graphics.TargetStart.Z) / PanDuration;
        graphics.Position.X += (graphics.PositionEnd.X - graphics.PositionStart.X) / PanDuration;
        graphics.Position.Y += (graphics.PositionEnd.Y - graphics.PositionStart.Y) / PanDuration;
        graphics.Position.Z += (graphics.PositionEnd.Z - graphics.PositionStart.Z) / PanDuration;
    }

    // OFFSET 0x021D1CC0
    private static void TitleScreen_ShowLogoBg(BgConfig bgConfig, int heapID)
    {
        Bg.Bg_FreeTilemapBuffer(bgConfig, 5);
        Bg.Bg_InitFromTemplate(bgConfig, 5, GameOpening.BgTemplateAt(0x021D73C4), 0);
        Graphics.Graphics_LoadTilesToBgLayer(TitleArchive, 0x17, bgConfig, 5, 0, 0, 0, heapID);
        Graphics.Graphics_LoadTilemapToBgLayer(TitleArchive, 0x19, bgConfig, 5, 0, 0, 0, heapID);
        Video.SetBlendControl(0, 0);
        Video.SetBlendAlpha(1, 2, 8, 0xA, 0xA);
        GXLayers.GXLayers_EngineBToggleLayers(2, 1);
    }

    // OFFSET 0x021D1D48
    private static void TitleScreen_ShowPressedBg(BgConfig bgConfig, int heapID)
    {
        Bg.Bg_FreeTilemapBuffer(bgConfig, 4);
        Bg.Bg_InitFromTemplate(bgConfig, 4, GameOpening.BgTemplateAt(0x021D7338), 0);
        Graphics.Graphics_LoadTilemapToBgLayer(TitleArchive, 0x16, bgConfig, 4, 0, 0, 0, heapID);
        Video.SetBgControl(4, (ushort)(Video.GetBgControl(4) & ~3));
        Video.SetBgControl(5, (ushort)((Video.GetBgControl(5) & ~3) | 1));
        Video.SetBgControl(6, (ushort)(Video.GetBgControl(6) & ~3));
        Video.SetBgControl(7, (ushort)((Video.GetBgControl(7) & ~3) | 2));
        Bg.Bg_SetOffset(bgConfig, 6, OffsetSetX, 0);
        Bg.Bg_SetOffset(bgConfig, 6, OffsetSetY, 1);
        Video.SetBlendAlpha(1, 3, 0xC, 0x1A, 0xA);
        GXLayers.GXLayers_EngineBToggleLayers(1, 1);
    }

    // OFFSET 0x021D1DF0
    private static bool TitleScreen_RunIntro(TitleScreenGraphics graphics, BgConfig bgConfig, int heapID)
    {
        bool finished = false;
        if (TitleScreen_SkipPressed())
        {
            graphics.IntroFinished = 1;
            graphics.IntroPlaying = 0;
            ScreenFadeControl.FinishScreenFade();
            BrightnessController.BrightnessController_ResetScreenController(3);
            return true;
        }
        switch (graphics.State)
        {
            case 0:
                GXLayers.GXLayers_EngineAToggleLayers(1, 1);
                graphics.LightState = 0;
                graphics.Timer = 0x10B;
                ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 0xF, 3, heapID);
                graphics.State = 1;
                break;
            case 1:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    if (graphics.Timer != 0)
                    {
                        graphics.Timer--;
                    }
                    else
                    {
                        graphics.Timer = 2;
                        graphics.State = 2;
                    }
                }
                break;
            case 2:
                if (BrightnessController.BrightnessController_IsTransitionComplete(1) && BrightnessController.BrightnessController_IsTransitionComplete(2))
                {
                    if (graphics.Timer != 0)
                    {
                        BrightnessController.BrightnessController_StartTransition(10, 0x10, 0, 7, 1);
                        BrightnessController.BrightnessController_StartTransition(10, 0x10, 0, 0x27, 2);
                        graphics.LightState = 2;
                        graphics.Timer--;
                        graphics.State = 3;
                    }
                    else
                    {
                        graphics.Timer = 0;
                        graphics.State = 4;
                    }
                }
                break;
            case 3:
                if (BrightnessController.BrightnessController_IsTransitionComplete(1) && BrightnessController.BrightnessController_IsTransitionComplete(2))
                {
                    BrightnessController.BrightnessController_StartTransition(10, 0, 0x10, 7, 1);
                    BrightnessController.BrightnessController_StartTransition(10, 0, 0x10, 0x27, 2);
                    graphics.LightState = 3;
                    graphics.State = 2;
                }
                break;
            case 4:
                if (graphics.Timer != 0)
                {
                    graphics.Timer--;
                }
                else
                {
                    graphics.Timer = 1;
                    graphics.State = 5;
                }
                break;
            case 5:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    if (graphics.Timer != 0)
                    {
                        ScreenFadeControl.StartScreenFade(0, 0, 0, White, 5, 2, heapID);
                        graphics.LightState = 2;
                        graphics.Timer--;
                        graphics.State = 6;
                    }
                    else
                    {
                        graphics.Timer = 0xA;
                        graphics.State = 8;
                        graphics.IntroFinished = 1;
                        ScreenFadeControl.SetScreenColorBrightness(0, 0);
                    }
                }
                break;
            case 6:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    graphics.IntroPlaying = 0;
                    GXLayers.GXLayers_EngineBToggleLayers(2, 1);
                    graphics.Models.AnimMode = 2;
                    ScreenFadeControl.StartScreenFade(3, 1, 1, White, 0x10, 3, heapID);
                    graphics.State = 5;
                }
                break;
            case 7:
                ScreenFadeControl.StartScreenFade(3, 1, 1, 0, 0x30, 1, heapID);
                GXLayers.GXLayers_EngineAToggleLayers(8, 1);
                graphics.State = 9;
                graphics.PanFrames = 0;
                break;
            case 8:
                if (graphics.Timer != 0)
                {
                    graphics.Timer--;
                }
                else
                {
                    graphics.State = 7;
                }
                break;
            case 9:
                TitleScreen_PanCamera(graphics);
                Camera.Camera_SetTarget(graphics.Target, graphics.Models.MainCamera!);
                Camera.Camera_SetPosition(graphics.Position, graphics.Models.MainCamera!);
                graphics.PanFrames++;
                if (graphics.PanFrames >= PanDuration)
                {
                    GXLayers.GXLayers_EngineBToggleLayers(4, 1);
                    TitleScreen_ShowLogoBg(bgConfig, heapID);
                    ScreenFadeControl.StartScreenFade(4, 1, 1, White, 0x10, 3, heapID);
                    GXLayers.GXLayers_EngineAToggleLayers(2, 1);
                    GXLayers.GXLayers_EngineBToggleLayers(8, 1);
                    graphics.Timer = 0x5A;
                    graphics.State = 10;
                }
                break;
            case 10:
                if (graphics.Timer != 0)
                {
                    graphics.Timer--;
                }
                else if (ScreenFadeControl.IsScreenFadeDone())
                {
                    finished = true;
                }
                break;
        }
        TitleScreen_UpdateLight(graphics);
        TitleScreen_UpdateIntroCamera(graphics, graphics.Models);
        TitleScreen_Draw(graphics, graphics.Models);
        return finished;
    }

    // OFFSET 0x021D20E4
    private static bool TitleScreen_RunLoop(TitleScreenGraphics graphics, BgConfig bgConfig, int heapID)
    {
        switch (graphics.State)
        {
            case 0:
                Camera.Camera_SetTarget(graphics.TargetEnd, graphics.Models.MainCamera!);
                Camera.Camera_SetPosition(graphics.PositionEnd, graphics.Models.MainCamera!);
                GXLayers.GXLayers_EngineAToggleLayers(1, 1);
                GXLayers.GXLayers_EngineAToggleLayers(8, 1);
                GXLayers.GXLayers_EngineBToggleLayers(8, 1);
                GXLayers.GXLayers_EngineBToggleLayers(4, 1);
                GXLayers.GXLayers_EngineAToggleLayers(2, 1);
                ScreenFadeControl.ResetScreenMasterBrightness(0);
                ScreenFadeControl.ResetScreenMasterBrightness(1);
                graphics.Models.AnimMode = 2;
                Render3D.SetGlobalLightColor(1, White);
                TitleScreen_ShowLogoBg(bgConfig, heapID);
                graphics.Timer = 0;
                graphics.State = 1;
                break;
            case 1:
                if (graphics.ShowPressStart == 1)
                {
                    if (graphics.Timer == 0)
                    {
                        GXLayers.GXLayers_EngineBToggleLayers(1, 1);
                    }
                    else if (graphics.Timer == 0x10)
                    {
                        GXLayers.GXLayers_EngineBToggleLayers(1, 0);
                    }
                }
                graphics.Timer++;
                graphics.Timer &= 0x1F;
                break;
        }
        TitleScreen_Draw(graphics, graphics.Models);
        return true;
    }

    // OFFSET 0x021D21C0
    private static bool TitleScreen_FreeGraphics(TitleScreenGraphics graphics, BgConfig bgConfig, int heapID)
    {
        Camera.Camera_Delete(graphics.Models.MainCamera!);
        Camera.Camera_Delete(graphics.Models.IntroCamera!);
        TitleScreen_FreeModels(graphics.Models);
        TitleScreen_FreeWindow(bgConfig, heapID, graphics);
        Video.SetBlendControl(0, 0);
        Render3D.Display3DControl &= Disp3DEdgeMarkingMask;
        gSystem.WhichScreenIs3D = 0;
        GXLayers.GXLayers_SwapDisplay();
        return true;
    }

    // OFFSET 0x021D2214
    private static void TitleScreen_LoadBgs(BgConfig bgConfig, int heapID, TitleScreenGraphics graphics)
    {
        Graphics.Graphics_LoadTilesToBgLayer(TitleArchive, 0x17, bgConfig, 7, 0, 0, 0, heapID);
        Graphics.Graphics_LoadTilesToBgLayer(TitleArchive, 0x1A, bgConfig, 3, 0, 0, 0, heapID);
        Graphics.Graphics_LoadTilemapToBgLayer(TitleArchive, 0x18, bgConfig, 7, 0, 0, 0, heapID);
        Graphics.Graphics_LoadTilemapToBgLayer(TitleArchive, 0x1B, bgConfig, 3, 0, 0, 0, heapID);
        Graphics.Graphics_LoadPalette(TitleArchive, 7, PaletteSubBg, 0, 0, heapID);
        Graphics.Graphics_LoadPalette(TitleArchive, 6, PaletteMainBg, 0, 0, heapID);
        Graphics.Graphics_LoadTilesToBgLayer(TitleArchive, 0xC, bgConfig, 6, 0, 0, 0, heapID);
        Graphics.Graphics_LoadPalette(TitleArchive, 0xB, PaletteSubBgExt, 0x4000, 0, heapID);
        Graphics.Graphics_LoadTilemapToBgLayer(TitleArchive, 0x16, bgConfig, 6, 0, 0, 0, heapID);
        Graphics.Graphics_LoadTilesToBgLayer(OpeningArchive, 0xE, bgConfig, 5, 0, 0, 0, heapID);
        Graphics.Graphics_LoadPalette(OpeningArchive, 0xD, PaletteSubBgExt, 0x2000, 0, heapID);
        Graphics.Graphics_LoadTilemapToBgLayer(OpeningArchive, 0xC, bgConfig, 5, 0, 0, 0, heapID);
        Graphics.Graphics_LoadTilesToBgLayer(TitleArchive, 5, bgConfig, 1, 0, 0, 0, heapID);
        Graphics.Graphics_LoadTilemapToBgLayer(TitleArchive, 3, bgConfig, 1, 0, 0, 0, heapID);
        Graphics.Graphics_LoadPalette(TitleArchive, 4, PaletteMainBg, 0x20, 0x60, heapID);
        Bg.Bg_MaskPalette(0, 0);
        Bg.Bg_MaskPalette(4, 0);
        Bg.Bg_ClearTilesRange(4, 0x20, 0, heapID);
        MessageLoader loader = MessageLoader.MessageLoader_Init(1, 0x1A, 0x261, heapID)!;
        GameString text = GameString.String_Init(0x40, heapID);
        byte[] rawTemplate = RomData.Overlay(Overlay, WindowTemplateAddress, 8);
        WindowTemplate template = new()
        {
            BgLayer = rawTemplate[0],
            TilemapLeft = rawTemplate[1],
            TilemapTop = rawTemplate[2],
            Width = rawTemplate[3],
            Height = rawTemplate[4],
            Palette = rawTemplate[5],
            BaseTile = (ushort)Bin.U16(rawTemplate, 6),
        };
        Bg.Window_AddFromTemplate(bgConfig, graphics.Window, template);
        Bg.Window_FillRectWithColor(graphics.Window, 0, 0, 0, 0xE0, 0x10);
        MessageLoader.MessageLoader_GetString(loader, 0, text);
        int x = Font.Font_CalcCenterAlignment(0, text, 1, graphics.Window.Width << 3);
        Text.Text_AddPrinterWithParamsColorAndSpacing(graphics.Window, 0, text, x, 0, 0, 0x00010100, 1, 0, null);
        GameString.String_Free(text);
        MessageLoader.MessageLoader_Free(loader);
        ushort[] colors = { 0x15, 0x15 };
        Bg.Bg_LoadPalette(4, colors, 1, 2, 0x42);
        Bg.Bg_LoadPalette(4, colors, 0, 2, 0x44);
    }

    // OFFSET 0x021D2428
    private static void TitleScreen_FreeWindow(BgConfig bgConfig, int heapID, TitleScreenGraphics graphics)
    {
        Bg.Window_Remove(graphics.Window);
    }

    // OFFSET 0x021D2438
    private static void TitleScreen_UpdateLight(TitleScreenGraphics graphics)
    {
        switch (graphics.LightState)
        {
            case 0:
                graphics.LightLevel = 5;
                graphics.LightState = 1;
                break;
            case 2:
                graphics.LightLevel += 2;
                if (graphics.LightLevel >= 0x1F)
                {
                    graphics.LightLevel = 0x1F;
                    graphics.LightState = 1;
                }
                break;
            case 3:
                graphics.LightLevel -= 2;
                if (graphics.LightLevel <= 5)
                {
                    graphics.LightLevel = 5;
                    graphics.LightState = 1;
                }
                break;
        }
        int level = graphics.LightLevel;
        Render3D.SetGlobalLightColor(1, (ushort)((level & 0x1F) | ((level << 5) & 0x3E0) | ((level << 10) & 0x7C00)));
    }

    // OFFSET 0x021D24C8
    private static void TitleScreen_InitCameraPath(TitleScreenGraphics graphics)
    {
        graphics.PositionStart = new VecFx32 { X = 0, Y = 0xC0000, Z = 0x258000 };
        graphics.PositionEnd = new VecFx32 { X = -0x40000, Y = 0xC0000, Z = 0x1E4000 };
        graphics.TargetStart = new VecFx32 { X = 0, Y = 0x64000, Z = -0x12000 };
        graphics.TargetEnd = new VecFx32 { X = 0, Y = 0x64000, Z = -0x12000 };
        graphics.Light0 = new VecFx16 { X = 0x8DB, Y = unchecked((short)0xF85F), Z = unchecked((short)0xF513) };
        graphics.Light1 = new VecFx16 { X = unchecked((short)0xF725), Y = 0x7A1, Z = unchecked((short)0xF513) };
        graphics.Reserved = 0x3000;
        long x = graphics.TargetEnd.X - graphics.PositionEnd.X;
        long y = graphics.TargetEnd.Y - graphics.PositionEnd.Y;
        long z = graphics.TargetEnd.Z - graphics.PositionEnd.Z;
        double length = Math.Sqrt((double)(x * x + y * y + z * z));
        if (length != 0)
        {
            graphics.Light0 = new VecFx16
            {
                X = (short)(x * Fx32.One / length),
                Y = (short)(y * Fx32.One / length),
                Z = (short)(z * Fx32.One / length),
            };
        }
    }

    // OFFSET 0x021D25AC
    private static void TitleScreen_Nop(Camera camera)
    {
    }
}
