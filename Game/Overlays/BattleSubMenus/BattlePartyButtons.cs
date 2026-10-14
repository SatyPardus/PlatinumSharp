namespace PlatinumSharp.Game;

public static partial class BattleParties
{
    private const uint ButtonWindowLists = 0x0222989C;
    private const uint PartyButtonScrollDistances = 0x0222988C;
    private const uint ButtonScrollDistances = 0x0222987C;
    private const uint PartyButtonSpriteOffsets = 0x02229896;
    private const uint ButtonSpriteOffsets = 0x02229890;
    private const uint ButtonRects = 0x02229924;
    private const int ButtonBgLayer = 6;
    private const int ButtonStateDefault = 0;
    private const int ButtonStatePressed = 1;
    private const int ButtonStateReleased = 2;
    private const int ButtonStateDisabled = 3;
    private const int ButtonCancel = 6;
    private const int ButtonShift = 7;
    private const int ButtonSummary = 8;
    private const int ButtonSummaryMoves = 9;
    private const int ButtonCheckMoves = 0xA;
    private const int ButtonSummaryCheckMoves = 0xB;
    private const int ButtonPrevPokemon = 0xC;
    private const int ButtonNextPokemon = 0xD;
    private const int ButtonPokemonMove1 = 0xE;
    private const int ButtonContestStats = 0x12;
    private const int ButtonRestorePPMove1 = 0x13;
    private const int ButtonLearnMove1 = 0x17;
    private const int ButtonMoveToLearn = 0x1B;
    private const int ButtonConfirmLearnMove = 0x1C;
    private const int ButtonConfirmLearnMoveContest = 0x1D;
    private const int ButtonMoveSummaryMove1 = 0x1E;
    private const int WindowListEnd = 0xFF;
    private const int MaxButtonWindows = 8;
    private const ushort TilePaletteMask = 0xFFF;

    private static byte ButtonRect(int button, int field)
    {
        return BattleSubMenuData.Bytes((uint)(ButtonRects + button * 4 + field), 1)[0];
    }

    // OFFSET 0x02225710
    private static void BattlePartyButtons_InitializeButtonData(BattleParty battleParty, byte[] rawData)
    {
        for (int i = 0; i < 4; i++)
        {
            LoadButtonData(battleParty.PartyPokemonButtonData[i], rawData, 0, i * 6, 0x10, 6);
        }
        for (int i = 0; i < 4; i++)
        {
            LoadButtonData(battleParty.AltPartyPokemonButtonData[i], rawData, 0x10, i * 6, 0x10, 6);
        }
        LoadButtonData(battleParty.SummaryCheckMovesButtonData[0], rawData, 0, 0x27, 0xD, 5);
        LoadButtonData(battleParty.SummaryCheckMovesButtonData[1], rawData, 0, 0x2C, 0xD, 5);
        LoadButtonData(battleParty.SummaryCheckMovesButtonData[2], rawData, 0xD, 0x27, 0xD, 5);
        LoadButtonData(battleParty.SummaryCheckMovesButtonData[3], rawData, 0xD, 0x2C, 0xD, 5);
        for (int i = 0; i < 4; i++)
        {
            LoadButtonData(battleParty.PrevPokemonButtonData[i], rawData, i * 5, 0x31, 5, 5);
        }
        for (int i = 0; i < 4; i++)
        {
            LoadButtonData(battleParty.NextPokemonButtonData[i], rawData, i * 5, 0x36, 5, 5);
        }
        for (int i = 0; i < 4; i++)
        {
            LoadButtonData(battleParty.CancelButtonData[i], rawData, 0x1A, 0x18 + i * 5, 5, 5);
        }
        for (int i = 0; i < 3; i++)
        {
            LoadButtonData(battleParty.LearnMoveConfirmButtonData[i], rawData, 0, 0x18 + i * 5, 0x1A, 5);
        }
        for (int i = 0; i < 3; i++)
        {
            LoadButtonData(battleParty.ContestStatsButtonData[i], rawData, 0x14, 0x31 + i * 4, 9, 4);
        }
        for (int i = 0; i < 3; i++)
        {
            LoadButtonData(battleParty.MoveSummaryScreenMoveButtonData[i], rawData, i * 5, 0x3B, 5, 2);
        }
    }

    // OFFSET 0x02225A3C
    private static void BattlePartyButtons_InitializeAdditionalButtonData(BattleParty battleParty, byte[] rawData)
    {
        LoadButtonData(battleParty.ShiftPokemonButtonData[0], rawData, 0, 0, 0x1E, 0x11);
        LoadButtonData(battleParty.ShiftPokemonButtonData[1], rawData, 0, 0x11, 0x1E, 0x11);
        LoadButtonData(battleParty.ShiftPokemonButtonData[2], rawData, 0, 0x22, 0x1E, 0x11);
        LoadButtonData(battleParty.MoveButtonData[0], rawData, 0, 0x33, 0x10, 6);
        LoadButtonData(battleParty.MoveButtonData[1], rawData, 0x10, 0x33, 0x10, 6);
        LoadButtonData(battleParty.MoveButtonData[2], rawData, 0, 0x39, 0x10, 6);
        LoadButtonData(battleParty.MoveButtonData[3], rawData, 0x10, 0x39, 0x10, 6);
    }

    // OFFSET 0x02225AF0
    private static void LoadButtonData(ushort[] dest, byte[] source, int x, int y, byte width, byte height)
    {
        for (ushort row = 0; row < height; row++)
        {
            for (ushort col = 0; col < width; col++)
            {
                int src = ((y + row) * 0x20 + x + col) * 2;
                dest[row * width + col] = (ushort)(source[src] | (source[src + 1] << 8));
            }
        }
    }

    // OFFSET 0x02225B40
    private static ushort[]? RetrieveRawButtonData(BattleParty battleParty, int button, int state, int useAltButtons)
    {
        switch (button)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                if (useAltButtons == 0)
                {
                    return battleParty.PartyPokemonButtonData[state];
                }
                return battleParty.AltPartyPokemonButtonData[state];
            case ButtonCancel:
                return battleParty.CancelButtonData[state];
            case ButtonShift:
                return battleParty.ShiftPokemonButtonData[state];
            case ButtonSummary:
            case ButtonSummaryMoves:
            case ButtonCheckMoves:
            case ButtonSummaryCheckMoves:
                return battleParty.SummaryCheckMovesButtonData[state];
            case ButtonPrevPokemon:
                return battleParty.PrevPokemonButtonData[state];
            case ButtonNextPokemon:
                return battleParty.NextPokemonButtonData[state];
            case ButtonContestStats:
                return battleParty.ContestStatsButtonData[state];
            case ButtonConfirmLearnMove:
            case ButtonConfirmLearnMoveContest:
                return battleParty.LearnMoveConfirmButtonData[state];
            case 0x1E:
            case 0x1F:
            case 0x20:
            case 0x21:
                return battleParty.MoveSummaryScreenMoveButtonData[state];
        }
        if (button >= ButtonPokemonMove1 && button <= ButtonMoveToLearn)
        {
            return battleParty.MoveButtonData[state];
        }
        return null;
    }

    private static void SetTilePalette(ushort[] tiles, int count, int palette)
    {
        for (byte i = 0; i < count; i++)
        {
            tiles[i] = (ushort)((tiles[i] & TilePaletteMask) | (palette << 12));
        }
    }

    // OFFSET 0x02225C4C
    private static void RetrieveButtonData(BattleParty battleParty, ushort[] dest, int button, int state, int useAltButtons)
    {
        ushort[] source = RetrieveRawButtonData(battleParty, button, state, useAltButtons)!;
        byte width = ButtonRect(button, 2);
        byte height = ButtonRect(button, 3);
        int size = width * height;
        Array.Copy(source, dest, size);
        if (button >= 0 && button <= 5)
        {
            BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[button];
            if (partyPokemon.Species == 0)
            {
                return;
            }
            if (partyPokemon.IsEgg != 0)
            {
                ushort[] eggTiles = new ushort[2];
                eggTiles[0] = dest[width * 2 + 5];
                eggTiles[1] = dest[width + width * 2 + 5];
                for (byte i = 0; i < 2; i++)
                {
                    for (byte j = 0; j < 9; j++)
                    {
                        dest[(i + 2) * width + j + 6] = eggTiles[i];
                    }
                }
                return;
            }
            if (partyPokemon.CurHP == 0)
            {
                SetTilePalette(dest, size, 2);
                return;
            }
            if (BattlePartyTask_CheckIfSwitchingWithPartnersPokemon(battleParty, (byte)button) == 1)
            {
                SetTilePalette(dest, size, 1);
            }
            return;
        }
        if (button == ButtonMoveToLearn)
        {
            SetTilePalette(dest, size, 0xA);
        }
    }

    // OFFSET 0x02225D8C
    private static void DrawButton(BattleParty battleParty, int button, int state, int useAltButtons)
    {
        byte height = ButtonRect(button, 3);
        byte width = ButtonRect(button, 2);
        ushort[] tiles = new ushort[width * height];
        RetrieveButtonData(battleParty, tiles, button, state, useAltButtons);
        byte[] buffer = new byte[tiles.Length * 2];
        for (int i = 0; i < tiles.Length; i++)
        {
            buffer[i * 2] = (byte)tiles[i];
            buffer[i * 2 + 1] = (byte)(tiles[i] >> 8);
        }
        Bg.Bg_LoadToTilemapRect(battleParty.Background!, ButtonBgLayer, buffer, ButtonRect(button, 0), ButtonRect(button, 1), width, height);
        Bg.Bg_ScheduleTilemapTransfer(battleParty.Background!, ButtonBgLayer);
    }

    // OFFSET 0x02225E08
    private static void UpdateWindowScroll(BattleParty battleParty, int button, int state)
    {
        uint windowList = BattleSubMenuData.U32((uint)(ButtonWindowLists + button * 4));
        if (windowList == 0)
        {
            return;
        }
        int distance;
        if (button <= 5)
        {
            distance = BattleSubMenuData.Bytes((uint)(PartyButtonScrollDistances + state), 1)[0];
        }
        else
        {
            distance = BattleSubMenuData.Bytes((uint)(ButtonScrollDistances + state), 1)[0];
        }
        int direction = 0;
        if (state == ButtonStateDefault)
        {
            direction = 1;
        }
        else if (state == ButtonStatePressed)
        {
            direction = 0;
        }
        else if (state == ButtonStateReleased)
        {
            direction = 1;
        }
        if (button >= ButtonPokemonMove1 && button <= ButtonPokemonMove1 + 3)
        {
            byte windowIndex = BattleSubMenuData.Bytes((uint)(windowList + battleParty.UseAltSummaryWindows), 1)[0];
            Bg.Window_Scroll(PartyWindow(battleParty, windowIndex), direction, distance, 0);
            Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, windowIndex));
            return;
        }
        byte[] windows = BattleSubMenuData.Bytes(windowList, MaxButtonWindows);
        for (ushort i = 0; i < MaxButtonWindows; i++)
        {
            if (windows[i] == WindowListEnd)
            {
                break;
            }
            Bg.Window_Scroll(PartyWindow(battleParty, windows[i]), direction, distance, 0);
            Bg.Window_ScheduleCopyToVRAM(PartyWindow(battleParty, windows[i]));
        }
    }

    private static short SpriteOffset(uint table, int state)
    {
        byte[] raw = BattleSubMenuData.Bytes((uint)(table + state * 2), 2);
        return (short)(raw[0] | (raw[1] << 8));
    }

    // OFFSET 0x02225EB8
    private static void UpdateSpritePositions(BattleParty battleParty, int button, int state)
    {
        switch (button)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            {
                short offset = SpriteOffset(PartyButtonSpriteOffsets, state);
                SpriteSystem.ManagedSprite_OffsetPositionXY(PartySprite(battleParty, SpriteSummaryStatus + button), 0, offset);
                SpriteSystem.ManagedSprite_OffsetPositionXY(PartySprite(battleParty, button), 0, offset);
                SpriteSystem.ManagedSprite_OffsetPositionXY(PartySprite(battleParty, SpritePartyPokemon + button), 0, offset);
                break;
            }
            case ButtonShift:
            {
                short offset = SpriteOffset(ButtonSpriteOffsets, state);
                int slot = battleParty.Context!.SelectedPartyIndex;
                SpriteSystem.ManagedSprite_OffsetPositionXY(PartySprite(battleParty, slot), 0, offset);
                SpriteSystem.ManagedSprite_OffsetPositionXY(PartySprite(battleParty, SpritePartyPokemon + slot), 0, offset);
                break;
            }
            case 0xE:
            case 0xF:
            case 0x10:
            case 0x11:
                SpriteSystem.ManagedSprite_OffsetPositionXY(PartySprite(battleParty, button + 7), 0, SpriteOffset(ButtonSpriteOffsets, state));
                break;
            case 0x13:
            case 0x14:
            case 0x15:
            case 0x16:
                SpriteSystem.ManagedSprite_OffsetPositionXY(PartySprite(battleParty, button + 2), 0, SpriteOffset(ButtonSpriteOffsets, state));
                break;
            case 0x17:
            case 0x18:
            case 0x19:
            case 0x1A:
            case 0x1B:
                SpriteSystem.ManagedSprite_OffsetPositionXY(PartySprite(battleParty, button - 2), 0, SpriteOffset(ButtonSpriteOffsets, state));
                break;
        }
    }

    // OFFSET 0x02225FCC
    private static void BattlePartyButtons_PressButton(BattleParty battleParty, int button)
    {
        battleParty.UseAltButtons = 0;
        if (button <= 5 && BattlePartyTask_CheckCanPartySlotBeSelected(battleParty, button) == PartySlotSelectableNotInBattle)
        {
            battleParty.UseAltButtons = 1;
        }
        battleParty.PressedButtonState = 0;
        battleParty.Unused1 = 0;
        battleParty.PressedButton = (byte)button;
        battleParty.IsAButtonPressed = 1;
    }

    // OFFSET 0x0222601C
    private static void BattlePartyButtons_Tick(BattleParty battleParty)
    {
        if (battleParty.IsAButtonPressed == 0)
        {
            return;
        }
        switch (battleParty.PressedButtonState)
        {
            case 0:
                DrawButton(battleParty, battleParty.PressedButton, ButtonStatePressed, battleParty.UseAltButtons);
                UpdateWindowScroll(battleParty, battleParty.PressedButton, ButtonStatePressed);
                UpdateSpritePositions(battleParty, battleParty.PressedButton, ButtonStatePressed);
                battleParty.Unused1 = 0;
                battleParty.PressedButtonState = 1;
                break;
            case 1:
                DrawButton(battleParty, battleParty.PressedButton, ButtonStateReleased, battleParty.UseAltButtons);
                UpdateWindowScroll(battleParty, battleParty.PressedButton, ButtonStateReleased);
                UpdateSpritePositions(battleParty, battleParty.PressedButton, ButtonStateReleased);
                battleParty.Unused1 = 0;
                battleParty.PressedButtonState = 2;
                break;
            case 2:
                DrawButton(battleParty, battleParty.PressedButton, ButtonStateDefault, battleParty.UseAltButtons);
                UpdateWindowScroll(battleParty, battleParty.PressedButton, ButtonStateDefault);
                UpdateSpritePositions(battleParty, battleParty.PressedButton, ButtonStateDefault);
                battleParty.Unused1 = 0;
                battleParty.IsAButtonPressed = 0;
                break;
        }
    }

    private static void DrawPrevNextButtons(BattleParty battleParty)
    {
        if (CheckMultipleSelectablePokemon(battleParty) == 1)
        {
            DrawButton(battleParty, ButtonPrevPokemon, ButtonStateDefault, 0);
            DrawButton(battleParty, ButtonNextPokemon, ButtonStateDefault, 0);
        }
        else
        {
            DrawButton(battleParty, ButtonPrevPokemon, ButtonStateDisabled, 0);
            DrawButton(battleParty, ButtonNextPokemon, ButtonStateDisabled, 0);
        }
    }

    private static void DrawContestStatsButton(BattleParty battleParty)
    {
        if (battleParty.HasVisitedContestHall == 1)
        {
            DrawButton(battleParty, ButtonContestStats, ButtonStateDefault, 0);
        }
    }

    // OFFSET 0x022260EC
    private static void BattlePartyButtons_InitializeButtons(BattleParty battleParty, int screen)
    {
        switch (screen)
        {
            case ScreenPokemonParty:
                for (ushort i = 0; i < 6; i++)
                {
                    byte selectable = BattlePartyTask_CheckCanPartySlotBeSelected(battleParty, i);
                    if (selectable == 0)
                    {
                        DrawButton(battleParty, i, ButtonStateDisabled, 1);
                    }
                    else if (selectable == PartySlotSelectableInBattle)
                    {
                        DrawButton(battleParty, i, ButtonStateDefault, 0);
                    }
                    else if (selectable == PartySlotSelectableNotInBattle)
                    {
                        DrawButton(battleParty, i, ButtonStateDefault, 1);
                    }
                }
                if (battleParty.Context!.BattlePartyMode == ModeSelectPokemonNoCancel)
                {
                    DrawButton(battleParty, ButtonCancel, ButtonStateDisabled, 0);
                }
                else
                {
                    DrawButton(battleParty, ButtonCancel, ButtonStateDefault, 0);
                }
                break;
            case ScreenSelectPokemon:
                DrawButton(battleParty, ButtonCancel, ButtonStateDefault, 0);
                DrawButton(battleParty, ButtonShift, ButtonStateDefault, 0);
                if (SelectedPartyPokemon(battleParty).IsEgg != 0)
                {
                    DrawButton(battleParty, ButtonSummary, ButtonStateDisabled, 0);
                    DrawButton(battleParty, ButtonCheckMoves, ButtonStateDisabled, 0);
                }
                else
                {
                    DrawButton(battleParty, ButtonSummary, ButtonStateDefault, 0);
                    DrawButton(battleParty, ButtonCheckMoves, ButtonStateDefault, 0);
                }
                break;
            case ScreenPokemonSummary:
                DrawPrevNextButtons(battleParty);
                DrawButton(battleParty, ButtonSummaryCheckMoves, ButtonStateDefault, 0);
                DrawButton(battleParty, ButtonCancel, ButtonStateDefault, 0);
                break;
            case ScreenPokemonMoves:
                DrawPrevNextButtons(battleParty);
                for (ushort i = 0; i < 4; i++)
                {
                    if (SelectedPartyPokemon(battleParty).Moves[i].Move != 0)
                    {
                        DrawButton(battleParty, ButtonPokemonMove1 + i, ButtonStateDefault, 0);
                    }
                    else
                    {
                        DrawButton(battleParty, ButtonPokemonMove1 + i, ButtonStateDisabled, 0);
                    }
                }
                DrawButton(battleParty, ButtonSummaryMoves, ButtonStateDefault, 0);
                DrawButton(battleParty, ButtonCancel, ButtonStateDefault, 0);
                break;
            case ScreenMoveSummary:
                DrawButton(battleParty, ButtonCancel, ButtonStateDefault, 0);
                for (ushort i = 0; i < 4; i++)
                {
                    if (battleParty.Context!.SelectedMoveSlot == i)
                    {
                        DrawButton(battleParty, ButtonMoveSummaryMove1 + i, ButtonStateReleased, 0);
                    }
                    else
                    {
                        DrawButton(battleParty, ButtonMoveSummaryMove1 + i, ButtonStateDefault, 0);
                    }
                }
                break;
            case ScreenRestoreMovePP:
                for (ushort i = 0; i < 4; i++)
                {
                    if (SelectedPartyPokemon(battleParty).Moves[i].Move != 0)
                    {
                        DrawButton(battleParty, ButtonRestorePPMove1 + i, ButtonStateDefault, 0);
                    }
                    else
                    {
                        DrawButton(battleParty, ButtonRestorePPMove1 + i, ButtonStateDisabled, 0);
                    }
                }
                DrawButton(battleParty, ButtonCancel, ButtonStateDefault, 0);
                break;
            case ScreenLearnMove:
            case ScreenLearnMoveContestStats:
                for (int i = ButtonLearnMove1; i <= ButtonMoveToLearn; i++)
                {
                    DrawButton(battleParty, i, ButtonStateDefault, 0);
                }
                DrawButton(battleParty, ButtonCancel, ButtonStateDefault, 0);
                DrawContestStatsButton(battleParty);
                break;
            case ScreenConfirmLearnMove:
                DrawButton(battleParty, ButtonConfirmLearnMove, ButtonStateDefault, 0);
                DrawButton(battleParty, ButtonCancel, ButtonStateDefault, 0);
                DrawContestStatsButton(battleParty);
                break;
            case ScreenConfirmLearnMoveContestStats:
                DrawButton(battleParty, ButtonConfirmLearnMoveContest, ButtonStateDefault, 0);
                DrawButton(battleParty, ButtonCancel, ButtonStateDefault, 0);
                DrawContestStatsButton(battleParty);
                break;
        }
    }

    // OFFSET 0x02226444
    private static void BattlePartyButtons_LoadScreenPaletteData(BattleParty battleParty, int screen)
    {
        if (screen == ScreenPokemonMoves)
        {
            PaletteData.PaletteData_LoadBuffer(battleParty.Palette!, battleParty.ScreenPaletteData, 0x10, PaletteBufferSubBg, 0xC0, 0x20);
        }
        else
        {
            PaletteData.PaletteData_LoadBuffer(battleParty.Palette!, battleParty.ScreenPaletteData, 0, PaletteBufferSubBg, 0xC0, 0x20);
        }
    }

    // OFFSET 0x02226484
    private static byte CheckMultipleSelectablePokemon(BattleParty battleParty)
    {
        ushort count = 0;
        for (ushort i = 0; i < 6; i++)
        {
            if (battleParty.PartyPokemon[i].Species != 0 && battleParty.PartyPokemon[i].IsEgg == 0)
            {
                count++;
            }
        }
        return (byte)(count >= 2 ? 1 : 0);
    }

    // OFFSET 0x022264C4
    private static void BattlePartyButtons_DrawSelectedPartyPokemonButton(BattleParty battleParty)
    {
        int slot = battleParty.Context!.SelectedPartyIndex;
        if (BattlePartyTask_CheckCanPartySlotBeSelected(battleParty, slot) == PartySlotSelectableNotInBattle)
        {
            DrawButton(battleParty, slot, ButtonStateDefault, 1);
        }
        else
        {
            DrawButton(battleParty, slot, ButtonStateDefault, 0);
        }
    }
}
