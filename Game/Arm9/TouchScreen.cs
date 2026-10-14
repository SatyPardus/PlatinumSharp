using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public static class TouchScreen
{
    public const int ButtonNotPressed = -1;
    public const int ButtonPressed = 0;
    public const int ButtonReleased = 1;
    public const int ButtonHeld = 2;
    public const int ButtonHeldOutOfBounds = 3;
    public const byte TableTerminator = 0xFF;
    public const byte CircleCode = 0xFE;

    private const int NoHit = -1;

    // OFFSET 0x02022594
    private static int TouchScreen_FindRectAtPoint(TouchScreenHitTable[] rects, uint x, uint y)
    {
        for (int i = 0; rects[i].Top != TableTerminator; i++)
        {
            bool inColumn = x - rects[i].Left < (uint)(rects[i].Right - rects[i].Left);
            bool inRow = y - rects[i].Top < (uint)(rects[i].Bottom - rects[i].Top);
            if (inRow & inColumn)
            {
                return i;
            }
        }
        return NoHit;
    }

    // OFFSET 0x020225E0
    private static bool TouchScreen_IsPointInCircle(in TouchScreenHitTable circle, uint x, uint y)
    {
        uint deltaX = circle.Bottom - x;
        uint deltaY = circle.Left - y;
        uint radius = circle.Right;
        return deltaX * deltaX + deltaY * deltaY < radius * radius;
    }

    // OFFSET 0x0202260C
    private static bool TouchScreen_IsPointInRect(in TouchScreenHitTable rect, uint x, uint y)
    {
        bool inColumn = x - rect.Left < (uint)(rect.Right - rect.Left);
        bool inRow = y - rect.Top < (uint)(rect.Bottom - rect.Top);
        return inRow & inColumn;
    }

    // OFFSET 0x02022644
    public static int TouchScreen_CheckRectangleHeld(TouchScreenHitTable[] rects)
    {
        if (gSystem.TouchHeld != 0)
        {
            return TouchScreen_FindRectAtPoint(rects, gSystem.TouchX, gSystem.TouchY);
        }
        return NoHit;
    }

    // OFFSET 0x02022664
    public static int TouchScreen_CheckRectanglePressed(TouchScreenHitTable[] rects)
    {
        if (gSystem.TouchPressed != 0)
        {
            return TouchScreen_FindRectAtPoint(rects, gSystem.TouchX, gSystem.TouchY);
        }
        return NoHit;
    }

    // OFFSET 0x02022684
    public static int TouchScreen_CheckHeldHitTableID(TouchScreenHitTable[] hitTable)
    {
        if (gSystem.TouchHeld != 0)
        {
            for (int i = 0; hitTable[i].Top != TableTerminator; i++)
            {
                if (hitTable[i].Top == CircleCode)
                {
                    if (TouchScreen_IsPointInCircle(hitTable[i], gSystem.TouchX, gSystem.TouchY))
                    {
                        return i;
                    }
                }
                else
                {
                    if (TouchScreen_IsPointInRect(hitTable[i], gSystem.TouchX, gSystem.TouchY))
                    {
                        return i;
                    }
                }
            }
        }
        return NoHit;
    }

    // OFFSET 0x020226DC
    public static int TouchScreen_CheckPressedHitTableID(TouchScreenHitTable[] hitTable)
    {
        if (gSystem.TouchPressed != 0)
        {
            for (int i = 0; hitTable[i].Top != TableTerminator; i++)
            {
                if (hitTable[i].Top == CircleCode)
                {
                    if (TouchScreen_IsPointInCircle(hitTable[i], gSystem.TouchX, gSystem.TouchY))
                    {
                        return i;
                    }
                }
                else
                {
                    if (TouchScreen_IsPointInRect(hitTable[i], gSystem.TouchX, gSystem.TouchY))
                    {
                        return i;
                    }
                }
            }
        }
        return NoHit;
    }

    // OFFSET 0x02022734
    public static bool TouchScreen_LocationHeld(in TouchScreenHitTable hitTable)
    {
        if (gSystem.TouchHeld != 0)
        {
            if (hitTable.Top == CircleCode)
            {
                return TouchScreen_IsPointInCircle(hitTable, gSystem.TouchX, gSystem.TouchY);
            }
            return TouchScreen_IsPointInRect(hitTable, gSystem.TouchX, gSystem.TouchY);
        }
        return false;
    }

    // OFFSET 0x02022760
    public static bool TouchScreen_LocationPressed(in TouchScreenHitTable hitTable)
    {
        if (gSystem.TouchPressed != 0)
        {
            if (hitTable.Top == CircleCode)
            {
                return TouchScreen_IsPointInCircle(hitTable, gSystem.TouchX, gSystem.TouchY);
            }
            return TouchScreen_IsPointInRect(hitTable, gSystem.TouchX, gSystem.TouchY);
        }
        return false;
    }

    // OFFSET 0x0202278C
    public static bool TouchScreen_Touched()
    {
        return gSystem.TouchHeld != 0;
    }

    // OFFSET 0x02022798
    public static bool TouchScreen_Tapped()
    {
        return gSystem.TouchPressed != 0;
    }

    // OFFSET 0x020227A4
    public static bool TouchScreen_GetHoldState(ref uint x, ref uint y)
    {
        if (gSystem.TouchHeld != 0)
        {
            x = gSystem.TouchX;
            y = gSystem.TouchY;
            return true;
        }
        return false;
    }

    // OFFSET 0x020227C0
    public static bool TouchScreen_GetTapState(ref uint x, ref uint y)
    {
        if (gSystem.TouchPressed != 0)
        {
            x = gSystem.TouchX;
            y = gSystem.TouchY;
            return true;
        }
        return false;
    }

    // OFFSET 0x020227DC
    public static int TouchScreen_CheckTouchedHitTableID(TouchScreenHitTable[] hitTable, uint touchX, uint touchY)
    {
        for (int i = 0; hitTable[i].Top != TableTerminator; i++)
        {
            if (hitTable[i].Top == CircleCode)
            {
                if (TouchScreen_IsPointInCircle(hitTable[i], touchX, touchY))
                {
                    return i;
                }
            }
            else
            {
                if (TouchScreen_IsPointInRect(hitTable[i], touchX, touchY))
                {
                    return i;
                }
            }
        }
        return NoHit;
    }

    // OFFSET 0x02022830
    public static bool TouchScreen_IsTouchInHitTable(in TouchScreenHitTable hitTable, uint touchX, uint touchY)
    {
        if (hitTable.Top == CircleCode)
        {
            return TouchScreen_IsPointInCircle(hitTable, touchX, touchY);
        }
        return TouchScreen_IsPointInRect(hitTable, touchX, touchY);
    }
}
