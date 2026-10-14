using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class AreaDataManagerLoadData
{
    public int AreaDataArchiveID;
    public MapPropAnimationManager? MapPropAnimationManager;
    public ushort MapPropModelIDsCount;
    public int Unused;
}

public sealed class AreaDataFile
{
    public const int Size = 8;

    public ushort MapPropArchivesID;
    public ushort MapTextureArchiveID;
    public ushort Unused;
    public ushort AreaLightArchiveID;
}

public sealed class AreaDataManager
{
    public const int MaxMapPropModelFiles = 768;

    public readonly byte[]?[] MapPropModelFiles = new byte[]?[MaxMapPropModelFiles];
    public byte[]? MapTextureFile;
    public byte[]? MapPropTextureFile;
    public G3dTexture? MapTexture;
    public G3dTexture? MapPropTexture;
    public MapPropMaterialShape? MapPropMaterialShape;
    public readonly AreaDataFile AreaData = new();
    public AreaDataManagerLoadData? LoadData;
    public byte[]? MapPropModelIDs;
}

public static class AreaData
{
    private const int Overlay = 5;
    private const int HeapIDField = 4;
    private const int AreaDataArchive = 0x2A;
    private const int AreaBuildModelIDsArchive = 0x2B;
    private const int MapTextureSetArchive = 0x2C;
    private const int MapPropTextureSetArchive = 0x47;
    private const int MapPropModelArchive = 0x28;
    private const int TextureDataOffset = 0x14;
    private const ushort Disp3DMask = 0xCFFF;
    private const ushort Disp3DEdgeMarking = 0x20;
    private const uint IndoorsEdgeColorTable = 0x021FF588;
    private const uint OutdoorsEdgeColorTable = 0x021FF598;
    private const int EdgeColorCount = 8;
    private const int AreaLightOutdoors = 0;
    private const int AreaLightOutdoorsAlternate = 3;
    private const string MaterialShapePath = "fielddata/build_model/build_model_matshp.dat";

    private static G3dModel? GetFirstModel(byte[] resource)
    {
        G3dModelSet? modelSet = G3d.GetModelSet(resource);
        if (modelSet == null)
        {
            return null;
        }
        return G3d.GetModelByIndex(modelSet, 0);
    }

    // OFFSET 0x021EF75C
    private static void AreaDataManager_StripTextureData(byte[] textureFile, G3dTexture texture)
    {
        Heap.Heap_Realloc(ref textureFile, (uint)texture.Offset + Bin.U32(texture.File, texture.Offset + TextureDataOffset));
    }

    // OFFSET 0x021EF76C
    public static AreaDataManager AreaDataManager_Alloc(int areaDataArchiveID, MapPropAnimationManager? mapPropAnimationManager)
    {
        AreaDataManager areaDataManager = new();
        areaDataManager.LoadData = new AreaDataManagerLoadData();
        areaDataManager.LoadData.AreaDataArchiveID = areaDataArchiveID;
        areaDataManager.LoadData.MapPropAnimationManager = mapPropAnimationManager;
        areaDataManager.LoadData.Unused = 0;
        return areaDataManager;
    }

    // OFFSET 0x021EF7A0
    public static void AreaDataManager_Load(AreaDataManager areaDataManager)
    {
        AreaDataManagerLoadData loadData = areaDataManager.LoadData!;
        byte[] areaDataFile = new byte[AreaDataFile.Size];
        NARC.NARC_ReadWholeMemberByIndexPair(areaDataFile, AreaDataArchive, loadData.AreaDataArchiveID);
        areaDataManager.AreaData.MapPropArchivesID = (ushort)Bin.U16(areaDataFile, 0);
        areaDataManager.AreaData.MapTextureArchiveID = (ushort)Bin.U16(areaDataFile, 2);
        areaDataManager.AreaData.Unused = (ushort)Bin.U16(areaDataFile, 4);
        areaDataManager.AreaData.AreaLightArchiveID = (ushort)Bin.U16(areaDataFile, 6);
        areaDataManager.MapPropModelIDs = NARC.NARC_AllocAndReadWholeMemberByIndexPair(AreaBuildModelIDsArchive, areaDataManager.AreaData.MapPropArchivesID, HeapIDField);
        loadData.MapPropModelIDsCount = (ushort)Bin.U16(areaDataManager.MapPropModelIDs, 0);
        if (loadData.MapPropModelIDsCount >= AreaDataManager.MaxMapPropModelFiles)
        {
            ErrorHandling.GF_AssertFail();
        }
        areaDataManager.MapTexture = null;
        areaDataManager.MapTextureFile = NARC.NARC_AllocAndReadWholeMemberByIndexPair(MapTextureSetArchive, areaDataManager.AreaData.MapTextureArchiveID, HeapIDField);
        areaDataManager.MapPropTextureFile = NARC.NARC_AllocAndReadWholeMemberByIndexPair(MapPropTextureSetArchive, areaDataManager.AreaData.MapPropArchivesID, HeapIDField);
        areaDataManager.MapTexture = G3d.GetTexture(areaDataManager.MapTextureFile);
        if (loadData.MapPropModelIDsCount != 0)
        {
            areaDataManager.MapPropTexture = G3d.GetTexture(areaDataManager.MapPropTextureFile);
        }
        else
        {
            areaDataManager.MapPropTexture = null;
        }
        if (!Easy3D.Easy3D_UploadTextureToVRAM(areaDataManager.MapTexture))
        {
            ErrorHandling.GF_AssertFail();
        }
        AreaDataManager_StripTextureData(areaDataManager.MapTextureFile, areaDataManager.MapTexture!);
        if (areaDataManager.MapPropTexture != null)
        {
            if (!Easy3D.Easy3D_UploadTextureToVRAM(areaDataManager.MapPropTexture))
            {
                ErrorHandling.GF_AssertFail();
            }
            AreaDataManager_StripTextureData(areaDataManager.MapPropTextureFile, areaDataManager.MapPropTexture);
        }
        for (int i = 0; i < AreaDataManager.MaxMapPropModelFiles; i++)
        {
            areaDataManager.MapPropModelFiles[i] = null;
        }
        areaDataManager.MapPropMaterialShape = null;
        NARC mapPropModelNARC = NARC.NARC_ctor(MapPropModelArchive, HeapIDField);
        int animeListFileCount = MapPropAnimations.MapPropAnimationManager_GetAnimeListNARCFileCount(loadData.MapPropAnimationManager!);
        for (int i = 0; i < loadData.MapPropModelIDsCount; i++)
        {
            int mapPropModelID = Bin.U16(areaDataManager.MapPropModelIDs, (i + 1) * 2);
            if (areaDataManager.MapPropModelFiles[mapPropModelID] != null)
            {
                ErrorHandling.GF_AssertFail();
            }
            byte[] modelFile = NARC.NARC_AllocAndReadWholeMember(mapPropModelNARC, (uint)mapPropModelID, HeapIDField);
            areaDataManager.MapPropModelFiles[mapPropModelID] = modelFile;
            if (mapPropModelID < animeListFileCount)
            {
                G3dModel? model = GetFirstModel(modelFile);
                MapPropAnimations.MapPropAnimationManager_LoadPropAnimations(mapPropModelID, model, areaDataManager.MapPropTexture, loadData.MapPropAnimationManager!);
            }
            if (!Easy3D.Easy3D_BindTextureToResource(modelFile, areaDataManager.MapPropTexture))
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        if (areaDataManager.MapPropModelFiles[0] == null)
        {
            byte[] modelFile = NARC.NARC_AllocAndReadWholeMember(mapPropModelNARC, 0, HeapIDField);
            areaDataManager.MapPropModelFiles[0] = modelFile;
            G3dTexture? texture = G3d.GetTexture(modelFile);
            if (texture != null)
            {
                if (!Easy3D.Easy3D_UploadTextureToVRAM(texture))
                {
                    ErrorHandling.GF_AssertFail();
                }
                if (!Easy3D.Easy3D_BindTextureToResource(modelFile, texture))
                {
                    ErrorHandling.GF_AssertFail();
                }
            }
        }
        NARC.NARC_dtor(mapPropModelNARC);
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DEdgeMarking);
        if (!AreaDataManager_IsOutdoorsLighting(areaDataManager))
        {
            Render3D.SetEdgeColorTable(RomData.OverlayU16(Overlay, IndoorsEdgeColorTable, EdgeColorCount));
        }
        else
        {
            Render3D.SetEdgeColorTable(RomData.OverlayU16(Overlay, OutdoorsEdgeColorTable, EdgeColorCount));
        }
        areaDataManager.MapPropMaterialShape = MapPropMaterialShape.MapPropMaterialShape_Alloc();
        MapPropMaterialShape.MapPropMaterialShape_Load(MaterialShapePath, areaDataManager.MapPropMaterialShape);
        areaDataManager.LoadData = null;
    }

    // OFFSET 0x021EF9E8
    public static byte[]? AreaDataManager_GetMapPropModelFile(int mapPropModelID, AreaDataManager? areaDataManager)
    {
        if (areaDataManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (areaDataManager!.MapPropModelFiles[mapPropModelID] == null)
        {
            if (areaDataManager.MapPropModelFiles[0] == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            return areaDataManager.MapPropModelFiles[0];
        }
        return areaDataManager.MapPropModelFiles[mapPropModelID];
    }

    // OFFSET 0x021EFA10
    public static void AreaDataManager_Free(ref AreaDataManager? areaDataManager)
    {
        AreaDataManager manager = areaDataManager!;
        if (manager.MapPropMaterialShape != null)
        {
            MapPropMaterialShape.MapPropMaterialShape_Free(manager.MapPropMaterialShape);
        }
        for (int i = 0; i < AreaDataManager.MaxMapPropModelFiles; i++)
        {
            if (manager.MapPropModelFiles[i] != null)
            {
                Heap.Heap_Free(manager.MapPropModelFiles[i]!);
            }
        }
        Heap.Heap_Free(manager.MapPropModelIDs!);
        Heap.Heap_Free(manager.MapTextureFile!);
        manager.MapTextureFile = null;
        Heap.Heap_Free(manager.MapPropTextureFile!);
        manager.MapPropTextureFile = null;
        areaDataManager = null;
    }

    // OFFSET 0x021EFA8C
    public static G3dTexture? AreaDataManager_GetMapTexture(AreaDataManager? areaDataManager)
    {
        if (areaDataManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return areaDataManager!.MapTexture;
    }

    // OFFSET 0x021EFAA0
    public static G3dTexture? AreaDataManager_GetMapPropTexture(AreaDataManager? areaDataManager)
    {
        if (areaDataManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return areaDataManager!.MapPropTexture;
    }

    // OFFSET 0x021EFAB4
    public static MapPropMaterialShape? AreaDataManager_GetMapPropMaterialShape(AreaDataManager areaDataManager)
    {
        return areaDataManager.MapPropMaterialShape;
    }

    // OFFSET 0x021EFAC0
    public static bool AreaDataManager_IsOutdoorsLighting(AreaDataManager areaDataManager)
    {
        int areaLightArchiveID = areaDataManager.AreaData.AreaLightArchiveID;
        if (areaLightArchiveID == AreaLightOutdoors || areaLightArchiveID == AreaLightOutdoorsAlternate)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021EFAD8
    public static byte AreaDataManager_GetAreaLightArchiveID(AreaDataManager areaDataManager)
    {
        return (byte)areaDataManager.AreaData.AreaLightArchiveID;
    }

    // OFFSET 0x021EFAE8
    public static int AreaDataManager_GetMapPropModelID(AreaDataManager areaDataManager, int index)
    {
        return Bin.U16(areaDataManager.MapPropModelIDs!, index * 2);
    }

    // OFFSET 0x021EFAF8
    public static bool AreaDataManager_HasMapPropModelFile(AreaDataManager areaDataManager, int mapPropModelID)
    {
        if (areaDataManager.MapPropModelFiles[mapPropModelID] != null)
        {
            return true;
        }
        return false;
    }
}
