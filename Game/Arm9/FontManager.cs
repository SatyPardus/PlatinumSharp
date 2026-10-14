using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void FontGlyphLoadFunc(FontManager fontManager, ushort glyphIndex, TextGlyph glyph);

public delegate int FontGlyphWidthFunc(FontManager fontManager, int glyphIndex);

public static class TextGlyphDecoder
{
    private const int LookupSize = 256;

    private static readonly ushort[] sHalfRowLookup = new ushort[LookupSize];
    private static ushort sFgColor;
    private static ushort sShadowColor;
    private static ushort sBgColor;
    private static byte[]? sFontAttributes;

    public static byte[]? FontAttributes => sFontAttributes;

    // OFFSET 0x0201D670
    public static void Text_SetFontAttributes(byte[] attributes)
    {
        sFontAttributes = attributes;
    }

    // OFFSET 0x0201D9FC
    public static void Text_GenerateHalfRowLookupTable(int fgColor, int bgColor, int shadowColor)
    {
        int[] colors = { 0, fgColor, shadowColor, bgColor };
        sBgColor = (ushort)bgColor;
        sFgColor = (ushort)fgColor;
        sShadowColor = (ushort)shadowColor;
        int index = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    for (int l = 0; l < 4; l++)
                    {
                        sHalfRowLookup[index++] = (ushort)((colors[l] << 12) | (colors[k] << 8) | (colors[j] << 4) | colors[i]);
                    }
                }
            }
        }
    }

    // OFFSET 0x0201DAA0
    public static void Text_DecompressGlyphTile(byte[] source, int sourceOffset, byte[] destination, int destinationOffset)
    {
        for (int i = 0; i < 8; i++)
        {
            int packed = Bin.U16(source, sourceOffset + i * 2);
            Bin.W16(destination, destinationOffset + i * 4, sHalfRowLookup[packed >> 8]);
            Bin.W16(destination, destinationOffset + i * 4 + 2, sHalfRowLookup[packed & 0xFF]);
        }
    }
}

public sealed class FontManager
{
    public const int GlyphAccessImmediate = 0;
    public const int GlyphAccessLazy = 1;

    private const int HeaderSize = 0x10;
    private const int GlyphTileBytes = 0x10;
    private const int FallbackGlyph = 0x1AB;
    private const int LazyBufferSize = 0x40;
    private const int MaxGlyphTiles = 2;
    private const ushort EndOfString = 0xFFFF;
    private const ushort NewLine = 0xE000;
    private const int CursorControlArg = 0x0203;
    private const int CursorControlBias = 0xC;

    private static readonly byte[] sGlyphShapes = { 0, 1, 2, 3 };
    private static readonly Action<FontManager, int>[] sInitGlyphAccessFuncs = { FontManager_InitImmediateAccess, FontManager_InitLazyAccess };
    private static readonly Action<FontManager>[] sFreeGlyphAccessFuncs = { FontManager_FreeImmediateAccess, FontManager_FreeLazyAccess };

    public int GlyphAccessMode;
    public FontGlyphLoadFunc? GlyphLoadFunc;
    public int GlyphShape;
    public int GlyphSize;
    public byte[]? GlyphBuffer;
    public readonly byte[] LazyGlyphBuffer = new byte[LazyBufferSize];
    public NARC? Narc;
    public uint ArcMember;
    public uint GlyphDataOffset;
    public uint WidthTableOffset;
    public uint GlyphCount;
    public byte MaxWidth;
    public byte Height;
    public byte GlyphTilesWide;
    public byte GlyphTilesHigh;
    public int IsMonospace;
    public FontGlyphWidthFunc? GlyphWidthFunc;
    public byte[]? WidthTable;

    // OFFSET 0x020232E0
    public static FontManager FontManager_New(int narcID, uint arcMember, int glyphAccessMode, int isMonospace, int heapID)
    {
        FontManager fontManager = new();
        FontManager_Init(fontManager, narcID, arcMember, isMonospace, heapID);
        FontManager_SetGlyphAccess(fontManager, glyphAccessMode, heapID);
        return fontManager;
    }

    // OFFSET 0x02023318
    public static void FontManager_Free(FontManager fontManager)
    {
        FontManager_FreeGlyphAccess(fontManager);
        FontManager_FreeResources(fontManager);
    }

    // OFFSET 0x02023330
    public static void FontManager_SwitchGlyphAccess(FontManager fontManager, int glyphAccessMode, int heapID)
    {
        if (fontManager.GlyphAccessMode != glyphAccessMode)
        {
            FontManager_FreeGlyphAccess(fontManager);
            FontManager_SetGlyphAccess(fontManager, glyphAccessMode, heapID);
        }
    }

    // OFFSET 0x02023350
    private static void FontManager_Init(FontManager fontManager, int narcID, uint arcMember, int isMonospace, int heapID)
    {
        fontManager.Narc = NARC.NARC_ctor(narcID, heapID);
        if (fontManager.Narc == null)
        {
            return;
        }
        byte[] header = new byte[HeaderSize];
        NARC.NARC_ReadFromMember(fontManager.Narc, arcMember, 0, HeaderSize, header);
        fontManager.GlyphDataOffset = Bin.U32(header, 0);
        fontManager.WidthTableOffset = Bin.U32(header, 4);
        fontManager.GlyphCount = Bin.U32(header, 8);
        fontManager.MaxWidth = header[0xC];
        fontManager.Height = header[0xD];
        fontManager.GlyphTilesWide = header[0xE];
        fontManager.GlyphTilesHigh = header[0xF];
        fontManager.IsMonospace = isMonospace;
        if (isMonospace != 0)
        {
            fontManager.WidthTable = null;
            fontManager.GlyphWidthFunc = FontManager_GetFixedGlyphWidth;
        }
        else
        {
            if (fontManager.WidthTableOffset == 0)
            {
                ErrorHandling.GF_AssertFail();
            }
            fontManager.WidthTable = new byte[fontManager.GlyphCount];
            fontManager.GlyphWidthFunc = FontManager_GetVariableGlyphWidth;
            NARC.NARC_ReadFromMember(fontManager.Narc, arcMember, fontManager.WidthTableOffset, fontManager.GlyphCount, fontManager.WidthTable);
        }
        if (fontManager.GlyphTilesWide > MaxGlyphTiles || fontManager.GlyphTilesHigh > MaxGlyphTiles)
        {
            ErrorHandling.GF_AssertFail();
        }
        fontManager.GlyphShape = sGlyphShapes[(fontManager.GlyphTilesWide - 1) * 2 + (fontManager.GlyphTilesHigh - 1)];
        fontManager.GlyphSize = fontManager.GlyphTilesWide * GlyphTileBytes * fontManager.GlyphTilesHigh;
        fontManager.ArcMember = arcMember;
    }

    // OFFSET 0x02023408
    private static void FontManager_FreeResources(FontManager fontManager)
    {
        if (fontManager.WidthTable != null)
        {
            fontManager.WidthTable = null;
        }
        if (fontManager.Narc != null)
        {
            NARC.NARC_dtor(fontManager.Narc);
        }
    }

    // OFFSET 0x02023424
    private static void FontManager_SetGlyphAccess(FontManager fontManager, int glyphAccessMode, int heapID)
    {
        fontManager.GlyphAccessMode = glyphAccessMode;
        sInitGlyphAccessFuncs[glyphAccessMode](fontManager, heapID);
    }

    // OFFSET 0x0202343C
    private static void FontManager_InitImmediateAccess(FontManager fontManager, int heapID)
    {
        uint size = (uint)fontManager.GlyphSize * fontManager.GlyphCount;
        fontManager.GlyphBuffer = new byte[size];
        fontManager.GlyphLoadFunc = FontManager_LoadGlyphImmediate;
        NARC.NARC_ReadFromMember(fontManager.Narc!, fontManager.ArcMember, fontManager.GlyphDataOffset, size, fontManager.GlyphBuffer);
    }

    // OFFSET 0x0202346C
    private static void FontManager_InitLazyAccess(FontManager fontManager, int heapID)
    {
        fontManager.GlyphLoadFunc = FontManager_LoadGlyphLazy;
    }

    // OFFSET 0x02023478
    private static void FontManager_FreeGlyphAccess(FontManager fontManager)
    {
        sFreeGlyphAccessFuncs[fontManager.GlyphAccessMode](fontManager);
    }

    // OFFSET 0x0202348C
    private static void FontManager_FreeImmediateAccess(FontManager fontManager)
    {
        fontManager.GlyphBuffer = null;
    }

    // OFFSET 0x0202349C
    private static void FontManager_FreeLazyAccess(FontManager fontManager)
    {
    }

    // OFFSET 0x020234A0
    public static void FontManager_TryLoadGlyph(FontManager fontManager, ushort charcode, TextGlyph glyph)
    {
        ushort glyphIndex;
        if (charcode <= fontManager.GlyphCount)
        {
            glyphIndex = (ushort)(charcode - 1);
        }
        else
        {
            glyphIndex = FallbackGlyph;
        }
        fontManager.GlyphLoadFunc!(fontManager, glyphIndex, glyph);
    }

    private static void FontManager_DecompressGlyph(FontManager fontManager, byte[] source, int sourceOffset, ushort glyphIndex, TextGlyph glyph)
    {
        switch (fontManager.GlyphShape)
        {
            case 0:
                TextGlyphDecoder.Text_DecompressGlyphTile(source, sourceOffset, glyph.Pixels, 0);
                break;
            case 1:
                TextGlyphDecoder.Text_DecompressGlyphTile(source, sourceOffset, glyph.Pixels, 0);
                TextGlyphDecoder.Text_DecompressGlyphTile(source, sourceOffset + 0x10, glyph.Pixels, 0x40);
                break;
            case 2:
                TextGlyphDecoder.Text_DecompressGlyphTile(source, sourceOffset, glyph.Pixels, 0);
                TextGlyphDecoder.Text_DecompressGlyphTile(source, sourceOffset + 0x10, glyph.Pixels, 0x20);
                break;
            case 3:
                TextGlyphDecoder.Text_DecompressGlyphTile(source, sourceOffset, glyph.Pixels, 0);
                TextGlyphDecoder.Text_DecompressGlyphTile(source, sourceOffset + 0x10, glyph.Pixels, 0x20);
                TextGlyphDecoder.Text_DecompressGlyphTile(source, sourceOffset + 0x20, glyph.Pixels, 0x40);
                TextGlyphDecoder.Text_DecompressGlyphTile(source, sourceOffset + 0x30, glyph.Pixels, 0x60);
                break;
        }
        glyph.Width = (byte)fontManager.GlyphWidthFunc!(fontManager, glyphIndex);
        glyph.Height = fontManager.Height;
    }

    // OFFSET 0x020234BC
    private static void FontManager_LoadGlyphImmediate(FontManager fontManager, ushort glyphIndex, TextGlyph glyph)
    {
        FontManager_DecompressGlyph(fontManager, fontManager.GlyphBuffer!, glyphIndex * fontManager.GlyphSize, glyphIndex, glyph);
    }

    // OFFSET 0x02023564
    private static void FontManager_LoadGlyphLazy(FontManager fontManager, ushort glyphIndex, TextGlyph glyph)
    {
        NARC.NARC_ReadFromMember(fontManager.Narc!, fontManager.ArcMember, (uint)(fontManager.GlyphDataOffset + glyphIndex * fontManager.GlyphSize), (uint)fontManager.GlyphSize, fontManager.LazyGlyphBuffer);
        FontManager_DecompressGlyph(fontManager, fontManager.LazyGlyphBuffer, 0, glyphIndex, glyph);
    }

    // OFFSET 0x02023620
    public static int FontManager_CalcStringWidth(FontManager fontManager, ushort[]? characters, int letterSpacing)
    {
        int width = 0;
        int pos = 0;
        while (characters![pos] != EndOfString)
        {
            if (characters[pos] == CharCodeFormat.FormatArg)
            {
                pos = CharCodeFormat.CharCode_SkipFormatArg(characters, pos);
                if (characters[pos] == EndOfString)
                {
                    break;
                }
            }
            width += letterSpacing + fontManager.GlyphWidthFunc!(fontManager, characters[pos] - 1);
            pos++;
        }
        return width - letterSpacing;
    }

    // OFFSET 0x0202366C
    public static int FontManager_AreAllCharsValid(FontManager fontManager, ushort[] characters)
    {
        int pos = 0;
        while (characters[pos] != EndOfString)
        {
            if (characters[pos] == CharCodeFormat.FormatArg)
            {
                pos = CharCodeFormat.CharCode_SkipFormatArg(characters, pos);
                if (characters[pos] == EndOfString)
                {
                    return 1;
                }
            }
            if ((uint)(characters[pos] - 1) >= fontManager.GlyphCount)
            {
                return 0;
            }
            pos++;
        }
        return 1;
    }

    // OFFSET 0x020236B0
    private static int FontManager_GetVariableGlyphWidth(FontManager fontManager, int glyphIndex)
    {
        if ((uint)glyphIndex < fontManager.GlyphCount)
        {
            return fontManager.WidthTable![glyphIndex];
        }
        return fontManager.WidthTable![FallbackGlyph];
    }

    // OFFSET 0x020236C8
    private static int FontManager_GetFixedGlyphWidth(FontManager fontManager, int glyphIndex)
    {
        return fontManager.MaxWidth;
    }

    // OFFSET 0x020236D0
    public static int FontManager_CalcMaxLineWidth(FontManager fontManager, ushort[]? characters, int letterSpacing)
    {
        uint maxWidth = 0;
        uint lineWidth = 0;
        int pos = 0;
        while (characters![pos] != EndOfString)
        {
            if (characters[pos] == CharCodeFormat.FormatArg)
            {
                pos = CharCodeFormat.CharCode_SkipFormatArg(characters, pos);
            }
            else if (characters[pos] == NewLine)
            {
                if (maxWidth < lineWidth - (uint)letterSpacing)
                {
                    maxWidth = lineWidth - (uint)letterSpacing;
                }
                lineWidth = 0;
                pos++;
            }
            else
            {
                lineWidth += (uint)(letterSpacing + fontManager.GlyphWidthFunc!(fontManager, characters[pos] - 1));
                pos++;
            }
        }
        if (maxWidth < lineWidth - (uint)letterSpacing)
        {
            maxWidth = lineWidth - (uint)letterSpacing;
        }
        return (int)maxWidth;
    }

    // OFFSET 0x02023738
    public static int FontManager_CalcStringWidthWithCursorControl(FontManager fontManager, ushort[]? characters)
    {
        int width = 0;
        int pos = 0;
        while (characters![pos] != EndOfString)
        {
            if (characters[pos] == CharCodeFormat.FormatArg)
            {
                if (CharCodeFormat.CharCode_FormatArgType(characters, pos) == CursorControlArg)
                {
                    width = CharCodeFormat.CharCode_FormatArgParam(characters, pos, 0) - CursorControlBias;
                }
                pos = CharCodeFormat.CharCode_SkipFormatArg(characters, pos);
            }
            else
            {
                width += fontManager.GlyphWidthFunc!(fontManager, characters[pos] - 1);
                pos++;
            }
        }
        return width;
    }
}
