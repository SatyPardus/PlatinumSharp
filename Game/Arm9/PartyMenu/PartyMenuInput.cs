using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public delegate void PartyMenuAction(PartyMenuApplication app, out int state);

public static partial class PartyMenus
{
    private const uint MemberPanelTemplatesMulti = 0x020F1E40;
    private const uint MemberPanelTemplatesStandard = 0x020F1DF8;
    private const uint SwitchFromConfirmOrders = 0x020F1BD4;
    private const uint TouchButtonCoords = 0x020F1BB8;
    private const uint PartyBallTouchRects = 0x020F1C94;
    private const uint FieldMoveTable = 0x020F1CB0;
    private const int FieldMoveCount = 15;
    private const int KeyRight = 0x10;
    private const int KeyLeft = 0x20;
    private const int KeyUp = 0x40;
    private const int KeyDown = 0x80;
    private const int SeqTouchButton = 0x5E4;
    private const int SeqHPTransfer = 0x5EC;
    private const int SeqCannotSelect = 0x5F2;
    private const int SlotConfirm = 6;
    private const int SlotCancel = 7;
    private const int InputDirectionPad = 1;
    private const int InputTouchScreen = 2;
    private const int Input4 = 4;
    private const int InputNone = 5;
    private const int WinHealthbarOffset = 3;
    private const int WinsPerMember = 5;

    private const int HPTransferBuffer = 0;
    private const int HPTransferState = 1;
    private const int HPTransferCounter = 2;
    private const int HPTransferJournalMove = 3;

    private static TouchScreenHitTable[] ReadTouchRects(uint address)
    {
        byte[] raw = RomData.Arm9(address, 0x20 * 4);
        int count = 0;
        while (raw[count * 4] != TouchScreen.TableTerminator)
        {
            count++;
        }
        TouchScreenHitTable[] rects = new TouchScreenHitTable[count + 1];
        for (int i = 0; i <= count; i++)
        {
            rects[i].Top = raw[i * 4];
            rects[i].Bottom = raw[i * 4 + 1];
            rects[i].Left = raw[i * 4 + 2];
            rects[i].Right = raw[i * 4 + 3];
        }
        return rects;
    }

    private static ushort[] ReadMemberPanelTemplates(uint address)
    {
        return RomData.Arm9U16(address, MaxPartySize * 6);
    }

    // OFFSET 0x0207F308
    private static void SetupRequestedModePanels(PartyMenuApplication app)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        ushort[] templates = ReadMemberPanelTemplates(partyMenu.Type == TypeMultiBattle ? MemberPanelTemplatesMulti : MemberPanelTemplatesStandard);
        int mode = partyMenu.Mode;
        if (mode == ModeUseEvoItem)
        {
            DrawMemberPanels_UsingEvoItem(app, templates);
        }
        else if (mode == ModeTeachMove)
        {
            DrawMemberPanels_TeachingMove(app, templates);
        }
        else if (mode == ModeContest)
        {
            DrawMemberPanels_EnteringContest(app, templates);
        }
        else if (mode == ModeSelectConfirm || mode == ModeBattleTower || mode == ModeBattleHall || mode == ModeBattleCastle)
        {
            DrawMemberPanels_SelectingOrder(app, templates);
        }
        else
        {
            DrawMemberPanels_Standard(app, templates);
        }
        Bg.Bg_ScheduleTilemapTransfer(app.BgConfig!, 2);
        Bg.Bg_ScheduleTilemapTransfer(app.BgConfig!, 4);
    }

    private static void DrawMemberSprites(PartyMenuApplication app, int slot, ushort[] templates, NARC narc)
    {
        int t = slot * 6;
        PartyMenu_DrawMemberSpeciesIcon(app, slot, templates[t + 2], templates[t + 3], narc);
        PartyMenu_DrawMemberPokeBall(app, slot, templates[t + 4], templates[t + 5]);
        PartyMenu_DrawMemberHeldItem(app, slot, app.PartyMembers[slot].HeldItem);
        PartyMenu_AlignMemberHeldItem(app, slot, (short)templates[t + 2], (short)templates[t + 3]);
        PartyMenu_DrawMemberBallSeal(app, slot);
        PartyMenu_AlignMemberBallSeal(app, slot);
        PartyMenu_DrawMemberStatusCondition(app, slot, (byte)(app.PartyMembers[slot].StatusIcon & 0xFFF));
        DrawMemberTouchScreenButton(app, slot, 0);
    }

    // OFFSET 0x0207F388
    private static void DrawMemberPanels_Standard(PartyMenuApplication app, ushort[] templates)
    {
        NARC narc = NARC.NARC_ctor(PokemonIconArchive, HeapID);
        for (int i = 0; i < MaxPartySize; i++)
        {
            if (PartyMenu_LoadMember(app, i))
            {
                bool hasHealthbar = app.PartyMembers[i].IsEgg != 1;
                DrawMemberPanel(app, i, (byte)templates[i * 6], (byte)templates[i * 6 + 1], hasHealthbar);
                PartyMenu_DrawMemberPanelData(app, i);
                DrawMemberSprites(app, i, templates, narc);
            }
            else
            {
                DrawEmptyMemberPanel(app, i, (short)templates[i * 6], (short)templates[i * 6 + 1]);
            }
        }
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x0207F4AC
    private static void DrawMemberPanels_UsingEvoItem(PartyMenuApplication app, ushort[] templates)
    {
        NARC narc = NARC.NARC_ctor(PokemonIconArchive, HeapID);
        for (int i = 0; i < MaxPartySize; i++)
        {
            if (PartyMenu_LoadMember(app, i))
            {
                DrawMemberPanel(app, i, (byte)templates[i * 6], (byte)templates[i * 6 + 1], false);
                PrintMemberEvoComment(app, i);
                DrawMemberSprites(app, i, templates, narc);
            }
            else
            {
                DrawEmptyMemberPanel(app, i, (short)templates[i * 6], (short)templates[i * 6 + 1]);
            }
        }
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x0207F5A0
    private static void DrawMemberPanels_TeachingMove(PartyMenuApplication app, ushort[] templates)
    {
        NARC narc = NARC.NARC_ctor(PokemonIconArchive, HeapID);
        for (int i = 0; i < MaxPartySize; i++)
        {
            if (PartyMenu_LoadMember(app, i))
            {
                DrawMemberPanel(app, i, (byte)templates[i * 6], (byte)templates[i * 6 + 1], false);
                PrintMemberMoveComment(app, i);
                DrawMemberSprites(app, i, templates, narc);
            }
            else
            {
                DrawEmptyMemberPanel(app, i, (short)templates[i * 6], (short)templates[i * 6 + 1]);
            }
        }
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x0207F694
    private static void DrawMemberPanels_EnteringContest(PartyMenuApplication app, ushort[] templates)
    {
        NARC narc = NARC.NARC_ctor(PokemonIconArchive, HeapID);
        for (int i = 0; i < MaxPartySize; i++)
        {
            if (PartyMenu_LoadMember(app, i))
            {
                DrawMemberPanel(app, i, (byte)templates[i * 6], (byte)templates[i * 6 + 1], false);
                PrintMemberContestComment(app, i);
                DrawMemberSprites(app, i, templates, narc);
            }
            else
            {
                DrawEmptyMemberPanel(app, i, (short)templates[i * 6], (short)templates[i * 6 + 1]);
            }
        }
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x0207F788
    private static void DrawMemberPanels_SelectingOrder(PartyMenuApplication app, ushort[] templates)
    {
        NARC narc = NARC.NARC_ctor(PokemonIconArchive, HeapID);
        for (int i = 0; i < MaxPartySize; i++)
        {
            if (PartyMenu_LoadMember(app, i))
            {
                DrawMemberPanel(app, i, (byte)templates[i * 6], (byte)templates[i * 6 + 1], false);
                PartyMenu_PrintMemberNameAndLevel(app, i);
                PrintMemberSelectionComment(app, i);
                DrawMemberSprites(app, i, templates, narc);
            }
            else
            {
                DrawEmptyMemberPanel(app, i, (short)templates[i * 6], (short)templates[i * 6 + 1]);
            }
        }
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x0207F884
    private static void DrawEmptyMemberPanel(PartyMenuApplication app, int slot, short x, short y)
    {
        Bg.Bg_CopyToTilemapRect(app.BgConfig!, 2, (byte)x, (byte)y, 0x10, 6, app.NoneMemberPanel, 0, 0, 0x10, 6);
        Bg.Bg_ChangeTilemapRectPalette(app.BgConfig!, 2, (byte)x, (byte)y, 0x10, 6, 1);
        PartyMenu_DrawMemberStatusCondition(app, slot, 7);
        PartyMenu_DrawMemberHeldItem(app, slot, 0);
        PartyMenu_DrawMemberBallSeal(app, slot);
    }

    // OFFSET 0x0207F8F8
    public static void PartyMenu_UpdateSlotPalette(PartyMenuApplication app, int slot)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, slot);
        int palette;
        if (app.InTargetSlotMode && (slot == app.CurrPartySlot || slot == app.SelectTargetSlot))
        {
            palette = 7;
        }
        else
        {
            palette = slot == app.CurrPartySlot ? 4 : 0;
            if (Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null) == 0)
            {
                palette += 2;
            }
            else if (IsSecondaryMultiSlot(app, slot))
            {
                palette += 1;
            }
        }
        Bg.Bg_LoadPalette(2, app.Colors, palette * 0x10, 0x10, (ushort)((slot + 3) * 0x20));
    }

    // OFFSET 0x0207F984
    private static bool IsSecondaryMultiSlot(PartyMenuApplication app, int slot)
    {
        return app.PartyMenu!.Type == TypeMultiBattle && (slot & 1) != 0;
    }

    // OFFSET 0x0207F9A0
    private static void SetupMenuCursor(PartyMenuApplication app)
    {
        byte x = 0;
        byte y = 0;
        byte unusedX = 0;
        byte unusedY = 0;
        GridMenuCursor.GridMenuCursor_CheckNavigation(app.CursorPosTable!, ref x, ref y, ref unusedX, ref unusedY, app.CurrPartySlot, 4);
        Sprite.Sprite_SetAnim(app.Sprites[SpriteCursorNormal]!, GetCursorAnim(app.PartyMenu!.Type, app.CurrPartySlot));
        SpriteSystem.Sprite_SetPositionXY(app.Sprites[SpriteCursorNormal]!, x, y);
    }

    // OFFSET 0x0207FA00
    private static int PartyMenu_HandleNavigationAndTouchScreen(PartyMenuApplication app)
    {
        if (PartyMenu_TryHandleDirectionPadInput(app))
        {
            return InputDirectionPad;
        }
        if (PartyMenu_TryHandleTouchScreenButtons(app))
        {
            return InputTouchScreen;
        }
        return InputNone;
    }

    // OFFSET 0x0207FA24
    private static bool PartyMenu_TryHandleDirectionPadInput(PartyMenuApplication app)
    {
        int keys = gSystem.PressedKeysRepeatable;
        byte direction;
        if ((keys & KeyUp) != 0)
        {
            direction = GridMenuCursor.DirectionUp;
        }
        else if ((keys & KeyDown) != 0)
        {
            direction = GridMenuCursor.DirectionDown;
        }
        else if ((keys & KeyLeft) != 0)
        {
            direction = GridMenuCursor.DirectionLeft;
        }
        else if ((keys & KeyRight) != 0)
        {
            direction = GridMenuCursor.DirectionRight;
        }
        else
        {
            direction = 4;
        }
        if (direction == 4)
        {
            return false;
        }
        byte x = 0;
        byte y = 0;
        byte newSlot;
        byte[] orders = RomData.Arm9(SwitchFromConfirmOrders, 6 * 4);
        if (app.CurrPartySlot == SlotConfirm)
        {
            if (direction == GridMenuCursor.DirectionUp)
            {
                newSlot = SelectFirstPresentSlot(app, ref x, ref y, orders, ((app.PrevPartySlot & 1) + 2) * 6);
            }
            else
            {
                newSlot = MoveCursorToPresentSlot(app, ref x, ref y, direction);
            }
        }
        else if (app.CurrPartySlot == SlotCancel)
        {
            int mode = app.PartyMenu!.Mode;
            if (mode != ModeSelectConfirm && mode != ModeBattleTower && mode != ModeBattleCastle && mode != ModeBattleHall && direction == GridMenuCursor.DirectionUp)
            {
                newSlot = SelectFirstPresentSlot(app, ref x, ref y, orders, ((app.PrevPartySlot & 1) + 2) * 6);
            }
            else if (direction == GridMenuCursor.DirectionDown)
            {
                newSlot = SelectFirstPresentSlot(app, ref x, ref y, orders, (app.PrevPartySlot & 1) * 6);
            }
            else
            {
                newSlot = MoveCursorToPresentSlot(app, ref x, ref y, direction);
            }
        }
        else
        {
            newSlot = MoveCursorToPresentSlot(app, ref x, ref y, direction);
        }
        if (newSlot == app.CurrPartySlot || newSlot == 0xFF)
        {
            return false;
        }
        if (newSlot == SlotConfirm || newSlot == SlotCancel)
        {
            Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteCursorNormal]!, 0);
        }
        else
        {
            Sprite.Sprite_SetAnim(app.Sprites[SpriteCursorNormal]!, GetCursorAnim(app.PartyMenu!.Type, newSlot));
            Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteCursorNormal]!, 1);
            SpriteSystem.Sprite_SetPositionXY(app.Sprites[SpriteCursorNormal]!, x, y);
        }
        byte oldSlot = app.CurrPartySlot;
        app.CurrPartySlot = newSlot;
        InitAnimAndPaletteForSlot(app, oldSlot, 0);
        InitAnimAndPaletteForSlot(app, app.CurrPartySlot, 1);
        SoundPlayback.Sound_PlayEffect(SeqConfirm);
        if (oldSlot < MaxPartySize)
        {
            DrawMemberTouchScreenButton(app, oldSlot, 0);
            Bg.Bg_ScheduleTilemapTransfer(app.BgConfig!, 4);
        }
        if (oldSlot != SlotConfirm && oldSlot != SlotCancel)
        {
            app.PrevPartySlot = oldSlot;
        }
        return true;
    }

    // OFFSET 0x0207FBE0
    private static byte MoveCursorToPresentSlot(PartyMenuApplication app, ref byte x, ref byte y, byte direction)
    {
        byte slot = app.CurrPartySlot;
        byte unusedX = 0;
        byte unusedY = 0;
        while (true)
        {
            slot = GridMenuCursor.GridMenuCursor_CheckNavigation(app.CursorPosTable!, ref x, ref y, ref unusedX, ref unusedY, slot, direction);
            if (slot == SlotConfirm || slot == SlotCancel || slot == 0xFF || PartyMenu_IsMemberPresent(app, slot) != 0)
            {
                return slot;
            }
        }
    }

    // OFFSET 0x0207FC30
    private static byte SelectFirstPresentSlot(PartyMenuApplication app, ref byte x, ref byte y, byte[] orders, int orderOffset)
    {
        byte unusedX = 0;
        byte unusedY = 0;
        for (int i = 0; i < MaxPartySize; i++)
        {
            if (PartyMenu_IsMemberPresent(app, orders[orderOffset + i]) != 0)
            {
                GridMenuCursor.GridMenuCursor_CheckNavigation(app.CursorPosTable!, ref x, ref y, ref unusedX, ref unusedY, orders[orderOffset + i], 4);
                return orders[orderOffset + i];
            }
        }
        GridMenuCursor.GridMenuCursor_CheckNavigation(app.CursorPosTable!, ref x, ref y, ref unusedX, ref unusedY, 0, 4);
        return 0;
    }

    // OFFSET 0x0207FC94
    private static bool PartyMenu_TryHandleTouchScreenButtons(PartyMenuApplication app)
    {
        int pressed = PartyMenu_GetTouchScreenPartyBallPressed(app);
        if (pressed == -1)
        {
            return false;
        }
        byte oldSlot = app.CurrPartySlot;
        app.CurrPartySlot = (byte)pressed;
        InitAnimAndPaletteForSlot(app, oldSlot, 0);
        InitAnimAndPaletteForSlot(app, app.CurrPartySlot, 1);
        if (oldSlot < MaxPartySize)
        {
            DrawMemberTouchScreenButton(app, oldSlot, 0);
        }
        byte x = 0;
        byte y = 0;
        byte unusedX = 0;
        byte unusedY = 0;
        GridMenuCursor.GridMenuCursor_CheckNavigation(app.CursorPosTable!, ref x, ref y, ref unusedX, ref unusedY, app.CurrPartySlot, 4);
        Sprite.Sprite_SetAnim(app.Sprites[SpriteCursorNormal]!, GetCursorAnim(app.PartyMenu!.Type, app.CurrPartySlot));
        Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteCursorNormal]!, 1);
        SpriteSystem.Sprite_SetPositionXY(app.Sprites[SpriteCursorNormal]!, x, y);
        app.TouchButtonState = 1;
        app.TouchButtonSlot = app.CurrPartySlot;
        SoundPlayback.Sound_PlayEffect(SeqTouchButton);
        if (oldSlot != SlotConfirm && oldSlot != SlotCancel)
        {
            app.PrevPartySlot = oldSlot;
        }
        return true;
    }

    // OFFSET 0x0207FD68
    public static void PartyMenu_MoveCursorToSlot(PartyMenuApplication app, byte slot)
    {
        if (slot == SlotConfirm || slot == SlotCancel)
        {
            Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteCursorNormal]!, 0);
        }
        else
        {
            byte x = 0;
            byte y = 0;
            byte unusedX = 0;
            byte unusedY = 0;
            GridMenuCursor.GridMenuCursor_CheckNavigation(app.CursorPosTable!, ref x, ref y, ref unusedX, ref unusedY, app.CurrPartySlot, 4);
            Sprite.Sprite_SetAnim(app.Sprites[SpriteCursorNormal]!, GetCursorAnim(app.PartyMenu!.Type, slot));
            Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteCursorNormal]!, 1);
            SpriteSystem.Sprite_SetPositionXY(app.Sprites[SpriteCursorNormal]!, x, y);
        }
        byte oldSlot = app.CurrPartySlot;
        app.CurrPartySlot = slot;
        InitAnimAndPaletteForSlot(app, oldSlot, 0);
        InitAnimAndPaletteForSlot(app, app.CurrPartySlot, 1);
        if (oldSlot < MaxPartySize)
        {
            DrawMemberTouchScreenButton(app, oldSlot, 0);
            Bg.Bg_ScheduleTilemapTransfer(app.BgConfig!, 4);
        }
    }

    // OFFSET 0x0207FE1C
    private static void UpdateTouchButtonPress(PartyMenuApplication app)
    {
        switch (app.TouchButtonState)
        {
            case 1:
                CalculateWindowPosition(app.TouchButtonSlot, out short x, out short y);
                PartyMenu_InitTouchButtonEffect(app, x, y);
                DrawMemberTouchScreenButton(app, app.TouchButtonSlot, 2);
                Bg.Bg_ScheduleTilemapTransfer(app.BgConfig!, 4);
                app.TouchButtonState++;
                break;
            case 2:
                DrawMemberTouchScreenButton(app, app.TouchButtonSlot, 1);
                Bg.Bg_ScheduleTilemapTransfer(app.BgConfig!, 4);
                app.TouchButtonState = 0;
                break;
        }
    }

    private static byte MemberIsEgg(PartyMenuApplication app, int slot)
    {
        return slot < MaxPartySize ? app.PartyMembers[slot].IsEgg : (byte)0;
    }

    // OFFSET 0x0207FE98
    private static int PartyMenu_HandleInput(PartyMenuApplication app)
    {
        if ((gSystem.PressedKeys & KeyA) != 0)
        {
            int slot = app.CurrPartySlot;
            if (slot == SlotConfirm)
            {
                return Input4;
            }
            if (slot == SlotCancel)
            {
                SoundPlayback.Sound_PlayEffect(SeqConfirm);
                if (!app.HideCancel)
                {
                    return InputCancel;
                }
            }
            else
            {
                int mode = app.PartyMenu!.Mode;
                if (mode == ModeSelectNoPrompt || mode == ModeNpcTrade)
                {
                    SoundPlayback.Sound_PlayEffect(SeqConfirm);
                    return InputConfirm;
                }
                if (mode == ModeFeedPoffin || mode == ModeMailbox)
                {
                    if (MemberIsEgg(app, slot) == 0)
                    {
                        SoundPlayback.Sound_PlayEffect(SeqConfirm);
                        return InputConfirm;
                    }
                    SoundPlayback.Sound_PlayEffect(SeqCannotSelect);
                    return InputNone;
                }
                if (mode == ModeBallSeal)
                {
                    if (MemberIsEgg(app, slot) == 0)
                    {
                        SoundPlayback.Sound_PlayEffect(SeqConfirm);
                        OpenContextMenu(app);
                        return InputConfirm;
                    }
                    SoundPlayback.Sound_PlayEffect(SeqCannotSelect);
                    return InputNone;
                }
                SoundPlayback.Sound_PlayEffect(SeqConfirm);
                OpenContextMenu(app);
                return InputConfirm;
            }
        }
        if ((gSystem.PressedKeys & KeyB) != 0 && !app.HideCancel)
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            app.CurrPartySlot = SlotCancel;
            return InputCancel;
        }
        int input = PartyMenu_HandleNavigationAndTouchScreen(app);
        if (input == InputTouchScreen)
        {
            int mode = app.PartyMenu!.Mode;
            if ((mode == ModeFeedPoffin || mode == ModeMailbox || mode == ModeBallSeal) && MemberIsEgg(app, app.CurrPartySlot) != 0)
            {
                SoundPlayback.Sound_PlayEffect(SeqCannotSelect);
                return InputNone;
            }
        }
        return input;
    }

    // OFFSET 0x0207FFC8
    private static void OpenContextMenu(PartyMenuApplication app)
    {
        RenderWindow.Window_EraseMessageBox(app.Windows[WinShortMessage], 1);
        byte[] entries = new byte[8];
        int count;
        switch (app.PartyMenu!.Mode)
        {
            case ModeField:
                count = GetContextMenuEntriesForPartyMon(app, entries);
                break;
            case ModeSelectConfirm:
                count = GetContextMenuEntries_SelectConfirm(app, entries);
                break;
            case ModeBallSeal:
                count = GetContextMenuEntries_BallSeal(app, entries);
                break;
            case ModeBattleTower:
                count = GetContextMenuEntries_BattleTower(app, entries);
                break;
            case ModeSelectEgg:
                count = GetContextMenuEntries_SelectEgg(app, entries);
                break;
            case ModeBattleHall:
                count = GetContextMenuEntries_BattleHall(app, entries);
                break;
            case ModeBattleCastle:
                count = GetContextMenuEntries_BattleCastle(app, entries);
                break;
            default:
                count = GetContextMenuEntries_Default(app, entries);
                break;
        }
        PartyMenu_DrawContextMenu(app, entries, count);
        PartyMenu_LoadContextMenuPrompt(app);
        PartyMenu_PrintMediumMessage(app, 0xFFFFFFFF, 1);
        SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 1);
    }

    // OFFSET 0x020800B4
    private static int GetContextMenuEntriesForPartyMon(PartyMenuApplication app, byte[] entries)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, app.CurrPartySlot);
        entries[0] = 1;
        int fieldMoves = 0;
        int count = 1;
        if (!BattleTowerSalon.FieldSystem_IsInBattleTowerSalon(app.PartyMenu.FieldSystem!))
        {
            if (app.PartyMembers[app.CurrPartySlot].IsEgg == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + i, null);
                    if (move == 0)
                    {
                        break;
                    }
                    byte index = GetFieldMoveIndex(move);
                    if (index != 0xFF)
                    {
                        entries[count++] = index;
                        PartyMenu_SetKnownFieldMove(app, move, fieldMoves);
                        fieldMoves++;
                    }
                }
                entries[count++] = 0;
                entries[count++] = (byte)(Item.Item_IsMail(app.PartyMembers[app.CurrPartySlot].HeldItem) == 1 ? 5 : 2);
            }
            else
            {
                entries[count++] = 0;
            }
        }
        entries[count++] = 9;
        return count;
    }

    // OFFSET 0x020801AC
    private static int GetContextMenuEntries_BallSeal(PartyMenuApplication app, byte[] entries)
    {
        entries[0] = 0xE;
        entries[1] = 9;
        return 2;
    }

    // OFFSET 0x020801B8
    private static int GetContextMenuEntries_BattleTower(PartyMenuApplication app, byte[] entries)
    {
        if (app.PartyMembers[app.CurrPartySlot].IsEgg == 0)
        {
            entries[0] = 8;
            entries[1] = 1;
            entries[2] = 9;
            return 3;
        }
        entries[0] = 1;
        entries[1] = 9;
        return 2;
    }

    // OFFSET 0x020801F0
    private static int GetContextMenuEntries_Default(PartyMenuApplication app, byte[] entries)
    {
        if (app.PartyMembers[app.CurrPartySlot].IsContestEligible)
        {
            entries[0] = 0xD;
            entries[1] = 1;
            entries[2] = 9;
            return 3;
        }
        entries[0] = 1;
        entries[1] = 9;
        return 2;
    }

    private static int EligibilityEntries(int eligibility, byte[] entries)
    {
        switch (eligibility)
        {
            case 0:
                entries[0] = 1;
                entries[1] = 9;
                return 2;
            case 1:
                entries[0] = 0xB;
                entries[1] = 1;
                entries[2] = 9;
                return 3;
            case 2:
                entries[0] = 0xC;
                entries[1] = 1;
                entries[2] = 9;
                return 3;
        }
        return 0;
    }

    // OFFSET 0x0208022C
    private static int GetContextMenuEntries_SelectConfirm(PartyMenuApplication app, byte[] entries)
    {
        return EligibilityEntries(PartyMenu_CheckEligibility(app, app.CurrPartySlot), entries);
    }

    // OFFSET 0x0208027C
    private static int GetContextMenuEntries_BattleHall(PartyMenuApplication app, byte[] entries)
    {
        return EligibilityEntries(PartyMenu_CheckBattleHallEligibility(app, app.CurrPartySlot), entries);
    }

    // OFFSET 0x020802CC
    private static int GetContextMenuEntries_BattleCastle(PartyMenuApplication app, byte[] entries)
    {
        return EligibilityEntries(PartyMenu_CheckBattleCastleEligibility(app, app.CurrPartySlot), entries);
    }

    // OFFSET 0x0208031C
    private static int GetContextMenuEntries_SelectEgg(PartyMenuApplication app, byte[] entries)
    {
        if (app.PartyMembers[app.CurrPartySlot].IsEgg == 1)
        {
            entries[0] = 1;
            entries[1] = 0xF;
            entries[2] = 9;
            return 3;
        }
        entries[0] = 1;
        entries[1] = 9;
        return 2;
    }

    private static bool IsInSelectionOrder(PartyMenu partyMenu, int slot)
    {
        int count = partyMenu.SelectionSlots >> 4;
        for (int i = 0; i < count; i++)
        {
            if (slot + 1 == partyMenu.SelectionOrder[i])
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x02080354
    public static int PartyMenu_CheckEligibility(PartyMenuApplication app, int slot)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        if (partyMenu.BattleRegulation != null)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, slot);
            if (!BattleRegulationCheck.BattleRegulation_CheckPokemon((BattleRegulation)partyMenu.BattleRegulation, mon, app.HeightWeight))
            {
                return 0;
            }
        }
        if (partyMenu.Mode == ModeBattleTower && Pokemon.Pokemon_IsOnBattleFrontierBanlist(app.PartyMembers[slot].Species))
        {
            return 0;
        }
        if (IsInSelectionOrder(partyMenu, slot))
        {
            return 2;
        }
        PartyMenuMember member = app.PartyMembers[slot];
        if (member.IsEgg == 1 || member.Level > partyMenu.RequiredLevel)
        {
            return 0;
        }
        return 1;
    }

    // OFFSET 0x02080404
    public static int PartyMenu_CheckBattleHallEligibility(PartyMenuApplication app, int slot)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        if (partyMenu.Mode == ModeBattleHall && Pokemon.Pokemon_IsOnBattleFrontierBanlist(app.PartyMembers[slot].Species))
        {
            return 0;
        }
        if (IsInSelectionOrder(partyMenu, slot))
        {
            return 2;
        }
        PartyMenuMember member = app.PartyMembers[slot];
        if (member.IsEgg == 1 || member.Level < partyMenu.RequiredLevel)
        {
            return 0;
        }
        return 1;
    }

    // OFFSET 0x02080488
    public static int PartyMenu_CheckBattleCastleEligibility(PartyMenuApplication app, int slot)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        if (partyMenu.Mode == ModeBattleCastle && Pokemon.Pokemon_IsOnBattleFrontierBanlist(app.PartyMembers[slot].Species))
        {
            return 0;
        }
        if (IsInSelectionOrder(partyMenu, slot))
        {
            return 2;
        }
        if (app.PartyMembers[slot].IsEgg == 1)
        {
            return 0;
        }
        return 1;
    }

    private static uint ToggleAnim(uint anim, int isSelected)
    {
        return isSelected == 0 ? anim & 2 : (anim & 2) + 1;
    }

    // OFFSET 0x02080500
    private static void InitAnimAndPaletteForSlot(PartyMenuApplication app, int slot, int isSelected)
    {
        if (slot == SlotConfirm)
        {
            byte anim = (byte)Sprite.Sprite_GetActiveAnim(app.Sprites[SpriteConfirmButton]!);
            Sprite.Sprite_SetAnim(app.Sprites[SpriteConfirmButton]!, (byte)ToggleAnim(anim, isSelected));
            return;
        }
        if (slot == SlotCancel)
        {
            byte anim = (byte)Sprite.Sprite_GetActiveAnim(app.Sprites[SpriteCancelButton]!);
            Sprite.Sprite_SetAnim(app.Sprites[SpriteCancelButton]!, (byte)ToggleAnim(anim, isSelected));
            return;
        }
        PartyMenuMember member = app.PartyMembers[slot];
        if (isSelected == 0)
        {
            member.SpriteXDelta -= 2;
            member.SpriteYDelta -= 2;
            Sprite.Sprite_SetAnim(app.Sprites[slot]!, 0);
        }
        else
        {
            member.SpriteXDelta += 2;
            member.SpriteYDelta += 2;
            Sprite.Sprite_SetAnim(app.Sprites[slot]!, 1);
        }
        PartyMenu_UpdateSlotPalette(app, slot);
    }

    // OFFSET 0x020805D0
    private static uint GetCursorAnim(int type, int slot)
    {
        if (slot == 0 || (type != 0 && slot == 1))
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020805E4
    private static int SelectSwitchSlot(PartyMenuApplication app)
    {
        if ((gSystem.PressedKeys & KeyA) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            if (app.CurrPartySlot >= MaxPartySize || app.CurrPartySlot == app.SelectTargetSlot)
            {
                PartyMenu_ResetCursor(app);
                return InputCancel;
            }
            PartyMenu_PrepareSwitch(app);
            return InputConfirm;
        }
        if ((gSystem.PressedKeys & KeyB) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            PartyMenu_ResetCursor(app);
            return InputCancel;
        }
        int input = PartyMenu_HandleNavigationAndTouchScreen(app);
        if (input == InputTouchScreen)
        {
            if (app.CurrPartySlot == app.SelectTargetSlot)
            {
                PartyMenu_ResetCursor(app);
                return InputCancel;
            }
            PartyMenu_PrepareSwitch(app);
            return InputConfirm;
        }
        return input;
    }

    private static int PrintSelectionError(PartyMenuApplication app, uint message)
    {
        PartyMenu_PrintLongMessage(app, message, 1);
        app.StateAfterMessage = StateWaitABPressChooseMons;
        SoundPlayback.Sound_PlayEffect(SeqCannotSelect);
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x02080670
    private static int HandleGameWindowEvent(PartyMenuApplication app)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        int minSlots = partyMenu.SelectionSlots & 0xF;
        for (int i = 0; i < minSlots; i++)
        {
            if (partyMenu.SelectionOrder[i] != 0)
            {
                continue;
            }
            switch (partyMenu.SelectionSlots >> 4)
            {
                case 2:
                    return PrintSelectionError(app, 0x6B);
                case 3:
                    return PrintSelectionError(app, 0x77);
                case 4:
                    return PrintSelectionError(app, 0x78);
                case 5:
                    return PrintSelectionError(app, 0x79);
                case 6:
                    return PrintSelectionError(app, 0x7A);
                default:
                    return PrintSelectionError(app, TextChooseAPokemon);
            }
        }
        if (partyMenu.BattleRegulation != null)
        {
            switch (BattleRegulationCheck.BattleRegulation_CheckParty((BattleRegulation)partyMenu.BattleRegulation, partyMenu.Party!, partyMenu.SelectionOrder, app.HeightWeight))
            {
                case 1:
                {
                    GameString format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, 0xB8)!;
                    StringTemplate.StringTemplate_SetNumber(app.Template!, 0, BattleRegulation.BattleRegulation_GetRuleValue((BattleRegulation)partyMenu.BattleRegulation, 3), 3, 0, 1);
                    StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, format);
                    GameString.String_Free(format);
                    return PrintSelectionError(app, 0xFFFFFFFF);
                }
                case 2:
                    return PrintSelectionError(app, 0xB6);
                case 3:
                    return PrintSelectionError(app, 0xB7);
            }
        }
        if (partyMenu.Mode == ModeBattleTower)
        {
            switch (CheckDuplicateValues(app))
            {
                case 1:
                    return PrintSelectionError(app, 0xB6);
                case 2:
                    return PrintSelectionError(app, 0xB7);
            }
        }
        if (partyMenu.Mode == ModeBattleHall && CheckUniqueValues(app) == 1)
        {
            return PrintSelectionError(app, 0xC9);
        }
        if (partyMenu.Mode == ModeBattleCastle && CheckEqualityInArray(app) == 1)
        {
            return PrintSelectionError(app, 0xB6);
        }
        partyMenu.MenuSelectionResult = 0;
        SoundPlayback.Sound_PlayEffect(SeqConfirm);
        return StateFadeOut;
    }

    private static PartyMenuMember OrderedMember(PartyMenuApplication app, int orderIndex)
    {
        return app.PartyMembers[app.PartyMenu!.SelectionOrder[orderIndex] - 1];
    }

    // OFFSET 0x020808CC
    private static int CheckDuplicateValues(PartyMenuApplication app)
    {
        byte[] order = app.PartyMenu!.SelectionOrder;
        for (int i = 0; i < 5; i++)
        {
            if (order[i] == 0)
            {
                return 0;
            }
            for (int j = i + 1; j < MaxPartySize; j++)
            {
                if (order[j] == 0)
                {
                    break;
                }
                PartyMenuMember first = OrderedMember(app, i);
                PartyMenuMember second = OrderedMember(app, j);
                if (first.Species == second.Species)
                {
                    return 1;
                }
                if (first.HeldItem != 0 && first.HeldItem == second.HeldItem)
                {
                    return 2;
                }
            }
        }
        return 0;
    }

    // OFFSET 0x02080968
    private static int CheckUniqueValues(PartyMenuApplication app)
    {
        byte[] order = app.PartyMenu!.SelectionOrder;
        for (int i = 0; i < 5; i++)
        {
            if (order[i] == 0)
            {
                return 0;
            }
            for (int j = i + 1; j < MaxPartySize; j++)
            {
                if (order[j] == 0)
                {
                    break;
                }
                if (OrderedMember(app, i).Species != OrderedMember(app, j).Species)
                {
                    return 1;
                }
            }
        }
        return 0;
    }

    // OFFSET 0x020809DC
    private static int CheckEqualityInArray(PartyMenuApplication app)
    {
        byte[] order = app.PartyMenu!.SelectionOrder;
        for (int i = 0; i < 5; i++)
        {
            if (order[i] == 0)
            {
                return 0;
            }
            for (int j = i + 1; j < MaxPartySize; j++)
            {
                if (order[j] == 0)
                {
                    break;
                }
                if (OrderedMember(app, i).Species == OrderedMember(app, j).Species)
                {
                    return 1;
                }
            }
        }
        return 0;
    }

    // OFFSET 0x02080A50
    private static void DrawMemberTouchScreenButton(PartyMenuApplication app, int slot, int anim)
    {
        byte[] tilemap = Bg.Bg_GetTilemapBuffer(app.BgConfig!, 4)!;
        ushort baseTile = (ushort)(anim * 25 + 0x1005);
        byte[] coords = RomData.Arm9(TouchButtonCoords, 0x10);
        int x = coords[slot * 2];
        int y = coords[slot * 2 + 1];
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                int index = ((y + row) * 32 + x + col) * 2;
                ushort tile = (ushort)(baseTile + row * 5 + col);
                tilemap[index] = (byte)tile;
                tilemap[index + 1] = (byte)(tile >> 8);
            }
        }
    }

    // OFFSET 0x02080AB8
    private static void CalculateWindowPosition(int slot, out short x, out short y)
    {
        byte[] coords = RomData.Arm9(TouchButtonCoords, 0x10);
        x = (short)(coords[slot * 2] * 8 + 0x14);
        y = (short)(coords[slot * 2 + 1] * 8 + 0x14);
    }

    // OFFSET 0x02080AD8
    private static int PartyMenu_GetTouchScreenPartyBallPressed(PartyMenuApplication app)
    {
        if (!ShouldShowSubscreen(app))
        {
            return -1;
        }
        int pressed = TouchScreen.TouchScreen_CheckRectanglePressed(ReadTouchRects(PartyBallTouchRects));
        if (pressed != -1)
        {
            ushort[] mask = { 0xFFFE };
            if (Bg.Bg_DoesPixelAtXYMatchVal(app.BgConfig!, 4, gSystem.TouchX, gSystem.TouchY, mask) == 0)
            {
                return -1;
            }
        }
        return pressed;
    }

    // OFFSET 0x02080B34
    private static int HandleWindowInputEvent(PartyMenuApplication app, ref int state)
    {
        uint selection = Menu.Menu_ProcessInput(app.ContextMenu!);
        if (selection == Menu.Cancel)
        {
            RenderWindow.Window_EraseMessageBox(app.Windows[WinMediumMessage], 1);
            PartyMenu_ClearContextWindow(app);
            int mode = app.PartyMenu!.Mode;
            if (mode == ModeSelectConfirm || mode == ModeBattleTower || mode == ModeBattleHall || mode == ModeBattleCastle)
            {
                PartyMenu_PrintShortMessage(app, TextChooseMonAndConfirm, 1);
            }
            else if (mode == ModeSelectEgg)
            {
                PartyMenu_PrintShortMessage(app, TextSelectEgg, 1);
            }
            else
            {
                PartyMenu_PrintShortMessage(app, TextChooseAPokemon, 1);
            }
            SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 0);
            return 1;
        }
        if (selection == Menu.NoChoice)
        {
            return 0;
        }
        PartyMenuContextAction(selection)(app, out state);
        return 0;
    }

    // OFFSET 0x02080BCC
    private static byte GetFieldMoveIndex(ushort move)
    {
        ushort[] moves = RomData.Arm9U16(FieldMoveTable, FieldMoveCount);
        for (int i = 0; i < FieldMoveCount; i++)
        {
            if (move == moves[i])
            {
                return (byte)(i + 0x10);
            }
        }
        return 0xFF;
    }

    private static bool HandleHPTransferTargetResult(PartyMenuApplication app, int result, out int state)
    {
        state = StateHPTransferFieldMove;
        switch (result)
        {
            case 0:
            {
                SoundPlayback.Sound_PlayEffect(SeqHPTransfer);
                SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 1);
                PartyMenuMember member = app.PartyMembers[app.CurrPartySlot];
                int missing = member.MaxHP - member.CurHP;
                if (missing < app.MonStats[HPTransferBuffer])
                {
                    app.MonStats[HPTransferBuffer] = (ushort)missing;
                }
                app.MonStats[HPTransferState] = 2;
                app.MonStats[HPTransferCounter] = 0;
                return false;
            }
            case 1:
                SoundPlayback.Sound_PlayEffect(SeqConfirm);
                app.MonStats[HPTransferState] = 1;
                state = StateShowMessageThenNextState;
                return true;
            case 2:
                SoundPlayback.Sound_PlayEffect(SeqCannotSelect);
                state = StateHPTransferFieldMove;
                return true;
        }
        return false;
    }

    // OFFSET 0x02080BF4
    private static int PartyMenu_UseHPTransferFieldMove(PartyMenuApplication app)
    {
        int state;
        switch (app.MonStats[HPTransferState])
        {
            case 0:
                if ((gSystem.PressedKeys & KeyA) != 0)
                {
                    if (app.CurrPartySlot >= MaxPartySize)
                    {
                        SoundPlayback.Sound_PlayEffect(SeqConfirm);
                        PartyMenu_ResetCursor(app);
                        return StateDefault;
                    }
                    if (HandleHPTransferTargetResult(app, CheckCanUseHPTransferFieldMove(app), out state))
                    {
                        return state;
                    }
                }
                if ((gSystem.PressedKeys & KeyB) != 0)
                {
                    SoundPlayback.Sound_PlayEffect(SeqConfirm);
                    PartyMenu_ResetCursor(app);
                    return StateDefault;
                }
                if (PartyMenu_HandleNavigationAndTouchScreen(app) == InputTouchScreen)
                {
                    if (app.CurrPartySlot == app.SelectTargetSlot)
                    {
                        PartyMenu_ResetCursor(app);
                        return StateDefault;
                    }
                    if (HandleHPTransferTargetResult(app, CheckCanUseHPTransferFieldMove(app), out state))
                    {
                        return state;
                    }
                }
                break;
            case 1:
                if ((gSystem.PressedKeys & (KeyA | KeyB)) != 0)
                {
                    SoundPlayback.Sound_PlayEffect(SeqConfirm);
                    RenderWindow.Window_EraseMessageBox(app.Windows[WinLongMessage], 1);
                    SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 0);
                    PartyMenu_PrintShortMessage(app, 0x24, 1);
                    app.MonStats[HPTransferState] = 0;
                }
                break;
            case 2:
                if (PartyMenu_HPTransferUpdateHP(app, app.SelectTargetSlot, -1))
                {
                    SoundPlayback.Sound_PlayEffect(SeqHPTransfer);
                    app.MonStats[HPTransferState] = 3;
                    app.MonStats[HPTransferCounter] = 0;
                }
                break;
            case 3:
                if (PartyMenu_HPTransferUpdateHP(app, app.CurrPartySlot, 1))
                {
                    Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, app.CurrPartySlot);
                    GameString format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, 0x40)!;
                    StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
                    StringTemplate.StringTemplate_SetNumber(app.Template!, 1, app.MonStats[HPTransferCounter], 3, 0, 1);
                    StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, format);
                    GameString.String_Free(format);
                    PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
                    JournalLocationEvent entry = Journal.JournalEntry_CreateEventUsedMove((byte)app.MonStats[HPTransferJournalMove], 0, HeapID);
                    Journal.JournalEntry_SaveData(app.PartyMenu.FieldSystem!.JournalEntry, entry, 1);
                    app.MonStats[HPTransferState] = 4;
                    app.StateAfterMessage = StateHPTransferFieldMove;
                    return StateShowMessageThenNextState;
                }
                break;
            case 4:
                if ((gSystem.PressedKeys & (KeyA | KeyB)) != 0)
                {
                    SoundPlayback.Sound_PlayEffect(SeqConfirm);
                    RenderWindow.Window_EraseMessageBox(app.Windows[WinLongMessage], 1);
                    SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 0);
                    PartyMenu_ResetCursor(app);
                    return StateDefault;
                }
                break;
        }
        return StateHPTransferFieldMove;
    }

    // OFFSET 0x02080ECC
    private static int CheckCanUseHPTransferFieldMove(PartyMenuApplication app)
    {
        PartyMenuMember member = app.PartyMembers[app.CurrPartySlot];
        if (member.IsEgg != 0)
        {
            return 2;
        }
        if (app.CurrPartySlot == app.SelectTargetSlot || member.CurHP == 0 || member.CurHP == member.MaxHP)
        {
            SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 1);
            PartyMenu_PrintLongMessage(app, 0x83, 1);
            app.MonStats[HPTransferState] = 1;
            app.StateAfterMessage = StateHPTransferFieldMove;
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02080F3C
    private static bool PartyMenu_HPTransferUpdateHP(PartyMenuApplication app, int slot, int delta)
    {
        PartyMenuMember member = app.PartyMembers[slot];
        member.CurHP = (ushort)(member.CurHP + (ushort)(short)delta);
        app.MonStats[HPTransferCounter]++;
        PartyMenu_ClearMemberHP(app, slot);
        Bg.Window_FillTilemap(app.Windows[slot * WinsPerMember + WinHealthbarOffset], 0);
        PartyMenu_PrintMemberCurrentHP(app, slot);
        PartyMenu_DrawMemberHealthbar(app, slot);
        if (app.MonStats[HPTransferBuffer] == app.MonStats[HPTransferCounter] || member.CurHP == member.MaxHP)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, slot);
            uint hp = member.CurHP;
            Pokemon.Pokemon_SetValue(mon, MonDataCurrentHP, hp);
            return true;
        }
        return false;
    }

    // OFFSET 0x02080FD0
    private static int HandleSpecialInput(PartyMenuApplication app)
    {
        if ((gSystem.PressedKeys & KeyA) != 0)
        {
            if (app.CurrPartySlot == SlotCancel)
            {
                if (app.HideCancel)
                {
                    return InputNone;
                }
                SoundPlayback.Sound_PlayEffect(SeqConfirm);
                return InputCancel;
            }
            if (MemberIsEgg(app, app.CurrPartySlot) == 0)
            {
                SoundPlayback.Sound_PlayEffect(SeqConfirm);
                return InputConfirm;
            }
            SoundPlayback.Sound_PlayEffect(SeqCannotSelect);
            return InputNone;
        }
        if ((gSystem.PressedKeys & KeyB) != 0)
        {
            if (app.HideCancel)
            {
                return InputNone;
            }
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            app.CurrPartySlot = SlotCancel;
            return InputCancel;
        }
        int input = PartyMenu_HandleNavigationAndTouchScreen(app);
        if (input == InputTouchScreen && MemberIsEgg(app, app.CurrPartySlot) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqCannotSelect);
            return InputNone;
        }
        return input;
    }
}
