namespace PlatinumSharp.Game;

public sealed class LoadedMapBuffers
{
    public const int FileCount = 4;
    public const int AllocTypeStatic = 1;
    public const int AllocTypeHeap = 2;
    public const int MapModelFileSize = 0xF000;
    public const int BdhcFileSize = 0x9000;

    public ushort MapModelAllocType;
    public ushort BdhcAllocType;
    public readonly byte[]?[] MapModelFiles = new byte[]?[FileCount];
    public readonly byte[]?[] BdhcFiles = new byte[]?[FileCount];

    private const int FieldHeapID = 4;

    private static readonly byte[][] sStaticMapModelFiles = CreateStaticMapModelFiles();

    private static byte[][] CreateStaticMapModelFiles()
    {
        byte[][] files = new byte[FileCount][];
        for (int i = 0; i < FileCount; i++)
        {
            files[i] = new byte[MapModelFileSize];
        }
        return files;
    }

    // OFFSET 0x021EEAC8
    public static LoadedMapBuffers? LoadedMapBuffers_New(bool allocBDHCFiles)
    {
        LoadedMapBuffers buffers = new();
        buffers.MapModelAllocType = AllocTypeStatic;
        if (allocBDHCFiles)
        {
            buffers.BdhcAllocType = AllocTypeHeap;
        }
        for (byte i = 0; i < FileCount; i++)
        {
            buffers.MapModelFiles[i] = sStaticMapModelFiles[i];
            if (buffers.MapModelFiles[i] == null)
            {
                return null;
            }
            int bdhcFileSize;
            if (allocBDHCFiles)
            {
                buffers.BdhcFiles[i] = Heap.Heap_Alloc(FieldHeapID, BdhcFileSize);
                if (buffers.BdhcFiles[i] == null)
                {
                    return null;
                }
                bdhcFileSize = BdhcFileSize;
            }
            else
            {
                buffers.BdhcFiles[i] = null;
                bdhcFileSize = 0;
            }
            LoadedMapBuffers_InitFileBuffers(i, MapModelFileSize, bdhcFileSize, buffers);
        }
        return buffers;
    }

    // OFFSET 0x021EEB48
    public static void LoadedMapBuffers_Free(LoadedMapBuffers buffers)
    {
        for (int i = 0; i < FileCount; i++)
        {
            if (buffers.MapModelAllocType == AllocTypeHeap)
            {
                Heap.Heap_Free(buffers.MapModelFiles[i]!);
            }
            buffers.MapModelFiles[i] = null;
            if (buffers.BdhcFiles[i] != null)
            {
                if (buffers.BdhcAllocType == AllocTypeHeap)
                {
                    Heap.Heap_Free(buffers.BdhcFiles[i]!);
                }
                buffers.BdhcFiles[i] = null;
            }
        }
    }

    // OFFSET 0x021EEB84
    public static void LoadedMapBuffers_GetMapModelFileBufPtr(byte index, LoadedMapBuffers buffers, out byte[]? mapModelFile)
    {
        mapModelFile = buffers.MapModelFiles[index];
    }

    // OFFSET 0x021EEB90
    public static void LoadedMapBuffers_GetBDHCFileBufPtr(byte index, LoadedMapBuffers buffers, out byte[]? bdhcFile)
    {
        bdhcFile = buffers.BdhcFiles[index];
    }

    // OFFSET 0x021EEB9C
    private static void LoadedMapBuffers_InitFileBuffers(byte index, int mapModelFileSize, int bdhcFileSize, LoadedMapBuffers buffers)
    {
        Array.Clear(buffers.MapModelFiles[index]!, 0, mapModelFileSize);
        if (bdhcFileSize != 0)
        {
            Array.Clear(buffers.BdhcFiles[index]!, 0, bdhcFileSize);
        }
    }

    // OFFSET 0x021EEBC0
    public static LoadedMapBuffers LoadedMapBuffers_NewWithHeapMapModelFiles(int allocBDHCFiles)
    {
        LoadedMapBuffers buffers = new();
        buffers.MapModelAllocType = AllocTypeHeap;
        if (allocBDHCFiles != 0)
        {
            buffers.BdhcAllocType = AllocTypeHeap;
        }
        for (uint i = 0; i < FileCount; i++)
        {
            buffers.MapModelFiles[i] = Heap.Heap_Alloc(FieldHeapID, MapModelFileSize);
            Array.Clear(buffers.MapModelFiles[i]!, 0, MapModelFileSize);
            if (allocBDHCFiles == 1)
            {
                buffers.BdhcFiles[i] = Heap.Heap_Alloc(FieldHeapID, BdhcFileSize);
                Array.Clear(buffers.BdhcFiles[i]!, 0, BdhcFileSize);
            }
        }
        return buffers;
    }

    // OFFSET 0x021EEC24
    public static void LoadedMapBuffers_SwapMapModelData(LoadedMapBuffers a, LoadedMapBuffers b)
    {
        ushort allocType = a.MapModelAllocType;
        byte[]?[] files = new byte[]?[FileCount];
        Array.Copy(a.MapModelFiles, files, FileCount);
        a.MapModelAllocType = b.MapModelAllocType;
        b.MapModelAllocType = allocType;
        for (int i = 0; i < FileCount; i++)
        {
            a.MapModelFiles[i] = b.MapModelFiles[i];
            b.MapModelFiles[i] = files[i];
        }
    }
}
