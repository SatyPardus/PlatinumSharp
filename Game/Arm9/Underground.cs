using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed class UndergroundRecord
{
    public const int Size = 0x3C;
    public const int CounterCount = 15;
    public const int MaxCount = 999999;
    public const int FlagRankNormal = 0;
    public const int FlagRankBronze = 1;
    public const int FlagRankSilver = 2;
    public const int FlagRankGold = 3;
    public const int FlagRankPlatinum = 4;

    private const uint CountMask = 0x000FFFFF;
    private const int TrainerScore = 0;
    private const int PeopleMet = 1;
    private const int GiftsGiven = 2;
    private const int FlagsStolen = 3;
    private const int SpheresDug = 4;
    private const int FossilsDug = 5;
    private const int NonFossilsDug = 6;
    private const int TrapHits = 7;
    private const int TrapsTriggered = 8;
    private const int PlayersHelped = 9;
    private const int GiftsReceived = 10;
    private const int TimesFlagTaken = 11;
    private const int FlagsRecovered = 12;
    private const int TimesBaseMoved = 13;
    private const int CapturedFlags = 14;
    private const int PlatinumFlagCount = 50;
    private const int GoldFlagCount = 10;
    private const int SilverFlagCount = 3;
    private const int BronzeFlagCount = 1;

    public readonly uint[] Counters = new uint[CounterCount];

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < CounterCount; i++)
        {
            Bin.W32(body, offset + i * 4, Counters[i]);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < CounterCount; i++)
        {
            Counters[i] = Bin.U32(body, offset + i * 4);
        }
    }

    private int Get(int counter)
    {
        return (int)(Counters[counter] & CountMask);
    }

    private void Set(int counter, int value)
    {
        Counters[counter] = (Counters[counter] & ~CountMask) | ((uint)value & CountMask);
    }

    private void Increment(int counter)
    {
        int value = Get(counter);
        if ((uint)value < MaxCount)
        {
            Set(counter, value + 1);
        }
    }

    private void Add(int counter, int amount)
    {
        if (amount > 0 && (uint)Get(counter) > (uint)(MaxCount - amount))
        {
            Set(counter, MaxCount);
            return;
        }
        Set(counter, Get(counter) + amount);
    }

    public int CapturedFlagCount => Get(CapturedFlags);

    // OFFSET 0x02028558
    public static int UndergroundRecord_Size()
    {
        return Size;
    }

    // OFFSET 0x0202855C
    public static UndergroundRecord UndergroundRecord_New(int heapID)
    {
        return new UndergroundRecord();
    }

    // OFFSET 0x020294B8
    public static int UndergroundRecord_GetTrainerScore(UndergroundRecord record)
    {
        return record.Get(TrainerScore);
    }

    // OFFSET 0x020294C0
    public static bool UndergroundRecord_HasPlatBaseFlag(UndergroundRecord record)
    {
        if (UndergroundRecord_GetFlagRank(record) == FlagRankPlatinum)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020294D4
    public static void UndergroundRecord_SetTrainerScore(UndergroundRecord record, int trainerScore)
    {
        record.Set(TrainerScore, trainerScore);
    }

    // OFFSET 0x020294EC
    public static int UndergroundRecord_GetPeopleMet(UndergroundRecord record)
    {
        return record.Get(PeopleMet);
    }

    // OFFSET 0x020294F4
    public static void UndergroundRecord_IncrementPeopleMet(UndergroundRecord record, int unused)
    {
        record.Increment(PeopleMet);
    }

    // OFFSET 0x02029524
    public static int UndergroundRecord_GetNumGiftsGiven(UndergroundRecord record)
    {
        return record.Get(GiftsGiven);
    }

    // OFFSET 0x0202952C
    public static void UndergroundRecord_IncrementGiftsGiven(UndergroundRecord record)
    {
        record.Increment(GiftsGiven);
    }

    // OFFSET 0x0202955C
    public static void UndergroundRecord_IncrementFlagsStolen(UndergroundRecord record)
    {
        record.Increment(FlagsStolen);
    }

    // OFFSET 0x0202958C
    public static int UndergroundRecord_GetFlagRank(UndergroundRecord record)
    {
        int count = record.Get(CapturedFlags);
        if (count >= PlatinumFlagCount)
        {
            return FlagRankPlatinum;
        }
        if (count >= GoldFlagCount)
        {
            return FlagRankGold;
        }
        if (count >= SilverFlagCount)
        {
            return FlagRankSilver;
        }
        if (count >= BronzeFlagCount)
        {
            return FlagRankBronze;
        }
        return FlagRankNormal;
    }

    // OFFSET 0x020295B8
    public static int UndergroundRecord_GetCapturedFlagCount(UndergroundRecord record)
    {
        return record.Get(CapturedFlags);
    }

    // OFFSET 0x020295C0
    public static void UndergroundRecord_IncrementCapturedFlagCount(UndergroundRecord record)
    {
        record.Increment(CapturedFlags);
    }

    // OFFSET 0x020295F0
    public static int UndergroundRecord_GetNumSpheresDug(UndergroundRecord record)
    {
        return record.Get(SpheresDug);
    }

    // OFFSET 0x020295F8
    public static void UndergroundRecord_AddNumSpheresDug(UndergroundRecord record, int amount)
    {
        record.Add(SpheresDug, amount);
    }

    // OFFSET 0x0202963C
    public static int UndergroundRecord_GetNumFossilsDug(UndergroundRecord record)
    {
        return record.Get(FossilsDug);
    }

    // OFFSET 0x02029644
    public static void UndergroundRecord_AddNumFossilsDug(UndergroundRecord record, int amount)
    {
        record.Add(FossilsDug, amount);
    }

    // OFFSET 0x02029688
    public static void UndergroundRecord_AddNumNonFossilsDug(UndergroundRecord record, int amount)
    {
        record.Add(NonFossilsDug, amount);
    }

    // OFFSET 0x020296CC
    public static int UndergroundRecord_GetNumTrapHits(UndergroundRecord record)
    {
        return record.Get(TrapHits);
    }

    // OFFSET 0x020296D4
    public static void UndergroundRecord_IncrementNumTrapHits(UndergroundRecord record)
    {
        record.Increment(TrapHits);
    }

    // OFFSET 0x02029704
    public static int UndergroundRecord_GetNumTrapsTriggered(UndergroundRecord record)
    {
        return record.Get(TrapsTriggered);
    }

    // OFFSET 0x0202970C
    public static void UndergroundRecord_IncrementNumTrapsTriggered(UndergroundRecord record)
    {
        record.Increment(TrapsTriggered);
    }

    // OFFSET 0x0202973C
    public static int UndergroundRecord_GetNumPlayersHelped(UndergroundRecord record)
    {
        return record.Get(PlayersHelped);
    }

    // OFFSET 0x02029744
    public static void UndergroundRecord_IncrementNumPlayersHelped(UndergroundRecord record)
    {
        record.Increment(PlayersHelped);
    }

    // OFFSET 0x02029774
    public static int UndergroundRecord_GetGiftsReceived(UndergroundRecord record)
    {
        return record.Get(GiftsReceived);
    }

    // OFFSET 0x0202977C
    public static void UndergroundRecord_IncrementGiftsReceived(UndergroundRecord record)
    {
        record.Increment(GiftsReceived);
    }

    // OFFSET 0x020297AC
    public static int UndergroundRecord_GetTimesFlagTaken(UndergroundRecord record)
    {
        return record.Get(TimesFlagTaken);
    }

    // OFFSET 0x020297B4
    public static void UndergroundRecord_IncrementTimesFlagTaken(UndergroundRecord record)
    {
        record.Increment(TimesFlagTaken);
    }

    // OFFSET 0x020297E4
    public static int UndergroundRecord_GetFlagsRecovered(UndergroundRecord record)
    {
        return record.Get(FlagsRecovered);
    }

    // OFFSET 0x020297EC
    public static void UndergroundRecord_IncrementFlagsRecovered(UndergroundRecord record)
    {
        record.Increment(FlagsRecovered);
    }

    // OFFSET 0x0202981C
    public static int UndergroundRecord_GetTimesBaseMoved(UndergroundRecord record)
    {
        return record.Get(TimesBaseMoved);
    }

    // OFFSET 0x02029824
    public static void UndergroundRecord_IncrementTimesBaseMoved(UndergroundRecord record)
    {
        record.Increment(TimesBaseMoved);
    }
}

public sealed class SecretBase
{
    public const int Size = 0x94;
    public const int MaxPlacedGoods = 15;
    public const int MaxBoulders = 16;
    public const int MaxGoodIndex = 32;
    public const int MaxCoord = 32;
    public const int FirstBoulderIndex = 16;
    public const int GoodNone = 0;
    public const int GoodPlainPC = 1;
    public const int GoodBronzePC = 2;
    public const int GoodSilverPC = 3;
    public const int GoodGoldPC = 4;
    public const int GoodPlatinumPC = 5;
    public const int GoodBoulder = 6;

    private const int PlacedGoodSize = 3;
    private const int PlacedGoodsBytes = 0x2D;
    private const int BouldersOffset = 0x2D;
    private const int BouldersBytes = 0x20;
    private const int RecordOffset = 0x50;
    private const int EntranceXOffset = 0x8C;
    private const int EntranceZOffset = 0x8E;
    private const int EntranceDirOffset = 0x90;
    private const int ActiveOffset = 0x91;
    private const int PCXCoord = 15;
    private const int PCZCoord = 12;
    private const int PlatinumFlagCount = 50;
    private const int GoldFlagCount = 10;
    private const int SilverFlagCount = 3;
    private const int BronzeFlagCount = 1;

    public readonly byte[] PlacedGoods = new byte[PlacedGoodsBytes];
    public readonly byte[] Boulders = new byte[BouldersBytes];
    public readonly UndergroundRecord Record = new();
    public ushort EntranceX;
    public ushort EntranceZ;
    public byte EntranceDir;
    public byte Active;

    public void WriteTo(byte[] body, int offset)
    {
        Array.Copy(PlacedGoods, 0, body, offset, PlacedGoodsBytes);
        Array.Copy(Boulders, 0, body, offset + BouldersOffset, BouldersBytes);
        Record.WriteTo(body, offset + RecordOffset);
        Bin.W16(body, offset + EntranceXOffset, EntranceX);
        Bin.W16(body, offset + EntranceZOffset, EntranceZ);
        body[offset + EntranceDirOffset] = EntranceDir;
        body[offset + ActiveOffset] = Active;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Array.Copy(body, offset, PlacedGoods, 0, PlacedGoodsBytes);
        Array.Copy(body, offset + BouldersOffset, Boulders, 0, BouldersBytes);
        Record.ReadFrom(body, offset + RecordOffset);
        EntranceX = (ushort)Bin.U16(body, offset + EntranceXOffset);
        EntranceZ = (ushort)Bin.U16(body, offset + EntranceZOffset);
        EntranceDir = body[offset + EntranceDirOffset];
        Active = body[offset + ActiveOffset];
    }

    // OFFSET 0x02028554
    public static int SecretBase_Size()
    {
        return Size;
    }

    // OFFSET 0x020292C0
    public static void SecretBase_Init(SecretBase secretBase)
    {
        secretBase.ReadFrom(new byte[Size], 0);
    }

    // OFFSET 0x020292CC
    public static void SecretBase_Clear(SecretBase secretBase)
    {
        Array.Clear(secretBase.PlacedGoods);
        Array.Clear(secretBase.Boulders);
        secretBase.EntranceX = 0;
        secretBase.EntranceZ = 0;
        secretBase.EntranceDir = 0;
        secretBase.Active = 0;
    }

    // OFFSET 0x02029300
    public static void SecretBase_SetInactive(SecretBase secretBase)
    {
        secretBase.Active = 0;
    }

    // OFFSET 0x02029308
    public static void SecretBase_AddGoodAtIndex(SecretBase? secretBase, int index, int goodID, int x, int z)
    {
        if (index >= MaxGoodIndex)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (x >= MaxCoord)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (z >= MaxCoord)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (secretBase == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (index == 0)
        {
            return;
        }
        if (index < FirstBoulderIndex)
        {
            int entry = (index - 1) * PlacedGoodSize;
            secretBase!.PlacedGoods[entry] = (byte)x;
            secretBase.PlacedGoods[entry + 1] = (byte)z;
            secretBase.PlacedGoods[entry + 2] = (byte)goodID;
            return;
        }
        index -= FirstBoulderIndex;
        secretBase!.Boulders[index * 2] = (byte)x;
        secretBase.Boulders[index * 2 + 1] = (byte)z;
    }

    // OFFSET 0x02029364
    public static void SecretBase_SetGoodCoordsAtIndex(SecretBase secretBase, int index, int x, int z)
    {
        int goodID = SecretBase_GetGoodIDAtIndex(secretBase, index);
        SecretBase_AddGoodAtIndex(secretBase, index, goodID, x, z);
    }

    // OFFSET 0x02029384
    private static int SecretBase_GetPCGoodID(SecretBase secretBase)
    {
        int count = secretBase.Record.CapturedFlagCount;
        if (count >= PlatinumFlagCount)
        {
            return GoodPlatinumPC;
        }
        if (count >= GoldFlagCount)
        {
            return GoodGoldPC;
        }
        if (count >= SilverFlagCount)
        {
            return GoodSilverPC;
        }
        if (count >= BronzeFlagCount)
        {
            return GoodBronzePC;
        }
        return GoodPlainPC;
    }

    // OFFSET 0x020293B0
    public static int SecretBase_GetGoodIDAtIndex(SecretBase secretBase, int index)
    {
        if (index >= MaxGoodIndex)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (index == 0)
        {
            return SecretBase_GetPCGoodID(secretBase);
        }
        if (index < FirstBoulderIndex)
        {
            return secretBase.PlacedGoods[(index - 1) * PlacedGoodSize + 2];
        }
        index -= FirstBoulderIndex;
        if (secretBase.Boulders[index * 2] != 0 && secretBase.Boulders[index * 2 + 1] != 0)
        {
            return GoodBoulder;
        }
        return GoodNone;
    }

    // OFFSET 0x020293FC
    public static int SecretBase_GetGoodXCoordAtIndex(SecretBase secretBase, int index)
    {
        if (index >= MaxGoodIndex)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (index == 0)
        {
            return PCXCoord;
        }
        if (index < FirstBoulderIndex)
        {
            return secretBase.PlacedGoods[(index - 1) * PlacedGoodSize];
        }
        return secretBase.Boulders[(index - FirstBoulderIndex) * 2];
    }

    // OFFSET 0x0202942C
    public static int SecretBase_GetGoodZCoordAtIndex(SecretBase secretBase, int index)
    {
        if (index >= MaxGoodIndex)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (index == 0)
        {
            return PCZCoord;
        }
        if (index < FirstBoulderIndex)
        {
            return secretBase.PlacedGoods[(index - 1) * PlacedGoodSize + 1];
        }
        return secretBase.Boulders[(index - FirstBoulderIndex) * 2 + 1];
    }

    // OFFSET 0x02029460
    public static int SecretBase_GetUnremovableBoulderCount(SecretBase secretBase)
    {
        uint count = (uint)secretBase.Record.CapturedFlagCount;
        if (count >= PlatinumFlagCount)
        {
            return 0;
        }
        if (count >= GoldFlagCount)
        {
            return 6;
        }
        if (count >= SilverFlagCount)
        {
            return 11;
        }
        if (count >= BronzeFlagCount)
        {
            return 15;
        }
        return 16;
    }

    // OFFSET 0x0202948C
    public static int SecretBase_GetPlacedGoodsLimit(SecretBase secretBase)
    {
        uint count = (uint)secretBase.Record.CapturedFlagCount;
        if (count >= PlatinumFlagCount)
        {
            return 15;
        }
        if (count >= GoldFlagCount)
        {
            return 15;
        }
        if (count >= SilverFlagCount)
        {
            return 15;
        }
        if (count >= BronzeFlagCount)
        {
            return 12;
        }
        return 10;
    }

    // OFFSET 0x02029854
    public static void SecretBase_SetEntrance(SecretBase secretBase, int x, int z, int dir)
    {
        secretBase.EntranceX = (ushort)x;
        secretBase.EntranceZ = (ushort)z;
        secretBase.EntranceDir = (byte)dir;
        secretBase.Active = 1;
    }

    // OFFSET 0x02029874
    public static int SecretBase_GetEntranceXCoord(SecretBase secretBase)
    {
        return secretBase.EntranceX;
    }

    // OFFSET 0x0202987C
    public static int SecretBase_GetEntranceZCoord(SecretBase secretBase)
    {
        return secretBase.EntranceZ;
    }

    // OFFSET 0x02029884
    public static int SecretBase_GetEntranceDir(SecretBase secretBase)
    {
        return secretBase.EntranceDir;
    }

    // OFFSET 0x0202988C
    public static int SecretBase_IsActive(SecretBase secretBase)
    {
        return secretBase.Active;
    }

    // OFFSET 0x02029894
    public static SecretBase SaveData_GetSecretBase(SaveData saveData)
    {
        return ((Underground)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryUnderground)).Base;
    }

    // OFFSET 0x020298A0
    public static UndergroundRecord SaveData_GetUndergroundRecord(SaveData saveData)
    {
        return ((Underground)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryUnderground)).Base.Record;
    }

    // OFFSET 0x020298AC
    public static UndergroundRecord SecretBase_GetUndergroundRecord(SecretBase secretBase)
    {
        return secretBase.Record;
    }
}

public sealed class Underground : ISaveBlock
{
    public const int Size = 0x9B4;
    public const int FlagOwnerCount = 5;
    public const int FlagOwnerNameLength = 8;
    public const int MaxSpawnedTraps = 64;
    public const int MaxMiningSpots = 255;
    public const int MaxPlacedTraps = 16;
    public const int MaxBuriedSpheres = 100;
    public const int MaxGoodsPC = 200;
    public const int InventorySize = 40;
    public const int MaxPlacedGoods = 15;
    public const int GiftPenaltyNone = 0;
    public const int GiftPenaltyPrimed = 1;
    public const int GiftPenaltyActive = 2;
    public const int NoSlot = -1;

    private const int SeedOffset = 0x94;
    private const int GiftPenaltyMinutesOffset = 0x98;
    private const int GiftPenaltyStateOffset = 0x9C;
    private const int CollectedOrbFlagOffset = 0x9D;
    private const int FlagOwnerIDsOffset = 0xA0;
    private const int FlagOwnerNamesOffset = 0xB4;
    private const int FlagOwnerLanguagesOffset = 0x104;
    private const int FlagOwnerGameCodesOffset = 0x109;
    private const int FlagOwnerIndexOffset = 0x10E;
    private const int SpawnedTrapIDsOffset = 0x10F;
    private const int SpawnedTrapCoordsOffset = 0x14F;
    private const int MiningSpotCoordsOffset = 0x20F;
    private const int PlacedTrapIDsOffset = 0x50C;
    private const int PlacedTrapCoordsOffset = 0x51C;
    private const int PlacedTrapSpawnedIndicesOffset = 0x54C;
    private const int BuriedSphereTypesOffset = 0x55C;
    private const int BuriedSphereGrowthOffset = 0x5C0;
    private const int BuriedSphereInitialSizesOffset = 0x624;
    private const int BuriedSphereCoordsOffset = 0x688;
    private const int UnusedOffset = 0x7B4;
    private const int UnusedBytes = 0x58;
    private const int PlatesMinedOffset = 0x80C;
    private const int GoodsPCOffset = 0x810;
    private const int TrapsOffset = 0x8D8;
    private const int GoodsBagOffset = 0x900;
    private const int TreasuresOffset = 0x928;
    private const int SphereTypesOffset = 0x950;
    private const int SphereSizesOffset = 0x978;
    private const int PlacedGoodSlotsOffset = 0x9A0;
    private const int StepCountOffset = 0x9AF;
    private const int FlagsOffset = 0x9B0;
    private const int CoordSize = 3;
    private const byte SpawnFlagMask = 0x0F;
    private const byte MinedFlagMask = 0xF0;
    private const byte MinedFlag = 0x10;
    private const int MaxDaysProcessed = 99;
    private const int MaxSphereGrowth = 99;
    private const int GiftPenaltyDuration = 1440;
    private const int MaxStepCount = 99;
    private const int FirstPlate = 0x2C;
    private const int LastPlate = 0x3B;
    private const int FirstTreasure = 11;
    private const int TreasureLimit = 60;
    private const uint TreasureItemTable = 0x02100920;
    private const uint SphereGrowthTable = 0x020E5B44;
    private const int SphereTypeCount = 6;
    private const uint RandomMultiplier = 0x5D588B65;
    private const uint RandomIncrement = 0x00269EC3;

    public readonly SecretBase Base = new();
    public uint RandomSeed;
    public int GiftPenaltyMinutes;
    public byte GiftPenaltyState;
    public byte UnusedCollectedOrbFlag;
    public readonly uint[] FlagOwnerIDs = new uint[FlagOwnerCount];
    public readonly ushort[][] FlagOwnerNames = new ushort[FlagOwnerCount][];
    public readonly byte[] FlagOwnerLanguages = new byte[FlagOwnerCount];
    public readonly byte[] FlagOwnerGameCodes = new byte[FlagOwnerCount];
    public byte FlagOwnerIndex;
    public readonly byte[] SpawnedTrapIDs = new byte[MaxSpawnedTraps];
    public readonly byte[] SpawnedTrapCoords = new byte[MaxSpawnedTraps * CoordSize];
    public readonly byte[] MiningSpotCoords = new byte[MaxMiningSpots * CoordSize];
    public readonly byte[] PlacedTrapIDs = new byte[MaxPlacedTraps];
    public readonly byte[] PlacedTrapCoords = new byte[MaxPlacedTraps * CoordSize];
    public readonly byte[] PlacedTrapSpawnedIndices = new byte[MaxPlacedTraps];
    public readonly byte[] BuriedSphereTypes = new byte[MaxBuriedSpheres];
    public readonly byte[] BuriedSphereGrowth = new byte[MaxBuriedSpheres];
    public readonly byte[] BuriedSphereInitialSizes = new byte[MaxBuriedSpheres];
    public readonly byte[] BuriedSphereCoords = new byte[MaxBuriedSpheres * CoordSize];
    public readonly byte[] Unused = new byte[UnusedBytes];
    public uint PlatesMined;
    public readonly byte[] GoodsPC = new byte[MaxGoodsPC];
    public readonly byte[] Traps = new byte[InventorySize];
    public readonly byte[] GoodsBag = new byte[InventorySize];
    public readonly byte[] Treasures = new byte[InventorySize];
    public readonly byte[] SphereTypes = new byte[InventorySize];
    public readonly byte[] SphereSizes = new byte[InventorySize];
    public readonly byte[] PlacedGoodSlots = new byte[MaxPlacedGoods];
    public byte StepCount;
    public byte Flags;

    private static ushort[]? sTreasureItems;
    private static byte[]? sSphereGrowth;

    public Underground()
    {
        for (int i = 0; i < FlagOwnerCount; i++)
        {
            FlagOwnerNames[i] = new ushort[FlagOwnerNameLength];
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        Base.WriteTo(body, offset);
        Bin.W32(body, offset + SeedOffset, RandomSeed);
        Bin.W32(body, offset + GiftPenaltyMinutesOffset, (uint)GiftPenaltyMinutes);
        body[offset + GiftPenaltyStateOffset] = GiftPenaltyState;
        body[offset + CollectedOrbFlagOffset] = UnusedCollectedOrbFlag;
        for (int i = 0; i < FlagOwnerCount; i++)
        {
            Bin.W32(body, offset + FlagOwnerIDsOffset + i * 4, FlagOwnerIDs[i]);
            for (int j = 0; j < FlagOwnerNameLength; j++)
            {
                Bin.W16(body, offset + FlagOwnerNamesOffset + (i * FlagOwnerNameLength + j) * 2, FlagOwnerNames[i][j]);
            }
        }
        Array.Copy(FlagOwnerLanguages, 0, body, offset + FlagOwnerLanguagesOffset, FlagOwnerCount);
        Array.Copy(FlagOwnerGameCodes, 0, body, offset + FlagOwnerGameCodesOffset, FlagOwnerCount);
        body[offset + FlagOwnerIndexOffset] = FlagOwnerIndex;
        Array.Copy(SpawnedTrapIDs, 0, body, offset + SpawnedTrapIDsOffset, SpawnedTrapIDs.Length);
        Array.Copy(SpawnedTrapCoords, 0, body, offset + SpawnedTrapCoordsOffset, SpawnedTrapCoords.Length);
        Array.Copy(MiningSpotCoords, 0, body, offset + MiningSpotCoordsOffset, MiningSpotCoords.Length);
        Array.Copy(PlacedTrapIDs, 0, body, offset + PlacedTrapIDsOffset, PlacedTrapIDs.Length);
        Array.Copy(PlacedTrapCoords, 0, body, offset + PlacedTrapCoordsOffset, PlacedTrapCoords.Length);
        Array.Copy(PlacedTrapSpawnedIndices, 0, body, offset + PlacedTrapSpawnedIndicesOffset, PlacedTrapSpawnedIndices.Length);
        Array.Copy(BuriedSphereTypes, 0, body, offset + BuriedSphereTypesOffset, BuriedSphereTypes.Length);
        Array.Copy(BuriedSphereGrowth, 0, body, offset + BuriedSphereGrowthOffset, BuriedSphereGrowth.Length);
        Array.Copy(BuriedSphereInitialSizes, 0, body, offset + BuriedSphereInitialSizesOffset, BuriedSphereInitialSizes.Length);
        Array.Copy(BuriedSphereCoords, 0, body, offset + BuriedSphereCoordsOffset, BuriedSphereCoords.Length);
        Array.Copy(Unused, 0, body, offset + UnusedOffset, UnusedBytes);
        Bin.W32(body, offset + PlatesMinedOffset, PlatesMined);
        Array.Copy(GoodsPC, 0, body, offset + GoodsPCOffset, MaxGoodsPC);
        Array.Copy(Traps, 0, body, offset + TrapsOffset, InventorySize);
        Array.Copy(GoodsBag, 0, body, offset + GoodsBagOffset, InventorySize);
        Array.Copy(Treasures, 0, body, offset + TreasuresOffset, InventorySize);
        Array.Copy(SphereTypes, 0, body, offset + SphereTypesOffset, InventorySize);
        Array.Copy(SphereSizes, 0, body, offset + SphereSizesOffset, InventorySize);
        Array.Copy(PlacedGoodSlots, 0, body, offset + PlacedGoodSlotsOffset, MaxPlacedGoods);
        body[offset + StepCountOffset] = StepCount;
        body[offset + FlagsOffset] = Flags;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Base.ReadFrom(body, offset);
        RandomSeed = Bin.U32(body, offset + SeedOffset);
        GiftPenaltyMinutes = (int)Bin.U32(body, offset + GiftPenaltyMinutesOffset);
        GiftPenaltyState = body[offset + GiftPenaltyStateOffset];
        UnusedCollectedOrbFlag = body[offset + CollectedOrbFlagOffset];
        for (int i = 0; i < FlagOwnerCount; i++)
        {
            FlagOwnerIDs[i] = Bin.U32(body, offset + FlagOwnerIDsOffset + i * 4);
            for (int j = 0; j < FlagOwnerNameLength; j++)
            {
                FlagOwnerNames[i][j] = (ushort)Bin.U16(body, offset + FlagOwnerNamesOffset + (i * FlagOwnerNameLength + j) * 2);
            }
        }
        Array.Copy(body, offset + FlagOwnerLanguagesOffset, FlagOwnerLanguages, 0, FlagOwnerCount);
        Array.Copy(body, offset + FlagOwnerGameCodesOffset, FlagOwnerGameCodes, 0, FlagOwnerCount);
        FlagOwnerIndex = body[offset + FlagOwnerIndexOffset];
        Array.Copy(body, offset + SpawnedTrapIDsOffset, SpawnedTrapIDs, 0, SpawnedTrapIDs.Length);
        Array.Copy(body, offset + SpawnedTrapCoordsOffset, SpawnedTrapCoords, 0, SpawnedTrapCoords.Length);
        Array.Copy(body, offset + MiningSpotCoordsOffset, MiningSpotCoords, 0, MiningSpotCoords.Length);
        Array.Copy(body, offset + PlacedTrapIDsOffset, PlacedTrapIDs, 0, PlacedTrapIDs.Length);
        Array.Copy(body, offset + PlacedTrapCoordsOffset, PlacedTrapCoords, 0, PlacedTrapCoords.Length);
        Array.Copy(body, offset + PlacedTrapSpawnedIndicesOffset, PlacedTrapSpawnedIndices, 0, PlacedTrapSpawnedIndices.Length);
        Array.Copy(body, offset + BuriedSphereTypesOffset, BuriedSphereTypes, 0, BuriedSphereTypes.Length);
        Array.Copy(body, offset + BuriedSphereGrowthOffset, BuriedSphereGrowth, 0, BuriedSphereGrowth.Length);
        Array.Copy(body, offset + BuriedSphereInitialSizesOffset, BuriedSphereInitialSizes, 0, BuriedSphereInitialSizes.Length);
        Array.Copy(body, offset + BuriedSphereCoordsOffset, BuriedSphereCoords, 0, BuriedSphereCoords.Length);
        Array.Copy(body, offset + UnusedOffset, Unused, 0, UnusedBytes);
        PlatesMined = Bin.U32(body, offset + PlatesMinedOffset);
        Array.Copy(body, offset + GoodsPCOffset, GoodsPC, 0, MaxGoodsPC);
        Array.Copy(body, offset + TrapsOffset, Traps, 0, InventorySize);
        Array.Copy(body, offset + GoodsBagOffset, GoodsBag, 0, InventorySize);
        Array.Copy(body, offset + TreasuresOffset, Treasures, 0, InventorySize);
        Array.Copy(body, offset + SphereTypesOffset, SphereTypes, 0, InventorySize);
        Array.Copy(body, offset + SphereSizesOffset, SphereSizes, 0, InventorySize);
        Array.Copy(body, offset + PlacedGoodSlotsOffset, PlacedGoodSlots, 0, MaxPlacedGoods);
        StepCount = body[offset + StepCountOffset];
        Flags = body[offset + FlagsOffset];
    }

    private static void SetCoords(byte[] coords, int index, int x, int z)
    {
        coords[index * CoordSize] = (byte)x;
        coords[index * CoordSize + 1] = (byte)(((x & 0xF00) >> 8) + ((z & 0xF00) >> 4));
        coords[index * CoordSize + 2] = (byte)z;
    }

    private static int CoordX(byte[] coords, int index)
    {
        return coords[index * CoordSize] + ((coords[index * CoordSize + 1] << 8) & 0xF00);
    }

    private static int CoordZ(byte[] coords, int index)
    {
        return coords[index * CoordSize + 2] + ((coords[index * CoordSize + 1] << 4) & 0xF00);
    }

    private static int FindEmpty(byte[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] == 0)
            {
                return i;
            }
        }
        return NoSlot;
    }

    private static int CountUsed(byte[] list)
    {
        int count;
        for (count = 0; count < list.Length; count++)
        {
            if (list[count] == 0)
            {
                break;
            }
        }
        return count;
    }

    private static int RemoveAt(byte[] list, int slot)
    {
        int value = list[slot];
        for (int i = slot; i < list.Length - 1; i++)
        {
            list[i] = list[i + 1];
        }
        list[list.Length - 1] = 0;
        return value;
    }

    private static void MoveAfter(byte[] list, int origSlot, int slotToMoveAfter)
    {
        byte[] copy = (byte[])list.Clone();
        int used = 0;
        for (int i = 0; i < list.Length; i++)
        {
            if (i != origSlot)
            {
                list[used] = copy[i];
                used++;
            }
            if (i == slotToMoveAfter)
            {
                list[used] = copy[origSlot];
                used++;
            }
        }
    }

    private void SetSpawnFlag()
    {
        Flags = (byte)((Flags & ~SpawnFlagMask) | 1);
    }

    // OFFSET 0x02028574
    public static void Underground_Init(Underground underground)
    {
        Rtc.GetCurrentDateTime(out RtcDate date, out RtcTime time);
        uint seed = (uint)(time.Second + gSystem.FrameCounter);
        seed += (uint)(((((((((date.Year << 5) + date.Month) << 5) + date.Day) << 5) + time.Hour) << 5) + time.Minute) << 5);
        underground.ReadFrom(new byte[Size], 0);
        underground.RandomSeed = seed;
        underground.SetSpawnFlag();
    }

    // OFFSET 0x020285D8
    private static int Underground_FindEmptyGoodsBagSlot(Underground underground)
    {
        return FindEmpty(underground.GoodsBag);
    }

    // OFFSET 0x020285F8
    private static int Underground_FindEmptySphereSlot(Underground underground)
    {
        return FindEmpty(underground.SphereTypes);
    }

    // OFFSET 0x02028618
    private static int Underground_FindEmptyTreasureSlot(Underground underground)
    {
        return FindEmpty(underground.Treasures);
    }

    // OFFSET 0x02028638
    private static int Underground_FindEmptyTrapSlot(Underground underground)
    {
        return FindEmpty(underground.Traps);
    }

    // OFFSET 0x02028658
    public static void Underground_HandleDailyEvents(SaveData saveData, int daysPassed)
    {
        Underground underground = SaveData_GetUnderground(saveData);
        sSphereGrowth ??= RomData.Arm9(SphereGrowthTable, SphereTypeCount * 2);
        if (daysPassed <= 0)
        {
            return;
        }
        int days = daysPassed;
        if (days > MaxDaysProcessed)
        {
            days = MaxDaysProcessed;
        }
        uint seed = underground.RandomSeed;
        for (int day = 0; day < days; day++)
        {
            for (int i = 0; i < MaxBuriedSpheres; i++)
            {
                int type = underground.BuriedSphereTypes[i];
                if (type == 0)
                {
                    continue;
                }
                uint range = sSphereGrowth[type];
                seed = seed * RandomMultiplier + RandomIncrement;
                uint random;
                if (range == 0)
                {
                    random = seed >> 16;
                }
                else
                {
                    random = ((seed >> 16) * range) >> 16;
                }
                int growth = sSphereGrowth[SphereTypeCount + type] + (ushort)random;
                if (underground.BuriedSphereGrowth[i] + growth < MaxSphereGrowth)
                {
                    underground.BuriedSphereGrowth[i] = (byte)(underground.BuriedSphereGrowth[i] + growth);
                }
                else
                {
                    underground.BuriedSphereGrowth[i] = MaxSphereGrowth;
                }
            }
        }
        underground.RandomSeed = MathUtil.ARNG_Next(underground.RandomSeed + (uint)daysPassed);
        underground.SetSpawnFlag();
    }

    // OFFSET 0x02028758
    public static void Underground_ProgressGiftPenalty(SaveData saveData, int minutes, int hasTimePenalty)
    {
        Underground underground = SaveData_GetUnderground(saveData);
        if (minutes < 0 || hasTimePenalty != 0)
        {
            return;
        }
        if (underground.GiftPenaltyMinutes < minutes)
        {
            underground.GiftPenaltyMinutes = 0;
        }
        else
        {
            underground.GiftPenaltyMinutes -= minutes;
        }
    }

    // OFFSET 0x0202878C
    public static void Underground_UpdateGiftPenaltyState(SaveData saveData)
    {
        Underground underground = SaveData_GetUnderground(saveData);
        if (underground.GiftPenaltyMinutes == 0 && underground.GiftPenaltyState == GiftPenaltyActive)
        {
            underground.GiftPenaltyState = GiftPenaltyNone;
        }
        if (underground.GiftPenaltyMinutes == 0 && underground.GiftPenaltyState == GiftPenaltyPrimed)
        {
            underground.GiftPenaltyMinutes = GiftPenaltyDuration;
        }
        if (underground.GiftPenaltyState == GiftPenaltyPrimed)
        {
            underground.GiftPenaltyState = GiftPenaltyActive;
        }
    }

    // OFFSET 0x020287E0
    public static void Underground_SetGiftPenaltyPrimedFlag(SaveData saveData)
    {
        Underground underground = SaveData_GetUnderground(saveData);
        if (underground.GiftPenaltyState == GiftPenaltyNone)
        {
            underground.GiftPenaltyState = GiftPenaltyPrimed;
        }
    }

    // OFFSET 0x020287F8
    public static void Underground_ClearGiftPenaltyPrimedFlag(SaveData saveData)
    {
        Underground underground = SaveData_GetUnderground(saveData);
        if (underground.GiftPenaltyState == GiftPenaltyPrimed)
        {
            underground.GiftPenaltyState = GiftPenaltyNone;
        }
    }

    // OFFSET 0x02028810
    public static bool Underground_CanExchangeGifts(SaveData saveData)
    {
        Underground underground = SaveData_GetUnderground(saveData);
        if (underground.GiftPenaltyState != GiftPenaltyActive)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02028828
    public static void Underground_SetUnusedCollectedOrbFlag(Underground underground)
    {
        underground.UnusedCollectedOrbFlag = 1;
    }

    // OFFSET 0x02028830
    public static void Underground_StoreRegisteredFlagOwnerInfo(Underground underground, TrainerInfo info)
    {
        int index = underground.FlagOwnerIndex;
        for (int i = 0; i < FlagOwnerCount; i++)
        {
            if (underground.FlagOwnerIDs[i] == TrainerInfo.TrainerInfo_ID(info))
            {
                return;
            }
        }
        if (index >= FlagOwnerCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        Array.Copy(TrainerInfo.TrainerInfo_Name(info), underground.FlagOwnerNames[index], FlagOwnerNameLength);
        underground.FlagOwnerIDs[index] = TrainerInfo.TrainerInfo_ID(info);
        underground.FlagOwnerLanguages[index] = TrainerInfo.TrainerInfo_Language(info);
        underground.FlagOwnerGameCodes[index] = TrainerInfo.TrainerInfo_GameCode(info);
        underground.FlagOwnerIndex++;
        if (underground.FlagOwnerIndex >= FlagOwnerCount)
        {
            underground.FlagOwnerIndex = 0;
        }
    }

    // OFFSET 0x020288C8
    public static TrainerInfo? Underground_GetRegisteredFlagOwnerInfo(Underground underground, int heapID, int offset)
    {
        int index = underground.FlagOwnerIndex - offset - 1;
        if (index < 0)
        {
            index += FlagOwnerCount;
        }
        if (underground.FlagOwnerNames[index][0] == 0)
        {
            return null;
        }
        TrainerInfo info = TrainerInfo.TrainerInfo_New(heapID);
        TrainerInfo.TrainerInfo_SetName(info, underground.FlagOwnerNames[index]);
        TrainerInfo.TrainerInfo_SetGameCode(info, underground.FlagOwnerGameCodes[index]);
        TrainerInfo.TrainerInfo_SetLanguage(info, underground.FlagOwnerLanguages[index]);
        TrainerInfo.TrainerInfo_SetID(info, underground.FlagOwnerIDs[index]);
        return info;
    }

    // OFFSET 0x02028930
    public static uint Underground_GetRandomSeed(Underground underground)
    {
        return underground.RandomSeed;
    }

    // OFFSET 0x02028938
    public static int Underground_ConvertTreasureToBagItem(int treasureID)
    {
        if (treasureID < FirstTreasure)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (treasureID >= TreasureLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        sTreasureItems ??= RomData.Arm9U16(TreasureItemTable, TreasureLimit - FirstTreasure);
        return sTreasureItems[treasureID - FirstTreasure];
    }

    // OFFSET 0x0202895C
    public static bool Underground_TryAddGoodPC(Underground underground, int goodID)
    {
        bool added = false;
        for (int i = 0; i < MaxGoodsPC; i++)
        {
            if (underground.GoodsPC[i] == 0)
            {
                underground.GoodsPC[i] = (byte)goodID;
                added = true;
                break;
            }
        }
        return added;
    }

    // OFFSET 0x02028984
    public static bool Underground_IsRoomForGoodsInPC(Underground underground, int unused)
    {
        for (int i = 0; i < MaxGoodsPC; i++)
        {
            if (underground.GoodsPC[i] == 0)
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x020289A0
    public static int Underground_GetGoodsCountPC(Underground underground)
    {
        return CountUsed(underground.GoodsPC);
    }

    // OFFSET 0x020289B8
    public static int Underground_GetGoodAtSlotPC(Underground underground, int slot)
    {
        return underground.GoodsPC[slot];
    }

    // OFFSET 0x020289C4
    public static int Underground_RemoveGoodAtSlotPC(Underground underground, int slot)
    {
        if (Underground_IsGoodAtSlotPlacedInBase(underground, slot))
        {
            ErrorHandling.GF_AssertFail();
        }
        int goodID = RemoveAt(underground.GoodsPC, slot);
        Underground_ShiftPlacedGoodSlots(underground, slot, -1);
        return goodID;
    }

    // OFFSET 0x02028A10
    public static void Underground_MoveGoodPC(Underground underground, int origSlot, int slotToMoveAfter)
    {
        int placedIndex = NoSlot;
        MoveAfter(underground.GoodsPC, origSlot, slotToMoveAfter);
        for (int i = 0; i < MaxPlacedGoods; i++)
        {
            if (origSlot + 1 == underground.PlacedGoodSlots[i])
            {
                placedIndex = i;
                break;
            }
        }
        Underground_ShiftPlacedGoodSlots(underground, slotToMoveAfter, 1);
        Underground_ShiftPlacedGoodSlots(underground, origSlot, -1);
        if (placedIndex == NoSlot)
        {
            return;
        }
        if (origSlot < slotToMoveAfter)
        {
            underground.PlacedGoodSlots[placedIndex] = (byte)(slotToMoveAfter + 1);
        }
        else
        {
            underground.PlacedGoodSlots[placedIndex] = (byte)(slotToMoveAfter + 2);
        }
    }

    // OFFSET 0x02028ACC
    public static int Underground_AddPlacedGood(Underground underground, int slot, int index)
    {
        if (index < 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (index > MaxPlacedGoods)
        {
            ErrorHandling.GF_AssertFail();
        }
        underground.PlacedGoodSlots[index - 1] = (byte)(slot + 1);
        return underground.GoodsPC[slot];
    }

    // OFFSET 0x02028AFC
    public static bool Underground_IsGoodAtSlotPlacedInBase(Underground underground, int slot)
    {
        for (int i = 0; i < MaxPlacedGoods; i++)
        {
            if (slot + 1 == underground.PlacedGoodSlots[i])
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x02028B20
    public static void Underground_RemovePlacedGood(Underground underground, int index)
    {
        if (index - 1 < MaxPlacedGoods)
        {
            underground.PlacedGoodSlots[index - 1] = 0;
        }
    }

    // OFFSET 0x02028B34
    public static void Underground_InitPlacedGoodSlots(Underground underground)
    {
        Array.Clear(underground.PlacedGoodSlots);
    }

    // OFFSET 0x02028B48
    private static void Underground_ShiftPlacedGoodSlots(Underground underground, int slot, int delta)
    {
        for (int i = 0; i < MaxPlacedGoods; i++)
        {
            if (underground.PlacedGoodSlots[i] > slot + 1)
            {
                underground.PlacedGoodSlots[i] = (byte)(underground.PlacedGoodSlots[i] + delta);
            }
        }
    }

    // OFFSET 0x02028B70
    public static int Underground_GetGoodsCountBag(Underground underground)
    {
        return CountUsed(underground.GoodsBag);
    }

    // OFFSET 0x02028B88
    public static int Underground_GetGoodAtSlotBag(Underground underground, int slot)
    {
        return underground.GoodsBag[slot];
    }

    // OFFSET 0x02028B94
    public static int Underground_RemoveGoodAtSlotBag(Underground underground, int slot)
    {
        return RemoveAt(underground.GoodsBag, slot);
    }

    // OFFSET 0x02028BC8
    public static bool Underground_TryAddGoodBag(Underground underground, int goodID)
    {
        bool added = false;
        int slot = Underground_FindEmptyGoodsBagSlot(underground);
        if (slot != NoSlot)
        {
            underground.GoodsBag[slot] = (byte)goodID;
            added = true;
        }
        return added;
    }

    // OFFSET 0x02028BE8
    public static void Underground_MoveGoodBag(Underground underground, int origSlot, int slotToMoveAfter)
    {
        MoveAfter(underground.GoodsBag, origSlot, slotToMoveAfter);
    }

    // OFFSET 0x02028C3C
    public static int Underground_GetSphereCount(Underground underground)
    {
        return CountUsed(underground.SphereTypes);
    }

    // OFFSET 0x02028C54
    public static int Underground_GetSphereTypeAtSlot(Underground underground, int slot)
    {
        return underground.SphereTypes[slot];
    }

    // OFFSET 0x02028C60
    public static int Underground_GetSphereSizeAtSlot(Underground underground, int slot)
    {
        return underground.SphereSizes[slot];
    }

    // OFFSET 0x02028C6C
    public static int Underground_RemoveSphereAtSlot(Underground underground, int slot)
    {
        int type = underground.SphereTypes[slot];
        for (int i = slot; i < InventorySize - 1; i++)
        {
            underground.SphereTypes[i] = underground.SphereTypes[i + 1];
            underground.SphereSizes[i] = underground.SphereSizes[i + 1];
        }
        underground.SphereTypes[InventorySize - 1] = 0;
        return type;
    }

    // OFFSET 0x02028CB0
    public static bool Underground_TryAddSphere(Underground underground, int sphereType, int sphereSize)
    {
        bool added = false;
        int slot = Underground_FindEmptySphereSlot(underground);
        if (slot != NoSlot)
        {
            underground.SphereTypes[slot] = (byte)sphereType;
            underground.SphereSizes[slot] = (byte)sphereSize;
            added = true;
        }
        return added;
    }

    // OFFSET 0x02028CD8
    public static void Underground_MoveSphereInInventory(Underground underground, int origSlot, int slotToMoveAfter)
    {
        MoveAfter(underground.SphereTypes, origSlot, slotToMoveAfter);
        MoveAfter(underground.SphereSizes, origSlot, slotToMoveAfter);
    }

    // OFFSET 0x02028D58
    public static int Underground_GetTreasureCount(Underground underground)
    {
        return CountUsed(underground.Treasures);
    }

    // OFFSET 0x02028D74
    public static int Underground_GetTreasureAtSlot(Underground underground, int slot)
    {
        return underground.Treasures[slot];
    }

    // OFFSET 0x02028D80
    public static int Underground_RemoveTreasureAtSlot(Underground underground, int slot)
    {
        return RemoveAt(underground.Treasures, slot);
    }

    // OFFSET 0x02028DB4
    public static bool Underground_TryAddTreasure(Underground underground, int treasureID)
    {
        bool added = false;
        int slot = Underground_FindEmptyTreasureSlot(underground);
        if (slot != NoSlot)
        {
            underground.Treasures[slot] = (byte)treasureID;
            added = true;
        }
        return added;
    }

    // OFFSET 0x02028DD8
    public static void Underground_MoveTreasureInInventory(Underground underground, int origSlot, int slotToMoveAfter)
    {
        MoveAfter(underground.Treasures, origSlot, slotToMoveAfter);
    }

    // OFFSET 0x02028E28
    public static int Underground_GetTrapCount(Underground underground)
    {
        return CountUsed(underground.Traps);
    }

    // OFFSET 0x02028E44
    public static int Underground_GetTrapAtSlot(Underground underground, int slot)
    {
        return underground.Traps[slot];
    }

    // OFFSET 0x02028E50
    public static int Underground_RemoveTrapAtSlot(Underground underground, int slot)
    {
        return RemoveAt(underground.Traps, slot);
    }

    // OFFSET 0x02028E84
    public static bool Underground_TryAddTrap(Underground underground, int trapID)
    {
        bool added = false;
        int slot = Underground_FindEmptyTrapSlot(underground);
        if (slot != NoSlot)
        {
            underground.Traps[slot] = (byte)trapID;
            added = true;
        }
        return added;
    }

    // OFFSET 0x02028EA8
    public static void Underground_MoveTrapInInventory(Underground underground, int origSlot, int slotToMoveAfter)
    {
        MoveAfter(underground.Traps, origSlot, slotToMoveAfter);
    }

    // OFFSET 0x02028EF8
    public static void Underground_SaveSpawnedTrap(Underground underground, int trapID, int index, int x, int z)
    {
        if (index >= MaxSpawnedTraps)
        {
            ErrorHandling.GF_AssertFail();
        }
        underground.SpawnedTrapIDs[index] = (byte)trapID;
        SetCoords(underground.SpawnedTrapCoords, index, x, z);
    }

    // OFFSET 0x02028F40
    public static int Underground_GetSpawnedTrapIDAtIndex(Underground underground, int index)
    {
        if (index >= MaxSpawnedTraps)
        {
            ErrorHandling.GF_AssertFail();
        }
        return underground.SpawnedTrapIDs[index];
    }

    // OFFSET 0x02028F5C
    public static int Underground_GetSpawnedTrapXCoordAtIndex(Underground underground, int index)
    {
        if (index >= MaxSpawnedTraps)
        {
            ErrorHandling.GF_AssertFail();
        }
        return CoordX(underground.SpawnedTrapCoords, index);
    }

    // OFFSET 0x02028F88
    public static int Underground_GetSpawnedTrapZCoordAtIndex(Underground underground, int index)
    {
        if (index >= MaxSpawnedTraps)
        {
            ErrorHandling.GF_AssertFail();
        }
        return CoordZ(underground.SpawnedTrapCoords, index);
    }

    // OFFSET 0x02028FB4
    public static void Underground_RemoveSpawnedTrapAtIndex(Underground underground, int index)
    {
        if (index >= MaxSpawnedTraps)
        {
            ErrorHandling.GF_AssertFail();
        }
        underground.SpawnedTrapIDs[index] = 0;
        Array.Clear(underground.SpawnedTrapCoords, index * CoordSize, CoordSize);
    }

    // OFFSET 0x02028FE0
    public static void Underground_SavePlacedTrap(Underground underground, int trapID, int index, int x, int z, int spawnedIndex)
    {
        if (index >= MaxPlacedTraps)
        {
            ErrorHandling.GF_AssertFail();
        }
        underground.PlacedTrapIDs[index] = (byte)trapID;
        SetCoords(underground.PlacedTrapCoords, index, x, z);
        underground.PlacedTrapSpawnedIndices[index] = (byte)spawnedIndex;
    }

    // OFFSET 0x02029030
    public static int Underground_GetPlacedTrapIDAtIndex(Underground underground, int index)
    {
        return underground.PlacedTrapIDs[index];
    }

    // OFFSET 0x0202903C
    public static int Underground_GetPlacedTrapXCoordAtIndex(Underground underground, int index)
    {
        return CoordX(underground.PlacedTrapCoords, index);
    }

    // OFFSET 0x0202905C
    public static int Underground_GetPlacedTrapZCoordAtIndex(Underground underground, int index)
    {
        return CoordZ(underground.PlacedTrapCoords, index);
    }

    // OFFSET 0x0202907C
    public static int Underground_GetPlacedTrapSpawnedIndexAtIndex(Underground underground, int index)
    {
        return underground.PlacedTrapSpawnedIndices[index];
    }

    // OFFSET 0x02029088
    public static void Underground_SaveBuriedSphere(Underground underground, int type, int index, int x, int z, int initialSize, int growth)
    {
        if (index >= MaxBuriedSpheres)
        {
            ErrorHandling.GF_AssertFail();
        }
        underground.BuriedSphereTypes[index] = (byte)type;
        SetCoords(underground.BuriedSphereCoords, index, x, z);
        underground.BuriedSphereInitialSizes[index] = (byte)initialSize;
        underground.BuriedSphereGrowth[index] = (byte)growth;
    }

    // OFFSET 0x020290DC
    public static int Underground_GetBuriedSphereTypeAtIndex(Underground underground, int index)
    {
        return underground.BuriedSphereTypes[index];
    }

    // OFFSET 0x020290E8
    public static int Underground_GetBuriedSphereXCoordAtIndex(Underground underground, int index)
    {
        return CoordX(underground.BuriedSphereCoords, index);
    }

    // OFFSET 0x02029108
    public static int Underground_GetBuriedSphereZCoordAtIndex(Underground underground, int index)
    {
        return CoordZ(underground.BuriedSphereCoords, index);
    }

    // OFFSET 0x02029128
    public static int Underground_GetBuriedSphereInitialSizeAtIndex(Underground underground, int index)
    {
        return underground.BuriedSphereInitialSizes[index];
    }

    // OFFSET 0x02029134
    public static int Underground_GetBuriedSphereGrowthAtIndex(Underground underground, int index)
    {
        return underground.BuriedSphereGrowth[index];
    }

    // OFFSET 0x02029140
    public static int Underground_TryAddMiningSpot(Underground underground, int x, int z)
    {
        int index;
        for (index = 0; index < MaxMiningSpots; index++)
        {
            int entry = index * CoordSize;
            if (underground.MiningSpotCoords[entry] == 0 && underground.MiningSpotCoords[entry + 1] == 0 && underground.MiningSpotCoords[entry + 2] == 0)
            {
                SetCoords(underground.MiningSpotCoords, index, x, z);
                break;
            }
        }
        if (index == MaxMiningSpots)
        {
            index = 0;
        }
        return index;
    }

    // OFFSET 0x020291A4
    public static void Underground_RemoveMiningSpotAtIndex(Underground underground, int index)
    {
        if (index >= MaxMiningSpots)
        {
            ErrorHandling.GF_AssertFail();
        }
        Array.Clear(underground.MiningSpotCoords, index * CoordSize, CoordSize);
    }

    // OFFSET 0x020291CC
    public static int Underground_GetMiningSpotXCoordAtIndex(Underground underground, int index)
    {
        return CoordX(underground.MiningSpotCoords, index);
    }

    // OFFSET 0x020291EC
    public static int Underground_GetMiningSpotZCoordAtIndex(Underground underground, int index)
    {
        return CoordZ(underground.MiningSpotCoords, index);
    }

    // OFFSET 0x0202920C
    public static bool Underground_HasPlayerNeverMined(Underground underground)
    {
        if ((underground.Flags & MinedFlagMask) == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02029220
    public static void Underground_SetHasMined(Underground underground)
    {
        underground.Flags = (byte)((underground.Flags & ~MinedFlagMask) | MinedFlag);
    }

    // OFFSET 0x02029234
    public static int Underground_ShouldSpawnNewBuriedObjects(Underground underground)
    {
        return underground.Flags & SpawnFlagMask;
    }

    // OFFSET 0x02029240
    public static void Underground_FlagSpawnedNewBuriedObjects(Underground underground)
    {
        underground.Flags = (byte)(underground.Flags & ~SpawnFlagMask);
    }

    // OFFSET 0x02029250
    public static void Underground_SetPlateMined(Underground underground, int miningItemID)
    {
        if (miningItemID >= FirstPlate && miningItemID <= LastPlate)
        {
            underground.PlatesMined |= 1u << (miningItemID - FirstPlate);
        }
    }

    // OFFSET 0x02029274
    public static bool Underground_HasPlateNeverBeenMined(Underground underground, int miningItemID)
    {
        if (miningItemID < FirstPlate || miningItemID > LastPlate)
        {
            return true;
        }
        if ((underground.PlatesMined & (1u << (miningItemID - FirstPlate))) != 0)
        {
            return false;
        }
        return true;
    }

    // OFFSET 0x0202929C
    public static void Underground_IncrementStepCount(Underground underground)
    {
        if (underground.StepCount >= MaxStepCount)
        {
            underground.StepCount = 0;
        }
        else
        {
            underground.StepCount++;
        }
    }

    // OFFSET 0x020292B4
    public static int Underground_GetStepCount(Underground underground)
    {
        return underground.StepCount;
    }

    // OFFSET 0x020298B0
    public static Underground SaveData_GetUnderground(SaveData saveData)
    {
        return (Underground)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryUnderground);
    }
}
