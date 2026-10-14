using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.NARC;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class Graphics
{
    private const int LocationMainBgExtended = 2;
    private const int LocationMainObjExtended = 3;
    private const int LocationSubBgExtended = 6;
    private const int LocationSubObjExtended = 7;
    private const uint ObjectMappingMask = 0x00300010;

    private static readonly int[] sPaletteTargets =
    {
        Video.MainBackground,
        Video.MainObject,
        Video.MainBackgroundExtended,
        Video.MainObjectExtended,
        Video.SubBackground,
        Video.SubObject,
        Video.SubBackgroundExtended,
        Video.SubObjectExtended,
    };

    public static PaletteFile? GetPlttFile(int narcID, int narcMemberIdx, int heapID)
    {
        byte[]? buffer = Graphics_GetPlttData(narcID, (uint)narcMemberIdx, out PaletteFile? palette, heapID);
        return buffer == null ? null : palette;
    }

    private static byte[]? Uncompress(byte[] buffer, out uint size)
    {
        size = Bin.U32(buffer, 0) >> 8;
        return Lz.TryDecompress(buffer);
    }

    // OFFSET 0x02006E3C
    public static uint Graphics_LoadTilesToBgLayer(int narcID, uint narcMemberIdx, object bgConfig, uint bgLayer, uint offset, uint size, int compressed, int heapID)
    {
        byte[]? buffer = LoadMemberFromNARC(narcID, narcMemberIdx, compressed, heapID, 0);
        return LoadTilesToBgLayer(buffer, bgConfig, bgLayer, offset, size);
    }

    // OFFSET 0x02006E60
    public static void Graphics_LoadTilemapToBgLayer(int narcID, uint narcMemberIdx, object bgConfig, uint bgLayer, uint offset, uint size, int compressed, int heapID)
    {
        byte[]? buffer = LoadMemberFromNARC(narcID, narcMemberIdx, compressed, heapID, 1);
        LoadTilemapToBgLayer(buffer, bgConfig, bgLayer, offset, size);
    }

    // OFFSET 0x02006E84
    public static void Graphics_LoadPalette(int narcID, uint narcMemberIdx, int loadLocation, uint palOffset, uint size, int heapID)
    {
        Graphics_LoadPaletteWithSrcOffset(narcID, narcMemberIdx, loadLocation, 0, palOffset, size, heapID);
    }

    // OFFSET 0x02006E9C
    public static void Graphics_LoadPaletteWithSrcOffset(int narcID, uint narcMemberIdx, int loadLocation, uint srcOffset, uint palOffset, uint size, int heapID)
    {
        byte[]? buffer = LoadMemberFromNARC(narcID, narcMemberIdx, 0, heapID, 1);
        LoadPaletteWithSrcOffset(buffer, loadLocation, srcOffset, palOffset, size);
    }

    // OFFSET 0x02006EC0
    public static uint Graphics_LoadObjectTiles(int narcID, uint narcMemberIdx, int display, uint offset, uint size, int compressed, int heapID)
    {
        byte[]? buffer = LoadMemberFromNARC(narcID, narcMemberIdx, compressed, heapID, 1);
        return LoadObjectTiles(buffer, display, offset, size);
    }

    // OFFSET 0x02006EE0
    public static void Graphics_LoadPartialPalette(int narcID, uint narcMemberIdx, int vramType, uint baseAddr, int heapID, ImagePaletteProxy paletteProxy)
    {
        byte[]? buffer = LoadMemberFromNARC(narcID, narcMemberIdx, 0, heapID, 1);
        LoadPartialPalette(buffer, vramType, baseAddr, paletteProxy);
    }

    // OFFSET 0x02006F00
    public static uint Graphics_LoadImageMapping(int narcID, uint narcMemberIdx, int compressed, int layout, uint size, int vramType, uint baseAddr, int heapID, ImageProxy imageProxy)
    {
        byte[]? buffer = LoadMemberFromNARC(narcID, narcMemberIdx, compressed, heapID, 1);
        return LoadImageMapping(buffer, layout, size, vramType, baseAddr, imageProxy);
    }

    // OFFSET 0x02006F28
    public static void Graphics_LoadImageMappingAndSetVramMode(int narcID, uint narcMemberIdx, int compressed, int layout, uint size, int vramType, uint baseAddr, int heapID, ImageProxy imageProxy)
    {
        byte[]? buffer = LoadMemberFromNARC(narcID, narcMemberIdx, compressed, heapID, 1);
        LoadImageMappingAndSetVramMode(buffer, layout, size, vramType, baseAddr, imageProxy);
    }

    // OFFSET 0x02006F50
    public static byte[]? Graphics_GetCharData(int narcID, uint narcMemberIdx, int compressed, out CharacterData? outCharData, int heapID)
    {
        byte[]? buffer = LoadMemberFromNARC(narcID, narcMemberIdx, compressed, heapID, 0);
        return GetCharacterData(buffer, out outCharData);
    }

    // OFFSET 0x02006F6C
    public static byte[]? Graphics_GetScrnData(int narcID, uint narcMemberIdx, int compressed, out ScreenData? outScreenData, int heapID)
    {
        byte[]? buffer = LoadMemberFromNARC(narcID, narcMemberIdx, compressed, heapID, 0);
        return GetScreenData(buffer, out outScreenData);
    }

    // OFFSET 0x02006F88
    public static byte[]? Graphics_GetPlttData(int narcID, uint narcMemberIdx, out PaletteFile? outPaletteData, int heapID)
    {
        byte[]? buffer = LoadMemberFromNARC(narcID, narcMemberIdx, 0, heapID, 0);
        return GetPaletteData(buffer, out outPaletteData);
    }

    // OFFSET 0x02006FA0
    public static byte[]? Graphics_GetCellBank(int narcID, uint narcMemberIdx, int compressed, out CellDataBank? outCellBank, int heapID)
    {
        byte[]? buffer = LoadMemberFromNARC(narcID, narcMemberIdx, compressed, heapID, 0);
        return GetCellBank(buffer, out outCellBank);
    }

    // OFFSET 0x02006FBC
    public static byte[]? Graphics_GetAnimBank(int narcID, uint narcMemberIdx, int compressed, out AnimBankData? outAnimBank, int heapID)
    {
        byte[]? buffer = LoadMemberFromNARC(narcID, narcMemberIdx, compressed, heapID, 0);
        return GetAnimBank(buffer, out outAnimBank);
    }

    // OFFSET 0x02006FD8
    public static byte[]? LoadCompressedMemberFromNARC(int narcID, uint narcMemberIdx, int heapID)
    {
        return LoadMemberFromNARC(narcID, narcMemberIdx, 1, heapID, 0);
    }

    // OFFSET 0x02006FE8
    public static byte[]? LoadMemberFromNARC(int narcID, uint narcMemberIdx, int compressed, int heapID, int allocAtEnd)
    {
        byte[] buffer = new byte[NARC_GetMemberSizeByIndexPair(narcID, (int)narcMemberIdx)];
        NARC_ReadWholeMemberByIndexPair(buffer, narcID, (int)narcMemberIdx);
        if (compressed != 0)
        {
            return Uncompress(buffer, out _);
        }
        return buffer;
    }

    // OFFSET 0x02007068
    public static byte[]? LoadMemberFromNARC_OutFileSize(int narcID, uint narcMemberIdx, int compressed, int heapID, int allocAtEnd, out uint fileSize)
    {
        fileSize = NARC_GetMemberSizeByIndexPair(narcID, (int)narcMemberIdx);
        byte[] buffer = new byte[fileSize];
        NARC_ReadWholeMemberByIndexPair(buffer, narcID, (int)narcMemberIdx);
        if (compressed != 0)
        {
            return Uncompress(buffer, out fileSize);
        }
        return buffer;
    }

    // OFFSET 0x020070E8
    public static uint Graphics_LoadTilesToBgLayerFromOpenNARC(NARC narc, uint narcMemberIdx, object bgConfig, uint bgLayer, uint offset, uint size, int compressed, int heapID)
    {
        byte[]? buffer = LoadMemberFromOpenNARC(narc, narcMemberIdx, compressed, heapID, 0);
        return LoadTilesToBgLayer(buffer, bgConfig, bgLayer, offset, size);
    }

    // OFFSET 0x0200710C
    public static void Graphics_LoadTilemapToBgLayerFromOpenNARC(NARC narc, uint narcMemberIdx, object bgConfig, uint bgLayer, uint offset, uint size, int compressed, int heapID)
    {
        byte[]? buffer = LoadMemberFromOpenNARC(narc, narcMemberIdx, compressed, heapID, 1);
        LoadTilemapToBgLayer(buffer, bgConfig, bgLayer, offset, size);
    }

    // OFFSET 0x02007130
    public static void Graphics_LoadPaletteFromOpenNARC(NARC narc, uint narcMemberIdx, int loadLocation, uint offset, uint size, int heapID)
    {
        Graphics_LoadPaletteWithSrcOffsetFromOpenNARC(narc, narcMemberIdx, loadLocation, 0, offset, size, heapID);
    }

    // OFFSET 0x02007148
    public static void Graphics_LoadPaletteWithSrcOffsetFromOpenNARC(NARC narc, uint narcMemberIdx, int loadLocation, uint srcOffset, uint offset, uint size, int heapID)
    {
        byte[]? buffer = LoadMemberFromOpenNARC(narc, narcMemberIdx, 0, heapID, 1);
        LoadPaletteWithSrcOffset(buffer, loadLocation, srcOffset, offset, size);
    }

    // OFFSET 0x0200716C
    public static void Graphics_LoadPartialPaletteFromOpenNARC(NARC narc, uint narcMemberIdx, int vramType, uint baseAddr, int heapID, ImagePaletteProxy paletteProxy)
    {
        byte[]? buffer = LoadMemberFromOpenNARC(narc, narcMemberIdx, 0, heapID, 1);
        LoadPartialPalette(buffer, vramType, baseAddr, paletteProxy);
    }

    // OFFSET 0x0200718C
    public static uint Graphics_LoadImageMappingFromOpenNARC(NARC narc, uint narcMemberIdx, int compressed, int layout, uint size, int vramType, uint baseAddr, int heapID, ImageProxy imageProxy)
    {
        byte[]? buffer = LoadMemberFromOpenNARC(narc, narcMemberIdx, compressed, heapID, 1);
        return LoadImageMapping(buffer, layout, size, vramType, baseAddr, imageProxy);
    }

    // OFFSET 0x020071B4
    public static byte[]? Graphics_GetCharDataFromOpenNARC(NARC narc, uint narcMemberIdx, int compressed, out CharacterData? outCharData, int heapID)
    {
        byte[]? buffer = LoadMemberFromOpenNARC(narc, narcMemberIdx, compressed, heapID, 0);
        return GetCharacterData(buffer, out outCharData);
    }

    // OFFSET 0x020071D0
    public static byte[]? Graphics_GetScrnDataFromOpenNARC(NARC narc, uint narcMemberIdx, int compressed, out ScreenData? outScreenData, int heapID)
    {
        byte[]? buffer = LoadMemberFromOpenNARC(narc, narcMemberIdx, compressed, heapID, 0);
        return GetScreenData(buffer, out outScreenData);
    }

    // OFFSET 0x020071EC
    public static byte[]? Graphics_GetPlttDataFromOpenNARC(NARC narc, uint narcMemberIdx, out PaletteFile? outPaletteData, int heapID)
    {
        byte[]? buffer = LoadMemberFromOpenNARC(narc, narcMemberIdx, 0, heapID, 0);
        return GetPaletteData(buffer, out outPaletteData);
    }

    // OFFSET 0x02007204
    public static byte[]? Graphics_GetCellBankFromOpenNARC(NARC narc, uint narcMemberIdx, int compressed, out CellDataBank? outCellBank, int heapID)
    {
        byte[]? buffer = LoadMemberFromOpenNARC(narc, narcMemberIdx, compressed, heapID, 0);
        return GetCellBank(buffer, out outCellBank);
    }

    // OFFSET 0x02007220
    public static byte[]? Graphics_GetAnimBankFromOpenNARC(NARC narc, uint narcMemberIdx, int compressed, out AnimBankData? outAnimBank, int heapID)
    {
        byte[]? buffer = LoadMemberFromOpenNARC(narc, narcMemberIdx, compressed, heapID, 0);
        return GetAnimBank(buffer, out outAnimBank);
    }

    // OFFSET 0x0200723C
    public static byte[]? LoadMemberFromOpenNARC(NARC narc, uint narcMemberIdx, int compressed, int heapID, int allocAtEnd)
    {
        return LoadMemberFromOpenNARC_OutFileSize(narc, narcMemberIdx, compressed, heapID, allocAtEnd, out _);
    }

    // OFFSET 0x02007250
    public static byte[]? LoadMemberFromOpenNARC_OutFileSize(NARC narc, uint narcMemberIdx, int compressed, int heapID, int allocAtEnd, out uint fileSize)
    {
        fileSize = NARC_GetMemberSize(narc, narcMemberIdx);
        byte[] buffer = new byte[fileSize];
        NARC_ReadWholeMember(narc, narcMemberIdx, buffer);
        if (compressed != 0)
        {
            return Uncompress(buffer, out fileSize);
        }
        return buffer;
    }

    // OFFSET 0x020072D0
    private static uint LoadTilesToBgLayer(byte[]? ncgrBuffer, object bgConfig, uint bgLayer, uint offset, uint size)
    {
        if (ncgrBuffer != null && G2d.GetUnpackedCharacterData(ncgrBuffer, out CharacterData? charData))
        {
            if (size == 0)
            {
                size = charData!.SizeBytes;
            }
            Bg.Bg_LoadTiles(bgConfig, (byte)bgLayer, charData!.RawData, size, offset);
        }
        return size;
    }

    // OFFSET 0x02007314
    private static void LoadTilemapToBgLayer(byte[]? nscrBuffer, object bgConfig, uint bgLayer, uint offset, uint size)
    {
        if (nscrBuffer == null || !G2d.GetUnpackedScreenData(nscrBuffer, out ScreenData? screenData))
        {
            return;
        }
        if (size == 0)
        {
            size = screenData!.SizeBytes;
        }
        if (Bg.Bg_GetTilemapBuffer(bgConfig, (byte)bgLayer) != null)
        {
            Bg.Bg_LoadTilemapBuffer(bgConfig, (byte)bgLayer, screenData!.RawData, size);
        }
        Bg.Bg_CopyTilemapBufferRangeToVRAM(bgConfig, (byte)bgLayer, screenData!.RawData, size, offset);
    }

    // OFFSET 0x02007374
    private static uint LoadObjectTiles(byte[]? ncgrBuffer, int display, uint offset, uint size)
    {
        if (ncgrBuffer != null && G2d.GetUnpackedCharacterData(ncgrBuffer, out CharacterData? charData))
        {
            if (size == 0)
            {
                size = charData!.SizeBytes;
            }
            Video.LoadObjectTiles(display, charData!.RawData, offset, size);
        }
        return size;
    }

    // OFFSET 0x020073BC
    private static void LoadPaletteWithSrcOffset(byte[]? nclrBuffer, int loadLocation, uint srcOffset, uint offset, uint size)
    {
        if (nclrBuffer == null || !G2d.GetUnpackedPaletteData(nclrBuffer, out PaletteFile? paletteData))
        {
            return;
        }
        if (size == 0)
        {
            size = paletteData!.ByteSize - srcOffset;
        }
        switch (loadLocation)
        {
            case LocationMainBgExtended:
            case LocationMainObjExtended:
            case LocationSubBgExtended:
            case LocationSubObjExtended:
            default:
                Video.LoadPaletteRange(sPaletteTargets[loadLocation], paletteData!.Colors, (int)(srcOffset / 2), offset, size);
                break;
        }
    }

    // OFFSET 0x0200749C
    private static void LoadPartialPalette(byte[]? nclrBuffer, int vramType, uint baseAddr, ImagePaletteProxy paletteProxy)
    {
        if (nclrBuffer == null)
        {
            return;
        }
        bool hasCompressInfo = G2d.GetUnpackedPaletteCompressInfo(nclrBuffer, out PaletteCompressInfo? compressInfo);
        if (G2d.GetUnpackedPaletteData(nclrBuffer, out PaletteFile? paletteData))
        {
            if (hasCompressInfo)
            {
                G2d.LoadPalette(paletteData!, compressInfo, baseAddr, vramType, paletteProxy);
            }
            else
            {
                G2d.LoadPalette(paletteData!, null, baseAddr, vramType, paletteProxy);
            }
        }
    }

    // OFFSET 0x020074EC
    private static uint LoadImageMapping(byte[]? ncgrBuffer, int layout, uint size, int vramType, uint baseAddr, ImageProxy imageProxy)
    {
        uint result = 0;
        if (ncgrBuffer != null && G2d.GetUnpackedCharacterData(ncgrBuffer, out CharacterData? charData))
        {
            if (size != 0)
            {
                charData!.SizeBytes = size;
            }
            G2d.LoadImageMapping(layout, charData!, baseAddr, vramType, imageProxy);
            result = charData!.SizeBytes;
        }
        return result;
    }

    // OFFSET 0x02007534
    private static void LoadImageMappingAndSetVramMode(byte[]? ncgrBuffer, int layout, uint size, int vramType, uint baseAddr, ImageProxy imageProxy)
    {
        if (ncgrBuffer == null || !G2d.GetUnpackedCharacterData(ncgrBuffer, out CharacterData? charData))
        {
            return;
        }
        if (size != 0)
        {
            charData!.SizeBytes = size;
        }
        if (vramType == G2d.VramMain)
        {
            charData!.MappingType = Video.GetDisplayControl(0) & ObjectMappingMask;
        }
        else if (vramType == G2d.VramSub)
        {
            charData!.MappingType = Video.GetDisplayControl(1) & ObjectMappingMask;
        }
        G2d.LoadImageMapping(layout, charData!, baseAddr, vramType, imageProxy);
    }

    // OFFSET 0x020075A0
    private static byte[]? GetCharacterData(byte[]? ncgrBuffer, out CharacterData? outCharData)
    {
        outCharData = null;
        if (ncgrBuffer != null && !G2d.GetUnpackedCharacterData(ncgrBuffer, out outCharData))
        {
            return null;
        }
        return ncgrBuffer;
    }

    // OFFSET 0x020075BC
    private static byte[]? GetScreenData(byte[]? nscrBuffer, out ScreenData? outScreenData)
    {
        outScreenData = null;
        if (nscrBuffer != null && !G2d.GetUnpackedScreenData(nscrBuffer, out outScreenData))
        {
            return null;
        }
        return nscrBuffer;
    }

    // OFFSET 0x020075D8
    private static byte[]? GetPaletteData(byte[]? nclrBuffer, out PaletteFile? outPaletteData)
    {
        outPaletteData = null;
        if (nclrBuffer != null && !G2d.GetUnpackedPaletteData(nclrBuffer, out outPaletteData))
        {
            return null;
        }
        return nclrBuffer;
    }

    // OFFSET 0x020075F4
    private static byte[]? GetCellBank(byte[]? ncerBuffer, out CellDataBank? outCellBank)
    {
        outCellBank = null;
        if (ncerBuffer != null && !G2d.GetUnpackedCellBank(ncerBuffer, out outCellBank))
        {
            return null;
        }
        return ncerBuffer;
    }

    // OFFSET 0x02007610
    private static byte[]? GetAnimBank(byte[]? nanrBuffer, out AnimBankData? outAnimBank)
    {
        outAnimBank = null;
        if (nanrBuffer != null && !G2d.GetUnpackedAnimBank(nanrBuffer, out outAnimBank))
        {
            return null;
        }
        return nanrBuffer;
    }
}
