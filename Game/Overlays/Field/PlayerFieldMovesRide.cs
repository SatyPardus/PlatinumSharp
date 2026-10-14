using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class PlayerFieldMoves
{
    private const int SubTaskContinue = 0;
    private const int SubTaskFinished = 1;
    private const int SubTaskRepeat = 2;
    private const int SoundEffectWaterfall = 0x64D;
    private const int SoundEffectWarp = 0x64F;
    private const int WaterfallTiles = 2;
    private const int WaterfallFirstPhaseFrames = 0x20;
    private const int WaterfallFrames = 0x40;
    private const int RockClimbStateMove = 5;
    private const uint HealAnimationSpinAddress = 0x021F9B9C;
    private const uint HealAnimationFinishAddress = 0x021F9C00;
    private const int AnimationCommandBytes = 0x100;
    private const uint RequestStateVsSeeker = 0x80;
    private const uint RequestStateSprite17 = 0x200;
    private const uint VsSeekerTaskPriority = 0xFFFF;
    private const uint WarpTaskPriority = 100;
    private const int WarpFrames = 0x14;
    private const double WarpBaseSpeed = 9011.2;
    private const int FadeDuration = 6;

    private delegate int RockClimbSubTask(RockClimbTask env);

    private delegate int WaterfallSubTask(WaterfallTask env);

    private sealed class RockClimbTask
    {
        public int State;
        public int Direction;
        public int Counter;
        public FieldSystem FieldSystem = null!;
        public PlayerAvatar PlayerAvatar = null!;
        public MapObject MapObject = null!;
        public OverworldAnimManager? Mount;
        public readonly MonRideTask MonRide = new();
        public object? Effect;
    }

    private sealed class WaterfallTask
    {
        public int State;
        public int Direction;
        public int Counter;
        public int DestX;
        public int DestY;
        public int DestZ;
        public int Travelled;
        public VecFx32 Step;
        public VecFx32 Dest;
        public FieldSystem FieldSystem = null!;
        public PlayerAvatar PlayerAvatar = null!;
        public MapObject MapObject = null!;
        public readonly MonRideTask MonRide = new();
        public object? Effect;
    }

    private sealed class HealAnimationTask
    {
        public FieldSystem FieldSystem = null!;
        public PlayerAvatar PlayerAvatar = null!;
        public MapObject MapObject = null!;
        public int State;
        public int Gender;
        public SysTask? AnimationTask;
    }

    private sealed class VsSeekerTask
    {
        public int State;
        public int PlayerState;
        public FieldSystem FieldSystem = null!;
        public PlayerAvatar PlayerAvatar = null!;
    }

    private sealed class WarpAnimationTask
    {
        public FieldSystem FieldSystem = null!;
        public Action<int> SetFinished = null!;
        public int State;
        public int Counter;
        public int Direction;
    }

    private static readonly RockClimbSubTask[] sRockClimbSubTasks =
    {
        SubTask_RockClimb_PlayCutIn,
        SubTask_RockClimb_WaitCutIn,
        SubTask_RockClimb_CreateMount,
        SubTask_RockClimb_HopOn,
        SubTask_RockClimb_WaitHop,
        SubTask_RockClimb_Move,
        SubTask_RockClimb_LoopOrHopOff,
        SubTask_RockClimb_WaitFinished,
    };

    private static readonly WaterfallSubTask[] sWaterfallAscentSubTasks =
    {
        SubTask_Waterfall_PlayAscentCutIn,
        SubTask_Waterfall_WaitForAscentCutIn,
        SubTask_Waterfall_InitAscent,
        SubTask_Waterfall_Ascend,
        SubTask_Waterfall_FinishAscent,
    };

    private static readonly WaterfallSubTask[] sWaterfallDescentSubTasks =
    {
        SubTask_Waterfall_PlayDescentCutIn,
        SubTask_Waterfall_WaitForDescentCutIn,
        SubTask_Waterfall_InitDescent,
        SubTask_Waterfall_Descend,
        SubTask_Waterfall_FinishDescent,
    };

    // OFFSET 0x021E06F8
    private static void RockClimbTask_Start(FieldSystem fieldSystem, int direction, MonRideTask monRide)
    {
        RockClimbTask env = MonRideTaskEnv_New<RockClimbTask>();
        env.Direction = direction;
        env.FieldSystem = fieldSystem;
        env.PlayerAvatar = fieldSystem.PlayerAvatar!;
        env.MapObject = PlayerAvatar.PlayerAvatar_GetMapObject(env.PlayerAvatar);
        env.MonRide.CopyFrom(monRide);
        FieldTask.FieldTask_InitCall(fieldSystem.Task!, FieldTask_UseRockClimb, env);
    }

    // OFFSET 0x021E0734
    public static void FieldTask_StartUseRockClimb(FieldTask task, int direction, int partySlot)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        MonRideTask monRide = new();
        MonRideTask_Init(fieldSystem, GetPokemonByIndex(fieldSystem, partySlot), monRide);
        RockClimbTask_Start(fieldSystem, direction, monRide);
    }

    // OFFSET 0x021E0760
    public static int PlayerAvatar_CanUseRockClimb(int tileBehavior, int direction)
    {
        switch (direction)
        {
            case DirectionNorth:
            case DirectionSouth:
                if (TileBehavior.TileBehavior_IsRockClimbNorthSouth((byte)tileBehavior) == 1)
                {
                    return 1;
                }
                break;
            case DirectionWest:
            case DirectionEast:
                if (TileBehavior.TileBehavior_IsRockClimbEastWest((byte)tileBehavior) == 1)
                {
                    return 1;
                }
                break;
        }
        return 0;
    }

    // OFFSET 0x021E07A0
    private static bool FieldTask_UseRockClimb(FieldTask task)
    {
        RockClimbTask env = (RockClimbTask)FieldTask.FieldTask_GetEnv(task)!;
        int result;
        do
        {
            result = sRockClimbSubTasks[env.State](env);
        }
        while (result == SubTaskRepeat);
        if (result == SubTaskFinished)
        {
            MonRideTaskEnv_Free(env);
        }
        return result != 0;
    }

    // OFFSET 0x021E07CC
    private static int SubTask_RockClimb_PlayCutIn(RockClimbTask env)
    {
        NewMonRideCutIn(env.FieldSystem, env.MonRide);
        env.State++;
        return SubTaskContinue;
    }

    // OFFSET 0x021E07E4
    private static int SubTask_RockClimb_WaitCutIn(RockClimbTask env)
    {
        if (CheckMonRideCutInFinished(env.MonRide) == true)
        {
            env.State++;
        }
        return SubTaskContinue;
    }

    // OFFSET 0x021E07FC
    private static int SubTask_RockClimb_CreateMount(RockClimbTask env)
    {
        int x = PlayerAvatar.PlayerAvatar_GetXPos(env.PlayerAvatar) + MapObjectMove.MapObject_GetDxFromDir(env.Direction);
        int z = PlayerAvatar.PlayerAvatar_GetZPos(env.PlayerAvatar) + MapObjectMove.MapObject_GetDzFromDir(env.Direction);
        env.Mount = DirectionalMountEffect.DirectionalMountEffect_New(env.MapObject, x, z, env.Direction, 0);
        env.Effect = ov6_0224892C(env.FieldSystem);
        SoundPlayback.Sound_PlayEffect(SoundEffectStrength);
        env.State++;
        return SubTaskContinue;
    }

    // OFFSET 0x021E0850
    private static int SubTask_RockClimb_HopOn(RockClimbTask env)
    {
        if (MovementActions.LocalMapObj_IsAnimationSet(env.MapObject) == 1)
        {
            MovementActions.LocalMapObj_SetAnimationCode(env.MapObject, MovementActions.MovementAction_TurnActionTowardsDir(env.Direction, MovementJump));
            env.State++;
        }
        return SubTaskContinue;
    }

    // OFFSET 0x021E0878
    private static int SubTask_RockClimb_WaitHop(RockClimbTask env)
    {
        if (MovementActions.LocalMapObj_CheckAnimationFinished(env.MapObject) == 1)
        {
            env.State++;
            DirectionalMountEffect.DirectionalMountEffect_SetSyncPos(env.Mount!, 1);
        }
        return SubTaskContinue;
    }

    // OFFSET 0x021E0898
    private static int SubTask_RockClimb_Move(RockClimbTask env)
    {
        if (MovementActions.LocalMapObj_IsAnimationSet(env.MapObject) == 1)
        {
            MovementActions.LocalMapObj_SetAnimationCode(env.MapObject, MovementActions.MovementAction_TurnActionTowardsDir(env.Direction, 0x10));
            env.State++;
        }
        return SubTaskContinue;
    }

    // OFFSET 0x021E08C0
    private static int SubTask_RockClimb_LoopOrHopOff(RockClimbTask env)
    {
        if (MovementActions.LocalMapObj_CheckAnimationFinished(env.MapObject) == 0)
        {
            return SubTaskContinue;
        }
        int direction = MapObject.MapObject_GetMovingDir(env.MapObject);
        byte behavior = (byte)MapObjectMove.MapObject_GetTileBehaviorFromDir(env.MapObject, direction);
        if (PlayerAvatar_CanUseRockClimb(behavior, direction) == 1)
        {
            env.State = RockClimbStateMove;
            return SubTaskRepeat;
        }
        MovementActions.LocalMapObj_SetAnimationCode(env.MapObject, MovementActions.MovementAction_TurnActionTowardsDir(env.Direction, MovementJump));
        env.Counter = 0;
        env.State++;
        DirectionalMountEffect.DirectionalMountEffect_SetSyncPos(env.Mount!, 0);
        ov6_02248940(env.Effect);
        env.Effect = null;
        return SubTaskContinue;
    }

    // OFFSET 0x021E0924
    private static int SubTask_RockClimb_WaitFinished(RockClimbTask env)
    {
        if (MovementActions.LocalMapObj_CheckAnimationFinished(env.MapObject) == 0)
        {
            return SubTaskContinue;
        }
        MovementActions.LocalMapObj_ClearFinishedAnimation(env.MapObject);
        FieldEffects.FieldEffectManager_FinishAnimManager(env.Mount);
        return SubTaskFinished;
    }

    // OFFSET 0x021E0948
    private static WaterfallTask WaterfallTaskEnv_New(FieldSystem fieldSystem, int direction, MonRideTask? monRide)
    {
        WaterfallTask env = MonRideTaskEnv_New<WaterfallTask>();
        env.Direction = direction;
        env.FieldSystem = fieldSystem;
        env.PlayerAvatar = fieldSystem.PlayerAvatar!;
        env.MapObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
        if (monRide != null)
        {
            env.MonRide.CopyFrom(monRide);
        }
        return env;
    }

    // OFFSET 0x021E097C
    private static void FieldSystem_CreateTaskUseWaterfall(FieldSystem fieldSystem, int direction)
    {
        WaterfallTask env = WaterfallTaskEnv_New(fieldSystem, direction, null);
        FieldTask.FieldSystem_CreateTask(fieldSystem, FieldTask_UseWaterfall, env);
    }

    // OFFSET 0x021E0998
    public static void FieldTask_StartUseWaterfall(FieldTask task, int direction, int partySlot)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        MonRideTask monRide = new();
        MonRideTask_Init(fieldSystem, GetPokemonByIndex(fieldSystem, partySlot), monRide);
        WaterfallTask env = WaterfallTaskEnv_New(fieldSystem, direction, monRide);
        FieldTask.FieldTask_InitCall(task, FieldTask_UseWaterfall, env);
    }

    // OFFSET 0x021E09D4
    private static bool FieldTask_UseWaterfall(FieldTask task)
    {
        WaterfallTask env = (WaterfallTask)FieldTask.FieldTask_GetEnv(task)!;
        int result;
        do
        {
            if (env.Direction == DirectionNorth)
            {
                result = sWaterfallAscentSubTasks[env.State](env);
            }
            else
            {
                result = sWaterfallDescentSubTasks[env.State](env);
            }
        }
        while (result == SubTaskRepeat);
        if (result == SubTaskFinished)
        {
            MonRideTaskEnv_Free(env);
            return true;
        }
        return false;
    }

    // OFFSET 0x021E0A1C
    private static int SubTask_Waterfall_PlayAscentCutIn(WaterfallTask env)
    {
        if (env.MonRide.Active == 1)
        {
            NewMonRideCutIn(env.FieldSystem, env.MonRide);
            env.State++;
            return SubTaskContinue;
        }
        env.State = 2;
        return SubTaskFinished;
    }

    // OFFSET 0x021E0A44
    private static int SubTask_Waterfall_WaitForAscentCutIn(WaterfallTask env)
    {
        if (CheckMonRideCutInFinished(env.MonRide) == true)
        {
            SoundPlayback.Sound_PlayEffect(SoundEffectWaterfall);
            env.State++;
        }
        return SubTaskContinue;
    }

    // OFFSET 0x021E0A68
    private static int SubTask_Waterfall_InitAscent(WaterfallTask env)
    {
        int x = MapObject.MapObject_GetX(env.MapObject) + MapObjectMove.MapObject_GetDxFromDir(DirectionNorth) * WaterfallTiles;
        int z = MapObject.MapObject_GetZ(env.MapObject) + MapObjectMove.MapObject_GetDzFromDir(DirectionNorth) * WaterfallTiles;
        MapObjectMove.VecFx32_SetPosFromMapCoords(x, z, ref env.Dest);
        MapObjectMove.MapObject_RecalculatePositionHeight(env.FieldSystem, ref env.Dest);
        env.DestX = x;
        env.DestY = (env.Dest.Y >> 3) / 0x1000;
        env.DestZ = z;
        MapObject.MapObject_CopyPos(env.MapObject, out VecFx32 position);
        if (position.Z <= env.Dest.Z)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (position.Y >= env.Dest.Y)
        {
            ErrorHandling.GF_AssertFail();
        }
        env.Step.X = 0;
        env.Step.Y = (env.Dest.Y - position.Y) / WaterfallFrames;
        env.Step.Z = (env.Dest.Z - position.Z) / WaterfallFrames;
        env.Effect = ov6_022485F4(env.FieldSystem);
        env.State++;
        return SubTaskContinue;
    }

    // OFFSET 0x021E0B24
    private static int SubTask_Waterfall_Ascend(WaterfallTask env)
    {
        MapObject.MapObject_CopyPos(env.MapObject, out VecFx32 position);
        position.Y += env.Step.Y;
        if (position.Y > env.Dest.Y)
        {
            position.Y = env.Dest.Y;
        }
        MapObject.MapObject_SetPos(env.MapObject, position);
        env.Counter++;
        if (env.Counter >= WaterfallFirstPhaseFrames)
        {
            env.Counter = 0;
            env.State++;
        }
        return SubTaskContinue;
    }

    // OFFSET 0x021E0B64
    private static int SubTask_Waterfall_FinishAscent(WaterfallTask env)
    {
        MapObject.MapObject_CopyPos(env.MapObject, out VecFx32 position);
        position.Y += env.Step.Y;
        if (position.Y > env.Dest.Y)
        {
            position.Y = env.Dest.Y;
        }
        position.Z += env.Step.Z;
        if (position.Z < env.Dest.Z)
        {
            position.Z = env.Dest.Z;
        }
        MapObject.MapObject_SetPos(env.MapObject, position);
        env.Counter++;
        if (env.Counter < WaterfallFrames)
        {
            return SubTaskContinue;
        }
        if (position.Z != env.Dest.Z)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (position.Y != env.Dest.Y)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObject.MapObject_SetX(env.MapObject, env.DestX);
        MapObject.MapObject_SetY(env.MapObject, env.DestY);
        MapObject.MapObject_SetZ(env.MapObject, env.DestZ);
        MapObjectMove.MapObject_UpdateCoords(env.MapObject);
        ov6_02248608(env.Effect);
        return SubTaskFinished;
    }

    // OFFSET 0x021E0BEC
    private static int SubTask_Waterfall_PlayDescentCutIn(WaterfallTask env)
    {
        if (env.MonRide.Active == 1)
        {
            NewMonRideCutIn(env.FieldSystem, env.MonRide);
            env.State++;
            return SubTaskContinue;
        }
        env.State = 2;
        return SubTaskRepeat;
    }

    // OFFSET 0x021E0C10
    private static int SubTask_Waterfall_WaitForDescentCutIn(WaterfallTask env)
    {
        if (CheckMonRideCutInFinished(env.MonRide) == true)
        {
            SoundPlayback.Sound_PlayEffect(SoundEffectWaterfall);
            env.State++;
        }
        return SubTaskContinue;
    }

    // OFFSET 0x021E0C34
    private static int SubTask_Waterfall_InitDescent(WaterfallTask env)
    {
        int x = MapObject.MapObject_GetX(env.MapObject) + MapObjectMove.MapObject_GetDxFromDir(DirectionSouth) * WaterfallTiles;
        int z = MapObject.MapObject_GetZ(env.MapObject) + MapObjectMove.MapObject_GetDzFromDir(DirectionSouth) * WaterfallTiles;
        MapObjectMove.VecFx32_SetPosFromMapCoords(x, z, ref env.Dest);
        MapObjectMove.MapObject_RecalculatePositionHeight(env.FieldSystem, ref env.Dest);
        env.Travelled = 0;
        env.DestX = x;
        env.DestY = (env.Dest.Y >> 3) / 0x1000;
        env.DestZ = z;
        MapObject.MapObject_CopyPos(env.MapObject, out VecFx32 position);
        if (position.Z >= env.Dest.Z)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (position.Y <= env.Dest.Y)
        {
            ErrorHandling.GF_AssertFail();
        }
        env.Step.X = 0;
        env.Step.Y = (env.Dest.Y - position.Y) / WaterfallFrames;
        env.Step.Z = (env.Dest.Z - position.Z) / WaterfallFrames;
        env.Effect = ov6_022485F4(env.FieldSystem);
        env.State++;
        return SubTaskContinue;
    }

    // OFFSET 0x021E0CF4
    private static int SubTask_Waterfall_Descend(WaterfallTask env)
    {
        MapObject.MapObject_CopyPos(env.MapObject, out VecFx32 position);
        position.Z += env.Step.Z;
        if (position.Z > env.Dest.Z)
        {
            position.Z = env.Dest.Z;
        }
        else
        {
            env.Travelled += env.Step.Z;
        }
        MapObject.MapObject_SetPos(env.MapObject, position);
        env.Counter++;
        if (env.Counter >= WaterfallFirstPhaseFrames)
        {
            env.Counter = 0;
            env.State++;
        }
        return SubTaskContinue;
    }

    // OFFSET 0x021E0D40
    private static int SubTask_Waterfall_FinishDescent(WaterfallTask env)
    {
        MapObject.MapObject_CopyPos(env.MapObject, out VecFx32 position);
        position.Y += env.Step.Y;
        if (position.Y < env.Dest.Y)
        {
            position.Y = env.Dest.Y;
        }
        position.Z += env.Step.Z;
        if (position.Z > env.Dest.Z)
        {
            position.Z = env.Dest.Z;
        }
        else
        {
            env.Travelled += env.Step.Z;
        }
        MapObject.MapObject_SetPos(env.MapObject, position);
        env.Counter++;
        if (env.Counter < WaterfallFrames)
        {
            return SubTaskContinue;
        }
        if (position.Z != env.Dest.Z)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (position.Y != env.Dest.Y)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObject.MapObject_SetX(env.MapObject, env.DestX);
        MapObject.MapObject_SetY(env.MapObject, env.DestY);
        MapObject.MapObject_SetZ(env.MapObject, env.DestZ);
        MapObjectMove.MapObject_UpdateCoords(env.MapObject);
        ov6_02248608(env.Effect);
        return SubTaskFinished;
    }

    // OFFSET 0x021E0DD4
    public static void FieldTask_StartHealAnimation(FieldTask task)
    {
        FieldSystem_StartHealAnimation(FieldTask.FieldTask_GetFieldSystem(task));
    }

    // OFFSET 0x021E0DE0
    private static void FieldSystem_StartHealAnimation(FieldSystem fieldSystem)
    {
        HealAnimationTask env = MonRideTaskEnv_New<HealAnimationTask>();
        env.FieldSystem = fieldSystem;
        env.PlayerAvatar = fieldSystem.PlayerAvatar!;
        env.MapObject = PlayerAvatar.PlayerAvatar_GetMapObject(env.PlayerAvatar);
        env.Gender = PlayerAvatar.PlayerAvatar_GetGender(env.PlayerAvatar);
        FieldTask.FieldTask_InitCall(fieldSystem.Task!, FieldTask_HealAnimation, env);
    }

    // OFFSET 0x021E0E10
    private static bool FieldTask_HealAnimation(FieldTask task)
    {
        HealAnimationTask env = (HealAnimationTask)FieldTask.FieldTask_GetEnv(task)!;
        switch (env.State)
        {
            case 0:
                env.AnimationTask = MovementActions.MapObject_StartAnimation(env.MapObject, new BufferView(RomData.Overlay(FieldOverlayID, HealAnimationSpinAddress, AnimationCommandBytes), 0));
                env.State++;
                break;
            case 1:
                if (MovementActions.MapObject_HasAnimationEnded(env.AnimationTask!) == 1)
                {
                    PlayerAvatarRequestState.PlayerAvatar_SetGraphics(env.PlayerAvatar, PlayerAvatar.Player_GetSpriteFromStateAndGender(SpriteStateHeal, env.Gender));
                    MovementActions.MapObject_FinishAnimation(env.AnimationTask!);
                    env.AnimationTask = MovementActions.MapObject_StartAnimation(env.MapObject, new BufferView(RomData.Overlay(FieldOverlayID, HealAnimationFinishAddress, AnimationCommandBytes), 0));
                    env.State++;
                }
                break;
            case 2:
                if (MovementActions.MapObject_HasAnimationEnded(env.AnimationTask!) == 1)
                {
                    MovementActions.MapObject_FinishAnimation(env.AnimationTask!);
                    MonRideTaskEnv_Free(env);
                    return true;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x021E0F54
    private static SysTask? FieldSystem_StartRequestedStateTask(FieldSystem fieldSystem, uint requestState)
    {
        PlayerAvatar playerAvatar = fieldSystem.PlayerAvatar!;
        int playerState = PlayerAvatar.PlayerAvatar_GetPlayerState(playerAvatar);
        if (playerState != PlayerStateWalking)
        {
            return null;
        }
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar);
        VsSeekerTask env = new();
        env.State = 0;
        env.FieldSystem = fieldSystem;
        env.PlayerAvatar = playerAvatar;
        env.PlayerState = playerState;
        MapObject.MapObject_SetPauseMovementOff(mapObject);
        PlayerAvatarRequestState.PlayerAvatar_RequestState(playerAvatar, requestState);
        PlayerAvatarRequestState.PlayerAvatar_ProcessRequestedStates(playerAvatar);
        SysTask? task = SysTask.SysTask_Start(SysTask_DrawPlayer, env, VsSeekerTaskPriority);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return task;
    }

    // OFFSET 0x021E0FC0
    private static void FieldSystem_EndRequestedStateTask(SysTask? task)
    {
        if (task == null)
        {
            return;
        }
        VsSeekerTask env = (VsSeekerTask)SysTaskManager.SysTask_GetParam(task)!;
        PlayerAvatar playerAvatar = env.PlayerAvatar;
        PlayerAvatar.Player_GetSpriteFromStateAndGender(PlayerStateWalking, PlayerAvatar.PlayerAvatar_GetGender(playerAvatar));
        PlayerAvatarRequestState.RequestState_Walking(playerAvatar);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x021E0FF0
    private static void SysTask_DrawPlayer(SysTask task, object? param)
    {
        VsSeekerTask env = (VsSeekerTask)param!;
        MapObject.MapObject_CallDraw(PlayerAvatar.PlayerAvatar_GetMapObject(env.PlayerAvatar));
    }

    // OFFSET 0x021E1000
    public static SysTask? FieldSystem_StartVsSeekerTask(FieldSystem fieldSystem)
    {
        return FieldSystem_StartRequestedStateTask(fieldSystem, RequestStateVsSeeker);
    }

    // OFFSET 0x021E100C
    public static void FieldSystem_EndVsSeekerTask(SysTask? task)
    {
        FieldSystem_EndRequestedStateTask(task);
    }

    // OFFSET 0x021E1014
    public static SysTask? FieldSystem_StartSprite17Task(FieldSystem fieldSystem)
    {
        return FieldSystem_StartRequestedStateTask(fieldSystem, RequestStateSprite17);
    }

    // OFFSET 0x021E1020
    public static void FieldSystem_EndSprite17Task(SysTask? task)
    {
        FieldSystem_EndRequestedStateTask(task);
    }

    // OFFSET 0x021E135C
    private static void WarpAnimationTask_Rotate(WarpAnimationTask env)
    {
        switch (env.Direction)
        {
            case DirectionNorth:
                env.Direction = DirectionWest;
                break;
            case DirectionSouth:
                env.Direction = DirectionEast;
                break;
            case DirectionWest:
                env.Direction = DirectionSouth;
                break;
            case DirectionEast:
                env.Direction = DirectionNorth;
                break;
        }
        PlayerAvatar.PlayerAvatar_TryFace(env.FieldSystem.PlayerAvatar!, env.Direction);
    }

    private static int WarpAnimation_Height(int frame)
    {
        return (int)((double)frame * (WarpBaseSpeed + (double)(frame << 11)));
    }

    // OFFSET 0x021E139C
    private static void SysTask_WarpOut(SysTask task, object? param)
    {
        WarpAnimationTask env = (WarpAnimationTask)param!;
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(env.FieldSystem.PlayerAvatar!);
        switch (env.State)
        {
            case 0:
                env.State = 1;
                SoundPlayback.Sound_PlayEffect(SoundEffectWarp);
                goto case 1;
            case 1:
                if (env.Counter % 2 != 0)
                {
                    WarpAnimationTask_Rotate(env);
                }
                MapObject.MapObject_CopySpritePosOffset(mapObject, out VecFx32 offset);
                offset.Y = WarpAnimation_Height(env.Counter);
                MapObject.MapObject_SetSpritePosOffset(mapObject, offset);
                env.Counter++;
                if (env.Counter == WarpFrames)
                {
                    ScreenFadeControl.StartScreenFade(2, 0, 0, 0, FadeDuration, 1, HeapIDField);
                }
                else if (env.Counter > WarpFrames && ScreenFadeControl.IsScreenFadeDone())
                {
                    env.SetFinished(1);
                    SysTask.SysTask_Done(task);
                }
                break;
        }
    }

    // OFFSET 0x021E1470
    private static void SysTask_WarpIn(SysTask task, object? param)
    {
        WarpAnimationTask env = (WarpAnimationTask)param!;
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(env.FieldSystem.PlayerAvatar!);
        VecFx32 offset;
        switch (env.State)
        {
            case 0:
                MapObject.MapObject_SetPauseMovementOff(mapObject);
                MapObject.MapObject_CopySpritePosOffset(mapObject, out offset);
                offset.Y = WarpAnimation_Height(WarpFrames - env.Counter);
                MapObject.MapObject_SetSpritePosOffset(mapObject, offset);
                MapObjectRender.MapObject_Draw(mapObject);
                SoundPlayback.Sound_PlayEffect(SoundEffectWarp);
                env.State = 1;
                goto case 1;
            case 1:
                if (env.Counter % 2 != 0)
                {
                    WarpAnimationTask_Rotate(env);
                }
                MapObject.MapObject_CopySpritePosOffset(mapObject, out offset);
                offset.Y = WarpAnimation_Height(WarpFrames - env.Counter);
                MapObject.MapObject_SetSpritePosOffset(mapObject, offset);
                env.Counter++;
                if (env.Counter == 2)
                {
                    ScreenFadeControl.StartScreenFade(1, 1, 1, 0, FadeDuration, 1, HeapIDField);
                }
                if (env.Counter > WarpFrames)
                {
                    env.State = 2;
                }
                break;
            case 2:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    PlayerAvatar.PlayerAvatar_TryFace(env.FieldSystem.PlayerAvatar!, DirectionSouth);
                    env.SetFinished(1);
                    SysTask.SysTask_Done(task);
                }
                break;
        }
    }

    // OFFSET 0x021E15A8
    public static void FieldSystem_StartWarpAnimation(FieldSystem fieldSystem, int warpOut, Action<int> setFinished)
    {
        WarpAnimationTask env = new();
        env.FieldSystem = fieldSystem;
        env.SetFinished = setFinished;
        env.Direction = PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!);
        if (warpOut != 0)
        {
            SysTask.SysTask_Start(SysTask_WarpOut, env, WarpTaskPriority);
        }
        else
        {
            SysTask.SysTask_Start(SysTask_WarpIn, env, WarpTaskPriority);
        }
    }

    public static void FieldSystem_StartWarpAnimation(FieldSystem fieldSystem, int warpOut, MapChangeFromErrorData finishedFlagOwner)
    {
        FieldSystem_StartWarpAnimation(fieldSystem, warpOut, value => finishedFlagOwner.FinishedFlag = value);
    }
}
