namespace PlatinumSharp.Game;

public struct GridMenuCursorPosition
{
    public byte XCoord1;
    public byte YCoord1;
    public byte XCoord2;
    public byte YCoord2;
    public byte UpIndex;
    public byte DownIndex;
    public byte LeftIndex;
    public byte RightIndex;
}

public static class GridMenuCursor
{
    public const int DirectionUp = 0;
    public const int DirectionDown = 1;
    public const int DirectionLeft = 2;
    public const int DirectionRight = 3;

    // OFFSET 0x0201E010
    public static void GridMenuCursor_GetFirstCoords(in GridMenuCursorPosition position, out byte xCoord, out byte yCoord)
    {
        xCoord = position.XCoord1;
        yCoord = position.YCoord1;
    }

    // OFFSET 0x0201E01C
    public static void GridMenuCursor_GetSecondCoords(in GridMenuCursorPosition position, out byte xCoord, out byte yCoord)
    {
        xCoord = position.XCoord2;
        yCoord = position.YCoord2;
    }

    // OFFSET 0x0201E028
    public static byte GridMenuCursor_CheckNavigation(GridMenuCursorPosition[] positions, ref byte xCoord1, ref byte yCoord1, ref byte xCoord2, ref byte yCoord2, byte currentPositionIndex, byte direction)
    {
        byte nextIndex = currentPositionIndex;
        switch (direction)
        {
            case DirectionUp:
                nextIndex = positions[currentPositionIndex].UpIndex;
                break;
            case DirectionDown:
                nextIndex = positions[currentPositionIndex].DownIndex;
                break;
            case DirectionLeft:
                nextIndex = positions[currentPositionIndex].LeftIndex;
                break;
            case DirectionRight:
                nextIndex = positions[currentPositionIndex].RightIndex;
                break;
        }
        xCoord1 = positions[nextIndex].XCoord1;
        yCoord1 = positions[nextIndex].YCoord1;
        xCoord2 = positions[nextIndex].XCoord2;
        yCoord2 = positions[nextIndex].YCoord2;
        return nextIndex;
    }

    public static byte GridMenuCursor_CheckNavigation(GridMenuCursorPosition[] positions, byte currentPositionIndex, byte direction)
    {
        return direction switch
        {
            DirectionUp => positions[currentPositionIndex].UpIndex,
            DirectionDown => positions[currentPositionIndex].DownIndex,
            DirectionLeft => positions[currentPositionIndex].LeftIndex,
            DirectionRight => positions[currentPositionIndex].RightIndex,
            _ => currentPositionIndex,
        };
    }
}

public static class AsciiUtil
{
    private const int MaxCopyLength = 0x100;
    private const int ResourceNameLength = 16;
    private const ushort FirstHalfWidthChar = 0x121;

    // OFFSET 0x0201E0A4
    public static int Ascii_Length(byte[] str, int start = 0)
    {
        int length = 0;
        while ((sbyte)str[start + length] != 0)
        {
            length++;
        }
        return length;
    }

    // OFFSET 0x0201E0B8
    public static int Ascii_CopyToTerminator(byte[] source, int sourcePos, byte[] destination, byte terminator)
    {
        for (int i = 0; i < MaxCopyLength; i++)
        {
            destination[i] = source[sourcePos + i];
            if (terminator == source[sourcePos + i] || source[sourcePos + i] == 0)
            {
                destination[i] = 0;
                if (terminator == '\r' && source[sourcePos + i + 1] == '\n')
                {
                    return sourcePos + i + 2;
                }
                return sourcePos + i + 1;
            }
        }
        return -1;
    }

    // OFFSET 0x0201E0FC
    public static int Ascii_ConvertToInt(byte[] str)
    {
        int length = Ascii_Length(str);
        int multiplier = 1;
        int result = 0;
        for (int i = length - 1; i >= 0; i--)
        {
            sbyte character = (sbyte)str[i];
            if (character >= '0' && character <= '9')
            {
                result += multiplier * (character - '0');
            }
            else if (i == 0)
            {
                if (character == '-')
                {
                    result *= -1;
                }
            }
            else
            {
                return -1;
            }
            multiplier *= 10;
        }
        return result;
    }

    // OFFSET 0x0201E140
    public static void Ascii_SetResourceName(byte[] resourceName, byte[] source)
    {
        Array.Clear(resourceName, 0, ResourceNameLength);
        byte length = (byte)Ascii_Length(source);
        for (byte i = 0; i < length; i++)
        {
            resourceName[i] = source[i];
        }
    }

    // OFFSET 0x0201E17C
    public static bool IsFullWidthChar(ushort character)
    {
        return character < FirstHalfWidthChar;
    }
}

public static class FxUtil
{
    private const int AngleQuarter = 0x4000;
    private const int AngleThreeQuarters = 0xC000;

    // OFFSET 0x0201E190
    public static int FX_Min(int a, int b)
    {
        return a >= b ? b : a;
    }

    // OFFSET 0x0201E198
    public static int FX_Max(int a, int b)
    {
        return a <= b ? b : a;
    }

    // OFFSET 0x0201E1A0
    public static int VEC_AngleBetween(in VecFx32 first, in VecFx32 second)
    {
        Mtx.Normalize(first, out VecFx32 a);
        Mtx.Normalize(second, out VecFx32 b);
        int dot = Fx32.Mul(a.Z, b.Z) + Fx32.Mul(a.X, b.X);
        int cross = Fx32.Mul(a.Z, b.X) - Fx32.Mul(a.X, b.Z);
        if (dot == 0)
        {
            if (cross > 0)
            {
                return AngleQuarter;
            }
            return AngleThreeQuarters;
        }
        return Mtx.Atan2Idx(cross, dot);
    }

    // OFFSET 0x0201E268
    public static void MTX_Rot33Vec(out MtxFx33 rotation, in VecFx32 angles)
    {
        int index = (ushort)angles.X >> 4;
        Mtx.RotX33(out rotation, Render3D.Sin(index), Render3D.Cos(index));
        index = (ushort)angles.Y >> 4;
        Mtx.RotY33(out MtxFx33 axisRotation, Render3D.Sin(index), Render3D.Cos(index));
        Mtx.Concat33(rotation, axisRotation, out rotation);
        index = (ushort)angles.Z >> 4;
        Mtx.RotZ33(out axisRotation, Render3D.Sin(index), Render3D.Cos(index));
        Mtx.Concat33(rotation, axisRotation, out rotation);
    }

    // OFFSET 0x0201E2E0
    public static void MTX_Rot33Angles(out MtxFx33 rotation, ushort alpha, ushort beta, ushort gamma)
    {
        Mtx.RotX33(out rotation, MathUtil.CalcSineDegrees(alpha), MathUtil.CalcCosineDegrees(alpha));
        Mtx.RotY33(out MtxFx33 axisRotation, MathUtil.CalcSineDegrees(beta), MathUtil.CalcCosineDegrees(beta));
        Mtx.Concat33(rotation, axisRotation, out rotation);
        Mtx.RotZ33(out axisRotation, MathUtil.CalcSineDegrees(gamma), MathUtil.CalcCosineDegrees(gamma));
        Mtx.Concat33(rotation, axisRotation, out rotation);
    }

    // OFFSET 0x0201E34C
    public static void CalcLinearFov(ushort angularFov, int distance, int aspectRatio, out int width, out int height)
    {
        int tangent = Fx32.Div(Render3D.Sin(angularFov >> 4), Render3D.Cos(angularFov >> 4));
        height = Fx32.Mul(Fx32.Mul(distance, tangent), 2 * Fx32.One);
        width = Fx32.Mul(height, aspectRatio);
    }
}

public static class IntDistance
{
    // OFFSET 0x0201E3BC
    public static uint CalcDistance2D(int x0, int y0, int x1, int y1)
    {
        int deltaX = x0 - x1;
        int deltaY = y0 - y1;
        return (uint)(Fx32.Sqrt((deltaX * deltaX + deltaY * deltaY) << Fx32.Shift) >> Fx32.Shift);
    }
}
