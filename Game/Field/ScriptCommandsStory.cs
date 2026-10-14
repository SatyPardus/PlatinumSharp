namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const int NarcMessageBanks = 0x1A;
    private const int MessageBankSpeciesNames = 0x19C;
    private const int MessageLoaderTypeNarc = 1;
    private const int WarpIDNone = -1;

    private static void RegisterStory(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x02042984] = ScrCmd_Warp;
        ported[0x0204342C] = ScrCmd_StartFirstBattle;
        ported[0x02043C3C] = ScrCmd_HealParty;
        ported[0x020479C0] = ScrCmd_BufferPlayerStarterSpeciesName;
        ported[0x02047A10] = ScrCmd_BufferRivalStarterSpeciesName;
        ported[0x02047A60] = ScrCmd_BufferCounterpartStarterSpeciesName;
        ported[0x02048A50] = ScrCmd_CheckWonBattle;
        ported[0x02048A80] = ScrCmd_CheckLostBattle;
        ported[0x02048AB0] = ScrCmd_CheckDidNotCapture;
        ported[0x0204E594] = ScrCmd_SetHasPartner;
        ported[0x0204E5A8] = ScrCmd_ClearHasPartner;
        ported[0x0204E5BC] = ScrCmd_CheckStepFlag;
        ported[0x0204E5E8] = ScrCmd_SetStepFlag;
        ported[0x0204E5FC] = ScrCmd_ClearStepFlag;
        ported[0x0204E80C] = ScrCmd_CanFitItem;
    }

    // OFFSET 0x02042984
    private static bool ScrCmd_Warp(ScriptContext ctx)
    {
        ushort mapID = ReadVarValue(ctx);
        ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort x = ReadVarValue(ctx);
        ushort z = ReadVarValue(ctx);
        ushort direction = ReadVarValue(ctx);
        FieldMapChange.FieldTask_StartMapChangeFull(ctx.Task!, mapID, WarpIDNone, x, z, direction);
        return true;
    }

    // OFFSET 0x0204342C
    private static bool ScrCmd_StartFirstBattle(ScriptContext ctx)
    {
        ScriptManager manager = Manager(ctx);
        ushort trainerID = ReadVarValue(ctx);
        Encounters.Encounter_NewVsFirstBattle(ctx.Task!, trainerID, HeapIDFieldTask, result => manager.BattleResult = result);
        return true;
    }

    // OFFSET 0x02043C3C
    private static bool ScrCmd_HealParty(ScriptContext ctx)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(ctx.Task!);
        PartyHeal.Party_HealAllMembers(Party.SaveData_GetParty(fieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x02047998
    private static GameString? Species_GetNameString(ushort species, int heapID)
    {
        MessageLoader? loader = MessageLoader.MessageLoader_Init(MessageLoaderTypeNarc, NarcMessageBanks, MessageBankSpeciesNames, heapID);
        GameString? name = MessageLoader.MessageLoader_GetNewString(loader!, species);
        MessageLoader.MessageLoader_Free(loader);
        return name;
    }

    // OFFSET 0x020479C0
    private static bool ScrCmd_BufferPlayerStarterSpeciesName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort species = SystemVars.SystemVars_GetPlayerStarter(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        GameString? name = Species_GetNameString(species, HeapIDField);
        StringTemplate.StringTemplate_SetString(template, templateArg, name!, 0, 1, 2);
        GameString.String_Free(name);
        return false;
    }

    // OFFSET 0x02047A10
    private static bool ScrCmd_BufferRivalStarterSpeciesName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort species = SystemVars.SystemVars_GetRivalStarter(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        GameString? name = Species_GetNameString(species, HeapIDField);
        StringTemplate.StringTemplate_SetString(template, templateArg, name!, 0, 1, 2);
        GameString.String_Free(name);
        return false;
    }

    // OFFSET 0x02047A60
    private static bool ScrCmd_BufferCounterpartStarterSpeciesName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort species = SystemVars.SystemVars_GetPlayerCounterpartStarter(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        GameString? name = Species_GetNameString(species, HeapIDField);
        StringTemplate.StringTemplate_SetString(template, templateArg, name!, 0, 1, 2);
        GameString.String_Free(name);
        return false;
    }

    // OFFSET 0x02048A50
    private static bool ScrCmd_CheckWonBattle(ScriptContext ctx)
    {
        ScriptManager manager = Manager(ctx);
        BufferView destVar = VarPointer(ctx);
        destVar.W16(0, FieldBattleResult.CheckPlayerWonBattle(manager.BattleResult));
        return true;
    }

    // OFFSET 0x02048A80
    private static bool ScrCmd_CheckLostBattle(ScriptContext ctx)
    {
        ScriptManager manager = Manager(ctx);
        BufferView destVar = VarPointer(ctx);
        destVar.W16(0, FieldBattleResult.CheckPlayerLostBattle(manager.BattleResult));
        return true;
    }

    // OFFSET 0x02048AB0
    private static bool ScrCmd_CheckDidNotCapture(ScriptContext ctx)
    {
        ScriptManager manager = Manager(ctx);
        BufferView destVar = VarPointer(ctx);
        destVar.W16(0, FieldBattleResult.CheckPlayerDidNotCapture(manager.BattleResult));
        return true;
    }

    // OFFSET 0x0204E594
    private static bool ScrCmd_SetHasPartner(ScriptContext ctx)
    {
        SystemFlags.SystemFlag_SetHasPartner(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x0204E5A8
    private static bool ScrCmd_ClearHasPartner(ScriptContext ctx)
    {
        SystemFlags.SystemFlag_ClearHasPartner(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x0204E5BC
    private static bool ScrCmd_CheckStepFlag(ScriptContext ctx)
    {
        BufferView destVar = VarPointer(ctx);
        destVar.W16(0, SystemFlags.SystemFlag_CheckStep(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!)) ? 1 : 0);
        return false;
    }

    // OFFSET 0x0204E5E8
    private static bool ScrCmd_SetStepFlag(ScriptContext ctx)
    {
        SystemFlags.SystemFlag_SetStep(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x0204E5FC
    private static bool ScrCmd_ClearStepFlag(ScriptContext ctx)
    {
        SystemFlags.SystemFlag_ClearStep(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x0204E80C
    private static bool ScrCmd_CanFitItem(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort item = ReadVarValue(ctx);
        ushort count = ReadVarValue(ctx);
        BufferView destVar = VarPointer(ctx);
        destVar.W16(0, BagItems.Bag_CanFitItem(BagItems.SaveData_GetBag(fieldSystem.SaveData!), item, count, HeapIDField) ? 1 : 0);
        return false;
    }
}
