using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class CharacterData
{
    public ushort Height;
    public ushort Width;
    public uint PixelFormat;
    public uint MappingType;
    public uint CharacterFormat;
    public uint SizeBytes;
    public byte[] RawData = Array.Empty<byte>();
}

public sealed class ScreenData
{
    public ushort Width;
    public ushort Height;
    public ushort ColorMode;
    public ushort Format;
    public uint SizeBytes;
    public byte[] RawData = Array.Empty<byte>();
}

public sealed class PaletteCompressInfo
{
    public ushort PaletteCount;
    public ushort[] PaletteIndexes = Array.Empty<ushort>();
}

public sealed class CellDataBank
{
    public byte[] File = Array.Empty<byte>();
    public int SectionOffset;
}

public sealed class AnimBankData
{
    public byte[] File = Array.Empty<byte>();
    public int SectionOffset;
}

public sealed class MultiCellDataBank
{
    public byte[] File = Array.Empty<byte>();
    public int SectionOffset;
}

public sealed class MultiCellAnimBankData
{
    public byte[] File = Array.Empty<byte>();
    public int SectionOffset;
}

public sealed class ImageProxy
{
    public CharacterData? Character;
    public uint BaseAddress;
    public int VramType;
    public int Layout;
    public uint SizeS;
    public uint SizeT;
    public uint Format;
    public uint Color0Transparent;
    public int ExtendedPalette;
    public bool Located;
    public readonly int[] Locations = { -1, -1, -1 };

    public void CopyFrom(ImageProxy source)
    {
        Character = source.Character;
        BaseAddress = source.BaseAddress;
        VramType = source.VramType;
        Layout = source.Layout;
        SizeS = source.SizeS;
        SizeT = source.SizeT;
        Format = source.Format;
        Color0Transparent = source.Color0Transparent;
        ExtendedPalette = source.ExtendedPalette;
        Located = source.Located;
        Array.Copy(source.Locations, Locations, Locations.Length);
    }

    public void Init()
    {
        Character = null;
        Located = false;
        Array.Fill(Locations, -1);
        BaseAddress = 0;
        VramType = 0;
        Layout = 0;
        SizeS = 0;
        SizeT = 0;
        Format = 0;
        Color0Transparent = 0;
        ExtendedPalette = 0;
    }
}

public sealed class ImagePaletteProxy
{
    public PaletteFile? Palette;
    public PaletteCompressInfo? CompressInfo;
    public uint BaseAddress;
    public int VramType;
    public readonly int[] Locations = { -1, -1, -1 };

    public void Init()
    {
        Array.Fill(Locations, -1);
        Palette = null;
        CompressInfo = null;
        BaseAddress = 0;
        VramType = 0;
    }
}

public static class G2d
{
    public const int Vram3D = 0;
    public const int VramMain = 1;
    public const int VramSub = 2;
    public const int Layout1D = 1;
    public const int Layout2D = 2;

    private const uint ObjMappingMask = 0x00300010;

    public static bool GetUnpackedCharacterData(byte[] file, out CharacterData? data)
    {
        data = null;
        int s = Bin.FindSection(file, "RAHC");
        if (s < 0)
        {
            return false;
        }
        uint size = Bin.U32(file, s + 24);
        int start = s + 8 + Bin.I32(file, s + 28);
        int available = Math.Max(0, Math.Min((int)size, file.Length - start));
        byte[] raw = new byte[available];
        Array.Copy(file, start, raw, 0, available);
        data = new CharacterData
        {
            Height = (ushort)Bin.U16(file, s + 8),
            Width = (ushort)Bin.U16(file, s + 10),
            PixelFormat = Bin.U32(file, s + 12),
            MappingType = Bin.U32(file, s + 16),
            CharacterFormat = Bin.U32(file, s + 20),
            SizeBytes = size,
            RawData = raw,
        };
        return true;
    }

    public static bool GetUnpackedScreenData(byte[] file, out ScreenData? data)
    {
        data = null;
        int s = Bin.FindSection(file, "NRCS");
        if (s < 0)
        {
            return false;
        }
        uint size = Bin.U32(file, s + 16);
        int start = s + 20;
        int available = Math.Max(0, Math.Min((int)size, file.Length - start));
        byte[] raw = new byte[available];
        Array.Copy(file, start, raw, 0, available);
        data = new ScreenData
        {
            Width = (ushort)Bin.U16(file, s + 8),
            Height = (ushort)Bin.U16(file, s + 10),
            ColorMode = (ushort)Bin.U16(file, s + 12),
            Format = (ushort)Bin.U16(file, s + 14),
            SizeBytes = size,
            RawData = raw,
        };
        return true;
    }

    public static bool GetUnpackedPaletteData(byte[] file, out PaletteFile? data)
    {
        data = null;
        int s = Bin.FindSection(file, "TTLP");
        if (s < 0)
        {
            return false;
        }
        int sectionSize = Bin.I32(file, s + 4);
        int start = s + 8 + Bin.I32(file, s + 20);
        int available = Math.Max(0, Math.Min(sectionSize - 0x18, file.Length - start));
        ushort[] colors = new ushort[available / 2];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = (ushort)Bin.U16(file, start + i * 2);
        }
        data = new PaletteFile
        {
            Format = Bin.I32(file, s + 8),
            Extended = Bin.U32(file, s + 12) != 0,
            ByteSize = (uint)available,
            Colors = colors,
        };
        return true;
    }

    public static bool GetUnpackedPaletteCompressInfo(byte[] file, out PaletteCompressInfo? info)
    {
        info = null;
        int s = Bin.FindSection(file, "PMCP");
        if (s < 0)
        {
            return false;
        }
        ushort count = (ushort)Bin.U16(file, s + 8);
        int start = s + 8 + Bin.I32(file, s + 12);
        ushort[] indexes = new ushort[count];
        for (int i = 0; i < count && start + i * 2 + 1 < file.Length; i++)
        {
            indexes[i] = (ushort)Bin.U16(file, start + i * 2);
        }
        info = new PaletteCompressInfo { PaletteCount = count, PaletteIndexes = indexes };
        return true;
    }

    public static bool GetUnpackedCellBank(byte[] file, out CellDataBank? bank)
    {
        int s = Bin.FindSection(file, "KBEC");
        bank = s < 0 ? null : new CellDataBank { File = file, SectionOffset = s };
        return s >= 0;
    }

    public static bool GetUnpackedAnimBank(byte[] file, out AnimBankData? bank)
    {
        int s = Bin.FindSection(file, "KNBA");
        bank = s < 0 ? null : new AnimBankData { File = file, SectionOffset = s };
        return s >= 0;
    }

    public static bool GetUnpackedMultiCellBank(byte[] file, out MultiCellDataBank? bank)
    {
        int s = Bin.FindSection(file, "KBCM");
        bank = s < 0 ? null : new MultiCellDataBank { File = file, SectionOffset = s };
        return s >= 0;
    }

    public static bool GetUnpackedMultiCellAnimBank(byte[] file, out MultiCellAnimBankData? bank)
    {
        int s = Bin.FindSection(file, "KNBA");
        bank = s < 0 ? null : new MultiCellAnimBankData { File = file, SectionOffset = s };
        return s >= 0;
    }

    public static uint GetCellBankTransferSize(CellDataBank bank)
    {
        int data = bank.SectionOffset + 8;
        int transferOffset = Bin.I32(bank.File, data + 0xC);
        return transferOffset == 0 ? 0 : Bin.U32(bank.File, data + transferOffset);
    }

    public static int GetImageLocation(ImageProxy proxy, int vramType)
    {
        return proxy.Located ? proxy.Locations[vramType] : -1;
    }

    public static int GetImagePaletteLocation(ImagePaletteProxy proxy, int vramType)
    {
        return proxy.Palette != null ? proxy.Locations[vramType] : -1;
    }

    public static void LoadImageMapping(int layout, CharacterData character, uint baseAddress, int vramType, ImageProxy proxy)
    {
        proxy.Character = character;
        proxy.Located = true;
        proxy.BaseAddress = baseAddress;
        proxy.VramType = vramType;
        proxy.Locations[vramType] = (int)baseAddress;
        proxy.Layout = layout;
        if (vramType == Vram3D)
        {
            proxy.SizeS = (uint)Math.Max(0, System.Numerics.BitOperations.Log2((uint)Math.Max(8, character.Width * 8)) - 3);
            proxy.SizeT = (uint)Math.Max(0, System.Numerics.BitOperations.Log2((uint)Math.Max(8, character.Height * 8)) - 3);
            proxy.Format = character.PixelFormat;
            proxy.Color0Transparent = 1;
            Video.LoadTexture(character.RawData, baseAddress, character.SizeBytes);
            return;
        }
        int display = vramType == VramSub ? 1 : 0;
        uint mapping = layout == Layout1D ? character.MappingType & ObjMappingMask : 0;
        Video.SetDisplayControl(display, (Video.GetDisplayControl(display) & ~ObjMappingMask) | mapping);
        Video.LoadObjectTiles(display, character.RawData, baseAddress, character.SizeBytes);
    }

    public static void LoadImageVramTransfer(CharacterData character, uint baseAddress, int vramType, ImageProxy proxy)
    {
        proxy.Character = character;
        proxy.Located = true;
        proxy.BaseAddress = baseAddress;
        proxy.VramType = vramType;
        proxy.Locations[vramType] = (int)baseAddress;
        proxy.Layout = character.MappingType == 0 ? Layout2D : Layout1D;
    }

    public static void LoadPalette(PaletteFile palette, PaletteCompressInfo? compressInfo, uint baseAddress, int vramType, ImagePaletteProxy proxy)
    {
        proxy.Palette = palette;
        proxy.CompressInfo = compressInfo;
        proxy.BaseAddress = baseAddress;
        proxy.VramType = vramType;
        proxy.Locations[vramType] = (int)baseAddress;
        if (vramType == Vram3D)
        {
            Video.LoadTexturePalette(palette.Colors, baseAddress, palette.ByteSize);
            return;
        }
        int target = vramType == VramSub ? Video.SubObject : Video.MainObject;
        if (compressInfo == null)
        {
            Video.LoadPaletteRange(target, palette.Colors, 0, baseAddress, palette.ByteSize);
            return;
        }
        for (int i = 0; i < compressInfo.PaletteCount; i++)
        {
            uint destination = baseAddress + (uint)compressInfo.PaletteIndexes[i] * 0x20;
            Video.LoadPaletteRange(target, palette.Colors, i * 16, destination, 0x20);
        }
    }
}
