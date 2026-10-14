using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class CharCodeFormat
{
    public const ushort FormatArg = 0xFFFE;

    private const int TypeMask = 0xFF00;
    private const int TypeStringVariable = 0x0100;
    private const int TypeCompressedVariable = 0x0500;
    private const int TypeNumberVariable = 0x0600;

    // OFFSET 0x0201D0C8
    public static int CharCode_SkipFormatArg(ushort[] text, int pos)
    {
        if (text[pos] != FormatArg)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (text[pos] == FormatArg)
        {
            int paramCount = text[pos + 2];
            pos = pos + 3 + paramCount;
        }
        return pos;
    }

    // OFFSET 0x0201D0F0
    public static int CharCode_FormatArgType(ushort[] text, int pos)
    {
        if (text[pos] != FormatArg)
        {
            ErrorHandling.GF_AssertFail();
        }
        return text[pos + 1];
    }

    // OFFSET 0x0201D108
    public static bool CharCode_IsFormatArg(ushort[] text, int pos)
    {
        int type = CharCode_FormatArgType(text, pos) & TypeMask;
        if (type == TypeStringVariable || type == TypeNumberVariable || type == TypeCompressedVariable)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0201D134
    public static int CharCode_FormatArgParam(ushort[] text, int pos, int index)
    {
        if (text[pos] != FormatArg)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (index >= text[pos + 2])
        {
            ErrorHandling.GF_AssertFail();
        }
        return text[pos + 3 + index];
    }
}
