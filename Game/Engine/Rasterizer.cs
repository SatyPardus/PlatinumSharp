using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class Rasterizer
{
    public const int Width = 256;
    public const int Height = 192;

    private const int LayersPerDisplay = 4;
    private const int ScreenBlockBytes = 0x800;
    private const int Transparent = -1;
    private const int LayerObject = 4;
    private const int LayerBackdrop = 5;
    private const int ObjectNormal = 0;
    private const int ObjectSemiTransparent = 1;
    private const int ObjectWindow = 2;
    private const int ObjectBitmap = 3;
    private const int NoPriority = 99;

    private static readonly int[] objectWidths = { 8, 16, 32, 64, 16, 32, 32, 64, 8, 8, 16, 32 };
    private static readonly int[] objectHeights = { 8, 16, 32, 64, 8, 8, 16, 32, 16, 32, 32, 64 };
    private static readonly int[][] bgLines = { new int[Width], new int[Width], new int[Width], new int[Width] };
    private static readonly int[] bg0Alpha = new int[Width];
    private static readonly int[] objLine = new int[Width];
    private static readonly byte[] objPriority = new byte[Width];
    private static readonly bool[] objSemi = new bool[Width];
    private static readonly bool[] objWindow = new bool[Width];
    private static readonly int[] order = new int[LayersPerDisplay];
    private static readonly int[] orderPriority = new int[LayersPerDisplay];

    public static int LayerMask { get; set; } = 0x1F;

    public static int ToArgb(ushort color)
    {
        int r = (color & 0x1F) * 255 / 31;
        int g = ((color >> 5) & 0x1F) * 255 / 31;
        int b = ((color >> 10) & 0x1F) * 255 / 31;
        return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
    }

    public static void RenderBackgrounds(int display, int[] pixels)
    {
        Render(display, pixels);
    }

    public static void RenderObjects(int display, int[] pixels)
    {
    }

    public static void Render(int display, int[] pixels)
    {
        uint control = Video.GetDisplayControl(display);
        ushort[] palette = Video.GetPalette(display == 0 ? Video.MainBackground : Video.SubBackground);
        int backdrop = palette[0] & 0x7FFF;
        int mode = (int)(control & 7);
        bool bg03D = display == 0 && (control & 8) != 0;
        int windows = (int)((control >> 13) & 7) | Video.GetVisibleWindows(display);
        ushort blendControl = Video.GetBlendControl(display);
        ushort blendAlpha = Video.GetBlendAlpha(display);
        int firstTargets = blendControl & 0x3F;
        int secondTargets = (blendControl >> 8) & 0x3F;
        int blendMode = (blendControl >> 6) & 3;
        int eva = Math.Min(16, blendAlpha & 0x1F);
        int evb = Math.Min(16, (blendAlpha >> 8) & 0x1F);
        int evy = Math.Min(16, Math.Abs(Video.GetBlendBrightness(display)));
        int count = 0;
        for (int priority = 0; priority < 4; priority++)
        {
            for (int index = 0; index < LayersPerDisplay; index++)
            {
                if ((control & (0x100u << index)) == 0 || (LayerMask & (1 << index)) == 0)
                {
                    continue;
                }
                if ((Video.GetBgControl(display * LayersPerDisplay + index) & 3) != priority)
                {
                    continue;
                }
                order[count] = index;
                orderPriority[count] = priority;
                count++;
            }
        }
        bool objects = (control & 0x1000) != 0 && (LayerMask & 0x10) != 0;
        (byte Left, byte Right, byte Top, byte Bottom) window0 = Video.GetWindow(display, 0);
        (byte Left, byte Right, byte Top, byte Bottom) window1 = Video.GetWindow(display, 1);
        int inside0 = Video.GetWindowInside(display, 0);
        int inside1 = Video.GetWindowInside(display, 1);
        int outside = Video.GetWindowOutside(display);
        for (int y = 0; y < Height; y++)
        {
            for (int k = 0; k < count; k++)
            {
                int index = order[k];
                int layer = display * LayersPerDisplay + index;
                int[] line = bgLines[index];
                if (index == 0 && bg03D)
                {
                    Render3DLine(y, line);
                    continue;
                }
                int type = BgType(mode, index);
                if (type == 0)
                {
                    RenderTextLine(display, layer, index, control, palette, y, line);
                }
                else if (type == 1)
                {
                    RenderAffineLine(display, layer, index, control, palette, y, line, false);
                }
                else if (type == 2)
                {
                    RenderAffineLine(display, layer, index, control, palette, y, line, true);
                }
                else
                {
                    Array.Fill(line, Transparent);
                }
            }
            if (objects)
            {
                RenderObjectLine(display, control, y);
            }
            bool row0 = (windows & 1) != 0 && InRange(y, window0.Top, window0.Bottom, Height);
            bool row1 = (windows & 2) != 0 && InRange(y, window1.Top, window1.Bottom, Height);
            int rowOffset = y * Width;
            for (int x = 0; x < Width; x++)
            {
                int mask = 0x3F;
                if (windows != 0)
                {
                    if (row0 && InRange(x, window0.Left, window0.Right, Width))
                    {
                        mask = inside0;
                    }
                    else if (row1 && InRange(x, window1.Left, window1.Right, Width))
                    {
                        mask = inside1;
                    }
                    else if ((windows & 4) != 0 && objects && objWindow[x])
                    {
                        mask = outside;
                    }
                    else
                    {
                        mask = outside;
                    }
                }
                int topLayer = LayerBackdrop;
                int topColor = backdrop;
                int secondLayer = LayerBackdrop;
                int secondColor = backdrop;
                int found = 0;
                int objectPriority = objects && objLine[x] != Transparent && (mask & 0x10) != 0 ? objPriority[x] : NoPriority;
                for (int k = 0; k <= count && found < 2; k++)
                {
                    int priority = k < count ? orderPriority[k] : 4;
                    if (objectPriority <= priority)
                    {
                        objectPriority = NoPriority;
                        if (found == 0)
                        {
                            topLayer = LayerObject;
                            topColor = objLine[x];
                        }
                        else
                        {
                            secondLayer = LayerObject;
                            secondColor = objLine[x];
                        }
                        found++;
                        if (found == 2)
                        {
                            break;
                        }
                    }
                    if (k == count)
                    {
                        break;
                    }
                    int index = order[k];
                    if ((mask & (1 << index)) == 0)
                    {
                        continue;
                    }
                    int color = bgLines[index][x];
                    if (color == Transparent)
                    {
                        continue;
                    }
                    if (found == 0)
                    {
                        topLayer = index;
                        topColor = color;
                    }
                    else
                    {
                        secondLayer = index;
                        secondColor = color;
                    }
                    found++;
                }
                int result = topColor;
                bool effects = (mask & 0x20) != 0;
                bool secondOk = (secondTargets & (1 << secondLayer)) != 0;
                if (topLayer == LayerObject && objSemi[x] && secondOk)
                {
                    result = Blend(topColor, secondColor, eva, evb);
                }
                else if (topLayer == 0 && bg03D && bg0Alpha[x] < 31 && secondOk && (firstTargets & 1) != 0)
                {
                    int alpha = bg0Alpha[x] + 1;
                    result = Blend(topColor, secondColor, alpha >> 1, 16 - (alpha >> 1));
                }
                else if (effects && (firstTargets & (1 << topLayer)) != 0)
                {
                    if (blendMode == 1)
                    {
                        if (secondOk)
                        {
                            result = Blend(topColor, secondColor, eva, evb);
                        }
                    }
                    else if (blendMode == 2)
                    {
                        result = Brighten(topColor, evy);
                    }
                    else if (blendMode == 3)
                    {
                        result = Darken(topColor, evy);
                    }
                }
                pixels[rowOffset + x] = ToArgb((ushort)result);
            }
        }
    }

    private static bool InRange(int value, int start, int end, int limit)
    {
        if (start <= end)
        {
            return value >= start && value < end;
        }
        return value >= start || value < end;
    }

    private static int Blend(int first, int second, int eva, int evb)
    {
        int r = Math.Min(31, ((first & 0x1F) * eva + (second & 0x1F) * evb) >> 4);
        int g = Math.Min(31, (((first >> 5) & 0x1F) * eva + ((second >> 5) & 0x1F) * evb) >> 4);
        int b = Math.Min(31, (((first >> 10) & 0x1F) * eva + ((second >> 10) & 0x1F) * evb) >> 4);
        return r | (g << 5) | (b << 10);
    }

    private static int Brighten(int color, int evy)
    {
        int r = color & 0x1F;
        int g = (color >> 5) & 0x1F;
        int b = (color >> 10) & 0x1F;
        r += ((31 - r) * evy) >> 4;
        g += ((31 - g) * evy) >> 4;
        b += ((31 - b) * evy) >> 4;
        return r | (g << 5) | (b << 10);
    }

    private static int Darken(int color, int evy)
    {
        int r = color & 0x1F;
        int g = (color >> 5) & 0x1F;
        int b = (color >> 10) & 0x1F;
        r -= (r * evy) >> 4;
        g -= (g * evy) >> 4;
        b -= (b * evy) >> 4;
        return r | (g << 5) | (b << 10);
    }

    private static int BgType(int mode, int index)
    {
        switch (mode)
        {
            case 0:
                return 0;
            case 1:
                return index == 3 ? 1 : 0;
            case 2:
                return index >= 2 ? 1 : 0;
            case 3:
                return index == 3 ? 2 : 0;
            case 4:
                return index == 2 ? 1 : index == 3 ? 2 : 0;
            case 5:
                return index >= 2 ? 2 : 0;
        }
        return 3;
    }

    private static void Render3DLine(int y, int[] line)
    {
        int[] frame = Render3D.Frame;
        int rowOffset = y * Width;
        for (int x = 0; x < Width; x++)
        {
            int value = frame[rowOffset + x];
            int alpha = (value >> 15) & 0x1F;
            if (alpha == 0)
            {
                line[x] = Transparent;
                bg0Alpha[x] = 0;
            }
            else
            {
                line[x] = value & 0x7FFF;
                bg0Alpha[x] = alpha;
            }
        }
    }

    private static ushort[]? ExtendedBgPalette(int display, int index, uint control, ushort bgControl)
    {
        if ((control & 0x40000000) == 0)
        {
            return null;
        }
        int slot = index;
        if (index < 2 && (bgControl & 0x2000) != 0)
        {
            slot += 2;
        }
        return Video.GetPalette((display == 0 ? Video.MainBackgroundExtended : Video.SubBackgroundExtended) + slot);
    }

    private static void RenderTextLine(int display, int layer, int index, uint displayControl, ushort[] palette, int y, int[] line)
    {
        ushort control = Video.GetBgControl(layer);
        byte[] vram = Video.GetBgVram(display);
        int screenBase = Video.GetBgScreenBase(layer);
        int charBase = Video.GetBgCharBase(layer);
        bool wide = (control & 0x4000) != 0;
        bool tall = (control & 0x8000) != 0;
        bool color256 = (control & 0x80) != 0;
        ushort[]? extended = color256 ? ExtendedBgPalette(display, index, displayControl, control) : null;
        int widthMask = (wide ? 512 : 256) - 1;
        int heightMask = (tall ? 512 : 256) - 1;
        uint offset = Video.GetBgOffset(layer);
        int scrollX = (int)(offset & 0x1FF);
        int scrollY = (int)((offset >> 16) & 0x1FF);
        int sourceY = (y + scrollY) & heightMask;
        int length = vram.Length;
        for (int x = 0; x < Width; x++)
        {
            int sourceX = (x + scrollX) & widthMask;
            int block = wide ? (sourceX >> 8) + (tall ? (sourceY >> 8) * 2 : 0) : (tall ? sourceY >> 8 : 0);
            int entryOffset = screenBase + block * ScreenBlockBytes + (((sourceY >> 3) & 0x1F) * 32 + ((sourceX >> 3) & 0x1F)) * 2;
            if (entryOffset + 1 >= length)
            {
                line[x] = Transparent;
                continue;
            }
            int entry = vram[entryOffset] | (vram[entryOffset + 1] << 8);
            int tileX = sourceX & 7;
            int tileY = sourceY & 7;
            if ((entry & 0x400) != 0)
            {
                tileX = 7 - tileX;
            }
            if ((entry & 0x800) != 0)
            {
                tileY = 7 - tileY;
            }
            int color;
            if (color256)
            {
                int tileOffset = charBase + (entry & 0x3FF) * 0x40 + tileY * 8 + tileX;
                color = tileOffset < length ? vram[tileOffset] : 0;
                if (color == 0)
                {
                    line[x] = Transparent;
                    continue;
                }
                line[x] = (extended != null ? extended[((entry >> 12) << 8) + color] : palette[color]) & 0x7FFF;
            }
            else
            {
                int tileOffset = charBase + (entry & 0x3FF) * 0x20 + tileY * 4 + (tileX >> 1);
                color = tileOffset < length ? (vram[tileOffset] >> ((tileX & 1) << 2)) & 0xF : 0;
                if (color == 0)
                {
                    line[x] = Transparent;
                    continue;
                }
                line[x] = palette[color + ((entry >> 12) << 4)] & 0x7FFF;
            }
        }
    }

    private static void RenderAffineLine(int display, int layer, int index, uint displayControl, ushort[] palette, int y, int[] line, bool extendedMode)
    {
        ushort control = Video.GetBgControl(layer);
        byte[] vram = Video.GetBgVram(display);
        int length = vram.Length;
        (MtxFx22 matrix, int originX, int originY) = Video.GetBgAffine(layer);
        int pa = matrix.M00 >> 4;
        int pb = matrix.M01 >> 4;
        int pc = matrix.M10 >> 4;
        int pd = matrix.M11 >> 4;
        if (pa == 0 && pb == 0 && pc == 0 && pd == 0)
        {
            pa = 0x100;
            pd = 0x100;
        }
        bool wrap = (control & 0x2000) != 0;
        int sizeBits = (control >> 14) & 3;
        bool bitmap = extendedMode && (control & 0x80) != 0;
        bool direct = bitmap && (control & 4) != 0;
        int width;
        int height;
        if (bitmap)
        {
            width = sizeBits == 0 ? 128 : sizeBits == 1 ? 256 : 512;
            height = sizeBits == 0 ? 128 : sizeBits == 3 ? 512 : 256;
        }
        else
        {
            width = 128 << sizeBits;
            height = width;
        }
        int screenBase = Video.GetBgScreenBase(layer);
        int charBase = Video.GetBgCharBase(layer);
        int bitmapBase = ((control >> 8) & 0x1F) * 0x4000;
        ushort[]? extended = extendedMode && !bitmap ? ExtendedBgPalette(display, index, displayControl, control) : null;
        int startX = originX + pb * y;
        int startY = originY + pd * y;
        for (int x = 0; x < Width; x++)
        {
            int sourceX = (startX + pa * x) >> 8;
            int sourceY = (startY + pc * x) >> 8;
            if (wrap)
            {
                sourceX &= width - 1;
                sourceY &= height - 1;
            }
            else if (sourceX < 0 || sourceX >= width || sourceY < 0 || sourceY >= height)
            {
                line[x] = Transparent;
                continue;
            }
            if (bitmap)
            {
                if (direct)
                {
                    int address = bitmapBase + (sourceY * width + sourceX) * 2;
                    int value = address + 1 < length ? vram[address] | (vram[address + 1] << 8) : 0;
                    line[x] = (value & 0x8000) != 0 ? value & 0x7FFF : Transparent;
                }
                else
                {
                    int address = bitmapBase + sourceY * width + sourceX;
                    int value = address < length ? vram[address] : 0;
                    line[x] = value != 0 ? palette[value] & 0x7FFF : Transparent;
                }
                continue;
            }
            int tilesPerRow = width >> 3;
            int color;
            if (extendedMode)
            {
                int entryOffset = screenBase + ((sourceY >> 3) * tilesPerRow + (sourceX >> 3)) * 2;
                if (entryOffset + 1 >= length)
                {
                    line[x] = Transparent;
                    continue;
                }
                int entry = vram[entryOffset] | (vram[entryOffset + 1] << 8);
                int tileX = sourceX & 7;
                int tileY = sourceY & 7;
                if ((entry & 0x400) != 0)
                {
                    tileX = 7 - tileX;
                }
                if ((entry & 0x800) != 0)
                {
                    tileY = 7 - tileY;
                }
                int tileOffset = charBase + (entry & 0x3FF) * 0x40 + tileY * 8 + tileX;
                color = tileOffset < length ? vram[tileOffset] : 0;
                if (color == 0)
                {
                    line[x] = Transparent;
                    continue;
                }
                line[x] = (extended != null ? extended[((entry >> 12) << 8) + color] : palette[color]) & 0x7FFF;
            }
            else
            {
                int entryOffset = screenBase + (sourceY >> 3) * tilesPerRow + (sourceX >> 3);
                if (entryOffset >= length)
                {
                    line[x] = Transparent;
                    continue;
                }
                int tileOffset = charBase + vram[entryOffset] * 0x40 + (sourceY & 7) * 8 + (sourceX & 7);
                color = tileOffset < length ? vram[tileOffset] : 0;
                line[x] = color != 0 ? palette[color] & 0x7FFF : Transparent;
            }
        }
    }

    private static void RenderObjectLine(int display, uint control, int y)
    {
        Array.Fill(objLine, Transparent);
        Array.Fill(objWindow, false);
        OamAttributes[] oam = OamManager.GetScreenOam(display);
        OamAffine[] affine = OamManager.GetScreenAffine(display);
        byte[] tiles = Video.GetObjectTiles(display);
        ushort[] palette = Video.GetPalette(display == 0 ? Video.MainObject : Video.SubObject);
        ushort[]? extended = (control & 0x80000000) != 0 ? Video.GetPalette(display == 0 ? Video.MainObjectExtended : Video.SubObjectExtended) : null;
        bool mapping1D = (control & 0x10) != 0;
        int boundaryShift = mapping1D ? (int)((control >> 20) & 3) : 0;
        for (int index = OamManager.Entries - 1; index >= 0; index--)
        {
            OamAttributes entry = oam[index];
            bool isAffine = (entry.Attr0 & 0x100) != 0;
            bool doubleSize = isAffine && (entry.Attr0 & 0x200) != 0;
            if (!isAffine && (entry.Attr0 & 0x200) != 0)
            {
                continue;
            }
            int objectMode = (entry.Attr0 >> 10) & 3;
            if (objectMode == ObjectBitmap)
            {
                continue;
            }
            int shape = (entry.Attr0 >> 14) & 3;
            int size = (entry.Attr1 >> 14) & 3;
            if (shape == 3)
            {
                continue;
            }
            int width = objectWidths[shape * 4 + size];
            int height = objectHeights[shape * 4 + size];
            int boundsWidth = doubleSize ? width * 2 : width;
            int boundsHeight = doubleSize ? height * 2 : height;
            int top = entry.Attr0 & 0xFF;
            if (top + boundsHeight > 256)
            {
                top -= 256;
            }
            int by = y - top;
            if (by < 0 || by >= boundsHeight)
            {
                continue;
            }
            int left = entry.Attr1 & 0x1FF;
            if (left >= 256)
            {
                left -= 512;
            }
            bool colors256 = (entry.Attr0 & 0x2000) != 0;
            bool flipH = !isAffine && (entry.Attr1 & 0x1000) != 0;
            bool flipV = !isAffine && (entry.Attr1 & 0x2000) != 0;
            int tileNumber = entry.Attr2 & 0x3FF;
            int paletteNumber = (entry.Attr2 >> 12) & 0xF;
            byte priority = (byte)((entry.Attr2 >> 10) & 3);
            int pa = 0x100;
            int pb = 0;
            int pc = 0;
            int pd = 0x100;
            if (isAffine)
            {
                OamAffine matrix = affine[(entry.Attr1 >> 9) & 0x1F];
                pa = matrix.PA;
                pb = matrix.PB;
                pc = matrix.PC;
                pd = matrix.PD;
            }
            for (int bx = 0; bx < boundsWidth; bx++)
            {
                int screenX = left + bx;
                if (screenX < 0 || screenX >= Width)
                {
                    continue;
                }
                int sourceX;
                int sourceY;
                if (isAffine)
                {
                    int dx = bx - boundsWidth / 2;
                    int dy = by - boundsHeight / 2;
                    sourceX = ((pa * dx + pb * dy) >> 8) + width / 2;
                    sourceY = ((pc * dx + pd * dy) >> 8) + height / 2;
                    if (sourceX < 0 || sourceX >= width || sourceY < 0 || sourceY >= height)
                    {
                        continue;
                    }
                }
                else
                {
                    sourceX = flipH ? width - 1 - bx : bx;
                    sourceY = flipV ? height - 1 - by : by;
                }
                int tileX = sourceX >> 3;
                int tileY = sourceY >> 3;
                int color;
                if (colors256)
                {
                    int tileOffset = mapping1D
                        ? tileNumber * (32 << boundaryShift) + (tileY * (width >> 3) + tileX) * 64
                        : (tileNumber + tileY * 32 + tileX * 2) * 32;
                    int address = tileOffset + (sourceY & 7) * 8 + (sourceX & 7);
                    color = address < tiles.Length ? tiles[address] : 0;
                }
                else
                {
                    int tileOffset = mapping1D
                        ? tileNumber * (32 << boundaryShift) + (tileY * (width >> 3) + tileX) * 32
                        : (tileNumber + tileY * 32 + tileX) * 32;
                    int address = tileOffset + (sourceY & 7) * 4 + ((sourceX & 7) >> 1);
                    color = address < tiles.Length ? (tiles[address] >> ((sourceX & 1) * 4)) & 0xF : 0;
                }
                if (color == 0)
                {
                    continue;
                }
                if (objectMode == ObjectWindow)
                {
                    objWindow[screenX] = true;
                    continue;
                }
                if (objLine[screenX] != Transparent && priority > objPriority[screenX])
                {
                    continue;
                }
                if (colors256)
                {
                    objLine[screenX] = (extended != null ? extended[(paletteNumber << 8) + color] : palette[color]) & 0x7FFF;
                }
                else
                {
                    objLine[screenX] = palette[paletteNumber * 16 + color] & 0x7FFF;
                }
                objPriority[screenX] = priority;
                objSemi[screenX] = objectMode == ObjectSemiTransparent;
            }
        }
    }

    public static void ApplyMasterBrightness(int display, int[] pixels)
    {
        int brightness = Video.GetMasterBrightness(display);
        if (brightness == 0)
        {
            return;
        }
        int amount = Math.Abs(brightness);
        for (int i = 0; i < pixels.Length; i++)
        {
            int argb = pixels[i];
            int r = (argb >> 16) & 0xFF;
            int g = (argb >> 8) & 0xFF;
            int b = argb & 0xFF;
            if (brightness > 0)
            {
                r += (255 - r) * amount / 16;
                g += (255 - g) * amount / 16;
                b += (255 - b) * amount / 16;
            }
            else
            {
                r -= r * amount / 16;
                g -= g * amount / 16;
                b -= b * amount / 16;
            }
            pixels[i] = unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
        }
    }
}
