namespace PlatinumSharp.Game;

public static partial class BattleBags
{
    private const uint MessageBoxWindowTemplate = 0x02229A60;
    private const uint MenuScreenWindowTemplates = 0x02229A88;
    private const uint PocketMenuScreenWindowTemplates = 0x02229AE0;
    private const uint UseItemScreenWindowTemplates = 0x02229A68;
    private const uint PocketItemMessageIDs = 0x02229AB0;
    private const int WindowTemplateSize = 8;
    private const uint ColorButton = 0x30201;
    private const uint ColorText = 0x10200;
    private const int FontSystem = 0;
    private const int FontMessage = 1;
    private const int WindowPocketName = 0x18;
    private const int WindowPocketPageNums = 0x19;
    private const int AltPocketMenuWindowsOffset = 0xC;
    private const ushort MessageBoxTile = 0x3E2;

    private static Window BagWindow(BattleBag battleBag, int index)
    {
        return battleBag.Windows![index];
    }

    private static void PrintBagText(Window window, int font, GameString text, int x, int y, uint color)
    {
        Text.Text_AddPrinterWithParamsAndColor(window, font, text, (uint)x, (uint)y, 0xFF, color, null);
    }

    private static BattleBagItem PocketItem(BattleBag battleBag, int index)
    {
        return battleBag.Items[battleBag.CurrentBattlePocket][index];
    }

    private static int PocketItemMessageID(int slot, int field)
    {
        return (int)BattleSubMenuData.U32((uint)(PocketItemMessageIDs + slot * 8 + field * 4));
    }

    // OFFSET 0x02227288
    private static void BattleBagText_InitializeWindows(BattleBag battleBag)
    {
        Bg.Window_AddFromTemplate(battleBag.Background!, battleBag.MessageBoxWindow, RomStructs.WindowTemplate(BattleSubMenuData.Bytes(MessageBoxWindowTemplate, WindowTemplateSize), 0));
        BattleBagText_InitializeScreenWindows(battleBag, battleBag.CurrentScreen);
    }

    // OFFSET 0x022272AC
    private static void BattleBagText_InitializeScreenWindows(BattleBag battleBag, int screen)
    {
        uint templates = 0;
        switch (screen)
        {
            case ScreenMenu:
                battleBag.NumWindows = 5;
                templates = MenuScreenWindowTemplates;
                break;
            case ScreenPocketMenu:
                battleBag.NumWindows = 0x1A;
                templates = PocketMenuScreenWindowTemplates;
                break;
            case ScreenUseItem:
                templates = UseItemScreenWindowTemplates;
                battleBag.NumWindows = 4;
                break;
        }
        battleBag.Windows = Bg.Window_New(battleBag.Context!.HeapID, battleBag.NumWindows);
        for (byte i = 0; i < battleBag.NumWindows; i++)
        {
            Bg.Window_AddFromTemplate(battleBag.Background!, battleBag.Windows[i], RomStructs.WindowTemplate(BattleSubMenuData.Bytes((uint)(templates + i * WindowTemplateSize), WindowTemplateSize), 0));
        }
    }

    // OFFSET 0x02227324
    private static void BattleBagText_ClearScreenWindows(BattleBag battleBag)
    {
        Bg.Windows_Delete(battleBag.Windows!, battleBag.NumWindows);
    }

    // OFFSET 0x02227334
    private static void BattleBagText_ClearWindows(BattleBag battleBag)
    {
        Bg.Windows_Delete(battleBag.Windows!, battleBag.NumWindows);
        Bg.Window_Remove(battleBag.MessageBoxWindow);
    }

    // OFFSET 0x02227350
    private static void BattleBagText_ChangeScreen(BattleBag battleBag, int screen)
    {
        switch (screen)
        {
            case ScreenMenu:
                RenderMenuScreen(battleBag);
                break;
            case ScreenPocketMenu:
                RenderPocketMenuScreen(battleBag);
                break;
            case ScreenUseItem:
                RenderUseItemScreen(battleBag);
                break;
        }
    }

    // OFFSET 0x02227374
    private static void PrintTextToWindow(BattleBag battleBag, int windowIndex, int messageID, int font, int y, uint color)
    {
        Window window = BagWindow(battleBag, windowIndex);
        GameString text = MessageLoader.MessageLoader_GetNewString(battleBag.MessageLoader!, (uint)messageID)!;
        int width = Font.Font_CalcStringWidth(font, text, 0);
        int x = (int)((uint)(Bg.Window_GetWidth(window) * 8 - width) >> 1);
        PrintBagText(window, font, text, x, y, color);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x022273CC
    private static void RenderMenuScreen(BattleBag battleBag)
    {
        for (int i = 0; i < 5; i++)
        {
            Bg.Window_FillTilemap(BagWindow(battleBag, i), 0);
        }
        PrintTextToWindow(battleBag, 0, 0, FontSubscreen, 8, ColorButton);
        PrintTextToWindow(battleBag, 0, 1, FontSubscreen, 0x18, ColorButton);
        PrintTextToWindow(battleBag, 1, 2, FontSubscreen, 8, ColorButton);
        PrintTextToWindow(battleBag, 1, 3, FontSubscreen, 0x18, ColorButton);
        PrintTextToWindow(battleBag, 2, 7, FontSubscreen, 8, ColorButton);
        PrintTextToWindow(battleBag, 3, 6, FontSubscreen, 8, ColorButton);
        if (battleBag.Context!.LastUsedItem == 0)
        {
            return;
        }
        GameString text = MessageLoader.MessageLoader_GetNewString(battleBag.MessageLoader!, 8)!;
        PrintBagText(BagWindow(battleBag, 4), FontSubscreen, text, 0, 6, ColorButton);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(BagWindow(battleBag, 4));
    }

    // OFFSET 0x022274A8
    private static void PrintPocketItemNameToWindow(BattleBag battleBag, int itemIndex, int slot, int windowIndex, int font, uint color)
    {
        Window window = BagWindow(battleBag, windowIndex);
        Bg.Window_FillTilemap(window, 0);
        BattleBagItem entry = PocketItem(battleBag, itemIndex);
        if (entry.Item != 0)
        {
            GameString format = MessageLoader.MessageLoader_GetNewString(battleBag.MessageLoader!, (uint)PocketItemMessageID(slot, 0))!;
            StringTemplate.StringTemplate_SetItemName(battleBag.StringTemplate!, 0, entry.Item);
            StringTemplate.StringTemplate_Format(battleBag.StringTemplate!, battleBag.String!, format);
            int width = Font.Font_CalcStringWidth(font, battleBag.String!, 0);
            int x = (int)((uint)(Bg.Window_GetWidth(window) * 8 - width) >> 1);
            PrintBagText(window, font, battleBag.String!, x, 8, color);
            GameString.String_Free(format);
        }
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x0222754C
    private static void PrintPocketItemAmountToWindow(BattleBag battleBag, int itemIndex, int slot, int windowIndex, int font, int y, uint color)
    {
        Window window = BagWindow(battleBag, windowIndex);
        Bg.Window_FillTilemap(window, 0);
        BattleBagItem entry = PocketItem(battleBag, itemIndex);
        if (entry.Quantity != 0)
        {
            GameString format = MessageLoader.MessageLoader_GetNewString(battleBag.MessageLoader!, (uint)PocketItemMessageID(slot, 1))!;
            StringTemplate.StringTemplate_SetNumber(battleBag.StringTemplate!, 0, entry.Quantity, 3, 0, 1);
            StringTemplate.StringTemplate_Format(battleBag.StringTemplate!, battleBag.String!, format);
            PrintBagText(window, font, battleBag.String!, 0, y, color);
            GameString.String_Free(format);
        }
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x022275E0
    private static void PrintPocketItemInfo(BattleBag battleBag, int slot)
    {
        int pageOffset = battleBag.Context!.PocketCurrentPages[battleBag.CurrentBattlePocket] * ItemsPerPage;
        int windowOffset = battleBag.UseAltPocketMenuWindows == 0 ? 0 : AltPocketMenuWindowsOffset;
        PrintPocketItemNameToWindow(battleBag, slot + pageOffset, slot, windowOffset + slot * 2, FontSubscreen, ColorButton);
        PrintPocketItemAmountToWindow(battleBag, slot + pageOffset, slot, windowOffset + 1 + slot * 2, FontSystem, 4, ColorText);
    }

    // OFFSET 0x02227650
    private static void BattleBagText_PrintAllPocketItemInfo(BattleBag battleBag)
    {
        Bg.Bg_FillTilemapRect(battleBag.Background!, 5, 0, 0, 0, 0x20, 0x13, 0x11);
        for (ushort i = 0; i < ItemsPerPage; i++)
        {
            PrintPocketItemInfo(battleBag, i);
        }
        battleBag.UseAltPocketMenuWindows ^= 1;
    }

    // OFFSET 0x02227698
    private static void BattleBagText_PrintPocketPageNums(BattleBag battleBag)
    {
        Window window = BagWindow(battleBag, WindowPocketPageNums);
        Bg.Window_FillTilemap(window, 0);
        GameString slash = MessageLoader.MessageLoader_GetNewString(battleBag.MessageLoader!, 0x1C)!;
        int slashWidth = Font.Font_CalcStringWidth(FontSystem, slash, 0);
        int x = (int)((uint)(Bg.Window_GetWidth(window) * 8 - slashWidth) >> 1);
        PrintBagText(window, FontSystem, slash, x, 4, ColorText);
        GameString.String_Free(slash);
        GameString format = MessageLoader.MessageLoader_GetNewString(battleBag.MessageLoader!, 0x1D)!;
        StringTemplate.StringTemplate_SetNumber(battleBag.StringTemplate!, 0, battleBag.NumBattlePocketPages[battleBag.CurrentBattlePocket] + 1, 2, 0, 1);
        StringTemplate.StringTemplate_Format(battleBag.StringTemplate!, battleBag.String!, format);
        PrintBagText(window, FontSystem, battleBag.String!, x + slashWidth, 4, ColorText);
        GameString.String_Free(format);
        format = MessageLoader.MessageLoader_GetNewString(battleBag.MessageLoader!, 0x1E)!;
        StringTemplate.StringTemplate_SetNumber(battleBag.StringTemplate!, 0, battleBag.Context!.PocketCurrentPages[battleBag.CurrentBattlePocket] + 1, 2, 0, 1);
        StringTemplate.StringTemplate_Format(battleBag.StringTemplate!, battleBag.String!, format);
        int width = Font.Font_CalcStringWidth(FontSystem, battleBag.String!, 0);
        PrintBagText(window, FontSystem, battleBag.String!, x - width, 4, ColorText);
        GameString.String_Free(format);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x022277C8
    private static void PrintPocketName(BattleBag battleBag)
    {
        Bg.Window_FillTilemap(BagWindow(battleBag, WindowPocketName), 0);
        switch (battleBag.CurrentBattlePocket)
        {
            case PocketRecoverHP:
                PrintTextToWindow(battleBag, WindowPocketName, 0x16, FontSystem, 4, ColorText);
                PrintTextToWindow(battleBag, WindowPocketName, 0x17, FontSystem, 0x14, ColorText);
                break;
            case PocketRecoverStatus:
                PrintTextToWindow(battleBag, WindowPocketName, 0x18, FontSystem, 4, ColorText);
                PrintTextToWindow(battleBag, WindowPocketName, 0x19, FontSystem, 0x14, ColorText);
                break;
            case PocketPokeBalls:
                PrintTextToWindow(battleBag, WindowPocketName, 0x1A, FontSystem, 0xC, ColorText);
                break;
            case PocketBattleItems:
                PrintTextToWindow(battleBag, WindowPocketName, 0x1B, FontSystem, 0xC, ColorText);
                break;
        }
    }

    // OFFSET 0x02227888
    private static void RenderPocketMenuScreen(BattleBag battleBag)
    {
        BattleBagText_PrintAllPocketItemInfo(battleBag);
        PrintPocketName(battleBag);
        BattleBagText_PrintPocketPageNums(battleBag);
    }

    // OFFSET 0x022278A0
    private static void PrintUseItemName(BattleBag battleBag, int itemIndex)
    {
        Window window = BagWindow(battleBag, 0);
        GameString format = MessageLoader.MessageLoader_GetNewString(battleBag.MessageLoader!, 9)!;
        StringTemplate.StringTemplate_SetItemName(battleBag.StringTemplate!, 0, PocketItem(battleBag, itemIndex).Item);
        StringTemplate.StringTemplate_Format(battleBag.StringTemplate!, battleBag.String!, format);
        Bg.Window_FillTilemap(window, 0);
        PrintBagText(window, FontSystem, battleBag.String!, 0, 0, ColorText);
        GameString.String_Free(format);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02227910
    private static void PrintUseItemDesc(BattleBag battleBag, int itemIndex)
    {
        Window window = BagWindow(battleBag, 2);
        GameString text = GameString.String_Init(0x82, battleBag.Context!.HeapID);
        Item.Item_LoadDescription(text, PocketItem(battleBag, itemIndex).Item, (ushort)battleBag.Context.HeapID);
        PrintBagText(window, FontSystem, text, 4, 0, ColorText);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02227974
    private static void RenderUseItemScreen(BattleBag battleBag)
    {
        for (int i = 0; i < 4; i++)
        {
            Bg.Window_FillTilemap(BagWindow(battleBag, i), 0);
        }
        BattleBagContext context = battleBag.Context!;
        int position = context.PocketCurrentPagePositions[battleBag.CurrentBattlePocket];
        int pageOffset = context.PocketCurrentPages[battleBag.CurrentBattlePocket] * ItemsPerPage;
        PrintUseItemName(battleBag, position + pageOffset);
        PrintPocketItemAmountToWindow(battleBag, position + pageOffset, 0, 1, FontSystem, 0, ColorText);
        PrintUseItemDesc(battleBag, position + pageOffset);
        PrintTextToWindow(battleBag, 3, 0x1F, FontSubscreen, 6, ColorButton);
    }

    // OFFSET 0x022279F4
    private static void BattleBagText_DisplayMessage(BattleBag battleBag)
    {
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(battleBag.MessageBoxWindow, 1, MessageBoxTile, 0xE);
        Bg.Window_FillTilemap(battleBag.MessageBoxWindow, 0xF);
        BattleBagText_PrintToMessageBox(battleBag);
    }

    // OFFSET 0x02227A1C
    private static void BattleBagText_PrintToMessageBox(BattleBag battleBag)
    {
        RenderText.RenderControlFlags_SetCanABSpeedUpPrint(1);
        int delay = BattleMessages.BattleSystem_GetTextSpeed(battleBag.Context!.BattleSys!);
        battleBag.TextPrinterID = (byte)Text.Text_AddPrinterWithParams(battleBag.MessageBoxWindow, FontMessage, battleBag.String, 0, 0, delay, null);
    }
}
