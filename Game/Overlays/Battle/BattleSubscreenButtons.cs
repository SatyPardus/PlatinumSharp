namespace PlatinumSharp.Game;

public static partial class BattleSubscreens
{
    private const uint TaskPriorityButtonAnim = 0x514;
    private const int KeyframeCount = 3;
    private const int KeyframePressed = 2;
    private const int KeyframeReleased = 1;
    private const int KeyframeDefault = 0;
    private const int ButtonCancel = 0xFF;
    private const int TargetButtonCancel = 4;
    private const int NoSprite = 0xFF;
    private const uint ActionButtonKeyframes = 0x022703A4;
    private const uint ActionButtonRects = 0x022702F4;
    private const uint ActionButtonPositions = 0x02270294;
    private const uint MoveButtonKeyframes = 0x022703F4;
    private const uint MoveButtonRects = 0x02270378;
    private const uint MoveCancelKeyframes = 0x0227040C;
    private const uint MoveCancelRect = 0x02270388;
    private const uint YesButtonKeyframes = 0x02270246;
    private const uint YesButtonRect = 0x02270220;
    private const uint NoButtonKeyframes = 0x02270240;
    private const uint NoButtonRect = 0x0227021C;
    private const uint TargetCancelKeyframes = 0x022703EC;
    private const uint TargetCancelRect = 0x0227034C;
    private const uint TargetButtonKeyframes = 0x022703D4;
    private const uint TargetButtonRects = 0x0227033C;
    private const uint StopPlaybackKeyframes = 0x022701F4;
    private const uint StopPlaybackRect = 0x022701D0;
    private const uint TargetLayoutTable = 0x022705B0;
    private const int ActionTilemapBuffer = 1;
    private const int MoveTilemapBuffer = 3;
    private const int TargetTilemapBuffer = 4;
    private const int YesNoTilemapBuffer = 6;
    private const int SubscreenTilemapWidth = 32;
    private const int BattlerTypeCount = 6;
    private const int BattlerTypeEnemySlot1 = 3;
    private const int BattlerTypeEnemySlot2 = 5;
    private const int TargetLayoutCount = 12;

    private static uint[]? targetLayouts;

    private static void BattleSubscreen_SetButtonAnim(BattleSubscreen subscreen, uint keyframes, uint rect, int srcBufIndex, int selectedButton)
    {
        MenuButtonPressAnimStateFieldButtonPressAnimationState anim = subscreen.MenuButtonPressAnimState.Field.ButtonPressAnimationState;
        anim.Keyframes = RomShorts(keyframes, KeyframeCount);
        anim.SubscreenTilemapRect = RomBytes(rect, 4);
        anim.SrcBufIndex = (byte)srcBufIndex;
        anim.SelectedButton = selectedButton;
    }

    // OFFSET 0x0226A318
    private static int BattleSubscreen_ProcessActionMenu(BattleSubscreen subscreen, int input, int buttonSlot)
    {
        int spriteIndex = NoSprite;
        int fontOAMIndex;
        int unread;
        switch (input)
        {
            case 1:
                fontOAMIndex = 0;
                unread = 0;
                switch (subscreen.BattlerType)
                {
                    case 0:
                        spriteIndex = subscreen.BattlerType;
                        break;
                    case 2:
                    case BattlerTypePlayerSlot2:
                        spriteIndex = subscreen.BattlerType - 2;
                        break;
                }
                break;
            case 2:
                fontOAMIndex = 1;
                unread = 1;
                break;
            case 3:
                fontOAMIndex = 2;
                unread = 1;
                break;
            case 4:
                fontOAMIndex = 3;
                unread = 1;
                break;
            default:
                return input;
        }
        BattleSubscreen_OnButtonSlotPressed(subscreen, buttonSlot);
        BattleSubscreen_StartButtonAnim(BattleSubscreen_ButtonPressTask, subscreen);
        BattleSubscreen_SetButtonAnim(subscreen, ActionButtonKeyframes + (uint)(input - 1) * 6, ActionButtonRects + (uint)(input - 1) * 4, ActionTilemapBuffer, input);
        MenuButtonPressAnimStateFieldButtonPressAnimationState anim = subscreen.MenuButtonPressAnimState.Field.ButtonPressAnimationState;
        anim.FontOAMIdx = (byte)fontOAMIndex;
        anim.SpriteIndex = (byte)spriteIndex;
        anim.Unread = (byte)unread;
        (short x, short y) = RomPosition(ActionButtonPositions, input - 1);
        anim.ScreenPos.X = x;
        anim.ScreenPos.Field.Y = y;
        anim.FreeSprites = 1;
        return input;
    }

    // OFFSET 0x0226A3F4
    private static int BattleSubscreen_ProcessMoveSelectMenu(BattleSubscreen subscreen, int input, int buttonSlot)
    {
        if (input == TouchScreen.ButtonNotPressed)
        {
            return input;
        }
        if (input >= 1 && input <= MoveSlots && subscreen.ActiveMenuData.MoveData.MoveIDs[input - 1] == 0)
        {
            return TouchScreen.ButtonNotPressed;
        }
        uint keyframes;
        uint rect;
        if (input >= 1 && input <= MoveSlots)
        {
            keyframes = MoveButtonKeyframes + (uint)(input - 1) * 6;
            rect = MoveButtonRects + (uint)(input - 1) * 4;
        }
        else if (input == ButtonCancel)
        {
            keyframes = MoveCancelKeyframes;
            rect = MoveCancelRect;
        }
        else
        {
            return TouchScreen.ButtonNotPressed;
        }
        BattleSubscreen_OnButtonSlotPressed(subscreen, buttonSlot);
        BattleSubscreen_StartButtonAnim(BattleSubscreen_MoveButtonPressTask, subscreen);
        BattleSubscreen_SetButtonAnim(subscreen, keyframes, rect, MoveTilemapBuffer, input);
        subscreen.MenuButtonPressAnimState.Field.ButtonPressAnimationState.FreeSprites = 1;
        return input;
    }

    // OFFSET 0x0226A4A4
    private static int BattleSubscreen_ProcessYesNoMenu(BattleSubscreen subscreen, int input, int buttonSlot)
    {
        int fontOAMIndex;
        if (input == TouchScreen.ButtonNotPressed)
        {
            return input;
        }
        if (input == 1)
        {
            fontOAMIndex = 0;
        }
        else if (input == ButtonCancel)
        {
            fontOAMIndex = 1;
        }
        else
        {
            return input;
        }
        BattleSubscreen_OnButtonSlotPressed(subscreen, buttonSlot);
        BattleSubscreen_StartButtonAnim(BattleSubscreen_ButtonPressTask, subscreen);
        if (input == 1)
        {
            BattleSubscreen_SetButtonAnim(subscreen, YesButtonKeyframes, YesButtonRect, YesNoTilemapBuffer, input);
        }
        else
        {
            BattleSubscreen_SetButtonAnim(subscreen, NoButtonKeyframes, NoButtonRect, YesNoTilemapBuffer, input);
        }
        MenuButtonPressAnimStateFieldButtonPressAnimationState anim = subscreen.MenuButtonPressAnimState.Field.ButtonPressAnimationState;
        anim.FontOAMIdx = (byte)fontOAMIndex;
        anim.Unread = 3;
        anim.FreeSprites = 1;
        return input;
    }

    // OFFSET 0x0226A528
    private static int BattleSubscreen_ProcessTargetSelectMenu(BattleSubscreen subscreen, int input, int buttonSlot)
    {
        byte[] selectable = new byte[MoveSlots];
        BattleSubscreen_GetTargetButtons(subscreen, selectable, 1);
        if (input >= 1 && input <= MoveSlots)
        {
            if (selectable[input - 1] == 0)
            {
                return TouchScreen.ButtonNotPressed;
            }
            BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!)!;
            Bg.Bg_FillTilemap(bgConfig, SubscreenFirstLayer + 1, BlankTile);
            Bg.Bg_ScheduleTilemapTransfer(bgConfig, SubscreenFirstLayer + 1);
            BattleSubscreen_OnTargetSelected(subscreen, input);
            BattleSubscreen_StartButtonAnim(BattleSubscreen_TargetButtonPressTask, subscreen);
            subscreen.MenuButtonPressAnimState.Field.TargetSelectAnimationState.SelectedTarget = input;
            return input;
        }
        if (input != ButtonCancel)
        {
            return input;
        }
        BattleSubscreen_OnButtonSlotPressed(subscreen, TargetButtonCancel);
        BattleSubscreen_StartButtonAnim(BattleSubscreen_ButtonPressTask, subscreen);
        BattleSubscreen_SetButtonAnim(subscreen, TargetCancelKeyframes, TargetCancelRect, TargetTilemapBuffer, input);
        MenuButtonPressAnimStateFieldButtonPressAnimationState anim = subscreen.MenuButtonPressAnimState.Field.ButtonPressAnimationState;
        anim.FontOAMIdx = ButtonFontOAMCancel;
        anim.Unread = 3;
        anim.FreeSprites = 1;
        return input;
    }

    // OFFSET 0x0226A634
    private static int BattleSubscreen_ProcessStopPlaybackMenu(BattleSubscreen subscreen, int input, int buttonSlot)
    {
        if (input == TouchScreen.ButtonNotPressed || input != 1)
        {
            return input;
        }
        BattleSubscreen_OnButtonSlotPressed(subscreen, buttonSlot);
        BattleSubscreen_StartButtonAnim(BattleSubscreen_ButtonPressTask, subscreen);
        BattleSubscreen_SetButtonAnim(subscreen, StopPlaybackKeyframes, StopPlaybackRect, YesNoTilemapBuffer, input);
        MenuButtonPressAnimStateFieldButtonPressAnimationState anim = subscreen.MenuButtonPressAnimState.Field.ButtonPressAnimationState;
        anim.FontOAMIdx = 0;
        anim.Unread = 3;
        anim.FreeSprites = 0;
        return input;
    }

    // OFFSET 0x0226B20C
    private static void BattleSubscreen_GetTargetButtons(BattleSubscreen subscreen, byte[] buttons, int checkPresent)
    {
        targetLayouts ??= RomData.OverlayU32(16, TargetLayoutTable, TargetLayoutCount * MoveSlots);
        byte[] battlersByType = new byte[BattlerTypeCount];
        BattleSystems.BattleSystem_GetBattlersByType(subscreen.BattleSys!, battlersByType);
        for (int i = 0; i < MoveSlots; i++)
        {
            int battler = battlersByType[i + 2];
            if (checkPresent == 1 && subscreen.ActiveMenuData.TargetSelectData.TargetMonData[battler].IsPresent == 0)
            {
                buttons[i] = 0;
            }
            else
            {
                buttons[i] = (byte)targetLayouts[subscreen.TargetingLayout * MoveSlots + i];
            }
        }
    }

    // OFFSET 0x0226B264
    private static void BattleSubscreen_GetHighlightedTargets(BattleSubscreen subscreen, int selected, byte[] buttons, int checkPresent)
    {
        switch (subscreen.TargetingLayout)
        {
            case 0:
            case 8:
            case 9:
            case 10:
            case 11:
                for (int i = 0; i < MoveSlots; i++)
                {
                    buttons[i] = (byte)(i == selected - 1 ? 1 : 0);
                }
                break;
            default:
                BattleSubscreen_GetTargetButtons(subscreen, buttons, checkPresent);
                break;
        }
    }

    // OFFSET 0x0226B2BC
    private static void BattleSubscreen_StartButtonAnim(SysTaskFunc func, BattleSubscreen subscreen)
    {
        if (subscreen.MenuTransitionTask != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        BattleSubscreen_ClearButtonAnim(subscreen);
        subscreen.MenuTransitionTask = SysTask.SysTask_Start(func, subscreen, TaskPriorityButtonAnim);
    }

    private static void BattleSubscreen_ClearButtonAnim(BattleSubscreen subscreen)
    {
        MenuButtonPressAnimState state = subscreen.MenuButtonPressAnimState;
        state.Phase = 0;
        state.WaitTimer = 0;
        MenuButtonPressAnimStateFieldButtonPressAnimationState anim = state.Field.ButtonPressAnimationState;
        anim.Keyframes = null;
        anim.SubscreenTilemapRect = null;
        anim.SelectedButton = 0;
        anim.SrcBufIndex = 0;
        anim.FontOAMIdx = 0;
        anim.SpriteIndex = 0;
        anim.Unread = 0;
        anim.FreeSprites = 0;
        anim.ScreenPos.X = 0;
        anim.ScreenPos.Field.Y = 0;
        anim.ScreenPos.Field.Z = 0;
        state.Field.TargetSelectAnimationState.SelectedTarget = 0;
    }

    // OFFSET 0x0226B2F0
    private static void BattleSubscreen_EndButtonAnim(BattleSubscreen subscreen)
    {
        if (subscreen.MenuTransitionTask != null)
        {
            SysTask.SysTask_Done(subscreen.MenuTransitionTask);
            BattleSubscreen_ClearButtonAnim(subscreen);
            subscreen.MenuTransitionTask = null;
        }
    }

    // OFFSET 0x0226B314
    private static void BattleSubscreen_OnTargetSelected(BattleSubscreen subscreen, int input)
    {
    }

    // OFFSET 0x0226B318
    private static void BattleSubscreen_OnButtonSlotPressed(BattleSubscreen subscreen, int buttonSlot)
    {
    }

    // OFFSET 0x0226B31C
    private static void BattleSubscreen_ApplyButtonKeyframe(BattleSubscreen subscreen, short[] keyframes, byte[] rect, int srcBufIndex, int keyframe)
    {
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!)!;
        byte[] dest = Bg.Bg_GetTilemapBuffer(bgConfig, SubscreenFirstLayer)!;
        byte[] src = subscreen.TilemapBuffers[srcBufIndex]!;
        int offset = keyframes[keyframe];
        for (int y = rect[0]; y <= rect[1]; y++)
        {
            for (int x = rect[2]; x <= rect[3]; x++)
            {
                int index = (y * SubscreenTilemapWidth + x) * 2;
                ushort value = (ushort)(Nitro.Bin.U16(src, index) + offset);
                dest[index] = (byte)value;
                dest[index + 1] = (byte)(value >> 8);
            }
        }
        Bg.Bg_ScheduleTilemapTransfer(bgConfig, SubscreenFirstLayer);
    }

    private static void BattleSubscreen_OffsetFontOAMY(BattleSubscreen subscreen, int index, int dy)
    {
        FontOAMSystem.FontOAM_GetXY(subscreen.FontOAMEntry[index].FontOAM, out int x, out int y);
        FontOAMSystem.FontOAM_SetXY(subscreen.FontOAMEntry[index].FontOAM, x, y + dy);
    }

    // OFFSET 0x0226B390
    private static void BattleSubscreen_ButtonPressTask(SysTask task, object? param)
    {
        BattleSubscreen subscreen = (BattleSubscreen)param!;
        BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!);
        MenuButtonPressAnimState state = subscreen.MenuButtonPressAnimState;
        MenuButtonPressAnimStateFieldButtonPressAnimationState anim = state.Field.ButtonPressAnimationState;
        switch (state.Phase)
        {
            case 0:
                BattleSubscreen_ApplyButtonKeyframe(subscreen, anim.Keyframes!, anim.SubscreenTilemapRect!, anim.SrcBufIndex, KeyframePressed);
                BattleSubscreen_OffsetFontOAMY(subscreen, anim.FontOAMIdx, -2);
                if (anim.SpriteIndex != NoSprite && subscreen.TargetSelectPokeIconSprites[anim.SpriteIndex] != null)
                {
                    SpriteSystem.ManagedSprite_OffsetPositionXY(subscreen.TargetSelectPokeIconSprites[anim.SpriteIndex]!, 0, -2);
                }
                state.Phase++;
                return;
            case 1:
                state.WaitTimer++;
                if (state.WaitTimer <= 0)
                {
                    return;
                }
                state.WaitTimer = 0;
                state.Phase++;
                goto case 2;
            case 2:
                BattleSubscreen_ApplyButtonKeyframe(subscreen, anim.Keyframes!, anim.SubscreenTilemapRect!, anim.SrcBufIndex, KeyframeReleased);
                BattleSubscreen_OffsetFontOAMY(subscreen, anim.FontOAMIdx, 1);
                if (anim.SpriteIndex != NoSprite && subscreen.TargetSelectPokeIconSprites[anim.SpriteIndex] != null)
                {
                    SpriteSystem.ManagedSprite_OffsetPositionXY(subscreen.TargetSelectPokeIconSprites[anim.SpriteIndex]!, 0, 1);
                }
                state.Phase++;
                return;
            default:
                state.WaitTimer++;
                if (state.WaitTimer > 0)
                {
                    BattleSubscreen_FinishMenu(subscreen, anim.FreeSprites, 0);
                    BattleSubscreen_EndButtonAnim(subscreen);
                }
                return;
        }
    }

    // OFFSET 0x0226B4E0
    private static void BattleSubscreen_MoveButtonPressTask(SysTask task, object? param)
    {
        BattleSubscreen subscreen = (BattleSubscreen)param!;
        BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!);
        MenuButtonPressAnimState state = subscreen.MenuButtonPressAnimState;
        MenuButtonPressAnimStateFieldButtonPressAnimationState anim = state.Field.ButtonPressAnimationState;
        MoveSelectData moves = subscreen.ActiveMenuData.MoveData;
        int nameIndex;
        int ppCountIndex;
        int ppLabelIndex;
        int move;
        switch (anim.SelectedButton)
        {
            case 1:
            case 2:
            case 3:
            case 4:
                nameIndex = anim.SelectedButton - 1;
                ppLabelIndex = ButtonFontOAMPPLabel + nameIndex;
                move = moves.MoveIDs[nameIndex];
                ppCountIndex = ButtonFontOAMPPCount + nameIndex;
                break;
            default:
                nameIndex = 0;
                ppCountIndex = 0;
                ppLabelIndex = 0;
                move = 0;
                break;
        }
        switch (state.Phase)
        {
            case 0:
                BattleSubscreen_ApplyButtonKeyframe(subscreen, anim.Keyframes!, anim.SubscreenTilemapRect!, anim.SrcBufIndex, KeyframePressed);
                BattleSubscreen_ShiftMoveButton(subscreen, anim.SelectedButton, nameIndex, ppCountIndex, ppLabelIndex, move, -2);
                state.Phase++;
                return;
            case 1:
                state.WaitTimer++;
                if (state.WaitTimer <= 0)
                {
                    return;
                }
                state.WaitTimer = 0;
                state.Phase++;
                goto case 2;
            case 2:
                BattleSubscreen_ApplyButtonKeyframe(subscreen, anim.Keyframes!, anim.SubscreenTilemapRect!, anim.SrcBufIndex, KeyframeReleased);
                BattleSubscreen_ShiftMoveButton(subscreen, anim.SelectedButton, nameIndex, ppCountIndex, ppLabelIndex, move, 1);
                state.Phase++;
                return;
            default:
                state.WaitTimer++;
                if (state.WaitTimer > 0)
                {
                    BattleSubscreen_FinishMenu(subscreen, 1, 1);
                    BattleSubscreen_EndButtonAnim(subscreen);
                }
                return;
        }
    }

    private static void BattleSubscreen_ShiftMoveButton(BattleSubscreen subscreen, int selected, int nameIndex, int ppCountIndex, int ppLabelIndex, int move, int dy)
    {
        if (selected != ButtonCancel)
        {
            BattleSubscreen_OffsetFontOAMY(subscreen, nameIndex, dy);
            BattleSubscreen_OffsetFontOAMY(subscreen, ppCountIndex, dy);
            BattleSubscreen_OffsetFontOAMY(subscreen, ppLabelIndex, dy);
            if (move != 0)
            {
                SpriteSystem.Sprite_OffsetPositionXY(subscreen.MoveSelectSprites[nameIndex]!.Sprite!, 0, (short)dy);
                if (subscreen.CategoryIconSprites[nameIndex] != null)
                {
                    SpriteSystem.Sprite_OffsetPositionXY(subscreen.CategoryIconSprites[nameIndex]!.Sprite!, 0, (short)dy);
                }
            }
        }
        else
        {
            BattleSubscreen_OffsetFontOAMY(subscreen, ButtonFontOAMCancel, dy);
        }
    }

    // OFFSET 0x0226B780
    private static void BattleSubscreen_TargetButtonPressTask(SysTask task, object? param)
    {
        BattleSubscreen subscreen = (BattleSubscreen)param!;
        BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!);
        MenuButtonPressAnimState state = subscreen.MenuButtonPressAnimState;
        byte[] highlighted = new byte[MoveSlots];
        switch (state.Phase)
        {
            case 0:
                BattleSubscreen_GetHighlightedTargets(subscreen, state.Field.TargetSelectAnimationState.SelectedTarget, highlighted, 1);
                BattleSubscreen_ShiftTargetButtons(subscreen, highlighted, KeyframePressed, -2);
                state.Phase++;
                return;
            case 1:
                state.WaitTimer++;
                if (state.WaitTimer <= 0)
                {
                    return;
                }
                state.WaitTimer = 0;
                state.Phase++;
                goto case 2;
            case 2:
                BattleSubscreen_GetHighlightedTargets(subscreen, state.Field.TargetSelectAnimationState.SelectedTarget, highlighted, 1);
                BattleSubscreen_ShiftTargetButtons(subscreen, highlighted, KeyframeReleased, 1);
                state.Phase++;
                return;
            case 3:
                state.WaitTimer++;
                if (state.WaitTimer > 0)
                {
                    state.WaitTimer = 0;
                    state.Phase++;
                }
                return;
            default:
                BattleSubscreen_FinishMenu(subscreen, 1, 1);
                BattleSubscreen_EndButtonAnim(subscreen);
                return;
        }
    }

    private static void BattleSubscreen_ShiftTargetButtons(BattleSubscreen subscreen, byte[] highlighted, int keyframe, int dy)
    {
        for (int i = 0; i < MoveSlots; i++)
        {
            if (highlighted[i] != 1)
            {
                continue;
            }
            BattleSubscreen_ApplyButtonKeyframe(subscreen, RomShorts(TargetButtonKeyframes + (uint)i * 6, KeyframeCount), RomBytes(TargetButtonRects + (uint)i * 4, 4), TargetTilemapBuffer, keyframe);
            if (subscreen.FontOAMEntry[i].FontOAM != null)
            {
                BattleSubscreen_OffsetFontOAMY(subscreen, i, dy);
            }
            if (subscreen.TargetSelectPokeIconSprites[i] != null)
            {
                SpriteSystem.ManagedSprite_OffsetPositionXY(subscreen.TargetSelectPokeIconSprites[i]!, 0, (short)dy);
            }
        }
    }
}
