using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class Bg
{
    public const int ScrollUp = 0;
    public const int ScrollDown = 1;
    public const int ScrollLeft = 2;
    public const int ScrollRight = 3;

    public const int RotationSet = 0;
    public const int RotationAdd = 1;
    public const int RotationSub = 2;
    public const int ScaleSetX = 3;
    public const int ScaleAddX = 4;
    public const int ScaleSubX = 5;
    public const int ScaleSetY = 6;
    public const int ScaleAddY = 7;
    public const int ScaleSubY = 8;
    public const int CenterSetX = 9;
    public const int CenterAddX = 10;
    public const int CenterSubX = 11;
    public const int CenterSetY = 12;
    public const int CenterAddY = 13;
    public const int CenterSubY = 14;

    private const int GlyphTileRows = 8;
    private const int GlyphTileColumns = 8;
    private const int GlyphTileBytes4bpp = 0x20;
    private const int GlyphTileBytes8bpp = 0x40;
    private const int TilePixels = 0x40;
    private const int TileIndexMask = 0x3FF;
    private const int FlipHorizontal = 1;
    private const int FlipVertical = 2;
    private const ushort EndOfColorList = 0xFFFF;

    private static int CountDoubledRows(int doubleMask)
    {
        int count = 0;
        for (int row = 0; row < GlyphTileRows; row++)
        {
            if (((doubleMask >> row) & 1) != 0)
            {
                count++;
            }
        }
        return count;
    }

    private static void DrawGlyphTile4bpp(byte[] glyph, int glyphOffset, byte[] pixels, int tilesWide, int x, int y, int columns, int rows, byte doubleMask)
    {
        for (int row = 0; row < rows; row++)
        {
            uint source = Bin.U32(glyph, glyphOffset + row * 4);
            bool doubled = ((doubleMask >> row) & 1) != 0;
            for (int column = 0; column < columns; column++)
            {
                byte pixel = (byte)((source >> (column * 4)) & 0xF);
                if (pixel != 0)
                {
                    int destX = x + column;
                    int shift = (destX & 1) << 2;
                    int offset = PixelOffset4bpp(destX, y, tilesWide);
                    byte value = (byte)((pixel << shift) | (pixels[offset] & (0xF0 >> shift)));
                    pixels[offset] = value;
                    if (doubled)
                    {
                        pixels[PixelOffset4bpp(destX, y + 1, tilesWide)] = value;
                    }
                }
            }
            y += doubled ? 2 : 1;
        }
    }

    private static void DrawGlyphTile8bpp(byte[] glyph, int glyphOffset, byte[] pixels, int tilesWide, int x, int y, int columns, int rows, byte doubleMask)
    {
        for (int row = 0; row < rows; row++)
        {
            bool doubled = ((doubleMask >> row) & 1) != 0;
            for (int column = 0; column < columns; column++)
            {
                byte pixel = glyph[glyphOffset + row * 8 + column];
                if (pixel != 0)
                {
                    int destX = x + column;
                    pixels[PixelOffset8bpp(destX, y, tilesWide)] = pixel;
                    if (doubled)
                    {
                        pixels[PixelOffset8bpp(destX, y + 1, tilesWide)] = pixel;
                    }
                }
            }
            y += doubled ? 2 : 1;
        }
    }

    // OFFSET 0x0201AED0
    public static void Window_CopyGlyph(object window, byte[] glyphPixels, int sourceWidth, int sourceHeight, int destX, int destY, int table)
    {
        Window target = Win(window);
        byte[] pixels = target.Pixels!;
        int windowWidth = (ushort)(target.Width << 3);
        int windowHeight = (ushort)(target.Height << 3);
        int drawWidth = windowWidth - destX;
        if (drawWidth >= sourceWidth)
        {
            drawWidth = sourceWidth;
        }
        int drawHeight = windowHeight - destY;
        if (drawHeight >= sourceHeight)
        {
            drawHeight = sourceHeight;
        }
        bool wide = drawWidth > GlyphTileColumns;
        bool tall = drawHeight > GlyphTileRows;
        int tilesWide = TilesWide(windowWidth);
        byte topMask = (byte)table;
        byte bottomMask = (byte)(table >> 8);
        int leftColumns = wide ? GlyphTileColumns : drawWidth;
        int topRows = tall ? GlyphTileRows : drawHeight;
        int bottomY = destY + GlyphTileRows + CountDoubledRows(table);
        if (target.ColorMode == 0)
        {
            DrawGlyphTile4bpp(glyphPixels, 0, pixels, tilesWide, destX, destY, leftColumns, topRows, topMask);
            if (wide)
            {
                DrawGlyphTile4bpp(glyphPixels, GlyphTileBytes4bpp, pixels, tilesWide, destX + GlyphTileColumns, destY, drawWidth - GlyphTileColumns, topRows, topMask);
            }
            if (tall)
            {
                DrawGlyphTile4bpp(glyphPixels, GlyphTileBytes4bpp * 2, pixels, tilesWide, destX, bottomY, leftColumns, drawHeight - GlyphTileRows, bottomMask);
                if (wide)
                {
                    DrawGlyphTile4bpp(glyphPixels, GlyphTileBytes4bpp * 3, pixels, tilesWide, destX + GlyphTileColumns, bottomY, drawWidth - GlyphTileColumns, drawHeight - GlyphTileRows, bottomMask);
                }
            }
            return;
        }
        uint convertSize = (uint)Math.Min(sourceWidth * 4 * sourceHeight * 8, glyphPixels.Length);
        byte[] converted = Bg_AllocPixels8bppFrom4bpp(glyphPixels, convertSize, target.Palette, Config(target.BgConfig).HeapID);
        DrawGlyphTile8bpp(converted, 0, pixels, tilesWide, destX, destY, leftColumns, topRows, topMask);
        if (wide)
        {
            DrawGlyphTile8bpp(converted, GlyphTileBytes8bpp, pixels, tilesWide, destX + GlyphTileColumns, destY, drawWidth - GlyphTileColumns, topRows, topMask);
        }
        if (tall)
        {
            DrawGlyphTile8bpp(converted, GlyphTileBytes8bpp * 2, pixels, tilesWide, destX, bottomY, leftColumns, drawHeight - GlyphTileRows, bottomMask);
            if (wide)
            {
                DrawGlyphTile8bpp(converted, GlyphTileBytes8bpp * 3, pixels, tilesWide, destX + GlyphTileColumns, bottomY, drawWidth - GlyphTileColumns, drawHeight - GlyphTileRows, bottomMask);
            }
        }
    }

    // OFFSET 0x0201C04C
    public static void Window_Scroll(object window, int direction, int distance, int fillValue)
    {
        Window target = Win(window);
        if (WindowBg(target).ColorMode == 0)
        {
            Window_Scroll4bpp(target, (byte)direction, (byte)distance, (byte)fillValue);
        }
        else
        {
            Window_Scroll8bpp(target, (byte)direction, (byte)distance, (byte)fillValue);
        }
    }

    private static uint Word(byte[] data, int offset)
    {
        return Bin.U32(data, offset);
    }

    private static void SetWord(byte[] data, int offset, uint value)
    {
        BitConverter.TryWriteBytes(data.AsSpan(offset), value);
    }

    // OFFSET 0x0201C06C
    private static void Window_Scroll4bpp(Window window, byte direction, byte distance, byte fillValue)
    {
        byte[] pixels = window.Pixels!;
        uint fill = (uint)((fillValue << 24) | (fillValue << 16) | (fillValue << 8) | fillValue);
        int width = window.Width;
        int size = window.Height * width * GlyphTileBytes4bpp;
        switch (direction)
        {
            case ScrollUp:
                for (int i = 0; i < size; i += GlyphTileBytes4bpp)
                {
                    int y = distance;
                    for (int j = 0; j < 8; j++)
                    {
                        int source = i + (((y & 7) | ((y & ~7) * width)) << 2);
                        if (source < size)
                        {
                            SetWord(pixels, i + (j << 2), Word(pixels, source));
                        }
                        else
                        {
                            SetWord(pixels, i + (j << 2), fill);
                        }
                        y++;
                    }
                }
                break;
            case ScrollDown:
            {
                int last = size - 4;
                for (int i = 0; i < size; i += GlyphTileBytes4bpp)
                {
                    int y = distance;
                    for (int j = 0; j < 8; j++)
                    {
                        int source = i + (((y & 7) | ((y & ~7) * width)) << 2);
                        if (source < size)
                        {
                            SetWord(pixels, last - (i + (j << 2)), Word(pixels, last - source));
                        }
                        else
                        {
                            SetWord(pixels, last - (i + (j << 2)), fill);
                        }
                        y++;
                    }
                }
                break;
            }
            case ScrollLeft:
            case ScrollRight:
                break;
        }
    }

    // OFFSET 0x0201C158
    private static void Window_Scroll8bpp(Window window, byte direction, byte distance, byte fillValue)
    {
        byte[] pixels = window.Pixels!;
        uint fill = (uint)((fillValue << 24) | (fillValue << 16) | (fillValue << 8) | fillValue);
        int width = window.Width;
        int size = window.Height * width * GlyphTileBytes8bpp;
        switch (direction)
        {
            case ScrollUp:
                for (int i = 0; i < size; i += GlyphTileBytes8bpp)
                {
                    int y = distance;
                    for (int j = 0; j < 8; j++)
                    {
                        int source = i + (((y & 7) | ((y & ~7) * width)) << 3);
                        if (source < size)
                        {
                            SetWord(pixels, i + (j << 3), Word(pixels, source));
                        }
                        else
                        {
                            SetWord(pixels, i + (j << 3), fill);
                        }
                        source += 4;
                        if (source < size + 4 && source + 4 <= pixels.Length)
                        {
                            SetWord(pixels, i + (j << 3) + 4, Word(pixels, source));
                        }
                        else
                        {
                            SetWord(pixels, i + (j << 3) + 4, fill);
                        }
                        y++;
                    }
                }
                break;
            case ScrollDown:
            {
                int last = size - 8;
                for (int i = 0; i < size; i += GlyphTileBytes8bpp)
                {
                    int y = distance;
                    for (int j = 0; j < 8; j++)
                    {
                        int source = i + (((y & 7) | ((y & ~7) * width)) << 3);
                        int destination = i + (j << 3);
                        if (source < size)
                        {
                            SetWord(pixels, last - destination, Word(pixels, last - source));
                        }
                        else
                        {
                            SetWord(pixels, last - destination, fill);
                        }
                        source -= 4;
                        if (source < size - 4)
                        {
                            SetWord(pixels, last - (destination - 4), Word(pixels, last - source));
                        }
                        else
                        {
                            SetWord(pixels, last - (destination - 4), fill);
                        }
                        y++;
                    }
                }
                break;
            }
            case ScrollLeft:
            case ScrollRight:
                break;
        }
    }

    // OFFSET 0x0201C28C
    public static object Window_GetBgConfig(object window)
    {
        return Win(window).BgConfig;
    }

    // OFFSET 0x0201C290
    public static int Window_GetBgLayer(object window)
    {
        return Win(window).BgLayer;
    }

    // OFFSET 0x0201C294
    public static int Window_GetWidth(object window)
    {
        return Win(window).Width;
    }

    // OFFSET 0x0201C298
    public static int Window_GetHeight(object window)
    {
        return Win(window).Height;
    }

    // OFFSET 0x0201C29C
    public static int Window_GetXPos(object window)
    {
        return Win(window).TilemapLeft;
    }

    // OFFSET 0x0201C2A0
    public static int Window_GetYPos(object window)
    {
        return Win(window).TilemapTop;
    }

    // OFFSET 0x0201C2A4
    public static ushort Window_GetBaseTile(object window)
    {
        return Win(window).BaseTile;
    }

    // OFFSET 0x0201C2AC
    public static void Window_SetXPos(object window, byte x)
    {
        Win(window).TilemapLeft = x;
    }

    // OFFSET 0x0201C2B0
    public static void Window_SetYPos(object window, byte y)
    {
        Win(window).TilemapTop = y;
    }

    // OFFSET 0x0201C2B4
    public static void Window_SetPalette(object window, byte palette)
    {
        Win(window).Palette = palette;
    }

    // OFFSET 0x0201C2B8
    public static void Bg_RunScheduledUpdates(object bgConfig)
    {
        BgConfig config = Config(bgConfig);
        Bg_RunScheduledScrolls(config);
        Bg_RunScheduledTilemapTransfers(config);
        config.ScrollScheduled = 0;
        config.BufferTransferScheduled = 0;
    }

    // OFFSET 0x0201C2D0
    private static void Bg_RunScheduledTilemapTransfers(BgConfig bgConfig)
    {
        for (int layer = 0; layer < LayerCount; layer++)
        {
            if ((bgConfig.BufferTransferScheduled & (1 << layer)) != 0)
            {
                Background bg = bgConfig.Bgs[layer];
                Bg_LoadScreenToVRAM(layer, bg.TilemapBuffer!, bg.BaseTile * 2, bg.BufferSize);
            }
        }
    }

    // OFFSET 0x0201C3C0
    public static void Bg_ScheduleTilemapTransfer(object bgConfig, int bgLayer)
    {
        Config(bgConfig).BufferTransferScheduled |= (ushort)(1 << bgLayer);
    }

    // OFFSET 0x0201C3D0
    private static void Bg_RunScheduledScrolls(BgConfig bgConfig)
    {
        for (int layer = 0; layer < LayerCount; layer++)
        {
            if ((bgConfig.ScrollScheduled & (1 << layer)) == 0)
            {
                continue;
            }
            Background bg = bgConfig.Bgs[layer];
            if (IsAffineCapable(layer) && bg.Type != TypeStatic)
            {
                MathUtil.CreateAffineTransformationMatrix(out MtxFx22 matrix, bg.Rotation, bg.XScale, bg.YScale, 2);
                Video.SetBgAffine(layer, matrix, bg.XCenter, bg.YCenter, bg.XOffset, bg.YOffset);
            }
            else
            {
                Video.SetBgOffset(layer, (uint)((bg.XOffset & 0x1FF) | ((bg.YOffset << 16) & 0x1FF0000)));
            }
        }
    }

    // OFFSET 0x0201C63C
    public static void Bg_ScheduleScroll(object bgConfig, byte bgLayer, byte bgOffsetUpdateOp, int value)
    {
        BgConfig config = Config(bgConfig);
        Bg_UpdateOffset(config.Bgs[bgLayer], bgOffsetUpdateOp, value);
        config.ScrollScheduled |= (ushort)(1 << bgLayer);
    }

    // OFFSET 0x0201C660
    public static void Bg_ScheduleAffineRotation(object bgConfig, byte bgLayer, byte bgAffineUpdateOp, ushort value)
    {
        BgConfig config = Config(bgConfig);
        Bg_UpdateRotation(config.Bgs[bgLayer], bgAffineUpdateOp, value);
        config.ScrollScheduled |= (ushort)(1 << bgLayer);
    }

    // OFFSET 0x0201C684
    private static void Bg_UpdateRotation(Background bg, byte bgAffineUpdateOp, ushort value)
    {
        switch (bgAffineUpdateOp)
        {
            case RotationSet:
                bg.Rotation = value;
                break;
            case RotationAdd:
                bg.Rotation += value;
                break;
            case RotationSub:
                bg.Rotation -= value;
                break;
        }
    }

    // OFFSET 0x0201C6A8
    public static void Bg_ScheduleAffineScale(object bgConfig, byte bgLayer, byte bgAffineUpdateOp, int value)
    {
        BgConfig config = Config(bgConfig);
        Bg_UpdateScale(config.Bgs[bgLayer], bgAffineUpdateOp, value);
        config.ScrollScheduled |= (ushort)(1 << bgLayer);
    }

    // OFFSET 0x0201C6CC
    private static void Bg_UpdateScale(Background bg, byte bgAffineUpdateOp, int value)
    {
        switch (bgAffineUpdateOp)
        {
            case ScaleSetX:
                bg.XScale = value;
                break;
            case ScaleAddX:
                bg.XScale += value;
                break;
            case ScaleSubX:
                bg.XScale -= value;
                break;
            case ScaleSetY:
                bg.YScale = value;
                break;
            case ScaleAddY:
                bg.YScale += value;
                break;
            case ScaleSubY:
                bg.YScale -= value;
                break;
        }
    }

    // OFFSET 0x0201C718
    public static void Bg_ScheduleAffineRotationCenter(object bgConfig, byte bgLayer, byte bgAffineUpdateOp, int value)
    {
        BgConfig config = Config(bgConfig);
        Bg_UpdateRotationCenter(config.Bgs[bgLayer], bgAffineUpdateOp, value);
        config.ScrollScheduled |= (ushort)(1 << bgLayer);
    }

    // OFFSET 0x0201C73C
    private static void Bg_UpdateRotationCenter(Background bg, byte bgAffineUpdateOp, int value)
    {
        switch (bgAffineUpdateOp)
        {
            case CenterSetX:
                bg.XCenter = value;
                break;
            case CenterAddX:
                bg.XCenter += value;
                break;
            case CenterSubX:
                bg.XCenter -= value;
                break;
            case CenterSetY:
                bg.YCenter = value;
                break;
            case CenterAddY:
                bg.YCenter += value;
                break;
            case CenterSubY:
                bg.YCenter -= value;
                break;
        }
    }

    // OFFSET 0x0201C784
    public static byte Bg_DoesPixelAtXYMatchVal(object bgConfig, byte bgLayer, ushort x, ushort y, ushort[] source)
    {
        BgConfig config = Config(bgConfig);
        Background bg = config.Bgs[bgLayer];
        byte[]? tilemap = bg.TilemapBuffer;
        if (tilemap == null)
        {
            return 0;
        }
        ushort tileIndex = Bg_GetTileIndex((byte)(x >> 3), (byte)(y >> 3), bg.ScreenSize);
        byte[] tiles = Bg_GetCharPtr(bgLayer);
        byte pixelX = (byte)(x & 7);
        byte pixelY = (byte)(y & 7);
        byte pixel;
        if (bg.ColorMode == 0)
        {
            byte[] tile = new byte[TilePixels];
            ushort entry = TilemapU16(tilemap, tileIndex);
            int tileStart = (entry & TileIndexMask) << 5;
            for (byte i = 0; i < GlyphTileBytes4bpp; i++)
            {
                tile[i << 1] = (byte)(tiles[tileStart + i] & 0xF);
                tile[(i << 1) + 1] = (byte)(tiles[tileStart + i] >> 4);
            }
            Bg_FlipTile(config, (byte)((entry >> 10) & 3), tile);
            pixel = tile[pixelX + (pixelY << 3)];
            if ((source[0] & (1 << pixel)) != 0)
            {
                return 1;
            }
            return 0;
        }
        if (bg.Type != TypeAffine)
        {
            byte[] tile = new byte[TilePixels];
            ushort entry = TilemapU16(tilemap, tileIndex);
            Array.Copy(tiles, (entry & TileIndexMask) << 6, tile, 0, TilePixels);
            Bg_FlipTile(config, (byte)((entry >> 10) & 3), tile);
            pixel = tile[pixelX + (pixelY << 3)];
        }
        else
        {
            pixel = tiles[(tilemap[tileIndex] << 6) + pixelX + (pixelY << 3)];
        }
        while (true)
        {
            if (source[0] == EndOfColorList)
            {
                break;
            }
            if (pixel == (byte)source[0])
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0201C8E0
    private static void Bg_FlipTile(BgConfig bgConfig, byte flip, byte[] pixels)
    {
        if (flip == 0)
        {
            return;
        }
        byte[] temporary = new byte[TilePixels];
        if ((flip & FlipHorizontal) != 0)
        {
            for (byte row = 0; row < 8; row++)
            {
                for (byte column = 0; column < 8; column++)
                {
                    temporary[(row << 3) + column] = pixels[(row << 3) + (7 - column)];
                }
            }
            Array.Copy(temporary, pixels, TilePixels);
        }
        if ((flip & FlipVertical) != 0)
        {
            for (byte row = 0; row < 8; row++)
            {
                Array.Copy(pixels, (7 - row) << 3, temporary, row << 3, 8);
            }
            Array.Copy(temporary, pixels, TilePixels);
        }
    }
}
