namespace PlatinumSharp.Game;

public sealed class SaveInfo
{
    public int NumSeen;
    public int LocationTextID;
    public TrainerInfo? TrainerInfo;
    public PlayTime? PlayTime;
}

public sealed class SaveInfoWindow
{
    public FieldSystem? FieldSystem;
    public int HeapID;
    public byte BgLayer;
    public BgConfig? BgConfig;
    public Window? Window;
    public StringTemplate? StringTemplate;
    public MessageLoader? MessageLoader;
    public readonly SaveInfo SaveInfo = new();
    public int Width;
    public int Height;
}

public static class SaveInfoWindows
{
    private const int TextBankSaveInfo = 0x216;
    private const ushort BaseTileStandardWindowFrame = 0x3D9;
    private const int StandardWindowPalette = 0xB;
    private const byte WindowPalette = 0xD;
    private const ushort WindowBaseTile = 0x189;
    private const int WindowWidth = 0xD;
    private const int ValueRightEdge = 0x68;
    private const int RowPokedex = 3;
    private const uint ValueMessageIDs = 0x021F9CBC;
    private const uint LabelMessageIDs = 0x021F9CD0;
    private const int FieldOverlayID = 5;
    private const int FontSystem = 0;

    // OFFSET 0x021E1D20
    private static void SaveInfo_SetValues(SaveInfo saveInfo, FieldSystem fieldSystem)
    {
        SaveData saveData = fieldSystem.SaveData!;
        BufferView location = FieldPlayerState.FieldPlayerState_GetPlayerLocation(FieldPlayerState.SaveData_GetFieldPlayerState(saveData));
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(saveData);
        saveInfo.LocationTextID = MapHeader.MapHeader_GetMapLabelTextID((int)location.U32(0));
        if (Pokedex.Pokedex_IsObtained(pokedex))
        {
            saveInfo.NumSeen = Pokedex.Pokedex_CountSeen(pokedex);
        }
        else
        {
            saveInfo.NumSeen = 0;
        }
        saveInfo.TrainerInfo = PlayerSave.SaveData_GetTrainerInfo(saveData);
        saveInfo.PlayTime = PlayerSave.SaveData_GetPlayTime(saveData);
    }

    private static void SetScaledNumber(StringTemplate template, uint idx, int value)
    {
        if (value >= 100)
        {
            StringTemplate.StringTemplate_SetNumber(template, idx, value, 3, 0, 1);
        }
        else if (value >= 10)
        {
            StringTemplate.StringTemplate_SetNumber(template, idx, value, 3, 1, 1);
        }
        else
        {
            StringTemplate.StringTemplate_SetNumber(template, idx, value, 2, 1, 1);
        }
    }

    // OFFSET 0x021E1D6C
    private static void SaveInfoWindow_SetStrings(StringTemplate template, SaveInfo saveInfo)
    {
        StringTemplate.StringTemplate_SetLocationName(template, 0, (uint)saveInfo.LocationTextID);
        StringTemplate.StringTemplate_SetPlayerName(template, 1, saveInfo.TrainerInfo!);
        StringTemplate.StringTemplate_SetNumber(template, 2, TrainerInfo.TrainerInfo_BadgeCount(saveInfo.TrainerInfo!), 1, 0, 1);
        SetScaledNumber(template, 3, saveInfo.NumSeen);
        SetScaledNumber(template, 4, PlayTime.PlayTime_GetHours(saveInfo.PlayTime!));
        StringTemplate.StringTemplate_SetNumber(template, 5, PlayTime.PlayTime_GetMinutes(saveInfo.PlayTime!), 2, 2, 1);
    }

    // OFFSET 0x021E1E10
    private static int SaveInfoWindow_Height(SaveInfo saveInfo)
    {
        if (saveInfo.NumSeen != 0)
        {
            return 0xA;
        }
        return 8;
    }

    // OFFSET 0x021E1E20
    private static void SaveInfoWindow_PrintText(SaveInfoWindow saveInfoWin)
    {
        int lineHeight = Font.Font_GetAttribute(FontSystem, 1) + Font.Font_GetAttribute(FontSystem, 3);
        int y = 0;
        GameString text = MessageUtil.MessageUtil_ExpandedString(saveInfoWin.StringTemplate!, saveInfoWin.MessageLoader!, 0, saveInfoWin.HeapID)!;
        Text.Text_AddPrinterWithParams(saveInfoWin.Window!, FontSystem, text, 0, y, 0xFF, null);
        GameString.String_Free(text);
        for (int i = 1; i < 5; i++)
        {
            uint labelID = RomData.OverlayU32(FieldOverlayID, (uint)(LabelMessageIDs + (i - 1) * 4), 1)[0];
            if (labelID == RowPokedex && saveInfoWin.SaveInfo.NumSeen == 0)
            {
                continue;
            }
            y += lineHeight;
            text = MessageLoader.MessageLoader_GetNewString(saveInfoWin.MessageLoader!, labelID)!;
            Text.Text_AddPrinterWithParams(saveInfoWin.Window!, FontSystem, text, 0, y, 0xFF, null);
            GameString.String_Free(text);
            uint valueID = RomData.OverlayU32(FieldOverlayID, (uint)(ValueMessageIDs + (i - 1) * 4), 1)[0];
            text = MessageUtil.MessageUtil_ExpandedString(saveInfoWin.StringTemplate!, saveInfoWin.MessageLoader!, valueID, saveInfoWin.HeapID)!;
            int x = ValueRightEdge - Font.Font_CalcStringWidth(FontSystem, text, Font.Font_GetAttribute(FontSystem, 2));
            Text.Text_AddPrinterWithParams(saveInfoWin.Window!, FontSystem, text, x, y, 0xFF, null);
            GameString.String_Free(text);
        }
    }

    // OFFSET 0x021E1F04
    public static void SaveInfoWindow_Draw(SaveInfoWindow saveInfoWin)
    {
        saveInfoWin.Window = new Window();
        Bg.Window_Add(saveInfoWin.BgConfig!, saveInfoWin.Window, saveInfoWin.BgLayer, 1, 1, (byte)saveInfoWin.Width, (byte)saveInfoWin.Height, WindowPalette, WindowBaseTile);
        RenderWindow.LoadStandardWindowGraphics(saveInfoWin.BgConfig!, saveInfoWin.BgLayer, BaseTileStandardWindowFrame, StandardWindowPalette, 0, saveInfoWin.HeapID);
        Bg.Window_FillTilemap(saveInfoWin.Window, Font.Font_GetAttribute(FontSystem, 6));
        SaveInfoWindow_PrintText(saveInfoWin);
        RenderWindow.Window_DrawStandardFrame(saveInfoWin.Window, 0, BaseTileStandardWindowFrame, StandardWindowPalette);
    }

    // OFFSET 0x021E1F7C
    public static void SaveInfoWindow_Erase(SaveInfoWindow saveInfoWin)
    {
        RenderWindow.Window_EraseStandardFrame(saveInfoWin.Window!, 0);
        Bg.Window_Remove(saveInfoWin.Window!);
        saveInfoWin.Window = null;
    }

    // OFFSET 0x021E1F98
    public static SaveInfoWindow SaveInfoWindow_New(FieldSystem fieldSystem, int heapID, byte bgLayer)
    {
        SaveInfoWindow saveInfoWin = new();
        saveInfoWin.FieldSystem = fieldSystem;
        saveInfoWin.HeapID = heapID;
        saveInfoWin.BgLayer = bgLayer;
        saveInfoWin.BgConfig = fieldSystem.BgConfig;
        saveInfoWin.StringTemplate = StringTemplate.StringTemplate_Default(heapID);
        saveInfoWin.MessageLoader = MessageLoader.MessageLoader_Init(1, 0x1A, TextBankSaveInfo, heapID);
        SaveInfo_SetValues(saveInfoWin.SaveInfo, fieldSystem);
        SaveInfoWindow_SetStrings(saveInfoWin.StringTemplate, saveInfoWin.SaveInfo);
        saveInfoWin.Width = WindowWidth;
        saveInfoWin.Height = SaveInfoWindow_Height(saveInfoWin.SaveInfo);
        return saveInfoWin;
    }

    // OFFSET 0x021E1FF4
    public static void SaveInfoWindow_Free(SaveInfoWindow saveInfoWin)
    {
        MessageLoader.MessageLoader_Free(saveInfoWin.MessageLoader);
        StringTemplate.StringTemplate_Free(saveInfoWin.StringTemplate);
    }
}
