namespace PlatinumSharp.Game;

public static class SplRandom
{
    private const uint Multiplier = 0x5EEDF715;
    private const uint Increment = 0x1B0CB173;

    public static uint State;

    public static uint Next()
    {
        State = unchecked(State * Multiplier + Increment);
        return State;
    }

    public static int Range(int value)
    {
        return unchecked(value * (int)(Next() >> 23) - (value << 8)) >> 8;
    }

    public static int UnitRange()
    {
        return ((int)((Next() >> 23) << 12) - 0x100000) >> 8;
    }

    public static int Attenuate(int value, int range)
    {
        return unchecked(value * (range + 0xFF - ((range * (int)(Next() >> 24)) >> 7))) >> 8;
    }

    // OFFSET 0x020A2354
    public static void SPLRandom_VecFx32_XY(out VecFx32 vec)
    {
        VecFx32 value;
        value.X = (int)Next() >> 8;
        value.Y = (int)Next() >> 8;
        value.Z = 0;
        SplMath.Normalize(value, out vec);
    }

    // OFFSET 0x020A23B0
    public static void SPLRandom_VecFx32(out VecFx32 vec)
    {
        VecFx32 value;
        value.X = (int)Next() >> 8;
        value.Y = (int)Next() >> 8;
        value.Z = (int)Next() >> 8;
        SplMath.Normalize(value, out vec);
    }
}
