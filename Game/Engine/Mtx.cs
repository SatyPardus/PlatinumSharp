namespace PlatinumSharp.Game;

public static class Mtx
{
    public static void Identity33(out MtxFx33 matrix)
    {
        matrix = default;
        matrix.M00 = Fx32.One;
        matrix.M11 = Fx32.One;
        matrix.M22 = Fx32.One;
    }

    public static void RotX33(out MtxFx33 matrix, int sin, int cos)
    {
        matrix = default;
        matrix.M00 = Fx32.One;
        matrix.M11 = cos;
        matrix.M12 = sin;
        matrix.M21 = -sin;
        matrix.M22 = cos;
    }

    public static void RotY33(out MtxFx33 matrix, int sin, int cos)
    {
        matrix = default;
        matrix.M00 = cos;
        matrix.M02 = -sin;
        matrix.M11 = Fx32.One;
        matrix.M20 = sin;
        matrix.M22 = cos;
    }

    public static void RotZ33(out MtxFx33 matrix, int sin, int cos)
    {
        matrix = default;
        matrix.M00 = cos;
        matrix.M01 = sin;
        matrix.M10 = -sin;
        matrix.M11 = cos;
        matrix.M22 = Fx32.One;
    }

    private static int Dot(int a0, int b0, int a1, int b1, int a2, int b2)
    {
        return (int)(((long)a0 * b0 + (long)a1 * b1 + (long)a2 * b2) >> Fx32.Shift);
    }

    public static void Concat33(in MtxFx33 a, in MtxFx33 b, out MtxFx33 result)
    {
        MtxFx33 product;
        product.M00 = Dot(a.M00, b.M00, a.M01, b.M10, a.M02, b.M20);
        product.M01 = Dot(a.M00, b.M01, a.M01, b.M11, a.M02, b.M21);
        product.M02 = Dot(a.M00, b.M02, a.M01, b.M12, a.M02, b.M22);
        product.M10 = Dot(a.M10, b.M00, a.M11, b.M10, a.M12, b.M20);
        product.M11 = Dot(a.M10, b.M01, a.M11, b.M11, a.M12, b.M21);
        product.M12 = Dot(a.M10, b.M02, a.M11, b.M12, a.M12, b.M22);
        product.M20 = Dot(a.M20, b.M00, a.M21, b.M10, a.M22, b.M20);
        product.M21 = Dot(a.M20, b.M01, a.M21, b.M11, a.M22, b.M21);
        product.M22 = Dot(a.M20, b.M02, a.M21, b.M12, a.M22, b.M22);
        result = product;
    }

    public static void Normalize(in VecFx32 source, out VecFx32 result)
    {
        double x = source.X;
        double y = source.Y;
        double z = source.Z;
        double length = Math.Sqrt(x * x + y * y + z * z);
        if (length == 0)
        {
            result = default;
            return;
        }
        result.X = (int)(x / length * Fx32.One);
        result.Y = (int)(y / length * Fx32.One);
        result.Z = (int)(z / length * Fx32.One);
    }

    public static void VecAdd(in VecFx32 a, in VecFx32 b, out VecFx32 result)
    {
        result.X = a.X + b.X;
        result.Y = a.Y + b.Y;
        result.Z = a.Z + b.Z;
    }

    public static void VecSubtract(in VecFx32 a, in VecFx32 b, out VecFx32 result)
    {
        result.X = a.X - b.X;
        result.Y = a.Y - b.Y;
        result.Z = a.Z - b.Z;
    }

    public static int VecMag(in VecFx32 vector)
    {
        ulong sum = (ulong)((long)vector.X * vector.X + (long)vector.Y * vector.Y + (long)vector.Z * vector.Z);
        ulong root = (ulong)Math.Sqrt(sum);
        while (root * root > sum)
        {
            root--;
        }
        while ((root + 1) * (root + 1) <= sum)
        {
            root++;
        }
        return (int)root;
    }

    public static ushort Atan2Idx(int y, int x)
    {
        double angle = Math.Atan2(y, x);
        return (ushort)(int)Math.Round(angle * 32768.0 / Math.PI);
    }
}
