using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class GameRecords : ISaveBlock
{
    public const int Size = 0x1BC;
    public const int LargeRecordCount = 71;
    public const int RecordCount = 148;
    public const int RecordTrainerScore = 1;
    public const int TrainerScoreCount = 51;
    public const int TrainerScoreCaughtNewSpecies = 22;
    public const uint MaxTrainerScore = 99999999;

    private const int SmallRecordsOffset = 0x11C;
    private const int EncryptedOffset = 4;
    private const int EncryptedSize = 0x1B4;
    private const int ChecksumOffset = 0x1B8;
    private const int KeyOffset = 0x1BA;
    private const uint LargeLimit = 999999;
    private const uint LargeExtendedLimit = 999999999;
    private const uint SmallLimit = 9999;
    private const uint SmallExtendedLimit = 0xFFFF;
    private const uint ExtendedLimitTable = 0x02100984;
    private const uint TrainerScoreTable = 0x020E5C68;

    public readonly byte[] Raw = new byte[Size];

    private static byte[]? sExtendedLimits;
    private static ushort[]? sTrainerScores;

    public void WriteTo(byte[] body, int offset)
    {
        Array.Copy(Raw, 0, body, offset, Size);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Array.Copy(body, offset, Raw, 0, Size);
    }

    private uint Seed()
    {
        return (uint)Bin.U16(Raw, ChecksumOffset) + ((uint)Bin.U16(Raw, KeyOffset) << 16);
    }

    // OFFSET 0x0202CD58
    public static void GameRecords_Init(GameRecords records)
    {
        Array.Clear(records.Raw);
        uint count = Interrupts.VBlankCount;
        Bin.W16(records.Raw, KeyOffset, (ushort)(count | (count << 8)));
        GameRecords_Encrypt(records, RecordTrainerScore);
    }

    // OFFSET 0x0202CD88
    public static GameRecords SaveData_GetGameRecords(SaveData saveData)
    {
        return (GameRecords)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryGameRecords);
    }

    // OFFSET 0x0202CD94
    private static void GameRecords_Encrypt(GameRecords records, int id)
    {
        if (id == 0)
        {
            return;
        }
        byte[] data = new byte[EncryptedSize];
        Array.Copy(records.Raw, EncryptedOffset, data, 0, EncryptedSize);
        Bin.W16(records.Raw, ChecksumOffset, (ushort)MathUtil.SumBytes(data, EncryptedSize));
        MathUtil.EncodeData(data, EncryptedSize, records.Seed());
        Array.Copy(data, 0, records.Raw, EncryptedOffset, EncryptedSize);
    }

    // OFFSET 0x0202CDC0
    private static void GameRecords_Decrypt(GameRecords records, int id)
    {
        if (id == 0)
        {
            return;
        }
        byte[] data = new byte[EncryptedSize];
        Array.Copy(records.Raw, EncryptedOffset, data, 0, EncryptedSize);
        MathUtil.DecodeData(data, EncryptedSize, records.Seed());
        Array.Copy(data, 0, records.Raw, EncryptedOffset, EncryptedSize);
    }

    // OFFSET 0x0202CDE0
    private static uint GameRecords_ReadValue(GameRecords records, int id)
    {
        if (id < LargeRecordCount)
        {
            return Bin.U32(records.Raw, id * 4);
        }
        if (id < RecordCount)
        {
            return (uint)Bin.U16(records.Raw, SmallRecordsOffset + (id - LargeRecordCount) * 2);
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x0202CE08
    private static uint GameRecords_WriteValue(GameRecords records, int id, uint value)
    {
        if (id < LargeRecordCount)
        {
            Bin.W32(records.Raw, id * 4, value);
        }
        else if (id < RecordCount)
        {
            Bin.W16(records.Raw, SmallRecordsOffset + (id - LargeRecordCount) * 2, (ushort)value);
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        return GameRecords_ReadValue(records, id);
    }

    // OFFSET 0x0202CE3C
    private static uint GameRecords_Limit(int id)
    {
        sExtendedLimits ??= RomData.Arm9(ExtendedLimitTable, RecordCount);
        if (id < LargeRecordCount)
        {
            if (sExtendedLimits[id] != 0)
            {
                return LargeExtendedLimit;
            }
            return LargeLimit;
        }
        if (id < RecordCount)
        {
            if (sExtendedLimits[id] != 0)
            {
                return SmallExtendedLimit;
            }
            return SmallLimit;
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x0202CE84
    private static ushort GameRecords_TrainerScoreValue(int scoreID)
    {
        sTrainerScores ??= RomData.Arm9U16(TrainerScoreTable, TrainerScoreCount);
        return sTrainerScores[scoreID];
    }

    // OFFSET 0x0202CE90
    public static uint GameRecords_SetRecordValue(GameRecords records, int id, uint value)
    {
        uint limit = GameRecords_Limit(id);
        uint result;
        GameRecords_Decrypt(records, id);
        if (value < limit)
        {
            result = GameRecords_WriteValue(records, id, value);
        }
        else
        {
            result = GameRecords_WriteValue(records, id, limit);
        }
        GameRecords_Encrypt(records, id);
        return result;
    }

    // OFFSET 0x0202CED0
    public static uint GameRecords_SetAndLimitRecordValue(GameRecords records, int id, uint value)
    {
        uint limit = GameRecords_Limit(id);
        GameRecords_Decrypt(records, id);
        uint result = GameRecords_ReadValue(records, id);
        if (value > limit)
        {
            value = limit;
        }
        if (result < value)
        {
            result = GameRecords_WriteValue(records, id, value);
        }
        else if (result > limit)
        {
            result = GameRecords_WriteValue(records, id, limit);
        }
        GameRecords_Encrypt(records, id);
        return result;
    }

    // OFFSET 0x0202CF28
    public static uint GameRecords_IncrementRecordValue(GameRecords records, int id)
    {
        uint limit = GameRecords_Limit(id);
        uint result;
        GameRecords_Decrypt(records, id);
        uint value = GameRecords_ReadValue(records, id) + 1;
        if (value < limit)
        {
            result = GameRecords_WriteValue(records, id, value);
        }
        else
        {
            result = GameRecords_WriteValue(records, id, limit);
        }
        GameRecords_Encrypt(records, id);
        return result;
    }

    // OFFSET 0x0202CF70
    public static uint GameRecords_AddToRecordValue(GameRecords records, int id, uint toAdd)
    {
        uint limit = GameRecords_Limit(id);
        uint result;
        GameRecords_Decrypt(records, id);
        uint value = GameRecords_ReadValue(records, id) + toAdd;
        if (value < limit)
        {
            result = GameRecords_WriteValue(records, id, value);
        }
        else
        {
            result = GameRecords_WriteValue(records, id, limit);
        }
        GameRecords_Encrypt(records, id);
        return result;
    }

    // OFFSET 0x0202CFB8
    public static uint GameRecords_GetRecordValue(GameRecords records, int id)
    {
        uint limit = GameRecords_Limit(id);
        GameRecords_Decrypt(records, id);
        uint value = GameRecords_ReadValue(records, id);
        GameRecords_Encrypt(records, id);
        if (value > limit)
        {
            return limit;
        }
        return value;
    }

    // OFFSET 0x0202CFEC
    public static void GameRecords_IncrementTrainerScore(GameRecords records, int scoreID)
    {
        if (scoreID >= TrainerScoreCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        uint score = GameRecords_GetRecordValue(records, RecordTrainerScore);
        if (score + GameRecords_TrainerScoreValue(scoreID) > MaxTrainerScore)
        {
            GameRecords_SetRecordValue(records, RecordTrainerScore, MaxTrainerScore);
        }
        else
        {
            GameRecords_AddToRecordValue(records, RecordTrainerScore, GameRecords_TrainerScoreValue(scoreID));
        }
    }

    // OFFSET 0x0202D034
    public static uint GameRecords_GetTrainerScore(GameRecords records)
    {
        return GameRecords_GetRecordValue(records, RecordTrainerScore);
    }

    // OFFSET 0x0202D040
    public static void GameRecords_IncrementTrainerScoreOnCatch(GameRecords records, Pokedex pokedex, ushort species)
    {
        if (!Pokedex.Pokedex_HasCaughtSpecies(pokedex, species))
        {
            GameRecords_IncrementTrainerScore(records, TrainerScoreCaughtNewSpecies);
        }
    }
}
