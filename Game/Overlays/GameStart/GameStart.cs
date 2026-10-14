using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class GameStart
{
    private const int HeapID = 0x4D;
    private const int HeapSize = 0x20000;
    private const int Overlay = 57;
    private const uint NewGameIntroTemplate = 0x020F6824;
    private const uint FieldNewGameTemplate = 0x020EA10C;
    private const uint FieldContinueTemplate = 0x020EA11C;
    private const uint InitialBerryPatchesAddress = 0x021D0FA0;
    private const int InitialBerryPatchCount = 0x76;
    private const int InitialBerryPatchStoredWords = 0xF0;
    private const int InitialBerryPatchTableWords = 0x100;
    private const uint StartingMoney = 0xBB8;
    private const ushort FlagGameStarted = 0x960;

    // OFFSET 0x021D0D80
    public static int GameStartNewSave_Init(ApplicationManager appMan, ref int state)
    {
        Heap.Heap_Create(3, HeapID, HeapSize);
        Main.InitRNG();
        return 1;
    }

    // OFFSET 0x021D0D98
    public static int GameStartNewSave_Main(ApplicationManager appMan, ref int state)
    {
        ApplicationArgs args = (ApplicationArgs)ApplicationManager.ApplicationManager_Args(appMan)!;
        GameStart_ResetSaveData(HeapID, args.SaveData!);
        return 1;
    }

    // OFFSET 0x021D0DAC
    public static int GameStartNewSave_Exit(ApplicationManager appMan, ref int state)
    {
        Heap.Heap_Destroy(HeapID);
        Main.EnqueueApplication(-1, ApplicationTemplate.At(NewGameIntroTemplate));
        return 1;
    }

    // OFFSET 0x021D0DC8
    public static int GameStartNewGame_Init(ApplicationManager appMan, ref int state)
    {
        Heap.Heap_Create(3, HeapID, HeapSize);
        Main.InitRNG();
        return 1;
    }

    // OFFSET 0x021D0DE0
    public static int GameStartNewGame_Main(ApplicationManager appMan, ref int state)
    {
        ApplicationArgs args = (ApplicationArgs)ApplicationManager.ApplicationManager_Args(appMan)!;
        SaveData saveData = args.SaveData!;
        GameStart_InitNewGame(HeapID, saveData, 1);
        PlayTimeManager.PlayTime_Start(PlayerSave.SaveData_GetPlayTime(saveData));
        return 1;
    }

    // OFFSET 0x021D0E00
    public static int GameStartNewGame_Exit(ApplicationManager appMan, ref int state)
    {
        Heap.Heap_Destroy(HeapID);
        Main.EnqueueApplication(-1, ApplicationTemplate.At(FieldNewGameTemplate));
        return 1;
    }

    // OFFSET 0x021D0E1C
    public static int GameStartContinue_Init(ApplicationManager appMan, ref int state)
    {
        Heap.Heap_Create(3, HeapID, HeapSize);
        Main.InitRNG();
        return 1;
    }

    // OFFSET 0x021D0E34
    public static int GameStartContinue_Main(ApplicationManager appMan, ref int state)
    {
        ApplicationArgs args = (ApplicationArgs)ApplicationManager.ApplicationManager_Args(appMan)!;
        SaveData saveData = args.SaveData!;
        SystemData systemData = SystemData.SaveData_GetSystemData(saveData);
        GameStart_LoadSaveData(HeapID, saveData);
        Options.Options_SetSystemButtonMode(saveData, 0);
        if (!SystemData.SystemData_MatchesCurrentSystem(systemData) || !SystemData.SystemData_MatchesCurrentRTCOffset(systemData))
        {
            SystemData.GameTime_StartPenalty(SystemData.SaveData_GetGameTime(saveData));
            SystemData.SystemData_Init(systemData);
            PartyForms.Party_ResetShayminForms(Party.SaveData_GetParty(saveData));
        }
        PlayTimeManager.PlayTime_Start(PlayerSave.SaveData_GetPlayTime(saveData));
        return 1;
    }

    // OFFSET 0x021D0E90
    public static int GameStartContinue_Exit(ApplicationManager appMan, ref int state)
    {
        Heap.Heap_Destroy(HeapID);
        Main.EnqueueApplication(-1, ApplicationTemplate.At(FieldContinueTemplate));
        return 1;
    }

    // OFFSET 0x021D0EAC
    private static void GameStart_InitNewGame(int heapID, SaveData saveData, int setTrainerID)
    {
        SystemData.SystemData_Init(SystemData.SaveData_GetSystemData(saveData));
        SystemData.GameTime_Clear(SystemData.SaveData_GetGameTime(saveData));
        RecordMixedRNG rng = RecordMixedRNG.SaveData_GetRecordMixedRNG(saveData);
        RecordMixedRNG.RecordMixedRNG_SetEntrySeed(rng, 1, MathUtil.MTRNG_Next());
        BattleTowerSeed.BattleTowerSeed_Reset(saveData);
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(saveData);
        uint trainerID = MathUtil.MTRNG_Next();
        if (setTrainerID != 0)
        {
            TrainerInfo.TrainerInfo_SetID(trainerInfo, trainerID);
        }
        uint gender = TrainerInfo.TrainerInfo_Gender(trainerInfo);
        int appearance = PlayerAppearance.PlayerAppearance_GetByTrainerID(trainerID, gender, 0);
        TrainerInfo.TrainerInfo_SetAppearance(trainerInfo, (byte)appearance);
        BerryPatch[] patches = MiscSaveBlock.SaveData_GetBerryPatches(saveData);
        ushort[] initialPatches = new ushort[InitialBerryPatchTableWords];
        ushort[] stored = RomData.OverlayU16(Overlay, InitialBerryPatchesAddress, InitialBerryPatchStoredWords);
        Array.Copy(stored, initialPatches, stored.Length);
        BerryPatches.BerryPatches_Init(patches, heapID, initialPatches, InitialBerryPatchCount);
    }

    // OFFSET 0x021D0F30
    private static void GameStart_LoadSaveData(int heapID, SaveData saveData)
    {
        if (!SaveData.SaveData_Load(saveData))
        {
            sub_020C3EE4(0);
        }
    }

    // OFFSET 0x021D0F44
    private static void GameStart_ResetSaveData(int heapID, SaveData saveData)
    {
        SaveData.SaveData_Clear(saveData);
        FieldPlayerState.FieldPlayerState_InitNewGameLocation(saveData);
        TrainerInfo.TrainerInfo_SetMoney(PlayerSave.SaveData_GetTrainerInfo(saveData), StartingMoney);
        SystemFlags.SystemFlag_SetBagAcquired(VarsFlags.SaveData_GetVarsFlags(saveData));
    }
}
