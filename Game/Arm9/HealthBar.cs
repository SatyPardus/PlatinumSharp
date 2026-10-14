namespace PlatinumSharp.Game;

public static class HealthBar
{
    public const int ColorEmpty = 0;
    public const int ColorRed = 1;
    public const int ColorYellow = 2;
    public const int ColorGreen = 3;
    public const int ColorFull = 4;

    private const int MaxFlagIndex = 32;

    // OFFSET 0x020787CC
    public static uint FlagIndex(int index)
    {
        uint mask = 1;
        if (index >= MaxFlagIndex)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < index; i++)
        {
            mask <<= 1;
        }
        return mask;
    }

    // OFFSET 0x0208C0BC
    public static byte HealthBar_Fill(uint curHP, uint maxHP, uint barSize)
    {
        byte fill = (byte)(curHP * barSize / maxHP);
        if (fill == 0 && curHP != 0)
        {
            fill = 1;
        }
        return fill;
    }

    // OFFSET 0x0208C0D4
    public static byte HealthBar_FillColor(uint fill, uint barSize)
    {
        fill <<= 8;
        barSize <<= 8;
        if (fill > barSize >> 1)
        {
            return ColorGreen;
        }
        if (fill > barSize / 5)
        {
            return ColorYellow;
        }
        if (fill != 0)
        {
            return ColorRed;
        }
        return ColorEmpty;
    }

    // OFFSET 0x0208C104
    public static byte HealthBar_Color(ushort curHP, ushort maxHP, uint barSize)
    {
        if (curHP == maxHP)
        {
            return ColorFull;
        }
        return HealthBar_FillColor(HealthBar_Fill(curHP, maxHP, (byte)barSize), barSize);
    }

    // OFFSET 0x0208C120
    public static void App_StartScreenFade(bool fadeOut, int heapID)
    {
        if (!fadeOut)
        {
            ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 6, 1, heapID);
        }
        else
        {
            ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 6, 1, heapID);
        }
    }

    // OFFSET 0x0208C15C
    public static int App_AdjustItemCountWithInput(ref short count, ushort max)
    {
        short original = count;
        int keys = Globals.gSystem.PressedKeysRepeatable;
        if ((keys & KeyUp) != 0)
        {
            count++;
            if (count > max)
            {
                count = 1;
            }
            return count == original ? 0 : 1;
        }
        if ((keys & KeyDown) != 0)
        {
            count--;
            if (count <= 0)
            {
                count = (short)max;
            }
            return count == original ? 0 : 2;
        }
        if ((keys & KeyLeft) != 0)
        {
            count -= 10;
            if (count <= 0)
            {
                count = 1;
            }
            return count == original ? 0 : 2;
        }
        if ((keys & KeyRight) != 0)
        {
            count += 10;
            if (count > max)
            {
                count = (short)max;
            }
            return count == original ? 0 : 1;
        }
        return 0;
    }

    private const int KeyRight = 0x10;
    private const int KeyLeft = 0x20;
    private const int KeyUp = 0x40;
    private const int KeyDown = 0x80;
}
