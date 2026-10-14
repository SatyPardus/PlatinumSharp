namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const int SaveInfoHeapID = 4;
    private const byte SaveInfoBgLayer = 3;

    private static void RegisterSave(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x020434EC] = ScrCmd_CheckSaveType;
        ported[0x02043540] = ScrCmd_TrySaveGame;
        ported[0x02043F40] = ScrCmd_StoreSaveResult;
        ported[0x0204616C] = ScrCmd_OpenSaveInfo;
        ported[0x0204619C] = ScrCmd_CloseSaveInfo;
        ported[0x02045250] = ScrCmd_StartSaveAnimation;
        ported[0x02045274] = ScrCmd_EndSaveAnimation;
    }

    // OFFSET 0x020434EC
    private static bool ScrCmd_CheckSaveType(ScriptContext ctx)
    {
        SaveData saveData = ctx.FieldSystem.SaveData!;
        BufferView destVar = VarPointer(ctx);
        if (SaveData.SaveData_OverwriteCheck(saveData))
        {
            destVar.W16(0, 0);
        }
        else if (SaveData.SaveData_DataExists(saveData) == 0)
        {
            destVar.W16(0, 1);
        }
        else if (SaveData.SaveData_FullSaveRequired(saveData) != 0)
        {
            destVar.W16(0, 2);
        }
        else
        {
            destVar.W16(0, 3);
        }
        return false;
    }

    // OFFSET 0x02043540
    private static bool ScrCmd_TrySaveGame(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        BufferView destVar = VarPointer(ctx);
        destVar.W16(0, FieldSave.FieldSystem_Save(fieldSystem) ? 1 : 0);
        return false;
    }

    // OFFSET 0x02043F40
    private static bool ScrCmd_StoreSaveResult(ScriptContext ctx)
    {
        ScriptManager scriptManager = ScriptManager.FieldSystem_GetScriptManager(ctx.FieldSystem);
        ushort value = VarValue(ctx);
        scriptManager.SaveType?.W16(0, value);
        return false;
    }

    // OFFSET 0x0204616C
    private static bool ScrCmd_OpenSaveInfo(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ScriptManager scriptManager = ScriptManager.FieldSystem_GetScriptManager(fieldSystem);
        if (!SaveData.SaveData_OverwriteCheck(fieldSystem.SaveData!))
        {
            SaveInfoWindow window = SaveInfoWindows.SaveInfoWindow_New(fieldSystem, SaveInfoHeapID, SaveInfoBgLayer);
            scriptManager.SaveInfoWindow = window;
            SaveInfoWindows.SaveInfoWindow_Draw(window);
        }
        return false;
    }

    // OFFSET 0x0204619C
    private static bool ScrCmd_CloseSaveInfo(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ScriptManager scriptManager = ScriptManager.FieldSystem_GetScriptManager(fieldSystem);
        if (!SaveData.SaveData_OverwriteCheck(fieldSystem.SaveData!))
        {
            SaveInfoWindow window = (SaveInfoWindow)scriptManager.SaveInfoWindow!;
            SaveInfoWindows.SaveInfoWindow_Erase(window);
            SaveInfoWindows.SaveInfoWindow_Free(window);
        }
        return false;
    }

    // OFFSET 0x02045250
    private static bool ScrCmd_StartSaveAnimation(ScriptContext ctx)
    {
        ScriptManager scriptManager = ScriptManager.FieldSystem_GetScriptManager(ctx.FieldSystem);
        scriptManager.PlayerTask = null;
        scriptManager.PlayerTask = PlayerFieldMoves.FieldSystem_StartVsSeekerTask(ctx.FieldSystem);
        return true;
    }

    // OFFSET 0x02045274
    private static bool ScrCmd_EndSaveAnimation(ScriptContext ctx)
    {
        ScriptManager scriptManager = ScriptManager.FieldSystem_GetScriptManager(ctx.FieldSystem);
        PlayerFieldMoves.FieldSystem_EndVsSeekerTask(scriptManager.PlayerTask);
        return true;
    }
}
