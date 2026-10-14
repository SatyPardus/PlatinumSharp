namespace PlatinumSharp.Game;

public sealed class ResourceHeapItem
{
    public byte[]? Data;
    public uint ID;
}

public sealed class ResourceHeap
{
    private const int HeaderSize = 0x18;
    private const int ItemSize = 8;

    public int ParentHeapID;
    public int HeapID;
    public uint ItemCount;
    public uint HeapSize;
    public uint ItemsSize;
    public ResourceHeapItem[] Items = Array.Empty<ResourceHeapItem>();

    // OFFSET 0x021F067C
    public static ResourceHeap ResourceHeap_New(int parentHeapID, int heapID, uint itemPoolSize, uint itemCount)
    {
        uint itemsSize = itemCount * ItemSize;
        uint heapSize = itemPoolSize + itemsSize + HeaderSize;
        if (!Heap.Heap_Create(parentHeapID, heapID, heapSize))
        {
            ErrorHandling.GF_AssertFail();
        }
        ResourceHeap resourceHeap = new();
        resourceHeap.ParentHeapID = parentHeapID;
        resourceHeap.HeapID = heapID;
        resourceHeap.ItemCount = itemCount;
        resourceHeap.HeapSize = heapSize;
        resourceHeap.ItemsSize = itemsSize;
        resourceHeap.Items = new ResourceHeapItem[itemCount];
        for (uint i = 0; i < itemCount; i++)
        {
            resourceHeap.Items[i] = new ResourceHeapItem();
        }
        return resourceHeap;
    }

    // OFFSET 0x021F06D8
    public static void ResourceHeap_Free(ResourceHeap resourceHeap)
    {
        for (uint i = 0; i < resourceHeap.ItemCount; i++)
        {
            ResourceHeapItem item = resourceHeap.Items[i];
            if (item.Data != null)
            {
                ResourceHeap_FreeItem(resourceHeap, item.ID);
            }
        }
        int heapID = resourceHeap.HeapID;
        Heap.Heap_Destroy(heapID);
    }

    // OFFSET 0x021F070C
    public static byte[] ResourceHeap_AllocItem(ResourceHeap resourceHeap, uint id, uint dataSize, int allocAtEnd)
    {
        ResourceHeapItem? item = ResourceHeap_FindFreeItem(resourceHeap);
        if (item == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte[] data = ResourceHeap_AllocItemData(resourceHeap, dataSize, allocAtEnd);
        ResourceHeapItem_Init(item!, id, data);
        return data;
    }

    // OFFSET 0x021F0740
    public static void ResourceHeap_FreeItem(ResourceHeap resourceHeap, uint id)
    {
        ResourceHeapItem? item = ResourceHeap_FindItem(resourceHeap, id);
        if (item == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        FreeHeapItemData(item!.Data!);
        ResourceHeapItem_Reset(item);
    }

    // OFFSET 0x021F075C
    public static byte[]? ResourceHeap_GetItemData(ResourceHeap resourceHeap, uint id)
    {
        ResourceHeapItem? item = ResourceHeap_FindItem(resourceHeap, id);
        if (item == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return item!.Data;
    }

    // OFFSET 0x021F0770
    public static bool ResourceHeap_HasItem(ResourceHeap resourceHeap, uint id)
    {
        return ResourceHeap_FindItem(resourceHeap, id) != null;
    }

    // OFFSET 0x021F0784
    public static byte[] ResourceHeap_LoadMemberFromNARC(ResourceHeap resourceHeap, uint id, NARC narc, uint memberIndex, int allocAtEnd)
    {
        uint size = NARC.NARC_GetMemberSize(narc, memberIndex);
        byte[] data = ResourceHeap_AllocItem(resourceHeap, id, size, allocAtEnd);
        NARC.NARC_ReadWholeMember(narc, memberIndex, data);
        return data;
    }

    // OFFSET 0x021F07B4
    private static byte[] ResourceHeap_AllocItemData(ResourceHeap resourceHeap, uint size, int allocAtEnd)
    {
        byte[]? data;
        if (allocAtEnd == 0)
        {
            data = Heap.Heap_Alloc(resourceHeap.HeapID, size);
        }
        else
        {
            data = Heap.Heap_AllocAtEnd(resourceHeap.HeapID, size);
        }
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return data!;
    }

    // OFFSET 0x021F07D4
    private static void FreeHeapItemData(byte[] itemData)
    {
        Heap.Heap_Free(itemData);
    }

    // OFFSET 0x021F07DC
    private static ResourceHeapItem? ResourceHeap_FindItem(ResourceHeap resourceHeap, uint id)
    {
        for (uint i = 0; i < resourceHeap.ItemCount; i++)
        {
            ResourceHeapItem item = resourceHeap.Items[i];
            if (item.Data != null && item.ID == id)
            {
                return item;
            }
        }
        return null;
    }

    // OFFSET 0x021F07FC
    private static ResourceHeapItem? ResourceHeap_FindFreeItem(ResourceHeap resourceHeap)
    {
        for (uint i = 0; i < resourceHeap.ItemCount; i++)
        {
            if (resourceHeap.Items[i].Data == null)
            {
                return resourceHeap.Items[i];
            }
        }
        return null;
    }

    // OFFSET 0x021F0814
    private static void ResourceHeapItem_Init(ResourceHeapItem item, uint id, byte[] data)
    {
        item.Data = data;
        item.ID = id;
    }

    // OFFSET 0x021F081C
    private static void ResourceHeapItem_Reset(ResourceHeapItem item)
    {
        item.Data = null;
        item.ID = 0;
    }
}
