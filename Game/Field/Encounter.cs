using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class Encounter
{
    public Action<int>? SetResult;
    public int EncounterEffectID;
    public int BattleBGM;
    public int State;
    public FieldBattleDTO DTO = null!;
}

public static class Encounters
{
    private const int HeapIDFieldTask = 0xB;
    private const uint BattleTypeUpdated = 0x80000000;
    private const uint BattleTypeLegendaryWild = 0x4A;
    private const int EffectTypeCount = 0x23;
    private const int EffectTypeDefault = 0x21;
    private const int EffectTypeWild = 0x22;
    private const int EffectTypeFrontier = 0x1F;
    private const uint EffectTable = 0x020EC208;
    private const int EffectTableEntrySize = 4;
    private const ushort EffectByTerrain = 0xFFFF;
    private const int MapHeaderDistortionWorldTurnback = 0xFB;
    private const int RecordWildBattles = 7;
    private const int RecordTrainerBattles = 8;
    private const uint BattleTypeTrainerDoubleSingleTrainer = 3;
    private const uint BattleTypeTrainerDouble = 0x13;
    private const uint BattleTypeTagDouble = 0x4B;
    private const int MapHeaderDistortionWorldFirst = 0x23D;
    private const int MapHeaderDistortionWorldLast = 0x247;
    private const uint BattleStatusDistortion = 0x80;
    private const int ScoreWildDefeated = 8;
    private const int ScoreCaughtInDex = 9;
    private const int ScoreCaughtNotInDex = 0xA;
    private const int ScoreTrainerDefeated = 0xB;
    private const int WildMonsPerJournalEntry = 5;
    private const int JournalDataMon = 2;
    private const int ParamSpecies = 5;
    private const int ParamGender = 111;
    private const int ParamLevel = 161;
    private const int TerrainCave = 5;
    private const int TerrainWater = 7;
    private const uint BattleTypeNonTrainerSpecial = 0x720;
    private const int BattleResultWin = 1;
    private const int BattleResultCaught = 4;

    private static ushort[]? effectTable;

    // OFFSET 0x02050ABC
    private static void FieldTask_StartBattle(FieldTask task, FieldBattleDTO dto)
    {
        FieldTask.FieldTask_InitCall(task, FieldTask.FieldTask_RunBattleProcess, dto);
    }

    // OFFSET 0x02050ACC
    private static Encounter Encounter_New(FieldBattleDTO dto, int effectID, int battleBGM, Action<int>? setResult)
    {
        Encounter encounter = new();
        encounter.SetResult = setResult;
        setResult?.Invoke(0);
        encounter.EncounterEffectID = effectID;
        encounter.BattleBGM = battleBGM;
        encounter.DTO = dto;
        return encounter;
    }

    // OFFSET 0x02050AF0
    private static void Encounter_Free(Encounter encounter)
    {
        FieldBattleDTO.FieldBattleDTO_Free(encounter.DTO);
    }

    // OFFSET 0x02050B04
    private static int Encounter_StoreResultAndCheckWon(Encounter encounter)
    {
        encounter.SetResult?.Invoke(encounter.DTO.ResultMask);
        return FieldBattleResult.CheckPlayerWonBattle(encounter.DTO.ResultMask);
    }

    // OFFSET 0x02050B1C
    private static void Encounter_UpdateFieldSystem(FieldBattleDTO dto, FieldSystem fieldSystem)
    {
        if ((dto.BattleType & BattleTypeUpdated) == 0)
        {
            FieldBattleDTO.FieldBattleDTO_UpdateFieldSystem(dto, fieldSystem);
        }
    }

    // OFFSET 0x02050B30
    private static bool FieldTask_Encounter(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        Encounter encounter = (Encounter)FieldTask.FieldTask_GetEnv(task)!;
        ref int state = ref FieldTask.FieldTask_GetState(task);
        switch (state)
        {
            case 0:
                MapObjectManager.MapObjectMan_PauseAllMovement(fieldSystem.MapObjectManager!);
                FieldTransition.FieldTransition_StartEncounterEffect(task, encounter.EncounterEffectID, encounter.BattleBGM);
                state++;
                break;
            case 1:
                FieldTransition.FieldTransition_FinishMap(task);
                state++;
                break;
            case 2:
                FieldTask_StartBattle(task, encounter.DTO);
                state++;
                break;
            case 3:
            {
                FieldBattleDTO dto = encounter.DTO;
                Encounter_UpdateFieldSystem(dto, fieldSystem);
                if (dto.BattleType == FieldBattleDTO.BattleTypeWild || dto.BattleType == FieldBattleDTO.BattleTypeRoamer || dto.BattleType == BattleTypeLegendaryWild)
                {
                    FieldTimeEvents.FieldSystem_SaveTVSegment_CatchThatPokemonShow(fieldSystem, dto.CaptureAttempt, dto.ResultMask);
                }
                if (Encounter_StoreResultAndCheckWon(encounter) == 0)
                {
                    Encounter_Free(encounter);
                    return true;
                }
                if (SystemFlags.SystemFlag_CheckHasPartner(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)))
                {
                    PartyHeal.Party_HealAllMembers(Party.SaveData_GetParty(fieldSystem.SaveData!));
                }
                FieldSystem_UpdateBattleScores(fieldSystem, dto);
                FieldSystem_UpdateBattleJournal(fieldSystem, dto);
                SystemVars.SystemVars_SetTotalTurnsForLastBattle(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), (ushort)dto.TotalTurnsElapsed);
                FieldTransition.FieldTransition_StartMap(task);
                state++;
                break;
            }
            case 4:
                MapObjectManager.MapObjectMan_UnpauseAllMovement(fieldSystem.MapObjectManager!);
                FieldTransition.FieldTransition_FadeIn(task);
                state++;
                break;
            case 5:
                Encounter_Free(encounter);
                return true;
        }
        return false;
    }

    // OFFSET 0x02050DD4
    private static Encounter NewWildEncounter(FieldBattleDTO dto, int effectID, int battleBGM, Action<int>? setResult)
    {
        Encounter encounter = new();
        encounter.SetResult = setResult;
        setResult?.Invoke(0);
        encounter.EncounterEffectID = effectID;
        encounter.BattleBGM = battleBGM;
        encounter.DTO = dto;
        encounter.State = 0;
        return encounter;
    }

    // OFFSET 0x02050DFC
    private static void FreeWildEncounter(Encounter encounter)
    {
        FieldBattleDTO.FieldBattleDTO_Free(encounter.DTO);
    }

    // OFFSET 0x02050E10
    public static void Encounter_NewVsWild(FieldSystem fieldSystem, FieldBattleDTO dto)
    {
        if (SystemFlags.SystemFlag_CheckSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)))
        {
            Encounter safari = Encounter_New(dto, EncounterEffect_GetEffectID(dto), EncounterEffect_GetBattleBGM(dto), null);
            FieldTask.FieldSystem_CreateTask(fieldSystem, FieldTask_SafariEncounter, safari);
            return;
        }
        Encounter encounter = NewWildEncounter(dto, EncounterEffect_GetEffectID(dto), EncounterEffect_GetBattleBGM(dto), null);
        FieldTask.FieldSystem_CreateTask(fieldSystem, FieldTask_WildEncounter, encounter);
    }

    // OFFSET 0x02050E78
    public static void Encounter_StartVsWild(FieldSystem fieldSystem, FieldTask task, FieldBattleDTO dto)
    {
        if (SystemFlags.SystemFlag_CheckSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)))
        {
            Encounter safari = Encounter_New(dto, EncounterEffect_GetEffectID(dto), EncounterEffect_GetBattleBGM(dto), null);
            FieldTask.FieldTask_InitJump(task, FieldTask_SafariEncounter, safari);
            return;
        }
        Encounter encounter = NewWildEncounter(dto, EncounterEffect_GetEffectID(dto), EncounterEffect_GetBattleBGM(dto), null);
        FieldTask.FieldTask_InitJump(task, FieldTask_WildEncounter, encounter);
    }

    // OFFSET 0x02050EE0
    private static bool FieldTask_WildEncounter(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        Encounter encounter = (Encounter)FieldTask.FieldTask_GetEnv(task)!;
        switch (encounter.State)
        {
            case 0:
                MapObjectManager.MapObjectMan_PauseAllMovement(fieldSystem.MapObjectManager!);
                GameRecords.GameRecords_IncrementRecordValue(GameRecords.SaveData_GetGameRecords(fieldSystem.SaveData!), RecordWildBattles);
                FieldTransition.FieldTransition_StartEncounterEffect(task, encounter.EncounterEffectID, encounter.BattleBGM);
                encounter.State++;
                break;
            case 1:
                FieldTransition.FieldTransition_FinishMap(task);
                encounter.State++;
                break;
            case 2:
                FieldTask_StartBattle(task, encounter.DTO);
                encounter.State++;
                break;
            case 3:
            {
                FieldBattleDTO dto = encounter.DTO;
                Encounter_UpdateFieldSystem(dto, fieldSystem);
                FieldTimeEvents.FieldSystem_SaveTVSegment_CatchThatPokemonShow(fieldSystem, dto.CaptureAttempt, dto.ResultMask);
                if (FieldBattleResult.CheckPlayerWonBattle(dto.ResultMask) == 0)
                {
                    FreeWildEncounter(encounter);
                    RadarChain.RadarChain_Clear(fieldSystem.RadarChain!);
                    FieldTask.FieldTask_InitJump(task, BlackOut.FieldTask_BlackOut, null);
                    return false;
                }
                if (SystemFlags.SystemFlag_CheckHasPartner(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)))
                {
                    PartyHeal.Party_HealAllMembers(Party.SaveData_GetParty(fieldSystem.SaveData!));
                }
                FieldSystem_UpdateBattleScores(fieldSystem, dto);
                FieldSystem_UpdateBattleJournal(fieldSystem, dto);
                if (RadarChain.RadarChain_GetActive(fieldSystem.RadarChain!) != 0)
                {
                    if (RadarChain.RadarChain_GetPatchEntered(fieldSystem.RadarChain!) != 0)
                    {
                        if (dto.ResultMask != BattleResultWin && dto.ResultMask != BattleResultCaught)
                        {
                            RadarChain.RadarChain_Clear(fieldSystem.RadarChain!);
                        }
                    }
                    else
                    {
                        RadarChain.RadarChain_Clear(fieldSystem.RadarChain!);
                    }
                }
                FieldTransition.FieldTransition_StartMap(task);
                encounter.State++;
                break;
            }
            case 4:
                RoamerAfterBattle.RoamerAfterBattle_UpdateRoamers(fieldSystem, encounter.DTO);
                FieldTransition.FieldTransition_FadeIn(task);
                encounter.State++;
                break;
            case 5:
                if (RadarChain.RadarChain_GetActive(fieldSystem.RadarChain!) != 0)
                {
                    RadarChain.PokeRadar_SetupGrassPatches(fieldSystem, encounter.DTO.ResultMask, fieldSystem.RadarChain!);
                    RadarChain.PokeRadar_CreateShakingPatches(fieldSystem, fieldSystem.RadarChain!);
                }
                encounter.State++;
                break;
            case 6:
                if (RadarChain.PokeRadar_FinishPatchShakes(fieldSystem.RadarChain!))
                {
                    MapObjectManager.MapObjectMan_UnpauseAllMovement(fieldSystem.MapObjectManager!);
                    FreeWildEncounter(encounter);
                    return true;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x02051074
    private static bool FieldTask_SafariEncounter(FieldTask task)
    {
        throw Unported.Partial(0x02051074);
    }

    // OFFSET 0x02050C4C
    private static void Encounter_Start(FieldTask task, FieldBattleDTO dto, int effectID, int battleBGM, Action<int>? setResult)
    {
        Encounter encounter = Encounter_New(dto, effectID, battleBGM, setResult);
        FieldTask.FieldTask_InitCall(task, FieldTask_Encounter, encounter);
    }

    // OFFSET 0x02051480
    public static void Encounter_NewVsFirstBattle(FieldTask task, int trainerID, int heapID, Action<int>? setResult)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        FieldBattleDTO dto = FieldBattleDTO.FieldBattleDTO_New(HeapIDFieldTask, FieldBattleDTO.BattleTypeTrainer);
        FieldBattleDTO.FieldBattleDTO_Init(dto, fieldSystem);
        dto.BattleStatusMask = 1;
        dto.TrainerIDs[FieldBattleDTO.BattlerEnemy1] = trainerID;
        dto.TrainerIDs[FieldBattleDTO.BattlerEnemy2] = 0;
        dto.TrainerIDs[FieldBattleDTO.BattlerPartner] = 0;
        TrainerData.Trainer_Encounter(dto, fieldSystem.SaveData!, heapID);
        GameRecords.GameRecords_IncrementRecordValue(GameRecords.SaveData_GetGameRecords(fieldSystem.SaveData!), RecordTrainerBattles);
        Encounter_Start(task, dto, EncounterEffect_GetEffectID(dto), EncounterEffect_GetBattleBGM(dto), setResult);
    }

    // OFFSET 0x020514E8
    private static bool FieldTask_CatchingTutorialEncounter(FieldTask task)
    {
        Encounter encounter = (Encounter)FieldTask.FieldTask_GetEnv(task)!;
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        ref int state = ref FieldTask.FieldTask_GetState(task);
        switch (state)
        {
            case 0:
                MapObjectManager.MapObjectMan_PauseAllMovement(fieldSystem.MapObjectManager!);
                FieldTransition.FieldTransition_StartEncounterEffect(task, encounter.EncounterEffectID, encounter.BattleBGM);
                state++;
                break;
            case 1:
                FieldTransition.FieldTransition_FinishMap(task);
                state++;
                break;
            case 2:
                FieldTask_StartBattle(task, encounter.DTO);
                state++;
                break;
            case 3:
                state++;
                break;
            case 4:
                FieldTransition.FieldTransition_StartMap(task);
                state++;
                break;
            case 5:
                MapObjectManager.MapObjectMan_UnpauseAllMovement(fieldSystem.MapObjectManager!);
                FieldTransition.FieldTransition_FadeIn(task);
                state++;
                break;
            case 6:
                Encounter_Free(encounter);
                return true;
        }
        return false;
    }

    // OFFSET 0x02051590
    public static void Encounter_StartCatchingTutorial(FieldTask task)
    {
        FieldBattleDTO dto = FieldBattleDTO.FieldBattleDTO_NewCatchingTutorial(HeapIDFieldTask, FieldTask.FieldTask_GetFieldSystem(task));
        int effectID = EncounterEffect_GetEffectID(dto);
        Encounter encounter = Encounter_New(dto, effectID, EncounterEffect_GetBattleBGM(dto), null);
        FieldTask.FieldTask_InitCall(task, FieldTask_CatchingTutorialEncounter, encounter);
    }

    // OFFSET 0x020515CC
    public static void Encounter_NewVsTrainer(FieldTask task, int enemyTrainer1, int enemyTrainer2, int partnerTrainer, int heapID, Action<int>? setResult)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        uint battleType;
        if (enemyTrainer2 != 0 && enemyTrainer1 != enemyTrainer2)
        {
            battleType = partnerTrainer == 0 ? BattleTypeTrainerDouble : BattleTypeTagDouble;
        }
        else if (enemyTrainer1 == enemyTrainer2)
        {
            battleType = BattleTypeTrainerDoubleSingleTrainer;
        }
        else
        {
            battleType = FieldBattleDTO.BattleTypeTrainer;
        }
        RadarChain.RadarChain_Clear(fieldSystem.RadarChain!);
        FieldBattleDTO dto = FieldBattleDTO.FieldBattleDTO_New(HeapIDFieldTask, battleType);
        FieldBattleDTO.FieldBattleDTO_Init(dto, fieldSystem);
        int mapHeaderID = (int)fieldSystem.Location!.U32(0);
        if (mapHeaderID >= MapHeaderDistortionWorldFirst && mapHeaderID <= MapHeaderDistortionWorldLast)
        {
            dto.BattleStatusMask |= BattleStatusDistortion;
        }
        dto.TrainerIDs[FieldBattleDTO.BattlerEnemy1] = enemyTrainer1;
        dto.TrainerIDs[FieldBattleDTO.BattlerEnemy2] = enemyTrainer2;
        dto.TrainerIDs[FieldBattleDTO.BattlerPartner] = partnerTrainer;
        TrainerData.Trainer_Encounter(dto, fieldSystem.SaveData!, heapID);
        GameRecords.GameRecords_IncrementRecordValue(GameRecords.SaveData_GetGameRecords(fieldSystem.SaveData!), RecordTrainerBattles);
        int effectID = EncounterEffect_GetEffectID(dto);
        Encounter_Start(task, dto, effectID, EncounterEffect_GetBattleBGM(dto), setResult);
    }

    // OFFSET 0x020518B0
    private static void FieldSystem_UpdateBattleScores(FieldSystem fieldSystem, FieldBattleDTO dto)
    {
        uint battleType = dto.BattleType;
        int result = dto.ResultMask;
        if ((battleType & FieldBattleDTO.BattleTypeLink) != 0 || (battleType & FieldBattleDTO.BattleTypeFrontier) != 0)
        {
            return;
        }
        if (battleType == FieldBattleDTO.BattleTypeWild || battleType == FieldBattleDTO.BattleTypeRoamer || battleType == BattleTypeLegendaryWild)
        {
            if (result == FieldBattleResult.Win)
            {
                GameRecords.GameRecords_IncrementTrainerScore(GameRecords.SaveData_GetGameRecords(fieldSystem.SaveData!), ScoreWildDefeated);
            }
            else if (result == FieldBattleResult.Captured)
            {
                FieldSystem_ScoreCapture(fieldSystem, dto);
            }
        }
        else if ((battleType & FieldBattleDTO.BattleTypeTrainer) != 0 || (battleType & 0x10) != 0)
        {
            if (result == FieldBattleResult.Win)
            {
                GameRecords.GameRecords_IncrementTrainerScore(GameRecords.SaveData_GetGameRecords(fieldSystem.SaveData!), ScoreTrainerDefeated);
            }
        }
        else if ((battleType & FieldBattleDTO.BattleTypeSafari) != 0 || (battleType & FieldBattleDTO.BattleTypePalPark) != 0)
        {
            if (result == FieldBattleResult.Captured)
            {
                FieldSystem_ScoreCapture(fieldSystem, dto);
            }
        }
    }

    private static void FieldSystem_ScoreCapture(FieldSystem fieldSystem, FieldBattleDTO dto)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(dto.Parties[FieldBattleDTO.BattlerEnemy1]!, 0);
        int species = (int)Pokemon.Pokemon_GetValue(mon, ParamSpecies, null);
        if (DexMode.Pokemon_GetDexNumber(0, species) != 0)
        {
            GameRecords.GameRecords_IncrementTrainerScore(GameRecords.SaveData_GetGameRecords(fieldSystem.SaveData!), ScoreCaughtInDex);
        }
        else
        {
            GameRecords.GameRecords_IncrementTrainerScore(GameRecords.SaveData_GetGameRecords(fieldSystem.SaveData!), ScoreCaughtNotInDex);
        }
    }

    // OFFSET 0x02051988
    private static void FieldSystem_UpdateBattleJournal(FieldSystem fieldSystem, FieldBattleDTO dto)
    {
        uint battleType = dto.BattleType;
        int result = dto.ResultMask;
        if ((battleType & FieldBattleDTO.BattleTypeLink) != 0 || (battleType & FieldBattleDTO.BattleTypeFrontier) != 0 || (battleType & FieldBattleDTO.BattleTypePalPark) != 0)
        {
            return;
        }
        if (battleType == FieldBattleDTO.BattleTypeWild || battleType == FieldBattleDTO.BattleTypeRoamer || battleType == BattleTypeLegendaryWild || battleType == FieldBattleDTO.BattleTypeSafari)
        {
            if (result == FieldBattleResult.Win)
            {
                fieldSystem.WildMonsDefeated++;
                if (fieldSystem.WildMonsDefeated >= WildMonsPerJournalEntry)
                {
                    Pokemon mon = Party.Party_GetPokemonBySlotIndex(dto.Parties[FieldBattleDTO.BattlerEnemy1]!, 0);
                    PlayTime playTime = PlayerSave.SaveData_GetPlayTime(fieldSystem.SaveData!);
                    ushort species = (ushort)Pokemon.Pokemon_GetValue(mon, ParamSpecies, null);
                    byte gender = (byte)Pokemon.Pokemon_GetValue(mon, ParamGender, null);
                    JournalMonEvent data = Journal.JournalEntry_CreateEventMonDefeated(playTime, species, gender, (byte)dto.TimeOfDay, HeapIDFieldTask);
                    Journal.JournalEntry_SaveData(fieldSystem.JournalEntry, data, JournalDataMon);
                }
            }
            else if (result == FieldBattleResult.Captured)
            {
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(dto.Parties[dto.CaughtBattlerIndex]!, 0);
                PlayTime playTime = PlayerSave.SaveData_GetPlayTime(fieldSystem.SaveData!);
                ushort species = (ushort)Pokemon.Pokemon_GetValue(mon, ParamSpecies, null);
                byte gender = (byte)Pokemon.Pokemon_GetValue(mon, ParamGender, null);
                JournalMonEvent data = Journal.JournalEntry_CreateEventMonCaught(playTime, species, gender, (byte)dto.TimeOfDay, HeapIDFieldTask);
                Journal.JournalEntry_SaveData(fieldSystem.JournalEntry, data, JournalDataMon);
            }
        }
        else if ((battleType & FieldBattleDTO.BattleTypeTrainer) != 0 || (battleType & 0x10) != 0)
        {
            if (result == FieldBattleResult.Win)
            {
                Journal.JournalEntry_CreateAndSaveEventTrainer(fieldSystem.JournalEntry, (ushort)fieldSystem.Location!.U32(0), (ushort)dto.TrainerIDs[FieldBattleDTO.BattlerEnemy1], HeapIDFieldTask);
            }
        }
    }

    // OFFSET 0x02051B50
    private static int EncounterEffect_GetType(FieldBattleDTO dto)
    {
        uint battleType = dto.BattleType;
        if ((battleType & FieldBattleDTO.BattleTypeTrainer) != 0)
        {
            int type = TrainerClass_GetEffectType(dto.Trainers[FieldBattleDTO.BattlerEnemy1][TrainerData.ClassOffset]);
            if ((battleType & FieldBattleDTO.BattleTypeFrontier) != 0)
            {
                if (type == EffectTypeFrontier)
                {
                    return type;
                }
                return (battleType & FieldBattleDTO.BattleTypeDoubles) != 0 ? 0x1D : 0x1B;
            }
            if ((uint)(type - 0x18) <= 2)
            {
                return type;
            }
            if ((battleType & FieldBattleDTO.BattleTypeDoubles) != 0)
            {
                return type == 7 ? 0x20 : 0x1D;
            }
            if ((battleType & FieldBattleDTO.BattleTypeLink) != 0)
            {
                return 0x1C;
            }
            return type;
        }
        int wildType = Party_GetWildEffectType(dto.Parties[FieldBattleDTO.BattlerEnemy1]!, dto.MapHeaderID);
        if (wildType >= EffectTypeWild && (battleType & FieldBattleDTO.BattleTypeDoubles) != 0)
        {
            wildType = 0x1E;
        }
        return wildType;
    }

    // OFFSET 0x02051BBC
    private static int EncounterEffect_GetEffectIDForType(int type, FieldBattleDTO dto)
    {
        if (type >= EffectTypeCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        effectTable ??= RomData.Arm9U16(EffectTable, EffectTypeCount * 2);
        int effectID = effectTable[type * 2];
        if (effectID == EffectByTerrain)
        {
            effectID = EncounterEffect_GetEffectIDForTerrain(dto);
        }
        return effectID;
    }

    // OFFSET 0x02051BE8
    private static int EncounterEffect_GetBGMForType(int type)
    {
        if (type >= EffectTypeCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        effectTable ??= RomData.Arm9U16(EffectTable, EffectTypeCount * 2);
        return effectTable[type * 2 + 1];
    }

    // OFFSET 0x02051C00
    public static int EncounterEffect_GetEffectID(FieldBattleDTO dto)
    {
        return EncounterEffect_GetEffectIDForType(EncounterEffect_GetType(dto), dto);
    }

    // OFFSET 0x02051C10
    public static int EncounterEffect_GetBattleBGM(FieldBattleDTO dto)
    {
        return EncounterEffect_GetBGMForType(EncounterEffect_GetType(dto));
    }

    // OFFSET 0x02051C20
    private static int TrainerClass_GetEffectType(int trainerClass)
    {
        switch (trainerClass)
        {
            case 62:
                return 0;
            case 63:
                return 13;
            case 64:
                return 6;
            case 65:
                return 8;
            case 66:
                return 9;
            case 67:
                return 10;
            case 68:
                return 11;
            case 69:
                return 12;
            case 72:
            case 87:
            case 88:
                return 0x19;
            case 73:
            case 89:
                return 0x18;
            case 74:
                return 1;
            case 75:
                return 2;
            case 76:
                return 3;
            case 77:
                return 4;
            case 78:
                return 5;
            case 79:
                return 7;
            case 86:
                return 0x1A;
            case 97:
            case 99:
            case 100:
            case 101:
            case 102:
                return EffectTypeFrontier;
        }
        return EffectTypeDefault;
    }

    // OFFSET 0x02051CD0
    private static int Party_GetWildEffectType(Party party, int mapHeaderID)
    {
        Pokemon mon = FieldParty.Party_FindFirstEligibleBattler(party)!;
        switch (Pokemon.Pokemon_GetValue(mon, ParamSpecies, null))
        {
            case 144:
            case 145:
            case 146:
                if (mapHeaderID != MapHeaderDistortionWorldTurnback)
                {
                    return 0x15;
                }
                break;
            case 377:
            case 378:
            case 379:
                if (mapHeaderID != MapHeaderDistortionWorldTurnback)
                {
                    return 0x17;
                }
                break;
            case 479:
            case 485:
            case 486:
            case 491:
                return 0x13;
            case 480:
            case 482:
                return 0x10;
            case 481:
                return 0x11;
            case 483:
            case 484:
                return 0xF;
            case 487:
                return 0x16;
            case 488:
                return 0x14;
            case 492:
                return 0xE;
            case 493:
                return 0x12;
        }
        return EffectTypeWild;
    }

    // OFFSET 0x021DEEC8
    private static int EncounterEffect_GetEffectIDForTerrain(FieldBattleDTO dto)
    {
        int trainer = 1;
        uint battleType = dto.BattleType;
        if ((battleType & FieldBattleDTO.BattleTypeTrainer) == 0)
        {
            if ((battleType & BattleTypeNonTrainerSpecial) != 0)
            {
                ErrorHandling.GF_AssertFail();
                trainer = 0;
            }
            else if (battleType == FieldBattleDTO.BattleTypeWild)
            {
                trainer = 0;
            }
        }
        Pokemon playerMon = FieldParty.Party_FindFirstEligibleBattler(dto.Parties[FieldBattleDTO.BattlerPlayer]!)!;
        Pokemon enemyMon = FieldParty.Party_FindFirstEligibleBattler(dto.Parties[FieldBattleDTO.BattlerEnemy1]!)!;
        int playerLevel = (int)Pokemon.Pokemon_GetValue(playerMon, ParamLevel, null);
        int levelDifference = (int)Pokemon.Pokemon_GetValue(enemyMon, ParamLevel, null) - playerLevel;
        int effect = 0;
        switch (dto.Terrain)
        {
            case TerrainCave:
                effect = 4;
                break;
            case TerrainWater:
                effect = 2;
                break;
        }
        if (levelDifference > 0)
        {
            effect++;
        }
        return effect + trainer * 6;
    }
}
