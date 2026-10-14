namespace PlatinumSharp.Game;

public sealed class DynamicTerrainHeightPlate
{
    public const int Size = 0x18;

    public int StartTileX;
    public int StartTileZ;
    public int SizeX;
    public int SizeZ;
    public int Height;
    public int Valid;
}

public sealed class DynamicTerrainHeightManager
{
    public const int Size = 0x08;

    public int PlatesCount;
    public DynamicTerrainHeightPlate[] Plates = Array.Empty<DynamicTerrainHeightPlate>();

    // OFFSET 0x021EF250
    private static int DynamicTerrainHeightPlate_ContainsTile(int tileX, int tileZ, DynamicTerrainHeightPlate plate)
    {
        if (plate.Valid == 0)
        {
            return 0;
        }
        int endTileX = plate.StartTileX + plate.SizeX - 1;
        int endTileZ = plate.StartTileZ + plate.SizeZ - 1;
        if (plate.StartTileX <= tileX && tileX <= endTileX && plate.StartTileZ <= tileZ && tileZ <= endTileZ)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021EF28C
    public static DynamicTerrainHeightManager DynamicTerrainHeightManager_New(byte platesCount, int heapID)
    {
        DynamicTerrainHeightManager manager = new();
        manager.Plates = new DynamicTerrainHeightPlate[platesCount];
        for (int i = 0; i < platesCount; i++)
        {
            manager.Plates[i] = new DynamicTerrainHeightPlate();
        }
        manager.PlatesCount = platesCount;
        for (byte i = 0; i < platesCount; i++)
        {
            manager.Plates[i].Valid = 0;
        }
        return manager;
    }

    // OFFSET 0x021EF2CC
    public static void DynamicTerrainHeightManager_SetPlate(byte index, int startTileX, int startTileZ, int sizeX, int sizeZ, int height, DynamicTerrainHeightManager manager)
    {
        manager.Plates[index].StartTileX = startTileX;
        manager.Plates[index].StartTileZ = startTileZ;
        manager.Plates[index].SizeX = sizeX;
        manager.Plates[index].SizeZ = sizeZ;
        manager.Plates[index].Height = height;
        manager.Plates[index].Valid = 1;
    }

    // OFFSET 0x021EF300
    public static void DynamicTerrainHeightManager_Free(DynamicTerrainHeightManager manager)
    {
        manager.Plates = Array.Empty<DynamicTerrainHeightPlate>();
    }

    // OFFSET 0x021EF314
    public static int DynamicTerrainHeightManager_GetPlateIndexOfTile(int tileX, int tileZ, DynamicTerrainHeightManager manager, out byte index)
    {
        index = 0;
        for (byte i = 0; i < manager.PlatesCount; i++)
        {
            if (DynamicTerrainHeightPlate_ContainsTile(tileX, tileZ, manager.Plates[i]) != 0)
            {
                index = i;
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x021EF35C
    public static int DynamicTerrainHeightManager_GetHeight(byte index, DynamicTerrainHeightManager manager)
    {
        if (index >= manager.PlatesCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (manager.Plates[index].Valid == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        return manager.Plates[index].Height;
    }

    // OFFSET 0x021EF388
    public static void DynamicTerrainHeightManager_SetHeight(byte index, int height, DynamicTerrainHeightManager manager)
    {
        if (index >= manager.PlatesCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        manager.Plates[index].Height = height;
    }
}
