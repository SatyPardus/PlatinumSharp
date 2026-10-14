using System.Buffers.Binary;

namespace PlatinumSharp.Game;

public static partial class BagApplication
{
    private const uint YesNoWindowTemplate = 0x02241150;
    private const uint BagSpriteScale = 0x02241158;
    private const uint CharTransferTemplateAddress = 0x02241164;
    private const uint SpriteCapacitiesAddress = 0x02241178;
    private const uint RenderOamTemplateAddress = 0x02241190;
    private const uint SpriteTemplatesAddress = 0x022411B0;
    private const int SpriteTemplateSize = 0x34;
    private const int PocketNamesBank = 0x18B;
    private const int StandardWindowTile = 0x3F7;
    private const int StandardWindowPalette = 0xE;
    private const uint ColorDescription = 0xF0E00;
    private const uint ColorItemList = 0x10200;
    private const int FirstHMItem = 0x1A4;
    private const int FirstTMItem = 0x148;
    private const int ItemGraphicsArchive = 0x10;
    private const int ListIconsMember = 0x26;
    private const int PocketIconsMember = 0x15;
    private const int SeqPrinterJingle = 0x60D;
    private const int SpriteItemCountArrowUp = 8;
    private const int SpriteItemCountArrowDown = 9;
    private const int SpriteMoveType = 10;
    private const int SpriteMoveCategory = 11;
    private const int SpriteShockwave = 6;
    private const int ResourceBag = 0xC0F9;
    private const int ResourcePocketHighlight = 0xC0FA;
    private const int ResourceArrows = 0xC0FB;
    private const int ResourceItemHighlight = 0xC0FC;
    private const int ResourceMovingBar = 0xC0FD;
    private const int ResourceShockwave = 0xC0FE;
    private const int ResourceItem = 0xC0FF;
    private const int ResourceCountArrows = 0xC100;
    private const int ResourceMoveType = 0xC101;
    private const int ResourceMoveCategory = 0xC102;
    private const int PocketHighlightY = 0x61;
    private const int WinPocketNames = 2;
    private const int WinPocketIndicator = 5;
    private const int WinPoffinCount = 10;

    private static readonly ushort[] ItemActionStringIDs = { 0, 6, 0x10, 0x5F, 0x60, 1, 2, 0x12, 3, 4, 5, 8 };

    // OFFSET 0x0223F040
    private static void BagUI_CreateWindows(BagController controller)
    {
        BgConfig bgConfig = controller.BgConfig!;
        Bg.Window_Add(bgConfig, controller.Windows[0], 2, 0xE, 0, 0x11, 0x12, 3, 1);
        Bg.Window_Add(bgConfig, controller.Windows[1], 0, 0, 0x12, 0x20, 6, 3, 0x133);
        Bg.Window_Add(bgConfig, controller.Windows[2], 2, 0, 0xD, 0x24, 3, 3, 0x1F3);
        Bg.Window_Add(bgConfig, controller.Windows[3], 0, 6, 0x13, 0xE, 4, 0xB, 0x25F);
        Bg.Window_Add(bgConfig, controller.Windows[4], 0, 6, 0x13, 0xD, 4, 0xB, 0x25F);
        Bg.Window_Add(bgConfig, controller.Windows[5], 0, 0, 0xB, 0xC, 2, 0xD, 0x297);
        Bg.Window_Add(bgConfig, controller.Windows[6], 0, 2, 0x13, 0x1B, 4, 0xB, 0x2AF);
        Bg.Window_Add(bgConfig, controller.Windows[7], 0, 0x13, 0xD, 0xC, 4, 3, 0x337);
        Bg.Window_Add(bgConfig, controller.Windows[8], 0, 1, 1, 0xA, 4, 3, 0x367);
        Bg.Window_Add(bgConfig, controller.Windows[9], 0, 0x18, 0x13, 7, 4, 3, 0x387);
        Bg.Window_Add(bgConfig, controller.Windows[10], 0, 1, 0xC, 0xB, 4, 3, 0x387);
    }

    // OFFSET 0x0223F1DC
    private static void BagUI_DeleteWindows(Window[] windows)
    {
        for (int i = 0; i < BagController.NumWindows; i++)
        {
            Bg.Window_Remove(windows[i]);
        }
    }

    // OFFSET 0x0223F1F8
    private static void BagUI_LoadPocketNames(BagController controller)
    {
        MessageLoader loader = MessageLoader.MessageLoader_Init(1, MessageArchive, PocketNamesBank, HeapID)!;
        for (int i = 0; i < BagContext.PocketMax; i++)
        {
            controller.PocketNames[i] = MessageLoader.MessageLoader_GetNewString(loader, (uint)i);
        }
        MessageLoader.MessageLoader_Free(loader);
    }

    // OFFSET 0x0223F238
    private static void BagUI_FreePocketNames(BagController controller)
    {
        for (int i = 0; i < BagContext.PocketMax; i++)
        {
            GameString.String_Free(controller.PocketNames[i]);
        }
    }

    // OFFSET 0x0223F25C
    private static void BagUI_ClearPocketNameBox(BagController controller)
    {
        for (int i = 0; i < 0xC; i++)
        {
            Bg.Bg_FillTilemapRect(controller.BgConfig!, 2, (ushort)(0x1FF + i), i, 0xD, 1, 1, 3);
            Bg.Bg_FillTilemapRect(controller.BgConfig!, 2, (ushort)(0x223 + i), i, 0xE, 1, 1, 3);
        }
    }

    // OFFSET 0x0223F2C0
    private static void PrintPocketNameCentered(BagController controller, GameString name, ushort centerX)
    {
        int width = Font.Font_CalcStringWidth(0, name, 0);
        Text.Text_AddPrinterWithParamsAndColor(controller.Windows[WinPocketNames], 0, name, (uint)(centerX - (width >> 1)), 2, 0xFF, ColorItemList, null);
    }

    // OFFSET 0x0223F2FC
    private static void BagUI_PrintPocketNames(BagController controller)
    {
        Bg.Window_FillTilemap(controller.Windows[WinPocketNames], 0);
        BagContext bagCtx = controller.BagCtx!;
        BagPocketSelector selector = controller.PocketSelector;
        GameString current = controller.PocketNames[bagCtx.AccessiblePockets[bagCtx.CurrPocketIdx].PocketType]!;
        GameString next = controller.PocketNames[bagCtx.AccessiblePockets[selector.NextPocket].PocketType]!;
        ushort x;
        if (selector.NextPocketDirection == 0)
        {
            x = (ushort)(selector.AnimFrame * 12 + 0x92);
            PrintPocketNameCentered(controller, next, (ushort)(x - 0x60));
        }
        else
        {
            x = (ushort)(0x92 - selector.AnimFrame * 12);
            PrintPocketNameCentered(controller, next, (ushort)(x + 0x60));
        }
        PrintPocketNameCentered(controller, current, x);
        Bg.Window_LoadTiles(controller.Windows[WinPocketNames]);
    }

    // OFFSET 0x0223F390
    private static byte[] LoadPocketSelectorIcons(BagController controller, out CharacterData? charData)
    {
        byte[] data = NARC.NARC_AllocAndReadWholeMember(controller.BagGraphicsNARC!, PocketIconsMember, HeapID);
        G2d.GetUnpackedCharacterData(data, out charData);
        return data;
    }

    private static void BlitPocketSelectorIcon(BagController controller, CharacterData charData, int pocketIdx, bool selected)
    {
        int pocketType = controller.BagCtx!.AccessiblePockets[pocketIdx].PocketType;
        int sourceX = selected ? (byte)(pocketType * 32 + 0x10) : (byte)(pocketType << 5);
        int destX = (ushort)(controller.PocketSelectorIconsX + controller.PocketSelectorIconsSpacing * pocketIdx);
        Bg.Window_BlitBitmapRect(controller.Windows[WinPocketIndicator], charData.RawData, sourceX, 0, 0x100, 0x10, destX, 3, 0xA, 0xA);
    }

    // OFFSET 0x0223F3AC
    private static void BagUI_DrawPocketSelectorIcon(BagController controller, int pocketIdx, int selected)
    {
        byte[] data = LoadPocketSelectorIcons(controller, out CharacterData? charData);
        BlitPocketSelectorIcon(controller, charData!, pocketIdx, selected == 1);
        Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinPocketIndicator]);
        Heap.Heap_FreeExplicit(HeapID, data);
    }

    // OFFSET 0x0223F438
    private static void BagUI_DrawPocketSelectorIcons(BagController controller)
    {
        byte[] data = LoadPocketSelectorIcons(controller, out CharacterData? charData);
        Bg.Window_FillTilemap(controller.Windows[WinPocketIndicator], 0);
        for (int i = 0; i < controller.NumPockets; i++)
        {
            BlitPocketSelectorIcon(controller, charData!, i, i == controller.BagCtx!.CurrPocketIdx);
        }
        Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinPocketIndicator]);
        Heap.Heap_FreeExplicit(HeapID, data);
    }

    // OFFSET 0x0223F4E8
    private static void BufferPocketSlotItemName(BagController controller, int slot, uint templateIdx)
    {
        ushort item = BagApplication_GetItemSlotParam(controller, (ushort)slot, 0);
        StringTemplate.StringTemplate_SetItemName(controller.StrTemplate!, templateIdx, item);
    }

    // OFFSET 0x0223F508
    private static void BufferPocketSlotItemNamePlural(BagController controller, int slot, uint templateIdx)
    {
        ushort item = BagApplication_GetItemSlotParam(controller, (ushort)slot, 0);
        StringTemplate.StringTemplate_SetItemNamePlural(controller.StrTemplate!, templateIdx, item);
    }

    // OFFSET 0x0223F528
    private static void BagUI_PrintItemDescription(BagController controller, ushort item)
    {
        GameString description;
        if (item != 0xFFFF)
        {
            description = GameString.String_Init(0x82, HeapID);
            Item.Item_LoadDescription(description, item, HeapID);
        }
        else
        {
            description = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x5E)!;
        }
        Text.Text_AddPrinterWithParamsAndColor(controller.Windows[WinItemDescription], 0, description, 0x28, 0, 0xFF, ColorDescription, null);
        GameString.String_Free(description);
    }

    private static void PrintDescriptionString(BagController controller, uint entry, uint x, uint y)
    {
        GameString str = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, entry)!;
        Text.Text_AddPrinterWithParamsAndColor(controller.Windows[WinItemDescription], 0, str, x, y, 0xFF, ColorDescription, null);
        GameString.String_Free(str);
    }

    private static void PrintMoveStatValue(BagController controller, int value, uint maxDigits, uint y)
    {
        GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, value <= 1 ? 0x18u : 0x5Bu)!;
        StringTemplate.StringTemplate_SetNumber(controller.StrTemplate!, 0, value, maxDigits, 0, 1);
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
        GameString.String_Free(format);
        Text.Text_AddPrinterWithParamsAndColor(controller.Windows[WinItemDescription], 0, controller.StringBuffer, 0xA0, y, 0xFF, ColorDescription, null);
    }

    // OFFSET 0x0223F584
    private static void BagUI_PrintTMHMMoveStats(BagController controller, ushort item)
    {
        ushort move = Item.Item_MoveForTMHM(item);
        PrintDescriptionString(controller, 0x62, 0, 0);
        PrintDescriptionString(controller, 0x56, 0, 0x10);
        PrintDescriptionString(controller, 0x59, 0x60, 0);
        PrintDescriptionString(controller, 0x57, 0x60, 0x10);
        PrintDescriptionString(controller, 0x58, 0x60, 0x20);
        uint maxPP = MoveTable.MoveTable_CalcMaxPP(move, 0);
        GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x5A)!;
        StringTemplate.StringTemplate_SetNumber(controller.StrTemplate!, 0, (int)maxPP, 2, 1, 1);
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
        GameString.String_Free(format);
        Text.Text_AddPrinterWithParamsAndColor(controller.Windows[WinItemDescription], 0, controller.StringBuffer, 0x30, 0x10, 0xFF, ColorDescription, null);
        int power = (ushort)MoveTable.MoveTable_LoadParam(move, 2);
        PrintMoveStatValue(controller, power, 3, 0x10);
        int accuracy = (ushort)MoveTable.MoveTable_LoadParam(move, 4);
        GameString accuracyFormat = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, accuracy == 0 ? 0x18u : 0x5Bu)!;
        StringTemplate.StringTemplate_SetNumber(controller.StrTemplate!, 0, accuracy, 3, 0, 1);
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, accuracyFormat);
        GameString.String_Free(accuracyFormat);
        Text.Text_AddPrinterWithParamsAndColor(controller.Windows[WinItemDescription], 0, controller.StringBuffer, 0xA0, 0x20, 0xFF, ColorDescription, null);
    }

    // OFFSET 0x0223F7D4
    private static void BagUI_LoadItemCountStrings(BagController controller)
    {
        controller.ItemCountX = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x26);
        controller.ItemCountNumberFmt = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x25);
    }

    // OFFSET 0x0223F800
    private static void BagUI_FreeItemCountStrings(BagController controller)
    {
        GameString.String_Free(controller.ItemCountX);
        GameString.String_Free(controller.ItemCountNumberFmt);
    }

    // OFFSET 0x0223F81C
    private static void BagUI_PrintItemCount(BagController controller, ushort quantity, byte y, uint color)
    {
        uint xPos = CurrentPocket(controller).PocketType == PocketTMs ? 0x73u : 0x6Du;
        Text.Text_AddPrinterWithParamsAndColor(controller.Windows[WinItemList], 0, controller.ItemCountX, xPos, y, 0xFF, color, null);
        GameString count = GameString.String_Init(10, HeapID);
        StringTemplate.StringTemplate_SetNumber(controller.StrTemplate!, 0, quantity, 3, 0, 1);
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, count, controller.ItemCountNumberFmt!);
        int width = Font.Font_CalcStringWidth(0, count, 0);
        Text.Text_AddPrinterWithParamsAndColor(controller.Windows[WinItemList], 0, count, (uint)(0x86 - width), y, 0xFF, color, null);
        GameString.String_Free(count);
    }

    // OFFSET 0x0223F8D0
    private static void BagUI_PrintTMHMNumber(BagController controller, BufferView items, int slot, byte y)
    {
        ushort item = PocketItemAt(items, slot);
        if (item < FirstHMItem)
        {
            FontSpecialCharsContext.FontSpecialChars_DrawPartyScreenText(controller.SpecialChars!, 2, item - (FirstTMItem - 1), 2, 2, controller.Windows[WinItemList], 0, (uint)(y + 5));
            BagUI_PrintItemCount(controller, PocketQuantityAt(items, slot), y, ColorItemList);
            return;
        }
        FontSpecialCharsContext.FontSpecialChars_DrawPartyScreenHPText(controller.SpecialChars!, item - (FirstHMItem - 1), 2, 1, controller.Windows[WinItemList], 0x10, (uint)(y + 5));
        BagUI_PrintHMIcon(controller, y);
    }

    private static ushort PocketItemAt(BufferView items, int slot)
    {
        return items.U16(slot * 4);
    }

    private static ushort PocketQuantityAt(BufferView items, int slot)
    {
        return items.U16(slot * 4 + 2);
    }

    // OFFSET 0x0223F94C
    private static void BagUI_PrintBerryNumber(BagController controller, BufferView items, int slot, byte y)
    {
        int number = Item.Item_BerryNumber(PocketItemAt(items, slot)) + 1;
        FontSpecialCharsContext.FontSpecialChars_DrawPartyScreenText(controller.SpecialChars!, 2, number, 2, 2, controller.Windows[WinItemList], 0, (uint)(y + 5));
        BagUI_PrintItemCount(controller, PocketQuantityAt(items, slot), y, ColorItemList);
    }

    // OFFSET 0x0223F994
    private static byte[] LoadItemListIcons(BagController controller, out CharacterData? charData)
    {
        byte[] data = NARC.NARC_AllocAndReadWholeMember(controller.BagGraphicsNARC!, ListIconsMember, HeapID);
        G2d.GetUnpackedCharacterData(data, out charData);
        return data;
    }

    // OFFSET 0x0223F9B0
    private static void BagUI_PrintHMIcon(BagController controller, byte y)
    {
        byte[] data = LoadItemListIcons(controller, out CharacterData? charData);
        Bg.Window_BlitBitmapRect(controller.Windows[WinItemList], charData!.RawData, 0x28, 0, 0x40, 0x10, 0, y, 0x18, 0x10);
        Heap.Heap_FreeExplicit(HeapID, data);
    }

    // OFFSET 0x0223F9F0
    private static void BagUI_PrintTMHMCancel(BagController controller, byte y)
    {
        GameString cancel = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x29)!;
        Bg.Window_FillRectWithColor(controller.Windows[WinItemList], 0, 0, y, 0x88, 0x10);
        Text.Text_AddPrinterWithParamsAndColor(controller.Windows[WinItemList], 0, cancel, 0, y, 0xFF, ColorItemList, null);
        GameString.String_Free(cancel);
    }

    // OFFSET 0x0223FA44
    private static void BagUI_PrintRegisteredIcon(BagController controller, byte y)
    {
        byte[] data = LoadItemListIcons(controller, out CharacterData? charData);
        Bg.Window_BlitBitmapRect(controller.Windows[WinItemList], charData!.RawData, 0, 0, 0x40, 0x10, 0x60, y, 0x28, 0x10);
        Heap.Heap_FreeExplicit(HeapID, data);
    }

    // OFFSET 0x0223FA88
    private static void BagUI_LoadItemActionStrings(BagController controller)
    {
        for (int i = 0; i < BagController.NumItemActions; i++)
        {
            controller.ItemActionStrings[i] = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, ItemActionStringIDs[i]);
        }
    }

    // OFFSET 0x0223FB50
    private static void BagUI_FreeItemActionStrings(BagController controller)
    {
        for (int i = 0; i < BagController.NumItemActions; i++)
        {
            GameString.String_Free(controller.ItemActionStrings[i]);
        }
    }

    // OFFSET 0x0223FB70
    private static void BagUI_ShowItemActionsMenu(BagController controller, byte[] actions, int count)
    {
        int msgBoxWindow;
        if (CurrentPocket(controller).PocketType == PocketBerries)
        {
            Bg.Window_Add(controller.BgConfig!, controller.ItemActionsWindow, 0, 0x17, (byte)(0x17 - count * 2), 8, (byte)(count * 2), 3, 0x337);
            msgBoxWindow = WinMsgBoxNarrow;
        }
        else
        {
            Bg.Window_Add(controller.BgConfig!, controller.ItemActionsWindow, 0, 0x18, (byte)(0x17 - count * 2), 7, (byte)(count * 2), 3, 0x337);
            msgBoxWindow = WinMsgBox;
        }
        controller.ItemActionChoices = StringList.StringList_New((uint)count, HeapID);
        for (int i = 0; i < count; i++)
        {
            StringList.StringList_AddFromString(controller.ItemActionChoices, controller.ItemActionStrings[actions[i]], GetItemActionFunc(actions[i]));
        }
        MenuTemplate template = new()
        {
            Choices = controller.ItemActionChoices,
            Window = controller.ItemActionsWindow,
            FontID = 0,
            XSize = 1,
            YSize = (byte)count,
            LineSpacing = 0,
            SuppressCursor = 0,
            LoopAround = count >= 4 ? 1 : 0,
        };
        controller.Menu = Menu.Menu_New(template, 8, 0, 0, HeapID, 2);
        BagContext bagCtx = controller.BagCtx!;
        if (CurrentPocket(controller).PocketType == PocketTMs)
        {
            Bg.Window_FillTilemap(controller.Windows[WinItemDescription], 0);
            BagUI_PrintTMHMMoveStats(controller, bagCtx.SelectedItem);
            Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinItemDescription]);
            BagUI_UpdateTypeAndCategoryIcons(controller, bagCtx.SelectedItem, true);
        }
        else
        {
            Window window = controller.Windows[msgBoxWindow];
            RenderWindow.Window_DrawMessageBoxWithScrollCursor(window, 1, MsgBoxTile, MsgBoxPalette);
            Bg.Window_FillTilemap(window, 0xF);
            GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x2A)!;
            GameString message = GameString.String_Init(0x38, HeapID);
            BagApplicationPocket pocket = CurrentPocket(controller);
            BufferPocketSlotItemName(controller, pocket.CursorScroll + pocket.CursorPos - 1, 0);
            StringTemplate.StringTemplate_Format(controller.StrTemplate!, message, format);
            Text.Text_AddPrinterWithParams(window, 1, message, 0, 0, 0xFF, null);
            GameString.String_Free(message);
            GameString.String_Free(format);
            Bg.Window_ScheduleCopyToVRAM(window);
        }
        RenderWindow.Window_DrawStandardFrame(controller.ItemActionsWindow, 1, StandardWindowTile, StandardWindowPalette);
        Bg.Window_ScheduleCopyToVRAM(controller.ItemActionsWindow);
    }

    // OFFSET 0x0223FD84
    private static void BagUI_CloseItemActionsMenu(BagController controller)
    {
        int pocketType = CurrentPocket(controller).PocketType;
        int msgBoxWindow = pocketType == PocketBerries ? WinMsgBoxNarrow : WinMsgBox;
        if (pocketType != PocketTMs)
        {
            RenderWindow.Window_EraseMessageBox(controller.Windows[msgBoxWindow], 1);
            Bg.Window_ClearAndScheduleCopyToVRAM(controller.Windows[msgBoxWindow]);
        }
        RenderWindow.Window_EraseStandardFrame(controller.ItemActionsWindow, 1);
        Bg.Window_ClearAndScheduleCopyToVRAM(controller.ItemActionsWindow);
        Menu.Menu_Free(controller.Menu!, out _);
        StringList.StringList_Free(controller.ItemActionChoices!);
        Bg.Window_Remove(controller.ItemActionsWindow);
        Bg.Window_FillTilemap(controller.Windows[WinItemDescription], 0);
        BagUI_PrintItemDescription(controller, controller.BagCtx!.SelectedItem);
        Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinItemDescription]);
        BagUI_UpdateTypeAndCategoryIcons(controller, 0, false);
    }

    // OFFSET 0x0223FE18
    private static void BagUI_PrintMovingItemMsg(BagController controller)
    {
        Bg.Window_FillTilemap(controller.Windows[WinItemDescription], 0);
        GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x2D)!;
        GameString message = GameString.String_Init(0x82, HeapID);
        BufferPocketSlotItemName(controller, controller.MovedItemPos - 1, 0);
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, message, format);
        Text.Text_AddPrinterWithParamsAndColor(controller.Windows[WinItemDescription], 0, message, 0x28, 0, 0xFF, ColorDescription, null);
        Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinItemDescription]);
        GameString.String_Free(message);
        GameString.String_Free(format);
    }

    // OFFSET 0x0223FE94
    private static void BagUI_ShowItemTrashWindows(BagController controller)
    {
        RenderWindow.Window_DrawStandardFrame(controller.Windows[WinThrowAwayCount], 1, StandardWindowTile, StandardWindowPalette);
        BagUI_PrintItemTrashCount(controller);
        Window window = controller.Windows[WinMsgBox];
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(window, 1, MsgBoxTile, MsgBoxPalette);
        Bg.Window_FillTilemap(window, 0xF);
        GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x34)!;
        GameString message = GameString.String_Init(0x38, HeapID);
        BagApplicationPocket pocket = CurrentPocket(controller);
        BufferPocketSlotItemName(controller, pocket.CursorScroll + pocket.CursorPos - 1, 0);
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, message, format);
        Text.Text_AddPrinterWithParams(window, 1, message, 0, 0, 0xFF, null);
        Bg.Window_ScheduleCopyToVRAM(window);
        GameString.String_Free(message);
        GameString.String_Free(format);
    }

    // OFFSET 0x0223FF44
    private static void BagUI_PrintItemTrashCount(BagController controller)
    {
        Window window = controller.Windows[WinThrowAwayCount];
        Bg.Window_FillTilemap(window, 0xF);
        GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x54)!;
        StringTemplate.StringTemplate_SetNumber(controller.StrTemplate!, 0, controller.SelectedItemCount, 3, 2, 1);
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
        Text.Text_AddPrinterWithParams(window, 1, controller.StringBuffer, 0x10, 8, 0xFF, null);
        Bg.Window_ScheduleCopyToVRAM(window);
        GameString.String_Free(format);
    }

    // OFFSET 0x0223FFC0
    private static void BagUI_CloseItemTrashWindows(BagController controller)
    {
        RenderWindow.Window_EraseMessageBox(controller.Windows[WinMsgBox], 1);
        RenderWindow.Window_EraseStandardFrame(controller.Windows[WinThrowAwayCount], 1);
        Bg.Window_ClearAndScheduleCopyToVRAM(controller.Windows[WinMsgBox]);
        Bg.Window_ClearAndScheduleCopyToVRAM(controller.Windows[WinThrowAwayCount]);
        Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinItemDescription]);
    }

    // OFFSET 0x0223FFF0
    private static void BagUI_PrintConfirmItemTrashMsg(BagController controller)
    {
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(controller.Windows[WinMsgBoxWide], 1, MsgBoxTile, MsgBoxPalette);
        Bg.Window_FillTilemap(controller.Windows[WinMsgBoxWide], 0xF);
        GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x36)!;
        BagApplicationPocket pocket = CurrentPocket(controller);
        if (controller.SelectedItemCount == 1)
        {
            BufferPocketSlotItemName(controller, pocket.CursorScroll + pocket.CursorPos - 1, 0);
        }
        else
        {
            BufferPocketSlotItemNamePlural(controller, pocket.CursorScroll + pocket.CursorPos - 1, 0);
        }
        StringTemplate.StringTemplate_SetNumber(controller.StrTemplate!, 1, controller.SelectedItemCount, 3, 0, 1);
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
        GameString.String_Free(format);
        controller.MsgBoxPrinterID = (byte)BagUI_PrintStrBufferToWideMsgBox(controller);
    }

    // OFFSET 0x022400A0
    private static int BagUI_PrintStrBufferToWideMsgBox(BagController controller)
    {
        RenderText.RenderControlFlags_SetCanABSpeedUpPrint(1);
        RenderText.RenderControlFlags_SetAutoScrollFlags(0);
        int delay = Options.Options_TextFrameDelay(controller.Options!);
        return Text.Text_AddPrinterWithParams(controller.Windows[WinMsgBoxWide], 1, controller.StringBuffer, 0, 0, delay, BagUI_MessagePrinterCallback);
    }

    // OFFSET 0x022400E0
    private static bool BagUI_MessagePrinterCallback(TextPrinter printer, ushort argument)
    {
        switch (argument)
        {
            case 1:
                return SoundPlayback.Sound_IsAnyEffectPlaying();
            case 2:
                return SoundPlayback.Sound_IsBGMPausedByFanfare() != 0;
            case 3:
                SoundPlayback.Sound_PlayEffect(SeqPrinterJingle);
                break;
            case 4:
                return SoundPlayback.Sound_IsEffectPlaying(SeqPrinterJingle) != 0;
        }
        return false;
    }

    // OFFSET 0x02240120
    private static void BagUI_ShowYesNoMenu(BagController controller)
    {
        WindowTemplate template = RomStructs.WindowTemplate(RomData.Overlay(Overlay, YesNoWindowTemplate, 8), 0);
        controller.Menu = Menu.Menu_MakeYesNoChoice(controller.BgConfig!, template, StandardWindowTile, StandardWindowPalette, HeapID);
    }

    // OFFSET 0x02240148
    private static void BagUI_PrintSellCountAndValue(BagController controller, bool update)
    {
        Window window = controller.Windows[WinSellCountValue];
        if (!update)
        {
            RenderWindow.Window_DrawStandardFrame(window, 1, StandardWindowTile, StandardWindowPalette);
        }
        Bg.Window_FillTilemap(window, 0xF);
        GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x50)!;
        StringTemplate.StringTemplate_SetNumber(controller.StrTemplate!, 0, controller.SelectedItemCount, 2, 2, 1);
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
        Text.Text_AddPrinterWithParams(window, 1, controller.StringBuffer, 0, 8, 0xFF, null);
        GameString.String_Free(format);
        format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x51)!;
        StringTemplate.StringTemplate_SetNumber(controller.StrTemplate!, 0, (int)(controller.SelectedItemCount * controller.SoldItemPrice), 6, 1, 1);
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
        int width = Font.Font_CalcStringWidth(0, controller.StringBuffer, 0);
        Text.Text_AddPrinterWithParams(window, 1, controller.StringBuffer, 0x60 - width, 8, 0xFF, null);
        Bg.Window_ScheduleCopyToVRAM(window);
        GameString.String_Free(format);
    }

    // OFFSET 0x02240248
    private static void BagUI_PrintMoney(BagController controller, bool update)
    {
        Window window = controller.Windows[WinMoney];
        GameString format;
        if (!update)
        {
            Bg.Window_FillTilemap(window, 0xF);
            RenderWindow.Window_DrawStandardFrame(window, 1, StandardWindowTile, StandardWindowPalette);
            format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x4E)!;
            Text.Text_AddPrinterWithParams(window, 1, format, 0, 0, 0xFF, null);
            GameString.String_Free(format);
        }
        else
        {
            Bg.Window_FillRectWithColor(window, 0xF, 0, 0x10, 0x50, 0x10);
        }
        format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x4F)!;
        StringTemplate.StringTemplate_SetNumber(controller.StrTemplate!, 0, (int)TrainerInfo.TrainerInfo_Money(controller.TrainerInfo!), 6, 1, 1);
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
        int width = Font.Font_CalcStringWidth(0, controller.StringBuffer, 0);
        Text.Text_AddPrinterWithParams(window, 1, controller.StringBuffer, 0x50 - width, 0x10, 0xFF, null);
        Bg.Window_ScheduleCopyToVRAM(window);
        GameString.String_Free(format);
    }

    // OFFSET 0x02240328
    private static void BagUI_DrawPoffinCountMsgBox(BagController controller)
    {
        Window window = controller.Windows[WinPoffinCount];
        Bg.Window_FillTilemap(window, 0xF);
        RenderWindow.Window_DrawStandardFrame(window, 1, StandardWindowTile, StandardWindowPalette);
        GameString title = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x6C)!;
        Text.Text_AddPrinterWithParams(window, 1, title, 0, 0, 0xFF, null);
        GameString.String_Free(title);
        GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x6D)!;
        int count = PoffinCase.PoffinCase_CountFilledSlots(PoffinCase.SaveData_GetPoffinCase(controller.BagCtx!.SaveData!));
        StringTemplate.StringTemplate_SetNumber(controller.StrTemplate!, 0, count, 3, 1, 1);
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
        GameString.String_Free(format);
        int width = Font.Font_CalcStringWidth(0, controller.StringBuffer, 0);
        Text.Text_AddPrinterWithParams(window, 1, controller.StringBuffer, 0x58 - width, 0x10, 0xFF, null);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x022403F4
    private static void BagUI_InitSprites(BagController controller)
    {
        GXLayers.GXLayers_EngineAToggleLayers(0x10, 1);
        GXLayers.GXLayers_EngineBToggleLayers(0x10, 1);
        VramTransfer.VramTransfer_New(0x20, HeapID);
        InitSpriteSystem(controller);
        LoadSpriteResources(controller);
        InitSprites(controller);
    }

    private static RenderOamTemplate ReadRenderOamTemplate()
    {
        uint[] v = RomData.OverlayU32(Overlay, RenderOamTemplateAddress, 8);
        return new RenderOamTemplate { MainOamStart = (int)v[0], MainOamCount = (int)v[1], MainAffineOamStart = (int)v[2], MainAffineOamCount = (int)v[3], SubOamStart = (int)v[4], SubOamCount = (int)v[5], SubAffineOamStart = (int)v[6], SubAffineOamCount = (int)v[7] };
    }

    private static CharTransferTemplateWithModes ReadCharTransferTemplate()
    {
        uint[] v = RomData.OverlayU32(Overlay, CharTransferTemplateAddress, 5);
        return new CharTransferTemplateWithModes { MaxTasks = (int)v[0], SizeMain = (int)v[1], SizeSub = (int)v[2], ModeMain = (int)v[3], ModeSub = (int)v[4] };
    }

    // OFFSET 0x02240424
    private static void InitSpriteSystem(BagController controller)
    {
        uint[] raw = RomData.OverlayU32(Overlay, SpriteCapacitiesAddress, 6);
        int[] capacities = new int[raw.Length];
        for (int i = 0; i < raw.Length; i++)
        {
            capacities[i] = (int)raw[i];
        }
        controller.SpriteSystem = SpriteSystem.SpriteSystem_Alloc(HeapID);
        controller.SpriteMan = SpriteSystem.SpriteManager_New(controller.SpriteSystem);
        SpriteSystem.SpriteSystem_Init(controller.SpriteSystem, ReadRenderOamTemplate(), ReadCharTransferTemplate(), 0x20);
        SpriteSystem.SpriteSystem_InitSprites(controller.SpriteSystem, controller.SpriteMan, NumSpritesForInit);
        SpriteSystem.SpriteSystem_InitManagerWithCapacities(controller.SpriteSystem, controller.SpriteMan, capacities);
    }

    private const int NumSpritesForInit = 0xC;

    // OFFSET 0x022404C0
    private static void LoadSpriteResources(BagController controller)
    {
        SpriteSystem system = controller.SpriteSystem!;
        SpriteManager manager = controller.SpriteMan!;
        NARC narc = controller.BagGraphicsNARC!;
        int bagChar;
        int bagPltt;
        if (controller.TrainerGender == 0)
        {
            bagChar = 2;
            bagPltt = 3;
        }
        else
        {
            bagChar = 6;
            bagPltt = 7;
        }
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(system, manager, narc, bagChar, 0, 1, ResourceBag);
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(system, manager, narc, 0x19, 0, 1, ResourcePocketHighlight);
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(system, manager, narc, 0x1C, 0, 1, ResourceArrows);
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(system, manager, narc, 0x1F, 0, 1, ResourceItemHighlight);
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(system, manager, narc, 0xA, 0, 1, ResourceMovingBar);
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(system, manager, narc, 0x24, 0, 2, ResourceShockwave);
        SpriteSystem.SpriteSystem_LoadCharResObj(system, manager, 0x3E, 4, 0, 1, ResourceCountArrows);
        SpriteSystem.SpriteSystem_LoadCharResObj(system, manager, ItemGraphicsArchive, Item.Item_FileID(0, 1), 0, 1, ResourceItem);
        TypeIcon.TypeIcon_LoadChar(system, manager, 1, 0, ResourceMoveType);
        CategoryIcon.CategoryIcon_LoadChar(system, manager, 1, 0, ResourceMoveCategory);
        SpriteSystem.SpriteSystem_LoadPlttResObjFromOpenNarc(system, manager, narc, bagPltt, 0, 1, 1, ResourceBag);
        SpriteSystem.SpriteSystem_LoadPlttResObjFromOpenNarc(system, manager, narc, 0x14, 0, 2, 1, ResourcePocketHighlight);
        SpriteSystem.SpriteSystem_LoadPlttResObj(system, manager, ItemGraphicsArchive, Item.Item_FileID(0, 2), 0, 1, 1, ResourceItemHighlight);
        SpriteSystem.SpriteSystem_LoadPlttResObj(system, manager, 0x3E, 0xA, 0, 2, 1, ResourceMovingBar);
        TypeIcon.TypeIcon_LoadPltt(system, manager, 1, ResourceShockwave);
        SpriteSystem.SpriteSystem_LoadPlttResObjFromOpenNarc(system, manager, narc, 0x25, 0, 1, 2, ResourceArrows);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(system, manager, narc, 1, 0, ResourceBag);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(system, manager, narc, 0x18, 0, ResourcePocketHighlight);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(system, manager, narc, 0x1B, 0, ResourceArrows);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(system, manager, narc, 0x1E, 0, ResourceItemHighlight);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(system, manager, narc, 9, 0, ResourceMovingBar);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(system, manager, narc, 0x23, 0, ResourceShockwave);
        SpriteSystem.SpriteSystem_LoadCellResObj(system, manager, ItemGraphicsArchive, Item.Item_IconCellFile(), 0, ResourceItem);
        SpriteSystem.SpriteSystem_LoadCellResObj(system, manager, 0x3E, 5, 0, ResourceCountArrows);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(system, manager, narc, 0, 0, ResourceBag);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(system, manager, narc, 0x17, 0, ResourcePocketHighlight);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(system, manager, narc, 0x1A, 0, ResourceArrows);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(system, manager, narc, 0x1D, 0, ResourceItemHighlight);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(system, manager, narc, 8, 0, ResourceMovingBar);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(system, manager, narc, 0x22, 0, ResourceShockwave);
        SpriteSystem.SpriteSystem_LoadAnimResObj(system, manager, ItemGraphicsArchive, Item.Item_IconAnimFile(), 0, ResourceItem);
        SpriteSystem.SpriteSystem_LoadAnimResObj(system, manager, 0x3E, 6, 0, ResourceCountArrows);
        TypeIcon.TypeIcon_LoadAnim(system, manager, ResourceMoveType, ResourceMoveType);
    }

    private static SpriteTemplate ReadSpriteTemplate(int index)
    {
        byte[] raw = RomData.Overlay(Overlay, (uint)(SpriteTemplatesAddress + index * SpriteTemplateSize), SpriteTemplateSize);
        SpriteTemplate template = new()
        {
            X = BinaryPrimitives.ReadInt16LittleEndian(raw.AsSpan(0)),
            Y = BinaryPrimitives.ReadInt16LittleEndian(raw.AsSpan(2)),
            Z = BinaryPrimitives.ReadInt16LittleEndian(raw.AsSpan(4)),
            AnimIdx = BinaryPrimitives.ReadUInt16LittleEndian(raw.AsSpan(6)),
            Priority = BinaryPrimitives.ReadInt32LittleEndian(raw.AsSpan(8)),
            PlttIdx = BinaryPrimitives.ReadInt32LittleEndian(raw.AsSpan(0xC)),
            VramType = BinaryPrimitives.ReadInt32LittleEndian(raw.AsSpan(0x10)),
            BgPriority = BinaryPrimitives.ReadInt32LittleEndian(raw.AsSpan(0x2C)),
            VramTransfer = BinaryPrimitives.ReadInt32LittleEndian(raw.AsSpan(0x30)),
        };
        for (int i = 0; i < template.Resources.Length; i++)
        {
            template.Resources[i] = BinaryPrimitives.ReadInt32LittleEndian(raw.AsSpan(0x14 + i * 4));
        }
        return template;
    }

    // OFFSET 0x02240950
    private static void InitSprites(BagController controller)
    {
        for (int i = 0; i < BagController.NumSprites; i++)
        {
            controller.Sprites[i] = SpriteSystem.SpriteSystem_NewSprite(controller.SpriteSystem!, controller.SpriteMan!, ReadSpriteTemplate(i));
        }
        SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteMovingItemPosBar]!, 0);
        SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteShockwave]!, 0);
        SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteMoveType]!, 0);
        SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteMoveCategory]!, 0);
        if ((byte)(controller.BagCtx!.Mode - ModePoffins) <= 1)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpritePocketHighlight]!, 0);
        }
        if (controller.NumPockets == 1)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteLeftArrow]!, 0);
            SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteRightArrow]!, 0);
        }
        BagUI_ToggleItemCountArrows(controller, 0);
        SpriteSystem.ManagedSprite_SetAnim(controller.Sprites[SpriteBag]!, CurrentPocket(controller).PocketType);
        int x = CalcPocketHighlightXForPocket(controller, controller.BagCtx.CurrPocketIdx);
        SpriteSystem.ManagedSprite_SetPositionXY(controller.Sprites[SpritePocketHighlight]!, (short)x, PocketHighlightY);
        SetItemHighlightRow(controller, SpriteItemHighlight, CurrentPocket(controller).CursorPos, 0x18);
        uint[] scale = RomData.OverlayU32(Overlay, BagSpriteScale, 3);
        VecFx32 bagScale = new() { X = (int)scale[0], Y = (int)scale[1], Z = (int)scale[2] };
        Sprite.Sprite_SetAffineScaleEx(controller.Sprites[SpriteBag]!.Sprite!, bagScale, 2);
    }

    // OFFSET 0x02240A88
    private static void BagUI_FreeSprites(BagController controller)
    {
        for (int i = 0; i < BagController.NumSprites; i++)
        {
            SpriteSystem.Sprite_DeleteAndFreeResources(controller.Sprites[i]!);
        }
        SpriteSystem.SpriteSystem_FreeResourcesAndManager(controller.SpriteSystem!, controller.SpriteMan!);
        SpriteSystem.SpriteSystem_Free(controller.SpriteSystem!);
    }

    // OFFSET 0x02240ABC
    private static void BagUI_TickSpriteAnimations(BagController controller)
    {
        for (int i = 0; i < BagController.NumSprites; i++)
        {
            SpriteSystem.ManagedSprite_TickFrame(controller.Sprites[i]!);
        }
    }

    // OFFSET 0x02240AD8
    private static void BagUI_UpdateItemSprite(BagController controller, ushort item)
    {
        SpriteSystem.SpriteSystem_ReplaceCharResObj(controller.SpriteSystem!, controller.SpriteMan!, ItemGraphicsArchive, Item.Item_FileID(item, 1), 0, ResourceItem);
        SpriteSystem.SpriteSystem_ReplacePlttResObj(controller.SpriteSystem!, controller.SpriteMan!, ItemGraphicsArchive, Item.Item_FileID(item, 2), 0, ResourceItemHighlight);
    }

    // OFFSET 0x02240B34
    private static void BagUI_SetHighlightSpritesPalette(BagController controller, uint palette)
    {
        SpriteSystem.ManagedSprite_SetExplicitPalette(controller.Sprites[SpriteItemHighlight]!, palette);
        SpriteSystem.ManagedSprite_SetExplicitPalette(controller.Sprites[SpritePocketHighlight]!, palette);
    }

    // OFFSET 0x02240B50
    private static bool BagUI_IsPocketHighlightDoneMoving(BagController controller)
    {
        return controller.PocketHighlight.IsMoving == 0;
    }

    // OFFSET 0x02240B68
    private static void BagUI_StartMovingPocketHighlight(BagController controller)
    {
        controller.PocketHighlight.CurrentStep = 0;
        controller.PocketHighlight.IsMoving = 1;
        CalcPocketHighlightMovement(controller);
    }

    // OFFSET 0x02240B80
    private static void BagUI_StepPocketHighlightMovement(BagController controller)
    {
        if (controller.PocketHighlight.IsMoving == 1)
        {
            StepPocketHighlightMovingAnim(controller);
        }
    }

    // OFFSET 0x02240B98
    private static void StepPocketHighlightMovingAnim(BagController controller)
    {
        BagPocketHighlight highlight = controller.PocketHighlight;
        Sprite sprite = controller.Sprites[SpritePocketHighlight]!.Sprite!;
        VecFx32 position = Sprite.Sprite_GetPosition(sprite);
        if (highlight.Direction == 0)
        {
            position.X -= highlight.Positions[highlight.CurrentStep];
        }
        else
        {
            position.X += highlight.Positions[highlight.CurrentStep];
        }
        Sprite.Sprite_SetPosition(sprite, position);
        highlight.CurrentStep++;
        if (highlight.CurrentStep == 8)
        {
            position.X = CalcPocketHighlightXForPocket(controller, controller.NextPocket) << 12;
            Sprite.Sprite_SetPosition(sprite, position);
            highlight.IsMoving = 0;
        }
    }

    // OFFSET 0x02240C30
    private static byte CalcPocketHighlightXForPocket(BagController controller, int pocketIdx)
    {
        return (byte)(controller.PocketSelectorIconsX + controller.PocketSelectorIconsSpacing * pocketIdx + 6);
    }

    // OFFSET 0x02240C48
    private static void CalcPocketHighlightMovement(BagController controller)
    {
        BagPocketHighlight highlight = controller.PocketHighlight;
        VecFx32 position = Sprite.Sprite_GetPosition(controller.Sprites[SpritePocketHighlight]!.Sprite!);
        int target = CalcPocketHighlightXForPocket(controller, controller.NextPocket) << 12;
        int step;
        if (target < position.X)
        {
            step = (position.X - target) / 100;
            highlight.Direction = 0;
        }
        else
        {
            step = (target - position.X) / 100;
            highlight.Direction = 1;
        }
        highlight.Positions[0] = 0;
        highlight.Positions[1] = step * 40;
        highlight.Positions[2] = step * 25;
        highlight.Positions[3] = step * 15;
        highlight.Positions[4] = step * 10;
        highlight.Positions[5] = step * 7;
        highlight.Positions[6] = step * 3;
        highlight.Positions[7] = 0;
    }

    // OFFSET 0x02240CF0
    private static void BagUI_ShowItemCountArrows(BagController controller, int position)
    {
        if (position == 0)
        {
            SpriteSystem.ManagedSprite_SetPositionXY(controller.Sprites[SpriteItemCountArrowUp]!, 0xDC, 0x9C);
            SpriteSystem.ManagedSprite_SetPositionXY(controller.Sprites[SpriteItemCountArrowDown]!, 0xDC, 0xB4);
        }
        else
        {
            SpriteSystem.ManagedSprite_SetPositionXY(controller.Sprites[SpriteItemCountArrowUp]!, 0xA2, 0x6C);
            SpriteSystem.ManagedSprite_SetPositionXY(controller.Sprites[SpriteItemCountArrowDown]!, 0xA2, 0x84);
        }
        BagUI_ToggleItemCountArrows(controller, 1);
    }

    // OFFSET 0x02240D3C
    private static void BagUI_ToggleItemCountArrows(BagController controller, int draw)
    {
        SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteItemCountArrowUp]!, draw);
        SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteItemCountArrowDown]!, draw);
    }

    // OFFSET 0x02240D5C
    private static void BagUI_UpdateTypeAndCategoryIcons(BagController controller, ushort item, bool show)
    {
        SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteMoveType]!, show ? 1 : 0);
        SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteMoveCategory]!, show ? 1 : 0);
        if (!show)
        {
            return;
        }
        ushort move = Item.Item_MoveForTMHM(item);
        int type = (ushort)MoveTable.MoveTable_LoadParam(move, 3);
        int moveClass = (ushort)MoveTable.MoveTable_LoadParam(move, 1);
        SpriteSystem.SpriteSystem_ReplaceCharResObj(controller.SpriteSystem!, controller.SpriteMan!, TypeIcon.TypeIcon_GetNARC(), (int)TypeIcon.TypeIcon_GetChar(type), 1, ResourceMoveType);
        SpriteSystem.ManagedSprite_SetExplicitPalette(controller.Sprites[SpriteMoveType]!, (uint)(TypeIcon.TypeIcon_GetPlttIndex(type) + 6));
        SpriteSystem.SpriteSystem_ReplaceCharResObj(controller.SpriteSystem!, controller.SpriteMan!, CategoryIcon.CategoryIcon_GetNARC(), (int)CategoryIcon.CategoryIcon_GetChar(moveClass), 1, ResourceMoveCategory);
        SpriteSystem.ManagedSprite_SetExplicitPalette(controller.Sprites[SpriteMoveCategory]!, (uint)(CategoryIcon.CategoryIcon_GetPlttIndex(moveClass) + 6));
    }

    // OFFSET 0x02240E24
    private static void BagUI_DrawBtnShockwave(BagController controller, short x, short y)
    {
        ManagedSprite sprite = controller.Sprites[SpriteShockwave]!;
        SpriteSystem.ManagedSprite_SetDrawFlag(sprite, 1);
        SpriteSystem.ManagedSprite_SetPositionXY(sprite, x, y);
        SpriteSystem.ManagedSprite_SetAnimationFrame(sprite, 0);
        SpriteSystem.ManagedSprite_SetAnim(sprite, 0);
    }

    // OFFSET 0x02240E5C
    private static void BagUI_TickBtnShockwaveAnim(BagController controller)
    {
        ManagedSprite sprite = controller.Sprites[SpriteShockwave]!;
        if (SpriteSystem.ManagedSprite_GetDrawFlag(sprite) == 1)
        {
            SpriteSystem.ManagedSprite_TickNFrames(sprite, 0x1000);
            if (SpriteSystem.ManagedSprite_GetAnimationFrame(sprite) == 2)
            {
                SpriteSystem.ManagedSprite_SetDrawFlag(sprite, 0);
            }
        }
    }
}
