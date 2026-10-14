using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class MathUtil
{
    public const int AffineModeIndex = 0;
    public const int AffineMode256 = 1;
    public const int AffineModeDegrees = 2;

    private const int FullCircleDegrees = 360;
    private const long DegreesToIndexFactor = 0xB60B60B60B;
    private const uint LcrngMultiplier = 0x41C64E6D;
    private const uint LcrngIncrement = 0x6073;
    private const uint ArngMultiplier = 0x6C078965;
    private const int MtSize = 624;
    private const int MtPeriod = 397;
    private const uint MtDefaultSeed = 5489;
    private const uint MtUpperMask = 0x80000000;
    private const uint MtLowerMask = 0x7FFFFFFF;
    private const uint MtTemperMaskB = 0x9D2C5680;
    private const uint MtTemperMaskC = 0xEFC60000;
    private const int PiFx = 0x3243;
    private const ushort CrcPolynomial = 0x1021;
    private const ushort CrcInitialValue = 0xFFFF;

    private static readonly uint[] sMtMatrix = { 0, 0x9908B0DF };
    private static readonly uint[] sMtState = new uint[MtSize];
    private static int sMtIndex = MtSize + 1;
    private static uint sLcrngSeed;
    private static ushort[]? sCrcTable;

    private static int RoundSineToFx32(short value)
    {
        float scaled = value / 4096f;
        if (scaled > 0)
        {
            return (int)(scaled * 4096f + 0.5f);
        }
        return (int)(scaled * 4096f - 0.5f);
    }

    // OFFSET 0x0201D15C
    public static int CalcSineDegrees(ushort degrees)
    {
        if (degrees >= FullCircleDegrees)
        {
            return 0;
        }
        return RoundSineToFx32(Render3D.Sin(CalcAngleRotationIdx(degrees) >> 4));
    }

    // OFFSET 0x0201D1D4
    public static int CalcCosineDegrees(ushort degrees)
    {
        if (degrees >= FullCircleDegrees)
        {
            return 0;
        }
        return RoundSineToFx32(Render3D.Cos(CalcAngleRotationIdx(degrees) >> 4));
    }

    // OFFSET 0x0201D250
    public static int CalcSineDegrees_Wraparound(ushort degrees)
    {
        return CalcSineDegrees((ushort)(degrees % FullCircleDegrees));
    }

    // OFFSET 0x0201D264
    public static int CalcCosineDegrees_Wraparound(ushort degrees)
    {
        return CalcCosineDegrees((ushort)(degrees % FullCircleDegrees));
    }

    // OFFSET 0x0201D278
    public static ushort CalcAngleRotationIdx(ushort degrees)
    {
        if (degrees >= FullCircleDegrees)
        {
            return 0;
        }
        long product = (long)(degrees << Fx32.Shift) * DegreesToIndexFactor;
        int high = (int)(product >> 32);
        return (ushort)((high + 0x800) >> Fx32.Shift);
    }

    // OFFSET 0x0201D2A4
    public static ushort CalcAngleRotationIdx_Wraparound(ushort degrees)
    {
        return CalcAngleRotationIdx((ushort)(degrees % FullCircleDegrees));
    }

    // OFFSET 0x0201D2B8
    public static int CalcSineDegrees_FX32(int degrees)
    {
        return CalcSineDegrees_Wraparound((ushort)(degrees >> Fx32.Shift));
    }

    // OFFSET 0x0201D2C4
    public static int CalcCosineDegrees_FX32(int degrees)
    {
        return CalcCosineDegrees_Wraparound((ushort)(degrees >> Fx32.Shift));
    }

    // OFFSET 0x0201D2D0
    public static uint LCRNG_GetSeed()
    {
        return sLcrngSeed;
    }

    // OFFSET 0x0201D2DC
    public static void LCRNG_SetSeed(uint seed)
    {
        sLcrngSeed = seed;
    }

    // OFFSET 0x0201D2E8
    public static ushort LCRNG_Next()
    {
        sLcrngSeed = sLcrngSeed * LcrngMultiplier + LcrngIncrement;
        return (ushort)(sLcrngSeed >> 16);
    }

    // OFFSET 0x0201D30C
    public static uint ARNG_Next(uint seed)
    {
        return seed * ArngMultiplier + 1;
    }

    // OFFSET 0x0201D318
    public static void MTRNG_SetSeed(uint seed)
    {
        sMtState[0] = seed;
        for (sMtIndex = 1; sMtIndex < MtSize; sMtIndex++)
        {
            sMtState[sMtIndex] = ArngMultiplier * (sMtState[sMtIndex - 1] ^ (sMtState[sMtIndex - 1] >> 30)) + (uint)sMtIndex;
        }
    }

    // OFFSET 0x0201D35C
    public static uint MTRNG_Next()
    {
        uint value;
        if (sMtIndex >= MtSize)
        {
            if (sMtIndex == MtSize + 1)
            {
                MTRNG_SetSeed(MtDefaultSeed);
            }
            int i;
            for (i = 0; i < MtSize - MtPeriod; i++)
            {
                value = (sMtState[i] & MtUpperMask) | (sMtState[i + 1] & MtLowerMask);
                sMtState[i] = sMtState[i + MtPeriod] ^ (value >> 1) ^ sMtMatrix[value & 1];
            }
            for (; i < MtSize - 1; i++)
            {
                value = (sMtState[i] & MtUpperMask) | (sMtState[i + 1] & MtLowerMask);
                sMtState[i] = sMtState[i + (MtPeriod - MtSize)] ^ (value >> 1) ^ sMtMatrix[value & 1];
            }
            value = (sMtState[MtSize - 1] & MtUpperMask) | (sMtState[0] & MtLowerMask);
            sMtState[MtSize - 1] = sMtState[MtPeriod - 1] ^ (value >> 1) ^ sMtMatrix[value & 1];
            sMtIndex = 0;
        }
        value = sMtState[sMtIndex++];
        value ^= value >> 11;
        value ^= (value << 7) & MtTemperMaskB;
        value ^= (value << 15) & MtTemperMaskC;
        value ^= value >> 18;
        return value;
    }

    // OFFSET 0x0201D470
    public static void CreateAffineTransformationMatrix(out MtxFx22 matrix, ushort degrees, int xScale, int yScale, int mode)
    {
        if (mode == AffineMode256)
        {
            degrees = (ushort)((degrees * 0xFFFF) >> 8);
        }
        else if (mode == AffineModeDegrees)
        {
            degrees = (ushort)((uint)(degrees * 0xFFFF) / FullCircleDegrees);
        }
        int sin = Render3D.Sin(degrees >> 4);
        int cos = Render3D.Cos(degrees >> 4);
        matrix.M00 = Fx32.Mul(cos, xScale);
        matrix.M01 = Fx32.Mul(sin, xScale);
        matrix.M10 = Fx32.Mul(-sin, yScale);
        matrix.M11 = Fx32.Mul(cos, yScale);
    }

    // OFFSET 0x0201D4CC
    public static int ApproximateArcLength(int x0, int y0, int x1, int y1, uint unusedRadius)
    {
        int fx0 = x0 << Fx32.Shift;
        int fy0 = y0 << Fx32.Shift;
        int fx1 = x1 << Fx32.Shift;
        int fy1 = y1 << Fx32.Shift;
        int cross = Fx32.Mul(fx0, fy1) - Fx32.Mul(fx1, fy0);
        long lengthSquared = (long)fy0 * fy0 + (long)fx0 * fx0;
        int length = Fx32.Sqrt((int)Math.Min(int.MaxValue, lengthSquared >> Fx32.Shift));
        int normalX = length == 0 ? 0 : Fx32.Div(fy0, length);
        int normalY = length == 0 ? 0 : Fx32.Div(fx0, length);
        int deltaX = fx1 - fx0;
        int deltaY = fy1 - fy0;
        int dot = (int)(((long)normalX * deltaX + (long)normalY * deltaY) >> Fx32.Shift);
        int result = dot >> Fx32.Shift;
        if (result < 0)
        {
            result = -result;
        }
        if (cross <= 0)
        {
            result *= -1;
        }
        return result;
    }

    // OFFSET 0x0201D580
    public static int CalcRadialAngle(ushort radius, int distance)
    {
        int circumference = Fx32.Mul(radius << (Fx32.Shift + 1), PiFx);
        return (distance * 0xFFFF) / (circumference >> Fx32.Shift);
    }

    // OFFSET 0x0201D5B8
    public static uint SumBytes(byte[] data, uint size)
    {
        uint sum = 0;
        for (uint i = 0; i < size; i++)
        {
            sum += data[i];
        }
        return sum;
    }

    // OFFSET 0x0201D5D4
    public static void EncodeData(byte[] data, uint size, uint seed)
    {
        for (int i = 0; i < size / 2; i++)
        {
            Bin.W16(data, i * 2, (ushort)(Bin.U16(data, i * 2) ^ LCRNG_NextFrom(ref seed)));
        }
    }

    public static void EncodeData(byte[] data, int offset, uint size, uint seed)
    {
        for (int i = 0; i < size / 2; i++)
        {
            Bin.W16(data, offset + i * 2, (ushort)(Bin.U16(data, offset + i * 2) ^ LCRNG_NextFrom(ref seed)));
        }
    }

    // OFFSET 0x0201D600
    public static void DecodeData(byte[] data, uint size, uint seed)
    {
        EncodeData(data, size, seed);
    }

    // OFFSET 0x0201D608
    private static ushort LCRNG_NextFrom(ref uint seed)
    {
        seed = seed * LcrngMultiplier + LcrngIncrement;
        return (ushort)(seed >> 16);
    }

    // OFFSET 0x0201D628
    public static ushort CalcCRC16Checksum(byte[] data, uint dataLength)
    {
        ushort crc = CrcInitialValue;
        for (uint i = 0; i < dataLength; i++)
        {
            crc = (ushort)((crc << 8) ^ sCrcTable![((crc >> 8) ^ data[i]) & 0xFF]);
        }
        return crc;
    }

    // OFFSET 0x0201D640
    public static void InitCRC16Table(int heapID)
    {
        if (sCrcTable != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sCrcTable = new ushort[256];
        for (int i = 0; i < 256; i++)
        {
            ushort value = (ushort)(i << 8);
            for (int bit = 0; bit < 8; bit++)
            {
                value = (ushort)((value & 0x8000) != 0 ? (value << 1) ^ CrcPolynomial : value << 1);
            }
            sCrcTable[i] = value;
        }
    }
}
