using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed partial class FieldInput
{
    public const int FlagsOffset = 0x00;
    public const int PlayerDirOffset = 0x02;
    public const int TransitionDirOffset = 0x03;

    public bool Interact;
    public bool EndMovement;
    public bool Menu;
    public bool RegisteredItem;
    public bool Sign;
    public bool MapTransition;
    public bool Movement;
    public bool Dummy1;
    public bool Dummy2;
    public bool Dummy3;
    public bool Dummy4;
    public bool Dummy5;
    public byte PlayerDir;
    public sbyte TransitionDir;
}

public static class FieldControl
{
    private const int PadButtonA = 0x0001;
    private const int PadButtonB = 0x0002;
    private const int PadKeyRight = 0x0010;
    private const int PadKeyLeft = 0x0020;
    private const int PadKeyUp = 0x0040;
    private const int PadKeyDown = 0x0080;
    private const int PadButtonX = 0x0400;
    private const int PadButtonY = 0x0800;
    private const int PadDirectionKeys = 0x00F0;
    private const int DirNone = -1;
    private const int DirNorth = 0;
    private const int DirSouth = 1;
    private const int DirWest = 2;
    private const int DirEast = 3;
    private const int PlayerMoveStateNone = 0;
    private const int PlayerMoveStateEnd = 3;
    private const int MoveStateMoving = 1;
    private const int PlayerStateSurfing = 2;
    private const int DistortionStateFloor = 1;
    private const int DistortionStateNone = 0;
    private const int InitScriptFrame = 1;
    private const int DynamicMapFeaturesHearthomeGym = 2;
    private const int DynamicMapFeaturesCanalaveGym = 3;
    private const int DynamicMapFeaturesVeilstoneGym = 4;
    private const int DynamicMapFeaturesDistortionWorld = 9;
    private const int FieldMoveStrengthActive = 1;
    private const int FieldMoveWaterfallKnown = 2;
    private const int FieldMoveDistortionWorld = 4;
    private const int FlagOpCheck = 2;
    private const int MoveSurf = 0x39;
    private const int MoveWaterfall = 0x7F;
    private const int PartySlotNone = 0xFF;
    private const int BadgeFen = 3;
    private const int TrainerTypeNone = 9;
    private const int MovementTypeNone = 1;
    private const int MapObjectStatusActive = 1;
    private const int ScriptNone = 0xFFFF;
    private const int ScriptUnionRoomDisconnect = 5;
    private const int ScriptPoisonFaint = 0x07D3;
    private const int ScriptPC = 0x07E2;
    private const int ScriptBikeParking = 0x07EE;
    private const int ScriptDaycareEgg = 0x07EF;
    private const int ScriptSmallBookshelf1 = 0x09C4;
    private const int ScriptSmallBookshelf2 = 0x09C5;
    private const int ScriptBookshelf1 = 0x09C6;
    private const int ScriptBookshelf2 = 0x09C7;
    private const int ScriptTrashCan = 0x09C8;
    private const int ScriptMartShelf1 = 0x09C9;
    private const int ScriptMartShelf2 = 0x09CA;
    private const int ScriptMartShelf3 = 0x09CB;
    private const int ScriptTownMap = 0x09CC;
    private const int ScriptSafariOutOfSteps = 0x2261;
    private const int ScriptSafariOutOfBalls = 0x2262;
    private const int ScriptColosseumExit = 0x238D;
    private const int ScriptRockClimb = 0x2713;
    private const int ScriptSurf = 0x2714;
    private const int ScriptWaterfall = 0x2716;
    private const int ScriptTV = 0x2774;
    private const int SeqSeMenuOpen = 0x05FD;
    private const int RecordDaycareEggs = 0x0B;
    private const int TrainerScoreDaycareEgg = 0x0F;
    private const int FriendshipStepInterval = 0x80;
    private const int FriendshipEventWalking = 5;
    private const int PoisonStepMask = 3;
    private const int PoisonResultNone = 0;
    private const int PoisonResultDamaged = 1;
    private const int PoisonResultFainted = 2;
    private const int SafariMaxSteps = 500;
    private const int PoketchEventStep = 5;
    private const int WarpSpecialDestWarpID = 0x100;
    private const int WarpSpecialDestHeaderID = 0xFFF;
    private const int TransitionTypeDoor = 1;
    private const int TransitionTypeEscalator = 2;
    private const int TransitionTypeStairs = 3;
    private const int LocationMapHeaderID = 0x00;
    private const int LocationWarpID = 0x04;
    private const int LocationX = 0x08;
    private const int LocationZ = 0x0C;
    private const int LocationFaceDirection = 0x10;
    private const int LocationSize = 0x14;
    private const int WirelessOtherPlayersMask = 0xFE;
    private const int CommMaxNetID = 5;
    private const int UnionRoomStateMenu = 4;

    private static PlayerAvatar Avatar(FieldSystem fieldSystem)
    {
        return fieldSystem.PlayerAvatar!;
    }

    private static BufferView NewLocation()
    {
        return new BufferView(new byte[LocationSize], 0);
    }

    private static void StopPlayerForInteraction(FieldSystem fieldSystem)
    {
        if (PlayerMove.PlayerAvatar_CheckForceStopMovement(fieldSystem.PlayerAvatar) == 1)
        {
            PlayerMove.PlayerAvatar_ForceStopMovement(fieldSystem.PlayerAvatar, PlayerAvatar.PlayerAvatar_GetFacingDir(Avatar(fieldSystem)));
        }
    }

    // OFFSET 0x021D1C30
    private static void FieldInput_Clear(FieldInput input)
    {
        input.Interact = false;
        input.EndMovement = false;
        input.Menu = false;
        input.RegisteredItem = false;
        input.Sign = false;
        input.MapTransition = false;
        input.Movement = false;
        input.Dummy1 = false;
        input.Dummy2 = false;
        input.Dummy3 = false;
        input.Dummy4 = false;
        input.Dummy5 = false;
        input.PlayerDir = 0xFF;
        input.TransitionDir = DirNone;
    }

    // OFFSET 0x021D1CAC
    public static void FieldInput_Update(FieldInput input, FieldSystem fieldSystem, ushort pressedKeys, ushort heldKeys)
    {
        FieldInput_Clear(input);
        int playerMoveState = PlayerAvatar.PlayerAvatar_GetPlayerMoveState(Avatar(fieldSystem));
        int moveState = PlayerAvatar.PlayerAvatar_GetMoveState(Avatar(fieldSystem));
        int facingDir = PlayerAvatar.PlayerAvatar_GetFacingDir(Avatar(fieldSystem));
        input.PressedKeys = pressedKeys;
        input.HeldKeys = heldKeys;
        if (playerMoveState == PlayerMoveStateEnd || playerMoveState == PlayerMoveStateNone)
        {
            if ((pressedKeys & PadButtonX) != 0)
            {
                input.Menu = true;
            }
            if ((pressedKeys & PadButtonY) != 0)
            {
                input.RegisteredItem = true;
            }
            if ((pressedKeys & PadButtonA) != 0)
            {
                input.Interact = true;
            }
            if ((pressedKeys & PadButtonB) != 0)
            {
                input.Dummy1 = true;
            }
            int heldDirections = heldKeys & PadDirectionKeys;
            if (heldDirections != 0)
            {
                input.Sign = true;
            }
            if (heldDirections != 0)
            {
                input.MapTransition = true;
            }
        }
        if (playerMoveState == PlayerMoveStateEnd && moveState == MoveStateMoving)
        {
            input.Movement = true;
        }
        if (playerMoveState == PlayerMoveStateEnd)
        {
            input.EndMovement = true;
        }
        if ((facingDir == DirNorth && (heldKeys & PadKeyUp) != 0)
            || (facingDir == DirSouth && (heldKeys & PadKeyDown) != 0)
            || (facingDir == DirWest && (heldKeys & PadKeyLeft) != 0)
            || (facingDir == DirEast && (heldKeys & PadKeyRight) != 0))
        {
            input.TransitionDir = (sbyte)facingDir;
        }
        else
        {
            input.TransitionDir = DirNone;
        }
        input.PlayerDir = (byte)PlayerMove.PlayerAvatar_CalcFaceDirection(fieldSystem.PlayerAvatar, pressedKeys, heldKeys);
    }

    // OFFSET 0x021D1DA4
    public static int FieldInput_Process(FieldInput input, FieldSystem fieldSystem)
    {
        if (!input.Dummy5 && ScriptManager.FieldSystem_RunInitScript(fieldSystem, InitScriptFrame))
        {
            return 1;
        }
        if (!input.Dummy5)
        {
            int doubleBattle = FieldParty.Party_HasTwoAliveMons(Party.SaveData_GetParty(fieldSystem.SaveData!));
            if (SystemFlags.SystemFlag_CheckHasPartner(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)))
            {
                doubleBattle = 1;
            }
            if (TrainerEncounter.FieldSystem_CheckForTrainersWantingBattle(fieldSystem, doubleBattle) == 1
                || (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(fieldSystem, DynamicMapFeaturesHearthomeGym) == 1 && ov8_0224C51C(fieldSystem) == 1))
            {
                PlayerMove.PlayerAvatar_ClearMoveState(fieldSystem.PlayerAvatar);
                MapObjectManager.MapObjectMan_PauseAllMovement(fieldSystem.MapObjectManager!);
                return 1;
            }
        }
        if (input.Movement)
        {
            SystemFlags.SystemFlag_ClearStep(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
            if (Field_ProcessStep(fieldSystem) == 1)
            {
                return 1;
            }
        }
        if (!input.Dummy5)
        {
            int fieldMoveFlags = 0;
            int direction = PlayerMove.PlayerAvatar_CalcFaceDirection(fieldSystem.PlayerAvatar, input.PressedKeys, input.HeldKeys);
            if (SystemFlags.SystemFlag_HandleStrengthActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), FlagOpCheck))
            {
                fieldMoveFlags |= FieldMoveStrengthActive;
            }
            if (FieldParty.Party_HasMonWithMove(Party.SaveData_GetParty(fieldSystem.SaveData!), MoveWaterfall) != PartySlotNone)
            {
                fieldMoveFlags |= FieldMoveWaterfallKnown;
            }
            if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(fieldSystem, DynamicMapFeaturesDistortionWorld) == 1 && ov9_02250F74(fieldSystem) == 1)
            {
                fieldMoveFlags |= FieldMoveDistortionWorld;
            }
            if (PlayerFieldMoves.PlayerAvatar_CheckFieldMoveTrigger(fieldSystem, fieldSystem.PlayerAvatar, direction, fieldMoveFlags) == 1)
            {
                return 1;
            }
        }
        if (input.EndMovement)
        {
            if (Field_CheckWildEncounter(fieldSystem) != 0)
            {
                return 1;
            }
            if (Field_CheckSign(fieldSystem) == 1)
            {
                return 1;
            }
            if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(fieldSystem, DynamicMapFeaturesDistortionWorld) == 1)
            {
                int direction = (sbyte)input.PlayerDir;
                if (direction == DirNone)
                {
                    direction = PlayerAvatar.PlayerAvatar_GetFacingDir(Avatar(fieldSystem));
                }
                if (ov9_0224A67C(fieldSystem, direction) == 1)
                {
                    return 1;
                }
            }
        }
        if (input.Sign)
        {
            if (input.PlayerDir == PlayerAvatar.PlayerAvatar_GetFacingDir(Avatar(fieldSystem)) && Field_CheckSign(fieldSystem) == 1)
            {
                return 1;
            }
        }
        if (input.Interact)
        {
            MapObject? mapObject;
            int found;
            if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(fieldSystem, DynamicMapFeaturesDistortionWorld) == 1)
            {
                found = Field_DistortionInteract(fieldSystem, out mapObject);
            }
            else
            {
                found = FieldEvents.FieldEvent_GetInteractedObject(fieldSystem, out mapObject);
            }
            if (found == 1)
            {
                StopPlayerForInteraction(fieldSystem);
                if (MapObject.MapObject_GetTrainerType(mapObject!) != TrainerTypeNone)
                {
                    ScriptManager.ScriptManager_Set(fieldSystem, (ushort)MapObject.MapObject_GetScript(mapObject!), mapObject);
                }
                else
                {
                    ScriptManager.ScriptManager_Set(fieldSystem, 0, mapObject);
                }
                return 1;
            }
            int distortionState = PlayerAvatar.PlayerAvatar_MapDistortionState(Avatar(fieldSystem));
            if ((uint)distortionState <= DistortionStateFloor)
            {
                int bgScript = FieldEvents.FieldEvent_GetInteractedBgEventScript(fieldSystem, MapHeaderData.MapHeaderData_GetBgEvents(fieldSystem), MapHeaderData.MapHeaderData_GetNumBgEvents(fieldSystem));
                if (bgScript != ScriptNone)
                {
                    ScriptManager.ScriptManager_Set(fieldSystem, (ushort)bgScript, null);
                    return 1;
                }
            }
            if (distortionState == DistortionStateNone)
            {
                if (HoneyTrees.HoneyTree_TryInteract(fieldSystem, out int honeyTreeScript) != 0)
                {
                    ScriptManager.ScriptManager_Set(fieldSystem, (ushort)honeyTreeScript, null);
                    return 1;
                }
            }
            int distortionDir = PlayerAvatar.PlayerAvatar_GetDistortionDir(Avatar(fieldSystem));
            int facingBehavior = (int)PlayerMove.PlayerAvatar_GetDistortionFacingTileBehaviour(fieldSystem.PlayerAvatar, distortionDir);
            int behaviorScript = Field_TileBehaviorToScript(fieldSystem, (byte)facingBehavior);
            if (behaviorScript != ScriptNone)
            {
                ScriptManager.ScriptManager_Set(fieldSystem, (ushort)behaviorScript, null);
                return 1;
            }
            int facingDir = PlayerAvatar.PlayerAvatar_GetFacingDir(Avatar(fieldSystem));
            Field_Step(fieldSystem, out int x, out int z);
            if (VillaMap.Villa_TryInteractWithFurniture(fieldSystem, x, z, facingDir) == 1)
            {
                return 1;
            }
        }
        if (input.MapTransition)
        {
            if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(fieldSystem, DynamicMapFeaturesDistortionWorld) == 1)
            {
                ov9_0224A800(fieldSystem, input.TransitionDir);
            }
            else if (Field_CheckMapTransition(fieldSystem, input) == 1)
            {
                Field_TrySetMapConnection(fieldSystem);
                return 1;
            }
        }
        if (input.RegisteredItem)
        {
            if (sub_02069238(fieldSystem) == 1)
            {
                return 1;
            }
        }
        if (input.Menu)
        {
            if (StartMenu.FieldSystem_IsInValidLocation(fieldSystem))
            {
                SoundPlayback.Sound_PlayEffect(SeqSeMenuOpen);
                StartMenu.StartMenu_Open(fieldSystem);
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x021D20DC
    private static int Field_CheckSign(FieldSystem fieldSystem)
    {
        if (FieldEvents.FieldEvent_GetInteractedSignObject(fieldSystem, out MapObject? mapObject) == 1)
        {
            ScriptManager.ScriptManager_Set(fieldSystem, (ushort)MapObject.MapObject_GetScript(mapObject!), mapObject);
            return 1;
        }
        int script = FieldEvents.FieldEvent_GetInteractedWallSignScript(fieldSystem, MapHeaderData.MapHeaderData_GetBgEvents(fieldSystem), MapHeaderData.MapHeaderData_GetNumBgEvents(fieldSystem));
        if (script != ScriptNone)
        {
            ScriptManager.ScriptManager_Set(fieldSystem, (ushort)script, null);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021D213C
    public static int FieldInput_Process_Underground(FieldInput input, FieldSystem fieldSystem)
    {
        if (!input.Dummy5 && ScriptManager.FieldSystem_RunInitScript(fieldSystem, InitScriptFrame))
        {
            return 1;
        }
        if (sub_020363A0() != 0 || sub_020593CC(CommSystem.CommSys_CurNetId()) != 0)
        {
            return 0;
        }
        ov23_02242814();
        if (input.Interact)
        {
            ov23_02242830(0);
            return 0;
        }
        if (input.Menu)
        {
            ov23_022427F8();
            return 0;
        }
        ov23_02242458();
        return 0;
    }

    // OFFSET 0x021D219C
    public static int FieldInput_Process_Colosseum(FieldInput input, FieldSystem fieldSystem)
    {
        if (input.MapTransition && input.TransitionDir == DirSouth)
        {
            if (TileBehavior.TileBehavior_IsWarpEntranceSouth(Field_CurrentTileBehavior(fieldSystem)) != 0)
            {
                ScriptManager.ScriptManager_Set(fieldSystem, ScriptColosseumExit, null);
                return 1;
            }
        }
        if (sub_020363A0() != 0 || sub_020593CC(CommSystem.CommSys_CurNetId()) != 0 || sub_02059D2C() == 0)
        {
            return 0;
        }
        if (input.Interact)
        {
            if (FieldEvents.FieldEvent_GetInteractedObject(fieldSystem, out MapObject? mapObject) == 1 && MapObject.MapObject_GetMovementType(mapObject!) != MovementTypeNone)
            {
                StopPlayerForInteraction(fieldSystem);
                ScriptManager.ScriptManager_Set(fieldSystem, (ushort)MapObject.MapObject_GetScript(mapObject!), mapObject);
                return 1;
            }
        }
        if (input.Interact)
        {
            sub_0205B2D4(fieldSystem);
            return 1;
        }
        if (input.Menu)
        {
            SoundPlayback.Sound_PlayEffect(SeqSeMenuOpen);
            StartMenu.StartMenu_OpenColosseum(fieldSystem);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021D2274
    private static int Field_CheckTrainerInfo()
    {
        int count = 0;
        for (int netID = 1; netID < CommMaxNetID; netID++)
        {
            if (sub_02032EE8(netID) != null)
            {
                count++;
            }
        }
        if (count >= 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021D2298
    public static int FieldInput_Process_UnionRoom(FieldInput input, FieldSystem fieldSystem)
    {
        if ((sub_020318EC() & WirelessOtherPlayersMask) != 0)
        {
            if (Field_CheckTrainerInfo() != 0 && CommSystem.CommSys_ConnectedCount() > 1)
            {
                ScriptManager.ScriptManager_Set(fieldSystem, ScriptUnionRoomDisconnect, null);
                return 1;
            }
            return 0;
        }
        if (input.Interact)
        {
            if (FieldEvents.FieldEvent_GetInteractedObject(fieldSystem, out MapObject? mapObject) == 1)
            {
                StopPlayerForInteraction(fieldSystem);
                sub_02036B84();
                ScriptManager.ScriptManager_Set(fieldSystem, (ushort)MapObject.MapObject_GetScript(mapObject!), mapObject);
                return 1;
            }
        }
        if (input.Movement)
        {
            if (TileBehavior.TileBehavior_IsWarpPanel(Field_CurrentTileBehavior(fieldSystem)) != 0)
            {
                sub_020545EC(fieldSystem);
                return 1;
            }
        }
        if (input.Menu)
        {
            if (CommSystem.CommSys_ConnectedCount() <= 1)
            {
                SoundPlayback.Sound_PlayEffect(SeqSeMenuOpen);
                StartMenu.StartMenu_OpenUnionRoom(fieldSystem);
                sub_0205BEA8(UnionRoomStateMenu);
                sub_02036BA0();
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x021D2368
    public static int FieldInput_Process_BattleTower(FieldInput input, FieldSystem fieldSystem)
    {
        if (!input.Dummy5 && ScriptManager.FieldSystem_RunInitScript(fieldSystem, InitScriptFrame))
        {
            return 1;
        }
        if (input.Interact)
        {
            if (FieldEvents.FieldEvent_GetInteractedObject(fieldSystem, out MapObject? mapObject) == 1)
            {
                StopPlayerForInteraction(fieldSystem);
                if (MapObject.MapObject_GetTrainerType(mapObject!) != TrainerTypeNone)
                {
                    ScriptManager.ScriptManager_Set(fieldSystem, (ushort)MapObject.MapObject_GetScript(mapObject!), mapObject);
                }
                else
                {
                    ScriptManager.ScriptManager_Set(fieldSystem, 0, mapObject);
                }
                return 1;
            }
            int bgScript = FieldEvents.FieldEvent_GetInteractedBgEventScript(fieldSystem, MapHeaderData.MapHeaderData_GetBgEvents(fieldSystem), MapHeaderData.MapHeaderData_GetNumBgEvents(fieldSystem));
            if (bgScript != ScriptNone)
            {
                ScriptManager.ScriptManager_Set(fieldSystem, (ushort)bgScript, null);
                return 1;
            }
            int behaviorScript = Field_TileBehaviorToScript(fieldSystem, Field_NextTileBehavior(fieldSystem));
            if (behaviorScript != ScriptNone)
            {
                ScriptManager.ScriptManager_Set(fieldSystem, (ushort)behaviorScript, null);
                return 1;
            }
        }
        if (input.MapTransition)
        {
            if (Field_CheckMapTransition(fieldSystem, input) == 1)
            {
                return 1;
            }
        }
        if (input.RegisteredItem)
        {
            if (sub_02069238(fieldSystem) == 1)
            {
                return 1;
            }
        }
        if (input.Menu)
        {
            SoundPlayback.Sound_PlayEffect(SeqSeMenuOpen);
            StartMenu.StartMenu_Open(fieldSystem);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021D249C
    private static int Field_CheckWildEncounter(FieldSystem fieldSystem)
    {
        Field_GetPlayerPos(fieldSystem, out int x, out int z);
        if (SystemFlags.SystemFlag_CheckInPalPark(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)))
        {
            if (sub_02056374(fieldSystem, x, z) == 1)
            {
                sub_02051450(fieldSystem, sub_0205639C(fieldSystem));
                return 1;
            }
            return 0;
        }
        if (MapHeader.MapHeader_HasWildEncounters((int)fieldSystem.Location!.U32(LocationMapHeaderID)) && WildEncounters.WildEncounters_TryWildEncounter(fieldSystem) == 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021D2504
    private static int Field_CheckMapTransition(FieldSystem fieldSystem, FieldInput input)
    {
        if (input.TransitionDir == DirNone)
        {
            return 0;
        }
        if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(fieldSystem, DynamicMapFeaturesVeilstoneGym) == 1 && ov8_0224BF4C(fieldSystem) == 1)
        {
            return 1;
        }
        Field_Step(fieldSystem, out int x, out int z);
        if (TerrainCollisionManager.TerrainCollisionManager_CheckCollision(fieldSystem, x, z) == 0)
        {
            return 0;
        }
        BufferView nextMap = NewLocation();
        if (Field_MapConnection(fieldSystem, x, z, nextMap) != 0
            && input.TransitionDir != DirNone
            && TileBehavior.TileBehavior_IsDoor(TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z)) != 0)
        {
            int direction = input.TransitionDir;
            if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(fieldSystem, DynamicMapFeaturesHearthomeGym) == 1)
            {
                ov8_0224C62C(fieldSystem, x, z, ref direction);
            }
            FieldWarpTransition.FieldSystem_StartWarpTransition(fieldSystem, (int)nextMap.U32(LocationMapHeaderID), (int)nextMap.U32(LocationWarpID), 0, 0, direction, TransitionTypeDoor);
            return 1;
        }
        Field_GetPlayerPos(fieldSystem, out x, out z);
        int behavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
        if (TileBehavior.TileBehavior_IsWarpEntranceEast(behavior) != 0 || TileBehavior.TileBehavior_IsWarpEast(behavior) != 0)
        {
            if (input.TransitionDir != DirEast)
            {
                return 0;
            }
        }
        else if (TileBehavior.TileBehavior_IsWarpEntranceWest(behavior) != 0 || TileBehavior.TileBehavior_IsWarpWest(behavior) != 0)
        {
            if (input.TransitionDir != DirWest)
            {
                return 0;
            }
        }
        else if (TileBehavior.TileBehavior_IsWarpEntranceSouth(behavior) != 0 || TileBehavior.TileBehavior_IsWarpSouth(behavior) != 0)
        {
            if (input.TransitionDir != DirSouth)
            {
                return 0;
            }
        }
        else if (TileBehavior.TileBehavior_IsWarpStairsEast(behavior) != 0)
        {
            if (input.TransitionDir != DirEast)
            {
                return 0;
            }
        }
        else if (TileBehavior.TileBehavior_IsWarpStairsWest(behavior) != 0)
        {
            if (input.TransitionDir != DirWest)
            {
                return 0;
            }
        }
        if (Field_MapConnection(fieldSystem, x, z, nextMap) == 0)
        {
            return 0;
        }
        int transitionType;
        if (TileBehavior.TileBehavior_IsDoor(behavior) != 0)
        {
            transitionType = TransitionTypeDoor;
        }
        else if (TileBehavior.TileBehavior_IsWarpStairsEast(behavior) != 0)
        {
            transitionType = TransitionTypeStairs;
        }
        else if (TileBehavior.TileBehavior_IsWarpStairsWest(behavior) != 0)
        {
            transitionType = TransitionTypeStairs;
        }
        else if (TileBehavior.TileBehavior_IsWarpEntranceEast(behavior) != 0
            || TileBehavior.TileBehavior_IsWarpEast(behavior) != 0
            || TileBehavior.TileBehavior_IsWarpEntranceWest(behavior) != 0
            || TileBehavior.TileBehavior_IsWarpWest(behavior) != 0
            || TileBehavior.TileBehavior_IsWarpEntranceSouth(behavior) != 0
            || TileBehavior.TileBehavior_IsWarpSouth(behavior) != 0)
        {
            FieldWarpTransition.FieldSystem_StartWarpTransitionByMapType(fieldSystem, (int)nextMap.U32(LocationMapHeaderID), (int)nextMap.U32(LocationWarpID), 0, 0, input.TransitionDir);
            return 1;
        }
        else
        {
            return 0;
        }
        FieldWarpTransition.FieldSystem_StartWarpTransition(fieldSystem, (int)nextMap.U32(LocationMapHeaderID), (int)nextMap.U32(LocationWarpID), 0, 0, input.TransitionDir, transitionType);
        return 1;
    }

    // OFFSET 0x021D271C
    private static int Field_TileBehaviorToScript(FieldSystem fieldSystem, byte behavior)
    {
        int facingDir = PlayerAvatar.PlayerAvatar_GetFacingDir(Avatar(fieldSystem));
        if (TileBehavior.TileBehavior_IsPC(behavior) != 0 && facingDir == DirNorth)
        {
            return ScriptPC;
        }
        if (TileBehavior.TileBehavior_IsSmallBookshelf1(behavior) != 0)
        {
            return ScriptSmallBookshelf1;
        }
        if (TileBehavior.TileBehavior_IsSmallBookshelf2(behavior) != 0)
        {
            return ScriptSmallBookshelf2;
        }
        if (TileBehavior.TileBehavior_IsBookshelf1(behavior) != 0)
        {
            return ScriptBookshelf1;
        }
        if (TileBehavior.TileBehavior_IsBookshelf2(behavior) != 0)
        {
            return ScriptBookshelf2;
        }
        if (TileBehavior.TileBehavior_IsTrashCan(behavior) != 0)
        {
            return ScriptTrashCan;
        }
        if (TileBehavior.TileBehavior_IsMartShelf1(behavior) != 0)
        {
            return ScriptMartShelf1;
        }
        if (TileBehavior.TileBehavior_IsMartShelf2(behavior) != 0)
        {
            return ScriptMartShelf2;
        }
        if (TileBehavior.TileBehavior_IsMartShelf3(behavior) != 0)
        {
            return ScriptMartShelf3;
        }
        if (TileBehavior.TileBehavior_IsWaterfall(behavior) != 0)
        {
            return ScriptWaterfall;
        }
        if (TileBehavior.TileBehavior_IsTownMap(behavior) != 0)
        {
            return ScriptTownMap;
        }
        if (TileBehavior.TileBehavior_IsBikeParking(behavior) != 0)
        {
            return ScriptBikeParking;
        }
        if (TileBehavior.TileBehavior_IsTV(behavior) != 0 && facingDir == DirNorth)
        {
            return ScriptTV;
        }
        if (PlayerFieldMoves.PlayerAvatar_CanUseRockClimb(behavior, facingDir) != 0)
        {
            return ScriptRockClimb;
        }
        if (PlayerAvatar.PlayerAvatar_GetPlayerState(Avatar(fieldSystem)) != PlayerStateSurfing)
        {
            TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!);
            int currentBehavior = (int)PlayerMove.PlayerAvatar_GetDistortionCurrTileBehaviour(fieldSystem.PlayerAvatar);
            if (PlayerFieldMoves.PlayerAvatar_CanUseSurf(fieldSystem.PlayerAvatar, currentBehavior, behavior) != 0
                && TrainerInfo.TrainerInfo_HasBadge(trainerInfo, BadgeFen)
                && FieldParty.Party_HasMonWithMove(Party.SaveData_GetParty(fieldSystem.SaveData!), MoveSurf) != PartySlotNone)
            {
                return ScriptSurf;
            }
        }
        return ScriptNone;
    }

    // OFFSET 0x021D2884
    private static int Field_ProcessStep(FieldSystem fieldSystem)
    {
        if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(fieldSystem, DynamicMapFeaturesCanalaveGym) == 1 && ov8_0224AAA8(fieldSystem) != 0)
        {
            return 1;
        }
        if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(fieldSystem, DynamicMapFeaturesDistortionWorld) == 1 && ov9_0224A71C(fieldSystem) == 1)
        {
            return 1;
        }
        int facingDir = PlayerAvatar.PlayerAvatar_GetFacingDir(Avatar(fieldSystem));
        if (PlayerFieldMoves.PlayerAvatar_CheckSlideTile(fieldSystem, fieldSystem.PlayerAvatar, facingDir) == 1)
        {
            return 1;
        }
        int x = PlayerAvatar.PlayerAvatar_GetXPos(Avatar(fieldSystem));
        int z = PlayerAvatar.PlayerAvatar_GetZPos(Avatar(fieldSystem));
        byte behavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
        if (Field_CheckCoordEvent(fieldSystem) == 1)
        {
            return 1;
        }
        if (Field_CheckTransition(fieldSystem, x, z, behavior) == 1)
        {
            Field_TrySetMapConnection(fieldSystem);
            return 1;
        }
        if (PlayerAvatar.PlayerAvatar_CheckForcedMovement(Avatar(fieldSystem)) != 0)
        {
            return 0;
        }
        FieldPoketch.FieldSystem_SendPoketchEvent(fieldSystem, PoketchEventStep, 1);
        if (Field_UpdatePoison(fieldSystem) == 1)
        {
            return 1;
        }
        if (Field_UpdateSafari(fieldSystem) == 1)
        {
            return 1;
        }
        if (Field_UpdateDaycare(fieldSystem) == 1)
        {
            return 1;
        }
        if (Field_UpdateVsSeeker(fieldSystem) == 1)
        {
            return 1;
        }
        if (Field_UpdatePokeRadar(fieldSystem) == 1)
        {
            return 1;
        }
        if (Field_UpdateRepel(fieldSystem) == 1)
        {
            return 1;
        }
        if (Field_UpdateFriendship(fieldSystem) != 0)
        {
            Field_CalculateFriendship(fieldSystem);
        }
        SystemVars.SystemVars_IncrementAmitySquareStepCount(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
        return 0;
    }

    // OFFSET 0x021D29A0
    private static int Field_CheckCoordEvent(FieldSystem fieldSystem)
    {
        int script = FieldEvents.FieldEvent_GetCoordEventScript(fieldSystem, MapHeaderData.MapHeaderData_GetCoordEvents(fieldSystem), MapHeaderData.MapHeaderData_GetNumCoordEvents(fieldSystem));
        if (script != ScriptNone)
        {
            ScriptManager.ScriptManager_Set(fieldSystem, (ushort)script, null);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021D29D8
    private static int Field_CheckTransition(FieldSystem fieldSystem, int x, int z, byte behavior)
    {
        BufferView nextMap = NewLocation();
        if (Field_MapConnection(fieldSystem, x, z, nextMap) == 0)
        {
            return 0;
        }
        if (TileBehavior.TileBehavior_IsEscalatorFlipFace(behavior) == 1)
        {
            int direction = PlayerAvatar.PlayerAvatar_GetFacingDir(Avatar(fieldSystem));
            if (direction == DirWest)
            {
                direction = DirEast;
            }
            else if (direction == DirEast)
            {
                direction = DirWest;
            }
            else
            {
                ErrorHandling.GF_AssertFail();
                return 0;
            }
            FieldWarpTransition.FieldSystem_StartWarpTransition(fieldSystem, (int)nextMap.U32(LocationMapHeaderID), (int)nextMap.U32(LocationWarpID), 0, 0, direction, TransitionTypeEscalator);
            return 1;
        }
        if (TileBehavior.TileBehavior_IsEscalator(behavior) == 1)
        {
            int direction = PlayerAvatar.PlayerAvatar_GetFacingDir(Avatar(fieldSystem));
            if (direction != DirWest && direction != DirEast)
            {
                ErrorHandling.GF_AssertFail();
                return 0;
            }
            FieldWarpTransition.FieldSystem_StartWarpTransition(fieldSystem, (int)nextMap.U32(LocationMapHeaderID), (int)nextMap.U32(LocationWarpID), 0, 0, direction, TransitionTypeEscalator);
            return 1;
        }
        if (TileBehavior.TileBehavior_IsWarpEntranceNorth(behavior) != 0 || TileBehavior.TileBehavior_IsWarpNorth(behavior) != 0)
        {
            FieldWarpTransition.FieldSystem_StartWarpTransitionByMapType(fieldSystem, (int)nextMap.U32(LocationMapHeaderID), (int)nextMap.U32(LocationWarpID), 0, 0, DirNorth);
            return 1;
        }
        if (TileBehavior.TileBehavior_IsWarpPanel(behavior) != 0)
        {
            sub_02053F58(fieldSystem, (int)nextMap.U32(LocationMapHeaderID), (int)nextMap.U32(LocationWarpID));
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021D2ABC
    private static int Field_UpdateDaycare(FieldSystem fieldSystem)
    {
        Party party = Party.SaveData_GetParty(fieldSystem.SaveData!);
        Daycare daycare = Daycare.SaveData_GetDaycare(fieldSystem.SaveData!);
        if (FieldDaycare.Daycare_Update(daycare, party, fieldSystem) == 1)
        {
            GameRecords records = GameRecords.SaveData_GetGameRecords(fieldSystem.SaveData!);
            GameRecords.GameRecords_IncrementRecordValue(records, RecordDaycareEggs);
            GameRecords.GameRecords_IncrementTrainerScore(records, TrainerScoreDaycareEgg);
            ScriptManager.ScriptManager_Set(fieldSystem, ScriptDaycareEgg, null);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021D2B08
    private static int Field_UpdateVsSeeker(FieldSystem fieldSystem)
    {
        VsSeeker.VsSeeker_UpdateStepCount(fieldSystem);
        return 0;
    }

    // OFFSET 0x021D2B14
    private static int Field_UpdatePokeRadar(FieldSystem fieldSystem)
    {
        RadarChain.PokeRadar_ChargeStep(fieldSystem);
        return 0;
    }

    // OFFSET 0x021D2B20
    private static int Field_UpdateRepel(FieldSystem fieldSystem)
    {
        return Repel.Repel_UpdateSteps(fieldSystem.SaveData!, fieldSystem);
    }

    // OFFSET 0x021D2B2C
    private static int Field_UpdateFriendship(FieldSystem fieldSystem)
    {
        int updateFriendship = 0;
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        ushort stepCount = (ushort)(SystemVars.SystemVars_GetFriendshipStepCount(varsFlags) + 1);
        if (stepCount >= FriendshipStepInterval)
        {
            stepCount = 0;
            updateFriendship = 1;
        }
        SystemVars.SystemVars_SetFriendshipStepCount(varsFlags, stepCount);
        return updateFriendship;
    }

    // OFFSET 0x021D2B54
    private static void Field_CalculateFriendship(FieldSystem fieldSystem)
    {
        Party party = Party.SaveData_GetParty(fieldSystem.SaveData!);
        ushort mapLabelTextID = (ushort)MapHeader.MapHeader_GetMapLabelTextID((int)fieldSystem.Location!.U32(LocationMapHeaderID));
        int count = Party.Party_GetCurrentCount(party);
        for (int i = 0; i < count; i++)
        {
            Pokemon.Pokemon_UpdateFriendship(Party.Party_GetPokemonBySlotIndex(party, i), FriendshipEventWalking, mapLabelTextID);
        }
    }

    // OFFSET 0x021D2B94
    private static int Field_UpdatePoison(FieldSystem fieldSystem)
    {
        Party party = Party.SaveData_GetParty(fieldSystem.SaveData!);
        BufferView poisonSteps = FieldPlayerState.FieldPlayerState_GetPoisonStepCount(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        poisonSteps.W16(0, poisonSteps.U16(0) + 1);
        poisonSteps.W16(0, poisonSteps.U16(0) & PoisonStepMask);
        if (poisonSteps.U16(0) != 0)
        {
            return 0;
        }
        ushort mapLabelTextID = (ushort)MapHeader.MapHeader_GetMapLabelTextID((int)fieldSystem.Location!.U32(LocationMapHeaderID));
        switch (FieldParty.Pokemon_DoPoisonDamage(party, mapLabelTextID))
        {
            case PoisonResultNone:
                return 0;
            case PoisonResultDamaged:
                FieldPoisonEffect.Field_DoPoisonEffect((FieldPoisonEffect)fieldSystem.FieldMapContext!.HBlankScreenEffect!);
                return 0;
            case PoisonResultFainted:
                FieldPoisonEffect.Field_DoPoisonEffect((FieldPoisonEffect)fieldSystem.FieldMapContext!.HBlankScreenEffect!);
                ScriptManager.ScriptManager_Set(fieldSystem, ScriptPoisonFaint, null);
                return 1;
        }
        return 0;
    }

    // OFFSET 0x021D2C14
    private static int Field_UpdateSafari(FieldSystem fieldSystem)
    {
        if (!SystemFlags.SystemFlag_CheckSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)))
        {
            return 0;
        }
        BufferView ballCount = FieldPlayerState.FieldPlayerState_GetSafariBallCount(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        if (ballCount.U16(0) == 0)
        {
            ScriptManager.ScriptManager_Set(fieldSystem, ScriptSafariOutOfBalls, null);
            return 1;
        }
        BufferView stepCount = FieldPlayerState.FieldPlayerState_GetSafariStepCount(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        stepCount.W16(0, stepCount.U16(0) + 1);
        if (stepCount.U16(0) >= SafariMaxSteps)
        {
            ScriptManager.ScriptManager_Set(fieldSystem, ScriptSafariOutOfSteps, null);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021D2C7C
    private static void Field_GetPlayerPos(FieldSystem fieldSystem, out int playerX, out int playerZ)
    {
        playerX = PlayerAvatar.PlayerAvatar_GetXPos(Avatar(fieldSystem));
        playerZ = PlayerAvatar.PlayerAvatar_GetZPos(Avatar(fieldSystem));
    }

    // OFFSET 0x021D2C98
    private static void Field_Step(FieldSystem fieldSystem, out int playerX, out int playerZ)
    {
        int direction = PlayerAvatar.PlayerAvatar_GetFacingDir(Avatar(fieldSystem));
        Field_StepInDirection(fieldSystem, direction, out playerX, out playerZ);
    }

    // OFFSET 0x021D2CB4
    private static void Field_StepInDirection(FieldSystem fieldSystem, int direction, out int playerX, out int playerZ)
    {
        Field_GetPlayerPos(fieldSystem, out playerX, out playerZ);
        switch (direction)
        {
            case DirNorth:
                playerZ--;
                break;
            case DirSouth:
                playerZ++;
                break;
            case DirWest:
                playerX--;
                break;
            case DirEast:
                playerX++;
                break;
        }
    }

    // OFFSET 0x021D2CFC
    private static byte Field_CurrentTileBehavior(FieldSystem fieldSystem)
    {
        Field_GetPlayerPos(fieldSystem, out int x, out int z);
        return TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
    }

    // OFFSET 0x021D2D18
    private static byte Field_NextTileBehavior(FieldSystem fieldSystem)
    {
        Field_Step(fieldSystem, out int x, out int z);
        return TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
    }

    // OFFSET 0x021D2D34
    private static int Field_MapConnection(FieldSystem fieldSystem, int playerX, int playerZ, BufferView nextMap)
    {
        int warpIndex = MapHeaderData.MapHeaderData_GetIndexOfWarpEventAtPos(fieldSystem, playerX, playerZ);
        if (warpIndex == -1)
        {
            return 0;
        }
        WarpEvent? warp = MapHeaderData.MapHeaderData_GetWarpEventByIndex(fieldSystem, warpIndex);
        if (warp == null)
        {
            return 0;
        }
        if (warp.DestWarpID == WarpSpecialDestWarpID)
        {
            if (warp.DestHeaderID != WarpSpecialDestHeaderID)
            {
                ErrorHandling.GF_AssertFail();
            }
            BufferView special = FieldPlayerState.FieldPlayerState_GetSpecialLocation(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
            special.CopyTo(0, nextMap.Buffer, nextMap.Offset, LocationSize);
        }
        else
        {
            nextMap.W32(LocationMapHeaderID, warp.DestHeaderID);
            nextMap.W32(LocationWarpID, warp.DestWarpID);
            nextMap.W32(LocationX, warp.X);
            nextMap.W32(LocationZ, warp.Z);
            nextMap.W32(LocationFaceDirection, DirSouth);
        }
        BufferView entrance = FieldPlayerState.FieldPlayerState_GetEntranceLocation(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        int facingDir = PlayerAvatar.PlayerAvatar_GetFacingDir(Avatar(fieldSystem));
        entrance.W32(LocationMapHeaderID, fieldSystem.Location!.U32(LocationMapHeaderID));
        entrance.W32(LocationWarpID, (uint)warpIndex);
        entrance.W32(LocationX, (uint)playerX);
        entrance.W32(LocationZ, (uint)playerZ);
        entrance.W32(LocationFaceDirection, (uint)facingDir);
        return 1;
    }

    // OFFSET 0x021D2DCC
    private static void Field_SetMapConnection(FieldSystem fieldSystem, int playerX, int playerZ, int playerDir)
    {
        BufferView exit = FieldPlayerState.FieldPlayerState_GetExitLocation(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        fieldSystem.Location!.CopyTo(0, exit.Buffer, exit.Offset, LocationSize);
        exit.W32(LocationFaceDirection, (uint)playerDir);
        exit.W32(LocationX, (uint)playerX);
        exit.W32(LocationZ, (uint)playerZ);
        if (playerDir == DirNorth)
        {
            exit.W32(LocationZ, exit.U32(LocationZ) + 1);
        }
        exit.W32(LocationMapHeaderID, fieldSystem.Location.U32(LocationMapHeaderID));
        exit.W32(LocationWarpID, unchecked((uint)-1));
    }

    // OFFSET 0x021D2E14
    private static void Field_TrySetMapConnection(FieldSystem fieldSystem)
    {
        BufferView nextMap = NewLocation();
        Field_GetPlayerPos(fieldSystem, out int x, out int z);
        if (Field_MapConnection(fieldSystem, x, z, nextMap) != 0)
        {
            if (MapHeader.MapHeader_IsOnMainMatrix((int)fieldSystem.Location!.U32(LocationMapHeaderID))
                && !MapHeader.MapHeader_IsOnMainMatrix((int)nextMap.U32(LocationMapHeaderID)))
            {
                Field_SetMapConnection(fieldSystem, x, z, PlayerAvatar.PlayerAvatar_GetFacingDir(Avatar(fieldSystem)));
            }
            return;
        }
        Field_Step(fieldSystem, out x, out z);
        if (Field_MapConnection(fieldSystem, x, z, nextMap) != 0)
        {
            if (MapHeader.MapHeader_IsOnMainMatrix((int)fieldSystem.Location!.U32(LocationMapHeaderID))
                && !MapHeader.MapHeader_IsOnMainMatrix((int)nextMap.U32(LocationMapHeaderID)))
            {
                Field_SetMapConnection(fieldSystem, x, z, PlayerAvatar.PlayerAvatar_GetFacingDir(Avatar(fieldSystem)));
            }
        }
    }

    // OFFSET 0x021D2EA4
    private static int Field_DistortionInteract(FieldSystem fieldSystem, out MapObject? mapObject)
    {
        int index = 0;
        mapObject = null;
        PlayerMove.PlayerAvatar_GetFacingDistortionWorldPos(fieldSystem.PlayerAvatar, out int x, out int y, out int z);
        while (MapObjectManager.MapObjectMan_FindObjectWithStatus(fieldSystem.MapObjectManager!, out MapObject? candidate, ref index, MapObjectStatusActive) != 0)
        {
            mapObject = candidate;
            int objectX = MapObject.MapObject_GetX(candidate);
            int objectY = MapObject.MapObject_GetY(candidate) / 2;
            int objectZ = MapObject.MapObject_GetZ(candidate);
            if (y == objectY && x == objectX && z == objectZ)
            {
                return 1;
            }
        }
        return 0;
    }
}
