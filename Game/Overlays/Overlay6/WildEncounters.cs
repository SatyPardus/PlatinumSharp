using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class EncounterSlot
{
    public uint Species;
    public ushort MaxLevel;
    public ushort MinLevel;
}

public sealed class WildEncounterFieldParams
{
    public uint TrainerID;
    public int IsRepelActive;
    public int IsSweetScent;
    public byte RepelLevel;
    public byte FirstMonIsEgg;
    public byte FirstMonAbility;
    public byte ShellosForm;
    public byte GastrodonForm;
    public byte UnownTable;
}

public sealed class RadarEncounterParams
{
    public int ShakeType;
    public int PreserveChain;
    public int IsShiny;
    public int Active;
}

public static class WildEncounters
{
    public const int EncounterTypeGrass = 0;
    public const int EncounterTypeSurf = 1;
    public const int EncounterTypeFishing = 2;

    private const int Overlay = 6;
    private const int HeapIDField = 4;
    private const int HeapIDFieldTask = 0xB;
    private const int GrassSlotCount = 12;
    private const int WaterSlotCount = 5;
    private const int SlotSize = 8;
    private const int NoFishingRod = 0xFF;
    private const int PartySlotEnemy1 = 1;
    private const int PartySlotEnemy2 = 3;
    private const int IVsRandom = 0x20;

    private const int GrassEncounterRateOffset = 0x00;
    private const int GrassSlotsOffset = 0x04;
    private const int SwarmSpecies1Offset = 0x64;
    private const int SwarmSpecies2Offset = 0x68;
    private const int DaySpecies1Offset = 0x6C;
    private const int DaySpecies2Offset = 0x70;
    private const int NightSpecies1Offset = 0x74;
    private const int NightSpecies2Offset = 0x78;
    private const int RadarSpecies1Offset = 0x7C;
    private const int RadarSpecies2Offset = 0x80;
    private const int RadarSpecies3Offset = 0x84;
    private const int RadarSpecies4Offset = 0x88;
    private const int ShellosFormOffset = 0x8C;
    private const int GastrodonFormOffset = 0x90;
    private const int UnownTableOffset = 0xA0;
    private const int SurfEncounterRateOffset = 0xCC;
    private const int SurfSlotsOffset = 0xD0;
    private const int OldRodRateOffset = 0x124;
    private const int GoodRodRateOffset = 0x150;
    private const int SuperRodRateOffset = 0x17C;

    private const uint UnownTableCounts = 0x02248FF0;
    private const uint UnownTableLists = 0x02248FF4;
    private const int UnownTableSize = 8;
    private const int UnownTableMax = 8;

    private const int BattleTypeWild = 0;
    private const uint BattleTypeRoamer = 0x100;
    private const uint BattleTypeWildDoubleWithPartner = 0x4A;
    private const uint BattleStatusHoneyTree = 2;
    private const int TrophyGardenNarc = 0x6A;
    private const int TrophyGardenMember = 8;
    private const ushort TrophyGardenNone = 0xFFFF;
    private const int DailyMonSwarm = 2;
    private const int DailyMonGreatMarsh = 1;
    private const int TimeOfDayDay = 1;
    private const int TimeOfDayNight = 3;
    private const int RoamerDataSpecies = 4;
    private const int RoamerDataLevel = 6;
    private const int RoamerDataIVs = 2;
    private const int RoamerDataPersonality = 3;
    private const int RoamerDataStatus = 7;
    private const int RoamerDataHP = 5;
    private const int RoamerSlots = 6;
    private const int BaseEncounterChance = 40;
    private const int TallGrassBonus = 30;
    private const int PlayerStateCycling = 1;
    private const int GracePeriodChance = 5;
    private const int MaxGracePeriodSteps = 8;
    private const int WeatherSandstorm = 0xA;
    private const int WeatherSnowFirst = 5;
    private const int WeatherSnowLast = 7;
    private const int ItemCleanseTag = 0xE0;
    private const int ItemPureIncense = 0x140;
    private const int FluteFactorBlack = 1;
    private const int FluteFactorWhite = 2;

    private const int ParamPersonality = 0;
    private const int ParamSpecies = 5;
    private const int ParamHeldItem = 6;
    private const int ParamOTID = 7;
    private const int ParamAbility = 0xA;
    private const int ParamIsEgg = 0x4C;
    private const int ParamGender = 0x6F;
    private const int ParamForm = 0x70;
    private const int ParamStatus = 0xA0;
    private const int ParamLevel = 0xA1;
    private const int ParamCurrentHP = 0xA3;
    private const int SpeciesParamType1 = 6;
    private const int SpeciesParamType2 = 7;
    private const int SpeciesParamGenderRatio = 0x12;
    private const int GenderRatioMale = 0;
    private const int GenderRatioFemale = 0xFE;
    private const int GenderRatioNone = 0xFF;
    private const int GenderMale = 0;
    private const int GenderFemale = 1;
    private const int GenderNone = 2;
    private const int NumNatures = 25;

    private const int TypeSteel = 8;
    private const int TypeElectric = 0xD;
    private const int AbilityStench = 1;
    private const int AbilitySandVeil = 8;
    private const int AbilityStatic = 9;
    private const int AbilityCompoundEyes = 0xE;
    private const int AbilityIntimidate = 0x16;
    private const int AbilitySynchronize = 0x1C;
    private const int AbilityIlluminate = 0x23;
    private const int AbilityMagnetPull = 0x2A;
    private const int AbilityPressure = 0x2E;
    private const int AbilityKeenEye = 0x33;
    private const int AbilityHustle = 0x37;
    private const int AbilityCuteCharm = 0x38;
    private const int AbilityStickyHold = 0x3C;
    private const int AbilityArenaTrap = 0x47;
    private const int AbilityVitalSpirit = 0x48;
    private const int AbilityWhiteSmoke = 0x49;
    private const int AbilitySnowCloak = 0x51;
    private const int AbilityQuickFeet = 0x5F;
    private const int AbilityNoGuard = 0x63;
    private const int AbilityNone = 0x7B;
    private const int SpeciesUnown = 0xC9;
    private const int SpeciesShellos = 0x1A6;
    private const int SpeciesGastrodon = 0x1A7;
    private const int HoneyTreeMinLevel = 5;
    private const int HoneyTreeMaxLevel = 15;

    private static ushort Random100()
    {
        ushort value = (ushort)(MathUtil.LCRNG_Next() / 656);
        if (value >= 100)
        {
            ErrorHandling.GF_AssertFail();
        }
        return value;
    }

    private static ushort RandomCoin()
    {
        ushort value = (ushort)((uint)(MathUtil.LCRNG_Next() << 1) >> 16);
        if (value >= 2)
        {
            ErrorHandling.GF_AssertFail();
        }
        return value;
    }

    private static EncounterSlot[] NewSlots(int count)
    {
        EncounterSlot[] slots = new EncounterSlot[count];
        for (int i = 0; i < count; i++)
        {
            slots[i] = new EncounterSlot();
        }
        return slots;
    }

    private static EncounterSlot[] LoadGrassSlots(byte[] encounterData)
    {
        EncounterSlot[] slots = NewSlots(GrassSlotCount);
        for (int i = 0; i < GrassSlotCount; i++)
        {
            int offset = GrassSlotsOffset + i * SlotSize;
            slots[i].Species = Bin.U32(encounterData, offset + 4);
            slots[i].MaxLevel = (ushort)(sbyte)encounterData[offset];
            slots[i].MinLevel = (ushort)(sbyte)encounterData[offset];
        }
        return slots;
    }

    private static EncounterSlot[] LoadWaterSlots(byte[] encounterData, int offset)
    {
        EncounterSlot[] slots = NewSlots(WaterSlotCount);
        for (int i = 0; i < WaterSlotCount; i++)
        {
            int slotOffset = offset + i * SlotSize;
            slots[i].Species = Bin.U32(encounterData, slotOffset + 4);
            slots[i].MaxLevel = (ushort)(sbyte)encounterData[slotOffset];
            slots[i].MinLevel = (ushort)(sbyte)encounterData[slotOffset + 1];
        }
        return slots;
    }

    private static int MapID(FieldSystem fieldSystem)
    {
        return (int)fieldSystem.Location!.U32(0);
    }

    // OFFSET 0x02240C9C
    public static void WildEncounters_ReplaceTimedEncounters(byte[] encounterData, EncounterSlot slot1, EncounterSlot slot2)
    {
        int timeOfDay = Rtc.GetTimeOfDay();
        if ((uint)(timeOfDay - TimeOfDayDay) <= 1)
        {
            slot1.Species = Bin.U32(encounterData, DaySpecies1Offset);
            slot2.Species = Bin.U32(encounterData, DaySpecies2Offset);
        }
        else if ((uint)(timeOfDay - TimeOfDayNight) <= 1)
        {
            slot1.Species = Bin.U32(encounterData, NightSpecies1Offset);
            slot2.Species = Bin.U32(encounterData, NightSpecies2Offset);
        }
    }

    // OFFSET 0x02240CC8
    private static void WildEncounters_ReplaceSwarmEncounters(FieldSystem fieldSystem, byte[] encounterData, EncounterSlot slot1, EncounterSlot slot2)
    {
        SpecialEncounter specialEncounter = SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!);
        if (SpecialEncounter.SpecialEncounter_IsSwarmEnabled(specialEncounter) == 0)
        {
            return;
        }
        if (Swarms.Swarm_GetMapId(SpecialEncounter.SpecialEncounter_GetDailyMon(specialEncounter, DailyMonSwarm)) != MapID(fieldSystem))
        {
            return;
        }
        slot1.Species = Bin.U32(encounterData, SwarmSpecies1Offset);
        slot2.Species = Bin.U32(encounterData, SwarmSpecies2Offset);
    }

    // OFFSET 0x02240D00
    private static void WildEncounters_ReplaceTrophyGardenEncounters(FieldSystem fieldSystem, int nationalDexObtained, EncounterSlot slot1, EncounterSlot slot2)
    {
        if (!MapHeader.MapHeader_IsTrophyGarden(MapID(fieldSystem)))
        {
            return;
        }
        SpecialEncounter.SpecialEncounter_GetTrophyGardenMons(fieldSystem.SaveData!, out ushort mon1, out ushort mon2);
        if (nationalDexObtained == 0)
        {
            return;
        }
        byte[] table = NARC.NARC_AllocAtEndAndReadWholeMemberByIndexPair(TrophyGardenNarc, TrophyGardenMember, HeapIDField);
        if (mon1 != TrophyGardenNone)
        {
            slot1.Species = Bin.U32(table, mon1 * 4);
        }
        if (mon2 != TrophyGardenNone)
        {
            slot2.Species = Bin.U32(table, mon2 * 4);
        }
        Heap.Heap_Free(table);
    }

    // OFFSET 0x02240D5C
    public static int WildEncounters_TryWildEncounter(FieldSystem fieldSystem)
    {
        int playerX = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        int playerZ = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        byte tileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, playerX, playerZ);
        byte encounterRate = WildEncounters_GetEncounterRate(fieldSystem, tileBehavior, out byte encounterType);
        if (encounterRate == 0)
        {
            return 0;
        }
        Party party = Party.SaveData_GetParty(fieldSystem.SaveData!);
        byte[] encounterData = MapHeaderData.MapHeaderData_GetWildEncounters(fieldSystem);
        Pokemon firstMon = Party.Party_GetPokemonBySlotIndex(party, 0);
        WildEncounterFieldParams fieldParams = new();
        InitEncounterFieldParams(fieldSystem, firstMon, encounterData, fieldParams);
        if (!SpecialEncounter.SpecialEncounter_RepelStepsEmpty(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!)))
        {
            Pokemon battler = FieldParty.Party_FindFirstEligibleBattler(party)!;
            fieldParams.IsRepelActive = 1;
            fieldParams.RepelLevel = (byte)Pokemon.Pokemon_GetValue(battler, ParamLevel, null);
        }
        int weather = FieldPlayerState.FieldPlayerState_GetWeather(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        encounterRate = ModifyEncounterRateWithFieldParams(0, encounterRate, fieldParams, weather, firstMon);
        ModifyEncounterRateWithFlute(fieldSystem, ref encounterRate);
        ModifyEncounterRateWithHeldItem(firstMon, ref encounterRate);
        int result = ShouldGetRandomEncounter(fieldSystem, encounterRate, tileBehavior) ? 1 : 0;
        RadarEncounterParams radar = new();
        if (!RadarChain.PokeRadar_ShouldDoRadarEncounter(playerX, playerZ, fieldSystem, fieldSystem.RadarChain!, ref radar.ShakeType, out radar.PreserveChain, out radar.IsShiny))
        {
            radar.Active = 0;
        }
        else
        {
            result = 1;
            radar.Active = 1;
        }
        if (result == 0)
        {
            return 0;
        }
        bool hasPartner = SystemFlags.SystemFlag_CheckHasPartner(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
        if (!hasPartner && radar.Active == 0 && TryEncounterRoamer(fieldSystem, out Roamer? roamer))
        {
            if (RepelPreventsEncounter((byte)Roamer.Roamer_GetData(roamer!, RoamerDataLevel), fieldParams))
            {
                return 0;
            }
            FieldBattleDTO roamerDTO = FieldBattleDTO.FieldBattleDTO_New(HeapIDFieldTask, BattleTypeRoamer);
            FieldBattleDTO.FieldBattleDTO_Init(roamerDTO, fieldSystem);
            AddRoamerToEnemyParty(fieldParams.TrainerID, roamer!, roamerDTO);
            RadarChain.RadarChain_Clear(fieldSystem.RadarChain!);
            Encounters.Encounter_NewVsWild(fieldSystem, roamerDTO);
            return 1;
        }
        bool safariGameActive = false;
        FieldBattleDTO dto;
        if (!hasPartner)
        {
            safariGameActive = SystemFlags.SystemFlag_CheckSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
            CreateWildSingleBattle(fieldSystem, safariGameActive, out dto);
        }
        else
        {
            dto = FieldBattleDTO.FieldBattleDTO_New(HeapIDFieldTask, BattleTypeWildDoubleWithPartner);
        }
        FieldBattleDTO.FieldBattleDTO_Init(dto, fieldSystem);
        int generated;
        if (encounterType == EncounterTypeGrass)
        {
            EncounterSlot[] slots = LoadGrassSlots(encounterData);
            int nationalDexObtained = NationalDexObtained(fieldSystem);
            WildEncounters_ReplaceTimedEncounters(encounterData, slots[2], slots[3]);
            WildEncounters_ReplaceSwarmEncounters(fieldSystem, encounterData, slots[0], slots[1]);
            WildEncounters_ReplaceTrophyGardenEncounters(fieldSystem, nationalDexObtained, slots[6], slots[7]);
            DualSlotEncounters.WildEncounters_ReplaceDualSlotEncounters(encounterData, nationalDexObtained, slots[8], slots[9]);
            if (!hasPartner)
            {
                WildEncounters_ReplaceGreatMarshDailyEncounters(fieldSystem, safariGameActive, nationalDexObtained, slots);
                generated = TryGenerateGrassEncounter_WithRadar(fieldSystem, firstMon, dto, encounterData, slots, fieldParams, radar) ? 1 : 0;
            }
            else
            {
                dto.TrainerIDs[FieldBattleDTO.BattlerPartner] = SystemVars.SystemVars_GetPartnerTrainerID(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
                TrainerData.Trainer_Encounter(dto, fieldSystem.SaveData!, HeapIDFieldTask);
                generated = TryGenerateGrassEncounter_DoubleBattle(fieldSystem, firstMon, dto, slots, fieldParams) ? 1 : 0;
            }
        }
        else if (encounterType == EncounterTypeSurf)
        {
            EncounterSlot[] slots = LoadWaterSlots(encounterData, SurfSlotsOffset);
            generated = TryGenerateSurfEncounter(fieldSystem, firstMon, dto, slots, fieldParams) ? 1 : 0;
        }
        else
        {
            ErrorHandling.GF_AssertFail();
            FieldBattleDTO.FieldBattleDTO_Free(dto);
            return 0;
        }
        if (generated != 0)
        {
            Encounters.Encounter_NewVsWild(fieldSystem, dto);
        }
        else
        {
            result = 0;
        }
        if (result == 0)
        {
            FieldBattleDTO.FieldBattleDTO_Free(dto);
        }
        fieldSystem.WildEncounterSteps = 0;
        return result;
    }

    private static int NationalDexObtained(FieldSystem fieldSystem)
    {
        return Pokedex.Pokedex_IsNationalDexObtained(Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!)) ? 1 : 0;
    }

    // OFFSET 0x0224106C
    public static int WildEncounters_TryFishingEncounter(FieldSystem fieldSystem, int fishingRodType, out FieldBattleDTO? dto)
    {
        dto = null;
        byte encounterRate = (byte)GetFishingEncounterRate(fieldSystem, fishingRodType);
        if (encounterRate == 0)
        {
            return 0;
        }
        Pokemon firstMon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), 0);
        WildEncounterFieldParams fieldParams = new();
        InitEncounterFieldParams(fieldSystem, firstMon, null, fieldParams);
        int weather = FieldPlayerState.FieldPlayerState_GetWeather(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        int modifiedRate = ModifyEncounterRateWithFieldParams(1, encounterRate, fieldParams, weather, firstMon);
        ushort random = Random100();
        if (random >= modifiedRate)
        {
            return 0;
        }
        bool safariGameActive = SystemFlags.SystemFlag_CheckSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
        CreateWildSingleBattle(fieldSystem, safariGameActive, out dto);
        FieldBattleDTO.FieldBattleDTO_Init(dto, fieldSystem);
        FieldBattleDTO.FieldBattleDTO_SetWaterTerrain(dto);
        EncounterSlot[] slots;
        if (MapHeader.MapHeader_HasFeebasTiles(MapID(fieldSystem)) && FeebasFishing.PlayerAvatar_IsFacingFeebasTile(fieldSystem))
        {
            FeebasFishing.LoadFeebasLevelRange(out byte maxLevel, out byte minLevel);
            FeebasFishing.LoadFeebasFromNARC(out int species);
            slots = NewSlots(WaterSlotCount);
            for (byte i = 0; i < WaterSlotCount; i++)
            {
                slots[i].Species = (uint)species;
                slots[i].MaxLevel = maxLevel;
                slots[i].MinLevel = minLevel;
            }
        }
        else
        {
            byte[] encounterData = MapHeaderData.MapHeaderData_GetWildEncounters(fieldSystem);
            int offset = 0;
            switch (fishingRodType)
            {
                case 0:
                    offset = OldRodRateOffset + 4;
                    break;
                case 1:
                    offset = GoodRodRateOffset + 4;
                    break;
                case 2:
                    offset = SuperRodRateOffset + 4;
                    break;
            }
            slots = LoadWaterSlots(encounterData, offset);
        }
        return TryGenerateFishingEncounter(fieldSystem, firstMon, dto, slots, fieldParams, fishingRodType) ? 1 : 0;
    }

    // OFFSET 0x022411C8
    public static int WildEncounters_TrySweetScentEncounter(FieldSystem fieldSystem, FieldTask task)
    {
        int playerX = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        int playerZ = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        byte tileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, playerX, playerZ);
        if (WildEncounters_GetEncounterRate(fieldSystem, tileBehavior, out byte encounterType) == 0)
        {
            return 0;
        }
        Party party = Party.SaveData_GetParty(fieldSystem.SaveData!);
        byte[] encounterData = MapHeaderData.MapHeaderData_GetWildEncounters(fieldSystem);
        Pokemon firstMon = Party.Party_GetPokemonBySlotIndex(party, 0);
        WildEncounterFieldParams fieldParams = new();
        InitEncounterFieldParams(fieldSystem, firstMon, encounterData, fieldParams);
        fieldParams.IsSweetScent = 1;
        RadarEncounterParams radar = new();
        fieldParams.IsRepelActive = 0;
        bool hasPartner = SystemFlags.SystemFlag_CheckHasPartner(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
        if (!hasPartner && TryEncounterRoamer(fieldSystem, out Roamer? roamer))
        {
            FieldBattleDTO roamerDTO = FieldBattleDTO.FieldBattleDTO_New(HeapIDFieldTask, BattleTypeRoamer);
            FieldBattleDTO.FieldBattleDTO_Init(roamerDTO, fieldSystem);
            AddRoamerToEnemyParty(fieldParams.TrainerID, roamer!, roamerDTO);
            RadarChain.RadarChain_Clear(fieldSystem.RadarChain!);
            Encounters.Encounter_StartVsWild(fieldSystem, task, roamerDTO);
            return 1;
        }
        bool safariGameActive = false;
        FieldBattleDTO dto;
        if (!hasPartner)
        {
            safariGameActive = SystemFlags.SystemFlag_CheckSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
            CreateWildSingleBattle(fieldSystem, safariGameActive, out dto);
        }
        else
        {
            dto = FieldBattleDTO.FieldBattleDTO_New(HeapIDFieldTask, BattleTypeWildDoubleWithPartner);
        }
        FieldBattleDTO.FieldBattleDTO_Init(dto, fieldSystem);
        bool generated;
        if (encounterType == EncounterTypeGrass)
        {
            EncounterSlot[] slots = LoadGrassSlots(encounterData);
            int nationalDexObtained = NationalDexObtained(fieldSystem);
            WildEncounters_ReplaceTimedEncounters(encounterData, slots[2], slots[3]);
            WildEncounters_ReplaceSwarmEncounters(fieldSystem, encounterData, slots[0], slots[1]);
            WildEncounters_ReplaceTrophyGardenEncounters(fieldSystem, nationalDexObtained, slots[6], slots[7]);
            DualSlotEncounters.WildEncounters_ReplaceDualSlotEncounters(encounterData, nationalDexObtained, slots[8], slots[9]);
            if (!hasPartner)
            {
                WildEncounters_ReplaceGreatMarshDailyEncounters(fieldSystem, safariGameActive, nationalDexObtained, slots);
                generated = TryGenerateGrassEncounter_WithRadar(fieldSystem, firstMon, dto, encounterData, slots, fieldParams, radar);
            }
            else
            {
                dto.TrainerIDs[FieldBattleDTO.BattlerPartner] = SystemVars.SystemVars_GetPartnerTrainerID(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
                TrainerData.Trainer_Encounter(dto, fieldSystem.SaveData!, HeapIDFieldTask);
                generated = TryGenerateGrassEncounter_DoubleBattle(fieldSystem, firstMon, dto, slots, fieldParams);
            }
        }
        else if (encounterType == EncounterTypeSurf)
        {
            EncounterSlot[] slots = LoadWaterSlots(encounterData, SurfSlotsOffset);
            generated = TryGenerateSurfEncounter(fieldSystem, firstMon, dto, slots, fieldParams);
        }
        else
        {
            ErrorHandling.GF_AssertFail();
            return 0;
        }
        if (generated)
        {
            Encounters.Encounter_StartVsWild(fieldSystem, task, dto);
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        fieldSystem.WildEncounterSteps = 0;
        return 1;
    }

    // OFFSET 0x022413E4
    public static int WildEncounters_TryMudEncounter(FieldSystem fieldSystem, out FieldBattleDTO? dto)
    {
        dto = null;
        int playerX = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        int playerZ = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        byte tileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, playerX, playerZ);
        byte encounterRate = WildEncounters_GetEncounterRate(fieldSystem, tileBehavior, out byte encounterType);
        if (encounterRate == 0)
        {
            return 0;
        }
        Party party = Party.SaveData_GetParty(fieldSystem.SaveData!);
        byte[] encounterData = MapHeaderData.MapHeaderData_GetWildEncounters(fieldSystem);
        Pokemon firstMon = Party.Party_GetPokemonBySlotIndex(party, 0);
        WildEncounterFieldParams fieldParams = new();
        InitEncounterFieldParams(fieldSystem, firstMon, encounterData, fieldParams);
        if (!SpecialEncounter.SpecialEncounter_RepelStepsEmpty(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!)))
        {
            Pokemon battler = FieldParty.Party_FindFirstEligibleBattler(party)!;
            fieldParams.IsRepelActive = 1;
            fieldParams.RepelLevel = (byte)Pokemon.Pokemon_GetValue(battler, ParamLevel, null);
        }
        int weather = FieldPlayerState.FieldPlayerState_GetWeather(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        encounterRate = ModifyEncounterRateWithFieldParams(0, encounterRate, fieldParams, weather, firstMon);
        ModifyEncounterRateWithFlute(fieldSystem, ref encounterRate);
        ModifyEncounterRateWithHeldItem(firstMon, ref encounterRate);
        int result = ShouldGetRandomEncounter(fieldSystem, encounterRate, tileBehavior) ? 1 : 0;
        RadarEncounterParams radar = new();
        bool hasPartner = SystemFlags.SystemFlag_CheckHasPartner(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
        if (!hasPartner && TryEncounterRoamer(fieldSystem, out Roamer? roamer))
        {
            if (RepelPreventsEncounter((byte)Roamer.Roamer_GetData(roamer!, RoamerDataLevel), fieldParams))
            {
                return 0;
            }
            dto = FieldBattleDTO.FieldBattleDTO_New(HeapIDFieldTask, BattleTypeRoamer);
            FieldBattleDTO.FieldBattleDTO_Init(dto, fieldSystem);
            AddRoamerToEnemyParty(fieldParams.TrainerID, roamer!, dto);
            RadarChain.RadarChain_Clear(fieldSystem.RadarChain!);
            return 1;
        }
        bool safariGameActive = false;
        if (!hasPartner)
        {
            safariGameActive = SystemFlags.SystemFlag_CheckSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
            CreateWildSingleBattle(fieldSystem, safariGameActive, out dto);
        }
        else
        {
            dto = FieldBattleDTO.FieldBattleDTO_New(HeapIDFieldTask, BattleTypeWildDoubleWithPartner);
        }
        FieldBattleDTO.FieldBattleDTO_Init(dto, fieldSystem);
        if (encounterType != EncounterTypeGrass)
        {
            ErrorHandling.GF_AssertFail();
            FieldBattleDTO.FieldBattleDTO_Free(dto);
            return 0;
        }
        EncounterSlot[] slots = LoadGrassSlots(encounterData);
        int nationalDexObtained = NationalDexObtained(fieldSystem);
        WildEncounters_ReplaceTimedEncounters(encounterData, slots[2], slots[3]);
        WildEncounters_ReplaceSwarmEncounters(fieldSystem, encounterData, slots[0], slots[1]);
        WildEncounters_ReplaceTrophyGardenEncounters(fieldSystem, nationalDexObtained, slots[6], slots[7]);
        DualSlotEncounters.WildEncounters_ReplaceDualSlotEncounters(encounterData, nationalDexObtained, slots[8], slots[9]);
        bool generated;
        if (!hasPartner)
        {
            WildEncounters_ReplaceGreatMarshDailyEncounters(fieldSystem, safariGameActive, nationalDexObtained, slots);
            generated = TryGenerateGrassEncounter_WithRadar(fieldSystem, firstMon, dto, encounterData, slots, fieldParams, radar);
        }
        else
        {
            dto.TrainerIDs[FieldBattleDTO.BattlerPartner] = SystemVars.SystemVars_GetPartnerTrainerID(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
            TrainerData.Trainer_Encounter(dto, fieldSystem.SaveData!, HeapIDFieldTask);
            generated = TryGenerateGrassEncounter_DoubleBattle(fieldSystem, firstMon, dto, slots, fieldParams);
        }
        if (!generated)
        {
            result = 0;
        }
        if (result == 0)
        {
            FieldBattleDTO.FieldBattleDTO_Free(dto);
        }
        else
        {
            fieldSystem.WildEncounterSteps = 0;
        }
        return result;
    }

    // OFFSET 0x02241674
    private static bool TryGenerateGrassEncounter_WithRadar(FieldSystem fieldSystem, Pokemon firstMon, FieldBattleDTO dto, byte[] encounterData, EncounterSlot[] slots, WildEncounterFieldParams fieldParams, RadarEncounterParams radar)
    {
        bool generated;
        if (radar.Active != 0)
        {
            if (radar.ShakeType == 1)
            {
                slots[4].Species = Bin.U32(encounterData, RadarSpecies1Offset);
                slots[5].Species = Bin.U32(encounterData, RadarSpecies2Offset);
                slots[10].Species = Bin.U32(encounterData, RadarSpecies3Offset);
                slots[11].Species = Bin.U32(encounterData, RadarSpecies4Offset);
            }
            RadarChain.RadarChain_GetMon(fieldSystem.RadarChain!, out int chainSpecies, out int chainLevel);
            if (radar.PreserveChain == 1)
            {
                uint trainerID = TrainerInfo.TrainerInfo_ID(PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!));
                generated = CreateWildMon_FromRadarKeepChain(chainSpecies, chainLevel, PartySlotEnemy1, radar.IsShiny, trainerID, fieldParams, firstMon, dto);
            }
            else
            {
                generated = CreateWildMon_FromRadarNoChain(fieldSystem, firstMon, fieldParams, slots, PartySlotEnemy1, dto, chainSpecies, chainLevel);
            }
            if (generated)
            {
                int playerX = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
                int playerZ = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
                RadarChain.PokeRadar_SpawnPatches(fieldSystem, playerX, playerZ, fieldSystem.RadarChain!);
            }
            return generated;
        }
        generated = TryGenerateWildMon(firstMon, NoFishingRod, fieldParams, slots, EncounterTypeGrass, PartySlotEnemy1, dto);
        if (generated)
        {
            RadarChain.RadarChain_Clear(fieldSystem.RadarChain!);
        }
        return generated;
    }

    // OFFSET 0x0224174C
    private static bool TryGenerateGrassEncounter_DoubleBattle(FieldSystem fieldSystem, Pokemon firstMon, FieldBattleDTO dto, EncounterSlot[] slots, WildEncounterFieldParams fieldParams)
    {
        if (!TryGenerateWildMon(firstMon, NoFishingRod, fieldParams, slots, EncounterTypeGrass, PartySlotEnemy1, dto))
        {
            return false;
        }
        return TryGenerateWildMon(firstMon, NoFishingRod, fieldParams, slots, EncounterTypeGrass, PartySlotEnemy2, dto);
    }

    // OFFSET 0x02241790
    private static bool TryGenerateSurfEncounter(FieldSystem fieldSystem, Pokemon firstMon, FieldBattleDTO dto, EncounterSlot[] slots, WildEncounterFieldParams fieldParams)
    {
        return TryGenerateWildMon(firstMon, NoFishingRod, fieldParams, slots, EncounterTypeSurf, PartySlotEnemy1, dto);
    }

    // OFFSET 0x022417AC
    private static bool TryGenerateFishingEncounter(FieldSystem fieldSystem, Pokemon firstMon, FieldBattleDTO dto, EncounterSlot[] slots, WildEncounterFieldParams fieldParams, int fishingRodType)
    {
        return TryGenerateWildMon(firstMon, fishingRodType, fieldParams, slots, EncounterTypeFishing, PartySlotEnemy1, dto);
    }

    // OFFSET 0x022417C8
    private static bool ShouldGetRandomEncounter(FieldSystem fieldSystem, uint encounterRate, byte tileBehavior)
    {
        if (!GracePeriodStepsUsed(fieldSystem, encounterRate << 8))
        {
            fieldSystem.WildEncounterSteps++;
            if (Random100() >= GracePeriodChance)
            {
                return false;
            }
        }
        byte chance = BaseEncounterChance;
        if (TileBehavior.TileBehavior_IsVeryTallGrass(tileBehavior) != 0)
        {
            chance += TallGrassBonus;
        }
        else if (PlayerAvatar.PlayerAvatar_GetPlayerState(fieldSystem.PlayerAvatar!) == PlayerStateCycling)
        {
            chance += TallGrassBonus;
        }
        chance = (byte)SpecialDates.SpecialDates_ModifyEncounterRate(chance, FieldTime.FieldSystem_HasTimePenalty(fieldSystem));
        if (chance > 100)
        {
            chance = 100;
        }
        ushort random = Random100();
        return random < chance && CheckEncounterRateSuccess(fieldSystem, encounterRate);
    }

    // OFFSET 0x02241874
    private static byte WildEncounters_GetEncounterRate(FieldSystem fieldSystem, byte tileBehavior, out byte encounterType)
    {
        encounterType = 0;
        if (TileBehavior.TileBehavior_HasEncounters(tileBehavior) != 0)
        {
            if (TileBehavior.TileBehavior_IsSurfable(tileBehavior) != 0)
            {
                encounterType = EncounterTypeSurf;
                return (byte)GetSurfEncounterRate(fieldSystem);
            }
            encounterType = EncounterTypeGrass;
            return (byte)GetGrassEncounterRate(fieldSystem);
        }
        return 0;
    }

    // OFFSET 0x022418B4
    private static bool GracePeriodStepsUsed(FieldSystem fieldSystem, uint encounterRate)
    {
        uint steps = encounterRate / 10 >> 8;
        if (steps > MaxGracePeriodSteps)
        {
            steps = MaxGracePeriodSteps;
        }
        return fieldSystem.WildEncounterSteps >= MaxGracePeriodSteps - steps;
    }

    // OFFSET 0x022418DC
    private static bool CheckEncounterRateSuccess(FieldSystem fieldSystem, uint encounterRate)
    {
        ushort random = Random100();
        return random < encounterRate;
    }

    // OFFSET 0x02241904
    private static byte GetGroundEncounterSlot()
    {
        byte random = (byte)Random100();
        if (random < 20)
        {
            return 0;
        }
        if (random < 40)
        {
            return 1;
        }
        if (random < 50)
        {
            return 2;
        }
        if (random < 60)
        {
            return 3;
        }
        if (random < 70)
        {
            return 4;
        }
        if (random < 80)
        {
            return 5;
        }
        if (random < 85)
        {
            return 6;
        }
        if (random < 90)
        {
            return 7;
        }
        if (random < 94)
        {
            return 8;
        }
        if (random < 98)
        {
            return 9;
        }
        if (random == 98)
        {
            return 10;
        }
        return 11;
    }

    // OFFSET 0x022419A0
    private static byte GetWaterEncounterSlot()
    {
        byte random = (byte)Random100();
        if (random < 60)
        {
            return 0;
        }
        if (random < 90)
        {
            return 1;
        }
        if (random < 95)
        {
            return 2;
        }
        if (random < 99)
        {
            return 3;
        }
        return 4;
    }

    // OFFSET 0x022419EC
    private static byte GetRodEncounterSlot(int fishingRodType)
    {
        byte slot = 0;
        byte random = (byte)Random100();
        switch (fishingRodType)
        {
            case 0:
                if (random < 60)
                {
                    slot = 0;
                }
                else if (random < 90)
                {
                    slot = 1;
                }
                else if (random < 95)
                {
                    slot = 2;
                }
                else if (random < 99)
                {
                    slot = 3;
                }
                else
                {
                    slot = 4;
                }
                break;
            case 1:
            case 2:
                if (random < 40)
                {
                    slot = 0;
                }
                else if (random < 80)
                {
                    slot = 1;
                }
                else if (random < 95)
                {
                    slot = 2;
                }
                else if (random < 99)
                {
                    slot = 3;
                }
                else
                {
                    slot = 4;
                }
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return slot;
    }

    // OFFSET 0x02241A90
    private static void ModifyEncounterRateWithHeldItem(Pokemon mon, ref byte encounterRate)
    {
        ushort item = (ushort)Pokemon.Pokemon_GetValue(mon, ParamHeldItem, null);
        if (item == ItemCleanseTag || item == ItemPureIncense)
        {
            encounterRate = (byte)(encounterRate * 2 / 3);
        }
    }

    // OFFSET 0x02241ABC
    private static void ModifyEncounterRateWithFlute(FieldSystem fieldSystem, ref byte encounterRate)
    {
        byte factor = SpecialEncounter.SpecialEncounter_GetFluteFactor(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!));
        if (factor == FluteFactorBlack)
        {
            encounterRate = (byte)(encounterRate >> 1);
        }
        else if (factor == FluteFactorWhite)
        {
            encounterRate = (byte)(encounterRate + (encounterRate >> 1));
        }
    }

    // OFFSET 0x02241AE4
    private static byte GetNatureForWildMon(Pokemon firstMon, WildEncounterFieldParams fieldParams)
    {
        if (fieldParams.FirstMonIsEgg == 0 && fieldParams.FirstMonAbility == AbilitySynchronize && RandomCoin() == 0)
        {
            return (byte)(Pokemon.Pokemon_GetValue(firstMon, ParamPersonality, null) % NumNatures);
        }
        ushort nature = (ushort)(MathUtil.LCRNG_Next() / 0xA3E);
        if (nature >= NumNatures)
        {
            ErrorHandling.GF_AssertFail();
        }
        return (byte)nature;
    }

    // OFFSET 0x02241B40
    private static byte GetWildMonLevel(EncounterSlot slot, WildEncounterFieldParams fieldParams)
    {
        byte minLevel;
        byte maxLevel;
        if (slot.MaxLevel >= slot.MinLevel)
        {
            minLevel = (byte)slot.MinLevel;
            maxLevel = (byte)slot.MaxLevel;
        }
        else
        {
            minLevel = (byte)slot.MaxLevel;
            maxLevel = (byte)slot.MinLevel;
        }
        byte random = (byte)(MathUtil.LCRNG_Next() % (byte)(maxLevel - minLevel + 1));
        if (fieldParams.FirstMonIsEgg == 0)
        {
            byte ability = fieldParams.FirstMonAbility;
            if (ability == AbilityHustle || ability == AbilityVitalSpirit || ability == AbilityPressure)
            {
                if (RandomCoin() != 0)
                {
                    return maxLevel;
                }
                return (byte)(minLevel + random);
            }
        }
        return (byte)(minLevel + random);
    }

    // OFFSET 0x02241BAC
    private static void CreateWildMonShinyWithGenderOrNature(ushort species, byte level, int partySlot, uint trainerID, WildEncounterFieldParams fieldParams, Pokemon firstMon, FieldBattleDTO dto)
    {
        Pokemon mon = Pokemon.Pokemon_New(HeapIDFieldTask);
        Pokemon.Pokemon_Init(mon);
        bool checkGenderOrNature = false;
        byte firstMonGender = 0;
        byte firstMonNature = 0;
        if (fieldParams.FirstMonIsEgg == 0)
        {
            if (fieldParams.FirstMonAbility == AbilityCuteCharm)
            {
                uint genderRatio = SpeciesData.SpeciesData_GetSpeciesValue(species, SpeciesParamGenderRatio);
                if (genderRatio != GenderRatioMale && genderRatio != GenderRatioFemale && genderRatio != GenderRatioNone)
                {
                    ushort random = (ushort)(MathUtil.LCRNG_Next() / 0x5556);
                    if (random >= 3)
                    {
                        ErrorHandling.GF_AssertFail();
                    }
                    if (random != 0)
                    {
                        firstMonGender = (byte)Pokemon.Pokemon_GetValue(firstMon, ParamGender, null);
                        checkGenderOrNature = true;
                    }
                }
            }
            else if (fieldParams.FirstMonAbility == AbilitySynchronize && RandomCoin() == 0)
            {
                firstMonNature = Pokemon.Pokemon_GetNature(firstMon);
                checkGenderOrNature = true;
            }
        }
        uint personality = Pokemon.Pokemon_FindShinyPersonality(trainerID);
        if (checkGenderOrNature)
        {
            while (true)
            {
                if (fieldParams.FirstMonAbility == AbilityCuteCharm)
                {
                    uint gender = SpeciesData.Species_GetGender(species, personality);
                    if (gender == GenderNone)
                    {
                        ErrorHandling.GF_AssertFail();
                    }
                    if (gender != firstMonGender)
                    {
                        break;
                    }
                    personality = Pokemon.Pokemon_FindShinyPersonality(trainerID);
                }
                else if (fieldParams.FirstMonAbility == AbilitySynchronize)
                {
                    if (Pokemon.Personality_GetNature(personality) == firstMonNature)
                    {
                        break;
                    }
                    personality = Pokemon.Pokemon_FindShinyPersonality(trainerID);
                }
            }
        }
        Pokemon.Pokemon_InitWith(mon, species, level, IVsRandom, 1, personality, 1, fieldParams.TrainerID);
        if (!AddWildMonToParty(partySlot, fieldParams, mon, dto))
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02241CC0
    private static void CreateWildMon(ushort species, byte level, int partySlot, WildEncounterFieldParams fieldParams, Pokemon firstMon, FieldBattleDTO dto)
    {
        Pokemon mon = Pokemon.Pokemon_New(HeapIDFieldTask);
        Pokemon.Pokemon_Init(mon);
        bool hasGender = true;
        uint genderRatio = SpeciesData.SpeciesData_GetSpeciesValue(species, SpeciesParamGenderRatio);
        if (genderRatio == GenderRatioMale || genderRatio == GenderRatioFemale || genderRatio == GenderRatioNone)
        {
            hasGender = false;
        }
        if (hasGender && fieldParams.FirstMonIsEgg == 0 && fieldParams.FirstMonAbility == AbilityCuteCharm)
        {
            ushort random = (ushort)(MathUtil.LCRNG_Next() / 0x5556);
            if (random >= 3)
            {
                ErrorHandling.GF_AssertFail();
            }
            if (random != 0)
            {
                byte gender = (byte)Pokemon.Pokemon_GetValue(firstMon, ParamGender, null);
                if (gender == GenderFemale)
                {
                    gender = GenderMale;
                }
                else if (gender == GenderMale)
                {
                    gender = GenderFemale;
                }
                else
                {
                    ErrorHandling.GF_AssertFail();
                }
                byte cuteCharmNature = GetNatureForWildMon(firstMon, fieldParams);
                Pokemon.Pokemon_InitWithGenderNatureLetter(mon, species, level, IVsRandom, gender, cuteCharmNature, 0);
                Pokemon.Pokemon_SetValue(mon, ParamOTID, fieldParams.TrainerID);
                if (!AddWildMonToParty(partySlot, fieldParams, mon, dto))
                {
                    ErrorHandling.GF_AssertFail();
                }
                return;
            }
        }
        byte nature = GetNatureForWildMon(firstMon, fieldParams);
        Pokemon.Pokemon_InitWithNature(mon, species, level, IVsRandom, nature);
        Pokemon.Pokemon_SetValue(mon, ParamOTID, fieldParams.TrainerID);
        if (!AddWildMonToParty(partySlot, fieldParams, mon, dto))
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    private static byte ChooseTypeMatchedSlot(Pokemon firstMon, WildEncounterFieldParams fieldParams, EncounterSlot[] slots, byte count)
    {
        if (!TryGetSlotForTypeMatchAbility(firstMon, fieldParams, slots, count, TypeSteel, AbilityMagnetPull, out byte slot)
            && !TryGetSlotForTypeMatchAbility(firstMon, fieldParams, slots, count, TypeElectric, AbilityStatic, out slot))
        {
            slot = 0xFF;
        }
        return slot;
    }

    // OFFSET 0x02241DC4
    private static bool TryGenerateWildMon(Pokemon firstMon, int fishingRodType, WildEncounterFieldParams fieldParams, EncounterSlot[] slots, int encounterType, int partySlot, FieldBattleDTO dto)
    {
        byte slot = 0;
        byte level = 0;
        switch (encounterType)
        {
            case EncounterTypeGrass:
                slot = ChooseTypeMatchedSlot(firstMon, fieldParams, slots, GrassSlotCount);
                if (slot == 0xFF)
                {
                    slot = GetGroundEncounterSlot();
                }
                slot = TryFindHigherLevelSlot(slots, fieldParams, slot);
                level = (byte)slots[slot].MaxLevel;
                break;
            case EncounterTypeSurf:
                TryGetSlotForTypeMatchAbility(firstMon, fieldParams, slots, WaterSlotCount, TypeSteel, AbilityMagnetPull, out slot);
                if (!TryGetSlotForTypeMatchAbility(firstMon, fieldParams, slots, WaterSlotCount, TypeElectric, AbilityStatic, out slot))
                {
                    slot = GetWaterEncounterSlot();
                }
                level = GetWildMonLevel(slots[slot], fieldParams);
                break;
            case EncounterTypeFishing:
                TryGetSlotForTypeMatchAbility(firstMon, fieldParams, slots, WaterSlotCount, TypeSteel, AbilityMagnetPull, out slot);
                if (!TryGetSlotForTypeMatchAbility(firstMon, fieldParams, slots, WaterSlotCount, TypeElectric, AbilityStatic, out slot))
                {
                    slot = GetRodEncounterSlot(fishingRodType);
                }
                level = GetWildMonLevel(slots[slot], fieldParams);
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        if (FirstMonAbilityPreventsEncounter(fieldParams, firstMon, level))
        {
            return false;
        }
        if (RepelPreventsEncounter(level, fieldParams))
        {
            return false;
        }
        CreateWildMon((ushort)slots[slot].Species, level, partySlot, fieldParams, firstMon, dto);
        return true;
    }

    // OFFSET 0x02241F2C
    private static bool CreateWildMon_FromRadarKeepChain(int species, int level, int partySlot, int isShiny, uint trainerID, WildEncounterFieldParams fieldParams, Pokemon firstMon, FieldBattleDTO dto)
    {
        if (species == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (isShiny != 0)
        {
            CreateWildMonShinyWithGenderOrNature((ushort)species, (byte)level, partySlot, trainerID, fieldParams, firstMon, dto);
        }
        else
        {
            CreateWildMon((ushort)species, (byte)level, partySlot, fieldParams, firstMon, dto);
        }
        return true;
    }

    // OFFSET 0x02241F7C
    private static bool CreateWildMon_FromRadarNoChain(FieldSystem fieldSystem, Pokemon firstMon, WildEncounterFieldParams fieldParams, EncounterSlot[] slots, int partySlot, FieldBattleDTO dto, int chainSpecies, int chainLevel)
    {
        byte slot = ChooseTypeMatchedSlot(firstMon, fieldParams, slots, GrassSlotCount);
        if (slot == 0xFF)
        {
            slot = GetGroundEncounterSlot();
        }
        int species = (int)slots[slot].Species;
        byte level = (byte)slots[slot].MaxLevel;
        if (chainSpecies == 0)
        {
            RadarChain.RadarChain_SetMon(fieldSystem.RadarChain!, species, level);
            RadarChain.RadarChain_Increment(fieldSystem);
        }
        else if (species == chainSpecies)
        {
            level = (byte)chainLevel;
            RadarChain.RadarChain_Increment(fieldSystem);
        }
        else
        {
            RadarChain.RadarChain_Clear(fieldSystem.RadarChain!);
        }
        CreateWildMon((ushort)species, level, partySlot, fieldParams, firstMon, dto);
        return true;
    }

    // OFFSET 0x02242034
    public static void CreateWildMon_HoneyTree(FieldSystem fieldSystem, FieldBattleDTO dto)
    {
        int species = HoneyTrees.HoneyTree_GetSpecies(fieldSystem);
        Pokemon firstMon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), 0);
        WildEncounterFieldParams fieldParams = new();
        InitEncounterFieldParams(fieldSystem, firstMon, null, fieldParams);
        ushort random = (ushort)(MathUtil.LCRNG_Next() / 0x1746);
        if (random >= 11)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte level = (byte)(random + HoneyTreeMinLevel);
        if (fieldParams.FirstMonIsEgg == 0)
        {
            byte ability = fieldParams.FirstMonAbility;
            if ((ability == AbilityHustle || ability == AbilityVitalSpirit || ability == AbilityPressure) && RandomCoin() != 0)
            {
                level = HoneyTreeMaxLevel;
            }
        }
        HoneyTrees.HoneyTree_Unslather(fieldSystem);
        dto.BattleStatusMask |= BattleStatusHoneyTree;
        CreateWildMon((ushort)species, level, PartySlotEnemy1, fieldParams, firstMon, dto);
    }

    // OFFSET 0x022420D4
    public static void CreateWildMon_Scripted(FieldSystem fieldSystem, ushort species, byte level, FieldBattleDTO dto)
    {
        Pokemon firstMon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), 0);
        WildEncounterFieldParams fieldParams = new();
        InitEncounterFieldParams(fieldSystem, firstMon, null, fieldParams);
        CreateWildMon(species, level, PartySlotEnemy1, fieldParams, firstMon, dto);
    }

    // OFFSET 0x02242110
    public static bool WildEncounters_TileHasEncounterRate(FieldSystem fieldSystem, byte tileBehavior)
    {
        int rate = 0;
        if (TileBehavior.TileBehavior_HasEncounters(tileBehavior) != 0)
        {
            if (TileBehavior.TileBehavior_IsSurfable(tileBehavior) != 0)
            {
                rate = GetSurfEncounterRate(fieldSystem);
            }
            else
            {
                rate = GetGrassEncounterRate(fieldSystem);
            }
        }
        return rate != 0;
    }

    // OFFSET 0x0224214C
    private static int GetGrassEncounterRate(FieldSystem fieldSystem)
    {
        return (int)Bin.U32(MapHeaderData.MapHeaderData_GetWildEncounters(fieldSystem), GrassEncounterRateOffset);
    }

    // OFFSET 0x02242158
    private static int GetSurfEncounterRate(FieldSystem fieldSystem)
    {
        return (int)Bin.U32(MapHeaderData.MapHeaderData_GetWildEncounters(fieldSystem), SurfEncounterRateOffset);
    }

    // OFFSET 0x02242164
    private static int GetFishingEncounterRate(FieldSystem fieldSystem, int fishingRodType)
    {
        byte[] encounterData = MapHeaderData.MapHeaderData_GetWildEncounters(fieldSystem);
        switch (fishingRodType)
        {
            case 0:
                return (int)Bin.U32(encounterData, OldRodRateOffset);
            case 1:
                return (int)Bin.U32(encounterData, GoodRodRateOffset);
            case 2:
                return (int)Bin.U32(encounterData, SuperRodRateOffset);
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x0224219C
    private static bool ForceMatchingTypeEncounterSlot(EncounterSlot[] slots, byte count, byte type, out byte slot)
    {
        slot = 0;
        byte[] matching = new byte[GrassSlotCount];
        byte matches = 0;
        for (byte i = 0; i < count; i++)
        {
            byte type1 = (byte)SpeciesData.SpeciesData_GetSpeciesValue((int)slots[i].Species, SpeciesParamType1);
            byte type2 = (byte)SpeciesData.SpeciesData_GetSpeciesValue((int)slots[i].Species, SpeciesParamType2);
            if (type1 == type || type2 == type)
            {
                matching[matches++] = i;
            }
        }
        if (matches == 0 || matches == count)
        {
            return false;
        }
        slot = matching[MathUtil.LCRNG_Next() % matches];
        return true;
    }

    // OFFSET 0x0224222C
    private static bool TryGetSlotForTypeMatchAbility(Pokemon unused, WildEncounterFieldParams fieldParams, EncounterSlot[] slots, byte count, byte type, byte ability, out byte slot)
    {
        slot = 0;
        if (fieldParams.FirstMonIsEgg != 0 || fieldParams.FirstMonAbility != ability)
        {
            return false;
        }
        if (RandomCoin() != 0)
        {
            return false;
        }
        return ForceMatchingTypeEncounterSlot(slots, count, type, out slot);
    }

    // OFFSET 0x0224226C
    private static byte ModifyEncounterRateWithFieldParams(int isFishing, int encounterRate, WildEncounterFieldParams fieldParams, int weather, Pokemon firstMon)
    {
        if (fieldParams.FirstMonIsEgg != 0)
        {
            return (byte)encounterRate;
        }
        byte ability = fieldParams.FirstMonAbility;
        if (isFishing == 0)
        {
            if (ability == AbilityArenaTrap || ability == AbilityNoGuard || ability == AbilityIlluminate)
            {
                encounterRate *= 2;
            }
            else if (ability == AbilitySandVeil)
            {
                if (weather == WeatherSandstorm)
                {
                    encounterRate /= 2;
                }
            }
            else if (ability == AbilitySnowCloak)
            {
                if ((uint)(weather - WeatherSnowFirst) <= WeatherSnowLast - WeatherSnowFirst)
                {
                    encounterRate /= 2;
                }
            }
            else if (ability == AbilityWhiteSmoke || ability == AbilityQuickFeet || ability == AbilityStench)
            {
                encounterRate /= 2;
            }
        }
        if (encounterRate > 100)
        {
            encounterRate = 100;
        }
        return (byte)encounterRate;
    }

    // OFFSET 0x022422D0
    private static bool FirstMonAbilityPreventsEncounter(WildEncounterFieldParams fieldParams, Pokemon firstMon, byte wildLevel)
    {
        if (fieldParams.IsSweetScent != 0)
        {
            return false;
        }
        if (fieldParams.FirstMonIsEgg != 0)
        {
            return false;
        }
        if (fieldParams.FirstMonAbility != AbilityKeenEye && fieldParams.FirstMonAbility != AbilityIntimidate)
        {
            return false;
        }
        byte firstMonLevel = (byte)Pokemon.Pokemon_GetValue(firstMon, ParamLevel, null);
        if (firstMonLevel <= 5)
        {
            return false;
        }
        if (wildLevel > firstMonLevel - 5)
        {
            return false;
        }
        return RandomCoin() == 0;
    }

    // OFFSET 0x02242328
    private static void CreateWildSingleBattle(FieldSystem fieldSystem, bool safariGameActive, out FieldBattleDTO dto)
    {
        if (!safariGameActive)
        {
            dto = FieldBattleDTO.FieldBattleDTO_New(HeapIDFieldTask, BattleTypeWild);
            return;
        }
        BufferView ballCount = FieldPlayerState.FieldPlayerState_GetSafariBallCount(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        dto = FieldBattleDTO.FieldBattleDTO_NewSafari(HeapIDFieldTask, ballCount.U16(0));
    }

    // OFFSET 0x02242354
    private static void WildEncounters_ReplaceGreatMarshDailyEncounters(FieldSystem fieldSystem, bool safariGameActive, int nationalDexObtained, EncounterSlot[] slots)
    {
        if (!safariGameActive)
        {
            return;
        }
        uint dailyMon = SpecialEncounter.SpecialEncounter_GetDailyMon(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!), DailyMonGreatMarsh);
        GreatMarshDailyEncounters.ReplaceGreatMarshDailyEncounters((int)dailyMon, nationalDexObtained, MapID(fieldSystem), slots[6], slots[7]);
    }

    // OFFSET 0x02242388
    private static bool RepelPreventsEncounter(byte wildLevel, WildEncounterFieldParams fieldParams)
    {
        return fieldParams.IsRepelActive != 0 && fieldParams.RepelLevel > wildLevel;
    }

    // OFFSET 0x0224239C
    private static void AddRoamerToEnemyParty(uint trainerID, Roamer roamer, FieldBattleDTO dto)
    {
        Pokemon mon = Pokemon.Pokemon_New(HeapIDField);
        uint species = Roamer.Roamer_GetData(roamer, RoamerDataSpecies);
        byte level = (byte)Roamer.Roamer_GetData(roamer, RoamerDataLevel);
        uint ivs = Roamer.Roamer_GetData(roamer, RoamerDataIVs);
        uint personality = Roamer.Roamer_GetData(roamer, RoamerDataPersonality);
        uint status = Roamer.Roamer_GetData(roamer, RoamerDataStatus);
        ushort hp = (ushort)Roamer.Roamer_GetData(roamer, RoamerDataHP);
        Pokemon.Pokemon_InitWithCombinedIVs(mon, (ushort)species, level, ivs, personality);
        Pokemon.Pokemon_SetValue(mon, ParamOTID, trainerID);
        Pokemon.Pokemon_SetValue(mon, ParamStatus, status);
        Pokemon.Pokemon_SetValue(mon, ParamCurrentHP, hp);
        if (!Party.Party_AddPokemon(dto.Parties[PartySlotEnemy1]!, mon))
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02242440
    private static bool TryEncounterRoamer(FieldSystem fieldSystem, out Roamer? encounteredRoamer)
    {
        encounteredRoamer = null;
        SpecialEncounter specialEncounter = SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!);
        Roamer[] roamers = new Roamer[RoamerSlots];
        byte count = 0;
        for (byte i = 0; i < RoamerSlots; i++)
        {
            int route = RoamingPokemon.RoamingPokemon_GetRouteFromId(SpecialEncounter.SpecialEncounter_GetRoamerRouteIndex(specialEncounter, i));
            if (SpecialEncounter.SpecialEncounter_IsRoamerActive(specialEncounter, i) != 0 && route == MapID(fieldSystem))
            {
                roamers[count++] = SpecialEncounter.SpecialEncounter_GetRoamer(specialEncounter, i);
            }
        }
        if (count == 0)
        {
            return false;
        }
        if (RandomCoin() == 0)
        {
            return false;
        }
        if (count > 1)
        {
            ushort index = (ushort)(MathUtil.LCRNG_Next() / (ushort)(0xFFFF / count + 1));
            if (index >= count)
            {
                ErrorHandling.GF_AssertFail();
            }
            encounteredRoamer = roamers[index];
        }
        else
        {
            encounteredRoamer = roamers[0];
        }
        return true;
    }

    // OFFSET 0x02242514
    private static bool AddWildMonToParty(int partySlot, WildEncounterFieldParams fieldParams, Pokemon mon, FieldBattleDTO dto)
    {
        int compoundEyes = fieldParams.FirstMonIsEgg == 0 && fieldParams.FirstMonAbility == AbilityCompoundEyes ? 1 : 0;
        Pokemon.Pokemon_GiveHeldItem(mon, dto.BattleType, compoundEyes);
        bool setForm = false;
        byte form = 0;
        uint species = Pokemon.Pokemon_GetValue(mon, ParamSpecies, null);
        if (species == SpeciesShellos)
        {
            setForm = true;
            form = (byte)(fieldParams.ShellosForm != 0 ? 1 : 0);
        }
        else if (species == SpeciesGastrodon)
        {
            setForm = true;
            form = (byte)(fieldParams.GastrodonForm != 0 ? 1 : 0);
        }
        else if (species == SpeciesUnown)
        {
            setForm = true;
            uint tableEntry = (uint)(fieldParams.UnownTable * UnownTableSize);
            byte count = (byte)RomData.OverlayU32(Overlay, UnownTableCounts + tableEntry, 1)[0];
            int index = MathUtil.LCRNG_Next() % count;
            uint list = RomData.OverlayU32(Overlay, UnownTableLists + tableEntry, 1)[0];
            form = RomData.Overlay(Overlay, (uint)(list + index), 1)[0];
        }
        if (setForm)
        {
            Pokemon.Pokemon_SetValue(mon, ParamForm, form);
        }
        return Party.Party_AddPokemon(dto.Parties[partySlot]!, mon);
    }

    // OFFSET 0x022425D4
    private static byte TryFindHigherLevelSlot(EncounterSlot[] slots, WildEncounterFieldParams fieldParams, byte slot)
    {
        if (fieldParams.FirstMonIsEgg != 0)
        {
            return slot;
        }
        byte ability = fieldParams.FirstMonAbility;
        if (ability != AbilityVitalSpirit && ability != AbilityHustle && ability != AbilityPressure)
        {
            return slot;
        }
        if (RandomCoin() == 0)
        {
            return slot;
        }
        for (byte i = 0; i < GrassSlotCount; i++)
        {
            if (slots[i].Species == slots[slot].Species && slots[i].MaxLevel > slots[slot].MaxLevel)
            {
                slot = i;
            }
        }
        return slot;
    }

    // OFFSET 0x02242634
    private static void InitEncounterFieldParams(FieldSystem fieldSystem, Pokemon firstMon, byte[]? encounterData, WildEncounterFieldParams fieldParams)
    {
        if (Pokemon.Pokemon_GetValue(firstMon, ParamIsEgg, null) == 0)
        {
            fieldParams.FirstMonIsEgg = 0;
            fieldParams.FirstMonAbility = (byte)Pokemon.Pokemon_GetValue(firstMon, ParamAbility, null);
        }
        else
        {
            fieldParams.FirstMonIsEgg = 1;
            fieldParams.FirstMonAbility = AbilityNone;
        }
        fieldParams.RepelLevel = 0;
        fieldParams.IsRepelActive = 0;
        fieldParams.IsSweetScent = 0;
        if (encounterData != null)
        {
            fieldParams.ShellosForm = (byte)Bin.U32(encounterData, ShellosFormOffset);
            fieldParams.GastrodonForm = (byte)Bin.U32(encounterData, GastrodonFormOffset);
            uint unownTable = Bin.U32(encounterData, UnownTableOffset);
            if (unownTable != 0)
            {
                if ((int)unownTable > UnownTableMax)
                {
                    ErrorHandling.GF_AssertFail();
                }
                fieldParams.UnownTable = (byte)(unownTable - 1);
            }
            else
            {
                fieldParams.UnownTable = 0;
            }
        }
        fieldParams.TrainerID = TrainerInfo.TrainerInfo_ID(PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!));
    }
}
