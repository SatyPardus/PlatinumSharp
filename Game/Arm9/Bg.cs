using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class GraphicsModes
{
    public int DisplayMode;
    public int MainBgMode;
    public int SubBgMode;
    public int Bg0As2DOr3D;
}

public sealed class BgTemplate
{
    public uint X;
    public uint Y;
    public uint BufferSize;
    public uint BaseTile;
    public byte ScreenSize;
    public byte ColorMode;
    public byte ScreenBase;
    public byte CharBase;
    public byte BgExtPltt;
    public byte Priority;
    public byte AreaOver;
    public int Mosaic;
}

public sealed class Background
{
    public byte[]? TilemapBuffer;
    public uint BufferSize;
    public uint BaseTile;
    public int XOffset;
    public int YOffset;
    public byte Type;
    public byte ScreenSize;
    public byte ColorMode;
    public byte TileSize;
    public ushort Rotation;
    public int XScale;
    public int YScale;
    public int XCenter;
    public int YCenter;
}

public sealed class BgConfig
{
    public int HeapID;
    public ushort ScrollScheduled;
    public ushort BufferTransferScheduled;
    public readonly Background[] Bgs = { new(), new(), new(), new(), new(), new(), new(), new() };
}

public static partial class Bg
{
    public const int LayerMain0 = 0;
    public const int LayerMain1 = 1;
    public const int LayerMain2 = 2;
    public const int LayerMain3 = 3;
    public const int LayerSub0 = 4;
    public const int LayerSub1 = 5;
    public const int LayerSub2 = 6;
    public const int LayerSub3 = 7;
    public const int LayerCount = 8;

    public const int TypeStatic = 0;
    public const int TypeAffine = 1;
    public const int Type256x16Pltt = 2;

    public const int ControlColorMode = 0;
    public const int ControlScreenBase = 1;
    public const int ControlCharBase = 2;

    public const int OffsetSetX = 0;
    public const int OffsetAddX = 1;
    public const int OffsetSubX = 2;
    public const int OffsetSetY = 3;
    public const int OffsetAddY = 4;
    public const int OffsetSubY = 5;

    public const int PaletteKeep = 0x10;
    public const int PaletteRaw = 0x11;

    private const int ControlKeepMask = 0x43;
    private const int ControlPriorityMask = 3;
    private const int ControlMosaic = 0x40;
    private const int ControlScreenBaseMask = 0x1F00;
    private const int ControlCharBaseMask = 0x3C;
    private const int ControlCharBaseMask256 = 0x38;
    private const int ControlExtendedBit = 0x2000;
    private const uint ScreenOffsetMask = 0xC7FFFFFF;
    private const uint CharOffsetMask = 0xF8FFFFFF;
    private const int DisplayMain = 0;
    private const int DisplaySub = 1;
    private const int TileSize4bpp = 0x20;
    private const int TileSize8bpp = 0x40;

    private static BgConfig Config(object bgConfig)
    {
        return (BgConfig)bgConfig;
    }

    private static bool IsAffineCapable(int bgLayer)
    {
        return (bgLayer & 3) >= 2;
    }

    private static ushort TilemapU16(byte[] buffer, int index)
    {
        return (ushort)Bin.U16(buffer, index * 2);
    }

    private static void SetTilemapU16(byte[] buffer, int index, ushort value)
    {
        Bin.W16(buffer, index * 2, value);
    }

    // OFFSET 0x02018340
    public static BgConfig BgConfig_New(int heapID)
    {
        BgConfig bgConfig = new();
        bgConfig.HeapID = heapID;
        bgConfig.ScrollScheduled = 0;
        bgConfig.BufferTransferScheduled = 0;
        return bgConfig;
    }

    // OFFSET 0x02018364
    public static int BgConfig_GetHeapID(object bgConfig)
    {
        return Config(bgConfig).HeapID;
    }

    // OFFSET 0x02018368
    public static void SetAllGraphicsModes(GraphicsModes graphicsModes)
    {
        Video.SetGraphicsMode(graphicsModes.DisplayMode, graphicsModes.MainBgMode, graphicsModes.Bg0As2DOr3D);
        Video.SetSubGraphicsMode(graphicsModes.SubBgMode);
        Video.SetDisplayControl(DisplayMain, Video.GetDisplayControl(DisplayMain) & ScreenOffsetMask);
        Video.SetDisplayControl(DisplayMain, Video.GetDisplayControl(DisplayMain) & CharOffsetMask);
        GXLayers.GXLayers_DisableEngineALayers();
        GXLayers.GXLayers_DisableEngineBLayers();
    }

    // OFFSET 0x020183A0
    public static void SetScreenGraphicsModes(GraphicsModes graphicsModes, byte screen)
    {
        if (screen == DisplayMain)
        {
            Video.SetGraphicsMode(graphicsModes.DisplayMode, graphicsModes.MainBgMode, graphicsModes.Bg0As2DOr3D);
            GXLayers.GXLayers_DisableEngineALayers();
        }
        else
        {
            Video.SetSubGraphicsMode(graphicsModes.SubBgMode);
            GXLayers.GXLayers_DisableEngineBLayers();
        }
    }

    private static ushort BuildControl(int bgLayer, int bgType, ushort previous, int screenSize, int colorMode, int screenBase, int charBase, int extended)
    {
        int control = (previous & ControlKeepMask) | (screenSize << 14) | (screenBase << 8) | (charBase << 2);
        if (!IsAffineCapable(bgLayer))
        {
            control |= (colorMode << 7) | (extended << 13);
        }
        else if (bgType == TypeStatic)
        {
            control |= colorMode << 7;
        }
        else
        {
            control |= extended << 13;
        }
        return (ushort)control;
    }

    // OFFSET 0x020183C4
    public static void Bg_InitFromTemplate(object bgConfig, int bgLayer, BgTemplate bgTemplate, byte bgType)
    {
        BgConfig config = Config(bgConfig);
        byte screenSize = Bg_ConvertScreenSize(bgTemplate.ScreenSize, bgType);
        if (bgLayer < LayerCount)
        {
            if (bgLayer < LayerSub0)
            {
                GXLayers.GXLayers_EngineAToggleLayers(1 << bgLayer, 1);
            }
            else
            {
                GXLayers.GXLayers_EngineBToggleLayers(1 << (bgLayer - LayerSub0), 1);
            }
            int extended = IsAffineCapable(bgLayer) ? bgTemplate.AreaOver : bgTemplate.BgExtPltt;
            ushort control = BuildControl(bgLayer, bgType, Video.GetBgControl(bgLayer), screenSize, bgTemplate.ColorMode, bgTemplate.ScreenBase, bgTemplate.CharBase, extended);
            control = (ushort)((control & ~ControlPriorityMask) | bgTemplate.Priority);
            if (bgTemplate.Mosaic != 0)
            {
                control |= ControlMosaic;
            }
            else
            {
                control = (ushort)(control & ~ControlMosaic);
            }
            Video.SetBgControl(bgLayer, control);
        }
        Background bg = config.Bgs[bgLayer];
        bg.Rotation = 0;
        bg.XScale = Fx32.One;
        bg.YScale = Fx32.One;
        bg.XCenter = 0;
        bg.YCenter = 0;
        if (bgTemplate.BufferSize != 0)
        {
            bg.TilemapBuffer = new byte[bgTemplate.BufferSize];
            bg.BufferSize = bgTemplate.BufferSize;
            bg.BaseTile = bgTemplate.BaseTile;
        }
        else
        {
            bg.TilemapBuffer = null;
            bg.BufferSize = 0;
            bg.BaseTile = 0;
        }
        bg.ScreenSize = bgTemplate.ScreenSize;
        bg.Type = bgType;
        bg.ColorMode = bgTemplate.ColorMode;
        if (bgType == TypeStatic && bgTemplate.ColorMode == 0)
        {
            bg.TileSize = TileSize4bpp;
        }
        else
        {
            bg.TileSize = TileSize8bpp;
        }
        Bg_SetOffset(bgConfig, bgLayer, OffsetSetX, (int)bgTemplate.X);
        Bg_SetOffset(bgConfig, bgLayer, OffsetSetY, (int)bgTemplate.Y);
    }

    // OFFSET 0x02018898
    public static void Bg_SetControlParam(object bgConfig, int bgLayer, int bgControlParam, byte value)
    {
        BgConfig config = Config(bgConfig);
        if (bgControlParam == ControlColorMode)
        {
            config.Bgs[bgLayer].ColorMode = value;
        }
        if (bgLayer >= LayerCount)
        {
            return;
        }
        Background bg = config.Bgs[bgLayer];
        int control = Video.GetBgControl(bgLayer);
        if (bgControlParam == ControlScreenBase)
        {
            control = (control & ~ControlScreenBaseMask) | ((value << 8) & ControlScreenBaseMask);
        }
        else if (bgControlParam == ControlCharBase)
        {
            int mask = IsAffineCapable(bgLayer) && bg.Type == Type256x16Pltt ? ControlCharBaseMask256 : ControlCharBaseMask;
            control = (control & ~mask) | ((value << 2) & mask);
        }
        int screenSize = (control >> 14) & 3;
        int screenBase = (control >> 8) & 0x1F;
        int extended = (control >> 13) & 1;
        int charBase = IsAffineCapable(bgLayer) && bg.Type == Type256x16Pltt ? ((control >> 3) & 7) << 1 : (control >> 2) & 0xF;
        Video.SetBgControl(bgLayer, BuildControl(bgLayer, bg.Type, Video.GetBgControl(bgLayer), screenSize, bg.ColorMode, screenBase, charBase, extended));
    }

    // OFFSET 0x02018F80
    private static byte Bg_ConvertScreenSize(byte screenSize, byte bgType)
    {
        switch (bgType)
        {
            case TypeStatic:
                if (screenSize == 1)
                {
                    return 0;
                }
                if (screenSize == 2)
                {
                    return 2;
                }
                if (screenSize == 3)
                {
                    return 1;
                }
                if (screenSize == 4)
                {
                    return 3;
                }
                break;
            case TypeAffine:
            case Type256x16Pltt:
                if (screenSize == 0)
                {
                    return 0;
                }
                if (screenSize == 1)
                {
                    return 1;
                }
                if (screenSize == 4)
                {
                    return 2;
                }
                if (screenSize == 5)
                {
                    return 3;
                }
                break;
        }
        return 0;
    }

    // OFFSET 0x02018FF4
    private static void Bg_GetScreenTileDimensions(byte screenSize, out byte width, out byte height)
    {
        width = 0;
        height = 0;
        switch (screenSize)
        {
            case 0:
                width = 0x10;
                height = 0x10;
                break;
            case 1:
                width = 0x20;
                height = 0x20;
                break;
            case 2:
                width = 0x20;
                height = 0x40;
                break;
            case 3:
                width = 0x40;
                height = 0x20;
                break;
            case 4:
                width = 0x40;
                height = 0x40;
                break;
            case 5:
                width = 0x80;
                height = 0x80;
                break;
        }
    }

    // OFFSET 0x02019044
    public static void Bg_FreeTilemapBuffer(object bgConfig, int bgLayer)
    {
        Background bg = Config(bgConfig).Bgs[bgLayer];
        if (bg.TilemapBuffer != null)
        {
            bg.TilemapBuffer = null;
        }
    }

    // OFFSET 0x02019060
    public static void Bg_SetPriority(byte bgLayer, byte priority)
    {
        if (bgLayer < LayerCount)
        {
            Video.SetBgControl(bgLayer, (ushort)((Video.GetBgControl(bgLayer) & ~ControlPriorityMask) | priority));
        }
    }

    // OFFSET 0x02019120
    public static void Bg_ToggleLayer(byte bgLayer, byte enable)
    {
        if (bgLayer < LayerSub0)
        {
            GXLayers.GXLayers_EngineAToggleLayers(1 << bgLayer, enable);
        }
        else if (bgLayer < LayerCount)
        {
            GXLayers.GXLayers_EngineBToggleLayers(1 << (bgLayer - LayerSub0), enable);
        }
    }

    // OFFSET 0x02019184
    public static void Bg_SetOffset(object bgConfig, int bgLayer, byte bgOffsetUpdateOp, int value)
    {
        BgConfig config = Config(bgConfig);
        Background bg = config.Bgs[bgLayer];
        Bg_UpdateOffset(bg, bgOffsetUpdateOp, value);
        int x = bg.XOffset;
        int y = bg.YOffset;
        if (bgLayer >= LayerCount)
        {
            return;
        }
        if (IsAffineCapable(bgLayer) && bg.Type != TypeStatic)
        {
            Bg_ResetAffine(config, bgLayer);
            return;
        }
        Video.SetBgOffset(bgLayer, (uint)((x & 0x1FF) | ((y << 16) & 0x1FF0000)));
    }

    // OFFSET 0x020192EC
    public static int Bg_GetXOffset(object bgConfig, int bgLayer)
    {
        return Config(bgConfig).Bgs[bgLayer].XOffset;
    }

    // OFFSET 0x020192F8
    public static int Bg_GetYOffset(object bgConfig, int bgLayer)
    {
        return Config(bgConfig).Bgs[bgLayer].YOffset;
    }

    // OFFSET 0x02019304
    private static void Bg_UpdateOffset(Background bg, byte bgOffsetUpdateOp, int value)
    {
        switch (bgOffsetUpdateOp)
        {
            case OffsetSetX:
                bg.XOffset = value;
                break;
            case OffsetAddX:
                bg.XOffset += value;
                break;
            case OffsetSubX:
                bg.XOffset -= value;
                break;
            case OffsetSetY:
                bg.YOffset = value;
                break;
            case OffsetAddY:
                bg.YOffset += value;
                break;
            case OffsetSubY:
                bg.YOffset -= value;
                break;
        }
    }

    // OFFSET 0x02019348
    public static void Bg_SetAffineParams(object bgConfig, int bgLayer, in MtxFx22 matrix, int centerX, int centerY)
    {
        if (bgLayer < LayerCount && IsAffineCapable(bgLayer))
        {
            Background bg = Config(bgConfig).Bgs[bgLayer];
            Video.SetBgAffine(bgLayer, matrix, centerX, centerY, bg.XOffset, bg.YOffset);
        }
    }

    // OFFSET 0x020193E4
    private static void Bg_ResetAffine(BgConfig bgConfig, int bgLayer)
    {
        MathUtil.CreateAffineTransformationMatrix(out MtxFx22 matrix, 0, Fx32.One, Fx32.One, 0);
        Bg_SetAffineParams(bgConfig, bgLayer, matrix, 0, 0);
    }

    // OFFSET 0x02019410
    private static void Bg_CopyOrUncompressTilemapData(byte[] source, byte[] destination, uint size)
    {
        if (size == 0)
        {
            byte[]? data = Lz.TryDecompress(source);
            if (data != null)
            {
                Array.Copy(data, destination, Math.Min(data.Length, destination.Length));
            }
            return;
        }
        Array.Copy(source, destination, Math.Min((int)size, Math.Min(source.Length, destination.Length)));
    }

    // OFFSET 0x02019448
    public static void Bg_CopyTilemapBufferToVRAM(object bgConfig, int bgLayer)
    {
        Background bg = Config(bgConfig).Bgs[bgLayer];
        Bg_CopyTilemapBufferRangeToVRAM(bgConfig, bgLayer, bg.TilemapBuffer!, bg.BufferSize, bg.BaseTile);
    }

    // OFFSET 0x02019460
    public static void Bg_CopyTilemapBufferRangeToVRAM(object bgConfig, int bgLayer, byte[] source, uint size, uint offset)
    {
        if (size == 0)
        {
            Background bg = Config(bgConfig).Bgs[bgLayer];
            byte[]? tilemapBuffer = bg.TilemapBuffer;
            if (tilemapBuffer != null)
            {
                Bg_CopyOrUncompressTilemapData(source, tilemapBuffer, size);
                Bg_LoadScreenToVRAM(bgLayer, tilemapBuffer, bg.BaseTile * 2, bg.BufferSize);
                return;
            }
            uint uncompressedSize = Bin.U32(source, 0) >> 8;
            byte[] temporary = new byte[uncompressedSize];
            Bg_CopyOrUncompressTilemapData(source, temporary, size);
            Bg_LoadScreenToVRAM(bgLayer, temporary, offset * 2, uncompressedSize);
            return;
        }
        Bg_LoadScreenToVRAM(bgLayer, source, offset * 2, size);
    }

    // OFFSET 0x020194E0
    private static void Bg_LoadScreenToVRAM(int bgLayer, byte[] source, uint offset, uint size)
    {
        if (bgLayer < LayerCount)
        {
            Video.LoadBgScreen(bgLayer, source, 0, offset, size);
        }
    }

    // OFFSET 0x02019574
    public static void Bg_LoadTilemapBuffer(object bgConfig, int bgLayer, byte[] source, uint size)
    {
        Bg_CopyOrUncompressTilemapData(source, Config(bgConfig).Bgs[bgLayer].TilemapBuffer!, size);
    }

    // OFFSET 0x0201958C
    public static void Bg_LoadTiles(object bgConfig, int bgLayer, byte[] source, uint size, uint tileStart)
    {
        if (Config(bgConfig).Bgs[bgLayer].ColorMode == 0)
        {
            Bg_LoadTilesToVRAM(bgConfig, bgLayer, source, size, tileStart << 5);
        }
        else
        {
            Bg_LoadTilesToVRAM(bgConfig, bgLayer, source, size, tileStart << 6);
        }
    }

    // OFFSET 0x020195B8
    public static void Bg_LoadTilesToVRAM(object bgConfig, int bgLayer, byte[] source, uint size, uint offset)
    {
        if (size == 0)
        {
            uint uncompressedSize = Bin.U32(source, 0) >> 8;
            byte[] temporary = new byte[uncompressedSize];
            Bg_CopyOrUncompressTilemapData(source, temporary, size);
            Bg_LoadCharToVRAM(bgLayer, temporary, offset, uncompressedSize);
            return;
        }
        Bg_LoadCharToVRAM(bgLayer, source, offset, size);
    }

    // OFFSET 0x020195FC
    private static void Bg_LoadCharToVRAM(int bgLayer, byte[] source, uint offset, uint size)
    {
        if (bgLayer < LayerCount)
        {
            Video.LoadBgChar(bgLayer, source, 0, offset, size);
        }
    }

    // OFFSET 0x02019690
    public static void Bg_ClearTilesRange(byte bgLayer, uint size, uint offset, int heapID)
    {
        byte[] temporary = new byte[size];
        Bg_LoadCharToVRAM(bgLayer, temporary, offset, size);
    }

    // OFFSET 0x020196C0
    public static void Bg_FillTilesRange(object bgConfig, uint bgLayer, uint fillValue, uint tileCount, uint offset)
    {
        Background bg = Config(bgConfig).Bgs[bgLayer];
        uint size = tileCount * bg.TileSize;
        byte[] temporary = new byte[size];
        if (bg.TileSize == TileSize4bpp)
        {
            fillValue = (fillValue << 12) | (fillValue << 8) | (fillValue << 4) | fillValue;
            fillValue |= fillValue << 16;
        }
        else
        {
            fillValue = (fillValue << 24) | (fillValue << 16) | (fillValue << 8) | fillValue;
        }
        for (int i = 0; i + 4 <= size; i += 4)
        {
            BitConverter.TryWriteBytes(temporary.AsSpan(i), fillValue);
        }
        Bg_LoadCharToVRAM((int)bgLayer, temporary, bg.TileSize * offset, size);
    }

    // OFFSET 0x0201972C
    public static void Bg_LoadPalette(int bgLayer, ushort[] source, int sourceIndex, uint size, ushort offset)
    {
        if ((uint)bgLayer < LayerSub0)
        {
            Video.LoadPaletteRange(Video.MainBackground, source, sourceIndex, offset, size);
        }
        else
        {
            Video.LoadPaletteRange(Video.SubBackground, source, sourceIndex, offset, size);
        }
    }

    // OFFSET 0x0201975C
    public static void Bg_MaskPalette(byte bgLayer, ushort mask)
    {
        Bg_LoadPalette(bgLayer, new[] { mask }, 0, 2, 0);
    }

    // OFFSET 0x02019774
    private static ushort Bg_GetTileIndex(byte x, byte y, byte screenSize)
    {
        switch (screenSize)
        {
            case 0:
                return (ushort)(x + (y << 4));
            case 1:
            case 2:
                return (ushort)(x + (y << 5));
            case 3:
                return (ushort)((x & 0x1F) + ((y + (x & ~0x1F)) << 5));
            case 4:
            {
                ushort block = (ushort)((x >> 5) + ((y >> 5) << 1));
                return (ushort)((ushort)(block << 10) + (x & 0x1F) + ((y & 0x1F) << 5));
            }
        }
        return 0;
    }

    // OFFSET 0x020197E0
    private static ushort Bg_GetSourceTileIndex(byte x, byte y, byte width, byte height)
    {
        short wideColumns = (short)(width - 0x20);
        short tallRows = (short)(height - 0x20);
        byte quadrant = 0;
        ushort index = 0;
        if ((x >> 5) != 0)
        {
            quadrant++;
        }
        if ((y >> 5) != 0)
        {
            quadrant += 2;
        }
        switch (quadrant)
        {
            case 0:
                if (wideColumns >= 0)
                {
                    index = (ushort)(index + x + (y << 5));
                }
                else
                {
                    index = (ushort)(index + x + y * width);
                }
                break;
            case 1:
                if (tallRows >= 0)
                {
                    index = (ushort)(index + 0x400);
                }
                else
                {
                    index = (ushort)(index + (height << 5));
                }
                index = (ushort)(index + (x & 0x1F) + y * wideColumns);
                break;
            case 2:
                index = (ushort)(index + (width << 5));
                if (wideColumns >= 0)
                {
                    index = (ushort)(index + x + ((y & 0x1F) << 5));
                }
                else
                {
                    index = (ushort)(index + x + (y & 0x1F) * width);
                }
                break;
            case 3:
                index = (ushort)(index + ((width + tallRows) << 5));
                index = (ushort)(index + (x & 0x1F) + (y & 0x1F) * wideColumns);
                break;
        }
        return index;
    }

    // OFFSET 0x020198C0
    public static void Bg_LoadToTilemapRect(object bgConfig, int bgLayer, byte[] source, byte destX, byte destY, byte destWidth, byte destHeight)
    {
        Bg_CopyToTilemapRect(bgConfig, bgLayer, destX, destY, destWidth, destHeight, source, 0, 0, destWidth, destHeight);
    }

    // OFFSET 0x020198E8
    public static void Bg_CopyToTilemapRect(object bgConfig, int bgLayer, int destX, int destY, int destWidth, int destHeight, byte[] source, int sourceX, int sourceY, int sourceWidth, int sourceHeight)
    {
        Background bg = Config(bgConfig).Bgs[bgLayer];
        if (bg.Type != TypeAffine)
        {
            Bg_CopyToTextTilemapRect(bg, (byte)destX, (byte)destY, (byte)destWidth, (byte)destHeight, source, (byte)sourceX, (byte)sourceY, (byte)sourceWidth, (byte)sourceHeight, 0);
        }
        else
        {
            Bg_CopyToAffineTilemapRect(bg, (byte)destX, (byte)destY, (byte)destWidth, (byte)destHeight, source, (byte)sourceX, (byte)sourceY, (byte)sourceWidth, (byte)sourceHeight, 0);
        }
    }

    // OFFSET 0x02019964
    public static void Bg_CopyRectToTilemapRect(object bgConfig, int bgLayer, int destX, int destY, int destWidth, int destHeight, byte[] source, int sourceX, int sourceY, int sourceWidth, int sourceHeight)
    {
        Background bg = Config(bgConfig).Bgs[bgLayer];
        if (bg.Type != TypeAffine)
        {
            Bg_CopyToTextTilemapRect(bg, (byte)destX, (byte)destY, (byte)destWidth, (byte)destHeight, source, (byte)sourceX, (byte)sourceY, (byte)sourceWidth, (byte)sourceHeight, 1);
        }
        else
        {
            Bg_CopyToAffineTilemapRect(bg, (byte)destX, (byte)destY, (byte)destWidth, (byte)destHeight, source, (byte)sourceX, (byte)sourceY, (byte)sourceWidth, (byte)sourceHeight, 1);
        }
    }

    // OFFSET 0x020199E0
    private static void Bg_CopyToTextTilemapRect(Background bg, byte destX, byte destY, byte destWidth, byte destHeight, byte[] source, byte sourceX, byte sourceY, byte sourceWidth, byte sourceHeight, byte mode)
    {
        byte[]? tilemap = bg.TilemapBuffer;
        if (tilemap == null)
        {
            return;
        }
        Bg_GetScreenTileDimensions(bg.ScreenSize, out byte screenWidth, out byte screenHeight);
        for (byte row = 0; row < destHeight; row++)
        {
            if (destY + row >= screenHeight || sourceY + row >= sourceHeight)
            {
                break;
            }
            for (byte column = 0; column < destWidth; column++)
            {
                if (destX + column >= screenWidth || sourceX + column >= sourceWidth)
                {
                    break;
                }
                ushort destIndex = Bg_GetTileIndex((byte)(destX + column), (byte)(destY + row), bg.ScreenSize);
                int sourceIndex;
                if (mode == 0)
                {
                    sourceIndex = (sourceY + row) * sourceWidth + sourceX + column;
                }
                else
                {
                    sourceIndex = Bg_GetSourceTileIndex((byte)(sourceX + column), (byte)(sourceY + row), sourceWidth, sourceHeight);
                }
                SetTilemapU16(tilemap, destIndex, TilemapU16(source, sourceIndex));
            }
        }
    }

    // OFFSET 0x02019B54
    private static void Bg_CopyToAffineTilemapRect(Background bg, byte destX, byte destY, byte destWidth, byte destHeight, byte[] source, byte sourceX, byte sourceY, byte sourceWidth, byte sourceHeight, byte mode)
    {
        byte[]? tilemap = bg.TilemapBuffer;
        if (tilemap == null)
        {
            return;
        }
        Bg_GetScreenTileDimensions(bg.ScreenSize, out byte screenWidth, out byte screenHeight);
        for (byte row = 0; row < destHeight; row++)
        {
            if (destY + row >= screenHeight || sourceY + row >= sourceHeight)
            {
                break;
            }
            for (byte column = 0; column < destWidth; column++)
            {
                if (destX + column >= screenWidth || sourceX + column >= sourceWidth)
                {
                    break;
                }
                ushort destIndex = Bg_GetTileIndex((byte)(destX + column), (byte)(destY + row), bg.ScreenSize);
                int sourceIndex;
                if (mode == 0)
                {
                    sourceIndex = (sourceY + row) * sourceWidth + sourceX + column;
                }
                else
                {
                    sourceIndex = Bg_GetSourceTileIndex((byte)(sourceX + column), (byte)(sourceY + row), sourceWidth, sourceHeight);
                }
                tilemap[destIndex] = source[sourceIndex];
            }
        }
    }

    // OFFSET 0x02019CB8
    public static void Bg_FillTilemapRect(object bgConfig, int bgLayer, ushort fillValue, int x, int y, int width, int height, int palette)
    {
        Background bg = Config(bgConfig).Bgs[bgLayer];
        if (bg.Type != TypeAffine)
        {
            Bg_FillTextTilemapRect(bg, fillValue, (byte)x, (byte)y, (byte)width, (byte)height, (byte)palette);
        }
        else
        {
            Bg_FillAffineTilemapRect(bg, (byte)fillValue, (byte)x, (byte)y, (byte)width, (byte)height);
        }
    }

    // OFFSET 0x02019D08
    private static void Bg_FillTextTilemapRect(Background bg, ushort fillValue, byte x, byte y, byte width, byte height, byte palette)
    {
        byte[]? tilemap = bg.TilemapBuffer;
        if (tilemap == null)
        {
            return;
        }
        Bg_GetScreenTileDimensions(bg.ScreenSize, out byte screenWidth, out byte screenHeight);
        for (byte row = y; row < y + height; row++)
        {
            if (row >= screenHeight)
            {
                break;
            }
            for (byte column = x; column < x + width; column++)
            {
                if (column >= screenWidth)
                {
                    break;
                }
                ushort index = Bg_GetTileIndex(column, row, bg.ScreenSize);
                if (palette == PaletteRaw)
                {
                    SetTilemapU16(tilemap, index, fillValue);
                }
                else if (palette == PaletteKeep)
                {
                    SetTilemapU16(tilemap, index, (ushort)(fillValue + (TilemapU16(tilemap, index) & 0xF000)));
                }
                else
                {
                    SetTilemapU16(tilemap, index, (ushort)((palette << 12) + fillValue));
                }
            }
        }
    }

    // OFFSET 0x02019DB8
    private static void Bg_FillAffineTilemapRect(Background bg, byte fillValue, byte x, byte y, byte width, byte height)
    {
        byte[]? tilemap = bg.TilemapBuffer;
        if (tilemap == null)
        {
            return;
        }
        Bg_GetScreenTileDimensions(bg.ScreenSize, out byte screenWidth, out byte screenHeight);
        for (byte row = y; row < y + height; row++)
        {
            if (row >= screenHeight)
            {
                break;
            }
            for (byte column = x; column < x + width; column++)
            {
                if (column >= screenWidth)
                {
                    break;
                }
                tilemap[Bg_GetTileIndex(column, row, bg.ScreenSize)] = fillValue;
            }
        }
    }

    // OFFSET 0x02019E2C
    public static void Bg_ChangeTilemapRectPalette(object bgConfig, int bgLayer, byte x, byte y, byte width, byte height, byte palette)
    {
        Background bg = Config(bgConfig).Bgs[bgLayer];
        byte[]? tilemap = bg.TilemapBuffer;
        if (tilemap == null)
        {
            return;
        }
        Bg_GetScreenTileDimensions(bg.ScreenSize, out byte screenWidth, out byte screenHeight);
        for (byte row = y; row < y + height; row++)
        {
            if (row >= screenHeight)
            {
                break;
            }
            for (byte column = x; column < x + width; column++)
            {
                if (column >= screenWidth)
                {
                    break;
                }
                ushort index = Bg_GetTileIndex(column, row, bg.ScreenSize);
                SetTilemapU16(tilemap, index, (ushort)((TilemapU16(tilemap, index) & 0x0FFF) | (palette << 12)));
            }
        }
    }

    // OFFSET 0x02019EBC
    public static void Bg_ClearTilemap(object bgConfig, int bgLayer)
    {
        Background bg = Config(bgConfig).Bgs[bgLayer];
        if (bg.TilemapBuffer != null)
        {
            Array.Clear(bg.TilemapBuffer, 0, (int)bg.BufferSize);
            Bg_CopyTilemapBufferToVRAM(bgConfig, bgLayer);
        }
    }

    private static void FillTilemap16(Background bg, ushort fillValue)
    {
        for (int i = 0; i + 2 <= bg.BufferSize; i += 2)
        {
            Bin.W16(bg.TilemapBuffer!, i, fillValue);
        }
    }

    // OFFSET 0x02019EE0
    public static void Bg_FillTilemap(object bgConfig, int bgLayer, ushort fillValue)
    {
        Background bg = Config(bgConfig).Bgs[bgLayer];
        if (bg.TilemapBuffer != null)
        {
            FillTilemap16(bg, fillValue);
            Bg_CopyTilemapBufferToVRAM(bgConfig, bgLayer);
        }
    }

    // OFFSET 0x02019F04
    public static void Bg_ScheduleFillTilemap(object bgConfig, int bgLayer, ushort fillValue)
    {
        Background bg = Config(bgConfig).Bgs[bgLayer];
        if (bg.TilemapBuffer != null)
        {
            FillTilemap16(bg, fillValue);
            Bg_ScheduleTilemapTransfer(bgConfig, bgLayer);
        }
    }

    // OFFSET 0x02019F28
    public static byte[] Bg_GetCharPtr(int bgLayer)
    {
        if ((uint)bgLayer >= LayerCount)
        {
            return Array.Empty<byte>();
        }
        byte[] vram = Video.GetBgVram(bgLayer >> 2);
        int start = Video.GetBgCharBase(bgLayer);
        return vram.AsSpan(start).ToArray();
    }

    // OFFSET 0x02019F80
    public static void Bg_ConvertPixels4bppTo8bpp(byte[] source, uint size, byte[] destination, byte palette)
    {
        byte paletteOffset = (byte)(palette << 4);
        for (uint i = 0; i < size; i++)
        {
            byte low = (byte)(source[i] & 0xF);
            if (low != 0)
            {
                low += paletteOffset;
            }
            destination[i * 2] = low;
            byte high = (byte)((source[i] >> 4) & 0xF);
            if (high != 0)
            {
                high += paletteOffset;
            }
            destination[i * 2 + 1] = high;
        }
    }

    // OFFSET 0x02019FC0
    public static byte[] Bg_AllocPixels8bppFrom4bpp(byte[] source, uint size, byte palette, int heapID)
    {
        byte[] destination = new byte[size * 2];
        Bg_ConvertPixels4bppTo8bpp(source, size, destination, palette);
        return destination;
    }

    // OFFSET 0x02019FE4
    public static byte[]? Bg_GetTilemapBuffer(object bgConfig, int bgLayer)
    {
        return Config(bgConfig).Bgs[bgLayer].TilemapBuffer;
    }

    // OFFSET 0x02019FF0
    public static int Bg_GetXOffset2(object bgConfig, int bgLayer)
    {
        return Config(bgConfig).Bgs[bgLayer].XOffset;
    }

    // OFFSET 0x02019FFC
    public static ushort Bg_GetRotation(object bgConfig, int bgLayer)
    {
        return Config(bgConfig).Bgs[bgLayer].Rotation;
    }

    // OFFSET 0x0201A008
    public static byte Bg_GetPriority(object bgConfig, int bgLayer)
    {
        if (bgLayer < LayerCount)
        {
            return (byte)(Video.GetBgControl(bgLayer) & ControlPriorityMask);
        }
        return 0;
    }
}
