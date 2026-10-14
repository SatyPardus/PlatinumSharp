namespace PlatinumSharp.Game;

public sealed class GfdFreeBlock
{
    public uint Address;
    public uint Size;
    public GfdFreeBlock? Prev;
    public GfdFreeBlock? Next;
}

public static class NnsGfd
{
    private const int FrmTexRegionCount = 5;
    private const int FrmTex4x4RegionCount = 2;
    private const uint MaxTexSize = 0x7FFF0;
    private const uint MaxPlttSize = 0x7FFF8;
    private const uint Pltt4ColorLimit = 0x10000;
    private const int AllocFromLow = 1;
    private const int LnkTexRegionCount = 4;
    private const uint TexSlotSize = 0x20000;
    private const int BlockSize = 16;

    private sealed class FrmTexRegion
    {
        public uint Head;
        public uint Tail;
        public int Active;
        public int IsIndexRegion;
        public ushort Type;
        public uint BaseAddress;
    }

    private struct LnkTexRegion
    {
        public uint Remaining;
        public uint NormalSize;
        public uint Compressed4x4Size;
    }

    private static readonly FrmTexRegion[] sFrmTexRegions =
    {
        new() { Head = 0xFFFFFFFF, Tail = 0xFFFFFFFF, IsIndexRegion = 0, Type = 0, BaseAddress = 0x00000 },
        new() { Head = 0xFFFFFFFF, Tail = 0xFFFFFFFF, IsIndexRegion = 1, Type = 1, BaseAddress = 0x20000 },
        new() { Head = 0xFFFFFFFF, Tail = 0xFFFFFFFF, IsIndexRegion = 1, Type = 2, BaseAddress = 0x30000 },
        new() { Head = 0xFFFFFFFF, Tail = 0xFFFFFFFF, IsIndexRegion = 0, Type = 3, BaseAddress = 0x40000 },
        new() { Head = 0xFFFFFFFF, Tail = 0xFFFFFFFF, IsIndexRegion = 0, Type = 4, BaseAddress = 0x60000 },
    };

    private static readonly FrmTexRegion[] sFrmTex4x4Order = { sFrmTexRegions[0], sFrmTexRegions[3] };
    private static readonly FrmTexRegion[] sFrmTexNormalOrder = new FrmTexRegion[FrmTexRegionCount];
    private static ushort sFrmTexNumSlots;
    private static uint sFrmPlttHead;
    private static uint sFrmPlttTail;
    private static uint sFrmPlttSize;
    private static GfdFreeBlock? sLnkTexNormalList;
    private static GfdFreeBlock? sLnkTex4x4List;
    private static GfdFreeBlock? sLnkTexBlockPool;
    private static uint sLnkTexSize;
    private static uint sLnkTexSizeFor4x4;
    private static uint sLnkTexWorkSize;
    private static GfdFreeBlock? sLnkPlttList;
    private static GfdFreeBlock? sLnkPlttBlockPool;
    private static uint sLnkPlttSize;
    private static uint sLnkPlttWorkSize;

    private static uint MakeTexKey(uint address, uint size, int is4x4Comp)
    {
        return ((address << 13) >> 16) | ((size >> 4) << 16) | ((uint)is4x4Comp << 31);
    }

    private static uint MakePlttKey(uint address, uint size)
    {
        return ((address << 13) >> 16) | ((size >> 3) << 16);
    }

    // OFFSET 0x020A5A4C
    private static void FrmTex_SetNormalOrder(int first, int second, int third, int fourth, int fifth)
    {
        sFrmTexNormalOrder[0] = sFrmTexRegions[first];
        sFrmTexNormalOrder[1] = sFrmTexRegions[second];
        sFrmTexNormalOrder[2] = sFrmTexRegions[third];
        sFrmTexNormalOrder[3] = sFrmTexRegions[fourth];
        sFrmTexNormalOrder[4] = sFrmTexRegions[fifth];
    }

    // OFFSET 0x020A5A94
    public static void InitFrmTexVramManager(ushort numSlots, bool useAsDefault)
    {
        if (numSlots <= 2)
        {
            FrmTex_SetNormalOrder(4, 3, 2, 0, 1);
        }
        else
        {
            FrmTex_SetNormalOrder(4, 3, 0, 2, 1);
        }
        sFrmTexNumSlots = numSlots;
        ResetFrmTexVramState();
        if (useAsDefault)
        {
            Gfd.AllocTexVram = AllocFrmTexVram;
            Gfd.FreeTexVram = FreeFrmTexVram;
        }
    }

    // OFFSET 0x020A5B1C
    public static void ResetFrmTexVramState()
    {
        int activeRegions = sFrmTexNumSlots;
        if (activeRegions > 1)
        {
            activeRegions++;
        }
        for (int i = 0; i < FrmTexRegionCount; i++)
        {
            FrmTexRegion region = sFrmTexRegions[i];
            if (i < activeRegions)
            {
                region.Active = 1;
            }
            else
            {
                region.Active = 0;
            }
            if (region.IsIndexRegion != 0)
            {
                region.Head = 0;
                region.Tail = TexSlotSize / 2;
            }
            else
            {
                region.Head = 0;
                region.Tail = TexSlotSize;
            }
        }
    }

    // OFFSET 0x020A5B90
    private static uint AllocFrmTexVram(uint size, int is4x4Comp, uint option)
    {
        if (size == 0)
        {
            size = 0x10;
        }
        else
        {
            size = (size + 0xF) & ~0xFu;
        }
        if (size >= MaxTexSize)
        {
            return Gfd.InvalidKey;
        }
        bool allocated = false;
        uint address = 0;
        if (is4x4Comp != 0)
        {
            for (int i = 0; i < FrmTex4x4RegionCount; i++)
            {
                FrmTexRegion region = sFrmTex4x4Order[i];
                if (region.Active == 0 || region.Tail - region.Head < size)
                {
                    continue;
                }
                FrmTexRegion? indexRegion;
                switch (region.Type)
                {
                    case 0:
                        indexRegion = sFrmTexRegions[1];
                        break;
                    case 3:
                        indexRegion = sFrmTexRegions[2];
                        break;
                    default:
                        indexRegion = null;
                        break;
                }
                if (indexRegion!.Active == 0 || indexRegion.Tail - indexRegion.Head < size >> 1)
                {
                    continue;
                }
                uint head = region.Head;
                region.Head = head + size;
                indexRegion.Head += size >> 1;
                address = head + region.BaseAddress;
                allocated = true;
                break;
            }
        }
        else
        {
            for (int i = 0; i < FrmTexRegionCount; i++)
            {
                FrmTexRegion region = sFrmTexNormalOrder[i];
                if (region.Active == 0 || region.Tail - region.Head < size)
                {
                    continue;
                }
                uint tail = region.Tail - size;
                region.Tail = tail;
                address = tail + region.BaseAddress;
                allocated = true;
                break;
            }
        }
        if (!allocated)
        {
            return Gfd.InvalidKey;
        }
        return MakeTexKey(address, size, is4x4Comp);
    }

    // OFFSET 0x020A5D00
    private static int FreeFrmTexVram(uint key)
    {
        return 0;
    }

    // OFFSET 0x020A5D08
    public static void GetFrmTexVramState(uint[] state)
    {
        for (int i = 0; i < FrmTexRegionCount; i++)
        {
            state[i * 2] = sFrmTexRegions[i].Head;
            state[i * 2 + 1] = sFrmTexRegions[i].Tail;
        }
    }

    // OFFSET 0x020A5D48
    public static void SetFrmTexVramState(uint[] state)
    {
        for (int i = 0; i < FrmTexRegionCount; i++)
        {
            sFrmTexRegions[i].Head = state[i * 2];
            sFrmTexRegions[i].Tail = state[i * 2 + 1];
        }
    }

    // OFFSET 0x020A5D88
    public static void InitFrmPlttVramManager(uint size, bool useAsDefault)
    {
        sFrmPlttSize = size;
        ResetFrmPlttVramState();
        if (useAsDefault)
        {
            Gfd.AllocPlttVram = AllocFrmPlttVram;
            Gfd.FreePlttVram = FreeFrmPlttVram;
        }
    }

    // OFFSET 0x020A5DD4
    private static uint AllocFrmPlttVram(uint size, int is4Pltt, uint option)
    {
        uint address = 0;
        bool allocated;
        if (size == 0)
        {
            size = 8;
        }
        else
        {
            size = (size + 7) & ~7u;
        }
        if (size >= MaxPlttSize)
        {
            return Gfd.InvalidKey;
        }
        if (option == AllocFromLow)
        {
            uint head = sFrmPlttHead;
            uint padding;
            if (is4Pltt == 0)
            {
                padding = (0x10 - (head & 0xF)) & 0xF;
            }
            else
            {
                padding = (8 - (head & 7)) & 7;
            }
            uint total = size + padding;
            if (sFrmPlttTail - head < total)
            {
                allocated = false;
            }
            else if (is4Pltt != 0 && head + total > Pltt4ColorLimit)
            {
                allocated = false;
            }
            else
            {
                address = head + padding;
                sFrmPlttHead += total;
                allocated = true;
            }
        }
        else
        {
            uint tail = sFrmPlttTail;
            allocated = false;
            if (tail >= size)
            {
                uint padding;
                if (is4Pltt != 0)
                {
                    padding = (tail - size) & 7;
                }
                else
                {
                    padding = (tail - size) & 0xF;
                }
                uint total = size + padding;
                if (tail - sFrmPlttHead >= total)
                {
                    if (!(is4Pltt != 0 && tail > Pltt4ColorLimit))
                    {
                        address = sFrmPlttTail - total;
                        sFrmPlttTail = address;
                        allocated = true;
                    }
                }
            }
        }
        if (!allocated)
        {
            return Gfd.InvalidKey;
        }
        return MakePlttKey(address, size);
    }

    // OFFSET 0x020A5F10
    private static int FreeFrmPlttVram(uint key)
    {
        return 0;
    }

    // OFFSET 0x020A5F18
    public static void GetFrmPlttVramState(uint[] state)
    {
        state[0] = sFrmPlttHead;
        state[1] = sFrmPlttTail;
    }

    // OFFSET 0x020A5F34
    public static void SetFrmPlttVramState(uint[] state)
    {
        sFrmPlttHead = state[0];
        sFrmPlttTail = state[1];
    }

    // OFFSET 0x020A5F50
    public static void ResetFrmPlttVramState()
    {
        sFrmPlttHead = 0;
        sFrmPlttTail = sFrmPlttSize;
    }

    private static void Unlink(ref GfdFreeBlock? list, GfdFreeBlock block)
    {
        GfdFreeBlock? prev = block.Prev;
        GfdFreeBlock? next = block.Next;
        if (prev != null)
        {
            prev.Next = next;
        }
        else
        {
            list = next;
        }
        if (next != null)
        {
            next.Prev = prev;
        }
    }

    private static void PushFront(ref GfdFreeBlock? list, GfdFreeBlock block)
    {
        if (list != null)
        {
            list.Prev = block;
        }
        block.Next = list;
        block.Prev = null;
        list = block;
    }

    private static GfdFreeBlock? PopFront(ref GfdFreeBlock? pool)
    {
        GfdFreeBlock? block = pool;
        if (block != null)
        {
            pool = block.Next;
        }
        return block;
    }

    // OFFSET 0x020A6424
    private static uint FreeList_MergeRegion(ref GfdFreeBlock? list, ref GfdFreeBlock? pool, ref uint begin, ref uint end)
    {
        uint merged = 0;
        GfdFreeBlock? block = list;
        while (block != null)
        {
            GfdFreeBlock? next = block.Next;
            if (block.Address == end)
            {
                merged |= 1;
                end = block.Address + block.Size;
                Unlink(ref list, block);
                PushFront(ref pool, block);
            }
            if (block.Address + block.Size == begin)
            {
                begin = block.Address;
                Unlink(ref list, block);
                merged |= 1;
                PushFront(ref pool, block);
            }
            block = next;
        }
        return merged;
    }

    // OFFSET 0x020A6508
    private static void FreeList_Init(ref GfdFreeBlock? list)
    {
        list = null;
    }

    // OFFSET 0x020A6514
    private static GfdFreeBlock? BlockPool_Init(uint count)
    {
        if (count == 0)
        {
            return null;
        }
        GfdFreeBlock[] blocks = new GfdFreeBlock[count];
        for (uint i = 0; i < count; i++)
        {
            blocks[i] = new GfdFreeBlock();
        }
        for (uint i = 0; i < count - 1; i++)
        {
            blocks[i].Next = blocks[i + 1];
            blocks[i + 1].Prev = blocks[i];
        }
        blocks[0].Prev = null;
        blocks[count - 1].Next = null;
        return blocks[0];
    }

    // OFFSET 0x020A6560
    private static bool FreeList_Add(ref GfdFreeBlock? list, ref GfdFreeBlock? pool, uint address, uint size)
    {
        GfdFreeBlock? block = PopFront(ref pool);
        if (block == null)
        {
            return false;
        }
        block.Address = address;
        block.Size = size;
        block.Prev = null;
        block.Next = null;
        PushFront(ref list, block);
        return true;
    }

    // OFFSET 0x020A65BC
    private static bool FreeList_Alloc(ref GfdFreeBlock? list, ref GfdFreeBlock? pool, out uint address, uint size)
    {
        return FreeList_AllocAligned(ref list, ref pool, out address, size, 0);
    }

    // OFFSET 0x020A65D0
    private static bool FreeList_AllocAligned(ref GfdFreeBlock? list, ref GfdFreeBlock? pool, out uint address, uint size, uint alignment)
    {
        GfdFreeBlock? found = null;
        uint aligned = 0;
        uint padding = 0;
        uint needed = 0;
        for (GfdFreeBlock? block = list; block != null; block = block.Next)
        {
            if (alignment <= 1)
            {
                aligned = block.Address;
                padding = 0;
                needed = size;
            }
            else
            {
                aligned = (block.Address + (alignment - 1)) & ~(alignment - 1);
                padding = aligned - block.Address;
                needed = size + padding;
            }
            if (block.Size >= needed)
            {
                found = block;
                break;
            }
        }
        if (found == null)
        {
            address = 0;
            return false;
        }
        if (padding != 0)
        {
            GfdFreeBlock? remainder = PopFront(ref pool);
            if (remainder == null)
            {
                address = 0;
                return false;
            }
            remainder.Address = found.Address;
            remainder.Size = padding;
            remainder.Prev = null;
            remainder.Next = null;
            PushFront(ref list, remainder);
        }
        found.Size -= needed;
        found.Address += needed;
        if (found.Size == 0)
        {
            Unlink(ref list, found);
            PushFront(ref pool, found);
        }
        address = aligned;
        return true;
    }

    // OFFSET 0x020A6710
    private static void FreeList_MergeAll(ref GfdFreeBlock? list, ref GfdFreeBlock? pool)
    {
        GfdFreeBlock? block = list;
        while (block != null)
        {
            uint begin = block.Address;
            uint end = block.Address + block.Size;
            if (FreeList_MergeRegion(ref list, ref pool, ref begin, ref end) == 0)
            {
                block = block.Next;
            }
            else
            {
                block.Address = begin;
                block.Size = end - begin;
                block = list;
            }
        }
    }

    // OFFSET 0x020A6794
    private static bool FreeList_Free(ref GfdFreeBlock? list, ref GfdFreeBlock? pool, uint address, uint size)
    {
        uint begin = address;
        uint end = address + size;
        FreeList_MergeRegion(ref list, ref pool, ref begin, ref end);
        GfdFreeBlock? block = PopFront(ref pool);
        if (block == null)
        {
            return false;
        }
        block.Address = begin;
        block.Size = end - begin;
        block.Prev = null;
        block.Next = null;
        PushFront(ref list, block);
        return true;
    }

    // OFFSET 0x020A6824
    public static uint GetLnkTexVramManagerWorkSize(uint numBlocks)
    {
        return numBlocks << 4;
    }

    // OFFSET 0x020A682C
    public static void InitLnkTexVramManager(uint size, uint sizeFor4x4, byte[] work, uint workSize, bool useAsDefault)
    {
        sLnkTexSize = size;
        sLnkTexSizeFor4x4 = sizeFor4x4;
        sLnkTexWorkSize = workSize;
        ResetLnkTexVramState();
        if (useAsDefault)
        {
            Gfd.AllocTexVram = AllocLnkTexVram;
            Gfd.FreeTexVram = FreeLnkTexVram;
        }
    }

    // OFFSET 0x020A6884
    private static uint AllocLnkTexVram(uint size, int is4x4Comp, uint option)
    {
        if (size == 0)
        {
            size = 0x10;
        }
        else
        {
            size = (size + 0xF) & ~0xFu;
        }
        if (size >= MaxTexSize)
        {
            return Gfd.InvalidKey;
        }
        bool allocated;
        uint address;
        if (is4x4Comp != 0)
        {
            allocated = FreeList_Alloc(ref sLnkTex4x4List, ref sLnkTexBlockPool, out address, size);
        }
        else
        {
            allocated = FreeList_Alloc(ref sLnkTexNormalList, ref sLnkTexBlockPool, out address, size);
        }
        if (!allocated)
        {
            return Gfd.InvalidKey;
        }
        return MakeTexKey(address, size, is4x4Comp);
    }

    // OFFSET 0x020A6914
    public static int FreeLnkTexVram(uint key)
    {
        uint size = ((key & 0x7FFF0000) >> 16) << 4;
        uint address = (key << 16) >> 13;
        uint is4x4Comp = (key & 0x80000000) >> 31;
        if (size == 0)
        {
            return 2;
        }
        bool freed;
        if (is4x4Comp != 0)
        {
            freed = FreeList_Free(ref sLnkTex4x4List, ref sLnkTexBlockPool, address, size);
        }
        else
        {
            freed = FreeList_Free(ref sLnkTexNormalList, ref sLnkTexBlockPool, address, size);
        }
        return freed ? 0 : 1;
    }

    // OFFSET 0x020A6988
    public static void ResetLnkTexVramState()
    {
        LnkTexRegion[] regions = new LnkTexRegion[LnkTexRegionCount];
        for (int i = 0; i < LnkTexRegionCount; i++)
        {
            regions[i].Remaining = TexSlotSize;
        }
        uint remaining4x4 = sLnkTexSizeFor4x4;
        uint indexSize = sLnkTexSizeFor4x4 >> 1;
        uint remainingNormal = sLnkTexSize - (sLnkTexSizeFor4x4 + (sLnkTexSizeFor4x4 >> 1));
        for (int i = 0; i < LnkTexRegionCount; i++)
        {
            if (i != 0 && i != 2)
            {
                continue;
            }
            if (regions[i].Remaining != 0 && remaining4x4 != 0)
            {
                uint taken = regions[i].Remaining;
                if (taken > remaining4x4)
                {
                    taken = remaining4x4;
                }
                regions[i].Compressed4x4Size += taken;
                remaining4x4 -= taken;
                regions[i].Remaining -= taken;
            }
        }
        regions[1].Remaining -= indexSize;
        for (int i = 0; i < LnkTexRegionCount; i++)
        {
            if (regions[i].Remaining != 0 && remainingNormal != 0)
            {
                uint taken = regions[i].Remaining;
                if (taken > remainingNormal)
                {
                    taken = remainingNormal;
                }
                regions[i].NormalSize += taken;
                remainingNormal -= taken;
                regions[i].Remaining -= taken;
            }
        }
        FreeList_Init(ref sLnkTexNormalList);
        FreeList_Init(ref sLnkTex4x4List);
        sLnkTexBlockPool = BlockPool_Init(sLnkTexWorkSize >> 4);
        if (regions[0].Compressed4x4Size != 0)
        {
            FreeList_Add(ref sLnkTex4x4List, ref sLnkTexBlockPool, 0, regions[0].Compressed4x4Size);
        }
        if (regions[0].NormalSize != 0)
        {
            FreeList_Add(ref sLnkTexNormalList, ref sLnkTexBlockPool, regions[0].Compressed4x4Size, regions[0].NormalSize);
        }
        if (regions[2].Compressed4x4Size != 0)
        {
            FreeList_Add(ref sLnkTex4x4List, ref sLnkTexBlockPool, 0x40000, regions[2].Compressed4x4Size);
        }
        if (regions[2].NormalSize != 0)
        {
            FreeList_Add(ref sLnkTexNormalList, ref sLnkTexBlockPool, regions[2].Compressed4x4Size + 0x40000, regions[2].NormalSize);
        }
        if (regions[3].NormalSize != 0)
        {
            FreeList_Add(ref sLnkTexNormalList, ref sLnkTexBlockPool, 0x60000, regions[3].NormalSize);
        }
        if (regions[1].NormalSize != 0)
        {
            FreeList_Add(ref sLnkTexNormalList, ref sLnkTexBlockPool, indexSize + 0x20000, regions[1].NormalSize);
        }
        FreeList_MergeAll(ref sLnkTexNormalList, ref sLnkTexBlockPool);
        FreeList_MergeAll(ref sLnkTex4x4List, ref sLnkTexBlockPool);
    }

    // OFFSET 0x020A6B84
    public static uint GetLnkPlttVramManagerWorkSize(uint numBlocks)
    {
        return numBlocks << 4;
    }

    // OFFSET 0x020A6B8C
    public static void InitLnkPlttVramManager(uint size, byte[] work, uint workSize, bool useAsDefault)
    {
        sLnkPlttSize = size;
        sLnkPlttWorkSize = workSize;
        ResetLnkPlttVramState();
        if (useAsDefault)
        {
            Gfd.AllocPlttVram = AllocLnkPlttVram;
            Gfd.FreePlttVram = FreeLnkPlttVram;
        }
    }

    // OFFSET 0x020A6BE0
    private static uint AllocLnkPlttVram(uint size, int is4Pltt, uint option)
    {
        if (size == 0)
        {
            size = 8;
        }
        else
        {
            size = (size + 7) & ~7u;
        }
        if (size >= MaxPlttSize)
        {
            return Gfd.InvalidKey;
        }
        bool allocated;
        uint address;
        if (is4Pltt != 0)
        {
            allocated = FreeList_AllocAligned(ref sLnkPlttList, ref sLnkPlttBlockPool, out address, size, 8);
            if (address + size > Pltt4ColorLimit)
            {
                FreeList_Free(ref sLnkPlttList, ref sLnkPlttBlockPool, address, size);
                return Gfd.InvalidKey;
            }
        }
        else
        {
            allocated = FreeList_AllocAligned(ref sLnkPlttList, ref sLnkPlttBlockPool, out address, size, 16);
        }
        if (!allocated)
        {
            return Gfd.InvalidKey;
        }
        return MakePlttKey(address, size);
    }

    // OFFSET 0x020A6CAC
    public static int FreeLnkPlttVram(uint key)
    {
        uint size = ((key & 0xFFFF0000) >> 16) << 3;
        uint address = (key << 16) >> 13;
        bool freed = FreeList_Free(ref sLnkPlttList, ref sLnkPlttBlockPool, address, size);
        return freed ? 0 : 1;
    }

    // OFFSET 0x020A6CF0
    public static void ResetLnkPlttVramState()
    {
        sLnkPlttBlockPool = BlockPool_Init(sLnkPlttWorkSize >> 4);
        FreeList_Init(ref sLnkPlttList);
        FreeList_Add(ref sLnkPlttList, ref sLnkPlttBlockPool, 0, sLnkPlttSize);
        FreeList_MergeAll(ref sLnkPlttList, ref sLnkPlttBlockPool);
    }
}
