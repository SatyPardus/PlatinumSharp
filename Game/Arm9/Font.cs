using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class Font
{
    private const int FontCount = 4;
    private const int FontArchive = 0xE;
    private const int AttributeSize = 8;

    private static readonly TextGlyph sGlyph = new();
    private static readonly object?[] sGlyphData = new object?[FontCount];
    private static readonly FontManager?[] sManagers = new FontManager?[FontCount];
    private static ushort[]? sArchiveTable;
    private static byte[]? sAttributes;

    private static ushort[] ArchiveTable => sArchiveTable ??= RomData.Arm9U16(0x020E4CD4, FontCount * 2);

    private static byte[] Attributes => sAttributes ??= RomData.Arm9(0x020E4CE4, FontCount * AttributeSize);

    private static FontManager Manager(int font)
    {
        FontManager? manager = sManagers[font];
        if (manager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return manager!;
    }

    // OFFSET 0x02002B7C
    public static void Fonts_Init()
    {
        for (int i = 0; i < FontCount; i++)
        {
            sGlyphData[i] = null;
            sManagers[i] = null;
        }
        TextGlyphDecoder.Text_SetFontAttributes(Attributes);
    }

    // OFFSET 0x02002BB8
    public static void Font_InitManager(int font, int heapID)
    {
        sManagers[font] = FontManager.FontManager_New(FontArchive, ArchiveTable[font * 2], 1, ArchiveTable[font * 2 + 1], heapID);
    }

    // OFFSET 0x02002BEC
    public static void Font_UseImmediateGlyphAccess(int font, int heapID)
    {
        if (font >= FontCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        FontManager.FontManager_SwitchGlyphAccess(Manager(font), 0, heapID);
    }

    // OFFSET 0x02002C28
    public static void Font_UseLazyGlyphAccess(int font)
    {
        if (font >= FontCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        FontManager.FontManager_SwitchGlyphAccess(Manager(font), 1, 0);
    }

    // OFFSET 0x02002C60
    public static void Font_Free(int font)
    {
        if (font >= FontCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        object? glyphData = sGlyphData[font];
        if (glyphData != null)
        {
            int i;
            for (i = 0; i < FontCount; i++)
            {
                if (i != font && ArchiveTable[i * 2] == ArchiveTable[font * 2] && sManagers[i] != null)
                {
                    sGlyphData[i] = glyphData;
                    break;
                }
            }
            if (i == FontCount)
            {
                sGlyphData[font] = null;
            }
        }
        if (sManagers[font] != null)
        {
            FontManager.FontManager_Free(sManagers[font]!);
            sManagers[font] = null;
        }
    }

    // OFFSET 0x02002CFC
    public static TextGlyph Font_TryLoadGlyph(int font, ushort character)
    {
        FontManager.FontManager_TryLoadGlyph(sManagers[font]!, character, sGlyph);
        return sGlyph;
    }

    // OFFSET 0x02002D18
    public static int Font_RenderText(int font, TextPrinter printer)
    {
        if (!printer.GlyphFontSet)
        {
            printer.GlyphFontID = font & 0xF;
            printer.GlyphFontSet = true;
        }
        return RenderText.RenderTextStep(printer);
    }

    // OFFSET 0x02002D48
    public static int Font_CalcCharArrayWidth(int font, ushort[]? characters, int letterSpacing)
    {
        return FontManager.FontManager_CalcStringWidth(Manager(font), characters, letterSpacing);
    }

    // OFFSET 0x02002D7C
    public static int Font_CalcStringWidth(int font, GameString? text, int letterSpacing)
    {
        FontManager manager = Manager(font);
        return FontManager.FontManager_CalcStringWidth(manager, GameString.String_GetData(text), letterSpacing);
    }

    // OFFSET 0x02002DB4
    public static int Font_AreAllCharsValid(int font, GameString text, GameString buffer)
    {
        FontManager manager = Manager(font);
        GameString.String_Clear(buffer);
        GameString.String_ConcatTrainerName(buffer, text);
        return FontManager.FontManager_AreAllCharsValid(manager, GameString.String_GetData(buffer));
    }

    // OFFSET 0x02002DF8
    public static int Font_GetAttribute(int font, int attribute)
    {
        if (attribute < 0 || attribute >= AttributeSize)
        {
            return 0;
        }
        return Attributes[font * AttributeSize + attribute];
    }

    // OFFSET 0x02002E7C
    public static void Font_LoadTextPalette(int paletteLocation, int paletteSlotOffset, int heapID)
    {
        Graphics.Graphics_LoadPalette(FontArchive, 6, paletteLocation, (uint)paletteSlotOffset, 0x20, heapID);
    }

    // OFFSET 0x02002E98
    public static void Font_LoadScreenIndicatorsPalette(int paletteLocation, int paletteSlotOffset, int heapID)
    {
        Graphics.Graphics_LoadPalette(FontArchive, 7, paletteLocation, (uint)paletteSlotOffset, 0x20, heapID);
    }

    // OFFSET 0x02002EB4
    public static int Font_CalcMaxLineWidth(int font, GameString? text, int letterSpacing)
    {
        FontManager manager = Manager(font);
        return FontManager.FontManager_CalcMaxLineWidth(manager, GameString.String_GetData(text), letterSpacing);
    }

    // OFFSET 0x02002EEC
    public static int Font_CalcCenterAlignment(int font, GameString? text, int letterSpacing, int windowWidth)
    {
        int width = Font_CalcStringWidth(font, text, letterSpacing);
        if ((uint)width < (uint)windowWidth)
        {
            return (int)((uint)(windowWidth - width) >> 1);
        }
        return 0;
    }

    // OFFSET 0x02002F04
    public static int Font_CalcStringWidthWithCursorControl(int font, GameString? text)
    {
        FontManager manager = Manager(font);
        return FontManager.FontManager_CalcStringWidthWithCursorControl(manager, GameString.String_GetData(text));
    }
}
