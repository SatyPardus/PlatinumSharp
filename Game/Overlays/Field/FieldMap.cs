using System.Numerics;
using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class FieldMap
{
    public const int UpdateTextures = 1;
    public const int UpdateLandData = 2;
    public const int UpdateDraw3D = 4;
    public const int UpdateMapPropAnimations = 8;
    public const int UpdateAll = 0xF;
    public const int FadeTypeIn = 1;
    public const int FadeTypeOut = 0;

    private const int Overlay = 5;
    private const int HeapIDApplication = 3;
    private const int HeapIDFieldMap = 4;
    private const int OverlayFieldExtension = 6;
    private const int OverlayGenericFeatures = 7;
    private const int OverlayGymFeatures = 8;
    private const int OverlayDistortionWorld = 9;
    private const int ExtensionOverlayGym = 0;
    private const int ExtensionOverlayGeneric = 1;
    private const int ExtensionOverlayDistortionWorld = 2;
    private const int DynamicMapFeaturesNone = 0;
    private const int DynamicMapFeaturesDistortionWorld = 9;
    private const int InitStateReset = 0;
    private const int InitStateLoad = 1;
    private const int InitStateBottomScreen = 2;
    private const int InitStateDone = 3;
    private const int MapLoadTypeOverworld = 0;
    private const int MapLoadTypeUnderground = 1;
    private const int MapLoadTypeUnion = 2;
    private const int LocationMapHeaderID = 0x00;
    private const int LocationX = 0x08;
    private const int LocationZ = 0x0C;
    private const int LocationFaceDirection = 0x10;
    private const int MapTileCount = 32;
    private const int RenderTaskCapacity = 8;
    private const uint VramTransferCapacity = 0x80;
    private const int BillboardListCount = 4;
    private const int InitScriptFirst = 4;
    private const int InitScriptResume = 3;
    private const int PoketchEventPlayerMoved = 1;
    private const int NarcMapObjectsToPreload = 0x60;
    private const int MapObjectResourceCapacity = 0x20;
    private const int MapObjectExtraResources = 3;
    private const int MapObjectTextureCount = 10;
    private const int MapObjectTextureCountUnion = 5;
    private const int FieldEffectRendererCapacity = 0x22;
    private const int FieldEffectAnimManagers = 0x50;
    private const int FieldEffectAnimManagersDistortionWorld = 0x70;
    private const int DynamicTerrainHeightCapacity = 8;
    private const int OamCount = 0x7C;
    private const int AffineOamCount = 0x1F;
    private const int PlttTransferCapacity = 0x14;
    private const int VramTransferMode32K = 0x10;
    private const int FadeDuration = 6;
    private const int SortModeAuto = 0;
    private const int ProjectionDepthShift = 4 * Fx32.One;
    private const int CullModeBack = 2;
    private const int AlphaOpaque = 0x1F;
    private const uint PolygonAttrFog = 0x8000;
    private const uint ModelAttributesApplyAll = 0x400000;
    private const uint BankTemplate = 0x021F8988;
    private const uint GraphicsModesTemplate = 0x021F8924;
    private const uint Bg1Template = 0x021F8950;
    private const uint Bg2Template = 0x021F8934;
    private const uint Bg3Template = 0x021F896C;
    private const uint CharTransferTemplateAddress = 0x021F8914;
    private const uint UndergroundFieldEffectRenderers = 0x021FF7D0;
    private const uint DistortionWorldFieldEffectRenderers = 0x021FF6B8;
    private const uint DefaultFieldEffectRenderers = 0x021FF744;

    // OFFSET 0x021D0D80
    private static void FieldMap_VBlank(object? data)
    {
        FieldSystem fieldSystem = (FieldSystem)data!;
        Bg.Bg_RunScheduledUpdates(fieldSystem.BgConfig!);
        VramTransfer.VramTransfer_Process();
        RenderOam.RenderOam_Transfer();
        MapObjectRenderData renderData = MapObjectManager.MapObjectMan_GetRenderData(fieldSystem.MapObjectManager!);
        BillboardList? billboardList = MapObjectRenderResources.MapObjectRenderData_GetBillboardList(renderData);
        Billboards.BillboardList_ResetRedraw(billboardList!);
    }

    // OFFSET 0x021D0DA4
    public static int FieldMap_Init(ApplicationManager appMan, ref int state)
    {
        FieldSystem fieldSystem = (FieldSystem)ApplicationManager.ApplicationManager_Args(appMan)!;
        int done = 0;
        switch (state)
        {
            case InitStateReset:
                GameSystem.SetVBlankCallback(null, null);
                GameSystem.DisableHBlank();
                Video.SetBlendControl(0, 0);
                Video.SetBlendControl(1, 0);
                ScreenFadeControl.ResetVisibleHardwareWindows(0);
                ScreenFadeControl.ResetVisibleHardwareWindows(1);
                FieldMap_EnableAllUpdates(fieldSystem);
                FieldMapChange.FieldMapChange_Set3DDisplay(fieldSystem);
                if (fieldSystem.MapLoadMode!.Unknown20 != 0)
                {
                    GameOverlay.Overlay_LoadByID(OverlayFieldExtension, GameOverlay.LoadNoInitAsync);
                    switch (FieldMap_GetExtensionOverlay(fieldSystem))
                    {
                        case ExtensionOverlayGym:
                            GameOverlay.Overlay_LoadByID(OverlayGymFeatures, GameOverlay.LoadNoInitAsync);
                            break;
                        case ExtensionOverlayGeneric:
                            GameOverlay.Overlay_LoadByID(OverlayGenericFeatures, GameOverlay.LoadNoInitAsync);
                            break;
                        case ExtensionOverlayDistortionWorld:
                            GameOverlay.Overlay_LoadByID(OverlayDistortionWorld, GameOverlay.LoadNoInitAsync);
                            break;
                    }
                }
                Heap.Heap_Create(HeapIDApplication, HeapIDFieldMap, fieldSystem.MapLoadMode.FieldMapHeapSize);
                if (fieldSystem.FieldMapContext != null)
                {
                    ErrorHandling.GF_AssertFail();
                }
                fieldSystem.FieldMapContext = new FieldMapContext();
                fieldSystem.FieldMapContext.RenderTaskManager = RenderTaskManager.RenderTaskManager_New(fieldSystem, HeapIDFieldMap, RenderTaskCapacity);
                FieldMap_SetVRAMBanks();
                VramTransfer.VramTransfer_New(VramTransferCapacity, HeapIDFieldMap);
                Billboards.BillboardLists_Create(BillboardListCount, HeapIDFieldMap);
                Easy3D.Easy3D_Init(HeapIDFieldMap);
                FieldMap_InitSpriteTransfers();
                FieldMap_InitOam();
                GXLayers.GXLayers_SwapDisplay();
                fieldSystem.BgConfig = Bg.BgConfig_New(HeapIDFieldMap);
                FieldMap_InitBgs(fieldSystem.BgConfig);
                FieldMessage.FieldMessage_LoadTextPalettes(0, 1);
                ScriptManager.FieldSystem_RunInitScript(fieldSystem, InitScriptFirst);
                break;
            case InitStateLoad:
                FieldMap_InitGraphicsAndManagers(fieldSystem);
                AreaData.AreaDataManager_Load(fieldSystem.AreaDataManager);
                fieldSystem.MapPropManager = MapProps.MapPropManager_New(HeapIDFieldMap);
                SecretBaseProps.FieldSystem_LoadSecretBaseGoodProps(fieldSystem);
                FieldMap_InitLandManager(fieldSystem);
                FieldMap_InitObjectsAndEffects(fieldSystem);
                FieldMap_InitSceneManagers(fieldSystem);
                if (fieldSystem.FieldMapContext!.WeatherSystem != null)
                {
                    int weather = FieldPlayerState.FieldPlayerState_GetWeather(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
                    FieldWeather.WeatherSystem_Set(fieldSystem.FieldMapContext.WeatherSystem, weather);
                }
                FieldBgm.FieldBGM_PlayEffectiveForMapHeader(fieldSystem, (int)fieldSystem.Location!.U32(LocationMapHeaderID));
                ScriptManager.FieldSystem_RunInitScript(fieldSystem, InitScriptResume);
                fieldSystem.FieldMapContext.HBlankSystem = HBlankSystem.HBlankSystem_New(HeapIDFieldMap);
                HBlankSystem.HBlankSystem_Start(fieldSystem.FieldMapContext.HBlankSystem);
                fieldSystem.FieldMapContext.HBlankScreenEffect = FieldPoisonEffect.FieldPoisonEffect_New(HeapIDFieldMap, fieldSystem.FieldMapContext.HBlankSystem);
                break;
            case InitStateBottomScreen:
                FieldBottomScreen.FieldSystem_InitBottomScreen(fieldSystem);
                break;
            case InitStateDone:
                if (FieldBottomScreen.FieldSystem_IsBottomScreenRunning(fieldSystem) != 0)
                {
                    done = 1;
                    fieldSystem.RunningFieldMap = 1;
                }
                break;
        }
        state++;
        return done;
    }

    // OFFSET 0x021D0F68
    public static int FieldMap_Main(ApplicationManager appMan, ref int state)
    {
        FieldSystem fieldSystem = (FieldSystem)ApplicationManager.ApplicationManager_Args(appMan)!;
        if (FieldMap_UpdateLocationToPlayerPosition(fieldSystem))
        {
            BerryPatchManager.BerryPatches_UpdateGrowthStates(fieldSystem);
            FieldMap_PushMapHistory(fieldSystem);
            FieldPoketch.FieldSystem_SendPoketchEvent(fieldSystem, PoketchEventPlayerMoved, 1);
            FieldMap_ChangeZone(fieldSystem);
        }
        FieldMap_Update(fieldSystem, (byte)fieldSystem.FieldMapUpdateFlags);
        if (fieldSystem.RunningFieldMap != 0)
        {
            return 0;
        }
        return 1;
    }

    // OFFSET 0x021D0FB4
    public static int FieldMap_Exit(ApplicationManager appMan, ref int state)
    {
        FieldSystem fieldSystem = (FieldSystem)ApplicationManager.ApplicationManager_Args(appMan)!;
        LandDataManager.LandDataManager_Tick(fieldSystem, fieldSystem.LandDataManager);
        switch (state)
        {
            case 0:
                DynamicMapFeatures.DynamicMapFeatures_Free(fieldSystem);
                LandDataManager.LandDataManager_ForgetTrackedTarget(fieldSystem.LandDataManager);
                fieldSystem.Location!.W32(LocationX, (uint)PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar));
                fieldSystem.Location.W32(LocationZ, (uint)PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar));
                fieldSystem.Location.W32(LocationFaceDirection, (uint)PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar));
                DynamicTerrainHeightManager.DynamicTerrainHeightManager_Free(fieldSystem.DynamicTerrainHeightManager);
                if (fieldSystem.MapPropAnimationManager == null)
                {
                    ErrorHandling.GF_AssertFail();
                }
                LandDataManager.LandDataManager_End(fieldSystem.LandDataManager);
                MapPropAnimations.MapPropAnimationManager_UnloadAllAnimations(fieldSystem.MapPropAnimationManager);
                MapPropAnimations.MapPropAnimationManager_Free(fieldSystem.MapPropAnimationManager);
                MapPropAnimations.MapPropOneShotAnimationManager_Free(ref fieldSystem.MapPropOneShotAnimationManager);
                FieldTextureResources.FieldTextureManager_FreeAllSlots(fieldSystem.FieldMapContext!.TextureResourceManager);
                FieldTextureResources.FieldTextureManager_Destroy(fieldSystem.FieldMapContext.TextureResourceManager);
                fieldSystem.FieldMapContext.TextureResourceManager = null;
                MapObjectManager.MapObjectMan_SuspendDrawAll(fieldSystem.MapObjectManager);
                MapObjectRender.MapObjectMan_FreeRender(fieldSystem.MapObjectManager);
                MapObjectManager.MapObjectMan_StopAllMovement(fieldSystem.MapObjectManager);
                FieldEffects.FieldEffectManager_Free(fieldSystem.FieldEffectManager);
                MapObjectsToPreload_Free(fieldSystem.MapObjectsToPreload!);
                fieldSystem.MapObjectsToPreload = null;
                MapProps.MapPropManager_Free(fieldSystem.MapPropManager);
                state++;
                break;
            case 1:
                if (LandDataManager.LandDataManager_HasEnded(fieldSystem.LandDataManager))
                {
                    AreaData.AreaDataManager_Free(ref fieldSystem.AreaDataManager);
                    LandDataManager.LandDataManager_FreeNARCAndLoadedMapBuffers(fieldSystem.LandDataManager);
                    HoneyTrees.HoneyTree_FreeShakeData(ref fieldSystem.HoneyTreeShakeList);
                    FieldCamera.FieldCamera_Delete(fieldSystem);
                    AreaLight.AreaLightManager_Free(ref fieldSystem.AreaLightManager);
                    Signposts.Signpost_Free(fieldSystem.Signpost);
                    MapNamePopUps.MapNamePopUp_Destroy(fieldSystem.FieldMapContext!.MapNamePopUp);
                    if (fieldSystem.FieldMapContext.WeatherSystem != null)
                    {
                        FieldWeather.WeatherSystem_Free(fieldSystem.FieldMapContext.WeatherSystem);
                    }
                    FieldPoisonEffect.FieldPoisonEffect_Free(fieldSystem.FieldMapContext.HBlankScreenEffect);
                    HBlankSystem.HBlankSystem_Delete(fieldSystem.FieldMapContext.HBlankSystem);
                    BerryPatchManager.BerryPatchManager_Free(fieldSystem.FieldMapContext.BerryPatchManager);
                    FogManager.FogManager_Free(ref fieldSystem.FogManager);
                    ModelAttributes.ModelAttributes_Free(ref fieldSystem.AreaModelAttributes);
                    FieldMap_FreeOam();
                    FieldMap_FreeBgs(fieldSystem.BgConfig!);
                    FieldBottomScreen.FieldSystem_EndBottomScreen(fieldSystem);
                    state++;
                }
                break;
            case 2:
                if (FieldBottomScreen.FieldSystem_IsBottomScreenDone(fieldSystem) != 0)
                {
                    FieldMap_FreeSpriteTransfers();
                    Billboards.BillboardLists_Delete();
                    VramTransfer.VramTransfer_Free();
                    Easy3D.Easy3D_Shutdown();
                    RenderTaskManager.RenderTaskManager_Free(fieldSystem.FieldMapContext!.RenderTaskManager);
                    GameSystem.SetVBlankCallback(null, null);
                    fieldSystem.FieldMapContext = null;
                    Heap.Heap_Destroy(HeapIDFieldMap);
                    if (fieldSystem.MapLoadMode!.Unknown20 != 0)
                    {
                        GameOverlay.Overlay_UnloadByID(OverlayFieldExtension);
                        GameOverlay.Overlay_UnloadByID(OverlayGymFeatures);
                        GameOverlay.Overlay_UnloadByID(OverlayGenericFeatures);
                        GameOverlay.Overlay_UnloadByID(OverlayDistortionWorld);
                    }
                    return 1;
                }
                break;
        }
        return 0;
    }

    // OFFSET 0x021D1178
    private static int FieldMap_GetExtensionOverlay(FieldSystem fieldSystem)
    {
        PersistedMapFeatures features = MiscSaveBlock.SaveData_GetPersistedMapFeatures(FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!);
        int id = PersistedMapFeatures.PersistedMapFeatures_GetID(features);
        if (id == DynamicMapFeaturesNone)
        {
            return ExtensionOverlayGeneric;
        }
        if (id == DynamicMapFeaturesDistortionWorld)
        {
            return ExtensionOverlayDistortionWorld;
        }
        return ExtensionOverlayGym;
    }

    // OFFSET 0x021D119C
    private static bool FieldMap_UpdateLocationToPlayerPosition(FieldSystem fieldSystem)
    {
        int x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar);
        int z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar);
        BufferView location = fieldSystem.Location!;
        if (x != (int)location.U32(LocationX) || z != (int)location.U32(LocationZ))
        {
            location.W32(LocationX, (uint)x);
            fieldSystem.Location!.W32(LocationZ, (uint)z);
            return true;
        }
        return false;
    }

    // OFFSET 0x021D11CC
    private static bool FieldMap_ChangeZone(FieldSystem fieldSystem)
    {
        if (FieldMap_InDistortionWorld(fieldSystem))
        {
            return false;
        }
        int x = (PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar) - LandDataManager.LandDataManager_GetOffsetTileX(fieldSystem.LandDataManager)) / MapTileCount;
        int z = (PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar) - LandDataManager.LandDataManager_GetOffsetTileZ(fieldSystem.LandDataManager)) / MapTileCount;
        int newMapHeaderID = MapMatrix.MapMatrix_GetMapHeaderIDAtCoords((MapMatrix)fieldSystem.MapMatrix!, x, z);
        int oldMapHeaderID = (int)fieldSystem.Location!.U32(LocationMapHeaderID);
        if (newMapHeaderID == oldMapHeaderID)
        {
            return false;
        }
        FieldPlayerState fieldState = FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!);
        fieldSystem.Location.W32(LocationMapHeaderID, (uint)newMapHeaderID);
        MapHeaderData.MapHeaderData_Load(fieldSystem, newMapHeaderID);
        FieldMapChange.FieldMapChange_UpdateGameData(fieldSystem, true);
        int numObjectEvents = (int)MapHeaderData.MapHeaderData_GetNumObjectEvents(fieldSystem);
        ObjectEvent[]? objectEvents = MapHeaderData.MapHeaderData_GetObjectEvents(fieldSystem);
        MapObjectManager.MapObjectMan_DeleteObjectsOnMapChange(fieldSystem.MapObjectManager, oldMapHeaderID, newMapHeaderID, numObjectEvents, objectEvents);
        RadarChain.RadarChain_Clear(fieldSystem.RadarChain);
        int bgmID = FieldBgm.FieldBGM_GetEffective(fieldSystem, (int)fieldSystem.Location.U32(LocationMapHeaderID));
        FieldBgm.FieldBGM_TryFadeOut(fieldSystem, bgmID, 1);
        MapHeaderData.MapHeaderData_LoadObjectEventsIntoMapObjects(fieldSystem);
        if (fieldSystem.FieldMapContext!.WeatherSystem != null)
        {
            FieldWeather.WeatherSystem_Change(fieldSystem.FieldMapContext.WeatherSystem, FieldPlayerState.FieldPlayerState_GetWeather(fieldState));
        }
        int oldMapLabelTextID = MapHeader.MapHeader_GetMapLabelTextID(oldMapHeaderID);
        int newMapLabelTextID = MapHeader.MapHeader_GetMapLabelTextID(newMapHeaderID);
        int mapLabelWindowID = MapHeader.MapHeader_GetMapLabelWindowID(newMapHeaderID);
        if (oldMapLabelTextID != newMapLabelTextID)
        {
            if (mapLabelWindowID != 0)
            {
                mapLabelWindowID--;
            }
            MapNamePopUps.MapNamePopUp_Show(fieldSystem.FieldMapContext.MapNamePopUp, newMapLabelTextID, mapLabelWindowID);
        }
        return true;
    }

    // OFFSET 0x021D12D0
    public static void FieldMap_ChangeZoneDistortionWorld(FieldSystem fieldSystem, int mapHeaderID)
    {
        int oldMapHeaderID = (int)fieldSystem.Location!.U32(LocationMapHeaderID);
        FieldPlayerState fieldState = FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!);
        fieldSystem.Location.W32(LocationMapHeaderID, (uint)mapHeaderID);
        MapHeaderData.MapHeaderData_Load(fieldSystem, mapHeaderID);
        FieldMapChange.FieldMapChange_UpdateGameDataDistortionWorld(fieldSystem, true);
        int numObjectEvents = (int)MapHeaderData.MapHeaderData_GetNumObjectEvents(fieldSystem);
        ObjectEvent[]? objectEvents = MapHeaderData.MapHeaderData_GetObjectEvents(fieldSystem);
        MapObjectManager.MapObjectMan_DeleteObjectsOnMapChange(fieldSystem.MapObjectManager, oldMapHeaderID, mapHeaderID, numObjectEvents, objectEvents);
        int bgmID = FieldBgm.FieldBGM_GetEffective(fieldSystem, (int)fieldSystem.Location.U32(LocationMapHeaderID));
        FieldBgm.FieldBGM_TryFadeOut(fieldSystem, bgmID, 1);
        MapHeaderData.MapHeaderData_LoadObjectEventsIntoMapObjects(fieldSystem);
        if (fieldSystem.FieldMapContext!.WeatherSystem != null)
        {
            FieldWeather.WeatherSystem_Change(fieldSystem.FieldMapContext.WeatherSystem, FieldPlayerState.FieldPlayerState_GetWeather(fieldState));
        }
    }

    // OFFSET 0x021D134C
    private static void FieldMap_Update(FieldSystem fieldSystem, byte flags)
    {
        if (!FieldTask.FieldSystem_IsRunningTask(fieldSystem))
        {
            FieldTime.FieldSystem_UpdateGameTime(fieldSystem);
        }
        AreaLight.AreaLightManager_UpdateActiveTemplate(fieldSystem.AreaLightManager);
        Signposts.Signpost_DoCurrentCommand(fieldSystem);
        if ((flags & UpdateTextures) != 0)
        {
            FieldTextureResources.FieldTextureManager_Update(fieldSystem.FieldMapContext!.TextureResourceManager);
        }
        if ((flags & UpdateMapPropAnimations) != 0)
        {
            MapPropAnimations.MapPropAnimationManager_AdvanceAnimations(fieldSystem.MapPropAnimationManager);
        }
        if ((flags & UpdateLandData) != 0)
        {
            LandDataManager.LandDataManager_Tick(fieldSystem, fieldSystem.LandDataManager);
            if (FieldMap_InDistortionWorld(fieldSystem))
            {
                ov9_0224CA5C(fieldSystem);
            }
        }
        if ((flags & UpdateDraw3D) != 0)
        {
            FieldMap_Draw3D(fieldSystem);
        }
    }

    // OFFSET 0x021D13B4
    private static void FieldMap_PushMapHistory(FieldSystem fieldSystem)
    {
        if (!MapHeader.MapHeader_IsOnMainMatrix((int)fieldSystem.Location!.U32(LocationMapHeaderID)))
        {
            return;
        }
        object? mapHistory = FieldPlayerState.FieldPlayerState_GetMapHistory(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        int x = (PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar) - LandDataManager.LandDataManager_GetOffsetTileX(fieldSystem.LandDataManager)) / MapTileCount;
        int z = (PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar) - LandDataManager.LandDataManager_GetOffsetTileZ(fieldSystem.LandDataManager)) / MapTileCount;
        int faceDirection = PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar);
        OverworldMapHistory.OverworldMapHistory_Push(mapHistory, x, z, faceDirection);
    }

    // OFFSET 0x021D1414
    private static void FieldMap_SetVRAMBanks()
    {
        GXLayers.GXLayers_SetBanks(RomStructs.Banks(RomData.Overlay(Overlay, BankTemplate, RomStructs.BanksSize), 0));
    }

    // OFFSET 0x021D1434
    public static void FieldMap_ReinitBgs(BgConfig bgConfig)
    {
        FieldMap_InitBgs(bgConfig);
    }

    // OFFSET 0x021D143C
    public static void FieldMap_ReleaseBgs(BgConfig bgConfig)
    {
        FieldMap_FreeBgs(bgConfig);
    }

    // OFFSET 0x021D1444
    private static void FieldMap_InitBgs(BgConfig bgConfig)
    {
        Bg.SetAllGraphicsModes(RomStructs.GraphicsModes(RomData.Overlay(Overlay, GraphicsModesTemplate, RomStructs.GraphicsModesSize), 0));
        Bg.Bg_InitFromTemplate(bgConfig, 1, RomStructs.BgTemplate(RomData.Overlay(Overlay, Bg1Template, RomStructs.BgTemplateSize), 0), 0);
        Bg.Bg_ClearTilesRange(1, 0x20, 0, HeapIDFieldMap);
        Bg.Bg_ClearTilemap(bgConfig, 1);
        Bg.Bg_InitFromTemplate(bgConfig, 2, RomStructs.BgTemplate(RomData.Overlay(Overlay, Bg2Template, RomStructs.BgTemplateSize), 0), 0);
        Bg.Bg_ClearTilesRange(2, 0x20, 0, HeapIDFieldMap);
        Bg.Bg_ClearTilemap(bgConfig, 2);
        Bg.Bg_InitFromTemplate(bgConfig, 3, RomStructs.BgTemplate(RomData.Overlay(Overlay, Bg3Template, RomStructs.BgTemplateSize), 0), 0);
        Bg.Bg_ClearTilesRange(3, 0x20, 0, HeapIDFieldMap);
        Bg.Bg_ClearTilemap(bgConfig, 3);
        Video.LoadPaletteRange(Video.MainBackground, new ushort[1], 0, 0, 2);
    }

    // OFFSET 0x021D1524
    private static void FieldMap_FreeBgs(BgConfig bgConfig)
    {
        GXLayers.GXLayers_EngineAToggleLayers(0xF, 0);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 1);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 2);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 3);
    }

    // OFFSET 0x021D154C
    private static void FieldMap_InitOam()
    {
        OamManager.InitModule();
        RenderOam.RenderOam_Init(0, OamCount, 0, AffineOamCount, 0, OamCount, 0, AffineOamCount, HeapIDFieldMap);
    }

    // OFFSET 0x021D1570
    private static void FieldMap_FreeOam()
    {
        RenderOam.RenderOam_Free();
    }

    // OFFSET 0x021D1578
    private static void FieldMap_InitModelAttributes(ModelAttributes modelAttributes)
    {
        ModelAttributes.ModelAttributes_SetPolygonMode(modelAttributes, 0, 0);
        ModelAttributes.ModelAttributes_SetCullMode(modelAttributes, CullModeBack, 0);
        ModelAttributes.ModelAttributes_SetAlpha(modelAttributes, AlphaOpaque, 0);
        ModelAttributes.ModelAttributes_SetMiscAttrEnabled(modelAttributes, (int)PolygonAttrFog, 1, 0);
        ModelAttributes.ModelAttributes_ApplyGlobal(modelAttributes, (int)ModelAttributesApplyAll);
    }

    // OFFSET 0x021D15B4
    public static void FieldMap_InitSpriteTransfers()
    {
        uint[] words = RomData.OverlayU32(Overlay, CharTransferTemplateAddress, 4);
        CharTransferTemplate template = new()
        {
            MaxTasks = (int)words[0],
            SizeMain = (int)words[1],
            SizeSub = (int)words[2],
            HeapID = (int)words[3],
        };
        CharTransfer.CharTransfer_InitWithVramModes(template, VramTransferMode32K, VramTransferMode32K);
        PlttTransfer.PlttTransfer_Init(PlttTransferCapacity, HeapIDFieldMap);
        CharTransfer.CharTransfer_ClearBuffers();
        PlttTransfer.PlttTransfer_Clear();
    }

    // OFFSET 0x021D15E8
    public static void FieldMap_FreeSpriteTransfers()
    {
        CharTransfer.CharTransfer_Free();
        PlttTransfer.PlttTransfer_Free();
    }

    // OFFSET 0x021D15F4
    private static void FieldMap_Draw3D(FieldSystem fieldSystem)
    {
        G3SwapRequest.G3_ResetG3X();
        if (fieldSystem.CameraRollEnabled == 1)
        {
            if (FieldMap_InDistortionWorld(fieldSystem))
            {
                ov9_02249F9C(fieldSystem);
            }
            Camera.Camera_ComputeViewMatrixWithRoll();
        }
        else
        {
            Camera.Camera_ComputeViewMatrix();
        }
        RadarChain.PokeRadar_ClearIfAllOutOfView(fieldSystem);
        LandDataManager.LandDataManager_RenderLoadedMaps(fieldSystem.LandDataManager, fieldSystem.AreaModelAttributes);
        if (FieldMap_InDistortionWorld(fieldSystem))
        {
            ov9_0224CA50(fieldSystem);
        }
        MapProps.MapPropManager_Render2(fieldSystem.MapPropManager, fieldSystem.AreaDataManager);
        Matrix4x4 projection = Render3D.Projection;
        Matrix4x4 shifted = projection;
        int depthScale = (int)MathF.Round(projection.M33 * Fx32.One);
        shifted.M43 += Fx32.Mul(depthScale, ProjectionDepthShift) / (float)Fx32.One;
        Render3D.SetGlobalProjection(shifted);
        Render3D.FlushGlobalState();
        FieldEffects.FieldEffectManager_Render(fieldSystem.FieldEffectManager);
        Billboards.BillboardLists_Draw();
        if (FieldMap_InDistortionWorld(fieldSystem))
        {
            ov9_02250780(fieldSystem);
        }
        Render3D.SetGlobalProjection(projection);
        Render3D.FlushGlobalState();
        RenderTaskManager.RenderTaskManager_Draw(fieldSystem.FieldMapContext!.RenderTaskManager);
        G3SwapRequest.G3_RequestSwapBuffers(SortModeAuto, Render3D.ProjectionCache);
    }

    // OFFSET 0x021D16F4
    public static void FieldMap_SetDraw3DEnabled(FieldSystem fieldSystem, bool enable)
    {
        if (enable)
        {
            fieldSystem.FieldMapUpdateFlags |= UpdateDraw3D;
        }
        else
        {
            fieldSystem.FieldMapUpdateFlags &= ~(uint)UpdateDraw3D;
        }
    }

    // OFFSET 0x021D1718
    public static void FieldMap_SetTextureUpdateEnabled(FieldSystem fieldSystem, bool enable)
    {
        if (enable)
        {
            fieldSystem.FieldMapUpdateFlags |= UpdateTextures;
        }
        else
        {
            fieldSystem.FieldMapUpdateFlags &= ~(uint)UpdateTextures;
        }
    }

    // OFFSET 0x021D173C
    private static void FieldMap_EnableAllUpdates(FieldSystem fieldSystem)
    {
        fieldSystem.FieldMapUpdateFlags = UpdateAll;
    }

    // OFFSET 0x021D1744
    public static void FieldMap_FadeScreen(byte fadeType)
    {
        if (fadeType == FadeTypeIn)
        {
            ScreenFadeControl.StartScreenFade(0, 1, 1, 0, FadeDuration, 1, HeapIDFieldMap);
        }
        else if (fadeType == FadeTypeOut)
        {
            ScreenFadeControl.StartScreenFade(0, 0, 0, 0, FadeDuration, 1, HeapIDFieldMap);
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021D1790
    private static void FieldMap_InitGraphicsAndManagers(FieldSystem fieldSystem)
    {
        GXLayers.GXLayers_EngineAToggleLayers(1, 0);
        Render3D.SwapBuffers(SortModeAuto, Render3D.ProjectionCache);
        fieldSystem.MapPropAnimationManager = MapPropAnimations.MapPropAnimationManager_New();
        fieldSystem.MapPropOneShotAnimationManager = MapPropAnimations.MapPropOneShotAnimationManager_New();
        int areaDataArchiveID = MapHeader.MapHeader_GetAreaDataArchiveID((int)fieldSystem.Location!.U32(LocationMapHeaderID));
        fieldSystem.AreaDataManager = AreaData.AreaDataManager_Alloc(areaDataArchiveID, fieldSystem.MapPropAnimationManager);
        int preloadArchiveID = MapHeader.MapHeader_GetPreloadedMapObjectsArchiveID((int)fieldSystem.Location.U32(LocationMapHeaderID));
        if (fieldSystem.MapObjectsToPreload != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        fieldSystem.MapObjectsToPreload = MapObjectsToPreload_Fetch(HeapIDFieldMap, preloadArchiveID);
    }

    // OFFSET 0x021D17EC
    private static void FieldMap_InitLandManager(FieldSystem fieldSystem)
    {
        fieldSystem.LandDataManager = LandDataManager.LandDataManager_New(fieldSystem.MapMatrix, fieldSystem.AreaDataManager, fieldSystem.MapPropAnimationManager, fieldSystem.SkipMapAttributes);
        if (FieldMap_InDistortionWorld(fieldSystem))
        {
            int offsetX = 0;
            int offsetY = 0;
            int offsetZ = 0;
            ov9_02251094((int)fieldSystem.Location!.U32(LocationMapHeaderID), ref offsetX, ref offsetY, ref offsetZ);
            LandDataManager.LandDataManager_DistortionWorldSetOffsets(fieldSystem.LandDataManager, offsetX, offsetY, offsetZ);
            LandDataManager.LandDataManager_SetInDistortionWorld(fieldSystem.LandDataManager, 1);
            LandDataManager.LandDataManager_SetSkipMapProps(fieldSystem.LandDataManager, 1);
        }
        fieldSystem.DynamicTerrainHeightManager = DynamicTerrainHeightManager.DynamicTerrainHeightManager_New(DynamicTerrainHeightCapacity, HeapIDFieldMap);
        fieldSystem.HoneyTreeShakeList = HoneyTrees.HoneyTree_ShakeDataInit();
        if (fieldSystem.MapLoadType == MapLoadTypeOverworld)
        {
            LandDataManager.LandDataManager_SetMapLoadedCallback(fieldSystem.LandDataManager, HoneyTrees.HoneyTree_OnMapLoaded, fieldSystem);
        }
        LandDataManager.LandDataManager_InitialLoad(fieldSystem.LandDataManager, (int)fieldSystem.Location!.U32(LocationX), (int)fieldSystem.Location.U32(LocationZ));
    }

    // OFFSET 0x021D1878
    private static void FieldMap_InitObjectsAndEffects(FieldSystem fieldSystem)
    {
        fieldSystem.FieldEffectManager = FieldEffects.FieldEffectManager_New(fieldSystem, FieldEffectRendererCapacity, HeapIDFieldMap);
        int animManagerCount = FieldEffectAnimManagers;
        if (FieldMap_InDistortionWorld(fieldSystem))
        {
            animManagerCount = FieldEffectAnimManagersDistortionWorld;
        }
        FieldEffects.FieldEffectManager_InitAnimManagerList(fieldSystem.FieldEffectManager, animManagerCount);
        FieldEffects.FieldEffectManager_InitBillboardManager(fieldSystem.FieldEffectManager, HeapIDFieldMap, 0x20, 0x20, 0x20, 0x20, 0x5000, 0x800, 0x10000);
        if ((uint)(fieldSystem.MapLoadType - MapLoadTypeUnderground) <= 1)
        {
            MapObjectManager.MapObjectMan_SetShadowsEnabled(fieldSystem.MapObjectManager, 0);
        }
        uint renderers;
        if (fieldSystem.MapLoadType == MapLoadTypeUnderground)
        {
            renderers = UndergroundFieldEffectRenderers;
        }
        else if (FieldMap_InDistortionWorld(fieldSystem))
        {
            renderers = DistortionWorldFieldEffectRenderers;
        }
        else
        {
            renderers = DefaultFieldEffectRenderers;
        }
        FieldEffects.FieldEffectManager_InitRenderers(fieldSystem.FieldEffectManager, renderers);
        int textureCount = MapObjectTextureCount;
        if (fieldSystem.MapLoadType == MapLoadTypeUnion)
        {
            textureCount = MapObjectTextureCountUnion;
        }
        int preloadCount = MapObjectsToPreload_GetCount(fieldSystem.MapObjectsToPreload!);
        int[] preloadIDs = MapObjectsToPreload_GetIDs(fieldSystem.MapObjectsToPreload!);
        MapObjectRender.MapObjectMan_InitRender(fieldSystem.MapObjectManager, MapObjectResourceCapacity, preloadCount + MapObjectExtraResources, preloadIDs, textureCount);
        MapObjectModelEffect.FieldEffectManager_LoadDefaultMapObjectModels(fieldSystem.FieldEffectManager);
        PersistedMapFeatures features = MiscSaveBlock.SaveData_GetPersistedMapFeatures(FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!);
        PlayerAvatar.PlayerAvatar_InitMapFeatures(fieldSystem.PlayerAvatar, PersistedMapFeatures.PersistedMapFeatures_GetID(features));
        MapObjectManager.MapObjectMan_ResumeDrawAll(fieldSystem.MapObjectManager);
        CommPlayerManager.CommPlayerMan_SyncLinkPlayerMapObjects();
        MapObjectManager.MapObjectMan_ResumeAllMovement(fieldSystem.MapObjectManager);
        LandDataManager.LandDataManager_TrackTarget(() => PlayerAvatar.PlayerAvatar_GetPos(fieldSystem.PlayerAvatar!), fieldSystem.LandDataManager);
        fieldSystem.FieldMapContext!.BerryPatchManager = BerryPatchManager.BerryPatchManager_New(fieldSystem, HeapIDFieldMap);
    }

    // OFFSET 0x021D1968
    private static void FieldMap_InitSceneManagers(FieldSystem fieldSystem)
    {
        GXLayers.GXLayers_EngineAToggleLayers(1, 1);
        GXLayers.GXLayers_TurnBothDispOn();
        fieldSystem.AreaModelAttributes = ModelAttributes.ModelAttributes_New();
        FieldMap_InitModelAttributes(fieldSystem.AreaModelAttributes);
        fieldSystem.FogManager = FogManager.FogManager_New();
        int cameraType = FieldPlayerState.FieldPlayerState_GetCameraType(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        FieldCamera.FieldCamera_Create(() => PlayerAvatar.PlayerAvatar_GetPos(fieldSystem.PlayerAvatar!), fieldSystem, cameraType, 1);
        fieldSystem.AreaLightManager = AreaLight.AreaLightManager_New(fieldSystem.AreaModelAttributes, AreaData.AreaDataManager_GetAreaLightArchiveID(fieldSystem.AreaDataManager));
        if (FieldMap_InDistortionWorld(fieldSystem))
        {
            fieldSystem.FieldMapContext!.WeatherSystem = null;
        }
        else
        {
            fieldSystem.FieldMapContext!.WeatherSystem = FieldWeather.WeatherSystem_New(fieldSystem);
        }
        fieldSystem.FieldMapContext.MapNamePopUp = MapNamePopUps.MapNamePopUp_Create(fieldSystem.BgConfig);
        fieldSystem.Signpost = Signposts.Signpost_Init(HeapIDFieldMap);
        fieldSystem.FieldMapContext.TextureResourceManager = FieldTextureResources.FieldTextureManager_Create();
        FieldTextureResources.FieldTextureManager_LoadTexture(fieldSystem.FieldMapContext.TextureResourceManager, AreaData.AreaDataManager_GetMapTexture(fieldSystem.AreaDataManager));
        DynamicMapFeatures.DynamicMapFeatures_Init(fieldSystem);
        BikeSlopeWatcher.BikeSlopeWatcher_Start(fieldSystem);
        GameSystem.SetVBlankCallback(FieldMap_VBlank, fieldSystem);
    }

    // OFFSET 0x021D1A14
    private static MapObjectsToPreload MapObjectsToPreload_Fetch(int heapID, int memberID)
    {
        MapObjectsToPreload mapObjectsToPreload = new();
        byte[] data = NARC.NARC_AllocAtEndAndReadWholeMemberByIndexPair(NarcMapObjectsToPreload, memberID, heapID);
        for (int i = 0; i < MapObjectsToPreload.MaxCount; i++)
        {
            mapObjectsToPreload.IDs[i] = MapObjectsToPreload.Sentinel;
        }
        int count;
        for (count = 0; count < MapObjectsToPreload.MaxCount; count++)
        {
            int id = Bin.U16(data, count * 2);
            mapObjectsToPreload.IDs[count] = id;
            if (id == MapObjectsToPreload.Sentinel)
            {
                break;
            }
        }
        mapObjectsToPreload.Count = (ushort)count;
        Heap.Heap_Free(data);
        return mapObjectsToPreload;
    }

    // OFFSET 0x021D1A68
    private static int[] MapObjectsToPreload_GetIDs(MapObjectsToPreload mapObjectsToPreload)
    {
        return mapObjectsToPreload.IDs;
    }

    // OFFSET 0x021D1A6C
    private static int MapObjectsToPreload_GetCount(MapObjectsToPreload mapObjectsToPreload)
    {
        return mapObjectsToPreload.Count;
    }

    // OFFSET 0x021D1A70
    private static void MapObjectsToPreload_Free(MapObjectsToPreload mapObjectsToPreload)
    {
    }

    // OFFSET 0x021D1A78
    private static bool FieldMap_InDistortionWorld(FieldSystem fieldSystem)
    {
        PersistedMapFeatures features = MiscSaveBlock.SaveData_GetPersistedMapFeatures(FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!);
        if (PersistedMapFeatures.PersistedMapFeatures_GetID(features) == DynamicMapFeaturesDistortionWorld)
        {
            return true;
        }
        return false;
    }
}
