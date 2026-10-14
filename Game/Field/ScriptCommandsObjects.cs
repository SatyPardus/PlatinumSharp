using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const int LocalIDPartner = 0xF2;
    private const int LocalIDCameraObject = 0xF1;
    private const int LocalIDFollower = 0xFE;
    private const int MovementTypeFollowPlayer = 0x30;
    private const int CameraObjectGraphicsID = 0x2000;
    private const int LocationMapIdOffset = 0x00;
    private const int WaitPlayer = 1;
    private const int WaitPartner = 2;
    private const int WaitTarget = 4;
    private const int WaitTargetLinked = 8;

    private static byte sObjectsToWaitFor;

    private sealed class MovementTaskData
    {
        public SysTask? Task;
        public SysTask AnimationTask = null!;
        public byte[]? AnimationList;
        public FieldSystem FieldSystem = null!;
    }

    private static void RegisterObjects(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x02040D74] = ScrCmd_ApplyMovement;
        ported[0x02040EFC] = ScrCmd_WaitMovement;
        ported[0x02040FA4] = ScrCmd_LockAll;
        ported[0x020411C4] = ScrCmd_ReleaseAll;
        ported[0x020411D4] = ScrCmd_LockObject;
        ported[0x020411F0] = ScrCmd_ReleaseObject;
        ported[0x0204120C] = ScrCmd_AddObject;
        ported[0x02041254] = ScrCmd_RemoveObject;
        ported[0x02041364] = ScrCmd_AddCameraObject;
        ported[0x020413D8] = ScrCmd_RemoveCameraObject;
        ported[0x020413F0] = ScrCmd_FacePlayer;
        ported[0x02041420] = ScrCmd_GetPlayerMapPos;
        ported[0x02041464] = ScrCmd_GetObjectMapPos;
        ported[0x020414C4] = ScrCmd_GetPlayerDir;
        ported[0x020415D0] = ScrCmd_SetObjectPersistent;
        ported[0x02041604] = ScrCmd_SetObjectMovementType;
        ported[0x0204163C] = ScrCmd_GetObjectMovementType;
        ported[0x02041684] = ScrCmd_RestartPartner;
        ported[0x0204169C] = ScrCmd_CountUniqueSeals;
        ported[0x020416C8] = ScrCmd_CountSeal;
        ported[0x02041708] = ScrCmd_GiveOrTakeSeal;
    }

    // OFFSET 0x02040D74
    private static bool ScrCmd_ApplyMovement(ScriptContext ctx)
    {
        ushort localID = VarValue(ctx);
        int offset = (int)ScriptContext.ScriptContext_ReadWord(ctx);
        MapObject? mapObject = FieldSystem_GetMapObject(ctx.FieldSystem, localID);
        if (mapObject == null)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        SysTask animationTask = MovementActions.MapObject_StartAnimation(mapObject, new BufferView(ctx.Scripts!, ctx.ScriptPtr + offset));
        Manager(ctx).MovementCount++;
        ScriptContext_StartMovementTask(ctx.FieldSystem, animationTask, null);
        return false;
    }

    // OFFSET 0x02040ED4
    private static MapObject? FieldSystem_GetMapObject(FieldSystem fieldSystem, int localID)
    {
        if (localID == LocalIDPartner)
        {
            return MapObjectManager.MapObjMan_GetLocalMapObjByMovementType(fieldSystem.MapObjectManager!, MovementTypeFollowPlayer);
        }
        if (localID == LocalIDCameraObject)
        {
            return ScriptManager.FieldSystem_GetScriptManager(fieldSystem).CameraObject;
        }
        return MapObjectManager.MapObjMan_LocalMapObjByIndex(fieldSystem.MapObjectManager, localID);
    }

    // OFFSET 0x02040EFC
    private static bool ScrCmd_WaitMovement(ScriptContext ctx)
    {
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMovement);
        return true;
    }

    // OFFSET 0x02040F0C
    private static bool ScriptContext_WaitForMovement(ScriptContext ctx)
    {
        if (Manager(ctx).MovementCount == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02040F28
    private static void ScriptContext_StartMovementTask(FieldSystem fieldSystem, SysTask animationTask, byte[]? animationList)
    {
        MovementTaskData data = new();
        data.FieldSystem = fieldSystem;
        data.AnimationTask = animationTask;
        data.AnimationList = animationList;
        data.Task = SysTask.SysTask_Start(SysTask_WaitForMovement, data, 0);
    }

    // OFFSET 0x02040F5C
    private static void SysTask_WaitForMovement(SysTask task, object? param)
    {
        MovementTaskData data = (MovementTaskData)param!;
        ScriptManager scriptManager = ScriptManager.FieldSystem_GetScriptManager(data.FieldSystem);
        if (MovementActions.MapObject_HasAnimationEnded(data.AnimationTask) == 1)
        {
            MovementActions.MapObject_FinishAnimation(data.AnimationTask);
            SysTask.SysTask_Done(data.Task!);
            if (data.AnimationList != null)
            {
                Heap.Heap_Free(data.AnimationList);
            }
            if (scriptManager.MovementCount == 0)
            {
                ErrorHandling.GF_AssertFail();
                return;
            }
            scriptManager.MovementCount--;
        }
    }

    // OFFSET 0x02040FA4
    private static bool ScrCmd_LockAll(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        if (Manager(ctx).TargetObject == null)
        {
            MapObjectManager.MapObjectMan_PauseAllMovement(fieldSystem.MapObjectManager!);
            MapObject? partner = MapObjectManager.MapObjMan_GetLocalMapObjByMovementType(fieldSystem.MapObjectManager!, MovementTypeFollowPlayer);
            if (partner != null
                && SystemFlags.SystemFlag_CheckHasPartner(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!))
                && MapObject.MapObject_IsMoving(partner) != 0)
            {
                MapObject.MapObject_SetPauseMovementOff(partner);
                ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForPartnerToStop);
                return true;
            }
        }
        else
        {
            ScriptContext_LockAllWithTarget(ctx);
        }
        return true;
    }

    // OFFSET 0x02041004
    private static bool ScriptContext_WaitForObjectsToStop(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ScriptManager scriptManager = Manager(ctx);
        MapObject player = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
        if ((sObjectsToWaitFor & WaitPlayer) != 0 && MovementActions.LocalMapObj_CheckAnimationFinished(player) == 1)
        {
            MapObject.MapObject_SetPauseMovementOn(player);
            sObjectsToWaitFor &= unchecked((byte)~WaitPlayer);
        }
        if ((sObjectsToWaitFor & WaitTarget) != 0 && MapObject.MapObject_IsMoving(scriptManager.TargetObject!) == 0)
        {
            MapObject.MapObject_SetPauseMovementOn(scriptManager.TargetObject!);
            sObjectsToWaitFor &= unchecked((byte)~WaitTarget);
        }
        if ((sObjectsToWaitFor & WaitPartner) != 0)
        {
            MapObject partner = MapObjectManager.MapObjMan_GetLocalMapObjByMovementType(fieldSystem.MapObjectManager!, MovementTypeFollowPlayer)!;
            if (MapObject.MapObject_IsMoving(partner) == 0)
            {
                MapObject.MapObject_SetPauseMovementOn(partner);
                sObjectsToWaitFor &= unchecked((byte)~WaitPartner);
            }
        }
        if ((sObjectsToWaitFor & WaitTargetLinked) != 0)
        {
            MapObject linked = MapObjectPartner.MapObject_GetTrainerPartner(scriptManager.TargetObject!)!;
            if (MapObject.MapObject_IsMoving(linked) == 0)
            {
                MapObject.MapObject_SetPauseMovementOn(linked);
                sObjectsToWaitFor &= unchecked((byte)~WaitTargetLinked);
            }
        }
        if (sObjectsToWaitFor == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020410CC
    private static bool ScriptContext_WaitForPartnerToStop(ScriptContext ctx)
    {
        MapObject partner = MapObjectManager.MapObjMan_GetLocalMapObjByMovementType(ctx.FieldSystem.MapObjectManager!, MovementTypeFollowPlayer)!;
        if (MapObject.MapObject_IsMoving(partner) == 0)
        {
            MapObject.MapObject_SetPauseMovementOn(partner);
            return true;
        }
        return false;
    }

    // OFFSET 0x020410F4
    private static bool ScriptContext_LockAllWithTarget(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ScriptManager scriptManager = Manager(ctx);
        MapObject player = PlayerAvatar.PlayerAvatar_GetMapObject(fieldSystem.PlayerAvatar!);
        MapObject? partner = MapObjectManager.MapObjMan_GetLocalMapObjByMovementType(fieldSystem.MapObjectManager!, MovementTypeFollowPlayer);
        MapObject? linked = MapObjectPartner.MapObject_GetTrainerPartner(scriptManager.TargetObject!);
        sObjectsToWaitFor = 0;
        MapObjectManager.MapObjectMan_PauseAllMovement(fieldSystem.MapObjectManager!);
        if (MovementActions.LocalMapObj_CheckAnimationFinished(player) == 0)
        {
            sObjectsToWaitFor |= WaitPlayer;
            MapObject.MapObject_SetPauseMovementOff(player);
        }
        if (MapObject.MapObject_IsMoving(scriptManager.TargetObject!) != 0)
        {
            sObjectsToWaitFor |= WaitTarget;
            MapObject.MapObject_SetPauseMovementOff(scriptManager.TargetObject!);
        }
        if (partner != null
            && SystemFlags.SystemFlag_CheckHasPartner(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!))
            && MapObject.MapObject_IsMoving(partner) != 0)
        {
            sObjectsToWaitFor |= WaitPartner;
            MapObject.MapObject_SetPauseMovementOff(partner);
        }
        if (linked != null && MapObject.MapObject_IsMoving(linked) != 0)
        {
            sObjectsToWaitFor |= WaitTargetLinked;
            MapObject.MapObject_SetPauseMovementOff(linked);
        }
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForObjectsToStop);
        return true;
    }

    // OFFSET 0x020411C4
    private static bool ScrCmd_ReleaseAll(ScriptContext ctx)
    {
        MapObjectManager.MapObjectMan_UnpauseAllMovement(ctx.FieldSystem.MapObjectManager!);
        return true;
    }

    // OFFSET 0x020411D4
    private static bool ScrCmd_LockObject(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort localID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        MapObject.MapObject_SetPauseMovementOn(MapObjectManager.MapObjMan_LocalMapObjByIndex(fieldSystem.MapObjectManager, localID)!);
        return false;
    }

    // OFFSET 0x020411F0
    private static bool ScrCmd_ReleaseObject(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort localID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        MapObject.MapObject_SetPauseMovementOff(MapObjectManager.MapObjMan_LocalMapObjByIndex(fieldSystem.MapObjectManager, localID)!);
        return false;
    }

    // OFFSET 0x0204120C
    private static bool ScrCmd_AddObject(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort localID = VarValue(ctx);
        uint numObjectEvents = MapHeaderData.MapHeaderData_GetNumObjectEvents(fieldSystem);
        ObjectEvent[]? objectEvents = MapHeaderData.MapHeaderData_GetObjectEvents(fieldSystem);
        MapObject? mapObject = MapObjectManager.MapObjectMan_AddMapObjectFromLocalID(fieldSystem.MapObjectManager!, localID, (int)numObjectEvents, (int)fieldSystem.Location!.U32(LocationMapIdOffset), objectEvents);
        if (mapObject == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return false;
    }

    // OFFSET 0x02041254
    private static bool ScrCmd_RemoveObject(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort localID = VarValue(ctx);
        MapObject? mapObject = MapObjectManager.MapObjMan_LocalMapObjByIndex(fieldSystem.MapObjectManager, localID);
        if (mapObject == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        else
        {
            MapObject.MapObject_SetFlagAndDeleteObject(mapObject);
        }
        return false;
    }

    // OFFSET 0x02041364
    private static bool ScrCmd_AddCameraObject(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort x = VarValue(ctx);
        ushort z = VarValue(ctx);
        ScriptManager scriptManager = Manager(ctx);
        scriptManager.CameraObject = MapObjectManager.MapObjectMan_AddMapObject(fieldSystem.MapObjectManager!, x, z, 0, CameraObjectGraphicsID, 0, (int)fieldSystem.Location!.U32(LocationMapIdOffset));
        MapObjectMove.MapObject_RecalculateObjectHeight(scriptManager.CameraObject!);
        MapObject.MapObject_SetHidden(scriptManager.CameraObject!, 1);
        MapObject.MapObject_SetStatus18Off(scriptManager.CameraObject!, 0);
        return false;
    }

    // OFFSET 0x020413D8
    private static bool ScrCmd_RemoveCameraObject(ScriptContext ctx)
    {
        MapObject.MapObject_Delete(Manager(ctx).CameraObject!);
        return false;
    }

    // OFFSET 0x020413F0
    private static bool ScrCmd_FacePlayer(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        int direction = MapObjectMove.Direction_GetOpposite(PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!));
        MapObject? target = Manager(ctx).TargetObject;
        if (target == null)
        {
            return false;
        }
        MapObjectRender.MapObject_FaceAndDraw(target, direction);
        return false;
    }

    // OFFSET 0x02041420
    private static bool ScrCmd_GetPlayerMapPos(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        BufferView destX = VarPointer(ctx);
        BufferView destZ = VarPointer(ctx);
        destX.W16(0, PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!));
        destZ.W16(0, PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!));
        return false;
    }

    // OFFSET 0x02041464
    private static bool ScrCmd_GetObjectMapPos(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort localID = VarValue(ctx);
        MapObject mapObject = MapObjectManager.MapObjMan_LocalMapObjByIndex(fieldSystem.MapObjectManager, localID)!;
        BufferView destX = VarPointer(ctx);
        BufferView destZ = VarPointer(ctx);
        destX.W16(0, MapObject.MapObject_GetX(mapObject));
        destZ.W16(0, MapObject.MapObject_GetZ(mapObject));
        return false;
    }

    // OFFSET 0x020414C4
    private static bool ScrCmd_GetPlayerDir(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, PlayerAvatar.PlayerAvatar_GetFacingDir(ctx.FieldSystem.PlayerAvatar!));
        return false;
    }

    // OFFSET 0x020415D0
    private static bool ScrCmd_SetObjectPersistent(ScriptContext ctx)
    {
        ushort localID = VarValue(ctx);
        MapObject mapObject = MapObjectManager.MapObjMan_LocalMapObjByIndex(ctx.FieldSystem.MapObjectManager, localID)!;
        byte persistent = ScriptContext.ScriptContext_ReadByte(ctx);
        MapObject.MapObject_SetFlagIsPersistent(mapObject, persistent);
        return false;
    }

    // OFFSET 0x02041604
    private static bool ScrCmd_SetObjectMovementType(ScriptContext ctx)
    {
        ushort localID = VarValue(ctx);
        MapObject mapObject = MapObjectManager.MapObjMan_LocalMapObjByIndex(ctx.FieldSystem.MapObjectManager, localID)!;
        ushort movementType = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        MapObject.MapObject_SwitchMovementType(mapObject, movementType);
        return false;
    }

    // OFFSET 0x0204163C
    private static bool ScrCmd_GetObjectMovementType(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, 0);
        ushort localID = VarValue(ctx);
        MapObject? mapObject = MapObjectManager.MapObjMan_LocalMapObjByIndex(ctx.FieldSystem.MapObjectManager, localID);
        if (mapObject != null)
        {
            destination.W16(0, MapObject.MapObject_GetMovementType(mapObject));
        }
        return false;
    }

    // OFFSET 0x02041684
    private static bool ScrCmd_RestartPartner(ScriptContext ctx)
    {
        MapObject partner = MapObjectManager.MapObjMan_GetLocalMapObjByMovementType(ctx.FieldSystem.MapObjectManager!, MovementTypeFollowPlayer)!;
        MapObject.MapObject_SetLocalIDAndRestart(partner, LocalIDFollower);
        return false;
    }

    // OFFSET 0x0204169C
    private static bool ScrCmd_CountUniqueSeals(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, SealCase.SealCase_CountUniqueSeals(SealCase.SaveData_GetSealCase(ctx.FieldSystem.SaveData!)));
        return false;
    }

    // OFFSET 0x020416C8
    private static bool ScrCmd_CountSeal(ScriptContext ctx)
    {
        ushort sealID = VarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, SealCase.SealCase_CountSealOccurrenceAnywhere(SealCase.SaveData_GetSealCase(ctx.FieldSystem.SaveData!), sealID));
        return false;
    }

    // OFFSET 0x02041708
    private static bool ScrCmd_GiveOrTakeSeal(ScriptContext ctx)
    {
        ushort sealID = VarValue(ctx);
        ushort quantity = VarValue(ctx);
        SealCase.GiveOrTakeSeal(SealCase.SaveData_GetSealCase(ctx.FieldSystem.SaveData!), sealID, (short)quantity);
        return false;
    }
}
