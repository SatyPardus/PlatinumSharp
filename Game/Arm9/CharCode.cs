namespace PlatinumSharp.Game;

public static class CharCode
{
    public const ushort EOS = 0xFFFF;

    private const ushort UnknownDigit = 0xE2;
    private const ushort PaddingSpace = 0x1;

    private static uint[]? sPowersOfTen;
    private static ushort[]? sDigits;

    // OFFSET 0x020021B0
    public static int CharCode_Copy(ushort[] dst, int dstPos, ushort[] src, int srcPos)
    {
        while (src[srcPos] != EOS)
        {
            dst[dstPos++] = src[srcPos++];
        }
        dst[dstPos] = EOS;
        return dstPos;
    }

    // OFFSET 0x020021D0
    public static int CharCode_CopyNumChars(ushort[] dst, int dstPos, ushort[] src, int srcPos, uint num)
    {
        for (uint i = 0; i < num; i++)
        {
            dst[dstPos + i] = src[srcPos + i];
        }
        return dstPos + (int)num;
    }

    // OFFSET 0x020021F0
    public static uint CharCode_Length(ushort[] str, int pos)
    {
        uint length = 0;
        while (str[pos] != EOS)
        {
            pos++;
            length++;
        }
        return length;
    }

    // OFFSET 0x0200220C
    public static int CharCode_Compare(ushort[] str1, int pos1, ushort[] str2, int pos2)
    {
        while (str1[pos1] == str2[pos2])
        {
            if (str1[pos1] == EOS)
            {
                return 0;
            }
            pos1++;
            pos2++;
        }
        return 1;
    }

    // OFFSET 0x02002238
    public static int CharCode_CompareNumChars(ushort[] str1, int pos1, ushort[] str2, int pos2, uint num)
    {
        while (str1[pos1] == str2[pos2])
        {
            if (num == 0)
            {
                return 0;
            }
            if (str1[pos1] == EOS && str2[pos2] == EOS)
            {
                return 0;
            }
            pos1++;
            pos2++;
            num--;
        }
        return 1;
    }

    // OFFSET 0x02002278
    public static int CharCode_FillWith(ushort[] str, int pos, ushort fill, uint num)
    {
        for (uint i = 0; i < num; i++)
        {
            str[pos + i] = fill;
        }
        return pos + (int)num;
    }

    // OFFSET 0x02002294
    public static int CharCode_FillWithEOS(ushort[] str, int pos, uint num)
    {
        return CharCode_FillWith(str, pos, EOS, num);
    }

    // OFFSET 0x020022A4
    public static int CharCode_FromInt(ushort[] str, int pos, int value, int paddingMode, uint digits)
    {
        sPowersOfTen ??= RomData.Arm9U32(0x020E4CA4, 11);
        sDigits ??= RomData.Arm9U16(0x020E4C88, 10);
        uint remaining = (uint)value;
        for (uint divisor = sPowersOfTen[digits]; divisor != 0; divisor /= 10)
        {
            ushort digit = (ushort)(remaining / divisor);
            remaining -= divisor * digit;
            if (paddingMode == 2)
            {
                str[pos++] = digit >= 10 ? UnknownDigit : sDigits[digit];
            }
            else if (digit != 0 || divisor == 1)
            {
                paddingMode = 2;
                str[pos++] = digit >= 10 ? UnknownDigit : sDigits[digit];
            }
            else if (paddingMode == 1)
            {
                str[pos++] = PaddingSpace;
            }
        }
        str[pos] = EOS;
        return pos;
    }
}
