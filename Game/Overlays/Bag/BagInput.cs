using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public static partial class BagApplication
{
    private const uint PocketButtonLayouts = 0x022410C8;
    private const uint DialButtonRect = 0x02240E98;
    private const int SeqButtonPress = 0x5E4;
    private const int ButtonTouchLayer = 4;
    private const ushort ButtonPixelValue = 0xFFFE;
    private const int ItemHighlightX = 0xB1;
    private const int DialRotationStep = 0x24;
    private const int SpriteLeftArrow = 2;
    private const int SpriteRightArrow = 3;
    private const int SpriteMovingItemPosBar = 5;
    private const int SpriteItem = 7;

    private const int ListInputNone = 0;
    private const int ListInputSelected = 1;
    private const int ListInputStartMoving = 2;
    private const int ListInputExit = 3;

    private static TouchScreenHitTable[] OverlayTouchRects(uint address)
    {
        byte[] raw = RomData.Overlay(Overlay, address, 0x20 * 4);
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

    private static byte[] PocketButtonPositions(int numPockets)
    {
        uint address = RomData.OverlayU32(Overlay, (uint)(PocketButtonLayouts + numPockets * 8), 1)[0];
        return RomData.Overlay(Overlay, address, numPockets * 2);
    }

    private static TouchScreenHitTable[] PocketButtonRects(int numPockets)
    {
        return OverlayTouchRects(RomData.OverlayU32(Overlay, (uint)(PocketButtonLayouts + 4 + numPockets * 8), 1)[0]);
    }

    private static void SetTilemapEntry(byte[] tilemap, int index, int value)
    {
        tilemap[index * 2] = (byte)value;
        tilemap[index * 2 + 1] = (byte)(value >> 8);
    }

    private static void SetItemHighlightRow(BagController controller, int sprite, int cursorPos, int yOffset)
    {
        SpriteSystem.ManagedSprite_SetPositionXY(controller.Sprites[sprite]!, ItemHighlightX, (short)((cursorPos - 1) * 16 + yOffset));
    }

    // OFFSET 0x0223C51C
    private static int ProcessItemListInput_Normal(BagController controller)
    {
        StepPocketSelectorArrowsAnim(controller);
        if (CheckItemListDialScroll_Normal(controller))
        {
            return StateWaitSelectItemGeneral;
        }
        if (CheckPocketChange_DPad(controller))
        {
            return StateSwitchPocket;
        }
        if (CheckPocketChange_Touch(controller))
        {
            return StateSwitchPocket;
        }
        int input = ProcessItemListMenuInput(controller);
        if (input == ListInputSelected)
        {
            BagUI_SetHighlightSpritesPalette(controller, 2);
            Bg.Window_FillTilemap(controller.Windows[WinItemDescription], 0);
            Bg.Window_ClearAndCopyToVRAM(controller.Windows[WinItemDescription]);
            MakeItemActionsMenu(controller);
            if (CurrentPocket(controller).PocketType == PocketTMs)
            {
                ToggleHideItemSprite(controller, true);
            }
            return StateWaitSelectItemAction;
        }
        if (input == ListInputStartMoving)
        {
            StartMovingItem(controller);
            return StateMovingItem;
        }
        if (input == ListInputExit)
        {
            return StateExit;
        }
        return StateWaitSelectItemGeneral;
    }

    // OFFSET 0x0223C5B8
    private static int ProcessItemListMenuInput(BagController controller)
    {
        BagApplicationPocket pocket = CurrentPocket(controller);
        ListMenu.ListMenu_GetListAndCursorPos(controller.ItemList!, out pocket.CursorScroll, out pocket.CursorPos);
        if ((gSystem.PressedKeys & KeySelect) != 0 && CanMoveSelectedEntry(controller))
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            return ListInputStartMoving;
        }
        uint input = ListMenu.ListMenu_ProcessInput(controller.ItemList!);
        ListMenu.ListMenu_GetListAndCursorPos(controller.ItemList!, out ushort listPos, out ushort cursorPos);
        if (pocket.CursorPos != cursorPos)
        {
            SetItemHighlightRow(controller, SpriteItemHighlight, cursorPos, 0x18);
        }
        pocket.CursorScroll = listPos;
        pocket.CursorPos = cursorPos;
        if (input == ListMenu.NoChoice && CheckDialButtonPressed(controller))
        {
            input = ListMenu.ListMenu_GetIndexOfChoice(controller.ItemList!, (ushort)(listPos + cursorPos));
            controller.DialButtonAnimStep = 1;
        }
        if (input == ListMenu.Cancel)
        {
            if (controller.BagCtx!.Mode == ModeUnk5)
            {
                return ListInputNone;
            }
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            controller.BagCtx.SelectedItem = 0;
            controller.BagCtx.ExitCode = ExitCodeDone;
            HealthBar.App_StartScreenFade(true, HeapID);
            return ListInputExit;
        }
        if (input == ListMenu.NoChoice)
        {
            byte action = ListMenu.ListMenu_GetLastAction(controller.ItemList!);
            if (action == 1)
            {
                RotateDial(controller, DialRotationStep);
            }
            else if (action == 2)
            {
                RotateDial(controller, -DialRotationStep);
            }
            return ListInputNone;
        }
        SoundPlayback.Sound_PlayEffect(SeqConfirm);
        controller.BagCtx!.SelectedItem = PocketItem(pocket, (int)input);
        controller.SelectedItemCountLimit = PocketQuantity(pocket, (int)input);
        return ListInputSelected;
    }

    // OFFSET 0x0223C720
    private static void CalcPocketSelectorIconsPos(BagController controller)
    {
        int numPockets = controller.NumPockets;
        controller.PocketSelectorIconsX = (byte)((0x5A - numPockets * 10) / (numPockets + 1) + 6);
        controller.PocketSelectorIconsSpacing = (byte)(controller.PocketSelectorIconsX + 4);
    }

    // OFFSET 0x0223C750
    private static bool CheckPocketChange_DPad(BagController controller)
    {
        BagContext bagCtx = controller.BagCtx!;
        BagPocketSelector selector = controller.PocketSelector;
        if ((gSystem.PressedKeysRepeatable & KeyLeft) != 0)
        {
            if (controller.NumPockets == 1)
            {
                return false;
            }
            SoundPlayback.Sound_PlayEffect(SeqPocketSwitch);
            if (bagCtx.CurrPocketIdx != 0)
            {
                selector.NextPocket = (byte)(bagCtx.CurrPocketIdx - 1);
                controller.NextPocket = (byte)(bagCtx.CurrPocketIdx - 1);
            }
            else
            {
                selector.NextPocket = (byte)(controller.NumPockets - 1);
                controller.NextPocket = (byte)(controller.NumPockets - 1);
            }
            BagUI_StartMovingPocketHighlight(controller);
            selector.NextPocketDirection = 0;
            SetupPocketChangeFromDPad(controller);
            Bg.Bg_ScheduleTilemapTransfer(controller.BgConfig!, ButtonTouchLayer);
            return true;
        }
        if ((gSystem.PressedKeysRepeatable & KeyRight) != 0)
        {
            if (controller.NumPockets == 1)
            {
                return false;
            }
            SoundPlayback.Sound_PlayEffect(SeqPocketSwitch);
            int next = bagCtx.CurrPocketIdx + 1;
            if (next < controller.NumPockets)
            {
                selector.NextPocket = (byte)next;
                controller.NextPocket = (byte)(bagCtx.CurrPocketIdx + 1);
            }
            else
            {
                selector.NextPocket = 0;
                controller.NextPocket = 0;
            }
            BagUI_StartMovingPocketHighlight(controller);
            selector.NextPocketDirection = 1;
            SetupPocketChangeFromDPad(controller);
            Bg.Bg_ScheduleTilemapTransfer(controller.BgConfig!, ButtonTouchLayer);
            return true;
        }
        return false;
    }

    // OFFSET 0x0223C848
    private static void SetupPocketChangeFromDPad(BagController controller)
    {
        SetupPocketSwitch(controller);
        controller.PocketSelector.PocketChangeState = 3;
        controller.PocketSelector.RedrawSelectorIcons = false;
    }

    // OFFSET 0x0223C868
    private static void SetupPocketChangeFromTouch(BagController controller)
    {
        BagPocketSelector selector = controller.PocketSelector;
        if (selector.NextPocketDirection != 2)
        {
            SetupPocketSwitch(controller);
        }
        else
        {
            selector.AnimStage = 2;
        }
        selector.PocketChangeState = 0;
        selector.RedrawSelectorIcons = true;
    }

    // OFFSET 0x0223C89C
    private static void SetupPocketSwitch(BagController controller)
    {
        BagPocketSelector selector = controller.PocketSelector;
        selector.AnimStage = 0;
        selector.AnimFrame = 0;
        selector.PocketChangeFrame = 0;
        selector.ButtonState = 0;
        selector.TouchState = 0;
        FreeItemListMenu(controller);
        Bg.Window_FillTilemap(controller.Windows[WinItemList], 0);
        Bg.Window_FillTilemap(controller.Windows[WinItemDescription], 0);
        Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinItemList]);
        Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinItemDescription]);
        SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteItemHighlight]!, 0);
        SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteItem]!, 0);
        BagUI_DrawPocketSelectorIcon(controller, controller.BagCtx!.CurrPocketIdx, 0);
        DrawPocketButton(controller, controller.BagCtx.CurrPocketIdx, 0);
    }

    // OFFSET 0x0223C920
    private static bool DoPocketSwitch(BagController controller)
    {
        BagPocketSelector selector = controller.PocketSelector;
        if (!TouchScreen.TouchScreen_Touched())
        {
            selector.TouchState = 1;
        }
        int held = CheckPlayerHeldPocketButton(controller);
        if (held == -1 || held != selector.NextPocket)
        {
            selector.ButtonState = 1;
        }
        if (!DoDPadPocketSwitch(controller) && selector.TouchState == 1 && held != -1)
        {
            controller.BagCtx!.CurrPocketIdx = selector.NextPocket;
            byte target = (byte)held;
            if (selector.NextPocket > target)
            {
                selector.NextPocketDirection = 0;
                selector.AnimStage = 0;
                DrawPocketButton(controller, selector.NextPocket, 0);
                BagUI_DrawPocketSelectorIcon(controller, selector.NextPocket, 0);
            }
            else if (selector.NextPocket < target)
            {
                selector.NextPocketDirection = 1;
                selector.AnimStage = 0;
                DrawPocketButton(controller, selector.NextPocket, 0);
                BagUI_DrawPocketSelectorIcon(controller, selector.NextPocket, 0);
            }
            else
            {
                selector.NextPocketDirection = 2;
            }
            selector.NextPocket = target;
            selector.ButtonState = 0;
            selector.TouchState = 0;
            selector.PocketChangeState = 0;
            controller.NextPocket = target;
            BagUI_StartMovingPocketHighlight(controller);
        }
        bool selectorDone = StepPocketSelectorAnim(controller);
        bool buttonDone = StepPocketSwitchPressedButtonAnim(controller);
        BagUI_StepPocketHighlightMovement(controller);
        if (selectorDone && buttonDone && selector.ButtonState == 1 && BagUI_IsPocketHighlightDoneMoving(controller))
        {
            if (selector.RedrawSelectorIcons)
            {
                DrawPocketButton(controller, selector.NextPocket, 1);
                Bg.Bg_ScheduleTilemapTransfer(controller.BgConfig!, ButtonTouchLayer);
            }
            SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteItem]!, 1);
            return true;
        }
        return false;
    }

    // OFFSET 0x0223CA5C
    private static bool DoDPadPocketSwitch(BagController controller)
    {
        BagPocketSelector selector = controller.PocketSelector;
        BagContext bagCtx = controller.BagCtx!;
        if ((gSystem.PressedKeys & KeyLeft) != 0)
        {
            if (controller.NumPockets == 1)
            {
                return true;
            }
            SoundPlayback.Sound_PlayEffect(SeqPocketSwitch);
            SpriteSystem.ManagedSprite_SetAnim(controller.Sprites[SpriteBag]!, bagCtx.AccessiblePockets[selector.NextPocket].PocketType);
            DrawPocketButton(controller, selector.NextPocket, 0);
            Bg.Bg_ScheduleTilemapTransfer(controller.BgConfig!, ButtonTouchLayer);
            bagCtx.CurrPocketIdx = selector.NextPocket;
            if (selector.NextPocket != 0)
            {
                selector.NextPocket--;
                controller.NextPocket--;
            }
            else
            {
                selector.NextPocket = (byte)(controller.NumPockets - 1);
                controller.NextPocket = (byte)(controller.NumPockets - 1);
            }
            selector.AnimStage = 0;
            selector.NextPocketDirection = 0;
            selector.TouchState = 1;
            selector.ButtonState = 1;
            selector.RedrawSelectorIcons = false;
            if (selector.PocketChangeState != 3)
            {
                selector.PocketChangeState = 4;
            }
            BagUI_StartMovingPocketHighlight(controller);
            return true;
        }
        if ((gSystem.PressedKeys & KeyRight) != 0)
        {
            if (controller.NumPockets == 1)
            {
                return true;
            }
            SoundPlayback.Sound_PlayEffect(SeqPocketSwitch);
            SpriteSystem.ManagedSprite_SetAnim(controller.Sprites[SpriteBag]!, bagCtx.AccessiblePockets[selector.NextPocket].PocketType);
            DrawPocketButton(controller, selector.NextPocket, 0);
            Bg.Bg_ScheduleTilemapTransfer(controller.BgConfig!, ButtonTouchLayer);
            bagCtx.CurrPocketIdx = selector.NextPocket;
            if (selector.NextPocket + 1 < controller.NumPockets)
            {
                selector.NextPocket++;
                controller.NextPocket++;
            }
            else
            {
                selector.NextPocket = 0;
                controller.NextPocket = 0;
            }
            selector.AnimStage = 0;
            selector.NextPocketDirection = 1;
            selector.TouchState = 1;
            selector.ButtonState = 1;
            selector.RedrawSelectorIcons = false;
            if (selector.PocketChangeState != 3)
            {
                selector.PocketChangeState = 4;
            }
            BagUI_StartMovingPocketHighlight(controller);
            return true;
        }
        return false;
    }

    // OFFSET 0x0223CBD8
    private static bool StepPocketSelectorAnim(BagController controller)
    {
        BagPocketSelector selector = controller.PocketSelector;
        switch (selector.AnimStage)
        {
            case 0:
                selector.AnimFrame = 0;
                selector.AnimStage++;
                return false;
            case 1:
                if (selector.AnimFrame < 8)
                {
                    selector.AnimFrame++;
                    BagUI_PrintPocketNames(controller);
                    return false;
                }
                BagContext bagCtx = controller.BagCtx!;
                if (controller.ItemList != null)
                {
                    BagApplicationPocket previous = CurrentPocket(controller);
                    ListMenu.ListMenu_Free(controller.ItemList, out previous.CursorScroll, out previous.CursorPos);
                    StringList.StringList_Free(controller.ItemListEntries!);
                }
                bagCtx.CurrPocketIdx = selector.NextPocket;
                SpriteSystem.ManagedSprite_SetAnim(controller.Sprites[SpriteBag]!, CurrentPocket(controller).PocketType);
                BagUI_DrawPocketSelectorIcons(controller);
                BagUI_DrawPocketSelectorIcon(controller, bagCtx.CurrPocketIdx, 1);
                LoadCurrentPocketItemNames(controller);
                BagApplicationPocket pocket = CurrentPocket(controller);
                LimitItemListScroll(ref pocket.CursorScroll, ref pocket.CursorPos, pocket.ListEntryCount);
                RestrictItemListCursor(ref pocket.CursorScroll, ref pocket.CursorPos, pocket.ListEntryCount, 9);
                CreateItemListMenu(controller, pocket.CursorScroll, pocket.CursorPos);
                SetItemHighlightRow(controller, SpriteItemHighlight, pocket.CursorPos, 0x18);
                SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteItemHighlight]!, 1);
                selector.AnimStage++;
                return true;
            case 2:
                return true;
        }
        return false;
    }

    // OFFSET 0x0223CD40
    private static bool CheckPocketChange_Touch(BagController controller)
    {
        int pressed = CheckPlayerPressedPocketButton(controller);
        if (pressed == -1)
        {
            return false;
        }
        if (controller.NumPockets == 1)
        {
            return false;
        }
        BagPocketSelector selector = controller.PocketSelector;
        byte current = controller.BagCtx!.CurrPocketIdx;
        byte target = (byte)pressed;
        if (current == target)
        {
            selector.NextPocketDirection = 2;
        }
        else if (current > target)
        {
            selector.NextPocketDirection = 0;
        }
        else
        {
            selector.NextPocketDirection = 1;
        }
        selector.NextPocket = target;
        controller.NextPocket = target;
        BagUI_StartMovingPocketHighlight(controller);
        SetupPocketChangeFromTouch(controller);
        return true;
    }

    // OFFSET 0x0223CDB0
    private static int CheckPlayerPressedPocketButton(BagController controller)
    {
        int pressed = TouchScreen.TouchScreen_CheckRectanglePressed(PocketButtonRects(controller.NumPockets));
        if (pressed != -1 && Bg.Bg_DoesPixelAtXYMatchVal(controller.BgConfig!, ButtonTouchLayer, gSystem.TouchX, gSystem.TouchY, new[] { ButtonPixelValue }) == 0)
        {
            return -1;
        }
        return pressed;
    }

    // OFFSET 0x0223CE08
    private static int CheckPlayerHeldPocketButton(BagController controller)
    {
        int held = TouchScreen.TouchScreen_CheckRectangleHeld(PocketButtonRects(controller.NumPockets));
        if (held != -1 && Bg.Bg_DoesPixelAtXYMatchVal(controller.BgConfig!, ButtonTouchLayer, gSystem.TouchX, gSystem.TouchY, new[] { ButtonPixelValue }) == 0)
        {
            return -1;
        }
        return held;
    }

    // OFFSET 0x0223CE60
    private static bool StepPocketSwitchPressedButtonAnim(BagController controller)
    {
        BagPocketSelector selector = controller.PocketSelector;
        switch (selector.PocketChangeState)
        {
            case 0:
                selector.PocketChangeFrame = 0;
                selector.RedrawSelectorIcons = true;
                selector.PressedPocketBtnIdx = selector.NextPocket;
                selector.PocketChangeState++;
                break;
            case 1:
                if (selector.PocketChangeFrame == 3)
                {
                    SoundPlayback.Sound_PlayEffect(SeqButtonPress);
                    DrawPocketButton(controller, selector.PressedPocketBtnIdx, 2);
                    Bg.Bg_ScheduleTilemapTransfer(controller.BgConfig!, ButtonTouchLayer);
                    byte[] positions = PocketButtonPositions(controller.NumPockets);
                    int x = positions[selector.PressedPocketBtnIdx * 2] * 8 + 0x14;
                    int y = positions[selector.PressedPocketBtnIdx * 2 + 1] * 8 + 0x14;
                    BagUI_DrawBtnShockwave(controller, (short)x, (short)y);
                    selector.PocketChangeState++;
                }
                break;
            case 2:
                if (selector.PocketChangeFrame == 7)
                {
                    selector.PocketChangeState++;
                    return true;
                }
                break;
            case 3:
                return true;
            case 4:
                selector.PocketChangeState = 3;
                return true;
        }
        selector.PocketChangeFrame++;
        return false;
    }

    // OFFSET 0x0223CF20
    private static void DrawPocketButton(BagController controller, int pocketIdx, int buttonState)
    {
        byte[] tilemap = Bg.Bg_GetTilemapBuffer(controller.BgConfig!, ButtonTouchLayer)!;
        byte[] positions = PocketButtonPositions(controller.NumPockets);
        int pocketType = controller.BagCtx!.AccessiblePockets[pocketIdx].PocketType;
        int tileBase = (ushort)(buttonState * 5 + (pocketType >> 1) * 0x96 + (pocketType & 1) * 0xF + 0x1E);
        int x = positions[pocketIdx * 2];
        int y = positions[pocketIdx * 2 + 1];
        for (int row = 0; row < 5; row++)
        {
            int rowBase = tileBase + row * 0x1E;
            for (int col = 0; col < 5; col++)
            {
                SetTilemapEntry(tilemap, (y + row) * 32 + col + x, rowBase + col + 0x2000);
            }
        }
    }

    // OFFSET 0x0223CFB0
    private static void DrawDialButton(BagController controller, int buttonState)
    {
        byte[] tilemap = Bg.Bg_GetTilemapBuffer(controller.BgConfig!, ButtonTouchLayer)!;
        int tileBase = (ushort)(buttonState * 6 + 0x276);
        for (int row = 0; row < 6; row++)
        {
            int rowBase = tileBase + row * 0x1E;
            for (int col = 0; col < 6; col++)
            {
                SetTilemapEntry(tilemap, (row + 7) * 32 + col + 0xD, rowBase + col + 0x3000);
            }
        }
        Bg.Bg_ScheduleTilemapTransfer(controller.BgConfig!, ButtonTouchLayer);
    }

    // OFFSET 0x0223D014
    private static void StepDialButtonPressedAnim(BagController controller)
    {
        switch (controller.DialButtonAnimStep)
        {
            case 1:
                BagUI_DrawBtnShockwave(controller, 0x80, 0x50);
                SoundPlayback.Sound_PlayEffect(SeqButtonPress);
                DrawDialButton(controller, 2);
                controller.DialButtonAnimFrame = 0;
                controller.DialButtonAnimStep++;
                break;
            case 2:
                if (controller.DialButtonAnimFrame == 3)
                {
                    DrawDialButton(controller, 1);
                    controller.DialButtonAnimStep++;
                    controller.DialButtonAnimFrame = 0;
                }
                else
                {
                    controller.DialButtonAnimFrame++;
                }
                break;
            case 3:
                if (controller.DialButtonAnimFrame == 2)
                {
                    DrawDialButton(controller, 0);
                    controller.DialButtonAnimStep = 0;
                    controller.DialButtonAnimFrame = 0;
                }
                else
                {
                    controller.DialButtonAnimFrame++;
                }
                break;
        }
    }

    // OFFSET 0x0223D0BC
    private static void DrawTouchScreenButtons(BagController controller)
    {
        DrawDialButton(controller, 0);
        if (controller.NumPockets != 1)
        {
            for (int i = 0; i < controller.NumPockets; i++)
            {
                DrawPocketButton(controller, i, 0);
            }
        }
        Bg.Bg_ScheduleTilemapTransfer(controller.BgConfig!, ButtonTouchLayer);
    }

    private static void ShiftSpriteX(ManagedSprite sprite, int delta)
    {
        SpriteSystem.ManagedSprite_GetPositionXY(sprite, out short x, out short y);
        x = (short)(x + delta);
        SpriteSystem.ManagedSprite_SetPositionXY(sprite, x, y);
    }

    // OFFSET 0x0223D0FC
    private static void StepPocketSelectorArrowsAnim(BagController controller)
    {
        if (controller.PocketSelectorArrowsAnimTimer == 0)
        {
            switch (controller.PocketSelectorArrowsAnimStep)
            {
                case 0:
                case 1:
                    ShiftSpriteX(controller.Sprites[SpriteLeftArrow]!, -1);
                    ShiftSpriteX(controller.Sprites[SpriteRightArrow]!, 1);
                    break;
                case 2:
                case 3:
                    ShiftSpriteX(controller.Sprites[SpriteLeftArrow]!, 1);
                    ShiftSpriteX(controller.Sprites[SpriteRightArrow]!, -1);
                    break;
            }
            controller.PocketSelectorArrowsAnimStep = (byte)((controller.PocketSelectorArrowsAnimStep + 1) & 3);
        }
        controller.PocketSelectorArrowsAnimTimer = (byte)((controller.PocketSelectorArrowsAnimTimer + 1) & 3);
    }

    // OFFSET 0x0223D1F4
    private static bool CheckDialButtonPressed(BagController controller)
    {
        if (TouchScreen.TouchScreen_CheckRectanglePressed(OverlayTouchRects(DialButtonRect)) == -1)
        {
            return false;
        }
        return Bg.Bg_DoesPixelAtXYMatchVal(controller.BgConfig!, ButtonTouchLayer, gSystem.TouchX, gSystem.TouchY, new[] { ButtonPixelValue }) != 0;
    }

    // OFFSET 0x0223D244
    private static bool CanMoveSelectedEntry(BagController controller)
    {
        BagApplicationPocket pocket = CurrentPocket(controller);
        if (ListMenu.ListMenu_GetIndexOfChoice(controller.ItemList!, (ushort)(pocket.CursorScroll + pocket.CursorPos)) == ListMenu.Cancel)
        {
            return false;
        }
        if (controller.BagCtx!.Mode != ModeField)
        {
            return false;
        }
        return (byte)(pocket.PocketType - PocketTMs) > 1;
    }

    // OFFSET 0x0223D2A0
    private static void StartMovingItem(BagController controller)
    {
        BagApplicationPocket pocket = CurrentPocket(controller);
        controller.IsMovingItem = 1;
        controller.MovedItemPos = (byte)(pocket.CursorScroll + pocket.CursorPos);
        controller.MovedItemID = ListMenu.ListMenu_GetIndexOfChoice(controller.ItemList!, controller.MovedItemPos);
        BagUI_PrintMovingItemMsg(controller);
        ListMenu.ListMenu_Draw(controller.ItemList!);
        SwitchSpritesForSorting(controller);
    }

    // OFFSET 0x0223D2F8
    private static bool ProcessItemListInput_MovingItem(BagController controller)
    {
        if (CheckItemListDialScroll_Sorting(controller))
        {
            return false;
        }
        BagApplicationPocket pocket = CurrentPocket(controller);
        ListMenu.ListMenu_GetListAndCursorPos(controller.ItemList!, out pocket.CursorScroll, out pocket.CursorPos);
        if (CheckDialButtonPressed(controller))
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            MoveItemToCurrentPosition(controller);
            controller.DialButtonAnimStep = 1;
            return true;
        }
        if ((gSystem.PressedKeys & KeySelect) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            MoveItemToCurrentPosition(controller);
            return true;
        }
        uint input = ListMenu.ListMenu_ProcessInput(controller.ItemList!);
        ListMenu.ListMenu_GetListAndCursorPos(controller.ItemList!, out ushort listPos, out ushort cursorPos);
        if (pocket.CursorPos != cursorPos)
        {
            SetItemHighlightRow(controller, SpriteMovingItemPosBar, cursorPos, 0x10);
        }
        pocket.CursorScroll = listPos;
        pocket.CursorPos = cursorPos;
        if (input == ListMenu.Cancel)
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            if ((gSystem.PressedKeys & KeyA) != 0)
            {
                MoveItemToCurrentPosition(controller);
            }
            else
            {
                StopMovingItem(controller);
            }
            return true;
        }
        if (input == ListMenu.NoChoice)
        {
            byte action = ListMenu.ListMenu_GetLastAction(controller.ItemList!);
            if (action == 1)
            {
                RotateDial(controller, DialRotationStep);
            }
            else if (action == 2)
            {
                RotateDial(controller, -DialRotationStep);
            }
            return false;
        }
        SoundPlayback.Sound_PlayEffect(SeqConfirm);
        MoveItemToCurrentPosition(controller);
        return true;
    }

    // OFFSET 0x0223D42C
    private static void MoveItemToCurrentPosition(BagController controller)
    {
        BagApplicationPocket pocket = CurrentPocket(controller);
        ushort target = (ushort)(pocket.CursorScroll + pocket.CursorPos);
        if (controller.MovedItemPos != target && controller.MovedItemPos != (ushort)(target - 1))
        {
            Item.Item_MoveInPocket(pocket.Items!, (ushort)(controller.MovedItemPos - 1), (ushort)(target - 1));
            StringList.StringList_Free(controller.ItemListEntries!);
            LoadCurrentPocketItemNames(controller);
        }
        StopMovingItem(controller);
    }

    // OFFSET 0x0223D484
    private static void StopMovingItem(BagController controller)
    {
        BagApplicationPocket pocket = CurrentPocket(controller);
        ListMenu.ListMenu_Free(controller.ItemList!, out pocket.CursorScroll, out pocket.CursorPos);
        controller.ItemList = null;
        if (controller.MovedItemPos < pocket.CursorScroll + pocket.CursorPos)
        {
            pocket.CursorPos--;
        }
        controller.IsMovingItem = 0;
        controller.MovedItemPos = 0;
        controller.MovedItemID = 0;
        CreateItemListMenu(controller, pocket.CursorScroll, pocket.CursorPos);
        SwitchSpritesForSorting(controller);
    }

    // OFFSET 0x0223D4E8
    private static void SwitchSpritesForSorting(BagController controller)
    {
        BagApplicationPocket pocket = CurrentPocket(controller);
        if (controller.IsMovingItem == 0)
        {
            SetItemHighlightRow(controller, SpriteItemHighlight, pocket.CursorPos, 0x18);
            SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteItemHighlight]!, 1);
            SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteMovingItemPosBar]!, 0);
            if (controller.NumPockets != 1)
            {
                SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteLeftArrow]!, 1);
                SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteRightArrow]!, 1);
            }
            return;
        }
        SetItemHighlightRow(controller, SpriteMovingItemPosBar, pocket.CursorPos, 0x10);
        SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteItemHighlight]!, 0);
        SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteMovingItemPosBar]!, 1);
        if (controller.NumPockets != 1)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteLeftArrow]!, 0);
            SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteRightArrow]!, 0);
        }
    }
}
