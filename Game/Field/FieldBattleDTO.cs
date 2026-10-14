using static PlatinumSharp.Game.Globals;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class CaptureAttempt
{
    public const int Size = 0x24;

    private const int MonDataSpecies = 5;
    private const int MonDataGender = 0x6F;
    private const int MonDataLanguage = 0xC;
    private const int MonDataForm = 0x7A;
    private const int MonDataPokeball = 0x9B;
    private const int MonDataHasNickname = 0x4D;
    private const int MonDataNicknameString = 0x77;
    private const int NicknameLength = 0xB;
    private const uint NicknameBufferSize = 0x40;

    public readonly byte[] Data = new byte[Size];

    // OFFSET 0x0206D140
    public static CaptureAttempt CaptureAttempt_New(int heapID)
    {
        return new CaptureAttempt();
    }

    // OFFSET 0x0206D158
    public static void CaptureAttempt_Free(CaptureAttempt? captureAttempt)
    {
    }

    // OFFSET 0x0206D160
    public static void CaptureAttempt_Init(CaptureAttempt captureAttempt, Pokemon mon, int resultMask, int ballsThrown, int heapID)
    {
        byte[] data = captureAttempt.Data;
        Array.Clear(data, 0, Size);
        Nitro.Bin.W32(data, 0, (uint)resultMask);
        Nitro.Bin.W16(data, 4, (ushort)ballsThrown);
        Pokemon_GetCaptureInfo(mon, out ushort species, out byte gender, out byte language, out byte form);
        Nitro.Bin.W16(data, 6, species);
        data[8] = gender;
        data[9] = language;
        data[0xA] = form;
        ushort ball = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataPokeball, null);
        Nitro.Bin.W16(data, 0x22, ball);
        if (ball == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        ushort[] nickname = new ushort[NicknameLength];
        Pokemon_GetNicknameIfSet(heapID, mon, out data[0xB], nickname);
        for (int i = 0; i < NicknameLength; i++)
        {
            Nitro.Bin.W16(data, 0xC + i * 2, nickname[i]);
        }
    }

    // OFFSET 0x0206CE38
    public static void Pokemon_GetCaptureInfo(Pokemon mon, out ushort species, out byte gender, out byte language, out byte form)
    {
        species = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
        gender = (byte)Pokemon.Pokemon_GetValue(mon, MonDataGender, null);
        language = (byte)Pokemon.Pokemon_GetValue(mon, MonDataLanguage, null);
        form = (byte)Pokemon.Pokemon_GetValue(mon, MonDataForm, null);
    }

    // OFFSET 0x0206CED0
    public static void Pokemon_GetNicknameIfSet(int heapID, Pokemon mon, out byte hasNickname, ushort[] nickname)
    {
        hasNickname = (byte)Pokemon.Pokemon_GetValue(mon, MonDataHasNickname, null);
        if (hasNickname != 0)
        {
            GameString str = GameString.String_Init(NicknameBufferSize, heapID);
            Pokemon.Pokemon_GetValue(mon, MonDataNicknameString, str);
            GameString.String_ToChars(str, nickname, NicknameLength);
            GameString.String_Free(str);
        }
    }
}

public sealed class FieldBattleDTO
{
    public const int Size = 0x1C8;
    public const int MaxBattlers = 4;
    public const int TrainerSize = 0x34;
    public const int TrainerClassOffset = 1;
    public const int TrainerNameOffset = 0x14;
    public const int BattlerPlayer = 0;
    public const int BattlerEnemy1 = 1;
    public const int BattlerPartner = 2;
    public const int BattlerEnemy2 = 3;
    public const int BattleTypeWild = 0;
    public const int BattleTypeTrainer = 1;
    public const int BattleTypeDoubles = 2;
    public const int BattleTypeLink = 4;
    public const int BattleTypeSafari = 0x20;
    public const int BattleTypeFrontier = 0x80;
    public const int BattleTypeRoamer = 0x100;
    public const int BattleTypePalPark = 0x200;
    public const int BattleTypeCatchTutorial = 0x400;
    public const int TerrainSand = 1;
    public const int TerrainGrass = 2;
    public const int TerrainCave = 5;
    public const int TerrainSnow = 6;
    public const int TerrainWater = 7;
    public const int TerrainIce = 8;
    public const int TerrainBuilding = 9;
    public const int TerrainMud = 10;
    public const int TerrainMax = 0x18;
    public const int BackgroundWater = 1;
    public const int BackgroundBuilding = 6;

    private const int BackgroundCount = 0x17;
    private const uint BackgroundTerrainTable = 0x020EC294;
    private const int PlayerFormSurfing = 2;
    private const int PlayerFormOffset = 4;
    private const int LocationXOffset = 8;
    private const int LocationZOffset = 0xC;
    private const int MessageBankCounterpartNames = 0x229;
    private const int NarcMessages = 0x1A;
    private const int ItemPokeBall = 4;
    private const int TutorialBallCount = 20;
    private const int TutorialPlayerLevel = 5;
    private const int TutorialEnemyLevel = 2;
    private const int SpeciesBidoof = 399;
    private const int NameLength = 8;

    public uint BattleType;
    public readonly Party?[] Parties = new Party?[MaxBattlers];
    public int ResultMask;
    public readonly int[] TrainerIDs = new int[MaxBattlers];
    public readonly byte[][] Trainers = { new byte[TrainerSize], new byte[TrainerSize], new byte[TrainerSize], new byte[TrainerSize] };
    public readonly TrainerInfo?[] TrainerInfos = new TrainerInfo?[MaxBattlers];
    public Bag? Bag;
    public BagCursor? BagCursor;
    public Pokedex? Pokedex;
    public object? PCBoxes;
    public readonly ChatotCry?[] ChatotCries = new ChatotCry?[MaxBattlers];
    public Poketch? Poketch;
    public WiFiHistory? WiFiHistory;
    public Options? Options;
    public CaptureAttempt? CaptureAttempt;
    public readonly byte[] BattleRecords = new byte[0xC];
    public GameRecords? GameRecords;
    public Journal? JournalEntry;
    public PalPad? PalPad;
    public int Background;
    public int Terrain;
    public int MapLabelTextID;
    public int MapHeaderID;
    public int TimeOfDay;
    public int MapEvolutionMethod;
    public int VisitedContestHall;
    public int MetBebe;
    public int CaughtBattlerIndex;
    public int FieldWeather;
    public uint LeveledUpMonsMask;
    public readonly uint[] SystemVersion = new uint[MaxBattlers];
    public object? EvolutionData;
    public uint BattleStatusMask;
    public int CountSafariBalls;
    public uint RulesetMask;
    public int Unknown198;
    public uint Seed;
    public readonly int[] NetworkIDs = new int[MaxBattlers];
    public ushort NetworkID;
    public byte Unknown1B2;
    public byte Unknown1B3;
    public int TotalTurnsElapsed;
    public byte[]? SubscreenCursorOn;
    public readonly byte[] Unknown1BC = new byte[4];
    public SaveData? SaveData;
    public uint RecordingStopped;

    // OFFSET 0x02051D8C
    public static FieldBattleDTO FieldBattleDTO_New(int heapID, uint battleType)
    {
        FieldBattleDTO dto = new();
        dto.BattleType = battleType;
        dto.BattleStatusMask = 0;
        dto.ResultMask = 0;
        for (int i = 0; i < MaxBattlers; i++)
        {
            dto.TrainerIDs[i] = 0;
            Array.Clear(dto.Trainers[i], 0, TrainerSize);
        }
        dto.Background = 0;
        dto.Terrain = TerrainMax;
        dto.MapLabelTextID = 0;
        dto.TimeOfDay = 0;
        dto.MapEvolutionMethod = 0;
        dto.VisitedContestHall = 1;
        dto.MetBebe = 1;
        dto.FieldWeather = 0;
        for (int i = 0; i < MaxBattlers; i++)
        {
            dto.Parties[i] = Party.Party_New(heapID);
        }
        for (int i = 0; i < MaxBattlers; i++)
        {
            dto.TrainerInfos[i] = TrainerInfo.TrainerInfo_New(heapID);
        }
        for (int i = 0; i < MaxBattlers; i++)
        {
            dto.ChatotCries[i] = ChatotCry.ChatotCry_New(heapID);
        }
        dto.Bag = BagItems.Bag_New(heapID);
        dto.Pokedex = Pokedex.Pokedex_New(heapID);
        dto.Options = Options.Options_New(heapID);
        dto.CaptureAttempt = CaptureAttempt.CaptureAttempt_New(heapID);
        dto.BagCursor = null;
        dto.SubscreenCursorOn = null;
        dto.CountSafariBalls = 0;
        dto.WiFiHistory = null;
        dto.GameRecords = null;
        dto.JournalEntry = null;
        dto.RulesetMask = 0;
        Rtc.GetCurrentDateTime(out RtcDate date, out RtcTime time);
        dto.Seed = (uint)gSystem.FrameCounter + (uint)(date.Year + (((date.Month << 8) * date.Day) << 16) + (time.Hour << 16) + ((time.Minute + time.Second) << 24));
        if (CommSystem.CommSys_IsInitialized() == 1)
        {
            for (int i = 0; i < CommSystem.CommSys_ConnectedCount(); i++)
            {
                dto.NetworkIDs[i] = CommSystem.CommSys_GetPlayerSlot(i);
            }
            dto.NetworkID = CommSystem.CommSys_CurNetId();
        }
        Array.Clear(dto.BattleRecords, 0, dto.BattleRecords.Length);
        return dto;
    }

    // OFFSET 0x02051F24
    public static FieldBattleDTO FieldBattleDTO_NewSafari(int heapID, int countSafariBalls)
    {
        FieldBattleDTO dto = FieldBattleDTO_New(heapID, BattleTypeSafari);
        dto.CountSafariBalls = countSafariBalls;
        return dto;
    }

    // OFFSET 0x02051F38
    public static FieldBattleDTO FieldBattleDTO_NewPalPark(int heapID, int countParkBalls)
    {
        FieldBattleDTO dto = FieldBattleDTO_New(heapID, BattleTypePalPark);
        dto.CountSafariBalls = countParkBalls;
        return dto;
    }

    // OFFSET 0x02051F4C
    public static FieldBattleDTO FieldBattleDTO_NewCatchingTutorial(int heapID, FieldSystem fieldSystem)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!);
        Options options = PlayerSave.SaveData_GetOptions(fieldSystem.SaveData!);
        FieldBattleDTO dto = FieldBattleDTO_New(heapID, BattleTypeCatchTutorial);
        MessageLoader? loader = MessageLoader.MessageLoader_Init(1, NarcMessages, MessageBankCounterpartNames, heapID);
        GameString name = GameString.String_Init(NameLength, heapID);
        MessageLoader.MessageLoader_GetString(loader!, TrainerInfo.TrainerInfo_Gender(trainerInfo) ^ 1, name);
        TrainerInfo.TrainerInfo_SetName(dto.TrainerInfos[BattlerPlayer]!, GameString.String_GetData(name));
        GameString.String_Free(name);
        MessageLoader.MessageLoader_Free(loader);
        TrainerInfo.TrainerInfo_SetGender(dto.TrainerInfos[BattlerPlayer]!, (int)(TrainerInfo.TrainerInfo_Gender(trainerInfo) ^ 1));
        FieldBattleDTO_SetupBackground(dto, fieldSystem);
        Options.Options_Copy(options, dto.Options!);
        dto.TimeOfDay = FieldTime.FieldSystem_GetTimeOfDay(fieldSystem);
        BagItems.Bag_TryAddItem(dto.Bag!, ItemPokeBall, TutorialBallCount, heapID);
        Pokemon mon = Pokemon.Pokemon_New(heapID);
        Pokemon.Pokemon_InitWith(mon, SystemVars.SystemVars_GetPlayerCounterpartStarter(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)), TutorialPlayerLevel, Pokemon.RandomIVs, 0, 0, Pokemon.OTIDTypeNotShiny, 0);
        Party.Party_AddPokemon(dto.Parties[BattlerPlayer]!, mon);
        Pokemon.Pokemon_InitWith(mon, SpeciesBidoof, TutorialEnemyLevel, Pokemon.RandomIVs, 0, 0, Pokemon.OTIDTypeNotShiny, 0);
        Party.Party_AddPokemon(dto.Parties[BattlerEnemy1]!, mon);
        dto.PCBoxes = SaveDataTable.SaveData_GetPCBoxes(fieldSystem.SaveData!);
        dto.BagCursor = fieldSystem.BagCursor;
        dto.SubscreenCursorOn = null;
        dto.GameRecords = GameRecords.SaveData_GetGameRecords(fieldSystem.SaveData!);
        dto.JournalEntry = fieldSystem.JournalEntry;
        dto.MapHeaderID = (int)fieldSystem.Location!.U32(0);
        FieldBattleDTO_CopyPlayerInfoToTrainerData(dto);
        return dto;
    }

    // OFFSET 0x020520A4
    public static void FieldBattleDTO_Free(FieldBattleDTO dto)
    {
        for (int i = 0; i < MaxBattlers; i++)
        {
            if (dto.Parties[i] == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            dto.Parties[i] = null;
        }
        for (int i = 0; i < MaxBattlers; i++)
        {
            if (dto.TrainerInfos[i] == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            dto.TrainerInfos[i] = null;
        }
        for (int i = 0; i < MaxBattlers; i++)
        {
            if (dto.ChatotCries[i] == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            dto.ChatotCries[i] = null;
        }
        dto.Bag = null;
        dto.Pokedex = null;
        dto.Options = null;
        CaptureAttempt.CaptureAttempt_Free(dto.CaptureAttempt);
    }

    // OFFSET 0x0205213C
    public static void FieldBattleDTO_AddPokemonToBattler(FieldBattleDTO dto, Pokemon mon, int battler)
    {
        if (battler >= MaxBattlers)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!Party.Party_AddPokemon(dto.Parties[battler]!, mon))
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02052164
    public static void FieldBattleDTO_CopyPartyToBattler(FieldBattleDTO dto, Party party, int battler)
    {
        if (battler >= MaxBattlers)
        {
            ErrorHandling.GF_AssertFail();
        }
        Party.Party_Copy(party, dto.Parties[battler]!);
    }

    // OFFSET 0x02052184
    public static void FieldBattleDTO_CopyTrainerInfoToBattler(FieldBattleDTO dto, TrainerInfo trainerInfo, int battler)
    {
        if (battler >= MaxBattlers)
        {
            ErrorHandling.GF_AssertFail();
        }
        TrainerInfo.TrainerInfo_Copy(trainerInfo, dto.TrainerInfos[battler]!);
    }

    // OFFSET 0x020521A4
    public static void FieldBattleDTO_CopyChatotCryToBattler(FieldBattleDTO dto, ChatotCry chatotCry, int battler)
    {
        ChatotCry.ChatotCry_Copy(dto.ChatotCries[battler]!, chatotCry);
    }

    // OFFSET 0x020521B8
    public static void FieldBattleDTO_InitFromGameState(FieldBattleDTO dto, FieldSystem? fieldSystem, SaveData saveData, int mapHeaderID, Journal? journalEntry, BagCursor? bagCursor, byte[]? subscreenCursorOn)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(saveData);
        Party party = Party.SaveData_GetParty(saveData);
        Bag bag = BagItems.SaveData_GetBag(saveData);
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(saveData);
        ChatotCry chatotCry = ChatotCry.SaveData_GetChatotCry(saveData);
        Options options = PlayerSave.SaveData_GetOptions(saveData);
        FieldPlayerState fieldPlayerState = FieldPlayerState.SaveData_GetFieldPlayerState(saveData);
        if (fieldSystem != null)
        {
            FieldBattleDTO_SetupBackground(dto, fieldSystem);
            dto.TimeOfDay = FieldTime.FieldSystem_GetTimeOfDay(fieldSystem);
        }
        else
        {
            dto.Background = MapHeader.MapHeader_GetBattleBG(mapHeaderID);
            dto.Terrain = TerrainBuilding;
            dto.TimeOfDay = Rtc.TimeOfDayForHour(SystemData.SaveData_GetGameTime(saveData).Time.Hour);
        }
        FieldBattleDTO_CopyTrainerInfoToBattler(dto, trainerInfo, BattlerPlayer);
        FieldBattleDTO_CopyPartyToBattler(dto, party, BattlerPlayer);
        BagItems.Bag_Copy(bag, dto.Bag!);
        Pokedex.Pokedex_Copy(pokedex, dto.Pokedex!);
        Options.Options_Copy(options, dto.Options!);
        FieldBattleDTO_CopyChatotCryToBattler(dto, chatotCry, BattlerPlayer);
        dto.PCBoxes = SaveDataTable.SaveData_GetPCBoxes(saveData);
        dto.MapLabelTextID = MapHeader.MapHeader_GetMapLabelTextID(mapHeaderID);
        dto.MapEvolutionMethod = MapHeader.MapHeader_GetMapEvolutionMethod(mapHeaderID);
        dto.VisitedContestHall = PokemonSummaryShared.PokemonSummaryScreen_ShowContestData(saveData);
        dto.MetBebe = SystemFlags.SystemFlag_CheckMetBebe(VarsFlags.SaveData_GetVarsFlags(saveData)) ? 1 : 0;
        dto.FieldWeather = FieldPlayerState.FieldPlayerState_GetWeather(fieldPlayerState);
        dto.BagCursor = bagCursor;
        dto.SubscreenCursorOn = subscreenCursorOn;
        dto.Poketch = FieldPoketch.SaveData_GetPoketch(saveData);
        dto.WiFiHistory = WiFiHistory.SaveData_WiFiHistory(saveData);
        dto.GameRecords = GameRecords.SaveData_GetGameRecords(saveData);
        dto.JournalEntry = journalEntry;
        dto.PalPad = PalPad.SaveData_GetPalPad(saveData);
        dto.MapHeaderID = mapHeaderID;
        dto.SaveData = saveData;
    }

    // OFFSET 0x02052314
    public static void FieldBattleDTO_Init(FieldBattleDTO dto, FieldSystem fieldSystem)
    {
        FieldBattleDTO_InitFromGameState(dto, fieldSystem, fieldSystem.SaveData!, (int)fieldSystem.Location!.U32(0), fieldSystem.JournalEntry, fieldSystem.BagCursor, fieldSystem.BattleSubscreenCursorOn);
        FieldBattleDTO_CopyPlayerInfoToTrainerData(dto);
    }

    // OFFSET 0x020526E8
    public static void FieldBattleDTO_UpdateFieldSystem(FieldBattleDTO dto, FieldSystem fieldSystem)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!);
        Party party = Party.SaveData_GetParty(fieldSystem.SaveData!);
        Bag bag = BagItems.SaveData_GetBag(fieldSystem.SaveData!);
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!);
        BufferView safariBalls = FieldPlayerState.FieldPlayerState_GetSafariBallCount(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        TrainerInfo.TrainerInfo_Copy(dto.TrainerInfos[BattlerPlayer]!, trainerInfo);
        Party.Party_Copy(dto.Parties[BattlerPlayer]!, party);
        BagItems.Bag_Copy(dto.Bag!, bag);
        Pokedex.Pokedex_Copy(dto.Pokedex!, pokedex);
        safariBalls.W16(0, dto.CountSafariBalls);
    }

    // OFFSET 0x02052754
    public static void FieldBattleDTO_UpdatePokedex(FieldBattleDTO dto, FieldSystem fieldSystem)
    {
        PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!);
        Party.SaveData_GetParty(fieldSystem.SaveData!);
        BagItems.SaveData_GetBag(fieldSystem.SaveData!);
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!);
        Pokedex.Pokedex_Copy(dto.Pokedex!, pokedex);
    }

    // OFFSET 0x02052780
    private static int FieldSystem_GetBattleTerrain(FieldSystem fieldSystem, int background)
    {
        byte behavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, (int)fieldSystem.Location!.U32(LocationXOffset), (int)fieldSystem.Location!.U32(LocationZOffset));
        if (TileBehavior.TileBehavior_IsIce(behavior) != 0)
        {
            return TerrainIce;
        }
        if (TileBehavior.TileBehavior_IsTallGrass(behavior) != 0 || TileBehavior.TileBehavior_IsVeryTallGrass(behavior) != 0)
        {
            return TerrainGrass;
        }
        if (TileBehavior.TileBehavior_IsSand(behavior) != 0)
        {
            return TerrainSand;
        }
        if (TileBehavior.TileBehavior_IsSnow(behavior) != 0)
        {
            return TerrainSnow;
        }
        if (TileBehavior.TileBehavior_IsMud(behavior) != 0 || TileBehavior.TileBehavior_IsMudWithGrass(behavior) != 0)
        {
            return TerrainMud;
        }
        if (TileBehavior.TileBehavior_IsCaveFloor(behavior) != 0)
        {
            return TerrainCave;
        }
        if (TileBehavior.TileBehavior_IsSurfable(behavior) != 0)
        {
            return TerrainWater;
        }
        if (background < BackgroundCount)
        {
            return (int)RomData.Arm9U32(BackgroundTerrainTable + (uint)background * 4, 1)[0];
        }
        ErrorHandling.GF_AssertFail();
        return TerrainMax;
    }

    // OFFSET 0x0205281C
    public static void FieldBattleDTO_SetupBackground(FieldBattleDTO dto, FieldSystem fieldSystem)
    {
        BufferView playerData = FieldPlayerState.FieldPlayerState_GetPlayerData(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        dto.Background = MapHeader.MapHeader_GetBattleBG((int)fieldSystem.Location!.U32(0));
        if (playerData.U32(PlayerFormOffset) == PlayerFormSurfing)
        {
            dto.Background = BackgroundWater;
        }
        dto.Terrain = FieldSystem_GetBattleTerrain(fieldSystem, dto.Background);
    }

    // OFFSET 0x0205285C
    public static void FieldBattleDTO_SetWaterTerrain(FieldBattleDTO dto)
    {
        dto.Terrain = TerrainWater;
    }

    // OFFSET 0x02052894
    public static void FieldBattleDTO_CopyPlayerInfoToTrainerData(FieldBattleDTO dto)
    {
        byte[] trainer = dto.Trainers[BattlerPlayer];
        trainer[TrainerClassOffset] = (byte)TrainerInfo.TrainerInfo_Gender(dto.TrainerInfos[BattlerPlayer]!);
        ushort[] name = TrainerInfo.TrainerInfo_Name(dto.TrainerInfos[BattlerPlayer]!);
        for (int i = 0; i < NameLength; i++)
        {
            Nitro.Bin.W16(trainer, TrainerNameOffset + i * 2, name[i]);
            if (name[i] == 0xFFFF)
            {
                break;
            }
        }
        Array.Copy(trainer, dto.Trainers[BattlerPartner], TrainerSize);
    }
}
