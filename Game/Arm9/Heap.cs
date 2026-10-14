using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class HeapAllocator
{
    public ExpHeap? Heap;
    public int Alignment;
}

public static class Heap
{
    private const int ExtraHeapSlots = 0x18;
    private const uint BlockHeaderSize = 0x10;
    private const int ArenaMain = 0;
    private const int ArenaMainEx = 2;
    private const int AlignFromStart = 4;
    private const int AlignFromEnd = -4;

    private static ExpHeap?[] sHeapHandles = Array.Empty<ExpHeap?>();
    private static ExpHeap?[] sParentHeapHandles = Array.Empty<ExpHeap?>();
    private static HeapBlock?[] sSubHeapBlocks = Array.Empty<HeapBlock?>();
    private static ushort[] sNumMemBlocks = Array.Empty<ushort>();
    private static byte[] sHeapIdxs = Array.Empty<byte>();
    private static ushort sTotalNumHeaps;
    private static ushort sNumTemplates;
    private static ushort sMaxHeaps;
    private static ushort sUnallocatedHeapID;

    // OFFSET 0x02017E74
    public static void Heap_InitSystem(uint templatesAddress, uint templateCount, uint totalNumHeaps, uint preSize)
    {
        uint usableHeaps = templateCount + ExtraHeapSlots;
        if (totalNumHeaps < usableHeaps)
        {
            totalNumHeaps = usableHeaps;
        }
        if (preSize != 0)
        {
            while ((preSize & 3) != 0)
            {
                preSize++;
            }
            Platform.ReserveMainArena(preSize);
        }
        sHeapHandles = new ExpHeap?[usableHeaps + 1];
        sParentHeapHandles = new ExpHeap?[usableHeaps];
        sSubHeapBlocks = new HeapBlock?[usableHeaps];
        sNumMemBlocks = new ushort[totalNumHeaps];
        sHeapIdxs = new byte[totalNumHeaps];
        sTotalNumHeaps = (ushort)totalNumHeaps;
        sNumTemplates = (ushort)templateCount;
        sUnallocatedHeapID = (ushort)usableHeaps;
        sMaxHeaps = (ushort)usableHeaps;
        uint[] templates = RomData.Arm9U32(templatesAddress, (int)templateCount * 2);
        uint i;
        for (i = 0; i < templateCount; i++)
        {
            uint size = templates[i * 2];
            uint arena = templates[i * 2 + 1];
            bool reserved;
            switch (arena)
            {
                case ArenaMainEx:
                    reserved = Platform.ReserveMainExArena(size);
                    break;
                case ArenaMain:
                default:
                    reserved = Platform.ReserveMainArena(size);
                    break;
            }
            if (reserved)
            {
                sHeapHandles[i] = ExpHeap.Create(size);
                sHeapIdxs[i] = (byte)i;
            }
            else
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        for (i = templateCount; i < usableHeaps + 1; i++)
        {
            sHeapHandles[i] = null;
            sHeapIdxs[i] = (byte)sUnallocatedHeapID;
        }
        for (; i < totalNumHeaps; i++)
        {
            sHeapIdxs[i] = (byte)sUnallocatedHeapID;
        }
        for (i = 0; i < totalNumHeaps; i++)
        {
            sNumMemBlocks[i] = 0;
        }
    }

    // OFFSET 0x02017FA0
    private static int Heap_FindEmptyHandleIndex()
    {
        for (int i = sNumTemplates; i < sMaxHeaps; i++)
        {
            if (sHeapHandles[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    // OFFSET 0x02017FC8
    public static bool Heap_Create(int parent, int child, uint size)
    {
        return Heap_CreateInternal(parent, child, size, AlignFromStart);
    }

    // OFFSET 0x02017FD4
    public static bool Heap_CreateAtEnd(int parent, int child, uint size)
    {
        return Heap_CreateInternal(parent, child, size, AlignFromEnd);
    }

    // OFFSET 0x02017FE0
    private static bool Heap_CreateInternal(int parent, int child, uint size, int alignment)
    {
        if (Platform.InInterrupt)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (sUnallocatedHeapID == sHeapIdxs[child])
        {
            ExpHeap? parentHeap = sHeapHandles[sHeapIdxs[parent]];
            if (parentHeap != null)
            {
                HeapBlock? block = parentHeap.Alloc(size, alignment, 0);
                if (block != null)
                {
                    int index = Heap_FindEmptyHandleIndex();
                    if (index >= 0)
                    {
                        sHeapHandles[index] = ExpHeap.Create(size);
                        if (sHeapHandles[index] != null)
                        {
                            sParentHeapHandles[index] = parentHeap;
                            sSubHeapBlocks[index] = block;
                            sHeapIdxs[child] = (byte)index;
                            return true;
                        }
                        ErrorHandling.GF_AssertFail();
                    }
                    else
                    {
                        ErrorHandling.GF_AssertFail();
                    }
                }
                else
                {
                    ErrorHandling.GF_AssertFail();
                }
            }
            else
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        return false;
    }

    // OFFSET 0x0201807C
    public static void Heap_Destroy(int heapID)
    {
        if (Platform.InInterrupt)
        {
            ErrorHandling.GF_AssertFail();
        }
        int index = sHeapIdxs[heapID];
        ExpHeap? heap = sHeapHandles[index];
        if (heap != null)
        {
            heap.Destroy();
            ExpHeap? parentHeap = sParentHeapHandles[index];
            HeapBlock? block = sSubHeapBlocks[index];
            if (parentHeap != null && block != null)
            {
                parentHeap.Free(block);
            }
            else
            {
                ErrorHandling.GF_AssertFail();
            }
            sHeapHandles[index] = null;
            sParentHeapHandles[index] = null;
            sSubHeapBlocks[index] = null;
            sHeapIdxs[heapID] = (byte)sUnallocatedHeapID;
        }
    }

    // OFFSET 0x020180F0
    private static byte[]? Heap_AllocInternal(ExpHeap? heap, uint size, int alignment, int heapID)
    {
        if (heap == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        HeapBlock? block = heap!.Alloc(size + BlockHeaderSize, alignment, size);
        if (block == null)
        {
            return null;
        }
        block.GroupID = (byte)heapID;
        return block.Memory;
    }

    // OFFSET 0x02018134
    private static void Heap_AllocFailed()
    {
        if (CommManager.CommMan_IsInitialized() != 0)
        {
            sub_0209B49C();
        }
    }

    // OFFSET 0x02018144
    public static byte[]? Heap_Alloc(int heapID, uint size)
    {
        byte[]? memory = null;
        if ((uint)heapID < sTotalNumHeaps)
        {
            memory = Heap_AllocInternal(sHeapHandles[sHeapIdxs[heapID]], size, AlignFromStart, heapID);
        }
        if (memory != null)
        {
            sNumMemBlocks[heapID]++;
        }
        else
        {
            Heap_AllocFailed();
        }
        return memory;
    }

    // OFFSET 0x02018184
    public static byte[]? Heap_AllocAtEnd(int heapID, uint size)
    {
        byte[]? memory = null;
        if ((uint)heapID < sTotalNumHeaps)
        {
            memory = Heap_AllocInternal(sHeapHandles[sHeapIdxs[heapID]], size, AlignFromEnd, heapID);
        }
        if (memory != null)
        {
            sNumMemBlocks[heapID]++;
        }
        else
        {
            Heap_AllocFailed();
        }
        return memory;
    }

    // OFFSET 0x020181C4
    public static void Heap_Free(byte[] memory)
    {
        HeapBlock? block = ExpHeap.FindBlock(memory);
        int heapID = block?.GroupID ?? sTotalNumHeaps;
        if (heapID < sTotalNumHeaps)
        {
            ExpHeap? heap = sHeapHandles[sHeapIdxs[heapID]];
            if (heap == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            if (sNumMemBlocks[heapID] == 0)
            {
                GF_heap_c_dummy_return_true(heapID);
            }
            if (sNumMemBlocks[heapID] == 0)
            {
                ErrorHandling.GF_AssertFail();
            }
            sNumMemBlocks[heapID]--;
            heap!.Free(block!);
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02018238
    public static void Heap_FreeExplicit(int heapID, byte[] memory)
    {
        if (Platform.InInterrupt)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((uint)heapID < sTotalNumHeaps)
        {
            ExpHeap? heap = sHeapHandles[sHeapIdxs[heapID]];
            if (heap == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            HeapBlock? block = ExpHeap.FindBlock(memory);
            if (block == null || block.GroupID != heapID)
            {
                ErrorHandling.GF_AssertFail();
            }
            heap!.Free(block!);
            if (sNumMemBlocks[heapID] == 0)
            {
                ErrorHandling.GF_AssertFail();
            }
            sNumMemBlocks[heapID]--;
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x020182A4
    public static uint HeapExp_FndGetTotalFreeSize(int heapID)
    {
        if ((uint)heapID < sTotalNumHeaps)
        {
            return sHeapHandles[sHeapIdxs[heapID]]!.GetTotalFreeSize();
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x020182CC
    public static void HeapExp_FndInitAllocator(HeapAllocator allocator, int heapID, int alignment)
    {
        if ((uint)heapID < sTotalNumHeaps)
        {
            allocator.Heap = sHeapHandles[sHeapIdxs[heapID]];
            allocator.Alignment = alignment;
            return;
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x020182F0
    public static void Heap_Realloc(ref byte[] memory, uint newSize)
    {
        if (Platform.InInterrupt)
        {
            ErrorHandling.GF_AssertFail();
        }
        HeapBlock? block = ExpHeap.FindBlock(memory);
        if (block != null && block.Size >= newSize + BlockHeaderSize)
        {
            ExpHeap heap = sHeapHandles[sHeapIdxs[block.GroupID]]!;
            heap.Resize(block, newSize + BlockHeaderSize);
            return;
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x0201833C
    public static bool GF_heap_c_dummy_return_true(int heapID)
    {
        return true;
    }
}
