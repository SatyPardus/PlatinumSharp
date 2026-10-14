namespace PlatinumSharp.Game;

public static partial class BattleSystems
{
    // OFFSET 0x0223F1F8
    public static int BattleSystem_GetVisitedContestHall(BattleSystem battleSys)
    {
        return battleSys.VisitedContestHall;
    }
}

public static partial class BattleParties
{
    public const int ScreenPokemonParty = 0;
    public const int ScreenSelectPokemon = 1;
    public const int ScreenPokemonSummary = 2;
    public const int ScreenPokemonMoves = 3;
    public const int ScreenMoveSummary = 4;
    public const int ScreenRestoreMovePP = 5;
    public const int ScreenLearnMove = 6;
    public const int ScreenConfirmLearnMove = 7;
    public const int ScreenLearnMoveContestStats = 8;
    public const int ScreenConfirmLearnMoveContestStats = 9;

    public const int ModeSelectPokemon = 0;
    public const int ModeSelectPokemonNoCancel = 1;
    public const int ModeUseItem = 2;
    public const int ModeLearnMove = 3;

    public const int PartySlotSelectableInBattle = 1;
    public const int PartySlotSelectableNotInBattle = 2;
    public const int MoveToLearnSlot = 4;
    public const int CancelSlot = 6;

    private const int StateInitialize = 0;
    private const int StatePartyPokemonScreen = 1;
    private const int StateSelectPokemonScreen = 2;
    private const int StatePokemonSummaryScreen = 3;
    private const int StatePokemonMovesScreen = 4;
    private const int StateMoveSummaryScreen = 5;
    private const int StateSetupPartyPokemonScreen = 6;
    private const int StateSetupSelectPokemonScreen = 7;
    private const int StateSetupPokemonSummaryScreen = 8;
    private const int StateSetupPokemonMovesScreen = 9;
    private const int StateSetupMoveSummaryScreen = 10;
    private const int StateSetupLearnMoveScreen = 11;
    private const int StateSetupConfirmLearnMoveScreen = 12;
    private const int StateSetupRestoreMovePPScreen = 13;
    private const int StateRefreshPokemonDetailsScreens = 14;
    private const int StateDisplayCantSwitchMessage = 15;
    private const int StateClearErrorMessage = 16;
    private const int StateAwaitingTextFinish = 17;
    private const int StateAwaitingInput = 18;
    private const int StateLearnMoveScreen = 19;
    private const int StateConfirmLearnMoveScreen = 20;
    private const int StateRestoreMovePPScreen = 21;
    private const int StateScreenTransition = 22;
    private const int StateUseRestorationItem = 23;
    private const int StateUseAllMovePPRestorationItem = 24;
    private const int StateExit = 25;
    private const int StateFinishTask = 26;

    private const int SeqSelect = 0x5DD;
    private const int SeqRestore = 0x5EC;
    private const int NarcBattlePartyGraphics = 0x48;
    private const int NarcWindowGraphics = 0x26;
    private const int NarcObjPalettes = 0xE;
    private const int FontSubscreen = 2;
    private const int PaletteBufferSubBg = 1;
    private const int ItemParamRestorePPOne = 0x24;
    private const int ItemParamRestorePPAll = 0x25;
    private const int ItemParamPartyUseOutside = 0x17;
    private const int ItemFluffyTail = 0x41;
    private const int ItemPokeDoll = 0x42;
    private const int ItemPokeToy = 0x43;
    private const int MsgCantSwitchPartner = 0x50;
    private const int MsgCantSwitchFainted = 0x4D;
    private const int MsgCantSwitchInBattle = 0x4C;
    private const int MsgCantSwitchEgg = 0x4F;
    private const int MsgCantSwitchAlreadySelected = 0x5D;
    private const int MsgCantSwitchLearningMove = 0x4E;
    private const int MsgItemNoEffect = 0x51;
    private const int TileXPBarFull = 0x1E;
    private const int TileXPBarBase = 0x16;
    private const int TileAppealEmpty = 0x125;
    private const int TileAppealFull = 0x140;
    private const int BattleTypeLinkTag1 = 0x4A;
    private const int BattleTypeLinkTag2 = 0x4B;
    private const uint BattleTypePartnerMask = 0x12;
    private const uint BattleTypeTwoVsTwoMask = 8;
    private const int PadButtonA = 1;
    private const int PadButtonB = 2;

    private const uint GraphicsModesAddress = 0x02228D04;
    private const uint BgTemplateLayer7 = 0x02228D7C;
    private const uint BgTemplateLayer6 = 0x02228DB4;
    private const uint BgTemplateLayer5 = 0x02228DD0;
    private const uint BgTemplateLayer4 = 0x02228D98;
    private const uint PartyPokemonTouchRects = 0x02228DEC;
    private const uint SelectPokemonTouchRects = 0x02228D24;
    private const uint PokemonSummaryTouchRects = 0x02228D38;
    private const uint PokemonMovesTouchRects = 0x02228E2C;
    private const uint MoveSummaryTouchRects = 0x02228D4C;
    private const uint LearnMoveTouchRects = 0x02228E0C;
    private const uint ConfirmLearnMoveTouchRects = 0x02228D14;
    private const uint RestoreMovePPTouchRects = 0x02228D64;
    private const uint DoubleBattlePartyOrder = 0x02228CFC;
    private const uint ScreenBackgroundMembers = 0x02228E50;

    private static int Index(BattleParty battleParty)
    {
        return battleParty.Context!.SelectedPartyIndex;
    }

    // OFFSET 0x0221FC20
    public static void BattlePartyTask_Start(BattlePartyContext context)
    {
        if (context.SelectedPartyIndex > 5)
        {
            context.SelectedPartyIndex = 0;
        }
        SysTask task = SysTaskExtensions.SysTask_StartAndAllocateParam<BattleParty>(BattlePartyTask_Tick, BattleParty.Size, 0, context.HeapID)!;
        BattleParty battleParty = (BattleParty)SysTaskManager.SysTask_GetParam(task)!;
        battleParty.Context = context;
        battleParty.Background = BattleSystems.BattleSystem_GetBgConfig(context.BattleSys!);
        battleParty.Palette = BattleSystems.BattleSystem_GetPaletteData(context.BattleSys!);
        battleParty.CurrentState = StateInitialize;
        battleParty.PartySlotLearningMove = context.SelectedPartyIndex;
        battleParty.HasVisitedContestHall = (byte)(BattleSystems.BattleSystem_GetVisitedContestHall(context.BattleSys!) & 0xF);
        battleParty.BattlerSlot = (byte)BattleSystems.BattleSystem_BattlerSlot(context.BattleSys!, context.Battler);
    }

    // OFFSET 0x0221FCAC
    private static void BattlePartyTask_Tick(SysTask task, object? param)
    {
        BattleParty battleParty = (BattleParty)param!;
        switch (battleParty.CurrentState)
        {
            case StateInitialize:
                battleParty.CurrentState = (byte)BattlePartyTask_Initialize(battleParty);
                break;
            case StatePartyPokemonScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_PartyPokemonScreen(battleParty);
                break;
            case StateSelectPokemonScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_SelectPokemonScreen(battleParty);
                break;
            case StatePokemonSummaryScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_PokemonSummaryScreen(battleParty);
                break;
            case StatePokemonMovesScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_PokemonMovesScreen(battleParty);
                break;
            case StateMoveSummaryScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_MoveSummaryScreen(battleParty);
                break;
            case StateSetupPartyPokemonScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_SetupPartyPokemonScreen(battleParty);
                break;
            case StateSetupSelectPokemonScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_SetupSelectPokemonScreen(battleParty);
                break;
            case StateSetupPokemonSummaryScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_SetupPokemonSummaryScreen(battleParty);
                break;
            case StateSetupPokemonMovesScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_SetupPokemonMovesScreen(battleParty);
                break;
            case StateSetupMoveSummaryScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_SetupMoveSummaryScreen(battleParty);
                break;
            case StateSetupLearnMoveScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_SetupLearnMoveScreen(battleParty);
                break;
            case StateSetupConfirmLearnMoveScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_SetupConfirmLearnMoveScreen(battleParty);
                break;
            case StateSetupRestoreMovePPScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_SetupRestoreMovePPScreen(battleParty);
                break;
            case StateRefreshPokemonDetailsScreens:
                battleParty.CurrentState = (byte)BattlePartyTask_RefreshPokemonDetailsScreens(battleParty);
                break;
            case StateDisplayCantSwitchMessage:
                battleParty.CurrentState = (byte)BattlePartyTask_DisplayCantSwitchMessage(battleParty);
                break;
            case StateClearErrorMessage:
                battleParty.CurrentState = (byte)BattlePartyTask_ClearErrorMessage(battleParty);
                break;
            case StateAwaitingTextFinish:
                battleParty.CurrentState = (byte)BattlePartyTask_AwaitingTextFinish(battleParty);
                break;
            case StateAwaitingInput:
                battleParty.CurrentState = (byte)BattlePartyTask_AwaitingInput(battleParty);
                break;
            case StateLearnMoveScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_LearnMoveScreen(battleParty);
                break;
            case StateConfirmLearnMoveScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_ConfirmLearnMoveScreen(battleParty);
                break;
            case StateRestoreMovePPScreen:
                battleParty.CurrentState = (byte)BattlePartyTask_RestoreMovePPScreen(battleParty);
                break;
            case StateScreenTransition:
                battleParty.CurrentState = (byte)BattlePartyTask_ScreenTransition(battleParty);
                break;
            case StateUseRestorationItem:
                battleParty.CurrentState = (byte)BattlePartyTask_UseRestorationItem(battleParty);
                break;
            case StateUseAllMovePPRestorationItem:
                battleParty.CurrentState = (byte)BattlePartyTask_UseAllMovePPRestorationItem(battleParty);
                break;
            case StateExit:
                battleParty.CurrentState = (byte)BattlePartyTask_Exit(battleParty);
                break;
            case StateFinishTask:
                if (BattlePartyTask_FinishTask(task, battleParty) == 1)
                {
                    return;
                }
                break;
        }
        BattlePartySprites_TickPartyPokemonSpriteAnimations(battleParty);
        SpriteSystem.SpriteSystem_DrawSprites(battleParty.SpriteManager);
        BattlePartyButtons_Tick(battleParty);
    }

    // OFFSET 0x0221FE5C
    private static int BattlePartyTask_Initialize(BattleParty battleParty)
    {
        int nextState;
        Video.SetBlendControl(1, 0);
        if (battleParty.Context!.BattlePartyMode == ModeLearnMove)
        {
            battleParty.CurrentScreen = ScreenLearnMove;
            nextState = StateLearnMoveScreen;
        }
        else
        {
            battleParty.CurrentScreen = ScreenPokemonParty;
            nextState = StatePartyPokemonScreen;
        }
        battleParty.Cursor = BattleSubMenuCursor.MakeBattleSubMenuCursor(battleParty.Context.HeapID);
        InitialisePartyPokemon(battleParty);
        InitializeBackground(battleParty);
        LoadGraphicsData(battleParty);
        InitializeMessageLoader(battleParty);
        Font.Font_InitManager(FontSubscreen, battleParty.Context.HeapID);
        DrawScreenBackground(battleParty, battleParty.CurrentScreen);
        BattlePartyButtons_InitializeButtons(battleParty, battleParty.CurrentScreen);
        BattlePartyButtons_LoadScreenPaletteData(battleParty, battleParty.CurrentScreen);
        BattlePartySprites_InitializeSprites(battleParty);
        BattlePartySprites_SetupScreen(battleParty, battleParty.CurrentScreen);
        BattlePartyText_InitializeWindows(battleParty);
        BattlePartyText_ChangeScreen(battleParty, battleParty.CurrentScreen);
        if (battleParty.Context.IsCursorEnabled != 0)
        {
            BattleSubMenuCursor.SetBattleSubMenuCursorVisibility(battleParty.Cursor, 1);
        }
        if (battleParty.CurrentScreen == ScreenPokemonParty && BattlePartyTask_CheckIfSwitchingWithPartnersPokemon(battleParty, 0) == 1)
        {
            battleParty.Context.SelectedPartyIndex = 1;
        }
        BattlePartySprites_SetupCursor(battleParty, battleParty.CurrentScreen);
        DrawXPBar(battleParty, battleParty.CurrentScreen);
        PaletteData.PaletteData_StartFade(battleParty.Palette!, 0xA, 0xFFFF, -8, 0x10, 0, 0);
        return nextState;
    }

    // OFFSET 0x0221FF60
    private static int BattlePartyTask_PartyPokemonScreen(BattleParty battleParty)
    {
        if (PaletteData.PaletteData_GetSelectedBuffersMask(battleParty.Palette!) != 0)
        {
            return StatePartyPokemonScreen;
        }
        if (CheckPartyPokemonScreenButtonPressed(battleParty) != 1)
        {
            return StatePartyPokemonScreen;
        }
        BattlePartyContext context = battleParty.Context!;
        if (context.SelectedPartyIndex == CancelSlot)
        {
            if (context.BattlePartyMode == ModeSelectPokemonNoCancel)
            {
                return StatePartyPokemonScreen;
            }
            SoundPlayback.Sound_PlayEffect(SeqSelect);
            BattlePartyButtons_PressButton(battleParty, 6);
            return StateExit;
        }
        SoundPlayback.Sound_PlayEffect(SeqSelect);
        BattlePartyButtons_PressButton(battleParty, context.SelectedPartyIndex);
        if (context.BattlePartyMode == ModeUseItem)
        {
            return PartyUseItemScreen(battleParty);
        }
        battleParty.QueuedState = StateSetupSelectPokemonScreen;
        return StateScreenTransition;
    }

    // OFFSET 0x0221FFDC
    private static int PartyUseItemScreen(BattleParty battleParty)
    {
        BattlePartyContext context = battleParty.Context!;
        int index = context.SelectedPartyIndex;
        if ((index == 0 && context.EmbargoRemainingTurns[0] != 0) || (index == 1 && context.EmbargoRemainingTurns[1] != 0))
        {
            BattlePartyText_PrintEmbargoPreventingItemUse(battleParty);
            BattlePartyText_DisplayErrorMessage(battleParty);
            context.SelectedPartyIndex = CancelSlot;
            battleParty.QueuedState = StateExit;
            return StateAwaitingTextFinish;
        }
        if (Item.Item_LoadParam(context.SelectedBattleBagItem, ItemParamRestorePPOne, context.HeapID) != 0
            && Item.Item_LoadParam(context.SelectedBattleBagItem, ItemParamRestorePPAll, context.HeapID) == 0
            && battleParty.PartyPokemon[index].IsEgg == 0)
        {
            battleParty.QueuedState = StateSetupRestoreMovePPScreen;
            return StateScreenTransition;
        }
        if (BattleSystems.BattleSystem_UseBagItem(context.BattleSys!, context.Battler, context.PokemonPartySlots[index], 0, context.SelectedBattleBagItem) == 1)
        {
            if (Item.Item_LoadParam(context.SelectedBattleBagItem, ItemParamRestorePPAll, context.HeapID) != 0)
            {
                battleParty.QueuedState = StateSetupRestoreMovePPScreen;
            }
            else if (BattlePartyTask_CheckCanPartySlotBeSelected(battleParty, index) == PartySlotSelectableInBattle
                && Item.Item_LoadParam(context.SelectedBattleBagItem, ItemParamPartyUseOutside, context.HeapID) == 0)
            {
                UseBagItem(context.BattleSys!, context.SelectedBattleBagItem, context.SelectedBattleBagPocket, context.HeapID);
                BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[context.SelectedPartyIndex];
                partyPokemon.Mon = BattleSystems.BattleSystem_GetPartyPokemon(context.BattleSys!, context.Battler, context.PokemonPartySlots[context.SelectedPartyIndex]);
                context.CurrentDamage = (ushort)Pokemon.Pokemon_GetValue(partyPokemon.Mon, 0xA3, null);
                context.CurrentDamage = (ushort)(context.CurrentDamage - partyPokemon.CurHP);
                battleParty.QueuedState = StateExit;
            }
            else
            {
                battleParty.QueuedState = StateUseRestorationItem;
            }
            battleParty.UseItemState = 0;
            return StateScreenTransition;
        }
        MessageLoader.MessageLoader_GetString(battleParty.MessageLoader!, MsgItemNoEffect, battleParty.String!);
        BattlePartyText_DisplayErrorMessage(battleParty);
        context.SelectedPartyIndex = CancelSlot;
        battleParty.QueuedState = StateExit;
        return StateAwaitingTextFinish;
    }

    // OFFSET 0x0222012C
    private static int BattlePartyTask_SelectPokemonScreen(BattleParty battleParty)
    {
        switch (CheckSelectPokemonScreenButtonsPressed(battleParty))
        {
            case 0:
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, 7);
                if (CheckCanSwitchPokemon(battleParty) == 1)
                {
                    return StateExit;
                }
                battleParty.QueuedState = StateDisplayCantSwitchMessage;
                return StateScreenTransition;
            case 1:
                if (CheckSelectedPokemonIsEgg(battleParty) == 1)
                {
                    break;
                }
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, 8);
                battleParty.QueuedState = StateSetupPokemonSummaryScreen;
                return StateScreenTransition;
            case 2:
                if (CheckSelectedPokemonIsEgg(battleParty) == 1)
                {
                    break;
                }
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, 0xA);
                battleParty.QueuedState = StateSetupPokemonMovesScreen;
                return StateScreenTransition;
            case 3:
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, 6);
                battleParty.QueuedState = StateSetupPartyPokemonScreen;
                return StateScreenTransition;
        }
        return StateSelectPokemonScreen;
    }

    private static int ChangeSelectedPokemon(BattleParty battleParty, int delta, int button)
    {
        byte next = UpdateSelectedPartyIndex(battleParty, battleParty.Context!.SelectedPartyIndex, delta);
        if (next == 0xFF)
        {
            return -1;
        }
        battleParty.Context.SelectedPartyIndex = next;
        SoundPlayback.Sound_PlayEffect(SeqSelect);
        BattlePartyButtons_PressButton(battleParty, button);
        battleParty.QueuedState = StateRefreshPokemonDetailsScreens;
        return StateScreenTransition;
    }

    // OFFSET 0x022201DC
    private static int BattlePartyTask_PokemonSummaryScreen(BattleParty battleParty)
    {
        int result;
        switch (CheckPokemonSummaryScreenButtonsPressed(battleParty))
        {
            case 0:
                result = ChangeSelectedPokemon(battleParty, -1, 0xC);
                if (result != -1)
                {
                    return result;
                }
                break;
            case 1:
                result = ChangeSelectedPokemon(battleParty, 1, 0xD);
                if (result != -1)
                {
                    return result;
                }
                break;
            case 2:
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, 0xB);
                battleParty.QueuedState = StateSetupPokemonMovesScreen;
                return StateScreenTransition;
            case 3:
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, 6);
                battleParty.SelectPokemonPreviousScreenButton = 1;
                battleParty.QueuedState = StateSetupSelectPokemonScreen;
                return StateScreenTransition;
        }
        return StatePokemonSummaryScreen;
    }

    // OFFSET 0x0222029C
    private static int BattlePartyTask_PokemonMovesScreen(BattleParty battleParty)
    {
        int button = CheckPokemonMovesScreenButtonsPressed(battleParty);
        int result;
        switch (button)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                if (battleParty.PartyPokemon[Index(battleParty)].Moves[button].Move == 0)
                {
                    break;
                }
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, button + 0xE);
                battleParty.Context!.SelectedMoveSlot = (byte)button;
                battleParty.QueuedState = StateSetupMoveSummaryScreen;
                return StateScreenTransition;
            case 4:
                result = ChangeSelectedPokemon(battleParty, -1, 0xC);
                if (result != -1)
                {
                    return result;
                }
                break;
            case 5:
                result = ChangeSelectedPokemon(battleParty, 1, 0xD);
                if (result != -1)
                {
                    return result;
                }
                break;
            case 6:
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, 9);
                battleParty.QueuedState = StateSetupPokemonSummaryScreen;
                return StateScreenTransition;
            case 7:
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, 6);
                battleParty.SelectPokemonPreviousScreenButton = 2;
                battleParty.QueuedState = StateSetupSelectPokemonScreen;
                return StateScreenTransition;
        }
        return StatePokemonMovesScreen;
    }

    // OFFSET 0x022203A0
    private static int BattlePartyTask_MoveSummaryScreen(BattleParty battleParty)
    {
        int button = CheckMoveSummaryScreenButtonsPressed(battleParty);
        switch (button)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                if (battleParty.Context!.SelectedMoveSlot != button && battleParty.PartyPokemon[Index(battleParty)].Moves[button].Move == 0)
                {
                    break;
                }
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                battleParty.Context.SelectedMoveSlot = (byte)button;
                return StateSetupMoveSummaryScreen;
            case 4:
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, 6);
                battleParty.QueuedState = StateSetupPokemonMovesScreen;
                return StateScreenTransition;
        }
        return StateMoveSummaryScreen;
    }

    private static int ReadMenuInput(BattleParty battleParty, uint rects, int cancelValue)
    {
        int input = CheckTouchRectIsPressed(battleParty, BattleSubMenuData.TouchRects(rects));
        if (input == TouchScreen.ButtonNotPressed)
        {
            input = BattleSubMenuCursor.BattleSubMenuCursorTick(battleParty.Cursor!);
            if (input == BattleSubMenuCursor.Cancelled)
            {
                input = cancelValue;
            }
        }
        else
        {
            BattlePartySprites_DisableCursor(battleParty);
        }
        return input;
    }

    private static void ToggleContestStats(BattleParty battleParty)
    {
        battleParty.DisplayingContestStats = (byte)((battleParty.DisplayingContestStats ^ 1) & 0xF);
    }

    // OFFSET 0x02220418
    private static int BattlePartyTask_LearnMoveScreen(BattleParty battleParty)
    {
        int input = ReadMenuInput(battleParty, LearnMoveTouchRects, 6);
        switch (input)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
                battleParty.Context!.SelectedMoveSlot = (byte)input;
                battleParty.LearnMovePreviousScreenButton = (byte)input;
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, input + 0x17);
                battleParty.QueuedState = StateSetupConfirmLearnMoveScreen;
                return StateScreenTransition;
            case 5:
                if (battleParty.HasVisitedContestHall == 0)
                {
                    break;
                }
                ToggleContestStats(battleParty);
                battleParty.LearnMovePreviousScreenButton = (byte)input;
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, 0x12);
                battleParty.QueuedState = StateSetupLearnMoveScreen;
                return StateScreenTransition;
            case 6:
                battleParty.Context!.SelectedMoveSlot = MoveToLearnSlot;
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, 6);
                battleParty.QueuedState = StateExit;
                return StateScreenTransition;
        }
        return StateLearnMoveScreen;
    }

    // OFFSET 0x0222050C
    private static int BattlePartyTask_ConfirmLearnMoveScreen(BattleParty battleParty)
    {
        int input = ReadMenuInput(battleParty, ConfirmLearnMoveTouchRects, 2);
        switch (input)
        {
            case 0:
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                if (battleParty.DisplayingContestStats == 0)
                {
                    BattlePartyButtons_PressButton(battleParty, 0x1C);
                }
                else
                {
                    BattlePartyButtons_PressButton(battleParty, 0x1D);
                }
                if (CheckSelectedMoveIsHM(battleParty) == 1)
                {
                    BattlePartyText_PrintHMMovesCantBeForgottenText(battleParty);
                    if (battleParty.DisplayingContestStats == 0)
                    {
                        ClearMoveStats(battleParty);
                    }
                    else
                    {
                        ClearMoveContestStats(battleParty);
                    }
                    battleParty.QueuedState = StateConfirmLearnMoveScreen;
                }
                else
                {
                    battleParty.QueuedState = StateExit;
                }
                return StateScreenTransition;
            case 1:
                if (battleParty.HasVisitedContestHall == 0)
                {
                    break;
                }
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, 0x12);
                ToggleContestStats(battleParty);
                battleParty.ConfirmLearnMovePreviousScreenButton = (byte)input;
                battleParty.QueuedState = StateSetupConfirmLearnMoveScreen;
                return StateScreenTransition;
            case 2:
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, 6);
                battleParty.ConfirmLearnMovePreviousScreenButton = 0;
                battleParty.QueuedState = StateSetupLearnMoveScreen;
                return StateScreenTransition;
        }
        return StateConfirmLearnMoveScreen;
    }

    // OFFSET 0x02220628
    private static int BattlePartyTask_RestoreMovePPScreen(BattleParty battleParty)
    {
        BattlePartyContext context = battleParty.Context!;
        int input = ReadMenuInput(battleParty, RestoreMovePPTouchRects, 4);
        switch (input)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                if (battleParty.PartyPokemon[context.SelectedPartyIndex].Moves[input].Move == 0)
                {
                    break;
                }
                battleParty.Context!.SelectedMoveSlot = (byte)input;
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, input + 0x13);
                if (BattleSystems.BattleSystem_UseBagItem(context.BattleSys!, context.Battler, context.PokemonPartySlots[context.SelectedPartyIndex], input, context.SelectedBattleBagItem) == 1)
                {
                    battleParty.UseItemState = 0;
                    battleParty.QueuedState = StateUseRestorationItem;
                    return StateScreenTransition;
                }
                MessageLoader.MessageLoader_GetString(battleParty.MessageLoader!, MsgItemNoEffect, battleParty.String!);
                BattlePartyText_DisplayErrorMessage(battleParty);
                battleParty.Context.SelectedPartyIndex = CancelSlot;
                battleParty.QueuedState = StateExit;
                return StateAwaitingTextFinish;
            case 4:
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                BattlePartyButtons_PressButton(battleParty, 6);
                battleParty.QueuedState = StateSetupPartyPokemonScreen;
                return StateScreenTransition;
        }
        return StateRestoreMovePPScreen;
    }

    // OFFSET 0x0222072C
    private static int BattlePartyTask_SetupPartyPokemonScreen(BattleParty battleParty)
    {
        ChangeBattlePartyScreen(battleParty, ScreenPokemonParty);
        return StatePartyPokemonScreen;
    }

    // OFFSET 0x02220738
    private static int BattlePartyTask_SetupSelectPokemonScreen(BattleParty battleParty)
    {
        ChangeBattlePartyScreen(battleParty, ScreenSelectPokemon);
        return StateSelectPokemonScreen;
    }

    // OFFSET 0x02220744
    private static int BattlePartyTask_SetupPokemonSummaryScreen(BattleParty battleParty)
    {
        ChangeBattlePartyScreen(battleParty, ScreenPokemonSummary);
        return StatePokemonSummaryScreen;
    }

    // OFFSET 0x02220750
    private static int BattlePartyTask_SetupPokemonMovesScreen(BattleParty battleParty)
    {
        ChangeBattlePartyScreen(battleParty, ScreenPokemonMoves);
        return StatePokemonMovesScreen;
    }

    // OFFSET 0x0222075C
    private static int BattlePartyTask_SetupMoveSummaryScreen(BattleParty battleParty)
    {
        ChangeBattlePartyScreen(battleParty, ScreenMoveSummary);
        return StateMoveSummaryScreen;
    }

    // OFFSET 0x02220768
    private static int BattlePartyTask_SetupLearnMoveScreen(BattleParty battleParty)
    {
        ChangeBattlePartyScreen(battleParty, battleParty.DisplayingContestStats == 0 ? ScreenLearnMove : ScreenLearnMoveContestStats);
        return StateLearnMoveScreen;
    }

    // OFFSET 0x0222078C
    private static int BattlePartyTask_SetupConfirmLearnMoveScreen(BattleParty battleParty)
    {
        BattlePartySprites_DrawMovesSprites(battleParty);
        ChangeBattlePartyScreen(battleParty, battleParty.DisplayingContestStats == 0 ? ScreenConfirmLearnMove : ScreenConfirmLearnMoveContestStats);
        return StateConfirmLearnMoveScreen;
    }

    // OFFSET 0x022207B8
    private static int BattlePartyTask_SetupRestoreMovePPScreen(BattleParty battleParty)
    {
        ChangeBattlePartyScreen(battleParty, ScreenRestoreMovePP);
        if (Item.Item_LoadParam(battleParty.Context!.SelectedBattleBagItem, ItemParamRestorePPAll, battleParty.Context.HeapID) != 0)
        {
            return StateUseAllMovePPRestorationItem;
        }
        return StateRestoreMovePPScreen;
    }

    // OFFSET 0x022207DC
    private static int BattlePartyTask_RefreshPokemonDetailsScreens(BattleParty battleParty)
    {
        BattlePartySprites_SetupScreen(battleParty, battleParty.CurrentScreen);
        BattlePartyText_ChangeScreen(battleParty, battleParty.CurrentScreen);
        BattlePartyButtons_InitializeButtons(battleParty, battleParty.CurrentScreen);
        DrawXPBar(battleParty, battleParty.CurrentScreen);
        if (battleParty.CurrentScreen == ScreenPokemonSummary)
        {
            return StatePokemonSummaryScreen;
        }
        return StatePokemonMovesScreen;
    }

    // OFFSET 0x0222081C
    private static int BattlePartyTask_DisplayCantSwitchMessage(BattleParty battleParty)
    {
        BattlePartyText_DisplayErrorMessage(battleParty);
        battleParty.QueuedState = StateClearErrorMessage;
        return StateAwaitingTextFinish;
    }

    // OFFSET 0x02220834
    private static int BattlePartyTask_ClearErrorMessage(BattleParty battleParty)
    {
        RenderWindow.Window_EraseMessageBox(battleParty.MessageBoxWindows[1], 0);
        return StateSelectPokemonScreen;
    }

    // OFFSET 0x02220848
    private static int BattlePartyTask_AwaitingTextFinish(BattleParty battleParty)
    {
        if (Text.Text_IsPrinterActive(battleParty.TextPrinterID) == 0)
        {
            return StateAwaitingInput;
        }
        return StateAwaitingTextFinish;
    }

    // OFFSET 0x02220864
    private static int BattlePartyTask_AwaitingInput(BattleParty battleParty)
    {
        if ((Globals.gSystem.PressedKeys & (PadButtonA | PadButtonB)) != 0 || TouchScreen.TouchScreen_Tapped())
        {
            return battleParty.QueuedState;
        }
        return StateAwaitingInput;
    }

    // OFFSET 0x0222088C
    private static int BattlePartyTask_ScreenTransition(BattleParty battleParty)
    {
        if (battleParty.IsAButtonPressed == 0)
        {
            return battleParty.QueuedState;
        }
        return StateScreenTransition;
    }

    // OFFSET 0x022208A4
    private static int BattlePartyTask_UseRestorationItem(BattleParty battleParty)
    {
        BattlePartyContext context = battleParty.Context!;
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[context.SelectedPartyIndex];
        switch (battleParty.UseItemState)
        {
            case 0:
                partyPokemon.Mon = BattleSystems.BattleSystem_GetPartyPokemon(context.BattleSys!, context.Battler, context.PokemonPartySlots[context.SelectedPartyIndex]);
                BattlePartyText_PrintUseItemEffect(battleParty);
                if (battleParty.CurrentScreen == ScreenRestoreMovePP)
                {
                    battleParty.SelectedPokemonCurrentMovePPs[0] = (ushort)Pokemon.Pokemon_GetValue(partyPokemon.Mon, 0x3A + context.SelectedMoveSlot, null);
                    battleParty.UseItemState = 2;
                }
                else
                {
                    partyPokemon.SummaryStatus = (byte)(Pokemon.Pokemon_GetSummaryStatus(partyPokemon.Mon) & 0xF);
                    if (partyPokemon.SummaryStatus == Pokemon.SummaryStatusNone)
                    {
                        SpriteSystem.ManagedSprite_SetDrawFlag(battleParty.Sprites[context.SelectedPartyIndex + 0xD]!, 0);
                        BattlePartyText_PrintPartyPokemonLevel(battleParty, context.SelectedPartyIndex);
                    }
                    battleParty.SelectedPokemonCurrentHP = (ushort)Pokemon.Pokemon_GetValue(partyPokemon.Mon, 0xA3, null);
                    battleParty.UseItemState = 4;
                }
                SoundPlayback.Sound_PlayEffect(SeqRestore);
                break;
            case 1:
                if (battleParty.SelectedPokemonCurrentHP != partyPokemon.CurHP)
                {
                    partyPokemon.CurHP++;
                    BattlePartyText_RenderPartyPokemonStats(battleParty, context.SelectedPartyIndex);
                }
                else
                {
                    battleParty.UseItemState = 3;
                }
                break;
            case 2:
                {
                    BattlePartyPokemonMove move = partyPokemon.Moves[context.SelectedMoveSlot];
                    if (battleParty.SelectedPokemonCurrentMovePPs[0] != move.CurrentPP)
                    {
                        move.CurrentPP++;
                        BattlePartyText_PrintSelectedMoveCurrentPP(battleParty, (ushort)(context.SelectedMoveSlot + 1), context.SelectedMoveSlot);
                    }
                    else
                    {
                        battleParty.UseItemState = 3;
                    }
                    break;
                }
            case 3:
                UseBagItem(context.BattleSys!, context.SelectedBattleBagItem, context.SelectedBattleBagPocket, context.HeapID);
                BattlePartyText_DisplayErrorMessage(battleParty);
                battleParty.QueuedState = StateExit;
                return StateAwaitingTextFinish;
            case 4:
                if (battleParty.SelectedPokemonCurrentHP != partyPokemon.CurHP)
                {
                    partyPokemon.CurHP++;
                    BattlePartyText_RenderPartyPokemonStats(battleParty, context.SelectedPartyIndex);
                    BattlePartyButtons_DrawSelectedPartyPokemonButton(battleParty);
                }
                battleParty.UseItemState = 1;
                break;
        }
        return StateUseRestorationItem;
    }

    // OFFSET 0x02220A4C
    private static int BattlePartyTask_UseAllMovePPRestorationItem(BattleParty battleParty)
    {
        BattlePartyContext context = battleParty.Context!;
        switch (battleParty.UseItemState)
        {
            case 0:
                {
                    BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[context.SelectedPartyIndex];
                    partyPokemon.Mon = BattleSystems.BattleSystem_GetPartyPokemon(context.BattleSys!, context.Battler, context.PokemonPartySlots[context.SelectedPartyIndex]);
                    for (int i = 0; i < 4; i++)
                    {
                        if (battleParty.PartyPokemon[context.SelectedPartyIndex].Moves[i].Move != 0)
                        {
                            battleParty.SelectedPokemonCurrentMovePPs[i] = (ushort)Pokemon.Pokemon_GetValue(battleParty.PartyPokemon[context.SelectedPartyIndex].Mon!, 0x3A + i, null);
                        }
                    }
                    BattlePartyText_PrintUseItemEffect(battleParty);
                    SoundPlayback.Sound_PlayEffect(SeqRestore);
                    battleParty.UseItemState = 1;
                    break;
                }
            case 1:
                {
                    int finished = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        BattlePartyPokemonMove move = battleParty.PartyPokemon[context.SelectedPartyIndex].Moves[i];
                        if (move.Move == 0)
                        {
                            finished++;
                        }
                        else if (battleParty.SelectedPokemonCurrentMovePPs[i] != move.CurrentPP)
                        {
                            move.CurrentPP++;
                            BattlePartyText_PrintSelectedMoveCurrentPP(battleParty, (ushort)(i + 1), (ushort)i);
                        }
                        else
                        {
                            finished++;
                        }
                    }
                    if (finished == 4)
                    {
                        battleParty.UseItemState = 2;
                    }
                    break;
                }
            case 2:
                UseBagItem(context.BattleSys!, context.SelectedBattleBagItem, context.SelectedBattleBagPocket, context.HeapID);
                BattlePartyText_DisplayErrorMessage(battleParty);
                battleParty.QueuedState = StateExit;
                return StateAwaitingTextFinish;
        }
        return StateUseAllMovePPRestorationItem;
    }

    // OFFSET 0x02220B78
    private static int BattlePartyTask_Exit(BattleParty battleParty)
    {
        PaletteData.PaletteData_StartFade(battleParty.Palette!, 0xA, 0xFFFF, -8, 0, 0x10, 0);
        return StateFinishTask;
    }

    // OFFSET 0x02220BA4
    private static int BattlePartyTask_FinishTask(SysTask task, BattleParty battleParty)
    {
        if (PaletteData.PaletteData_GetSelectedBuffersMask(battleParty.Palette!) != 0)
        {
            return 0;
        }
        CleanupMessageLoader(battleParty);
        BattlePartySprites_CleanupSprites(battleParty);
        BattlePartyText_ClearWindows(battleParty);
        CleanupBackground(battleParty.Background!);
        battleParty.Context!.IsCursorEnabled = (byte)BattleSubMenuCursor.IsBattleSubMenuCursorVisible(battleParty.Cursor!);
        BattleSubMenuCursor.DeleteBattleSubMenuCursor(battleParty.Cursor!);
        Font.Font_Free(FontSubscreen);
        battleParty.Context.BattlePartyExited = 1;
        SysTaskExtensions.SysTask_FinishAndFreeParam(task);
        return 1;
    }

    // OFFSET 0x02220C0C
    private static void InitializeBackground(BattleParty battleParty)
    {
        Bg.SetScreenGraphicsModes(BattleSubMenuData.GraphicsModes(GraphicsModesAddress), 1);
        Bg.Bg_InitFromTemplate(battleParty.Background!, 7, BattleSubMenuData.BgTemplate(BgTemplateLayer7), 0);
        Bg.Bg_InitFromTemplate(battleParty.Background!, 6, BattleSubMenuData.BgTemplate(BgTemplateLayer6), 0);
        Bg.Bg_InitFromTemplate(battleParty.Background!, 5, BattleSubMenuData.BgTemplate(BgTemplateLayer5), 0);
        Bg.Bg_ClearTilemap(battleParty.Background!, 5);
        Bg.Bg_InitFromTemplate(battleParty.Background!, 4, BattleSubMenuData.BgTemplate(BgTemplateLayer4), 0);
        Bg.Bg_ClearTilemap(battleParty.Background!, 4);
        Bg.Bg_ClearTilesRange(5, 0x20, 0, battleParty.Context!.HeapID);
        Bg.Bg_ClearTilesRange(4, 0x20, 0, battleParty.Context.HeapID);
        Bg.Bg_ScheduleTilemapTransfer(battleParty.Background!, 5);
        Bg.Bg_ScheduleTilemapTransfer(battleParty.Background!, 4);
    }

    // OFFSET 0x02220D1C
    private static void CleanupBackground(BgConfig background)
    {
        GXLayers.GXLayers_EngineBToggleLayers(0x1F, 0);
        Bg.Bg_FreeTilemapBuffer(background, 4);
        Bg.Bg_FreeTilemapBuffer(background, 5);
        Bg.Bg_FreeTilemapBuffer(background, 6);
        Bg.Bg_FreeTilemapBuffer(background, 7);
    }

    // OFFSET 0x02220D4C
    private static void LoadGraphicsData(BattleParty battleParty)
    {
        int heapID = battleParty.Context!.HeapID;
        NARC narc = NARC.NARC_ctor(NarcBattlePartyGraphics, heapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x16, battleParty.Background!, 7, 0, 0, 0, heapID);
        byte[] file = NARC.NARC_AllocAndReadWholeMember(narc, 0x14, heapID);
        G2d.GetUnpackedScreenData(file, out ScreenData? screenData);
        BattlePartyButtons_InitializeButtonData(battleParty, screenData!.RawData);
        file = NARC.NARC_AllocAndReadWholeMember(narc, 0x15, heapID);
        G2d.GetUnpackedScreenData(file, out screenData);
        BattlePartyButtons_InitializeAdditionalButtonData(battleParty, screenData!.RawData);
        PaletteData.PaletteData_LoadBufferFromFileStart(battleParty.Palette!, NarcBattlePartyGraphics, 0x17, heapID, PaletteBufferSubBg, 0x200, 0);
        NARC.NARC_dtor(narc);
        ushort[] unfaded = PaletteData.PaletteData_GetUnfadedBuffer(battleParty.Palette!, PaletteBufferSubBg)!;
        Array.Copy(unfaded, 0xC0, battleParty.ScreenPaletteData, 0, 0x20);
        PaletteData.PaletteData_LoadBufferFromFileStart(battleParty.Palette!, NarcObjPalettes, 6, heapID, PaletteBufferSubBg, 0x20, 0xD0);
        PaletteData.PaletteData_LoadBufferFromFileStart(battleParty.Palette!, NarcObjPalettes, 7, heapID, PaletteBufferSubBg, 0x20, 0xF0);
        uint frame = (uint)BattleSystems.BattleSystem_GetOptionsFrame(battleParty.Context.BattleSys!);
        Graphics.Graphics_LoadTilesToBgLayer(NarcWindowGraphics, RenderWindow.GetMessageBoxTilesNARCMember(frame), battleParty.Background!, 4, 1, 0, 0, heapID);
        PaletteData.PaletteData_LoadBufferFromFileStart(battleParty.Palette!, NarcWindowGraphics, (int)RenderWindow.GetMessageBoxPaletteNARCMember(frame), heapID, PaletteBufferSubBg, 0x20, 0xE0);
        unfaded = PaletteData.PaletteData_GetUnfadedBuffer(battleParty.Palette!, PaletteBufferSubBg)!;
        ushort[] colors = new ushort[0x10];
        Array.Copy(unfaded, 0xD0, colors, 0, 0x10);
        colors[7] = unfaded[0x9A];
        colors[8] = unfaded[0x9B];
        colors[3] = unfaded[0x9C];
        colors[4] = unfaded[0x9D];
        PaletteData.PaletteData_LoadBuffer(battleParty.Palette!, colors, 0, PaletteBufferSubBg, 0xD0, 0x20);
    }

    // OFFSET 0x02220F08
    private static void InitializeMessageLoader(BattleParty battleParty)
    {
        int heapID = battleParty.Context!.HeapID;
        battleParty.MessageLoader = MessageLoader.MessageLoader_Init(0, 0x1A, 3, heapID);
        battleParty.Unused3 = FontSpecialCharsContext.FontSpecialChars_Init(0xF, 0xE, 0, heapID);
        battleParty.StringTemplate = StringTemplate.StringTemplate_Default(heapID);
        battleParty.String = GameString.String_Init(0x200, heapID);
    }

    // OFFSET 0x02220F60
    private static void CleanupMessageLoader(BattleParty battleParty)
    {
        MessageLoader.MessageLoader_Free(battleParty.MessageLoader);
        FontSpecialCharsContext.FontSpecialChars_Free(battleParty.Unused3);
        StringTemplate.StringTemplate_Free(battleParty.StringTemplate);
        GameString.String_Free(battleParty.String);
    }

    // OFFSET 0x02220F98
    private static void InitialisePartyPokemon(BattleParty battleParty)
    {
        Party party = battleParty.Context!.Party!;
        for (int slot = 0; slot < Party.Party_GetCurrentCount(party); slot++)
        {
            BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[slot];
            partyPokemon.Mon = Party.Party_GetPokemonBySlotIndex(party, slot);
            Pokemon mon = partyPokemon.Mon;
            partyPokemon.Species = (ushort)Pokemon.Pokemon_GetValue(mon, 5, null);
            if (partyPokemon.Species == 0)
            {
                continue;
            }
            partyPokemon.Attack = (ushort)Pokemon.Pokemon_GetValue(mon, 0xA5, null);
            partyPokemon.Defence = (ushort)Pokemon.Pokemon_GetValue(mon, 0xA6, null);
            partyPokemon.Speed = (ushort)Pokemon.Pokemon_GetValue(mon, 0xA7, null);
            partyPokemon.SpAtk = (ushort)Pokemon.Pokemon_GetValue(mon, 0xA8, null);
            partyPokemon.SpDef = (ushort)Pokemon.Pokemon_GetValue(mon, 0xA9, null);
            partyPokemon.CurHP = (ushort)Pokemon.Pokemon_GetValue(mon, 0xA3, null);
            partyPokemon.MaxHP = (ushort)Pokemon.Pokemon_GetValue(mon, 0xA4, null);
            partyPokemon.Type1 = (byte)Pokemon.Pokemon_GetValue(mon, 0xB1, null);
            partyPokemon.Type2 = (byte)Pokemon.Pokemon_GetValue(mon, 0xB2, null);
            partyPokemon.Level = (byte)(Pokemon.Pokemon_GetValue(mon, 0xA1, null) & 0x7F);
            partyPokemon.HideGenderMarker = (byte)(Pokemon.Pokemon_GetValue(mon, 0xB0, null) == 1 ? 0 : 1);
            partyPokemon.Gender = (byte)(Pokemon.Pokemon_GetGender(mon) & 7);
            partyPokemon.SummaryStatus = (byte)(Pokemon.Pokemon_GetSummaryStatus(mon) & 0xF);
            partyPokemon.IsEgg = (byte)(Pokemon.Pokemon_GetValue(mon, 0x4C, null) & 1);
            partyPokemon.Ability = (ushort)Pokemon.Pokemon_GetValue(mon, 0xA, null);
            partyPokemon.HeldItem = (ushort)Pokemon.Pokemon_GetValue(mon, 6, null);
            partyPokemon.Exp = Pokemon.Pokemon_GetValue(mon, 8, null);
            partyPokemon.CurrentLevelBaseExp = Pokemon.Species_GetExpForLevel(partyPokemon.Species, partyPokemon.Level);
            if (partyPokemon.Level == 100)
            {
                partyPokemon.NextLevelExp = partyPokemon.CurrentLevelBaseExp;
            }
            else
            {
                partyPokemon.NextLevelExp = Pokemon.Species_GetExpForLevel(partyPokemon.Species, partyPokemon.Level + 1);
            }
            partyPokemon.Cool = (byte)Pokemon.Pokemon_GetValue(mon, 0x13, null);
            partyPokemon.Beauty = (byte)Pokemon.Pokemon_GetValue(mon, 0x14, null);
            partyPokemon.Cute = (byte)Pokemon.Pokemon_GetValue(mon, 0x15, null);
            partyPokemon.Smart = (byte)Pokemon.Pokemon_GetValue(mon, 0x16, null);
            partyPokemon.Tough = (byte)Pokemon.Pokemon_GetValue(mon, 0x17, null);
            partyPokemon.Mail = (byte)Pokemon.Pokemon_GetValue(mon, 0xA2, null);
            partyPokemon.Form = (byte)Pokemon.Pokemon_GetValue(mon, 0x70, null);
            for (int i = 0; i < 4; i++)
            {
                BattlePartyPokemonMove move = partyPokemon.Moves[i];
                move.Move = (ushort)Pokemon.Pokemon_GetValue(mon, 0x36 + i, null);
                if (move.Move == 0)
                {
                    continue;
                }
                move.CurrentPP = (byte)Pokemon.Pokemon_GetValue(mon, 0x3A + i, null);
                move.MaxPP = (byte)Pokemon.Pokemon_GetValue(mon, 0x3E + i, null);
                move.MaxPP = (byte)MoveTable.MoveTable_CalcMaxPP(move.Move, move.MaxPP);
                move.Type = (byte)MoveTable.MoveTable_LoadParam(move.Move, 3);
                move.Class = (byte)MoveTable.MoveTable_LoadParam(move.Move, 1);
                move.Accuracy = (byte)MoveTable.MoveTable_LoadParam(move.Move, 4);
                move.Power = (byte)MoveTable.MoveTable_LoadParam(move.Move, 2);
            }
        }
    }

    // OFFSET 0x0222124C
    private static int CheckPartyPokemonScreenButtonPressed(BattleParty battleParty)
    {
        int input = CheckTouchRectIsPressed(battleParty, BattleSubMenuData.TouchRects(PartyPokemonTouchRects));
        if (input == TouchScreen.ButtonNotPressed)
        {
            input = BattleSubMenuCursor.BattleSubMenuCursorTick(battleParty.Cursor!);
            if (input == BattleSubMenuCursor.Cancelled)
            {
                input = CancelSlot;
            }
            else if (input == BattleSubMenuCursor.NoSelection)
            {
                return 0;
            }
            if (input == CancelSlot || BattlePartyTask_CheckCanPartySlotBeSelected(battleParty, input) != 0)
            {
                battleParty.Context!.SelectedPartyIndex = (byte)input;
                return 1;
            }
            return 0;
        }
        if (input == CancelSlot || BattlePartyTask_CheckCanPartySlotBeSelected(battleParty, input) != 0)
        {
            battleParty.Context!.SelectedPartyIndex = (byte)input;
            BattlePartySprites_DisableCursor(battleParty);
            return 1;
        }
        return 0;
    }

    private static byte CheckScreenButtonsPressed(BattleParty battleParty, uint rects, int cancelValue)
    {
        int input = CheckTouchRectIsPressed(battleParty, BattleSubMenuData.TouchRects(rects));
        if (input == TouchScreen.ButtonNotPressed)
        {
            input = BattleSubMenuCursor.BattleSubMenuCursorTick(battleParty.Cursor!);
            if (input == BattleSubMenuCursor.Cancelled)
            {
                input = cancelValue;
            }
            else if (input == BattleSubMenuCursor.NoSelection)
            {
                return 0xFF;
            }
        }
        else
        {
            BattlePartySprites_DisableCursor(battleParty);
        }
        return (byte)input;
    }

    // OFFSET 0x022212C4
    private static byte CheckSelectPokemonScreenButtonsPressed(BattleParty battleParty)
    {
        return CheckScreenButtonsPressed(battleParty, SelectPokemonTouchRects, 3);
    }

    // OFFSET 0x0222130C
    private static byte CheckPokemonSummaryScreenButtonsPressed(BattleParty battleParty)
    {
        return CheckScreenButtonsPressed(battleParty, PokemonSummaryTouchRects, 3);
    }

    // OFFSET 0x02221354
    private static byte CheckPokemonMovesScreenButtonsPressed(BattleParty battleParty)
    {
        return CheckScreenButtonsPressed(battleParty, PokemonMovesTouchRects, 7);
    }

    // OFFSET 0x0222139C
    private static byte CheckMoveSummaryScreenButtonsPressed(BattleParty battleParty)
    {
        return CheckScreenButtonsPressed(battleParty, MoveSummaryTouchRects, 4);
    }

    // OFFSET 0x022213E4
    private static int CheckTouchRectIsPressed(BattleParty battleParty, TouchScreenHitTable[] rects)
    {
        return TouchScreen.TouchScreen_CheckRectanglePressed(rects);
    }

    // OFFSET 0x022213F0
    public static byte BattlePartyTask_CheckCanPartySlotBeSelected(BattleParty battleParty, int partySlot)
    {
        if (battleParty.PartyPokemon[partySlot].Species == 0)
        {
            return 0;
        }
        if (partySlot == 0)
        {
            return PartySlotSelectableInBattle;
        }
        if ((BattlePartyTask_CheckIfPartnerBattle(battleParty) != 0 || BattlePartyTask_CheckIf2V2Battle(battleParty) != 0) && partySlot == 1)
        {
            return PartySlotSelectableInBattle;
        }
        return PartySlotSelectableNotInBattle;
    }

    // OFFSET 0x02221428
    private static byte UpdateSelectedPartyIndex(BattleParty battleParty, int current, int delta)
    {
        int index;
        if (BattlePartyTask_CheckIf2V2Battle(battleParty) == 1)
        {
            byte[] order = BattleSubMenuData.Bytes(DoubleBattlePartyOrder, 6);
            for (index = 0; index < 6; index++)
            {
                if (current == order[index])
                {
                    break;
                }
            }
            while (true)
            {
                index += delta;
                if (index < 0)
                {
                    index = 5;
                }
                else if (index >= 6)
                {
                    index = 0;
                }
                if (current == order[index])
                {
                    return 0xFF;
                }
                if (BattlePartyTask_CheckCanPartySlotBeSelected(battleParty, order[index]) != 0 && battleParty.PartyPokemon[order[index]].IsEgg == 0)
                {
                    return order[index];
                }
            }
        }
        index = current;
        while (true)
        {
            index += delta;
            if (index < 0)
            {
                index = 5;
            }
            else if (index >= 6)
            {
                index = 0;
            }
            if (current == index)
            {
                return 0xFF;
            }
            if (BattlePartyTask_CheckCanPartySlotBeSelected(battleParty, index) != 0 && battleParty.PartyPokemon[index].IsEgg == 0)
            {
                return (byte)index;
            }
        }
    }

    // OFFSET 0x022214E0
    private static void DrawXPBar(BattleParty battleParty, int screen)
    {
        if (screen != ScreenPokemonSummary)
        {
            return;
        }
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[Index(battleParty)];
        byte fill;
        if (partyPokemon.Level < 100)
        {
            fill = HealthBar.HealthBar_Fill(partyPokemon.Exp - partyPokemon.CurrentLevelBaseExp, partyPokemon.NextLevelExp - partyPokemon.CurrentLevelBaseExp, 0x40);
        }
        else
        {
            fill = HealthBar.HealthBar_Fill(0, 0, 0x40);
        }
        for (int i = 0; i < 8; i++)
        {
            ushort tile = fill >= 8 ? (ushort)TileXPBarFull : (ushort)(fill + TileXPBarBase);
            DrawXPBarSegment(battleParty, tile, (ushort)(i + 0xA), 8);
            fill = fill >= 8 ? (byte)(fill - 8) : (byte)0;
        }
        Bg.Bg_ScheduleTilemapTransfer(battleParty.Background!, 7);
    }

    // OFFSET 0x02221560
    private static void DrawXPBarSegment(BattleParty battleParty, ushort tile, ushort x, byte y)
    {
        Bg.Bg_FillTilemapRect(battleParty.Background!, 7, tile, (byte)x, y, 1, 1, 0x10);
    }

    // OFFSET 0x02221590
    private static void DrawAppealPt(BattleParty battleParty, ushort tile, byte index)
    {
        int x = index * 2 + 2;
        Bg.Bg_FillTilemapRect(battleParty.Background!, 7, tile, (byte)x, 0xE, 1, 1, 0x10);
        Bg.Bg_FillTilemapRect(battleParty.Background!, 7, (ushort)(tile + 1), (byte)(index * 2 + 3), 0xE, 1, 1, 0x10);
        Bg.Bg_FillTilemapRect(battleParty.Background!, 7, (ushort)(tile + 0x20), (byte)x, 0xF, 1, 1, 0x10);
        Bg.Bg_FillTilemapRect(battleParty.Background!, 7, (ushort)(tile + 0x21), (byte)(index * 2 + 3), 0xF, 1, 1, 0x10);
    }

    // OFFSET 0x02221630
    private static void DrawEmptyAppealPts(BattleParty battleParty)
    {
        for (int i = 0; i < 6; i++)
        {
            DrawAppealPt(battleParty, TileAppealEmpty, (byte)i);
        }
    }

    // OFFSET 0x02221654
    private static void DrawMoveContestStats(BattleParty battleParty, int screen)
    {
        if (screen != ScreenConfirmLearnMoveContestStats)
        {
            return;
        }
        DrawEmptyAppealPts(battleParty);
        BattlePartyContext context = battleParty.Context!;
        ushort move = context.SelectedMoveSlot < 4 ? battleParty.PartyPokemon[context.SelectedPartyIndex].Moves[context.SelectedMoveSlot].Move : context.MoveToLearn;
        sbyte appeal = (sbyte)(ContestEffect.ContestEffect_GetAppeal((int)MoveTable.MoveTable_LoadParam(move, 0xA)) / 10);
        for (int i = 0; i < appeal; i++)
        {
            DrawAppealPt(battleParty, TileAppealFull, (byte)i);
        }
        Bg.Bg_ScheduleTilemapTransfer(battleParty.Background!, 7);
    }

    // OFFSET 0x022216C0
    private static void ChangeBattlePartyScreen(BattleParty battleParty, int screen)
    {
        DrawScreenBackground(battleParty, screen);
        Bg.Bg_ScheduleFillTilemap(battleParty.Background!, 4, 0);
        Bg.Bg_ScheduleFillTilemap(battleParty.Background!, 5, 0);
        BattlePartySprites_SetupScreen(battleParty, screen);
        BattlePartyText_ClearScreenWindows(battleParty);
        BattlePartyText_InitializeScreenWindows(battleParty, screen);
        BattlePartyText_ChangeScreen(battleParty, screen);
        DrawXPBar(battleParty, screen);
        DrawMoveContestStats(battleParty, screen);
        BattlePartySprites_SetupCursor(battleParty, screen);
        BattlePartyButtons_InitializeButtons(battleParty, screen);
        BattlePartyButtons_LoadScreenPaletteData(battleParty, screen);
        battleParty.CurrentScreen = (byte)screen;
    }

    // OFFSET 0x02221738
    private static void DrawScreenBackground(BattleParty battleParty, int screen)
    {
        for (int i = 0; i < 2; i++)
        {
            int member = (int)BattleSubMenuData.U32((uint)(ScreenBackgroundMembers + screen * 8 + i * 4));
            byte[] file = NARC.NARC_AllocAndReadWholeMemberByIndexPair(NarcBattlePartyGraphics, member, battleParty.Context!.HeapID);
            G2d.GetUnpackedScreenData(file, out ScreenData? screenData);
            Bg.Bg_LoadToTilemapRect(battleParty.Background!, i + 6, screenData!.RawData, 0, 0, 0x20, 0x18);
            Bg.Bg_ScheduleTilemapTransfer(battleParty.Background!, i + 6);
        }
    }

    private static void FormatNicknameMessage(BattleParty battleParty, int messageID, Pokemon mon)
    {
        GameString message = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, (uint)messageID)!;
        StringTemplate.StringTemplate_SetNickname(battleParty.StringTemplate!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
        StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, battleParty.String!, message);
        GameString.String_Free(message);
    }

    // OFFSET 0x022217A4
    private static int CheckCanSwitchPokemon(BattleParty battleParty)
    {
        BattlePartyContext context = battleParty.Context!;
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[context.SelectedPartyIndex];
        if (BattlePartyTask_CheckIfSwitchingWithPartnersPokemon(battleParty, context.SelectedPartyIndex) == 1)
        {
            GameString message = MessageLoader.MessageLoader_GetNewString(battleParty.MessageLoader!, MsgCantSwitchPartner)!;
            int partner = BattleSystems.BattleSystem_GetPartner(context.BattleSys!, context.Battler);
            Trainer trainer = BattleMessages.TrainerFromData(BattleSystems.BattleSystem_GetTrainer(context.BattleSys!, partner));
            StringTemplate.StringTemplate_SetTrainerNameBattle(battleParty.StringTemplate!, 0, trainer);
            StringTemplate.StringTemplate_Format(battleParty.StringTemplate!, battleParty.String!, message);
            GameString.String_Free(message);
            return 0;
        }
        if (partyPokemon.CurHP == 0)
        {
            FormatNicknameMessage(battleParty, MsgCantSwitchFainted, partyPokemon.Mon!);
            return 0;
        }
        byte partySlot = context.PokemonPartySlots[context.SelectedPartyIndex];
        if (context.PlayerPokemonPartySlot == partySlot || context.PartnerPokemonPartySlot == partySlot)
        {
            FormatNicknameMessage(battleParty, MsgCantSwitchInBattle, partyPokemon.Mon!);
            return 0;
        }
        if (CheckSelectedPokemonIsEgg(battleParty) == 1)
        {
            MessageLoader.MessageLoader_GetString(battleParty.MessageLoader!, MsgCantSwitchEgg, battleParty.String!);
            return 0;
        }
        if (context.DoubleBattleFirstSelectedPartySlot != CancelSlot && context.DoubleBattleFirstSelectedPartySlot == context.PokemonPartySlots[context.SelectedPartyIndex])
        {
            FormatNicknameMessage(battleParty, MsgCantSwitchAlreadySelected, battleParty.PartyPokemon[context.SelectedPartyIndex].Mon!);
            return 0;
        }
        if (context.MoveToLearn != 0)
        {
            FormatNicknameMessage(battleParty, MsgCantSwitchLearningMove, battleParty.PartyPokemon[battleParty.PartySlotLearningMove].Mon!);
            return 0;
        }
        return 1;
    }

    // OFFSET 0x0222194C
    private static byte CheckSelectedPokemonIsEgg(BattleParty battleParty)
    {
        return (byte)(battleParty.PartyPokemon[Index(battleParty)].IsEgg != 0 ? 1 : 0);
    }

    // OFFSET 0x0222196C
    public static int BattlePartyTask_CheckIfPartnerBattle(BattleParty battleParty)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleParty.Context!.BattleSys!);
        if (battleType == BattleTypeLinkTag1 || battleType == BattleTypeLinkTag2)
        {
            return 0;
        }
        return (battleType & BattleTypePartnerMask) != 0 ? 1 : 0;
    }

    // OFFSET 0x0222198C
    public static int BattlePartyTask_CheckIf2V2Battle(BattleParty battleParty)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleParty.Context!.BattleSys!);
        if (battleType == BattleTypeLinkTag1 || battleType == BattleTypeLinkTag2)
        {
            return 0;
        }
        return (battleType & BattleTypeTwoVsTwoMask) != 0 ? 1 : 0;
    }

    // OFFSET 0x022219AC
    public static int BattlePartyTask_CheckIfSwitchingWithPartnersPokemon(BattleParty battleParty, byte partySlot)
    {
        if (BattlePartyTask_CheckIf2V2Battle(battleParty) != 1)
        {
            return 0;
        }
        if (battleParty.BattlerSlot == 2)
        {
            return (partySlot & 1) != 0 ? 1 : 0;
        }
        return (partySlot & 1) == 0 ? 1 : 0;
    }

    // OFFSET 0x022219DC
    private static int CheckSelectedMoveIsHM(BattleParty battleParty)
    {
        BattlePartyContext context = battleParty.Context!;
        ushort move = context.SelectedMoveSlot == MoveToLearnSlot ? context.MoveToLearn : battleParty.PartyPokemon[context.SelectedPartyIndex].Moves[context.SelectedMoveSlot].Move;
        return Item.Item_IsHMMove(move);
    }

    // OFFSET 0x02221A04
    private static void ClearMoveStats(BattleParty battleParty)
    {
        SpriteSystem.ManagedSprite_SetDrawFlag(battleParty.Sprites[26]!, 0);
        Bg.Window_ClearAndScheduleCopyToVRAM(battleParty.Windows![10]);
        Bg.Window_ClearAndScheduleCopyToVRAM(battleParty.Windows[6]);
        Bg.Window_ClearAndScheduleCopyToVRAM(battleParty.Windows[7]);
    }

    // OFFSET 0x02221A3C
    private static void ClearMoveContestStats(BattleParty battleParty)
    {
        DrawEmptyAppealPts(battleParty);
        Bg.Bg_ScheduleTilemapTransfer(battleParty.Background!, 7);
    }

    // OFFSET 0x02221A54
    private static void UseBagItem(BattleSystem battleSys, ushort item, ushort pocket, int heapID)
    {
        if (item != ItemFluffyTail && item != ItemPokeToy && item != ItemPokeDoll)
        {
            BagItems.Bag_TryRemoveItem(BattleSystems.BattleSystem_GetBag(battleSys)!, item, 1, heapID);
        }
        BagCursor.BagCursor_SetLastUsedBattleItem(BattleSystems.BattleSystem_GetBagCursor(battleSys)!, item, pocket);
    }
}
