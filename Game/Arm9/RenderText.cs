using static PlatinumSharp.Game.Globals;
using static PlatinumSharp.Game.Font;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class TextGlyph
{
    public byte[] Pixels = new byte[0x80];
    public byte Width;
    public byte Height;
}

public sealed class TextPrinter
{
    public ushort[] Text = Array.Empty<ushort>();
    public int TextPos;
    public Window Window = null!;
    public byte FontID;
    public byte X;
    public byte Y;
    public ushort CurrX;
    public ushort CurrY;
    public ushort LetterSpacing;
    public ushort LineSpacing;
    public byte FgColor;
    public byte BgColor;
    public byte ShadowColor;
    public ushort GlyphDrawMode;
    public byte Unused1A;
    public byte ColorCache;
    public int GlyphFontID;
    public bool GlyphFontSet;
    public bool SpeedUp;
    public int ScrollArrowDelay;
    public int ScrollArrowFrame;
    public byte AutoScrollDelay;
    public byte State;
    public int TextSpeed;
    public bool CopyToVRAM;
    public byte DelayCounter;
    public byte ScrollDistance;
    public ushort CallbackArgument;
    public GameString? SourceString;
    public byte Unused14;
    public TextPrinterCallback? Callback;
    public byte Active;
    public byte ID;
    public byte CallbackResult;
    public byte[]? IconGfx;
}

public static class RenderText
{
    public const int RenderPrint = 0;
    public const int RenderFinish = 1;
    public const int RenderUpdate = 2;
    public const int RenderPause = 3;

    private const int KeysAB = 0x3;
    private const int ConfirmSound = 0x5DC;
    private const int AutoScrollWait = 100;

    private static bool sCanABSpeedUpPrint;
    private static bool sAutoScroll;
    private static bool sSpeedUpOnTouch;
    private static bool sAutoScrollCanContinue;
    private static bool sSpeedUpBattle;
    private static bool sWaitBattle;
    private static ushort sScrollArrowBaseTile;
    private static byte[]? sScrollArrowFrames;

    // OFFSET 0x02002328
    public static int RenderTextStep(TextPrinter printer)
    {
        switch (printer.State)
        {
            case 0:
                return RenderNextCharacter(printer);
            case 1:
                if (TextPrinter_Wait(printer) != 0)
                {
                    TextPrinter_ClearScrollArrow(printer);
                    printer.State = 0;
                }
                return RenderPause;
            case 2:
                if (TextPrinter_WaitWithScrollArrow(printer))
                {
                    TextPrinter_ClearScrollArrow(printer);
                    Bg.Window_FillTilemap(printer.Window, printer.BgColor);
                    printer.CurrX = printer.X;
                    printer.CurrY = printer.Y;
                    printer.State = 0;
                }
                return RenderPause;
            case 3:
                if (TextPrinter_WaitWithScrollArrow(printer))
                {
                    TextPrinter_ClearScrollArrow(printer);
                    printer.ScrollDistance = (byte)(printer.LineSpacing + Font_GetAttribute(printer.FontID, 1));
                    printer.CurrX = printer.X;
                    printer.State = 4;
                }
                return RenderPause;
            case 4:
                if (printer.ScrollDistance != 0)
                {
                    byte fill = (byte)((printer.BgColor << 4) | printer.BgColor);
                    if (printer.ScrollDistance < 4)
                    {
                        Bg.Window_Scroll(printer.Window, 0, printer.ScrollDistance, fill);
                        printer.ScrollDistance = 0;
                    }
                    else
                    {
                        Bg.Window_Scroll(printer.Window, 0, 4, fill);
                        printer.ScrollDistance -= 4;
                    }
                    Bg.Window_CopyToVRAM(printer.Window);
                }
                else
                {
                    printer.State = 0;
                }
                return RenderPause;
            case 5:
                printer.State = 0;
                return RenderPause;
            case 6:
                if (printer.DelayCounter != 0)
                {
                    printer.DelayCounter--;
                }
                else
                {
                    printer.State = 0;
                }
                return RenderPause;
            default:
                return RenderFinish;
        }
    }

    private static int RenderNextCharacter(TextPrinter printer)
    {
        if (((gSystem.HeldKeys & KeysAB) != 0 && printer.SpeedUp) || (gSystem.TouchHeld != 0 && sSpeedUpOnTouch))
        {
            printer.DelayCounter = 0;
            if (printer.TextSpeed != 0)
            {
                sSpeedUpBattle = true;
            }
        }
        if (printer.DelayCounter != 0 && printer.TextSpeed != 0)
        {
            printer.DelayCounter--;
            if (sCanABSpeedUpPrint)
            {
                if ((gSystem.PressedKeys & KeysAB) != 0 || (gSystem.TouchPressed != 0 && sSpeedUpOnTouch))
                {
                    printer.SpeedUp = true;
                    printer.DelayCounter = 0;
                }
            }
            return RenderPause;
        }
        printer.DelayCounter = (byte)printer.TextSpeed;
        ushort character = printer.Text[printer.TextPos];
        printer.TextPos++;
        if (character == 0xF100)
        {
            ErrorHandling.GF_AssertFail();
        }
        switch (character)
        {
            case 0xFFFF:
                return RenderFinish;
            case 0xE000:
                printer.CurrX = printer.X;
                printer.CurrY = (ushort)(printer.CurrY + printer.LineSpacing + Font_GetAttribute(printer.FontID, 1));
                return RenderUpdate;
            case 0xF0FD:
                printer.TextPos++;
                return RenderUpdate;
            case 0x25BC:
                printer.State = 2;
                TextPrinter_InitScrollArrowAnim(printer);
                return RenderPause;
            case 0x25BD:
                printer.State = 3;
                TextPrinter_InitScrollArrowAnim(printer);
                return RenderPause;
            case 0xFFFE:
                return RenderControlCode(printer);
        }
        TextGlyph glyph = Font_TryLoadGlyph(printer.GlyphFontID, character);
        Bg.Window_CopyGlyph(printer.Window, glyph.Pixels, glyph.Width, glyph.Height, printer.CurrX, printer.CurrY, printer.GlyphDrawMode);
        printer.CurrX = (ushort)(printer.CurrX + glyph.Width + printer.LetterSpacing);
        return RenderPrint;
    }

    private static int RenderControlCode(TextPrinter printer)
    {
        printer.TextPos--;
        ushort code = (ushort)CharCodeFormat.CharCode_FormatArgType(printer.Text, printer.TextPos);
        switch (code)
        {
            case 0xFF00:
            {
                ushort color = (ushort)CharCodeFormat.CharCode_FormatArgParam(printer.Text, printer.TextPos, 0);
                if (color == 0xFF)
                {
                    byte previous = printer.ColorCache;
                    printer.ColorCache = (byte)((printer.FgColor - 1) / 2 + 100);
                    if (previous < 100 || previous >= 107)
                    {
                        break;
                    }
                    color = (ushort)(previous - 100);
                }
                else if (color >= 100)
                {
                    printer.ColorCache = (byte)color;
                    break;
                }
                printer.FgColor = (byte)(color * 2 + 1);
                printer.ShadowColor = (byte)(color * 2 + 2);
                TextGlyphDecoder.Text_GenerateHalfRowLookupTable(printer.FgColor, printer.BgColor, printer.ShadowColor);
                break;
            }
            case 0x200:
            {
                ushort argument = (ushort)CharCodeFormat.CharCode_FormatArgParam(printer.Text, printer.TextPos, 0);
                Text.TextPrinter_RenderIcon(printer, printer.CurrX, printer.CurrY, argument);
                if (printer.CopyToVRAM)
                {
                    Bg.Window_CopyToVRAM(printer.Window);
                }
                break;
            }
            case 0x201:
                printer.DelayCounter = (byte)CharCodeFormat.CharCode_FormatArgParam(printer.Text, printer.TextPos, 0);
                printer.TextPos = CharCodeFormat.CharCode_SkipFormatArg(printer.Text, printer.TextPos);
                printer.State = 6;
                return RenderPause;
            case 0x202:
                printer.CallbackArgument = (ushort)CharCodeFormat.CharCode_FormatArgParam(printer.Text, printer.TextPos, 0);
                printer.TextPos = CharCodeFormat.CharCode_SkipFormatArg(printer.Text, printer.TextPos);
                return RenderPause;
            case 0x203:
                printer.CurrX = (ushort)CharCodeFormat.CharCode_FormatArgParam(printer.Text, printer.TextPos, 0);
                break;
            case 0x204:
                printer.CurrY = (ushort)CharCodeFormat.CharCode_FormatArgParam(printer.Text, printer.TextPos, 0);
                break;
            case 0xFF01:
            {
                ushort argument = (ushort)CharCodeFormat.CharCode_FormatArgParam(printer.Text, printer.TextPos, 0);
                if (argument == 100)
                {
                    printer.GlyphDrawMode = 0;
                    printer.Unused1A = 0;
                }
                else if (argument == 200)
                {
                    printer.GlyphDrawMode = 0xFFFC;
                    printer.Unused1A = 0;
                }
                break;
            }
            case 0xFE06:
            {
                ushort argument = (ushort)CharCodeFormat.CharCode_FormatArgParam(printer.Text, printer.TextPos, 0);
                if (argument == 0xFE00)
                {
                    printer.State = 3;
                    TextPrinter_InitScrollArrowAnim(printer);
                    printer.TextPos = CharCodeFormat.CharCode_SkipFormatArg(printer.Text, printer.TextPos);
                    return RenderPause;
                }
                if (argument == 0xFE01)
                {
                    printer.State = 2;
                    TextPrinter_InitScrollArrowAnim(printer);
                    printer.TextPos = CharCodeFormat.CharCode_SkipFormatArg(printer.Text, printer.TextPos);
                    return RenderPause;
                }
                break;
            }
        }
        printer.TextPos = CharCodeFormat.CharCode_SkipFormatArg(printer.Text, printer.TextPos);
        return RenderUpdate;
    }

    // OFFSET 0x020027A8
    public static void TextPrinter_SetScrollArrowBaseTile(ushort tile)
    {
        sScrollArrowBaseTile = tile;
    }

    // OFFSET 0x020027B4
    public static void TextPrinter_InitScrollArrowAnim(TextPrinter printer)
    {
        if (sAutoScroll)
        {
            printer.AutoScrollDelay = 0;
        }
        else
        {
            printer.ScrollArrowFrame = 0;
            printer.ScrollArrowDelay = 0;
        }
    }

    // OFFSET 0x020027E0
    public static void TextPrinter_DrawScrollArrow(TextPrinter printer)
    {
        if (sAutoScroll)
        {
            return;
        }
        if (printer.ScrollArrowDelay != 0)
        {
            printer.ScrollArrowDelay = (printer.ScrollArrowDelay - 1) & 0x1F;
            return;
        }
        sScrollArrowFrames ??= RomData.Arm9(0x020E4CD0, 4);
        int bgLayer = Bg.Window_GetBgLayer(printer.Window);
        int x = Bg.Window_GetXPos(printer.Window);
        int y = Bg.Window_GetYPos(printer.Window);
        int right = x + Bg.Window_GetWidth(printer.Window);
        int tile = sScrollArrowBaseTile + sScrollArrowFrames[printer.ScrollArrowFrame] * 4;
        object bgConfig = printer.Window.BgConfig;
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 0x12), (byte)(right + 1), (byte)(y + 2), 1, 1, 0x10);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 0x13), (byte)(right + 2), (byte)(y + 2), 1, 1, 0x10);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 0x14), (byte)(right + 1), (byte)(y + 3), 1, 1, 0x10);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 0x15), (byte)(right + 2), (byte)(y + 3), 1, 1, 0x10);
        Bg.Bg_CopyTilemapBufferToVRAM(bgConfig, bgLayer);
        printer.ScrollArrowDelay = 8;
        printer.ScrollArrowFrame = (printer.ScrollArrowFrame + 1) & 3;
    }

    // OFFSET 0x02002968
    public static void TextPrinter_ClearScrollArrow(TextPrinter printer)
    {
        int bgLayer = Bg.Window_GetBgLayer(printer.Window);
        int x = Bg.Window_GetXPos(printer.Window);
        int y = Bg.Window_GetYPos(printer.Window);
        int right = x + Bg.Window_GetWidth(printer.Window);
        int tile = sScrollArrowBaseTile;
        object bgConfig = printer.Window.BgConfig;
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 0xA), (byte)(right + 1), (byte)(y + 2), 1, 2, 0x10);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 0xB), (byte)(right + 2), (byte)(y + 2), 1, 2, 0x10);
        Bg.Bg_CopyTilemapBufferToVRAM(bgConfig, bgLayer);
    }

    // OFFSET 0x020029FC
    private static bool TextPrinter_Continue(TextPrinter printer)
    {
        if ((gSystem.PressedKeys & KeysAB) != 0 || (gSystem.TouchPressed != 0 && sSpeedUpOnTouch))
        {
            SoundPlayback.Sound_PlayEffect(ConfirmSound);
            sWaitBattle = true;
            return true;
        }
        return false;
    }

    // OFFSET 0x02002A44
    public static bool TextPrinter_WaitAutoMode(TextPrinter printer)
    {
        if (printer.AutoScrollDelay == AutoScrollWait)
        {
            return true;
        }
        printer.AutoScrollDelay++;
        if (sAutoScrollCanContinue)
        {
            return TextPrinter_Continue(printer);
        }
        return false;
    }

    // OFFSET 0x02002A80
    public static bool TextPrinter_WaitWithScrollArrow(TextPrinter printer)
    {
        if (sAutoScroll)
        {
            return TextPrinter_WaitAutoMode(printer);
        }
        TextPrinter_DrawScrollArrow(printer);
        return TextPrinter_Continue(printer);
    }

    // OFFSET 0x02002AA4
    public static byte TextPrinter_Wait(TextPrinter printer)
    {
        if (sAutoScroll)
        {
            return (byte)(TextPrinter_WaitAutoMode(printer) ? 1 : 0);
        }
        return (byte)(TextPrinter_Continue(printer) ? 1 : 0);
    }

    // OFFSET 0x02002AC8
    public static void RenderControlFlags_SetCanABSpeedUpPrint(int value)
    {
        sCanABSpeedUpPrint = (value & 1) != 0;
    }

    // OFFSET 0x02002AE4
    public static void RenderControlFlags_SetAutoScrollFlags(int flags)
    {
        sAutoScroll = (flags & 1) != 0;
        sAutoScrollCanContinue = ((flags >> 1) & 1) != 0;
    }

    // OFFSET 0x02002B20
    public static void RenderControlFlags_SetSpeedUpOnTouch(int value)
    {
        sSpeedUpOnTouch = (value & 1) != 0;
    }

    // OFFSET 0x02002B3C
    public static byte RenderControlFlags_GetSpeedUpBattle()
    {
        return (byte)(sSpeedUpBattle ? 1 : 0);
    }

    // OFFSET 0x02002B4C
    public static void RenderControlFlags_ZeroSpeedUpBattle()
    {
        sSpeedUpBattle = false;
    }

    // OFFSET 0x02002B5C
    public static byte RenderControlFlags_GetWaitBattle()
    {
        return (byte)(sWaitBattle ? 1 : 0);
    }

    // OFFSET 0x02002B6C
    public static void RenderControlFlags_ZeroWaitBattle()
    {
        sWaitBattle = false;
    }
}
