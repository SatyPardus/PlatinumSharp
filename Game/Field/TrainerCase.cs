namespace PlatinumSharp.Game;

public sealed class TrainerCase : BufferView
{
    public const int Size = 0x66C;
    public const int MaxBadges = 8;
    public const int SignatureSize = 0x600;

    public const int GameVersionOffset = 0x00;
    public const int LanguageOffset = 0x01;
    public const int GymLeadersToHideOffset = 0x02;
    public const int CardLevelOffset = 0x03;
    public const int FlagsOffset = 0x04;
    public const int TrainerAppearanceOffset = 0x05;
    public const int NameOffset = 0x08;
    public const int MoneyOffset = 0x1C;
    public const int SeenPokemonOffset = 0x20;
    public const int ScoreOffset = 0x24;
    public const int TrainerIDOffset = 0x28;
    public const int PlayTimeHoursOffset = 0x2A;
    public const int HofDebutHourOffset = 0x2C;
    public const int PlayTimeMinutesOffset = 0x2E;
    public const int AdventureStartedYearOffset = 0x2F;
    public const int AdventureStartedMonthOffset = 0x30;
    public const int AdventureStartedDayOffset = 0x31;
    public const int HofDebutYearOffset = 0x32;
    public const int HofDebutMonthOffset = 0x33;
    public const int HofDebutDayOffset = 0x34;
    public const int HofDebutMinuteOffset = 0x35;
    public const int TimesLinkedOffset = 0x38;
    public const int LinkBattleWinsOffset = 0x3C;
    public const int LinkBattleLossesOffset = 0x40;
    public const int LinkTradesOffset = 0x44;
    public const int BadgesOffset = 0x48;
    public const int SignatureOffset = 0x68;
    public const int ChecksumOffset = 0x668;
    public const int UnusedOffset = 0x66A;

    public const int FlagBadgesInteractable = 0;
    public const int FlagLiveTimeDisplay = 1;
    public const int FlagGender = 2;
    public const int FlagPokedexObtained = 3;

    private const int GameVersionPlatinum = 0xC;
    private const int RecordTimesLinkedBase = 0x20;
    private const int RecordLinkContests = 0x19;
    private const int RecordUnionBattles = 0x14;
    private const int RecordLinkTrades = 0x18;
    private const int RecordWifiBattles = 0x5B;
    private const int RecordWifiTrades = 0x13;
    private const int RecordLinkBattleWins = 0x15;
    private const int RecordWifiBattleWins = 0x1A;
    private const int RecordLinkBattleLosses = 0x16;
    private const int RecordWifiBattleLosses = 0x1B;
    private const uint MaxTimesLinked = 999999;
    private const uint MaxLinkBattles = 9999;
    private const uint MaxLinkTrades = 99999;
    private const int FrontierStreakThreshold = 100;

    public PlayTime? PlayTime;

    public TrainerCase(byte[] buffer) : base(buffer, 0)
    {
    }

    private void SetFlag(int bit, int value)
    {
        SetBits(FlagsOffset, bit, 1, value & 1);
    }

    // OFFSET 0x02071D40
    public static void TrainerCase_Init(byte badgesInteractable, byte liveTimeDisplay, byte gymLeadersToHide, byte trainerAppearance, FieldSystem fieldSystem, TrainerCase trainerCase)
    {
        SaveData saveData = FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!;
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(saveData);
        GameRecords records = GameRecords.SaveData_GetGameRecords(saveData);
        trainerCase.W8(TrainerAppearanceOffset, trainerAppearance);
        byte cardLevel = TrainerCase_CalculateTrainerCardLevel(fieldSystem);
        TrainerCase_SetFields(badgesInteractable, GameVersionPlatinum, cardLevel, gymLeadersToHide, TrainerInfo.TrainerInfo_Language(trainerInfo), trainerCase);
        ushort id = TrainerInfo.TrainerInfo_ID_LowHalf(trainerInfo);
        byte gender = (byte)TrainerInfo.TrainerInfo_Gender(trainerInfo);
        ushort[] name = TrainerInfo.TrainerInfo_Name(trainerInfo);
        uint money = TrainerInfo.TrainerInfo_Money(trainerInfo);
        uint seen = Pokedex.Pokedex_CountSeen(Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!));
        bool pokedexObtained = Pokedex.Pokedex_IsObtained(Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!));
        uint score = GameRecords.GameRecords_GetTrainerScore(records);
        TrainerCase_SetTrainerInfo(id, gender, name, money, seen, pokedexObtained, score, trainerCase);
        PlayTime playTime = PlayerSave.SaveData_GetPlayTime(saveData);
        FieldTime.FieldSystem_GetStartTimestamp(fieldSystem, out RtcDate startDate, out RtcTime time);
        FieldTime.FieldSystem_GetFirstCompletionTimestamp(fieldSystem, out RtcDate completionDate, out time);
        bool gameCompleted = SystemFlags.SystemFlag_CheckGameCompleted(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
        TrainerCase_SetDates((byte)(gameCompleted ? 1 : 0), playTime, startDate, completionDate, time, liveTimeDisplay, trainerCase);
        TrainerCaseSaveData caseSaveData = TrainerCaseSaveData.SaveData_GetTrainerCaseSaveData(fieldSystem.SaveData!);
        uint timesLinkedBase = GameRecords.GameRecords_GetRecordValue(records, RecordTimesLinkedBase);
        uint linkContests = GameRecords.GameRecords_GetRecordValue(records, RecordLinkContests);
        uint unionBattles = GameRecords.GameRecords_GetRecordValue(records, RecordUnionBattles);
        uint linkTrades = GameRecords.GameRecords_GetRecordValue(records, RecordLinkTrades);
        uint wifiBattles = GameRecords.GameRecords_GetRecordValue(records, RecordWifiBattles);
        uint wifiTrades = GameRecords.GameRecords_GetRecordValue(records, RecordWifiTrades);
        uint linkActivities = linkContests + unionBattles + linkTrades + wifiBattles + wifiTrades;
        uint linkWins = GameRecords.GameRecords_GetRecordValue(records, RecordLinkBattleWins);
        uint wifiWins = GameRecords.GameRecords_GetRecordValue(records, RecordWifiBattleWins);
        uint linkLosses = GameRecords.GameRecords_GetRecordValue(records, RecordLinkBattleLosses);
        uint wifiLosses = GameRecords.GameRecords_GetRecordValue(records, RecordWifiBattleLosses);
        uint tradesWifi = GameRecords.GameRecords_GetRecordValue(records, RecordWifiTrades);
        uint tradesLink = GameRecords.GameRecords_GetRecordValue(records, RecordLinkTrades);
        byte[] signature = TrainerCaseSaveData.TrainerCaseSaveData_GetSignature(caseSaveData);
        TrainerCase_SetLinkDataAndSignature(timesLinkedBase + linkActivities, linkWins + wifiWins, linkLosses + wifiLosses, tradesWifi + tradesLink, signature, trainerCase);
        TrainerCase_SetBadgeData(trainerInfo, fieldSystem, trainerCase);
        int checksum = 0;
        for (int i = 0; i < Size; i++)
        {
            checksum ^= trainerCase.U8(i);
        }
        trainerCase.W16(ChecksumOffset, checksum);
        trainerCase.W16(UnusedOffset, 0);
    }

    // OFFSET 0x02071F04
    public static TrainerCase TrainerCase_New(ushort heapID)
    {
        byte[] buffer = Heap.Heap_Alloc(heapID, Size)!;
        Array.Clear(buffer, 0, Size);
        return new TrainerCase(buffer);
    }

    // OFFSET 0x02071F20
    public static void TrainerCase_Free(TrainerCase trainerCase)
    {
        Heap.Heap_Free(trainerCase.Buffer);
    }

    // OFFSET 0x02071F28
    public static byte TrainerCase_CalculateTrainerCardLevel(FieldSystem fieldSystem)
    {
        SaveData saveData = FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!;
        GameRecords.SaveData_GetGameRecords(saveData);
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(saveData);
        UndergroundRecord undergroundRecord = SecretBase.SaveData_GetUndergroundRecord(saveData);
        BattleFrontierSave frontier = BattleFrontierSave.SaveData_GetBattleFrontier(saveData);
        byte level = 0;
        if (SystemFlags.SystemFlag_CheckGameCompleted(varsFlags))
        {
            level++;
        }
        if (Pokedex.Pokedex_NationalDexCompleted(Pokedex.SaveData_GetPokedex(saveData)))
        {
            level++;
        }
        if (BattleFrontierSave.BattleFrontierSave_GetStat(frontier, 0, 0xFF) >= FrontierStreakThreshold
            || BattleFrontierSave.BattleFrontierSave_GetStat(frontier, 2, 0xFF) >= FrontierStreakThreshold
            || BattleFrontierSave.BattleFrontierSave_GetStat(frontier, 4, 0xFF) >= FrontierStreakThreshold
            || BattleFrontierSave.BattleFrontierSave_GetStat(frontier, 6, 0xFF) >= FrontierStreakThreshold
            || BattleFrontierSave.BattleFrontierSave_GetStat(frontier, 8, 0xFF) >= FrontierStreakThreshold)
        {
            level++;
        }
        if (SystemFlags.SystemFlag_CheckContestMaster(varsFlags, 0)
            || SystemFlags.SystemFlag_CheckContestMaster(varsFlags, 1)
            || SystemFlags.SystemFlag_CheckContestMaster(varsFlags, 3)
            || SystemFlags.SystemFlag_CheckContestMaster(varsFlags, 4)
            || SystemFlags.SystemFlag_CheckContestMaster(varsFlags, 2))
        {
            level++;
        }
        if (UndergroundRecord.UndergroundRecord_HasPlatBaseFlag(undergroundRecord))
        {
            level++;
        }
        return level;
    }

    // OFFSET 0x02072014
    private static void TrainerCase_SetFields(byte badgesInteractable, byte gameVersion, byte cardLevel, byte gymLeadersToHide, byte language, TrainerCase trainerCase)
    {
        trainerCase.SetFlag(FlagBadgesInteractable, badgesInteractable);
        trainerCase.W8(GameVersionOffset, gameVersion);
        trainerCase.W8(CardLevelOffset, cardLevel);
        trainerCase.W8(LanguageOffset, language);
        trainerCase.W8(GymLeadersToHideOffset, gymLeadersToHide);
    }

    // OFFSET 0x02072038
    private static void TrainerCase_SetTrainerInfo(ushort id, byte gender, ushort[] name, uint money, uint seenPokemon, bool pokedexObtained, uint score, TrainerCase trainerCase)
    {
        trainerCase.W16(TrainerIDOffset, id);
        trainerCase.SetFlag(FlagGender, gender);
        ushort[] nameBuffer = new ushort[8];
        CharCode.CharCode_CopyNumChars(nameBuffer, 0, name, 0, 8);
        for (int i = 0; i < nameBuffer.Length; i++)
        {
            trainerCase.W16(NameOffset + i * 2, nameBuffer[i]);
        }
        trainerCase.W32(MoneyOffset, money);
        trainerCase.W32(SeenPokemonOffset, seenPokemon);
        trainerCase.SetFlag(FlagPokedexObtained, pokedexObtained ? 1 : 0);
        trainerCase.W32(ScoreOffset, score);
    }

    // OFFSET 0x0207207C
    private static void TrainerCase_SetDates(byte gameCompleted, PlayTime playTime, in RtcDate adventureStartedDate, in RtcDate hofDebutDate, in RtcTime hofDebutTime, byte liveTimeDisplay, TrainerCase trainerCase)
    {
        trainerCase.W16(PlayTimeHoursOffset, PlayTime.PlayTime_GetHours(playTime));
        trainerCase.W8(PlayTimeMinutesOffset, PlayTime.PlayTime_GetMinutes(playTime));
        trainerCase.W8(AdventureStartedYearOffset, adventureStartedDate.Year);
        trainerCase.W8(AdventureStartedMonthOffset, adventureStartedDate.Month);
        trainerCase.W8(AdventureStartedDayOffset, adventureStartedDate.Day);
        if (gameCompleted != 0)
        {
            trainerCase.W8(HofDebutYearOffset, hofDebutDate.Year);
            trainerCase.W8(HofDebutMonthOffset, hofDebutDate.Month);
            trainerCase.W8(HofDebutDayOffset, hofDebutDate.Day);
            trainerCase.W16(HofDebutHourOffset, hofDebutTime.Hour);
            trainerCase.W8(HofDebutMinuteOffset, hofDebutTime.Minute);
        }
        else
        {
            trainerCase.W8(HofDebutYearOffset, 0);
            trainerCase.W8(HofDebutMonthOffset, 0);
            trainerCase.W8(HofDebutDayOffset, 0);
            trainerCase.W16(HofDebutHourOffset, 0);
            trainerCase.W8(HofDebutMinuteOffset, 0);
        }
        trainerCase.SetFlag(FlagLiveTimeDisplay, liveTimeDisplay);
        trainerCase.PlayTime = liveTimeDisplay != 0 ? playTime : null;
    }

    // OFFSET 0x02072120
    private static void TrainerCase_SetLinkDataAndSignature(uint timesLinked, uint linkBattleWins, uint linkBattleLosses, uint linkTrades, byte[] signature, TrainerCase trainerCase)
    {
        trainerCase.W32(TimesLinkedOffset, timesLinked > MaxTimesLinked ? MaxTimesLinked : timesLinked);
        trainerCase.W32(LinkBattleWinsOffset, linkBattleWins > MaxLinkBattles ? MaxLinkBattles : linkBattleWins);
        trainerCase.W32(LinkBattleLossesOffset, linkBattleLosses > MaxLinkBattles ? MaxLinkBattles : linkBattleLosses);
        trainerCase.W32(LinkTradesOffset, linkTrades > MaxLinkTrades ? MaxLinkTrades : linkTrades);
        trainerCase.CopyFrom(SignatureOffset, signature, 0, SignatureSize);
    }

    // OFFSET 0x0207216C
    private static void TrainerCase_SetBadgeData(TrainerInfo trainerInfo, FieldSystem fieldSystem, TrainerCase trainerCase)
    {
        uint[] badges = TrainerCaseSaveData.TrainerCaseSaveData_GetBadges(TrainerCaseSaveData.SaveData_GetTrainerCaseSaveData(fieldSystem.SaveData!));
        for (byte i = 0; i < MaxBadges; i++)
        {
            int position = BadgesOffset + i * 4;
            uint badge = trainerCase.U32(position);
            badge = TrainerInfo.TrainerInfo_HasBadge(trainerInfo, i) ? badge | 1 : badge & ~1u;
            uint polish = TrainerCaseSaveData.TrainerCaseSaveDataBadge_GetPolish(i, badges);
            trainerCase.W32(position, (polish << 1) | (badge & 1));
        }
    }

    // OFFSET 0x020721D4
    public static void TrainerCase_SaveBadgePolish(FieldSystem fieldSystem, TrainerCase trainerCase)
    {
        uint[] badges = TrainerCaseSaveData.TrainerCaseSaveData_GetBadges(TrainerCaseSaveData.SaveData_GetTrainerCaseSaveData(fieldSystem.SaveData!));
        for (byte i = 0; i < MaxBadges; i++)
        {
            uint polish = (uint)((int)trainerCase.U32(BadgesOffset + i * 4) >> 1);
            TrainerCaseSaveData.TrainerCaseSaveDataBadge_SetPolish(i, polish, badges);
        }
    }
}
