using static PlatinumSharp.Game.MapObjectManager;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed partial class MapObject
{
    // OFFSET 0x020628B4
    public static void MapObject_SetStatus(MapObject mapObject, uint status)
    {
        mapObject.Status = status;
    }

    // OFFSET 0x020628B8
    public static uint MapObject_GetStatus(MapObject mapObject)
    {
        return mapObject.Status;
    }

    // OFFSET 0x020628BC
    public static void MapObject_SetStatusFlagOn(MapObject mapObject, uint flag)
    {
        mapObject.Status |= flag;
    }

    // OFFSET 0x020628C4
    public static void MapObject_SetStatusFlagOff(MapObject mapObject, uint flag)
    {
        mapObject.Status &= ~flag;
    }

    // OFFSET 0x020628D0
    public static uint MapObject_CheckStatus(MapObject mapObject, uint flag)
    {
        return mapObject.Status & flag;
    }

    // OFFSET 0x020628D8
    public static bool MapObject_CheckStatusFlag(MapObject mapObject, uint flag)
    {
        if ((mapObject.Status & flag) != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020628E8
    public static void MapObject_SetStatus2(MapObject mapObject, uint status)
    {
        mapObject.Status2 = status;
    }

    // OFFSET 0x020628EC
    public static uint MapObject_GetStatus2(MapObject mapObject)
    {
        return mapObject.Status2;
    }

    // OFFSET 0x020628F0
    public static void MapObject_SetStatus2FlagOn(MapObject mapObject, uint flag)
    {
        mapObject.Status2 |= flag;
    }

    // OFFSET 0x020628F8
    public static void MapObject_SetStatus2FlagOff(MapObject mapObject, uint flag)
    {
        mapObject.Status2 &= ~flag;
    }

    // OFFSET 0x02062904
    public static uint MapObject_CheckStatus2(MapObject mapObject, uint flag)
    {
        return mapObject.Status2 & flag;
    }

    // OFFSET 0x0206290C
    public static void MapObject_SetLocalID(MapObject mapObject, int localID)
    {
        mapObject.LocalID = localID;
    }

    // OFFSET 0x02062910
    public static int MapObject_GetLocalID(MapObject mapObject)
    {
        return mapObject.LocalID;
    }

    // OFFSET 0x02062914
    public static void MapObject_SetMapHeaderID(MapObject mapObject, int mapHeaderID)
    {
        mapObject.MapHeaderID = mapHeaderID;
    }

    // OFFSET 0x02062918
    public static int MapObject_GetMapHeaderID(MapObject mapObject)
    {
        return mapObject.MapHeaderID;
    }

    // OFFSET 0x0206291C
    public static void MapObject_SetGraphicsID(MapObject mapObject, int graphicsID)
    {
        mapObject.GraphicsID = graphicsID;
    }

    // OFFSET 0x02062920
    public static int MapObject_GetGraphicsID(MapObject mapObject)
    {
        return mapObject.GraphicsID;
    }

    // OFFSET 0x02062924
    public static int MapObject_GetEffectiveGraphicsID(MapObject mapObject)
    {
        int graphicsID = MapObject_GetGraphicsID(mapObject);
        if (BerryPatchGraphics.BerryPatchGraphics_IsBerryPatch(graphicsID) == 1)
        {
            graphicsID = BerryPatchGraphics.BerryPatchGraphics_GetCurrentGraphicsResourceID(mapObject);
        }
        return graphicsID;
    }

    // OFFSET 0x02062944
    public static void MapObject_SetMovementType(MapObject mapObject, int movementType)
    {
        mapObject.MovementType = movementType;
    }

    // OFFSET 0x02062948
    public static int MapObject_GetMovementType(MapObject mapObject)
    {
        return mapObject.MovementType;
    }

    // OFFSET 0x0206294C
    public static void MapObject_SetTrainerType(MapObject mapObject, int trainerType)
    {
        mapObject.TrainerType = trainerType;
    }

    // OFFSET 0x02062950
    public static int MapObject_GetTrainerType(MapObject mapObject)
    {
        return mapObject.TrainerType;
    }

    // OFFSET 0x02062954
    public static void MapObject_SetFlag(MapObject mapObject, int flag)
    {
        mapObject.Flag = flag;
    }

    // OFFSET 0x02062958
    public static int MapObject_GetFlag(MapObject mapObject)
    {
        return mapObject.Flag;
    }

    // OFFSET 0x0206295C
    public static void MapObject_SetScript(MapObject mapObject, int script)
    {
        mapObject.Script = script;
    }

    // OFFSET 0x02062960
    public static int MapObject_GetScript(MapObject mapObject)
    {
        return mapObject.Script;
    }

    // OFFSET 0x02062964
    public static void MapObject_SetInitialDir(MapObject mapObject, int initialDir)
    {
        mapObject.InitialDir = initialDir;
    }

    // OFFSET 0x02062968
    public static int MapObject_GetInitialDir(MapObject mapObject)
    {
        return mapObject.InitialDir;
    }

    // OFFSET 0x0206296C
    public static void MapObject_Face(MapObject mapObject, int dir)
    {
        mapObject.PrevFacingDir = mapObject.FacingDir;
        mapObject.FacingDir = dir;
    }

    // OFFSET 0x02062974
    public static void MapObject_TryFace(MapObject mapObject, int dir)
    {
        if (MapObject_CheckStatus(mapObject, StatusLockDir) == 0)
        {
            mapObject.PrevFacingDir = mapObject.FacingDir;
            mapObject.FacingDir = dir;
        }
    }

    // OFFSET 0x0206298C
    public static int MapObject_GetFacingDir(MapObject mapObject)
    {
        return mapObject.FacingDir;
    }

    // OFFSET 0x02062990
    public static int MapObject_GetPrevFacingDir(MapObject mapObject)
    {
        return mapObject.PrevFacingDir;
    }

    // OFFSET 0x02062994
    public static void MapObject_Turn(MapObject mapObject, int dir)
    {
        mapObject.PrevMovingDir = mapObject.MovingDir;
        mapObject.MovingDir = dir;
    }

    // OFFSET 0x0206299C
    public static int MapObject_GetMovingDir(MapObject mapObject)
    {
        return mapObject.MovingDir;
    }

    // OFFSET 0x020629A0
    public static void MapObject_TryFaceAndTurn(MapObject mapObject, int dir)
    {
        MapObject_TryFace(mapObject, dir);
        MapObject_Turn(mapObject, dir);
    }

    // OFFSET 0x020629B4
    public static void MapObject_SetDataAt(MapObject mapObject, int value, int index)
    {
        switch (index)
        {
            case 0:
                mapObject.Data[0] = value;
                break;
            case 1:
                mapObject.Data[1] = value;
                break;
            case 2:
                mapObject.Data[2] = value;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
    }

    // OFFSET 0x020629D8
    public static int MapObject_GetDataAt(MapObject mapObject, int index)
    {
        switch (index)
        {
            case 0:
                return mapObject.Data[0];
            case 1:
                return mapObject.Data[1];
            case 2:
                return mapObject.Data[2];
            default:
                ErrorHandling.GF_AssertFail();
                return 0;
        }
    }

    // OFFSET 0x020629FC
    public static void MapObject_SetMovementRangeX(MapObject mapObject, int movementRangeX)
    {
        mapObject.MovementRangeX = movementRangeX;
    }

    // OFFSET 0x02062A00
    public static int MapObject_GetMovementRangeX(MapObject mapObject)
    {
        return mapObject.MovementRangeX;
    }

    // OFFSET 0x02062A04
    public static void MapObject_SetMovementRangeZ(MapObject mapObject, int movementRangeZ)
    {
        mapObject.MovementRangeZ = movementRangeZ;
    }

    // OFFSET 0x02062A08
    public static int MapObject_GetMovementRangeZ(MapObject mapObject)
    {
        return mapObject.MovementRangeZ;
    }

    // OFFSET 0x02062A0C
    public static void MapObject_SetDrawState(MapObject mapObject, int drawState)
    {
        mapObject.DrawState = drawState;
    }

    // OFFSET 0x02062A14
    public static int MapObject_GetDrawState(MapObject mapObject)
    {
        return mapObject.DrawState;
    }

    // OFFSET 0x02062A1C
    public static void MapObject_SetTask(MapObject mapObject, SysTask? task)
    {
        mapObject.Task = task;
    }

    // OFFSET 0x02062A24
    public static SysTask? MapObject_GetTask(MapObject mapObject)
    {
        return mapObject.Task;
    }

    // OFFSET 0x02062A2C
    public static void MapObject_DeleteTask(MapObject mapObject)
    {
        SysTask.SysTask_Done(MapObject_GetTask(mapObject)!);
    }

    // OFFSET 0x02062A38
    public static void MapObject_SetMapObjectManager(MapObject mapObject, MapObjectManager? mapObjectManager)
    {
        mapObject.MapObjectManager = mapObjectManager;
    }

    // OFFSET 0x02062A40
    public static MapObjectManager? MapObject_MapObjectManager(MapObject mapObject)
    {
        return mapObject.MapObjectManager;
    }

    // OFFSET 0x02062A48
    private static MapObjectManager? MapObject_MapObjectManagerDeconst(MapObject mapObject)
    {
        return mapObject.MapObjectManager;
    }

    // OFFSET 0x02062A54
    public static byte[] MapObject_InitMovementTypeData(MapObject mapObject, int size)
    {
        if (size > MovementTypeDataSize)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte[] data = MapObject_GetMovementTypeData(mapObject);
        Array.Clear(data, 0, size);
        return data;
    }

    // OFFSET 0x02062A78
    public static byte[] MapObject_GetMovementTypeData(MapObject mapObject)
    {
        return mapObject.MovementTypeData;
    }

    // OFFSET 0x02062A7C
    public static byte[] MapObject_InitTrainerTypeData(MapObject mapObject, int size)
    {
        if (size > TrainerTypeDataSize)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte[] data = MapObject_GetTrainerTypeData(mapObject);
        Array.Clear(data, 0, size);
        return data;
    }

    // OFFSET 0x02062AA0
    public static byte[] MapObject_GetTrainerTypeData(MapObject mapObject)
    {
        return mapObject.TrainerTypeData;
    }

    // OFFSET 0x02062AA4
    public static byte[] MapObject_InitMovementData(MapObject mapObject, int size)
    {
        if (size > MovementDataSize)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte[] data = MapObject_GetMovementData(mapObject);
        Array.Clear(data, 0, size);
        return data;
    }

    // OFFSET 0x02062AC8
    public static byte[] MapObject_GetMovementData(MapObject mapObject)
    {
        return mapObject.MovementData;
    }

    // OFFSET 0x02062ACC
    public static byte[] MapObject_InitDrawData(MapObject mapObject, int size)
    {
        if (size > DrawDataSize)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte[] data = MapObject_GetDrawData(mapObject);
        Array.Clear(data, 0, size);
        return data;
    }

    // OFFSET 0x02062AF0
    public static byte[] MapObject_GetDrawData(MapObject mapObject)
    {
        return mapObject.DrawData;
    }

    // OFFSET 0x02062AF8
    public static void MapObject_SetMovementInitFunc(MapObject mapObject, MapObjectFunc? func)
    {
        mapObject.MovementInit = func;
    }

    // OFFSET 0x02062B00
    public static void MapObject_CallMovementInit(MapObject mapObject)
    {
        mapObject.MovementInit!(mapObject);
    }

    // OFFSET 0x02062B0C
    public static void MapObject_SetMovementMoveFunc(MapObject mapObject, MapObjectFunc? func)
    {
        mapObject.MovementMove = func;
    }

    // OFFSET 0x02062B14
    public static void MapObject_CallMovementMove(MapObject mapObject)
    {
        mapObject.MovementMove!(mapObject);
    }

    // OFFSET 0x02062B20
    public static void MapObject_SetMovementExitFunc(MapObject mapObject, MapObjectFunc? func)
    {
        mapObject.MovementExit = func;
    }

    // OFFSET 0x02062B28
    public static void MapObject_CallMovementExit(MapObject mapObject)
    {
        mapObject.MovementExit!(mapObject);
    }

    // OFFSET 0x02062B34
    public static void MapObject_CallMovementReturn(MapObject mapObject)
    {
        MapObjectMovementFuncs funcs = MapObject_GetMovementFuncs((uint)MapObject_GetMovementType(mapObject))!;
        funcs.Return!(mapObject);
    }

    // OFFSET 0x02062B4C
    public static void MapObject_SetDrawInitFunc(MapObject mapObject, MapObjectFunc? func)
    {
        mapObject.DrawInit = func;
    }

    // OFFSET 0x02062B54
    public static void MapObject_CallDrawInit(MapObject mapObject)
    {
        mapObject.DrawInit!(mapObject);
    }

    // OFFSET 0x02062B60
    public static void MapObject_SetDrawFunc(MapObject mapObject, MapObjectFunc? func)
    {
        mapObject.Draw = func;
    }

    // OFFSET 0x02062B68
    public static void MapObject_CallDraw(MapObject mapObject)
    {
        mapObject.Draw!(mapObject);
    }

    // OFFSET 0x02062B74
    public static void MapObject_SetDrawDeleteFunc(MapObject mapObject, MapObjectFunc? func)
    {
        mapObject.DrawDelete = func;
    }

    // OFFSET 0x02062B7C
    public static void MapObject_CallDrawDelete(MapObject mapObject)
    {
        mapObject.DrawDelete!(mapObject);
    }

    // OFFSET 0x02062B88
    public static void MapObject_SetDrawSuspendFunc(MapObject mapObject, MapObjectFunc? func)
    {
        mapObject.DrawSuspend = func;
    }

    // OFFSET 0x02062B90
    public static void MapObject_CallDrawSuspend(MapObject mapObject)
    {
        mapObject.DrawSuspend!(mapObject);
    }

    // OFFSET 0x02062B9C
    public static void MapObject_SetDrawResumeFunc(MapObject mapObject, MapObjectFunc? func)
    {
        mapObject.DrawResume = func;
    }

    // OFFSET 0x02062BA4
    public static void MapObject_CallDrawResume(MapObject mapObject)
    {
        mapObject.DrawResume!(mapObject);
    }

    // OFFSET 0x02062BB0
    public static void MapObject_SetMovementAction(MapObject mapObject, int movementAction)
    {
        mapObject.MovementAction = movementAction;
    }

    // OFFSET 0x02062BB8
    public static int MapObject_GetMovementAction(MapObject mapObject)
    {
        return mapObject.MovementAction;
    }

    // OFFSET 0x02062BC0
    public static void MapObject_SetMovementStep(MapObject mapObject, int movementStep)
    {
        mapObject.MovementStep = movementStep;
    }

    // OFFSET 0x02062BC8
    public static void MapObject_AdvanceMovementStep(MapObject mapObject)
    {
        mapObject.MovementStep++;
    }

    // OFFSET 0x02062BD8
    public static int MapObject_GetMovementStep(MapObject mapObject)
    {
        return mapObject.MovementStep;
    }

    // OFFSET 0x02062BE0
    public static void MapObject_SetCurrTileBehavior(MapObject mapObject, uint tileBehavior)
    {
        mapObject.CurrTileBehavior = (ushort)tileBehavior;
    }

    // OFFSET 0x02062BE8
    public static uint MapObject_GetCurrTileBehavior(MapObject mapObject)
    {
        return mapObject.CurrTileBehavior;
    }

    // OFFSET 0x02062BF0
    public static void MapObject_SetPrevTileBehavior(MapObject mapObject, uint tileBehavior)
    {
        mapObject.PrevTileBehavior = (ushort)tileBehavior;
    }

    // OFFSET 0x02062BF8
    public static uint MapObject_GetPrevTileBehavior(MapObject mapObject)
    {
        return mapObject.PrevTileBehavior;
    }

    // OFFSET 0x02062C00
    public static FieldSystem? MapObject_FieldSystem(MapObject mapObject)
    {
        return MapObjectMan_FieldSystem(MapObject_MapObjectManagerDeconst(mapObject)!);
    }

    // OFFSET 0x02062C0C
    public static int MapObject_GetTaskBasePriority(MapObject mapObject)
    {
        return MapObjectMan_GetTaskBasePriority(MapObject_MapObjectManager(mapObject)!);
    }

    // OFFSET 0x02062C18
    public static int MapObject_GetAliasMapHeaderID(MapObject mapObject)
    {
        if (MapObject_IsAlias(mapObject) != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        return MapObject_GetFlag(mapObject);
    }

    // OFFSET 0x02062CBC
    public static uint MapObject_CheckManagerStatus(MapObject mapObject, uint flag)
    {
        return MapObjectMan_CheckStatus(MapObject_MapObjectManager(mapObject)!, flag);
    }

    // OFFSET 0x02062CF8
    public static int MapObject_IsActive(MapObject mapObject)
    {
        return MapObject_CheckStatusFlag(mapObject, StatusActive) ? 1 : 0;
    }

    // OFFSET 0x02062D04
    public static void MapObject_SetMoving(MapObject mapObject)
    {
        MapObject_SetStatusFlagOn(mapObject, StatusMoving);
    }

    // OFFSET 0x02062D10
    public static void MapObject_SetNotMoving(MapObject mapObject)
    {
        MapObject_SetStatusFlagOff(mapObject, StatusMoving);
    }

    // OFFSET 0x02062D1C
    public static int MapObject_IsMoving(MapObject mapObject)
    {
        return MapObject_CheckStatusFlag(mapObject, StatusMoving) ? 1 : 0;
    }

    // OFFSET 0x02062D28
    public static void MapObject_SetStartMovement(MapObject mapObject)
    {
        MapObject_SetStatusFlagOn(mapObject, StatusStartMovement);
    }

    // OFFSET 0x02062D34
    public static void MapObject_SetEndMovementOff(MapObject mapObject)
    {
        MapObject_SetStatusFlagOff(mapObject, StatusEndMovement);
    }

    // OFFSET 0x02062D40
    public static void MapObject_SetDrawInitialized(MapObject mapObject)
    {
        MapObject_SetStatusFlagOn(mapObject, StatusDrawInitialized);
    }

    // OFFSET 0x02062D4C
    public static int MapObject_IsDrawInitialized(MapObject mapObject)
    {
        return MapObject_CheckStatusFlag(mapObject, StatusDrawInitialized) ? 1 : 0;
    }

    // OFFSET 0x02062D58
    public static int MapObject_IsHidden(MapObject mapObject)
    {
        return MapObject_CheckStatusFlag(mapObject, StatusHide) ? 1 : 0;
    }

    // OFFSET 0x02062D64
    public static void MapObject_SetHidden(MapObject mapObject, int hidden)
    {
        if (hidden == 1)
        {
            MapObject_SetStatusFlagOn(mapObject, StatusHide);
        }
        else
        {
            MapObject_SetStatusFlagOff(mapObject, StatusHide);
        }
    }

    // OFFSET 0x02062D80
    public static void MapObject_SetStatus18Off(MapObject mapObject, int off)
    {
        if (off == 1)
        {
            MapObject_SetStatusFlagOff(mapObject, Status18);
        }
        else
        {
            MapObject_SetStatusFlagOn(mapObject, Status18);
        }
    }

    // OFFSET 0x02062D9C
    public static int MapObject_IsStatus19Off(MapObject mapObject)
    {
        if (MapObject_CheckStatusFlag(mapObject, Status19))
        {
            return 0;
        }
        return 1;
    }

    // OFFSET 0x02062DB4
    public static void MapObject_SetStatus19(MapObject mapObject, int on)
    {
        if (on == 1)
        {
            MapObject_SetStatusFlagOn(mapObject, Status19);
        }
        else
        {
            MapObject_SetStatusFlagOff(mapObject, Status19);
        }
    }

    // OFFSET 0x02062DD0
    public static void MapObject_SetPauseMovementOn(MapObject mapObject)
    {
        MapObject_SetStatusFlagOn(mapObject, StatusPauseMovement);
    }

    // OFFSET 0x02062DDC
    public static void MapObject_SetPauseMovementOff(MapObject mapObject)
    {
        MapObject_SetStatusFlagOff(mapObject, StatusPauseMovement);
    }

    // OFFSET 0x02062DE8
    public static int MapObject_IsMovementPaused(MapObject mapObject)
    {
        if (MapObject_CheckStatusFlag(mapObject, StatusPauseMovement))
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02062DFC
    public static int MapObject_IsDrawActive(MapObject mapObject)
    {
        if (MapObjectMan_IsDrawInitialized(MapObject_MapObjectManager(mapObject)!) == 0)
        {
            return 0;
        }
        if (MapObject_CheckStatus(mapObject, StatusDrawInitialized) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02062E28
    public static void MapObject_SetHeightCalculationDisabled(MapObject mapObject, int disabled)
    {
        if (disabled == 1)
        {
            MapObject_SetStatusFlagOn(mapObject, StatusHeightCalculationDisabled);
        }
        else
        {
            MapObject_SetStatusFlagOff(mapObject, StatusHeightCalculationDisabled);
        }
    }

    // OFFSET 0x02062E44
    public static int MapObject_IsHeightCalculationDisabled(MapObject mapObject)
    {
        if (MapObject_CheckStatus(mapObject, StatusHeightCalculationDisabled) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02062E5C
    public static void MapObject_SetFlagIsPersistent(MapObject mapObject, int persistent)
    {
        if (persistent == 1)
        {
            MapObject_SetStatusFlagOn(mapObject, StatusPersistent);
        }
        else
        {
            MapObject_SetStatusFlagOff(mapObject, StatusPersistent);
        }
    }

    // OFFSET 0x02062E78
    public static void MapObject_SetAlias(MapObject mapObject, int alias)
    {
        if (alias == 1)
        {
            MapObject_SetStatusFlagOn(mapObject, StatusAlias);
        }
        else
        {
            MapObject_SetStatusFlagOff(mapObject, StatusAlias);
        }
    }

    // OFFSET 0x02062E94
    public static int MapObject_IsAlias(MapObject mapObject)
    {
        if (MapObject_CheckStatus(mapObject, StatusAlias) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02062EAC
    public static void MapObject_SetStatus26(MapObject mapObject, int on)
    {
        if (on == 1)
        {
            MapObject_SetStatusFlagOn(mapObject, Status26);
        }
        else
        {
            MapObject_SetStatusFlagOff(mapObject, Status26);
        }
    }

    // OFFSET 0x02062EC8
    public static int MapObject_IsStatus26On(MapObject mapObject)
    {
        if (MapObject_CheckStatus(mapObject, Status26) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02062EE0
    public static void MapObject_SetFlagDoNotSinkIntoTerrain(MapObject mapObject, int on)
    {
        if (on == 1)
        {
            MapObject_SetStatusFlagOn(mapObject, StatusDoNotSinkIntoTerrain);
        }
        else
        {
            MapObject_SetStatusFlagOff(mapObject, StatusDoNotSinkIntoTerrain);
        }
    }

    // OFFSET 0x02062EFC
    public static int MapObject_CheckFlagDoNotSinkIntoTerrain(MapObject mapObject)
    {
        if (MapObject_CheckStatus(mapObject, StatusDoNotSinkIntoTerrain) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02062F14
    public static void MapObject_SetElevatedBridgeStatus(MapObject mapObject, int onElevatedBridge)
    {
        if (onElevatedBridge == 1)
        {
            MapObject_SetStatusFlagOn(mapObject, StatusOnElevatedBridge);
        }
        else
        {
            MapObject_SetStatusFlagOff(mapObject, StatusOnElevatedBridge);
        }
    }

    // OFFSET 0x02062F30
    public static int MapObject_IsStatusOnElevatedBridge(MapObject mapObject)
    {
        if (MapObject_CheckStatus(mapObject, StatusOnElevatedBridge) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02062F48
    public static void MapObject_SetStatus24(MapObject mapObject, int on)
    {
        if (on == 1)
        {
            MapObject_SetStatusFlagOn(mapObject, Status24);
        }
        else
        {
            MapObject_SetStatusFlagOff(mapObject, Status24);
        }
    }

    // OFFSET 0x02062F64
    public static int MapObject_IsStatus24On(MapObject mapObject)
    {
        if (MapObject_CheckStatus(mapObject, Status24) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02062F7C
    public static int MapObject_HasMovementAction(MapObject mapObject)
    {
        if (MapObject_CheckStatus(mapObject, StatusMovementAction) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02062F90
    public static void MapObject_SetDynamicHeightCalculationEnabled(MapObject mapObject, int enabled)
    {
        if (enabled == 1)
        {
            MapObject_SetStatusFlagOn(mapObject, StatusDynamicHeightCalculationEnabled);
        }
        else
        {
            MapObject_SetStatusFlagOff(mapObject, StatusDynamicHeightCalculationEnabled);
        }
    }

    // OFFSET 0x02062FAC
    public static int MapObject_IsDynamicHeightCalculationEnabled(MapObject mapObject)
    {
        if (MapObject_CheckStatus(mapObject, StatusDynamicHeightCalculationEnabled) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02062FC4
    public static void MapObject_SetStatus2Bit2(MapObject mapObject, int on)
    {
        if (on == 1)
        {
            MapObject_SetStatus2FlagOn(mapObject, Status2Bit2);
        }
        else
        {
            MapObject_SetStatus2FlagOff(mapObject, Status2Bit2);
        }
    }

    // OFFSET 0x02062FDC
    public static int MapObject_IsStatus2Bit2On(MapObject mapObject)
    {
        if (MapObject_CheckStatus2(mapObject, Status2Bit2) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02062FF0
    public static int MapObject_GetXInitial(MapObject mapObject)
    {
        return mapObject.XInitial;
    }

    // OFFSET 0x02062FF4
    public static void MapObject_SetXInitial(MapObject mapObject, int x)
    {
        mapObject.XInitial = x;
    }

    // OFFSET 0x02062FF8
    public static int MapObject_GetYInitial(MapObject mapObject)
    {
        return mapObject.YInitial;
    }

    // OFFSET 0x02062FFC
    public static void MapObject_SetYInitial(MapObject mapObject, int y)
    {
        mapObject.YInitial = y;
    }

    // OFFSET 0x02063000
    public static int MapObject_GetZInitial(MapObject mapObject)
    {
        return mapObject.ZInitial;
    }

    // OFFSET 0x02063004
    public static void MapObject_SetZInitial(MapObject mapObject, int z)
    {
        mapObject.ZInitial = z;
    }

    // OFFSET 0x02063008
    public static int MapObject_GetXPrev(MapObject mapObject)
    {
        return mapObject.XPrev;
    }

    // OFFSET 0x0206300C
    public static void MapObject_SetXPrev(MapObject mapObject, int x)
    {
        mapObject.XPrev = x;
    }

    // OFFSET 0x02063010
    public static int MapObject_GetYPrev(MapObject mapObject)
    {
        return mapObject.YPrev;
    }

    // OFFSET 0x02063014
    public static void MapObject_SetYPrev(MapObject mapObject, int y)
    {
        mapObject.YPrev = y;
    }

    // OFFSET 0x02063018
    public static int MapObject_GetZPrev(MapObject mapObject)
    {
        return mapObject.ZPrev;
    }

    // OFFSET 0x0206301C
    public static void MapObject_SetZPrev(MapObject mapObject, int z)
    {
        mapObject.ZPrev = z;
    }

    // OFFSET 0x02063020
    public static int MapObject_GetX(MapObject mapObject)
    {
        return mapObject.X;
    }

    // OFFSET 0x02063024
    public static void MapObject_SetX(MapObject mapObject, int x)
    {
        mapObject.X = x;
    }

    // OFFSET 0x02063028
    public static void MapObject_AddX(MapObject mapObject, int dx)
    {
        mapObject.X += dx;
    }

    // OFFSET 0x02063030
    public static int MapObject_GetY(MapObject mapObject)
    {
        return mapObject.Y;
    }

    // OFFSET 0x02063034
    public static void MapObject_SetY(MapObject mapObject, int y)
    {
        mapObject.Y = y;
    }

    // OFFSET 0x02063038
    public static void MapObject_AddY(MapObject mapObject, int dy)
    {
        mapObject.Y += dy;
    }

    // OFFSET 0x02063040
    public static int MapObject_GetZ(MapObject mapObject)
    {
        return mapObject.Z;
    }

    // OFFSET 0x02063044
    public static void MapObject_SetZ(MapObject mapObject, int z)
    {
        mapObject.Z = z;
    }

    // OFFSET 0x02063048
    public static void MapObject_AddZ(MapObject mapObject, int dz)
    {
        mapObject.Z += dz;
    }

    // OFFSET 0x02063050
    public static void MapObject_CopyPos(MapObject mapObject, out VecFx32 position)
    {
        position = mapObject.Position;
    }

    // OFFSET 0x02063060
    public static void MapObject_SetPos(MapObject mapObject, in VecFx32 position)
    {
        mapObject.Position = position;
    }

    // OFFSET 0x02063070
    public static ref VecFx32 MapObject_GetPos(MapObject mapObject)
    {
        return ref mapObject.Position;
    }

    // OFFSET 0x02063074
    public static int MapObject_GetPosY(MapObject mapObject)
    {
        return mapObject.Position.Y;
    }

    // OFFSET 0x02063078
    public static void MapObject_CopySpriteJumpOffset(MapObject mapObject, out VecFx32 offset)
    {
        offset = mapObject.SpriteJumpOffset;
    }

    // OFFSET 0x02063088
    public static void MapObject_SetSpriteJumpOffset(MapObject mapObject, in VecFx32 offset)
    {
        mapObject.SpriteJumpOffset = offset;
    }

    // OFFSET 0x02063098
    public static ref VecFx32 MapObject_GetSpriteJumpOffset(MapObject mapObject)
    {
        return ref mapObject.SpriteJumpOffset;
    }

    // OFFSET 0x0206309C
    public static void MapObject_CopySpritePosOffset(MapObject mapObject, out VecFx32 offset)
    {
        offset = mapObject.SpritePosOffset;
    }

    // OFFSET 0x020630AC
    public static void MapObject_SetSpritePosOffset(MapObject mapObject, in VecFx32 offset)
    {
        mapObject.SpritePosOffset = offset;
    }

    // OFFSET 0x020630BC
    public static void MapObject_CopySpriteTerrainOffset(MapObject mapObject, out VecFx32 offset)
    {
        offset = mapObject.SpriteTerrainOffset;
    }

    // OFFSET 0x020630CC
    public static void MapObject_SetSpriteTerrainOffset(MapObject mapObject, in VecFx32 offset)
    {
        mapObject.SpriteTerrainOffset = offset;
    }

    // OFFSET 0x020630DC
    public static int MapObject_GetPosYAsTile(MapObject mapObject)
    {
        return Fx32ToInt(MapObject_GetPosY(mapObject) >> 3);
    }

    // OFFSET 0x020630F0
    public static void ObjectEvent_SetLocalID(ObjectEvent objectEvent, int localID)
    {
        objectEvent.LocalID = (ushort)localID;
    }

    // OFFSET 0x020630F4
    public static int ObjectEvent_GetLocalID(ObjectEvent objectEvent)
    {
        return objectEvent.LocalID;
    }

    // OFFSET 0x020630F8
    public static void ObjectEvent_SetGraphicsID(ObjectEvent objectEvent, int graphicsID)
    {
        objectEvent.GraphicsID = (ushort)graphicsID;
    }

    // OFFSET 0x020630FC
    public static int ObjectEvent_GetGraphicsID(ObjectEvent objectEvent)
    {
        return objectEvent.GraphicsID;
    }

    // OFFSET 0x02063100
    public static void ObjectEvent_SetMovementType(ObjectEvent objectEvent, int movementType)
    {
        objectEvent.MovementType = (ushort)movementType;
    }

    // OFFSET 0x02063104
    public static int ObjectEvent_GetMovementType(ObjectEvent objectEvent)
    {
        return objectEvent.MovementType;
    }

    // OFFSET 0x02063108
    public static void ObjectEvent_SetTrainerType(ObjectEvent objectEvent, int trainerType)
    {
        objectEvent.TrainerType = (ushort)trainerType;
    }

    // OFFSET 0x0206310C
    public static int ObjectEvent_GetTrainerType(ObjectEvent objectEvent)
    {
        return objectEvent.TrainerType;
    }

    // OFFSET 0x02063110
    public static void ObjectEvent_SetHiddenFlag(ObjectEvent objectEvent, int hiddenFlag)
    {
        objectEvent.HiddenFlag = (ushort)hiddenFlag;
    }

    // OFFSET 0x02063114
    public static int ObjectEvent_GetHiddenFlag(ObjectEvent objectEvent)
    {
        return objectEvent.HiddenFlag;
    }

    // OFFSET 0x02063118
    public static void ObjectEvent_SetScript(ObjectEvent objectEvent, int script)
    {
        objectEvent.Script = (ushort)script;
    }

    // OFFSET 0x0206311C
    public static int ObjectEvent_GetScript(ObjectEvent objectEvent)
    {
        return objectEvent.Script;
    }

    // OFFSET 0x02063120
    public static void ObjectEvent_SetInitialDir(ObjectEvent objectEvent, int initialDir)
    {
        objectEvent.Dir = (short)initialDir;
    }

    // OFFSET 0x02063124
    public static int ObjectEvent_GetInitialDir(ObjectEvent objectEvent)
    {
        return objectEvent.Dir;
    }

    // OFFSET 0x0206312C
    public static void ObjectEvent_SetDataAt(ObjectEvent objectEvent, int value, int index)
    {
        switch (index)
        {
            case 0:
                objectEvent.Data[0] = (ushort)value;
                break;
            case 1:
                objectEvent.Data[1] = (ushort)value;
                break;
            case 2:
                objectEvent.Data[2] = (ushort)value;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
    }

    // OFFSET 0x02063150
    public static int ObjectEvent_GetDataAt(ObjectEvent objectEvent, int index)
    {
        switch (index)
        {
            case 0:
                return objectEvent.Data[0];
            case 1:
                return objectEvent.Data[1];
            case 2:
                return objectEvent.Data[2];
            default:
                ErrorHandling.GF_AssertFail();
                return 0;
        }
    }

    // OFFSET 0x02063174
    public static void ObjectEvent_SetMovementRangeX(ObjectEvent objectEvent, int movementRangeX)
    {
        objectEvent.MovementRangeX = (short)movementRangeX;
    }

    // OFFSET 0x02063178
    public static int ObjectEvent_GetMovementRangeX(ObjectEvent objectEvent)
    {
        return objectEvent.MovementRangeX;
    }

    // OFFSET 0x02063180
    public static void ObjectEvent_SetMovementRangeZ(ObjectEvent objectEvent, int movementRangeZ)
    {
        objectEvent.MovementRangeZ = (short)movementRangeZ;
    }

    // OFFSET 0x02063184
    public static int ObjectEvent_GetMovementRangeZ(ObjectEvent objectEvent)
    {
        return objectEvent.MovementRangeZ;
    }

    // OFFSET 0x0206318C
    public static void ObjectEvent_SetX(ObjectEvent objectEvent, int x)
    {
        objectEvent.X = (ushort)x;
    }

    // OFFSET 0x02063190
    public static int ObjectEvent_GetX(ObjectEvent objectEvent)
    {
        return objectEvent.X;
    }

    // OFFSET 0x02063194
    public static void ObjectEvent_SetY(ObjectEvent objectEvent, int y)
    {
        objectEvent.Y = y;
    }

    // OFFSET 0x02063198
    public static int ObjectEvent_GetY(ObjectEvent objectEvent)
    {
        return objectEvent.Y;
    }

    // OFFSET 0x0206319C
    public static void ObjectEvent_SetZ(ObjectEvent objectEvent, int z)
    {
        objectEvent.Z = (ushort)z;
    }

    // OFFSET 0x020631A0
    public static int ObjectEvent_GetZ(ObjectEvent objectEvent)
    {
        return objectEvent.Z;
    }

    // OFFSET 0x020631A4
    public static ObjectEvent? ObjectEvent_FindByLocalID(int localID, int numObjectEvents, ObjectEvent[]? objectEvents)
    {
        int index = 0;
        do
        {
            ObjectEvent objectEvent = objectEvents![index];
            if (ObjectEvent_HasNoScript(objectEvent) == 0 && ObjectEvent_GetLocalID(objectEvent) == localID)
            {
                return objectEvent;
            }
            index++;
        }
        while (index < numObjectEvents);
        return null;
    }

    // OFFSET 0x020631D8
    public static int ObjectEvent_HasNoScript(ObjectEvent objectEvent)
    {
        if ((ushort)ObjectEvent_GetScript(objectEvent) == ScriptAlias)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020631F4
    public static int ObjectEvent_GetAliasMapHeaderID(ObjectEvent objectEvent)
    {
        if (ObjectEvent_HasNoScript(objectEvent) != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        return ObjectEvent_GetHiddenFlag(objectEvent);
    }

    // OFFSET 0x0206320C
    public static MapObjectMovementFuncs? MapObject_GetMovementFuncs(uint movementType)
    {
        if (movementType >= MovementTypeCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        movementFuncsTable ??= new MapObjectMovementFuncs?[MovementTypeCount];
        MapObjectMovementFuncs? funcs = movementFuncsTable[movementType];
        if (funcs == null)
        {
            uint address = RomData.Arm9U32(MovementFuncsTableAddress + movementType * 4, 1)[0];
            uint[] words = RomData.Arm9U32(address, MapObjectMovementFuncs.Size / 4);
            funcs = new MapObjectMovementFuncs
            {
                MovementType = words[0],
                Init = ResolveCallback(words[1]),
                Move = ResolveCallback(words[2]),
                Exit = ResolveCallback(words[3]),
                Return = ResolveCallback(words[4]),
            };
            movementFuncsTable[movementType] = funcs;
        }
        return funcs;
    }

    // OFFSET 0x02063224
    private static MapObjectFunc? MapObjectMovementFuncs_GetInit(MapObjectMovementFuncs funcs)
    {
        return funcs.Init;
    }

    // OFFSET 0x02063228
    private static MapObjectFunc? MapObjectMovementFuncs_GetMove(MapObjectMovementFuncs funcs)
    {
        return funcs.Move;
    }

    // OFFSET 0x0206322C
    private static MapObjectFunc? MapObjectMovementFuncs_GetExit(MapObjectMovementFuncs funcs)
    {
        return funcs.Exit;
    }

    // OFFSET 0x02063230
    private static MapObjectFunc? MapObjectDrawFuncs_GetInit(MapObjectDrawFuncs funcs)
    {
        return funcs.Init;
    }

    // OFFSET 0x02063234
    private static MapObjectFunc? MapObjectDrawFuncs_GetDraw(MapObjectDrawFuncs funcs)
    {
        return funcs.Draw;
    }

    // OFFSET 0x02063238
    private static MapObjectFunc? MapObjectDrawFuncs_GetDelete(MapObjectDrawFuncs funcs)
    {
        return funcs.Delete;
    }

    // OFFSET 0x0206323C
    private static MapObjectFunc? MapObjectDrawFuncs_GetSuspend(MapObjectDrawFuncs funcs)
    {
        return funcs.Suspend;
    }

    // OFFSET 0x02063240
    private static MapObjectFunc? MapObjectDrawFuncs_GetResume(MapObjectDrawFuncs funcs)
    {
        return funcs.Resume;
    }

    // OFFSET 0x02063244
    private static MapObjectDrawFuncs? MapObject_GetDrawFuncs(uint graphicsID)
    {
        if (drawFuncsTable == null)
        {
            List<uint> entries = new();
            uint address = DrawFuncsTableAddress;
            while (true)
            {
                uint[] entry = RomData.OverlayU32(DrawFuncsOverlayID, address, 2);
                entries.Add(entry[0]);
                entries.Add(entry[1]);
                if (entry[0] == DrawFuncsTableEnd)
                {
                    break;
                }
                address += 8;
            }
            drawFuncsTable = entries.ToArray();
        }
        int position = 0;
        do
        {
            if (drawFuncsTable[position] == graphicsID)
            {
                return DrawFuncsAt(drawFuncsTable[position + 1]);
            }
            position += 2;
        }
        while (drawFuncsTable[position] != DrawFuncsTableEnd);
        ErrorHandling.GF_AssertFail();
        return null;
    }

    // OFFSET 0x020632D4
    public static void MapObject_SetPosDirFromVec(MapObject mapObject, in VecFx32 position, int dir)
    {
        MapObject_SetX(mapObject, Fx32ToInt(position.X >> 4));
        MapObject_SetY(mapObject, Fx32ToInt(position.Y >> 3));
        MapObject_SetZ(mapObject, Fx32ToInt(position.Z >> 4));
        MapObject_SetPos(mapObject, position);
        MapObjectMove.MapObject_UpdateCoords(mapObject);
        MapObject_Face(mapObject, dir);
        MovementActions.MapObject_ClearMovementAction(mapObject);
        MapObject_SetStatusFlagOn(mapObject, StatusStartMovement);
        MapObject_SetStatusFlagOff(mapObject, StatusMoving | StatusEndMovement);
    }

    // OFFSET 0x02063340
    public static void MapObject_SetPosDirFromCoords(MapObject mapObject, int x, int y, int z, int dir)
    {
        VecFx32 position;
        position.X = TileCenterToFx32(x);
        MapObject_SetX(mapObject, x);
        position.Y = y << 15;
        MapObject_SetY(mapObject, y);
        position.Z = TileCenterToFx32(z);
        MapObject_SetZ(mapObject, z);
        MapObject_SetPos(mapObject, position);
        MapObjectMove.MapObject_UpdateCoords(mapObject);
        MapObject_Face(mapObject, dir);
        MapObject_SetStatusFlagOn(mapObject, StatusStartMovement);
        MapObject_SetStatusFlagOff(mapObject, StatusMoving | StatusEndMovement);
        MovementActions.MapObject_ClearMovementAction(mapObject);
    }

    // OFFSET 0x020633A8
    public static void MapObject_SwitchMovementType(MapObject mapObject, int movementType)
    {
        MapObject_CallMovementExit(mapObject);
        MapObject_SetMovementType(mapObject, movementType);
        MapObject_SetMovementFuncs(mapObject);
        MapObjectMove.MapObject_InitMove(mapObject);
    }

    // OFFSET 0x020633C8
    public static void MapObject_SetLocalIDAndRestart(MapObject mapObject, int localID)
    {
        MapObject_SetLocalID(mapObject, localID);
        MapObject_SetStartMovement(mapObject);
        MapObject_ClearTransientStatus(mapObject);
    }

    // OFFSET 0x020633E0
    public static void MapObject_MovementInitNone(MapObject mapObject)
    {
    }

    // OFFSET 0x020633E4
    public static void MapObject_MovementMoveNone(MapObject mapObject)
    {
    }

    // OFFSET 0x020633E8
    public static void MapObject_MovementExitNone(MapObject mapObject)
    {
    }

    // OFFSET 0x020633EC
    public static void MapObject_MovementReturnNone(MapObject mapObject)
    {
    }

    // OFFSET 0x020633F0
    public static void MapObject_DrawInitNone(MapObject mapObject)
    {
    }

    // OFFSET 0x020633F4
    public static void MapObject_DrawNone(MapObject mapObject)
    {
    }

    // OFFSET 0x020633F8
    public static void MapObject_DrawSuspendNone(MapObject mapObject)
    {
    }

    // OFFSET 0x020633FC
    public static void MapObject_DrawResumeNone(MapObject mapObject)
    {
    }
}
