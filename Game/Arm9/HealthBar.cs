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
}
