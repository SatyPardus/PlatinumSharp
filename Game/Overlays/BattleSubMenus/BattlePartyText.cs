namespace PlatinumSharp.Game;

public static partial class BattleParties
{
    private const uint MessageBoxWindowTemplates = 0x02228EA0;
    private const uint PartyPokemonWindowTemplates = 0x02228F54;
    private const uint SelectPokemonWindowTemplates = 0x02228EDC;
    private const uint PokemonSummaryWindowTemplates = 0x022290FC;
    private const uint PokemonMovesWindowTemplates = 0x02228FBC;
    private const uint MoveSummaryWindowTemplates = 0x02229074;
    private const uint RestoreMovePPWindowTemplates = 0x02228EFC;
    private const uint LearnMoveWindowTemplates = 0x02228F24;
    private const uint ConfirmLearnMoveWindowTemplates = 0x02229014;
    private const uint ConfirmLearnMoveContestWindowTemplates = 0x02228F84;
    private const uint MoveNameMessageIDs = 0x02228EB0;
    private const uint PartyNameMessageIDs = 0x02228EC4;
    private const int WindowTemplateSize = 8;
    private const uint ColorLabel = 0xF0E00;
    private const uint ColorValue = 0x10200;
    private const uint ColorButton = 0x70809;
    private const uint ColorMaleLight = 0x70800;
    private const uint ColorMaleDark = 0xA0B00;
    private const uint ColorFemaleLight = 0x30400;
    private const uint ColorFemaleDark = 0xC0D00;
    private const int FontSystem = 0;
    private const int FontMessage = 1;
    private const int FontSubscreenText = 2;
    private const int MsgMoveToLearnName = 0x49;
    private const int MoveEmbargo = 0x175;

    private static Window PartyWindow(BattleParty battleParty, int index)
    {
        return battleParty.Windows![index];
    }

    private static void PrintText(Window window, int font, GameString text, int x, int y, uint color)
    {
        Text.Text_AddPrinterWithParamsAndColor(window, font, text, (uint)x, (uint)y, 0xFF, color, null);
    }

    private static int MessageID(uint table, int index)
    {
        return (int)BattleSubMenuData.U32((uint)(table + index * 4));
    }

    // OFFSET 0x02221A88
    private static void BattlePartyText_InitializeWindows(BattleParty battleParty)
    {
        for (int i = 0; i < BattleParty.NumMessageBoxWindows; i++)
        {
            Bg.Window_AddFromTemplate(battleParty.Background!, battleParty.MessageBoxWindows[i], RomStructs.WindowTemplate(BattleSubMenuData.Bytes((uint)(MessageBoxWindowTemplates + i * WindowTemplateSize), WindowTemplateSize), 0));
        }
        BattlePartyText_InitializeScreenWindows(battleParty, battleParty.CurrentScreen);
    }

    // OFFSET 0x02221AC4
    private static void BattlePartyText_InitializeScreenWindows(BattleParty battleParty, int screen)
    {
        uint templates = 0;
        switch (screen)
        {
            case ScreenPokemonParty:
                battleParty.NumWindows = 6;
                templates = PartyPokemonWindowTemplates;
                break;
            case ScreenSelectPokemon:
                battleParty.NumWindows = 4;
                templates = SelectPokemonWindowTemplates;
                break;
            case ScreenPokemonSummary:
                battleParty.NumWindows = 0x23;
                templates = PokemonSummaryWindowTemplates;
                break;
            case ScreenPokemonMoves:
                battleParty.NumWindows = 0xB;
                templates = PokemonMovesWindowTemplates;
                break;
            case ScreenMoveSummary:
                battleParty.NumWindows = 0x11;
                templates = MoveSummaryWindowTemplates;
                break;
            case ScreenRestoreMovePP:
                battleParty.NumWindows = 5;
                templates = RestoreMovePPWindowTemplates;
                break;
            case ScreenLearnMove:
            case ScreenLearnMoveContestStats:
                battleParty.NumWindows = 6;
                templates = LearnMoveWindowTemplates;
                break;
            case ScreenConfirmLearnMove:
                battleParty.NumWindows = 0xC;
                templates = ConfirmLearnMoveWindowTemplates;
                break;
            case ScreenConfirmLearnMoveContestStats:
                battleParty.NumWindows = 7;
                templates = ConfirmLearnMoveContestWindowTemplates;
                break;
        }
        battleParty.Windows = Bg.Window_New(battleParty.Context!.HeapID, battleParty.NumWindows);
        for (int i = 0; i < battleParty.NumWindows; i++)
        {
            Bg.Window_AddFromTemplate(battleParty.Background!, battleParty.Windows[i], RomStructs.WindowTemplate(BattleSubMenuData.Bytes((uint)(templates + i * WindowTemplateSize), WindowTemplateSize), 0));
        }
    }

    // OFFSET 0x02221BB0
    private static void BattlePartyText_ClearScreenWindows(BattleParty battleParty)
    {
        Bg.Windows_Delete(battleParty.Windows!, battleParty.NumWindows);
    }

    // OFFSET 0x02221BC8
    private static void BattlePartyText_ClearWindows(BattleParty battleParty)
    {
        Bg.Windows_Delete(battleParty.Windows!, battleParty.NumWindows);
        for (int i = 0; i < BattleParty.NumMessageBoxWindows; i++)
        {
            Bg.Window_Remove(battleParty.MessageBoxWindows[i]);
        }
    }

    // OFFSET 0x02221BF8
    private static void BattlePartyText_ChangeScreen(BattleParty battleParty, int screen)
    {
        switch (screen)
        {
            case ScreenPokemonParty:
                RenderPartyPokemonScreen(battleParty);
                break;
            case ScreenSelectPokemon:
                RenderSelectPokemonScreen(battleParty);
                break;
            case ScreenPokemonSummary:
                RenderPokemonSummaryScreen(battleParty);
                break;
            case ScreenPokemonMoves:
                RenderPokemonMovesScreen(battleParty);
                break;
            case ScreenMoveSummary:
                RenderMoveSummaryScreen(battleParty);
                break;
            case ScreenRestoreMovePP:
                RenderRestoreMovePPScreen(battleParty);
                break;
            case ScreenLearnMove:
            case ScreenLearnMoveContestStats:
                RenderLearnMoveScreen(battleParty);
                break;
            case ScreenConfirmLearnMove:
                RenderConfirmLearnMoveScreen(battleParty);
                break;
            case ScreenConfirmLearnMoveContestStats:
                RenderConfirmLearnMoveContestStatsScreen(battleParty);
                break;
        }
    }

    private static GameString? GenderMarkerString(BattleParty battleParty, BattlePartyPokemon partyPokemon)
    {
        if (partyPokemon.HideGenderMarker != 0 || partyPokemon.IsEgg != 0)
        {
            return null;
        }
        if (partyPokemon.Gender == 0)
        {
            return MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x10);
        }
        if (partyPokemon.Gender == 1)
        {
            return MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x11);
        }
        return null;
    }

    // OFFSET 0x02221C54
    private static void PrintPokemonNameHeader(BattleParty battleParty, int windowIndex, int font, int partySlot, int x, int y)
    {
        Window window = PartyWindow(battleParty, windowIndex);
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[partySlot];
        GameString name = GameString.String_Init(0xC, battleParty.Context!.HeapID);
        GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, (uint)MessageID(PartyNameMessageIDs, partySlot))!;
        StringTemplate.StringTemplate_SetNickname(battleParty.StringTemplate!, 0, Pokemon.Pokemon_GetBoxPokemon(partyPokemon.Mon!));
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, name, format);
        if (font == FontSystem)
        {
            PrintText(window, font, name, x, y, ColorLabel);
        }
        else
        {
            PrintText(window, font, name, x, y, ColorButton);
        }
        GameString.String_Free(format);
        GameString.String_Free(name);
        if (partyPokemon.HideGenderMarker == 0 && partyPokemon.IsEgg == 0)
        {
            if (partyPokemon.Gender == 0)
            {
                GameString marker = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x10)!;
                int markerX = Bg.Window_GetWidth(window) * 8 - Font.Font_CalcStringWidth(FontSystem, marker, 0);
                PrintText(window, FontSystem, marker, markerX, y, font == FontSystem ? ColorMaleLight : ColorMaleDark);
                GameString.String_Free(marker);
            }
            else if (partyPokemon.Gender == 1)
            {
                GameString marker = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x11)!;
                int markerX = Bg.Window_GetWidth(window) * 8 - Font.Font_CalcStringWidth(FontSystem, marker, 0);
                PrintText(window, FontSystem, marker, markerX, y, font == FontSystem ? ColorFemaleLight : ColorFemaleDark);
                GameString.String_Free(marker);
            }
        }
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02221E08
    private static void PrintPokemonLevel(BattleParty battleParty, int windowIndex, int partySlot, int x, int y)
    {
        FontSpecialCharsContext.FontSpecialChars_DrawPartyScreenText(battleParty.Unused3!, 1, battleParty.PartyPokemon[partySlot].Level, 3, 0, PartyWindow(battleParty, windowIndex), (uint)(x + 8), (uint)y);
        Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, windowIndex));
    }

    // OFFSET 0x02221E50
    private static void PrintPartyPokemonStats(BattleParty battleParty, int windowIndex, int partySlot, int x, int y)
    {
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[partySlot];
        Window window = PartyWindow(battleParty, windowIndex);
        FontSpecialCharsContext.FontSpecialChars_DrawPartyScreenHPText(battleParty.Unused3!, partyPokemon.CurHP, 3, 1, window, (uint)x, (uint)y);
        FontSpecialCharsContext.FontSpecialChars_DrawPartyScreenLevelText(battleParty.Unused3!, 0, window, (uint)(x + 0x18), (uint)y);
        FontSpecialCharsContext.FontSpecialChars_DrawPartyScreenHPText(battleParty.Unused3!, partyPokemon.MaxHP, 3, 0, window, (uint)(x + 0x20), (uint)y);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02221ED0
    private static void DrawPokemonHealthBar(BattleParty battleParty, int windowIndex, int partySlot, int x, int y)
    {
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[partySlot];
        int color = 1;
        byte fill = HealthBar.HealthBar_Fill(partyPokemon.CurHP, partyPokemon.MaxHP, 0x30);
        switch (HealthBar.HealthBar_Color(partyPokemon.CurHP, partyPokemon.MaxHP, 0x30))
        {
            case 0:
                Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, windowIndex));
                return;
            case 1:
                color = 5;
                break;
            case 2:
                color = 3;
                break;
        }
        Window window = PartyWindow(battleParty, windowIndex);
        Bg.Window_FillRectWithColor(window, color + 1, x, y + 1, fill, 1);
        Bg.Window_FillRectWithColor(window, color, x, y + 2, fill, 2);
        Bg.Window_FillRectWithColor(window, color + 1, x, y + 4, fill, 1);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02221FA8
    private static void PrintPokemonAbilityName(BattleParty battleParty, int windowIndex, int partySlot)
    {
        GameString text = GameString.String_Init(0x10, battleParty.Context!.HeapID);
        GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 8)!;
        StringTemplate.StringTemplate_SetAbilityName(battleParty.StringTemplate!, 0, battleParty.PartyPokemon[partySlot].Ability);
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, text, format);
        PrintText(PartyWindow(battleParty, windowIndex), FontSystem, text, 0, 0, ColorLabel);
        GameString.String_Free(format);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, windowIndex));
    }

    // OFFSET 0x02222030
    private static void PrintPokemonHeldItem(BattleParty battleParty, int windowIndex, int partySlot)
    {
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[partySlot];
        GameString text;
        if (partyPokemon.HeldItem == 0)
        {
            text = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x14)!;
        }
        else
        {
            text = GameString.String_Init(0x12, battleParty.Context!.HeapID);
            GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 9)!;
            StringTemplate.StringTemplate_SetItemName(battleParty.StringTemplate!, 0, partyPokemon.HeldItem);
            StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, text, format);
            GameString.String_Free(format);
        }
        PrintText(PartyWindow(battleParty, windowIndex), FontSystem, text, 0, 0, ColorLabel);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, windowIndex));
    }

    // OFFSET 0x022220CC
    private static void PrintMoveName(BattleParty battleParty, ushort move, int windowIndex, int messageID, int font, int y, uint color)
    {
        Window window = PartyWindow(battleParty, windowIndex);
        GameString text = GameString.String_Init(0x10, battleParty.Context!.HeapID);
        GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, (uint)messageID)!;
        StringTemplate.StringTemplate_SetMoveName(battleParty.StringTemplate!, 0, move);
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, text, format);
        int x;
        if (font == FontSubscreenText)
        {
            x = (int)((uint)(Bg.Window_GetWidth(window) * 8 - Font.Font_CalcStringWidth(font, text, 0)) >> 1);
        }
        else
        {
            x = 0;
        }
        PrintText(window, font, text, x, y, color);
        GameString.String_Free(format);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02222170
    private static void PrintPPLabel(BattleParty battleParty, int windowIndex, int x, int y)
    {
        GameString text = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0xE)!;
        PrintText(PartyWindow(battleParty, windowIndex), FontSystem, text, x, y, ColorLabel);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, windowIndex));
    }

    // OFFSET 0x022221C8
    private static void PrintSelectOptionText(BattleParty battleParty, int messageID)
    {
        Window window = battleParty.MessageBoxWindows[0];
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(window, 1, 1, 0xE);
        Bg.Window_FillTilemap(window, 0xF);
        GameString text = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, (uint)messageID)!;
        PrintText(window, FontMessage, text, 0, 0, ColorValue);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x0222222C
    private static void PrintButtonText(BattleParty battleParty, int windowIndex, int messageID)
    {
        Window window = PartyWindow(battleParty, windowIndex);
        GameString text = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, (uint)messageID)!;
        int width = Font.Font_CalcStringWidth(FontSubscreenText, text, 0);
        int x = (int)((uint)(Bg.Window_GetWidth(window) * 8 - width) >> 1);
        PrintText(window, FontSubscreenText, text, x, 6, ColorButton);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    private static int SummaryOffset(BattleParty battleParty)
    {
        return (ushort)(battleParty.UseAltSummaryWindows * 0x16);
    }

    private static void PrintRightAlignedNumber(BattleParty battleParty, Window window, int messageID, int value, uint digits, int paddingMode, int y, uint color)
    {
        GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, (uint)messageID)!;
        GameString text = GameString.String_Init(8, battleParty.Context!.HeapID);
        StringTemplate.StringTemplate_SetNumber(battleParty.StringTemplate!, 0, value, digits, paddingMode, 1);
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, text, format);
        byte width = (byte)Font.Font_CalcStringWidth(FontSystem, text, 0);
        byte x = (byte)(Bg.Window_GetWidth(window) * 8 - width);
        PrintText(window, FontSystem, text, x, y, color);
        GameString.String_Free(format);
        GameString.String_Free(text);
    }

    // OFFSET 0x02222290
    private static void PrintSelectedPokemonLevel(BattleParty battleParty, int partySlot)
    {
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[partySlot];
        int offset = SummaryOffset(battleParty);
        GameString label = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x17)!;
        PrintText(PartyWindow(battleParty, 0x13), FontSystem, label, 0, 0, ColorLabel);
        GameString.String_Free(label);
        GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x18)!;
        GameString text = GameString.String_Init(8, battleParty.Context!.HeapID);
        StringTemplate.StringTemplate_SetNumber(battleParty.StringTemplate!, 0, partyPokemon.Level, 3, 0, 1);
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, text, format);
        PrintText(PartyWindow(battleParty, offset + 0xB), FontSystem, text, 0, 0, ColorLabel);
        GameString.String_Free(format);
        GameString.String_Free(text);
        label = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x19)!;
        PrintText(PartyWindow(battleParty, 0x14), FontSystem, label, 0, 0, ColorLabel);
        GameString.String_Free(label);
        format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x1A)!;
        text = GameString.String_Init(0xE, battleParty.Context.HeapID);
        if (partyPokemon.Level < 100)
        {
            StringTemplate.StringTemplate_SetNumber(battleParty.StringTemplate!, 0, (int)(partyPokemon.NextLevelExp - partyPokemon.Exp), 6, 1, 1);
        }
        else
        {
            StringTemplate.StringTemplate_SetNumber(battleParty.StringTemplate!, 0, 0, 6, 1, 1);
        }
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, text, format);
        Window expWindow = PartyWindow(battleParty, offset + 0xC);
        ushort x = (ushort)(Bg.Window_GetWidth(expWindow) * 8 - Font.Font_CalcStringWidth(FontSystem, text, 0));
        PrintText(expWindow, FontSystem, text, x, 0, ColorValue);
        GameString.String_Free(format);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, 0x13));
        Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, offset + 0xB));
        Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, 0x14));
        Bg.Window_ScheduleCopyToVRAM(expWindow);
    }

    private static void PrintStat(BattleParty battleParty, int labelMessage, int valueMessage, int labelWindow, int valueWindow, int value)
    {
        GameString label = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, (uint)labelMessage)!;
        PrintText(PartyWindow(battleParty, labelWindow), FontSystem, label, 0, 0, ColorLabel);
        GameString.String_Free(label);
        Window window = PartyWindow(battleParty, SummaryOffset(battleParty) + valueWindow);
        PrintRightAlignedNumber(battleParty, window, valueMessage, value, 3, 0, 0, ColorValue);
        Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, labelWindow));
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02222464
    private static void PrintPokemonAttackStat(BattleParty battleParty, int partySlot)
    {
        PrintStat(battleParty, 0x20, 0x21, 0xE, 5, battleParty.PartyPokemon[partySlot].Attack);
    }

    // OFFSET 0x02222560
    private static void PrintPokemonDefenseStat(BattleParty battleParty, int partySlot)
    {
        PrintStat(battleParty, 0x22, 0x23, 0xF, 6, battleParty.PartyPokemon[partySlot].Defence);
    }

    // OFFSET 0x0222265C
    private static void PrintPokemonSpeedStat(BattleParty battleParty, int partySlot)
    {
        PrintStat(battleParty, 0x28, 0x29, 0x10, 7, battleParty.PartyPokemon[partySlot].Speed);
    }

    // OFFSET 0x0222275C
    private static void PrintPokemonSpAtkStat(BattleParty battleParty, int partySlot)
    {
        PrintStat(battleParty, 0x24, 0x25, 0x11, 8, battleParty.PartyPokemon[partySlot].SpAtk);
    }

    // OFFSET 0x0222285C
    private static void PrintPokemonSpDefStat(BattleParty battleParty, int partySlot)
    {
        PrintStat(battleParty, 0x26, 0x27, 0x12, 9, battleParty.PartyPokemon[partySlot].SpDef);
    }

    // OFFSET 0x0222295C
    private static void PrintPokemonHPStat(BattleParty battleParty, int partySlot)
    {
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[partySlot];
        int offset = SummaryOffset(battleParty);
        GameString label = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x1C)!;
        PrintText(PartyWindow(battleParty, 0xD), FontSystem, label, 0, 0, ColorLabel);
        GameString.String_Free(label);
        GameString slash = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x1F)!;
        int slashWidth = Font.Font_CalcStringWidth(FontSystem, slash, 0);
        ushort center = (ushort)((Bg.Window_GetWidth(PartyWindow(battleParty, 4)) * 8 - slashWidth) / 2);
        Window window = PartyWindow(battleParty, offset + 4);
        PrintText(window, FontSystem, slash, center, 0, ColorValue);
        GameString.String_Free(slash);
        GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x1D)!;
        GameString text = GameString.String_Init(8, battleParty.Context!.HeapID);
        StringTemplate.StringTemplate_SetNumber(battleParty.StringTemplate!, 0, partyPokemon.CurHP, 3, 0, 1);
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, text, format);
        int width = Font.Font_CalcStringWidth(FontSystem, text, 0);
        PrintText(window, FontSystem, text, center - width, 0, ColorValue);
        GameString.String_Free(format);
        GameString.String_Free(text);
        format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x1E)!;
        text = GameString.String_Init(8, battleParty.Context.HeapID);
        StringTemplate.StringTemplate_SetNumber(battleParty.StringTemplate!, 0, partyPokemon.MaxHP, 3, 0, 1);
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, text, format);
        PrintText(window, FontSystem, text, center + slashWidth, 0, ColorValue);
        GameString.String_Free(format);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, 0xD));
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02222AF4
    private static void PrintPokemonAbilityDescription(BattleParty battleParty, int partySlot)
    {
        int offset = SummaryOffset(battleParty);
        MessageLoader loader = MessageLoader.MessageLoader_Init(1, 0x1A, 0x264, battleParty.Context!.HeapID)!;
        GameString text = MessageLoader.MessageLoader_GetNewString(loader, battleParty.PartyPokemon[partySlot].Ability)!;
        PrintText(PartyWindow(battleParty, offset + 2), FontSystem, text, 0, 0, ColorValue);
        GameString.String_Free(text);
        MessageLoader.MessageLoader_Free(loader);
        Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, offset + 2));
    }

    private static void PrintLabel(BattleParty battleParty, int windowIndex, int messageID)
    {
        Window window = PartyWindow(battleParty, windowIndex);
        GameString text = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, (uint)messageID)!;
        PrintText(window, FontSystem, text, 0, 0, ColorLabel);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    private static void PrintRightAlignedMessage(BattleParty battleParty, Window window, int messageID)
    {
        GameString text = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, (uint)messageID)!;
        ushort width = (ushort)Font.Font_CalcStringWidth(FontSystem, text, 0);
        ushort x = (ushort)(Bg.Window_GetWidth(window) * 8 - width);
        PrintText(window, FontSystem, text, x, 0, ColorValue);
        GameString.String_Free(text);
    }

    private static void PrintRightAlignedValue(BattleParty battleParty, Window window, int messageID, int value)
    {
        GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, (uint)messageID)!;
        GameString text = GameString.String_Init(8, battleParty.Context!.HeapID);
        StringTemplate.StringTemplate_SetNumber(battleParty.StringTemplate!, 0, value, 3, 0, 1);
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, text, format);
        ushort width = (ushort)Font.Font_CalcStringWidth(FontSystem, text, 0);
        ushort x = (ushort)(Bg.Window_GetWidth(window) * 8 - width);
        PrintText(window, FontSystem, text, x, 0, ColorValue);
        GameString.String_Free(format);
        GameString.String_Free(text);
    }

    // OFFSET 0x02222B6C
    private static void PrintMoveAccuracyLabel(BattleParty battleParty, int windowIndex)
    {
        PrintLabel(battleParty, windowIndex, 0x33);
    }

    // OFFSET 0x02222BB4
    private static void PrintMoveAccuracyValue(BattleParty battleParty, int windowIndex, int accuracy)
    {
        Window window = PartyWindow(battleParty, windowIndex);
        if (accuracy == 0)
        {
            PrintRightAlignedMessage(battleParty, window, 0x32);
        }
        else
        {
            PrintRightAlignedValue(battleParty, window, 0x34, accuracy);
        }
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02222C9C
    private static void PrintMovePowerLabel(BattleParty battleParty, int windowIndex)
    {
        PrintLabel(battleParty, windowIndex, 0x30);
    }

    // OFFSET 0x02222CE4
    private static void PrintMovePowerValue(BattleParty battleParty, int windowIndex, int power)
    {
        Window window = PartyWindow(battleParty, windowIndex);
        if (power <= 1)
        {
            PrintRightAlignedMessage(battleParty, window, 0x32);
        }
        else
        {
            PrintRightAlignedValue(battleParty, window, 0x31, power);
        }
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02222DCC
    private static void PrintMoveDescription(BattleParty battleParty, int windowIndex, ushort move)
    {
        Window window = PartyWindow(battleParty, windowIndex);
        MessageLoader loader = MessageLoader.MessageLoader_Init(1, 0x1A, 0x286, battleParty.Context!.HeapID)!;
        GameString text = MessageLoader.MessageLoader_GetNewString(loader, move)!;
        PrintText(window, FontSystem, text, 0, 0, ColorValue);
        GameString.String_Free(text);
        MessageLoader.MessageLoader_Free(loader);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02222E2C
    private static void PrintMoveContestEffect(BattleParty battleParty, int windowIndex, ushort move)
    {
        Window window = PartyWindow(battleParty, windowIndex);
        int messageID = ContestEffect.ContestEffect_GetDescription((int)MoveTable.MoveTable_LoadParam(move, 0xA));
        MessageLoader loader = MessageLoader.MessageLoader_Init(0, 0x1A, 0xD2, battleParty.Context!.HeapID)!;
        GameString text = MessageLoader.MessageLoader_GetNewString(loader, (uint)messageID)!;
        PrintText(window, FontSystem, text, 0, 0, ColorValue);
        GameString.String_Free(text);
        MessageLoader.MessageLoader_Free(loader);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02222E94
    private static void PrintMoveCategoryLabel(BattleParty battleParty, int windowIndex)
    {
        Window window = PartyWindow(battleParty, windowIndex);
        GameString text = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x35)!;
        ushort width = (ushort)Font.Font_CalcStringWidth(FontSystem, text, 0);
        ushort x = (ushort)((Bg.Window_GetWidth(window) * 8 - width) / 2);
        PrintText(window, FontSystem, text, x, 0, ColorLabel);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02222EF8
    private static void PrintMoveCategory(BattleParty battleParty, int windowIndex, int moveClass)
    {
        Window window = PartyWindow(battleParty, windowIndex);
        GameString? text = moveClass switch
        {
            0 => MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x36),
            1 => MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x38),
            2 => MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x37),
            _ => null,
        };
        int x = Font.Font_CalcCenterAlignment(FontSystem, text, 0, 0x38);
        PrintText(window, FontSystem, text!, x, 0, ColorValue);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02222F74
    private static void PrintSummaryScreenMovePPStats(BattleParty battleParty, int windowIndex, int currentPP, int maxPP)
    {
        Window window = PartyWindow(battleParty, windowIndex);
        GameString slash = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x2E)!;
        int slashWidth = Font.Font_CalcStringWidth(FontSystem, slash, 0);
        int center = (int)((uint)(Bg.Window_GetWidth(window) * 8 - slashWidth) >> 1);
        PrintText(window, FontSystem, slash, center, 0, ColorLabel);
        GameString.String_Free(slash);
        GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x2C)!;
        GameString text = GameString.String_Init(6, battleParty.Context!.HeapID);
        StringTemplate.StringTemplate_SetNumber(battleParty.StringTemplate!, 0, currentPP, 3, 0, 1);
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, text, format);
        PrintText(window, FontSystem, text, center - Font.Font_CalcStringWidth(FontSystem, text, 0), 0, ColorLabel);
        GameString.String_Free(format);
        GameString.String_Free(text);
        format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x2D)!;
        text = GameString.String_Init(6, battleParty.Context.HeapID);
        StringTemplate.StringTemplate_SetNumber(battleParty.StringTemplate!, 0, maxPP, 3, 0, 1);
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, text, format);
        PrintText(window, FontSystem, text, center + slashWidth, 0, ColorLabel);
        GameString.String_Free(format);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x022230A8
    private static void PrintConfirmMoveButton(BattleParty battleParty, int windowIndex)
    {
        Window window = PartyWindow(battleParty, windowIndex);
        GameString text;
        if (battleParty.Context!.SelectedMoveSlot == MoveToLearnSlot)
        {
            text = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x3B)!;
        }
        else
        {
            text = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x3A)!;
        }
        int width = Font.Font_CalcStringWidth(FontSubscreenText, text, 0);
        PrintText(window, FontSubscreenText, text, (int)((uint)(0x60 - width) >> 1), 6, ColorButton);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02223118
    private static void BattlePartyText_PrintHMMovesCantBeForgottenText(BattleParty battleParty)
    {
        Window window = battleParty.CurrentScreen == ScreenConfirmLearnMove ? PartyWindow(battleParty, 8) : PartyWindow(battleParty, 5);
        Bg.Window_FillTilemap(window, 0);
        GameString text = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x3C)!;
        PrintText(window, FontSystem, text, 0, 0, ColorValue);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x0222317C
    private static void PrintMovesScreenMovePPStats(BattleParty battleParty, BattlePartyPokemonMove move, int windowIndex)
    {
        Window window = PartyWindow(battleParty, windowIndex);
        GameString text = GameString.String_Init(6, battleParty.Context!.HeapID);
        GameString label = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x2B)!;
        PrintText(window, FontSystem, label, 0x28, 0x18, ColorLabel);
        GameString.String_Free(label);
        GameString slash = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x2E)!;
        int slashWidth = Font.Font_CalcStringWidth(FontSystem, slash, 0);
        PrintText(window, FontSystem, slash, 0x50, 0x18, ColorLabel);
        GameString.String_Free(slash);
        GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x2D)!;
        StringTemplate.StringTemplate_SetNumber(battleParty.StringTemplate!, 0, move.MaxPP, 2, 0, 1);
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, text, format);
        PrintText(window, FontSystem, text, slashWidth + 0x50, 0x18, ColorLabel);
        GameString.String_Free(format);
        format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x2C)!;
        StringTemplate.StringTemplate_SetNumber(battleParty.StringTemplate!, 0, move.CurrentPP, 2, 0, 1);
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, text, format);
        int width = Font.Font_CalcStringWidth(FontSystem, text, 0);
        PrintText(window, FontSystem, text, 0x50 - width, 0x18, ColorLabel);
        GameString.String_Free(format);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x022232C8
    private static void PrintMoveCurrentPP(BattleParty battleParty, BattlePartyPokemonMove move, int windowIndex)
    {
        Window window = PartyWindow(battleParty, windowIndex);
        GameString text = GameString.String_Init(6, battleParty.Context!.HeapID);
        GameString label = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x2B)!;
        int labelEnd = Font.Font_CalcStringWidth(FontSystem, label, 0);
        GameString.String_Free(label);
        labelEnd += 0x28;
        Bg.Window_FillRectWithColor(window, 0, (ushort)labelEnd, 0x18, (ushort)(0x50 - labelEnd), 0x10);
        GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x2C)!;
        StringTemplate.StringTemplate_SetNumber(battleParty.StringTemplate!, 0, move.CurrentPP, 2, 0, 1);
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, text, format);
        int width = Font.Font_CalcStringWidth(FontSystem, text, 0);
        PrintText(window, FontSystem, text, 0x50 - width, 0x18, ColorLabel);
        GameString.String_Free(format);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x0222339C
    private static void RenderPartyPokemonScreen(BattleParty battleParty)
    {
        for (int i = 0; i < Party.Party_GetCurrentCount(battleParty.Context!.Party!); i++)
        {
            Bg.Window_FillTilemap(PartyWindow(battleParty, i), 0);
            BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[i];
            if (partyPokemon.Species == 0)
            {
                continue;
            }
            PrintPokemonNameHeader(battleParty, i, FontSubscreenText, i, 0x20, 8);
            if (partyPokemon.IsEgg == 0)
            {
                BattlePartyText_RenderPartyPokemonStats(battleParty, i);
            }
            if (Pokemon.Pokemon_GetSummaryStatus(partyPokemon.Mon!) == Pokemon.SummaryStatusNone)
            {
                BattlePartyText_PrintPartyPokemonLevel(battleParty, i);
            }
        }
        if (battleParty.Context.BattlePartyMode == ModeUseItem)
        {
            PrintSelectOptionText(battleParty, 7);
        }
        else
        {
            PrintSelectOptionText(battleParty, 6);
        }
    }

    // OFFSET 0x02223448
    private static void BattlePartyText_RenderPartyPokemonStats(BattleParty battleParty, int partySlot)
    {
        Window window = PartyWindow(battleParty, partySlot);
        Bg.Window_FillRectWithColor(window, 0, 0x38, 0x20, 0x18, 8);
        Bg.Window_FillRectWithColor(window, 0, 0x40, 0x18, 0x40, 8);
        PrintPartyPokemonStats(battleParty, partySlot, partySlot, 0x38, 0x20);
        DrawPokemonHealthBar(battleParty, partySlot, partySlot, 0x40, 0x18);
    }

    // OFFSET 0x022234A8
    private static void BattlePartyText_PrintPartyPokemonLevel(BattleParty battleParty, int partySlot)
    {
        if (battleParty.PartyPokemon[partySlot].IsEgg == 0)
        {
            PrintPokemonLevel(battleParty, partySlot, partySlot, 0, 0x20);
        }
    }

    // OFFSET 0x022234C8
    private static void RenderSelectPokemonScreen(BattleParty battleParty)
    {
        for (int i = 0; i < 4; i++)
        {
            Bg.Window_FillTilemap(PartyWindow(battleParty, i), 0);
        }
        PrintSelectedPokemonName(battleParty, Index(battleParty));
        PrintButtonText(battleParty, 1, 0xF);
        if (battleParty.PartyPokemon[Index(battleParty)].IsEgg == 0)
        {
            PrintButtonText(battleParty, 2, 0x12);
            PrintButtonText(battleParty, 3, 0x13);
            return;
        }
        Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, 2));
        Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, 3));
    }

    // OFFSET 0x02223550
    private static void PrintSelectedPokemonName(BattleParty battleParty, int partySlot)
    {
        Window window = PartyWindow(battleParty, 0);
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[partySlot];
        GameString name = GameString.String_Init(0xC, battleParty.Context!.HeapID);
        GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, (uint)MessageID(PartyNameMessageIDs, partySlot))!;
        StringTemplate.StringTemplate_SetNickname(battleParty.StringTemplate!, 0, Pokemon.Pokemon_GetBoxPokemon(partyPokemon.Mon!));
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, name, format);
        GameString.String_Free(format);
        GameString? marker = GenderMarkerString(battleParty, partyPokemon);
        byte nameWidth = (byte)Font.Font_CalcStringWidth(FontSubscreenText, name, 0);
        byte markerWidth;
        int padding;
        if (marker == null)
        {
            markerWidth = 0;
            padding = 0;
        }
        else
        {
            markerWidth = (byte)Font.Font_CalcStringWidth(FontSystem, marker, 0);
            padding = 8;
        }
        byte x = (byte)((Bg.Window_GetWidth(window) * 8 - nameWidth - markerWidth - padding) / 2);
        PrintText(window, FontSubscreenText, name, x, 8, ColorButton);
        GameString.String_Free(name);
        if (marker != null)
        {
            PrintText(window, FontSystem, marker, padding + x + nameWidth, 8, partyPokemon.Gender == 0 ? ColorMaleDark : ColorFemaleDark);
            GameString.String_Free(marker);
        }
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x022236B8
    private static void RenderPokemonMovesScreen(BattleParty battleParty)
    {
        int offset = (ushort)(battleParty.UseAltSummaryWindows * 6);
        for (int i = 0; i < 5; i++)
        {
            Bg.Window_FillTilemap(PartyWindow(battleParty, offset + i), 0);
        }
        Bg.Window_FillTilemap(PartyWindow(battleParty, 5), 0);
        PrintPokemonNameHeader(battleParty, offset, FontSystem, Index(battleParty), 0, 0);
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[Index(battleParty)];
        for (int i = 0; i < 4; i++)
        {
            BattlePartyPokemonMove move = partyPokemon.Moves[i];
            if (move.Move == 0)
            {
                continue;
            }
            PrintMoveName(battleParty, move.Move, offset + 1 + i, MessageID(MoveNameMessageIDs, i), FontSubscreenText, 8, ColorButton);
            PrintMovesScreenMovePPStats(battleParty, move, offset + 1 + i);
        }
        PrintButtonText(battleParty, 5, 0x12);
        for (int i = 1; i < 5; i++)
        {
            Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, offset + i));
        }
        battleParty.UseAltSummaryWindows ^= 1;
    }

    // OFFSET 0x022237F8
    private static void RenderPokemonSummaryScreen(BattleParty battleParty)
    {
        int offset = SummaryOffset(battleParty);
        int[] fixedWindows = { 0x13, 0x14, 0xE, 0xF, 0x10, 0x11, 0x12, 0xD, 0x15 };
        foreach (int window in fixedWindows)
        {
            Bg.Window_FillTilemap(PartyWindow(battleParty, window), 0);
        }
        int[] offsetWindows = { 0, 0xA, 0xB, 0xC, 5, 6, 7, 8, 9, 4, 1, 2, 3 };
        foreach (int window in offsetWindows)
        {
            Bg.Window_FillTilemap(PartyWindow(battleParty, offset + window), 0);
        }
        int slot = Index(battleParty);
        PrintPokemonNameHeader(battleParty, offset, FontSystem, slot, 0, 0);
        PrintPokemonHPStat(battleParty, slot);
        DrawPokemonHealthBar(battleParty, offset + 0xA, slot, 0, 0);
        PrintSelectedPokemonLevel(battleParty, slot);
        PrintPokemonAttackStat(battleParty, slot);
        PrintPokemonDefenseStat(battleParty, slot);
        PrintPokemonSpeedStat(battleParty, slot);
        PrintPokemonSpAtkStat(battleParty, slot);
        PrintPokemonSpDefStat(battleParty, slot);
        PrintPokemonAbilityName(battleParty, offset + 1, slot);
        PrintPokemonHeldItem(battleParty, offset + 3, slot);
        PrintPokemonAbilityDescription(battleParty, slot);
        PrintButtonText(battleParty, 0x15, 0x13);
        battleParty.UseAltSummaryWindows ^= 1;
    }

    // OFFSET 0x02223A10
    private static void RenderMoveSummaryScreen(BattleParty battleParty)
    {
        int offset = battleParty.UseAltSummaryWindows * 0xB;
        for (int i = 6; i <= 10; i++)
        {
            Bg.Window_FillTilemap(PartyWindow(battleParty, i), 0);
        }
        int[] offsetWindows = { 1, 0, 2, 3, 5, 4 };
        foreach (int window in offsetWindows)
        {
            Bg.Window_FillTilemap(PartyWindow(battleParty, offset + window), 0);
        }
        BattlePartyContext context = battleParty.Context!;
        BattlePartyPokemonMove move = battleParty.PartyPokemon[context.SelectedPartyIndex].Moves[context.SelectedMoveSlot];
        PrintPokemonNameHeader(battleParty, 6, FontSystem, context.SelectedPartyIndex, 0, 0);
        PrintPPLabel(battleParty, 7, 0, 0);
        PrintMoveName(battleParty, move.Move, offset, MessageID(MoveNameMessageIDs, context.SelectedMoveSlot), FontSystem, 0, ColorLabel);
        PrintMoveAccuracyLabel(battleParty, 8);
        PrintMoveAccuracyValue(battleParty, offset + 2, move.Accuracy);
        PrintMovePowerLabel(battleParty, 9);
        PrintMovePowerValue(battleParty, offset + 3, move.Power);
        PrintMoveDescription(battleParty, offset + 4, move.Move);
        PrintMoveCategoryLabel(battleParty, 0xA);
        PrintMoveCategory(battleParty, offset + 5, move.Class);
        PrintSummaryScreenMovePPStats(battleParty, offset + 1, move.CurrentPP, move.MaxPP);
        battleParty.UseAltSummaryWindows ^= 1;
    }

    // OFFSET 0x02223B74
    private static void RenderLearnMoveScreen(BattleParty battleParty)
    {
        for (int i = 0; i < 6; i++)
        {
            Bg.Window_FillTilemap(PartyWindow(battleParty, i), 0);
        }
        PrintPokemonNameHeader(battleParty, 0, FontSystem, Index(battleParty), 0, 0);
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[Index(battleParty)];
        for (int i = 0; i < 4; i++)
        {
            BattlePartyPokemonMove move = partyPokemon.Moves[i];
            if (move.Move == 0)
            {
                continue;
            }
            PrintMoveName(battleParty, move.Move, i + 1, MessageID(MoveNameMessageIDs, i), FontSubscreenText, 8, ColorButton);
            PrintMovesScreenMovePPStats(battleParty, move, i + 1);
        }
        ushort moveToLearn = battleParty.Context!.MoveToLearn;
        PrintMoveName(battleParty, moveToLearn, 5, MsgMoveToLearnName, FontSubscreenText, 8, ColorButton);
        BattlePartyPokemonMove learnMove = new();
        learnMove.CurrentPP = (byte)MoveTable.MoveTable_LoadParam(moveToLearn, 5);
        learnMove.MaxPP = learnMove.CurrentPP;
        PrintMovesScreenMovePPStats(battleParty, learnMove, 5);
        for (int i = 1; i <= 5; i++)
        {
            Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, i));
        }
    }

    // OFFSET 0x02223C9C
    private static void RenderConfirmLearnMoveScreen(BattleParty battleParty)
    {
        int[] windows = { 0, 2, 3, 1, 4, 6, 5, 7, 9, 0xA, 8, 0xB };
        foreach (int window in windows)
        {
            Bg.Window_FillTilemap(PartyWindow(battleParty, window), 0);
        }
        BattlePartyContext context = battleParty.Context!;
        PrintPokemonNameHeader(battleParty, 0, FontSystem, context.SelectedPartyIndex, 0, 0);
        PrintPPLabel(battleParty, 2, 0, 0);
        PrintMoveAccuracyLabel(battleParty, 4);
        PrintMovePowerLabel(battleParty, 5);
        PrintMoveCategoryLabel(battleParty, 9);
        if (context.SelectedMoveSlot < 4)
        {
            BattlePartyPokemonMove move = battleParty.PartyPokemon[context.SelectedPartyIndex].Moves[context.SelectedMoveSlot];
            PrintMoveName(battleParty, move.Move, 1, MessageID(MoveNameMessageIDs, context.SelectedMoveSlot), FontSystem, 0, ColorLabel);
            PrintMoveAccuracyValue(battleParty, 6, move.Accuracy);
            PrintMovePowerValue(battleParty, 7, move.Power);
            PrintMoveDescription(battleParty, 8, move.Move);
            PrintMoveCategory(battleParty, 0xA, move.Class);
            PrintSummaryScreenMovePPStats(battleParty, 3, move.CurrentPP, move.MaxPP);
        }
        else
        {
            int pp = (int)MoveTable.MoveTable_LoadParam(context.MoveToLearn, 5);
            PrintMoveName(battleParty, context.MoveToLearn, 1, MsgMoveToLearnName, FontSystem, 0, ColorLabel);
            PrintMoveDescription(battleParty, 8, context.MoveToLearn);
            PrintMoveAccuracyValue(battleParty, 6, (int)MoveTable.MoveTable_LoadParam(context.MoveToLearn, 4));
            PrintMovePowerValue(battleParty, 7, (int)MoveTable.MoveTable_LoadParam(context.MoveToLearn, 2));
            PrintMoveCategory(battleParty, 0xA, (int)MoveTable.MoveTable_LoadParam(context.MoveToLearn, 1));
            PrintSummaryScreenMovePPStats(battleParty, 3, pp, pp);
        }
        PrintConfirmMoveButton(battleParty, 0xB);
    }

    // OFFSET 0x02223E68
    private static void RenderRestoreMovePPScreen(BattleParty battleParty)
    {
        for (int i = 0; i < 5; i++)
        {
            Bg.Window_FillTilemap(PartyWindow(battleParty, i), 0);
        }
        PrintPokemonNameHeader(battleParty, 0, FontSystem, Index(battleParty), 0, 0);
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[Index(battleParty)];
        for (int i = 0; i < 4; i++)
        {
            BattlePartyPokemonMove move = partyPokemon.Moves[i];
            if (move.Move == 0)
            {
                continue;
            }
            PrintMoveName(battleParty, move.Move, i + 1, MessageID(MoveNameMessageIDs, i), FontSubscreenText, 8, ColorButton);
            PrintMovesScreenMovePPStats(battleParty, move, i + 1);
        }
        if (Item.Item_LoadParam(battleParty.Context!.SelectedBattleBagItem, ItemParamRestorePPAll, battleParty.Context.HeapID) == 0)
        {
            PrintSelectOptionText(battleParty, 0x5E);
        }
        for (int i = 1; i <= 4; i++)
        {
            Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, i));
        }
    }

    // OFFSET 0x02223F5C
    private static void BattlePartyText_PrintSelectedMoveCurrentPP(BattleParty battleParty, ushort windowIndex, ushort moveSlot)
    {
        PrintMoveCurrentPP(battleParty, battleParty.PartyPokemon[Index(battleParty)].Moves[moveSlot], windowIndex);
    }

    // OFFSET 0x02223F7C
    private static void RenderConfirmLearnMoveContestStatsScreen(BattleParty battleParty)
    {
        int[] windows = { 0, 2, 3, 1, 4, 5, 6 };
        foreach (int window in windows)
        {
            Bg.Window_FillTilemap(PartyWindow(battleParty, window), 0);
        }
        BattlePartyContext context = battleParty.Context!;
        PrintPokemonNameHeader(battleParty, 0, FontSystem, context.SelectedPartyIndex, 0, 0);
        PrintPPLabel(battleParty, 2, 0, 0);
        GameString label = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x39)!;
        int x = Font.Font_CalcCenterAlignment(FontSystem, label, 0, 0x60);
        PrintText(PartyWindow(battleParty, 4), FontSystem, label, x, 0, ColorLabel);
        GameString.String_Free(label);
        Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, 4));
        if (context.SelectedMoveSlot < 4)
        {
            BattlePartyPokemonMove move = battleParty.PartyPokemon[context.SelectedPartyIndex].Moves[context.SelectedMoveSlot];
            PrintMoveName(battleParty, move.Move, 1, MessageID(MoveNameMessageIDs, context.SelectedMoveSlot), FontSystem, 0, ColorLabel);
            PrintMoveContestEffect(battleParty, 5, move.Move);
            PrintSummaryScreenMovePPStats(battleParty, 3, move.CurrentPP, move.MaxPP);
        }
        else
        {
            int pp = (int)MoveTable.MoveTable_LoadParam(context.MoveToLearn, 5);
            PrintMoveName(battleParty, context.MoveToLearn, 1, MsgMoveToLearnName, FontSystem, 0, ColorLabel);
            PrintMoveContestEffect(battleParty, 5, context.MoveToLearn);
            PrintSummaryScreenMovePPStats(battleParty, 3, pp, pp);
        }
        PrintConfirmMoveButton(battleParty, 6);
    }

    // OFFSET 0x022240E0
    private static void BattlePartyText_DisplayErrorMessage(BattleParty battleParty)
    {
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(battleParty.MessageBoxWindows[1], 1, 1, 0xE);
        Bg.Window_FillTilemap(battleParty.MessageBoxWindows[1], 0xF);
        BattlePartyText_PrintToErrorMessageBox(battleParty);
    }

    // OFFSET 0x02224108
    private static void BattlePartyText_PrintToErrorMessageBox(BattleParty battleParty)
    {
        RenderText.RenderControlFlags_SetCanABSpeedUpPrint(1);
        int delay = BattleMessages.BattleSystem_GetTextSpeed(battleParty.Context!.BattleSys!);
        battleParty.TextPrinterID = (byte)Text.Text_AddPrinterWithParams(battleParty.MessageBoxWindows[1], FontMessage, battleParty.String, 0, 0, delay, null);
    }

    private static void FormatPokemonMessage(BattleParty battleParty, int messageID, Pokemon mon)
    {
        GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, (uint)messageID)!;
        StringTemplate.StringTemplate_SetNickname(battleParty.StringTemplate!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, battleParty.String!, format);
        GameString.String_Free(format);
    }

    // OFFSET 0x02224144
    private static void BattlePartyText_PrintUseItemEffect(BattleParty battleParty)
    {
        BattlePartyContext context = battleParty.Context!;
        byte[] itemData = Item.Item_Load(context.SelectedBattleBagItem, 0, context.HeapID)!;
        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(context.BattleSys!, context.Battler, context.PokemonPartySlots[context.SelectedPartyIndex]);
        ushort currentHP = (ushort)Pokemon.Pokemon_GetValue(mon, 0xA3, null);
        byte cured = 0;
        int[] cureParams = { 0xF, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15 };
        for (int i = 0; i < cureParams.Length; i++)
        {
            if (Item.Item_Get(itemData, 0, cureParams[i]) != 0)
            {
                cured = (byte)(cured | (1 << i));
            }
        }
        ushort previousHP = battleParty.PartyPokemon[context.SelectedPartyIndex].CurHP;
        if (previousHP == 0 && currentHP != 0)
        {
            FormatPokemonMessage(battleParty, 0x58, mon);
        }
        else if (currentHP != previousHP)
        {
            GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x52)!;
            StringTemplate.StringTemplate_SetNickname(battleParty.StringTemplate!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
            StringTemplate.StringTemplate_SetNumber(battleParty.StringTemplate!, 1, currentHP - battleParty.PartyPokemon[context.SelectedPartyIndex].CurHP, 3, 0, 1);
            StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, battleParty.String!, format);
            GameString.String_Free(format);
        }
        else if (Item.Item_Get(itemData, 0, ItemParamRestorePPOne) != 0 || Item.Item_Get(itemData, 0, ItemParamRestorePPAll) != 0)
        {
            MessageLoader.MessageLoader_GetString(battleParty.MessageLoader!, 0x57, battleParty.String!);
        }
        else
        {
            int messageID = cured switch
            {
                1 => 0x5C,
                2 => 0x53,
                4 => 0x55,
                8 => 0x56,
                0x10 => 0x54,
                0x20 => 0x5A,
                0x40 => 0x5B,
                _ => 0x59,
            };
            FormatPokemonMessage(battleParty, messageID, mon);
        }
    }

    // OFFSET 0x0222449C
    private static void BattlePartyText_PrintEmbargoPreventingItemUse(BattleParty battleParty)
    {
        BattlePartyContext context = battleParty.Context!;
        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(context.BattleSys!, context.Battler, context.PokemonPartySlots[context.SelectedPartyIndex]);
        GameString format = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, 0x5F)!;
        StringTemplate.StringTemplate_SetNickname(battleParty.StringTemplate!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
        StringTemplate.StringTemplate_SetMoveName(battleParty.StringTemplate!, 1, MoveEmbargo);
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, battleParty.String!, format);
        GameString.String_Free(format);
    }
}
