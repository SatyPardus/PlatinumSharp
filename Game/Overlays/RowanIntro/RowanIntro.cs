using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed class RowanIntroData
{
    public int HeapID;
    public SaveData? SaveData;
    public Options? Options;
    public int State;
    public int BufferedState;
    public ApplicationManager? AppMan;
    public BgConfig? BgConfig;
    public readonly Window TextWindow = new();
    public int ChoiceBoxState;
    public readonly Window ChoiceBoxWindow = new();
    public ListMenu? ListMenu;
    public StringListEntry[]? Choices;
    public int PlayerChoice;
    public MessageLoader? MsgLoader;
    public int DisplayMessageState;
    public int DisplayTextBlockState;
    public int TextPrinterID;
    public GameString? Text;
    public PaletteAnimator? PaletteAnimator;
    public StringTemplate? StrFormatter;
    public YesNoTouchMenu? YesNoTouchMenu;
    public object? Unused6C;
    public NamingScreenArgs? PlayerNamingScreenArgs;
    public NamingScreenArgs? RivalNamingScreenArgs;
    public int FadeBgLayerState;
    public int FadeBgLayerCurAlpha;
    public int FadeBgLayerCurAlphaInv;
    public int PlayerGender;
    public byte BgLayer3TilemapIndex;
    public byte BgLayer1TilemapIndex;
    public byte BgLayer2TilemapIndex;
    public byte BgSubLayer3TilemapIndex;
    public byte MaleRunAnimSpriteIndex;
    public byte MaleRunAnimUpdateCounter;
    public byte FemaleRunAnimSpriteIndex;
    public byte FemaleRunAnimUpdateCounter;
    public int DelayUpdateCounter;
    public int AnimProgress;
    public int AnimDelayUpdateCounter;
    public int BunearyAnimCarryover;
    public int BunearyParabolaCoeff;
    public int BunearyAnimUpdateCounter;
    public int BunearyPaletteBlendUpdateCounter;
    public ushort[]? BunearyPalette;
    public ushort[]? BunearyBlendedPalette;
}

public static class RowanIntro
{
    private const int Overlay = 73;
    private const int HeapID = 0x52;
    private const uint HeapSize = 0x40000;
    private const int GameStartOverlay = 0x39;
    private const uint GameStartNewGameTemplate = 0x021D0F80;
    private const uint NamingScreenTemplate = 0x020F2DAC;
    private const uint TvTemplate = 0x021D3820;
    private const uint GameVersionAddress = 0x020E4C40;
    private const uint BanksAddress = 0x021D392C;
    private const uint GraphicsModesAddress = 0x021D3810;
    private const uint MainBgTemplateAddress = 0x021D38D4;
    private const uint SubBgTemplateAddress = 0x021D38F0;
    private const uint MessageWindowTemplateAddress = 0x021D37EC;
    private const uint YesNoWindowTemplateAddress = 0x021D37C4;
    private const uint InfoWindowTemplateAddress = 0x021D37DC;
    private const uint RivalNamesWindowTemplateAddress = 0x021D37CC;
    private const uint CenteredTextBlockTemplateAddress = 0x021D37D4;
    private const uint TextBlockTemplateAddress = 0x021D37E4;
    private const uint YesNoChoicesAddress = 0x021D3840;
    private const uint InfoChoicesAddress = 0x021D38A4;
    private const uint RivalNameChoicesAddress = 0x021D3954;
    private const uint ChoiceBoxTemplateAddress = 0x021D390C;
    private const uint Layer3TilemapsAddress = 0x021D3850;
    private const uint TilemapLocationsAddress = 0x021D397C;
    private const uint SubLayer3TilemapsAddress = 0x021D3878;
    private const uint BunearyTilemapAddress = 0x021D39D4;
    private const uint YesNoParamsAddress = 0x021D3864;
    private const uint PokeballFramesAddress = 0x021D37F4;
    private const uint MaleRunFramesAddress = 0x021D3800;
    private const uint FemaleRunFramesAddress = 0x021D3830;
    private const uint MaleShrinkFramesAddress = 0x021D38BC;
    private const uint FemaleShrinkFramesAddress = 0x021D388C;
    private const uint AllLayersMask = 0xFFFFE0FF;
    private const int IntroArchive = 0x7D;
    private const int MessageArchive = 0x1A;
    private const int MessageBank = 0x185;
    private const uint MessageBoxTile = 0x3E2;
    private const uint WindowFrameTile = 0x3D9;
    private const int IntroBgm = 0x405;
    private const int SeConfirm = 0x5DC;
    private const int SePokeballOpen = 0x706;
    private const int SpeciesBuneary = 0x1AB;
    private const ushort BunearyBlendColor = 0x6A3C;
    private const int BunearyTilemapCells = 0x64;
    private const int VersionDiamond = 0xA;
    private const int VersionPlatinum = 0xC;
    private const int KeyA = 1;
    private const int KeyB = 2;
    private const int KeyRight = 0x10;
    private const int KeyLeft = 0x20;
    private const int ListNothingChosen = -1;
    private const int ListCancel = -2;
    private const int GenderMale = 0;
    private const int GenderFemale = 1;
    private const int DisplayMain = 0;
    private const int DisplaySub = 1;
    private const int FadeIn = 0;
    private const int FadeOut = 1;
    private const int MoveToCenter = 0;
    private const int MoveRight = 1;
    private const int MoveLeft = 2;
    private const int ChoicesYesNo = 0;
    private const int ChoicesInfo = 1;
    private const int ChoicesRivalName = 2;
    private const int TextBlockTopAligned = 0;
    private const int TextBlockCentered = 1;
    private const int TextBlockCenteredInArea = 2;

    private const int StateFirstFadeBlackStart = 0;
    private const int StateFirstFadeBlackEnd = 1;
    private const int StateDialogueWelcome = 2;
    private const int StateFadeInRowanStart = 3;
    private const int StateFadeInRowanEnd = 4;
    private const int StateDialogueRowanIntro = 5;
    private const int StateMoveRowanRightForInfo = 6;
    private const int StateInfoChoiceBox = 7;
    private const int StateInfoFadeBlackStart = 8;
    private const int StateInfoFadeBlackEnd = 9;
    private const int StateControlInfoFadeInStart = 10;
    private const int StateControlInfoFadeInEnd = 11;
    private const int StateControlInfoText0 = 12;
    private const int StateControlInfoShowXyIcons = 13;
    private const int StateControlInfoText1 = 14;
    private const int StateControlInfoHideXyIcons = 15;
    private const int StateControlInfoText2 = 16;
    private const int StateControlInfoDialogueDsIcon = 17;
    private const int StateControlInfoText3 = 18;
    private const int StateControlInfoHideFg = 19;
    private const int StateControlInfoDialogueUnderstood = 20;
    private const int StateControlInfoShowYesno = 21;
    private const int StateControlInfoWaitInput = 22;
    private const int StateControlInfoProcessYesno = 23;
    private const int StateControlInfoFadeOutStart = 24;
    private const int StateControlInfoFadeOutEnd = 25;
    private const int StateControlInfoRepeat = 26;
    private const int StateControlInfoDialogueUseTouchscreen = 27;
    private const int StateFadeInRowan2Start = 28;
    private const int StateFadeInRowan2End = 29;
    private const int StateDialogueAnotherInfo = 30;
    private const int StateAdventureInfoFadeInStart = 31;
    private const int StateAdventureInfoFadeInEnd = 32;
    private const int StateAdventureInfoText0 = 33;
    private const int StateAdventureInfoText1 = 34;
    private const int StateAdventureInfoText2 = 35;
    private const int StateAdventureInfoText3 = 36;
    private const int StateAdventureInfoText4 = 37;
    private const int StateAdventureInfoText5 = 38;
    private const int StateAdventureInfoFadeOutStart = 39;
    private const int StateAdventureInfoFadeOutEnd = 40;
    private const int StateLeftRowanAfterInfo = 41;
    private const int StateDialogueWidelyInhabited = 42;
    private const int StatePkblFadeStart = 43;
    private const int StatePkblFadeMiddle = 44;
    private const int StatePkblFadeEnd = 45;
    private const int StatePkblDialogue = 46;
    private const int StatePkblWaitInput = 47;
    private const int StatePkblAnimPushIn = 48;
    private const int StatePkblDialogueUseTouchscreen = 49;
    private const int StatePkblAnimFlash0 = 50;
    private const int StatePkblAnimFlash1 = 51;
    private const int StatePkblAnimFlash2 = 52;
    private const int StatePkblAnimFlash3 = 53;
    private const int StatePkblAnimSpawnPkmAndFlash4 = 54;
    private const int StatePkblAnimMvPkmUpAndFlashEnd = 55;
    private const int StatePkblAnimMvPkmDownAndBounce = 56;
    private const int StatePkblAnimEndDelay = 57;
    private const int StatePkblDialogueLiveAlongside = 58;
    private const int StatePkblPutAwayPkm = 59;
    private const int StateBetweenDialogueDelay = 60;
    private const int StateDialogueAboutYourself = 61;
    private const int StateGendrFadeOutRowan = 62;
    private const int StateGendrFadeInAvatarPrep = 63;
    private const int StateGendrFadeInAvatarMale = 64;
    private const int StateGendrFadeInAvatarFemale = 65;
    private const int StateGendrDialogue = 66;
    private const int StateGendrChoice = 67;
    private const int StateGendrFadeOutAvatarFemale = 68;
    private const int StateGendrCentreAvatarMale = 69;
    private const int StateGendrFadeOutAvatarMale = 70;
    private const int StateGendrCentreAvatarFemale = 71;
    private const int StateGendrConfirmDialogue = 72;
    private const int StateGendrConfirmChoiceBox = 73;
    private const int StateGendrRepeat = 74;
    private const int StateNameDialogue = 75;
    private const int StateNameAppKeyboard = 76;
    private const int StateNameKeyboard = 77;
    private const int StateNameKeyboardFadeIntoAvatar = 78;
    private const int StateNameConfirmDialogue = 79;
    private const int StateNameConfirmChoiceBox = 80;
    private const int StateNameFadeOutAvatar = 81;
    private const int StateLoadRowanTilemap0 = 82;
    private const int StateFadeInRowanAfterName = 83;
    private const int StateDialogueSoYoure = 84;
    private const int StateFadeOutRowanForRival = 85;
    private const int StateLoadRivalTilemap = 86;
    private const int StateFadeInRival = 87;
    private const int StateRivalNameDialogue = 88;
    private const int StateMoveRivalRightForNames = 89;
    private const int StateRivalNameChoiceBox = 90;
    private const int StateMoveRivalLeftAfterName = 91;
    private const int StateRivalNameAppKeyboard = 92;
    private const int StateRivalNameKeyboard = 93;
    private const int StateRivalNameKeyboardFadeIntoAvatar = 94;
    private const int StateRivalNameConfirmDialogue = 95;
    private const int StateRivalNameConfirmChoiceBox = 96;
    private const int StateFadeOutRival = 97;
    private const int StateLoadRowanTilemap1 = 98;
    private const int StateFadeInRowanAfterRival = 99;
    private const int StateDelayBeforeEnd0 = 100;
    private const int StateDialogueEnd = 101;
    private const int StateFadeOutRowanEnd = 102;
    private const int StateDelayBeforeEnd1 = 103;
    private const int StateLoadMiniAvatar = 104;
    private const int StateFadeInAvatarEnd = 105;
    private const int StateDelayBeforeEnd2 = 106;
    private const int StateAvatarShrinkAnimation = 107;
    private const int StateEnd = 108;
    private const int StateExit = 109;

    private static WindowTemplate WindowTemplateAt(uint address)
    {
        return RomStructs.WindowTemplate(RomData.Overlay(Overlay, address, RomStructs.WindowTemplateSize), 0);
    }

    private static BgTemplate BgTemplateAt(uint address, byte screenBase, byte charBase)
    {
        BgTemplate template = RomStructs.BgTemplate(RomData.Overlay(Overlay, address, RomStructs.BgTemplateSize), 0);
        template.ScreenBase = screenBase;
        template.CharBase = charBase;
        return template;
    }

    private static ListMenuTemplate ListMenuTemplateAt(uint address)
    {
        byte[] raw = RomData.Overlay(Overlay, address, 0x20);
        int spacing = Bin.U16(raw, 0x1A);
        return new ListMenuTemplate
        {
            Count = (ushort)Bin.U16(raw, 0x10),
            MaxDisplay = (ushort)Bin.U16(raw, 0x12),
            HeaderXOffset = raw[0x14],
            TextXOffset = raw[0x15],
            CursorXOffset = raw[0x16],
            YOffset = raw[0x17] & 0xF,
            TextColorFg = raw[0x17] >> 4,
            TextColorBg = raw[0x18] & 0xF,
            TextColorShadow = raw[0x18] >> 4,
            LetterSpacing = spacing & 7,
            LineSpacing = (spacing >> 3) & 0xF,
            PagerMode = (spacing >> 7) & 3,
            FontID = (spacing >> 9) & 0x3F,
            CursorType = (spacing >> 15) & 1,
        };
    }

    private static void SetBlendAlpha(int display, int plane, RowanIntroData data)
    {
        Video.SetBlendAlpha(display, plane, 0xE, data.FadeBgLayerCurAlpha, data.FadeBgLayerCurAlphaInv);
    }

    // OFFSET 0x021D0D80
    public static int RowanIntro_Init(ApplicationManager appMan, ref int state)
    {
        Heap.Heap_Create(3, HeapID, HeapSize);
        RowanIntroData data = new();
        appMan.Data = data;
        data.HeapID = HeapID;
        data.SaveData = ((ApplicationArgs)ApplicationManager.ApplicationManager_Args(appMan)!).SaveData;
        data.Options = PlayerSave.SaveData_GetOptions(data.SaveData!);
        data.State = 0;
        data.BufferedState = 0;
        data.AppMan = null;
        data.PlayerNamingScreenArgs = NamingScreenArgs.NamingScreenArgs_Init(HeapID, 0, 0, 7, data.Options);
        data.RivalNamingScreenArgs = NamingScreenArgs.NamingScreenArgs_Init(HeapID, 3, 0, 7, data.Options);
        data.BgLayer3TilemapIndex = 0;
        data.BgLayer1TilemapIndex = 0;
        data.BgLayer2TilemapIndex = 0;
        data.BgSubLayer3TilemapIndex = 0;
        data.DelayUpdateCounter = 0;
        data.BunearyPalette = new ushort[0x10];
        data.BunearyBlendedPalette = new ushort[0x10];
        return 1;
    }

    // OFFSET 0x021D0E20
    public static int RowanIntro_Main(ApplicationManager appMan, ref int state)
    {
        RowanIntroData data = (RowanIntroData)ApplicationManager.ApplicationManager_Data(appMan)!;
        int finished = 0;
        switch (state)
        {
            case 0:
                ScreenFadeControl.SetScreenColorBrightness(0, 0);
                ScreenFadeControl.SetScreenColorBrightness(1, 0);
                GameSystem.SetVBlankCallback(null, null);
                GameSystem.SetHBlankCallback(null, null);
                GXLayers.GXLayers_DisableEngineALayers();
                GXLayers.GXLayers_DisableEngineBLayers();
                Video.SetDisplayControl(0, Video.GetDisplayControl(0) & AllLayersMask);
                Video.SetDisplayControl(1, Video.GetDisplayControl(1) & AllLayersMask);
                GameSystem.SetAutorepeat(4, 8);
                RowanIntro_InitGraphics(data);
                RowanIntro_InitMessageStructs(data);
                RowanIntro_InitYesNoTouchMenu(data);
                GameSystem.SetVBlankCallback(RowanIntro_VBlankCallback, data);
                GXLayers.GXLayers_TurnBothDispOn();
                state = 1;
                break;
            case 1:
                if (RowanIntro_Run(data))
                {
                    ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 6, 1, data.HeapID);
                    state = 2;
                }
                if (data.AppMan != null)
                {
                    ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 6, 1, data.HeapID);
                    state = 3;
                }
                break;
            case 2:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    RowanIntro_FreeMessageStructs(data);
                    RowanIntro_FreeGraphics(data);
                    RowanIntro_FreeYesNoTouchMenu(data);
                    GameSystem.SetVBlankCallback(null, null);
                    finished = 1;
                }
                break;
            case 3:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    RowanIntro_FreeMessageStructs(data);
                    RowanIntro_FreeGraphics(data);
                    RowanIntro_FreeYesNoTouchMenu(data);
                    GameSystem.SetVBlankCallback(null, null);
                    state = 4;
                }
                break;
            case 4:
                if (ApplicationManager.ApplicationManager_Exec(data.AppMan!))
                {
                    ApplicationManager.ApplicationManager_Free(data.AppMan!);
                    data.AppMan = null;
                    state = 5;
                }
                break;
            case 5:
                state = 0;
                break;
        }
        return finished;
    }

    // OFFSET 0x021D0F7C
    public static int RowanIntro_Exit(ApplicationManager appMan, ref int state)
    {
        RowanIntroData data = (RowanIntroData)ApplicationManager.ApplicationManager_Data(appMan)!;
        int heapID = data.HeapID;
        data.BunearyPalette = null;
        data.BunearyBlendedPalette = null;
        TrainerInfo.TrainerInfo_SetNameFromString(PlayerSave.SaveData_GetTrainerInfo(data.SaveData!), data.PlayerNamingScreenArgs!.TextInputStr!);
        TrainerInfo.TrainerInfo_SetGender(PlayerSave.SaveData_GetTrainerInfo(data.SaveData!), data.PlayerNamingScreenArgs.PlayerGenderOrMonSpecies);
        MiscSaveBlock.MiscSaveBlock_SetRivalName(MiscSaveBlock.SaveData_MiscSaveBlock(data.SaveData!), data.RivalNamingScreenArgs!.TextInputStr!);
        NamingScreenArgs.NamingScreenArgs_Free(data.PlayerNamingScreenArgs);
        NamingScreenArgs.NamingScreenArgs_Free(data.RivalNamingScreenArgs);
        ApplicationManager.ApplicationManager_FreeData(appMan);
        Heap.Heap_Destroy(heapID);
        Main.EnqueueApplication(GameStartOverlay, ApplicationTemplate.At(GameStartNewGameTemplate));
        return 1;
    }

    // OFFSET 0x021D0FF0
    private static void RowanIntro_VBlankCallback(object? param)
    {
        RowanIntroData data = (RowanIntroData)param!;
        Bg.Bg_RunScheduledUpdates(data.BgConfig!);
    }

    // OFFSET 0x021D0FFC
    private static bool RowanIntro_WasPokeballOpened()
    {
        if (gSystem.TouchPressed == 0)
        {
            return false;
        }
        int x = gSystem.TouchX;
        int y = gSystem.TouchY;
        if (x >= 0x100 || y >= 0xC0)
        {
            return false;
        }
        int dx = x < 0x80 ? 0x80 - x : x - 0x80;
        int dy = y < 0x64 ? 0x64 - y : y - 0x64;
        return dx * dx + dy * dy <= 0x100;
    }

    // OFFSET 0x021D1058
    private static void RowanIntro_InitGraphics(RowanIntroData data)
    {
        GXLayers.GXLayers_SetBanks(RomStructs.Banks(RomData.Overlay(Overlay, BanksAddress, RomStructs.BanksSize), 0));
        data.BgConfig = Bg.BgConfig_New(data.HeapID);
        Bg.SetAllGraphicsModes(RomStructs.GraphicsModes(RomData.Overlay(Overlay, GraphicsModesAddress, RomStructs.GraphicsModesSize), 0));
        Bg.Bg_InitFromTemplate(data.BgConfig, 0, BgTemplateAt(MainBgTemplateAddress, 0xF, 6), 0);
        Bg.Bg_ClearTilemap(data.BgConfig, 0);
        Bg.Bg_InitFromTemplate(data.BgConfig, 1, BgTemplateAt(MainBgTemplateAddress, 0xE, 5), 0);
        Bg.Bg_ClearTilemap(data.BgConfig, 1);
        Bg.Bg_InitFromTemplate(data.BgConfig, 2, BgTemplateAt(MainBgTemplateAddress, 0xD, 4), 0);
        Bg.Bg_ClearTilemap(data.BgConfig, 2);
        Bg.Bg_InitFromTemplate(data.BgConfig, 3, BgTemplateAt(MainBgTemplateAddress, 0xC, 3), 0);
        Bg.Bg_ClearTilemap(data.BgConfig, 3);
        RenderWindow.LoadMessageBoxGraphics(data.BgConfig, 0, MessageBoxTile, 4, 0, data.HeapID);
        RenderWindow.LoadStandardWindowGraphics(data.BgConfig, 0, WindowFrameTile, 3, 0, data.HeapID);
        Font.Font_LoadTextPalette(0, 0xA0, data.HeapID);
        Font.Font_LoadScreenIndicatorsPalette(0, 0xC0, data.HeapID);
        Bg.Bg_InitFromTemplate(data.BgConfig, 4, BgTemplateAt(SubBgTemplateAddress, 0xF, 6), 0);
        Bg.Bg_ClearTilemap(data.BgConfig, 4);
        Bg.Bg_InitFromTemplate(data.BgConfig, 5, BgTemplateAt(SubBgTemplateAddress, 0xE, 5), 0);
        Bg.Bg_ClearTilemap(data.BgConfig, 5);
        Bg.Bg_InitFromTemplate(data.BgConfig, 6, BgTemplateAt(SubBgTemplateAddress, 0xD, 4), 0);
        Bg.Bg_ClearTilemap(data.BgConfig, 6);
        Bg.Bg_InitFromTemplate(data.BgConfig, 7, BgTemplateAt(SubBgTemplateAddress, 0xC, 3), 0);
        Bg.Bg_ClearTilemap(data.BgConfig, 7);
        Bg.Bg_ToggleLayer(0, 0);
        Bg.Bg_ToggleLayer(1, 0);
        Bg.Bg_ToggleLayer(2, 0);
        Bg.Bg_ToggleLayer(3, 0);
        Bg.Bg_ToggleLayer(4, 0);
        Bg.Bg_ToggleLayer(5, 0);
        Bg.Bg_ToggleLayer(6, 0);
        Bg.Bg_ToggleLayer(7, 0);
        RowanIntro_LoadInitialTilemaps(data);
        data.FadeBgLayerState = 0;
    }

    // OFFSET 0x021D1238
    private static void RowanIntro_FreeGraphics(RowanIntroData data)
    {
        Bg.Bg_ToggleLayer(0, 0);
        Bg.Bg_ToggleLayer(1, 0);
        Bg.Bg_ToggleLayer(2, 0);
        Bg.Bg_ToggleLayer(3, 0);
        Bg.Bg_ToggleLayer(4, 0);
        Bg.Bg_ToggleLayer(5, 0);
        Bg.Bg_ToggleLayer(6, 0);
        Bg.Bg_ToggleLayer(7, 0);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 0);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 1);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 2);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 3);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 4);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 5);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 6);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 7);
        data.BgConfig = null;
    }

    // OFFSET 0x021D12C4
    private static void RowanIntro_InitMessageStructs(RowanIntroData data)
    {
        data.MsgLoader = MessageLoader.MessageLoader_Init(1, MessageArchive, MessageBank, data.HeapID);
        Text.Text_ResetAllPrinters();
        data.PaletteAnimator = PaletteAnimator.PaletteAnimator_New(null, 0, 6, data.HeapID);
        data.StrFormatter = StringTemplate.StringTemplate_Default(data.HeapID);
        data.DisplayMessageState = 0;
        data.DisplayTextBlockState = 0;
        data.ChoiceBoxState = 0;
    }

    // OFFSET 0x021D1300
    private static void RowanIntro_FreeMessageStructs(RowanIntroData data)
    {
        StringTemplate.StringTemplate_Free(data.StrFormatter!);
        PaletteAnimator.PaletteAnimator_Free(data.PaletteAnimator!);
        MessageLoader.MessageLoader_Free(data.MsgLoader);
    }

    // OFFSET 0x021D1318
    private static void RowanIntro_InitYesNoTouchMenu(RowanIntroData data)
    {
        data.YesNoTouchMenu = YesNoTouchMenu.YesNoTouchMenu_New(data.HeapID);
    }

    // OFFSET 0x021D1328
    private static void RowanIntro_FreeYesNoTouchMenu(RowanIntroData data)
    {
        YesNoTouchMenu.YesNoTouchMenu_Free(data.YesNoTouchMenu!);
    }

    // OFFSET 0x021D1334
    private static bool RowanIntro_FadeBgLayer(RowanIntroData data, int bgLayer, int fadeOut)
    {
        bool finished = false;
        int plane;
        int display;
        switch (bgLayer)
        {
            case 1:
                plane = 2;
                display = DisplayMain;
                break;
            case 2:
                plane = 4;
                display = DisplayMain;
                break;
            case 4:
                plane = 1;
                display = DisplaySub;
                break;
            case 5:
                plane = 2;
                display = DisplaySub;
                break;
            case 6:
                plane = 4;
                display = DisplaySub;
                break;
            default:
                plane = 1;
                display = DisplayMain;
                break;
        }
        switch (data.FadeBgLayerState)
        {
            case 0:
                if (fadeOut == FadeIn)
                {
                    data.FadeBgLayerCurAlpha = 0;
                    data.FadeBgLayerCurAlphaInv = 0x10;
                    data.FadeBgLayerState = 1;
                    SetBlendAlpha(display, plane, data);
                    Bg.Bg_ToggleLayer((byte)bgLayer, 1);
                }
                else
                {
                    data.FadeBgLayerCurAlpha = 0x10;
                    data.FadeBgLayerCurAlphaInv = 0;
                    data.FadeBgLayerState = 2;
                }
                break;
            case 1:
                if (data.FadeBgLayerCurAlphaInv != 0)
                {
                    data.FadeBgLayerCurAlpha++;
                    data.FadeBgLayerCurAlphaInv--;
                    SetBlendAlpha(display, plane, data);
                }
                else
                {
                    data.FadeBgLayerState = 3;
                }
                break;
            case 2:
                if (data.FadeBgLayerCurAlpha != 0)
                {
                    data.FadeBgLayerCurAlpha--;
                    data.FadeBgLayerCurAlphaInv++;
                    SetBlendAlpha(display, plane, data);
                }
                else
                {
                    data.FadeBgLayerState = 3;
                    Bg.Bg_ToggleLayer((byte)bgLayer, 0);
                }
                break;
            case 3:
                Video.SetBlendControl(DisplayMain, 0);
                Video.SetBlendControl(DisplaySub, 0);
                finished = true;
                data.FadeBgLayerState = 0;
                break;
        }
        return finished;
    }

    // OFFSET 0x021D14B8
    private static bool RowanIntro_Delay(RowanIntroData data, int updateCount)
    {
        if (data.DelayUpdateCounter < updateCount)
        {
            data.DelayUpdateCounter++;
            return false;
        }
        data.DelayUpdateCounter = 0;
        return true;
    }

    // OFFSET 0x021D14DC
    private static void RowanIntro_ChangePaletteAndCopyTilemap(RowanIntroData data, int bgLayer, int palette)
    {
        Bg.Bg_ChangeTilemapRectPalette(data.BgConfig!, (byte)bgLayer, 0, 0, 0x20, 0x18, (byte)palette);
        Bg.Bg_CopyTilemapBufferToVRAM(data.BgConfig!, (byte)bgLayer);
    }

    // OFFSET 0x021D1510
    private static bool RowanIntro_DisplayMessage(RowanIntroData data, int textID, int endEarly)
    {
        bool finished = false;
        switch (data.DisplayMessageState)
        {
            case 0:
            {
                Bg.Window_AddFromTemplate(data.BgConfig!, data.TextWindow, WindowTemplateAt(MessageWindowTemplateAddress));
                Bg.Window_FillRectWithColor(data.TextWindow, 0xF, 0, 0, 0xD8, 0x20);
                RenderWindow.Window_DrawMessageBoxWithScrollCursor(data.TextWindow, 0, (ushort)MessageBoxTile, 4);
                RenderText.RenderControlFlags_SetCanABSpeedUpPrint(1);
                RenderText.RenderControlFlags_SetAutoScrollFlags(0);
                GameString format = GameString.String_Init(0x400, data.HeapID);
                data.Text = GameString.String_Init(0x400, data.HeapID);
                MessageLoader.MessageLoader_GetString(data.MsgLoader!, (uint)textID, format);
                StringTemplate.StringTemplate_SetString(data.StrFormatter!, 0, data.PlayerNamingScreenArgs!.TextInputStr!, data.PlayerGender, 1, 2);
                StringTemplate.StringTemplate_SetString(data.StrFormatter!, 1, data.RivalNamingScreenArgs!.TextInputStr!, 0, 1, 2);
                StringTemplate.StringTemplate_Format(data.StrFormatter!, data.Text, format);
                GameString.String_Free(format);
                data.TextPrinterID = Text.Text_AddPrinterWithParams(data.TextWindow, 1, data.Text, 0, 0, Options.Options_TextFrameDelay(data.Options!), null);
                data.DisplayMessageState = 1;
                break;
            }
            case 1:
                if (Text.Text_IsPrinterActive((byte)data.TextPrinterID) == 0)
                {
                    GameString.String_Free(data.Text);
                    data.DisplayMessageState = 2;
                }
                break;
            case 2:
                if (endEarly != 0 || (gSystem.PressedKeys & KeyA) == KeyA)
                {
                    Bg.Window_Remove(data.TextWindow);
                    data.DisplayMessageState = 0;
                    finished = true;
                }
                break;
        }
        return finished;
    }

    // OFFSET 0x021D1634
    private static void RowanIntro_ListMenuCursorCallback(ListMenu menu, uint index, byte onInit)
    {
        if (onInit == 0)
        {
            SoundPlayback.Sound_PlayEffect(SeConfirm);
        }
    }

    // OFFSET 0x021D1648
    private static bool RowanIntro_ChoiceBox(RowanIntroData data, int choicesCase, int ignoreCancel)
    {
        bool finished = false;
        switch (data.ChoiceBoxState)
        {
            case 0:
            {
                uint windowTemplate;
                uint choicesAddress;
                int count;
                switch (choicesCase)
                {
                    case ChoicesInfo:
                        windowTemplate = InfoWindowTemplateAddress;
                        choicesAddress = InfoChoicesAddress;
                        count = 3;
                        break;
                    case ChoicesRivalName:
                        windowTemplate = RivalNamesWindowTemplateAddress;
                        choicesAddress = RivalNameChoicesAddress;
                        count = 5;
                        break;
                    default:
                        windowTemplate = YesNoWindowTemplateAddress;
                        choicesAddress = YesNoChoicesAddress;
                        count = 2;
                        break;
                }
                Bg.Window_AddFromTemplate(data.BgConfig!, data.ChoiceBoxWindow, WindowTemplateAt(windowTemplate));
                data.Choices = StringList.StringList_New((uint)count, data.HeapID);
                uint[] choices = RomData.OverlayU32(Overlay, choicesAddress, count * 2);
                for (int i = 0; i < count; i++)
                {
                    StringList.StringList_AddFromMessageBank(data.Choices, data.MsgLoader!, choices[i * 2], choices[i * 2 + 1]);
                }
                ListMenuTemplate template = ListMenuTemplateAt(ChoiceBoxTemplateAddress);
                template.Choices = data.Choices;
                template.Count = (ushort)count;
                template.MaxDisplay = (ushort)count;
                template.CursorCallback = RowanIntro_ListMenuCursorCallback;
                template.Window = data.ChoiceBoxWindow;
                data.ListMenu = ListMenu.ListMenu_New(template, 0, 0, (byte)data.HeapID);
                RenderWindow.Window_DrawStandardFrame(data.ChoiceBoxWindow, 1, (ushort)WindowFrameTile, 3);
                Bg.Window_CopyToVRAM(data.ChoiceBoxWindow);
                data.ChoiceBoxState = 1;
                break;
            }
            case 1:
                data.PlayerChoice = (int)ListMenu.ListMenu_ProcessInput(data.ListMenu!);
                if (data.PlayerChoice == ListNothingChosen)
                {
                    break;
                }
                if (data.PlayerChoice == ListCancel && ignoreCancel == 1)
                {
                    break;
                }
                RenderWindow.Window_EraseStandardFrame(data.ChoiceBoxWindow, 0);
                Bg.Window_Remove(data.ChoiceBoxWindow);
                ListMenu.ListMenu_Free(data.ListMenu!, out _, out _);
                StringList.StringList_Free(data.Choices!);
                SoundPlayback.Sound_PlayEffect(SeConfirm);
                data.ChoiceBoxState = 0;
                finished = true;
                break;
        }
        return finished;
    }

    // OFFSET 0x021D1784
    private static bool RowanIntro_DisplayTextBlock(RowanIntroData data, int textID, int blockCase, int top, int height)
    {
        bool finished = false;
        switch (data.DisplayTextBlockState)
        {
            case 0:
            {
                Bg.Bg_ToggleLayer(0, 0);
                data.Text = GameString.String_Init(0x400, data.HeapID);
                MessageLoader.MessageLoader_GetString(data.MsgLoader!, (uint)textID, data.Text);
                if (blockCase == TextBlockCentered)
                {
                    WindowTemplate template = WindowTemplateAt(CenteredTextBlockTemplateAddress);
                    uint lines = GameString.String_NumLines(data.Text);
                    template.TilemapTop = (byte)(0xC - lines);
                    template.Height = (byte)(lines << 1);
                    Bg.Window_AddFromTemplate(data.BgConfig!, data.TextWindow, template);
                    Bg.Window_FillRectWithColor(data.TextWindow, 0, 0, 0, 0xC0, 0xC0);
                    Text.Text_AddPrinterWithParamsAndColor(data.TextWindow, 0, data.Text, 0, 0, 0, 0x00010200, null);
                }
                else
                {
                    WindowTemplate template = WindowTemplateAt(TextBlockTemplateAddress);
                    if (blockCase == TextBlockCenteredInArea)
                    {
                        uint lines = GameString.String_NumLines(data.Text);
                        template.TilemapTop = (byte)(top + height / 2 - lines);
                        template.Height = (byte)(lines << 1);
                    }
                    else
                    {
                        template.TilemapTop = (byte)top;
                        template.Height = (byte)height;
                    }
                    Bg.Window_AddFromTemplate(data.BgConfig!, data.TextWindow, template);
                    Bg.Window_FillRectWithColor(data.TextWindow, 0, 0, 0, 0xC0, 0xC0);
                    Text.Text_AddPrinterWithParamsAndColor(data.TextWindow, 0, data.Text, 0, 0, 0, 0x000F0200, null);
                }
                GameString.String_Free(data.Text);
                data.DisplayTextBlockState = 1;
                break;
            }
            case 1:
                Bg.Window_CopyToVRAM(data.TextWindow);
                data.DisplayTextBlockState = 2;
                break;
            case 2:
                if (RowanIntro_FadeBgLayer(data, 0, FadeIn))
                {
                    data.DisplayTextBlockState = 3;
                }
                break;
            case 3:
                if ((gSystem.PressedKeys & KeyA) == KeyA || (gSystem.PressedKeys & KeyB) == KeyB)
                {
                    SoundPlayback.Sound_PlayEffect(SeConfirm);
                    data.DisplayTextBlockState = 4;
                }
                break;
            case 4:
                if (RowanIntro_FadeBgLayer(data, 0, FadeOut))
                {
                    data.DisplayTextBlockState = 5;
                }
                break;
            case 5:
                Bg.Window_Remove(data.TextWindow);
                Bg.Bg_ClearTilemap(data.BgConfig!, 0);
                data.DisplayTextBlockState = 0;
                finished = true;
                break;
        }
        return finished;
    }

    // OFFSET 0x021D1930
    private static void RowanIntro_LoadInitialTilemaps(RowanIntroData data)
    {
        Graphics.Graphics_LoadTilesToBgLayer(IntroArchive, 0, data.BgConfig!, 3, 0, 0, 0, data.HeapID);
        Bg.Bg_ClearTilesRange(0, 0x20, 0, data.HeapID);
        Graphics.Graphics_LoadTilesToBgLayer(IntroArchive, 0x18, data.BgConfig!, 7, 0, 0, 0, data.HeapID);
        int version = RomData.Arm9(GameVersionAddress, 1)[0];
        uint mainPalette;
        uint subPalette;
        if (version == VersionPlatinum)
        {
            mainPalette = 3;
            subPalette = 0x1B;
        }
        else if (version == VersionDiamond)
        {
            mainPalette = 1;
            subPalette = 0x19;
        }
        else
        {
            mainPalette = 2;
            subPalette = 0x1A;
        }
        Graphics.Graphics_LoadPalette(IntroArchive, mainPalette, 0, 0, 0x60, data.HeapID);
        Graphics.Graphics_LoadPalette(IntroArchive, subPalette, 4, 0, 0xA0, data.HeapID);
        RowanIntro_LoadLayer3Tilemap(data);
        RowanIntro_LoadTilemap(data);
        RowanIntro_LoadSubLayer3Tilemap(data);
        Bg.Bg_MaskPalette(0, 0);
        Bg.Bg_MaskPalette(4, 0);
    }

    // OFFSET 0x021D19DC
    private static void RowanIntro_LoadLayer3Tilemap(RowanIntroData data)
    {
        uint[] tilemaps = RomData.OverlayU32(Overlay, Layer3TilemapsAddress, 5);
        if (data.BgLayer3TilemapIndex < 5)
        {
            Graphics.Graphics_LoadTilemapToBgLayer(IntroArchive, tilemaps[data.BgLayer3TilemapIndex], data.BgConfig!, 3, 0, 0, 0, data.HeapID);
        }
    }

    // OFFSET 0x021D1A20
    private static void RowanIntro_LoadTilemap(RowanIntroData data)
    {
        uint[] locations = RomData.OverlayU32(Overlay, TilemapLocationsAddress, 0x16);
        if (data.BgLayer1TilemapIndex != 0 && data.BgLayer1TilemapIndex < 0xC)
        {
            Graphics.Graphics_LoadTilesToBgLayer(IntroArchive, locations[data.BgLayer1TilemapIndex * 2], data.BgConfig!, 1, 0, 0, 0, data.HeapID);
            Graphics.Graphics_LoadPalette(IntroArchive, locations[data.BgLayer1TilemapIndex * 2 + 1], 0, 0xE0, 0x20, data.HeapID);
            Graphics.Graphics_LoadTilemapToBgLayer(IntroArchive, 0x17, data.BgConfig!, 1, 0, 0, 0, data.HeapID);
            RowanIntro_ChangePaletteAndCopyTilemap(data, 1, 7);
        }
        if (data.BgLayer2TilemapIndex != 0 && data.BgLayer2TilemapIndex < 0xC)
        {
            Graphics.Graphics_LoadTilesToBgLayer(IntroArchive, locations[data.BgLayer2TilemapIndex * 2], data.BgConfig!, 2, 0, 0, 0, data.HeapID);
            Graphics.Graphics_LoadPalette(IntroArchive, locations[data.BgLayer2TilemapIndex * 2 + 1], 0, 0x100, 0x20, data.HeapID);
            Graphics.Graphics_LoadTilemapToBgLayer(IntroArchive, 0x17, data.BgConfig!, 2, 0, 0, 0, data.HeapID);
            RowanIntro_ChangePaletteAndCopyTilemap(data, 2, 8);
        }
    }

    // OFFSET 0x021D1B14
    private static void RowanIntro_LoadSubLayer3Tilemap(RowanIntroData data)
    {
        uint[] tilemaps = RomData.OverlayU32(Overlay, SubLayer3TilemapsAddress, 5);
        if (data.BgSubLayer3TilemapIndex < 5)
        {
            Graphics.Graphics_LoadTilemapToBgLayer(IntroArchive, tilemaps[data.BgSubLayer3TilemapIndex], data.BgConfig!, 7, 0, 0, 0, data.HeapID);
            if (data.BgSubLayer3TilemapIndex == 1)
            {
                RowanIntro_ChangePaletteAndCopyTilemap(data, 7, 3);
            }
            else if (data.BgSubLayer3TilemapIndex == 2)
            {
                RowanIntro_ChangePaletteAndCopyTilemap(data, 7, 2);
            }
        }
    }

    // OFFSET 0x021D1B80
    private static void RowanIntro_LoadBunearySprite(RowanIntroData data)
    {
        PokemonSpriteTemplate template = new();
        PokemonSpriteTemplates.BuildPokemonSpriteTemplate(template, SpeciesBuneary, 0, 2, 0, 0, 0);
        int narcID = (int)(template.Word0 & 0xFFFF);
        int character = (int)(template.Word0 >> 16);
        int paletteIndex = (int)(template.Word1 & 0xFFFF);
        byte[] cells = RomData.Overlay(Overlay, BunearyTilemapAddress, BunearyTilemapCells);
        byte[] tilemap = new byte[BunearyTilemapCells * 2];
        for (int i = 0; i < BunearyTilemapCells; i++)
        {
            int tile = cells[i] + 1;
            tilemap[i * 2] = (byte)tile;
            tilemap[i * 2 + 1] = (byte)(tile >> 8);
        }
        byte[] tiles = CharacterSprite.CharacterSprite_LoadTiles(narcID, character, data.HeapID);
        ushort[] palette = CharacterSprite.CharacterSprite_LoadPalette(narcID, paletteIndex, data.HeapID);
        Array.Copy(palette, data.BunearyPalette!, 0x10);
        PaletteData.BlendPalettes(data.BunearyPalette!, palette, 1, 0x10, BunearyBlendColor);
        Bg.Bg_FillTilemapRect(data.BgConfig!, 2, 0, 0, 0, 0x20, 0x18, 8);
        Bg.Bg_LoadToTilemapRect(data.BgConfig!, 2, tilemap, 0xB, 9, 0xA, 0xA);
        RowanIntro_ChangePaletteAndCopyTilemap(data, 2, 8);
        Bg.Bg_ClearTilesRange(2, 0x20, 0, data.HeapID);
        Bg.Bg_LoadTiles(data.BgConfig!, 2, tiles, 0xC80, 1);
        Bg.Bg_LoadPalette(2, palette, 0, 0x20, 0x100);
        Bg.Bg_FillTilemapRect(data.BgConfig!, 5, 0, 0, 0, 0x20, 0x18, 0xA);
        Bg.Bg_LoadToTilemapRect(data.BgConfig!, 5, tilemap, 0xB, 7, 0xA, 0xA);
        RowanIntro_ChangePaletteAndCopyTilemap(data, 5, 0xA);
        Bg.Bg_ClearTilesRange(5, 0x20, 0, data.HeapID);
        Bg.Bg_LoadTiles(data.BgConfig!, 5, tiles, 0xC80, 1);
        Bg.Bg_LoadPalette(5, palette, 0, 0x20, 0x140);
    }

    // OFFSET 0x021D1CE0
    private static void RowanIntro_LoadPokeballTilemap(RowanIntroData data)
    {
        Graphics.Graphics_LoadTilemapToBgLayer(IntroArchive, 0x28, data.BgConfig!, 6, 0, 0, 0, data.HeapID);
        RowanIntro_ChangePaletteAndCopyTilemap(data, 6, 9);
        Graphics.Graphics_LoadPalette(IntroArchive, 0x29, 4, 0xE0, 0x60, data.HeapID);
        Bg.Bg_ClearTilesRange(6, 0x20, 0, data.HeapID);
        Graphics.Graphics_LoadTilesToBgLayer(IntroArchive, 0x20, data.BgConfig!, 6, 0x20, 0, 0, data.HeapID);
    }

    // OFFSET 0x021D1D44
    private static bool RowanIntro_MoveBgLayer(RowanIntroData data, int bgLayer, int moveCase)
    {
        bool finished = false;
        if (moveCase == MoveToCenter)
        {
            int x = Bg.Bg_GetXOffset(data.BgConfig!, bgLayer);
            if (x != 0)
            {
                if (x > 0)
                {
                    Bg.Bg_SetOffset(data.BgConfig!, (byte)bgLayer, Bg.OffsetSubX, 4);
                }
                else
                {
                    Bg.Bg_SetOffset(data.BgConfig!, (byte)bgLayer, Bg.OffsetAddX, 4);
                }
            }
            else
            {
                finished = true;
            }
        }
        else if (moveCase == MoveRight)
        {
            if (Bg.Bg_GetXOffset(data.BgConfig!, bgLayer) != -0x30)
            {
                Bg.Bg_SetOffset(data.BgConfig!, (byte)bgLayer, Bg.OffsetSubX, 4);
            }
            else
            {
                finished = true;
            }
        }
        else if (moveCase == MoveLeft)
        {
            if (Bg.Bg_GetXOffset(data.BgConfig!, bgLayer) != 0)
            {
                Bg.Bg_SetOffset(data.BgConfig!, (byte)bgLayer, Bg.OffsetAddX, 4);
            }
            else
            {
                finished = true;
            }
        }
        return finished;
    }

    // OFFSET 0x021D1DCC
    private static void RowanIntro_ResetAvatarRunAnimation(RowanIntroData data)
    {
        data.MaleRunAnimSpriteIndex = 0;
        data.MaleRunAnimUpdateCounter = 0;
        data.FemaleRunAnimSpriteIndex = 0;
        data.FemaleRunAnimUpdateCounter = 0;
    }

    // OFFSET 0x021D1DE8
    private static void RowanIntro_AnimateAvatarRun(RowanIntroData data)
    {
        if (data.PlayerGender == GenderMale)
        {
            if (data.MaleRunAnimUpdateCounter != 0)
            {
                data.MaleRunAnimUpdateCounter--;
            }
            else
            {
                data.MaleRunAnimSpriteIndex++;
                data.MaleRunAnimSpriteIndex &= 3;
                data.MaleRunAnimUpdateCounter = 4;
            }
            Video.SetBlendAlpha(DisplayMain, 4, 8, 6, 0xA);
            uint[] frames = RomData.OverlayU32(Overlay, MaleRunFramesAddress, 4);
            Graphics.Graphics_LoadTilesToBgLayer(IntroArchive, frames[data.MaleRunAnimSpriteIndex], data.BgConfig!, 1, 0, 0, 0, data.HeapID);
        }
        else
        {
            if (data.FemaleRunAnimUpdateCounter != 0)
            {
                data.FemaleRunAnimUpdateCounter--;
            }
            else
            {
                data.FemaleRunAnimSpriteIndex++;
                data.FemaleRunAnimSpriteIndex &= 3;
                data.FemaleRunAnimUpdateCounter = 4;
            }
            Video.SetBlendAlpha(DisplayMain, 2, 8, 6, 0xA);
            uint[] frames = RomData.OverlayU32(Overlay, FemaleRunFramesAddress, 4);
            Graphics.Graphics_LoadTilesToBgLayer(IntroArchive, frames[data.FemaleRunAnimSpriteIndex], data.BgConfig!, 2, 0, 0, 0, data.HeapID);
        }
    }

    // OFFSET 0x021D1EFC
    private static void RowanIntro_ResetBlending(RowanIntroData data)
    {
        Video.SetBlendControl(DisplayMain, 0);
    }

    // OFFSET 0x021D1F08
    private static void RowanIntro_ResetAnimationCounters(RowanIntroData data)
    {
        data.AnimProgress = 0;
        data.AnimDelayUpdateCounter = 0;
    }

    // OFFSET 0x021D1F18
    private static bool RowanIntro_AnimateAvatarShrink(RowanIntroData data)
    {
        bool finished = false;
        if (data.AnimDelayUpdateCounter != 0)
        {
            data.AnimDelayUpdateCounter--;
        }
        else
        {
            data.AnimProgress++;
            data.AnimDelayUpdateCounter = 8;
        }
        uint[] frames = RomData.OverlayU32(Overlay, data.PlayerGender == GenderMale ? MaleShrinkFramesAddress : FemaleShrinkFramesAddress, 6);
        uint frame = frames[data.AnimProgress];
        if (frame == 0xFF)
        {
            finished = true;
        }
        else
        {
            Graphics.Graphics_LoadTilesToBgLayer(IntroArchive, frame, data.BgConfig!, 1, 0, 0, 0, data.HeapID);
        }
        return finished;
    }

    // OFFSET 0x021D1FA0
    private static void RowanIntro_AnimateBuneary_BlendSpritePalette(RowanIntroData data)
    {
        if (data.BunearyPaletteBlendUpdateCounter > 0)
        {
            data.BunearyPaletteBlendUpdateCounter--;
            int fraction = data.BunearyPaletteBlendUpdateCounter / 3;
            PaletteData.BlendPalettes(data.BunearyPalette!, data.BunearyBlendedPalette!, 1, (byte)fraction, BunearyBlendColor);
            Bg.Bg_LoadPalette(2, data.BunearyBlendedPalette!, 0, 0x20, 0x100);
            Bg.Bg_LoadPalette(5, data.BunearyBlendedPalette!, 0, 0x20, 0x140);
        }
    }

    // OFFSET 0x021D200C
    private static bool RowanIntro_AnimateBuneary(RowanIntroData data, ref int state)
    {
        bool finished = false;
        switch (state)
        {
            case 0:
                Bg.Bg_SetOffset(data.BgConfig!, 2, Bg.OffsetSetX, 0);
                Bg.Bg_SetOffset(data.BgConfig!, 2, Bg.OffsetSetY, -0x68);
                Bg.Bg_SetPriority(2, 0);
                Bg.Bg_ToggleLayer(5, 1);
                data.BunearyAnimCarryover = 0;
                data.BunearyParabolaCoeff = 8;
                data.BunearyAnimUpdateCounter = 0;
                data.BunearyPaletteBlendUpdateCounter = 0x30;
                state = 1;
                break;
            case 1:
            {
                Bg.Bg_GetXOffset(data.BgConfig!, 2);
                int y = Bg.Bg_GetYOffset(data.BgConfig!, 5);
                int counter = data.BunearyAnimUpdateCounter;
                int offset = data.BunearyParabolaCoeff * 9 * counter - counter * 9 * counter / 2;
                if (y < 0x58)
                {
                    data.BunearyAnimUpdateCounter++;
                    Bg.Bg_SetOffset(data.BgConfig!, 2, Bg.OffsetAddX, 2);
                    Bg.Bg_SetOffset(data.BgConfig!, 5, Bg.OffsetAddX, 2);
                    Bg.Bg_SetOffset(data.BgConfig!, 5, Bg.OffsetSetY, offset);
                }
                else
                {
                    Bg.Bg_ToggleLayer(5, 0);
                    data.BunearyAnimUpdateCounter = 0;
                    state = 2;
                }
                break;
            }
            case 2:
                if (data.BunearyAnimUpdateCounter != 0)
                {
                    data.BunearyAnimUpdateCounter--;
                }
                else
                {
                    Bg.Bg_ToggleLayer(2, 1);
                    data.BunearyAnimCarryover = 0;
                    data.BunearyParabolaCoeff = 9;
                    data.BunearyAnimUpdateCounter = 0;
                    state = 3;
                }
                break;
            case 3:
            {
                Bg.Bg_GetXOffset(data.BgConfig!, 2);
                Bg.Bg_GetYOffset(data.BgConfig!, 2);
                int counter = data.BunearyAnimUpdateCounter;
                int offset = data.BunearyParabolaCoeff * 9 * counter - 0x68 - counter * 9 * counter / 2;
                if (data.BunearyAnimCarryover > 0 && offset <= 0)
                {
                    Bg.Bg_SetOffset(data.BgConfig!, 2, Bg.OffsetSetY, 0);
                    data.BunearyAnimCarryover = 0;
                    data.BunearyParabolaCoeff = 3;
                    data.BunearyAnimUpdateCounter = 0;
                    state = 4;
                }
                else
                {
                    data.BunearyAnimUpdateCounter++;
                    data.BunearyAnimCarryover = offset;
                    Bg.Bg_SetOffset(data.BgConfig!, 2, Bg.OffsetAddX, 1);
                    offset >>= 1;
                    if (offset > 0x90)
                    {
                        offset = 0x90;
                    }
                    Bg.Bg_SetOffset(data.BgConfig!, 2, Bg.OffsetSetY, offset);
                }
                break;
            }
            case 4:
            {
                Bg.Bg_GetXOffset(data.BgConfig!, 2);
                Bg.Bg_GetYOffset(data.BgConfig!, 2);
                int counter = data.BunearyAnimUpdateCounter;
                int offset = data.BunearyParabolaCoeff * 9 * counter - counter * 9 * counter / 3;
                if (data.BunearyAnimCarryover > 0 && offset <= 0)
                {
                    Bg.Bg_SetOffset(data.BgConfig!, 2, Bg.OffsetSetY, 0);
                    data.BunearyAnimCarryover = 0;
                    data.BunearyParabolaCoeff = 3;
                    data.BunearyAnimUpdateCounter = 0;
                    state = 5;
                }
                else
                {
                    data.BunearyAnimUpdateCounter++;
                    Bg.Bg_SetOffset(data.BgConfig!, 2, Bg.OffsetSubX, 2);
                    Bg.Bg_SetOffset(data.BgConfig!, 2, Bg.OffsetSetY, offset);
                    data.BunearyAnimCarryover = offset;
                }
                break;
            }
            case 5:
            {
                Bg.Bg_GetXOffset(data.BgConfig!, 2);
                Bg.Bg_GetYOffset(data.BgConfig!, 2);
                int counter = data.BunearyAnimUpdateCounter;
                int offset = data.BunearyParabolaCoeff * 9 * counter - counter * 9 * counter / 3;
                if (data.BunearyAnimCarryover > 0 && offset <= 0)
                {
                    Bg.Bg_SetOffset(data.BgConfig!, 2, Bg.OffsetSetY, 0);
                    state = 6;
                }
                else
                {
                    data.BunearyAnimUpdateCounter++;
                    Bg.Bg_SetOffset(data.BgConfig!, 2, Bg.OffsetAddX, 4);
                    Bg.Bg_SetOffset(data.BgConfig!, 2, Bg.OffsetSetY, offset);
                    data.BunearyAnimCarryover = offset;
                }
                break;
            }
            case 6:
                SoundPlayback.Sound_PlayPokemonCry(SpeciesBuneary, 0);
                finished = true;
                break;
        }
        RowanIntro_AnimateBuneary_BlendSpritePalette(data);
        return finished;
    }

    // OFFSET 0x021D230C
    private static void RowanIntro_SetBunearyLayerPriority(RowanIntroData data)
    {
        Bg.Bg_SetPriority(2, 1);
    }

    // OFFSET 0x021D2318
    private static bool RowanIntro_Run(RowanIntroData data)
    {
        bool finished = false;
        switch (data.State)
        {
            case StateFirstFadeBlackStart:
                Sound.Sound_SetSceneAndPlayBGM(2, IntroBgm, 1);
                SoundPlayback.Sound_StopBGM(IntroBgm, 0);
                Bg.Bg_ToggleLayer(0, 1);
                Bg.Bg_ToggleLayer(7, 1);
                ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 6, 1, data.HeapID);
                data.State = StateFirstFadeBlackEnd;
                break;
            case StateFirstFadeBlackEnd:
                if (ScreenFadeControl.IsScreenFadeDone() && RowanIntro_Delay(data, 0x28))
                {
                    data.State = StateDialogueWelcome;
                }
                break;
            case StateDialogueWelcome:
                if (RowanIntro_DisplayMessage(data, 0, 1))
                {
                    data.State = StateFadeInRowanStart;
                    Bg.Bg_ClearTilemap(data.BgConfig!, 0);
                }
                break;
            case StateFadeInRowanStart:
                SoundPlayback.Sound_PlayBGM(IntroBgm);
                data.BgLayer1TilemapIndex = 1;
                data.BgLayer2TilemapIndex = 0;
                RowanIntro_LoadTilemap(data);
                Bg.Bg_ToggleLayer(3, 1);
                Bg.Bg_ToggleLayer(1, 1);
                ScreenFadeControl.StartScreenFade(3, 1, 1, 0, 0x10, 4, data.HeapID);
                data.State = StateFadeInRowanEnd;
                break;
            case StateFadeInRowanEnd:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    data.State = StateDialogueRowanIntro;
                }
                break;
            case StateDialogueRowanIntro:
                if (RowanIntro_DisplayMessage(data, 1, 1))
                {
                    data.State = StateMoveRowanRightForInfo;
                }
                break;
            case StateMoveRowanRightForInfo:
                if (RowanIntro_MoveBgLayer(data, 1, MoveRight))
                {
                    data.State = StateInfoChoiceBox;
                }
                break;
            case StateInfoChoiceBox:
                if (RowanIntro_ChoiceBox(data, ChoicesInfo, 1))
                {
                    switch (data.PlayerChoice)
                    {
                        case 1:
                            data.BufferedState = StateControlInfoFadeInStart;
                            data.State = StateInfoFadeBlackStart;
                            break;
                        case 2:
                            data.BufferedState = StateAdventureInfoFadeInStart;
                            data.State = StateInfoFadeBlackStart;
                            Bg.Bg_ClearTilemap(data.BgConfig!, 0);
                            break;
                        case 3:
                            data.State = StateLeftRowanAfterInfo;
                            break;
                    }
                }
                break;
            case StateInfoFadeBlackStart:
                ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 6, 1, data.HeapID);
                data.State = StateInfoFadeBlackEnd;
                break;
            case StateInfoFadeBlackEnd:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    Bg.Bg_ClearTilemap(data.BgConfig!, 0);
                    Bg.Bg_ToggleLayer(1, 0);
                    data.State = data.BufferedState;
                }
                break;
            case StateControlInfoFadeInStart:
                data.BgLayer3TilemapIndex = 1;
                RowanIntro_LoadLayer3Tilemap(data);
                data.BgSubLayer3TilemapIndex = 1;
                RowanIntro_LoadSubLayer3Tilemap(data);
                ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 6, 1, data.HeapID);
                data.State = StateControlInfoFadeInEnd;
                break;
            case StateControlInfoFadeInEnd:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    data.State = StateControlInfoText0;
                }
                break;
            case StateControlInfoText0:
                if (RowanIntro_DisplayTextBlock(data, 2, TextBlockTopAligned, 3, 0x12))
                {
                    data.State = StateControlInfoShowXyIcons;
                }
                break;
            case StateControlInfoShowXyIcons:
                data.BgLayer3TilemapIndex = 2;
                RowanIntro_LoadLayer3Tilemap(data);
                data.State = StateControlInfoText1;
                break;
            case StateControlInfoText1:
                if (RowanIntro_DisplayTextBlock(data, 3, TextBlockTopAligned, 7, 0xC))
                {
                    data.State = StateControlInfoHideXyIcons;
                }
                break;
            case StateControlInfoHideXyIcons:
                data.BgLayer3TilemapIndex = 3;
                RowanIntro_LoadLayer3Tilemap(data);
                data.State = StateControlInfoText2;
                break;
            case StateControlInfoText2:
                if (RowanIntro_DisplayTextBlock(data, 4, TextBlockCenteredInArea, 4, 0xC))
                {
                    data.State = StateControlInfoDialogueDsIcon;
                }
                break;
            case StateControlInfoDialogueDsIcon:
                if (RowanIntro_DisplayMessage(data, 6, 1))
                {
                    data.State = StateControlInfoText3;
                }
                break;
            case StateControlInfoText3:
                if (RowanIntro_DisplayTextBlock(data, 5, TextBlockCenteredInArea, 4, 0xA))
                {
                    data.State = StateControlInfoHideFg;
                    Bg.Bg_ClearTilemap(data.BgConfig!, 0);
                }
                break;
            case StateControlInfoHideFg:
                Bg.Bg_ToggleLayer(0, 1);
                data.State = StateControlInfoDialogueUnderstood;
                break;
            case StateControlInfoDialogueUnderstood:
                if (RowanIntro_DisplayMessage(data, 7, 1))
                {
                    data.State = StateControlInfoShowYesno;
                }
                break;
            case StateControlInfoShowYesno:
            {
                byte[] raw = RomData.Overlay(Overlay, YesNoParamsAddress, 0x14);
                YesNoTouchMenuParams menuParams = new()
                {
                    BgConfig = data.BgConfig,
                    BgLayer = (int)Bin.U32(raw, 4),
                    BaseTile = Bin.U32(raw, 8),
                    Palette = Bin.U32(raw, 0xC),
                    TilemapLeft = raw[0x10],
                    TilemapTop = raw[0x11],
                };
                YesNoTouchMenu.YesNoTouchMenu_InitWithParams(data.YesNoTouchMenu!, menuParams);
                Bg.Bg_ToggleLayer(6, 1);
                data.BgSubLayer3TilemapIndex = 3;
                RowanIntro_LoadSubLayer3Tilemap(data);
                data.State = StateControlInfoWaitInput;
                break;
            }
            case StateControlInfoWaitInput:
            {
                if (gSystem.PressedKeys != 0)
                {
                    data.State = StateControlInfoDialogueUseTouchscreen;
                    break;
                }
                uint input = YesNoTouchMenu.YesNoTouchMenu_ProcessInputInstant(data.YesNoTouchMenu!);
                if (input - 3 <= 1)
                {
                    data.State = StateControlInfoProcessYesno;
                }
                break;
            }
            case StateControlInfoProcessYesno:
                switch (YesNoTouchMenu.YesNoTouchMenu_ProcessInputInstant(data.YesNoTouchMenu!))
                {
                    case 1:
                        data.State = StateControlInfoFadeOutStart;
                        break;
                    case 2:
                        data.State = StateControlInfoRepeat;
                        break;
                }
                break;
            case StateControlInfoFadeOutStart:
                if (RowanIntro_FadeBgLayer(data, 6, FadeOut))
                {
                    YesNoTouchMenu.YesNoTouchMenu_Reset(data.YesNoTouchMenu!);
                    ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 6, 1, data.HeapID);
                    data.State = StateControlInfoFadeOutEnd;
                }
                break;
            case StateControlInfoFadeOutEnd:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    Bg.Bg_ClearTilemap(data.BgConfig!, 0);
                    data.State = StateFadeInRowan2Start;
                }
                break;
            case StateControlInfoRepeat:
                if (RowanIntro_FadeBgLayer(data, 6, FadeOut))
                {
                    Bg.Bg_ClearTilemap(data.BgConfig!, 0);
                    YesNoTouchMenu.YesNoTouchMenu_Reset(data.YesNoTouchMenu!);
                    data.BgLayer3TilemapIndex = 1;
                    RowanIntro_LoadLayer3Tilemap(data);
                    data.BgSubLayer3TilemapIndex = 1;
                    RowanIntro_LoadSubLayer3Tilemap(data);
                    data.State = StateControlInfoText0;
                }
                break;
            case StateControlInfoDialogueUseTouchscreen:
                if (RowanIntro_DisplayMessage(data, 8, 1))
                {
                    data.State = StateControlInfoWaitInput;
                }
                break;
            case StateFadeInRowan2Start:
                data.BgLayer3TilemapIndex = 0;
                RowanIntro_LoadLayer3Tilemap(data);
                data.BgSubLayer3TilemapIndex = 0;
                RowanIntro_LoadSubLayer3Tilemap(data);
                Bg.Bg_ToggleLayer(1, 1);
                Bg.Bg_SetOffset(data.BgConfig!, 1, Bg.OffsetSetX, 0);
                ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 6, 1, data.HeapID);
                data.State = StateFadeInRowan2End;
                break;
            case StateFadeInRowan2End:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    data.State = StateDialogueAnotherInfo;
                }
                break;
            case StateDialogueAnotherInfo:
                if (RowanIntro_DisplayMessage(data, 9, 1))
                {
                    data.State = StateMoveRowanRightForInfo;
                }
                break;
            case StateAdventureInfoFadeInStart:
                data.BgLayer3TilemapIndex = 4;
                RowanIntro_LoadLayer3Tilemap(data);
                data.BgSubLayer3TilemapIndex = 2;
                RowanIntro_LoadSubLayer3Tilemap(data);
                ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 6, 1, data.HeapID);
                data.State = StateAdventureInfoFadeInEnd;
                break;
            case StateAdventureInfoFadeInEnd:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    data.State = StateAdventureInfoText0;
                }
                break;
            case StateAdventureInfoText0:
                if (RowanIntro_DisplayTextBlock(data, 0xA, TextBlockCentered, 9, 6))
                {
                    data.State = StateAdventureInfoText1;
                }
                break;
            case StateAdventureInfoText1:
                if (RowanIntro_DisplayTextBlock(data, 0xB, TextBlockCentered, 8, 8))
                {
                    data.State = StateAdventureInfoText2;
                }
                break;
            case StateAdventureInfoText2:
                if (RowanIntro_DisplayTextBlock(data, 0xC, TextBlockCentered, 9, 6))
                {
                    data.State = StateAdventureInfoText3;
                }
                break;
            case StateAdventureInfoText3:
                if (RowanIntro_DisplayTextBlock(data, 0xD, TextBlockCentered, 5, 0xE))
                {
                    data.State = StateAdventureInfoText4;
                }
                break;
            case StateAdventureInfoText4:
                if (RowanIntro_DisplayTextBlock(data, 0xE, TextBlockCentered, 0xA, 4))
                {
                    data.State = StateAdventureInfoText5;
                }
                break;
            case StateAdventureInfoText5:
                if (RowanIntro_DisplayTextBlock(data, 0xF, TextBlockCentered, 6, 0xC))
                {
                    data.State = StateAdventureInfoFadeOutStart;
                }
                break;
            case StateAdventureInfoFadeOutStart:
                ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 6, 1, data.HeapID);
                data.State = StateAdventureInfoFadeOutEnd;
                break;
            case StateAdventureInfoFadeOutEnd:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    Bg.Bg_ClearTilemap(data.BgConfig!, 0);
                    Bg.Bg_ToggleLayer(0, 1);
                    data.State = StateFadeInRowan2Start;
                }
                break;
            case StateLeftRowanAfterInfo:
                if (RowanIntro_MoveBgLayer(data, 1, MoveLeft))
                {
                    data.State = StateDialogueWidelyInhabited;
                }
                break;
            case StateDialogueWidelyInhabited:
                if (RowanIntro_DisplayMessage(data, 0x10, 1))
                {
                    data.State = StatePkblFadeStart;
                }
                break;
            case StatePkblFadeStart:
                ScreenFadeControl.StartScreenFade(4, 0, 0, 0, 6, 1, data.HeapID);
                data.State = StatePkblFadeMiddle;
                break;
            case StatePkblFadeMiddle:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    RowanIntro_LoadPokeballTilemap(data);
                    data.BgSubLayer3TilemapIndex = 4;
                    RowanIntro_LoadSubLayer3Tilemap(data);
                    Bg.Bg_ToggleLayer(6, 1);
                    ScreenFadeControl.StartScreenFade(4, 1, 1, 0, 6, 1, data.HeapID);
                    data.State = StatePkblFadeEnd;
                }
                break;
            case StatePkblFadeEnd:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    data.State = StatePkblDialogue;
                }
                break;
            case StatePkblDialogue:
                if (RowanIntro_DisplayMessage(data, 0x11, 1))
                {
                    data.State = StatePkblWaitInput;
                }
                break;
            case StatePkblWaitInput:
                if (RowanIntro_WasPokeballOpened())
                {
                    data.AnimProgress = 0;
                    data.AnimDelayUpdateCounter = 0;
                    Bg.Bg_ClearTilemap(data.BgConfig!, 0);
                    data.State = StatePkblAnimPushIn;
                }
                else if (gSystem.PressedKeys != 0)
                {
                    data.State = StatePkblDialogueUseTouchscreen;
                }
                break;
            case StatePkblAnimPushIn:
            {
                if (data.AnimDelayUpdateCounter != 0)
                {
                    data.AnimDelayUpdateCounter--;
                    break;
                }
                uint[] frames = RomData.OverlayU32(Overlay, PokeballFramesAddress, 3);
                uint frame = frames[data.AnimProgress];
                if (frame == 0xFFFF)
                {
                    SoundPlayback.Sound_PlayEffect(SePokeballOpen);
                    data.State = StatePkblAnimFlash0;
                }
                else
                {
                    Graphics.Graphics_LoadTilesToBgLayer(IntroArchive, frame, data.BgConfig!, 6, 0x20, 0, 0, data.HeapID);
                    data.AnimProgress++;
                    data.AnimDelayUpdateCounter = 4;
                }
                break;
            }
            case StatePkblDialogueUseTouchscreen:
                if (RowanIntro_DisplayMessage(data, 0x12, 1))
                {
                    data.State = StatePkblWaitInput;
                }
                break;
            case StatePkblAnimFlash0:
                BrightnessController.BrightnessController_StartTransition(1, 0x10, 0, 0xB, 1);
                BrightnessController.BrightnessController_StartTransition(1, 0x10, 0, 0xD, 2);
                data.State = StatePkblAnimFlash1;
                break;
            case StatePkblAnimFlash1:
                if (BrightnessController.BrightnessController_IsTransitionComplete(1) && BrightnessController.BrightnessController_IsTransitionComplete(2))
                {
                    BrightnessController.BrightnessController_StartTransition(1, 0, 0x10, 0xB, 1);
                    BrightnessController.BrightnessController_StartTransition(1, 0, 0x10, 0xD, 2);
                    data.State = StatePkblAnimFlash2;
                }
                break;
            case StatePkblAnimFlash2:
                if (BrightnessController.BrightnessController_IsTransitionComplete(1) && BrightnessController.BrightnessController_IsTransitionComplete(2))
                {
                    BrightnessController.BrightnessController_StartTransition(4, 0x10, 0, 0xB, 1);
                    BrightnessController.BrightnessController_StartTransition(4, 0x10, 0, 0xD, 2);
                    data.State = StatePkblAnimFlash3;
                }
                break;
            case StatePkblAnimFlash3:
                if (BrightnessController.BrightnessController_IsTransitionComplete(1) && BrightnessController.BrightnessController_IsTransitionComplete(2))
                {
                    data.State = StatePkblAnimSpawnPkmAndFlash4;
                }
                break;
            case StatePkblAnimSpawnPkmAndFlash4:
                RowanIntro_LoadBunearySprite(data);
                Bg.Bg_ToggleLayer(6, 0);
                data.AnimProgress = 0;
                RowanIntro_AnimateBuneary(data, ref data.AnimProgress);
                data.BgSubLayer3TilemapIndex = 0;
                RowanIntro_LoadSubLayer3Tilemap(data);
                BrightnessController.BrightnessController_StartTransition(0x10, 0, 0x10, 0xB, 1);
                BrightnessController.BrightnessController_StartTransition(0x10, 0, 0x10, 0xD, 2);
                data.State = StatePkblAnimMvPkmUpAndFlashEnd;
                break;
            case StatePkblAnimMvPkmUpAndFlashEnd:
                RowanIntro_AnimateBuneary(data, ref data.AnimProgress);
                if (BrightnessController.BrightnessController_IsTransitionComplete(1) && BrightnessController.BrightnessController_IsTransitionComplete(2))
                {
                    data.State = StatePkblAnimMvPkmDownAndBounce;
                }
                break;
            case StatePkblAnimMvPkmDownAndBounce:
                if (RowanIntro_AnimateBuneary(data, ref data.AnimProgress))
                {
                    data.State = StatePkblAnimEndDelay;
                }
                break;
            case StatePkblAnimEndDelay:
                if (RowanIntro_Delay(data, 0x28))
                {
                    data.State = StatePkblDialogueLiveAlongside;
                }
                break;
            case StatePkblDialogueLiveAlongside:
                if (RowanIntro_DisplayMessage(data, 0x13, 1))
                {
                    Bg.Bg_ClearTilemap(data.BgConfig!, 0);
                    data.State = StatePkblPutAwayPkm;
                }
                break;
            case StatePkblPutAwayPkm:
                if (RowanIntro_FadeBgLayer(data, 2, FadeOut))
                {
                    RowanIntro_SetBunearyLayerPriority(data);
                    data.State = StateBetweenDialogueDelay;
                }
                break;
            case StateBetweenDialogueDelay:
                if (RowanIntro_Delay(data, 0x1E))
                {
                    data.State = StateDialogueAboutYourself;
                }
                break;
            case StateDialogueAboutYourself:
                if (RowanIntro_DisplayMessage(data, 0x14, 1))
                {
                    data.State = StateGendrFadeOutRowan;
                }
                break;
            case StateGendrFadeOutRowan:
                if (RowanIntro_FadeBgLayer(data, 1, FadeOut))
                {
                    data.State = StateGendrFadeInAvatarPrep;
                }
                break;
            case StateGendrFadeInAvatarPrep:
                RowanIntro_ResetAvatarRunAnimation(data);
                data.BgLayer1TilemapIndex = 2;
                data.BgLayer2TilemapIndex = 6;
                RowanIntro_LoadTilemap(data);
                Bg.Bg_SetOffset(data.BgConfig!, 1, Bg.OffsetSetX, -0x30);
                Bg.Bg_SetOffset(data.BgConfig!, 2, Bg.OffsetSetX, 0x30);
                data.State = StateGendrFadeInAvatarMale;
                break;
            case StateGendrFadeInAvatarMale:
                if (RowanIntro_FadeBgLayer(data, 1, FadeIn))
                {
                    data.State = StateGendrFadeInAvatarFemale;
                }
                break;
            case StateGendrFadeInAvatarFemale:
                if (RowanIntro_FadeBgLayer(data, 2, FadeIn))
                {
                    data.State = StateGendrDialogue;
                }
                break;
            case StateGendrDialogue:
                if (RowanIntro_DisplayMessage(data, 0x15, 1))
                {
                    data.PlayerGender = GenderMale;
                    data.State = StateGendrChoice;
                }
                break;
            case StateGendrChoice:
            {
                int pressed = gSystem.PressedKeys;
                if ((pressed & KeyA) == KeyA)
                {
                    data.FadeBgLayerCurAlpha = 6;
                    data.FadeBgLayerCurAlphaInv = 0xA;
                    data.FadeBgLayerState = 2;
                    data.State = data.PlayerGender == GenderMale ? StateGendrFadeOutAvatarFemale : StateGendrFadeOutAvatarMale;
                    break;
                }
                if ((pressed & KeyLeft) == KeyLeft || (pressed & KeyRight) == KeyRight)
                {
                    data.PlayerGender = data.PlayerGender == GenderMale ? GenderFemale : GenderMale;
                    SoundPlayback.Sound_PlayEffect(SeConfirm);
                }
                RowanIntro_AnimateAvatarRun(data);
                break;
            }
            case StateGendrFadeOutAvatarFemale:
                if (RowanIntro_FadeBgLayer(data, 2, FadeOut))
                {
                    data.State = StateGendrCentreAvatarMale;
                }
                break;
            case StateGendrCentreAvatarMale:
                if (RowanIntro_MoveBgLayer(data, 1, MoveToCenter))
                {
                    RowanIntro_ResetBlending(data);
                    data.State = StateGendrConfirmDialogue;
                }
                break;
            case StateGendrFadeOutAvatarMale:
                if (RowanIntro_FadeBgLayer(data, 1, FadeOut))
                {
                    data.State = StateGendrCentreAvatarFemale;
                }
                break;
            case StateGendrCentreAvatarFemale:
                if (RowanIntro_MoveBgLayer(data, 2, MoveToCenter))
                {
                    RowanIntro_ResetBlending(data);
                    data.State = StateGendrConfirmDialogue;
                }
                break;
            case StateGendrConfirmDialogue:
                if (RowanIntro_DisplayMessage(data, data.PlayerGender == GenderMale ? 0x16 : 0x17, 1))
                {
                    data.State = StateGendrConfirmChoiceBox;
                }
                break;
            case StateGendrConfirmChoiceBox:
                if (RowanIntro_ChoiceBox(data, ChoicesYesNo, 0))
                {
                    switch (data.PlayerChoice)
                    {
                        case 1:
                            data.State = StateNameDialogue;
                            break;
                        case ListCancel:
                        case 2:
                            Bg.Bg_ClearTilemap(data.BgConfig!, 0);
                            data.State = StateGendrRepeat;
                            break;
                    }
                }
                break;
            case StateGendrRepeat:
                if (RowanIntro_FadeBgLayer(data, data.PlayerGender == GenderMale ? 1 : 2, FadeOut))
                {
                    data.State = StateGendrFadeInAvatarPrep;
                }
                break;
            case StateNameDialogue:
                if (RowanIntro_DisplayMessage(data, 0x18, 1))
                {
                    data.State = StateNameAppKeyboard;
                }
                break;
            case StateNameAppKeyboard:
                data.PlayerNamingScreenArgs!.PlayerGenderOrMonSpecies = data.PlayerGender;
                data.AppMan = ApplicationManager.ApplicationManager_New(ApplicationTemplate.At(NamingScreenTemplate), data.PlayerNamingScreenArgs, data.HeapID);
                data.State = StateNameKeyboard;
                break;
            case StateNameKeyboard:
                Bg.Bg_ToggleLayer(0, 1);
                Bg.Bg_ToggleLayer(3, 1);
                Bg.Bg_ToggleLayer(7, 1);
                if (data.PlayerGender == GenderMale)
                {
                    Bg.Bg_ToggleLayer(1, 1);
                    Bg.Bg_SetOffset(data.BgConfig!, 1, Bg.OffsetSetX, 0);
                }
                else
                {
                    Bg.Bg_ToggleLayer(2, 1);
                    Bg.Bg_SetOffset(data.BgConfig!, 2, Bg.OffsetSetX, 0);
                }
                ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 6, 1, data.HeapID);
                data.State = StateNameKeyboardFadeIntoAvatar;
                break;
            case StateNameKeyboardFadeIntoAvatar:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    data.State = StateNameConfirmDialogue;
                }
                break;
            case StateNameConfirmDialogue:
                if (RowanIntro_DisplayMessage(data, data.PlayerGender == GenderMale ? 0x19 : 0x1A, 1))
                {
                    data.State = StateNameConfirmChoiceBox;
                }
                break;
            case StateNameConfirmChoiceBox:
                if (RowanIntro_ChoiceBox(data, ChoicesYesNo, 0))
                {
                    Bg.Bg_ClearTilemap(data.BgConfig!, 0);
                    switch (data.PlayerChoice)
                    {
                        case 1:
                            data.BufferedState = StateLoadRowanTilemap0;
                            data.State = StateNameFadeOutAvatar;
                            break;
                        case ListCancel:
                        case 2:
                            GameString.String_Clear(data.PlayerNamingScreenArgs!.TextInputStr);
                            data.BufferedState = StateGendrFadeInAvatarPrep;
                            data.State = StateNameFadeOutAvatar;
                            break;
                    }
                }
                break;
            case StateNameFadeOutAvatar:
                if (RowanIntro_FadeBgLayer(data, data.PlayerGender == GenderMale ? 1 : 2, FadeOut))
                {
                    data.State = data.BufferedState;
                }
                break;
            case StateLoadRowanTilemap0:
                data.BgLayer1TilemapIndex = 1;
                data.BgLayer2TilemapIndex = 0;
                RowanIntro_LoadTilemap(data);
                data.State = StateFadeInRowanAfterName;
                break;
            case StateFadeInRowanAfterName:
                if (RowanIntro_FadeBgLayer(data, 1, FadeIn))
                {
                    data.State = StateDialogueSoYoure;
                }
                break;
            case StateDialogueSoYoure:
                if (RowanIntro_DisplayMessage(data, 0x1B, 1))
                {
                    data.State = StateFadeOutRowanForRival;
                }
                break;
            case StateFadeOutRowanForRival:
                if (RowanIntro_FadeBgLayer(data, 1, FadeOut))
                {
                    data.State = StateLoadRivalTilemap;
                }
                break;
            case StateLoadRivalTilemap:
                data.BgLayer1TilemapIndex = 0xA;
                data.BgLayer2TilemapIndex = 0;
                RowanIntro_LoadTilemap(data);
                data.State = StateFadeInRival;
                break;
            case StateFadeInRival:
                if (RowanIntro_FadeBgLayer(data, 1, FadeIn))
                {
                    data.State = StateRivalNameDialogue;
                }
                break;
            case StateRivalNameDialogue:
                if (RowanIntro_DisplayMessage(data, 0x1C, 1))
                {
                    data.State = StateMoveRivalRightForNames;
                }
                break;
            case StateMoveRivalRightForNames:
                if (RowanIntro_MoveBgLayer(data, 1, MoveRight))
                {
                    data.State = StateRivalNameChoiceBox;
                }
                break;
            case StateRivalNameChoiceBox:
                if (RowanIntro_ChoiceBox(data, ChoicesRivalName, 1))
                {
                    switch (data.PlayerChoice)
                    {
                        case 1:
                            data.State = StateRivalNameAppKeyboard;
                            break;
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        {
                            uint[] choices = RomData.OverlayU32(Overlay, RivalNameChoicesAddress, 10);
                            GameString? name = MessageLoader.MessageLoader_GetNewString(data.MsgLoader!, choices[(data.PlayerChoice - 1) * 2]);
                            GameString.String_Copy(data.RivalNamingScreenArgs!.TextInputStr, name);
                            GameString.String_Free(name);
                            data.State = StateMoveRivalLeftAfterName;
                            break;
                        }
                    }
                }
                break;
            case StateMoveRivalLeftAfterName:
                if (RowanIntro_MoveBgLayer(data, 1, MoveLeft))
                {
                    data.State = StateRivalNameConfirmDialogue;
                }
                break;
            case StateRivalNameAppKeyboard:
                data.AppMan = ApplicationManager.ApplicationManager_New(ApplicationTemplate.At(NamingScreenTemplate), data.RivalNamingScreenArgs, data.HeapID);
                data.State = StateRivalNameKeyboard;
                break;
            case StateRivalNameKeyboard:
                Bg.Bg_ToggleLayer(0, 1);
                Bg.Bg_ToggleLayer(3, 1);
                Bg.Bg_ToggleLayer(7, 1);
                Bg.Bg_ToggleLayer(1, 1);
                Bg.Bg_SetOffset(data.BgConfig!, 1, Bg.OffsetSetX, 0);
                ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 6, 1, data.HeapID);
                data.State = StateRivalNameKeyboardFadeIntoAvatar;
                break;
            case StateRivalNameKeyboardFadeIntoAvatar:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    data.State = StateRivalNameConfirmDialogue;
                }
                break;
            case StateRivalNameConfirmDialogue:
                if (RowanIntro_DisplayMessage(data, 0x1D, 1))
                {
                    data.State = StateRivalNameConfirmChoiceBox;
                }
                break;
            case StateRivalNameConfirmChoiceBox:
                if (RowanIntro_ChoiceBox(data, ChoicesYesNo, 0))
                {
                    switch (data.PlayerChoice)
                    {
                        case 1:
                            Bg.Bg_ClearTilemap(data.BgConfig!, 0);
                            data.State = StateFadeOutRival;
                            break;
                        case ListCancel:
                        case 2:
                            GameString.String_Clear(data.RivalNamingScreenArgs!.TextInputStr);
                            data.State = StateRivalNameDialogue;
                            break;
                    }
                }
                break;
            case StateFadeOutRival:
                if (RowanIntro_FadeBgLayer(data, 1, FadeOut))
                {
                    data.State = StateLoadRowanTilemap1;
                }
                break;
            case StateLoadRowanTilemap1:
                data.BgLayer1TilemapIndex = 1;
                data.BgLayer2TilemapIndex = 0;
                RowanIntro_LoadTilemap(data);
                data.State = StateFadeInRowanAfterRival;
                break;
            case StateFadeInRowanAfterRival:
                if (RowanIntro_FadeBgLayer(data, 1, FadeIn))
                {
                    data.State = StateDelayBeforeEnd0;
                }
                break;
            case StateDelayBeforeEnd0:
                if (RowanIntro_Delay(data, 0x1E))
                {
                    data.State = StateDialogueEnd;
                }
                break;
            case StateDialogueEnd:
                if (RowanIntro_DisplayMessage(data, 0x1E, 1))
                {
                    SoundPlayback.Sound_FadeOutBGM(0, 0x32);
                    data.State = StateFadeOutRowanEnd;
                }
                break;
            case StateFadeOutRowanEnd:
                if (RowanIntro_FadeBgLayer(data, 1, FadeOut))
                {
                    Bg.Bg_ClearTilemap(data.BgConfig!, 0);
                    data.State = StateDelayBeforeEnd1;
                }
                break;
            case StateDelayBeforeEnd1:
                if (RowanIntro_Delay(data, 0x1E))
                {
                    data.State = StateLoadMiniAvatar;
                }
                break;
            case StateLoadMiniAvatar:
                data.BgLayer1TilemapIndex = (byte)(data.PlayerGender == GenderMale ? 2 : 6);
                data.BgLayer2TilemapIndex = 0;
                RowanIntro_LoadTilemap(data);
                data.State = StateFadeInAvatarEnd;
                break;
            case StateFadeInAvatarEnd:
                if (RowanIntro_FadeBgLayer(data, 1, FadeIn))
                {
                    RowanIntro_ResetAnimationCounters(data);
                    data.State = StateDelayBeforeEnd2;
                }
                break;
            case StateDelayBeforeEnd2:
                if (RowanIntro_Delay(data, 0x1E))
                {
                    data.State = StateAvatarShrinkAnimation;
                }
                break;
            case StateAvatarShrinkAnimation:
                if (RowanIntro_AnimateAvatarShrink(data))
                {
                    data.State = StateEnd;
                }
                break;
            case StateEnd:
                data.AppMan = ApplicationManager.ApplicationManager_New(ApplicationTemplate.At(TvTemplate), null, data.HeapID);
                data.State = StateExit;
                break;
            case StateExit:
                finished = true;
                break;
        }
        return finished;
    }
}
