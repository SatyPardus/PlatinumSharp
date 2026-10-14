using static PlatinumSharp.Game.MapObject;
using static PlatinumSharp.Game.MapObjectBuffer;
using static PlatinumSharp.Game.MapObjectMove;
using static PlatinumSharp.Game.MovementActions;

namespace PlatinumSharp.Game;

public static class MapObjectFollowMovement
{
    private const int FollowStateOffset = 0x0;
    private const int FollowActiveOffset = 0x1;
    private const int FollowXOffset = 0x2;
    private const int FollowZOffset = 0x4;
    private const int FollowUnusedOffset = 0x6;
    private const int FollowPlayerDataSize = 8;
    private const int FollowPartnerDataSize = 0xC;

    private const int DisguiseStateOffset = 0x0;
    private const int DisguiseKindOffset = 0x1;
    private const int DisguiseRevealedOffset = 0x2;
    private const int DisguiseDataSize = 8;

    private const int CopyStateOffset = 0x0;
    private const int CopyPlayerDirOffset = 0x1;
    private const int CopyGrassOnlyOffset = 0x2;
    private const int CopyDataSize = 4;

    private const int WallStateOffset = 0x0;
    private const int WallModeOffset = 0x4;
    private const int WallHandOffset = 0x8;
    private const int WallDataSize = 0xC;
    private const int WallModeSwitchHands = 2;

    private const uint DisguiseJumpOffsetAddress = 0x020EF8F4;
    private const uint DisguiseReturnJumpOffsetAddress = 0x020EF900;
    private const uint OtherHandAddress = 0x020EF8E4;
    private const uint WallSideDxAddress = 0x020EF92C;
    private const uint WallSideDzAddress = 0x020EF94C;
    private const uint WallCornerDirAddress = 0x020EF96C;
    private const uint WallDiagonalDxAddress = 0x020EF98C;
    private const uint WallDiagonalDzAddress = 0x020EF9AC;
    private const uint WallTurnBackDirAddress = 0x020EF9CC;

    private const uint StatusHideShadow = 1u << 20;
    private const uint StatusFollowBlocked = 0x1840;
    private const int ActionFace = 0x0;
    private const int ActionWalkNormal = 0xC;
    private const int ActionWalkOnSpotSlow = 0x1C;
    private const int PlayerActionFirstSpecial = 0x58;
    private const int NoWall = -1;

    private static readonly MovementTypeStateFunc[] sFollowPlayerStates =
    {
        MovementType_FollowPlayer_StateCheck,
        MovementType_FollowPlayer_StateWaitAction,
    };

    private static readonly MovementTypeStateFunc[] sFollowPartnerStates =
    {
        MovementType_FollowPartner_StateCheck,
        MovementType_FollowPartner_StateWaitAction,
    };

    private static readonly MovementTypeStateFunc[] sDisguiseStates =
    {
        MovementType_Disguise_StateInit,
        MovementType_Disguise_StateHidden,
    };

    private static readonly MovementTypeStateFunc[] sCopyPlayerStates =
    {
        MovementType_CopyPlayer_StateFace,
        MovementType_CopyPlayer_StateWaitFace,
        MovementType_CopyPlayer_StateWaitPlayer,
        MovementType_CopyPlayer_StateCopy,
        MovementType_CopyPlayer_StateWaitAction,
    };

    private static readonly MovementTypeStateFunc[] sWallFollowStates =
    {
        MovementType_WallFollow_StateStart,
        MovementType_WallFollow_StateStep,
        MovementType_WallFollow_StateWaitAction,
    };

    public static bool Dispatch(uint address, MapObject mapObject)
    {
        switch (address & ~1u)
        {
            case 0x02069BE0:
                MovementType_FollowPlayer_Init(mapObject);
                return true;
            case 0x02069C0C:
                MovementType_FollowPlayer_Move(mapObject);
                return true;
            case 0x02069C44:
                MovementType_FollowPlayer_Exit(mapObject);
                return true;
            case 0x02069DF4:
                MovementType_FollowPartner_Init(mapObject);
                return true;
            case 0x02069E1C:
                MovementType_FollowPartner_Move(mapObject);
                return true;
            case 0x02069E4C:
                MovementType_FollowPartner_Exit(mapObject);
                return true;
            case 0x02069E50:
                MovementType_FollowPartner_Return(mapObject);
                return true;
            case 0x0206A104:
                MovementType_DisguiseSnow_Init(mapObject);
                return true;
            case 0x0206A110:
                MovementType_DisguiseSand_Init(mapObject);
                return true;
            case 0x0206A11C:
                MovementType_DisguiseRock_Init(mapObject);
                return true;
            case 0x0206A128:
                MovementType_DisguiseGrass_Init(mapObject);
                return true;
            case 0x0206A134:
                MovementType_Disguise_Move(mapObject);
                return true;
            case 0x0206A158:
                MovementType_Disguise_Exit(mapObject);
                return true;
            case 0x0206A168:
                MovementType_Disguise_Return(mapObject);
                return true;
            case 0x0206A25C:
                MovementType_CopyPlayerNorth_Init(mapObject);
                return true;
            case 0x0206A268:
                MovementType_CopyPlayerSouth_Init(mapObject);
                return true;
            case 0x0206A274:
                MovementType_CopyPlayerWest_Init(mapObject);
                return true;
            case 0x0206A280:
                MovementType_CopyPlayerEast_Init(mapObject);
                return true;
            case 0x0206A28C:
                MovementType_CopyPlayerInGrassNorth_Init(mapObject);
                return true;
            case 0x0206A298:
                MovementType_CopyPlayerInGrassSouth_Init(mapObject);
                return true;
            case 0x0206A2A4:
                MovementType_CopyPlayerInGrassWest_Init(mapObject);
                return true;
            case 0x0206A2B0:
                MovementType_CopyPlayerInGrassEast_Init(mapObject);
                return true;
            case 0x0206A2BC:
                MovementType_CopyPlayer_Move(mapObject);
                return true;
            case 0x0206A490:
                MovementType_WallFollowLeft_Init(mapObject);
                return true;
            case 0x0206A49C:
                MovementType_WallFollowRight_Init(mapObject);
                return true;
            case 0x0206A4A8:
                MovementType_WallFollowSwitchLeft_Init(mapObject);
                return true;
            case 0x0206A4B4:
                MovementType_WallFollowSwitchRight_Init(mapObject);
                return true;
            case 0x0206A4C0:
                MovementType_WallFollow_Move(mapObject);
                return true;
        }
        return false;
    }

    private static void RunStates(MovementTypeStateFunc[] states, MapObject mapObject, byte[] data, int state)
    {
        while (states[state](mapObject, data) == 1)
        {
            state = U8(data, 0);
        }
    }

    // OFFSET 0x02069BE0
    public static void MovementType_FollowPlayer_Init(MapObject mapObject)
    {
        byte[] data = MapObject_InitMovementTypeData(mapObject, FollowPlayerDataSize);
        FollowPlayer_CheckPlayerExists(mapObject, data);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_SetNotMoving(mapObject);
        MapObject_SetStatus18Off(mapObject, 0);
    }

    // OFFSET 0x02069C0C
    public static void MovementType_FollowPlayer_Move(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        if (FollowPlayer_CheckPlayerExists(mapObject, data) == 0)
        {
            return;
        }
        MapObject_SetStatus18Off(mapObject, 0);
        RunStates(sFollowPlayerStates, mapObject, data, U8(data, FollowStateOffset));
    }

    // OFFSET 0x02069C44
    public static void MovementType_FollowPlayer_Exit(MapObject mapObject)
    {
    }

    // OFFSET 0x02069C48
    private static int MovementType_FollowPlayer_StateCheck(MapObject mapObject, byte[] data)
    {
        MapObject_SetNotMoving(mapObject);
        MapObject_SetEndMovementOff(mapObject);
        if (FollowPlayer_HasPlayerMoved(mapObject, data) != 1)
        {
            return 0;
        }
        FollowPlayer_UpdatePlayerPos(mapObject, data);
        if (FollowPlayer_TryStep(mapObject) != 1)
        {
            return 0;
        }
        MapObject_SetMoving(mapObject);
        W8(data, FollowStateOffset, U8(data, FollowStateOffset) + 1);
        return 1;
    }

    // OFFSET 0x02069C8C
    private static int MovementType_FollowPlayer_StateWaitAction(MapObject mapObject, byte[] data)
    {
        if (MapObject_DoMovementActionAndCheckEnd(mapObject) == 1)
        {
            MapObject_SetNotMoving(mapObject);
            W8(data, FollowStateOffset, 0);
        }
        return 0;
    }

    // OFFSET 0x02069CA8
    private static int FollowPlayer_CheckPlayerExists(MapObject mapObject, byte[] data)
    {
        if (PlayerAvatar.MapObjectMan_GetPlayerMapObject(MapObject_MapObjectManager(mapObject)) == null)
        {
            W8(data, FollowActiveOffset, 0);
            return 0;
        }
        if (U8(data, FollowActiveOffset) == 0)
        {
            FollowPlayer_InitPlayerPos(mapObject, data);
        }
        return 1;
    }

    // OFFSET 0x02069CD4
    private static void FollowPlayer_InitPlayerPos(MapObject mapObject, byte[] data)
    {
        PlayerAvatar playerAvatar = PlayerAvatar.FieldSystem_GetPlayerAvatar(MapObject_FieldSystem(mapObject)!)!;
        W8(data, FollowActiveOffset, 1);
        W16(data, FollowXOffset, PlayerAvatar.PlayerAvatar_GetXPos(playerAvatar));
        W16(data, FollowZOffset, PlayerAvatar.PlayerAvatar_GetZPos(playerAvatar));
        W16(data, FollowUnusedOffset, 0xFF);
    }

    // OFFSET 0x02069CFC
    private static int FollowPlayer_HasPlayerMoved(MapObject mapObject, byte[] data)
    {
        PlayerAvatar? playerAvatar = PlayerAvatar.FieldSystem_GetPlayerAvatar(MapObject_FieldSystem(mapObject)!);
        if (playerAvatar == null)
        {
            return 0;
        }
        int x = PlayerAvatar.PlayerAvatar_GetXPos(playerAvatar);
        int z = PlayerAvatar.PlayerAvatar_GetZPos(playerAvatar);
        if (x != S16(data, FollowXOffset) || z != S16(data, FollowZOffset))
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02069D30
    private static void FollowPlayer_UpdatePlayerPos(MapObject mapObject, byte[] data)
    {
        PlayerAvatar playerAvatar = PlayerAvatar.FieldSystem_GetPlayerAvatar(MapObject_FieldSystem(mapObject)!)!;
        W16(data, FollowXOffset, PlayerAvatar.PlayerAvatar_GetXPos(playerAvatar));
        W16(data, FollowZOffset, PlayerAvatar.PlayerAvatar_GetZPos(playerAvatar));
    }

    // OFFSET 0x02069D50
    private static int FollowPlayer_GetStepAction(MapObject mapObject)
    {
        PlayerAvatar playerAvatar = PlayerAvatar.FieldSystem_GetPlayerAvatar(MapObject_FieldSystem(mapObject)!)!;
        int action = (int)PlayerAvatar.PlayerAvatar_GetMovementAction(playerAvatar);
        switch (action - PlayerActionFirstSpecial)
        {
            case 0:
                return 0x10;
            case 1:
                return 0x11;
            case 2:
                return 0x12;
            case 3:
                return 0x13;
        }
        return action;
    }

    // OFFSET 0x02069D8C
    private static int FollowPlayer_TryStep(MapObject mapObject)
    {
        PlayerAvatar playerAvatar = PlayerAvatar.FieldSystem_GetPlayerAvatar(MapObject_FieldSystem(mapObject)!)!;
        int x = MapObject_GetX(mapObject);
        int z = MapObject_GetZ(mapObject);
        int playerX = PlayerAvatar.PlayerAvatar_XPosPrev(playerAvatar);
        int playerZ = PlayerAvatar.PlayerAvatar_ZPosPrev(playerAvatar);
        if (x == playerX && z == playerZ)
        {
            return 0;
        }
        int action = FollowPlayer_GetStepAction(mapObject);
        int dir = GetDirectionBetweenPoints(x, z, playerX, playerZ);
        MapObject_SetMovementActionForMove(mapObject, MovementAction_TurnActionTowardsDir(dir, action));
        return 1;
    }

    // OFFSET 0x02069DF4
    public static void MovementType_FollowPartner_Init(MapObject mapObject)
    {
        byte[] data = MapObject_InitMovementTypeData(mapObject, FollowPartnerDataSize);
        mapObject.MovementTypePartner = null;
        FollowPartner_FindPartner(mapObject, data);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_SetNotMoving(mapObject);
        W8(data, FollowActiveOffset, 0);
    }

    // OFFSET 0x02069E1C
    public static void MovementType_FollowPartner_Move(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        if (FollowPartner_FindPartner(mapObject, data) == 0)
        {
            return;
        }
        RunStates(sFollowPartnerStates, mapObject, data, U8(data, FollowStateOffset));
    }

    // OFFSET 0x02069E4C
    public static void MovementType_FollowPartner_Exit(MapObject mapObject)
    {
    }

    // OFFSET 0x02069E50
    public static void MovementType_FollowPartner_Return(MapObject mapObject)
    {
        W8(MapObject_GetMovementTypeData(mapObject), FollowActiveOffset, 0);
    }

    // OFFSET 0x02069E5C
    private static int MovementType_FollowPartner_StateCheck(MapObject mapObject, byte[] data)
    {
        MapObject_SetNotMoving(mapObject);
        MapObject_SetEndMovementOff(mapObject);
        if (FollowPartner_HasPartnerMoved(mapObject, data) != 1 || FollowPartner_TryStep(mapObject, data) != 1)
        {
            return 0;
        }
        MapObject_SetMoving(mapObject);
        W8(data, FollowStateOffset, U8(data, FollowStateOffset) + 1);
        return 1;
    }

    // OFFSET 0x02069E98
    private static int MovementType_FollowPartner_StateWaitAction(MapObject mapObject, byte[] data)
    {
        if (MapObject_DoMovementActionAndCheckEnd(mapObject) == 0)
        {
            return 0;
        }
        MapObject_SetNotMoving(mapObject);
        W8(data, FollowStateOffset, 0);
        return 0;
    }

    // OFFSET 0x02069F48
    private static int FollowPartner_FindPartner(MapObject mapObject, byte[] data)
    {
        MapObjectManager mapObjectManager = MapObject_MapObjectManager(mapObject);
        int index = 0;
        int mapHeaderID = MapObject_GetMapHeaderID(mapObject);
        int trainerID = TrainerEncounter.MapObject_GetTrainerIDFromScript(mapObject);
        while (MapObjectManager.MapObjectMan_FindObjectWithStatus(mapObjectManager, out MapObject? other, ref index, 1) == 1)
        {
            if (mapObject != other
                && mapHeaderID == MapObject_GetMapHeaderID(other!)
                && trainerID == TrainerEncounter.MapObject_GetTrainerIDFromScript(other!))
            {
                if (U8(data, FollowActiveOffset) == 0)
                {
                    FollowPartner_InitPartner(mapObject, data, other!);
                }
                return 1;
            }
        }
        W8(data, FollowActiveOffset, 0);
        return 0;
    }

    // OFFSET 0x02069FC4
    private static void FollowPartner_InitPartner(MapObject mapObject, byte[] data, MapObject partner)
    {
        W8(data, FollowActiveOffset, 1);
        W16(data, FollowXOffset, MapObject_GetX(partner));
        W16(data, FollowZOffset, MapObject_GetZ(partner));
        W16(data, FollowUnusedOffset, 0xFF);
        mapObject.MovementTypePartner = partner;
    }

    // OFFSET 0x02069FE8
    private static int FollowPartner_HasPartnerMoved(MapObject mapObject, byte[] data)
    {
        MapObject partner = mapObject.MovementTypePartner!;
        int x = MapObject_GetX(mapObject);
        int z = MapObject_GetZ(mapObject);
        int partnerX = MapObject_GetXPrev(partner);
        int partnerZ = MapObject_GetZPrev(partner);
        if (x == partnerX && z == partnerZ)
        {
            return 0;
        }
        if (MapObject_IsMoving(partner) == 1 || MapObject_CheckStatus(partner, StatusFollowBlocked) == 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0206A034
    private static int FollowPartner_TryStep(MapObject mapObject, byte[] data)
    {
        MapObject partner = mapObject.MovementTypePartner!;
        int x = MapObject_GetX(mapObject);
        int z = MapObject_GetZ(mapObject);
        int partnerX = MapObject_GetX(partner);
        int partnerZ = MapObject_GetZ(partner);
        int partnerPrevX = MapObject_GetXPrev(partner);
        int partnerPrevZ = MapObject_GetZPrev(partner);
        if (x == partnerX && z == partnerZ)
        {
            return 0;
        }
        int dir = GetDirectionBetweenPoints(x, z, partnerPrevX, partnerPrevZ);
        int nextX = x + MapObject_GetDxFromDir(dir);
        int nextZ = z + MapObject_GetDzFromDir(dir);
        if (nextX == partnerX && nextZ == partnerZ)
        {
            return 0;
        }
        MapObject_SetMovementActionForMove(mapObject, MovementAction_TurnActionTowardsDir(dir, ActionWalkNormal));
        return 1;
    }

    private static VecFx32 ReadJumpOffset(uint address)
    {
        uint[] raw = RomData.Arm9U32(address, 3);
        return new VecFx32 { X = (int)raw[0], Y = (int)raw[1], Z = (int)raw[2] };
    }

    // OFFSET 0x0206A0BC
    private static void MovementType_Disguise_Init(MapObject mapObject, int kind)
    {
        byte[] data = MapObject_InitMovementTypeData(mapObject, DisguiseDataSize);
        mapObject.MovementTypeAnimManager = null;
        W8(data, DisguiseKindOffset, kind);
        MapObject_SetDrawState(mapObject, 0);
        MapObject_SetNotMoving(mapObject);
        MapObject_SetStatusFlagOn(mapObject, StatusHideShadow);
        MapObject_SetSpriteJumpOffset(mapObject, ReadJumpOffset(DisguiseJumpOffsetAddress));
    }

    // OFFSET 0x0206A104
    public static void MovementType_DisguiseSnow_Init(MapObject mapObject)
    {
        MovementType_Disguise_Init(mapObject, 0);
    }

    // OFFSET 0x0206A110
    public static void MovementType_DisguiseSand_Init(MapObject mapObject)
    {
        MovementType_Disguise_Init(mapObject, 1);
    }

    // OFFSET 0x0206A11C
    public static void MovementType_DisguiseRock_Init(MapObject mapObject)
    {
        MovementType_Disguise_Init(mapObject, 2);
    }

    // OFFSET 0x0206A128
    public static void MovementType_DisguiseGrass_Init(MapObject mapObject)
    {
        MovementType_Disguise_Init(mapObject, 3);
    }

    // OFFSET 0x0206A134
    public static void MovementType_Disguise_Move(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        RunStates(sDisguiseStates, mapObject, data, U8(data, DisguiseStateOffset));
    }

    // OFFSET 0x0206A158
    public static void MovementType_Disguise_Exit(MapObject mapObject)
    {
        OverworldAnimManager? animManager = MapObject_GetDisguiseAnimManager(mapObject);
        if (animManager != null)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
        }
    }

    // OFFSET 0x0206A168
    public static void MovementType_Disguise_Return(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        W8(data, DisguiseStateOffset, 0);
        MapObject_SetDisguiseAnimManager(mapObject, null);
        if (U8(data, DisguiseRevealedOffset) != 0)
        {
            return;
        }
        MapObject_SetSpriteJumpOffset(mapObject, ReadJumpOffset(DisguiseReturnJumpOffsetAddress));
        MapObject_SetStatusFlagOn(mapObject, StatusHideShadow);
    }

    // OFFSET 0x0206A1AC
    private static int MovementType_Disguise_StateInit(MapObject mapObject, byte[] data)
    {
        if (U8(data, DisguiseRevealedOffset) == 0)
        {
            MapObject_SetDisguiseAnimManager(mapObject, TileModelEffect.TileModelEffect_New(mapObject, U8(data, DisguiseKindOffset)));
        }
        MapObject_SetNotMoving(mapObject);
        MapObject_SetEndMovementOff(mapObject);
        W8(data, DisguiseStateOffset, U8(data, DisguiseStateOffset) + 1);
        return 0;
    }

    // OFFSET 0x0206A1DC
    private static int MovementType_Disguise_StateHidden(MapObject mapObject, byte[] data)
    {
        if (U8(data, DisguiseRevealedOffset) != 0)
        {
            return 0;
        }
        if (MapObject_GetDisguiseAnimManager(mapObject) == null && MapObject_IsDrawActive(mapObject) == 1)
        {
            MapObject_SetDisguiseAnimManager(mapObject, TileModelEffect.TileModelEffect_New(mapObject, U8(data, DisguiseKindOffset)));
        }
        MapObject_SetStatusFlagOn(mapObject, StatusHideShadow);
        return 0;
    }

    // OFFSET 0x0206A218
    public static void MapObject_SetDisguiseAnimManager(MapObject mapObject, OverworldAnimManager? animManager)
    {
        MapObject_GetMovementTypeData(mapObject);
        mapObject.MovementTypeAnimManager = animManager;
    }

    // OFFSET 0x0206A224
    public static OverworldAnimManager? MapObject_GetDisguiseAnimManager(MapObject mapObject)
    {
        MapObject_GetMovementTypeData(mapObject);
        return mapObject.MovementTypeAnimManager;
    }

    // OFFSET 0x0206A230
    public static void MapObject_RevealDisguise(MapObject mapObject)
    {
        W8(MapObject_GetMovementTypeData(mapObject), DisguiseRevealedOffset, 1);
    }

    // OFFSET 0x0206A23C
    private static void MovementType_CopyPlayer_Init(MapObject mapObject, int dir, int grassOnly)
    {
        byte[] data = MapObject_InitMovementTypeData(mapObject, CopyDataSize);
        W8(data, CopyPlayerDirOffset, -1);
        W8(data, CopyGrassOnlyOffset, grassOnly);
        MapObject_TryFace(mapObject, 0);
    }

    // OFFSET 0x0206A25C
    public static void MovementType_CopyPlayerNorth_Init(MapObject mapObject)
    {
        MovementType_CopyPlayer_Init(mapObject, 0, 0);
    }

    // OFFSET 0x0206A268
    public static void MovementType_CopyPlayerSouth_Init(MapObject mapObject)
    {
        MovementType_CopyPlayer_Init(mapObject, 1, 0);
    }

    // OFFSET 0x0206A274
    public static void MovementType_CopyPlayerWest_Init(MapObject mapObject)
    {
        MovementType_CopyPlayer_Init(mapObject, 2, 0);
    }

    // OFFSET 0x0206A280
    public static void MovementType_CopyPlayerEast_Init(MapObject mapObject)
    {
        MovementType_CopyPlayer_Init(mapObject, 3, 0);
    }

    // OFFSET 0x0206A28C
    public static void MovementType_CopyPlayerInGrassNorth_Init(MapObject mapObject)
    {
        MovementType_CopyPlayer_Init(mapObject, 0, 1);
    }

    // OFFSET 0x0206A298
    public static void MovementType_CopyPlayerInGrassSouth_Init(MapObject mapObject)
    {
        MovementType_CopyPlayer_Init(mapObject, 1, 1);
    }

    // OFFSET 0x0206A2A4
    public static void MovementType_CopyPlayerInGrassWest_Init(MapObject mapObject)
    {
        MovementType_CopyPlayer_Init(mapObject, 2, 1);
    }

    // OFFSET 0x0206A2B0
    public static void MovementType_CopyPlayerInGrassEast_Init(MapObject mapObject)
    {
        MovementType_CopyPlayer_Init(mapObject, 3, 1);
    }

    // OFFSET 0x0206A2BC
    public static void MovementType_CopyPlayer_Move(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        RunStates(sCopyPlayerStates, mapObject, data, U8(data, CopyStateOffset));
    }

    // OFFSET 0x0206A2E0
    private static int MovementType_CopyPlayer_StateFace(MapObject mapObject, byte[] data)
    {
        MapObject_SetMovementActionForMove(mapObject, MovementAction_TurnActionTowardsDir(MapObject_GetFacingDir(mapObject), ActionFace));
        MapObject_SetNotMoving(mapObject);
        MapObject_SetEndMovementOff(mapObject);
        W8(data, CopyStateOffset, 1);
        return 1;
    }

    // OFFSET 0x0206A30C
    private static int MovementType_CopyPlayer_StateWaitFace(MapObject mapObject, byte[] data)
    {
        if (MapObject_DoMovementActionAndCheckEnd(mapObject) == 1)
        {
            W8(data, CopyStateOffset, 2);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0206A324
    private static int MovementType_CopyPlayer_StateWaitPlayer(MapObject mapObject, byte[] data)
    {
        if (S8(data, CopyPlayerDirOffset) == -1)
        {
            W8(data, CopyPlayerDirOffset, PlayerAvatar.PlayerAvatar_GetFacingDir(MapObject_FieldSystem(mapObject)!.PlayerAvatar!));
        }
        MapObject_SetNotMoving(mapObject);
        MapObject_SetEndMovementOff(mapObject);
        W8(data, CopyStateOffset, 3);
        return 1;
    }

    // OFFSET 0x0206A354
    private static uint CopyPlayer_CheckCollisionOrNotGrass(MapObject mapObject, int dir)
    {
        uint result = (uint)MapObject_GetTileBehaviorFromDir(mapObject, dir);
        if (TileBehavior.TileBehavior_IsVeryTallGrass((byte)result) == 0)
        {
            result = CollisionTerrain;
        }
        return MapObject_CheckCollisionInDir(mapObject, dir) | result;
    }

    // OFFSET 0x0206A37C
    private static void CopyPlayer_TryStep(MapObject mapObject, int dir, int action, int grassOnly)
    {
        uint collision;
        if (grassOnly == 0)
        {
            collision = MapObject_CheckCollisionInDir(mapObject, dir);
        }
        else
        {
            collision = CopyPlayer_CheckCollisionOrNotGrass(mapObject, dir);
        }
        if (collision != 0)
        {
            action = MovementAction_TurnActionTowardsDir(dir, ActionFace);
        }
        else
        {
            action = MovementAction_TurnActionTowardsDir(dir, action);
            MapObject_SetMoving(mapObject);
        }
        MapObject_SetMovementActionForMove(mapObject, action);
    }

    // OFFSET 0x0206A3BC
    private static int MovementType_CopyPlayer_StateCopy(MapObject mapObject, byte[] data)
    {
        FieldSystem fieldSystem = MapObject_FieldSystem(mapObject)!;
        int dir = PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!);
        switch (PlayerAvatar.PlayerAvatar_GetMovementActionSpeed(fieldSystem.PlayerAvatar!))
        {
            case 0:
            case 1:
                MapObject_SetMovementActionForMove(mapObject, MovementAction_TurnActionTowardsDir(dir, ActionFace));
                break;
            case 2:
                CopyPlayer_TryStep(mapObject, dir, 4, U8(data, CopyGrassOnlyOffset));
                break;
            case 3:
                CopyPlayer_TryStep(mapObject, dir, 8, U8(data, CopyGrassOnlyOffset));
                break;
            case 4:
                CopyPlayer_TryStep(mapObject, dir, 0xC, U8(data, CopyGrassOnlyOffset));
                break;
            case 5:
                CopyPlayer_TryStep(mapObject, dir, 0x10, U8(data, CopyGrassOnlyOffset));
                break;
            case 6:
                CopyPlayer_TryStep(mapObject, dir, 0x14, U8(data, CopyGrassOnlyOffset));
                break;
        }
        W8(data, CopyStateOffset, 4);
        return 1;
    }

    // OFFSET 0x0206A454
    private static int MovementType_CopyPlayer_StateWaitAction(MapObject mapObject, byte[] data)
    {
        if (MapObject_DoMovementActionAndCheckEnd(mapObject) == 0)
        {
            return 0;
        }
        MapObject_SetNotMoving(mapObject);
        MapObject_SetEndMovementOff(mapObject);
        W8(data, CopyStateOffset, 2);
        return 0;
    }

    // OFFSET 0x0206A47C
    private static void MovementType_WallFollow_Init(MapObject mapObject, int mode, int hand)
    {
        byte[] data = MapObject_InitMovementTypeData(mapObject, WallDataSize);
        W32(data, WallModeOffset, mode);
        W32(data, WallHandOffset, hand);
    }

    // OFFSET 0x0206A490
    public static void MovementType_WallFollowLeft_Init(MapObject mapObject)
    {
        MovementType_WallFollow_Init(mapObject, 0, 0);
    }

    // OFFSET 0x0206A49C
    public static void MovementType_WallFollowRight_Init(MapObject mapObject)
    {
        MovementType_WallFollow_Init(mapObject, 1, 1);
    }

    // OFFSET 0x0206A4A8
    public static void MovementType_WallFollowSwitchLeft_Init(MapObject mapObject)
    {
        MovementType_WallFollow_Init(mapObject, WallModeSwitchHands, 0);
    }

    // OFFSET 0x0206A4B4
    public static void MovementType_WallFollowSwitchRight_Init(MapObject mapObject)
    {
        MovementType_WallFollow_Init(mapObject, WallModeSwitchHands, 1);
    }

    // OFFSET 0x0206A4C0
    public static void MovementType_WallFollow_Move(MapObject mapObject)
    {
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        int state = S32(data, WallStateOffset);
        while (sWallFollowStates[state](mapObject, data) == 1)
        {
            state = S32(data, WallStateOffset);
        }
    }

    // OFFSET 0x0206A4E4
    private static int MovementType_WallFollow_StateStart(MapObject mapObject, byte[] data)
    {
        MapObject_SetNotMoving(mapObject);
        W32(data, WallStateOffset, S32(data, WallStateOffset) + 1);
        return 1;
    }

    // OFFSET 0x0206A4F8
    private static int MovementType_WallFollow_StateStep(MapObject mapObject, byte[] data)
    {
        WallFollow_TryStep(mapObject, data, ActionWalkNormal);
        W32(data, WallStateOffset, S32(data, WallStateOffset) + 1);
        return 1;
    }

    // OFFSET 0x0206A50C
    private static int MovementType_WallFollow_StateWaitAction(MapObject mapObject, byte[] data)
    {
        if (MapObject_DoMovementActionAndCheckEnd(mapObject) == 1)
        {
            W32(data, WallStateOffset, 0);
            return 1;
        }
        return 0;
    }

    private static int DirHandTable(uint address, int dir, int hand)
    {
        return (int)RomData.Arm9U32((uint)(address + dir * 8 + hand * 4), 1)[0];
    }

    // OFFSET 0x0206A524
    private static int WallFollow_CheckSideCollisionAt(FieldSystem fieldSystem, int x, int z, int dir, int hand)
    {
        return TerrainCollisionManager.TerrainCollisionManager_CheckCollision(fieldSystem, x + DirHandTable(WallSideDxAddress, dir, hand), z + DirHandTable(WallSideDzAddress, dir, hand));
    }

    // OFFSET 0x0206A54C
    private static int WallFollow_CheckDiagonalCollisionAt(FieldSystem fieldSystem, int x, int z, int dir, int hand)
    {
        return TerrainCollisionManager.TerrainCollisionManager_CheckCollision(fieldSystem, x + DirHandTable(WallDiagonalDxAddress, dir, hand), z + DirHandTable(WallDiagonalDzAddress, dir, hand));
    }

    // OFFSET 0x0206A574
    private static int WallFollow_CheckSideCollision(MapObject mapObject, int dir, int hand)
    {
        FieldSystem fieldSystem = MapObject_FieldSystem(mapObject)!;
        int x = MapObject_GetX(mapObject);
        int z = MapObject_GetZ(mapObject);
        return WallFollow_CheckSideCollisionAt(fieldSystem, x, z, dir, hand);
    }

    // OFFSET 0x0206A5A4
    private static int WallFollow_CheckDiagonalCollision(MapObject mapObject, int dir, int hand)
    {
        FieldSystem fieldSystem = MapObject_FieldSystem(mapObject)!;
        int x = MapObject_GetX(mapObject);
        int z = MapObject_GetZ(mapObject);
        return WallFollow_CheckDiagonalCollisionAt(fieldSystem, x, z, dir, hand);
    }

    // OFFSET 0x0206A5D4
    private static int WallFollow_GetNextDir(MapObject mapObject, int dir, int hand)
    {
        if (WallFollow_CheckSideCollision(mapObject, dir, hand) != 0)
        {
            return dir;
        }
        if (WallFollow_CheckDiagonalCollision(mapObject, dir, hand) == 0)
        {
            return NoWall;
        }
        return DirHandTable(WallCornerDirAddress, dir, hand);
    }

    // OFFSET 0x0206A60C
    private static uint WallFollow_UpdateDir(MapObject mapObject, ref int dir, int hand)
    {
        dir = WallFollow_GetNextDir(mapObject, dir, hand);
        if (dir == NoWall)
        {
            return 0;
        }
        return MapObject_CheckCollisionInDir(mapObject, dir);
    }

    private static int WallFollow_StopInPlace(MapObject mapObject)
    {
        MapObject_SetMovementActionForMove(mapObject, MovementAction_TurnActionTowardsDir(MapObject_GetFacingDir(mapObject), ActionWalkOnSpotSlow));
        return 0;
    }

    private static int WallFollow_Step(MapObject mapObject, int dir, int action)
    {
        action = MovementAction_TurnActionTowardsDir(dir, action);
        MapObject_SetMoving(mapObject);
        MapObject_SetMovementActionForMove(mapObject, action);
        return 1;
    }

    // OFFSET 0x0206A630
    private static int WallFollow_TryStep(MapObject mapObject, byte[] data, int action)
    {
        int hand = S32(data, WallHandOffset);
        int dir = MapObject_GetFacingDir(mapObject);
        uint collision = WallFollow_UpdateDir(mapObject, ref dir, hand);
        if (dir == NoWall)
        {
            return WallFollow_StopInPlace(mapObject);
        }
        if (collision == 0)
        {
            return WallFollow_Step(mapObject, dir, action);
        }
        if ((collision & CollisionOutOfRange) != 0 && S32(data, WallModeOffset) == WallModeSwitchHands)
        {
            dir = Direction_GetOpposite(MapObject_GetFacingDir(mapObject));
            hand = (int)RomData.Arm9U32((uint)(OtherHandAddress + hand * 4), 1)[0];
            W32(data, WallHandOffset, hand);
            collision = WallFollow_UpdateDir(mapObject, ref dir, hand);
            if (dir == NoWall)
            {
                return WallFollow_StopInPlace(mapObject);
            }
            if (collision == 0)
            {
                return WallFollow_Step(mapObject, dir, action);
            }
        }
        if ((collision & CollisionTerrain) != 0)
        {
            dir = DirHandTable(WallTurnBackDirAddress, dir, hand);
            collision = WallFollow_UpdateDir(mapObject, ref dir, hand);
            if (dir == NoWall)
            {
                return WallFollow_StopInPlace(mapObject);
            }
            if (collision == 0)
            {
                return WallFollow_Step(mapObject, dir, action);
            }
        }
        return WallFollow_StopInPlace(mapObject);
    }
}
