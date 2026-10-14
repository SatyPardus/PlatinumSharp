using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public struct VecFx32
{
    public int X;
    public int Y;
    public int Z;
}

public sealed class AffineSpriteListTemplate
{
    public SpriteList? List;
    public SpriteResourcesHeader? ResourceData;
    public VecFx32 Position;
    public VecFx32 AffineScale;
    public ushort AffineZRotation;
    public int Priority;
    public int VramType;
    public int HeapID;
}

public sealed class SpriteListTemplate
{
    public SpriteList? List;
    public SpriteResourcesHeader? ResourceData;
    public VecFx32 Position;
    public int Priority;
    public int VramType;
    public int HeapID;
}

public sealed class RenderOamTemplate
{
    public int MainOamStart;
    public int MainOamCount;
    public int MainAffineOamStart;
    public int MainAffineOamCount;
    public int SubOamStart;
    public int SubOamCount;
    public int SubAffineOamStart;
    public int SubAffineOamCount;
}

public sealed class CharTransferTemplate
{
    public int MaxTasks;
    public int SizeMain;
    public int SizeSub;
    public int HeapID;
}

public sealed class CharTransferTemplateWithModes
{
    public int MaxTasks;
    public int SizeMain;
    public int SizeSub;
    public int ModeMain;
    public int ModeSub;
}

public sealed class SpriteTemplateFromResourceHeader
{
    public int ResourceHeaderID;
    public short X;
    public short Y;
    public short Z;
    public ushort AnimIdx;
    public int Priority;
    public int PlttIdx;
    public int VramType;
    public int Unused18;
    public int Unused1C;
    public int Unused20;
    public int Unused24;
}

public sealed class SpriteTemplate
{
    public short X;
    public short Y;
    public short Z;
    public ushort AnimIdx;
    public int Priority;
    public int PlttIdx;
    public int VramType;
    public readonly int[] Resources = new int[SpriteManager.ResourceTypes];
    public int BgPriority;
    public int VramTransfer;
}

public sealed class ManagedSprite
{
    public Sprite? Sprite;
    public SpriteResourcesHeader? Header;
    public SpriteResourcesHeaderList? HeaderList;
    public int VramTransfer;
}

public sealed class SpriteManager
{
    public const int ResourceTypes = 6;

    public SpriteList? SpriteList;
    public SpriteResourcesHeaderList? Headers;
    public SpriteResourceTable[]? Tables;
    public readonly SpriteResourceCollection?[] Collections = new SpriteResourceCollection?[ResourceTypes];
    public readonly SpriteResourceList?[] Lists = new SpriteResourceList?[ResourceTypes];
    public readonly int[] Loaded = new int[ResourceTypes];
    public int NumTypes;
}

public sealed class SpriteSystem
{
    private const int VramSub = 2;
    private const int SubScreenOffset = 192 << 12;
    private const int FxOne = 0x1000;
    private const int ResdatPath = 6;

    public int HeapID;
    public int ManagerCount;
    public int HasOamManager;
    public CellTransferState[]? CellTransferState;
    public readonly G2dRenderer Renderer = new();

    // OFFSET 0x0200C6E4
    public static SpriteSystem SpriteSystem_Alloc(int heapID)
    {
        SpriteSystem spriteSys = new();
        spriteSys.HeapID = heapID;
        spriteSys.ManagerCount = 0;
        spriteSys.HasOamManager = 1;
        return spriteSys;
    }

    // OFFSET 0x0200C704
    public static SpriteManager SpriteManager_New(SpriteSystem? spriteSys)
    {
        if (spriteSys == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteManager spriteMan = new();
        spriteSys!.ManagerCount++;
        for (int i = 0; i < SpriteManager.ResourceTypes; i++)
        {
            spriteMan.Collections[i] = null;
        }
        return spriteMan;
    }

    // OFFSET 0x0200C738
    public static G2dRenderer SpriteSystem_GetRenderer(SpriteSystem spriteSys)
    {
        return spriteSys.Renderer;
    }

    // OFFSET 0x0200C73C
    public static bool SpriteSystem_Init(SpriteSystem? spriteSys, RenderOamTemplate oamTemplate, CharTransferTemplateWithModes transferTemplate, int numPalettes)
    {
        if (spriteSys == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (spriteSys == null)
        {
            return false;
        }
        CharTransferTemplate charTemplate = new()
        {
            MaxTasks = transferTemplate.MaxTasks,
            SizeMain = transferTemplate.SizeMain,
            SizeSub = transferTemplate.SizeSub,
            HeapID = spriteSys.HeapID,
        };
        CharTransfer.CharTransfer_InitWithVramModes(charTemplate, transferTemplate.ModeMain, transferTemplate.ModeSub);
        PlttTransfer.PlttTransfer_Init(numPalettes, spriteSys.HeapID);
        if (spriteSys.HasOamManager == 1)
        {
            RenderOam.RenderOam_Init(
                oamTemplate.MainOamStart,
                oamTemplate.MainOamCount,
                oamTemplate.MainAffineOamStart,
                oamTemplate.MainAffineOamCount,
                oamTemplate.SubOamStart,
                oamTemplate.SubOamCount,
                oamTemplate.SubAffineOamStart,
                oamTemplate.SubAffineOamCount,
                spriteSys.HeapID);
        }
        spriteSys.CellTransferState = CellTransfer.CellTransfer_New(0x20, spriteSys.HeapID);
        CharTransfer.CharTransfer_ClearBuffers();
        PlttTransfer.PlttTransfer_Clear();
        return true;
    }

    // OFFSET 0x0200C7C0
    public static bool SpriteSystem_InitSprites(SpriteSystem? spriteSys, SpriteManager? spriteMan, int maxSprites)
    {
        if (spriteSys == null || spriteMan == null)
        {
            return false;
        }
        spriteMan.SpriteList = SpriteUtil.SpriteList_InitRendering(maxSprites, spriteSys.Renderer, spriteSys.HeapID);
        return true;
    }

    // OFFSET 0x0200C7E4
    public static void Sprite_Delete2(Sprite sprite)
    {
        Sprite.Sprite_Delete(sprite);
    }

    // OFFSET 0x0200C7EC
    public static void SpriteSystem_DrawSprites(SpriteManager? spriteMan)
    {
        if (spriteMan == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        Sprite.SpriteList_Update(spriteMan!.SpriteList!);
    }

    // OFFSET 0x0200C800
    public static void SpriteSystem_TransferOam()
    {
        RenderOam.RenderOam_Transfer();
    }

    // OFFSET 0x0200C808
    public static void SpriteSystem_UpdateTransfer()
    {
        CellTransfer.CellTransfer_Update();
    }

    // OFFSET 0x0200C810
    public static void SpriteManager_DeleteAllSprites(SpriteManager spriteMan)
    {
        Sprite.SpriteList_Delete(spriteMan.SpriteList!);
    }

    // OFFSET 0x0200C81C
    public static void SpriteManager_FreeResourceHeaders(SpriteManager spriteMan)
    {
        if (spriteMan.Headers != null)
        {
            SpriteUtil.SpriteResourcesHeaderList_Free(spriteMan.Headers);
        }
    }

    // OFFSET 0x0200C82C
    public static void SpriteManager_FreeResources(SpriteManager spriteMan)
    {
        for (int i = 0; i < spriteMan.NumTypes; i++)
        {
            SpriteResourceTable.SpriteResourceTable_Clear(SpriteResourceTable.SpriteResourceTable_GetArrayElement(spriteMan.Tables!, i));
        }
        spriteMan.Tables = null;
        SpriteTransfer.SpriteTransfer_ResetCharTransferList(spriteMan.Lists[SpriteResource.TypeChar]);
        SpriteTransfer.SpriteTransfer_ResetPlttTransferList(spriteMan.Lists[SpriteResource.TypePltt]);
        for (int i = 0; i < spriteMan.NumTypes; i++)
        {
            SpriteResourceList.SpriteResourceList_Delete(spriteMan.Lists[i]!);
            SpriteResourceCollection.SpriteResourceCollection_Delete(spriteMan.Collections[i]);
        }
    }

    // OFFSET 0x0200C880
    public static void SpriteSystem_FreeVramTransfers(SpriteSystem spriteSys)
    {
        CellTransfer.CellTransfer_Free(spriteSys.CellTransferState);
        CharTransfer.CharTransfer_Free();
        PlttTransfer.PlttTransfer_Free();
        if (spriteSys.HasOamManager == 1)
        {
            RenderOam.RenderOam_Free();
        }
    }

    // OFFSET 0x0200C8A0
    public static void SpriteSystem_FreeSpriteManager(SpriteSystem spriteSys, SpriteManager spriteMan)
    {
        spriteSys.ManagerCount--;
    }

    // OFFSET 0x0200C8B0
    public static void SpriteSystem_DestroySpriteManager(SpriteSystem spriteSys, SpriteManager spriteMan)
    {
        SpriteManager_DeleteAllSprites(spriteMan);
        SpriteManager_FreeResourceHeaders(spriteMan);
        SpriteManager_FreeResources(spriteMan);
        SpriteSystem_FreeSpriteManager(spriteSys, spriteMan);
    }

    // OFFSET 0x0200C8D4
    public static void SpriteSystem_Free(SpriteSystem spriteSys)
    {
        if (spriteSys.ManagerCount != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteSystem_FreeVramTransfers(spriteSys);
    }

    // OFFSET 0x0200C8F0
    public static bool SpriteSystem_LoadResourceDataFromFilepaths(SpriteSystem? spriteSys, SpriteManager? spriteMan, string?[] paths)
    {
        int numTypes = SpriteManager.ResourceTypes;
        if (spriteSys == null || spriteMan == null)
        {
            return false;
        }
        if (paths[SpriteResource.TypeMultiCell] == null)
        {
            numTypes = 4;
        }
        spriteMan.NumTypes = numTypes;
        spriteMan.Tables = new SpriteResourceTable[numTypes];
        for (int i = 0; i < numTypes; i++)
        {
            spriteMan.Tables[i] = new SpriteResourceTable();
            SpriteResourceTable table = SpriteResourceTable.SpriteResourceTable_GetArrayElement(spriteMan.Tables, i);
            byte[] data = GameSystem.ReadFileToHeap(spriteSys.HeapID, paths[i]!)!;
            SpriteResourceTable.SpriteResourceTable_LoadFromBinary(data, table, spriteSys.HeapID);
        }
        for (int i = 0; i < numTypes; i++)
        {
            SpriteResourceTable table = SpriteResourceTable.SpriteResourceTable_GetArrayElement(spriteMan.Tables, i);
            int count = SpriteResourceTable.SpriteResourceTable_GetCount(table);
            spriteMan.Collections[i] = SpriteResourceCollection.SpriteResourceCollection_New(count, i, spriteSys.HeapID);
        }
        for (int i = 0; i < numTypes; i++)
        {
            SpriteResourceTable table = SpriteResourceTable.SpriteResourceTable_GetArrayElement(spriteMan.Tables, i);
            int count = SpriteResourceTable.SpriteResourceTable_GetCount(table);
            spriteMan.Lists[i] = SpriteResourceList.SpriteResourceList_New(count, spriteSys.HeapID);
            spriteMan.Loaded[i] = SpriteResourceCollection.SpriteResourceCollection_Extend(spriteMan.Collections[i], table, spriteMan.Lists[i], spriteSys.HeapID);
        }
        SpriteTransfer.SpriteTransfer_RequestCharList(spriteMan.Lists[SpriteResource.TypeChar]);
        SpriteTransfer.SpriteTransfer_RequestPlttWholeRangeList(spriteMan.Lists[SpriteResource.TypePltt]);
        byte[] resdat = GameSystem.ReadFileToHeap(spriteSys.HeapID, paths[ResdatPath]!)!;
        spriteMan.Headers = SpriteUtil.SpriteResourcesHeaderList_NewFromResdat(
            resdat,
            spriteSys.HeapID,
            spriteMan.Collections[SpriteResource.TypeChar],
            spriteMan.Collections[SpriteResource.TypePltt],
            spriteMan.Collections[SpriteResource.TypeCell],
            spriteMan.Collections[SpriteResource.TypeAnim],
            spriteMan.Collections[SpriteResource.TypeMultiCell],
            spriteMan.Collections[SpriteResource.TypeMultiAnim]);
        return true;
    }

    // OFFSET 0x0200CA08
    public static Sprite? SpriteSystem_NewSpriteFromResourceHeader(SpriteSystem spriteSys, SpriteManager spriteMan, SpriteTemplateFromResourceHeader template)
    {
        return CreateSpriteFromResourceHeader(
            spriteSys,
            spriteMan,
            template.ResourceHeaderID,
            template.X,
            template.Y,
            template.X,
            template.AnimIdx,
            template.Priority,
            template.PlttIdx,
            template.VramType,
            template.Unused18,
            template.Unused1C,
            template.Unused20,
            template.Unused24);
    }

    // OFFSET 0x0200CA44
    private static Sprite? CreateSpriteFromResourceHeader(SpriteSystem spriteSys, SpriteManager spriteMan, int resourceHeaderID, short x, short y, short z, ushort animIdx, int priority, int plttIdx, int vramType, int unused1, int unused2, int unused3, int unused4)
    {
        AffineSpriteListTemplate template = new();
        template.List = spriteMan.SpriteList;
        template.ResourceData = spriteMan.Headers!.Headers![resourceHeaderID];
        template.Position.X = x * FxOne;
        template.Position.Y = y * FxOne;
        template.Position.Z = z * FxOne;
        if (vramType == VramSub)
        {
            template.Position.Y += SubScreenOffset;
        }
        template.AffineScale.X = FxOne;
        template.AffineScale.Y = FxOne;
        template.AffineScale.Z = FxOne;
        template.AffineZRotation = 0;
        template.Priority = priority;
        template.VramType = vramType;
        template.HeapID = spriteSys.HeapID;
        Sprite? sprite = Sprite.SpriteList_AddAffine(template);
        if (sprite != null)
        {
            Sprite.Sprite_SetAnim(sprite, animIdx);
            Sprite.Sprite_SetExplicitPalette(sprite, (uint)plttIdx);
        }
        return sprite;
    }

    // OFFSET 0x0200CB30
    public static bool SpriteSystem_InitManagerWithCapacities(SpriteSystem? spriteSys, SpriteManager? spriteMan, int[] capacities)
    {
        int numTypes = SpriteManager.ResourceTypes;
        if (spriteSys == null || spriteMan == null)
        {
            return false;
        }
        if (capacities[SpriteResource.TypeMultiCell] == 0 || capacities[SpriteResource.TypeMultiAnim] == 0)
        {
            numTypes = 4;
        }
        spriteMan.NumTypes = numTypes;
        for (int i = 0; i < numTypes; i++)
        {
            spriteMan.Collections[i] = SpriteResourceCollection.SpriteResourceCollection_New(capacities[i], i, spriteSys.HeapID);
        }
        for (int i = 0; i < numTypes; i++)
        {
            if (capacities[i] == 0)
            {
                continue;
            }
            spriteMan.Lists[i] = SpriteResourceList.SpriteResourceList_New(capacities[i], spriteSys.HeapID);
            spriteMan.Loaded[i] = 0;
            for (int j = 0; j < spriteMan.Lists[i]!.Capacity; j++)
            {
                spriteMan.Lists[i]!.Resources[j] = null;
            }
        }
        return true;
    }

    // OFFSET 0x0200CBDC
    public static bool SpriteSystem_LoadCharResObj(SpriteSystem spriteSys, SpriteManager spriteMan, int narcID, int memberIdx, int compressed, int vramType, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(spriteMan.Collections[SpriteResource.TypeChar], resourceID))
        {
            return false;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_AddTiles(spriteMan.Collections[SpriteResource.TypeChar], narcID, memberIdx, compressed, resourceID, vramType, spriteSys.HeapID);
        if (resource != null)
        {
            SpriteTransfer.SpriteTransfer_RequestCharAtEnd(resource);
            RegisterLoadedResource(spriteMan.Lists[SpriteResource.TypeChar]!, resource);
            return true;
        }
        ErrorHandling.GF_AssertFail();
        return resource != null;
    }

    // OFFSET 0x0200CC3C
    public static bool SpriteSystem_LoadCharResObjFromOpenNarc(SpriteSystem spriteSys, SpriteManager spriteMan, NARC narc, int memberIdx, int compressed, int vramType, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(spriteMan.Collections[SpriteResource.TypeChar], resourceID))
        {
            return false;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_AddTilesFrom(spriteMan.Collections[SpriteResource.TypeChar], narc, memberIdx, compressed, resourceID, vramType, spriteSys.HeapID);
        if (resource != null)
        {
            SpriteTransfer.SpriteTransfer_RequestCharAtEnd(resource);
            RegisterLoadedResource(spriteMan.Lists[SpriteResource.TypeChar]!, resource);
            return true;
        }
        ErrorHandling.GF_AssertFail();
        return resource != null;
    }

    // OFFSET 0x0200CC9C
    public static sbyte SpriteSystem_LoadPlttResObj(SpriteSystem spriteSys, SpriteManager spriteMan, int narcID, int memberIdx, int compressed, int paletteCount, int vramType, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(spriteMan.Collections[SpriteResource.TypePltt], resourceID))
        {
            return -1;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_AddPalette(spriteMan.Collections[SpriteResource.TypePltt], narcID, memberIdx, compressed, resourceID, vramType, paletteCount, spriteSys.HeapID);
        if (resource != null)
        {
            if (!SpriteTransfer.SpriteTransfer_RequestPlttFreeSpace(resource))
            {
                ErrorHandling.GF_AssertFail();
            }
            RegisterLoadedResource(spriteMan.Lists[SpriteResource.TypePltt]!, resource);
            return (sbyte)SpriteTransfer.SpriteTransfer_GetPlttOffset(resource, vramType);
        }
        ErrorHandling.GF_AssertFail();
        return -1;
    }

    // OFFSET 0x0200CD0C
    public static sbyte SpriteSystem_LoadPlttResObjFromOpenNarc(SpriteSystem spriteSys, SpriteManager spriteMan, NARC narc, int memberIdx, int compressed, int paletteCount, int vramType, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(spriteMan.Collections[SpriteResource.TypePltt], resourceID))
        {
            return -1;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_AddPaletteFrom(spriteMan.Collections[SpriteResource.TypePltt], narc, memberIdx, compressed, resourceID, vramType, paletteCount, spriteSys.HeapID);
        if (resource != null)
        {
            if (!SpriteTransfer.SpriteTransfer_RequestPlttFreeSpace(resource))
            {
                ErrorHandling.GF_AssertFail();
            }
            RegisterLoadedResource(spriteMan.Lists[SpriteResource.TypePltt]!, resource);
            return (sbyte)SpriteTransfer.SpriteTransfer_GetPlttOffset(resource, vramType);
        }
        ErrorHandling.GF_AssertFail();
        return -1;
    }

    // OFFSET 0x0200CD7C
    public static byte SpriteSystem_LoadPaletteBuffer(PaletteData paletteData, int bufferID, SpriteSystem spriteSys, SpriteManager spriteMan, int narcID, int memberIdx, int compressed, int paletteCount, int vramType, int resourceID)
    {
        int offset = SpriteSystem_LoadPlttResObj(spriteSys, spriteMan, narcID, memberIdx, compressed, paletteCount, vramType, resourceID);
        if (offset != -1)
        {
            PaletteData.PaletteData_LoadBufferFromHardware(paletteData, bufferID, (ushort)(offset * 16), (uint)(paletteCount << 5));
        }
        return (byte)offset;
    }

    // OFFSET 0x0200CDC4
    public static byte SpriteSystem_LoadPaletteBufferFromOpenNarc(PaletteData paletteData, int bufferID, SpriteSystem spriteSys, SpriteManager spriteMan, NARC narc, int memberIdx, int compressed, int paletteCount, int vramType, int resourceID)
    {
        int offset = SpriteSystem_LoadPlttResObjFromOpenNarc(spriteSys, spriteMan, narc, memberIdx, compressed, paletteCount, vramType, resourceID);
        if (offset != -1)
        {
            PaletteData.PaletteData_LoadBufferFromHardware(paletteData, bufferID, (ushort)(offset * 16), (uint)(paletteCount << 5));
        }
        return (byte)offset;
    }

    // OFFSET 0x0200CE0C
    public static bool SpriteSystem_LoadCellResObj(SpriteSystem spriteSys, SpriteManager spriteMan, int narcID, int memberIdx, int compressed, int resourceID)
    {
        return LoadResObjInternal(spriteSys, spriteMan, narcID, memberIdx, compressed, SpriteResource.TypeCell, resourceID);
    }

    // OFFSET 0x0200CE24
    public static bool SpriteSystem_LoadCellResObjFromOpenNarc(SpriteSystem spriteSys, SpriteManager spriteMan, NARC narc, int memberIdx, int compressed, int resourceID)
    {
        return LoadResObjFromNarcInternal(spriteSys, spriteMan, narc, memberIdx, compressed, SpriteResource.TypeCell, resourceID);
    }

    // OFFSET 0x0200CE3C
    public static bool SpriteSystem_LoadAnimResObj(SpriteSystem spriteSys, SpriteManager spriteMan, int narcID, int memberIdx, int compressed, int resourceID)
    {
        return LoadResObjInternal(spriteSys, spriteMan, narcID, memberIdx, compressed, SpriteResource.TypeAnim, resourceID);
    }

    // OFFSET 0x0200CE54
    public static bool SpriteSystem_LoadAnimResObjFromOpenNarc(SpriteSystem spriteSys, SpriteManager spriteMan, NARC narc, int memberIdx, int compressed, int resourceID)
    {
        return LoadResObjFromNarcInternal(spriteSys, spriteMan, narc, memberIdx, compressed, SpriteResource.TypeAnim, resourceID);
    }

    // OFFSET 0x0200CE6C
    public static ManagedSprite? SpriteSystem_NewSprite(SpriteSystem spriteSys, SpriteManager spriteMan, SpriteTemplate template)
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
        if (spriteMan.Collections[SpriteResource.TypeMultiCell] == null || spriteMan.Collections[SpriteResource.TypeMultiAnim] == null)
        {
            resourceIDs[SpriteResource.TypeMultiCell] = -1;
            resourceIDs[SpriteResource.TypeMultiAnim] = -1;
        }
        else
        {
            if (resourceIDs[SpriteResource.TypeMultiCell] != -1
                && !SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(spriteMan.Collections[SpriteResource.TypeMultiCell], resourceIDs[SpriteResource.TypeMultiCell]))
            {
                resourceIDs[SpriteResource.TypeMultiCell] = -1;
            }
            if (resourceIDs[SpriteResource.TypeMultiAnim] != -1
                && !SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(spriteMan.Collections[SpriteResource.TypeMultiAnim], resourceIDs[SpriteResource.TypeMultiAnim]))
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
            spriteMan.Collections[SpriteResource.TypeChar],
            spriteMan.Collections[SpriteResource.TypePltt],
            spriteMan.Collections[SpriteResource.TypeCell],
            spriteMan.Collections[SpriteResource.TypeAnim],
            spriteMan.Collections[SpriteResource.TypeMultiCell],
            spriteMan.Collections[SpriteResource.TypeMultiAnim]);
        AffineSpriteListTemplate listTemplate = new();
        listTemplate.List = spriteMan.SpriteList;
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
        listTemplate.HeapID = spriteSys.HeapID;
        managedSprite.Sprite = Sprite.SpriteList_AddAffine(listTemplate);
        managedSprite.VramTransfer = template.VramTransfer;
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

    // OFFSET 0x0200D04C
    public static ImagePaletteProxy? SpriteManager_FindPlttResourceProxy(SpriteManager spriteMan, int resourceID)
    {
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_Find(spriteMan.Collections[SpriteResource.TypePltt], resourceID);
        return SpriteTransfer.SpriteTransfer_GetPaletteProxy(resource, null);
    }

    // OFFSET 0x0200D05C
    public static int SpriteManager_FindPlttResourceOffset(SpriteManager spriteMan, int resourceID, int vramType)
    {
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_Find(spriteMan.Collections[SpriteResource.TypePltt], resourceID);
        return SpriteTransfer.SpriteTransfer_GetPlttOffset(resource, vramType);
    }

    // OFFSET 0x0200D070
    public static bool SpriteManager_UnloadCharObjById(SpriteManager spriteMan, int resourceID)
    {
        return UnregisterLoadedCharResource(spriteMan.Collections[SpriteResource.TypeChar], spriteMan.Lists[SpriteResource.TypeChar]!, resourceID);
    }

    // OFFSET 0x0200D080
    public static bool SpriteManager_UnloadPlttObjById(SpriteManager spriteMan, int resourceID)
    {
        return UnregisterLoadedPlttResource(spriteMan.Collections[SpriteResource.TypePltt], spriteMan.Lists[SpriteResource.TypePltt]!, resourceID);
    }

    // OFFSET 0x0200D090
    public static bool SpriteManager_UnloadCellObjById(SpriteManager spriteMan, int resourceID)
    {
        return UnregisterLoadedResource(spriteMan.Collections[SpriteResource.TypeCell], spriteMan.Lists[SpriteResource.TypeCell]!, resourceID);
    }

    // OFFSET 0x0200D0A0
    public static bool SpriteManager_UnloadAnimObjById(SpriteManager spriteMan, int resourceID)
    {
        return UnregisterLoadedResource(spriteMan.Collections[SpriteResource.TypeAnim], spriteMan.Lists[SpriteResource.TypeAnim]!, resourceID);
    }

    // OFFSET 0x0200D0B0
    public static void SpriteSystem_FreeResourcesAndManager(SpriteSystem spriteSys, SpriteManager spriteMan)
    {
        SpriteManager_DeleteAllSprites(spriteMan);
        SpriteTransfer.SpriteTransfer_ResetCharTransferList(spriteMan.Lists[SpriteResource.TypeChar]);
        SpriteTransfer.SpriteTransfer_ResetPlttTransferList(spriteMan.Lists[SpriteResource.TypePltt]);
        for (int i = 0; i < spriteMan.NumTypes; i++)
        {
            SpriteResourceList.SpriteResourceList_Delete(spriteMan.Lists[i]!);
            SpriteResourceCollection.SpriteResourceCollection_Delete(spriteMan.Collections[i]);
        }
        SpriteSystem_FreeSpriteManager(spriteSys, spriteMan);
    }

    // OFFSET 0x0200D0F4
    public static void Sprite_DeleteAndFreeResources(ManagedSprite managedSprite)
    {
        if (managedSprite.VramTransfer != 0)
        {
            SpriteTransfer.SpriteTransfer_DeleteCharTransfer(managedSprite.Header!.ImageProxy);
        }
        Sprite.Sprite_Delete(managedSprite.Sprite!);
        SpriteUtil.SpriteResourcesHeaderList_Free(managedSprite.HeaderList);
    }

    // OFFSET 0x0200D11C
    private static bool LoadResObjInternal(SpriteSystem spriteSys, SpriteManager spriteMan, int narcID, int memberIdx, int compressed, int type, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(spriteMan.Collections[type], resourceID))
        {
            return false;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_Add(spriteMan.Collections[type], narcID, memberIdx, compressed, resourceID, type, spriteSys.HeapID);
        if (resource != null)
        {
            bool result = RegisterLoadedResource(spriteMan.Lists[type]!, resource);
            if (!result)
            {
                ErrorHandling.GF_AssertFail();
            }
            return result;
        }
        ErrorHandling.GF_AssertFail();
        return resource != null;
    }

    // OFFSET 0x0200D18C
    private static bool LoadResObjFromNarcInternal(SpriteSystem spriteSys, SpriteManager spriteMan, NARC narc, int memberIdx, int compressed, int type, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(spriteMan.Collections[type], resourceID))
        {
            return false;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_AddFrom(spriteMan.Collections[type], narc, memberIdx, compressed, resourceID, type, spriteSys.HeapID);
        if (resource != null)
        {
            bool result = RegisterLoadedResource(spriteMan.Lists[type]!, resource);
            if (!result)
            {
                ErrorHandling.GF_AssertFail();
            }
            return result;
        }
        ErrorHandling.GF_AssertFail();
        return resource != null;
    }

    // OFFSET 0x0200D1FC
    private static bool RegisterLoadedResource(SpriteResourceList list, SpriteResource resource)
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

    // OFFSET 0x0200D230
    private static bool UnregisterLoadedResource(SpriteResourceCollection? collection, SpriteResourceList list, int resourceID)
    {
        for (int i = 0; i < list.Capacity; i++)
        {
            if (list.Resources[i] != null && SpriteResource.SpriteResource_GetID(list.Resources[i]) == resourceID)
            {
                SpriteResourceCollection.SpriteResourceCollection_Remove(collection, list.Resources[i]!);
                list.Resources[i] = null;
                list.Count--;
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0200D27C
    private static bool UnregisterLoadedCharResource(SpriteResourceCollection? collection, SpriteResourceList list, int resourceID)
    {
        for (int i = 0; i < list.Capacity; i++)
        {
            if (list.Resources[i] != null && SpriteResource.SpriteResource_GetID(list.Resources[i]) == resourceID)
            {
                CharTransfer.CharTransfer_ResetTask(resourceID);
                SpriteResourceCollection.SpriteResourceCollection_Remove(collection, list.Resources[i]!);
                list.Resources[i] = null;
                list.Count--;
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0200D2D0
    private static bool UnregisterLoadedPlttResource(SpriteResourceCollection? collection, SpriteResourceList list, int resourceID)
    {
        for (int i = 0; i < list.Capacity; i++)
        {
            if (list.Resources[i] != null && SpriteResource.SpriteResource_GetID(list.Resources[i]) == resourceID)
            {
                PlttTransfer.PlttTransfer_ResetTask(resourceID);
                SpriteResourceCollection.SpriteResourceCollection_Remove(collection, list.Resources[i]!);
                list.Resources[i] = null;
                list.Count--;
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0200D324
    public static void Sprite_TickFrame(Sprite sprite)
    {
        Sprite.Sprite_UpdateAnim(sprite, FxOne);
    }

    // OFFSET 0x0200D330
    public static void ManagedSprite_TickFrame(ManagedSprite managedSprite)
    {
        Sprite_TickFrame(managedSprite.Sprite!);
    }

    // OFFSET 0x0200D33C
    public static void ManagedSprite_TickTwoFrames(ManagedSprite managedSprite)
    {
        Sprite.Sprite_UpdateAnim(managedSprite.Sprite!, FxOne * 2);
    }

    // OFFSET 0x0200D34C
    public static void ManagedSprite_TickNFrames(ManagedSprite managedSprite, int frames)
    {
        Sprite.Sprite_UpdateAnim(managedSprite.Sprite!, frames);
    }

    // OFFSET 0x0200D358
    public static uint ManagedSprite_GetNumFrames(ManagedSprite managedSprite)
    {
        return Sprite.Sprite_GetAnimCount(managedSprite.Sprite!);
    }

    // OFFSET 0x0200D364
    public static void ManagedSprite_SetAnim(ManagedSprite managedSprite, uint animID)
    {
        Sprite.Sprite_SetAnim(managedSprite.Sprite!, animID);
    }

    // OFFSET 0x0200D370
    public static void ManagedSprite_SetAnimNoRestart(ManagedSprite managedSprite, uint animID)
    {
        Sprite.Sprite_SetAnimNoRestart(managedSprite.Sprite!, animID);
    }

    // OFFSET 0x0200D37C
    public static uint ManagedSprite_GetActiveAnim(ManagedSprite managedSprite)
    {
        return Sprite.Sprite_GetActiveAnim(managedSprite.Sprite!);
    }

    // OFFSET 0x0200D388
    private static void SetSpriteAnimateFlag(Sprite sprite, int animate)
    {
        Sprite.Sprite_SetAnimateFlag(sprite, animate);
    }

    // OFFSET 0x0200D390
    public static void ManagedSprite_SetAnimateFlag(ManagedSprite managedSprite, int animate)
    {
        SetSpriteAnimateFlag(managedSprite.Sprite!, animate);
    }

    // OFFSET 0x0200D39C
    private static void SetSpriteAnimationSpeed(Sprite sprite, int speed)
    {
        Sprite.Sprite_SetAnimSpeed(sprite, speed);
    }

    // OFFSET 0x0200D3A4
    public static void ManagedSprite_SetAnimationSpeed(ManagedSprite managedSprite, int speed)
    {
        SetSpriteAnimationSpeed(managedSprite.Sprite!, speed);
    }

    // OFFSET 0x0200D3B0
    private static bool IsSpriteAnimated(Sprite sprite)
    {
        return Sprite.Sprite_IsAnimated(sprite);
    }

    // OFFSET 0x0200D3B8
    public static bool ManagedSprite_IsAnimated(ManagedSprite managedSprite)
    {
        return IsSpriteAnimated(managedSprite.Sprite!);
    }

    // OFFSET 0x0200D3C4
    public static void SetSpriteAnimationFrame(Sprite sprite, ushort frame)
    {
        Sprite.Sprite_SetAnimFrame(sprite, frame);
    }

    // OFFSET 0x0200D3CC
    public static void ManagedSprite_SetAnimationFrame(ManagedSprite managedSprite, ushort frame)
    {
        SetSpriteAnimationFrame(managedSprite.Sprite!, frame);
    }

    // OFFSET 0x0200D3D8
    private static ushort GetSpriteAnimationFrame(Sprite sprite)
    {
        return Sprite.Sprite_GetAnimFrame(sprite);
    }

    // OFFSET 0x0200D3E0
    public static ushort ManagedSprite_GetAnimationFrame(ManagedSprite managedSprite)
    {
        return GetSpriteAnimationFrame(managedSprite.Sprite!);
    }

    // OFFSET 0x0200D3EC
    public static void Sprite_SetDrawFlag2(Sprite sprite, int draw)
    {
        Sprite.Sprite_SetDrawFlag(sprite, draw);
    }

    // OFFSET 0x0200D3F4
    public static void ManagedSprite_SetDrawFlag(ManagedSprite managedSprite, int draw)
    {
        Sprite_SetDrawFlag2(managedSprite.Sprite!, draw);
    }

    // OFFSET 0x0200D400
    public static int Sprite_GetDrawFlag2(Sprite sprite)
    {
        return Sprite.Sprite_GetDrawFlag(sprite);
    }

    // OFFSET 0x0200D408
    public static int ManagedSprite_GetDrawFlag(ManagedSprite managedSprite)
    {
        return Sprite_GetDrawFlag2(managedSprite.Sprite!);
    }

    // OFFSET 0x0200D414
    public static void Sprite_SetExplicitPalette2(Sprite sprite, uint palette)
    {
        Sprite.Sprite_SetExplicitPalette(sprite, palette);
    }

    // OFFSET 0x0200D41C
    public static void ManagedSprite_SetExplicitPalette(ManagedSprite managedSprite, uint palette)
    {
        Sprite_SetExplicitPalette2(managedSprite.Sprite!, palette);
    }

    // OFFSET 0x0200D428
    private static void SetSpriteExplicitPaletteOffset(Sprite sprite, uint offset)
    {
        Sprite.Sprite_SetExplicitPaletteOffset(sprite, offset);
    }

    // OFFSET 0x0200D430
    public static void ManagedSprite_SetExplicitPaletteOffset(ManagedSprite managedSprite, uint offset)
    {
        SetSpriteExplicitPaletteOffset(managedSprite.Sprite!, offset);
    }

    // OFFSET 0x0200D43C
    public static uint ManagedSprite_GetExplicitPaletteOffset(ManagedSprite managedSprite)
    {
        return Sprite.Sprite_GetExplicitPaletteOffset(managedSprite.Sprite!);
    }

    // OFFSET 0x0200D448
    private static void SetSpriteExplicitPriority(Sprite sprite, int priority)
    {
        Sprite.Sprite_SetExplicitPriority(sprite, (byte)priority);
    }

    // OFFSET 0x0200D454
    public static int ManagedSprite_GetExplicitPriority(ManagedSprite managedSprite)
    {
        return Sprite.Sprite_GetExplicitPriority(managedSprite.Sprite!);
    }

    // OFFSET 0x0200D460
    public static void ManagedSprite_SetExplicitPriority(ManagedSprite managedSprite, int priority)
    {
        SetSpriteExplicitPriority(managedSprite.Sprite!, priority);
    }

    // OFFSET 0x0200D46C
    private static void SetSpritePriority(Sprite sprite, uint priority)
    {
        Sprite.Sprite_SetPriority(sprite, priority);
    }

    // OFFSET 0x0200D474
    public static void ManagedSprite_SetPriority(ManagedSprite managedSprite, uint priority)
    {
        SetSpritePriority(managedSprite.Sprite!, priority);
    }

    // OFFSET 0x0200D480
    public static uint GetSpritePriority(Sprite sprite)
    {
        return Sprite.Sprite_GetPriority(sprite);
    }

    // OFFSET 0x0200D488
    public static uint ManagedSprite_GetPriority(ManagedSprite managedSprite)
    {
        return GetSpritePriority(managedSprite.Sprite!);
    }

    // OFFSET 0x0200D494
    public static void Sprite_SetPositionXY(Sprite sprite, short x, short y)
    {
        VecFx32 position = new() { X = x * FxOne, Y = y * FxOne };
        if (Sprite.Sprite_GetVRamType(sprite) == VramSub)
        {
            position.Y += SubScreenOffset;
        }
        position.Z = 0;
        Sprite.Sprite_SetPosition(sprite, position);
    }

    // OFFSET 0x0200D4C4
    public static void ManagedSprite_SetPositionXY(ManagedSprite managedSprite, short x, short y)
    {
        Sprite_SetPositionXY(managedSprite.Sprite!, x, y);
    }

    // OFFSET 0x0200D4D0
    public static void Sprite_SetPositionXYWithSubscreenOffset(Sprite sprite, short x, short y, int offset)
    {
        VecFx32 position = new() { X = x * FxOne, Y = y * FxOne };
        if (Sprite.Sprite_GetVRamType(sprite) == VramSub)
        {
            position.Y += offset;
        }
        position.Z = 0;
        Sprite.Sprite_SetPosition(sprite, position);
    }

    // OFFSET 0x0200D500
    public static void ManagedSprite_SetPositionXYWithSubscreenOffset(ManagedSprite managedSprite, short x, short y, int offset)
    {
        Sprite_SetPositionXYWithSubscreenOffset(managedSprite.Sprite!, x, y, offset);
    }

    // OFFSET 0x0200D50C
    public static void Sprite_GetPositionXY(Sprite sprite, out short x, out short y)
    {
        VecFx32 position = Sprite.Sprite_GetPosition(sprite);
        x = (short)(position.X / FxOne);
        if (Sprite.Sprite_GetVRamType(sprite) == VramSub)
        {
            y = (short)((position.Y - SubScreenOffset) / FxOne);
        }
        else
        {
            y = (short)(position.Y / FxOne);
        }
    }

    // OFFSET 0x0200D550
    public static void ManagedSprite_GetPositionXY(ManagedSprite managedSprite, out short x, out short y)
    {
        Sprite_GetPositionXY(managedSprite.Sprite!, out x, out y);
    }

    // OFFSET 0x0200D55C
    public static void ManagedSprite_GetPositionXYWithSubscreenOffset(Sprite sprite, out short x, out short y, int offset)
    {
        VecFx32 position = Sprite.Sprite_GetPosition(sprite);
        x = (short)(position.X / FxOne);
        if (Sprite.Sprite_GetVRamType(sprite) == VramSub)
        {
            y = (short)((position.Y - offset) / FxOne);
        }
        else
        {
            y = (short)(position.Y / FxOne);
        }
    }

    // OFFSET 0x0200D5A0
    public static void Sprite_GetPositionXYWithSubscreenOffset2(ManagedSprite managedSprite, out short x, out short y, int offset)
    {
        ManagedSprite_GetPositionXYWithSubscreenOffset(managedSprite.Sprite!, out x, out y, offset);
    }

    // OFFSET 0x0200D5AC
    public static void Sprite_OffsetPositionXY(Sprite sprite, short x, short y)
    {
        VecFx32 current = Sprite.Sprite_GetPosition(sprite);
        VecFx32 position = new() { X = current.X + x * FxOne, Y = current.Y + y * FxOne, Z = current.Z };
        Sprite.Sprite_SetPosition(sprite, position);
    }

    // OFFSET 0x0200D5DC
    public static void ManagedSprite_OffsetPositionXY(ManagedSprite managedSprite, short x, short y)
    {
        Sprite_OffsetPositionXY(managedSprite.Sprite!, x, y);
    }

    // OFFSET 0x0200D5E8
    public static void ManagedSprite_OffsetPositionFxXY(ManagedSprite managedSprite, int x, int y)
    {
        VecFx32 current = Sprite.Sprite_GetPosition(managedSprite.Sprite!);
        VecFx32 position = new() { X = current.X + x, Y = current.Y + y, Z = current.Z };
        Sprite.Sprite_SetPosition(managedSprite.Sprite!, position);
    }

    // OFFSET 0x0200D614
    public static void ManagedSprite_SetPositionFxXY(ManagedSprite managedSprite, int x, int y)
    {
        VecFx32 current = Sprite.Sprite_GetPosition(managedSprite.Sprite!);
        VecFx32 position = new() { X = x, Y = y, Z = current.Z };
        Sprite.Sprite_SetPosition(managedSprite.Sprite!, position);
    }

    // OFFSET 0x0200D638
    public static void ManagedSprite_GetPositionFxXY(ManagedSprite managedSprite, out int x, out int y)
    {
        VecFx32 position = Sprite.Sprite_GetPosition(managedSprite.Sprite!);
        x = position.X;
        y = position.Y;
    }

    // OFFSET 0x0200D650
    public static void ManagedSprite_SetPositionFxXYWithSubscreenOffset(ManagedSprite managedSprite, int x, int y, int offset)
    {
        if (Sprite.Sprite_GetVRamType(managedSprite.Sprite!) == VramSub)
        {
            ManagedSprite_SetPositionFxXY(managedSprite, x, y + offset);
        }
        else
        {
            ManagedSprite_SetPositionFxXY(managedSprite, x, y);
        }
    }

    // OFFSET 0x0200D67C
    public static void ManagedSprite_GetPositionFxXYWithSubscreenOffset(ManagedSprite managedSprite, out int x, out int y, int offset)
    {
        ManagedSprite_GetPositionFxXY(managedSprite, out x, out y);
        if (Sprite.Sprite_GetVRamType(managedSprite.Sprite!) == VramSub)
        {
            y -= offset;
        }
    }

    // OFFSET 0x0200D69C
    private static void SetSpriteAffineOverwriteMode(Sprite sprite, int mode)
    {
        Sprite.Sprite_SetAffineOverwriteMode(sprite, mode);
    }

    // OFFSET 0x0200D6A4
    public static void ManagedSprite_SetAffineOverwriteMode(ManagedSprite managedSprite, int mode)
    {
        SetSpriteAffineOverwriteMode(managedSprite.Sprite!, mode);
    }

    // OFFSET 0x0200D6B0
    private static void SetSpriteAffineScale(Sprite sprite, float x, float y)
    {
        VecFx32 scale = Sprite.Sprite_GetAffineScale(sprite);
        scale.X = (int)(FxOne * x);
        scale.Y = (int)(FxOne * y);
        Sprite.Sprite_SetAffineScale(sprite, scale);
    }

    // OFFSET 0x0200D6E8
    public static void ManagedSprite_SetAffineScale(ManagedSprite managedSprite, float x, float y)
    {
        SetSpriteAffineScale(managedSprite.Sprite!, x, y);
    }

    // OFFSET 0x0200D6F4
    private static void OffsetSpriteAffineScale(Sprite sprite, float x, float y)
    {
        VecFx32 scale = Sprite.Sprite_GetAffineScale(sprite);
        scale.X = (int)(scale.X + FxOne * x);
        scale.Y = (int)(scale.Y + FxOne * y);
        Sprite.Sprite_SetAffineScale(sprite, scale);
    }

    // OFFSET 0x0200D74C
    public static void ManagedSprite_OffsetAffineScale(ManagedSprite managedSprite, float x, float y)
    {
        OffsetSpriteAffineScale(managedSprite.Sprite!, x, y);
    }

    // OFFSET 0x0200D758
    public static void GetSpriteAffineScale(Sprite sprite, out float x, out float y)
    {
        VecFx32 scale = Sprite.Sprite_GetAffineScale(sprite);
        x = (float)scale.X / FxOne;
        y = (float)scale.Y / FxOne;
    }

    // OFFSET 0x0200D788
    public static void ManagedSprite_GetAffineScale(ManagedSprite managedSprite, out float x, out float y)
    {
        GetSpriteAffineScale(managedSprite.Sprite!, out x, out y);
    }

    // OFFSET 0x0200D794
    private static void SetSpriteAffineZRotation(Sprite sprite, ushort angle)
    {
        Sprite.Sprite_SetAffineZRotation(sprite, angle);
    }

    // OFFSET 0x0200D79C
    public static void ManagedSprite_SetAffineZRotation(ManagedSprite managedSprite, ushort angle)
    {
        SetSpriteAffineZRotation(managedSprite.Sprite!, angle);
    }

    // OFFSET 0x0200D7A8
    public static void OffsetSpriteAffineZRotation(Sprite sprite, int offset)
    {
        ushort angle = Sprite.Sprite_GetAffineZRotation(sprite);
        Sprite.Sprite_SetAffineZRotation(sprite, (ushort)(angle + offset));
    }

    // OFFSET 0x0200D7C0
    public static void ManagedSprite_OffsetAffineZRotation(ManagedSprite managedSprite, int offset)
    {
        OffsetSpriteAffineZRotation(managedSprite.Sprite!, offset);
    }

    // OFFSET 0x0200D7CC
    public static void Sprite_SetFlipMode2(Sprite sprite, int mode)
    {
        Sprite.Sprite_SetFlipMode(sprite, mode);
    }

    // OFFSET 0x0200D7D4
    public static void ManagedSprite_SetFlipMode(ManagedSprite managedSprite, int mode)
    {
        Sprite_SetFlipMode2(managedSprite.Sprite!, mode);
    }

    // OFFSET 0x0200D7E0
    public static void ManagedSprite_SetAffineTranslation(ManagedSprite managedSprite, short x, short y)
    {
        VecFx32 translation = new() { X = x * FxOne, Y = y * FxOne, Z = 0 };
        Sprite.Sprite_SetAffineTranslation(managedSprite.Sprite!, translation);
    }

    // OFFSET 0x0200D7FC
    public static void ManagedSprite_SetMosaicFlag(ManagedSprite managedSprite, int mosaic)
    {
        Sprite.Sprite_SetMosaicFlag(managedSprite.Sprite!, mosaic);
    }

    // OFFSET 0x0200D808
    public static void Sprite_SetExplicitOamMode2(Sprite sprite, int mode)
    {
        Sprite.Sprite_SetExplicitOAMMode(sprite, mode);
    }

    // OFFSET 0x0200D810
    public static void ManagedSprite_SetExplicitOamMode(ManagedSprite managedSprite, int mode)
    {
        Sprite_SetExplicitOamMode2(managedSprite.Sprite!, mode);
    }

    // OFFSET 0x0200D81C
    public static uint ManagedSprite_GetUserAttrForCurrentAnimFrame(ManagedSprite managedSprite)
    {
        return Sprite.Sprite_GetUserAttrForCurrentAnimFrame(managedSprite.Sprite!);
    }

    // OFFSET 0x0200D828
    public static bool SpriteSystem_LoadCharResObjWithHardwareMappingType(SpriteSystem spriteSys, SpriteManager spriteMan, int narcID, int memberIdx, int compressed, int vramType, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(spriteMan.Collections[SpriteResource.TypeChar], resourceID))
        {
            return false;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_AddTiles(spriteMan.Collections[SpriteResource.TypeChar], narcID, memberIdx, compressed, resourceID, vramType, spriteSys.HeapID);
        if (resource != null)
        {
            SpriteTransfer.SpriteTransfer_RequestCharWithHardwareMappingType(resource);
            RegisterLoadedResource(spriteMan.Lists[SpriteResource.TypeChar]!, resource);
            return true;
        }
        ErrorHandling.GF_AssertFail();
        return resource != null;
    }

    // OFFSET 0x0200D888
    public static bool SpriteSystem_LoadCharResObjAtEndWithHardwareMappingType(SpriteSystem spriteSys, SpriteManager spriteMan, int narcID, int memberIdx, int compressed, int vramType, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(spriteMan.Collections[SpriteResource.TypeChar], resourceID))
        {
            return false;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_AddTiles(spriteMan.Collections[SpriteResource.TypeChar], narcID, memberIdx, compressed, resourceID, vramType, spriteSys.HeapID);
        if (resource != null)
        {
            SpriteTransfer.SpriteTransfer_RequestCharAtEndWithHardwareMappingType(resource);
            RegisterLoadedResource(spriteMan.Lists[SpriteResource.TypeChar]!, resource);
            return true;
        }
        ErrorHandling.GF_AssertFail();
        return resource != null;
    }

    // OFFSET 0x0200D8E8
    public static bool SpriteSystem_LoadCharResObjFromOpenNarcWithHardwareMappingType(SpriteSystem spriteSys, SpriteManager spriteMan, NARC narc, int memberIdx, int compressed, int vramType, int resourceID)
    {
        if (!SpriteResourceCollection.SpriteResourceCollection_IsIDUnused(spriteMan.Collections[SpriteResource.TypeChar], resourceID))
        {
            return false;
        }
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_AddTilesFrom(spriteMan.Collections[SpriteResource.TypeChar], narc, memberIdx, compressed, resourceID, vramType, spriteSys.HeapID);
        if (resource != null)
        {
            SpriteTransfer.SpriteTransfer_RequestCharAtEndWithHardwareMappingType(resource);
            RegisterLoadedResource(spriteMan.Lists[SpriteResource.TypeChar]!, resource);
            return true;
        }
        ErrorHandling.GF_AssertFail();
        return resource != null;
    }

    // OFFSET 0x0200D948
    public static void SpriteSystem_ReplaceCharResObj(SpriteSystem spriteSys, SpriteManager spriteMan, int narcID, int memberIdx, int compressed, int resourceID)
    {
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_Find(spriteMan.Collections[SpriteResource.TypeChar], resourceID);
        SpriteResourceCollection.SpriteResourceCollection_ModifyTiles(spriteMan.Collections[SpriteResource.TypeChar], resource, narcID, memberIdx, compressed, spriteSys.HeapID);
        SpriteTransfer.SpriteTransfer_RetransferCharData(resource);
    }

    // OFFSET 0x0200D97C
    public static void SpriteSystem_ReplacePlttResObj(SpriteSystem spriteSys, SpriteManager spriteMan, int narcID, int memberIdx, int compressed, int resourceID)
    {
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_Find(spriteMan.Collections[SpriteResource.TypePltt], resourceID);
        SpriteResourceCollection.SpriteResourceCollection_ModifyPalette(spriteMan.Collections[SpriteResource.TypePltt], resource, narcID, memberIdx, compressed, spriteSys.HeapID);
        SpriteTransfer.SpriteTransfer_ReplacePlttData(resource);
    }

    // OFFSET 0x0200D9B0
    public static SpriteList? SpriteManager_GetSpriteList(SpriteManager spriteMan)
    {
        return spriteMan.SpriteList;
    }

    // OFFSET 0x0200D9B4
    public static void SpriteSystem_ReplaceCharResObjFromOpenNarc(SpriteSystem spriteSys, SpriteManager spriteMan, NARC narc, int memberIdx, int compressed, int resourceID)
    {
        SpriteResource? resource = SpriteResourceCollection.SpriteResourceCollection_Find(spriteMan.Collections[SpriteResource.TypeChar], resourceID);
        SpriteResourceCollection.SpriteResourceCollection_ModifyTilesFrom(spriteMan.Collections[SpriteResource.TypeChar], resource, narc, memberIdx, compressed, spriteSys.HeapID);
        SpriteTransfer.SpriteTransfer_RetransferCharData(resource);
    }
}
