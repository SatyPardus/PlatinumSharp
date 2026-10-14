using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class CharacterSprite
{
    private const int TileRows = 8;
    private const int TileRowBytes = 4;
    private const int TileBytes = 0x20;
    private const int RegionCount = 6;
    private const int FaceFront = 2;
    private const int SpeciesTableEnd = 0x1EF;

    private static uint[] LoadRect(uint address)
    {
        return RomData.Arm9U32(address, 4);
    }

    private static uint[][] LoadRegions(uint address)
    {
        uint[] raw = RomData.Arm9U32(address, RegionCount * 4);
        uint[][] regions = new uint[RegionCount][];
        for (int i = 0; i < RegionCount; i++)
        {
            regions[i] = new[] { raw[i * 4], raw[i * 4 + 1], raw[i * 4 + 2], raw[i * 4 + 3] };
        }
        return regions;
    }

    // OFFSET 0x020131EC
    private static void CharacterSprite_CopyTileFromUntiledArray(byte[] src, byte[] dst, ref int srcOffset, ref int dstOffset, int rowBytes, int srcStride)
    {
        for (int i = 0; i < TileRows; i++)
        {
            Array.Copy(src, srcOffset, dst, dstOffset, rowBytes);
            srcOffset += srcStride;
            dstOffset += rowBytes;
        }
    }

    // OFFSET 0x0201322C
    private static CharacterData CharacterSprite_LoadSpriteData(int narcID, int memberIdx, int heapID)
    {
        byte[]? file = NARC.NARC_AllocAndReadWholeMemberByIndexPair(narcID, memberIdx, heapID);
        if (file == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        bool unpacked = G2d.GetUnpackedCharacterData(file!, out CharacterData? charData);
        if (!unpacked)
        {
            ErrorHandling.GF_AssertFail();
        }
        return charData!;
    }

    // OFFSET 0x02013260
    private static void CharacterSprite_TileUntiledPokemonSprite(int x, int y, int width, int height, CharacterData charData, byte[] dst)
    {
        int xEnd = x + width;
        if (charData.Width < xEnd)
        {
            ErrorHandling.GF_AssertFail();
        }
        int yEnd = y + height;
        if (charData.Height < yEnd)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte[] rawData = charData.RawData;
        int stride = charData.Width * TileRowBytes;
        int rowOffset = y * stride;
        int dstOffset = 0;
        for (int tileY = y; tileY < yEnd; tileY++)
        {
            for (int tileX = x; tileX < xEnd; tileX++)
            {
                int srcOffset = tileX * TileRowBytes + rowOffset * TileRows;
                CharacterSprite_CopyTileFromUntiledArray(rawData, dst, ref srcOffset, ref dstOffset, TileRowBytes, stride);
            }
            rowOffset += stride;
        }
    }

    // OFFSET 0x020132F8
    private static bool CharacterSprite_SpeciesIsSpinda(int species)
    {
        uint[] table = RomData.Arm9U32(0x020E52FC, 2);
        for (int i = 0; i < table.Length; i++)
        {
            if (table[i] == SpeciesTableEnd)
            {
                break;
            }
            if (table[i] == species)
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x02013338
    private static void CharacterSprite_LoadTiledPokemonSprite(int narcID, int memberIdx, int heapID, int x, int y, int width, int height, byte[] dst, uint personality, bool isAnimated, int face, int species)
    {
        CharacterData charData = CharacterSprite_LoadSpriteData(narcID, memberIdx, heapID);
        PokemonSprite.PokemonSprite_Decrypt(charData.RawData, narcID);
        if (face == FaceFront && CharacterSprite_SpeciesIsSpinda(species))
        {
            PokemonSprite.PokemonSprite_DrawSpindaSpots(charData.RawData, personality, isAnimated);
        }
        CharacterSprite_TileUntiledPokemonSprite(x, y, width, height, charData, dst);
    }

    // OFFSET 0x02013388
    private static byte[] CharacterSprite_GetTiledPokemonSprite(int narcID, int memberIdx, int heapID, int x, int y, int width, int height, uint personality, bool isAnimated, int face, int species)
    {
        byte[] dst = new byte[width * height * TileBytes];
        CharacterSprite_LoadTiledPokemonSprite(narcID, memberIdx, heapID, x, y, width, height, dst, personality, isAnimated, face, species);
        return dst;
    }

    // OFFSET 0x020133D4
    public static void CharacterSprite_LoadTiledData(int narcID, int memberIdx, int heapID, int x, int y, int width, int height, byte[] dst)
    {
        byte[]? file = NARC.NARC_AllocAndReadWholeMemberByIndexPair(narcID, memberIdx, heapID);
        if (file == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!G2d.GetUnpackedCharacterData(file!, out CharacterData? charData))
        {
            ErrorHandling.GF_AssertFail();
        }
        int xEnd = x + width;
        if (charData!.Width < xEnd)
        {
            ErrorHandling.GF_AssertFail();
        }
        int yEnd = y + height;
        if (charData.Height < yEnd)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte[] rawData = charData.RawData;
        PokemonSprite.PokemonSprite_Decrypt(rawData, narcID);
        int stride = charData.Width * TileRowBytes;
        int rowOffset = y * stride;
        int dstOffset = 0;
        for (int tileY = y; tileY < yEnd; tileY++)
        {
            for (int tileX = x; tileX < xEnd; tileX++)
            {
                int srcOffset = tileX * TileRowBytes + rowOffset * TileRows;
                CharacterSprite_CopyTileFromUntiledArray(rawData, dst, ref srcOffset, ref dstOffset, TileRowBytes, stride);
            }
            rowOffset += stride;
        }
    }

    // OFFSET 0x020134A8
    private static byte[] CharacterSprite_GetTiledSprite(int narcID, int memberIdx, int heapID, int x, int y, int width, int height)
    {
        byte[] dst = new byte[width * height * TileBytes];
        CharacterSprite_LoadTiledData(narcID, memberIdx, heapID, x, y, width, height, dst);
        return dst;
    }

    // OFFSET 0x020134E4
    private static void CharacterSprite_CopyTilesRect(int srcWidth, int srcHeight, int x, int y, int width, int height, ref int dstOffset, byte[] src, byte[] dst, int dstBase)
    {
        int xEnd = x + width;
        int yEnd = y + height;
        for (int tileY = y; tileY < yEnd; tileY++)
        {
            for (int tileX = x; tileX < xEnd; tileX++)
            {
                int srcOffset = tileX * TileBytes + srcWidth * (tileY * TileBytes);
                Array.Copy(src, srcOffset, dst, dstBase + dstOffset, TileBytes);
                dstOffset += TileBytes;
            }
        }
    }

    // OFFSET 0x02013560
    private static void CharacterSprite_CopyTilesRegion(int srcWidth, int srcHeight, uint[] region, ref int dstOffset, byte[] src, byte[] dst, int dstBase)
    {
        CharacterSprite_CopyTilesRect(srcWidth, srcHeight, (int)region[0], (int)region[1], (int)region[2], (int)region[3], ref dstOffset, src, dst, dstBase);
    }

    // OFFSET 0x02013584
    private static void CharacterSprite_LoadSpriteRect(int narcID, int memberIdx, int heapID, int x, int y, int width, int height, byte[] dst, int dstBase)
    {
        uint[][] regions = LoadRegions(0x020E5324);
        int dstOffset = 0;
        byte[] tiles = CharacterSprite_GetTiledSprite(narcID, memberIdx, heapID, x, y, width, height);
        for (int i = 0; i < RegionCount; i++)
        {
            CharacterSprite_CopyTilesRegion(width, height, regions[i], ref dstOffset, tiles, dst, dstBase);
        }
    }

    // OFFSET 0x020135F0
    public static void CharacterSprite_LoadSpriteRegion(int narcID, int memberIdx, int heapID, uint[] rect, byte[] dst, int dstBase = 0)
    {
        CharacterSprite_LoadSpriteRect(narcID, memberIdx, heapID, (int)rect[0], (int)rect[1], (int)rect[2], (int)rect[3], dst, dstBase);
    }

    // OFFSET 0x02013610
    public static void CharacterSprite_LoadSpriteFrame0(int narcID, int memberIdx, int heapID, byte[] dst)
    {
        CharacterSprite_LoadSpriteRegion(narcID, memberIdx, heapID, LoadRect(0x020E5304), dst);
    }

    // OFFSET 0x0201363C
    public static byte[] CharacterSprite_LoadTiles(int narcID, int memberIdx, int heapID)
    {
        byte[] dst = new byte[0xC80];
        CharacterSprite_LoadSpriteFrame0(narcID, memberIdx, heapID, dst);
        return dst;
    }

    // OFFSET 0x02013660
    public static ushort[] CharacterSprite_LoadPalette(int narcID, int memberIdx, int heapID)
    {
        ushort[] dst = new ushort[0x10];
        byte[] file = NARC.NARC_AllocAndReadWholeMemberByIndexPair(narcID, memberIdx, heapID);
        if (!G2d.GetUnpackedPaletteData(file, out PaletteFile? palette))
        {
            ErrorHandling.GF_AssertFail();
        }
        Array.Copy(palette!.Colors, dst, 0x10);
        return dst;
    }

    // OFFSET 0x020136A4
    private static void CharacterSprite_LoadPokemonSpriteRect(int narcID, int memberIdx, int heapID, int x, int y, int width, int height, byte[] dst, uint personality, bool isAnimated, int face, int species, int dstBase)
    {
        uint[][] regions = LoadRegions(0x020E5384);
        int dstOffset = 0;
        byte[] tiles = CharacterSprite_GetTiledPokemonSprite(narcID, memberIdx, heapID, x, y, width, height, personality, isAnimated, face, species);
        for (int i = 0; i < RegionCount; i++)
        {
            CharacterSprite_CopyTilesRegion(width, height, regions[i], ref dstOffset, tiles, dst, dstBase);
        }
    }

    // OFFSET 0x02013720
    public static void CharacterSprite_LoadPokemonSpriteRegion(int narcID, int memberIdx, int heapID, uint[] rect, byte[] dst, uint personality, bool isAnimated, int face, int species, int dstBase = 0)
    {
        CharacterSprite_LoadPokemonSpriteRect(narcID, memberIdx, heapID, (int)rect[0], (int)rect[1], (int)rect[2], (int)rect[3], dst, personality, isAnimated, face, species, dstBase);
    }

    // OFFSET 0x02013750
    public static void CharacterSprite_LoadPokemonSprite(int narcID, int memberIdx, int heapID, byte[] dst, uint personality, bool isAnimated, int face, int species)
    {
        CharacterSprite_LoadPokemonSpriteRegion(narcID, memberIdx, heapID, LoadRect(0x020E5314), dst, personality, isAnimated, face, species);
    }
}
