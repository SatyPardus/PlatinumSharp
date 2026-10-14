using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class MapProp
{
    public int ModelID;
    public bool Loaded;
    public bool Hidden;
    public bool ApplyRotation;
    public readonly G3dRenderObj RenderObj = new();
    public G3dModel? Model;
    public VecFx32 Position;
    public VecFx32 Rotation;
    public VecFx32 Scale;
}

public sealed class MapPropManager
{
    public const int MaxLoadedMapProps = 32;

    public readonly MapProp[] LoadedProps = new MapProp[MaxLoadedMapProps];

    public MapPropManager()
    {
        for (int i = 0; i < MaxLoadedMapProps; i++)
        {
            LoadedProps[i] = new MapProp();
        }
    }
}

public static class MapProps
{
    private const int HeapIDField = 4;
    private const int MapPropFileSize = 0x30;
    private const int MapPropFileModelID = 0x00;
    private const int MapPropFilePosition = 0x04;
    private const int MapPropFileRotation = 0x10;
    private const int MapPropFileScale = 0x1C;
    private const int MapPropModelAttributesMask = 0xF33;
    private const int NoMaterial = 0xFF;

    private static MtxFx33 IdentityRotation()
    {
        return new MtxFx33 { M00 = Fx32.One, M11 = Fx32.One, M22 = Fx32.One };
    }

    private static VecFx32 ReadVector(byte[] data, int offset)
    {
        return new VecFx32 { X = Bin.I32(data, offset), Y = Bin.I32(data, offset + 4), Z = Bin.I32(data, offset + 8) };
    }

    // OFFSET 0x021E15F4
    public static MapPropManager MapPropManager_New(int heapID)
    {
        MapPropManager mapPropManager = new();
        MapPropManager_Init(mapPropManager);
        return mapPropManager;
    }

    // OFFSET 0x021E1608
    public static void MapPropManager_Free(MapPropManager mapPropManager)
    {
    }

    // OFFSET 0x021E1610
    public static void MapPropManager_Init(MapPropManager mapPropManager)
    {
        for (byte i = 0; i < MapPropManager.MaxLoadedMapProps; i++)
        {
            MapProp mapProp = mapPropManager.LoadedProps[i];
            mapProp.ModelID = 0;
            mapProp.Loaded = false;
            mapProp.Hidden = false;
            mapProp.ApplyRotation = false;
            mapProp.Position = default;
            mapProp.Rotation = default;
            mapProp.Scale = default;
            mapProp.Model = null;
        }
    }

    // OFFSET 0x021E1674
    public static void MapPropManager_ClearOne(int index, MapPropManager mapPropManager)
    {
        if (index >= MapPropManager.MaxLoadedMapProps)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapProp mapProp = mapPropManager.LoadedProps[index];
        mapProp.ModelID = 0;
        mapProp.Loaded = false;
        mapProp.Hidden = false;
        mapProp.ApplyRotation = false;
        mapProp.Position = default;
        mapProp.Rotation = default;
        mapProp.Scale = default;
        mapProp.Model = null;
    }

    // OFFSET 0x021E16CC
    public static void MapPropManager_Load(NARC landDataNARC, int mapPropFilesSize, AreaDataManager areaDataManager, MapPropManager mapPropManager, MapPropAnimationManager? mapPropAnimationManager)
    {
        byte[]? mapPropFiles = null;
        uint mapPropFilesCount;
        if (mapPropFilesSize != 0)
        {
            mapPropFiles = Heap.Heap_AllocAtEnd(HeapIDField, (uint)mapPropFilesSize);
            NARC.NARC_ReadFile(landDataNARC, (uint)mapPropFilesSize, mapPropFiles!);
            mapPropFilesCount = (uint)mapPropFilesSize / MapPropFileSize;
        }
        else
        {
            mapPropFilesCount = 0;
        }
        for (int i = 0; i < MapPropManager.MaxLoadedMapProps; i++)
        {
            MapProp mapProp = mapPropManager.LoadedProps[i];
            if ((uint)i < mapPropFilesCount)
            {
                int file = i * MapPropFileSize;
                mapProp.ModelID = Bin.I32(mapPropFiles!, file + MapPropFileModelID);
                mapProp.Loaded = true;
                mapProp.ApplyRotation = false;
                mapProp.Position = ReadVector(mapPropFiles!, file + MapPropFilePosition);
                mapProp.Rotation = ReadVector(mapPropFiles!, file + MapPropFileRotation);
                mapProp.Scale = ReadVector(mapPropFiles!, file + MapPropFileScale);
                MapProp_LoadModel(mapProp.ModelID, areaDataManager, mapProp.RenderObj, out mapProp.Model);
                if (!AreaData.AreaDataManager_HasMapPropModelFile(areaDataManager, mapProp.ModelID))
                {
                    mapProp.ModelID = 0;
                }
                MapPropAnimations.MapPropAnimationManager_AddAllAnimationsToRenderObj(mapProp.ModelID, mapProp.RenderObj, mapPropAnimationManager);
            }
            else
            {
                mapProp.ModelID = 0;
                mapProp.Loaded = false;
                mapProp.ApplyRotation = false;
                mapProp.Position = default;
                mapProp.Rotation = default;
                mapProp.Scale = default;
            }
        }
        if (mapPropFiles != null)
        {
            Heap.Heap_Free(mapPropFiles);
        }
    }

    // OFFSET 0x021E17CC
    public static void MapPropManager_Render(in VecFx32 positionOffset, AreaDataManager areaDataManager, int applyAttributesToModel, ModelAttributes? modelAttributes, MapPropManager mapPropManager)
    {
        MtxFx33 rotation = IdentityRotation();
        for (int i = 0; i < MapPropManager.MaxLoadedMapProps; i++)
        {
            MapProp mapProp = mapPropManager.LoadedProps[i];
            if (!mapProp.Loaded || mapProp.Hidden)
            {
                continue;
            }
            VecFx32 position = mapProp.Position;
            position.X += positionOffset.X;
            position.Z += positionOffset.Z;
            if (GfxBoxTest.GFXBoxTest_IsModelInView(mapProp.Model!, position, rotation, mapProp.Scale) == 0)
            {
                continue;
            }
            MapPropMaterialShape materialShape = AreaData.AreaDataManager_GetMapPropMaterialShape(areaDataManager)!;
            if (applyAttributesToModel == 1)
            {
                ModelAttributes.ModelAttributes_ApplyToModel(modelAttributes!, mapProp.Model!, MapPropModelAttributesMask);
            }
            MapPropMaterialShape.MapProp_GetMaterialShapeIDsCount(mapProp.ModelID, materialShape, out ushort idsCount);
            if (idsCount == 0)
            {
                Easy3D.Easy3D_DrawRenderObj(mapProp.RenderObj, position, rotation, mapProp.Scale);
            }
            else
            {
                MapProp_RenderMaterialShapes(mapProp.Model!, position, rotation, mapProp.Scale, materialShape, mapProp.ModelID);
            }
        }
    }

    // OFFSET 0x021E1890
    public static ref VecFx32 MapProp_GetRotation(MapProp mapProp)
    {
        return ref mapProp.Rotation;
    }

    // OFFSET 0x021E1894
    public static VecFx32 MapProp_GetPosition(MapProp mapProp)
    {
        return mapProp.Position;
    }

    // OFFSET 0x021E18A4
    public static void MapProp_SetPosition(MapProp mapProp, in VecFx32 position)
    {
        mapProp.Position = position;
    }

    // OFFSET 0x021E18B4
    public static void MapProp_SetHidden(MapProp mapProp, bool hidden)
    {
        mapProp.Hidden = hidden;
    }

    // OFFSET 0x021E18B8
    public static int MapProp_GetModelID(MapProp mapProp)
    {
        return mapProp.ModelID;
    }

    // OFFSET 0x021E18BC
    public static G3dRenderObj MapProp_GetRenderObj(MapProp mapProp)
    {
        return mapProp.RenderObj;
    }

    // OFFSET 0x021E18C0
    public static G3dModel? MapProp_GetModel(MapProp mapProp)
    {
        return mapProp.Model;
    }

    // OFFSET 0x021E18C4
    public static MapProp MapPropManager_GetLoadedProp(MapPropManager mapPropManager, int index)
    {
        return mapPropManager.LoadedProps[index];
    }

    // OFFSET 0x021E18CC
    public static MapProp? MapPropManager_FindLoadedPropByModelID(MapPropManager mapPropManager, int modelID)
    {
        for (int i = 0; i < MapPropManager.MaxLoadedMapProps; i++)
        {
            if (mapPropManager.LoadedProps[i].ModelID == modelID)
            {
                return mapPropManager.LoadedProps[i];
            }
        }
        return null;
    }

    // OFFSET 0x021E18E0
    public static MapProp MapPropManager_GetLoadedPropSafely(MapPropManager mapPropManager, int index)
    {
        if (index >= MapPropManager.MaxLoadedMapProps)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapProp mapProp = mapPropManager.LoadedProps[index];
        if (!mapProp.Loaded)
        {
            ErrorHandling.GF_AssertFail();
        }
        return mapProp;
    }

    // OFFSET 0x021E1904
    private static void MapProp_LoadModel(int modelID, AreaDataManager areaDataManager, G3dRenderObj renderObj, out G3dModel? model)
    {
        byte[] modelFile = AreaData.AreaDataManager_GetMapPropModelFile(modelID, areaDataManager)!;
        G3dModelSet? modelSet = G3d.GetModelSet(modelFile);
        model = modelSet == null ? null : G3d.GetModelByIndex(modelSet, 0);
        G3d.InitRenderObj(renderObj, model!);
    }

    // OFFSET 0x021E1944
    private static void MapProp_RenderMaterialShapes(G3dModel model, in VecFx32 position, in MtxFx33 rotation, in VecFx32 scale, MapPropMaterialShape materialShape, int modelID)
    {
        Render3D.SetBaseTranslation(position);
        Render3D.SetBaseRotation(rotation);
        Render3D.SetBaseScale(scale);
        Render3D.FlushGlobalState();
        MapPropMaterialShape.MapProp_GetMaterialShapeIDsLocator(modelID, materialShape, out ushort idsCount, out ushort idsIndex);
        BufferView ids = MapPropMaterialShape.MapPropMaterialShape_GetMaterialShapeIDsAt(idsIndex, materialShape);
        byte materialID = NoMaterial;
        for (byte i = 0; i < idsCount; i++)
        {
            int nextMaterialID = ids.U16(i * 4);
            bool sendMaterial;
            if (materialID != nextMaterialID)
            {
                materialID = (byte)nextMaterialID;
                sendMaterial = true;
            }
            else
            {
                sendMaterial = false;
            }
            G3d.Draw1Mat1Shp(model, materialID, ids.U16(i * 4 + 2), sendMaterial);
        }
    }

    // OFFSET 0x021E19CC
    public static byte MapPropManager_LoadOne(MapPropManager mapPropManager, AreaDataManager areaDataManager, int modelID, in VecFx32 position, VecFx32? rotation, MapPropAnimationManager? mapPropAnimationManager)
    {
        VecFx32 scale = new() { X = Fx32.One, Y = Fx32.One, Z = Fx32.One };
        for (byte i = 0; i < MapPropManager.MaxLoadedMapProps; i++)
        {
            MapProp mapProp = mapPropManager.LoadedProps[i];
            if (mapProp.Loaded)
            {
                continue;
            }
            mapProp.Loaded = true;
            mapProp.Position = position;
            if (rotation != null)
            {
                mapProp.ApplyRotation = true;
                mapProp.Rotation = rotation.Value;
            }
            else
            {
                mapProp.ApplyRotation = false;
            }
            mapProp.Scale = scale;
            mapProp.ModelID = modelID;
            MapProp_LoadModel(modelID, areaDataManager, mapProp.RenderObj, out mapProp.Model);
            MapPropAnimations.MapPropAnimationManager_AddAnimationToRenderObj(mapProp.ModelID, 0, false, mapProp.RenderObj, mapPropAnimationManager);
            return i;
        }
        ErrorHandling.GF_AssertFail();
        return MapPropManager.MaxLoadedMapProps;
    }

    // OFFSET 0x021E1A6C
    public static void MapPropManager_Render2(MapPropManager mapPropManager, AreaDataManager areaDataManager)
    {
        MtxFx33 rotation = IdentityRotation();
        for (byte i = 0; i < MapPropManager.MaxLoadedMapProps; i++)
        {
            MapProp mapProp = mapPropManager.LoadedProps[i];
            if (!mapProp.Loaded || mapProp.Hidden)
            {
                continue;
            }
            MapPropMaterialShape materialShape = AreaData.AreaDataManager_GetMapPropMaterialShape(areaDataManager)!;
            MapPropMaterialShape.MapProp_GetMaterialShapeIDsCount(mapProp.ModelID, materialShape, out ushort idsCount);
            if (mapProp.ApplyRotation)
            {
                FxUtil.MTX_Rot33Vec(out rotation, mapProp.Rotation);
            }
            if (idsCount == 0)
            {
                Easy3D.Easy3D_DrawRenderObj(mapProp.RenderObj, mapProp.Position, rotation, mapProp.Scale);
            }
            else
            {
                MapProp_RenderMaterialShapes(mapProp.Model!, mapProp.Position, rotation, mapProp.Scale, materialShape, mapProp.ModelID);
            }
        }
    }
}
