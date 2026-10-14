using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed class MainMenuAppData
{
    public const int OptionCount = 8;

    public BgConfig? BgConfig;
    public SaveData? SaveData;
    public Pokedex? Pokedex;
    public TrainerInfo? TrainerInfo;
    public PlayTime? PlayTime;
    public MysteryGift? MysteryGift;
    public int FramesCounter;
    public int NextOptionBaseTile;
    public int AgbGameType;
    public int RangerLinkAvailable;
    public int WiiConnectionAvailable;
    public int MysteryGiftUnlocked;
    public int ExtraUnlockedOptions;
    public int PendingAlerts;
    public int ShownAlerts;
    public int ShouldUpdateOptions;
    public int PokedexObtained;
    public int BadgeCount;
    public int FocusedOption;
    public int NextApplication;
    public readonly Window[] OptionWindows = { new(), new(), new(), new(), new(), new(), new(), new() };
    public readonly int[] OptionApps = new int[OptionCount];
    public readonly int[] OptionWirelessIconTypes = new int[OptionCount];
    public int ScrollPos;
    public int ScrollTarget;
    public int WirelessCheckState;
    public int WirelessCheckTimeout;
    public int AlertsState;
    public int AlertsPending;
    public int AlertsDelay;
    public int AlertDismissKeys;
    public readonly Window AlertWindow = new();
    public int IsNewGame;
    public int NewAlerts;
    public readonly int[] WfcUserInfoErasedMsgPending = new int[1];
    public readonly Window WfcUserInfoErasedWindow = new();
    public Sprite? ScrollUpArrowSprite;
    public Sprite? ScrollDownArrowSprite;
    public int FocusedBorderCycleIndex;
}

public delegate int MainMenuOptionRenderFunc(MainMenuAppData appData, int option, MainMenuWindow window, int yPos);

public static class MainMenu
{
    private const int Overlay = 97;
    private const int HeapID = 0x51;
    private const uint HeapSize = 0x40000;
    private const uint BanksAddress = 0x0223D694;
    private const uint GraphicsModesAddress = 0x0223D684;
    private const uint WfcUserInfoErasedTemplateAddress = 0x0223DF54;
    private const int WfcUserInfoErasedTemplateSize = 0x1C;
    private const int WfcUserInfoErasedTemplateCount = 1;
    private const uint ContinueStringIDsAddress = 0x0223DF44;
    private const uint BorderCycleColorsAddress = 0x0223DF70;
    private const uint AlertMysteryGiftOk = 0x0223DFB0;
    private const uint AlertMysteryGiftNoDex = 0x0223DFC4;
    private const uint AlertRangerLinkOk = 0x0223DFD8;
    private const uint AlertRangerLinkNoDex = 0x0223DFEC;
    private const uint AlertNewGame = 0x0223E000;
    private const uint OptionsAddress = 0x0223E014;
    private const int OptionTemplateSize = 0x10;
    private const uint GameLanguageAddress = 0x020E4C44;

    private const uint RenderContinueOptionAddress = 0x0222B5C0;
    private const uint RenderGBAMigrationOptionAddress = 0x0222B768;
    private const uint RenderMysteryGiftOptionAddress = 0x0222B7DC;
    private const uint RenderRangerLinkOptionAddress = 0x0222B888;
    private const uint RenderWiiConnectionOptionAddress = 0x0222B8E4;
    private const uint RenderWFCSettingsOptionAddress = 0x0222B934;
    private const uint RenderWiiMsgSettingsOptionAddress = 0x0222B978;

    private const int GameStartOverlay = 0x39;
    private const int MainMenuOverlay = 0x61;
    private const int WiiMessageOverlay = 0x62;
    private const int GameOpeningOverlay = 0x4D;
    private const uint LoadSaveTemplate = 0x021D0F70;
    private const uint RowanIntroTemplate = 0x021D0F90;
    private const uint MysteryGiftTemplate = 0x0223D71C;
    private const uint GBAMigratorTemplate = 0x0223D864;
    private const uint RangerLinkTemplate = 0x0223D6BC;
    private const uint WfcSettingsTemplate = 0x020F6DF0;
    private const uint WiiMessageTemplate = 0x02249BAC;
    private const uint TitleScreenTemplate = 0x021D742C;
    private const string WiiConnectionRom = "data/eoo.dat";

    private const int NextTitleScreen = 0;
    private const int NextLoadSave = 1;
    private const int NextGameIntro = 2;
    private const int NextMysteryGift = 3;
    private const int NextRangerLink = 4;
    private const int NextGBAMigration = 5;
    private const int NextWiiConnection = 6;
    private const int NextWfcSettings = 7;
    private const int NextWiiMsgSettings = 8;

    private const int StateInitGraphics = 0;
    private const int StateCheckWfcUserInfo = 1;
    private const int StateWarnWfcUserInfoErased = 2;
    private const int StateCheckNewGameAndGBA = 3;
    private const int StateLoadGraphics = 4;
    private const int StateSelectOption = 5;
    private const int StateConfirmNewGame = 6;
    private const int StateExit = 7;
    private const int StateWaitScreenTransition = 8;
    private const int StateUnused9 = 9;

    private const int WirelessCheckStart = 10;
    private const int WirelessCheckInitWireless = 11;
    private const int WirelessCheckIdle = 12;
    private const int WirelessCheckCheckResult = 13;
    private const int WirelessCheckStop = 14;

    private const int AlertsWait = 15;
    private const int AlertsLoadGraphics = 16;
    private const int AlertsShowNextAlert = 17;
    private const int AlertsWaitDismiss = 18;
    private const int AlertsHideAlert = 19;

    private const int WirelessIconNone = 0;
    private const int WirelessIconLocal = 1;
    private const int WirelessIconWifi = 2;

    private const int KeyA = 1;
    private const int KeyB = 2;
    private const int KeyUp = 0x40;
    private const int KeyDown = 0x80;
    private const int NewGameWarn = 0x80;
    private const int SoundSelect = 0x5DC;
    private const int MessageArchive = 0x1A;
    private const int OptionsTextBank = 0x226;
    private const int AlertsTextBank = 0xE;
    private const int GraphicsArchive = 0x74;
    private const int OptionWindowWidth = 0x1A;
    private const int OptionWindowsBaseTileStart = 0x13;
    private const int AlertWindowContentBaseTile = 0x13;
    private const int ScreenHeight = 0xC0;
    private const int MaxScrollSpeed = 0xC000;
    private const ushort BackgroundColor = 0x7D8C;
    private const ushort UnfocusedOptionBgColor = 0x6B5A;
    private const ushort WirelessIconsTileStart = 0x4380;
    private const uint MaleTextColor = 0x0007080F;
    private const uint FemaleTextColor = 0x0003040F;

    // OFFSET 0x0222AE60
    private static int CheckWFCUserInfoErased(MainMenuAppData appData)
    {
        return 0;
    }

    // OFFSET 0x0222AE64
    private static int ShowWFCUserInfoErasedMsg(MainMenuAppData appData)
    {
        if (Bg.Window_IsInUse(appData.WfcUserInfoErasedWindow) == 0)
        {
            for (int i = 0; i < WfcUserInfoErasedTemplateCount; i++)
            {
                if (appData.WfcUserInfoErasedMsgPending[i] == 1)
                {
                    appData.WfcUserInfoErasedMsgPending[i] = 0;
                    uint[] template = RomData.OverlayU32(Overlay, WfcUserInfoErasedTemplateAddress + (uint)(i * WfcUserInfoErasedTemplateSize), 7);
                    MainMenuWindow window = new();
                    MainMenuUtil.MainMenuUtil_InitWindow(window, appData.WfcUserInfoErasedWindow, 0, (int)template[4], 1, 2);
                    MainMenuUtil.MainMenuWindow_SetDimensionsAndBasetile(window, (int)template[2], (int)template[3], AlertWindowContentBaseTile);
                    MainMenuUtil.MainMenuUtil_ShowWindowAtPos(appData.BgConfig!, window, (int)template[0], (int)template[1], (int)template[5]);
                    return 1;
                }
            }
            return 0;
        }
        if ((gSystem.PressedKeys & (KeyA | KeyB)) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SoundSelect);
            RenderWindow.Window_EraseStandardFrame(appData.WfcUserInfoErasedWindow, 0);
            Bg.Window_Remove(appData.WfcUserInfoErasedWindow);
        }
        return 1;
    }

    // OFFSET 0x0222AF1C
    private static void DetectGBAGame(MainMenuAppData appData)
    {
        int result = GBACart.GBACart_Load(null);
        int version = 0;
        appData.AgbGameType = 0;
        if (result != 0)
        {
            return;
        }
        switch (GBACart.GBACart_GetAGBGameType())
        {
            case 0:
                version = 2;
                break;
            case 1:
                version = 1;
                break;
            case 2:
                version = 5;
                break;
            case 3:
                version = 4;
                break;
            case 4:
                version = 3;
                break;
        }
        GameSystem.SetGBACartridgeVersion(version);
        if (Pokedex.Pokedex_IsNationalDexObtained(appData.Pokedex!))
        {
            if (RomData.Arm9(GameLanguageAddress, 1)[0] == GBACart.GBACart_GetLanguage())
            {
                appData.AgbGameType = GBACart.GBACart_GetAGBGameType() + 1;
                MainMenuUtil.MainMenuUtil_UnsetGBACartIRQFunc();
            }
        }
    }

    // OFFSET 0x0222AF8C
    private static void DetectWirelessConnections(MainMenuAppData appData)
    {
        switch (appData.WirelessCheckState)
        {
            case WirelessCheckStart:
                appData.WirelessCheckState = WirelessCheckInitWireless;
                break;
            case WirelessCheckInitWireless:
                Unported.sub_02037D48(appData.SaveData!);
                appData.WirelessCheckTimeout = 0x78;
                appData.WirelessCheckState = WirelessCheckCheckResult;
                appData.PendingAlerts = 0;
                if (MysteryGift.MysteryGift_GetMysteryGiftUnlockedFlag(appData.MysteryGift!))
                {
                    appData.PendingAlerts |= 1;
                    appData.ShownAlerts |= 1;
                }
                break;
            case WirelessCheckIdle:
                break;
            case WirelessCheckCheckResult:
                if (appData.AlertsPending != 0)
                {
                    break;
                }
                int available = Unported.sub_02037DA0();
                int pending = appData.PendingAlerts;
                available &= ~pending;
                if (available != 0 && appData.AlertsState == AlertsWait && appData.AlertsPending == 0 && pending != available)
                {
                    appData.ShouldUpdateOptions = 1;
                    if ((available & 1) != 0)
                    {
                        appData.MysteryGiftUnlocked = 1;
                        available = 1;
                    }
                    if ((available & 2) != 0)
                    {
                        appData.RangerLinkAvailable = 1;
                        available = 2;
                    }
                    if ((available & 4) != 0)
                    {
                        appData.WiiConnectionAvailable = 1;
                        available = 4;
                    }
                    if ((available & 3) != 0)
                    {
                        appData.NewAlerts = 1;
                    }
                    appData.PendingAlerts |= available;
                }
                appData.WirelessCheckTimeout--;
                if (appData.WirelessCheckTimeout == 0)
                {
                    Unported.sub_02037D84();
                    appData.WirelessCheckState = WirelessCheckIdle;
                }
                break;
            case WirelessCheckStop:
                Unported.sub_02037D84();
                appData.WirelessCheckState = WirelessCheckIdle;
                break;
        }
    }

    // OFFSET 0x0222B07C
    private static int ShowAlerts(MainMenuAppData appData)
    {
        switch (appData.AlertsState)
        {
            case AlertsWait:
                if (appData.AlertsPending == 0)
                {
                    return 0;
                }
                appData.AlertsPending--;
                if (appData.AlertsPending == 0)
                {
                    appData.AlertsState = AlertsLoadGraphics;
                }
                return 1;
            case AlertsLoadGraphics:
                RenderWindow.LoadStandardWindowGraphics(appData.BgConfig!, 1, 1, 2, 0, HeapID);
                Bg.Bg_ClearTilemap(appData.BgConfig!, 1);
                SetMainBgColor(0x21, UnfocusedOptionBgColor);
                appData.AlertsState = AlertsShowNextAlert;
                break;
            case AlertsShowNextAlert:
            {
                MainMenuWindow window = new();
                MainMenuUtil.MainMenuUtil_InitWindow(window, appData.AlertWindow, 0, AlertsTextBank, 1, 2);
                int alerts = ~appData.ShownAlerts & appData.PendingAlerts;
                uint templateAddress = 0;
                if ((alerts & 1) != 0)
                {
                    templateAddress = (appData.ExtraUnlockedOptions & 1) != 0 ? AlertMysteryGiftOk : AlertMysteryGiftNoDex;
                }
                else if ((alerts & 2) != 0)
                {
                    templateAddress = (appData.ExtraUnlockedOptions & 2) != 0 ? AlertRangerLinkOk : AlertRangerLinkNoDex;
                }
                else if ((alerts & NewGameWarn) != 0)
                {
                    templateAddress = AlertNewGame;
                    alerts = 0;
                }
                appData.ShownAlerts |= alerts;
                uint[] template = RomData.OverlayU32(Overlay, templateAddress, 5);
                MainMenuUtil.MainMenuWindow_SetDimensionsAndBasetile(window, (int)template[2], (int)template[3], AlertWindowContentBaseTile);
                window.BgLayer = 1;
                MainMenuUtil.MainMenuUtil_ShowWindowAtPos(appData.BgConfig!, window, (int)template[0], (int)template[1], (int)template[4]);
                int x = Bg.Window_GetXPos(window.Window!);
                int y = Bg.Window_GetYPos(window.Window!);
                int width = Bg.Window_GetWidth(window.Window!);
                int height = Bg.Window_GetHeight(window.Window!);
                Bg.Bg_ChangeTilemapRectPalette(appData.BgConfig!, 1, (byte)x, (byte)y, (byte)width, (byte)height, 0);
                GXLayers.GXLayers_EngineAToggleLayers(1, 0);
                GXLayers.GXLayers_EngineAToggleLayers(4, 0);
                GXLayers.GXLayers_EngineAToggleLayers(2, 1);
                appData.AlertsDelay = 0x1E;
                appData.AlertsState = AlertsWaitDismiss;
                break;
            }
            case AlertsWaitDismiss:
                if (appData.AlertsDelay != 0)
                {
                    appData.AlertsDelay--;
                }
                else if ((gSystem.PressedKeys & (KeyA | KeyB)) != 0)
                {
                    Bg.Window_Remove(appData.AlertWindow);
                    appData.AlertsState = AlertsHideAlert;
                    appData.AlertDismissKeys = gSystem.PressedKeys;
                    SoundPlayback.Sound_PlayEffect(SoundSelect);
                }
                break;
            case AlertsHideAlert:
                GXLayers.GXLayers_EngineAToggleLayers(1, 1);
                GXLayers.GXLayers_EngineAToggleLayers(4, 1);
                GXLayers.GXLayers_EngineAToggleLayers(2, 0);
                appData.AlertsState = AlertsWait;
                break;
        }
        return 1;
    }

    // OFFSET 0x0222B25C
    private static void DoScrollStep(MainMenuAppData appData)
    {
        if (appData.ScrollPos == appData.ScrollTarget)
        {
            return;
        }
        int step = (appData.ScrollTarget - appData.ScrollPos) / 4;
        if (Math.Abs(step) > MaxScrollSpeed)
        {
            step = step > 0 ? MaxScrollSpeed : -MaxScrollSpeed;
        }
        appData.ScrollPos += step;
        if (Math.Abs(appData.ScrollTarget - appData.ScrollPos) < 0x200)
        {
            appData.ScrollPos = appData.ScrollTarget;
        }
        Bg.Bg_ScheduleScroll(appData.BgConfig!, 0, 3, appData.ScrollPos / 0x1000);
        Bg.Bg_ScheduleScroll(appData.BgConfig!, 2, 3, appData.ScrollPos / 0x1000);
    }

    // OFFSET 0x0222B2EC
    private static void InitMainMenuGraphics(MainMenuAppData appData)
    {
        GXLayers.GXLayers_SetBanks(RomStructs.Banks(RomData.Overlay(Overlay, BanksAddress, RomStructs.BanksSize), 0));
        Bg.SetAllGraphicsModes(RomStructs.GraphicsModes(RomData.Overlay(Overlay, GraphicsModesAddress, RomStructs.GraphicsModesSize), 0));
        MainMenuUtil.MainMenuUtil_InitBGLayer(appData.BgConfig!, 0, 2, 0xF000, 0);
        Video.SetBgControl(0, (ushort)((Video.GetBgControl(0) & ~3) | 2));
        Bg.Bg_ClearTilesRange(0, 0x20, 0, HeapID);
        MainMenuUtil.MainMenuUtil_InitBGLayer(appData.BgConfig!, 1, 1, 0xD800, 0x8000);
        Video.SetBgControl(1, (ushort)((Video.GetBgControl(1) & ~3) | 1));
        Bg.Bg_ClearTilesRange(1, 0x20, 0, HeapID);
        MainMenuUtil.MainMenuUtil_InitBGLayer(appData.BgConfig!, 2, 2, 0xE000, 0);
        Video.SetBgControl(2, (ushort)(Video.GetBgControl(2) & ~3));
        Bg.Bg_ClearTilesRange(2, 0x20, 0, HeapID);
        Text.Text_ResetAllPrinters();
        Font.Font_LoadTextPalette(0, 0x20, HeapID);
        Font.Font_LoadTextPalette(0, 0, HeapID);
        SetMainBgColor(0, 0);
        SetMainBgColor(0x1F, UnfocusedOptionBgColor);
        RenderWindow.LoadStandardWindowGraphics(appData.BgConfig!, 0, 1, 2, 0, HeapID);
        RenderWindow.LoadStandardWindowGraphics(appData.BgConfig!, 0, 0xA, 3, 1, HeapID);
        SetMainBgColor(0x21, UnfocusedOptionBgColor);
    }

    // OFFSET 0x0222B404
    private static void LoadScrollArrowsSprites(MainMenuAppData appData)
    {
        MainMenuUtil.MainMenuUtil_InitCharPlttTransferBuffers();
        MainMenuUtil.MainMenuUtil_InitSpriteLoader();
        MainMenuUtil.MainMenuUtil_LoadSprite(GraphicsArchive, 0x2B, 0x28, 0x2A, 0x29, 0);
        appData.ScrollUpArrowSprite = MainMenuUtil.MainMenuUtil_InitSprite(0, appData.ScrollUpArrowSprite, 0x80, 8, 0);
        Sprite.Sprite_SetDrawFlag(appData.ScrollUpArrowSprite, 0);
        appData.ScrollDownArrowSprite = MainMenuUtil.MainMenuUtil_InitSprite(0, appData.ScrollDownArrowSprite, 0x80, 0xB8, 1);
        Sprite.Sprite_SetDrawFlag(appData.ScrollDownArrowSprite, 0);
    }

    // OFFSET 0x0222B46C
    private static void LoadWirelessIconsGraphics(MainMenuAppData appData)
    {
        Graphics.Graphics_LoadPalette(GraphicsArchive, 0x2D, 0, 0x80, 0x20, HeapID);
        Graphics.Graphics_LoadTilesToBgLayer(GraphicsArchive, 0x2C, appData.BgConfig!, 2, 0x380, 0x800, 0, HeapID);
    }

    // OFFSET 0x0222B4AC
    private static void DrawWirelessIcon(MainMenuAppData appData, int column, int row, int type)
    {
        byte[] tilemap = Bg.Bg_GetTilemapBuffer(appData.BgConfig!, 2)!;
        ushort tile = WirelessIconsTileStart;
        if (type == WirelessIconWifi)
        {
            tile += 2;
        }
        WriteTile(tilemap, row * 32 + column, tile);
        WriteTile(tilemap, row * 32 + column + 1, (ushort)(tile + 1));
        WriteTile(tilemap, (row + 1) * 32 + column, (ushort)(tile + 8));
        WriteTile(tilemap, (row + 1) * 32 + column + 1, (ushort)(tile + 9));
        Bg.Bg_CopyTilemapBufferToVRAM(appData.BgConfig!, 2);
    }

    // OFFSET 0x0222B4FC
    private static void ClearWirelessIcon(MainMenuAppData appData, int column, int row)
    {
        byte[] tilemap = Bg.Bg_GetTilemapBuffer(appData.BgConfig!, 2)!;
        ushort tile = WirelessIconsTileStart + 4;
        WriteTile(tilemap, row * 32 + column, tile);
        WriteTile(tilemap, row * 32 + column + 1, tile);
        WriteTile(tilemap, (row + 1) * 32 + column, tile);
        WriteTile(tilemap, (row + 1) * 32 + column + 1, tile);
        Bg.Bg_CopyTilemapBufferToVRAM(appData.BgConfig!, 2);
    }

    // OFFSET 0x0222B53C
    private static void PrintRightAlignedWithMargin(Window window, MessageLoader loader, StringTemplate template, uint textColor, uint entryID, uint yOffset)
    {
        GameString? text = MessageUtil.MessageUtil_ExpandedString(template, loader, entryID, HeapID);
        int width = Font.Font_CalcStringWidth(0, text, Font.Font_GetAttribute(0, 2));
        int x = Bg.Window_GetWidth(window) * 8 - (width + 0x20);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, text, (uint)x, yOffset, 0xFF, textColor, null);
        GameString.String_Free(text);
    }

    // OFFSET 0x0222B590
    private static void SetTemplateNumberCustomFormatting(StringTemplate template, int number)
    {
        uint maxDigits;
        int paddingMode;
        if (number >= 100)
        {
            maxDigits = 3;
            paddingMode = 0;
        }
        else if (number >= 10)
        {
            maxDigits = 3;
            paddingMode = 1;
        }
        else
        {
            maxDigits = 2;
            paddingMode = 1;
        }
        StringTemplate.StringTemplate_SetNumber(template, 0, number, maxDigits, paddingMode, 1);
    }

    // OFFSET 0x0222B5C0
    private static int RenderContinueOption(MainMenuAppData appData, int option, MainMenuWindow window, int yPos)
    {
        MessageLoader loader = MessageLoader.MessageLoader_Init(1, MessageArchive, OptionsTextBank, HeapID)!;
        StringTemplate template = StringTemplate.StringTemplate_Default(HeapID);
        uint textColor = TrainerInfo.TrainerInfo_Gender(appData.TrainerInfo!) == 1 ? FemaleTextColor : MaleTextColor;
        MainMenuUtil.MainMenuUtil_ShowWindowAtPos(appData.BgConfig!, window, 3, yPos, (int)OptionTemplate(option)[2]);
        uint[] stringIDs = RomData.OverlayU32(Overlay, ContinueStringIDsAddress, 4);
        for (int i = 1; i < 5; i++)
        {
            if (i == 4 && appData.PokedexObtained == 0)
            {
                continue;
            }
            GameString? text = MessageUtil.MessageUtil_ExpandedString(template, loader, stringIDs[i - 1], HeapID);
            Text.Text_AddPrinterWithParamsAndColor(window.Window!, 0, text, 0x20, (uint)(i * 0x10), 0xFF, textColor, null);
            GameString.String_Free(text);
        }
        StringTemplate.StringTemplate_SetPlayerName(template, 0, appData.TrainerInfo!);
        PrintRightAlignedWithMargin(window.Window!, loader, template, textColor, 0x10, 0x10);
        SetTemplateNumberCustomFormatting(template, PlayTime.PlayTime_GetHours(appData.PlayTime!));
        StringTemplate.StringTemplate_SetNumber(template, 1, PlayTime.PlayTime_GetMinutes(appData.PlayTime!), 2, 2, 1);
        PrintRightAlignedWithMargin(window.Window!, loader, template, textColor, 0x11, 0x20);
        StringTemplate.StringTemplate_SetNumber(template, 0, appData.BadgeCount, 1, 0, 1);
        PrintRightAlignedWithMargin(window.Window!, loader, template, textColor, 0x13, 0x30);
        if (appData.PokedexObtained != 0)
        {
            SetTemplateNumberCustomFormatting(template, Pokedex.Pokedex_CountSeen(appData.Pokedex!));
            PrintRightAlignedWithMargin(window.Window!, loader, template, textColor, 0x12, 0x40);
        }
        RenderWindow.Window_DrawStandardFrame(window.Window!, 0, (ushort)window.FrameBaseTile, (byte)window.FramePltt);
        appData.OptionApps[option] = (int)OptionTemplate(option)[0];
        StringTemplate.StringTemplate_Free(template);
        MessageLoader.MessageLoader_Free(loader);
        return 1;
    }

    // OFFSET 0x0222B768
    private static int RenderGBAMigrationOption(MainMenuAppData appData, int option, MainMenuWindow window, int yPos)
    {
        int textEntryID = 0;
        if (appData.AgbGameType == 0)
        {
            return 0;
        }
        switch (appData.AgbGameType - 1)
        {
            case 0:
                textEntryID = 4;
                break;
            case 1:
                textEntryID = 5;
                break;
            case 2:
                textEntryID = 6;
                break;
            case 3:
                textEntryID = 7;
                break;
            case 4:
                textEntryID = 8;
                break;
        }
        MainMenuUtil.MainMenuUtil_ShowWindowAtPos(appData.BgConfig!, window, 3, yPos, textEntryID);
        ClearWirelessIcon(appData, OptionWindowWidth, yPos);
        appData.OptionApps[option] = (int)OptionTemplate(option)[0];
        return 1;
    }

    // OFFSET 0x0222B7DC
    private static int RenderMysteryGiftOption(MainMenuAppData appData, int option, MainMenuWindow window, int yPos)
    {
        if (appData.MysteryGiftUnlocked == 0)
        {
            if (MysteryGift.MysteryGift_GetMysteryGiftUnlockedFlag(appData.MysteryGift!))
            {
                appData.MysteryGiftUnlocked = 1;
            }
            if (SystemData.SystemData_IsMysteryGiftUnlocked(SystemData.SaveData_GetSystemData(appData.SaveData!)) == 1)
            {
                appData.MysteryGiftUnlocked = 1;
            }
            DistributionCartridge.DistributionCartridge_UseHeap(HeapID);
            if (DistributionCartridge.DistributionCartridge_ReadLength() != 0)
            {
                appData.MysteryGiftUnlocked = 1;
                MainMenuUtil.MainMenuUtil_ToggleTerminateOnGBACartRemoved(1);
            }
        }
        if (appData.MysteryGiftUnlocked == 1 && appData.PokedexObtained == 0)
        {
            appData.MysteryGiftUnlocked = 0;
        }
        if (appData.MysteryGiftUnlocked == 1)
        {
            MainMenuUtil.MainMenuUtil_ShowWindowAtPos(appData.BgConfig!, window, 3, yPos, (int)OptionTemplate(option)[2]);
            ClearWirelessIcon(appData, OptionWindowWidth, yPos);
            appData.OptionApps[option] = (int)OptionTemplate(option)[0];
            appData.ExtraUnlockedOptions |= 1;
            MysteryGift.MysteryGift_SetMysteryGiftUnlockedFlag(appData.MysteryGift!);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0222B888
    private static int RenderRangerLinkOption(MainMenuAppData appData, int option, MainMenuWindow window, int yPos)
    {
        if (appData.RangerLinkAvailable == 1 && appData.PokedexObtained == 1)
        {
            MainMenuUtil.MainMenuUtil_ShowWindowAtPos(appData.BgConfig!, window, 3, yPos, (int)OptionTemplate(option)[2]);
            DrawWirelessIcon(appData, OptionWindowWidth, yPos, WirelessIconLocal);
            appData.OptionWirelessIconTypes[option] = WirelessIconLocal;
            appData.OptionApps[option] = (int)OptionTemplate(option)[0];
            appData.ExtraUnlockedOptions |= 2;
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0222B8E4
    private static int RenderWiiConnectionOption(MainMenuAppData appData, int option, MainMenuWindow window, int yPos)
    {
        if (appData.WiiConnectionAvailable == 1)
        {
            MainMenuUtil.MainMenuUtil_ShowWindowAtPos(appData.BgConfig!, window, 3, yPos, (int)OptionTemplate(option)[2]);
            appData.OptionWirelessIconTypes[option] = WirelessIconLocal;
            DrawWirelessIcon(appData, OptionWindowWidth, yPos, WirelessIconLocal);
            appData.OptionApps[option] = (int)OptionTemplate(option)[0];
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0222B934
    private static int RenderWFCSettingsOption(MainMenuAppData appData, int option, MainMenuWindow window, int yPos)
    {
        MainMenuUtil.MainMenuUtil_ShowWindowAtPos(appData.BgConfig!, window, 3, yPos, (int)OptionTemplate(option)[2]);
        appData.OptionWirelessIconTypes[option] = WirelessIconWifi;
        DrawWirelessIcon(appData, OptionWindowWidth, yPos, WirelessIconWifi);
        appData.OptionApps[option] = (int)OptionTemplate(option)[0];
        return 1;
    }

    // OFFSET 0x0222B978
    private static int RenderWiiMsgSettingsOption(MainMenuAppData appData, int option, MainMenuWindow window, int yPos)
    {
        MainMenuUtil.MainMenuUtil_ShowWindowAtPos(appData.BgConfig!, window, 3, yPos, (int)OptionTemplate(option)[2]);
        appData.OptionWirelessIconTypes[option] = WirelessIconNone;
        ClearWirelessIcon(appData, OptionWindowWidth, yPos);
        appData.OptionApps[option] = (int)OptionTemplate(option)[0];
        return 1;
    }

    // OFFSET 0x0222B9BC
    private static int RenderOptions(MainMenuAppData appData)
    {
        int rendered = 0;
        int yPos = 1;
        appData.NextOptionBaseTile = OptionWindowsBaseTileStart;
        for (int i = 0; i < MainMenuAppData.OptionCount; i++)
        {
            uint[] template = OptionTemplate(i);
            int height = (int)template[1];
            MainMenuWindow window = new();
            MainMenuUtil.MainMenuUtil_InitWindow(window, appData.OptionWindows[i], 1, OptionsTextBank, 1, 2);
            MainMenuUtil.MainMenuWindow_SetDimensionsAndBasetile(window, OptionWindowWidth, height, appData.NextOptionBaseTile);
            MainMenuOptionRenderFunc? renderFunc = RenderFuncAt(template[3]);
            if (renderFunc != null)
            {
                if (appData.OptionApps[i] != 0)
                {
                    Bg.Window_SetXPos(window.Window!, 3);
                    Bg.Window_SetYPos(window.Window!, (byte)yPos);
                    RenderWindow.Window_DrawStandardFrame(window.Window!, 0, (ushort)window.FrameBaseTile, (byte)window.FramePltt);
                    if (appData.OptionWirelessIconTypes[i] != 0)
                    {
                        DrawWirelessIcon(appData, OptionWindowWidth, yPos, appData.OptionWirelessIconTypes[i]);
                    }
                    else
                    {
                        ClearWirelessIcon(appData, OptionWindowWidth, yPos);
                    }
                    yPos += height + 2;
                    rendered = 1;
                }
                else if (renderFunc(appData, i, window, yPos) == 1)
                {
                    yPos += height + 2;
                    rendered = 1;
                }
            }
            else
            {
                MainMenuUtil.MainMenuUtil_ShowWindowAtPos(appData.BgConfig!, window, 3, yPos, (int)template[2]);
                appData.OptionApps[i] = (int)template[0];
                yPos += height + 2;
            }
            appData.NextOptionBaseTile += OptionWindowWidth * height;
        }
        return rendered;
    }

    // OFFSET 0x0222BAD8
    private static void RenderOptionsFrames(MainMenuAppData appData, int focusedOption)
    {
        for (int i = 0; i < MainMenuAppData.OptionCount; i++)
        {
            Window window = appData.OptionWindows[i];
            if (Bg.Window_IsInUse(window) == 0)
            {
                continue;
            }
            if (i == focusedOption)
            {
                RenderWindow.Window_DrawStandardFrame(window, 1, 0xA, 3);
                int x = Bg.Window_GetXPos(window);
                int y = Bg.Window_GetYPos(window);
                int width = Bg.Window_GetWidth(window);
                int height = Bg.Window_GetHeight(window);
                Bg.Bg_ChangeTilemapRectPalette(appData.BgConfig!, 0, (byte)x, (byte)y, (byte)width, (byte)height, 0);
            }
            else
            {
                RenderWindow.Window_DrawStandardFrame(window, 1, 1, 2);
                int x = Bg.Window_GetXPos(window);
                int y = Bg.Window_GetYPos(window);
                int width = Bg.Window_GetWidth(window);
                int height = Bg.Window_GetHeight(window);
                Bg.Bg_ChangeTilemapRectPalette(appData.BgConfig!, 0, (byte)x, (byte)y, (byte)width, (byte)height, 1);
            }
        }
        Bg.Bg_CopyTilemapBufferToVRAM(appData.BgConfig!, 0);
    }

    // OFFSET 0x0222BB88
    private static void FocusNextOption(MainMenuAppData appData, int direction)
    {
        int current = appData.FocusedOption;
        int option = current;
        while (true)
        {
            option += direction;
            if (option == -1)
            {
                option = 0;
            }
            if (option == MainMenuAppData.OptionCount)
            {
                option = MainMenuAppData.OptionCount - 1;
            }
            if (option == current)
            {
                break;
            }
            if (appData.OptionApps[option] != 0)
            {
                SoundPlayback.Sound_PlayEffect(SoundSelect);
                break;
            }
        }
        appData.FocusedOption = option;
    }

    // OFFSET 0x0222BBC8
    private static void TargetFocusedOptionForScroll(MainMenuAppData appData)
    {
        int top = (Bg.Window_GetYPos(appData.OptionWindows[appData.FocusedOption]) - 1) * 8;
        int height = (Bg.Window_GetHeight(appData.OptionWindows[appData.FocusedOption]) + 2) * 8;
        int scroll = appData.ScrollTarget / 0x1000;
        if (scroll > top)
        {
            appData.ScrollTarget = top << 12;
        }
        if (scroll + ScreenHeight <= top)
        {
            appData.ScrollTarget = (top + height - ScreenHeight) << 12;
        }
    }

    // OFFSET 0x0222BC1C
    private static void DrawScrollArrows(MainMenuAppData appData)
    {
        int showDown = 0;
        int showUp = 0;
        int scroll = appData.ScrollTarget / 0x1000;
        int bottom = scroll + ScreenHeight;
        for (int i = 0; i < MainMenuAppData.OptionCount; i++)
        {
            Window window = appData.OptionWindows[i];
            if (Bg.Window_IsInUse(window) != 0)
            {
                int top = (Bg.Window_GetYPos(window) - 1) * 8;
                Bg.Window_GetHeight(window);
                if (scroll > top)
                {
                    showUp = 1;
                }
                if (bottom <= top)
                {
                    showDown = 1;
                }
            }
        }
        Sprite.Sprite_SetDrawFlag(appData.ScrollUpArrowSprite!, showUp);
        Sprite.Sprite_SetDrawFlag(appData.ScrollDownArrowSprite!, showDown);
    }

    // OFFSET 0x0222BC9C
    private static void FreeApplicationResources(ApplicationManager appMan)
    {
        MainMenuAppData appData = (MainMenuAppData)ApplicationManager.ApplicationManager_Data(appMan)!;
        if (appData.ScrollUpArrowSprite != null || appData.ScrollDownArrowSprite != null)
        {
            Sprite.Sprite_Delete(appData.ScrollUpArrowSprite!);
            Sprite.Sprite_Delete(appData.ScrollDownArrowSprite!);
            MainMenuUtil.MainMenuUtil_FreeSprites();
        }
        for (int i = 0; i < MainMenuAppData.OptionCount; i++)
        {
            if (appData.OptionWindows[i].BgConfig != null)
            {
                Bg.Window_ClearAndCopyToVRAM(appData.OptionWindows[i]);
                Bg.Window_Remove(appData.OptionWindows[i]);
            }
        }
        Bg.Bg_FreeTilemapBuffer(appData.BgConfig!, 0);
        Bg.Bg_FreeTilemapBuffer(appData.BgConfig!, 1);
        Bg.Bg_FreeTilemapBuffer(appData.BgConfig!, 2);
        appData.BgConfig = null;
        GameSystem.SetVBlankCallback(null, null);
    }

    // OFFSET 0x0222BD14
    private static void DoColorCycleStep(MainMenuAppData appData)
    {
        if (BorderCycleColor(appData.FocusedBorderCycleIndex) == 0)
        {
            appData.FocusedBorderCycleIndex = 0;
        }
        int index = appData.FocusedBorderCycleIndex;
        appData.FocusedBorderCycleIndex = index + 1;
        SetMainBgColor(0x36, BorderCycleColor(index));
    }

    // OFFSET 0x0222BD48
    private static void MainMenuVBlankCallback(object? data)
    {
        VramTransfer.VramTransfer_Process();
        RenderOam.RenderOam_Transfer();
        Bg.Bg_RunScheduledUpdates(data!);
    }

    // OFFSET 0x0222BD70
    public static int MainMenu_Init(ApplicationManager appMan, ref int state)
    {
        Heap.Heap_Create(3, HeapID, HeapSize);
        MainMenuAppData appData = new();
        appMan.Data = appData;
        appData.BgConfig = Bg.BgConfig_New(HeapID);
        ScreenFadeControl.SetScreenColorBrightness(0, 0);
        ScreenFadeControl.SetScreenColorBrightness(1, 0);
        appData.SaveData = ((ApplicationArgs)ApplicationManager.ApplicationManager_Args(appMan)!).SaveData;
        appData.MysteryGift = (MysteryGift)SaveDataTable.SaveData_GetMysteryGift(appData.SaveData!);
        appData.ScrollPos = 0;
        appData.ScrollTarget = 0;
        appData.TrainerInfo = PlayerSave.SaveData_GetTrainerInfo(appData.SaveData!);
        appData.Pokedex = Pokedex.SaveData_GetPokedex(appData.SaveData!);
        appData.PlayTime = PlayerSave.SaveData_GetPlayTime(appData.SaveData!);
        appData.PokedexObtained = Pokedex.Pokedex_IsObtained(appData.Pokedex) ? 1 : 0;
        appData.BadgeCount = TrainerInfo.TrainerInfo_BadgeCount(appData.TrainerInfo);
        appData.AlertsState = AlertsWait;
        MainMenuUtil.MainMenuUtil_Init(HeapID);
        if (SaveData.SaveData_DataExists(appData.SaveData!) == 0)
        {
            appData.IsNewGame = 1;
        }
        Sound.Sound_ConfigureBGMChannelsAndReverb(0);
        Sound.Sound_SetScene(0);
        return 1;
    }

    // OFFSET 0x0222BE24
    public static int MainMenu_Main(ApplicationManager appMan, ref int state)
    {
        MainMenuAppData appData = (MainMenuAppData)ApplicationManager.ApplicationManager_Data(appMan)!;
        appData.FramesCounter++;
        Cartridge.IsExisting();
        if (ShowAlerts(appData) == 1)
        {
            DetectWirelessConnections(appData);
            DoScrollStep(appData);
            return 0;
        }
        DoColorCycleStep(appData);
        switch (state)
        {
            case StateInitGraphics:
                InitMainMenuGraphics(appData);
                state = StateCheckWfcUserInfo;
                break;
            case StateCheckWfcUserInfo:
                if (CheckWFCUserInfoErased(appData) == 0)
                {
                    state = StateCheckNewGameAndGBA;
                }
                else
                {
                    MainMenuUtil.MainMenuUtil_StartScreenFadeToState(1, StateWarnWfcUserInfoErased, ref state, StateWaitScreenTransition);
                    SetMainBgColor(0, BackgroundColor);
                }
                break;
            case StateWarnWfcUserInfoErased:
                if (ShowWFCUserInfoErasedMsg(appData) == 0)
                {
                    MainMenuUtil.MainMenuUtil_StartScreenFadeToState(0, StateCheckNewGameAndGBA, ref state, StateWaitScreenTransition);
                }
                break;
            case StateCheckNewGameAndGBA:
                appData.WirelessCheckState = WirelessCheckIdle;
                if (appData.IsNewGame == 1)
                {
                    appData.NextApplication = NextGameIntro;
                    MainMenuUtil.MainMenuUtil_StartScreenFadeToState(0, StateExit, ref state, StateWaitScreenTransition);
                }
                else
                {
                    DetectGBAGame(appData);
                    state = StateLoadGraphics;
                }
                break;
            case StateLoadGraphics:
                LoadScrollArrowsSprites(appData);
                LoadWirelessIconsGraphics(appData);
                GameSystem.SetVBlankCallback(MainMenuVBlankCallback, appData.BgConfig);
                RenderOptions(appData);
                RenderOptionsFrames(appData, appData.FocusedOption);
                MainMenuUtil.MainMenuUtil_StartScreenFadeToState(1, StateSelectOption, ref state, StateWaitScreenTransition);
                SetMainBgColor(0, BackgroundColor);
                appData.WirelessCheckState = WirelessCheckStart;
                break;
            case StateSelectOption:
            {
                int pressed = gSystem.PressedKeys;
                if ((pressed & (KeyA | KeyB)) != 0)
                {
                    if ((pressed & KeyA) != 0)
                    {
                        SoundPlayback.Sound_PlayEffect(SoundSelect);
                        appData.NextApplication = appData.OptionApps[appData.FocusedOption];
                        if (appData.NextApplication == NextGBAMigration && Cartridge.IsPulledOut())
                        {
                            if (appData.WirelessCheckState != WirelessCheckIdle)
                            {
                                Unported.sub_02037D84();
                            }
                            Unported.sub_0209A8E0(HeapID);
                        }
                    }
                    else
                    {
                        SoundPlayback.Sound_PlayEffect(SoundSelect);
                        appData.NextApplication = NextTitleScreen;
                        MainMenuUtil.MainMenuUtil_SetFadeToWhite(1);
                    }
                    if (appData.NextApplication == NextGameIntro)
                    {
                        appData.PendingAlerts |= NewGameWarn;
                        appData.AlertsPending = 1;
                        state = StateConfirmNewGame;
                    }
                    else
                    {
                        if ((uint)(appData.NextApplication - NextWiiConnection) <= 1)
                        {
                            MainMenuUtil.MainMenuUtil_SetFadeToWhite(1);
                        }
                        MainMenuUtil.MainMenuUtil_StartScreenFadeToState(0, StateExit, ref state, StateWaitScreenTransition);
                    }
                    if (appData.WirelessCheckState == WirelessCheckCheckResult)
                    {
                        appData.WirelessCheckState = WirelessCheckStop;
                    }
                }
                else if (appData.ShouldUpdateOptions == 1)
                {
                    RenderOptions(appData);
                    appData.ShouldUpdateOptions = 0;
                }
                else
                {
                    if ((pressed & KeyUp) != 0)
                    {
                        FocusNextOption(appData, -1);
                    }
                    if ((gSystem.PressedKeys & KeyDown) != 0)
                    {
                        FocusNextOption(appData, 1);
                    }
                    RenderOptionsFrames(appData, appData.FocusedOption);
                    TargetFocusedOptionForScroll(appData);
                    DrawScrollArrows(appData);
                    if (state == StateSelectOption && appData.NewAlerts == 1)
                    {
                        appData.NewAlerts = 0;
                        appData.AlertsPending = 1;
                    }
                }
                break;
            }
            case StateConfirmNewGame:
                if (appData.AlertsState == AlertsWait)
                {
                    if ((appData.AlertDismissKeys & KeyB) != 0)
                    {
                        state = StateSelectOption;
                    }
                    else
                    {
                        MainMenuUtil.MainMenuUtil_StartScreenFadeToState(0, StateExit, ref state, StateWaitScreenTransition);
                    }
                }
                break;
            case StateExit:
                FreeApplicationResources(appMan);
                return 1;
            case StateWaitScreenTransition:
                MainMenuUtil.MainMenuUtil_CheckScreenFadeDone(ref state);
                break;
            case StateUnused9:
                return 1;
        }
        DetectWirelessConnections(appData);
        DoScrollStep(appData);
        MainMenuUtil.MainMenuUtil_UpdateSprites();
        return 0;
    }

    // OFFSET 0x0222C094
    private static void EnqueueNextApplication(MainMenuAppData appData)
    {
        switch (appData.NextApplication)
        {
            case NextLoadSave:
                Main.EnqueueApplication(GameStartOverlay, ApplicationTemplate.At(LoadSaveTemplate));
                break;
            case NextGameIntro:
                Main.EnqueueApplication(GameStartOverlay, ApplicationTemplate.At(RowanIntroTemplate));
                break;
            case NextMysteryGift:
                Main.EnqueueApplication(MainMenuOverlay, ApplicationTemplate.At(MysteryGiftTemplate));
                break;
            case NextGBAMigration:
                Main.EnqueueApplication(MainMenuOverlay, ApplicationTemplate.At(GBAMigratorTemplate));
                break;
            case NextRangerLink:
                Main.EnqueueApplication(MainMenuOverlay, ApplicationTemplate.At(RangerLinkTemplate));
                break;
            case NextWiiConnection:
                Unported.sub_020243E0(WiiConnectionRom);
                break;
            case NextWfcSettings:
                SoundPlayback.Sound_StopWaveOutAndSequences();
                Main.EnqueueApplication(-1, ApplicationTemplate.At(WfcSettingsTemplate));
                break;
            case NextWiiMsgSettings:
                SoundPlayback.Sound_StopWaveOutAndSequences();
                Main.EnqueueApplication(WiiMessageOverlay, ApplicationTemplate.At(WiiMessageTemplate));
                break;
            case NextTitleScreen:
                Main.EnqueueApplication(GameOpeningOverlay, ApplicationTemplate.At(TitleScreenTemplate));
                break;
        }
    }

    // OFFSET 0x0222C150
    public static int MainMenu_Exit(ApplicationManager appMan, ref int state)
    {
        EnqueueNextApplication((MainMenuAppData)ApplicationManager.ApplicationManager_Data(appMan)!);
        ApplicationManager.ApplicationManager_FreeData(appMan);
        Heap.Heap_Destroy(HeapID);
        MainMenuUtil.MainMenuUtil_ToggleTerminateOnGBACartRemoved(0);
        return 1;
    }

    private static uint[] OptionTemplate(int option)
    {
        return RomData.OverlayU32(Overlay, OptionsAddress + (uint)(option * OptionTemplateSize), 4);
    }

    private static MainMenuOptionRenderFunc? RenderFuncAt(uint address)
    {
        switch (address & ~1u)
        {
            case RenderContinueOptionAddress:
                return RenderContinueOption;
            case RenderGBAMigrationOptionAddress:
                return RenderGBAMigrationOption;
            case RenderMysteryGiftOptionAddress:
                return RenderMysteryGiftOption;
            case RenderRangerLinkOptionAddress:
                return RenderRangerLinkOption;
            case RenderWiiConnectionOptionAddress:
                return RenderWiiConnectionOption;
            case RenderWFCSettingsOptionAddress:
                return RenderWFCSettingsOption;
            case RenderWiiMsgSettingsOptionAddress:
                return RenderWiiMsgSettingsOption;
        }
        return null;
    }

    private static ushort BorderCycleColor(int index)
    {
        return RomData.OverlayU16(Overlay, BorderCycleColorsAddress + (uint)(index * 2), 1)[0];
    }

    private static void SetMainBgColor(int index, ushort color)
    {
        Bg.Bg_LoadPalette(0, new[] { color }, 0, 2, (ushort)(index * 2));
    }

    private static void WriteTile(byte[] tilemap, int index, ushort tile)
    {
        tilemap[index * 2] = (byte)tile;
        tilemap[index * 2 + 1] = (byte)(tile >> 8);
    }
}
