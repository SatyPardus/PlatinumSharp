using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class FontSpecialCharsContext
{
    private const int FontArchive = 0xE;
    private const int SpecialCharsMember = 4;
    private const int GlyphSize = 8;
    private const int GlyphBytes = 0x20;
    private const ushort FirstDigit = 0xA2;
    private const ushort LastDigit = 0xAB;
    private const uint LabelTableAddress = 0x020E4FEC;

    public byte[]? File;
    public CharacterData? CharData;
    public readonly ushort[] Text = new ushort[16];
    public uint BgColor;

    private static byte[] Slice(byte[] data, int offset, int length)
    {
        byte[] slice = new byte[length];
        Array.Copy(data, offset, slice, 0, Math.Min(length, data.Length - offset));
        return slice;
    }

    // OFFSET 0x0200C440
    public static FontSpecialCharsContext FontSpecialChars_Init(uint fgColor, uint shadowColor, uint bgColor, int heapID)
    {
        FontSpecialCharsContext context = new();
        context.File = Graphics.Graphics_GetCharData(FontArchive, SpecialCharsMember, 1, out context.CharData, heapID);
        byte[] rawData = context.CharData!.RawData;
        uint[] colors = { bgColor, fgColor, shadowColor };
        for (uint i = 0; i < context.CharData.SizeBytes; i++)
        {
            int high = rawData[i] >> 4;
            int low = rawData[i] & 0xF;
            if (high <= 2 && low <= 2)
            {
                rawData[i] = (byte)((colors[high] << 4) | colors[low]);
            }
        }
        context.BgColor = bgColor;
        return context;
    }

    // OFFSET 0x0200C560
    public static void FontSpecialChars_Free(FontSpecialCharsContext? context)
    {
        if (context != null)
        {
            context.File = null;
        }
    }

    // OFFSET 0x0200C578
    public static void FontSpecialChars_DrawPartyScreenLevelText(FontSpecialCharsContext context, int type, object window, uint x, uint y)
    {
        ushort[] label = RomData.Arm9U16(LabelTableAddress + (uint)type * 4, 2);
        int offset = label[0];
        int width = label[1];
        byte[] source = Slice(context.CharData!.RawData, offset, width * GlyphSize / 2);
        Bg.Window_BlitBitmapRect(window, source, 0, 0, width, GlyphSize, (ushort)x, (ushort)y, width, GlyphSize);
    }

    // OFFSET 0x0200C5BC
    public static void FontSpecialChars_DrawPartyScreenHPText(FontSpecialCharsContext context, int num, uint maxDigits, int paddingMode, object window, uint x, uint y)
    {
        CharCode.CharCode_FromInt(context.Text, 0, num, paddingMode, maxDigits);
        for (int i = 0; context.Text[i] != CharCode.EOS; i++)
        {
            ushort c = context.Text[i];
            if (c >= FirstDigit && c <= LastDigit)
            {
                byte[] source = Slice(context.CharData!.RawData, (c - FirstDigit) * GlyphBytes, GlyphBytes);
                Bg.Window_BlitBitmapRect(window, source, 0, 0, GlyphSize, GlyphSize, (ushort)x, (ushort)y, GlyphSize, GlyphSize);
            }
            else
            {
                Bg.Window_FillRectWithColor(window, (byte)context.BgColor, (ushort)x, (ushort)y, GlyphSize, GlyphSize);
            }
            x += GlyphSize;
        }
    }

    // OFFSET 0x0200C648
    public static void FontSpecialChars_DrawPartyScreenText(FontSpecialCharsContext context, int type, int num, uint maxDigits, int paddingMode, object window, uint x, uint y)
    {
        FontSpecialChars_DrawPartyScreenLevelText(context, type, window, x, y);
        FontSpecialChars_DrawPartyScreenHPText(context, num, maxDigits, paddingMode, window, x + 0x10, y);
    }

    // OFFSET 0x0200C67C
    public static void FontSpecialChars_DrawBattleScreenText(FontSpecialCharsContext context, int num, uint maxDigits, int paddingMode, byte[] dst)
    {
        byte fill = (byte)((context.BgColor << 4) | context.BgColor);
        CharCode.CharCode_FromInt(context.Text, 0, num, paddingMode, maxDigits);
        for (int i = 0; context.Text[i] != CharCode.EOS; i++)
        {
            ushort c = context.Text[i];
            int offset = i * GlyphBytes;
            if (c >= FirstDigit && c <= LastDigit)
            {
                Array.Copy(context.CharData!.RawData, (c - FirstDigit) * GlyphBytes, dst, offset, GlyphBytes);
            }
            else
            {
                Array.Fill(dst, fill, offset, GlyphBytes);
            }
        }
    }
}
