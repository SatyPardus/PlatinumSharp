namespace PlatinumSharp.Game;

public static partial class PartyMenus
{
    private const uint MemberWindowTemplates = 0x020F1F08;
    private const uint YesNoWindowTemplate = 0x020F1E98;
    private const uint CancelButtonWindowTemplate = 0x020F1ED0;
    private const uint ConfirmButtonWindowTemplate = 0x020F1ED8;
    private const uint ShortMessageWindowTemplate = 0x020F1EE0;
    private const uint MediumMessageWindowTemplate = 0x020F1EE8;
    private const uint LongMessageWindowTemplate = 0x020F1EF0;
    private const uint GiveItemWindowTemplate = 0x020F1EF8;
    private const uint MoveListWindowTemplate = 0x020F1F00;
    private const uint MemberNameMessages = 0x020F1EA0;
    private const int WinConfirmButton = 30;
    private const int WinCancelButton = 31;
    private const int WinNameOffset = 0;
    private const int WinLevelOffset = 1;
    private const int WinHPOffset = 2;
    private const int WinCommentOffset = 4;
    private const uint TextColorBlack = 0xF0E00;
    private const uint TextColorMale = 0x30400;
    private const uint TextColorFemale = 0x50600;
    private const int StatusNone = 7;
    private const int SeqLevelUp = 0x5E6;
    private const int FanfareLevelUp = 0x483;

    private static Window MemberWindow(PartyMenuApplication app, int slot, int offset)
    {
        return app.Windows[slot * WinsPerMember + offset];
    }

    // OFFSET 0x020819DC
    private static void AddPartyMemberWindows(BgConfig bgConfig, Window[] windows, int first, uint templates)
    {
        for (int i = 0; i < WinsPerMember; i++)
        {
            Bg.Window_AddFromTemplate(bgConfig, windows[first + i], RomStructs.Arm9WindowTemplate((uint)(templates + i * 8)));
        }
    }

    // OFFSET 0x02081A24
    private static void PartyMenu_AddWindows(PartyMenuApplication app)
    {
        BgConfig bgConfig = app.BgConfig!;
        AddPartyMemberWindows(bgConfig, app.Windows, 0, MemberWindowTemplates);
        AddPartyMemberWindows(bgConfig, app.Windows, 10, MemberWindowTemplates + 0x50);
        AddPartyMemberWindows(bgConfig, app.Windows, 20, MemberWindowTemplates + 0xA0);
        if (app.PartyMenu!.Type == TypeMultiBattle)
        {
            AddPartyMemberWindows(bgConfig, app.Windows, 5, MemberWindowTemplates + 0xF0);
            AddPartyMemberWindows(bgConfig, app.Windows, 15, MemberWindowTemplates + 0x118);
            AddPartyMemberWindows(bgConfig, app.Windows, 25, MemberWindowTemplates + 0x140);
        }
        else
        {
            AddPartyMemberWindows(bgConfig, app.Windows, 5, MemberWindowTemplates + 0x28);
            AddPartyMemberWindows(bgConfig, app.Windows, 15, MemberWindowTemplates + 0x78);
            AddPartyMemberWindows(bgConfig, app.Windows, 25, MemberWindowTemplates + 0xC8);
        }
        int mode = app.PartyMenu.Mode;
        if (mode == ModeSelectConfirm || mode == ModeBattleTower || mode == ModeBattleHall || mode == ModeBattleCastle)
        {
            WindowTemplate template = RomStructs.Arm9WindowTemplate(CancelButtonWindowTemplate);
            template.TilemapTop = 0x16;
            Bg.Window_AddFromTemplate(bgConfig, app.Windows[WinCancelButton], template);
        }
        else
        {
            Bg.Window_AddFromTemplate(bgConfig, app.Windows[WinCancelButton], RomStructs.Arm9WindowTemplate(CancelButtonWindowTemplate));
        }
        Bg.Window_AddFromTemplate(bgConfig, app.Windows[WinConfirmButton], RomStructs.Arm9WindowTemplate(ConfirmButtonWindowTemplate));
        Bg.Window_AddFromTemplate(bgConfig, app.Windows[WinShortMessage], RomStructs.Arm9WindowTemplate(ShortMessageWindowTemplate));
        Bg.Window_AddFromTemplate(bgConfig, app.Windows[WinMediumMessage], RomStructs.Arm9WindowTemplate(MediumMessageWindowTemplate));
        Bg.Window_AddFromTemplate(bgConfig, app.Windows[WinLongMessage], RomStructs.Arm9WindowTemplate(LongMessageWindowTemplate));
        Bg.Window_AddFromTemplate(bgConfig, app.Windows[WinGiveItemOrMail], RomStructs.Arm9WindowTemplate(GiveItemWindowTemplate));
        Bg.Window_AddFromTemplate(bgConfig, app.Windows[WinMoveList], RomStructs.Arm9WindowTemplate(MoveListWindowTemplate));
    }

    // OFFSET 0x02081B90
    private static void PartyMenu_RemoveWindows(PartyMenuApplication app)
    {
        for (int i = 0; i < PartyMenuApplication.NumWindows; i++)
        {
            Bg.Window_Remove(app.Windows[i]);
        }
        if (Bg.Window_IsInUse(app.MenuWindows[0]) == 1)
        {
            Bg.Window_Remove(app.MenuWindows[0]);
        }
    }

    // OFFSET 0x02081BC0
    private static void PartyMenu_LoadContextMenuStrings(PartyMenuApplication app)
    {
        uint[] messages = { 0x91, 0x92, 0x93, 0xA0, 0xA1, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9A, 0xA6, 0xC8 };
        for (int i = 0; i < messages.Length; i++)
        {
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, messages[i], app.MenuStrings[i]!);
        }
    }

    // OFFSET 0x02081CAC
    private static void PartyMenu_SetKnownFieldMove(PartyMenuApplication app, ushort move, int index)
    {
        GameString format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, (uint)(index + 0x9C))!;
        StringTemplate.StringTemplate_SetMoveName(app.Template!, 0, move);
        StringTemplate.StringTemplate_Format(app.Template!, app.MenuStrings[index + 0x10]!, format);
        GameString.String_Free(format);
    }

    // OFFSET 0x02081CF4
    private static void PartyMenu_DrawContextMenu(PartyMenuApplication app, byte[] entries, int count)
    {
        Bg.Window_Add(app.BgConfig!, app.MenuWindows[0], 0, 0x13, (byte)(0x17 - count * 2), 0xC, (byte)(count * 2), 0, 0x240);
        app.ContextMenuChoices = StringList.StringList_New((uint)count, HeapID);
        int fieldMove = 0;
        for (int i = 0; i < count; i++)
        {
            if (entries[i] >= 0x10)
            {
                StringList.StringList_AddFromString(app.ContextMenuChoices, app.MenuStrings[0x10 + fieldMove], PartyMenu_GetAction(entries[i]));
                fieldMove++;
            }
            else
            {
                StringList.StringList_AddFromString(app.ContextMenuChoices, app.MenuStrings[entries[i]], PartyMenu_GetAction(entries[i]));
            }
        }
        MenuTemplate template = new()
        {
            Choices = app.ContextMenuChoices,
            Window = app.MenuWindows[0],
            FontID = 0,
            XSize = 1,
            YSize = (byte)count,
            LineSpacing = 0,
            SuppressCursor = 0,
            LoopAround = count >= 4 ? 1 : 0,
        };
        RenderWindow.Window_DrawStandardFrame(app.MenuWindows[0], 1, 1, 0xE);
        app.ContextMenu = Menu.Menu_NewAndCopyToVRAM(template, 8, 0, 0, HeapID, KeyB);
    }

    // OFFSET 0x02081E08
    private static void PartyMenu_LoadContextMenuPrompt(PartyMenuApplication app)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        if (partyMenu.Mode == ModeBallSeal)
        {
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x2A, app.TmpString!);
            return;
        }
        if (partyMenu.Mode == ModeSelectEgg && app.PartyMembers[app.CurrPartySlot].IsEgg == 1)
        {
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0xC6, app.TmpString!);
            return;
        }
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
        GameString format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, 0x25)!;
        StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
        StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, format);
        GameString.String_Free(format);
    }

    // OFFSET 0x02081EAC
    private static void PartyMenu_PrintMemberHPSlash(PartyMenuApplication app, int slot)
    {
        FontSpecialCharsContext.FontSpecialChars_DrawPartyScreenLevelText(app.SpecialChars!, 0, MemberWindow(app, slot, WinHPOffset), 0x1C, 2);
    }

    // OFFSET 0x02081ED8
    private static void PartyMenu_SetMemberName(PartyMenuApplication app, Pokemon mon, int slot)
    {
        ushort messageID = RomData.Arm9U16((uint)(MemberNameMessages + slot * 8), 1)[0];
        GameString format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, messageID)!;
        StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
        StringTemplate.StringTemplate_Format(app.Template!, app.PartyMembers[slot].Name!, format);
        GameString.String_Free(format);
    }

    // OFFSET 0x02081F2C
    private static void PartyMenu_PrintMemberName(PartyMenuApplication app, int slot)
    {
        Window window = MemberWindow(app, slot, WinNameOffset);
        PartyMenuMember member = app.PartyMembers[slot];
        Text.Text_AddPrinterWithParamsAndColor(window, 0, member.Name, 0, 0, 0xFF, TextColorBlack, null);
        if (member.IsEgg == 1)
        {
            Bg.Window_ScheduleCopyToVRAM(window);
            return;
        }
        if (!member.HideGenderMarker)
        {
            if (member.Gender == 0)
            {
                MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x1B, app.TmpFormat!);
                Text.Text_AddPrinterWithParamsAndColor(window, 0, app.TmpFormat, 0x40, 0, 0xFF, TextColorMale, null);
            }
            else if (member.Gender == 1)
            {
                MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x1C, app.TmpFormat!);
                Text.Text_AddPrinterWithParamsAndColor(window, 0, app.TmpFormat, 0x40, 0, 0xFF, TextColorFemale, null);
            }
        }
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02081FFC
    private static void PartyMenu_PrintMemberLevel(PartyMenuApplication app, int slot)
    {
        Window window = MemberWindow(app, slot, WinLevelOffset);
        PartyMenuMember member = app.PartyMembers[slot];
        if (member.StatusIcon == StatusNone)
        {
            FontSpecialCharsContext.FontSpecialChars_DrawPartyScreenText(app.SpecialChars!, 1, member.Level, 3, 0, window, 5, 2);
        }
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02082058
    private static void PartyMenu_PrintMemberCurrentHP(PartyMenuApplication app, int slot)
    {
        Window window = MemberWindow(app, slot, WinHPOffset);
        FontSpecialCharsContext.FontSpecialChars_DrawPartyScreenHPText(app.SpecialChars!, app.PartyMembers[slot].CurHP, 3, 1, window, 4, 2);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02082098
    private static void PartyMenu_ClearMemberHP(PartyMenuApplication app, int slot)
    {
        Window window = MemberWindow(app, slot, WinHPOffset);
        Bg.Window_FillRectWithColor(window, 0, 4, 0, 0x18, 0x10);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x020820C4
    private static void PartyMenu_PrintMemberMaxHP(PartyMenuApplication app, int slot)
    {
        Window window = MemberWindow(app, slot, WinHPOffset);
        FontSpecialCharsContext.FontSpecialChars_DrawPartyScreenHPText(app.SpecialChars!, app.PartyMembers[slot].MaxHP, 3, 0, window, 0x24, 2);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02082104
    private static void PartyMenu_DrawMemberHealthbar(PartyMenuApplication app, int slot)
    {
        Window window = MemberWindow(app, slot, WinHealthbarOffset);
        PartyMenuMember member = app.PartyMembers[slot];
        ushort offset = (ushort)(((window.Palette << 4) + 9) * 2);
        switch (HealthBar.HealthBar_Color(member.CurHP, member.MaxHP, 0x30))
        {
            case HealthBar.ColorEmpty:
                Bg.Window_ScheduleCopyToVRAM(window);
                return;
            case HealthBar.ColorGreen:
            case HealthBar.ColorFull:
                Bg.Bg_LoadPalette(1, app.Colors, 9, 4, offset);
                break;
            case HealthBar.ColorYellow:
                Bg.Bg_LoadPalette(1, app.Colors, 0x19, 4, offset);
                break;
            case HealthBar.ColorRed:
                Bg.Bg_LoadPalette(1, app.Colors, 0x29, 4, offset);
                break;
        }
        int fill = HealthBar.HealthBar_Fill(member.CurHP, member.MaxHP, 0x30);
        Bg.Window_FillRectWithColor(window, 0xA, 0, 2, fill, 1);
        Bg.Window_FillRectWithColor(window, 9, 0, 3, fill, 2);
        Bg.Window_FillRectWithColor(window, 0xA, 0, 5, fill, 1);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x020821F8
    private static void PartyMenu_DrawMemberPanelData(PartyMenuApplication app, int slot)
    {
        for (int i = 0; i < 4; i++)
        {
            Bg.Window_FillTilemap(MemberWindow(app, slot, i), 0);
        }
        PartyMenu_PrintMemberName(app, slot);
        if (app.PartyMembers[slot].IsEgg == 1)
        {
            for (int i = 0; i < 4; i++)
            {
                Bg.Window_ScheduleCopyToVRAM(MemberWindow(app, slot, i));
            }
            return;
        }
        PartyMenu_PrintMemberHPSlash(app, slot);
        PartyMenu_PrintMemberLevel(app, slot);
        PartyMenu_PrintMemberCurrentHP(app, slot);
        PartyMenu_PrintMemberMaxHP(app, slot);
        PartyMenu_DrawMemberHealthbar(app, slot);
    }

    // OFFSET 0x020822BC
    public static void PartyMenu_LoadMemberWindowTiles(PartyMenuApplication app, int slot)
    {
        for (int i = 0; i < 4; i++)
        {
            Bg.Window_LoadTiles(MemberWindow(app, slot, i));
        }
    }

    // OFFSET 0x020822F4
    public static void PartyMenu_ClearMemberWindows(PartyMenuApplication app, int slot)
    {
        for (int i = 0; i < 4; i++)
        {
            Bg.Window_ClearAndScheduleCopyToVRAM(MemberWindow(app, slot, i));
        }
    }

    private static void PrintMemberNameAndLevelOnly(PartyMenuApplication app, int slot)
    {
        Bg.Window_FillTilemap(MemberWindow(app, slot, WinNameOffset), 0);
        Bg.Window_FillTilemap(MemberWindow(app, slot, WinLevelOffset), 0);
        PartyMenu_PrintMemberName(app, slot);
    }

    // OFFSET 0x0208232C
    private static void PrintMemberEvoComment(PartyMenuApplication app, int slot)
    {
        PrintMemberNameAndLevelOnly(app, slot);
        if (app.PartyMembers[slot].IsEgg == 1)
        {
            PartyMenu_PrintMemberComment_CanUseEvoItem(app, slot, 1);
            return;
        }
        PartyMenu_PrintMemberLevel(app, slot);
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, slot);
        if (Pokemon.Pokemon_GetEvolutionTargetSpecies(null, mon, EvoClassByItem, app.PartyMenu.UsedItemID, out _) == 0)
        {
            PartyMenu_PrintMemberComment_CanUseEvoItem(app, slot, 1);
        }
        else
        {
            PartyMenu_PrintMemberComment_CanUseEvoItem(app, slot, 0);
        }
    }

    // OFFSET 0x020823C4
    private static void PrintMemberMoveComment(PartyMenuApplication app, int slot)
    {
        PrintMemberNameAndLevelOnly(app, slot);
        if (app.PartyMembers[slot].IsEgg == 1)
        {
            PartyMenu_PrintMemberComment_CanLearnMove(app, slot, 1);
            return;
        }
        PartyMenu_PrintMemberLevel(app, slot);
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, slot);
        byte result = PartyMenu_TeachMove_Check(app, mon);
        if (result == 0xFF)
        {
            PartyMenu_PrintMemberComment_CanLearnMove(app, slot, 1);
        }
        else if (result == 0xFD)
        {
            PartyMenu_PrintMemberComment_CanLearnMove(app, slot, 2);
        }
        else
        {
            PartyMenu_PrintMemberComment_CanLearnMove(app, slot, 0);
        }
    }

    // OFFSET 0x0208245C
    private static void PrintMemberContestComment(PartyMenuApplication app, int slot)
    {
        PrintMemberNameAndLevelOnly(app, slot);
        if (app.PartyMembers[slot].IsEgg != 1)
        {
            PartyMenu_PrintMemberLevel(app, slot);
        }
        PartyMenu_PrintMemberComment_IsContestEligible(app, slot, app.PartyMembers[slot].IsContestEligible ? 1 : 0);
    }

    // OFFSET 0x020824C0
    private static void PartyMenu_PrintMemberNameAndLevel(PartyMenuApplication app, int slot)
    {
        PrintMemberNameAndLevelOnly(app, slot);
        if (app.PartyMembers[slot].IsEgg != 1)
        {
            PartyMenu_PrintMemberLevel(app, slot);
        }
    }

    // OFFSET 0x02082508
    public static void PrintMemberSelectionComment(PartyMenuApplication app, int slot)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        int count = partyMenu.SelectionSlots >> 4;
        for (int i = 0; i < count; i++)
        {
            if (slot + 1 == partyMenu.SelectionOrder[i])
            {
                PartyMenu_PrintSelectionEligibility(app, slot, i);
                return;
            }
        }
        int eligibility;
        if (partyMenu.Mode == ModeBattleHall)
        {
            eligibility = PartyMenu_CheckBattleHallEligibility(app, slot);
        }
        else if (partyMenu.Mode == ModeBattleCastle)
        {
            eligibility = PartyMenu_CheckBattleCastleEligibility(app, slot);
        }
        else
        {
            eligibility = PartyMenu_CheckEligibility(app, slot);
        }
        PartyMenu_PrintSelectionEligibility(app, slot, eligibility == 0 ? 8 : 7);
    }

    // OFFSET 0x020825A4
    private static int CalcCenterXPos(int font, GameString text, int width)
    {
        return (int)((uint)(width - Font.Font_CalcStringWidth(font, text, 0)) >> 1);
    }

    private static void PrintButtonLabel(PartyMenuApplication app, int windowIndex, uint message)
    {
        Window window = app.Windows[windowIndex];
        MessageLoader.MessageLoader_GetString(app.MessageLoader!, message, app.TmpFormat!);
        int x = CalcCenterXPos(0, app.TmpFormat!, window.Width * 8);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, app.TmpFormat, (uint)x, 0, 0xFF, TextColorBlack, null);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x020825B4
    private static void PartyMenu_PrintButtonText(PartyMenuApplication app, byte buttons)
    {
        Bg.Window_FillTilemap(app.Windows[WinConfirmButton], 0);
        Bg.Window_FillTilemap(app.Windows[WinCancelButton], 0);
        if ((buttons & 1) != 0)
        {
            PrintButtonLabel(app, WinConfirmButton, 0);
        }
        if ((buttons & 2) != 0)
        {
            PrintButtonLabel(app, WinCancelButton, 1);
        }
    }

    // OFFSET 0x02082680
    private static void PrintPartyMenuMessage(PartyMenuApplication app, Window window, uint message, int drawFrame)
    {
        if (drawFrame == 1)
        {
            RenderWindow.Window_DrawMessageBoxWithScrollCursor(window, 1, 0xA, 0xF);
        }
        Bg.Window_FillTilemap(window, 0xF);
        if (message != 0xFFFFFFFF)
        {
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, message, app.TmpString!);
        }
        Text.Text_AddPrinterWithParams(window, 1, app.TmpString, 0, 0, 0xFF, null);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x020826E0
    public static void PartyMenu_PrintShortMessage(PartyMenuApplication app, uint message, int drawFrame)
    {
        PrintPartyMenuMessage(app, app.Windows[WinShortMessage], message, drawFrame);
    }

    // OFFSET 0x020826F4
    public static void PartyMenu_PrintMediumMessage(PartyMenuApplication app, uint message, int drawFrame)
    {
        PrintPartyMenuMessage(app, app.Windows[WinMediumMessage], message, drawFrame);
    }

    // OFFSET 0x02082708
    public static void PartyMenu_PrintLongMessage(PartyMenuApplication app, uint message, int drawFrame)
    {
        Window window = app.Windows[WinLongMessage];
        if (drawFrame == 1)
        {
            RenderWindow.Window_DrawMessageBoxWithScrollCursor(window, 1, 0xA, 0xF);
        }
        Bg.Window_FillTilemap(window, 0xF);
        if (message != 0xFFFFFFFF)
        {
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, message, app.TmpString!);
        }
        PartyMenu_AddLongMessagePrinter(app);
    }

    // OFFSET 0x0208274C
    public static void PartyMenu_AddLongMessagePrinter(PartyMenuApplication app)
    {
        RenderText.RenderControlFlags_SetCanABSpeedUpPrint(1);
        RenderText.RenderControlFlags_SetAutoScrollFlags(0);
        app.TextPrinterID = (byte)Text.Text_AddPrinterWithParams(app.Windows[WinLongMessage], 1, app.TmpString, 0, 0, Options.Options_TextFrameDelay(app.PartyMenu!.Options!), TextPrinter_HandleSound);
    }

    // OFFSET 0x0208279C
    private static bool TextPrinter_HandleSound(TextPrinter printer, ushort argument)
    {
        switch (argument)
        {
            case 1:
                return SoundPlayback.Sound_IsAnyEffectPlaying();
            case 2:
                return SoundPlayback.Sound_IsBGMPausedByFanfare() != 0;
            case 3:
                SoundPlayback.Sound_PlayEffect(SeqLevelUp);
                break;
            case 4:
                SoundPlayback.Sound_PlayFanfare(FanfareLevelUp);
                break;
            case 5:
                return SoundPlayback.Sound_IsEffectPlaying(SeqLevelUp) != 0;
        }
        return false;
    }

    // OFFSET 0x020827EC
    public static void PartyMenu_DrawYesNoChoice(PartyMenuApplication app)
    {
        app.ContextMenu = Menu.Menu_MakeYesNoChoice(app.BgConfig!, RomStructs.Arm9WindowTemplate(YesNoWindowTemplate), 1, 0xE, HeapID);
    }

    private static void PrintMemberComment(PartyMenuApplication app, int slot, uint message)
    {
        Window window = MemberWindow(app, slot, WinCommentOffset);
        Bg.Window_FillTilemap(window, 0);
        GameString text = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, message)!;
        Text.Text_AddPrinterWithParamsAndColor(window, 0, text, 0, 0, 0xFF, TextColorBlack, null);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02082810
    private static void PartyMenu_PrintMemberComment_CanUseEvoItem(PartyMenuApplication app, int slot, int unable)
    {
        PrintMemberComment(app, slot, unable == 0 ? 0xB2u : 0xB3u);
    }

    // OFFSET 0x02082880
    private static void PartyMenu_PrintMemberComment_CanLearnMove(PartyMenuApplication app, int slot, int state)
    {
        PrintMemberComment(app, slot, state switch { 0 => 0xAFu, 1 => 0xB0u, _ => 0xB1u });
    }

    // OFFSET 0x02082900
    private static void PartyMenu_PrintMemberComment_IsContestEligible(PartyMenuApplication app, int slot, int eligible)
    {
        PrintMemberComment(app, slot, eligible == 0 ? 0xB5u : 0xB4u);
    }

    // OFFSET 0x02082964
    private static void PartyMenu_PrintSelectionEligibility(PartyMenuApplication app, int slot, int order)
    {
        uint message;
        if (order < MaxPartySize)
        {
            message = (uint)(order + 0xA9);
        }
        else if (order == 7)
        {
            message = 0xA7;
        }
        else
        {
            message = 0xA8;
        }
        PrintMemberComment(app, slot, message);
    }

    // OFFSET 0x020829DC
    public static void PartyMenu_DrawLevelUpStatIncreases(PartyMenuApplication app)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, app.CurrPartySlot);
        int[] statParams = { 0xA4, 0xA5, 0xA6, 0xA8, 0xA9, 0xA7 };
        ushort[] stats = new ushort[6];
        for (int i = 0; i < 6; i++)
        {
            stats[i] = (ushort)Pokemon.Pokemon_GetValue(mon, statParams[i], null);
        }
        Window window = app.MenuWindows[0];
        Bg.Window_Add(app.BgConfig!, window, 0, 1, 1, 0xE, 0xC, 0, 0x240);
        RenderWindow.Window_DrawStandardFrame(window, 1, 1, 0xE);
        Bg.Window_FillTilemap(window, 0xF);
        int y = 0;
        for (int i = 0; i < 6; i++)
        {
            GameString label = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, (uint)(i + 0xB9))!;
            Text.Text_AddPrinterWithParams(window, 0, label, 0, y, 0xFF, null);
            GameString.String_Free(label);
            GameString format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, 0xBF)!;
            StringTemplate.StringTemplate_SetNumber(app.Template!, 0, stats[i] - app.MonStats[i], 2, 0, 1);
            StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, format);
            GameString.String_Free(format);
            int x = 0x68 - Font.Font_CalcStringWidth(0, app.TmpString, 0);
            Text.Text_AddPrinterWithParams(window, 0, app.TmpString, x, y, 0xFF, null);
            app.MonStats[i] = stats[i];
            y += 0x10;
        }
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02082B58
    public static void PartyMenu_DrawLevelUpNewStatValues(PartyMenuApplication app)
    {
        Window window = app.MenuWindows[0];
        Bg.Window_FillRectWithColor(window, 0xF, 0x50, 0, 0x20, 0x70);
        GameString format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, 0xC0)!;
        int y = 0;
        for (int i = 0; i < 6; i++)
        {
            StringTemplate.StringTemplate_SetNumber(app.Template!, 0, app.MonStats[i], 3, 0, 1);
            StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, format);
            int width = Font.Font_CalcStringWidth(0, app.TmpString, 0);
            Text.Text_AddPrinterWithParams(window, 0, app.TmpString, 0x68 - width, y, 0xFF, null);
            y += 0x10;
        }
        GameString.String_Free(format);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x02082C10
    public static void PartyMenu_RemoveContextWindow(PartyMenuApplication app)
    {
        RenderWindow.Window_EraseStandardFrame(app.MenuWindows[0], 0);
        Bg.Window_Remove(app.MenuWindows[0]);
    }
}
