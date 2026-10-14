namespace PlatinumSharp.Game;

public static class SpriteResourceManager
{
    private const int VramSub = 2;
    private const int SubScreenOffset = 192 << 12;
    private const int FxOne = 0x1000;
    private const int TypesWithoutMultiCells = 4;

    private const int PathMultiCell = 4;
    private const int PathResdat = 6;
    private const uint AffineScaleTable = 0x021F89C0;
    private const int OverlayField = 5;

    // OFFSET 0x021D2F14
    public static void SpriteResourceManager_Init(Overlay5SpriteManager manager, string?[] resourcePaths, int maxSprites, int heapID)
    {
        manager.SpriteList = SpriteUtil.SpriteList_InitRendering(maxSprites, manager.Renderer, heapID);
        manager.HeapID = (ushort)heapID;
        manager.NumTypes = (ushort)(resourcePaths[PathMultiCell] == null ? TypesWithoutMultiCells : SpriteManager.ResourceTypes);
        SpriteResourceTable[] tables = new SpriteResourceTable[manager.NumTypes];
        for (int i = 0; i < manager.NumTypes; i++)
        {
            tables[i] = new SpriteResourceTable();
            SpriteResourceTable table = SpriteResourceTable.SpriteResourceTable_GetArrayElement(tables, i);
            byte[] data = GameSystem.ReadFileToHeap(heapID, resourcePaths[i]!)!;
            SpriteResourceTable.SpriteResourceTable_LoadFromBinary(data, table, heapID);
        }
        for (int i = 0; i < manager.NumTypes; i++)
        {
            int count = SpriteResourceTable.SpriteResourceTable_GetCount(SpriteResourceTable.SpriteResourceTable_GetArrayElement(tables, i));
            manager.Collections[i] = SpriteResourceCollection.SpriteResourceCollection_New(count, i, heapID);
        }
        for (int i = 0; i < manager.NumTypes; i++)
        {
            SpriteResourceTable table = SpriteResourceTable.SpriteResourceTable_GetArrayElement(tables, i);
            manager.Lists[i] = SpriteResourceList.SpriteResourceList_New(SpriteResourceTable.SpriteResourceTable_GetCount(table), heapID);
            SpriteResourceCollection.SpriteResourceCollection_Extend(manager.Collections[i], table, manager.Lists[i], heapID);
        }
        for (int i = 0; i < manager.NumTypes; i++)
        {
            SpriteResourceTable.SpriteResourceTable_Clear(SpriteResourceTable.SpriteResourceTable_GetArrayElement(tables, i));
        }
        SpriteTransfer.SpriteTransfer_RequestCharListAtEnd(manager.Lists[SpriteResource.TypeChar]);
        SpriteTransfer.SpriteTransfer_RequestPlttFreeSpaceList(manager.Lists[SpriteResource.TypePltt]);
        byte[] resdat = GameSystem.ReadFileToHeap(heapID, resourcePaths[PathResdat]!)!;
        manager.Headers = SpriteUtil.SpriteResourcesHeaderList_NewFromResdat(
            resdat,
            heapID,
            manager.Collections[SpriteResource.TypeChar],
            manager.Collections[SpriteResource.TypePltt],
            manager.Collections[SpriteResource.TypeCell],
            manager.Collections[SpriteResource.TypeAnim],
            manager.Collections[SpriteResource.TypeMultiCell],
            manager.Collections[SpriteResource.TypeMultiAnim]);
    }

    // OFFSET 0x021D30A8
    public static void SpriteResourceManager_Free(Overlay5SpriteManager manager)
    {
        Sprite.SpriteList_Delete(manager.SpriteList);
        SpriteUtil.SpriteResourcesHeaderList_Free(manager.Headers);
        SpriteTransfer.SpriteTransfer_ResetCharTransferList(manager.Lists[SpriteResource.TypeChar]);
        SpriteTransfer.SpriteTransfer_ResetPlttTransferList(manager.Lists[SpriteResource.TypePltt]);
        for (int i = 0; i < manager.NumTypes; i++)
        {
            SpriteResourceList.SpriteResourceList_Delete(manager.Lists[i]!);
            SpriteResourceCollection.SpriteResourceCollection_Delete(manager.Collections[i]);
        }
    }

    // OFFSET 0x021D3104
    public static Sprite SpriteResourceManager_CreateSprite(Overlay5SpriteManager manager, SpriteTemplateFromResourceHeader template)
    {
        uint[] scale = RomData.OverlayU32(OverlayField, AffineScaleTable, 3);
        AffineSpriteListTemplate listTemplate = new();
        listTemplate.List = manager.SpriteList;
        listTemplate.ResourceData = manager.Headers!.Headers![template.ResourceHeaderID];
        listTemplate.Position.X = template.X << 12;
        listTemplate.Position.Y = template.Y << 12;
        listTemplate.Position.Z = template.Z << 12;
        listTemplate.AffineScale.X = (int)scale[0];
        listTemplate.AffineScale.Y = (int)scale[1];
        listTemplate.AffineScale.Z = (int)scale[2];
        listTemplate.AffineZRotation = 0;
        listTemplate.Priority = template.Priority;
        listTemplate.VramType = template.VramType;
        listTemplate.HeapID = manager.HeapID;
        Sprite? sprite = Sprite.SpriteList_AddAffine(listTemplate);
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        Sprite.Sprite_SetAnim(sprite!, template.AnimIdx);
        Sprite.Sprite_SetExplicitPaletteWithOffset(sprite!, (uint)template.PlttIdx);
        return sprite!;
    }

    // OFFSET 0x021D3190
    public static void SpriteResourceManager_SetCapacities(Overlay5SpriteManager manager, uint[] capacities, int maxSprites, int heapID)
    {
        manager.SpriteList = SpriteUtil.SpriteList_InitRendering(maxSprites, manager.Renderer, heapID);
        manager.HeapID = (ushort)heapID;
        if (capacities[SpriteResource.TypeMultiCell] == 0 || capacities[SpriteResource.TypeMultiAnim] == 0)
        {
            manager.NumTypes = TypesWithoutMultiCells;
            manager.Collections[SpriteResource.TypeMultiCell] = null;
            manager.Collections[SpriteResource.TypeMultiAnim] = null;
        }
        else
        {
            manager.NumTypes = SpriteManager.ResourceTypes;
        }
        for (int i = 0; i < manager.NumTypes; i++)
        {
            manager.Collections[i] = SpriteResourceCollection.SpriteResourceCollection_New((int)capacities[i], i, heapID);
        }
        for (int i = 0; i < manager.NumTypes; i++)
        {
            if (capacities[i] == 0)
            {
                continue;
            }
            manager.Lists[i] = SpriteResourceList.SpriteResourceList_New((int)capacities[i], heapID);
            SpriteResourceList list = manager.Lists[i]!;
            for (int j = 0; j < list.Capacity; j++)
            {
                list.Resources[j] = null;
            }
        }
    }

    // OFFSET 0x021D3270
    public static int SpriteResourceManager_AddPalette(Overlay5SpriteManager manager, int narcID, int memberIdx, int compressed, int paletteCount, int vramType, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(manager.Collections[SpriteResource.TypePltt], resourceID))
        {
            ErrorHandling.GF_AssertFail();
            return 0;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_AddPalette(manager.Collections[SpriteResource.TypePltt], narcID, memberIdx, compressed, resourceID, vramType, paletteCount, manager.HeapID);
        if (resource != null)
        {
            if (SpriteTransfer.SpriteTransfer_RequestPlttFreeSpace(resource) != true)
            {
                ErrorHandling.GF_AssertFail();
            }
            SpriteResourceList_AddResource(manager.Lists[SpriteResource.TypePltt]!, resource);
            return SpriteTransfer.SpriteTransfer_GetPlttOffset(resource, vramType);
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x021D32E8
    public static int SpriteResourceManager_LoadPalette(Overlay5SpriteManager manager, NARC narc, int memberIdx, int compressed, int paletteCount, int vramType, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(manager.Collections[SpriteResource.TypePltt], resourceID))
        {
            ErrorHandling.GF_AssertFail();
            return 0;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_AddPaletteFrom(manager.Collections[SpriteResource.TypePltt], narc, memberIdx, compressed, resourceID, vramType, paletteCount, manager.HeapID);
        if (resource != null)
        {
            if (SpriteTransfer.SpriteTransfer_RequestPlttFreeSpace(resource) != true)
            {
                ErrorHandling.GF_AssertFail();
            }
            SpriteResourceList_AddResource(manager.Lists[SpriteResource.TypePltt]!, resource);
            return SpriteTransfer.SpriteTransfer_GetPlttOffset(resource, vramType);
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x021D3360
    public static void SpriteResourceManager_AddCell(Overlay5SpriteManager manager, int narcID, int memberIdx, int compressed, int resourceID)
    {
        SpriteResourceManager_AddResourceByType(manager, narcID, memberIdx, compressed, SpriteResource.TypeCell, resourceID);
    }

    // OFFSET 0x021D3374
    public static void SpriteResourceManager_LoadCell(Overlay5SpriteManager manager, NARC narc, int memberIdx, int compressed, int resourceID)
    {
        SpriteResourceManager_LoadResourceByType(manager, narc, memberIdx, compressed, SpriteResource.TypeCell, resourceID);
    }

    // OFFSET 0x021D3388
    public static void SpriteResourceManager_AddAnimation(Overlay5SpriteManager manager, int narcID, int memberIdx, int compressed, int resourceID)
    {
        SpriteResourceManager_AddResourceByType(manager, narcID, memberIdx, compressed, SpriteResource.TypeAnim, resourceID);
    }

    // OFFSET 0x021D339C
    public static void SpriteResourceManager_LoadAnimation(Overlay5SpriteManager manager, NARC narc, int memberIdx, int compressed, int resourceID)
    {
        SpriteResourceManager_LoadResourceByType(manager, narc, memberIdx, compressed, SpriteResource.TypeAnim, resourceID);
    }

    // OFFSET 0x021D33B0
    public static void SpriteResourceManager_AddTiles(Overlay5SpriteManager manager, int narcID, int memberIdx, int compressed, int vramType, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(manager.Collections[SpriteResource.TypeChar], resourceID))
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_AddTiles(manager.Collections[SpriteResource.TypeChar], narcID, memberIdx, compressed, resourceID, vramType, manager.HeapID);
        if (resource != null)
        {
            SpriteTransfer.SpriteTransfer_RequestCharAtEnd(resource);
            SpriteResourceList_AddResource(manager.Lists[SpriteResource.TypeChar]!, resource);
            return;
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x021D3414
    public static void SpriteResourceManager_LoadTiles(Overlay5SpriteManager manager, NARC narc, int memberIdx, int compressed, int vramType, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(manager.Collections[SpriteResource.TypeChar], resourceID))
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_AddTilesFrom(manager.Collections[SpriteResource.TypeChar], narc, memberIdx, compressed, resourceID, vramType, manager.HeapID);
        if (resource != null)
        {
            SpriteTransfer.SpriteTransfer_RequestCharAtEnd(resource);
            SpriteResourceList_AddResource(manager.Lists[SpriteResource.TypeChar]!, resource);
            return;
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x021D3478
    private static bool SpriteResourceList_AddResource(SpriteResourceList list, SpriteResource resource)
    {
        for (int i = 0; i < list.Capacity; i++)
        {
            if (list.Resources[i] == null)
            {
                list.Resources[i] = resource;
                list.Count++;
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x021D34AC
    private static void SpriteResourceManager_AddResourceByType(Overlay5SpriteManager manager, int narcID, int memberIdx, int compressed, int type, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(manager.Collections[type], resourceID))
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_Add(manager.Collections[type], narcID, memberIdx, compressed, resourceID, type, manager.HeapID);
        if (resource != null)
        {
            if (SpriteResourceList_AddResource(manager.Lists[type]!, resource) != true)
            {
                ErrorHandling.GF_AssertFail();
            }
            return;
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x021D3518
    private static void SpriteResourceManager_LoadResourceByType(Overlay5SpriteManager manager, NARC narc, int memberIdx, int compressed, int type, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(manager.Collections[type], resourceID))
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_AddFrom(manager.Collections[type], narc, memberIdx, compressed, resourceID, type, manager.HeapID);
        if (resource != null)
        {
            if (SpriteResourceList_AddResource(manager.Lists[type]!, resource) != true)
            {
                ErrorHandling.GF_AssertFail();
            }
            return;
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x021D3584
    public static ManagedSprite SpriteResourceManager_CreateManagedSprite(Overlay5SpriteManager manager, SpriteTemplate template)
    {
        ManagedSprite managedSprite = new();
        managedSprite.HeaderList = new SpriteResourcesHeaderList();
        managedSprite.HeaderList.Headers = new[] { new SpriteResourcesHeader() };
        managedSprite.Header = managedSprite.HeaderList.Headers[0];
        int[] resourceIDs = new int[SpriteManager.ResourceTypes];
        for (int i = 0; i < SpriteManager.ResourceTypes; i++)
        {
            resourceIDs[i] = template.Resources[i];
        }
        if (manager.Collections[SpriteResource.TypeMultiCell] == null || manager.Collections[SpriteResource.TypeMultiAnim] == null)
        {
            resourceIDs[SpriteResource.TypeMultiCell] = -1;
            resourceIDs[SpriteResource.TypeMultiAnim] = -1;
        }
        else
        {
            if (resourceIDs[SpriteResource.TypeMultiCell] != -1
                && !SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(manager.Collections[SpriteResource.TypeMultiCell], resourceIDs[SpriteResource.TypeMultiCell]))
            {
                resourceIDs[SpriteResource.TypeMultiCell] = -1;
            }
            if (resourceIDs[SpriteResource.TypeMultiAnim] != -1
                && !SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(manager.Collections[SpriteResource.TypeMultiAnim], resourceIDs[SpriteResource.TypeMultiAnim]))
            {
                resourceIDs[SpriteResource.TypeMultiAnim] = -1;
            }
        }
        SpriteUtil.SpriteResourcesHeader_Init(
            managedSprite.Header,
            resourceIDs[SpriteResource.TypeChar],
            resourceIDs[SpriteResource.TypePltt],
            resourceIDs[SpriteResource.TypeCell],
            resourceIDs[SpriteResource.TypeAnim],
            resourceIDs[SpriteResource.TypeMultiCell],
            resourceIDs[SpriteResource.TypeMultiAnim],
            template.VramTransfer,
            template.BgPriority,
            manager.Collections[SpriteResource.TypeChar],
            manager.Collections[SpriteResource.TypePltt],
            manager.Collections[SpriteResource.TypeCell],
            manager.Collections[SpriteResource.TypeAnim],
            manager.Collections[SpriteResource.TypeMultiCell],
            manager.Collections[SpriteResource.TypeMultiAnim]);
        AffineSpriteListTemplate listTemplate = new();
        listTemplate.List = manager.SpriteList;
        listTemplate.ResourceData = managedSprite.Header;
        listTemplate.Position.X = template.X * FxOne;
        listTemplate.Position.Y = template.Y * FxOne;
        listTemplate.Position.Z = template.Z * FxOne;
        if (template.VramType == VramSub)
        {
            listTemplate.Position.Y += SubScreenOffset;
        }
        listTemplate.AffineScale.X = FxOne;
        listTemplate.AffineScale.Y = FxOne;
        listTemplate.AffineScale.Z = FxOne;
        listTemplate.AffineZRotation = 0;
        listTemplate.Priority = template.Priority;
        listTemplate.VramType = template.VramType;
        listTemplate.HeapID = manager.HeapID;
        managedSprite.Sprite = Sprite.SpriteList_AddAffine(listTemplate);
        if (managedSprite.Sprite != null)
        {
            uint palette = Sprite.Sprite_GetExplicitPalette(managedSprite.Sprite);
            Sprite.Sprite_SetAnim(managedSprite.Sprite, template.AnimIdx);
            Sprite.Sprite_SetExplicitPalette(managedSprite.Sprite, palette + (uint)template.PlttIdx);
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        return managedSprite;
    }

    // OFFSET 0x021D375C
    public static void SpriteResourceManager_Cleanup(Overlay5SpriteManager manager)
    {
        Sprite.SpriteList_Delete(manager.SpriteList);
        SpriteTransfer.SpriteTransfer_ResetCharTransferList(manager.Lists[SpriteResource.TypeChar]);
        SpriteTransfer.SpriteTransfer_ResetPlttTransferList(manager.Lists[SpriteResource.TypePltt]);
        for (int i = 0; i < manager.NumTypes; i++)
        {
            SpriteResourceList.SpriteResourceList_Delete(manager.Lists[i]!);
            SpriteResourceCollection.SpriteResourceCollection_Delete(manager.Collections[i]);
        }
    }
}
