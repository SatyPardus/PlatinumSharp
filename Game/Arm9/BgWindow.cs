using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class Bitmap
{
    public byte[] Pixels = Array.Empty<byte>();
    public ushort Width;
    public ushort Height;
}

public sealed class WindowTemplate
{
    public byte BgLayer;
    public byte TilemapLeft;
    public byte TilemapTop;
    public byte Width;
    public byte Height;
    public byte Palette;
    public ushort BaseTile;
}

public delegate void WindowFunc(Window window);

public static partial class Bg
{
    public const ushort NoTransparentColor = 0xFFFF;

    private const int NoBgLayer = 0xFF;
    private const int BaseTileMask = 0x7FFF;
    private const int TileRowMask4bpp = 0x3FE0;
    private const int TileRowMask8bpp = 0x7FC0;

    private static readonly byte[] sAffineScreenWidths = { 0x10, 0x20, 0x20, 0x20, 0x20, 0x20 };

    private static readonly WindowFunc[] sPutToTilemapFuncs = { Window_PutToTextTilemap, Window_PutToAffineTilemap, Window_PutToTextTilemap };
    private static readonly WindowFunc[] sClearAndScheduleCopyFuncs = { Window_ClearAndScheduleCopyText, Window_ClearAndScheduleCopyAffine, Window_ClearAndScheduleCopyText };
    private static readonly WindowFunc[] sClearAndCopyFuncs = { Window_ClearAndCopyText, Window_ClearAndCopyAffine, Window_ClearAndCopyText };
    private static readonly WindowFunc[] sScheduleCopyFuncs = { Window_ScheduleCopyText, Window_ScheduleCopyAffine, Window_ScheduleCopyText };
    private static readonly WindowFunc[] sCopyFuncs = { Window_CopyText, Window_CopyAffine, Window_CopyText };
    private static readonly WindowFunc[] sClearTilemapFuncs = { Window_ClearTextTilemap, Window_ClearAffineTilemap, Window_ClearTextTilemap };

    private static Window Win(object window)
    {
        return (Window)window;
    }

    private static Background WindowBg(Window window)
    {
        return Config(window.BgConfig).Bgs[window.BgLayer];
    }

    private static int TilesWide(int pixels)
    {
        return (pixels + (pixels & 7)) >> 3;
    }

    private static int PixelOffset4bpp(int x, int y, int tilesWide)
    {
        return ((x >> 1) & 3) + ((x << 2) & TileRowMask4bpp) + ((y << 2) & TileRowMask4bpp) * tilesWide + ((y << 2) & 0x1C);
    }

    private static int PixelOffset8bpp(int x, int y, int tilesWide)
    {
        return (x & 7) + ((x << 3) & TileRowMask8bpp) + ((y << 3) & TileRowMask8bpp) * tilesWide + ((y << 3) & 0x38);
    }

    // OFFSET 0x0201A1E4
    public static void Bitmap_BlitRect4bpp(Bitmap source, Bitmap destination, ushort sourceX, ushort sourceY, ushort destX, ushort destY, ushort width, ushort height, ushort transparent)
    {
        int endX;
        int endY;
        if (destination.Width - destX < width)
        {
            endX = sourceX + (destination.Width - destX);
        }
        else
        {
            endX = width + sourceX;
        }
        if (destination.Height - destY < height)
        {
            endY = sourceY + (destination.Height - destY);
        }
        else
        {
            endY = height + sourceY;
        }
        int sourceTilesWide = TilesWide(source.Width);
        int destTilesWide = TilesWide(destination.Width);
        int currentDestY = destY;
        for (int y = sourceY; y < endY; y++)
        {
            int currentDestX = destX;
            for (int x = sourceX; x < endX; x++)
            {
                int sourceOffset = PixelOffset4bpp(x, y, sourceTilesWide);
                int destOffset = PixelOffset4bpp(currentDestX, currentDestY, destTilesWide);
                int pixel = (source.Pixels[sourceOffset] >> ((x & 1) << 2)) & 0xF;
                if (transparent == NoTransparentColor || pixel != transparent)
                {
                    int shift = (currentDestX & 1) << 2;
                    destination.Pixels[destOffset] = (byte)((pixel << shift) | (destination.Pixels[destOffset] & (0xF0 >> shift)));
                }
                currentDestX++;
            }
            currentDestY++;
        }
    }

    // OFFSET 0x0201A424
    public static void Bitmap_BlitRect8bpp(Bitmap source, Bitmap destination, ushort sourceX, ushort sourceY, ushort destX, ushort destY, ushort width, ushort height, ushort transparent)
    {
        int endX;
        int endY;
        if (destination.Width - destX < width)
        {
            endX = sourceX + (destination.Width - destX);
        }
        else
        {
            endX = width + sourceX;
        }
        if (destination.Height - destY < height)
        {
            endY = sourceY + (destination.Height - destY);
        }
        else
        {
            endY = height + sourceY;
        }
        int sourceTilesWide = TilesWide(source.Width);
        int destTilesWide = TilesWide(destination.Width);
        int currentDestY = destY;
        for (int y = sourceY; y < endY; y++)
        {
            int currentDestX = destX;
            for (int x = sourceX; x < endX; x++)
            {
                byte pixel = source.Pixels[PixelOffset8bpp(x, y, sourceTilesWide)];
                if (transparent == NoTransparentColor || pixel != transparent)
                {
                    destination.Pixels[PixelOffset8bpp(currentDestX, currentDestY, destTilesWide)] = pixel;
                }
                currentDestX++;
            }
            currentDestY++;
        }
    }

    // OFFSET 0x0201A60C
    public static void Bitmap_FillRect4bpp(Bitmap bitmap, ushort x, ushort y, ushort width, ushort height, byte fillValue)
    {
        int endX = x + width;
        if (endX > bitmap.Width)
        {
            endX = bitmap.Width;
        }
        int endY = y + height;
        if (endY > bitmap.Height)
        {
            endY = bitmap.Height;
        }
        int tilesWide = TilesWide(bitmap.Width);
        for (int row = y; row < endY; row++)
        {
            for (int column = x; column < endX; column++)
            {
                int offset = PixelOffset4bpp(column, row, tilesWide);
                if ((column & 1) != 0)
                {
                    bitmap.Pixels[offset] &= 0x0F;
                    bitmap.Pixels[offset] |= (byte)(fillValue << 4);
                }
                else
                {
                    bitmap.Pixels[offset] &= 0xF0;
                    bitmap.Pixels[offset] |= fillValue;
                }
            }
        }
    }

    // OFFSET 0x0201A6D0
    public static void Bitmap_FillRect8bpp(Bitmap bitmap, ushort x, ushort y, ushort width, ushort height, byte fillValue)
    {
        int endX = x + width;
        if (endX > bitmap.Width)
        {
            endX = bitmap.Width;
        }
        int endY = y + height;
        if (endY > bitmap.Height)
        {
            endY = bitmap.Height;
        }
        int tilesWide = TilesWide(bitmap.Width);
        for (int row = y; row < endY; row++)
        {
            for (int column = x; column < endX; column++)
            {
                bitmap.Pixels[PixelOffset8bpp(column, row, tilesWide)] = fillValue;
            }
        }
    }

    // OFFSET 0x0201A778
    public static Window[] Window_New(int heapID, byte windowCount)
    {
        Window[] windows = new Window[windowCount];
        for (ushort i = 0; i < windowCount; i++)
        {
            windows[i] = new Window();
            Window_Init(windows[i]);
        }
        return windows;
    }

    // OFFSET 0x0201A7A0
    public static void Window_Init(object window)
    {
        Window target = Win(window);
        target.BgConfig = null!;
        target.BgLayer = NoBgLayer;
        target.TilemapLeft = 0;
        target.TilemapTop = 0;
        target.Width = 0;
        target.Height = 0;
        target.Palette = 0;
        target.BaseTile = 0;
        target.Pixels = null;
        target.ColorMode = 0;
    }

    // OFFSET 0x0201A7CC
    public static byte Window_IsInUse(object window)
    {
        Window target = Win(window);
        if (target.BgConfig == null || target.BgLayer == NoBgLayer || target.Pixels == null)
        {
            return 0;
        }
        return 1;
    }

    // OFFSET 0x0201A7E8
    public static void Window_Add(object bgConfig, object window, byte bgLayer, byte tilemapLeft, byte tilemapTop, byte width, byte height, byte palette, ushort baseTile)
    {
        Window target = Win(window);
        Background bg = Config(bgConfig).Bgs[bgLayer];
        if (bg.TilemapBuffer == null)
        {
            return;
        }
        byte[] pixels = new byte[width * height * bg.TileSize];
        target.BgConfig = bgConfig;
        target.BgLayer = bgLayer;
        target.TilemapLeft = tilemapLeft;
        target.TilemapTop = tilemapTop;
        target.Width = width;
        target.Height = height;
        target.Palette = palette;
        target.BaseTile = (ushort)(baseTile & BaseTileMask);
        target.Pixels = pixels;
        target.ColorMode = (byte)(bg.ColorMode == 0 ? 0 : 1);
    }

    // OFFSET 0x0201A870
    public static void Window_AddToTopLeftCorner(object bgConfig, object window, byte width, byte height, ushort baseTile, byte fillValue)
    {
        Window target = Win(window);
        byte[] pixels = new byte[width * height * TileSize4bpp];
        Array.Fill(pixels, (byte)((fillValue << 4) | fillValue));
        target.BgConfig = bgConfig;
        target.Width = width;
        target.Height = height;
        target.BaseTile = (ushort)(baseTile & BaseTileMask);
        target.Pixels = pixels;
        target.ColorMode = 0;
    }

    // OFFSET 0x0201A8D4
    public static void Window_AddFromTemplate(object bgConfig, object window, object template)
    {
        WindowTemplate source = (WindowTemplate)template;
        Window_Add(bgConfig, window, source.BgLayer, source.TilemapLeft, source.TilemapTop, source.Width, source.Height, source.Palette, source.BaseTile);
    }

    // OFFSET 0x0201A8FC
    public static void Window_Remove(object window)
    {
        Window target = Win(window);
        target.Pixels = null;
        target.BgConfig = null!;
        target.BgLayer = NoBgLayer;
        target.TilemapLeft = 0;
        target.TilemapTop = 0;
        target.Width = 0;
        target.Height = 0;
        target.Palette = 0;
        target.BaseTile = 0;
    }

    // OFFSET 0x0201A928
    public static void Windows_Delete(Window[] windows, byte windowCount)
    {
        for (ushort i = 0; i < windowCount; i++)
        {
            if (windows[i].Pixels != null)
            {
                windows[i].Pixels = null;
            }
        }
    }

    private static void Window_Dispatch(Window? window, WindowFunc[] funcs)
    {
        if (window == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (window!.BgConfig == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (window.BgLayer >= LayerCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (WindowBg(window).Type >= 3)
        {
            ErrorHandling.GF_AssertFail();
        }
        funcs[WindowBg(window).Type](window);
    }

    // OFFSET 0x0201A954
    public static void Window_CopyToVRAM(object window)
    {
        Window_Dispatch((Window?)window, sCopyFuncs);
    }

    // OFFSET 0x0201A9A4
    public static void Window_ScheduleCopyToVRAM(object window)
    {
        Window_Dispatch((Window?)window, sScheduleCopyFuncs);
    }

    // OFFSET 0x0201A9F4
    public static void Window_PutToTilemap(object window)
    {
        Window target = Win(window);
        sPutToTilemapFuncs[WindowBg(target).Type](target);
    }

    // OFFSET 0x0201AA10
    public static void Window_PutRectToTilemap(object window, uint width, uint height)
    {
        Window target = Win(window);
        byte savedWidth = target.Width;
        byte savedHeight = target.Height;
        target.Width = (byte)width;
        target.Height = (byte)height;
        sPutToTilemapFuncs[WindowBg(target).Type](target);
        target.Width = savedWidth;
        target.Height = savedHeight;
    }

    // OFFSET 0x0201AA3C
    public static void Window_ClearTilemap(object window)
    {
        Window target = Win(window);
        sClearTilemapFuncs[WindowBg(target).Type](target);
    }

    private static int TextTilemapIndex(int x, int y)
    {
        return ((y & 0x1F) << 5) + (((y & 0x20) + (x & 0x20)) << 5) + (x & 0x1F);
    }

    // OFFSET 0x0201AA58
    private static void Window_PutToTextTilemap(Window window)
    {
        byte[]? tilemap = WindowBg(window).TilemapBuffer;
        if (tilemap == null)
        {
            return;
        }
        uint tile = window.BaseTile;
        uint right = (uint)(window.TilemapLeft + window.Width);
        uint bottom = (uint)(window.TilemapTop + window.Height);
        for (uint y = window.TilemapTop; y < bottom; y++)
        {
            for (uint x = window.TilemapLeft; x < right; x++)
            {
                SetTilemapU16(tilemap, TextTilemapIndex((int)x, (int)y), (ushort)(tile | (uint)(window.Palette << 12)));
                tile++;
            }
        }
    }

    // OFFSET 0x0201AADC
    private static void Window_PutToAffineTilemap(Window window)
    {
        Background bg = WindowBg(window);
        byte[]? tilemap = bg.TilemapBuffer;
        if (tilemap == null)
        {
            return;
        }
        int screenWidth = sAffineScreenWidths[bg.ScreenSize];
        int row = window.TilemapTop * screenWidth + window.TilemapLeft;
        int tile = window.BaseTile;
        for (int y = 0; y < window.Height; y++)
        {
            for (int x = 0; x < window.Width; x++)
            {
                tilemap[row + x] = (byte)tile;
                tile++;
            }
            row += screenWidth;
        }
    }

    // OFFSET 0x0201AB38
    private static void Window_ClearTextTilemap(Window window)
    {
        byte[]? tilemap = WindowBg(window).TilemapBuffer;
        if (tilemap == null)
        {
            return;
        }
        uint right = (uint)(window.TilemapLeft + window.Width);
        uint bottom = (uint)(window.TilemapTop + window.Height);
        for (uint y = window.TilemapTop; y < bottom; y++)
        {
            for (uint x = window.TilemapLeft; x < right; x++)
            {
                SetTilemapU16(tilemap, TextTilemapIndex((int)x, (int)y), 0);
            }
        }
    }

    // OFFSET 0x0201ABC8
    private static void Window_ClearAffineTilemap(Window window)
    {
        Background bg = WindowBg(window);
        byte[]? tilemap = bg.TilemapBuffer;
        if (tilemap == null)
        {
            return;
        }
        int screenWidth = sAffineScreenWidths[bg.ScreenSize];
        int row = window.TilemapTop * screenWidth + window.TilemapLeft;
        for (int y = 0; y < window.Height; y++)
        {
            for (int x = 0; x < window.Width; x++)
            {
                tilemap[row + x] = 0;
            }
            row += screenWidth;
        }
    }

    private static void Window_CopyTilemapToVRAM(Window window)
    {
        Background bg = WindowBg(window);
        Bg_CopyTilemapBufferRangeToVRAM(window.BgConfig, window.BgLayer, bg.TilemapBuffer!, bg.BufferSize, bg.BaseTile);
    }

    // OFFSET 0x0201AC20
    private static void Window_CopyText(Window window)
    {
        Window_PutToTextTilemap(window);
        Window_LoadTiles(window);
        Window_CopyTilemapToVRAM(window);
    }

    // OFFSET 0x0201AC4C
    private static void Window_ScheduleCopyText(Window window)
    {
        Window_PutToTextTilemap(window);
        Bg_ScheduleTilemapTransfer(window.BgConfig, window.BgLayer);
        Window_LoadTiles(window);
    }

    // OFFSET 0x0201AC64
    private static void Window_CopyAffine(Window window)
    {
        Window_PutToAffineTilemap(window);
        Window_CopyTilemapToVRAM(window);
        Bg_LoadTiles(window.BgConfig, window.BgLayer, window.Pixels!, (uint)(window.Width * window.Height * TileSize8bpp), window.BaseTile);
    }

    // OFFSET 0x0201ACA0
    private static void Window_ScheduleCopyAffine(Window window)
    {
        Window_PutToAffineTilemap(window);
        Bg_ScheduleTilemapTransfer(window.BgConfig, window.BgLayer);
        Bg_LoadTiles(window.BgConfig, window.BgLayer, window.Pixels!, (uint)(window.Width * window.Height * TileSize8bpp), window.BaseTile);
    }

    // OFFSET 0x0201ACCC
    public static void Window_LoadTiles(object window)
    {
        Window target = Win(window);
        Bg_LoadTiles(target.BgConfig, target.BgLayer, target.Pixels!, (uint)(target.Width * target.Height * WindowBg(target).TileSize), target.BaseTile);
    }

    // OFFSET 0x0201ACF4
    public static void Window_ClearAndCopyToVRAM(object window)
    {
        Window target = Win(window);
        sClearAndCopyFuncs[WindowBg(target).Type](target);
    }

    // OFFSET 0x0201AD10
    public static void Window_ClearAndScheduleCopyToVRAM(object window)
    {
        Window target = Win(window);
        sClearAndScheduleCopyFuncs[WindowBg(target).Type](target);
    }

    // OFFSET 0x0201AD2C
    private static void Window_ClearAndCopyText(Window window)
    {
        Window_ClearTextTilemap(window);
        Window_CopyTilemapToVRAM(window);
    }

    // OFFSET 0x0201AD54
    private static void Window_ClearAndScheduleCopyText(Window window)
    {
        Window_ClearTextTilemap(window);
        Bg_ScheduleTilemapTransfer(window.BgConfig, window.BgLayer);
    }

    // OFFSET 0x0201AD68
    private static void Window_ClearAndCopyAffine(Window window)
    {
        Window_ClearAffineTilemap(window);
        Window_CopyTilemapToVRAM(window);
    }

    // OFFSET 0x0201AD90
    private static void Window_ClearAndScheduleCopyAffine(Window window)
    {
        Window_ClearAffineTilemap(window);
        Bg_ScheduleTilemapTransfer(window.BgConfig, window.BgLayer);
    }

    // OFFSET 0x0201ADA4
    public static void Window_FillTilemap(object window, int value)
    {
        Window target = Win(window);
        int tileSize = WindowBg(target).TileSize;
        byte fill = (byte)value;
        if (tileSize == TileSize4bpp)
        {
            fill = (byte)((fill << 4) | fill);
        }
        Array.Fill(target.Pixels!, fill, 0, tileSize * target.Width * target.Height);
    }

    // OFFSET 0x0201ADDC
    public static void Window_BlitBitmapRect(object window, byte[] source, int sourceX, int sourceY, int sourceWidth, int sourceHeight, int destX, int destY, int destWidth, int destHeight)
    {
        Window_BlitBitmapRectWithTransparency(window, source, sourceX, sourceY, sourceWidth, sourceHeight, destX, destY, destWidth, destHeight, 0);
    }

    // OFFSET 0x0201AE08
    public static void Window_BlitBitmapRectWithTransparency(object window, byte[] source, int sourceX, int sourceY, int sourceWidth, int sourceHeight, int destX, int destY, int destWidth, int destHeight, int transparent)
    {
        Window target = Win(window);
        Bitmap sourceBitmap = new() { Pixels = source, Width = (ushort)sourceWidth, Height = (ushort)sourceHeight };
        Bitmap destBitmap = new() { Pixels = target.Pixels!, Width = (ushort)(target.Width << 3), Height = (ushort)(target.Height << 3) };
        if (WindowBg(target).ColorMode == 0)
        {
            Bitmap_BlitRect4bpp(sourceBitmap, destBitmap, (ushort)sourceX, (ushort)sourceY, (ushort)destX, (ushort)destY, (ushort)destWidth, (ushort)destHeight, (ushort)transparent);
        }
        else
        {
            Bitmap_BlitRect8bpp(sourceBitmap, destBitmap, (ushort)sourceX, (ushort)sourceY, (ushort)destX, (ushort)destY, (ushort)destWidth, (ushort)destHeight, (ushort)transparent);
        }
    }

    // OFFSET 0x0201AE78
    public static void Window_FillRectWithColor(object window, int color, int x, int y, int width, int height)
    {
        Window target = Win(window);
        Bitmap bitmap = new() { Pixels = target.Pixels!, Width = (ushort)(target.Width << 3), Height = (ushort)(target.Height << 3) };
        if (WindowBg(target).ColorMode == 0)
        {
            Bitmap_FillRect4bpp(bitmap, (ushort)x, (ushort)y, (ushort)width, (ushort)height, (byte)color);
        }
        else
        {
            Bitmap_FillRect8bpp(bitmap, (ushort)x, (ushort)y, (ushort)width, (ushort)height, (byte)color);
        }
    }
}
