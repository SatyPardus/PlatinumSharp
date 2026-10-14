using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class MigratedPokemon : ISaveBlock
{
    public const int Size = 0x688;
    public const int TransferMonCount = 6;
    public const int HistoryCount = 20;
    public const int MacAddressLength = 6;

    private const int TrainerIDsOffset = 0x588;
    private const int TimestampsOffset = 0x5D8;
    private const int RtcOffsetOffset = 0x678;
    private const int MacAddressOffset = 0x680;
    private const int MonDataSpeciesExists = 0xAC;
    private const long OldestSearchStart = 0x7FFFFFFF;

    public readonly Pokemon[] Mons = new Pokemon[TransferMonCount];
    public readonly uint[] GbaTrainerIDs = new uint[HistoryCount];
    public readonly long[] Timestamps = new long[HistoryCount];
    public long RtcOffset;
    public readonly byte[] MacAddress = new byte[MacAddressLength];

    public MigratedPokemon()
    {
        for (int i = 0; i < TransferMonCount; i++)
        {
            Mons[i] = new Pokemon();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < TransferMonCount; i++)
        {
            Array.Copy(Mons[i].Data, 0, body, offset + i * Pokemon.Size, Pokemon.Size);
        }
        for (int i = 0; i < HistoryCount; i++)
        {
            Bin.W32(body, offset + TrainerIDsOffset + i * 4, GbaTrainerIDs[i]);
            Bin.W32(body, offset + TimestampsOffset + i * 8, (uint)Timestamps[i]);
            Bin.W32(body, offset + TimestampsOffset + i * 8 + 4, (uint)(Timestamps[i] >> 32));
        }
        Bin.W32(body, offset + RtcOffsetOffset, (uint)RtcOffset);
        Bin.W32(body, offset + RtcOffsetOffset + 4, (uint)(RtcOffset >> 32));
        Array.Copy(MacAddress, 0, body, offset + MacAddressOffset, MacAddressLength);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < TransferMonCount; i++)
        {
            Array.Copy(body, offset + i * Pokemon.Size, Mons[i].Data, 0, Pokemon.Size);
        }
        for (int i = 0; i < HistoryCount; i++)
        {
            GbaTrainerIDs[i] = Bin.U32(body, offset + TrainerIDsOffset + i * 4);
            Timestamps[i] = Bin.U32(body, offset + TimestampsOffset + i * 8) | ((long)Bin.U32(body, offset + TimestampsOffset + i * 8 + 4) << 32);
        }
        RtcOffset = Bin.U32(body, offset + RtcOffsetOffset) | ((long)Bin.U32(body, offset + RtcOffsetOffset + 4) << 32);
        Array.Copy(body, offset + MacAddressOffset, MacAddress, 0, MacAddressLength);
    }

    private static void InitTransferMons(MigratedPokemon transferData)
    {
        for (int i = 0; i < TransferMonCount; i++)
        {
            Pokemon.Pokemon_Init(transferData.Mons[i]);
            if (Pokemon.Pokemon_GetValue(transferData.Mons[i], MonDataSpeciesExists, null) != 0)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
    }

    // OFFSET 0x0202EEC8
    public static void PalParkTransfer_Init(MigratedPokemon transferData)
    {
        transferData.ReadFrom(new byte[Size], 0);
        InitTransferMons(transferData);
    }

    // OFFSET 0x0202EF04
    public static void ClearPalParkTransferPokemonData(MigratedPokemon transferData)
    {
        for (int i = 0; i < TransferMonCount; i++)
        {
            Array.Clear(transferData.Mons[i].Data);
        }
        InitTransferMons(transferData);
    }

    // OFFSET 0x0202EF40
    private static int PalParkTransfer_FindHistorySlot(MigratedPokemon transferData, uint gbaTrainerId)
    {
        for (int i = 0; i < HistoryCount; i++)
        {
            if (gbaTrainerId == transferData.GbaTrainerIDs[i])
            {
                return i;
            }
        }
        for (int i = 0; i < HistoryCount; i++)
        {
            if (transferData.GbaTrainerIDs[i] == 0)
            {
                return i;
            }
        }
        int oldest = 0;
        long oldestTime = OldestSearchStart;
        for (int i = 0; i < HistoryCount; i++)
        {
            if (transferData.Timestamps[i] < oldestTime)
            {
                oldestTime = transferData.Timestamps[i];
                oldest = i;
            }
        }
        return oldest;
    }

    // OFFSET 0x0202EFA4
    public static void BoxMonToTransferData(MigratedPokemon transferData, BoxPokemon boxMon, int slot)
    {
        Pokemon.Pokemon_FromBoxPokemon(boxMon, transferData.Mons[slot]);
    }

    // OFFSET 0x0202EFB8
    public static void PalParkTransfer_SaveTransferHistory(MigratedPokemon transferData, uint gbaTrainerId)
    {
        int slot = PalParkTransfer_FindHistorySlot(transferData, gbaTrainerId);
        transferData.GbaTrainerIDs[slot] = gbaTrainerId;
        transferData.Timestamps[slot] = Rtc.GetTimestamp();
        transferData.RtcOffset = Platform.OwnerRtcOffset;
        Array.Copy(Platform.MacAddress, transferData.MacAddress, MacAddressLength);
    }

    // OFFSET 0x0202F000
    public static void MigratedPokemon_ConvertToPokemon(MigratedPokemon transferData, int slot, Pokemon mon)
    {
        if (slot >= TransferMonCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        Array.Copy(transferData.Mons[slot].Data, mon.Data, Pokemon.Size);
    }

    // OFFSET 0x0202F028
    public static int GetPalParkTransferMonCount(MigratedPokemon transferData)
    {
        int count = 0;
        for (int i = 0; i < TransferMonCount; i++)
        {
            if (Pokemon.Pokemon_GetValue(transferData.Mons[i], MonDataSpeciesExists, null) != 0)
            {
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x0202F050
    public static int PalParkTransfer_GetSecondsSinceLastTransfer(MigratedPokemon transferData, uint gbaTrainerId)
    {
        long now = Rtc.GetTimestamp();
        for (int i = 0; i < HistoryCount; i++)
        {
            if (gbaTrainerId == transferData.GbaTrainerIDs[i])
            {
                return (int)((uint)now - (uint)transferData.Timestamps[i]);
            }
        }
        return 0;
    }

    // OFFSET 0x0202F088
    public static bool MacAddressMatchesLastPalParkTransfer(MigratedPokemon transferData)
    {
        byte[] macAddress = Platform.MacAddress;
        for (int i = 0; i < MacAddressLength; i++)
        {
            if (macAddress[i] != transferData.MacAddress[i])
            {
                return false;
            }
        }
        return true;
    }

    // OFFSET 0x0202F0BC
    public static bool RtcOffsetMatchesLastPalParkTransfer(MigratedPokemon transferData)
    {
        if (Platform.OwnerRtcOffset == transferData.RtcOffset)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202F0E0
    public static bool IsPalParkTransferMacAddressUnset(MigratedPokemon transferData)
    {
        byte combined = 0;
        for (int i = 0; i < MacAddressLength; i++)
        {
            combined |= transferData.MacAddress[i];
        }
        if (combined == 0)
        {
            return true;
        }
        return false;
    }
}

public sealed class LinkContestRecords : ISaveBlock
{
    public const int Size = 0x28;
    public const int ContestTypeCount = 5;
    public const int PlacementCount = 4;
    public const int MaxRecord = 9999;

    public readonly ushort[] Records = new ushort[ContestTypeCount * PlacementCount];

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < Records.Length; i++)
        {
            Bin.W16(body, offset + i * 2, Records[i]);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < Records.Length; i++)
        {
            Records[i] = (ushort)Bin.U16(body, offset + i * 2);
        }
    }

    // OFFSET 0x0202F10C
    public static void LinkContestRecords_Init(LinkContestRecords linkContestRecords)
    {
        for (int type = 0; type < ContestTypeCount; type++)
        {
            for (int placement = 0; placement < PlacementCount; placement++)
            {
                linkContestRecords.Records[type * PlacementCount + placement] = 0;
            }
        }
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryLinkContestRecords);
    }

    // OFFSET 0x0202F134
    public static void LinkContestRecords_IncrementSaveData(SaveData saveData, int contestType, int placement)
    {
        LinkContestRecords records = (LinkContestRecords)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryLinkContestRecords);
        if (records.Records[contestType * PlacementCount + placement] < MaxRecord)
        {
            records.Records[contestType * PlacementCount + placement]++;
        }
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryLinkContestRecords);
    }

    // OFFSET 0x0202F160
    public static ushort LinkContestRecords_GetSaveData(SaveData saveData, int contestType, int placement)
    {
        LinkContestRecords records = (LinkContestRecords)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryLinkContestRecords);
        SaveData.SaveData_Checksum(SaveDataTable.EntryLinkContestRecords);
        return records.Records[contestType * PlacementCount + placement];
    }
}

public static class CatchRecords
{
    private const int MonDataSpecies = 5;
    private const int MonDataIsEgg = 0x4C;

    // OFFSET 0x0202F180
    public static void SaveData_UpdateCatchRecords(SaveData saveData, Pokemon mon)
    {
        if (Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) != 0)
        {
            return;
        }
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(saveData);
        GameRecords records = GameRecords.SaveData_GetGameRecords(saveData);
        ushort species = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
        GameRecords.GameRecords_IncrementTrainerScoreOnCatch(records, pokedex, species);
        Pokedex.Pokedex_Capture(pokedex, mon);
        Poketch.Poketch_PokemonHistoryEnqueue(FieldPoketch.SaveData_GetPoketch(saveData), mon.Box);
    }
}
