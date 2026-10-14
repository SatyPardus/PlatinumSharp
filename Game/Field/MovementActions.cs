using static PlatinumSharp.Game.MapObject;
using static PlatinumSharp.Game.MapObjectBuffer;
using static PlatinumSharp.Game.MapObjectMove;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate int MovementActionStepFunc(MapObject mapObject);

public delegate int MovementAnimationFunc(MoveAnimData data);

public sealed class MoveAnimData
{
    public const int Size = 0x14;

    public int State;
    public int Finished;
    public int Counter;
    public MapObject? MapObject;
    public BufferView? AnimCmd;
}

public static partial class MovementActions
{
    public const int MovementActionCount = 0x9A;
    public const int MovementActionEnd = 0xFE;
    public const int MovementActionNone = 0xFF;

    public const int AnimCmdSize = 4;
    public const int AnimCmdActionOffset = 0x0;
    public const int AnimCmdCountOffset = 0x2;

    private const uint ActionStepsTableAddress = 0x020EE4B8;
    private const uint ActionDirTablesAddress = 0x020EE31C;
    private const uint JumpHeightTablesAddress = 0x02100B84;
    private const uint WalkSlightlyFastStepsAddress = 0x020EEE6C;
    private const uint WalkSlightlyFasterStepsAddress = 0x020EECEC;
    private const uint WalkEverSoSlightlyFastStepsAddress = 0x020EEE84;
    private const uint EndStepAddress = 0x02065924;
    private const int JumpHeightTableCount = 3;
    private const int JumpHeightTableSize = 16;
    private const int DirCount = 4;
    private const int WalkSlightlyFastStepCount = 6;
    private const int WalkSlightlyFasterStepCount = 3;
    private const int WalkEverSoSlightlyFastStepCount = 7;

    private const int WalkDataSize = 0xC;
    private const int WalkDrawStateOffset = 0x0;
    private const int WalkDurationOffset = 0x2;
    private const int WalkDirOffset = 0x4;
    private const int WalkDistanceOffset = 0x8;

    private const int JumpDataSize = 0x10;
    private const int JumpDistanceOffset = 0x0;
    private const int JumpTravelledOffset = 0x4;
    private const int JumpSpeedOffset = 0x8;
    private const int JumpProgressOffset = 0xA;
    private const int JumpDirOffset = 0xC;
    private const int JumpDurationOffset = 0xD;
    private const int JumpDrawStateOffset = 0xE;
    private const int JumpHeightTableOffset = 0xF;
    private const int JumpProgressMax = 0xF00;
    private const int TileSize = 0x10000;

    private const int DelayDataSize = 4;
    private const int WarpDataSize = 8;
    private const int WarpHeightOffset = 0x0;
    private const int WarpSpeedOffset = 0x4;
    private const int WarpOutSpeed = 0x10000;
    private const int WarpOutHeightUnit = 0x8000;
    private const int WarpOutMaxHeight = 0x28;
    private const int WarpInStartHeight = 0x140000;
    private const int WarpInSpeed = -0x10000;

    private const int EmoteDataSize = 8;
    private const int EmoteTypeOffset = 0x0;

    private const int UnevenDataSize = 8;
    private const int UnevenDirOffset = 0x0;
    private const int UnevenDrawStateOffset = 0x2;
    private const int UnevenDurationOffset = 0x4;
    private const int UnevenStepOffset = 0x6;

    private const int CounterDataSize = 4;
    private const int NurseBowFrames = 8;
    private const int NurseBowDrawState = 9;
    private const int RevealTrainerFrames = 0x10;
    private const int PlayerGiveReceiveFrames = 0x15;

    private const int VectorWalkDataSize = 0x10;
    private const int VectorWalkDurationOffset = 0x0;
    private const int VectorWalkStepOffset = 0x4;

    private const int AxisJumpDataSize = 0x10;
    private const int AxisJumpDurationOffset = 0x0;
    private const int AxisJumpHeightAxisOffset = 0x1;
    private const int AxisJumpMoveAxisOffset = 0x2;
    private const int AxisJumpInvertOffset = 0x3;
    private const int AxisJumpDistanceOffset = 0x4;
    private const int AxisJumpTravelledOffset = 0x8;
    private const int AxisJumpProgressOffset = 0xC;
    private const int AxisJumpSpeedOffset = 0xE;
    private const int AxisX = 0;
    private const int AxisY = 1;
    private const int AxisZ = 2;

    private const int SeqJump = 0x60B;
    private const int SeqLand = 0x647;

    private static readonly MovementAnimationFunc[] sMovementAnimationFuncs =
    {
        MovementAnimation_Init,
        MovementAnimation_CheckSet,
        MovementAnimation_Set,
        MovementAnimation_CheckFinished,
        MovementAnimation_Increment,
        MovementAnimation_End,
    };

    private static MovementActionStepFunc[]?[]? sActionSteps;
    private static int[][]? sActionDirTables;
    private static int[][]? sJumpHeightTables;
    private static int[]? sWalkSlightlyFastSteps;
    private static int[]? sWalkSlightlyFasterSteps;
    private static int[]? sWalkEverSoSlightlyFastSteps;
    private static readonly Dictionary<uint, VecFx32> sRomVecs = new();

    // OFFSET 0x020655F4
    public static int LocalMapObj_IsAnimationSet(MapObject mapObject)
    {
        if (!MapObject_CheckStatusFlag(mapObject, StatusActive))
        {
            return 0;
        }
        if (MapObject_CheckStatusFlag(mapObject, StatusMoving))
        {
            return 0;
        }
        if (MapObject_CheckStatusFlag(mapObject, StatusMovementAction)
            && !MapObject_CheckStatusFlag(mapObject, StatusMovementActionEnded))
        {
            return 0;
        }
        return 1;
    }

    // OFFSET 0x02065638
    public static void LocalMapObj_SetAnimationCode(MapObject mapObject, int movementAction)
    {
        if (movementAction >= MovementActionCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObject_SetMovementAction(mapObject, movementAction);
        MapObject_SetMovementStep(mapObject, 0);
        MapObject_SetStatusFlagOn(mapObject, StatusMovementAction);
        MapObject_SetStatusFlagOff(mapObject, StatusMovementActionEnded);
    }

    // OFFSET 0x02065668
    public static void MapObject_SetMovementActionForMove(MapObject mapObject, int movementAction)
    {
        MapObject_SetMovementAction(mapObject, movementAction);
        MapObject_SetMovementStep(mapObject, 0);
        MapObject_SetStatusFlagOff(mapObject, StatusMovementActionEnded);
    }

    // OFFSET 0x02065684
    public static int LocalMapObj_CheckAnimationFinished(MapObject mapObject)
    {
        if (!MapObject_CheckStatusFlag(mapObject, StatusMovementAction))
        {
            return 1;
        }
        if (MapObject_CheckStatusFlag(mapObject, StatusMovementActionEnded))
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020656AC
    public static int LocalMapObj_ClearFinishedAnimation(MapObject mapObject)
    {
        if (!MapObject_CheckStatusFlag(mapObject, StatusMovementAction))
        {
            return 1;
        }
        if (!MapObject_CheckStatusFlag(mapObject, StatusMovementActionEnded))
        {
            return 0;
        }
        MapObject_SetStatusFlagOff(mapObject, StatusMovementAction | StatusMovementActionEnded);
        return 1;
    }

    // OFFSET 0x020656DC
    public static void MapObject_ClearMovementAction(MapObject mapObject)
    {
        MapObject_SetStatusFlagOff(mapObject, StatusMovementAction);
        MapObject_SetStatusFlagOn(mapObject, StatusMovementActionEnded);
        MapObject_SetMovementAction(mapObject, MovementActionNone);
        MapObject_SetMovementStep(mapObject, 0);
    }

    // OFFSET 0x02065700
    public static SysTask MapObject_StartAnimation(MapObject mapObject, BufferView animCmd)
    {
        MoveAnimData data = new();
        int priority = MapObjectManager.MapObjectMan_GetTaskBasePriority(MapObject_MapObjectManager(mapObject)!) - 1;
        SysTask? task = SysTask.SysTask_Start(MapObject_DoAnimation, data, (uint)priority);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        data.MapObject = mapObject;
        data.AnimCmd = animCmd;
        return task!;
    }

    // OFFSET 0x0206574C
    public static int MapObject_HasAnimationEnded(SysTask task)
    {
        MoveAnimData data = (MoveAnimData)SysTaskManager.SysTask_GetParam(task)!;
        return data.Finished;
    }

    // OFFSET 0x02065758
    public static void MapObject_FinishAnimation(SysTask task)
    {
        MoveAnimData data = (MoveAnimData)SysTaskManager.SysTask_GetParam(task)!;
        if (LocalMapObj_CheckAnimationFinished(data.MapObject!) != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        LocalMapObj_ClearFinishedAnimation(data.MapObject!);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x02065788
    private static void MapObject_DoAnimation(SysTask task, object? param)
    {
        MoveAnimData data = (MoveAnimData)param!;
        while (sMovementAnimationFuncs[data.State](data) == 1)
        {
        }
    }

    // OFFSET 0x020657A4
    private static int MovementAnimation_Init(MoveAnimData data)
    {
        data.Counter = 0;
        data.State = 1;
        return 1;
    }

    // OFFSET 0x020657B0
    private static int MovementAnimation_CheckSet(MoveAnimData data)
    {
        if (LocalMapObj_IsAnimationSet(data.MapObject!) == 0)
        {
            return 0;
        }
        data.State = 2;
        return 1;
    }

    // OFFSET 0x020657CC
    private static int MovementAnimation_Set(MoveAnimData data)
    {
        LocalMapObj_SetAnimationCode(data.MapObject!, data.AnimCmd!.U16(AnimCmdActionOffset));
        data.State = 3;
        return 0;
    }

    // OFFSET 0x020657E4
    private static int MovementAnimation_CheckFinished(MoveAnimData data)
    {
        if (LocalMapObj_CheckAnimationFinished(data.MapObject!) == 0)
        {
            return 0;
        }
        data.State = 4;
        return 1;
    }

    // OFFSET 0x02065800
    private static int MovementAnimation_Increment(MoveAnimData data)
    {
        data.Counter++;
        BufferView animCmd = data.AnimCmd!;
        if (data.Counter < animCmd.U16(AnimCmdCountOffset))
        {
            data.State = 1;
            return 1;
        }
        data.AnimCmd = new BufferView(animCmd.Buffer, animCmd.Offset + AnimCmdSize);
        if (animCmd.U16(AnimCmdSize + AnimCmdActionOffset) != MovementActionEnd)
        {
            data.State = 0;
            return 1;
        }
        data.Finished = 1;
        data.State = 5;
        return 0;
    }

    // OFFSET 0x02065834
    private static int MovementAnimation_End(MoveAnimData data)
    {
        return 0;
    }

    // OFFSET 0x02065838
    public static int MovementAction_TurnActionTowardsDir(int targetDir, int movementAction)
    {
        if (targetDir >= DirCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        int[][] tables = ActionDirTables();
        for (int i = 0; i < tables.Length; i++)
        {
            int[] table = tables[i];
            for (int dir = 0; dir < DirCount; dir++)
            {
                if (movementAction == table[dir])
                {
                    return table[targetDir];
                }
            }
        }
        ErrorHandling.GF_AssertFail();
        return movementAction;
    }

    // OFFSET 0x0206587C
    public static int MovementAction_GetDirFromAction(int movementAction)
    {
        int[][] tables = ActionDirTables();
        for (int i = 0; i < tables.Length; i++)
        {
            int[] table = tables[i];
            for (int dir = 0; dir < DirCount; dir++)
            {
                if (movementAction == table[dir])
                {
                    return dir;
                }
            }
        }
        return DirNone;
    }

    // OFFSET 0x020658B4
    public static void MapObject_DoMovementAction(MapObject mapObject)
    {
        while (true)
        {
            int movementAction = MapObject_GetMovementAction(mapObject);
            if (movementAction == MovementActionNone)
            {
                break;
            }
            int movementStep = MapObject_GetMovementStep(mapObject);
            if (MapObject_DoMovementActionStep(mapObject, movementAction, movementStep) == 0)
            {
                break;
            }
        }
    }

    // OFFSET 0x020658DC
    public static int MapObject_DoMovementActionAndCheckEnd(MapObject mapObject)
    {
        MapObject_DoMovementAction(mapObject);
        if (!MapObject_CheckStatusFlag(mapObject, StatusMovementActionEnded))
        {
            return 0;
        }
        MapObject_SetStatusFlagOff(mapObject, StatusMovementActionEnded);
        MapObject_SetMovementAction(mapObject, MovementActionNone);
        MapObject_SetMovementStep(mapObject, 0);
        return 1;
    }

    // OFFSET 0x02065910
    private static int MapObject_DoMovementActionStep(MapObject mapObject, int movementAction, int movementStep)
    {
        return ActionSteps(movementAction)[movementStep](mapObject);
    }

    // OFFSET 0x02065924
    private static int MovementAction_End(MapObject mapObject)
    {
        MapObject_SetStatusFlagOn(mapObject, StatusMovementActionEnded);
        return 0;
    }

    // OFFSET 0x02065930
    private static void MovementAction_InitFace(MapObject mapObject, int dir)
    {
        MapObject_TryFace(mapObject, dir);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_UpdateCoords(mapObject);
        MapObject_AdvanceMovementStep(mapObject);
    }

    // OFFSET 0x02065980
    private static void MovementAction_InitWalk(MapObject mapObject, int dir, int distance, int duration, int drawState)
    {
        byte[] data = MapObject_InitMovementData(mapObject, WalkDataSize);
        W16(data, WalkDrawStateOffset, drawState);
        W16(data, WalkDurationOffset, duration);
        W32(data, WalkDirOffset, dir);
        W32(data, WalkDistanceOffset, distance);
        MapObject_StepDir(mapObject, dir);
        MapObject_TryFaceAndTurn(mapObject, dir);
        MapObject_SetDrawState(mapObject, (ushort)drawState);
        MapObject_SetStatusFlagOn(mapObject, StatusStartMovement);
        MapObject_AdvanceMovementStep(mapObject);
    }

    // OFFSET 0x020659C8
    private static int MovementAction_Walk_Step1(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementData(mapObject);
        MapObject_MovePosInDir(mapObject, S32(data, WalkDirOffset), S32(data, WalkDistanceOffset));
        MapObject_RecalculateObjectHeight(mapObject);
        W16(data, WalkDurationOffset, S16(data, WalkDurationOffset) - 1);
        if (S16(data, WalkDurationOffset) > 0)
        {
            return 0;
        }
        MapObject_SetStatusFlagOn(mapObject, StatusEndMovement | StatusMovementActionEnded);
        MapObject_UpdateCoords(mapObject);
        MapObject_CallDraw(mapObject);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x02065C64
    private static void MovementAction_InitWalkOnSpot(MapObject mapObject, int dir, int duration, int drawState)
    {
        byte[] data = MapObject_InitMovementData(mapObject, WalkDataSize);
        W16(data, WalkDrawStateOffset, drawState);
        W16(data, WalkDurationOffset, duration + 1);
        MapObject_TryFace(mapObject, dir);
        MapObject_SetDrawState(mapObject, drawState);
        MapObject_UpdateCoords(mapObject);
        MapObject_AdvanceMovementStep(mapObject);
    }

    // OFFSET 0x02065C98
    private static int MovementAction_WalkOnSpot_Step1(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementData(mapObject);
        W16(data, WalkDurationOffset, S16(data, WalkDurationOffset) - 1);
        if (S16(data, WalkDurationOffset) > 0)
        {
            return 0;
        }
        MapObject_SetStatusFlagOn(mapObject, StatusMovementActionEnded);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x02065E0C
    private static void MovementAction_InitJumpCustomSound(MapObject mapObject, int dir, int distance, int duration, int drawState, int heightTable, int speed, int seqID)
    {
        byte[] data = MapObject_InitMovementData(mapObject, JumpDataSize);
        W8(data, JumpDirOffset, dir);
        W32(data, JumpDistanceOffset, distance);
        W8(data, JumpDurationOffset, duration);
        W8(data, JumpDrawStateOffset, drawState);
        W8(data, JumpHeightTableOffset, heightTable);
        W16(data, JumpSpeedOffset, speed);
        if (distance == 0)
        {
            MapObject_UpdateCoords(mapObject);
        }
        else
        {
            MapObject_StepDir(mapObject, dir);
        }
        MapObject_SetStatusFlagOn(mapObject, StatusStartMovement | StatusStartJump);
        MapObject_TryFaceAndTurn(mapObject, dir);
        MapObject_SetDrawState(mapObject, (ushort)drawState);
        MapObject_AdvanceMovementStep(mapObject);
        if (seqID != 0)
        {
            SoundPlayback.Sound_PlayEffect((ushort)seqID);
        }
    }

    // OFFSET 0x02065E7C
    private static void MovementAction_InitJump(MapObject mapObject, int dir, int distance, int duration, int drawState, int heightTable, int speed)
    {
        MovementAction_InitJumpCustomSound(mapObject, dir, distance, duration, (ushort)drawState, (short)heightTable, (ushort)speed, SeqJump);
    }

    // OFFSET 0x02065EA0
    private static int MovementAction_Jump_Step1(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementData(mapObject);
        if (S32(data, JumpDistanceOffset) != 0)
        {
            MapObject_MovePosInDir(mapObject, S8(data, JumpDirOffset), S32(data, JumpDistanceOffset));
            MapObject_RecalculateObjectHeight(mapObject);
            if (S32(data, JumpTravelledOffset) >= TileSize)
            {
                W32(data, JumpTravelledOffset, 0);
                MapObject_StepDir(mapObject, S8(data, JumpDirOffset));
                MapObject_SetStatusFlagOn(mapObject, StatusStartMovement);
            }
            int distance = S32(data, JumpDistanceOffset);
            if (distance < 0)
            {
                distance = -distance;
            }
            W32(data, JumpTravelledOffset, S32(data, JumpTravelledOffset) + distance);
        }
        W16(data, JumpProgressOffset, U16(data, JumpProgressOffset) + U16(data, JumpSpeedOffset));
        if (U16(data, JumpProgressOffset) > JumpProgressMax)
        {
            W16(data, JumpProgressOffset, JumpProgressMax);
        }
        VecFx32 offset = default;
        offset.Y = JumpHeightTables()[S8(data, JumpHeightTableOffset)][U16(data, JumpProgressOffset) >> 8];
        MapObject_SetSpriteJumpOffset(mapObject, offset);
        W8(data, JumpDurationOffset, S8(data, JumpDurationOffset) - 1);
        if (S8(data, JumpDurationOffset) > 0)
        {
            return 0;
        }
        VecFx32 zero = default;
        MapObject_SetSpriteJumpOffset(mapObject, zero);
        MapObject_SetStatusFlagOn(mapObject, StatusEndMovement | StatusMovementActionEnded | StatusEndJump);
        MapObject_UpdateCoords(mapObject);
        MapObject_CallDraw(mapObject);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_AdvanceMovementStep(mapObject);
        SoundPlayback.Sound_PlayEffect(SeqLand);
        return 1;
    }

    // OFFSET 0x020662AC
    private static void MovementAction_InitDelay(MapObject mapObject, int delay)
    {
        byte[] data = MapObject_InitMovementData(mapObject, DelayDataSize);
        W32(data, 0, delay);
        MapObject_AdvanceMovementStep(mapObject);
    }

    // OFFSET 0x020662C4
    private static int MovementAction_Delay_Step1(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementData(mapObject);
        if (S32(data, 0) != 0)
        {
            W32(data, 0, S32(data, 0) - 1);
            return 0;
        }
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x02066338
    private static int MovementAction_WarpOut_Step0(MapObject mapObject)
    {
        byte[] data = MapObject_InitMovementData(mapObject, WarpDataSize);
        W32(data, WarpSpeedOffset, WarpOutSpeed);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x0206635C
    private static int MovementAction_WarpOut_Step1(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementData(mapObject);
        W32(data, WarpHeightOffset, S32(data, WarpHeightOffset) + S32(data, WarpSpeedOffset));
        VecFx32 offset = default;
        offset.Y = S32(data, WarpHeightOffset);
        MapObject_SetSpriteJumpOffset(mapObject, offset);
        if (S32(data, WarpHeightOffset) / WarpOutHeightUnit < WarpOutMaxHeight)
        {
            return 0;
        }
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x020663A4
    private static int MovementAction_WarpIn_Step0(MapObject mapObject)
    {
        byte[] data = MapObject_InitMovementData(mapObject, WarpDataSize);
        W32(data, WarpHeightOffset, WarpInStartHeight);
        W32(data, WarpSpeedOffset, WarpInSpeed);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x020663D0
    private static int MovementAction_WarpIn_Step1(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementData(mapObject);
        W32(data, WarpHeightOffset, S32(data, WarpHeightOffset) + S32(data, WarpSpeedOffset));
        if (S32(data, WarpHeightOffset) < 0)
        {
            W32(data, WarpHeightOffset, 0);
        }
        VecFx32 offset = default;
        offset.Y = S32(data, WarpHeightOffset);
        MapObject_SetSpriteJumpOffset(mapObject, offset);
        if (S32(data, WarpHeightOffset) > 0)
        {
            return 0;
        }
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x02066418
    private static int MovementAction_SetInvisible_Step0(MapObject mapObject)
    {
        MapObject_SetStatusFlagOn(mapObject, StatusHide);
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x02066430
    private static int MovementAction_SetVisible_Step0(MapObject mapObject)
    {
        MapObject_SetStatusFlagOff(mapObject, StatusHide);
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x02066448
    private static int MovementAction_LockDir_Step0(MapObject mapObject)
    {
        MapObject_SetStatusFlagOn(mapObject, StatusLockDir);
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x0206645C
    private static int MovementAction_UnlockDir_Step0(MapObject mapObject)
    {
        MapObject_SetStatusFlagOff(mapObject, StatusLockDir);
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x02066470
    private static int MovementAction_PauseAnimation_Step0(MapObject mapObject)
    {
        MapObject_SetStatusFlagOn(mapObject, StatusPauseAnimation);
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x02066488
    private static int MovementAction_ResumeAnimation_Step0(MapObject mapObject)
    {
        MapObject_SetStatusFlagOff(mapObject, StatusPauseAnimation);
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x020664A0
    private static void MovementAction_InitEmote(MapObject mapObject, int emote, int variant)
    {
        byte[] data = MapObject_InitMovementData(mapObject, EmoteDataSize);
        W32(data, EmoteTypeOffset, emote);
        mapObject.MovementEmoteAnimManager = EmoteIconEffect.EmoteIconEffect_New(mapObject, emote, 1, variant);
        MapObject_AdvanceMovementStep(mapObject);
    }

    // OFFSET 0x020664C8
    private static int MovementAction_Emote_Step1(MapObject mapObject)
    {
        if (EmoteIconEffect.EmoteIconEffect_IsFinished(mapObject.MovementEmoteAnimManager) == 1)
        {
            OverworldAnimManagers.OverworldAnimManager_Finish(mapObject.MovementEmoteAnimManager);
            MapObject_AdvanceMovementStep(mapObject);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02066520
    private static void MovementAction_InitWalkUneven(MapObject mapObject, int dir, int duration, int drawState)
    {
        byte[] data = MapObject_InitMovementData(mapObject, UnevenDataSize);
        W16(data, UnevenDirOffset, dir);
        W16(data, UnevenDrawStateOffset, drawState);
        W16(data, UnevenDurationOffset, duration);
        MapObject_StepDir(mapObject, dir);
        MapObject_TryFaceAndTurn(mapObject, dir);
        MapObject_SetDrawState(mapObject, drawState);
        MapObject_SetStatusFlagOn(mapObject, StatusStartMovement);
        MapObject_AdvanceMovementStep(mapObject);
    }

    // OFFSET 0x02066560
    private static int MovementAction_WalkUneven(MapObject mapObject, int[] stepSizes)
    {
        byte[] data = MapObject_GetMovementData(mapObject);
        MapObject_MovePosInDir(mapObject, S16(data, UnevenDirOffset), stepSizes[S16(data, UnevenStepOffset)]);
        MapObject_RecalculateObjectHeight(mapObject);
        W16(data, UnevenStepOffset, S16(data, UnevenStepOffset) + 1);
        if (S16(data, UnevenStepOffset) < S16(data, UnevenDurationOffset))
        {
            return 0;
        }
        MapObject_SetStatusFlagOn(mapObject, StatusEndMovement | StatusMovementActionEnded);
        MapObject_UpdateCoords(mapObject);
        MapObject_CallDraw(mapObject);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x02066600
    private static int MovementAction_WalkSlightlyFast_Step1(MapObject mapObject)
    {
        sWalkSlightlyFastSteps ??= RomInts(WalkSlightlyFastStepsAddress, WalkSlightlyFastStepCount);
        if (MovementAction_WalkUneven(mapObject, sWalkSlightlyFastSteps) == 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02066658
    private static int MovementAction_WalkSlightlyFaster_Step1(MapObject mapObject)
    {
        sWalkSlightlyFasterSteps ??= RomInts(WalkSlightlyFasterStepsAddress, WalkSlightlyFasterStepCount);
        if (MovementAction_WalkUneven(mapObject, sWalkSlightlyFasterSteps) == 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020666B0
    private static int MovementAction_WalkEverSoSlightlyFast_Step1(MapObject mapObject)
    {
        sWalkEverSoSlightlyFastSteps ??= RomInts(WalkEverSoSlightlyFastStepsAddress, WalkEverSoSlightlyFastStepCount);
        if (MovementAction_WalkUneven(mapObject, sWalkEverSoSlightlyFastSteps) == 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020666C8
    private static int MovementAction_PokecenterNurseBow_Step0(MapObject mapObject)
    {
        MapObject_InitMovementData(mapObject, CounterDataSize);
        MapObject_SetDrawState(mapObject, NurseBowDrawState);
        MapObject_AdvanceMovementStep(mapObject);
        return 0;
    }

    // OFFSET 0x020666E4
    private static int MovementAction_PokecenterNurseBow_Step1(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementData(mapObject);
        W32(data, 0, S32(data, 0) + 1);
        if (S32(data, 0) >= NurseBowFrames)
        {
            MapObject_TryFace(mapObject, DirSouth);
            MapObject_SetDrawState(mapObject, 0);
            MapObject_AdvanceMovementStep(mapObject);
        }
        return 0;
    }

    // OFFSET 0x02066710
    private static int MovementAction_RevealTrainer_Step0(MapObject mapObject)
    {
        MapObject_InitMovementData(mapObject, CounterDataSize);
        OverworldAnimManager? hidingAnimManager = sub_0206A224(mapObject);
        if (hidingAnimManager != null)
        {
            OverworldAnimManagers.OverworldAnimManager_Finish(hidingAnimManager);
        }
        VecFx32 zero = default;
        MapObject_SetSpriteJumpOffset(mapObject, zero);
        TilePuffEffect.TilePuffEffect_New(mapObject);
        MapObject_SetStatusFlagOn(mapObject, StatusStartMovement | StatusStartJump);
        MapObject_SetStatusFlagOff(mapObject, StatusHideShadow);
        MapObject_AdvanceMovementStep(mapObject);
        return 0;
    }

    // OFFSET 0x02066764
    private static int MovementAction_RevealTrainer_Step1(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementData(mapObject);
        VecFx32 offset = default;
        offset.Y = JumpHeightTables()[0][S32(data, 0)];
        MapObject_SetSpriteJumpOffset(mapObject, offset);
        W32(data, 0, S32(data, 0) + 2);
        if ((uint)S32(data, 0) < RevealTrainerFrames)
        {
            return 0;
        }
        offset.Y = 0;
        MapObject_SetSpriteJumpOffset(mapObject, offset);
        MapObject_SetStatusFlagOn(mapObject, StatusEndMovement | StatusMovementActionEnded | StatusEndJump);
        sub_0206A230(mapObject);
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x020667CC
    private static int MovementAction_PlayerGive_Step0(MapObject mapObject)
    {
        MapObject_InitMovementData(mapObject, CounterDataSize);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_AdvanceMovementStep(mapObject);
        return 0;
    }

    // OFFSET 0x020667E8
    private static int MovementAction_PlayerReceive_Step0(MapObject mapObject)
    {
        MapObject_InitMovementData(mapObject, CounterDataSize);
        MapObject_SetDrawState(mapObject, 1);
        MapObject_AdvanceMovementStep(mapObject);
        return 0;
    }

    // OFFSET 0x02066804
    private static int MovementAction_PlayerGiveReceive_Step1(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementData(mapObject);
        W32(data, 0, S32(data, 0) + 1);
        if ((uint)S32(data, 0) < PlayerGiveReceiveFrames)
        {
            return 0;
        }
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x02066824
    private static void MovementAction_InitVectorWalk(MapObject mapObject, in VecFx32 step, int facingDir, int movingDir, int duration, int drawState)
    {
        byte[] data = MapObject_InitMovementData(mapObject, VectorWalkDataSize);
        W32(data, VectorWalkDurationOffset, duration);
        W32(data, VectorWalkStepOffset, step.X);
        W32(data, VectorWalkStepOffset + 4, step.Y);
        W32(data, VectorWalkStepOffset + 8, step.Z);
        MapObject_TryFace(mapObject, facingDir);
        MapObject_Turn(mapObject, movingDir);
        MapObject_SetDrawState(mapObject, (byte)drawState);
        MapObject_SetStartMovement(mapObject);
        MapObject_SetXPrev(mapObject, MapObject_GetX(mapObject));
        MapObject_SetYPrev(mapObject, MapObject_GetY(mapObject));
        MapObject_SetZPrev(mapObject, MapObject_GetZ(mapObject));
        if (step.X < 0)
        {
            MapObject_AddX(mapObject, -1);
        }
        else if (step.X > 0)
        {
            MapObject_AddX(mapObject, 1);
        }
        if (step.Y < 0)
        {
            MapObject_AddY(mapObject, -2);
        }
        else if (step.Y > 0)
        {
            MapObject_AddY(mapObject, 2);
        }
        if (step.Z < 0)
        {
            MapObject_AddZ(mapObject, -1);
        }
        else if (step.Z > 0)
        {
            MapObject_AddZ(mapObject, 1);
        }
        MapObject_AdvanceMovementStep(mapObject);
    }

    // OFFSET 0x020668EC
    private static int MovementAction_VectorWalk_Step1(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementData(mapObject);
        VecFx32 step;
        step.X = S32(data, VectorWalkStepOffset);
        step.Y = S32(data, VectorWalkStepOffset + 4);
        step.Z = S32(data, VectorWalkStepOffset + 8);
        MapObject_AddVecToPos(mapObject, step);
        W32(data, VectorWalkDurationOffset, S32(data, VectorWalkDurationOffset) - 1);
        if (S32(data, VectorWalkDurationOffset) > 0)
        {
            return 0;
        }
        MapObject_SetStatusFlagOn(mapObject, StatusEndMovement | StatusMovementActionEnded);
        MapObject_UpdateCoords(mapObject);
        MapObject_CallDraw(mapObject);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_AdvanceMovementStep(mapObject);
        return 1;
    }

    // OFFSET 0x02066F88
    private static void MovementAction_InitAxisJump(MapObject mapObject, int distance, int facingDir, int movingDir, int duration, int drawState, int moveAxis, int heightAxis, int invertHeight)
    {
        int sign = 1;
        byte[] data = MapObject_InitMovementData(mapObject, AxisJumpDataSize);
        W8(data, AxisJumpDurationOffset, duration);
        W32(data, AxisJumpDistanceOffset, distance);
        W8(data, AxisJumpMoveAxisOffset, moveAxis);
        W8(data, AxisJumpHeightAxisOffset, heightAxis);
        W8(data, AxisJumpInvertOffset, invertHeight);
        W16(data, AxisJumpSpeedOffset, 0x1000 / U8(data, AxisJumpDurationOffset));
        MapObject_TryFace(mapObject, facingDir);
        MapObject_Turn(mapObject, movingDir);
        MapObject_SetDrawState(mapObject, (byte)drawState);
        MapObject_SetStartMovement(mapObject);
        MapObject_SetXPrev(mapObject, MapObject_GetX(mapObject));
        MapObject_SetYPrev(mapObject, MapObject_GetY(mapObject));
        MapObject_SetZPrev(mapObject, MapObject_GetZ(mapObject));
        if ((byte)moveAxis > AxisZ)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (distance != 0)
        {
            switch ((byte)moveAxis)
            {
                case AxisX:
                    if (distance < 0)
                    {
                        sign = -1;
                    }
                    MapObject_AddX(mapObject, sign);
                    break;
                case AxisY:
                    if (distance < 0)
                    {
                        sign = -1;
                    }
                    MapObject_AddY(mapObject, sign << 1);
                    break;
                case AxisZ:
                    if (distance < 0)
                    {
                        sign = -1;
                    }
                    MapObject_AddZ(mapObject, sign);
                    break;
            }
        }
        MapObject_AdvanceMovementStep(mapObject);
    }

    // OFFSET 0x02067068
    private static int MovementAction_AxisJump_Step1(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementData(mapObject);
        MapObject_CopyPos(mapObject, out VecFx32 position);
        switch (U8(data, AxisJumpMoveAxisOffset))
        {
            case AxisX:
                position.X += S32(data, AxisJumpDistanceOffset);
                break;
            case AxisY:
                position.Y += S32(data, AxisJumpDistanceOffset);
                break;
            case AxisZ:
                position.Z += S32(data, AxisJumpDistanceOffset);
                break;
        }
        MapObject_SetPos(mapObject, position);
        int distance = S32(data, AxisJumpDistanceOffset);
        if (distance < 0)
        {
            distance = -distance;
        }
        W32(data, AxisJumpTravelledOffset, S32(data, AxisJumpTravelledOffset) + distance);
        W16(data, AxisJumpProgressOffset, U16(data, AxisJumpProgressOffset) + U16(data, AxisJumpSpeedOffset));
        if (U16(data, AxisJumpProgressOffset) > JumpProgressMax)
        {
            W16(data, AxisJumpProgressOffset, JumpProgressMax);
        }
        VecFx32 offset = default;
        int height = JumpHeightTables()[1][U16(data, AxisJumpProgressOffset) >> 8];
        if (U8(data, AxisJumpInvertOffset) == 1)
        {
            height = -height;
        }
        switch (U8(data, AxisJumpHeightAxisOffset))
        {
            case AxisX:
                offset.X = height;
                break;
            case AxisY:
                offset.Y = height;
                break;
            case AxisZ:
                offset.Z = height;
                break;
        }
        MapObject_SetSpriteJumpOffset(mapObject, offset);
        W8(data, AxisJumpDurationOffset, U8(data, AxisJumpDurationOffset) - 1);
        if (S32(data, AxisJumpTravelledOffset) >= TileSize && U8(data, AxisJumpDurationOffset) != 0)
        {
            W32(data, AxisJumpTravelledOffset, S32(data, AxisJumpTravelledOffset) - TileSize);
            int sign = 1;
            int step = S32(data, AxisJumpDistanceOffset);
            MapObject_SetXPrev(mapObject, MapObject_GetX(mapObject));
            MapObject_SetYPrev(mapObject, MapObject_GetY(mapObject));
            MapObject_SetZPrev(mapObject, MapObject_GetZ(mapObject));
            switch (U8(data, AxisJumpMoveAxisOffset))
            {
                case AxisX:
                    if (step < 0)
                    {
                        sign -= 2;
                    }
                    MapObject_AddX(mapObject, sign);
                    break;
                case AxisY:
                    if (step < 0)
                    {
                        sign -= 2;
                    }
                    MapObject_AddY(mapObject, sign << 1);
                    break;
                case AxisZ:
                    if (step < 0)
                    {
                        sign -= 2;
                    }
                    MapObject_AddZ(mapObject, sign);
                    break;
            }
        }
        if (U8(data, AxisJumpDurationOffset) != 0)
        {
            return 0;
        }
        VecFx32 zero = default;
        MapObject_SetSpriteJumpOffset(mapObject, zero);
        MapObject_SetStatusFlagOn(mapObject, StatusEndMovement | StatusMovementActionEnded | StatusEndJump);
        MapObject_UpdateCoords(mapObject);
        MapObject_CallDraw(mapObject);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_AdvanceMovementStep(mapObject);
        SoundPlayback.Sound_PlayEffect(SeqLand);
        return 1;
    }

    private static MovementActionStepFunc[] ActionSteps(int movementAction)
    {
        sActionSteps ??= new MovementActionStepFunc[]?[MovementActionCount];
        MovementActionStepFunc[]? steps = sActionSteps[movementAction];
        if (steps == null)
        {
            List<MovementActionStepFunc> list = new();
            uint address = RomData.Arm9U32(ActionStepsTableAddress + (uint)(movementAction * 4), 1)[0];
            while (true)
            {
                uint step = RomData.Arm9U32(address, 1)[0];
                MovementActionStepFunc? func = ResolveStep(step);
                if (func == null)
                {
                    throw Missing(step & ~1u);
                }
                list.Add(func);
                if ((step & ~1u) == EndStepAddress)
                {
                    break;
                }
                address += 4;
            }
            steps = list.ToArray();
            sActionSteps[movementAction] = steps;
        }
        return steps;
    }

    private static int[][] ActionDirTables()
    {
        if (sActionDirTables == null)
        {
            List<int[]> tables = new();
            uint address = ActionDirTablesAddress;
            while (true)
            {
                uint table = RomData.Arm9U32(address, 1)[0];
                if (table == 0)
                {
                    break;
                }
                tables.Add(RomInts(table, DirCount));
                address += 4;
            }
            sActionDirTables = tables.ToArray();
        }
        return sActionDirTables;
    }

    private static int[][] JumpHeightTables()
    {
        if (sJumpHeightTables == null)
        {
            uint[] addresses = RomData.Arm9U32(JumpHeightTablesAddress, JumpHeightTableCount);
            int[][] tables = new int[JumpHeightTableCount][];
            for (int i = 0; i < JumpHeightTableCount; i++)
            {
                tables[i] = RomInts(addresses[i], JumpHeightTableSize);
            }
            sJumpHeightTables = tables;
        }
        return sJumpHeightTables;
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

    private static VecFx32 RomVec(uint address)
    {
        if (!sRomVecs.TryGetValue(address, out VecFx32 vec))
        {
            uint[] words = RomData.Arm9U32(address, 3);
            vec.X = (int)words[0];
            vec.Y = (int)words[1];
            vec.Z = (int)words[2];
            sRomVecs[address] = vec;
        }
        return vec;
    }
}
