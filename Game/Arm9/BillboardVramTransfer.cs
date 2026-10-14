using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class BillboardVRAMTransfer
{
    public BillboardGfxSequence? GfxSequence;
    public G3dTexture? Texture;
    public uint TexKey;
    public uint PlttKey;
    public byte TextureIdx;
    public byte PlttIdx;
}

public sealed class BillboardListVRAMTransfer
{
    public BillboardVRAMTransfer[] BillboardTransfers = Array.Empty<BillboardVRAMTransfer>();
    public int MaxElements;
}

public static class BillboardVramTransfer
{
    private const byte NoIndex = 0xFF;
    private const int TransferTexture = 0;
    private const int TransferPalette = 1;
    private const int TextureDictOffset = 0x3C;
    private const int TextureVramKeyOffset = 8;
    private const int TextureDataOffset = 0x14;
    private const int PaletteDictOffset = 0x34;
    private const int PaletteDataOffset = 0x38;

    // OFFSET 0x0201DD00
    public static BillboardListVRAMTransfer BillboardListVRAMTransfer_New(int maxElements, int heapID)
    {
        BillboardListVRAMTransfer listTransfer = new();
        listTransfer.BillboardTransfers = new BillboardVRAMTransfer[maxElements];
        listTransfer.MaxElements = maxElements;
        for (int i = 0; i < listTransfer.MaxElements; i++)
        {
            listTransfer.BillboardTransfers[i] = new BillboardVRAMTransfer();
            BillboardVRAMTransfer_Clear(listTransfer.BillboardTransfers[i]);
        }
        return listTransfer;
    }

    // OFFSET 0x0201DD3C
    public static void BillboardListVRAMTransfer_Free(BillboardListVRAMTransfer listTransfer)
    {
        BillboardListVRAMTransfer_ResetAll(listTransfer);
    }

    // OFFSET 0x0201DD54
    public static BillboardVRAMTransfer? BillboardListVRAMTransfer_NewBillboardTransfer(BillboardListVRAMTransfer listTransfer, BillboardGfxSequence gfxSequence, G3dTexture texture, uint texKey, uint plttKey, int frame)
    {
        BillboardVRAMTransfer? transfer = null;
        for (int i = 0; i < listTransfer.MaxElements; i++)
        {
            if (listTransfer.BillboardTransfers[i].GfxSequence == null)
            {
                transfer = listTransfer.BillboardTransfers[i];
                break;
            }
        }
        if (transfer == null)
        {
            return null;
        }
        transfer.GfxSequence = gfxSequence;
        transfer.Texture = texture;
        transfer.TexKey = texKey;
        transfer.PlttKey = plttKey;
        transfer.TextureIdx = NoIndex;
        transfer.PlttIdx = NoIndex;
        BillboardVRAMTransfer_RequestFrame(transfer, (ushort)(frame >> Fx32.Shift));
        return transfer;
    }

    // OFFSET 0x0201DDAC
    public static void BillboardVRAMTransfer_Reset(BillboardVRAMTransfer transfer)
    {
        BillboardVRAMTransfer_Clear(transfer);
    }

    // OFFSET 0x0201DDB4
    public static void BillboardListVRAMTransfer_ResetAll(BillboardListVRAMTransfer listTransfer)
    {
        for (int i = 0; i < listTransfer.MaxElements; i++)
        {
            BillboardVRAMTransfer_Clear(listTransfer.BillboardTransfers[i]);
        }
    }

    // OFFSET 0x0201DDD8
    public static void BillboardVRAMTransfer_Request(BillboardVRAMTransfer transfer, int frame)
    {
        BillboardVRAMTransfer_RequestFrame(transfer, (ushort)(frame >> Fx32.Shift));
    }

    // OFFSET 0x0201DDE4
    private static void BillboardVRAMTransfer_Clear(BillboardVRAMTransfer transfer)
    {
        transfer.GfxSequence = null;
        transfer.Texture = null;
        transfer.TexKey = 0;
        transfer.PlttKey = 0;
        transfer.TextureIdx = 0;
        transfer.PlttIdx = 0;
    }

    // OFFSET 0x0201DDF4
    private static void BillboardVRAMTransfer_RequestFrame(BillboardVRAMTransfer transfer, ushort frame)
    {
        ushort indices = BillboardGfxSequence.BillboardGfxSequence_GetTexPlttIndexAt(transfer.GfxSequence!, frame);
        byte textureIdx = (byte)indices;
        byte plttIdx = (byte)(indices >> 8);
        if (transfer.TextureIdx != textureIdx)
        {
            BillboardVRAMTransfer_RequestTexture(transfer, textureIdx);
        }
        if (transfer.PlttIdx != plttIdx)
        {
            BillboardVRAMTransfer_RequestPalette(transfer, plttIdx);
        }
    }

    private static byte[] Slice(G3dTexture texture, int offset, uint size)
    {
        if (offset < 0)
        {
            return Array.Empty<byte>();
        }
        int length = (int)Math.Min(size, texture.File.Length - offset);
        return texture.File.AsSpan(offset, Math.Max(0, length)).ToArray();
    }

    // OFFSET 0x0201DE3C
    private static void BillboardVRAMTransfer_RequestTexture(BillboardVRAMTransfer transfer, byte textureIdx)
    {
        int offset = BillboardVRAMTransfer_GetTextureBuffer(transfer, textureIdx);
        uint size = ((transfer.TexKey & 0x7FFF0000) >> 16) << 4;
        VramTransfer.VramTransfer_Request(TransferTexture, (transfer.TexKey & 0xFFFF) << 3, Slice(transfer.Texture!, offset, size), size);
        transfer.TextureIdx = textureIdx;
    }

    // OFFSET 0x0201DE68
    private static void BillboardVRAMTransfer_RequestPalette(BillboardVRAMTransfer transfer, byte plttIdx)
    {
        int offset = BillboardVRAMTransfer_GetPaletteBuffer(transfer, plttIdx);
        uint size = ((transfer.PlttKey & 0xFFFF0000) >> 16) << 3;
        byte[] bytes = Slice(transfer.Texture!, offset, size);
        ushort[] colors = new ushort[bytes.Length / 2];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = (ushort)Bin.U16(bytes, i * 2);
        }
        VramTransfer.VramTransfer_Request(TransferPalette, (transfer.PlttKey & 0xFFFF) << 3, colors, size);
        transfer.PlttIdx = plttIdx;
    }

    // OFFSET 0x0201DE94
    private static int BillboardVRAMTransfer_GetTextureBuffer(BillboardVRAMTransfer transfer, byte textureIdx)
    {
        return GetTextureDataVRAMBuffer(transfer.Texture, textureIdx);
    }

    // OFFSET 0x0201DEA0
    private static int BillboardVRAMTransfer_GetPaletteBuffer(BillboardVRAMTransfer transfer, byte plttIdx)
    {
        return GetPlttDataVRAMBuffer(transfer.Texture, plttIdx);
    }

    // OFFSET 0x0201DEAC
    public static int GetTextureDataVRAMBuffer(G3dTexture? texture, byte index)
    {
        if (texture == null)
        {
            return -1;
        }
        byte[] file = texture.File;
        int entry = G3dDict.GetEntryByIndex(file, texture.Offset + TextureDictOffset, index);
        if (entry < 0)
        {
            return -1;
        }
        int dataOffset = (int)Bin.U32(file, texture.Offset + TextureDataOffset);
        int imageOffset = (int)(Bin.U32(file, entry) & 0xFFFF) + (int)Bin.U32(file, texture.Offset + TextureVramKeyOffset);
        return texture.Offset + dataOffset + (imageOffset << 3);
    }

    // OFFSET 0x0201DEFC
    public static int GetPlttDataVRAMBuffer(G3dTexture? texture, byte index)
    {
        if (texture == null)
        {
            return -1;
        }
        byte[] file = texture.File;
        int dictOffset = Bin.U16(file, texture.Offset + PaletteDictOffset);
        if (dictOffset == 0)
        {
            return -1;
        }
        int entry = G3dDict.GetEntryByIndex(file, texture.Offset + dictOffset, index);
        if (entry < 0)
        {
            return -1;
        }
        int dataOffset = (int)Bin.U32(file, texture.Offset + PaletteDataOffset);
        return texture.Offset + dataOffset + (Bin.U16(file, entry) << 3);
    }

    // OFFSET 0x0201DF40
    private static uint CalcTextureVramAddress(G3dTexture texture, int entry)
    {
        uint imageOffset = (Bin.U32(texture.File, entry) & 0xFFFF) << 3;
        uint baseAddress = (Bin.U32(texture.File, texture.Offset + TextureVramKeyOffset) & 0xFFFF) << 3;
        return imageOffset + baseAddress;
    }

    private static int FindTextureEntry(G3dTexture? texture, string name)
    {
        byte[] resourceName = new byte[G3dDict.NameLength];
        for (int i = 0; i < name.Length && i < G3dDict.NameLength; i++)
        {
            resourceName[i] = (byte)name[i];
        }
        if (texture == null)
        {
            return -1;
        }
        return G3dDict.GetEntryByName(texture.File, texture.Offset + TextureDictOffset, resourceName);
    }

    // OFFSET 0x0201DF50
    public static uint GetTextureDataBufferFromResourceName(G3dTexture? texture, string name)
    {
        int entry = FindTextureEntry(texture, name);
        if (entry < 0)
        {
            return 0;
        }
        return CalcTextureVramAddress(texture!, entry);
    }

    // OFFSET 0x0201DF84
    private static uint CalcTextureDataSize(G3dTexture texture, int entry)
    {
        uint param = Bin.U32(texture.File, entry);
        uint divisor;
        switch ((param >> 26) & 7)
        {
            case 2:
                divisor = 4;
                break;
            case 3:
                divisor = 2;
                break;
            case 1:
            case 4:
            case 6:
                divisor = 1;
                break;
            default:
                return 0;
        }
        uint width = ((param >> 20) & 7) << 4;
        uint height = ((param >> 23) & 7) << 4;
        return width * height / divisor;
    }

    // OFFSET 0x0201DFE4
    public static uint CalcTextureDataSizeFromResourceName(G3dTexture? texture, string name)
    {
        int entry = FindTextureEntry(texture, name);
        return CalcTextureDataSize(texture!, entry);
    }
}
