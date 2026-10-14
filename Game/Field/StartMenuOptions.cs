using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed partial class StartMenu
{
    private const uint PartyMenuTemplate = 0x020F1E88;
    private const uint SummaryPagesAllAddress = 0x020EA02C;
    private const uint SummaryPagesMovesAddress = 0x020EA01C;
    private const uint BagPocketsAddress = 0x020EA020;
    private const int SummaryPagesAllCount = 9;
    private const int SummaryPagesMovesCount = 3;
    private const int BagPocketsCount = 9;
    private const int FadeTypeOut = 0;
    private const int HeapIDSave = 0x20;
    private const int HeapIDEvolution = 0x49;
    private const uint EvolutionHeapSize = 0x30000;
    private const int SaveInfoSize = 8;
    private const int SaveInfoResultOffset = 4;
    private const ushort ScriptSaveOverwriteBlocked = 0x7F2;
    private const ushort ScriptSave = 0x7D5;
    private const ushort ScriptRetireSafari = 0x2275;
    private const ushort ScriptRetire = 4;
    private const ushort BgmEvolution = 0x475;
    private const int HiddenLocationFullmoonIsland = 0;
    private const int HiddenLocationNewmoonIsland = 1;
    private const int HiddenLocationSpringPath = 2;
    private const int HiddenLocationSeabreakPath = 3;
    private const int SummaryDataTypeParty = 1;
    private const int SummaryModeNormal = 0;
    private const int SummaryModeSelectMove = 2;
    private const int PartyMenuModeField = 0;
    private const int PartyMenuModeUseItem = 5;
    private const int PartyMenuModeUseEvoItem = 6;
    private const int PartyMenuModeLearnedMove = 7;
    private const int PartyMenuModeLevelUpMove = 8;
    private const int PartyMenuModeGiveItem = 9;
    private const int PartyMenuModeGiveItemDone = 10;
    private const int PartyMenuModeMailFromBag = 11;
    private const int PartyMenuModeMailFromParty = 12;
    private const int PartyMenuModeUseItemDone = 16;
    private const int PartyResultSummary = 1;
    private const int PartyResultGiveItem = 3;
    private const int PartyResultLearnMove = 4;
    private const int PartyResultLevelUpMove = 5;
    private const int PartyResultWriteMail = 6;
    private const int PartyResultReadMail = 7;
    private const int PartyResultEvolveItem = 8;
    private const int PartyResultEvolveLevel = 9;
    private const int PartyResultOpenBag = 10;
    private const int PartyResultFirstFieldMove = 11;
    private const int PartyResultLastFieldMove = 23;
    private const int BagExitUseItem = 0;
    private const int BagExitBerryTag = 1;
    private const int BagExitGiveItem = 2;
    private const int BagExitGiveFromParty = 4;
    private const int MailGiveFromPartyMenu = 0;
    private const int MailGiveFromBag = 1;
    private const int MailReadFromPartyMenu = 2;
    private const int MailReadFromBag = 3;
    private const int EvolutionSourceLevel = 0;
    private const int EvolutionSourceItem = 3;
    private const int ItemParamFieldUseFunc = 6;
    private const int MonDataHeldItem = 6;
    private const int BerryCount = 0x40;
    private const int BerryPocket = 4;
    private const int BerryTagExtraEntries = 3;
    private const int EasyChatTypeUnionRoom = 4;
    private const int TileSize = 32;
    private const int TileCenter = 16;
    private const int JournalDataTypeLocation = 1;

    // OFFSET 0x0203B610
    private static void StartMenu_ApplicationStart(FieldTask task)
    {
        if (!ScreenFadeControl.IsScreenFadeDone())
        {
            return;
        }
        FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        StartMenu_Close(menu);
        Bg.Window_Remove(menu.PrimaryWindow);
        StartMenu_EraseBallCount(task);
        menu.Callback!(task);
        menu.State = StateApplicationRun;
    }

    // OFFSET 0x0203B64C
    private static void StartMenu_ApplicationRun(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        if (!FieldTask.FieldSystem_IsRunningApplication(fieldSystem))
        {
            menu.Callback!(task);
        }
    }

    // OFFSET 0x0203B674
    public static void StartMenu_SetCallback(StartMenu menu, FieldTaskFunc callback)
    {
        menu.Callback = callback;
        menu.State = StateApplicationRun;
    }

    // OFFSET 0x0203B680
    private static bool StartMenu_SelectPokedex(FieldTask task)
    {
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        FieldMap.FieldMap_FadeScreen(FadeTypeOut);
        menu.Callback = StartMenu_OpenPokedex;
        menu.State = StateApplicationStart;
        return true;
    }

    // OFFSET 0x0203B6A4
    private static bool StartMenu_OpenPokedex(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        PokedexOverlayArgs args = new();
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!);
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!);
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        args.Pokedex = pokedex;
        args.TrainerInfo = trainerInfo;
        args.TimeOfDay = FieldTime.FieldSystem_GetTimeOfDay(fieldSystem);
        args.FullmoonIslandVisible = SystemVars.SystemVars_CheckHiddenLocation(varsFlags, HiddenLocationFullmoonIsland) ? 1 : 0;
        args.NewmoonIslandVisible = SystemVars.SystemVars_CheckHiddenLocation(varsFlags, HiddenLocationNewmoonIsland) ? 1 : 0;
        args.SpringPathVisible = SystemVars.SystemVars_CheckHiddenLocation(varsFlags, HiddenLocationSpringPath) ? 1 : 0;
        args.SeabreakPathVisible = SystemVars.SystemVars_CheckHiddenLocation(varsFlags, HiddenLocationSeabreakPath) ? 1 : 0;
        args.PokedexMemory = fieldSystem.PokedexMemory;
        sub_0203E0AC(fieldSystem, args);
        menu.TaskData = args;
        menu.Callback = StartMenu_ExitPokedex;
        return false;
    }

    // OFFSET 0x0203B738
    private static bool StartMenu_ExitPokedex(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        FieldTask.FieldSystem_StartFieldMap(fieldSystem);
        if (menu.TaskData != null)
        {
            menu.TaskData = null;
        }
        menu.State = StateReinit;
        return false;
    }

    // OFFSET 0x0203B768
    private static bool StartMenu_SelectPokemon(FieldTask task)
    {
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        FieldMap.FieldMap_FadeScreen(FadeTypeOut);
        menu.Callback = StartMenu_OpenPartyMenu;
        menu.State = StateApplicationStart;
        return true;
    }

    // OFFSET 0x0203B78C
    private static bool StartMenu_OpenPartyMenu(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        menu.TaskData = sub_0203D390(fieldSystem, menu.FieldMoveContext, 0);
        menu.Callback = StartMenu_ExitPartyMenu;
        return false;
    }

    private static PokemonSummary NewPartySummary(FieldSystem fieldSystem, PartyMenu partyMenu)
    {
        PokemonSummary summary = new();
        summary.MonData = Party.SaveData_GetParty(fieldSystem.SaveData!);
        summary.Options = PlayerSave.SaveData_GetOptions(fieldSystem.SaveData!);
        summary.DataType = SummaryDataTypeParty;
        summary.MonIndex = partyMenu.SelectedMonSlot;
        return summary;
    }

    private static PartyMenu NewPartyMenu(FieldSystem fieldSystem, StartMenu menu)
    {
        PartyMenu partyMenu = new();
        partyMenu.Party = Party.SaveData_GetParty(fieldSystem.SaveData!);
        partyMenu.Bag = BagItems.SaveData_GetBag(fieldSystem.SaveData!);
        partyMenu.Mailbox = Mailbox.SaveData_GetMailbox(fieldSystem.SaveData!);
        partyMenu.Options = PlayerSave.SaveData_GetOptions(fieldSystem.SaveData!);
        partyMenu.FieldMoveContext = menu.FieldMoveContext;
        partyMenu.Type = 0;
        return partyMenu;
    }

    // OFFSET 0x0203B7C0
    public static bool StartMenu_ExitPartyMenu(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        PartyMenu partyMenu = (PartyMenu)menu.TaskData!;
        switch (partyMenu.MenuSelectionResult)
        {
            case PartyResultSummary:
            {
                PokemonSummary summary = NewPartySummary(fieldSystem, partyMenu);
                summary.MonMax = (byte)Party.Party_GetCurrentCount((Party)summary.MonData!);
                summary.Move = 0;
                summary.Mode = SummaryModeNormal;
                summary.SpecialRibbons = RibbonSaveData.SaveData_GetRibbons(fieldSystem.SaveData!);
                summary.DexMode = (uint)DexMode.SaveData_GetDexMode(fieldSystem.SaveData!);
                summary.ShowContest = PokemonSummaryShared.PokemonSummaryScreen_ShowContestData(fieldSystem.SaveData!);
                summary.ChatotCry = null;
                sub_0208D720(summary, RomData.Arm9(SummaryPagesAllAddress, SummaryPagesAllCount));
                sub_0208E9C0(summary, PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!));
                sub_0203D334(fieldSystem, summary);
                menu.TaskData = summary;
                StartMenu_SetCallback(menu, StartMenu_ExitSummary);
                break;
            }
            case PartyResultLearnMove:
            {
                PokemonSummary summary = NewMoveSummary(fieldSystem, partyMenu);
                StartMenuSummaryData data = new();
                data.Item = partyMenu.UsedItemID;
                data.LevelUpMoveIndex = 0;
                menu.AdditionalTaskData = data;
                menu.TaskData = summary;
                StartMenu_SetCallback(menu, StartMenu_ExitSummary);
                break;
            }
            case PartyResultLevelUpMove:
            {
                PokemonSummary summary = NewMoveSummary(fieldSystem, partyMenu);
                StartMenuSummaryData data = new();
                data.Item = 0;
                data.LevelUpMoveIndex = (ushort)partyMenu.LevelUpMoveIndex;
                menu.AdditionalTaskData = data;
                menu.TaskData = summary;
                StartMenu_SetCallback(menu, StartMenu_ExitSummary);
                break;
            }
            case PartyResultWriteMail:
            {
                byte mailType = Item.Item_MailNumber(partyMenu.UsedItemID);
                menu.TaskData = sub_0203D920(fieldSystem, 2, partyMenu.SelectedMonSlot, mailType, HeapIDFieldTask);
                if (partyMenu.Mode == PartyMenuModeGiveItemDone)
                {
                    menu.AdditionalTaskData = StartMenu_BuildMailData(partyMenu.UsedItemID, MailGiveFromPartyMenu, partyMenu.SelectedMonSlot);
                }
                else
                {
                    menu.AdditionalTaskData = StartMenu_BuildMailData(partyMenu.UsedItemID, MailGiveFromBag, partyMenu.SelectedMonSlot);
                }
                StartMenu_SetCallback(menu, StartMenu_ExitMail);
                break;
            }
            case PartyResultReadMail:
            {
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), partyMenu.SelectedMonSlot);
                menu.TaskData = sub_0203D984(fieldSystem, mon, HeapIDFieldTask);
                menu.AdditionalTaskData = StartMenu_BuildMailData(partyMenu.UsedItemID, MailReadFromPartyMenu, partyMenu.SelectedMonSlot);
                StartMenu_SetCallback(menu, StartMenu_ExitMail);
                break;
            }
            case PartyResultGiveItem:
            {
                StartMenuPartySlot slot = new();
                slot.Slot = partyMenu.SelectedMonSlot;
                menu.AdditionalTaskData = slot;
                Bag bag = BagItems.SaveData_GetBag(fieldSystem.SaveData!);
                PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!);
                menu.TaskData = BagItems.Bag_CreateContextWithPockets(bag, RomData.Arm9(BagPocketsAddress, BagPocketsCount), HeapIDFieldTask);
                sub_0207CB2C(menu.TaskData, fieldSystem.SaveData!, 1, fieldSystem.BagCursor);
                sub_0203D1E4(fieldSystem, menu.TaskData);
                StartMenu_SetCallback(menu, StartMenu_ExitBag);
                break;
            }
            case PartyResultEvolveItem:
            {
                StartMenuEvolutionData data = new();
                data.Method = partyMenu.UsedItemID;
                data.EvolutionSource = EvolutionSourceItem;
                data.PartySlot = partyMenu.SelectedMonSlot;
                data.TargetSpecies = partyMenu.EvoTargetSpecies;
                data.EvoType = partyMenu.EvoType;
                menu.TaskData = data;
                menu.State = StateEvolveInit;
                break;
            }
            case PartyResultEvolveLevel:
            {
                StartMenuEvolutionData data = new();
                data.Method = (ushort)MapHeader.MapHeader_GetMapEvolutionMethod((int)fieldSystem.Location!.U32(0));
                data.EvolutionSource = EvolutionSourceLevel;
                data.PartySlot = partyMenu.SelectedMonSlot;
                data.TargetSpecies = partyMenu.EvoTargetSpecies;
                data.EvoType = partyMenu.EvoType;
                menu.TaskData = data;
                menu.State = StateEvolveInit;
                break;
            }
            case PartyResultOpenBag:
                menu.TaskData = sub_0203D20C(fieldSystem, menu.ItemUseContext);
                StartMenu_SetCallback(menu, StartMenu_ExitBag);
                break;
            case >= PartyResultFirstFieldMove and <= PartyResultLastFieldMove:
            {
                FieldMoveTaskContext context = new();
                context.FieldMove = (ushort)(partyMenu.MenuSelectionResult - PartyResultFirstFieldMove);
                context.SelectedMonSlot = partyMenu.SelectedMonSlot;
                context.Task = task;
                FieldMoveTaskFunc function = sub_0207070C(0, context.FieldMove);
                function(context, menu.FieldMoveContext);
                break;
            }
            default:
                if (partyMenu.Mode == PartyMenuModeUseItem
                    || partyMenu.Mode == PartyMenuModeUseEvoItem
                    || partyMenu.Mode == PartyMenuModeLearnedMove
                    || partyMenu.Mode == PartyMenuModeUseItemDone
                    || partyMenu.Mode == PartyMenuModeLevelUpMove)
                {
                    menu.TaskData = sub_0203D20C(fieldSystem, menu.ItemUseContext);
                    if (partyMenu.SelectedMonSlot >= PartyMenu.MaxPartySize)
                    {
                        sub_0207CB70(menu.TaskData, 0);
                    }
                    else
                    {
                        sub_0207CB70(menu.TaskData, partyMenu.SelectedMonSlot);
                    }
                    StartMenu_SetCallback(menu, StartMenu_ExitBag);
                }
                else if (partyMenu.Mode == PartyMenuModeGiveItem)
                {
                    menu.TaskData = sub_0203D20C(fieldSystem, menu.ItemUseContext);
                    StartMenu_SetCallback(menu, StartMenu_ExitBag);
                }
                else
                {
                    FieldTask.FieldSystem_StartFieldMap(fieldSystem);
                    menu.State = StateReinit;
                }
                break;
        }
        return false;
    }

    private static PokemonSummary NewMoveSummary(FieldSystem fieldSystem, PartyMenu partyMenu)
    {
        PokemonSummary summary = NewPartySummary(fieldSystem, partyMenu);
        summary.MonMax = 1;
        summary.Move = partyMenu.LearnedMove;
        summary.Mode = SummaryModeSelectMove;
        summary.DexMode = (uint)DexMode.SaveData_GetDexMode(fieldSystem.SaveData!);
        summary.ShowContest = PokemonSummaryShared.PokemonSummaryScreen_ShowContestData(fieldSystem.SaveData!);
        summary.ChatotCry = null;
        sub_0208D720(summary, RomData.Arm9(SummaryPagesMovesAddress, SummaryPagesMovesCount));
        sub_0208E9C0(summary, PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!));
        sub_0203D334(fieldSystem, summary);
        return summary;
    }

    // OFFSET 0x0203BBF4
    private static bool StartMenu_SelectBag(FieldTask task)
    {
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        FieldMap.FieldMap_FadeScreen(FadeTypeOut);
        menu.Callback = StartMenu_Bag;
        menu.State = StateApplicationStart;
        return true;
    }

    // OFFSET 0x0203BC18
    private static bool StartMenu_Bag(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        menu.TaskData = sub_0203D20C(fieldSystem, menu.ItemUseContext);
        sub_0207CB70(menu.TaskData, 0);
        menu.Callback = StartMenu_ExitBag;
        ov5_021E2064(fieldSystem);
        return false;
    }

    // OFFSET 0x0203BC5C
    private static bool StartMenu_ExitBag(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        object? bagContext = menu.TaskData;
        switch (sub_0207CB9C(bagContext))
        {
            case BagExitUseItem:
            {
                ItemMenuUseContext context = new();
                context.Item = sub_0207CB94(bagContext);
                context.SelectedMonSlot = sub_0207CBA4(bagContext);
                context.Task = task;
                uint index = (uint)Item.Item_LoadParam(context.Item, ItemParamFieldUseFunc, HeapIDFieldTask);
                ItemMenuUseFunc function = sub_020683F4(0, (ushort)index);
                function(context, menu.ItemUseContext);
                break;
            }
            case BagExitBerryTag:
                StartMenu_ShowBerryTag(task, sub_0207CB94(bagContext));
                break;
            case BagExitGiveItem:
            {
                PartyMenu partyMenu = NewPartyMenu(fieldSystem, menu);
                partyMenu.Mode = PartyMenuModeGiveItem;
                partyMenu.UsedItemID = sub_0207CB94(bagContext);
                partyMenu.FieldSystem = fieldSystem;
                FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(PartyMenuTemplate), partyMenu);
                menu.TaskData = partyMenu;
                StartMenu_SetCallback(menu, StartMenu_ExitPartyMenu);
                break;
            }
            case BagExitGiveFromParty:
            {
                Party party = Party.SaveData_GetParty(fieldSystem.SaveData!);
                int slot = ((StartMenuPartySlot)menu.AdditionalTaskData!).Slot;
                ushort item = sub_0207CB94(bagContext);
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, slot);
                menu.AdditionalTaskData = null;
                if (Item.Item_IsMail(item) == 1 && Pokemon.Pokemon_GetValue(mon, MonDataHeldItem, null) == 0)
                {
                    byte mailType = Item.Item_MailNumber(item);
                    menu.TaskData = sub_0203D920(fieldSystem, 2, (byte)slot, mailType, HeapIDFieldTask);
                    menu.AdditionalTaskData = StartMenu_BuildMailData(item, MailGiveFromPartyMenu, (byte)slot);
                    StartMenu_SetCallback(menu, StartMenu_ExitMail);
                }
                else
                {
                    PartyMenu partyMenu = new();
                    partyMenu.Party = party;
                    partyMenu.Bag = BagItems.SaveData_GetBag(fieldSystem.SaveData!);
                    partyMenu.Mailbox = Mailbox.SaveData_GetMailbox(fieldSystem.SaveData!);
                    partyMenu.Options = PlayerSave.SaveData_GetOptions(fieldSystem.SaveData!);
                    partyMenu.FieldMoveContext = menu.FieldMoveContext;
                    partyMenu.Type = 0;
                    partyMenu.UsedItemID = sub_0207CB94(bagContext);
                    partyMenu.SelectedMonSlot = (byte)slot;
                    partyMenu.FieldSystem = fieldSystem;
                    if (partyMenu.UsedItemID == 0)
                    {
                        partyMenu.Mode = PartyMenuModeField;
                    }
                    else
                    {
                        partyMenu.Mode = PartyMenuModeGiveItemDone;
                    }
                    FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(PartyMenuTemplate), partyMenu);
                    menu.TaskData = partyMenu;
                    StartMenu_SetCallback(menu, StartMenu_ExitPartyMenu);
                }
                break;
            }
            default:
                FieldTask.FieldSystem_StartFieldMap(fieldSystem);
                menu.State = StateReinit;
                break;
        }
        return false;
    }

    // OFFSET 0x0203BE8C
    private static bool StartMenu_SelectTrainerCase(FieldTask task)
    {
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        FieldMap.FieldMap_FadeScreen(FadeTypeOut);
        menu.Callback = StartMenu_TrainerCase;
        menu.State = StateApplicationStart;
        return true;
    }

    // OFFSET 0x0203BEB0
    private static bool StartMenu_TrainerCase(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        menu.TaskData = sub_02071F04(HeapIDFieldTask);
        sub_02071D40(1, 1, 0, 0xFF, fieldSystem, menu.TaskData);
        sub_0203E09C(fieldSystem, menu.TaskData);
        menu.Callback = StartMenu_ExitTrainerCase;
        return false;
    }

    // OFFSET 0x0203BF00
    private static bool StartMenu_ExitTrainerCase(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        sub_020721D4(fieldSystem, menu.TaskData);
        sub_02071F20(menu.TaskData);
        FieldTask.FieldSystem_StartFieldMap(fieldSystem);
        menu.State = StateReinit;
        return false;
    }

    // OFFSET 0x0203BF38
    private static bool StartMenu_SelectSave(FieldTask task)
    {
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        StartMenu_Close(menu);
        RenderWindow.Window_EraseStandardFrame(menu.PrimaryWindow, 1);
        Bg.Bg_ScheduleTilemapTransfer(menu.PrimaryWindow.BgConfig, menu.PrimaryWindow.BgLayer);
        Bg.Window_Remove(menu.PrimaryWindow);
        StartMenu_EraseBallCount(task);
        menu.State = StateSave;
        return true;
    }

    // OFFSET 0x0203BF6C
    private static void StartMenu_Save(FieldTask task)
    {
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        if (SaveData.SaveData_OverwriteCheck(fieldSystem.SaveData!))
        {
            ScriptManager.ScriptManager_Start(task, ScriptSaveOverwriteBlocked, null, null);
        }
        else
        {
            byte[] saveInfo = Heap.Heap_Alloc(HeapIDSave, SaveInfoSize)!;
            menu.TaskData = saveInfo;
            BufferView result = new(saveInfo, SaveInfoResultOffset);
            result.W16(0, 0);
            ScriptManager.ScriptManager_Start(task, ScriptSave, null, result);
        }
        menu.State = StateSaveWait;
    }

    // OFFSET 0x0203BFC0
    private static void StartMenu_SaveWait(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        if (SaveData.SaveData_OverwriteCheck(fieldSystem.SaveData!))
        {
            menu.State = StateInit;
            return;
        }
        byte[] saveInfo = (byte[])menu.TaskData!;
        if (new BufferView(saveInfo, SaveInfoResultOffset).U16(0) == 0)
        {
            menu.State = StateInit;
        }
        else
        {
            menu.State = StateSaved;
        }
        Heap.Heap_Free(saveInfo);
    }

    // OFFSET 0x0203C000
    private static bool StartMenu_SelectOptions(FieldTask task)
    {
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        FieldMap.FieldMap_FadeScreen(FadeTypeOut);
        menu.Callback = StartMenu_Options;
        menu.State = StateApplicationStart;
        return true;
    }

    // OFFSET 0x0203C024
    private static bool StartMenu_Options(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        menu.TaskData = sub_0203D8EC(fieldSystem);
        menu.Callback = StartMenu_ExitOptions;
        return false;
    }

    // OFFSET 0x0203C050
    private static bool StartMenu_ExitOptions(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        menu.TaskData = null;
        FieldTask.FieldSystem_StartFieldMap(fieldSystem);
        menu.State = StateReinit;
        return false;
    }

    // OFFSET 0x0203C07C
    private static bool StartMenu_SelectChat(FieldTask task)
    {
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        FieldMap.FieldMap_FadeScreen(FadeTypeOut);
        menu.Callback = StartMenu_Chat;
        menu.State = StateApplicationStart;
        return true;
    }

    // OFFSET 0x0203C0A0
    private static bool StartMenu_Chat(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        menu.TaskData = sub_0209747C(2, 0, fieldSystem.SaveData!, HeapIDFieldTask);
        EasyChatSentence sentence = new();
        EasyChatSentence.EasyChatSentence_InitWithType(sentence, EasyChatTypeUnionRoom);
        sub_02097500(menu.TaskData, sentence);
        sub_0203D874(fieldSystem, menu.TaskData);
        menu.Callback = StartMenu_ExitChat;
        return false;
    }

    // OFFSET 0x0203C0F8
    private static bool StartMenu_ExitChat(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        if (sub_02097528(menu.TaskData) == 0)
        {
            EasyChatSentence sentence = new();
            sub_02097540(menu.TaskData, sentence);
            if (WirelessManager.WirelessMan_IsInitialized())
            {
                sub_0205C12C(sentence);
                sub_0205C010(fieldSystem.CommUnionRoom, sentence);
            }
            menu.State = StateFadeAndEnd;
        }
        else
        {
            menu.State = StateReinit;
        }
        sub_020974EC(menu.TaskData);
        FieldTask.FieldSystem_StartFieldMap(fieldSystem);
        sub_0205C2B0(fieldSystem.Unknown80);
        return false;
    }

    // OFFSET 0x0203C164
    private static bool StartMenu_SelectRetire(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        StartMenu_Close(menu);
        RenderWindow.Window_EraseStandardFrame(menu.PrimaryWindow, 1);
        Bg.Bg_ScheduleTilemapTransfer(menu.PrimaryWindow.BgConfig, menu.PrimaryWindow.BgLayer);
        Bg.Window_Remove(menu.PrimaryWindow);
        StartMenu_EraseBallCount(task);
        if (SystemFlags.SystemFlag_CheckSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)) == true)
        {
            ScriptManager.ScriptManager_Change(task, ScriptRetireSafari, null);
        }
        else
        {
            ScriptManager.ScriptManager_Change(task, ScriptRetire, null);
        }
        return false;
    }

    // OFFSET 0x0203C1C8
    private static bool StartMenu_ExitSummary(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        PokemonSummary summary = (PokemonSummary)menu.TaskData!;
        if (summary.Mode == SummaryModeSelectMove)
        {
            StartMenuSummaryData data = (StartMenuSummaryData)menu.AdditionalTaskData!;
            PartyMenu partyMenu = NewPartyMenu(fieldSystem, menu);
            partyMenu.FieldSystem = fieldSystem;
            if (data.Item != 0)
            {
                partyMenu.Mode = PartyMenuModeLearnedMove;
                partyMenu.LevelUpMoveIndex = 0;
            }
            else
            {
                partyMenu.Mode = PartyMenuModeLevelUpMove;
                partyMenu.LevelUpMoveIndex = data.LevelUpMoveIndex;
            }
            partyMenu.UsedItemID = data.Item;
            partyMenu.SelectedMonSlot = summary.MonIndex;
            partyMenu.LearnedMove = summary.Move;
            partyMenu.SelectedMoveSlot = summary.SelectedMoveSlot;
            FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(PartyMenuTemplate), partyMenu);
            menu.AdditionalTaskData = null;
            menu.TaskData = partyMenu;
            StartMenu_SetCallback(menu, StartMenu_ExitPartyMenu);
        }
        else
        {
            menu.TaskData = sub_0203D390(fieldSystem, menu.FieldMoveContext, summary.MonIndex);
            StartMenu_SetCallback(menu, StartMenu_ExitPartyMenu);
        }
        return false;
    }

    // OFFSET 0x0203C2D8
    private static void StartMenu_ShowBerryTag(FieldTask task, ushort item)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        menu.TaskData = sub_020972FC(HeapIDFieldTask);
        Bag bag = BagItems.SaveData_GetBag(fieldSystem.SaveData!);
        sub_02097320(menu.TaskData, (byte)item, 1);
        byte count = 0;
        for (byte i = 0; i < BerryCount; i++)
        {
            ushort berry = Item.Item_ForBerryNumber(i);
            if (BagItems.Bag_CanRemoveItem(bag, berry, 1, HeapIDFieldTask) == true)
            {
                sub_02097320(menu.TaskData, (byte)berry, 0);
                count++;
            }
        }
        BagCursor.BagCursor_GetFieldPocketPosition(fieldSystem.BagCursor!, BerryPocket, out byte index, out byte scroll);
        sub_0209733C(menu.TaskData, scroll, index, (byte)(count + BerryTagExtraEntries));
        sub_0203D2E4(fieldSystem, menu.TaskData);
        StartMenu_SetCallback(menu, StartMenu_ExitBerryTag);
    }

    // OFFSET 0x0203C390
    private static bool StartMenu_ExitBerryTag(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        sub_02097390(menu.TaskData, out byte scroll, out byte index);
        BagCursor.BagCursor_SetFieldPocketPosition(fieldSystem.BagCursor!, BerryPocket, index, scroll);
        menu.TaskData = sub_0203D20C(fieldSystem, menu.ItemUseContext);
        StartMenu_SetCallback(menu, StartMenu_ExitBag);
        return false;
    }

    // OFFSET 0x0203C3F4
    public static bool StartMenu_ExitTownMap(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        menu.TaskData = sub_0203D20C(fieldSystem, menu.ItemUseContext);
        StartMenu_SetCallback(menu, StartMenu_ExitBag);
        return false;
    }

    // OFFSET 0x0203C434
    public static bool StartMenu_FlyDestinationSelected(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        int slot = ((StartMenuPartySlot)menu.AdditionalTaskData!).Slot;
        menu.AdditionalTaskData = null;
        TownMapContext townMap = (TownMapContext)menu.TaskData!;
        if (townMap.FlyLocationSelected == 0)
        {
            menu.TaskData = sub_0203D390(fieldSystem, menu.FieldMoveContext, (byte)slot);
            StartMenu_SetCallback(menu, StartMenu_ExitPartyMenu);
        }
        else
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(fieldSystem.SaveData!), slot);
            short x = (short)(townMap.FlyLocationX * TileSize + TileCenter);
            short z = (short)(townMap.FlyLocationZ * TileSize + TileCenter);
            object? flyTask = sub_0207064C(HeapIDFieldTask, fieldSystem, mon, (ushort)townMap.FlyLocationMapHeader, x, z);
            JournalLocationEvent journalEvent = Journal.JournalEntry_CreateEventUsedMove(1, (ushort)townMap.FlyLocationMapHeader, HeapIDFieldTask);
            Journal.JournalEntry_SaveData(fieldSystem.JournalEntry, journalEvent, JournalDataTypeLocation);
            FieldTask.FieldSystem_StartFieldMap(fieldSystem);
            menu.Callback = sub_02070680;
            menu.TaskData = flyTask;
            menu.State = StateNewTask;
        }
        return false;
    }

    // OFFSET 0x0203C50C
    public static bool StartMenu_ExitJournal(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        menu.TaskData = sub_0203D20C(fieldSystem, menu.ItemUseContext);
        StartMenu_SetCallback(menu, StartMenu_ExitBag);
        return false;
    }

    // OFFSET 0x0203C540
    public static StartMenuMailData StartMenu_BuildMailData(ushort item, byte usageType, byte partySlot)
    {
        StartMenuMailData data = new();
        data.Item = item;
        data.PartySlot = partySlot;
        data.UsageType = usageType;
        return data;
    }

    // OFFSET 0x0203C558
    public static bool StartMenu_ExitMail(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        StartMenuMailData data = (StartMenuMailData)menu.AdditionalTaskData!;
        switch (data.UsageType)
        {
            case MailReadFromBag:
                sub_02097770(menu.TaskData);
                menu.TaskData = sub_0203D20C(fieldSystem, menu.ItemUseContext);
                StartMenu_SetCallback(menu, StartMenu_ExitBag);
                break;
            case MailReadFromPartyMenu:
                sub_02097770(menu.TaskData);
                menu.TaskData = sub_0203D390(fieldSystem, menu.FieldMoveContext, data.PartySlot);
                StartMenu_SetCallback(menu, StartMenu_ExitPartyMenu);
                break;
            case MailGiveFromPartyMenu:
                if (sub_02097728(menu.TaskData) == 1)
                {
                    StartMenu_ProcessGivenMail(fieldSystem, menu, PartyMenuModeMailFromParty);
                }
                else
                {
                    sub_02097770(menu.TaskData);
                    menu.TaskData = sub_0203D390(fieldSystem, menu.FieldMoveContext, data.PartySlot);
                    StartMenu_SetCallback(menu, StartMenu_ExitPartyMenu);
                }
                break;
            case MailGiveFromBag:
                if (sub_02097728(menu.TaskData) == 1)
                {
                    StartMenu_ProcessGivenMail(fieldSystem, menu, PartyMenuModeMailFromBag);
                }
                else
                {
                    sub_02097770(menu.TaskData);
                    menu.TaskData = sub_0203D20C(fieldSystem, menu.ItemUseContext);
                    StartMenu_SetCallback(menu, StartMenu_ExitBag);
                }
                break;
        }
        menu.AdditionalTaskData = null;
        return false;
    }

    // OFFSET 0x0203C668
    private static void StartMenu_ProcessGivenMail(FieldSystem fieldSystem, StartMenu menu, byte mode)
    {
        StartMenuMailData data = (StartMenuMailData)menu.AdditionalTaskData!;
        PartyMenu partyMenu = NewPartyMenu(fieldSystem, menu);
        partyMenu.UsedItemID = data.Item;
        partyMenu.SelectedMonSlot = data.PartySlot;
        partyMenu.Mode = mode;
        partyMenu.FieldSystem = fieldSystem;
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, data.PartySlot);
        sub_02097750(menu.TaskData, mon);
        sub_02097770(menu.TaskData);
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(PartyMenuTemplate), partyMenu);
        menu.TaskData = partyMenu;
        StartMenu_SetCallback(menu, StartMenu_ExitPartyMenu);
    }

    // OFFSET 0x0203C710
    public static bool StartMenu_ExitPoffinCase(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        sub_02098AF0(menu.TaskData);
        menu.TaskData = sub_0203D20C(fieldSystem, menu.ItemUseContext);
        StartMenu_SetCallback(menu, StartMenu_ExitBag);
        return false;
    }

    // OFFSET 0x0203C750
    public static bool StartMenu_ExitPalPad(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        menu.TaskData = sub_0203D20C(fieldSystem, menu.ItemUseContext);
        StartMenu_SetCallback(menu, StartMenu_ExitBag);
        return false;
    }

    // OFFSET 0x0203C784
    public static bool StartMenu_ExitVsRecorder(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        menu.TaskData = sub_0203D20C(fieldSystem, menu.ItemUseContext);
        StartMenu_SetCallback(menu, StartMenu_ExitBag);
        return false;
    }

    // OFFSET 0x0203C7B8
    private static void StartMenu_EvolveInit(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        StartMenuEvolutionData data = (StartMenuEvolutionData)menu.TaskData!;
        SoundPlayback.Sound_StopWaveOutAndSequences();
        Heap.Heap_Create(3, HeapIDEvolution, EvolutionHeapSize);
        Party party = Party.SaveData_GetParty(fieldSystem.SaveData!);
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, data.PartySlot);
        int flags;
        if (data.EvolutionSource == EvolutionSourceLevel)
        {
            flags = 1;
        }
        else
        {
            flags = 0;
        }
        Options options = PlayerSave.SaveData_GetOptions(fieldSystem.SaveData!);
        int showContest = PokemonSummaryShared.PokemonSummaryScreen_ShowContestData(fieldSystem.SaveData!);
        Pokedex pokedex = Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!);
        Bag bag = BagItems.SaveData_GetBag(fieldSystem.SaveData!);
        GameRecords records = GameRecords.SaveData_GetGameRecords(fieldSystem.SaveData!);
        object poketch = FieldPoketch.SaveData_GetPoketch(fieldSystem.SaveData!);
        object? evolution = sub_0207AE68(party, mon, data.TargetSpecies, options, showContest, pokedex, bag, records, poketch, data.EvoType, flags, HeapIDEvolution);
        StartMenuPartySlot slot = new();
        slot.Slot = data.PartySlot;
        menu.AdditionalTaskData = slot;
        menu.TaskData = evolution;
        menu.State = StateEvolve;
    }

    // OFFSET 0x0203C8CC
    private static void StartMenu_Evolve(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        if (sub_0207B0D0(menu.TaskData) != 1)
        {
            return;
        }
        sub_0207B0E0(menu.TaskData);
        Heap.Heap_Destroy(HeapIDEvolution);
        SoundPlayback.Sound_StopBGM(BgmEvolution, 0);
        Sound.Sound_SetScene(0);
        FieldBgm.FieldBGM_PlayEffectiveForMapHeader(fieldSystem, (int)fieldSystem.Location!.U32(0));
        menu.TaskData = sub_0203D20C(fieldSystem, menu.ItemUseContext);
        sub_0207CB70(menu.TaskData, (byte)((StartMenuPartySlot)menu.AdditionalTaskData!).Slot);
        menu.AdditionalTaskData = null;
        StartMenu_SetCallback(menu, StartMenu_ExitBag);
    }
}

public static class BattleTowerSalon
{
    private const int MapHeaderBattleTowerSalon = 0x1ED;

    // OFFSET 0x0206C0D0
    public static bool FieldSystem_IsInBattleTowerSalon(FieldSystem fieldSystem)
    {
        if (fieldSystem.Location!.U32(0) == MapHeaderBattleTowerSalon)
        {
            return true;
        }
        return false;
    }
}
