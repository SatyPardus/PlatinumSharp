using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class BerryPatch
{
    public const int Size = 0xE;

    public byte BerryID;
    public byte GrowthStage;
    public ushort StageMinutes;
    public ushort MinuteRemainder;
    public byte RegrowCount;
    public ushort Yield;
    public byte Moisture;
    public byte YieldRating;
    public byte Mulch;
    public byte GrowthActive;

    public void WriteTo(byte[] body, int offset)
    {
        body[offset] = BerryID;
        body[offset + 1] = GrowthStage;
        Bin.W16(body, offset + 2, StageMinutes);
        Bin.W16(body, offset + 4, MinuteRemainder);
        body[offset + 6] = RegrowCount;
        body[offset + 7] = 0;
        Bin.W16(body, offset + 8, Yield);
        body[offset + 0xA] = Moisture;
        body[offset + 0xB] = YieldRating;
        body[offset + 0xC] = Mulch;
        body[offset + 0xD] = GrowthActive;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        BerryID = body[offset];
        GrowthStage = body[offset + 1];
        StageMinutes = (ushort)Bin.U16(body, offset + 2);
        MinuteRemainder = (ushort)Bin.U16(body, offset + 4);
        RegrowCount = body[offset + 6];
        Yield = (ushort)Bin.U16(body, offset + 8);
        Moisture = body[offset + 0xA];
        YieldRating = body[offset + 0xB];
        Mulch = body[offset + 0xC];
        GrowthActive = body[offset + 0xD];
    }
}

public static class BerryPatches
{
    public const int MaxPatches = 128;
    public const int BerryCount = 64;
    public const int StageNone = 0;
    public const int StagePlanted = 1;
    public const int StageSprouted = 2;
    public const int StageGrowing = 3;
    public const int StageBlooming = 4;
    public const int StageFruit = 5;
    public const int MulchNone = 0;
    public const int MulchGrowth = 1;
    public const int MulchDamp = 2;
    public const int MulchStable = 3;
    public const int MulchGooey = 4;
    public const int MoistureDry = 0;
    public const int MoistureLow = 1;
    public const int MoistureHigh = 2;

    private const int GrowthDataEntrySize = 3;
    private const int GrowthDataStageHours = 0;
    private const int GrowthDataMoistureDrain = 1;
    private const int GrowthDataBaseYield = 2;
    private const int BerryAttributeBaseYield = 2;
    private const int BerryAttributeStageHours = 3;
    private const int BerryAttributeMoistureDrain = 4;
    private const int FirstBerryItem = 0x94;
    private const int MinutesPerHour = 60;
    private const int FullMoisture = 100;
    private const int LowMoistureLimit = 50;
    private const int InitialYieldRating = 3;
    private const int FullYieldRating = 5;
    private const int MinimumYield = 2;
    private const int InitialFruitMultiplier = 4;
    private const int FruitMultiplier = 4;
    private const int StableFruitMultiplier = 6;
    private const int RegrowLimit = 10;
    private const int GooeyRegrowLimit = 15;
    private const int GrowingStageCount = 3;

    // OFFSET 0x02027B6C
    public static void BerryPatches_Clear(BerryPatch[] patches)
    {
        for (int i = 0; i < MaxPatches; i++)
        {
            BerryPatch_Clear(patches[i]);
        }
        for (int i = 0; i < MaxPatches; i++)
        {
            patches[i].GrowthStage = 0;
            patches[i].BerryID = 0;
        }
    }

    // OFFSET 0x02027B90
    public static void BerryPatches_Init(BerryPatch[] patches, int heapID, ushort[] initialPatches, int count)
    {
        byte[] growthData = BerryGrowthData_Init(heapID);
        for (int i = 0; i < MaxPatches || i < count; i++)
        {
            BerryPatch patch = patches[i];
            BerryPatch_Clear(patch);
            patch.BerryID = (byte)(initialPatches[i * 2] - FirstBerryItem);
            patch.GrowthStage = StageFruit;
            patch.StageMinutes = (ushort)(BerryGrowthData_StageMinutes(growthData, patch.BerryID, patch.Mulch) * InitialFruitMultiplier);
            patch.Yield = initialPatches[i * 2 + 1];
            patch.Moisture = FullMoisture;
            patch.YieldRating = InitialYieldRating;
        }
    }

    // OFFSET 0x02027BF4
    public static byte[] BerryGrowthData_Init(int heapID)
    {
        object archive = BerryData.BerryData_OpenArchive(heapID);
        byte[] growthData = new byte[BerryCount * GrowthDataEntrySize];
        for (int i = 0; i < BerryCount; i++)
        {
            object berryData = BerryData.BerryData_LoadFromOpenArchive(archive, i, heapID);
            growthData[i * GrowthDataEntrySize + GrowthDataStageHours] = (byte)BerryData.BerryData_GetAttribute(berryData, BerryAttributeStageHours);
            growthData[i * GrowthDataEntrySize + GrowthDataMoistureDrain] = (byte)BerryData.BerryData_GetAttribute(berryData, BerryAttributeMoistureDrain);
            growthData[i * GrowthDataEntrySize + GrowthDataBaseYield] = (byte)BerryData.BerryData_GetAttribute(berryData, BerryAttributeBaseYield);
        }
        BerryData.BerryData_CloseArchive(archive);
        return growthData;
    }

    private static int GrowthDataValue(byte[] growthData, int berryID, int field)
    {
        int index = (berryID - 1) * GrowthDataEntrySize + field;
        if (index < 0 || index >= growthData.Length)
        {
            return 0;
        }
        return growthData[index];
    }

    // OFFSET 0x02027C50
    private static void BerryPatch_Clear(BerryPatch patch)
    {
        patch.BerryID = 0;
        patch.GrowthStage = 0;
        patch.StageMinutes = 0;
        patch.MinuteRemainder = 0;
        patch.RegrowCount = 0;
        patch.Yield = 0;
        patch.Moisture = 0;
        patch.YieldRating = 0;
        patch.Mulch = 0;
        patch.GrowthActive = 0;
    }

    // OFFSET 0x02027C68
    private static int BerryPatch_CalcYield(BerryPatch patch, byte[] growthData)
    {
        return patch.YieldRating * GrowthDataValue(growthData, patch.BerryID, GrowthDataBaseYield);
    }

    // OFFSET 0x02027C7C
    private static int BerryGrowthData_StageMinutes(byte[] growthData, int berryID, int mulch)
    {
        int minutes = GrowthDataValue(growthData, berryID, GrowthDataStageHours) * MinutesPerHour;
        if (mulch == MulchGrowth)
        {
            return minutes * 3 / 4;
        }
        if (mulch == MulchDamp)
        {
            return minutes + minutes / 2;
        }
        return minutes;
    }

    // OFFSET 0x02027CA8
    private static int BerryGrowthData_MoistureDrain(byte[] growthData, int berryID, int mulch)
    {
        int drain = GrowthDataValue(growthData, berryID, GrowthDataMoistureDrain);
        if (mulch == MulchDamp)
        {
            return drain / 2;
        }
        if (mulch == MulchGrowth)
        {
            return drain + drain / 2;
        }
        return drain;
    }

    // OFFSET 0x02027CCC
    private static int BerryPatch_FruitMultiplier(BerryPatch patch)
    {
        if (patch.Mulch == MulchStable)
        {
            return StableFruitMultiplier;
        }
        return FruitMultiplier;
    }

    // OFFSET 0x02027CDC
    private static int BerryPatch_RegrowLimit(BerryPatch patch)
    {
        if (patch.Mulch == MulchGooey)
        {
            return GooeyRegrowLimit;
        }
        return RegrowLimit;
    }

    // OFFSET 0x02027CEC
    private static int BerryPatch_LifetimeStages(BerryPatch patch)
    {
        int fruitMultiplier = BerryPatch_FruitMultiplier(patch);
        return BerryPatch_RegrowLimit(patch) * (fruitMultiplier + GrowingStageCount) + 1;
    }

    // OFFSET 0x02027D04
    public static int BerryPatches_GrowthStage(BerryPatch[] patches, int patchID)
    {
        return patches[patchID].GrowthStage;
    }

    // OFFSET 0x02027D10
    public static int BerryPatches_BerryID(BerryPatch[] patches, int patchID)
    {
        return patches[patchID].BerryID;
    }

    // OFFSET 0x02027D18
    public static int BerryPatches_MoistureLevel(BerryPatch[] patches, int patchID)
    {
        int moisture = patches[patchID].Moisture;
        if (moisture == 0)
        {
            return MoistureDry;
        }
        if (moisture <= LowMoistureLimit)
        {
            return MoistureLow;
        }
        return MoistureHigh;
    }

    // OFFSET 0x02027D34
    public static int BerryPatches_YieldRating(BerryPatch[] patches, int patchID)
    {
        return patches[patchID].YieldRating;
    }

    // OFFSET 0x02027D40
    public static void BerryPatches_PlantBerry(BerryPatch[] patches, int patchID, byte[] growthData, int berryID)
    {
        BerryPatch patch = patches[patchID];
        patch.BerryID = (byte)berryID;
        patch.GrowthStage = StagePlanted;
        patch.StageMinutes = (ushort)BerryGrowthData_StageMinutes(growthData, berryID, patch.Mulch);
        patch.MinuteRemainder = 0;
        patch.RegrowCount = 0;
        patch.Yield = 0;
        patch.Moisture = FullMoisture;
        patch.YieldRating = FullYieldRating;
        patch.GrowthActive = 1;
    }

    // OFFSET 0x02027D78
    public static void BerryPatches_Water(BerryPatch[] patches, int patchID)
    {
        patches[patchID].Moisture = FullMoisture;
    }

    // OFFSET 0x02027D84
    public static int BerryPatches_IsGrowthActive(BerryPatch[] patches, int patchID)
    {
        return patches[patchID].GrowthActive;
    }

    // OFFSET 0x02027D90
    public static void BerryPatches_SetGrowthActive(BerryPatch[] patches, int patchID, int active)
    {
        patches[patchID].GrowthActive = (byte)active;
    }

    // OFFSET 0x02027D9C
    public static int BerryPatches_Mulch(BerryPatch[] patches, int patchID)
    {
        return patches[patchID].Mulch;
    }

    // OFFSET 0x02027DA8
    public static void BerryPatches_SetMulch(BerryPatch[] patches, int patchID, int mulch)
    {
        patches[patchID].Mulch = (byte)mulch;
    }

    // OFFSET 0x02027DB4
    public static int BerryPatches_Yield(BerryPatch[] patches, int patchID)
    {
        return patches[patchID].Yield;
    }

    // OFFSET 0x02027DC0
    public static int BerryPatches_Harvest(BerryPatch[] patches, int patchID)
    {
        BerryPatch patch = patches[patchID];
        int yield = patch.Yield;
        BerryPatch_Clear(patch);
        return yield;
    }

    // OFFSET 0x02027DD4
    private static void BerryPatch_AdvanceStage(BerryPatch patch, byte[] growthData)
    {
        switch (patch.GrowthStage)
        {
            case StageNone:
                ErrorHandling.GF_AssertFail();
                break;
            case StagePlanted:
            case StageSprouted:
            case StageGrowing:
                patch.GrowthStage++;
                break;
            case StageBlooming:
                patch.Yield = (ushort)BerryPatch_CalcYield(patch, growthData);
                if (patch.Yield < MinimumYield)
                {
                    patch.Yield = MinimumYield;
                }
                patch.GrowthStage++;
                break;
            case StageFruit:
                patch.Yield = 0;
                patch.GrowthStage = StageSprouted;
                patch.YieldRating = FullYieldRating;
                patch.RegrowCount++;
                if (patch.RegrowCount == BerryPatch_RegrowLimit(patch))
                {
                    BerryPatch_Clear(patch);
                }
                break;
        }
    }

    // OFFSET 0x02027E40
    private static void BerryPatch_DrainMoisture(BerryPatch patch, byte[] growthData, int minutes)
    {
        if (patch.GrowthStage == StageFruit)
        {
            return;
        }
        int drain = BerryGrowthData_MoistureDrain(growthData, patch.BerryID, patch.Mulch);
        int total = minutes + patch.MinuteRemainder;
        int hours = total / MinutesPerHour;
        patch.MinuteRemainder = (ushort)(total % MinutesPerHour);
        if (hours == 0)
        {
            return;
        }
        if (patch.Moisture >= drain * hours)
        {
            patch.Moisture = (byte)(patch.Moisture - drain * hours);
            return;
        }
        if (patch.Moisture != 0)
        {
            hours -= (patch.Moisture + (drain - 1)) / drain;
            patch.Moisture = 0;
        }
        if (patch.YieldRating > hours)
        {
            patch.YieldRating = (byte)(patch.YieldRating - hours);
        }
        else
        {
            patch.YieldRating = 0;
        }
    }

    // OFFSET 0x02027EAC
    public static void BerryPatches_ElapseMinutes(BerryPatch[] patches, byte[] growthData, int minutes)
    {
        for (int i = 0; i < MaxPatches; i++)
        {
            BerryPatch patch = patches[i];
            if (patch.BerryID == 0 || patch.GrowthStage == StageNone || patch.GrowthActive == 0)
            {
                continue;
            }
            int stageMinutes = BerryGrowthData_StageMinutes(growthData, patch.BerryID, patch.Mulch);
            if (minutes >= BerryPatch_LifetimeStages(patch) * stageMinutes)
            {
                BerryPatch_Clear(patch);
                continue;
            }
            int remaining = minutes;
            while (patch.GrowthStage != StageNone && remaining != 0)
            {
                if (patch.StageMinutes > remaining)
                {
                    BerryPatch_DrainMoisture(patch, growthData, remaining);
                    patch.StageMinutes = (ushort)(patch.StageMinutes - remaining);
                    break;
                }
                BerryPatch_DrainMoisture(patch, growthData, patch.StageMinutes);
                BerryPatch_AdvanceStage(patch, growthData);
                remaining -= patch.StageMinutes;
                patch.StageMinutes = (ushort)BerryGrowthData_StageMinutes(growthData, patch.BerryID, patch.Mulch);
                if (patch.GrowthStage == StageFruit)
                {
                    patch.StageMinutes = (ushort)(patch.StageMinutes * BerryPatch_FruitMultiplier(patch));
                }
            }
        }
    }
}
