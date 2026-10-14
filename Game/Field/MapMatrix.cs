using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class MapMatrixData
{
    public byte Height;
    public byte Width;
    public readonly ushort[] MapHeaderIDs = new ushort[MapMatrix.MaxSize];
    public readonly byte[] Altitudes = new byte[MapMatrix.MaxSize];
    public readonly ushort[] LandDataIDs = new ushort[MapMatrix.MaxSize];
    public readonly byte[] ModelNamePrefix = new byte[MapMatrix.MaxNameLength];
}

public sealed class MainMapMatrixData
{
    public readonly ushort[] MapHeaderIDs = new ushort[MapMatrix.MaxSize];
}

public sealed class MapMatrix
{
    public const int MaxSize = 900;
    public const int MaxNameLength = 16;
    public const int MainMatrixWidth = 30;

    private const int NarcMapMatrix = 0x29;
    private const int HeapIDField = 0xB;

    public byte Width;
    public byte Height;
    public byte MatrixID;
    public readonly MapMatrixData Data = new();

    // OFFSET 0x02039C80
    private static void MapMatrixData_Load(MapMatrixData data, int mapMatrixID, int mapHeaderID)
    {
        data.Width = 0;
        data.Height = 0;
        for (int i = 0; i < MaxSize; i++)
        {
            data.MapHeaderIDs[i] = 0;
            data.Altitudes[i] = 0;
            data.LandDataIDs[i] = 0;
        }
        for (int i = 0; i < MaxNameLength; i++)
        {
            data.ModelNamePrefix[i] = 0;
        }
        byte[] file = NARC.NARC_AllocAtEndAndReadWholeMemberByIndexPair(NarcMapMatrix, mapMatrixID, HeapIDField);
        data.Width = file[0];
        data.Height = file[1];
        int hasMapHeaderIDs = file[2];
        int hasAltitudes = file[3];
        int nameLength = file[4];
        int position = 5;
        if (nameLength > MaxNameLength)
        {
            ErrorHandling.GF_AssertFail();
        }
        Array.Copy(file, position, data.ModelNamePrefix, 0, nameLength);
        position += nameLength;
        int count = data.Width * data.Height;
        if (hasMapHeaderIDs != 0)
        {
            for (int i = 0; i < count; i++)
            {
                data.MapHeaderIDs[i] = (ushort)Bin.U16(file, position + (i * 2));
            }
            position += count * 2;
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                data.MapHeaderIDs[i] = (ushort)mapHeaderID;
            }
        }
        if (hasAltitudes != 0)
        {
            Array.Copy(file, position, data.Altitudes, 0, count);
            position += count;
        }
        for (int i = 0; i < count; i++)
        {
            data.LandDataIDs[i] = (ushort)Bin.U16(file, position + (i * 2));
        }
        Heap.Heap_Free(file);
    }

    // OFFSET 0x02039D6C
    public static MapMatrix MapMatrix_New()
    {
        return MapMatrix_NewWithHeapID(HeapIDField);
    }

    // OFFSET 0x02039D78
    public static MapMatrix MapMatrix_NewWithHeapID(int heapID)
    {
        MapMatrix mapMatrix = new();
        mapMatrix.Width = 0;
        mapMatrix.Height = 0;
        mapMatrix.MatrixID = 0;
        return mapMatrix;
    }

    // OFFSET 0x02039D90
    public static void MapMatrix_Copy(MapMatrix? source, MapMatrix? destination)
    {
        if (destination == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (source == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (source == destination)
        {
            ErrorHandling.GF_AssertFail();
        }
        destination!.Width = source!.Width;
        destination.Height = source.Height;
        destination.MatrixID = source.MatrixID;
        destination.Data.Height = source.Data.Height;
        destination.Data.Width = source.Data.Width;
        Array.Copy(source.Data.MapHeaderIDs, destination.Data.MapHeaderIDs, MaxSize);
        Array.Copy(source.Data.Altitudes, destination.Data.Altitudes, MaxSize);
        Array.Copy(source.Data.LandDataIDs, destination.Data.LandDataIDs, MaxSize);
        Array.Copy(source.Data.ModelNamePrefix, destination.Data.ModelNamePrefix, MaxNameLength);
    }

    // OFFSET 0x02039DC0
    public static void MapMatrix_Load(int mapHeaderID, MapMatrix mapMatrix)
    {
        int mapMatrixID = MapHeader.MapHeader_GetMapMatrixID(mapHeaderID);
        MapMatrixData_Load(mapMatrix.Data, mapMatrixID, mapHeaderID);
        mapMatrix.MatrixID = (byte)mapMatrixID;
        mapMatrix.Height = mapMatrix.Data.Height;
        mapMatrix.Width = mapMatrix.Data.Width;
    }

    // OFFSET 0x02039DE4
    public static void MapMatrix_Free(MapMatrix mapMatrix)
    {
    }

    // OFFSET 0x02039DEC
    public static int MapMatrix_GetLandDataIDByIndex2(int index, MapMatrix mapMatrix)
    {
        if (index >= mapMatrix.Width * mapMatrix.Height)
        {
            ErrorHandling.GF_AssertFail();
        }
        return mapMatrix.Data.LandDataIDs[index];
    }

    // OFFSET 0x02039E10
    public static int MapMatrix_GetWidth(MapMatrix? mapMatrix)
    {
        if (mapMatrix == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return mapMatrix!.Width;
    }

    // OFFSET 0x02039E20
    public static int MapMatrix_GetHeight(MapMatrix? mapMatrix)
    {
        if (mapMatrix == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return mapMatrix!.Height;
    }

    // OFFSET 0x02039E30
    public static int MapMatrix_GetMapHeaderIDAtCoords(MapMatrix mapMatrix, int x, int y)
    {
        int height = mapMatrix.Height;
        int width = mapMatrix.Width;
        if (x < 0 || x >= width)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (y < 0 || y >= height)
        {
            ErrorHandling.GF_AssertFail();
        }
        return mapMatrix.Data.MapHeaderIDs[x + (y * width)];
    }

    // OFFSET 0x02039E68
    public static int MapMatrix_GetMapHeaderIDAtIndex(MapMatrix mapMatrix, int index)
    {
        int count = mapMatrix.Width * mapMatrix.Height;
        if (index < 0 || index >= count)
        {
            ErrorHandling.GF_AssertFail();
        }
        return mapMatrix.Data.MapHeaderIDs[index];
    }

    // OFFSET 0x02039E88
    public static int MapMatrix_GetMatrixID(MapMatrix mapMatrix)
    {
        return mapMatrix.MatrixID;
    }

    // OFFSET 0x02039E8C
    public static int MapMatrix_GetAltitudeAtCoords(MapMatrix mapMatrix, int unused, int x, int y, int width)
    {
        if (x >= width)
        {
            ErrorHandling.GF_AssertFail();
        }
        int index = x + (y * width);
        if (index >= MaxSize)
        {
            ErrorHandling.GF_AssertFail();
        }
        return mapMatrix.Data.Altitudes[index];
    }

    // OFFSET 0x02039EBC
    public static MainMapMatrixData MainMapMatrixData_Load(int heapID)
    {
        MainMapMatrixData mainMapMatrixData = new();
        byte[] file = NARC.NARC_AllocAtEndAndReadWholeMemberByIndexPair(NarcMapMatrix, 0, heapID);
        int position = 5 + file[4];
        for (int i = 0; i < MaxSize; i++)
        {
            mainMapMatrixData.MapHeaderIDs[i] = (ushort)Bin.U16(file, position + (i * 2));
        }
        Heap.Heap_Free(file);
        return mainMapMatrixData;
    }

    // OFFSET 0x02039EF0
    public static void MainMapMatrixData_Free(MainMapMatrixData? mainMapMatrixData)
    {
        if (mainMapMatrixData == null)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02039F04
    public static int MainMapMatrixData_GetMapHeaderIDAtCoords(MainMapMatrixData mainMapMatrixData, int x, int y)
    {
        return mainMapMatrixData.MapHeaderIDs[x + (y * MainMatrixWidth)];
    }

    // OFFSET 0x02039F10
    public static int MapMatrixData_GetMapHeaderIDAtCoords(int mapMatrixID, int x, int y)
    {
        MapMatrixData data = new();
        MapMatrixData_Load(data, (ushort)mapMatrixID, 0);
        if (x >= data.Width)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (y >= data.Height)
        {
            ErrorHandling.GF_AssertFail();
        }
        return data.MapHeaderIDs[x + (y * data.Width)];
    }

    // OFFSET 0x02039F74
    public static int MapMatrix_GetLandDataIDByIndex(int index, MapMatrix? mapMatrix)
    {
        if (mapMatrix == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return MapMatrix_GetLandDataIDByIndex2(index, mapMatrix!);
    }

    // OFFSET 0x02039F8C
    public static void MapMatrix_RevealSpringPath(MapMatrix mapMatrix)
    {
        int width = mapMatrix.Width;
        ushort[] landDataIDs = mapMatrix.Data.LandDataIDs;
        byte[] altitudes = mapMatrix.Data.Altitudes;
        if (mapMatrix.MatrixID == 0)
        {
            int upperLeft = (width * 0x15) + 0x17;
            int upperRight = (width * 0x15) + 0x18;
            landDataIDs[upperLeft] = 0xB0;
            landDataIDs[upperRight] = 0xB0;
            int lowerLeft = (width * 0x16) + 0x17;
            int lowerRight = (width * 0x16) + 0x18;
            landDataIDs[lowerLeft] = 0xB0;
            landDataIDs[lowerRight] = 0xB0;
            altitudes[upperLeft] = 2;
            altitudes[upperRight] = 2;
            altitudes[lowerLeft] = 2;
            altitudes[lowerRight] = 2;
        }
    }

    // OFFSET 0x02039FE0
    public static void MapMatrix_RevealSeabreakPath(MapMatrix mapMatrix)
    {
        ushort[] landDataIDs = mapMatrix.Data.LandDataIDs;
        int width = mapMatrix.Width;
        if (mapMatrix.MatrixID == 0)
        {
            landDataIDs[(width * 0xF) + 0x1C] = 0x77;
            landDataIDs[(width * 0x10) + 0x1B] = 0x78;
            landDataIDs[(width * 0x10) + 0x1C] = 0x79;
            landDataIDs[(width * 0x11) + 0x1B] = 0x7A;
        }
    }
}
