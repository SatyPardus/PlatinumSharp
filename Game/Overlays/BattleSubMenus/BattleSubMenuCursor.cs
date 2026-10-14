namespace PlatinumSharp.Game;

public sealed class BattleSubMenuCursor
{
    public const int NoSelection = -1;
    public const int Cancelled = -2;

    private const int PadButtonA = 0x1;
    private const int PadButtonB = 0x2;
    private const int PadKeyRight = 0x10;
    private const int PadKeyLeft = 0x20;
    private const int PadKeyUp = 0x40;
    private const int PadKeyDown = 0x80;
    private const int CursorWakeKeys = 0xF3;
    private const int SeqCursorMove = 0x5DC;
    private const int SeqCancel = 0x5DD;
    private const byte NoPosition = 0xFF;
    private const byte StorePreviousFlag = 0x80;
    private const byte PositionIndexMask = 0x7F;

    public BattleCursor? Sprites;
    public GridMenuCursorPosition[]? Positions;
    public byte IsVisible;
    public byte CurrentPositionIndex;
    public byte PreviousPositionIndex;
    public uint EnabledPositionsMask;

    // OFFSET 0x02228A38
    public static BattleSubMenuCursor MakeBattleSubMenuCursor(int heapID)
    {
        return new BattleSubMenuCursor();
    }

    // OFFSET 0x02228A50
    public static void DeleteBattleSubMenuCursor(BattleSubMenuCursor cursor)
    {
    }

    // OFFSET 0x02228A58
    public static BattleCursor? GetBattleSubMenuCursorSprites(BattleSubMenuCursor cursor)
    {
        return cursor.Sprites;
    }

    // OFFSET 0x02228A5C
    public static int IsBattleSubMenuCursorVisible(BattleSubMenuCursor cursor)
    {
        return cursor.IsVisible;
    }

    // OFFSET 0x02228A60
    public static void SetBattleSubMenuCursorVisibility(BattleSubMenuCursor cursor, int isVisible)
    {
        cursor.IsVisible = (byte)isVisible;
    }

    // OFFSET 0x02228A64
    public static void SetBattleSubMenuCursorSprites(BattleSubMenuCursor cursor, BattleCursor sprites)
    {
        cursor.Sprites = sprites;
    }

    private static void SetCursorRect(BattleSubMenuCursor cursor, int index)
    {
        GridMenuCursorPosition position = cursor.Positions![index];
        BattleCursors.BattleCursor_SetRect(cursor.Sprites!, position.XCoord1, position.XCoord2, position.YCoord1, position.YCoord2);
    }

    // OFFSET 0x02228A68
    public static void SetBattleSubMenuCursorCurrentPosition(BattleSubMenuCursor cursor, byte positionIndex)
    {
        cursor.CurrentPositionIndex = positionIndex;
        if (cursor.IsVisible == 1)
        {
            SetCursorRect(cursor, cursor.CurrentPositionIndex);
        }
    }

    // OFFSET 0x02228A90
    public static void ResetBattleSubMenuCursorCurrentPosition(BattleSubMenuCursor cursor)
    {
        cursor.CurrentPositionIndex = 0;
        cursor.PreviousPositionIndex = NoPosition;
    }

    // OFFSET 0x02228A9C
    public static void SetBattleSubMenuCursorPositions(BattleSubMenuCursor cursor, GridMenuCursorPosition[] positions)
    {
        ResetBattleSubMenuCursorCurrentPosition(cursor);
        cursor.Positions = positions;
        cursor.EnabledPositionsMask = 0xFFFFFFFF;
        if (cursor.IsVisible == 1)
        {
            SetCursorRect(cursor, 0);
        }
    }

    // OFFSET 0x02228AC8
    public static void SetBattleSubMenuCursorEnabledPositionsMask(BattleSubMenuCursor cursor, uint mask)
    {
        cursor.EnabledPositionsMask = mask;
    }

    // OFFSET 0x02228ACC
    private static int IsCursorVisible(BattleSubMenuCursor cursor)
    {
        if (cursor.IsVisible == 1)
        {
            return 1;
        }
        if ((Globals.gSystem.PressedKeys & CursorWakeKeys) != 0)
        {
            cursor.IsVisible = 1;
            SetCursorRect(cursor, cursor.CurrentPositionIndex);
            SoundPlayback.Sound_PlayEffect(SeqCursorMove);
        }
        return 0;
    }

    // OFFSET 0x02228B18
    private static int CheckShouldStorePreviousPosition(in GridMenuCursorPosition position, int direction)
    {
        switch (direction)
        {
            case GridMenuCursor.DirectionUp:
                return (position.DownIndex & StorePreviousFlag) != 0 ? 1 : 0;
            case GridMenuCursor.DirectionDown:
                return (position.UpIndex & StorePreviousFlag) != 0 ? 1 : 0;
            case GridMenuCursor.DirectionLeft:
                return (position.RightIndex & StorePreviousFlag) != 0 ? 1 : 0;
            case GridMenuCursor.DirectionRight:
                return (position.LeftIndex & StorePreviousFlag) != 0 ? 1 : 0;
        }
        return 0;
    }

    // OFFSET 0x02228B64
    public static int BattleSubMenuCursorTick(BattleSubMenuCursor cursor)
    {
        if (IsCursorVisible(cursor) == 0)
        {
            return NoSelection;
        }
        int pressed = Globals.gSystem.PressedKeys;
        byte next;
        byte direction;
        if ((pressed & PadKeyUp) != 0)
        {
            direction = GridMenuCursor.DirectionUp;
            next = GridMenuCursor.GridMenuCursor_CheckNavigation(cursor.Positions!, cursor.CurrentPositionIndex, direction);
        }
        else if ((pressed & PadKeyDown) != 0)
        {
            direction = GridMenuCursor.DirectionDown;
            next = GridMenuCursor.GridMenuCursor_CheckNavigation(cursor.Positions!, cursor.CurrentPositionIndex, direction);
        }
        else if ((pressed & PadKeyLeft) != 0)
        {
            direction = GridMenuCursor.DirectionLeft;
            next = GridMenuCursor.GridMenuCursor_CheckNavigation(cursor.Positions!, cursor.CurrentPositionIndex, direction);
        }
        else if ((pressed & PadKeyRight) != 0)
        {
            direction = GridMenuCursor.DirectionRight;
            next = GridMenuCursor.GridMenuCursor_CheckNavigation(cursor.Positions!, cursor.CurrentPositionIndex, direction);
        }
        else
        {
            direction = 0;
            next = NoPosition;
        }
        if (next != NoPosition)
        {
            int storePrevious = 1;
            if ((next & StorePreviousFlag) != 0)
            {
                if (cursor.PreviousPositionIndex != NoPosition)
                {
                    next = cursor.PreviousPositionIndex;
                }
                else
                {
                    next = (byte)(next ^ StorePreviousFlag);
                }
            }
            while ((cursor.EnabledPositionsMask & (1u << next)) == 0)
            {
                storePrevious = 0;
                byte candidate = (byte)(GridMenuCursor.GridMenuCursor_CheckNavigation(cursor.Positions!, next, direction) & PositionIndexMask);
                if (candidate == next || candidate == cursor.CurrentPositionIndex)
                {
                    next = cursor.CurrentPositionIndex;
                    break;
                }
                next = candidate;
            }
            if (cursor.CurrentPositionIndex != next)
            {
                GridMenuCursor.GridMenuCursor_GetFirstCoords(in cursor.Positions![next], out byte x1, out byte y1);
                GridMenuCursor.GridMenuCursor_GetSecondCoords(in cursor.Positions[next], out byte x2, out byte y2);
                if (CheckShouldStorePreviousPosition(in cursor.Positions[next], direction) == 1 && storePrevious != 0)
                {
                    cursor.PreviousPositionIndex = cursor.CurrentPositionIndex;
                }
                else
                {
                    cursor.PreviousPositionIndex = NoPosition;
                }
                cursor.CurrentPositionIndex = next;
                BattleCursors.BattleCursor_SetRect(cursor.Sprites!, x1, x2, y1, y2);
                SoundPlayback.Sound_PlayEffect(SeqCursorMove);
            }
            return NoSelection;
        }
        if ((pressed & PadButtonA) != 0)
        {
            return cursor.CurrentPositionIndex;
        }
        if ((pressed & PadButtonB) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqCancel);
            return Cancelled;
        }
        return NoSelection;
    }
}
