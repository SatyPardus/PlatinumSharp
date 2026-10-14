using static PlatinumSharp.Game.Globals;
using static PlatinumSharp.Game.Font;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class MenuTemplate
{
    public StringListEntry[] Choices = Array.Empty<StringListEntry>();
    public Window Window = null!;
    public byte FontID;
    public byte XSize;
    public byte YSize;
    public int LineSpacing;
    public int SuppressCursor;
    public int LoopAround;

    public void CopyFrom(MenuTemplate other)
    {
        Choices = other.Choices;
        Window = other.Window;
        FontID = other.FontID;
        XSize = other.XSize;
        YSize = other.YSize;
        LineSpacing = other.LineSpacing;
        SuppressCursor = other.SuppressCursor;
        LoopAround = other.LoopAround;
    }
}

public sealed class Menu
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
    private const int ConfirmSound = 0x5DC;

    public readonly MenuTemplate Template = new();
    public ColoredArrow Cursor = null!;
    public uint CancelKeys;
    public byte CursorPos;
    public byte Width;
    public byte XOffset;
    public byte YOffset;
    public byte GlyphWidth;
    public byte GlyphHeight;
    public byte LastAction;
    public byte HeapID;

    // OFFSET 0x02001AF4
    public static Menu Menu_New(MenuTemplate template, byte xOffset, byte yOffset, byte cursorStart, byte heapID, uint cancelKeys)
    {
        Menu menu = new();
        menu.Template.CopyFrom(template);
        menu.Cursor = ColoredArrow.ColoredArrow_New(heapID);
        menu.CancelKeys = cancelKeys;
        menu.CursorPos = cursorStart;
        menu.Width = CalcMaxEntryWidth(menu);
        menu.HeapID = heapID;
        menu.XOffset = xOffset;
        menu.YOffset = yOffset;
        menu.GlyphWidth = (byte)(Font_GetAttribute(template.FontID, 0) + Font_GetAttribute(template.FontID, 2));
        menu.GlyphHeight = (byte)(Font_GetAttribute(template.FontID, 1) + Font_GetAttribute(template.FontID, 3));
        DrawWholeMenu(menu);
        DrawCursor(menu);
        return menu;
    }

    // OFFSET 0x02001B7C
    public static Menu Menu_NewAndCopyToVRAM(MenuTemplate template, byte xOffset, byte yOffset, byte cursorStart, byte heapID, uint cancelKeys)
    {
        Menu menu = Menu_New(template, xOffset, yOffset, cursorStart, heapID, cancelKeys);
        Bg.Window_CopyToVRAM(menu.Template.Window);
        return menu;
    }

    // OFFSET 0x02001B9C
    public static Menu Menu_NewSimple(MenuTemplate template, byte cursorStart, byte heapID)
    {
        byte xOffset = (byte)Font_GetAttribute(template.FontID, 0);
        return Menu_NewAndCopyToVRAM(template, xOffset, 0, cursorStart, heapID, KeyB);
    }

    // OFFSET 0x02001BC4
    public static void Menu_Free(Menu menu, out byte cursorPos)
    {
        cursorPos = menu.CursorPos;
        ColoredArrow.ColoredArrow_Free(menu.Cursor);
    }

    // OFFSET 0x02001BE0
    public static uint Menu_ProcessInput(Menu menu)
    {
        menu.LastAction = 0;
        int pressed = gSystem.PressedKeys;
        if ((pressed & KeyA) != 0)
        {
            SoundPlayback.Sound_PlayEffect(ConfirmSound);
            return menu.Template.Choices[menu.CursorPos].Index;
        }
        if ((menu.CancelKeys & (uint)pressed) != 0)
        {
            SoundPlayback.Sound_PlayEffect(ConfirmSound);
            return Cancel;
        }
        if ((pressed & KeyUp) != 0)
        {
            if (TryMovingCursorAndPlaySound(menu, 0, ConfirmSound))
            {
                menu.LastAction = 1;
            }
            return NoChoice;
        }
        if ((pressed & KeyDown) != 0)
        {
            if (TryMovingCursorAndPlaySound(menu, 1, ConfirmSound))
            {
                menu.LastAction = 2;
            }
            return NoChoice;
        }
        if ((pressed & KeyLeft) != 0)
        {
            if (TryMovingCursorAndPlaySound(menu, 2, ConfirmSound))
            {
                menu.LastAction = 3;
            }
            return NoChoice;
        }
        if ((pressed & KeyRight) != 0)
        {
            if (TryMovingCursorAndPlaySound(menu, 3, ConfirmSound))
            {
                menu.LastAction = 4;
            }
            return NoChoice;
        }
        return NoChoice;
    }

    // OFFSET 0x02001C94
    public static uint Menu_ProcessInputWithSound(Menu menu, ushort sdatID)
    {
        menu.LastAction = 0;
        int pressed = gSystem.PressedKeys;
        if ((pressed & KeyA) != 0)
        {
            SoundPlayback.Sound_PlayEffect(ConfirmSound);
            return menu.Template.Choices[menu.CursorPos].Index;
        }
        if ((menu.CancelKeys & (uint)pressed) != 0)
        {
            SoundPlayback.Sound_PlayEffect(ConfirmSound);
            return Cancel;
        }
        if ((pressed & KeyUp) != 0)
        {
            if (TryMovingCursorAndPlaySound(menu, 0, sdatID))
            {
                menu.LastAction = 1;
            }
            return NoChoice;
        }
        if ((pressed & KeyDown) != 0)
        {
            if (TryMovingCursorAndPlaySound(menu, 1, sdatID))
            {
                menu.LastAction = 2;
            }
            return NoChoice;
        }
        if ((pressed & KeyLeft) != 0)
        {
            if (TryMovingCursorAndPlaySound(menu, 2, sdatID))
            {
                menu.LastAction = 3;
            }
            return NoChoice;
        }
        if ((pressed & KeyRight) != 0)
        {
            if (TryMovingCursorAndPlaySound(menu, 3, sdatID))
            {
                menu.LastAction = 4;
            }
            return NoChoice;
        }
        return NoChoice;
    }

    // OFFSET 0x02001D44
    public static uint Menu_ProcessExternalInput(Menu menu, byte input)
    {
        switch (input)
        {
            case 0:
                SoundPlayback.Sound_PlayEffect(ConfirmSound);
                return menu.Template.Choices[menu.CursorPos].Index;
            case 1:
                SoundPlayback.Sound_PlayEffect(ConfirmSound);
                return Cancel;
            case 2:
                TryMovingCursorAndPlaySound(menu, 0, ConfirmSound);
                return NoChoice;
            case 3:
                TryMovingCursorAndPlaySound(menu, 1, ConfirmSound);
                return NoChoice;
            case 4:
                TryMovingCursorAndPlaySound(menu, 2, ConfirmSound);
                return NoChoice;
            case 5:
                TryMovingCursorAndPlaySound(menu, 3, ConfirmSound);
                return NoChoice;
            default:
                return NoChoice;
        }
    }

    // OFFSET 0x02001DC4
    public static byte Menu_GetCursorPos(Menu menu)
    {
        return menu.CursorPos;
    }

    // OFFSET 0x02001DC8
    public static byte Menu_GetLastAction(Menu menu)
    {
        return menu.LastAction;
    }

    // OFFSET 0x02001DCC
    private static bool TryMovingCursorAndPlaySound(Menu menu, byte direction, int sound)
    {
        byte oldPos = menu.CursorPos;
        if (TryMovingCursor(menu, direction) == 0)
        {
            return false;
        }
        int fill = Font_GetAttribute(menu.Template.FontID, 6);
        CalcCursorDrawCoords(menu, out byte x, out byte y, oldPos);
        Bg.Window_FillRectWithColor(menu.Template.Window, fill, x, y, 8, menu.GlyphHeight);
        DrawCursor(menu);
        SoundPlayback.Sound_PlayEffect(sound);
        return true;
    }

    // OFFSET 0x02001E24
    private static byte TryMovingCursor(Menu menu, byte direction)
    {
        MenuTemplate t = menu.Template;
        int pos = menu.CursorPos;
        sbyte newPos;
        if (direction == 0)
        {
            if (t.YSize <= 1)
            {
                return 0;
            }
            if (pos % t.YSize == 0)
            {
                if (t.LoopAround == 0)
                {
                    return 0;
                }
                newPos = (sbyte)(pos + (t.YSize - 1));
            }
            else
            {
                newPos = (sbyte)(pos - 1);
            }
        }
        else if (direction == 1)
        {
            if (t.YSize <= 1)
            {
                return 0;
            }
            int last = t.YSize - 1;
            if (pos % t.YSize == last)
            {
                if (t.LoopAround == 0)
                {
                    return 0;
                }
                newPos = (sbyte)(pos - last);
            }
            else
            {
                newPos = (sbyte)(pos + 1);
            }
        }
        else if (direction == 2)
        {
            if (t.XSize <= 1)
            {
                return 0;
            }
            if (pos < t.YSize)
            {
                if (t.LoopAround == 0)
                {
                    return 0;
                }
                newPos = (sbyte)(pos + t.YSize * (t.XSize - 1));
            }
            else
            {
                newPos = (sbyte)(pos - t.YSize);
            }
        }
        else
        {
            if (t.XSize <= 1)
            {
                return 0;
            }
            if (pos >= t.YSize * (t.XSize - 1))
            {
                if (t.LoopAround == 0)
                {
                    return 0;
                }
                newPos = (sbyte)(pos % t.YSize);
            }
            else
            {
                newPos = (sbyte)(pos + t.YSize);
            }
        }
        if (t.Choices[newPos].Index == Header)
        {
            return 0;
        }
        menu.CursorPos = (byte)newPos;
        return 1;
    }

    // OFFSET 0x02001F1C
    private static byte CalcMaxEntryWidth(Menu menu)
    {
        MenuTemplate t = menu.Template;
        byte max = 0;
        for (byte i = 0; i < t.XSize * t.YSize; i++)
        {
            byte width = (byte)Font_CalcStringWidth(t.FontID, t.Choices[i].Text, 0);
            if (max < width)
            {
                max = width;
            }
        }
        return max;
    }

    // OFFSET 0x02001F5C
    private static void DrawWholeMenu(Menu menu)
    {
        MenuTemplate t = menu.Template;
        Bg.Window_FillTilemap(t.Window, Font_GetAttribute(t.FontID, 6));
        byte x = menu.XOffset;
        byte columnStep = (byte)(menu.Width + menu.GlyphWidth * 2);
        for (byte column = 0; column < t.XSize; column++)
        {
            for (byte row = 0; row < t.YSize; row++)
            {
                byte y = (byte)(menu.YOffset + row * (menu.GlyphHeight + t.LineSpacing));
                Text.Text_AddPrinterWithParams(t.Window, t.FontID, t.Choices[column * t.YSize + row].Text, x, y, 0xFF, null);
            }
            x += columnStep;
        }
    }

    // OFFSET 0x02001FE8
    private static void DrawCursor(Menu menu)
    {
        if (menu.Template.SuppressCursor == 1)
        {
            return;
        }
        CalcCursorDrawCoords(menu, out byte x, out byte y, menu.CursorPos);
        ColoredArrow.ColoredArrow_Print(menu.Cursor, menu.Template.Window, (uint)x, (uint)y);
    }

    // OFFSET 0x02002018
    private static void CalcCursorDrawCoords(Menu menu, out byte outX, out byte outY, byte cursorPos)
    {
        MenuTemplate t = menu.Template;
        outX = (byte)(cursorPos / t.YSize * (menu.Width + menu.GlyphWidth * 2));
        outY = (byte)(menu.YOffset + cursorPos % t.YSize * (menu.GlyphHeight + t.LineSpacing));
    }

    // OFFSET 0x02002054
    public static Menu Menu_MakeYesNoChoiceWithCursorAt(object bgConfig, object winTemplate, ushort borderTileStart, byte borderPalette, byte cursorStart, uint heapID)
    {
        MessageLoader messages = MessageLoader.MessageLoader_Init(1, 0x1A, 0x169, (int)heapID)!;
        StringListEntry[] choices = StringList.StringList_New(2, (int)heapID);
        StringList.StringList_AddFromMessageBank(choices, messages, 0x29, 0);
        StringList.StringList_AddFromMessageBank(choices, messages, 0x2A, Cancel);
        MessageLoader.MessageLoader_Free(messages);
        MenuTemplate template = new()
        {
            Choices = choices,
            Window = Bg.Window_New((int)heapID, 1)[0],
            FontID = 0,
            XSize = 1,
            YSize = 2,
            LineSpacing = 0,
            SuppressCursor = 0,
            LoopAround = 0,
        };
        Bg.Window_AddFromTemplate(bgConfig, template.Window, winTemplate);
        RenderWindow.Window_DrawStandardFrame(template.Window, 1, borderTileStart, borderPalette);
        return Menu_NewAndCopyToVRAM(template, 8, 0, cursorStart, (byte)heapID, KeyB);
    }

    // OFFSET 0x02002100
    public static Menu Menu_MakeYesNoChoice(object bgConfig, object winTemplate, ushort borderTileStart, byte borderPalette, uint heapID)
    {
        return Menu_MakeYesNoChoiceWithCursorAt(bgConfig, winTemplate, borderTileStart, borderPalette, 0, heapID);
    }

    // OFFSET 0x02002114
    public static uint Menu_ProcessInputAndHandleExit(Menu menu, uint heapID)
    {
        uint result = Menu_ProcessInput(menu);
        if (result != NoChoice)
        {
            Menu_DestroyForExit(menu, heapID);
        }
        return result;
    }

    // OFFSET 0x02002134
    public static uint Menu_ProcessExternalInputAndHandleExit(Menu menu, byte input, uint heapID)
    {
        uint result = Menu_ProcessExternalInput(menu, input);
        if (result != NoChoice)
        {
            Menu_DestroyForExit(menu, heapID);
        }
        return result;
    }

    // OFFSET 0x02002154
    public static void Menu_DestroyForExit(Menu menu, uint heapID)
    {
        RenderWindow.Window_EraseStandardFrame(menu.Template.Window, 0);
        Bg.Window_Remove(menu.Template.Window);
        StringList.StringList_Free(menu.Template.Choices);
        Menu_Free(menu, out _);
    }

    // OFFSET 0x02002180
    public static void Window_DrawMenuCursor(object window, uint x, uint y)
    {
        byte[] bitmap = RomData.Arm9(0x020E4C48, 0x40);
        Bg.Window_BlitBitmapRect(window, bitmap, 0, 0, 8, 16, (ushort)x, (ushort)y, 8, 16);
    }
}
