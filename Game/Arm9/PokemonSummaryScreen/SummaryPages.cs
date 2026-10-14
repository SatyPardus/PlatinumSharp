namespace PlatinumSharp.Game;

public static partial class PokemonSummaryScreenApp
{
    public const int SpriteTabInfo = 0;
    public const int SpriteCaughtBall = 8;
    public const int SpriteMoveSelector1 = 9;
    public const int SpriteMonTypeIcon1 = 11;
    public const int SpriteMonTypeIcon2 = 12;
    public const int SpriteMoveTypeIcon1 = 13;
    public const int SpriteMonIcon = 19;
    public const int SpriteStatusIcon = 20;
    public const int SpritePageArrowLeft = 21;
    public const int SpritePageArrowRight = 22;
    public const int SpriteMarkingCircle = 23;
    public const int SpriteSheen1 = 29;
    public const int SpriteButtonTapCircle = 41;
    public const int SpriteConditionArrowCool = 42;
    public const int SpriteAButton = 47;
    public const int SpriteContestStatDotCool = 48;
    public const int SpriteShinyIcon = 53;
    public const int SpritePokerusCuredIcon = 54;
    public const int SpriteRibbon1 = 55;
    public const int SpriteRibbon9 = 63;
    public const int SpriteRibbonCursor = 67;
    public const int SpriteRibbonArrowUp = 68;
    public const int SpriteRibbonArrowDown = 69;
    public const int SpriteRibbonFlash = 70;
    public const int SpriteConditionFlashCool = 71;
    public const int SpritePokerusIcon = 76;

    public const int WindowLabelItem = 6;
    public const int WindowLabelRibbonCount = 30;
    public const int WindowItemName = 32;
    public const int WindowMonLevel = 33;
    public const int WindowButtonPrompt = 35;

    private const int NoMove = -1;
    private const int SeqPageChange = 0x5E1;
    private const int HeartTileEmpty = 0x12E;
    private const int HeartTileFull = 0x12C;
    private const int MoveParamContestEffect = 0xA;
    private const int RibbonsPerRow = 4;

    private static Window StaticWindow(PokemonSummaryScreen summaryScreen, int index)
    {
        return summaryScreen.StaticWindows[index];
    }

    private static void SetSpriteDraw(PokemonSummaryScreen summaryScreen, int sprite, int draw)
    {
        Sprite.Sprite_SetDrawFlag(summaryScreen.Sprites[sprite]!, draw);
    }

    private static void SetSpriteDraw2(PokemonSummaryScreen summaryScreen, int sprite, int draw)
    {
        SpriteSystem.Sprite_SetDrawFlag2(summaryScreen.Sprites[sprite]!, draw);
    }

    // OFFSET 0x0208D720
    public static void PokemonSummaryScreen_FlagVisiblePages(PokemonSummary summary, byte[] pages)
    {
        summary.PageFlags = 0;
        for (int i = 0; i < PageMax; i++)
        {
            byte page = pages[i];
            if (page == PageMax)
            {
                break;
            }
            summary.PageFlags |= (byte)(1 << page);
        }
    }

    // OFFSET 0x0208D748
    private static void TryHideContestPages(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummary data = summaryScreen.Data!;
        if (data.ShowContest == 1)
        {
            return;
        }
        if ((data.PageFlags & 0x10) != 0)
        {
            data.PageFlags ^= 0x10;
        }
        if ((data.PageFlags & 0x20) != 0)
        {
            data.PageFlags ^= 0x20;
        }
        if ((data.PageFlags & 0x40) != 0)
        {
            data.PageFlags ^= 0x40;
        }
    }

    // OFFSET 0x0208D790
    public static byte PokemonSummaryScreen_PageIsVisble(PokemonSummaryScreen summaryScreen, int page)
    {
        if (summaryScreen.MonData.IsEgg != 0 && page != PageMemo && page != PageExit)
        {
            return 0;
        }
        return (byte)((1 << page) & summaryScreen.Data!.PageFlags);
    }

    // OFFSET 0x0208D7BC
    public static byte PokemonSummaryScreen_CountVisiblePages(PokemonSummaryScreen summaryScreen)
    {
        byte count = 0;
        for (int i = 0; i < PageMax; i++)
        {
            if (((1 << i) & summaryScreen.Data!.PageFlags) != 0)
            {
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x0208D7EC
    private static void SetupPageFromSubscreenButton(PokemonSummaryScreen summaryScreen, byte page)
    {
        if (summaryScreen.Page == page)
        {
            return;
        }
        if (summaryScreen.Data!.Mode == ModeSelectMove)
        {
            ClearMoveAttributeWindows(summaryScreen);
        }
        PokemonSummaryScreen_RemoveExtraWindows(summaryScreen);
        summaryScreen.Page = (sbyte)page;
        PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, null);
        PokemonSummaryScreen_UpdatePageTabSprites(summaryScreen);
        PokemonSummaryScreen_SetPageArrowsPos(summaryScreen);
        PokemonSummaryScreen_UpdateTypeIcons(summaryScreen);
        PokemonSummaryScreen_UpdateRibbonSprites(summaryScreen);
        PokemonSummaryScreen_InitSheenSprites(summaryScreen);
        PokemonSummaryScreen_AddExtraWindows(summaryScreen);
        PokemonSummaryScreen_UpdateConditionFlashSprites(summaryScreen, 0);
        Bg.Bg_FillTilemapRect(summaryScreen.BgConfig!, 1, 0, 0xE, 4, 0x13, 0x14, 0);
        Bg.Bg_CopyTilemapBufferToVRAM(summaryScreen.BgConfig!, 1);
        PokemonSummaryScreen_DrawExtraWindows(summaryScreen);
        LoadCurrentPageTilemap(summaryScreen);
        PokemonSummaryScreen_InitConditionRects(summaryScreen);
        if (summaryScreen.Data.Mode == ModeSelectMove)
        {
            RedrawMoveSelectPage(summaryScreen);
        }
    }

    // OFFSET 0x0208D898
    private static void ChangePage(PokemonSummaryScreen summaryScreen, int delta)
    {
        sbyte page = summaryScreen.Page;
        bool isEgg = summaryScreen.MonData.IsEgg != 0;
        while (true)
        {
            page = (sbyte)(page + delta);
            if (page < 0)
            {
                page = PageExit;
            }
            else if (page > PageExit)
            {
                page = 0;
            }
            bool visible = ((1 << page) & summaryScreen.Data!.PageFlags) != 0;
            if (!isEgg)
            {
                if (visible)
                {
                    break;
                }
            }
            else if (visible && (page == PageMemo || page == PageExit))
            {
                break;
            }
        }
        if (page == summaryScreen.Page)
        {
            return;
        }
        SoundPlayback.Sound_PlayEffect(SeqPageChange);
        PokemonSummaryScreen_UpdateSubscreenButtonGfx(summaryScreen);
        PokemonSummaryScreen_UpdateConditionFlashSprites(summaryScreen, 0);
        SetupPageFromSubscreenButton(summaryScreen, (byte)page);
    }

    // OFFSET 0x0208D920
    private static byte CheckSubscreenPressAndSetButton(PokemonSummaryScreen summaryScreen)
    {
        byte button = PokemonSummaryScreen_CheckSubscreenButtonPress(summaryScreen);
        if (button == 0xFF)
        {
            return 0;
        }
        summaryScreen.ButtonAnimFrame = 0;
        summaryScreen.ButtonCurrent = button;
        summaryScreen.ButtonState = 0;
        return 1;
    }

    // OFFSET 0x0208D948
    private static void LoadCurrentPageTilemap(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummary data = summaryScreen.Data!;
        int member;
        if (data.Mode == ModeSelectMove && data.Move != 0)
        {
            member = summaryScreen.Page == PageBattleMoves ? 0xC : 0xD;
        }
        else
        {
            member = summaryScreen.Page + 3;
        }
        byte[] buffer = NARC.NARC_AllocAndReadWholeMemberByIndexPair(SummaryArchive, member, HeapIDSummary);
        G2d.GetUnpackedScreenData(buffer, out ScreenData? screenData);
        Bg.Bg_LoadToTilemapRect(summaryScreen.BgConfig!, 3, screenData!.RawData, 0, 0, 0x20, 0x20);
        Bg.Bg_ScheduleTilemapTransfer(summaryScreen.BgConfig!, 3);
        Heap.Heap_Free(buffer);
        if (summaryScreen.Page == PageInfo)
        {
            DrawExperienceProgressBar(summaryScreen);
        }
        else if (summaryScreen.Page == PageSkills)
        {
            DrawHealthBar(summaryScreen);
        }
    }

    // OFFSET 0x0208D9D0
    private static void DrawHealthBar(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummaryMonData monData = summaryScreen.MonData;
        int tileBase = 0;
        switch (HealthBar.HealthBar_Color(monData.CurHP, monData.MaxHP, 0x30))
        {
            case 0:
            case 3:
            case 4:
                tileBase = 0xA0C0;
                break;
            case 1:
                tileBase = 0xA100;
                break;
            case 2:
                tileBase = 0xA0E0;
                break;
        }
        byte fill = HealthBar.HealthBar_Fill(monData.CurHP, monData.MaxHP, 0x30);
        ushort fullTile = (ushort)(tileBase + 8);
        for (int i = 0; i < 6; i++)
        {
            ushort tile = fill >= 8 ? fullTile : (ushort)(tileBase + fill);
            Bg.Bg_FillTilemapRect(summaryScreen.BgConfig!, 3, tile, (byte)(i + 0x18), 6, 1, 1, 0x11);
            fill = fill >= 8 ? (byte)(fill - 8) : (byte)0;
        }
        Bg.Bg_ScheduleTilemapTransfer(summaryScreen.BgConfig!, 3);
    }

    // OFFSET 0x0208DA84
    private static void DrawExperienceProgressBar(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummaryMonData monData = summaryScreen.MonData;
        byte fill;
        if (monData.Level < MaxLevel)
        {
            fill = HealthBar.HealthBar_Fill(monData.CurExp - monData.CurLevelExp, monData.NextLevelExp - monData.CurLevelExp, 0x38);
        }
        else
        {
            fill = HealthBar.HealthBar_Fill(0, 0, 0x38);
        }
        for (int i = 0; i < 7; i++)
        {
            ushort tile = fill >= 8 ? (ushort)0xB4 : (ushort)(fill + 0xAC);
            Bg.Bg_FillTilemapRect(summaryScreen.BgConfig!, 3, tile, (byte)(i + 0x17), 0x17, 1, 1, 0x11);
            fill = fill >= 8 ? (byte)(fill - 8) : (byte)0;
        }
        Bg.Bg_ScheduleTilemapTransfer(summaryScreen.BgConfig!, 3);
    }

    // OFFSET 0x0208DB10
    private static void ChangeSummaryMon(PokemonSummaryScreen summaryScreen, int delta)
    {
        sbyte index = TryAdvanceSummaryMonIndex(summaryScreen, delta);
        if (index == -1)
        {
            return;
        }
        summaryScreen.Data!.MonIndex = (byte)index;
        SetMonData(summaryScreen);
        PlayMonCry(summaryScreen);
        PokemonSummaryScreen_PrintNicknameAndGender(summaryScreen);
        PokemonSummaryScreen_PrintLevel(summaryScreen);
        PokemonSummaryScreen_PrintItemName(summaryScreen);
        if (summaryScreen.Page == PageInfo)
        {
            DrawExperienceProgressBar(summaryScreen);
        }
        else if (summaryScreen.Page == PageSkills)
        {
            DrawHealthBar(summaryScreen);
        }
        else if (summaryScreen.Page == PageCondition)
        {
            PokemonSummaryScreen_InitMaxAndDeltaConditionRects(summaryScreen);
        }
        PokemonSummaryScreen_ChangeMonSprite(summaryScreen);
        PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, null);
        PokemonSummaryScreen_UpdatePageTabSprites(summaryScreen);
        PokemonSummaryScreen_SetPageArrowsPos(summaryScreen);
        PokemonSummaryScreen_SetMonTypeIcons(summaryScreen);
        PokemonSummaryScreen_UpdateTypeIcons(summaryScreen);
        PokemonSummaryScreen_UpdateRibbonSprites(summaryScreen);
        PokemonSummaryScreen_SetMonIcon(summaryScreen);
        PokemonSummaryScreen_SetCaughtBallGfx(summaryScreen);
        PokemonSummaryScreen_UpdateStatusIcon(summaryScreen);
        PokemonSummaryScreen_InitSheenSprites(summaryScreen);
        PokemonSummaryScreen_UpdateMiscMonDataSprites(summaryScreen);
        PokemonSummaryScreen_DrawExtraWindows(summaryScreen);
    }

    // OFFSET 0x0208DBC4
    private static sbyte TryAdvanceSummaryMonIndex(PokemonSummaryScreen summaryScreen, int delta)
    {
        switch (summaryScreen.Data!.DataType)
        {
            case 0:
                return TryAdvanceMonIndex(summaryScreen, delta);
            case 1:
                return TryAdvancePartyMonIndex(summaryScreen, delta);
            case 2:
                return TryAdvanceBoxMonIndex(summaryScreen, delta);
        }
        return -1;
    }

    // OFFSET 0x0208DBF4
    private static byte CanAdvanceToEgg(PokemonSummaryScreen summaryScreen)
    {
        if (summaryScreen.Data!.Mode == ModePoffin)
        {
            return 0;
        }
        if (summaryScreen.Page == PageMemo || summaryScreen.Page == PageExit)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0208DC1C
    private static sbyte TryAdvanceMonIndex(PokemonSummaryScreen summaryScreen, int delta)
    {
        PokemonSummary data = summaryScreen.Data!;
        sbyte index = (sbyte)data.MonIndex;
        while (true)
        {
            index = (sbyte)(index + delta);
            if (index < 0 || index >= data.MonMax)
            {
                return -1;
            }
            Pokemon mon = ((Pokemon[])data.MonData!)[index];
            if (Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null) == 0)
            {
                continue;
            }
            if (Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) == 0 || CanAdvanceToEgg(summaryScreen) == 1)
            {
                return index;
            }
        }
    }

    // OFFSET 0x0208DC84
    private static sbyte TryAdvancePartyMonIndex(PokemonSummaryScreen summaryScreen, int delta)
    {
        PokemonSummary data = summaryScreen.Data!;
        sbyte index = (sbyte)data.MonIndex;
        while (true)
        {
            index = (sbyte)(index + delta);
            if (index < 0 || index >= data.MonMax)
            {
                return -1;
            }
            Pokemon mon = Party.Party_GetPokemonBySlotIndex((Party)data.MonData!, index);
            if (Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null) == 0)
            {
                continue;
            }
            if (Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) == 0 || CanAdvanceToEgg(summaryScreen) == 1)
            {
                return index;
            }
        }
    }

    // OFFSET 0x0208DCE0
    private static sbyte TryAdvanceBoxMonIndex(PokemonSummaryScreen summaryScreen, int delta)
    {
        PokemonSummary data = summaryScreen.Data!;
        sbyte index = (sbyte)data.MonIndex;
        while (true)
        {
            index = (sbyte)(index + delta);
            if (index < 0 || index >= data.MonMax)
            {
                return -1;
            }
            BoxPokemon boxMon = ((BoxPokemon[])data.MonData!)[index];
            if (Pokemon.BoxPokemon_GetValue(boxMon, MonDataSpecies, null) == 0)
            {
                continue;
            }
            if (Pokemon.BoxPokemon_GetValue(boxMon, MonDataIsEgg, null) == 0 || CanAdvanceToEgg(summaryScreen) == 1)
            {
                return index;
            }
        }
    }

    // OFFSET 0x0208DD48
    public static object PokemonSummaryScreen_MonData(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummary data = summaryScreen.Data!;
        switch (data.DataType)
        {
            case 0:
                return ((Pokemon[])data.MonData!)[data.MonIndex];
            case 1:
                return Party.Party_GetPokemonBySlotIndex((Party)data.MonData!, data.MonIndex);
            case 2:
                return ((BoxPokemon[])data.MonData!)[data.MonIndex];
        }
        return null!;
    }

    private static void ClearMoveInfoWindows(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowMonLevel));
        Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelItem));
        Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowItemName));
    }

    private static void ShowMoveInfoWindows(PokemonSummaryScreen summaryScreen)
    {
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowMonLevel));
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelItem));
        Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowItemName));
    }

    private static void ShowMonTypeIcons(PokemonSummaryScreen summaryScreen)
    {
        SetSpriteDraw2(summaryScreen, SpriteMonTypeIcon1, 1);
        if (summaryScreen.MonData.Type1 != summaryScreen.MonData.Type2)
        {
            SetSpriteDraw2(summaryScreen, SpriteMonTypeIcon2, 1);
        }
    }

    private static bool ScrollMoveInfoIn(PokemonSummaryScreen summaryScreen)
    {
        if (Bg.Bg_GetXOffset(summaryScreen.BgConfig!, 2) <= 0x40)
        {
            Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 0, 0);
            return true;
        }
        Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 2, 0x40);
        return false;
    }

    private static bool ScrollMoveInfoOut(PokemonSummaryScreen summaryScreen)
    {
        if (Bg.Bg_GetXOffset(summaryScreen.BgConfig!, 2) >= 0x80)
        {
            Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 0, 0x88);
            return true;
        }
        Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 1, 0x40);
        return false;
    }

    private static void ShowButtonPrompt(PokemonSummaryScreen summaryScreen, uint entry)
    {
        PokemonSummaryScreen_ClearAndPrintButtonPrompt(summaryScreen, entry);
        PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, StaticWindow(summaryScreen, WindowButtonPrompt));
    }

    // OFFSET 0x0208DD8C
    private static byte SetupBattleMoveInfo(PokemonSummaryScreen summaryScreen)
    {
        switch (summaryScreen.PageState)
        {
            case 0:
                Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 0, 0x88);
                Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 3, 0);
                PokemonSummaryScreen_UpdatePageArrows(summaryScreen, 0);
                ClearMoveInfoWindows(summaryScreen);
                Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowButtonPrompt));
                PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, null);
                summaryScreen.Cursor = 0;
                summaryScreen.PageState = 1;
                break;
            case 1:
                if (ScrollMoveInfoIn(summaryScreen))
                {
                    summaryScreen.PageState = 2;
                }
                break;
            case 2:
                PokemonSummaryScreen_ShowMove5OrCancel(summaryScreen);
                if (summaryScreen.Data!.Mode != ModeLock)
                {
                    ShowButtonPrompt(summaryScreen, 0x98);
                }
                UpdateMoveAttributes(summaryScreen);
                SetSpriteDraw2(summaryScreen, SpriteMonTypeIcon1, 1);
                PokemonSummaryScreen_ShowMonIcon(summaryScreen);
                if (summaryScreen.MonData.Type1 != summaryScreen.MonData.Type2)
                {
                    SetSpriteDraw2(summaryScreen, SpriteMonTypeIcon2, 1);
                }
                SetSpriteDraw(summaryScreen, SpriteMoveSelector1, 1);
                return 1;
        }
        return 0;
    }

    // OFFSET 0x0208DEA4
    private static byte HideBattleMoveInfo(PokemonSummaryScreen summaryScreen)
    {
        switch (summaryScreen.PageState)
        {
            case 0:
                SetSpriteDraw2(summaryScreen, SpriteMonTypeIcon1, 0);
                SetSpriteDraw2(summaryScreen, SpriteMonTypeIcon2, 0);
                SetSpriteDraw2(summaryScreen, SpriteMoveCategory, 0);
                SetSpriteDraw2(summaryScreen, SpriteMonIcon, 0);
                SetSpriteDraw(summaryScreen, SpriteMoveSelector1, 0);
                Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowButtonPrompt));
                PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, null);
                PokemonSummaryScreen_HideMoveCancelText(summaryScreen);
                PokemonSummaryScreen_ClearBattleAttributeWindows(summaryScreen);
                summaryScreen.PageState = 1;
                break;
            case 1:
                if (ScrollMoveInfoOut(summaryScreen))
                {
                    summaryScreen.PageState = 2;
                }
                break;
            case 2:
                ShowMoveInfoWindows(summaryScreen);
                PokemonSummaryScreen_ClearAndPrintButtonPrompt(summaryScreen, 0x81);
                PokemonSummaryScreen_UpdatePageArrows(summaryScreen, 1);
                PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, StaticWindow(summaryScreen, WindowButtonPrompt));
                return 1;
        }
        return 0;
    }

    // OFFSET 0x0208DF94
    private static byte TryChangeSelectedMove(PokemonSummaryScreen summaryScreen, int delta)
    {
        byte current = summaryScreen.Cursor;
        sbyte slot = (sbyte)current;
        while (true)
        {
            slot = (sbyte)(slot + delta);
            if (slot < 0)
            {
                slot = 4;
            }
            else if (slot == 5)
            {
                slot = 0;
            }
            if (slot == 4 || summaryScreen.MonData.Moves[slot] != 0)
            {
                break;
            }
        }
        if (slot == current)
        {
            return 0;
        }
        summaryScreen.Cursor = (byte)(slot & 0xF);
        return 1;
    }

    // OFFSET 0x0208DFF4
    private static void UpdateMoveAttributes(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummaryScreen_UpdateMoveSelectorPos(summaryScreen);
        int move;
        if (summaryScreen.Cursor == 4)
        {
            ushort learnMove = summaryScreen.Data!.Move;
            move = learnMove != 0 ? learnMove : NoMove;
        }
        else
        {
            move = summaryScreen.MonData.Moves[summaryScreen.Cursor];
        }
        if (summaryScreen.Page == PageBattleMoves)
        {
            UpdateBattleMoveAttributes(summaryScreen, move);
        }
        else
        {
            UpdateContestMoveAttributes(summaryScreen, move);
        }
    }

    // OFFSET 0x0208E07C
    private static void UpdateBattleMoveAttributes(PokemonSummaryScreen summaryScreen, int move)
    {
        if (move == NoMove)
        {
            PokemonSummaryScreen_ClearBattleAttributeWindows(summaryScreen);
            SetSpriteDraw2(summaryScreen, SpriteMoveCategory, 0);
            return;
        }
        PokemonSummaryScreen_PrintBattleMoveAttributes(summaryScreen, (uint)move);
        PokemonSummaryScreen_UpdateMoveCategoryIcon(summaryScreen, (uint)move);
        SetSpriteDraw2(summaryScreen, SpriteMoveCategory, 1);
    }

    // OFFSET 0x0208E0B8
    private static void UpdateContestMoveAttributes(PokemonSummaryScreen summaryScreen, int move)
    {
        UpdateAppealHearts(summaryScreen, move);
        if (move == NoMove)
        {
            PokemonSummaryScreen_ClearContestAttributeWindows(summaryScreen);
            return;
        }
        PokemonSummaryScreen_PrintContestMoveAttributes(summaryScreen, (uint)move);
    }

    // OFFSET 0x0208E0DC
    private static void SwapSelectedMoves(PokemonSummaryScreen summaryScreen)
    {
        object mon = PokemonSummaryScreen_MonData(summaryScreen);
        int a = summaryScreen.Cursor;
        int b = summaryScreen.CursorTmp;
        if (summaryScreen.Data!.DataType == DataTypeBoxMon)
        {
            Pokemon.BoxPokemon_SwapMoveSlots((BoxPokemon)mon, a, b);
        }
        else
        {
            Pokemon.Pokemon_SwapMoveSlots((Pokemon)mon, a, b);
        }
        PokemonSummaryMonData monData = summaryScreen.MonData;
        (monData.Moves[a], monData.Moves[b]) = (monData.Moves[b], monData.Moves[a]);
        (monData.CurPP[a], monData.CurPP[b]) = (monData.CurPP[b], monData.CurPP[a]);
        (monData.MaxPP[a], monData.MaxPP[b]) = (monData.MaxPP[b], monData.MaxPP[a]);
    }

    // OFFSET 0x0208E190
    private static void SetupMoveInfoNoTransition(PokemonSummaryScreen summaryScreen)
    {
        ClearMoveInfoWindows(summaryScreen);
        Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 0, 0);
        PokemonSummaryScreen_ShowMove5OrCancel(summaryScreen);
        UpdateMoveAttributes(summaryScreen);
        SetSpriteDraw2(summaryScreen, SpriteMonTypeIcon1, 1);
        PokemonSummaryScreen_ShowMonIcon(summaryScreen);
        if (summaryScreen.MonData.Type1 != summaryScreen.MonData.Type2)
        {
            SetSpriteDraw2(summaryScreen, SpriteMonTypeIcon2, 1);
        }
        SetSpriteDraw(summaryScreen, SpriteMoveSelector1, 1);
    }

    // OFFSET 0x0208E208
    private static byte SetupContestMoveInfo(PokemonSummaryScreen summaryScreen)
    {
        switch (summaryScreen.PageState)
        {
            case 0:
                Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 0, 0x88);
                Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 3, 0x100);
                UpdateAppealHearts(summaryScreen, NoMove);
                PokemonSummaryScreen_UpdatePageArrows(summaryScreen, 0);
                ClearMoveInfoWindows(summaryScreen);
                Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowButtonPrompt));
                PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, null);
                summaryScreen.Cursor = 0;
                summaryScreen.PageState = 1;
                break;
            case 1:
                if (ScrollMoveInfoIn(summaryScreen))
                {
                    summaryScreen.PageState = 2;
                }
                break;
            case 2:
                PokemonSummaryScreen_ShowMove5OrCancel(summaryScreen);
                if (summaryScreen.Data!.Mode != ModeLock)
                {
                    ShowButtonPrompt(summaryScreen, 0x98);
                }
                UpdateMoveAttributes(summaryScreen);
                PokemonSummaryScreen_ShowMonIcon(summaryScreen);
                PokemonSummaryScreen_DrawContestStatDots(summaryScreen);
                SetSpriteDraw(summaryScreen, SpriteMoveSelector1, 1);
                return 1;
        }
        return 0;
    }

    // OFFSET 0x0208E308
    private static byte HideContestMoveInfo(PokemonSummaryScreen summaryScreen)
    {
        switch (summaryScreen.PageState)
        {
            case 0:
                PokemonSummaryScreen_HideContestStatDots(summaryScreen);
                SetSpriteDraw2(summaryScreen, SpriteMonIcon, 0);
                UpdateAppealHearts(summaryScreen, NoMove);
                SetSpriteDraw(summaryScreen, SpriteMoveSelector1, 0);
                Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowButtonPrompt));
                PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, null);
                PokemonSummaryScreen_HideMoveCancelText(summaryScreen);
                PokemonSummaryScreen_ClearContestAttributeWindows(summaryScreen);
                summaryScreen.PageState = 1;
                break;
            case 1:
                if (ScrollMoveInfoOut(summaryScreen))
                {
                    summaryScreen.PageState = 2;
                }
                break;
            case 2:
                ShowMoveInfoWindows(summaryScreen);
                PokemonSummaryScreen_ClearAndPrintButtonPrompt(summaryScreen, 0x9E);
                PokemonSummaryScreen_UpdatePageArrows(summaryScreen, 1);
                PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, StaticWindow(summaryScreen, WindowButtonPrompt));
                return 1;
        }
        return 0;
    }

    // OFFSET 0x0208E3DC
    private static void DrawAppealHeart(PokemonSummaryScreen summaryScreen, ushort tile, byte slot)
    {
        BgConfig bgConfig = summaryScreen.BgConfig!;
        byte left = (byte)(slot * 2 + 2);
        byte right = (byte)(slot * 2 + 3);
        Bg.Bg_FillTilemapRect(bgConfig, 2, tile, left, 0x2F, 1, 1, 0x10);
        Bg.Bg_FillTilemapRect(bgConfig, 2, (ushort)(tile + 1), right, 0x2F, 1, 1, 0x10);
        Bg.Bg_FillTilemapRect(bgConfig, 2, (ushort)(tile + 0x20), left, 0x30, 1, 1, 0x10);
        Bg.Bg_FillTilemapRect(bgConfig, 2, (ushort)(tile + 0x21), right, 0x30, 1, 1, 0x10);
    }

    // OFFSET 0x0208E46C
    private static void DrawEmptyHearts(PokemonSummaryScreen summaryScreen)
    {
        for (int i = 0; i < 6; i++)
        {
            DrawAppealHeart(summaryScreen, HeartTileEmpty, (byte)i);
        }
        Bg.Bg_ScheduleTilemapTransfer(summaryScreen.BgConfig!, 2);
    }

    // OFFSET 0x0208E498
    private static void UpdateAppealHearts(PokemonSummaryScreen summaryScreen, int move)
    {
        DrawEmptyHearts(summaryScreen);
        if (move != NoMove)
        {
            int effect = (int)MoveTable.MoveTable_LoadParam(move, MoveParamContestEffect);
            sbyte hearts = (sbyte)(ContestEffect.ContestEffect_GetAppeal(effect) / 10);
            for (int i = 0; i < hearts; i++)
            {
                DrawAppealHeart(summaryScreen, HeartTileFull, (byte)i);
            }
        }
        Bg.Bg_ScheduleTilemapTransfer(summaryScreen.BgConfig!, 2);
    }

    // OFFSET 0x0208E4EC
    private static void ClearMoveAttributeWindows(PokemonSummaryScreen summaryScreen)
    {
        if (summaryScreen.Page == PageBattleMoves)
        {
            PokemonSummaryScreen_ClearBattleAttributeWindows(summaryScreen);
            return;
        }
        PokemonSummaryScreen_ClearContestAttributeWindows(summaryScreen);
    }

    // OFFSET 0x0208E508
    private static void RedrawMoveSelectPage(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummaryScreen_ShowMove5OrCancel(summaryScreen);
        UpdateMoveAttributes(summaryScreen);
        PokemonSummaryScreen_ShowMonIcon(summaryScreen);
        if (summaryScreen.Page == PageBattleMoves)
        {
            PokemonSummaryScreen_HideContestStatDots(summaryScreen);
            ShowMonTypeIcons(summaryScreen);
            Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 3, 0);
            return;
        }
        PokemonSummaryScreen_DrawContestStatDots(summaryScreen);
        Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 3, 0x100);
    }

    // OFFSET 0x0208E57C
    private static byte SetupRibbonInfo(PokemonSummaryScreen summaryScreen)
    {
        switch (summaryScreen.PageState)
        {
            case 0:
                Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 0, 0x100);
                Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 3, 0);
                PokemonSummaryScreen_UpdatePageArrows(summaryScreen, 0);
                Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelItem));
                Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowItemName));
                Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelRibbonCount));
                Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowButtonPrompt));
                Bg.Window_ClearAndScheduleCopyToVRAM(summaryScreen.ExtraWindows![0]);
                PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, null);
                summaryScreen.RibbonCol = 0;
                summaryScreen.RibbonRow = 0;
                summaryScreen.PageState = 1;
                break;
            case 1:
                if (Bg.Bg_GetYOffset(summaryScreen.BgConfig!, 2) >= 0x30)
                {
                    Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 3, 0x38);
                    summaryScreen.PageState = 2;
                }
                else
                {
                    Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 4, 0x10);
                }
                break;
            case 2:
                ShowButtonPrompt(summaryScreen, 0xB5);
                SetSpriteDraw(summaryScreen, SpriteRibbonCursor, 1);
                SetSpriteDraw(summaryScreen, SpriteRibbonFlash, 1);
                for (int i = 0; i < 4; i++)
                {
                    SetSpriteDraw(summaryScreen, SpriteRibbon9 + i, 0);
                }
                ChangeSelectedRibbon(summaryScreen, 0);
                return 1;
        }
        return 0;
    }

    // OFFSET 0x0208E6A8
    private static byte HideRibbonInfo(PokemonSummaryScreen summaryScreen)
    {
        switch (summaryScreen.PageState)
        {
            case 0:
                summaryScreen.RibbonCol = 0;
                summaryScreen.RibbonRow = 0;
                Bg.Window_ClearAndScheduleCopyToVRAM(summaryScreen.ExtraWindows![1]);
                Bg.Window_ClearAndScheduleCopyToVRAM(summaryScreen.ExtraWindows[2]);
                Bg.Window_ClearAndScheduleCopyToVRAM(summaryScreen.ExtraWindows[3]);
                Bg.Window_ClearAndScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowButtonPrompt));
                PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, null);
                PokemonSummaryScreen_UpdateRibbonSprites(summaryScreen);
                summaryScreen.PageState = 1;
                break;
            case 1:
                if (Bg.Bg_GetYOffset(summaryScreen.BgConfig!, 2) <= 8)
                {
                    Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 3, 0);
                    summaryScreen.PageState = 2;
                }
                else
                {
                    Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, 2, 5, 0x10);
                }
                break;
            case 2:
                Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelItem));
                Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowItemName));
                Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowLabelRibbonCount));
                Bg.Window_ScheduleCopyToVRAM(StaticWindow(summaryScreen, WindowButtonPrompt));
                Bg.Window_ScheduleCopyToVRAM(summaryScreen.ExtraWindows![0]);
                PokemonSummaryScreen_ClearAndPrintButtonPrompt(summaryScreen, 0xB4);
                PokemonSummaryScreen_UpdatePageArrows(summaryScreen, 1);
                PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, StaticWindow(summaryScreen, WindowButtonPrompt));
                return 1;
        }
        return 0;
    }

    // OFFSET 0x0208E794
    private static void ChangeSelectedRibbon(PokemonSummaryScreen summaryScreen, int delta)
    {
        byte oldCol = summaryScreen.RibbonCol;
        byte oldRow = summaryScreen.RibbonRow;
        byte max = summaryScreen.RibbonMax;
        if (delta == 1)
        {
            if ((oldCol & 3) == 3)
            {
                return;
            }
            if (oldRow * RibbonsPerRow + oldCol + 1 < max)
            {
                summaryScreen.RibbonCol = (byte)(oldCol + 1);
            }
        }
        else if (delta == -1)
        {
            if ((oldCol & 3) == 0)
            {
                return;
            }
            summaryScreen.RibbonCol = (byte)(oldCol - 1);
        }
        else if (delta == RibbonsPerRow)
        {
            if (oldCol < RibbonsPerRow)
            {
                if (oldRow * RibbonsPerRow + oldCol + RibbonsPerRow < max)
                {
                    summaryScreen.RibbonCol = (byte)(oldCol + RibbonsPerRow);
                }
                else if (max > RibbonsPerRow && max % RibbonsPerRow != 0)
                {
                    summaryScreen.RibbonCol = (byte)(max % RibbonsPerRow + 3);
                }
            }
            else if (oldRow * RibbonsPerRow + 8 < max)
            {
                summaryScreen.RibbonRow++;
                if (summaryScreen.RibbonCol + summaryScreen.RibbonRow * RibbonsPerRow >= max)
                {
                    summaryScreen.RibbonCol = (byte)(max % RibbonsPerRow + 3);
                }
            }
        }
        else if (delta == -RibbonsPerRow)
        {
            if (oldCol >= RibbonsPerRow)
            {
                summaryScreen.RibbonCol = (byte)(oldCol - RibbonsPerRow);
            }
            else if (oldRow != 0)
            {
                summaryScreen.RibbonRow--;
            }
        }
        if (oldCol != summaryScreen.RibbonCol || oldRow != summaryScreen.RibbonRow)
        {
            SoundPlayback.Sound_PlayEffect(SeqSelect);
        }
        summaryScreen.RibbonID = PokemonSummaryScreen_RibbonIDAt(summaryScreen, summaryScreen.RibbonCol);
        PokemonSummaryScreen_UpdateRibbonCursorPos(summaryScreen);
        if (oldRow != summaryScreen.RibbonRow)
        {
            PokemonSummaryScreen_RefreshRibbonSprites(summaryScreen);
        }
        PokemonSummaryScreen_PrintRibbonNameAndDesc(summaryScreen);
        PokemonSummaryScreen_PrintRibbonNumbers(summaryScreen);
        SetSpriteDraw(summaryScreen, SpriteRibbonArrowUp, summaryScreen.RibbonRow != 0 ? 1 : 0);
        SetSpriteDraw(summaryScreen, SpriteRibbonArrowDown, summaryScreen.RibbonRow * RibbonsPerRow + 8 < summaryScreen.RibbonMax ? 1 : 0);
    }
}
