using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public static class SpecialDates
{
    private const int Overlay = 6;
    private const uint MonthDateLists = 0x0224954C;
    private const uint EncounterModifiers = 0x02249538;
    private const int MonthEntrySize = 8;
    private const int MonthsPerYear = 12;

    // OFFSET 0x02246B74
    public static int SpecialDates_ModifyEncounterRate(int encounterModifier, int hasTimePenalty)
    {
        if (hasTimePenalty != 0)
        {
            return encounterModifier;
        }
        if (encounterModifier == 0)
        {
            return 0;
        }
        Rtc.GetCurrentDate(out RtcDate date);
        if (date.Month == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (date.Month > MonthsPerYear)
        {
            ErrorHandling.GF_AssertFail();
        }
        uint entry = (uint)(MonthDateLists + (date.Month - 1) * MonthEntrySize);
        uint[] month = RomData.OverlayU32(Overlay, entry, 2);
        int count = (int)month[0];
        for (byte i = 0; i < count; i++)
        {
            ushort specialDate = RomData.OverlayU16(Overlay, (uint)(month[1] + i * 2), 1)[0];
            if (date.Day != (byte)(specialDate >> 8))
            {
                continue;
            }
            int modifier = (int)RomData.OverlayU32(Overlay, (uint)(EncounterModifiers + (byte)specialDate * 4), 1)[0];
            int modified = encounterModifier + modifier;
            if (modified >= 0)
            {
                return modified;
            }
            return 1;
        }
        return encounterModifier;
    }
}

public static class Swarms
{
    private const int Overlay = 6;
    private const uint SwarmMapIDs = 0x02249090;
    private const uint SwarmCount = 0x16;
    private const int WildEncountersSize = 0x1A8;
    private const int SwarmSpeciesOffset = 0x64;

    // OFFSET 0x02243218
    public static uint Swarm_GetMapId(uint swarm)
    {
        return RomData.OverlayU32(Overlay, SwarmMapIDs + swarm % SwarmCount * 4, 1)[0];
    }

    // OFFSET 0x0224322C
    public static void Swarm_GetMapIdAndSpecies(uint swarm, BufferView mapIdDest, BufferView speciesDest)
    {
        uint mapID = RomData.OverlayU32(Overlay, SwarmMapIDs + swarm % SwarmCount * 4, 1)[0];
        byte[] encounters = new byte[WildEncountersSize];
        MapHeaderData.MapHeaderData_LoadWildEncounters(encounters, (int)mapID);
        speciesDest.W16(0, (int)Bin.U32(encounters, SwarmSpeciesOffset));
        mapIdDest.W16(0, (int)mapID);
    }
}

public static class GreatMarshDailyEncounters
{
    private const int GreatMarshNarc = 0x6A;
    private const int NationalDexMember = 9;
    private const int RegionalDexMember = 10;
    private const int HeapIDField = 4;
    private const int FirstGreatMarshMap = 0x1F8;
    private const int GreatMarshAreas = 6;
    private const int BitsPerArea = 5;

    // OFFSET 0x02242F74
    public static void ReplaceGreatMarshDailyEncounters(int dailyMon, int nationalDexObtained, int mapHeaderID, EncounterSlot slot1, EncounterSlot slot2)
    {
        byte area = GreatMarsh_GetAreaNumFromMapId(mapHeaderID);
        int member = nationalDexObtained != 0 ? NationalDexMember : RegionalDexMember;
        byte[] table = NARC.NARC_AllocAtEndAndReadWholeMemberByIndexPair(GreatMarshNarc, member, HeapIDField);
        byte index = (byte)((dailyMon >> (area * BitsPerArea)) & 0x1F);
        slot1.Species = Bin.U32(table, index * 4);
        slot2.Species = Bin.U32(table, index * 4);
        Heap.Heap_Free(table);
    }

    // OFFSET 0x02242FC0
    private static byte GreatMarsh_GetAreaNumFromMapId(int mapHeaderID)
    {
        int area = mapHeaderID - FirstGreatMarshMap;
        if ((uint)area >= GreatMarshAreas)
        {
            ErrorHandling.GF_AssertFail();
            return 0;
        }
        return (byte)area;
    }
}

public static class FeebasFishing
{
    private const int FeebasNarc = 0x6A;
    private const int FeebasSpeciesMember = 0;
    private const int FeebasTilesMember = 1;
    private const int HeapIDField = 4;
    private const int FeebasTileCount = 4;
    private const byte FeebasMaxLevel = 20;
    private const byte FeebasMinLevel = 10;
    private const int MapBlockTiles = 32;

    // OFFSET 0x02247660
    public static bool PlayerAvatar_IsFacingFeebasTile(FieldSystem fieldSystem)
    {
        ushort coin = (ushort)((uint)(MathUtil.LCRNG_Next() << 1) >> 16);
        if (coin >= 2)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (coin == 0)
        {
            return false;
        }
        uint seed = RecordMixedRNG.RecordMixedRNG_GetRand(RecordMixedRNG.SaveData_GetRecordMixedRNG(fieldSystem.SaveData!));
        PlayerMove.PlayerAvatar_GetFacingTileCoords(fieldSystem.PlayerAvatar!, out int facingX, out int facingZ);
        int matrixWidth = MapMatrix.MapMatrix_GetWidth(fieldSystem.MapMatrix);
        bool isFeebasTile = false;
        uint[] seedBytes = { seed >> 24, (byte)(seed >> 16), (byte)(seed >> 8), (byte)seed };
        byte[] data = NARC.NARC_AllocAtEndAndReadWholeMemberByIndexPair(FeebasNarc, FeebasTilesMember, HeapIDField);
        int groupCount = (int)Bin.U32(data, 0);
        int totalTiles = 0;
        for (byte i = 0; i < groupCount; i++)
        {
            totalTiles += (int)Bin.U32(data, 4 + i * 4);
        }
        int tilesOffset = (groupCount * 2 + 2) * 2;
        int tilesPerGroup = totalTiles / 4;
        if (tilesPerGroup >= 0x100)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte extraTiles = (byte)(totalTiles % 4);
        byte extraOffset = 0;
        int[] feebasTiles = new int[FeebasTileCount];
        for (byte i = 0; i < FeebasTileCount; i++)
        {
            int index = (int)(seedBytes[i] % (uint)tilesPerGroup) + tilesPerGroup * i + extraOffset;
            feebasTiles[i] = (ushort)Bin.U16(data, tilesOffset + index * 2);
            if (extraTiles != 0)
            {
                extraOffset++;
                extraTiles--;
            }
        }
        int facingTile = facingX + facingZ * (matrixWidth * MapBlockTiles);
        for (byte i = 0; i < FeebasTileCount; i++)
        {
            if (facingTile == feebasTiles[i])
            {
                isFeebasTile = true;
                break;
            }
        }
        Heap.Heap_Free(data);
        return isFeebasTile;
    }

    // OFFSET 0x02247794
    public static void LoadFeebasLevelRange(out byte maxLevel, out byte minLevel)
    {
        maxLevel = FeebasMaxLevel;
        minLevel = FeebasMinLevel;
    }

    // OFFSET 0x022477A0
    public static void LoadFeebasFromNARC(out int species)
    {
        byte[] data = NARC.NARC_AllocAtEndAndReadWholeMemberByIndexPair(FeebasNarc, FeebasSpeciesMember, HeapIDField);
        species = (int)Bin.U32(data, 0);
        Heap.Heap_Free(data);
    }
}

public static class DualSlotEncounters
{
    private const int VersionRuby = 1;
    private const int VersionSapphire = 2;
    private const int VersionEmerald = 3;
    private const int VersionFireRed = 4;
    private const int VersionLeafGreen = 5;

    // OFFSET 0x022477B8
    public static void WildEncounters_ReplaceDualSlotEncounters(byte[] encounterData, int nationalDexObtained, EncounterSlot slot1, EncounterSlot slot2)
    {
        if (nationalDexObtained == 0)
        {
            return;
        }
        int offset;
        switch (gSystem.GbaCartridgeVersion)
        {
            case VersionRuby:
                offset = 0xAC;
                break;
            case VersionSapphire:
                offset = 0xA4;
                break;
            case VersionEmerald:
                offset = 0xB4;
                break;
            case VersionFireRed:
                offset = 0xBC;
                break;
            case VersionLeafGreen:
                offset = 0xC4;
                break;
            default:
                return;
        }
        slot1.Species = Bin.U32(encounterData, offset);
        slot2.Species = Bin.U32(encounterData, offset + 4);
    }
}

public static class RoamerAfterBattle
{
    private const int ParamSpecies = 5;
    private const int ParamStatus = 0xA0;
    private const int ParamCurrentHP = 0xA3;
    private const int RoamerDataSpecies = 4;
    private const byte RoamerDataHP = 5;
    private const byte RoamerDataStatus = 7;
    private const int RoamerSlots = 6;
    private const int BattleResultWin = 1;
    private const int BattleResultCaught = 4;
    private const ushort RoamingStateCaught = 1;
    private const ushort RoamingStateDefeated = 2;
    private const int MoveOffMapChance = 30;

    // OFFSET 0x02246034
    public static void RoamerAfterBattle_UpdateRoamers(FieldSystem fieldSystem, FieldBattleDTO dto)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(dto.Parties[1]!, 0);
        SpecialEncounter specialEncounter = SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!);
        uint species = Pokemon.Pokemon_GetValue(mon, ParamSpecies, null);
        Roamer? roamer = GetBattledRoamer(specialEncounter, species);
        int mapID = (int)fieldSystem.Location!.U32(0);
        if (roamer != null)
        {
            ushort hp = (ushort)Pokemon.Pokemon_GetValue(mon, ParamCurrentHP, null);
            byte status = (byte)Pokemon.Pokemon_GetValue(mon, ParamStatus, null);
            if (dto.ResultMask == BattleResultWin && hp == 0)
            {
                SpecialEncounter.SpecialEncounter_ZeroRoamerData(roamer);
                SystemVars.SystemVars_SetRoamingSpeciesState(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), (ushort)species, RoamingStateDefeated);
            }
            else if (dto.ResultMask == BattleResultCaught)
            {
                SpecialEncounter.SpecialEncounter_ZeroRoamerData(roamer);
                SystemVars.SystemVars_SetRoamingSpeciesState(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), (ushort)species, RoamingStateCaught);
            }
            else
            {
                Roamer.Roamer_SetData(roamer, RoamerDataHP, hp);
                Roamer.Roamer_SetData(roamer, RoamerDataStatus, status);
            }
            MoveRoamersOffMap(specialEncounter, mapID);
            return;
        }
        ushort random = (ushort)(MathUtil.LCRNG_Next() / 656);
        if (random >= 100)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (random < MoveOffMapChance)
        {
            MoveRoamersOffMap(specialEncounter, mapID);
        }
    }

    // OFFSET 0x02246110
    private static void MoveRoamersOffMap(SpecialEncounter specialEncounter, int currentMap)
    {
        for (byte i = 0; i < RoamerSlots; i++)
        {
            if (SpecialEncounter.SpecialEncounter_IsRoamerActive(specialEncounter, i) == 0)
            {
                continue;
            }
            if (currentMap == RoamingPokemon.RoamingPokemon_GetRouteFromId(SpecialEncounter.SpecialEncounter_GetRoamerRouteIndex(specialEncounter, i)))
            {
                RoamingPokemon.RoamingPokemon_MoveToRandomMap(specialEncounter, i);
            }
        }
    }

    // OFFSET 0x02246148
    private static Roamer? GetBattledRoamer(SpecialEncounter specialEncounter, uint species)
    {
        for (byte i = 0; i < RoamerSlots; i++)
        {
            if (SpecialEncounter.SpecialEncounter_IsRoamerActive(specialEncounter, i) == 0)
            {
                continue;
            }
            Roamer roamer = SpecialEncounter.SpecialEncounter_GetRoamer(specialEncounter, i);
            if (species == Roamer.Roamer_GetData(roamer, RoamerDataSpecies))
            {
                return roamer;
            }
        }
        return null;
    }
}
