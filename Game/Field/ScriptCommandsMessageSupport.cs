namespace PlatinumSharp.Game;

public static class FieldMessage
{
    private const int HeapIDField = 4;
    private const int TextPaletteSlotOffset = 0x1A0;
    private const int ScreenIndicatorsPaletteSlotOffset = 0x180;
    private const int BgLayerMain3 = 3;
    private const int BgLayerSub3 = 7;
    private const int WindowLeft = 2;
    private const int WindowTop = 0x13;
    private const int WindowWidth = 0x1B;
    private const int WindowHeight = 4;
    private const int WindowPalette = 0xC;
    private const int WindowBaseTileMain = 0x237;
    private const int WindowBaseTileSub = 0x194;
    private const int MessageBoxTile = 0x3E2;
    private const int MessageBoxPalette = 0xA;
    private const int FillValue = 0xF;
    private const int FontMessage = 1;
    private const int SignpostNarrowLeft = 9;
    private const int SignpostNarrowWidth = 0x14;
    private const int SignpostPalette = 9;
    private const int SignpostTile = 0x2A3;

    // OFFSET 0x0205D8CC
    public static void FieldMessage_LoadTextPalettes(int paletteLocation, int resetPrinters)
    {
        if (resetPrinters == 1)
        {
            Text.Text_ResetAllPrinters();
        }
        Font.Font_LoadTextPalette(paletteLocation, TextPaletteSlotOffset, HeapIDField);
        Font.Font_LoadScreenIndicatorsPalette(paletteLocation, ScreenIndicatorsPaletteSlotOffset, HeapIDField);
    }

    // OFFSET 0x0205D8F4
    public static void FieldMessage_AddWindow(object bgConfig, Window window, int bgLayer)
    {
        if (bgLayer == BgLayerMain3)
        {
            Bg.Window_Add(bgConfig, window, BgLayerMain3, WindowLeft, WindowTop, WindowWidth, WindowHeight, WindowPalette, WindowBaseTileMain);
        }
        else
        {
            Bg.Window_Add(bgConfig, window, BgLayerSub3, WindowLeft, WindowTop, WindowWidth, WindowHeight, WindowPalette, WindowBaseTileSub);
        }
    }

    // OFFSET 0x0205D944
    public static void FieldMessage_DrawWindow(Window window, Options options)
    {
        int bgLayer = Bg.Window_GetBgLayer(window);
        int frame = Options.Options_Frame(options);
        RenderWindow.LoadMessageBoxGraphics(window.BgConfig, (uint)bgLayer, MessageBoxTile, MessageBoxPalette, (byte)frame, HeapIDField);
        FieldMessage_ClearWindow(window);
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(window, 0, MessageBoxTile, MessageBoxPalette);
    }

    // OFFSET 0x0205D988
    public static void FieldMessage_ClearWindow(Window window)
    {
        Bg.Window_FillTilemap(window, FillValue);
    }

    // OFFSET 0x0205D994
    public static byte FieldMessage_Print(Window window, GameString text, Options options, int canSpeedUp)
    {
        RenderText.RenderControlFlags_SetCanABSpeedUpPrint(canSpeedUp);
        RenderText.RenderControlFlags_SetAutoScrollFlags(0);
        RenderText.RenderControlFlags_SetSpeedUpOnTouch(0);
        int renderDelay = Options.Options_TextFrameDelay(options);
        return (byte)Text.Text_AddPrinterWithParams(window, FontMessage, text, 0, 0, renderDelay, null);
    }

    // OFFSET 0x0205D9CC
    public static byte FieldMessage_PrintWithParams(Window window, GameString text, int fontID, int renderDelay, byte canSpeedUp, int autoScroll)
    {
        RenderText.RenderControlFlags_SetCanABSpeedUpPrint(canSpeedUp);
        RenderText.RenderControlFlags_SetAutoScrollFlags(autoScroll);
        RenderText.RenderControlFlags_SetSpeedUpOnTouch(0);
        return (byte)Text.Text_AddPrinterWithParams(window, fontID, text, 0, 0, renderDelay, null);
    }

    // OFFSET 0x0205DA04
    public static byte FieldMessage_FinishedPrinting(byte printerID)
    {
        if (Text.Text_IsPrinterActive(printerID) == 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DA1C
    public static void FieldMessage_AddSignpostWindow(object bgConfig, Window window, uint signpostType, int bgLayer)
    {
        int left;
        int width;
        if (signpostType <= 1)
        {
            left = SignpostNarrowLeft;
            width = SignpostNarrowWidth;
        }
        else
        {
            left = WindowLeft;
            width = WindowWidth;
        }
        if (bgLayer == BgLayerMain3)
        {
            Bg.Window_Add(bgConfig, window, BgLayerMain3, (byte)left, WindowTop, (byte)width, WindowHeight, SignpostPalette, WindowBaseTileMain);
        }
        else
        {
            Bg.Window_Add(bgConfig, window, BgLayerSub3, (byte)left, WindowTop, (byte)width, WindowHeight, SignpostPalette, WindowBaseTileSub);
        }
    }

    // OFFSET 0x0205DA80
    public static void FieldMessage_LoadAndDrawSignpost(Window window, int signpostType, ushort contentID)
    {
        int bgLayer = Bg.Window_GetBgLayer(window);
        RenderWindow.LoadSignpostContentGraphics(window.BgConfig, (uint)bgLayer, SignpostTile, SignpostPalette, (byte)signpostType, contentID, HeapIDField);
        Bg.Window_FillTilemap(window, FillValue);
        RenderWindow.Window_DrawSignpost(window, 0, SignpostTile, SignpostPalette, (byte)signpostType);
    }
}

public sealed class ScriptMessageOptions
{
    public byte RenderDelay;
    public byte AutoScroll;
    public byte FontID;
}

public static class ScriptMessage
{
    private const int HeapIDFieldTaskTemporary = 0x20;
    private const int BgLayerMain3 = 3;
    private const int FillValue = 0xF;
    private const int FontMessage = 1;
    private const int InstantMarker = 0xFF;

    private sealed class State
    {
        public GameString MessageBuffer = null!;
        public GameString TemporaryBuffer = null!;
        public StringTemplate Template = null!;
        public Window Window = null!;
        public ScriptManager Manager = null!;
    }

    // OFFSET 0x021DD42C
    public static void ScriptMessageOptions_Init(ScriptMessageOptions options, ScriptContext ctx)
    {
        options.RenderDelay = (byte)ScriptMessage_GetTextFrameDelay(ctx);
        options.AutoScroll = 0;
        options.FontID = FontMessage;
    }

    // OFFSET 0x021DD444
    public static void ScriptMessage_Show(ScriptContext ctx, MessageLoader loader, ushort messageID, byte canSpeedUp, ScriptMessageOptions? options)
    {
        State state = new();
        ScriptMessage_InitState(ctx.FieldSystem, state);
        ScriptMessage_OpenWindow(ctx.FieldSystem, state);
        ScriptMessage_LoadString(state, loader, messageID);
        int renderDelay;
        int autoScroll;
        int fontID;
        if (options == null)
        {
            renderDelay = ScriptMessage_GetTextFrameDelay(ctx);
            autoScroll = 0;
            fontID = FontMessage;
        }
        else
        {
            renderDelay = options.RenderDelay;
            autoScroll = options.AutoScroll;
            fontID = options.FontID;
        }
        ScriptMessage_Print(state, fontID, renderDelay, canSpeedUp, autoScroll);
    }

    // OFFSET 0x021DD498
    public static void ScriptMessage_ShowInstant(ScriptContext ctx, MessageLoader loader, ushort messageID)
    {
        State state = new();
        ScriptMessage_InitState(ctx.FieldSystem, state);
        ScriptMessage_OpenWindow(ctx.FieldSystem, state);
        ScriptMessage_LoadString(state, loader, messageID);
        ScriptMessage_PrintInstant(state, FontMessage);
    }

    // OFFSET 0x021DD4CC
    public static void ScriptMessage_ShowSentence(ScriptContext ctx, ushort sentenceType, ushort sentenceID, ushort word1, short word2, byte canSpeedUp)
    {
        State state = new();
        ScriptMessage_InitState(ctx.FieldSystem, state);
        ScriptMessage_OpenWindow(ctx.FieldSystem, state);
        ScriptMessage_LoadSentence(state, sentenceType, sentenceID, word1, (ushort)word2);
        if (canSpeedUp != InstantMarker)
        {
            ScriptMessage_Print(state, FontMessage, ScriptMessage_GetTextFrameDelay(ctx), canSpeedUp, 0);
        }
        else
        {
            ScriptMessage_PrintInstant(state, FontMessage);
        }
    }

    // OFFSET 0x021DD530
    public static void ScriptMessage_ShowWithTemplate(ScriptContext ctx, StringTemplate template, byte messageID, byte canSpeedUp)
    {
        State state = new();
        ScriptMessage_InitStateWithTemplate(ctx.FieldSystem, template, state);
        ScriptMessage_OpenWindow(ctx.FieldSystem, state);
        ScriptMessage_LoadString(state, ctx.Loader!, messageID);
        ScriptMessage_Print(state, FontMessage, ScriptMessage_GetTextFrameDelay(ctx), canSpeedUp, 0);
    }

    // OFFSET 0x021DD574
    private static int ScriptMessage_GetTextFrameDelay(ScriptContext ctx)
    {
        return Options.Options_TextFrameDelay(PlayerSave.SaveData_GetOptions(ctx.FieldSystem.SaveData!));
    }

    // OFFSET 0x021DD588
    private static void ScriptMessage_InitState(FieldSystem fieldSystem, State state)
    {
        ScriptManager scriptManager = ScriptManager.FieldSystem_GetScriptManager(fieldSystem);
        state.MessageBuffer = scriptManager.MessageBuffer!;
        state.TemporaryBuffer = scriptManager.TemporaryBuffer!;
        state.Template = scriptManager.StringTemplate!;
        state.Window = scriptManager.Window;
        state.Manager = scriptManager;
    }

    // OFFSET 0x021DD5D0
    private static void ScriptMessage_InitStateWithTemplate(FieldSystem fieldSystem, StringTemplate template, State state)
    {
        ScriptManager scriptManager = ScriptManager.FieldSystem_GetScriptManager(fieldSystem);
        state.MessageBuffer = scriptManager.MessageBuffer!;
        state.TemporaryBuffer = scriptManager.TemporaryBuffer!;
        state.Template = template;
        state.Window = scriptManager.Window;
        state.Manager = scriptManager;
    }

    // OFFSET 0x021DD610
    private static void ScriptMessage_OpenWindow(FieldSystem fieldSystem, State state)
    {
        if (state.Manager.IsMessageBoxOpen == 0)
        {
            FieldMessage.FieldMessage_AddWindow(fieldSystem.BgConfig!, state.Window, BgLayerMain3);
            FieldMessage.FieldMessage_DrawWindow(state.Window, PlayerSave.SaveData_GetOptions(fieldSystem.SaveData!));
            state.Manager.IsMessageBoxOpen = 1;
        }
        Bg.Window_FillTilemap(state.Window, FillValue);
    }

    // OFFSET 0x021DD648
    private static void ScriptMessage_LoadString(State state, MessageLoader loader, uint messageID)
    {
        MessageLoader.MessageLoader_GetString(loader, messageID, state.TemporaryBuffer);
        StringTemplate.StringTemplate_Format(state.Template, state.MessageBuffer, state.TemporaryBuffer);
    }

    // OFFSET 0x021DD664
    private static void ScriptMessage_LoadSentence(State state, ushort sentenceType, ushort sentenceID, ushort word1, ushort word2)
    {
        EasyChatSentence sentence = new();
        EasyChatSentence.EasyChatSentence_Init(sentence);
        EasyChatSentence.EasyChatSentence_SetTypeAndID(sentence, sentenceType, sentenceID);
        EasyChatSentence.EasyChatSentence_SetWord(sentence, 0, word1);
        EasyChatSentence.EasyChatSentence_SetWord(sentence, 1, word2);
        GameString? text = EasyChatSentence.EasyChatSentence_ToString(sentence, HeapIDFieldTaskTemporary);
        GameString.String_Copy(state.MessageBuffer, text);
        GameString.String_Free(text);
    }

    // OFFSET 0x021DD6B4
    private static void ScriptMessage_Print(State state, int fontID, int renderDelay, byte canSpeedUp, int autoScroll)
    {
        state.Manager.MessageID = FieldMessage.FieldMessage_PrintWithParams(state.Window, state.MessageBuffer, fontID, renderDelay, canSpeedUp, autoScroll);
    }

    // OFFSET 0x021DD6DC
    private static void ScriptMessage_PrintInstant(State state, int fontID)
    {
        state.Manager.MessageID = (byte)Text.Text_AddPrinterWithParams(state.Window, fontID, state.MessageBuffer, 0, 0, 0, null);
    }
}
