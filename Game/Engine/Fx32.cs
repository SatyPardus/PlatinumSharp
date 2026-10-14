namespace PlatinumSharp.Game;

public static class Fx32
{
    public const int Shift = 12;
    public const int One = 1 << Shift;

    public static int Mul(int a, int b)
    {
        return (int)(((long)a * b + (One >> 1)) >> Shift);
    }

    public static int Div(int numerator, int denominator)
    {
        if (denominator == 0)
        {
            return numerator < 0 ? int.MinValue : int.MaxValue;
        }
        return (int)(((long)numerator << Shift) / denominator);
    }

    public static int Sqrt(int value)
    {
        if (value <= 0)
        {
            return 0;
        }
        return (int)Math.Floor(Math.Sqrt((double)((long)value << Shift)));
    }
}
