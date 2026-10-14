namespace PlatinumSharp.Game;

public static partial class BattleBags
{
    public const int ScreenMenu = 0;
    public const int ScreenPocketMenu = 1;
    public const int ScreenUseItem = 2;

    public const int PocketRecoverHP = 0;
    public const int PocketRecoverStatus = 1;
    public const int PocketPokeBalls = 2;
    public const int PocketBattleItems = 3;
    public const int PocketNone = 4;

    private const int StateInitialize = 0;
    private const int StateMenuScreen = 1;
    private const int StatePocketMenuScreen = 2;
    private const int StateUseItemScreen = 3;
    private const int StateSetupMenuScreen = 4;
    private const int StateSetupPocketMenuScreen = 5;
    private const int StateSetupUseItemScreen = 6;
    private const int StateChangePocketPage = 7;
    private const int StateClearErrorMessage = 8;
    private const int StateAwaitingTextFinish = 9;
    private const int StateAwaitingInput = 10;
    private const int StateScreenTransition = 11;
    private const int StateCatchTutorial = 12;
    private const int StateExit = 13;
    private const int StateFinishTask = 14;

    private const int SeqSelect = 0x5DD;
    private const int NarcBattleBagGraphics = 0x4D;
    private const int NarcWindowGraphics = 0x26;
    private const int NarcObjPalettes = 0xE;
    private const int FontSubscreen = 2;
    private const int PaletteBufferSubBg = 1;
    private const int ItemsPerPage = 6;
    private const int ItemParamBattleUseCategory = 7;
    private const int ItemParamBattlePockets = 0xD;
    private const int ItemGuardSpec = 0x37;
    private const int MoveEmbargo = 0x175;
    private const uint BattleTypeTrainer = 1;
    private const uint BattleTypeCatchTutorial = 0x400;
    private const int MaxPartySize = 6;
    private const int MaxPCBoxes = 0x12;
    private const int BagPockets = 8;
    private const int TextBankCommonStrings = 0xD5;
    private const int CommonStringsCantDoThatRightNow = 0x24;
    private const int MsgEmbargoBlockingItemUse = 0x2E;
    private const int MsgItemHasNoUse = 0x22;
    private const int MsgCantUseBallTwoPokemon = 0x2C;
    private const int MsgCantUseBallNoRoomLeft = 0x2D;
    private const int MsgCantUseBallPokemonHidden = 0x2F;
    private const int MsgCantUseBallPokemonSubstituted = 0x30;
    private const int PadButtonA = 1;
    private const int PadButtonB = 2;
    private const int BattleContextPartySlot = 2;

    private const uint MenuTouchRects = 0x02229A1C;
    private const uint PocketMenuTouchRects = 0x02229A38;
    private const uint UseItemTouchRects = 0x022299AC;
    private const uint GraphicsModesAddress = 0x022299B8;
    private const uint BgTemplateLayer6 = 0x02229A00;
    private const uint BgTemplateLayer5 = 0x022299E4;
    private const uint BgTemplateLayer4 = 0x022299C8;
    private const uint ItemBattlePocketMasks = 0x02229BB0;

    // OFFSET 0x022264F4
    public static void BattleBagTask_Start(BattleBagContext context)
    {
        SysTask task = SysTaskExtensions.SysTask_StartAndAllocateParam<BattleBag>(BattleBagTask_Tick, BattleBag.Size, 100, context.HeapID)!;
        BattleBag battleBag = (BattleBag)SysTaskManager.SysTask_GetParam(task)!;
        battleBag.Context = context;
        battleBag.Background = BattleSystems.BattleSystem_GetBgConfig(context.BattleSys!);
        battleBag.Palette = BattleSystems.BattleSystem_GetPaletteData(context.BattleSys!);
        battleBag.CurrentState = StateInitialize;
        BagCursor cursor = BattleSystems.BattleSystem_GetBagCursor(context.BattleSys!)!;
        for (byte i = 0; i < BattleBagContext.NumPockets; i++)
        {
            BagCursor.BagCursor_GetBattleCategoryPosition(cursor, i, out battleBag.Context.PocketCurrentPagePositions[i], out battleBag.Context.PocketCurrentPages[i]);
        }
        battleBag.Context.LastUsedItem = BagCursor.BagCursor_GetLastUsedBattleItem(cursor);
        battleBag.Context.LastUsedItemPocket = (byte)BagCursor.BagCursor_GetLastUsedBattleItemCategory(cursor);
        BattleBag_CanUseLastUsedItem(battleBag);
        if ((BattleSystems.BattleSystem_GetBattleType(battleBag.Context.BattleSys!) & BattleTypeCatchTutorial) != 0)
        {
            battleBag.Context.IsInCatchTutorial = 1;
        }
    }

    // OFFSET 0x02226590
    private static void BattleBagTask_Tick(SysTask task, object? param)
    {
        BattleBag battleBag = (BattleBag)param!;
        switch (battleBag.CurrentState)
        {
            case StateInitialize:
                battleBag.CurrentState = BattleBagTask_Initialize(battleBag);
                break;
            case StateMenuScreen:
                battleBag.CurrentState = BattleBagTask_MenuScreen(battleBag);
                break;
            case StatePocketMenuScreen:
                battleBag.CurrentState = BattleBagTask_PocketMenuScreen(battleBag);
                break;
            case StateUseItemScreen:
                battleBag.CurrentState = BattleBagTask_UseItemScreen(battleBag);
                break;
            case StateSetupMenuScreen:
                battleBag.CurrentState = BattleBagTask_SetupMenuScreen(battleBag);
                break;
            case StateSetupPocketMenuScreen:
                battleBag.CurrentState = BattleBagTask_SetupPocketMenuScreen(battleBag);
                break;
            case StateSetupUseItemScreen:
                battleBag.CurrentState = BattleBagTask_SetupUseItemScreen(battleBag);
                break;
            case StateChangePocketPage:
                battleBag.CurrentState = BattleBagTask_ChangePocketPage(battleBag);
                break;
            case StateClearErrorMessage:
                battleBag.CurrentState = BattleBagTask_ClearErrorMessage(battleBag);
                break;
            case StateAwaitingTextFinish:
                battleBag.CurrentState = BattleBagTask_AwaitingTextFinish(battleBag);
                break;
            case StateAwaitingInput:
                battleBag.CurrentState = BattleBagTask_AwaitingInput(battleBag);
                break;
            case StateScreenTransition:
                battleBag.CurrentState = BattleBagTask_ScreenTransition(battleBag);
                break;
            case StateCatchTutorial:
                battleBag.CurrentState = BattleBagTask_CatchTutorial(battleBag);
                break;
            case StateExit:
                battleBag.CurrentState = BattleBagTask_Exit(battleBag);
                break;
            case StateFinishTask:
                if (BattleBagTask_FinishTask(task, battleBag) == 1)
                {
                    return;
                }
                break;
        }
        BattleBagButtons_Tick(battleBag);
        SpriteSystem.SpriteSystem_DrawSprites(battleBag.SpriteManager);
    }

    // OFFSET 0x0222668C
    private static byte BattleBagTask_Initialize(BattleBag battleBag)
    {
        Video.SetBlendControl(1, 0);
        BattleBagContext context = battleBag.Context!;
        battleBag.Cursor = BattleSubMenuCursor.MakeBattleSubMenuCursor(context.HeapID);
        InitializeBackground(battleBag);
        LoadGraphicsData(battleBag);
        InitializeMessageLoader(battleBag);
        Font.Font_InitManager(FontSubscreen, context.HeapID);
        battleBag.CurrentBattlePocket = (byte)BagCursor.BagCursor_GetBattleCurrentCategory(BattleSystems.BattleSystem_GetBagCursor(context.BattleSys!)!);
        BattleBag_Init(battleBag);
        BattleBagButtons_InitializeButtons(battleBag, battleBag.CurrentScreen);
        BattleBagText_InitializeWindows(battleBag);
        BattleBagText_ChangeScreen(battleBag, battleBag.CurrentScreen);
        BattleBagSprites_InitializeSprites(battleBag);
        BattleBagSprites_SetupScreen(battleBag, battleBag.CurrentScreen);
        if (context.IsCursorEnabled != 0)
        {
            BattleSubMenuCursor.SetBattleSubMenuCursorVisibility(battleBag.Cursor, 1);
        }
        BattleBagSprites_SetupCursor(battleBag, battleBag.CurrentScreen);
        BattleBagSprites_SetupCatchTutorialCursor(battleBag, battleBag.CurrentScreen);
        PaletteData.PaletteData_StartFade(battleBag.Palette!, 0xA, 0xFFFF, -8, 0x10, 0, 0);
        if (context.IsInCatchTutorial == 1)
        {
            return StateCatchTutorial;
        }
        return StateMenuScreen;
    }

    private static int ReadInput(BattleBag battleBag, uint rects, int cancelInput)
    {
        int input = CheckTouchRectIsPressed(battleBag, BattleSubMenuData.TouchRects(rects));
        if (input == TouchScreen.ButtonNotPressed)
        {
            input = BattleSubMenuCursor.BattleSubMenuCursorTick(battleBag.Cursor!);
            if (input == BattleSubMenuCursor.Cancelled)
            {
                input = cancelInput;
            }
        }
        else
        {
            BattleBagSprites_DisableCursor(battleBag);
        }
        return input;
    }

    // OFFSET 0x02226760
    private static byte BattleBagTask_MenuScreen(BattleBag battleBag)
    {
        if (PaletteData.PaletteData_GetSelectedBuffersMask(battleBag.Palette!) != 0)
        {
            return StateMenuScreen;
        }
        int input = ReadInput(battleBag, MenuTouchRects, 5);
        BattleBagContext context = battleBag.Context!;
        switch (input)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                battleBag.CurrentBattlePocket = (byte)input;
                battleBag.QueuedState = StateSetupPocketMenuScreen;
                BattleBagButtons_PressButton(battleBag, input, ScreenMenu);
                return StateScreenTransition;
            case 4:
                if (context.LastUsedItem == 0)
                {
                    break;
                }
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                battleBag.CurrentBattlePocket = context.LastUsedItemPocket;
                battleBag.QueuedState = StateSetupUseItemScreen;
                BattleBag_SetLastUsedPocket(battleBag);
                BattleBagButtons_PressButton(battleBag, 4, ScreenMenu);
                return StateScreenTransition;
            case 5:
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                context.SelectedBattleBagItem = 0;
                context.SelectedBattleBagPocket = PocketNone;
                BattleBagButtons_PressButton(battleBag, 5, ScreenMenu);
                return StateExit;
        }
        return StateMenuScreen;
    }

    // OFFSET 0x02226838
    private static byte BattleBagTask_PocketMenuScreen(BattleBag battleBag)
    {
        int input = ReadInput(battleBag, PocketMenuTouchRects, 8);
        BattleBagContext context = battleBag.Context!;
        switch (input)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                if (BattleBag_GetItem(battleBag, input) == 0)
                {
                    break;
                }
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                context.PocketCurrentPagePositions[battleBag.CurrentBattlePocket] = (byte)input;
                battleBag.QueuedState = StateSetupUseItemScreen;
                BattleBagButtons_PressButton(battleBag, input + 6, ScreenMenu);
                return StateScreenTransition;
            case 6:
                if (battleBag.NumBattlePocketPages[battleBag.CurrentBattlePocket] == 0)
                {
                    break;
                }
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                battleBag.QueuedState = StateChangePocketPage;
                battleBag.QueuedBattlePocketPageChange = -1;
                BattleBagButtons_PressButton(battleBag, 0xC, ScreenMenu);
                return StateScreenTransition;
            case 7:
                if (battleBag.NumBattlePocketPages[battleBag.CurrentBattlePocket] == 0)
                {
                    break;
                }
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                battleBag.QueuedState = StateChangePocketPage;
                battleBag.QueuedBattlePocketPageChange = 1;
                BattleBagButtons_PressButton(battleBag, 0xD, ScreenMenu);
                return StateScreenTransition;
            case 8:
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                battleBag.QueuedState = StateSetupMenuScreen;
                BattleBagButtons_PressButton(battleBag, 0xE, ScreenMenu);
                return StateScreenTransition;
        }
        return StatePocketMenuScreen;
    }

    // OFFSET 0x02226948
    private static byte BattleBagTask_ChangePocketPage(BattleBag battleBag)
    {
        BattleBagContext context = battleBag.Context!;
        int pocket = battleBag.CurrentBattlePocket;
        sbyte currentPage = (sbyte)context.PocketCurrentPages[pocket];
        context.PocketCurrentPagePositions[pocket] = 0;
        sbyte page = (sbyte)(currentPage + battleBag.QueuedBattlePocketPageChange);
        byte numPages = battleBag.NumBattlePocketPages[pocket];
        if (page > numPages)
        {
            context.PocketCurrentPages[pocket] = 0;
        }
        else if (page < 0)
        {
            context.PocketCurrentPages[pocket] = numPages;
        }
        else
        {
            context.PocketCurrentPages[pocket] = (byte)page;
        }
        BattleBagText_PrintAllPocketItemInfo(battleBag);
        BattleBagText_PrintPocketPageNums(battleBag);
        BattleBagSprites_SetupScreen(battleBag, battleBag.CurrentScreen);
        BattleBagButtons_InitializeButtons(battleBag, battleBag.CurrentScreen);
        return StatePocketMenuScreen;
    }

    // OFFSET 0x022269C0
    private static byte BattleBagTask_UseItemScreen(BattleBag battleBag)
    {
        int input = ReadInput(battleBag, UseItemTouchRects, 1);
        BattleBagContext context = battleBag.Context!;
        if (input == 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqSelect);
            context.SelectedBattleBagItem = BattleBag_GetItem(battleBag, context.PocketCurrentPagePositions[battleBag.CurrentBattlePocket]);
            context.SelectedBattleBagPocket = battleBag.CurrentBattlePocket;
            BattleBagButtons_PressButton(battleBag, 0xF, ScreenMenu);
            return TryUseItem(battleBag);
        }
        if (input == 1)
        {
            SoundPlayback.Sound_PlayEffect(SeqSelect);
            battleBag.QueuedState = StateSetupPocketMenuScreen;
            BattleBagButtons_PressButton(battleBag, 0x10, ScreenMenu);
            return StateScreenTransition;
        }
        return StateUseItemScreen;
    }

    private static byte ShowErrorMessage(BattleBag battleBag)
    {
        BattleBagText_DisplayMessage(battleBag);
        battleBag.QueuedState = StateClearErrorMessage;
        return StateAwaitingTextFinish;
    }

    private static byte ShowErrorMessage(BattleBag battleBag, int messageID)
    {
        MessageLoader.MessageLoader_GetString(battleBag.MessageLoader!, (uint)messageID, battleBag.String!);
        return ShowErrorMessage(battleBag);
    }

    // OFFSET 0x02226A5C
    private static byte TryUseItem(BattleBag battleBag)
    {
        BattleBagContext context = battleBag.Context!;
        if (battleBag.CurrentBattlePocket == PocketBattleItems)
        {
            int partySlot = BattleBagTask_GetSelectedPartySlot(battleBag);
            int itemBattleUse = Item.Item_LoadParam(context.SelectedBattleBagItem, ItemParamBattleUseCategory, context.HeapID);
            if (context.EmbargoRemainingTurns != 0 && context.SelectedBattleBagItem != ItemGuardSpec && itemBattleUse != 3)
            {
                Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(context.BattleSys!, context.Battler, partySlot);
                GameString format = MessageLoader.MessageLoader_GetNewString(battleBag.MessageLoader!, MsgEmbargoBlockingItemUse)!;
                StringTemplate.StringTemplate_SetNickname(battleBag.StringTemplate!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
                StringTemplate.StringTemplate_SetMoveName(battleBag.StringTemplate!, 1, MoveEmbargo);
                StringTemplate.StringTemplate_Format(battleBag.StringTemplate!, battleBag.String!, format);
                GameString.String_Free(format);
                return ShowErrorMessage(battleBag);
            }
            if (BattleSystems.BattleSystem_UseBagItem(context.BattleSys!, context.Battler, partySlot, 0, context.SelectedBattleBagItem) == 1)
            {
                UseBagItem(context.BattleSys!, context.SelectedBattleBagItem, battleBag.CurrentBattlePocket, context.HeapID);
                return StateExit;
            }
            if (itemBattleUse == 3)
            {
                if ((BattleSystems.BattleSystem_GetBattleType(context.BattleSys!) & BattleTypeTrainer) == 0)
                {
                    UseBagItem(context.BattleSys!, context.SelectedBattleBagItem, battleBag.CurrentBattlePocket, context.HeapID);
                    return StateExit;
                }
                MessageLoader loader = MessageLoader.MessageLoader_Init(1, 0x1A, TextBankCommonStrings, context.HeapID)!;
                GameString format = MessageLoader.MessageLoader_GetNewString(loader, CommonStringsCantDoThatRightNow)!;
                StringTemplate.StringTemplate_SetPlayerName(battleBag.StringTemplate!, 0, context.TrainerInfo!);
                StringTemplate.StringTemplate_Format(battleBag.StringTemplate!, battleBag.String!, format);
                GameString.String_Free(format);
                MessageLoader.MessageLoader_Free(loader);
                return ShowErrorMessage(battleBag);
            }
            return ShowErrorMessage(battleBag, MsgItemHasNoUse);
        }
        if (battleBag.CurrentBattlePocket == PocketPokeBalls)
        {
            if (context.HasTwoOpponents == 1)
            {
                return ShowErrorMessage(battleBag, MsgCantUseBallTwoPokemon);
            }
            if (context.OpponentHidden == 1)
            {
                return ShowErrorMessage(battleBag, MsgCantUseBallPokemonHidden);
            }
            if (context.OpponentSubstituted == 1)
            {
                return ShowErrorMessage(battleBag, MsgCantUseBallPokemonSubstituted);
            }
            Party party = BattleSystems.BattleSystem_GetParty(context.BattleSys!, context.Battler);
            PCBoxes boxes = BattleSystems.BattleSystem_GetPCBoxes(context.BattleSys!)!;
            if (Party.Party_GetCurrentCount(party) == MaxPartySize && PCBoxes.PCBoxes_FirstEmptyBox(boxes) == MaxPCBoxes)
            {
                return ShowErrorMessage(battleBag, MsgCantUseBallNoRoomLeft);
            }
        }
        return StateExit;
    }

    // OFFSET 0x02226C48
    private static byte BattleBagTask_SetupMenuScreen(BattleBag battleBag)
    {
        ChangeBattleBagScreen(battleBag, ScreenMenu);
        return StateMenuScreen;
    }

    // OFFSET 0x02226C54
    private static byte BattleBagTask_SetupPocketMenuScreen(BattleBag battleBag)
    {
        ChangeBattleBagScreen(battleBag, ScreenPocketMenu);
        return StatePocketMenuScreen;
    }

    // OFFSET 0x02226C60
    private static byte BattleBagTask_SetupUseItemScreen(BattleBag battleBag)
    {
        ChangeBattleBagScreen(battleBag, ScreenUseItem);
        return StateUseItemScreen;
    }

    // OFFSET 0x02226C6C
    private static byte BattleBagTask_ClearErrorMessage(BattleBag battleBag)
    {
        RenderWindow.Window_EraseMessageBox(battleBag.MessageBoxWindow, 0);
        return StateUseItemScreen;
    }

    // OFFSET 0x02226C7C
    private static byte BattleBagTask_AwaitingTextFinish(BattleBag battleBag)
    {
        if (Text.Text_IsPrinterActive(battleBag.TextPrinterID) == 0)
        {
            return StateAwaitingInput;
        }
        return StateAwaitingTextFinish;
    }

    // OFFSET 0x02226C94
    private static byte BattleBagTask_AwaitingInput(BattleBag battleBag)
    {
        if ((Globals.gSystem.PressedKeys & (PadButtonA | PadButtonB)) != 0 || TouchScreen.TouchScreen_Tapped())
        {
            return battleBag.QueuedState;
        }
        return StateAwaitingInput;
    }

    // OFFSET 0x02226CBC
    private static byte BattleBagTask_ScreenTransition(BattleBag battleBag)
    {
        if (battleBag.PressedButtonState == 2)
        {
            return battleBag.QueuedState;
        }
        return StateScreenTransition;
    }

    // OFFSET 0x02226CD4
    private static byte BattleBagTask_Exit(BattleBag battleBag)
    {
        PaletteData.PaletteData_StartFade(battleBag.Palette!, 0xA, 0xFFFF, -8, 0, 0x10, 0);
        return StateFinishTask;
    }

    // OFFSET 0x02226CFC
    private static int BattleBagTask_FinishTask(SysTask task, BattleBag battleBag)
    {
        if (PaletteData.PaletteData_GetSelectedBuffersMask(battleBag.Palette!) != 0)
        {
            return 0;
        }
        BattleBagSprites_CleanupSprites(battleBag);
        BattleBagText_ClearWindows(battleBag);
        CleanupMessageLoader(battleBag);
        CleanupBackground(battleBag.Background!);
        BattleBagContext context = battleBag.Context!;
        context.IsCursorEnabled = (byte)BattleSubMenuCursor.IsBattleSubMenuCursorVisible(battleBag.Cursor!);
        BattleSubMenuCursor.DeleteBattleSubMenuCursor(battleBag.Cursor!);
        Font.Font_Free(FontSubscreen);
        if (context.SelectedBattleBagItem != 0)
        {
            BagCursor cursor = BattleSystems.BattleSystem_GetBagCursor(context.BattleSys!)!;
            for (byte i = 0; i < BattleBagContext.NumPockets; i++)
            {
                BagCursor.BagCursor_SetBattleCategoryPosition(cursor, i, context.PocketCurrentPagePositions[i], context.PocketCurrentPages[i]);
            }
            BagCursor.BagCursor_SetBattleCurrentCategory(cursor, battleBag.CurrentBattlePocket);
        }
        context.BattleBagExited = 1;
        SysTaskExtensions.SysTask_FinishAndFreeParam(task);
        return 1;
    }

    private static bool CatchTutorialTapped(BattleBag battleBag)
    {
        if (BattleSubscreens.TouchIndicator_IsTapped(battleBag.CatchTutorialCursor!) == 1)
        {
            return true;
        }
        battleBag.CatchTutorialTickCount++;
        return false;
    }

    // OFFSET 0x02226D94
    private static byte BattleBagTask_CatchTutorial(BattleBag battleBag)
    {
        if (PaletteData.PaletteData_GetSelectedBuffersMask(battleBag.Palette!) != 0)
        {
            return StateCatchTutorial;
        }
        BattleBagContext context = battleBag.Context!;
        switch (battleBag.CatchTutorialState)
        {
            case 0:
                if (CatchTutorialTapped(battleBag))
                {
                    SoundPlayback.Sound_PlayEffect(SeqSelect);
                    battleBag.CurrentBattlePocket = PocketPokeBalls;
                    battleBag.QueuedState = StateCatchTutorial;
                    BattleBagButtons_PressButton(battleBag, PocketPokeBalls, ScreenMenu);
                    battleBag.CatchTutorialTickCount = 0;
                    battleBag.CatchTutorialState++;
                    return StateScreenTransition;
                }
                break;
            case 1:
                BattleBagTask_SetupPocketMenuScreen(battleBag);
                battleBag.CatchTutorialState++;
                break;
            case 2:
                if (CatchTutorialTapped(battleBag))
                {
                    SoundPlayback.Sound_PlayEffect(SeqSelect);
                    context.PocketCurrentPagePositions[battleBag.CurrentBattlePocket] = 0;
                    battleBag.QueuedState = StateCatchTutorial;
                    BattleBagButtons_PressButton(battleBag, 6, ScreenMenu);
                    battleBag.CatchTutorialTickCount = 0;
                    battleBag.CatchTutorialState++;
                    return StateScreenTransition;
                }
                break;
            case 3:
                BattleBagTask_SetupUseItemScreen(battleBag);
                battleBag.CatchTutorialState++;
                break;
            case 4:
                if (CatchTutorialTapped(battleBag))
                {
                    SoundPlayback.Sound_PlayEffect(SeqSelect);
                    context.SelectedBattleBagItem = BattleBag_GetItem(battleBag, context.PocketCurrentPagePositions[battleBag.CurrentBattlePocket]);
                    context.SelectedBattleBagPocket = battleBag.CurrentBattlePocket;
                    BattleBagButtons_PressButton(battleBag, 0xF, ScreenMenu);
                    return TryUseItem(battleBag);
                }
                break;
        }
        return StateCatchTutorial;
    }

    // OFFSET 0x02226ED0
    private static void InitializeBackground(BattleBag battleBag)
    {
        Bg.SetScreenGraphicsModes(BattleSubMenuData.GraphicsModes(GraphicsModesAddress), 1);
        Bg.Bg_InitFromTemplate(battleBag.Background!, 6, BattleSubMenuData.BgTemplate(BgTemplateLayer6), 0);
        Bg.Bg_InitFromTemplate(battleBag.Background!, 5, BattleSubMenuData.BgTemplate(BgTemplateLayer5), 0);
        Bg.Bg_ClearTilemap(battleBag.Background!, 5);
        Bg.Bg_InitFromTemplate(battleBag.Background!, 4, BattleSubMenuData.BgTemplate(BgTemplateLayer4), 0);
        Bg.Bg_ClearTilemap(battleBag.Background!, 4);
        Bg.Bg_ClearTilesRange(5, 0x20, 0, battleBag.Context!.HeapID);
        Bg.Bg_ClearTilesRange(4, 0x20, 0, battleBag.Context.HeapID);
        Bg.Bg_ScheduleTilemapTransfer(battleBag.Background!, 5);
        Bg.Bg_ScheduleTilemapTransfer(battleBag.Background!, 4);
    }

    // OFFSET 0x02226F9C
    private static void CleanupBackground(BgConfig background)
    {
        GXLayers.GXLayers_EngineBToggleLayers(0x17, 0);
        Bg.Bg_FreeTilemapBuffer(background, 4);
        Bg.Bg_FreeTilemapBuffer(background, 5);
        Bg.Bg_FreeTilemapBuffer(background, 6);
    }

    // OFFSET 0x02226FC4
    private static void LoadGraphicsData(BattleBag battleBag)
    {
        int heapID = battleBag.Context!.HeapID;
        NARC narc = NARC.NARC_ctor(NarcBattleBagGraphics, heapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 2, battleBag.Background!, 6, 0, 0, 0, heapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0, battleBag.Background!, 6, 0, 0, 0, heapID);
        byte[] file = NARC.NARC_AllocAndReadWholeMember(narc, 1, heapID);
        G2d.GetUnpackedScreenData(file, out ScreenData? screenData);
        BattleBagButtons_InitializeButtonData(battleBag, screenData!.RawData);
        NARC.NARC_dtor(narc);
        PaletteData.PaletteData_LoadBufferFromFileStart(battleBag.Palette!, NarcBattleBagGraphics, 3, heapID, PaletteBufferSubBg, 0x180, 0);
        PaletteData.PaletteData_LoadBufferFromFileStart(battleBag.Palette!, NarcObjPalettes, 7, heapID, PaletteBufferSubBg, 0x20, 0xF0);
        uint frame = (uint)BattleSystems.BattleSystem_GetOptionsFrame(battleBag.Context.BattleSys!);
        Graphics.Graphics_LoadTilesToBgLayer(NarcWindowGraphics, RenderWindow.GetMessageBoxTilesNARCMember(frame), battleBag.Background!, 4, 0x3E2, 0, 0, heapID);
        PaletteData.PaletteData_LoadBufferFromFileStart(battleBag.Palette!, NarcWindowGraphics, (int)RenderWindow.GetMessageBoxPaletteNARCMember(frame), heapID, PaletteBufferSubBg, 0x20, 0xE0);
    }

    // OFFSET 0x022270B8
    private static void InitializeMessageLoader(BattleBag battleBag)
    {
        int heapID = battleBag.Context!.HeapID;
        battleBag.MessageLoader = MessageLoader.MessageLoader_Init(0, 0x1A, 2, heapID);
        battleBag.Unused1 = FontSpecialCharsContext.FontSpecialChars_Init(0xF, 0xE, 0, heapID);
        battleBag.StringTemplate = StringTemplate.StringTemplate_Default(heapID);
        battleBag.String = GameString.String_Init(0x200, heapID);
    }

    // OFFSET 0x022270F8
    private static void CleanupMessageLoader(BattleBag battleBag)
    {
        MessageLoader.MessageLoader_Free(battleBag.MessageLoader);
        FontSpecialCharsContext.FontSpecialChars_Free(battleBag.Unused1);
        StringTemplate.StringTemplate_Free(battleBag.StringTemplate);
        GameString.String_Free(battleBag.String);
    }

    // OFFSET 0x02227118
    private static void SetupBackgroundScroll(BattleBag battleBag, int screen)
    {
        switch (screen)
        {
            case ScreenMenu:
                Bg.Bg_ScheduleScroll(battleBag.Background!, 6, 0, 0);
                Bg.Bg_ScheduleScroll(battleBag.Background!, 6, 3, 0);
                break;
            case ScreenPocketMenu:
                Bg.Bg_ScheduleScroll(battleBag.Background!, 6, 0, 0x100);
                Bg.Bg_ScheduleScroll(battleBag.Background!, 6, 3, 0);
                break;
            case ScreenUseItem:
                Bg.Bg_ScheduleScroll(battleBag.Background!, 6, 0, 0);
                Bg.Bg_ScheduleScroll(battleBag.Background!, 6, 3, 0x100);
                break;
        }
    }

    // OFFSET 0x0222717C
    private static void SetupBackgroundTilemap(BattleBag battleBag, int screen)
    {
        if (screen != ScreenUseItem)
        {
            return;
        }
        Bg.Bg_ChangeTilemapRectPalette(battleBag.Background!, 6, 2, 0x23, 0x1C, 4, (byte)(battleBag.CurrentBattlePocket + 8));
        Bg.Bg_ChangeTilemapRectPalette(battleBag.Background!, 6, 2, 0x28, 0x1C, 8, (byte)(battleBag.CurrentBattlePocket + 8));
    }

    // OFFSET 0x022271D0
    private static void ChangeBattleBagScreen(BattleBag battleBag, byte screen)
    {
        SetupBackgroundTilemap(battleBag, screen);
        SetupBackgroundScroll(battleBag, screen);
        Bg.Bg_ScheduleFillTilemap(battleBag.Background!, 4, 0);
        Bg.Bg_ScheduleFillTilemap(battleBag.Background!, 5, 0);
        BattleBagText_ClearScreenWindows(battleBag);
        BattleBagText_InitializeScreenWindows(battleBag, screen);
        BattleBagText_ChangeScreen(battleBag, screen);
        BattleBagButtons_InitializeButtons(battleBag, screen);
        BattleBagSprites_SetupCursor(battleBag, screen);
        BattleBagSprites_SetupCatchTutorialCursor(battleBag, screen);
        battleBag.CurrentScreen = screen;
        BattleBagSprites_SetupScreen(battleBag, battleBag.CurrentScreen);
    }

    // OFFSET 0x02227238
    private static int CheckTouchRectIsPressed(BattleBag battleBag, TouchScreenHitTable[] rects)
    {
        return TouchScreen.TouchScreen_CheckRectanglePressed(rects);
    }

    // OFFSET 0x02227244
    public static int BattleBagTask_GetSelectedPartySlot(BattleBag battleBag)
    {
        BattleBagContext context = battleBag.Context!;
        BattleContext battleCtx = BattleSystems.BattleSystem_GetBattleContext(context.BattleSys!)!;
        return BattleContexts.BattleContext_Get(context.BattleSys!, battleCtx, BattleContextPartySlot, context.Battler);
    }

    // OFFSET 0x02227260
    private static void UseBagItem(BattleSystem battleSys, ushort item, ushort pocket, int heapID)
    {
        BagItems.Bag_TryRemoveItem(BattleSystems.BattleSystem_GetBag(battleSys)!, item, 1, heapID);
        BagCursor.BagCursor_SetLastUsedBattleItem(BattleSystems.BattleSystem_GetBagCursor(battleSys)!, item, pocket);
    }

    // OFFSET 0x02227A4C
    private static byte BattleBag_CanUseLastUsedItem(BattleBag battleBag)
    {
        BattleBagContext context = battleBag.Context!;
        if (context.LastUsedItem == 0)
        {
            return 0;
        }
        if (!BagItems.Bag_CanRemoveItem(context.Bag!, context.LastUsedItem, 1, context.HeapID))
        {
            context.LastUsedItem = 0;
            context.LastUsedItemPocket = 0;
            return 0;
        }
        return 1;
    }

    // OFFSET 0x02227A7C
    private static void BattleBag_SetLastUsedPocket(BattleBag battleBag)
    {
        BattleBagContext context = battleBag.Context!;
        int pocket = battleBag.CurrentBattlePocket;
        for (uint i = 0; i < BattleBag.PocketSize; i++)
        {
            if (context.LastUsedItem == battleBag.Items[pocket][i].Item)
            {
                context.PocketCurrentPagePositions[pocket] = (byte)(i % ItemsPerPage);
                context.PocketCurrentPages[battleBag.CurrentBattlePocket] = (byte)(i / ItemsPerPage);
                return;
            }
        }
    }

    // OFFSET 0x02227AC8
    private static void BattleBag_Init(BattleBag battleBag)
    {
        BattleBagContext context = battleBag.Context!;
        byte[] pocketMasks = BattleSubMenuData.Bytes(ItemBattlePocketMasks, BattleBagContext.NumPockets);
        for (int bagPocket = 0; bagPocket < BagPockets; bagPocket++)
        {
            for (int slot = 0; ; slot++)
            {
                BufferView? itemSlot = BagItems.Bag_GetItemSlot(context.Bag!, (ushort)bagPocket, (ushort)slot);
                if (itemSlot == null)
                {
                    break;
                }
                ushort item = itemSlot.U16(0);
                ushort quantity = itemSlot.U16(2);
                if (item == 0 || quantity == 0)
                {
                    continue;
                }
                int battlePockets = Item.Item_LoadParam(item, ItemParamBattlePockets, context.HeapID);
                for (int i = 0; i < BattleBagContext.NumPockets; i++)
                {
                    if ((battlePockets & (1 << i)) == 0)
                    {
                        continue;
                    }
                    int pocket = pocketMasks[i];
                    BattleBagItem entry = battleBag.Items[pocket][battleBag.NumBattlePocketItems[pocket]];
                    entry.Item = item;
                    entry.Quantity = quantity;
                    battleBag.NumBattlePocketItems[pocket]++;
                }
            }
        }
        for (int i = 0; i < BattleBagContext.NumPockets; i++)
        {
            if (battleBag.NumBattlePocketItems[i] == 0)
            {
                battleBag.NumBattlePocketPages[i] = 0;
            }
            else
            {
                battleBag.NumBattlePocketPages[i] = (byte)((battleBag.NumBattlePocketItems[i] - 1) / ItemsPerPage);
            }
            if (battleBag.NumBattlePocketPages[i] < context.PocketCurrentPages[i])
            {
                context.PocketCurrentPages[i] = battleBag.NumBattlePocketPages[i];
            }
        }
    }

    // OFFSET 0x02227BA8
    private static ushort BattleBag_GetItem(BattleBag battleBag, int slot)
    {
        int pocket = battleBag.CurrentBattlePocket;
        BattleBagItem entry = battleBag.Items[pocket][slot + battleBag.Context!.PocketCurrentPages[pocket] * ItemsPerPage];
        if (entry.Item == 0 || entry.Quantity == 0)
        {
            return 0;
        }
        return entry.Item;
    }
}
