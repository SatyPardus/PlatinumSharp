namespace PlatinumSharp.Game;

public sealed class MapNamePopUp
{
    public const int IsInitedOffset = 0x00;
    public const int StateOffset = 0x04;
    public const int TaskOffset = 0x08;
    public const int YOffsetOffset = 0x0C;
    public const int TimerOffset = 0x10;
    public const int ShouldSlideInOffset = 0x14;
    public const int EntryIDOffset = 0x18;
    public const int BgConfigOffset = 0x1C;
    public const int WindowOffset = 0x20;
    public const int XOffsetOffset = 0x30;
    public const int WindowIDOffset = 0x31;
    public const int CharDataOffset = 0x34;
    public const int TilesOffset = 0x38;
    public const int MsgLoaderOffset = 0x3C;
    public const int StringOffset = 0x40;
    public const int Size = 0x44;

    public int IsInited;
    public byte State;
    public SysTask? Task;
    public int YOffset;
    public ushort Timer;
    public int ShouldSlideIn;
    public int EntryID;
    public BgConfig? BgConfig;
    public readonly Window Window = new();
    public byte XOffset;
    public byte WindowID;
    public CharacterData? CharData;
    public byte[]? Tiles;
    public MessageLoader? MsgLoader;
    public GameString? String;
}

public static class MapNamePopUps
{
    private const int HeapIDFieldMap = 4;
    private const int StateEnd = 0;
    private const int StateSlideIn = 1;
    private const int StateSlideOut = 2;
    private const int StateWait = 3;
    private const int BgLayerMain3 = 3;
    private const byte BgOffsetSetY = 3;
    private const int HiddenYOffset = 0x26;
    private const int SlideSpeed = 4;
    private const int WaitFrames = 60;
    private const int PopUpWidthTiles = 17;
    private const int PopUpHeightTiles = 5;
    private const int WindowWidthTiles = 32;
    private const int WindowPalette = 7;
    private const int WindowBaseTile = 0x197;
    private const int AreaGfxBaseTile = 0x2D9;
    private const int NarcAreaWindowGraphics = 0xA0;
    private const int NarcMessages = 0x1A;
    private const int TextBankLocationNames = 0x1B1;
    private const int MessageLoaderLoadOnDemand = 1;
    private const int StringSize = 22;
    private const int FontSystem = 0;
    private const int TextY = 16;
    private const uint TextColor = 0x00030200;
    private const int PaletteSlotBytes = 0x20;
    private const int LocationMapHeaderID = 0x00;

    // OFFSET 0x021DD6FC
    private static void MapNamePopUp_LoadPalette(ushort[] source, ushort size, ushort offset)
    {
        Video.LoadPaletteRange(Video.MainBackground, source, 0, (uint)(offset * PaletteSlotBytes), (uint)(size * PaletteSlotBytes));
    }

    // OFFSET 0x021DD718
    private static void MapNamePopUp_CreateWindow(MapNamePopUp mapPopUp)
    {
        Bg.Window_Add(mapPopUp.BgConfig!, mapPopUp.Window, BgLayerMain3, 0, 0, WindowWidthTiles, PopUpHeightTiles, WindowPalette, WindowBaseTile);
    }

    // OFFSET 0x021DD744
    private static void MapNamePopUp_LoadAreaGfx(MapNamePopUp mapPopUp, byte bgLayer, ushort tileStart, byte paletteOffset, byte unused)
    {
        byte narcMemberIdx = (byte)(mapPopUp.WindowID * 2);
        mapPopUp.Tiles = Graphics.Graphics_GetCharData(NarcAreaWindowGraphics, narcMemberIdx, 0, out mapPopUp.CharData, HeapIDFieldMap);
        Bg.Bg_LoadTiles(mapPopUp.BgConfig!, bgLayer, mapPopUp.CharData!.RawData, mapPopUp.CharData.SizeBytes, tileStart);
        Graphics.Graphics_GetPlttData(NarcAreaWindowGraphics, (uint)(narcMemberIdx + 1), out PaletteFile? paletteData, HeapIDFieldMap);
        MapNamePopUp_LoadPalette(paletteData!.Colors, 1, paletteOffset);
    }

    // OFFSET 0x021DD7A0
    private static void MapNamePopUp_DrawWindowFrame(MapNamePopUp mapPopUp, int stringWidth)
    {
        int leftMargin = ((stringWidth + 8) / 8 * 8 - stringWidth) / 2 + 4;
        byte extraTiles;
        if (leftMargin >= 8)
        {
            extraTiles = 0;
        }
        else
        {
            extraTiles = (byte)(((8 - leftMargin) * 2 + 7) / 8);
        }
        int xOffset;
        if (stringWidth <= 0)
        {
            xOffset = 0;
        }
        else
        {
            xOffset = (((stringWidth + 8) / 8 + extraTiles) * 8 + 8 - stringWidth) / 2;
        }
        mapPopUp.XOffset = (byte)(xOffset + 4);
        MapNamePopUp_LoadAreaGfx(mapPopUp, BgLayerMain3, AreaGfxBaseTile, WindowPalette, 0);
        Bg.Window_FillTilemap(mapPopUp.Window, 0);
        for (int i = 0; i < PopUpWidthTiles * PopUpHeightTiles; i++)
        {
            int column = i % PopUpWidthTiles;
            int row = i / PopUpWidthTiles;
            Bg.Window_BlitBitmapRect(mapPopUp.Window, mapPopUp.CharData!.RawData, (ushort)(i * 8), 0, 8, 8, (ushort)(column * 8), (ushort)(row * 8), 8, 8);
        }
        Bg.Window_CopyToVRAM(mapPopUp.Window);
        mapPopUp.Tiles = null;
    }

    // OFFSET 0x021DD880
    private static void MapNamePopUp_Reset(MapNamePopUp mapPopUp)
    {
        mapPopUp.IsInited = 0;
        mapPopUp.State = StateEnd;
        mapPopUp.Task = null;
        mapPopUp.YOffset = 0;
        mapPopUp.Timer = 0;
        mapPopUp.ShouldSlideIn = 0;
        mapPopUp.EntryID = 0;
        mapPopUp.BgConfig = null;
    }

    // OFFSET 0x021DD894
    private static void MapNamePopUp_SetBgConfig(MapNamePopUp mapPopUp, BgConfig? bgConfig)
    {
        MapNamePopUp_Reset(mapPopUp);
        mapPopUp.BgConfig = bgConfig;
    }

    // OFFSET 0x021DD8A4
    private static void MapNamePopUp_Task(SysTask task, object? param)
    {
        MapNamePopUp mapPopUp = (MapNamePopUp)param!;
        switch (mapPopUp.State)
        {
            case StateEnd:
                break;
            case StateSlideIn:
                mapPopUp.YOffset -= SlideSpeed;
                if (mapPopUp.YOffset < 0)
                {
                    mapPopUp.YOffset = 0;
                }
                Bg.Bg_SetOffset(mapPopUp.BgConfig!, BgLayerMain3, BgOffsetSetY, mapPopUp.YOffset);
                if (mapPopUp.YOffset == 0)
                {
                    mapPopUp.Timer = 0;
                    mapPopUp.State = StateWait;
                }
                break;
            case StateWait:
                mapPopUp.Timer++;
                if (mapPopUp.Timer >= WaitFrames)
                {
                    mapPopUp.Timer = 0;
                    mapPopUp.State = StateSlideOut;
                }
                break;
            case StateSlideOut:
                mapPopUp.YOffset += SlideSpeed;
                if (mapPopUp.YOffset > HiddenYOffset)
                {
                    mapPopUp.YOffset = HiddenYOffset;
                }
                Bg.Bg_SetOffset(mapPopUp.BgConfig!, BgLayerMain3, BgOffsetSetY, mapPopUp.YOffset);
                if (mapPopUp.YOffset == HiddenYOffset)
                {
                    if (mapPopUp.ShouldSlideIn != 0)
                    {
                        mapPopUp.ShouldSlideIn = 0;
                        int stringWidth = LocationName.LocationName_LoadAndGetWidth(mapPopUp.MsgLoader!, mapPopUp.EntryID, mapPopUp.String!);
                        MapNamePopUp_DrawWindowFrame(mapPopUp, stringWidth);
                        MapNamePopUp_PrintMapName(mapPopUp, mapPopUp.String!);
                        mapPopUp.State = StateSlideIn;
                    }
                    else
                    {
                        MapNamePopUp_Hide(mapPopUp);
                    }
                }
                break;
        }
    }

    // OFFSET 0x021DD954
    private static void MapNamePopUp_PrintMapName(MapNamePopUp mapPopUp, GameString text)
    {
        Text.Text_AddPrinterWithParamsAndColor(mapPopUp.Window, FontSystem, text, mapPopUp.XOffset, TextY, 0, TextColor, null);
    }

    // OFFSET 0x021DD980
    private static void MapNamePopUp_StartSlideOut(MapNamePopUp mapPopUp)
    {
        mapPopUp.State = StateSlideOut;
        mapPopUp.Timer = 0;
    }

    // OFFSET 0x021DD98C
    public static MapNamePopUp MapNamePopUp_Create(BgConfig? bgConfig)
    {
        MapNamePopUp mapPopUp = new();
        mapPopUp.String = GameString.String_Init(StringSize, HeapIDFieldMap);
        MapNamePopUp_SetBgConfig(mapPopUp, bgConfig);
        MapNamePopUp_CreateWindow(mapPopUp);
        mapPopUp.MsgLoader = MessageLoader.MessageLoader_Init(MessageLoaderLoadOnDemand, NarcMessages, TextBankLocationNames, HeapIDFieldMap);
        return mapPopUp;
    }

    // OFFSET 0x021DD9C8
    public static void MapNamePopUp_Destroy(object? mapPopUpObject)
    {
        MapNamePopUp mapPopUp = (MapNamePopUp)mapPopUpObject!;
        MessageLoader.MessageLoader_Free(mapPopUp.MsgLoader);
        Bg.Window_Remove(mapPopUp.Window);
        GameString.String_Free(mapPopUp.String);
    }

    // OFFSET 0x021DD9E8
    public static void MapNamePopUp_Show(object? mapPopUpObject, int mapLabelTextID, int mapLabelWindowID)
    {
        MapNamePopUp mapPopUp = (MapNamePopUp)mapPopUpObject!;
        mapPopUp.EntryID = mapLabelTextID;
        if (mapPopUp.IsInited == 0)
        {
            mapPopUp.IsInited = 1;
            Bg.Bg_SetOffset(mapPopUp.BgConfig!, BgLayerMain3, BgOffsetSetY, HiddenYOffset);
            mapPopUp.YOffset = HiddenYOffset;
            mapPopUp.Task = SysTask.SysTask_Start(MapNamePopUp_Task, mapPopUp, 0);
            mapPopUp.State = StateSlideIn;
            int stringWidth = LocationName.LocationName_LoadAndGetWidth(mapPopUp.MsgLoader!, mapPopUp.EntryID, mapPopUp.String!);
            mapPopUp.WindowID = (byte)mapLabelWindowID;
            MapNamePopUp_DrawWindowFrame(mapPopUp, stringWidth);
            MapNamePopUp_PrintMapName(mapPopUp, mapPopUp.String!);
            return;
        }
        switch (mapPopUp.State)
        {
            case StateSlideIn:
            case StateWait:
                MapNamePopUp_StartSlideOut(mapPopUp);
                mapPopUp.ShouldSlideIn = 1;
                mapPopUp.WindowID = (byte)mapLabelWindowID;
                break;
            case StateSlideOut:
                mapPopUp.ShouldSlideIn = 1;
                mapPopUp.WindowID = (byte)mapLabelWindowID;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
    }

    // OFFSET 0x021DDA78
    public static void MapNamePopUp_Hide(object? mapPopUpObject)
    {
        MapNamePopUp mapPopUp = (MapNamePopUp)mapPopUpObject!;
        if (mapPopUp.Task != null)
        {
            SysTask.SysTask_Done(mapPopUp.Task);
        }
        Bg.Window_ClearAndCopyToVRAM(mapPopUp.Window);
        Bg.Bg_SetOffset(mapPopUp.BgConfig!, BgLayerMain3, BgOffsetSetY, 0);
        MapNamePopUp_SetBgConfig(mapPopUp, mapPopUp.BgConfig);
    }

    // OFFSET 0x021DDAA4
    public static void FieldSystem_RequestLocationName(FieldSystem fieldSystem)
    {
        int mapHeaderID = (int)fieldSystem.Location!.U32(LocationMapHeaderID);
        if (MapHeader.MapHeader_GetMapLabelWindowID(mapHeaderID) == 0)
        {
            return;
        }
        if (MapHeader.MapHeader_IsBuilding(mapHeaderID))
        {
            return;
        }
        int mapLabelTextID = MapHeader.MapHeader_GetMapLabelTextID(mapHeaderID);
        int mapLabelWindowID = MapHeader.MapHeader_GetMapLabelWindowID(mapHeaderID);
        if (mapLabelWindowID != 0)
        {
            mapLabelWindowID--;
        }
        MapNamePopUp_Show(fieldSystem.FieldMapContext!.MapNamePopUp, mapLabelTextID, mapLabelWindowID);
    }
}

public static class LocationName
{
    private const int FontSystem = 0;

    // OFFSET 0x02071CFC
    public static int LocationName_LoadAndGetWidth(MessageLoader loader, int entryID, GameString dst)
    {
        MessageLoader.MessageLoader_GetString(loader, (uint)entryID, dst);
        return Font.Font_CalcStringWidth(FontSystem, dst, 0);
    }
}
