namespace PlatinumSharp.Game;

public sealed class FieldMenuManager
{
    public const int EntriesMax = 0x1C;

    public FieldSystem? FieldSystem;
    public SysTask? SysTask;
    public readonly Window MenuWindow = new();
    public Window? ParentWindow;
    public readonly GameString?[] ChoicesStringsBuffers = new GameString?[EntriesMax];
    public MessageLoader? MessageLoader;
    public StringTemplate? StringTemplate;
    public byte SysTaskDelay;
    public byte InitialCursorPos;
    public byte CanExitWithB;
    public byte FreeMsgLoaderOnDelete;
    public byte AnchorRight;
    public byte AnchorBottom;
    public byte AnchorX;
    public byte AnchorY;
    public byte OptionsCount;
    public ushort[]? SelectedOptionPtr;
    public ushort[]? ListMenuListOffsetPtr;
    public ushort[]? ListMenuCursorPosPtr;
    public readonly MenuTemplate MenuTemplate = new();
    public Menu? Menu;
    public readonly StringListEntry[] MenuChoicesStrings = Enumerable.Range(0, EntriesMax).Select(_ => new StringListEntry()).ToArray();
    public readonly ListMenuTemplate ListMenuTemplate = new();
    public ListMenu? ListMenu;
    public ushort ListMenuAltTextIndex;
    public readonly StringListEntry[] ListMenuChoicesStrings = Enumerable.Range(0, EntriesMax).Select(_ => new StringListEntry()).ToArray();
    public readonly ushort[] ChoicesAltTextStringIDs = new ushort[EntriesMax];
    public ushort CursorPos;
}

public static class FieldMenus
{
    private const int HeapIDField = 4;
    private const int TextBankMenuEntries = 0x169;
    private const int TextBankMoney = 0x21F;
    private const int BgLayerMain3 = 3;
    private const ushort BaseTileStandardWindowFrame = 0x3D9;
    private const int StandardWindowPalette = 0xB;
    private const byte MenuWindowPalette = 0xD;
    private const ushort MenuWindowBaseTile = 0x3D;
    private const int MenuLoopAroundMinOptions = 4;
    private const int ListMenuMaxDisplay = 8;
    private const int GlyphRowTilesHeight = 2;
    private const int CursorMarginSize = 0xC;
    private const ushort SeqConfirm = 0x5DC;
    private const ushort MenuCancel = 0xFFFE;
    private const ushort NoSelectionYet = 0xEEEE;
    private const ushort FloorWindowClose = 0xFFFF;
    private const uint MenuNothingChosen = 0xFFFFFFFF;
    private const uint MenuCancelled = 0xFFFFFFFE;
    private const uint ListHeaderIndex = 0xFFFFFFFD;
    private const ushort NoAltText = 0xFF;
    private const int PadKeyRight = 0x10;
    private const int PadKeyLeft = 0x20;
    private const int PadKeyUp = 0x40;
    private const int PadKeyDown = 0x80;
    private const byte ListMenuParentAttribute = 0x13;

    private static uint PixelToTiles(uint length)
    {
        if ((length & 7) == 0)
        {
            return length >> 3;
        }
        return (length >> 3) + 1;
    }

    // OFFSET 0x021DC018
    private static void FieldMenuManager_Init(FieldSystem fieldSystem, FieldMenuManager menuManager, byte anchorX, byte anchorY, byte initialCursorPos, byte canExitWithB, ushort[] selectedOptionPtr, StringTemplate? stringTemplate, Window? parentWindow, MessageLoader? messageLoader)
    {
        if (messageLoader == null)
        {
            menuManager.MessageLoader = MessageLoader.MessageLoader_Init(1, 0x1A, TextBankMenuEntries, HeapIDField);
            menuManager.FreeMsgLoaderOnDelete = 1;
        }
        else
        {
            menuManager.MessageLoader = messageLoader;
            menuManager.FreeMsgLoaderOnDelete = 0;
        }
        menuManager.StringTemplate = stringTemplate;
        menuManager.FieldSystem = fieldSystem;
        menuManager.SelectedOptionPtr = selectedOptionPtr;
        menuManager.SelectedOptionPtr[0] = 0;
        menuManager.ListMenuListOffsetPtr = null;
        menuManager.ListMenuCursorPosPtr = null;
        menuManager.CanExitWithB = (byte)(canExitWithB & 1);
        menuManager.InitialCursorPos = initialCursorPos;
        menuManager.AnchorRight = 0;
        menuManager.AnchorBottom = 0;
        menuManager.AnchorX = anchorX;
        menuManager.AnchorY = anchorY;
        menuManager.OptionsCount = 0;
        menuManager.ParentWindow = parentWindow;
        menuManager.SysTaskDelay = 3;
        menuManager.CursorPos = initialCursorPos;
        for (int i = 0; i < FieldMenuManager.EntriesMax; i++)
        {
            menuManager.MenuChoicesStrings[i].Text = null;
            menuManager.MenuChoicesStrings[i].Index = 0;
        }
        for (int i = 0; i < FieldMenuManager.EntriesMax; i++)
        {
            menuManager.ListMenuChoicesStrings[i].Text = null;
            menuManager.ListMenuChoicesStrings[i].Index = 0;
            menuManager.ChoicesAltTextStringIDs[i] = NoAltText;
        }
        for (int i = 0; i < FieldMenuManager.EntriesMax; i++)
        {
            menuManager.ChoicesStringsBuffers[i] = GameString.String_Init(0x50, HeapIDField);
        }
        menuManager.SelectedOptionPtr[0] = NoSelectionYet;
    }

    // OFFSET 0x021DC150
    public static FieldMenuManager FieldMenuManager_New(FieldSystem fieldSystem, byte anchorX, byte anchorY, byte initialCursorPos, byte canExitWithB, ushort[] selectedOptionPtr, StringTemplate? stringTemplate, Window? parentWindow, MessageLoader? messageLoader)
    {
        FieldMenuManager menuManager = new();
        FieldMenuManager_Init(fieldSystem, menuManager, anchorX, anchorY, initialCursorPos, canExitWithB, selectedOptionPtr, stringTemplate, parentWindow, messageLoader);
        return menuManager;
    }

    // OFFSET 0x021DC1A4
    public static void FieldMenuManager_AddMenuEntry(FieldMenuManager menuManager, uint entryID, uint index)
    {
        _FieldMenuManager_AddMenuEntry(menuManager, entryID, index);
    }

    private static void AddStandardMenuWindow(FieldMenuManager menuManager, uint width, uint height)
    {
        Bg.Window_Add(menuManager.FieldSystem!.BgConfig!, menuManager.MenuWindow, BgLayerMain3, menuManager.AnchorX, menuManager.AnchorY, (byte)width, (byte)height, MenuWindowPalette, MenuWindowBaseTile);
        DrawStandardFrame(menuManager);
    }

    private static void DrawStandardFrame(FieldMenuManager menuManager)
    {
        RenderWindow.LoadStandardWindowGraphics(menuManager.FieldSystem!.BgConfig!, BgLayerMain3, BaseTileStandardWindowFrame, StandardWindowPalette, 0, HeapIDField);
        RenderWindow.Window_DrawStandardFrame(menuManager.MenuWindow, 1, BaseTileStandardWindowFrame, StandardWindowPalette);
    }

    // OFFSET 0x021DC1AC
    public static void FieldMenuManager_ShowSingleColumnMenu(FieldMenuManager menuManager)
    {
        uint menuWidth = PixelToTiles(FieldMenuManager_CalcMenuWidth(menuManager));
        if (menuManager.AnchorRight != 0)
        {
            menuManager.AnchorX = (byte)(menuManager.AnchorX - menuWidth);
        }
        if (menuManager.AnchorBottom != 0)
        {
            menuManager.AnchorY = (byte)(menuManager.AnchorY - menuManager.OptionsCount * GlyphRowTilesHeight);
        }
        AddStandardMenuWindow(menuManager, menuWidth, (uint)(menuManager.OptionsCount * GlyphRowTilesHeight));
        FieldMenuManager_SetupSingleColumnMenu(menuManager);
        menuManager.Menu = Menu.Menu_NewSimple(menuManager.MenuTemplate, menuManager.InitialCursorPos, HeapIDField);
        menuManager.SysTask = SysTask.SysTask_Start(FieldMenu_MenuSysTaskCallback, menuManager, 0);
    }

    // OFFSET 0x021DC290
    private static void _FieldMenuManager_AddMenuEntry(FieldMenuManager menuManager, uint entryID, uint index)
    {
        GameString entryBuf = GameString.String_Init(0x50, HeapIDField);
        MessageLoader.MessageLoader_GetString(menuManager.MessageLoader!, entryID, entryBuf);
        StringTemplate.StringTemplate_Format(menuManager.StringTemplate!, menuManager.ChoicesStringsBuffers[menuManager.OptionsCount]!, entryBuf);
        menuManager.MenuChoicesStrings[menuManager.OptionsCount].Text = menuManager.ChoicesStringsBuffers[menuManager.OptionsCount];
        GameString.String_Free(entryBuf);
        menuManager.MenuChoicesStrings[menuManager.OptionsCount].Index = index;
        menuManager.OptionsCount++;
    }

    private static uint CalcEntriesWidth(StringListEntry[] entries, int count)
    {
        uint maxWidth = 0;
        for (int i = 0; i < count; i++)
        {
            if (entries[i].Text == null)
            {
                break;
            }
            uint entryWidth = (uint)Font.Font_CalcStringWidthWithCursorControl(0, entries[i].Text);
            if (maxWidth < entryWidth)
            {
                maxWidth = entryWidth;
            }
        }
        return maxWidth + CursorMarginSize;
    }

    // OFFSET 0x021DC300
    private static uint FieldMenuManager_CalcMenuWidth(FieldMenuManager menuManager)
    {
        return CalcEntriesWidth(menuManager.MenuChoicesStrings, menuManager.OptionsCount);
    }

    // OFFSET 0x021DC33C
    private static void FieldMenuManager_SetupSingleColumnMenu(FieldMenuManager menuManager)
    {
        MenuTemplate template = menuManager.MenuTemplate;
        template.Choices = menuManager.MenuChoicesStrings;
        template.Window = menuManager.MenuWindow;
        template.FontID = 0;
        template.XSize = 1;
        template.YSize = menuManager.OptionsCount;
        template.LineSpacing = 0;
        template.SuppressCursor = 0;
        template.LoopAround = menuManager.OptionsCount >= MenuLoopAroundMinOptions ? 1 : 0;
    }

    // OFFSET 0x021DC3B0
    private static void FieldMenu_MenuSysTaskCallback(SysTask sysTask, object? param)
    {
        FieldMenuManager menuManager = (FieldMenuManager)param!;
        if (menuManager.SysTaskDelay != 0)
        {
            menuManager.SysTaskDelay--;
            return;
        }
        if (!ScreenFadeControl.IsScreenFadeDone())
        {
            return;
        }
        uint selectedEntry = Menu.Menu_ProcessInput(menuManager.Menu!);
        if (selectedEntry == MenuCancelled)
        {
            if (menuManager.CanExitWithB == 1)
            {
                menuManager.SelectedOptionPtr![0] = MenuCancel;
                FieldMenuManager_DeleteWithMenu(menuManager);
            }
        }
        else if (selectedEntry != MenuNothingChosen)
        {
            menuManager.SelectedOptionPtr![0] = (ushort)selectedEntry;
            FieldMenuManager_DeleteWithMenu(menuManager);
        }
    }

    private static void FreeChoiceStrings(FieldMenuManager menuManager)
    {
        for (int i = 0; i < FieldMenuManager.EntriesMax; i++)
        {
            GameString.String_Free(menuManager.ChoicesStringsBuffers[i]);
        }
        if (menuManager.FreeMsgLoaderOnDelete == 1)
        {
            MessageLoader.MessageLoader_Free(menuManager.MessageLoader);
        }
    }

    // OFFSET 0x021DC424
    public static void FieldMenuManager_DeleteWithMenu(FieldMenuManager menuManager)
    {
        SoundPlayback.Sound_PlayEffect(SeqConfirm);
        Menu.Menu_Free(menuManager.Menu!, out _);
        RenderWindow.Window_EraseStandardFrame(menuManager.MenuTemplate.Window, 0);
        Bg.Window_Remove(menuManager.MenuTemplate.Window);
        FreeChoiceStrings(menuManager);
        SysTask.SysTask_Done(menuManager.SysTask!);
    }

    // OFFSET 0x021DC48C
    public static FieldMenuManager FieldMenuManager_New2(FieldSystem fieldSystem, byte anchorX, byte anchorY, byte initialCursorPos, byte canExitWithB, ushort[] selectedOptionPtr, StringTemplate? stringTemplate, Window? parentWindow, MessageLoader? messageLoader)
    {
        return FieldMenuManager_New(fieldSystem, anchorX, anchorY, initialCursorPos, canExitWithB, selectedOptionPtr, stringTemplate, parentWindow, messageLoader);
    }

    // OFFSET 0x021DC4B0
    public static void FieldMenuManager_AddListMenuEntry(FieldMenuManager menuManager, uint entryID, uint altTextStringID, uint entryIndex)
    {
        _FieldMenuManager_AddListMenuEntry(menuManager, entryID, altTextStringID, entryIndex);
    }

    // OFFSET 0x021DC4B8
    public static void FieldMenuManager_ShowListMenu(FieldMenuManager menuManager)
    {
        uint menuWidth = PixelToTiles(FieldMenuManager_CalcListMenuWidth(menuManager));
        if (menuManager.AnchorRight != 0)
        {
            menuManager.AnchorX = (byte)(menuManager.AnchorX - menuWidth);
        }
        if (menuManager.AnchorBottom != 0)
        {
            if (menuManager.OptionsCount > ListMenuMaxDisplay)
            {
                menuManager.AnchorY = (byte)(menuManager.AnchorY - ListMenuMaxDisplay * GlyphRowTilesHeight);
            }
            else
            {
                menuManager.AnchorY = (byte)(menuManager.AnchorY - menuManager.OptionsCount * GlyphRowTilesHeight);
            }
        }
        _FieldMenuManager_ShowListMenuWithWidth(menuManager, menuWidth);
    }

    // OFFSET 0x021DC528
    public static void FieldMenuManager_ShowListMenuWithWidth(FieldMenuManager menuManager, ushort windowWidth)
    {
        _FieldMenuManager_ShowListMenuWithWidth(menuManager, windowWidth);
    }

    private static void AddListMenuWindow(FieldMenuManager menuManager, uint windowWidth)
    {
        if (menuManager.OptionsCount > ListMenuMaxDisplay)
        {
            AddStandardMenuWindow(menuManager, windowWidth, ListMenuMaxDisplay * GlyphRowTilesHeight);
        }
        else
        {
            AddStandardMenuWindow(menuManager, windowWidth, (uint)(menuManager.OptionsCount * GlyphRowTilesHeight));
        }
        FieldMenuManager_InitListMenuTemplate(menuManager);
    }

    // OFFSET 0x021DC530
    private static void _FieldMenuManager_ShowListMenuWithWidth(FieldMenuManager menuManager, uint windowWidth)
    {
        AddListMenuWindow(menuManager, windowWidth);
        menuManager.ListMenu = ListMenu.ListMenu_New(menuManager.ListMenuTemplate, 0, menuManager.InitialCursorPos, HeapIDField);
        FieldMenuManager_UpdateListMenuAltText(menuManager);
        menuManager.SysTask = SysTask.SysTask_Start(FieldMenu_ListMenuSysTaskCallback, menuManager, 0);
    }

    // OFFSET 0x021DC600
    public static void FieldMenuManager_ShowListMenuWithCursorPosition(FieldMenuManager menuManager, ushort[] listOffsetPtr, ushort[] cursorPosPtr)
    {
        uint menuWidth = PixelToTiles(FieldMenuManager_CalcListMenuWidth(menuManager));
        AddListMenuWindow(menuManager, menuWidth);
        menuManager.ListMenuListOffsetPtr = listOffsetPtr;
        menuManager.ListMenuCursorPosPtr = cursorPosPtr;
        menuManager.CursorPos = (ushort)(menuManager.ListMenuListOffsetPtr[0] + menuManager.ListMenuCursorPosPtr[0]);
        menuManager.ListMenu = ListMenu.ListMenu_New(menuManager.ListMenuTemplate, listOffsetPtr[0], cursorPosPtr[0], HeapIDField);
        FieldMenuManager_UpdateListMenuAltText(menuManager);
        menuManager.SysTask = SysTask.SysTask_Start(FieldMenu_ListMenuSysTaskCallback, menuManager, 0);
    }

    // OFFSET 0x021DC708
    private static void _FieldMenuManager_AddListMenuEntry(FieldMenuManager menuManager, uint entryID, uint altTextStringID, uint entryIndex)
    {
        GameString entryBuf = GameString.String_Init(0x50, HeapIDField);
        MessageLoader.MessageLoader_GetString(menuManager.MessageLoader!, entryID, entryBuf);
        StringTemplate.StringTemplate_Format(menuManager.StringTemplate!, menuManager.ChoicesStringsBuffers[menuManager.OptionsCount]!, entryBuf);
        menuManager.ListMenuChoicesStrings[menuManager.OptionsCount].Text = menuManager.ChoicesStringsBuffers[menuManager.OptionsCount];
        GameString.String_Free(entryBuf);
        if (entryIndex == 0xFA)
        {
            menuManager.ListMenuChoicesStrings[menuManager.OptionsCount].Index = ListHeaderIndex;
        }
        else
        {
            menuManager.ListMenuChoicesStrings[menuManager.OptionsCount].Index = entryIndex;
        }
        menuManager.ChoicesAltTextStringIDs[menuManager.OptionsCount] = (ushort)altTextStringID;
        menuManager.OptionsCount++;
    }

    // OFFSET 0x021DC7A8
    private static uint FieldMenuManager_CalcListMenuWidth(FieldMenuManager menuManager)
    {
        return CalcEntriesWidth(menuManager.ListMenuChoicesStrings, menuManager.OptionsCount);
    }

    // OFFSET 0x021DC7E4
    private static void FieldMenuManager_InitListMenuTemplate(FieldMenuManager menuManager)
    {
        ListMenuTemplate template = menuManager.ListMenuTemplate;
        template.Choices = menuManager.ListMenuChoicesStrings;
        template.CursorCallback = FieldMenu_ListMenuCursorCallback;
        template.PrintCallback = FieldMenu_ListMenuPrintCallback;
        template.Window = menuManager.MenuWindow;
        template.Count = menuManager.OptionsCount;
        template.MaxDisplay = ListMenuMaxDisplay;
        template.HeaderXOffset = 1;
        template.TextXOffset = 0xC;
        template.CursorXOffset = 2;
        template.YOffset = 1;
        template.TextColorFg = 1;
        template.TextColorBg = 0xF;
        template.TextColorShadow = 2;
        template.LetterSpacing = 0;
        template.LineSpacing = 0;
        template.PagerMode = 1;
        template.FontID = 0;
        template.CursorType = 0;
        template.Parent = menuManager;
    }

    // OFFSET 0x021DC8F4
    private static void FieldMenu_ListMenuPrintCallback(ListMenu listMenu, uint index, byte yOffset)
    {
        if (index == ListHeaderIndex)
        {
            ListMenu.ListMenu_SetAltTextColors(listMenu, 3, 0xF, 4);
        }
        else
        {
            ListMenu.ListMenu_SetAltTextColors(listMenu, 1, 0xF, 2);
        }
    }

    // OFFSET 0x021DC918
    private static void FieldMenu_ListMenuCursorCallback(ListMenu listMenu, uint index, byte onInit)
    {
        FieldMenuManager menuManager = (FieldMenuManager)ListMenu.ListMenu_GetAttribute(listMenu, ListMenuParentAttribute)!;
        ListMenu.ListMenu_GetListAndCursorPos(listMenu, out ushort listPos, out ushort cursorPos);
        if (menuManager.ListMenuListOffsetPtr != null && menuManager.ListMenuCursorPosPtr != null)
        {
            menuManager.ListMenuListOffsetPtr[0] = listPos;
            menuManager.ListMenuCursorPosPtr[0] = cursorPos;
        }
    }

    // OFFSET 0x021DC95C
    private static void FieldMenu_ListMenuSysTaskCallback(SysTask sysTask, object? param)
    {
        FieldMenuManager menuManager = (FieldMenuManager)param!;
        if (menuManager.SysTaskDelay != 0)
        {
            menuManager.SysTaskDelay--;
            return;
        }
        if (!ScreenFadeControl.IsScreenFadeDone())
        {
            return;
        }
        uint input = ListMenu.ListMenu_ProcessInput(menuManager.ListMenu!);
        ushort oldCursorPos = menuManager.CursorPos;
        ListMenu.ListMenu_CalcTrueCursorPos(menuManager.ListMenu!, out menuManager.CursorPos);
        if (oldCursorPos != menuManager.CursorPos)
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
        }
        int keys = Globals.gSystem.PressedKeysRepeatable;
        if ((keys & PadKeyUp) != 0 || (keys & PadKeyDown) != 0 || (keys & PadKeyLeft) != 0 || (keys & PadKeyRight) != 0)
        {
            FieldMenuManager_UpdateListMenuAltText(menuManager);
        }
        if (input == MenuCancelled)
        {
            if (menuManager.CanExitWithB == 1)
            {
                SoundPlayback.Sound_PlayEffect(SeqConfirm);
                menuManager.SelectedOptionPtr![0] = MenuCancel;
                FieldMenuManager_DeleteWithListMenu(menuManager);
            }
        }
        else if (input != MenuNothingChosen)
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            menuManager.SelectedOptionPtr![0] = (ushort)input;
            FieldMenuManager_DeleteWithListMenu(menuManager);
        }
    }

    // OFFSET 0x021DCA28
    private static void FieldMenuManager_DeleteWithListMenu(FieldMenuManager menuManager)
    {
        SoundPlayback.Sound_PlayEffect(SeqConfirm);
        ListMenu.ListMenu_Free(menuManager.ListMenu!, out _, out _);
        RenderWindow.Window_EraseStandardFrame(menuManager.ListMenuTemplate.Window, 0);
        Bg.Window_Remove(menuManager.MenuWindow);
        FreeChoiceStrings(menuManager);
        SysTask.SysTask_Done(menuManager.SysTask!);
    }

    // OFFSET 0x021DCA90
    private static void FieldMenuManager_PrintListMenuAltText(FieldMenuManager menuManager, ushort entryID, int printerDelay)
    {
        GameString entryBuf = GameString.String_Init(0x50, HeapIDField);
        GameString formatted = GameString.String_Init(0x50, HeapIDField);
        Bg.Window_FillTilemap(menuManager.ParentWindow!, 0xF);
        MessageLoader.MessageLoader_GetString(menuManager.MessageLoader!, entryID, entryBuf);
        StringTemplate.StringTemplate_Format(menuManager.StringTemplate!, formatted, entryBuf);
        Text.Text_AddPrinterWithParams(menuManager.ParentWindow!, 1, formatted, 0, 0, printerDelay, null);
        GameString.String_Free(entryBuf);
        GameString.String_Free(formatted);
    }

    // OFFSET 0x021DCAF4
    private static void FieldMenuManager_UpdateListMenuAltText(FieldMenuManager menuManager)
    {
        ListMenu.ListMenu_CalcTrueCursorPos(menuManager.ListMenu!, out menuManager.ListMenuAltTextIndex);
        ushort altTextID = menuManager.ChoicesAltTextStringIDs[menuManager.ListMenuAltTextIndex];
        if (altTextID != NoAltText)
        {
            FieldMenuManager_PrintListMenuAltText(menuManager, altTextID, 0);
        }
    }

    // OFFSET 0x021DCB24
    public static void FieldMenu_ShowCurrentFloorWindow(FieldSystem fieldSystem, byte tilemapLeft, byte tilemapTop, ushort[] selectedOptionPtr, StringTemplate stringTemplate, ushort unused)
    {
        FieldMenuManager menuManager = FieldMenuManager_New(fieldSystem, tilemapLeft, tilemapTop, 0, 0, selectedOptionPtr, stringTemplate, null, null);
        uint width = PixelToTiles((uint)(Font.Font_GetAttribute(0, 0) << 3));
        Bg.Window_Add(fieldSystem.BgConfig!, menuManager.MenuWindow, BgLayerMain3, menuManager.AnchorX, menuManager.AnchorY, (byte)width, 4, MenuWindowPalette, 0xDD);
        DrawStandardFrame(menuManager);
        Bg.Window_FillRectWithColor(menuManager.MenuWindow, 0xF, 0, 0, (ushort)(width << 3), 0x20);
        FieldMenuManager_PrintString(menuManager, 0xF, 0, 0);
        FieldMenuManager_PrintString(menuManager, 0x10, 0x20, 0x10);
        menuManager.MenuTemplate.Window = menuManager.MenuWindow;
        Bg.Window_CopyToVRAM(menuManager.MenuWindow);
        menuManager.SysTask = SysTask.SysTask_Start(FieldMenu_CurrentFloorWindowSysTaskCallback, menuManager, 0);
    }

    // OFFSET 0x021DCC00
    private static void FieldMenuManager_PrintString(FieldMenuManager menuManager, ushort entryID, byte xOffset, byte yOffset)
    {
        GameString entryBuf = GameString.String_Init(0x50, HeapIDField);
        GameString formatted = GameString.String_Init(0x50, HeapIDField);
        MessageLoader.MessageLoader_GetString(menuManager.MessageLoader!, entryID, entryBuf);
        StringTemplate.StringTemplate_Format(menuManager.StringTemplate!, formatted, entryBuf);
        Text.Text_AddPrinterWithParams(menuManager.MenuWindow, 0, formatted, xOffset, yOffset, 0xFF, null);
        GameString.String_Free(entryBuf);
        GameString.String_Free(formatted);
    }

    // OFFSET 0x021DCC64
    private static void FieldMenu_CurrentFloorWindowSysTaskCallback(SysTask sysTask, object? param)
    {
        FieldMenuManager menuManager = (FieldMenuManager)param!;
        if (menuManager.SelectedOptionPtr![0] != FloorWindowClose)
        {
            return;
        }
        RenderWindow.Window_EraseStandardFrame(menuManager.MenuTemplate.Window, 0);
        Bg.Window_Remove(menuManager.MenuTemplate.Window);
        FreeChoiceStrings(menuManager);
        SysTask.SysTask_Done(menuManager.SysTask!);
    }

    // OFFSET 0x021DCCC8
    public static ushort FieldMenu_GetFloorsAbove(int location)
    {
        switch (location)
        {
            case 0xB:
                return 3;
            case 0xC:
                return 2;
            case 0xD:
                return 1;
            case 0xE:
                return 0;
            case 0x67:
                return 1;
            case 0x68:
                return 0;
            case 0x70:
                return 1;
            case 0x71:
                return 0;
            case 0x89:
                return 4;
            case 0x8A:
                return 3;
            case 0x8B:
                return 2;
            case 0x8C:
                return 1;
            case 0x8D:
                return 0;
            case 0x96:
                return 1;
            case 0xA4:
                return 0;
            case 0x1CD:
                return 1;
            case 0x1CE:
                return 0;
            case 0x236:
                return 5;
        }
        return 1;
    }

    // OFFSET 0x021DCD94
    public static void FieldMenuManager_ShowMultiColumnMenu(FieldMenuManager menuManager, byte columnsCount)
    {
        uint columnWidth = PixelToTiles(FieldMenuManager_CalcMenuWidth(menuManager));
        int count = menuManager.OptionsCount;
        byte rowsCount = (byte)(count / columnsCount);
        if (count % columnsCount != 0)
        {
            rowsCount++;
        }
        AddStandardMenuWindow(menuManager, (byte)(columnWidth * columnsCount), (uint)(rowsCount * GlyphRowTilesHeight));
        FieldMenuManager_SetupMultiColumnMenu(menuManager, columnsCount, rowsCount);
        menuManager.Menu = Menu.Menu_NewSimple(menuManager.MenuTemplate, menuManager.InitialCursorPos, HeapIDField);
        menuManager.SysTask = SysTask.SysTask_Start(FieldMenu_MenuSysTaskCallback, menuManager, 0);
    }

    // OFFSET 0x021DCE64
    private static void FieldMenuManager_SetupMultiColumnMenu(FieldMenuManager menuManager, byte columnsCount, byte rowsCount)
    {
        MenuTemplate template = menuManager.MenuTemplate;
        template.Choices = menuManager.MenuChoicesStrings;
        template.Window = menuManager.MenuWindow;
        template.FontID = 0;
        template.XSize = columnsCount;
        template.YSize = rowsCount;
        template.LineSpacing = 0;
        template.SuppressCursor = 0;
    }

    private static Window NewCurrencyWindow(FieldSystem fieldSystem, byte tilemapLeft, byte tilemapTop, byte height, ushort baseTile)
    {
        Window window = Bg.Window_New(HeapIDField, 1)[0];
        Bg.Window_Add(fieldSystem.BgConfig!, window, BgLayerMain3, tilemapLeft, tilemapTop, 0xA, height, MenuWindowPalette, baseTile);
        RenderWindow.LoadStandardWindowGraphics(fieldSystem.BgConfig!, BgLayerMain3, BaseTileStandardWindowFrame, StandardWindowPalette, 0, HeapIDField);
        RenderWindow.Window_DrawStandardFrame(window, 1, BaseTileStandardWindowFrame, StandardWindowPalette);
        return window;
    }

    // OFFSET 0x021DCEB0
    public static Window FieldMenu_CreateMoneyWindow(FieldSystem fieldSystem, byte tilemapLeft, byte tilemapTop)
    {
        Window window = NewCurrencyWindow(fieldSystem, tilemapLeft, tilemapTop, 4, 1);
        Bg.Window_FillTilemap(window, 0xF);
        MessageLoader loader = MessageLoader.MessageLoader_Init(0, 0x1A, TextBankMoney, HeapIDField)!;
        GameString label = MessageLoader.MessageLoader_GetNewString(loader, 0x12)!;
        Text.Text_AddPrinterWithParams(window, 0, label, 0, 0, 0xFF, null);
        MessageLoader.MessageLoader_Free(loader);
        GameString.String_Free(label);
        FieldMenu_PrintMoneyToWindow(fieldSystem, window);
        return window;
    }

    // OFFSET 0x021DCF58
    public static void FieldMenu_DeleteMoneyWindow(Window window)
    {
        RenderWindow.Window_EraseStandardFrame(window, 0);
        Bg.Windows_Delete(new[] { window }, 1);
    }

    private static void PrintCurrencyValue(Window window, int bank, uint messageID, int value, uint digits, int y)
    {
        MessageLoader loader = MessageLoader.MessageLoader_Init(0, 0x1A, bank, HeapIDField)!;
        StringTemplate template = StringTemplate.StringTemplate_Default(HeapIDField);
        GameString text = GameString.String_Init(0x10, HeapIDField);
        GameString format = MessageLoader.MessageLoader_GetNewString(loader, messageID)!;
        StringTemplate.StringTemplate_SetNumber(template, 0, value, digits, 1, 1);
        StringTemplate.StringTemplate_Format(template, text, format);
        int x = 0x50 - Font.Font_CalcStringWidth(0, text, 0);
        Text.Text_AddPrinterWithParams(window, 0, text, x, y, 0xFF, null);
        GameString.String_Free(format);
        GameString.String_Free(text);
        StringTemplate.StringTemplate_Free(template);
        MessageLoader.MessageLoader_Free(loader);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x021DCF6C
    public static void FieldMenu_PrintMoneyToWindow(FieldSystem fieldSystem, Window window)
    {
        Bg.Window_FillRectWithColor(window, 0xF, 0, 0x10, 0x50, 0x10);
        PrintCurrencyValue(window, TextBankMoney, 0x13, (int)TrainerInfo.TrainerInfo_Money(PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!)), 6, 0x10);
    }

    // OFFSET 0x021DD020
    public static Window FieldMenu_DrawCoinWindow(FieldSystem fieldSystem, byte tilemapLeft, byte tilemapTop)
    {
        Window window = NewCurrencyWindow(fieldSystem, tilemapLeft, tilemapTop, 2, 0x29);
        FieldMenu_PrintCoinsToWindow(fieldSystem, window);
        return window;
    }

    // OFFSET 0x021DD084
    public static void FieldMenu_DeleteSpecialCurrencyWindow(Window window)
    {
        RenderWindow.Window_EraseStandardFrame(window, 0);
        Bg.Windows_Delete(new[] { window }, 1);
    }

    // OFFSET 0x021DD098
    public static void FieldMenu_PrintCoinsToWindow(FieldSystem fieldSystem, Window window)
    {
        Bg.Window_FillTilemap(window, 0xF);
        PrintCurrencyValue(window, TextBankMenuEntries, 0xC5, Coins.Coins_GetValue(PlayerSave.SaveData_GetCoins(fieldSystem.SaveData!)), 5, 0);
    }

    // OFFSET 0x021DD140
    public static Window FieldMenu_DrawBPWindow(FieldSystem fieldSystem, byte tilemapLeft, byte tilemapTop)
    {
        Window window = NewCurrencyWindow(fieldSystem, tilemapLeft, tilemapTop, 2, 0x29);
        FieldMenu_PrintBPToWindow(fieldSystem, window);
        return window;
    }

    // OFFSET 0x021DD1A4
    public static void FieldMenu_PrintBPToWindow(FieldSystem fieldSystem, Window window)
    {
        Bg.Window_FillTilemap(window, 0xF);
        PrintCurrencyValue(window, TextBankMenuEntries, 0xE6, BattleTowerRecords.BattlePoints_ApplyFuncAndGet(BattleTowerRecords.SaveData_GetBattleTowerRecords(fieldSystem.SaveData!), 0, 0), 5, 0);
    }

    // OFFSET 0x021DD250
    public static FieldMenuManager FieldMenuManager_NewMoveTutorCostWindow(FieldSystem fieldSystem, byte anchorX, byte anchorY, ushort[] selectedOptionPtr, StringTemplate stringTemplate, byte redCost, byte blueCost, byte yellowCost, byte greenCost)
    {
        FieldMenuManager menuManager = FieldMenuManager_New(fieldSystem, anchorX, anchorY, 0, 0, selectedOptionPtr, stringTemplate, null, null);
        Bg.Window_Add(fieldSystem.BgConfig!, menuManager.MenuWindow, BgLayerMain3, menuManager.AnchorX, menuManager.AnchorY, 0xA, 0x10, MenuWindowPalette, MenuWindowBaseTile);
        DrawStandardFrame(menuManager);
        Bg.Window_FillRectWithColor(menuManager.MenuWindow, 0xF, 0, 0, 0x50, 0x80);
        FieldMenuManager_PrintString(menuManager, 0x111, 0, 0);
        StringTemplate.StringTemplate_SetNumber(stringTemplate, 0, redCost, 3, 1, 1);
        FieldMenuManager_PrintString(menuManager, 0x115, 0, 0x10);
        FieldMenuManager_PrintString(menuManager, 0x112, 0, 0x20);
        StringTemplate.StringTemplate_SetNumber(stringTemplate, 0, blueCost, 3, 1, 1);
        FieldMenuManager_PrintString(menuManager, 0x115, 0, 0x30);
        FieldMenuManager_PrintString(menuManager, 0x113, 0, 0x40);
        StringTemplate.StringTemplate_SetNumber(stringTemplate, 0, yellowCost, 3, 1, 1);
        FieldMenuManager_PrintString(menuManager, 0x115, 0, 0x50);
        FieldMenuManager_PrintString(menuManager, 0x114, 0, 0x60);
        StringTemplate.StringTemplate_SetNumber(stringTemplate, 0, greenCost, 3, 1, 1);
        FieldMenuManager_PrintString(menuManager, 0x115, 0, 0x70);
        menuManager.MenuTemplate.Window = menuManager.MenuWindow;
        Bg.Window_CopyToVRAM(menuManager.MenuWindow);
        return menuManager;
    }

    // OFFSET 0x021DD3A8
    public static void FieldMenuManager_DeleteMoveTutorCost(FieldMenuManager menuManager)
    {
        RenderWindow.Window_EraseStandardFrame(menuManager.MenuTemplate.Window, 0);
        Bg.Window_Remove(menuManager.MenuTemplate.Window);
        FreeChoiceStrings(menuManager);
    }

    // OFFSET 0x021DD3F4
    public static void FieldMenuManager_SetHorizontalAnchor(FieldMenuManager menuManager, int anchorRight)
    {
        menuManager.AnchorRight = (byte)(anchorRight & 1);
    }

    // OFFSET 0x021DD410
    public static void FieldMenuManager_SetVerticalAnchor(FieldMenuManager menuManager, int anchorBottom)
    {
        menuManager.AnchorBottom = (byte)(anchorBottom & 1);
    }
}

public static class BattleTowerRecords
{
    private const int BattlePointsOffset = 0x954;
    private const int MaxBattlePoints = 0x270F;

    // OFFSET 0x0202D750
    public static BufferView SaveData_GetBattleTowerRecords(SaveData saveData)
    {
        BattleFrontierSave frontier = (BattleFrontierSave)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryFrontier);
        return new BufferView(frontier.Raw, BattlePointsOffset);
    }

    // OFFSET 0x0202D230
    public static ushort BattlePoints_ApplyFuncAndGet(BufferView records, int value, int func)
    {
        switch (func)
        {
            case 1:
                records.W16(0, value > MaxBattlePoints ? MaxBattlePoints : value);
                break;
            case 5:
            {
                int total = records.U16(0) + value;
                records.W16(0, total > MaxBattlePoints ? MaxBattlePoints : total);
                break;
            }
            case 6:
            {
                int current = records.U16(0);
                records.W16(0, current < value ? 0 : current - value);
                break;
            }
        }
        return records.U16(0);
    }
}
