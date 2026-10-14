using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class NamingScreenTaskEnv
{
    public int State;
    public int PartySlot;
    public BufferView? Result;
    public NamingScreenArgs? Args;
    public GameString? OriginalName;
}

public static class FieldSystemChildProcesses
{
    public const uint BattleTemplate = 0x020EA358;
    public const uint JournalDisplayTemplate = 0x020EA2E8;
    private const int HeapIDFieldTask = 0xB;
    private const int HeapIDFieldMap = 0x20;
    public const uint BagTemplate = 0x020EA338;
    public const uint TrainerCaseTemplate = 0x020EA228;
    public const uint OptionsMenuTemplate = 0x020EA288;
    private const uint NamingScreenTemplate = 0x020F2DAC;
    private const int NamingScreenTypePlayer = 0;
    private const int NamingScreenTypePokemon = 1;
    private const int NamingScreenTypeGroup = 5;
    private const int NamingScreenTypeTablet = 6;
    private const uint NamingScreenNameSize = 12;
    private const int MonDataGender = 0x6F;
    private const int MonDataForm = 0x70;
    private const int MonDataNicknameAndFlag = 0x76;
    private const uint FieldBagPockets = 0x020EA164;
    private const uint BerryBagPockets = 0x020EA154;
    private const uint PoffinBagPockets = 0x020EA158;
    private const int PocketListMaxLength = 9;
    private const int PlayerStateCycling = 1;

    // OFFSET 0x0203D1B8
    public static int FieldBattle_Init(ApplicationManager appMan, ref int state)
    {
        return 1;
    }

    // OFFSET 0x0203D1BC
    public static int FieldBattle_Main(ApplicationManager appMan, ref int state)
    {
        if (Battle.Battle_Main(appMan, ref state) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0203D1D0
    public static int FieldBattle_Exit(ApplicationManager appMan, ref int state)
    {
        return 1;
    }

    // OFFSET 0x0203D1D4
    public static void FieldSystem_StartBattleProcess(FieldSystem fieldSystem, object? battleParams)
    {
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(BattleTemplate), battleParams);
    }

    // OFFSET 0x0203D30C
    public static void FieldSystem_OpenJournalDisplay(FieldSystem fieldSystem, object? unused)
    {
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(JournalDisplayTemplate), fieldSystem.SaveData);
    }

    // OFFSET 0x0203D334
    public static void FieldSystem_OpenSummaryScreen(FieldSystem fieldSystem, PokemonSummary summary)
    {
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(PokemonSummaryScreenApp.PokemonSummaryScreenTemplate), summary);
    }

    // OFFSET 0x0203D344
    public static PartyMenu PartyMenu_New(int heapID, FieldSystem fieldSystem, int type, int mode)
    {
        PartyMenu partyMenu = new();
        partyMenu.Party = Party.SaveData_GetParty(fieldSystem.SaveData!);
        partyMenu.Bag = BagItems.SaveData_GetBag(fieldSystem.SaveData!);
        partyMenu.Mailbox = Mailbox.SaveData_GetMailbox(fieldSystem.SaveData!);
        partyMenu.Options = PlayerSave.SaveData_GetOptions(fieldSystem.SaveData!);
        partyMenu.Type = (byte)type;
        partyMenu.Mode = (byte)mode;
        partyMenu.FieldSystem = fieldSystem;
        return partyMenu;
    }

    // OFFSET 0x0203D390
    public static PartyMenu FieldSystem_OpenPartyMenu(FieldSystem fieldSystem, FieldMoveContext fieldMoveContext, byte partySlot)
    {
        PartyMenu partyMenu = PartyMenu_New(HeapIDFieldTask, fieldSystem, 0, PartyMenus.ModeField);
        partyMenu.FieldMoveContext = fieldMoveContext;
        partyMenu.SelectedMonSlot = partySlot;
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(PartyMenus.PartyMenuTemplate), partyMenu);
        return partyMenu;
    }

    // OFFSET 0x0203D3C0
    public static PartyMenu FieldSystem_OpenPartyMenu_SelectNoPrompt(FieldTask? task, FieldSystem fieldSystem)
    {
        PartyMenu partyMenu = PartyMenu_New(HeapIDFieldTask, fieldSystem, 0, PartyMenus.ModeSelectNoPrompt);
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(PartyMenus.PartyMenuTemplate), partyMenu);
        return partyMenu;
    }

    // OFFSET 0x0203D3E4
    public static PartyMenu FieldSystem_OpenPartyMenu_NpcTrade(FieldTask? task, FieldSystem fieldSystem)
    {
        PartyMenu partyMenu = PartyMenu_New(HeapIDFieldTask, fieldSystem, 0, PartyMenus.ModeNpcTrade);
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(PartyMenus.PartyMenuTemplate), partyMenu);
        return partyMenu;
    }

    // OFFSET 0x0203D408
    public static byte PartyMenu_GetSelectedSlot(PartyMenu partyMenu)
    {
        return partyMenu.SelectedMonSlot;
    }

    // OFFSET 0x0203D410
    public static PartyMenu FieldSystem_OpenPartyMenu_Daycare(FieldTask? task, FieldSystem fieldSystem, byte partySlot)
    {
        PartyMenu partyMenu = PartyMenu_New(HeapIDFieldTask, fieldSystem, 0, PartyMenus.ModeDaycare);
        partyMenu.SelectedMonSlot = partySlot;
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(PartyMenus.PartyMenuTemplate), partyMenu);
        return partyMenu;
    }

    // OFFSET 0x0203D438
    public static byte PartyMenu_GetSelectionResult(PartyMenu partyMenu)
    {
        return partyMenu.MenuSelectionResult;
    }

    // OFFSET 0x0203D1E4
    public static void FieldSystem_StartBagApplication(FieldSystem fieldSystem, BagContext bagContext)
    {
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(BagTemplate), bagContext);
    }

    // OFFSET 0x0203D20C
    public static BagContext FieldSystem_OpenBag(FieldSystem fieldSystem, ItemUseContext itemUseContext)
    {
        Bag bag = BagItems.SaveData_GetBag(fieldSystem.SaveData!);
        BagContext bagContext = BagItems.Bag_CreateContextWithPockets(bag, RomData.Arm9(FieldBagPockets, PocketListMaxLength), HeapIDFieldTask);
        BagContext.BagContext_Init(bagContext, fieldSystem.SaveData!, 0, fieldSystem.BagCursor);
        BagContext.BagContext_SetMapLoadType(bagContext, (ushort)fieldSystem.MapLoadType);
        if (PlayerAvatar.PlayerAvatar_GetPlayerState(fieldSystem.PlayerAvatar!) == PlayerStateCycling)
        {
            BagContext.BagContext_SetIsCycling(bagContext);
        }
        BagContext.BagContext_SetItemUseContext(bagContext, itemUseContext);
        FieldSystem_StartBagApplication(fieldSystem, bagContext);
        return bagContext;
    }

    // OFFSET 0x0203D264
    public static BagContext FieldSystem_OpenBagForSelection(FieldSystem fieldSystem, int pocketSet)
    {
        Bag bag = BagItems.SaveData_GetBag(fieldSystem.SaveData!);
        uint pockets = 0;
        switch (pocketSet)
        {
            case 0:
                pockets = BerryBagPockets;
                break;
            case 1:
                pockets = PoffinBagPockets;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        BagContext bagContext = BagItems.Bag_CreateContextWithPockets(bag, RomData.Arm9(pockets, PocketListMaxLength), HeapIDFieldMap);
        BagContext.BagContext_Init(bagContext, fieldSystem.SaveData!, 3, fieldSystem.BagCursor);
        FieldSystem_StartBagApplication(fieldSystem, bagContext);
        return bagContext;
    }

    // OFFSET 0x0203D2C4
    public static ushort BagContext_GetSelectedItem(BagContext bagContext)
    {
        ushort item = BagContext.BagContext_GetItem(bagContext);
        if (item != 0 && BagContext.BagContext_GetExitCode(bagContext) == 5)
        {
            ErrorHandling.GF_AssertFail();
        }
        return item;
    }

    // OFFSET 0x0203DE98
    private static bool FieldTask_NamingScreen(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        NamingScreenTaskEnv env = (NamingScreenTaskEnv)FieldTask.FieldTask_GetEnv(task)!;
        switch (env.State)
        {
            case 0:
                FieldTransition.FieldTransition_FinishMap(task);
                env.State++;
                break;
            case 1:
                FieldTask.FieldTask_RunApplication(task, ApplicationTemplate.At(NamingScreenTemplate), env.Args);
                env.State++;
                break;
            case 2:
                FieldTransition.FieldTransition_StartMap(task);
                env.State++;
                break;
            case 3:
            {
                NamingScreenArgs args = env.Args!;
                if (args.Type == NamingScreenTypePokemon)
                {
                    if (GameString.String_Compare(args.TextInputStr, env.OriginalName) == 0)
                    {
                        args.ReturnCode = 1;
                    }
                }
                else if (args.Type == NamingScreenTypeGroup)
                {
                    ushort[] name = GameString.String_GetData(args.TextInputStr);
                    if (RecordMixedRNG.RecordMixedRNG_DoesGroupNameExist(RecordMixedRNG.SaveData_GetRecordMixedRNG(fieldSystem.SaveData!), name))
                    {
                        args.ReturnCode = 2;
                    }
                }
                if (args.ReturnCode == 0)
                {
                    NamingScreen_ApplyName(task);
                }
                env.Result?.W16(0, args.ReturnCode);
                NamingScreenArgs.NamingScreenArgs_Free(args);
                GameString.String_Free(env.OriginalName);
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0203DF68
    private static void NamingScreen_ApplyName(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        NamingScreenTaskEnv env = (NamingScreenTaskEnv)FieldTask.FieldTask_GetEnv(task)!;
        NamingScreenArgs args = env.Args!;
        switch (args.Type)
        {
            case NamingScreenTypePlayer:
                TrainerInfo.TrainerInfo_SetName(PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!), args.NameInputRaw);
                break;
            case NamingScreenTypePokemon:
            {
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), env.PartySlot);
                Pokemon.Pokemon_SetValue(mon, MonDataNicknameAndFlag, args.NameInputRaw);
                break;
            }
            case NamingScreenTypeGroup:
                RecordMixedRNG.RecordMixedRNG_SetEntryName(RecordMixedRNG.SaveData_GetRecordMixedRNG(fieldSystem.SaveData!), 0, 0, args.TextInputStr!);
                break;
            case NamingScreenTypeTablet:
                MiscSaveBlock.MiscSaveBlock_SetTabletName(MiscSaveBlock.SaveData_MiscSaveBlock(fieldSystem.SaveData!), args.TextInputStr!);
                break;
        }
    }

    // OFFSET 0x0203DFE8
    public static void FieldTask_OpenNamingScreen(FieldTask task, int type, int speciesOrGender, int maxChars, int partySlot, ushort[]? initialName, BufferView? result)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        NamingScreenTaskEnv env = new();
        env.State = 0;
        env.PartySlot = partySlot;
        env.Result = result;
        env.Args = NamingScreenArgs.NamingScreenArgs_Init(HeapIDFieldTask, type, speciesOrGender, maxChars, PlayerSave.SaveData_GetOptions(fieldSystem.SaveData!));
        env.OriginalName = GameString.String_Init(NamingScreenNameSize, HeapIDFieldTask);
        switch (type)
        {
            case NamingScreenTypePokemon:
            {
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), env.PartySlot);
                env.Args.MonGender = (int)Pokemon.Pokemon_GetValue(mon, MonDataGender, null);
                env.Args.MonForm = (int)Pokemon.Pokemon_GetValue(mon, MonDataForm, null);
                if (initialName != null)
                {
                    GameString.String_CopyChars(env.OriginalName, initialName);
                }
                break;
            }
            case NamingScreenTypeGroup:
                GameString.String_CopyChars(env.OriginalName, initialName!);
                break;
            default:
                if (initialName != null)
                {
                    GameString.String_CopyChars(env.Args.TextInputStr, initialName);
                }
                break;
        }
        FieldTask.FieldTask_InitCall(task, FieldTask_NamingScreen, env);
    }

    // OFFSET 0x0203E09C
    public static void FieldSystem_OpenTrainerCaseApp(FieldSystem fieldSystem, TrainerCase trainerCase)
    {
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(TrainerCaseTemplate), trainerCase);
    }

    // OFFSET 0x0203D8DC
    public static void FieldSystem_LaunchOptionsMenuApp(FieldSystem fieldSystem, Options options)
    {
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(OptionsMenuTemplate), options);
    }

    // OFFSET 0x0203D8EC
    public static byte[] FieldSystem_OpenOptionsMenu(FieldSystem fieldSystem)
    {
        byte[] data = Heap.Heap_AllocAtEnd(HeapIDFieldTask, 2)!;
        FieldSystem_LaunchOptionsMenuApp(fieldSystem, PlayerSave.SaveData_GetOptions(FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!));
        return data;
    }
}
