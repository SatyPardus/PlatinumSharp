using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public static partial class PartyMenus
{
    private const int SeqSwitchSlide = 0x6A0;
    private const int ExitCodeGiveItem = 3;
    private const int ExitCodeReadMail = 7;
    private const int ExitCodeCut = 0xB;
    private const int ExitCodeFly = 0xC;
    private const int ExitCodeSurf = 0xD;
    private const int ExitCodeStrength = 0xE;
    private const int ExitCodeDefog = 0xF;
    private const int ExitCodeRockSmash = 0x10;
    private const int ExitCodeWaterfall = 0x11;
    private const int ExitCodeRockClimb = 0x12;
    private const int ExitCodeFlash = 0x13;
    private const int ExitCodeTeleport = 0x14;
    private const int ExitCodeDig = 0x15;
    private const int ExitCodeSweetScent = 0x16;
    private const int ExitCodeChatter = 0x17;
    private const int JournalMoveMilkDrink = 0xD;
    private const int JournalMoveSoftboiled = 0xE;

    private static PartyMenuAction PartyMenuContextAction(uint address)
    {
        switch (address & ~1u)
        {
            case 0x02083AD0:
                return PartyMenu_SelectSwitch;
            case 0x0208472C:
                return PartyMenu_SelectSummary;
            case 0x020833BC:
                return PartyMenu_SelectItem;
            case 0x0208347C:
                return PartyMenu_SelectItemGive;
            case 0x020834B0:
                return PartyMenu_SelectItemTake;
            case 0x02083700:
                return PartyMenu_SelectMail;
            case 0x020837C0:
                return PartyMenu_SelectMailRead;
            case 0x020837F4:
                return PartyMenu_SelectMailTake;
            case 0x020839FC:
                return PartyMenu_SelectBallSeal;
            case 0x020844B0:
                return PartyMenu_SelectChooseWithLimit;
            case 0x020845E8:
                return PartyMenu_SelectRemoveFromSelection;
            case 0x020846CC:
                return PartyMenu_SelectEnter;
            case 0x020846FC:
                return PartyMenu_SelectConfirm;
            case 0x02084760:
                return PartyMenu_SelectSetBallCapsule;
            case 0x020848A8:
                return PartyMenu_SelectCut;
            case 0x020848C0:
                return PartyMenu_SelectRockSmash;
            case 0x020848D8:
                return PartyMenu_SelectStrength;
            case 0x020848F0:
                return PartyMenu_SelectSurf;
            case 0x02084908:
                return PartyMenu_SelectDefog;
            case 0x02084920:
                return PartyMenu_SelectRockClimb;
            case 0x02084938:
                return PartyMenu_SelectFly;
            case 0x02084950:
                return PartyMenu_SelectWaterfall;
            case 0x02084968:
                return PartyMenu_SelectFlash;
            case 0x02084980:
                return PartyMenu_SelectTeleport;
            case 0x02084998:
                return PartyMenu_SelectDig;
            case 0x020849B0:
                return PartyMenu_SelectSweetScent;
            case 0x020849C8:
                return PartyMenu_SelectChatter;
            case 0x020849E0:
                return PartyMenu_SelectMilkDrink;
            case 0x020849FC:
                return PartyMenu_SelectSoftboiled;
        }
        throw new InvalidOperationException($"Unknown party menu action 0x{address:X8}");
    }

    private static void OpenSubMenu(PartyMenuApplication app, uint prompt, int first, int second)
    {
        PartyMenu_ClearContextWindow(app);
        PartyMenu_PrintMediumMessage(app, prompt, 0);
        app.ContextMenuChoices = StringList.StringList_New(3, HeapID);
        StringList.StringList_AddFromString(app.ContextMenuChoices, app.MenuStrings[first], PartyMenu_GetAction(first));
        StringList.StringList_AddFromString(app.ContextMenuChoices, app.MenuStrings[second], PartyMenu_GetAction(second));
        StringList.StringList_AddFromString(app.ContextMenuChoices, app.MenuStrings[9], PartyMenu_GetAction(9));
        MenuTemplate template = new()
        {
            Choices = app.ContextMenuChoices,
            Window = app.Windows[WinGiveItemOrMail],
            FontID = 0,
            XSize = 1,
            YSize = 3,
            LineSpacing = 0,
            SuppressCursor = 0,
            LoopAround = 0,
        };
        RenderWindow.Window_DrawStandardFrame(app.Windows[WinGiveItemOrMail], 1, 1, 0xE);
        app.ContextMenu = Menu.Menu_NewAndCopyToVRAM(template, 8, 0, 0, HeapID, KeyB);
    }

    private static void FreeContextMenu(PartyMenuApplication app)
    {
        Menu.Menu_Free(app.ContextMenu!, out _);
        StringList.StringList_Free(app.ContextMenuChoices!);
    }

    private static int ReturnToPartyMenu(PartyMenuApplication app)
    {
        RenderWindow.Window_EraseMessageBox(app.Windows[WinLongMessage], 1);
        PartyMenu_PrintShortMessage(app, TextChooseAPokemon, 1);
        SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 0);
        return StateDefault;
    }

    // OFFSET 0x020833BC
    private static void PartyMenu_SelectItem(PartyMenuApplication app, out int state)
    {
        OpenSubMenu(app, 0x26, 3, 4);
        state = StateHandleContextMenuInput;
    }

    // OFFSET 0x0208347C
    private static void PartyMenu_SelectItemGive(PartyMenuApplication app, out int state)
    {
        FreeContextMenu(app);
        app.PartyMenu!.MenuSelectionResult = ExitCodeGiveItem;
        state = StateFadeOut;
    }

    // OFFSET 0x020834B0
    private static void PartyMenu_SelectItemTake(PartyMenuApplication app, out int state)
    {
        int nextState = StatePrinting;
        RenderWindow.Window_EraseStandardFrame(app.Windows[WinGiveItemOrMail], 1);
        FreeContextMenu(app);
        PartyMenu partyMenu = app.PartyMenu!;
        PartyMenuMember member = app.PartyMembers[app.CurrPartySlot];
        ushort heldItem = member.HeldItem;
        FieldSystem? fieldSystem = partyMenu.FieldSystem;
        if (heldItem == 0)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x51, app.TmpFormat!);
            StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
            StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, app.TmpFormat!);
        }
        else if (BagItems.Bag_TryAddItem(partyMenu.Bag!, heldItem, 1, HeapID))
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
            Pokemon.Pokemon_SetValue(mon, MonDataHeldItem, 0u);
            Pokemon.Pokemon_SetArceusForm(mon);
            if (fieldSystem == null || CurrentMapID(fieldSystem) < MapGiratinaFormLockFirst || CurrentMapID(fieldSystem) > MapGiratinaFormLockLast)
            {
                int form = Pokemon.Pokemon_SetGiratinaFormByHeldItem(mon);
                if (member.HeldItem == ItemGriseousOrb && form == 0)
                {
                    nextState = StatePrintingFormChange;
                }
            }
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x52, app.TmpFormat!);
            StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
            StringTemplate.StringTemplate_SetItemName(app.Template!, 1, member.HeldItem);
            StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, app.TmpFormat!);
            member.HeldItem = 0;
            PartyMenu_DrawMemberHeldItem(app, app.CurrPartySlot, member.HeldItem);
        }
        else
        {
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x53, app.TmpString!);
        }
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(app.Windows[WinLongMessage], 1, 0xA, 0xF);
        Bg.Window_FillTilemap(app.Windows[WinLongMessage], 0xF);
        PartyMenu_AddLongMessagePrinter(app);
        state = nextState;
    }

    // OFFSET 0x02083658
    private static int PartyMenu_WaitForPrinter(PartyMenuApplication app)
    {
        if (ABPressedAfterPrinting(app))
        {
            return ReturnToPartyMenu(app);
        }
        return StatePrinting;
    }

    // OFFSET 0x020836A8
    private static int PartyMenu_WaitForPrinterThenFormChange(PartyMenuApplication app)
    {
        if (ABPressedAfterPrinting(app))
        {
            RenderWindow.Window_EraseMessageBox(app.Windows[WinLongMessage], 1);
            PartyMenu_SetupFormChangeAnim(app);
            return StateFormChange;
        }
        return StatePrintingFormChange;
    }

    // OFFSET 0x020836E4
    private static int PartyMenu_ChangeForm(PartyMenuApplication app)
    {
        if (PartyMenuFormChanges.PartyMenuFormChange_ChangeForm(app))
        {
            PartyMenu_TeardownFormChangeAnim(app);
            return StatePrinting;
        }
        return StateFormChange;
    }

    // OFFSET 0x02083700
    private static void PartyMenu_SelectMail(PartyMenuApplication app, out int state)
    {
        OpenSubMenu(app, 0x27, 6, 7);
        state = StateHandleContextMenuInput;
    }

    // OFFSET 0x020837C0
    private static void PartyMenu_SelectMailRead(PartyMenuApplication app, out int state)
    {
        FreeContextMenu(app);
        app.PartyMenu!.MenuSelectionResult = ExitCodeReadMail;
        state = StateFadeOut;
    }

    // OFFSET 0x020837F4
    private static void PartyMenu_SelectMailTake(PartyMenuApplication app, out int state)
    {
        RenderWindow.Window_EraseStandardFrame(app.Windows[WinGiveItemOrMail], 1);
        FreeContextMenu(app);
        PartyMenu_PrintLongMessage(app, 0x2B, 1);
        app.OnYes = PartyMenuCB_TakeMail_Transfer;
        app.OnNo = PartyMenuCB_TakeMail_PromptRemoval;
        app.StateAfterMessage = StateDrawYesNoChoice;
        state = StateShowMessageThenNextState;
    }

    // OFFSET 0x0208384C
    private static int PartyMenuCB_TakeMail_Transfer(PartyMenuApplication app)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, app.CurrPartySlot);
        if (Mailbox.Mailbox_MoveMailFromPokemon(app.PartyMenu.Mailbox!, mon, HeapID) != -1)
        {
            app.PartyMembers[app.CurrPartySlot].HeldItem = 0;
            PartyMenu_DrawMemberHeldItem(app, app.CurrPartySlot, app.PartyMembers[app.CurrPartySlot].HeldItem);
            PartyMenu_PrintLongMessage(app, 0x2E, 0);
        }
        else
        {
            PartyMenu_PrintLongMessage(app, 0x32, 0);
        }
        app.StateAfterMessage = StateWaitABPress2;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x020838C4
    private static int PartyMenuCB_TakeMail_PromptRemoval(PartyMenuApplication app)
    {
        PartyMenu_PrintLongMessage(app, 0x2F, 0);
        app.OnYes = PartyMenuCB_TakeMail_Remove;
        app.OnNo = PartyMenuCB_TakeMail_Cancel;
        app.StateAfterMessage = StateDrawYesNoChoice;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x020838F4
    private static int PartyMenuCB_TakeMail_Remove(PartyMenuApplication app)
    {
        PartyMenuMember member = app.PartyMembers[app.CurrPartySlot];
        if (BagItems.Bag_TryAddItem(app.PartyMenu!.Bag!, member.HeldItem, 1, HeapID))
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu.Party!, app.CurrPartySlot);
            Pokemon.Pokemon_SetValue(mon, MonDataHeldItem, 0u);
            Pokemon.Pokemon_SetArceusForm(mon);
            Pokemon.Pokemon_SetGiratinaFormByHeldItem(mon);
            member.HeldItem = 0;
            PartyMenu_DrawMemberHeldItem(app, app.CurrPartySlot, member.HeldItem);
            PartyMenu_PrintLongMessage(app, 0x33, 0);
        }
        else
        {
            PartyMenu_PrintLongMessage(app, 0x53, 0);
        }
        app.StateAfterMessage = StateWaitABPress2;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x02083990
    private static int PartyMenuCB_TakeMail_Cancel(PartyMenuApplication app)
    {
        return ReturnToPartyMenu(app);
    }

    // OFFSET 0x020839BC
    private static int PartyMenu_WaitABPress2(PartyMenuApplication app)
    {
        if ((gSystem.PressedKeys & (KeyA | KeyB)) != 0)
        {
            return ReturnToPartyMenu(app);
        }
        return StateWaitABPress2;
    }

    // OFFSET 0x020839FC
    private static void PartyMenu_SelectBallSeal(PartyMenuApplication app, out int state)
    {
        if (app.PartyMembers[app.CurrPartySlot].BallSeal == 0)
        {
            FreeContextMenu(app);
            app.PartyMenu!.MenuSelectionResult = 0;
            state = StateFadeOut;
            return;
        }
        PartyMenu_ClearContextWindow(app);
        PartyMenu_PrintLongMessage(app, 0xC3, 1);
        app.OnYes = PartyMenuCB_RemoveBallCapsule_Yes;
        app.OnNo = PartyMenuCB_RemoveBallCapsule_No;
        app.StateAfterMessage = StateDrawYesNoChoice;
        state = StateShowMessageThenNextState;
    }

    // OFFSET 0x02083A78
    private static int PartyMenuCB_RemoveBallCapsule_Yes(PartyMenuApplication app)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, app.CurrPartySlot);
        Pokemon.Pokemon_RemoveBallCapsule(mon);
        app.PartyMenu.MenuSelectionResult = 0;
        return StateFadeOut;
    }

    // OFFSET 0x02083AA4
    private static int PartyMenuCB_RemoveBallCapsule_No(PartyMenuApplication app)
    {
        return ReturnToPartyMenu(app);
    }

    private static void ShowSwitchCursor(PartyMenuApplication app)
    {
        app.InTargetSlotMode = true;
        app.SelectTargetSlot = (byte)(app.CurrPartySlot & 0x3F);
        SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 0);
        SpriteSystem.Sprite_GetPositionXY(app.Sprites[SpriteCursorNormal]!, out short x, out short y);
        SpriteSystem.Sprite_SetPositionXY(app.Sprites[SpriteCursorSwitch]!, x, y);
        Sprite.Sprite_SetAnim(app.Sprites[SpriteCursorSwitch]!, GetCursorAnim(app.PartyMenu!.Type, app.SelectTargetSlot) + 2);
        Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteCursorSwitch]!, 1);
        PartyMenu_UpdateSlotPalette(app, app.SelectTargetSlot);
    }

    // OFFSET 0x02083AD0
    private static void PartyMenu_SelectSwitch(PartyMenuApplication app, out int state)
    {
        ShowSwitchCursor(app);
        RenderWindow.Window_EraseMessageBox(app.Windows[WinMediumMessage], 1);
        PartyMenu_ClearContextWindow(app);
        PartyMenu_PrintShortMessage(app, 0x1E, 1);
        state = StateSelectSwitchSlot;
    }

    // OFFSET 0x02083B88
    private static void PartyMenu_ResetCursor(PartyMenuApplication app)
    {
        app.InTargetSlotMode = false;
        Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteCursorSwitch]!, 0);
        if (app.CurrPartySlot < MaxPartySize)
        {
            PartyMenu_UpdateSlotPalette(app, app.CurrPartySlot);
        }
        PartyMenu_UpdateSlotPalette(app, app.SelectTargetSlot);
        PartyMenu_PrintShortMessage(app, TextChooseAPokemon, 1);
    }

    // OFFSET 0x02083BD4
    private static void PartyMenu_PrepareSwitch(PartyMenuApplication app)
    {
        PartyOrderSwitchData orderSwitch = app.OrderSwitch;
        orderSwitch.Slots[0] = app.CurrPartySlot;
        orderSwitch.Slots[1] = app.SelectTargetSlot;
        orderSwitch.InProgress = 1;
        orderSwitch.ScrollTicker = 0;
        orderSwitch.State = 0;
        orderSwitch.Sides[0] = (byte)(orderSwitch.Slots[0] & 1);
        orderSwitch.Sides[1] = (byte)(orderSwitch.Slots[1] & 1);
        byte[] panels = Bg.Bg_GetTilemapBuffer(app.BgConfig!, 2)!;
        byte[] windows = Bg.Bg_GetTilemapBuffer(app.BgConfig!, 1)!;
        PartyMenuMember first = app.PartyMembers[orderSwitch.Slots[0]];
        PartyMenuMember second = app.PartyMembers[orderSwitch.Slots[1]];
        for (int row = 0; row < 6; row++)
        {
            int firstOffset = ((first.PanelYPos + row) * 32 + first.PanelXPos) * 2;
            Array.Copy(panels, firstOffset, orderSwitch.BufPanels[0], row * 0x20, 0x20);
            Array.Copy(windows, firstOffset, orderSwitch.BufWindows[0], row * 0x20, 0x20);
            int secondOffset = ((second.PanelYPos + row) * 32 + second.PanelXPos) * 2;
            Array.Copy(panels, secondOffset, orderSwitch.BufPanels[1], row * 0x20, 0x20);
            Array.Copy(windows, secondOffset, orderSwitch.BufWindows[1], row * 0x20, 0x20);
        }
        Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteCursorNormal]!, 0);
        Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteCursorSwitch]!, 0);
    }

    // OFFSET 0x02083D1C
    private static bool PartyMenu_DoSwitch(PartyMenuApplication app)
    {
        PartyOrderSwitchData orderSwitch = app.OrderSwitch;
        switch (orderSwitch.State)
        {
            case 0:
                SoundPlayback.Sound_PlayEffect(SeqSwitchSlide);
                orderSwitch.State = 1;
                break;
            case 1:
                orderSwitch.ScrollTicker++;
                UpdateBGScroll(app, 0);
                UpdateBGScroll(app, 1);
                UpdateSpriteScroll(app, 0, orderSwitch.Sides[0]);
                UpdateSpriteScroll(app, 1, orderSwitch.Sides[1]);
                Bg.Bg_ScheduleTilemapTransfer(app.BgConfig!, 2);
                Bg.Bg_ScheduleTilemapTransfer(app.BgConfig!, 1);
                if (orderSwitch.ScrollTicker == 0x10)
                {
                    orderSwitch.State = 2;
                }
                break;
            case 2:
                SwapSwitchedSlots(app);
                SoundPlayback.Sound_PlayEffect(SeqSwitchSlide);
                orderSwitch.State = 3;
                break;
            case 3:
                orderSwitch.ScrollTicker--;
                UpdateBGScroll(app, 0);
                UpdateBGScroll(app, 1);
                UpdateSpriteScroll(app, 0, orderSwitch.Sides[0] ^ 1);
                UpdateSpriteScroll(app, 1, orderSwitch.Sides[1] ^ 1);
                Bg.Bg_ScheduleTilemapTransfer(app.BgConfig!, 2);
                Bg.Bg_ScheduleTilemapTransfer(app.BgConfig!, 1);
                if (orderSwitch.ScrollTicker == 0)
                {
                    orderSwitch.State = 4;
                }
                break;
            case 4:
                Party.Party_SwapSlots(app.PartyMenu!.Party!, orderSwitch.Slots[0], orderSwitch.Slots[1]);
                Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteCursorNormal]!, 1);
                orderSwitch.InProgress = 0;
                app.InTargetSlotMode = false;
                PartyMenu_UpdateSlotPalette(app, app.CurrPartySlot);
                PartyMenu_UpdateSlotPalette(app, app.SelectTargetSlot);
                PartyMenu_PrintShortMessage(app, TextChooseAPokemon, 0);
                return true;
        }
        return false;
    }

    // OFFSET 0x02083E8C
    private static void UpdateBGScroll(PartyMenuApplication app, int index)
    {
        PartyOrderSwitchData orderSwitch = app.OrderSwitch;
        PartyMenuMember member = app.PartyMembers[orderSwitch.Slots[index]];
        byte x = (byte)member.PanelXPos;
        byte y = (byte)member.PanelYPos;
        Bg.Bg_FillTilemapRect(app.BgConfig!, 2, 0, x, y, 0x10, 6, 0x10);
        Bg.Bg_FillTilemapRect(app.BgConfig!, 1, 0, x, y, 0x10, 6, 0x10);
        int ticker = orderSwitch.ScrollTicker;
        if (orderSwitch.Sides[index] == 0)
        {
            Bg.Bg_CopyToTilemapRect(app.BgConfig!, 2, x, y, 0x10 - ticker, 6, orderSwitch.BufPanels[index], ticker, 0, 0x10, 6);
            Bg.Bg_CopyToTilemapRect(app.BgConfig!, 1, x, y, 0x10 - ticker, 6, orderSwitch.BufWindows[index], ticker, 0, 0x10, 6);
        }
        else
        {
            Bg.Bg_CopyToTilemapRect(app.BgConfig!, 2, (byte)(x + ticker), y, 0x10 - ticker, 6, orderSwitch.BufPanels[index], 0, 0, 0x10, 6);
            Bg.Bg_CopyToTilemapRect(app.BgConfig!, 1, (byte)(x + ticker), y, 0x10 - ticker, 6, orderSwitch.BufWindows[index], 0, 0, 0x10, 6);
        }
    }

    // OFFSET 0x02083FDC
    private static void UpdateSpriteScroll(PartyMenuApplication app, int index, int direction)
    {
        int slot = app.OrderSwitch.Slots[index];
        PartyMenuMember member = app.PartyMembers[slot];
        SpriteSystem.Sprite_GetPositionXY(app.Sprites[slot]!, out short ballX, out short ballY);
        int delta = direction == 0 ? -8 : 8;
        member.SpriteXDelta = (short)(member.SpriteXDelta + delta);
        member.StatusXPos = (short)(member.StatusXPos + delta);
        member.ItemXPos = (short)(member.ItemXPos + delta);
        ballX = (short)(ballX + delta);
        SpriteSystem.Sprite_SetPositionXY(member.Sprite!, member.SpriteXDelta, member.SpriteYDelta);
        SpriteSystem.Sprite_SetPositionXY(app.Sprites[SpriteStatusIconBase + slot]!, member.StatusXPos, member.StatusYPos);
        SpriteSystem.Sprite_SetPositionXY(app.Sprites[SpriteHeldItemBase + slot]!, member.ItemXPos, member.ItemYPos);
        SpriteSystem.Sprite_SetPositionXY(app.Sprites[SpriteBallSealBase + slot]!, (short)(member.ItemXPos + 8), member.ItemYPos);
        SpriteSystem.Sprite_SetPositionXY(app.Sprites[slot]!, ballX, ballY);
    }

    // OFFSET 0x02084134
    private static void SwapSwitchedSlots(PartyMenuApplication app)
    {
        PartyOrderSwitchData orderSwitch = app.OrderSwitch;
        int firstSlot = orderSwitch.Slots[0];
        int secondSlot = orderSwitch.Slots[1];
        PartyMenuMember first = app.PartyMembers[firstSlot];
        PartyMenuMember second = app.PartyMembers[secondSlot];
        app.PartyMembers[firstSlot] = second;
        app.PartyMembers[secondSlot] = first;
        (first.SpriteXDelta, second.SpriteXDelta) = (second.SpriteXDelta, first.SpriteXDelta);
        (first.SpriteYDelta, second.SpriteYDelta) = (second.SpriteYDelta, first.SpriteYDelta);
        (first.StatusXPos, second.StatusXPos) = (second.StatusXPos, first.StatusXPos);
        (first.StatusYPos, second.StatusYPos) = (second.StatusYPos, first.StatusYPos);
        (first.ItemXPos, second.ItemXPos) = (second.ItemXPos, first.ItemXPos);
        (first.ItemYPos, second.ItemYPos) = (second.ItemYPos, first.ItemYPos);
        (first.PanelXPos, second.PanelXPos) = (second.PanelXPos, first.PanelXPos);
        (first.PanelYPos, second.PanelYPos) = (second.PanelYPos, first.PanelYPos);
        PartyMenu_DrawMemberPanelData(app, firstSlot);
        PartyMenu_DrawMemberPanelData(app, secondSlot);
        PartyMenu_ClearMemberWindows(app, firstSlot);
        PartyMenu_ClearMemberWindows(app, secondSlot);
        UpdateSlotHealthbar(app, 0);
        UpdateSlotHealthbar(app, 1);
        PartyMenu_DrawMemberStatusCondition(app, firstSlot, (byte)(app.PartyMembers[firstSlot].StatusIcon & 0xFFF));
        PartyMenu_DrawMemberStatusCondition(app, secondSlot, (byte)(app.PartyMembers[secondSlot].StatusIcon & 0xFFF));
        PartyMenu_DrawMemberHeldItem(app, firstSlot, app.PartyMembers[firstSlot].HeldItem);
        PartyMenu_DrawMemberHeldItem(app, secondSlot, app.PartyMembers[secondSlot].HeldItem);
        PartyMenu_DrawMemberBallSeal(app, firstSlot);
        PartyMenu_DrawMemberBallSeal(app, secondSlot);
    }

    // OFFSET 0x02084420
    private static void UpdateSlotHealthbar(PartyMenuApplication app, int index)
    {
        PartyOrderSwitchData orderSwitch = app.OrderSwitch;
        byte[] panel = orderSwitch.BufPanels[index];
        byte[] healthbar = PartyMenu_GetHealthbarTilemap(app, out int healthbarOffset);
        bool isEgg = app.PartyMembers[orderSwitch.Slots[index]].IsEgg == 1;
        for (int i = 0; i < 9; i++)
        {
            int at = 0x6C + i * 2;
            ushort current = (ushort)(panel[at] | panel[at + 1] << 8);
            ushort tile;
            if (isEgg)
            {
                tile = (ushort)((current & 0xF000) | 0x17);
            }
            else
            {
                ushort source = (ushort)(healthbar[healthbarOffset + i * 2] | healthbar[healthbarOffset + i * 2 + 1] << 8);
                tile = (ushort)((source & 0xFFF) | (current & 0xF000));
            }
            panel[at] = (byte)tile;
            panel[at + 1] = (byte)(tile >> 8);
        }
    }

    // OFFSET 0x020844B0
    private static void PartyMenu_SelectChooseWithLimit(PartyMenuApplication app, out int state)
    {
        RenderWindow.Window_EraseMessageBox(app.Windows[WinMediumMessage], 1);
        PartyMenu_ClearContextWindow(app);
        PartyMenu partyMenu = app.PartyMenu!;
        int maxSlots = partyMenu.SelectionSlots >> 4;
        for (int i = 0; i < maxSlots; i++)
        {
            if (partyMenu.SelectionOrder[i] != 0)
            {
                continue;
            }
            partyMenu.SelectionOrder[i] = (byte)(app.CurrPartySlot + 1);
            PrintMemberSelectionComment(app, app.CurrPartySlot);
            PartyMenu_PrintShortMessage(app, TextChooseMonAndConfirm, 1);
            SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 0);
            if (i == (partyMenu.SelectionSlots >> 4) - 1)
            {
                PartyMenu_MoveCursorToSlot(app, SlotConfirm);
            }
            state = StateDefault;
            return;
        }
        switch (maxSlots)
        {
            case 1:
                PartyMenu_PrintLongMessage(app, 0x7B, 1);
                break;
            case 2:
                PartyMenu_PrintLongMessage(app, 0x64, 1);
                break;
            case 3:
                PartyMenu_PrintLongMessage(app, 0x7C, 1);
                break;
            case 4:
                PartyMenu_PrintLongMessage(app, 0x7D, 1);
                break;
            case 5:
                PartyMenu_PrintLongMessage(app, 0x7E, 1);
                break;
        }
        app.StateAfterMessage = StateWaitABPressChooseMons;
        state = StateShowMessageThenNextState;
    }

    // OFFSET 0x020845A8
    private static int PartyMenu_WaitABPressChooseMons(PartyMenuApplication app)
    {
        if ((gSystem.PressedKeys & (KeyA | KeyB)) != 0)
        {
            RenderWindow.Window_EraseMessageBox(app.Windows[WinLongMessage], 1);
            PartyMenu_PrintShortMessage(app, TextChooseMonAndConfirm, 1);
            SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 0);
            return StateDefault;
        }
        return StateWaitABPressChooseMons;
    }

    // OFFSET 0x020845E8
    private static void PartyMenu_SelectRemoveFromSelection(PartyMenuApplication app, out int state)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        int maxSlots = partyMenu.SelectionSlots >> 4;
        for (int i = 0; i < maxSlots; i++)
        {
            if (app.CurrPartySlot + 1 != partyMenu.SelectionOrder[i])
            {
                continue;
            }
            partyMenu.SelectionOrder[i] = 0;
            for (int j = i; j < (partyMenu.SelectionSlots >> 4) - 1; j++)
            {
                partyMenu.SelectionOrder[j] = partyMenu.SelectionOrder[j + 1];
                partyMenu.SelectionOrder[j + 1] = 0;
            }
            break;
        }
        for (int i = 0; i < MaxPartySize; i++)
        {
            if (app.PartyMembers[i].IsPresent != 0)
            {
                PrintMemberSelectionComment(app, i);
            }
        }
        RenderWindow.Window_EraseMessageBox(app.Windows[WinMediumMessage], 1);
        PartyMenu_ClearContextWindow(app);
        PartyMenu_PrintShortMessage(app, TextChooseAPokemon, 1);
        SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 0);
        state = StateDefault;
    }

    // OFFSET 0x020846CC
    private static void PartyMenu_SelectEnter(PartyMenuApplication app, out int state)
    {
        app.PartyMenu!.MenuSelectionResult = 0;
        FreeContextMenu(app);
        state = StateFadeOut;
    }

    // OFFSET 0x020846FC
    private static void PartyMenu_SelectConfirm(PartyMenuApplication app, out int state)
    {
        app.PartyMenu!.MenuSelectionResult = 0;
        FreeContextMenu(app);
        state = StateFadeOut;
    }

    // OFFSET 0x0208472C
    private static void PartyMenu_SelectSummary(PartyMenuApplication app, out int state)
    {
        app.PartyMenu!.MenuSelectionResult = 1;
        FreeContextMenu(app);
        state = StateFadeOut;
    }

    // OFFSET 0x02084760
    private static void PartyMenu_SelectSetBallCapsule(PartyMenuApplication app, out int state)
    {
        PartyMenu_ClearContextWindow(app);
        RenderWindow.Window_EraseMessageBox(app.Windows[WinMediumMessage], 1);
        state = PartyMenu_SetBallCapsule(app);
    }

    // OFFSET 0x02084780
    private static int PartyMenu_SetBallCapsule(PartyMenuApplication app)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, app.CurrPartySlot);
        if (Pokemon.Pokemon_GetValue(mon, MonDataBallCapsuleID, null) == 0)
        {
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x81, app.TmpString!);
            Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteBallSealBase + app.CurrPartySlot]!, 1);
        }
        else
        {
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x82, app.TmpString!);
            app.CurrPartySlot = SlotCancel;
        }
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
        app.PartyMenu.MenuSelectionResult = 0;
        app.StateAfterMessage = StateWaitABPressBeforeFade;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x02084808
    private static void PartyMenu_SelectFieldMove(PartyMenuApplication app, out int state)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        FieldMoveCheckFunc? check = FieldMoveTasks.FieldMove_GetCheckFunc((ushort)(partyMenu.MenuSelectionResult - ExitCodeCut));
        int result = check != null ? check(partyMenu.FieldMoveContext!) : 1;
        uint message = 0;
        switch (result)
        {
            case 0:
                FreeContextMenu(app);
                state = StateFadeOut;
                return;
            case 1:
                message = 0x68;
                break;
            case 2:
                message = 0x4C;
                break;
            case 3:
                message = 0xC4;
                break;
            case 4:
                message = 0x66;
                break;
        }
        RenderWindow.Window_EraseMessageBox(app.Windows[WinMediumMessage], 1);
        PartyMenu_ClearContextWindow(app);
        PartyMenu_PrintLongMessage(app, message, 1);
        app.StateAfterMessage = StateWaitABPress;
        state = StateShowMessageThenNextState;
    }

    private static void SelectFieldMoveWithExitCode(PartyMenuApplication app, int exitCode, out int state)
    {
        app.PartyMenu!.MenuSelectionResult = (byte)exitCode;
        PartyMenu_SelectFieldMove(app, out state);
    }

    // OFFSET 0x020848A8
    private static void PartyMenu_SelectCut(PartyMenuApplication app, out int state)
    {
        SelectFieldMoveWithExitCode(app, ExitCodeCut, out state);
    }

    // OFFSET 0x020848C0
    private static void PartyMenu_SelectRockSmash(PartyMenuApplication app, out int state)
    {
        SelectFieldMoveWithExitCode(app, ExitCodeRockSmash, out state);
    }

    // OFFSET 0x020848D8
    private static void PartyMenu_SelectStrength(PartyMenuApplication app, out int state)
    {
        SelectFieldMoveWithExitCode(app, ExitCodeStrength, out state);
    }

    // OFFSET 0x020848F0
    private static void PartyMenu_SelectSurf(PartyMenuApplication app, out int state)
    {
        SelectFieldMoveWithExitCode(app, ExitCodeSurf, out state);
    }

    // OFFSET 0x02084908
    private static void PartyMenu_SelectDefog(PartyMenuApplication app, out int state)
    {
        SelectFieldMoveWithExitCode(app, ExitCodeDefog, out state);
    }

    // OFFSET 0x02084920
    private static void PartyMenu_SelectRockClimb(PartyMenuApplication app, out int state)
    {
        SelectFieldMoveWithExitCode(app, ExitCodeRockClimb, out state);
    }

    // OFFSET 0x02084938
    private static void PartyMenu_SelectFly(PartyMenuApplication app, out int state)
    {
        SelectFieldMoveWithExitCode(app, ExitCodeFly, out state);
    }

    // OFFSET 0x02084950
    private static void PartyMenu_SelectWaterfall(PartyMenuApplication app, out int state)
    {
        SelectFieldMoveWithExitCode(app, ExitCodeWaterfall, out state);
    }

    // OFFSET 0x02084968
    private static void PartyMenu_SelectFlash(PartyMenuApplication app, out int state)
    {
        SelectFieldMoveWithExitCode(app, ExitCodeFlash, out state);
    }

    // OFFSET 0x02084980
    private static void PartyMenu_SelectTeleport(PartyMenuApplication app, out int state)
    {
        SelectFieldMoveWithExitCode(app, ExitCodeTeleport, out state);
    }

    // OFFSET 0x02084998
    private static void PartyMenu_SelectDig(PartyMenuApplication app, out int state)
    {
        SelectFieldMoveWithExitCode(app, ExitCodeDig, out state);
    }

    // OFFSET 0x020849B0
    private static void PartyMenu_SelectSweetScent(PartyMenuApplication app, out int state)
    {
        SelectFieldMoveWithExitCode(app, ExitCodeSweetScent, out state);
    }

    // OFFSET 0x020849C8
    private static void PartyMenu_SelectChatter(PartyMenuApplication app, out int state)
    {
        SelectFieldMoveWithExitCode(app, ExitCodeChatter, out state);
    }

    // OFFSET 0x020849E0
    private static void PartyMenu_SelectMilkDrink(PartyMenuApplication app, out int state)
    {
        state = PartyMenu_StartFieldMoveHPTransfer(app);
        if (state == StateHPTransferFieldMove)
        {
            app.MonStats[HPTransferJournalMove] = JournalMoveMilkDrink;
        }
    }

    // OFFSET 0x020849FC
    private static void PartyMenu_SelectSoftboiled(PartyMenuApplication app, out int state)
    {
        state = PartyMenu_StartFieldMoveHPTransfer(app);
        if (state == StateHPTransferFieldMove)
        {
            app.MonStats[HPTransferJournalMove] = JournalMoveSoftboiled;
        }
    }

    // OFFSET 0x02084A18
    private static int PartyMenu_StartFieldMoveHPTransfer(PartyMenuApplication app)
    {
        RenderWindow.Window_EraseMessageBox(app.Windows[WinMediumMessage], 1);
        PartyMenu_ClearContextWindow(app);
        PartyMenuMember member = app.PartyMembers[app.CurrPartySlot];
        app.MonStats[HPTransferBuffer] = (ushort)(member.MaxHP / 5);
        if (member.CurHP <= app.MonStats[HPTransferBuffer])
        {
            PartyMenu_PrintLongMessage(app, 0x8A, 1);
            app.StateAfterMessage = StateWaitABPress;
            return StateShowMessageThenNextState;
        }
        ShowSwitchCursor(app);
        PartyMenu_PrintShortMessage(app, 0x24, 1);
        app.MonStats[HPTransferState] = 0;
        return StateHPTransferFieldMove;
    }

    // OFFSET 0x02084B34
    private static int PartyMenu_WaitABPress(PartyMenuApplication app)
    {
        if ((gSystem.PressedKeys & (KeyA | KeyB)) != 0)
        {
            return ReturnToPartyMenu(app);
        }
        return StateWaitABPress;
    }
}
