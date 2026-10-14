namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private static void RegisterStrings(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x020474F4] = ScrCmd_BufferPlayerName;
        ported[0x02047528] = ScrCmd_BufferRivalName;
        ported[0x0204754C] = ScrCmd_BufferCounterpartName;
        ported[0x02047570] = ScrCmd_BufferPartyMonSpecies;
        ported[0x020475B8] = ScrCmd_BufferTypeName;
        ported[0x020475F0] = ScrCmd_BufferItemName;
        ported[0x02047628] = ScrCmd_BufferPocketName;
        ported[0x0204769C] = ScrCmd_BufferMoveName;
        ported[0x020476D4] = ScrCmd_BufferNumber;
        ported[0x02047E58] = ScrCmd_BufferItemNamePlural;
        ported[0x02047E90] = ScrCmd_BufferUndergroundGoodsNameWithArticle;
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

    // OFFSET 0x020476D4
    private static bool ScrCmd_BufferNumber(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort number = VarValue(ctx);
        StringTemplate.StringTemplate_SetNumber(template, templateArg, number, (uint)FieldNumberHelpers.CountDigits(number), 1, 1);
        return false;
    }

    // OFFSET 0x02047E58
    private static bool ScrCmd_BufferItemNamePlural(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort itemID = VarValue(ctx);
        StringTemplate.StringTemplate_SetItemNamePlural(template, templateArg, itemID);
        return false;
    }

    // OFFSET 0x02047E90
    private static bool ScrCmd_BufferUndergroundGoodsNameWithArticle(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort goodsID = VarValue(ctx);
        StringTemplate.StringTemplate_SetUndergroundGoodsNameWithArticle(template, templateArg, goodsID);
        return false;
    }

    // OFFSET 0x02047570
    private static bool ScrCmd_BufferPartyMonSpecies(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort slot = VarValue(ctx);
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), slot);
        StringTemplate.StringTemplate_SetSpeciesName(template, templateArg, Pokemon.Pokemon_GetBoxPokemon(mon));
        return false;
    }
}
