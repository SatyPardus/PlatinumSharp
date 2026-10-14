using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class RibbonSaveData : ISaveBlock
{
    public const int Size = 0xE;

    public readonly byte[] Ribbons = new byte[Size];

    public void WriteTo(byte[] body, int offset)
    {
        Array.Copy(Ribbons, 0, body, offset, Size);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Array.Copy(body, offset, Ribbons, 0, Size);
    }

    // OFFSET 0x0202D77C
    public static void RibbonSaveData_Init(RibbonSaveData ribbons)
    {
        for (int i = 0; i < Size; i++)
        {
            ribbons.Ribbons[i] = 0;
        }
    }

    // OFFSET 0x0202D79C
    public static RibbonSaveData SaveData_GetRibbons(SaveData saveData)
    {
        return (RibbonSaveData)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryRibbons);
    }
}

public sealed class HoneyTree
{
    public const int Size = 8;

    public int Timer;
    public byte EncounterTableIndex;
    public byte EncounterGroup;
    public byte Rarity;
    public byte ShakeCount;

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, (uint)Timer);
        body[offset + 4] = EncounterTableIndex;
        body[offset + 5] = EncounterGroup;
        body[offset + 6] = Rarity;
        body[offset + 7] = ShakeCount;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Timer = (int)Bin.U32(body, offset);
        EncounterTableIndex = body[offset + 4];
        EncounterGroup = body[offset + 5];
        Rarity = body[offset + 6];
        ShakeCount = body[offset + 7];
    }
}

public sealed class PlayerHoneyTreeStates
{
    public const int TreeCount = 21;
    public const int Size = 4 + TreeCount * HoneyTree.Size;

    public byte LastSlatheredTree;
    public readonly HoneyTree[] Trees = new HoneyTree[TreeCount];

    public PlayerHoneyTreeStates()
    {
        for (int i = 0; i < TreeCount; i++)
        {
            Trees[i] = new HoneyTree();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        body[offset] = LastSlatheredTree;
        for (int i = 0; i < TreeCount; i++)
        {
            Trees[i].WriteTo(body, offset + 4 + i * HoneyTree.Size);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        LastSlatheredTree = body[offset];
        for (int i = 0; i < TreeCount; i++)
        {
            Trees[i].ReadFrom(body, offset + 4 + i * HoneyTree.Size);
        }
    }
}

public sealed class Roamer
{
    public const int Size = 0x14;
    public const int DataMapID = 1;
    public const int DataIVs = 2;
    public const int DataPersonality = 3;
    public const int DataSpecies = 4;
    public const int DataCurrentHP = 5;
    public const int DataLevel = 6;
    public const int DataStatus = 7;
    public const int DataActive = 8;

    public uint MapID;
    public uint IVs;
    public uint Personality;
    public ushort Species;
    public ushort CurrentHP;
    public byte Level;
    public byte Status;
    public byte Active;

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, MapID);
        Bin.W32(body, offset + 4, IVs);
        Bin.W32(body, offset + 8, Personality);
        Bin.W16(body, offset + 0xC, Species);
        Bin.W16(body, offset + 0xE, CurrentHP);
        body[offset + 0x10] = Level;
        body[offset + 0x11] = Status;
        body[offset + 0x12] = Active;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        MapID = Bin.U32(body, offset);
        IVs = Bin.U32(body, offset + 4);
        Personality = Bin.U32(body, offset + 8);
        Species = (ushort)Bin.U16(body, offset + 0xC);
        CurrentHP = (ushort)Bin.U16(body, offset + 0xE);
        Level = body[offset + 0x10];
        Status = body[offset + 0x11];
        Active = body[offset + 0x12];
    }

    // OFFSET 0x0202D93C
    public static uint Roamer_GetData(Roamer roamer, byte dataType)
    {
        switch (dataType)
        {
            case DataMapID:
                return roamer.MapID;
            case DataIVs:
                return roamer.IVs;
            case DataPersonality:
                return roamer.Personality;
            case DataSpecies:
                return roamer.Species;
            case DataCurrentHP:
                return roamer.CurrentHP;
            case DataLevel:
                return roamer.Level;
            case DataStatus:
                return roamer.Status;
            case DataActive:
                return roamer.Active;
        }
        return 0;
    }

    // OFFSET 0x0202D980
    public static void Roamer_SetData(Roamer roamer, byte dataType, uint data)
    {
        switch (dataType)
        {
            case DataMapID:
                roamer.MapID = data;
                break;
            case DataIVs:
                roamer.IVs = data;
                break;
            case DataPersonality:
                roamer.Personality = data;
                break;
            case DataSpecies:
                roamer.Species = (ushort)data;
                break;
            case DataCurrentHP:
                roamer.CurrentHP = (ushort)data;
                break;
            case DataLevel:
                roamer.Level = (byte)data;
                break;
            case DataStatus:
                roamer.Status = (byte)data;
                break;
            case DataActive:
                roamer.Active = (byte)data;
                break;
        }
    }
}

public sealed class SpecialEncounter : ISaveBlock
{
    public const int Size = 0x154;
    public const int RoamerCount = 6;
    public const int DailyMarsh = 1;
    public const int DailySwarm = 2;
    public const int MaxFluteFactor = 2;
    public const int TrophyGardenMonCount = 16;
    public const int RadarChainRecordsSize = 0xC;

    private const int TrophyGardenOffset = 0xC;
    private const int HoneyTreesOffset = 0x10;
    private const int RadarChainRecordsOffset = 0xBC;
    private const int CurrentMapOffset = 0xC8;
    private const int PreviousMapOffset = 0xCC;
    private const int RoamersOffset = 0xD0;
    private const int RoamerRoutesOffset = 0x148;
    private const int SwarmEnabledOffset = 0x14E;
    private const int RepelStepsOffset = 0x14F;
    private const int RadarChargeOffset = 0x150;
    private const int FluteFactorOffset = 0x151;
    private const ushort NoTrophyGardenMon = 0xFFFF;

    public uint MarshDaily;
    public uint SwarmDaily;
    public uint Unused;
    public readonly ushort[] TrophyGardenMons = new ushort[2];
    public readonly PlayerHoneyTreeStates HoneyTrees = new();
    public readonly byte[] RadarChainRecords = new byte[RadarChainRecordsSize];
    public int CurrentMap;
    public int PreviousMap;
    public readonly Roamer[] Roamers = new Roamer[RoamerCount];
    public readonly byte[] RoamerRouteIndices = new byte[RoamerCount];
    public byte SwarmEnabled;
    public byte RepelSteps;
    public byte RadarCharge;
    public byte FluteFactor;

    public SpecialEncounter()
    {
        for (int i = 0; i < RoamerCount; i++)
        {
            Roamers[i] = new Roamer();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, MarshDaily);
        Bin.W32(body, offset + 4, SwarmDaily);
        Bin.W32(body, offset + 8, Unused);
        Bin.W16(body, offset + TrophyGardenOffset, TrophyGardenMons[0]);
        Bin.W16(body, offset + TrophyGardenOffset + 2, TrophyGardenMons[1]);
        HoneyTrees.WriteTo(body, offset + HoneyTreesOffset);
        Array.Copy(RadarChainRecords, 0, body, offset + RadarChainRecordsOffset, RadarChainRecordsSize);
        Bin.W32(body, offset + CurrentMapOffset, (uint)CurrentMap);
        Bin.W32(body, offset + PreviousMapOffset, (uint)PreviousMap);
        for (int i = 0; i < RoamerCount; i++)
        {
            Roamers[i].WriteTo(body, offset + RoamersOffset + i * Roamer.Size);
        }
        Array.Copy(RoamerRouteIndices, 0, body, offset + RoamerRoutesOffset, RoamerCount);
        body[offset + SwarmEnabledOffset] = SwarmEnabled;
        body[offset + RepelStepsOffset] = RepelSteps;
        body[offset + RadarChargeOffset] = RadarCharge;
        body[offset + FluteFactorOffset] = FluteFactor;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        MarshDaily = Bin.U32(body, offset);
        SwarmDaily = Bin.U32(body, offset + 4);
        Unused = Bin.U32(body, offset + 8);
        TrophyGardenMons[0] = (ushort)Bin.U16(body, offset + TrophyGardenOffset);
        TrophyGardenMons[1] = (ushort)Bin.U16(body, offset + TrophyGardenOffset + 2);
        HoneyTrees.ReadFrom(body, offset + HoneyTreesOffset);
        Array.Copy(body, offset + RadarChainRecordsOffset, RadarChainRecords, 0, RadarChainRecordsSize);
        CurrentMap = (int)Bin.U32(body, offset + CurrentMapOffset);
        PreviousMap = (int)Bin.U32(body, offset + PreviousMapOffset);
        for (int i = 0; i < RoamerCount; i++)
        {
            Roamers[i].ReadFrom(body, offset + RoamersOffset + i * Roamer.Size);
        }
        Array.Copy(body, offset + RoamerRoutesOffset, RoamerRouteIndices, 0, RoamerCount);
        SwarmEnabled = body[offset + SwarmEnabledOffset];
        RepelSteps = body[offset + RepelStepsOffset];
        RadarCharge = body[offset + RadarChargeOffset];
        FluteFactor = body[offset + FluteFactorOffset];
    }

    // OFFSET 0x0202D7B0
    public static void SpecialEncounter_Init(SpecialEncounter specialEncounter)
    {
        specialEncounter.ReadFrom(new byte[Size], 0);
        specialEncounter.MarshDaily = MathUtil.MTRNG_Next();
        specialEncounter.SwarmDaily = MathUtil.MTRNG_Next();
        specialEncounter.Unused = 0;
        specialEncounter.TrophyGardenMons[0] = NoTrophyGardenMon;
        specialEncounter.TrophyGardenMons[1] = NoTrophyGardenMon;
        specialEncounter.HoneyTrees.LastSlatheredTree = PlayerHoneyTreeStates.TreeCount;
        for (int i = 0; i < PlayerHoneyTreeStates.TreeCount; i++)
        {
            HoneyTree tree = specialEncounter.HoneyTrees.Trees[i];
            tree.Timer = 0;
            tree.EncounterTableIndex = 0;
            tree.EncounterGroup = 0;
            tree.Rarity = 0;
            tree.ShakeCount = 0;
        }
        specialEncounter.SwarmEnabled = 0;
        specialEncounter.RepelSteps = 0;
        specialEncounter.RadarCharge = 0;
        specialEncounter.FluteFactor = 0;
    }

    // OFFSET 0x0202D80C
    public static void SpecialEncounter_SetMixedRecordDailies(SpecialEncounter specialEncounter, uint mixedRecord)
    {
        specialEncounter.MarshDaily = mixedRecord;
        specialEncounter.SwarmDaily = mixedRecord;
    }

    // OFFSET 0x0202D814
    public static uint SpecialEncounter_GetDailyMon(SpecialEncounter specialEncounter, byte dailyType)
    {
        switch (dailyType)
        {
            case DailyMarsh:
                return specialEncounter.MarshDaily;
            case DailySwarm:
                return specialEncounter.SwarmDaily;
            default:
                ErrorHandling.GF_AssertFail();
                return 0;
        }
    }

    // OFFSET 0x0202D830
    public static byte[] SpecialEncounter_GetRadarChainRecords(SpecialEncounter specialEncounter)
    {
        return specialEncounter.RadarChainRecords;
    }

    // OFFSET 0x0202D834
    public static SpecialEncounter SaveData_GetSpecialEncounters(SaveData saveData)
    {
        return (SpecialEncounter)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryEncounters);
    }

    // OFFSET 0x0202D840
    public static PlayerHoneyTreeStates SpecialEncounter_GetPlayerHoneyTreeStates(SpecialEncounter specialEncounter)
    {
        return specialEncounter.HoneyTrees;
    }

    // OFFSET 0x0202D844
    public static int SpecialEncounter_GetLastSlatheredTreeId(PlayerHoneyTreeStates treeData)
    {
        return treeData.LastSlatheredTree;
    }

    // OFFSET 0x0202D848
    public static void SpecialEncounter_SetLastSlatheredTreeId(byte treeId, PlayerHoneyTreeStates treeData)
    {
        treeData.LastSlatheredTree = treeId;
    }

    // OFFSET 0x0202D84C
    public static HoneyTree SpecialEncounter_GetHoneyTree(byte treeId, PlayerHoneyTreeStates treeData)
    {
        return treeData.Trees[treeId];
    }

    // OFFSET 0x0202D854
    public static void SpecialEncounter_DecrementHoneyTreeTimers(SaveData saveData, int decrement)
    {
        PlayerHoneyTreeStates treeData = SaveData_GetSpecialEncounters(saveData).HoneyTrees;
        for (int i = 0; i < PlayerHoneyTreeStates.TreeCount; i++)
        {
            HoneyTree tree = SpecialEncounter_GetHoneyTree((byte)i, treeData);
            if (tree.Timer != 0)
            {
                tree.Timer -= decrement;
                if (tree.Timer < 0)
                {
                    tree.Timer = 0;
                }
            }
        }
    }

    // OFFSET 0x0202D884
    public static void SpecialEncounter_EnableSwarms(SaveData saveData)
    {
        SaveData_GetSpecialEncounters(saveData).SwarmEnabled = 1;
    }

    // OFFSET 0x0202D898
    public static byte SpecialEncounter_IsSwarmEnabled(SpecialEncounter specialEncounter)
    {
        return specialEncounter.SwarmEnabled;
    }

    // OFFSET 0x0202D8A4
    public static void SpecialEncounter_UpdateRecentRoutes(SpecialEncounter specialEncounter, int newMap)
    {
        if (specialEncounter.CurrentMap != newMap)
        {
            specialEncounter.PreviousMap = specialEncounter.CurrentMap;
            specialEncounter.CurrentMap = newMap;
        }
    }

    // OFFSET 0x0202D8BC
    public static int SpecialEncounter_GetPlayerPreviousMap(SpecialEncounter specialEncounter)
    {
        return specialEncounter.PreviousMap;
    }

    // OFFSET 0x0202D8C4
    public static byte SpecialEncounter_GetRoamerRouteIndex(SpecialEncounter specialEncounter, byte roamerId)
    {
        if (roamerId >= RoamerCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return specialEncounter.RoamerRouteIndices[roamerId];
    }

    // OFFSET 0x0202D8DC
    public static void SpecialEncounter_SetRoamerRouteIndex(SpecialEncounter specialEncounter, byte roamerId, byte routeIndex)
    {
        if (roamerId >= RoamerCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        specialEncounter.RoamerRouteIndices[roamerId] = routeIndex;
    }

    // OFFSET 0x0202D8F8
    public static byte SpecialEncounter_IsRoamerActive(SpecialEncounter specialEncounter, byte slot)
    {
        if (slot >= RoamerCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return specialEncounter.Roamers[slot].Active;
    }

    // OFFSET 0x0202D914
    public static void SpecialEncounter_ZeroRoamerData(Roamer roamer)
    {
        roamer.ReadFrom(new byte[Roamer.Size], 0);
    }

    // OFFSET 0x0202D924
    public static Roamer SpecialEncounter_GetRoamer(SpecialEncounter specialEncounter, byte slot)
    {
        if (slot >= RoamerCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return specialEncounter.Roamers[slot];
    }

    // OFFSET 0x0202D9C4
    public static ref byte SpecialEncounter_GetRadarCharge(SpecialEncounter specialEncounter)
    {
        return ref specialEncounter.RadarCharge;
    }

    // OFFSET 0x0202D9CC
    public static ref byte SpecialEncounter_GetRepelSteps(SpecialEncounter specialEncounter)
    {
        return ref specialEncounter.RepelSteps;
    }

    // OFFSET 0x0202D9D8
    public static bool SpecialEncounter_RepelStepsEmpty(SpecialEncounter specialEncounter)
    {
        if (specialEncounter.RepelSteps == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202D9EC
    public static void SpecialEncounter_SetFluteFactor(SpecialEncounter specialEncounter, byte flute)
    {
        if (flute > MaxFluteFactor)
        {
            ErrorHandling.GF_AssertFail();
        }
        specialEncounter.FluteFactor = flute;
    }

    // OFFSET 0x0202DA04
    public static byte SpecialEncounter_GetFluteFactor(SpecialEncounter specialEncounter)
    {
        return specialEncounter.FluteFactor;
    }

    // OFFSET 0x0202DA10
    public static void SpecialEncounter_GetTrophyGardenMons(SaveData saveData, out ushort slot1, out ushort slot2)
    {
        SpecialEncounter specialEncounter = SaveData_GetSpecialEncounters(saveData);
        slot1 = specialEncounter.TrophyGardenMons[0];
        slot2 = specialEncounter.TrophyGardenMons[1];
    }

    // OFFSET 0x0202DA24
    public static void TrophyGarden_ShiftSlotsForNewMon(SaveData saveData, ushort newMon)
    {
        SpecialEncounter specialEncounter = SaveData_GetSpecialEncounters(saveData);
        if (newMon >= TrophyGardenMonCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        specialEncounter.TrophyGardenMons[1] = specialEncounter.TrophyGardenMons[0];
        specialEncounter.TrophyGardenMons[0] = newMon;
    }
}

public sealed class GlobalTrade : ISaveBlock
{
    public const int Size = 0xF8;

    private const int ListedOffset = 0xEC;
    private const int UnusedOffset = 0xEE;
    private const int DepositDateOffset = 0xF0;
    private const int SearchDateOffset = 0xF4;

    public readonly byte[] StoredPokemon = new byte[Pokemon.Size];
    public ushort Listed;
    public ushort Unused;
    public uint DepositTradeDatestamp;
    public uint SearchTradeDatestamp;

    public void WriteTo(byte[] body, int offset)
    {
        Array.Copy(StoredPokemon, 0, body, offset, Pokemon.Size);
        Bin.W16(body, offset + ListedOffset, Listed);
        Bin.W16(body, offset + UnusedOffset, Unused);
        Bin.W32(body, offset + DepositDateOffset, DepositTradeDatestamp);
        Bin.W32(body, offset + SearchDateOffset, SearchTradeDatestamp);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Array.Copy(body, offset, StoredPokemon, 0, Pokemon.Size);
        Listed = (ushort)Bin.U16(body, offset + ListedOffset);
        Unused = (ushort)Bin.U16(body, offset + UnusedOffset);
        DepositTradeDatestamp = Bin.U32(body, offset + DepositDateOffset);
        SearchTradeDatestamp = Bin.U32(body, offset + SearchDateOffset);
    }

    // OFFSET 0x0202DA40
    public static GlobalTrade SaveData_GetGlobalTrade(SaveData saveData)
    {
        return (GlobalTrade)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryGlobalTrade);
    }

    // OFFSET 0x0202DA50
    public static void GlobalTrade_Init(GlobalTrade globalTrade)
    {
        globalTrade.ReadFrom(new byte[Size], 0);
    }

    // OFFSET 0x0202DA60
    public static int GlobalTrade_IsPokemonListed(GlobalTrade globalTrade)
    {
        return globalTrade.Listed;
    }

    // OFFSET 0x0202DA68
    public static void GlobalTrade_SetPokemonListed(GlobalTrade globalTrade, int listed)
    {
        globalTrade.Listed = (ushort)listed;
    }

    // OFFSET 0x0202DA70
    public static void GlobalTrade_CopyFromStoredPokemon(GlobalTrade globalTrade, Pokemon mon)
    {
        Array.Copy(globalTrade.StoredPokemon, mon.Data, Pokemon.Size);
    }

    // OFFSET 0x0202DA7C
    public static void GlobalTrade_CopyToStoredPokemon(GlobalTrade globalTrade, Pokemon mon)
    {
        Array.Copy(mon.Data, globalTrade.StoredPokemon, Pokemon.Size);
    }

    // OFFSET 0x0202DA8C
    public static uint GlobalTrade_GetDepositTradeDatestamp(GlobalTrade globalTrade)
    {
        return globalTrade.DepositTradeDatestamp;
    }

    // OFFSET 0x0202DA94
    public static void GlobalTrade_SetDepositTradeDatestamp(GlobalTrade globalTrade, uint date)
    {
        globalTrade.DepositTradeDatestamp = date;
    }

    // OFFSET 0x0202DA9C
    public static uint GlobalTrade_GetSearchTradeDatestamp(GlobalTrade globalTrade)
    {
        return globalTrade.SearchTradeDatestamp;
    }

    // OFFSET 0x0202DAA4
    public static void GlobalTrade_SetSearchTradeDatestamp(GlobalTrade globalTrade, uint date)
    {
        globalTrade.SearchTradeDatestamp = date;
    }

    // OFFSET 0x0202DAAC
    public static int GlobalTrade_GetUnusedInt(GlobalTrade globalTrade)
    {
        return globalTrade.Unused;
    }
}
