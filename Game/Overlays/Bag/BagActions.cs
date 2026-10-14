using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public delegate int BagItemActionFunc(BagController controller);

public static partial class BagApplication
{
    private const uint ItemActionFuncs = 0x02241034;
    private const uint TMSpriteHiddenTilemap = 0x02241064;
    private const uint TMSpriteShownTilemap = 0x02241096;
    private const uint DialTouchedTable = 0x02240EA8;
    private const uint DialHeldTable = 0x02240EB4;
    private const int ItemActionUseAddress = 0x0223D859;
    private const int ItemActionTrashAddress = 0x0223DEB9;
    private const int ItemActionRegisterAddress = 0x0223E1E5;
    private const int ItemActionDeselectAddress = 0x0223E221;
    private const int ItemActionGiveAddress = 0x0223E255;
    private const int ItemActionCheckTagAddress = 0x0223DE79;
    private const int ItemActionConfirmAddress = 0x0223DE99;
    private const int ItemUseTaskTeachMoveAddress = 0x0223DA15;
    private const int ItemUseTaskShowMessageAddress = 0x0223DDD1;
    private const int MenuDialStep = 0x12;
    private const int SeqCountChange = 0x638;
    private const int SeqRepel = 0x600;
    private const int SeqSale = 0x644;
    private const int WinMsgBox = 3;
    private const int WinMsgBoxNarrow = 4;
    private const int WinMsgBoxWide = 6;
    private const int WinSellCountValue = 7;
    private const int WinMoney = 8;
    private const int WinThrowAwayCount = 9;
    private const int MsgBoxTile = 0x3D9;
    private const int MsgBoxPalette = 0xC;
    private const int PocketMail = 5;
    private const int PocketItems = 0;
    private const int MapLoadTypeUnderground = 2;
    private const ushort ItemBlackFlute = 0x44;
    private const ushort ItemWhiteFlute = 0x45;
    private const ushort ItemSuperRepel = 0x4C;
    private const ushort ItemMaxRepel = 0x4D;
    private const ushort ItemRepel = 0x4F;
    private const ushort ItemPoffinCase = 0x1C1;
    private const ushort ItemBicycle = 0x1C2;
    private const int ItemParamPrice = 0;
    private const int ItemParamHoldEffectParam = 2;
    private const int ItemParamPreventToss = 3;
    private const int ItemParamIsSelectable = 4;
    private const int ItemParamFieldUseFunc = 6;
    private const int FieldUseFuncMulch = 0xD;
    private const int ItemFileTypeData = 0;
    private const int ItemUseFuncCheck = 2;

    private const int ActionUse = 0;
    private const int ActionWalk = 1;
    private const int ActionCheck = 2;
    private const int ActionPlant = 3;
    private const int ActionOpen = 4;
    private const int ActionTrash = 5;
    private const int ActionRegister = 6;
    private const int ActionDeselect = 7;
    private const int ActionGive = 8;
    private const int ActionCheckTag = 9;
    private const int ActionConfirm = 10;
    private const int ActionCancel = 11;

    private const int ExitCodeUseItem = 0;
    private const int ExitCodeCheckTag = 1;
    private const int ExitCodeGive = 2;
    private const int ExitCodeConfirm = 3;
    private const int ExitCodeSelectedItem = 4;

    private static Window BagWindow(BagController controller, int index)
    {
        return controller.Windows[index];
    }

    private static bool PlayerAcknowledged()
    {
        return (gSystem.PressedKeys & (KeyA | KeyB)) != 0 || gSystem.TouchPressed != 0;
    }

    private static void DrawWideMessageBox(BagController controller)
    {
        Bg.Window_FillTilemap(BagWindow(controller, WinMsgBoxWide), 0xF);
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(BagWindow(controller, WinMsgBoxWide), 0, MsgBoxTile, MsgBoxPalette);
        controller.MsgBoxPrinterID = (byte)BagUI_PrintStrBufferToWideMsgBox(controller);
    }

    private static void FormatBagString(BagController controller, uint entry)
    {
        GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, entry)!;
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
        GameString.String_Free(format);
    }

    private static void RotateDialFromMenuAction(BagController controller, byte action)
    {
        if (action == 1)
        {
            RotateDial(controller, MenuDialStep);
        }
        else if (action == 2)
        {
            RotateDial(controller, -MenuDialStep);
        }
    }

    private static uint ProcessYesNoMenuInput(BagController controller)
    {
        if (CheckDialButtonPressed(controller))
        {
            uint input = Menu.Menu_ProcessExternalInputAndHandleExit(controller.Menu!, 0, HeapID);
            controller.DialButtonAnimStep = 1;
            return input;
        }
        return Menu.Menu_ProcessInputAndHandleExit(controller.Menu!, HeapID);
    }

    private static void EraseWideMessageBox(BagController controller)
    {
        RenderWindow.Window_EraseMessageBox(BagWindow(controller, WinMsgBoxWide), 0);
        Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinItemDescription]);
        BagUI_SetHighlightSpritesPalette(controller, 1);
    }

    private static void ExitWithFade(BagController controller, ushort exitCode)
    {
        HealthBar.App_StartScreenFade(true, HeapID);
        controller.BagCtx!.ExitCode = exitCode;
    }

    // OFFSET 0x0223D5AC
    private static void MakeItemActionsMenu(BagController controller)
    {
        BagContext bagCtx = controller.BagCtx!;
        byte[] itemData = Item.Item_Load(bagCtx.SelectedItem, ItemFileTypeData, HeapID)!;
        byte[] actions = new byte[NumItemActionSlots];
        int count = 0;
        byte pocketType = CurrentPocket(controller).PocketType;
        if (bagCtx.Mode == ModeField)
        {
            if (pocketType == PocketBerries)
            {
                actions[count++] = ActionCheckTag;
            }
            if ((ushort)(bagCtx.MapLoadType - MapLoadTypeUnderground) <= 1)
            {
                if (CurrentPocket(controller).PocketType == PocketMail)
                {
                    actions[count++] = ActionCheck;
                }
            }
            else if (Item.Item_Get(itemData, 0, ItemParamFieldUseFunc) != 0)
            {
                if (bagCtx.SelectedItem == ItemBicycle && bagCtx.IsCycling)
                {
                    actions[count] = ActionWalk;
                }
                else if (CurrentPocket(controller).PocketType == PocketMail)
                {
                    actions[count] = ActionCheck;
                }
                else if (bagCtx.SelectedItem == ItemPoffinCase)
                {
                    actions[count] = ActionOpen;
                }
                else if (CurrentPocket(controller).PocketType == PocketBerries && ItemUseContext.ItemUseContext_CanPlantBerry(bagCtx.ItemUseCtx!))
                {
                    actions[count] = ActionPlant;
                }
                else
                {
                    actions[count] = ActionUse;
                }
                count++;
            }
            if (Item.Item_Get(itemData, 0, ItemParamPreventToss) == 0)
            {
                actions[count++] = ActionGive;
                if (pocketType != PocketTMs)
                {
                    actions[count++] = ActionTrash;
                }
            }
            if (Item.Item_Get(itemData, 0, ItemParamIsSelectable) != 0)
            {
                actions[count++] = (byte)(BagItems.Bag_GetRegisteredItem(controller.Bag!) == bagCtx.SelectedItem ? ActionDeselect : ActionRegister);
            }
        }
        else if ((byte)(bagCtx.Mode - ModePoffins) <= 1)
        {
            actions[0] = ActionConfirm;
            actions[1] = ActionCheckTag;
            count += 2;
        }
        actions[count] = ActionCancel;
        BagUI_ShowItemActionsMenu(controller, actions, count + 1);
        Heap.Heap_FreeExplicit(HeapID, itemData);
    }

    private const int NumItemActionSlots = 12;

    // OFFSET 0x0223D730
    private static int ProcessMenuInput_SelectAction(BagController controller)
    {
        if (CheckDialScroll_Menu(controller))
        {
            return StateWaitSelectItemAction;
        }
        uint input = Menu.Menu_ProcessInput(controller.Menu!);
        if (input == Menu.NoChoice && CheckDialButtonPressed(controller))
        {
            input = Menu.Menu_ProcessExternalInput(controller.Menu!, 0);
            controller.DialButtonAnimStep = 1;
        }
        if (input == Menu.Cancel)
        {
            BagUI_SetHighlightSpritesPalette(controller, 1);
            BagUI_CloseItemActionsMenu(controller);
            if (CurrentPocket(controller).PocketType == PocketTMs)
            {
                ToggleHideItemSprite(controller, false);
            }
            return StateWaitSelectItemGeneral;
        }
        if (input == Menu.NoChoice)
        {
            RotateDialFromMenuAction(controller, Menu.Menu_GetLastAction(controller.Menu!));
            return StateWaitSelectItemAction;
        }
        return ItemActionFunc(input)(controller);
    }

    private static BagItemActionFunc ItemActionFunc(uint address)
    {
        switch (address)
        {
            case ItemActionUseAddress:
                return ItemAction_Use;
            case ItemActionTrashAddress:
                return ItemAction_Trash;
            case ItemActionRegisterAddress:
                return ItemAction_Register;
            case ItemActionDeselectAddress:
                return ItemAction_Deselect;
            case ItemActionGiveAddress:
                return ItemAction_Give;
            case ItemActionCheckTagAddress:
                return ItemAction_CheckTag;
            case ItemActionConfirmAddress:
                return ItemAction_Confirm;
        }
        throw Unported.Missing(address);
    }

    private static BagItemUseCallback ItemUseCallbackFunc(uint address)
    {
        switch (address)
        {
            case ItemUseTaskTeachMoveAddress:
                return ItemUseTask_TeachMove;
            case ItemUseTaskShowMessageAddress:
                return ItemUseTask_ShowMessage;
        }
        throw Unported.Missing(address);
    }

    // OFFSET 0x0223D7E8
    private static void ToggleHideItemSprite(BagController controller, bool hide)
    {
        if (!hide)
        {
            Bg.Bg_LoadToTilemapRect(controller.BgConfig!, 1, RomData.Overlay(Overlay, TMSpriteHiddenTilemap, 5 * 5 * 2), 0, 0x12, 5, 5);
            SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteItem]!, 1);
        }
        else
        {
            Bg.Bg_LoadToTilemapRect(controller.BgConfig!, 1, RomData.Overlay(Overlay, TMSpriteShownTilemap, 5 * 5 * 2), 0, 0x12, 5, 5);
            SpriteSystem.ManagedSprite_SetDrawFlag(controller.Sprites[SpriteItem]!, 0);
        }
        Bg.Bg_ScheduleTilemapTransfer(controller.BgConfig!, 1);
    }

    // OFFSET 0x0223D84C
    private static uint GetItemActionFunc(int action)
    {
        return RomData.OverlayU32(Overlay, (uint)(ItemActionFuncs + action * 4), 1)[0];
    }

    // OFFSET 0x0223D858
    private static int ItemAction_Use(BagController controller)
    {
        BagUI_CloseItemActionsMenu(controller);
        BagContext bagCtx = controller.BagCtx!;
        ushort useFunc = (ushort)Item.Item_LoadParam(bagCtx.SelectedItem, ItemParamFieldUseFunc, HeapID);
        ItemCheckUseFunc? checkFunc = (ItemCheckUseFunc?)ItemUseFunctions.GetItemUseFunction(ItemUseFuncCheck, useFunc);
        if (checkFunc != null)
        {
            int error = checkFunc(bagCtx.ItemUseCtx!);
            if (error != 0)
            {
                BagContext.BagContext_FormatErrorMessage(controller.TrainerInfo!, controller.StringBuffer!, bagCtx.SelectedItem, error, HeapID);
                DrawWideMessageBox(controller);
                return StateShowingItemUseMsg;
            }
        }
        return TryUseItemInBag(controller);
    }

    // OFFSET 0x0223D8EC
    private static int CheckPlayerDismissedItemUseMsg(BagController controller)
    {
        if (Text.Text_IsPrinterActive(controller.MsgBoxPrinterID) == 0 && PlayerAcknowledged())
        {
            RenderWindow.Window_EraseMessageBox(BagWindow(controller, WinMsgBoxWide), 0);
            Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinItemDescription]);
            BagUI_SetHighlightSpritesPalette(controller, 1);
            if (controller.BagCtx!.Mode == ModeGardening)
            {
                return StateSelectItemGardening;
            }
            return StateWaitSelectItemGeneral;
        }
        return StateShowingItemUseMsg;
    }

    // OFFSET 0x0223D94C
    private static int TryUseItemInBag(BagController controller)
    {
        BagContext bagCtx = controller.BagCtx!;
        controller.ItemUseTaskState = 0;
        if (CurrentPocket(controller).PocketType == PocketTMs)
        {
            controller.ItemUseCallback = ItemUseCallbackFunc(ItemUseTaskTeachMoveAddress);
            return StateRunItemUseTask;
        }
        if (BagContext.BagContext_FormatUsageMessage(bagCtx.SaveData!, controller.StringBuffer!, bagCtx.SelectedItem, HeapID))
        {
            DrawWideMessageBox(controller);
            return StateShowingItemUseMsg;
        }
        if (UseItemInBag(controller, bagCtx.SelectedItem))
        {
            controller.ItemUseCallback = ItemUseCallbackFunc(ItemUseTaskShowMessageAddress);
            return StateRunItemUseTask;
        }
        ExitWithFade(controller, ExitCodeUseItem);
        return StateExit;
    }

    // OFFSET 0x0223DA04
    private static int RunItemUseCallback(BagController controller)
    {
        return controller.ItemUseCallback!(controller);
    }

    // OFFSET 0x0223DA14
    private static int ItemUseTask_TeachMove(BagController controller)
    {
        switch (controller.ItemUseTaskState)
        {
            case 0:
            {
                ushort move = Item.Item_MoveForTMHM(controller.BagCtx!.SelectedItem);
                StringTemplate.StringTemplate_SetMoveName(controller.StrTemplate!, 0, move);
                if (Item.Item_IsHMMove(move) == 1)
                {
                    MessageLoader.MessageLoader_GetString(controller.BagStringsLoader!, 0x3B, controller.StringBuffer!);
                }
                else
                {
                    MessageLoader.MessageLoader_GetString(controller.BagStringsLoader!, 0x3A, controller.StringBuffer!);
                }
                DrawWideMessageBox(controller);
                controller.ItemUseTaskState = 1;
                break;
            }
            case 1:
                if (Text.Text_IsPrinterActive(controller.MsgBoxPrinterID) == 0 && PlayerAcknowledged())
                {
                    GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x3C)!;
                    Bg.Window_FillTilemap(BagWindow(controller, WinMsgBoxWide), 0xF);
                    StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
                    GameString.String_Free(format);
                    controller.MsgBoxPrinterID = (byte)BagUI_PrintStrBufferToWideMsgBox(controller);
                    controller.ItemUseTaskState = 2;
                }
                break;
            case 2:
                if (Text.Text_IsPrinterActive(controller.MsgBoxPrinterID) == 0)
                {
                    BagUI_ShowYesNoMenu(controller);
                    controller.ItemUseTaskState = 3;
                }
                break;
            case 3:
            {
                if (CheckDialScroll_Menu(controller))
                {
                    break;
                }
                uint input = ProcessYesNoMenuInput(controller);
                if (input == 0)
                {
                    HealthBar.App_StartScreenFade(true, HeapID);
                    ToggleHideItemSprite(controller, false);
                    controller.BagCtx!.ExitCode = ExitCodeUseItem;
                    return StateExit;
                }
                if (input == Menu.Cancel)
                {
                    EraseWideMessageBox(controller);
                    ToggleHideItemSprite(controller, false);
                    return StateWaitSelectItemGeneral;
                }
                if (input == Menu.NoChoice)
                {
                    RotateDialFromMenuAction(controller, Menu.Menu_GetLastAction(controller.Menu!));
                }
                break;
            }
        }
        return StateRunItemUseTask;
    }

    // OFFSET 0x0223DBF4
    private static bool UseItemInBag(BagController controller, ushort item)
    {
        GameString format;
        StringTemplate.StringTemplate_SetPlayerName(controller.StrTemplate!, 0, controller.TrainerInfo!);
        StringTemplate.StringTemplate_SetItemName(controller.StrTemplate!, 1, item);
        if (item == ItemBlackFlute)
        {
            format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x40)!;
            BagApplication_SetFluteFactor(controller, 1);
            controller.SelectedItemCount = 0;
        }
        else if (item == ItemWhiteFlute)
        {
            format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x3F)!;
            BagApplication_SetFluteFactor(controller, 2);
            controller.SelectedItemCount = 0;
        }
        else if (item == ItemMaxRepel || item == ItemSuperRepel || item == ItemRepel)
        {
            format = UseRepel(controller, item);
        }
        else
        {
            return false;
        }
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
        GameString.String_Free(format);
        return true;
    }

    // OFFSET 0x0223DC9C
    private static GameString UseRepel(BagController controller, ushort item)
    {
        if (!SpecialEncounter.SpecialEncounter_RepelStepsEmpty(BagApplication_GetSpecialEncounters(controller)))
        {
            controller.SelectedItemCount = 0;
            return MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x3E)!;
        }
        BagApplication_SetRepelSteps(controller, (byte)Item.Item_LoadParam(item, ItemParamHoldEffectParam, HeapID));
        controller.SelectedItemCount = 1;
        SoundPlayback.Sound_PlayEffect(SeqRepel);
        return MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x3D)!;
    }

    private static void RecreateItemList(BagController controller)
    {
        BagApplicationPocket pocket = CurrentPocket(controller);
        ListMenu.ListMenu_Free(controller.ItemList!, out pocket.CursorScroll, out pocket.CursorPos);
        StringList.StringList_Free(controller.ItemListEntries!);
        LoadCurrentPocketItemNames(controller);
        pocket = CurrentPocket(controller);
        LimitItemListScroll(ref pocket.CursorScroll, ref pocket.CursorPos, pocket.ListEntryCount);
        RestrictItemListCursor(ref pocket.CursorScroll, ref pocket.CursorPos, pocket.ListEntryCount, 9);
        CreateItemListMenu(controller, pocket.CursorScroll, pocket.CursorPos);
    }

    // OFFSET 0x0223DCF8
    private static void TrashSelectedItem(BagController controller)
    {
        BagApplicationPocket pocket = CurrentPocket(controller);
        BagItems.Pocket_TryRemoveItem(pocket.Items!, (uint)(pocket.ListEntryCount - 3), controller.BagCtx!.SelectedItem, (ushort)controller.SelectedItemCount);
        RecreateItemList(controller);
    }

    // OFFSET 0x0223DDD0
    private static int ItemUseTask_ShowMessage(BagController controller)
    {
        switch (controller.ItemUseTaskState)
        {
            case 0:
                DrawWideMessageBox(controller);
                controller.ItemUseTaskState = 1;
                break;
            case 1:
                if (Text.Text_IsPrinterActive(controller.MsgBoxPrinterID) == 0 && PlayerAcknowledged())
                {
                    RenderWindow.Window_EraseMessageBox(BagWindow(controller, WinMsgBoxWide), 0);
                    controller.ItemUseTaskState = 2;
                }
                break;
            case 2:
                if (controller.SelectedItemCount != 0)
                {
                    TrashSelectedItem(controller);
                }
                Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinItemDescription]);
                BagUI_SetHighlightSpritesPalette(controller, 1);
                return StateWaitSelectItemGeneral;
        }
        return StateRunItemUseTask;
    }

    // OFFSET 0x0223DE78
    private static int ItemAction_CheckTag(BagController controller)
    {
        BagUI_CloseItemActionsMenu(controller);
        ExitWithFade(controller, ExitCodeCheckTag);
        return StateExit;
    }

    // OFFSET 0x0223DE98
    private static int ItemAction_Confirm(BagController controller)
    {
        BagUI_CloseItemActionsMenu(controller);
        ExitWithFade(controller, ExitCodeConfirm);
        return StateExit;
    }

    // OFFSET 0x0223DEB8
    private static int ItemAction_Trash(BagController controller)
    {
        BagUI_CloseItemActionsMenu(controller);
        controller.SelectedItemCount = 1;
        BagApplicationPocket pocket = CurrentPocket(controller);
        if (BagItems.Pocket_GetItemQuantity(pocket.Items!, (uint)(pocket.ListEntryCount - 3), controller.BagCtx!.SelectedItem) == 1)
        {
            BagUI_PrintConfirmItemTrashMsg(controller);
            return StateShowConfirmTrashMsg;
        }
        BagUI_ShowItemTrashWindows(controller);
        BagUI_ShowItemCountArrows(controller, 0);
        return StateSelectItemTrashCount;
    }

    // OFFSET 0x0223DF0C
    private static int ProcessItemCountInput_TrashCount(BagController controller)
    {
        if (CheckDialItemAmountChange(controller, ref controller.SelectedItemCount, controller.SelectedItemCountLimit))
        {
            BagUI_PrintItemTrashCount(controller);
            return StateSelectItemTrashCount;
        }
        if (CheckDialButtonPressed(controller))
        {
            BagUI_PrintConfirmItemTrashMsg(controller);
            BagUI_ToggleItemCountArrows(controller, 0);
            controller.DialButtonAnimStep = 1;
            return StateShowConfirmTrashMsg;
        }
        switch (HealthBar.App_AdjustItemCountWithInput(ref controller.SelectedItemCount, controller.SelectedItemCountLimit))
        {
            case 1:
                RotateDial(controller, MenuDialStep);
                BagUI_PrintItemTrashCount(controller);
                SoundPlayback.Sound_PlayEffect(SeqCountChange);
                return StateSelectItemTrashCount;
            case 2:
                RotateDial(controller, -MenuDialStep);
                BagUI_PrintItemTrashCount(controller);
                SoundPlayback.Sound_PlayEffect(SeqCountChange);
                return StateSelectItemTrashCount;
        }
        if ((gSystem.PressedKeys & KeyA) != 0)
        {
            BagUI_PrintConfirmItemTrashMsg(controller);
            BagUI_ToggleItemCountArrows(controller, 0);
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            return StateShowConfirmTrashMsg;
        }
        if ((gSystem.PressedKeys & KeyB) != 0)
        {
            BagUI_CloseItemTrashWindows(controller);
            BagUI_ToggleItemCountArrows(controller, 0);
            BagUI_SetHighlightSpritesPalette(controller, 1);
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            return StateWaitSelectItemGeneral;
        }
        return StateSelectItemTrashCount;
    }

    // OFFSET 0x0223DFF8
    private static int ShowConfirmTrashMenu(BagController controller)
    {
        if (Text.Text_IsPrinterActive(controller.MsgBoxPrinterID) == 0)
        {
            BagUI_ShowYesNoMenu(controller);
            return StateConfirmTrash;
        }
        return StateShowConfirmTrashMsg;
    }

    // OFFSET 0x0223E01C
    private static int ProcessMenuInput_ConfirmTrash(BagController controller)
    {
        if (CheckDialScroll_Menu(controller))
        {
            return StateConfirmTrash;
        }
        uint input = ProcessYesNoMenuInput(controller);
        if (input == 0)
        {
            GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x35)!;
            if (controller.SelectedItemCount == 1)
            {
                StringTemplate.StringTemplate_SetItemName(controller.StrTemplate!, 0, controller.BagCtx!.SelectedItem);
            }
            else
            {
                StringTemplate.StringTemplate_SetItemNamePlural(controller.StrTemplate!, 0, controller.BagCtx!.SelectedItem);
            }
            StringTemplate.StringTemplate_SetNumber(controller.StrTemplate!, 1, controller.SelectedItemCount, 3, 0, 1);
            StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
            GameString.String_Free(format);
            Bg.Window_FillTilemap(BagWindow(controller, WinMsgBoxWide), 0xF);
            controller.MsgBoxPrinterID = (byte)BagUI_PrintStrBufferToWideMsgBox(controller);
            return StateResolveTrash;
        }
        if (input == Menu.Cancel)
        {
            EraseWideMessageBox(controller);
            return StateWaitSelectItemGeneral;
        }
        if (input == Menu.NoChoice)
        {
            RotateDialFromMenuAction(controller, Menu.Menu_GetLastAction(controller.Menu!));
        }
        return StateConfirmTrash;
    }

    // OFFSET 0x0223E158
    private static int ResolveTrash(BagController controller)
    {
        if (Text.Text_IsPrinterActive(controller.MsgBoxPrinterID) != 0)
        {
            return StateResolveTrash;
        }
        controller.HideDescription = 1;
        TrashSelectedItem(controller);
        Bg.Window_ScheduleCopyToVRAM(BagWindow(controller, WinMsgBoxWide));
        return StateShowingTrashedMsg;
    }

    // OFFSET 0x0223E18C
    private static int CheckPlayerDismissedTrashedMsg(BagController controller)
    {
        if (Text.Text_IsPrinterActive(controller.MsgBoxPrinterID) == 0 && PlayerAcknowledged())
        {
            controller.HideDescription = 0;
            EraseWideMessageBox(controller);
            return StateWaitSelectItemGeneral;
        }
        return StateShowingTrashedMsg;
    }

    // OFFSET 0x0223E1E4
    private static int ItemAction_Register(BagController controller)
    {
        BagItems.Bag_SetRegisteredItem(controller.Bag!, controller.BagCtx!.SelectedItem);
        ListMenu.ListMenu_Draw(controller.ItemList!);
        BagUI_CloseItemActionsMenu(controller);
        Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinItemDescription]);
        BagUI_SetHighlightSpritesPalette(controller, 1);
        return StateWaitSelectItemGeneral;
    }

    // OFFSET 0x0223E220
    private static int ItemAction_Deselect(BagController controller)
    {
        BagItems.Bag_SetRegisteredItem(controller.Bag!, 0);
        ListMenu.ListMenu_Draw(controller.ItemList!);
        BagUI_CloseItemActionsMenu(controller);
        Bg.Window_ScheduleCopyToVRAM(controller.Windows[WinItemDescription]);
        BagUI_SetHighlightSpritesPalette(controller, 1);
        return StateWaitSelectItemGeneral;
    }

    // OFFSET 0x0223E254
    private static int ItemAction_Give(BagController controller)
    {
        BagUI_CloseItemActionsMenu(controller);
        HealthBar.App_StartScreenFade(true, HeapID);
        ToggleHideItemSprite(controller, false);
        controller.BagCtx!.ExitCode = ExitCodeGive;
        return StateExit;
    }

    private static int ProcessPocketAndListInput(BagController controller, int idleState, out int input)
    {
        input = ListInputNone;
        StepPocketSelectorArrowsAnim(controller);
        if (CheckItemListDialScroll_Normal(controller))
        {
            return idleState;
        }
        if (CheckPocketChange_DPad(controller))
        {
            return StateSwitchPocket;
        }
        if (CheckPocketChange_Touch(controller))
        {
            return StateSwitchPocket;
        }
        input = ProcessItemListMenuInput(controller);
        return -1;
    }

    // OFFSET 0x0223E27C
    private static int ProcessItemListInput_GiveToMon(BagController controller)
    {
        int state = ProcessPocketAndListInput(controller, StateSelectItemToGive, out int input);
        if (state != -1)
        {
            return state;
        }
        if (input == ListInputSelected)
        {
            BagContext bagCtx = controller.BagCtx!;
            if (Item.Item_LoadParam(bagCtx.SelectedItem, ItemParamPreventToss, HeapID) != 0)
            {
                Bg.Window_FillTilemap(BagWindow(controller, WinMsgBoxWide), 0xF);
                RenderWindow.Window_DrawMessageBoxWithScrollCursor(BagWindow(controller, WinMsgBoxWide), 0, MsgBoxTile, MsgBoxPalette);
                StringTemplate.StringTemplate_SetItemName(controller.StrTemplate!, 0, bagCtx.SelectedItem);
                FormatBagString(controller, 0x2E);
                controller.MsgBoxPrinterID = (byte)BagUI_PrintStrBufferToWideMsgBox(controller);
                BagUI_SetHighlightSpritesPalette(controller, 2);
                return StateShowingItemCantBeHeldMsg;
            }
            bagCtx.ExitCode = ExitCodeSelectedItem;
            HealthBar.App_StartScreenFade(true, HeapID);
            return StateExit;
        }
        if (input == ListInputExit)
        {
            controller.BagCtx!.ExitCode = ExitCodeSelectedItem;
            return StateExit;
        }
        return StateSelectItemToGive;
    }

    // OFFSET 0x0223E36C
    private static int CheckPlayerDismissedItemCantBeHeldMsg(BagController controller)
    {
        if (Text.Text_IsPrinterActive(controller.MsgBoxPrinterID) == 0 && PlayerAcknowledged())
        {
            EraseWideMessageBox(controller);
            return StateSelectItemToGive;
        }
        return StateShowingItemCantBeHeldMsg;
    }

    private static void SetSaleValueNumber(BagController controller)
    {
        StringTemplate.StringTemplate_SetNumber(controller.StrTemplate!, 1, (int)(controller.SelectedItemCount * controller.SoldItemPrice), 6, 0, 1);
    }

    // OFFSET 0x0223E3BC
    private static int ProcessItemListInput_SellItems(BagController controller)
    {
        int state = ProcessPocketAndListInput(controller, StateSelectItemToSell, out int input);
        if (state != -1)
        {
            return state;
        }
        if (input == ListInputSelected)
        {
            BagContext bagCtx = controller.BagCtx!;
            BagUI_PrintMoney(controller, false);
            Bg.Window_FillTilemap(BagWindow(controller, WinMsgBoxWide), 0xF);
            RenderWindow.Window_DrawMessageBoxWithScrollCursor(BagWindow(controller, WinMsgBoxWide), 0, MsgBoxTile, MsgBoxPalette);
            StringTemplate.StringTemplate_SetItemName(controller.StrTemplate!, 0, bagCtx.SelectedItem);
            BagUI_SetHighlightSpritesPalette(controller, 2);
            controller.SoldItemPrice = (uint)Item.Item_LoadParam(bagCtx.SelectedItem, ItemParamPrice, HeapID);
            if (Item.Item_LoadParam(bagCtx.SelectedItem, ItemParamPreventToss, HeapID) != 0 || controller.SoldItemPrice == 0)
            {
                FormatBagString(controller, 0x4A);
                controller.MsgBoxPrinterID = (byte)BagUI_PrintStrBufferToWideMsgBox(controller);
                return StateShowingItemsSoldMsg;
            }
            controller.SelectedItemCount = 1;
            controller.SoldItemPrice >>= 1;
            BagApplicationPocket pocket = CurrentPocket(controller);
            if (BagItems.Pocket_GetItemQuantity(pocket.Items!, (uint)(pocket.ListEntryCount - 3), bagCtx.SelectedItem) == 1)
            {
                GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x4C)!;
                SetSaleValueNumber(controller);
                StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
                GameString.String_Free(format);
                controller.MsgBoxPrinterID = (byte)BagUI_PrintStrBufferToWideMsgBox(controller);
                return StatePrintConfirmSaleMsg;
            }
            FormatBagString(controller, 0x4B);
            controller.MsgBoxPrinterID = (byte)BagUI_PrintStrBufferToWideMsgBox(controller);
            return StateWaitSellHowManyMsg;
        }
        if (input == ListInputExit)
        {
            return StateExit;
        }
        return StateSelectItemToSell;
    }

    // OFFSET 0x0223E588
    private static int ShowSaleCountValueWindow(BagController controller)
    {
        if (Text.Text_IsPrinterActive(controller.MsgBoxPrinterID) == 0)
        {
            if (controller.SelectedItemCountLimit > 99)
            {
                controller.SelectedItemCountLimit = 99;
            }
            BagUI_PrintSellCountAndValue(controller, false);
            BagUI_ShowItemCountArrows(controller, 1);
            return StateSelectItemSellCount;
        }
        return StateWaitSellHowManyMsg;
    }

    private static void PrintConfirmSaleMessage(BagController controller)
    {
        BagUI_ToggleItemCountArrows(controller, 0);
        RenderWindow.Window_EraseStandardFrame(BagWindow(controller, WinSellCountValue), 1);
        Bg.Window_FillTilemap(BagWindow(controller, WinMsgBoxWide), 0xF);
        GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x4C)!;
        SetSaleValueNumber(controller);
        StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
        GameString.String_Free(format);
        controller.MsgBoxPrinterID = (byte)BagUI_PrintStrBufferToWideMsgBox(controller);
    }

    // OFFSET 0x0223E5C4
    private static int ProcessItemCountInput_SellCount(BagController controller)
    {
        if (CheckDialItemAmountChange(controller, ref controller.SelectedItemCount, controller.SelectedItemCountLimit))
        {
            BagUI_PrintSellCountAndValue(controller, true);
            return StateSelectItemSellCount;
        }
        if (CheckDialButtonPressed(controller))
        {
            PrintConfirmSaleMessage(controller);
            controller.DialButtonAnimStep = 1;
            return StatePrintConfirmSaleMsg;
        }
        switch (HealthBar.App_AdjustItemCountWithInput(ref controller.SelectedItemCount, controller.SelectedItemCountLimit))
        {
            case 1:
                RotateDial(controller, MenuDialStep);
                BagUI_PrintSellCountAndValue(controller, true);
                SoundPlayback.Sound_PlayEffect(SeqCountChange);
                return StateSelectItemSellCount;
            case 2:
                RotateDial(controller, -MenuDialStep);
                BagUI_PrintSellCountAndValue(controller, true);
                SoundPlayback.Sound_PlayEffect(SeqCountChange);
                return StateSelectItemSellCount;
        }
        if ((gSystem.PressedKeys & KeyA) != 0)
        {
            PrintConfirmSaleMessage(controller);
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            return StatePrintConfirmSaleMsg;
        }
        if ((gSystem.PressedKeys & KeyB) != 0)
        {
            controller.SoldItemPrice = 0;
            BagUI_ToggleItemCountArrows(controller, 0);
            RenderWindow.Window_EraseStandardFrame(BagWindow(controller, WinMoney), 1);
            RenderWindow.Window_EraseStandardFrame(BagWindow(controller, WinSellCountValue), 1);
            EraseWideMessageBox(controller);
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            return StateSelectItemToSell;
        }
        return StateSelectItemSellCount;
    }

    // OFFSET 0x0223E7A8
    private static int WaitConfirmSaleMsgPrinted(BagController controller)
    {
        if (Text.Text_IsPrinterActive(controller.MsgBoxPrinterID) == 0)
        {
            BagUI_ShowYesNoMenu(controller);
            return StateConfirmSale;
        }
        return StatePrintConfirmSaleMsg;
    }

    // OFFSET 0x0223E7CC
    private static int ProcessMenuInput_ConfirmSale(BagController controller)
    {
        if (CheckDialScroll_Menu(controller))
        {
            return StateConfirmSale;
        }
        uint input = ProcessYesNoMenuInput(controller);
        if (input == 0)
        {
            GameString format = MessageLoader.MessageLoader_GetNewString(controller.BagStringsLoader!, 0x4D)!;
            if (controller.SelectedItemCount > 1)
            {
                StringTemplate.StringTemplate_SetItemNamePlural(controller.StrTemplate!, 0, controller.BagCtx!.SelectedItem);
            }
            else
            {
                StringTemplate.StringTemplate_SetItemName(controller.StrTemplate!, 0, controller.BagCtx!.SelectedItem);
            }
            SetSaleValueNumber(controller);
            StringTemplate.StringTemplate_Format(controller.StrTemplate!, controller.StringBuffer!, format);
            GameString.String_Free(format);
            Bg.Window_FillTilemap(BagWindow(controller, WinMsgBoxWide), 0xF);
            controller.MsgBoxPrinterID = (byte)BagUI_PrintStrBufferToWideMsgBox(controller);
            return StateResolveSale;
        }
        if (input == Menu.NoChoice)
        {
            RotateDialFromMenuAction(controller, Menu.Menu_GetLastAction(controller.Menu!));
            return StateConfirmSale;
        }
        if (input == Menu.Cancel)
        {
            controller.SoldItemPrice = 0;
            RenderWindow.Window_EraseStandardFrame(BagWindow(controller, WinMoney), 1);
            EraseWideMessageBox(controller);
            return StateSelectItemToSell;
        }
        return StateConfirmSale;
    }

    // OFFSET 0x0223E920
    private static int ResolveSale(BagController controller)
    {
        if (Text.Text_IsPrinterActive(controller.MsgBoxPrinterID) != 0)
        {
            return StateResolveSale;
        }
        SoundPlayback.Sound_PlayEffect(SeqSale);
        TrainerInfo.TrainerInfo_GiveMoney(controller.TrainerInfo!, (uint)(controller.SelectedItemCount * controller.SoldItemPrice));
        BagContext bagCtx = controller.BagCtx!;
        if (controller.SelectedItemCount == 1)
        {
            if (bagCtx.SoldAmount != 0xFF)
            {
                bagCtx.SoldAmount++;
            }
        }
        else
        {
            bagCtx.SoldAmount = 2;
        }
        BagUI_PrintMoney(controller, true);
        controller.HideDescription = 1;
        TrashSelectedItem(controller);
        Bg.Window_ScheduleCopyToVRAM(BagWindow(controller, WinMsgBoxWide));
        return StateShowingItemsSoldMsg;
    }

    // OFFSET 0x0223E9B0
    private static int CheckPlayerDismissedItemsSoldMsg(BagController controller)
    {
        if (Text.Text_IsPrinterActive(controller.MsgBoxPrinterID) == 0 && PlayerAcknowledged())
        {
            controller.HideDescription = 0;
            controller.SoldItemPrice = 0;
            RenderWindow.Window_EraseStandardFrame(BagWindow(controller, WinMoney), 1);
            EraseWideMessageBox(controller);
            return StateSelectItemToSell;
        }
        return StateShowingItemsSoldMsg;
    }

    // OFFSET 0x0223EA18
    private static int ProcessItemListInput_Gardening(BagController controller)
    {
        int state = ProcessPocketAndListInput(controller, StateSelectItemGardening, out int input);
        if (state != -1)
        {
            return state;
        }
        if (input == ListInputSelected)
        {
            BagContext bagCtx = controller.BagCtx!;
            if (CurrentPocket(controller).PocketType == PocketItems && Item.Item_LoadParam(bagCtx.SelectedItem, ItemParamFieldUseFunc, HeapID) != FieldUseFuncMulch)
            {
                BagContext.BagContext_FormatErrorMessage(controller.TrainerInfo!, controller.StringBuffer!, bagCtx.SelectedItem, -1, HeapID);
                DrawWideMessageBox(controller);
                return StateShowingItemUseMsg;
            }
            bagCtx.ExitCode = ExitCodeUseItem;
            HealthBar.App_StartScreenFade(true, HeapID);
            return StateExit;
        }
        if (input == ListInputExit)
        {
            controller.BagCtx!.ExitCode = ExitCodeDone;
            return StateExit;
        }
        return StateSelectItemGardening;
    }

    // OFFSET 0x0223EB08
    private static void RotateDial(BagController controller, int delta)
    {
        short rotation = (short)((short)Bg.Bg_GetRotation(controller.BgConfig!, 7) + delta);
        if (rotation >= 360)
        {
            rotation = (short)(rotation - 360);
        }
        else if (rotation < 0)
        {
            rotation = (short)(rotation + 360);
        }
        Bg.Bg_ScheduleAffineRotation(controller.BgConfig!, 7, 0, (ushort)rotation);
        controller.DialRotation = rotation;
    }

    // OFFSET 0x0223EB54
    private static bool DetectPlayerTouchedDial()
    {
        return TouchScreen.TouchScreen_CheckPressedHitTableID(OverlayTouchRects(DialTouchedTable)) == 1;
    }

    // OFFSET 0x0223EB6C
    private static bool DetectPlayerHeldDial()
    {
        return TouchScreen.TouchScreen_CheckHeldHitTableID(OverlayTouchRects(DialHeldTable)) == 1;
    }

    // OFFSET 0x0223EB84
    private static bool CalcDialScroll(BagController controller, int degreesPerStep)
    {
        if (DetectPlayerTouchedDial())
        {
            controller.IsTouchingDial = 1;
            controller.QueuedScroll = 0;
            controller.QueuedScrollRemainder = 0;
            controller.PreviousTouchX = gSystem.TouchX;
            controller.PreviousTouchY = gSystem.TouchY;
        }
        if (controller.IsTouchingDial != 1)
        {
            return false;
        }
        if (!DetectPlayerHeldDial())
        {
            controller.IsTouchingDial = 0;
            controller.QueuedScroll = 0;
            return true;
        }
        int arcLength = MathUtil.ApproximateArcLength(0x80 - controller.PreviousTouchX, 0x50 - controller.PreviousTouchY, 0x80 - gSystem.TouchX, 0x50 - gSystem.TouchY, 0x50);
        int angle = MathUtil.CalcRadialAngle(0x50, arcLength * 2);
        controller.DialRotation = (short)(controller.DialRotation + ((angle << 8) / 0xB6 >> 8));
        if (controller.DialRotation < 0)
        {
            controller.DialRotation = (short)(controller.DialRotation + 360);
        }
        else if (controller.DialRotation >= 360)
        {
            controller.DialRotation = (short)(controller.DialRotation - 360);
        }
        Bg.Bg_ScheduleAffineRotation(controller.BgConfig!, 7, 0, (ushort)controller.DialRotation);
        int stepLength = 0x1F6 / degreesPerStep;
        if (arcLength > 0)
        {
            if (controller.QueuedScroll < 0)
            {
                controller.QueuedScroll = (short)(arcLength / stepLength);
                controller.QueuedScrollRemainder = arcLength % stepLength;
            }
            else
            {
                controller.QueuedScroll = (short)(controller.QueuedScroll + (controller.QueuedScrollRemainder + arcLength) / stepLength);
                controller.QueuedScrollRemainder = (controller.QueuedScrollRemainder + arcLength) % stepLength;
            }
        }
        else if (arcLength < 0)
        {
            if (controller.QueuedScroll > 0)
            {
                controller.QueuedScroll = (short)(arcLength / stepLength);
                controller.QueuedScrollRemainder = arcLength % stepLength;
            }
            else
            {
                controller.QueuedScroll = (short)(controller.QueuedScroll + (controller.QueuedScrollRemainder + arcLength) / stepLength);
                controller.QueuedScrollRemainder = (controller.QueuedScrollRemainder + arcLength) % stepLength;
            }
        }
        controller.PreviousTouchX = gSystem.TouchX;
        controller.PreviousTouchY = gSystem.TouchY;
        return true;
    }

    private static bool ConsumeQueuedScroll(BagController controller, bool calcResult, Func<BagController, int, bool> step, int upInput, int downInput)
    {
        if (controller.QueuedScroll > 0)
        {
            controller.QueuedScroll = step(controller, upInput) ? (short)(controller.QueuedScroll - 1) : (short)0;
            return true;
        }
        if (controller.QueuedScroll < 0)
        {
            controller.QueuedScroll = step(controller, downInput) ? (short)(controller.QueuedScroll + 1) : (short)0;
            return true;
        }
        return calcResult;
    }

    // OFFSET 0x0223ED14
    private static bool CheckItemListDialScroll_Normal(BagController controller)
    {
        bool result = CalcDialScroll(controller, 0x24);
        return ConsumeQueuedScroll(controller, result, DoItemListInput_Normal, KeyUp, KeyDown);
    }

    private static bool DoItemListInput(BagController controller, int input, int sprite, int yOffset)
    {
        BagApplicationPocket pocket = CurrentPocket(controller);
        ListMenu.ListMenu_TestInput(controller.ItemList!, null, pocket.CursorScroll, pocket.CursorPos, 1, (ushort)input, out ushort listPos, out ushort cursorPos);
        if (listPos == pocket.CursorScroll && cursorPos == pocket.CursorPos)
        {
            return false;
        }
        SetItemHighlightRow(controller, sprite, cursorPos, yOffset);
        pocket.CursorScroll = listPos;
        pocket.CursorPos = cursorPos;
        return true;
    }

    // OFFSET 0x0223ED64
    private static bool DoItemListInput_Normal(BagController controller, int input)
    {
        return DoItemListInput(controller, input, SpriteItemHighlight, 0x18);
    }

    // OFFSET 0x0223EE30
    private static bool CheckItemListDialScroll_Sorting(BagController controller)
    {
        bool result = CalcDialScroll(controller, 0x24);
        return ConsumeQueuedScroll(controller, result, DoItemListInput_Sorting, KeyUp, KeyDown);
    }

    // OFFSET 0x0223EE80
    private static bool DoItemListInput_Sorting(BagController controller, int input)
    {
        return DoItemListInput(controller, input, SpriteMovingItemPosBar, 0x10);
    }

    // OFFSET 0x0223EF4C
    private static bool CheckDialScroll_Menu(BagController controller)
    {
        bool result = CalcDialScroll(controller, MenuDialStep);
        return ConsumeQueuedScroll(controller, result, DoMenuInput, 2, 3);
    }

    // OFFSET 0x0223EF9C
    private static bool DoMenuInput(BagController controller, int input)
    {
        byte before = Menu.Menu_GetCursorPos(controller.Menu!);
        Menu.Menu_ProcessExternalInput(controller.Menu!, (byte)input);
        return before != Menu.Menu_GetCursorPos(controller.Menu!);
    }

    // OFFSET 0x0223EFD0
    private static bool CheckDialItemAmountChange(BagController controller, ref short count, ushort max)
    {
        bool result = CalcDialScroll(controller, MenuDialStep);
        short original = count;
        if (controller.QueuedScroll > 0)
        {
            controller.QueuedScroll--;
            count++;
            if (count > max)
            {
                count = 1;
            }
            if (count == original)
            {
                return false;
            }
            SoundPlayback.Sound_PlayEffect(SeqCountChange);
            return true;
        }
        if (controller.QueuedScroll < 0)
        {
            controller.QueuedScroll++;
            count--;
            if (count <= 0)
            {
                count = (short)max;
            }
            if (count == original)
            {
                return false;
            }
            SoundPlayback.Sound_PlayEffect(SeqCountChange);
            return true;
        }
        return result;
    }
}
