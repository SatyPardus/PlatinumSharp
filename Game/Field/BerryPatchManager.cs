using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class BerryPatchManager
{
    public int HeapID;
    public byte[]? GrowthData;
    public readonly G3dRenderObj RenderObj = new();
    public G3dModel? Model;
    public byte[]? ModelResource;

    private const int HeapIDField = 0xB;
    private const uint NarcMemberBerryPatchModel = 0x11;
    private const uint ViewTestScaleAddress = 0x020EC52C;
    private const uint AnimWalkWestAddress = 0x020EC51C;
    private const uint AnimWalkEastAddress = 0x020EC524;
    private const int AnimCmdListSize = 8;

    private const int ItemToBerryOffset = 0x94;
    private const int ItemToMulchOffset = 0x5E;

    private const int DirNorth = 0;
    private const int DirSouth = 1;
    private const int DirWest = 2;
    private const int DirEast = 3;

    private const int PatchFlagEmpty = 1;
    private const int PatchFlagNoMulch = 2;
    private const int PatchFlagPlanted = 4;

    private const uint PlayerRequestStateWateringCan = 0x10;
    private const int WateringIdleFrames = 0x5A;

    private const int StateInit = 0;
    private const int StateWater = 1;
    private const int StateWaitInput = 2;
    private const int StateWaitMove = 3;
    private const int StateEnd = 4;

    private sealed class BerryWateringTask
    {
        public int State;
        public int Direction;
        public int Timer;
        public SysTask? AnimationTask;
    }

    // OFFSET 0x02055C50
    private static ushort BerryPatches_ConvertTagNumberToItemID(int tagNumber)
    {
        if (tagNumber == 0)
        {
            return 0;
        }
        return (ushort)(tagNumber + ItemToBerryOffset);
    }

    // OFFSET 0x02055C60
    private static ushort BerryPatches_ConvertItemIDToTagNumber(int itemID)
    {
        if (itemID == 0)
        {
            return 0;
        }
        return (ushort)(itemID - ItemToBerryOffset);
    }

    // OFFSET 0x02055C70
    private static ushort BerryPatches_ConvertMulchTypeToItemID(int mulchType)
    {
        if (mulchType == 0)
        {
            return 0;
        }
        return (ushort)(mulchType + ItemToMulchOffset);
    }

    // OFFSET 0x02055C80
    private static int BerryPatches_ConvertItemIDToMulchType(int itemID)
    {
        if (itemID == 0)
        {
            return 0;
        }
        return itemID - ItemToMulchOffset;
    }

    // OFFSET 0x02055C8C
    public static BerryPatchManager BerryPatchManager_New(FieldSystem fieldSystem, int heapID)
    {
        BerryPatchManager manager = new();
        manager.HeapID = heapID;
        manager.GrowthData = BerryPatches.BerryGrowthData_Init(heapID);
        BerryPatchManager_Init3DRendering(fieldSystem, manager);
        return manager;
    }

    // OFFSET 0x02055CBC
    public static void BerryPatchManager_Free(BerryPatchManager manager)
    {
        BerryPatchManager_Cleanup3DRendering(manager);
        manager.GrowthData = null;
    }

    // OFFSET 0x02055CD4
    public static void BerryPatches_ElapseTime(FieldSystem fieldSystem, int minutes)
    {
        if (fieldSystem.FieldMapContext == null)
        {
            byte[] growthData = BerryPatches.BerryGrowthData_Init(HeapIDField);
            BerryPatches.BerryPatches_ElapseMinutes(MiscSaveBlock.SaveData_GetBerryPatches(fieldSystem.SaveData!), growthData, minutes);
            return;
        }
        BerryPatchManager manager = fieldSystem.FieldMapContext.BerryPatchManager!;
        BerryPatches.BerryPatches_ElapseMinutes(MiscSaveBlock.SaveData_GetBerryPatches(fieldSystem.SaveData!), manager.GrowthData!, minutes);
    }

    // OFFSET 0x02055D14
    private static void BerryPatchManager_Init3DRendering(FieldSystem fieldSystem, BerryPatchManager manager)
    {
        FieldEffectManager fieldEffectManager = fieldSystem.FieldEffectManager!;
        uint size = FieldEffects.FieldEffectManager_GetNARCMemberSize(fieldEffectManager, NarcMemberBerryPatchModel);
        manager.ModelResource = Heap.Heap_Alloc(manager.HeapID, size);
        FieldEffects.FieldEffectManager_ReadNARCWholeMember(fieldEffectManager, NarcMemberBerryPatchModel, manager.ModelResource!);
        Easy3D.Easy3D_InitRenderObjFromResource(manager.RenderObj, out manager.Model, manager.ModelResource!);
    }

    // OFFSET 0x02055D48
    private static void BerryPatchManager_Cleanup3DRendering(BerryPatchManager manager)
    {
        FieldEffects.FieldEffectManager_HeapFree(manager.ModelResource!);
        manager.ModelResource = null;
    }

    // OFFSET 0x02055D54
    private static bool BerryPatches_IsInView(FieldSystem fieldSystem, in VecFx32 position)
    {
        uint[] scaleValues = RomData.Arm9U32(ViewTestScaleAddress, 3);
        VecFx32 scale;
        scale.X = (int)scaleValues[0];
        scale.Y = (int)scaleValues[1];
        scale.Z = (int)scaleValues[2];
        Mtx.Identity33(out MtxFx33 rotation);
        BerryPatchManager manager = fieldSystem.FieldMapContext!.BerryPatchManager!;
        if (GfxBoxTest.GFXBoxTest_IsModelInView(manager.Model!, position, rotation, scale) != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02055D94
    public static void BerryPatches_UpdateGrowthStates(FieldSystem fieldSystem)
    {
        int index = 0;
        BerryPatch[] patches = MiscSaveBlock.SaveData_GetBerryPatches(fieldSystem.SaveData!);
        while (MapObjectManager.MapObjectMan_FindObjectWithStatus(fieldSystem.MapObjectManager!, out MapObject? mapObject, ref index, MapObject.StatusActive) == 1)
        {
            if (BerryPatchGraphics.BerryPatchGraphics_IsBerryPatch(MapObject.MapObject_GetGraphicsID(mapObject!)) == 1
                && BerryPatches_IsInView(fieldSystem, MapObject.MapObject_GetPos(mapObject!)))
            {
                BerryPatches.BerryPatches_SetGrowthActive(patches, MapObject.MapObject_GetDataAt(mapObject!, 0), 1);
            }
        }
    }

    // OFFSET 0x02055E00
    public static bool BerryPatches_HarvestBerry(FieldSystem fieldSystem, MapObject mapObject)
    {
        BerryPatch[] patches = MiscSaveBlock.SaveData_GetBerryPatches(fieldSystem.SaveData!);
        int patchID = MapObject.MapObject_GetDataAt(mapObject, 0);
        int berryID = BerryPatches.BerryPatches_BerryID(patches, patchID);
        int yield = BerryPatches.BerryPatches_Yield(patches, patchID);
        ushort itemID = BerryPatches_ConvertTagNumberToItemID(berryID);
        int yieldRating = BerryPatches.BerryPatches_YieldRating(patches, patchID);
        sub_0206D914(fieldSystem, itemID, (byte)yieldRating, (ushort)yield);
        BerryPatches.BerryPatches_Harvest(patches, patchID);
        BerryPatchGraphics.BerryPatchGraphics_MarkForUpdate(mapObject);
        Bag bag = BagItems.SaveData_GetBag(fieldSystem.SaveData!);
        return BagItems.Bag_TryAddItem(bag, BerryPatches_ConvertTagNumberToItemID(berryID), (ushort)yield, 4);
    }

    // OFFSET 0x02055E80
    public static void BerryPatches_SetMulchType(FieldSystem fieldSystem, MapObject mapObject, ushort mulchItemID)
    {
        BerryPatch[] patches = MiscSaveBlock.SaveData_GetBerryPatches(fieldSystem.SaveData!);
        int patchID = MapObject.MapObject_GetDataAt(mapObject, 0);
        BerryPatches.BerryPatches_SetMulch(patches, patchID, BerryPatches_ConvertItemIDToMulchType(mulchItemID));
    }

    // OFFSET 0x02055EAC
    public static void BerryPatches_PlantBerry(FieldSystem fieldSystem, MapObject mapObject, ushort berryItemID)
    {
        BerryPatch[] patches = MiscSaveBlock.SaveData_GetBerryPatches(fieldSystem.SaveData!);
        int patchID = MapObject.MapObject_GetDataAt(mapObject, 0);
        int berryID = BerryPatches_ConvertItemIDToTagNumber(berryItemID);
        BerryPatchManager manager = fieldSystem.FieldMapContext!.BerryPatchManager!;
        BerryPatches.BerryPatches_PlantBerry(patches, patchID, manager.GrowthData!, berryID);
    }

    // OFFSET 0x02055EE0
    public static void BerryPatches_ResetMoisture(FieldSystem fieldSystem, MapObject mapObject)
    {
        BerryPatch[] patches = MiscSaveBlock.SaveData_GetBerryPatches(fieldSystem.SaveData!);
        BerryPatches.BerryPatches_Water(patches, MapObject.MapObject_GetDataAt(mapObject, 0));
    }

    // OFFSET 0x02055F00
    public static int BerryPatches_GetGrowthStage(FieldSystem fieldSystem, MapObject mapObject)
    {
        BerryPatch[] patches = MiscSaveBlock.SaveData_GetBerryPatches(fieldSystem.SaveData!);
        return BerryPatches.BerryPatches_GrowthStage(patches, MapObject.MapObject_GetDataAt(mapObject, 0));
    }

    // OFFSET 0x02055F20
    public static int BerryPatches_GetBerryID(FieldSystem fieldSystem, MapObject mapObject)
    {
        BerryPatch[] patches = MiscSaveBlock.SaveData_GetBerryPatches(fieldSystem.SaveData!);
        return BerryPatches.BerryPatches_BerryID(patches, MapObject.MapObject_GetDataAt(mapObject, 0));
    }

    // OFFSET 0x02055F40
    public static ushort BerryPatches_GetItemID(FieldSystem fieldSystem, MapObject mapObject)
    {
        BerryPatch[] patches = MiscSaveBlock.SaveData_GetBerryPatches(fieldSystem.SaveData!);
        return BerryPatches_ConvertTagNumberToItemID(BerryPatches.BerryPatches_BerryID(patches, MapObject.MapObject_GetDataAt(mapObject, 0)));
    }

    // OFFSET 0x02055F64
    public static ushort BerryPatches_GetMulchItemID(FieldSystem fieldSystem, MapObject mapObject)
    {
        BerryPatch[] patches = MiscSaveBlock.SaveData_GetBerryPatches(fieldSystem.SaveData!);
        return BerryPatches_ConvertMulchTypeToItemID(BerryPatches.BerryPatches_Mulch(patches, MapObject.MapObject_GetDataAt(mapObject, 0)));
    }

    // OFFSET 0x02055F88
    public static int BerryPatches_GetMoisture(FieldSystem fieldSystem, MapObject mapObject)
    {
        BerryPatch[] patches = MiscSaveBlock.SaveData_GetBerryPatches(fieldSystem.SaveData!);
        return BerryPatches.BerryPatches_MoistureLevel(patches, MapObject.MapObject_GetDataAt(mapObject, 0));
    }

    // OFFSET 0x02055FA8
    public static int BerryPatches_GetYield(FieldSystem fieldSystem, MapObject mapObject)
    {
        BerryPatch[] patches = MiscSaveBlock.SaveData_GetBerryPatches(fieldSystem.SaveData!);
        return BerryPatches.BerryPatches_Yield(patches, MapObject.MapObject_GetDataAt(mapObject, 0));
    }

    // OFFSET 0x02055FC8
    public static uint BerryPatches_GetPatchFlags(FieldSystem fieldSystem, MapObject? mapObject)
    {
        uint flags = 0;
        if (mapObject == null || MapObject.MapObject_GetGraphicsID(mapObject) != BerryPatchGraphics.GraphicsIDBerryPatch)
        {
            return 0;
        }
        if (BerryPatches_GetGrowthStage(fieldSystem, mapObject) == 0)
        {
            flags |= PatchFlagEmpty;
            if (BerryPatches_GetMulchItemID(fieldSystem, mapObject) == 0)
            {
                flags |= PatchFlagNoMulch;
            }
        }
        else
        {
            flags |= PatchFlagPlanted;
        }
        return flags;
    }

    // OFFSET 0x02056010
    private static bool BerryPatches_CheckCollision(FieldSystem fieldSystem, BerryWateringTask task, int direction)
    {
        int x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        int z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        if (direction == DirWest)
        {
            x--;
        }
        else if (direction == DirEast)
        {
            x++;
        }
        else if (direction == DirNorth)
        {
            z--;
        }
        else if (direction == DirSouth)
        {
            z++;
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        if (TerrainCollisionManager.TerrainCollisionManager_CheckCollision(fieldSystem, x, z) != 0)
        {
            return true;
        }
        if (MapObjectManager.MapObjectMan_FindObjectAtTile(fieldSystem.MapObjectManager!, x, z, 0) != null)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02056074
    private static MapObject? BerryPatches_GetAdjacentObject(FieldSystem fieldSystem, int direction)
    {
        int x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        int z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        if (direction == DirWest)
        {
            x--;
        }
        else if (direction == DirEast)
        {
            x++;
        }
        return MapObjectManager.MapObjectMan_FindObjectAtTile(fieldSystem.MapObjectManager!, x, z - 1, 0);
    }

    // OFFSET 0x020560A8
    private static MapObject? BerryPatches_GetTargetPatch(FieldSystem fieldSystem, BerryWateringTask task)
    {
        int x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        int z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        if (task.Direction == DirNorth)
        {
            z--;
        }
        else if (task.Direction == DirSouth)
        {
            z++;
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        return MapObjectManager.MapObjectMan_FindObjectAtTile(fieldSystem.MapObjectManager!, x, z, 0);
    }

    // OFFSET 0x020560E4
    private static bool BerryPatches_IsBerryPatch(MapObject mapObject)
    {
        if (MapObject.MapObject_GetGraphicsID(mapObject) == BerryPatchGraphics.GraphicsIDBerryPatch)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020560F8
    private static void BerryPatches_WaterPatch(FieldSystem fieldSystem, BerryWateringTask task)
    {
        MapObject? mapObject = BerryPatches_GetTargetPatch(fieldSystem, task);
        if (mapObject != null)
        {
            BerryPatches_ResetMoisture(fieldSystem, mapObject);
        }
    }

    // OFFSET 0x0205610C
    private static void BerryPatches_StartAnimation(FieldSystem fieldSystem, BerryWateringTask task, uint animCmdAddress)
    {
        MapObject playerObject = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
        BufferView animCmd = new(RomData.Arm9(animCmdAddress, AnimCmdListSize), 0);
        task.AnimationTask = MovementActions.MapObject_StartAnimation(playerObject, animCmd);
    }

    // OFFSET 0x02056124
    private static bool BerryPatches_TaskMain(FieldTask fieldTask)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(fieldTask);
        BerryWateringTask task = (BerryWateringTask)FieldTask.FieldTask_GetEnv(fieldTask)!;
        switch (task.State)
        {
            case StateInit:
                PlayerAvatarRequestState.PlayerAvatar_RequestState(fieldSystem.PlayerAvatar!, PlayerRequestStateWateringCan);
                PlayerAvatarRequestState.PlayerAvatar_ProcessRequestedStates(fieldSystem.PlayerAvatar!);
                MapObject.MapObject_SetPauseMovementOff(PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!));
                task.State = StateWater;
                break;
            case StateWater:
                BerryPatches_WaterPatch(fieldSystem, task);
                task.Timer = 0;
                task.State = StateWaitInput;
                BerryPatches_TaskWaitInput(fieldSystem, task);
                break;
            case StateWaitInput:
                BerryPatches_TaskWaitInput(fieldSystem, task);
                break;
            case StateWaitMove:
                if (MovementActions.MapObject_HasAnimationEnded(task.AnimationTask!) != 0)
                {
                    MovementActions.MapObject_FinishAnimation(task.AnimationTask!);
                    MapObject? patch = BerryPatches_GetTargetPatch(fieldSystem, task);
                    if (patch != null && BerryPatches_IsBerryPatch(patch))
                    {
                        task.State = StateWater;
                    }
                    else
                    {
                        PlayerAvatar.PlayerAvatar_TryFace(fieldSystem.PlayerAvatar!, task.Direction);
                        task.State = StateEnd;
                    }
                }
                break;
            case StateEnd:
                PlayerAvatar.PlayerAvatar_TryFace(fieldSystem.PlayerAvatar!, task.Direction);
                MapObject.MapObject_SetPauseMovementOn(PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!));
                return true;
        }
        return false;
    }

    private static void BerryPatches_TaskWaitInput(FieldSystem fieldSystem, BerryWateringTask task)
    {
        int heldKeys = Globals.gSystem.HeldKeys;
        if ((heldKeys & Input.KeyLeft) != 0)
        {
            if (BerryPatches_TaskTryMove(fieldSystem, task, DirWest, AnimWalkWestAddress))
            {
                return;
            }
        }
        else if ((heldKeys & Input.KeyRight) != 0)
        {
            if (BerryPatches_TaskTryMove(fieldSystem, task, DirEast, AnimWalkEastAddress))
            {
                return;
            }
        }
        else if ((heldKeys & Input.KeyUp) != 0 && task.Direction == DirSouth)
        {
            PlayerAvatar.PlayerAvatar_TryFace(fieldSystem.PlayerAvatar!, DirNorth);
            task.State = StateEnd;
            return;
        }
        else if ((heldKeys & Input.KeyDown) != 0 && task.Direction == DirNorth)
        {
            task.State = StateEnd;
            return;
        }
        task.Timer++;
        if (task.Timer > WateringIdleFrames)
        {
            PlayerAvatar.PlayerAvatar_TryFace(fieldSystem.PlayerAvatar!, task.Direction);
            task.State = StateEnd;
        }
    }

    private static bool BerryPatches_TaskTryMove(FieldSystem fieldSystem, BerryWateringTask task, int direction, uint animCmdAddress)
    {
        MapObject? adjacent = BerryPatches_GetAdjacentObject(fieldSystem, direction);
        if (adjacent == null || !BerryPatches_IsBerryPatch(adjacent))
        {
            task.State = StateEnd;
            return true;
        }
        if (!BerryPatches_CheckCollision(fieldSystem, task, direction))
        {
            BerryPatches_StartAnimation(fieldSystem, task, animCmdAddress);
            task.State = StateWaitMove;
            return true;
        }
        return false;
    }

    // OFFSET 0x020562AC
    public static void BerryPatches_StartWatering(FieldSystem fieldSystem)
    {
        BerryWateringTask task = new();
        task.State = StateInit;
        task.AnimationTask = null;
        task.Direction = PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!);
        FieldTask.FieldTask_InitCall(fieldSystem.Task!, BerryPatches_TaskMain, task);
    }

    // OFFSET 0x020562D8
    public static void BerryPatches_EndWatering(FieldSystem fieldSystem)
    {
        uint transition = PlayerAvatar.Player_ConvertStateToTransition(PlayerAvatar.PlayerAvatar_GetPlayerState(fieldSystem.PlayerAvatar!));
        PlayerAvatarRequestState.PlayerAvatar_RequestState(fieldSystem.PlayerAvatar!, transition);
        PlayerAvatarRequestState.PlayerAvatar_ProcessRequestedStates(fieldSystem.PlayerAvatar!);
    }
}
