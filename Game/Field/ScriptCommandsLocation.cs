namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const int PlayerStateCycling = 1;
    private const ushort SeqBicycle = 0x480;
    private const uint RequestStateWalking = 1;
    private const uint RequestStateCycling = 2;
    private const int BGMFadeOutMode = 1;
    private const int MonDataFriendship = 9;
    private const int MonDataFatefulEncounter = 0x6E;
    private const int NoPartySlot = 0xFF;
    private const int NoPartySlotForMove = 6;
    private const int MonDataMove1 = 0x36;
    private const int MonMoveCount = 4;

    private static void RegisterLocation(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x02040D34] = ScrCmd_SetMenuXOriginSide;
        ported[0x02040D54] = ScrCmd_SetMenuYOriginSide;
        ported[0x02042A60] = ScrCmd_GetPreviousMapID;
        ported[0x02042CB4] = ScrCmd_CheckPlayerOnBike;
        ported[0x02042CE4] = ScrCmd_SetPlayerBike;
        ported[0x02043100] = ScrCmd_GetFloorsAbove;
        ported[0x02043130] = ScrCmd_ShowCurrentFloor;
        ported[0x02045134] = ScrCmd_GetCapturedFlagCount;
        ported[0x0204D5CC] = ScrCmd_GetPartyMonFriendship;
        ported[0x0204DC98] = ScrCmd_CheckPartyHasSpecies;
        ported[0x0204D2D8] = ScrCmd_FindPartySlotWithMove;
        ported[0x0204E12C] = ScrCmd_FindPartySlotWithFatefulEncounterSpecies;
        ported[0x0204EA04] = ScrCmd_CheckHasRoomForGoodsInPC;
        ported[0x02048060] = ScrCmd_CapitalizeFirstLetter;
        ported[0x02048080] = ScrCmd_BufferFloorNumber;
    }

    // OFFSET 0x02040D34
    private static bool ScrCmd_SetMenuXOriginSide(ScriptContext ctx)
    {
        FieldMenuManager menuManager = (FieldMenuManager)Manager(ctx).FieldMenuManager!;
        FieldMenus.FieldMenuManager_SetHorizontalAnchor(menuManager, ScriptContext.ScriptContext_ReadByte(ctx));
        return true;
    }

    // OFFSET 0x02040D54
    private static bool ScrCmd_SetMenuYOriginSide(ScriptContext ctx)
    {
        FieldMenuManager menuManager = (FieldMenuManager)Manager(ctx).FieldMenuManager!;
        FieldMenus.FieldMenuManager_SetVerticalAnchor(menuManager, ScriptContext.ScriptContext_ReadByte(ctx));
        return true;
    }

    // OFFSET 0x02042A60
    private static bool ScrCmd_GetPreviousMapID(ScriptContext ctx)
    {
        BufferView previous = FieldPlayerState.FieldPlayerState_GetPreviousLocation(FieldPlayerState.SaveData_GetFieldPlayerState(ctx.FieldSystem.SaveData!));
        BufferView destination = VarPointer(ctx);
        destination.W16(0, (int)previous.U32(0));
        return false;
    }

    // OFFSET 0x02042CB4
    private static bool ScrCmd_CheckPlayerOnBike(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, PlayerAvatar.PlayerAvatar_GetPlayerState(ctx.FieldSystem.PlayerAvatar!) == PlayerStateCycling ? 1 : 0);
        return false;
    }

    // OFFSET 0x02042CE4
    private static bool ScrCmd_SetPlayerBike(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        if (ScriptContext.ScriptContext_ReadByte(ctx) == 1)
        {
            FieldBgm.FieldBGM_SetOverride(fieldSystem, SeqBicycle);
            FieldBgm.FieldBGM_TryFadeOut(fieldSystem, SeqBicycle, BGMFadeOutMode);
            PlayerAvatarRequestState.PlayerAvatar_RequestState(fieldSystem.PlayerAvatar!, RequestStateCycling);
            PlayerAvatarRequestState.PlayerAvatar_ProcessRequestedStates(fieldSystem.PlayerAvatar!);
        }
        else
        {
            PlayerAvatarRequestState.PlayerAvatar_RequestState(fieldSystem.PlayerAvatar!, RequestStateWalking);
            PlayerAvatarRequestState.PlayerAvatar_ProcessRequestedStates(fieldSystem.PlayerAvatar!);
            FieldBgm.FieldBGM_SetOverride(fieldSystem, 0);
            FieldBgm.FieldBGM_TryFadeOut(fieldSystem, FieldBgm.FieldBGM_GetEffective(fieldSystem, (int)fieldSystem.Location!.U32(0)), BGMFadeOutMode);
        }
        return false;
    }

    // OFFSET 0x02043100
    private static bool ScrCmd_GetFloorsAbove(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        BufferView special = FieldPlayerState.FieldPlayerState_GetSpecialLocation(FieldPlayerState.SaveData_GetFieldPlayerState(ctx.FieldSystem.SaveData!));
        destination.W16(0, FieldMenus.FieldMenu_GetFloorsAbove((int)special.U32(0)));
        return false;
    }

    // OFFSET 0x02043130
    private static bool ScrCmd_ShowCurrentFloor(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte tilemapLeft = ScriptContext.ScriptContext_ReadByte(ctx);
        byte tilemapTop = ScriptContext.ScriptContext_ReadByte(ctx);
        BufferView selectedOption = VarPointer(ctx);
        ushort unused = VarValue(ctx);
        FieldMenus.FieldMenu_ShowCurrentFloorWindow(fieldSystem, tilemapLeft, tilemapTop, selectedOption, template, unused);
        return false;
    }

    // OFFSET 0x02045134
    private static bool ScrCmd_GetCapturedFlagCount(ScriptContext ctx)
    {
        SaveData saveData = ctx.FieldSystem.SaveData!;
        BufferView destination = VarPointer(ctx);
        destination.W16(0, UndergroundRecord.UndergroundRecord_GetCapturedFlagCount(SecretBase.SaveData_GetUndergroundRecord(saveData)));
        return false;
    }

    // OFFSET 0x0204D5CC
    private static bool ScrCmd_GetPartyMonFriendship(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        BufferView destination = VarPointer(ctx);
        ushort slot = VarValue(ctx);
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), slot);
        destination.W16(0, (int)Pokemon.Pokemon_GetValue(mon, MonDataFriendship, null));
        return false;
    }

    // OFFSET 0x0204DC98
    private static bool ScrCmd_CheckPartyHasSpecies(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        BufferView destination = VarPointer(ctx);
        ushort species = VarValue(ctx);
        byte partyCount = (byte)Party.Party_GetCurrentCount(Party.SaveData_GetParty(fieldSystem.SaveData!));
        destination.W16(0, 0);
        for (byte slot = 0; slot < partyCount; slot++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), slot);
            if (Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) == 0 && species == (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null))
            {
                destination.W16(0, 1);
                break;
            }
        }
        return false;
    }

    // OFFSET 0x0204E12C
    private static bool ScrCmd_FindPartySlotWithFatefulEncounterSpecies(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        BufferView destination = VarPointer(ctx);
        ushort species = VarValue(ctx);
        byte partyCount = (byte)Party.Party_GetCurrentCount(Party.SaveData_GetParty(fieldSystem.SaveData!));
        destination.W16(0, NoPartySlot);
        for (byte slot = 0; slot < partyCount; slot++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), slot);
            if (Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) == 0 && species == (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null)
                && Pokemon.Pokemon_GetValue(mon, MonDataFatefulEncounter, null) == 1)
            {
                destination.W16(0, slot);
                break;
            }
        }
        return false;
    }

    // OFFSET 0x0204EA04
    private static bool ScrCmd_CheckHasRoomForGoodsInPC(ScriptContext ctx)
    {
        SaveData saveData = ctx.FieldSystem.SaveData!;
        ushort goods = VarValue(ctx);
        VarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, Underground.Underground_IsRoomForGoodsInPC(Underground.SaveData_GetUnderground(saveData), goods) ? 1 : 0);
        return false;
    }

    // OFFSET 0x02048060
    private static bool ScrCmd_CapitalizeFirstLetter(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        StringTemplate.StringTemplate_CapitalizeArgAtIndex(template, ScriptContext.ScriptContext_ReadByte(ctx));
        return false;
    }

    // OFFSET 0x02048080
    private static bool ScrCmd_BufferFloorNumber(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte idx = ScriptContext.ScriptContext_ReadByte(ctx);
        byte floor = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetFloorNumber(template, idx, floor);
        return false;
    }

    // OFFSET 0x0204D2D8
    private static bool ScrCmd_FindPartySlotWithMove(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        BufferView destination = VarPointer(ctx);
        ushort move = VarValue(ctx);
        byte partyCount = (byte)Party.Party_GetCurrentCount(Party.SaveData_GetParty(fieldSystem.SaveData!));
        destination.W16(0, NoPartySlotForMove);
        for (byte slot = 0; slot < partyCount; slot++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), slot);
            if (Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) != 0)
            {
                continue;
            }
            for (int i = 0; i < MonMoveCount; i++)
            {
                if (move == Pokemon.Pokemon_GetValue(mon, MonDataMove1 + i, null))
                {
                    destination.W16(0, slot);
                    return false;
                }
            }
        }
        return false;
    }
}
