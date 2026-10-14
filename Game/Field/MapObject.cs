using static PlatinumSharp.Game.MapObjectManager;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void MapObjectFunc(MapObject mapObject);

public sealed class MapObjectMovementFuncs
{
    public const int Size = 0x14;

    public uint MovementType;
    public MapObjectFunc? Init;
    public MapObjectFunc? Move;
    public MapObjectFunc? Exit;
    public MapObjectFunc? Return;
}

public sealed class MapObjectDrawFuncs
{
    public const int Size = 0x14;

    public MapObjectFunc? Init;
    public MapObjectFunc? Draw;
    public MapObjectFunc? Delete;
    public MapObjectFunc? Suspend;
    public MapObjectFunc? Resume;
}

public sealed class ObjectEventLoader
{
    public const int Size = 0x14;

    public int MapHeaderID;
    public int NumObjectEvents;
    public int Index;
    public MapObjectManager? MapObjectManager;
    public ObjectEvent[]? ObjectEvents;
}

public sealed class MapObjectManager
{
    public const int Size = 0x12C;
    public const int StatusOffset = 0x0;
    public const int MaxObjectsOffset = 0x4;
    public const int ObjectCountOffset = 0x8;
    public const int TaskBasePriorityOffset = 0xC;
    public const int NarcOffset = 0x14;
    public const int RenderDataOffset = 0x18;
    public const int MapObjectsOffset = 0x124;
    public const int FieldSystemOffset = 0x128;

    public const uint StatusDrawInitialized = 1 << 0;
    public const uint StatusStopMovement = 1 << 1;
    public const uint StatusPauseMovement = 1 << 2;
    public const uint StatusShadowsDisabled = 1 << 3;

    public const int HeapIDField = 0xB;

    public uint Status;
    public int MaxObjects;
    public int ObjectCount;
    public int TaskBasePriority;
    public NARC? Narc;
    public MapObjectRenderData RenderData = new();
    public MapObject[]? MapObjects;
    public FieldSystem? FieldSystem;

    // OFFSET 0x02061804
    public static MapObjectManager MapObjectMan_New(FieldSystem fieldSystem, int maxObjects, int taskBasePriority)
    {
        MapObjectManager mapObjectManager = MapObjectMan_Alloc(maxObjects);
        MapObjectMan_SetFieldSystem(mapObjectManager, fieldSystem);
        MapObjectMan_SetMaxObjects(mapObjectManager, maxObjects);
        MapObjectMan_SetTaskBasePriority(mapObjectManager, taskBasePriority);
        return mapObjectManager;
    }

    // OFFSET 0x02061830
    public static void MapObjectMan_Delete(MapObjectManager mapObjectManager)
    {
        MapObjectMan_GetMapObjects(mapObjectManager);
        mapObjectManager.MapObjects = null;
    }

    // OFFSET 0x0206184C
    public static void MapObjectMan_DeleteObjectsOnMapChange(MapObjectManager mapObjectManager, int oldMapHeaderID, int newMapHeaderID, int numObjectEvents, ObjectEvent[]? objectEvents)
    {
        int maxObjects = MapObjectMan_GetMaxObjects(mapObjectManager);
        MapObject[] mapObjects = MapObjectMan_GetMapObjects(mapObjectManager)!;
        for (int i = 0; i < maxObjects; i++)
        {
            MapObject mapObject = mapObjects[i];
            if (MapObject.MapObject_IsActive(mapObject) == 1)
            {
                int match = MapObject.MapObject_MatchObjectEvents(mapObject, newMapHeaderID, numObjectEvents, objectEvents);
                if (match == 0)
                {
                    if (MapObject.MapObject_GetMapHeaderID(mapObject) != newMapHeaderID
                        && !MapObject.MapObject_CheckStatusFlag(mapObject, MapObject.StatusPersistent))
                    {
                        MapObject.MapObject_Delete(mapObject);
                    }
                }
            }
        }
        MapObjectRenderResources.MapObjectMan_FreeUnusedResources(mapObjectManager, MapObjectMan_GetRenderData(mapObjectManager));
    }

    // OFFSET 0x020618C8
    private static MapObjectManager MapObjectMan_Alloc(int maxObjects)
    {
        MapObjectManager mapObjectManager = new();
        MapObject[] mapObjects = new MapObject[maxObjects];
        for (int i = 0; i < maxObjects; i++)
        {
            mapObjects[i] = new MapObject(mapObjects, i);
        }
        MapObjectMan_SetMapObjects(mapObjectManager, mapObjects);
        return mapObjectManager;
    }

    // OFFSET 0x02061918
    public static MapObject? MapObjectMan_AddMapObjectFromHeader(MapObjectManager mapObjectManager, ObjectEvent sourceEvent, int mapHeaderID)
    {
        ObjectEvent objectEvent = CopyObjectEvent(sourceEvent);
        int localID = MapObject.ObjectEvent_GetLocalID(objectEvent);
        MapObject? mapObject;
        if (MapObject.ObjectEvent_HasNoScript(objectEvent) == 0)
        {
            mapObject = MapObjectMan_FindAliasObject(mapObjectManager, localID, mapHeaderID);
            if (mapObject != null)
            {
                MapObject.MapObject_ConvertAliasToLocal(mapObject, objectEvent, mapHeaderID);
                return mapObject;
            }
        }
        else
        {
            int aliasMapHeaderID = MapObject.ObjectEvent_GetAliasMapHeaderID(objectEvent);
            mapObject = MapObjectMan_FindObjectByLocalIDAndMap(mapObjectManager, localID, aliasMapHeaderID);
            if (mapObject != null)
            {
                MapObject.MapObject_ConvertLocalToAlias(mapObject, mapHeaderID, objectEvent);
                return mapObject;
            }
        }
        mapObject = MapObjectMan_FindFreeObject(mapObjectManager);
        if (mapObject == null)
        {
            return null;
        }
        MapObject.MapObject_InitFromObjectEvent(mapObject, objectEvent, MapObjectMan_FieldSystem(mapObjectManager)!);
        MapObject.MapObject_InitStatus(mapObject, mapObjectManager);
        MapObject.MapObject_SetMapHeaderID(mapObject, mapHeaderID);
        MapObject.MapObject_InitMovement(mapObject);
        MapObject.MapObject_InitDraw(mapObject);
        MapObject.MapObject_SetStatusFlagOn(mapObject, MapObject.StatusStartMovement);
        MapObjectMan_AddMoveTask(mapObjectManager, mapObject);
        MapObjectMan_IncObjectCount(MapObjectMan_Deconst(mapObjectManager));
        return mapObject;
    }

    // OFFSET 0x020619DC
    public static MapObject? MapObjectMan_AddMapObject(MapObjectManager mapObjectManager, int x, int z, int initialDir, int graphicsID, int movementType, int mapHeaderID)
    {
        ObjectEvent objectEvent = new();
        MapObject.ObjectEvent_SetLocalID(objectEvent, 0);
        MapObject.ObjectEvent_SetGraphicsID(objectEvent, graphicsID);
        MapObject.ObjectEvent_SetMovementType(objectEvent, movementType);
        MapObject.ObjectEvent_SetTrainerType(objectEvent, 0);
        MapObject.ObjectEvent_SetHiddenFlag(objectEvent, 0);
        MapObject.ObjectEvent_SetScript(objectEvent, 0);
        MapObject.ObjectEvent_SetInitialDir(objectEvent, initialDir);
        MapObject.ObjectEvent_SetDataAt(objectEvent, 0, 0);
        MapObject.ObjectEvent_SetDataAt(objectEvent, 0, 1);
        MapObject.ObjectEvent_SetDataAt(objectEvent, 0, 2);
        MapObject.ObjectEvent_SetMovementRangeX(objectEvent, 0);
        MapObject.ObjectEvent_SetMovementRangeZ(objectEvent, 0);
        MapObject.ObjectEvent_SetX(objectEvent, x);
        MapObject.ObjectEvent_SetZ(objectEvent, z);
        MapObject.ObjectEvent_SetY(objectEvent, 0);
        return MapObjectMan_AddMapObjectFromHeader(mapObjectManager, objectEvent, mapHeaderID);
    }

    // OFFSET 0x02061A74
    public static MapObject? MapObjectMan_AddMapObjectFromLocalID(MapObjectManager mapObjectManager, int localID, int numObjectEvents, int mapHeaderID, ObjectEvent[]? objectEvents)
    {
        MapObject? mapObject = null;
        ObjectEvent? objectEvent = MapObject.ObjectEvent_FindByLocalID(localID, numObjectEvents, objectEvents);
        if (objectEvent != null)
        {
            int hiddenFlag = MapObject.ObjectEvent_GetHiddenFlag(objectEvent);
            if (!FieldSystemVarsFlags.FieldSystem_CheckFlag(MapObjectMan_FieldSystem(mapObjectManager)!, (ushort)hiddenFlag))
            {
                mapObject = MapObjectMan_AddMapObjectFromHeader(mapObjectManager, objectEvent, mapHeaderID);
            }
        }
        return mapObject;
    }

    // OFFSET 0x02061BBC
    public static void MapObjectMan_DeleteAll(MapObjectManager mapObjectManager)
    {
        int maxObjects = MapObjectMan_GetMaxObjects(mapObjectManager);
        MapObject[] mapObjects = MapObjectMan_GetMapObjects(mapObjectManager)!;
        for (int i = 0; i < maxObjects; i++)
        {
            MapObject mapObject = mapObjects[i];
            if (MapObject.MapObject_CheckStatus(mapObject, MapObject.StatusActive) != 0)
            {
                MapObject.MapObject_Delete(mapObject);
            }
        }
    }

    // OFFSET 0x02061BF0
    public static void MapObjectMan_SuspendDrawAll(MapObjectManager mapObjectManager)
    {
        if (MapObjectMan_IsDrawInitialized(mapObjectManager) != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        int maxObjects = MapObjectMan_GetMaxObjects(mapObjectManager);
        MapObject[] mapObjects = MapObjectMan_GetMapObjects(mapObjectManager)!;
        for (int i = 0; i < maxObjects; i++)
        {
            MapObject mapObject = mapObjects[i];
            if (MapObject.MapObject_CheckStatus(mapObject, MapObject.StatusActive) != 0
                && MapObject.MapObject_CheckStatus(mapObject, MapObject.StatusDrawInitialized) != 0)
            {
                MapObject.MapObject_CallDrawSuspend(mapObject);
                MapObject.MapObject_AfterDrawSuspend(mapObject);
            }
        }
    }

    // OFFSET 0x02061C48
    public static void MapObjectMan_ResumeDrawAll(MapObjectManager mapObjectManager)
    {
        if (MapObjectMan_IsDrawInitialized(mapObjectManager) != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        int maxObjects = MapObjectMan_GetMaxObjects(mapObjectManager);
        MapObject[] mapObjects = MapObjectMan_GetMapObjects(mapObjectManager)!;
        for (int i = 0; i < maxObjects; i++)
        {
            MapObject mapObject = mapObjects[i];
            if (MapObject.MapObject_IsActive(mapObject) == 1)
            {
                if (MapObject.MapObject_IsDrawInitialized(mapObject) == 1)
                {
                    MapObject.MapObject_CallDrawResume(mapObject);
                }
                else
                {
                    MapObject.MapObject_InitDraw(mapObject);
                }
                MapObject.MapObject_ResetMovementStatus(mapObject);
                MapObjectMove.MapObject_MoveIfScripted(mapObject);
            }
        }
    }

    // OFFSET 0x02061CA8
    public static void MapObjectMan_SaveAll(FieldSystem fieldSystem, MapObjectManager mapObjectManager, BufferView mapObjectSaves, int count)
    {
        int index = 0;
        int position = 0;
        while (MapObjectMan_FindObjectWithStatus(mapObjectManager, out MapObject? mapObject, ref index, MapObject.StatusActive) != 0)
        {
            MapObject.MapObject_Save(fieldSystem, mapObject!, new BufferView(mapObjectSaves.Buffer, mapObjectSaves.Offset + position));
            count--;
            position += MapObject.SaveSize;
            if (count <= 0)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        if (count != 0)
        {
            mapObjectSaves.Clear(position, count * MapObject.SaveSize);
        }
    }

    // OFFSET 0x02061D04
    public static void MapObjectMan_LoadAllObjects(MapObjectManager mapObjectManager, BufferView mapObjectSaves, int count)
    {
        int position = 0;
        while (count != 0)
        {
            BufferView mapObjectSave = new(mapObjectSaves.Buffer, mapObjectSaves.Offset + position);
            if ((mapObjectSave.U32(MapObject.SaveStatusOffset) & MapObject.StatusActive) != 0)
            {
                MapObject? mapObject = MapObjectMan_FindFreeObject(mapObjectManager);
                if (mapObject == null)
                {
                    ErrorHandling.GF_AssertFail();
                }
                MapObject.MapObject_LoadSave(mapObject!, mapObjectSave);
                MapObjectMan_RestoreLoadedObject(mapObjectManager, mapObject!);
            }
            position += MapObject.SaveSize;
            count--;
        }
    }

    // OFFSET 0x02061FA8
    private static void MapObjectMan_RestoreLoadedObject(MapObjectManager mapObjectManager, MapObject mapObject)
    {
        MapObject.MapObject_ResetStatusAfterLoad(mapObject);
        MapObject.MapObject_ResetPositionAfterLoad(mapObject);
        MapObject.MapObject_SetMapObjectManager(mapObject, mapObjectManager);
        MapObject.MapObject_SetMovementFuncs(mapObject);
        MovementActions.MapObject_ClearMovementAction(mapObject);
        MapObject.MapObject_InitDraw(mapObject);
        MapObjectMan_AddMoveTask(mapObjectManager, mapObject);
        MapObject.MapObject_CallMovementReturn(mapObject);
        MapObjectMan_IncObjectCount(MapObjectMan_Deconst(mapObjectManager));
    }

    // OFFSET 0x02062068
    public static void MapObjectMan_AddAllFromObjectEvents(MapObjectManager mapObjectManager, int mapHeaderID, int numObjectEvents, ObjectEvent[]? objectEvents)
    {
        if (numObjectEvents == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        ObjectEvent[] objectEventsCopy = new ObjectEvent[numObjectEvents];
        for (int i = 0; i < numObjectEvents; i++)
        {
            objectEventsCopy[i] = CopyObjectEvent(objectEvents![i]);
        }
        ObjectEventLoader loader = new()
        {
            MapHeaderID = mapHeaderID,
            NumObjectEvents = numObjectEvents,
            Index = 0,
            MapObjectManager = mapObjectManager,
            ObjectEvents = objectEventsCopy,
        };
        MapObjectMan_RunObjectEventLoader(loader);
    }

    // OFFSET 0x020620C4
    private static void MapObjectMan_RunObjectEventLoader(ObjectEventLoader loader)
    {
        FieldSystem fieldSystem = MapObjectMan_FieldSystem(loader.MapObjectManager!)!;
        ObjectEvent[] objectEvents = loader.ObjectEvents!;
        int position = 0;
        do
        {
            ObjectEvent objectEvent = objectEvents[position];
            if (MapObject.ObjectEvent_HasNoScript(objectEvent) == 1
                || !FieldSystemVarsFlags.FieldSystem_CheckFlag(fieldSystem, objectEvent.HiddenFlag))
            {
                MapObject? mapObject = MapObjectMan_AddMapObjectFromHeader(loader.MapObjectManager!, objectEvent, loader.MapHeaderID);
                if (mapObject == null)
                {
                    ErrorHandling.GF_AssertFail();
                }
            }
            loader.Index++;
            position++;
        }
        while (loader.Index < loader.NumObjectEvents);
        loader.ObjectEvents = null;
    }

    // OFFSET 0x02062120
    private static MapObject? MapObjectMan_FindFreeObject(MapObjectManager mapObjectManager)
    {
        int maxObjects = MapObjectMan_GetMaxObjects(mapObjectManager);
        MapObject[] mapObjects = MapObjectMan_GetMapObjects(mapObjectManager)!;
        for (int i = 0; i < maxObjects; i++)
        {
            MapObject mapObject = mapObjects[i];
            if (MapObject.MapObject_CheckStatus(mapObject, MapObject.StatusActive) == 0)
            {
                return mapObject;
            }
        }
        return null;
    }

    // OFFSET 0x02062154
    private static MapObject? MapObjectMan_FindAliasObject(MapObjectManager mapObjectManager, int localID, int aliasMapHeaderID)
    {
        int index = 0;
        while (MapObjectMan_FindObjectWithStatus(mapObjectManager, out MapObject? mapObject, ref index, MapObject.StatusActive) == 1)
        {
            if (MapObject.MapObject_IsAlias(mapObject!) == 1
                && MapObject.MapObject_GetLocalID(mapObject!) == localID
                && MapObject.MapObject_GetAliasMapHeaderID(mapObject!) == aliasMapHeaderID)
            {
                return mapObject;
            }
        }
        return null;
    }

    // OFFSET 0x020621AC
    private static void MapObjectMan_AddMoveTask(MapObjectManager mapObjectManager, MapObject mapObject)
    {
        int priority = MapObjectMan_GetTaskBasePriority(mapObjectManager);
        int movementType = MapObject.MapObject_GetMovementType(mapObject);
        if (movementType == MapObject.MovementTypeFollowPlayer || movementType == MapObject.MovementTypeFollowPartnerTrainer)
        {
            priority += 2;
        }
        SysTask? task = SysTask.SysTask_Start(MapObject.MapObjectTask_Move, mapObject, (uint)priority);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObject.MapObject_SetTask(mapObject, task);
    }

    // OFFSET 0x020624CC
    private static MapObject? MapObjectMan_FindObjectByLocalIDAndMap(MapObjectManager mapObjectManager, int localID, int mapHeaderID)
    {
        int index = 0;
        while (MapObjectMan_FindObjectWithStatus(mapObjectManager, out MapObject? mapObject, ref index, MapObject.StatusActive) == 1)
        {
            if (MapObject.MapObject_GetLocalID(mapObject!) == localID
                && MapObject.MapObject_GetMapHeaderID(mapObject!) == mapHeaderID)
            {
                return mapObject;
            }
        }
        return null;
    }

    // OFFSET 0x0206251C
    public static MapObject? MapObjMan_LocalMapObjByIndex(MapObjectManager? mapObjectManager, int localID)
    {
        if (mapObjectManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        int remaining = MapObjectMan_GetMaxObjects(mapObjectManager!);
        MapObject[] mapObjects = MapObjectMan_GetMapObjectsStatic(mapObjectManager!)!;
        int position = 0;
        do
        {
            MapObject mapObject = mapObjects[position];
            if (MapObject.MapObject_CheckStatusFlag(mapObject, MapObject.StatusActive)
                && MapObject.MapObject_IsAlias(mapObject) == 0
                && MapObject.MapObject_GetLocalID(mapObject) == localID)
            {
                return mapObject;
            }
            remaining--;
            position++;
        }
        while (remaining > 0);
        return null;
    }

    // OFFSET 0x02062570
    public static MapObject? MapObjMan_GetLocalMapObjByMovementType(MapObjectManager mapObjectManager, int movementType)
    {
        int remaining = MapObjectMan_GetMaxObjects(mapObjectManager);
        MapObject[] mapObjects = MapObjectMan_GetMapObjectsStatic(mapObjectManager)!;
        int position = 0;
        do
        {
            MapObject mapObject = mapObjects[position];
            if (MapObject.MapObject_CheckStatusFlag(mapObject, MapObject.StatusActive)
                && MapObject.MapObject_GetMovementType(mapObject) == movementType)
            {
                return mapObject;
            }
            remaining--;
            position++;
        }
        while (remaining > 0);
        return null;
    }

    // OFFSET 0x020625B0
    public static int MapObjectMan_FindObjectWithStatus(MapObjectManager mapObjectManager, out MapObject? mapObject, ref int index, uint status)
    {
        mapObject = null;
        int maxObjects = MapObjectMan_GetMaxObjects(mapObjectManager);
        if (index >= maxObjects)
        {
            return 0;
        }
        MapObject[] mapObjects = MapObjectMan_GetMapObjectsStatic(mapObjectManager)!;
        int position = index;
        do
        {
            MapObject candidate = mapObjects[position];
            index++;
            if (MapObject.MapObject_CheckStatus(candidate, status) == status)
            {
                mapObject = candidate;
                return 1;
            }
            position++;
        }
        while (index < maxObjects);
        return 0;
    }

    // OFFSET 0x0206281C
    private static MapObjectManager MapObjectMan_Deconst(MapObjectManager mapObjectManager)
    {
        return mapObjectManager;
    }

    // OFFSET 0x02062820
    public static void MapObjectMan_SetMaxObjects(MapObjectManager mapObjectManager, int maxObjects)
    {
        mapObjectManager.MaxObjects = maxObjects;
    }

    // OFFSET 0x02062824
    public static int MapObjectMan_GetMaxObjects(MapObjectManager mapObjectManager)
    {
        return mapObjectManager.MaxObjects;
    }

    // OFFSET 0x02062828
    private static void MapObjectMan_IncObjectCount(MapObjectManager mapObjectManager)
    {
        mapObjectManager.ObjectCount++;
    }

    // OFFSET 0x02062830
    public static void MapObjectMan_DecObjectCount(MapObjectManager mapObjectManager)
    {
        mapObjectManager.ObjectCount--;
    }

    // OFFSET 0x02062838
    public static void MapObjectMan_SetStatusFlagOn(MapObjectManager mapObjectManager, uint flag)
    {
        mapObjectManager.Status |= flag;
    }

    // OFFSET 0x02062840
    public static void MapObjectMan_SetStatusFlagOff(MapObjectManager mapObjectManager, uint flag)
    {
        mapObjectManager.Status &= ~flag;
    }

    // OFFSET 0x0206284C
    public static uint MapObjectMan_CheckStatus(MapObjectManager mapObjectManager, uint flag)
    {
        return mapObjectManager.Status & flag;
    }

    // OFFSET 0x02062854
    public static void MapObjectMan_SetTaskBasePriority(MapObjectManager mapObjectManager, int taskBasePriority)
    {
        mapObjectManager.TaskBasePriority = taskBasePriority;
    }

    // OFFSET 0x02062858
    public static int MapObjectMan_GetTaskBasePriority(MapObjectManager mapObjectManager)
    {
        return mapObjectManager.TaskBasePriority;
    }

    // OFFSET 0x0206285C
    public static MapObjectRenderData MapObjectMan_GetRenderData(MapObjectManager mapObjectManager)
    {
        return mapObjectManager.RenderData;
    }

    // OFFSET 0x02062860
    public static void MapObjectMan_SetMapObjects(MapObjectManager mapObjectManager, MapObject[]? mapObjects)
    {
        mapObjectManager.MapObjects = mapObjects;
    }

    // OFFSET 0x02062868
    public static MapObject[]? MapObjectMan_GetMapObjectsConst(MapObjectManager mapObjectManager)
    {
        return mapObjectManager.MapObjects;
    }

    // OFFSET 0x02062870
    private static MapObject[]? MapObjectMan_GetMapObjectsStatic(MapObjectManager mapObjectManager)
    {
        return mapObjectManager.MapObjects;
    }

    // OFFSET 0x02062878
    public static MapObject[]? MapObjectMan_GetMapObjects(MapObjectManager mapObjectManager)
    {
        return mapObjectManager.MapObjects;
    }

    // OFFSET 0x02062880
    public static void MapObjectMan_NextMapObject(ref MapObject? mapObject)
    {
        MapObject current = mapObject!;
        int next = current.Index + 1;
        mapObject = next < current.Owner.Length ? current.Owner[next] : null;
    }

    // OFFSET 0x0206288C
    public static void MapObjectMan_SetFieldSystem(MapObjectManager mapObjectManager, FieldSystem? fieldSystem)
    {
        mapObjectManager.FieldSystem = fieldSystem;
    }

    // OFFSET 0x02062894
    public static FieldSystem? MapObjectMan_FieldSystem(MapObjectManager mapObjectManager)
    {
        return mapObjectManager.FieldSystem;
    }

    // OFFSET 0x0206289C
    public static void MapObjectMan_SetNARC(MapObjectManager mapObjectManager, NARC? narc)
    {
        mapObjectManager.Narc = narc;
    }

    // OFFSET 0x020628A0
    public static NARC? MapObjectMan_GetNARC(MapObjectManager mapObjectManager)
    {
        if (mapObjectManager.Narc == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return mapObjectManager.Narc;
    }

    // OFFSET 0x02062C30
    public static void MapObjectMan_StopAllMovement(MapObjectManager mapObjectManager)
    {
        MapObjectMan_SetStatusFlagOn(mapObjectManager, StatusStopMovement | StatusPauseMovement);
    }

    // OFFSET 0x02062C3C
    public static void MapObjectMan_ResumeAllMovement(MapObjectManager mapObjectManager)
    {
        MapObjectMan_SetStatusFlagOff(mapObjectManager, StatusStopMovement | StatusPauseMovement);
    }

    // OFFSET 0x02062C48
    public static void MapObjectMan_PauseAllMovement(MapObjectManager mapObjectManager)
    {
        int maxObjects = MapObjectMan_GetMaxObjects(mapObjectManager);
        MapObject[] mapObjects = MapObjectMan_GetMapObjects(mapObjectManager)!;
        for (int i = 0; i < maxObjects; i++)
        {
            MapObject mapObject = mapObjects[i];
            if (MapObject.MapObject_IsActive(mapObject) != 0)
            {
                MapObject.MapObject_SetPauseMovementOn(mapObject);
            }
        }
    }

    // OFFSET 0x02062C78
    public static void MapObjectMan_UnpauseAllMovement(MapObjectManager mapObjectManager)
    {
        int maxObjects = MapObjectMan_GetMaxObjects(mapObjectManager);
        MapObject[] mapObjects = MapObjectMan_GetMapObjects(mapObjectManager)!;
        for (int i = 0; i < maxObjects; i++)
        {
            MapObject mapObject = mapObjects[i];
            if (MapObject.MapObject_IsActive(mapObject) != 0)
            {
                MapObject.MapObject_SetPauseMovementOff(mapObject);
            }
        }
    }

    // OFFSET 0x02062CA8
    public static int MapObjectMan_IsDrawInitialized(MapObjectManager mapObjectManager)
    {
        if (MapObjectMan_CheckStatus(mapObjectManager, StatusDrawInitialized) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02062CCC
    public static void MapObjectMan_SetShadowsEnabled(MapObjectManager mapObjectManager, int enabled)
    {
        if (enabled == 0)
        {
            MapObjectMan_SetStatusFlagOn(mapObjectManager, StatusShadowsDisabled);
        }
        else
        {
            MapObjectMan_SetStatusFlagOff(mapObjectManager, StatusShadowsDisabled);
        }
    }

    // OFFSET 0x02062CE4
    public static int MapObjectMan_AreShadowsEnabled(MapObjectManager mapObjectManager)
    {
        if (MapObjectMan_CheckStatus(mapObjectManager, StatusShadowsDisabled) == 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0206326C
    public static MapObject? MapObjectMan_FindObjectAtTile(MapObjectManager mapObjectManager, int x, int z, int checkPrevious)
    {
        int maxObjects = MapObjectMan_GetMaxObjects(mapObjectManager);
        MapObject[] mapObjects = MapObjectMan_GetMapObjects(mapObjectManager)!;
        for (int i = 0; i < maxObjects; i++)
        {
            MapObject mapObject = mapObjects[i];
            if (MapObject.MapObject_CheckStatus(mapObject, MapObject.StatusActive) != 0)
            {
                if (checkPrevious != 0
                    && MapObject.MapObject_GetXPrev(mapObject) == x
                    && MapObject.MapObject_GetZPrev(mapObject) == z)
                {
                    return mapObject;
                }
                if (MapObject.MapObject_GetX(mapObject) == x && MapObject.MapObject_GetZ(mapObject) == z)
                {
                    return mapObject;
                }
            }
        }
        return null;
    }

    private static ObjectEvent CopyObjectEvent(ObjectEvent source)
    {
        ObjectEvent copy = new()
        {
            LocalID = source.LocalID,
            GraphicsID = source.GraphicsID,
            MovementType = source.MovementType,
            TrainerType = source.TrainerType,
            HiddenFlag = source.HiddenFlag,
            Script = source.Script,
            Dir = source.Dir,
            MovementRangeX = source.MovementRangeX,
            MovementRangeZ = source.MovementRangeZ,
            X = source.X,
            Z = source.Z,
            Y = source.Y,
        };
        copy.Data[0] = source.Data[0];
        copy.Data[1] = source.Data[1];
        copy.Data[2] = source.Data[2];
        return copy;
    }
}

public sealed partial class MapObject
{
    public const int Size = 0x128;
    public const int StatusOffset = 0x0;
    public const int Status2Offset = 0x4;
    public const int LocalIDOffset = 0x8;
    public const int MapHeaderIDOffset = 0xC;
    public const int GraphicsIDOffset = 0x10;
    public const int MovementTypeOffset = 0x14;
    public const int TrainerTypeOffset = 0x18;
    public const int FlagOffset = 0x1C;
    public const int ScriptOffset = 0x20;
    public const int InitialDirOffset = 0x24;
    public const int FacingDirOffset = 0x28;
    public const int MovingDirOffset = 0x2C;
    public const int PrevFacingDirOffset = 0x30;
    public const int PrevMovingDirOffset = 0x34;
    public const int DataOffset = 0x38;
    public const int MovementRangeXOffset = 0x44;
    public const int MovementRangeZOffset = 0x48;
    public const int XInitialOffset = 0x4C;
    public const int YInitialOffset = 0x50;
    public const int ZInitialOffset = 0x54;
    public const int XPrevOffset = 0x58;
    public const int YPrevOffset = 0x5C;
    public const int ZPrevOffset = 0x60;
    public const int XOffset = 0x64;
    public const int YOffset = 0x68;
    public const int ZOffset = 0x6C;
    public const int PositionOffset = 0x70;
    public const int SpriteJumpOffsetOffset = 0x7C;
    public const int SpritePosOffsetOffset = 0x88;
    public const int SpriteTerrainOffsetOffset = 0x94;
    public const int DrawStateOffset = 0xA0;
    public const int MovementActionOffset = 0xA4;
    public const int MovementStepOffset = 0xA8;
    public const int CurrTileBehaviorOffset = 0xAC;
    public const int PrevTileBehaviorOffset = 0xAE;
    public const int TaskOffset = 0xB0;
    public const int MapObjectManagerOffset = 0xB4;
    public const int MovementInitOffset = 0xB8;
    public const int MovementMoveOffset = 0xBC;
    public const int MovementExitOffset = 0xC0;
    public const int DrawInitOffset = 0xC4;
    public const int DrawOffset = 0xC8;
    public const int DrawDeleteOffset = 0xCC;
    public const int DrawSuspendOffset = 0xD0;
    public const int DrawResumeOffset = 0xD4;
    public const int MovementTypeDataOffset = 0xD8;
    public const int TrainerTypeDataOffset = 0xE8;
    public const int MovementDataOffset = 0xF8;
    public const int DrawDataOffset = 0x108;

    public const int MovementTypeDataSize = 0x10;
    public const int TrainerTypeDataSize = 0x10;
    public const int MovementDataSize = 0x10;
    public const int DrawDataSize = 0x20;

    public const uint StatusActive = 1 << 0;
    public const uint StatusMoving = 1 << 1;
    public const uint StatusStartMovement = 1 << 2;
    public const uint StatusEndMovement = 1 << 3;
    public const uint StatusMovementAction = 1 << 4;
    public const uint StatusMovementActionEnded = 1 << 5;
    public const uint StatusPauseMovement = 1 << 6;
    public const uint StatusLockDir = 1 << 7;
    public const uint StatusPauseAnimation = 1 << 8;
    public const uint StatusHide = 1 << 9;
    public const uint StatusPersistent = 1 << 10;
    public const uint Status11 = 1 << 11;
    public const uint Status12 = 1 << 12;
    public const uint Status13 = 1 << 13;
    public const uint StatusDrawInitialized = 1 << 14;
    public const uint StatusShowShadow = 1 << 15;
    public const uint StatusStartJump = 1 << 16;
    public const uint StatusEndJump = 1 << 17;
    public const uint Status18 = 1 << 18;
    public const uint Status19 = 1 << 19;
    public const uint StatusHideShadow = 1 << 20;
    public const uint Status21 = 1 << 21;
    public const uint Status22 = 1 << 22;
    public const uint StatusHeightCalculationDisabled = 1 << 23;
    public const uint Status24 = 1 << 24;
    public const uint StatusAlias = 1 << 25;
    public const uint Status26 = 1 << 26;
    public const uint StatusDoNotSinkIntoTerrain = 1 << 27;
    public const uint StatusOnElevatedBridge = 1 << 28;
    public const uint StatusDynamicHeightCalculationEnabled = 1 << 29;

    public const uint StatusInitial = StatusActive | Status11 | Status12;
    public const uint StatusClearedOnLoad = 0x00EF4248;
    public const uint StatusTransient = 0x05108000;
    public const uint Status2Bit2 = 1 << 2;

    public const int MovementTypeFollowPlayer = 0x30;
    public const int MovementTypeFollowPartnerTrainer = 0x32;
    public const int MovementTypeCount = 0x44;
    public const int GraphicsIDNone = 0xFFFF;
    public const int GraphicsIDInvisible = 0x2000;
    public const int GraphicsIDVariableFirst = 0x65;
    public const int GraphicsIDVariableLast = 0x74;
    public const int ScriptAlias = 0xFFFF;

    public const int SaveSize = 0x50;
    public const int SaveStatusOffset = 0x0;
    public const int SaveStatus2Offset = 0x4;
    public const int SaveLocalIDOffset = 0x8;
    public const int SaveMovementTypeOffset = 0x9;
    public const int SaveMovementRangeXOffset = 0xA;
    public const int SaveMovementRangeZOffset = 0xB;
    public const int SaveInitialDirOffset = 0xC;
    public const int SaveFacingDirOffset = 0xD;
    public const int SaveMovingDirOffset = 0xE;
    public const int SaveMapHeaderIDOffset = 0x10;
    public const int SaveGraphicsIDOffset = 0x12;
    public const int SaveTrainerTypeOffset = 0x14;
    public const int SaveFlagOffset = 0x16;
    public const int SaveScriptOffset = 0x18;
    public const int SaveDataOffset = 0x1A;
    public const int SaveXInitialOffset = 0x20;
    public const int SaveYInitialOffset = 0x22;
    public const int SaveZInitialOffset = 0x24;
    public const int SaveXOffset = 0x26;
    public const int SaveYOffset = 0x28;
    public const int SaveZOffset = 0x2A;
    public const int SavePosYOffset = 0x2C;
    public const int SaveMovementTypeDataOffset = 0x30;
    public const int SaveTrainerTypeDataOffset = 0x40;

    private const uint MovementFuncsTableAddress = 0x020EE3A8;
    private const uint DrawFuncsTableAddress = 0x021FB97C;
    private const uint DrawFuncsInvisibleAddress = 0x021FB470;
    private const uint DrawFuncsTableEnd = 0xFFFF;
    private const int DrawFuncsOverlayID = 5;

    public readonly MapObject[] Owner;
    public readonly int Index;

    public uint Status;
    public uint Status2;
    public int LocalID;
    public int MapHeaderID;
    public int GraphicsID;
    public int MovementType;
    public int TrainerType;
    public int Flag;
    public int Script;
    public int InitialDir;
    public int FacingDir;
    public int MovingDir;
    public int PrevFacingDir;
    public int PrevMovingDir;
    public readonly int[] Data = new int[3];
    public int MovementRangeX;
    public int MovementRangeZ;
    public int XInitial;
    public int YInitial;
    public int ZInitial;
    public int XPrev;
    public int YPrev;
    public int ZPrev;
    public int X;
    public int Y;
    public int Z;
    public VecFx32 Position;
    public VecFx32 SpriteJumpOffset;
    public VecFx32 SpritePosOffset;
    public VecFx32 SpriteTerrainOffset;
    public int DrawState;
    public int MovementAction;
    public int MovementStep;
    public ushort CurrTileBehavior;
    public ushort PrevTileBehavior;
    public SysTask? Task;
    public MapObjectManager? MapObjectManager;
    public MapObjectFunc? MovementInit;
    public MapObjectFunc? MovementMove;
    public MapObjectFunc? MovementExit;
    public MapObjectFunc? DrawInit;
    public MapObjectFunc? Draw;
    public MapObjectFunc? DrawDelete;
    public MapObjectFunc? DrawSuspend;
    public MapObjectFunc? DrawResume;
    public readonly byte[] MovementTypeData = new byte[MovementTypeDataSize];
    public readonly byte[] TrainerTypeData = new byte[TrainerTypeDataSize];
    public readonly byte[] MovementData = new byte[MovementDataSize];
    public readonly byte[] DrawData = new byte[DrawDataSize];

    private static MapObjectMovementFuncs?[]? movementFuncsTable;
    private static uint[]? drawFuncsTable;
    private static readonly Dictionary<uint, MapObjectDrawFuncs> drawFuncsByAddress = new();

    public MapObject(MapObject[] owner, int index)
    {
        Owner = owner;
        Index = index;
    }

    // OFFSET 0x02061AB4
    public static void MapObject_ChangeGraphics(MapObject mapObject, int graphicsID)
    {
        MapObject_SetGraphicsID(mapObject, graphicsID);
        MapObject_ResetMovementStatus(mapObject);
        MapObject_SetStatusFlagOff(mapObject, StatusDrawInitialized);
        MapObject_InitDraw(mapObject);
    }

    // OFFSET 0x02061AD4
    public static void MapObject_ReplaceGraphics(MapObject mapObject, int graphicsID)
    {
        if (MapObject_IsDrawActive(mapObject) == 1)
        {
            MapObject_ClearDraw(mapObject);
        }
        MapObject_ChangeGraphics(mapObject, graphicsID);
    }

    // OFFSET 0x02061AF4
    public static void MapObject_Delete(MapObject mapObject)
    {
        if (MapObjectMan_IsDrawInitialized(MapObject_MapObjectManager(mapObject)!) == 1)
        {
            MapObject_CallDrawDelete(mapObject);
        }
        MapObject_CallMovementExit(mapObject);
        MapObject_DeleteTask(mapObject);
        MapObjectMan_DecObjectCount(MapObject_MapObjectManagerDeconst(mapObject)!);
        MapObject_Clear(mapObject);
    }

    // OFFSET 0x02061B28
    public static void MapObject_SetFlagAndDeleteObject(MapObject mapObject)
    {
        int flag = MapObject_GetFlag(mapObject);
        FieldSystemVarsFlags.FieldSystem_SetFlag(MapObject_FieldSystem(mapObject)!, (ushort)flag);
        MapObject_Delete(mapObject);
    }

    // OFFSET 0x02061B48
    public static void MapObject_ClearDraw(MapObject mapObject)
    {
        if (MapObjectMan_IsDrawInitialized(MapObject_MapObjectManager(mapObject)!) == 1)
        {
            if (MapObject_CheckStatus(mapObject, StatusDrawInitialized) != 0)
            {
                MapObject_CallDrawDelete(mapObject);
            }
            MapObject_SetStatusFlagOff(mapObject, StatusDrawInitialized);
        }
        MapObject_SetGraphicsID(mapObject, GraphicsIDNone);
        MapObject_SetDrawInitFunc(mapObject, MapObject_DrawInitNone);
        MapObject_SetDrawFunc(mapObject, MapObject_DrawNone);
        MapObject_SetDrawDeleteFunc(mapObject, MapObject_DrawNone);
        MapObject_SetDrawSuspendFunc(mapObject, MapObject_DrawSuspendNone);
        MapObject_SetDrawResumeFunc(mapObject, MapObject_DrawResumeNone);
    }

    // OFFSET 0x02061D3C
    public static void MapObject_Save(FieldSystem fieldSystem, MapObject mapObject, BufferView mapObjectSave)
    {
        mapObjectSave.W32(SaveStatusOffset, MapObject_GetStatus(mapObject));
        mapObjectSave.W32(SaveStatus2Offset, MapObject_GetStatus2(mapObject));
        mapObjectSave.W8(SaveLocalIDOffset, MapObject_GetLocalID(mapObject));
        mapObjectSave.W16(SaveMapHeaderIDOffset, MapObject_GetMapHeaderID(mapObject));
        mapObjectSave.W16(SaveGraphicsIDOffset, MapObject_GetGraphicsID(mapObject));
        mapObjectSave.W8(SaveMovementTypeOffset, MapObject_GetMovementType(mapObject));
        mapObjectSave.W16(SaveTrainerTypeOffset, MapObject_GetTrainerType(mapObject));
        mapObjectSave.W16(SaveFlagOffset, MapObject_GetFlag(mapObject));
        mapObjectSave.W16(SaveScriptOffset, MapObject_GetScript(mapObject));
        mapObjectSave.W8(SaveInitialDirOffset, MapObject_GetInitialDir(mapObject));
        mapObjectSave.W8(SaveFacingDirOffset, MapObject_GetFacingDir(mapObject));
        mapObjectSave.W8(SaveMovingDirOffset, MapObject_GetMovingDir(mapObject));
        mapObjectSave.W16(SaveDataOffset, MapObject_GetDataAt(mapObject, 0));
        mapObjectSave.W16(SaveDataOffset + 2, MapObject_GetDataAt(mapObject, 1));
        mapObjectSave.W16(SaveDataOffset + 4, MapObject_GetDataAt(mapObject, 2));
        mapObjectSave.W8(SaveMovementRangeXOffset, MapObject_GetMovementRangeX(mapObject));
        mapObjectSave.W8(SaveMovementRangeZOffset, MapObject_GetMovementRangeZ(mapObject));
        mapObjectSave.W16(SaveXInitialOffset, MapObject_GetXInitial(mapObject));
        mapObjectSave.W16(SaveYInitialOffset, MapObject_GetYInitial(mapObject));
        mapObjectSave.W16(SaveZInitialOffset, MapObject_GetZInitial(mapObject));
        mapObjectSave.W16(SaveXOffset, MapObject_GetX(mapObject));
        mapObjectSave.W16(SaveYOffset, MapObject_GetY(mapObject));
        mapObjectSave.W16(SaveZOffset, MapObject_GetZ(mapObject));
        VecFx32 position = default;
        MapObjectMove.VecFx32_SetPosFromMapCoords((short)mapObjectSave.U16(SaveXOffset), (short)mapObjectSave.U16(SaveZOffset), ref position);
        position.Y = MapObject_GetPosY(mapObject);
        int dynamicHeight = MapObject_IsDynamicHeightCalculationEnabled(mapObject);
        if (MapObjectMove.MapObject_RecalculatePositionHeightEx(fieldSystem, ref position, dynamicHeight) == 0)
        {
            mapObjectSave.W32(SavePosYOffset, (uint)MapObject_GetPosY(mapObject));
        }
        else
        {
            if (MapObject_IsHeightCalculationDisabled(mapObject) == 1)
            {
                position.Y = MapObject_GetPosY(mapObject);
            }
            mapObjectSave.W32(SavePosYOffset, (uint)position.Y);
        }
        mapObjectSave.CopyFrom(SaveMovementTypeDataOffset, MapObject_GetMovementTypeData(mapObject), 0, MovementTypeDataSize);
        mapObjectSave.CopyFrom(SaveTrainerTypeDataOffset, MapObject_GetTrainerTypeData(mapObject), 0, TrainerTypeDataSize);
    }

    // OFFSET 0x02061E80
    public static void MapObject_LoadSave(MapObject mapObject, BufferView mapObjectSave)
    {
        MapObject_SetStatus(mapObject, mapObjectSave.U32(SaveStatusOffset));
        MapObject_SetStatus2(mapObject, mapObjectSave.U32(SaveStatus2Offset));
        MapObject_SetLocalID(mapObject, mapObjectSave.U8(SaveLocalIDOffset));
        MapObject_SetMapHeaderID(mapObject, mapObjectSave.U16(SaveMapHeaderIDOffset));
        MapObject_SetGraphicsID(mapObject, mapObjectSave.U16(SaveGraphicsIDOffset));
        MapObject_SetMovementType(mapObject, mapObjectSave.U8(SaveMovementTypeOffset));
        MapObject_SetTrainerType(mapObject, mapObjectSave.U16(SaveTrainerTypeOffset));
        MapObject_SetFlag(mapObject, mapObjectSave.U16(SaveFlagOffset));
        MapObject_SetScript(mapObject, mapObjectSave.U16(SaveScriptOffset));
        MapObject_SetInitialDir(mapObject, (sbyte)mapObjectSave.U8(SaveInitialDirOffset));
        MapObject_Face(mapObject, (sbyte)mapObjectSave.U8(SaveFacingDirOffset));
        MapObject_Turn(mapObject, (sbyte)mapObjectSave.U8(SaveMovingDirOffset));
        MapObject_SetDataAt(mapObject, (short)mapObjectSave.U16(SaveDataOffset), 0);
        MapObject_SetDataAt(mapObject, (short)mapObjectSave.U16(SaveDataOffset + 2), 1);
        MapObject_SetDataAt(mapObject, (short)mapObjectSave.U16(SaveDataOffset + 4), 2);
        MapObject_SetMovementRangeX(mapObject, (sbyte)mapObjectSave.U8(SaveMovementRangeXOffset));
        MapObject_SetMovementRangeZ(mapObject, (sbyte)mapObjectSave.U8(SaveMovementRangeZOffset));
        MapObject_SetXInitial(mapObject, (short)mapObjectSave.U16(SaveXInitialOffset));
        MapObject_SetYInitial(mapObject, (short)mapObjectSave.U16(SaveYInitialOffset));
        MapObject_SetZInitial(mapObject, (short)mapObjectSave.U16(SaveZInitialOffset));
        MapObject_SetX(mapObject, (short)mapObjectSave.U16(SaveXOffset));
        MapObject_SetY(mapObject, (short)mapObjectSave.U16(SaveYOffset));
        MapObject_SetZ(mapObject, (short)mapObjectSave.U16(SaveZOffset));
        VecFx32 position = default;
        position.Y = (int)mapObjectSave.U32(SavePosYOffset);
        MapObject_SetPos(mapObject, position);
        mapObjectSave.CopyTo(SaveMovementTypeDataOffset, MapObject_GetMovementTypeData(mapObject), 0, MovementTypeDataSize);
        mapObjectSave.CopyTo(SaveTrainerTypeDataOffset, MapObject_GetTrainerTypeData(mapObject), 0, TrainerTypeDataSize);
    }

    // OFFSET 0x02061FF0
    public static void MapObject_ResetStatusAfterLoad(MapObject mapObject)
    {
        MapObject_SetStatusFlagOn(mapObject, StatusActive | StatusStartMovement);
        MapObject_SetStatusFlagOff(mapObject, StatusClearedOnLoad);
        MapObject_ClearTransientStatus(mapObject);
    }

    // OFFSET 0x02062010
    public static void MapObject_ResetPositionAfterLoad(MapObject mapObject)
    {
        MapObject_CopyPos(mapObject, out VecFx32 position);
        int x = MapObject_GetX(mapObject);
        position.X = TileCenterToFx32(x);
        MapObject_SetXPrev(mapObject, x);
        MapObject_SetYPrev(mapObject, MapObject_GetY(mapObject));
        int z = MapObject_GetZ(mapObject);
        position.Z = TileCenterToFx32(z);
        MapObject_SetZPrev(mapObject, z);
        MapObject_SetPos(mapObject, position);
    }

    // OFFSET 0x020621E8
    public static void MapObject_InitFromObjectEvent(MapObject mapObject, ObjectEvent objectEvent, FieldSystem fieldSystem)
    {
        MapObject_SetLocalID(mapObject, ObjectEvent_GetLocalID(objectEvent));
        MapObject_SetGraphicsID(mapObject, MapObject_GetFieldSystemGraphicsID(fieldSystem, ObjectEvent_GetGraphicsID(objectEvent)));
        MapObject_SetMovementType(mapObject, ObjectEvent_GetMovementType(objectEvent));
        MapObject_SetTrainerType(mapObject, ObjectEvent_GetTrainerType(objectEvent));
        MapObject_SetFlag(mapObject, ObjectEvent_GetHiddenFlag(objectEvent));
        MapObject_SetScript(mapObject, ObjectEvent_GetScript(objectEvent));
        MapObject_SetInitialDir(mapObject, ObjectEvent_GetInitialDir(objectEvent));
        MapObject_SetDataAt(mapObject, ObjectEvent_GetDataAt(objectEvent, 0), 0);
        MapObject_SetDataAt(mapObject, ObjectEvent_GetDataAt(objectEvent, 1), 1);
        MapObject_SetDataAt(mapObject, ObjectEvent_GetDataAt(objectEvent, 2), 2);
        MapObject_SetMovementRangeX(mapObject, ObjectEvent_GetMovementRangeX(objectEvent));
        MapObject_SetMovementRangeZ(mapObject, ObjectEvent_GetMovementRangeZ(objectEvent));
        MapObject_InitPositionFromObjectEvent(mapObject, objectEvent);
    }

    // OFFSET 0x020622B8
    public static void MapObject_InitPositionFromObjectEvent(MapObject mapObject, ObjectEvent objectEvent)
    {
        VecFx32 position;
        int x = ObjectEvent_GetX(objectEvent);
        position.X = TileCenterToFx32(x);
        MapObject_SetXInitial(mapObject, x);
        MapObject_SetXPrev(mapObject, x);
        MapObject_SetX(mapObject, x);
        int y = ObjectEvent_GetY(objectEvent);
        position.Y = y;
        y = Fx32ToInt(y >> 3);
        MapObject_SetYInitial(mapObject, y);
        MapObject_SetYPrev(mapObject, y);
        MapObject_SetY(mapObject, y);
        int z = ObjectEvent_GetZ(objectEvent);
        position.Z = TileCenterToFx32(z);
        MapObject_SetZInitial(mapObject, z);
        MapObject_SetZPrev(mapObject, z);
        MapObject_SetZ(mapObject, z);
        MapObject_SetPos(mapObject, position);
    }

    // OFFSET 0x0206234C
    public static void MapObject_InitStatus(MapObject mapObject, MapObjectManager mapObjectManager)
    {
        MapObject_SetStatusFlagOn(mapObject, StatusInitial);
        if (MapObject_HasNoScript(mapObject) == 1)
        {
            MapObject_SetAlias(mapObject, 1);
        }
        MapObject_SetMapObjectManager(mapObject, mapObjectManager);
        MapObject_Face(mapObject, MapObject_GetInitialDir(mapObject));
        MapObject_Turn(mapObject, MapObject_GetInitialDir(mapObject));
        MovementActions.MapObject_ClearMovementAction(mapObject);
    }

    // OFFSET 0x0206239C
    public static void MapObject_SetMovementFuncs(MapObject mapObject)
    {
        MapObjectMovementFuncs funcs = MapObject_GetMovementFuncs((uint)MapObject_GetMovementType(mapObject))!;
        MapObject_SetMovementInitFunc(mapObject, MapObjectMovementFuncs_GetInit(funcs));
        MapObject_SetMovementMoveFunc(mapObject, MapObjectMovementFuncs_GetMove(funcs));
        MapObject_SetMovementExitFunc(mapObject, MapObjectMovementFuncs_GetExit(funcs));
    }

    // OFFSET 0x020623D4
    public static void MapObject_SetDrawFuncs(MapObject mapObject)
    {
        int graphicsID = MapObject_GetGraphicsID(mapObject);
        MapObjectDrawFuncs funcs;
        if (graphicsID == GraphicsIDInvisible)
        {
            funcs = DrawFuncsAt(DrawFuncsInvisibleAddress);
        }
        else
        {
            funcs = MapObject_GetDrawFuncs((uint)graphicsID)!;
        }
        MapObject_SetDrawInitFunc(mapObject, MapObjectDrawFuncs_GetInit(funcs));
        MapObject_SetDrawFunc(mapObject, MapObjectDrawFuncs_GetDraw(funcs));
        MapObject_SetDrawDeleteFunc(mapObject, MapObjectDrawFuncs_GetDelete(funcs));
        MapObject_SetDrawSuspendFunc(mapObject, MapObjectDrawFuncs_GetSuspend(funcs));
        MapObject_SetDrawResumeFunc(mapObject, MapObjectDrawFuncs_GetResume(funcs));
    }

    // OFFSET 0x0206243C
    public static void MapObject_Clear(MapObject mapObject)
    {
        mapObject.Status = 0;
        mapObject.Status2 = 0;
        mapObject.LocalID = 0;
        mapObject.MapHeaderID = 0;
        mapObject.GraphicsID = 0;
        mapObject.MovementType = 0;
        mapObject.TrainerType = 0;
        mapObject.Flag = 0;
        mapObject.Script = 0;
        mapObject.InitialDir = 0;
        mapObject.FacingDir = 0;
        mapObject.MovingDir = 0;
        mapObject.PrevFacingDir = 0;
        mapObject.PrevMovingDir = 0;
        Array.Clear(mapObject.Data);
        mapObject.MovementRangeX = 0;
        mapObject.MovementRangeZ = 0;
        mapObject.XInitial = 0;
        mapObject.YInitial = 0;
        mapObject.ZInitial = 0;
        mapObject.XPrev = 0;
        mapObject.YPrev = 0;
        mapObject.ZPrev = 0;
        mapObject.X = 0;
        mapObject.Y = 0;
        mapObject.Z = 0;
        mapObject.Position = default;
        mapObject.SpriteJumpOffset = default;
        mapObject.SpritePosOffset = default;
        mapObject.SpriteTerrainOffset = default;
        mapObject.DrawState = 0;
        mapObject.MovementAction = 0;
        mapObject.MovementStep = 0;
        mapObject.CurrTileBehavior = 0;
        mapObject.PrevTileBehavior = 0;
        mapObject.Task = null;
        mapObject.MapObjectManager = null;
        mapObject.MovementInit = null;
        mapObject.MovementMove = null;
        mapObject.MovementExit = null;
        mapObject.DrawInit = null;
        mapObject.Draw = null;
        mapObject.DrawDelete = null;
        mapObject.DrawSuspend = null;
        mapObject.DrawResume = null;
        Array.Clear(mapObject.MovementTypeData);
        Array.Clear(mapObject.TrainerTypeData);
        Array.Clear(mapObject.MovementData);
        Array.Clear(mapObject.DrawData);
    }

    // OFFSET 0x0206244C
    public static int MapObject_MatchObjectEvents(MapObject mapObject, int mapHeaderID, int numObjectEvents, ObjectEvent[]? objectEvents)
    {
        for (int i = 0; i < numObjectEvents; i++)
        {
            ObjectEvent objectEvent = objectEvents![i];
            if (ObjectEvent_GetLocalID(objectEvent) != MapObject_GetLocalID(mapObject))
            {
                continue;
            }
            if (ObjectEvent_HasNoScript(objectEvent) == 1)
            {
                int aliasMapHeaderID = ObjectEvent_GetAliasMapHeaderID(objectEvent);
                if (MapObject_IsAlias(mapObject) == 1)
                {
                    if (aliasMapHeaderID == MapObject_GetAliasMapHeaderID(mapObject))
                    {
                        return 1;
                    }
                }
                else if (aliasMapHeaderID == MapObject_GetMapHeaderID(mapObject))
                {
                    return 2;
                }
            }
            else if (MapObject_IsAlias(mapObject) == 1 && MapObject_GetAliasMapHeaderID(mapObject) == mapHeaderID)
            {
                return 2;
            }
        }
        return 0;
    }

    // OFFSET 0x02062604
    public static void MapObject_ResetMovementStatus(MapObject mapObject)
    {
        MapObject_SetStatusFlagOn(mapObject, StatusStartMovement);
        MapObject_ClearTransientStatus(mapObject);
    }

    // OFFSET 0x02062618
    public static void MapObject_ClearTransientStatus(MapObject mapObject)
    {
        MapObject_SetStatusFlagOff(mapObject, StatusTransient);
    }

    // OFFSET 0x02062628
    public static void MapObject_AfterDrawSuspend(MapObject mapObject)
    {
    }

    // OFFSET 0x0206262C
    public static int MapObject_GetFieldSystemGraphicsID(FieldSystem fieldSystem, int graphicsID)
    {
        if (graphicsID >= GraphicsIDVariableFirst && graphicsID <= GraphicsIDVariableLast)
        {
            graphicsID = FieldSystemVarsFlags.FieldSystem_GetGraphicsID(fieldSystem, (ushort)(graphicsID - GraphicsIDVariableFirst));
        }
        return graphicsID;
    }

    // OFFSET 0x02062648
    public static void MapObject_UpdateIfStatus12(MapObject mapObject)
    {
        if (MapObject_CheckStatus(mapObject, Status12) != 0)
        {
            MapObjectMove.MapObject_RecalculateObjectHeight(mapObject);
        }
    }

    // OFFSET 0x02062660
    public static void MapObject_InitMovement(MapObject mapObject)
    {
        MapObject_SetMovementFuncs(mapObject);
        MapObjectMove.MapObject_InitMove(mapObject);
    }

    // OFFSET 0x02062670
    public static void MapObject_InitDraw(MapObject mapObject)
    {
        if (MapObjectMan_IsDrawInitialized(MapObject_MapObjectManager(mapObject)!) == 0)
        {
            return;
        }
        MapObject_UpdateIfStatus12(mapObject);
        MapObject_SetDrawState(mapObject, 0);
        MapObjectRenderResources.MapObject_SetBillboardPending(mapObject, 0);
        if (MapObject_IsDrawInitialized(mapObject) == 0)
        {
            MapObject_SetDrawFuncs(mapObject);
            MapObject_CallDrawInit(mapObject);
            MapObject_SetDrawInitialized(mapObject);
        }
    }

    // OFFSET 0x020626B4
    public static int MapObject_HasNoScript(MapObject mapObject)
    {
        if ((ushort)MapObject_GetScript(mapObject) == ScriptAlias)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020626D0
    public static void MapObject_ConvertAliasToLocal(MapObject mapObject, ObjectEvent objectEvent, int mapHeaderID)
    {
        if (MapObject_IsAlias(mapObject) != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObject_SetAlias(mapObject, 0);
        MapObject_SetMapHeaderID(mapObject, mapHeaderID);
        MapObject_SetScript(mapObject, ObjectEvent_GetScript(objectEvent));
        MapObject_SetFlag(mapObject, ObjectEvent_GetHiddenFlag(objectEvent));
    }

    // OFFSET 0x02062714
    public static void MapObject_ConvertLocalToAlias(MapObject mapObject, int mapHeaderID, ObjectEvent objectEvent)
    {
        if (ObjectEvent_HasNoScript(objectEvent) != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObject_SetAlias(mapObject, 1);
        MapObject_SetScript(mapObject, ObjectEvent_GetScript(objectEvent));
        MapObject_SetFlag(mapObject, ObjectEvent_GetAliasMapHeaderID(objectEvent));
        MapObject_SetMapHeaderID(mapObject, mapHeaderID);
    }

    // OFFSET 0x02062758
    public static int MapObject_CalculateTaskPriority(MapObject mapObject, int priority)
    {
        return MapObject_GetTaskBasePriority(mapObject) + priority;
    }

    // OFFSET 0x02062764
    public static int MapObject_IsLocalObjectOfMap(MapObject mapObject, int localID, int mapHeaderID)
    {
        if (!MapObject_CheckStatusFlag(mapObject, StatusActive))
        {
            return 0;
        }
        if (MapObject_GetLocalID(mapObject) != localID)
        {
            return 0;
        }
        if (MapObject_GetMapHeaderID(mapObject) != mapHeaderID)
        {
            if (MapObject_IsAlias(mapObject) == 0)
            {
                return 0;
            }
            if (MapObject_GetAliasMapHeaderID(mapObject) != mapHeaderID)
            {
                return 0;
            }
        }
        return 1;
    }

    // OFFSET 0x020627B4
    public static int MapObject_IsLocalObjectOfMapWithGraphics(MapObject mapObject, int graphicsID, int localID, int mapHeaderID)
    {
        if (!MapObject_CheckStatusFlag(mapObject, StatusActive))
        {
            return 0;
        }
        if (MapObject_GetEffectiveGraphicsID(mapObject) != graphicsID)
        {
            return 0;
        }
        return MapObject_IsLocalObjectOfMap(mapObject, localID, mapHeaderID);
    }

    // OFFSET 0x020627E8
    public static void MapObjectTask_Move(SysTask task, object? param)
    {
        MapObject mapObject = (MapObject)param!;
        MapObjectMove.MapObject_Move(mapObject);
        if (MapObject_IsActive(mapObject) != 0)
        {
            MapObjectTask_Draw(mapObject);
        }
    }

    // OFFSET 0x02062804
    private static void MapObjectTask_Draw(MapObject mapObject)
    {
        if (MapObjectMan_IsDrawInitialized(MapObject_MapObjectManager(mapObject)!) == 1)
        {
            MapObjectRender.MapObject_Draw(mapObject);
        }
    }

    private static int TileCenterToFx32(int tile)
    {
        return (tile << 16) + 0x8000;
    }

    private static int Fx32ToInt(int value)
    {
        return (value + (int)((uint)(value >> 11) >> 20)) >> 12;
    }

    private static MapObjectFunc ResolveCallback(uint address)
    {
        switch (address & ~1u)
        {
            case 0x020633E0:
                return MapObject_MovementInitNone;
            case 0x020633E4:
                return MapObject_MovementMoveNone;
            case 0x020633E8:
                return MapObject_MovementExitNone;
            case 0x020633EC:
                return MapObject_MovementReturnNone;
            case 0x020633F0:
                return MapObject_DrawInitNone;
            case 0x020633F4:
                return MapObject_DrawNone;
            case 0x020633F8:
                return MapObject_DrawSuspendNone;
            case 0x020633FC:
                return MapObject_DrawResumeNone;
            default:
                return mapObject =>
                {
                    if (!MapObjectDrawHandlers.Dispatch(address, mapObject) && !MapObjectMovementTypes.Dispatch(address, mapObject) && !MapObjectFollowMovement.Dispatch(address, mapObject))
                    {
                        MapObjectCallback(address, mapObject);
                    }
                };
        }
    }

    private static MapObjectDrawFuncs DrawFuncsAt(uint address)
    {
        if (!drawFuncsByAddress.TryGetValue(address, out MapObjectDrawFuncs? funcs))
        {
            uint[] words = RomData.OverlayU32(DrawFuncsOverlayID, address, MapObjectDrawFuncs.Size / 4);
            funcs = new MapObjectDrawFuncs
            {
                Init = ResolveCallback(words[0]),
                Draw = ResolveCallback(words[1]),
                Delete = ResolveCallback(words[2]),
                Suspend = ResolveCallback(words[3]),
                Resume = ResolveCallback(words[4]),
            };
            drawFuncsByAddress[address] = funcs;
        }
        return funcs;
    }
}
