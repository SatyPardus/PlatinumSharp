namespace PlatinumSharp.Nitro.Graphics;

public sealed class Raster
{
    public int Width { get; }
    public int Height { get; }
    public int[] Pixels { get; }

    public Raster(int width, int height)
    {
        Width = width;
        Height = height;
        Pixels = new int[width * height];
    }
}

public sealed class Palette
{
    public int[] Colors { get; }
    public bool Is8Bpp { get; }

    private Palette(int[] colors, bool is8Bpp)
    {
        Colors = colors;
        Is8Bpp = is8Bpp;
    }

    public static Palette Grayscale()
    {
        int[] colors = new int[256];
        for (int i = 0; i < 256; i++)
        {
            int v = i < 16 ? i * 17 : i;
            colors[i] = unchecked((int)0xFF000000) | (v << 16) | (v << 8) | v;
        }
        for (int i = 16; i < 256; i++)
        {
            int v = (i & 15) * 17;
            colors[i] = unchecked((int)0xFF000000) | (v << 16) | (v << 8) | v;
        }
        return new Palette(colors, false);
    }

    public static Palette Parse(byte[] d)
    {
        int s = Bin.FindSection(d, "TTLP");
        if (s < 0)
        {
            throw new InvalidDataException("NCLR has no palette section");
        }
        int sectionSize = Bin.I32(d, s + 4);
        int offset = s + 8 + Bin.I32(d, s + 0x14);
        int count = Math.Min(sectionSize - 0x18, d.Length - offset) / 2;
        int[] colors = new int[Math.Max(count, 256)];
        for (int i = 0; i < count; i++)
        {
            colors[i] = ToArgb(Bin.U16(d, offset + i * 2));
        }
        return new Palette(colors, Bin.U32(d, s + 8) == 4);
    }

    public static int ToArgb(int bgr555)
    {
        int r = bgr555 & 31;
        int g = (bgr555 >> 5) & 31;
        int b = (bgr555 >> 10) & 31;
        r = (r << 3) | (r >> 2);
        g = (g << 3) | (g >> 2);
        b = (b << 3) | (b >> 2);
        return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
    }
}

public sealed class TileSet
{
    public int WidthTiles { get; }
    public int HeightTiles { get; }
    public int Bpp { get; }
    public byte[] Pixels { get; }

    private TileSet(int widthTiles, int heightTiles, int bpp, byte[] pixels)
    {
        WidthTiles = widthTiles;
        HeightTiles = heightTiles;
        Bpp = bpp;
        Pixels = pixels;
    }

    public static TileSet Parse(byte[] d)
    {
        int s = Bin.FindSection(d, "RAHC");
        if (s < 0)
        {
            throw new InvalidDataException("NCGR has no character section");
        }
        int h = Bin.U16(d, s + 8);
        int w = Bin.U16(d, s + 10);
        int bpp = Bin.U32(d, s + 12) == 4 ? 8 : 4;
        bool linear = (Bin.U32(d, s + 0x14) & 0xFF) == 1;
        int offset = s + 8 + Bin.I32(d, s + 0x1C);
        int size = Math.Min(Bin.I32(d, s + 0x18), d.Length - offset);
        int tiles = size / (bpp * 8);
        if (w == 0xFFFF || h == 0xFFFF || w * h == 0)
        {
            w = Math.Clamp(tiles, 1, 32);
            h = Math.Max(1, (tiles + w - 1) / w);
        }
        byte[] pixels = new byte[w * 8 * h * 8];
        int total = Math.Min(pixels.Length, size * 8 / bpp);
        if (linear)
        {
            for (int i = 0; i < total; i++)
            {
                pixels[i] = ReadPixel(d, offset, i, bpp);
            }
        }
        else
        {
            for (int i = 0; i < total; i++)
            {
                int tile = i >> 6;
                int x = (tile % w) * 8 + (i & 7);
                int y = (tile / w) * 8 + ((i >> 3) & 7);
                pixels[y * w * 8 + x] = ReadPixel(d, offset, i, bpp);
            }
        }
        return new TileSet(w, h, bpp, pixels);
    }

    private static byte ReadPixel(byte[] d, int offset, int index, int bpp)
    {
        if (bpp == 8)
        {
            return d[offset + index];
        }
        return (byte)((d[offset + (index >> 1)] >> ((index & 1) * 4)) & 0xF);
    }

    public int GetTilePixel(int tile, int x, int y)
    {
        int tx = tile % WidthTiles;
        int ty = tile / WidthTiles;
        if (ty >= HeightTiles)
        {
            return 0;
        }
        return Pixels[(ty * 8 + y) * WidthTiles * 8 + tx * 8 + x];
    }
}

public sealed class TileMap
{
    public int Width { get; }
    public int Height { get; }
    public ushort[] Cells { get; }

    private TileMap(int width, int height, ushort[] cells)
    {
        Width = width;
        Height = height;
        Cells = cells;
    }

    public static TileMap Parse(byte[] d)
    {
        int s = Bin.FindSection(d, "NRCS");
        if (s < 0)
        {
            throw new InvalidDataException("NSCR has no screen section");
        }
        int width = Bin.U16(d, s + 8);
        int height = Bin.U16(d, s + 10);
        int size = Math.Min(Bin.I32(d, s + 0x10), d.Length - (s + 0x14));
        ushort[] cells = new ushort[(width / 8) * (height / 8)];
        for (int i = 0; i < cells.Length && i * 2 + 1 < size; i++)
        {
            cells[i] = (ushort)Bin.U16(d, s + 0x14 + i * 2);
        }
        return new TileMap(width, height, cells);
    }
}
