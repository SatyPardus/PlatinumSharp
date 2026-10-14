namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private static void RegisterStrings(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x020474F4] = ScrCmd_BufferPlayerName;
        ported[0x02047528] = ScrCmd_BufferRivalName;
        ported[0x0204754C] = ScrCmd_BufferCounterpartName;
        ported[0x020475B8] = ScrCmd_BufferTypeName;
        ported[0x020475F0] = ScrCmd_BufferItemName;
        ported[0x02047628] = ScrCmd_BufferPocketName;
        ported[0x0204769C] = ScrCmd_BufferMoveName;
    }

    // OFFSET 0x020474F4
    private static bool ScrCmd_BufferPlayerName(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!);
        StringTemplate.StringTemplate_SetPlayerName(template, templateArg, trainerInfo);
        return false;
    }

    // OFFSET 0x02047528
    private static bool ScrCmd_BufferRivalName(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetRivalName(template, templateArg, fieldSystem.SaveData!);
        return false;
    }

    // OFFSET 0x0204754C
    private static bool ScrCmd_BufferCounterpartName(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetCounterpartName(template, templateArg, fieldSystem.SaveData!);
        return false;
    }

    // OFFSET 0x020475B8
    private static bool ScrCmd_BufferTypeName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort typeID = VarValue(ctx);
        StringTemplate.StringTemplate_SetPokemonTypeName(template, templateArg, typeID);
        return false;
    }

    // OFFSET 0x020475F0
    private static bool ScrCmd_BufferItemName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort itemID = VarValue(ctx);
        StringTemplate.StringTemplate_SetItemName(template, templateArg, itemID);
        return false;
    }

    // OFFSET 0x02047628
    private static bool ScrCmd_BufferPocketName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort pocketID = VarValue(ctx);
        StringTemplate.StringTemplate_SetBagPocketName(template, templateArg, pocketID);
        return false;
    }

    // OFFSET 0x0204769C
    private static bool ScrCmd_BufferMoveName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort moveID = VarValue(ctx);
        StringTemplate.StringTemplate_SetMoveName(template, templateArg, moveID);
        return false;
    }
}
