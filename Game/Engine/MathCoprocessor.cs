namespace PlatinumSharp.Game;

public static class MathCoprocessor
{
    public static uint Div32(int numerator, int denominator)
    {
        if (denominator == 0)
        {
            return numerator < 0 ? 1u : 0xFFFFFFFFu;
        }
        return (uint)(numerator / denominator);
    }

    public static uint Sqrt32(uint value)
    {
        uint root = (uint)Math.Sqrt(value);
        while ((ulong)root * root > value)
        {
            root--;
        }
        while ((ulong)(root + 1) * (root + 1) <= value)
        {
            root++;
        }
        return root;
    }
}
