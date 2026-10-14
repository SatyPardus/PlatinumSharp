using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class Trainer
{
    public byte MonDataType;
    public byte TrainerClass;
    public readonly ushort[] Name = new ushort[8];
}

public struct StringTemplateArgHeader
{
    public byte Unused0;
    public byte Unused1;
    public byte Unused2;
    public byte Unused3;
}

public sealed class StringTemplateArg
{
    public StringTemplateArgHeader Header;
    public GameString String = new();
}

public sealed class StringTemplate
{
    private const int MessageArchive = 0x1A;
    private const int MonDataSpecies = 5;
    private const int MonDataNicknameString = 0x77;
    private const int MonDataOTNameString = 0x91;
    private const ushort FormatCode = 0xFFFE;

    private static ushort[]? sMetLocationBanks;

    public uint Count;
    public int HeapID;
    public StringTemplateArg[] Args = Array.Empty<StringTemplateArg>();
    public GameString Buffer = new();

    private static void SetArgFromArchive(StringTemplate template, uint idx, int bankID, uint entryID)
    {
        MessageLoader? loader = MessageLoader.MessageLoader_Init(MessageLoader.TypeNarcHandle, MessageArchive, bankID, template.HeapID);
        if (loader != null)
        {
            MessageLoader.MessageLoader_GetString(loader, entryID, template.Buffer);
            SetStringTemplateArg(template, idx, template.Buffer, null);
            MessageLoader.MessageLoader_Free(loader);
        }
    }

    // OFFSET 0x0200B358
    public static StringTemplate StringTemplate_Default(int heapID)
    {
        return StringTemplate_New(8, 0x20, heapID);
    }

    // OFFSET 0x0200B368
    public static StringTemplate StringTemplate_New(uint count, uint maxLength, int heapID)
    {
        if (count == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (maxLength == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        StringTemplate template = new();
        template.Count = count;
        template.HeapID = heapID;
        template.Buffer = GameString.String_Init(maxLength, heapID);
        template.Args = new StringTemplateArg[count];
        for (uint i = 0; i < count; i++)
        {
            template.Args[i] = new StringTemplateArg();
            InitStringTemplateArgHeader(ref template.Args[i].Header);
            template.Args[i].String = GameString.String_Init(maxLength, heapID);
        }
        return template;
    }

    // OFFSET 0x0200B3F0
    public static void StringTemplate_Free(StringTemplate template)
    {
        if (template.Count == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (uint i = 0; i < template.Count; i++)
        {
            GameString.String_Free(template.Args[i].String);
        }
        GameString.String_Free(template.Buffer);
        template.Count = 0;
    }

    // OFFSET 0x0200B444
    private static void InitStringTemplateArgHeader(ref StringTemplateArgHeader header)
    {
    }

    // OFFSET 0x0200B448
    private static void SetStringTemplateArg(StringTemplate template, uint idx, GameString str, StringTemplateArgHeader? header)
    {
        if (idx >= template.Count)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (idx < template.Count)
        {
            if (header != null)
            {
                template.Args[idx].Header = header.Value;
            }
            GameString.String_Copy(template.Args[idx].String, str);
        }
    }

    // OFFSET 0x0200B48C
    public static void StringTemplate_SetString(StringTemplate template, uint idx, GameString str, int unused3 = 0, int unused4 = 0, int unused5 = 0)
    {
        SetStringTemplateArg(template, idx, str, null);
    }

    // OFFSET 0x0200B498
    public static void StringTemplate_SetPlayerName(StringTemplate template, uint idx, object trainerInfo)
    {
        GameString.String_CopyChars(template.Buffer, TrainerInfo.TrainerInfo_Name((TrainerInfo)trainerInfo));
        SetStringTemplateArg(template, idx, template.Buffer, null);
    }

    // OFFSET 0x0200B4BC
    public static void StringTemplate_SetRivalName(StringTemplate template, uint idx, object saveData)
    {
        GameString.String_CopyChars(template.Buffer, MiscSaveBlock.MiscSaveBlock_RivalName(MiscSaveBlock.SaveData_MiscSaveBlockConst((SaveData)saveData)));
        SetStringTemplateArg(template, idx, template.Buffer, null);
    }

    // OFFSET 0x0200B4E4
    public static void StringTemplate_SetCounterpartName(StringTemplate template, uint idx, object saveData)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo((SaveData)saveData);
        MessageLoader loader = MessageLoader.MessageLoader_Init(MessageLoader.TypeNarcHandle, MessageArchive, 0x229, template.HeapID)!;
        if (TrainerInfo.TrainerInfo_Gender(trainerInfo) == 0)
        {
            MessageLoader.MessageLoader_GetString(loader, 1, template.Buffer);
        }
        else
        {
            MessageLoader.MessageLoader_GetString(loader, 0, template.Buffer);
        }
        SetStringTemplateArg(template, idx, template.Buffer, null);
        MessageLoader.MessageLoader_Free(loader);
    }

    // OFFSET 0x0200B538
    public static void StringTemplate_SetSpeciesName(StringTemplate template, uint idx, BoxPokemon boxMon)
    {
        MessageLoader loader = MessageLoader.MessageLoader_Init(MessageLoader.TypeNarcHandle, MessageArchive, 0x19C, template.HeapID)!;
        uint species = Pokemon.BoxPokemon_GetValue(boxMon, MonDataSpecies, null);
        MessageLoader.MessageLoader_GetString(loader, species, template.Buffer);
        SetStringTemplateArg(template, idx, template.Buffer, null);
        MessageLoader.MessageLoader_Free(loader);
    }

    // OFFSET 0x0200B578
    public static void StringTemplate_SetSpeciesNameWithArticle(StringTemplate template, uint idx, BoxPokemon boxMon)
    {
        uint species = Pokemon.BoxPokemon_GetValue(boxMon, MonDataSpecies, null);
        StringTemplate_SetSpeciesNameWithArticleByID(template, idx, species);
    }

    // OFFSET 0x0200B594
    public static void StringTemplate_SetSpeciesNameWithArticleByID(StringTemplate template, uint idx, uint species)
    {
        MessageLoader loader = MessageLoader.MessageLoader_Init(MessageLoader.TypeNarcHandle, MessageArchive, 0x19D, template.HeapID)!;
        MessageLoader.MessageLoader_GetString(loader, species, template.Buffer);
        SetStringTemplateArg(template, idx, template.Buffer, null);
        MessageLoader.MessageLoader_Free(loader);
    }

    // OFFSET 0x0200B5CC
    public static void StringTemplate_SetNickname(StringTemplate template, uint idx, BoxPokemon boxMon)
    {
        Pokemon.BoxPokemon_GetValue(boxMon, MonDataNicknameString, template.Buffer);
        SetStringTemplateArg(template, idx, template.Buffer, null);
    }

    // OFFSET 0x0200B5EC
    public static void StringTemplate_SetOTName(StringTemplate template, uint idx, BoxPokemon boxMon)
    {
        Pokemon.BoxPokemon_GetValue(boxMon, MonDataOTNameString, template.Buffer);
        SetStringTemplateArg(template, idx, template.Buffer, null);
    }

    // OFFSET 0x0200B60C
    public static void StringTemplate_SetNumber(StringTemplate template, uint idx, int num, uint maxDigits, int paddingMode, int charsetMode)
    {
        GameString.String_FormatInt(template.Buffer, num, maxDigits, paddingMode, charsetMode);
        SetStringTemplateArg(template, idx, template.Buffer, null);
    }

    // OFFSET 0x0200B630
    public static void StringTemplate_SetMoveName(StringTemplate template, uint idx, uint moveID)
    {
        SetArgFromArchive(template, idx, 0x287, moveID);
    }

    // OFFSET 0x0200B668
    public static void StringTemplate_SetRibbonName(StringTemplate template, uint idx, uint ribbonID)
    {
        SetArgFromArchive(template, idx, 0x217, ribbonID);
    }

    // OFFSET 0x0200B6A0
    public static void StringTemplate_SetAbilityName(StringTemplate template, uint idx, uint abilityID)
    {
        SetArgFromArchive(template, idx, 0x262, abilityID);
    }

    // OFFSET 0x0200B6D8
    public static void StringTemplate_SetNatureName(StringTemplate template, uint idx, uint natureID)
    {
        MessageLoader loader = MessageLoader.MessageLoader_Init(MessageLoader.TypeNarcHandle, MessageArchive, 0xCA, template.HeapID)!;
        MessageLoader.MessageLoader_GetString(loader, natureID, template.Buffer);
        SetStringTemplateArg(template, idx, template.Buffer, null);
        MessageLoader.MessageLoader_Free(loader);
    }

    // OFFSET 0x0200B70C
    public static void StringTemplate_SetItemName(StringTemplate template, uint idx, uint itemID)
    {
        SetArgFromArchive(template, idx, 0x188, itemID);
    }

    // OFFSET 0x0200B744
    public static void StringTemplate_SetItemNameWithArticle(StringTemplate template, uint idx, uint itemID)
    {
        SetArgFromArchive(template, idx, 0x189, itemID);
    }

    // OFFSET 0x0200B77C
    public static void StringTemplate_SetItemNamePlural(StringTemplate template, uint idx, uint itemID)
    {
        SetArgFromArchive(template, idx, 0x18A, itemID);
    }

    // OFFSET 0x0200B7B4
    public static void StringTemplate_SetBagPocketName(StringTemplate template, uint idx, uint pocketID)
    {
        SetArgFromArchive(template, idx, 0x18C, pocketID);
    }

    // OFFSET 0x0200B7EC
    public static void StringTemplate_SetPokemonTypeName(StringTemplate template, uint idx, uint typeID)
    {
        SetArgFromArchive(template, idx, 0x270, typeID);
    }

    // OFFSET 0x0200B824
    public static void StringTemplate_SetPokemonStatName(StringTemplate template, uint idx, uint statID)
    {
        SetArgFromArchive(template, idx, 0x227, statID);
    }

    // OFFSET 0x0200B85C
    public static void StringTemplate_SetStatusConditionName(StringTemplate template, uint idx, uint conditionID)
    {
        SetArgFromArchive(template, idx, 0xDB, conditionID);
    }

    // OFFSET 0x0200B890
    public static void StringTemplate_SetFlavorName(StringTemplate template, uint idx, uint flavorID)
    {
        SetArgFromArchive(template, idx, 0x25E, flavorID);
    }

    // OFFSET 0x0200B8C8
    public static void StringTemplate_SetLocationName(StringTemplate template, uint idx, uint location)
    {
        MessageLoader? loader = MessageLoader.MessageLoader_Init(MessageLoader.TypeNarcHandle, MessageArchive, 0x1B1, template.HeapID);
        if (loader == null)
        {
            return;
        }
        if (location == 0 || location >= MessageLoader.MessageLoader_MessageCount(loader))
        {
            MessageLoader.MessageLoader_Free(loader);
            loader = MessageLoader.MessageLoader_Init(MessageLoader.TypeNarcHandle, MessageArchive, 0x1B2, template.HeapID)!;
            location = 2;
        }
        MessageLoader.MessageLoader_GetString(loader, location, template.Buffer);
        SetStringTemplateArg(template, idx, template.Buffer, null);
        MessageLoader.MessageLoader_Free(loader);
    }

    // OFFSET 0x0200B928
    public static void StringTemplate_SetPoketchAppName(StringTemplate template, uint idx, uint appID)
    {
        SetArgFromArchive(template, idx, 0x1C9, appID);
    }

    // OFFSET 0x0200B960
    public static void StringTemplate_SetTrainerClassName(StringTemplate template, uint idx, uint trainerClass)
    {
        SetArgFromArchive(template, idx, 0x26B, trainerClass);
    }

    // OFFSET 0x0200B998
    public static void StringTemplate_SetTrainerClassNameWithArticle(StringTemplate template, uint idx, uint trainerClass)
    {
        SetArgFromArchive(template, idx, 0x26C, trainerClass);
    }

    // OFFSET 0x0200B9D0
    public static void StringTemplate_SetTrainerClassNameBattle(StringTemplate template, uint idx, Trainer trainer)
    {
        SetArgFromArchive(template, idx, 0x26B, trainer.TrainerClass);
    }

    // OFFSET 0x0200BA08
    public static void StringTemplate_SetTrainerName(StringTemplate template, uint idx, uint trainerID)
    {
        SetArgFromArchive(template, idx, 0x26A, trainerID);
    }

    // OFFSET 0x0200BA40
    public static void StringTemplate_SetFrontierTrainerName(StringTemplate template, uint idx, uint trainerID)
    {
        SetArgFromArchive(template, idx, 0x15, trainerID);
    }

    // OFFSET 0x0200BA74
    public static void StringTemplate_SetTrainerNameBattle(StringTemplate template, uint idx, Trainer trainer)
    {
        GameString.String_CopyChars(template.Buffer, trainer.Name);
        SetStringTemplateArg(template, idx, template.Buffer, null);
    }

    // OFFSET 0x0200BA94
    public static void StringTemplate_SetUndergroundItemName(StringTemplate template, uint idx, uint itemID)
    {
        SetArgFromArchive(template, idx, 0x274, itemID);
    }

    // OFFSET 0x0200BACC
    public static void StringTemplate_SetUndergroundItemNameWithArticle(StringTemplate template, uint idx, uint itemID)
    {
        SetArgFromArchive(template, idx, 0x275, itemID);
    }

    // OFFSET 0x0200BB04
    public static void StringTemplate_SetUndergroundTrapName(StringTemplate template, uint idx, uint trapID)
    {
        SetArgFromArchive(template, idx, 0x276, trapID);
    }

    // OFFSET 0x0200BB3C
    public static void StringTemplate_SetUndergroundTrapNameWithArticle(StringTemplate template, uint idx, uint trapID)
    {
        SetArgFromArchive(template, idx, 0x277, trapID);
    }

    // OFFSET 0x0200BB74
    public static void StringTemplate_SetContestJudgeName(StringTemplate template, uint idx, uint judgeID)
    {
        SetArgFromArchive(template, idx, 0xCF, judgeID);
    }

    // OFFSET 0x0200BBA8
    public static void StringTemplate_SetContestRankName(StringTemplate template, uint idx, uint rankID)
    {
        SetArgFromArchive(template, idx, 0xCC, rankID);
    }

    // OFFSET 0x0200BBDC
    public static void StringTemplate_SetContestTypeName(StringTemplate template, uint idx, uint typeID)
    {
        SetArgFromArchive(template, idx, 0xCC, typeID);
    }

    // OFFSET 0x0200BC10
    public static void StringTemplate_SetUndergroundQuestion(StringTemplate template, uint idx, uint questionID)
    {
        SetArgFromArchive(template, idx, 0x279, questionID);
    }

    // OFFSET 0x0200BC48
    public static void StringTemplate_SetUndergroundAnswer(StringTemplate template, uint idx, uint answerID)
    {
        SetArgFromArchive(template, idx, 0x278, answerID);
    }

    // OFFSET 0x0200BC80
    public static void StringTemplate_SetUndergroundGoodsName(StringTemplate template, uint idx, uint goodsID)
    {
        SetArgFromArchive(template, idx, 0x272, goodsID);
    }

    // OFFSET 0x0200BCB8
    public static void StringTemplate_SetUndergroundGoodsNameWithArticle(StringTemplate template, uint idx, uint goodsID)
    {
        SetArgFromArchive(template, idx, 0x273, goodsID);
    }

    // OFFSET 0x0200BCF0
    public static void StringTemplate_SetGenderMarker(StringTemplate template, uint idx, uint gender)
    {
        MessageLoader loader = MessageLoader.MessageLoader_Init(MessageLoader.TypeNarcHandle, MessageArchive, 0xD5, template.HeapID)!;
        switch (gender)
        {
            case 0:
                MessageLoader.MessageLoader_GetString(loader, 0x44, template.Buffer);
                break;
            case 1:
                MessageLoader.MessageLoader_GetString(loader, 0x45, template.Buffer);
                break;
            default:
                GameString.String_Clear(template.Buffer);
                break;
        }
        SetStringTemplateArg(template, idx, template.Buffer, null);
        MessageLoader.MessageLoader_Free(loader);
    }

    // OFFSET 0x0200BD40
    public static void StringTemplate_SetPCBoxName(StringTemplate template, uint idx, object pcBoxes, uint boxID)
    {
        sub_02079AF4(pcBoxes, boxID, template.Buffer);
        SetStringTemplateArg(template, idx, template.Buffer, null);
    }

    // OFFSET 0x0200BD60
    public static void StringTemplate_SetGymName(StringTemplate template, uint idx, uint gymID)
    {
        SetArgFromArchive(template, idx, 0x17A, gymID);
    }

    // OFFSET 0x0200BD98
    public static void StringTemplate_SetTimeOfDay(StringTemplate template, uint idx, uint timeID)
    {
        SetArgFromArchive(template, idx, 0x260, timeID);
    }

    // OFFSET 0x0200BDD0
    public static void StringTemplate_SetCountryName(StringTemplate template, uint idx, uint countryID)
    {
        SetArgFromArchive(template, idx, 0x2B6, countryID);
    }

    // OFFSET 0x0200BE08
    public static void StringTemplate_SetCityName(StringTemplate template, uint idx, uint countryID, uint cityID)
    {
        uint bankID = sub_02099720(countryID);
        if (bankID != 0 && cityID != 0)
        {
            SetArgFromArchive(template, idx, (int)bankID, cityID);
        }
    }

    // OFFSET 0x0200BE48
    public static void StringTemplate_SetEasyChatWord(StringTemplate template, uint idx, ushort word)
    {
        EasyChatWords.EasyChatWord_ToString(word, template.Buffer);
        SetStringTemplateArg(template, idx, template.Buffer, null);
    }

    // OFFSET 0x0200BE64
    public static void StringTemplate_SetBallSealName(StringTemplate template, uint idx, uint sealID)
    {
        SetArgFromArchive(template, idx, 0xC, sealID);
    }

    // OFFSET 0x0200BE98
    public static void StringTemplate_SetBallSealNamePlural(StringTemplate template, uint idx, uint sealID)
    {
        SetArgFromArchive(template, idx, 0xD, sealID);
    }

    // OFFSET 0x0200BECC
    public static void StringTemplate_SetMetLocationName(StringTemplate template, uint idx, uint metLocation)
    {
        sMetLocationBanks ??= RomData.Arm9U16(0x020E4FE4, 4);
        uint type = (uint)SpecialMetLoc.SpecialMetLoc_GetType(metLocation);
        uint entryID = (uint)SpecialMetLoc.SpecialMetLoc_GetIndex(metLocation);
        MessageLoader? loader = MessageLoader.MessageLoader_Init(MessageLoader.TypeNarcHandle, MessageArchive, sMetLocationBanks[type], template.HeapID);
        if (loader == null)
        {
            return;
        }
        if (entryID < MessageLoader.MessageLoader_MessageCount(loader) && (type != 0 || entryID != 0))
        {
            MessageLoader.MessageLoader_GetString(loader, entryID, template.Buffer);
            SetStringTemplateArg(template, idx, template.Buffer, null);
            MessageLoader.MessageLoader_Free(loader);
            return;
        }
        MessageLoader.MessageLoader_Free(loader);
        int bankID;
        if (type == 0 && entryID == 0)
        {
            bankID = 0x1B3;
            entryID = 0xC;
        }
        else
        {
            bankID = 0x1B2;
            entryID = 2;
        }
        SetArgFromArchive(template, idx, bankID, entryID);
    }

    // OFFSET 0x0200BF74
    public static void StringTemplate_SetPoffinName(StringTemplate template, uint idx, uint poffinID)
    {
        SetArgFromArchive(template, idx, 0x1D1, poffinID);
    }

    // OFFSET 0x0200BFAC
    public static void StringTemplate_SetContestAccessoryName(StringTemplate template, uint idx, uint accessoryID)
    {
        SetArgFromArchive(template, idx, 0x182, accessoryID);
    }

    // OFFSET 0x0200BFE4
    public static void StringTemplate_SetContestAccessoryNameWithArticle(StringTemplate template, uint idx, uint accessoryID)
    {
        SetArgFromArchive(template, idx, 0x183, accessoryID);
    }

    // OFFSET 0x0200C01C
    public static void StringTemplate_SetContestBackdropName(StringTemplate template, uint idx, uint backdropID)
    {
        SetArgFromArchive(template, idx, 0x184, backdropID);
    }

    // OFFSET 0x0200C054
    public static void StringTemplate_SetUnionGroupName(StringTemplate template, object saveData, int groupID, uint idx, int nameType)
    {
        RecordMixedRNG groups = RecordMixedRNG.SaveData_GetRecordMixedRNG((SaveData)saveData);
        int gender = RecordMixedRNG.RecordMixedRNG_GetEntryGender(groups, groupID);
        int language = RecordMixedRNG.RecordMixedRNG_GetEntryLanguage(groups, groupID);
        GameString str = GameString.String_Init(0x40, 4);
        GameString.String_CopyChars(str, RecordMixedRNG.RecordMixedRNG_GetEntryName(groups, groupID, nameType));
        StringTemplate_SetString(template, idx, str, gender, 1, language);
        GameString.String_Free(str);
    }

    // OFFSET 0x0200C0B0
    public static void StringTemplate_SetPlazaMinigameName(StringTemplate template, uint idx, uint minigameID)
    {
        SetArgFromArchive(template, idx, 0x28B, minigameID);
    }

    // OFFSET 0x0200C0E8
    public static void StringTemplate_SetPlazaEventName(StringTemplate template, uint idx, uint eventID)
    {
        SetArgFromArchive(template, idx, 0x289, eventID);
    }

    // OFFSET 0x0200C120
    public static void StringTemplate_SetPlazaItemName(StringTemplate template, uint idx, uint itemID)
    {
        SetArgFromArchive(template, idx, 0x28C, itemID);
    }

    // OFFSET 0x0200C158
    public static void StringTemplate_SetJPGreeting(StringTemplate template, uint idx, uint greetingID)
    {
        SetArgFromArchive(template, idx, 0x29B, greetingID);
    }

    // OFFSET 0x0200C190
    public static void StringTemplate_SetENGreeting(StringTemplate template, uint idx, uint greetingID)
    {
        SetArgFromArchive(template, idx, 0x297, greetingID);
    }

    // OFFSET 0x0200C1C8
    public static void StringTemplate_SetFRGreeting(StringTemplate template, uint idx, uint greetingID)
    {
        SetArgFromArchive(template, idx, 0x298, greetingID);
    }

    // OFFSET 0x0200C200
    public static void StringTemplate_SetITGreeting(StringTemplate template, uint idx, uint greetingID)
    {
        SetArgFromArchive(template, idx, 0x29A, greetingID);
    }

    // OFFSET 0x0200C238
    public static void StringTemplate_SetDEGreeting(StringTemplate template, uint idx, uint greetingID)
    {
        SetArgFromArchive(template, idx, 0x299, greetingID);
    }

    // OFFSET 0x0200C270
    public static void StringTemplate_SetESGreeting(StringTemplate template, uint idx, uint greetingID)
    {
        SetArgFromArchive(template, idx, 0x29C, greetingID);
    }

    // OFFSET 0x0200C2A8
    public static void StringTemplate_SetFurniture(StringTemplate template, uint idx, uint furnitureID)
    {
        SetArgFromArchive(template, idx, 0x173, furnitureID);
    }

    // OFFSET 0x0200C2E0
    public static void StringTemplate_SetMonthName(StringTemplate template, uint idx, uint month)
    {
        if (month < 1 || month > 12)
        {
            month = 1;
        }
        SetArgFromArchive(template, idx, 0x19E, month - 1);
    }

    // OFFSET 0x0200C324
    public static void StringTemplate_CapitalizeArgAtIndex(StringTemplate template, uint idx)
    {
        GameString.String_UpperChar(template.Args[idx].String, 0);
    }

    // OFFSET 0x0200C338
    public static void StringTemplate_SetFloorNumber(StringTemplate template, uint idx, uint floor)
    {
        if (floor > 5)
        {
            ErrorHandling.GF_AssertFail();
        }
        SetArgFromArchive(template, idx, 0x169, floor == 0 ? 0x79 : floor + 0x73);
    }

    // OFFSET 0x0200C388
    public static void StringTemplate_Format(StringTemplate template, GameString dst, GameString fmt)
    {
        ushort[] src = GameString.String_GetData(fmt);
        int pos = 0;
        GameString.String_Clear(dst);
        while (src[pos] != GameString.EOS)
        {
            if (src[pos] != FormatCode)
            {
                GameString.String_AppendChar(dst, src[pos++]);
                continue;
            }
            if (CharCodeFormat.CharCode_IsFormatArg(src, pos))
            {
                uint idx = (uint)CharCodeFormat.CharCode_FormatArgParam(src, pos, 0);
                if (idx >= template.Count)
                {
                    ErrorHandling.GF_AssertFail();
                }
                GameString.String_ConcatTrainerName(dst, template.Args[idx].String);
                pos = CharCodeFormat.CharCode_SkipFormatArg(src, pos);
            }
            else
            {
                int end = CharCodeFormat.CharCode_SkipFormatArg(src, pos);
                while (pos < end)
                {
                    GameString.String_AppendChar(dst, src[pos++]);
                }
            }
        }
    }

    // OFFSET 0x0200C41C
    public static void StringTemplate_ClearArgs(StringTemplate template)
    {
        for (uint i = 0; i < template.Count; i++)
        {
            GameString.String_Clear(template.Args[i].String);
        }
    }
}
