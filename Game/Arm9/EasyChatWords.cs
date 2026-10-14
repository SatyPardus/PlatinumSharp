using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class UnlockedEasyChatWords
{
    public uint UnlockedGreetingBits;
    public uint UnlockedToughWordBits;
}

public sealed class EasyChatWordLoader
{
    public int HeapID;
    public readonly MessageLoader?[] Loaders = new MessageLoader?[EasyChatWords.TextBankCount];
}

public static class EasyChatWords
{
    public const int TextBankCount = 11;
    public const ushort WordNone = 0xFFFF;
    public const uint NoToughWordUnlocked = 32;

    private const int MessageArchive = 0x1A;
    private const int WordIndexMask = 0xFFF;
    private const int ToughWordsBank = 9;
    private const int ToughWordCount = 32;
    private const int DefaultGreetingCount = 6;
    private const int GameLanguage = 2;
    private const int SaveTableEntry = 0x22;
    private const uint SaveSize = 8;

    private static ushort[]? sBankIDs;
    private static ushort[]? sBankSizes;
    private static byte[]? sDefaultGreetings;

    private static ushort[] BankIDs => sBankIDs ??= RomData.Arm9U16(0x020E550C, TextBankCount);

    private static ushort[] BankSizes => sBankSizes ??= RomData.Arm9U16(0x020E5522, TextBankCount);

    private static byte[] DefaultGreetings => sDefaultGreetings ??= RomData.Arm9(0x020E5500, DefaultGreetingCount * 2);

    // OFFSET 0x02014D38
    public static EasyChatWordLoader EasyChatWordLoader_New(int heapID)
    {
        EasyChatWordLoader wordLoader = new();
        for (int i = 0; i < TextBankCount; i++)
        {
            wordLoader.HeapID = heapID;
            wordLoader.Loaders[i] = MessageLoader.MessageLoader_Init(1, MessageArchive, BankIDs[i], heapID);
        }
        return wordLoader;
    }

    // OFFSET 0x02014D70
    public static void EasyChatWordLoader_Free(EasyChatWordLoader wordLoader)
    {
        for (int i = 0; i < TextBankCount; i++)
        {
            MessageLoader.MessageLoader_Free(wordLoader.Loaders[i]);
        }
    }

    // OFFSET 0x02014D90
    public static void EasyChatWordLoader_GetString(EasyChatWordLoader wordLoader, ushort word, GameString dst)
    {
        EasyChatWord_GetLoaderIndexAndEntry(word, out uint loaderIndex, out uint bankEntry);
        MessageLoader.MessageLoader_GetString(wordLoader.Loaders[loaderIndex]!, bankEntry, dst);
    }

    // OFFSET 0x02014DB8
    public static void EasyChatWord_ToString(ushort word, GameString dst)
    {
        if (word != WordNone)
        {
            EasyChatWord_GetLoaderIndexAndEntry(word, out uint loaderIndex, out uint bankEntry);
            uint bankID = BankIDs[loaderIndex];
            MessageBank.MessageBank_GetStringFromNARC(MessageArchive, bankID, bankEntry, 0, dst);
            return;
        }
        GameString.String_Clear(dst);
    }

    // OFFSET 0x02014DFC
    public static ushort EasyChatWord_FromBankAndEntry(uint bankID, uint bankEntry)
    {
        for (uint i = 0; i < TextBankCount; i++)
        {
            if (bankID == BankIDs[i])
            {
                ushort firstWord = 0;
                for (ushort j = 0; j < i; j++)
                {
                    firstWord += BankSizes[j];
                }
                return (ushort)(firstWord + bankEntry);
            }
        }
        return WordNone;
    }

    // OFFSET 0x02014E4C
    public static bool EasyChatWord_GetLoaderIndexAndEntry(ushort word, out uint loaderIndex, out uint bankEntry)
    {
        uint wordIndex = (uint)(word & WordIndexMask);
        uint total = 0;
        for (uint i = 0; i < TextBankCount; i++)
        {
            total += BankSizes[i];
            if (wordIndex < total)
            {
                loaderIndex = i;
                bankEntry = wordIndex - (total - BankSizes[i]);
                return true;
            }
        }
        loaderIndex = 0;
        bankEntry = 0;
        return false;
    }

    // OFFSET 0x02014E8C
    public static uint UnlockedEasyChatWords_SaveSize()
    {
        return SaveSize;
    }

    // OFFSET 0x02014E90
    public static void UnlockedEasyChatWords_Init(UnlockedEasyChatWords unlockedWords)
    {
        unlockedWords.UnlockedGreetingBits = 0;
        unlockedWords.UnlockedToughWordBits = 0;
        byte[] defaults = DefaultGreetings;
        for (uint i = 0; i < DefaultGreetingCount; i++)
        {
            if (defaults[i * 2] == GameLanguage)
            {
                EasyChatWords_UnlockGreeting(unlockedWords, defaults[i * 2 + 1]);
                break;
            }
        }
        SaveData.SaveData_SetChecksum(SaveTableEntry);
    }

    // OFFSET 0x02014EC4
    public static UnlockedEasyChatWords SaveData_GetUnlockedEasyChatWords(SaveData saveData)
    {
        SaveData.SaveData_Checksum(SaveTableEntry);
        return (UnlockedEasyChatWords)SaveData.SaveData_SaveTable(saveData, SaveTableEntry);
    }

    // OFFSET 0x02014ED8
    public static bool EasyChatWords_IsToughWordUnlocked(UnlockedEasyChatWords unlockedWords, int bankEntry)
    {
        return ((unlockedWords.UnlockedToughWordBits >> bankEntry) & 1) != 0;
    }

    // OFFSET 0x02014EE4
    public static uint EasyChatWords_TryUnlockRandomToughWord(UnlockedEasyChatWords unlockedWords)
    {
        uint lockedCount = 0;
        for (int i = 0; i < ToughWordCount; i++)
        {
            if (((unlockedWords.UnlockedToughWordBits >> i) & 1) == 0)
            {
                lockedCount++;
            }
        }
        if (lockedCount != 0)
        {
            uint skip = MathUtil.LCRNG_Next() % lockedCount;
            for (int i = 0; i < ToughWordCount; i++)
            {
                if (((unlockedWords.UnlockedToughWordBits >> i) & 1) == 0)
                {
                    if (skip == 0)
                    {
                        unlockedWords.UnlockedToughWordBits |= 1u << i;
                        SaveData.SaveData_SetChecksum(SaveTableEntry);
                        return (uint)i;
                    }
                    skip--;
                }
            }
        }
        SaveData.SaveData_SetChecksum(SaveTableEntry);
        return NoToughWordUnlocked;
    }

    // OFFSET 0x02014F48
    public static bool EasyChatWords_AreAllToughWordsUnlocked(UnlockedEasyChatWords unlockedWords)
    {
        for (int i = 0; i < ToughWordCount; i++)
        {
            if (((unlockedWords.UnlockedToughWordBits >> i) & 1) == 0)
            {
                return false;
            }
        }
        return true;
    }

    // OFFSET 0x02014F64
    public static ushort EasyChatWords_GetToughWordFromBankEntry(uint bankEntry)
    {
        ushort firstWord = 0;
        for (int i = 0; i < ToughWordsBank; i++)
        {
            firstWord += BankSizes[i];
        }
        return (ushort)(firstWord + bankEntry);
    }

    // OFFSET 0x02014F8C
    public static bool EasyChatWords_IsGreetingUnlocked(UnlockedEasyChatWords unlockedWords, int bankEntry)
    {
        return ((unlockedWords.UnlockedGreetingBits >> bankEntry) & 1) != 0;
    }

    // OFFSET 0x02014F98
    public static void EasyChatWords_UnlockGreeting(UnlockedEasyChatWords unlockedWords, int bankEntry)
    {
        unlockedWords.UnlockedGreetingBits |= 1u << bankEntry;
        SaveData.SaveData_SetChecksum(SaveTableEntry);
    }
}
