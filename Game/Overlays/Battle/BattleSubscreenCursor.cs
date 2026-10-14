namespace PlatinumSharp.Game;

public static partial class BattleSubscreens
{
    private const int CursorSfx = 0x5DC;
    private const int CursorActivateKeys = 0xCF3;
    private const int PadButtonA = 0x0001;
    private const int PadButtonB = 0x0002;
    private const int PadKeyRight = 0x0010;
    private const int PadKeyLeft = 0x0020;
    private const int PadKeyUp = 0x0040;
    private const int PadKeyDown = 0x0080;
    private const int PadDirectionMask = 0xF0;
    private const byte CursorBlocked = 0xFF;
    private const int CursorInset = 8;
    private const int CursorSubscreenOffset = 0x110000;
    private const int ActionMenuColumns = 3;
    private const int ActionMenuRows = 2;
    private const int ActionButtonFight = 0;
    private const int ActionButtonRun = 3;
    private const int MoveMenuColumns = 2;
    private const int MoveMenuRows = 3;
    private const int TargetMenuColumns = 2;
    private const int TargetMenuRows = 3;
    private const int ButtonResultCancel = 4;
    private const int FirstBattleMenuIndex = 5;
    private const int FirstBattleMenuReadyIndex = 6;
    private const uint ActionCursorTable = 0x022701EC;
    private const uint MoveCursorTable = 0x022701E4;
    private const uint TargetCursorTable = 0x022701DC;
    private const uint TwoButtonCursorTable = 0x022701CC;
    private const uint YesNoCursorTable = 0x022701C8;

    private static void BattleSubscreen_SetCursorRect(BattleSubscreen subscreen, BattleMenuConfig config, int button)
    {
        TouchScreenHitTable rect = config.TouchScreenRects![button];
        BattleCursors.BattleCursor_SetRectWithOffset(subscreen.CursorRenderer!, rect.Left + CursorInset, rect.Right - CursorInset, rect.Top + CursorInset, rect.Bottom - CursorInset, CursorSubscreenOffset);
    }

    private static SavedCursorPosition BattleSubscreen_GetSavedCursor(BattleSubscreen subscreen)
    {
        int battler = BattleSystems.BattleSystem_GetBattlerOfType(subscreen.BattleSys!, subscreen.BattlerType);
        return Battlers.Battler_GetCursorMemory(BattleSystems.BattleSystem_GetBattlerData(subscreen.BattleSys!, battler)!);
    }

    private static int BattleSubscreen_FindResult(BattleMenuConfig config, int result)
    {
        int i = 0;
        while (true)
        {
            if (config.ButtonResults![i] == result)
            {
                return i;
            }
            i++;
        }
    }

    // OFFSET 0x0226BE48
    private static int BattleSubscreen_CheckCursorInput(BattleSubscreen subscreen)
    {
        BattleMenuConfig config = MenuConfig(subscreen.ActiveMenuConfigIndex);
        if (config.HandleCursorInput == 0)
        {
            return TouchScreen.ButtonNotPressed;
        }
        if (subscreen.Cursor.IsActive == 0)
        {
            if (subscreen.SuppressActivationSfx == 1 || (Globals.gSystem.PressedKeysRepeatable & CursorActivateKeys) != 0)
            {
                if (subscreen.SuppressActivationSfx == 0)
                {
                    SoundPlayback.Sound_PlayEffect(CursorSfx);
                }
                subscreen.Cursor.IsActive = 1;
                subscreen.SuppressActivationSfx = 0;
                MenuCursorFunc(config.HandleCursorInput)(subscreen, 1);
            }
            return TouchScreen.ButtonNotPressed;
        }
        return MenuCursorFunc(config.HandleCursorInput)(subscreen, 0);
    }

    // OFFSET 0x0226BEC0
    private static int BattleSubscreen_ActionMenuCursor(BattleSubscreen subscreen, int init)
    {
        MenuCursor cursor = subscreen.Cursor;
        BattleMenuConfig config = MenuConfig(subscreen.ActiveMenuConfigIndex);
        byte[] table = RomBytes(ActionCursorTable, ActionMenuColumns * ActionMenuRows);
        SavedCursorPosition saved = BattleSubscreen_GetSavedCursor(subscreen);
        if (init == 1)
        {
            cursor.X = saved.ActionMenuX;
            cursor.Y = saved.ActionMenuY;
            BattleSubscreen_SetCursorRect(subscreen, config, table[cursor.Y * ActionMenuColumns + cursor.X]);
            return TouchScreen.ButtonNotPressed;
        }
        int pressed;
        if (subscreen.ActiveMenuConfigIndex == FirstBattleMenuIndex || subscreen.ActiveMenuConfigIndex == FirstBattleMenuReadyIndex)
        {
            pressed = BattleSubscreen_MoveCursor(cursor, 1, 1, table);
        }
        else
        {
            int button = table[cursor.Y * ActionMenuColumns + cursor.X];
            if (button == ActionButtonRun && (Globals.gSystem.PressedKeysRepeatable & PadKeyUp) != 0)
            {
                return TouchScreen.ButtonNotPressed;
            }
            pressed = BattleSubscreen_MoveCursor(cursor, ActionMenuColumns, ActionMenuRows, table);
            if (pressed == 0 && button == ActionButtonFight)
            {
                if ((Globals.gSystem.PressedKeysRepeatable & PadKeyLeft) != 0)
                {
                    cursor.X = 0;
                    cursor.Y = 1;
                    SoundPlayback.Sound_PlayEffect(CursorSfx);
                    pressed = PadKeyLeft;
                }
                else if ((Globals.gSystem.PressedKeysRepeatable & PadKeyRight) != 0)
                {
                    cursor.X = 2;
                    cursor.Y = 1;
                    SoundPlayback.Sound_PlayEffect(CursorSfx);
                    pressed = PadKeyRight;
                }
            }
        }
        switch (pressed)
        {
            case PadKeyUp:
            case PadKeyDown:
            case PadKeyLeft:
            case PadKeyRight:
                BattleSubscreen_SetCursorRect(subscreen, config, table[cursor.Y * ActionMenuColumns + cursor.X]);
                break;
            case PadButtonA:
                return table[cursor.Y * ActionMenuColumns + cursor.X];
            case PadButtonB:
                if (subscreen.HasCancelButton == 1)
                {
                    return BattleSubscreen_FindResult(config, ButtonResultCancel);
                }
                break;
        }
        return TouchScreen.ButtonNotPressed;
    }

    // OFFSET 0x0226C0A0
    private static void BattleSubscreen_SaveActionMenuCursor(BattleSubscreen subscreen, int input)
    {
        BattleSystems.BattleSystem_GetBattlerOfType(subscreen.BattleSys!, subscreen.BattlerType);
        if (input == ActionButtonRun && subscreen.IsWaitingForPartner != 0)
        {
            return;
        }
        SavedCursorPosition saved = BattleSubscreen_GetSavedCursor(subscreen);
        byte[] table = RomBytes(ActionCursorTable, ActionMenuColumns * ActionMenuRows);
        for (int y = 0; y < ActionMenuRows; y++)
        {
            for (int x = 0; x < ActionMenuColumns; x++)
            {
                if (input == table[y * ActionMenuColumns + x])
                {
                    saved.ActionMenuX = (sbyte)x;
                    saved.ActionMenuY = (sbyte)y;
                    return;
                }
            }
        }
    }

    // OFFSET 0x0226C100
    private static int BattleSubscreen_TwoButtonMenuCursor(BattleSubscreen subscreen, int init)
    {
        MenuCursor cursor = subscreen.Cursor;
        BattleMenuConfig config = MenuConfig(subscreen.ActiveMenuConfigIndex);
        byte[] table = RomBytes(TwoButtonCursorTable, 2);
        if (init == 1)
        {
            cursor.X = 0;
            cursor.Y = 0;
            BattleSubscreen_SetCursorRect(subscreen, config, table[cursor.Y + cursor.X]);
            return TouchScreen.ButtonNotPressed;
        }
        switch (BattleSubscreen_MoveCursor(cursor, 1, 2, table))
        {
            case PadKeyUp:
            case PadKeyDown:
            case PadKeyLeft:
            case PadKeyRight:
                BattleSubscreen_SetCursorRect(subscreen, config, table[cursor.Y + cursor.X]);
                break;
            case PadButtonA:
                return table[cursor.Y + cursor.X];
        }
        return TouchScreen.ButtonNotPressed;
    }

    // OFFSET 0x0226C1F8
    private static int BattleSubscreen_MoveSelectCursor(BattleSubscreen subscreen, int init)
    {
        SavedCursorPosition saved = BattleSubscreen_GetSavedCursor(subscreen);
        MenuCursor cursor = subscreen.Cursor;
        BattleMenuConfig config = MenuConfig(subscreen.ActiveMenuConfigIndex);
        MoveSelectData moves = subscreen.ActiveMenuData.MoveData;
        byte[] table = RomBytes(MoveCursorTable, MoveMenuColumns * MoveMenuRows);
        if (init == 1)
        {
            cursor.X = saved.MoveMenuX;
            cursor.Y = saved.MoveMenuY;
            int button = table[cursor.Y * MoveMenuColumns + cursor.X];
            if (button != 0 && moves.MoveIDs[button - 1] == 0)
            {
                saved.MoveMenuX = 0;
                saved.MoveMenuY = 0;
                cursor.X = 0;
                cursor.Y = 0;
                button = table[cursor.Y * MoveMenuColumns + cursor.X];
            }
            BattleSubscreen_SetCursorRect(subscreen, config, button);
            return TouchScreen.ButtonNotPressed;
        }
        byte[] local = (byte[])table.Clone();
        switch (BattleSubscreen_MoveCursor(cursor, MoveMenuColumns, MoveMenuRows, local))
        {
            case PadKeyUp:
            case PadKeyDown:
            case PadKeyLeft:
            case PadKeyRight:
                BattleSubscreen_SetCursorRect(subscreen, config, table[cursor.Y * MoveMenuColumns + cursor.X]);
                break;
            case PadButtonA:
                return table[cursor.Y * MoveMenuColumns + cursor.X];
            case PadButtonB:
                return BattleSubscreen_FindResult(config, ButtonCancel);
        }
        return TouchScreen.ButtonNotPressed;
    }

    // OFFSET 0x0226C378
    private static void BattleSubscreen_SaveMoveSelectCursor(BattleSubscreen subscreen, int input)
    {
        if (input == 0)
        {
            return;
        }
        SavedCursorPosition saved = BattleSubscreen_GetSavedCursor(subscreen);
        byte[] table = RomBytes(MoveCursorTable, MoveMenuColumns * MoveMenuRows);
        for (int y = 0; y < MoveMenuRows; y++)
        {
            for (int x = 0; x < MoveMenuColumns; x++)
            {
                if (input == table[y * MoveMenuColumns + x])
                {
                    saved.MoveMenuX = (sbyte)x;
                    saved.MoveMenuY = (sbyte)y;
                    return;
                }
            }
        }
    }

    // OFFSET 0x0226C3C8
    private static int BattleSubscreen_TargetSelectCursor(BattleSubscreen subscreen, int init)
    {
        int multiTarget = 0;
        SavedCursorPosition saved = BattleSubscreen_GetSavedCursor(subscreen);
        MenuCursor cursor = subscreen.Cursor;
        BattleMenuConfig config = MenuConfig(subscreen.ActiveMenuConfigIndex);
        TouchScreenHitTable[] rects = config.TouchScreenRects!;
        byte[] romTable = RomBytes(TargetCursorTable, TargetMenuColumns * TargetMenuRows);
        byte[] table = (byte[])romTable.Clone();
        byte[] present = new byte[MoveSlots];
        byte[] selectable = new byte[MoveSlots];
        BattleSubscreen_GetTargetButtons(subscreen, present, 1);
        BattleSubscreen_GetTargetButtons(subscreen, selectable, 0);
        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 2; x++)
            {
                if (selectable[table[y * TargetMenuColumns + x]] == 0)
                {
                    table[y * TargetMenuColumns + x] = CursorBlocked;
                }
            }
        }
        int columns;
        int rows;
        int left = 0;
        int right = 0;
        int top = 0;
        int bottom = 0;
        int pointerX = -1;
        int pointerY = -1;
        int pointerAnim = 0;
        int cornerLeftY = -1;
        int cornerRightY = -1;
        switch (subscreen.TargetingLayout)
        {
            case 1:
            case 2:
            case 3:
            case 5:
            case 7:
            {
                multiTarget = 1;
                columns = 1;
                rows = 2;
                int upperBottom;
                if (selectable[3] == 1 && selectable[1] == 1)
                {
                    left = rects[3].Left + CursorInset;
                    upperBottom = rects[3].Bottom - CursorInset;
                    right = rects[1].Right - CursorInset;
                    top = rects[3].Top + CursorInset;
                }
                else if (selectable[3] == 1 && selectable[1] == 0)
                {
                    left = rects[3].Left + CursorInset;
                    upperBottom = rects[3].Bottom - CursorInset;
                    right = rects[3].Right - CursorInset;
                    top = rects[3].Top + CursorInset;
                }
                else if (selectable[3] == 0 && selectable[1] == 1)
                {
                    left = rects[1].Left + CursorInset;
                    upperBottom = rects[1].Bottom - CursorInset;
                    right = rects[1].Right - CursorInset;
                    top = rects[1].Top + CursorInset;
                }
                else
                {
                    left = -1;
                    right = -1;
                    top = -1;
                    upperBottom = -1;
                }
                int lowerLeft;
                int lowerRight;
                int lowerTop;
                if (selectable[0] == 1 && selectable[2] == 1)
                {
                    lowerLeft = rects[0].Left + CursorInset;
                    lowerRight = rects[2].Right - CursorInset;
                    lowerTop = rects[0].Top + CursorInset;
                    bottom = rects[0].Bottom - CursorInset;
                }
                else if (selectable[0] == 1 && selectable[2] == 0)
                {
                    lowerLeft = rects[0].Left + CursorInset;
                    lowerRight = rects[0].Right - CursorInset;
                    lowerTop = rects[0].Top + CursorInset;
                    bottom = rects[0].Bottom - CursorInset;
                }
                else if (selectable[0] == 0 && selectable[2] == 1)
                {
                    lowerLeft = rects[2].Left + CursorInset;
                    lowerRight = rects[2].Right - CursorInset;
                    lowerTop = rects[2].Top + CursorInset;
                    bottom = rects[2].Bottom - CursorInset;
                }
                else
                {
                    lowerLeft = -1;
                    lowerRight = -1;
                    lowerTop = -1;
                    bottom = -1;
                }
                if (left != lowerLeft)
                {
                    if (left == -1 || lowerLeft != -1)
                    {
                        if (left == -1 && lowerLeft != -1)
                        {
                            left = lowerLeft;
                        }
                        else
                        {
                            pointerX = lowerLeft;
                            pointerAnim = 2;
                        }
                    }
                }
                if (right != lowerRight)
                {
                    if (right == -1 || lowerRight != -1)
                    {
                        if (right == -1 && lowerRight != -1)
                        {
                            right = lowerRight;
                        }
                        else
                        {
                            pointerX = lowerRight;
                            pointerAnim = 3;
                        }
                    }
                }
                if (top == -1 || lowerTop != -1)
                {
                    if (top == -1 && lowerTop != -1)
                    {
                        top = lowerTop;
                    }
                    else
                    {
                        pointerY = upperBottom;
                    }
                }
                if (upperBottom != -1 && bottom == -1)
                {
                    bottom = upperBottom;
                }
                else if (upperBottom != -1 || bottom == -1)
                {
                    pointerY = bottom;
                    if (pointerAnim == 2)
                    {
                        cornerLeftY = upperBottom;
                    }
                    else
                    {
                        cornerRightY = upperBottom;
                    }
                }
                break;
            }
            default:
                columns = 2;
                rows = 3;
                break;
        }
        if (init == 1)
        {
            if (multiTarget == 0)
            {
                if (saved.TargetingLayout == subscreen.TargetingLayout)
                {
                    cursor.X = saved.TargetSelectX;
                    cursor.Y = saved.TargetSelectY;
                }
                else if (present[3] == 1)
                {
                    cursor.X = 0;
                    cursor.Y = 0;
                }
                else if (present[1] == 1)
                {
                    cursor.X = 1;
                    cursor.Y = 0;
                }
                else if (present[0] == 1)
                {
                    cursor.X = 0;
                    cursor.Y = 1;
                }
                else
                {
                    cursor.X = 1;
                    cursor.Y = 1;
                }
                BattleSubscreen_SetCursorRect(subscreen, config, romTable[cursor.Y * TargetMenuColumns + cursor.X]);
            }
            else
            {
                cursor.X = 0;
                cursor.Y = 0;
                BattleSubscreen_SetMultiTargetCursor(subscreen, left, right, top, bottom, pointerX, cornerLeftY, cornerRightY);
                if (pointerX != -1)
                {
                    BattleCursors.BattleCursor_SetPointer(subscreen.CursorRenderer!, pointerX, pointerY, pointerAnim, CursorSubscreenOffset);
                }
            }
            return TouchScreen.ButtonNotPressed;
        }
        int pressed = multiTarget == 0
            ? BattleSubscreen_MoveCursor(cursor, columns, rows, table)
            : BattleSubscreen_MoveCursor(cursor, columns, rows, null);
        switch (pressed)
        {
            case PadKeyUp:
            case PadKeyDown:
            case PadKeyLeft:
            case PadKeyRight:
                if (multiTarget == 0)
                {
                    BattleSubscreen_SetCursorRect(subscreen, config, romTable[cursor.Y * TargetMenuColumns + cursor.X]);
                }
                else if (cursor.Y == 0)
                {
                    BattleSubscreen_SetMultiTargetCursor(subscreen, left, right, top, bottom, pointerX, cornerLeftY, cornerRightY);
                    if (pointerX != -1)
                    {
                        BattleCursors.BattleCursor_SetPointer(subscreen.CursorRenderer!, pointerX, pointerY, pointerAnim, CursorSubscreenOffset);
                    }
                    else
                    {
                        BattleCursors.BattleCursor_HidePointer(subscreen.CursorRenderer!);
                    }
                }
                else
                {
                    BattleSubscreen_SetCursorRect(subscreen, config, TargetButtonCancel);
                    BattleCursors.BattleCursor_HidePointer(subscreen.CursorRenderer!);
                }
                break;
            case PadButtonA:
                if (multiTarget == 1)
                {
                    if (cursor.Y > 0)
                    {
                        return TargetButtonCancel;
                    }
                    for (int i = 0; i < MoveSlots; i++)
                    {
                        if (present[i] == 1)
                        {
                            return i;
                        }
                    }
                }
                else
                {
                    int button = romTable[cursor.Y * TargetMenuColumns + cursor.X];
                    if (button == TargetButtonCancel)
                    {
                        return button;
                    }
                    if (present[button] == 1)
                    {
                        return button;
                    }
                }
                break;
            case PadButtonB:
                return TargetButtonCancel;
        }
        return TouchScreen.ButtonNotPressed;
    }

    private static void BattleSubscreen_SetMultiTargetCursor(BattleSubscreen subscreen, int left, int right, int top, int bottom, int pointerX, int cornerLeftY, int cornerRightY)
    {
        BattleCursor cursor = subscreen.CursorRenderer!;
        if ((cornerLeftY == -1 && cornerRightY == -1) || pointerX == -1)
        {
            BattleCursors.BattleCursor_SetRectWithOffset(cursor, left, right, top, bottom, CursorSubscreenOffset);
        }
        else if (cornerLeftY != -1 && cornerRightY == -1)
        {
            BattleCursors.BattleCursor_SetCornersWithOffset(cursor, left, top, right, top, left, cornerLeftY, right, bottom, CursorSubscreenOffset);
        }
        else
        {
            BattleCursors.BattleCursor_SetCornersWithOffset(cursor, left, top, right, top, left, bottom, right, cornerRightY, CursorSubscreenOffset);
        }
    }

    // OFFSET 0x0226C9B8
    private static void BattleSubscreen_SaveTargetSelectCursor(BattleSubscreen subscreen, int input)
    {
        if (input == TargetButtonCancel)
        {
            return;
        }
        SavedCursorPosition saved = BattleSubscreen_GetSavedCursor(subscreen);
        saved.TargetingLayout = (sbyte)subscreen.TargetingLayout;
        byte[] table = RomBytes(TargetCursorTable, TargetMenuColumns * TargetMenuRows);
        for (int y = 0; y < TargetMenuRows; y++)
        {
            for (int x = 0; x < TargetMenuColumns; x++)
            {
                if (input == table[y * TargetMenuColumns + x])
                {
                    saved.TargetSelectX = (sbyte)x;
                    saved.TargetSelectY = (sbyte)y;
                    return;
                }
            }
        }
    }

    // OFFSET 0x0226CA14
    private static int BattleSubscreen_YesNoMenuCursor(BattleSubscreen subscreen, int init)
    {
        MenuCursor cursor = subscreen.Cursor;
        BattleMenuConfig config = MenuConfig(subscreen.ActiveMenuConfigIndex);
        byte[] table = RomBytes(YesNoCursorTable, 2);
        if (init == 1)
        {
            BattleSubscreen_SetCursorRect(subscreen, config, table[cursor.Y + cursor.X]);
            return TouchScreen.ButtonNotPressed;
        }
        switch (BattleSubscreen_MoveCursor(cursor, 1, 2, table))
        {
            case PadKeyUp:
            case PadKeyDown:
            case PadKeyLeft:
            case PadKeyRight:
                BattleSubscreen_SetCursorRect(subscreen, config, table[cursor.Y + cursor.X]);
                break;
            case PadButtonA:
                return table[cursor.Y + cursor.X];
            case PadButtonB:
                return 1;
        }
        return TouchScreen.ButtonNotPressed;
    }

    // OFFSET 0x0226CB10
    private static int BattleSubscreen_MoveCursor(MenuCursor cursor, int columns, int rows, byte[]? table)
    {
        if (cursor.X >= columns)
        {
            cursor.X = (sbyte)(columns - 1);
        }
        if (cursor.Y >= rows)
        {
            cursor.Y = (sbyte)(rows - 1);
        }
        int oldX = cursor.X;
        int oldY = cursor.Y;
        int keys = Globals.gSystem.PressedKeysRepeatable;
        int pressed;
        if ((keys & PadKeyUp) != 0)
        {
            cursor.Y = (sbyte)(oldY - 1);
            if (cursor.Y < 0)
            {
                cursor.Y = 0;
            }
            if (table != null && table[cursor.Y * columns + cursor.X] == CursorBlocked)
            {
                do
                {
                    cursor.Y--;
                    if (cursor.Y < 0)
                    {
                        cursor.Y = (sbyte)oldY;
                        break;
                    }
                } while (table[cursor.Y * columns + cursor.X] == CursorBlocked);
            }
            pressed = PadKeyUp;
        }
        else if ((keys & PadKeyDown) != 0)
        {
            cursor.Y = (sbyte)(oldY + 1);
            if (cursor.Y >= rows)
            {
                cursor.Y = (sbyte)(rows - 1);
            }
            if (table != null && table[cursor.Y * columns + cursor.X] == CursorBlocked)
            {
                do
                {
                    cursor.Y++;
                    if (cursor.Y >= rows)
                    {
                        cursor.Y = (sbyte)oldY;
                        break;
                    }
                } while (table[cursor.Y * columns + cursor.X] == CursorBlocked);
            }
            pressed = PadKeyDown;
        }
        else if ((keys & PadKeyLeft) != 0)
        {
            cursor.X = (sbyte)(oldX - 1);
            if (cursor.X < 0)
            {
                cursor.X = 0;
            }
            if (table != null && table[cursor.Y * columns + cursor.X] == CursorBlocked)
            {
                do
                {
                    cursor.X--;
                    if (cursor.X < 0)
                    {
                        cursor.X = (sbyte)oldX;
                        break;
                    }
                } while (table[cursor.Y * columns + cursor.X] == CursorBlocked);
            }
            pressed = PadKeyLeft;
        }
        else if ((keys & PadKeyRight) != 0)
        {
            cursor.X = (sbyte)(oldX + 1);
            if (cursor.X >= columns)
            {
                cursor.X = (sbyte)(columns - 1);
            }
            if (table != null && table[cursor.Y * columns + cursor.X] == CursorBlocked)
            {
                do
                {
                    cursor.X++;
                    if (cursor.X >= columns)
                    {
                        cursor.X = (sbyte)oldX;
                        break;
                    }
                } while (table[cursor.Y * columns + cursor.X] == CursorBlocked);
            }
            pressed = PadKeyRight;
        }
        else if ((keys & PadButtonA) != 0)
        {
            pressed = PadButtonA;
        }
        else if ((keys & PadButtonB) != 0)
        {
            pressed = PadButtonB;
        }
        else
        {
            return 0;
        }
        if (table != null && table[oldY * columns + oldX] == table[cursor.Y * columns + cursor.X])
        {
            cursor.X = (sbyte)oldX;
            cursor.Y = (sbyte)oldY;
        }
        if (cursor.X != oldX || cursor.Y != oldY)
        {
            SoundPlayback.Sound_PlayEffect(CursorSfx);
        }
        else if ((pressed & PadDirectionMask) != 0)
        {
            return 0;
        }
        return pressed;
    }
}
