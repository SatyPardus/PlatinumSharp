using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void MapObjectBillboardCallback(object? param, Billboard billboard);

public sealed class MapObjectBillboardSlot
{
    public Billboard? Billboard;
}

public sealed class MapObjectBillboardState
{
    public ushort Draw;
    public ushort AnimNum;
    public int FrameNum;
}

public sealed class MapObjectResourceEntry
{
    public int GraphicsID;
    public BillboardResources Resources = new();
}

public sealed class MapObjectPendingAdd
{
    public int GraphicsID;
    public MapObject? MapObject;
    public MapObjectBillboardSlot? Slot;
}

public sealed class MapObjectPendingLoad
{
    public uint MemberIndex;
    public int GraphicsID;
    public int IsDynamic;
    public TextureResourceManager? TextureManager;
}

public sealed class MapObjectPendingTransfer
{
    public int Uploaded;
    public int GraphicsID;
    public TextureResourceManager? TextureManager;
}

public sealed class MapObjectDeferredLoads
{
    public short Count;
    public short MaxLoadsPerFrame;
    public short LoadsThisFrame;
    public ushort Locked;
    public MapObjectPendingAdd[] PendingAdds = Array.Empty<MapObjectPendingAdd>();
    public MapObjectPendingLoad[] PendingLoads = Array.Empty<MapObjectPendingLoad>();
    public MapObjectPendingTransfer[] PendingTransfers = Array.Empty<MapObjectPendingTransfer>();
    public SysTask? LoadTask;
    public SysTask? AddTask;
    public SysTask? UploadTask;
    public SysTask? DiscardTask;
}

public sealed class MapObjectGraphicsEntry
{
    public const int Size = 0x10;

    public int GraphicsID;
    public int ModelID;
    public int SequenceID;
    public BillboardAnim[] Anims = Array.Empty<BillboardAnim>();
}

public sealed class MapObjectFileEntry
{
    public const int Size = 8;

    public int ID;
    public uint MemberIndex;
}

public sealed class MapObjectGraphicsChange
{
    public int NewGraphicsID;
    public short State;
    public byte[]? ModelData;
    public byte[]? SequenceData;
    public byte[]? TextureData;
    public BillboardGfxSequence GfxSequence = new();
    public Billboard? Billboard;
    public BillboardResources Resources = new();
    public MapObject? MapObject;
    public OverworldAnimManager? ReflectionAnim;
    public object? CallbackParam;
    public MapObjectBillboardCallback? Callback;
}

public sealed class MapObjectRenderData
{
    public const int ModelSlotCount = 8;
    public const int SequenceSlotCount = 8;
    public const int TextureSlotCount = 0x20;

    public int MaxObjects;
    public int ModelPreloadCount;
    public int ModelDynamicCount;
    public int SequencePreloadCount;
    public int SequenceDynamicCount;
    public int TexturePreloadCount;
    public int TextureDynamicCount;
    public readonly int[] ModelIDs = new int[ModelSlotCount];
    public readonly int[] SequenceIDs = new int[SequenceSlotCount];
    public readonly int[] TextureIDs = new int[TextureSlotCount];
    public BillboardList? BillboardList;
    public BillboardResources[]? Resources;
    public ResourceCollection? ModelCollection;
    public ResourceCollection? SequenceCollection;
    public TextureResourceManager? TextureManager;
    public MapObjectResourceEntry[]? ResourceEntries;
    public ResourceHeap? ModelHeap;
    public ResourceHeap? SequenceHeap;
    public MapObjectDeferredLoads? Deferred;
    public MapObjectManager? MapObjectManager;
}

public static class MapObjectRenderResources
{
    public const int TextureStateNone = 0;
    public const int TextureStatePreloaded = 1;
    public const int TextureStateDynamic = 2;
    public const int TextureStateQueuedPreload = 3;
    public const int TextureStateQueuedDynamic = 4;
    public const int SlotPreloaded = 0;
    public const int SlotDynamic = 1;
    public const int SlotNone = 2;

    private const int HeapIDFieldMap = MapObjectRender.HeapIDFieldMap;
    private const int HeapIDModels = 0x40;
    private const int HeapIDSequences = 0x41;
    private const int GraphicsIDNone = MapObjectRender.GraphicsIDNone;
    private const int OverlayID = 5;
    private const uint PreloadModelIDsAddress = 0x021FAF50;
    private const uint PreloadSequenceIDsAddress = 0x021FAF48;
    private const uint NoFogModelIDsAddress = 0x021FAF40;
    private const uint ModelFilesAddress = 0x021FB484;
    private const uint SequenceFilesAddress = 0x021FB5BC;
    private const uint TextureFilesAddress = 0x021FC9B4;
    private const uint GraphicsTableAddress = 0x021FD77C;
    private const uint BillboardScaleAddress = 0x021FF4B4;
    private const uint WalkAnimsAddress = 0x021FF4C0;
    private const uint RunAnimsAddress = 0x021FF4D0;
    private const uint AlternateAnimsAddress = 0x021FF4E0;
    private const int AnimTableEnd = 2;
    private const int ModelPreloadSlots = 4;
    private const int SequencePreloadSlots = 4;
    private const int MaterialPolygonAttrOffset = 0xC;
    private const uint PolygonAttrFogEnable = 0x8000;
    private const int SpriteZOffset = 6 * Fx32.One;
    private const uint StatusHideSprite = 1 << 9;
    private const uint StatusUnderCover = 1 << 12;
    private const uint StatusCoverVisible = 1 << 13;
    private const uint StatusDrawSuspended = 1 << 21;
    private const uint StatusBillboardPending = 1 << 22;
    private const uint ManagerStatusActive = 1 << 0;
    private const uint TaskPriorityLast = 0xFF;

    private static MapObjectFileEntry[]? sModelFiles;
    private static MapObjectFileEntry[]? sSequenceFiles;
    private static MapObjectFileEntry[]? sTextureFiles;
    private static MapObjectGraphicsEntry[]? sGraphicsTable;
    private static readonly Dictionary<uint, BillboardAnim[]> sAnimTables = new();

    private static int[] ReadIDList(uint address)
    {
        List<int> ids = new();
        while (true)
        {
            int id = (int)RomData.OverlayU32(OverlayID, address, 1)[0];
            ids.Add(id);
            if (id == GraphicsIDNone)
            {
                break;
            }
            address += 4;
        }
        return ids.ToArray();
    }

    private static MapObjectFileEntry[] ReadFileTable(uint address)
    {
        List<MapObjectFileEntry> entries = new();
        while (true)
        {
            uint[] words = RomData.OverlayU32(OverlayID, address, 2);
            entries.Add(new MapObjectFileEntry { ID = (int)words[0], MemberIndex = words[1] });
            if (words[0] == GraphicsIDNone)
            {
                break;
            }
            address += MapObjectFileEntry.Size;
        }
        return entries.ToArray();
    }

    private static MapObjectFileEntry[] ModelFiles()
    {
        return sModelFiles ??= ReadFileTable(ModelFilesAddress);
    }

    private static MapObjectFileEntry[] SequenceFiles()
    {
        return sSequenceFiles ??= ReadFileTable(SequenceFilesAddress);
    }

    private static MapObjectFileEntry[] TextureFiles()
    {
        return sTextureFiles ??= ReadFileTable(TextureFilesAddress);
    }

    private static BillboardAnim[] ReadAnims(uint address)
    {
        if (!sAnimTables.TryGetValue(address, out BillboardAnim[]? anims))
        {
            List<BillboardAnim> entries = new();
            uint current = address;
            while (true)
            {
                uint[] words = RomData.OverlayU32(OverlayID, current, 3);
                BillboardAnim anim = new() { StartFrame = (int)words[0], EndFrame = (int)words[1], StopAtEnd = (int)words[2] };
                entries.Add(anim);
                if (anim.StopAtEnd == AnimTableEnd)
                {
                    break;
                }
                current += BillboardAnim.Size;
            }
            anims = entries.ToArray();
            sAnimTables[address] = anims;
        }
        return anims;
    }

    private static MapObjectGraphicsEntry[] GraphicsTable()
    {
        if (sGraphicsTable == null)
        {
            List<MapObjectGraphicsEntry> entries = new();
            uint address = GraphicsTableAddress;
            while (true)
            {
                uint[] words = RomData.OverlayU32(OverlayID, address, 4);
                MapObjectGraphicsEntry entry = new() { GraphicsID = (int)words[0] };
                entries.Add(entry);
                if (words[0] == GraphicsIDNone)
                {
                    break;
                }
                entry.ModelID = (int)words[1];
                entry.SequenceID = (int)words[2];
                entry.Anims = ReadAnims(words[3]);
                address += MapObjectGraphicsEntry.Size;
            }
            sGraphicsTable = entries.ToArray();
        }
        return sGraphicsTable;
    }

    private static MapObjectFileEntry FindFileEntryOrAssert(MapObjectFileEntry[] table, int id)
    {
        int index = 0;
        while (table[index].ID != id)
        {
            index++;
            if (table[index].ID == GraphicsIDNone)
            {
                break;
            }
        }
        if (table[index].ID == GraphicsIDNone)
        {
            ErrorHandling.GF_AssertFail();
        }
        return table[index];
    }

    // OFFSET 0x021ECE40
    public static void MapObjectRenderData_Init(MapObjectRenderData renderData, MapObjectManager mapObjectManager, int maxObjects, int priority, int maxTextures, int preloadTextureCount, int[] preloadIDs, int maxLoadsPerFrame)
    {
        renderData.MapObjectManager = mapObjectManager;
        MapObjectRenderData_InitBillboardList(renderData, maxObjects);
        MapObjectRenderData_InitDeferredLoads(renderData, maxObjects, maxLoadsPerFrame);
        MapObjectRenderData_InitResourceStores(renderData, MapObjectRenderData.ModelSlotCount, ModelPreloadSlots, MapObjectRenderData.SequenceSlotCount, SequencePreloadSlots, maxTextures, preloadTextureCount);
        MapObjectRenderData_InitResourceEntries(renderData, maxObjects);
        MapObjectRenderData_PreloadAll(renderData, preloadIDs, ReadIDList(PreloadModelIDsAddress), ReadIDList(PreloadSequenceIDsAddress));
    }

    // OFFSET 0x021ECE94
    public static void MapObjectRenderData_Free(MapObjectRenderData renderData)
    {
        MapObjectRenderData_FreeDeferredLoads(renderData);
        MapObjectRenderData_FreeResourceEntries(renderData);
        DeleteBillboardList(MapObjectRenderData_GetBillboardList(renderData));
        MapObjectRenderData_FreeResourceStores(renderData);
    }

    // OFFSET 0x021ECEB4
    public static int MapObject_AddBillboardForGraphics(MapObject mapObject, MapObjectBillboardSlot slot, int graphicsID)
    {
        slot.Billboard = null;
        MapObjectRenderData renderData = MapObject_GetRenderData(mapObject);
        int state = MapObjectRenderData_GetTextureState(renderData, graphicsID);
        if (state == TextureStateNone)
        {
            MapObject_AddBillboardLoadingTexture(mapObject, slot, graphicsID);
            return state;
        }
        if ((uint)(state - TextureStateQueuedPreload) <= 1)
        {
            MapObjectRenderData_QueueBillboardAdd(renderData, mapObject, slot, graphicsID);
            return state;
        }
        slot.Billboard = MapObject_CreateBillboard(mapObject, graphicsID);
        return state;
    }

    // OFFSET 0x021ECF04
    public static int MapObject_AddBillboard(MapObject mapObject, MapObjectBillboardSlot slot)
    {
        return MapObject_AddBillboardForGraphics(mapObject, slot, MapObject.MapObject_GetGraphicsID(mapObject));
    }

    // OFFSET 0x021ECF1C
    public static Billboard MapObject_CreateBillboard(MapObject mapObject, int graphicsID)
    {
        MapObjectRenderData renderData = MapObject_GetRenderData(mapObject);
        BillboardList? list = MapObjectRenderData_GetBillboardList(renderData);
        MapObjectRenderData_LoadGraphics(renderData, graphicsID);
        BillboardResources? resources = MapObjectRenderData_GetResources(renderData, (uint)graphicsID);
        if (resources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObjectRender.MapObject_GetSpritePos(mapObject, out VecFx32 position);
        int fogEnabled = GraphicsUsesFog(graphicsID);
        Billboard? billboard = AppendBillboard(list, resources!, in position, fogEnabled);
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return billboard!;
    }

    // OFFSET 0x021ECF70
    public static void MapObject_DeleteBillboardForGraphics(MapObject mapObject, MapObjectBillboardSlot slot, int graphicsID)
    {
        Billboards.Billboard_Delete(slot.Billboard);
        slot.Billboard = null;
        MapObjectManager mapObjectManager = MapObject.MapObject_MapObjectManager(mapObject)!;
        if (MapObjectMan_IsGraphicsUsedByOther(mapObjectManager, mapObject, graphicsID) == 0)
        {
            MapObjectRenderData_ReleaseResources(MapObject_GetRenderData(mapObject), graphicsID);
        }
    }

    // OFFSET 0x021ECFA4
    public static void MapObject_DeleteBillboard(MapObject mapObject, MapObjectBillboardSlot slot)
    {
        MapObject_CancelBillboardAdd(mapObject);
        if (slot.Billboard != null)
        {
            int graphicsID = MapObject.MapObject_GetGraphicsID(mapObject);
            if (BerryPatchGraphics.BerryPatchGraphics_IsBerryPatch(graphicsID) == 1)
            {
                graphicsID = BerryPatchGraphics.BerryPatchGraphics_GetCurrentGraphicsResourceID(mapObject);
            }
            MapObject_DeleteBillboardForGraphics(mapObject, slot, graphicsID);
        }
    }

    // OFFSET 0x021ECFD8
    public static void MapObject_DeleteBillboardAndRelease(MapObject mapObject, MapObjectBillboardSlot slot, int graphicsID)
    {
        MapObject_CancelBillboardAdd(mapObject);
        if (slot.Billboard != null)
        {
            Billboards.Billboard_Delete(slot.Billboard);
            slot.Billboard = null;
        }
        if (graphicsID != GraphicsIDNone)
        {
            MapObjectManager mapObjectManager = MapObject.MapObject_MapObjectManager(mapObject)!;
            if (MapObjectMan_IsGraphicsUsedByOther(mapObjectManager, mapObject, graphicsID) == 0)
            {
                MapObjectRenderData_ReleaseResources(MapObject_GetRenderData(mapObject), graphicsID);
            }
        }
    }

    // OFFSET 0x021ED01C
    public static void Billboard_SaveState(Billboard billboard, MapObjectBillboardState state)
    {
        state.Draw = Billboards.Billboard_GetDrawFlag(billboard);
        state.AnimNum = (ushort)Billboards.Billboard_GetAnimNum(billboard);
        state.FrameNum = Billboards.Billboard_GetFrameNum(billboard);
    }

    // OFFSET 0x021ED03C
    public static void Billboard_RestoreState(Billboard billboard, MapObjectBillboardState state)
    {
        Billboards.Billboard_SetDrawFlag(billboard, (byte)state.Draw);
        Billboards.Billboard_SetAnimNum(billboard, state.AnimNum);
        Billboards.Billboard_SetFrameNum(billboard, state.FrameNum);
    }

    // OFFSET 0x021ED060
    public static void MapObject_AddBillboardLoadingTexture(MapObject mapObject, MapObjectBillboardSlot slot, int graphicsID)
    {
        MapObjectRenderData renderData = MapObject_GetRenderData(mapObject);
        if (MapObjectRenderData_LoadDynamicTexture(renderData, graphicsID) == 0)
        {
            slot.Billboard = MapObject_CreateBillboard(mapObject, graphicsID);
            return;
        }
        MapObjectRenderData_QueueBillboardAdd(renderData, mapObject, slot, graphicsID);
    }

    // OFFSET 0x021ED094
    private static void MapObject_CancelBillboardAdd(MapObject mapObject)
    {
        MapObjectRenderData_CancelBillboardAdd(MapObject_GetRenderData(mapObject), mapObject);
    }

    // OFFSET 0x021ED0A4
    private static void MapObjectRenderData_InitResourceEntries(MapObjectRenderData renderData, int count)
    {
        renderData.Resources = new BillboardResources[count];
        renderData.ResourceEntries = new MapObjectResourceEntry[count];
        for (int i = 0; i < count; i++)
        {
            renderData.Resources[i] = new BillboardResources();
            renderData.ResourceEntries[i] = new MapObjectResourceEntry();
            renderData.ResourceEntries[i].GraphicsID = GraphicsIDNone;
            renderData.ResourceEntries[i].Resources = renderData.Resources[i];
        }
    }

    // OFFSET 0x021ED0F0
    private static void MapObjectRenderData_FreeResourceEntries(MapObjectRenderData renderData)
    {
        MapObjectRenderData_ClearResourceEntries(renderData);
        renderData.Resources = null;
        renderData.ResourceEntries = null;
    }

    // OFFSET 0x021ED110
    private static BillboardResources? MapObjectRenderData_GetResources(MapObjectRenderData renderData, uint graphicsID)
    {
        MapObjectResourceEntry[] entries = renderData.ResourceEntries!;
        for (int i = 0; i < renderData.MaxObjects; i++)
        {
            if (entries[i].GraphicsID == (int)graphicsID)
            {
                return entries[i].Resources;
            }
        }
        for (int i = 0; i < renderData.MaxObjects; i++)
        {
            if (entries[i].GraphicsID == GraphicsIDNone)
            {
                entries[i].GraphicsID = (int)graphicsID;
                MapObjectRenderData_BuildResources(renderData, graphicsID, entries[i].Resources);
                return entries[i].Resources;
            }
        }
        return null;
    }

    // OFFSET 0x021ED150
    public static int MapObjectMan_CopyResources(MapObjectManager mapObjectManager, uint graphicsID, BillboardResources resources)
    {
        MapObjectRenderData renderData = MapObjectRender.RenderDataOf(mapObjectManager);
        MapObjectResourceEntry[] entries = renderData.ResourceEntries!;
        for (int i = 0; i < renderData.MaxObjects; i++)
        {
            if (entries[i].GraphicsID == (int)graphicsID)
            {
                resources.CopyFrom(entries[i].Resources);
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x021ED184
    private static void MapObjectRenderData_ReleaseResources(MapObjectRenderData renderData, int graphicsID)
    {
        MapObjectResourceEntry[] entries = renderData.ResourceEntries!;
        for (int i = 0; i < renderData.MaxObjects; i++)
        {
            if (entries[i].GraphicsID == graphicsID)
            {
                entries[i].GraphicsID = GraphicsIDNone;
                return;
            }
        }
    }

    // OFFSET 0x021ED1A4
    private static void MapObjectRenderData_ClearResourceEntries(MapObjectRenderData renderData)
    {
        MapObjectResourceEntry[] entries = renderData.ResourceEntries!;
        for (int i = 0; i < renderData.MaxObjects; i++)
        {
            if (entries[i].GraphicsID != GraphicsIDNone)
            {
                entries[i].GraphicsID = GraphicsIDNone;
            }
        }
    }

    // OFFSET 0x021ED1C8
    private static int MapObjectMan_IsGraphicsUsedByOther(MapObjectManager mapObjectManager, MapObject mapObject, int graphicsID)
    {
        int count = MapObjectManager.MapObjectMan_GetMaxObjects(mapObjectManager);
        MapObject? other = MapObjectManager.MapObjectMan_GetMapObjectsConst(mapObjectManager)![0];
        do
        {
            if (other != mapObject && MapObject.MapObject_IsActive(other!) == 1)
            {
                int otherGraphicsID = MapObject.MapObject_GetGraphicsID(other!);
                if (BerryPatchGraphics.BerryPatchGraphics_IsBerryPatch(otherGraphicsID) == 1)
                {
                    otherGraphicsID = BerryPatchGraphics.BerryPatchGraphics_GetCurrentGraphicsResourceID(other!);
                }
                if (otherGraphicsID != GraphicsIDNone && otherGraphicsID == graphicsID)
                {
                    return 1;
                }
            }
            MapObjectManager.MapObjectMan_NextMapObject(ref other);
            count--;
        }
        while (count != 0);
        return 0;
    }

    // OFFSET 0x021ED224
    private static void MapObjectRenderData_InitResourceStores(MapObjectRenderData renderData, int modelCount, int modelPreloadCount, int sequenceCount, int sequencePreloadCount, int maxTextures, int preloadTextureCount)
    {
        renderData.ModelHeap = ResourceHeap.ResourceHeap_New(HeapIDFieldMap, HeapIDModels, (uint)(modelCount << 12), (uint)modelCount);
        MapObjectRenderData_ClearModelIDs(renderData);
        MapObjectRenderData_SetModelPreloadCount(renderData, modelPreloadCount);
        MapObjectRenderData_SetModelDynamicCount(renderData, modelCount - modelPreloadCount);
        renderData.SequenceHeap = ResourceHeap.ResourceHeap_New(HeapIDFieldMap, HeapIDSequences, (uint)(sequenceCount << 7), (uint)sequenceCount);
        MapObjectRenderData_ClearSequenceIDs(renderData);
        MapObjectRenderData_SetSequencePreloadCount(renderData, sequencePreloadCount);
        MapObjectRenderData_SetSequenceDynamicCount(renderData, sequenceCount - sequencePreloadCount);
        MapObjectRenderData_SetTextureManager(renderData, TextureResourceManager.TextureResourceManager_New(maxTextures, HeapIDFieldMap));
        MapObjectRenderData_ClearTextureIDs(renderData);
        MapObjectRenderData_SetTexturePreloadCount(renderData, preloadTextureCount);
        MapObjectRenderData_SetTextureDynamicCount(renderData, maxTextures - preloadTextureCount);
    }

    // OFFSET 0x021ED2AC
    private static void MapObjectRenderData_FreeResourceStores(MapObjectRenderData renderData)
    {
        ResourceHeap.ResourceHeap_Free(renderData.ModelHeap!);
        ResourceHeap.ResourceHeap_Free(renderData.SequenceHeap!);
        TextureResourceManager.TextureResourceManager_Delete(MapObjectRenderData_GetTextureManager(renderData));
    }

    // OFFSET 0x021ED2D0
    private static MapObjectFileEntry? FindFileEntry(int id, int terminator, MapObjectFileEntry[] table)
    {
        int index = 0;
        do
        {
            if (table[index].ID == id)
            {
                return table[index];
            }
            index++;
        }
        while (table[index].ID != terminator);
        return null;
    }

    // OFFSET 0x021ED2E8
    private static int MapObjectRenderData_LoadHeapItem(MapObjectRenderData renderData, ResourceHeap resourceHeap, int id, int terminator, MapObjectFileEntry[] table)
    {
        if (ResourceHeap.ResourceHeap_HasItem(resourceHeap, (uint)id))
        {
            return 0;
        }
        MapObjectFileEntry? entry = FindFileEntry(id, terminator, table);
        if (entry == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        NARC narc = MapObjectManager.MapObjectMan_GetNARC(renderData.MapObjectManager!)!;
        ResourceHeap.ResourceHeap_LoadMemberFromNARC(resourceHeap, (uint)id, narc, entry!.MemberIndex, 0);
        return 1;
    }

    // OFFSET 0x021ED334
    private static int MapObjectRenderData_LoadTexture(MapObjectRenderData renderData, int isDynamic, int graphicsID, MapObjectFileEntry[] table)
    {
        if (!TextureResourceManager.TextureResourceManager_IsIDUnused(renderData.TextureManager, graphicsID))
        {
            return 0;
        }
        if (MapObjectRenderData_GetQueuedTextureState(renderData, graphicsID) != 0)
        {
            return 1;
        }
        MapObjectFileEntry? entry = FindFileEntry(graphicsID, GraphicsIDNone, table);
        if (entry == null)
        {
            return 2;
        }
        if (MapObjectRenderData_LoadOrQueueTexture(renderData, graphicsID, entry.MemberIndex, isDynamic) == 1)
        {
            return 0;
        }
        return 1;
    }

    // OFFSET 0x021ED390
    private static int MapObjectRenderData_LoadPreloadTexture(MapObjectRenderData renderData, int graphicsID)
    {
        return MapObjectRenderData_LoadTexture(renderData, 0, graphicsID, TextureFiles());
    }

    // OFFSET 0x021ED3A4
    private static int MapObjectRenderData_LoadDynamicTexture(MapObjectRenderData renderData, int graphicsID)
    {
        return MapObjectRenderData_LoadTexture(renderData, 1, graphicsID, TextureFiles());
    }

    // OFFSET 0x021ED3B8
    private static void MapObjectRenderData_PreloadTextures(MapObjectRenderData renderData, int[] graphicsIDs)
    {
        int index = 0;
        while (graphicsIDs[index] != GraphicsIDNone)
        {
            MapObjectRenderData_LoadPreloadTexture(renderData, graphicsIDs[index]);
            index++;
        }
    }

    // OFFSET 0x021ED3DC
    private static void MapObjectRenderData_LoadPreloadModel(MapObjectRenderData renderData, int modelID)
    {
        MapObjectRenderData_GetModelCollection(renderData);
        MapObjectRenderData_LoadHeapItem(renderData, renderData.ModelHeap!, modelID, GraphicsIDNone, ModelFiles());
        MapObjectRenderData_AddPreloadModelID(renderData, modelID);
    }

    // OFFSET 0x021ED40C
    private static void MapObjectRenderData_LoadDynamicModel(MapObjectRenderData renderData, int modelID)
    {
        MapObjectRenderData_GetModelCollection(renderData);
        MapObjectRenderData_LoadHeapItem(renderData, renderData.ModelHeap!, modelID, GraphicsIDNone, ModelFiles());
        MapObjectRenderData_AddDynamicModelID(renderData, modelID);
    }

    // OFFSET 0x021ED43C
    private static void MapObjectRenderData_PreloadModels(MapObjectRenderData renderData, int[] modelIDs)
    {
        int index = 0;
        while (modelIDs[index] != GraphicsIDNone)
        {
            MapObjectRenderData_LoadPreloadModel(renderData, modelIDs[index]);
            index++;
        }
    }

    // OFFSET 0x021ED460
    private static void MapObjectRenderData_LoadPreloadSequence(MapObjectRenderData renderData, int sequenceID)
    {
        MapObjectRenderData_GetSequenceCollection(renderData);
        MapObjectRenderData_LoadHeapItem(renderData, renderData.SequenceHeap!, sequenceID, GraphicsIDNone, SequenceFiles());
        MapObjectRenderData_AddPreloadSequenceID(renderData, sequenceID);
    }

    // OFFSET 0x021ED490
    private static void MapObjectRenderData_LoadDynamicSequence(MapObjectRenderData renderData, int sequenceID)
    {
        MapObjectRenderData_GetSequenceCollection(renderData);
        MapObjectRenderData_LoadHeapItem(renderData, renderData.SequenceHeap!, sequenceID, GraphicsIDNone, SequenceFiles());
        MapObjectRenderData_AddDynamicSequenceID(renderData, sequenceID);
    }

    // OFFSET 0x021ED4C0
    private static void MapObjectRenderData_PreloadSequences(MapObjectRenderData renderData, int[] sequenceIDs)
    {
        int index = 0;
        while (sequenceIDs[index] != GraphicsIDNone)
        {
            MapObjectRenderData_LoadPreloadSequence(renderData, sequenceIDs[index]);
            index++;
        }
    }

    // OFFSET 0x021ED4E4
    private static void MapObjectRenderData_PreloadAll(MapObjectRenderData renderData, int[] graphicsIDs, int[] modelIDs, int[] sequenceIDs)
    {
        MapObjectRenderData_PreloadTextures(renderData, graphicsIDs);
        MapObjectRenderData_PreloadModels(renderData, modelIDs);
        MapObjectRenderData_PreloadSequences(renderData, sequenceIDs);
    }

    // OFFSET 0x021ED504
    private static void IDSlots_Fill(int[] slots, int offset, int value, int count)
    {
        do
        {
            slots[offset] = value;
            offset++;
            count--;
        }
        while (count != 0);
    }

    // OFFSET 0x021ED50C
    private static int IDSlots_Store(int[] slots, int offset, int value, int match, int count)
    {
        do
        {
            if (slots[offset] == match)
            {
                slots[offset] = value;
                return 1;
            }
            offset++;
            count--;
        }
        while (count != 0);
        return 0;
    }

    // OFFSET 0x021ED528
    private static int IDSlots_Contains(int[] slots, int offset, int value, int count)
    {
        do
        {
            if (slots[offset] == value)
            {
                return 1;
            }
            offset++;
            count--;
        }
        while (count != 0);
        return 0;
    }

    // OFFSET 0x021ED53C
    private static int IDSlots_Replace(int[] slots, int offset, int match, int value, int count)
    {
        do
        {
            if (slots[offset] == match)
            {
                slots[offset] = value;
                return 1;
            }
            offset++;
            count--;
        }
        while (count != 0);
        return 0;
    }

    // OFFSET 0x021ED558
    private static void MapObjectRenderData_ClearModelIDs(MapObjectRenderData renderData)
    {
        IDSlots_Fill(MapObjectRenderData_GetModelIDs(renderData), 0, GraphicsIDNone, MapObjectRenderData.ModelSlotCount);
    }

    // OFFSET 0x021ED56C
    private static void MapObjectRenderData_AddPreloadModelID(MapObjectRenderData renderData, int modelID)
    {
        int count = MapObjectRenderData_GetModelPreloadCount(renderData);
        if (IDSlots_Store(MapObjectRenderData_GetModelIDs(renderData), 0, modelID, GraphicsIDNone, count) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021ED598
    private static int MapObjectRenderData_IsPreloadModel(MapObjectRenderData renderData, int modelID)
    {
        int count = MapObjectRenderData_GetModelPreloadCount(renderData);
        return IDSlots_Contains(MapObjectRenderData_GetModelIDs(renderData), 0, modelID, count);
    }

    // OFFSET 0x021ED5B4
    private static void MapObjectRenderData_AddDynamicModelID(MapObjectRenderData renderData, int modelID)
    {
        int[] slots = MapObjectRenderData_GetModelIDs(renderData);
        int offset = MapObjectRenderData_GetModelPreloadCount(renderData);
        int count = MapObjectRenderData_GetModelDynamicCount(renderData);
        if (IDSlots_Store(slots, offset, modelID, GraphicsIDNone, count) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021ED5E8
    private static int MapObjectRenderData_IsDynamicModel(MapObjectRenderData renderData, int modelID)
    {
        int[] slots = MapObjectRenderData_GetModelIDs(renderData);
        int offset = MapObjectRenderData_GetModelPreloadCount(renderData);
        int count = MapObjectRenderData_GetModelDynamicCount(renderData);
        return IDSlots_Contains(slots, offset, modelID, count);
    }

    // OFFSET 0x021ED610
    private static void MapObjectRenderData_RemoveDynamicModelID(MapObjectRenderData renderData, int modelID)
    {
        int[] slots = MapObjectRenderData_GetModelIDs(renderData);
        int offset = MapObjectRenderData_GetModelPreloadCount(renderData);
        int count = MapObjectRenderData_GetModelDynamicCount(renderData);
        IDSlots_Replace(slots, offset, modelID, GraphicsIDNone, count);
    }

    // OFFSET 0x021ED63C
    private static void MapObjectRenderData_FreeUnusedModels(MapObjectManager mapObjectManager, MapObjectRenderData renderData)
    {
        MapObjectRenderData_GetModelCollection(renderData);
        int[] slots = MapObjectRenderData_GetModelIDs(renderData);
        int offset = MapObjectRenderData_GetModelPreloadCount(renderData);
        int count = MapObjectRenderData_GetModelDynamicCount(renderData);
        do
        {
            if (slots[offset] != GraphicsIDNone)
            {
                if (MapObjectMan_IsModelInUse(mapObjectManager, slots[offset], null) == 0)
                {
                    ResourceHeap.ResourceHeap_FreeItem(renderData.ModelHeap!, (uint)slots[offset]);
                    slots[offset] = GraphicsIDNone;
                }
            }
            offset++;
            count--;
        }
        while (count != 0);
    }

    // OFFSET 0x021ED694
    private static void MapObjectRenderData_ClearSequenceIDs(MapObjectRenderData renderData)
    {
        IDSlots_Fill(MapObjectRenderData_GetSequenceIDs(renderData), 0, GraphicsIDNone, MapObjectRenderData.SequenceSlotCount);
    }

    // OFFSET 0x021ED6A8
    private static void MapObjectRenderData_AddPreloadSequenceID(MapObjectRenderData renderData, int sequenceID)
    {
        int count = MapObjectRenderData_GetSequencePreloadCount(renderData);
        if (IDSlots_Store(MapObjectRenderData_GetSequenceIDs(renderData), 0, sequenceID, GraphicsIDNone, count) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021ED6D4
    private static int MapObjectRenderData_IsPreloadSequence(MapObjectRenderData renderData, int sequenceID)
    {
        int count = MapObjectRenderData_GetSequencePreloadCount(renderData);
        return IDSlots_Contains(MapObjectRenderData_GetSequenceIDs(renderData), 0, sequenceID, count);
    }

    // OFFSET 0x021ED6F0
    private static void MapObjectRenderData_AddDynamicSequenceID(MapObjectRenderData renderData, int sequenceID)
    {
        int[] slots = MapObjectRenderData_GetSequenceIDs(renderData);
        int offset = MapObjectRenderData_GetSequencePreloadCount(renderData);
        int count = MapObjectRenderData_GetSequenceDynamicCount(renderData);
        if (IDSlots_Store(slots, offset, sequenceID, GraphicsIDNone, count) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021ED724
    private static int MapObjectRenderData_IsDynamicSequence(MapObjectRenderData renderData, int sequenceID)
    {
        int[] slots = MapObjectRenderData_GetSequenceIDs(renderData);
        int offset = MapObjectRenderData_GetSequencePreloadCount(renderData);
        int count = MapObjectRenderData_GetSequenceDynamicCount(renderData);
        return IDSlots_Contains(slots, offset, sequenceID, count);
    }

    // OFFSET 0x021ED74C
    private static void MapObjectRenderData_RemoveDynamicSequenceID(MapObjectRenderData renderData, int sequenceID)
    {
        int[] slots = MapObjectRenderData_GetSequenceIDs(renderData);
        int offset = MapObjectRenderData_GetSequencePreloadCount(renderData);
        int count = MapObjectRenderData_GetSequenceDynamicCount(renderData);
        IDSlots_Replace(slots, offset, sequenceID, GraphicsIDNone, count);
    }

    // OFFSET 0x021ED778
    private static void MapObjectRenderData_FreeUnusedSequences(MapObjectManager mapObjectManager, MapObjectRenderData renderData)
    {
        MapObjectRenderData_GetSequenceCollection(renderData);
        int[] slots = MapObjectRenderData_GetSequenceIDs(renderData);
        int offset = MapObjectRenderData_GetSequencePreloadCount(renderData);
        int count = MapObjectRenderData_GetSequenceDynamicCount(renderData);
        do
        {
            if (slots[offset] != GraphicsIDNone)
            {
                if (MapObjectMan_IsSequenceInUse(mapObjectManager, slots[offset], null) == 0)
                {
                    ResourceHeap.ResourceHeap_FreeItem(renderData.SequenceHeap!, (uint)slots[offset]);
                    slots[offset] = GraphicsIDNone;
                }
            }
            offset++;
            count--;
        }
        while (count != 0);
    }

    // OFFSET 0x021ED7D0
    private static void MapObjectRenderData_ClearTextureIDs(MapObjectRenderData renderData)
    {
        IDSlots_Fill(MapObjectRenderData_GetTextureIDs(renderData), 0, GraphicsIDNone, MapObjectRenderData.TextureSlotCount);
    }

    // OFFSET 0x021ED7E4
    private static void MapObjectRenderData_AddPreloadTextureID(MapObjectRenderData renderData, int graphicsID)
    {
        int count = MapObjectRenderData_GetTexturePreloadCount(renderData);
        if (IDSlots_Store(MapObjectRenderData_GetTextureIDs(renderData), 0, graphicsID, GraphicsIDNone, count) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021ED810
    private static int MapObjectRenderData_IsPreloadTexture(MapObjectRenderData renderData, int graphicsID)
    {
        int count = MapObjectRenderData_GetTexturePreloadCount(renderData);
        return IDSlots_Contains(MapObjectRenderData_GetTextureIDs(renderData), 0, graphicsID, count);
    }

    // OFFSET 0x021ED82C
    private static int MapObjectRenderData_GetTextureState(MapObjectRenderData renderData, int graphicsID)
    {
        if (MapObjectRenderData_IsPreloadTexture(renderData, graphicsID) == 1)
        {
            return TextureStatePreloaded;
        }
        if (MapObjectRenderData_IsDynamicTexture(renderData, graphicsID) == 1)
        {
            return TextureStateDynamic;
        }
        return MapObjectRenderData_GetQueuedTextureState(renderData, graphicsID);
    }

    // OFFSET 0x021ED858
    private static void MapObjectRenderData_AddDynamicTextureID(MapObjectRenderData renderData, int graphicsID)
    {
        int[] slots = MapObjectRenderData_GetTextureIDs(renderData);
        int offset = MapObjectRenderData_GetTexturePreloadCount(renderData);
        int count = MapObjectRenderData_GetTextureDynamicCount(renderData);
        if (IDSlots_Store(slots, offset, graphicsID, GraphicsIDNone, count) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021ED88C
    private static void MapObjectRenderData_RemoveDynamicTextureID(MapObjectRenderData renderData, int graphicsID)
    {
        int[] slots = MapObjectRenderData_GetTextureIDs(renderData);
        int offset = MapObjectRenderData_GetTexturePreloadCount(renderData);
        int count = MapObjectRenderData_GetTextureDynamicCount(renderData);
        IDSlots_Replace(slots, offset, graphicsID, GraphicsIDNone, count);
    }

    // OFFSET 0x021ED8B8
    private static void MapObjectRenderData_FreeUnusedTextures(MapObjectManager mapObjectManager, MapObjectRenderData renderData)
    {
        TextureResourceManager? textureManager = MapObjectRenderData_GetTextureManager(renderData);
        int[] slots = MapObjectRenderData_GetTextureIDs(renderData);
        int offset = MapObjectRenderData_GetTexturePreloadCount(renderData);
        int count = MapObjectRenderData_GetTextureDynamicCount(renderData);
        do
        {
            int graphicsID = slots[offset];
            if (graphicsID != GraphicsIDNone)
            {
                if (MapObjectMan_IsGraphicsInUse(mapObjectManager, graphicsID, null) == 0)
                {
                    TextureResourceManager.TextureResourceManager_RemoveTextureWithID(textureManager, graphicsID);
                    MapObjectRenderData_CancelQueuedTexture(renderData, graphicsID);
                    MapObjectRenderData_CancelTextureTransfer(renderData, graphicsID);
                    slots[offset] = GraphicsIDNone;
                }
            }
            offset++;
            count--;
        }
        while (count != 0);
    }

    // OFFSET 0x021ED924
    private static int MapObjectRenderData_IsDynamicTexture(MapObjectRenderData renderData, int graphicsID)
    {
        int[] slots = MapObjectRenderData_GetTextureIDs(renderData);
        int offset = MapObjectRenderData_GetTexturePreloadCount(renderData);
        int count = MapObjectRenderData_GetTextureDynamicCount(renderData);
        return IDSlots_Contains(slots, offset, graphicsID, count);
    }

    // OFFSET 0x021ED94C
    private static int MapObjectRenderData_GetTextureSlotType(MapObjectRenderData renderData, int graphicsID)
    {
        if (MapObjectRenderData_IsPreloadTexture(renderData, graphicsID) == 1)
        {
            return SlotPreloaded;
        }
        if (MapObjectRenderData_IsDynamicTexture(renderData, graphicsID) == 1)
        {
            return SlotDynamic;
        }
        return SlotNone;
    }

    // OFFSET 0x021ED974
    private static int MapObjectRenderData_GetModelSlotType(MapObjectRenderData renderData, int modelID)
    {
        if (MapObjectRenderData_IsPreloadModel(renderData, modelID) == 1)
        {
            return SlotPreloaded;
        }
        if (MapObjectRenderData_IsDynamicModel(renderData, modelID) == 1)
        {
            return SlotDynamic;
        }
        return SlotNone;
    }

    // OFFSET 0x021ED99C
    private static int MapObjectRenderData_GetSequenceSlotType(MapObjectRenderData renderData, int sequenceID)
    {
        if (MapObjectRenderData_IsPreloadSequence(renderData, sequenceID) == 1)
        {
            return SlotPreloaded;
        }
        if (MapObjectRenderData_IsDynamicSequence(renderData, sequenceID) == 1)
        {
            return SlotDynamic;
        }
        return SlotNone;
    }

    // OFFSET 0x021ED9C4
    private static void MapObjectRenderData_EnsureTexture(MapObjectRenderData renderData, int graphicsID)
    {
        if (MapObjectRenderData_GetTextureState(renderData, graphicsID) == TextureStateNone)
        {
            MapObjectRenderData_LoadDynamicTexture(renderData, graphicsID);
        }
    }

    // OFFSET 0x021ED9DC
    private static void MapObjectRenderData_EnsureModel(MapObjectRenderData renderData, int modelID)
    {
        if (MapObjectRenderData_GetModelSlotType(renderData, modelID) == SlotNone)
        {
            MapObjectRenderData_LoadDynamicModel(renderData, modelID);
        }
    }

    // OFFSET 0x021ED9F4
    private static void MapObjectRenderData_EnsureSequence(MapObjectRenderData renderData, int sequenceID)
    {
        if (MapObjectRenderData_GetSequenceSlotType(renderData, sequenceID) == SlotNone)
        {
            MapObjectRenderData_LoadDynamicSequence(renderData, sequenceID);
        }
    }

    // OFFSET 0x021EDA0C
    private static void MapObjectRenderData_LoadGraphics(MapObjectRenderData renderData, int graphicsID)
    {
        MapObjectRenderData_EnsureModel(renderData, GetModelIDForGraphics(graphicsID));
        MapObjectRenderData_EnsureSequence(renderData, GetSequenceIDForGraphics(graphicsID));
        MapObjectRenderData_EnsureTexture(renderData, graphicsID);
    }

    // OFFSET 0x021EDA38
    public static void MapObjectMan_FreeUnusedResources(MapObjectManager mapObjectManager, MapObjectRenderData renderData)
    {
        MapObjectRenderData_FreeUnusedModels(mapObjectManager, renderData);
        MapObjectRenderData_FreeUnusedSequences(mapObjectManager, renderData);
        MapObjectRenderData_FreeUnusedTextures(mapObjectManager, renderData);
    }

    // OFFSET 0x021EDA54
    private static int MapObjectMan_IsGraphicsInUse(MapObjectManager mapObjectManager, int graphicsID, MapObject? exclude)
    {
        int index = 0;
        while (MapObjectManager.MapObjectMan_FindObjectWithStatus(mapObjectManager, out MapObject? mapObject, ref index, ManagerStatusActive) == 1)
        {
            if (exclude != null && exclude == mapObject)
            {
                continue;
            }
            int otherGraphicsID = MapObject.MapObject_GetGraphicsID(mapObject!);
            if (BerryPatchGraphics.BerryPatchGraphics_IsBerryPatch(otherGraphicsID) == 1)
            {
                otherGraphicsID = BerryPatchGraphics.BerryPatchGraphics_GetCurrentGraphicsResourceID(mapObject!);
            }
            if (otherGraphicsID == graphicsID)
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x021EDAB4
    private static int MapObjectMan_IsModelInUse(MapObjectManager mapObjectManager, int modelID, MapObject? exclude)
    {
        int index = 0;
        while (MapObjectManager.MapObjectMan_FindObjectWithStatus(mapObjectManager, out MapObject? mapObject, ref index, ManagerStatusActive) == 1)
        {
            if (exclude != null && exclude == mapObject)
            {
                continue;
            }
            int graphicsID = MapObject.MapObject_GetGraphicsID(mapObject!);
            if (graphicsID == GraphicsIDNone)
            {
                continue;
            }
            if ((MapObjectRender.MapObject_GetRenderDetails(mapObject!)!.Flags & 0xF) != MapObjectRender.RenderTypeBillboard)
            {
                continue;
            }
            if (BerryPatchGraphics.BerryPatchGraphics_IsBerryPatch(graphicsID) == 1)
            {
                graphicsID = BerryPatchGraphics.BerryPatchGraphics_GetCurrentGraphicsResourceID(mapObject!);
            }
            if (graphicsID == GraphicsIDNone)
            {
                continue;
            }
            if (modelID == GetModelIDForGraphics(graphicsID))
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x021EDB3C
    private static int MapObjectMan_IsSequenceInUse(MapObjectManager mapObjectManager, int sequenceID, MapObject? exclude)
    {
        int index = 0;
        int graphicsID = 0;
        while (MapObjectManager.MapObjectMan_FindObjectWithStatus(mapObjectManager, out MapObject? mapObject, ref index, ManagerStatusActive) == 1)
        {
            if (exclude != null && mapObject == exclude)
            {
                continue;
            }
            if (graphicsID == GraphicsIDNone)
            {
                continue;
            }
            if ((MapObjectRender.MapObject_GetRenderDetails(mapObject!)!.Flags & 0xF) != MapObjectRender.RenderTypeBillboard)
            {
                continue;
            }
            graphicsID = MapObject.MapObject_GetGraphicsID(mapObject!);
            if (BerryPatchGraphics.BerryPatchGraphics_IsBerryPatch(graphicsID) == 1)
            {
                graphicsID = BerryPatchGraphics.BerryPatchGraphics_GetCurrentGraphicsResourceID(mapObject!);
            }
            if (graphicsID == GraphicsIDNone)
            {
                continue;
            }
            if (sequenceID == GetSequenceIDForGraphics(graphicsID))
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x021EDBC4
    public static void MapObjectMan_ReleaseGraphics(MapObjectManager mapObjectManager, int graphicsID, MapObject? mapObject)
    {
        MapObjectRenderData renderData = MapObjectRender.RenderDataOf(mapObjectManager);
        int modelID = GetModelIDForGraphics(graphicsID);
        if (MapObjectRenderData_GetModelSlotType(renderData, modelID) == SlotDynamic)
        {
            if (MapObjectMan_IsModelInUse(mapObjectManager, modelID, mapObject) == 0)
            {
                ResourceHeap.ResourceHeap_FreeItem(renderData.ModelHeap!, (uint)modelID);
                MapObjectRenderData_RemoveDynamicModelID(renderData, modelID);
            }
        }
        int sequenceID = GetSequenceIDForGraphics(graphicsID);
        if (MapObjectRenderData_GetSequenceSlotType(renderData, sequenceID) == SlotDynamic)
        {
            if (MapObjectMan_IsSequenceInUse(mapObjectManager, sequenceID, mapObject) == 0)
            {
                ResourceHeap.ResourceHeap_FreeItem(renderData.SequenceHeap!, (uint)sequenceID);
                MapObjectRenderData_RemoveDynamicSequenceID(renderData, sequenceID);
            }
        }
        if (MapObjectRenderData_GetTextureSlotType(renderData, graphicsID) == SlotDynamic)
        {
            if (MapObjectMan_IsGraphicsInUse(mapObjectManager, graphicsID, mapObject) == 0)
            {
                TextureResourceManager.TextureResourceManager_RemoveTextureWithID(MapObjectRenderData_GetTextureManager(renderData), graphicsID);
                MapObjectRenderData_RemoveDynamicTextureID(renderData, graphicsID);
                MapObjectRenderData_CancelQueuedTexture(renderData, graphicsID);
                MapObjectRenderData_CancelTextureTransfer(renderData, graphicsID);
            }
        }
    }

    // OFFSET 0x021EDC84
    public static void MapObjectRenderData_SetBillboardList(MapObjectRenderData renderData, BillboardList? list)
    {
        renderData.BillboardList = list;
    }

    // OFFSET 0x021EDC8C
    public static BillboardList? MapObjectRenderData_GetBillboardList(MapObjectRenderData renderData)
    {
        return renderData.BillboardList;
    }

    // OFFSET 0x021EDC94
    public static void MapObjectRenderData_SetMaxObjects(MapObjectRenderData renderData, int maxObjects)
    {
        renderData.MaxObjects = maxObjects;
    }

    // OFFSET 0x021EDC98
    public static ResourceCollection? MapObjectRenderData_GetModelCollection(MapObjectRenderData renderData)
    {
        return renderData.ModelCollection;
    }

    // OFFSET 0x021EDCA0
    public static ResourceCollection? MapObjectRenderData_GetSequenceCollection(MapObjectRenderData renderData)
    {
        return renderData.SequenceCollection;
    }

    // OFFSET 0x021EDCA8
    public static void MapObjectRenderData_SetTextureManager(MapObjectRenderData renderData, TextureResourceManager? textureManager)
    {
        renderData.TextureManager = textureManager;
    }

    // OFFSET 0x021EDCB0
    public static TextureResourceManager? MapObjectRenderData_GetTextureManager(MapObjectRenderData renderData)
    {
        return renderData.TextureManager;
    }

    // OFFSET 0x021EDCB8
    private static int[] MapObjectRenderData_GetTextureIDs(MapObjectRenderData renderData)
    {
        return renderData.TextureIDs;
    }

    // OFFSET 0x021EDCBC
    private static int[] MapObjectRenderData_GetModelIDs(MapObjectRenderData renderData)
    {
        return renderData.ModelIDs;
    }

    // OFFSET 0x021EDCC0
    private static int[] MapObjectRenderData_GetSequenceIDs(MapObjectRenderData renderData)
    {
        return renderData.SequenceIDs;
    }

    // OFFSET 0x021EDCC4
    private static void MapObjectRenderData_SetModelPreloadCount(MapObjectRenderData renderData, int count)
    {
        renderData.ModelPreloadCount = count;
    }

    // OFFSET 0x021EDCC8
    private static int MapObjectRenderData_GetModelPreloadCount(MapObjectRenderData renderData)
    {
        return renderData.ModelPreloadCount;
    }

    // OFFSET 0x021EDCCC
    private static void MapObjectRenderData_SetModelDynamicCount(MapObjectRenderData renderData, int count)
    {
        renderData.ModelDynamicCount = count;
    }

    // OFFSET 0x021EDCD0
    private static int MapObjectRenderData_GetModelDynamicCount(MapObjectRenderData renderData)
    {
        return renderData.ModelDynamicCount;
    }

    // OFFSET 0x021EDCD4
    private static void MapObjectRenderData_SetSequencePreloadCount(MapObjectRenderData renderData, int count)
    {
        renderData.SequencePreloadCount = count;
    }

    // OFFSET 0x021EDCD8
    private static int MapObjectRenderData_GetSequencePreloadCount(MapObjectRenderData renderData)
    {
        return renderData.SequencePreloadCount;
    }

    // OFFSET 0x021EDCDC
    private static void MapObjectRenderData_SetSequenceDynamicCount(MapObjectRenderData renderData, int count)
    {
        renderData.SequenceDynamicCount = count;
    }

    // OFFSET 0x021EDCE0
    private static int MapObjectRenderData_GetSequenceDynamicCount(MapObjectRenderData renderData)
    {
        return renderData.SequenceDynamicCount;
    }

    // OFFSET 0x021EDCE4
    private static void MapObjectRenderData_SetTexturePreloadCount(MapObjectRenderData renderData, int count)
    {
        renderData.TexturePreloadCount = count;
    }

    // OFFSET 0x021EDCE8
    private static int MapObjectRenderData_GetTexturePreloadCount(MapObjectRenderData renderData)
    {
        return renderData.TexturePreloadCount;
    }

    // OFFSET 0x021EDCEC
    private static void MapObjectRenderData_SetTextureDynamicCount(MapObjectRenderData renderData, int count)
    {
        renderData.TextureDynamicCount = count;
    }

    // OFFSET 0x021EDCF0
    private static int MapObjectRenderData_GetTextureDynamicCount(MapObjectRenderData renderData)
    {
        return renderData.TextureDynamicCount;
    }

    // OFFSET 0x021EDCF4
    private static byte[] MapObjectRenderData_LoadResource(MapObjectRenderData renderData, uint memberIndex, int allocAtStart)
    {
        return MapObjectRender.MapObjectMan_LoadRenderResource(renderData.MapObjectManager!, memberIndex, allocAtStart);
    }

    // OFFSET 0x021EDD04
    private static MapObjectGraphicsEntry? GetGraphicsEntry(int graphicsID)
    {
        MapObjectGraphicsEntry[] table = GraphicsTable();
        int index = 0;
        do
        {
            if (table[index].GraphicsID == graphicsID)
            {
                return table[index];
            }
            index++;
        }
        while (table[index].GraphicsID != GraphicsIDNone);
        ErrorHandling.GF_AssertFail();
        return null;
    }

    // OFFSET 0x021EDD2C
    private static int GetModelIDForGraphics(int graphicsID)
    {
        return GetGraphicsEntry(graphicsID)!.ModelID;
    }

    // OFFSET 0x021EDD38
    private static int GetSequenceIDForGraphics(int graphicsID)
    {
        return GetGraphicsEntry(graphicsID)!.SequenceID;
    }

    // OFFSET 0x021EDD44
    private static BillboardAnim[] GetAnimsForGraphics(int graphicsID)
    {
        return GetGraphicsEntry(graphicsID)!.Anims;
    }

    // OFFSET 0x021EDD50
    private static int GraphicsUsesFog(int graphicsID)
    {
        int modelID = GetModelIDForGraphics(graphicsID);
        int[] noFogModelIDs = ReadIDList(NoFogModelIDsAddress);
        int index = 0;
        do
        {
            if (noFogModelIDs[index] == modelID)
            {
                return 0;
            }
            index++;
        }
        while (noFogModelIDs[index] != GraphicsIDNone);
        return 1;
    }

    // OFFSET 0x021EDD78
    public static void MapObject_SetBillboardPending(MapObject mapObject, int pending)
    {
        if (pending == 1)
        {
            MapObject.MapObject_SetStatusFlagOn(mapObject, StatusBillboardPending);
        }
        else
        {
            MapObject.MapObject_SetStatusFlagOff(mapObject, StatusBillboardPending);
        }
    }

    // OFFSET 0x021EDD94
    public static int MapObject_IsBillboardPending(MapObject mapObject)
    {
        if (MapObject.MapObject_CheckStatus(mapObject, StatusBillboardPending) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021EDDAC
    private static void MapObjectRenderData_InitBillboardList(MapObjectRenderData renderData, int maxObjects)
    {
        BillboardListParams listParams = new();
        listParams.MaxElements = maxObjects;
        listParams.HeapID = HeapIDFieldMap;
        MapObjectRenderData_SetBillboardList(renderData, Billboards.BillboardList_New(listParams));
        MapObjectRenderData_SetMaxObjects(renderData, maxObjects);
    }

    // OFFSET 0x021EDDD4
    private static void DeleteBillboardList(BillboardList? list)
    {
        Billboards.BillboardList_Delete(list);
    }

    // OFFSET 0x021EDDDC
    private static Billboard? AppendBillboard(BillboardList? list, BillboardResources resources, in VecFx32 position, int fogEnabled)
    {
        BillboardTemplate template = new();
        template.List = list;
        template.Resources = resources;
        template.Pos = position;
        uint[] scale = RomData.OverlayU32(OverlayID, BillboardScaleAddress, 3);
        template.Scale.X = (int)scale[0];
        template.Scale.Y = (int)scale[1];
        template.Scale.Z = (int)scale[2];
        Billboard? billboard = Billboards.BillboardList_Append(template);
        if (billboard != null)
        {
            Billboards.Billboard_SetDrawFlag(billboard, 0);
            Billboards.Billboard_SetAnimNum(billboard, 0);
            Billboards.Billboard_SetAnimFrameNum(billboard, 0);
            G3d.SetMaterialParam(Billboards.Billboard_GetModel(billboard)!, MaterialPolygonAttrOffset, PolygonAttrFogEnable, fogEnabled != 0 ? PolygonAttrFogEnable : 0);
            AreaLight.AreaLight_UseGlobalModelAttributes(Billboards.Billboard_GetModel(billboard)!);
        }
        return billboard;
    }

    // OFFSET 0x021EDE3C
    private static void MapObjectRenderData_BuildResources(MapObjectRenderData renderData, uint graphicsID, BillboardResources resources)
    {
        resources.ModelRes = ResourceHeap.ResourceHeap_GetItemData(renderData.ModelHeap!, (uint)GetModelIDForGraphics((int)graphicsID));
        byte[] sequenceData = ResourceHeap.ResourceHeap_GetItemData(renderData.SequenceHeap!, (uint)GetSequenceIDForGraphics((int)graphicsID))!;
        BillboardGfxSequence.BillboardGfxSequence_SetData(sequenceData, 0, resources.GfxSequence);
        TextureResource? textureResource = TextureResourceManager.TextureResourceManager_FindTextureResource(MapObjectRenderData_GetTextureManager(renderData), (int)graphicsID);
        resources.Texture = TextureResourceManager.TextureResource_GetUnderlyingResource(textureResource);
        resources.TexKey = TextureResourceManager.TextureResource_GetTexKey(textureResource);
        resources.Tex4x4Key = TextureResourceManager.TextureResource_GetTex4x4Key(textureResource);
        resources.PlttKey = TextureResourceManager.TextureResource_GetPlttKey(textureResource);
        resources.Anims = GetAnimsForGraphics((int)graphicsID);
    }

    // OFFSET 0x021EDEA8
    private static MapObjectRenderData MapObject_GetRenderData(MapObject mapObject)
    {
        return MapObjectRender.RenderDataOf(MapObject.MapObject_MapObjectManager(mapObject)!);
    }

    // OFFSET 0x021EDEB4
    public static int MapObject_UpdateBillboardPos(MapObject mapObject, Billboard billboard)
    {
        MapObjectRender.MapObject_GetSpritePos(mapObject, out VecFx32 position);
        position.Z += SpriteZOffset;
        Billboards.Billboard_SetPos(billboard, in position);
        return 0;
    }

    // OFFSET 0x021EDED8
    public static void MapObject_UpdateBillboardDrawFlag(MapObject mapObject, Billboard billboard)
    {
        int draw = 1;
        if (MapObject.MapObject_CheckStatusFlag(mapObject, StatusHideSprite))
        {
            draw = 0;
        }
        if (MapObject.MapObject_CheckStatusFlag(mapObject, StatusUnderCover))
        {
            if (!MapObject.MapObject_CheckStatusFlag(mapObject, StatusCoverVisible))
            {
                draw = 0;
            }
        }
        Billboards.Billboard_SetDrawFlag(billboard, (byte)draw);
    }

    // OFFSET 0x021EDF18
    public static int GetWalkAnimForDir(int dir)
    {
        return (int)RomData.OverlayU32(OverlayID, WalkAnimsAddress + (uint)(dir << 2), 1)[0];
    }

    // OFFSET 0x021EDF24
    public static int GetRunAnimForDir(int dir)
    {
        return (int)RomData.OverlayU32(OverlayID, RunAnimsAddress + (uint)(dir << 2), 1)[0];
    }

    // OFFSET 0x021EDF30
    public static int GetAlternateAnimForDir(int dir)
    {
        return (int)RomData.OverlayU32(OverlayID, AlternateAnimsAddress + (uint)(dir << 2), 1)[0];
    }

    // OFFSET 0x021EDF3C
    private static int MapObjectRenderData_LoadOrQueueTexture(MapObjectRenderData renderData, int graphicsID, uint memberIndex, int isDynamic)
    {
        MapObjectDeferredLoads deferred = renderData.Deferred!;
        if (deferred.LoadsThisFrame >= deferred.MaxLoadsPerFrame || Billboards.BillboardList_IsRedrawing(renderData.BillboardList))
        {
            MapObjectPendingLoad[] loads = deferred.PendingLoads;
            for (int i = 0; i < deferred.Count; i++)
            {
                if (loads[i].TextureManager == null)
                {
                    loads[i].TextureManager = renderData.TextureManager;
                    loads[i].IsDynamic = isDynamic;
                    loads[i].GraphicsID = graphicsID;
                    loads[i].MemberIndex = memberIndex;
                    return 0;
                }
            }
            ErrorHandling.GF_AssertFail();
            return 0;
        }
        byte[] data = MapObjectRenderData_LoadResource(renderData, memberIndex, 0);
        MapObjectRenderData_AddTexture(renderData, graphicsID, data, isDynamic);
        deferred.LoadsThisFrame++;
        return 1;
    }

    // OFFSET 0x021EDFBC
    private static void MapObjectRenderData_CancelQueuedTexture(MapObjectRenderData renderData, int graphicsID)
    {
        MapObjectDeferredLoads deferred = renderData.Deferred!;
        MapObjectPendingLoad[] loads = deferred.PendingLoads;
        for (int i = 0; i < deferred.Count; i++)
        {
            if (loads[i].GraphicsID == graphicsID && loads[i].TextureManager != null)
            {
                loads[i].TextureManager = null;
                return;
            }
        }
    }

    // OFFSET 0x021EDFF0
    private static int MapObjectRenderData_GetQueuedTextureState(MapObjectRenderData renderData, int graphicsID)
    {
        MapObjectDeferredLoads deferred = renderData.Deferred!;
        MapObjectPendingLoad[] loads = deferred.PendingLoads;
        for (int i = 0; i < deferred.Count; i++)
        {
            if (loads[i].GraphicsID == graphicsID && loads[i].TextureManager != null)
            {
                if (loads[i].IsDynamic == 0)
                {
                    return TextureStateQueuedPreload;
                }
                return TextureStateQueuedDynamic;
            }
        }
        return TextureStateNone;
    }

    // OFFSET 0x021EE030
    private static void MapObjectRenderData_LoadQueuedTexturesTask(SysTask task, object? param)
    {
        MapObjectRenderData renderData = (MapObjectRenderData)param!;
        MapObjectDeferredLoads deferred = renderData.Deferred!;
        MapObjectPendingLoad[] loads = deferred.PendingLoads;
        if (Billboards.BillboardList_IsRedrawing(renderData.BillboardList))
        {
            return;
        }
        int index = 0;
        while (deferred.LoadsThisFrame < deferred.MaxLoadsPerFrame && index < deferred.Count)
        {
            MapObjectPendingLoad load = loads[index];
            if (load.TextureManager != null)
            {
                byte[] data = MapObjectRenderData_LoadResource(renderData, load.MemberIndex, 0);
                MapObjectRenderData_AddTexture(renderData, load.GraphicsID, data, load.IsDynamic);
                load.TextureManager = null;
                deferred.LoadsThisFrame++;
            }
            index++;
        }
        for (int i = 0; i < deferred.Count - 1; i++)
        {
            if (loads[i].TextureManager != null)
            {
                continue;
            }
            for (int j = i + 1; j < deferred.Count; j++)
            {
                if (loads[j].TextureManager != null)
                {
                    loads[i].MemberIndex = loads[j].MemberIndex;
                    loads[i].GraphicsID = loads[j].GraphicsID;
                    loads[i].IsDynamic = loads[j].IsDynamic;
                    loads[i].TextureManager = loads[j].TextureManager;
                    loads[j].TextureManager = null;
                    break;
                }
            }
        }
        deferred.LoadsThisFrame = 0;
    }

    // OFFSET 0x021EE0E8
    private static void MapObjectRenderData_AddTexture(MapObjectRenderData renderData, int graphicsID, byte[] data, int isDynamic)
    {
        TextureResource? textureResource = TextureResourceManager.TextureResourceManager_AddTextureAndAllocVRAM(renderData.TextureManager, data, graphicsID, 1, HeapIDFieldMap);
        if (textureResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObjectRenderData_QueueTextureTransfer(renderData, graphicsID);
        if (isDynamic == 0)
        {
            MapObjectRenderData_AddPreloadTextureID(renderData, graphicsID);
        }
        else
        {
            MapObjectRenderData_AddDynamicTextureID(renderData, graphicsID);
        }
    }

    // OFFSET 0x021EE134
    private static void MapObjectRenderData_QueueTextureTransfer(MapObjectRenderData renderData, int graphicsID)
    {
        MapObjectDeferredLoads deferred = renderData.Deferred!;
        MapObjectPendingTransfer[] transfers = deferred.PendingTransfers;
        deferred.Locked = 1;
        for (int i = 0; i < deferred.Count; i++)
        {
            if (transfers[i].TextureManager == null)
            {
                transfers[i].TextureManager = renderData.TextureManager;
                transfers[i].GraphicsID = graphicsID;
                transfers[i].Uploaded = 0;
                deferred.Locked = 0;
                return;
            }
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x021EE170
    private static void MapObjectRenderData_CancelTextureTransfer(MapObjectRenderData renderData, int graphicsID)
    {
        MapObjectDeferredLoads deferred = renderData.Deferred!;
        MapObjectPendingTransfer[] transfers = deferred.PendingTransfers;
        deferred.Locked = 1;
        for (int i = 0; i < deferred.Count; i++)
        {
            if (transfers[i].TextureManager != null && transfers[i].GraphicsID == graphicsID)
            {
                transfers[i].TextureManager = null;
                transfers[i].Uploaded = 0;
                break;
            }
        }
        deferred.Locked = 0;
    }

    // OFFSET 0x021EE1AC
    private static void MapObjectRenderData_UploadTexturesTask(SysTask task, object? param)
    {
        MapObjectRenderData renderData = (MapObjectRenderData)param!;
        MapObjectDeferredLoads deferred = renderData.Deferred!;
        if (deferred.Locked == 1)
        {
            return;
        }
        MapObjectPendingTransfer[] transfers = deferred.PendingTransfers;
        for (int i = 0; i < deferred.Count; i++)
        {
            if (transfers[i].Uploaded == 0 && transfers[i].TextureManager != null)
            {
                TextureResourceManager.TextureResourceManager_UploadTextureWithID(transfers[i].TextureManager, transfers[i].GraphicsID);
                transfers[i].Uploaded = 1;
            }
        }
    }

    // OFFSET 0x021EE1E8
    private static void MapObjectRenderData_DiscardTextureDataTask(SysTask task, object? param)
    {
        MapObjectRenderData renderData = (MapObjectRenderData)param!;
        MapObjectDeferredLoads deferred = renderData.Deferred!;
        MapObjectPendingTransfer[] transfers = deferred.PendingTransfers;
        deferred.Locked = 1;
        for (int i = 0; i < deferred.Count; i++)
        {
            if (transfers[i].Uploaded == 1 && transfers[i].TextureManager != null)
            {
                TextureResourceManager.TextureResourceManager_DiscardTextureDataWithID(transfers[i].TextureManager, transfers[i].GraphicsID);
                transfers[i].Uploaded = 0;
                transfers[i].TextureManager = null;
            }
        }
        deferred.Locked = 0;
    }

    // OFFSET 0x021EE228
    private static void MapObjectRenderData_QueueBillboardAdd(MapObjectRenderData renderData, MapObject mapObject, MapObjectBillboardSlot slot, int graphicsID)
    {
        MapObjectDeferredLoads deferred = renderData.Deferred!;
        MapObjectPendingAdd[] adds = deferred.PendingAdds;
        for (int i = 0; i < deferred.Count; i++)
        {
            if (adds[i].MapObject == null)
            {
                adds[i].MapObject = mapObject;
                adds[i].Slot = slot;
                adds[i].GraphicsID = graphicsID;
                adds[i].Slot!.Billboard = null;
                MapObject_SetBillboardPending(mapObject, 1);
                return;
            }
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x021EE264
    private static void MapObjectRenderData_CancelBillboardAdd(MapObjectRenderData renderData, MapObject mapObject)
    {
        MapObjectDeferredLoads deferred = renderData.Deferred!;
        MapObjectPendingAdd[] adds = deferred.PendingAdds;
        for (int i = 0; i < deferred.Count; i++)
        {
            if (adds[i].MapObject == mapObject)
            {
                adds[i].MapObject = null;
                adds[i].Slot = null;
                return;
            }
        }
    }

    // OFFSET 0x021EE294
    private static void MapObjectRenderData_ProcessBillboardAdds(MapObjectRenderData renderData)
    {
        MapObjectDeferredLoads deferred = renderData.Deferred!;
        MapObjectPendingAdd[] adds = deferred.PendingAdds;
        for (int i = 0; i < deferred.Count; i++)
        {
            if (adds[i].MapObject != null)
            {
                MapObjectRenderData_TryPendingAdd(renderData, adds[i]);
            }
        }
    }

    // OFFSET 0x021EE2C4
    private static void MapObjectRenderData_ProcessBillboardAddsTask(SysTask task, object? param)
    {
        MapObjectRenderData_ProcessBillboardAdds((MapObjectRenderData)param!);
    }

    // OFFSET 0x021EE2D0
    private static void MapObjectRenderData_TryPendingAdd(MapObjectRenderData renderData, MapObjectPendingAdd pendingAdd)
    {
        if (MapObjectRenderData_GetTextureSlotType(renderData, pendingAdd.GraphicsID) == SlotNone)
        {
            return;
        }
        pendingAdd.Slot!.Billboard = MapObject_CreateBillboard(pendingAdd.MapObject!, pendingAdd.GraphicsID);
        if (pendingAdd.Slot.Billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObject_SetBillboardPending(pendingAdd.MapObject!, 0);
        if (MapObject.MapObject_CheckStatusFlag(pendingAdd.MapObject!, StatusDrawSuspended))
        {
            MapObject.MapObject_CallDrawResume(pendingAdd.MapObject!);
        }
        else
        {
            MapObject.MapObject_CallDraw(pendingAdd.MapObject!);
        }
        pendingAdd.MapObject = null;
        pendingAdd.Slot = null;
    }

    // OFFSET 0x021EE320
    private static void MapObjectRenderData_InitDeferredLoads(MapObjectRenderData renderData, int count, int maxLoadsPerFrame)
    {
        MapObjectDeferredLoads deferred = new();
        deferred.Count = (short)count;
        deferred.MaxLoadsPerFrame = (short)maxLoadsPerFrame;
        deferred.PendingAdds = new MapObjectPendingAdd[count];
        deferred.PendingLoads = new MapObjectPendingLoad[count];
        deferred.PendingTransfers = new MapObjectPendingTransfer[count];
        for (int i = 0; i < count; i++)
        {
            deferred.PendingAdds[i] = new MapObjectPendingAdd();
            deferred.PendingLoads[i] = new MapObjectPendingLoad();
            deferred.PendingTransfers[i] = new MapObjectPendingTransfer();
        }
        renderData.Deferred = deferred;
        int priority = MapObjectManager.MapObjectMan_GetTaskBasePriority(renderData.MapObjectManager!);
        deferred.LoadTask = SysTask.SysTask_Start(MapObjectRenderData_LoadQueuedTexturesTask, renderData, (uint)(priority + 1));
        deferred.AddTask = SysTask.SysTask_Start(MapObjectRenderData_ProcessBillboardAddsTask, renderData, (uint)(priority + 2));
        deferred.UploadTask = SysTask.SysTask_ExecuteOnVBlank(MapObjectRenderData_UploadTexturesTask, renderData, TaskPriorityLast);
        deferred.DiscardTask = SysTask.SysTask_ExecuteAfterVBlank(MapObjectRenderData_DiscardTextureDataTask, renderData, TaskPriorityLast);
    }

    // OFFSET 0x021EE3C4
    private static void MapObjectRenderData_FreeDeferredLoads(MapObjectRenderData renderData)
    {
        MapObjectDeferredLoads deferred = renderData.Deferred!;
        deferred.Locked = 1;
        SysTask.SysTask_Done(deferred.LoadTask!);
        SysTask.SysTask_Done(deferred.AddTask!);
        SysTask.SysTask_Done(deferred.UploadTask!);
        SysTask.SysTask_Done(deferred.DiscardTask!);
        renderData.Deferred = null;
    }

    // OFFSET 0x021EE3FC
    public static Billboard? MapObject_ChangeGraphicsKeepingBillboard(MapObject mapObject, int graphicsID, MapObjectBillboardCallback? callback, object? callbackParam)
    {
        if (MapObject_IsBillboardPending(mapObject) != 1)
        {
            if (MapObjectDrawHandlers.MapObject_GetBillboard(mapObject) != null)
            {
                return MapObject_StartGraphicsChange(mapObject, graphicsID, callback, callbackParam);
            }
        }
        int oldGraphicsID = MapObject.MapObject_GetGraphicsID(mapObject);
        MapObjectManager mapObjectManager = MapObject.MapObject_MapObjectManager(mapObject)!;
        MapObject.MapObject_ClearDraw(mapObject);
        MapObjectMan_ReleaseGraphics(mapObjectManager, oldGraphicsID, mapObject);
        MapObject.MapObject_ChangeGraphics(mapObject, graphicsID);
        return null;
    }

    // OFFSET 0x021EE454
    private static Billboard? MapObject_StartGraphicsChange(MapObject mapObject, int graphicsID, MapObjectBillboardCallback? callback, object? callbackParam)
    {
        MapObjectRenderData renderData = MapObject_GetRenderData(mapObject);
        BillboardList? list = MapObjectRenderData_GetBillboardList(renderData);
        Billboard? oldBillboard = MapObjectDrawHandlers.MapObject_GetBillboard(mapObject);
        MapObjectGraphicsChange change = new();
        change.NewGraphicsID = graphicsID;
        change.MapObject = mapObject;
        int oldGraphicsID = MapObject.MapObject_GetGraphicsID(mapObject);
        MapObjectFileEntry modelFile = FindFileEntryOrAssert(ModelFiles(), GetModelIDForGraphics(oldGraphicsID));
        change.ModelData = MapObjectRenderData_LoadResource(renderData, modelFile.MemberIndex, 0);
        MapObjectFileEntry sequenceFile = FindFileEntryOrAssert(SequenceFiles(), GetSequenceIDForGraphics(oldGraphicsID));
        change.SequenceData = MapObjectRenderData_LoadResource(renderData, sequenceFile.MemberIndex, 0);
        BillboardGfxSequence.BillboardGfxSequence_SetData(change.SequenceData, 0, change.GfxSequence);
        MapObjectFileEntry textureFile = FindFileEntryOrAssert(TextureFiles(), oldGraphicsID);
        change.TextureData = MapObjectRenderData_LoadResource(renderData, textureFile.MemberIndex, 0);
        G3dTexture? texture = G3d.GetTexture(change.TextureData);
        Billboards.BillboardResources_SetWithoutKeys(change.Resources, change.ModelData, texture, GetAnimsForGraphics(oldGraphicsID), change.GfxSequence);
        VecFx32 position = Billboards.Billboard_GetPos(oldBillboard);
        change.Billboard = AppendBillboard(list, change.Resources, in position, GraphicsUsesFog(oldGraphicsID));
        if (change.Billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        Billboard? result = change.Billboard;
        Billboards.Billboard_SetRotMatrix(change.Billboard, Billboards.Billboard_GetRotMatrix(oldBillboard));
        Billboards.Billboard_SetAnimNum(change.Billboard, Billboards.Billboard_GetAnimNum(oldBillboard));
        Billboards.Billboard_SetAnimFrameNum(change.Billboard, Billboards.Billboard_GetAnimFrameNum(oldBillboard));
        Billboards.Billboard_SetFrameNum(change.Billboard, Billboards.Billboard_GetFrameNum(oldBillboard));
        Billboards.Billboard_AdvanceAnim(change.Billboard, 0);
        Billboards.Billboard_SetDrawFlag(change.Billboard, 1);
        Billboards.Billboard_TryRequestVRAMTransfer(change.Billboard!);
        if (MapObject.MapObject_IsStatus24On(mapObject) == 1)
        {
            int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, 2);
            int tileBehavior = (byte)MapObjectMove.MapObject_GetTileBehaviorFromDir(mapObject, 1);
            int reflectionType;
            if (TileBehavior.TileBehavior_IsReflective(tileBehavior) == 1)
            {
                reflectionType = 2;
            }
            else if (TileBehavior.TileBehavior_IsPuddle(tileBehavior) == 1)
            {
                reflectionType = 0;
            }
            else
            {
                reflectionType = 1;
            }
            MapObject.MapObject_CopyPos(mapObject, out VecFx32 objectPos);
            FieldEffectManager? fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject);
            change.ReflectionAnim = ReflectionEffect.ReflectionEffect_NewForBillboard(fieldEffectManager, change.Resources, change.Billboard, in objectPos, reflectionType, priority);
        }
        MapObject.MapObject_ClearDraw(mapObject);
        MapObjectMan_ReleaseGraphics(renderData.MapObjectManager!, oldGraphicsID, mapObject);
        MapObject.MapObject_ChangeGraphics(mapObject, graphicsID);
        MapObjectManager.MapObjectMan_GetTaskBasePriority(renderData.MapObjectManager!);
        SysTask? task = SysTask.SysTask_ExecuteAfterVBlank(MapObjectGraphicsChangeTask, change, TaskPriorityLast);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObjectGraphicsChangeTask(task!, change);
        if (Billboards.Billboard_GetState(change.Billboard) == Billboards.StateNone)
        {
            result = null;
        }
        else
        {
            change.CallbackParam = callbackParam;
            change.Callback = callback;
        }
        return result;
    }

    private static void MapObjectGraphicsChange_Finish(SysTask task, MapObjectGraphicsChange change)
    {
        if (change.Callback != null)
        {
            change.Callback(change.CallbackParam, change.Billboard!);
        }
        if (change.ReflectionAnim != null)
        {
            OverworldAnimManagers.OverworldAnimManager_Finish(change.ReflectionAnim);
        }
        Billboards.Billboard_Delete(change.Billboard);
        Heap.Heap_Free(change.ModelData!);
        Heap.Heap_Free(change.SequenceData!);
        Heap.Heap_Free(change.TextureData!);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x021EE698
    private static void MapObjectGraphicsChangeTask(SysTask task, object? param)
    {
        MapObjectGraphicsChange change = (MapObjectGraphicsChange)param!;
        MapObject mapObject = change.MapObject!;
        switch (change.State)
        {
            case 0:
                if (MapObject.MapObject_IsActive(mapObject) != 0 && MapObject_IsBillboardPending(mapObject) != 0)
                {
                    if (change.NewGraphicsID != MapObject.MapObject_GetGraphicsID(mapObject))
                    {
                        change.State++;
                    }
                    return;
                }
                MapObjectGraphicsChange_Finish(task, change);
                break;
            case 1:
                if (MapObject_IsBillboardPending(mapObject) != 0)
                {
                    return;
                }
                MapObjectGraphicsChange_Finish(task, change);
                break;
        }
    }
}
