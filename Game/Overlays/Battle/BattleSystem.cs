using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class Battle
{
    private const int HeapIDSystem = 3;
    private const int HeapIDBattle = 5;
    private const int HeapIDEvolution = 0x49;
    private const uint HeapSizeBattle = 0xB0000;
    private const uint HeapSizeEvolution = 0x30000;
    private const uint BattleTypeLink = 4;
    private const uint BattleTypeDoubles = 2;
    private const uint BattleTypeMulti = 8;
    private const uint BattleTypeTag = 0x10;
    private const uint BattleTypeFrontier = 0x80;
    private const uint BattleTypePalPark = 0x200;
    private const uint BattleTypeTrainer = 1;
    private const uint BattleStatusRecording = 0x10;
    private const int OverlayBattleController = 11;
    private const int OverlayBattleAnim = 12;
    private const int OverlayLoadAsync = 2;
    private const int ResultWin = 1;
    private const int ResultCaptured = 4;
    private const int ResultPlayerFled = 5;
    private const int PartySize = 6;
    private const int ParamSpecies = 5;
    private const int ParamIsEgg = 76;
    private const int ParamGender = 111;
    private const int ParamOTName = 144;
    private const int ParamHP = 163;
    private const int ContextParamSelectedPartySlot = 2;
    private const int FriendshipEventBossBattle = 3;
    private const int FirstBossClass = 62;
    private const uint SinglesBattlerTypes = 0x0226E25C;
    private const uint DoublesBattlerTypes = 0x0226E560;

    private const int StateInit = 0;
    private const int StateLinkWaitStart = 1;
    private const int StateLinkWait = 2;
    private const int StateSetup = 3;
    private const int StateLinkIntro = 4;
    private const int StateSetupBanner = 5;
    private const int StateBannerWait = 6;
    private const int StateSyncWait = 7;
    private const int StateBattleInit = 8;
    private const int StateBattleRun = 9;
    private const int StateResultStart = 10;
    private const int StateResultWait = 11;
    private const int StateResultDone = 12;
    private const int StateEvolutionCheck = 13;
    private const int StateEvolutionRun = 14;
    private const int StateEnd = 15;

    // OFFSET 0x0223B140
    public static int Battle_Main(ApplicationManager appMan, ref int state)
    {
        FieldBattleDTO dto = (FieldBattleDTO)ApplicationManager.ApplicationManager_Args(appMan)!;
        switch (state)
        {
            case StateInit:
                Heap.Heap_Create(HeapIDSystem, HeapIDBattle, HeapSizeBattle);
                if ((dto.BattleType & BattleTypeLink) != 0 && (dto.BattleStatusMask & BattleStatusRecording) == 0)
                {
                    state = StateLinkWaitStart;
                }
                else
                {
                    state = StateSetup;
                }
                break;
            case StateLinkWaitStart:
            case StateLinkWait:
            case StateLinkIntro:
            case StateBannerWait:
            case StateSyncWait:
            case StateResultWait:
                throw Unported.Partial(0x0223B140);
            case StateSetup:
                if (BattleSystem_Setup(appMan))
                {
                    state = StateLinkIntro;
                }
                else
                {
                    state = StateSetupBanner;
                }
                break;
            case StateSetupBanner:
                if (BattleSystem_StartLinkBanner(appMan))
                {
                    state = StateBannerWait;
                }
                else
                {
                    state = StateBattleInit;
                }
                break;
            case StateBattleInit:
                GameOverlay.Overlay_LoadByID(OverlayBattleController, OverlayLoadAsync);
                GameOverlay.Overlay_LoadByID(OverlayBattleAnim, OverlayLoadAsync);
                BattleSystem_Init(appMan);
                state = StateBattleRun;
                break;
            case StateBattleRun:
                if (BattleSystem_Run(appMan) == 1)
                {
                    BattleSystem_Exit(appMan);
                    state = StateResultStart;
                }
                break;
            case StateResultStart:
                if (BattleSystem_StartLinkResult(appMan))
                {
                    state = StateResultWait;
                }
                else
                {
                    Heap.Heap_Destroy(HeapIDBattle);
                    state = StateEvolutionCheck;
                }
                break;
            case StateResultDone:
                state = StateEnd;
                break;
            case StateEvolutionCheck:
            {
                int species = FieldBattleDTO_FindEvolvingPartyMember(dto, out int slot, out int evolutionType);
                if (species != 0)
                {
                    Heap.Heap_Create(HeapIDSystem, HeapIDEvolution, HeapSizeEvolution);
                    Pokemon mon = Party.Party_GetPokemonBySlotIndex(dto.Parties[FieldBattleDTO.BattlerPlayer]!, slot);
                    dto.EvolutionData = sub_0207AE68(dto.Parties[FieldBattleDTO.BattlerPlayer]!, mon, species, dto.Options!, dto.VisitedContestHall, dto.Pokedex!, dto.Bag!, dto.GameRecords!, dto.Poketch!, evolutionType, 3, HeapIDEvolution);
                    state = StateEvolutionRun;
                }
                else
                {
                    state = StateEnd;
                }
                break;
            }
            case StateEvolutionRun:
                if (sub_0207B0D0(dto.EvolutionData) == 1)
                {
                    sub_0207B0E0(dto.EvolutionData);
                    Heap.Heap_Destroy(HeapIDEvolution);
                    state = StateEvolutionCheck;
                }
                break;
            case StateEnd:
                return 1;
        }
        return 0;
    }

    // OFFSET 0x0223C2C0
    private static void BattleSystem_InitFromDTO(BattleSystem battleSys, FieldBattleDTO dto)
    {
        battleSys.BattleType = dto.BattleType;
        for (int i = 0; i < BattleSystem.MaxBattlerSlots; i++)
        {
            battleSys.TrainerInfos[i] = TrainerInfo.TrainerInfo_New(HeapIDBattle);
            TrainerInfo.TrainerInfo_Copy(dto.TrainerInfos[i]!, battleSys.TrainerInfos[i]!);
            battleSys.ChatotCries[i] = dto.ChatotCries[i];
        }
        battleSys.NetworkID = dto.NetworkID;
        for (int i = 0; i < BattleSystem.MaxBattlerSlots; i++)
        {
            battleSys.LinkPlayerPositions[i] = dto.NetworkIDs[i];
        }
        battleSys.SeedLCRNG = MathUtil.LCRNG_GetSeed();
        battleSys.SeedRandNext = dto.Seed;
        battleSys.SeedDTO = dto.Seed;
        battleSys.BattleStatusMask = dto.BattleStatusMask;
        battleSys.Bag = BagItems.Bag_New(HeapIDBattle);
        BagItems.Bag_Copy(dto.Bag!, battleSys.Bag);
        battleSys.Pokedex = Pokedex.Pokedex_New(HeapIDBattle);
        Pokedex.Pokedex_Copy(dto.Pokedex!, battleSys.Pokedex);
        battleSys.PcBoxes = (PCBoxes?)dto.PCBoxes;
        battleSys.Options = dto.Options;
        battleSys.PalPad = dto.PalPad;
        battleSys.BagCursor = dto.BagCursor;
        battleSys.SubscreenCursorOn = dto.SubscreenCursorOn;
        battleSys.Poketch = dto.Poketch;
        battleSys.MapEvolutionMethod = dto.MapEvolutionMethod;
        battleSys.CaptureAttempt = dto.CaptureAttempt;
        battleSys.SafariBalls = dto.CountSafariBalls;
        battleSys.BattleTerrain = dto.Terrain;
        battleSys.Background = dto.Background;
        battleSys.MapLabelTextID = dto.MapLabelTextID;
        battleSys.Time = dto.TimeOfDay;
        battleSys.RulesetMask = dto.RulesetMask;
        battleSys.VisitedContestHall = dto.VisitedContestHall;
        battleSys.MetBebe = dto.MetBebe;
        battleSys.FieldWeather = dto.FieldWeather;
        battleSys.Records = dto.GameRecords;
        if (dto.GameRecords == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < BattleSystem.MaxBattlerSlots; i++)
        {
            battleSys.TrainerIDs[i] = (ushort)dto.TrainerIDs[i];
            Array.Copy(dto.Trainers[i], battleSys.Trainers[i], TrainerData.Size);
        }
        battleSys.BattleCtx = BattleContexts.BattleContext_New(battleSys);
        for (int i = 0; i < BattleSystem.MaxBattlerSlots; i++)
        {
            battleSys.Parties[i] = Party.Party_New(HeapIDBattle);
        }
        for (int i = 0; i < BattleSystem.MaxBattlerSlots; i++)
        {
            for (int slot = 0; slot < Party.Party_GetCurrentCount(dto.Parties[i]!); slot++)
            {
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(dto.Parties[i]!, slot);
                Pokemon.Pokemon_SetValue(mon, ParamGender, Pokemon.Pokemon_GetGender(mon));
            }
        }
        if ((battleSys.BattleType & BattleTypeLink) != 0 || (battleSys.BattleType & BattleTypeTag) != 0 || (battleSys.BattleType & BattleTypeMulti) != 0)
        {
            throw Unported.Partial(0x0223C2C0);
        }
        if ((battleSys.BattleType & BattleTypeDoubles) != 0)
        {
            byte[] types = RomData.Overlay(16, DoublesBattlerTypes, BattleSystem.MaxBattlerSlots);
            for (int i = 0; i < BattleSystem.MaxBattlerSlots; i++)
            {
                battleSys.Battlers[i] = Battlers.Battler_New(battleSys, i, types[i]);
                Battlers.Battler_InitTaskFuncs(battleSys, battleSys.Battlers[i]);
            }
            battleSys.MaxBattlers = BattleSystem.MaxBattlerSlots;
            for (int i = 0; i < BattleSystem.MaxBattlerSlots; i++)
            {
                Party.Party_Copy(dto.Parties[i]!, battleSys.Parties[i]!);
            }
            for (int battler = 0; battler < battleSys.MaxBattlers; battler++)
            {
                int side = battler & 1;
                Party party = battleSys.Parties[side]!;
                int slot;
                for (slot = 0; slot < Party.Party_GetCurrentCount(party); slot++)
                {
                    Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, slot);
                    if (battler > 1 && slot == BattleContexts.BattleContext_Get(battleSys, battleSys.BattleCtx, ContextParamSelectedPartySlot, side))
                    {
                        continue;
                    }
                    if (Pokemon_CanBattle(mon))
                    {
                        break;
                    }
                }
                BattleContexts.BattleContext_Set(battleSys, battleSys.BattleCtx, ContextParamSelectedPartySlot, battler, slot);
            }
            BattleContexts.BattleContext_InitPartyOrder(battleSys, battleSys.BattleCtx);
            battleSys.BattleInitialized = 1;
        }
        else
        {
            byte[] types = RomData.Overlay(16, SinglesBattlerTypes, 2);
            for (int i = 0; i < 2; i++)
            {
                battleSys.Battlers[i] = Battlers.Battler_New(battleSys, i, types[i]);
                Battlers.Battler_InitTaskFuncs(battleSys, battleSys.Battlers[i]);
            }
            battleSys.MaxBattlers = 2;
            for (int battler = 0; battler < BattleSystem.MaxBattlerSlots; battler++)
            {
                Party.Party_Copy(dto.Parties[battler]!, battleSys.Parties[battler]!);
                Party party = battleSys.Parties[battler]!;
                int slot;
                for (slot = 0; slot < Party.Party_GetCurrentCount(party); slot++)
                {
                    if (Pokemon_CanBattle(Party.Party_GetPokemonBySlotIndex(party, slot)))
                    {
                        break;
                    }
                }
                BattleContexts.BattleContext_Set(battleSys, battleSys.BattleCtx, ContextParamSelectedPartySlot, battler, slot);
            }
            BattleContexts.BattleContext_InitPartyOrder(battleSys, battleSys.BattleCtx);
            battleSys.BattleInitialized = 1;
        }
        if ((battleSys.BattleType & BattleTypePalPark) != 0)
        {
            ushort[] name = new ushort[TrainerInfo.NameLength];
            Pokemon.Pokemon_GetValue(Party.Party_GetPokemonBySlotIndex(battleSys.Parties[1]!, 0), ParamOTName, name);
            for (int i = 0; i < TrainerInfo.NameLength; i++)
            {
                Bin.W16(battleSys.Trainers[1], TrainerData.NameOffset + i * 2, name[i]);
            }
        }
        if ((battleSys.BattleType & BattleTypeTrainer) != 0)
        {
            if (TrainerClass_IsBoss(battleSys.Trainers[1][TrainerData.ClassOffset]) || TrainerClass_IsBoss(battleSys.Trainers[3][TrainerData.ClassOffset]))
            {
                for (int side = 0; side <= 2; side += 2)
                {
                    Party party = battleSys.Parties[side]!;
                    for (int slot = 0; slot < Party.Party_GetCurrentCount(party); slot++)
                    {
                        Pokemon.Pokemon_UpdateFriendship(Party.Party_GetPokemonBySlotIndex(party, slot), FriendshipEventBossBattle, (ushort)battleSys.MapLabelTextID);
                    }
                }
            }
        }
    }

    private static bool Pokemon_CanBattle(Pokemon mon)
    {
        return Pokemon.Pokemon_GetValue(mon, ParamSpecies, null) != 0 && Pokemon.Pokemon_GetValue(mon, ParamIsEgg, null) == 0 && Pokemon.Pokemon_GetValue(mon, ParamHP, null) != 0;
    }

    // OFFSET 0x0223CD3C
    private static bool TrainerClass_IsBoss(int trainerClass)
    {
        switch (trainerClass - FirstBossClass)
        {
            case 0:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
            case 17:
                return true;
        }
        return false;
    }

    private static bool BattleSystem_IsLocalBattle(uint battleType, uint battleStatusMask)
    {
        return (battleType & BattleTypeLink) == 0 || (battleStatusMask & BattleStatusRecording) != 0 || (battleType & BattleTypeFrontier) != 0;
    }

    // OFFSET 0x0223D800
    private static bool BattleSystem_Setup(ApplicationManager appMan)
    {
        ApplicationManager.ApplicationManager_NewData(appMan, BattleSystem.Size, HeapIDBattle);
        BattleSystem battleSys = new();
        appMan.Data = battleSys;
        FieldBattleDTO dto = (FieldBattleDTO)ApplicationManager.ApplicationManager_Args(appMan)!;
        BattleSystem_InitFromDTO(battleSys, dto);
        if (BattleSystem_IsLocalBattle(battleSys.BattleType, battleSys.BattleStatusMask))
        {
            NetworkIcon.NetworkIcon_Destroy();
            return false;
        }
        throw Unported.Partial(0x0223D800);
    }

    // OFFSET 0x0223D98C
    private static bool BattleSystem_StartLinkBanner(ApplicationManager appMan)
    {
        BattleSystem battleSys = (BattleSystem)ApplicationManager.ApplicationManager_Data(appMan)!;
        ApplicationManager.ApplicationManager_Args(appMan);
        if (BattleSystem_IsLocalBattle(battleSys.BattleType, battleSys.BattleStatusMask))
        {
            NetworkIcon.NetworkIcon_Destroy();
            return false;
        }
        throw Unported.Partial(0x0223D98C);
    }

    // OFFSET 0x0223DB1C
    private static bool BattleSystem_StartLinkResult(ApplicationManager appMan)
    {
        FieldBattleDTO dto = (FieldBattleDTO)ApplicationManager.ApplicationManager_Args(appMan)!;
        if (BattleSystem_IsLocalBattle(dto.BattleType, dto.BattleStatusMask))
        {
            return false;
        }
        throw Unported.Partial(0x0223DB1C);
    }

    // OFFSET 0x0223ECC4
    private static int FieldBattleDTO_FindEvolvingPartyMember(FieldBattleDTO dto, out int slot, out int evolutionType)
    {
        slot = 0;
        evolutionType = 0;
        if (dto.ResultMask != ResultWin && dto.ResultMask != ResultCaptured && dto.ResultMask != ResultPlayerFled)
        {
            return 0;
        }
        while (dto.LeveledUpMonsMask != 0)
        {
            for (slot = 0; slot < PartySize; slot++)
            {
                if ((dto.LeveledUpMonsMask & (1u << slot)) != 0)
                {
                    dto.LeveledUpMonsMask &= ~(1u << slot);
                    break;
                }
            }
            if (slot < PartySize)
            {
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(dto.Parties[FieldBattleDTO.BattlerPlayer]!, slot);
                int species = Pokemon.Pokemon_GetEvolutionTargetSpecies(dto.Parties[FieldBattleDTO.BattlerPlayer]!, mon, 0, (ushort)dto.MapEvolutionMethod, out evolutionType);
                if (species != 0)
                {
                    return species;
                }
            }
        }
        return 0;
    }
}

public static class NetworkIcon
{
    public static object? sNetworkIcon;

    private const int DefaultX = 0xF0;
    private const int DefaultY = 0;

    // OFFSET 0x02039734
    public static void NetworkIcon_Init()
    {
        int isWiFi = 0;
        if (CommManager.CommMan_IsWiFi() != 0)
        {
            isWiFi = 1;
        }
        NetworkIcon_InitAt(DefaultX, DefaultY, isWiFi, 1);
    }

    // OFFSET 0x02039750
    public static void NetworkIcon_InitAt(int x, int y, int isWiFi, int vramType)
    {
        if (WirelessManager.WirelessDriver_IsInitialized())
        {
            throw Unported.Partial(0x02039750);
        }
    }

    // OFFSET 0x020397E4
    public static void NetworkIcon_InitIfConnected()
    {
        if (CommSystem.CommSys_IsInitialized() != 0)
        {
            if (CommSystem.CommSys_ConnectedCount() > 1 || CommManager.CommMan_IsWiFi() != 0)
            {
                NetworkIcon_Init();
            }
        }
        else if (CommManager.CommMan_IsWiFi() != 0)
        {
            NetworkIcon_Init();
        }
    }

    // OFFSET 0x020397B0
    public static void NetworkIcon_SetStrength(int strength)
    {
        if (sNetworkIcon != null)
        {
            throw Unported.Partial(0x020397B0);
        }
    }

    // OFFSET 0x02039794
    public static void NetworkIcon_Destroy()
    {
        if (sNetworkIcon != null)
        {
            throw Unported.Partial(0x02039794);
        }
    }
}
