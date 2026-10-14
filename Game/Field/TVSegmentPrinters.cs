using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static partial class TVSegments
{
    private const uint TVProgramTable = 0x020EFD60;
    private const int TVProgramEntrySize = 0xC;
    private const int TVProgramTypeCount = 6;
    private const int TVSegmentEntrySize = 8;
    private const int HeapIDField = 4;
    private const int TemplateStringSize = 0x40;
    private const int LanguageEnglish = 2;
    private const int MaxDisplayCount = 999;
    private const int PaddingNone = 0;
    private const int CharsetFullWidth = 1;
    private const ushort ItemExplorerKit = 0x1AC;
    private const int HeapIDBagCheck = 0x20;
    private const int FirstArrivalCheck = 2;
    private const uint ArrivalEterna = 6;
    private const uint ArrivalHearthome = 9;
    private const uint ArrivalVeilstone = 0xA;
    private const uint ArrivalPastoria = 0xB;
    private const uint ArrivalSunyshore = 0x11;
    private const uint ArrivalSnowpoint = 0x12;
    private const uint RecordMixedGroupTable = 0x020EFD34;
    private const int RecordMixedGroupCount = 5;
    private const uint BerryAreaArrivalTable = 0x020EFD3C;
    private const int BerryAreaArrivalCount = 0x12;
    private const uint BerryPatchLocationTable = 0x020F0074;
    private const int BerryPatchCount = 0x76;
    private const uint WeatherForecastMaps = 0x02100BA4;
    private const int WeatherForecastMapCount = 5;
    private const int RoamerSlotCount = 6;
    private const int RoamerRouteCount = 0x1D;
    private const int RoamerDataSpecies = 4;
    private const int RoamerDataPersonality = 3;
    private const int MapNameSize = 0x16;
    private const int ClipPhotoCount = 0xB;
    private const int OTNameSize = 8;
    private const int SpeciesCount = 0x1ED;
    private const int MessageBankSpeciesNames = 0x19C;
    private const int NarcMessages = 0x1A;
    private const int MessageLoaderLazy = 1;
    private const int MonDataSpecies = 5;
    private const int MonDataGender = 0x6F;
    private const int AccessoryCount = 100;
    private const int MoveCount = 0x1D3;
    private const int PokemonTypeCount = 0x11;
    private const int PokemonTypeMystery = 9;

    private static ushort[] Chars(byte[] data, int offset, int count)
    {
        ushort[] chars = new ushort[count];
        for (int i = 0; i < count && offset + i * 2 + 1 < data.Length; i++)
        {
            chars[i] = (ushort)Bin.U16(data, offset + i * 2);
        }
        return chars;
    }

    private static int U16(byte[] data, int offset)
    {
        return (int)Bin.U16(data, offset);
    }

    private static uint U32(byte[] data, int offset)
    {
        return Bin.U32(data, offset);
    }

    private static void SetNumber(StringTemplate template, uint idx, int num, uint digits)
    {
        StringTemplate.StringTemplate_SetNumber(template, idx, num, digits, PaddingNone, CharsetFullWidth);
    }

    private static bool HasSeen(FieldSystem fieldSystem, int species)
    {
        return Pokedex.Pokedex_HasSeenSpecies(Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!), (ushort)species);
    }

    private static bool ArrivedAt(FieldSystem fieldSystem, uint firstArrival)
    {
        return SystemFlags.SystemFlag_HandleFirstArrivalToZone(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), FirstArrivalCheck, firstArrival);
    }

    // OFFSET 0x0206CCB0
    private static uint TVProgram_GetEntry(int programType)
    {
        if (programType <= 0 || programType >= TVProgramTypeCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        uint entry = (uint)(TVProgramTable + (programType - 1) * TVProgramEntrySize);
        if (RomData.Arm9U32(entry, 1)[0] != programType)
        {
            ErrorHandling.GF_AssertFail();
        }
        return entry;
    }

    // OFFSET 0x0206CCDC
    private static uint[] TVProgram_GetSegmentFuncs(uint program, TVEpisode episode)
    {
        int segmentID = TVEpisodes.TVEpisode_GetSegmentID(episode);
        ushort[] header = RomData.Arm9U16(program + 4, 2);
        if (segmentID <= 0 || segmentID >= header[1])
        {
            ErrorHandling.GF_AssertFail();
        }
        uint segments = RomData.Arm9U32(program + 8, 1)[0];
        return RomData.Arm9U32((uint)(segments + (segmentID - 1) * TVSegmentEntrySize), 2);
    }

    // OFFSET 0x0206CD00
    public static ushort TVProgram_PrintSegment(int programType, FieldSystem fieldSystem, StringTemplate template, TVEpisode episode, BufferView bankDest)
    {
        uint program = TVProgram_GetEntry(programType);
        bankDest.W16(0, RomData.Arm9U16(program + 4, 1)[0]);
        uint print = TVProgram_GetSegmentFuncs(program, episode)[0];
        if (print == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        return TVSegment_Print(print, fieldSystem, template, episode);
    }

    // OFFSET 0x0206CD2C
    public static bool TVProgram_IsSegmentEligible(int programType, FieldSystem fieldSystem, TVEpisode episode)
    {
        uint[] funcs = TVProgram_GetSegmentFuncs(TVProgram_GetEntry(programType), episode);
        if (funcs[0] == 0)
        {
            return false;
        }
        if (funcs[1] == 0)
        {
            return true;
        }
        return TVSegment_IsEligible(funcs[1], fieldSystem, episode);
    }

    // OFFSET 0x0206CD94
    private static void TVSegment_SetTemplateChars(StringTemplate template, uint idx, ushort[] chars, int gender, int language, int capitalize)
    {
        GameString str = GameString.String_Init(TemplateStringSize, HeapIDField);
        GameString.String_CopyChars(str, chars);
        StringTemplate.StringTemplate_SetString(template, idx, str, gender, capitalize, language);
        GameString.String_Free(str);
    }

    // OFFSET 0x0206CDD0
    private static void TVEpisode_SetTemplateTrainerName(StringTemplate template, uint idx, TVEpisode episode)
    {
        TVSegment_SetTemplateChars(template, idx, TVEpisodes.TVEpisode_GetTrainerName(episode), TVEpisodes.TVEpisode_GetGender(episode), TVEpisodes.TVEpisode_GetLanguage(episode), 1);
    }

    // OFFSET 0x0206CE74
    private static void TVSegment_SetTemplateSpeciesName(StringTemplate template, uint idx, int species, int gender, int language, int gameCode)
    {
        ushort[] name = new ushort[0xC];
        MessageLoader.MessageLoader_GetSpeciesName((uint)species, HeapIDField, name);
        TVSegment_SetTemplateChars(template, idx, name, gender, language, 1);
    }

    // OFFSET 0x0206CEA4
    private static void TVSegment_SetTemplateDefaultSpeciesName(StringTemplate template, uint idx, int species)
    {
        ushort[] name = new ushort[0xC];
        MessageLoader.MessageLoader_GetSpeciesName((uint)species, HeapIDField, name);
        TVSegment_SetTemplateChars(template, idx, name, 0, LanguageEnglish, 1);
    }

    private static void SetSpecies(StringTemplate template, uint idx, byte[] d, int speciesOffset, int detailsOffset)
    {
        TVSegment_SetTemplateSpeciesName(template, idx, U16(d, speciesOffset), d[detailsOffset], d[detailsOffset + 1], d[detailsOffset + 2]);
    }

    // OFFSET 0x0206D230
    private static ushort TVSegment_Print_CatchThatPokemonCaught(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetSpecies(template, 1, d, 6, 8);
        StringTemplate.StringTemplate_SetItemName(template, 2, (uint)U16(d, 0x22));
        SetNumber(template, 3, U16(d, 4), 3);
        if (d[0xB] != 0)
        {
            TVSegment_SetTemplateChars(template, 4, Chars(d, 0xC, 11), d[8], d[9], 1);
            return 0;
        }
        return 1;
    }

    // OFFSET 0x0206D2E0
    private static ushort TVSegment_Print_CatchThatPokemonFailed(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetNumber(template, 1, U16(d, 4), 3);
        return (ushort)(U32(d, 0) == 1 ? 2 : 3);
    }

    // OFFSET 0x0206D320
    private static bool TVSegment_IsEligible_CatchThatPokemon(FieldSystem fieldSystem, TVEpisode episode)
    {
        return HasSeen(fieldSystem, U16(TVEpisodes.TVEpisode_GetSegment(episode), 6));
    }

    // OFFSET 0x0206D374
    private static ushort TVSegment_Print_UsedItem(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        if (U32(d, 8) != 0)
        {
            StringTemplate.StringTemplate_SetItemName(template, 1, (uint)U16(d, 6));
            SetSpecies(template, 2, d, 0, 2);
            return 4;
        }
        return 5;
    }

    // OFFSET 0x0206D3C0
    private static bool TVSegment_IsEligible_UsedItem(FieldSystem fieldSystem, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        if (U32(d, 8) == 0)
        {
            return true;
        }
        return HasSeen(fieldSystem, U16(d, 0));
    }

    private static ushort PrintGroupEvent(StringTemplate template, TVEpisode episode, ushort message)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVSegment_SetTemplateChars(template, 1, Chars(d, 0, 0x14), 0, TVEpisodes.TVEpisode_GetLanguage(episode), 1);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        return message;
    }

    // OFFSET 0x0206D43C
    private static ushort TVSegment_Print_GroupJoined(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        return PrintGroupEvent(template, episode, 6);
    }

    // OFFSET 0x0206D474
    private static ushort TVSegment_Print_GroupCreated(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        return PrintGroupEvent(template, episode, 0x20);
    }

    // OFFSET 0x0206D4D4
    private static ushort TVSegment_Print_FoundHiddenItem(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        StringTemplate.StringTemplate_SetLocationName(template, 0, (uint)U16(d, 2));
        TVEpisode_SetTemplateTrainerName(template, 1, episode);
        StringTemplate.StringTemplate_SetItemName(template, 2, (uint)U16(d, 0));
        return 0xA;
    }

    // OFFSET 0x0206D530
    private static ushort TVSegment_Print_BoughtItems(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        StringTemplate.StringTemplate_SetItemName(template, 0, (uint)U16(d, 0));
        SetNumber(template, 1, d[2], 3);
        TVEpisode_SetTemplateTrainerName(template, 2, episode);
        StringTemplate.StringTemplate_SetItemNamePlural(template, 3, (uint)U16(d, 0));
        return 0xB;
    }

    // OFFSET 0x0206D5B0
    private static ushort TVSegment_Print_EncounteredPokemon(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        StringTemplate.StringTemplate_SetLocationName(template, 1, (uint)U16(d, 6));
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetSpecies(template, 2, d, 0, 2);
        return 0xC;
    }

    // OFFSET 0x0206D5F0
    private static bool TVSegment_IsEligible_SpeciesAt0(FieldSystem fieldSystem, TVEpisode episode)
    {
        return HasSeen(fieldSystem, U16(TVEpisodes.TVEpisode_GetSegment(episode), 0));
    }

    // OFFSET 0x0206D644
    private static ushort TVSegment_Print_NicknamedPokemon(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetSpecies(template, 1, d, 0, 2);
        TVSegment_SetTemplateChars(template, 2, Chars(d, 6, 11), d[2], d[3], 1);
        int variant = MathUtil.LCRNG_Next() / 0x3334;
        if (variant >= 5)
        {
            ErrorHandling.GF_AssertFail();
        }
        return (ushort)(variant + 0x11);
    }

    // OFFSET 0x0206D6A8
    private static bool TVSegment_IsEligible_NicknamedPokemon(FieldSystem fieldSystem, TVEpisode episode)
    {
        return HasSeen(fieldSystem, U16(TVEpisodes.TVEpisode_GetSegment(episode), 0));
    }

    // OFFSET 0x0206D6E0
    private static ushort TVSegment_Print_DugUpItem(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        StringTemplate.StringTemplate_SetUndergroundItemName(template, 1, (uint)U16(d, 0));
        SetNumber(template, 2, U16(d, 2), 3);
        return 0x18;
    }

    // OFFSET 0x0206D75C
    private static ushort TVSegment_Print_SafariGame(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetSpecies(template, 1, d, 0, 2);
        SetNumber(template, 2, d[5], 2);
        return 0x1A;
    }

    // OFFSET 0x0206D7A4
    private static bool TVSegment_IsEligible_SafariGame(FieldSystem fieldSystem, TVEpisode episode)
    {
        return HasSeen(fieldSystem, U16(TVEpisodes.TVEpisode_GetSegment(episode), 0));
    }

    // OFFSET 0x0206D8B0
    private static ushort TVSegment_Print_HatchedEgg(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetSpecies(template, 1, d, 0, 2);
        return (ushort)(d[2] == 0 ? 0x1B : 0x1C);
    }

    // OFFSET 0x0206D8F0
    private static bool TVSegment_IsEligible_HatchedEgg(FieldSystem fieldSystem, TVEpisode episode)
    {
        return HasSeen(fieldSystem, U16(TVEpisodes.TVEpisode_GetSegment(episode), 0));
    }

    // OFFSET 0x0206D910
    private static bool TVSegment_IsEligible_Never(FieldSystem fieldSystem, TVEpisode episode)
    {
        return false;
    }

    // OFFSET 0x0206D94C
    private static ushort TVSegment_Print_ReceivedItems(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        StringTemplate.StringTemplate_SetItemName(template, 1, (uint)U16(d, 0));
        SetNumber(template, 2, U16(d, 4), 2);
        return 0x1E;
    }

    // OFFSET 0x0206D98C
    private static ushort TVSegment_Print_ReceivedItem(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        StringTemplate.StringTemplate_SetItemName(template, 1, (uint)U16(d, 0));
        return 0x1F;
    }

    // OFFSET 0x0206D9F4
    private static ushort TVSegment_Print_BallSeal(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        StringTemplate.StringTemplate_SetBallSealName(template, 1, d[6]);
        SetSpecies(template, 2, d, 0, 2);
        int variant = MathUtil.LCRNG_Next() / 0x5556;
        if (variant >= 3)
        {
            ErrorHandling.GF_AssertFail();
        }
        return (ushort)(variant + 0x21);
    }

    // OFFSET 0x0206DA50
    private static bool TVSegment_IsEligible_BallSeal(FieldSystem fieldSystem, TVEpisode episode)
    {
        return HasSeen(fieldSystem, U16(TVEpisodes.TVEpisode_GetSegment(episode), 0));
    }

    // OFFSET 0x0206DA84
    private static void TVSegment_SetTemplateOtherPlayer(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        if (U32(d, 0) != TrainerInfo.TrainerInfo_Size())
        {
            ErrorHandling.GF_AssertFail();
        }
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        TrainerInfo other = new TrainerInfo();
        other.ReadFrom(d, 4);
        StringTemplate.StringTemplate_SetPlayerName(template, 1, other);
    }

    // OFFSET 0x0206DAF0
    private static ushort TVSegment_Print_SecretBaseVisited(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        TVSegment_SetTemplateOtherPlayer(fieldSystem, template, episode);
        return 0x24;
    }

    // OFFSET 0x0206DAFC
    private static ushort TVSegment_Print_SecretBaseFlagTaken(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        TVSegment_SetTemplateOtherPlayer(fieldSystem, template, episode);
        return 0x25;
    }

    // OFFSET 0x0206DB08
    private static bool TVSegment_IsEligible_HasExplorerKit(FieldSystem fieldSystem, TVEpisode episode)
    {
        return BagItems.Bag_CanRemoveItem(BagItems.SaveData_GetBag(fieldSystem.SaveData!), ItemExplorerKit, 1, HeapIDBagCheck);
    }

    // OFFSET 0x0206DB38
    private static ushort TVSegment_Print_ChangedRoom(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        return 0x26;
    }

    // OFFSET 0x0206DB48
    private static bool TVSegment_IsEligible_ArrivedEterna(FieldSystem fieldSystem, TVEpisode episode)
    {
        return ArrivedAt(fieldSystem, ArrivalEterna);
    }

    // OFFSET 0x0206DB74
    private static ushort TVSegment_Print_BoughtFurniture(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        StringTemplate.StringTemplate_SetFurniture(template, 1, d[0]);
        return 0x27;
    }

    // OFFSET 0x0206DB9C
    private static bool TVSegment_IsEligible_ArrivedEterna2(FieldSystem fieldSystem, TVEpisode episode)
    {
        return ArrivedAt(fieldSystem, ArrivalEterna);
    }

    // OFFSET 0x0206DBE8
    private static ushort TVSegment_Print_BattleFrontierStreak(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetSpecies(template, 1, d, 2, 4);
        SetNumber(template, 2, U16(d, 0), 4);
        return (ushort)(d[7] == 0 ? 1 : 0);
    }

    // OFFSET 0x0206DC3C
    private static bool TVSegment_IsEligible_BattleFrontierStreak(FieldSystem fieldSystem, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        if (!HasSeen(fieldSystem, U16(d, 2)))
        {
            return false;
        }
        return ArrivedAt(fieldSystem, ArrivalSunyshore);
    }

    // OFFSET 0x0206DC9C
    private static ushort TVSegment_Print_PokemonWeight(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetSpecies(template, 1, d, 0, 2);
        uint tenths = (U32(d, 8) * 1000 / 0xFE + 5) / 10;
        SetNumber(template, 2, (int)(tenths / 10), 3);
        StringTemplate.StringTemplate_SetNumber(template, 3, (int)(tenths % 10), 1, PaddingNone, CharsetFullWidth);
        return 2;
    }

    // OFFSET 0x0206DD1C
    private static bool TVSegment_IsEligible_PokemonWeight(FieldSystem fieldSystem, TVEpisode episode)
    {
        return HasSeen(fieldSystem, U16(TVEpisodes.TVEpisode_GetSegment(episode), 0));
    }

    // OFFSET 0x0206DD5C
    private static ushort TVSegment_Print_PoketchSteps(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetNumber(template, 1, (int)U32(d, 8), 10);
        SetNumber(template, 2, (int)U32(d, 0), 6);
        SetNumber(template, 3, (int)U32(d, 4), 6);
        return 3;
    }

    // OFFSET 0x0206DE80
    private static ushort TVSegment_Print_Ribbons(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        if (d[0x18] != 0)
        {
            TVSegment_SetTemplateChars(template, 1, Chars(d, 0, 11), d[0x19], d[0x1A], 1);
        }
        else
        {
            TVSegment_SetTemplateSpeciesName(template, 1, U16(d, 0x1C), d[0x19], d[0x1A], d[0x1B]);
        }
        StringTemplate.StringTemplate_SetRibbonName(template, 2, d[0x16]);
        SetNumber(template, 3, d[0x17], 3);
        return 4;
    }

    // OFFSET 0x0206DF14
    private static ushort TVSegment_Print_UndergroundTraps(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        StringTemplate.StringTemplate_SetUndergroundTrapName(template, 1, (uint)U16(d, 0));
        SetNumber(template, 2, Math.Min(U16(d, 2), MaxDisplayCount), 3);
        return 7;
    }

    // OFFSET 0x0206DF88
    private static ushort TVSegment_Print_UndergroundFlags(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetNumber(template, 1, Math.Min(U16(d, 0), MaxDisplayCount), 3);
        return 8;
    }

    // OFFSET 0x0206DFC8
    private static bool TVSegment_IsEligible_HasExplorerKit2(FieldSystem fieldSystem, TVEpisode episode)
    {
        return BagItems.Bag_CanRemoveItem(BagItems.SaveData_GetBag(fieldSystem.SaveData!), ItemExplorerKit, 1, HeapIDBagCheck);
    }

    // OFFSET 0x0206E018
    private static ushort TVSegment_Print_CaptureCount(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetNumber(template, 1, (int)U32(d, 4), 4);
        return 9;
    }

    // OFFSET 0x0206E04C
    private static bool TVSegment_IsEligible_ArrivedSunyshore(FieldSystem fieldSystem, TVEpisode episode)
    {
        return ArrivedAt(fieldSystem, ArrivalSunyshore);
    }

    // OFFSET 0x0206E098
    private static ushort TVSegment_Print_DailyCount(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetNumber(template, 1, U16(d, 2), 4);
        return 0xA;
    }

    // OFFSET 0x0206E0CC
    private static bool TVSegment_IsEligible_ArrivedHearthome(FieldSystem fieldSystem, TVEpisode episode)
    {
        return ArrivedAt(fieldSystem, ArrivalHearthome);
    }

    // OFFSET 0x0206E118
    private static ushort TVSegment_Print_InterviewSection0B7C(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        SetNumber(template, 0, U16(d, 2), 4);
        TVEpisode_SetTemplateTrainerName(template, 1, episode);
        StringTemplate.StringTemplate_SetEasyChatWord(template, 2, (ushort)U16(d, 4));
        return (ushort)(d[1] == 1 ? 8 : 9);
    }

    // OFFSET 0x0206E160
    private static bool TVSegment_IsEligible_ArrivedSunyshore2(FieldSystem fieldSystem, TVEpisode episode)
    {
        return ArrivedAt(fieldSystem, ArrivalSunyshore);
    }

    // OFFSET 0x0206E1C0
    private static ushort TVSegment_Print_InterviewHatchedMon(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        if (d[5] != 0)
        {
            TVEpisode_SetTemplateTrainerName(template, 0, episode);
            SetSpecies(template, 1, d, 0, 2);
            TVSegment_SetTemplateChars(template, 2, Chars(d, 6, 11), d[2], d[3], 1);
            StringTemplate.StringTemplate_SetEasyChatWord(template, 3, (ushort)U16(d, 0x1C));
            return 0xA;
        }
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetSpecies(template, 1, d, 0, 2);
        StringTemplate.StringTemplate_SetEasyChatWord(template, 3, (ushort)U16(d, 0x1C));
        return 0xB;
    }

    // OFFSET 0x0206E248
    private static bool TVSegment_IsEligible_InterviewHatchedMon(FieldSystem fieldSystem, TVEpisode episode)
    {
        return HasSeen(fieldSystem, U16(TVEpisodes.TVEpisode_GetSegment(episode), 0));
    }

    // OFFSET 0x0206E28C
    private static ushort TVSegment_Print_InterviewPoketch(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        StringTemplate.StringTemplate_SetPoketchAppName(template, 1, U32(d, 0));
        StringTemplate.StringTemplate_SetEasyChatWord(template, 2, (ushort)U16(d, 4));
        return 0x10;
    }

    // OFFSET 0x0206E300
    private static ushort TVSegment_Print_InterviewContest(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVSegment_SetTemplateSpeciesName(template, 0, U16(d, 2), d[4], d[5], d[6]);
        StringTemplate.StringTemplate_SetContestTypeName(template, 1, ContestStrings.ContestType_GetNameMessageID(d[8]));
        StringTemplate.StringTemplate_SetContestRankName(template, 2, ContestStrings.ContestRank_GetNameMessageID(d[7]));
        StringTemplate.StringTemplate_SetNumber(template, 3, d[9], 1, PaddingNone, CharsetFullWidth);
        TVEpisode_SetTemplateTrainerName(template, 4, episode);
        StringTemplate.StringTemplate_SetEasyChatWord(template, 5, (ushort)U16(d, 0xA));
        return (ushort)(d[9] == 1 ? 0x12 : 0x11);
    }

    // OFFSET 0x0206E37C
    private static bool TVSegment_IsEligible_InterviewContest(FieldSystem fieldSystem, TVEpisode episode)
    {
        return HasSeen(fieldSystem, U16(TVEpisodes.TVEpisode_GetSegment(episode), 2));
    }

    // OFFSET 0x0206E3C8
    private static ushort TVSegment_Print_InterviewDressUp(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        TVSegment_SetTemplateDefaultSpeciesName(template, 1, U16(d, 0));
        StringTemplate.StringTemplate_SetEasyChatWord(template, 2, (ushort)U16(d, 2));
        return 0x14;
    }

    // OFFSET 0x0206E3F8
    private static bool TVSegment_IsEligible_InterviewDressUp(FieldSystem fieldSystem, TVEpisode episode)
    {
        return HasSeen(fieldSystem, U16(TVEpisodes.TVEpisode_GetSegment(episode), 0));
    }

    // OFFSET 0x0206E428
    private static ushort TVSegment_Print_InterviewAnswer(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        return (ushort)(U32(d, 0) + 0x14);
    }

    // OFFSET 0x0206E480
    private static ushort TVSegment_Print_InterviewPoffin(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        int poffin = d[1];
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        StringTemplate.StringTemplate_SetPoffinName(template, 1, (uint)poffin);
        StringTemplate.StringTemplate_SetEasyChatWord(template, 2, (ushort)U16(d, 2));
        switch (poffin - 0x19)
        {
            case 0:
                return 0x27;
            case 1:
                return 0x28;
            case 2:
                return 0x29;
            case 3:
                return 0x2A;
        }
        return 0x26;
    }

    // OFFSET 0x0206E520
    private static ushort TVSegment_Print_InterviewNature(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetSpecies(template, 1, d, 2, 4);
        StringTemplate.StringTemplate_SetNatureName(template, 2, d[0x1E]);
        StringTemplate.StringTemplate_SetEasyChatWord(template, 5, (ushort)U16(d, 0x24));
        switch (d[0x1F])
        {
            case 0:
                return 0x2D;
            case 2:
                StringTemplate.StringTemplate_SetContestAccessoryName(template, 3, d[0x20]);
                return 0x2C;
            case 1:
                StringTemplate.StringTemplate_SetItemName(template, 3, (uint)U16(d, 0x22));
                return 0x2E;
        }
        return 0x2D;
    }

    // OFFSET 0x0206E5E4
    private static ushort TVSegment_Print_InterviewSection0B88(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        SetSpecies(template, 1, d, 2, 4);
        if (d[7] != 0)
        {
            TVSegment_SetTemplateChars(template, 2, Chars(d, 8, 11), d[4], d[5], 1);
        }
        else
        {
            SetSpecies(template, 2, d, 2, 4);
        }
        StringTemplate.StringTemplate_SetEasyChatWord(template, 3, (ushort)U16(d, 0x1E));
        return 0x2F;
    }

    // OFFSET 0x0206E654
    private static bool TVSegment_IsEligible_ArrivedSunyshore3(FieldSystem fieldSystem, TVEpisode episode)
    {
        return ArrivedAt(fieldSystem, ArrivalSunyshore);
    }

    private static ushort PrintEasyChatWord(StringTemplate template, TVEpisode episode, ushort message)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        StringTemplate.StringTemplate_SetEasyChatWord(template, 1, (ushort)U16(d, 0));
        return message;
    }

    // OFFSET 0x0206E680
    private static ushort TVSegment_Print_InterviewWord15(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        return PrintEasyChatWord(template, episode, 0x30);
    }

    // OFFSET 0x0206E6C0
    private static ushort TVSegment_Print_InterviewWord16(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        return PrintEasyChatWord(template, episode, 0x31);
    }

    // OFFSET 0x0206E700
    private static ushort TVSegment_Print_InterviewWord17(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        return PrintEasyChatWord(template, episode, 0x32);
    }

    // OFFSET 0x0206E740
    private static ushort TVSegment_Print_InterviewWord18(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        return PrintEasyChatWord(template, episode, 0x33);
    }

    // OFFSET 0x0206E7AC
    private static ushort TVSegment_Print_InterviewBattleTower(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        byte[] d = TVEpisodes.TVEpisode_GetSegment(episode);
        GameString str = GameString.String_Init(TemplateStringSize, HeapIDField);
        TVEpisode_SetTemplateTrainerName(template, 0, episode);
        GameString.String_CopyChars(str, Chars(d, 6, 8));
        StringTemplate.StringTemplate_SetString(template, 1, str, d[2], 0, d[3]);
        GameString.String_Free(str);
        StringTemplate.StringTemplate_SetEasyChatWord(template, 2, (ushort)U16(d, 0x16));
        switch (d[1])
        {
            case 1:
                return 0x34;
            case 4:
                return 0x35;
            case 5:
                return 0x36;
            case 2:
            case 3:
                return 0x37;
            case 6:
                return 0x38;
        }
        return 0;
    }

    // OFFSET 0x0206E834
    private static bool TVSegment_IsEligible_ArrivedSunyshore4(FieldSystem fieldSystem, TVEpisode episode)
    {
        return ArrivedAt(fieldSystem, ArrivalSunyshore);
    }

    // OFFSET 0x0206E848
    private static int RecordMixedRNG_CountValidGroups(RecordMixedRNG rng)
    {
        byte[] groups = RomData.Arm9(RecordMixedGroupTable, RecordMixedGroupCount);
        int count = 0;
        for (int i = 0; i < RecordMixedGroupCount; i++)
        {
            if (RecordMixedRNG.RecordMixedRNG_IsEntryValid(rng, groups[i]))
            {
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x0206E870
    private static ushort TVSegment_Print_GroupNews(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        RecordMixedRNG rng = RecordMixedRNG.SaveData_GetRecordMixedRNG(fieldSystem.SaveData!);
        int count = RecordMixedRNG_CountValidGroups(rng);
        if (count <= 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        int skip = count > 1 ? (int)(MathUtil.MTRNG_Next() % (uint)count) : 0;
        byte[] groups = RomData.Arm9(RecordMixedGroupTable, RecordMixedGroupCount);
        int group = 0;
        for (int i = 0; i < RecordMixedGroupCount; i++)
        {
            if (!RecordMixedRNG.RecordMixedRNG_IsEntryValid(rng, groups[i]))
            {
                continue;
            }
            if (skip == 0)
            {
                group = groups[i];
                break;
            }
            skip--;
        }
        if (skip != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        int type = MathUtil.LCRNG_Next() / 0xF10;
        if (type >= PokemonTypeCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (type >= PokemonTypeMystery)
        {
            type++;
        }
        StringTemplate.StringTemplate_SetUnionGroupName(template, fieldSystem.SaveData!, group, 0, 1);
        StringTemplate.StringTemplate_SetUnionGroupName(template, fieldSystem.SaveData!, group, 1, 0);
        StringTemplate.StringTemplate_SetPokemonTypeName(template, 2, (uint)type);
        return 0;
    }

    // OFFSET 0x0206E928
    private static bool TVSegment_IsEligible_GroupNews(FieldSystem fieldSystem, TVEpisode episode)
    {
        return RecordMixedRNG_CountValidGroups(RecordMixedRNG.SaveData_GetRecordMixedRNG(fieldSystem.SaveData!)) != 0;
    }

    // OFFSET 0x0206E940
    private static ushort TVSegment_Print_WeatherForecast(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        int index = MathUtil.LCRNG_Next() / 0x3334;
        if (index >= WeatherForecastMapCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        int mapID = RomData.Arm9U16((uint)(WeatherForecastMaps + index * 2), 1)[0];
        int weather = FieldOverworldWeather.FieldSystem_GetWeather(fieldSystem, mapID);
        StringTemplate.StringTemplate_SetLocationName(template, 0, (uint)MapHeader.MapHeader_GetMapLabelTextID(mapID));
        switch (weather)
        {
            case 0:
            {
                int variant = MathUtil.LCRNG_Next() >> 14;
                if (variant >= 4)
                {
                    ErrorHandling.GF_AssertFail();
                }
                switch (variant)
                {
                    case 0:
                        return 1;
                    case 1:
                        return 2;
                    case 2:
                        return 3;
                    case 3:
                        return 4;
                }
                return 5;
            }
            case 1:
                return 5;
            case 2:
                return 6;
            case 3:
                return 7;
            case 4:
                return 0xB;
            case 5:
                return 8;
            case 6:
                return 9;
            case 7:
                return 0xA;
            case 11:
                return 0xC;
        }
        ErrorHandling.GF_AssertFail();
        return 1;
    }

    // OFFSET 0x0206EA0C
    private static bool TVSegment_IsEligible_Always(FieldSystem fieldSystem, TVEpisode episode)
    {
        return true;
    }

    private static bool InRange(int value, int single, int first, int last)
    {
        return value == single || (value >= first && value <= last);
    }

    // OFFSET 0x0206EA10
    private static ushort TVSegment_Print_LocalNews(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!);
        int map = (int)fieldSystem.Location!.U32(0);
        if (InRange(map, 0x19B, 0x19C, 0x1A1))
        {
            StringTemplate.StringTemplate_SetPlayerName(template, 0, trainerInfo);
            StringTemplate.StringTemplate_SetRivalName(template, 1, fieldSystem.SaveData!);
            return 0xD;
        }
        if (InRange(map, 0x1A2, 0x1A6, 0x1A9))
        {
            StringTemplate.StringTemplate_SetCounterpartName(template, 1, fieldSystem.SaveData!);
            return 0xE;
        }
        if (InRange(map, 0x1AA, 0x1AE, 0x1B0) || map == 0x101)
        {
            return 0xF;
        }
        if (InRange(map, 0x1B1, 0x1B5, 0x1B9))
        {
            return 0x10;
        }
        if (InRange(map, 0x1BA, 0x1BD, 0x1C1))
        {
            return 0x11;
        }
        if (InRange(map, 3, 8, 0x20))
        {
            return 0x12;
        }
        if (InRange(map, 0x21, 0x26, 0x2C) || map == 0x1EB)
        {
            return 0x13;
        }
        if (InRange(map, 0x2D, 0x32, 0x40))
        {
            return 0x14;
        }
        if (InRange(map, 0x41, 0x47, 0x55))
        {
            return 0x15;
        }
        if (InRange(map, 0x56, 0x67, 0x77))
        {
            return 0x16;
        }
        if (InRange(map, 0x78, 0x7D, 0x83))
        {
            return 0x17;
        }
        if (InRange(map, 0x84, 0x88, 0x95) || (map >= 0x131 && map <= 0x136))
        {
            return 0x18;
        }
        if (InRange(map, 0x96, 0x9D, 0xA4) || map == 0x204)
        {
            return 0x19;
        }
        if (InRange(map, 0xA5, 0xAA, 0xAB))
        {
            return 0x1A;
        }
        StringTemplate.StringTemplate_SetPlayerName(template, 0, trainerInfo);
        StringTemplate.StringTemplate_SetRivalName(template, 1, fieldSystem.SaveData!);
        return 0x1B;
    }

    // OFFSET 0x0206EB94
    private static ushort TVSegment_Print_Swarm(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        uint daily = SpecialEncounter.SpecialEncounter_GetDailyMon(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!), 2);
        byte[] result = new byte[4];
        Swarms.Swarm_GetMapIdAndSpecies(daily, new BufferView(result, 2), new BufferView(result, 0));
        StringTemplate.StringTemplate_SetLocationName(template, 0, (uint)MapHeader.MapHeader_GetMapLabelTextID(U16(result, 2)));
        TVSegment_SetTemplateDefaultSpeciesName(template, 1, U16(result, 0));
        return 0x1D;
    }

    // OFFSET 0x0206EBD4
    private static bool TVSegment_IsEligible_Swarm(FieldSystem fieldSystem, TVEpisode episode)
    {
        return SpecialEncounter.SpecialEncounter_IsSwarmEnabled(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!)) != 0;
    }

    // OFFSET 0x0206EBE4
    private static bool TVSegment_IsEligible_Never2(FieldSystem fieldSystem, TVEpisode episode)
    {
        return false;
    }

    // OFFSET 0x0206EBE8
    private static int FieldSystem_PickRandomBerryPatch(FieldSystem fieldSystem)
    {
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        ushort[] arrivals = RomData.Arm9U16(BerryAreaArrivalTable, BerryAreaArrivalCount);
        bool[] arrived = new bool[BerryAreaArrivalCount];
        for (int i = 0; i < BerryAreaArrivalCount; i++)
        {
            arrived[i] = SystemFlags.SystemFlag_HandleFirstArrivalToZone(varsFlags, FirstArrivalCheck, arrivals[i]);
        }
        ushort[] patches = RomData.Arm9U16(BerryPatchLocationTable, BerryPatchCount * 2);
        byte[] candidates = new byte[BerryPatchCount];
        int count = 0;
        for (int i = 0; i < BerryPatchCount; i++)
        {
            if (arrived[patches[i * 2]])
            {
                candidates[count++] = (byte)i;
            }
        }
        if (count == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        int index;
        if (count <= 1)
        {
            index = 0;
        }
        else
        {
            index = MathUtil.LCRNG_Next() / (ushort)(0xFFFF / count + 1);
            if (index >= count)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        return candidates[index];
    }

    // OFFSET 0x0206EC90
    private static ushort TVSegment_Print_BerryReport(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        BerryPatch[] patches = MiscSaveBlock.SaveData_GetBerryPatches(fieldSystem.SaveData!);
        int patch = FieldSystem_PickRandomBerryPatch(fieldSystem);
        int mapID = RomData.Arm9U16((uint)(BerryPatchLocationTable + (patch * 2 + 1) * 2), 1)[0];
        StringTemplate.StringTemplate_SetLocationName(template, 0, (uint)MapHeader.MapHeader_GetMapLabelTextID(mapID));
        switch (BerryPatches.BerryPatches_GrowthStage(patches, patch))
        {
            case 5:
                return 0x24;
            case 4:
                return 0x25;
            case 3:
                return 0x26;
            case 2:
                return 0x27;
        }
        return 0x28;
    }

    // OFFSET 0x0206ECFC
    private static bool TVSegment_IsEligible_ArrivedHearthome2(FieldSystem fieldSystem, TVEpisode episode)
    {
        return ArrivedAt(fieldSystem, ArrivalHearthome);
    }

    // OFFSET 0x0206ED10
    private static bool TVSegment_IsEligible_Never3(FieldSystem fieldSystem, TVEpisode episode)
    {
        return false;
    }

    // OFFSET 0x0206ED14
    private static ushort TVSegment_Print_NatureFortune(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        int found = 0xFF;
        uint personality = (uint)(MathUtil.LCRNG_Next() % 0xFFFF);
        byte nature = Pokemon.Personality_GetNature(personality);
        StringTemplate.StringTemplate_SetNatureName(template, 0, nature);
        if (nature == 0 || nature == 6 || nature == 12 || nature == 18 || nature == 24)
        {
            return 0x2E;
        }
        if ((personality & 1) == 0)
        {
            for (int flavor = 0; flavor < 5; flavor++)
            {
                if (Pokemon.Pokemon_GetFlavorAffinityOf(personality, flavor) == 1)
                {
                    found = flavor;
                    break;
                }
            }
            StringTemplate.StringTemplate_SetFlavorName(template, 2, (uint)found);
            return 0x2D;
        }
        for (int stat = 0; stat < 5; stat++)
        {
            if (Pokemon.Nature_GetStatModifier(nature, stat + 1) > 0)
            {
                found = stat;
                break;
            }
        }
        StringTemplate.StringTemplate_SetPokemonStatName(template, 1, (uint)(found + 1));
        return 0x2C;
    }

    // OFFSET 0x0206EDAC
    private static ushort TVSegment_Print_RoamerSighting(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        GameString name = GameString.String_Init(MapNameSize, HeapIDField);
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!);
        SpecialEncounter encounters = SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!);
        byte route = (byte)(MathUtil.LCRNG_Next() % RoamerRouteCount);
        MapHeader.MapHeader_LoadName(RoamingPokemon.RoamingPokemon_GetRouteFromId(route), HeapIDField, name);
        StringTemplate.StringTemplate_SetString(template, 0, name, 0, 1, LanguageEnglish);
        GameString.String_Free(name);
        for (int slot = 0; slot < RoamerSlotCount; slot++)
        {
            if (SpecialEncounter.SpecialEncounter_IsRoamerActive(encounters, (byte)slot) == 0)
            {
                continue;
            }
            Roamer roamer = SpecialEncounter.SpecialEncounter_GetRoamer(encounters, (byte)slot);
            uint species = Roamer.Roamer_GetData(roamer, RoamerDataSpecies);
            uint personality = Roamer.Roamer_GetData(roamer, RoamerDataPersonality);
            int gender = (int)SpeciesData.Species_GetGender((ushort)species, personality);
            TVSegment_SetTemplateSpeciesName(template, 1, (ushort)species, gender, TrainerInfo.TrainerInfo_Language(trainerInfo), TrainerInfo.TrainerInfo_GameCode(trainerInfo));
            break;
        }
        return 0x31;
    }

    // OFFSET 0x0206EE74
    private static bool TVSegment_IsEligible_RoamerSighting(FieldSystem fieldSystem, TVEpisode episode)
    {
        SpecialEncounter encounters = SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!);
        for (int slot = 0; slot < RoamerSlotCount; slot++)
        {
            if (SpecialEncounter.SpecialEncounter_IsRoamerActive(encounters, (byte)slot) != 0)
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0206EE9C
    private static int ImageClips_CountPhotos(ImageClips imageClips)
    {
        int count = 0;
        for (int i = 0; i < ClipPhotoCount; i++)
        {
            if (ImageClips.ImageClips_PhotoHasData(imageClips, i))
            {
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x0206EEBC
    private static ushort TVSegment_Print_DressUpPhoto(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        ImageClips imageClips = ImageClips.SaveData_GetImageClips(fieldSystem.SaveData!);
        int count = ImageClips_CountPhotos(imageClips);
        int skip = count > 1 ? (int)(MathUtil.MTRNG_Next() % (uint)count) : 0;
        int index = 0;
        for (index = 0; index < ClipPhotoCount; index++)
        {
            if (!ImageClips.ImageClips_PhotoHasData(imageClips, index))
            {
                continue;
            }
            if (skip == 0)
            {
                break;
            }
            skip--;
        }
        if (index >= ClipPhotoCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        ClipPhoto photo = ImageClips.ImageClips_GetPhoto(imageClips, index);
        GameString name = GameString.String_Init(OTNameSize, HeapIDField);
        int gender = ClipPhoto.ClipPhoto_OTGender(photo);
        ClipPhoto.ClipPhoto_OTName(photo, name);
        int language = ClipPhoto.ClipPhoto_Language(photo);
        StringTemplate.StringTemplate_SetString(template, 0, name, gender, 1, language);
        GameString.String_Free(name);
        StringTemplate.StringTemplate_SetEasyChatWord(template, 1, ClipPhoto.ClipPhoto_TitleWord(photo));
        return 0x34;
    }

    // OFFSET 0x0206EF64
    private static bool TVSegment_IsEligible_DressUpPhoto(FieldSystem fieldSystem, TVEpisode episode)
    {
        return ImageClips_CountPhotos(ImageClips.SaveData_GetImageClips(fieldSystem.SaveData!)) != 0;
    }

    // OFFSET 0x0206EF7C
    private static ushort TVSegment_Print_Commercial(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        int message = 0;
        if (SystemFlags.SystemFlag_CheckGameCompleted(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)))
        {
            message = MathUtil.LCRNG_Next() & 7;
        }
        else if (ArrivedAt(fieldSystem, ArrivalPastoria))
        {
            message = MathUtil.LCRNG_Next() % 5;
        }
        else if (ArrivedAt(fieldSystem, ArrivalVeilstone))
        {
            message = MathUtil.LCRNG_Next() & 3;
        }
        else if (ArrivedAt(fieldSystem, ArrivalSnowpoint))
        {
            message = MathUtil.LCRNG_Next() & 1;
        }
        return (ushort)message;
    }

    private static int PickSeenSpecies(Pokedex pokedex, int species)
    {
        for (int tries = 1; tries <= SpeciesCount; tries++)
        {
            if (Pokedex.Pokedex_HasSeenSpecies(pokedex, (ushort)species))
            {
                return species;
            }
            species++;
            if (species >= SpeciesCount)
            {
                species = 1;
            }
        }
        return -1;
    }

    // OFFSET 0x0206F01C
    private static ushort TVSegment_Print_PokemonTrivia(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!);
        int species = PickSeenSpecies(pokedex, MathUtil.LCRNG_Next() % 0x1EC + 1);
        GameString name = TVSegment_LoadSpeciesName(species, HeapIDField);
        StringTemplate.StringTemplate_SetString(template, 0, name, 0, 1, LanguageEnglish);
        GameString.String_Free(name);
        switch (MathUtil.LCRNG_Next() % 3)
        {
            case 0:
                return 8;
            case 1:
                SetNumber(template, 1, MathUtil.LCRNG_Next() % 0x22 + 0x19, 2);
                return 9;
        }
        return 0xA;
    }

    // OFFSET 0x0206F0D8
    private static GameString TVSegment_LoadSpeciesName(int species, int heapID)
    {
        MessageLoader loader = MessageLoader.MessageLoader_Init(MessageLoaderLazy, NarcMessages, MessageBankSpeciesNames, heapID)!;
        GameString name = MessageLoader.MessageLoader_GetNewString(loader, (uint)species)!;
        MessageLoader.MessageLoader_Free(loader);
        return name;
    }

    // OFFSET 0x0206F100
    private static bool TVSegment_IsEligible_PokedexObtained(FieldSystem fieldSystem, TVEpisode episode)
    {
        return Pokedex.Pokedex_IsObtained(Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!));
    }

    // OFFSET 0x0206F118
    private static ushort TVSegment_Print_Variety1(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        switch (MathUtil.LCRNG_Next() % 3)
        {
            case 0:
                return 0xB;
            case 1:
                return 0xC;
        }
        return 0xD;
    }

    // OFFSET 0x0206F13C
    private static ushort TVSegment_Print_Variety2(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        switch (MathUtil.LCRNG_Next() % 3)
        {
            case 0:
                return 0xE;
            case 1:
                return 0xF;
        }
        return 0x10;
    }

    // OFFSET 0x0206F160
    private static ushort TVSegment_Print_PartnerPokemon(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!);
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!);
        Party party = Party.SaveData_GetParty(fieldSystem.SaveData!);
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, FieldParty.SaveData_GetFirstNonEggInParty(fieldSystem.SaveData!));
        int species = (int)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
        int gender = (int)Pokemon.Pokemon_GetValue(mon, MonDataGender, null);
        TVSegment_SetTemplateSpeciesName(template, 0, (ushort)species, (byte)gender, TrainerInfo.TrainerInfo_Language(trainerInfo), TrainerInfo.TrainerInfo_GameCode(trainerInfo));
        StringTemplate.StringTemplate_SetContestAccessoryName(template, 1, (uint)(MathUtil.LCRNG_Next() % AccessoryCount));
        int other = MathUtil.LCRNG_Next() % 0x1EB + 1;
        for (int tries = 1; tries <= SpeciesCount; tries++)
        {
            if (Pokedex.Pokedex_HasSeenSpecies(pokedex, (ushort)other))
            {
                GameString name = TVSegment_LoadSpeciesName(other, HeapIDField);
                StringTemplate.StringTemplate_SetString(template, 2, name, 0, 1, LanguageEnglish);
                GameString.String_Free(name);
                break;
            }
            other++;
            if (other >= SpeciesCount)
            {
                other = 1;
            }
        }
        StringTemplate.StringTemplate_SetMoveName(template, 3, (uint)(MathUtil.LCRNG_Next() % MoveCount - 1));
        return 0x11;
    }

    // OFFSET 0x0206F260
    private static bool TVSegment_IsEligible_PokedexObtained2(FieldSystem fieldSystem, TVEpisode episode)
    {
        return Pokedex.Pokedex_IsObtained(Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!));
    }

    // OFFSET 0x0206F278
    private static ushort TVSegment_Print_Variety3(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        return 0x12;
    }

    // OFFSET 0x0206F27C
    private static ushort TVSegment_Print_Variety4(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        return 0x13;
    }

    // OFFSET 0x0206F280
    private static bool TVSegment_IsEligible_HasCoalBadge(FieldSystem fieldSystem, TVEpisode episode)
    {
        return TrainerInfo.TrainerInfo_HasBadge(PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!), 0);
    }

    // OFFSET 0x0206F29C
    private static ushort TVSegment_Print_Variety5(FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        return (ushort)(0x14 + MathUtil.LCRNG_Next() % 9);
    }

    private static ushort TVSegment_Print(uint function, FieldSystem fieldSystem, StringTemplate template, TVEpisode episode)
    {
        switch (function & ~1u)
        {
            case 0x0206D230: return TVSegment_Print_CatchThatPokemonCaught(fieldSystem, template, episode);
            case 0x0206D2E0: return TVSegment_Print_CatchThatPokemonFailed(fieldSystem, template, episode);
            case 0x0206D374: return TVSegment_Print_UsedItem(fieldSystem, template, episode);
            case 0x0206D43C: return TVSegment_Print_GroupJoined(fieldSystem, template, episode);
            case 0x0206D474: return TVSegment_Print_GroupCreated(fieldSystem, template, episode);
            case 0x0206D4D4: return TVSegment_Print_FoundHiddenItem(fieldSystem, template, episode);
            case 0x0206D530: return TVSegment_Print_BoughtItems(fieldSystem, template, episode);
            case 0x0206D5B0: return TVSegment_Print_EncounteredPokemon(fieldSystem, template, episode);
            case 0x0206D644: return TVSegment_Print_NicknamedPokemon(fieldSystem, template, episode);
            case 0x0206D6E0: return TVSegment_Print_DugUpItem(fieldSystem, template, episode);
            case 0x0206D75C: return TVSegment_Print_SafariGame(fieldSystem, template, episode);
            case 0x0206D8B0: return TVSegment_Print_HatchedEgg(fieldSystem, template, episode);
            case 0x0206D94C: return TVSegment_Print_ReceivedItems(fieldSystem, template, episode);
            case 0x0206D98C: return TVSegment_Print_ReceivedItem(fieldSystem, template, episode);
            case 0x0206D9F4: return TVSegment_Print_BallSeal(fieldSystem, template, episode);
            case 0x0206DAF0: return TVSegment_Print_SecretBaseVisited(fieldSystem, template, episode);
            case 0x0206DAFC: return TVSegment_Print_SecretBaseFlagTaken(fieldSystem, template, episode);
            case 0x0206DB38: return TVSegment_Print_ChangedRoom(fieldSystem, template, episode);
            case 0x0206DB74: return TVSegment_Print_BoughtFurniture(fieldSystem, template, episode);
            case 0x0206DBE8: return TVSegment_Print_BattleFrontierStreak(fieldSystem, template, episode);
            case 0x0206DC9C: return TVSegment_Print_PokemonWeight(fieldSystem, template, episode);
            case 0x0206DD5C: return TVSegment_Print_PoketchSteps(fieldSystem, template, episode);
            case 0x0206DE80: return TVSegment_Print_Ribbons(fieldSystem, template, episode);
            case 0x0206DF14: return TVSegment_Print_UndergroundTraps(fieldSystem, template, episode);
            case 0x0206DF88: return TVSegment_Print_UndergroundFlags(fieldSystem, template, episode);
            case 0x0206E018: return TVSegment_Print_CaptureCount(fieldSystem, template, episode);
            case 0x0206E098: return TVSegment_Print_DailyCount(fieldSystem, template, episode);
            case 0x0206E118: return TVSegment_Print_InterviewSection0B7C(fieldSystem, template, episode);
            case 0x0206E1C0: return TVSegment_Print_InterviewHatchedMon(fieldSystem, template, episode);
            case 0x0206E28C: return TVSegment_Print_InterviewPoketch(fieldSystem, template, episode);
            case 0x0206E300: return TVSegment_Print_InterviewContest(fieldSystem, template, episode);
            case 0x0206E3C8: return TVSegment_Print_InterviewDressUp(fieldSystem, template, episode);
            case 0x0206E428: return TVSegment_Print_InterviewAnswer(fieldSystem, template, episode);
            case 0x0206E480: return TVSegment_Print_InterviewPoffin(fieldSystem, template, episode);
            case 0x0206E520: return TVSegment_Print_InterviewNature(fieldSystem, template, episode);
            case 0x0206E5E4: return TVSegment_Print_InterviewSection0B88(fieldSystem, template, episode);
            case 0x0206E680: return TVSegment_Print_InterviewWord15(fieldSystem, template, episode);
            case 0x0206E6C0: return TVSegment_Print_InterviewWord16(fieldSystem, template, episode);
            case 0x0206E700: return TVSegment_Print_InterviewWord17(fieldSystem, template, episode);
            case 0x0206E740: return TVSegment_Print_InterviewWord18(fieldSystem, template, episode);
            case 0x0206E7AC: return TVSegment_Print_InterviewBattleTower(fieldSystem, template, episode);
            case 0x0206E870: return TVSegment_Print_GroupNews(fieldSystem, template, episode);
            case 0x0206E940: return TVSegment_Print_WeatherForecast(fieldSystem, template, episode);
            case 0x0206EA10: return TVSegment_Print_LocalNews(fieldSystem, template, episode);
            case 0x0206EB94: return TVSegment_Print_Swarm(fieldSystem, template, episode);
            case 0x0206EC90: return TVSegment_Print_BerryReport(fieldSystem, template, episode);
            case 0x0206ED14: return TVSegment_Print_NatureFortune(fieldSystem, template, episode);
            case 0x0206EDAC: return TVSegment_Print_RoamerSighting(fieldSystem, template, episode);
            case 0x0206EEBC: return TVSegment_Print_DressUpPhoto(fieldSystem, template, episode);
            case 0x0206EF7C: return TVSegment_Print_Commercial(fieldSystem, template, episode);
            case 0x0206F01C: return TVSegment_Print_PokemonTrivia(fieldSystem, template, episode);
            case 0x0206F118: return TVSegment_Print_Variety1(fieldSystem, template, episode);
            case 0x0206F13C: return TVSegment_Print_Variety2(fieldSystem, template, episode);
            case 0x0206F160: return TVSegment_Print_PartnerPokemon(fieldSystem, template, episode);
            case 0x0206F278: return TVSegment_Print_Variety3(fieldSystem, template, episode);
            case 0x0206F27C: return TVSegment_Print_Variety4(fieldSystem, template, episode);
            case 0x0206F29C: return TVSegment_Print_Variety5(fieldSystem, template, episode);
        }
        throw Unported.Missing(function);
    }

    private static bool TVSegment_IsEligible(uint function, FieldSystem fieldSystem, TVEpisode episode)
    {
        switch (function & ~1u)
        {
            case 0x0206D320: return TVSegment_IsEligible_CatchThatPokemon(fieldSystem, episode);
            case 0x0206D3C0: return TVSegment_IsEligible_UsedItem(fieldSystem, episode);
            case 0x0206D5F0: return TVSegment_IsEligible_SpeciesAt0(fieldSystem, episode);
            case 0x0206D6A8: return TVSegment_IsEligible_NicknamedPokemon(fieldSystem, episode);
            case 0x0206D7A4: return TVSegment_IsEligible_SafariGame(fieldSystem, episode);
            case 0x0206D8F0: return TVSegment_IsEligible_HatchedEgg(fieldSystem, episode);
            case 0x0206D910: return TVSegment_IsEligible_Never(fieldSystem, episode);
            case 0x0206DA50: return TVSegment_IsEligible_BallSeal(fieldSystem, episode);
            case 0x0206DB08: return TVSegment_IsEligible_HasExplorerKit(fieldSystem, episode);
            case 0x0206DB48: return TVSegment_IsEligible_ArrivedEterna(fieldSystem, episode);
            case 0x0206DB9C: return TVSegment_IsEligible_ArrivedEterna2(fieldSystem, episode);
            case 0x0206DC3C: return TVSegment_IsEligible_BattleFrontierStreak(fieldSystem, episode);
            case 0x0206DD1C: return TVSegment_IsEligible_PokemonWeight(fieldSystem, episode);
            case 0x0206DFC8: return TVSegment_IsEligible_HasExplorerKit2(fieldSystem, episode);
            case 0x0206E04C: return TVSegment_IsEligible_ArrivedSunyshore(fieldSystem, episode);
            case 0x0206E0CC: return TVSegment_IsEligible_ArrivedHearthome(fieldSystem, episode);
            case 0x0206E160: return TVSegment_IsEligible_ArrivedSunyshore2(fieldSystem, episode);
            case 0x0206E248: return TVSegment_IsEligible_InterviewHatchedMon(fieldSystem, episode);
            case 0x0206E37C: return TVSegment_IsEligible_InterviewContest(fieldSystem, episode);
            case 0x0206E3F8: return TVSegment_IsEligible_InterviewDressUp(fieldSystem, episode);
            case 0x0206E654: return TVSegment_IsEligible_ArrivedSunyshore3(fieldSystem, episode);
            case 0x0206E834: return TVSegment_IsEligible_ArrivedSunyshore4(fieldSystem, episode);
            case 0x0206E928: return TVSegment_IsEligible_GroupNews(fieldSystem, episode);
            case 0x0206EA0C: return TVSegment_IsEligible_Always(fieldSystem, episode);
            case 0x0206EBD4: return TVSegment_IsEligible_Swarm(fieldSystem, episode);
            case 0x0206EBE4: return TVSegment_IsEligible_Never2(fieldSystem, episode);
            case 0x0206ECFC: return TVSegment_IsEligible_ArrivedHearthome2(fieldSystem, episode);
            case 0x0206ED10: return TVSegment_IsEligible_Never3(fieldSystem, episode);
            case 0x0206EE74: return TVSegment_IsEligible_RoamerSighting(fieldSystem, episode);
            case 0x0206EF64: return TVSegment_IsEligible_DressUpPhoto(fieldSystem, episode);
            case 0x0206F100: return TVSegment_IsEligible_PokedexObtained(fieldSystem, episode);
            case 0x0206F260: return TVSegment_IsEligible_PokedexObtained2(fieldSystem, episode);
            case 0x0206F280: return TVSegment_IsEligible_HasCoalBadge(fieldSystem, episode);
        }
        throw Unported.Missing(function);
    }
}

public static class ContestStrings
{
    private const int RankMaster = 4;
    private const int ContestModeVisual = 5;

    // OFFSET 0x02095888
    public static uint ContestRank_GetNameMessageID(int rank)
    {
        switch (rank)
        {
            case 0:
                return 0x2E;
            case 1:
                return 0x2F;
            case 2:
                return 0x30;
            case 3:
                return 0x31;
        }
        return 0x33;
    }

    // OFFSET 0x020958B8
    public static uint ContestType_GetNameMessageID(int type)
    {
        return ContestType_GetNameMessageIDForMode(type, 2);
    }

    // OFFSET 0x020958C4
    private static uint ContestType_GetNameMessageIDForMode(int type, int mode)
    {
        if (mode == ContestModeVisual)
        {
            return 0x34;
        }
        switch (type)
        {
            case 0:
                return 0x29;
            case 1:
                return 0x2A;
            case 2:
                return 0x2B;
            case 3:
                return 0x2C;
        }
        return 0x2D;
    }
}
