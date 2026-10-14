using static PlatinumSharp.Game.MapObject;
using static PlatinumSharp.Game.MapObjectBuffer;
using static PlatinumSharp.Game.MapObjectMove;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate int MovementTypeStateFunc(MapObject mapObject, byte[] data);

public struct MapObjectMovementBounds
{
    public int MinX;
    public int MinZ;
    public int MaxX;
    public int MaxZ;
}

public static class MapObjectMovementTypes
{
    public const int TrainerTypeSightStraight = 1;
    public const int TrainerTypeSightAround = 2;
    public const int TrainerTypeWalkSpinCounterclockwise = 7;
    public const int TrainerTypeWalkSpinClockwise = 8;
    public const int TrainerTypeApproach = 0xA;

    private const uint ListRegionAddress = 0x020EE7AC;
    private const uint ListRegionEnd = 0x020EEC48;
    private const uint SpinCounterclockwiseAddress = 0x020EEA24;
    private const uint SpinClockwiseAddress = 0x020EEA38;
    private const uint SpinAlternatingCounterclockwiseAddress = 0x020EEA4C;
    private const uint SpinAlternatingClockwiseAddress = 0x020EEA60;
    private const uint DelayListAddress = 0x020EEA88;
    private const uint WalkSpinDirsAddress = 0x020EEAB0;
    private const uint RunnerAwareMovementTypesAddress = 0x020EEAD0;
    private const uint DirectionListsAddress = 0x020EEB08;
    private const int DirectionListsEnd = 0x27;
    private const int MovementTypesEnd = 0xFF;
    private const int ListEnd = -1;
    private const int DirCount = 4;

    private const int LookAroundDataSize = 8;
    private const int LookAroundPausedOffset = 0x0;
    private const int LookAroundDelayOffset = 0x2;
    private const int LookAroundListOffset = 0x4;

    private const int WanderDataSize = 0x10;
    private const int WanderStateOffset = 0x0;
    private const int WanderDelayOffset = 0x2;
    private const int WanderRangeModeOffset = 0x4;
    private const int WanderActionOffset = 0x8;
    private const int WanderListOffset = 0xC;
    private const int WanderRangeModeBounds = 1;
    private const int WanderRangeModeIgnoreObstacles = 2;

    private const int FaceDataSize = 8;
    private const int FaceDirOffset = 0x0;
    private const int FaceStateOffset = 0x4;

    private const int SpinDataSize = 8;
    private const int SpinRotationOffset = 0x0;
    private const int SpinStateOffset = 0x2;
    private const int SpinTimerOffset = 0x4;
    private const int SpinRotationCounterclockwise = 2;
    private const int SpinRotationClockwise = 3;
    private const int SpinDelayFrames = 0x18;
    private const int SpinPlayerList = 0x26;

    private const int PaceDataSize = 8;
    private const int PaceStateOffset = 0x0;
    private const int PaceReversedOffset = 0x2;
    private const int PaceWalkSpinOffset = 0x4;

    private const int RouteDataSize = 0xC;
    private const int RouteStateOffset = 0x0;
    private const int RouteIndexOffset = 0x1;
    private const int RouteTurnIndexOffset = 0x2;
    private const int RouteAxisOffset = 0x3;
    private const int RouteListOffset = 0x4;
    private const int RouteReverseOffset = 0x5;
    private const int RouteWalkSpinOffset = 0x8;
    private const int RouteLastIndex = 3;

    private const int WalkSpinPrevDirOffset = 0x0;
    private const int WalkSpinRotationOffset = 0x1;
    private const int WalkSpinWasLockedOffset = 0x2;

    private const int ActionFace = 0x0;
    private const int ActionWalkNormal = 0xC;
    private const int ActionWalkOnSpotNormal = 0x20;
    private const int ActionJumpOnSpotFast = 0x30;

    private static readonly MovementTypeStateFunc[] sSpinStates =
    {
        MovementType_Spin_StateStart,
        MovementType_Spin_StateWaitAction,
        MovementType_Spin_StateDelay,
        MovementType_Spin_StateTurn,
    };

    private static readonly MovementTypeStateFunc[] sSpinAlternatingStates =
    {
        MovementType_SpinAlternating_StateStart,
        MovementType_SpinAlternating_StateWaitAction,
        MovementType_SpinAlternating_StateDelay,
        MovementType_SpinAlternating_StateTurn,
    };

    private static readonly MovementTypeStateFunc[] sPaceStates =
    {
        MovementType_Pace_StateStart,
        MovementType_Pace_StateStep,
        MovementType_Pace_StateWaitAction,
    };

    private static readonly MovementTypeStateFunc[] sRouteStates =
    {
        MovementType_Route_StateStep,
        MovementType_Route_StateWaitAction,
    };

    private static readonly MovementTypeStateFunc[] sReversibleRouteStates =
    {
        MovementType_ReversibleRoute_StateCheckPlayer,
        MovementType_ReversibleRoute_StateWaitJump,
        MovementType_ReversibleRoute_StateStep,
        MovementType_ReversibleRoute_StateWaitAction,
    };

    private static int[]? sListRegion;

    public static bool Dispatch(uint address, MapObject mapObject)
    {
        switch (address & ~1u)
        {
            case 0x0206453C:
                MovementType_LookAroundAll_Init(mapObject);
                return true;
            case 0x02064548:
                MovementType_LookAroundNorthWest_Init(mapObject);
                return true;
            case 0x02064554:
                MovementType_LookAroundNorthEast_Init(mapObject);
                return true;
            case 0x02064560:
                MovementType_LookAroundSouthWest_Init(mapObject);
                return true;
            case 0x0206456C:
                MovementType_LookAroundSouthEast_Init(mapObject);
                return true;
            case 0x02064578:
                MovementType_LookAroundNorthSouthWest_Init(mapObject);
                return true;
            case 0x02064584:
                MovementType_LookAroundNorthSouthEast_Init(mapObject);
                return true;
            case 0x02064590:
                MovementType_LookAroundNorthWestEast_Init(mapObject);
                return true;
            case 0x0206459C:
                MovementType_LookAroundSouthWestEast_Init(mapObject);
                return true;
            case 0x020645A8:
                MovementType_LookAroundNorthSouth_Init(mapObject);
                return true;
            case 0x020645B4:
                MovementType_LookAroundWestEast_Init(mapObject);
                return true;
            case 0x020645C0:
                MovementType_LookAround_Move(mapObject);
                return true;
            case 0x02064624:
                MovementType_LookAround_Exit(mapObject);
                return true;
            case 0x02064628:
                MovementType_WanderAround_Init(mapObject);
                return true;
            case 0x02064638:
                MovementType_WanderNorthSouth_Init(mapObject);
                return true;
            case 0x02064648:
                MovementType_WanderWestEast_Init(mapObject);
                return true;
            case 0x02064658:
                MovementType_WanderWestEastIgnoreObstacles_Init(mapObject);
                return true;
            case 0x02064690:
                MovementType_Wander_Move(mapObject);
                return true;
            case 0x02064918:
                MovementType_FaceDirection_Move(mapObject);
                return true;
            case 0x0206493C:
                MovementType_FaceNorth_Init(mapObject);
                return true;
            case 0x02064948:
                MovementType_FaceSouth_Init(mapObject);
                return true;
            case 0x02064954:
                MovementType_FaceWest_Init(mapObject);
                return true;
            case 0x02064960:
                MovementType_FaceEast_Init(mapObject);
                return true;
            case 0x02064990:
                MovementType_SpinCounterclockwise_Init(mapObject);
                return true;
            case 0x0206499C:
                MovementType_SpinClockwise_Init(mapObject);
                return true;
            case 0x020649A8:
                MovementType_Spin_Move(mapObject);
                return true;
            case 0x02064AF0:
                MovementType_SpinAlternating_Init(mapObject);
                return true;
            case 0x02064AFC:
                MovementType_SpinAlternating_Move(mapObject);
                return true;
            case 0x02064C28:
                MovementType_Pace_Init(mapObject);
                return true;
            case 0x02064C48:
                MovementType_Pace_Move(mapObject);
                return true;
            case 0x02064DC8:
                MovementType_Route0E_Init(mapObject);
                return true;
            case 0x02064DD8:
                MovementType_Route0F_Init(mapObject);
                return true;
            case 0x02064DE8:
                MovementType_Route10_Init(mapObject);
                return true;
            case 0x02064DF8:
                MovementType_Route11_Init(mapObject);
                return true;
            case 0x02064E08:
                MovementType_Route12_Init(mapObject);
                return true;
            case 0x02064E18:
                MovementType_Route13_Init(mapObject);
                return true;
            case 0x02064E28:
                MovementType_Route14_Init(mapObject);
                return true;
            case 0x02064E38:
                MovementType_Route15_Init(mapObject);
                return true;
            case 0x02064E48:
                MovementType_Route16_Init(mapObject);
                return true;
            case 0x02064E58:
                MovementType_Route17_Init(mapObject);
                return true;
            case 0x02064E68:
                MovementType_Route18_Init(mapObject);
                return true;
            case 0x02064E78:
                MovementType_Route19_Init(mapObject);
                return true;
            case 0x02064E88:
                MovementType_Route1A_Init(mapObject);
                return true;
            case 0x02064E98:
                MovementType_Route1B_Init(mapObject);
                return true;
            case 0x02064EA8:
                MovementType_Route1C_Init(mapObject);
                return true;
            case 0x02064EB8:
                MovementType_Route1D_Init(mapObject);
                return true;
            case 0x02064EC8:
                MovementType_Route_Move(mapObject);
                return true;
            case 0x0206505C:
                MovementType_ReversibleRoute1E_Init(mapObject);
                return true;
            case 0x0206506C:
                MovementType_ReversibleRoute1F_Init(mapObject);
                return true;
            case 0x0206507C:
                MovementType_ReversibleRoute20_Init(mapObject);
                return true;
            case 0x0206508C:
                MovementType_ReversibleRoute21_Init(mapObject);
                return true;
            case 0x0206509C:
                MovementType_ReversibleRoute22_Init(mapObject);
                return true;
            case 0x020650AC:
                MovementType_ReversibleRoute23_Init(mapObject);
                return true;
            case 0x020650BC:
                MovementType_ReversibleRoute24_Init(mapObject);
                return true;
            case 0x020650CC:
                MovementType_ReversibleRoute25_Init(mapObject);
                return true;
            case 0x020650DC:
                MovementType_ReversibleRoute_Move(mapObject);
                return true;
            case 0x02067840:
                BerryPatchGraphics.BerryPatchGraphics_NewData(mapObject);
                return true;
            case 0x02067850:
                BerryPatchGraphics.BerryPatchGraphics_UpdateGrowthStage(mapObject);
                return true;
            case 0x0206786C:
                BerryPatchGraphics.BerryPatchGraphics_NoOp(mapObject);
                return true;
            case 0x02067870:
                BerryPatchGraphics.BerryPatchGraphics_NewGraphics(mapObject);
                return true;
            case 0x02067890:
                BerryPatchGraphics.BerryPatchGraphics_UpdateGraphics(mapObject);
                return true;
            case 0x02067950:
                BerryPatchGraphics.BerryPatchGraphics_FreeGraphics(mapObject);
                return true;
            case 0x02067968:
                BerryPatchGraphics.BerryPatchGraphics_PauseGraphics(mapObject);
                return true;
            case 0x02067998:
                BerryPatchGraphics.BerryPatchGraphics_ResumeGraphics(mapObject);
                return true;
            default:
                return false;
        }
    }

    // OFFSET 0x0206450C
    private static void MovementType_InitLookAround(MapObject mapObject, int directionListID)
    {
        byte[] data = MapObject_InitMovementTypeData(mapObject, LookAroundDataSize);
        W16(data, LookAroundDelayOffset, IntList_GetRandom(DelayListAddress, ListEnd));
        W32(data, LookAroundListOffset, directionListID);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_SetNotMoving(mapObject);
    }

    // OFFSET 0x0206453C
    private static void MovementType_LookAroundAll_Init(MapObject mapObject)
    {
        MovementType_InitLookAround(mapObject, 0);
    }

    // OFFSET 0x02064548
    private static void MovementType_LookAroundNorthWest_Init(MapObject mapObject)
    {
        MovementType_InitLookAround(mapObject, 1);
    }

    // OFFSET 0x02064554
    private static void MovementType_LookAroundNorthEast_Init(MapObject mapObject)
    {
        MovementType_InitLookAround(mapObject, 2);
    }

    // OFFSET 0x02064560
    private static void MovementType_LookAroundSouthWest_Init(MapObject mapObject)
    {
        MovementType_InitLookAround(mapObject, 3);
    }

    // OFFSET 0x0206456C
    private static void MovementType_LookAroundSouthEast_Init(MapObject mapObject)
    {
        MovementType_InitLookAround(mapObject, 4);
    }

    // OFFSET 0x02064578
    private static void MovementType_LookAroundNorthSouthWest_Init(MapObject mapObject)
    {
        MovementType_InitLookAround(mapObject, 5);
    }

    // OFFSET 0x02064584
    private static void MovementType_LookAroundNorthSouthEast_Init(MapObject mapObject)
    {
        MovementType_InitLookAround(mapObject, 6);
    }

    // OFFSET 0x02064590
    private static void MovementType_LookAroundNorthWestEast_Init(MapObject mapObject)
    {
        MovementType_InitLookAround(mapObject, 7);
    }

    // OFFSET 0x0206459C
    private static void MovementType_LookAroundSouthWestEast_Init(MapObject mapObject)
    {
        MovementType_InitLookAround(mapObject, 8);
    }

    // OFFSET 0x020645A8
    private static void MovementType_LookAroundNorthSouth_Init(MapObject mapObject)
    {
        MovementType_InitLookAround(mapObject, 9);
    }

    // OFFSET 0x020645B4
    private static void MovementType_LookAroundWestEast_Init(MapObject mapObject)
    {
        MovementType_InitLookAround(mapObject, 10);
    }

    // OFFSET 0x020645C0
    private static void MovementType_LookAround_Move(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        int dir = MapObject_GetListDirTowardsRunningPlayer(mapObject, S32(data, LookAroundListOffset), ListEnd);
        if (dir != DirNone)
        {
            MapObject_TryFace(mapObject, dir);
        }
        else if (U16(data, LookAroundPausedOffset) == 0)
        {
            W16(data, LookAroundDelayOffset, S16(data, LookAroundDelayOffset) - 1);
            if (S16(data, LookAroundDelayOffset) <= 0)
            {
                W16(data, LookAroundDelayOffset, IntList_GetRandom(DelayListAddress, ListEnd));
                MapObject_TryFace(mapObject, DirectionList_GetRandom(S32(data, LookAroundListOffset), ListEnd));
            }
        }
        MapObject_UpdateCoords(mapObject);
    }

    // OFFSET 0x02064624
    private static void MovementType_LookAround_Exit(MapObject mapObject)
    {
    }

    // OFFSET 0x02064628
    private static void MovementType_WanderAround_Init(MapObject mapObject)
    {
        MovementType_InitWander(mapObject, ActionWalkNormal, 0xB, 0);
    }

    // OFFSET 0x02064638
    private static void MovementType_WanderNorthSouth_Init(MapObject mapObject)
    {
        MovementType_InitWander(mapObject, ActionWalkNormal, 0xC, 0);
    }

    // OFFSET 0x02064648
    private static void MovementType_WanderWestEast_Init(MapObject mapObject)
    {
        MovementType_InitWander(mapObject, ActionWalkNormal, 0xD, 0);
    }

    // OFFSET 0x02064658
    private static void MovementType_WanderWestEastIgnoreObstacles_Init(MapObject mapObject)
    {
        MovementType_InitWander(mapObject, ActionWalkNormal, 0xD, WanderRangeModeIgnoreObstacles);
    }

    // OFFSET 0x02064668
    private static void MovementType_InitWander(MapObject mapObject, int movementAction, int directionListID, int rangeMode)
    {
        byte[] data = MapObject_InitMovementTypeData(mapObject, WanderDataSize);
        W32(data, WanderRangeModeOffset, rangeMode);
        W32(data, WanderActionOffset, movementAction);
        W32(data, WanderListOffset, directionListID);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_SetNotMoving(mapObject);
    }

    // OFFSET 0x02064690
    private static void MovementType_Wander_Move(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        int state = S16(data, WanderStateOffset);
        if (state == 0)
        {
            MapObject_SetNotMoving(mapObject);
            MapObject_SetEndMovementOff(mapObject);
            int action = MovementActions.MovementAction_TurnActionTowardsDir(MapObject_GetFacingDir(mapObject), ActionFace);
            MovementActions.MapObject_SetMovementActionForMove(mapObject, action);
            W16(data, WanderStateOffset, S16(data, WanderStateOffset) + 1);
            return;
        }
        if (state == 1)
        {
            if (MovementActions.MapObject_DoMovementActionAndCheckEnd(mapObject) == 0)
            {
                return;
            }
            W16(data, WanderDelayOffset, IntList_GetRandom(DelayListAddress, ListEnd));
            W16(data, WanderStateOffset, S16(data, WanderStateOffset) + 1);
            state = 2;
        }
        if (state == 2)
        {
            W16(data, WanderDelayOffset, S16(data, WanderDelayOffset) - 1);
            if (S16(data, WanderDelayOffset) != 0)
            {
                return;
            }
            W16(data, WanderStateOffset, S16(data, WanderStateOffset) + 1);
            state = 3;
        }
        if (state == 3)
        {
            int dir = DirectionList_GetRandom(S32(data, WanderListOffset), ListEnd);
            MapObject_TryFaceAndTurn(mapObject, dir);
            if (S32(data, WanderRangeModeOffset) == WanderRangeModeBounds)
            {
                if (MapObject_IsDirInMovementBounds(mapObject, dir) == 0)
                {
                    W16(data, WanderStateOffset, 0);
                    return;
                }
            }
            uint collision = MapObject_CheckCollisionInDir(mapObject, dir);
            if (collision != 0)
            {
                if (S32(data, WanderRangeModeOffset) == WanderRangeModeIgnoreObstacles)
                {
                    if ((collision & CollisionOutOfRange) != 0)
                    {
                        W16(data, WanderStateOffset, 0);
                        return;
                    }
                }
                else
                {
                    W16(data, WanderStateOffset, 0);
                    return;
                }
            }
            int action = MovementActions.MovementAction_TurnActionTowardsDir(dir, S32(data, WanderActionOffset));
            MovementActions.MapObject_SetMovementActionForMove(mapObject, action);
            MapObject_SetMoving(mapObject);
            W16(data, WanderStateOffset, S16(data, WanderStateOffset) + 1);
            state = 4;
        }
        if (state == 4)
        {
            if (MovementActions.MapObject_DoMovementActionAndCheckEnd(mapObject) != 0)
            {
                MapObject_SetNotMoving(mapObject);
                W16(data, WanderStateOffset, 0);
            }
        }
    }

    // OFFSET 0x020647A0
    private static void MapObject_GetMovementBounds(MapObject mapObject, out MapObjectMovementBounds bounds)
    {
        bounds = default;
        int x = MapObject_GetXInitial(mapObject);
        int z = MapObject_GetZInitial(mapObject);
        int rangeX = MapObject_GetMovementRangeX(mapObject);
        int rangeZ = MapObject_GetMovementRangeZ(mapObject);
        switch (MapObject_GetMovementType(mapObject))
        {
            case 6:
                bounds.MinX = x - rangeX;
                bounds.MaxX = x;
                bounds.MinZ = z - rangeZ;
                bounds.MaxZ = z;
                return;
            case 7:
                bounds.MinX = x;
                bounds.MaxX = x + rangeX;
                bounds.MinZ = z - rangeZ;
                bounds.MaxZ = z;
                return;
            case 8:
                bounds.MinX = x - rangeX;
                bounds.MaxX = x;
                bounds.MinZ = z;
                bounds.MaxZ = z + rangeZ;
                return;
            case 9:
                bounds.MinX = x;
                bounds.MaxX = x + rangeX;
                bounds.MinZ = z;
                bounds.MaxZ = z + rangeZ;
                return;
            case 10:
                bounds.MinX = x - rangeX;
                bounds.MaxX = x;
                bounds.MinZ = z - rangeZ;
                bounds.MaxZ = z + rangeZ;
                return;
            case 11:
                bounds.MinX = x;
                bounds.MaxX = x + rangeX;
                bounds.MinZ = z - rangeZ;
                bounds.MaxZ = z + rangeZ;
                return;
            case 12:
                bounds.MinX = x - rangeX;
                bounds.MaxX = x + rangeX;
                bounds.MinZ = z - rangeZ;
                bounds.MaxZ = z;
                return;
            case 13:
                bounds.MinX = x - rangeX;
                bounds.MaxX = x + rangeX;
                bounds.MinZ = z;
                bounds.MaxZ = z + rangeZ;
                return;
            default:
                ErrorHandling.GF_AssertFail();
                return;
        }
    }

    // OFFSET 0x0206489C
    private static int MapObject_IsDirInMovementBounds(MapObject mapObject, int dir)
    {
        MapObject_GetMovementBounds(mapObject, out MapObjectMovementBounds bounds);
        int x = MapObject_GetX(mapObject) + MapObject_GetDxFromDir(dir);
        int z = MapObject_GetZ(mapObject) + MapObject_GetDzFromDir(dir);
        if (bounds.MinX > x || bounds.MaxX < x)
        {
            return 0;
        }
        if (bounds.MinZ > z || bounds.MaxZ < z)
        {
            return 0;
        }
        return 1;
    }

    // OFFSET 0x020648F4
    private static void MovementType_InitFaceDirection(MapObject mapObject, int dir)
    {
        byte[] data = MapObject_InitMovementTypeData(mapObject, FaceDataSize);
        W32(data, FaceDirOffset, dir);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_SetNotMoving(mapObject);
        MapObject_UpdateCoords(mapObject);
    }

    // OFFSET 0x02064918
    private static void MovementType_FaceDirection_Move(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        if (S32(data, FaceStateOffset) == 0)
        {
            MapObject_TryFace(mapObject, S32(data, FaceDirOffset));
            W32(data, FaceStateOffset, S32(data, FaceStateOffset) + 1);
        }
    }

    // OFFSET 0x0206493C
    private static void MovementType_FaceNorth_Init(MapObject mapObject)
    {
        MovementType_InitFaceDirection(mapObject, DirNorth);
    }

    // OFFSET 0x02064948
    private static void MovementType_FaceSouth_Init(MapObject mapObject)
    {
        MovementType_InitFaceDirection(mapObject, DirSouth);
    }

    // OFFSET 0x02064954
    private static void MovementType_FaceWest_Init(MapObject mapObject)
    {
        MovementType_InitFaceDirection(mapObject, DirWest);
    }

    // OFFSET 0x02064960
    private static void MovementType_FaceEast_Init(MapObject mapObject)
    {
        MovementType_InitFaceDirection(mapObject, DirEast);
    }

    // OFFSET 0x0206496C
    private static void MovementType_InitSpin(MapObject mapObject, int rotation)
    {
        byte[] data = MapObject_InitMovementTypeData(mapObject, SpinDataSize);
        W8(data, SpinRotationOffset, rotation);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_SetNotMoving(mapObject);
        MapObject_UpdateCoords(mapObject);
    }

    // OFFSET 0x02064990
    private static void MovementType_SpinCounterclockwise_Init(MapObject mapObject)
    {
        MovementType_InitSpin(mapObject, SpinRotationCounterclockwise);
    }

    // OFFSET 0x0206499C
    private static void MovementType_SpinClockwise_Init(MapObject mapObject)
    {
        MovementType_InitSpin(mapObject, SpinRotationClockwise);
    }

    // OFFSET 0x020649A8
    private static void MovementType_Spin_Move(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        while (sSpinStates[S8(data, SpinStateOffset)](mapObject, data) == 1)
        {
        }
    }

    // OFFSET 0x020649CC
    private static int MovementType_Spin_StateStart(MapObject mapObject, byte[] data)
    {
        int dir = MapObject_GetListDirTowardsRunningPlayer(mapObject, SpinPlayerList, ListEnd);
        if (dir == DirNone)
        {
            dir = MapObject_GetFacingDir(mapObject);
        }
        int action = MovementActions.MovementAction_TurnActionTowardsDir(dir, ActionFace);
        MovementActions.MapObject_SetMovementActionForMove(mapObject, action);
        W8(data, SpinStateOffset, 1);
        return 1;
    }

    // OFFSET 0x02064A00
    private static int MovementType_Spin_StateWaitAction(MapObject mapObject, byte[] data)
    {
        if (MovementActions.MapObject_DoMovementActionAndCheckEnd(mapObject) == 0)
        {
            return 0;
        }
        W32(data, SpinTimerOffset, 0);
        W8(data, SpinStateOffset, 2);
        return 1;
    }

    // OFFSET 0x02064A1C
    private static int MovementType_Spin_StateDelay(MapObject mapObject, byte[] data)
    {
        if (S32(data, SpinTimerOffset) != 0)
        {
            if (MapObject_GetListDirTowardsRunningPlayer(mapObject, SpinPlayerList, ListEnd) != DirNone)
            {
                W8(data, SpinStateOffset, 0);
                return 1;
            }
        }
        W32(data, SpinTimerOffset, S32(data, SpinTimerOffset) + 1);
        if (S32(data, SpinTimerOffset) < SpinDelayFrames)
        {
            return 0;
        }
        W8(data, SpinStateOffset, 3);
        return 1;
    }

    // OFFSET 0x02064A58
    private static int MovementType_Spin_StateTurn(MapObject mapObject, byte[] data)
    {
        uint list = SpinCounterclockwiseAddress;
        if (S8(data, SpinRotationOffset) != SpinRotationCounterclockwise)
        {
            list = SpinClockwiseAddress;
        }
        MapObject_TryFace(mapObject, NextSpinDir(list, MapObject_GetFacingDir(mapObject)));
        W8(data, SpinStateOffset, 0);
        return 1;
    }

    // OFFSET 0x02064AF0
    private static void MovementType_SpinAlternating_Init(MapObject mapObject)
    {
        MovementType_InitSpin(mapObject, SpinRotationClockwise);
    }

    // OFFSET 0x02064AFC
    private static void MovementType_SpinAlternating_Move(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        while (sSpinAlternatingStates[S8(data, SpinStateOffset)](mapObject, data) == 1)
        {
        }
    }

    // OFFSET 0x02064B20
    private static int MovementType_SpinAlternating_StateStart(MapObject mapObject, byte[] data)
    {
        int action = MovementActions.MovementAction_TurnActionTowardsDir(MapObject_GetFacingDir(mapObject), ActionFace);
        MovementActions.MapObject_SetMovementActionForMove(mapObject, action);
        W8(data, SpinStateOffset, 1);
        return 1;
    }

    // OFFSET 0x02064B40
    private static int MovementType_SpinAlternating_StateWaitAction(MapObject mapObject, byte[] data)
    {
        if (MovementActions.MapObject_DoMovementActionAndCheckEnd(mapObject) == 0)
        {
            return 0;
        }
        W32(data, SpinTimerOffset, 0);
        W8(data, SpinStateOffset, 2);
        return 1;
    }

    // OFFSET 0x02064B5C
    private static int MovementType_SpinAlternating_StateDelay(MapObject mapObject, byte[] data)
    {
        W32(data, SpinTimerOffset, S32(data, SpinTimerOffset) + 1);
        if (S32(data, SpinTimerOffset) < SpinDelayFrames)
        {
            return 0;
        }
        W8(data, SpinStateOffset, 3);
        return 1;
    }

    // OFFSET 0x02064B74
    private static int MovementType_SpinAlternating_StateTurn(MapObject mapObject, byte[] data)
    {
        uint list = SpinAlternatingCounterclockwiseAddress;
        if (S8(data, SpinRotationOffset) != SpinRotationCounterclockwise)
        {
            list = SpinAlternatingClockwiseAddress;
        }
        MapObject_TryFace(mapObject, NextSpinDir(list, MapObject_GetFacingDir(mapObject)));
        if (MapObject_GetFacingDir(mapObject) == MapObject_GetInitialDir(mapObject))
        {
            W8(data, SpinRotationOffset, Direction_GetOpposite(S8(data, SpinRotationOffset)));
        }
        W8(data, SpinStateOffset, 0);
        return 1;
    }

    // OFFSET 0x02064C28
    private static void MovementType_Pace_Init(MapObject mapObject)
    {
        byte[] data = MapObject_InitMovementTypeData(mapObject, PaceDataSize);
        if (MapObject_IsWalkSpinTrainer(mapObject) == 1)
        {
            WalkSpin_Init(mapObject, data, PaceWalkSpinOffset);
        }
    }

    // OFFSET 0x02064C48
    private static void MovementType_Pace_Move(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        while (sPaceStates[S16(data, PaceStateOffset)](mapObject, data) == 1)
        {
        }
    }

    // OFFSET 0x02064C6C
    private static int MovementType_Pace_StateStart(MapObject mapObject, byte[] data)
    {
        int dir = MapObject_GetInitialDir(mapObject);
        if (S16(data, PaceReversedOffset) == 1)
        {
            dir = Direction_GetOpposite(dir);
        }
        MapObject_Turn(mapObject, dir);
        if (MapObject_IsWalkSpinTrainer(mapObject) == 0)
        {
            MapObject_TryFace(mapObject, dir);
        }
        W16(data, PaceStateOffset, 1);
        return 1;
    }

    // OFFSET 0x02064CA8
    private static int MovementType_Pace_StateStep(MapObject mapObject, byte[] data)
    {
        int dir;
        if (S16(data, PaceReversedOffset) != 0)
        {
            int initialX = MapObject_GetXInitial(mapObject);
            int initialZ = MapObject_GetZInitial(mapObject);
            int x = MapObject_GetX(mapObject);
            int z = MapObject_GetZ(mapObject);
            if (initialX == x && initialZ == z)
            {
                dir = Direction_GetOpposite(MapObject_GetMovingDir(mapObject));
                MapObject_Turn(mapObject, dir);
                if (MapObject_IsWalkSpinTrainer(mapObject) == 0)
                {
                    MapObject_TryFace(mapObject, dir);
                }
                W16(data, PaceReversedOffset, 0);
            }
        }
        dir = MapObject_GetMovingDir(mapObject);
        uint collision = MapObject_CheckCollisionInDir(mapObject, dir);
        if ((collision & CollisionOutOfRange) != 0)
        {
            W16(data, PaceReversedOffset, 1);
            dir = Direction_GetOpposite(dir);
            collision = MapObject_CheckCollisionInDir(mapObject, dir);
        }
        int action = ActionWalkNormal;
        if (collision != 0)
        {
            action = ActionWalkOnSpotNormal;
        }
        action = MovementActions.MovementAction_TurnActionTowardsDir(dir, action);
        MovementActions.MapObject_SetMovementActionForMove(mapObject, action);
        if (MapObject_IsWalkSpinTrainer(mapObject) == 1)
        {
            WalkSpin_Turn(mapObject, data, PaceWalkSpinOffset);
        }
        MapObject_SetMoving(mapObject);
        W16(data, PaceStateOffset, 2);
        return 1;
    }

    // OFFSET 0x02064D68
    private static int MovementType_Pace_StateWaitAction(MapObject mapObject, byte[] data)
    {
        if (MovementActions.MapObject_DoMovementActionAndCheckEnd(mapObject) == 1)
        {
            MapObject_SetNotMoving(mapObject);
            if (MapObject_IsWalkSpinTrainer(mapObject) == 1)
            {
                WalkSpin_End(mapObject, data, PaceWalkSpinOffset);
            }
            W16(data, PaceStateOffset, 0);
        }
        return 0;
    }

    // OFFSET 0x02064D98
    private static void MovementType_InitRoute(MapObject mapObject, int turnIndex, int axis, int directionListID)
    {
        byte[] data = MapObject_InitMovementTypeData(mapObject, RouteDataSize);
        W8(data, RouteTurnIndexOffset, turnIndex);
        W8(data, RouteAxisOffset, axis);
        W32(data, RouteListOffset, directionListID);
        if (MapObject_IsWalkSpinTrainer(mapObject) == 1)
        {
            WalkSpin_Init(mapObject, data, RouteWalkSpinOffset);
        }
    }

    // OFFSET 0x02064DC8
    private static void MovementType_Route0E_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 0, 0xE);
    }

    // OFFSET 0x02064DD8
    private static void MovementType_Route0F_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 0, 0xF);
    }

    // OFFSET 0x02064DE8
    private static void MovementType_Route10_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 1, 0x10);
    }

    // OFFSET 0x02064DF8
    private static void MovementType_Route11_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 1, 0x11);
    }

    // OFFSET 0x02064E08
    private static void MovementType_Route12_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 0, 0x12);
    }

    // OFFSET 0x02064E18
    private static void MovementType_Route13_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 0, 0x13);
    }

    // OFFSET 0x02064E28
    private static void MovementType_Route14_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 1, 0x14);
    }

    // OFFSET 0x02064E38
    private static void MovementType_Route15_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 1, 0x15);
    }

    // OFFSET 0x02064E48
    private static void MovementType_Route16_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 1, 0x16);
    }

    // OFFSET 0x02064E58
    private static void MovementType_Route17_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 1, 0x17);
    }

    // OFFSET 0x02064E68
    private static void MovementType_Route18_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 0, 0x18);
    }

    // OFFSET 0x02064E78
    private static void MovementType_Route19_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 0, 0x19);
    }

    // OFFSET 0x02064E88
    private static void MovementType_Route1A_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 1, 0x1A);
    }

    // OFFSET 0x02064E98
    private static void MovementType_Route1B_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 1, 0x1B);
    }

    // OFFSET 0x02064EA8
    private static void MovementType_Route1C_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 0, 0x1C);
    }

    // OFFSET 0x02064EB8
    private static void MovementType_Route1D_Init(MapObject mapObject)
    {
        MovementType_InitRoute(mapObject, 2, 0, 0x1D);
    }

    // OFFSET 0x02064EC8
    private static void MovementType_Route_Move(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        while (sRouteStates[U8(data, RouteStateOffset)](mapObject, data) == 1)
        {
        }
    }

    // OFFSET 0x02064EEC
    private static int MovementType_Route_StateStep(MapObject mapObject, byte[] data)
    {
        if (U8(data, RouteIndexOffset) == U8(data, RouteTurnIndexOffset))
        {
            if (U8(data, RouteAxisOffset) == 0)
            {
                if (MapObject_GetXInitial(mapObject) == MapObject_GetX(mapObject))
                {
                    W8(data, RouteIndexOffset, U8(data, RouteIndexOffset) + 1);
                }
            }
            else
            {
                if (MapObject_GetZInitial(mapObject) == MapObject_GetZ(mapObject))
                {
                    W8(data, RouteIndexOffset, U8(data, RouteIndexOffset) + 1);
                }
            }
        }
        if (U8(data, RouteIndexOffset) == RouteLastIndex)
        {
            int initialX = MapObject_GetXInitial(mapObject);
            int initialZ = MapObject_GetZInitial(mapObject);
            int x = MapObject_GetX(mapObject);
            int z = MapObject_GetZ(mapObject);
            if (initialX == x && initialZ == z)
            {
                W8(data, RouteIndexOffset, 0);
            }
        }
        uint list = DirectionList_Get(S32(data, RouteListOffset));
        int dir = ListEntry(list, U8(data, RouteIndexOffset));
        MapObject_Turn(mapObject, dir);
        if (MapObject_IsWalkSpinTrainer(mapObject) == 0)
        {
            MapObject_TryFace(mapObject, dir);
        }
        uint collision = MapObject_CheckCollisionInDir(mapObject, dir);
        if ((collision & CollisionOutOfRange) != 0)
        {
            W8(data, RouteIndexOffset, U8(data, RouteIndexOffset) + 1);
            dir = ListEntry(list, U8(data, RouteIndexOffset));
            MapObject_Turn(mapObject, dir);
            if (MapObject_IsWalkSpinTrainer(mapObject) == 0)
            {
                MapObject_TryFace(mapObject, dir);
            }
            collision = MapObject_CheckCollisionInDir(mapObject, dir);
        }
        int action = ActionWalkNormal;
        if (collision != 0)
        {
            action = ActionWalkOnSpotNormal;
        }
        action = MovementActions.MovementAction_TurnActionTowardsDir(dir, action);
        MovementActions.MapObject_SetMovementActionForMove(mapObject, action);
        if (MapObject_IsWalkSpinTrainer(mapObject) == 1)
        {
            WalkSpin_Turn(mapObject, data, RouteWalkSpinOffset);
        }
        MapObject_SetMoving(mapObject);
        W8(data, RouteStateOffset, 1);
        return 1;
    }

    // OFFSET 0x02064FFC
    private static int MovementType_Route_StateWaitAction(MapObject mapObject, byte[] data)
    {
        if (MovementActions.MapObject_DoMovementActionAndCheckEnd(mapObject) == 1)
        {
            MapObject_SetNotMoving(mapObject);
            if (MapObject_IsWalkSpinTrainer(mapObject) == 1)
            {
                WalkSpin_End(mapObject, data, RouteWalkSpinOffset);
            }
            W8(data, RouteStateOffset, 0);
        }
        return 0;
    }

    // OFFSET 0x0206502C
    private static void MovementType_InitReversibleRoute(MapObject mapObject, int turnIndex, int axis, int directionListID)
    {
        byte[] data = MapObject_InitMovementTypeData(mapObject, RouteDataSize);
        W8(data, RouteTurnIndexOffset, turnIndex);
        W8(data, RouteAxisOffset, axis);
        W8(data, RouteListOffset, directionListID);
        if (MapObject_IsWalkSpinTrainer(mapObject) == 1)
        {
            WalkSpin_Init(mapObject, data, RouteWalkSpinOffset);
        }
    }

    // OFFSET 0x0206505C
    private static void MovementType_ReversibleRoute1E_Init(MapObject mapObject)
    {
        MovementType_InitReversibleRoute(mapObject, 2, 1, 0x1E);
    }

    // OFFSET 0x0206506C
    private static void MovementType_ReversibleRoute1F_Init(MapObject mapObject)
    {
        MovementType_InitReversibleRoute(mapObject, 2, 1, 0x1F);
    }

    // OFFSET 0x0206507C
    private static void MovementType_ReversibleRoute20_Init(MapObject mapObject)
    {
        MovementType_InitReversibleRoute(mapObject, 2, 0, 0x20);
    }

    // OFFSET 0x0206508C
    private static void MovementType_ReversibleRoute21_Init(MapObject mapObject)
    {
        MovementType_InitReversibleRoute(mapObject, 2, 0, 0x21);
    }

    // OFFSET 0x0206509C
    private static void MovementType_ReversibleRoute22_Init(MapObject mapObject)
    {
        MovementType_InitReversibleRoute(mapObject, 2, 1, 0x22);
    }

    // OFFSET 0x020650AC
    private static void MovementType_ReversibleRoute23_Init(MapObject mapObject)
    {
        MovementType_InitReversibleRoute(mapObject, 2, 1, 0x23);
    }

    // OFFSET 0x020650BC
    private static void MovementType_ReversibleRoute24_Init(MapObject mapObject)
    {
        MovementType_InitReversibleRoute(mapObject, 2, 0, 0x24);
    }

    // OFFSET 0x020650CC
    private static void MovementType_ReversibleRoute25_Init(MapObject mapObject)
    {
        MovementType_InitReversibleRoute(mapObject, 2, 0, 0x25);
    }

    // OFFSET 0x020650DC
    private static void MovementType_ReversibleRoute_Move(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        while (sReversibleRouteStates[U8(data, RouteStateOffset)](mapObject, data) == 1)
        {
        }
    }

    // OFFSET 0x02065100
    private static void MovementType_ReversibleRoute_Advance(byte[] data)
    {
        if (U8(data, RouteReverseOffset) == 1)
        {
            W8(data, RouteIndexOffset, S8(data, RouteIndexOffset) - 1);
            if (S8(data, RouteIndexOffset) < 0)
            {
                W8(data, RouteIndexOffset, RouteLastIndex);
            }
        }
        else
        {
            W8(data, RouteIndexOffset, S8(data, RouteIndexOffset) + 1);
        }
    }

    // OFFSET 0x02065124
    private static int MovementType_ReversibleRoute_StateCheckPlayer(MapObject mapObject, byte[] data)
    {
        if (MapObject_GetTrainerType(mapObject) == TrainerTypeApproach)
        {
            PlayerAvatar playerAvatar = PlayerAvatar.FieldSystem_GetPlayerAvatar(MapObject_FieldSystem(mapObject)!)!;
            int dir = MapObject_GetFacingDir(mapObject);
            int range = MapObject_GetDataAt(mapObject, 0);
            int distance = TrainerEncounter.MapObject_GetDistanceToPlayer(mapObject, playerAvatar, dir, range);
            if (distance != -1)
            {
                MovementActions.MovementAction_TurnActionTowardsDir(dir, ActionJumpOnSpotFast);
                MovementActions.MapObject_SetMovementActionForMove(mapObject, distance);
                MapObject_SetMoving(mapObject);
                W8(data, RouteStateOffset, 1);
                return 1;
            }
        }
        W8(data, RouteStateOffset, 2);
        return 1;
    }

    // OFFSET 0x02065188
    private static int MovementType_ReversibleRoute_StateWaitJump(MapObject mapObject, byte[] data)
    {
        if (MovementActions.MapObject_DoMovementActionAndCheckEnd(mapObject) == 1)
        {
            MapObject_SetNotMoving(mapObject);
            W8(data, RouteStateOffset, 2);
        }
        return 0;
    }

    // OFFSET 0x020651A4
    private static int MovementType_ReversibleRoute_StateStep(MapObject mapObject, byte[] data)
    {
        if (S8(data, RouteIndexOffset) == U8(data, RouteTurnIndexOffset))
        {
            if (U8(data, RouteAxisOffset) == 0)
            {
                if (MapObject_GetXInitial(mapObject) == MapObject_GetX(mapObject))
                {
                    MovementType_ReversibleRoute_Advance(data);
                }
            }
            else
            {
                if (MapObject_GetZInitial(mapObject) == MapObject_GetZ(mapObject))
                {
                    MovementType_ReversibleRoute_Advance(data);
                }
            }
        }
        if (S8(data, RouteIndexOffset) == RouteLastIndex)
        {
            int initialX = MapObject_GetXInitial(mapObject);
            int initialZ = MapObject_GetZInitial(mapObject);
            int x = MapObject_GetX(mapObject);
            int z = MapObject_GetZ(mapObject);
            if (initialX == x && initialZ == z)
            {
                W8(data, RouteIndexOffset, 0);
            }
        }
        uint list = DirectionList_Get(U8(data, RouteListOffset));
        int dir = ListEntry(list, S8(data, RouteIndexOffset));
        MapObject_Turn(mapObject, dir);
        if (MapObject_IsWalkSpinTrainer(mapObject) == 0)
        {
            MapObject_TryFace(mapObject, dir);
        }
        uint collision = MapObject_CheckCollisionInDir(mapObject, dir);
        if ((collision & CollisionOutOfRange) != 0)
        {
            MovementType_ReversibleRoute_Advance(data);
            dir = ListEntry(list, S8(data, RouteIndexOffset));
            MapObject_Turn(mapObject, dir);
            if (MapObject_IsWalkSpinTrainer(mapObject) == 0)
            {
                MapObject_TryFace(mapObject, dir);
            }
            collision = MapObject_CheckCollisionInDir(mapObject, dir);
        }
        int action = ActionWalkNormal;
        if (collision != 0)
        {
            action = ActionWalkOnSpotNormal;
        }
        action = MovementActions.MovementAction_TurnActionTowardsDir(dir, action);
        MovementActions.MapObject_SetMovementActionForMove(mapObject, action);
        if (MapObject_IsWalkSpinTrainer(mapObject) == 1)
        {
            WalkSpin_Turn(mapObject, data, RouteWalkSpinOffset);
        }
        MapObject_SetMoving(mapObject);
        W8(data, RouteStateOffset, 3);
        return 1;
    }

    // OFFSET 0x020652BC
    private static int MovementType_ReversibleRoute_StateWaitAction(MapObject mapObject, byte[] data)
    {
        if (MovementActions.MapObject_DoMovementActionAndCheckEnd(mapObject) == 1)
        {
            MapObject_SetNotMoving(mapObject);
            if (MapObject_IsWalkSpinTrainer(mapObject) == 1)
            {
                WalkSpin_End(mapObject, data, RouteWalkSpinOffset);
            }
            W8(data, RouteStateOffset, 0);
        }
        return 0;
    }

    // OFFSET 0x020652EC
    private static int IntList_Count(uint list, int terminator)
    {
        int count = 0;
        while (ListEntry(list, count) != terminator)
        {
            count++;
        }
        if (count == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        return count;
    }

    // OFFSET 0x0206530C
    private static int IntList_GetRandom(uint list, int terminator)
    {
        int random = MathUtil.LCRNG_Next();
        int count = IntList_Count(list, terminator);
        return ListEntry(list, random % count);
    }

    // OFFSET 0x02065330
    private static int DirectionList_GetRandom(int directionListID, int terminator)
    {
        uint list = DirectionList_Get(directionListID);
        int random = MathUtil.LCRNG_Next();
        int count = IntList_Count(list, terminator);
        return ListEntry(list, random % count);
    }

    // OFFSET 0x02065358
    private static uint DirectionList_Get(int directionListID)
    {
        uint entry = DirectionListsAddress;
        int id = 0;
        do
        {
            if (id == directionListID)
            {
                return (uint)ListEntry(entry, 1);
            }
            entry += 8;
            id = ListEntry(entry, 0);
        }
        while (id != DirectionListsEnd);
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x0206537C
    private static int MapObject_GetDirTowardsRunningPlayer(MapObject mapObject)
    {
        int trainerType = MapObject_GetTrainerType(mapObject);
        if (trainerType != TrainerTypeSightStraight && trainerType != TrainerTypeSightAround)
        {
            return DirNone;
        }
        PlayerAvatar playerAvatar = PlayerAvatar.FieldSystem_GetPlayerAvatar(MapObject_FieldSystem(mapObject)!)!;
        if (PlayerMove.PlayerAvatar_IsRunning(playerAvatar) == 0)
        {
            return DirNone;
        }
        int index = 0;
        int movementType = MapObject_GetMovementType(mapObject);
        int listedType;
        do
        {
            listedType = ListEntry(RunnerAwareMovementTypesAddress, index);
            index++;
        }
        while (listedType != movementType && listedType != MovementTypesEnd);
        if (movementType != listedType)
        {
            return DirNone;
        }
        int playerY = MapObject_GetPosYAsTile(PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar));
        if (playerY != MapObject_GetPosYAsTile(mapObject))
        {
            return DirNone;
        }
        int playerX = PlayerAvatar.PlayerAvatar_GetXPos(playerAvatar);
        int playerZ = PlayerAvatar.PlayerAvatar_GetZPos(playerAvatar);
        int range = MapObject_GetDataAt(mapObject, 0);
        int x = MapObject_GetX(mapObject);
        int z = MapObject_GetZ(mapObject);
        if (z - range > playerZ || z + range < playerZ || x - range > playerX || x + range < playerX)
        {
            return DirNone;
        }
        return GetDirectionBetweenPoints(x, z, playerX, playerZ);
    }

    // OFFSET 0x02065448
    private static int MapObject_GetListDirTowardsRunningPlayer(MapObject mapObject, int directionListID, int terminator)
    {
        uint list = DirectionList_Get(directionListID);
        int count = IntList_Count(list, terminator);
        if (count == 1)
        {
            return DirNone;
        }
        int dir = MapObject_GetDirTowardsRunningPlayer(mapObject);
        if (dir == DirNone)
        {
            return dir;
        }
        int index = 0;
        do
        {
            if (dir == ListEntry(list, index))
            {
                return dir;
            }
            index++;
        }
        while (index < count);
        int dirX = DirNone;
        int dirZ = DirNone;
        int x = MapObject_GetX(mapObject);
        int z = MapObject_GetZ(mapObject);
        PlayerAvatar playerAvatar = PlayerAvatar.FieldSystem_GetPlayerAvatar(MapObject_FieldSystem(mapObject)!)!;
        int playerX = PlayerAvatar.PlayerAvatar_GetXPos(playerAvatar);
        int playerZ = PlayerAvatar.PlayerAvatar_GetZPos(playerAvatar);
        if (x > playerX)
        {
            dirX = DirWest;
        }
        else if (x < playerX)
        {
            dirX = DirEast;
        }
        if (z > playerZ)
        {
            dirZ = DirNorth;
        }
        else if (z < playerZ)
        {
            dirZ = DirSouth;
        }
        index = 0;
        if (dirX == DirNone)
        {
            do
            {
                if (dirZ == ListEntry(list, index))
                {
                    return dirZ;
                }
                index++;
            }
            while (index < count);
        }
        else if (dirZ == DirNone)
        {
            do
            {
                if (dirX == ListEntry(list, index))
                {
                    return dirX;
                }
                index++;
            }
            while (index < count);
        }
        else
        {
            do
            {
                int listed = ListEntry(list, index);
                if (dirX == listed)
                {
                    return dirX;
                }
                if (dirZ == listed)
                {
                    return dirZ;
                }
                index++;
            }
            while (index < count);
        }
        return DirNone;
    }

    // OFFSET 0x0206553C
    private static int MapObject_IsWalkSpinTrainer(MapObject mapObject)
    {
        if ((uint)(MapObject_GetTrainerType(mapObject) - TrainerTypeWalkSpinCounterclockwise) <= 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02065550
    private static void WalkSpin_Init(MapObject mapObject, byte[] data, int offset)
    {
        if (MapObject_GetTrainerType(mapObject) == TrainerTypeWalkSpinCounterclockwise)
        {
            W8(data, offset + WalkSpinRotationOffset, 0);
        }
        else
        {
            W8(data, offset + WalkSpinRotationOffset, 1);
        }
    }

    // OFFSET 0x02065568
    private static void WalkSpin_Turn(MapObject mapObject, byte[] data, int offset)
    {
        int facingDir = MapObject_GetFacingDir(mapObject);
        int index = 0;
        while (index < DirCount)
        {
            if (facingDir == WalkSpinDir(S8(data, offset + WalkSpinRotationOffset), index))
            {
                break;
            }
            index++;
        }
        if (index >= DirCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        int next = (index + 1) % DirCount;
        W8(data, offset + WalkSpinPrevDirOffset, facingDir);
        int dir = WalkSpinDir(S8(data, offset + WalkSpinRotationOffset), next);
        if (MapObject_CheckStatus(mapObject, StatusLockDir) != 0)
        {
            W8(data, offset + WalkSpinWasLockedOffset, 1);
        }
        else
        {
            W8(data, offset + WalkSpinWasLockedOffset, 0);
        }
        MapObject_TryFace(mapObject, dir);
        MapObject_SetStatusFlagOn(mapObject, StatusLockDir);
    }

    // OFFSET 0x020655E4
    private static void WalkSpin_End(MapObject mapObject, byte[] data, int offset)
    {
        if (U8(data, offset + WalkSpinWasLockedOffset) == 0)
        {
            MapObject_SetStatusFlagOff(mapObject, StatusLockDir);
        }
    }

    private static int NextSpinDir(uint list, int facingDir)
    {
        int index = 0;
        if (ListEntry(list, 0) != ListEnd)
        {
            while (facingDir != ListEntry(list, index))
            {
                index++;
                if (ListEntry(list, index) == ListEnd)
                {
                    break;
                }
            }
        }
        if (ListEntry(list, index) == ListEnd)
        {
            ErrorHandling.GF_AssertFail();
        }
        index++;
        if (ListEntry(list, index) == ListEnd)
        {
            index = 0;
        }
        return ListEntry(list, index);
    }

    private static int WalkSpinDir(int rotation, int index)
    {
        return ListEntry(WalkSpinDirsAddress, rotation * DirCount + index);
    }

    private static int ListEntry(uint list, int index)
    {
        if (sListRegion == null)
        {
            int count = (int)(ListRegionEnd - ListRegionAddress) / 4;
            uint[] words = RomData.Arm9U32(ListRegionAddress, count);
            sListRegion = new int[count];
            for (int i = 0; i < count; i++)
            {
                sListRegion[i] = (int)words[i];
            }
        }
        return sListRegion[(int)(list - ListRegionAddress) / 4 + index];
    }
}
