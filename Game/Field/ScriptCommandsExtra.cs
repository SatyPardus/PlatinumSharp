namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const int HeapIDFieldTask = 0xB;
    private const int MetTerrainDefault = 0x18;
    private static void RegisterExtra(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x02043C0C] = ScrCmd_GetPlayerGender;
        ported[0x02043C54] = ScrCmd_Dummy335;
        ported[0x02043CC4] = ScrCmd_CheckPocketHasItems;
        ported[0x02043D04] = ScrCmd_SetObjectEventPos;
        ported[0x02043D54] = ScrCmd_SetPosition;
        ported[0x02043DE4] = ScrCmd_SetObjectEventMovementType;
        ported[0x02043E1C] = ScrCmd_SetObjectEventDir;
        ported[0x02043E54] = ScrCmd_SetWarpEventPos;
        ported[0x02043EA4] = ScrCmd_SetBgEventPos;
        ported[0x02043EF4] = ScrCmd_FaceObject;
        ported[0x02042918] = ScrCmd_FadeScreen;
        ported[0x02041D78] = ScrCmd_ReturnToField;
        ported[0x02041D88] = ScrCmd_FinishMap;
        ported[0x02042E38] = ScrCmd_GetPlayerStarterSpecies;
        ported[0x0204CFFC] = ScrCmd_GivePokemon;
        ported[0x020425E0] = ScrCmd_ChooseStarter;
        ported[0x02042628] = ScrCmd_SaveChosenStarter;
        ported[0x02042960] = ScrCmd_WaitFadeScreen;
    }

    private static ushort ReadVarValue(ScriptContext ctx)
    {
        return FieldSystemVarsFlags.FieldSystem_TryGetVar(ctx.FieldSystem, ScriptContext.ScriptContext_ReadHalfWord(ctx));
    }

    // OFFSET 0x02043C0C
    private static bool ScrCmd_GetPlayerGender(ScriptContext ctx)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(FieldSystemCore.FieldSystem_GetSaveData(ctx.FieldSystem)!);
        BufferView destVar = FieldSystemVarsFlags.FieldSystem_GetVarPointer(ctx.FieldSystem, ScriptContext.ScriptContext_ReadHalfWord(ctx))!;
        destVar.W16(0, (ushort)TrainerInfo.TrainerInfo_Gender(trainerInfo));
        return false;
    }

    // OFFSET 0x02043C54
    private static bool ScrCmd_Dummy335(ScriptContext ctx)
    {
        return false;
    }

    // OFFSET 0x02043CC4
    private static bool ScrCmd_CheckPocketHasItems(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort pocket = ReadVarValue(ctx);
        BufferView destVar = FieldSystemVarsFlags.FieldSystem_GetVarPointer(fieldSystem, ScriptContext.ScriptContext_ReadHalfWord(ctx))!;
        destVar.W16(0, BagItems.Bag_PocketHasItems(BagItems.SaveData_GetBag(fieldSystem.SaveData!), pocket) ? 1 : 0);
        return false;
    }

    // OFFSET 0x02043D04
    private static bool ScrCmd_SetObjectEventPos(ScriptContext ctx)
    {
        ushort localID = ReadVarValue(ctx);
        ushort x = ReadVarValue(ctx);
        ushort z = ReadVarValue(ctx);
        MapHeaderData.MapHeaderData_SetObjectEventPos(ctx.FieldSystem, localID, x, z);
        return false;
    }

    // OFFSET 0x02043D54
    private static bool ScrCmd_SetPosition(ScriptContext ctx)
    {
        ushort localID = ReadVarValue(ctx);
        ushort x = ReadVarValue(ctx);
        ushort y = ReadVarValue(ctx);
        ushort z = ReadVarValue(ctx);
        ushort direction = ReadVarValue(ctx);
        MapObject mapObject = MapObjectManager.MapObjMan_LocalMapObjByIndex(ctx.FieldSystem.MapObjectManager, localID)!;
        MapObject.MapObject_SetPosDirFromCoords(mapObject, x, y, z, direction);
        MapObjectMove.MapObject_RecalculateObjectHeight(mapObject);
        return false;
    }

    // OFFSET 0x02043DE4
    private static bool ScrCmd_SetObjectEventMovementType(ScriptContext ctx)
    {
        ushort localID = ReadVarValue(ctx);
        ushort movementType = ReadVarValue(ctx);
        MapHeaderData.MapHeaderData_SetObjectEventMovementType(ctx.FieldSystem, localID, movementType);
        return false;
    }

    // OFFSET 0x02043E1C
    private static bool ScrCmd_SetObjectEventDir(ScriptContext ctx)
    {
        ushort localID = ReadVarValue(ctx);
        ushort direction = ReadVarValue(ctx);
        MapHeaderData.MapHeaderData_SetObjectEventDir(ctx.FieldSystem, localID, direction);
        return false;
    }

    // OFFSET 0x02043E54
    private static bool ScrCmd_SetWarpEventPos(ScriptContext ctx)
    {
        ushort index = ReadVarValue(ctx);
        ushort x = ReadVarValue(ctx);
        ushort z = ReadVarValue(ctx);
        MapHeaderData.MapHeaderData_SetWarpEventPos(ctx.FieldSystem, index, x, z);
        return false;
    }

    // OFFSET 0x02043EA4
    private static bool ScrCmd_SetBgEventPos(ScriptContext ctx)
    {
        ushort index = ReadVarValue(ctx);
        ushort x = ReadVarValue(ctx);
        ushort z = ReadVarValue(ctx);
        MapHeaderData.MapHeaderData_SetBgEventPos(ctx.FieldSystem, index, x, z);
        return false;
    }

    // OFFSET 0x02043EF4
    private static bool ScrCmd_FaceObject(ScriptContext ctx)
    {
        ushort localID = ReadVarValue(ctx);
        ushort direction = ReadVarValue(ctx);
        MapObject? mapObject = MapObjectManager.MapObjMan_LocalMapObjByIndex(ctx.FieldSystem.MapObjectManager, localID);
        if (mapObject == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObjectRender.MapObject_FaceAndDraw(mapObject!, direction);
        return false;
    }

    // OFFSET 0x02042918
    private static bool ScrCmd_FadeScreen(ScriptContext ctx)
    {
        ushort duration = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort stepsPerFrame = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort fadeType = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort color = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ScreenFadeControl.StartScreenFade(0, fadeType, fadeType, color, duration, stepsPerFrame, HeapIDField);
        ScreenFadeControl.ResetVisibleHardwareWindows(0);
        ScreenFadeControl.ResetVisibleHardwareWindows(1);
        return false;
    }

    // OFFSET 0x02042960
    private static bool ScrCmd_WaitFadeScreen(ScriptContext ctx)
    {
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_IsScreenFadeDone);
        return true;
    }

    // OFFSET 0x02042970
    private static bool ScriptContext_IsScreenFadeDone(ScriptContext ctx)
    {
        if (ScreenFadeControl.IsScreenFadeDone())
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02041D60
    private static bool ScriptContext_WaitForApplicationExit(ScriptContext ctx)
    {
        if (!FieldTask.FieldSystem_IsRunningApplication(ctx.FieldSystem))
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020425E0
    private static bool ScrCmd_ChooseStarter(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ScriptManager scriptManager = Manager(ctx);
        ChooseStarterArgs args = new();
        scriptManager.DataPtr = args;
        args.Options = PlayerSave.SaveData_GetOptions(fieldSystem.SaveData!);
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(ChooseStarter.Template), args);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForApplicationExit);
        return true;
    }

    // OFFSET 0x02042628
    private static bool ScrCmd_SaveChosenStarter(ScriptContext ctx)
    {
        ScriptManager scriptManager = Manager(ctx);
        ChooseStarterArgs args = (ChooseStarterArgs)scriptManager.DataPtr!;
        SystemVars.SystemVars_SetPlayerStarter(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!), (ushort)args.Species);
        scriptManager.DataPtr = null;
        return false;
    }

    // OFFSET 0x02041D78
    private static bool ScrCmd_ReturnToField(ScriptContext ctx)
    {
        FieldTransition.FieldTransition_StartMap(ctx.FieldSystem.Task!);
        return true;
    }

    // OFFSET 0x02041D88
    private static bool ScrCmd_FinishMap(ScriptContext ctx)
    {
        FieldTransition.FieldTransition_FinishMap(ctx.FieldSystem.Task!);
        return true;
    }

    // OFFSET 0x02042E38
    private static bool ScrCmd_GetPlayerStarterSpecies(ScriptContext ctx)
    {
        BufferView destVar = FieldSystemVarsFlags.FieldSystem_GetVarPointer(ctx.FieldSystem, ScriptContext.ScriptContext_ReadHalfWord(ctx))!;
        destVar.W16(0, (ushort)SystemVars.SystemVars_GetPlayerStarter(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!)));
        return false;
    }

    // OFFSET 0x0204CFFC
    private static bool ScrCmd_GivePokemon(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        int metLocation = MapHeader.MapHeader_GetMapLabelTextID((int)fieldSystem.Location!.U32(0));
        ushort species = ReadVarValue(ctx);
        ushort level = ReadVarValue(ctx);
        ushort heldItem = ReadVarValue(ctx);
        BufferView destVar = FieldSystemVarsFlags.FieldSystem_GetVarPointer(fieldSystem, ScriptContext.ScriptContext_ReadHalfWord(ctx))!;
        Party.SaveData_GetParty(fieldSystem.SaveData!);
        destVar.W16(0, FieldParty.Pokemon_GiveMonFromScript(HeapIDFieldTask, fieldSystem.SaveData!, species, (byte)level, heldItem, metLocation, MetTerrainDefault) ? 1 : 0);
        return false;
    }
}
