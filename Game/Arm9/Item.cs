using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class Item
{
    public const int FileTypeData = 0;
    public const int FileTypeIcon = 1;
    public const int FileTypePalette = 2;
    public const int FileTypeGBAID = 3;

    public const int ParamPrice = 0;
    public const int ParamHoldEffect = 1;
    public const int ParamHoldEffectParam = 2;
    public const int ParamPreventToss = 3;
    public const int ParamIsSelectable = 4;
    public const int ParamFieldPocket = 5;
    public const int ParamFieldUseFunc = 6;
    public const int ParamBattleUseFunc = 7;
    public const int ParamPluckEffect = 8;
    public const int ParamFlingEffect = 9;
    public const int ParamFlingPower = 10;
    public const int ParamNaturalGiftPower = 11;
    public const int ParamNaturalGiftType = 12;
    public const int ParamBattlePocket = 13;
    public const int ParamPartyUse = 14;
    public const int ParamPartyFirst = 15;

    public const int ItemDataSize = 0x24;

    private const int MaxItem = 0x1D3;
    private const int ItemNone = 0;
    private const int ItemReturnID = 0xFFFF;
    private const int NarcItemData = 0xF;
    private const int NarcItemIcon = 0x10;
    private const int NarcMessages = 0x1A;
    private const int MessageBankItemNames = 0x188;
    private const int MessageBankItemDescriptions = 0x187;
    private const int IconNone = 0x2C3;
    private const int PaletteNone = 0x2C4;
    private const int IconReturn = 0x2C5;
    private const int PaletteReturn = 0x2C6;
    private const uint ArchiveLinkTableAddress = 0x020F0CC4;
    private const int ArchiveLinkSize = 8;
    private const int ArchiveLinkData = 0;
    private const int ArchiveLinkIcon = 2;
    private const int ArchiveLinkPalette = 4;
    private const int ArchiveLinkGBAID = 6;
    private const uint TMHMMoveTableAddress = 0x020F0BFC;
    private const int FirstTMHMItem = 0x148;
    private const int TMHMCount = 100;
    private const int FirstHMIndex = 92;
    private const int HMCount = 8;
    private const uint MailItemTableAddress = 0x020F0B64;
    private const int MailCount = 12;
    private const uint BerryItemTableAddress = 0x020F0B7C;
    private const int BerryCount = 0x40;
    private const int FirstBerryItem = 0x95;
    private const int FirstHerbalMedicine = 0x22;
    private const int HerbalMedicineCount = 4;
    private const int DataPrice = 0;
    private const int DataHoldEffect = 2;
    private const int DataHoldEffectParam = 3;
    private const int DataPluckEffect = 4;
    private const int DataFlingEffect = 5;
    private const int DataFlingPower = 6;
    private const int DataNaturalGiftPower = 7;
    private const int DataFlags = 8;
    private const int DataFieldUseFunc = 0xA;
    private const int DataBattleUseFunc = 0xB;
    private const int DataPartyUse = 0xC;
    private const int DataPartyParam = 0xE;
    private const int SlotSize = 4;

    private static ushort[]? archiveLinks;
    private static ushort[]? tmhmMoves;
    private static ushort[]? mailItems;
    private static ushort[]? berryItems;

    private static ushort ArchiveLink(int item, int field)
    {
        archiveLinks ??= RomData.Arm9U16(ArchiveLinkTableAddress, (MaxItem + 1) * ArchiveLinkSize / 2);
        return archiveLinks[(item * ArchiveLinkSize + field) / 2];
    }

    private static ushort[] TMHMMoves()
    {
        return tmhmMoves ??= RomData.Arm9U16(TMHMMoveTableAddress, TMHMCount);
    }

    private static ushort[] MailItems()
    {
        return mailItems ??= RomData.Arm9U16(MailItemTableAddress, MailCount);
    }

    private static ushort[] BerryItems()
    {
        return berryItems ??= RomData.Arm9U16(BerryItemTableAddress, BerryCount);
    }

    // OFFSET 0x0207CDEC
    public static void Item_MoveInPocket(BufferView slots, ushort from, ushort to)
    {
        if (from == to)
        {
            return;
        }
        ushort item = slots.U16(from * SlotSize);
        ushort quantity = slots.U16(from * SlotSize + 2);
        if (to > from)
        {
            to = (ushort)(to - 1);
            for (short i = (short)from; i < (short)to; i++)
            {
                slots.W16(i * SlotSize, slots.U16((i + 1) * SlotSize));
                slots.W16(i * SlotSize + 2, slots.U16((i + 1) * SlotSize + 2));
            }
        }
        else
        {
            for (short i = (short)from; i > (short)to; i--)
            {
                slots.W16(i * SlotSize, slots.U16((i - 1) * SlotSize));
                slots.W16(i * SlotSize + 2, slots.U16((i - 1) * SlotSize + 2));
            }
        }
        slots.W16(to * SlotSize, item);
        slots.W16(to * SlotSize + 2, quantity);
    }

    // OFFSET 0x0207CE78
    public static ushort Item_FileID(ushort item, int type)
    {
        switch (type)
        {
            case FileTypeData:
                if (item == ItemNone || item == ItemReturnID)
                {
                    break;
                }
                return ArchiveLink(item, ArchiveLinkData);
            case FileTypeIcon:
                if (item == ItemNone)
                {
                    return IconNone;
                }
                if (item == ItemReturnID)
                {
                    return IconReturn;
                }
                return ArchiveLink(item, ArchiveLinkIcon);
            case FileTypePalette:
                if (item == ItemNone)
                {
                    return PaletteNone;
                }
                if (item == ItemReturnID)
                {
                    return PaletteReturn;
                }
                return ArchiveLink(item, ArchiveLinkPalette);
            case FileTypeGBAID:
                if (item == ItemNone || item == ItemReturnID)
                {
                    break;
                }
                return ArchiveLink(item, ArchiveLinkGBAID);
        }
        return 0;
    }

    // OFFSET 0x0207CF10
    public static ushort Item_FromGBAID(ushort gbaID)
    {
        for (ushort i = 1; i <= MaxItem; i++)
        {
            if (gbaID == ArchiveLink(i, ArchiveLinkGBAID))
            {
                return i;
            }
        }
        return ItemNone;
    }

    // OFFSET 0x0207CF40
    public static int Item_IconCellFile()
    {
        return 1;
    }

    // OFFSET 0x0207CF44
    public static int Item_IconAnimFile()
    {
        return 0;
    }

    // OFFSET 0x0207CF48
    public static byte[]? Item_Load(ushort item, int type, int heapID)
    {
        if (item > MaxItem)
        {
            item = ItemNone;
        }
        switch (type)
        {
            case FileTypeData:
                return NARC.NARC_AllocAndReadWholeMemberByIndexPair(NarcItemData, ArchiveLink(item, ArchiveLinkData), heapID);
            case FileTypeIcon:
                return NARC.NARC_AllocAndReadWholeMemberByIndexPair(NarcItemIcon, ArchiveLink(item, ArchiveLinkIcon), heapID);
            case FileTypePalette:
                return NARC.NARC_AllocAndReadWholeMemberByIndexPair(NarcItemIcon, ArchiveLink(item, ArchiveLinkPalette), heapID);
        }
        return null;
    }

    // OFFSET 0x0207CFA0
    public static void Item_LoadName(GameString dst, ushort item, int heapID)
    {
        MessageLoader? loader = MessageLoader.MessageLoader_Init(1, NarcMessages, MessageBankItemNames, heapID);
        MessageLoader.MessageLoader_GetString(loader!, item, dst);
        MessageLoader.MessageLoader_Free(loader);
    }

    // OFFSET 0x0207CFC8
    public static void Item_LoadDescription(GameString dst, ushort item, int heapID)
    {
        MessageLoader? loader = MessageLoader.MessageLoader_Init(1, NarcMessages, MessageBankItemDescriptions, heapID);
        MessageLoader.MessageLoader_GetString(loader!, item, dst);
        MessageLoader.MessageLoader_Free(loader);
    }

    // OFFSET 0x0207CFF0
    public static int Item_LoadParam(ushort item, int param, int heapID)
    {
        byte[] itemData = Item_Load(item, FileTypeData, heapID)!;
        int value = Item_Get(itemData, 0, param);
        Heap.Heap_FreeExplicit(heapID, itemData);
        return value;
    }

    // OFFSET 0x0207D014
    public static int Item_Get(byte[] itemData, int offset, int param)
    {
        ushort flags = (ushort)Bin.U16(itemData, offset + DataFlags);
        switch (param)
        {
            case ParamPrice:
                return Bin.U16(itemData, offset + DataPrice);
            case ParamHoldEffect:
                return itemData[offset + DataHoldEffect];
            case ParamHoldEffectParam:
                return itemData[offset + DataHoldEffectParam];
            case ParamPreventToss:
                return (flags >> 5) & 1;
            case ParamIsSelectable:
                return (flags >> 6) & 1;
            case ParamFieldPocket:
                return (flags >> 7) & 0xF;
            case ParamFieldUseFunc:
                return itemData[offset + DataFieldUseFunc];
            case ParamBattleUseFunc:
                return itemData[offset + DataBattleUseFunc];
            case ParamPluckEffect:
                return itemData[offset + DataPluckEffect];
            case ParamFlingEffect:
                return itemData[offset + DataFlingEffect];
            case ParamFlingPower:
                return itemData[offset + DataFlingPower];
            case ParamNaturalGiftPower:
                return itemData[offset + DataNaturalGiftPower];
            case ParamNaturalGiftType:
                return flags & 0x1F;
            case ParamBattlePocket:
                return (flags >> 11) & 0x1F;
            case ParamPartyUse:
                return itemData[offset + DataPartyUse];
        }
        switch (itemData[offset + DataPartyUse])
        {
            case 0:
                return itemData[offset + DataPartyParam];
            case 1:
                return ItemPartyParam_Get(itemData, offset + DataPartyParam, param);
        }
        return 0;
    }

    // OFFSET 0x0207D0B0
    private static int ItemPartyParam_Get(byte[] data, int offset, int param)
    {
        int index = param - ParamPartyFirst;
        if ((uint)index > 43)
        {
            return 0;
        }
        if (index < 8)
        {
            return (data[offset] >> index) & 1;
        }
        if (index < 12)
        {
            return (data[offset + 1] >> (index - 8)) & 1;
        }
        if (index >= 21 && index < 29)
        {
            return (data[offset + 5] >> (index - 21)) & 1;
        }
        if (index >= 29 && index < 33)
        {
            return (data[offset + 6] >> (index - 29)) & 1;
        }
        if (index >= 33 && index < 39)
        {
            return (sbyte)data[offset + 7 + index - 33];
        }
        if (index >= 41)
        {
            return (sbyte)data[offset + 0xF + index - 41];
        }
        switch (index)
        {
            case 12:
                return data[offset + 1] >> 4;
            case 13:
                return data[offset + 2] & 0xF;
            case 14:
                return data[offset + 2] >> 4;
            case 15:
                return data[offset + 3] & 0xF;
            case 16:
                return data[offset + 3] >> 4;
            case 17:
                return data[offset + 4] & 0xF;
            case 18:
                return (data[offset + 4] >> 4) & 3;
            case 19:
                return (data[offset + 4] >> 6) & 1;
            case 20:
                return (data[offset + 4] >> 7) & 1;
            case 39:
                return data[offset + 0xD];
            case 40:
                return data[offset + 0xE];
        }
        return 0;
    }

    // OFFSET 0x0207D268
    public static ushort Item_MoveForTMHM(ushort item)
    {
        if (item < FirstTMHMItem || item > FirstTMHMItem + TMHMCount - 1)
        {
            return 0;
        }
        return TMHMMoves()[(ushort)(item - FirstTMHMItem)];
    }

    // OFFSET 0x0207D28C
    public static int Item_IsHMMove(ushort move)
    {
        for (byte i = 0; i < HMCount; i++)
        {
            if (move == TMHMMoves()[FirstHMIndex + i])
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0207D2B4
    public static byte Item_TMHMNumber(ushort item)
    {
        if (item < FirstTMHMItem || item > FirstTMHMItem + TMHMCount - 1)
        {
            return 0;
        }
        return (byte)(item - FirstTMHMItem);
    }

    // OFFSET 0x0207D2D0
    public static int Item_IsMail(ushort item)
    {
        for (uint i = 0; i < MailCount; i++)
        {
            if (item == MailItems()[i])
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0207D2F0
    public static byte Item_MailNumber(ushort item)
    {
        for (uint i = 0; i < MailCount; i++)
        {
            if (item == MailItems()[i])
            {
                return (byte)i;
            }
        }
        return 0;
    }

    // OFFSET 0x0207D310
    public static ushort Item_ForMailNumber(byte mail)
    {
        if (mail >= MailCount)
        {
            return 0;
        }
        return MailItems()[mail];
    }

    // OFFSET 0x0207D324
    public static int Item_IsBerry(ushort item)
    {
        for (uint i = 0; i < BerryCount; i++)
        {
            if (item == BerryItems()[i])
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0207D344
    public static byte Item_BerryNumber(ushort item)
    {
        if (item < FirstBerryItem)
        {
            return 0xFF;
        }
        return (byte)(item - FirstBerryItem);
    }

    // OFFSET 0x0207D354
    public static ushort Item_ForBerryNumber(byte berry)
    {
        if (berry >= BerryCount)
        {
            return 0xFFFF;
        }
        return BerryItems()[berry];
    }

    // OFFSET 0x0207D36C
    public static byte Item_IsHerbalMedicine(ushort item)
    {
        if ((ushort)(item - FirstHerbalMedicine) <= HerbalMedicineCount - 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0207D388
    public static byte[] Item_LoadTable(int heapID)
    {
        int size = Item_FileID(MaxItem, FileTypeData) * ItemDataSize;
        return NARC.NARC_AllocAndReadFromMemberByIndexPair(NarcItemData, 0, heapID, 0, size);
    }

    // OFFSET 0x0207D3B0
    public static int ItemTable_Index(int index)
    {
        return index * ItemDataSize;
    }
}
