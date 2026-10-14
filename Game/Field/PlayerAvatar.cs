using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class PlayerAvatar
{
    public const int MovementFlagsOffset = 0x00;
    public const int RequestStateFlagsOffset = 0x04;
    public const int MovementActionOffset = 0x08;
    public const int MovementActionSpeedOffset = 0x0C;
    public const int Unused10Offset = 0x10;
    public const int MoveStateOffset = 0x14;
    public const int PlayerMoveStateOffset = 0x18;
    public const int StateOffset = 0x1C;
    public const int GenderOffset = 0x20;
    public const int SpeedOffset = 0x24;
    public const int FaceLeftOrRightOffset = 0x28;
    public const int FaceUpOrDownOffset = 0x2C;
    public const int MapObjectOffset = 0x30;
    public const int SurfMountAnimManagerOffset = 0x34;
    public const int PlayerDataOffset = 0x38;
    public const int Unused3COffset = 0x3C;
    public const int Size = 0x40;

    public const int PlayerDataCyclingGearOffset = 0x00;
    public const int PlayerDataRunningShoesOffset = 0x02;
    public const int PlayerDataStateOffset = 0x04;
    public const int PlayerDataSize = 0x08;

    public const int DirectionNone = -1;
    public const int DirectionNorth = 0;
    public const int DirectionSouth = 1;
    public const int DirectionWest = 2;
    public const int DirectionEast = 3;

    public const int StateWalking = 0;
    public const int StateCycling = 1;
    public const int StateSurfing = 2;
    public const int StateCount = 6;

    public const int MoveStateNone = 0;
    public const int MoveStateMoving = 1;
    public const int MoveStateTurning = 2;

    public const int PlayerMoveStateNone = 0;
    public const int PlayerMoveStateStart = 1;
    public const int PlayerMoveStateMoving = 2;
    public const int PlayerMoveStateEnd = 3;

    public const uint MovementFlagForced = 0x0001;
    public const uint MovementFlagIgnoreTileBehavior = 0x0002;
    public const uint MovementFlagBikeBrake = 0x0004;
    public const uint MovementFlagOnCyclingRoad = 0x0008;
    public const uint MovementFlagEscapedFromDeepMud = 0x0010;
    public const uint MovementFlagMoveThroughMud = 0x0020;
    public const uint MovementFlagStep = 0x0040;
    public const uint MovementFlagForcedRetainSpeed = 0x0080;
    public const uint MovementFlagDistortionActive = 0x0100;
    public const uint MovementFlagDistortionFloor = 0x0200;
    public const uint MovementFlagDistortionWestWall = 0x0400;
    public const uint MovementFlagDistortionEastWall = 0x0800;
    public const uint MovementFlagDistortionCeiling = 0x1000;
    public const uint MovementFlagDistortionMask = 0x1F00;

    public const int DistortionStateNone = 0;
    public const int DistortionStateActive = 1;
    public const int DistortionStateFloor = 2;
    public const int DistortionStateWestWall = 3;
    public const int DistortionStateEastWall = 4;
    public const int DistortionStateCeiling = 5;

    private const int MovementActionNone = 0xFF;
    private const int PlayerLocalID = 0xFF;
    private const int PlayerMovementType = 1;
    private const int PlayerMapID = 1;
    private const int GraphicsIDHiddenMale = 0xFC;
    private const int GraphicsIDHiddenFemale = 0xFD;
    private const uint MapObjectStatusPlayerSet = 0x2400;
    private const uint MapObjectStatusPlayerClear = 0x180;
    private const uint MapObjectStatusHidden = 0x200;
    private const uint MapObjectStatusInUse = 1;
    private const int DynamicMapFeaturesDistortionWorld = 9;

    public uint MovementFlags;
    public uint RequestStateFlags;
    public uint MovementAction;
    public int MovementActionSpeed;
    public uint Unused10;
    public int MoveState;
    public int PlayerMoveState;
    public int State;
    public int Gender;
    public int Speed;
    public int FaceLeftOrRight;
    public int FaceUpOrDown;
    public MapObject? MapObject;
    public object? SurfMountAnimManager;
    public BufferView? PlayerData;
    public BufferView? Unused3C;

    // OFFSET 0x0205E7D0
    public static PlayerAvatar PlayerAvatar_New(MapObjectManager mapObjectManager, int x, int z, int direction, int playerState, int gender, int hidden, BufferView? playerData)
    {
        PlayerAvatar playerAvatar = PlayerAvatar_Alloc();
        PlayerAvatar_Init(playerAvatar, playerState, gender, playerData);
        int graphicsID;
        if (hidden == 0)
        {
            graphicsID = Player_GetSpriteFromStateAndGender(playerState, gender);
        }
        else if (gender == 0)
        {
            graphicsID = GraphicsIDHiddenMale;
        }
        else
        {
            graphicsID = GraphicsIDHiddenFemale;
        }
        PlayerAvatar_AddMapObject(playerAvatar, mapObjectManager, graphicsID, direction, x, z);
        return playerAvatar;
    }

    // OFFSET 0x0205E820
    public static PlayerAvatar PlayerAvatar_NewLoad(MapObjectManager mapObjectManager, BufferView? playerData, int gender)
    {
        PlayerAvatar playerAvatar = PlayerAvatar_Alloc();
        int playerState = PlayerData_GetState(playerData);
        PlayerAvatar_Init(playerAvatar, playerState, gender, playerData);
        MapObject mapObject = MapObjectMan_GetPlayerMapObjectChecked(mapObjectManager);
        MapObject.MapObject_SetGraphicsID(mapObject, Player_GetSpriteFromStateAndGender(playerState, gender));
        MapObject.MapObject_SetStatusFlagOn(mapObject, MapObjectStatusPlayerSet);
        MapObject.MapObject_SetStatusFlagOff(mapObject, MapObjectStatusPlayerClear);
        MapObject.MapObject_SetDynamicHeightCalculationEnabled(mapObject, 1);
        PlayerAvatar_SetMapObject(playerAvatar, mapObject);
        return playerAvatar;
    }

    // OFFSET 0x0205E884
    public static void PlayerAvatar_InitMapFeatures(PlayerAvatar playerAvatar, int dynamicMapFeaturesID)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        if (mapObject == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObject.MapObject_MapObjectManager(mapObject);
        WarpArrowEffect.WarpArrowEffect_New(playerAvatar);
        if (PlayerAvatar_GetPlayerState(playerAvatar) == StateSurfing && dynamicMapFeaturesID != DynamicMapFeaturesDistortionWorld)
        {
            int x = PlayerAvatar_GetXPos(playerAvatar);
            int z = PlayerAvatar_GetZPos(playerAvatar);
            int direction = PlayerAvatar_GetFacingDir(playerAvatar);
            object? surfMount = SurfMount.SurfMountRenderer_HandleSurfBegin(mapObject, x, z, direction, 1);
            PlayerAvatar_SetSurfMountAnimManager(playerAvatar, surfMount);
        }
    }

    // OFFSET 0x0205E8E0
    public static void PlayerAvatar_Free(PlayerAvatar playerAvatar)
    {
    }

    // OFFSET 0x0205E8E8
    public static void PlayerAvatar_Delete(PlayerAvatar playerAvatar)
    {
        MapObject.MapObject_Delete(PlayerAvatar_GetMapObject(playerAvatar));
        PlayerAvatar_Free(playerAvatar);
    }

    // OFFSET 0x0205E8FC
    private static PlayerAvatar PlayerAvatar_Alloc()
    {
        return new PlayerAvatar();
    }

    // OFFSET 0x0205E91C
    private static void PlayerAvatar_Init(PlayerAvatar playerAvatar, int playerState, int gender, BufferView? playerData)
    {
        PlayerAvatar_SetPlayerData(playerAvatar, playerData);
        PlayerAvatar_SetMoveState(playerAvatar, MoveStateNone);
        PlayerAvatar_SetPlayerMoveState(playerAvatar, PlayerMoveStateNone);
        PlayerAvatar_SetPlayerState(playerAvatar, playerState);
        PlayerAvatar_SetGender(playerAvatar, gender);
        PlayerAvatar_SetRequestStateFlag(playerAvatar, 0);
        PlayerAvatar_ClearSpeed(playerAvatar);
        PlayerAvatar_SetFaceLeftOrRight(playerAvatar, DirectionNone);
        PlayerAvatar_SetFaceUpOrDown(playerAvatar, DirectionNone);
        PlayerAvatar_SetMovementActionAndSpeed(playerAvatar, MovementActionNone, 0);
        PlayerAvatar_SetIgnoreTileBehavior(playerAvatar, 1);
        PlayerAvatar_SetEscapedFromDeepMud(playerAvatar, 1);
    }

    // OFFSET 0x0205E988
    private static void PlayerAvatar_AddMapObject(PlayerAvatar playerAvatar, MapObjectManager mapObjectManager, int graphicsID, int direction, int x, int z)
    {
        MapObject? newMapObject = MapObjectManager.MapObjectMan_AddMapObject(mapObjectManager, x, z, direction, graphicsID, PlayerMovementType, PlayerMapID);
        if (newMapObject == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObject mapObject = newMapObject!;
        MapObject.MapObject_SetLocalID(mapObject, PlayerLocalID);
        MapObject.MapObject_SetTrainerType(mapObject, 0);
        MapObject.MapObject_SetFlag(mapObject, 0);
        MapObject.MapObject_SetScript(mapObject, 0);
        MapObject.MapObject_SetDataAt(mapObject, 0, 0);
        MapObject.MapObject_SetDataAt(mapObject, 0, 1);
        MapObject.MapObject_SetDataAt(mapObject, 0, 2);
        MapObject.MapObject_SetMovementRangeX(mapObject, -1);
        MapObject.MapObject_SetMovementRangeZ(mapObject, -1);
        MapObject.MapObject_SetStatusFlagOn(mapObject, MapObjectStatusPlayerSet);
        MapObject.MapObject_SetStatusFlagOff(mapObject, MapObjectStatusPlayerClear);
        MapObject.MapObject_SetDynamicHeightCalculationEnabled(mapObject, 1);
        PlayerAvatar_SetMapObject(playerAvatar, mapObject);
    }

    // OFFSET 0x0205EA24
    public static MapObject? MapObjectMan_GetPlayerMapObject(MapObjectManager mapObjectManager)
    {
        int index = 0;
        MapObject? mapObject = null;
        while (MapObjectManager.MapObjectMan_FindObjectWithStatus(mapObjectManager, out MapObject? found, ref index, MapObjectStatusInUse) != 0)
        {
            mapObject = found;
            if (MapObject.MapObject_GetMovementType(mapObject!) == PlayerMovementType)
            {
                break;
            }
        }
        return mapObject;
    }

    // OFFSET 0x0205EA64
    private static MapObject MapObjectMan_GetPlayerMapObjectChecked(MapObjectManager mapObjectManager)
    {
        MapObject? mapObject = MapObjectMan_GetPlayerMapObject(mapObjectManager);
        if (mapObject == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return mapObject!;
    }

    // OFFSET 0x0205EA78
    public static int PlayerAvatar_GetFacingDir(PlayerAvatar playerAvatar)
    {
        return MapObject.MapObject_GetFacingDir(PlayerAvatar_GetMapObject(playerAvatar));
    }

    // OFFSET 0x0205EA84
    public static void PlayerAvatar_TryFace(PlayerAvatar playerAvatar, int direction)
    {
        MapObject.MapObject_TryFace(PlayerAvatar_GetMapObject(playerAvatar), direction);
    }

    // OFFSET 0x0205EA94
    public static int PlayerAvatar_GetMovingDir(PlayerAvatar playerAvatar)
    {
        return MapObject.MapObject_GetMovingDir(PlayerAvatar_GetMapObject(playerAvatar));
    }

    // OFFSET 0x0205EAA0
    public static int PlayerAvatar_GetDistortionDir(PlayerAvatar playerAvatar)
    {
        if (PlayerAvatar_DistortionStateOnFloor(playerAvatar) == 1)
        {
            return PlayerAvatar_GetFacingDir(playerAvatar);
        }
        return PlayerAvatar_GetMovingDir(playerAvatar);
    }

    // OFFSET 0x0205EABC
    public static int PlayerAvatar_GetXPos(PlayerAvatar playerAvatar)
    {
        return MapObject.MapObject_GetX(PlayerAvatar_GetMapObject(playerAvatar));
    }

    // OFFSET 0x0205EAC8
    public static int PlayerAvatar_GetZPos(PlayerAvatar playerAvatar)
    {
        return MapObject.MapObject_GetZ(PlayerAvatar_GetMapObject(playerAvatar));
    }

    // OFFSET 0x0205EAD4
    public static int PlayerAvatar_XPosPrev(PlayerAvatar playerAvatar)
    {
        return MapObject.MapObject_GetXPrev(PlayerAvatar_GetMapObject(playerAvatar));
    }

    // OFFSET 0x0205EAE0
    public static int PlayerAvatar_ZPosPrev(PlayerAvatar playerAvatar)
    {
        return MapObject.MapObject_GetZPrev(PlayerAvatar_GetMapObject(playerAvatar));
    }

    // OFFSET 0x0205EAEC
    public static void PlayerAvatar_GetPosPtr(PlayerAvatar playerAvatar, out VecFx32 position)
    {
        MapObject.MapObject_CopyPos(PlayerAvatar_GetMapObject(playerAvatar), out position);
    }

    // OFFSET 0x0205EAFC
    public static ref VecFx32 PlayerAvatar_GetPos(PlayerAvatar playerAvatar)
    {
        return ref MapObject.MapObject_GetPos(PlayerAvatar_GetConstMapObject(playerAvatar)!);
    }

    // OFFSET 0x0205EB08
    public static void PlayerAvatar_SetMoveState(PlayerAvatar playerAvatar, int moveState)
    {
        playerAvatar.MoveState = moveState;
    }

    // OFFSET 0x0205EB0C
    public static int PlayerAvatar_GetMoveState(PlayerAvatar playerAvatar)
    {
        return playerAvatar.MoveState;
    }

    // OFFSET 0x0205EB10
    public static void PlayerAvatar_SetPlayerMoveState(PlayerAvatar playerAvatar, int playerMoveState)
    {
        playerAvatar.PlayerMoveState = playerMoveState;
    }

    // OFFSET 0x0205EB14
    public static int PlayerAvatar_GetPlayerMoveState(PlayerAvatar playerAvatar)
    {
        return playerAvatar.PlayerMoveState;
    }

    // OFFSET 0x0205EB18
    public static void PlayerAvatar_SetVisible(PlayerAvatar playerAvatar, int visible)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        if (visible == 1)
        {
            MapObject.MapObject_SetStatusFlagOff(mapObject, MapObjectStatusHidden);
        }
        else
        {
            MapObject.MapObject_SetStatusFlagOn(mapObject, MapObjectStatusHidden);
        }
    }

    // OFFSET 0x0205EB38
    public static void PlayerAvatar_SetMapObject(PlayerAvatar playerAvatar, MapObject? mapObject)
    {
        playerAvatar.MapObject = mapObject;
    }

    // OFFSET 0x0205EB3C
    public static MapObject PlayerAvatar_GetMapObject(PlayerAvatar playerAvatar)
    {
        if (playerAvatar == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (playerAvatar!.MapObject == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return playerAvatar.MapObject!;
    }

    // OFFSET 0x0205EB54
    public static MapObject? PlayerAvatar_GetConstMapObject(PlayerAvatar playerAvatar)
    {
        return playerAvatar.MapObject;
    }

    // OFFSET 0x0205EB58
    public static void PlayerAvatar_SetPlayerState(PlayerAvatar playerAvatar, int playerState)
    {
        if (playerState >= StateCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        playerAvatar.State = playerState;
        PlayerAvatar_SetPlayerDataState(playerAvatar, playerState);
    }

    // OFFSET 0x0205EB74
    public static int PlayerAvatar_GetPlayerState(PlayerAvatar playerAvatar)
    {
        if (playerAvatar == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return playerAvatar!.State;
    }

    // OFFSET 0x0205EB84
    public static void PlayerAvatar_TurnOnRequestStateBit(PlayerAvatar playerAvatar, uint flags)
    {
        playerAvatar.RequestStateFlags |= flags;
    }

    // OFFSET 0x0205EB8C
    public static void PlayerAvatar_SetRequestStateFlag(PlayerAvatar playerAvatar, uint flags)
    {
        playerAvatar.RequestStateFlags = flags;
    }

    // OFFSET 0x0205EB90
    public static uint PlayerAvatar_GetRequestStateFlag(PlayerAvatar playerAvatar)
    {
        return playerAvatar.RequestStateFlags;
    }

    // OFFSET 0x0205EB94
    public static void PlayerAvatar_SetGender(PlayerAvatar playerAvatar, int gender)
    {
        playerAvatar.Gender = gender;
    }

    // OFFSET 0x0205EB98
    public static int PlayerAvatar_GetGender(PlayerAvatar playerAvatar)
    {
        return playerAvatar.Gender;
    }

    // OFFSET 0x0205EB9C
    private static void PlayerAvatar_TurnOnMovementFlag(PlayerAvatar playerAvatar, uint flags)
    {
        playerAvatar.MovementFlags |= flags;
    }

    // OFFSET 0x0205EBA4
    private static void PlayerAvatar_TurnOffMovementFlag(PlayerAvatar playerAvatar, uint flags)
    {
        playerAvatar.MovementFlags &= ~flags;
    }

    // OFFSET 0x0205EBB0
    private static uint PlayerAvatar_CheckMovementFlag(PlayerAvatar playerAvatar, uint flags)
    {
        return playerAvatar.MovementFlags & flags;
    }

    // OFFSET 0x0205EBB8
    public static int PlayerAvatar_GetSpeed(PlayerAvatar playerAvatar)
    {
        return playerAvatar.Speed;
    }

    // OFFSET 0x0205EBBC
    public static void PlayerAvatar_SetSpeed(PlayerAvatar playerAvatar, int speed)
    {
        playerAvatar.Speed = speed;
    }

    // OFFSET 0x0205EBC0
    public static void PlayerAvatar_ClearSpeed(PlayerAvatar playerAvatar)
    {
        playerAvatar.Speed = 0;
        PlayerAvatar_SetBikeBrake(playerAvatar, 0);
    }

    // OFFSET 0x0205EBCC
    public static int PlayerAvatar_AddMoveSpeed(PlayerAvatar playerAvatar, int amount, int maximum)
    {
        playerAvatar.Speed += amount;
        if (playerAvatar.Speed > maximum)
        {
            playerAvatar.Speed = maximum;
        }
        return playerAvatar.Speed;
    }

    // OFFSET 0x0205EBDC
    public static void PlayerAvatar_SetFaceLeftOrRight(PlayerAvatar playerAvatar, int direction)
    {
        playerAvatar.FaceLeftOrRight = direction;
    }

    // OFFSET 0x0205EBE0
    public static int PlayerAvatar_GetFaceLeftOrRight(PlayerAvatar playerAvatar)
    {
        return playerAvatar.FaceLeftOrRight;
    }

    // OFFSET 0x0205EBE4
    public static void PlayerAvatar_SetFaceUpOrDown(PlayerAvatar playerAvatar, int direction)
    {
        playerAvatar.FaceUpOrDown = direction;
    }

    // OFFSET 0x0205EBE8
    public static int PlayerAvatar_GetFaceUpOrDown(PlayerAvatar playerAvatar)
    {
        return playerAvatar.FaceUpOrDown;
    }

    // OFFSET 0x0205EBEC
    public static void PlayerAvatar_SetFaceDirection(PlayerAvatar playerAvatar, int leftOrRight, int upOrDown)
    {
        PlayerAvatar_SetFaceLeftOrRight(playerAvatar, leftOrRight);
        PlayerAvatar_SetFaceUpOrDown(playerAvatar, upOrDown);
    }

    // OFFSET 0x0205EC00
    public static void PlayerAvatar_SetSurfMountAnimManager(PlayerAvatar playerAvatar, object? surfMountAnimManager)
    {
        playerAvatar.SurfMountAnimManager = surfMountAnimManager;
    }

    // OFFSET 0x0205EC04
    public static object? PlayerAvatar_GetSurfMountAnimManager(PlayerAvatar playerAvatar)
    {
        return playerAvatar.SurfMountAnimManager;
    }

    // OFFSET 0x0205EC08
    public static void PlayerAvatar_SetPlayerData(PlayerAvatar playerAvatar, BufferView? playerData)
    {
        playerAvatar.PlayerData = playerData;
    }

    // OFFSET 0x0205EC0C
    public static BufferView? PlayerAvatar_PlayerData(PlayerAvatar playerAvatar)
    {
        return playerAvatar.PlayerData;
    }

    // OFFSET 0x0205EC10
    public static void PlayerAvatar_SetMovementAction(PlayerAvatar playerAvatar, uint movementAction)
    {
        playerAvatar.MovementAction = movementAction;
    }

    // OFFSET 0x0205EC14
    public static uint PlayerAvatar_GetMovementAction(PlayerAvatar playerAvatar)
    {
        return playerAvatar.MovementAction;
    }

    // OFFSET 0x0205EC18
    public static void PlayerAvatar_SetMovementActionSpeed(PlayerAvatar playerAvatar, int speed)
    {
        playerAvatar.MovementActionSpeed = speed;
    }

    // OFFSET 0x0205EC1C
    public static int PlayerAvatar_GetMovementActionSpeed(PlayerAvatar playerAvatar)
    {
        return playerAvatar.MovementActionSpeed;
    }

    // OFFSET 0x0205EC20
    public static void PlayerAvatar_SetMovementActionAndSpeed(PlayerAvatar playerAvatar, uint movementAction, int speed)
    {
        PlayerAvatar_SetMovementAction(playerAvatar, movementAction);
        PlayerAvatar_SetMovementActionSpeed(playerAvatar, speed);
    }

    // OFFSET 0x0205EC34
    public static void PlayerData_Init(BufferView playerData)
    {
        playerData.W16(PlayerDataCyclingGearOffset, 0);
        playerData.W16(PlayerDataRunningShoesOffset, 0);
        playerData.W32(PlayerDataStateOffset, 0);
    }

    // OFFSET 0x0205EC40
    public static int PlayerData_HasRunningShoes(BufferView? playerData)
    {
        if (playerData != null && playerData.U16(PlayerDataRunningShoesOffset) == 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205EC54
    public static void PlayerData_SetRunningShoes(BufferView playerData, int hasRunningShoes)
    {
        if (hasRunningShoes == 1)
        {
            playerData.W16(PlayerDataRunningShoesOffset, 1);
        }
        else
        {
            playerData.W16(PlayerDataRunningShoesOffset, 0);
        }
    }

    // OFFSET 0x0205EC64
    public static int PlayerData_GetCyclingGear(BufferView? playerData)
    {
        if (playerData == null)
        {
            return 0;
        }
        return playerData.U16(PlayerDataCyclingGearOffset);
    }

    // OFFSET 0x0205EC70
    public static void PlayerData_SetCyclingGear(BufferView? playerData, int cyclingGear)
    {
        if (playerData != null)
        {
            playerData.W16(PlayerDataCyclingGearOffset, cyclingGear);
        }
    }

    // OFFSET 0x0205EC78
    public static void PlayerAvatar_SetCyclingGear(PlayerAvatar playerAvatar, int cyclingGear)
    {
        PlayerData_SetCyclingGear(PlayerAvatar_PlayerData(playerAvatar), cyclingGear);
    }

    // OFFSET 0x0205EC88
    public static int PlayerAvatar_GetCyclingGear(PlayerAvatar playerAvatar)
    {
        return PlayerData_GetCyclingGear(PlayerAvatar_PlayerData(playerAvatar));
    }

    // OFFSET 0x0205EC94
    public static int PlayerData_GetState(BufferView? playerData)
    {
        if (playerData != null)
        {
            return (int)playerData.U32(PlayerDataStateOffset);
        }
        return 0;
    }

    // OFFSET 0x0205ECA0
    public static void PlayerData_SetState(BufferView? playerData, int playerState)
    {
        if (playerData != null)
        {
            playerData.W32(PlayerDataStateOffset, (uint)playerState);
        }
    }

    // OFFSET 0x0205ECA8
    public static void PlayerAvatar_SetPlayerDataState(PlayerAvatar playerAvatar, int playerState)
    {
        PlayerData_SetState(PlayerAvatar_PlayerData(playerAvatar), playerState);
    }

    // OFFSET 0x0205ECB8
    public static void PlayerAvatar_SetPosDirFromVec(PlayerAvatar playerAvatar, in VecFx32 position, int direction)
    {
        MapObject.MapObject_SetPosDirFromVec(PlayerAvatar_GetMapObject(playerAvatar), in position, direction);
        PlayerAvatar_SetMoveState(playerAvatar, MoveStateNone);
        PlayerAvatar_SetPlayerMoveState(playerAvatar, PlayerMoveStateNone);
    }

    // OFFSET 0x0205ECE0
    public static void PlayerAvatar_SetPosDirFromCoords(PlayerAvatar playerAvatar, int x, int z, int direction)
    {
        MapObject.MapObject_SetPosDirFromCoords(PlayerAvatar_GetMapObject(playerAvatar), x, 0, z, direction);
        PlayerAvatar_SetMoveState(playerAvatar, MoveStateNone);
        PlayerAvatar_SetPlayerMoveState(playerAvatar, PlayerMoveStateNone);
    }

    // OFFSET 0x0205ED0C
    public static void Player_SetYPos(PlayerAvatar playerAvatar, int y)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        position.Y = y;
        MapObject.MapObject_SetPos(mapObject, in position);
    }

    // OFFSET 0x0205ED2C
    public static void PlayerAvatar_SetHeightCalculationEnabled(PlayerAvatar playerAvatar, int enabled)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        if (enabled == 1)
        {
            MapObject.MapObject_SetHeightCalculationDisabled(mapObject, 0);
        }
        else
        {
            MapObject.MapObject_SetHeightCalculationDisabled(mapObject, 1);
        }
    }

    // OFFSET 0x0205ED48
    public static void PlayerAvatar_SetHeightCalculationEnabledAndUpdate(PlayerAvatar playerAvatar, int enabled)
    {
        MapObject mapObject = PlayerAvatar_GetMapObject(playerAvatar);
        if (enabled == 1)
        {
            MapObject.MapObject_SetHeightCalculationDisabled(mapObject, 0);
            MapObjectMove.MapObject_RecalculateObjectHeight(mapObject);
        }
        else
        {
            MapObject.MapObject_SetHeightCalculationDisabled(mapObject, 1);
        }
    }

    // OFFSET 0x0205ED6C
    public static int Player_GetSpriteFromStateAndGender(int playerState, int gender)
    {
        if (gender == 0)
        {
            switch (playerState)
            {
                case 0:
                    return 0x00;
                case 1:
                    return 0x15;
                case 2:
                    return 0xB2;
                case 16:
                    return 0xB0;
                case 17:
                    return 0xB4;
                case 18:
                    return 0xBA;
                case 19:
                    return 0xBC;
                case 20:
                    return 0xC4;
                case 21:
                    return 0xC6;
                case 22:
                    return 0xC8;
                case 23:
                    return 0x100;
                case 24:
                    return 0xD4;
                case 25:
                    return 0x102;
                case 26:
                    return 0x10C;
                case 27:
                    return 0x10E;
                case 28:
                    return 0xD2;
                case 29:
                    return 0x110;
                case 30:
                    return 0x112;
            }
        }
        else
        {
            switch (playerState)
            {
                case 0:
                    return 0x61;
                case 1:
                    return 0x62;
                case 2:
                    return 0xB3;
                case 16:
                    return 0xB1;
                case 17:
                    return 0xB5;
                case 18:
                    return 0xBB;
                case 19:
                    return 0xBD;
                case 20:
                    return 0xC5;
                case 21:
                    return 0xC7;
                case 22:
                    return 0xC9;
                case 23:
                    return 0x101;
                case 24:
                    return 0x107;
                case 25:
                    return 0x103;
                case 26:
                    return 0x10D;
                case 27:
                    return 0x10F;
                case 28:
                    return 0xD3;
                case 29:
                    return 0x111;
                case 30:
                    return 0x113;
            }
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x0205EED8
    public static uint Player_ConvertStateToTransition(int playerState)
    {
        switch (playerState)
        {
            case 0:
                return 0x001;
            case 1:
                return 0x002;
            case 2:
                return 0x004;
            case 17:
                return 0x010;
            case 19:
                return 0x020;
            case 20:
                return 0x040;
            case 21:
                return 0x080;
            case 22:
                return 0x100;
        }
        ErrorHandling.GF_AssertFail();
        return 0x001;
    }

    // OFFSET 0x0205EF3C
    public static PlayerAvatar? FieldSystem_GetPlayerAvatar(FieldSystem fieldSystem)
    {
        return (PlayerAvatar?)fieldSystem.PlayerAvatar;
    }

    // OFFSET 0x0205EF40
    public static void PlayerAvatar_SetForceMovement(PlayerAvatar playerAvatar, int enabled)
    {
        if (enabled == 1)
        {
            PlayerAvatar_TurnOnMovementFlag(playerAvatar, MovementFlagForced);
        }
        else
        {
            PlayerAvatar_TurnOffMovementFlag(playerAvatar, MovementFlagForced);
        }
    }

    // OFFSET 0x0205EF58
    public static int PlayerAvatar_CheckForcedMovement(PlayerAvatar playerAvatar)
    {
        if (PlayerAvatar_CheckMovementFlag(playerAvatar, MovementFlagForced) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205EF6C
    public static void PlayerAvatar_SetIgnoreTileBehavior(PlayerAvatar playerAvatar, int enabled)
    {
        if (enabled == 1)
        {
            PlayerAvatar_TurnOnMovementFlag(playerAvatar, MovementFlagIgnoreTileBehavior);
        }
        else
        {
            PlayerAvatar_TurnOffMovementFlag(playerAvatar, MovementFlagIgnoreTileBehavior);
        }
    }

    // OFFSET 0x0205EF84
    public static int PlayerAvatar_CheckIgnoreTileBehavior(PlayerAvatar playerAvatar)
    {
        if (PlayerAvatar_CheckMovementFlag(playerAvatar, MovementFlagIgnoreTileBehavior) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205EF98
    public static void PlayerAvatar_SetBikeBrake(PlayerAvatar playerAvatar, int enabled)
    {
        if (enabled == 1)
        {
            PlayerAvatar_TurnOnMovementFlag(playerAvatar, MovementFlagBikeBrake);
        }
        else
        {
            PlayerAvatar_TurnOffMovementFlag(playerAvatar, MovementFlagBikeBrake);
        }
    }

    // OFFSET 0x0205EFB0
    public static int PlayerAvatar_CheckBikeBrake(PlayerAvatar playerAvatar)
    {
        if (PlayerAvatar_CheckMovementFlag(playerAvatar, MovementFlagBikeBrake) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205EFC4
    public static void PlayerAvatar_SetOnCyclingRoad(PlayerAvatar playerAvatar, int enabled)
    {
        if (enabled == 1)
        {
            PlayerAvatar_TurnOnMovementFlag(playerAvatar, MovementFlagOnCyclingRoad);
        }
        else
        {
            PlayerAvatar_TurnOffMovementFlag(playerAvatar, MovementFlagOnCyclingRoad);
        }
    }

    // OFFSET 0x0205EFDC
    public static int PlayerAvatar_IsOnCyclingRoad(PlayerAvatar playerAvatar)
    {
        if (PlayerAvatar_CheckMovementFlag(playerAvatar, MovementFlagOnCyclingRoad) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205EFF0
    public static void PlayerAvatar_SetEscapedFromDeepMud(PlayerAvatar playerAvatar, int enabled)
    {
        if (enabled == 1)
        {
            PlayerAvatar_TurnOnMovementFlag(playerAvatar, MovementFlagEscapedFromDeepMud);
        }
        else
        {
            PlayerAvatar_TurnOffMovementFlag(playerAvatar, MovementFlagEscapedFromDeepMud);
        }
    }

    // OFFSET 0x0205F008
    public static int PlayerAvatar_CheckEscapedFromDeepMud(PlayerAvatar playerAvatar)
    {
        if (PlayerAvatar_CheckMovementFlag(playerAvatar, MovementFlagEscapedFromDeepMud) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205F01C
    public static void PlayerAvatar_SetMoveThroughMudFlag(PlayerAvatar playerAvatar, int enabled)
    {
        if (enabled == 1)
        {
            PlayerAvatar_TurnOnMovementFlag(playerAvatar, MovementFlagMoveThroughMud);
        }
        else
        {
            PlayerAvatar_TurnOffMovementFlag(playerAvatar, MovementFlagMoveThroughMud);
        }
    }

    // OFFSET 0x0205F034
    public static int PlayerAvatar_CheckMoveThroughMudFlag(PlayerAvatar playerAvatar)
    {
        if (PlayerAvatar_CheckMovementFlag(playerAvatar, MovementFlagMoveThroughMud) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205F048
    public static void PlayerAvatar_StartStep(PlayerAvatar playerAvatar)
    {
        PlayerAvatar_TurnOnMovementFlag(playerAvatar, MovementFlagStep);
    }

    // OFFSET 0x0205F054
    public static void PlayerAvatar_EndStep(PlayerAvatar playerAvatar)
    {
        PlayerAvatar_TurnOffMovementFlag(playerAvatar, MovementFlagStep);
    }

    // OFFSET 0x0205F060
    public static int PlayerAvatar_CheckStep(PlayerAvatar playerAvatar)
    {
        if (PlayerAvatar_CheckMovementFlag(playerAvatar, MovementFlagStep) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205F074
    public static void PlayerAvatar_SetForceMovementRetainSpeed(PlayerAvatar playerAvatar, int enabled)
    {
        if (enabled == 1)
        {
            PlayerAvatar_TurnOnMovementFlag(playerAvatar, MovementFlagForcedRetainSpeed);
        }
        else
        {
            PlayerAvatar_TurnOffMovementFlag(playerAvatar, MovementFlagForcedRetainSpeed);
        }
    }

    // OFFSET 0x0205F08C
    public static uint PlayerAvatar_CheckForcedMovementRetainSpeed(PlayerAvatar playerAvatar)
    {
        return PlayerAvatar_CheckMovementFlag(playerAvatar, MovementFlagForcedRetainSpeed);
    }

    // OFFSET 0x0205F098
    public static void PlayerAvatar_SetDistortionState(PlayerAvatar playerAvatar, int distortionState)
    {
        PlayerAvatar_TurnOffMovementFlag(playerAvatar, MovementFlagDistortionMask);
        switch (distortionState)
        {
            case DistortionStateNone:
                break;
            case DistortionStateActive:
                PlayerAvatar_TurnOnMovementFlag(playerAvatar, MovementFlagDistortionActive);
                break;
            case DistortionStateFloor:
                PlayerAvatar_TurnOnMovementFlag(playerAvatar, MovementFlagDistortionFloor);
                break;
            case DistortionStateWestWall:
                PlayerAvatar_TurnOnMovementFlag(playerAvatar, MovementFlagDistortionWestWall);
                break;
            case DistortionStateEastWall:
                PlayerAvatar_TurnOnMovementFlag(playerAvatar, MovementFlagDistortionEastWall);
                break;
            case DistortionStateCeiling:
                PlayerAvatar_TurnOnMovementFlag(playerAvatar, MovementFlagDistortionCeiling);
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
    }

    // OFFSET 0x0205F108
    public static int PlayerAvatar_MapDistortionState(PlayerAvatar playerAvatar)
    {
        int distortionState = DistortionStateNone;
        switch (PlayerAvatar_CheckMovementFlag(playerAvatar, MovementFlagDistortionMask))
        {
            case MovementFlagDistortionActive:
                distortionState = DistortionStateActive;
                break;
            case MovementFlagDistortionFloor:
                distortionState = DistortionStateFloor;
                break;
            case MovementFlagDistortionWestWall:
                distortionState = DistortionStateWestWall;
                break;
            case MovementFlagDistortionEastWall:
                distortionState = DistortionStateEastWall;
                break;
            case MovementFlagDistortionCeiling:
                distortionState = DistortionStateCeiling;
                break;
        }
        return distortionState;
    }

    // OFFSET 0x0205F158
    public static int PlayerAvatar_DistortionGravityChanged(PlayerAvatar playerAvatar)
    {
        if ((uint)PlayerAvatar_MapDistortionState(playerAvatar) > DistortionStateActive)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205F16C
    public static int PlayerAvatar_DistortionStateOnFloor(PlayerAvatar playerAvatar)
    {
        if ((uint)PlayerAvatar_MapDistortionState(playerAvatar) <= DistortionStateFloor)
        {
            return 1;
        }
        return 0;
    }
}
