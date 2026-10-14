using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed class SaveStatusScreenData
{
    public int HeapID;
    public int State;
    public int MessageID;
    public int TextState;
    public int PrinterID;
    public GameString? Text;
    public BgConfig? BgConfig;
    public MessageLoader? Loader;
    public readonly Window Window = new();
    public SaveData? SaveData;
    public uint Status;
}

public static class SaveStatusScreen
{
    private const int HeapID = 0x58;
    private const int HeapSize = 0x20000;
    private const int MainMenuOverlay = 0x61;
    private const uint MainMenuTemplate = 0x0223D674;
    private const uint BanksAddress = 0x020F8A8C;
    private const uint GraphicsModesAddress = 0x020F8A60;
    private const uint BgTemplateAddress = 0x020F8A70;
    private const uint WindowTemplateAddress = 0x020F8A58;
    private const uint AllLayersMask = 0xFFFFE0FF;
    private const int MessageArchive = 0x1A;
    private const int MessageBank = 0x196;
    private const uint MessageBoxTile = 0x1E2;
    private const uint WindowFrameTile = 0x1D9;
    private const ushort BackgroundColor = 0x6C21;
    private const int KeyA = 1;
    private const int TextDelay = 4;

    // OFFSET 0x0209A2C4
    public static int SaveStatusScreen_Init(ApplicationManager appMan, ref int state)
    {
        Heap.Heap_Create(3, HeapID, HeapSize);
        SaveStatusScreenData data = new();
        appMan.Data = data;
        data.HeapID = HeapID;
        data.State = 0;
        data.SaveData = ((ApplicationArgs)ApplicationManager.ApplicationManager_Args(appMan)!).SaveData;
        return 1;
    }

    // OFFSET 0x0209A300
    public static int SaveStatusScreen_Main(ApplicationManager appMan, ref int state)
    {
        SaveStatusScreenData data = (SaveStatusScreenData)ApplicationManager.ApplicationManager_Data(appMan)!;
        int finished = 0;
        switch (state)
        {
            case 0:
                ScreenFadeControl.SetScreenColorBrightness(0, 0);
                ScreenFadeControl.SetScreenColorBrightness(1, 0);
                GameSystem.SetVBlankCallback(null, null);
                GameSystem.SetHBlankCallback(null, null);
                GXLayers.GXLayers_DisableEngineALayers();
                GXLayers.GXLayers_DisableEngineBLayers();
                Video.SetDisplayControl(0, Video.GetDisplayControl(0) & AllLayersMask);
                Video.SetDisplayControl(1, Video.GetDisplayControl(1) & AllLayersMask);
                GameSystem.SetAutorepeat(4, 8);
                SaveStatusScreen_InitBgs(data);
                SaveStatusScreen_InitWindow(data);
                GXLayers.GXLayers_TurnBothDispOn();
                state = 1;
                break;
            case 1:
                if (SaveStatusScreen_Run(data))
                {
                    state = 2;
                }
                break;
            case 2:
                SaveStatusScreen_FreeWindow(data);
                SaveStatusScreen_FreeBgs(data);
                GameSystem.SetVBlankCallback(null, null);
                finished = 1;
                break;
        }
        return finished;
    }

    // OFFSET 0x0209A3A4
    public static int SaveStatusScreen_Exit(ApplicationManager appMan, ref int state)
    {
        SaveStatusScreenData data = (SaveStatusScreenData)ApplicationManager.ApplicationManager_Data(appMan)!;
        int heapID = data.HeapID;
        ApplicationManager.ApplicationManager_FreeData(appMan);
        Heap.Heap_Destroy(heapID);
        Main.EnqueueApplication(MainMenuOverlay, ApplicationTemplate.At(MainMenuTemplate));
        return 1;
    }

    // OFFSET 0x0209A3D0
    private static void SaveStatusScreen_InitBgs(SaveStatusScreenData data)
    {
        GXLayers.GXLayers_SetBanks(RomStructs.Arm9Banks(BanksAddress));
        data.BgConfig = Bg.BgConfig_New(data.HeapID);
        Bg.SetAllGraphicsModes(RomStructs.Arm9GraphicsModes(GraphicsModesAddress));
        Bg.Bg_InitFromTemplate(data.BgConfig, 0, RomStructs.Arm9BgTemplate(BgTemplateAddress), 0);
        Bg.Bg_ClearTilemap(data.BgConfig, 0);
        RenderWindow.LoadMessageBoxGraphics(data.BgConfig, 0, MessageBoxTile, 2, 0, data.HeapID);
        RenderWindow.LoadStandardWindowGraphics(data.BgConfig, 0, WindowFrameTile, 3, 0, data.HeapID);
        Font.Font_LoadTextPalette(0, 0x20, data.HeapID);
        Bg.Bg_ClearTilesRange(0, 0x20, 0, data.HeapID);
        Bg.Bg_MaskPalette(0, 0);
        Bg.Bg_MaskPalette(4, 0);
    }

    // OFFSET 0x0209A490
    private static void SaveStatusScreen_FreeBgs(SaveStatusScreenData data)
    {
        Bg.Bg_ToggleLayer(0, 0);
        Bg.Bg_ToggleLayer(1, 0);
        Bg.Bg_ToggleLayer(2, 0);
        Bg.Bg_ToggleLayer(3, 0);
        Bg.Bg_ToggleLayer(4, 0);
        Bg.Bg_ToggleLayer(5, 0);
        Bg.Bg_ToggleLayer(6, 0);
        Bg.Bg_ToggleLayer(7, 0);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 0);
        data.BgConfig = null;
    }

    // OFFSET 0x0209A4E4
    private static void SaveStatusScreen_InitWindow(SaveStatusScreenData data)
    {
        data.Loader = MessageLoader.MessageLoader_Init(1, MessageArchive, MessageBank, data.HeapID);
        Text.Text_ResetAllPrinters();
        data.TextState = 0;
        Bg.Window_AddFromTemplate(data.BgConfig!, data.Window, RomStructs.Arm9WindowTemplate(WindowTemplateAddress));
        Bg.Window_FillRectWithColor(data.Window, 0xF, 0, 0, 0xD8, 0x20);
    }

    // OFFSET 0x0209A530
    private static void SaveStatusScreen_FreeWindow(SaveStatusScreenData data)
    {
        Bg.Window_Remove(data.Window);
        MessageLoader.MessageLoader_Free(data.Loader);
    }

    // OFFSET 0x0209A544
    private static bool SaveStatusScreen_Run(SaveStatusScreenData data)
    {
        bool finished = false;
        switch (data.State)
        {
            case 0:
                data.Status = SaveData.SaveData_LoadCheckStatus(data.SaveData!);
                data.State = data.Status == 0 ? 6 : 1;
                break;
            case 1:
                data.State = 2;
                if ((data.Status & 2) != 0)
                {
                    data.Status &= ~3u;
                    data.MessageID = 1;
                }
                else if ((data.Status & 1) != 0)
                {
                    data.Status ^= 1;
                    data.MessageID = 0;
                }
                else if ((data.Status & 8) != 0)
                {
                    data.Status &= ~0xCu;
                    data.MessageID = 5;
                }
                else if ((data.Status & 4) != 0)
                {
                    data.Status ^= 4;
                    data.MessageID = 4;
                }
                else if ((data.Status & 0x20) != 0)
                {
                    data.Status &= ~0x30u;
                    data.MessageID = 3;
                }
                else if ((data.Status & 0x10) != 0)
                {
                    data.Status ^= 0x10;
                    data.MessageID = 2;
                }
                else
                {
                    data.State = 6;
                }
                break;
            case 2:
                Bg.Bg_MaskPalette(0, BackgroundColor);
                Bg.Bg_MaskPalette(4, BackgroundColor);
                ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 6, 1, data.HeapID);
                data.State = 3;
                break;
            case 3:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    data.State = 4;
                }
                break;
            case 4:
                if (SaveStatusScreen_PrintMessage(data, data.MessageID, 0, TextDelay))
                {
                    ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 6, 1, data.HeapID);
                    data.State = 5;
                }
                break;
            case 5:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    Bg.Bg_MaskPalette(0, 0);
                    Bg.Bg_MaskPalette(4, 0);
                    data.State = 1;
                }
                break;
            case 6:
                finished = true;
                break;
        }
        return finished;
    }

    // OFFSET 0x0209A688
    private static bool SaveStatusScreen_PrintMessage(SaveStatusScreenData data, int messageID, int skipWait, int delay)
    {
        bool finished = false;
        switch (data.TextState)
        {
            case 0:
                Bg.Window_FillRectWithColor(data.Window, 0xF, 0, 0, 0xD8, 0x20);
                RenderWindow.Window_DrawMessageBoxWithScrollCursor(data.Window, 0, (ushort)MessageBoxTile, 2);
                data.Text = GameString.String_Init(0x400, data.HeapID);
                MessageLoader.MessageLoader_GetString(data.Loader!, (uint)messageID, data.Text);
                data.PrinterID = Text.Text_AddPrinterWithParams(data.Window, 1, data.Text, 0, 0, delay, null);
                if (delay == 0)
                {
                    GameString.String_Free(data.Text);
                    data.TextState++;
                }
                data.TextState++;
                break;
            case 1:
                if (Text.Text_IsPrinterActive((byte)data.PrinterID) == 0)
                {
                    GameString.String_Free(data.Text);
                    data.TextState++;
                }
                break;
            case 2:
                if (skipWait != 0 || (gSystem.PressedKeys & KeyA) != 0)
                {
                    data.TextState = 0;
                    finished = true;
                }
                break;
        }
        return finished;
    }
}
