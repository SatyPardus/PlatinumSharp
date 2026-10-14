namespace PlatinumSharp.Game;

public static class BattleSubMenuData
{
    public const int Overlay = 13;
    private const int BgTemplateSize = 0x1C;
    private const int GraphicsModesSize = 0x10;
    private const int MaxTouchRects = 0x20;

    public static TouchScreenHitTable[] TouchRects(uint address)
    {
        byte[] raw = RomData.Overlay(Overlay, address, MaxTouchRects * 4);
        int count = 0;
        while (raw[count * 4] != TouchScreen.TableTerminator)
        {
            count++;
        }
        TouchScreenHitTable[] rects = new TouchScreenHitTable[count + 1];
        for (int i = 0; i <= count; i++)
        {
            rects[i].Top = raw[i * 4];
            rects[i].Bottom = raw[i * 4 + 1];
            rects[i].Left = raw[i * 4 + 2];
            rects[i].Right = raw[i * 4 + 3];
        }
        return rects;
    }

    public static GridMenuCursorPosition[] CursorPositions(uint address, int count)
    {
        byte[] raw = RomData.Overlay(Overlay, address, count * 8);
        GridMenuCursorPosition[] positions = new GridMenuCursorPosition[count];
        for (int i = 0; i < count; i++)
        {
            positions[i].XCoord1 = raw[i * 8];
            positions[i].YCoord1 = raw[i * 8 + 1];
            positions[i].XCoord2 = raw[i * 8 + 2];
            positions[i].YCoord2 = raw[i * 8 + 3];
            positions[i].UpIndex = raw[i * 8 + 4];
            positions[i].DownIndex = raw[i * 8 + 5];
            positions[i].LeftIndex = raw[i * 8 + 6];
            positions[i].RightIndex = raw[i * 8 + 7];
        }
        return positions;
    }

    public static BgTemplate BgTemplate(uint address)
    {
        return RomStructs.BgTemplate(RomData.Overlay(Overlay, address, BgTemplateSize), 0);
    }

    public static GraphicsModes GraphicsModes(uint address)
    {
        return RomStructs.GraphicsModes(RomData.Overlay(Overlay, address, GraphicsModesSize), 0);
    }

    public static byte[] Bytes(uint address, int count)
    {
        return RomData.Overlay(Overlay, address, count);
    }

    public static uint U32(uint address)
    {
        return Nitro.Bin.U32(RomData.Overlay(Overlay, address, 4), 0);
    }

    public static ushort[] PaletteWords(ushort[] buffer, int start, int count)
    {
        ushort[] words = new ushort[count];
        Array.Copy(buffer, start, words, 0, count);
        return words;
    }
}
