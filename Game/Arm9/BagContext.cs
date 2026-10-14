namespace PlatinumSharp.Game;

public sealed class BagApplicationPocket
{
    public BufferView? Items;
    public ushort CursorPos;
    public ushort CursorScroll;
    public byte PocketType;
    public byte ListEntryCount;
}

public sealed class BagContext
{
    public const int Size = 0x78;
    public const int PocketMax = 8;

    public SaveData? SaveData;
    public readonly BagApplicationPocket[] AccessiblePockets = NewPockets();
    public byte CurrPocketIdx;
    public byte Mode;
    public ushort SelectedItem;
    public ushort ExitCode;
    public BagCursor? BagCursor;
    public ItemUseContext? ItemUseCtx;
    public byte SelectedMonSlot;
    public byte SoldAmount;
    public bool IsCycling;
    public ushort MapLoadType;

    private const int SealCount = 0x50;

    private static BagApplicationPocket[] NewPockets()
    {
        BagApplicationPocket[] pockets = new BagApplicationPocket[PocketMax];
        for (int i = 0; i < pockets.Length; i++)
        {
            pockets[i] = new BagApplicationPocket();
        }
        return pockets;
    }

    // OFFSET 0x0207CB08
    public static BagContext BagContext_New(byte heapID)
    {
        return new BagContext();
    }

    // OFFSET 0x0207CB20
    public static uint BagContext_GetSize()
    {
        return Size;
    }

    // OFFSET 0x0207CB24
    public static void BagContext_SetMode(BagContext bagContext, byte mode)
    {
        bagContext.Mode = mode;
    }

    // OFFSET 0x0207CB2C
    public static void BagContext_Init(BagContext bagContext, SaveData saveData, byte mode, BagCursor? bagCursor)
    {
        BagContext_SetMode(bagContext, mode);
        bagContext.SaveData = saveData;
        bagContext.BagCursor = bagCursor;
        bagContext.SelectedItem = 0;
    }

    // OFFSET 0x0207CB48
    public static void BagContext_InitPocket(BagContext bagContext, BufferView items, byte pocketType, byte position)
    {
        bagContext.AccessiblePockets[position].Items = items;
        bagContext.AccessiblePockets[position].PocketType = pocketType;
    }

    // OFFSET 0x0207CB58
    public static void BagContext_SetIsCycling(BagContext bagContext)
    {
        bagContext.IsCycling = true;
    }

    // OFFSET 0x0207CB6C
    public static void BagContext_SetItemUseContext(BagContext bagContext, ItemUseContext? itemUseContext)
    {
        bagContext.ItemUseCtx = itemUseContext;
    }

    // OFFSET 0x0207CB70
    public static void BagContext_SetSelectedMonSlot(BagContext bagContext, byte slot)
    {
        bagContext.SelectedMonSlot = slot;
    }

    // OFFSET 0x0207CB78
    public static void BagContext_SetMapLoadType(BagContext bagContext, ushort mapLoadType)
    {
        bagContext.MapLoadType = (ushort)(mapLoadType & 0x7FFF);
    }

    // OFFSET 0x0207CB94
    public static ushort BagContext_GetItem(BagContext bagContext)
    {
        return bagContext.SelectedItem;
    }

    // OFFSET 0x0207CB9C
    public static ushort BagContext_GetExitCode(BagContext bagContext)
    {
        return bagContext.ExitCode;
    }

    // OFFSET 0x0207CBA4
    public static byte BagContext_GetSelectedMonSlot(BagContext bagContext)
    {
        return bagContext.SelectedMonSlot;
    }

    // OFFSET 0x0207CBAC
    public static byte BagContext_GetSoldAmount(BagContext bagContext)
    {
        return bagContext.SoldAmount;
    }

    // OFFSET 0x0207CBB4
    public static ushort BagContext_GetCoins(SaveData saveData)
    {
        return Coins.Coins_GetValue(PlayerSave.SaveData_GetCoins(saveData));
    }

    // OFFSET 0x0207CBC0
    public static int BagContext_CountSeals(SaveData saveData)
    {
        SealCase sealCase = SealCase.SaveData_GetSealCase(saveData);
        int count = 0;
        for (int seal = 1; seal <= SealCount; seal++)
        {
            count += SealCase.SealCase_CountSealOccurrenceAnywhere(sealCase, seal);
        }
        return count;
    }

    // OFFSET 0x0207CBE0
    public static uint BagContext_CountAccessories(SaveData saveData)
    {
        return FashionCase.FashionCase_CountAccessories(ImageClips.ImageClips_GetFashionCase(ImageClips.SaveData_GetImageClips(saveData)));
    }

    // OFFSET 0x0207CBF0
    public static uint BagContext_CountBackdrops(SaveData saveData)
    {
        return FashionCase.FashionCase_CountBackdrops(ImageClips.ImageClips_GetFashionCase(ImageClips.SaveData_GetImageClips(saveData)));
    }

    // OFFSET 0x0207CC00
    public static ushort BagContext_GetBattlePoints(SaveData saveData)
    {
        return BattleTowerRecords.BattlePoints_ApplyFuncAndGet(BattleTowerRecords.SaveData_GetBattleTowerRecords(saveData), 0, 0);
    }

    private const int MessageArchive = 0x1A;
    private const int BagTextBank = 7;
    private const int UnusableItemBank = 0xD5;
    private const ushort ItemPointCard = 0x1B0;
    private const ushort ItemSealCase = 0x1B2;
    private const ushort ItemFashionCase = 0x1B3;
    private const ushort ItemCoinCase = 0x1BC;

    // OFFSET 0x0207CC10
    public static bool BagContext_FormatUsageMessage(SaveData saveData, GameString dest, ushort item, int heapID)
    {
        MessageLoader loader = MessageLoader.MessageLoader_Init(0, MessageArchive, BagTextBank, heapID)!;
        StringTemplate template = StringTemplate.StringTemplate_Default(heapID);
        GameString format;
        if (item == 0)
        {
            format = MessageLoader.MessageLoader_GetNewString(loader, 0x63)!;
        }
        else if (item == ItemPointCard)
        {
            format = MessageLoader.MessageLoader_GetNewString(loader, 0x61)!;
            StringTemplate.StringTemplate_SetNumber(template, 0, BagContext_GetBattlePoints(saveData), 4, 0, 1);
        }
        else if (item == ItemSealCase)
        {
            format = MessageLoader.MessageLoader_GetNewString(loader, 0x5C)!;
            StringTemplate.StringTemplate_SetNumber(template, 0, BagContext_CountSeals(saveData), 4, 0, 1);
        }
        else if (item == ItemFashionCase)
        {
            format = MessageLoader.MessageLoader_GetNewString(loader, 0x5D)!;
            StringTemplate.StringTemplate_SetNumber(template, 0, (int)BagContext_CountAccessories(saveData), 3, 0, 1);
            StringTemplate.StringTemplate_SetNumber(template, 1, (int)BagContext_CountBackdrops(saveData), 2, 0, 1);
        }
        else if (item == ItemCoinCase)
        {
            format = MessageLoader.MessageLoader_GetNewString(loader, 0x39)!;
            StringTemplate.StringTemplate_SetNumber(template, 0, BagContext_GetCoins(saveData), 5, 0, 1);
        }
        else
        {
            StringTemplate.StringTemplate_Free(template);
            MessageLoader.MessageLoader_Free(loader);
            return false;
        }
        StringTemplate.StringTemplate_Format(template, dest, format);
        GameString.String_Free(format);
        StringTemplate.StringTemplate_Free(template);
        MessageLoader.MessageLoader_Free(loader);
        return true;
    }

    // OFFSET 0x0207CD34
    public static void BagContext_FormatErrorMessage(TrainerInfo trainerInfo, GameString dest, ushort item, int error, int heapID)
    {
        MessageLoader loader;
        switch (error)
        {
            case 1:
                loader = MessageLoader.MessageLoader_Init(1, MessageArchive, BagTextBank, heapID)!;
                MessageLoader.MessageLoader_GetString(loader, 0x38, dest);
                MessageLoader.MessageLoader_Free(loader);
                return;
            case 2:
                loader = MessageLoader.MessageLoader_Init(1, MessageArchive, BagTextBank, heapID)!;
                MessageLoader.MessageLoader_GetString(loader, 0x6F, dest);
                MessageLoader.MessageLoader_Free(loader);
                return;
            case 3:
                loader = MessageLoader.MessageLoader_Init(1, MessageArchive, BagTextBank, heapID)!;
                MessageLoader.MessageLoader_GetString(loader, 0x70, dest);
                MessageLoader.MessageLoader_Free(loader);
                return;
        }
        loader = MessageLoader.MessageLoader_Init(1, MessageArchive, UnusableItemBank, heapID)!;
        StringTemplate template = StringTemplate.StringTemplate_Default(heapID);
        GameString format = MessageLoader.MessageLoader_GetNewString(loader, 0x24)!;
        StringTemplate.StringTemplate_SetPlayerName(template, 0, trainerInfo);
        StringTemplate.StringTemplate_Format(template, dest, format);
        GameString.String_Free(format);
        StringTemplate.StringTemplate_Free(template);
        MessageLoader.MessageLoader_Free(loader);
    }
}
