namespace PlatinumSharp.Game;

public static partial class TrainerCaseApplication
{
    private const uint WindowTemplates = 0x0223D670;
    private const uint LabelTextIDs = 0x0223D644;
    private const uint TextColor = 0x10200;
    private const int FrontLabelCount = 7;
    private const int BackLabelFirst = 7;
    private const int WindowID = 0;
    private const int WindowName = 1;
    private const int WindowMoney = 2;
    private const int WindowPokedex = 3;
    private const int WindowScore = 4;
    private const int WindowAdventureStarted = 6;
    private const int WindowHofDebut = 7;
    private const int WindowTimesLinked = 8;
    private const int WindowLinkBattles = 9;
    private const int WindowLinkTrades = 10;
    private const uint FrontWidth = 0x88;
    private const uint BackWidth = 0xE0;

    // OFFSET 0x0223C69C
    private static void TrainerCard_AddWindows(BgConfig bgConfig, Window[] windows)
    {
        for (int i = 0; i < TrainerCaseApp.NumWindows; i++)
        {
            WindowTemplate template = RomStructs.WindowTemplate(RomData.Overlay(Overlay, (uint)(WindowTemplates + i * 8), 8), 0);
            Bg.Window_AddFromTemplate(bgConfig, windows[i], template);
        }
        Bg.Bg_FillTilesRange(bgConfig, 7, 0, 1, 0);
    }

    // OFFSET 0x0223C6D4
    private static void TrainerCard_RemoveWindows(Window[] windows)
    {
        for (int i = 0; i < TrainerCaseApp.NumWindows; i++)
        {
            Bg.Window_Remove(windows[i]);
        }
    }

    private static uint LabelTextID(int index)
    {
        return RomData.OverlayU32(Overlay, (uint)(LabelTextIDs + index * 4), 1)[0];
    }

    private static void PrintRightAligned(Window window, GameString text, uint width, uint y)
    {
        int textWidth = Font.Font_CalcStringWidth(0, text, 0);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, text, (uint)(width - textWidth), y, 0, TextColor, null);
    }

    private static void FormatAndPrint(MessageLoader loader, StringTemplate template, GameString format, GameString text, uint entry, Window window, uint width, uint y)
    {
        MessageLoader.MessageLoader_GetString(loader, entry, format);
        StringTemplate.StringTemplate_Format(template, text, format);
        PrintRightAligned(window, text, width, y);
    }

    // OFFSET 0x0223C6F0
    private static void TrainerCard_DrawFrontText(Window[] windows, TrainerCase trainerCase)
    {
        for (int i = 0; i < 5; i++)
        {
            Bg.Window_FillRectWithColor(windows[i], 0, 0, 0, (int)FrontWidth, 0x10);
        }
        Bg.Window_FillRectWithColor(windows[5], 0, 0, 0, (int)BackWidth, 0x10);
        Bg.Window_FillRectWithColor(windows[6], 0, 0, 0, (int)BackWidth, 0x10);
        MessageLoader loader = MessageLoader.MessageLoader_Init(0, MessageArchive, TrainerCardTextBank, HeapID)!;
        GameString text = GameString.String_Init(0x20, HeapID);
        for (int i = 0; i < FrontLabelCount; i++)
        {
            if (i == WindowPokedex && CaseFlag(trainerCase, TrainerCase.FlagPokedexObtained) == 0)
            {
                continue;
            }
            MessageLoader.MessageLoader_GetString(loader, LabelTextID(i), text);
            Text.Text_AddPrinterWithParamsAndColor(windows[i], 0, text, 0, 0, 0, TextColor, null);
        }
        GameString format = GameString.String_Init(0x20, HeapID);
        StringTemplate template = StringTemplate.StringTemplate_New(6, 0x20, HeapID);
        TrainerCard_DrawNumber(windows[WindowID], FrontWidth, 0, 0, text, trainerCase.U16(TrainerCase.TrainerIDOffset), 5, 2);
        ushort[] name = new ushort[8];
        for (int i = 0; i < name.Length; i++)
        {
            name[i] = trainerCase.U16(TrainerCase.NameOffset + i * 2);
        }
        GameString.String_CopyChars(text, name);
        TrainerCard_DrawString(windows[WindowName], FrontWidth, 0, 0, text);
        StringTemplate.StringTemplate_SetNumber(template, 5, (int)trainerCase.U32(TrainerCase.MoneyOffset), 6, 0, 1);
        FormatAndPrint(loader, template, format, text, 0xE, windows[WindowMoney], FrontWidth, 0);
        if (CaseFlag(trainerCase, TrainerCase.FlagPokedexObtained) != 0)
        {
            StringTemplate.StringTemplate_SetNumber(template, 5, (int)trainerCase.U32(TrainerCase.SeenPokemonOffset), 3, 0, 1);
            FormatAndPrint(loader, template, format, text, 0x15, windows[WindowPokedex], FrontWidth, 0);
        }
        TrainerCard_DrawNumber(windows[WindowScore], FrontWidth, 0, 0, text, trainerCase.U32(TrainerCase.ScoreOffset), 9, 1);
        if (CaseFlag(trainerCase, TrainerCase.FlagLiveTimeDisplay) != 0)
        {
            StringTemplate.StringTemplate_SetNumber(template, 0, PlayTime.PlayTime_GetHours(trainerCase.PlayTime!), 3, 1, 1);
            StringTemplate.StringTemplate_SetNumber(template, 1, PlayTime.PlayTime_GetMinutes(trainerCase.PlayTime!), 2, 2, 1);
            MessageLoader.MessageLoader_GetString(loader, 0x10, format);
        }
        else
        {
            StringTemplate.StringTemplate_SetNumber(template, 0, trainerCase.U16(TrainerCase.PlayTimeHoursOffset), 3, 1, 1);
            StringTemplate.StringTemplate_SetNumber(template, 1, trainerCase.U8(TrainerCase.PlayTimeMinutesOffset), 2, 2, 1);
            MessageLoader.MessageLoader_GetString(loader, 0xF, format);
        }
        StringTemplate.StringTemplate_Format(template, text, format);
        PrintRightAligned(windows[WindowTime], text, BackWidth, 0);
        StringTemplate.StringTemplate_SetNumber(template, 2, trainerCase.U8(TrainerCase.AdventureStartedYearOffset), 2, 2, 1);
        StringTemplate.StringTemplate_SetMonthName(template, 3, trainerCase.U8(TrainerCase.AdventureStartedMonthOffset));
        StringTemplate.StringTemplate_SetNumber(template, 4, trainerCase.U8(TrainerCase.AdventureStartedDayOffset), 2, 2, 1);
        FormatAndPrint(loader, template, format, text, 0x11, windows[WindowAdventureStarted], BackWidth, 0);
        GameString.String_Free(format);
        StringTemplate.StringTemplate_Free(template);
        GameString.String_Free(text);
        MessageLoader.MessageLoader_Free(loader);
    }

    // OFFSET 0x0223CA28
    private static void TrainerCard_DrawBackText(Window[] windows, TrainerCase trainerCase)
    {
        Bg.Window_FillRectWithColor(windows[WindowHofDebut], 0, 0, 0, (int)BackWidth, 0x20);
        Bg.Window_FillRectWithColor(windows[WindowTimesLinked], 0, 0, 0, (int)BackWidth, 0x10);
        Bg.Window_FillRectWithColor(windows[WindowLinkBattles], 0, 0, 0, (int)BackWidth, 0x10);
        Bg.Window_FillRectWithColor(windows[WindowLinkTrades], 0, 0, 0, (int)BackWidth, 0x10);
        MessageLoader loader = MessageLoader.MessageLoader_Init(0, MessageArchive, TrainerCardTextBank, HeapID)!;
        GameString text = GameString.String_Init(0x20, HeapID);
        for (int i = BackLabelFirst; i < TrainerCaseApp.NumWindows; i++)
        {
            MessageLoader.MessageLoader_GetString(loader, LabelTextID(i), text);
            Text.Text_AddPrinterWithParamsAndColor(windows[i], 0, text, 0, 0, 0, TextColor, null);
        }
        GameString format = GameString.String_Init(0x20, HeapID);
        StringTemplate template = StringTemplate.StringTemplate_New(6, 0x20, HeapID);
        if (trainerCase.U8(TrainerCase.HofDebutMonthOffset) != 0)
        {
            StringTemplate.StringTemplate_SetNumber(template, 2, trainerCase.U8(TrainerCase.HofDebutYearOffset), 2, 2, 1);
            StringTemplate.StringTemplate_SetMonthName(template, 3, trainerCase.U8(TrainerCase.HofDebutMonthOffset));
            StringTemplate.StringTemplate_SetNumber(template, 4, trainerCase.U8(TrainerCase.HofDebutDayOffset), 2, 2, 1);
            StringTemplate.StringTemplate_SetNumber(template, 0, trainerCase.U16(TrainerCase.HofDebutHourOffset), 3, 1, 1);
            StringTemplate.StringTemplate_SetNumber(template, 1, trainerCase.U8(TrainerCase.HofDebutMinuteOffset), 2, 2, 1);
            MessageLoader.MessageLoader_GetString(loader, 0x11, format);
            StringTemplate.StringTemplate_Format(template, text, format);
        }
        else
        {
            MessageLoader.MessageLoader_GetString(loader, 0xC, format);
            StringTemplate.StringTemplate_SetString(template, 0, format, 0, 2);
            StringTemplate.StringTemplate_SetString(template, 1, format, 0, 2);
            MessageLoader.MessageLoader_GetString(loader, 0x14, text);
        }
        PrintRightAligned(windows[WindowHofDebut], text, BackWidth, 0);
        FormatAndPrint(loader, template, format, text, 0xF, windows[WindowHofDebut], BackWidth, 0x10);
        StringTemplate.StringTemplate_SetNumber(template, 5, (int)trainerCase.U32(TrainerCase.TimesLinkedOffset), 6, 0, 1);
        FormatAndPrint(loader, template, format, text, 0x16, windows[WindowTimesLinked], BackWidth, 0);
        MessageLoader.MessageLoader_GetString(loader, 0x12, text);
        Text.Text_AddPrinterWithParamsAndColor(windows[WindowLinkBattles], 0, text, 0x70, 0, 0, TextColor, null);
        TrainerCard_DrawNumber(windows[WindowLinkBattles], BackWidth, 0, 0, text, trainerCase.U32(TrainerCase.LinkBattleLossesOffset), 4, 1);
        MessageLoader.MessageLoader_GetString(loader, 0x13, text);
        Text.Text_AddPrinterWithParamsAndColor(windows[WindowLinkBattles], 0, text, 0xB0, 0, 0, TextColor, null);
        TrainerCard_DrawNumber(windows[WindowLinkBattles], BackWidth, 0x40, 0, text, trainerCase.U32(TrainerCase.LinkBattleWinsOffset), 4, 1);
        StringTemplate.StringTemplate_SetNumber(template, 5, (int)trainerCase.U32(TrainerCase.LinkTradesOffset), 6, 0, 1);
        FormatAndPrint(loader, template, format, text, 0x16, windows[WindowLinkTrades], BackWidth, 0);
        GameString.String_Free(format);
        StringTemplate.StringTemplate_Free(template);
        GameString.String_Free(text);
        MessageLoader.MessageLoader_Free(loader);
    }

    // OFFSET 0x0223CD44
    private static void TrainerCard_ClearWindows(Window[] windows, byte start, byte end)
    {
        for (int i = start; i <= end; i++)
        {
            Bg.Window_ClearAndCopyToVRAM(windows[i]);
        }
    }

    // OFFSET 0x0223CD64
    private static void TrainerCard_DrawNumber(Window window, uint windowWidth, uint endXOffset, uint yOffset, GameString text, uint num, byte maxDigits, int paddingMode)
    {
        GameString.String_FormatInt(text, (int)num, maxDigits, paddingMode, 1);
        int width = Font.Font_CalcStringWidth(0, text, 0);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, text, (uint)(windowWidth - (width + endXOffset)), yOffset, 0, TextColor, null);
    }

    // OFFSET 0x0223CDB0
    private static void TrainerCard_DrawString(Window window, uint windowWidth, uint endXOffset, uint yOffset, GameString text)
    {
        int width = Font.Font_CalcStringWidth(0, text, 0);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, text, (uint)(windowWidth - (width + endXOffset)), yOffset, 0, TextColor, null);
    }

    // OFFSET 0x0223CDE8
    private static void TrainerCard_DrawUpdatedTime(Window[] windows, TrainerCase trainerCase, GameString unused)
    {
        if (trainerCase.PlayTime == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        PlayTime.PlayTime_GetHours(trainerCase.PlayTime!);
        Bg.Window_FillRectWithColor(windows[WindowTime], 0, 0xB8, 0, 0x28, 0x10);
        MessageLoader loader = MessageLoader.MessageLoader_Init(0, MessageArchive, TrainerCardTextBank, HeapID)!;
        GameString text = GameString.String_Init(0x20, HeapID);
        GameString format = GameString.String_Init(0x20, HeapID);
        StringTemplate template = StringTemplate.StringTemplate_New(2, 0x20, HeapID);
        StringTemplate.StringTemplate_SetNumber(template, 0, PlayTime.PlayTime_GetHours(trainerCase.PlayTime!), 3, 1, 1);
        StringTemplate.StringTemplate_SetNumber(template, 1, PlayTime.PlayTime_GetMinutes(trainerCase.PlayTime!), 2, 2, 1);
        FormatAndPrint(loader, template, format, text, 0x10, windows[WindowTime], BackWidth, 0);
        MessageLoader.MessageLoader_Free(loader);
        GameString.String_Free(text);
        GameString.String_Free(format);
        StringTemplate.StringTemplate_Free(template);
    }

    // OFFSET 0x0223CECC
    private static void TrainerCard_BlinkPlaytimeColon(Window window, bool showColon, GameString colon)
    {
        if (showColon)
        {
            Text.Text_AddPrinterWithParamsAndColor(window, 0, colon, 0xCD, 0, 0, TextColor, null);
            return;
        }
        Bg.Window_FillRectWithColor(window, 0, 0xCD, 0, 5, 0x10);
        Bg.Window_CopyToVRAM(window);
    }
}
