using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class EasyChatSentence
{
    public const int MaxWordSlots = 2;
    public const ushort TypeNone = 0xFFFF;
    public const int TypePreBattle = 0;
    public const int TypeWin = 1;
    public const int TypeLoss = 2;
    public const int TypeGeneral = 3;
    public const int TypeUnionRoom = 4;
    public const int TypeCount = 5;
    public const ushort WordNone = 0xFFFF;

    private const int MessageArchive = 0x1A;
    private const int EntriesPerType = 0x14;
    private const int FrontierDefaultCount = 4;
    private const int FrontierDefaultSize = 10;
    private const int EnteredUnionRoomID = 5;
    private const ushort EndOfString = 0xFFFF;

    public ushort Type;
    public ushort ID;
    public readonly ushort[] Words = new ushort[MaxWordSlots];

    private static ushort[]? sBankIDs;
    private static byte[]? sFrontierDefaults;

    private static ushort[] BankIDs => sBankIDs ??= RomData.Arm9U16(0x020E5498, TypeCount);

    private static byte[] FrontierDefaults => sFrontierDefaults ??= RomData.Arm9(0x020E54A2, FrontierDefaultCount * FrontierDefaultSize);

    // OFFSET 0x02014A84
    public static void EasyChatSentence_Init(EasyChatSentence sentence)
    {
        sentence.Type = TypeNone;
        for (int i = 0; i < MaxWordSlots; i++)
        {
            sentence.Words[i] = WordNone;
        }
    }

    // OFFSET 0x02014A9C
    public static void EasyChatSentence_InitWithType(EasyChatSentence sentence, uint type)
    {
        sentence.Type = (ushort)type;
        sentence.ID = 0;
        for (int i = 0; i < MaxWordSlots; i++)
        {
            sentence.Words[i] = WordNone;
        }
    }

    // OFFSET 0x02014AB4
    public static void EasyChatSentence_InitWithEnteredUnionRoom(EasyChatSentence sentence)
    {
        EasyChatSentence_InitWithType(sentence, TypeUnionRoom);
        sentence.ID = EnteredUnionRoomID;
    }

    // OFFSET 0x02014AC4
    public static void EasyChatSentence_SetDefaultFrontierFields(EasyChatSentence sentence, int index)
    {
        if ((uint)index >= FrontierDefaultCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((uint)index < FrontierDefaultCount)
        {
            byte[] defaults = FrontierDefaults;
            int entry = index * FrontierDefaultSize;
            EasyChatSentence_InitWithType(sentence, defaults[entry]);
            sentence.ID = defaults[entry + 1];
            short firstBank = (short)Bin.U16(defaults, entry + 2);
            if (firstBank != -1)
            {
                sentence.Words[0] = EasyChatWords.EasyChatWord_FromBankAndEntry((uint)firstBank, (uint)Bin.U16(defaults, entry + 4));
            }
            short secondBank = (short)Bin.U16(defaults, entry + 6);
            if (secondBank != -1)
            {
                sentence.Words[1] = EasyChatWords.EasyChatWord_FromBankAndEntry((uint)secondBank, (uint)Bin.U16(defaults, entry + 8));
            }
        }
    }

    // OFFSET 0x02014B34
    public static GameString? EasyChatSentence_ToString(EasyChatSentence sentence, int heapID)
    {
        StringTemplate template = StringTemplate.StringTemplate_Default(heapID);
        for (int i = 0; i < MaxWordSlots; i++)
        {
            if (sentence.Words[i] == WordNone)
            {
                break;
            }
            StringTemplate.StringTemplate_SetEasyChatWord(template, (uint)i, sentence.Words[i]);
        }
        MessageLoader loader = MessageLoader.MessageLoader_Init(1, MessageArchive, BankIDs[sentence.Type], heapID)!;
        GameString? result = MessageUtil.MessageUtil_ExpandedString(template, loader, sentence.ID, heapID);
        MessageLoader.MessageLoader_Free(loader);
        StringTemplate.StringTemplate_Free(template);
        return result;
    }

    // OFFSET 0x02014BA0
    public static GameString? EasyChatSentence_GetTemplateString(EasyChatSentence sentence, int heapID)
    {
        return MessageBank.MessageBank_GetNewStringFromNARC(MessageArchive, BankIDs[sentence.Type], sentence.ID, heapID);
    }

    // OFFSET 0x02014BBC
    public static bool EasyChatSentence_IsValid(EasyChatSentence sentence)
    {
        return sentence.Type != TypeNone;
    }

    // OFFSET 0x02014BD0
    public static bool EasyChatSentence_IsComplete(EasyChatSentence sentence)
    {
        uint slotCount = EasyChatSentence_CountWordSlots(sentence.Type, sentence.ID);
        for (uint i = 0; i < slotCount; i++)
        {
            if (sentence.Words[i] == WordNone)
            {
                return false;
            }
        }
        return true;
    }

    // OFFSET 0x02014C00
    private static uint EasyChatSentence_CountWordSlots(uint type, uint id)
    {
        if (type >= TypeCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (id >= EasyChatSentence_GetBankEntryCountForType(type))
        {
            ErrorHandling.GF_AssertFail();
        }
        GameString? str = MessageBank.MessageBank_GetNewStringFromNARC(MessageArchive, BankIDs[type], id, 0);
        ushort[] data = GameString.String_GetData(str);
        int pos = 0;
        uint count = 0;
        while (data[pos] != EndOfString)
        {
            if (data[pos] == CharCodeFormat.FormatArg)
            {
                if (CharCodeFormat.CharCode_IsFormatArg(data, pos))
                {
                    count++;
                }
                pos = CharCodeFormat.CharCode_SkipFormatArg(data, pos);
            }
            else
            {
                pos++;
            }
        }
        GameString.String_Free(str);
        return count;
    }

    // OFFSET 0x02014C78
    public static ushort EasyChatSentence_GetWord(EasyChatSentence sentence, int slot)
    {
        return sentence.Words[slot];
    }

    // OFFSET 0x02014C80
    public static uint EasyChatSentence_GetType(EasyChatSentence sentence)
    {
        return sentence.Type;
    }

    // OFFSET 0x02014C84
    public static uint EasyChatSentence_GetID(EasyChatSentence sentence)
    {
        return sentence.ID;
    }

    // OFFSET 0x02014C88
    public static bool EasyChatSentence_Compare(EasyChatSentence sentence1, EasyChatSentence sentence2)
    {
        if (sentence1.Type != sentence2.Type || sentence1.ID != sentence2.ID)
        {
            return false;
        }
        for (int i = 0; i < MaxWordSlots; i++)
        {
            if (sentence1.Words[i] != sentence2.Words[i])
            {
                return false;
            }
        }
        return true;
    }

    // OFFSET 0x02014CC0
    public static void EasyChatSentence_Copy(EasyChatSentence dest, EasyChatSentence src)
    {
        dest.Type = src.Type;
        dest.ID = src.ID;
        dest.Words[0] = src.Words[0];
        dest.Words[1] = src.Words[1];
    }

    // OFFSET 0x02014CD4
    public static uint EasyChatSentence_GetBankEntryCountForType(uint type)
    {
        if (type < TypeCount)
        {
            return EntriesPerType;
        }
        return 0;
    }

    // OFFSET 0x02014CE0
    public static void EasyChatSentence_SetTypeAndID(EasyChatSentence sentence, uint type, uint id)
    {
        if (type >= TypeCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        sentence.Type = (ushort)type;
        sentence.ID = (ushort)id;
    }

    // OFFSET 0x02014CF8
    public static void EasyChatSentence_SetWord(EasyChatSentence sentence, uint index, ushort word)
    {
        if (index >= MaxWordSlots)
        {
            ErrorHandling.GF_AssertFail();
        }
        sentence.Words[index] = word;
    }

    // OFFSET 0x02014D10
    public static void EasyChatSentence_ClearUnusedWords(EasyChatSentence sentence)
    {
        for (uint i = EasyChatSentence_CountWordSlots(sentence.Type, sentence.ID); i < MaxWordSlots; i++)
        {
            sentence.Words[i] = WordNone;
        }
    }
}
