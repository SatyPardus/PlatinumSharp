using static PlatinumSharp.Game.Globals;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class MonRideTask
{
    public int Active;
    public int Gender;
    public Pokemon? Mon;
    public object? CutIn;

    public void CopyFrom(MonRideTask source)
    {
        Active = source.Active;
        Gender = source.Gender;
        Mon = source.Mon;
        CutIn = source.CutIn;
    }
}

public static partial class PlayerFieldMoves
{
    public const int FieldMoveFlagStrength = 1 << 0;
    public const int FieldMoveFlagWaterfall = 1 << 1;
    public const int FieldMoveFlagDistortion = 1 << 2;

    private const int HeapIDField = 4;
    private const int DirectionNone = -1;
    private const int DirectionNorth = 0;
    private const int DirectionSouth = 1;
    private const int DirectionWest = 2;
    private const int DirectionEast = 3;
    private const int MoveStateWalking = 1;
    private const int PlayerStateWalking = 0;
    private const int PlayerStateSurfing = 2;
    private const int SpriteStateDistortionWalking = 0x18;
    private const int SpriteStateDistortionSurfing = 0x19;
    private const int SpriteStateHeal = 0x12;
    private const int GraphicsIDStrengthBoulder = 0x54;
    private const int GraphicsIDBreakableIce = 0x76;
    private const int DynamicMapFeaturesDistortionWorld = 9;
    private const uint CollisionOutOfRange = 1;
    private const int CollisionWater = 0x20;
    private const int RecordStrengthUses = 0x37;
    private const int RecordDeepMudStuck = 0x38;
    private const int SoundEffectStrength = 0x627;
    private const int SoundEffectDeepMud = 0x651;
    private const int SoundEffectDeepMudEscape = 0x652;
    private const int SoundEffectIceBreak = 0x65B;
    private const int SoundEffectSlide = 0x658;
    private const ushort BgmSurf = 0x47F;
    private const int MovementWalkFast = 0x08;
    private const int MovementWalkNormal = 0x0C;
    private const int MovementWalkSlow = 0x20;
    private const int MovementStruggle = 0x28;
    private const int MovementEscape = 0x30;
    private const int MovementJump = 0x34;
    private const int DeepMudSteps = 5;
    private const uint StatusFlagSliding = 0x100;
    private const uint StatusFlagPauseAnimation = 0x80;
    private const int SlideFrames = 7;
    private const uint SurfStartMovementsAddress = 0x021F9B3C;
    private const uint SurfEndMovementsAddress = 0x021F9B24;
    private const int DistortionStateCount = 6;
    private const int FieldOverlayID = 5;

    private sealed class StrengthTask
    {
        public int State;
        public int Direction;
        public FieldSystem FieldSystem = null!;
        public PlayerAvatar PlayerAvatar = null!;
        public MapObject Boulder = null!;
        public object? DistortionTask;
    }

    private sealed class SurfTask
    {
        public int State;
        public int Direction;
        public readonly MonRideTask MonRide = new();
        public FieldSystem FieldSystem = null!;
        public PlayerAvatar PlayerAvatar = null!;
        public MapObject MapObject = null!;
        public object? SurfMount;
    }

    private sealed class SurfEndTask
    {
        public int State;
        public int Direction;
        public FieldSystem FieldSystem = null!;
        public PlayerAvatar PlayerAvatar = null!;
        public MapObject MapObject = null!;
        public object? SurfMount;
    }

    private sealed class DeepMudTask
    {
        public int State;
        public int Steps;
        public FieldSystem FieldSystem = null!;
        public PlayerAvatar PlayerAvatar = null!;
    }

    private sealed class SlideTask
    {
        public int Direction;
        public int Counter;
        public int State;
        public FieldSystem FieldSystem = null!;
        public PlayerAvatar PlayerAvatar = null!;
    }

    private static uint[]? sSurfStartMovements;
    private static uint[]? sSurfEndMovements;

    // OFFSET 0x021DFDE0
    public static int PlayerAvatar_CheckFieldMoveTrigger(FieldSystem fieldSystem, PlayerAvatar? playerAvatar, int direction, int fieldMoveFlags)
    {
        if (playerAvatar == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar!) == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (PlayerAvatar.PlayerAvatar_CheckForcedMovement(playerAvatar!) == 1)
        {
            if (PlayerAvatar_CheckForcedMovementTrigger(fieldSystem, playerAvatar!, fieldMoveFlags) == true)
            {
                return 1;
            }
            return 0;
        }
        if (PlayerMove.PlayerAvatar_CheckStartMove(playerAvatar!, direction) == 0)
        {
            return 0;
        }
        if (direction == DirectionNone)
        {
            if (PlayerAvatar_CheckIdleTrigger(fieldSystem, playerAvatar!, direction, fieldMoveFlags) == true)
            {
                return 1;
            }
            return 0;
        }
        if (PlayerAvatar_CheckMoveTrigger(fieldSystem, playerAvatar!, direction, fieldMoveFlags) == true)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021DFE68
    private static bool PlayerAvatar_CheckMoveTrigger(FieldSystem fieldSystem, PlayerAvatar playerAvatar, int direction, int fieldMoveFlags)
    {
        int moveState = PlayerMove.PlayerAvatar_GetMoveStateFromPlayerAvatarState(playerAvatar, direction);
        if ((fieldMoveFlags & FieldMoveFlagDistortion) != 0 && ov9_0224A59C(fieldSystem, direction) == 1)
        {
            return true;
        }
        if ((fieldMoveFlags & FieldMoveFlagStrength) != 0 && FieldSystem_TryPushStrengthBoulder(fieldSystem, playerAvatar, direction, moveState) == true)
        {
            return true;
        }
        if ((fieldMoveFlags & FieldMoveFlagWaterfall) != 0 && FieldSystem_TryDescendWaterfall(fieldSystem, playerAvatar, direction, moveState) == true)
        {
            return true;
        }
        if (FieldSystem_TryStopSurfing(fieldSystem, playerAvatar, direction, moveState) == true)
        {
            return true;
        }
        if (FieldSystem_TryGetStuckInDeepMud(fieldSystem, playerAvatar, direction, moveState) == true)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021DFEF4
    private static bool PlayerAvatar_CheckIdleTrigger(FieldSystem fieldSystem, PlayerAvatar playerAvatar, int direction, int fieldMoveFlags)
    {
        int moveState = PlayerMove.PlayerAvatar_GetMoveStateFromPlayerAvatarState(playerAvatar, direction);
        if (FieldSystem_TryGetStuckInDeepMud(fieldSystem, playerAvatar, direction, moveState) == true)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021DFF1C
    private static bool FieldSystem_TryPushStrengthBoulder(FieldSystem fieldSystem, PlayerAvatar playerAvatar, int direction, int moveState)
    {
        if (moveState != MoveStateWalking)
        {
            return false;
        }
        MapObject? boulder = PlayerAvatar_GetObjectInDirection(playerAvatar, direction);
        if (boulder == null)
        {
            return false;
        }
        if (MapObject.MapObject_GetGraphicsID(boulder) != GraphicsIDStrengthBoulder)
        {
            return false;
        }
        uint collision = MapObjectMove.MapObject_CheckCollisionInDir(boulder, direction);
        if ((collision & ~CollisionOutOfRange) != 0)
        {
            if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(fieldSystem, DynamicMapFeaturesDistortionWorld) == 1)
            {
                if (ov9_0224F240(boulder, direction) == 0)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        StrengthTask_Start(direction, fieldSystem, playerAvatar, boulder);
        return true;
    }

    // OFFSET 0x021DFF88
    private static void StrengthTask_Start(int direction, FieldSystem fieldSystem, PlayerAvatar playerAvatar, MapObject boulder)
    {
        StrengthTask env = MonRideTaskEnv_New<StrengthTask>();
        env.Direction = direction;
        env.FieldSystem = fieldSystem;
        env.Boulder = boulder;
        env.PlayerAvatar = playerAvatar;
        FieldTask.FieldSystem_CreateTask(fieldSystem, FieldTask_PushStrengthBoulder, env);
        GameRecords.GameRecords_IncrementRecordValue(GameRecords.SaveData_GetGameRecords(fieldSystem.SaveData!), RecordStrengthUses);
    }

    // OFFSET 0x021DFFBC
    private static bool FieldTask_PushStrengthBoulder(FieldTask task)
    {
        StrengthTask env = (StrengthTask)FieldTask.FieldTask_GetEnv(task)!;
        MapObject playerObject = PlayerAvatar.PlayerAvatar_GetMapObject(env.PlayerAvatar);
        MapObject boulder = env.Boulder;
        switch (env.State)
        {
            case 0:
                if (MovementActions.LocalMapObj_IsAnimationSet(boulder) == 0)
                {
                    break;
                }
                if (MovementActions.LocalMapObj_IsAnimationSet(playerObject) == 0
                    && PlayerMove.PlayerAvatar_IsAnimationSetOrWalkOnSpotSlow(env.PlayerAvatar) == 0)
                {
                    break;
                }
                MovementActions.LocalMapObj_SetAnimationCode(boulder, MovementActions.MovementAction_TurnActionTowardsDir(env.Direction, MovementWalkFast));
                MovementActions.LocalMapObj_SetAnimationCode(playerObject, MovementActions.MovementAction_TurnActionTowardsDir(env.Direction, MovementWalkSlow));
                SoundPlayback.Sound_PlayEffect(SoundEffectStrength);
                env.State++;
                break;
            case 1:
                if (MovementActions.LocalMapObj_CheckAnimationFinished(boulder) == 0)
                {
                    break;
                }
                if (MovementActions.LocalMapObj_CheckAnimationFinished(playerObject) == 0)
                {
                    break;
                }
                if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(env.FieldSystem, DynamicMapFeaturesDistortionWorld) == 1
                    && ov9_0224F2B0(boulder) == 1)
                {
                    env.DistortionTask = ov9_0224F2BC(env.FieldSystem, task, boulder);
                    env.State = 3;
                    break;
                }
                MovementActions.LocalMapObj_ClearFinishedAnimation(boulder);
                MovementActions.LocalMapObj_ClearFinishedAnimation(playerObject);
                env.State++;
                MonRideTaskEnv_Free(env);
                return true;
            case 2:
                MonRideTaskEnv_Free(env);
                return true;
            case 3:
                if (ov9_0224F6EC(env.DistortionTask) == 1)
                {
                    MovementActions.LocalMapObj_ClearFinishedAnimation(boulder);
                    MovementActions.LocalMapObj_ClearFinishedAnimation(playerObject);
                    env.State = 2;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x021E00B0
    private static void SurfTask_Start(FieldSystem fieldSystem, int direction, MonRideTask monRide)
    {
        SurfTask env = MonRideTaskEnv_New<SurfTask>();
        env.Direction = direction;
        env.FieldSystem = fieldSystem;
        env.PlayerAvatar = fieldSystem.PlayerAvatar!;
        env.MapObject = PlayerAvatar.PlayerAvatar_GetMapObject(env.PlayerAvatar);
        env.MonRide.CopyFrom(monRide);
        FieldTask.FieldTask_InitCall(fieldSystem.Task!, FieldTask_UseSurf, env);
    }

    // OFFSET 0x021E00EC
    public static void FieldTask_StartUseSurf(FieldTask task, int direction, int partySlot)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        MonRideTask monRide = new();
        MonRideTask_Init(fieldSystem, GetPokemonByIndex(fieldSystem, partySlot), monRide);
        SurfTask_Start(fieldSystem, direction, monRide);
    }

    // OFFSET 0x021E0118
    public static int PlayerAvatar_CanUseSurf(PlayerAvatar? playerAvatar, int currentTileBehavior, int facingTileBehavior)
    {
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar!);
        if (TileBehavior.TileBehavior_IsSurfable((byte)facingTileBehavior) == 1)
        {
            if ((TileBehavior.TileBehavior_IsBridge((byte)currentTileBehavior) == 1
                    || TileBehavior.TileBehavior_IsBridgeStart((byte)currentTileBehavior) == 1)
                && MapObject.MapObject_IsStatusOnElevatedBridge(mapObject) == 1)
            {
                return 0;
            }
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021E0160
    private static bool FieldTask_UseSurf(FieldTask task)
    {
        SurfTask env = (SurfTask)FieldTask.FieldTask_GetEnv(task)!;
        switch (env.State)
        {
            case 0:
                if (PlayerAvatar.PlayerAvatar_MapDistortionState(env.PlayerAvatar) == 0)
                {
                    FieldBgm.FieldBGM_SetOverride(env.FieldSystem, 0);
                    FieldBgm.FieldBGM_TryFadeOut(env.FieldSystem, BgmSurf, 1);
                }
                if (env.MonRide.Active == 1)
                {
                    NewMonRideCutIn(env.FieldSystem, env.MonRide);
                    env.State++;
                }
                else
                {
                    env.State = 2;
                }
                break;
            case 1:
                if (CheckMonRideCutInFinished(env.MonRide) == true)
                {
                    env.State++;
                }
                break;
            case 2:
                if (PlayerAvatar.PlayerAvatar_MapDistortionState(env.PlayerAvatar) == 0)
                {
                    int x = PlayerAvatar.PlayerAvatar_GetXPos(env.PlayerAvatar) + MapObjectMove.MapObject_GetDxFromDir(env.Direction);
                    int z = PlayerAvatar.PlayerAvatar_GetZPos(env.PlayerAvatar) + MapObjectMove.MapObject_GetDzFromDir(env.Direction);
                    env.SurfMount = SurfMount.SurfMountRenderer_HandleSurfBegin(env.MapObject, x, z, env.Direction, 0);
                }
                else
                {
                    int x = MapObject.MapObject_GetX(env.MapObject);
                    int y = MapObject.MapObject_GetY(env.MapObject) / 2;
                    int z = MapObject.MapObject_GetZ(env.MapObject);
                    int distortionState = PlayerAvatar.PlayerAvatar_MapDistortionState(env.PlayerAvatar);
                    PlayerMove.PlayerAvatar_GetStepDistortionWorldPos(env.PlayerAvatar, env.Direction, ref x, ref y, ref z);
                    env.SurfMount = ov5_021F85BC(env.PlayerAvatar, x, y, z, env.Direction, 0, distortionState);
                }
                PlayerAvatar.PlayerAvatar_SetSurfMountAnimManager(env.PlayerAvatar, env.SurfMount);
                PlayerAvatar.PlayerAvatar_SetPlayerState(env.PlayerAvatar, PlayerStateSurfing);
                env.State++;
                break;
            case 3:
            case 4:
                if (env.State == 3)
                {
                    env.State = 4;
                }
                if (MovementActions.LocalMapObj_IsAnimationSet(env.MapObject) == 1)
                {
                    sSurfStartMovements ??= RomData.OverlayU32(FieldOverlayID, SurfStartMovementsAddress, DistortionStateCount);
                    int movement = (int)sSurfStartMovements[PlayerAvatar.PlayerAvatar_MapDistortionState(env.PlayerAvatar)];
                    MovementActions.LocalMapObj_SetAnimationCode(env.MapObject, MovementActions.MovementAction_TurnActionTowardsDir(env.Direction, movement));
                    env.State++;
                }
                break;
            case 5:
                if (MovementActions.LocalMapObj_CheckAnimationFinished(env.MapObject) == 0)
                {
                    break;
                }
                MovementActions.LocalMapObj_ClearFinishedAnimation(env.MapObject);
                if (PlayerAvatar.PlayerAvatar_MapDistortionState(env.PlayerAvatar) == 0)
                {
                    SurfMount.SurfMountRenderer_SetSyncPos((OverworldAnimManager)env.SurfMount!, 1);
                    PlayerAvatarRequestState.PlayerAvatar_SetGraphics(env.PlayerAvatar, PlayerAvatar.Player_GetSpriteFromStateAndGender(PlayerStateSurfing, PlayerAvatar.PlayerAvatar_GetGender(env.PlayerAvatar)));
                }
                else
                {
                    ov5_021F88CC((OverworldAnimManager?)env.SurfMount, 2);
                    PlayerAvatarRequestState.PlayerAvatar_SetGraphics(env.PlayerAvatar, PlayerAvatar.Player_GetSpriteFromStateAndGender(SpriteStateDistortionSurfing, PlayerAvatar.PlayerAvatar_GetGender(env.PlayerAvatar)));
                }
                MonRideTaskEnv_Free(env);
                return true;
        }
        return false;
    }

    // OFFSET 0x021E032C
    private static bool FieldSystem_TryStopSurfing(FieldSystem fieldSystem, PlayerAvatar playerAvatar, int direction, int moveState)
    {
        if (moveState != MoveStateWalking || PlayerAvatar.PlayerAvatar_GetPlayerState(playerAvatar) != PlayerStateSurfing)
        {
            return false;
        }
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar);
        int collision;
        if (PlayerAvatar.PlayerAvatar_DistortionGravityChanged(playerAvatar) == 0)
        {
            collision = PlayerMove.PlayerAvatar_CheckCollision(playerAvatar, mapObject, direction);
        }
        else
        {
            collision = PlayerMove.PlayerAvatar_CheckDistortionCollision(playerAvatar, mapObject, direction);
        }
        if (collision == CollisionWater)
        {
            return false;
        }
        if ((collision & ~CollisionWater) != 0)
        {
            return false;
        }
        SurfEndTask_Start(direction, fieldSystem, playerAvatar);
        return true;
    }

    // OFFSET 0x021E0390
    private static void SurfEndTask_Start(int direction, FieldSystem fieldSystem, PlayerAvatar playerAvatar)
    {
        SurfEndTask env = MonRideTaskEnv_New<SurfEndTask>();
        env.Direction = direction;
        env.FieldSystem = fieldSystem;
        env.PlayerAvatar = playerAvatar;
        env.MapObject = PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar);
        env.SurfMount = PlayerAvatar.PlayerAvatar_GetSurfMountAnimManager(playerAvatar);
        FieldTask.FieldSystem_CreateTask(fieldSystem, FieldTask_StopSurfing, env);
    }

    // OFFSET 0x021E03C8
    private static bool FieldTask_StopSurfing(FieldTask task)
    {
        SurfEndTask env = (SurfEndTask)FieldTask.FieldTask_GetEnv(task)!;
        switch (env.State)
        {
            case 0:
                if (MovementActions.LocalMapObj_IsAnimationSet(env.MapObject) == 1)
                {
                    sSurfEndMovements ??= RomData.OverlayU32(FieldOverlayID, SurfEndMovementsAddress, DistortionStateCount);
                    int distortionState = PlayerAvatar.PlayerAvatar_MapDistortionState(env.PlayerAvatar);
                    int graphicsID;
                    if (distortionState == 0)
                    {
                        SurfMount.SurfMountRenderer_SetSyncPos((OverworldAnimManager)env.SurfMount!, 0);
                        graphicsID = PlayerAvatar.Player_GetSpriteFromStateAndGender(PlayerStateWalking, PlayerAvatar.PlayerAvatar_GetGender(env.PlayerAvatar));
                    }
                    else
                    {
                        ov5_021F88DC((OverworldAnimManager?)env.SurfMount, 2);
                        graphicsID = PlayerAvatar.Player_GetSpriteFromStateAndGender(SpriteStateDistortionWalking, PlayerAvatar.PlayerAvatar_GetGender(env.PlayerAvatar));
                    }
                    PlayerAvatarRequestState.PlayerAvatar_SetGraphics(env.PlayerAvatar, graphicsID);
                    MovementActions.LocalMapObj_SetAnimationCode(env.MapObject, MovementActions.MovementAction_TurnActionTowardsDir(env.Direction, (int)sSurfEndMovements[distortionState]));
                    env.State++;
                }
                break;
            case 1:
                if (MovementActions.LocalMapObj_CheckAnimationFinished(env.MapObject) == 0)
                {
                    break;
                }
                MovementActions.LocalMapObj_ClearFinishedAnimation(env.MapObject);
                if (env.SurfMount != null)
                {
                    FieldEffects.FieldEffectManager_FinishAnimManager((OverworldAnimManager?)env.SurfMount);
                }
                PlayerAvatar.PlayerAvatar_SetSurfMountAnimManager(env.PlayerAvatar, null);
                PlayerAvatar.PlayerAvatar_SetPlayerState(env.PlayerAvatar, PlayerStateWalking);
                FieldBgm.FieldBGM_TryFadeOut(env.FieldSystem, FieldBgm.FieldBGM_GetForMapHeader(env.FieldSystem, (int)env.FieldSystem.Location!.U32(0)), 1);
                MonRideTaskEnv_Free(env);
                return true;
        }
        return false;
    }

    // OFFSET 0x021E04A8
    private static bool FieldSystem_TryDescendWaterfall(FieldSystem fieldSystem, PlayerAvatar playerAvatar, int direction, int moveState)
    {
        if (direction != DirectionSouth || moveState != MoveStateWalking || PlayerAvatar.PlayerAvatar_GetPlayerState(playerAvatar) != PlayerStateSurfing)
        {
            return false;
        }
        if (TileBehavior.TileBehavior_IsWaterfall((byte)PlayerMove.PlayerAvatar_GetTileBehaviorFromDir(playerAvatar, direction)) == 0)
        {
            return false;
        }
        FieldSystem_CreateTaskUseWaterfall(fieldSystem, direction);
        return true;
    }

    // OFFSET 0x021E04EC
    private static bool FieldSystem_TryGetStuckInDeepMud(FieldSystem fieldSystem, PlayerAvatar playerAvatar, int direction, int moveState)
    {
        if (PlayerAvatar.PlayerAvatar_CheckEscapedFromDeepMud(playerAvatar) == 1)
        {
            return false;
        }
        byte behavior = (byte)MapObject.MapObject_GetCurrTileBehavior(PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar));
        if (TileBehavior.TileBehavior_IsDeepMud(behavior) == 1 || TileBehavior.TileBehavior_IsDeepMudWithGrass(behavior) == 1)
        {
            FieldSystem_CreateTaskStuckInDeepMud(fieldSystem, playerAvatar);
            return true;
        }
        return false;
    }

    // OFFSET 0x021E0534
    private static void FieldSystem_CreateTaskStuckInDeepMud(FieldSystem fieldSystem, PlayerAvatar playerAvatar)
    {
        DeepMudTask env = MonRideTaskEnv_New<DeepMudTask>();
        env.FieldSystem = fieldSystem;
        env.PlayerAvatar = playerAvatar;
        FieldTask.FieldSystem_CreateTask(fieldSystem, FieldTask_StuckInDeepMud, env);
        GameRecords.GameRecords_IncrementRecordValue(GameRecords.SaveData_GetGameRecords(fieldSystem.SaveData!), RecordDeepMudStuck);
    }

    // OFFSET 0x021E0560
    private static bool FieldTask_StuckInDeepMud(FieldTask task)
    {
        DeepMudTask env = (DeepMudTask)FieldTask.FieldTask_GetEnv(task)!;
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(env.PlayerAvatar);
        if (env.State == 0)
        {
            SoundPlayback.Sound_PlayEffect(SoundEffectDeepMud);
            env.State++;
        }
        if (env.State == 1)
        {
            if (MovementActions.LocalMapObj_IsAnimationSet(mapObject) == 0
                && PlayerMove.PlayerAvatar_IsAnimationSetOrWalkOnSpotSlow(env.PlayerAvatar) == 0)
            {
                return false;
            }
            if (env.Steps >= DeepMudSteps)
            {
                SoundPlayback.Sound_PlayEffect(SoundEffectDeepMudEscape);
                PlayerAvatar.PlayerAvatar_SetEscapedFromDeepMud(env.PlayerAvatar, 1);
                MonRideTaskEnv_Free(env);
                return true;
            }
            env.State++;
        }
        if (env.State == 2)
        {
            int facingDir = PlayerAvatar.PlayerAvatar_GetFacingDir(env.PlayerAvatar);
            int direction = PlayerMove.PlayerAvatar_CalcFaceDirection(env.PlayerAvatar, (ushort)gSystem.PressedKeys, (ushort)gSystem.HeldKeys);
            if (direction == DirectionNone || direction == facingDir)
            {
                return false;
            }
            env.State = 1;
            env.Steps++;
            if (TileBehavior.TileBehavior_IsDeepMudWithGrass((byte)MapObject.MapObject_GetCurrTileBehavior(mapObject)) == 1)
            {
                FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
                if (ov6_022413E4(fieldSystem, out object? battleParams) == 1)
                {
                    PlayerAvatar.PlayerAvatar_SetEscapedFromDeepMud(env.PlayerAvatar, 1);
                    MonRideTaskEnv_Free(env);
                    sub_02050E78(fieldSystem, task, battleParams);
                    return false;
                }
            }
            int movement;
            if (env.Steps < DeepMudSteps)
            {
                movement = MovementStruggle;
            }
            else
            {
                movement = MovementEscape;
                MapObject.MapObject_SetFlagDoNotSinkIntoTerrain(mapObject, 1);
            }
            PlayerMove.PlayerAvatar_SetMapObjMovement(env.PlayerAvatar, MovementActions.MovementAction_TurnActionTowardsDir(direction, movement), 1);
        }
        return false;
    }

    // OFFSET 0x021E067C
    private static bool PlayerAvatar_CheckForcedMovementTrigger(FieldSystem fieldSystem, PlayerAvatar playerAvatar, int fieldMoveFlags)
    {
        if (MovementActions.LocalMapObj_IsAnimationSet(PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar)) == 0)
        {
            return false;
        }
        if (PlayerAvatar_TryBreakIce(fieldSystem, playerAvatar) == true)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021E06A8
    private static bool PlayerAvatar_TryBreakIce(FieldSystem fieldSystem, PlayerAvatar playerAvatar)
    {
        if (PlayerAvatar.PlayerAvatar_GetSpeed(playerAvatar) < 1)
        {
            return false;
        }
        MapObject? mapObject = PlayerAvatar_GetObjectInDirection(playerAvatar, PlayerAvatar.PlayerAvatar_GetMovingDir(playerAvatar));
        if (mapObject == null)
        {
            return false;
        }
        if (MapObject.MapObject_GetGraphicsID(mapObject) != GraphicsIDBreakableIce)
        {
            return false;
        }
        MapObjectDrawHandlers.AnimModel_DrawDelete(mapObject);
        MapObject.MapObject_Delete(mapObject);
        SoundPlayback.Sound_PlayEffect(SoundEffectIceBreak);
        return false;
    }

    // OFFSET 0x021E1028
    private static void MonRideTask_Init(FieldSystem fieldSystem, Pokemon mon, MonRideTask monRide)
    {
        monRide.Active = 1;
        monRide.Mon = mon;
        monRide.Gender = PlayerAvatar.PlayerAvatar_GetGender(fieldSystem.PlayerAvatar!);
    }

    // OFFSET 0x021E103C
    private static void NewMonRideCutIn(FieldSystem fieldSystem, MonRideTask monRide)
    {
        monRide.CutIn = ov6_02243F88(fieldSystem, 0, monRide.Mon!, monRide.Gender);
    }

    // OFFSET 0x021E1050
    private static bool CheckMonRideCutInFinished(MonRideTask monRide)
    {
        if (ov6_02243FBC(monRide.CutIn) == 1)
        {
            ov6_02243FC8(monRide.CutIn);
            return true;
        }
        return false;
    }

    // OFFSET 0x021E10D4
    private static MapObject? PlayerAvatar_GetObjectInDirection(PlayerAvatar playerAvatar, int direction)
    {
        int x = PlayerAvatar.PlayerAvatar_GetXPos(playerAvatar) + MapObjectMove.MapObject_GetDxFromDir(direction);
        int z = PlayerAvatar.PlayerAvatar_GetZPos(playerAvatar) + MapObjectMove.MapObject_GetDzFromDir(direction);
        MapObjectManager mapObjectManager = MapObject.MapObject_MapObjectManager(PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar))!;
        return MapObjectManager.MapObjectMan_FindObjectAtTile(mapObjectManager, x, z, 0);
    }

    // OFFSET 0x021E1110
    private static T MonRideTaskEnv_New<T>() where T : class, new()
    {
        return new T();
    }

    // OFFSET 0x021E1134
    private static void MonRideTaskEnv_Free(object env)
    {
    }

    // OFFSET 0x021E1140
    private static Pokemon GetPokemonByIndex(FieldSystem fieldSystem, int partySlot)
    {
        return Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), partySlot);
    }

    // OFFSET 0x021E1154
    public static int PlayerAvatar_CheckSlideTile(FieldSystem fieldSystem, PlayerAvatar? playerAvatar, int unusedDirection)
    {
        byte behavior = (byte)MapObject.MapObject_GetCurrTileBehavior(PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar!));
        int direction;
        if (TileBehavior.TileBehavior_IsSlideEastward(behavior) == 1)
        {
            direction = DirectionEast;
        }
        else if (TileBehavior.TileBehavior_IsSlideWestward(behavior) == 1)
        {
            direction = DirectionWest;
        }
        else if (TileBehavior.TileBehavior_IsSlideNorthward(behavior) == 1)
        {
            direction = DirectionNorth;
        }
        else if (TileBehavior.TileBehavior_IsSlideSouthward(behavior) == 1)
        {
            direction = DirectionSouth;
        }
        else
        {
            return 0;
        }
        SlideTask_Start(fieldSystem, playerAvatar!, direction);
        return 1;
    }

    // OFFSET 0x021E11B0
    private static void SlideTask_Start(FieldSystem fieldSystem, PlayerAvatar playerAvatar, int direction)
    {
        SlideTask env = SlideTaskEnv_New();
        env.FieldSystem = fieldSystem;
        env.PlayerAvatar = playerAvatar;
        env.Direction = direction;
        SoundPlayback.Sound_PlayEffect(SoundEffectSlide);
        FieldTask.FieldSystem_CreateTask(fieldSystem, FieldTask_Slide, env);
    }

    // OFFSET 0x021E11E0
    private static int Direction_RotateCounterclockwise(int direction)
    {
        switch (direction)
        {
            case DirectionNorth:
                return DirectionWest;
            case DirectionSouth:
                return DirectionEast;
            case DirectionWest:
                return DirectionSouth;
            case DirectionEast:
                return DirectionNorth;
        }
        return DirectionNorth;
    }

    // OFFSET 0x021E120C
    private static bool FieldTask_Slide(FieldTask task)
    {
        SlideTask env = (SlideTask)FieldTask.FieldTask_GetEnv(task)!;
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(env.PlayerAvatar);
        byte behavior = (byte)MapObject.MapObject_GetCurrTileBehavior(mapObject);
        switch (env.State)
        {
            case 0:
                MapObject.MapObject_SetStatusFlagOn(mapObject, StatusFlagSliding);
                env.State++;
                break;
            case 1:
                if (PlayerMove.PlayerAvatar_IsMapObjectAnimationSet(env.PlayerAvatar) == 0)
                {
                    break;
                }
                PlayerMove.PlayerAvatar_SetMapObjMovement(env.PlayerAvatar, MovementActions.MovementAction_TurnActionTowardsDir(env.Direction, MovementWalkNormal), 1);
                PlayerAvatar.PlayerAvatar_TryFace(env.PlayerAvatar, env.Direction);
                env.State++;
                env.Counter = SlideFrames;
                break;
            case 2:
                if (env.Counter == 2 || env.Counter == 4 || env.Counter == 6)
                {
                    env.Direction = Direction_RotateCounterclockwise(env.Direction);
                    PlayerAvatar.PlayerAvatar_TryFace(env.PlayerAvatar, env.Direction);
                }
                env.Counter--;
                if (env.Counter != 0)
                {
                    break;
                }
                if (TileBehavior.TileBehavior_IsSlideEastward(behavior) == 1)
                {
                    env.Direction = DirectionEast;
                }
                else if (TileBehavior.TileBehavior_IsSlideWestward(behavior) == 1)
                {
                    env.Direction = DirectionWest;
                }
                else if (TileBehavior.TileBehavior_IsSlideNorthward(behavior) == 1)
                {
                    env.Direction = DirectionNorth;
                }
                else if (TileBehavior.TileBehavior_IsSlideSouthward(behavior) == 1)
                {
                    env.Direction = DirectionSouth;
                }
                else
                {
                    env.Direction = Direction_RotateCounterclockwise(env.Direction);
                }
                if (PlayerMove.PlayerAvatar_CheckCollision(env.PlayerAvatar, mapObject, env.Direction) == 0)
                {
                    env.State = 1;
                    break;
                }
                MapObject.MapObject_SetStatusFlagOff(mapObject, StatusFlagPauseAnimation);
                MapObject.MapObject_SetStatusFlagOff(mapObject, StatusFlagSliding);
                PlayerAvatar.PlayerAvatar_TryFace(env.PlayerAvatar, env.Direction);
                SlideTaskEnv_Free(env);
                SoundPlayback.Sound_StopEffect(SoundEffectSlide, 0);
                return true;
        }
        return false;
    }

    // OFFSET 0x021E132C
    private static SlideTask SlideTaskEnv_New()
    {
        return new SlideTask();
    }

    // OFFSET 0x021E1350
    private static void SlideTaskEnv_Free(SlideTask env)
    {
    }
}
