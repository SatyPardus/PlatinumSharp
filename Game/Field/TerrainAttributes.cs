namespace PlatinumSharp.Game;

public sealed class TerrainAttributes
{
    public const int MapMatrixCount = 225;
    public const int MaxBlockCount = 16;
    public const int AttributesPerBlock = 0x400;
    public const int MapMatrixIndexToBlockIndexOffset = 0x00;
    public const int AttributesOffset = 0xE2;
    public const int Size = 0x80E2;

    private const int HeapIDField = 0xB;
    private const int NarcLandData = 0x43;
    private const uint LandDataAttributesOffset = 0x10;
    private const uint LandDataAttributesSize = 0x800;

    public readonly byte[] MapMatrixIndexToBlockIndex = new byte[MapMatrixCount];
    public readonly ushort[] Attributes = new ushort[AttributesPerBlock * MaxBlockCount];

    // OFFSET 0x02054BD0
    public static void TerrainAttributes_New(FieldSystem fieldSystem, int blockCount)
    {
        if (fieldSystem.TerrainAttributes == null)
        {
            fieldSystem.TerrainAttributes = new TerrainAttributes();
            TerrainAttributes_Load(fieldSystem.MapMatrix!, fieldSystem.TerrainAttributes, blockCount);
        }
    }

    // OFFSET 0x02054BF8
    public static void TerrainAttributes_Free(FieldSystem fieldSystem)
    {
        if (fieldSystem.TerrainAttributes != null)
        {
            fieldSystem.TerrainAttributes = null;
        }
    }

    // OFFSET 0x02054C0C
    public static Span<ushort> TerrainAttributes_Get(uint mapMatrixIndex, TerrainAttributes terrainAttributes)
    {
        int blockIndex = terrainAttributes.MapMatrixIndexToBlockIndex[mapMatrixIndex];
        return terrainAttributes.Attributes.AsSpan(blockIndex * AttributesPerBlock);
    }

    // OFFSET 0x02054C18
    private static void TerrainAttributes_Load(MapMatrix mapMatrix, TerrainAttributes terrainAttributes, int blockCount)
    {
        ushort[] blockIndexToLandDataID = new ushort[MaxBlockCount];
        byte loadedBlockCount = 0;
        int width = MapMatrix.MapMatrix_GetWidth(mapMatrix);
        int height = MapMatrix.MapMatrix_GetHeight(mapMatrix);
        int rowStart = 0;
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                ushort mapMatrixIndex = (ushort)(x + rowStart);
                int landDataID = MapMatrix.MapMatrix_GetLandDataIDByIndex2(mapMatrixIndex, mapMatrix);
                terrainAttributes.MapMatrixIndexToBlockIndex[mapMatrixIndex] = TerrainAttributes_GetOrPushLandDataID(landDataID, blockIndexToLandDataID, ref loadedBlockCount);
            }
            rowStart += width;
        }
        NARC narc = NARC.NARC_ctor(NarcLandData, HeapIDField);
        byte[] buffer = new byte[LandDataAttributesSize];
        for (int i = 0; i < blockCount; i++)
        {
            NARC.NARC_ReadFromMember(narc, blockIndexToLandDataID[i], LandDataAttributesOffset, LandDataAttributesSize, buffer);
            for (int j = 0; j < AttributesPerBlock; j++)
            {
                terrainAttributes.Attributes[i * AttributesPerBlock + j] = (ushort)(buffer[j * 2] | (buffer[j * 2 + 1] << 8));
            }
        }
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x02054CC4
    private static byte TerrainAttributes_GetOrPushLandDataID(int landDataID, ushort[] blockIndexToLandDataID, ref byte blockCount)
    {
        byte i;
        for (i = 0; i < blockCount; i++)
        {
            if (landDataID == blockIndexToLandDataID[i])
            {
                return i;
            }
        }
        if (i >= MaxBlockCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        blockIndexToLandDataID[i] = (ushort)landDataID;
        blockCount++;
        return i;
    }
}
