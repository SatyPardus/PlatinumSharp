using static PlatinumSharp.Game.Globals;
using static PlatinumSharp.Game.Font;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class StringListEntry
{
    public GameString? Text;
    public object? Callback;
    public bool Terminator;
    public uint Index;
}

public delegate void ListMenuCursorCallback(ListMenu menu, uint index, byte onInit);

public delegate void ListMenuPrintCallback(ListMenu menu, uint index, byte y);

public sealed class ListMenuTemplate
{
    public StringListEntry[] Choices = Array.Empty<StringListEntry>();
    public ListMenuCursorCallback? CursorCallback;
    public ListMenuPrintCallback? PrintCallback;
    public Window Window = null!;
    public ushort Count;
    public ushort MaxDisplay;
    public byte HeaderXOffset;
    public byte TextXOffset;
    public byte CursorXOffset;
    public int YOffset;
    public int TextColorFg;
    public int TextColorBg;
    public int TextColorShadow;
    public int LetterSpacing;
    public int LineSpacing;
    public int PagerMode;
    public int FontID;
    public int CursorType;
    public object? Parent;

    public static ListMenuTemplate FromBytes(byte[] raw)
    {
        int spacing = (int)PlatinumSharp.Nitro.Bin.U16(raw, 0x1A);
        return new ListMenuTemplate
        {
            Count = (ushort)PlatinumSharp.Nitro.Bin.U16(raw, 0x10),
            MaxDisplay = (ushort)PlatinumSharp.Nitro.Bin.U16(raw, 0x12),
            HeaderXOffset = raw[0x14],
            TextXOffset = raw[0x15],
            CursorXOffset = raw[0x16],
            YOffset = raw[0x17] & 0xF,
            TextColorFg = raw[0x17] >> 4,
            TextColorBg = raw[0x18] & 0xF,
            TextColorShadow = raw[0x18] >> 4,
            LetterSpacing = spacing & 7,
            LineSpacing = (spacing >> 3) & 0xF,
            PagerMode = (spacing >> 7) & 3,
            FontID = (spacing >> 9) & 0x3F,
            CursorType = (spacing >> 15) & 1,
        };
    }

    public void CopyFrom(ListMenuTemplate other)
    {
        Choices = other.Choices;
        CursorCallback = other.CursorCallback;
        PrintCallback = other.PrintCallback;
        Window = other.Window;
        Count = other.Count;
        MaxDisplay = other.MaxDisplay;
        HeaderXOffset = other.HeaderXOffset;
        TextXOffset = other.TextXOffset;
        CursorXOffset = other.CursorXOffset;
        YOffset = other.YOffset;
        TextColorFg = other.TextColorFg;
        TextColorBg = other.TextColorBg;
        TextColorShadow = other.TextColorShadow;
        LetterSpacing = other.LetterSpacing;
        LineSpacing = other.LineSpacing;
        PagerMode = other.PagerMode;
        FontID = other.FontID;
        CursorType = other.CursorType;
        Parent = other.Parent;
    }
}

public sealed class ListMenu
{
    public const uint Cancel = 0xFFFFFFFE;
    public const uint NoChoice = 0xFFFFFFFF;
    public const uint Header = 0xFFFFFFFD;

    private const int KeyA = 0x1;
    private const int KeyB = 0x2;
    private const int KeyRight = 0x10;
    private const int KeyLeft = 0x20;
    private const int KeyUp = 0x40;
    private const int KeyDown = 0x80;
    private const int KeyR = 0x100;
    private const int KeyL = 0x200;

    public readonly ListMenuTemplate Template = new();
    public int AltTextColorFg;
    public int AltTextColorBg;
    public int AltTextColorShadow;
    public int AltLetterSpacing;
    public int AltFontID;
    public bool UseAltText;
    public ColoredArrow Cursor = null!;
    public ushort ListPos;
    public ushort CursorPos;
    public byte Unused30;
    public byte Unused31;
    public byte Unused32;
    public byte LastAction;
    public byte HeapID;

    // OFFSET 0x0200112C
    public static ListMenu ListMenu_New(ListMenuTemplate template, ushort startListPos, ushort startCursorPos, byte heapID)
    {
        ListMenu menu = new();
        menu.Template.CopyFrom(template);
        menu.Cursor = ColoredArrow.ColoredArrow_New(heapID);
        menu.ListPos = startListPos;
        menu.CursorPos = startCursorPos;
        menu.Unused30 = 0;
        menu.Unused31 = 0;
        menu.Unused32 = 0xFF;
        menu.LastAction = 0;
        menu.HeapID = heapID;
        ListMenuTemplate t = menu.Template;
        menu.AltTextColorFg = t.TextColorFg;
        menu.AltTextColorBg = t.TextColorBg;
        menu.AltTextColorShadow = t.TextColorShadow;
        menu.AltLetterSpacing = t.LetterSpacing;
        menu.AltFontID = t.FontID;
        menu.UseAltText = false;
        if (t.Count < t.MaxDisplay)
        {
            t.MaxDisplay = t.Count;
        }
        ColoredArrow.ColoredArrow_SetColor(menu.Cursor, (uint)((t.TextColorFg << 16) | (t.TextColorShadow << 8) | t.TextColorBg));
        Bg.Window_FillTilemap(t.Window, t.TextColorBg);
        PrintEntries(menu, menu.ListPos, 0, t.MaxDisplay);
        PrintCursor(menu);
        InvokeCursorCallback(menu, 1);
        Bg.Window_CopyToVRAM(template.Window);
        return menu;
    }

    // OFFSET 0x02001288
    public static uint ListMenu_ProcessInput(ListMenu menu)
    {
        menu.LastAction = 0;
        int pressed = gSystem.PressedKeys;
        if ((pressed & KeyA) != 0)
        {
            return menu.Template.Choices[menu.ListPos + menu.CursorPos].Index;
        }
        if ((pressed & KeyB) != 0)
        {
            return Cancel;
        }
        int repeat = gSystem.PressedKeysRepeatable;
        if ((repeat & KeyUp) != 0)
        {
            if (UpdateSelectedRow(menu, 1, 1, 0) == 0)
            {
                menu.LastAction = 1;
            }
            return NoChoice;
        }
        if ((repeat & KeyDown) != 0)
        {
            if (UpdateSelectedRow(menu, 1, 1, 1) == 0)
            {
                menu.LastAction = 2;
            }
            return NoChoice;
        }
        int pageUp;
        int pageDown;
        switch (menu.Template.PagerMode)
        {
            case 1:
                pageUp = repeat & KeyLeft;
                pageDown = repeat & KeyRight;
                break;
            case 2:
                pageUp = repeat & KeyL;
                pageDown = repeat & KeyR;
                break;
            default:
                pageUp = 0;
                pageDown = 0;
                break;
        }
        if (pageUp != 0)
        {
            if (UpdateSelectedRow(menu, 1, (byte)menu.Template.MaxDisplay, 0) == 0)
            {
                menu.LastAction = 3;
            }
            return NoChoice;
        }
        if (pageDown != 0)
        {
            if (UpdateSelectedRow(menu, 1, (byte)menu.Template.MaxDisplay, 1) == 0)
            {
                menu.LastAction = 4;
            }
            return NoChoice;
        }
        return NoChoice;
    }

    // OFFSET 0x02001384
    public static void ListMenu_Free(ListMenu menu, out ushort listPos, out ushort cursorPos)
    {
        listPos = menu.ListPos;
        cursorPos = menu.CursorPos;
        ColoredArrow.ColoredArrow_Free(menu.Cursor);
    }

    // OFFSET 0x020013AC
    public static void ListMenu_Draw(ListMenu menu)
    {
        Bg.Window_FillTilemap(menu.Template.Window, menu.Template.TextColorBg);
        PrintEntries(menu, menu.ListPos, 0, menu.Template.MaxDisplay);
        PrintCursor(menu);
        Bg.Window_CopyToVRAM(menu.Template.Window);
    }

    // OFFSET 0x020013D8
    public static void ListMenu_SetTextColors(ListMenu menu, byte fg, byte bg, byte shadow)
    {
        menu.Template.TextColorFg = fg & 0xF;
        menu.Template.TextColorBg = bg & 0xF;
        menu.Template.TextColorShadow = shadow & 0xF;
    }

    // OFFSET 0x02001408
    public static uint ListMenu_TestInput(ListMenu menu, ListMenuTemplate? template, ushort listPos, ushort cursorPos, ushort updateCursor, ushort input, out ushort outListPos, out ushort outCursorPos)
    {
        if (template != null)
        {
            menu.Template.CopyFrom(template);
        }
        menu.ListPos = listPos;
        menu.CursorPos = cursorPos;
        menu.Unused30 = 0;
        menu.Unused31 = 0;
        if (input == KeyUp)
        {
            UpdateSelectedRow(menu, (byte)updateCursor, 1, 0);
        }
        else if (input == KeyDown)
        {
            UpdateSelectedRow(menu, (byte)updateCursor, 1, 1);
        }
        outListPos = menu.ListPos;
        outCursorPos = menu.CursorPos;
        return NoChoice;
    }

    // OFFSET 0x0200147C
    public static void ListMenu_SetAltTextColors(ListMenu menu, byte fg, byte bg, byte shadow)
    {
        menu.AltTextColorFg = fg & 0xF;
        menu.AltTextColorBg = bg & 0xF;
        menu.AltTextColorShadow = shadow & 0xF;
        menu.UseAltText = true;
    }

    // OFFSET 0x020014D0
    public static void ListMenu_CalcTrueCursorPos(ListMenu menu, out ushort pos)
    {
        pos = (ushort)(menu.ListPos + menu.CursorPos);
    }

    // OFFSET 0x020014DC
    public static void ListMenu_GetListAndCursorPos(ListMenu menu, out ushort listPos, out ushort cursorPos)
    {
        listPos = menu.ListPos;
        cursorPos = menu.CursorPos;
    }

    // OFFSET 0x020014F0
    public static byte ListMenu_GetLastAction(ListMenu menu)
    {
        return menu.LastAction;
    }

    // OFFSET 0x020014F8
    public static uint ListMenu_GetIndexOfChoice(ListMenu menu, ushort choice)
    {
        return menu.Template.Choices[choice].Index;
    }

    // OFFSET 0x02001504
    public static object? ListMenu_GetAttribute(ListMenu menu, byte attribute)
    {
        ListMenuTemplate t = menu.Template;
        switch (attribute)
        {
            case 0: return t.CursorCallback;
            case 1: return t.PrintCallback;
            case 2: return (uint)t.Count;
            case 3: return (uint)t.MaxDisplay;
            case 5: return (uint)t.HeaderXOffset;
            case 6: return (uint)t.TextXOffset;
            case 7: return (uint)t.CursorXOffset;
            case 8: return (uint)t.YOffset;
            case 9: return (uint)(t.LineSpacing + Font_GetAttribute(t.FontID, 1));
            case 10: return (uint)t.TextColorFg;
            case 11: return (uint)t.TextColorBg;
            case 12: return (uint)t.TextColorShadow;
            case 13: return (uint)t.LetterSpacing;
            case 14: return (uint)t.LineSpacing;
            case 15: return (uint)t.PagerMode;
            case 16: return (uint)t.FontID;
            case 17: return (uint)t.CursorType;
            case 18: return t.Window;
            case 19: return t.Parent;
            default: return NoChoice;
        }
    }

    // OFFSET 0x020015CC
    public static void ListMenu_SetChoices(ListMenu menu, StringListEntry[] choices)
    {
        menu.Template.Choices = choices;
    }

    // OFFSET 0x020015D0
    private static void PrintEntry(ListMenu menu, GameString? text, byte xOffset, byte yOffset)
    {
        if (text == null)
        {
            return;
        }
        ListMenuTemplate t = menu.Template;
        if (menu.UseAltText)
        {
            uint color = (uint)((menu.AltTextColorFg << 16) | (menu.AltTextColorShadow << 8) | menu.AltTextColorBg);
            Text.Text_AddPrinterWithParamsColorAndSpacing(t.Window, menu.AltFontID, text, xOffset, yOffset, 0xFF, color, menu.AltLetterSpacing, 0, null);
        }
        else
        {
            uint color = (uint)((t.TextColorFg << 16) | (t.TextColorShadow << 8) | t.TextColorBg);
            Text.Text_AddPrinterWithParamsColorAndSpacing(t.Window, t.FontID, text, xOffset, yOffset, 0xFF, color, t.LetterSpacing, 0, null);
        }
    }

    // OFFSET 0x02001688
    private static void PrintEntries(ListMenu menu, ushort startIndex, ushort lineOffset, int lineCount)
    {
        ListMenuTemplate t = menu.Template;
        byte lineHeight = (byte)(t.LineSpacing + Font_GetAttribute(t.FontID, 1));
        for (int i = 0; i < lineCount; i++)
        {
            StringListEntry entry = t.Choices[startIndex];
            byte x = entry.Index != Header ? t.TextXOffset : t.HeaderXOffset;
            byte y = (byte)(t.YOffset + lineHeight * lineOffset);
            t.PrintCallback?.Invoke(menu, entry.Index, y);
            PrintEntry(menu, t.Choices[startIndex].Text, x, y);
            startIndex++;
            lineOffset++;
        }
    }

    // OFFSET 0x02001720
    private static void PrintCursor(ListMenu menu)
    {
        ListMenuTemplate t = menu.Template;
        byte lineHeight = (byte)(t.LineSpacing + Font_GetAttribute(t.FontID, 1));
        byte x = t.CursorXOffset;
        byte y = (byte)(t.YOffset + menu.CursorPos * lineHeight);
        switch (t.CursorType)
        {
            case 0:
                ColoredArrow.ColoredArrow_Print(menu.Cursor, t.Window, (uint)x, (uint)y);
                break;
        }
    }

    // OFFSET 0x02001778
    private static void EraseCursor(ListMenu menu, ushort atLine)
    {
        ListMenuTemplate t = menu.Template;
        switch (t.CursorType)
        {
            case 0:
                byte lineHeight = (byte)(t.LineSpacing + Font_GetAttribute(t.FontID, 1));
                Bg.Window_FillRectWithColor(t.Window, t.TextColorBg, t.CursorXOffset, (ushort)(t.YOffset + atLine * lineHeight), 8, 16);
                break;
        }
    }

    // OFFSET 0x020017E0
    private static byte UpdateOffsetsForScroll(ListMenu menu, byte movingDown)
    {
        ListMenuTemplate t = menu.Template;
        ushort cursorPos = menu.CursorPos;
        ushort listPos = menu.ListPos;
        ushort maxDisplay = t.MaxDisplay;
        if (movingDown == 0)
        {
            ushort scrollLine = maxDisplay == 1 ? (ushort)0 : (ushort)(maxDisplay - (maxDisplay / 2 + maxDisplay % 2) - 1);
            if (listPos == 0)
            {
                while (cursorPos != 0)
                {
                    cursorPos--;
                    if (t.Choices[listPos + cursorPos].Index != Header)
                    {
                        menu.CursorPos = cursorPos;
                        return 1;
                    }
                }
                return 0;
            }
            while (cursorPos > scrollLine)
            {
                cursorPos--;
                if (t.Choices[listPos + cursorPos].Index != Header)
                {
                    menu.CursorPos = cursorPos;
                    return 1;
                }
            }
            menu.CursorPos = scrollLine;
            menu.ListPos = (ushort)(listPos - 1);
        }
        else
        {
            ushort scrollLine = maxDisplay == 1 ? (ushort)0 : (ushort)(maxDisplay / 2 + maxDisplay % 2);
            if (listPos == t.Count - maxDisplay)
            {
                while (cursorPos < maxDisplay - 1)
                {
                    cursorPos++;
                    if (t.Choices[listPos + cursorPos].Index != Header)
                    {
                        menu.CursorPos = cursorPos;
                        return 1;
                    }
                }
                return 0;
            }
            while (cursorPos < scrollLine)
            {
                cursorPos++;
                if (t.Choices[listPos + cursorPos].Index != Header)
                {
                    menu.CursorPos = cursorPos;
                    return 1;
                }
            }
            menu.CursorPos = scrollLine;
            menu.ListPos = (ushort)(listPos + 1);
        }
        return 2;
    }

    // OFFSET 0x02001900
    private static void ScrollList(ListMenu menu, byte lineCount, byte movingDown)
    {
        ListMenuTemplate t = menu.Template;
        if (lineCount >= t.MaxDisplay)
        {
            Bg.Window_FillTilemap(t.Window, t.TextColorBg);
            PrintEntries(menu, menu.ListPos, 0, t.MaxDisplay);
            return;
        }
        byte lineHeight = (byte)(t.LineSpacing + Font_GetAttribute(t.FontID, 1));
        int bg = t.TextColorBg;
        byte fill = (byte)((bg << 4) | bg);
        if (movingDown == 0)
        {
            Bg.Window_Scroll(t.Window, 1, (byte)(lineCount * lineHeight), fill);
            PrintEntries(menu, menu.ListPos, 0, lineCount);
            ushort y = (ushort)(t.YOffset + t.MaxDisplay * lineHeight);
            int width = Bg.Window_GetWidth(t.Window);
            int height = Bg.Window_GetHeight(t.Window);
            Bg.Window_FillRectWithColor(t.Window, bg, 0, y, (ushort)(width * 8), (ushort)(height * 8 - y));
        }
        else
        {
            Bg.Window_Scroll(t.Window, 0, (byte)(lineCount * lineHeight), fill);
            ushort offset = (ushort)(t.MaxDisplay - lineCount);
            PrintEntries(menu, (ushort)(menu.ListPos + offset), offset, lineCount);
            int width = Bg.Window_GetWidth(t.Window);
            Bg.Window_FillRectWithColor(t.Window, bg, 0, 0, (ushort)(width * 8), t.YOffset);
        }
    }

    // OFFSET 0x02001A18
    private static byte UpdateSelectedRow(ListMenu menu, byte updateCursor, byte scrollCount, byte movingDown)
    {
        ushort oldCursorPos = menu.CursorPos;
        byte scrolled = 0;
        int result = 0;
        byte i = 0;
        while (i < scrollCount)
        {
            byte step = UpdateOffsetsForScroll(menu, movingDown);
            result |= step;
            if (step == 2)
            {
                scrolled++;
                if (menu.Template.Choices[menu.ListPos + menu.CursorPos].Index == Header)
                {
                    continue;
                }
            }
            i++;
        }
        if (updateCursor != 0)
        {
            switch (result)
            {
                case 1:
                    EraseCursor(menu, oldCursorPos);
                    PrintCursor(menu);
                    InvokeCursorCallback(menu, 0);
                    Bg.Window_CopyToVRAM(menu.Template.Window);
                    break;
                case 2:
                case 3:
                    EraseCursor(menu, oldCursorPos);
                    ScrollList(menu, scrolled, movingDown);
                    PrintCursor(menu);
                    InvokeCursorCallback(menu, 0);
                    Bg.Window_CopyToVRAM(menu.Template.Window);
                    break;
                default:
                    return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x02001AD8
    private static void InvokeCursorCallback(ListMenu menu, byte onInit)
    {
        ListMenuTemplate t = menu.Template;
        t.CursorCallback?.Invoke(menu, t.Choices[menu.ListPos + menu.CursorPos].Index, onInit);
    }
}
