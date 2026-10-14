namespace PlatinumSharp.Game;

public sealed class DoorAnimation
{
    public const int StateOffset = 0x00;
    public const int TileXOffset = 0x14;
    public const int TileZOffset = 0x18;
    public const int HitboxInitializedOffset = 0x1C;
    public const int FovStepOffset = 0x1D;
    public const int AnimationCountOffset = 0x1E;
    public const int AdjustCameraOffset = 0x20;
    public const int SavedFovOffset = 0x24;
    public const int Size = 0x28;

    public int State;
    public int TileX;
    public int TileZ;
    public byte HitboxInitialized;
    public byte FovStep;
    public byte AnimationCount;
    public int AdjustCamera;
    public ushort SavedFov;
}

public sealed class WarpWalkTaskData
{
    public int State;
    public byte FovStep;
    public int AdjustCamera;
    public ushort SavedFov;
}

public static class DoorAnimations
{
    private const int DirSouth = 1;
    private const ushort ColorWhite = 0x7FFF;
    private const int SeqSeWarp = 0x603;
    private const int FadeTypeCircleIn = 0x10;
    private const int OverlayID = 5;
    private const uint EnterDoorModelIDsAddress = 0x021F89F4;
    private const uint ExitDoorModelIDsAddress = 0x021F8A44;
    private const int DoorModelIDCount = 20;
    private const uint ScriptDoorModelIDsAddress = 0x021F8A94;
    private const uint BikeSlopeModelIDsAddress = 0x021F89CC;
    private const byte BikeSlopeModelIDCount = 2;
    private const int OneShotTag = 1;
    private const int ModelIDNoCameraZoom = 0x4B;
    private const int DoorTypeNormal = 0;
    private const int DoorTypeSilent = 1;
    private const int DoorTypeAutomatic = 2;
    private const int SeqSeDoorOpen = 0x605;
    private const int SeqSeDoorClose = 0x607;
    private const int SeqSeDoorSlide = 0x608;
    private const int SeqSeDoorAutomatic = 0x5D4;
    private const int MovementActionWalkNorthIntoDoor = 0xC;
    private const int MovementActionWalkSouthOutOfDoor = 0xD;
    private const uint HitboxSizeX = 3;
    private const uint HitboxSizeZ = 1;
    private const ushort ExitFovAdjust = 0xFFA0;
    private const int ZoomInStep = 0xC;
    private const int ZoomOutStep = 0x10;
    private const int ZoomTotal = 0x60;
    private const int FadeTypeMain = 9;
    private const int FadeTypeSub = 1;
    private const int FadeDuration = 6;
    private const int HeapIDFieldTask = 0xB;
    private const byte FadeTypeIn = 1;

    private static int[] ModelIDs(uint address)
    {
        uint[] words = RomData.OverlayU32(OverlayID, address, DoorModelIDCount);
        int[] ids = new int[DoorModelIDCount];
        for (int i = 0; i < DoorModelIDCount; i++)
        {
            ids[i] = (int)words[i];
        }
        return ids;
    }

    private static bool LoadDoorAnimations(FieldSystem fieldSystem, DoorAnimation door, uint modelIDsAddress, int offsetTileZ, out int modelID)
    {
        int[] modelIDs = ModelIDs(modelIDsAddress);
        door.AdjustCamera = 0;
        door.FovStep = 0;
        TerrainCollisionManager.TerrainCollisionHitbox_Init(door.TileX, door.TileZ, -1, offsetTileZ, HitboxSizeX, HitboxSizeZ, out TerrainCollisionHitbox hitbox);
        door.HitboxInitialized = 1;
        object? mapProp = null;
        modelID = 0;
        if (!TerrainCollisionManager.FieldSystem_FindCollidingLoadedMapPropByModelIDs(fieldSystem, modelIDs, DoorModelIDCount, ref hitbox, ref mapProp, ref modelID))
        {
            return false;
        }
        byte animationCount = MapPropAnimations.MapPropAnimationManager_GetPropAnimationCount(fieldSystem.MapPropAnimationManager!, modelID);
        door.AnimationCount = animationCount;
        G3dTexture? texture = AreaData.AreaDataManager_GetMapPropTexture(fieldSystem.AreaDataManager);
        G3dRenderObj renderObj = MapProps.MapProp_GetRenderObj((MapProp)mapProp!);
        G3dModel? model = MapProps.MapProp_GetModel((MapProp)mapProp!);
        MapPropAnimations.MapPropOneShotAnimationManager_LoadPropAnimations(fieldSystem.MapPropAnimationManager!, fieldSystem.MapPropOneShotAnimationManager!, OneShotTag, modelID, renderObj, model, texture, animationCount, 1, false);
        return true;
    }

    private static int OpenSoundEffect(int modelID)
    {
        if (DoorAnimation_GetDoorType(modelID) == DoorTypeSilent)
        {
            return SeqSeDoorSlide;
        }
        if (DoorAnimation_GetDoorType(modelID) == DoorTypeAutomatic)
        {
            return SeqSeDoorAutomatic;
        }
        return SeqSeDoorOpen;
    }

    private static int CloseSoundEffect(int modelID)
    {
        if (DoorAnimation_GetDoorType(modelID) == DoorTypeSilent)
        {
            return 0;
        }
        if (DoorAnimation_GetDoorType(modelID) == DoorTypeAutomatic)
        {
            return 0;
        }
        return SeqSeDoorClose;
    }

    // OFFSET 0x021D431C
    public static DoorAnimation DoorAnimation_New()
    {
        DoorAnimation door = new();
        door.State = 0;
        return door;
    }

    // OFFSET 0x021D432C
    public static void DoorAnimation_Free(DoorAnimation? door)
    {
    }

    // OFFSET 0x021D4334
    public static void DoorAnimation_SetTile(int tileX, int tileZ, DoorAnimation door)
    {
        door.TileX = tileX;
        door.TileZ = tileZ;
    }

    // OFFSET 0x021D433C
    public static bool FieldSystem_RunDoorEnterAnimation(FieldSystem fieldSystem, DoorAnimation door)
    {
        switch (door.State)
        {
            case 0:
                if (!LoadDoorAnimations(fieldSystem, door, EnterDoorModelIDsAddress, -1, out _))
                {
                    ErrorHandling.GF_AssertFail();
                    return true;
                }
                door.State++;
                break;
            case 1:
            {
                int modelID = MapPropAnimations.MapPropOneShotAnimationManager_GetAnimationMapPropModelID(fieldSystem.MapPropOneShotAnimationManager!, OneShotTag);
                if (modelID != ModelIDNoCameraZoom)
                {
                    Camera.Camera_ReleaseTarget(fieldSystem.Camera!);
                    door.AdjustCamera = 1;
                }
                int soundEffect = OpenSoundEffect(modelID);
                if (door.AnimationCount != 2 && door.AnimationCount != 4)
                {
                    ErrorHandling.GF_AssertFail();
                }
                MapPropAnimations.MapPropOneShotAnimationManager_PlayAnimationWithSoundEffect(fieldSystem.MapPropOneShotAnimationManager!, OneShotTag, 0, soundEffect);
                door.State++;
                break;
            }
            case 2:
                if (MapPropAnimations.MapPropOneShotAnimationManager_IsAnimationLoopFinished(fieldSystem.MapPropOneShotAnimationManager!, OneShotTag))
                {
                    door.State++;
                }
                break;
            case 3:
                MovementActions.LocalMapObj_SetAnimationCode(PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!), MovementActionWalkNorthIntoDoor);
                door.State++;
                break;
            case 4:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                if (MovementActions.LocalMapObj_CheckAnimationFinished(mapObject) == 1)
                {
                    MovementActions.LocalMapObj_ClearFinishedAnimation(mapObject);
                    MapObject.MapObject_SetHidden(mapObject, 1);
                    int modelID = MapPropAnimations.MapPropOneShotAnimationManager_GetAnimationMapPropModelID(fieldSystem.MapPropOneShotAnimationManager!, OneShotTag);
                    int soundEffect = CloseSoundEffect(modelID);
                    if (door.AnimationCount != 2 && door.AnimationCount != 4)
                    {
                        ErrorHandling.GF_AssertFail();
                    }
                    MapPropAnimations.MapPropOneShotAnimationManager_PlayAnimationWithSoundEffect(fieldSystem.MapPropOneShotAnimationManager!, OneShotTag, 1, soundEffect);
                    door.State++;
                }
                break;
            }
            case 5:
                if (MapPropAnimations.MapPropOneShotAnimationManager_IsAnimationLoopFinished(fieldSystem.MapPropOneShotAnimationManager!, OneShotTag))
                {
                    MapPropAnimations.MapPropOneShotAnimationManager_UnloadAnimation(fieldSystem.MapPropAnimationManager, fieldSystem.MapPropOneShotAnimationManager!, OneShotTag);
                    return true;
                }
                break;
        }
        if (door.AdjustCamera != 0)
        {
            DoorAnimation_ZoomIn(fieldSystem.Camera!, ref door.FovStep);
        }
        return false;
    }

    // OFFSET 0x021D453C
    public static bool FieldSystem_RunDoorExitAnimation(FieldSystem fieldSystem, DoorAnimation door)
    {
        switch (door.State)
        {
            case 0:
            {
                if (!LoadDoorAnimations(fieldSystem, door, ExitDoorModelIDsAddress, 0, out int modelID))
                {
                    ErrorHandling.GF_AssertFail();
                    FieldMap.FieldMap_FadeScreen(FadeTypeIn);
                    door.State = 6;
                    return false;
                }
                door.SavedFov = Camera.Camera_GetFOV(fieldSystem.Camera!);
                if (modelID != ModelIDNoCameraZoom)
                {
                    Camera.Camera_AdjustFOV(ExitFovAdjust, fieldSystem.Camera!);
                }
                door.State++;
                break;
            }
            case 1:
            {
                FieldWarpTransition.FieldTask_StartScreenFade(fieldSystem.Task!, 0, FadeTypeMain, FadeTypeSub, 0, FadeDuration, 1, HeapIDFieldTask);
                int modelID = MapPropAnimations.MapPropOneShotAnimationManager_GetAnimationMapPropModelID(fieldSystem.MapPropOneShotAnimationManager!, OneShotTag);
                if (modelID != ModelIDNoCameraZoom)
                {
                    door.AdjustCamera = 1;
                }
                int soundEffect = OpenSoundEffect(modelID);
                int animIndex = 0;
                if (door.AnimationCount == 2)
                {
                    animIndex = 0;
                }
                else if (door.AnimationCount == 4)
                {
                    animIndex = 2;
                }
                else
                {
                    ErrorHandling.GF_AssertFail();
                }
                MapPropAnimations.MapPropOneShotAnimationManager_PlayAnimationWithSoundEffect(fieldSystem.MapPropOneShotAnimationManager!, OneShotTag, animIndex, soundEffect);
                door.State++;
                break;
            }
            case 2:
                if (MapPropAnimations.MapPropOneShotAnimationManager_IsAnimationLoopFinished(fieldSystem.MapPropOneShotAnimationManager!, OneShotTag))
                {
                    MapObject.MapObject_SetHidden(PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!), 0);
                    door.State++;
                }
                break;
            case 3:
                MovementActions.LocalMapObj_SetAnimationCode(PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!), MovementActionWalkSouthOutOfDoor);
                door.State++;
                break;
            case 4:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                if (MovementActions.LocalMapObj_CheckAnimationFinished(mapObject) == 1)
                {
                    MovementActions.LocalMapObj_ClearFinishedAnimation(mapObject);
                    int modelID = MapPropAnimations.MapPropOneShotAnimationManager_GetAnimationMapPropModelID(fieldSystem.MapPropOneShotAnimationManager!, OneShotTag);
                    int soundEffect = CloseSoundEffect(modelID);
                    int animIndex = 1;
                    if (door.AnimationCount == 2)
                    {
                        animIndex = 1;
                    }
                    else if (door.AnimationCount == 4)
                    {
                        animIndex = 3;
                    }
                    else
                    {
                        ErrorHandling.GF_AssertFail();
                    }
                    MapPropAnimations.MapPropOneShotAnimationManager_PlayAnimationWithSoundEffect(fieldSystem.MapPropOneShotAnimationManager!, OneShotTag, animIndex, soundEffect);
                    door.State++;
                }
                break;
            }
            case 5:
                if (MapPropAnimations.MapPropOneShotAnimationManager_IsAnimationLoopFinished(fieldSystem.MapPropOneShotAnimationManager!, OneShotTag)
                    && ScreenFadeControl.IsScreenFadeDone()
                    && door.SavedFov == Camera.Camera_GetFOV(fieldSystem.Camera!))
                {
                    MapPropAnimations.MapPropOneShotAnimationManager_UnloadAnimation(fieldSystem.MapPropAnimationManager, fieldSystem.MapPropOneShotAnimationManager!, OneShotTag);
                    return true;
                }
                break;
            case 6:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    return true;
                }
                break;
        }
        if (door.AdjustCamera != 0)
        {
            DoorAnimation_ZoomOut(fieldSystem.Camera!, ref door.FovStep);
        }
        return false;
    }

    // OFFSET 0x021D4BC8
    private static bool FieldTask_WaitForDoorAnimation(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        byte[] tag = (byte[])task.Environment!;
        if (MapPropAnimations.MapPropOneShotAnimationManager_IsAnimationLoopFinished(fieldSystem.MapPropOneShotAnimationManager!, tag[0]))
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021D4BF4
    public static void FieldSystem_LoadDoorAnimation(FieldSystem fieldSystem, int tileX, int tileZ, byte tag)
    {
        int[] modelIDs = ModelIDs(ScriptDoorModelIDsAddress);
        TerrainCollisionManager.TerrainCollisionHitbox_Init(tileX, tileZ, -1, 0, HitboxSizeX, HitboxSizeZ, out TerrainCollisionHitbox hitbox);
        object? mapProp = null;
        int modelID = 0;
        if (TerrainCollisionManager.FieldSystem_FindCollidingLoadedMapPropByModelIDs(fieldSystem, modelIDs, DoorModelIDCount, ref hitbox, ref mapProp, ref modelID))
        {
            byte animationCount = MapPropAnimations.MapPropAnimationManager_GetPropAnimationCount(fieldSystem.MapPropAnimationManager!, modelID);
            G3dTexture? texture = AreaData.AreaDataManager_GetMapPropTexture(fieldSystem.AreaDataManager);
            G3dRenderObj renderObj = MapProps.MapProp_GetRenderObj((MapProp)mapProp!);
            G3dModel? model = MapProps.MapProp_GetModel((MapProp)mapProp!);
            MapPropAnimations.MapPropOneShotAnimationManager_LoadPropAnimations(fieldSystem.MapPropAnimationManager!, fieldSystem.MapPropOneShotAnimationManager!, tag, modelID, renderObj, model, texture, animationCount, 1, false);
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021D4C88
    public static void FieldSystem_PlayDoorOpenAnimation(FieldSystem fieldSystem, byte tag)
    {
        int modelID = MapPropAnimations.MapPropOneShotAnimationManager_GetAnimationMapPropModelID(fieldSystem.MapPropOneShotAnimationManager!, tag);
        byte animationCount = MapPropAnimations.MapPropAnimationManager_GetPropAnimationCount(fieldSystem.MapPropAnimationManager!, modelID);
        int soundEffect = OpenSoundEffect(modelID);
        if (animationCount != 2 && animationCount != 4)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapPropAnimations.MapPropOneShotAnimationManager_PlayAnimationWithSoundEffect(fieldSystem.MapPropOneShotAnimationManager!, tag, 0, soundEffect);
    }

    // OFFSET 0x021D4CEC
    public static void FieldSystem_PlayDoorCloseAnimation(FieldSystem fieldSystem, byte tag)
    {
        int modelID = MapPropAnimations.MapPropOneShotAnimationManager_GetAnimationMapPropModelID(fieldSystem.MapPropOneShotAnimationManager!, tag);
        byte animationCount = MapPropAnimations.MapPropAnimationManager_GetPropAnimationCount(fieldSystem.MapPropAnimationManager!, modelID);
        int soundEffect = CloseSoundEffect(modelID);
        if (animationCount != 2 && animationCount != 4)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapPropAnimations.MapPropOneShotAnimationManager_PlayAnimationWithSoundEffect(fieldSystem.MapPropOneShotAnimationManager!, tag, 1, soundEffect);
    }

    // OFFSET 0x021D4D48
    public static void FieldSystem_WaitForDoorAnimation(FieldSystem fieldSystem, byte tag)
    {
        byte[] data = new byte[1];
        data[0] = tag;
        FieldTask.FieldTask_InitCall(fieldSystem.Task!, FieldTask_WaitForDoorAnimation, data);
    }

    // OFFSET 0x021D4D68
    public static void FieldSystem_UnloadDoorAnimation(FieldSystem fieldSystem, byte tag)
    {
        MapPropAnimations.MapPropOneShotAnimationManager_UnloadAnimation(fieldSystem.MapPropAnimationManager, fieldSystem.MapPropOneShotAnimationManager!, tag);
    }

    // OFFSET 0x021D4D78
    public static void FieldSystem_StartBikeSlopeAnimation(int tileX, int tileZ, int animationIndex, FieldSystem fieldSystem)
    {
        uint[] words = RomData.OverlayU32(OverlayID, BikeSlopeModelIDsAddress, BikeSlopeModelIDCount);
        int[] modelIDs = { (int)words[0], (int)words[1] };
        TerrainCollisionManager.TerrainCollisionHitbox_Init(tileX, tileZ, 0, -1, HitboxSizeZ, HitboxSizeX, out TerrainCollisionHitbox hitbox);
        object? mapProp = null;
        int modelID = 0;
        if (!TerrainCollisionManager.FieldSystem_FindCollidingLoadedMapPropByModelIDs(fieldSystem, modelIDs, BikeSlopeModelIDCount, ref hitbox, ref mapProp, ref modelID))
        {
            ErrorHandling.GF_AssertFail();
        }
        G3dRenderObj renderObj = MapProps.MapProp_GetRenderObj((MapProp)mapProp!);
        if (!MapPropAnimations.MapPropAnimationManager_AddAnimationToRenderObj(modelID, animationIndex, true, renderObj, fieldSystem.MapPropAnimationManager))
        {
            ErrorHandling.GF_AssertFail();
        }
        MapPropAnimation animation = MapPropAnimations.MapPropAnimationManager_GetAnimation(modelID, animationIndex, fieldSystem.MapPropAnimationManager!);
        MapPropAnimations.MapPropAnimation_GoToFirstFrame(animation);
        MapPropAnimations.MapPropAnimation_SetAnimationPaused(animation, false);
        MapPropAnimations.MapPropAnimation_StartLoop(animation);
    }

    // OFFSET 0x021D4E00
    public static WarpWalkTaskData WarpWalkTaskData_New()
    {
        WarpWalkTaskData data = new();
        data.State = 0;
        return data;
    }

    // OFFSET 0x021D4E10
    public static bool FieldTask_WalkOutOfCave(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpWalkTaskData data = (WarpWalkTaskData)task.Environment!;
        switch (data.State)
        {
            case 0:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                if (PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!) == DirSouth)
                {
                    MapObject.MapObject_SetHidden(mapObject, 1);
                    data.State = 1;
                }
                else
                {
                    MapObject.MapObject_SetHidden(mapObject, 0);
                    data.State = 3;
                }
                data.AdjustCamera = 0;
                data.FovStep = 0;
                data.SavedFov = Camera.Camera_GetFOV(fieldSystem.Camera!);
                Camera.Camera_AdjustFOV(ExitFovAdjust, fieldSystem.Camera!);
                ScreenFadeControl.StartScreenFade(0, 1, 1, ColorWhite, FadeDuration, 1, HeapIDFieldTask);
                data.AdjustCamera = 1;
                break;
            }
            case 1:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                MapObject.MapObject_SetHidden(mapObject, 0);
                MovementActions.LocalMapObj_SetAnimationCode(mapObject, MovementActionWalkSouthOutOfDoor);
                data.State++;
                break;
            }
            case 2:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                if (MovementActions.LocalMapObj_CheckAnimationFinished(mapObject) == 1)
                {
                    MovementActions.LocalMapObj_ClearFinishedAnimation(mapObject);
                    data.State++;
                }
                break;
            }
            case 3:
                if (ScreenFadeControl.IsScreenFadeDone() && data.SavedFov == Camera.Camera_GetFOV(fieldSystem.Camera!))
                {
                    return true;
                }
                break;
        }
        if (data.AdjustCamera != 0)
        {
            DoorAnimation_ZoomOut(fieldSystem.Camera!, ref data.FovStep);
        }
        return false;
    }

    // OFFSET 0x021D4F14
    public static bool FieldTask_WalkOutOfCaveExit(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpWalkTaskData data = (WarpWalkTaskData)task.Environment!;
        switch (data.State)
        {
            case 0:
                MapObject.MapObject_SetHidden(PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!), 1);
                data.AdjustCamera = 0;
                data.FovStep = 0;
                SoundPlayback.Sound_PlayEffect(SeqSeWarp);
                ScreenFadeControl.StartScreenFade(0, 0, 0, ColorWhite, FadeDuration, 1, HeapIDFieldTask);
                data.AdjustCamera = 1;
                data.State++;
                break;
            case 1:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    return true;
                }
                break;
        }
        if (data.AdjustCamera != 0)
        {
            DoorAnimation_ZoomIn(fieldSystem.Camera!, ref data.FovStep);
        }
        return false;
    }

    // OFFSET 0x021D4FA0
    public static bool FieldTask_WalkIntoCaveExit(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpWalkTaskData data = (WarpWalkTaskData)task.Environment!;
        switch (data.State)
        {
            case 0:
                data.AdjustCamera = 0;
                data.FovStep = 0;
                SoundPlayback.Sound_PlayEffect(SeqSeWarp);
                FieldWarpTransition.FieldTask_StartScreenFade(task, 0, FadeTypeCircleIn, 0, 0, FadeDuration, 1, HeapIDFieldTask);
                data.AdjustCamera = 1;
                data.State++;
                break;
            case 1:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    return true;
                }
                break;
        }
        if (data.AdjustCamera != 0)
        {
            DoorAnimation_ZoomIn(fieldSystem.Camera!, ref data.FovStep);
        }
        return false;
    }

    // OFFSET 0x021D5020
    public static bool FieldTask_WalkOutWithZoom(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpWalkTaskData data = (WarpWalkTaskData)task.Environment!;
        switch (data.State)
        {
            case 0:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                byte direction = (byte)PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!);
                if (direction == DirSouth)
                {
                    MapObject.MapObject_SetHidden(mapObject, 1);
                    data.State = 1;
                }
                else
                {
                    MapObject.MapObject_SetHidden(mapObject, 0);
                    data.State = 3;
                }
                data.AdjustCamera = 0;
                data.FovStep = 0;
                data.SavedFov = Camera.Camera_GetFOV(fieldSystem.Camera!);
                Camera.Camera_AdjustFOV(ExitFovAdjust, fieldSystem.Camera!);
                int fadeType = 0;
                switch (direction)
                {
                    case 0:
                        fadeType = 3;
                        break;
                    case 1:
                        fadeType = 5;
                        break;
                    case 2:
                        fadeType = 7;
                        break;
                    case 3:
                        fadeType = 0x27;
                        break;
                    default:
                        ErrorHandling.GF_AssertFail();
                        break;
                }
                FieldWarpTransition.FieldTask_StartScreenFade(task, 0, fadeType, 1, 0, FadeDuration, 1, HeapIDFieldTask);
                data.AdjustCamera = 1;
                break;
            }
            case 1:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                MapObject.MapObject_SetHidden(mapObject, 0);
                MovementActions.LocalMapObj_SetAnimationCode(mapObject, MovementActionWalkSouthOutOfDoor);
                data.State++;
                break;
            }
            case 2:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                if (MovementActions.LocalMapObj_CheckAnimationFinished(mapObject) == 1)
                {
                    MovementActions.LocalMapObj_ClearFinishedAnimation(mapObject);
                    data.State++;
                }
                break;
            }
            case 3:
                if (ScreenFadeControl.IsScreenFadeDone() && data.SavedFov == Camera.Camera_GetFOV(fieldSystem.Camera!))
                {
                    return true;
                }
                break;
        }
        if (data.AdjustCamera != 0)
        {
            DoorAnimation_ZoomOut(fieldSystem.Camera!, ref data.FovStep);
        }
        return false;
    }

    // OFFSET 0x021D5150
    public static bool FieldTask_WalkOut(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpWalkTaskData data = (WarpWalkTaskData)task.Environment!;
        switch (data.State)
        {
            case 0:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                if ((byte)PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!) == DirSouth)
                {
                    MapObject.MapObject_SetHidden(mapObject, 1);
                    data.State = 1;
                }
                else
                {
                    MapObject.MapObject_SetHidden(mapObject, 0);
                    data.State = 3;
                }
                FieldWarpTransition.FieldTask_StartScreenFade(task, 0, 1, 1, 0, FadeDuration, 1, HeapIDFieldTask);
                break;
            }
            case 1:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                MapObject.MapObject_SetHidden(mapObject, 0);
                MovementActions.LocalMapObj_SetAnimationCode(mapObject, MovementActionWalkSouthOutOfDoor);
                data.State++;
                break;
            }
            case 2:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                if (MovementActions.LocalMapObj_CheckAnimationFinished(mapObject) == 1)
                {
                    MovementActions.LocalMapObj_ClearFinishedAnimation(mapObject);
                    data.State++;
                }
                break;
            }
            case 3:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    return true;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x021D4798
    private static void DoorAnimation_ZoomIn(Camera camera, ref byte step)
    {
        byte delta = ZoomInStep;
        int total = step * ZoomInStep;
        if (total > ZoomTotal)
        {
            return;
        }
        if ((ushort)total > ZoomTotal)
        {
            delta = (byte)((ushort)total - ZoomTotal);
        }
        if ((ushort)(Camera.Camera_GetFOV(camera) - delta) != 0)
        {
            Camera.Camera_AdjustFOV((ushort)(-delta), camera);
        }
        step++;
    }

    // OFFSET 0x021D47DC
    private static void DoorAnimation_ZoomOut(Camera camera, ref byte step)
    {
        byte delta;
        int total = step << 4;
        if (total >= ZoomTotal)
        {
            return;
        }
        if ((ushort)total > ZoomTotal)
        {
            delta = (byte)((ushort)total - ZoomTotal);
        }
        else
        {
            delta = ZoomOutStep;
        }
        if ((ushort)(Camera.Camera_GetFOV(camera) + delta) != 0)
        {
            Camera.Camera_AdjustFOV(delta, camera);
        }
        step++;
    }

    // OFFSET 0x021D481C
    private static int DoorAnimation_GetDoorType(int modelID)
    {
        if (modelID == 0x1BA)
        {
            return DoorTypeAutomatic;
        }
        if (modelID == 0x46 || modelID == 0x12A || modelID == 0x1C8 || modelID == 0x1AB || modelID == 0x1E4 || modelID == 0x4B)
        {
            return DoorTypeSilent;
        }
        return DoorTypeNormal;
    }
}
