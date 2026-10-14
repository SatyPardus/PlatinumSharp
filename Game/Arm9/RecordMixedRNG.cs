using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class RecordMixedRNGEntry
{
    public const int Size = 0x2C;
    public const int NameLength = 8;

    private const int PlayerNameOffset = 0x10;

    public readonly ushort[] GroupName = new ushort[NameLength];
    public readonly ushort[] PlayerName = new ushort[NameLength];
    public byte Gender;
    public byte Language;
    public uint Seed;
    public uint Rand;

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < NameLength; i++)
        {
            Bin.W16(body, offset + i * 2, GroupName[i]);
            Bin.W16(body, offset + PlayerNameOffset + i * 2, PlayerName[i]);
        }
        body[offset + 0x20] = Gender;
        body[offset + 0x21] = Language;
        Bin.W32(body, offset + 0x24, Seed);
        Bin.W32(body, offset + 0x28, Rand);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < NameLength; i++)
        {
            GroupName[i] = (ushort)Bin.U16(body, offset + i * 2);
            PlayerName[i] = (ushort)Bin.U16(body, offset + PlayerNameOffset + i * 2);
        }
        Gender = body[offset + 0x20];
        Language = body[offset + 0x21];
        Seed = Bin.U32(body, offset + 0x24);
        Rand = Bin.U32(body, offset + 0x28);
    }
}

public sealed class RecordMixedRNG : ISaveBlock
{
    public const int EntryCount = 6;
    public const int EntryOwn = 0;
    public const int EntryActive = 1;
    public const int NameGroup = 0;
    public const int NamePlayer = 1;

    private const ushort EndOfString = 0xFFFF;

    public readonly RecordMixedRNGEntry[] Entries = new RecordMixedRNGEntry[EntryCount];

    public static RecordMixedRNG? sLastInitialised;

    public RecordMixedRNG()
    {
        for (int i = 0; i < EntryCount; i++)
        {
            Entries[i] = new RecordMixedRNGEntry();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < EntryCount; i++)
        {
            Entries[i].WriteTo(body, offset + i * RecordMixedRNGEntry.Size);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < EntryCount; i++)
        {
            Entries[i].ReadFrom(body, offset + i * RecordMixedRNGEntry.Size);
        }
    }

    // OFFSET 0x0202B384
    public static void RecordMixedRNG_CopyEntry(RecordMixedRNG rng, int source, int destination)
    {
        byte[] buffer = new byte[RecordMixedRNGEntry.Size];
        rng.Entries[source].WriteTo(buffer, 0);
        rng.Entries[destination].ReadFrom(buffer, 0);
    }

    // OFFSET 0x0202B3A8
    public static void RecordMixedRNG_Init(RecordMixedRNG rng)
    {
        for (int i = 0; i < EntryCount; i++)
        {
            rng.Entries[i].ReadFrom(new byte[RecordMixedRNGEntry.Size], 0);
            rng.Entries[i].GroupName[0] = EndOfString;
            rng.Entries[i].PlayerName[0] = EndOfString;
        }
        sLastInitialised = rng;
    }

    // OFFSET 0x0202B3D8
    public static void RecordMixedRNG_AdvanceEntries(RecordMixedRNG rng, uint steps)
    {
        for (uint i = 0; i < EntryCount; i++)
        {
            for (uint step = 0; step < steps; step++)
            {
                rng.Entries[i].Rand = MathUtil.ARNG_Next(rng.Entries[i].Rand);
            }
        }
    }

    // OFFSET 0x0202B400
    public static uint RecordMixedRNG_GetEntrySeed(RecordMixedRNG rng, int entry)
    {
        return rng.Entries[entry].Seed;
    }

    // OFFSET 0x0202B40C
    public static void RecordMixedRNG_SetEntrySeed(RecordMixedRNG rng, int entry, uint seed)
    {
        rng.Entries[entry].Seed = seed;
        rng.Entries[entry].Rand = MathUtil.ARNG_Next(seed);
    }

    // OFFSET 0x0202B428
    public static uint RecordMixedRNG_GetRand(RecordMixedRNG rng)
    {
        return rng.Entries[EntryActive].Rand;
    }

    // OFFSET 0x0202B42C
    public static ushort[] RecordMixedRNG_GetEntryName(RecordMixedRNG rng, int entry, int nameType)
    {
        if (nameType == NameGroup)
        {
            return rng.Entries[entry].GroupName;
        }
        return rng.Entries[entry].PlayerName;
    }

    // OFFSET 0x0202B444
    public static void RecordMixedRNG_SetEntryName(RecordMixedRNG rng, int entry, int nameType, GameString name)
    {
        if (nameType == NameGroup)
        {
            GameString.String_ToChars(name, rng.Entries[entry].GroupName, RecordMixedRNGEntry.NameLength);
            return;
        }
        GameString.String_ToChars(name, rng.Entries[entry].PlayerName, RecordMixedRNGEntry.NameLength);
    }

    // OFFSET 0x0202B470
    public static void RecordMixedRNG_SetEntryGender(RecordMixedRNG rng, int entry, int gender)
    {
        rng.Entries[entry].Gender = (byte)gender;
    }

    // OFFSET 0x0202B47C
    public static int RecordMixedRNG_GetEntryGender(RecordMixedRNG rng, int entry)
    {
        return rng.Entries[entry].Gender;
    }

    // OFFSET 0x0202B488
    public static int RecordMixedRNG_GetEntryLanguage(RecordMixedRNG rng, int entry)
    {
        return rng.Entries[entry].Language;
    }

    // OFFSET 0x0202B494
    public static void RecordMixedRNG_SetEntryLanguage(RecordMixedRNG rng, int entry, int language)
    {
        rng.Entries[entry].Language = (byte)language;
    }

    // OFFSET 0x0202B4A0
    public static RecordMixedRNG SaveData_GetRecordMixedRNG(SaveData saveData)
    {
        return (RecordMixedRNG)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryRecordMixedRNG);
    }

    // OFFSET 0x0202B4AC
    public static bool RecordMixedRNG_IsEntryValid(RecordMixedRNG rng, int entry)
    {
        if (!RecordMixedRNG_IsEntryEmpty(rng.Entries[entry]))
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202B4C4
    public static bool RecordMixedRNG_IsEntryEqualToActive(RecordMixedRNG rng, int entry)
    {
        return RecordMixedRNG_AreEntriesEqual(rng.Entries[EntryActive], rng.Entries[entry]);
    }

    // OFFSET 0x0202B4D8
    public static bool RecordMixedRNG_DoesGroupNameExist(RecordMixedRNG rng, ushort[] name)
    {
        if (name[0] == EndOfString)
        {
            return false;
        }
        for (int i = 0; i < EntryCount; i++)
        {
            if (CharCode.CharCode_CompareNumChars(name, 0, rng.Entries[i].GroupName, 0, RecordMixedRNGEntry.NameLength) == 0)
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0202B510
    private static bool RecordMixedRNG_IsEntryEmpty(RecordMixedRNGEntry entry)
    {
        if (entry.GroupName[0] == EndOfString)
        {
            return true;
        }
        if (entry.PlayerName[0] == EndOfString)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202B530
    private static bool RecordMixedRNG_AreEntriesEqual(RecordMixedRNGEntry first, RecordMixedRNGEntry second)
    {
        if (CharCode.CharCode_CompareNumChars(first.PlayerName, 0, second.PlayerName, 0, RecordMixedRNGEntry.NameLength) != 0)
        {
            return false;
        }
        if (CharCode.CharCode_CompareNumChars(first.GroupName, 0, second.GroupName, 0, RecordMixedRNGEntry.NameLength) != 0)
        {
            return false;
        }
        if (first.Gender != second.Gender)
        {
            return false;
        }
        if (first.Language != second.Language)
        {
            return false;
        }
        if (first.Seed == second.Seed)
        {
            return true;
        }
        return false;
    }
}
