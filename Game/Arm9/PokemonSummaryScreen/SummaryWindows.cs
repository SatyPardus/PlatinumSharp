namespace PlatinumSharp.Game;

public static partial class PokemonSummaryScreenApp
{
    private const uint StaticWindowTemplates = 0x020F4ED0;
    private const uint InfoWindowTemplates = 0x020F4E20;
    private const uint MemoWindowTemplates = 0x020F4DB0;
    private const uint SkillsWindowTemplates = 0x020F4E50;
    private const uint BattleMovesWindowTemplates = 0x020F4E90;
    private const uint ContestMovesWindowTemplates = 0x020F4DF0;
    private const uint RibbonsWindowTemplates = 0x020F4DD0;
    private const uint PoffinWindowTemplates = 0x020F4DB8;
    private const uint ColorWhite = 0xF0E00;
    private const uint ColorBlack = 0x10200;
    private const uint ColorBlue = 0x30400;
    private const uint ColorRed = 0x50600;
    private const uint ColorMessage = 0x1020F;
    private const int AlignLeft = 0;
    private const int AlignRight = 1;
    private const int AlignCenter = 2;
    private const int AbilityDescriptionBank = 0x264;
    private const int MoveDescriptionBank = 0x286;
    private const int ContestEffectDescriptionBank = 0xD2;
    private const int MoveParamPower = 2;
    private const int MoveParamAccuracy = 4;
    private const int WindowNickname = 34;
    private const int WindowMoveCancel = 24;
    private const int WindowLabelContestDesc = 29;
    private const int WindowLabelPower = 25;
    private const int WindowLabelAccuracy = 26;
    private const int WindowLabelEffect = 27;
    private const int WindowLabelRibbons = 31;
    private const int WindowLabelExit = 28;
    private const int MsgPoffinTooFull = 0xFE;

    private static readonly byte[] StaticWindowLabels =
    {
        0, 0x07, 1, 0x17, 2, 0x6D, 3, 0x7E, 4, 0x80, 5, 0x9D, 6, 0x04, 7, 0x08, 8, 0x0A, 9, 0x0C, 0xA, 0x0D,
        0xB, 0x0F, 0xC, 0x11, 0xD, 0x13, 0xF, 0x6E, 0x10, 0x6F, 0x11, 0x70, 0x12, 0x71, 0x13, 0x72, 0x14, 0x73,
        0x15, 0x74, 0x16, 0x7F,
    };

    private static Window ExtraWindow(PokemonSummaryScreen summaryScreen, int index)
    {
        return summaryScreen.ExtraWindows![index];
    }

    // OFFSET 0x0208FCF8
    private static void PokemonSummaryScreen_DrawStaticWindows(PokemonSummaryScreen summaryScreen)
    {
        for (ushort i = 0; i < PokemonSummaryScreen.StaticWindowMax; i++)
        {
            Bg.Window_AddFromTemplate(summaryScreen.BgConfig!, summaryScreen.StaticWindows[i], RomStructs.Arm9WindowTemplate((uint)(StaticWindowTemplates + i * RomStructs.WindowTemplateSize)));
            Bg.Window_FillTilemap(summaryScreen.StaticWindows[i], 0);
        }
        PrintStaticWindows(summaryScreen);
        PokemonSummaryScreen_PrintNicknameAndGender(summaryScreen);
        PokemonSummaryScreen_PrintLevel(summaryScreen);
        PokemonSummaryScreen_PrintItemName(summaryScreen);
    }

    // OFFSET 0x0208FD40
    public static void PokemonSummaryScreen_AddExtraWindows(PokemonSummaryScreen summaryScreen)
    {
        uint templates;
        switch (summaryScreen.Page)
        {
            case PageInfo:
                summaryScreen.NumExtraWindows = 6;
                templates = InfoWindowTemplates;
                break;
            case PageMemo:
                summaryScreen.NumExtraWindows = 1;
                templates = MemoWindowTemplates;
                break;
            case PageSkills:
                summaryScreen.NumExtraWindows = 8;
                templates = SkillsWindowTemplates;
                break;
            case PageBattleMoves:
                summaryScreen.NumExtraWindows = 8;
                templates = BattleMovesWindowTemplates;
                break;
            case PageContestMoves:
                summaryScreen.NumExtraWindows = 6;
                templates = ContestMovesWindowTemplates;
                break;
            case PageRibbons:
                summaryScreen.NumExtraWindows = 4;
                templates = RibbonsWindowTemplates;
                break;
            case PageCondition:
            case PageExit:
                if (summaryScreen.Data!.Mode != ModePoffin && summaryScreen.Data.Mode != ModeCondition)
                {
                    return;
                }
                summaryScreen.NumExtraWindows = 3;
                templates = PoffinWindowTemplates;
                break;
            default:
                return;
        }
        summaryScreen.ExtraWindows = Bg.Window_New(HeapIDSummary, (byte)summaryScreen.NumExtraWindows);
        for (uint i = 0; i < summaryScreen.NumExtraWindows; i++)
        {
            Bg.Window_AddFromTemplate(summaryScreen.BgConfig!, summaryScreen.ExtraWindows[i], RomStructs.Arm9WindowTemplate(templates));
            templates += RomStructs.WindowTemplateSize;
        }
    }

    // OFFSET 0x0208FE34
    public static void PokemonSummaryScreen_RemoveExtraWindows(PokemonSummaryScreen summaryScreen)
    {
        switch (summaryScreen.Page)
        {
            case PageCondition:
            case PageExit:
                if (summaryScreen.Data!.Mode != ModePoffin && summaryScreen.Data.Mode != ModeCondition)
                {
                    return;
                }
                break;
            case PageInfo:
            case PageMemo:
            case PageSkills:
            case PageBattleMoves:
            case PageContestMoves:
            case PageRibbons:
                break;
            default:
                return;
        }
        Bg.Windows_Delete(summaryScreen.ExtraWindows!, (byte)summaryScreen.NumExtraWindows);
    }

    // OFFSET 0x0208FE88
    private static void PokemonSummaryScreen_RemoveWindows(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummaryScreen_RemoveExtraWindows(summaryScreen);
        for (int i = 0; i < PokemonSummaryScreen.StaticWindowMax; i++)
        {
            Bg.Window_Remove(summaryScreen.StaticWindows[i]);
        }
    }

    // OFFSET 0x0208FEA4
    public static void PokemonSummaryScreen_PrintNicknameAndGender(PokemonSummaryScreen summaryScreen)
    {
        Window window = StaticWindow(summaryScreen, WindowNickname);
        Bg.Window_FillTilemap(window, 0);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, summaryScreen.MonData.Nickname, 0, 0, 0xFF, ColorWhite, null);
        if (summaryScreen.MonData.HideGender == 0)
        {
            if (summaryScreen.MonData.Gender == 0)
            {
                MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 1, summaryScreen.String!);
                PrintStringToWindow(summaryScreen, window, ColorBlue, AlignRight);
            }
            else if (summaryScreen.MonData.Gender == 1)
            {
                MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 2, summaryScreen.String!);
                PrintStringToWindow(summaryScreen, window, ColorRed, AlignRight);
            }
        }
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x0208FF3C
    public static void PokemonSummaryScreen_PrintLevel(PokemonSummaryScreen summaryScreen)
    {
        Window window = StaticWindow(summaryScreen, WindowMonLevel);
        Bg.Window_FillTilemap(window, 0);
        if (summaryScreen.MonData.IsEgg == 0)
        {
            FontSpecialCharsContext.FontSpecialChars_DrawPartyScreenLevelText(summaryScreen.SpecialChars!, 1, window, 0, 5);
            GameString format = MessageLoader.MessageLoader_GetNewString(summaryScreen.MsgLoader!, 3)!;
            StringTemplate.StringTemplate_SetNumber(summaryScreen.StrFormatter!, 0, summaryScreen.MonData.Level, 3, 0, 1);
            StringTemplate.StringTemplate_Format(summaryScreen.StrFormatter!, summaryScreen.String!, format);
            GameString.String_Free(format);
            Text.Text_AddPrinterWithParamsAndColor(window, 0, summaryScreen.String, 0x10, 0, 0xFF, ColorBlack, null);
        }
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x0208FFE0
    public static void PokemonSummaryScreen_PrintItemName(PokemonSummaryScreen summaryScreen)
    {
        Window window = StaticWindow(summaryScreen, WindowItemName);
        Bg.Window_FillTilemap(window, 0);
        if (summaryScreen.MonData.HeldItem != 0)
        {
            StringTemplate.StringTemplate_SetItemName(summaryScreen.StrFormatter!, 0, summaryScreen.MonData.HeldItem);
            GameString format = MessageLoader.MessageLoader_GetNewString(summaryScreen.MsgLoader!, 5)!;
            StringTemplate.StringTemplate_Format(summaryScreen.StrFormatter!, summaryScreen.String!, format);
            GameString.String_Free(format);
        }
        else
        {
            MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 6, summaryScreen.String!);
        }
        PrintStringToWindow(summaryScreen, window, ColorBlack, AlignLeft);
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelItem));
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02090064
    public static void PokemonSummaryScreen_ClearAndPrintButtonPrompt(PokemonSummaryScreen summaryScreen, uint entry)
    {
        Bg.Window_FillTilemap(StaticWindow(summaryScreen, WindowButtonPrompt), 0);
        PrintTextToStaticWindow(summaryScreen, WindowButtonPrompt, entry, ColorWhite, AlignLeft);
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowButtonPrompt));
    }

    // OFFSET 0x02090098
    private static bool DoesMonOTMatchPlayer(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummary data = summaryScreen.Data!;
        PokemonSummaryMonData monData = summaryScreen.MonData;
        if (monData.OTID == data.OTID && monData.OTGender == data.OTGender && GameString.String_Compare(monData.OTName, summaryScreen.PlayerName) == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020900D8
    private static void PrintStringToWindow(PokemonSummaryScreen summaryScreen, Window window, uint color, int alignment)
    {
        byte x;
        switch (alignment)
        {
            case AlignLeft:
                x = 0;
                break;
            case AlignRight:
            {
                byte width = (byte)Font.Font_CalcStringWidth(0, summaryScreen.String, 0);
                x = (byte)((byte)(Bg.Window_GetWidth(window) << 3) - width);
                break;
            }
            case AlignCenter:
            {
                byte width = (byte)Font.Font_CalcStringWidth(0, summaryScreen.String, 0);
                x = (byte)(((byte)(Bg.Window_GetWidth(window) << 3) - width) / 2);
                break;
            }
            default:
                x = (byte)alignment;
                break;
        }
        Text.Text_AddPrinterWithParamsAndColor(window, 0, summaryScreen.String, x, 0, 0xFF, color, null);
    }

    // OFFSET 0x02090158
    private static void PrintTextToStaticWindow(PokemonSummaryScreen summaryScreen, int windowIndex, uint entry, uint color, int alignment)
    {
        MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, entry, summaryScreen.String!);
        PrintStringToWindow(summaryScreen, summaryScreen.StaticWindows[windowIndex], color, alignment);
    }

    // OFFSET 0x02090184
    private static void SetAndFormatNumberBuf(PokemonSummaryScreen summaryScreen, uint entry, int number, uint maxDigits, int paddingMode)
    {
        GameString format = MessageLoader.MessageLoader_GetNewString(summaryScreen.MsgLoader!, entry)!;
        StringTemplate.StringTemplate_SetNumber(summaryScreen.StrFormatter!, 0, number, maxDigits, paddingMode, 1);
        StringTemplate.StringTemplate_Format(summaryScreen.StrFormatter!, summaryScreen.String!, format);
        GameString.String_Free(format);
    }

    // OFFSET 0x020901D0
    private static void PrintCurrentAndMaxInfo(PokemonSummaryScreen summaryScreen, int windowIndex, uint slashEntry, uint currentEntry, uint maxEntry, ushort current, ushort max, byte digits, byte centerX, byte y)
    {
        Window window = summaryScreen.ExtraWindows![windowIndex];
        MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, slashEntry, summaryScreen.String!);
        int slashWidth = Font.Font_CalcStringWidth(0, summaryScreen.String, 0);
        ushort slashX = (ushort)(centerX - (slashWidth >> 1));
        ushort maxX = (ushort)(slashX + slashWidth);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, summaryScreen.String, slashX, y, 0xFF, ColorBlack, null);
        SetAndFormatNumberBuf(summaryScreen, currentEntry, current, digits, 0);
        int currentWidth = Font.Font_CalcStringWidth(0, summaryScreen.String, 0);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, summaryScreen.String, (uint)(slashX - currentWidth), y, 0xFF, ColorBlack, null);
        SetAndFormatNumberBuf(summaryScreen, maxEntry, max, digits, 0);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, summaryScreen.String, maxX, y, 0xFF, ColorBlack, null);
    }

    // OFFSET 0x020902B0
    private static void PrintStaticWindows(PokemonSummaryScreen summaryScreen)
    {
        for (int i = 0; i < StaticWindowLabels.Length; i += 2)
        {
            PrintTextToStaticWindow(summaryScreen, StaticWindowLabels[i], StaticWindowLabels[i + 1], ColorWhite, AlignLeft);
        }
        MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 0x92, summaryScreen.String!);
        Text.Text_AddPrinterWithParamsAndColor(StaticWindow(summaryScreen, WindowMoveCancel), 0, summaryScreen.String, 0, 2, 0xFF, ColorWhite, null);
        PrintTextToStaticWindow(summaryScreen, 0x19, 0x95, ColorWhite, AlignLeft);
        PrintTextToStaticWindow(summaryScreen, 0x1A, 0x93, ColorWhite, AlignLeft);
        PrintTextToStaticWindow(summaryScreen, 0x1B, 0x94, ColorWhite, AlignLeft);
        PrintTextToStaticWindow(summaryScreen, 0x1C, 0xA2, ColorWhite, AlignCenter);
        PrintTextToStaticWindow(summaryScreen, 0x1D, 0xA0, ColorBlack, AlignCenter);
        PrintTextToStaticWindow(summaryScreen, 0x1E, 0xB6, ColorBlack, AlignLeft);
        PrintTextToStaticWindow(summaryScreen, 0x1F, 0xB3, ColorWhite, AlignLeft);
    }

    // OFFSET 0x020904C4
    public static void PokemonSummaryScreen_DrawExtraWindows(PokemonSummaryScreen summaryScreen)
    {
        Bg.Bg_FillTilemapRect(summaryScreen.BgConfig!, 1, 0, 0, 0, 0x20, 2, 0x10);
        Bg.Bg_FillTilemapRect(summaryScreen.BgConfig!, 1, 0, 0xE, 2, 0x12, 0x16, 0x10);
        switch (summaryScreen.Page)
        {
            case PageInfo:
                DrawInfoPageWindows(summaryScreen);
                break;
            case PageMemo:
                DrawMemoPageWindows(summaryScreen);
                break;
            case PageSkills:
                DrawSkillsPageWindows(summaryScreen);
                break;
            case PageCondition:
                DrawConditionPageWindows(summaryScreen);
                break;
            case PageBattleMoves:
                DrawBattleMovesPageWindows(summaryScreen);
                break;
            case PageContestMoves:
                DrawContestMovesPageWindows(summaryScreen);
                break;
            case PageRibbons:
                DrawRibbonsPageWindows(summaryScreen);
                break;
            case PageExit:
                DrawExitPageWindows(summaryScreen);
                break;
        }
    }

    // OFFSET 0x02090578
    private static void DrawInfoPageWindows(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, 0));
        for (int i = 7; i <= 14; i++)
        {
            Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, i));
        }
        for (int i = 0; i < 6; i++)
        {
            Bg.Window_FillTilemap(ExtraWindow(summaryScreen, i), 0);
        }
        PokemonSummaryMonData monData = summaryScreen.MonData;
        int dexNumber = DexMode.Pokemon_GetDexNumber((int)summaryScreen.Data!.DexMode, monData.Species);
        if (dexNumber != 0)
        {
            SetAndFormatNumberBuf(summaryScreen, 9, dexNumber, 3, 2);
        }
        else
        {
            MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 0x16, summaryScreen.String!);
        }
        if (monData.IsShiny == 0)
        {
            PrintStringToWindow(summaryScreen, ExtraWindow(summaryScreen, 0), ColorBlack, AlignCenter);
        }
        else
        {
            PrintStringToWindow(summaryScreen, ExtraWindow(summaryScreen, 0), ColorRed, AlignCenter);
        }
        int speciesWidth = Font.Font_CalcStringWidth(0, monData.SpeciesName, 0);
        Window speciesWindow = ExtraWindow(summaryScreen, 1);
        Text.Text_AddPrinterWithParamsAndColor(speciesWindow, 0, monData.SpeciesName, (uint)(speciesWindow.Width * 8 - speciesWidth) >> 1, 0, 0xFF, ColorBlack, null);
        int otWidth = Font.Font_CalcStringWidth(0, monData.OTName, 0);
        Window otWindow = ExtraWindow(summaryScreen, 2);
        uint otX = (uint)(otWindow.Width * 8 - otWidth) >> 1;
        if (monData.OTGender == 0)
        {
            Text.Text_AddPrinterWithParamsAndColor(otWindow, 0, monData.OTName, otX, 0, 0xFF, ColorBlue, null);
        }
        else
        {
            Text.Text_AddPrinterWithParamsAndColor(otWindow, 0, monData.OTName, otX, 0, 0xFF, ColorRed, null);
        }
        SetAndFormatNumberBuf(summaryScreen, 0x10, (ushort)monData.OTID, 5, 2);
        PrintStringToWindow(summaryScreen, ExtraWindow(summaryScreen, 3), ColorBlack, AlignCenter);
        SetAndFormatNumberBuf(summaryScreen, 0x12, (int)monData.CurExp, 7, 1);
        PrintStringToWindow(summaryScreen, ExtraWindow(summaryScreen, 4), ColorBlack, AlignCenter);
        if (monData.Level < MaxLevel)
        {
            SetAndFormatNumberBuf(summaryScreen, 0x15, (int)(monData.NextLevelExp - monData.CurExp), 7, 1);
        }
        else
        {
            SetAndFormatNumberBuf(summaryScreen, 0x15, 0, 7, 1);
        }
        PrintStringToWindow(summaryScreen, ExtraWindow(summaryScreen, 5), ColorBlack, AlignCenter);
        for (int i = 0; i < 6; i++)
        {
            Bg.Window_ScheduleCopyToVRAM(ExtraWindow(summaryScreen, i));
        }
    }

    // OFFSET 0x02090800
    private static void PrintTrainerMemo(Window window, Pokemon mon, bool isOTPlayer)
    {
        TrainerMemo memo = TrainerMemo.TrainerMemo_New(mon, isOTPlayer, HeapIDSummary);
        for (int i = 0; i < TrainerMemo.MaxLines; i++)
        {
            TrainerMemoLine line = memo.Lines[i];
            if (line.Text == null)
            {
                continue;
            }
            Text.Text_AddPrinterWithParamsAndColor(window, 0, line.Text, 0, (uint)((line.LineNum - 1) * 16), 0xFF, ColorBlack, null);
        }
        TrainerMemo.TrainerMemo_Free(memo);
    }

    // OFFSET 0x020908CC
    private static void DrawMemoPageWindows(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, 1));
        Bg.Window_FillTilemap(ExtraWindow(summaryScreen, 0), 0);
        object mon = PokemonSummaryScreen_MonData(summaryScreen);
        bool isOTPlayer = DoesMonOTMatchPlayer(summaryScreen);
        if (summaryScreen.Data!.DataType == DataTypeBoxMon)
        {
            Pokemon temp = Pokemon.Pokemon_New(HeapIDSummary);
            Pokemon.Pokemon_FromBoxPokemon((BoxPokemon)mon, temp);
            PrintTrainerMemo(ExtraWindow(summaryScreen, 0), temp, isOTPlayer);
        }
        else
        {
            PrintTrainerMemo(ExtraWindow(summaryScreen, 0), (Pokemon)mon, isOTPlayer);
        }
        Bg.Window_ScheduleCopyToVRAM(ExtraWindow(summaryScreen, 0));
    }

    private static void PrintStatValue(PokemonSummaryScreen summaryScreen, uint entry, ushort value, int window)
    {
        SetAndFormatNumberBuf(summaryScreen, entry, value, 3, 0);
        PrintStringToWindow(summaryScreen, ExtraWindow(summaryScreen, window), ColorBlack, AlignRight);
    }

    // OFFSET 0x0209093C
    private static void DrawSkillsPageWindows(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, 2));
        for (int i = 15; i <= 21; i++)
        {
            Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, i));
        }
        for (int i = 0; i < 8; i++)
        {
            Bg.Window_FillTilemap(ExtraWindow(summaryScreen, i), 0);
        }
        PokemonSummaryMonData monData = summaryScreen.MonData;
        byte centerX = (byte)(Bg.Window_GetWidth(ExtraWindow(summaryScreen, 0)) * 4);
        PrintCurrentAndMaxInfo(summaryScreen, 0, 0x75, 0x77, 0x76, monData.CurHP, monData.MaxHP, 3, centerX, 0);
        PrintStatValue(summaryScreen, 0x78, monData.Attack, 1);
        PrintStatValue(summaryScreen, 0x79, monData.Defense, 2);
        PrintStatValue(summaryScreen, 0x7A, monData.SpAttack, 3);
        PrintStatValue(summaryScreen, 0x7B, monData.SpDefense, 4);
        PrintStatValue(summaryScreen, 0x7C, monData.Speed, 5);
        StringTemplate.StringTemplate_SetAbilityName(summaryScreen.StrFormatter!, 0, monData.Ability);
        GameString format = MessageLoader.MessageLoader_GetNewString(summaryScreen.MsgLoader!, 0x7D)!;
        StringTemplate.StringTemplate_Format(summaryScreen.StrFormatter!, summaryScreen.String!, format);
        GameString.String_Free(format);
        PrintStringToWindow(summaryScreen, ExtraWindow(summaryScreen, 6), ColorBlack, AlignLeft);
        MessageLoader descriptions = MessageLoader.MessageLoader_Init(1, MessageArchive, AbilityDescriptionBank, HeapIDSummary)!;
        MessageLoader.MessageLoader_GetString(descriptions, monData.Ability, summaryScreen.String!);
        MessageLoader.MessageLoader_Free(descriptions);
        PrintStringToWindow(summaryScreen, ExtraWindow(summaryScreen, 7), ColorBlack, AlignLeft);
        for (int i = 0; i < 8; i++)
        {
            Bg.Window_ScheduleCopyToVRAM(ExtraWindow(summaryScreen, i));
        }
    }

    private static void PrintPreferredFlavor(PokemonSummaryScreen summaryScreen)
    {
        Window window = ExtraWindow(summaryScreen, 1);
        Bg.Window_FillTilemap(window, 0);
        MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 0xA5, summaryScreen.String!);
        PrintStringToWindow(summaryScreen, window, ColorWhite, AlignLeft);
        MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, (uint)(summaryScreen.MonData.PreferredFlavor + 0xA6), summaryScreen.String!);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, summaryScreen.String, 0, 0x10, 0xFF, ColorBlack, null);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02090BDC
    private static void DrawConditionPageWindows(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, 3));
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, 22));
        byte mode = summaryScreen.Data!.Mode;
        if (mode != ModePoffin && mode != ModeCondition)
        {
            return;
        }
        PrintPreferredFlavor(summaryScreen);
        if (summaryScreen.Data.Mode != ModePoffin)
        {
            return;
        }
        Window window = ExtraWindow(summaryScreen, 0);
        Bg.Window_FillTilemap(window, 0);
        MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 0xA3, summaryScreen.String!);
        PrintStringToWindow(summaryScreen, window, ColorWhite, AlignLeft);
        Bg.Window_ScheduleCopyToVRAM(window);
        PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, window);
    }

    private static void DrawMoveNameWindows(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, StaticWindow(summaryScreen, WindowButtonPrompt));
        for (int i = 0; i < 4; i++)
        {
            Bg.Window_FillTilemap(ExtraWindow(summaryScreen, i), 0);
        }
        for (byte i = 0; i < 4; i++)
        {
            PrintMoveNameAndPP(summaryScreen, i);
        }
        for (int i = 0; i < 4; i++)
        {
            Bg.Window_ScheduleCopyToVRAM(ExtraWindow(summaryScreen, i));
        }
    }

    // OFFSET 0x02090CD4
    private static void DrawBattleMovesPageWindows(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, 4));
        if (summaryScreen.Data!.Mode == ModeSelectMove)
        {
            PokemonSummaryScreen_ClearAndPrintButtonPrompt(summaryScreen, 0x9B);
        }
        else
        {
            PokemonSummaryScreen_ClearAndPrintButtonPrompt(summaryScreen, 0x81);
        }
        DrawMoveNameWindows(summaryScreen);
    }

    // OFFSET 0x02090D90
    private static void DrawContestMovesPageWindows(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, 5));
        if (summaryScreen.Data!.Mode == ModeSelectMove)
        {
            PokemonSummaryScreen_ClearAndPrintButtonPrompt(summaryScreen, 0x9F);
        }
        else
        {
            PokemonSummaryScreen_ClearAndPrintButtonPrompt(summaryScreen, 0x9E);
        }
        DrawMoveNameWindows(summaryScreen);
    }

    // OFFSET 0x02090E4C
    private static void DrawRibbonsPageWindows(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelRibbons));
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelRibbonCount));
        Bg.Window_FillTilemap(ExtraWindow(summaryScreen, 0), 0);
        SetAndFormatNumberBuf(summaryScreen, 0xB7, summaryScreen.RibbonMax, 3, 0);
        PrintStringToWindow(summaryScreen, ExtraWindow(summaryScreen, 0), ColorBlack, AlignLeft);
        Bg.Window_ScheduleCopyToVRAM(ExtraWindow(summaryScreen, 0));
        if (summaryScreen.RibbonMax == 0)
        {
            return;
        }
        PokemonSummaryScreen_ClearAndPrintButtonPrompt(summaryScreen, 0xB4);
        PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, StaticWindow(summaryScreen, WindowButtonPrompt));
    }

    // OFFSET 0x02090EC8
    private static void DrawExitPageWindows(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelExit));
        PokemonSummaryScreen_ClearAndPrintButtonPrompt(summaryScreen, 0xA1);
        PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, StaticWindow(summaryScreen, WindowButtonPrompt));
        byte mode = summaryScreen.Data!.Mode;
        if (mode != ModePoffin && mode != ModeCondition)
        {
            return;
        }
        PrintPreferredFlavor(summaryScreen);
    }

    // OFFSET 0x02090F84
    private static void PokemonSummaryScreen_PrintRibbonNumbers(PokemonSummaryScreen summaryScreen)
    {
        Window window = ExtraWindow(summaryScreen, 1);
        Bg.Window_FillTilemap(window, 0);
        GameString format = MessageLoader.MessageLoader_GetNewString(summaryScreen.MsgLoader!, 0xBA)!;
        StringTemplate.StringTemplate_SetNumber(summaryScreen.StrFormatter!, 0, summaryScreen.RibbonMax, 3, 0, 1);
        StringTemplate.StringTemplate_Format(summaryScreen.StrFormatter!, summaryScreen.String!, format);
        GameString.String_Free(format);
        byte x = (byte)(0x38 - (byte)Font.Font_CalcStringWidth(0, summaryScreen.String, 0));
        Text.Text_AddPrinterWithParamsAndColor(window, 0, summaryScreen.String, x, 0, 0xFF, ColorBlack, null);
        GameString slash = MessageLoader.MessageLoader_GetNewString(summaryScreen.MsgLoader!, 0xB9)!;
        x = (byte)(x - (byte)Font.Font_CalcStringWidth(0, slash, 0));
        Text.Text_AddPrinterWithParamsAndColor(window, 0, slash, x, 0, 0xFF, ColorBlack, null);
        GameString.String_Free(slash);
        format = MessageLoader.MessageLoader_GetNewString(summaryScreen.MsgLoader!, 0xB8)!;
        StringTemplate.StringTemplate_SetNumber(summaryScreen.StrFormatter!, 0, summaryScreen.RibbonRow * RibbonsPerRow + summaryScreen.RibbonCol + 1, 3, 0, 1);
        StringTemplate.StringTemplate_Format(summaryScreen.StrFormatter!, summaryScreen.String!, format);
        GameString.String_Free(format);
        x = (byte)(x - (byte)Font.Font_CalcStringWidth(0, summaryScreen.String, 0));
        Text.Text_AddPrinterWithParamsAndColor(window, 0, summaryScreen.String, x, 0, 0xFF, ColorBlack, null);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x020910E4
    private static void PokemonSummaryScreen_PrintRibbonNameAndDesc(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_FillTilemap(ExtraWindow(summaryScreen, 2), 0);
        Bg.Window_FillTilemap(ExtraWindow(summaryScreen, 3), 0);
        uint name = Ribbon.Ribbon_GetData(summaryScreen.RibbonID, Ribbon.ParamNameID);
        MessageLoader.MessageLoader_GetString(summaryScreen.RibbonLoader!, name, summaryScreen.String!);
        PrintStringToWindow(summaryScreen, ExtraWindow(summaryScreen, 2), ColorWhite, AlignLeft);
        byte[] specialRibbons = ((RibbonSaveData)summaryScreen.Data!.SpecialRibbons!).Ribbons;
        uint description = Ribbon.Ribbon_GetDescriptionID(specialRibbons, summaryScreen.RibbonID);
        MessageLoader.MessageLoader_GetString(summaryScreen.RibbonLoader!, description, summaryScreen.String!);
        PrintStringToWindow(summaryScreen, ExtraWindow(summaryScreen, 3), ColorBlack, AlignLeft);
        Bg.Window_ScheduleCopyToVRAM(ExtraWindow(summaryScreen, 2));
        Bg.Window_ScheduleCopyToVRAM(ExtraWindow(summaryScreen, 3));
    }

    // OFFSET 0x02091188
    private static void PrintMoveNameAndPP(PokemonSummaryScreen summaryScreen, byte slot)
    {
        Window window = ExtraWindow(summaryScreen, slot);
        uint move;
        uint curPP;
        uint maxPP;
        if (slot != 4)
        {
            move = summaryScreen.MonData.Moves[slot];
            curPP = summaryScreen.MonData.CurPP[slot];
            maxPP = summaryScreen.MonData.MaxPP[slot];
        }
        else
        {
            move = summaryScreen.Data!.Move;
            curPP = MoveTable.MoveTable_CalcMaxPP((ushort)move, 0);
            maxPP = curPP;
        }
        MessageLoader.MessageLoader_GetString(summaryScreen.MoveNameLoader!, move, summaryScreen.String!);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, summaryScreen.String, 1, 2, 0xFF, ColorWhite, null);
        if (move != 0)
        {
            MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 0x87, summaryScreen.String!);
            Text.Text_AddPrinterWithParamsAndColor(window, 0, summaryScreen.String, 0x10, 0x10, 0xFF, ColorBlack, null);
            PrintCurrentAndMaxInfo(summaryScreen, slot, 0x75, (uint)(slot + 0x88), (uint)(slot + 0x8D), (ushort)curPP, (ushort)maxPP, 2, 0x3C, 0x10);
            return;
        }
        MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 0x99, summaryScreen.String!);
        int width = Font.Font_CalcStringWidth(0, summaryScreen.String, 0);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, summaryScreen.String, (uint)(0x3C - (width >> 1)), 0x10, 0xFF, ColorBlack, null);
    }

    // OFFSET 0x020912A4
    private static void PokemonSummaryScreen_PrintBattleMoveAttributes(PokemonSummaryScreen summaryScreen, uint move)
    {
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelPower));
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelAccuracy));
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelEffect));
        Bg.Window_FillTilemap(ExtraWindow(summaryScreen, 5), 0);
        Bg.Window_FillTilemap(ExtraWindow(summaryScreen, 6), 0);
        Bg.Window_FillTilemap(ExtraWindow(summaryScreen, 7), 0);
        uint power = MoveTable.MoveTable_LoadParam((int)move, MoveParamPower);
        if (power <= 1)
        {
            MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 0x9A, summaryScreen.String!);
        }
        else
        {
            SetAndFormatNumberBuf(summaryScreen, 0x96, (int)power, 3, 1);
        }
        PrintStringToWindow(summaryScreen, ExtraWindow(summaryScreen, 5), ColorBlack, AlignCenter);
        uint accuracy = MoveTable.MoveTable_LoadParam((int)move, MoveParamAccuracy);
        if (accuracy == 0)
        {
            MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 0x9A, summaryScreen.String!);
        }
        else
        {
            SetAndFormatNumberBuf(summaryScreen, 0x97, (int)accuracy, 3, 1);
        }
        PrintStringToWindow(summaryScreen, ExtraWindow(summaryScreen, 6), ColorBlack, AlignCenter);
        MessageLoader descriptions = MessageLoader.MessageLoader_Init(1, MessageArchive, MoveDescriptionBank, HeapIDSummary)!;
        MessageLoader.MessageLoader_GetString(descriptions, move, summaryScreen.String!);
        PrintStringToWindow(summaryScreen, ExtraWindow(summaryScreen, 7), ColorBlack, AlignLeft);
        MessageLoader.MessageLoader_Free(descriptions);
        Bg.Window_ScheduleCopyToVRAM(ExtraWindow(summaryScreen, 5));
        Bg.Window_ScheduleCopyToVRAM(ExtraWindow(summaryScreen, 6));
        Bg.Window_ScheduleCopyToVRAM(ExtraWindow(summaryScreen, 7));
    }

    // OFFSET 0x020913D8
    public static void PokemonSummaryScreen_ClearBattleAttributeWindows(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelPower));
        Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelAccuracy));
        Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelEffect));
        Bg.Window_ClearAndScheduleCopyToVRAM(ExtraWindow(summaryScreen, 5));
        Bg.Window_ClearAndScheduleCopyToVRAM(ExtraWindow(summaryScreen, 6));
        Bg.Window_ClearAndScheduleCopyToVRAM(ExtraWindow(summaryScreen, 7));
    }

    // OFFSET 0x02091420
    public static void PokemonSummaryScreen_ShowMove5OrCancel(PokemonSummaryScreen summaryScreen)
    {
        if (summaryScreen.Data!.Move != 0)
        {
            Bg.Window_FillTilemap(ExtraWindow(summaryScreen, 4), 0);
            PrintMoveNameAndPP(summaryScreen, 4);
            Bg.Window_ScheduleCopyToVRAM(ExtraWindow(summaryScreen, 4));
            return;
        }
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowMoveCancel));
    }

    // OFFSET 0x0209145C
    public static void PokemonSummaryScreen_HideMoveCancelText(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowMoveCancel));
        Bg.Bg_ScheduleTilemapTransfer(summaryScreen.BgConfig!, 1);
    }

    // OFFSET 0x02091474
    private static void PokemonSummaryScreen_SwapMoveNameAndPP(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_FillTilemap(ExtraWindow(summaryScreen, summaryScreen.Cursor), 0);
        Bg.Window_FillTilemap(ExtraWindow(summaryScreen, summaryScreen.CursorTmp), 0);
        PrintMoveNameAndPP(summaryScreen, summaryScreen.Cursor);
        PrintMoveNameAndPP(summaryScreen, summaryScreen.CursorTmp);
        Bg.Window_ScheduleCopyToVRAM(ExtraWindow(summaryScreen, summaryScreen.Cursor));
        Bg.Window_ScheduleCopyToVRAM(ExtraWindow(summaryScreen, summaryScreen.CursorTmp));
    }

    // OFFSET 0x020914F8
    private static void PokemonSummaryScreen_PrintHMMovesCantBeForgotten(PokemonSummaryScreen summaryScreen)
    {
        Window window;
        if (summaryScreen.Page == PageBattleMoves)
        {
            Bg.Window_ClearAndScheduleCopyToVRAM(ExtraWindow(summaryScreen, 5));
            Bg.Window_ClearAndScheduleCopyToVRAM(ExtraWindow(summaryScreen, 6));
            Bg.Window_ClearAndScheduleCopyToVRAM(ExtraWindow(summaryScreen, 7));
            window = ExtraWindow(summaryScreen, 7);
        }
        else
        {
            window = ExtraWindow(summaryScreen, 5);
        }
        Bg.Window_FillTilemap(window, 0);
        MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 0x9C, summaryScreen.String!);
        PrintStringToWindow(summaryScreen, window, ColorBlack, AlignLeft);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02091570
    private static void PokemonSummaryScreen_PrintContestMoveAttributes(PokemonSummaryScreen summaryScreen, uint move)
    {
        Bg.Window_FillTilemap(ExtraWindow(summaryScreen, 5), 0);
        int description = ContestEffect.ContestEffect_GetDescription((int)MoveTable.MoveTable_LoadParam((int)move, MoveParamContestEffect));
        MessageLoader descriptions = MessageLoader.MessageLoader_Init(0, MessageArchive, ContestEffectDescriptionBank, HeapIDSummary)!;
        MessageLoader.MessageLoader_GetString(descriptions, (uint)description, summaryScreen.String!);
        Text.Text_AddPrinterWithParamsAndColor(ExtraWindow(summaryScreen, 5), 0, summaryScreen.String, 0, 0, 0xFF, ColorBlack, null);
        MessageLoader.MessageLoader_Free(descriptions);
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelContestDesc));
        Bg.Window_ScheduleCopyToVRAM(ExtraWindow(summaryScreen, 5));
    }

    // OFFSET 0x020915F4
    public static void PokemonSummaryScreen_ClearContestAttributeWindows(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelContestDesc));
        Bg.Window_ClearAndScheduleCopyToVRAM(ExtraWindow(summaryScreen, 5));
    }

    // OFFSET 0x02091610
    public static void PokemonSummaryScreen_PrintPoffinFeedMsg(PokemonSummaryScreen summaryScreen, int message)
    {
        uint entry;
        switch (message)
        {
            case 0:
                entry = 0xAC;
                break;
            case 1:
                entry = 0xAE;
                break;
            case 2:
                entry = 0xAF;
                break;
            case 3:
                entry = 0xB0;
                break;
            case 4:
                entry = 0xAD;
                break;
            case MsgPoffinTooFull:
                entry = 0xB1;
                break;
            default:
                entry = 0xB2;
                break;
        }
        Window window = ExtraWindow(summaryScreen, 2);
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(window, 1, 0x3E2, 0xD);
        Bg.Window_FillTilemap(window, 0xF);
        MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, entry, summaryScreen.String!);
        Text.Text_AddPrinterWithParamsAndColor(window, 1, summaryScreen.String, 0, 0, 0xFF, ColorMessage, null);
        Bg.Window_ScheduleCopyToVRAM(window);
    }
}
