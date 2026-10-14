using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void InitialLoadMapsFunc(LandDataManager landDataManager, int trackedTargetTileX, int trackedTargetTileZ, int offsetTileX, int offsetTileZ, int mapMatrixWidthTiles);
public delegate void LazyLoadMapsFunc(byte index, AreaDataManager? areaDataManager, MapMatrix? mapMatrix, int mapMatrixWidth, int mapMatrixHeight, LandDataManager landDataManager, MapLazyLoader loader);

public sealed class MapLoadTasksState
{
    public int MapModelTaskRunning;
    public int BdhcTaskRunning;
}

public sealed class LoadedMap
{
    public const int TerrainAttributesCount = 1024;

    public readonly ushort[] TerrainAttributes = new ushort[TerrainAttributesCount];
    public readonly G3dRenderObj MapRenderObj = new();
    public byte[]? MapModelFile;
    public byte[]? BdhcFile;
    public BDHC? Bdhc;
    public int MapMatrixIndex;
    public bool Valid;
    public MapPropManager? MapPropManager;
}

public sealed class MapLazyLoader
{
    public const int SlotCount = 2;

    public readonly LoadedMap?[] LoadSlots = new LoadedMap?[SlotCount];
    public SysTask? MapModelLoadSysTask;
    public SysTask? BdhcLoadSysTask;
    public readonly MapLoadTasksState MapLoadTasksState = new();
    public readonly int[] MapMatrixIndexes = new int[SlotCount];
    public readonly byte[] Quadrants = new byte[SlotCount];
    public byte NextIndexToLoad;
    public bool Available;
}

public sealed class MapLoadFunctions
{
    public InitialLoadMapsFunc? InitialLoad;
    public LazyLoadMapsFunc? LazyLoad;
}

public sealed class MapLazyLoaderManager
{
    public readonly MapLazyLoader Loader = new();
    public bool Loading;
    public byte LoadDirection;
}

public sealed class MapLazyLoaderPending
{
    public readonly LoadedMap?[] LoadSlots = new LoadedMap?[MapLazyLoader.SlotCount];
    public bool Pending;
    public byte TrackedTargetDirection;
    public readonly int[] MapMatrixIndexes = new int[MapLazyLoader.SlotCount];
    public readonly byte[] Quadrants = new byte[MapLazyLoader.SlotCount];
}

public sealed class MapLazyLoaderTrackedTarget
{
    public const int CoordNone = 0;
    public const int CoordPrevX = 1;
    public const int CoordPrevZ = 2;
    public const int CoordTargetX = 3;
    public const int CoordTargetZ = 4;

    public VecFx32 PrevPosition;
    public Func<VecFx32>? Position;
    public int HigherMovementCoord;
    public int LowerMovementCoord;
    public bool Moving;
    public byte MovingDirection;
    public bool Tracking;
}

public sealed class LandDataHeader
{
    public int TerrainAttributesSize;
    public int MapModelSize;
    public int BdhcSize;
    public int MapPropsSize;
}

public sealed class MapModelLoaderTaskContext
{
    public NARC? LandDataNARC;
    public int MapModelDataSize;
    public LoadedMap? Map;
    public G3dTexture? MapTexture;
    public byte CurrentSubTask;
    public bool KillLoadTask;
    public MapLoadTasksState? TasksState;
    public int BytesRead;
}

public sealed class LandDataManager
{
    public const int QuadrantCount = 4;
    public const int LazyLoaderManagerCount = 2;
    public const int InvalidQuadrant = 4;
    public const int DirectionInvalid = 0;
    public const int DirectionEast = 1;
    public const int DirectionSouth = 2;
    public const int DirectionWest = 3;
    public const int DirectionNorth = 4;

    public LoadedMapBuffers? LoadedMapBufs;
    public readonly MapLazyLoaderManager[] MapLazyLoaderMan = { new(), new() };
    public readonly MapLazyLoaderPending LazyLoaderPending = new();
    public readonly bool[] MapQuadrantsToCancelLazyLoad = new bool[QuadrantCount];
    public readonly LoadedMap?[] LoadedMaps = new LoadedMap?[QuadrantCount];
    public byte RunningLazyLoaderManCount;
    public byte NextLazyLoaderManIndex;
    public byte ActiveLazyLoaderManIndex;
    public int TrackedTargetMapMatrixIndex;
    public int TrackedTargetTileIndex;
    public byte TrackedTargetLoadedMapsQuadrant;
    public byte TrackedTargetMapQuadrant;
    public bool Ending;
    public int LazyLoaderSubTask;
    public AreaDataManager? AreaDataMan;
    public MapMatrix? MapMatrix;
    public int MapMatrixWidth;
    public int MapMatrixHeight;
    public int MapMatrixWidthTiles;
    public readonly MapLazyLoaderTrackedTarget LazyLoaderTrackedTarget = new();
    public MapPropAnimationManager? MapPropAnimMan;
    public MapLoadFunctions? MapLoadFns;
    public NARC? LandDataNARC;
    public LandDataMapLoadedCallback? MapLoadedCb;
    public object? MapLoadedCbUserData;
    public int InDistortionWorld;
    public int SkipMapProps;
    public int OffsetTileX;
    public int OffsetAltitude;
    public int OffsetTileZ;
    public VecFx32 Offset;

    private const int FieldHeapID = 4;
    private const int LandDataNarcID = 0x43;
    private const int LandDataHeaderSize = 0x10;
    private const int InvalidLandDataID = 0xFFFF;
    private const int TerrainAttributesSize = 0x800;
    private const int MapModelDataChunkSize = 0xE000;
    private const int MapTiles = 32;
    private const int HalfMapTiles = 16;
    private const int TileSize = 0x10000;
    private const int HalfTileSize = 0x8000;
    private const int MapSizeShift = 21;
    private const int MapCenter = 0x100000;
    private const int AltitudeShift = 15;
    private const int SubTaskLoadMap = 0;
    private const int SubTaskFinishMapLoad = 1;
    private const int SubTaskIdle = 2;
    private const int SubTaskGracefulShutdown = 3;
    private const byte ModelSubTaskFirstFileRead = 0;
    private const byte ModelSubTaskFileRead = 1;
    private const byte ModelSubTaskBindTexture = 2;
    private const byte ModelSubTaskInitRenderObj = 3;
    private const byte ModelSubTaskEndTask = 5;
    private const uint TaskPriority = 1;

    private static readonly MapLoadFunctions sMapLoadWithoutAttributesFns = new()
    {
        InitialLoad = LandDataManager_InitAndLoadWithoutAttributes,
        LazyLoad = LandDataManager_LazyLoadWithoutAttributes,
    };

    private static readonly MapLoadFunctions sMapLoadFns = new()
    {
        InitialLoad = LandDataManager_InitAndLoad,
        LazyLoad = LandDataManager_LazyLoad,
    };

    private static void ReadInto(NARC narc, int size, byte[] dest, int offset)
    {
        byte[] temp = new byte[size];
        NARC.NARC_ReadFile(narc, (uint)size, temp);
        Buffer.BlockCopy(temp, 0, dest, offset, size);
    }

    private static void ReadTerrainAttributes(NARC narc, int size, LoadedMap map)
    {
        byte[] temp = new byte[size];
        NARC.NARC_ReadFile(narc, (uint)size, temp);
        int count = size / 2;
        if (count > LoadedMap.TerrainAttributesCount)
        {
            count = LoadedMap.TerrainAttributesCount;
        }
        Buffer.BlockCopy(temp, 0, map.TerrainAttributes, 0, count * 2);
    }

    private static void ClearTerrainAttributes(LoadedMap map)
    {
        Array.Fill(map.TerrainAttributes, (ushort)0xFFFF);
    }

    private static G3dModel? GetFirstModel(byte[] mapModelFile)
    {
        G3dModelSet? modelSet = G3d.GetModelSet(mapModelFile);
        if (modelSet == null)
        {
            return null;
        }
        if (modelSet.File[modelSet.Offset + 9] == 0)
        {
            return null;
        }
        return G3d.GetModelByIndex(modelSet, 0);
    }

    private static int GetModelCount(byte[] mapModelFile)
    {
        G3dModelSet modelSet = G3d.GetModelSet(mapModelFile)!;
        return modelSet.File[modelSet.Offset + 9];
    }

    private static MtxFx33 IdentityRotation()
    {
        return new MtxFx33 { M00 = Fx32.One, M11 = Fx32.One, M22 = Fx32.One };
    }

    private static int ReadCoord(MapLazyLoaderTrackedTarget trackedTarget, int coord)
    {
        switch (coord)
        {
            case MapLazyLoaderTrackedTarget.CoordPrevX:
                return trackedTarget.PrevPosition.X;
            case MapLazyLoaderTrackedTarget.CoordPrevZ:
                return trackedTarget.PrevPosition.Z;
            case MapLazyLoaderTrackedTarget.CoordTargetX:
                return trackedTarget.Position!().X;
            default:
                return trackedTarget.Position!().Z;
        }
    }

    private static void CopyLazyLoaderManager(MapLazyLoaderManager dest, MapLazyLoaderManager src)
    {
        for (int i = 0; i < MapLazyLoader.SlotCount; i++)
        {
            dest.Loader.LoadSlots[i] = src.Loader.LoadSlots[i];
            dest.Loader.MapMatrixIndexes[i] = src.Loader.MapMatrixIndexes[i];
            dest.Loader.Quadrants[i] = src.Loader.Quadrants[i];
        }
        dest.Loader.MapModelLoadSysTask = src.Loader.MapModelLoadSysTask;
        dest.Loader.BdhcLoadSysTask = src.Loader.BdhcLoadSysTask;
        dest.Loader.MapLoadTasksState.MapModelTaskRunning = src.Loader.MapLoadTasksState.MapModelTaskRunning;
        dest.Loader.MapLoadTasksState.BdhcTaskRunning = src.Loader.MapLoadTasksState.BdhcTaskRunning;
        dest.Loader.NextIndexToLoad = src.Loader.NextIndexToLoad;
        dest.Loader.Available = src.Loader.Available;
        dest.Loading = src.Loading;
        dest.LoadDirection = src.LoadDirection;
    }

    private static void CopyLoaderState(LandDataManager dest, LandDataManager src)
    {
        for (int i = 0; i < LazyLoaderManagerCount; i++)
        {
            CopyLazyLoaderManager(dest.MapLazyLoaderMan[i], src.MapLazyLoaderMan[i]);
        }
        for (int i = 0; i < QuadrantCount; i++)
        {
            dest.MapQuadrantsToCancelLazyLoad[i] = src.MapQuadrantsToCancelLazyLoad[i];
        }
        dest.RunningLazyLoaderManCount = src.RunningLazyLoaderManCount;
        dest.NextLazyLoaderManIndex = src.NextLazyLoaderManIndex;
        dest.ActiveLazyLoaderManIndex = src.ActiveLazyLoaderManIndex;
        dest.TrackedTargetMapMatrixIndex = src.TrackedTargetMapMatrixIndex;
        dest.TrackedTargetTileIndex = src.TrackedTargetTileIndex;
        dest.TrackedTargetLoadedMapsQuadrant = src.TrackedTargetLoadedMapsQuadrant;
        dest.TrackedTargetMapQuadrant = src.TrackedTargetMapQuadrant;
        for (int i = 0; i < QuadrantCount; i++)
        {
            LoadedMap destMap = dest.LoadedMaps[i]!;
            LoadedMap srcMap = src.LoadedMaps[i]!;
            destMap.MapRenderObj.Model = srcMap.MapRenderObj.Model;
            destMap.MapRenderObj.AnimObjs.Clear();
            destMap.MapRenderObj.AnimObjs.AddRange(srcMap.MapRenderObj.AnimObjs);
            destMap.MapModelFile = srcMap.MapModelFile;
            destMap.Valid = srcMap.Valid;
            destMap.MapMatrixIndex = srcMap.MapMatrixIndex;
        }
    }

    private static void CopyTrackedTargetAndOffsets(LandDataManager dest, LandDataManager src)
    {
        MapLazyLoaderTrackedTarget destTarget = dest.LazyLoaderTrackedTarget;
        MapLazyLoaderTrackedTarget srcTarget = src.LazyLoaderTrackedTarget;
        destTarget.PrevPosition = srcTarget.PrevPosition;
        destTarget.Position = srcTarget.Position;
        destTarget.HigherMovementCoord = srcTarget.HigherMovementCoord;
        destTarget.LowerMovementCoord = srcTarget.LowerMovementCoord;
        destTarget.Moving = srcTarget.Moving;
        destTarget.MovingDirection = srcTarget.MovingDirection;
        destTarget.Tracking = srcTarget.Tracking;
        dest.InDistortionWorld = src.InDistortionWorld;
        dest.OffsetTileX = src.OffsetTileX;
        dest.OffsetAltitude = src.OffsetAltitude;
        dest.OffsetTileZ = src.OffsetTileZ;
        dest.Offset = src.Offset;
    }

    private static bool UpdateTrackedTarget(LandDataManager landDataManager)
    {
        MapLazyLoaderTrackedTarget trackedTarget = landDataManager.LazyLoaderTrackedTarget;
        if (landDataManager.InDistortionWorld != 0 || !trackedTarget.Tracking)
        {
            return true;
        }
        if (trackedTarget.Moving)
        {
            if (ReadCoord(trackedTarget, trackedTarget.LowerMovementCoord) > ReadCoord(trackedTarget, trackedTarget.HigherMovementCoord))
            {
                ErrorHandling.GF_AssertFail();
            }
            if (ReadCoord(trackedTarget, trackedTarget.HigherMovementCoord) - ReadCoord(trackedTarget, trackedTarget.LowerMovementCoord) >= TileSize)
            {
                VecFx32 position = trackedTarget.Position!();
                LandData_ObjectPosToTilePos(position.X, position.Z, out int tileX, out int tileZ);
                LandDataManager_UpdateTrackedTargetValues(tileX, tileZ, landDataManager.OffsetTileX, landDataManager.OffsetTileZ, landDataManager);
                trackedTarget.PrevPosition = trackedTarget.Position!();
                if (trackedTarget.PrevPosition.Z % HalfTileSize != 0)
                {
                    if (CommManager.CommMan_SetErrorState(1) != 0)
                    {
                        return false;
                    }
                }
                if (trackedTarget.PrevPosition.Z % HalfTileSize != 0)
                {
                    ErrorHandling.GF_AssertFail();
                }
                trackedTarget.Moving = false;
                trackedTarget.HigherMovementCoord = MapLazyLoaderTrackedTarget.CoordNone;
                trackedTarget.LowerMovementCoord = MapLazyLoaderTrackedTarget.CoordNone;
            }
            return true;
        }
        VecFx32 target = trackedTarget.Position!();
        if (trackedTarget.PrevPosition.X != target.X && trackedTarget.PrevPosition.Z != target.Z)
        {
            ErrorHandling.GF_AssertFail();
            return true;
        }
        if (trackedTarget.PrevPosition.X == target.X && trackedTarget.PrevPosition.Z == target.Z)
        {
            return true;
        }
        trackedTarget.Moving = true;
        int alignedCoord;
        if (trackedTarget.PrevPosition.X != target.X)
        {
            if (trackedTarget.PrevPosition.X > target.X)
            {
                trackedTarget.HigherMovementCoord = MapLazyLoaderTrackedTarget.CoordPrevX;
                trackedTarget.LowerMovementCoord = MapLazyLoaderTrackedTarget.CoordTargetX;
                trackedTarget.MovingDirection = DirectionWest;
            }
            else
            {
                trackedTarget.HigherMovementCoord = MapLazyLoaderTrackedTarget.CoordTargetX;
                trackedTarget.LowerMovementCoord = MapLazyLoaderTrackedTarget.CoordPrevX;
                trackedTarget.MovingDirection = DirectionEast;
            }
            alignedCoord = trackedTarget.PrevPosition.X;
        }
        else
        {
            if (trackedTarget.PrevPosition.Z > target.Z)
            {
                trackedTarget.HigherMovementCoord = MapLazyLoaderTrackedTarget.CoordPrevZ;
                trackedTarget.LowerMovementCoord = MapLazyLoaderTrackedTarget.CoordTargetZ;
                trackedTarget.MovingDirection = DirectionNorth;
            }
            else
            {
                trackedTarget.HigherMovementCoord = MapLazyLoaderTrackedTarget.CoordTargetZ;
                trackedTarget.LowerMovementCoord = MapLazyLoaderTrackedTarget.CoordPrevZ;
                trackedTarget.MovingDirection = DirectionSouth;
            }
            alignedCoord = trackedTarget.PrevPosition.Z;
        }
        if (alignedCoord % HalfTileSize != 0)
        {
            if (CommManager.CommMan_SetErrorState(1) != 0)
            {
                return false;
            }
        }
        if (alignedCoord % HalfTileSize != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        LandDataManager_CheckLazyLoadNextMapPair(trackedTarget.MovingDirection, landDataManager);
        return true;
    }

    private static void FinishMapLoad(LandDataManager landDataManager, MapLazyLoader loader)
    {
        byte quadrant = loader.Quadrants[loader.NextIndexToLoad - 1];
        if (landDataManager.MapQuadrantsToCancelLazyLoad[quadrant])
        {
            MapLazyLoader_KillTasks(loader);
            landDataManager.MapQuadrantsToCancelLazyLoad[loader.Quadrants[loader.NextIndexToLoad - 1]] = false;
        }
        if (loader.NextIndexToLoad > MapLazyLoader.SlotCount)
        {
            return;
        }
        if (!LandDataManager_LazyLoadFinished(loader.MapLoadTasksState))
        {
            return;
        }
        if (loader.NextIndexToLoad >= MapLazyLoader.SlotCount)
        {
            loader.Available = true;
        }
        else
        {
            landDataManager.LazyLoaderSubTask = SubTaskLoadMap;
        }
        LoadedMap map = loader.LoadSlots[loader.NextIndexToLoad - 1]!;
        if (map.Valid)
        {
            G3dModel? mapModel = GetFirstModel(map.MapModelFile!);
            if (AreaData.AreaDataManager_IsOutdoorsLighting(landDataManager.AreaDataMan!))
            {
                AreaLight.AreaLight_UseGlobalModelAttributes(mapModel!);
            }
        }
        map = loader.LoadSlots[loader.NextIndexToLoad - 1]!;
        int mapMatrixIndex = map.MapMatrixIndex;
        if (mapMatrixIndex < 0 || mapMatrixIndex >= landDataManager.MapMatrixWidth * landDataManager.MapMatrixHeight)
        {
            return;
        }
        if (landDataManager.MapLoadedCb != null)
        {
            landDataManager.MapLoadedCb(landDataManager.MapLoadedCbUserData, mapMatrixIndex, map.MapPropManager);
        }
    }

    private static void AdvanceLazyLoaderManagers(LandDataManager landDataManager)
    {
        if (landDataManager.RunningLazyLoaderManCount == 0)
        {
            landDataManager.ActiveLazyLoaderManIndex = 0;
            landDataManager.NextLazyLoaderManIndex = 0;
            landDataManager.LazyLoaderSubTask = SubTaskIdle;
            return;
        }
        if (!LandDataManager_IsActiveLazyLoaderAvailable(landDataManager))
        {
            return;
        }
        landDataManager.MapLazyLoaderMan[landDataManager.ActiveLazyLoaderManIndex].Loading = false;
        LandDataManager_ResetMapQuadrantsToCancelLazyLoad(landDataManager);
        landDataManager.RunningLazyLoaderManCount--;
        landDataManager.ActiveLazyLoaderManIndex = (byte)((landDataManager.ActiveLazyLoaderManIndex + 1) % LazyLoaderManagerCount);
        if (landDataManager.RunningLazyLoaderManCount != 0)
        {
            landDataManager.LazyLoaderSubTask = SubTaskLoadMap;
        }
        else
        {
            landDataManager.ActiveLazyLoaderManIndex = 0;
            landDataManager.NextLazyLoaderManIndex = 0;
            landDataManager.LazyLoaderSubTask = SubTaskIdle;
        }
        if (landDataManager.LazyLoaderPending.Pending)
        {
            landDataManager.LazyLoaderPending.Pending = false;
            LandDataManager_CancelMapQuadrantsToLazyLoad(landDataManager.LazyLoaderPending.TrackedTargetDirection, landDataManager);
            LandDataManager_QueueLazyLoadPendingMaps(landDataManager);
        }
    }

    // OFFSET 0x021E779C
    private static bool LandData_AreMapsInSameColumn(int lhsX, int rhsX, int mapMatrixWidth)
    {
        return lhsX % mapMatrixWidth == rhsX % mapMatrixWidth;
    }

    // OFFSET 0x021E77C0
    private static bool LandData_AreMapsInSameLine(int lhsY, int rhsY, int mapMatrixWidth)
    {
        return lhsY / mapMatrixWidth == rhsY / mapMatrixWidth;
    }

    // OFFSET 0x021E77E4
    private static void LandDataManager_KillLoadTasks(LandDataManager landDataManager, byte index)
    {
        MapLazyLoader loader = landDataManager.MapLazyLoaderMan[index].Loader;
        if (loader.MapLoadTasksState.MapModelTaskRunning != 0)
        {
            LandDataManager_KillLoadMapModel(loader.MapModelLoadSysTask!);
        }
        if (loader.MapLoadTasksState.BdhcTaskRunning != 0)
        {
            BDHC.BDHC_KillLoad(loader.BdhcLoadSysTask!);
        }
        loader.Available = true;
    }

    // OFFSET 0x021E7814
    private static void MapLazyLoader_KillTasks(MapLazyLoader loader)
    {
        if (loader.MapLoadTasksState.MapModelTaskRunning != 0)
        {
            LandDataManager_KillLoadMapModel(loader.MapModelLoadSysTask!);
        }
        if (loader.MapLoadTasksState.BdhcTaskRunning != 0)
        {
            BDHC.BDHC_KillLoad(loader.BdhcLoadSysTask!);
        }
        loader.MapLoadTasksState.MapModelTaskRunning = 0;
        loader.MapLoadTasksState.BdhcTaskRunning = 0;
    }

    // OFFSET 0x021E7838
    private static void LandData_CalculateLoadedMapMatrixIndexes(int inDistortionWorld, int trackedTargetTileX, int trackedTargetTileZ, int offsetTileX, int offsetTileZ, int mapMatrixWidth, int mapMatrixHeight, int mapMatrixWidthTiles, int[] loadedMapMatrixIndexes)
    {
        if (inDistortionWorld == 1)
        {
            for (int i = 0; i < QuadrantCount; i++)
            {
                loadedMapMatrixIndexes[i] = -1;
            }
            if (mapMatrixWidth == 1)
            {
                if (mapMatrixHeight == 1)
                {
                    loadedMapMatrixIndexes[0] = 0;
                    return;
                }
                loadedMapMatrixIndexes[0] = 0;
                loadedMapMatrixIndexes[2] = 1;
                return;
            }
            loadedMapMatrixIndexes[0] = 0;
            loadedMapMatrixIndexes[1] = 1;
            if (mapMatrixWidth >= 2)
            {
                loadedMapMatrixIndexes[2] = 2;
                loadedMapMatrixIndexes[3] = 3;
            }
            return;
        }
        if (trackedTargetTileX < offsetTileX)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (trackedTargetTileZ < offsetTileZ)
        {
            ErrorHandling.GF_AssertFail();
        }
        int mapX = (trackedTargetTileX - offsetTileX) / MapTiles;
        int mapZ = (trackedTargetTileZ - offsetTileZ) / MapTiles;
        int tileIndex = LandData_CalculateTileIndex(trackedTargetTileX, trackedTargetTileZ, offsetTileX, offsetTileZ, mapMatrixWidthTiles);
        byte quadrant = LandDataManager_CalculateMapQuadrantOfTile((uint)tileIndex, mapMatrixWidthTiles);
        int mapMatrixIndex = mapX + (mapZ * mapMatrixWidth);
        switch (quadrant)
        {
            case 0:
                loadedMapMatrixIndexes[3] = mapMatrixIndex;
                loadedMapMatrixIndexes[0] = mapMatrixIndex - mapMatrixWidth - 1;
                loadedMapMatrixIndexes[1] = mapMatrixIndex - mapMatrixWidth;
                loadedMapMatrixIndexes[2] = mapMatrixIndex - 1;
                if (mapX - 1 < 0)
                {
                    loadedMapMatrixIndexes[0] = -1;
                    loadedMapMatrixIndexes[2] = -1;
                }
                if (mapZ - 1 < 0)
                {
                    loadedMapMatrixIndexes[1] = -1;
                }
                break;
            case 1:
                loadedMapMatrixIndexes[2] = mapMatrixIndex;
                loadedMapMatrixIndexes[0] = mapMatrixIndex - mapMatrixWidth;
                loadedMapMatrixIndexes[1] = mapMatrixIndex - mapMatrixWidth + 1;
                loadedMapMatrixIndexes[3] = mapMatrixIndex + 1;
                if (mapX + 1 >= mapMatrixWidth)
                {
                    loadedMapMatrixIndexes[1] = -1;
                    loadedMapMatrixIndexes[3] = -1;
                }
                if (mapZ - 1 < 0)
                {
                    loadedMapMatrixIndexes[0] = -1;
                }
                break;
            case 2:
                loadedMapMatrixIndexes[1] = mapMatrixIndex;
                loadedMapMatrixIndexes[0] = mapMatrixIndex - 1;
                loadedMapMatrixIndexes[2] = mapMatrixIndex + mapMatrixWidth - 1;
                loadedMapMatrixIndexes[3] = mapMatrixIndex + mapMatrixWidth;
                if (mapX - 1 < 0)
                {
                    loadedMapMatrixIndexes[0] = -1;
                    loadedMapMatrixIndexes[2] = -1;
                }
                if (mapZ + 1 > mapMatrixHeight)
                {
                    loadedMapMatrixIndexes[3] = -1;
                }
                break;
            case 3:
                loadedMapMatrixIndexes[0] = mapMatrixIndex;
                loadedMapMatrixIndexes[1] = mapMatrixIndex + 1;
                loadedMapMatrixIndexes[2] = mapMatrixIndex + mapMatrixWidth;
                loadedMapMatrixIndexes[3] = mapMatrixIndex + mapMatrixWidth + 1;
                if (mapX + 1 >= mapMatrixWidth)
                {
                    loadedMapMatrixIndexes[1] = -1;
                    loadedMapMatrixIndexes[3] = -1;
                }
                if (mapZ + 1 > mapMatrixHeight)
                {
                    loadedMapMatrixIndexes[0] = -1;
                }
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
    }

    // OFFSET 0x021E79A8
    private static void LandDataManager_NewLoadedMaps(LandDataManager landDataManager)
    {
        for (byte i = 0; i < QuadrantCount; i++)
        {
            LoadedMap map = new();
            landDataManager.LoadedMaps[i] = map;
            map.Valid = false;
            LoadedMapBuffers.LoadedMapBuffers_GetMapModelFileBufPtr(i, landDataManager.LoadedMapBufs!, out map.MapModelFile);
            LoadedMapBuffers.LoadedMapBuffers_GetBDHCFileBufPtr(i, landDataManager.LoadedMapBufs!, out map.BdhcFile);
            if (landDataManager.SkipMapProps == 0)
            {
                map.MapPropManager = MapProps.MapPropManager_New(FieldHeapID);
            }
            else
            {
                map.MapPropManager = null;
            }
            map.MapMatrixIndex = -1;
            ClearTerrainAttributes(map);
        }
    }

    // OFFSET 0x021E7A54
    public static void LandDataManager_DistortionWorldNewLoadedMapsWithoutAttributesAndModel(LandDataManager landDataManager)
    {
        for (byte i = 0; i < QuadrantCount; i++)
        {
            LoadedMap map = new();
            landDataManager.LoadedMaps[i] = map;
            map.Valid = false;
            if (landDataManager.SkipMapProps == 0)
            {
                map.MapPropManager = MapProps.MapPropManager_New(FieldHeapID);
            }
            else
            {
                map.MapPropManager = null;
            }
            map.MapMatrixIndex = -1;
        }
    }

    // OFFSET 0x021E7AC4
    private static void LandDataManager_NewLoadedMapsWithoutAttributes(LandDataManager landDataManager)
    {
        for (byte i = 0; i < QuadrantCount; i++)
        {
            LoadedMap map = new();
            landDataManager.LoadedMaps[i] = map;
            map.Valid = false;
            LoadedMapBuffers.LoadedMapBuffers_GetMapModelFileBufPtr(i, landDataManager.LoadedMapBufs!, out map.MapModelFile);
            if (landDataManager.SkipMapProps == 0)
            {
                map.MapPropManager = MapProps.MapPropManager_New(FieldHeapID);
            }
            else
            {
                map.MapPropManager = null;
            }
            map.MapMatrixIndex = -1;
        }
    }

    // OFFSET 0x021E7B4C
    private static void LandDataManager_InitLoadedMapsWithoutAttributes(LandDataManager landDataManager)
    {
        for (byte i = 0; i < QuadrantCount; i++)
        {
            LoadedMap map = landDataManager.LoadedMaps[i]!;
            map.Valid = false;
            LoadedMapBuffers.LoadedMapBuffers_GetMapModelFileBufPtr(i, landDataManager.LoadedMapBufs!, out map.MapModelFile);
            if (map.MapPropManager != null)
            {
                MapProps.MapPropManager_Init(map.MapPropManager);
            }
            map.MapMatrixIndex = -1;
        }
    }

    // OFFSET 0x021E7BAC
    private static void LandDataHeader_Load(NARC? landDataNARC, int landDataID, LandDataHeader landDataHeader)
    {
        byte[] data;
        if (landDataNARC != null)
        {
            data = Heap.Heap_AllocAtEnd(FieldHeapID, LandDataHeaderSize)!;
            NARC.NARC_ReadFromMember(landDataNARC, (uint)landDataID, 0, LandDataHeaderSize, data);
        }
        else
        {
            ErrorHandling.GF_AssertFail();
            data = NARC.NARC_AllocAtEndAndReadFromMemberByIndexPair(LandDataNarcID, landDataID, FieldHeapID, 0, LandDataHeaderSize);
        }
        landDataHeader.TerrainAttributesSize = Bin.I32(data, 0);
        landDataHeader.MapPropsSize = Bin.I32(data, 4);
        landDataHeader.MapModelSize = Bin.I32(data, 8);
        landDataHeader.BdhcSize = Bin.I32(data, 12);
        Heap.Heap_Free(data);
    }

    // OFFSET 0x021E7C00
    private static void LandDataManager_LazyLoad(byte index, AreaDataManager? areaDataManager, MapMatrix? mapMatrix, int mapMatrixWidth, int mapMatrixHeight, LandDataManager landDataManager, MapLazyLoader loader)
    {
        int mapMatrixIndex = loader.MapMatrixIndexes[index];
        if (mapMatrixIndex < 0 || mapMatrixIndex >= mapMatrixWidth * mapMatrixHeight)
        {
            return;
        }
        int landDataID = MapMatrix.MapMatrix_GetLandDataIDByIndex(mapMatrixIndex, mapMatrix);
        if (landDataID == InvalidLandDataID)
        {
            return;
        }
        LandDataHeader landDataHeader = new();
        LandDataHeader_Load(landDataManager.LandDataNARC, landDataID, landDataHeader);
        LoadedMap map = loader.LoadSlots[index]!;
        ReadTerrainAttributes(landDataManager.LandDataNARC!, landDataHeader.TerrainAttributesSize, map);
        if (map.MapPropManager != null)
        {
            MapProps.MapPropManager_Load(landDataManager.LandDataNARC!, landDataHeader.MapPropsSize, areaDataManager!, map.MapPropManager, landDataManager.MapPropAnimMan);
        }
        loader.MapLoadTasksState.MapModelTaskRunning++;
        map = loader.LoadSlots[index]!;
        loader.MapModelLoadSysTask = LandDataManager_LazyLoadMapModel(landDataManager.LandDataNARC!, landDataHeader.MapModelSize, map, AreaData.AreaDataManager_GetMapTexture(areaDataManager), loader.MapLoadTasksState);
        loader.MapLoadTasksState.BdhcTaskRunning++;
        map = loader.LoadSlots[index]!;
        loader.BdhcLoadSysTask = BDHC.BDHC_LazyLoad(landDataManager.LandDataNARC!, landDataHeader.BdhcSize, map.Bdhc, loader.MapLoadTasksState, map.BdhcFile);
    }

    // OFFSET 0x021E7CD4
    private static void LandDataManager_LazyLoadWithoutAttributes(byte index, AreaDataManager? areaDataManager, MapMatrix? mapMatrix, int mapMatrixWidth, int mapMatrixHeight, LandDataManager landDataManager, MapLazyLoader loader)
    {
        int mapMatrixIndex = loader.MapMatrixIndexes[index];
        if (mapMatrixIndex < 0 || mapMatrixIndex >= mapMatrixWidth * mapMatrixHeight)
        {
            return;
        }
        int landDataID = MapMatrix.MapMatrix_GetLandDataIDByIndex(mapMatrixIndex, mapMatrix);
        if (landDataID == InvalidLandDataID)
        {
            return;
        }
        LandDataHeader landDataHeader = new();
        LandDataHeader_Load(landDataManager.LandDataNARC, landDataID, landDataHeader);
        byte[] terrainAttributes = new byte[TerrainAttributesSize];
        NARC.NARC_ReadFile(landDataManager.LandDataNARC!, TerrainAttributesSize, terrainAttributes);
        LoadedMap map = loader.LoadSlots[index]!;
        if (map.MapPropManager != null)
        {
            MapProps.MapPropManager_Load(landDataManager.LandDataNARC!, landDataHeader.MapPropsSize, areaDataManager!, map.MapPropManager, landDataManager.MapPropAnimMan);
        }
        loader.MapLoadTasksState.MapModelTaskRunning++;
        map = loader.LoadSlots[index]!;
        loader.MapModelLoadSysTask = LandDataManager_LazyLoadMapModel(landDataManager.LandDataNARC!, landDataHeader.MapModelSize, map, AreaData.AreaDataManager_GetMapTexture(areaDataManager), loader.MapLoadTasksState);
    }

    // OFFSET 0x021E7D98
    private static void LandDataManager_LazyLoadWithoutAttributesAndProps(byte index, AreaDataManager? areaDataManager, MapMatrix? mapMatrix, int mapMatrixWidth, int mapMatrixHeight, LandDataManager landDataManager, MapLazyLoader loader)
    {
        int mapMatrixIndex = loader.MapMatrixIndexes[index];
        if (mapMatrixIndex < 0 || mapMatrixIndex >= mapMatrixWidth * mapMatrixHeight)
        {
            return;
        }
        int landDataID = MapMatrix.MapMatrix_GetLandDataIDByIndex(mapMatrixIndex, mapMatrix);
        if (landDataID == InvalidLandDataID)
        {
            return;
        }
        LandDataHeader landDataHeader = new();
        LandDataHeader_Load(landDataManager.LandDataNARC, landDataID, landDataHeader);
        NARC.NARC_Seek(landDataManager.LandDataNARC!, TerrainAttributesSize);
        NARC.NARC_Seek(landDataManager.LandDataNARC!, (uint)landDataHeader.MapPropsSize);
        loader.MapLoadTasksState.MapModelTaskRunning++;
        LoadedMap map = loader.LoadSlots[index]!;
        loader.MapModelLoadSysTask = LandDataManager_LazyLoadMapModel(landDataManager.LandDataNARC!, landDataHeader.MapModelSize, map, AreaData.AreaDataManager_GetMapTexture(areaDataManager), loader.MapLoadTasksState);
    }

    // OFFSET 0x021E7E28
    private static void LandDataManager_Load(int mapMatrixIndex, byte loadedMapIndex, AreaDataManager? areaDataManager, MapMatrix? mapMatrix, int mapMatrixWidth, int mapMatrixHeight, bool isOutdoorsLighting, LandDataManager landDataManager)
    {
        if (mapMatrixIndex < 0 || mapMatrixIndex >= mapMatrixWidth * mapMatrixHeight)
        {
            return;
        }
        int landDataID = MapMatrix.MapMatrix_GetLandDataIDByIndex(mapMatrixIndex, mapMatrix);
        if (landDataID == InvalidLandDataID)
        {
            return;
        }
        LandDataHeader landDataHeader = new();
        LandDataHeader_Load(landDataManager.LandDataNARC, landDataID, landDataHeader);
        LoadedMap map = landDataManager.LoadedMaps[loadedMapIndex]!;
        ReadTerrainAttributes(landDataManager.LandDataNARC!, landDataHeader.TerrainAttributesSize, map);
        if (map.MapPropManager != null)
        {
            MapProps.MapPropManager_Load(landDataManager.LandDataNARC!, landDataHeader.MapPropsSize, areaDataManager!, map.MapPropManager, landDataManager.MapPropAnimMan);
        }
        G3dModel? mapModel = LandDataManager_LoadMapModel(landDataManager.LandDataNARC!, landDataHeader.MapModelSize, map, AreaData.AreaDataManager_GetMapTexture(areaDataManager));
        if (isOutdoorsLighting)
        {
            AreaLight.AreaLight_UseGlobalModelAttributes(mapModel!);
        }
        map.Valid = true;
        BDHC.BDHC_Load(landDataManager.LandDataNARC!, landDataHeader.BdhcSize, map.Bdhc!, map.BdhcFile);
        map.MapMatrixIndex = mapMatrixIndex;
        if (landDataManager.MapLoadedCb != null)
        {
            landDataManager.MapLoadedCb(landDataManager.MapLoadedCbUserData, mapMatrixIndex, map.MapPropManager);
        }
    }

    // OFFSET 0x021E7F1C
    private static void LandDataManager_LoadWithoutAttributes(int mapMatrixIndex, byte loadedMapIndex, AreaDataManager? areaDataManager, MapMatrix? mapMatrix, int mapMatrixWidth, int mapMatrixHeight, bool isOutdoorsLighting, LandDataManager landDataManager)
    {
        if (mapMatrixIndex < 0 || mapMatrixIndex >= mapMatrixWidth * mapMatrixHeight)
        {
            return;
        }
        int landDataID = MapMatrix.MapMatrix_GetLandDataIDByIndex(mapMatrixIndex, mapMatrix);
        if (landDataID == InvalidLandDataID)
        {
            return;
        }
        LandDataHeader landDataHeader = new();
        LandDataHeader_Load(landDataManager.LandDataNARC, landDataID, landDataHeader);
        byte[] terrainAttributes = new byte[TerrainAttributesSize];
        NARC.NARC_ReadFile(landDataManager.LandDataNARC!, TerrainAttributesSize, terrainAttributes);
        LoadedMap map = landDataManager.LoadedMaps[loadedMapIndex]!;
        if (map.MapPropManager != null)
        {
            MapProps.MapPropManager_Load(landDataManager.LandDataNARC!, landDataHeader.MapPropsSize, areaDataManager!, map.MapPropManager, landDataManager.MapPropAnimMan);
        }
        G3dModel? mapModel = LandDataManager_LoadMapModel(landDataManager.LandDataNARC!, landDataHeader.MapModelSize, map, AreaData.AreaDataManager_GetMapTexture(areaDataManager));
        if (isOutdoorsLighting)
        {
            AreaLight.AreaLight_UseGlobalModelAttributes(mapModel!);
        }
        map.Valid = true;
        map.MapMatrixIndex = mapMatrixIndex;
    }

    // OFFSET 0x021E7FF0
    private static void LandDataManager_LoadWithoutModel(int mapMatrixIndex, byte loadedMapIndex, AreaDataManager? areaDataManager, MapMatrix? mapMatrix, int mapMatrixWidth, int mapMatrixHeight, bool isOutdoorsLighting, LandDataManager landDataManager)
    {
        if (mapMatrixIndex < 0 || mapMatrixIndex >= mapMatrixWidth * mapMatrixHeight)
        {
            return;
        }
        int landDataID = MapMatrix.MapMatrix_GetLandDataIDByIndex(mapMatrixIndex, mapMatrix);
        if (landDataID == InvalidLandDataID)
        {
            return;
        }
        LandDataHeader landDataHeader = new();
        LandDataHeader_Load(landDataManager.LandDataNARC, landDataID, landDataHeader);
        LoadedMap map = landDataManager.LoadedMaps[loadedMapIndex]!;
        ReadTerrainAttributes(landDataManager.LandDataNARC!, landDataHeader.TerrainAttributesSize, map);
        if (map.MapPropManager != null)
        {
            MapProps.MapPropManager_Load(landDataManager.LandDataNARC!, landDataHeader.MapPropsSize, areaDataManager!, map.MapPropManager, landDataManager.MapPropAnimMan);
        }
        NARC.NARC_Seek(landDataManager.LandDataNARC!, (uint)landDataHeader.MapModelSize);
        BDHC.BDHC_Load(landDataManager.LandDataNARC!, landDataHeader.BdhcSize, map.Bdhc!, map.BdhcFile);
        map.MapMatrixIndex = mapMatrixIndex;
        if (landDataManager.MapLoadedCb != null)
        {
            landDataManager.MapLoadedCb(landDataManager.MapLoadedCbUserData, mapMatrixIndex, map.MapPropManager);
        }
    }

    // OFFSET 0x021E80BC
    private static bool LandDataManager_LazyLoadFinished(MapLoadTasksState tasksState)
    {
        if (tasksState.MapModelTaskRunning == 0 && tasksState.BdhcTaskRunning == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021E80D0
    private static bool LandDataManager_SetMapsToLazyLoaderManager(LandDataManager landDataManager, int firstMapMatrixIndex, int secondMapMatrixIndex, byte firstQuadrantToLoad, byte secondQuadrantToLoad, byte lazyLoaderManIndex, byte trackedTargetDirection)
    {
        MapLazyLoaderManager manager = landDataManager.MapLazyLoaderMan[lazyLoaderManIndex];
        if (manager.Loading)
        {
            ErrorHandling.GF_AssertFail();
        }
        manager.Loading = true;
        manager.LoadDirection = trackedTargetDirection;
        manager.Loader.MapMatrixIndexes[0] = firstMapMatrixIndex;
        manager.Loader.MapMatrixIndexes[1] = secondMapMatrixIndex;
        manager.Loader.Quadrants[0] = firstQuadrantToLoad;
        manager.Loader.Quadrants[1] = secondQuadrantToLoad;
        manager.Loader.LoadSlots[0] = landDataManager.LoadedMaps[firstQuadrantToLoad];
        manager.Loader.LoadSlots[1] = landDataManager.LoadedMaps[secondQuadrantToLoad];
        manager.Loader.NextIndexToLoad = 0;
        manager.Loader.Available = false;
        if (landDataManager.RunningLazyLoaderManCount == 0)
        {
            landDataManager.LazyLoaderSubTask = SubTaskLoadMap;
        }
        landDataManager.LoadedMaps[firstQuadrantToLoad]!.MapMatrixIndex = firstMapMatrixIndex;
        landDataManager.LoadedMaps[secondQuadrantToLoad]!.MapMatrixIndex = secondMapMatrixIndex;
        landDataManager.RunningLazyLoaderManCount++;
        landDataManager.NextLazyLoaderManIndex = (byte)((landDataManager.NextLazyLoaderManIndex + 1) % LazyLoaderManagerCount);
        return true;
    }

    // OFFSET 0x021E8188
    public static void LandDataManager_Tick(FieldSystem fieldSystem, LandDataManager landDataManager)
    {
        MapLazyLoader loader = landDataManager.MapLazyLoaderMan[landDataManager.ActiveLazyLoaderManIndex].Loader;
        if (!UpdateTrackedTarget(landDataManager))
        {
            return;
        }
        switch (landDataManager.LazyLoaderSubTask)
        {
            case SubTaskLoadMap:
                if (!landDataManager.MapQuadrantsToCancelLazyLoad[loader.Quadrants[loader.NextIndexToLoad]])
                {
                    landDataManager.MapLoadFns!.LazyLoad!(loader.NextIndexToLoad, landDataManager.AreaDataMan, landDataManager.MapMatrix, landDataManager.MapMatrixWidth, landDataManager.MapMatrixHeight, landDataManager, loader);
                }
                loader.NextIndexToLoad++;
                landDataManager.LazyLoaderSubTask = SubTaskFinishMapLoad;
                break;
            case SubTaskFinishMapLoad:
                FinishMapLoad(landDataManager, loader);
                break;
            case SubTaskIdle:
                break;
            case SubTaskGracefulShutdown:
                if (LandDataManager_LazyLoadFinished(loader.MapLoadTasksState))
                {
                    landDataManager.RunningLazyLoaderManCount = 0;
                }
                break;
        }
        AdvanceLazyLoaderManagers(landDataManager);
    }

    // OFFSET 0x021E8558
    private static void LandDataManager_QueueLazyLoadMapPair(int firstMapMatrixIndex, int secondMapMatrixIndex, byte firstQuadrantToLoad, byte secondQuadrantToLoad, byte trackedTargetDirection, LandDataManager landDataManager)
    {
        MapLazyLoaderPending pending = landDataManager.LazyLoaderPending;
        if (pending.Pending)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (LandDataManager_FindCancellableLazyLoaderManager(trackedTargetDirection, landDataManager, out byte managerIndex))
        {
            LandDataManager_KillLoadTasks(landDataManager, managerIndex);
            if (landDataManager.RunningLazyLoaderManCount >= LazyLoaderManagerCount)
            {
                pending.Pending = true;
                pending.MapMatrixIndexes[0] = firstMapMatrixIndex;
                pending.MapMatrixIndexes[1] = secondMapMatrixIndex;
                pending.Quadrants[0] = firstQuadrantToLoad;
                pending.Quadrants[1] = secondQuadrantToLoad;
                pending.LoadSlots[0] = landDataManager.LoadedMaps[firstQuadrantToLoad];
                pending.LoadSlots[1] = landDataManager.LoadedMaps[secondQuadrantToLoad];
                pending.TrackedTargetDirection = trackedTargetDirection;
                landDataManager.LoadedMaps[firstQuadrantToLoad]!.MapMatrixIndex = firstMapMatrixIndex;
                landDataManager.LoadedMaps[secondQuadrantToLoad]!.MapMatrixIndex = secondMapMatrixIndex;
                return;
            }
            LandDataManager_SetMapsToLazyLoaderManager(landDataManager, firstMapMatrixIndex, secondMapMatrixIndex, firstQuadrantToLoad, secondQuadrantToLoad, landDataManager.NextLazyLoaderManIndex, trackedTargetDirection);
            return;
        }
        if (!LandDataManager_SetMapsToLazyLoaderManager(landDataManager, firstMapMatrixIndex, secondMapMatrixIndex, firstQuadrantToLoad, secondQuadrantToLoad, landDataManager.NextLazyLoaderManIndex, trackedTargetDirection))
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021E8614
    private static void LandDataManager_ResetLoadedMap(byte index, LandDataManager landDataManager)
    {
        LoadedMap map = landDataManager.LoadedMaps[index]!;
        map.Valid = false;
        BDHC.BDHC_Reset(map.Bdhc);
        if (map.MapPropManager != null)
        {
            MapProps.MapPropManager_Init(map.MapPropManager);
        }
        map.MapMatrixIndex = -1;
        ClearTerrainAttributes(map);
        landDataManager.MapQuadrantsToCancelLazyLoad[index] = true;
    }

    // OFFSET 0x021E8668
    private static void LandDataManager_PrepareLoadingNextMapPair(byte firstResetLoadedMap, byte secondResetLoadedMap, byte firstKeptLoadedMap, byte secondKeptLoadedMap, LandDataManager landDataManager)
    {
        LandDataManager_ResetLoadedMap(firstResetLoadedMap, landDataManager);
        LandDataManager_ResetLoadedMap(secondResetLoadedMap, landDataManager);
        LoadedMap? firstReset = landDataManager.LoadedMaps[firstResetLoadedMap];
        LoadedMap? secondReset = landDataManager.LoadedMaps[secondResetLoadedMap];
        landDataManager.LoadedMaps[firstResetLoadedMap] = landDataManager.LoadedMaps[firstKeptLoadedMap];
        landDataManager.LoadedMaps[secondResetLoadedMap] = landDataManager.LoadedMaps[secondKeptLoadedMap];
        landDataManager.LoadedMaps[firstKeptLoadedMap] = firstReset;
        landDataManager.LoadedMaps[secondKeptLoadedMap] = secondReset;
    }

    // OFFSET 0x021E86A0
    private static void LandDataManager_QueueLazyLoadNextMapPair(int trackedTargetMapMatrixIndex, byte trackedTargetDirection, LandDataManager landDataManager)
    {
        int width = landDataManager.MapMatrixWidth;
        int mapCount = width * landDataManager.MapMatrixHeight;
        byte mapQuadrant = landDataManager.TrackedTargetMapQuadrant;
        int first = -1;
        int second = -1;
        byte firstQuadrant = 0;
        byte secondQuadrant = 0;
        switch (trackedTargetDirection)
        {
            case DirectionNorth:
                LandDataManager_PrepareLoadingNextMapPair(2, 3, 0, 1, landDataManager);
                if (mapQuadrant == 2)
                {
                    first = trackedTargetMapMatrixIndex - width;
                    firstQuadrant = 1;
                    secondQuadrant = 0;
                    second = first - 1;
                    if (first < 0)
                    {
                        first = -1;
                        second = -1;
                    }
                    else if (second < 0 || !LandData_AreMapsInSameLine(first, second, width))
                    {
                        second = -1;
                    }
                }
                else if (mapQuadrant == 3)
                {
                    first = trackedTargetMapMatrixIndex - width;
                    firstQuadrant = 0;
                    secondQuadrant = 1;
                    second = first + 1;
                    if (first < 0)
                    {
                        first = -1;
                        second = -1;
                    }
                    else if (!LandData_AreMapsInSameLine(first, second, width))
                    {
                        second = -1;
                    }
                }
                else
                {
                    ErrorHandling.GF_AssertFail();
                }
                break;
            case DirectionWest:
                LandDataManager_PrepareLoadingNextMapPair(1, 3, 0, 2, landDataManager);
                if (mapQuadrant == 1)
                {
                    first = trackedTargetMapMatrixIndex - 1;
                    second = trackedTargetMapMatrixIndex - width - 1;
                    firstQuadrant = 2;
                    secondQuadrant = 0;
                    if (first < 0 || !LandData_AreMapsInSameLine(first, trackedTargetMapMatrixIndex, width))
                    {
                        first = -1;
                        second = -1;
                    }
                    if (second < 0 || !LandData_AreMapsInSameColumn(first, second, width))
                    {
                        second = -1;
                    }
                }
                else if (mapQuadrant == 3)
                {
                    first = trackedTargetMapMatrixIndex - 1;
                    second = trackedTargetMapMatrixIndex + width - 1;
                    firstQuadrant = 0;
                    secondQuadrant = 2;
                    if (first < 0 || !LandData_AreMapsInSameLine(first, trackedTargetMapMatrixIndex, width))
                    {
                        first = -1;
                        second = -1;
                    }
                    if (mapCount <= second || !LandData_AreMapsInSameColumn(first, second, width))
                    {
                        second = -1;
                    }
                }
                else
                {
                    ErrorHandling.GF_AssertFail();
                }
                break;
            case DirectionEast:
                LandDataManager_PrepareLoadingNextMapPair(0, 2, 1, 3, landDataManager);
                if (mapQuadrant == 0)
                {
                    first = trackedTargetMapMatrixIndex + 1;
                    second = trackedTargetMapMatrixIndex - width + 1;
                    firstQuadrant = 3;
                    secondQuadrant = 1;
                    if (mapCount <= first || !LandData_AreMapsInSameLine(first, trackedTargetMapMatrixIndex, width))
                    {
                        first = -1;
                        second = -1;
                    }
                    if (mapCount <= second || !LandData_AreMapsInSameColumn(first, second, width))
                    {
                        second = -1;
                    }
                }
                else if (mapQuadrant == 2)
                {
                    first = trackedTargetMapMatrixIndex + 1;
                    second = trackedTargetMapMatrixIndex + width + 1;
                    firstQuadrant = 1;
                    secondQuadrant = 3;
                    if (mapCount <= first || !LandData_AreMapsInSameLine(first, trackedTargetMapMatrixIndex, width))
                    {
                        first = -1;
                        second = -1;
                    }
                    if (second < 0 || !LandData_AreMapsInSameColumn(first, second, width))
                    {
                        second = -1;
                    }
                }
                break;
            case DirectionSouth:
                LandDataManager_PrepareLoadingNextMapPair(0, 1, 2, 3, landDataManager);
                if (mapQuadrant == 0)
                {
                    firstQuadrant = 3;
                    secondQuadrant = 2;
                    first = trackedTargetMapMatrixIndex + width;
                    second = first - 1;
                    if (mapCount <= first)
                    {
                        first = -1;
                        second = -1;
                    }
                    else if (!LandData_AreMapsInSameLine(first, second, width))
                    {
                        second = -1;
                    }
                }
                else if (mapQuadrant == 1)
                {
                    firstQuadrant = 2;
                    secondQuadrant = 3;
                    first = trackedTargetMapMatrixIndex + width;
                    second = first + 1;
                    if (mapCount <= first)
                    {
                        first = -1;
                        second = -1;
                    }
                    else if (mapCount <= second || !LandData_AreMapsInSameLine(first, second, width))
                    {
                        second = -1;
                    }
                }
                else
                {
                    ErrorHandling.GF_AssertFail();
                }
                break;
            default:
                ErrorHandling.GF_AssertFail();
                return;
        }
        LandDataManager_QueueLazyLoadMapPair(first, second, firstQuadrant, secondQuadrant, trackedTargetDirection, landDataManager);
    }

    // OFFSET 0x021E89D0
    private static void LandDataManager_RenderLoadedMap(byte index, LandDataManager landDataManager, ModelAttributes? modelAttributes)
    {
        VecFx32 position = default;
        LoadedMap map = landDataManager.LoadedMaps[index]!;
        LandDataManager_CalculateRenderingPosition(map.MapMatrixIndex, landDataManager.MapMatrixWidth, landDataManager.MapMatrix, ref position);
        position.X += landDataManager.Offset.X;
        position.Y += landDataManager.Offset.Y;
        position.Z += landDataManager.Offset.Z;
        if (map.Valid)
        {
            VecFx32 scale = new() { X = Fx32.One, Y = Fx32.One, Z = Fx32.One };
            MtxFx33 rotation = IdentityRotation();
            Easy3D.Easy3D_DrawRenderObj(map.MapRenderObj, position, rotation, scale);
        }
        if (map.Valid && map.MapPropManager != null)
        {
            bool isOutdoorsLighting = AreaData.AreaDataManager_IsOutdoorsLighting(landDataManager.AreaDataMan!);
            MapProps.MapPropManager_Render(position, landDataManager.AreaDataMan!, isOutdoorsLighting ? 1 : 0, modelAttributes, map.MapPropManager);
        }
    }

    // OFFSET 0x021E8A98
    private static int LandData_CalculateMapMatrixIndex(uint tileIndex, int mapMatrixWidth, int mapMatrixWidthTiles)
    {
        uint x = tileIndex % (uint)mapMatrixWidthTiles;
        uint z = tileIndex / (uint)mapMatrixWidthTiles;
        return (int)((x >> 5) + ((z >> 5) * (uint)mapMatrixWidth));
    }

    // OFFSET 0x021E8ABC
    private static int LandData_CalculateTileIndex(int tileX, int tileZ, int offsetTileX, int offsetTileZ, int mapMatrixWidthTiles)
    {
        return (tileX - offsetTileX) + ((tileZ - offsetTileZ) * mapMatrixWidthTiles);
    }

    // OFFSET 0x021E8ACC
    private static uint LandData_CalculateMapMatrixIndexU32(uint tileIndex, int mapMatrixWidth, int mapMatrixWidthTiles)
    {
        uint x = tileIndex % (uint)mapMatrixWidthTiles;
        uint z = tileIndex / (uint)mapMatrixWidthTiles;
        return (x >> 5) + ((z >> 5) * (uint)mapMatrixWidth);
    }

    // OFFSET 0x021E8AF0
    private static byte LandData_GetOppositeQuadrant(byte quadrant)
    {
        if (quadrant == 0)
        {
            return 3;
        }
        if (quadrant == 1)
        {
            return 2;
        }
        if (quadrant == 2)
        {
            return 1;
        }
        if (quadrant == 3)
        {
            return 0;
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x021E8B1C
    private static uint LandData_CalculateNextTileIndex(byte trackedTargetDirection, int mapMatrixHeight, int mapMatrixWidthTiles, int trackedTargetTileIndex)
    {
        switch (trackedTargetDirection)
        {
            case DirectionEast:
                if ((trackedTargetTileIndex % mapMatrixWidthTiles) + 1 >= mapMatrixWidthTiles)
                {
                    return (uint)trackedTargetTileIndex;
                }
                return (uint)(trackedTargetTileIndex + 1);
            case DirectionSouth:
                if (trackedTargetTileIndex + mapMatrixWidthTiles < (mapMatrixHeight * MapTiles) * mapMatrixWidthTiles)
                {
                    return (uint)(trackedTargetTileIndex + mapMatrixWidthTiles);
                }
                return (uint)trackedTargetTileIndex;
            case DirectionWest:
                if ((trackedTargetTileIndex % mapMatrixWidthTiles) - 1 < 0)
                {
                    return (uint)trackedTargetTileIndex;
                }
                return (uint)(trackedTargetTileIndex - 1);
            case DirectionNorth:
                if (trackedTargetTileIndex - mapMatrixWidthTiles >= 0)
                {
                    return (uint)(trackedTargetTileIndex - mapMatrixWidthTiles);
                }
                return (uint)trackedTargetTileIndex;
            default:
                ErrorHandling.GF_AssertFail();
                return 0;
        }
    }

    // OFFSET 0x021E8B84
    private static byte LandData_CalculateNextQuadrant(byte direction, int quadrant)
    {
        switch (direction)
        {
            case DirectionEast:
                return (byte)(quadrant + 1);
            case DirectionSouth:
                return (byte)(quadrant + 2);
            case DirectionWest:
                return (byte)(quadrant - 1);
            case DirectionNorth:
                return (byte)(quadrant - 2);
            default:
                return direction;
        }
    }

    // OFFSET 0x021E8BC0
    private static void LandData_CalculateNextOppositeQuadrant(byte direction, int quadrant, ref byte oppositeQuadrant)
    {
        byte result = oppositeQuadrant;
        int nextQuadrant = LandData_CalculateNextQuadrant(direction, quadrant);
        if (quadrant < nextQuadrant)
        {
            int difference = nextQuadrant - quadrant;
            if (difference == 1)
            {
                result = (byte)(oppositeQuadrant - 1);
            }
            else if (difference == 2)
            {
                result = (byte)(oppositeQuadrant - 2);
            }
            else
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        else if (quadrant > nextQuadrant)
        {
            int difference = quadrant - nextQuadrant;
            if (difference == 1)
            {
                result = (byte)(oppositeQuadrant + 1);
            }
            else if (difference == 2)
            {
                result = (byte)(oppositeQuadrant + 2);
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
        oppositeQuadrant = result;
    }

    // OFFSET 0x021E8C24
    private static byte LandData_GetOppositeDirection(byte direction)
    {
        switch (direction)
        {
            case DirectionEast:
                return DirectionWest;
            case DirectionSouth:
                return DirectionNorth;
            case DirectionWest:
                return DirectionEast;
            case DirectionNorth:
                return DirectionSouth;
            default:
                ErrorHandling.GF_AssertFail();
                return DirectionInvalid;
        }
    }

    // OFFSET 0x021E8C58
    private static bool LandDataManager_FindCancellableLazyLoaderManager(byte trackedTargetDirection, LandDataManager landDataManager, out byte managerIndex)
    {
        managerIndex = 0;
        for (int i = 0; i < LazyLoaderManagerCount; i++)
        {
            MapLazyLoaderManager manager = landDataManager.MapLazyLoaderMan[i];
            if (manager.Loading && manager.LoadDirection == LandData_GetOppositeDirection(trackedTargetDirection))
            {
                managerIndex = (byte)i;
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x021E8C8C
    private static bool LandDataManager_SetPendingMapsToNextLazyLoaderManager(LandDataManager landDataManager)
    {
        MapLazyLoaderPending pending = landDataManager.LazyLoaderPending;
        MapLazyLoaderManager manager = landDataManager.MapLazyLoaderMan[landDataManager.NextLazyLoaderManIndex];
        if (manager.Loading)
        {
            ErrorHandling.GF_AssertFail();
        }
        manager.Loading = true;
        manager.LoadDirection = pending.TrackedTargetDirection;
        manager.Loader.MapMatrixIndexes[0] = pending.MapMatrixIndexes[0];
        manager.Loader.MapMatrixIndexes[1] = pending.MapMatrixIndexes[1];
        manager.Loader.Quadrants[0] = pending.Quadrants[0];
        manager.Loader.Quadrants[1] = pending.Quadrants[1];
        manager.Loader.LoadSlots[0] = pending.LoadSlots[0];
        manager.Loader.LoadSlots[1] = pending.LoadSlots[1];
        manager.Loader.NextIndexToLoad = 0;
        manager.Loader.Available = false;
        return true;
    }

    // OFFSET 0x021E8D50
    private static void LandDataManager_QueueLazyLoadPendingMaps(LandDataManager landDataManager)
    {
        MapLazyLoaderPending pending = landDataManager.LazyLoaderPending;
        if (landDataManager.RunningLazyLoaderManCount >= LazyLoaderManagerCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!LandDataManager_SetPendingMapsToNextLazyLoaderManager(landDataManager))
        {
            ErrorHandling.GF_AssertFail();
        }
        pending.LoadSlots[0]!.MapMatrixIndex = pending.MapMatrixIndexes[0];
        pending.LoadSlots[1]!.MapMatrixIndex = pending.MapMatrixIndexes[1];
        landDataManager.RunningLazyLoaderManCount++;
        landDataManager.NextLazyLoaderManIndex = (byte)((landDataManager.NextLazyLoaderManIndex + 1) % LazyLoaderManagerCount);
    }

    // OFFSET 0x021E8DAC
    private static bool LandDataManager_IsActiveLazyLoaderAvailable(LandDataManager landDataManager)
    {
        return landDataManager.MapLazyLoaderMan[landDataManager.ActiveLazyLoaderManIndex].Loader.Available;
    }

    // OFFSET 0x021E8DBC
    private static void LandDataManager_ResetMapQuadrantsToCancelLazyLoad(LandDataManager landDataManager)
    {
        for (byte i = 0; i < QuadrantCount; i++)
        {
            landDataManager.MapQuadrantsToCancelLazyLoad[i] = false;
        }
    }

    // OFFSET 0x021E8DD4
    private static void LandDataManager_CancelMapQuadrantsToLazyLoad(byte trackedTargetDirection, LandDataManager landDataManager)
    {
        switch (trackedTargetDirection)
        {
            case DirectionNorth:
                landDataManager.MapQuadrantsToCancelLazyLoad[2] = true;
                landDataManager.MapQuadrantsToCancelLazyLoad[3] = true;
                break;
            case DirectionEast:
                landDataManager.MapQuadrantsToCancelLazyLoad[0] = true;
                landDataManager.MapQuadrantsToCancelLazyLoad[2] = true;
                break;
            case DirectionSouth:
                landDataManager.MapQuadrantsToCancelLazyLoad[0] = true;
                landDataManager.MapQuadrantsToCancelLazyLoad[1] = true;
                break;
            case DirectionWest:
                landDataManager.MapQuadrantsToCancelLazyLoad[1] = true;
                landDataManager.MapQuadrantsToCancelLazyLoad[3] = true;
                break;
        }
    }

    // OFFSET 0x021E8E28
    private static void LandDataManager_InitAndLoad(LandDataManager landDataManager, int trackedTargetTileX, int trackedTargetTileZ, int offsetTileX, int offsetTileZ, int mapMatrixWidthTiles)
    {
        int[] loadedMapMatrixIndexes = new int[QuadrantCount];
        LandDataManager_NewLoadedMaps(landDataManager);
        LandData_CalculateLoadedMapMatrixIndexes(landDataManager.InDistortionWorld, trackedTargetTileX, trackedTargetTileZ, offsetTileX, offsetTileZ, landDataManager.MapMatrixWidth, landDataManager.MapMatrixHeight, mapMatrixWidthTiles, loadedMapMatrixIndexes);
        for (byte i = 0; i < QuadrantCount; i++)
        {
            LoadedMap map = landDataManager.LoadedMaps[i]!;
            map.Bdhc = BDHC.BDHC_New();
            BDHC.BDHC_MarkNotLoaded(map.Bdhc);
            bool isOutdoorsLighting = AreaData.AreaDataManager_IsOutdoorsLighting(landDataManager.AreaDataMan!);
            LandDataManager_Load(loadedMapMatrixIndexes[i], i, landDataManager.AreaDataMan, landDataManager.MapMatrix, landDataManager.MapMatrixWidth, landDataManager.MapMatrixHeight, isOutdoorsLighting, landDataManager);
        }
    }

    // OFFSET 0x021E8ECC
    private static void LandDataManager_InitAndLoadWithoutAttributes(LandDataManager landDataManager, int trackedTargetTileX, int trackedTargetTileZ, int offsetTileX, int offsetTileZ, int mapMatrixWidthTiles)
    {
        int[] loadedMapMatrixIndexes = new int[QuadrantCount];
        LandDataManager_NewLoadedMapsWithoutAttributes(landDataManager);
        LandData_CalculateLoadedMapMatrixIndexes(landDataManager.InDistortionWorld, trackedTargetTileX, trackedTargetTileZ, offsetTileX, offsetTileZ, landDataManager.MapMatrixWidth, landDataManager.MapMatrixHeight, mapMatrixWidthTiles, loadedMapMatrixIndexes);
        for (byte i = 0; i < QuadrantCount; i++)
        {
            landDataManager.LoadedMaps[i]!.Bdhc = null;
            bool isOutdoorsLighting = AreaData.AreaDataManager_IsOutdoorsLighting(landDataManager.AreaDataMan!);
            LandDataManager_LoadWithoutAttributes(loadedMapMatrixIndexes[i], i, landDataManager.AreaDataMan, landDataManager.MapMatrix, landDataManager.MapMatrixWidth, landDataManager.MapMatrixHeight, isOutdoorsLighting, landDataManager);
        }
    }

    // OFFSET 0x021E8F60
    private static void LandDataManager_InitMapLazyLoader(LandDataManager landDataManager)
    {
        landDataManager.ActiveLazyLoaderManIndex = 0;
        landDataManager.NextLazyLoaderManIndex = 0;
        landDataManager.RunningLazyLoaderManCount = 0;
        for (int i = 0; i < LazyLoaderManagerCount; i++)
        {
            landDataManager.MapLazyLoaderMan[i].Loading = false;
            landDataManager.MapLazyLoaderMan[i].Loader.Available = true;
        }
        LandDataManager_ResetMapQuadrantsToCancelLazyLoad(landDataManager);
    }

    // OFFSET 0x021E8F90
    private static void LandDataManager_UpdateTrackedTargetValues(int trackedTargetTileX, int trackedTargetTileZ, int offsetTileX, int offsetTileZ, LandDataManager landDataManager)
    {
        if (landDataManager.InDistortionWorld == 1)
        {
            if (landDataManager.MapMatrixWidth > 1)
            {
                trackedTargetTileX = offsetTileX + (MapTiles - 1);
            }
            else
            {
                trackedTargetTileX = offsetTileX + HalfMapTiles;
            }
            if (landDataManager.MapMatrixHeight > 1)
            {
                trackedTargetTileZ = offsetTileZ + (MapTiles - 1);
            }
            else
            {
                trackedTargetTileZ = offsetTileZ + HalfMapTiles;
            }
        }
        landDataManager.TrackedTargetTileIndex = LandData_CalculateTileIndex(trackedTargetTileX, trackedTargetTileZ, offsetTileX, offsetTileZ, landDataManager.MapMatrixWidthTiles);
        landDataManager.TrackedTargetMapQuadrant = LandDataManager_CalculateMapQuadrantOfTile((uint)landDataManager.TrackedTargetTileIndex, landDataManager.MapMatrixWidthTiles);
        landDataManager.TrackedTargetLoadedMapsQuadrant = LandData_GetOppositeQuadrant(landDataManager.TrackedTargetMapQuadrant);
        landDataManager.TrackedTargetMapMatrixIndex = (int)LandData_CalculateMapMatrixIndexU32((uint)landDataManager.TrackedTargetTileIndex, landDataManager.MapMatrixWidth, landDataManager.MapMatrixWidthTiles);
    }

    // OFFSET 0x021E901C
    private static void LandDataManager_CalculateRenderingPosition(int mapMatrixIndex, int mapMatrixWidth, MapMatrix? mapMatrix, ref VecFx32 position)
    {
        position.X = MapCenter;
        position.Z = MapCenter;
        if (mapMatrixIndex == -1)
        {
            return;
        }
        ushort mapX = (ushort)(mapMatrixIndex % mapMatrixWidth);
        ushort mapZ = (ushort)(mapMatrixIndex / mapMatrixWidth);
        int width = MapMatrix.MapMatrix_GetWidth(mapMatrix);
        int matrixID = MapMatrix.MapMatrix_GetMatrixID(mapMatrix!);
        int altitude = MapMatrix.MapMatrix_GetAltitudeAtCoords(mapMatrix!, matrixID, mapX, mapZ, width);
        position.Y = altitude << AltitudeShift;
        position.X += mapX << MapSizeShift;
        position.Z += mapZ << MapSizeShift;
    }

    // OFFSET 0x021E9084
    public static LandDataManager LandDataManager_New(MapMatrix? mapMatrix, AreaDataManager? areaDataManager, MapPropAnimationManager? mapPropAnimationManager, int skipAttributes)
    {
        LandDataManager landDataManager = new();
        bool allocBDHCFiles = false;
        if (skipAttributes == 0)
        {
            landDataManager.MapLoadFns = sMapLoadFns;
            allocBDHCFiles = true;
        }
        else if (skipAttributes == 1)
        {
            landDataManager.MapLoadFns = sMapLoadWithoutAttributesFns;
            allocBDHCFiles = false;
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        landDataManager.LoadedMapBufs = LoadedMapBuffers.LoadedMapBuffers_New(allocBDHCFiles);
        landDataManager.AreaDataMan = areaDataManager;
        landDataManager.MapMatrix = mapMatrix;
        landDataManager.MapMatrixWidth = MapMatrix.MapMatrix_GetWidth(mapMatrix);
        landDataManager.MapMatrixHeight = MapMatrix.MapMatrix_GetHeight(mapMatrix);
        landDataManager.MapMatrixWidthTiles = landDataManager.MapMatrixWidth * MapTiles;
        landDataManager.MapPropAnimMan = mapPropAnimationManager;
        landDataManager.LazyLoaderTrackedTarget.Tracking = true;
        LandDataManager_InitMapLazyLoader(landDataManager);
        landDataManager.Ending = false;
        landDataManager.LazyLoaderSubTask = SubTaskIdle;
        landDataManager.LandDataNARC = NARC.NARC_ctor(LandDataNarcID, FieldHeapID);
        landDataManager.MapLoadedCb = null;
        return landDataManager;
    }

    // OFFSET 0x021E9150
    public static void LandDataManager_InitialLoad(LandDataManager landDataManager, int playerTileX, int playerTileZ)
    {
        landDataManager.MapLoadFns!.InitialLoad!(landDataManager, playerTileX, playerTileZ, landDataManager.OffsetTileX, landDataManager.OffsetTileZ, landDataManager.MapMatrixWidthTiles);
        LandDataManager_UpdateTrackedTargetValues(playerTileX, playerTileZ, landDataManager.OffsetTileX, landDataManager.OffsetTileZ, landDataManager);
    }

    // OFFSET 0x021E9190
    public static void LandDataManager_CheckLazyLoadNextMapPair(byte trackedTargetDirection, LandDataManager landDataManager)
    {
        uint nextTileIndex = LandData_CalculateNextTileIndex(trackedTargetDirection, landDataManager.MapMatrixHeight, landDataManager.MapMatrixWidthTiles, landDataManager.TrackedTargetTileIndex);
        int nextMapMatrixIndex = LandData_CalculateMapMatrixIndex(nextTileIndex, landDataManager.MapMatrixWidth, landDataManager.MapMatrixWidthTiles);
        byte nextMapQuadrant = LandDataManager_CalculateMapQuadrantOfTile(nextTileIndex, landDataManager.MapMatrixWidthTiles);
        if (landDataManager.TrackedTargetMapMatrixIndex == nextMapMatrixIndex && landDataManager.TrackedTargetMapQuadrant != nextMapQuadrant)
        {
            LandDataManager_QueueLazyLoadNextMapPair(nextMapMatrixIndex, trackedTargetDirection, landDataManager);
            LandData_CalculateNextOppositeQuadrant(trackedTargetDirection, landDataManager.TrackedTargetMapQuadrant, ref landDataManager.TrackedTargetLoadedMapsQuadrant);
        }
    }

    // OFFSET 0x021E91FC
    public static void LandDataManager_RenderLoadedMaps(LandDataManager landDataManager, ModelAttributes? modelAttributes)
    {
        for (byte i = 0; i < QuadrantCount; i++)
        {
            if (landDataManager.LoadedMaps[i]!.MapMatrixIndex != -1)
            {
                LandDataManager_RenderLoadedMap(i, landDataManager, modelAttributes);
            }
        }
    }

    // OFFSET 0x021E9230
    public static void LandData_ObjectPosToTilePos(int objectPositionX, int objectPositionZ, out int tileX, out int tileZ)
    {
        tileX = objectPositionX / TileSize;
        tileZ = objectPositionZ / TileSize;
    }

    // OFFSET 0x021E924C
    public static void LandDataManager_End(LandDataManager landDataManager)
    {
        GameSystem.ClearUnusedSystemCache();
        NnsGfd.ResetLnkTexVramState();
        NnsGfd.ResetLnkPlttVramState();
        for (byte i = 0; i < LazyLoaderManagerCount; i++)
        {
            MapLazyLoader_KillTasks(landDataManager.MapLazyLoaderMan[i].Loader);
        }
        for (byte i = 0; i < QuadrantCount; i++)
        {
            LoadedMap map = landDataManager.LoadedMaps[i]!;
            map.Valid = false;
            BDHC.BDHC_Free(map.Bdhc);
            if (map.MapPropManager != null)
            {
                MapProps.MapPropManager_Free(map.MapPropManager);
            }
            landDataManager.LoadedMaps[i] = null;
        }
        landDataManager.LazyLoaderSubTask = SubTaskGracefulShutdown;
        landDataManager.Ending = true;
        landDataManager.LazyLoaderTrackedTarget.Tracking = false;
    }

    // OFFSET 0x021E92E4
    public static void LandDataManager_FreeNARCAndLoadedMapBuffers(LandDataManager landDataManager)
    {
        NARC.NARC_dtor(landDataManager.LandDataNARC!);
        LoadedMapBuffers.LoadedMapBuffers_Free(landDataManager.LoadedMapBufs!);
    }

    // OFFSET 0x021E9300
    public static bool LandDataManager_HasEnded(LandDataManager landDataManager)
    {
        if (landDataManager.Ending && landDataManager.RunningLazyLoaderManCount == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021E931C
    public static void LandDataManager_TrackTarget(Func<VecFx32> position, LandDataManager landDataManager)
    {
        landDataManager.LazyLoaderTrackedTarget.PrevPosition = position();
        landDataManager.LazyLoaderTrackedTarget.Position = position;
    }

    // OFFSET 0x021E9338
    public static void LandDataManager_ForgetTrackedTarget(LandDataManager landDataManager)
    {
        landDataManager.LazyLoaderTrackedTarget.Position = null;
    }

    // OFFSET 0x021E9340
    public static void LandDataManager_GetLoadedMapPropManager(byte index, LandDataManager landDataManager, out MapPropManager? mapPropManager)
    {
        mapPropManager = landDataManager.LoadedMaps[index]!.MapPropManager;
    }

    // OFFSET 0x021E9354
    public static byte LandDataManager_GetTrackedTargetLoadedMapsQuadrant(LandDataManager landDataManager)
    {
        return landDataManager.TrackedTargetLoadedMapsQuadrant;
    }

    // OFFSET 0x021E935C
    public static byte LandDataManager_CalculateMapQuadrantOfTile(uint tileIndex, int mapMatrixWidthTiles)
    {
        int tileZ = (int)(tileIndex / (uint)mapMatrixWidthTiles) % MapTiles;
        int tileX = (int)(tileIndex & (MapTiles - 1));
        if (tileX < HalfMapTiles)
        {
            if (tileZ < HalfMapTiles)
            {
                return 0;
            }
            return 2;
        }
        if (tileZ < HalfMapTiles)
        {
            return 1;
        }
        return 3;
    }

    // OFFSET 0x021E9390
    public static byte LandDataManager_GetRelativeLoadedMapsQuadrant(int mapMatrixIndex, byte unused, LandDataManager landDataManager)
    {
        if (landDataManager.TrackedTargetMapMatrixIndex == mapMatrixIndex)
        {
            return landDataManager.TrackedTargetLoadedMapsQuadrant;
        }
        if (mapMatrixIndex == -1)
        {
            ErrorHandling.GF_AssertFail();
        }
        int quadrant = landDataManager.TrackedTargetLoadedMapsQuadrant;
        int trackedIndex = landDataManager.TrackedTargetMapMatrixIndex;
        int width = landDataManager.MapMatrixWidth;
        switch (quadrant)
        {
            case 0:
                if (width == mapMatrixIndex - trackedIndex)
                {
                    return (byte)(quadrant + 2);
                }
                if (mapMatrixIndex - trackedIndex == 1 && LandData_AreMapsInSameLine(trackedIndex, mapMatrixIndex, width))
                {
                    return (byte)(quadrant + 1);
                }
                if (width + 1 == mapMatrixIndex - trackedIndex && !LandData_AreMapsInSameLine(trackedIndex, mapMatrixIndex, width))
                {
                    return (byte)(quadrant + 3);
                }
                return InvalidQuadrant;
            case 1:
                if (width == mapMatrixIndex - trackedIndex)
                {
                    return (byte)(quadrant + 2);
                }
                if (trackedIndex - mapMatrixIndex == 1 && LandData_AreMapsInSameLine(trackedIndex, mapMatrixIndex, width))
                {
                    return (byte)(quadrant - 1);
                }
                if (width - 1 == mapMatrixIndex - trackedIndex && !LandData_AreMapsInSameLine(trackedIndex, mapMatrixIndex, width))
                {
                    return (byte)(quadrant + 1);
                }
                return InvalidQuadrant;
            case 2:
                if (width == trackedIndex - mapMatrixIndex)
                {
                    return (byte)(quadrant - 2);
                }
                if (width - 1 == trackedIndex - mapMatrixIndex && !LandData_AreMapsInSameLine(trackedIndex, mapMatrixIndex, width))
                {
                    return (byte)(quadrant - 1);
                }
                if (mapMatrixIndex - trackedIndex == 1 && LandData_AreMapsInSameLine(trackedIndex, mapMatrixIndex, width))
                {
                    return (byte)(quadrant + 1);
                }
                return InvalidQuadrant;
            case 3:
                if (width == trackedIndex - mapMatrixIndex)
                {
                    return (byte)(quadrant - 2);
                }
                if (width + 1 == trackedIndex - mapMatrixIndex && !LandData_AreMapsInSameLine(trackedIndex, mapMatrixIndex, width))
                {
                    return (byte)(quadrant - 3);
                }
                if (trackedIndex - mapMatrixIndex == 1 && LandData_AreMapsInSameLine(trackedIndex, mapMatrixIndex, width))
                {
                    return (byte)(quadrant - 1);
                }
                return InvalidQuadrant;
            default:
                return InvalidQuadrant;
        }
    }

    // OFFSET 0x021E9560
    public static int LandDataManager_GetLoadedMapMatrixIndex(LandDataManager landDataManager, byte loadedMapIndex)
    {
        if (loadedMapIndex >= QuadrantCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return landDataManager.LoadedMaps[loadedMapIndex]!.MapMatrixIndex;
    }

    // OFFSET 0x021E9580
    public static bool LandDataManager_GetRelativeLoadedMapsQuadrantOfTile(LandDataManager landDataManager, int tileX, int tileZ, out byte result)
    {
        result = 0;
        int x = tileX - landDataManager.OffsetTileX;
        int z = tileZ - landDataManager.OffsetTileZ;
        int width = landDataManager.MapMatrixWidth;
        uint mapMatrixIndex = (uint)((x / MapTiles) + ((z / MapTiles) * width));
        if (mapMatrixIndex >= (uint)(width * landDataManager.MapMatrixHeight))
        {
            if (CommManager.CommMan_SetErrorState(1) != 0)
            {
                return false;
            }
        }
        if (mapMatrixIndex >= (uint)(landDataManager.MapMatrixWidth * landDataManager.MapMatrixHeight))
        {
            ErrorHandling.GF_AssertFail();
        }
        byte mapQuadrant = LandDataManager_CalculateMapQuadrantOfTile((uint)(x + (z * landDataManager.MapMatrixWidthTiles)), landDataManager.MapMatrixWidthTiles);
        byte quadrant = LandDataManager_GetRelativeLoadedMapsQuadrant((int)mapMatrixIndex, mapQuadrant, landDataManager);
        result = quadrant;
        if (quadrant > 3)
        {
            return false;
        }
        return true;
    }

    // OFFSET 0x021E9610
    public static BDHC? LandDataManager_GetLoadedMapBDHC(LandDataManager landDataManager, byte loadedMapIndex)
    {
        return landDataManager.LoadedMaps[loadedMapIndex]!.Bdhc;
    }

    // OFFSET 0x021E9624
    public static ushort[] LandDataManager_GetLoadedMapTerrainAttributes(LandDataManager landDataManager, byte loadedMapIndex)
    {
        return landDataManager.LoadedMaps[loadedMapIndex]!.TerrainAttributes;
    }

    // OFFSET 0x021E9630
    public static void LandDataManager_SetMapLoadedCallback(LandDataManager landDataManager, LandDataMapLoadedCallback? mapLoadedCallback, object? callbackUserData)
    {
        landDataManager.MapLoadedCb = mapLoadedCallback;
        landDataManager.MapLoadedCbUserData = callbackUserData;
    }

    // OFFSET 0x021E9640
    private static void LandDataManager_KillLoadMapModel(SysTask sysTask)
    {
        MapModelLoaderTaskContext context = (MapModelLoaderTaskContext)SysTaskManager.SysTask_GetParam(sysTask)!;
        context.KillLoadTask = true;
    }

    // OFFSET 0x021E964C
    private static void LandDataManager_LazyLoadMapModelTask(SysTask sysTask, object? sysTaskParam)
    {
        MapModelLoaderTaskContext context = (MapModelLoaderTaskContext)sysTaskParam!;
        if (context.KillLoadTask)
        {
            context.CurrentSubTask = ModelSubTaskEndTask;
        }
        switch (context.CurrentSubTask)
        {
            case ModelSubTaskFirstFileRead:
            {
                context.BytesRead = 0;
                int size = context.MapModelDataSize;
                if (size > MapModelDataChunkSize)
                {
                    size = MapModelDataChunkSize;
                    context.CurrentSubTask = ModelSubTaskFileRead;
                }
                else
                {
                    context.CurrentSubTask = ModelSubTaskBindTexture;
                }
                ReadInto(context.LandDataNARC!, size, context.Map!.MapModelFile!, context.BytesRead);
                context.BytesRead += size;
                break;
            }
            case ModelSubTaskFileRead:
            {
                int size = context.MapModelDataSize - context.BytesRead;
                bool finished;
                if (size > MapModelDataChunkSize)
                {
                    size = MapModelDataChunkSize;
                    finished = false;
                }
                else
                {
                    finished = true;
                }
                ReadInto(context.LandDataNARC!, size, context.Map!.MapModelFile!, context.BytesRead);
                if (finished)
                {
                    context.CurrentSubTask = ModelSubTaskBindTexture;
                }
                else
                {
                    context.BytesRead += size;
                }
                break;
            }
            case ModelSubTaskBindTexture:
                if (context.MapTexture != null)
                {
                    if (Easy3D.Easy3D_IsTextureUploadedToVRAM(context.MapTexture))
                    {
                        Easy3D.Easy3D_BindTextureToResource(context.Map!.MapModelFile!, context.MapTexture);
                    }
                }
                context.CurrentSubTask = ModelSubTaskInitRenderObj;
                break;
            case ModelSubTaskInitRenderObj:
            {
                if (GetModelCount(context.Map!.MapModelFile!) != 1)
                {
                    ErrorHandling.GF_AssertFail();
                }
                G3dModel? mapModel = GetFirstModel(context.Map.MapModelFile!);
                if (mapModel == null)
                {
                    ErrorHandling.GF_AssertFail();
                }
                G3d.InitRenderObj(context.Map.MapRenderObj, mapModel!);
                context.Map.Valid = true;
                context.CurrentSubTask = ModelSubTaskEndTask;
                break;
            }
            case ModelSubTaskEndTask:
                context.TasksState!.MapModelTaskRunning = 0;
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }

    // OFFSET 0x021E976C
    private static SysTask? LandDataManager_LazyLoadMapModel(NARC landDataNARC, int mapModelDataSize, LoadedMap map, G3dTexture? mapTexture, MapLoadTasksState tasksState)
    {
        MapModelLoaderTaskContext context = new();
        context.LandDataNARC = landDataNARC;
        context.MapModelDataSize = mapModelDataSize;
        context.Map = map;
        context.MapTexture = mapTexture;
        context.CurrentSubTask = ModelSubTaskFirstFileRead;
        map.Valid = false;
        context.TasksState = tasksState;
        context.KillLoadTask = false;
        return SysTask.SysTask_Start(LandDataManager_LazyLoadMapModelTask, context, TaskPriority);
    }

    // OFFSET 0x021E97AC
    private static G3dModel? LandDataManager_LoadMapModel(NARC landDataNARC, int mapModelFileSize, LoadedMap map, G3dTexture? mapTexture)
    {
        ReadInto(landDataNARC, mapModelFileSize, map.MapModelFile!, 0);
        if (mapTexture != null)
        {
            if (Easy3D.Easy3D_IsTextureUploadedToVRAM(mapTexture))
            {
                if (!Easy3D.Easy3D_BindTextureToResource(map.MapModelFile!, mapTexture))
                {
                    ErrorHandling.GF_AssertFail();
                }
            }
        }
        if (GetModelCount(map.MapModelFile!) != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        G3dModel? mapModel = GetFirstModel(map.MapModelFile!);
        if (mapModel == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        G3d.InitRenderObj(map.MapRenderObj, mapModel!);
        return mapModel;
    }

    // OFFSET 0x021E9828
    public static NARC? LandDataManager_GetLandDataNARC(LandDataManager landDataManager)
    {
        return landDataManager.LandDataNARC;
    }

    // OFFSET 0x021E9830
    public static LandDataManager LandDataManager_DistortionWorldNew(MapMatrix? mapMatrix, AreaDataManager? areaDataManager, NARC? landDataNARC)
    {
        LandDataManager landDataManager = new();
        landDataManager.LoadedMapBufs = LoadedMapBuffers.LoadedMapBuffers_NewWithHeapMapModelFiles(0);
        landDataManager.AreaDataMan = areaDataManager;
        landDataManager.MapMatrix = mapMatrix;
        if (mapMatrix != null)
        {
            landDataManager.MapMatrixWidth = MapMatrix.MapMatrix_GetWidth(mapMatrix);
            landDataManager.MapMatrixHeight = MapMatrix.MapMatrix_GetHeight(mapMatrix);
            landDataManager.MapMatrixWidthTiles = landDataManager.MapMatrixWidth * MapTiles;
        }
        landDataManager.LazyLoaderTrackedTarget.Tracking = false;
        LandDataManager_InitMapLazyLoader(landDataManager);
        landDataManager.Ending = false;
        landDataManager.LazyLoaderSubTask = SubTaskIdle;
        landDataManager.LandDataNARC = landDataNARC;
        if (landDataNARC == null)
        {
            landDataManager.LandDataNARC = NARC.NARC_ctor(LandDataNarcID, FieldHeapID);
        }
        return landDataManager;
    }

    // OFFSET 0x021E98C8
    public static void LandDataManager_DistortionWorldInit(LandDataManager landDataManager, MapMatrix? mapMatrix, AreaDataManager? areaDataManager, NARC? landDataNARC)
    {
        landDataManager.AreaDataMan = areaDataManager;
        landDataManager.MapMatrix = mapMatrix;
        landDataManager.MapMatrixWidth = MapMatrix.MapMatrix_GetWidth(mapMatrix);
        landDataManager.MapMatrixHeight = MapMatrix.MapMatrix_GetHeight(mapMatrix);
        landDataManager.MapMatrixWidthTiles = landDataManager.MapMatrixWidth * MapTiles;
        landDataManager.LazyLoaderTrackedTarget.Tracking = false;
        LandDataManager_InitMapLazyLoader(landDataManager);
        landDataManager.Ending = false;
        landDataManager.LazyLoaderSubTask = SubTaskIdle;
        landDataManager.LandDataNARC = landDataNARC;
        if (landDataNARC == null)
        {
            landDataManager.LandDataNARC = NARC.NARC_ctor(LandDataNarcID, FieldHeapID);
        }
    }

    // OFFSET 0x021E9938
    public static void LandDataManager_DistortionWorldEnd(LandDataManager landDataManager)
    {
        for (byte i = 0; i < QuadrantCount; i++)
        {
            LoadedMap map = landDataManager.LoadedMaps[i]!;
            map.Valid = false;
            if (map.MapPropManager != null)
            {
                MapProps.MapPropManager_Free(map.MapPropManager);
            }
            landDataManager.LoadedMaps[i] = null;
        }
        landDataManager.LazyLoaderSubTask = SubTaskGracefulShutdown;
        landDataManager.Ending = true;
        landDataManager.LazyLoaderTrackedTarget.Tracking = false;
    }

    // OFFSET 0x021E9998
    public static void LandDataManager_DistortionWorldInitLoadedMapPropManagers(LandDataManager landDataManager)
    {
        for (byte i = 0; i < QuadrantCount; i++)
        {
            MapPropManager? mapPropManager = landDataManager.LoadedMaps[i]!.MapPropManager;
            if (mapPropManager != null)
            {
                MapProps.MapPropManager_Init(mapPropManager);
            }
        }
    }

    // OFFSET 0x021E99C4
    public static void LandDataManager_DistortionWorldFreeLoadedMapBuffers(LandDataManager landDataManager)
    {
        LoadedMapBuffers.LoadedMapBuffers_Free(landDataManager.LoadedMapBufs!);
    }

    // OFFSET 0x021E99D8
    public static void LandDataManager_DistortionWorldInitialLoad(LandDataManager landDataManager, int playerTileX, int playerTileZ)
    {
        LandDataManager_DistortionWorldInitAndLoad(landDataManager, playerTileX, playerTileZ, landDataManager.OffsetTileX, landDataManager.OffsetTileZ, landDataManager.MapMatrixWidthTiles);
        LandDataManager_UpdateTrackedTargetValues(playerTileX, playerTileZ, landDataManager.OffsetTileX, landDataManager.OffsetTileZ, landDataManager);
    }

    // OFFSET 0x021E9A14
    private static void LandDataManager_DistortionWorldInitAndLoad(LandDataManager landDataManager, int trackedTargetTileX, int trackedTargetTileZ, int unusedOffsetTileX, int unusedOffsetTileZ, int mapMatrixWidthTiles)
    {
        int[] loadedMapMatrixIndexes = new int[QuadrantCount];
        LandDataManager_NewLoadedMapsWithoutAttributes(landDataManager);
        LandData_CalculateLoadedMapMatrixIndexes(landDataManager.InDistortionWorld, trackedTargetTileX, trackedTargetTileZ, landDataManager.OffsetTileX, landDataManager.OffsetTileZ, landDataManager.MapMatrixWidth, landDataManager.MapMatrixHeight, mapMatrixWidthTiles, loadedMapMatrixIndexes);
        for (byte i = 0; i < QuadrantCount; i++)
        {
            landDataManager.LoadedMaps[i]!.Bdhc = null;
            bool isOutdoorsLighting = AreaData.AreaDataManager_IsOutdoorsLighting(landDataManager.AreaDataMan!);
            LandDataManager_DistortionWorldLoad(loadedMapMatrixIndexes[i], i, landDataManager.AreaDataMan, landDataManager.MapMatrix, landDataManager.MapMatrixWidth, landDataManager.MapMatrixHeight, isOutdoorsLighting, landDataManager);
        }
    }

    // OFFSET 0x021E9AAC
    public static void LandDataManager_DistortionWorldInitLoadedMaps(LandDataManager landDataManager, int trackedTargetTileX, int trackedTargetTileZ, int[] loadedMapMatrixIndexes)
    {
        LandDataManager_InitLoadedMapsWithoutAttributes(landDataManager);
        LandData_CalculateLoadedMapMatrixIndexes(landDataManager.InDistortionWorld, trackedTargetTileX, trackedTargetTileZ, landDataManager.OffsetTileX, landDataManager.OffsetTileZ, landDataManager.MapMatrixWidth, landDataManager.MapMatrixHeight, landDataManager.MapMatrixWidthTiles, loadedMapMatrixIndexes);
        for (int i = 0; i < QuadrantCount; i++)
        {
            landDataManager.LoadedMaps[i]!.Valid = false;
        }
    }

    // OFFSET 0x021E9B10
    public static void LandDataManager_DistortionWorldLoadAndInvalidate(LandDataManager landDataManager, int loadedMapIndex, int mapMatrixIndex)
    {
        LoadedMap map = landDataManager.LoadedMaps[loadedMapIndex]!;
        map.Bdhc = null;
        bool isOutdoorsLighting = AreaData.AreaDataManager_IsOutdoorsLighting(landDataManager.AreaDataMan!);
        LandDataManager_DistortionWorldLoad(mapMatrixIndex, (byte)loadedMapIndex, landDataManager.AreaDataMan, landDataManager.MapMatrix, landDataManager.MapMatrixWidth, landDataManager.MapMatrixHeight, isOutdoorsLighting, landDataManager);
        landDataManager.LoadedMaps[loadedMapIndex]!.Valid = false;
    }

    // OFFSET 0x021E9B70
    private static void LandDataManager_DistortionWorldLoad(int mapMatrixIndex, byte loadedMapIndex, AreaDataManager? areaDataManager, MapMatrix? mapMatrix, int mapMatrixWidth, int mapMatrixHeight, bool isOutdoorsLighting, LandDataManager landDataManager)
    {
        if (mapMatrixIndex < 0 || mapMatrixIndex >= mapMatrixWidth * mapMatrixHeight)
        {
            return;
        }
        int landDataID = MapMatrix.MapMatrix_GetLandDataIDByIndex(mapMatrixIndex, mapMatrix);
        LandDataHeader landDataHeader = new();
        LandDataHeader_Load(landDataManager.LandDataNARC, landDataID, landDataHeader);
        NARC.NARC_Seek(landDataManager.LandDataNARC!, (uint)(landDataHeader.MapPropsSize + TerrainAttributesSize));
        LoadedMap map = landDataManager.LoadedMaps[loadedMapIndex]!;
        G3dModel? mapModel = LandDataManager_LoadMapModel(landDataManager.LandDataNARC!, landDataHeader.MapModelSize, map, AreaData.AreaDataManager_GetMapTexture(areaDataManager));
        if (isOutdoorsLighting)
        {
            AreaLight.AreaLight_UseGlobalModelAttributes(mapModel!);
        }
        map.Valid = true;
        map.MapMatrixIndex = mapMatrixIndex;
    }

    // OFFSET 0x021E9C0C
    public static void LandDataManager_DistortionWorldRenderInactiveFloor(LandDataManager landDataManager, ModelAttributes? modelAttributes)
    {
        for (int i = 0; i < QuadrantCount; i++)
        {
            if (landDataManager.LoadedMaps[i]!.MapMatrixIndex != -1)
            {
                LandDataManager_DistortionWorldRenderNextFloorMap((byte)i, landDataManager, modelAttributes);
            }
        }
    }

    // OFFSET 0x021E9C40
    private static void LandDataManager_DistortionWorldRenderNextFloorMap(byte index, LandDataManager landDataManager, ModelAttributes? modelAttributes)
    {
        VecFx32 position = default;
        VecFx32 scale = new() { X = Fx32.One, Y = Fx32.One, Z = Fx32.One };
        MtxFx33 rotation = IdentityRotation();
        LoadedMap map = landDataManager.LoadedMaps[index]!;
        LandDataManager_CalculateRenderingPosition(map.MapMatrixIndex, landDataManager.MapMatrixWidth, landDataManager.MapMatrix, ref position);
        position.X += landDataManager.Offset.X;
        position.Y += landDataManager.Offset.Y;
        position.Z += landDataManager.Offset.Z;
        if (map.Valid)
        {
            Easy3D.Easy3D_DrawRenderObj(map.MapRenderObj, position, rotation, scale);
        }
    }

    // OFFSET 0x021E9CD8
    public static void LandDataManager_DistortionWorldEndWithoutFreeing(LandDataManager landDataManager)
    {
        for (int i = 0; i < LazyLoaderManagerCount; i++)
        {
            MapLazyLoader_KillTasks(landDataManager.MapLazyLoaderMan[i].Loader);
        }
        for (int i = 0; i < QuadrantCount; i++)
        {
            landDataManager.LoadedMaps[i]!.Valid = false;
        }
        landDataManager.LazyLoaderSubTask = SubTaskGracefulShutdown;
        landDataManager.Ending = true;
        landDataManager.LazyLoaderTrackedTarget.Tracking = false;
        for (int i = 0; i < QuadrantCount; i++)
        {
            ClearTerrainAttributes(landDataManager.LoadedMaps[i]!);
        }
    }

    // OFFSET 0x021E9D3C
    public static void LandDataManager_DistortionWorldPrepareNextFloor(MapMatrix? mapMatrix, AreaDataManager? areaDataManager, LandDataManager src, LandDataManager dest, int trackedTargetTileX, int trackedTargetTileZ)
    {
        int[] loadedMapMatrixIndexes = new int[QuadrantCount];
        LoadedMapBuffers.LoadedMapBuffers_SwapMapModelData(src.LoadedMapBufs!, dest.LoadedMapBufs!);
        MapMatrix.MapMatrix_Copy(mapMatrix, dest.MapMatrix);
        CopyLoaderState(dest, src);
        dest.AreaDataMan = areaDataManager;
        dest.MapMatrixWidth = src.MapMatrixWidth;
        dest.MapMatrixHeight = src.MapMatrixHeight;
        dest.MapMatrixWidthTiles = src.MapMatrixWidth * MapTiles;
        CopyTrackedTargetAndOffsets(dest, src);
        LandDataManager_UpdateTrackedTargetValues(trackedTargetTileX, trackedTargetTileZ, dest.OffsetTileX, dest.OffsetTileZ, dest);
        dest.Ending = false;
        dest.LazyLoaderSubTask = src.LazyLoaderSubTask;
        for (int i = 0; i < QuadrantCount; i++)
        {
            loadedMapMatrixIndexes[i] = dest.LoadedMaps[i]!.MapMatrixIndex;
            BDHC.BDHC_MarkNotLoaded(dest.LoadedMaps[i]!.Bdhc!);
        }
        for (int i = 0; i < QuadrantCount; i++)
        {
            bool isOutdoorsLighting = AreaData.AreaDataManager_IsOutdoorsLighting(dest.AreaDataMan!);
            LandDataManager_LoadWithoutModel(loadedMapMatrixIndexes[i], (byte)i, dest.AreaDataMan, dest.MapMatrix, dest.MapMatrixWidth, dest.MapMatrixHeight, isOutdoorsLighting, dest);
        }
    }

    // OFFSET 0x021E9F98
    public static void LandDataManager_DistortionWorldPreparePreviousFloor(LandDataManager dest, LandDataManager src)
    {
        LoadedMapBuffers.LoadedMapBuffers_SwapMapModelData(src.LoadedMapBufs!, dest.LoadedMapBufs!);
        MapMatrix.MapMatrix_Copy(src.MapMatrix, dest.MapMatrix);
        CopyLoaderState(dest, src);
        dest.AreaDataMan = src.AreaDataMan;
        dest.MapMatrixWidth = src.MapMatrixWidth;
        dest.MapMatrixHeight = src.MapMatrixHeight;
        dest.MapMatrixWidthTiles = src.MapMatrixWidth * MapTiles;
        CopyTrackedTargetAndOffsets(dest, src);
        dest.Ending = false;
        dest.LazyLoaderSubTask = src.LazyLoaderSubTask;
    }

    // OFFSET 0x021EA174
    public static void LandDataManager_DistortionWorldTick(FieldSystem fieldSystem, LandDataManager landDataManager)
    {
        MapLazyLoader loader = landDataManager.MapLazyLoaderMan[landDataManager.ActiveLazyLoaderManIndex].Loader;
        if (!UpdateTrackedTarget(landDataManager))
        {
            return;
        }
        switch (landDataManager.LazyLoaderSubTask)
        {
            case SubTaskLoadMap:
                if (!landDataManager.MapQuadrantsToCancelLazyLoad[loader.Quadrants[loader.NextIndexToLoad]])
                {
                    LandDataManager_LazyLoadWithoutAttributesAndProps(loader.NextIndexToLoad, landDataManager.AreaDataMan, landDataManager.MapMatrix, landDataManager.MapMatrixWidth, landDataManager.MapMatrixHeight, landDataManager, loader);
                }
                loader.NextIndexToLoad++;
                landDataManager.LazyLoaderSubTask = SubTaskFinishMapLoad;
                break;
            case SubTaskFinishMapLoad:
                FinishMapLoad(landDataManager, loader);
                break;
            case SubTaskIdle:
                break;
            case SubTaskGracefulShutdown:
                if (LandDataManager_LazyLoadFinished(loader.MapLoadTasksState))
                {
                    landDataManager.RunningLazyLoaderManCount = 0;
                }
                break;
        }
        AdvanceLazyLoaderManagers(landDataManager);
    }

    // OFFSET 0x021EA540
    public static void LandDataManager_DistortionWorldInitWithoutNARC(LandDataManager landDataManager, MapMatrix? mapMatrix, AreaDataManager? areaDataManager)
    {
        landDataManager.AreaDataMan = areaDataManager;
        landDataManager.MapMatrix = mapMatrix;
        landDataManager.MapMatrixWidth = MapMatrix.MapMatrix_GetWidth(mapMatrix);
        landDataManager.MapMatrixHeight = MapMatrix.MapMatrix_GetHeight(mapMatrix);
        landDataManager.MapMatrixWidthTiles = landDataManager.MapMatrixWidth * MapTiles;
        LandDataManager_InitMapLazyLoader(landDataManager);
        landDataManager.Ending = false;
        landDataManager.LazyLoaderSubTask = SubTaskIdle;
    }

    // OFFSET 0x021EA58C
    public static void LandDataManager_DistortionWorldInvalidateLoadedMaps(LandDataManager landDataManager, int trackedTargetTileX, int trackedTargetTileZ, int[] loadedMapMatrixIndexes)
    {
        LandData_CalculateLoadedMapMatrixIndexes(landDataManager.InDistortionWorld, trackedTargetTileX, trackedTargetTileZ, landDataManager.OffsetTileX, landDataManager.OffsetTileZ, landDataManager.MapMatrixWidth, landDataManager.MapMatrixHeight, landDataManager.MapMatrixWidthTiles, loadedMapMatrixIndexes);
        for (int i = 0; i < QuadrantCount; i++)
        {
            landDataManager.LoadedMaps[i]!.Valid = false;
        }
    }

    // OFFSET 0x021EA5E0
    public static void LandDataManager_DistortionWorldLoadEntire(LandDataManager landDataManager, int loadedMapIndex, int mapMatrixIndex)
    {
        LoadedMap map = landDataManager.LoadedMaps[loadedMapIndex]!;
        map.Valid = false;
        LoadedMapBuffers.LoadedMapBuffers_GetMapModelFileBufPtr((byte)loadedMapIndex, landDataManager.LoadedMapBufs!, out map.MapModelFile);
        LoadedMapBuffers.LoadedMapBuffers_GetBDHCFileBufPtr((byte)loadedMapIndex, landDataManager.LoadedMapBufs!, out map.BdhcFile);
        map.MapMatrixIndex = -1;
        ClearTerrainAttributes(map);
        BDHC.BDHC_MarkNotLoaded(map.Bdhc!);
        bool isOutdoorsLighting = AreaData.AreaDataManager_IsOutdoorsLighting(landDataManager.AreaDataMan!);
        LandDataManager_Load(mapMatrixIndex, (byte)loadedMapIndex, landDataManager.AreaDataMan, landDataManager.MapMatrix, landDataManager.MapMatrixWidth, landDataManager.MapMatrixHeight, isOutdoorsLighting, landDataManager);
    }

    // OFFSET 0x021EA678
    public static void LandDataManager_DistortionWorldSetOffsets(LandDataManager landDataManager, int offsetTileX, int offsetAltitude, int offsetTileZ)
    {
        landDataManager.OffsetTileX = offsetTileX;
        landDataManager.OffsetAltitude = offsetAltitude;
        landDataManager.OffsetTileZ = offsetTileZ;
        landDataManager.Offset.X = offsetTileX << 16;
        landDataManager.Offset.Y = offsetAltitude << 16;
        landDataManager.Offset.Z = offsetTileZ << 16;
    }

    // OFFSET 0x021EA6A4
    public static void LandDataManager_SetInDistortionWorld(LandDataManager landDataManager, int inDistortionWorld)
    {
        landDataManager.InDistortionWorld = inDistortionWorld;
    }

    // OFFSET 0x021EA6AC
    public static int LandDataManager_GetOffsetTileX(LandDataManager landDataManager)
    {
        return landDataManager.OffsetTileX;
    }

    // OFFSET 0x021EA6B4
    public static int LandDataManager_GetOffsetTileZ(LandDataManager landDataManager)
    {
        return landDataManager.OffsetTileZ;
    }

    // OFFSET 0x021EA6BC
    public static void LandDataManager_GetOffset(LandDataManager landDataManager, out VecFx32 offset)
    {
        offset = landDataManager.Offset;
    }

    // OFFSET 0x021EA6D0
    public static void LandDataManager_SetSkipMapProps(LandDataManager landDataManager, int skipMapProps)
    {
        landDataManager.SkipMapProps = skipMapProps;
    }

    // OFFSET 0x021EA6D8
    public static void LandDataManager_SetMapMatrix(LandDataManager landDataManager, MapMatrix? mapMatrix)
    {
        landDataManager.MapMatrix = mapMatrix;
    }

    // OFFSET 0x021EA6E0
    public static void LandDataManager_SetLoadedMapValid(LandDataManager landDataManager, int index, bool valid)
    {
        landDataManager.LoadedMaps[index]!.Valid = valid;
    }

    // OFFSET 0x021EA6F4
    public static void LandDataManager_DistortionWorldUpdateTrackedTargetValues(LandDataManager landDataManager, int trackedTargetTileX, int trackedTargetTileZ)
    {
        LandDataManager_UpdateTrackedTargetValues(trackedTargetTileX, trackedTargetTileZ, landDataManager.OffsetTileX, landDataManager.OffsetTileZ, landDataManager);
    }
}
