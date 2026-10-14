using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class Resource
{
    public int ID;
    public byte[]? Data;
}

public sealed class ResourceCollection
{
    public Resource[]? Resources;
    public int Capacity;
    public int Count;

    // OFFSET 0x0202298C
    public static ResourceCollection ResourceCollection_New(int capacity, int heapID)
    {
        ResourceCollection collection = new();
        collection.Resources = new Resource[capacity];
        for (int i = 0; i < capacity; i++)
        {
            collection.Resources[i] = new Resource();
            Resource_Init(collection.Resources[i]);
        }
        collection.Capacity = capacity;
        collection.Count = 0;
        return collection;
    }

    // OFFSET 0x020229D8
    public static void ResourceCollection_Delete(ResourceCollection? collection)
    {
        if (collection == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        ResourceCollection_Clear(collection!);
        collection!.Resources = null;
    }

    // OFFSET 0x020229F8
    public static bool ResourceCollection_IsIDUnused(ResourceCollection? collection, int id)
    {
        if (collection == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return ResourceCollection_FindResource(collection!, id) == null;
    }

    // OFFSET 0x02022A1C
    public static Resource ResourceCollection_Add(ResourceCollection? collection, byte[]? data, int id)
    {
        if (collection == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        Resource? resource = ResourceCollection_AllocResource(collection!);
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!ResourceCollection_IsIDUnused(collection, id))
        {
            ErrorHandling.GF_AssertFail();
        }
        resource!.Data = data;
        resource.ID = id;
        collection!.Count++;
        return resource;
    }

    // OFFSET 0x02022A58
    public static Resource ResourceCollection_AddFromFile(ResourceCollection? collection, string? filename, int id, int heapID)
    {
        if (collection == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (filename == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        Resource? resource = ResourceCollection_AllocResource(collection!);
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!ResourceCollection_IsIDUnused(collection, id))
        {
            ErrorHandling.GF_AssertFail();
        }
        resource!.Data = GameSystem.ReadFileToHeap(heapID, filename!);
        if (resource.Data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        resource.ID = id;
        collection!.Count++;
        return resource;
    }

    // OFFSET 0x02022AB0
    public static void ResourceCollection_Remove(ResourceCollection? collection, Resource? resource)
    {
        if (collection == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (resource!.Data != null)
        {
            Heap.Heap_Free(resource.Data);
            resource.Data = null;
        }
        resource.ID = -1;
        collection!.Count--;
    }

    // OFFSET 0x02022AE4
    public static void ResourceCollection_Clear(ResourceCollection? collection)
    {
        if (collection == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (collection!.Resources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < collection.Capacity; i++)
        {
            if (collection.Resources![i].ID != -1)
            {
                ResourceCollection_Remove(collection, collection.Resources[i]);
            }
        }
    }

    // OFFSET 0x02022B20
    public static Resource? ResourceCollection_FindResource(ResourceCollection? collection, int id)
    {
        if (collection == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < collection!.Capacity; i++)
        {
            if (collection.Resources![i].ID == id)
            {
                return collection.Resources[i];
            }
        }
        return null;
    }

    // OFFSET 0x02022B54
    public static byte[]? Resource_GetData(Resource? resource)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return resource!.Data;
    }

    // OFFSET 0x02022B64
    public static void Resource_SetData(Resource? resource, byte[]? data)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        resource!.Data = data;
    }

    // OFFSET 0x02022B80
    public static int Resource_GetID(Resource? resource)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return resource!.ID;
    }

    // OFFSET 0x02022B90
    private static Resource? ResourceCollection_AllocResource(ResourceCollection? collection)
    {
        if (collection == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < collection!.Capacity; i++)
        {
            if (collection.Resources![i].ID == -1)
            {
                return collection.Resources[i];
            }
        }
        return null;
    }

    // OFFSET 0x02022BC0
    private static void Resource_Init(Resource? resource)
    {
        if (resource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        resource!.ID = -1;
        resource.Data = null;
    }
}
