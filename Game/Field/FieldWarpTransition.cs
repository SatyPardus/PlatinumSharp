using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class ScreenFadeTaskData
{
    public int State;
    public int Mode;
    public int TypeMain;
    public int TypeSub;
    public ushort Color;
    public int Duration;
    public int StepsPerFrame;
    public int HeapID;
}

public sealed class WarpTransitionTaskData
{
    public int State;
    public int SubState;
    public Location NextLocation = new();
    public DoorAnimation? DoorAnimation;
    public int TransitionType;
}

public static class FieldWarpTransition
{
    public const int TransitionTypeNone = 0;
    public const int TransitionTypeDoor = 1;
    public const int TransitionTypeEscalator = 2;
    public const int TransitionTypeStairs = 3;
    public const int TransitionTypeIntoCave = 4;
    public const int TransitionTypeOutOfCave = 5;
    public const int TransitionTypePlain = 6;

    private const int TransitionTypeCount = 7;
    private const int SeqSeWarp = 0x603;
    private const int DirWest = 2;
    private const int DirEast = 3;
    private const int MovementActionWalkOnSpotWest = 0xA;
    private const int MovementActionWalkOnSpotEast = 0xB;
    private const int Fx32One = 0x10000;
    private const byte FadeTypeOut = 0;
    private const byte FadeTypeIn = 1;

    private static readonly FieldTaskFunc[] exitTasks =
    {
        FieldTask_WarpExitPlain,
        FieldTask_WarpExitDoor,
        FieldTask_WarpExitEscalator,
        FieldTask_WarpExitStairs,
        FieldTask_WarpExitIntoCave,
        FieldTask_WarpExitOutOfCave,
        FieldTask_WarpExitPlain,
    };

    private static readonly FieldTaskFunc[] enterTasks =
    {
        FieldTask_WarpEnterDoor,
        FieldTask_WarpEnterWalkOut,
        FieldTask_WarpEnterEscalator,
        FieldTask_WarpEnterStairs,
        FieldTask_WarpEnterWalkOut,
        FieldTask_WarpEnterOutOfCave,
        FieldTask_WarpEnterWalkOut,
    };

    private static readonly Action<FieldSystem>?[] placePlayerFuncs =
    {
        null,
        null,
        FieldSystem_PlacePlayerOnEscalator,
        FieldSystem_PlacePlayerOnStairs,
        null,
        null,
        null,
    };

    // OFFSET 0x02056B30
    public static void FieldTask_StartScreenFade(FieldTask task, int mode, int typeMain, int typeSub, ushort color, int duration, int stepsPerFrame, int heapID)
    {
        ScreenFadeTaskData data = new();
        data.Mode = mode;
        data.TypeMain = typeMain;
        data.TypeSub = typeSub;
        data.Color = color;
        data.Duration = duration;
        data.StepsPerFrame = stepsPerFrame;
        data.HeapID = heapID;
        data.State = 0;
        FieldTask.FieldTask_InitCall(task, FieldTask_ScreenFade, data);
    }

    // OFFSET 0x02056B70
    private static bool FieldTask_ScreenFade(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        ScreenFadeTaskData data = (ScreenFadeTaskData)task.Environment!;
        switch (data.State)
        {
            case 0:
                HBlankSystem.HBlankSystem_Stop(fieldSystem.FieldMapContext!.HBlankSystem!);
                ScreenFadeControl.StartScreenFade(data.Mode, data.TypeMain, data.TypeSub, data.Color, data.Duration, data.StepsPerFrame, data.HeapID);
                data.State++;
                break;
            case 1:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    HBlankSystem.HBlankSystem_Start(fieldSystem.FieldMapContext!.HBlankSystem!);
                    return true;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x02056BDC
    public static void FieldSystem_StartWarpTransition(FieldSystem fieldSystem, int mapHeaderID, int warpID, int x, int z, int direction, int transitionType)
    {
        WarpTransitionTaskData data = new();
        data.State = 0;
        data.SubState = 0;
        data.NextLocation.MapId = mapHeaderID;
        data.NextLocation.WarpId = warpID;
        data.NextLocation.X = x;
        data.NextLocation.Z = z;
        data.NextLocation.FaceDirection = direction;
        data.TransitionType = transitionType;
        FieldTask.FieldSystem_CreateTask(fieldSystem, FieldTask_WarpTransition, data);
    }

    // OFFSET 0x02056C18
    public static void FieldSystem_StartWarpTransitionByMapType(FieldSystem fieldSystem, int mapHeaderID, int warpID, int x, int z, int direction)
    {
        WarpTransitionTaskData data = new();
        int transitionType = TransitionTypeNone;
        data.State = 0;
        data.SubState = 0;
        data.NextLocation.MapId = mapHeaderID;
        data.NextLocation.WarpId = warpID;
        data.NextLocation.X = x;
        data.NextLocation.Z = z;
        data.NextLocation.FaceDirection = direction;
        int currentMapHeaderID = (int)fieldSystem.Location!.U32(0);
        if (MapHeader.MapHeader_IsCave(currentMapHeaderID))
        {
            if (MapHeader.MapHeader_IsCave(mapHeaderID))
            {
                transitionType = TransitionTypePlain;
            }
            else if (MapHeader.MapHeader_IsOutdoors(mapHeaderID))
            {
                transitionType = TransitionTypeOutOfCave;
            }
            else if (MapHeader.MapHeader_IsBuilding(mapHeaderID))
            {
                transitionType = TransitionTypePlain;
            }
            else
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        else if (MapHeader.MapHeader_IsOutdoors(currentMapHeaderID))
        {
            if (MapHeader.MapHeader_IsCave(mapHeaderID))
            {
                transitionType = TransitionTypeIntoCave;
            }
            else if (MapHeader.MapHeader_IsBuilding(mapHeaderID))
            {
                transitionType = TransitionTypePlain;
            }
            else
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        else if (MapHeader.MapHeader_IsBuilding(currentMapHeaderID))
        {
            if (!MapHeader.MapHeader_IsOutdoors(mapHeaderID))
            {
                if (MapHeader.MapHeader_IsBuilding(mapHeaderID))
                {
                    transitionType = TransitionTypePlain;
                }
                else if (!MapHeader.MapHeader_IsCave(mapHeaderID))
                {
                    ErrorHandling.GF_AssertFail();
                }
            }
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        data.TransitionType = transitionType;
        FieldTask.FieldSystem_CreateTask(fieldSystem, FieldTask_WarpTransition, data);
    }

    // OFFSET 0x02056CFC
    private static bool FieldTask_WarpTransition(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpTransitionTaskData data = (WarpTransitionTaskData)task.Environment!;
        Location nextLocation = data.NextLocation;
        switch (data.State)
        {
            case 0:
                data.SubState = 0;
                FieldBgm.FieldBGM_FadeOutForMapHeader(fieldSystem, nextLocation.MapId);
                FieldTask.FieldTask_InitCall(task, exitTasks[data.TransitionType], data);
                data.State++;
                break;
            case 1:
                FieldTransition.FieldTransition_FinishMap(task);
                data.State++;
                break;
            case 2:
                FieldMapChange.FieldTask_ChangeMapByLocation(task, nextLocation);
                data.State++;
                break;
            case 3:
                FieldTransition.FieldTransition_StartMap(task);
                data.State++;
                break;
            case 4:
                placePlayerFuncs[data.TransitionType]?.Invoke(fieldSystem);
                data.State++;
                break;
            case 5:
                if (SoundPlayback.Sound_IsFadeActive() != 0)
                {
                    break;
                }
                FieldBgm.FieldBGM_PlayForMapHeader(fieldSystem, nextLocation.MapId);
                MapNamePopUps.FieldSystem_RequestLocationName(fieldSystem);
                data.SubState = 0;
                FieldTask.FieldTask_InitCall(task, enterTasks[data.TransitionType], data);
                data.State++;
                break;
            case 6:
                return true;
        }
        return false;
    }

    // OFFSET 0x02056DE4
    private static bool FieldTask_WarpExitPlain(FieldTask task)
    {
        WarpTransitionTaskData data = (WarpTransitionTaskData)task.Environment!;
        switch (data.SubState)
        {
            case 0:
                SoundPlayback.Sound_PlayEffect(SeqSeWarp);
                FieldTransition.FieldTransition_FadeOut(task);
                data.SubState++;
                break;
            case 1:
                return true;
        }
        return false;
    }

    // OFFSET 0x02056E20
    private static bool FieldTask_WarpExitDoor(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpTransitionTaskData data = (WarpTransitionTaskData)task.Environment!;
        switch (data.SubState)
        {
            case 0:
                data.DoorAnimation = DoorAnimations.DoorAnimation_New();
                DoorAnimations.DoorAnimation_SetTile(PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!), PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!), data.DoorAnimation!);
                data.SubState++;
                break;
            case 1:
                if (DoorAnimations.FieldSystem_RunDoorEnterAnimation(fieldSystem, data.DoorAnimation!))
                {
                    DoorAnimations.DoorAnimation_Free(data.DoorAnimation);
                    data.SubState++;
                }
                break;
            case 2:
                FieldTransition.FieldTransition_FadeOut(task);
                data.SubState++;
                break;
            case 3:
                return true;
        }
        return false;
    }

    // OFFSET 0x02056EA4
    private static bool FieldTask_WarpExitEscalator(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpTransitionTaskData data = (WarpTransitionTaskData)task.Environment!;
        switch (data.SubState)
        {
            case 0:
                data.DoorAnimation = DoorAnimations.DoorAnimation_New();
                DoorAnimations.DoorAnimation_SetTile(PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!), PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!), data.DoorAnimation!);
                data.SubState++;
                break;
            case 1:
                if (ov5_021D4A24(fieldSystem, data.DoorAnimation, (byte)PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!)) != 0)
                {
                    DoorAnimations.DoorAnimation_Free(data.DoorAnimation);
                    data.SubState++;
                }
                break;
            case 2:
                return true;
        }
        return false;
    }

    // OFFSET 0x02056F1C
    private static bool FieldTask_WarpExitStairs(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpTransitionTaskData data = (WarpTransitionTaskData)task.Environment!;
        switch (data.SubState)
        {
            case 0:
            {
                int direction = PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!);
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                if (direction == DirWest)
                {
                    MovementActions.LocalMapObj_SetAnimationCode(mapObject, MovementActionWalkOnSpotWest);
                }
                else if (direction == DirEast)
                {
                    MovementActions.LocalMapObj_SetAnimationCode(mapObject, MovementActionWalkOnSpotEast);
                }
                else
                {
                    ErrorHandling.GF_AssertFail();
                }
                data.SubState++;
                break;
            }
            case 1:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                if (MovementActions.LocalMapObj_CheckAnimationFinished(mapObject) == 1)
                {
                    MovementActions.LocalMapObj_ClearFinishedAnimation(mapObject);
                    data.SubState++;
                }
                break;
            }
            case 2:
                SoundPlayback.Sound_PlayEffect(SeqSeWarp);
                FieldMap.FieldMap_FadeScreen(FadeTypeOut);
                data.SubState++;
                break;
            case 3:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    return true;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x02056FC0
    private static bool FieldTask_WarpExitIntoCave(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpTransitionTaskData data = (WarpTransitionTaskData)task.Environment!;
        PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
        switch (data.SubState)
        {
            case 0:
                FieldTask.FieldTask_InitCall(task, DoorAnimations.FieldTask_WalkIntoCaveExit, DoorAnimations.WarpWalkTaskData_New());
                data.SubState++;
                break;
            case 1:
                return true;
        }
        return false;
    }

    // OFFSET 0x02057008
    private static bool FieldTask_WarpExitOutOfCave(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpTransitionTaskData data = (WarpTransitionTaskData)task.Environment!;
        PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
        switch (data.SubState)
        {
            case 0:
                FieldTask.FieldTask_InitCall(task, DoorAnimations.FieldTask_WalkOutOfCaveExit, DoorAnimations.WarpWalkTaskData_New());
                data.SubState++;
                break;
            case 1:
                return true;
        }
        return false;
    }

    // OFFSET 0x02057050
    private static bool FieldTask_WarpEnterDoor(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpTransitionTaskData data = (WarpTransitionTaskData)task.Environment!;
        switch (data.SubState)
        {
            case 0:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                int x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
                int z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
                if (TileBehavior.TileBehavior_IsDoor(TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z)) != 0)
                {
                    MapObject.MapObject_SetHidden(mapObject, 1);
                    data.SubState = 1;
                }
                else
                {
                    FieldTask.FieldTask_InitCall(task, DoorAnimations.FieldTask_WalkOutWithZoom, DoorAnimations.WarpWalkTaskData_New());
                    data.SubState = 3;
                }
                break;
            }
            case 1:
                data.DoorAnimation = DoorAnimations.DoorAnimation_New();
                DoorAnimations.DoorAnimation_SetTile(PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!), PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!), data.DoorAnimation!);
                data.SubState++;
                break;
            case 2:
                if (DoorAnimations.FieldSystem_RunDoorExitAnimation(fieldSystem, data.DoorAnimation!))
                {
                    DoorAnimations.DoorAnimation_Free(data.DoorAnimation);
                    MapObject.MapObject_SetHidden(PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!), 0);
                    return true;
                }
                break;
            case 3:
                return true;
        }
        return false;
    }

    // OFFSET 0x0205711C
    private static bool FieldTask_WarpEnterWalkOut(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpTransitionTaskData data = (WarpTransitionTaskData)task.Environment!;
        switch (data.SubState)
        {
            case 0:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                int x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
                int z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
                if (TileBehavior.TileBehavior_IsDoor(TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z)) != 0)
                {
                    MapObject.MapObject_SetHidden(mapObject, 1);
                    data.SubState = 1;
                    FieldTask.FieldTask_InitJump(task, FieldTask_WarpEnterDoor, data);
                }
                else
                {
                    FieldTask.FieldTask_InitCall(task, DoorAnimations.FieldTask_WalkOut, DoorAnimations.WarpWalkTaskData_New());
                    data.SubState++;
                }
                break;
            }
            case 1:
                return true;
        }
        return false;
    }

    // OFFSET 0x020571A0
    private static bool FieldTask_WarpEnterEscalator(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpTransitionTaskData data = (WarpTransitionTaskData)task.Environment!;
        switch (data.SubState)
        {
            case 0:
                data.DoorAnimation = DoorAnimations.DoorAnimation_New();
                DoorAnimations.DoorAnimation_SetTile(PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!), PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!), data.DoorAnimation!);
                data.SubState++;
                break;
            case 1:
                if (ov5_021D4858(fieldSystem, data.DoorAnimation, (byte)PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!)) != 0)
                {
                    DoorAnimations.DoorAnimation_Free(data.DoorAnimation);
                    data.SubState++;
                }
                break;
            case 2:
                return true;
        }
        return false;
    }

    // OFFSET 0x02057218
    private static bool FieldTask_WarpEnterStairs(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpTransitionTaskData data = (WarpTransitionTaskData)task.Environment!;
        switch (data.SubState)
        {
            case 0:
            {
                FieldMap.FieldMap_FadeScreen(FadeTypeIn);
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                int direction = PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!);
                if (direction == DirWest)
                {
                    MovementActions.LocalMapObj_SetAnimationCode(mapObject, MovementActionWalkOnSpotWest);
                }
                else if (direction == DirEast)
                {
                    MovementActions.LocalMapObj_SetAnimationCode(mapObject, MovementActionWalkOnSpotEast);
                }
                else
                {
                    ErrorHandling.GF_AssertFail();
                }
                data.SubState++;
                break;
            }
            case 1:
            {
                MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
                if (MovementActions.LocalMapObj_CheckAnimationFinished(mapObject) == 1)
                {
                    MovementActions.LocalMapObj_ClearFinishedAnimation(mapObject);
                    data.SubState++;
                }
                break;
            }
            case 2:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    data.SubState++;
                }
                break;
            case 3:
                return true;
        }
        return false;
    }

    // OFFSET 0x020572B8
    private static bool FieldTask_WarpEnterOutOfCave(FieldTask task)
    {
        FieldSystem fieldSystem = task.FieldSystem;
        WarpTransitionTaskData data = (WarpTransitionTaskData)task.Environment!;
        PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
        switch (data.SubState)
        {
            case 0:
                FieldTask.FieldTask_InitCall(task, DoorAnimations.FieldTask_WalkOutOfCave, DoorAnimations.WarpWalkTaskData_New());
                data.SubState++;
                break;
            case 1:
                return true;
        }
        return false;
    }

    // OFFSET 0x02057300
    private static void FieldSystem_PlacePlayerOnEscalator(FieldSystem fieldSystem)
    {
        PlayerAvatar avatar = fieldSystem.PlayerAvatar!;
        int direction = PlayerAvatar.PlayerAvatar_GetFacingDir(avatar);
        PlayerAvatar.PlayerAvatar_GetPosPtr(avatar, out VecFx32 position);
        if (direction == DirEast)
        {
            position.X -= Fx32One;
        }
        else
        {
            position.X += Fx32One;
        }
        position.Y = TerrainCollisionManager.TerrainCollisionManager_GetHeight(fieldSystem, position.Y, position.X, position.Z, out _);
        PlayerAvatar.PlayerAvatar_SetPosDirFromVec(avatar, position, direction);
        Camera.Camera_SetTargetAndUpdatePosition(PlayerAvatar.PlayerAvatar_GetPos(avatar), fieldSystem.Camera!);
        Camera.Camera_TrackTarget(() => PlayerAvatar.PlayerAvatar_GetPos(avatar), fieldSystem.Camera!);
    }

    // OFFSET 0x02057368
    private static void FieldSystem_PlacePlayerOnStairs(FieldSystem fieldSystem)
    {
        PlayerAvatar avatar = fieldSystem.PlayerAvatar!;
        int direction = PlayerAvatar.PlayerAvatar_GetFacingDir(avatar);
        PlayerAvatar.PlayerAvatar_GetPosPtr(avatar, out VecFx32 position);
        int x = PlayerAvatar.PlayerAvatar_GetXPos(avatar);
        int z = PlayerAvatar.PlayerAvatar_GetZPos(avatar);
        byte behavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
        if (TileBehavior.TileBehavior_IsWarpStairsEast(behavior) != 0)
        {
            position.X += Fx32One;
            direction = DirWest;
        }
        else if (TileBehavior.TileBehavior_IsWarpStairsWest(behavior) != 0)
        {
            position.X -= Fx32One;
            direction = DirEast;
        }
        position.Y = TerrainCollisionManager.TerrainCollisionManager_GetHeight(fieldSystem, position.Y, position.X, position.Z, out _);
        PlayerAvatar.PlayerAvatar_SetPosDirFromVec(avatar, position, direction);
        Camera.Camera_SetTargetAndUpdatePosition(PlayerAvatar.PlayerAvatar_GetPos(avatar), fieldSystem.Camera!);
        Camera.Camera_TrackTarget(() => PlayerAvatar.PlayerAvatar_GetPos(avatar), fieldSystem.Camera!);
    }
}
