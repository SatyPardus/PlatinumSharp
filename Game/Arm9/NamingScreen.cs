using static PlatinumSharp.Game.Globals;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class NamingScreen
{
    private const int HeapID = 0x12;
    private const int HeapSize = 0x28000;
    private const int NamingScreenArchive = 0x1F;
    private const int FontArchive = 0xC;
    private const int PokemonIconArchive = 0x13;
    private const int MessageArchive = 0x1A;
    private const int NamingScreenTextBank = 0x1A6;
    private const int GenericNamesTextBank = 0x1AB;
    private const int BattleStringsTextBank = 0x170;
    private const uint AllLayersMask = 0xFFFFE0FF;
    private const uint WindowsMask = 0xFFFF1FFF;
    private const uint Window0Enable = 0x2000;
    private const int DisplayMain = 0;
    private const int DisplaySub = 1;
    private const ushort CharEOS = 0xFFFF;
    private const ushort CharSpace = 0x1;
    private const ushort CharWideSpace = 0x1DE;
    private const ushort ControlDakuten = 0xD001;
    private const ushort ControlHandakuten = 0xD002;
    private const ushort ControlSpace = 0xD003;
    private const ushort ControlSkip = 0xD004;
    private const ushort ButtonStart = 0xE001;
    private const ushort ButtonPageUpper = 0xE002;
    private const ushort ButtonPageLower = 0xE003;
    private const ushort ButtonPageOthers = 0xE004;
    private const ushort ButtonPageJapanese = 0xE005;
    private const ushort ButtonPageJapanese2 = 0xE006;
    private const ushort ButtonBack = 0xE007;
    private const ushort ButtonOk = 0xE008;
    private const int TypePlayer = 0;
    private const int TypePokemon = 1;
    private const int TypeRival = 3;
    private const int TypeFriendCode = 4;
    private const int TypeGroup = 5;
    private const int AppStateWaitFadeIn = 0;
    private const int AppStateInitialDelay = 1;
    private const int AppStateRunning = 2;
    private const int AppStateWaitFadeOut = 3;
    private const int StateLoadGraphics = 0;
    private const int StateProcessInputs = 4;
    private const int StatePrintBattleMessage = 5;
    private const int StateWaitBattleMessage = 6;
    private const int StateStartFadeOut = 7;
    private const int SpriteBackButton = 5;
    private const int SpriteOkButton = 6;
    private const int SpriteOverlay = 7;
    private const int SpriteCursor = 8;
    private const int SpriteCount = 9;
    private const int WindowEnteredText = 3;
    private const int WindowLastChar = 4;
    private const int WindowGroupText = 8;
    private const int WindowPrompt = 9;
    private const int KeyA = 0x1;
    private const int KeyB = 0x2;
    private const int KeySelect = 0x4;
    private const int KeyStart = 0x8;
    private const int KeyRight = 0x10;
    private const int KeyLeft = 0x20;
    private const int KeyUp = 0x40;
    private const int KeyDown = 0x80;
    private const int KeyR = 0x100;
    private const int SeqCursorMove = 0x5DC;
    private const int SeqConfirm = 0x5E2;
    private const int SeqCharEntered = 0x5EB;
    private const int SeqCharsSlideDone = 0x69B;
    private const int SeqPageChange = 0x69E;
    private const uint EntryTextColor = 0xE0F01;
    private const uint KeyboardTextColor = 0xE0F00;
    private const uint ObjTextColor = 0xD0E0F;
    private const ushort MessageBoxTile = 0x100;
    private const byte MessageBoxPalette = 0xA;
    private const int SpeciesLevel = 5;
    private const uint BanksAddress = 0x020F28DC;
    private const uint GraphicsModesAddress = 0x020F24FC;
    private const uint Bg0TemplateAddress = 0x020F2670;
    private const uint Bg1TemplateAddress = 0x020F2750;
    private const uint Bg2TemplateAddress = 0x020F2814;
    private const uint SubBg0TemplateAddress = 0x020F25AC;
    private const uint PromptTextIndicesAddress = 0x020F2850;
    private const uint CharTransferTemplateAddress = 0x020F250C;
    private const uint SpriteAnimationsAddress = 0x020F2984;
    private const uint ObjOffsetsWideAddress = 0x020F24D8;
    private const uint CharsBgColorAddress = 0x020F24DC;
    private const uint CharsAltBgColorAddress = 0x020F24E0;
    private const uint HomeRowCursorAnimIDsAddress = 0x020F24E8;
    private const uint ObjOffsetsCharAddress = 0x020F24F0;
    private const uint HomeRowCursorXCoordsAddress = 0x020F251C;
    private const uint DpadMovementAddress = 0x020F2904;
    private const uint HandakutenTableAddress = 0x020F292C;
    private const uint DakutenTableAddress = 0x020F2954;
    private const uint TouchHitboxesAddress = 0x020F2A14;
    private const uint ConversionTableAddress = 0x020F2BBE;
    private const uint HomeRowLayoutsAddress = 0x02100C40;
    private const uint CharCodesAddress = 0x02100C54;
    private const int ConversionTableRows = 0x52;
    private const int DakutenTableRows = 12;
    private const int HandakutenTableRows = 10;
    private const int TouchHitboxCount = 0x47;
    private const int TouchHitboxSize = 6;
    private const int KeyboardPages = 5;
    private const int KeyboardPageRows = 5;

    private static NamingScreenData? sNamingScreenDummy;
    private static int[]? sSpriteAnimations;
    private static int[]? sDpadMovement;
    private static ushort[]? sConversionTable;
    private static ushort[]? sDakutenTable;
    private static ushort[]? sHandakutenTable;
    private static byte[]? sTouchHitboxes;

    private static int[] SpriteAnimations => sSpriteAnimations ??= ReadInts(SpriteAnimationsAddress, SpriteCount * 4);

    private static int[] DpadMovement => sDpadMovement ??= ReadInts(DpadMovementAddress, 10);

    private static ushort[] ConversionTable => sConversionTable ??= RomData.Arm9U16(ConversionTableAddress, ConversionTableRows * 3);

    private static ushort[] DakutenTable => sDakutenTable ??= RomData.Arm9U16(DakutenTableAddress, DakutenTableRows * 2);

    private static ushort[] HandakutenTable => sHandakutenTable ??= RomData.Arm9U16(HandakutenTableAddress, HandakutenTableRows * 2);

    private static byte[] TouchHitboxes => sTouchHitboxes ??= RomData.Arm9(TouchHitboxesAddress, TouchHitboxCount * TouchHitboxSize);

    private static int[] ReadInts(uint address, int count)
    {
        uint[] raw = RomData.Arm9U32(address, count);
        int[] values = new int[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = (int)raw[i];
        }
        return values;
    }

    private static ushort[] ReadCharCodes(uint address)
    {
        List<ushort> chars = new();
        while (true)
        {
            ushort c = RomData.Arm9U16(address, 1)[0];
            chars.Add(c);
            if (c == CharEOS)
            {
                break;
            }
            address += 2;
        }
        return chars.ToArray();
    }

    private static ushort[] KeyboardRow(int charsIdx, int row)
    {
        return ReadCharCodes(RomData.Arm9U32(CharCodesAddress + (uint)((charsIdx * KeyboardPageRows + row) * 4), 1)[0]);
    }

    private static int SpriteAnimX(int index) => SpriteAnimations[index * 4];

    private static int SpriteAnimY(int index) => SpriteAnimations[index * 4 + 1];

    private static int SpriteAnimID(int index) => SpriteAnimations[index * 4 + 2];

    private static int SpriteAnimPriority(int index) => SpriteAnimations[index * 4 + 3];

    // OFFSET 0x0208694C
    public static int NamingScreen_Init(ApplicationManager appMan, ref int state)
    {
        switch (state)
        {
            case 0:
            {
                GameSystem.SetVBlankCallback(null, null);
                GameSystem.DisableHBlank();
                GXLayers.GXLayers_DisableEngineALayers();
                GXLayers.GXLayers_DisableEngineBLayers();
                Video.SetDisplayControl(DisplayMain, Video.GetDisplayControl(DisplayMain) & AllLayersMask);
                Video.SetDisplayControl(DisplaySub, Video.GetDisplayControl(DisplaySub) & AllLayersMask);
                Heap.Heap_Create(3, HeapID, HeapSize);
                NamingScreenData data = new();
                appMan.Data = data;
                data.BgConfig = Bg.BgConfig_New(HeapID);
                NARC narc = NARC.NARC_ctor(NamingScreenArchive, HeapID);
                data.StrTemplate = StringTemplate.StringTemplate_Default(HeapID);
                data.NamingScreenMsgLoader = MessageLoader.MessageLoader_Init(0, MessageArchive, NamingScreenTextBank, HeapID);
                data.GenericNamesMsgLoader = MessageLoader.MessageLoader_Init(1, MessageArchive, GenericNamesTextBank, HeapID);
                data.BattleStringsMsgLoader = MessageLoader.MessageLoader_Init(1, MessageArchive, BattleStringsTextBank, HeapID);
                GameSystem.SetAutorepeat(4, 8);
                NamingScreen_InitGraphicsBanks();
                NamingScreen_InitBgs(data.BgConfig);
                NamingScreen_CopyParamsFromArgs(data, (NamingScreenArgs)ApplicationManager.ApplicationManager_Args(appMan)!);
                NamingScreen_LoadGraphicsFromNarc(data, narc);
                Font.Font_InitManager(2, HeapID);
                GameSystem.SetVBlankCallback(NamingScreen_VBlankCallback, null);
                NamingScreen_InitCursorsAndChars(data, appMan);
                Font.Font_UseImmediateGlyphAccess(0, HeapID);
                NamingScreen_TransferChars();
                NamingScreen_LoadObjectGfx(data, narc);
                NamingScreen_InitSprites(data);
                NamingScreen_InitWindows(data, appMan, narc);
                NamingScreen_PrintLastCharOfEntryBuf(data.Windows, WindowLastChar, data.EntryBuf, data.TextCursorPos, data.TmpBuf, data.PixelBuf, data.LastCharString!);
                Sound.Sound_SetSceneAndPlayBGM(0x34, 0, 0);
                ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 0x10, 1, HeapID);
                NamingScreen_ToggleEngineLayers(1);
                gSystem.WhichScreenIs3D = 1;
                GXLayers.GXLayers_SwapDisplay();
                NARC.NARC_dtor(narc);
                state++;
                break;
            }
            case 1:
            {
                NamingScreenData data = (NamingScreenData)ApplicationManager.ApplicationManager_Data(appMan)!;
                if (data.Type == TypePokemon)
                {
                    NamingScreen_LoadMonIcon(data.MonIconCharData!, data.PlttData!, data.PlayerGenderOrMonSpecies, data.MonForm);
                }
                sNamingScreenDummy = data;
                data.TouchscreenIconBlinker = PaletteAnimator.PaletteAnimator_New(null, 1, 0xC, HeapID);
                state = 0;
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x02086B30
    private static void NamingScreen_LoadMonIcon(CharacterData charData, PaletteFile plttData, int species, int form)
    {
        Video.LoadObjectTiles(DisplayMain, charData.RawData, 0x57E0, 0x200);
        int paletteIndex = (int)PokemonIcon.PokeIconPaletteIndex((uint)species, (uint)form, 0);
        Video.LoadPaletteRange(Video.MainObject, plttData.Colors, paletteIndex * 0x10, 0xC0, 0x20);
    }

    // OFFSET 0x02086B64
    public static int NamingScreen_Main(ApplicationManager appMan, ref int state)
    {
        NamingScreenData data = (NamingScreenData)ApplicationManager.ApplicationManager_Data(appMan)!;
        switch (state)
        {
            case AppStateWaitFadeIn:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    state = AppStateInitialDelay;
                    data.DelayUpdateCounter = 0;
                }
                break;
            case AppStateInitialDelay:
                data.DelayUpdateCounter++;
                NamingScreen_PlaceCursorSprite(data);
                NamingScreen_UpdateSpriteAnimations(data.SpritesToUpdate, data.UiSprites, data.CurrentCharsIdx);
                if (data.DelayUpdateCounter > 5)
                {
                    state = AppStateRunning;
                    data.DelayUpdateCounter = 0;
                }
                break;
            case AppStateRunning:
                switch (data.State)
                {
                    case StateProcessInputs:
                        state = NamingScreen_ProcessInputs(data, state);
                        NamingScreen_PlaceCursorSprite(data);
                        break;
                    case StatePrintBattleMessage:
                        NamingScreen_PrepareBattleMessage(data, appMan);
                        Bg.Window_FillTilemap(data.Windows[WindowPrompt], 0xF);
                        RenderWindow.Window_DrawMessageBoxWithScrollCursor(data.Windows[WindowPrompt], 0, MessageBoxTile, MessageBoxPalette);
                        data.BattleMessagePrinter = Text.Text_AddPrinterWithParams(data.Windows[WindowPrompt], 1, data.BattleMessageString, 0, 0, 1, null);
                        Bg.Window_CopyToVRAM(data.Windows[WindowPrompt]);
                        data.State = StateWaitBattleMessage;
                        break;
                    case StateWaitBattleMessage:
                        if (Text.Text_IsPrinterActive((byte)data.BattleMessagePrinter) == 0)
                        {
                            SoundPlayback.Sound_PlayEffect(SeqConfirm);
                            data.SpritesToUpdate[SpriteOkButton]++;
                            data.DelayUpdateCounter = 0;
                            data.State = StateStartFadeOut;
                        }
                        break;
                    case StateStartFadeOut:
                        data.DelayUpdateCounter++;
                        if (data.DelayUpdateCounter > 0x1E)
                        {
                            ScreenFadeControl.StartScreenFade(2, 0, 0, 0, 0x10, 1, HeapID);
                            state = AppStateWaitFadeOut;
                        }
                        break;
                }
                NamingScreen_AnimateChangeChars(
                    data.BgConfig!,
                    data.Windows,
                    ref data.State,
                    data.CurrentCharsIdx,
                    ref data.FreeCharsBgLayer,
                    data.CharsPosition,
                    data.UiSprites,
                    data.CharData!.RawData);
                NamingScreen_UpdateSpriteAnimations(data.SpritesToUpdate, data.UiSprites, data.CurrentCharsIdx);
                NamingScreen_PaletteGlowEffect(ref data.PlttGlowEffectAngle);
                break;
            case AppStateWaitFadeOut:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    return 1;
                }
                break;
        }
        Sprite.SpriteList_Update(data.SpriteList);
        return 0;
    }

    // OFFSET 0x02086D38
    private static int NamingScreen_ProcessInputs(NamingScreenData data, int state)
    {
        NamingScreenCursor cursor = data.KeyboardCursor;
        NamingScreen_ProcessDirectionInputs(data);
        int keys = gSystem.PressedKeys;
        if ((keys & KeySelect) != 0)
        {
            if (Sprite.Sprite_GetDrawFlag(data.UiSprites[SpriteCursor]!) == 0)
            {
                Sprite.Sprite_SetDrawFlag(data.UiSprites[SpriteCursor]!, 1);
                return state;
            }
            if (data.Type != TypeFriendCode)
            {
                data.State = StateLoadGraphics;
                data.CurrentCharsIdx++;
                if (data.CurrentCharsIdx >= 3)
                {
                    data.CurrentCharsIdx = 0;
                }
                data.SpritesToUpdate[data.CurrentCharsIdx]++;
                NamingScreen_LoadKeyboardLayout(data.KeyboardChars, data.CurrentCharsIdx);
                SoundPlayback.Sound_PlayEffect(SeqPageChange);
                cursor.HasCharacterBeenEntered = 1;
            }
            data.SpritesToUpdate[data.CurrentCharsIdx]++;
            NamingScreen_LoadKeyboardLayout(data.KeyboardChars, data.CurrentCharsIdx);
            SoundPlayback.Sound_PlayEffect(SeqPageChange);
        }
        else if ((keys & KeyA) != 0)
        {
            state = NamingScreen_ProcessCharacterInput(data, data.KeyboardChars[cursor.Y * NamingScreenData.Columns + cursor.X], 1);
            cursor.HasCharacterBeenEntered = 1;
        }
        else if (data.IsTouchInput == 1)
        {
            state = NamingScreen_ProcessCharacterInput(data, data.KeyboardChars[cursor.Y * NamingScreenData.Columns + cursor.X], 0);
            cursor.HasCharacterBeenEntered = 0;
        }
        else if ((keys & KeyB) != 0)
        {
            state = NamingScreen_ProcessCharacterInput(data, ButtonBack, 1);
        }
        else if ((keys & KeyR) != 0)
        {
            state = NamingScreen_ProcessCharacterInput(data, ButtonPageJapanese2, 1);
        }
        return state;
    }

    // OFFSET 0x02086E6C
    private static void NamingScreen_UseDefaultName(NamingScreenData data, NamingScreenArgs args)
    {
        if (data.Type == TypePlayer)
        {
            GameString? name = null;
            if (data.PlayerGenderOrMonSpecies == 0)
            {
                name = MessageLoader.MessageLoader_GetNewString(data.GenericNamesMsgLoader!, (uint)(MathUtil.LCRNG_Next() % 0x12));
            }
            else if (data.PlayerGenderOrMonSpecies == 1)
            {
                name = MessageLoader.MessageLoader_GetNewString(data.GenericNamesMsgLoader!, (uint)(MathUtil.LCRNG_Next() % 0x12 + 0x12));
            }
            GameString.String_Copy(args.TextInputStr, name);
            GameString.String_Free(name);
            GameString.String_ToChars(args.TextInputStr, args.NameInputRaw, 0xA);
        }
        else if (data.Type == TypeRival)
        {
            GameString? name = MessageLoader.MessageLoader_GetNewString(data.GenericNamesMsgLoader!, (uint)(MathUtil.LCRNG_Next() % 2 + 0x58));
            GameString.String_Copy(args.TextInputStr, name);
            GameString.String_Free(name);
            GameString.String_ToChars(args.TextInputStr, args.NameInputRaw, 0xA);
        }
        else
        {
            args.ReturnCode = 1;
        }
    }

    // OFFSET 0x02086F14
    private static bool NamingScreen_IsRawStringAllSpaces(ushort[] rawChars)
    {
        bool allSpaces = true;
        for (int i = 0; rawChars[i] != CharEOS; i++)
        {
            if (rawChars[i] != CharWideSpace)
            {
                allSpaces = false;
            }
        }
        return allSpaces;
    }

    // OFFSET 0x02086F3C
    public static int NamingScreen_Exit(ApplicationManager appMan, ref int state)
    {
        NamingScreenData data = (NamingScreenData)ApplicationManager.ApplicationManager_Data(appMan)!;
        NamingScreenArgs args = (NamingScreenArgs)ApplicationManager.ApplicationManager_Args(appMan)!;
        data.EntryBuf[data.TextCursorPos] = CharEOS;
        if (data.Type == TypePokemon)
        {
            Pokemon mon = Pokemon.Pokemon_New(HeapID);
            Pokemon.Pokemon_InitWith(mon, data.PlayerGenderOrMonSpecies, SpeciesLevel, 0xA, 0xA, 0xA, 0xA, 0xA);
        }
        if (data.TextCursorPos == 0
            || CharCode.CharCode_Compare(data.EntryBuf, 0, data.EntryBufBak, 0) == 0
            || NamingScreen_IsRawStringAllSpaces(data.EntryBuf))
        {
            NamingScreen_UseDefaultName(data, args);
        }
        else
        {
            CharCode.CharCode_Copy(data.EntryBufBak, 0, data.EntryBuf, 0);
            CharCode.CharCode_Copy(args.NameInputRaw, 0, data.EntryBuf, 0);
            GameString.String_CopyChars(args.TextInputStr, data.EntryBuf);
        }
        GameString.String_Free(data.GroupTextString);
        for (int i = 0; i < NamingScreenData.FollowTaskCount; i++)
        {
            SysTaskExtensions.SysTask_FinishAndFreeParam(data.SpriteFollowTasks[i]!);
        }
        SpriteTransfer.SpriteTransfer_ResetCharTransfer(data.SpriteResources[0]);
        SpriteTransfer.SpriteTransfer_ResetCharTransfer(data.SpriteResources[4]);
        SpriteTransfer.SpriteTransfer_ResetPlttTransfer(data.SpriteResources[1]);
        SpriteTransfer.SpriteTransfer_ResetPlttTransfer(data.SpriteResources[5]);
        for (int i = 0; i < 4; i++)
        {
            SpriteResourceCollection.SpriteResourceCollection_Delete(data.SpriteResourceCollections[i]);
        }
        Sprite.SpriteList_Delete(data.SpriteList);
        RenderOam.RenderOam_Free();
        data.CharDataAlloc = null;
        if (data.Type == TypePokemon)
        {
            data.MonIconRawCharData = null;
            data.MonIconRawPlttData = null;
        }
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 7);
        CharTransfer.CharTransfer_Free();
        PlttTransfer.PlttTransfer_Free();
        NamingScreen_FreeBgs(data.BgConfig!, data.Windows);
        Font.Font_UseLazyGlyphAccess(0);
        Video.SetDisplayControl(DisplayMain, Video.GetDisplayControl(DisplayMain) & WindowsMask);
        Font.Font_Free(2);
        if (data.BattleMessageString != null)
        {
            GameString.String_Free(data.BattleMessageString);
        }
        GameString.String_Free(data.PromptString);
        GameString.String_Free(data.LastCharString);
        MessageLoader.MessageLoader_Free(data.BattleStringsMsgLoader);
        MessageLoader.MessageLoader_Free(data.GenericNamesMsgLoader);
        MessageLoader.MessageLoader_Free(data.NamingScreenMsgLoader);
        StringTemplate.StringTemplate_Free(data.StrTemplate!);
        ApplicationManager.ApplicationManager_FreeData(appMan);
        GameSystem.SetVBlankCallback(null, null);
        Heap.Heap_Destroy(HeapID);
        gSystem.WhichScreenIs3D = 0;
        GXLayers.GXLayers_SwapDisplay();
        return 1;
    }

    // OFFSET 0x02087190
    private static void NamingScreen_VBlankCallback(object? unused)
    {
        VramTransfer.VramTransfer_Process();
        RenderOam.RenderOam_Transfer();
    }

    // OFFSET 0x020871B0
    private static void NamingScreen_CopyParamsFromArgs(NamingScreenData data, NamingScreenArgs args)
    {
        data.Type = args.Type;
        data.PlayerGenderOrMonSpecies = args.PlayerGenderOrMonSpecies;
        data.MonForm = args.MonForm;
        data.MaxChars = args.MaxChars;
        data.MonGender = args.MonGender;
        data.Options = args.Options;
    }

    // OFFSET 0x020871CC
    private static void NamingScreen_InitGraphicsBanks()
    {
        GXLayers.GXLayers_SetBanks(RomStructs.Arm9Banks(BanksAddress));
    }

    // OFFSET 0x020871EC
    private static void NamingScreen_InitBgs(BgConfig bgConfig)
    {
        Bg.SetAllGraphicsModes(RomStructs.Arm9GraphicsModes(GraphicsModesAddress));
        Bg.Bg_InitFromTemplate(bgConfig, 0, RomStructs.Arm9BgTemplate(Bg0TemplateAddress), 0);
        Bg.Bg_ClearTilemap(bgConfig, 0);
        Bg.Bg_InitFromTemplate(bgConfig, 1, RomStructs.Arm9BgTemplate(Bg1TemplateAddress), 0);
        Bg.Bg_ClearTilemap(bgConfig, 1);
        Bg.Bg_InitFromTemplate(bgConfig, 2, RomStructs.Arm9BgTemplate(Bg2TemplateAddress), 0);
        Bg.Bg_ClearTilemap(bgConfig, 2);
        Bg.Bg_InitFromTemplate(bgConfig, 4, RomStructs.Arm9BgTemplate(SubBg0TemplateAddress), 0);
        Bg.Bg_ClearTilemap(bgConfig, 4);
        NamingScreen_ToggleEngineLayers(0);
        Bg.Bg_ClearTilesRange(0, 0x20, 0, HeapID);
        Bg.Bg_ClearTilesRange(4, 0x20, 0, HeapID);
        Video.SetDisplayControl(DisplayMain, (Video.GetDisplayControl(DisplayMain) & WindowsMask) | Window0Enable);
        Video.SetWindowInside(DisplayMain, 0, (byte)((Video.GetWindowInside(DisplayMain, 0) & ~0x3F) | 0x14 | 0x20));
        Video.SetWindowOutside(DisplayMain, (byte)((Video.GetWindowOutside(DisplayMain) & ~0x3F) | 0x1F | 0x20));
        Video.SetWindowHorizontal(DisplayMain, 0, 0, 0xFF);
        Video.SetWindowVertical(DisplayMain, 0, 0, 0x40);
        Video.SetBlendControl(DisplaySub, 0);
    }

    // OFFSET 0x0208732C
    private static void NamingScreen_ToggleEngineLayers(int enable)
    {
        GXLayers.GXLayers_EngineAToggleLayers(0x1, (byte)enable);
        GXLayers.GXLayers_EngineAToggleLayers(0x2, (byte)enable);
        GXLayers.GXLayers_EngineAToggleLayers(0x4, (byte)enable);
        GXLayers.GXLayers_EngineAToggleLayers(0x8, 0);
        GXLayers.GXLayers_EngineAToggleLayers(0x10, (byte)enable);
        GXLayers.GXLayers_EngineBToggleLayers(0x1, (byte)enable);
        GXLayers.GXLayers_EngineBToggleLayers(0x2, 0);
        GXLayers.GXLayers_EngineBToggleLayers(0x10, 0);
    }

    // OFFSET 0x0208737C
    private static void NamingScreen_InitCursorsAndChars(NamingScreenData data, ApplicationManager appMan)
    {
        NamingScreenArgs args = (NamingScreenArgs)ApplicationManager.ApplicationManager_Args(appMan)!;
        data.State = StateProcessInputs;
        NamingScreen_InitializeCharsPosition(data.CharsPosition, 0);
        int layer = data.FreeCharsBgLayer;
        Bg.Bg_SetOffset(data.BgConfig!, (byte)layer, 0, data.CharsPosition[layer].X);
        Bg.Bg_SetOffset(data.BgConfig!, (byte)layer, 3, data.CharsPosition[layer].Y);
        int other = data.FreeCharsBgLayer ^ 1;
        Bg.Bg_SetOffset(data.BgConfig!, (byte)other, 0, data.CharsPosition[other].X);
        Bg.Bg_SetOffset(data.BgConfig!, (byte)other, 3, data.CharsPosition[other].Y);
        data.EntryBufBak[0] = CharEOS;
        if (args.TextInputStr != null)
        {
            GameString.String_ToChars(args.TextInputStr, data.EntryBufBak, 0x20);
        }
        Array.Fill(data.EntryBuf, CharSpace);
        if (data.Type == TypePokemon)
        {
            Pokemon mon = Pokemon.Pokemon_New(HeapID);
            Pokemon.Pokemon_InitWith(mon, data.PlayerGenderOrMonSpecies, SpeciesLevel, 0xA, 0xA, 0xA, 0xA, 0xA);
            StringTemplate.StringTemplate_SetSpeciesName(data.StrTemplate!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
        }
        if (args.BattleMsgID != 0)
        {
            data.PrintedFromBattleGMM = 1;
        }
        uint promptIndex = RomData.Arm9U32(PromptTextIndicesAddress + (uint)(data.Type * 4), 1)[0];
        data.PromptString = MessageUtil.MessageUtil_ExpandedString(data.StrTemplate!, data.NamingScreenMsgLoader!, promptIndex, HeapID);
        data.LastCharString = MessageUtil.MessageUtil_ExpandedString(data.StrTemplate!, data.NamingScreenMsgLoader!, 8, HeapID);
        data.GroupTextString = MessageLoader.MessageLoader_GetNewString(data.NamingScreenMsgLoader!, 7);
        data.TextCursorPos = (ushort)CharCode.CharCode_Length(data.EntryBufBak, 0);
        data.KeyboardCursor.X = 0;
        data.KeyboardCursor.Y = 1;
        data.KeyboardCursor.PrevX = -1;
        data.KeyboardCursor.PrevY = -1;
        data.KeyboardCursor.HasCharacterBeenEntered = 0;
        data.KeyboardCursor.IgnoreInput = 0;
        data.Unused484 = -1;
        data.Unused488 = 0;
        data.Unused48C = 0;
        data.EntryBuf[data.MaxChars] = CharEOS;
        for (int i = 0; i < NamingScreenData.FollowTaskCount; i++)
        {
            data.SpritesToUpdate[i] = 0;
        }
        if (data.Type == TypeFriendCode)
        {
            data.SpritesToUpdate[3] = 1;
        }
        else
        {
            data.SpritesToUpdate[0] = 1;
        }
    }

    // OFFSET 0x02087544
    private static void NamingScreen_PrepareBattleMessage(NamingScreenData data, ApplicationManager appMan)
    {
        NamingScreenArgs args = (NamingScreenArgs)ApplicationManager.ApplicationManager_Args(appMan)!;
        if (args.BattleMsgID == 0)
        {
            return;
        }
        GameString str = GameString.String_Init(200, HeapID);
        data.BattleMessageString = null;
        uint currentBox = sub_0207999C(args.PcBoxes);
        uint firstEmptyBox = sub_020799A0(args.PcBoxes);
        StringTemplate.StringTemplate_SetPCBoxName(data.StrTemplate!, 1, args.PcBoxes!, currentBox);
        if (currentBox != firstEmptyBox)
        {
            StringTemplate.StringTemplate_SetPCBoxName(data.StrTemplate!, 2, args.PcBoxes!, firstEmptyBox);
            args.BattleMsgID += 2;
        }
        else
        {
            StringTemplate.StringTemplate_SetPCBoxName(data.StrTemplate!, 2, args.PcBoxes!, currentBox);
        }
        if (data.TextCursorPos == 0 || NamingScreen_IsRawStringAllSpaces(data.EntryBuf))
        {
            Pokemon mon = Pokemon.Pokemon_New(HeapID);
            Pokemon.Pokemon_InitWith(mon, data.PlayerGenderOrMonSpecies, 1, 0, 0, 0, 0, 0);
            StringTemplate.StringTemplate_SetSpeciesName(data.StrTemplate!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
        }
        else
        {
            data.EntryBuf[data.TextCursorPos] = CharEOS;
            GameString.String_CopyChars(str, data.EntryBuf);
            StringTemplate.StringTemplate_SetString(data.StrTemplate!, 0, str, 0, 0, 0);
        }
        data.BattleMessageString = MessageUtil.MessageUtil_ExpandedString(data.StrTemplate!, data.BattleStringsMsgLoader!, (uint)args.BattleMsgID, HeapID);
        data.PrintedFromBattleGMM = 1;
        GameString.String_Free(str);
    }

    // OFFSET 0x0208765C
    private static void NamingScreen_FreeBgs(BgConfig bgConfig, Window[] windows)
    {
        for (int i = 0; i < NamingScreenData.WindowCount; i++)
        {
            Bg.Window_Remove(windows[i]);
        }
        Bg.Bg_FreeTilemapBuffer(bgConfig, 4);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 2);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 1);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 0);
    }
}
