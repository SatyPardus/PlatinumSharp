using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class GameString
{
    public const ushort EOS = 0xFFFF;
    public const ushort LineFeed = 0xE000;
    public const ushort CompressedLeadChar = 0xF100;

    public const int PaddingModeNone = 0;
    public const int PaddingModeSpaces = 1;
    public const int PaddingModeZeroes = 2;

    private const uint IntegrityValue = 0xB6F8D2EC;
    private const ushort UnknownDigit = 0xE2;
    private const int CompressedBits = 9;
    private const int CompressedMask = 0x1FF;
    private const int WordBits = 15;
    private const int MaxParsableLength = 0x12;

    public ushort MaxSize;
    public ushort Size;
    public uint Integrity;
    public ushort[] Data = Array.Empty<ushort>();

    private static ushort[]? sDigitsJP;
    private static ushort[]? sDigitsEN;
    private static uint[]? sPowersOfTen;
    private static ushort[]? sDigits64JP;
    private static ushort[]? sDigits64EN;
    private static ulong[]? sPowersOfTen64;

    private static ulong[] LoadPowersOfTen64()
    {
        uint[] raw = RomData.Arm9U32(0x020E5778, 42);
        ulong[] table = new ulong[raw.Length / 2];
        for (int i = 0; i < table.Length; i++)
        {
            table[i] = raw[i * 2] | ((ulong)raw[i * 2 + 1] << 32);
        }
        return table;
    }

    private static void String_Check(GameString? str)
    {
        if (str == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (str!.Integrity != IntegrityValue)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02023790
    public static GameString String_Init(uint maxSize, int heapID)
    {
        GameString str = new();
        str.Integrity = IntegrityValue;
        str.MaxSize = (ushort)maxSize;
        str.Size = 0;
        str.Data = new ushort[str.MaxSize + 1];
        str.Data[0] = EOS;
        return str;
    }

    // OFFSET 0x020237BC
    public static void String_Free(GameString? str)
    {
        String_Check(str);
        str!.Integrity = IntegrityValue + 1;
    }

    // OFFSET 0x020237E8
    public static void String_Clear(GameString? str)
    {
        String_Check(str);
        str!.Size = 0;
        str.Data[0] = EOS;
    }

    // OFFSET 0x02023810
    public static void String_Copy(GameString? dst, GameString? src)
    {
        String_Check(dst);
        String_Check(src);
        if (dst!.MaxSize > src!.Size)
        {
            Array.Copy(src.Data, dst.Data, src.Size + 1);
            dst.Size = src.Size;
            return;
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x02023868
    public static GameString String_Clone(GameString? src, int heapID)
    {
        String_Check(src);
        GameString dst = String_Init((uint)(src!.Size + 1), heapID);
        String_Copy(dst, src);
        return dst;
    }

    // OFFSET 0x020238A0
    public static void String_FormatInt(GameString? dst, int num, uint maxDigits, int paddingMode, int charsetMode)
    {
        String_Check(dst);
        bool negative = num < 0;
        if (dst!.MaxSize <= maxDigits + (negative ? 1u : 0u))
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        sDigitsJP ??= RomData.Arm9U16(0x020E5744, 10);
        sDigitsEN ??= RomData.Arm9U16(0x020E5730, 10);
        sPowersOfTen ??= RomData.Arm9U32(0x020E5754, 9);
        ushort[] digits = charsetMode == 0 ? sDigitsJP : sDigitsEN;
        String_Clear(dst);
        if (negative)
        {
            num = -num;
            dst.Data[dst.Size++] = (ushort)(charsetMode == 0 ? 0xF1 : 0x1BE);
        }
        for (uint div = sPowersOfTen[maxDigits]; div != 0; div /= 10)
        {
            ushort digit = (ushort)(num / (int)div);
            num -= (int)(div * digit);
            if (paddingMode == PaddingModeZeroes)
            {
                dst.Data[dst.Size++] = digit < 10 ? digits[digit] : UnknownDigit;
            }
            else if (digit != 0 || div == 1)
            {
                paddingMode = PaddingModeZeroes;
                dst.Data[dst.Size++] = digit < 10 ? digits[digit] : UnknownDigit;
            }
            else if (paddingMode == PaddingModeSpaces)
            {
                dst.Data[dst.Size++] = (ushort)(charsetMode == 0 ? 1 : 0x1E2);
            }
        }
        dst.Data[dst.Size] = EOS;
    }

    // OFFSET 0x020239D4
    public static void String_FormatU64(GameString? dst, ulong num, uint maxDigits, int paddingMode, int charsetMode)
    {
        String_Check(dst);
        if (dst!.MaxSize <= maxDigits)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        sDigits64JP ??= RomData.Arm9U16(0x020E5708, 10);
        sDigits64EN ??= RomData.Arm9U16(0x020E571C, 10);
        sPowersOfTen64 ??= LoadPowersOfTen64();
        ushort[] digits = charsetMode == 0 ? sDigits64JP : sDigits64EN;
        String_Clear(dst);
        for (ulong div = sPowersOfTen64[maxDigits]; div != 0; div /= 10)
        {
            ulong digit = num / div;
            num -= div * digit;
            if (paddingMode == PaddingModeZeroes)
            {
                dst.Data[dst.Size++] = digit < 10 ? digits[digit] : UnknownDigit;
            }
            else if (digit != 0 || div == 1)
            {
                paddingMode = PaddingModeZeroes;
                dst.Data[dst.Size++] = digit < 10 ? digits[digit] : UnknownDigit;
            }
            else if (paddingMode == PaddingModeSpaces)
            {
                dst.Data[dst.Size++] = (ushort)(charsetMode == 0 ? 1 : 0x1DE);
            }
        }
        dst.Data[dst.Size] = EOS;
    }

    // OFFSET 0x02023B38
    public static ulong String_AtoI(GameString str, ref bool success)
    {
        ulong result = 0;
        ulong multiplier = 1;
        if (str.Size > MaxParsableLength)
        {
            return 0;
        }
        for (int i = str.Size - 1; i >= 0; i--)
        {
            ulong digit = (ulong)(long)(str.Data[i] - 0xA2);
            if (digit >= 10)
            {
                digit = (ulong)(long)(str.Data[i] - 0x121);
                if (digit >= 10)
                {
                    success = false;
                    return result;
                }
            }
            result += digit * multiplier;
            multiplier *= 10;
        }
        success = true;
        return result;
    }

    // OFFSET 0x02023BE0
    public static int String_Compare(GameString? str1, GameString? str2)
    {
        String_Check(str1);
        String_Check(str2);
        for (int i = 0; str1!.Data[i] == str2!.Data[i]; i++)
        {
            if (str1.Data[i] == EOS)
            {
                return 0;
            }
        }
        return 1;
    }

    // OFFSET 0x02023C3C
    public static uint String_Length(GameString? str)
    {
        String_Check(str);
        return str!.Size;
    }

    // OFFSET 0x02023C5C
    public static uint String_NumLines(GameString? str)
    {
        String_Check(str);
        uint count = 1;
        for (int i = 0; i < str!.Size; i++)
        {
            if (str.Data[i] == LineFeed)
            {
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x02023C9C
    public static void String_CopyLineNum(GameString? dst, GameString? src, uint lineNum)
    {
        String_Check(src);
        String_Check(dst);
        int i = 0;
        if (lineNum != 0)
        {
            for (i = 0; i < src!.Size; i++)
            {
                if (src.Data[i] == LineFeed && --lineNum == 0)
                {
                    i++;
                    break;
                }
            }
        }
        String_Clear(dst);
        for (; i < src!.Size; i++)
        {
            ushort c = src.Data[i];
            if (c == LineFeed)
            {
                break;
            }
            String_AppendChar(dst, c);
        }
    }

    // OFFSET 0x02023D28
    public static void String_CopyChars(GameString? dst, ushort[] src, int srcPos = 0)
    {
        String_Check(dst);
        dst!.Size = 0;
        while (src[srcPos] != EOS)
        {
            if (dst.Size >= dst.MaxSize - 1)
            {
                ErrorHandling.GF_AssertFail();
                break;
            }
            dst.Data[dst.Size++] = src[srcPos++];
        }
        dst.Data[dst.Size] = EOS;
    }

    // OFFSET 0x02023D8C
    public static void String_CopyNumChars(GameString? dst, ushort[] src, uint num)
    {
        String_Check(dst);
        if (num > dst!.MaxSize)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        Array.Copy(src, dst.Data, num);
        uint count = 0;
        while (count < num && dst.Data[count] != EOS)
        {
            count++;
        }
        dst.Size = (ushort)count;
        if (count == num)
        {
            dst.Data[num - 1] = EOS;
        }
    }

    // OFFSET 0x02023DF0
    public static void String_ToChars(GameString? src, ushort[] dst, uint dstSize)
    {
        String_Check(src);
        if ((uint)(src!.Size + 1) <= dstSize)
        {
            Array.Copy(src.Data, dst, src.Size + 1);
            return;
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x02023E2C
    public static ushort[] String_GetData(GameString? str)
    {
        String_Check(str);
        return str!.Data;
    }

    // OFFSET 0x02023E4C
    public static void String_Concat(GameString? dst, GameString? src)
    {
        String_Check(dst);
        String_Check(src);
        if (dst!.Size + src!.Size + 1 <= dst.MaxSize)
        {
            Array.Copy(src.Data, 0, dst.Data, dst.Size, src.Size + 1);
            dst.Size += src.Size;
            return;
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x02023EB0
    public static void String_AppendChar(GameString? str, ushort c)
    {
        String_Check(str);
        if (str!.Size + 1 < str.MaxSize)
        {
            str.Data[str.Size++] = c;
            str.Data[str.Size] = EOS;
            return;
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x02023EF8
    public static bool String_IsTrainerName(GameString str)
    {
        return str.Size != 0 && str.Data[0] == CompressedLeadChar;
    }

    // OFFSET 0x02023F10
    public static void String_ConcatTrainerName(GameString dst, GameString src)
    {
        if (!String_IsTrainerName(src))
        {
            String_Concat(dst, src);
            return;
        }
        int outPos = dst.Size;
        int inPos = 1;
        int bit = 0;
        int count = 0;
        while (true)
        {
            int c = (src.Data[inPos] >> bit) & CompressedMask;
            bit += CompressedBits;
            if (bit >= WordBits)
            {
                inPos++;
                bit -= WordBits;
                if (bit != 0)
                {
                    c |= (src.Data[inPos] << (CompressedBits - bit)) & CompressedMask;
                }
            }
            if (c == CompressedMask)
            {
                break;
            }
            dst.Data[outPos++] = (ushort)c;
            count++;
        }
        dst.Data[outPos] = EOS;
        dst.Size += (ushort)count;
    }

    // OFFSET 0x02023F8C
    public static void String_UpperChar(GameString? str, int index)
    {
        String_Check(str);
        if (str!.Size > index)
        {
            ushort c = str.Data[index];
            if (c >= 0x145 && c <= 0x15E)
            {
                str.Data[index] = (ushort)(c - 0x1A);
            }
        }
    }
}
