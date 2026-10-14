using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public static partial class PartyMenus
{
    public const uint PartyMenuTemplate = 0x020F1E88;

    private const int HeapID = 0xC;
    private const uint HeapSize = 0x30000;
    private const int PartyMenuArchive = 0x14;
    private const int PokemonIconArchive = 0x13;
    private const int MessageArchive = 0x1A;
    private const int PartyMenuTextBank = 0x1C5;
    private const int MaxPartySize = 6;
    private const uint AllLayersMask = 0xFFFFE0FF;
    private const int DisplayMain = 0;
    private const int DisplaySub = 1;
    private const int KeyA = 0x1;
    private const int KeyB = 0x2;
    private const int SeqConfirm = 0x5DC;
    private const ushort Disp3DShadingMask = 0xCFFD;
    private const ushort Disp3DMask = 0xCFFF;
    private const ushort Disp3DAntiAlias = 0x10;
    private const ushort Disp3DAlphaTestMask = 0xCFFB;
    private const ushort Disp3DAlphaBlend = 8;
    private const ushort Disp3DEdgeMarkingMask = 0xCFDF;
    private const int ClearDepthMax = 0x7FFF;
    private const int ClearPolygonID = 63;

    private const uint VramBanksAddress = 0x020F1CD0;
    private const uint GraphicsModesAddress = 0x020F1BC4;
    private const uint BgTemplateLayer0 = 0x020F1C40;
    private const uint BgTemplateLayer1 = 0x020F1BEC;
    private const uint BgTemplateLayer2 = 0x020F1C08;
    private const uint BgTemplateLayer3 = 0x020F1C24;
    private const uint BgTemplateLayer4 = 0x020F1C78;
    private const uint BgTemplateLayer5 = 0x020F1C5C;
    private const uint CursorPositionsMulti = 0x020F1CF8;
    private const uint CursorPositionsConfirm = 0x020F1D38;
    private const uint CursorPositionsNoCancel = 0x020F1D78;
    private const uint CursorPositionsStandard = 0x020F1DB8;
    private const int CursorPositionCount = 8;

    public const int ModeField = 0;
    public const int ModeBattle = 1;
    public const int ModeSelectConfirm = 2;
    public const int ModeSelectNoPrompt = 3;
    public const int ModeUnk04 = 4;
    public const int ModeUseItem = 5;
    public const int ModeTeachMove = 6;
    public const int ModeTeachMoveDone = 7;
    public const int ModeLevelMoveDone = 8;
    public const int ModeGiveItem = 9;
    public const int ModeGiveItemDone = 10;
    public const int ModeGiveMail = 11;
    public const int ModeGiveMailDone = 12;
    public const int ModeContest = 13;
    public const int ModeMailbox = 14;
    public const int ModeBallSeal = 15;
    public const int ModeUseEvoItem = 16;
    public const int ModeBattleTower = 17;
    public const int ModeDaycare = 18;
    public const int ModeNpcTrade = 19;
    public const int ModeFeedPoffin = 20;
    public const int ModeSelectEgg = 21;
    public const int ModeBattleHall = 22;
    public const int ModeBattleCastle = 23;
    private const int ModeHideCancelFlag = 0x80;

    private const int TypeMultiBattle = 2;

    private const int StateStart = 0;
    private const int StateDefault = 1;
    private const int StateHandlePartyMenuAction = 2;
    private const int StateWaitABPress = 3;
    private const int StateUseItem = 4;
    private const int StateExecCallback = 5;
    private const int StateWaitMoveListSelection = 6;
    private const int StateUseSacredAsh = 7;
    private const int StateGiveItem = 8;
    private const int StateShowItemSwapConfirmation = 9;
    private const int StateHeldItemSwap = 10;
    private const int StateConfirmItemUpdate = 11;
    private const int State12 = 12;
    private const int State13 = 13;
    private const int State14 = 14;
    private const int StateHandleContextMenuInput = 15;
    private const int State16 = 16;
    private const int StatePrinting = 17;
    private const int StatePrintingFormChange = 18;
    private const int StateFormChange = 19;
    private const int StateWaitABPress2 = 20;
    private const int StateTeachMove = 21;
    private const int StateTeachMoveHandleInput = 22;
    private const int StateWaitABPressChooseMons = 23;
    private const int StateShowMessageThenNextState = 24;
    private const int StateWaitABPressBeforeFade = 25;
    private const int StateDrawYesNoChoice = 26;
    private const int State27 = 27;
    private const int StateSelectSwitchSlot = 28;
    private const int StateDoSwitch = 29;
    private const int StateHPTransferFieldMove = 30;
    private const int StateWaitFormChange = 31;
    private const int StateFadeOut = 32;
    private const int StateWaitFadeOut = 33;

    private const int InputConfirm = 0;
    private const int InputCancel = 3;

    private const int SpriteCursorNormal = 6;
    private const int SpriteCursorSwitch = 7;
    private const int SpriteConfirmButton = 8;
    private const int SpriteCancelButton = 9;

    private const int WinShortMessage = 32;
    private const int WinMediumMessage = 33;
    private const int WinLongMessage = 34;
    private const int WinGiveItemOrMail = 35;
    private const int WinMoveList = 36;

    private const int TextChooseAPokemon = 0x1D;
    private const int TextGiveToWhichMon = 0x1F;
    private const int TextUseOnWhichMon = 0x20;
    private const int TextTeachWhichMon = 0x21;
    private const int TextChooseMonAndConfirm = 0x22;
    private const int TextNoEggsAllowed = 0x69;
    private const int TextSelectEgg = 0xC5;

    private const int MonDataSpecies = 5;
    private const int MonDataHeldItem = 6;
    private const int MonDataMove1 = 0x36;
    private const int MonDataIsEgg = 0x4C;
    private const int MonDataForm = 0x70;
    private const int MonDataLevel = 0xA1;
    private const int MonDataBallCapsuleID = 0xA2;
    private const int MonDataCurrentHP = 0xA3;
    private const int MonDataMaxHP = 0xA4;
    private const int MonDataNidoranHasNickname = 0xB0;

    private static GridMenuCursorPosition[] ReadCursorPositions(uint address)
    {
        byte[] raw = RomData.Arm9(address, CursorPositionCount * 8);
        GridMenuCursorPosition[] positions = new GridMenuCursorPosition[CursorPositionCount];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i].XCoord1 = raw[i * 8];
            positions[i].YCoord1 = raw[i * 8 + 1];
            positions[i].XCoord2 = raw[i * 8 + 2];
            positions[i].YCoord2 = raw[i * 8 + 3];
            positions[i].UpIndex = raw[i * 8 + 4];
            positions[i].DownIndex = raw[i * 8 + 5];
            positions[i].LeftIndex = raw[i * 8 + 6];
            positions[i].RightIndex = raw[i * 8 + 7];
        }
        return positions;
    }

    // OFFSET 0x0207E0B8
    public static int PartyMenu_Init(ApplicationManager appMan, ref int state)
    {
        GameSystem.SetVBlankCallback(null, null);
        GameSystem.DisableHBlank();
        GXLayers.GXLayers_DisableEngineALayers();
        GXLayers.GXLayers_DisableEngineBLayers();
        Video.SetDisplayControl(DisplayMain, Video.GetDisplayControl(DisplayMain) & AllLayersMask);
        Video.SetDisplayControl(DisplaySub, Video.GetDisplayControl(DisplaySub) & AllLayersMask);
        Video.SetBlendControl(DisplayMain, 0);
        Video.SetBlendControl(DisplaySub, 0);
        GameSystem.SetAutorepeat(4, 8);
        Heap.Heap_Create(3, HeapID, HeapSize);
        NARC narc = NARC.NARC_ctor(PartyMenuArchive, HeapID);
        PartyMenuApplication app = NewPartyMenuApplication(appMan);
        ScreenFadeControl.StartScreenFade(0, 3, 3, 0, 6, 1, HeapID);
        CheckCancellableMode(app);
        SetVRAMBanks();
        InitBgs(app.BgConfig!);
        LoadGraphics(app, narc);
        TouchPad.EnableTouchPad();
        TouchPad.InitializeTouchPad(4);
        Sound.Sound_SetSceneAndPlayBGM(0x39, 0, 0);
        PartyMenu_AddWindows(app);
        PartyMenu_InitSpriteResources(app);
        PartyMenu_InitSprites(app);
        SetupRequestedMode(app);
        SetupRequestedModePanels(app);
        InitAnimAndPaletteForSlot(app, app.CurrPartySlot, 1);
        PartyMenu partyMenu = app.PartyMenu!;
        int mode = partyMenu.Mode;
        if (mode == ModeUseItem || mode == ModeUseEvoItem)
        {
            if (!CheckItemSacredAsh(partyMenu.UsedItemID))
            {
                PartyMenu_PrintShortMessage(app, TextUseOnWhichMon, 1);
            }
        }
        else if (mode == ModeTeachMove)
        {
            PartyMenu_PrintShortMessage(app, TextTeachWhichMon, 1);
        }
        else if (mode == ModeGiveItem || mode == ModeMailbox)
        {
            PartyMenu_PrintShortMessage(app, TextGiveToWhichMon, 1);
        }
        else if (mode == ModeTeachMoveDone || mode == ModeLevelMoveDone || mode == ModeGiveMail || mode == ModeGiveMailDone)
        {
            SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 1);
        }
        else if (mode == ModeSelectConfirm || mode == ModeBattleTower)
        {
            PartyMenu_PrintShortMessage(app, TextChooseMonAndConfirm, 1);
        }
        else if (mode == ModeBallSeal)
        {
            PartyMenu_PrintShortMessage(app, TextChooseMonAndConfirm, 1);
        }
        else if (mode == ModeBattleHall)
        {
            PartyMenu_PrintShortMessage(app, TextChooseMonAndConfirm, 1);
        }
        else if (mode == ModeBattleCastle)
        {
            PartyMenu_PrintShortMessage(app, TextChooseMonAndConfirm, 1);
        }
        else if (mode == ModeSelectEgg)
        {
            PartyMenu_PrintShortMessage(app, TextSelectEgg, 1);
        }
        else if (mode != ModeGiveItemDone)
        {
            PartyMenu_PrintShortMessage(app, TextChooseAPokemon, 1);
        }
        else
        {
            SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 1);
        }
        PartyMenu_LoadContextMenuStrings(app);
        SetupMenuCursor(app);
        if (!ShouldShowSubscreen(app))
        {
            GXLayers.GXLayers_EngineBToggleLayers(1, 0);
        }
        GameSystem.SetVBlankCallback(PartyMenu_VBlankCallback, app);
        NetworkIcon.NetworkIcon_InitIfConnected();
        NARC.NARC_dtor(narc);
        return 1;
    }

    // OFFSET 0x0207E2A8
    public static int PartyMenu_Main(ApplicationManager appMan, ref int state)
    {
        PartyMenuApplication app = (PartyMenuApplication)ApplicationManager.ApplicationManager_Data(appMan)!;
        switch (state)
        {
            case StateStart:
                state = SelectStartState(app);
                break;
            case StateDefault:
                state = HandleDefaultInput(app);
                break;
            case StateHandlePartyMenuAction:
                if (HandleWindowInputEvent(app, ref state) == 1)
                {
                    state = StateDefault;
                }
                break;
            case StateWaitABPress:
                state = PartyMenu_WaitABPress(app);
                break;
            case StateUseItem:
                state = HandleUseItem(app);
                break;
            case StateExecCallback:
                state = app.Callback!(app);
                break;
            case StateWaitMoveListSelection:
                state = PartyMenu_MoveSelection_HandleInput(app);
                break;
            case StateUseSacredAsh:
                state = PartyMenuCB_HandleSacredAsh(app);
                break;
            case StateGiveItem:
                state = HandleGiveItem(app);
                break;
            case StateShowItemSwapConfirmation:
                state = PartyMenu_ShowItemSwapConfirmation(app);
                break;
            case StateHeldItemSwap:
                state = ProcessPokemonItemSwap(app);
                break;
            case StateConfirmItemUpdate:
                state = PartyMenu_ConfirmItemUpdate(app);
                break;
            case State12:
                state = ProcessMessageResult(app);
                break;
            case State13:
                state = HandleOverlayCompletion(app);
                break;
            case State14:
                state = UpdatePokemonFormWithItem(app);
                break;
            case StateHandleContextMenuInput:
                state = HandleContextMenuInput(app);
                break;
            case State16:
                state = ProcessItemApplication(app);
                break;
            case StatePrinting:
                state = PartyMenu_WaitForPrinter(app);
                break;
            case StatePrintingFormChange:
                state = PartyMenu_WaitForPrinterThenFormChange(app);
                break;
            case StateFormChange:
                state = PartyMenu_ChangeForm(app);
                break;
            case StateWaitABPress2:
                state = PartyMenu_WaitABPress2(app);
                break;
            case StateTeachMove:
                state = HandleTeachMove(app);
                break;
            case StateTeachMoveHandleInput:
                state = PartyMenuCB_TeachMove_HandleInput(app);
                break;
            case StateWaitABPressChooseMons:
                state = PartyMenu_WaitABPressChooseMons(app);
                break;
            case StateShowMessageThenNextState:
                state = WaitForPrinter(app);
                break;
            case StateWaitABPressBeforeFade:
                state = WaitABPressBeforeFadeOut(app);
                break;
            case StateDrawYesNoChoice:
                state = DrawYesNoChoice(app);
                break;
            case State27:
                state = HandleYesNoChoice(app);
                break;
            case StateSelectSwitchSlot:
            {
                int input = SelectSwitchSlot(app);
                if (input == InputConfirm)
                {
                    state = StateDoSwitch;
                }
                else if (input == InputCancel)
                {
                    state = StateDefault;
                }
                break;
            }
            case StateDoSwitch:
                if (PartyMenu_DoSwitch(app))
                {
                    state = StateDefault;
                }
                break;
            case StateHPTransferFieldMove:
                state = PartyMenu_UseHPTransferFieldMove(app);
                break;
            case StateWaitFormChange:
                if (PartyMenuFormChanges.PartyMenuFormChange_ChangeForm(app))
                {
                    PartyMenu_TeardownFormChangeAnim(app);
                    state = StateWaitABPressBeforeFade;
                }
                else
                {
                    state = StateWaitFormChange;
                }
                break;
            case StateFadeOut:
                HealthBar.App_StartScreenFade(true, HeapID);
                state = StateWaitFadeOut;
                break;
            case StateWaitFadeOut:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    app.PartyMenu!.SelectedMonSlot = app.CurrPartySlot;
                    return 1;
                }
                break;
        }
        PartyMenu_UpdateMemberIcons(app);
        PartyMenu_UpdateTouchButtonEffect(app);
        UpdateTouchButtonPress(app);
        SpriteSystem.SpriteSystem_DrawSprites(app.SpriteMan);
        return 0;
    }

    // OFFSET 0x0207E490
    private static int SelectStartState(PartyMenuApplication app)
    {
        if (!ScreenFadeControl.IsScreenFadeDone())
        {
            return StateStart;
        }
        PartyMenu partyMenu = app.PartyMenu!;
        int mode = partyMenu.Mode;
        if (mode == ModeUseItem || mode == ModeUseEvoItem)
        {
            if (CheckItemSacredAsh(partyMenu.UsedItemID))
            {
                app.StateAfterMessage = 0;
                return StateUseSacredAsh;
            }
            return StateUseItem;
        }
        if (mode == ModeTeachMove)
        {
            return StateTeachMove;
        }
        if (mode == ModeTeachMoveDone)
        {
            return PartyMenuCB_TeachMove_Exit(app);
        }
        if (mode == ModeLevelMoveDone)
        {
            return PartyMenuCB_LevelMove_Exit(app);
        }
        if (mode == ModeGiveMail || mode == ModeGiveMailDone)
        {
            return State14;
        }
        if (mode == ModeGiveItem)
        {
            return StateGiveItem;
        }
        if (mode == ModeGiveItemDone)
        {
            return State16;
        }
        return StateDefault;
    }

    // OFFSET 0x0207E518
    private static int HandleDefaultInput(PartyMenuApplication app)
    {
        int input = PartyMenu_HandleInput(app);
        PartyMenu partyMenu = app.PartyMenu!;
        if (input == 0)
        {
            int mode = partyMenu.Mode;
            if (mode == ModeSelectNoPrompt || mode == ModeFeedPoffin)
            {
                partyMenu.MenuSelectionResult = 0;
                return StateFadeOut;
            }
            if (mode == ModeMailbox)
            {
                PartyMenu_GiveMail(app);
                return StateShowMessageThenNextState;
            }
            if (mode == ModeNpcTrade)
            {
                return CheckForItemApplication(app);
            }
            return StateHandlePartyMenuAction;
        }
        if (input == 4)
        {
            return HandleGameWindowEvent(app);
        }
        if (input == 3)
        {
            partyMenu.MenuSelectionResult = 0;
            return StateFadeOut;
        }
        if (input == 2)
        {
            if (partyMenu.Mode != ModeBallSeal)
            {
                partyMenu.MenuSelectionResult = 1;
                return StateFadeOut;
            }
            SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 1);
            return PartyMenu_SetBallCapsule(app);
        }
        return StateDefault;
    }

    // OFFSET 0x0207E5B4
    private static int HandleUseItem(PartyMenuApplication app)
    {
        int input = HandleSpecialInput(app);
        if (input == 0 || input == 2)
        {
            SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 1);
            return ApplyItemEffectOnPokemon(app);
        }
        if (input == 3)
        {
            app.PartyMenu!.MenuSelectionResult = 0;
            return StateFadeOut;
        }
        return StateUseItem;
    }

    // OFFSET 0x0207E5F4
    private static int HandleGiveItem(PartyMenuApplication app)
    {
        int input = HandleSpecialInput(app);
        if (input == 0 || input == 2)
        {
            SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 1);
            return ProcessItemApplication(app);
        }
        if (input == 3)
        {
            app.PartyMenu!.MenuSelectionResult = 0;
            return StateFadeOut;
        }
        return StateGiveItem;
    }

    // OFFSET 0x0207E634
    private static int HandleContextMenuInput(PartyMenuApplication app)
    {
        uint selection = Menu.Menu_ProcessInput(app.ContextMenu!);
        if (selection == Menu.Cancel)
        {
            RenderWindow.Window_EraseMessageBox(app.Windows[WinMediumMessage], 1);
            RenderWindow.Window_EraseStandardFrame(app.Windows[WinGiveItemOrMail], 1);
            Bg.Window_ClearAndScheduleCopyToVRAM(app.Windows[WinGiveItemOrMail]);
            Menu.Menu_Free(app.ContextMenu!, out _);
            StringList.StringList_Free(app.ContextMenuChoices!);
            PartyMenu_PrintShortMessage(app, TextChooseAPokemon, 1);
            SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 0);
            return StateDefault;
        }
        if (selection == Menu.NoChoice)
        {
            return StateHandleContextMenuInput;
        }
        int nextState;
        PartyMenuContextAction(selection)(app, out nextState);
        return nextState;
    }

    // OFFSET 0x0207E6C0
    private static int WaitForPrinter(PartyMenuApplication app)
    {
        if (Text.Text_IsPrinterActive(app.TextPrinterID) == 0)
        {
            return app.StateAfterMessage;
        }
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x0207E6E4
    private static int WaitABPressBeforeFadeOut(PartyMenuApplication app)
    {
        if ((gSystem.PressedKeys & (KeyA | KeyB)) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            return StateFadeOut;
        }
        return StateWaitABPressBeforeFade;
    }

    // OFFSET 0x0207E708
    private static int DrawYesNoChoice(PartyMenuApplication app)
    {
        PartyMenu_DrawYesNoChoice(app);
        return State27;
    }

    // OFFSET 0x0207E714
    private static int HandleYesNoChoice(PartyMenuApplication app)
    {
        uint selection = Menu.Menu_ProcessInputAndHandleExit(app.ContextMenu!, HeapID);
        if (selection == 0)
        {
            return app.OnYes!(app);
        }
        if (selection == Menu.Cancel)
        {
            return app.OnNo!(app);
        }
        return State27;
    }

    // OFFSET 0x0207E750
    private static int HandleTeachMove(PartyMenuApplication app)
    {
        int input = HandleSpecialInput(app);
        if (input == 0 || input == 2)
        {
            SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 1);
            if (app.PartyMembers[app.CurrPartySlot].IsEgg != 1)
            {
                return PartyMenuCB_TeachMove(app);
            }
            PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
            app.PartyMenu!.MenuSelectionResult = 0;
            app.StateAfterMessage = StateWaitABPressBeforeFade;
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, TextNoEggsAllowed, app.TmpString!);
            return StateShowMessageThenNextState;
        }
        if (input == 3)
        {
            app.PartyMenu!.MenuSelectionResult = 0;
            return StateFadeOut;
        }
        return StateTeachMove;
    }

    // OFFSET 0x0207E7E0
    public static int PartyMenu_Exit(ApplicationManager appMan, ref int state)
    {
        PartyMenuApplication app = (PartyMenuApplication)ApplicationManager.ApplicationManager_Data(appMan)!;
        GameSystem.SetVBlankCallback(null, null);
        PartyMenu_CleanupSprites(app);
        PartyMenu_RemoveWindows(app);
        FreeBgs(app.BgConfig!);
        TouchPad.DisableTouchPad();
        VramTransfer.VramTransfer_Free();
        for (int i = 0; i < MaxPartySize; i++)
        {
            GameString.String_Free(app.PartyMembers[i].Name);
        }
        GameString.String_Free(app.TmpString);
        GameString.String_Free(app.TmpFormat);
        for (int i = 0; i < PartyMenuApplication.NumStrings; i++)
        {
            GameString.String_Free(app.MenuStrings[i]);
        }
        MessageLoader.MessageLoader_Free(app.MessageLoader);
        FontSpecialCharsContext.FontSpecialChars_Free(app.SpecialChars);
        StringTemplate.StringTemplate_Free(app.Template!);
        if (app.HeightWeight != null)
        {
            HeightWeight.HeightWeightData_FreeLoaded(app.HeightWeight);
        }
        ApplicationManager.ApplicationManager_FreeData(appMan);
        Heap.Heap_Destroy(HeapID);
        return 1;
    }

    // OFFSET 0x0207E898
    private static void PartyMenu_VBlankCallback(object? data)
    {
        PartyMenuApplication app = (PartyMenuApplication)data!;
        Bg.Bg_RunScheduledUpdates(app.BgConfig!);
        VramTransfer.VramTransfer_Process();
        SpriteSystem.SpriteSystem_TransferOam();
    }

    // OFFSET 0x0207E8C0
    private static void SetVRAMBanks()
    {
        GXLayers.GXLayers_SetBanks(RomStructs.Arm9Banks(VramBanksAddress));
    }

    // OFFSET 0x0207E8E0
    private static void InitBgMainLayer0(BgConfig bgConfig)
    {
        Bg.Bg_InitFromTemplate(bgConfig, 0, RomStructs.Arm9BgTemplate(BgTemplateLayer0), 0);
        Bg.Bg_ClearTilemap(bgConfig, 0);
    }

    // OFFSET 0x0207E918
    private static void InitBgs(BgConfig bgConfig)
    {
        Bg.SetAllGraphicsModes(RomStructs.Arm9GraphicsModes(GraphicsModesAddress));
        InitBgMainLayer0(bgConfig);
        Bg.Bg_InitFromTemplate(bgConfig, 1, RomStructs.Arm9BgTemplate(BgTemplateLayer1), 0);
        Bg.Bg_ClearTilemap(bgConfig, 1);
        Bg.Bg_InitFromTemplate(bgConfig, 2, RomStructs.Arm9BgTemplate(BgTemplateLayer2), 0);
        Bg.Bg_ClearTilemap(bgConfig, 2);
        Bg.Bg_InitFromTemplate(bgConfig, 3, RomStructs.Arm9BgTemplate(BgTemplateLayer3), 0);
        Bg.Bg_InitFromTemplate(bgConfig, 4, RomStructs.Arm9BgTemplate(BgTemplateLayer4), 0);
        Bg.Bg_ClearTilemap(bgConfig, 4);
        Bg.Bg_InitFromTemplate(bgConfig, 5, RomStructs.Arm9BgTemplate(BgTemplateLayer5), 0);
        Bg.Bg_ClearTilesRange(0, 0x20, 0, HeapID);
        Bg.Bg_ClearTilesRange(4, 0x20, 0, HeapID);
    }

    // OFFSET 0x0207EA24
    private static void FreeBgs(BgConfig bgConfig)
    {
        GXLayers.GXLayers_EngineAToggleLayers(0x1F, 0);
        GXLayers.GXLayers_EngineBToggleLayers(0x13, 0);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 5);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 4);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 3);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 2);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 1);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 0);
    }

    // OFFSET 0x0207EA74
    public static void PartyMenu_UpdateFormChangeGraphicsMode(PartyMenuApplication app, bool enable3D)
    {
        if (!enable3D)
        {
            Bg.Bg_ToggleLayer(0, 0);
            Bg.Bg_FreeTilemapBuffer(app.BgConfig!, 0);
            Video.SetGraphicsMode(1, 0, 1);
            app.FormChange3DPipeline = InitG3DPipeline(HeapID);
            return;
        }
        GXLayers.GXLayers_EngineAToggleLayers(1, 0);
        FreeG3DPipeline(app.FormChange3DPipeline!);
        Video.SetGraphicsMode(1, 0, 0);
        InitBgMainLayer0(app.BgConfig!);
        Bg.Bg_ClearTilesRange(0, 0x20, 0, HeapID);
    }

    // OFFSET 0x0207EAD4
    private static G3DPipelineBuffers InitG3DPipeline(int heapID)
    {
        return G3DPipeline.G3DPipeline_InitEx(heapID, 0, 2, 0, 1, G3DPipelineCallback);
    }

    // OFFSET 0x0207EAF4
    private static void G3DPipelineCallback()
    {
        Render3D.Display3DControl &= Disp3DShadingMask;
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DAntiAlias);
        Render3D.Display3DControl &= Disp3DAlphaTestMask;
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DAlphaBlend);
        Render3D.Display3DControl &= Disp3DEdgeMarkingMask;
        Render3D.SetFog(false, 0, 0, 0);
        Render3D.SetClearColor(0, 0, ClearDepthMax, ClearPolygonID, false);
        Render3D.SetViewport(0, 0, 255, 191);
    }

    // OFFSET 0x0207EB64
    private static void FreeG3DPipeline(G3DPipelineBuffers buffers)
    {
        G3DPipeline.G3DPipelineBuffers_Free(buffers);
    }

    // OFFSET 0x0207EB6C
    private static void LoadGraphics(PartyMenuApplication app, NARC narc)
    {
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0xF, app.BgConfig!, 3, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x11, app.BgConfig!, 3, 0, 0, 0, HeapID);
        byte[] raw = NARC.NARC_AllocAndReadWholeMember(narc, 0x10, HeapID);
        G2d.GetUnpackedPaletteData(raw, out PaletteFile? palette);
        Bg.Bg_LoadPalette(3, palette!.Colors, 0, (ushort)palette.ByteSize, 0);
        Array.Copy(palette.Colors, 0x30, app.Colors, 0, PartyMenuApplication.NumMenuColors);
        Font.Font_LoadScreenIndicatorsPalette(0, 0x1A0, HeapID);
        RenderWindow.LoadStandardWindowGraphics(app.BgConfig!, 0, 1, 0xE, 0, HeapID);
        RenderWindow.LoadMessageBoxGraphics(app.BgConfig!, 0, 0xA, 0xF, (byte)Options.Options_Frame(app.PartyMenu!.Options!), HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 3, app.BgConfig!, 4, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 4, 4, 0x20, 0x20, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0xC, app.BgConfig!, 5, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0xE, app.BgConfig!, 5, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 0xD, 4, 0, 0x20, HeapID);
        PartyMenu_LoadMemberPanelTilemaps(HeapID, app.LeadMemberPanel, app.BackMemberPanel, app.NoneMemberPanel);
        Bg.Bg_MaskPalette(0, 0);
        Bg.Bg_MaskPalette(4, 0);
    }

    // OFFSET 0x0207ECC0
    private static PartyMenuApplication NewPartyMenuApplication(ApplicationManager appMan)
    {
        PartyMenuApplication app = new();
        appMan.Data = app;
        app.PartyMenu = (PartyMenu)ApplicationManager.ApplicationManager_Args(appMan)!;
        app.BgConfig = Bg.BgConfig_New(HeapID);
        if (app.PartyMenu.Mode == ModeSelectConfirm && app.PartyMenu.BattleRegulation != null)
        {
            app.HeightWeight = HeightWeight.HeightWeightData_LoadNew(HeapID);
        }
        else
        {
            app.HeightWeight = null;
        }
        app.MessageLoader = MessageLoader.MessageLoader_Init(0, MessageArchive, PartyMenuTextBank, HeapID);
        app.SpecialChars = FontSpecialCharsContext.FontSpecialChars_Init(0xF, 0xE, 0, HeapID);
        app.Template = StringTemplate.StringTemplate_Default(HeapID);
        for (int i = 0; i < MaxPartySize; i++)
        {
            app.PartyMembers[i].Name = GameString.String_Init(0xB, HeapID);
        }
        app.TmpString = GameString.String_Init(0x100, HeapID);
        app.TmpFormat = GameString.String_Init(0x100, HeapID);
        for (int i = 0; i < PartyMenuApplication.NumStrings; i++)
        {
            app.MenuStrings[i] = GameString.String_Init(0x20, HeapID);
        }
        app.CurrPartySlot = app.PartyMenu.SelectedMonSlot;
        app.PrevPartySlot = app.PartyMenu.SelectedMonSlot;
        return app;
    }

    // OFFSET 0x0207EDC0
    private static void CheckCancellableMode(PartyMenuApplication app)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        if ((partyMenu.Mode & ModeHideCancelFlag) != 0)
        {
            app.HideCancel = true;
            partyMenu.Mode ^= ModeHideCancelFlag;
        }
        else if (partyMenu.Mode == ModeSelectEgg)
        {
            app.HideCancel = true;
        }
        else
        {
            app.HideCancel = false;
        }
    }

    // OFFSET 0x0207EE14
    private static void SetupRequestedMode(PartyMenuApplication app)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        byte buttons = 3;
        int mode = partyMenu.Mode;
        if (partyMenu.Type == TypeMultiBattle)
        {
            app.CursorPosTable = ReadCursorPositions(CursorPositionsMulti);
        }
        else if (mode == ModeSelectConfirm || mode == ModeBattleTower || mode == ModeBattleHall || mode == ModeBattleCastle)
        {
            app.CursorPosTable = ReadCursorPositions(CursorPositionsConfirm);
        }
        else if (mode == ModeSelectEgg)
        {
            app.CursorPosTable = ReadCursorPositions(CursorPositionsNoCancel);
        }
        else
        {
            app.CursorPosTable = ReadCursorPositions(CursorPositionsStandard);
        }
        mode = partyMenu.Mode;
        if (mode != ModeSelectConfirm && mode != ModeBattleTower && mode != ModeBattleCastle && mode != ModeBattleHall)
        {
            Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteConfirmButton]!, 0);
            Sprite.Sprite_SetAnim(app.Sprites[SpriteCancelButton]!, 0);
            SpriteSystem.Sprite_GetPositionXY(app.Sprites[SpriteCancelButton]!, out short x, out short y);
            SpriteSystem.Sprite_SetPositionXY(app.Sprites[SpriteCancelButton]!, x, (short)(y - 8));
            buttons ^= 1;
        }
        mode = partyMenu.Mode;
        if (mode == ModeUnk04 || mode == ModeSelectEgg)
        {
            Sprite.Sprite_SetDrawFlag(app.Sprites[SpriteCancelButton]!, 0);
            buttons ^= 2;
        }
        PartyMenu_PrintButtonText(app, buttons);
    }

    // OFFSET 0x0207EF04
    public static byte PartyMenu_IsMemberPresent(PartyMenuApplication app, int slot)
    {
        return app.PartyMembers[slot].IsPresent;
    }

    // OFFSET 0x0207EF14
    private static bool PartyMenu_LoadMember(PartyMenuApplication app, int slot)
    {
        PartyMenuMember member = app.PartyMembers[slot];
        member.IsPresent = 0;
        if (Party.Party_GetCurrentCount(app.PartyMenu!.Party!) <= slot)
        {
            return false;
        }
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu.Party!, slot);
        ushort species = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
        if (species == 0)
        {
            return false;
        }
        PartyMenu_SetMemberName(app, mon, slot);
        member.Species = species;
        member.CurHP = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null);
        member.MaxHP = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMaxHP, null);
        member.Level = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataLevel, null);
        member.HeldItem = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataHeldItem, null);
        member.BallSeal = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataBallCapsuleID, null);
        member.IsEgg = (byte)Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null);
        member.Form = (byte)Pokemon.Pokemon_GetValue(mon, MonDataForm, null);
        member.HideGenderMarker = Pokemon.Pokemon_GetValue(mon, MonDataNidoranHasNickname, null) != 1;
        member.Gender = (byte)(Pokemon.Pokemon_GetGender(mon) & 3);
        member.IsPresent = 1;
        member.StatusIcon = (ushort)((byte)Pokemon.Pokemon_GetSummaryStatus(mon) & 0xFFF);
        CheckContestEligibility(app, mon, slot);
        return true;
    }

    // OFFSET 0x0207F094
    private static void CheckContestEligibility(PartyMenuApplication app, Pokemon mon, int slot)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        if (partyMenu.Mode != ModeContest)
        {
            return;
        }
        PartyMenuMember member = app.PartyMembers[slot];
        if (member.IsEgg == 1 || member.CurHP == 0)
        {
            member.IsContestEligible = false;
            return;
        }
        uint ribbons = CountEarnedRibbonsForContestType(mon, partyMenu.ContestType);
        int moveCount = 0;
        while (moveCount < 4 && Pokemon.Pokemon_GetValue(mon, MonDataMove1 + moveCount, null) != 0)
        {
            moveCount++;
        }
        member.IsContestEligible = partyMenu.ContestRank <= ribbons && moveCount >= 2;
    }

    // OFFSET 0x0207F134
    private static uint CountEarnedRibbonsForContestType(Pokemon mon, int contestType)
    {
        if (contestType > 4)
        {
            return 0;
        }
        int first = 0x7B + contestType * 4;
        uint count = 0;
        for (int i = 0; i < 4; i++)
        {
            count += Pokemon.Pokemon_GetValue(mon, first + i, null);
        }
        return count;
    }

    // OFFSET 0x0207F248
    public static byte[] PartyMenu_GetHealthbarTilemap(PartyMenuApplication app, out int offset)
    {
        offset = 0x6C;
        return app.BackMemberPanel;
    }

    // OFFSET 0x0207F250
    private static void DrawMemberPanel(PartyMenuApplication app, int slot, byte x, byte y, bool hasHealthbar)
    {
        byte[] panel;
        if (slot == 0 || (app.PartyMenu!.Type != 0 && slot == 1))
        {
            panel = app.LeadMemberPanel;
        }
        else
        {
            panel = app.BackMemberPanel;
        }
        app.PartyMembers[slot].PanelXPos = (sbyte)x;
        app.PartyMembers[slot].PanelYPos = (sbyte)y;
        Bg.Bg_CopyToTilemapRect(app.BgConfig!, 2, x, y, 0x10, 6, panel, 0, 0, 0x10, 6);
        if (!hasHealthbar)
        {
            Bg.Bg_FillTilemapRect(app.BgConfig!, 2, 0x17, x + 6, (byte)(y + 3), 9, 1, 0x10);
        }
        Bg.Bg_ChangeTilemapRectPalette(app.BgConfig!, 2, x, y, 0x10, 6, (byte)(slot + 3));
        PartyMenu_UpdateSlotPalette(app, slot);
    }
}
