namespace PlatinumSharp.Game;

public static partial class TrainerCaseApplication
{
    public const int Overlay = 71;
    private const int HeapID = 0x19;
    private const uint HeapSize = 0x28000;
    private const int TrainerCaseArchive = 0x33;
    private const int MessageArchive = 0x1A;
    private const int TrainerCardTextBank = 0x268;
    private const uint AllLayersMask = 0xFFFFE0FF;
    private const int KeyA = 0x1;
    private const int KeyB = 0x2;
    private const int SeqCardOpen = 0x695;
    private const int SeqCardFlip = 0x696;
    private const int SeqCaseOpenClose = 0x699;
    private const int SeqClose = 0x5DC;
    private const int SeqBadgeChime = 0x694;
    private const uint VramBanksAddress = 0x0223D57C;
    private const uint GraphicsModesAddress = 0x0223D4E0;
    private const uint BgTemplateLayer7 = 0x0223D4F0;
    private const uint BgTemplateLayer5 = 0x0223D50C;
    private const uint BgTemplateLayer6 = 0x0223D544;
    private const uint BgTemplateLayer2 = 0x0223D560;
    private const uint BgTemplateLayer3 = 0x0223D528;
    private const uint PolishingSounds = 0x0223D4B8;
    private const uint ButtonPressAnim = 0x0223D4A8;
    private const uint ButtonReleaseAnim = 0x0223D4AC;
    private const uint PolishThresholds = 0x0223D4B0;
    private const uint TouchRectTables = 0x0223D4D0;
    private const uint CoverScaleSteps = 0x0223D5CC;
    private const uint AppearanceSprites = 0x0223D604;
    private const uint ChimePitches = 0x0223D77C;
    private const int CoverTransitionSteps = 14;
    private const int Fx32One = 0x1000;

    private const int StateWaitFadeIn = 0;
    private const int StateHandleInput = 1;
    private const int StateWaitFadeOut = 2;
    private const int StateFlipCard = 3;
    private const int StateOpenCloseCase = 4;

    private const int InputNone = 0;
    private const int InputTouched = 1;
    private const int InputHeld = 2;
    private const int InputFlip = 3;
    private const int InputExit = 4;

    private const int SpriteSparklesTwo = 8;
    private const int SpriteSparklesFour = 16;
    private const int SpriteButtonEffect = 24;

    private const int WindowTime = 5;

    private static uint BadgeData(TrainerCase trainerCase, int badge)
    {
        return trainerCase.U32(TrainerCase.BadgesOffset + badge * 4);
    }

    private static bool BadgeObtained(TrainerCase trainerCase, int badge)
    {
        return (BadgeData(trainerCase, badge) & 1) != 0;
    }

    private static int BadgePolish(TrainerCase trainerCase, int badge)
    {
        return (int)BadgeData(trainerCase, badge) >> 1;
    }

    private static int CaseFlag(TrainerCase trainerCase, int bit)
    {
        return (trainerCase.U8(TrainerCase.FlagsOffset) >> bit) & 1;
    }

    private static int FxInv(int value)
    {
        return (int)(((long)Fx32One << 12) / value);
    }

    private static void SetScaleAffine(int layer, int xScale, int yScale, int centerX, int centerY)
    {
        MtxFx22 matrix = new() { M00 = FxInv(xScale), M01 = 0, M10 = 0, M11 = FxInv(yScale) };
        Video.SetBgAffine(layer, matrix, centerX, centerY, 0, 0);
    }

    private static BgTemplate OverlayBgTemplate(uint address)
    {
        return RomStructs.BgTemplate(RomData.Overlay(Overlay, address, 0x1C), 0);
    }

    // OFFSET 0x0223B140
    public static int TrainerCaseApp_Init(ApplicationManager appMan, ref int state)
    {
        GameSystem.SetVBlankCallback(null, null);
        GameSystem.SetHBlankCallback(null, null);
        GXLayers.GXLayers_DisableEngineALayers();
        GXLayers.GXLayers_DisableEngineBLayers();
        Video.SetDisplayControl(0, Video.GetDisplayControl(0) & AllLayersMask);
        Video.SetDisplayControl(1, Video.GetDisplayControl(1) & AllLayersMask);
        GameSystem.SetAutorepeat(4, 8);
        Heap.Heap_Create(3, HeapID, HeapSize);
        NARC narc = NARC.NARC_ctor(TrainerCaseArchive, HeapID);
        ApplicationManager.ApplicationManager_NewData(appMan, TrainerCaseApp.Size, HeapID);
        TrainerCaseApp app = new();
        appMan.Data = app;
        app.TrainerCase = (TrainerCase)ApplicationManager.ApplicationManager_Args(appMan)!;
        app.BgConfig = Bg.BgConfig_New(HeapID);
        TrainerCaseApp_InitStrings(app);
        TrainerCase_SetVRAMBanks();
        TrainerCaseApp_InitBackgrounds(app.BgConfig);
        TrainerCase_DrawTrainerCard(app, narc);
        TouchPad.EnableTouchPad();
        TouchPad.InitializeTouchPad(4);
        Sound.Sound_SetSceneAndPlayBGM(0x38, 0, 0);
        TrainerCaseApp_InitBadgeChimeState(app.BadgeChimeState);
        SoundPlayback.Sound_PlayEffect(SeqCardOpen);
        TrainerCaseApp_InitSpriteData(app.SpriteData, narc);
        byte[] obtained = new byte[TrainerCase.MaxBadges];
        for (int i = 0; i < TrainerCase.MaxBadges; i++)
        {
            obtained[i] = (byte)(BadgeObtained(app.TrainerCase, i) ? 1 : 0);
        }
        TrainerCase_DrawBadgeCaseSprites(app.SpriteData, obtained);
        TrainerCard_AddWindows(app.BgConfig, app.Windows);
        for (int i = 0; i < TrainerCase.MaxBadges; i++)
        {
            if (((app.TrainerCase.U8(TrainerCase.GymLeadersToHideOffset) >> i) & 1) != 0)
            {
                TrainerCase_HideGymLeader(app, i);
            }
        }
        Bg.Bg_CopyTilemapBufferToVRAM(app.BgConfig, 3);
        TrainerCard_DrawFrontText(app.Windows, app.TrainerCase);
        if (CaseFlag(app.TrainerCase, TrainerCase.FlagLiveTimeDisplay) != 0)
        {
            TrainerCard_BlinkPlaytimeColon(app.Windows[WindowTime], true, app.ColonString!);
        }
        app.ViewingBack = false;
        app.BadgeCaseOpenState = 0;
        app.TouchedRectangleIndex = -1;
        app.BadgeCaseButtonPushed = false;
        app.BadgeCaseButtonState = 0;
        for (int i = 0; i < TrainerCase.MaxBadges; i++)
        {
            app.PolishingProgress[i] = 0;
            if (!BadgeObtained(app.TrainerCase, i))
            {
                continue;
            }
            int level = TrainerCase_GetBadgePolishLevel(BadgePolish(app.TrainerCase, i));
            if (level <= 4)
            {
                TrainerCase_DrawBadgeDirt(app.SpriteData, i, (byte)(level == 4 ? 0 : 3 - level));
            }
            if (level == 3)
            {
                Sprite.Sprite_SetDrawFlag(app.SpriteData.Sprites[SpriteSparklesTwo + i]!, 1);
            }
            else if (level == 4)
            {
                Sprite.Sprite_SetDrawFlag(app.SpriteData.Sprites[SpriteSparklesFour + i]!, 1);
            }
        }
        TrainerCase_ResetAffineTransforms();
        GameSystem.SetVBlankCallback(TrainerCaseApp_VBlankCallback, null);
        NetworkIcon.NetworkIcon_InitIfConnected();
        Sound.Sound_SetPlayerVolume(1, 0x2A);
        ScreenFadeControl.StartScreenFade(2, 3, 3, 0, 6, 1, HeapID);
        NARC.NARC_dtor(narc);
        return 1;
    }

    private static void PressBadgeCaseButton(TrainerCaseApp app)
    {
        app.BadgeCaseButtonPushed = true;
        app.BadgeCaseButtonState = 1;
    }

    private static void ReleaseBadgeCaseButton(TrainerCaseApp app)
    {
        app.BadgeCaseButtonPushed = false;
        app.BadgeCaseButtonState = 2;
    }

    // OFFSET 0x0223B388
    public static int TrainerCaseApp_Main(ApplicationManager appMan, ref int state)
    {
        TrainerCaseApp app = (TrainerCaseApp)ApplicationManager.ApplicationManager_Data(appMan)!;
        TrainerCase trainerCase = app.TrainerCase!;
        switch (state)
        {
            case StateWaitFadeIn:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    state = StateHandleInput;
                }
                break;
            case StateHandleInput:
            {
                int input = TrainerCase_GetPlayerInput(app);
                if (input == InputTouched)
                {
                    if (app.TouchedRectangleIndex == 0)
                    {
                        PressBadgeCaseButton(app);
                        Sprite sprite = app.SpriteData.Sprites[SpriteButtonEffect]!;
                        Sprite.Sprite_SetDrawFlag(sprite, 1);
                        Sprite.Sprite_SetAnimateFlag(sprite, 1);
                        Sprite.Sprite_SetAnim(sprite, 0xA);
                        app.SubState = 0;
                        app.BadgeCaseButtonAnimIndex = 0;
                        app.BadgeCaseButtonAnimTimer = 0;
                        app.BadgeCaseCoverMoving = false;
                        state = StateOpenCloseCase;
                    }
                    else
                    {
                        byte badge = (byte)(app.TouchedRectangleIndex - 1);
                        if (BadgeObtained(trainerCase, badge))
                        {
                            byte level = (byte)TrainerCase_GetBadgePolishLevel(BadgePolish(trainerCase, badge));
                            TrainerCase_PlayBadgeChime(app.BadgeChimeState, badge, level);
                        }
                    }
                }
                else if (input == InputHeld)
                {
                    if (app.BadgeCaseButtonPushed && app.TouchedRectangleIndex != 0)
                    {
                        ReleaseBadgeCaseButton(app);
                    }
                    TrainerCase_HandleBadgePolishing(app);
                }
                else
                {
                    if (app.BadgeCaseButtonPushed && app.BadgeCaseButtonState == 0)
                    {
                        ReleaseBadgeCaseButton(app);
                    }
                    if (CaseFlag(trainerCase, TrainerCase.FlagBadgesInteractable) != 0)
                    {
                        TrainerCase_ResetBadgePolishingState(app.BadgePolishingState);
                    }
                    if (input == InputFlip)
                    {
                        app.SubState = 0;
                        state = StateFlipCard;
                    }
                    else if (input == InputExit)
                    {
                        SoundPlayback.Sound_PlayEffect(SeqClose);
                        ScreenFadeControl.StartScreenFade(1, 4, 4, 0, 6, 1, HeapID);
                        state = StateWaitFadeOut;
                    }
                }
                TrainerCase_UpdatePlayTime(app, CaseFlag(trainerCase, TrainerCase.FlagLiveTimeDisplay) != 0);
                break;
            }
            case StateWaitFadeOut:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    return 1;
                }
                break;
            case StateFlipCard:
                if (TrainerCase_FlipTrainerCard(app))
                {
                    state = StateHandleInput;
                }
                break;
            case StateOpenCloseCase:
                if (app.BadgeCaseButtonState == 0)
                {
                    int input = TrainerCase_GetPlayerInput(app);
                    if (input == InputTouched)
                    {
                        if (app.TouchedRectangleIndex == 0)
                        {
                            PressBadgeCaseButton(app);
                            app.BadgeCaseButtonAnimIndex = 0;
                            app.BadgeCaseButtonAnimTimer = 0;
                        }
                    }
                    else if (input == InputHeld)
                    {
                        if (app.BadgeCaseButtonPushed && app.TouchedRectangleIndex != 0)
                        {
                            ReleaseBadgeCaseButton(app);
                        }
                    }
                    else if (app.BadgeCaseButtonPushed)
                    {
                        ReleaseBadgeCaseButton(app);
                    }
                }
                if (!app.BadgeCaseCoverMoving)
                {
                    app.BadgeCaseCoverMoving = TrainerCase_OpenCloseBadgeCase(app);
                }
                if (app.BadgeCaseCoverMoving)
                {
                    app.BadgeCaseCoverMoving = false;
                    state = StateHandleInput;
                }
                break;
        }
        TrainerCase_HandleBadgeCaseButtonAnimation(app);
        Sprite.SpriteList_Update(app.SpriteData.SpriteList);
        return 0;
    }

    // OFFSET 0x0223B5B8
    public static int TrainerCaseApp_Exit(ApplicationManager appMan, ref int state)
    {
        TrainerCaseApp app = (TrainerCaseApp)ApplicationManager.ApplicationManager_Data(appMan)!;
        TrainerCase_ResetAffineTransforms();
        TrainerCase_FreeStrings(app);
        TrainerCase_FreeSprites(app.SpriteData);
        Heap.Heap_Free(app.TrainerSprite!);
        Heap.Heap_Free(app.TrainerScreenDataNSCRBuffer!);
        TrainerCard_RemoveWindows(app.Windows);
        TrainerCase_TeardownBgs(app.BgConfig!);
        TouchPad.DisableTouchPad();
        ApplicationManager.ApplicationManager_FreeData(appMan);
        GameSystem.SetVBlankCallback(null, null);
        Heap.Heap_Destroy(HeapID);
        Sound.Sound_SetPlayerVolume(1, 0x7F);
        return 1;
    }

    // OFFSET 0x0223B620
    private static void TrainerCaseApp_InitStrings(TrainerCaseApp app)
    {
        app.UnusedString = GameString.String_Init(4, HeapID);
        app.ColonString = GameString.String_Init(5, HeapID);
        MessageLoader loader = MessageLoader.MessageLoader_Init(0, MessageArchive, TrainerCardTextBank, HeapID)!;
        MessageLoader.MessageLoader_GetString(loader, 0xB, app.ColonString);
        MessageLoader.MessageLoader_Free(loader);
    }

    // OFFSET 0x0223B668
    private static void TrainerCase_FreeStrings(TrainerCaseApp app)
    {
        GameString.String_Free(app.UnusedString);
        GameString.String_Free(app.ColonString);
    }

    // OFFSET 0x0223B688
    private static void TrainerCase_SetVRAMBanks()
    {
        GXLayers.GXLayers_SetBanks(RomStructs.Banks(RomData.Overlay(Overlay, VramBanksAddress, 0x28), 0));
    }

    private static PaletteFile LoadPalette(NARC narc, uint member, out byte[] buffer)
    {
        buffer = Graphics.Graphics_GetPlttDataFromOpenNARC(narc, member, out PaletteFile? palette, HeapID)!;
        return palette!;
    }

    // OFFSET 0x0223B6A8
    private static void TrainerCase_LoadCardPalette(byte cardLevel, int pokedexObtained, NARC narc)
    {
        uint member = 6;
        if (pokedexObtained != 0)
        {
            switch (cardLevel)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    member = cardLevel;
                    break;
                default:
                    member = uint.MaxValue;
                    break;
            }
        }
        if (member == uint.MaxValue)
        {
            return;
        }
        PaletteFile palette = LoadPalette(narc, member, out byte[] buffer);
        Video.LoadPaletteRange(Video.SubBackground, palette.Colors, 0x10, 0x20, 0x60);
        Video.LoadPaletteRange(Video.SubBackground, palette.Colors, 0xF0, 0x1E0, 0x20);
        Heap.Heap_Free(buffer);
    }

    // OFFSET 0x0223B768
    private static void TrainerCase_LoadCaseCoverPalette(byte gameVersion, NARC narc)
    {
        uint member;
        switch (gameVersion)
        {
            case 10:
                member = 7;
                break;
            case 11:
                member = 8;
                break;
            case 12:
                member = 0xA;
                break;
            case 7:
                member = 0xB;
                break;
            case 8:
                member = 0xC;
                break;
            default:
                member = 9;
                break;
        }
        PaletteFile palette = LoadPalette(narc, member, out byte[] buffer);
        Video.LoadPaletteRange(Video.MainBackground, palette.Colors, 0, 0, 0x20);
        Video.LoadPaletteRange(Video.SubBackground, palette.Colors, 0, 0, 0x20);
        Heap.Heap_Free(buffer);
    }

    // OFFSET 0x0223B820
    private static void TrainerCase_LoadTrainerPalette(byte trainerAppearance, NARC narc)
    {
        PaletteFile palette = LoadPalette(narc, 0x30, out byte[] buffer);
        Video.LoadPaletteRange(Video.SubBackground, palette.Colors, trainerAppearance * 0x10, 0x80, 0x20);
        Heap.Heap_Free(buffer);
    }

    // OFFSET 0x0223B858
    private static void TrainerCaseApp_InitBackgrounds(BgConfig bgConfig)
    {
        Video.PowerControl = (ushort)(Video.PowerControl & 0x7FFF);
        Bg.SetAllGraphicsModes(RomStructs.GraphicsModes(RomData.Overlay(Overlay, GraphicsModesAddress, 0x10), 0));
        Bg.Bg_InitFromTemplate(bgConfig, 7, OverlayBgTemplate(BgTemplateLayer7), 2);
        Bg.Bg_ClearTilemap(bgConfig, 7);
        Bg.Bg_InitFromTemplate(bgConfig, 5, OverlayBgTemplate(BgTemplateLayer5), 0);
        Bg.Bg_ClearTilemap(bgConfig, 5);
        Bg.Bg_InitFromTemplate(bgConfig, 6, OverlayBgTemplate(BgTemplateLayer6), 1);
        Bg.Bg_ClearTilemap(bgConfig, 6);
        Bg.Bg_InitFromTemplate(bgConfig, 2, OverlayBgTemplate(BgTemplateLayer2), 0);
        Bg.Bg_ClearTilemap(bgConfig, 2);
        Bg.Bg_InitFromTemplate(bgConfig, 3, OverlayBgTemplate(BgTemplateLayer3), 1);
        Bg.Bg_ClearTilemap(bgConfig, 3);
    }

    // OFFSET 0x0223B968
    private static void TrainerCase_DrawTrainerCard(TrainerCaseApp app, NARC narc)
    {
        TrainerCase trainerCase = app.TrainerCase!;
        BgConfig bgConfig = app.BgConfig!;
        PaletteFile palette = LoadPalette(narc, 0, out byte[] buffer);
        Video.LoadPaletteRange(Video.SubBackground, palette.Colors, 0, 0, 0x200);
        Heap.Heap_Free(buffer);
        TrainerCase_LoadCardPalette(trainerCase.U8(TrainerCase.CardLevelOffset), CaseFlag(trainerCase, TrainerCase.FlagPokedexObtained), narc);
        palette = LoadPalette(narc, 0xD, out buffer);
        Video.LoadPaletteRange(Video.MainBackground, palette.Colors, 0, 0, 0x200);
        Heap.Heap_Free(buffer);
        TrainerCase_LoadCaseCoverPalette(trainerCase.U8(TrainerCase.GameVersionOffset), narc);
        byte appearance = trainerCase.U8(TrainerCase.TrainerAppearanceOffset);
        if (appearance == 0xFF)
        {
            uint spriteMember;
            uint screenMember;
            int paletteMember;
            byte version = trainerCase.U8(TrainerCase.GameVersionOffset);
            if (version == 10 || version == 11)
            {
                spriteMember = 0x20;
                screenMember = 0x2A;
                paletteMember = 0x1A;
            }
            else
            {
                screenMember = 0x28;
                paletteMember = -1;
                spriteMember = 0x1F;
            }
            app.TrainerSprite = Graphics.LoadMemberFromOpenNARC(narc, spriteMember, 0, HeapID, 0);
            if (app.TrainerSprite == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            if (!G2d.GetUnpackedCharacterData(app.TrainerSprite!, out app.TrainerCharacterData))
            {
                ErrorHandling.GF_AssertFail();
            }
            if (paletteMember != -1)
            {
                Graphics.Graphics_LoadPaletteFromOpenNARC(narc, (uint)paletteMember, 4, 0x80, 0x40, HeapID);
            }
            if (CaseFlag(trainerCase, TrainerCase.FlagGender) == 0)
            {
                app.TrainerScreenDataNSCRBuffer = Graphics.Graphics_GetScrnDataFromOpenNARC(narc, screenMember, 0, out app.TrainerScreenData, HeapID);
            }
            else
            {
                app.TrainerScreenDataNSCRBuffer = Graphics.Graphics_GetScrnDataFromOpenNARC(narc, screenMember + 1, 0, out app.TrainerScreenData, HeapID);
            }
        }
        else
        {
            uint member = RomData.OverlayU32(Overlay, (uint)(AppearanceSprites + appearance * 4), 1)[0];
            app.TrainerSprite = Graphics.LoadMemberFromOpenNARC(narc, member, 0, HeapID, 0);
            if (app.TrainerSprite == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            if (!G2d.GetUnpackedCharacterData(app.TrainerSprite!, out app.TrainerCharacterData))
            {
                ErrorHandling.GF_AssertFail();
            }
            app.TrainerScreenDataNSCRBuffer = Graphics.Graphics_GetScrnDataFromOpenNARC(narc, 0x31, 0, out app.TrainerScreenData, HeapID);
            TrainerCase_LoadTrainerPalette(appearance, narc);
        }
        TrainerCase_DrawTrainer(app);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x1B, bgConfig, 6, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x23, bgConfig, 6, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x1C, bgConfig, 5, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x25, bgConfig, 5, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x1D, bgConfig, 2, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x26, bgConfig, 2, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x1E, bgConfig, 3, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x27, bgConfig, 3, 0, 0, 0, HeapID);
        TrainerCase_UnpackSignature(trainerCase, app.Signature);
    }

    // OFFSET 0x0223BBDC
    private static void TrainerCase_TeardownBgs(BgConfig bgConfig)
    {
        GXLayers.GXLayers_EngineAToggleLayers(0x1F, 0);
        GXLayers.GXLayers_EngineBToggleLayers(0x13, 0);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 7);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 6);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 5);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 2);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 3);
    }

    // OFFSET 0x0223BC20
    private static bool TrainerCase_FlipTrainerCard(TrainerCaseApp app)
    {
        bool done = false;
        switch (app.SubState)
        {
            case 0:
                app.CardScalingSpeed = 8;
                app.CardXScale = Fx32One;
                app.CardYScale = Fx32One;
                app.CardXScale += 0x80;
                app.CardYScale += 0x80;
                SoundPlayback.Sound_PlayEffect(SeqCardFlip);
                app.SubState++;
                break;
            case 1:
                app.CardXScale -= 2 << (12 - app.CardScalingSpeed);
                if (app.CardXScale <= 0)
                {
                    app.CardXScale = 0x24;
                    app.SubState++;
                }
                app.CardScalingSpeed--;
                if (app.CardScalingSpeed <= 1)
                {
                    app.CardScalingSpeed = 1;
                }
                break;
            case 2:
                if (!app.ViewingBack)
                {
                    app.ViewingBack = true;
                    Graphics.Graphics_LoadTilemapToBgLayer(TrainerCaseArchive, 0x24, app.BgConfig!, 6, 0, 0, 0, HeapID);
                    TrainerCard_ClearWindows(app.Windows, 0, 6);
                    TrainerCase_ClearTrainerSprite(app);
                    Bg.Bg_ClearTilemap(app.BgConfig!, 7);
                    TrainerCard_DrawBackText(app.Windows, app.TrainerCase!);
                    TrainerCase_DisplaySignature(app.BgConfig!, 7, app.Signature);
                }
                else
                {
                    app.ViewingBack = false;
                    Graphics.Graphics_LoadTilemapToBgLayer(TrainerCaseArchive, 0x23, app.BgConfig!, 6, 0, 0, 0, HeapID);
                    TrainerCard_ClearWindows(app.Windows, 7, 0xA);
                    Bg.Bg_ClearTilemap(app.BgConfig!, 7);
                    TrainerCase_DrawTrainer(app);
                    TrainerCard_DrawFrontText(app.Windows, app.TrainerCase!);
                }
                app.SubState++;
                break;
            case 3:
                app.CardScalingSpeed++;
                if (app.CardScalingSpeed > 8)
                {
                    app.CardScalingSpeed = 8;
                }
                app.CardXScale += 2 << (12 - app.CardScalingSpeed);
                if (app.CardXScale >= Fx32One)
                {
                    app.CardXScale = Fx32One;
                    app.CardYScale = Fx32One;
                    done = true;
                }
                break;
        }
        SetScaleAffine(6, app.CardXScale, app.CardYScale, 0x80, 0x60);
        SetScaleAffine(7, app.CardXScale, app.CardYScale, 0x80, 0x60);
        return done;
    }

    // OFFSET 0x0223BDF8
    private static bool TrainerCase_OpenCloseBadgeCase(TrainerCaseApp app)
    {
        int[] steps = Array.ConvertAll(RomData.OverlayU32(Overlay, CoverScaleSteps, CoverTransitionSteps), v => (int)v);
        switch (app.SubState)
        {
            case 0:
                app.BadgeCaseTransitionIndex = 0;
                if (app.BadgeCaseOpenState == 0)
                {
                    app.BadgeCaseCoverYScale = Fx32One;
                    app.SubState = 1;
                }
                else
                {
                    app.SubState = 2;
                }
                TrainerCase_PlayOpenCloseBadgeCaseSoundEffect(app.BadgeCaseOpenState);
                break;
            case 1:
            {
                int index = app.BadgeCaseTransitionIndex++;
                app.BadgeCaseCoverYScale -= steps[index];
                if (app.BadgeCaseTransitionIndex == CoverTransitionSteps)
                {
                    app.BadgeCaseOpenState = 1;
                    app.SubState = 3;
                }
                break;
            }
            case 2:
            {
                int index = app.BadgeCaseTransitionIndex++;
                app.BadgeCaseCoverYScale += steps[13 - index];
                if (app.BadgeCaseTransitionIndex == CoverTransitionSteps)
                {
                    app.BadgeCaseOpenState = 0;
                    app.BadgeCaseCoverYScale = Fx32One;
                    app.SubState = 3;
                }
                break;
            }
            case 3:
                return true;
        }
        SetScaleAffine(3, Fx32One, app.BadgeCaseCoverYScale, 0x80, 0);
        return false;
    }

    private static TouchScreenHitTable[] OverlayTouchRects(uint address)
    {
        byte[] raw = RomData.Overlay(Overlay, address, 0x20 * 4);
        int count = 0;
        while (raw[count * 4] != TouchScreen.TableTerminator)
        {
            count++;
        }
        TouchScreenHitTable[] rects = new TouchScreenHitTable[count + 1];
        for (int i = 0; i <= count; i++)
        {
            rects[i].Top = raw[i * 4];
            rects[i].Bottom = raw[i * 4 + 1];
            rects[i].Left = raw[i * 4 + 2];
            rects[i].Right = raw[i * 4 + 3];
        }
        return rects;
    }

    private static TouchScreenHitTable[] CurrentTouchRects(TrainerCaseApp app)
    {
        int interactable = CaseFlag(app.TrainerCase!, TrainerCase.FlagBadgesInteractable);
        uint address = RomData.OverlayU32(Overlay, (uint)(TouchRectTables + interactable * 8 + app.BadgeCaseOpenState * 4), 1)[0];
        return OverlayTouchRects(address);
    }

    // OFFSET 0x0223BEF8
    private static int TrainerCase_GetPlayerInput(TrainerCaseApp app)
    {
        bool touched = false;
        int input = InputNone;
        app.TouchedRectangleIndex = -1;
        if (Globals.gSystem.TouchPressed != 0)
        {
            app.PolishingEnabled = true;
        }
        app.TouchedRectangleIndex = TrainerCase_CheckBadgeOrButtonTouched(app.BgConfig!, CurrentTouchRects(app));
        if (app.TouchedRectangleIndex != -1)
        {
            touched = true;
            input = InputTouched;
        }
        else if (Globals.gSystem.TouchHeld != 0)
        {
            app.TouchedRectangleIndex = TrainerCase_CheckBadgeOrButtonHeld(app.BgConfig!, CurrentTouchRects(app));
            if (app.PolishingEnabled)
            {
                touched = true;
                input = InputHeld;
            }
        }
        else
        {
            app.PolishingEnabled = false;
        }
        if (!touched)
        {
            if ((Globals.gSystem.PressedKeys & KeyA) != 0)
            {
                input = InputFlip;
            }
            else if ((Globals.gSystem.PressedKeys & KeyB) != 0)
            {
                input = InputExit;
            }
        }
        return input;
    }

    // OFFSET 0x0223BFBC
    private static void TrainerCase_HandleBadgePolishing(TrainerCaseApp app)
    {
        int touchX = Globals.gSystem.TouchX;
        int touchY = Globals.gSystem.TouchY;
        bool polished = false;
        if (touchX != 0xFFFF && touchY != 0xFFFF && app.LastTouchedX != 0xFFFF && app.LastTouchedY != 0xFFFF
            && app.TouchedRectangleIndex != -1 && app.TouchedRectangleIndex != 0
            && BadgeObtained(app.TrainerCase!, app.TouchedRectangleIndex - 1))
        {
            BadgePolishingState polishing = app.BadgePolishingState;
            int deltaX;
            if (app.LastTouchedX > touchX)
            {
                deltaX = app.LastTouchedX - touchX;
                polishing.CurrentXDirection = -1;
            }
            else
            {
                deltaX = touchX - app.LastTouchedX;
                polishing.CurrentXDirection = 1;
            }
            if (deltaX >= 3 && deltaX <= 40)
            {
                int deltaY = CalcYDelta(app, touchY);
                if (deltaY <= 40)
                {
                    polished = true;
                    TrainerCase_PlayPolishingSoundEffects(polishing);
                }
                else
                {
                    TrainerCase_ResetBadgePolishingDirection(polishing);
                }
            }
            else if (deltaX <= 40)
            {
                int deltaY = CalcYDelta(app, touchY);
                if (deltaY >= 3 && deltaY <= 40)
                {
                    polished = true;
                    TrainerCase_PlayPolishingSoundEffects(polishing);
                }
                else
                {
                    TrainerCase_ResetBadgePolishingDirection(polishing);
                }
            }
            if (polished)
            {
                TrainerCase_PolishBadge(app, (byte)(app.TouchedRectangleIndex - 1));
            }
        }
        app.LastTouchedX = Globals.gSystem.TouchX;
        app.LastTouchedY = Globals.gSystem.TouchY;
    }

    private static int CalcYDelta(TrainerCaseApp app, int touchY)
    {
        if (app.LastTouchedY > touchY)
        {
            app.BadgePolishingState.CurrentYDirection = -1;
            return app.LastTouchedY - touchY;
        }
        app.BadgePolishingState.CurrentYDirection = 1;
        return touchY - app.LastTouchedY;
    }

    // OFFSET 0x0223C0D8
    private static void TrainerCase_RedrawBadgeCaseButton(TrainerCaseApp app, int frame)
    {
        byte[] tilemap = Bg.Bg_GetTilemapBuffer(app.BgConfig!, 2)!;
        int tileBase = (ushort)(frame * 4 + 0x80);
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                int index = (row + 0x13) * 32 + 0xE + col;
                int value = tileBase + row * 32 + col;
                tilemap[index * 2] = (byte)value;
                tilemap[index * 2 + 1] = (byte)(value >> 8);
            }
        }
        Bg.Bg_CopyTilemapBufferToVRAM(app.BgConfig!, 2);
    }

    // OFFSET 0x0223C128
    private static void TrainerCase_HideGymLeader(TrainerCaseApp app, int badge)
    {
        int column = badge % 4;
        int row = badge >> 2;
        int x = (byte)(column * 7 + 3);
        int y = (byte)(row * 7 + 4);
        byte[] tilemap = Bg.Bg_GetTilemapBuffer(app.BgConfig!, 3)!;
        for (int r = 0; r < 5; r++)
        {
            for (int c = 0; c < 5; c++)
            {
                tilemap[(r + y) * 32 + x + c] = (byte)(r * 16 + c + 0xBA);
            }
        }
    }

    // OFFSET 0x0223C18C
    private static void TrainerCaseApp_VBlankCallback(object? data)
    {
        VramTransfer.VramTransfer_Process();
        RenderOam.RenderOam_Transfer();
    }

    // OFFSET 0x0223C1AC
    private static void TrainerCase_PolishBadge(TrainerCaseApp app, byte badge)
    {
        TrainerCase trainerCase = app.TrainerCase!;
        int polish = BadgePolish(trainerCase, badge);
        int newPolish = polish + 1;
        if (newPolish >= 200)
        {
            return;
        }
        int oldLevel = TrainerCase_GetBadgePolishLevel(polish);
        byte threshold = RomData.Overlay(Overlay, (uint)(PolishThresholds + oldLevel), 1)[0];
        app.PolishingProgress[badge]++;
        if (app.PolishingProgress[badge] < threshold)
        {
            return;
        }
        app.PolishingProgress[badge] = 0;
        uint data = BadgeData(trainerCase, badge);
        trainerCase.W32(TrainerCase.BadgesOffset + badge * 4, (uint)((((int)data >> 1) + 1) << 1) | (data & 1));
        int newLevel = TrainerCase_GetBadgePolishLevel(newPolish);
        if (oldLevel > newLevel)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (oldLevel >= newLevel)
        {
            return;
        }
        if (newLevel <= 4)
        {
            TrainerCase_DrawBadgeDirt(app.SpriteData, badge, (byte)(newLevel == 4 ? 0 : 3 - newLevel));
        }
        if (newLevel == 3)
        {
            Sprite.Sprite_SetDrawFlag(app.SpriteData.Sprites[SpriteSparklesTwo + badge]!, 1);
            return;
        }
        if (newLevel == 4)
        {
            Sprite.Sprite_SetDrawFlag(app.SpriteData.Sprites[SpriteSparklesTwo + badge]!, 0);
            Sprite.Sprite_SetDrawFlag(app.SpriteData.Sprites[SpriteSparklesFour + badge]!, 1);
        }
    }

    // OFFSET 0x0223C288
    private static void TrainerCase_ResetAffineTransforms()
    {
        SetScaleAffine(6, Fx32One, Fx32One, 0x80, 0x60);
        SetScaleAffine(7, Fx32One, Fx32One, 0x80, 0x60);
        SetScaleAffine(3, Fx32One, Fx32One, 0x80, 0);
    }

    // OFFSET 0x0223C2F4
    private static void TrainerCase_HandleBadgeCaseButtonAnimation(TrainerCaseApp app)
    {
        if (app.BadgeCaseButtonState == 1)
        {
            if (TrainerCase_AnimateBadgeCaseButton(app, ButtonPressAnim))
            {
                app.BadgeCaseButtonState = 0;
            }
        }
        else if (app.BadgeCaseButtonState == 2)
        {
            if (TrainerCase_AnimateBadgeCaseButton(app, ButtonReleaseAnim))
            {
                app.BadgeCaseButtonState = 0;
            }
        }
    }

    // OFFSET 0x0223C334
    private static bool TrainerCase_AnimateBadgeCaseButton(TrainerCaseApp app, uint animAddress)
    {
        byte[] anim = RomData.Overlay(Overlay, animAddress, 0x10);
        byte duration = anim[app.BadgeCaseButtonAnimIndex * 2];
        if (duration == 0)
        {
            app.BadgeCaseButtonAnimTimer = 0;
            app.BadgeCaseButtonAnimIndex = 0;
            return true;
        }
        if (app.BadgeCaseButtonAnimTimer >= duration)
        {
            app.BadgeCaseButtonAnimTimer = 0;
            app.BadgeCaseButtonAnimIndex++;
        }
        byte frame = anim[app.BadgeCaseButtonAnimIndex * 2 + 1];
        if (app.BadgeCaseButtonAnimTimer == 0)
        {
            TrainerCase_RedrawBadgeCaseButton(app, frame);
        }
        app.BadgeCaseButtonAnimTimer++;
        return false;
    }

    // OFFSET 0x0223C390
    private static void TrainerCase_DrawTrainer(TrainerCaseApp app)
    {
        ScreenData screenData = app.TrainerScreenData!;
        uint size = screenData.SizeBytes;
        CharacterData charData = app.TrainerCharacterData!;
        Bg.Bg_LoadTiles(app.BgConfig!, 7, charData.RawData, charData.SizeBytes, 0);
        if (Bg.Bg_GetTilemapBuffer(app.BgConfig!, 7) != null)
        {
            Bg.Bg_LoadTilemapBuffer(app.BgConfig!, 7, screenData.RawData, size);
        }
        Bg.Bg_CopyTilemapBufferRangeToVRAM(app.BgConfig!, 7, screenData.RawData, size, 0);
    }

    // OFFSET 0x0223C3E8
    private static void TrainerCase_ClearTrainerSprite(TrainerCaseApp app)
    {
        Bg.Bg_FillTilemapRect(app.BgConfig!, 7, 0, 0x14, 6, 6, 9, 0x10);
    }

    // OFFSET 0x0223C40C
    private static int TrainerCase_GetBadgePolishLevel(int polish)
    {
        if (polish >= 0 && polish < 100)
        {
            return 0;
        }
        if (polish < 140)
        {
            return 1;
        }
        if (polish < 170)
        {
            return 2;
        }
        if (polish < 190)
        {
            return 3;
        }
        if (polish < 200)
        {
            return 4;
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x0223C444
    private static void TrainerCase_ResetBadgePolishingDirection(BadgePolishingState state)
    {
        state.CurrentXDirection = 0;
        state.CurrentYDirection = 0;
    }

    // OFFSET 0x0223C44C
    private static void TrainerCase_ResetBadgePolishingState(BadgePolishingState state)
    {
        state.PreviousXDirection = 0;
        state.PreviousYDirection = 0;
        state.CurrentXDirection = 0;
        state.CurrentYDirection = 0;
        state.SoundEffectIndex = 0;
    }

    // OFFSET 0x0223C45C
    private static void TrainerCase_PlayPolishingSoundEffects(BadgePolishingState state)
    {
        uint[] sounds = RomData.OverlayU32(Overlay, PolishingSounds, 2);
        if (state.PreviousXDirection == 0 && state.PreviousYDirection == 0)
        {
            SoundPlayback.Sound_PlayEffect((int)sounds[0]);
        }
        if (state.PreviousXDirection * state.CurrentXDirection < 0 || state.PreviousYDirection * state.CurrentYDirection < 0)
        {
            state.SoundEffectIndex = (byte)((state.SoundEffectIndex + 1) % 2);
            SoundPlayback.Sound_PlayEffect((ushort)sounds[state.SoundEffectIndex]);
        }
        state.PreviousXDirection = state.CurrentXDirection;
        state.PreviousYDirection = state.CurrentYDirection;
        state.CurrentXDirection = 0;
        state.CurrentYDirection = 0;
    }

    // OFFSET 0x0223C4DC
    private static void TrainerCase_UnpackSignature(TrainerCase trainerCase, byte[] signature)
    {
        for (int i = 0; i < TrainerCaseApp.SignatureBytes; i++)
        {
            byte packed = trainerCase.U8(TrainerCase.SignatureOffset + (i / 64) * 8 + (i / 8) % 8);
            signature[i] = (byte)((packed >> (i % 8)) & 1);
        }
    }

    // OFFSET 0x0223C530
    private static void TrainerCase_DisplaySignature(BgConfig bgConfig, int bgLayer, byte[] signature)
    {
        Bg.Bg_LoadTiles(bgConfig, (byte)bgLayer, signature, TrainerCaseApp.SignatureBytes, 1);
        byte[] tilemap = Bg.Bg_GetTilemapBuffer(bgConfig, (byte)bgLayer)!;
        ushort tile = 0;
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 0x18; col++)
            {
                tile++;
                int index = (row + 0xE) * 32 + 4 + col;
                tilemap[index * 2] = (byte)tile;
                tilemap[index * 2 + 1] = (byte)(tile >> 8);
            }
        }
        Bg.Bg_CopyTilemapBufferToVRAM(bgConfig, (byte)bgLayer);
    }

    // OFFSET 0x0223C594
    private static void TrainerCase_PlayOpenCloseBadgeCaseSoundEffect(byte openState)
    {
        SoundPlayback.Sound_PlayEffect(SeqCaseOpenClose);
    }

    // OFFSET 0x0223C5A4
    private static void TrainerCase_UpdatePlayTime(TrainerCaseApp app, bool liveTimeDisplay)
    {
        if (!liveTimeDisplay)
        {
            return;
        }
        if (!app.ViewingBack)
        {
            if (app.Timer == 15)
            {
                TrainerCard_DrawUpdatedTime(app.Windows, app.TrainerCase!, app.UnusedString!);
                TrainerCard_BlinkPlaytimeColon(app.Windows[WindowTime], true, app.ColonString!);
            }
            else if (app.Timer == 0)
            {
                TrainerCard_BlinkPlaytimeColon(app.Windows[WindowTime], false, app.ColonString!);
            }
        }
        app.Timer = (byte)((app.Timer + 1) % 30);
    }

    // OFFSET 0x0223C60C
    private static int TrainerCase_CheckBadgeOrButtonTouched(BgConfig bgConfig, TouchScreenHitTable[] rects)
    {
        int index = TouchScreen.TouchScreen_CheckRectanglePressed(rects);
        if (index == -1 || index == 0)
        {
            return index;
        }
        if (Bg.Bg_DoesPixelAtXYMatchVal(bgConfig, 2, Globals.gSystem.TouchX, Globals.gSystem.TouchY, new ushort[] { 0x40 }) == 0)
        {
            return -1;
        }
        return index;
    }

    // OFFSET 0x0223C654
    private static int TrainerCase_CheckBadgeOrButtonHeld(BgConfig bgConfig, TouchScreenHitTable[] rects)
    {
        int index = TouchScreen.TouchScreen_CheckRectangleHeld(rects);
        if (index == -1 || index == 0)
        {
            return index;
        }
        if (Bg.Bg_DoesPixelAtXYMatchVal(bgConfig, 2, Globals.gSystem.TouchX, Globals.gSystem.TouchY, new ushort[] { 0x40 }) == 0)
        {
            return -1;
        }
        return index;
    }

    // OFFSET 0x0223D324
    private static void TrainerCaseApp_InitBadgeChimeState(byte[] state)
    {
        for (int i = 0; i < TrainerCaseApp.NumChimePlayers; i++)
        {
            state[i * 2] = (byte)i;
            state[i * 2 + 1] = 0xFF;
        }
    }

    // OFFSET 0x0223D338
    private static void TrainerCase_PlayBadgeChime(byte[] state, byte badge, byte polishLevel)
    {
        TrainerCase_CheckBadgeChimesFinishedPlaying(state);
        if (TrainerCase_FindSoundEffectPlayerWithSameBadgeChime(state, badge, polishLevel))
        {
            return;
        }
        if (TrainerCase_FindFreeSoundEffectPlayer(state, badge, polishLevel))
        {
            return;
        }
        TrainerCase_GetLeastRecentlyUsedSoundEffectPlayer(state, badge, polishLevel);
    }

    // OFFSET 0x0223D370
    private static void TrainerCase_CheckBadgeChimesFinishedPlaying(byte[] state)
    {
        for (int i = 0; i < TrainerCaseApp.NumChimePlayers; i++)
        {
            if (state[i * 2 + 1] != 0xFF && Sound.Sound_GetNumberOfPlayingSequencesForPlayer(state[i * 2] + 3) == 0)
            {
                state[i * 2 + 1] = 0xFF;
            }
        }
        TrainerCase_DoNothing(state);
    }

    // OFFSET 0x0223D3A0
    private static bool TrainerCase_FindSoundEffectPlayerWithSameBadgeChime(byte[] state, byte badge, byte polishLevel)
    {
        for (int i = 0; i < TrainerCaseApp.NumChimePlayers; i++)
        {
            if (badge == state[i * 2 + 1])
            {
                TrainerCase_PlayBadgeChimeSoundEffect(state[i * 2], badge, polishLevel);
                TrainerCase_FlagSoundEffectPlayerInUse(state, i, state[i * 2], badge);
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0223D3D8
    private static bool TrainerCase_FindFreeSoundEffectPlayer(byte[] state, byte badge, byte polishLevel)
    {
        for (int i = 0; i < TrainerCaseApp.NumChimePlayers; i++)
        {
            if (state[i * 2 + 1] == 0xFF)
            {
                TrainerCase_PlayBadgeChimeSoundEffect(state[i * 2], badge, polishLevel);
                TrainerCase_FlagSoundEffectPlayerInUse(state, i, state[i * 2], badge);
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0223D410
    private static bool TrainerCase_GetLeastRecentlyUsedSoundEffectPlayer(byte[] state, byte badge, byte polishLevel)
    {
        TrainerCase_PlayBadgeChimeSoundEffect(state[6], badge, polishLevel);
        TrainerCase_FlagSoundEffectPlayerInUse(state, 3, state[6], badge);
        return true;
    }

    // OFFSET 0x0223D42C
    private static void TrainerCase_FlagSoundEffectPlayerInUse(byte[] state, int index, byte player, byte badge)
    {
        for (int i = index; i > 0; i--)
        {
            state[i * 2] = state[i * 2 - 2];
            state[i * 2 + 1] = state[i * 2 - 1];
        }
        state[0] = player;
        state[1] = badge;
    }

    // OFFSET 0x0223D454
    private static void TrainerCase_DoNothing(byte[] state)
    {
        for (int i = 0; i < TrainerCaseApp.NumChimePlayers; i++)
        {
            if (state[i * 2] == 0xFF && i < TrainerCaseApp.NumChimePlayers - 1)
            {
                state[i * 2] = state[i * 2 + 2];
                state[i * 2 + 2] = 0xFF;
            }
        }
    }

    // OFFSET 0x0223D470
    private static void TrainerCase_PlayBadgeChimeSoundEffect(byte player, byte badge, byte polishLevel)
    {
        Sound.Sound_PlaySequenceWithPlayer(player + 3, player + 3, SeqBadgeChime);
        ushort pitch = RomData.OverlayU16(Overlay, (uint)(ChimePitches + badge * 2), 1)[0];
        Sound.Sound_SetPitchForHandle(player + 3, 0xFFFF, pitch - (4 - polishLevel) * 0x98);
    }
}
