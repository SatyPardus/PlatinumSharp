using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public static partial class PartyMenus
{
    private const int FormChangeOverlay = 0x76;
    private const ushort ItemGracidea = 0x1D2;
    private const ushort ItemGriseousOrb = 0x70;
    private const int SpeciesGiratina = 0x1E7;
    private const int MapDistortionWorld = 0x1D2;
    private const int MapGiratinaFormLockFirst = 0x23D;
    private const int MapGiratinaFormLockLast = 0x247;
    private const int ItemParamPPUp = 0x22;
    private const int ItemParamPPMax = 0x23;
    private const int ItemParamPPRestore = 0x24;
    private const int ItemParamPPRestoreAll = 0x25;
    private const int ItemParamEvolve = 0x1A;
    private const int EvoClassByItem = 3;
    private const int ExitCodeWriteMail = 6;
    private const int ExitCodeEvolveByItem = 8;
    private const int ExitCodeReturnToBag = 10;
    private const int TextItWontHaveAnyEffect = 0x69;
    private const int PanelTilemapMember = 0x16;

    private static int CurrentMapID(FieldSystem? fieldSystem)
    {
        return (int)fieldSystem!.Location!.U32(0);
    }

    // OFFSET 0x0208107C
    private static int ApplyItemEffectOnPokemon(PartyMenuApplication app)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        byte[] itemData = Item.Item_Load(partyMenu.UsedItemID, 0, HeapID)!;
        if (partyMenu.UsedItemID == ItemGracidea && Pokemon.Pokemon_CanShayminSkyForm(Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot)))
        {
            partyMenu.EvoTargetSpecies = 1;
            PartyMenu_SetupFormChangeAnim(app);
            return StateWaitFormChange;
        }
        if (Item.Item_Get(itemData, 0, ItemParamPPUp) != 0 || Item.Item_Get(itemData, 0, ItemParamPPMax) != 0)
        {
            PartyMenu_MoveSelection_Display(app, 0);
            return StateWaitMoveListSelection;
        }
        if (Item.Item_Get(itemData, 0, ItemParamPPRestore) != 0 && Item.Item_Get(itemData, 0, ItemParamPPRestoreAll) == 0)
        {
            PartyMenu_MoveSelection_Display(app, 1);
            return StateWaitMoveListSelection;
        }
        if (ItemEffects.Party_CanUseItemOnPokemonAt(partyMenu.Party!, partyMenu.UsedItemID, app.CurrPartySlot, 0, HeapID))
        {
            BagItems.Bag_TryRemoveItem(partyMenu.Bag!, partyMenu.UsedItemID, 1, HeapID);
            if (Item.Item_Get(itemData, 0, ItemParamEvolve) != 0)
            {
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
                partyMenu.EvoTargetSpecies = (ushort)Pokemon.Pokemon_GetEvolutionTargetSpecies(null, mon, EvoClassByItem, partyMenu.UsedItemID, out partyMenu.EvoType);
                partyMenu.MenuSelectionResult = ExitCodeEvolveByItem;
                return StateFadeOut;
            }
            if (Item.Item_IsHerbalMedicine(partyMenu.UsedItemID) == 1 && partyMenu.Broadcast != null)
            {
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
                TVSegments.FieldSystem_SaveTVSegment_HerbalMedicineTrainerSightingDummy(partyMenu.Broadcast, mon, partyMenu.UsedItemID);
            }
            PartyMenu_SetItemUseCallback(app);
        }
        else
        {
            PartyMenu_PrintLongMessage(app, TextItWontHaveAnyEffect, 1);
            app.CurrPartySlot = SlotCancel;
            app.Callback = PartyMenuCB_PrintThenWaitABPress;
        }
        return StateExecCallback;
    }

    // OFFSET 0x020811F4
    private static int CheckItemUsageValidity(PartyMenuApplication app)
    {
        ushort heldItem = app.PartyMembers[app.CurrPartySlot].HeldItem;
        if (heldItem == 0)
        {
            return 0;
        }
        if (Item.Item_IsMail(heldItem) == 1)
        {
            return 2;
        }
        return 1;
    }

    // OFFSET 0x02081224
    private static int ProcessItemApplication(PartyMenuApplication app)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
        FieldSystem? fieldSystem = partyMenu.FieldSystem;
        int state = -1;
        if (partyMenu.UsedItemID == ItemGriseousOrb)
        {
            if (Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null) != SpeciesGiratina)
            {
                MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0xCB, app.TmpFormat!);
                StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
                StringTemplate.StringTemplate_SetItemNameWithArticle(app.Template!, 1, partyMenu.UsedItemID);
                StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, app.TmpFormat!);
                state = StateConfirmItemUpdate;
            }
            else if (fieldSystem != null && CurrentMapID(fieldSystem) == MapDistortionWorld)
            {
                MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0xCC, app.TmpFormat!);
                StringTemplate.StringTemplate_SetItemName(app.Template!, 0, partyMenu.UsedItemID);
                StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, app.TmpFormat!);
                state = StateConfirmItemUpdate;
            }
        }
        if (state == -1)
        {
            switch (CheckItemUsageValidity(app))
            {
                case 0:
                    if (Item.Item_IsMail(partyMenu.UsedItemID) == 1)
                    {
                        partyMenu.MenuSelectionResult = ExitCodeWriteMail;
                        return StateFadeOut;
                    }
                    state = UpdatePokemonWithItem(app, mon, out _);
                    MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x76, app.TmpFormat!);
                    StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
                    StringTemplate.StringTemplate_SetItemName(app.Template!, 1, partyMenu.UsedItemID);
                    StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, app.TmpFormat!);
                    break;
                case 1:
                    MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x4E, app.TmpFormat!);
                    StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
                    StringTemplate.StringTemplate_SetItemNameWithArticle(app.Template!, 1, app.PartyMembers[app.CurrPartySlot].HeldItem);
                    StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, app.TmpFormat!);
                    state = StateShowItemSwapConfirmation;
                    break;
                case 2:
                    MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x4D, app.TmpString!);
                    state = StateConfirmItemUpdate;
                    break;
            }
        }
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(app.Windows[WinLongMessage], 1, 0xA, 0xF);
        Bg.Window_FillTilemap(app.Windows[WinLongMessage], 0xF);
        PartyMenu_AddLongMessagePrinter(app);
        return state;
    }

    // OFFSET 0x02081408
    private static int UpdatePokemonWithItem(PartyMenuApplication app, Pokemon mon, out int form)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        uint item = partyMenu.UsedItemID;
        FieldSystem? fieldSystem = partyMenu.FieldSystem;
        BagItems.Bag_TryRemoveItem(partyMenu.Bag!, partyMenu.UsedItemID, 1, HeapID);
        Pokemon.Pokemon_SetValue(mon, MonDataHeldItem, item);
        Pokemon.Pokemon_SetArceusForm(mon);
        if (fieldSystem == null || CurrentMapID(fieldSystem) < MapGiratinaFormLockFirst || CurrentMapID(fieldSystem) > MapGiratinaFormLockLast)
        {
            form = Pokemon.Pokemon_SetGiratinaFormByHeldItem(mon);
        }
        else
        {
            form = -1;
        }
        app.PartyMembers[app.CurrPartySlot].HeldItem = partyMenu.UsedItemID;
        PartyMenu_DrawMemberHeldItem(app, app.CurrPartySlot, app.PartyMembers[app.CurrPartySlot].HeldItem);
        if (item == ItemGriseousOrb && form != -1)
        {
            return State12;
        }
        return StateConfirmItemUpdate;
    }

    // OFFSET 0x020814A8
    private static void SwapPokemonItem(PartyMenuApplication app, Pokemon mon, ushort returnedItem, uint heldItem)
    {
        BagItems.Bag_TryAddItem(app.PartyMenu!.Bag!, returnedItem, 1, HeapID);
        Pokemon.Pokemon_SetValue(mon, MonDataHeldItem, heldItem);
        Pokemon.Pokemon_SetArceusForm(mon);
        Pokemon.Pokemon_SetGiratinaFormByHeldItem(mon);
        app.PartyMembers[app.CurrPartySlot].HeldItem = (ushort)heldItem;
        PartyMenu_DrawMemberHeldItem(app, app.CurrPartySlot, app.PartyMembers[app.CurrPartySlot].HeldItem);
    }

    private static bool ABPressedAfterPrinting(PartyMenuApplication app)
    {
        return Text.Text_IsPrinterActive(app.TextPrinterID) == 0 && (gSystem.PressedKeys & (KeyA | KeyB)) != 0;
    }

    // OFFSET 0x0208150C
    private static int PartyMenu_ConfirmItemUpdate(PartyMenuApplication app)
    {
        if (ABPressedAfterPrinting(app))
        {
            return ResetWindowOnInput(app);
        }
        return StateConfirmItemUpdate;
    }

    // OFFSET 0x0208153C
    private static int ProcessMessageResult(PartyMenuApplication app)
    {
        if (ABPressedAfterPrinting(app))
        {
            RenderWindow.Window_EraseMessageBox(app.Windows[WinLongMessage], 1);
            PartyMenu_SetupFormChangeAnim(app);
            return State13;
        }
        return State12;
    }

    // OFFSET 0x02081578
    private static int HandleOverlayCompletion(PartyMenuApplication app)
    {
        if (PartyMenuFormChanges.PartyMenuFormChange_ChangeForm(app))
        {
            PartyMenu_TeardownFormChangeAnim(app);
            return StateConfirmItemUpdate;
        }
        return State13;
    }

    // OFFSET 0x02081594
    private static int PartyMenu_ShowItemSwapConfirmation(PartyMenuApplication app)
    {
        if (Text.Text_IsPrinterActive(app.TextPrinterID) == 0)
        {
            PartyMenu_DrawYesNoChoice(app);
            return StateHeldItemSwap;
        }
        return StateShowItemSwapConfirmation;
    }

    // OFFSET 0x020815B8
    private static int ProcessPokemonItemSwap(PartyMenuApplication app)
    {
        uint selection = Menu.Menu_ProcessInputAndHandleExit(app.ContextMenu!, HeapID);
        if (selection == 0)
        {
            PartyMenu partyMenu = app.PartyMenu!;
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
            ushort usedItem = partyMenu.UsedItemID;
            ushort heldItem = app.PartyMembers[app.CurrPartySlot].HeldItem;
            int state = UpdatePokemonWithItem(app, mon, out int form);
            if (!BagItems.Bag_TryAddItem(partyMenu.Bag!, heldItem, 1, HeapID))
            {
                SwapPokemonItem(app, mon, usedItem, heldItem);
                MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x53, app.TmpString!);
                state = StateConfirmItemUpdate;
            }
            else
            {
                if (Item.Item_IsMail(partyMenu.UsedItemID) == 1)
                {
                    BagItems.Bag_TryRemoveItem(partyMenu.Bag!, heldItem, 1, HeapID);
                    SwapPokemonItem(app, mon, usedItem, heldItem);
                    partyMenu.MenuSelectionResult = ExitCodeWriteMail;
                    return StateFadeOut;
                }
                MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x54, app.TmpFormat!);
                StringTemplate.StringTemplate_SetItemName(app.Template!, 1, heldItem);
                StringTemplate.StringTemplate_SetItemName(app.Template!, 2, usedItem);
                StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, app.TmpFormat!);
                if (usedItem != ItemGriseousOrb && heldItem == ItemGriseousOrb && form != -1)
                {
                    state = State12;
                }
                else if (usedItem == ItemGriseousOrb && heldItem == ItemGriseousOrb)
                {
                    state = StateConfirmItemUpdate;
                }
            }
            Bg.Window_FillTilemap(app.Windows[WinLongMessage], 0xF);
            PartyMenu_AddLongMessagePrinter(app);
            return state;
        }
        if (selection == Menu.Cancel)
        {
            return ResetWindowOnInput(app);
        }
        return StateHeldItemSwap;
    }

    // OFFSET 0x0208170C
    private static int ResetWindowOnInput(PartyMenuApplication app)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        if (partyMenu.Mode == ModeGiveItemDone)
        {
            RenderWindow.Window_EraseMessageBox(app.Windows[WinLongMessage], 1);
            PartyMenu_PrintShortMessage(app, TextChooseAPokemon, 1);
            SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 0);
            partyMenu.Mode = ModeField;
            return StateDefault;
        }
        partyMenu.MenuSelectionResult = ExitCodeReturnToBag;
        return StateFadeOut;
    }

    // OFFSET 0x02081760
    private static int UpdatePokemonFormWithItem(PartyMenuApplication app)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
        ushort usedItem = partyMenu.UsedItemID;
        ushort heldItem = app.PartyMembers[app.CurrPartySlot].HeldItem;
        int state = UpdatePokemonWithItem(app, mon, out int form);
        if (heldItem == ItemGriseousOrb && state == StateConfirmItemUpdate && form == 0)
        {
            state = State12;
        }
        if (heldItem == 0)
        {
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x76, app.TmpFormat!);
            StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
            StringTemplate.StringTemplate_SetItemName(app.Template!, 1, partyMenu.UsedItemID);
            StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, app.TmpFormat!);
        }
        else
        {
            BagItems.Bag_TryAddItem(partyMenu.Bag!, heldItem, 1, HeapID);
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x54, app.TmpFormat!);
            StringTemplate.StringTemplate_SetItemName(app.Template!, 1, heldItem);
            StringTemplate.StringTemplate_SetItemName(app.Template!, 2, usedItem);
            StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, app.TmpFormat!);
        }
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(app.Windows[WinLongMessage], 1, 0xA, 0xF);
        Bg.Window_FillTilemap(app.Windows[WinLongMessage], 0xF);
        PartyMenu_AddLongMessagePrinter(app);
        if (partyMenu.Mode == ModeGiveMailDone)
        {
            partyMenu.Mode = ModeGiveItemDone;
        }
        return state;
    }

    // OFFSET 0x0208188C
    private static int CheckForItemApplication(PartyMenuApplication app)
    {
        if (app.PartyMembers[app.CurrPartySlot].BallSeal == 0)
        {
            app.PartyMenu!.MenuSelectionResult = 0;
            return StateFadeOut;
        }
        SpriteSystem.Sprite_SetExplicitPalette2(app.Sprites[SpriteCursorNormal]!, 1);
        PartyMenu_PrintLongMessage(app, 0xC3, 1);
        app.OnYes = PartyMenuCB_RemoveBallCapsule_Yes;
        app.OnNo = PartyMenuCB_RemoveBallCapsule_No;
        app.StateAfterMessage = StateDrawYesNoChoice;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x020818F8
    private static bool ShouldShowSubscreen(PartyMenuApplication app)
    {
        switch (app.PartyMenu!.Mode)
        {
            case ModeUseItem:
            case ModeTeachMove:
            case ModeTeachMoveDone:
            case ModeLevelMoveDone:
            case ModeGiveItem:
            case ModeGiveMail:
            case ModeMailbox:
            case ModeUseEvoItem:
            case ModeFeedPoffin:
                return false;
        }
        return true;
    }

    // OFFSET 0x02081940
    private static void PartyMenu_LoadMemberPanelTilemaps(int heapID, byte[] lead, byte[] back, byte[] none)
    {
        byte[] file = NARC.NARC_AllocAndReadWholeMemberByIndexPair(PartyMenuArchive, PanelTilemapMember, heapID);
        G2d.GetUnpackedScreenData(file, out ScreenData? screen);
        byte[] raw = screen!.RawData;
        for (int row = 0; row < 6; row++)
        {
            Array.Copy(raw, row * 0x40, lead, row * 0x20, 0x20);
            Array.Copy(raw, (row + 6) * 0x40, back, row * 0x20, 0x20);
            Array.Copy(raw, (row + 0xC) * 0x40, none, row * 0x20, 0x20);
        }
    }

    // OFFSET 0x020819B4
    public static void PartyMenu_SetupFormChangeAnim(PartyMenuApplication app)
    {
        GameOverlay.Overlay_LoadByID(FormChangeOverlay, 2);
        PartyMenuFormChanges.PartyMenuFormChange_Init(app);
    }

    // OFFSET 0x020819CC
    public static void PartyMenu_TeardownFormChangeAnim(PartyMenuApplication app)
    {
        GameOverlay.Overlay_UnloadByID(FormChangeOverlay);
    }
}
