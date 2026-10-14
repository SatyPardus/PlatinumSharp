namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const ushort MenuNoSelectionYet = 0xEEEE;

    private static void RegisterMenus(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x020408B0] = ScrCmd_InitGlobalTextMenu;
        ported[0x02040938] = ScrCmd_InitLocalTextMenu;
        ported[0x020409C0] = ScrCmd_AddMenuEntryImm;
        ported[0x020409E8] = ScrCmd_AddMenuEntry;
        ported[0x02040A2C] = ScrCmd_ShowMenu;
        ported[0x02040D08] = ScrCmd_ShowMenuMultiColumn;
    }

    private static bool ScrCmd_InitTextMenu(ScriptContext ctx, MessageLoader? loader)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ScriptManager manager = Manager(ctx);
        byte anchorX = ScriptContext.ScriptContext_ReadByte(ctx);
        byte anchorY = ScriptContext.ScriptContext_ReadByte(ctx);
        byte initialCursorPos = ScriptContext.ScriptContext_ReadByte(ctx);
        byte canExitWithB = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort varID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        BufferView selectedOption = FieldSystemVarsFlags.FieldSystem_GetVarPointer(fieldSystem, varID)!;
        manager.FieldMenuManager = FieldMenus.FieldMenuManager_New(fieldSystem, anchorX, anchorY, initialCursorPos, canExitWithB, selectedOption, manager.StringTemplate, manager.Window, loader);
        ctx.Data[0] = varID;
        return true;
    }

    // OFFSET 0x020408B0
    private static bool ScrCmd_InitGlobalTextMenu(ScriptContext ctx)
    {
        return ScrCmd_InitTextMenu(ctx, null);
    }

    // OFFSET 0x02040938
    private static bool ScrCmd_InitLocalTextMenu(ScriptContext ctx)
    {
        return ScrCmd_InitTextMenu(ctx, ctx.Loader);
    }

    // OFFSET 0x020409C0
    private static bool ScrCmd_AddMenuEntryImm(ScriptContext ctx)
    {
        FieldMenuManager menuManager = (FieldMenuManager)Manager(ctx).FieldMenuManager!;
        byte entryID = ScriptContext.ScriptContext_ReadByte(ctx);
        byte index = ScriptContext.ScriptContext_ReadByte(ctx);
        FieldMenus.FieldMenuManager_AddMenuEntry(menuManager, entryID, index);
        return false;
    }

    // OFFSET 0x020409E8
    private static bool ScrCmd_AddMenuEntry(ScriptContext ctx)
    {
        FieldMenuManager menuManager = (FieldMenuManager)Manager(ctx).FieldMenuManager!;
        ushort entryID = VarValue(ctx);
        ushort index = VarValue(ctx);
        FieldMenus.FieldMenuManager_AddMenuEntry(menuManager, entryID, index);
        return false;
    }

    // OFFSET 0x02040A2C
    private static bool ScrCmd_ShowMenu(ScriptContext ctx)
    {
        FieldMenus.FieldMenuManager_ShowSingleColumnMenu((FieldMenuManager)Manager(ctx).FieldMenuManager!);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMenuSelection);
        return true;
    }

    // OFFSET 0x02040A50
    private static bool ScriptContext_WaitForMenuSelection(ScriptContext ctx)
    {
        BufferView selectedOption = FieldSystemVarsFlags.FieldSystem_GetVarPointer(ctx.FieldSystem, (ushort)ctx.Data[0])!;
        return selectedOption.U16(0) != MenuNoSelectionYet;
    }

    // OFFSET 0x02040D08
    private static bool ScrCmd_ShowMenuMultiColumn(ScriptContext ctx)
    {
        FieldMenuManager menuManager = (FieldMenuManager)Manager(ctx).FieldMenuManager!;
        FieldMenus.FieldMenuManager_ShowMultiColumnMenu(menuManager, ScriptContext.ScriptContext_ReadByte(ctx));
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMenuSelection);
        return true;
    }
}
