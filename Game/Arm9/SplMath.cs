using System.Numerics;

namespace PlatinumSharp.Game;

public static class SplMath
{
    public static int Div(int numerator, int denominator)
    {
        if (denominator == 0)
        {
            return numerator;
        }
        if (denominator == -1)
        {
            return unchecked(-numerator);
        }
        return numerator / denominator;
    }

    public static int Mod(int numerator, int denominator)
    {
        if (denominator == 0 || denominator == -1)
        {
            return 0;
        }
        return numerator % denominator;
    }

    public static uint ModU(uint numerator, uint denominator)
    {
        if (denominator == 0)
        {
            return 0;
        }
        return numerator % denominator;
    }

    public static int Dot(in VecFx32 a, in VecFx32 b)
    {
        return (int)(((long)a.Y * b.Y + (long)a.X * b.X + (long)a.Z * b.Z + 0x800) >> Fx32.Shift);
    }

    public static void Cross(in VecFx32 a, in VecFx32 b, out VecFx32 result)
    {
        VecFx32 cross;
        cross.X = (int)(((long)a.Y * b.Z - (long)a.Z * b.Y + 0x800) >> Fx32.Shift);
        cross.Y = (int)(((long)a.Z * b.X - (long)a.X * b.Z + 0x800) >> Fx32.Shift);
        cross.Z = (int)(((long)a.X * b.Y - (long)a.Y * b.X + 0x800) >> Fx32.Shift);
        result = cross;
    }

    public static int Dot(in VecFx16 a, in VecFx16 b)
    {
        return (int)(((long)(a.Y * b.Y + a.X * b.X) + (a.Z * b.Z + 0x800)) >> Fx32.Shift);
    }

    public static void Cross(in VecFx16 a, in VecFx16 b, out VecFx16 result)
    {
        VecFx16 cross;
        cross.X = (short)((a.Y * b.Z - a.Z * b.Y + 0x800) >> Fx32.Shift);
        cross.Y = (short)((a.Z * b.X - a.X * b.Z + 0x800) >> Fx32.Shift);
        cross.Z = (short)((a.X * b.Y - a.Y * b.X + 0x800) >> Fx32.Shift);
        result = cross;
    }

    private static uint SquareRoot(ulong value)
    {
        ulong root = (ulong)Math.Sqrt(value);
        while (root * root > value)
        {
            root--;
        }
        while (root < uint.MaxValue && (root + 1) * (root + 1) <= value)
        {
            root++;
        }
        return (uint)root;
    }

    private static long NormalizeFactor(ulong squaredLength)
    {
        if (squaredLength == 0)
        {
            return 0;
        }
        ulong quotient = (1UL << 56) / squaredLength;
        uint root = SquareRoot(squaredLength << 2);
        return unchecked((long)quotient * (int)root);
    }

    private static int NormalizeComponent(long factor, int component)
    {
        int high = (int)(unchecked(factor * component) >> 32);
        return unchecked(high + 0x1000) >> 13;
    }

    public static void Normalize(in VecFx32 source, out VecFx32 result)
    {
        ulong squaredLength = unchecked((ulong)((long)source.Y * source.Y + (long)source.X * source.X + (long)source.Z * source.Z));
        long factor = NormalizeFactor(squaredLength);
        VecFx32 normalized;
        normalized.X = NormalizeComponent(factor, source.X);
        normalized.Y = NormalizeComponent(factor, source.Y);
        normalized.Z = NormalizeComponent(factor, source.Z);
        result = normalized;
    }

    public static void Normalize(in VecFx16 source, out VecFx16 result)
    {
        ulong squaredLength = (ulong)((long)(source.X * source.X) + (source.Y * source.Y) + (source.Z * source.Z));
        long factor = NormalizeFactor(squaredLength);
        VecFx16 normalized;
        normalized.X = (short)NormalizeComponent(factor, source.X);
        normalized.Y = (short)NormalizeComponent(factor, source.Y);
        normalized.Z = (short)NormalizeComponent(factor, source.Z);
        result = normalized;
    }

    public static void Identity43(out MtxFx43 matrix)
    {
        matrix = default;
        matrix.M00 = Fx32.One;
        matrix.M11 = Fx32.One;
        matrix.M22 = Fx32.One;
    }

    public static void Scale43(out MtxFx43 matrix, int x, int y, int z)
    {
        matrix = default;
        matrix.M00 = x;
        matrix.M11 = y;
        matrix.M22 = z;
    }

    private static int Row(int a0, int b0, int a1, int b1, int a2, int b2)
    {
        return (int)(((long)a0 * b0 + (long)a1 * b1 + (long)a2 * b2) >> Fx32.Shift);
    }

    public static void Concat43(in MtxFx43 a, in MtxFx43 b, out MtxFx43 result)
    {
        MtxFx43 product;
        product.M00 = Row(a.M00, b.M00, a.M01, b.M10, a.M02, b.M20);
        product.M01 = Row(a.M00, b.M01, a.M01, b.M11, a.M02, b.M21);
        product.M02 = Row(a.M00, b.M02, a.M01, b.M12, a.M02, b.M22);
        product.M10 = Row(a.M10, b.M00, a.M11, b.M10, a.M12, b.M20);
        product.M11 = Row(a.M10, b.M01, a.M11, b.M11, a.M12, b.M21);
        product.M12 = Row(a.M10, b.M02, a.M11, b.M12, a.M12, b.M22);
        product.M20 = Row(a.M20, b.M00, a.M21, b.M10, a.M22, b.M20);
        product.M21 = Row(a.M20, b.M01, a.M21, b.M11, a.M22, b.M21);
        product.M22 = Row(a.M20, b.M02, a.M21, b.M12, a.M22, b.M22);
        product.M30 = Row(a.M30, b.M00, a.M31, b.M10, a.M32, b.M20) + b.M30;
        product.M31 = Row(a.M30, b.M01, a.M31, b.M11, a.M32, b.M21) + b.M31;
        product.M32 = Row(a.M30, b.M02, a.M31, b.M12, a.M32, b.M22) + b.M32;
        result = product;
    }

    public static void Copy43To33(in MtxFx43 source, out MtxFx33 result)
    {
        result.M00 = source.M00;
        result.M01 = source.M01;
        result.M02 = source.M02;
        result.M10 = source.M10;
        result.M11 = source.M11;
        result.M12 = source.M12;
        result.M20 = source.M20;
        result.M21 = source.M21;
        result.M22 = source.M22;
    }

    public static void MultVec33(in VecFx32 vector, in MtxFx33 matrix, out VecFx32 result)
    {
        VecFx32 product;
        product.X = Row(vector.Y, matrix.M10, vector.X, matrix.M00, vector.Z, matrix.M20);
        product.Y = Row(vector.Y, matrix.M11, vector.X, matrix.M01, vector.Z, matrix.M21);
        product.Z = Row(vector.Y, matrix.M12, vector.X, matrix.M02, vector.Z, matrix.M22);
        result = product;
    }

    public static void MultVec43(in VecFx32 vector, in MtxFx43 matrix, out VecFx32 result)
    {
        VecFx32 product;
        product.X = Row(vector.Y, matrix.M10, vector.X, matrix.M00, vector.Z, matrix.M20) + matrix.M30;
        product.Y = Row(vector.Y, matrix.M11, vector.X, matrix.M01, vector.Z, matrix.M21) + matrix.M31;
        product.Z = Row(vector.Y, matrix.M12, vector.X, matrix.M02, vector.Z, matrix.M22) + matrix.M32;
        result = product;
    }

    public static Matrix4x4 ToMatrix(in MtxFx43 matrix)
    {
        const float One = Fx32.One;
        return new Matrix4x4(
            matrix.M00 / One, matrix.M01 / One, matrix.M02 / One, 0,
            matrix.M10 / One, matrix.M11 / One, matrix.M12 / One, 0,
            matrix.M20 / One, matrix.M21 / One, matrix.M22 / One, 0,
            matrix.M30 / One, matrix.M31 / One, matrix.M32 / One, 1);
    }

    private static int ToFx32(float value)
    {
        return (int)MathF.Round(value * Fx32.One);
    }

    public static MtxFx43 FromMatrix(in Matrix4x4 matrix)
    {
        MtxFx43 result;
        result.M00 = ToFx32(matrix.M11);
        result.M01 = ToFx32(matrix.M12);
        result.M02 = ToFx32(matrix.M13);
        result.M10 = ToFx32(matrix.M21);
        result.M11 = ToFx32(matrix.M22);
        result.M12 = ToFx32(matrix.M23);
        result.M20 = ToFx32(matrix.M31);
        result.M21 = ToFx32(matrix.M32);
        result.M22 = ToFx32(matrix.M33);
        result.M30 = ToFx32(matrix.M41);
        result.M31 = ToFx32(matrix.M42);
        result.M32 = ToFx32(matrix.M43);
        return result;
    }

    public static void LoadMtx43(in MtxFx43 matrix)
    {
        Render3D.PositionMatrix = ToMatrix(matrix);
    }

    public static void MultMtx43(in MtxFx43 matrix)
    {
        Render3D.PositionMatrix = ToMatrix(matrix) * Render3D.PositionMatrix;
    }

    public static void LoadIdentity()
    {
        Render3D.PositionMatrix = Matrix4x4.Identity;
    }
}
