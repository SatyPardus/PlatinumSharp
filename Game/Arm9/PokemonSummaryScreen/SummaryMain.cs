using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public static partial class PokemonSummaryScreenApp
{
    private const uint HeapSize = 0x40000;
    private const int SummaryArchive = 0x27;
    private const int PlPokeDataArchive = 0x93;
    private const int MessageArchive = 0x1A;
    private const int SummaryTextBank = 0x1C7;
    private const int RibbonTextBank = 0x217;
    private const int MoveNamesBank = 0x287;
    private const uint AllLayersMask = 0xFFFFE0FF;
    private const uint WindowsMask = 0xFFFF1FFF;
    private const uint VramBanksAddress = 0x020F40E4;
    private const uint GraphicsModesAddress = 0x020F4048;
    private const uint BgTemplateLayer1 = 0x020F4074;
    private const uint BgTemplateLayer2 = 0x020F4090;
    private const uint BgTemplateLayer3 = 0x020F40AC;
    private const uint BgTemplateLayer4 = 0x020F40C8;
    private const uint BgTemplateLayer5 = 0x020F4058;
    private const int KeyA = 0x1;
    private const int KeyB = 0x2;
    private const int KeyRight = 0x10;
    private const int KeyLeft = 0x20;
    private const int KeyUp = 0x40;
    private const int KeyDown = 0x80;
    private const int SeqSelect = 0x5DC;
    private const int SeqConfirm = 0x5DD;
    private const int SeqMoveInfo = 0x69C;
    private const int SpeciesChatot = 0x1B9;
    private const int MaxLevel = 100;
    private const int NumRibbons = 0x50;
    private const int FlavorNone = 5;
    private const int NumFlavors = 5;

    public const int ModeNormal = 0;
    public const int ModeLock = 1;
    public const int ModeSelectMove = 2;
    public const int ModePoffin = 3;
    public const int ModeCondition = 4;

    public const int DataTypeMon = 0;
    public const int DataTypeBoxMon = 2;

    public const int PageInfo = 0;
    public const int PageMemo = 1;
    public const int PageSkills = 2;
    public const int PageBattleMoves = 3;
    public const int PageCondition = 4;
    public const int PageContestMoves = 5;
    public const int PageRibbons = 6;
    public const int PageExit = 7;
    public const int PageMax = 8;

    public const int ReturnSelect = 0;
    public const int ReturnCancel = 1;

    private const int StateTransitionIn = 0;
    private const int StateWaitTransition = 1;
    private const int StateInput = 2;
    private const int StateSetupBattleMoveInfo = 3;
    private const int StateHideBattleMoveInfo = 4;
    private const int StateSetupContestMoveInfo = 5;
    private const int StateHideContestMoveInfo = 6;
    private const int StateMoveDetails = 7;
    private const int StateMoveSwap = 8;
    private const int StateSelectMove = 9;
    private const int StateHMMessage = 10;
    private const int StateSetupRibbonInfo = 11;
    private const int StateHideRibbonInfo = 12;
    private const int StateRibbonSelect = 13;
    private const int StateSubscreen = 14;
    private const int StatePoffinFeed = 15;
    private const int StateContestStatMessages = 16;
    private const int StatePoffinMessage = 17;
    private const int StateTransitionOut = 18;
    private const int StateExit = 19;

    private const int MonDataSpecies = 5;
    private const int MonDataHeldItem = 6;
    private const int MonDataOTID = 7;
    private const int MonDataExp = 8;
    private const int MonDataAbility = 0xA;
    private const int MonDataMarkings = 0xB;
    private const int MonDataCool = 0x13;
    private const int MonDataBeauty = 0x14;
    private const int MonDataCute = 0x15;
    private const int MonDataSmart = 0x16;
    private const int MonDataTough = 0x17;
    private const int MonDataSheen = 0x18;
    private const int MonDataMove1 = 0x36;
    private const int MonDataMove1PP = 0x3A;
    private const int MonDataMove1PPUps = 0x3E;
    private const int MonDataIsEgg = 0x4C;
    private const int MonDataForm = 0x70;
    private const int MonDataPokeball = 0x9B;
    private const int MonDataOTGender = 0x9D;
    private const int MonDataLevel = 0xA1;
    private const int MonDataCurrentHP = 0xA3;
    private const int MonDataMaxHP = 0xA4;
    private const int MonDataAttack = 0xA5;
    private const int MonDataDefense = 0xA6;
    private const int MonDataSpeed = 0xA7;
    private const int MonDataSpAttack = 0xA8;
    private const int MonDataSpDefense = 0xA9;
    private const int MonDataHasNickname = 0xB0;
    private const int MonDataType1 = 0xB1;
    private const int MonDataType2 = 0xB2;

    private const int SpriteMoveSelector2 = 10;
    private const int SpriteMoveCategory = 18;

    // OFFSET 0x0208C330
    public static int PokemonSummaryScreen_Init(ApplicationManager appMan, ref int state)
    {
        GameSystem.SetVBlankCallback(null, null);
        GameSystem.DisableHBlank();
        GXLayers.GXLayers_DisableEngineALayers();
        GXLayers.GXLayers_DisableEngineBLayers();
        Video.SetDisplayControl(0, Video.GetDisplayControl(0) & AllLayersMask);
        Video.SetDisplayControl(1, Video.GetDisplayControl(1) & AllLayersMask);
        Video.SetDisplayControl(0, Video.GetDisplayControl(0) & WindowsMask);
        Video.SetDisplayControl(1, Video.GetDisplayControl(1) & WindowsMask);
        Video.SetBlendControl(0, 0);
        Video.SetBlendControl(1, 0);
        GameSystem.SetAutorepeat(4, 8);
        Heap.Heap_Create(3, HeapIDSummary, HeapSize);
        NARC narc = NARC.NARC_ctor(SummaryArchive, HeapIDSummary);
        ApplicationManager.ApplicationManager_NewData(appMan, PokemonSummaryScreen.Size, HeapIDSummary);
        PokemonSummaryScreen summaryScreen = new();
        appMan.Data = summaryScreen;
        summaryScreen.Data = (PokemonSummary)ApplicationManager.ApplicationManager_Args(appMan)!;
        summaryScreen.BgConfig = Bg.BgConfig_New(HeapIDSummary);
        summaryScreen.MonSprite.MonAnimMan = PokemonAnimManager.PokemonAnimManager_New(HeapIDSummary, 1, 1);
        summaryScreen.NarcPlPokeData = NARC.NARC_ctor(PlPokeDataArchive, HeapIDSummary);
        Font.Font_UseImmediateGlyphAccess(0, HeapIDSummary);
        TouchPad.EnableTouchPad();
        TouchPad.InitializeTouchPad(4);
        TryHideContestPages(summaryScreen);
        SetVRAMBanks();
        SetupBgs(summaryScreen.BgConfig);
        LoadBgGraphics(summaryScreen, narc);
        SetAlphaBlending();
        PokemonSummaryScreen_Setup3DGfx();
        InitializeStringsAndCopyOTName(summaryScreen);
        SetMonData(summaryScreen);
        PokemonSummaryScreen_InitSpriteResources(summaryScreen);
        PokemonSummaryScreen_SetDefaultSpriteStates(summaryScreen);
        PokemonSummaryScreen_LoadMonSprite(summaryScreen);
        PokemonSummaryScreen_SetCaughtBallGfx(summaryScreen);
        PokemonSummaryScreen_UpdateStatusIcon(summaryScreen);
        PokemonSummaryScreen_DrawStaticWindows(summaryScreen);
        SetupInitialPageGfx(summaryScreen);
        PokemonSummaryScreen_SetSubscreenType(summaryScreen);
        PokemonSummaryScreen_SetupCamera(summaryScreen);
        GameSystem.SetVBlankCallback(PokemonSummaryScreen_VBlankCallback, summaryScreen);
        GXLayers.GXLayers_TurnBothDispOn();
        Sound.Sound_SetSceneAndPlayBGM(0x3D, 0, 0);
        NetworkIcon.NetworkIcon_InitIfConnected();
        NARC.NARC_dtor(narc);
        return 1;
    }

    // OFFSET 0x0208C488
    public static int PokemonSummaryScreen_Main(ApplicationManager appMan, ref int state)
    {
        PokemonSummaryScreen summaryScreen = (PokemonSummaryScreen)ApplicationManager.ApplicationManager_Data(appMan)!;
        switch (state)
        {
            case StateTransitionIn:
                HealthBar.App_StartScreenFade(false, HeapIDSummary);
                state = StateWaitTransition;
                break;
            case StateWaitTransition:
                state = WaitSummaryScreenTransition(summaryScreen);
                break;
            case StateInput:
                state = HandleInput_Main(summaryScreen);
                break;
            case StateSetupBattleMoveInfo:
                state = WaitSetupBattleMoveInfo(summaryScreen);
                break;
            case StateHideBattleMoveInfo:
                state = WaitHideBattleMoveInfo(summaryScreen);
                break;
            case StateSetupContestMoveInfo:
                state = WaitSetupContestMoveInfo(summaryScreen);
                break;
            case StateHideContestMoveInfo:
                state = WaitHideContestMoveInfo(summaryScreen);
                break;
            case StateMoveDetails:
                state = HandleInput_MoveDetails(summaryScreen);
                break;
            case StateMoveSwap:
                state = HandleInput_MoveSwap(summaryScreen);
                break;
            case StateSelectMove:
                state = HandleInput_SelectMove(summaryScreen);
                break;
            case StateHMMessage:
                state = WaitForHMMsgInput(summaryScreen);
                break;
            case StateSetupRibbonInfo:
                state = WaitSetupRibbonInfo(summaryScreen);
                break;
            case StateHideRibbonInfo:
                state = WaitHideRibbonInfo(summaryScreen);
                break;
            case StateRibbonSelect:
                state = HandleInput_RibbonSelect(summaryScreen);
                break;
            case StateSubscreen:
                state = HandleInput_Subscreen(summaryScreen);
                break;
            case StatePoffinFeed:
                state = SetupPoffinFeedConditionPage(summaryScreen);
                break;
            case StateContestStatMessages:
                state = PrintContestStatChangeMsgs(summaryScreen);
                break;
            case StatePoffinMessage:
                state = WaitForPoffinFeedMsgInput(summaryScreen);
                break;
            case StateTransitionOut:
                state = SummaryScreenTransitionOut(summaryScreen);
                break;
            case StateExit:
                if (ScreenTransitionIsDone(summaryScreen) == 1)
                {
                    return 1;
                }
                break;
        }
        PokemonSummaryScreen_UpdateArrowAndTapAnims(summaryScreen);
        PokemonSummaryScreen_DrawSheenSprites(summaryScreen);
        PokemonSummaryScreen_UpdateRibbonFlashAnim(summaryScreen);
        SpriteSystem.SpriteSystem_DrawSprites(summaryScreen.SpriteMan);
        PokemonSummaryScreen_Update3DGfx(summaryScreen);
        return 0;
    }

    // OFFSET 0x0208C5A0
    public static int PokemonSummaryScreen_Exit(ApplicationManager appMan, ref int state)
    {
        PokemonSummaryScreen summaryScreen = (PokemonSummaryScreen)ApplicationManager.ApplicationManager_Data(appMan)!;
        GameSystem.SetVBlankCallback(null, null);
        PokemonSummaryScreen_FreeCameraAndMonSprite(summaryScreen);
        PokemonSummaryScreen_FreeSpriteResources(summaryScreen);
        PokemonSummaryScreen_RemoveWindows(summaryScreen);
        TeardownBgs(summaryScreen.BgConfig!);
        TouchPad.DisableTouchPad();
        VramTransfer.VramTransfer_Free();
        FreeStrings(summaryScreen);
        NARC.NARC_dtor(summaryScreen.NarcPlPokeData!);
        Font.Font_UseLazyGlyphAccess(0);
        Video.SetBlendControl(0, 0);
        ApplicationManager.ApplicationManager_FreeData(appMan);
        Heap.Heap_Destroy(HeapIDSummary);
        return 1;
    }

    // OFFSET 0x0208C604
    private static void PokemonSummaryScreen_VBlankCallback(object? data)
    {
        PokemonSummaryScreen summaryScreen = (PokemonSummaryScreen)data!;
        Bg.Bg_RunScheduledUpdates(summaryScreen.BgConfig!);
        PokemonSpriteManager.PokemonSpriteManager_UpdateCharAndPltt(summaryScreen.MonSprite.SpriteManager!);
        VramTransfer.VramTransfer_Process();
        SpriteSystem.SpriteSystem_TransferOam();
    }

    // OFFSET 0x0208C638
    private static void SetVRAMBanks()
    {
        GXLayers.GXLayers_SetBanks(RomStructs.Arm9Banks(VramBanksAddress));
    }

    // OFFSET 0x0208C658
    private static void SetupBgs(BgConfig bgConfig)
    {
        Bg.SetAllGraphicsModes(RomStructs.Arm9GraphicsModes(GraphicsModesAddress));
        Bg.Bg_InitFromTemplate(bgConfig, 1, RomStructs.Arm9BgTemplate(BgTemplateLayer1), 0);
        Bg.Bg_ClearTilemap(bgConfig, 1);
        Bg.Bg_InitFromTemplate(bgConfig, 2, RomStructs.Arm9BgTemplate(BgTemplateLayer2), 0);
        Bg.Bg_ClearTilemap(bgConfig, 2);
        Bg.Bg_ScheduleScroll(bgConfig, 2, 0, 0x88);
        Bg.Bg_InitFromTemplate(bgConfig, 3, RomStructs.Arm9BgTemplate(BgTemplateLayer3), 0);
        Bg.Bg_InitFromTemplate(bgConfig, 4, RomStructs.Arm9BgTemplate(BgTemplateLayer4), 0);
        Bg.Bg_ClearTilemap(bgConfig, 4);
        Bg.Bg_InitFromTemplate(bgConfig, 5, RomStructs.Arm9BgTemplate(BgTemplateLayer5), 0);
        Bg.Bg_ClearTilesRange(1, 0x20, 0, HeapIDSummary);
        Bg.Bg_ClearTilesRange(4, 0x20, 0, HeapIDSummary);
    }

    // OFFSET 0x0208C76C
    private static void TeardownBgs(BgConfig bgConfig)
    {
        GXLayers.GXLayers_DisableEngineALayers();
        GXLayers.GXLayers_DisableEngineBLayers();
        Bg.Bg_FreeTilemapBuffer(bgConfig, 5);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 4);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 3);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 2);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 1);
    }

    // OFFSET 0x0208C7AC
    private static void LoadBgGraphics(PokemonSummaryScreen summaryScreen, NARC narc)
    {
        BgConfig bgConfig = summaryScreen.BgConfig!;
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0, bgConfig, 3, 0, 0, 0, HeapIDSummary);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 3, bgConfig, 3, 0, 0, 0, HeapIDSummary);
        Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 1, 0, 0, 0, HeapIDSummary);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0xB, bgConfig, 2, 0, 0, 0, HeapIDSummary);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 2, bgConfig, 4, 0, 0, 0, HeapIDSummary);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x10, bgConfig, 5, 0, 0, 0, HeapIDSummary);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0xF, bgConfig, 5, 0, 0, 0, HeapIDSummary);
        Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 0xE, 4, 0, 0, HeapIDSummary);
    }

    // OFFSET 0x0208C86C
    private static void SetAlphaBlending()
    {
        Video.SetBlendAlpha(0, 1, 0x1E, 0x17, 8);
    }

    // OFFSET 0x0208C884
    private static void InitializeStringsAndCopyOTName(PokemonSummaryScreen summaryScreen)
    {
        summaryScreen.MsgLoader = MessageLoader.MessageLoader_Init(0, MessageArchive, SummaryTextBank, HeapIDSummary);
        summaryScreen.RibbonLoader = MessageLoader.MessageLoader_Init(1, MessageArchive, RibbonTextBank, HeapIDSummary);
        summaryScreen.SpecialChars = FontSpecialCharsContext.FontSpecialChars_Init(1, 2, 0, HeapIDSummary);
        summaryScreen.StrFormatter = StringTemplate.StringTemplate_Default(HeapIDSummary);
        summaryScreen.MonData.SpeciesName = GameString.String_Init(0xC, HeapIDSummary);
        summaryScreen.MonData.Nickname = GameString.String_Init(0xC, HeapIDSummary);
        summaryScreen.MonData.OTName = GameString.String_Init(8, HeapIDSummary);
        summaryScreen.String = GameString.String_Init(0x80, HeapIDSummary);
        summaryScreen.MoveNameLoader = MessageLoader.MessageLoader_Init(0, MessageArchive, MoveNamesBank, HeapIDSummary);
        summaryScreen.PlayerName = GameString.String_Init(8, HeapIDSummary);
        if (summaryScreen.Data!.OTName != null)
        {
            GameString.String_CopyChars(summaryScreen.PlayerName, summaryScreen.Data.OTName);
        }
    }

    // OFFSET 0x0208C950
    private static void FreeStrings(PokemonSummaryScreen summaryScreen)
    {
        MessageLoader.MessageLoader_Free(summaryScreen.MoveNameLoader);
        MessageLoader.MessageLoader_Free(summaryScreen.RibbonLoader);
        MessageLoader.MessageLoader_Free(summaryScreen.MsgLoader);
        FontSpecialCharsContext.FontSpecialChars_Free(summaryScreen.SpecialChars);
        StringTemplate.StringTemplate_Free(summaryScreen.StrFormatter!);
        GameString.String_Free(summaryScreen.MonData.SpeciesName);
        GameString.String_Free(summaryScreen.MonData.Nickname);
        GameString.String_Free(summaryScreen.MonData.OTName);
        GameString.String_Free(summaryScreen.String);
        GameString.String_Free(summaryScreen.PlayerName);
    }

    // OFFSET 0x0208C9C8
    private static int WaitSummaryScreenTransition(PokemonSummaryScreen summaryScreen)
    {
        if (!ScreenFadeControl.IsScreenFadeDone())
        {
            return StateWaitTransition;
        }
        PokemonSummaryScreen_LoadMonAnimation(summaryScreen);
        PlayMonCry(summaryScreen);
        switch (summaryScreen.Data!.Mode)
        {
            case ModeSelectMove:
                return StateSelectMove;
            case ModeCondition:
                return StatePoffinFeed;
        }
        return StateInput;
    }

    private static int ExitWithCancel(PokemonSummaryScreen summaryScreen)
    {
        summaryScreen.Data!.ReturnMode = ReturnCancel;
        return StateTransitionOut;
    }

    // OFFSET 0x0208CA00
    private static int HandleInput_Main(PokemonSummaryScreen summaryScreen)
    {
        if (summaryScreen.SubscreenExit == 1)
        {
            return ExitWithCancel(summaryScreen);
        }
        int repeatKeys = gSystem.PressedKeysRepeatable;
        if ((repeatKeys & KeyLeft) != 0)
        {
            ChangePage(summaryScreen, -1);
            return StateInput;
        }
        if ((repeatKeys & KeyRight) != 0)
        {
            ChangePage(summaryScreen, 1);
            return StateInput;
        }
        if ((repeatKeys & KeyUp) != 0)
        {
            ChangeSummaryMon(summaryScreen, -1);
            return StateInput;
        }
        if ((repeatKeys & KeyDown) != 0)
        {
            ChangeSummaryMon(summaryScreen, 1);
            return StateInput;
        }
        int keys = gSystem.PressedKeys;
        if ((keys & KeyB) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            return ExitWithCancel(summaryScreen);
        }
        if ((keys & KeyA) != 0)
        {
            if (summaryScreen.Data!.Mode == ModePoffin && summaryScreen.Page == PageCondition)
            {
                SoundPlayback.Sound_PlayEffect(SeqConfirm);
                return TryFeedPoffin(summaryScreen);
            }
            switch (summaryScreen.Page)
            {
                case PageBattleMoves:
                    SoundPlayback.Sound_PlayEffect(SeqMoveInfo);
                    summaryScreen.PageState = 0;
                    return StateSetupBattleMoveInfo;
                case PageContestMoves:
                    SoundPlayback.Sound_PlayEffect(SeqMoveInfo);
                    summaryScreen.PageState = 0;
                    return StateSetupContestMoveInfo;
                case PageRibbons:
                    if (summaryScreen.RibbonMax != 0)
                    {
                        SoundPlayback.Sound_PlayEffect(SeqConfirm);
                        summaryScreen.PageState = 0;
                        return StateSetupRibbonInfo;
                    }
                    break;
                case PageExit:
                    SoundPlayback.Sound_PlayEffect(SeqConfirm);
                    return ExitWithCancel(summaryScreen);
            }
        }
        if (CheckSubscreenPressAndSetButton(summaryScreen) == 1)
        {
            summaryScreen.PageState = 2;
            return StateSubscreen;
        }
        return StateInput;
    }

    // OFFSET 0x0208CB38
    private static int WaitSetupBattleMoveInfo(PokemonSummaryScreen summaryScreen)
    {
        return SetupBattleMoveInfo(summaryScreen) == 1 ? StateMoveDetails : StateSetupBattleMoveInfo;
    }

    // OFFSET 0x0208CB4C
    private static int WaitHideBattleMoveInfo(PokemonSummaryScreen summaryScreen)
    {
        return HideBattleMoveInfo(summaryScreen) == 1 ? StateInput : StateHideBattleMoveInfo;
    }

    // OFFSET 0x0208CB60
    private static int WaitSetupContestMoveInfo(PokemonSummaryScreen summaryScreen)
    {
        return SetupContestMoveInfo(summaryScreen) == 1 ? StateMoveDetails : StateSetupContestMoveInfo;
    }

    // OFFSET 0x0208CB74
    private static int WaitHideContestMoveInfo(PokemonSummaryScreen summaryScreen)
    {
        return HideContestMoveInfo(summaryScreen) == 1 ? StateInput : StateHideContestMoveInfo;
    }

    private static bool HandleMoveCursorInput(PokemonSummaryScreen summaryScreen, int keys)
    {
        if ((keys & KeyUp) != 0)
        {
            if (TryChangeSelectedMove(summaryScreen, -1) == 1)
            {
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                UpdateMoveAttributes(summaryScreen);
            }
            return true;
        }
        if ((keys & KeyDown) != 0)
        {
            if (TryChangeSelectedMove(summaryScreen, 1) == 1)
            {
                SoundPlayback.Sound_PlayEffect(SeqSelect);
                UpdateMoveAttributes(summaryScreen);
            }
            return true;
        }
        return false;
    }

    private static int CloseMoveDetails(PokemonSummaryScreen summaryScreen)
    {
        SoundPlayback.Sound_PlayEffect(SeqMoveInfo);
        summaryScreen.PageState = 0;
        return summaryScreen.Page == PageBattleMoves ? StateHideBattleMoveInfo : StateHideContestMoveInfo;
    }

    // OFFSET 0x0208CB88
    private static int HandleInput_MoveDetails(PokemonSummaryScreen summaryScreen)
    {
        int keys = gSystem.PressedKeys;
        if (HandleMoveCursorInput(summaryScreen, keys))
        {
            return StateMoveDetails;
        }
        if ((keys & KeyA) != 0)
        {
            if (summaryScreen.Cursor == 4)
            {
                return CloseMoveDetails(summaryScreen);
            }
            if (summaryScreen.Data!.Mode != ModeLock)
            {
                SoundPlayback.Sound_PlayEffect(SeqConfirm);
                PokemonSummaryScreen_SetMoveSelector2Pos(summaryScreen);
                summaryScreen.CursorTmp = summaryScreen.Cursor;
                return StateMoveSwap;
            }
        }
        if ((keys & KeyB) != 0)
        {
            return CloseMoveDetails(summaryScreen);
        }
        return StateMoveDetails;
    }

    // OFFSET 0x0208CC6C
    private static int HandleInput_MoveSwap(PokemonSummaryScreen summaryScreen)
    {
        int keys = gSystem.PressedKeys;
        if (HandleMoveCursorInput(summaryScreen, keys))
        {
            return StateMoveSwap;
        }
        if ((keys & KeyA) != 0)
        {
            Sprite.Sprite_SetDrawFlag(summaryScreen.Sprites[SpriteMoveSelector2]!, 0);
            if (summaryScreen.Cursor != 4 && summaryScreen.Cursor != summaryScreen.CursorTmp)
            {
                SoundPlayback.Sound_PlayEffect(SeqConfirm);
                SwapSelectedMoves(summaryScreen);
                PokemonSummaryScreen_SwapMoveTypeIcons(summaryScreen, summaryScreen.Cursor, summaryScreen.CursorTmp);
                PokemonSummaryScreen_SwapMoveNameAndPP(summaryScreen);
                UpdateMoveAttributes(summaryScreen);
            }
            else
            {
                SoundPlayback.Sound_PlayEffect(SeqConfirm);
            }
            return StateMoveDetails;
        }
        if ((keys & KeyB) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            Sprite.Sprite_SetDrawFlag(summaryScreen.Sprites[SpriteMoveSelector2]!, 0);
            return StateMoveDetails;
        }
        return StateMoveSwap;
    }

    // OFFSET 0x0208CD44
    private static int HandleInput_SelectMove(PokemonSummaryScreen summaryScreen)
    {
        int keys = gSystem.PressedKeys;
        if ((keys & KeyLeft) != 0)
        {
            ChangePage(summaryScreen, -1);
            return StateSelectMove;
        }
        if ((keys & KeyRight) != 0)
        {
            ChangePage(summaryScreen, 1);
            return StateSelectMove;
        }
        if (HandleMoveCursorInput(summaryScreen, keys))
        {
            return StateSelectMove;
        }
        PokemonSummary data = summaryScreen.Data!;
        if ((keys & KeyA) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            if (summaryScreen.Cursor != 4 && Item.Item_IsHMMove(summaryScreen.MonData.Moves[summaryScreen.Cursor]) == 1 && data.Move != 0)
            {
                SpriteSystem.Sprite_SetDrawFlag2(summaryScreen.Sprites[SpriteMoveCategory]!, 0);
                DrawEmptyHearts(summaryScreen);
                PokemonSummaryScreen_PrintHMMovesCantBeForgotten(summaryScreen);
                return StateHMMessage;
            }
            data.SelectedMoveSlot = summaryScreen.Cursor;
            data.ReturnMode = ReturnSelect;
            return StateTransitionOut;
        }
        if ((keys & KeyB) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            data.SelectedMoveSlot = 4;
            data.ReturnMode = ReturnCancel;
            return StateTransitionOut;
        }
        return StateSelectMove;
    }

    // OFFSET 0x0208CE54
    private static int WaitForHMMsgInput(PokemonSummaryScreen summaryScreen)
    {
        if ((gSystem.PressedKeys & (KeyA | KeyB)) != 0)
        {
            UpdateMoveAttributes(summaryScreen);
            return StateSelectMove;
        }
        return StateHMMessage;
    }

    // OFFSET 0x0208CE70
    private static int WaitSetupRibbonInfo(PokemonSummaryScreen summaryScreen)
    {
        return SetupRibbonInfo(summaryScreen) == 1 ? StateRibbonSelect : StateSetupRibbonInfo;
    }

    // OFFSET 0x0208CE84
    private static int WaitHideRibbonInfo(PokemonSummaryScreen summaryScreen)
    {
        return HideRibbonInfo(summaryScreen) == 1 ? StateInput : StateHideRibbonInfo;
    }

    // OFFSET 0x0208CE98
    private static int HandleInput_RibbonSelect(PokemonSummaryScreen summaryScreen)
    {
        int repeatKeys = gSystem.PressedKeysRepeatable;
        if ((repeatKeys & KeyLeft) != 0)
        {
            ChangeSelectedRibbon(summaryScreen, -1);
            return StateRibbonSelect;
        }
        if ((repeatKeys & KeyRight) != 0)
        {
            ChangeSelectedRibbon(summaryScreen, 1);
            return StateRibbonSelect;
        }
        if ((repeatKeys & KeyUp) != 0)
        {
            ChangeSelectedRibbon(summaryScreen, -4);
            return StateRibbonSelect;
        }
        if ((repeatKeys & KeyDown) != 0)
        {
            ChangeSelectedRibbon(summaryScreen, 4);
            return StateRibbonSelect;
        }
        if ((gSystem.PressedKeys & (KeyA | KeyB)) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            summaryScreen.PageState = 0;
            return StateHideRibbonInfo;
        }
        return StateRibbonSelect;
    }

    // OFFSET 0x0208CF0C
    private static int HandleInput_Subscreen(PokemonSummaryScreen summaryScreen)
    {
        if (PokemonSummaryScreen_DrawSubscreenButtonAnim(summaryScreen) == 1)
        {
            return summaryScreen.PageState;
        }
        if (summaryScreen.ButtonState == 1)
        {
            int page = PokemonSummaryScreen_GetSubscreenButtonPage(summaryScreen, summaryScreen.ButtonCurrent);
            if (page == PageExit)
            {
                summaryScreen.SubscreenExit = 1;
            }
            if (summaryScreen.MonData.IsEgg == 1)
            {
                if (page == PageMemo || page == PageExit)
                {
                    SetupPageFromSubscreenButton(summaryScreen, (byte)page);
                }
            }
            else
            {
                SetupPageFromSubscreenButton(summaryScreen, (byte)page);
            }
        }
        return StateSubscreen;
    }

    private static bool UpdateConditionStat(ref byte stat, Pokemon mon, int param)
    {
        byte previous = stat;
        stat = (byte)Pokemon.Pokemon_GetValue(mon, param, null);
        return previous != stat;
    }

    // OFFSET 0x0208CF78
    private static int SetupPoffinFeedConditionPage(PokemonSummaryScreen summaryScreen)
    {
        if ((gSystem.PressedKeys & (KeyA | KeyB)) == 0)
        {
            return StatePoffinFeed;
        }
        PokemonSummary data = summaryScreen.Data!;
        Pokemon mon;
        if (data.DataType == DataTypeBoxMon)
        {
            BoxPokemon boxMon = (BoxPokemon)PokemonSummaryScreen_MonData(summaryScreen);
            mon = Pokemon.Pokemon_New(HeapIDSummary);
            Pokemon.Pokemon_FromBoxPokemon(boxMon, mon);
        }
        else
        {
            mon = (Pokemon)PokemonSummaryScreen_MonData(summaryScreen);
        }
        Unported.sub_02098EF8(data.Poffin, mon);
        PokemonSummaryMonData monData = summaryScreen.MonData;
        summaryScreen.PageState = 0;
        if (UpdateConditionStat(ref monData.Cool, mon, MonDataCool))
        {
            summaryScreen.PageState |= 1;
        }
        if (UpdateConditionStat(ref monData.Beauty, mon, MonDataBeauty))
        {
            summaryScreen.PageState |= 2;
        }
        if (UpdateConditionStat(ref monData.Cute, mon, MonDataCute))
        {
            summaryScreen.PageState |= 4;
        }
        if (UpdateConditionStat(ref monData.Smart, mon, MonDataSmart))
        {
            summaryScreen.PageState |= 8;
        }
        if (UpdateConditionStat(ref monData.Tough, mon, MonDataTough))
        {
            summaryScreen.PageState |= 0x10;
        }
        monData.Sheen = (byte)Pokemon.Pokemon_GetValue(mon, MonDataSheen, null);
        Font.Font_LoadScreenIndicatorsPalette(0, 0x1C0, HeapIDSummary);
        RenderWindow.LoadMessageBoxGraphics(summaryScreen.BgConfig!, 1, 0x3E2, 0xD, (byte)Options.Options_Frame(data.Options!), HeapIDSummary);
        if (summaryScreen.PageState == 0)
        {
            PokemonSummaryScreen_PrintPoffinFeedMsg(summaryScreen, 0xFE);
            return StatePoffinMessage;
        }
        PokemonSummaryScreen_InitMaxAndDeltaConditionRects(summaryScreen);
        PokemonSummaryScreen_InitSheenSprites(summaryScreen);
        PokemonSummaryScreen_UpdateConditionArrowSprites(summaryScreen);
        return StateContestStatMessages;
    }

    // OFFSET 0x0208D114
    private static int PrintContestStatChangeMsgs(PokemonSummaryScreen summaryScreen)
    {
        if ((gSystem.PressedKeys & (KeyA | KeyB)) == 0)
        {
            return StateContestStatMessages;
        }
        for (byte i = 0; i < 5; i++)
        {
            int bit = 1 << i;
            if ((summaryScreen.PageState & bit) != 0)
            {
                PokemonSummaryScreen_PrintPoffinFeedMsg(summaryScreen, i);
                summaryScreen.PageState ^= (byte)bit;
                if (summaryScreen.PageState == 0)
                {
                    return StatePoffinMessage;
                }
                break;
            }
        }
        return StateContestStatMessages;
    }

    // OFFSET 0x0208D164
    private static int WaitForPoffinFeedMsgInput(PokemonSummaryScreen summaryScreen)
    {
        return (gSystem.PressedKeys & (KeyA | KeyB)) != 0 ? StateTransitionOut : StatePoffinMessage;
    }

    // OFFSET 0x0208D17C
    private static int SummaryScreenTransitionOut(PokemonSummaryScreen summaryScreen)
    {
        HealthBar.App_StartScreenFade(true, HeapIDSummary);
        return StateExit;
    }

    // OFFSET 0x0208D18C
    private static byte ScreenTransitionIsDone(PokemonSummaryScreen summaryScreen)
    {
        return (byte)(ScreenFadeControl.IsScreenFadeDone() ? 1 : 0);
    }

    // OFFSET 0x0208D1A4
    private static void SetMonData(PokemonSummaryScreen summaryScreen)
    {
        object mon = PokemonSummaryScreen_MonData(summaryScreen);
        if (summaryScreen.Data!.DataType == DataTypeBoxMon)
        {
            SetMonDataFromBoxMon(summaryScreen, (BoxPokemon)mon, summaryScreen.MonData);
            return;
        }
        SetMonDataFromMon(summaryScreen, (Pokemon)mon, summaryScreen.MonData);
    }

    // OFFSET 0x0208D1D4
    private static void SetMonDataFromBoxMon(PokemonSummaryScreen summaryScreen, BoxPokemon boxMon, PokemonSummaryMonData monData)
    {
        Pokemon mon = Pokemon.Pokemon_New(HeapIDSummary);
        Pokemon.Pokemon_FromBoxPokemon(boxMon, mon);
        SetMonDataFromMon(summaryScreen, mon, monData);
    }

    private static uint MonValue(Pokemon mon, int param)
    {
        return Pokemon.Pokemon_GetValue(mon, param, null);
    }

    // OFFSET 0x0208D200
    private static void SetMonDataFromMon(PokemonSummaryScreen summaryScreen, Pokemon mon, PokemonSummaryMonData monData)
    {
        int decryptionContext = Pokemon.Pokemon_EnterDecryptionContext(mon);
        monData.Species = (ushort)MonValue(mon, MonDataSpecies);
        BoxPokemon boxMon = Pokemon.Pokemon_GetBoxPokemon(mon);
        StringTemplate template = summaryScreen.StrFormatter!;
        MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 0xB, summaryScreen.String!);
        StringTemplate.StringTemplate_SetSpeciesName(template, 0, boxMon);
        StringTemplate.StringTemplate_Format(template, monData.SpeciesName!, summaryScreen.String!);
        MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 0, summaryScreen.String!);
        StringTemplate.StringTemplate_SetNickname(template, 0, boxMon);
        StringTemplate.StringTemplate_Format(template, monData.Nickname!, summaryScreen.String!);
        MessageLoader.MessageLoader_GetString(summaryScreen.MsgLoader!, 0xE, summaryScreen.String!);
        StringTemplate.StringTemplate_SetOTName(template, 0, boxMon);
        StringTemplate.StringTemplate_Format(template, monData.OTName!, summaryScreen.String!);
        monData.HeldItem = (ushort)MonValue(mon, MonDataHeldItem);
        monData.Level = (byte)(MonValue(mon, MonDataLevel) & 0x7F);
        monData.IsEgg = (byte)(MonValue(mon, MonDataIsEgg) & 1);
        monData.HideGender = (byte)(MonValue(mon, MonDataHasNickname) == 1 && monData.IsEgg == 0 ? 0 : 1);
        monData.Gender = (byte)(Pokemon.Pokemon_GetGender(mon) & 3);
        monData.CaughtBall = (byte)(MonValue(mon, MonDataPokeball) & 0x3F);
        monData.Type1 = (byte)MonValue(mon, MonDataType1);
        monData.Type2 = (byte)MonValue(mon, MonDataType2);
        monData.OTID = MonValue(mon, MonDataOTID);
        monData.CurExp = MonValue(mon, MonDataExp);
        monData.OTGender = (byte)MonValue(mon, MonDataOTGender);
        monData.CurLevelExp = Pokemon.Species_GetExpForLevel(monData.Species, monData.Level);
        if (monData.Level == MaxLevel)
        {
            monData.NextLevelExp = monData.CurLevelExp;
        }
        else
        {
            monData.NextLevelExp = Pokemon.Species_GetExpForLevel(monData.Species, monData.Level + 1);
        }
        monData.CurHP = (ushort)MonValue(mon, MonDataCurrentHP);
        monData.MaxHP = (ushort)MonValue(mon, MonDataMaxHP);
        monData.Attack = (ushort)MonValue(mon, MonDataAttack);
        monData.Defense = (ushort)MonValue(mon, MonDataDefense);
        monData.SpAttack = (ushort)MonValue(mon, MonDataSpAttack);
        monData.SpDefense = (ushort)MonValue(mon, MonDataSpDefense);
        monData.Speed = (ushort)MonValue(mon, MonDataSpeed);
        monData.Ability = (byte)MonValue(mon, MonDataAbility);
        monData.Nature = Pokemon.Pokemon_GetNature(mon);
        for (int i = 0; i < PokemonSummaryMonData.LearnedMovesMax; i++)
        {
            monData.Moves[i] = (ushort)MonValue(mon, MonDataMove1 + i);
            monData.CurPP[i] = (byte)MonValue(mon, MonDataMove1PP + i);
            byte ppUps = (byte)MonValue(mon, MonDataMove1PPUps + i);
            monData.MaxPP[i] = (byte)MoveTable.MoveTable_CalcMaxPP(monData.Moves[i], ppUps);
        }
        monData.Cool = (byte)MonValue(mon, MonDataCool);
        monData.Beauty = (byte)MonValue(mon, MonDataBeauty);
        monData.Cute = (byte)MonValue(mon, MonDataCute);
        monData.Smart = (byte)MonValue(mon, MonDataSmart);
        monData.Tough = (byte)MonValue(mon, MonDataTough);
        monData.Sheen = (byte)MonValue(mon, MonDataSheen);
        monData.PreferredFlavor = FlavorNone;
        for (int i = 0; i < NumFlavors; i++)
        {
            if (Pokemon.Pokemon_GetFlavorAffinity(mon, i) == 1)
            {
                monData.PreferredFlavor = (byte)i;
                break;
            }
        }
        monData.Markings = (ushort)MonValue(mon, MonDataMarkings);
        monData.Form = (ushort)MonValue(mon, MonDataForm);
        monData.Status = (uint)(Pokemon.Pokemon_GetSummaryStatus(mon) & 0x0FFFFFFF);
        if (Pokemon.Pokemon_IsPokerusCured(mon))
        {
            monData.Pokerus = 2;
        }
        else if (Pokemon.Pokemon_HasPokerus(mon))
        {
            monData.Pokerus = 1;
            if (monData.Status == Pokemon.SummaryStatusNone)
            {
                monData.Status = 0;
            }
        }
        else
        {
            monData.Pokerus = 0;
        }
        monData.IsShiny = (byte)(Pokemon.Pokemon_IsShiny(mon) == 1 ? 1 : 0);
        for (int i = 0; i < monData.Ribbons.Length; i++)
        {
            monData.Ribbons[i] = 0;
        }
        summaryScreen.RibbonMax = 0;
        for (int i = 0; i < NumRibbons; i++)
        {
            if (MonValue(mon, (int)Ribbon.Ribbon_GetData((byte)i, Ribbon.ParamMonDataParam)) != 0)
            {
                monData.Ribbons[i >> 5] |= 1u << (i & 0x1F);
                summaryScreen.RibbonMax++;
            }
        }
        Pokemon.Pokemon_ExitDecryptionContext(mon, decryptionContext);
    }

    // OFFSET 0x0208D618
    private static void PlayMonCry(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummaryMonData monData = summaryScreen.MonData;
        if (monData.IsEgg != 0)
        {
            return;
        }
        PokemonSpriteMetadata.PokemonSprite_LoadCryDelay(summaryScreen.NarcPlPokeData!, out byte cryDelay, monData.Species, 1);
        if (monData.Species == SpeciesChatot)
        {
            SoundChatot.Sound_PlayDelayedChatotCry(summaryScreen.Data!.ChatotCry, 0, 0x64, 0, cryDelay);
            return;
        }
        SoundPlayback.Sound_PlayDelayedPokemonCry(monData.Species, cryDelay, (byte)monData.Form);
    }

    // OFFSET 0x0208D678
    private static void SetupInitialPageGfx(PokemonSummaryScreen summaryScreen)
    {
        switch (summaryScreen.Data!.Mode)
        {
            case ModeNormal:
            case ModeLock:
                summaryScreen.Page = (sbyte)(summaryScreen.MonData.IsEgg == 0 ? PageInfo : PageMemo);
                break;
            case ModeSelectMove:
                summaryScreen.Page = PageBattleMoves;
                break;
            case ModePoffin:
            case ModeCondition:
                summaryScreen.Page = PageCondition;
                break;
        }
        PokemonSummaryScreen_UpdateAButtonSprite(summaryScreen, null);
        PokemonSummaryScreen_UpdatePageTabSprites(summaryScreen);
        PokemonSummaryScreen_UpdateTypeIcons(summaryScreen);
        PokemonSummaryScreen_UpdateRibbonSprites(summaryScreen);
        PokemonSummaryScreen_SetPageArrowsPos(summaryScreen);
        PokemonSummaryScreen_UpdateMiscMonDataSprites(summaryScreen);
        PokemonSummaryScreen_InitSheenSprites(summaryScreen);
        PokemonSummaryScreen_AddExtraWindows(summaryScreen);
        PokemonSummaryScreen_DrawExtraWindows(summaryScreen);
        LoadCurrentPageTilemap(summaryScreen);
        PokemonSummaryScreen_InitConditionRects(summaryScreen);
        if (summaryScreen.Data.Mode == ModeSelectMove)
        {
            SetupMoveInfoNoTransition(summaryScreen);
        }
    }
}
