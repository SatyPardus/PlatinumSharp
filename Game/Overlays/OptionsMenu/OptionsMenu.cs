namespace PlatinumSharp.Game;

public sealed class OptionsMenuEntry
{
    public const int MaxChoices = 20;

    public ushort NumChoices;
    public ushort Selected;
    public readonly GameString?[] Choices = new GameString?[MaxChoices];
}

public sealed class OptionsMenuData
{
    public const int Size = 0x2BC;
    public const int NumWindows = 3;
    public const int MaxEntries = 7;

    public int HeapID;
    public int State;
    public int SubState;
    public int SaveSelections;
    public int Cursor;
    public bool RedrawMessageBox;
    public BgConfig? BgConfig;
    public ushort Options;
    public Options? SaveOptions;
    public MessageLoader? MsgLoader;
    public byte[]? NscrBuffer;
    public ScreenData? TilemapData;
    public readonly Window[] Windows = { new(), new(), new() };
    public readonly OptionsMenuEntry[] Entries = NewEntries();
    public Menu? YesNoChoice;
    public int TextPrinter;

    private static OptionsMenuEntry[] NewEntries()
    {
        OptionsMenuEntry[] entries = new OptionsMenuEntry[MaxEntries];
        for (int i = 0; i < entries.Length; i++)
        {
            entries[i] = new OptionsMenuEntry();
        }
        return entries;
    }
}

public static class OptionsMenu
{
    public const int Overlay = 74;
    private const int HeapIDOptionsMenu = 0x26;
    private const uint HeapSize = 0x10000;
    private const int OptionsMenuArchive = 0x49;
    private const int MessageArchive = 0x1A;
    private const int OptionsMenuTextBank = 0xDC;
    private const uint AllLayersMask = 0xFFFFE0FF;
    private const int KeyA = 0x1;
    private const int KeyB = 0x2;
    private const int KeyRight = 0x10;
    private const int KeyLeft = 0x20;
    private const int KeyUp = 0x40;
    private const int KeyDown = 0x80;
    private const int SeqSelect = 0x5DC;
    private const int SeqSaved = 0x61B;
    private const uint EntryTitleIDs = 0x021D1BF4;
    private const uint EntryChoiceBaseIDs = 0x021D1BFC;
    private const uint EntryXOffsets = 0x021D1C04;
    private const uint EntryDescriptionIDs = 0x021D1C0C;
    private const uint YesNoWindowTemplate = 0x021D1C14;
    private const uint GraphicsModesAddress = 0x021D1C1C;
    private const uint BgLayers = 0x021D1C2C;
    private const uint EntryChoiceCounts = 0x021D1C40;
    private const uint VramBanksAddress = 0x021D1C5C;
    private const uint BgTemplates = 0x021D1C84;
    private const int NumBgs = 5;
    private const int StandardWindowTile = 0x232;
    private const int StandardWindowPalette = 0xE;
    private const int MessageBoxTile = 0x23B;
    private const int MessageBoxPalette = 0xF;
    private const uint ColorTitle = 0x10200;
    private const uint ColorChoice = 0x1020F;
    private const uint ColorSelected = 0x3040F;

    private const int EntryTextSpeed = 0;
    private const int EntrySoundMode = 1;
    private const int EntryBattleScene = 2;
    private const int EntryBattleStyle = 3;
    private const int EntryButtonMode = 4;
    private const int EntryMessageBoxStyle = 5;
    private const int EntryClose = 6;

    private const int WindowTitle = 0;
    private const int WindowEntries = 1;
    private const int WindowDescription = 2;

    private const int SaveSelectionsYes = 1;
    private const int SaveSelectionsNo = 2;

    private const int OptionTextSpeedShift = 0;
    private const int OptionTextSpeedMask = 0xF;
    private const int OptionSoundModeShift = 4;
    private const int OptionSoundModeMask = 0x3;
    private const int OptionBattleSceneShift = 6;
    private const int OptionBattleStyleShift = 7;
    private const int OptionButtonModeShift = 8;
    private const int OptionButtonModeMask = 0x3;
    private const int OptionFrameShift = 10;
    private const int OptionFrameMask = 0x1F;

    private static int GetOption(OptionsMenuData menuData, int shift, int mask)
    {
        return (menuData.Options >> shift) & mask;
    }

    private static void SetOption(OptionsMenuData menuData, int shift, int mask, int value)
    {
        menuData.Options = (ushort)((menuData.Options & ~(mask << shift)) | ((value & mask) << shift));
    }

    private static byte TableByte(uint address, int index)
    {
        return RomData.Overlay(Overlay, (uint)(address + index), 1)[0];
    }

    // OFFSET 0x021D0D80
    public static int OptionsMenu_Init(ApplicationManager appMan, ref int state)
    {
        Options options = (Options)ApplicationManager.ApplicationManager_Args(appMan)!;
        Heap.Heap_Create(3, HeapIDOptionsMenu, HeapSize);
        ApplicationManager.ApplicationManager_NewData(appMan, OptionsMenuData.Size, HeapIDOptionsMenu);
        OptionsMenuData menuData = new();
        appMan.Data = menuData;
        SetOption(menuData, OptionTextSpeedShift, OptionTextSpeedMask, Options.Options_TextSpeed(options));
        SetOption(menuData, OptionBattleSceneShift, 1, Options.Options_BattleScene(options));
        SetOption(menuData, OptionBattleStyleShift, 1, Options.Options_BattleStyle(options));
        SetOption(menuData, OptionSoundModeShift, OptionSoundModeMask, Options.Options_SoundMode(options));
        SetOption(menuData, OptionButtonModeShift, OptionButtonModeMask, Options.Options_ButtonMode(options));
        SetOption(menuData, OptionFrameShift, OptionFrameMask, Options.Options_Frame(options));
        menuData.HeapID = HeapIDOptionsMenu;
        menuData.SaveOptions = options;
        RenderText.RenderControlFlags_SetCanABSpeedUpPrint(0);
        return 1;
    }

    // OFFSET 0x021D0E58
    public static int OptionsMenu_Exit(ApplicationManager appMan, ref int state)
    {
        OptionsMenuData menuData = (OptionsMenuData)ApplicationManager.ApplicationManager_Data(appMan)!;
        if (menuData.SaveSelections == SaveSelectionsYes)
        {
            SetOption(menuData, OptionTextSpeedShift, OptionTextSpeedMask, menuData.Entries[EntryTextSpeed].Selected);
            SetOption(menuData, OptionBattleSceneShift, 1, menuData.Entries[EntryBattleScene].Selected);
            SetOption(menuData, OptionBattleStyleShift, 1, menuData.Entries[EntryBattleStyle].Selected);
            SetOption(menuData, OptionSoundModeShift, OptionSoundModeMask, menuData.Entries[EntrySoundMode].Selected);
            SetOption(menuData, OptionButtonModeShift, OptionButtonModeMask, menuData.Entries[EntryButtonMode].Selected);
            SetOption(menuData, OptionFrameShift, OptionFrameMask, menuData.Entries[EntryMessageBoxStyle].Selected);
        }
        Options saveOptions = menuData.SaveOptions!;
        Options.Options_SetTextSpeed(saveOptions, GetOption(menuData, OptionTextSpeedShift, OptionTextSpeedMask));
        Options.Options_SetBattleScene(saveOptions, GetOption(menuData, OptionBattleSceneShift, 1));
        Options.Options_SetBattleStyle(saveOptions, GetOption(menuData, OptionBattleStyleShift, 1));
        Options.Options_SetSoundMode(saveOptions, GetOption(menuData, OptionSoundModeShift, OptionSoundModeMask));
        Options.Options_SetButtonMode(saveOptions, GetOption(menuData, OptionButtonModeShift, OptionButtonModeMask));
        Options.Options_SetFrame(saveOptions, GetOption(menuData, OptionFrameShift, OptionFrameMask));
        Sound.Sound_SetPlaybackMode(GetOption(menuData, OptionSoundModeShift, OptionSoundModeMask));
        Options.Options_SetSystemButtonMode(null, GetOption(menuData, OptionButtonModeShift, OptionButtonModeMask));
        RenderText.RenderControlFlags_SetCanABSpeedUpPrint(1);
        ApplicationManager.ApplicationManager_FreeData(appMan);
        Heap.Heap_Destroy(menuData.HeapID);
        return 1;
    }

    // OFFSET 0x021D0F60
    public static int OptionsMenu_Main(ApplicationManager appMan, ref int state)
    {
        OptionsMenuData menuData = (OptionsMenuData)ApplicationManager.ApplicationManager_Data(appMan)!;
        switch (menuData.State)
        {
            case 0:
                if (!SetupMenuVisuals(menuData))
                {
                    return 0;
                }
                ScreenFadeControl.StartScreenFade(3, 1, 1, 0, 6, 1, menuData.HeapID);
                break;
            case 1:
                if (!ScreenFadeControl.IsScreenFadeDone())
                {
                    return 0;
                }
                break;
            case 2:
            {
                int keys = Globals.gSystem.PressedKeys;
                if (((keys & KeyA) != 0 && menuData.Cursor == EntryClose) || (keys & KeyB) != 0)
                {
                    if (ChangesWereMade(menuData))
                    {
                        menuData.State = 3;
                        return 0;
                    }
                    SoundPlayback.Sound_PlayEffect(SeqSelect);
                    menuData.SaveSelections = SaveSelectionsNo;
                    menuData.State = 6;
                    return 0;
                }
                ProcessMainInput(menuData);
                return 0;
            }
            case 3:
                RenderText.RenderControlFlags_SetCanABSpeedUpPrint(1);
                PrintBankEntryAsDescription(menuData, 0x31, false);
                break;
            case 4:
                if (IsTextPrinterDone(menuData))
                {
                    RenderText.RenderControlFlags_SetCanABSpeedUpPrint(0);
                    DrawConfirmationPrompt(menuData);
                    menuData.State = 5;
                }
                return 0;
            case 5:
            {
                uint input = ProcessConfirmationInput(menuData);
                if (input == Menu.NoChoice)
                {
                    return 0;
                }
                if (input == 0)
                {
                    SoundPlayback.Sound_StopEffect(SeqSelect, 0);
                    SoundPlayback.Sound_PlayEffect(SeqSaved);
                    menuData.SaveSelections = SaveSelectionsYes;
                }
                else
                {
                    menuData.SaveSelections = SaveSelectionsNo;
                }
                menuData.State = 6;
                return 0;
            }
            case 6:
                if (Text.Text_IsPrinterActive((byte)menuData.TextPrinter) != 0)
                {
                    Text.Text_RemovePrinter((byte)menuData.TextPrinter);
                }
                ScreenFadeControl.StartScreenFade(3, 0, 0, 0, 6, 1, menuData.HeapID);
                break;
            case 7:
                if (!ScreenFadeControl.IsScreenFadeDone())
                {
                    return 0;
                }
                break;
            case 8:
                return TeardownMenuData(menuData) ? 1 : 0;
            default:
                break;
        }
        menuData.State++;
        return 0;
    }

    // OFFSET 0x021D10F8
    private static void SetVRAMBanks()
    {
        GXLayers.GXLayers_SetBanks(RomStructs.Banks(RomData.Overlay(Overlay, VramBanksAddress, 0x28), 0));
    }

    // OFFSET 0x021D1118
    private static void OptionsMenuVBlank(object? data)
    {
        OptionsMenuData menuData = (OptionsMenuData)data!;
        if (menuData.RedrawMessageBox)
        {
            RenderWindow.LoadMessageBoxGraphics(menuData.BgConfig!, 1, MessageBoxTile, MessageBoxPalette, (byte)menuData.Entries[EntryMessageBoxStyle].Selected, menuData.HeapID);
            menuData.RedrawMessageBox = false;
        }
        SpriteSystem.SpriteSystem_TransferOam();
        VramTransfer.VramTransfer_Process();
        Bg.Bg_RunScheduledUpdates(menuData.BgConfig!);
    }

    private static void DisableDisplays()
    {
        GameSystem.SetVBlankCallback(null, null);
        GameSystem.DisableHBlank();
        GXLayers.GXLayers_DisableEngineALayers();
        GXLayers.GXLayers_DisableEngineBLayers();
        Video.SetDisplayControl(0, Video.GetDisplayControl(0) & AllLayersMask);
        Video.SetDisplayControl(1, Video.GetDisplayControl(1) & AllLayersMask);
    }

    // OFFSET 0x021D1178
    private static bool SetupMenuVisuals(OptionsMenuData menuData)
    {
        switch (menuData.SubState)
        {
            case 0:
                DisableDisplays();
                SetVRAMBanks();
                ScreenFadeControl.ResetVisibleHardwareWindows(0);
                ScreenFadeControl.ResetVisibleHardwareWindows(1);
                SetupBgs(menuData);
                break;
            case 1:
                LoadBgTiles(menuData);
                menuData.MsgLoader = MessageLoader.MessageLoader_Init(1, MessageArchive, OptionsMenuTextBank, menuData.HeapID);
                LoadAllEntryChoices(menuData);
                break;
            case 2:
                SetupWindows(menuData);
                PrintTitleAndEntries(menuData);
                VramTransfer.VramTransfer_New(0x20, menuData.HeapID);
                GXLayers.GXLayers_EngineAToggleLayers(0x10, 1);
                NetworkIcon.NetworkIcon_InitIfConnected();
                GameSystem.SetVBlankCallback(OptionsMenuVBlank, menuData);
                menuData.SubState = 0;
                return true;
        }
        menuData.SubState++;
        return false;
    }

    // OFFSET 0x021D122C
    private static bool TeardownMenuData(OptionsMenuData menuData)
    {
        switch (menuData.SubState)
        {
            case 0:
                VramTransfer.VramTransfer_Free();
                TeardownWindows(menuData);
                for (int i = 0; i < OptionsMenuData.MaxEntries; i++)
                {
                    OptionsMenuEntry entry = menuData.Entries[i];
                    for (int j = 0; j < entry.NumChoices; j++)
                    {
                        GameString.String_Free(entry.Choices[j]);
                    }
                }
                MessageLoader.MessageLoader_Free(menuData.MsgLoader);
                TeardownTilemaps(menuData);
                TeardownBgs(menuData);
                break;
            case 1:
                DisableDisplays();
                menuData.SubState = 0;
                return true;
        }
        menuData.SubState++;
        return false;
    }

    // OFFSET 0x021D12D4
    private static void SetupBgs(OptionsMenuData menuData)
    {
        menuData.BgConfig = Bg.BgConfig_New(menuData.HeapID);
        Bg.SetAllGraphicsModes(RomStructs.GraphicsModes(RomData.Overlay(Overlay, GraphicsModesAddress, 0x10), 0));
        uint[] layers = RomData.OverlayU32(Overlay, BgLayers, NumBgs);
        for (int i = 0; i < NumBgs; i++)
        {
            BgTemplate template = RomStructs.BgTemplate(RomData.Overlay(Overlay, (uint)(BgTemplates + i * 0x1C), 0x1C), 0);
            Bg.Bg_InitFromTemplate(menuData.BgConfig, (byte)layers[i], template, 0);
            Bg.Bg_ClearTilemap(menuData.BgConfig, (byte)layers[i]);
        }
        Bg.Bg_ClearTilesRange(0, 0x20, 0, menuData.HeapID);
        Bg.Bg_ClearTilesRange(4, 0x20, 0, menuData.HeapID);
    }

    // OFFSET 0x021D135C
    private static void TeardownBgs(OptionsMenuData menuData)
    {
        Bg.Bg_FreeTilemapBuffer(menuData.BgConfig!, 4);
        Bg.Bg_FreeTilemapBuffer(menuData.BgConfig!, 3);
        Bg.Bg_FreeTilemapBuffer(menuData.BgConfig!, 2);
        Bg.Bg_FreeTilemapBuffer(menuData.BgConfig!, 1);
        Bg.Bg_FreeTilemapBuffer(menuData.BgConfig!, 0);
    }

    // OFFSET 0x021D1390
    private static void LoadBgTiles(OptionsMenuData menuData)
    {
        NARC narc = NARC.NARC_ctor(OptionsMenuArchive, menuData.HeapID);
        byte[] buffer = Heap.Heap_AllocAtEnd(menuData.HeapID, NARC.NARC_GetMemberSize(narc, 1))!;
        NARC.NARC_ReadWholeMember(narc, 1, buffer);
        G2d.GetUnpackedCharacterData(buffer, out CharacterData? charData);
        Bg.Bg_LoadTiles(menuData.BgConfig!, 0, charData!.RawData, charData.SizeBytes, 0);
        Bg.Bg_LoadTiles(menuData.BgConfig!, 4, charData.RawData, charData.SizeBytes, 0);
        Heap.Heap_Free(buffer);
        buffer = Heap.Heap_AllocAtEnd(menuData.HeapID, NARC.NARC_GetMemberSize(narc, 0))!;
        NARC.NARC_ReadWholeMember(narc, 0, buffer);
        G2d.GetUnpackedPaletteData(buffer, out PaletteFile? palette);
        Bg.Bg_LoadPalette(0, palette!.Colors, 0, 0x20, 0);
        Bg.Bg_LoadPalette(4, palette.Colors, 0, 0x20, 0);
        Heap.Heap_Free(buffer);
        menuData.NscrBuffer = Heap.Heap_Alloc(menuData.HeapID, NARC.NARC_GetMemberSize(narc, 2));
        NARC.NARC_ReadWholeMember(narc, 2, menuData.NscrBuffer!);
        G2d.GetUnpackedScreenData(menuData.NscrBuffer!, out menuData.TilemapData);
        NARC.NARC_dtor(narc);
        Bg.Bg_FillTilemapRect(menuData.BgConfig!, 2, 1, 0, 0, 0x20, 0x20, 0x11);
        Bg.Bg_FillTilemapRect(menuData.BgConfig!, 4, 1, 0, 0, 0x20, 0x20, 0x11);
        ScreenData tilemap = menuData.TilemapData!;
        Bg.Bg_CopyRectToTilemapRect(menuData.BgConfig!, 0, 0, 0, 0x20, 2, tilemap.RawData, 0, 0, tilemap.Width >> 3, tilemap.Height >> 3);
        Bg.Bg_SetOffset(menuData.BgConfig!, 0, 3, -0x18);
        Bg.Bg_ScheduleTilemapTransfer(menuData.BgConfig!, 2);
        Bg.Bg_ScheduleTilemapTransfer(menuData.BgConfig!, 0);
        Bg.Bg_ScheduleTilemapTransfer(menuData.BgConfig!, 4);
    }

    // OFFSET 0x021D14E8
    private static void TeardownTilemaps(OptionsMenuData menuData)
    {
        Heap.Heap_Free(menuData.NscrBuffer!);
    }

    // OFFSET 0x021D14F4
    private static void SetupWindows(OptionsMenuData menuData)
    {
        BgConfig bgConfig = menuData.BgConfig!;
        Bg.Window_Add(bgConfig, menuData.Windows[WindowTitle], 1, 1, 0, 0xC, 2, 0xD, 0xA);
        Bg.Window_Add(bgConfig, menuData.Windows[WindowEntries], 1, 1, 3, 0x1E, 0xE, 0xD, 0x22);
        Bg.Window_Add(bgConfig, menuData.Windows[WindowDescription], 1, 2, 0x13, 0x1B, 4, 0xC, 0x1C6);
        RenderWindow.LoadStandardWindowGraphics(bgConfig, 1, StandardWindowTile, StandardWindowPalette, 0, menuData.HeapID);
        RenderWindow.LoadMessageBoxGraphics(bgConfig, 1, MessageBoxTile, MessageBoxPalette, (byte)GetOption(menuData, OptionFrameShift, OptionFrameMask), menuData.HeapID);
        Font.Font_LoadTextPalette(0, 0x1A0, menuData.HeapID);
        Font.Font_LoadTextPalette(4, 0x1A0, menuData.HeapID);
        Font.Font_LoadScreenIndicatorsPalette(0, 0x180, menuData.HeapID);
        Font.Font_LoadScreenIndicatorsPalette(4, 0x180, menuData.HeapID);
        Bg.Window_FillTilemap(menuData.Windows[WindowTitle], 0);
        Bg.Window_FillTilemap(menuData.Windows[WindowEntries], 0xFF);
        Bg.Window_FillTilemap(menuData.Windows[WindowDescription], 0xFF);
        Bg.Window_ClearTilemap(menuData.Windows[WindowDescription]);
        Bg.Window_ClearTilemap(menuData.Windows[WindowEntries]);
        Bg.Window_ClearTilemap(menuData.Windows[WindowTitle]);
        RenderWindow.Window_DrawStandardFrame(menuData.Windows[WindowEntries], 1, StandardWindowTile, StandardWindowPalette);
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(menuData.Windows[WindowDescription], 1, MessageBoxTile, MessageBoxPalette);
    }

    // OFFSET 0x021D1624
    private static void TeardownWindows(OptionsMenuData menuData)
    {
        RenderWindow.Window_EraseStandardFrame(menuData.Windows[WindowEntries], 0);
        RenderWindow.Window_EraseMessageBox(menuData.Windows[WindowDescription], 0);
        for (int i = 0; i < OptionsMenuData.NumWindows; i++)
        {
            Window window = menuData.Windows[i];
            Bg.Window_ClearAndCopyToVRAM(window);
            Bg.Window_FillTilemap(window, 0);
            Bg.Window_ClearTilemap(window);
            Bg.Window_Remove(window);
        }
    }

    // OFFSET 0x021D1668
    private static void PrintTitleAndEntries(OptionsMenuData menuData)
    {
        GameString text = GameString.String_Init(0x100, menuData.HeapID);
        MessageLoader.MessageLoader_GetString(menuData.MsgLoader!, 0, text);
        Text.Text_AddPrinterWithParamsAndColor(menuData.Windows[WindowTitle], 0, text, 2, 2, 0, ColorTitle, null);
        for (int i = 0; i < OptionsMenuData.MaxEntries; i++)
        {
            GameString.String_Clear(text);
            MessageLoader.MessageLoader_GetString(menuData.MsgLoader!, TableByte(EntryTitleIDs, i), text);
            Text.Text_AddPrinterWithParamsAndColor(menuData.Windows[WindowEntries], 0, text, 4, (uint)(i << 4), 0xFF, ColorChoice, null);
        }
        for (int i = 0; i < OptionsMenuData.MaxEntries; i++)
        {
            PrintEntryChoices(menuData, (ushort)i);
        }
        PrintEntryDescription(menuData, 0, true);
        Bg.Window_CopyToVRAM(menuData.Windows[WindowTitle]);
        Bg.Window_CopyToVRAM(menuData.Windows[WindowEntries]);
        GameString.String_Free(text);
    }

    // OFFSET 0x021D1720
    private static void LoadAllEntryChoices(OptionsMenuData menuData)
    {
        uint[] counts = RomData.OverlayU32(Overlay, EntryChoiceCounts, OptionsMenuData.MaxEntries);
        for (int i = 0; i < OptionsMenuData.MaxEntries; i++)
        {
            OptionsMenuEntry entry = menuData.Entries[i];
            entry.NumChoices = (ushort)counts[i];
            for (int j = 0; j < (int)counts[i]; j++)
            {
                entry.Choices[j] = MessageLoader.MessageLoader_GetNewString(menuData.MsgLoader!, (uint)(TableByte(EntryChoiceBaseIDs, i) + j));
            }
        }
        menuData.Entries[EntryTextSpeed].Selected = (ushort)GetOption(menuData, OptionTextSpeedShift, OptionTextSpeedMask);
        menuData.Entries[EntryBattleScene].Selected = (ushort)GetOption(menuData, OptionBattleSceneShift, 1);
        menuData.Entries[EntryBattleStyle].Selected = (ushort)GetOption(menuData, OptionBattleStyleShift, 1);
        menuData.Entries[EntrySoundMode].Selected = (ushort)GetOption(menuData, OptionSoundModeShift, OptionSoundModeMask);
        menuData.Entries[EntryButtonMode].Selected = (ushort)GetOption(menuData, OptionButtonModeShift, OptionButtonModeMask);
        menuData.Entries[EntryMessageBoxStyle].Selected = (ushort)GetOption(menuData, OptionFrameShift, OptionFrameMask);
    }

    // OFFSET 0x021D17CC
    private static void PrintEntryChoices(OptionsMenuData menuData, ushort entryIndex)
    {
        Window window = menuData.Windows[WindowEntries];
        uint y = (uint)(entryIndex << 4);
        sbyte xOffset = (sbyte)TableByte(EntryXOffsets, entryIndex);
        Bg.Window_FillRectWithColor(window, 0xFF, (ushort)(xOffset + 0x64), (ushort)y, 0x180, 0x10);
        OptionsMenuEntry entry = menuData.Entries[entryIndex];
        if (entryIndex == EntryMessageBoxStyle)
        {
            Text.Text_AddPrinterWithParamsAndColor(window, 0, entry.Choices[entry.Selected], 0x94, y, 0xFF, ColorSelected, null);
            Bg.Window_CopyToVRAM(window);
            menuData.RedrawMessageBox = true;
            return;
        }
        if (entryIndex == EntrySoundMode)
        {
            Sound.Sound_SetPlaybackMode(entry.Selected);
        }
        else if (entryIndex == EntryButtonMode)
        {
            Options.Options_SetSystemButtonMode(null, entry.Selected);
        }
        else if (entryIndex == EntryTextSpeed)
        {
            Options.Options_SetTextSpeed(menuData.SaveOptions!, entry.Selected);
            PrintEntryDescription(menuData, entryIndex, false);
        }
        sbyte buttonModeX = 0;
        for (int i = 0; i < entry.NumChoices; i++)
        {
            uint color = i == entry.Selected ? ColorSelected : ColorChoice;
            if (entryIndex == EntryButtonMode)
            {
                Text.Text_AddPrinterWithParamsAndColor(window, 0, entry.Choices[i], (uint)(buttonModeX + 0x64), y, 0xFF, color, null);
                buttonModeX = (sbyte)(buttonModeX + Font.Font_CalcStringWidth(0, entry.Choices[i], 0) + 0xC);
            }
            else
            {
                Text.Text_AddPrinterWithParamsAndColor(window, 0, entry.Choices[i], (uint)(xOffset + i * 0x30 + 0x64), y, 0xFF, color, null);
            }
        }
        Bg.Window_CopyToVRAM(window);
    }

    // OFFSET 0x021D1968
    private static void PrintBankEntryAsDescription(OptionsMenuData menuData, uint entry, bool scheduleVRAMCopy)
    {
        if (!IsTextPrinterDone(menuData))
        {
            Text.Text_RemovePrinter((byte)menuData.TextPrinter);
        }
        int delay = Options.Options_TextFrameDelay(menuData.SaveOptions!);
        Window window = menuData.Windows[WindowDescription];
        Bg.Window_FillTilemap(window, 0xF);
        GameString text = GameString.String_Init(0x100, menuData.HeapID);
        MessageLoader.MessageLoader_GetString(menuData.MsgLoader!, entry, text);
        if (!scheduleVRAMCopy)
        {
            menuData.TextPrinter = Text.Text_AddPrinterWithParamsAndColor(window, 1, text, 4, 0, (uint)delay, ColorChoice, null);
        }
        else
        {
            Text.Text_AddPrinterWithParamsAndColor(window, 1, text, 4, 0, 0xFF, ColorChoice, null);
            Bg.Window_ScheduleCopyToVRAM(window);
        }
        GameString.String_Free(text);
    }

    // OFFSET 0x021D1A08
    private static bool IsTextPrinterDone(OptionsMenuData menuData)
    {
        return Text.Text_IsPrinterActive((byte)menuData.TextPrinter) == 0;
    }

    private static void MoveCursor(OptionsMenuData menuData, int newCursor)
    {
        menuData.Cursor = newCursor;
        Bg.Bg_ScheduleScroll(menuData.BgConfig!, 0, 3, -(menuData.Cursor * 16 + 0x18));
        PrintEntryDescription(menuData, (ushort)menuData.Cursor, true);
        SoundPlayback.Sound_PlayEffect(SeqSelect);
    }

    // OFFSET 0x021D1A24
    private static void ProcessMainInput(OptionsMenuData menuData)
    {
        OptionsMenuEntry entry = menuData.Entries[menuData.Cursor];
        if (menuData.Cursor != EntryClose)
        {
            int keys = Globals.gSystem.PressedKeys;
            if ((keys & KeyRight) != 0)
            {
                entry.Selected = (ushort)((entry.Selected + 1) % entry.NumChoices);
                PrintEntryChoices(menuData, (ushort)menuData.Cursor);
                SoundPlayback.Sound_PlayEffect(SeqSelect);
            }
            else if ((keys & KeyLeft) != 0)
            {
                entry.Selected = (ushort)((entry.Selected + entry.NumChoices - 1) % entry.NumChoices);
                PrintEntryChoices(menuData, (ushort)menuData.Cursor);
                SoundPlayback.Sound_PlayEffect(SeqSelect);
            }
        }
        if ((Globals.gSystem.PressedKeys & KeyUp) != 0)
        {
            MoveCursor(menuData, (menuData.Cursor + 6) % 7);
            return;
        }
        if ((Globals.gSystem.PressedKeys & KeyDown) != 0)
        {
            MoveCursor(menuData, (menuData.Cursor + 1) % 7);
        }
    }

    // OFFSET 0x021D1B44
    private static bool ChangesWereMade(OptionsMenuData menuData)
    {
        if (GetOption(menuData, OptionTextSpeedShift, OptionTextSpeedMask) == menuData.Entries[EntryTextSpeed].Selected
            && GetOption(menuData, OptionBattleSceneShift, 1) == menuData.Entries[EntryBattleScene].Selected
            && GetOption(menuData, OptionBattleStyleShift, 1) == menuData.Entries[EntryBattleStyle].Selected
            && GetOption(menuData, OptionSoundModeShift, OptionSoundModeMask) == menuData.Entries[EntrySoundMode].Selected
            && GetOption(menuData, OptionButtonModeShift, OptionButtonModeMask) == menuData.Entries[EntryButtonMode].Selected
            && GetOption(menuData, OptionFrameShift, OptionFrameMask) == menuData.Entries[EntryMessageBoxStyle].Selected)
        {
            return false;
        }
        return true;
    }

    // OFFSET 0x021D1BA8
    private static void DrawConfirmationPrompt(OptionsMenuData menuData)
    {
        WindowTemplate template = RomStructs.WindowTemplate(RomData.Overlay(Overlay, YesNoWindowTemplate, 8), 0);
        menuData.YesNoChoice = Menu.Menu_MakeYesNoChoice(menuData.BgConfig!, template, StandardWindowTile, StandardWindowPalette, (uint)menuData.HeapID);
    }

    // OFFSET 0x021D1BD0
    private static uint ProcessConfirmationInput(OptionsMenuData menuData)
    {
        return Menu.Menu_ProcessInputAndHandleExit(menuData.YesNoChoice!, (uint)menuData.HeapID);
    }

    // OFFSET 0x021D1BE4
    private static void PrintEntryDescription(OptionsMenuData menuData, ushort entry, bool scheduleVRAMCopy)
    {
        PrintBankEntryAsDescription(menuData, TableByte(EntryDescriptionIDs, entry), scheduleVRAMCopy);
    }
}
