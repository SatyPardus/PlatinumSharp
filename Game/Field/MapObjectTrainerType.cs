using static PlatinumSharp.Game.MapObject;
using static PlatinumSharp.Game.MapObjectBuffer;

namespace PlatinumSharp.Game;

public delegate int MapObjectTrainerTypeFunc(MapObject mapObject);

public static class MapObjectTrainerType
{
    public const int TrainerTypeCount = 12;
    public const int TrainerTypeLookSideways = 4;
    public const int TrainerTypeSpinCounterclockwise = 5;
    public const int TrainerTypeSpinClockwise = 6;

    private const uint SidewaysRowsAddress = 0x020EF5F0;
    private const uint SidewaysDirsAddress = 0x020EF600;
    private const uint SpinDirsAddress = 0x020EF610;
    private const int DirCount = 4;
    private const int SidewaysDirCount = 2;
    private const int SpinRotationCount = 2;

    private const int DataSize = 9;
    private const int WatchStateOffset = 0x0;
    private const int LookStateOffset = 0x1;
    private const int StepCountOffset = 0x2;
    private const int StepsBeforeLookOffset = 0x3;
    private const int SavedDirOffset = 0x4;
    private const int RowOffset = 0x5;
    private const int DirIndexOffset = 0x6;
    private const int LookCountOffset = 0x7;
    private const int DelayOffset = 0x8;
    private const int LookDelayFrames = 8;
    private const int LookCountMax = 4;
    private const int ActionFace = 0;

    private static readonly MapObjectFunc[] sInitFuncs =
    {
        TrainerType_InitNone,
        TrainerType_InitNone,
        TrainerType_InitNone,
        TrainerType_InitNone,
        TrainerType_LookSideways_Init,
        TrainerType_Spin_Init,
        TrainerType_Spin_Init,
        TrainerType_InitNone,
        TrainerType_InitNone,
        TrainerType_InitNone,
        TrainerType_InitNone,
        TrainerType_InitNone,
    };

    private static readonly MapObjectTrainerTypeFunc[] sWatchFuncs =
    {
        TrainerType_WatchNone,
        TrainerType_WatchNone,
        TrainerType_WatchNone,
        TrainerType_WatchNone,
        TrainerType_LookSideways_Watch,
        TrainerType_Spin_Watch,
        TrainerType_Spin_Watch,
        TrainerType_WatchNone,
        TrainerType_WatchNone,
        TrainerType_WatchNone,
        TrainerType_WatchNone,
        TrainerType_WatchNone,
    };

    private static readonly MapObjectTrainerTypeFunc[] sLookFuncs =
    {
        TrainerType_LookNone,
        TrainerType_LookNone,
        TrainerType_LookNone,
        TrainerType_LookNone,
        TrainerType_LookSideways_Look,
        TrainerType_Spin_Look,
        TrainerType_Spin_Look,
        TrainerType_LookNone,
        TrainerType_LookNone,
        TrainerType_LookNone,
        TrainerType_LookNone,
        TrainerType_LookNone,
    };

    private static int[]? sSidewaysRows;
    private static int[]? sSidewaysDirs;
    private static int[]? sSpinDirs;

    // OFFSET 0x020673B8
    public static void MapObject_InitTrainerType(MapObject mapObject)
    {
        MapObject_CallTrainerTypeInit(mapObject);
    }

    // OFFSET 0x020673C0
    public static int MapObject_UpdateTrainerType(MapObject mapObject)
    {
        if (MapObject_CallTrainerTypeWatch(mapObject) == 0)
        {
            return 0;
        }
        if (MapObject_CallTrainerTypeLook(mapObject) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020673E4
    private static void MapObject_CallTrainerTypeInit(MapObject mapObject)
    {
        sInitFuncs[MapObject_GetTrainerType(mapObject)](mapObject);
    }

    // OFFSET 0x02067400
    private static int MapObject_CallTrainerTypeWatch(MapObject mapObject)
    {
        return sWatchFuncs[MapObject_GetTrainerType(mapObject)](mapObject);
    }

    // OFFSET 0x0206741C
    private static int MapObject_CallTrainerTypeLook(MapObject mapObject)
    {
        return sLookFuncs[MapObject_GetTrainerType(mapObject)](mapObject);
    }

    // OFFSET 0x02067438
    private static int MapObject_HasChangedTile(MapObject mapObject)
    {
        if (MapObject_GetX(mapObject) != MapObject_GetXPrev(mapObject))
        {
            return 1;
        }
        if (MapObject_GetZ(mapObject) != MapObject_GetZPrev(mapObject))
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0206746C
    private static int MapObject_IsOnPrevTile(MapObject mapObject)
    {
        if (MapObject_GetX(mapObject) != MapObject_GetXPrev(mapObject))
        {
            return 0;
        }
        if (MapObject_GetZ(mapObject) == MapObject_GetZPrev(mapObject))
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020674A0
    private static void TrainerType_InitNone(MapObject mapObject)
    {
    }

    // OFFSET 0x020674A4
    private static int TrainerType_WatchNone(MapObject mapObject)
    {
        return 0;
    }

    // OFFSET 0x020674A8
    private static int TrainerType_LookNone(MapObject mapObject)
    {
        return 0;
    }

    // OFFSET 0x020674AC
    private static void TrainerType_LookSideways_Init(MapObject mapObject)
    {
        byte[] data = MapObject_InitTrainerTypeData(mapObject, DataSize);
        W8(data, StepsBeforeLookOffset, MapObject_GetDataAt(mapObject, 1));
    }

    // OFFSET 0x020674C4
    private static int TrainerType_LookSideways_Watch(MapObject mapObject)
    {
        byte[] data = MapObject_GetTrainerTypeData(mapObject);
        int state = U8(data, WatchStateOffset);
        if (state == 0)
        {
            if (MapObject_HasChangedTile(mapObject) == 1)
            {
                W8(data, WatchStateOffset, U8(data, WatchStateOffset) + 1);
            }
            return 0;
        }
        if (state == 1)
        {
            if (MapObject_IsOnPrevTile(mapObject) == 0)
            {
                return 0;
            }
            W8(data, StepCountOffset, S8(data, StepCountOffset) + 1);
            if (S8(data, StepCountOffset) < S8(data, StepsBeforeLookOffset))
            {
                W8(data, WatchStateOffset, 0);
                return 0;
            }
            W8(data, WatchStateOffset, U8(data, WatchStateOffset) + 1);
            state = 2;
        }
        if (state == 2)
        {
            if (MapObject_IsMoving(mapObject) == 1)
            {
                return 0;
            }
            W8(data, WatchStateOffset, U8(data, WatchStateOffset) + 1);
            W8(data, StepCountOffset, 0);
            W8(data, LookStateOffset, 0);
            state = 3;
        }
        if (state == 3)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02067540
    private static int TrainerType_LookSideways_Look(MapObject mapObject)
    {
        byte[] data = MapObject_GetTrainerTypeData(mapObject);
        int state = U8(data, LookStateOffset);
        if (state == 0)
        {
            sSidewaysRows ??= RomInts(SidewaysRowsAddress, DirCount);
            int facingDir = MapObject_GetFacingDir(mapObject);
            W8(data, SavedDirOffset, facingDir);
            W8(data, RowOffset, sSidewaysRows[facingDir]);
            W8(data, LookStateOffset, U8(data, LookStateOffset) + 1);
            state = 1;
        }
        if (state == 1)
        {
            sSidewaysDirs ??= RomInts(SidewaysDirsAddress, SidewaysDirCount * SidewaysDirCount);
            int dir = sSidewaysDirs[S8(data, RowOffset) * SidewaysDirCount + S8(data, DirIndexOffset)];
            int action = MovementActions.MovementAction_TurnActionTowardsDir(dir, ActionFace);
            MovementActions.MapObject_SetMovementActionForMove(mapObject, action);
            W8(data, LookStateOffset, U8(data, LookStateOffset) + 1);
            state = 2;
        }
        if (state == 2)
        {
            if (MovementActions.MapObject_DoMovementActionAndCheckEnd(mapObject) == 0)
            {
                return 1;
            }
            W8(data, LookStateOffset, U8(data, LookStateOffset) + 1);
            state = 3;
        }
        if (state == 3)
        {
            W8(data, DelayOffset, S8(data, DelayOffset) + 1);
            if (S8(data, DelayOffset) < LookDelayFrames)
            {
                return 1;
            }
            W8(data, DelayOffset, 0);
            W8(data, LookCountOffset, S8(data, LookCountOffset) + 1);
            if (S8(data, LookCountOffset) < LookCountMax)
            {
                W8(data, DirIndexOffset, (S8(data, DirIndexOffset) + 1) & 1);
                W8(data, LookStateOffset, 1);
                return 1;
            }
            MapObject_TryFace(mapObject, S8(data, SavedDirOffset));
            W8(data, LookStateOffset, U8(data, LookStateOffset) + 1);
            W8(data, LookCountOffset, 0);
            W8(data, WatchStateOffset, 0);
        }
        return 0;
    }

    // OFFSET 0x0206762C
    private static void TrainerType_Spin_Init(MapObject mapObject)
    {
        byte[] data = MapObject_InitTrainerTypeData(mapObject, DataSize);
        W8(data, StepsBeforeLookOffset, MapObject_GetDataAt(mapObject, 1));
        if (MapObject_GetTrainerType(mapObject) == TrainerTypeSpinCounterclockwise)
        {
            W8(data, RowOffset, 0);
        }
        else
        {
            W8(data, RowOffset, 1);
        }
    }

    // OFFSET 0x02067658
    private static int TrainerType_Spin_Watch(MapObject mapObject)
    {
        byte[] data = MapObject_GetTrainerTypeData(mapObject);
        int state = U8(data, WatchStateOffset);
        if (state == 0)
        {
            if (MapObject_HasChangedTile(mapObject) == 1)
            {
                W8(data, WatchStateOffset, U8(data, WatchStateOffset) + 1);
            }
            return 0;
        }
        if (state == 1)
        {
            if (MapObject_IsOnPrevTile(mapObject) == 0)
            {
                return 0;
            }
            W8(data, StepCountOffset, S8(data, StepCountOffset) + 1);
            if (S8(data, StepCountOffset) < S8(data, StepsBeforeLookOffset))
            {
                W8(data, WatchStateOffset, 0);
                return 0;
            }
            W8(data, WatchStateOffset, U8(data, WatchStateOffset) + 1);
            state = 2;
        }
        if (state == 2)
        {
            if (MapObject_IsMoving(mapObject) == 1)
            {
                return 0;
            }
            W8(data, WatchStateOffset, U8(data, WatchStateOffset) + 1);
            W8(data, StepCountOffset, 0);
            W8(data, LookStateOffset, 0);
            state = 3;
        }
        if (state == 3)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020676D4
    private static int TrainerType_Spin_Look(MapObject mapObject)
    {
        sSpinDirs ??= RomInts(SpinDirsAddress, SpinRotationCount * DirCount);
        int[] spinDirs = sSpinDirs;
        byte[] data = MapObject_GetTrainerTypeData(mapObject);
        int state = U8(data, LookStateOffset);
        if (state == 0)
        {
            int facingDir = MapObject_GetFacingDir(mapObject);
            int index = 0;
            while (index < DirCount)
            {
                if (facingDir == spinDirs[S8(data, RowOffset) * DirCount + index])
                {
                    break;
                }
                index++;
            }
            if (index >= DirCount)
            {
                ErrorHandling.GF_AssertFail();
            }
            W8(data, SavedDirOffset, facingDir);
            W8(data, DirIndexOffset, (index + 1) % DirCount);
            W8(data, LookStateOffset, U8(data, LookStateOffset) + 1);
            state = 1;
        }
        if (state == 1)
        {
            int dir = spinDirs[S8(data, RowOffset) * DirCount + S8(data, DirIndexOffset)];
            int action = MovementActions.MovementAction_TurnActionTowardsDir(dir, ActionFace);
            MovementActions.MapObject_SetMovementActionForMove(mapObject, action);
            W8(data, LookStateOffset, U8(data, LookStateOffset) + 1);
            state = 2;
        }
        if (state == 2)
        {
            if (MovementActions.MapObject_DoMovementActionAndCheckEnd(mapObject) == 0)
            {
                return 1;
            }
            W8(data, LookStateOffset, U8(data, LookStateOffset) + 1);
            state = 3;
        }
        if (state == 3)
        {
            W8(data, DelayOffset, S8(data, DelayOffset) + 1);
            if (S8(data, DelayOffset) < LookDelayFrames)
            {
                return 1;
            }
            W8(data, DelayOffset, 0);
            W8(data, LookCountOffset, S8(data, LookCountOffset) + 1);
            if (S8(data, LookCountOffset) < LookCountMax)
            {
                W8(data, DirIndexOffset, (S8(data, DirIndexOffset) + 1) % DirCount);
                W8(data, LookStateOffset, 1);
                return 1;
            }
            MapObject_TryFace(mapObject, S8(data, SavedDirOffset));
            W8(data, LookStateOffset, U8(data, LookStateOffset) + 1);
            W8(data, LookCountOffset, 0);
            W8(data, WatchStateOffset, 0);
        }
        return 0;
    }

    private static int[] RomInts(uint address, int count)
    {
        uint[] words = RomData.Arm9U32(address, count);
        int[] values = new int[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = (int)words[i];
        }
        return values;
    }
}
