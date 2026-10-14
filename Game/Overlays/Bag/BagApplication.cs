using System.Buffers.Binary;
using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public static partial class BagApplication
{
    public const int Overlay = 84;
    private const int HeapID = 6;
    private const uint HeapSize = 0x30000;
    private const int BagGraphicsArchive = 0xD;
    private const int MessageArchive = 0x1A;
    private const int BagTextBank = 7;
    private const int ItemNamesBank = 0x188;
    private const int MoveNamesBank = 0x287;
    private const uint AllLayersMask = 0xFFFFE0FF;
    private const int DisplayMain = 0;
    private const int DisplaySub = 1;
    private const int KeyA = 0x1;
    private const int KeyB = 0x2;
    private const int KeySelect = 0x4;
    private const int KeyRight = 0x10;
    private const int KeyLeft = 0x20;
    private const int KeyUp = 0x40;
    private const int KeyDown = 0x80;
    private const int SeqConfirm = 0x5DC;
    private const int SeqPocketSwitch = 0x6CA;
    private const int SeqCursor0 = 0x6CC;
    private const uint VramBanksAddress = 0x0224100C;
    private const uint GraphicsModesAddress = 0x02240EC0;
    private const uint BgTemplateLayer0 = 0x02240EE4;
    private const uint BgTemplateLayer1 = 0x02240F00;
    private const uint BgTemplateLayer2 = 0x02240F38;
    private const uint BgTemplateLayer3 = 0x02240F8C;
    private const uint BgTemplateLayer4 = 0x02240F1C;
    private const uint BgTemplateLayer5 = 0x02240F54;
    private const uint BgTemplateLayer7 = 0x02240F70;
    private const uint ItemListTemplateAddress = 0x02240FA8;
    private const uint PocketSizes = 0x02241118;

    public const int ModeField = 0;
    public const int ModeGiveToMon = 1;
    public const int ModeSellItems = 2;
    public const int ModeGardening = 3;
    public const int ModePoffins = 4;
    public const int ModeUnk5 = 5;

    private const int ExitCodeDone = 5;
    private const int PocketTMs = 3;
    private const int PocketBerries = 4;
    private const int PocketKeyItems = 7;

    private const int StateWaitInitialScreenFade = 0;
    private const int StateWaitSelectItemGeneral = 1;
    private const int StateSwitchPocket = 2;
    private const int StateMovingItem = 3;
    private const int StateWaitSelectItemAction = 4;
    private const int StateSelectItemTrashCount = 7;
    private const int StateShowConfirmTrashMsg = 8;
    private const int StateConfirmTrash = 9;
    private const int StateResolveTrash = 10;
    private const int StateShowingTrashedMsg = 11;
    private const int StateShowingItemUseMsg = 12;
    private const int StateRunItemUseTask = 13;
    private const int StateSelectItemToGive = 14;
    private const int StateShowingItemCantBeHeldMsg = 15;
    private const int StateSelectItemToSell = 16;
    private const int StateWaitSellHowManyMsg = 17;
    private const int StateSelectItemSellCount = 18;
    private const int StatePrintConfirmSaleMsg = 19;
    private const int StateConfirmSale = 20;
    private const int StateResolveSale = 21;
    private const int StateShowingItemsSoldMsg = 22;
    private const int StateSelectItemGardening = 23;
    private const int StateExit = 24;

    private const int SpriteBag = 0;
    private const int SpritePocketHighlight = 1;
    private const int SpriteItemHighlight = 4;

    private const int WinItemList = 0;
    private const int WinItemDescription = 1;

    private static BagApplicationPocket CurrentPocket(BagController controller)
    {
        BagContext bagCtx = controller.BagCtx!;
        return bagCtx.AccessiblePockets[bagCtx.CurrPocketIdx];
    }

    private static ushort PocketItem(BagApplicationPocket pocket, int slot)
    {
        return pocket.Items!.U16(slot * 4);
    }

    private static ushort PocketQuantity(BagApplicationPocket pocket, int slot)
    {
        return pocket.Items!.U16(slot * 4 + 2);
    }

    private static int PocketSize(int pocketType)
    {
        return RomData.Overlay(Overlay, (uint)(PocketSizes + pocketType), 1)[0];
    }

    // OFFSET 0x0223B5A0
    public static int BagApplication_Init(ApplicationManager appMan, ref int state)
    {
        GameSystem.SetVBlankCallback(null, null);
        GameSystem.DisableHBlank();
        GXLayers.GXLayers_DisableEngineALayers();
        GXLayers.GXLayers_DisableEngineBLayers();
        Video.SetDisplayControl(DisplayMain, Video.GetDisplayControl(DisplayMain) & AllLayersMask);
        Video.SetDisplayControl(DisplaySub, Video.GetDisplayControl(DisplaySub) & AllLayersMask);
        Video.SetBlendControl(DisplayMain, 0);
        Video.SetBlendControl(DisplaySub, 0);
        Heap.Heap_Create(3, HeapID, HeapSize);
        BagController controller = new();
        appMan.Data = controller;
        controller.BagCtx = (BagContext)ApplicationManager.ApplicationManager_Args(appMan)!;
        RetrieveSaveData(controller);
        controller.BgConfig = Bg.BgConfig_New(HeapID);
        controller.TrainerGender = (byte)TrainerInfo.TrainerInfo_Gender(controller.TrainerInfo!);
        ScreenFadeControl.StartScreenFade(0, 3, 3, 0, 6, 1, HeapID);
        GameSystem.SetAutorepeat(3, 8);
        RestoreCursorPosition(controller);
        SetupTextLoaders(controller);
        InitItemNameBuffers(controller);
        CountAccessiblePockets(controller);
        CalcPocketSelectorIconsPos(controller);
        SetVRAMBanks();
        SetupBGLayers(controller.BgConfig);
        LoadGraphics(controller);
        DrawTouchScreenButtons(controller);
        TouchPad.EnableTouchPad();
        TouchPad.InitializeTouchPad(4);
        BagUI_CreateWindows(controller);
        RenderText.RenderControlFlags_SetSpeedUpOnTouch(1);
        BagUI_LoadPocketNames(controller);
        BagUI_ClearPocketNameBox(controller);
        BagUI_PrintPocketNames(controller);
        BagUI_DrawPocketSelectorIcons(controller);
        BagUI_LoadItemActionStrings(controller);
        BagUI_LoadItemCountStrings(controller);
        LoadCurrentPocketItemNames(controller);
        BagApplicationPocket pocket = CurrentPocket(controller);
        LimitItemListScroll(ref pocket.CursorScroll, ref pocket.CursorPos, pocket.ListEntryCount);
        RestrictItemListCursor(ref pocket.CursorScroll, ref pocket.CursorPos, pocket.ListEntryCount, 9);
        BagUI_InitSprites(controller);
        CreateItemListMenu(controller, pocket.CursorScroll, pocket.CursorPos);
        if ((byte)(controller.BagCtx.Mode - ModePoffins) <= 1)
        {
            BagUI_DrawPoffinCountMsgBox(controller);
        }
        GameSystem.SetVBlankCallback(BagApplication_VBlankCallback, controller);
        Sound.Sound_SetSceneAndPlayBGM(0x33, 0, 0);
        NetworkIcon.NetworkIcon_InitIfConnected();
        return 1;
    }

    private static int StateAfterPocketSelect(BagController controller)
    {
        switch (controller.BagCtx!.Mode)
        {
            case ModeSellItems:
                return StateSelectItemToSell;
            case ModeGiveToMon:
                return StateSelectItemToGive;
            case ModeGardening:
                return StateSelectItemGardening;
        }
        return StateWaitSelectItemGeneral;
    }

    // OFFSET 0x0223B76C
    public static int BagApplication_Main(ApplicationManager appMan, ref int state)
    {
        BagController controller = (BagController)ApplicationManager.ApplicationManager_Data(appMan)!;
        switch (state)
        {
            case StateWaitInitialScreenFade:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    state = StateAfterPocketSelect(controller);
                }
                break;
            case StateWaitSelectItemGeneral:
                state = ProcessItemListInput_Normal(controller);
                break;
            case StateSwitchPocket:
                StepPocketSelectorArrowsAnim(controller);
                if (DoPocketSwitch(controller))
                {
                    state = StateAfterPocketSelect(controller);
                }
                break;
            case StateMovingItem:
                if (ProcessItemListInput_MovingItem(controller))
                {
                    state = StateWaitSelectItemGeneral;
                }
                break;
            case StateWaitSelectItemAction:
                state = ProcessMenuInput_SelectAction(controller);
                break;
            case StateSelectItemTrashCount:
                state = ProcessItemCountInput_TrashCount(controller);
                break;
            case StateShowConfirmTrashMsg:
                state = ShowConfirmTrashMenu(controller);
                break;
            case StateConfirmTrash:
                state = ProcessMenuInput_ConfirmTrash(controller);
                break;
            case StateResolveTrash:
                state = ResolveTrash(controller);
                break;
            case StateShowingTrashedMsg:
                state = CheckPlayerDismissedTrashedMsg(controller);
                break;
            case StateShowingItemUseMsg:
                state = CheckPlayerDismissedItemUseMsg(controller);
                break;
            case StateRunItemUseTask:
                state = RunItemUseCallback(controller);
                break;
            case StateSelectItemToGive:
                state = ProcessItemListInput_GiveToMon(controller);
                break;
            case StateShowingItemCantBeHeldMsg:
                state = CheckPlayerDismissedItemCantBeHeldMsg(controller);
                break;
            case StateSelectItemToSell:
                state = ProcessItemListInput_SellItems(controller);
                break;
            case StateWaitSellHowManyMsg:
                state = ShowSaleCountValueWindow(controller);
                break;
            case StateSelectItemSellCount:
                state = ProcessItemCountInput_SellCount(controller);
                break;
            case StatePrintConfirmSaleMsg:
                state = WaitConfirmSaleMsgPrinted(controller);
                break;
            case StateConfirmSale:
                state = ProcessMenuInput_ConfirmSale(controller);
                break;
            case StateResolveSale:
                state = ResolveSale(controller);
                break;
            case StateShowingItemsSoldMsg:
                state = CheckPlayerDismissedItemsSoldMsg(controller);
                break;
            case StateSelectItemGardening:
                state = ProcessItemListInput_Gardening(controller);
                break;
            case StateExit:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    return 1;
                }
                break;
        }
        StepDialButtonPressedAnim(controller);
        BagUI_TickBtnShockwaveAnim(controller);
        BagUI_TickSpriteAnimations(controller);
        SpriteSystem.SpriteSystem_DrawSprites(controller.SpriteMan);
        return 0;
    }

    // OFFSET 0x0223B900
    public static int BagApplication_Exit(ApplicationManager appMan, ref int state)
    {
        BagController controller = (BagController)ApplicationManager.ApplicationManager_Data(appMan)!;
        BagUI_FreeSprites(controller);
        FreeItemListMenu(controller);
        SaveCursorPosition(controller);
        BagUI_DeleteWindows(controller.Windows);
        FreeBGLayers(controller.BgConfig!);
        TouchPad.DisableTouchPad();
        VramTransfer.VramTransfer_Free();
        BagUI_FreeItemActionStrings(controller);
        BagUI_FreePocketNames(controller);
        BagUI_FreeItemCountStrings(controller);
        FreeItemNameBuffers(controller);
        GameString.String_Free(controller.StringBuffer);
        MessageLoader.MessageLoader_Free(controller.MoveNamesLoader);
        MessageLoader.MessageLoader_Free(controller.ItemNamesLoader);
        MessageLoader.MessageLoader_Free(controller.BagStringsLoader);
        FontSpecialCharsContext.FontSpecialChars_Free(controller.SpecialChars);
        StringTemplate.StringTemplate_Free(controller.StrTemplate!);
        NARC.NARC_dtor(controller.BagGraphicsNARC!);
        ApplicationManager.ApplicationManager_FreeData(appMan);
        GameSystem.SetVBlankCallback(null, null);
        Heap.Heap_Destroy(HeapID);
        GameSystem.SetAutorepeat(4, 8);
        return 1;
    }

    // OFFSET 0x0223B9AC
    private static void RetrieveSaveData(BagController controller)
    {
        SaveData saveData = controller.BagCtx!.SaveData!;
        controller.Bag = BagItems.SaveData_GetBag(saveData);
        controller.TrainerInfo = PlayerSave.SaveData_GetTrainerInfo(saveData);
        controller.Options = PlayerSave.SaveData_GetOptions(saveData);
    }

    // OFFSET 0x0223B9E4
    private static SpecialEncounter BagApplication_GetSpecialEncounters(BagController controller)
    {
        return SpecialEncounter.SaveData_GetSpecialEncounters(controller.BagCtx!.SaveData!);
    }

    // OFFSET 0x0223B9F4
    public static void BagApplication_SetRepelSteps(BagController controller, byte steps)
    {
        SpecialEncounter.SpecialEncounter_GetRepelSteps(BagApplication_GetSpecialEncounters(controller)) = steps;
    }

    // OFFSET 0x0223BA04
    public static void BagApplication_SetFluteFactor(BagController controller, byte flute)
    {
        SpecialEncounter.SpecialEncounter_SetFluteFactor(BagApplication_GetSpecialEncounters(controller), flute);
    }

    // OFFSET 0x0223BA14
    private static void BagApplication_VBlankCallback(object? data)
    {
        BagController controller = (BagController)data!;
        Bg.Bg_RunScheduledUpdates(controller.BgConfig!);
        VramTransfer.VramTransfer_Process();
        SpriteSystem.SpriteSystem_TransferOam();
    }

    // OFFSET 0x0223BA3C
    private static void SetVRAMBanks()
    {
        GXLayers.GXLayers_SetBanks(RomStructs.Banks(RomData.Overlay(Overlay, VramBanksAddress, 0x28), 0));
    }

    private static BgTemplate OverlayBgTemplate(uint address)
    {
        return RomStructs.BgTemplate(RomData.Overlay(Overlay, address, 0x1C), 0);
    }

    // OFFSET 0x0223BA5C
    private static void SetupBGLayers(BgConfig bgConfig)
    {
        Bg.SetAllGraphicsModes(RomStructs.GraphicsModes(RomData.Overlay(Overlay, GraphicsModesAddress, 0x10), 0));
        Bg.Bg_InitFromTemplate(bgConfig, 0, OverlayBgTemplate(BgTemplateLayer0), 0);
        Bg.Bg_ClearTilemap(bgConfig, 0);
        Bg.Bg_InitFromTemplate(bgConfig, 1, OverlayBgTemplate(BgTemplateLayer1), 0);
        Bg.Bg_InitFromTemplate(bgConfig, 2, OverlayBgTemplate(BgTemplateLayer2), 0);
        Bg.Bg_ClearTilemap(bgConfig, 2);
        Bg.Bg_InitFromTemplate(bgConfig, 3, OverlayBgTemplate(BgTemplateLayer3), 0);
        Bg.Bg_InitFromTemplate(bgConfig, 4, OverlayBgTemplate(BgTemplateLayer4), 0);
        Bg.Bg_ClearTilemap(bgConfig, 4);
        Bg.Bg_InitFromTemplate(bgConfig, 5, OverlayBgTemplate(BgTemplateLayer5), 0);
        Bg.Bg_InitFromTemplate(bgConfig, 7, OverlayBgTemplate(BgTemplateLayer7), 1);
        Bg.Bg_ScheduleAffineRotationCenter(bgConfig, 7, 9, 0x80);
        Bg.Bg_ScheduleAffineRotationCenter(bgConfig, 7, 0xC, 0x50);
        Bg.Bg_ClearTilesRange(0, 0x20, 0, HeapID);
        Bg.Bg_ClearTilesRange(4, 0x20, 0, HeapID);
    }

    // OFFSET 0x0223BBC4
    private static void FreeBGLayers(BgConfig bgConfig)
    {
        GXLayers.GXLayers_EngineAToggleLayers(0x1F, 0);
        GXLayers.GXLayers_EngineBToggleLayers(0x1B, 0);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 7);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 5);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 4);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 3);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 2);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 1);
        Bg.Bg_FreeTilemapBuffer(bgConfig, 0);
    }

    // OFFSET 0x0223BC1C
    private static void LoadGraphics(BagController controller)
    {
        NARC narc = NARC.NARC_ctor(BagGraphicsArchive, HeapID);
        controller.BagGraphicsNARC = narc;
        BgConfig bgConfig = controller.BgConfig!;
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0xB, bgConfig, 1, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0xE, bgConfig, 1, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0xD, bgConfig, 3, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 0xC, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 0x16, 0, 0x1A0, 0x20, HeapID);
        Font.Font_LoadScreenIndicatorsPalette(0, 0x160, HeapID);
        RenderWindow.LoadStandardWindowGraphics(bgConfig, 0, 0x3F7, 0xE, 0, HeapID);
        RenderWindow.LoadMessageBoxGraphics(bgConfig, 0, 0x3D9, 0xC, (byte)Options.Options_Frame(controller.Options!), HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0xF, bgConfig, 5, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 0x10, 4, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x11, bgConfig, 5, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x20, bgConfig, 7, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 0x21, bgConfig, 7, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0x13, bgConfig, 4, 0, 0, 0, HeapID);
        Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 0x12, 4, 0x40, 0x40, HeapID);
    }

    // OFFSET 0x0223BDB4
    private static void SetupTextLoaders(BagController controller)
    {
        controller.BagStringsLoader = MessageLoader.MessageLoader_Init(0, MessageArchive, BagTextBank, HeapID);
        controller.SpecialChars = FontSpecialCharsContext.FontSpecialChars_Init(1, 2, 0, HeapID);
        controller.StrTemplate = StringTemplate.StringTemplate_Default(HeapID);
        controller.ItemNamesLoader = MessageLoader.MessageLoader_Init(0, MessageArchive, ItemNamesBank, HeapID);
        controller.MoveNamesLoader = MessageLoader.MessageLoader_Init(0, MessageArchive, MoveNamesBank, HeapID);
        controller.StringBuffer = GameString.String_Init(0x100, HeapID);
    }

    // OFFSET 0x0223BE24
    private static void CountAccessiblePockets(BagController controller)
    {
        controller.NumPockets = 0;
        for (int i = 0; i < BagContext.PocketMax; i++)
        {
            if (controller.BagCtx!.AccessiblePockets[i].Items != null)
            {
                controller.NumPockets++;
            }
        }
    }

    // OFFSET 0x0223BE5C
    public static ushort BagApplication_GetItemSlotParam(BagController controller, int slot, int param)
    {
        BagApplicationPocket pocket = CurrentPocket(controller);
        if (param == 0)
        {
            return PocketItem(pocket, slot);
        }
        return PocketQuantity(pocket, slot);
    }

    // OFFSET 0x0223BE84
    public static void BagApplication_GetItemName(MessageLoader loader, GameString dest, ushort item, int heapID)
    {
        MessageLoader.MessageLoader_GetString(loader, item, dest);
    }

    // OFFSET 0x0223BE94
    public static void BagApplication_GetTMHMMoveName(MessageLoader loader, GameString dest, ushort item, int heapID)
    {
        MessageLoader.MessageLoader_GetString(loader, Item.Item_MoveForTMHM(item), dest);
    }

    // OFFSET 0x0223BEAC
    private static void RestoreCursorPosition(BagController controller)
    {
        BagContext bagCtx = controller.BagCtx!;
        bagCtx.CurrPocketIdx = 0;
        if (bagCtx.BagCursor == null)
        {
            for (int i = 0; i < BagContext.PocketMax; i++)
            {
                BagApplicationPocket pocket = bagCtx.AccessiblePockets[i];
                if (pocket.Items == null)
                {
                    return;
                }
                pocket.CursorPos = 1;
                pocket.CursorScroll = 0;
            }
            return;
        }
        for (int i = 0; i < BagContext.PocketMax; i++)
        {
            BagApplicationPocket pocket = bagCtx.AccessiblePockets[i];
            if (pocket.Items == null)
            {
                break;
            }
            BagCursor.BagCursor_GetFieldPocketPosition(bagCtx.BagCursor, pocket.PocketType, out byte index, out byte scroll);
            if (index == 0)
            {
                index = 1;
            }
            pocket.CursorPos = index;
            pocket.CursorScroll = scroll;
        }
        ushort fieldPocket = BagCursor.BagCursor_GetFieldPocket(bagCtx.BagCursor);
        for (int i = 0; i < BagContext.PocketMax; i++)
        {
            BagApplicationPocket pocket = bagCtx.AccessiblePockets[i];
            if (pocket.Items == null)
            {
                return;
            }
            if (fieldPocket == pocket.PocketType)
            {
                bagCtx.CurrPocketIdx = (byte)i;
            }
        }
    }

    // OFFSET 0x0223BF68
    private static void SaveCursorPosition(BagController controller)
    {
        BagContext bagCtx = controller.BagCtx!;
        if (bagCtx.BagCursor == null)
        {
            return;
        }
        for (int i = 0; i < BagContext.PocketMax; i++)
        {
            BagApplicationPocket pocket = bagCtx.AccessiblePockets[i];
            if (pocket.Items == null)
            {
                break;
            }
            BagCursor.BagCursor_SetFieldPocketPosition(bagCtx.BagCursor, pocket.PocketType, (byte)pocket.CursorPos, (byte)pocket.CursorScroll);
        }
        BagCursor.BagCursor_SetFieldPocket(bagCtx.BagCursor, bagCtx.AccessiblePockets[bagCtx.CurrPocketIdx].PocketType);
    }

    // OFFSET 0x0223BFBC
    private static void LoadCurrentPocketItemNames(BagController controller)
    {
        BagApplicationPocket pocket = CurrentPocket(controller);
        int size = PocketSize(pocket.PocketType);
        controller.ItemListEntries = StringList.StringList_New((uint)(size + 3), HeapID);
        StringList.StringList_AddFromMessageBank(controller.ItemListEntries, controller.BagStringsLoader!, 0x20, Menu.Header);
        int count = 0;
        if (pocket.PocketType == PocketTMs)
        {
            for (; count < PocketSize(pocket.PocketType); count++)
            {
                if (PocketItem(pocket, count) == 0 || PocketQuantity(pocket, count) == 0)
                {
                    break;
                }
                BagApplication_GetTMHMMoveName(controller.MoveNamesLoader!, controller.ItemNames[count]!, PocketItem(pocket, count), HeapID);
                StringList.StringList_AddFromString(controller.ItemListEntries, controller.ItemNames[count], (uint)count);
            }
            StringList.StringList_AddFromMessageBank(controller.ItemListEntries, controller.BagStringsLoader!, 0x20, Menu.Cancel);
            StringList.StringList_AddFromMessageBank(controller.ItemListEntries, controller.BagStringsLoader!, 0x20, Menu.Header);
            pocket.ListEntryCount = (byte)(count + 3);
            return;
        }
        for (; count < PocketSize(pocket.PocketType); count++)
        {
            if (PocketItem(pocket, count) == 0 || PocketQuantity(pocket, count) == 0)
            {
                break;
            }
            BagApplication_GetItemName(controller.ItemNamesLoader!, controller.ItemNames[count]!, PocketItem(pocket, count), HeapID);
            StringList.StringList_AddFromString(controller.ItemListEntries, controller.ItemNames[count], (uint)count);
        }
        if (controller.BagCtx!.Mode == ModeUnk5)
        {
            StringList.StringList_AddFromMessageBank(controller.ItemListEntries, controller.BagStringsLoader!, 0x20, Menu.Header);
            pocket.ListEntryCount = (byte)(count + 2);
            return;
        }
        if (pocket.PocketType == PocketBerries)
        {
            StringList.StringList_AddFromMessageBank(controller.ItemListEntries, controller.BagStringsLoader!, 0x20, Menu.Cancel);
        }
        else
        {
            StringList.StringList_AddFromMessageBank(controller.ItemListEntries, controller.BagStringsLoader!, 0x29, Menu.Cancel);
        }
        StringList.StringList_AddFromMessageBank(controller.ItemListEntries, controller.BagStringsLoader!, 0x20, Menu.Header);
        pocket.ListEntryCount = (byte)(count + 3);
    }

    // OFFSET 0x0223C158
    private static void InitItemNameBuffers(BagController controller)
    {
        for (int i = 0; i < BagController.LargestPocketSize; i++)
        {
            controller.ItemNames[i] = GameString.String_Init(0x12, HeapID);
        }
    }

    // OFFSET 0x0223C178
    private static void FreeItemNameBuffers(BagController controller)
    {
        for (int i = 0; i < BagController.LargestPocketSize; i++)
        {
            GameString.String_Free(controller.ItemNames[i]);
        }
    }

    // OFFSET 0x0223C194
    private static void LimitItemListScroll(ref ushort scroll, ref ushort cursorPos, byte entryCount)
    {
        int lastIndex = (byte)(entryCount - 1);
        int visible = lastIndex > 8 ? 7 : (byte)(lastIndex - 1);
        if (scroll != 0 && scroll + visible > lastIndex - 1)
        {
            scroll = (ushort)(lastIndex - 1 - visible);
        }
        if (scroll + cursorPos >= lastIndex - 1)
        {
            cursorPos = (ushort)(lastIndex - 1 - scroll);
        }
    }

    // OFFSET 0x0223C1D0
    private static void RestrictItemListCursor(ref ushort scroll, ref ushort cursorPos, byte entryCount, int maxDisplay)
    {
        int lastIndex = (byte)(entryCount - 1);
        int visible = lastIndex > 8 ? 7 : (byte)(lastIndex - 1);
        int half = maxDisplay >> 1;
        if (cursorPos < half + 2)
        {
            return;
        }
        int steps = 0;
        if (cursorPos - half - 2 < 0)
        {
            return;
        }
        do
        {
            if (lastIndex - 1 == scroll + visible)
            {
                return;
            }
            steps = (byte)(steps + 1);
            cursorPos--;
            scroll++;
        }
        while (steps <= cursorPos - half - 2);
    }

    private static ListMenuTemplate ItemListTemplate()
    {
        ListMenuTemplate template = ListMenuTemplate.FromBytes(RomData.Overlay(Overlay, ItemListTemplateAddress, 0x20));
        template.CursorCallback = ItemListCursorCallback;
        template.PrintCallback = ItemListPrintCallback;
        return template;
    }

    // OFFSET 0x0223C224
    private static void CreateItemListMenu(BagController controller, ushort listPos, ushort cursorPos)
    {
        ListMenuTemplate template = ItemListTemplate();
        template.Choices = controller.ItemListEntries!;
        template.Window = controller.Windows[WinItemList];
        BagApplicationPocket pocket = CurrentPocket(controller);
        template.Count = pocket.ListEntryCount;
        template.Parent = controller;
        template.TextXOffset = (byte)((byte)(pocket.PocketType - PocketTMs) <= 1 ? 0x23 : 0);
        controller.ItemList = ListMenu.ListMenu_New(template, listPos, cursorPos, HeapID);
        Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinItemList]);
    }

    // OFFSET 0x0223C2AC
    private static void ItemListCursorCallback(ListMenu menu, uint index, byte onInit)
    {
        BagController controller = (BagController)ListMenu.ListMenu_GetAttribute(menu, 0x13)!;
        if (onInit != 1)
        {
            switch (controller.CursorSoundIdx)
            {
                case 0:
                    SoundPlayback.Sound_PlayEffect(SeqCursor0);
                    break;
                case 1:
                    SoundPlayback.Sound_PlayEffect(SeqCursor0 + 1);
                    break;
                default:
                    SoundPlayback.Sound_PlayEffect(SeqCursor0 + 2);
                    break;
            }
            controller.CursorSoundIdx = (byte)((controller.CursorSoundIdx + 1) % 3);
            if (controller.IsTouchingDial == 0 || !SpriteSystem.ManagedSprite_IsAnimated(controller.Sprites[SpriteBag]!))
            {
                SpriteSystem.ManagedSprite_SetAnimationFrame(controller.Sprites[SpriteBag]!, 0);
                SpriteSystem.ManagedSprite_SetAnim(controller.Sprites[SpriteBag]!, (uint)(CurrentPocket(controller).PocketType + 8));
            }
        }
        if (controller.IsMovingItem == 1)
        {
            return;
        }
        Bg.Window_FillTilemap(controller.Windows[WinItemDescription], 0);
        if (index != Menu.Cancel)
        {
            BagApplicationPocket pocket = CurrentPocket(controller);
            BagUI_PrintItemDescription(controller, PocketItem(pocket, (int)index));
            BagUI_UpdateItemSprite(controller, PocketItem(pocket, (int)index));
        }
        else
        {
            BagUI_PrintItemDescription(controller, 0xFFFF);
            BagUI_UpdateItemSprite(controller, 0xFFFF);
        }
        if (controller.HideDescription == 0)
        {
            Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinItemDescription]);
        }
    }

    // OFFSET 0x0223C3B8
    private static void ItemListPrintCallback(ListMenu menu, uint index, byte y)
    {
        BagController controller = (BagController)ListMenu.ListMenu_GetAttribute(menu, 0x13)!;
        BagApplicationPocket pocket = CurrentPocket(controller);
        if (controller.IsMovingItem == 1 && controller.MovedItemID == index)
        {
            ListMenu.ListMenu_SetAltTextColors(menu, 8, 0, 9);
        }
        else
        {
            ListMenu.ListMenu_SetAltTextColors(menu, 1, 0, 2);
        }
        if (pocket.PocketType == PocketKeyItems)
        {
            if (index == Menu.Header || index == Menu.Cancel)
            {
                return;
            }
            if (PocketItem(pocket, (int)index) == BagItems.Bag_GetRegisteredItem(controller.Bag!))
            {
                BagUI_PrintRegisteredIcon(controller, y);
            }
            return;
        }
        if (pocket.PocketType == PocketTMs)
        {
            if (index == Menu.Cancel)
            {
                BagUI_PrintTMHMCancel(controller, y);
                return;
            }
            if (index == Menu.Header)
            {
                return;
            }
            BagUI_PrintTMHMNumber(controller, pocket.Items!, (int)index, y);
            return;
        }
        if (pocket.PocketType == PocketBerries)
        {
            if (index == Menu.Cancel)
            {
                BagUI_PrintTMHMCancel(controller, y);
                return;
            }
            if (index == Menu.Header)
            {
                return;
            }
            BagUI_PrintBerryNumber(controller, pocket.Items!, (int)index, y);
            return;
        }
        if (index == Menu.Header || index == Menu.Cancel)
        {
            return;
        }
        uint color = controller.IsMovingItem == 1 && controller.MovedItemID == index ? 0x80900u : 0x10200u;
        BagUI_PrintItemCount(controller, PocketQuantity(pocket, (int)index), y, color);
    }

    // OFFSET 0x0223C4E0
    private static void FreeItemListMenu(BagController controller)
    {
        BagApplicationPocket pocket = CurrentPocket(controller);
        ListMenu.ListMenu_Free(controller.ItemList!, out pocket.CursorScroll, out pocket.CursorPos);
        StringList.StringList_Free(controller.ItemListEntries!);
        controller.ItemList = null;
    }
}
