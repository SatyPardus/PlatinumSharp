namespace PlatinumSharp.Game;

public static class Video
{
    public const int MainBackground = 0;
    public const int SubBackground = 1;
    public const int MainObject = 2;
    public const int SubObject = 3;
    public const int MainBackgroundExtended = 4;
    public const int SubBackgroundExtended = 8;
    public const int MainObjectExtended = 12;
    public const int SubObjectExtended = 13;
    public const int PaletteTargets = 14;

    private const int StandardColors = 256;
    private const int ExtendedColors = 4096;

    private static readonly ushort[][] palettes = CreatePalettes();

    public static event Action<int>? PaletteChanged;

    private static ushort[][] CreatePalettes()
    {
        ushort[][] result = new ushort[PaletteTargets][];
        for (int i = 0; i < PaletteTargets; i++)
        {
            result[i] = new ushort[i < MainBackgroundExtended ? StandardColors : ExtendedColors];
        }
        return result;
    }

    public static ushort[] GetPalette(int target)
    {
        return palettes[target];
    }

    private const int ObjectTileBytes = 0x40000;
    private const uint ExtendedSlotBytes = 0x2000;

    private static readonly byte[][] objectTiles = { new byte[ObjectTileBytes], new byte[ObjectTileBytes] };
    private static readonly uint[] displayControl = new uint[2];

    public static event Action<int>? ObjectTilesChanged;

    public static byte[] GetObjectTiles(int display)
    {
        return objectTiles[display];
    }

    public static void LoadObjectTiles(int display, byte[] data, uint offset, uint size)
    {
        byte[] destination = objectTiles[display];
        long count = Math.Min(Math.Min(size, (uint)data.Length), Math.Max(0, destination.Length - (long)offset));
        if (count > 0)
        {
            Array.Copy(data, 0, destination, offset, count);
        }
        ObjectTilesChanged?.Invoke(display);
    }

    private const int TextureBytes = 0x80000;
    private const int TexturePaletteColors = 0x10000;

    private static readonly byte[] textures = new byte[TextureBytes];
    private static readonly ushort[] texturePalettes = new ushort[TexturePaletteColors];

    public static event Action? TexturesChanged;

    public static byte[] Textures => textures;

    public static ushort[] TexturePalettes => texturePalettes;

    public static void LoadTexture(byte[] data, uint address, uint size)
    {
        long count = Math.Min(Math.Min(size, (uint)data.Length), Math.Max(0, textures.Length - (long)address));
        if (count > 0)
        {
            Array.Copy(data, 0, textures, address, count);
        }
        TexturesChanged?.Invoke();
    }

    public static void LoadTexturePalette(ushort[] colors, uint address, uint byteSize)
    {
        long index = address / 2;
        long count = Math.Min(Math.Min(byteSize / 2, (uint)colors.Length), Math.Max(0, texturePalettes.Length - index));
        if (count > 0)
        {
            Array.Copy(colors, 0, texturePalettes, index, count);
        }
        TexturesChanged?.Invoke();
    }

    public static int ObjectBank => (int)banks[4];

    public static int SubObjectBank => (int)banks[6];

    private static readonly int[] visibleWindows = new int[2];

    public static int GetVisibleWindows(int display)
    {
        return visibleWindows[display];
    }

    public static void SetVisibleWindows(int display, int mask)
    {
        visibleWindows[display] = mask & 7;
    }

    public static int VCount { get; set; }

    public static bool InHBlank { get; set; }

    private static readonly byte[,] windowLeft = new byte[2, 2];
    private static readonly byte[,] windowRight = new byte[2, 2];
    private static readonly byte[,] windowTop = new byte[2, 2];
    private static readonly byte[,] windowBottom = new byte[2, 2];

    public static (byte Left, byte Right, byte Top, byte Bottom) GetWindow(int display, int window)
    {
        return (windowLeft[display, window], windowRight[display, window], windowTop[display, window], windowBottom[display, window]);
    }

    private static readonly byte[,] windowInside = new byte[2, 2];
    private static readonly byte[] windowOutside = new byte[2];

    public static byte GetWindowInside(int display, int window)
    {
        return windowInside[display, window];
    }

    public static byte GetWindowOutside(int display)
    {
        return windowOutside[display];
    }

    public static void SetWindowInside(int display, int window, byte mask)
    {
        windowInside[display, window] = (byte)(mask & 0x3F);
    }

    public static void SetWindowOutside(int display, byte mask)
    {
        windowOutside[display] = (byte)(mask & 0x3F);
    }

    private static readonly byte[] objWindowInside = new byte[2];

    private static readonly byte[] objMosaicH = new byte[2];
    private static readonly byte[] objMosaicV = new byte[2];

    public static (byte H, byte V) GetObjMosaicSize(int display)
    {
        return (objMosaicH[display], objMosaicV[display]);
    }

    public static void SetObjMosaicSize(int display, int h, int v)
    {
        objMosaicH[display] = (byte)(h & 0xF);
        objMosaicV[display] = (byte)(v & 0xF);
    }

    public static byte GetObjWindowInside(int display)
    {
        return objWindowInside[display];
    }

    public static void SetObjWindowInside(int display, byte mask)
    {
        objWindowInside[display] = (byte)(mask & 0x3F);
    }

    public static void SetWindowHorizontal(int display, int window, byte left, byte right)
    {
        windowLeft[display, window] = left;
        windowRight[display, window] = right;
    }

    public static void SetWindowVertical(int display, int window, byte top, byte bottom)
    {
        windowTop[display, window] = top;
        windowBottom[display, window] = bottom;
    }

    private static readonly int[] masterBrightness = new int[2];

    public static event Action<int>? MasterBrightnessChanged;

    public static int GetMasterBrightness(int display)
    {
        return masterBrightness[display];
    }

    public static void SetMasterBrightness(int display, int brightness)
    {
        masterBrightness[display] = Math.Clamp(brightness, -16, 16);
        MasterBrightnessChanged?.Invoke(display);
    }

    private static readonly int[] blendPlaneMask = new int[2];
    private static readonly int[] blendBrightness = new int[2];

    public static event Action<int>? BrightnessChanged;

    public static int GetBlendPlaneMask(int display)
    {
        return blendPlaneMask[display];
    }

    public static int GetBlendBrightness(int display)
    {
        return blendBrightness[display];
    }

    public static void SetBlendBrightness(int display, int planeMask, int brightness)
    {
        blendPlaneMask[display] = planeMask;
        blendBrightness[display] = Math.Clamp(brightness, -16, 16);
        blendControl[display] = (ushort)((blendControl[display] & 0x3F00) | (planeMask & 0x3F) | (brightness < 0 ? 0xC0 : 0x80));
        BrightnessChanged?.Invoke(display);
    }

    private static readonly uint[] banks = new uint[10];

    public static IReadOnlyList<uint> Banks => banks;

    public static void ResetBanks()
    {
        Array.Clear(banks);
    }

    public static void SetBanks(params uint[] values)
    {
        Array.Copy(values, banks, Math.Min(values.Length, banks.Length));
        displayControl[0] = (displayControl[0] & 0x3FFFFFFF) | (banks[1] != 0 ? 0x40000000u : 0) | (banks[5] != 0 ? 0x80000000u : 0);
        displayControl[1] = (displayControl[1] & 0x3FFFFFFF) | (banks[3] != 0 ? 0x40000000u : 0) | (banks[7] != 0 ? 0x80000000u : 0);
    }

    public const int BgLayerCount = 8;
    public const int MainBgVramBytes = 0x80000;
    public const int SubBgVramBytes = 0x20000;

    private static readonly ushort[] bgControl = new ushort[BgLayerCount];
    private static readonly uint[] bgOffset = new uint[BgLayerCount];
    private static readonly MtxFx22[] bgAffineMatrix = new MtxFx22[BgLayerCount];
    private static readonly int[] bgAffineX = new int[BgLayerCount];
    private static readonly int[] bgAffineY = new int[BgLayerCount];
    private static readonly byte[][] bgVram = { new byte[MainBgVramBytes], new byte[SubBgVramBytes] };

    public static event Action<int>? BgChanged;

    public static byte[] GetBgVram(int display)
    {
        return bgVram[display];
    }

    public static void ClearBgVram(int display)
    {
        Array.Clear(bgVram[display]);
        BgChanged?.Invoke(display);
    }

    public static void ClearObjectTiles(int display)
    {
        Array.Clear(objectTiles[display]);
        ObjectTilesChanged?.Invoke(display);
    }

    public static ushort GetBgControl(int layer)
    {
        return bgControl[layer];
    }

    public static void SetBgControl(int layer, ushort value)
    {
        bgControl[layer] = value;
        BgChanged?.Invoke(layer);
    }

    public static uint GetBgOffset(int layer)
    {
        return bgOffset[layer];
    }

    public static void SetBgOffset(int layer, uint value)
    {
        bgOffset[layer] = value;
        BgChanged?.Invoke(layer);
    }

    public static (MtxFx22 Matrix, int X, int Y) GetBgAffine(int layer)
    {
        return (bgAffineMatrix[layer], bgAffineX[layer], bgAffineY[layer]);
    }

    public static void SetBgAffine(int layer, in MtxFx22 matrix, int centerX, int centerY, int x, int y)
    {
        int offsetX = x - centerX;
        int offsetY = y - centerY;
        bgAffineMatrix[layer] = matrix;
        bgAffineX[layer] = (centerX << 8) + ((matrix.M00 * offsetX + matrix.M01 * offsetY) >> 4);
        bgAffineY[layer] = (centerY << 8) + ((matrix.M10 * offsetX + matrix.M11 * offsetY) >> 4);
        BgChanged?.Invoke(layer);
    }

    public static void SetGraphicsMode(int displayMode, int bgMode, int bg0Mode)
    {
        uint control = displayControl[0] & 0xFFF0FFF0;
        displayControl[0] = control | ((uint)displayMode << 16) | (uint)bgMode | ((uint)bg0Mode << 3);
    }

    public static void SetSubGraphicsMode(int bgMode)
    {
        displayControl[1] = (displayControl[1] & 0xFFFFFFF8) | (uint)bgMode;
    }

    public static int GetBgScreenBase(int layer)
    {
        int display = layer >> 2;
        int offset = display == 0 ? (int)((displayControl[0] >> 27) & 7) * 0x10000 : 0;
        return offset + ((bgControl[layer] >> 8) & 0x1F) * 0x800;
    }

    public static int GetBgCharBase(int layer)
    {
        int display = layer >> 2;
        int offset = display == 0 ? (int)((displayControl[0] >> 24) & 7) * 0x10000 : 0;
        return offset + ((bgControl[layer] >> 2) & 0xF) * 0x4000;
    }

    private static void LoadBgData(int layer, int baseAddress, byte[] source, int sourceOffset, uint destinationOffset, uint size)
    {
        byte[] vram = bgVram[layer >> 2];
        long start = baseAddress + destinationOffset;
        long count = Math.Min(size, Math.Min(source.Length - sourceOffset, vram.Length - start));
        if (count > 0)
        {
            Array.Copy(source, sourceOffset, vram, start, count);
        }
        BgChanged?.Invoke(layer);
    }

    public static void LoadBgScreen(int layer, byte[] source, int sourceOffset, uint destinationOffset, uint size)
    {
        LoadBgData(layer, GetBgScreenBase(layer), source, sourceOffset, destinationOffset, size);
    }

    public static void LoadBgChar(int layer, byte[] source, int sourceOffset, uint destinationOffset, uint size)
    {
        LoadBgData(layer, GetBgCharBase(layer), source, sourceOffset, destinationOffset, size);
    }

    private static readonly ushort[] blendControl = new ushort[2];

    public static ushort GetBlendControl(int display)
    {
        return blendControl[display];
    }

    public static void SetBlendControl(int display, ushort value)
    {
        blendControl[display] = value;
        if (value == 0)
        {
            blendPlaneMask[display] = 0;
        }
        BrightnessChanged?.Invoke(display);
    }

    private const ushort BlendModeAlpha = 0x40;

    private static readonly ushort[] blendAlpha = new ushort[2];

    public static ushort GetBlendAlpha(int display)
    {
        return blendAlpha[display];
    }

    public static void SetBlendAlpha(int display, int firstPlanes, int secondPlanes, int firstCoefficient, int secondCoefficient)
    {
        blendControl[display] = (ushort)(firstPlanes | BlendModeAlpha | (secondPlanes << 8));
        blendAlpha[display] = (ushort)(firstCoefficient | (secondCoefficient << 8));
        BrightnessChanged?.Invoke(display);
    }

    public static void SetBlendAlphaCoefficients(int display, ushort value)
    {
        blendAlpha[display] = value;
        BrightnessChanged?.Invoke(display);
    }

    private const ushort OffscreenAttr0 = 0x00C0;

    public static ushort PowerControl { get; set; }

    public static bool HBlankInterruptEnabled { get; set; }

    public static bool VBlankInterruptEnabled { get; set; }

    public static void ClearVideoMemory()
    {
        Array.Clear(objectTiles[0]);
        Array.Clear(objectTiles[1]);
        Array.Clear(textures);
        ObjectTilesChanged?.Invoke(0);
        ObjectTilesChanged?.Invoke(1);
        TexturesChanged?.Invoke();
    }

    public static void ResetObjectAttributes(int display)
    {
        OamAttributes[] oam = OamManager.GetScreenOam(display);
        for (int i = 0; i < oam.Length; i++)
        {
            oam[i] = new OamAttributes { Attr0 = OffscreenAttr0 };
        }
    }

    public static void ClearStandardPalettes(int display)
    {
        int background = display == 0 ? MainBackground : SubBackground;
        int objects = display == 0 ? MainObject : SubObject;
        Array.Clear(palettes[background]);
        Array.Clear(palettes[objects]);
        PaletteChanged?.Invoke(background);
        PaletteChanged?.Invoke(objects);
    }

    public static uint GetDisplayControl(int display)
    {
        return displayControl[display];
    }

    public static void SetDisplayControl(int display, uint value)
    {
        displayControl[display] = value;
    }

    public static void LoadPaletteRange(int target, ushort[] colors, int sourceIndex, uint destinationByteOffset, uint byteSize)
    {
        bool slotted = target == MainBackgroundExtended || target == SubBackgroundExtended;
        while (byteSize > 0 && sourceIndex < colors.Length)
        {
            int slot = slotted ? (int)(destinationByteOffset / ExtendedSlotBytes) : 0;
            uint within = slotted ? destinationByteOffset % ExtendedSlotBytes : destinationByteOffset;
            if (slotted && slot > 3)
            {
                break;
            }
            ushort[] destination = palettes[target + slot];
            uint room = (uint)destination.Length * 2 - Math.Min(within, (uint)destination.Length * 2);
            uint chunk = Math.Min(byteSize, room);
            int count = (int)Math.Min(chunk / 2, (uint)(colors.Length - sourceIndex));
            if (count <= 0)
            {
                break;
            }
            Array.Copy(colors, sourceIndex, destination, within / 2, count);
            PaletteChanged?.Invoke(target + slot);
            sourceIndex += count;
            destinationByteOffset += (uint)count * 2;
            byteSize -= (uint)count * 2;
            if (!slotted)
            {
                break;
            }
        }
    }

    public static void LoadPalette(int target, ushort[] colors, uint byteSize)
    {
        ushort[] destination = palettes[target];
        int count = (int)Math.Min(Math.Min(byteSize / 2, (uint)colors.Length), (uint)destination.Length);
        Array.Copy(colors, destination, count);
        PaletteChanged?.Invoke(target);
    }
}
