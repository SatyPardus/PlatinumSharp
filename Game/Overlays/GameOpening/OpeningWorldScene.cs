using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed class OpeningWorldSceneData
{
    public const int ModelCapacity = 6;

    public byte State;
    public byte SubState;
    public byte Initialized;
    public byte BgsLoaded;
    public byte ModelsLoaded;
    public byte Loading;
    public byte ModelsActive;
    public byte SpawnTimer;
    public byte SpawnInterval;
    public byte LoadStep;
    public int Reserved;
    public BgConfig? BgConfig;
    public OpeningSprites? Sprites;
    public OpeningParticleSet? ParticleSet;
    public OpeningStarters? Starters;
    public byte[]? TextureResource;
    public G3dTexture? Texture;
    public Camera? Camera;
    public readonly G3dRenderObj[] RenderObjs = new G3dRenderObj[ModelCapacity];
    public readonly byte[]?[] Resources = new byte[]?[ModelCapacity];
    public int ModelCount;
    public ushort[]? PaletteBuffer;
    public byte HideParticles;
    public byte EnableWindowRequest;
    public byte DisableWindowRequest;
    public byte LayersOnMask;
    public byte LayersOffMask;
    public short MainLeft;
    public short MainRight;
    public short MainTop;
    public short MainBottom;
    public short SubLeft;
    public short SubRight;
    public short SubTop;
    public short SubBottom;
    public byte BgMode;
    public byte SwapRequest;
    public byte DrawModels;
    public byte DrawParticles;
    public int ScrollFast;
    public int ScrollSlow;

    public OpeningWorldSceneData()
    {
        for (int i = 0; i < ModelCapacity; i++)
        {
            RenderObjs[i] = new G3dRenderObj();
        }
    }
}

public sealed class OpeningLegendSceneData
{
    public byte State;
    public byte SubState;
    public byte Initialized;
    public byte BgsLoaded;
    public int Scroll;
    public BgConfig? BgConfig;
}

public static class OpeningWorldScene
{
    private const int HeapID = 0x4C;
    private const int Overlay = 77;
    private const int GraphicsArchive = 0x80;
    private const int LogoArchive = 0x41;
    private const uint WorldBanks = 0x021D771C;
    private const uint LegendBanks = 0x021D7744;
    private const uint WorldGraphicsModes = 0x021D74AC;
    private const uint CloudGraphicsModes = 0x021D74DC;
    private const uint LegendGraphicsModes = 0x021D74BC;
    private const uint EdgeColorTable = 0x021D74EC;
    private const uint LightVectors = 0x021D776C;
    private const uint LightColors = 0x021D7444;
    private const uint TextureMembers = 0x021D7458;
    private const uint ModelMembers = 0x021D779C;
    private const uint CameraAngleAddress = 0x021D7500;
    private const uint CameraTurnAddress = 0x021D743C;
    private const uint CameraTargetStart = 0x021D7494;
    private const uint CameraOffsetStart = 0x021D7464;
    private const uint CameraTargetLake = 0x021D7470;
    private const uint CameraOffsetLake = 0x021D74A0;
    private const uint CameraTargetCity = 0x021D744C;
    private const uint CameraOffsetCity = 0x021D747C;
    private const uint ModelScaleAddress = 0x021D7488;
    private const uint ModelRotationAddress = 0x021D76D0;
    private const int CameraDistance = 0x29AEC1;
    private const ushort CameraFov = 0x5C1;
    private const ushort CameraStartFov = 0x981;
    private const ushort White = 0x7FFF;
    private const int PaletteMainBg = 0;
    private const int PaletteSubBg = 4;
    private const int OffsetSetX = 0;
    private const int OffsetAddX = 1;
    private const int OffsetSetY = 3;
    private const int OffsetAddY = 4;
    private const uint WindowEnableMask = 0xFFFF1FFF;
    private const uint Window0Enable = 0x2000;
    private const uint Window01Enable = 0x6000;
    private const ushort Disp3DToonMask = 0xCFFD;
    private const ushort Disp3DMask = 0xCFFF;
    private const ushort Disp3DAntiAlias = 0x10;
    private const ushort Disp3DAlphaTestMask = 0xCFFB;
    private const ushort Disp3DAlphaBlend = 8;
    private const ushort Disp3DEdgeMarking = 0x20;
    private const int ModelFlagDiffuse = 0x40;
    private const int ModelFlagAmbient = 0x80;
    private const int ModelFlagSpecular = 0x200;
    private const int ModelFlagEmission = 0x400;
    private const int FrameLogoBg = 0x280;
    private const int FrameLogoParticles = 0x2BC;
    private const int FrameLogoFlash = 0x311;
    private const int FrameLogoSwap = 0x316;
    private const int FrameWorldStart = 0x3B0;
    private const int FrameWorldObjects = 0x3B1;
    private const int FrameWorldFadeIn = 0x3CF;
    private const int FrameWorldFadeOut = 0x628;
    private const int FrameCloudsStart = 0x640;
    private const int FrameCloudsEnd = 0x726;
    private const int FrameRingFade = 0x735;
    private const int FrameRingShow = 0x744;
    private const int FrameRingHold = 0x3C;
    private const int FrameStartersFade = 0x7DA;
    private const int FrameStartersSecond = 0x852;
    private const int FrameFinalSlide = 0x8A7;
    private const int FrameFinalFade = 0x8A8;
    private const int FrameCameraLake = 0x4A7;
    private const int FrameCameraCitySpan = 0xD2;
    private const int FrameCameraZoomOut = 0x618;
    private const int FrameLoadLake = 0x4A1;
    private const int FrameFadeCity = 0x573;
    private const int FrameLoadCity = 0x579;
    private const int FrameLegendFadeIn = 0x89D;
    private const int FrameLegendFadeOut = 0x974;
    private const int CloudSpeed = 0x10000;

    private static VecFx32 VectorAt(uint address)
    {
        uint[] raw = RomData.OverlayU32(Overlay, address, 3);
        return new VecFx32 { X = (int)raw[0], Y = (int)raw[1], Z = (int)raw[2] };
    }

    private static CameraAngle AngleAt(uint address)
    {
        ushort[] raw = RomData.OverlayU16(Overlay, address, 4);
        return new CameraAngle { X = raw[0], Y = raw[1], Z = raw[2], Padding = raw[3] };
    }

    // OFFSET 0x021D25D0
    private static void OpeningWorldScene_SetBanks()
    {
        GXLayers.GXLayers_SetBanks(GameOpening.BanksAt(WorldBanks));
    }

    // OFFSET 0x021D25F0
    private static void OpeningLegendScene_SetBanks()
    {
        GXLayers.GXLayers_SetBanks(GameOpening.BanksAt(LegendBanks));
    }

    // OFFSET 0x021D2724
    private static void OpeningWorldScene_InitBgs(OpeningData data)
    {
        data.BgConfig = Bg.BgConfig_New(data.HeapID);
        Bg.SetAllGraphicsModes(GameOpening.GraphicsModesAt(WorldGraphicsModes));
        Bg.Bg_InitFromTemplate(data.BgConfig, 1, GameOpening.BgTemplateAt(0x021D75B8), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 2, GameOpening.BgTemplateAt(0x021D760C), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 3, GameOpening.BgTemplateAt(0x021D7628), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 4, GameOpening.BgTemplateAt(0x021D7644), 0);
        BgTemplate[] subTemplates = GameOpening.BgTemplatesAt(0x021D7838, 3);
        Bg.Bg_InitFromTemplate(data.BgConfig, 5, subTemplates[0], 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 6, subTemplates[1], 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 7, subTemplates[2], 0);
        Bg.Bg_MaskPalette(1, 0);
        Bg.Bg_MaskPalette(4, 0);
    }

    // OFFSET 0x021D2828
    private static void OpeningWorldScene_InitCloudBgs(OpeningData data)
    {
        OpeningWorldScene_FreeBgs(data, 0);
        Bg.Bg_InitFromTemplate(data.BgConfig!, 3, GameOpening.BgTemplateAt(0x021D767C), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig!, 1, GameOpening.BgTemplateAt(0x021D7698), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig!, 2, GameOpening.BgTemplateAt(0x021D76B4), 0);
        BgTemplate[] subTemplates = GameOpening.BgTemplatesAt(0x021D77E4, 3);
        Bg.Bg_InitFromTemplate(data.BgConfig!, 5, subTemplates[0], 0);
        Bg.Bg_InitFromTemplate(data.BgConfig!, 6, subTemplates[1], 0);
        Bg.Bg_InitFromTemplate(data.BgConfig!, 7, subTemplates[2], 0);
        Bg.Bg_MaskPalette(1, 0);
        Bg.Bg_MaskPalette(4, 0);
        GXLayers.GXLayers_EngineBToggleLayers(1, 0);
        data.World.BgMode = 1;
    }

    // OFFSET 0x021D2900
    private static void OpeningWorldScene_InitFinalBgs(OpeningData data)
    {
        OpeningWorldScene_FreeModels(data);
        OpeningWorldScene_InitCloudBgs(data);
        Bg.SetAllGraphicsModes(GameOpening.GraphicsModesAt(CloudGraphicsModes));
        Bg.Bg_InitFromTemplate(data.BgConfig!, 0, GameOpening.BgTemplateAt(0x021D7564), 0);
        Bg.Bg_InitFromTemplate(data.BgConfig!, 4, GameOpening.BgTemplateAt(0x021D759C), 0);
        Bg.Bg_SetOffset(data.BgConfig!, 0, OffsetSetX, 0);
        Bg.Bg_SetOffset(data.BgConfig!, 0, OffsetSetY, 0);
        Bg.Bg_SetOffset(data.BgConfig!, 4, OffsetSetX, 0);
        Bg.Bg_SetOffset(data.BgConfig!, 4, OffsetSetY, 0);
        GXLayers.GXLayers_EngineAToggleLayers(1, 0);
        GXLayers.GXLayers_EngineBToggleLayers(1, 0);
        GXLayers.GXLayers_EngineAToggleLayers(2, 1);
        GXLayers.GXLayers_EngineBToggleLayers(2, 1);
        GXLayers.GXLayers_EngineAToggleLayers(4, 1);
        GXLayers.GXLayers_EngineBToggleLayers(4, 1);
        GXLayers.GXLayers_EngineAToggleLayers(8, 1);
        GXLayers.GXLayers_EngineBToggleLayers(8, 1);
        GXLayers.GXLayers_EngineAToggleLayers(0x10, 1);
        GXLayers.GXLayers_EngineBToggleLayers(0x10, 1);
        data.World.BgMode = 2;
    }

    // OFFSET 0x021D2A00
    private static void OpeningLegendScene_InitBgs(OpeningData data)
    {
        data.BgConfig = Bg.BgConfig_New(data.HeapID);
        Bg.SetAllGraphicsModes(GameOpening.GraphicsModesAt(LegendGraphicsModes));
        Bg.Bg_InitFromTemplate(data.BgConfig, 2, GameOpening.BgTemplateAt(0x021D7660), 0);
        Bg.Bg_MaskPalette(2, 0);
    }

    // OFFSET 0x021D2A58
    private static void OpeningWorldScene_Init3D(OpeningData data)
    {
        data.Pipeline = G3DPipeline.G3DPipeline_InitEx(data.HeapID, 0, 1, 0, 4, OpeningWorldScene_Setup3D);
        Gfd.AllocTexVram(0x8000, 0, 0);
        Gfd.AllocPlttVram(0x80, 0, 1);
    }

    // OFFSET 0x021D2AA0
    private static void OpeningWorldScene_Setup3D()
    {
        Render3D.FlushBuffer();
        Video.SetBgControl(0, (ushort)((Video.GetBgControl(0) & ~3) | 1));
        Render3D.Display3DControl &= Disp3DToonMask;
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DAntiAlias);
        Render3D.Display3DControl &= Disp3DAlphaTestMask;
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DAlphaBlend);
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DEdgeMarking);
        Render3D.SetEdgeColorTable(RomData.OverlayU16(Overlay, EdgeColorTable, 8));
        Render3D.SetFog(false, 0, 0, 0);
        Render3D.SetClearColor(0, 0, 0x7FFF, 0x3F, false);
        Render3D.SetViewport(0, 0, 255, 191);
    }

    // OFFSET 0x021D2B38
    private static void OpeningWorldScene_VBlank(object? param)
    {
        OpeningData data = (OpeningData)param!;
        OpeningWorldSceneData scene = data.World;
        if (scene.SwapRequest == 1)
        {
            gSystem.WhichScreenIs3D = 1;
            GXLayers.GXLayers_SwapDisplay();
            GXLayers.GXLayers_EngineBToggleLayers(1, 1);
            GXLayers.GXLayers_EngineAToggleLayers(4, 0);
            GXLayers.GXLayers_EngineAToggleLayers(8, 0);
            GXLayers.GXLayers_EngineBToggleLayers(0x10, 1);
            scene.SwapRequest = 0;
            Camera.Camera_ComputeProjectionMatrix(0, scene.Camera!);
            Camera.Camera_SetAsActive(scene.Camera!);
        }
        if (data.BgConfig != null)
        {
            Bg.Bg_RunScheduledUpdates(data.BgConfig);
        }
        for (int i = 0; i < 8; i++)
        {
            if ((scene.LayersOnMask & 1) != 0)
            {
                Bg.Bg_ToggleLayer((byte)i, 1);
            }
            if ((scene.LayersOffMask & 1) != 0)
            {
                Bg.Bg_ToggleLayer((byte)i, 0);
            }
            scene.LayersOnMask >>= 1;
            scene.LayersOffMask >>= 1;
        }
        Video.SetWindowHorizontal(0, 0, (byte)scene.MainLeft, (byte)scene.MainRight);
        Video.SetWindowVertical(0, 0, (byte)scene.MainTop, (byte)scene.MainBottom);
        Video.SetWindowHorizontal(1, 0, (byte)scene.SubLeft, (byte)scene.SubRight);
        Video.SetWindowVertical(1, 0, (byte)scene.SubTop, (byte)scene.SubBottom);
        if (scene.EnableWindowRequest == 1)
        {
            OpeningWorldScene_EnableWindows();
            scene.EnableWindowRequest = 0;
        }
        if (scene.DisableWindowRequest == 1)
        {
            OpeningWorldScene_DisableWindows();
            scene.DisableWindowRequest = 0;
        }
        VramTransfer.VramTransfer_Process();
        RenderOam.RenderOam_Transfer();
        OpeningStarters.OpeningStarters_VBlank(data.Starters);
    }

    // OFFSET 0x021D2C80
    private static void OpeningWorldScene_EnableWindows()
    {
        Video.SetDisplayControl(0, (Video.GetDisplayControl(0) & WindowEnableMask) | Window0Enable);
        Video.SetWindowInside(0, 0, 0x3F);
        Video.SetWindowOutside(0, 0x31);
        Video.SetDisplayControl(1, (Video.GetDisplayControl(1) & WindowEnableMask) | Window0Enable);
        Video.SetWindowInside(1, 0, 0x3F);
        Video.SetWindowOutside(1, 0x31);
    }

    // OFFSET 0x021D2CE8
    private static void OpeningWorldScene_DisableWindows()
    {
        Video.SetDisplayControl(0, Video.GetDisplayControl(0) & WindowEnableMask);
        Video.SetDisplayControl(1, Video.GetDisplayControl(1) & WindowEnableMask);
    }

    // OFFSET 0x021D33F0
    public static bool OpeningWorldScene_Run(OpeningData data)
    {
        OpeningWorldSceneData scene = data.World;
        if (data.Skipped != 0)
        {
            scene.State = 4;
        }
        switch (scene.State)
        {
            case 0:
                OpeningWorldScene_Init(data);
                scene.State++;
                break;
            case 1:
                OpeningWorldScene_LoadBgs(scene);
                scene.State++;
                break;
            case 2:
                if (OpeningWorldScene_LoadModelsStep(scene, 0))
                {
                    scene.ModelsActive = 1;
                    scene.State++;
                }
                break;
            case 3:
                if (OpeningWorldScene_Update(data, scene, data.Frame))
                {
                    scene.State++;
                }
                OpeningSprites.OpeningSprites_Draw(scene.Sprites!);
                if (scene.ModelsActive != 0)
                {
                    if (scene.Loading == 0)
                    {
                        OpeningWorldScene_Draw(scene);
                    }
                }
                else
                {
                    OpeningStarters.OpeningStarters_Draw(data.Starters);
                }
                break;
            case 4:
                OpeningWorldScene_Free(data);
                return true;
        }
        return false;
    }

    // OFFSET 0x021D34A8
    private static void OpeningWorldScene_Init(OpeningData data)
    {
        OpeningWorldSceneData scene = data.World;
        OpeningWorldScene_SetBanks();
        OpeningWorldScene_InitBgs(data);
        OpeningWorldScene_Init3D(data);
        OpeningSprites.OpeningSprites_Size();
        OpeningStarters.OpeningStarters_Size();
        scene.Sprites = new OpeningSprites();
        scene.ParticleSet = OpeningEffects.OpeningParticleSet_New(0x10);
        scene.Starters = new OpeningStarters();
        scene.SpawnInterval = 4;
        OpeningStarters.OpeningStarters_Init(scene.Starters);
        scene.BgConfig = data.BgConfig;
        gSystem.WhichScreenIs3D = 0;
        GXLayers.GXLayers_SwapDisplay();
        uint[] vectors = RomData.OverlayU32(Overlay, LightVectors, 12);
        ushort[] colors = RomData.OverlayU16(Overlay, LightColors, 4);
        for (byte i = 0; i < 4; i++)
        {
            Render3D.SetGlobalLightVector(i, (short)vectors[i * 3], (short)vectors[i * 3 + 1], (short)vectors[i * 3 + 2]);
            Render3D.SetGlobalLightColor(i, colors[i]);
        }
        Render3D.SetGlobalDiffuseAmbient(0x4210, 0x318A, false);
        Render3D.SetGlobalSpecularEmission(0x4A52, 0x39CE, false);
        Render3D.SetGlobalPolygonAttr(0xD, 0, 2, 0, 0x1F, 0x8000);
        GameSystem.SetVBlankCallback(OpeningWorldScene_VBlank, data);
        BrightnessController.BrightnessController_SetScreenBrightness(-16, 8, 1);
        scene.Initialized = 1;
    }

    // OFFSET 0x021D35B8
    private static void OpeningWorldScene_FreeModels(OpeningData data)
    {
        OpeningWorldSceneData scene = data.World;
        if (scene.ModelsLoaded == 0 && scene.Loading == 0)
        {
            return;
        }
        Camera.Camera_ClearActive();
        Camera.Camera_Delete(scene.Camera!);
        for (int i = 0; i < scene.ModelCount; i++)
        {
            Heap.Heap_Free(scene.Resources[i]!);
        }
        scene.ModelCount = 0;
        Heap.Heap_Free(scene.TextureResource!);
        scene.ModelsLoaded = 0;
        scene.Loading = 0;
    }

    // OFFSET 0x021D361C
    private static void OpeningWorldScene_FreeBgs(OpeningData data, int freeConfig)
    {
        OpeningWorldSceneData scene = data.World;
        if (scene.Initialized == 0 || data.BgConfig == null)
        {
            return;
        }
        BgConfig bgConfig = data.BgConfig;
        if (scene.BgMode == 0)
        {
            Bg.Bg_FreeTilemapBuffer(bgConfig, 3);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 1);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 2);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 4);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 5);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 6);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 7);
        }
        else if (scene.BgMode == 1)
        {
            Bg.Bg_FreeTilemapBuffer(bgConfig, 3);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 1);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 2);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 5);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 6);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 7);
        }
        else
        {
            Bg.Bg_FreeTilemapBuffer(bgConfig, 0);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 3);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 1);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 2);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 4);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 5);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 6);
            Bg.Bg_FreeTilemapBuffer(bgConfig, 7);
        }
        if (freeConfig == 1)
        {
            data.BgConfig = null;
        }
    }

    // OFFSET 0x021D36F8
    private static void OpeningWorldScene_Free(OpeningData data)
    {
        OpeningWorldSceneData scene = data.World;
        Video.SetDisplayControl(0, Video.GetDisplayControl(0) & WindowEnableMask);
        OpeningWorldScene_FreeModels(data);
        if (scene.BgsLoaded != 0)
        {
            OpeningSprites.OpeningSprites_FreeRendering(scene.Sprites!);
            scene.BgsLoaded = 0;
        }
        if (scene.Initialized != 0)
        {
            if (scene.ParticleSet != null)
            {
                OpeningEffects.OpeningParticleSet_Free(scene.ParticleSet);
                scene.ParticleSet = null;
            }
            if (scene.Sprites != null)
            {
                scene.Sprites = null;
            }
            if (scene.Starters != null)
            {
                OpeningStarters.OpeningStarters_Free(scene.Starters);
                scene.Starters = null;
            }
            OpeningWorldScene_FreeBgs(data, 1);
            if (scene.PaletteBuffer != null)
            {
                scene.PaletteBuffer = null;
            }
            G3DPipeline.G3DPipelineBuffers_Free(data.Pipeline!);
            scene.Initialized = 0;
        }
        Render3D.FlushBuffer();
        Render3D.SetFog(false, 0, 0, 0);
        Video.SetDisplayControl(0, Video.GetDisplayControl(0) & WindowEnableMask);
        Video.SetDisplayControl(1, Video.GetDisplayControl(1) & WindowEnableMask);
        GameSystem.SetVBlankCallback(null, null);
    }

    private static void ClearBackdrops()
    {
        Video.LoadPaletteRange(Video.MainBackground, new ushort[1], 0, 0, 2);
        Video.LoadPaletteRange(Video.SubBackground, new ushort[1], 0, 0, 2);
    }

    // OFFSET 0x021D37C0
    private static void OpeningWorldScene_LoadBgs(OpeningWorldSceneData scene)
    {
        BgConfig bgConfig = scene.BgConfig!;
        NARC narc = NARC.NARC_ctor(GraphicsArchive, HeapID);
        Graphics.Graphics_LoadPaletteWithSrcOffsetFromOpenNARC(narc, 0x60, PaletteMainBg, 0x180, 0x180, 0x40, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x5F, bgConfig, 3, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0xC, bgConfig, 3, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadPaletteWithSrcOffsetFromOpenNARC(narc, 0xD, PaletteMainBg, 0x1C0, 0x1C0, 0x40, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0xE, bgConfig, 2, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0xC, bgConfig, 2, 0, 0, 0, HeapID);
        NARC logo = NARC.NARC_ctor(LogoArchive, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(logo, 0x38, bgConfig, 1, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(logo, 0x39, bgConfig, 1, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(logo, 0x39, bgConfig, 1, 0x400, 0, 0, HeapID);
        Graphics.Graphics_LoadPaletteFromOpenNARC(logo, 0x37, PaletteMainBg, 0, 0x20, HeapID);
        NARC.NARC_dtor(logo);
        Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 0x3D, PaletteSubBg, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x3B, bgConfig, 5, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x3F, bgConfig, 5, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x3A, bgConfig, 6, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x3E, bgConfig, 6, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x3C, bgConfig, 7, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x40, bgConfig, 7, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0xE, bgConfig, 4, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0xC, bgConfig, 4, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadPaletteWithSrcOffsetFromOpenNARC(narc, 0xD, PaletteSubBg, 0x1C0, 0x1C0, 0x40, HeapID);
        OpeningSprites.OpeningSprites_InitRendering(scene.Sprites!);
        OpeningSprites.OpeningSprites_CreateSprites(scene.Sprites!);
        OpeningSprites.OpeningSprites_CreateParticleSprites(scene.Sprites!, scene.ParticleSet!);
        ClearBackdrops();
        GXLayers.GXLayers_EngineAToggleLayers(0xE, 0);
        GXLayers.GXLayers_EngineBToggleLayers(0xF, 0);
        ScreenFadeControl.ResetScreenMasterBrightness(0);
        ScreenFadeControl.ResetScreenMasterBrightness(1);
        NARC.NARC_dtor(narc);
        scene.BgsLoaded = 1;
    }

    // OFFSET 0x021D3A10
    private static void OpeningWorldScene_LoadSkyBgs(OpeningData data, OpeningWorldSceneData scene)
    {
        BgConfig bgConfig = scene.BgConfig!;
        Bg.Bg_SetControlParam(bgConfig, 2, 0, 0);
        Bg.Bg_SetControlParam(bgConfig, 3, 0, 0);
        Video.SetBgControl(0, (ushort)(Video.GetBgControl(0) & ~3));
        Bg.Bg_SetPriority(1, 3);
        NARC narc = NARC.NARC_ctor(GraphicsArchive, HeapID);
        Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 0x44, PaletteMainBg, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x42, bgConfig, 1, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x46, bgConfig, 1, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x41, bgConfig, 2, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x45, bgConfig, 2, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x43, bgConfig, 3, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x47, bgConfig, 3, 0, 0, 0, HeapID);
        ClearBackdrops();
        Bg.Bg_SetOffset(bgConfig, 1, OffsetSetX, 0);
        Bg.Bg_SetOffset(bgConfig, 2, OffsetSetX, 0);
        Bg.Bg_SetOffset(bgConfig, 3, OffsetSetX, 0);
        Bg.Bg_SetOffset(bgConfig, 5, OffsetSetX, 0);
        Bg.Bg_SetOffset(bgConfig, 6, OffsetSetX, 0);
        Bg.Bg_SetOffset(bgConfig, 7, OffsetSetX, 0);
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x021D3B5C
    private static void OpeningWorldScene_LoadCloudBgs(OpeningData data, OpeningWorldSceneData scene)
    {
        OpeningWorldScene_InitCloudBgs(data);
        BgConfig bgConfig = scene.BgConfig!;
        NARC narc = NARC.NARC_ctor(GraphicsArchive, HeapID);
        Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 0x48, PaletteMainBg, 0, 0, HeapID);
        Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 0x48, PaletteSubBg, 0, 0, HeapID);
        scene.PaletteBuffer = new ushort[0x100];
        Array.Copy(Video.GetPalette(Video.MainBackground), scene.PaletteBuffer, 0x100);
        scene.PaletteBuffer[0] = 0;
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x49, bgConfig, 1, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x4C, bgConfig, 1, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x49, bgConfig, 5, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x4C, bgConfig, 5, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x4A, bgConfig, 2, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x4D, bgConfig, 2, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x4A, bgConfig, 6, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x4D, bgConfig, 6, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x4B, bgConfig, 3, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x4E, bgConfig, 3, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x4B, bgConfig, 7, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x4E, bgConfig, 7, 0, 0, 0, HeapID);
        ClearBackdrops();
        Bg.Bg_SetOffset(bgConfig, 1, OffsetSetX, -0x80);
        Bg.Bg_SetOffset(bgConfig, 2, OffsetSetX, -0xC8);
        Bg.Bg_SetOffset(bgConfig, 3, OffsetSetX, -0x98);
        Bg.Bg_SetOffset(bgConfig, 5, OffsetSetX, 0x80);
        Bg.Bg_SetOffset(bgConfig, 6, OffsetSetX, 0xC0);
        Bg.Bg_SetOffset(bgConfig, 7, OffsetSetX, 0xB0);
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x021D3D4C
    private static void OpeningWorldScene_LoadFinalBgs(OpeningData data, OpeningWorldSceneData scene)
    {
        OpeningWorldScene_InitFinalBgs(data);
        BgConfig bgConfig = scene.BgConfig!;
        NARC narc = NARC.NARC_ctor(GraphicsArchive, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x4F, bgConfig, 0, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x50, bgConfig, 0, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x51, bgConfig, 4, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x52, bgConfig, 4, 0, 0, 0, HeapID);
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x021D3DC4
    private static void OpeningWorldScene_ArrangeCloudBgs(OpeningWorldSceneData scene)
    {
        BgConfig bgConfig = scene.BgConfig!;
        if (gSystem.WhichScreenIs3D == 0)
        {
            Bg.Bg_SetOffset(bgConfig, 1, OffsetSetY, 0);
            Bg.Bg_SetOffset(bgConfig, 2, OffsetSetY, 0);
            Bg.Bg_SetOffset(bgConfig, 3, OffsetSetY, 0);
            Bg.Bg_SetOffset(bgConfig, 5, OffsetSetY, 0x100);
            Bg.Bg_SetOffset(bgConfig, 6, OffsetSetY, 0x100);
            Bg.Bg_SetOffset(bgConfig, 7, OffsetSetY, 0x100);
        }
        else
        {
            Bg.Bg_SetOffset(bgConfig, 1, OffsetSetY, 0x100);
            Bg.Bg_SetOffset(bgConfig, 2, OffsetSetY, 0x100);
            Bg.Bg_SetOffset(bgConfig, 3, OffsetSetY, 0x100);
            Bg.Bg_SetOffset(bgConfig, 5, OffsetSetY, 0);
            Bg.Bg_SetOffset(bgConfig, 6, OffsetSetY, 0);
            Bg.Bg_SetOffset(bgConfig, 7, OffsetSetY, 0);
        }
        Bg.Bg_SetPriority(1, 1);
        Bg.Bg_SetPriority(2, 2);
        Bg.Bg_SetPriority(3, 0);
        Bg.Bg_SetPriority(5, 1);
        Bg.Bg_SetPriority(6, 2);
        Bg.Bg_SetPriority(7, 0);
        OpeningSprites.OpeningSprites_ResetClouds(scene.Sprites!, 0);
    }

    // OFFSET 0x021D3EDC
    private static void OpeningWorldScene_LoadTexture(OpeningWorldSceneData scene, int set)
    {
        uint member = RomData.OverlayU32(Overlay, TextureMembers + (uint)set * 4, 1)[0];
        scene.TextureResource = NARC.NARC_AllocAndReadWholeMemberByIndexPair(GraphicsArchive, (int)member, HeapID);
        scene.Texture = G3d.GetTexture(scene.TextureResource);
        if (!Easy3D.Easy3D_UploadTextureToVRAM(scene.Texture))
        {
            ErrorHandling.GF_AssertFail();
        }
        if (Bin.U32(scene.Texture!.File, scene.Texture.Offset + 0x14) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021D3F24
    private static void OpeningWorldScene_LoadModels(OpeningWorldSceneData scene, int set, int start, int end)
    {
        if (end > OpeningWorldSceneData.ModelCapacity)
        {
            end = OpeningWorldSceneData.ModelCapacity;
        }
        uint[] members = RomData.OverlayU32(Overlay, ModelMembers + (uint)set * 0x18, OpeningWorldSceneData.ModelCapacity);
        NARC narc = NARC.NARC_ctor(GraphicsArchive, HeapID);
        for (byte i = (byte)start; i < end; i++)
        {
            if (members[i] == 0)
            {
                break;
            }
            byte[] resource = NARC.NARC_AllocAndReadWholeMember(narc, members[i], HeapID);
            scene.Resources[i] = resource;
            if (!Easy3D.Easy3D_BindTextureToResource(resource, scene.Texture))
            {
                ErrorHandling.GF_AssertFail();
            }
            G3dModelSet modelSet = G3d.GetModelSet(resource)!;
            if (resource[modelSet.Offset + 9] != 1)
            {
                ErrorHandling.GF_AssertFail();
            }
            G3dModel? model = G3d.GetModelByIndex(modelSet, 0);
            if (model == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            G3d.InitRenderObj(scene.RenderObjs[i], model!);
            G3d.SetMaterialFlags(model!, false, ModelFlagDiffuse);
            G3d.SetMaterialFlags(model!, false, ModelFlagAmbient);
            G3d.SetMaterialFlags(model!, false, ModelFlagSpecular);
            G3d.SetMaterialFlags(model!, false, ModelFlagEmission);
            G3d.SetMaterialPolygonAttrMask(model!, false, 0xF);
            scene.ModelCount++;
        }
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x021D4044
    private static void OpeningWorldScene_InitCamera(OpeningWorldSceneData scene)
    {
        scene.Camera = Camera.Camera_Alloc(HeapID);
        VecFx32 target = VectorAt(CameraTargetStart);
        Camera.Camera_InitWithTarget(() => target, CameraDistance, AngleAt(CameraAngleAddress), CameraFov, 0, false, scene.Camera);
        Camera.Camera_SetFOV(CameraStartFov, scene.Camera);
        Camera.Camera_Move(VectorAt(CameraOffsetStart), scene.Camera);
        Camera.Camera_SetAsActive(scene.Camera);
    }

    // OFFSET 0x021D40B8
    private static void OpeningWorldScene_LoadModelsNow(OpeningWorldSceneData scene, int set)
    {
        OpeningWorldScene_LoadTexture(scene, set);
        OpeningWorldScene_LoadModels(scene, set, 0, OpeningWorldSceneData.ModelCapacity);
        OpeningWorldScene_InitCamera(scene);
        scene.ModelsLoaded = 1;
    }

    // OFFSET 0x021D40DC
    private static bool OpeningWorldScene_LoadModelsStep(OpeningWorldSceneData scene, int set)
    {
        if (set == 0)
        {
            switch (scene.LoadStep)
            {
                case 0:
                    scene.Loading = 1;
                    OpeningWorldScene_LoadTexture(scene, set);
                    OpeningWorldScene_InitCamera(scene);
                    break;
                case 1:
                    OpeningWorldScene_LoadModels(scene, set, 0, 1);
                    break;
                default:
                    OpeningWorldScene_LoadModels(scene, set, 1, 6);
                    scene.LoadStep = 0;
                    scene.Loading = 0;
                    scene.ModelsLoaded = 1;
                    return true;
            }
        }
        else
        {
            switch (scene.LoadStep)
            {
                case 0:
                    scene.Loading = 1;
                    OpeningWorldScene_LoadTexture(scene, set);
                    OpeningWorldScene_InitCamera(scene);
                    break;
                case 1:
                    OpeningWorldScene_LoadModels(scene, set, 0, 2);
                    break;
                case 2:
                    OpeningWorldScene_LoadModels(scene, set, 2, 4);
                    break;
                default:
                    OpeningWorldScene_LoadModels(scene, set, 4, 6);
                    scene.LoadStep = 0;
                    scene.Loading = 0;
                    scene.ModelsLoaded = 1;
                    return true;
            }
        }
        scene.LoadStep++;
        return false;
    }

    // OFFSET 0x021D4188
    private static void OpeningWorldScene_Draw(OpeningWorldSceneData scene)
    {
        uint[] rawRotation = RomData.OverlayU32(Overlay, ModelRotationAddress, 9);
        MtxFx33 rotation = new()
        {
            M00 = (int)rawRotation[0],
            M01 = (int)rawRotation[1],
            M02 = (int)rawRotation[2],
            M10 = (int)rawRotation[3],
            M11 = (int)rawRotation[4],
            M12 = (int)rawRotation[5],
            M20 = (int)rawRotation[6],
            M21 = (int)rawRotation[7],
            M22 = (int)rawRotation[8],
        };
        VecFx32 scale = VectorAt(ModelScaleAddress);
        VecFx32 position = new() { X = 0, Y = 0, Z = 0 };
        Render3D.FlushBuffer();
        G3SwapRequest.G3_ResetG3X();
        Camera.Camera_ComputeViewMatrix();
        if (scene.DrawModels == 1)
        {
            for (byte i = 0; i < scene.ModelCount; i++)
            {
                Easy3D.Easy3D_DrawRenderObj(scene.RenderObjs[i], position, rotation, scale);
            }
        }
        if (scene.DrawParticles == 1)
        {
            Render3D.FlushBuffer();
            G3SwapRequest.G3_ResetG3X();
            ParticleSystem.ParticleSystem_DrawAll();
            ParticleSystem.ParticleSystem_UpdateAll();
        }
        G3SwapRequest.G3_RequestSwapBuffers(0, 1);
    }

    // OFFSET 0x021D4230
    private static bool OpeningWorldScene_Update(OpeningData data, OpeningWorldSceneData scene, int frame)
    {
        BgConfig bgConfig = scene.BgConfig!;
        OpeningSprites sprites = scene.Sprites!;
        if (scene.SpawnTimer == 0)
        {
            OpeningEffects.OpeningParticleSet_SpawnPair(scene.ParticleSet!, scene.HideParticles);
        }
        scene.SpawnTimer = (byte)((scene.SpawnTimer + 1) % scene.SpawnInterval);
        switch (scene.SubState)
        {
            case 0:
                if (frame >= FrameLogoBg)
                {
                    Bg.Bg_ToggleLayer(3, 1);
                    BrightnessController.BrightnessController_StartTransition(8, 0, -16, 8, 1);
                    scene.SubState++;
                }
                break;
            case 1:
                if (BrightnessController.BrightnessController_IsTransitionComplete(1) && frame == FrameLogoParticles)
                {
                    Bg.Bg_ToggleLayer(0, 1);
                    OpeningStarters.OpeningStarters_CreateEmitter(scene.Starters, 9);
                    OpeningStarters.OpeningStarters_CreateEmitter(scene.Starters, 10);
                    OpeningStarters.OpeningStarters_CreateEmitter(scene.Starters, 11);
                    Video.SetBlendAlpha(0, 0, 0xF, 0, 0);
                    scene.DrawParticles = 1;
                    OpeningSprites.OpeningSprites_SetLogoAnimate(sprites, 1);
                    scene.SubState++;
                }
                break;
            case 2:
                if (frame == FrameLogoFlash)
                {
                    ScreenFadeControl.StartScreenFade(3, 0, 0, White, 4, 1, HeapID);
                }
                if (frame == FrameLogoSwap)
                {
                    if (!ScreenFadeControl.IsScreenFadeDone())
                    {
                        ErrorHandling.GF_AssertFail();
                    }
                    scene.LayersOnMask = 4;
                    scene.LayersOffMask = 8;
                    ScreenFadeControl.StartScreenFade(3, 1, 1, White, 4, 1, HeapID);
                }
                if (frame == FrameWorldStart)
                {
                    GXLayers.GXLayers_EngineAToggleLayers(1, 0);
                    scene.SwapRequest = 1;
                    scene.DrawParticles = 0;
                    scene.DrawModels = 1;
                    if (ParticleSystem.ParticleSystem_GetActiveEmitterCount(OpeningStarters.OpeningStarters_GetParticleSystem(scene.Starters)) != 0)
                    {
                        ErrorHandling.GF_AssertFail();
                    }
                }
                if (frame == FrameWorldObjects)
                {
                    GXLayers.GXLayers_EngineBToggleLayers(0x10, 1);
                }
                if (frame >= FrameWorldFadeIn)
                {
                    GXLayers.GXLayers_EngineAToggleLayers(1, 1);
                    ScreenFadeControl.StartScreenFade(3, 1, 1, White, 0x12, 1, HeapID);
                    scene.SpawnInterval = 0x10;
                    scene.SubState++;
                }
                break;
            case 3:
                OpeningWorldScene_SwitchModels(data, scene, frame);
                OpeningWorldScene_MoveCamera(scene, frame);
                OpeningSprites.OpeningSprites_UpdateFlock(sprites, frame);
                OpeningSprites.OpeningSprites_UpdateBirdLeft(sprites, frame);
                OpeningSprites.OpeningSprites_UpdateBirdRight(sprites, frame);
                if (frame >= FrameWorldFadeOut)
                {
                    ScreenFadeControl.StartScreenFade(0, 0, 0, White, 0x12, 1, HeapID);
                    scene.SubState++;
                }
                break;
            case 4:
                OpeningWorldScene_MoveCamera(scene, frame);
                OpeningSprites.OpeningSprites_UpdateFlock(sprites, frame);
                OpeningSprites.OpeningSprites_UpdateBirdLeft(sprites, frame);
                OpeningSprites.OpeningSprites_UpdateBirdRight(sprites, frame);
                if (ScreenFadeControl.IsScreenFadeDone() && frame >= FrameCloudsStart)
                {
                    Bg.Bg_ToggleLayer(4, 0);
                    OpeningSprites.OpeningSprites_HideParticleSprites(sprites, scene.ParticleSet!);
                    scene.HideParticles = 1;
                    OpeningWorldScene_LoadSkyBgs(data, scene);
                    OpeningSprites.OpeningSprites_SetDrawFlag(sprites, OpeningSprites.SpriteLogoBottom, 1);
                    OpeningSprites.OpeningSprites_SetDrawFlag(sprites, OpeningSprites.SpriteLogoShine, 1);
                    Bg.Bg_ToggleLayer(1, 1);
                    Bg.Bg_ToggleLayer(2, 1);
                    Bg.Bg_ToggleLayer(3, 1);
                    Bg.Bg_ToggleLayer(5, 1);
                    Bg.Bg_ToggleLayer(6, 1);
                    Bg.Bg_ToggleLayer(7, 1);
                    Bg.Bg_SetOffset(bgConfig, 1, OffsetSetX, 0x100);
                    Bg.Bg_SetOffset(bgConfig, 1, OffsetSetY, 0);
                    Bg.Bg_SetOffset(bgConfig, 2, OffsetSetX, 0);
                    Bg.Bg_SetOffset(bgConfig, 2, OffsetSetY, 0x100);
                    Bg.Bg_SetOffset(bgConfig, 3, OffsetSetX, 0);
                    Bg.Bg_SetOffset(bgConfig, 3, OffsetSetY, 0);
                    Bg.Bg_SetOffset(bgConfig, 5, OffsetSetX, 0x100);
                    Bg.Bg_SetOffset(bgConfig, 5, OffsetSetY, 0);
                    Bg.Bg_SetOffset(bgConfig, 6, OffsetSetX, 0);
                    Bg.Bg_SetOffset(bgConfig, 6, OffsetSetY, 0x100);
                    Bg.Bg_SetOffset(bgConfig, 7, OffsetSetX, 0);
                    Bg.Bg_SetOffset(bgConfig, 7, OffsetSetY, 0);
                    scene.ScrollFast = 0x100000;
                    scene.MainLeft = 0;
                    scene.MainRight = 0xFF;
                    scene.MainTop = 0x20;
                    scene.MainBottom = 0x9F;
                    scene.SubLeft = 0;
                    scene.SubRight = 0xFF;
                    scene.SubTop = 0x20;
                    scene.SubBottom = 0x9F;
                    scene.EnableWindowRequest = 1;
                    scene.ModelsActive = 0;
                    Video.SetBlendControl(0, 0);
                    Video.SetBlendControl(1, 0);
                    ScreenFadeControl.StartScreenFade(0, 1, 1, White, 0x12, 1, HeapID);
                    scene.SubState++;
                }
                break;
            case 5:
                OpeningWorldScene_ScrollSky(scene);
                OpeningSprites.OpeningSprites_ScrollLogo(sprites);
                if (ScreenFadeControl.IsScreenFadeDone() && frame >= FrameCloudsEnd)
                {
                    Bg.Bg_ScheduleScroll(bgConfig, 3, OffsetSetX, 0);
                    Bg.Bg_ScheduleScroll(bgConfig, 3, OffsetSetY, 0x100);
                    Bg.Bg_ScheduleScroll(bgConfig, 7, OffsetSetX, 0);
                    Bg.Bg_ScheduleScroll(bgConfig, 7, OffsetSetY, 0x100);
                    Bg.Bg_ScheduleScroll(bgConfig, 2, OffsetSetX, 0);
                    Bg.Bg_ScheduleScroll(bgConfig, 2, OffsetSetY, 0);
                    Bg.Bg_ScheduleScroll(bgConfig, 6, OffsetSetX, 0);
                    Bg.Bg_ScheduleScroll(bgConfig, 6, OffsetSetY, 0);
                    scene.LayersOffMask = 0x22;
                    scene.DisableWindowRequest = 1;
                    OpeningSprites.OpeningSprites_SetDrawFlag(sprites, OpeningSprites.SpriteLogoBottom, 0);
                    OpeningSprites.OpeningSprites_SetDrawFlag(sprites, OpeningSprites.SpriteLogoShine, 0);
                    scene.SubState++;
                }
                break;
            case 6:
                OpeningWorldScene_ScrollStreaks(scene);
                OpeningWorldScene_UpdateRingBgs(scene, frame);
                if (frame == FrameRingFade)
                {
                    ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 4, 1, HeapID);
                    scene.SubState++;
                }
                break;
            case 7:
                OpeningWorldScene_ScrollStreaks(scene);
                OpeningWorldScene_UpdateRingBgs(scene, frame);
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    scene.LayersOnMask = 0x22;
                    scene.LayersOffMask = 0x44;
                    Bg.Bg_ScheduleScroll(bgConfig, 1, OffsetSetX, 0);
                    Bg.Bg_ScheduleScroll(bgConfig, 1, OffsetSetY, 0);
                    Bg.Bg_ScheduleScroll(bgConfig, 5, OffsetSetX, 0);
                    Bg.Bg_ScheduleScroll(bgConfig, 5, OffsetSetY, 0);
                    ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 4, 1, HeapID);
                    scene.SubState++;
                }
                break;
            case 8:
                OpeningWorldScene_ScrollStreaks(scene);
                OpeningWorldScene_UpdateRingBgs(scene, frame);
                OpeningSprites.OpeningSprites_SpinRings(sprites, frame);
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    if (frame == FrameRingShow)
                    {
                        Bg.Bg_ScheduleScroll(bgConfig, 1, OffsetSetX, 0);
                        Bg.Bg_ScheduleScroll(bgConfig, 1, OffsetSetY, 0x100);
                        Bg.Bg_ScheduleScroll(bgConfig, 5, OffsetSetX, 0);
                        Bg.Bg_ScheduleScroll(bgConfig, 5, OffsetSetY, 0x100);
                        OpeningSprites.OpeningSprites_SetDrawFlag(sprites, OpeningSprites.SpriteRingTop, 1);
                        OpeningSprites.OpeningSprites_SetDrawFlag(sprites, OpeningSprites.SpriteRingBottom, 1);
                    }
                    else if (frame >= FrameRingShow + FrameRingHold)
                    {
                        ScreenFadeControl.StartScreenFade(0, 0, 0, White, 4, 1, HeapID);
                        scene.SubState++;
                    }
                }
                break;
            case 9:
                OpeningSprites.OpeningSprites_SpinRings(sprites, frame);
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    ScreenFadeControl.StartScreenFade(0, 1, 1, White, 0x40, 1, HeapID);
                    OpeningSprites.OpeningSprites_SetDrawFlag(sprites, OpeningSprites.SpriteRingTop, 0);
                    OpeningSprites.OpeningSprites_SetDrawFlag(sprites, OpeningSprites.SpriteRingBottom, 0);
                    OpeningSprites.OpeningSprites_SetDrawFlag(sprites, OpeningSprites.SpriteCloudsTop, 1);
                    OpeningSprites.OpeningSprites_SetDrawFlag(sprites, OpeningSprites.SpriteCloudsTop + 1, 1);
                    OpeningSprites.OpeningSprites_SetDrawFlag(sprites, OpeningSprites.SpriteCloudsBottom, 1);
                    OpeningSprites.OpeningSprites_SetDrawFlag(sprites, OpeningSprites.SpriteCloudsBottom + 1, 1);
                    OpeningSprites.OpeningSprites_ResetClouds(sprites, 0);
                    OpeningWorldScene_LoadCloudBgs(data, scene);
                    gSystem.WhichScreenIs3D = 0;
                    GXLayers.GXLayers_SwapDisplay();
                    OpeningWorldScene_ArrangeCloudBgs(scene);
                    scene.DisableWindowRequest = 1;
                    scene.SubState++;
                }
                else
                {
                    OpeningWorldScene_ScrollStreaks(scene);
                    OpeningWorldScene_UpdateRingBgs(scene, frame);
                }
                break;
            case 10:
                OpeningSprites.OpeningSprites_ScrollClouds(sprites, CloudSpeed);
                OpeningWorldScene_SlideCloudBgs(scene, frame);
                if (ScreenFadeControl.IsScreenFadeDone() && frame >= FrameStartersFade)
                {
                    ScreenFadeControl.StartScreenFade(3, 0, 0, 0, 4, 1, HeapID);
                    scene.SubState++;
                }
                break;
            case 11:
                OpeningSprites.OpeningSprites_ScrollClouds(sprites, CloudSpeed);
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    Video.SetBlendAlpha(0, 0, 0x3E, 0, 0);
                    Video.LoadPalette(Video.MainBackground, new ushort[0x100], 0x200);
                    ScreenFadeControl.StartScreenFade(3, 1, 1, 0, 4, 1, HeapID);
                    scene.SubState++;
                }
                break;
            case 12:
                OpeningSprites.OpeningSprites_ScrollClouds(sprites, CloudSpeed);
                if (ScreenFadeControl.IsScreenFadeDone() && OpeningStarters.OpeningStarters_Update(scene.Starters!, 0))
                {
                    ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 4, 1, HeapID);
                    scene.SubState++;
                }
                break;
            case 13:
                OpeningSprites.OpeningSprites_ScrollClouds(sprites, CloudSpeed);
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    gSystem.WhichScreenIs3D = 1;
                    GXLayers.GXLayers_SwapDisplay();
                    OpeningWorldScene_ArrangeCloudBgs(scene);
                    ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 4, 1, HeapID);
                    scene.SubState++;
                }
                break;
            case 14:
                OpeningSprites.OpeningSprites_ScrollClouds(sprites, CloudSpeed);
                if (ScreenFadeControl.IsScreenFadeDone() && frame >= FrameStartersSecond && OpeningStarters.OpeningStarters_Update(scene.Starters!, 1))
                {
                    ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 4, 1, HeapID);
                    scene.Reserved = 0;
                    scene.SubState++;
                }
                break;
            case 15:
                OpeningSprites.OpeningSprites_ScrollClouds(sprites, CloudSpeed);
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    Video.LoadPalette(Video.MainBackground, scene.PaletteBuffer!, 0x200);
                    gSystem.WhichScreenIs3D = 0;
                    GXLayers.GXLayers_SwapDisplay();
                    OpeningWorldScene_LoadFinalBgs(data, scene);
                    OpeningWorldScene_ArrangeCloudBgs(scene);
                    OpeningSprites.OpeningSprites_ResetClouds(sprites, 1);
                    Bg.Bg_ScheduleScroll(bgConfig, 1, OffsetSetX, -0x3C);
                    Bg.Bg_ScheduleScroll(bgConfig, 2, OffsetSetX, -0x3C);
                    Bg.Bg_ScheduleScroll(bgConfig, 3, OffsetSetX, -0x3C);
                    Bg.Bg_ScheduleScroll(bgConfig, 5, OffsetSetX, 0x3C);
                    Bg.Bg_ScheduleScroll(bgConfig, 6, OffsetSetX, 0x3C);
                    Bg.Bg_ScheduleScroll(bgConfig, 7, OffsetSetX, 0x3C);
                    ScreenFadeControl.StartScreenFade(3, 1, 1, 0, 0x10, 1, HeapID);
                    scene.SubState++;
                }
                break;
            case 16:
                OpeningSprites.OpeningSprites_ScrollClouds(sprites, CloudSpeed);
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    ScreenFadeControl.StartScreenFade(4, 1, 1, 0, 0x10, 1, HeapID);
                    scene.SubState++;
                }
                break;
            case 17:
                OpeningSprites.OpeningSprites_ScrollClouds(sprites, CloudSpeed);
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    if (frame >= FrameFinalSlide)
                    {
                        OpeningWorldScene_SlideFinalBgs(scene);
                    }
                    if (frame >= FrameFinalFade)
                    {
                        scene.LayersOnMask |= 0x11;
                        ScreenFadeControl.StartScreenFade(0, 0, 0, White, 6, 1, HeapID);
                        scene.SubState++;
                    }
                }
                break;
            case 18:
                OpeningWorldScene_SlideFinalBgs(scene);
                OpeningSprites.OpeningSprites_ScrollClouds(sprites, CloudSpeed);
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    gSystem.WhichScreenIs3D = 1;
                    GXLayers.GXLayers_SwapDisplay();
                    scene.SubState++;
                }
                break;
            case 19:
                return true;
        }
        return false;
    }

    // OFFSET 0x021D4B70
    private static void OpeningWorldScene_ScrollSky(OpeningWorldSceneData scene)
    {
        BgConfig bgConfig = scene.BgConfig!;
        scene.ScrollFast += 0x280;
        scene.ScrollSlow += 0x110;
        Bg.Bg_SetOffset(bgConfig, 1, OffsetSetY, -(scene.ScrollSlow >> 12));
        Bg.Bg_SetOffset(bgConfig, 2, OffsetSetY, -(scene.ScrollFast >> 12));
        Bg.Bg_SetOffset(bgConfig, 5, OffsetSetY, -(scene.ScrollSlow >> 12));
        Bg.Bg_SetOffset(bgConfig, 6, OffsetSetY, -(scene.ScrollFast >> 12));
        scene.Reserved = 0;
    }

    // OFFSET 0x021D4BE4
    private static void OpeningWorldScene_ScrollStreaks(OpeningWorldSceneData scene)
    {
        Bg.Bg_ScheduleScroll(scene.BgConfig!, 3, OffsetAddX, 0x10);
        Bg.Bg_ScheduleScroll(scene.BgConfig!, 7, OffsetAddX, -0x10);
    }

    // OFFSET 0x021D4C04
    private static void OpeningWorldScene_UpdateRingBgs(OpeningWorldSceneData scene, int frame)
    {
        BgConfig bgConfig = scene.BgConfig!;
        if (frame == FrameRingShow)
        {
            Video.SetWindowInside(0, 0, 0x3F);
            Video.SetWindowOutside(0, 0x3D);
            scene.MainLeft = 0;
            scene.MainRight = 0xFF;
            scene.MainTop = 0;
            scene.MainBottom = 0x88;
            Video.SetWindowHorizontal(0, 0, (byte)scene.MainLeft, (byte)scene.MainRight);
            Video.SetWindowVertical(0, 0, (byte)scene.MainTop, (byte)scene.MainBottom);
            Video.SetDisplayControl(0, (Video.GetDisplayControl(0) & WindowEnableMask) | Window0Enable);
            Video.SetWindowInside(1, 0, 0x3F);
            Video.SetWindowOutside(1, 0x3D);
            Video.SetWindowInside(1, 1, 0x3F);
            scene.SubLeft = 0;
            scene.SubRight = 0xFF;
            scene.SubTop = 0;
            scene.SubBottom = 0x88;
            Video.SetWindowHorizontal(1, 0, (byte)scene.SubLeft, (byte)scene.SubRight);
            Video.SetWindowVertical(1, 0, (byte)scene.SubTop, (byte)scene.SubBottom);
            Video.SetWindowHorizontal(1, 1, 1, 0);
            Video.SetWindowVertical(1, 1, (byte)scene.SubTop, (byte)scene.SubBottom);
            Video.SetDisplayControl(1, (Video.GetDisplayControl(1) & WindowEnableMask) | Window01Enable);
            return;
        }
        if (frame <= FrameRingShow)
        {
            return;
        }
        if (Bg.Bg_GetXOffset2(bgConfig, 1) >= 0x100)
        {
            scene.LayersOffMask |= 2;
        }
        if (Bg.Bg_GetXOffset2(bgConfig, 5) <= -0x100)
        {
            scene.LayersOffMask |= 0x20;
        }
        Bg.Bg_ScheduleScroll(bgConfig, 1, OffsetAddX, 0xC);
        Bg.Bg_ScheduleScroll(bgConfig, 1, OffsetAddY, -1);
        Bg.Bg_ScheduleScroll(bgConfig, 5, OffsetAddX, -0xC);
        Bg.Bg_ScheduleScroll(bgConfig, 5, OffsetAddY, -1);
    }

    private static void SlideTowardZero(BgConfig bgConfig, byte upper, byte lower, int step)
    {
        int value = Bg.Bg_GetXOffset(bgConfig, upper) + step;
        if (value > 0)
        {
            value = 0;
        }
        Bg.Bg_ScheduleScroll(bgConfig, upper, OffsetSetX, value);
        value = Bg.Bg_GetXOffset(bgConfig, lower) - step;
        if (value < 0)
        {
            value = 0;
        }
        Bg.Bg_ScheduleScroll(bgConfig, lower, OffsetSetX, value);
    }

    // OFFSET 0x021D4DC8
    private static void OpeningWorldScene_SlideCloudBgs(OpeningWorldSceneData scene, int frame)
    {
        BgConfig bgConfig = scene.BgConfig!;
        if (frame > 0x788)
        {
            SlideTowardZero(bgConfig, 1, 5, 0x14);
        }
        if (frame >= 0x78E)
        {
            SlideTowardZero(bgConfig, 2, 6, 0x14);
        }
        if (frame >= 0x794)
        {
            SlideTowardZero(bgConfig, 3, 7, 0x14);
        }
    }

    // OFFSET 0x021D4E90
    private static void OpeningWorldScene_SlideFinalBgs(OpeningWorldSceneData scene)
    {
        BgConfig bgConfig = scene.BgConfig!;
        SlideTowardZero(bgConfig, 1, 5, 0xC);
        SlideTowardZero(bgConfig, 2, 6, 0xC);
        SlideTowardZero(bgConfig, 3, 7, 0xC);
    }

    // OFFSET 0x021D4F38
    private static void OpeningWorldScene_MoveCamera(OpeningWorldSceneData scene, int frame)
    {
        Camera camera = scene.Camera!;
        VecFx32 delta = new() { X = 0, Y = 0, Z = 0 };
        if (frame < FrameCameraLake)
        {
            if (Camera.Camera_GetFOV(camera) - 0x20 > CameraFov)
            {
                Camera.Camera_AdjustFOV(0xFFE0, camera);
            }
            else
            {
                Camera.Camera_SetFOV(CameraFov, camera);
            }
            delta.Z -= 0x2000;
            Camera.Camera_Move(delta, camera);
            return;
        }
        if (frame == FrameCameraLake)
        {
            VecFx32 target = VectorAt(CameraTargetLake);
            Camera.Camera_InitWithTarget(() => target, CameraDistance, AngleAt(CameraAngleAddress), CameraFov, 0, false, camera);
            Camera.Camera_Move(VectorAt(CameraOffsetLake), camera);
            Render3D.FlushBuffer();
            Render3D.SetFog(true, 0, 3, 0x1194);
            Render3D.SetFogColor(0x001F7FFF);
            uint[] table = new uint[8];
            for (int i = 0; i < 8; i++)
            {
                table[i] = (uint)((i * 0x10) | ((4 + i * 0x10) << 8) | ((8 + i * 0x10) << 16) | ((0xC + i * 0x10) << 24));
            }
            Render3D.SetFogTable(table);
            Video.SetBlendAlpha(0, 2, 0x21, 7, 9);
            Bg.Bg_SetPriority(1, 0);
            Bg.Bg_ToggleLayer(1, 1);
            return;
        }
        if (frame < FrameCameraLake + FrameCameraCitySpan)
        {
            delta.X += 0x4000;
            Camera.Camera_Move(delta, camera);
            Bg.Bg_ScheduleScroll(scene.BgConfig!, 1, OffsetAddX, 4);
            return;
        }
        if (frame == FrameCameraLake + FrameCameraCitySpan)
        {
            VecFx32 target = VectorAt(CameraTargetCity);
            Camera.Camera_InitWithTarget(() => target, CameraDistance, AngleAt(CameraAngleAddress), CameraFov, 0, false, camera);
            Camera.Camera_Move(VectorAt(CameraOffsetCity), camera);
            Camera.Camera_AdjustAngleAroundSelf(AngleAt(CameraTurnAddress), camera);
            Render3D.FlushBuffer();
            Render3D.SetFog(false, 0, 0, 0);
            Video.SetBlendControl(0, 0);
            Bg.Bg_ToggleLayer(1, 0);
            return;
        }
        if (frame >= FrameCameraZoomOut)
        {
            if (Camera.Camera_GetFOV(camera) + 0x18 < 0x4000)
            {
                Camera.Camera_AdjustFOV(0x18, camera);
            }
        }
        delta.X -= 0x2000;
        Camera.Camera_Move(delta, camera);
    }

    // OFFSET 0x021D513C
    private static void OpeningWorldScene_SwitchModels(OpeningData data, OpeningWorldSceneData scene, int frame)
    {
        if (frame > FrameFadeCity)
        {
            if (frame == FrameLoadCity)
            {
                if (!ScreenFadeControl.IsScreenFadeDone())
                {
                    ErrorHandling.GF_AssertFail();
                }
                OpeningWorldScene_FreeModels(data);
                OpeningWorldScene_LoadModelsNow(scene, 2);
                ScreenFadeControl.StartScreenFade(3, 1, 1, 0, 4, 1, HeapID);
            }
            return;
        }
        if (frame == FrameFadeCity)
        {
            ScreenFadeControl.StartScreenFade(3, 0, 0, 0, 4, 1, HeapID);
            return;
        }
        switch (frame - FrameLoadLake)
        {
            case 0:
                ScreenFadeControl.StartScreenFade(3, 0, 0, 0, 4, 1, HeapID);
                break;
            case 6:
                if (!ScreenFadeControl.IsScreenFadeDone())
                {
                    ErrorHandling.GF_AssertFail();
                }
                OpeningWorldScene_FreeModels(data);
                OpeningWorldScene_LoadModelsStep(scene, 1);
                break;
            case 7:
                OpeningWorldScene_LoadModelsStep(scene, 1);
                break;
            case 8:
                OpeningWorldScene_LoadModelsStep(scene, 1);
                break;
            case 9:
                if (!OpeningWorldScene_LoadModelsStep(scene, 1))
                {
                    ErrorHandling.GF_AssertFail();
                }
                ScreenFadeControl.StartScreenFade(3, 1, 1, 0, 4, 1, HeapID);
                break;
        }
    }

    // OFFSET 0x021D5254
    public static bool OpeningLegendScene_Run(OpeningData data)
    {
        OpeningLegendSceneData scene = data.Legend;
        if (data.Skipped != 0)
        {
            scene.State = 3;
        }
        switch (scene.State)
        {
            case 0:
                OpeningLegendScene_Init(data);
                scene.State++;
                break;
            case 1:
                OpeningLegendScene_LoadBgs(scene);
                scene.State++;
                break;
            case 2:
                if (OpeningLegendScene_Update(scene, data.Frame))
                {
                    scene.State++;
                }
                break;
            case 3:
                OpeningLegendScene_Free(data);
                return true;
        }
        return false;
    }

    // OFFSET 0x021D52C8
    private static void OpeningLegendScene_Init(OpeningData data)
    {
        OpeningLegendScene_SetBanks();
        OpeningLegendScene_InitBgs(data);
        data.Legend.BgConfig = data.BgConfig;
        ScreenFadeControl.SetScreenColorBrightness(0, White);
        ScreenFadeControl.SetScreenColorBrightness(1, White);
        Video.SetBlendControl(0, 0);
        data.Legend.Initialized = 1;
    }

    private static int ScrollPixels(int scroll)
    {
        return (scroll + (int)((uint)(scroll >> 11) >> 20)) >> 12;
    }

    // OFFSET 0x021D5308
    private static void OpeningLegendScene_LoadBgs(OpeningLegendSceneData scene)
    {
        BgConfig bgConfig = scene.BgConfig!;
        Graphics.Graphics_LoadPalette(GraphicsArchive, 0x63, PaletteMainBg, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayer(GraphicsArchive, 0x62, bgConfig, 2, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayer(GraphicsArchive, 0x61, bgConfig, 2, 0, 0, 0, HeapID);
        GXLayers.GXLayers_DisableEngineALayers();
        GXLayers.GXLayers_EngineASetLayers(0);
        GXLayers.GXLayers_EngineBSetLayers(0);
        scene.Scroll = -0x18000;
        Bg.Bg_SetOffset(bgConfig, 2, OffsetSetX, 0);
        Bg.Bg_SetOffset(bgConfig, 2, OffsetSetY, ScrollPixels(scene.Scroll));
        scene.BgsLoaded = 1;
    }

    // OFFSET 0x021D5390
    private static bool OpeningLegendScene_Update(OpeningLegendSceneData scene, int frame)
    {
        switch (scene.SubState)
        {
            case 0:
                if (frame >= FrameLegendFadeIn)
                {
                    ScreenFadeControl.StartScreenFade(0, 1, 1, White, 0x1E, 1, HeapID);
                    scene.SubState++;
                }
                break;
            case 1:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    GXLayers.GXLayers_EngineAToggleLayers(4, 1);
                    ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 0x5A, 1, HeapID);
                    scene.SubState++;
                }
                break;
            case 2:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    scene.SubState++;
                }
                break;
            case 3:
                if (frame >= FrameLegendFadeOut)
                {
                    ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 8, 1, HeapID);
                    scene.SubState++;
                }
                break;
            case 4:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    return true;
                }
                break;
        }
        scene.Scroll += 0x400;
        Bg.Bg_SetOffset(scene.BgConfig!, 2, OffsetSetY, ScrollPixels(scene.Scroll));
        return false;
    }

    // OFFSET 0x021D5478
    private static void OpeningLegendScene_Free(OpeningData data)
    {
        OpeningLegendSceneData scene = data.Legend;
        if (scene.Initialized != 0)
        {
            Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 2);
            data.BgConfig = null;
            scene.Initialized = 0;
        }
        if (scene.BgsLoaded != 0)
        {
            scene.BgsLoaded = 0;
        }
    }
}
