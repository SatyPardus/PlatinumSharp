using static PlatinumSharp.Game.PlayerAvatar;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class PlayerMove
{
    public const int CollisionHit = 0x01;
    public const int CollisionMapObject = 0x02;
    public const int CollisionJump = 0x04;
    public const int CollisionWarp = 0x08;
    public const int CollisionBikeRamp = 0x10;
    public const int CollisionWater = 0x20;
    public const int CollisionBikeBridge = 0x40;
    public const int CollisionJumpTwice = 0x80;

    private const int MapObjectCollisionOutOfRange = 0x01;
    private const int MapObjectCollisionTerrain = 0x02;
    private const int MapObjectCollisionObject = 0x04;
    private const int MapObjectCollisionWarp = 0x08;

    private const int TileMovementNone = 0;
    private const int TileMovementIce = 1;
    private const int TileMovementBikeSlope = 2;
    private const int TileMovementBikeRampEast = 3;
    private const int TileMovementBikeRampWest = 4;
    private const int TileMovementCyclingRoadDown = 5;

    private const int HeightChangeNone = 0;
    private const int HeightChangeUp = 1;
    private const int HeightChangeDown = 2;

    private const int BikeStateStop = 0;
    private const int BikeStateMoving = 1;
    private const int BikeStateTurning = 2;
    private const int BikeStateBrake = 3;

    private const int KeyB = 0x02;
    private const int KeyRight = 0x10;
    private const int KeyLeft = 0x20;
    private const int KeyUp = 0x40;
    private const int KeyDown = 0x80;

    private const int ActionFace = 0x00;
    private const int ActionWalkSlowest = 0x04;
    private const int ActionWalkSlow = 0x08;
    private const int ActionWalkNormal = 0x0C;
    private const int ActionWalkFast = 0x10;
    private const int ActionWalkFaster = 0x14;
    private const int ActionBikeSlopeSlide = 0x15;
    private const int ActionWalkOnSpotSlow = 0x1C;
    private const int ActionTurn = 0x28;
    private const int ActionJump = 0x38;
    private const int ActionBikeSlow = 0x4C;
    private const int ActionBikeNormal = 0x50;
    private const int ActionRun = 0x58;
    private const int ActionRampWestSlow = 0x5C;
    private const int ActionRampEastSlow = 0x5D;
    private const int ActionRampWestFast = 0x5E;
    private const int ActionRampEastFast = 0x5F;
    private const int ActionJumpTwice = 0x75;

    private const int SeqWalkPuddle = 0x549;
    private const int SeqBump = 0x601;
    private const int SeqGearHigh = 0x619;
    private const int SeqGearLow = 0x61C;
    private const int SeqWalkTerrain641 = 0x641;
    private const int SeqWalkTerrain642 = 0x642;
    private const int SeqWalkTerrain653 = 0x653;
    private const int SeqBikeSlope = 0x654;
    private const int SeqWalkTerrain655 = 0x655;
    private const int SeqBikeRamp = 0x656;

    private const uint MapObjectStatusForcedMovement = 0x180;
    private const int DynamicMapFeaturesDistortionWorld = 9;
    private const int RecordStepsTaken = 0;
    private const int IceHeightProbeDistance = 0x4000;
    private const int BikeMaxSpeed = 3;

    private const uint TileMoveHandlerTableAddress = 0x020EDAEC;
    private const uint TileMovementTableAddress = 0x020EDB84;
    private const uint DistortionFloorStepTableAddress = 0x020EDB04;
    private const uint DistortionWestWallStepTableAddress = 0x020EDB24;
    private const uint DistortionEastWallStepTableAddress = 0x020EDB44;
    private const uint DistortionCeilingStepTableAddress = 0x020EDB64;
    private const uint WestWallWalkActionsAddress = 0x020EDACC;
    private const uint WestWallRunActionsAddress = 0x020EDABC;
    private const uint WestWallSurfActionsAddress = 0x020EDAAC;
    private const uint WestWallBumpActionsAddress = 0x020EDA9C;
    private const uint WestWallTurnDirectionsAddress = 0x020EDA8C;
    private const uint EastWallWalkActionsAddress = 0x020EDA7C;
    private const uint EastWallRunActionsAddress = 0x020EDA5C;
    private const uint EastWallSurfActionsAddress = 0x020ED9EC;
    private const uint EastWallBumpActionsAddress = 0x020ED9CC;
    private const uint EastWallTurnDirectionsAddress = 0x020EDADC;
    private const uint CeilingWalkActionsAddress = 0x020EDA2C;
    private const uint CeilingRunActionsAddress = 0x020EDA1C;
    private const uint CeilingSurfActionsAddress = 0x020EDA6C;
    private const uint CeilingBumpActionsAddress = 0x020ED9DC;
    private const uint CeilingTurnDirectionsAddress = 0x020EDA3C;
    private const uint WestWallTurnActionDirectionsAddress = 0x020EDA0C;
    private const uint EastWallTurnActionDirectionsAddress = 0x020EDA4C;
    private const uint CeilingTurnActionDirectionsAddress = 0x020ED9FC;

    private delegate int TileMoveHandler(PlayerAvatar playerAvatar, int direction);
    private delegate int TileBehaviorCheck(int tileBehavior);

    private static TileMoveHandler TileMoveHandlerFromAddress(uint address)
    {
        switch (address & ~1u)
        {
            case 0x0205F6C4:
                return PlayerAvatar_TileMove_None;
            case 0x0205F6D0:
                return PlayerAvatar_TileMove_Ice;
            case 0x0205F808:
                return PlayerAvatar_TileMove_BikeSlope;
            case 0x0205F95C:
                return PlayerAvatar_TileMove_BikeRampEast;
            case 0x0205F9AC:
                return PlayerAvatar_TileMove_BikeRampWest;
            case 0x0205F9FC:
                return PlayerAvatar_TileMove_CyclingRoadDown;
        }
        throw new InvalidOperationException($"Unknown tile movement handler 0x{address:X8}");
    }

    private static TileBehaviorCheck TileBehaviorCheckFromAddress(uint address)
    {
        switch (address & ~1u)
        {
            case 0x0205DC5C:
                return TileBehavior.TileBehavior_IsIce;
            case 0x0205DD68:
                return TileBehavior.TileBehavior_IsBikeSlope;
            case 0x0205DD90:
                return TileBehavior.TileBehavior_IsBikeRampEastward;
            case 0x0205DD9C:
                return TileBehavior.TileBehavior_IsBikeRampWestward;
        }
        throw new InvalidOperationException($"Unknown tile behavior check 0x{address:X8}");
    }

    // OFFSET 0x0205F180
    public static void PlayerAvatar_MoveMain(PlayerAvatar playerAvatar, object? landDataManager, int direction, ushort pressedKeys, ushort heldKeys, int poketchTapped)
    {
        if (direction == DirectionNone)
        {
            direction = PlayerAvatar_CalcFaceDirectionInternal(playerAvatar, pressedKeys, heldKeys);
        }
        PlayerAvatar_TryCyclingGearChange(playerAvatar, pressedKeys);
        if (PlayerAvatar_CheckStartMoveInternal(playerAvatar, direction) == 0)
        {
            return;
        }
        PlayerAvatar_StartMoveInit(playerAvatar, direction, pressedKeys, heldKeys);
        PlayerAvatarRequestState.PlayerAvatar_ProcessRequestedStates(playerAvatar);
        if (PlayerAvatar_TryMoveFromCurTileBehavior(playerAvatar, direction) == 1)
        {
            PlayerAvatarRequestState.PlayerAvatar_RestoreWalkingGraphics(playerAvatar);
            return;
        }
        if (PlayerAvatar_GetPlayerState(playerAvatar) == StateWalking)
        {
            if (PlayerAvatar_UpdateMoveState2(playerAvatar, direction) != MoveStateNone)
            {
                PlayerAvatarRequestState.PlayerAvatar_RestoreWalkingGraphics(playerAvatar);
            }
            else if (poketchTapped == 1)
            {
                PlayerAvatarRequestState.PlayerAvatar_RefreshWalkingGraphics(playerAvatar);
            }
        }
        PlayerAvatar_SetMovementFromState(playerAvatar, landDataManager, direction, pressedKeys, heldKeys);
        PlayerAvatar_EnableTileBehaviorCheck(playerAvatar);
        PlayerAvatar_PlayWalkSE(playerAvatar);
    }

    // OFFSET 0x0205F224
    public static int PlayerAvatar_CheckStartMove(PlayerAvatar playerAvatar, int direction)
    {
        return PlayerAvatar_CheckStartMoveInternal(playerAvatar, direction);
    }

    // OFFSET 0x0205F22C
    private static int PlayerAvatar_CheckStartMoveInternal(PlayerAvatar playerAvatar, int direction)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        if (MovementActions.LocalMapObj_IsAnimationSet(mapObject) == 1)
        {
            return 1;
        }
        if (direction == DirectionNone)
        {
            return 0;
        }
        if (MovementAction_IsWalkOnSpotSlow(MapObject.MapObject_GetMovementAction(mapObject)) != 1)
        {
            return 0;
        }
        int collision;
        if (PlayerAvatar_DistortionGravityChanged(playerAvatar) == 1)
        {
            collision = PlayerAvatar_CheckDistortionCollision(playerAvatar, mapObject, direction);
            if ((collision & ~CollisionJumpTwice) != 0 && PlayerAvatar_MapDistortionState(playerAvatar) == DistortionStateFloor)
            {
                FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
                int x = MapObject.MapObject_GetX(mapObject);
                int y = MapObject.MapObject_GetY(mapObject) / 2;
                int z = MapObject.MapObject_GetZ(mapObject);
                int stepX = x;
                int stepY = y;
                int stepZ = z;
                PlayerAvatar_GetStepDistortionWorldPos(playerAvatar, direction, ref stepX, ref stepY, ref stepZ);
                if (ov9_02250FBC(fieldSystem, stepX, stepY, stepZ) == 0 && ov9_02250FD8(fieldSystem, stepX, stepY, stepZ) == 1)
                {
                    ov9_02251000(fieldSystem, stepX, stepY, stepZ);
                    collision = PlayerAvatar_CheckDistortionCollision(playerAvatar, mapObject, direction);
                    ov9_02251000(fieldSystem, x, y, z);
                }
            }
        }
        else
        {
            collision = PlayerAvatar_CheckCollision(playerAvatar, mapObject, direction);
        }
        if (collision == 0)
        {
            return 1;
        }
        if (collision == CollisionWater && PlayerAvatar_GetPlayerState(playerAvatar) == StateSurfing)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205F350
    private static void PlayerAvatar_StartMoveInit(PlayerAvatar playerAvatar, int direction, ushort pressedKeys, ushort heldKeys)
    {
        int leftOrRight = Movement_FaceLeftRightFromInput(heldKeys);
        int upOrDown = Movement_FaceUpDownFromInput(heldKeys);
        PlayerAvatar_SetFaceDirection(playerAvatar, leftOrRight, upOrDown);
        PlayerAvatar_EndStep(playerAvatar);
    }

    // OFFSET 0x0205F378
    private static void PlayerAvatar_EnableTileBehaviorCheck(PlayerAvatar playerAvatar)
    {
        if (PlayerAvatar_CheckStep(playerAvatar) == 1 && PlayerAvatar_GetMoveState(playerAvatar) == MoveStateMoving)
        {
            PlayerAvatar_SetIgnoreTileBehavior(playerAvatar, 0);
            if (PlayerAvatar_CheckEscapedFromDeepMud(playerAvatar) == 1)
            {
                PlayerAvatar_SetEscapedFromDeepMud(playerAvatar, 0);
                MapObject.MapObject_SetFlagDoNotSinkIntoTerrain(PlayerAvatar_GetMapObject(playerAvatar), 0);
            }
        }
    }

    // OFFSET 0x0205F3B8
    private static void PlayerAvatar_PlayWalkSE(PlayerAvatar playerAvatar)
    {
        if (PlayerAvatar_GetMoveState(playerAvatar) != MoveStateMoving)
        {
            return;
        }
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        int tileBehavior = (byte)MapObject.MapObject_GetCurrTileBehavior(mapObject);
        int direction = MovementActions.MovementAction_GetDirFromAction(MapObject.MapObject_GetMovementAction(mapObject));
        int nextTileBehavior;
        if (direction == DirectionNone)
        {
            nextTileBehavior = tileBehavior;
        }
        else
        {
            nextTileBehavior = (byte)MapObjectMove.MapObject_GetTileBehaviorFromDir(mapObject, direction);
        }
        if (MapObjectMove.MapObject_IsOnSnow(mapObject, tileBehavior) == 1 || TileBehavior.TileBehavior_IsSnowWithShadows(tileBehavior) == 1)
        {
            SoundPlayback.Sound_PlayEffect(SeqWalkPuddle);
        }
        if (TileBehavior.TileBehavior_IsPuddle(tileBehavior) == 1)
        {
            SoundPlayback.Sound_PlayEffect(SeqWalkTerrain641);
        }
        if (TileBehavior.TileBehavior_IsShallowWater(tileBehavior) == 1)
        {
            SoundPlayback.Sound_PlayEffect(SeqWalkTerrain642);
        }
        TileBehavior.TileBehavior_IsSand(tileBehavior);
        if (TileBehavior.TileBehavior_IsMud(tileBehavior) == 1 && TileBehavior.TileBehavior_IsDeepMud(tileBehavior) != 1)
        {
            SoundPlayback.Sound_PlayEffect(SeqWalkTerrain655);
        }
        if (MovementAction_IsWalkOnSpotSlow(MapObject.MapObject_GetMovementAction(mapObject)) == 0)
        {
            if (TileBehavior.TileBehavior_IsVeryTallGrass(tileBehavior) == 1 || TileBehavior.TileBehavior_IsVeryTallGrass(nextTileBehavior) == 1)
            {
                SoundPlayback.Sound_PlayEffect(SeqWalkTerrain653);
            }
        }
    }

    // OFFSET 0x0205F490
    public static void PlayerAvatar_UpdatePlayerMoveState(PlayerAvatar playerAvatar)
    {
        int moveState = PlayerAvatar_GetMoveState(playerAvatar);
        int playerMoveState = PlayerAvatar_GetPlayerMoveState(playerAvatar);
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        PlayerAvatar_SetPlayerMoveState(playerAvatar, PlayerMoveStateNone);
        int tileMovement = PlayerAvatar_GetTileMovement(playerAvatar, DirectionNone);
        if (tileMovement != TileMovementNone && tileMovement != TileMovementCyclingRoadDown)
        {
            PlayerAvatar_SetPlayerMoveState(playerAvatar, PlayerMoveStateMoving);
            return;
        }
        if (MovementActions.LocalMapObj_IsAnimationSet(mapObject) == 0)
        {
            switch (moveState)
            {
                case MoveStateNone:
                    break;
                case MoveStateMoving:
                    if (MovementAction_IsWalkOnSpotSlow(MapObject.MapObject_GetMovementAction(mapObject)) == 1)
                    {
                        break;
                    }
                    if (playerMoveState == PlayerMoveStateNone || playerMoveState == PlayerMoveStateEnd)
                    {
                        PlayerAvatar_SetPlayerMoveState(playerAvatar, PlayerMoveStateStart);
                    }
                    else
                    {
                        PlayerAvatar_SetPlayerMoveState(playerAvatar, PlayerMoveStateMoving);
                    }
                    break;
                case MoveStateTurning:
                    PlayerAvatar_SetPlayerMoveState(playerAvatar, PlayerMoveStateMoving);
                    break;
            }
            return;
        }
        if (MovementActions.LocalMapObj_CheckAnimationFinished(mapObject) != 1)
        {
            return;
        }
        switch (moveState)
        {
            case MoveStateNone:
                break;
            case MoveStateMoving:
                if (playerMoveState == PlayerMoveStateNone)
                {
                    break;
                }
                if (playerMoveState == PlayerMoveStateEnd)
                {
                    PlayerAvatar_SetPlayerMoveState(playerAvatar, PlayerMoveStateNone);
                }
                else
                {
                    PlayerAvatar_SetPlayerMoveState(playerAvatar, PlayerMoveStateEnd);
                }
                break;
            case MoveStateTurning:
                if (playerMoveState == PlayerMoveStateNone)
                {
                    break;
                }
                if (playerMoveState == PlayerMoveStateEnd)
                {
                    PlayerAvatar_SetPlayerMoveState(playerAvatar, PlayerMoveStateNone);
                }
                else
                {
                    PlayerAvatar_SetPlayerMoveState(playerAvatar, PlayerMoveStateEnd);
                }
                break;
        }
    }

    // OFFSET 0x0205F56C
    public static void PlayerAvatar_ClearMoveState(PlayerAvatar playerAvatar)
    {
        PlayerAvatar_SetMoveState(playerAvatar, MoveStateNone);
        PlayerAvatar_SetPlayerMoveState(playerAvatar, PlayerMoveStateNone);
        PlayerAvatar_ClearSpeed(playerAvatar);
    }

    // OFFSET 0x0205F588
    public static int PlayerAvatar_CheckForceStopMovement(PlayerAvatar playerAvatar)
    {
        int moveState = PlayerAvatar_GetMoveState(playerAvatar);
        int playerMoveState = PlayerAvatar_GetPlayerMoveState(playerAvatar);
        if (moveState == MoveStateNone)
        {
            return 1;
        }
        if (moveState == MoveStateTurning)
        {
            return 1;
        }
        if (moveState == MoveStateMoving)
        {
            if (playerMoveState == PlayerMoveStateNone || playerMoveState == PlayerMoveStateEnd)
            {
                return 1;
            }
            MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
            if (MovementActions.LocalMapObj_IsAnimationSet(mapObject) == 1)
            {
                return 1;
            }
            if (MovementAction_IsWalkOnSpotSlow(MapObject.MapObject_GetMovementAction(mapObject)) == 1)
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0205F5E4
    public static void PlayerAvatar_ForceStopMovement(PlayerAvatar playerAvatar, int direction)
    {
        PlayerAvatar_SetMoveState(playerAvatar, MoveStateNone);
        PlayerAvatar_SetPlayerMoveState(playerAvatar, PlayerMoveStateNone);
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        MapObject.MapObject_TryFace(mapObject, direction);
        MapObject.MapObject_SetDrawState(mapObject, 0);
        MapObjectMove.MapObject_UpdateCoords(mapObject);
        MovementActions.MapObject_ClearMovementAction(mapObject);
        MovementActions.LocalMapObj_SetAnimationCode(mapObject, MovementActions.MovementAction_TurnActionTowardsDir(direction, ActionFace));
    }

    // OFFSET 0x0205F62C
    private static int PlayerAvatar_TryMoveFromCurTileBehavior(PlayerAvatar playerAvatar, int direction)
    {
        int tileMovement = PlayerAvatar_GetTileMovement(playerAvatar, direction);
        return PlayerAvatar_TryMoveTileBehavior(playerAvatar, tileMovement, direction);
    }

    // OFFSET 0x0205F644
    private static int PlayerAvatar_GetTileMovement(PlayerAvatar playerAvatar, int direction)
    {
        int tileBehavior = (int)MapObject.MapObject_GetCurrTileBehavior(PlayerAvatar_GetMapObject(playerAvatar));
        if (PlayerAvatar_IsOnCyclingRoadBridge(playerAvatar, tileBehavior, direction) == 1)
        {
            return TileMovementCyclingRoadDown;
        }
        if (PlayerAvatar_CheckIgnoreTileBehavior(playerAvatar) == 1)
        {
            return TileMovementNone;
        }
        int index = 0;
        while (true)
        {
            uint[] entry = RomData.Arm9U32(TileMovementTableAddress + (uint)(index * 8), 2);
            if (entry[0] == 0)
            {
                break;
            }
            if (TileBehaviorCheckFromAddress(entry[0])((byte)tileBehavior) == 1)
            {
                return (int)entry[1];
            }
            index++;
        }
        return TileMovementNone;
    }

    // OFFSET 0x0205F6A4
    private static int PlayerAvatar_TryMoveTileBehavior(PlayerAvatar playerAvatar, int tileMovement, int direction)
    {
        uint handler = RomData.Arm9U32(TileMoveHandlerTableAddress + (uint)(tileMovement * 4), 1)[0];
        if (TileMoveHandlerFromAddress(handler)(playerAvatar, direction) == 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205F6C4
    private static int PlayerAvatar_TileMove_None(PlayerAvatar playerAvatar, int direction)
    {
        PlayerAvatar_ClearTileForcedMovement(playerAvatar);
        return 0;
    }

    // OFFSET 0x0205F6D0
    private static int PlayerAvatar_TileMove_Ice(PlayerAvatar playerAvatar, int direction)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        int movingDirection = MapObject.MapObject_GetMovingDir(mapObject);
        int heightChange = PlayerAvatar_CheckIceHeightChange(playerAvatar, movingDirection);
        int collision = PlayerAvatar_CheckCollision(playerAvatar, mapObject, movingDirection);
        PlayerAvatar_SetForceMovement(playerAvatar, 1);
        if (collision != 0)
        {
            PlayerAvatar_ClearTileForcedMovement(playerAvatar);
            if (heightChange != HeightChangeUp)
            {
                PlayerAvatar_SetIgnoreTileBehavior(playerAvatar, 1);
                PlayerAvatar_SetMoveState(playerAvatar, MoveStateNone);
                return 0;
            }
            movingDirection = MapObjectMove.Direction_GetOpposite(movingDirection);
            if (PlayerAvatar_CheckCollision(playerAvatar, mapObject, movingDirection) != 0)
            {
                return 0;
            }
            MapObject.MapObject_SetStatusFlagOn(mapObject, MapObjectStatusForcedMovement);
            PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionWalkSlow), 3);
            PlayerAvatar_SetIgnoreTileBehavior(playerAvatar, 1);
            PlayerAvatar_SetForceMovement(playerAvatar, 1);
            PlayerAvatar_SetFaceDirection(playerAvatar, DirectionNone, DirectionNone);
            PlayerAvatar_SetMoveState(playerAvatar, MoveStateMoving);
            return 1;
        }
        if (PlayerAvatar_UpdateIceSpeedFromHeightChange(playerAvatar, heightChange) == 0)
        {
            PlayerAvatar_ClearTileForcedMovement(playerAvatar);
            movingDirection = MapObjectMove.Direction_GetOpposite(movingDirection);
            if (PlayerAvatar_CheckCollision(playerAvatar, mapObject, movingDirection) != 0)
            {
                return 0;
            }
            MapObject.MapObject_SetStatusFlagOn(mapObject, MapObjectStatusForcedMovement);
            PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionWalkSlow), 3);
            PlayerAvatar_SetIgnoreTileBehavior(playerAvatar, 1);
            PlayerAvatar_SetForceMovement(playerAvatar, 1);
            PlayerAvatar_SetFaceDirection(playerAvatar, DirectionNone, DirectionNone);
        }
        else
        {
            MapObject.MapObject_SetStatusFlagOn(mapObject, MapObjectStatusForcedMovement);
            PlayerAvatar_SetIceMovement(playerAvatar, movingDirection);
        }
        PlayerAvatar_SetMoveState(playerAvatar, MoveStateMoving);
        return 1;
    }

    // OFFSET 0x0205F808
    private static int PlayerAvatar_TileMove_BikeSlope(PlayerAvatar playerAvatar, int direction)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        int movingDirection = MapObject.MapObject_GetMovingDir(mapObject);
        SoundPlayback.Sound_PlayEffect(SeqBikeSlope);
        if (movingDirection == DirectionNorth)
        {
            if (PlayerAvatar_GetPlayerState(playerAvatar) == StateCycling && PlayerAvatar_GetSpeed(playerAvatar) >= BikeMaxSpeed && direction == DirectionNorth)
            {
                PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionBikeSlopeSlide), 6);
                PlayerAvatar_SetMoveState(playerAvatar, MoveStateMoving);
                return 1;
            }
            movingDirection = MapObjectMove.Direction_GetOpposite(movingDirection);
            if (PlayerAvatar_CheckCollision(playerAvatar, mapObject, movingDirection) != 0)
            {
                return 0;
            }
            MapObject.MapObject_SetStatusFlagOn(mapObject, MapObjectStatusForcedMovement);
            PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionWalkSlow), 3);
            PlayerAvatar_ClearSpeed(playerAvatar);
            PlayerAvatar_SetForceMovement(playerAvatar, 1);
            PlayerAvatar_SetMoveState(playerAvatar, MoveStateMoving);
            PlayerAvatar_SetMoveThroughMudFlag(playerAvatar, 1);
            PlayerAvatar_SetFaceDirection(playerAvatar, DirectionNone, DirectionNone);
            return 1;
        }
        if (movingDirection == DirectionSouth)
        {
            if (PlayerAvatar_CheckMoveThroughMudFlag(playerAvatar) == 0)
            {
                PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionBikeSlopeSlide), 6);
                PlayerAvatar_SetMoveState(playerAvatar, MoveStateMoving);
                if (PlayerAvatar_GetCyclingGear(playerAvatar) == 1)
                {
                    PlayerAvatar_SetSpeed(playerAvatar, BikeMaxSpeed);
                }
                PlayerAvatar_SetFaceDirection(playerAvatar, DirectionNone, DirectionNone);
                return 1;
            }
            MapObject.MapObject_SetStatusFlagOn(mapObject, MapObjectStatusForcedMovement);
            PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionWalkSlow), 3);
            PlayerAvatar_SetForceMovement(playerAvatar, 1);
            PlayerAvatar_SetMoveState(playerAvatar, MoveStateMoving);
            PlayerAvatar_SetMoveThroughMudFlag(playerAvatar, 1);
            PlayerAvatar_SetFaceDirection(playerAvatar, DirectionNone, DirectionNone);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205F95C
    private static int PlayerAvatar_TileMove_BikeRampEast(PlayerAvatar playerAvatar, int direction)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        MapObject.MapObject_GetMovingDir(mapObject);
        if (PlayerAvatar_GetCyclingGear(playerAvatar) == 1)
        {
            SoundPlayback.Sound_PlayEffect(SeqBikeRamp);
            PlayerAvatar_SetMovement(playerAvatar, mapObject, ActionRampEastFast, 2);
        }
        else
        {
            PlayerAvatar_SetMovement(playerAvatar, mapObject, ActionRampEastSlow, 3);
        }
        PlayerAvatar_SetForceMovement(playerAvatar, 1);
        PlayerAvatar_SetForceMovementRetainSpeed(playerAvatar, 1);
        return 1;
    }

    // OFFSET 0x0205F9AC
    private static int PlayerAvatar_TileMove_BikeRampWest(PlayerAvatar playerAvatar, int direction)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        MapObject.MapObject_GetMovingDir(mapObject);
        if (PlayerAvatar_GetCyclingGear(playerAvatar) == 1)
        {
            SoundPlayback.Sound_PlayEffect(SeqBikeRamp);
            PlayerAvatar_SetMovement(playerAvatar, mapObject, ActionRampWestFast, 2);
        }
        else
        {
            PlayerAvatar_SetMovement(playerAvatar, mapObject, ActionRampWestSlow, 2);
        }
        PlayerAvatar_SetForceMovement(playerAvatar, 1);
        PlayerAvatar_SetForceMovementRetainSpeed(playerAvatar, 1);
        return 1;
    }

    // OFFSET 0x0205F9FC
    private static int PlayerAvatar_TileMove_CyclingRoadDown(PlayerAvatar playerAvatar, int direction)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        if (PlayerAvatar_CheckCollision(playerAvatar, mapObject, DirectionSouth) == 0)
        {
            PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(DirectionSouth, ActionWalkFaster), 6);
            PlayerAvatar_SetMoveState(playerAvatar, MoveStateMoving);
            PlayerAvatar_SetSpeed(playerAvatar, BikeMaxSpeed);
            return 1;
        }
        PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(DirectionSouth, 1), 1);
        PlayerAvatar_ClearSpeed(playerAvatar);
        PlayerAvatar_SetMoveState(playerAvatar, MoveStateNone);
        PlayerAvatar_SetBikeBrake(playerAvatar, 0);
        return 1;
    }

    // OFFSET 0x0205FA6C
    private static void PlayerAvatar_ClearTileForcedMovement(PlayerAvatar playerAvatar)
    {
        if (PlayerAvatar_CheckForcedMovement(playerAvatar) == 1)
        {
            MapObject.MapObject_SetStatusFlagOff(PlayerAvatar_GetMapObject(playerAvatar), MapObjectStatusForcedMovement);
            if (PlayerAvatar_CheckForcedMovementRetainSpeed(playerAvatar) == 0)
            {
                PlayerAvatar_ClearSpeed(playerAvatar);
            }
            PlayerAvatar_SetForceMovement(playerAvatar, 0);
            PlayerAvatar_SetForceMovementRetainSpeed(playerAvatar, 0);
            PlayerAvatar_SetMoveThroughMudFlag(playerAvatar, 0);
        }
    }

    // OFFSET 0x0205FAB0
    private static int PlayerAvatar_CheckIceHeightChange(PlayerAvatar playerAvatar, int direction)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        VecFx32 nextPosition = position;
        MapObjectMove.VecFx32_StepDirection(direction, ref nextPosition, IceHeightProbeDistance);
        if (MapObjectMove.MapObject_RecalculatePositionHeight(fieldSystem, ref nextPosition) == 0 || position.Y == nextPosition.Y)
        {
            return HeightChangeNone;
        }
        if (position.Y > nextPosition.Y)
        {
            return HeightChangeDown;
        }
        return HeightChangeUp;
    }

    // OFFSET 0x0205FB10
    private static int PlayerAvatar_UpdateIceSpeedFromHeightChange(PlayerAvatar playerAvatar, int heightChange)
    {
        int speed = PlayerAvatar_GetSpeed(playerAvatar);
        if (heightChange == HeightChangeUp)
        {
            speed--;
            if (speed < 0)
            {
                return 0;
            }
        }
        else if (heightChange == HeightChangeDown)
        {
            speed++;
            if (speed > BikeMaxSpeed)
            {
                speed = BikeMaxSpeed;
            }
        }
        PlayerAvatar_SetSpeed(playerAvatar, speed);
        return 1;
    }

    // OFFSET 0x0205FB40
    private static void PlayerAvatar_SetIceMovement(PlayerAvatar playerAvatar, int direction)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        int action = ActionWalkFast;
        int duration = 5;
        switch (PlayerAvatar_GetSpeed(playerAvatar))
        {
            case 1:
                action = ActionBikeNormal;
                break;
            case 2:
                action = ActionWalkFaster;
                duration = 6;
                break;
            case 3:
                action = ActionWalkFaster;
                duration = 6;
                break;
        }
        PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(direction, action), duration);
    }

    // OFFSET 0x0205FB88
    private static void PlayerAvatar_SetMovementFromState(PlayerAvatar playerAvatar, object? landDataManager, int direction, ushort pressedKeys, ushort heldKeys)
    {
        int playerState = PlayerAvatar_GetPlayerState(playerAvatar);
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        switch (playerState)
        {
            case StateWalking:
            case StateSurfing:
                PlayerAvatar_SetMovement_Normal(playerAvatar, mapObject, landDataManager, direction, pressedKeys, heldKeys);
                break;
            case StateCycling:
                PlayerAvatar_SetMovement_Bike(playerAvatar, mapObject, landDataManager, direction, pressedKeys, heldKeys);
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
    }

    // OFFSET 0x0205FBE4
    private static void PlayerAvatar_SetMovement_Normal(PlayerAvatar playerAvatar, MapObject mapObject, object? landDataManager, int direction, ushort pressedKeys, ushort heldKeys)
    {
        switch (PlayerAvatar_UpdateMoveState2(playerAvatar, direction))
        {
            case MoveStateNone:
                PlayerAvatar_SetMovement_NotMoving(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
            case MoveStateMoving:
                PlayerAvatar_SetMovement_Moving(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
            case MoveStateTurning:
                PlayerAvatar_SetMovement_Turning(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
        }
    }

    // OFFSET 0x0205FC48
    private static int PlayerAvatar_UpdateMoveState2(PlayerAvatar playerAvatar, int direction)
    {
        int moveState = PlayerAvatar_UpdateMoveState(playerAvatar, direction);
        PlayerAvatar_SetMoveState(playerAvatar, PlayerAvatar_ValidateState(moveState));
        return moveState;
    }

    // OFFSET 0x0205FC64
    private static int PlayerAvatar_ValidateState(int moveState)
    {
        switch (moveState)
        {
            case MoveStateNone:
                return MoveStateNone;
            case MoveStateMoving:
                return MoveStateMoving;
            case MoveStateTurning:
                return MoveStateTurning;
        }
        ErrorHandling.GF_AssertFail();
        return MoveStateNone;
    }

    // OFFSET 0x0205FC88
    private static int MapObject_GetDeepSnowMovementAction(MapObject mapObject, int tileBehavior, int action)
    {
        if (TileBehavior.TileBehavior_IsDeepestSnow(tileBehavior) == 1)
        {
            return ActionWalkSlowest;
        }
        if (TileBehavior.TileBehavior_IsDeeperSnow(tileBehavior) == 1)
        {
            return ActionWalkSlow;
        }
        if (TileBehavior.TileBehavior_IsDeepSnow(tileBehavior) == 1)
        {
            action = ActionWalkNormal;
        }
        return action;
    }

    // OFFSET 0x0205FCBC
    private static int MapObject_DummyReturnMovementAction(MapObject mapObject, int tileBehavior, int action)
    {
        return action;
    }

    // OFFSET 0x0205FCC0
    private static int PlayerAvatar_UpdateMoveState(PlayerAvatar playerAvatar, int direction)
    {
        if (direction == DirectionNone)
        {
            PlayerAvatar_SetMoveState(playerAvatar, MoveStateNone);
            return MoveStateNone;
        }
        int facingDirection = PlayerAvatar_GetFacingDir(playerAvatar);
        switch (PlayerAvatar_MapDistortionState(playerAvatar))
        {
            case DistortionStateNone:
            case DistortionStateActive:
            case DistortionStateFloor:
                break;
            default:
                facingDirection = PlayerAvatar_GetMovingDir(playerAvatar);
                break;
        }
        if (facingDirection != direction && PlayerAvatar_GetMoveState(playerAvatar) != MoveStateMoving)
        {
            PlayerAvatar_SetMoveState(playerAvatar, MoveStateTurning);
            return MoveStateTurning;
        }
        PlayerAvatar_SetMoveState(playerAvatar, MoveStateMoving);
        return MoveStateMoving;
    }

    // OFFSET 0x0205FD20
    private static void PlayerAvatar_SetMovement_NotMoving(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(MapObject.MapObject_GetFacingDir(mapObject), ActionFace), 1);
    }

    // OFFSET 0x0205FD40
    private static void PlayerAvatar_SetMovement_Moving(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        switch (PlayerAvatar_MapDistortionState(playerAvatar))
        {
            case DistortionStateFloor:
                PlayerAvatar_SetMovement_DistortionFloor(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
            case DistortionStateWestWall:
                PlayerAvatar_SetMovement_DistortionWestWall(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
            case DistortionStateEastWall:
                PlayerAvatar_SetMovement_DistortionEastWall(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
            case DistortionStateCeiling:
                PlayerAvatar_SetMovement_DistortionCeiling(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
            default:
                PlayerAvatar_SetMovement_NormalOverworld(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
        }
    }

    // OFFSET 0x0205FDC8
    private static void PlayerAvatar_SetMovement_NormalOverworld(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        int collision = PlayerAvatar_CheckCollision(playerAvatar, mapObject, direction);
        int action;
        int duration;
        if (PlayerAvatar_GetPlayerState(playerAvatar) != StateSurfing)
        {
            duration = 4;
            if ((collision & CollisionJump) != 0)
            {
                action = ActionJump;
                duration = 3;
            }
            else if ((collision & CollisionJumpTwice) != 0)
            {
                action = ActionJumpTwice;
                duration = 2;
            }
            else if (collision != 0)
            {
                action = ActionWalkOnSpotSlow;
                duration = 1;
                if ((collision & CollisionWarp) == 0)
                {
                    SoundPlayback.Sound_PlayEffect(SeqBump);
                }
                MapObject.MapObject_Turn(mapObject, direction);
            }
            else
            {
                action = ActionWalkNormal;
                if (PlayerData_HasRunningShoes(PlayerAvatar_PlayerData(playerAvatar)) == 1 && PlayerAvatar_IsRunButtonHeld(playerAvatar, heldKeys) == 1)
                {
                    action = ActionRun;
                    duration = 5;
                }
                action = MapObject_GetDeepSnowMovementAction(mapObject, (byte)MapObject.MapObject_GetCurrTileBehavior(mapObject), action);
                PlayerAvatar_IncrementStepsRecord(playerAvatar);
                PlayerAvatar_StartStep(playerAvatar);
            }
        }
        else if (collision == 0 || collision == CollisionWater)
        {
            duration = 5;
            action = MapObject_DummyReturnMovementAction(mapObject, (byte)MapObject.MapObject_GetCurrTileBehavior(mapObject), ActionWalkFast);
            PlayerAvatar_IncrementStepsRecord(playerAvatar);
            PlayerAvatar_StartStep(playerAvatar);
        }
        else
        {
            action = ActionWalkOnSpotSlow;
            duration = 1;
            if ((collision & CollisionWarp) == 0)
            {
                SoundPlayback.Sound_PlayEffect(SeqBump);
            }
            MapObject.MapObject_Turn(mapObject, direction);
        }
        PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(direction, action), duration);
    }

    // OFFSET 0x0205FECC
    private static void PlayerAvatar_SetMovement_DistortionFloor(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        int collision = PlayerAvatar_CheckDistortionCollision(playerAvatar, mapObject, direction);
        if ((collision & ~CollisionJumpTwice) != 0)
        {
            FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
            int x = MapObject.MapObject_GetX(mapObject);
            int y = MapObject.MapObject_GetY(mapObject);
            int z = MapObject.MapObject_GetZ(mapObject);
            y /= 2;
            PlayerAvatar_GetStepDistortionWorldPos(playerAvatar, direction, ref x, ref y, ref z);
            if (ov9_02250FBC(fieldSystem, x, y, z) == 0 && ov9_02250FD8(fieldSystem, x, y, z) == 1)
            {
                ov9_02251000(fieldSystem, x, y, z);
                collision = PlayerAvatar_CheckDistortionCollision(playerAvatar, mapObject, direction);
            }
        }
        int action;
        int duration;
        if (PlayerAvatar_GetPlayerState(playerAvatar) != StateSurfing)
        {
            if ((collision & CollisionJumpTwice) != 0)
            {
                action = ActionJumpTwice;
                duration = 2;
                PlayerAvatar_IncrementStepsRecord(playerAvatar);
                PlayerAvatar_StartStep(playerAvatar);
            }
            else if (collision != 0)
            {
                action = ActionWalkOnSpotSlow;
                duration = 1;
                SoundPlayback.Sound_PlayEffect(SeqBump);
                MapObject.MapObject_Turn(mapObject, direction);
            }
            else
            {
                action = ActionWalkNormal;
                duration = 4;
                if (PlayerData_HasRunningShoes(PlayerAvatar_PlayerData(playerAvatar)) == 1 && PlayerAvatar_IsRunButtonHeld(playerAvatar, heldKeys) == 1)
                {
                    action = ActionRun;
                    duration = 5;
                }
                PlayerAvatar_IncrementStepsRecord(playerAvatar);
                PlayerAvatar_StartStep(playerAvatar);
            }
        }
        else if (collision == 0 || collision == CollisionWater)
        {
            duration = 5;
            action = ActionWalkFast;
            PlayerAvatar_IncrementStepsRecord(playerAvatar);
            PlayerAvatar_StartStep(playerAvatar);
        }
        else
        {
            action = ActionWalkOnSpotSlow;
            duration = 1;
            SoundPlayback.Sound_PlayEffect(SeqBump);
            MapObject.MapObject_Turn(mapObject, direction);
        }
        PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(direction, action), duration);
    }

    // OFFSET 0x0206000C
    private static void PlayerAvatar_UpdateMovement_NormalDistortion(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys, uint[] walkActions, uint[] runActions, uint[] surfActions, uint[] bumpActions, uint[] turnDirections)
    {
        int collision = PlayerAvatar_CheckDistortionCollision(playerAvatar, mapObject, direction);
        int action;
        int duration;
        if (PlayerAvatar_GetPlayerState(playerAvatar) != StateSurfing)
        {
            if (collision != 0)
            {
                action = (int)bumpActions[direction];
                duration = 1;
                SoundPlayback.Sound_PlayEffect(SeqBump);
                MapObject.MapObject_Turn(mapObject, (int)turnDirections[direction]);
            }
            else
            {
                action = (int)walkActions[direction];
                duration = 4;
                if (PlayerData_HasRunningShoes(PlayerAvatar_PlayerData(playerAvatar)) == 1 && PlayerAvatar_IsRunButtonHeld(playerAvatar, heldKeys) == 1)
                {
                    duration = 5;
                    action = (int)runActions[direction];
                }
                PlayerAvatar_IncrementStepsRecord(playerAvatar);
            }
        }
        else if (collision == 0 || collision == CollisionWater)
        {
            action = (int)surfActions[direction];
            duration = 4;
            PlayerAvatar_IncrementStepsRecord(playerAvatar);
            PlayerAvatar_StartStep(playerAvatar);
        }
        else
        {
            action = (int)bumpActions[direction];
            duration = 1;
            SoundPlayback.Sound_PlayEffect(SeqBump);
            MapObject.MapObject_Turn(mapObject, (int)turnDirections[direction]);
        }
        PlayerAvatar_SetMovement(playerAvatar, mapObject, action, duration);
    }

    // OFFSET 0x020600CC
    private static void PlayerAvatar_SetMovement_DistortionWestWall(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        uint[] walkActions = RomData.Arm9U32(WestWallWalkActionsAddress, 4);
        uint[] runActions = RomData.Arm9U32(WestWallRunActionsAddress, 4);
        uint[] surfActions = RomData.Arm9U32(WestWallSurfActionsAddress, 4);
        uint[] bumpActions = RomData.Arm9U32(WestWallBumpActionsAddress, 4);
        uint[] turnDirections = RomData.Arm9U32(WestWallTurnDirectionsAddress, 4);
        PlayerAvatar_UpdateMovement_NormalDistortion(playerAvatar, mapObject, direction, pressedKeys, heldKeys, walkActions, runActions, surfActions, bumpActions, turnDirections);
    }

    // OFFSET 0x02060150
    private static void PlayerAvatar_SetMovement_DistortionEastWall(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        uint[] walkActions = RomData.Arm9U32(EastWallWalkActionsAddress, 4);
        uint[] runActions = RomData.Arm9U32(EastWallRunActionsAddress, 4);
        uint[] surfActions = RomData.Arm9U32(EastWallSurfActionsAddress, 4);
        uint[] bumpActions = RomData.Arm9U32(EastWallBumpActionsAddress, 4);
        uint[] turnDirections = RomData.Arm9U32(EastWallTurnDirectionsAddress, 4);
        PlayerAvatar_UpdateMovement_NormalDistortion(playerAvatar, mapObject, direction, pressedKeys, heldKeys, walkActions, runActions, surfActions, bumpActions, turnDirections);
    }

    // OFFSET 0x020601D4
    private static void PlayerAvatar_SetMovement_DistortionCeiling(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        uint[] walkActions = RomData.Arm9U32(CeilingWalkActionsAddress, 4);
        uint[] runActions = RomData.Arm9U32(CeilingRunActionsAddress, 4);
        uint[] surfActions = RomData.Arm9U32(CeilingSurfActionsAddress, 4);
        uint[] bumpActions = RomData.Arm9U32(CeilingBumpActionsAddress, 4);
        uint[] turnDirections = RomData.Arm9U32(CeilingTurnDirectionsAddress, 4);
        PlayerAvatar_UpdateMovement_NormalDistortion(playerAvatar, mapObject, direction, pressedKeys, heldKeys, walkActions, runActions, surfActions, bumpActions, turnDirections);
    }

    // OFFSET 0x02060258
    private static void PlayerAvatar_SetMovement_Turning(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        int actionDirection = direction;
        switch (PlayerAvatar_MapDistortionState(playerAvatar))
        {
            case DistortionStateWestWall:
                actionDirection = (int)RomData.Arm9U32(WestWallTurnActionDirectionsAddress, 4)[direction];
                break;
            case DistortionStateEastWall:
                actionDirection = (int)RomData.Arm9U32(EastWallTurnActionDirectionsAddress, 4)[direction];
                break;
            case DistortionStateCeiling:
                actionDirection = (int)RomData.Arm9U32(CeilingTurnActionDirectionsAddress, 4)[direction];
                break;
        }
        PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(actionDirection, ActionTurn), 1);
        MapObject.MapObject_Turn(mapObject, direction);
    }

    // OFFSET 0x020602DC
    private static void PlayerAvatar_SetMovement_Bike(PlayerAvatar playerAvatar, MapObject mapObject, object? landDataManager, int direction, ushort pressedKeys, ushort heldKeys)
    {
        if (PlayerAvatar_GetCyclingGear(playerAvatar) == 1)
        {
            PlayerAvatar_SetMovement_BikeFourthGear(playerAvatar, mapObject, landDataManager, direction, pressedKeys, heldKeys);
        }
        else
        {
            PlayerAvatar_SetMovement_BikeThirdGear(playerAvatar, mapObject, landDataManager, direction, pressedKeys, heldKeys);
        }
    }

    // OFFSET 0x02060324
    private static void PlayerAvatar_TryCyclingGearChange(PlayerAvatar playerAvatar, ushort pressedKeys)
    {
        if (PlayerAvatar_GetPlayerState(playerAvatar) != StateCycling || (pressedKeys & KeyB) == 0)
        {
            return;
        }
        int tileBehavior = (int)MapObject.MapObject_GetCurrTileBehavior(PlayerAvatar_GetMapObject(playerAvatar));
        if (TileBehavior.TileBehavior_IsBikeRampEastward((byte)tileBehavior) != 0)
        {
            return;
        }
        if (TileBehavior.TileBehavior_IsBikeRampWestward((byte)tileBehavior) != 0)
        {
            return;
        }
        int cyclingGear = 1;
        if (PlayerAvatar_GetCyclingGear(playerAvatar) == 1)
        {
            cyclingGear = 0;
        }
        PlayerAvatar_SetCyclingGear(playerAvatar, cyclingGear);
        if (cyclingGear == 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqGearLow);
        }
        else
        {
            SoundPlayback.Sound_PlayEffect(SeqGearHigh);
        }
    }

    // OFFSET 0x02060390
    public static int PlayerAvatar_GetMovementActionFromSpeed(PlayerAvatar playerAvatar, int direction)
    {
        int action = ActionBikeSlow;
        switch (PlayerAvatar_GetSpeed(playerAvatar))
        {
            case 1:
                action = ActionWalkFast;
                break;
            case 2:
                action = ActionBikeNormal;
                break;
            case 3:
                action = ActionWalkFaster;
                break;
        }
        return MovementActions.MovementAction_TurnActionTowardsDir(direction, action);
    }

    // OFFSET 0x020603BC
    private static int PlayerAvatar_AccelerateBike(PlayerAvatar playerAvatar)
    {
        int speed = PlayerAvatar_AddMoveSpeed(playerAvatar, 1, BikeMaxSpeed);
        if (PlayerAvatar_CheckBikeBrake(playerAvatar) == 0 && speed >= 2)
        {
            PlayerAvatar_SetBikeBrake(playerAvatar, 1);
        }
        if (speed == BikeMaxSpeed)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020603EC
    private static int PlayerAvatar_DecelerateBike(PlayerAvatar playerAvatar)
    {
        int stillMoving = 1;
        int speed = PlayerAvatar_GetSpeed(playerAvatar) - 1;
        if (speed < 0)
        {
            speed = 0;
            stillMoving = 0;
        }
        PlayerAvatar_SetSpeed(playerAvatar, speed);
        if (PlayerAvatar_CheckBikeBrake(playerAvatar) == 1 && speed == 0)
        {
            PlayerAvatar_SetBikeBrake(playerAvatar, 0);
        }
        return stillMoving;
    }

    // OFFSET 0x02060420
    private static void PlayerAvatar_SetMovement_BikeThirdGear(PlayerAvatar playerAvatar, MapObject mapObject, object? landDataManager, int direction, ushort pressedKeys, ushort heldKeys)
    {
        switch (PlayerAvatar_UpdateBikeStateThirdGear(playerAvatar, direction))
        {
            case BikeStateStop:
                PlayerAvatar_SetMovement_BikeThirdGearStop(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
            case BikeStateMoving:
                PlayerAvatar_SetMovement_BikeThirdGearMoving(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
            case BikeStateTurning:
                PlayerAvatar_SetMovement_BikeThirdGearTurning(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
            case BikeStateBrake:
                PlayerAvatar_SetMovement_BikeThirdGearBrake(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
        }
    }

    // OFFSET 0x02060494
    private static int PlayerAvatar_UpdateBikeStateThirdGear(PlayerAvatar playerAvatar, int direction)
    {
        int bikeState = PlayerAvatar_GetBikeStateThirdGear(playerAvatar, direction);
        PlayerAvatar_SetMoveState(playerAvatar, PlayerAvatarBikeState_GetMoveStateThirdGear(bikeState));
        return bikeState;
    }

    // OFFSET 0x020604B0
    private static int PlayerAvatarBikeState_GetMoveStateThirdGear(int bikeState)
    {
        switch (bikeState)
        {
            case BikeStateStop:
                return MoveStateNone;
            case BikeStateMoving:
                return MoveStateMoving;
            case BikeStateTurning:
                return MoveStateTurning;
            case BikeStateBrake:
                return MoveStateMoving;
        }
        ErrorHandling.GF_AssertFail();
        return MoveStateNone;
    }

    // OFFSET 0x020604E4
    private static int PlayerAvatar_GetBikeStateThirdGear(PlayerAvatar playerAvatar, int direction)
    {
        if (direction == DirectionNone)
        {
            if (PlayerAvatar_CheckBikeBrake(playerAvatar) == 1)
            {
                PlayerAvatar_SetMoveState(playerAvatar, MoveStateMoving);
                return BikeStateBrake;
            }
            PlayerAvatar_SetMoveState(playerAvatar, MoveStateNone);
            return BikeStateStop;
        }
        int facingDirection = PlayerAvatar_GetFacingDir(playerAvatar);
        PlayerAvatar_SetBikeBrake(playerAvatar, 0);
        if (facingDirection != direction && PlayerAvatar_GetMoveState(playerAvatar) != MoveStateMoving)
        {
            PlayerAvatar_SetMoveState(playerAvatar, MoveStateTurning);
            return BikeStateTurning;
        }
        PlayerAvatar_SetMoveState(playerAvatar, MoveStateMoving);
        return BikeStateMoving;
    }

    // OFFSET 0x02060548
    private static void PlayerAvatar_SetMovement_BikeThirdGearStop(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(MapObject.MapObject_GetFacingDir(mapObject), ActionFace), 1);
        PlayerAvatar_ClearSpeed(playerAvatar);
    }

    // OFFSET 0x02060570
    private static void PlayerAvatar_SetMovement_BikeThirdGearMoving(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        int collision = PlayerAvatar_CheckCollision(playerAvatar, mapObject, direction);
        int action;
        int duration;
        if ((collision & CollisionJump) != 0)
        {
            action = ActionJump;
            duration = 3;
            PlayerAvatar_IncrementStepsRecord(playerAvatar);
            PlayerAvatar_StartStep(playerAvatar);
        }
        else if ((collision & CollisionJumpTwice) != 0)
        {
            action = ActionJumpTwice;
            duration = 2;
            PlayerAvatar_IncrementStepsRecord(playerAvatar);
            PlayerAvatar_StartStep(playerAvatar);
        }
        else if ((collision & CollisionBikeRamp) != 0)
        {
            action = ActionBikeNormal;
            duration = 5;
            PlayerAvatar_IncrementStepsRecord(playerAvatar);
            PlayerAvatar_StartStep(playerAvatar);
            PlayerAvatar_SetForceMovement(playerAvatar, 1);
        }
        else if ((collision & CollisionBikeBridge) != 0)
        {
            action = ActionFace;
            duration = 1;
            direction = MapObject.MapObject_GetMovingDir(mapObject);
            MapObject.MapObject_Turn(mapObject, direction);
            PlayerAvatar_ClearSpeed(playerAvatar);
            PlayerAvatar_SetFaceDirection(playerAvatar, DirectionNone, DirectionNone);
        }
        else if (collision != 0)
        {
            if (PlayerAvatar_IsOnBikeBridgeWrongDirection(playerAvatar, mapObject, direction) == 0)
            {
                action = ActionWalkOnSpotSlow;
                duration = 1;
                if ((collision & CollisionWarp) == 0)
                {
                    SoundPlayback.Sound_PlayEffect(SeqBump);
                }
                MapObject.MapObject_Turn(mapObject, direction);
                PlayerAvatar_ClearSpeed(playerAvatar);
            }
            else
            {
                action = ActionFace;
                duration = 1;
                direction = MapObject.MapObject_GetMovingDir(mapObject);
                MapObject.MapObject_Turn(mapObject, direction);
                PlayerAvatar_ClearSpeed(playerAvatar);
                PlayerAvatar_SetFaceDirection(playerAvatar, DirectionNone, DirectionNone);
            }
        }
        else
        {
            action = ActionBikeNormal;
            duration = 5;
            PlayerAvatar_IncrementStepsRecord(playerAvatar);
            PlayerAvatar_StartStep(playerAvatar);
            PlayerAvatar_SetSpeed(playerAvatar, 2);
        }
        PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(direction, action), duration);
    }

    // OFFSET 0x02060688
    private static void PlayerAvatar_SetMovement_BikeThirdGearTurning(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        PlayerAvatar_ClearSpeed(playerAvatar);
        if (PlayerAvatar_IsOnBikeBridgeWrongDirection(playerAvatar, mapObject, direction) != 1)
        {
            PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(direction, ActionTurn), 1);
            MapObject.MapObject_Turn(mapObject, direction);
            PlayerAvatar_ClearSpeed(playerAvatar);
        }
    }

    // OFFSET 0x020606C8
    private static void PlayerAvatar_SetMovement_BikeThirdGearBrake(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        int stillMoving = PlayerAvatar_DecelerateBike(playerAvatar);
        int movingDirection = PlayerAvatar_GetMovingDir(playerAvatar);
        int collision = PlayerAvatar_CheckCollision(playerAvatar, mapObject, movingDirection);
        int action;
        int duration;
        if ((collision & CollisionJump) != 0)
        {
            action = MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionJump);
            duration = 3;
        }
        else if ((collision & CollisionJumpTwice) != 0)
        {
            action = MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionJumpTwice);
            duration = 2;
        }
        else if ((collision & CollisionBikeBridge) != 0)
        {
            action = MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionFace);
            duration = 1;
            PlayerAvatar_ClearSpeed(playerAvatar);
        }
        else if (collision != 0)
        {
            stillMoving = 0;
            action = MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionWalkOnSpotSlow);
            duration = 1;
            if ((collision & CollisionWarp) == 0)
            {
                SoundPlayback.Sound_PlayEffect(SeqBump);
            }
            MapObject.MapObject_Turn(mapObject, movingDirection);
            PlayerAvatar_ClearSpeed(playerAvatar);
        }
        else
        {
            action = PlayerAvatar_GetMovementActionFromSpeed(playerAvatar, movingDirection);
            duration = 5;
        }
        if (stillMoving == 0)
        {
            PlayerAvatar_SetBikeBrake(playerAvatar, 0);
        }
        PlayerAvatar_SetMovement(playerAvatar, mapObject, action, duration);
    }

    // OFFSET 0x0206078C
    private static void PlayerAvatar_SetMovement_BikeFourthGear(PlayerAvatar playerAvatar, MapObject mapObject, object? landDataManager, int direction, ushort pressedKeys, ushort heldKeys)
    {
        switch (PlayerAvatar_UpdateBikeStateFourthGear(playerAvatar, direction))
        {
            case BikeStateStop:
                PlayerAvatar_SetMovement_BikeFourthGearStop(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
            case BikeStateMoving:
                PlayerAvatar_SetMovement_BikeFourthGearMoving(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
            case BikeStateTurning:
                PlayerAvatar_SetMovement_BikeFourthGearTurning(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
            case BikeStateBrake:
                PlayerAvatar_SetMovement_BikeFourthGearBrake(playerAvatar, mapObject, direction, pressedKeys, heldKeys);
                break;
        }
    }

    // OFFSET 0x02060800
    private static int PlayerAvatar_UpdateBikeStateFourthGear(PlayerAvatar playerAvatar, int direction)
    {
        int bikeState = PlayerAvatar_GetBikeStateFourthGear(playerAvatar, direction);
        PlayerAvatar_SetMoveState(playerAvatar, PlayerAvatarBikeState_GetMoveStateFourthGear(bikeState));
        return bikeState;
    }

    // OFFSET 0x0206081C
    private static int PlayerAvatarBikeState_GetMoveStateFourthGear(int bikeState)
    {
        switch (bikeState)
        {
            case BikeStateStop:
                return MoveStateNone;
            case BikeStateMoving:
                return MoveStateMoving;
            case BikeStateTurning:
                return MoveStateTurning;
            case BikeStateBrake:
                return MoveStateMoving;
        }
        ErrorHandling.GF_AssertFail();
        return MoveStateNone;
    }

    // OFFSET 0x02060850
    private static int PlayerAvatar_GetBikeStateFourthGear(PlayerAvatar playerAvatar, int direction)
    {
        int speed = PlayerAvatar_GetSpeed(playerAvatar);
        if (direction == DirectionNone)
        {
            if (speed < 2)
            {
                PlayerAvatar_SetMoveState(playerAvatar, MoveStateNone);
                return BikeStateStop;
            }
            PlayerAvatar_SetMoveState(playerAvatar, MoveStateMoving);
            return BikeStateBrake;
        }
        if (PlayerAvatar_GetFacingDir(playerAvatar) != direction && PlayerAvatar_GetMoveState(playerAvatar) != MoveStateMoving)
        {
            if (speed < 2)
            {
                PlayerAvatar_SetMoveState(playerAvatar, MoveStateTurning);
                return BikeStateTurning;
            }
            PlayerAvatar_SetMoveState(playerAvatar, MoveStateMoving);
            return BikeStateBrake;
        }
        PlayerAvatar_SetMoveState(playerAvatar, MoveStateMoving);
        return BikeStateMoving;
    }

    // OFFSET 0x020608BC
    private static void PlayerAvatar_SetMovement_BikeFourthGearStop(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(MapObject.MapObject_GetFacingDir(mapObject), ActionFace), 1);
        PlayerAvatar_ClearSpeed(playerAvatar);
    }

    // OFFSET 0x020608E4
    private static void PlayerAvatar_SetMovement_BikeFourthGearMoving(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        int collision = PlayerAvatar_CheckCollision(playerAvatar, mapObject, direction);
        int action;
        int duration;
        if ((collision & CollisionJump) != 0)
        {
            action = MovementActions.MovementAction_TurnActionTowardsDir(direction, ActionJump);
            duration = 3;
            PlayerAvatar_AccelerateBike(playerAvatar);
            PlayerAvatar_IncrementStepsRecord(playerAvatar);
            PlayerAvatar_StartStep(playerAvatar);
        }
        else if ((collision & CollisionJumpTwice) != 0)
        {
            action = MovementActions.MovementAction_TurnActionTowardsDir(direction, ActionJumpTwice);
            duration = 2;
            PlayerAvatar_AccelerateBike(playerAvatar);
            PlayerAvatar_IncrementStepsRecord(playerAvatar);
            PlayerAvatar_StartStep(playerAvatar);
        }
        else if ((collision & CollisionBikeRamp) != 0)
        {
            if (PlayerAvatar_GetSpeed(playerAvatar) >= BikeMaxSpeed)
            {
                action = PlayerAvatar_GetMovementActionFromSpeed(playerAvatar, direction);
                duration = 5;
                PlayerAvatar_StartStep(playerAvatar);
                PlayerAvatar_SetForceMovement(playerAvatar, 1);
            }
            else
            {
                action = MovementActions.MovementAction_TurnActionTowardsDir(direction, ActionWalkOnSpotSlow);
                duration = 1;
                if ((collision & CollisionWarp) == 0)
                {
                    SoundPlayback.Sound_PlayEffect(SeqBump);
                }
                MapObject.MapObject_Turn(mapObject, direction);
                PlayerAvatar_ClearSpeed(playerAvatar);
            }
        }
        else if ((collision & CollisionBikeBridge) != 0)
        {
            int movingDirection = MapObject.MapObject_GetMovingDir(mapObject);
            action = MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionFace);
            duration = 1;
            MapObject.MapObject_Turn(mapObject, movingDirection);
            PlayerAvatar_ClearSpeed(playerAvatar);
            PlayerAvatar_SetFaceDirection(playerAvatar, DirectionNone, DirectionNone);
        }
        else if (collision != 0)
        {
            if (PlayerAvatar_IsOnBikeBridgeWrongDirection(playerAvatar, mapObject, direction) == 0)
            {
                action = MovementActions.MovementAction_TurnActionTowardsDir(direction, ActionWalkOnSpotSlow);
                duration = 1;
                if ((collision & CollisionWarp) == 0)
                {
                    SoundPlayback.Sound_PlayEffect(SeqBump);
                }
                MapObject.MapObject_Turn(mapObject, direction);
                PlayerAvatar_ClearSpeed(playerAvatar);
            }
            else
            {
                duration = 1;
                int movingDirection = MapObject.MapObject_GetMovingDir(mapObject);
                action = MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionFace);
                MapObject.MapObject_Turn(mapObject, movingDirection);
                PlayerAvatar_ClearSpeed(playerAvatar);
                PlayerAvatar_SetFaceDirection(playerAvatar, DirectionNone, DirectionNone);
            }
        }
        else
        {
            duration = 5;
            action = PlayerAvatar_GetMovementActionFromSpeed(playerAvatar, direction);
            PlayerAvatar_AccelerateBike(playerAvatar);
            PlayerAvatar_IncrementStepsRecord(playerAvatar);
            PlayerAvatar_StartStep(playerAvatar);
        }
        PlayerAvatar_SetMovement(playerAvatar, mapObject, action, duration);
    }

    // OFFSET 0x02060A60
    private static void PlayerAvatar_SetMovement_BikeFourthGearTurning(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        PlayerAvatar_ClearSpeed(playerAvatar);
        if (PlayerAvatar_IsOnBikeBridgeWrongDirection(playerAvatar, mapObject, direction) != 1)
        {
            PlayerAvatar_SetMovement(playerAvatar, mapObject, MovementActions.MovementAction_TurnActionTowardsDir(direction, ActionTurn), 1);
            MapObject.MapObject_Turn(mapObject, direction);
            PlayerAvatar_ClearSpeed(playerAvatar);
        }
    }

    // OFFSET 0x02060AA0
    private static void PlayerAvatar_SetMovement_BikeFourthGearBrake(PlayerAvatar playerAvatar, MapObject mapObject, int direction, ushort pressedKeys, ushort heldKeys)
    {
        int stillMoving = PlayerAvatar_DecelerateBike(playerAvatar);
        int movingDirection = PlayerAvatar_GetMovingDir(playerAvatar);
        int collision = PlayerAvatar_CheckCollision(playerAvatar, mapObject, movingDirection);
        int action;
        int duration;
        if ((collision & CollisionJump) != 0)
        {
            action = MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionJump);
            duration = 3;
        }
        else if ((collision & CollisionJumpTwice) != 0)
        {
            action = MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionJumpTwice);
            duration = 2;
        }
        else if ((collision & CollisionBikeBridge) != 0)
        {
            action = MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionFace);
            duration = 1;
            PlayerAvatar_ClearSpeed(playerAvatar);
        }
        else if (collision != 0)
        {
            stillMoving = 0;
            duration = 1;
            action = MovementActions.MovementAction_TurnActionTowardsDir(movingDirection, ActionWalkOnSpotSlow);
            if ((collision & CollisionWarp) == 0)
            {
                SoundPlayback.Sound_PlayEffect(SeqBump);
            }
            MapObject.MapObject_Turn(mapObject, movingDirection);
            PlayerAvatar_ClearSpeed(playerAvatar);
        }
        else
        {
            action = PlayerAvatar_GetMovementActionFromSpeed(playerAvatar, movingDirection);
            duration = 5;
        }
        if (stillMoving == 0)
        {
            PlayerAvatar_SetBikeBrake(playerAvatar, 0);
        }
        PlayerAvatar_SetMovement(playerAvatar, mapObject, action, duration);
    }

    // OFFSET 0x02060B64
    private static void PlayerAvatar_SetMovement(PlayerAvatar playerAvatar, MapObject mapObject, int movementAction, int speed)
    {
        PlayerAvatar_SetMovementActionAndSpeed(playerAvatar, (uint)movementAction, speed);
        MovementActions.LocalMapObj_SetAnimationCode(mapObject, movementAction);
    }

    // OFFSET 0x02060B7C
    public static int PlayerAvatar_CheckCollision(PlayerAvatar playerAvatar, MapObject mapObject, int direction)
    {
        int collision = 0;
        int mapObjectCollision = PlayerAvatar_CheckMapObjCollision(playerAvatar, mapObject, direction);
        if ((mapObjectCollision & (MapObjectCollisionTerrain | MapObjectCollisionWarp)) != 0)
        {
            collision |= CollisionHit;
            if (PlayerAvatar_WillWarp(playerAvatar, mapObject, direction) != 0)
            {
                collision |= CollisionWarp;
            }
        }
        if ((mapObjectCollision & MapObjectCollisionObject) != 0)
        {
            collision |= CollisionMapObject;
        }
        if (PlayerAvatar_WillJump(playerAvatar, mapObject, direction) != 0)
        {
            collision |= CollisionJump;
        }
        if (PlayerAvatar_WillJumpTwice(playerAvatar, mapObject, direction) != 0)
        {
            collision |= CollisionJumpTwice;
        }
        if (PlayerAvatar_WillHitBikeRamp(playerAvatar, mapObject, direction) == 1)
        {
            collision |= CollisionBikeRamp;
        }
        if (PlayerAvatar_WillBeOnWater(playerAvatar, mapObject, direction) == 1)
        {
            collision |= CollisionWater;
        }
        if (PlayerAvatar_WillBeOnBikeBridge(playerAvatar, mapObject, direction) == 1)
        {
            collision |= CollisionBikeBridge | CollisionHit;
        }
        if (PlayerAvatar_CheckBikeCollision(playerAvatar, mapObject, direction) == 1)
        {
            collision |= CollisionHit;
        }
        return collision;
    }

    // OFFSET 0x02060C24
    private static int PlayerAvatar_CheckMapObjCollision(PlayerAvatar playerAvatar, MapObject mapObject, int direction)
    {
        int x = MapObject.MapObject_GetX(mapObject);
        int offsetX = MapObjectMove.MapObject_GetDxFromDir(direction);
        int y = MapObject.MapObject_GetY(mapObject);
        int z = MapObject.MapObject_GetZ(mapObject);
        int offsetZ = MapObjectMove.MapObject_GetDzFromDir(direction);
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        int collision = 0;
        if (MapObjectMove.MapObject_IsOutOfRange(mapObject, x + offsetX, y, z + offsetZ) == 1)
        {
            collision |= MapObjectCollisionOutOfRange;
        }
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        if (TerrainCollisionManager.TerrainCollisionManager_WillPlayerCollide(fieldSystem, ref position, x + offsetX, z + offsetZ, out sbyte warpFlag) == 1)
        {
            collision |= MapObjectCollisionTerrain;
            if (warpFlag != 0)
            {
                collision |= MapObjectCollisionWarp;
            }
        }
        if (MapObjectMove.MapObject_IsMovementBlockedByTiles(mapObject, x + offsetX, z + offsetZ, direction) == 1)
        {
            collision |= MapObjectCollisionTerrain;
        }
        if (MapObjectMove.MapObject_IsObjectAt(mapObject, x + offsetX, y, z + offsetZ) == 1)
        {
            collision |= MapObjectCollisionObject;
        }
        return collision;
    }

    // OFFSET 0x02060CE4
    private static int PlayerAvatar_WillJump(PlayerAvatar playerAvatar, MapObject mapObject, int direction)
    {
        if (direction == DirectionNone)
        {
            return 0;
        }
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        int x = MapObject.MapObject_GetX(mapObject);
        int offsetX = MapObjectMove.MapObject_GetDxFromDir(direction);
        int z = MapObject.MapObject_GetZ(mapObject);
        int offsetZ = MapObjectMove.MapObject_GetDzFromDir(direction);
        if (DynamicMapFeatures.DynamicMapFeatures_WillPlayerJumpEternaGymClock(fieldSystem, x + offsetX, z + offsetZ, 0, direction) == 1)
        {
            return 1;
        }
        int tileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x + offsetX, z + offsetZ);
        switch (direction)
        {
            case DirectionNorth:
                if (TileBehavior.TileBehavior_IsJumpNorth(tileBehavior) == 1)
                {
                    return 1;
                }
                break;
            case DirectionSouth:
                if (TileBehavior.TileBehavior_IsJumpSouth(tileBehavior) == 1)
                {
                    return 1;
                }
                break;
            case DirectionWest:
                if (TileBehavior.TileBehavior_IsJumpWest(tileBehavior) == 1)
                {
                    return 1;
                }
                break;
            case DirectionEast:
                if (TileBehavior.TileBehavior_IsJumpEast(tileBehavior) == 1)
                {
                    return 1;
                }
                break;
        }
        return 0;
    }

    // OFFSET 0x02060D98
    private static int PlayerAvatar_WillJumpTwice(PlayerAvatar playerAvatar, MapObject mapObject, int direction)
    {
        int result = 0;
        if (direction == DirectionNone)
        {
            return result;
        }
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        int x = MapObject.MapObject_GetX(mapObject);
        int offsetX = MapObjectMove.MapObject_GetDxFromDir(direction);
        int z = MapObject.MapObject_GetZ(mapObject);
        int offsetZ = MapObjectMove.MapObject_GetDzFromDir(direction);
        int tileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x + offsetX, z + offsetZ);
        switch (direction)
        {
            case DirectionNorth:
                result = TileBehavior.TileBehavior_IsJumpNorthTwice(tileBehavior);
                break;
            case DirectionSouth:
                result = TileBehavior.TileBehavior_IsJumpSouthTwice(tileBehavior);
                break;
            case DirectionWest:
                result = TileBehavior.TileBehavior_IsJumpWestTwice(tileBehavior);
                break;
            case DirectionEast:
                result = TileBehavior.TileBehavior_IsJumpEastTwice(tileBehavior);
                break;
        }
        if (result == 1 && PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(fieldSystem, DynamicMapFeaturesDistortionWorld) == 1)
        {
            if (ov9_022511A0(fieldSystem, x + offsetX, z + offsetZ, direction) == 1)
            {
                result = 0;
            }
        }
        return result;
    }

    // OFFSET 0x02060E40
    private static int PlayerAvatar_WillWarp(PlayerAvatar playerAvatar, MapObject mapObject, int direction)
    {
        if (direction == DirectionNone)
        {
            return 0;
        }
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        int x = MapObject.MapObject_GetX(mapObject);
        int z = MapObject.MapObject_GetZ(mapObject);
        int tileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
        switch (direction)
        {
            case DirectionNorth:
                if (TileBehavior.TileBehavior_IsWarpEntranceNorth(tileBehavior) == 1)
                {
                    return 1;
                }
                break;
            case DirectionSouth:
                if (TileBehavior.TileBehavior_IsWarpEntranceSouth(tileBehavior) == 1)
                {
                    return 1;
                }
                break;
            case DirectionWest:
                if (TileBehavior.TileBehavior_IsWarpEntranceWest(tileBehavior) == 1)
                {
                    return 1;
                }
                break;
            case DirectionEast:
                if (TileBehavior.TileBehavior_IsWarpEntranceEast(tileBehavior) == 1)
                {
                    return 1;
                }
                break;
        }
        int offsetX = MapObjectMove.MapObject_GetDxFromDir(direction);
        int offsetZ = MapObjectMove.MapObject_GetDzFromDir(direction);
        tileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x + offsetX, z + offsetZ);
        if (TileBehavior.TileBehavior_IsDoor(tileBehavior) == 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02060EE4
    private static int PlayerAvatar_WillHitBikeRamp(PlayerAvatar playerAvatar, MapObject mapObject, int direction)
    {
        if (direction == DirectionNone)
        {
            return 0;
        }
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        int x = MapObject.MapObject_GetX(mapObject);
        int offsetX = MapObjectMove.MapObject_GetDxFromDir(direction);
        int z = MapObject.MapObject_GetZ(mapObject);
        int offsetZ = MapObjectMove.MapObject_GetDzFromDir(direction);
        int tileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x + offsetX, z + offsetZ);
        if (direction == DirectionEast && TileBehavior.TileBehavior_IsBikeRampEastward(tileBehavior) != 0)
        {
            return 1;
        }
        if (direction == DirectionWest && TileBehavior.TileBehavior_IsBikeRampWestward(tileBehavior) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02060F4C
    private static int PlayerAvatar_WillBeOnWater(PlayerAvatar playerAvatar, MapObject mapObject, int direction)
    {
        if (direction == DirectionNone)
        {
            return 0;
        }
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        int x = MapObject.MapObject_GetX(mapObject);
        int offsetX = MapObjectMove.MapObject_GetDxFromDir(direction);
        int z = MapObject.MapObject_GetZ(mapObject);
        int offsetZ = MapObjectMove.MapObject_GetDzFromDir(direction);
        int tileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x + offsetX, z + offsetZ);
        if (MapObjectMove.MapObject_IsOnWater(mapObject, tileBehavior) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02060FA8
    private static int PlayerAvatar_WillBeOnBikeBridge(PlayerAvatar playerAvatar, MapObject mapObject, int direction)
    {
        if (direction == DirectionNone)
        {
            return 0;
        }
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        int x = MapObject.MapObject_GetX(mapObject);
        int offsetX = MapObjectMove.MapObject_GetDxFromDir(direction);
        int z = MapObject.MapObject_GetZ(mapObject);
        int offsetZ = MapObjectMove.MapObject_GetDzFromDir(direction);
        int tileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x + offsetX, z + offsetZ);
        if (PlayerAvatar_GetPlayerState(playerAvatar) == StateCycling)
        {
            if (MapObjectMove.MapObject_IsOnBikeBridgeNorthSouth(mapObject, tileBehavior) == 1)
            {
                if ((uint)direction > DirectionSouth)
                {
                    return 1;
                }
                return 0;
            }
            if (MapObjectMove.MapObject_IsOnBikeBridgeEastWest(mapObject, tileBehavior) == 1)
            {
                if ((uint)(direction - DirectionWest) > 1)
                {
                    return 1;
                }
                return 0;
            }
            return 0;
        }
        if (MapObjectMove.MapObject_IsOnBikeBridgeNorthSouth(mapObject, tileBehavior) == 1 || MapObjectMove.MapObject_IsOnBikeBridgeEastWest(mapObject, tileBehavior) == 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02061058
    private static int PlayerAvatar_CheckBikeCollision(PlayerAvatar playerAvatar, MapObject mapObject, int direction)
    {
        if (direction == DirectionNone || PlayerAvatar_GetPlayerState(playerAvatar) != StateCycling)
        {
            return 0;
        }
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        int x = MapObject.MapObject_GetX(mapObject);
        int offsetX = MapObjectMove.MapObject_GetDxFromDir(direction);
        int z = MapObject.MapObject_GetZ(mapObject);
        int offsetZ = MapObjectMove.MapObject_GetDzFromDir(direction);
        int tileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x + offsetX, z + offsetZ);
        if (MapObjectMove.MapObject_IsOnWater(mapObject, tileBehavior) != 0)
        {
            return 1;
        }
        if (MapObjectMove.MapObject_IsOnSnow(mapObject, tileBehavior) != 0)
        {
            return 1;
        }
        if (TileBehavior.TileBehavior_IsVeryTallGrass(tileBehavior) != 0)
        {
            return 1;
        }
        if (TileBehavior.TileBehavior_IsMud(tileBehavior) != 0)
        {
            return 1;
        }
        if (TileBehavior.TileBehavior_IsMudWithGrass(tileBehavior) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02061100
    private static int PlayerAvatar_CheckDistortionMapObjectCollision(PlayerAvatar playerAvatar, MapObject mapObject, int direction)
    {
        int collision = 0;
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(fieldSystem, DynamicMapFeaturesDistortionWorld) == 1)
        {
            int x = MapObject.MapObject_GetX(mapObject);
            int y = MapObject.MapObject_GetY(mapObject) / 2;
            int z = MapObject.MapObject_GetZ(mapObject);
            PlayerAvatar_GetStepDistortionWorldPos(playerAvatar, direction, ref x, ref y, ref z);
            if (ov9_02250F90(fieldSystem, x, y, z) == 1)
            {
                collision |= MapObjectCollisionTerrain;
            }
            y <<= 1;
            if (MapObjectMove.MapObject_IsObjectAt(mapObject, x, y, z) == 1)
            {
                collision |= MapObjectCollisionObject;
            }
        }
        return collision;
    }

    // OFFSET 0x02061180
    private static int PlayerAvatar_WillBeOnWaterDistortion(PlayerAvatar playerAvatar, MapObject mapObject, int direction)
    {
        if (direction == DirectionNone)
        {
            return 0;
        }
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(fieldSystem, DynamicMapFeaturesDistortionWorld) != 1)
        {
            return 0;
        }
        int x = MapObject.MapObject_GetX(mapObject);
        int y = MapObject.MapObject_GetY(mapObject) / 2;
        int z = MapObject.MapObject_GetZ(mapObject);
        PlayerAvatar_GetStepDistortionWorldPos(playerAvatar, direction, ref x, ref y, ref z);
        ov9_02251044(fieldSystem, x, y, z, out uint tileBehavior);
        if (TileBehavior.TileBehavior_IsSurfable((byte)tileBehavior) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020611FC
    public static int PlayerAvatar_CheckDistortionCollision(PlayerAvatar playerAvatar, MapObject mapObject, int direction)
    {
        int collision = 0;
        int mapObjectCollision = PlayerAvatar_CheckDistortionMapObjectCollision(playerAvatar, mapObject, direction);
        if ((mapObjectCollision & (MapObjectCollisionTerrain | MapObjectCollisionWarp)) != 0)
        {
            collision |= CollisionHit;
        }
        if ((mapObjectCollision & MapObjectCollisionObject) != 0)
        {
            collision |= CollisionMapObject;
        }
        if (PlayerAvatar_WillBeOnWaterDistortion(playerAvatar, mapObject, direction) == 1)
        {
            collision |= CollisionWater;
        }
        if (PlayerAvatar_WillJumpTwiceDistortion(playerAvatar, mapObject, direction) == 1)
        {
            collision |= CollisionJumpTwice;
        }
        return collision;
    }

    // OFFSET 0x02061248
    private static int PlayerAvatar_WillJumpTwiceDistortion(PlayerAvatar playerAvatar, MapObject mapObject, int direction)
    {
        int result = 0;
        if (direction == DirectionNone)
        {
            return result;
        }
        if (PlayerAvatar_MapDistortionState(playerAvatar) != DistortionStateFloor)
        {
            return result;
        }
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(fieldSystem, DynamicMapFeaturesDistortionWorld) != 1)
        {
            return result;
        }
        int x = MapObject.MapObject_GetX(mapObject);
        int y = MapObject.MapObject_GetY(mapObject) / 2;
        int z = MapObject.MapObject_GetZ(mapObject);
        PlayerAvatar_GetStepDistortionWorldPos(playerAvatar, direction, ref x, ref y, ref z);
        result = ov9_02251044(fieldSystem, x, y, z, out uint tileBehavior);
        switch (direction)
        {
            case DirectionNorth:
                result = TileBehavior.TileBehavior_IsJumpNorthTwice((byte)tileBehavior);
                break;
            case DirectionSouth:
                result = TileBehavior.TileBehavior_IsJumpSouthTwice((byte)tileBehavior);
                break;
            case DirectionWest:
                result = TileBehavior.TileBehavior_IsJumpWestTwice((byte)tileBehavior);
                break;
            case DirectionEast:
                result = TileBehavior.TileBehavior_IsJumpEastTwice((byte)tileBehavior);
                break;
        }
        return result;
    }

    // OFFSET 0x02061308
    public static int PlayerAvatar_CalcFaceDirection(PlayerAvatar playerAvatar, ushort pressedKeys, ushort heldKeys)
    {
        return PlayerAvatar_CalcFaceDirectionInternal(playerAvatar, pressedKeys, heldKeys);
    }

    // OFFSET 0x02061310
    private static int Movement_FaceLeftRightFromInput(ushort heldKeys)
    {
        if ((heldKeys & KeyLeft) != 0)
        {
            return DirectionWest;
        }
        if ((heldKeys & KeyRight) != 0)
        {
            return DirectionEast;
        }
        return DirectionNone;
    }

    // OFFSET 0x0206132C
    private static int Movement_FaceUpDownFromInput(ushort heldKeys)
    {
        if ((heldKeys & KeyUp) != 0)
        {
            return DirectionNorth;
        }
        if ((heldKeys & KeyDown) != 0)
        {
            return DirectionSouth;
        }
        return DirectionNone;
    }

    // OFFSET 0x02061348
    private static int PlayerAvatar_CalcFaceDirectionInternal(PlayerAvatar playerAvatar, ushort pressedKeys, ushort heldKeys)
    {
        int leftOrRight = Movement_FaceLeftRightFromInput(heldKeys);
        int upOrDown = Movement_FaceUpDownFromInput(heldKeys);
        if (leftOrRight == DirectionNone)
        {
            return upOrDown;
        }
        if (upOrDown == DirectionNone)
        {
            return leftOrRight;
        }
        int movingDirection = PlayerAvatar_GetMovingDir(playerAvatar);
        int previousLeftOrRight = PlayerAvatar_GetFaceLeftOrRight(playerAvatar);
        int previousUpOrDown = PlayerAvatar_GetFaceUpOrDown(playerAvatar);
        if (movingDirection != DirectionNone)
        {
            if (leftOrRight == previousLeftOrRight && upOrDown == previousUpOrDown)
            {
                return movingDirection;
            }
            if (upOrDown != previousUpOrDown)
            {
                return upOrDown;
            }
            return leftOrRight;
        }
        return upOrDown;
    }

    // OFFSET 0x020613AC
    public static int PlayerAvatar_IsAnimationSetOrWalkOnSpotSlow(PlayerAvatar playerAvatar)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        if (MovementActions.LocalMapObj_IsAnimationSet(mapObject) == 1)
        {
            return 1;
        }
        if (MovementAction_IsWalkOnSpotSlow(MapObject.MapObject_GetMovementAction(mapObject)) == 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020613D8
    private static int MovementAction_IsWalkOnSpotSlow(int movementAction)
    {
        switch (movementAction)
        {
            case ActionWalkOnSpotSlow + DirectionNorth:
            case ActionWalkOnSpotSlow + DirectionSouth:
            case ActionWalkOnSpotSlow + DirectionWest:
            case ActionWalkOnSpotSlow + DirectionEast:
                return 1;
        }
        return 0;
    }

    // OFFSET 0x020613FC
    public static int PlayerAvatar_IsRunButtonHeld(PlayerAvatar playerAvatar, ushort heldKeys)
    {
        if ((heldKeys & KeyB) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0206140C
    public static int PlayerAvatar_IsRunning(PlayerAvatar playerAvatar)
    {
        switch (PlayerAvatar_GetMovementAction(playerAvatar))
        {
            case ActionRun + DirectionNorth:
            case ActionRun + DirectionSouth:
            case ActionRun + DirectionWest:
            case ActionRun + DirectionEast:
                return 1;
        }
        return 0;
    }

    // OFFSET 0x02061434
    public static int PlayerAvatar_GetMoveStateFromPlayerAvatarState(PlayerAvatar playerAvatar, int direction)
    {
        int moveState;
        switch (PlayerAvatar_GetPlayerState(playerAvatar))
        {
            case StateWalking:
            case StateSurfing:
                moveState = PlayerAvatar_ValidateState(PlayerAvatar_UpdateMoveState(playerAvatar, direction));
                break;
            case StateCycling:
                PlayerAvatar_GetCyclingGear(playerAvatar);
                moveState = PlayerAvatarBikeState_GetMoveStateFourthGear(PlayerAvatar_GetBikeStateFourthGear(playerAvatar, direction));
                break;
            default:
                moveState = MoveStateNone;
                ErrorHandling.GF_AssertFail();
                break;
        }
        return moveState;
    }

    // OFFSET 0x0206147C
    public static int PlayerAvatar_GetMovementActionAnimCode(PlayerAvatar playerAvatar, ushort pressedKeys, ushort heldKeys, int actionSpeed, int isRunning, int collision)
    {
        int direction = PlayerAvatar_CalcFaceDirectionInternal(playerAvatar, pressedKeys, heldKeys);
        int moveState = PlayerAvatar_GetMoveStateFromPlayerAvatarState(playerAvatar, direction);
        PlayerAvatar_SetMoveState(playerAvatar, moveState);
        if (moveState == MoveStateNone)
        {
            return MovementActions.MovementAction_TurnActionTowardsDir(PlayerAvatar_GetFacingDir(playerAvatar), ActionFace);
        }
        if (moveState == MoveStateTurning)
        {
            return MovementActions.MovementAction_TurnActionTowardsDir(direction, ActionTurn);
        }
        PlayerAvatar_GetMapObject(playerAvatar);
        int action = ActionWalkSlowest;
        if ((collision & CollisionJump) != 0)
        {
            action = ActionJump;
        }
        else if ((collision & CollisionJumpTwice) != 0)
        {
            action = ActionJumpTwice;
        }
        else if (collision != 0)
        {
            action = ActionWalkOnSpotSlow;
            if ((collision & CollisionWarp) == 0)
            {
                SoundPlayback.Sound_PlayEffect(SeqBump);
            }
        }
        else
        {
            switch (actionSpeed)
            {
                case 0:
                    break;
                case 1:
                    action = ActionWalkSlow;
                    break;
                case 2:
                    action = ActionWalkNormal;
                    break;
                case 3:
                    action = ActionBikeSlow;
                    break;
                case 4:
                    action = ActionWalkFast;
                    break;
                case 5:
                    action = ActionWalkFaster;
                    break;
                default:
                    action = ActionWalkSlowest;
                    break;
            }
            if (isRunning == 1 && PlayerAvatar_IsRunButtonHeld(playerAvatar, heldKeys) == 1)
            {
                action = ActionRun;
            }
        }
        return MovementActions.MovementAction_TurnActionTowardsDir(direction, action);
    }

    // OFFSET 0x02061544
    public static int PlayerAvatar_IsMapObjectAnimationSet(PlayerAvatar playerAvatar)
    {
        return MovementActions.LocalMapObj_IsAnimationSet(PlayerAvatar_GetMapObject(playerAvatar));
    }

    // OFFSET 0x02061550
    public static void PlayerAvatar_SetMapObjMovement(PlayerAvatar playerAvatar, int movementAction, int speed)
    {
        PlayerAvatar_SetMovement(playerAvatar, PlayerAvatar_GetMapObject(playerAvatar), movementAction, speed);
    }

    // OFFSET 0x0206156C
    public static int PlayerAvatar_GetTileBehaviorFromDir(PlayerAvatar playerAvatar, int direction)
    {
        return MapObjectMove.MapObject_GetTileBehaviorFromDir(PlayerAvatar_GetMapObject(playerAvatar), direction);
    }

    // OFFSET 0x0206157C
    public static void PlayerAvatar_MoveCoordsInDirection(PlayerAvatar playerAvatar, int direction, out int x, out int z)
    {
        x = PlayerAvatar_GetXPos(playerAvatar) + MapObjectMove.MapObject_GetDxFromDir(direction);
        z = PlayerAvatar_GetZPos(playerAvatar) + MapObjectMove.MapObject_GetDzFromDir(direction);
    }

    // OFFSET 0x020615AC
    public static void PlayerAvatar_GetFacingTileCoords(PlayerAvatar playerAvatar, out int x, out int z)
    {
        PlayerAvatar_MoveCoordsInDirection(playerAvatar, PlayerAvatar_GetFacingDir(playerAvatar), out x, out z);
    }

    // OFFSET 0x020615C8
    private static void PlayerAvatar_IncrementStepsRecord(PlayerAvatar playerAvatar)
    {
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(PlayerAvatar_GetMapObject(playerAvatar))!;
        GameRecords.GameRecords_IncrementRecordValue(GameRecords.SaveData_GetGameRecords(fieldSystem.SaveData!), RecordStepsTaken);
    }

    // OFFSET 0x020615E0
    private static int PlayerAvatar_IsOnBikeBridgeWrongDirection(PlayerAvatar playerAvatar, MapObject mapObject, int direction)
    {
        if (direction == DirectionNone)
        {
            return 0;
        }
        int tileBehavior = (byte)MapObject.MapObject_GetCurrTileBehavior(mapObject);
        if (MapObjectMove.MapObject_IsOnBikeBridgeNorthSouth(mapObject, tileBehavior) == 1)
        {
            if ((uint)direction > DirectionSouth)
            {
                return 1;
            }
            return 0;
        }
        if (MapObjectMove.MapObject_IsOnBikeBridgeEastWest(mapObject, tileBehavior) == 1)
        {
            if ((uint)(direction - DirectionWest) > 1)
            {
                return 1;
            }
            return 0;
        }
        return 0;
    }

    // OFFSET 0x02061630
    private static int PlayerAvatar_IsOnCyclingRoadBridge(PlayerAvatar playerAvatar, int tileBehavior, int direction)
    {
        if (direction != DirectionNone)
        {
            return 0;
        }
        if (PlayerAvatar_GetPlayerState(playerAvatar) != StateCycling)
        {
            return 0;
        }
        if (PlayerAvatar_IsOnCyclingRoad(playerAvatar) == 0)
        {
            return 0;
        }
        if (MapObjectMove.MapObject_IsOnElevatedBridge(PlayerAvatar_GetMapObject(playerAvatar), tileBehavior) == 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02061674
    public static void PlayerAvatar_GetStepDistortionWorldPos(PlayerAvatar playerAvatar, int direction, ref int x, ref int y, ref int z)
    {
        uint tableAddress;
        switch (PlayerAvatar_MapDistortionState(playerAvatar))
        {
            case DistortionStateNone:
            case DistortionStateActive:
            case DistortionStateFloor:
                tableAddress = DistortionFloorStepTableAddress;
                break;
            case DistortionStateWestWall:
                tableAddress = DistortionWestWallStepTableAddress;
                break;
            case DistortionStateEastWall:
                tableAddress = DistortionEastWallStepTableAddress;
                break;
            case DistortionStateCeiling:
                tableAddress = DistortionCeilingStepTableAddress;
                break;
            default:
                return;
        }
        ushort[] step = RomData.Arm9U16(tableAddress + (uint)(direction * 8), 3);
        x += (short)step[0];
        y += (short)step[1];
        z += (short)step[2];
    }

    // OFFSET 0x020616F0
    public static uint PlayerAvatar_GetDistortionFacingTileBehaviour(PlayerAvatar playerAvatar, int direction)
    {
        if (PlayerAvatar_DistortionGravityChanged(playerAvatar) == 0)
        {
            return (uint)PlayerAvatar_GetTileBehaviorFromDir(playerAvatar, direction);
        }
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        int x = MapObject.MapObject_GetX(mapObject);
        int y = MapObject.MapObject_GetY(mapObject) / 2;
        int z = MapObject.MapObject_GetZ(mapObject);
        PlayerAvatar_GetStepDistortionWorldPos(playerAvatar, direction, ref x, ref y, ref z);
        ov9_02251044(fieldSystem, x, y, z, out uint tileBehavior);
        return tileBehavior;
    }

    // OFFSET 0x02061760
    public static uint PlayerAvatar_GetDistortionCurrTileBehaviour(PlayerAvatar playerAvatar)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        int x = MapObject.MapObject_GetX(mapObject);
        int y = MapObject.MapObject_GetY(mapObject) / 2;
        int z = MapObject.MapObject_GetZ(mapObject);
        if (PlayerAvatar_DistortionGravityChanged(playerAvatar) == 0)
        {
            return (uint)TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
        }
        ov9_02251044(fieldSystem, x, y, z, out uint tileBehavior);
        return tileBehavior;
    }

    // OFFSET 0x020617BC
    public static void PlayerAvatar_GetFacingDistortionWorldPos(PlayerAvatar playerAvatar, out int x, out int y, out int z)
    {
        int direction = PlayerAvatar_GetDistortionDir(playerAvatar);
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        x = MapObject.MapObject_GetX(mapObject);
        y = MapObject.MapObject_GetY(mapObject) / 2;
        z = MapObject.MapObject_GetZ(mapObject);
        PlayerAvatar_GetStepDistortionWorldPos(playerAvatar, direction, ref x, ref y, ref z);
    }
}
