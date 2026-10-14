using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class RomStructs
{
    public const int BgTemplateSize = 0x1C;
    public const int GraphicsModesSize = 0x10;
    public const int BanksSize = 0x28;
    public const int WindowTemplateSize = 8;

    public static BgTemplate BgTemplate(byte[] raw, int offset)
    {
        return new BgTemplate
        {
            X = Bin.U32(raw, offset),
            Y = Bin.U32(raw, offset + 4),
            BufferSize = Bin.U32(raw, offset + 8),
            BaseTile = Bin.U32(raw, offset + 0xC),
            ScreenSize = raw[offset + 0x10],
            ColorMode = raw[offset + 0x11],
            ScreenBase = raw[offset + 0x12],
            CharBase = raw[offset + 0x13],
            BgExtPltt = raw[offset + 0x14],
            Priority = raw[offset + 0x15],
            AreaOver = raw[offset + 0x16],
            Mosaic = (int)Bin.U32(raw, offset + 0x18),
        };
    }

    public static GraphicsModes GraphicsModes(byte[] raw, int offset)
    {
        return new GraphicsModes
        {
            DisplayMode = (int)Bin.U32(raw, offset),
            MainBgMode = (int)Bin.U32(raw, offset + 4),
            SubBgMode = (int)Bin.U32(raw, offset + 8),
            Bg0As2DOr3D = (int)Bin.U32(raw, offset + 0xC),
        };
    }

    public static GXBanks Banks(byte[] raw, int offset)
    {
        return new GXBanks
        {
            Bg = Bin.U32(raw, offset),
            BgExtPltt = Bin.U32(raw, offset + 4),
            SubBg = Bin.U32(raw, offset + 8),
            SubBgExtPltt = Bin.U32(raw, offset + 0xC),
            Obj = Bin.U32(raw, offset + 0x10),
            ObjExtPltt = Bin.U32(raw, offset + 0x14),
            SubObj = Bin.U32(raw, offset + 0x18),
            SubObjExtPltt = Bin.U32(raw, offset + 0x1C),
            Tex = Bin.U32(raw, offset + 0x20),
            TexPltt = Bin.U32(raw, offset + 0x24),
        };
    }

    public static WindowTemplate WindowTemplate(byte[] raw, int offset)
    {
        return new WindowTemplate
        {
            BgLayer = raw[offset],
            TilemapLeft = raw[offset + 1],
            TilemapTop = raw[offset + 2],
            Width = raw[offset + 3],
            Height = raw[offset + 4],
            Palette = raw[offset + 5],
            BaseTile = (ushort)Bin.U16(raw, offset + 6),
        };
    }

    public static BgTemplate Arm9BgTemplate(uint address)
    {
        return BgTemplate(RomData.Arm9(address, BgTemplateSize), 0);
    }

    public static GraphicsModes Arm9GraphicsModes(uint address)
    {
        return GraphicsModes(RomData.Arm9(address, GraphicsModesSize), 0);
    }

    public static GXBanks Arm9Banks(uint address)
    {
        return Banks(RomData.Arm9(address, BanksSize), 0);
    }

    public static WindowTemplate Arm9WindowTemplate(uint address)
    {
        return WindowTemplate(RomData.Arm9(address, WindowTemplateSize), 0);
    }

    public const int SpriteTemplateSize = 0x28;

    public static RenderOamTemplate Arm9RenderOamTemplate(uint address)
    {
        int[] v = Array.ConvertAll(RomData.Arm9U32(address, 8), x => (int)x);
        return new RenderOamTemplate { MainOamStart = v[0], MainOamCount = v[1], MainAffineOamStart = v[2], MainAffineOamCount = v[3], SubOamStart = v[4], SubOamCount = v[5], SubAffineOamStart = v[6], SubAffineOamCount = v[7] };
    }

    public static CharTransferTemplateWithModes Arm9CharTransferTemplate(uint address)
    {
        int[] v = Array.ConvertAll(RomData.Arm9U32(address, 5), x => (int)x);
        return new CharTransferTemplateWithModes { MaxTasks = v[0], SizeMain = v[1], SizeSub = v[2], ModeMain = v[3], ModeSub = v[4] };
    }

    public static SpriteTemplateFromResourceHeader Arm9SpriteTemplate(uint address)
    {
        byte[] raw = RomData.Arm9(address, SpriteTemplateSize);
        return new SpriteTemplateFromResourceHeader
        {
            ResourceHeaderID = (int)Bin.U32(raw, 0),
            X = (short)Bin.U16(raw, 4),
            Y = (short)Bin.U16(raw, 6),
            Z = (short)Bin.U16(raw, 8),
            AnimIdx = (ushort)Bin.U16(raw, 0xA),
            Priority = (int)Bin.U32(raw, 0xC),
            PlttIdx = (int)Bin.U32(raw, 0x10),
            VramType = (int)Bin.U32(raw, 0x14),
            Unused18 = (int)Bin.U32(raw, 0x18),
            Unused1C = (int)Bin.U32(raw, 0x1C),
            Unused20 = (int)Bin.U32(raw, 0x20),
            Unused24 = (int)Bin.U32(raw, 0x24),
        };
    }

    public static string?[] Arm9StringTable(uint address, int count)
    {
        uint[] pointers = RomData.Arm9U32(address, count);
        string?[] strings = new string?[count];
        for (int i = 0; i < count; i++)
        {
            strings[i] = pointers[i] == 0 ? null : RomData.Arm9String(pointers[i]);
        }
        return strings;
    }
}
