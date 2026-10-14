namespace PlatinumSharp.Game;

public static class FieldNumberHelpers
{
    // OFFSET 0x0205DFC4
    public static int CountDigits(uint value)
    {
        if (value / 10 == 0)
        {
            return 1;
        }
        if (value / 100 == 0)
        {
            return 2;
        }
        if (value / 1000 == 0)
        {
            return 3;
        }
        if (value / 10000 == 0)
        {
            return 4;
        }
        if (value / 100000 == 0)
        {
            return 5;
        }
        if (value / 1000000 == 0)
        {
            return 6;
        }
        if (value / 10000000 == 0)
        {
            return 7;
        }
        if (value / 100000000 == 0)
        {
            return 8;
        }
        return 1;
    }
}
