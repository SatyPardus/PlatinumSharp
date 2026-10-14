namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const int StringHeapIDField = 4;
    private const int StringHeapIDFieldTask = 0xB;
    private const int StringHeapIDBerry = 0x20;
    private const int MapNameLength = 0x16;
    private const int TabletNameLength = 0xB;
    private const int ItemFirstBerry = 0x95;
    private const int MonParamMove1 = 0x36;
    private const int RibbonDataNameID = 3;
    private const int GameLanguageEnglish = 2;

    private static void RegisterStrings2(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x02047660] = ScrCmd_BufferTMHMMoveName;
        ported[0x0204771C] = ScrCmd_BufferVarPaddingDigits;
        ported[0x0204777C] = ScrCmd_BufferValuePaddingDigits;
        ported[0x020477D0] = ScrCmd_BufferPartyMonNickname;
        ported[0x02047818] = ScrCmd_BufferMonNicknameFromPC;
        ported[0x02047874] = ScrCmd_BufferPoketchAppName;
        ported[0x020478AC] = ScrCmd_BufferTrainerClassName;
        ported[0x020478E4] = ScrCmd_BufferTrainerClassFromAppearance;
        ported[0x02047930] = ScrCmd_BufferSpeciesNameFromVar;
        ported[0x02047AB0] = ScrCmd_BufferUndergroundGoodsName;
        ported[0x02047AE8] = ScrCmd_BufferUndergroundTrapName;
        ported[0x02047B20] = ScrCmd_BufferUndergroundItemName;
        ported[0x02047B58] = ScrCmd_BufferMapName;
        ported[0x02047BB8] = ScrCmd_BufferBerryName;
        ported[0x02047C30] = ScrCmd_BufferNatureName;
        ported[0x02047C68] = ScrCmd_BufferAccessoryName;
        ported[0x02047CA0] = ScrCmd_BufferTabletName;
        ported[0x02047D00] = ScrCmd_BufferPartyMoveName;
        ported[0x02047D68] = ScrCmd_BufferRibbonName;
        ported[0x02047DAC] = ScrCmd_BufferBallSealName;
        ported[0x02047DE8] = ScrCmd_BufferTrainerName;
        ported[0x02047E20] = ScrCmd_BufferItemNameWithArticle;
        ported[0x02047EC8] = ScrCmd_BufferUndergroundTrapNameWithArticle;
        ported[0x02047F00] = ScrCmd_BufferUndergroundItemNameWithArticle;
        ported[0x02047F38] = ScrCmd_BufferSpeciesNameWithArticle;
        ported[0x02047F80] = ScrCmd_BufferPlayerCounterpartStarterSpeciesNameWithArticle;
        ported[0x02047FB4] = ScrCmd_BufferAccessoryNameWithArticle;
        ported[0x02047FEC] = ScrCmd_BufferTrainerClassNameWithArticle;
    }

    // OFFSET 0x02047660
    private static bool ScrCmd_BufferTMHMMoveName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort item = VarValue(ctx);
        StringTemplate.StringTemplate_SetMoveName(template, templateArg, Item.Item_MoveForTMHM(item));
        return false;
    }

    // OFFSET 0x0204771C
    private static bool ScrCmd_BufferVarPaddingDigits(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort value = VarValue(ctx);
        byte paddingMode = ScriptContext.ScriptContext_ReadByte(ctx);
        byte digits = ScriptContext.ScriptContext_ReadByte(ctx);
        if (paddingMode == 0)
        {
            digits = (byte)FieldNumberHelpers.CountDigits(value);
        }
        StringTemplate.StringTemplate_SetNumber(template, templateArg, value, digits, paddingMode, 1);
        return false;
    }

    // OFFSET 0x0204777C
    private static bool ScrCmd_BufferValuePaddingDigits(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        uint value = ScriptContext.ScriptContext_ReadWord(ctx);
        byte paddingMode = ScriptContext.ScriptContext_ReadByte(ctx);
        byte digits = ScriptContext.ScriptContext_ReadByte(ctx);
        if (paddingMode == 0)
        {
            digits = (byte)FieldNumberHelpers.CountDigits(value);
        }
        StringTemplate.StringTemplate_SetNumber(template, templateArg, (int)value, digits, paddingMode, 1);
        return false;
    }

    // OFFSET 0x020477D0
    private static bool ScrCmd_BufferPartyMonNickname(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort slot = VarValue(ctx);
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(ctx.FieldSystem.SaveData!), slot);
        StringTemplate.StringTemplate_SetNickname(template, templateArg, Pokemon.Pokemon_GetBoxPokemon(mon));
        return false;
    }

    // OFFSET 0x02047818
    private static bool ScrCmd_BufferMonNicknameFromPC(ScriptContext ctx)
    {
        PCBoxes boxes = (PCBoxes)SaveDataTable.SaveData_GetPCBoxes(ctx.FieldSystem.SaveData!);
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        int index = VarValue(ctx);
        BoxPokemon boxMon = PCBoxes.PCBoxes_GetBoxMonAt(boxes, index / PCBoxes.MonsPerBox, index % PCBoxes.MonsPerBox);
        StringTemplate.StringTemplate_SetNickname(template, templateArg, boxMon);
        return false;
    }

    // OFFSET 0x02047874
    private static bool ScrCmd_BufferPoketchAppName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetPoketchAppName(template, templateArg, VarValue(ctx));
        return false;
    }

    // OFFSET 0x020478AC
    private static bool ScrCmd_BufferTrainerClassName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetTrainerClassName(template, templateArg, VarValue(ctx));
        return false;
    }

    // OFFSET 0x020478E4
    private static bool ScrCmd_BufferTrainerClassFromAppearance(ScriptContext ctx)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(ctx.FieldSystem.SaveData!);
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        uint gender = TrainerInfo.TrainerInfo_Gender(trainerInfo);
        int trainerClass = PlayerAppearance.PlayerAppearance_GetData(gender, TrainerInfo.TrainerInfo_Appearance(trainerInfo), PlayerAppearance.AppearanceFieldTrainerClassArticle);
        StringTemplate.StringTemplate_SetTrainerClassNameWithArticle(template, templateArg, (uint)trainerClass);
        return false;
    }

    // OFFSET 0x02047930
    private static bool ScrCmd_BufferSpeciesNameFromVar(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort species = VarValue(ctx);
        ushort unused = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        byte unused2 = ScriptContext.ScriptContext_ReadByte(ctx);
        GameString name = Species_GetNameString(species, StringHeapIDField)!;
        StringTemplate.StringTemplate_SetString(template, templateArg, name, unused, unused2, GameLanguageEnglish);
        GameString.String_Free(name);
        return false;
    }

    // OFFSET 0x02047AB0
    private static bool ScrCmd_BufferUndergroundGoodsName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetUndergroundGoodsName(template, templateArg, VarValue(ctx));
        return false;
    }

    // OFFSET 0x02047AE8
    private static bool ScrCmd_BufferUndergroundTrapName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetUndergroundTrapName(template, templateArg, VarValue(ctx));
        return false;
    }

    // OFFSET 0x02047B20
    private static bool ScrCmd_BufferUndergroundItemName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetUndergroundItemName(template, templateArg, VarValue(ctx));
        return false;
    }

    // OFFSET 0x02047B58
    private static bool ScrCmd_BufferMapName(ScriptContext ctx)
    {
        GameString name = GameString.String_Init(MapNameLength, StringHeapIDField);
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        MapHeader.MapHeader_LoadName(VarValue(ctx), StringHeapIDField, name);
        StringTemplate.StringTemplate_SetString(template, templateArg, name, 0, 1, GameLanguageEnglish);
        GameString.String_Free(name);
        return false;
    }

    // OFFSET 0x02047BB8
    private static bool ScrCmd_BufferBerryName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort item = VarValue(ctx);
        ushort count = VarValue(ctx);
        GameString name = BerryData.BerryData_AllocAndGetName((ushort)(item - ItemFirstBerry), StringHeapIDBerry)!;
        StringTemplate.StringTemplate_SetString(template, templateArg, name, 0, count < 2 ? 1 : 0, GameLanguageEnglish);
        GameString.String_Free(name);
        return false;
    }

    // OFFSET 0x02047C30
    private static bool ScrCmd_BufferNatureName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetNatureName(template, templateArg, VarValue(ctx));
        return false;
    }

    // OFFSET 0x02047C68
    private static bool ScrCmd_BufferAccessoryName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetContestAccessoryName(template, templateArg, VarValue(ctx));
        return false;
    }

    // OFFSET 0x02047CA0
    private static bool ScrCmd_BufferTabletName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        GameString name = GameString.String_Init(TabletNameLength, StringHeapIDFieldTask);
        GameString.String_CopyChars(name, MiscSaveBlock.MiscSaveBlock_TabletName(MiscSaveBlock.SaveData_MiscSaveBlock(ctx.FieldSystem.SaveData!)));
        StringTemplate.StringTemplate_SetString(template, templateArg, name, 0, 0, GameLanguageEnglish);
        GameString.String_Free(name);
        return false;
    }

    // OFFSET 0x02047D00
    private static bool ScrCmd_BufferPartyMoveName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort slot = VarValue(ctx);
        ushort moveSlot = VarValue(ctx);
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(ctx.FieldSystem.SaveData!), slot);
        ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, MonParamMove1 + moveSlot, null);
        StringTemplate.StringTemplate_SetMoveName(template, templateArg, move);
        return false;
    }

    // OFFSET 0x02047D68
    private static bool ScrCmd_BufferRibbonName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        byte ribbon = (byte)VarValue(ctx);
        StringTemplate.StringTemplate_SetRibbonName(template, templateArg, (ushort)Ribbon.Ribbon_GetData(ribbon, RibbonDataNameID));
        return false;
    }

    // OFFSET 0x02047DAC
    private static bool ScrCmd_BufferBallSealName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetBallSealName(template, templateArg, (ushort)(VarValue(ctx) - 1));
        return false;
    }

    // OFFSET 0x02047DE8
    private static bool ScrCmd_BufferTrainerName(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetTrainerName(template, templateArg, VarValue(ctx));
        return false;
    }

    // OFFSET 0x02047E20
    private static bool ScrCmd_BufferItemNameWithArticle(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetItemNameWithArticle(template, templateArg, VarValue(ctx));
        return false;
    }

    // OFFSET 0x02047EC8
    private static bool ScrCmd_BufferUndergroundTrapNameWithArticle(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetUndergroundTrapNameWithArticle(template, templateArg, VarValue(ctx));
        return false;
    }

    // OFFSET 0x02047F00
    private static bool ScrCmd_BufferUndergroundItemNameWithArticle(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetUndergroundItemNameWithArticle(template, templateArg, VarValue(ctx));
        return false;
    }

    // OFFSET 0x02047F38
    private static bool ScrCmd_BufferSpeciesNameWithArticle(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort species = VarValue(ctx);
        ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetSpeciesNameWithArticleByID(template, templateArg, species);
        return false;
    }

    // OFFSET 0x02047F80
    private static bool ScrCmd_BufferPlayerCounterpartStarterSpeciesNameWithArticle(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        ushort species = SystemVars.SystemVars_GetPlayerCounterpartStarter(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        StringTemplate.StringTemplate_SetSpeciesNameWithArticleByID(template, templateArg, species);
        return false;
    }

    // OFFSET 0x02047FB4
    private static bool ScrCmd_BufferAccessoryNameWithArticle(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetContestAccessoryNameWithArticle(template, templateArg, VarValue(ctx));
        return false;
    }

    // OFFSET 0x02047FEC
    private static bool ScrCmd_BufferTrainerClassNameWithArticle(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        byte templateArg = ScriptContext.ScriptContext_ReadByte(ctx);
        StringTemplate.StringTemplate_SetTrainerClassNameWithArticle(template, templateArg, VarValue(ctx));
        return false;
    }
}
