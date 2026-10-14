namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const int NarcMessageBanks = 0x1A;
    private const int MessageBankSpeciesNames = 0x19C;
    private const int MessageLoaderTypeNarc = 1;
    private const int WarpIDNone = -1;
    private const int ItemParamPocket = 5;
    private const byte NationalDexSet = 1;
    private const byte NationalDexGet = 2;
    private const int MonDataHPEV = 0xD;
    private const uint GameCornerPrizeTable = 0x020EC018;
    private const int MonDataNickname = 0x75;
    private const int NicknameBufferLength = 20;
    private const int NicknameMaxChars = 10;
    private const int NamingScreenTypePokemon = 1;
    private const byte DefogFuncClear = 0;
    private const int OrbFlagFirstValue = 0x87;
    private const ushort FirstPlate = 0x12A;
    private const ushort LastPlate = 0x139;
    private const byte DefogFuncSet = 1;
    private const byte DefogFuncCheck = 2;
    private const int MonDataOTID = 7;
    private const int MonDataSpecies = 5;
    private const int MonDataIsEgg = 0x4C;

    private static void RegisterStory(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x02042984] = ScrCmd_Warp;
        ported[0x0204342C] = ScrCmd_StartFirstBattle;
        ported[0x02043C3C] = ScrCmd_HealParty;
        ported[0x020479C0] = ScrCmd_BufferPlayerStarterSpeciesName;
        ported[0x02047A10] = ScrCmd_BufferRivalStarterSpeciesName;
        ported[0x02047A60] = ScrCmd_BufferCounterpartStarterSpeciesName;
        ported[0x02048A50] = ScrCmd_CheckWonBattle;
        ported[0x02048A80] = ScrCmd_CheckLostBattle;
        ported[0x02048AB0] = ScrCmd_CheckDidNotCapture;
        ported[0x0204E594] = ScrCmd_SetHasPartner;
        ported[0x0204E5A8] = ScrCmd_ClearHasPartner;
        ported[0x0204E5BC] = ScrCmd_CheckStepFlag;
        ported[0x0204E5E8] = ScrCmd_SetStepFlag;
        ported[0x0204E5FC] = ScrCmd_ClearStepFlag;
        ported[0x0204E80C] = ScrCmd_CanFitItem;
        ported[0x0204E704] = ScrCmd_DoDefogFunc;
        ported[0x02042718] = ScrCmd_OpenPokemonNamingScreen;
        ported[0x0204B85C] = ScrCmd_MysteryGiftGive;
        ported[0x02044C64] = ScrCmd_SetNewsPressDeadline;
        ported[0x02044C90] = ScrCmd_GetNewsPressDeadline;
        ported[0x02044CBC] = ScrCmd_EnableSwarms;
        ported[0x02044CCC] = ScrCmd_ActivateRoamingPokemon;
        ported[0x02044DB4] = ScrCmd_TurnOnPokedexLanguageDetection;
        ported[0x02044DC8] = ScrCmd_TurnOnPokedexFormDetection;
        ported[0x02044DDC] = ScrCmd_GetSetNationalDexEnabled;
        ported[0x02044E40] = ScrCmd_GetPartyMonEVTotal;
        ported[0x02044EDC] = ScrCmd_GetDayOfWeek;
        ported[0x0204E378] = ScrCmd_GetGameCornerPrizeData;
        ported[0x0204E3CC] = ScrCmd_CheckPokedexAcquired;
        ported[0x02044528] = ScrCmd_GiveJournal;
        ported[0x02043568] = ScrCmd_SaveExtraData;
        ported[0x02043578] = ScrCmd_CheckIsMiscSaveInit;
        ported[0x020435A0] = ScrCmd_EnablePoketch;
        ported[0x020435AC] = ScrCmd_CheckPoketchEnabled;
        ported[0x020435DC] = ScrCmd_RegisterPoketchApp;
        ported[0x02043608] = ScrCmd_CheckPoketchAppRegistered;
        ported[0x02044558] = ScrCmd_CreateJournalEvent;
        ported[0x02043460] = ScrCmd_StartCatchingTutorial;
        ported[0x02043190] = ScrCmd_GetLocalDexSeenCount;
        ported[0x020431C0] = ScrCmd_GetLocalDexCaughtCount;
        ported[0x020431F0] = ScrCmd_GetNationalDexSeenCount;
        ported[0x02043220] = ScrCmd_GetNationalDexCaughtCount;
        ported[0x02043250] = ScrCmd_Unused_122;
        ported[0x02043254] = ScrCmd_LoadPokedexRating;
        ported[0x0204E3FC] = ScrCmd_GivePokedex;
        ported[0x0204E410] = ScrCmd_CheckRunningShoesAcquired;
        ported[0x0204E4A4] = ScrCmd_GiveBadge;
        ported[0x0204E4D8] = ScrCmd_CheckBagAcquired;
        ported[0x02044820] = ScrCmd_CheckNationalDexCompleted;
        ported[0x020448F0] = ScrCmd_Unused_1EF;
        ported[0x02044908] = ScrCmd_IncrementGameRecord;
        ported[0x02044928] = ScrCmd_Unused_1E6;
        ported[0x02044980] = ScrCmd_Unused_1E7;
        ported[0x020449F4] = ScrCmd_AddToGameRecord;
        ported[0x02044A28] = ScrCmd_AddToGameRecordBigValue;
        ported[0x02045E14] = ScrCmd_GetUndergroundTalkCounter;
        ported[0x02045EA0] = ScrCmd_GetWiFListValidFriendsCount;
        ported[0x02045ECC] = ScrCmd_TrySetUnusedCollectedOrbFlag;
        ported[0x02045F04] = ScrCmd_CheckItemIsPlate;
        ported[0x0204E75C] = ScrCmd_AddItem;
        ported[0x0204E7B4] = ScrCmd_RemoveItem;
        ported[0x0204E864] = ScrCmd_CheckItem;
        ported[0x0204E8BC] = ScrCmd_GetItemQuantity;
        ported[0x02044328] = ScrCmd_CountMailInMailbox;
        ported[0x0204D08C] = ScrCmd_GetPartyMonSpecies;
        ported[0x0204E900] = ScrCmd_IsItemTMHM;
        ported[0x0204E934] = ScrCmd_GetItemPocket;
        ported[0x0204E96C] = ScrCmd_Dummy081;
        ported[0x0204E970] = ScrCmd_Dummy082;
        ported[0x0204D0E8] = ScrCmd_CheckIsPartyMonOutsider;
        ported[0x02044374] = ScrCmd_GetTimeOfDay;
        ported[0x02044398] = ScrCmd_GetRandom;
        ported[0x020443D0] = ScrCmd_GetRandom2;
    }

    // OFFSET 0x02042718
    private static bool ScrCmd_OpenPokemonNamingScreen(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort slot = VarValue(ctx);
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), slot);
        ushort[] nickname = new ushort[NicknameBufferLength];
        Pokemon.Pokemon_GetValue(mon, MonDataNickname, nickname);
        BufferView result = VarPointer(ctx);
        int species = (int)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
        FieldSystemChildProcesses.FieldTask_OpenNamingScreen(ctx.Task!, NamingScreenTypePokemon, species, NicknameMaxChars, slot, nickname, result);
        return true;
    }

    // OFFSET 0x0204E378
    private static bool ScrCmd_GetGameCornerPrizeData(ScriptContext ctx)
    {
        ushort prize = VarValue(ctx);
        BufferView item = VarPointer(ctx);
        BufferView price = VarPointer(ctx);
        ushort[] entry = RomData.Arm9U16((uint)(GameCornerPrizeTable + prize * 4), 2);
        item.W16(0, entry[0]);
        price.W16(0, entry[1]);
        return false;
    }

    private const uint FirstArrivalEternaCity = 10;
    private const int JournalEventGameCorner = 0x10;
    private const int JournalEventUsedMoveBase = 0x13;
    private const int JournalHeapID = 4;

    // OFFSET 0x02043568
    private static bool ScrCmd_SaveExtraData(ScriptContext ctx)
    {
        SaveData.SaveDataExtra_Init(ctx.FieldSystem.SaveData!);
        return false;
    }

    // OFFSET 0x02043578
    private static bool ScrCmd_CheckIsMiscSaveInit(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, SaveData.SaveData_MiscSaveBlock_InitFlag(ctx.FieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x020435A0
    private static bool ScrCmd_EnablePoketch(ScriptContext ctx)
    {
        FieldPoketch.FieldTask_StartEnablePoketch(ctx.Task!);
        return true;
    }

    // OFFSET 0x020435AC
    private static bool ScrCmd_CheckPoketchEnabled(ScriptContext ctx)
    {
        Poketch poketch = FieldPoketch.SaveData_GetPoketch(ctx.FieldSystem.SaveData!);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, FieldPoketch.Poketch_IsEnabled(poketch));
        return false;
    }

    // OFFSET 0x020435DC
    private static bool ScrCmd_RegisterPoketchApp(ScriptContext ctx)
    {
        ushort appID = VarValue(ctx);
        Poketch.Poketch_RegisterApp(FieldPoketch.SaveData_GetPoketch(ctx.FieldSystem.SaveData!), appID);
        return false;
    }

    // OFFSET 0x02043608
    private static bool ScrCmd_CheckPoketchAppRegistered(ScriptContext ctx)
    {
        ushort appID = VarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, FieldPoketch.Poketch_IsAppRegistered(FieldPoketch.SaveData_GetPoketch(ctx.FieldSystem.SaveData!), appID));
        return false;
    }

    // OFFSET 0x02044528
    private static bool ScrCmd_GiveJournal(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        SystemFlags.SystemFlag_HandleJournalAcquired(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), SystemFlags.HandleFlagSet);
        fieldSystem.JournalEntry = Journal.Journal_GetSavedPage(Journal.SaveData_GetJournal(fieldSystem.SaveData!), 1);
        FieldMapChange.FieldMapChange_SaveJournalTitleForCurrentMap(fieldSystem);
        return false;
    }

    // OFFSET 0x02044558
    private static bool ScrCmd_CreateJournalEvent(ScriptContext ctx)
    {
        ushort eventType = VarValue(ctx);
        ushort param = VarValue(ctx);
        VarValue(ctx);
        VarValue(ctx);
        VarValue(ctx);
        ScriptManager manager = Manager(ctx);
        switch (eventType - JournalEventGameCorner)
        {
            case 0:
                manager.DataPtr = Journal.JournalEntry_CreateEventGameCorner(JournalHeapID);
                break;
            case 1:
                manager.DataPtr = Journal.JournalEntry_CreateEventSafariGame(JournalHeapID);
                break;
            case 2:
                manager.DataPtr = Journal.JournalEntry_CreateEventObtainedItem(param, JournalHeapID);
                break;
            case 3:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
                manager.DataPtr = Journal.JournalEntry_CreateEventUsedMove((byte)(eventType - JournalEventUsedMoveBase), param, JournalHeapID);
                break;
            case 20:
            case 21:
            case 22:
            case 23:
            case 24:
                manager.DataPtr = Journal.JournalEntry_CreateEventBattleFacility(JournalHeapID, eventType);
                break;
            default:
                return true;
        }
        Journal.JournalEntry_SaveData(ctx.FieldSystem.JournalEntry, manager.DataPtr!, 1);
        return true;
    }

    // OFFSET 0x02043190
    private static bool ScrCmd_GetLocalDexSeenCount(ScriptContext ctx)
    {
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(ctx.FieldSystem.SaveData!);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, Pokedex.Pokedex_CountSeen_Local(pokedex));
        return false;
    }

    // OFFSET 0x020431C0
    private static bool ScrCmd_GetLocalDexCaughtCount(ScriptContext ctx)
    {
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(ctx.FieldSystem.SaveData!);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, Pokedex.Pokedex_CountCaught_Local(pokedex));
        return false;
    }

    // OFFSET 0x020431F0
    private static bool ScrCmd_GetNationalDexSeenCount(ScriptContext ctx)
    {
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(ctx.FieldSystem.SaveData!);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, Pokedex.Pokedex_CountSeen_National(pokedex));
        return false;
    }

    // OFFSET 0x02043220
    private static bool ScrCmd_GetNationalDexCaughtCount(ScriptContext ctx)
    {
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(ctx.FieldSystem.SaveData!);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, Pokedex.Pokedex_CountCaught_National(pokedex));
        return false;
    }

    // OFFSET 0x02043250
    private static bool ScrCmd_Unused_122(ScriptContext ctx)
    {
        return false;
    }

    // OFFSET 0x02043254
    private static bool ScrCmd_LoadPokedexRating(ScriptContext ctx)
    {
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(ctx.FieldSystem.SaveData!);
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(ctx.FieldSystem.SaveData!);
        byte national = ScriptContext.ScriptContext_ReadByte(ctx);
        BufferView destination = VarPointer(ctx);
        if (national == 0)
        {
            int seen = Pokedex.Pokedex_NumSeen_Local(pokedex);
            bool visitedEterna = SystemFlags.SystemFlag_HandleFirstArrivalToZone(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!), SystemFlags.HandleFlagCheck, FirstArrivalEternaCity);
            destination.W16(0, PokedexRating.PokedexRating_GetLocalMessage(seen, (ushort)(visitedEterna ? 1 : 0)));
        }
        else
        {
            int caught = Pokedex.Pokedex_NumCaught_National(pokedex);
            destination.W16(0, PokedexRating.PokedexRating_GetNationalMessage(caught, (ushort)TrainerInfo.TrainerInfo_Gender(trainerInfo)));
        }
        return false;
    }

    // OFFSET 0x0204E3CC
    private static bool ScrCmd_CheckPokedexAcquired(ScriptContext ctx)
    {
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(ctx.FieldSystem.SaveData!);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, Pokedex.Pokedex_IsObtained(pokedex) ? 1 : 0);
        return false;
    }

    // OFFSET 0x0204E3FC
    private static bool ScrCmd_GivePokedex(ScriptContext ctx)
    {
        Pokedex.Pokedex_ObtainPokedex(Pokedex.SaveData_GetPokedex(ctx.FieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x0204E410
    private static bool ScrCmd_CheckRunningShoesAcquired(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        BufferView playerData = FieldPlayerState.FieldPlayerState_GetPlayerData(FieldPlayerState.SaveData_GetFieldPlayerState(ctx.FieldSystem.SaveData!));
        destination.W16(0, PlayerAvatar.PlayerData_HasRunningShoes(playerData));
        return false;
    }

    // OFFSET 0x0204E4A4
    private static bool ScrCmd_GiveBadge(ScriptContext ctx)
    {
        ushort badge = VarValue(ctx);
        if (badge >= BadgeCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        TrainerInfo.TrainerInfo_SetBadge(PlayerSave.SaveData_GetTrainerInfo(ctx.FieldSystem.SaveData!), badge);
        return false;
    }

    // OFFSET 0x0204E4D8
    private static bool ScrCmd_CheckBagAcquired(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, SystemFlags.SystemFlag_CheckBagAcquired(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!)) ? 1 : 0);
        return false;
    }

    // OFFSET 0x02044C64
    private static bool ScrCmd_SetNewsPressDeadline(ScriptContext ctx)
    {
        ushort deadline = VarValue(ctx);
        SystemVars.SystemVars_SetNewsPressDeadline(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!), deadline);
        return false;
    }

    // OFFSET 0x02044C90
    private static bool ScrCmd_GetNewsPressDeadline(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, SystemVars.SystemVars_GetNewsPressDeadline(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!)));
        return false;
    }

    // OFFSET 0x02044CBC
    private static bool ScrCmd_EnableSwarms(ScriptContext ctx)
    {
        SpecialEncounter.SpecialEncounter_EnableSwarms(ctx.FieldSystem.SaveData!);
        return false;
    }

    // OFFSET 0x02044CCC
    private static bool ScrCmd_ActivateRoamingPokemon(ScriptContext ctx)
    {
        byte slot = ScriptContext.ScriptContext_ReadByte(ctx);
        RoamingPokemon.RoamingPokemon_ActivateSlot(ctx.FieldSystem.SaveData!, slot);
        return false;
    }

    // OFFSET 0x02044DB4
    private static bool ScrCmd_TurnOnPokedexLanguageDetection(ScriptContext ctx)
    {
        Pokedex.Pokedex_TurnOnLanguageDetection(Pokedex.SaveData_GetPokedex(ctx.FieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x02044DC8
    private static bool ScrCmd_TurnOnPokedexFormDetection(ScriptContext ctx)
    {
        Pokedex.Pokedex_TurnOnFormDetection(Pokedex.SaveData_GetPokedex(ctx.FieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x02044DDC
    private static bool ScrCmd_GetSetNationalDexEnabled(ScriptContext ctx)
    {
        byte operation = ScriptContext.ScriptContext_ReadByte(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, 0);
        if (operation == NationalDexSet)
        {
            Pokedex.Pokedex_ObtainNationalDex(Pokedex.SaveData_GetPokedex(ctx.FieldSystem.SaveData!));
            TrainerInfo.TrainerInfo_GiveNationalDex(PlayerSave.SaveData_GetTrainerInfo(ctx.FieldSystem.SaveData!));
        }
        else if (operation == NationalDexGet)
        {
            destination.W16(0, Pokedex.Pokedex_IsNationalDexObtained(Pokedex.SaveData_GetPokedex(ctx.FieldSystem.SaveData!)) ? 1 : 0);
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        return false;
    }

    // OFFSET 0x02044E40
    private static bool ScrCmd_GetPartyMonEVTotal(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        ushort slot = VarValue(ctx);
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(ctx.FieldSystem.SaveData!), slot);
        uint hp = Pokemon.Pokemon_GetValue(mon, MonDataHPEV, null);
        uint attack = Pokemon.Pokemon_GetValue(mon, MonDataHPEV + 1, null);
        uint defense = Pokemon.Pokemon_GetValue(mon, MonDataHPEV + 2, null);
        uint speed = Pokemon.Pokemon_GetValue(mon, MonDataHPEV + 3, null);
        uint spAttack = Pokemon.Pokemon_GetValue(mon, MonDataHPEV + 4, null);
        uint spDefense = Pokemon.Pokemon_GetValue(mon, MonDataHPEV + 5, null);
        destination.W16(0, (int)(spDefense + spAttack + speed + defense + hp + attack));
        return false;
    }

    // OFFSET 0x02044EDC
    private static bool ScrCmd_GetDayOfWeek(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        Rtc.GetCurrentDate(out RtcDate date);
        destination.W16(0, date.Week);
        return false;
    }

    // OFFSET 0x02044820
    private static bool ScrCmd_CheckNationalDexCompleted(ScriptContext ctx)
    {
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(ctx.FieldSystem.SaveData!);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, 0);
        if (Pokedex.Pokedex_NationalDexCompleted(pokedex))
        {
            destination.W16(0, 1);
        }
        return false;
    }

    // OFFSET 0x020448F0
    private static bool ScrCmd_Unused_1EF(ScriptContext ctx)
    {
        VarPointer(ctx);
        return false;
    }

    // OFFSET 0x02044908
    private static bool ScrCmd_IncrementGameRecord(ScriptContext ctx)
    {
        ushort record = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        GameRecords.GameRecords_IncrementRecordValue(GameRecords.SaveData_GetGameRecords(ctx.FieldSystem.SaveData!), record);
        return false;
    }

    // OFFSET 0x02044928
    private static bool ScrCmd_Unused_1E6(ScriptContext ctx)
    {
        ushort record = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort highVar = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort lowVar = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        BufferView high = FieldSystemVarsFlags.FieldSystem_GetVarPointer(ctx.FieldSystem, highVar)!;
        BufferView low = FieldSystemVarsFlags.FieldSystem_GetVarPointer(ctx.FieldSystem, lowVar)!;
        uint value = GameRecords.GameRecords_GetRecordValue(GameRecords.SaveData_GetGameRecords(ctx.FieldSystem.SaveData!), record);
        high.W16(0, (int)((value & 0xFFFF0000) >> 16));
        low.W16(0, (int)(value & 0xFFFF));
        return false;
    }

    // OFFSET 0x02044980
    private static bool ScrCmd_Unused_1E7(ScriptContext ctx)
    {
        ushort record = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort high = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort low = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        byte operation = ScriptContext.ScriptContext_ReadByte(ctx);
        uint value = ((uint)high << 16) | low;
        GameRecords records = GameRecords.SaveData_GetGameRecords(ctx.FieldSystem.SaveData!);
        switch (operation)
        {
            case 0:
                GameRecords.GameRecords_AddToRecordValue(records, record, value);
                break;
            case 1:
                GameRecords.GameRecords_SetRecordValue(records, record, value);
                break;
            case 2:
                GameRecords.GameRecords_SetAndLimitRecordValue(records, record, value);
                break;
        }
        return false;
    }

    // OFFSET 0x020449F4
    private static bool ScrCmd_AddToGameRecord(ScriptContext ctx)
    {
        ushort record = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort value = VarValue(ctx);
        GameRecords.GameRecords_AddToRecordValue(GameRecords.SaveData_GetGameRecords(ctx.FieldSystem.SaveData!), record, value);
        return false;
    }

    // OFFSET 0x02044A28
    private static bool ScrCmd_AddToGameRecordBigValue(ScriptContext ctx)
    {
        ushort record = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        uint value = ScriptContext.ScriptContext_ReadWord(ctx);
        GameRecords.GameRecords_AddToRecordValue(GameRecords.SaveData_GetGameRecords(ctx.FieldSystem.SaveData!), record, value);
        return false;
    }

    // OFFSET 0x02045E14
    private static bool ScrCmd_GetUndergroundTalkCounter(ScriptContext ctx)
    {
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, SystemVars.SystemVars_GetUndergroundTalkCounter(varsFlags));
        return false;
    }

    // OFFSET 0x02045EA0
    private static bool ScrCmd_GetWiFListValidFriendsCount(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, WiFiList.WiFiList_GetValidFriendsCount(WiFiList.SaveData_GetWiFiList(ctx.FieldSystem.SaveData!)));
        return false;
    }

    // OFFSET 0x02045ECC
    private static bool ScrCmd_TrySetUnusedCollectedOrbFlag(ScriptContext ctx)
    {
        ushort value = VarValue(ctx);
        Underground underground = Underground.SaveData_GetUnderground(ctx.FieldSystem.SaveData!);
        if ((ushort)(value - OrbFlagFirstValue) <= 1)
        {
            Underground.Underground_SetUnusedCollectedOrbFlag(underground);
        }
        return false;
    }

    // OFFSET 0x02045F04
    private static bool ScrCmd_CheckItemIsPlate(ScriptContext ctx)
    {
        ushort item = VarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, 0);
        if (item >= FirstPlate && item <= LastPlate)
        {
            destination.W16(0, 1);
        }
        return false;
    }

    // OFFSET 0x0204E704
    private static bool ScrCmd_DoDefogFunc(ScriptContext ctx)
    {
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!);
        switch (ScriptContext.ScriptContext_ReadByte(ctx))
        {
            case DefogFuncClear:
                SystemFlags.SystemFlag_ClearDefogActive(varsFlags);
                break;
            case DefogFuncSet:
                SystemFlags.SystemFlag_SetDefogActive(varsFlags);
                break;
            case DefogFuncCheck:
            {
                BufferView destination = VarPointer(ctx);
                destination.W16(0, SystemFlags.SystemFlag_CheckDefogActive(varsFlags) ? 1 : 0);
                break;
            }
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return false;
    }

    // OFFSET 0x0204E75C
    private static bool ScrCmd_AddItem(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort item = ReadVarValue(ctx);
        ushort count = ReadVarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, BagItems.Bag_TryAddItem(BagItems.SaveData_GetBag(fieldSystem.SaveData!), item, count, HeapIDField) ? 1 : 0);
        return false;
    }

    // OFFSET 0x0204E7B4
    private static bool ScrCmd_RemoveItem(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort item = ReadVarValue(ctx);
        ushort count = ReadVarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, BagItems.Bag_TryRemoveItem(BagItems.SaveData_GetBag(fieldSystem.SaveData!), item, count, HeapIDField) ? 1 : 0);
        return false;
    }

    // OFFSET 0x0204E864
    private static bool ScrCmd_CheckItem(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort item = ReadVarValue(ctx);
        ushort count = ReadVarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, BagItems.Bag_CanRemoveItem(BagItems.SaveData_GetBag(fieldSystem.SaveData!), item, count, HeapIDFieldTask) ? 1 : 0);
        return false;
    }

    // OFFSET 0x0204E8BC
    private static bool ScrCmd_GetItemQuantity(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort item = ReadVarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, BagItems.Bag_GetItemQuantity(BagItems.SaveData_GetBag(fieldSystem.SaveData!), item, HeapIDFieldTask));
        return false;
    }

    // OFFSET 0x0204E900
    private static bool ScrCmd_IsItemTMHM(ScriptContext ctx)
    {
        ushort item = VarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, FieldItemHelpers.Item_IsTMHM(item));
        return false;
    }

    // OFFSET 0x0204E934
    private static bool ScrCmd_GetItemPocket(ScriptContext ctx)
    {
        ushort item = VarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, Item.Item_LoadParam(item, ItemParamPocket, HeapIDFieldTask));
        return false;
    }

    // OFFSET 0x0204E96C
    private static bool ScrCmd_Dummy081(ScriptContext ctx)
    {
        return false;
    }

    // OFFSET 0x0204E970
    private static bool ScrCmd_Dummy082(ScriptContext ctx)
    {
        return false;
    }

    // OFFSET 0x0204D08C
    private static bool ScrCmd_GetPartyMonSpecies(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        BufferView slot = VarPointer(ctx);
        BufferView destination = VarPointer(ctx);
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), slot.U16(0));
        if (Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) == 0)
        {
            destination.W16(0, (int)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null));
        }
        else
        {
            destination.W16(0, 0);
        }
        return false;
    }

    // OFFSET 0x0204D0E8
    private static bool ScrCmd_CheckIsPartyMonOutsider(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(FieldSystemCore.FieldSystem_GetSaveData(fieldSystem));
        BufferView slot = VarPointer(ctx);
        BufferView destination = VarPointer(ctx);
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), slot.U16(0));
        ushort otID = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataOTID, null);
        destination.W16(0, otID == (ushort)TrainerInfo.TrainerInfo_ID(trainerInfo) ? 0 : 1);
        return false;
    }

    // OFFSET 0x02044328
    private static bool ScrCmd_CountMailInMailbox(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        BufferView destination = VarPointer(ctx);
        destination.W16(0, Mailbox.Mailbox_CountUsedSlots(Mailbox.SaveData_GetMailbox(fieldSystem.SaveData!), 0));
        return false;
    }

    // OFFSET 0x02044374
    private static bool ScrCmd_GetTimeOfDay(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, FieldTime.FieldSystem_GetTimeOfDay(ctx.FieldSystem));
        return false;
    }

    // OFFSET 0x02044398
    private static bool ScrCmd_GetRandom(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        ushort max = VarValue(ctx);
        destination.W16(0, MathUtil.LCRNG_Next() % max);
        return true;
    }

    // OFFSET 0x020443D0
    private static bool ScrCmd_GetRandom2(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        ushort max = VarValue(ctx);
        destination.W16(0, MathUtil.LCRNG_Next() % max);
        return true;
    }

    // OFFSET 0x02042984
    private static bool ScrCmd_Warp(ScriptContext ctx)
    {
        ushort mapID = ReadVarValue(ctx);
        ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort x = ReadVarValue(ctx);
        ushort z = ReadVarValue(ctx);
        ushort direction = ReadVarValue(ctx);
        FieldMapChange.FieldTask_StartMapChangeFull(ctx.Task!, mapID, WarpIDNone, x, z, direction);
        return true;
    }

    // OFFSET 0x0204342C
    private static bool ScrCmd_StartFirstBattle(ScriptContext ctx)
    {
        ScriptManager manager = Manager(ctx);
        ushort trainerID = ReadVarValue(ctx);
        Encounters.Encounter_NewVsFirstBattle(ctx.Task!, trainerID, HeapIDFieldTask, result => manager.BattleResult = result);
        return true;
    }

    // OFFSET 0x02043460
    private static bool ScrCmd_StartCatchingTutorial(ScriptContext ctx)
    {
        Encounters.Encounter_StartCatchingTutorial(ctx.Task!);
        return true;
    }

    // OFFSET 0x02043C3C
    private static bool ScrCmd_HealParty(ScriptContext ctx)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(ctx.Task!);
        PartyHeal.Party_HealAllMembers(Party.SaveData_GetParty(fieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x02047998
    private static GameString? Species_GetNameString(ushort species, int heapID)
    {
        MessageLoader? loader = MessageLoader.MessageLoader_Init(MessageLoaderTypeNarc, NarcMessageBanks, MessageBankSpeciesNames, heapID);
        GameString? name = MessageLoader.MessageLoader_GetNewString(loader!, species);
        MessageLoader.MessageLoader_Free(loader);
        return name;
    }

    // OFFSET 0x020479C0
    private static bool ScrCmd_BufferPlayerStarterSpeciesName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort species = SystemVars.SystemVars_GetPlayerStarter(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        GameString? name = Species_GetNameString(species, HeapIDField);
        StringTemplate.StringTemplate_SetString(template, templateArg, name!, 0, 1, 2);
        GameString.String_Free(name);
        return false;
    }

    // OFFSET 0x02047A10
    private static bool ScrCmd_BufferRivalStarterSpeciesName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort species = SystemVars.SystemVars_GetRivalStarter(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        GameString? name = Species_GetNameString(species, HeapIDField);
        StringTemplate.StringTemplate_SetString(template, templateArg, name!, 0, 1, 2);
        GameString.String_Free(name);
        return false;
    }

    // OFFSET 0x02047A60
    private static bool ScrCmd_BufferCounterpartStarterSpeciesName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort species = SystemVars.SystemVars_GetPlayerCounterpartStarter(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        GameString? name = Species_GetNameString(species, HeapIDField);
        StringTemplate.StringTemplate_SetString(template, templateArg, name!, 0, 1, 2);
        GameString.String_Free(name);
        return false;
    }

    // OFFSET 0x02048A50
    private static bool ScrCmd_CheckWonBattle(ScriptContext ctx)
    {
        ScriptManager manager = Manager(ctx);
        BufferView destVar = VarPointer(ctx);
        destVar.W16(0, FieldBattleResult.CheckPlayerWonBattle(manager.BattleResult));
        return true;
    }

    // OFFSET 0x02048A80
    private static bool ScrCmd_CheckLostBattle(ScriptContext ctx)
    {
        ScriptManager manager = Manager(ctx);
        BufferView destVar = VarPointer(ctx);
        destVar.W16(0, FieldBattleResult.CheckPlayerLostBattle(manager.BattleResult));
        return true;
    }

    // OFFSET 0x02048AB0
    private static bool ScrCmd_CheckDidNotCapture(ScriptContext ctx)
    {
        ScriptManager manager = Manager(ctx);
        BufferView destVar = VarPointer(ctx);
        destVar.W16(0, FieldBattleResult.CheckPlayerDidNotCapture(manager.BattleResult));
        return true;
    }

    // OFFSET 0x0204E594
    private static bool ScrCmd_SetHasPartner(ScriptContext ctx)
    {
        SystemFlags.SystemFlag_SetHasPartner(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x0204E5A8
    private static bool ScrCmd_ClearHasPartner(ScriptContext ctx)
    {
        SystemFlags.SystemFlag_ClearHasPartner(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x0204E5BC
    private static bool ScrCmd_CheckStepFlag(ScriptContext ctx)
    {
        BufferView destVar = VarPointer(ctx);
        destVar.W16(0, SystemFlags.SystemFlag_CheckStep(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!)) ? 1 : 0);
        return false;
    }

    // OFFSET 0x0204E5E8
    private static bool ScrCmd_SetStepFlag(ScriptContext ctx)
    {
        SystemFlags.SystemFlag_SetStep(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x0204E5FC
    private static bool ScrCmd_ClearStepFlag(ScriptContext ctx)
    {
        SystemFlags.SystemFlag_ClearStep(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x0204E80C
    private static bool ScrCmd_CanFitItem(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort item = ReadVarValue(ctx);
        ushort count = ReadVarValue(ctx);
        BufferView destVar = VarPointer(ctx);
        destVar.W16(0, BagItems.Bag_CanFitItem(BagItems.SaveData_GetBag(fieldSystem.SaveData!), item, count, HeapIDField) ? 1 : 0);
        return false;
    }
}
