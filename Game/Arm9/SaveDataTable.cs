using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class SaveTableEntry
{
    public int ID;
    public int BlockID;
    public Func<int> SizeFunc = () => 0;
    public Func<object> CreateFunc = () => new object();
    public Action<object>? InitFunc;
}

public sealed class ExtraSaveTableEntry
{
    public int ID;
    public int Sector;
    public Func<int> SizeFunc = () => 0;
    public Action<byte[]> InitFunc = _ => { };
}

public static class SaveDataTable
{
    public const int EntrySystem = 0;
    public const int EntryPlayer = 1;
    public const int EntryParty = 2;
    public const int EntryBag = 3;
    public const int EntryVarsFlags = 4;
    public const int EntryPoketch = 5;
    public const int EntryFieldPlayerState = 6;
    public const int EntryPokedex = 7;
    public const int EntryDaycare = 8;
    public const int EntryPalPad = 9;
    public const int EntryMisc = 10;
    public const int EntryFieldOverworldState = 11;
    public const int EntryUnderground = 12;
    public const int EntryRegulationBattles = 13;
    public const int EntryImageClips = 14;
    public const int EntryMailbox = 15;
    public const int EntryPoffins = 16;
    public const int EntryRecordMixedRNG = 17;
    public const int EntryJournal = 18;
    public const int EntryTrainerCase = 19;
    public const int EntryGameRecords = 20;
    public const int EntrySealCase = 21;
    public const int EntryChatot = 22;
    public const int EntryFrontier = 23;
    public const int EntryRibbons = 24;
    public const int EntryEncounters = 25;
    public const int EntryGlobalTrade = 26;
    public const int EntryTVBroadcast = 27;
    public const int EntryRankings = 28;
    public const int EntryWiFiList = 29;
    public const int EntryWiFiHistory = 30;
    public const int EntryMysteryGift = 31;
    public const int EntryPalParkTransfer = 32;
    public const int EntryLinkContestRecords = 33;
    public const int EntryUnlockedEasyChatWords = 34;
    public const int EntryEmail = 35;
    public const int EntryWiFiPlazaQuestions = 36;
    public const int EntryPCBoxes = 37;
    public const int ExtraHallOfFame = 0;
    public const int ExtraFrontier = 1;
    public const int ExtraMyRecording = 2;

    public static readonly SaveTableEntry[] sSaveTable =
    {
        new SaveTableEntry
        {
            ID = EntrySystem,
            BlockID = SaveData.BlockNormal,
            SizeFunc = SystemData_SaveSize,
            CreateFunc = () => new SystemData(),
            InitFunc = block => SystemData.SystemData_Clear((SystemData)block),
        },
        new SaveTableEntry
        {
            ID = EntryPlayer,
            BlockID = SaveData.BlockNormal,
            SizeFunc = PlayerData_SaveSize,
            CreateFunc = () => new PlayerSave(),
            InitFunc = block => PlayerSave.Player_Init((PlayerSave)block),
        },
        new SaveTableEntry
        {
            ID = EntryParty,
            BlockID = SaveData.BlockNormal,
            SizeFunc = Party_SaveSize,
            CreateFunc = () => new Party(),
            InitFunc = block => Party.Party_Init((Party)block),
        },
        new SaveTableEntry
        {
            ID = EntryBag,
            BlockID = SaveData.BlockNormal,
            SizeFunc = Bag_SaveSize,
            CreateFunc = () => new Bag(),
            InitFunc = block => Bag.Bag_Init((Bag)block),
        },
        new SaveTableEntry
        {
            ID = EntryVarsFlags,
            BlockID = SaveData.BlockNormal,
            SizeFunc = VarsFlags_SaveSize,
            CreateFunc = () => new VarsFlags(),
            InitFunc = block => VarsFlags.VarsFlags_Init((VarsFlags)block),
        },
        new SaveTableEntry
        {
            ID = EntryPoketch,
            BlockID = SaveData.BlockNormal,
            SizeFunc = Poketch_SaveSize,
            CreateFunc = () => new Poketch(),
            InitFunc = block => Poketch.Poketch_Init((Poketch)block),
        },
        new SaveTableEntry
        {
            ID = EntryFieldPlayerState,
            BlockID = SaveData.BlockNormal,
            SizeFunc = FieldPlayerState_SaveSize,
            CreateFunc = () => new FieldPlayerState(),
            InitFunc = block => FieldPlayerState.FieldPlayerState_Init((FieldPlayerState)block),
        },
        new SaveTableEntry
        {
            ID = EntryPokedex,
            BlockID = SaveData.BlockNormal,
            SizeFunc = Pokedex_SaveSize,
            CreateFunc = () => new Pokedex(),
            InitFunc = block => Pokedex.Pokedex_Init((Pokedex)block),
        },
        new SaveTableEntry
        {
            ID = EntryDaycare,
            BlockID = SaveData.BlockNormal,
            SizeFunc = Daycare_SaveSize,
            CreateFunc = () => new Daycare(),
            InitFunc = block => Daycare.Daycare_Init((Daycare)block),
        },
        new SaveTableEntry
        {
            ID = EntryPalPad,
            BlockID = SaveData.BlockNormal,
            SizeFunc = PalPad_SaveSize,
            CreateFunc = () => new PalPad(),
            InitFunc = block => PalPad.PalPad_Init((PalPad)block),
        },
        new SaveTableEntry
        {
            ID = EntryMisc,
            BlockID = SaveData.BlockNormal,
            SizeFunc = MiscSaveBlock_SaveSize,
            CreateFunc = () => new MiscSaveBlock(),
            InitFunc = block => MiscSaveBlock.MiscSaveBlock_Init((MiscSaveBlock)block),
        },
        new SaveTableEntry
        {
            ID = EntryFieldOverworldState,
            BlockID = SaveData.BlockNormal,
            SizeFunc = FieldOverworldState_SaveSize,
            CreateFunc = () => new FieldOverworldState(),
            InitFunc = block => FieldOverworldState.FieldOverworldState_Init((FieldOverworldState)block),
        },
        new SaveTableEntry
        {
            ID = EntryUnderground,
            BlockID = SaveData.BlockNormal,
            SizeFunc = Underground_SaveSize,
            CreateFunc = () => new Underground(),
            InitFunc = block => Underground.Underground_Init((Underground)block),
        },
        new SaveTableEntry
        {
            ID = EntryRegulationBattles,
            BlockID = SaveData.BlockNormal,
            SizeFunc = RegulationBattles_SaveSize,
            CreateFunc = () => new BattleRegulationData(),
            InitFunc = block => BattleRegulation.RegulationBattles_Init((BattleRegulationData)block),
        },
        new SaveTableEntry
        {
            ID = EntryImageClips,
            BlockID = SaveData.BlockNormal,
            SizeFunc = ImageClips_SaveSize,
            CreateFunc = () => new ImageClips(),
            InitFunc = block => ImageClips.ImageClips_Init((ImageClips)block),
        },
        new SaveTableEntry
        {
            ID = EntryMailbox,
            BlockID = SaveData.BlockNormal,
            SizeFunc = Mailbox_SaveSize,
            CreateFunc = () => new Mailbox(),
            InitFunc = block => Mailbox.Mailbox_Init((Mailbox)block),
        },
        new SaveTableEntry
        {
            ID = EntryPoffins,
            BlockID = SaveData.BlockNormal,
            SizeFunc = PoffinCase_SaveSize,
            CreateFunc = () => new PoffinCase(),
            InitFunc = block => PoffinCase.PoffinCase_Init((PoffinCase)block),
        },
        new SaveTableEntry
        {
            ID = EntryRecordMixedRNG,
            BlockID = SaveData.BlockNormal,
            SizeFunc = RecordMixedRNG_SaveSize,
            CreateFunc = () => new RecordMixedRNG(),
            InitFunc = block => RecordMixedRNG.RecordMixedRNG_Init((RecordMixedRNG)block),
        },
        new SaveTableEntry
        {
            ID = EntryJournal,
            BlockID = SaveData.BlockNormal,
            SizeFunc = Journal_SaveSize,
            CreateFunc = () => new Journal(),
            InitFunc = block => Journal.Journal_Init((Journal)block),
        },
        new SaveTableEntry
        {
            ID = EntryTrainerCase,
            BlockID = SaveData.BlockNormal,
            SizeFunc = TrainerCase_SaveSize,
            CreateFunc = () => new TrainerCaseSaveData(),
            InitFunc = block => TrainerCaseSaveData.TrainerCaseSaveData_Init((TrainerCaseSaveData)block),
        },
        new SaveTableEntry
        {
            ID = EntryGameRecords,
            BlockID = SaveData.BlockNormal,
            SizeFunc = GameRecords_SaveSize,
            CreateFunc = () => new GameRecords(),
            InitFunc = block => GameRecords.GameRecords_Init((GameRecords)block),
        },
        new SaveTableEntry
        {
            ID = EntrySealCase,
            BlockID = SaveData.BlockNormal,
            SizeFunc = SealCase_SaveSize,
            CreateFunc = () => new SealCase(),
            InitFunc = block => SealCase.SealCase_Init((SealCase)block),
        },
        new SaveTableEntry
        {
            ID = EntryChatot,
            BlockID = SaveData.BlockNormal,
            SizeFunc = ChatotCry_SaveSize,
            CreateFunc = () => new ChatotCry(),
            InitFunc = block => ChatotCry.ChatotCry_Init((ChatotCry)block),
        },
        new SaveTableEntry
        {
            ID = EntryFrontier,
            BlockID = SaveData.BlockNormal,
            SizeFunc = Frontier_SaveSize,
            CreateFunc = () => new BattleFrontierSave(),
            InitFunc = block => BattleFrontierSave.BattleFrontier_Init((BattleFrontierSave)block),
        },
        new SaveTableEntry
        {
            ID = EntryRibbons,
            BlockID = SaveData.BlockNormal,
            SizeFunc = Ribbons_SaveSize,
            CreateFunc = () => new RibbonSaveData(),
            InitFunc = block => RibbonSaveData.RibbonSaveData_Init((RibbonSaveData)block),
        },
        new SaveTableEntry
        {
            ID = EntryEncounters,
            BlockID = SaveData.BlockNormal,
            SizeFunc = Encounters_SaveSize,
            CreateFunc = () => new SpecialEncounter(),
            InitFunc = block => SpecialEncounter.SpecialEncounter_Init((SpecialEncounter)block),
        },
        new SaveTableEntry
        {
            ID = EntryGlobalTrade,
            BlockID = SaveData.BlockNormal,
            SizeFunc = GlobalTrade_SaveSize,
            CreateFunc = () => new GlobalTrade(),
            InitFunc = block => GlobalTrade.GlobalTrade_Init((GlobalTrade)block),
        },
        new SaveTableEntry
        {
            ID = EntryTVBroadcast,
            BlockID = SaveData.BlockNormal,
            SizeFunc = TVBroadcast_SaveSize,
            CreateFunc = () => new TVBroadcast(),
            InitFunc = block => TVBroadcast.TVBroadcast_Init((TVBroadcast)block),
        },
        new SaveTableEntry
        {
            ID = EntryRankings,
            BlockID = SaveData.BlockNormal,
            SizeFunc = Rankings_SaveSize,
            CreateFunc = () => new Rankings(),
            InitFunc = block => Rankings.Rankings_Init((Rankings)block),
        },
        new SaveTableEntry
        {
            ID = EntryWiFiList,
            BlockID = SaveData.BlockNormal,
            SizeFunc = WiFiList_SaveSize,
            CreateFunc = () => new WiFiList(),
            InitFunc = block => WiFiList.WiFiList_Init((WiFiList)block),
        },
        new SaveTableEntry
        {
            ID = EntryWiFiHistory,
            BlockID = SaveData.BlockNormal,
            SizeFunc = WiFiHistory_SaveSize,
            CreateFunc = () => new WiFiHistory(),
            InitFunc = block => WiFiHistory.WiFiHistory_Init((WiFiHistory)block),
        },
        new SaveTableEntry
        {
            ID = EntryMysteryGift,
            BlockID = SaveData.BlockNormal,
            SizeFunc = MysteryGift_SaveSize,
            CreateFunc = () => new MysteryGift(),
            InitFunc = block => MysteryGift.MysteryGift_Init((MysteryGift)block),
        },
        new SaveTableEntry
        {
            ID = EntryPalParkTransfer,
            BlockID = SaveData.BlockNormal,
            SizeFunc = PalParkTransfer_SaveSize,
            CreateFunc = () => new MigratedPokemon(),
            InitFunc = block => MigratedPokemon.PalParkTransfer_Init((MigratedPokemon)block),
        },
        new SaveTableEntry
        {
            ID = EntryLinkContestRecords,
            BlockID = SaveData.BlockNormal,
            SizeFunc = LinkContestRecords_SaveSize,
            CreateFunc = () => new LinkContestRecords(),
            InitFunc = block => LinkContestRecords.LinkContestRecords_Init((LinkContestRecords)block),
        },
        new SaveTableEntry
        {
            ID = EntryUnlockedEasyChatWords,
            BlockID = SaveData.BlockNormal,
            SizeFunc = () => (int)EasyChatWords.UnlockedEasyChatWords_SaveSize(),
            CreateFunc = () => new UnlockedEasyChatWords(),
            InitFunc = block => EasyChatWords.UnlockedEasyChatWords_Init((UnlockedEasyChatWords)block),
        },
        new SaveTableEntry
        {
            ID = EntryEmail,
            BlockID = SaveData.BlockNormal,
            SizeFunc = Email_SaveSize,
            CreateFunc = () => new Email(),
            InitFunc = block => Email.Email_Init((Email)block),
        },
        new SaveTableEntry
        {
            ID = EntryWiFiPlazaQuestions,
            BlockID = SaveData.BlockNormal,
            SizeFunc = WiFiPlazaQuestions_SaveSize,
            CreateFunc = () => new WiFiPlazaQuestions(),
            InitFunc = block => WiFiPlazaQuestions.WiFiPlazaQuestions_Init((WiFiPlazaQuestions)block),
        },
        new SaveTableEntry
        {
            ID = EntryPCBoxes,
            BlockID = SaveData.BlockBoxes,
            SizeFunc = PCBoxes_SaveSize,
            CreateFunc = () => new PCBoxes(),
            InitFunc = block => PCBoxes.PCBoxes_Init((PCBoxes)block),
        },
    };

    public static readonly ExtraSaveTableEntry[] sExtraSaveTable =
    {
        new() { ID = 0, Sector = 0x20, SizeFunc = HallOfFame_SaveSize, InitFunc = HallOfFame.HallOfFame_Init },
        new() { ID = 1, Sector = 0x23, SizeFunc = FrontierExtra_SaveSize, InitFunc = sub_02030888 },
        new() { ID = 2, Sector = 0x24, SizeFunc = BattleRecording_SaveSize, InitFunc = sub_0202F1DC },
        new() { ID = 3, Sector = 0x26, SizeFunc = BattleRecording_SaveSize, InitFunc = sub_0202F1DC },
        new() { ID = 4, Sector = 0x28, SizeFunc = BattleRecording_SaveSize, InitFunc = sub_0202F1DC },
        new() { ID = 5, Sector = 0x2A, SizeFunc = BattleRecording_SaveSize, InitFunc = sub_0202F1DC },
    };

    private static SaveTableEntry Entry(int id, int blockID, Func<int> sizeFunc, Func<object> createFunc)
    {
        return new SaveTableEntry { ID = id, BlockID = blockID, SizeFunc = sizeFunc, CreateFunc = createFunc };
    }

    // OFFSET 0x0202440C
    public static object SaveData_GetTVBroadcast(SaveData saveData)
    {
        SaveData.SaveData_Checksum(EntryTVBroadcast);
        return SaveData.SaveData_SaveTable(saveData, EntryTVBroadcast);
    }

    // OFFSET 0x02024420
    public static object SaveData_GetPCBoxes(SaveData saveData)
    {
        return SaveData.SaveData_SaveTable(saveData, EntryPCBoxes);
    }

    // OFFSET 0x0202442C
    public static object SaveData_GetMysteryGift(SaveData saveData)
    {
        SaveData.SaveData_Checksum(EntryMysteryGift);
        return SaveData.SaveData_SaveTable(saveData, EntryMysteryGift);
    }

    // OFFSET 0x02024440
    public static object SaveData_GetPalParkTransfer(SaveData saveData)
    {
        return SaveData.SaveData_SaveTable(saveData, EntryPalParkTransfer);
    }

    // OFFSET 0x0202444C
    public static byte[]? SaveData_LoadHallOfFame(SaveData saveData, int heapID, out int loadResult)
    {
        return SaveData.SaveDataExtra_Get(saveData, heapID, ExtraHallOfFame, out loadResult);
    }

    // OFFSET 0x02024458
    public static int SaveData_SaveHallOfFame(SaveData saveData, byte[] hallOfFame)
    {
        return SaveData.SaveDataExtra_Save(saveData, ExtraHallOfFame, hallOfFame);
    }

    // OFFSET 0x02024464
    public static byte[]? SaveData_LoadBattleRecording(SaveData saveData, int heapID, out int loadResult, int slot)
    {
        return SaveData.SaveDataExtra_Mirror(saveData, heapID, slot + ExtraMyRecording, out loadResult, out _);
    }

    // OFFSET 0x0202447C
    public static int SaveData_SaveBattleRecording(SaveData saveData, byte[] recording, int slot)
    {
        return SaveData.SaveDataExtra_SaveMirror(saveData, slot + ExtraMyRecording, recording);
    }

    // OFFSET 0x0202448C
    public static byte[]? SaveData_LoadFrontierExtra(SaveData saveData, int heapID, out int loadResult)
    {
        return SaveData.SaveDataExtra_Mirror(saveData, heapID, ExtraFrontier, out loadResult, out _);
    }

    // OFFSET 0x020244A0
    public static int SaveData_SaveFrontierExtra(SaveData saveData, byte[] frontierExtra)
    {
        return SaveData.SaveDataExtra_SaveMirror(saveData, ExtraFrontier, frontierExtra);
    }

    // OFFSET 0x02025CB0
    public static int SystemData_SaveSize()
    {
        return 0x5C;
    }

    // OFFSET 0x02025E08
    public static int PlayerData_SaveSize()
    {
        return 0x2C;
    }

    // OFFSET 0x02079FEC
    public static int Party_SaveSize()
    {
        return 0x590;
    }

    // OFFSET 0x0207D3B8
    public static int Bag_SaveSize()
    {
        return 0x774;
    }

    // OFFSET 0x020507CC
    public static int VarsFlags_SaveSize()
    {
        return 0x3AC;
    }

    // OFFSET 0x02056720
    public static int Poketch_SaveSize()
    {
        return 0x118;
    }

    // OFFSET 0x0203A6DC
    public static int FieldPlayerState_SaveSize()
    {
        return 0xA0;
    }

    // OFFSET 0x0202631C
    public static int Pokedex_SaveSize()
    {
        return 0x324;
    }

    // OFFSET 0x020261E4
    public static int Daycare_SaveSize()
    {
        return 0x1E0;
    }

    // OFFSET 0x02027F84
    public static int PalPad_SaveSize()
    {
        return 0x880;
    }

    // OFFSET 0x020277A4
    public static int MiscSaveBlock_SaveSize()
    {
        return 0x78C;
    }

    // OFFSET 0x0203A6E0
    public static int FieldOverworldState_SaveSize()
    {
        return 0x1400;
    }

    // OFFSET 0x0202854C
    public static int Underground_SaveSize()
    {
        return 0x9B4;
    }

    // OFFSET 0x02026030
    public static int RegulationBattles_SaveSize()
    {
        return 0x20;
    }

    // OFFSET 0x02029C58
    public static int ImageClips_SaveSize()
    {
        return 0x834;
    }

    // OFFSET 0x0202843C
    public static int Mailbox_SaveSize()
    {
        return 0x460;
    }

    // OFFSET 0x0202AB34
    public static int PoffinCase_SaveSize()
    {
        return 0x320;
    }

    // OFFSET 0x0202B37C
    public static int RecordMixedRNG_SaveSize()
    {
        return 0x108;
    }

    // OFFSET 0x0202B604
    public static int Journal_SaveSize()
    {
        return 0x460;
    }

    // OFFSET 0x0202C7FC
    public static int TrainerCase_SaveSize()
    {
        return 0x620;
    }

    // OFFSET 0x0202CD50
    public static int GameRecords_SaveSize()
    {
        return 0x1BC;
    }

    // OFFSET 0x0202C9F4
    public static int SealCase_SaveSize()
    {
        return 0x170;
    }

    // OFFSET 0x0202CC64
    public static int ChatotCry_SaveSize()
    {
        return 0x3EC;
    }

    // OFFSET 0x0203061C
    public static int Frontier_SaveSize()
    {
        return 0x1628;
    }

    // OFFSET 0x0202D778
    public static int Ribbons_SaveSize()
    {
        return 0xE;
    }

    // OFFSET 0x0202D7A8
    public static int Encounters_SaveSize()
    {
        return 0x154;
    }

    // OFFSET 0x0202DA4C
    public static int GlobalTrade_SaveSize()
    {
        return 0xF8;
    }

    // OFFSET 0x0202E2CC
    public static int TVBroadcast_SaveSize()
    {
        return 0xBC8;
    }

    // OFFSET 0x0202E884
    public static int Rankings_SaveSize()
    {
        return 0xEA0;
    }

    // OFFSET 0x0202ACE0
    public static int WiFiList_SaveSize()
    {
        return 0x8C0;
    }

    // OFFSET 0x0202C858
    public static int WiFiHistory_SaveSize()
    {
        return 0xFF8;
    }

    // OFFSET 0x0202DAB4
    public static int MysteryGift_SaveSize()
    {
        return 0x1328;
    }

    // OFFSET 0x0202EEC0
    public static int PalParkTransfer_SaveSize()
    {
        return 0x688;
    }

    // OFFSET 0x0202F108
    public static int LinkContestRecords_SaveSize()
    {
        return 0x28;
    }

    // OFFSET 0x02030CE8
    public static int Email_SaveSize()
    {
        return 0x40;
    }

    // OFFSET 0x02030EA4
    public static int WiFiPlazaQuestions_SaveSize()
    {
        return 8;
    }

    // OFFSET 0x020797D4
    public static int PCBoxes_SaveSize()
    {
        return 0x121C8;
    }

    // OFFSET 0x0202DF8C
    public static int HallOfFame_SaveSize()
    {
        return 0x2AB0;
    }

    // OFFSET 0x02030880
    public static int FrontierExtra_SaveSize()
    {
        return 0xBA0;
    }

    // OFFSET 0x0202F1D4
    public static int BattleRecording_SaveSize()
    {
        return 0x1D50;
    }
}
