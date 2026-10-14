using System.Buffers.Binary;
using System.IO.Compression;

namespace PlatinumSharp.Nitro.Graphics;

public static class Renderer
{
    public static Raster RenderTiles(TileSet tiles, Palette palette, int paletteRow)
    {
        int width = tiles.WidthTiles * 8;
        int height = tiles.HeightTiles * 8;
        Raster raster = new(width, height);
        int baseColor = tiles.Bpp == 4 ? paletteRow * 16 : 0;
        for (int i = 0; i < raster.Pixels.Length; i++)
        {
            raster.Pixels[i] = Lookup(palette, baseColor + tiles.Pixels[i]);
        }
        return raster;
    }

    public static Raster RenderPalette(Palette palette)
    {
        const int swatch = 12;
        int rows = (palette.Colors.Length + 15) / 16;
        Raster raster = new(16 * swatch, rows * swatch);
        for (int y = 0; y < raster.Height; y++)
        {
            for (int x = 0; x < raster.Width; x++)
            {
                raster.Pixels[y * raster.Width + x] = Lookup(palette, (y / swatch) * 16 + x / swatch);
            }
        }
        return raster;
    }

    public static Raster RenderScreen(TileMap map, TileSet tiles, Palette palette)
    {
        Raster raster = new(map.Width, map.Height);
        Array.Fill(raster.Pixels, Lookup(palette, 0));
        DrawLayer(raster, map, tiles, palette, 0, 0);
        return raster;
    }

    public static void DrawLayer(Raster target, TileMap map, TileSet tiles, Palette palette, int scrollX, int scrollY)
    {
        int columns = map.Width / 8;
        int rows = map.Height / 8;
        if (columns == 0 || rows == 0)
        {
            return;
        }
        for (int y = 0; y < target.Height; y++)
        {
            int my = Mod(y + scrollY, map.Height);
            for (int x = 0; x < target.Width; x++)
            {
                int mx = Mod(x + scrollX, map.Width);
                int cell = map.Cells[(my / 8) * columns + mx / 8];
                int px = (cell & 0x400) != 0 ? 7 - (mx & 7) : mx & 7;
                int py = (cell & 0x800) != 0 ? 7 - (my & 7) : my & 7;
                int index = tiles.GetTilePixel(cell & 0x3FF, px, py);
                if (index == 0)
                {
                    continue;
                }
                int color = tiles.Bpp == 4 ? (cell >> 12) * 16 + index : index;
                target.Pixels[y * target.Width + x] = Lookup(palette, color);
            }
        }
    }

    private static int Mod(int value, int modulus)
    {
        int r = value % modulus;
        return r < 0 ? r + modulus : r;
    }

    private static int Lookup(Palette palette, int index)
    {
        return index < palette.Colors.Length ? palette.Colors[index] | unchecked((int)0xFF000000) : unchecked((int)0xFF000000);
    }

    public static void SavePng(Raster raster, string path)
    {
        using FileStream file = File.Create(path);
        file.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        byte[] header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(0), raster.Width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), raster.Height);
        header[8] = 8;
        header[9] = 6;
        WriteChunk(file, "IHDR", header);
        using MemoryStream packed = new();
        using (ZLibStream zlib = new(packed, CompressionLevel.Optimal, true))
        {
            byte[] row = new byte[1 + raster.Width * 4];
            for (int y = 0; y < raster.Height; y++)
            {
                for (int x = 0; x < raster.Width; x++)
                {
                    int argb = raster.Pixels[y * raster.Width + x];
                    row[1 + x * 4] = (byte)(argb >> 16);
                    row[2 + x * 4] = (byte)(argb >> 8);
                    row[3 + x * 4] = (byte)argb;
                    row[4 + x * 4] = (byte)(argb >> 24);
                }
                zlib.Write(row);
            }
        }
        WriteChunk(file, "IDAT", packed.ToArray());
        WriteChunk(file, "IEND", Array.Empty<byte>());
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        byte[] buffer = new byte[data.Length + 12];
        BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(0), data.Length);
        for (int i = 0; i < 4; i++)
        {
            buffer[4 + i] = (byte)type[i];
        }
        data.CopyTo(buffer, 8);
        uint crc = 0xFFFFFFFF;
        for (int i = 4; i < data.Length + 8; i++)
        {
            crc ^= buffer[i];
            for (int k = 0; k < 8; k++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
            }
        }
        BinaryPrimitives.WriteUInt32BigEndian(buffer.AsSpan(data.Length + 8), ~crc);
        stream.Write(buffer);
    }
}
