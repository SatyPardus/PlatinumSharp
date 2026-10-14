using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class ColoredArrow
{
    private const uint DefaultColor = 0x0001020F;
    private const uint StringSize = 4;
    private const uint NoDelay = 0xFF;

    public uint Color;
    public GameString? String;

    // OFFSET 0x020149F0
    public static ColoredArrow ColoredArrow_New(int heapID)
    {
        ColoredArrow arrow = new();
        arrow.Color = DefaultColor;
        arrow.String = GameString.String_Init(StringSize, heapID);
        GameString.String_CopyChars(arrow.String, RomData.Arm9U16(0x020E5494, 2));
        return arrow;
    }

    // OFFSET 0x02014A20
    public static void ColoredArrow_Free(ColoredArrow? arrow)
    {
        if (arrow == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (arrow != null)
        {
            if (arrow.String != null)
            {
                GameString.String_Free(arrow.String);
            }
        }
    }

    // OFFSET 0x02014A40
    public static void ColoredArrow_SetColor(ColoredArrow? arrow, uint color)
    {
        if (arrow == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (arrow != null)
        {
            arrow.Color = color;
        }
    }

    // OFFSET 0x02014A58
    public static void ColoredArrow_Print(ColoredArrow arrow, Window window, uint xOffset, uint yOffset)
    {
        Text.Text_AddPrinterWithParamsAndColor(window, 0, arrow.String!, xOffset, yOffset, NoDelay, arrow.Color, null);
        Bg.Window_LoadTiles(window);
    }
}
