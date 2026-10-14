using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class MessageBank
{
    private const int HeaderSize = 4;
    private const int EntrySize = 8;

    private static uint EntryKey(int seed, uint entryID)
    {
        ushort key = (ushort)((uint)seed * 0x2FD * (entryID + 1));
        return key | ((uint)key << 16);
    }

    private static void DecodeString(ushort[] text, uint entryID, uint length)
    {
        ushort key = (ushort)(0x91BD3 * (entryID + 1));
        for (uint i = 0; i < length; i++)
        {
            text[i] ^= key;
            key += 0x493D;
        }
    }

    private static ushort[] ReadChars(NARC narc, uint bankID, uint offset, uint length)
    {
        byte[] raw = new byte[length * 2];
        NARC.NARC_ReadFromMember(narc, bankID, offset, length * 2, raw);
        ushort[] text = new ushort[length];
        for (int i = 0; i < text.Length; i++)
        {
            text[i] = (ushort)Bin.U16(raw, i * 2);
        }
        return text;
    }

    // OFFSET 0x0200AC5C
    public static byte[] MessageBank_Load(int narcID, int bankID, int heapID)
    {
        return NARC.NARC_AllocAndReadWholeMemberByIndexPair(narcID, bankID, heapID);
    }

    // OFFSET 0x0200AC64
    public static void MessageBank_Free(byte[]? bank)
    {
    }

    // OFFSET 0x0200AC6C
    public static void MessageBank_Get(byte[] bank, uint entryID, ushort[] dst)
    {
        if (entryID >= (uint)Bin.U16(bank, 0))
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        uint key = EntryKey(Bin.U16(bank, 2), entryID);
        uint offset = Bin.U32(bank, HeaderSize + (int)entryID * EntrySize) ^ key;
        uint length = Bin.U32(bank, HeaderSize + (int)entryID * EntrySize + 4) ^ key;
        MemCopyEntry(dst, bank, offset, length);
        DecodeString(dst, entryID, length);
    }

    // OFFSET 0x0200ACF0
    public static void MessageBank_GetFromNARC(int narcID, uint bankID, uint entryID, int heapID, ushort[] dst)
    {
        NARC? narc = NARC.NARC_ctor(narcID, heapID);
        if (narc == null)
        {
            return;
        }
        byte[] header = new byte[HeaderSize];
        byte[] entry = new byte[EntrySize];
        NARC.NARC_ReadFromMember(narc, bankID, 0, HeaderSize, header);
        NARC.NARC_ReadFromMember(narc, bankID, entryID * EntrySize + HeaderSize, EntrySize, entry);
        uint key = EntryKey(Bin.U16(header, 2), entryID);
        uint offset = Bin.U32(entry, 0) ^ key;
        uint length = Bin.U32(entry, 4) ^ key;
        ushort[] text = ReadChars(narc, bankID, offset, length);
        DecodeString(text, entryID, length);
        Array.Copy(text, dst, length);
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x0200AD98
    private static void MemCopyEntry(ushort[] dst, byte[] bank, uint offset, uint length)
    {
        for (uint i = 0; i < length; i++)
        {
            dst[i] = (ushort)Bin.U16(bank, (int)(offset + i * 2));
        }
    }

    // OFFSET 0x0200ADAC
    public static void MessageBank_GetString(byte[] bank, uint entryID, GameString dst)
    {
        if (entryID >= (uint)Bin.U16(bank, 0))
        {
            ErrorHandling.GF_AssertFail();
            GameString.String_Clear(dst);
            return;
        }
        uint key = EntryKey(Bin.U16(bank, 2), entryID);
        uint offset = Bin.U32(bank, HeaderSize + (int)entryID * EntrySize) ^ key;
        uint length = Bin.U32(bank, HeaderSize + (int)entryID * EntrySize + 4) ^ key;
        ushort[] text = new ushort[length];
        MemCopyEntry(text, bank, offset, length);
        DecodeString(text, entryID, length);
        GameString.String_CopyNumChars(dst, text, length);
    }

    // OFFSET 0x0200AE5C
    public static GameString? MessageBank_GetNewString(byte[] bank, uint entryID, int heapID)
    {
        if (entryID >= (uint)Bin.U16(bank, 0))
        {
            ErrorHandling.GF_AssertFail();
            return GameString.String_Init(4, heapID);
        }
        uint key = EntryKey(Bin.U16(bank, 2), entryID);
        uint offset = Bin.U32(bank, HeaderSize + (int)entryID * EntrySize) ^ key;
        uint length = Bin.U32(bank, HeaderSize + (int)entryID * EntrySize + 4) ^ key;
        ushort[] text = new ushort[length];
        MemCopyEntry(text, bank, offset, length);
        DecodeString(text, entryID, length);
        GameString str = GameString.String_Init(length, heapID);
        GameString.String_CopyNumChars(str, text, length);
        return str;
    }

    // OFFSET 0x0200AF20
    public static void MessageBank_GetStringFromNARC(int narcID, uint bankID, uint entryID, int heapID, GameString dst)
    {
        NARC? narc = NARC.NARC_ctor(narcID, heapID);
        if (narc != null)
        {
            MessageBank_GetStringFromHandle(narc, bankID, entryID, heapID, dst);
            NARC.NARC_dtor(narc);
        }
    }

    // OFFSET 0x0200AF48
    public static void MessageBank_GetStringFromHandle(NARC narc, uint bankID, uint entryID, int heapID, GameString dst)
    {
        byte[] header = new byte[HeaderSize];
        NARC.NARC_ReadFromMember(narc, bankID, 0, HeaderSize, header);
        if (entryID >= (uint)Bin.U16(header, 0))
        {
            ErrorHandling.GF_AssertFail();
            GameString.String_Clear(dst);
            return;
        }
        byte[] entry = new byte[EntrySize];
        NARC.NARC_ReadFromMember(narc, bankID, entryID * EntrySize + HeaderSize, EntrySize, entry);
        uint key = EntryKey(Bin.U16(header, 2), entryID);
        uint offset = Bin.U32(entry, 0) ^ key;
        uint length = Bin.U32(entry, 4) ^ key;
        ushort[] text = ReadChars(narc, bankID, offset, length);
        DecodeString(text, entryID, length);
        GameString.String_CopyNumChars(dst, text, length);
    }

    // OFFSET 0x0200B010
    public static GameString? MessageBank_GetNewStringFromNARC(int narcID, uint bankID, uint entryID, int heapID)
    {
        NARC? narc = NARC.NARC_ctor(narcID, heapID);
        if (narc == null)
        {
            return GameString.String_Init(4, heapID);
        }
        GameString? str = MessageBank_GetNewStringFromHandle(narc, bankID, entryID, heapID);
        NARC.NARC_dtor(narc);
        return str;
    }

    // OFFSET 0x0200B044
    public static GameString? MessageBank_GetNewStringFromHandle(NARC narc, uint bankID, uint entryID, int heapID)
    {
        byte[] header = new byte[HeaderSize];
        NARC.NARC_ReadFromMember(narc, bankID, 0, HeaderSize, header);
        if (entryID >= (uint)Bin.U16(header, 0))
        {
            ErrorHandling.GF_AssertFail();
            return GameString.String_Init(4, heapID);
        }
        byte[] entry = new byte[EntrySize];
        NARC.NARC_ReadFromMember(narc, bankID, entryID * EntrySize + HeaderSize, EntrySize, entry);
        uint key = EntryKey(Bin.U16(header, 2), entryID);
        uint offset = Bin.U32(entry, 0) ^ key;
        uint length = Bin.U32(entry, 4) ^ key;
        GameString str = GameString.String_Init(length, heapID);
        ushort[] text = ReadChars(narc, bankID, offset, length);
        DecodeString(text, entryID, length);
        GameString.String_CopyNumChars(str, text, length);
        return str;
    }

    // OFFSET 0x0200B120
    public static uint MessageBank_EntryCount(byte[] bank)
    {
        return (uint)Bin.U16(bank, 0);
    }

    // OFFSET 0x0200B124
    public static uint MessageBank_NARCEntryCount(int narcID, int bankID)
    {
        byte[] header = new byte[HeaderSize];
        NARC.NARC_ReadFromMemberByIndexPair(header, narcID, bankID, 0, HeaderSize);
        return (uint)Bin.U16(header, 0);
    }
}

public sealed class MessageLoader
{
    public const int TypePreload = 0;
    public const int TypeNarcHandle = 1;

    private const int MessageArchive = 0x1A;
    private const int SpeciesNameBank = 0x19C;

    public ushort Type;
    public ushort HeapID;
    public ushort NarcID;
    public ushort BankID;
    public byte[]? Bank;
    public NARC? Narc;

    // OFFSET 0x0200B144
    public static MessageLoader? MessageLoader_Init(int type, int narcID, int bankID, int heapID)
    {
        MessageLoader loader = new();
        if (type == TypePreload)
        {
            loader.Bank = MessageBank.MessageBank_Load(narcID, bankID, heapID);
            if (loader.Bank == null)
            {
                return null;
            }
        }
        else
        {
            loader.Narc = NARC.NARC_ctor(narcID, heapID);
        }
        loader.Type = (ushort)type;
        loader.NarcID = (ushort)narcID;
        loader.BankID = (ushort)bankID;
        loader.HeapID = (ushort)heapID;
        return loader;
    }

    // OFFSET 0x0200B190
    public static void MessageLoader_Free(MessageLoader? loader)
    {
        if (loader == null)
        {
            return;
        }
        switch (loader.Type)
        {
            case TypePreload:
                MessageBank.MessageBank_Free(loader.Bank);
                break;
            case TypeNarcHandle:
                NARC.NARC_dtor(loader.Narc!);
                break;
        }
    }

    // OFFSET 0x0200B1B8
    public static void MessageLoader_GetString(MessageLoader loader, uint entryID, GameString dst)
    {
        switch (loader.Type)
        {
            case TypePreload:
                MessageBank.MessageBank_GetString(loader.Bank!, entryID, dst);
                break;
            case TypeNarcHandle:
                MessageBank.MessageBank_GetStringFromHandle(loader.Narc!, loader.BankID, entryID, loader.HeapID, dst);
                break;
        }
    }

    // OFFSET 0x0200B1EC
    public static GameString? MessageLoader_GetNewString(MessageLoader loader, uint entryID)
    {
        switch (loader.Type)
        {
            case TypePreload:
                return MessageBank.MessageBank_GetNewString(loader.Bank!, entryID, loader.HeapID);
            case TypeNarcHandle:
                return MessageBank.MessageBank_GetNewStringFromHandle(loader.Narc!, loader.BankID, entryID, loader.HeapID);
        }
        return null;
    }

    // OFFSET 0x0200B218
    public static uint MessageLoader_MessageCount(MessageLoader loader)
    {
        switch (loader.Type)
        {
            case TypePreload:
                return MessageBank.MessageBank_EntryCount(loader.Bank!);
            case TypeNarcHandle:
                return MessageBank.MessageBank_NARCEntryCount(loader.NarcID, loader.BankID);
        }
        return 0;
    }

    // OFFSET 0x0200B240
    public static void MessageLoader_Get(MessageLoader loader, uint entryID, ushort[] dst)
    {
        switch (loader.Type)
        {
            case TypePreload:
                MessageBank.MessageBank_Get(loader.Bank!, entryID, dst);
                break;
            case TypeNarcHandle:
                MessageBank.MessageBank_GetFromNARC(loader.NarcID, loader.BankID, entryID, loader.HeapID, dst);
                break;
        }
    }

    // OFFSET 0x0200B274
    public static void MessageLoader_GetSpeciesName(uint species, int heapID, ushort[] dst)
    {
        MessageLoader? loader = MessageLoader_Init(TypeNarcHandle, MessageArchive, SpeciesNameBank, heapID);
        MessageLoader_Get(loader!, species, dst);
        MessageLoader_Free(loader);
    }
}

public static class MessageUtil
{
    private const int MessageArchive = 0x1A;
    private const int MoveNameBank = 0x287;
    private const int SpeciesNameBank = 0x19C;

    // OFFSET 0x0200B29C
    public static GameString? MessageUtil_ExpandedString(StringTemplate template, MessageLoader loader, uint entryID, int heapID)
    {
        GameString? result = null;
        GameString buffer = GameString.String_Init(0x400, 0);
        GameString? format = MessageLoader.MessageLoader_GetNewString(loader, entryID);
        if (format != null)
        {
            StringTemplate.StringTemplate_Format(template, buffer, format);
            result = GameString.String_Clone(buffer, heapID);
            GameString.String_Free(format);
        }
        GameString.String_Free(buffer);
        return result;
    }

    // OFFSET 0x0200B2EC
    public static GameString? MessageUtil_MoveName(uint moveID, int heapID)
    {
        MessageLoader? loader = MessageLoader.MessageLoader_Init(MessageLoader.TypeNarcHandle, MessageArchive, MoveNameBank, heapID);
        if (loader == null)
        {
            return null;
        }
        GameString name = GameString.String_Init(0x10, heapID);
        MessageLoader.MessageLoader_GetString(loader, moveID, name);
        MessageLoader.MessageLoader_Free(loader);
        return name;
    }

    // OFFSET 0x0200B32C
    public static GameString? MessageUtil_SpeciesName(uint species, int heapID)
    {
        MessageLoader? loader = MessageLoader.MessageLoader_Init(MessageLoader.TypeNarcHandle, MessageArchive, SpeciesNameBank, heapID);
        if (loader == null)
        {
            return null;
        }
        GameString? name = MessageLoader.MessageLoader_GetNewString(loader, species);
        MessageLoader.MessageLoader_Free(loader);
        return name;
    }
}
