using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate bool TextPrinterCallback(TextPrinter printer, ushort argument);

public static class Text
{
    public const int MaxPrinters = 8;
    public const byte NoPrinter = 0xFF;
    public const int InstantDelay = 0;
    public const int NoTransferDelay = 0xFF;

    private const int FontAttributeSize = 8;
    private const int AttributeLetterSpacing = 2;
    private const int AttributeLineSpacing = 3;
    private const int AttributeUnused = 4;
    private const int AttributeFgColor = 5;
    private const int AttributeBgColor = 6;
    private const int AttributeShadowColor = 7;
    private const int MaxInstantSteps = 0x400;
    private const int SpeedMask = 0x7F;
    private const int IconArchive = 0xE;
    private const int IconMember = 5;
    private const int IconGfxSize = 0x600;
    private const int IconFrameSize = 0x180;
    private const int IconWidth = 0x18;
    private const int IconHeight = 0x20;

    private static readonly SysTask?[] sPrinterTasks = new SysTask?[MaxPrinters];
    private static byte sPrintingPaused;

    private static TextPrinter CreateTemplate(object window, int fontID, object? text, int x, int y)
    {
        return new TextPrinter
        {
            SourceString = (GameString?)text,
            Window = (Window)window,
            FontID = (byte)fontID,
            X = (byte)x,
            Y = (byte)y,
            CurrX = (ushort)x,
            CurrY = (ushort)y,
            GlyphDrawMode = 0,
            Unused1A = 0,
            ColorCache = 0xFF,
        };
    }

    // OFFSET 0x0201D67C
    private static byte Text_CreatePrinterTask(SysTaskFunc taskFunc, TextPrinter printer, uint priority)
    {
        int i;
        for (i = 0; i < MaxPrinters; i++)
        {
            if (sPrinterTasks[i] == null)
            {
                sPrinterTasks[i] = SysTask.SysTask_CreateOnPrintQueue(taskFunc, printer, priority);
                if (sPrinterTasks[i] == null)
                {
                    i = MaxPrinters;
                }
                break;
            }
        }
        return (byte)i;
    }

    // OFFSET 0x0201D6B0
    private static void Text_DestroyPrinterTask(byte printerID)
    {
        if (printerID >= MaxPrinters)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (sPrinterTasks[printerID] == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (printerID < MaxPrinters && sPrinterTasks[printerID] != null)
        {
            TextPrinter? printer = (TextPrinter?)SysTaskManager.SysTask_GetParam(sPrinterTasks[printerID]!);
            if (printer != null)
            {
                TextPrinter_FreeIconGfx(printer);
            }
            SysTask.SysTask_Done(sPrinterTasks[printerID]!);
            sPrinterTasks[printerID] = null;
        }
    }

    // OFFSET 0x0201D6F8
    private static bool Text_IsPrinterTaskActive(byte printerID)
    {
        return sPrinterTasks[printerID] != null;
    }

    // OFFSET 0x0201D710
    public static void Text_ResetAllPrinters()
    {
        for (int i = 0; i < MaxPrinters; i++)
        {
            sPrinterTasks[i] = null;
        }
    }

    // OFFSET 0x0201D724
    public static byte Text_IsPrinterActive(byte printerID)
    {
        return (byte)(Text_IsPrinterTaskActive(printerID) ? 1 : 0);
    }

    // OFFSET 0x0201D730
    public static void Text_RemovePrinter(byte printerID)
    {
        Text_DestroyPrinterTask(printerID);
    }

    // OFFSET 0x0201D738
    public static int Text_AddPrinterWithParams(object window, int fontID, object? text, int x, int y, int renderDelay, TextPrinterCallback? callback)
    {
        byte[] attributes = TextGlyphDecoder.FontAttributes!;
        int font = fontID * FontAttributeSize;
        TextPrinter template = CreateTemplate(window, fontID, text, x, y);
        template.LetterSpacing = attributes[font + AttributeLetterSpacing];
        template.LineSpacing = attributes[font + AttributeLineSpacing];
        template.Unused14 = attributes[font + AttributeUnused];
        template.FgColor = attributes[font + AttributeFgColor];
        template.BgColor = attributes[font + AttributeBgColor];
        template.ShadowColor = attributes[font + AttributeShadowColor];
        return Text_AddPrinter(template, renderDelay, callback);
    }

    // OFFSET 0x0201D78C
    public static int Text_AddPrinterWithParamsAndColor(object window, int fontID, object? text, uint x, uint y, uint renderDelay, uint color, TextPrinterCallback? callback)
    {
        byte[] attributes = TextGlyphDecoder.FontAttributes!;
        int font = fontID * FontAttributeSize;
        TextPrinter template = CreateTemplate(window, fontID, text, (int)x, (int)y);
        template.LetterSpacing = attributes[font + AttributeLetterSpacing];
        template.LineSpacing = attributes[font + AttributeLineSpacing];
        template.Unused14 = attributes[font + AttributeUnused];
        template.FgColor = (byte)(color >> 16);
        template.ShadowColor = (byte)(color >> 8);
        template.BgColor = (byte)color;
        return Text_AddPrinter(template, (int)renderDelay, callback);
    }

    // OFFSET 0x0201D7E0
    public static int Text_AddPrinterWithParamsColorAndSpacing(object window, int fontID, object? text, int x, int y, int renderDelay, uint color, int letterSpacing, int lineSpacing, TextPrinterCallback? callback)
    {
        byte[] attributes = TextGlyphDecoder.FontAttributes!;
        TextPrinter template = CreateTemplate(window, fontID, text, x, y);
        template.LetterSpacing = (ushort)letterSpacing;
        template.LineSpacing = (ushort)lineSpacing;
        template.Unused14 = attributes[fontID * FontAttributeSize + AttributeUnused];
        template.FgColor = (byte)(color >> 16);
        template.ShadowColor = (byte)(color >> 8);
        template.BgColor = (byte)color;
        return Text_AddPrinter(template, renderDelay, callback);
    }

    // OFFSET 0x0201D834
    public static int Text_AddPrinter(TextPrinter template, int renderDelay, TextPrinterCallback? callback)
    {
        if (TextGlyphDecoder.FontAttributes == null)
        {
            return NoPrinter;
        }
        TextPrinter printer = template;
        printer.Active = 1;
        printer.State = 0;
        printer.TextSpeed = renderDelay & SpeedMask;
        printer.DelayCounter = 0;
        printer.ScrollDistance = 0;
        printer.CallbackResult = 0;
        printer.SpeedUp = false;
        printer.ScrollArrowDelay = 0;
        printer.ScrollArrowFrame = 0;
        printer.AutoScrollDelay = 0;
        printer.GlyphFontSet = false;
        printer.Text = GameString.String_GetData(printer.SourceString);
        printer.TextPos = 0;
        printer.Callback = callback;
        sPrintingPaused = 0;
        TextPrinter_InitIconGfx(printer);
        if (renderDelay != NoTransferDelay && renderDelay != InstantDelay)
        {
            printer.TextSpeed = (printer.TextSpeed + 0xFF) & SpeedMask;
            printer.CopyToVRAM = true;
            printer.ID = Text_CreatePrinterTask(Text_PrinterTask, printer, 1);
            return printer.ID;
        }
        printer.TextSpeed = 0;
        printer.CopyToVRAM = false;
        TextGlyphDecoder.Text_GenerateHalfRowLookupTable(template.FgColor, template.BgColor, template.ShadowColor);
        for (uint i = 0; i < MaxInstantSteps; i++)
        {
            if (TextPrinter_Render(printer) == RenderText.RenderFinish)
            {
                break;
            }
        }
        if (renderDelay != NoTransferDelay)
        {
            Bg.Window_CopyToVRAM(printer.Window);
        }
        TextPrinter_FreeIconGfx(printer);
        return MaxPrinters;
    }

    // OFFSET 0x0201D97C
    private static void Text_PrinterTask(SysTask task, object? param)
    {
        TextPrinter printer = (TextPrinter)param!;
        if (sPrintingPaused != 0)
        {
            return;
        }
        if (printer.CallbackResult == 0)
        {
            printer.CallbackArgument = 0;
            TextGlyphDecoder.Text_GenerateHalfRowLookupTable(printer.FgColor, printer.BgColor, printer.ShadowColor);
            switch (TextPrinter_Render(printer))
            {
                case RenderText.RenderPrint:
                    Bg.Window_CopyToVRAM(printer.Window);
                    goto case RenderText.RenderPause;
                case RenderText.RenderPause:
                    if (printer.Callback != null)
                    {
                        printer.CallbackResult = (byte)(printer.Callback(printer, printer.CallbackArgument) ? 1 : 0);
                    }
                    break;
                case RenderText.RenderFinish:
                    Text_DestroyPrinterTask(printer.ID);
                    break;
            }
            return;
        }
        printer.CallbackResult = (byte)(printer.Callback!(printer, printer.CallbackArgument) ? 1 : 0);
    }

    // OFFSET 0x0201D9E8
    private static int TextPrinter_Render(TextPrinter printer)
    {
        int result;
        do
        {
            result = Font.Font_RenderText(printer.FontID, printer);
        } while (result == RenderText.RenderUpdate);
        return result;
    }

    // OFFSET 0x0201DB48
    private static void TextPrinter_InitIconGfx(TextPrinter printer)
    {
        printer.IconGfx = null;
    }

    // OFFSET 0x0201DB50
    private static byte[] Text_LoadIconGfx()
    {
        byte[] iconGfx = new byte[IconGfxSize];
        Graphics.Graphics_GetCharData(IconArchive, IconMember, 0, out CharacterData? charData, 0);
        Array.Copy(charData!.RawData, iconGfx, IconGfxSize);
        return iconGfx;
    }

    // OFFSET 0x0201DB8C
    public static void TextPrinter_RenderIcon(TextPrinter printer, int unusedX, int unusedY, int icon)
    {
        Window window = printer.Window;
        if (printer.IconGfx == null)
        {
            printer.IconGfx = Text_LoadIconGfx();
        }
        byte[] frame = new byte[IconFrameSize];
        Array.Copy(printer.IconGfx, icon * IconFrameSize, frame, 0, IconFrameSize);
        ushort destX = (ushort)((Bg.Window_GetWidth(window) - 3) * 8);
        Bg.Window_BlitBitmapRect(window, frame, 0, 0, IconWidth, IconHeight, destX, 0, IconWidth, IconHeight);
    }

    // OFFSET 0x0201DBD8
    private static void TextPrinter_FreeIconGfx(TextPrinter printer)
    {
        if (printer.IconGfx != null)
        {
            printer.IconGfx = null;
        }
    }
}
