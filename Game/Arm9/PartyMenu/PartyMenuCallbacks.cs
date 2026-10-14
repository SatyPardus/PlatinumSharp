using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public static partial class PartyMenus
{
    private const int ItemParamPartyUse = 0xE;
    private const int ItemParamReviveAll = 0x18;
    private const int ItemParamLevelUp = 0x19;
    private const int ItemParamHPRestore = 0x26;
    private const int ExitCodeOverwriteMoveTMHM = 4;
    private const int ExitCodeOverwriteMoveLevelUp = 5;
    private const int ExitCodeEvolveByLevel = 9;
    private const int FriendshipEventTMHM = 4;
    private const int MonDataStatus = 0xA0;
    private const int MonDataFriendship = 9;
    private const int MonDataHPEV = 0xD;
    private const int MonDataMove1PPUps = 0x3E;
    private const int MonDataMove1PP = 0x3A;
    private const int MoveSlotNone = 4;
    private const int LearnMoveFull = 0xFFFF;
    private const int LearnMoveAlreadyKnown = 0xFFFE;

    // OFFSET 0x02084B70
    private static int NormalizeItemEffect(ushort item)
    {
        byte[] itemData = Item.Item_Load(item, 0, HeapID)!;
        if (Item.Item_Get(itemData, 0, ItemParamPartyUse) != 1)
        {
            return 0x1C;
        }
        for (int param = 0x1B; param <= 0x21; param++)
        {
            if (Item.Item_Get(itemData, 0, param) != 0)
            {
                return 0;
            }
        }
        if (Item.Item_Get(itemData, 0, ItemParamReviveAll) != 0)
        {
            return 1;
        }
        if (Item.Item_Get(itemData, 0, ItemParamLevelUp) != 0)
        {
            return 2;
        }
        int cures = 0;
        for (int i = 0; i < 6; i++)
        {
            cures += Item.Item_Get(itemData, 0, 0xF + i) << i;
        }
        switch (cures)
        {
            case 1:
                return 3;
            case 2:
                return 4;
            case 4:
                return 5;
            case 8:
                return 6;
            case 0x10:
                return 7;
            case 0x20:
                return 8;
            case 0x3F:
                return Item.Item_Get(itemData, 0, ItemParamHPRestore) != 0 ? 0xB : 9;
        }
        if (Item.Item_Get(itemData, 0, 0x15) != 0)
        {
            return 0xA;
        }
        if (Item.Item_Get(itemData, 0, ItemParamHPRestore) != 0)
        {
            return 0xB;
        }
        for (int stat = 0; stat < 6; stat++)
        {
            int change = Item.Item_Get(itemData, 0, 0x30 + stat);
            if (change > 0)
            {
                return 0xC + stat;
            }
            if (change < 0)
            {
                return 0x12 + stat;
            }
        }
        if (Item.Item_Get(itemData, 0, ItemParamEvolve) != 0)
        {
            return 0x18;
        }
        if (Item.Item_Get(itemData, 0, ItemParamPPUp) != 0)
        {
            return 0x19;
        }
        if (Item.Item_Get(itemData, 0, ItemParamPPMax) != 0)
        {
            return 0x1A;
        }
        if (Item.Item_Get(itemData, 0, ItemParamPPRestore) != 0 || Item.Item_Get(itemData, 0, ItemParamPPRestoreAll) != 0)
        {
            return 0x1B;
        }
        return 0x1C;
    }

    private static void FormatTmpString(PartyMenuApplication app, uint message)
    {
        GameString format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, message)!;
        StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, format);
        GameString.String_Free(format);
    }

    // OFFSET 0x02084E58
    private static void BufferUsedItemMessage(PartyMenuApplication app, ushort item, int value)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, app.CurrPartySlot);
        StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
        int effect = NormalizeItemEffect(item);
        switch (effect)
        {
            case 3:
                FormatTmpString(app, 0x5B);
                return;
            case 4:
                FormatTmpString(app, 0x41);
                return;
            case 5:
                FormatTmpString(app, 0x43);
                return;
            case 6:
                FormatTmpString(app, 0x44);
                return;
            case 7:
                FormatTmpString(app, 0x42);
                return;
            case 8:
                FormatTmpString(app, 0x49);
                return;
            case 9:
                FormatTmpString(app, 0x47);
                return;
            case 10:
                FormatTmpString(app, 0x4A);
                return;
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
            case 17:
                StringTemplate.StringTemplate_SetPokemonStatName(app.Template!, 1, (uint)(effect - 12));
                FormatTmpString(app, 0x4B);
                return;
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
                StringTemplate.StringTemplate_SetPokemonStatName(app.Template!, 1, (uint)(effect - 18));
                FormatTmpString(app, (uint)(value + 0x6E));
                return;
            case 25:
            case 26:
                StringTemplate.StringTemplate_SetMoveName(app.Template!, 0, (uint)value);
                FormatTmpString(app, 0x48);
                return;
            case 27:
                MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x45, app.TmpString!);
                return;
        }
        MessageLoader.MessageLoader_GetString(app.MessageLoader!, TextItWontHaveAnyEffect, app.TmpString!);
    }

    // OFFSET 0x020852B8
    private static void PartyMenu_SetItemUseCallback(PartyMenuApplication app)
    {
        switch (NormalizeItemEffect(app.PartyMenu!.UsedItemID))
        {
            case 2:
                app.Callback = PartyMenuCB_UseItem_RareCandy;
                break;
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
            case 17:
            case 25:
            case 26:
            case 27:
                app.Callback = PartyMenuCB_UseItem_Basic;
                break;
            case 11:
                app.Callback = PartyMenuCB_UseItem_RestoreHP;
                break;
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
                app.Callback = PartyMenuCB_UseItem_SubtractEVs;
                break;
        }
    }

    // OFFSET 0x02085348
    private static int PartyMenuCB_PrintThenWaitABPress(PartyMenuApplication app)
    {
        if (Text.Text_IsPrinterActive(app.TextPrinterID) != 0)
        {
            return StateExecCallback;
        }
        if ((gSystem.PressedKeys & (KeyA | KeyB)) != 0)
        {
            app.PartyMenu!.MenuSelectionResult = 0;
            return StateFadeOut;
        }
        return StateExecCallback;
    }

    private static void ApplyItemToCurrentMember(PartyMenuApplication app, byte moveSlot)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        ushort location = GetCurrentMapLabel(app);
        ItemEffects.Party_ApplyItemEffectsToMember(partyMenu.Party!, partyMenu.UsedItemID, app.CurrPartySlot, moveSlot, location, HeapID);
    }

    private static void RefreshCurrentMember(PartyMenuApplication app)
    {
        PartyMenu_LoadMember(app, app.CurrPartySlot);
        PartyMenu_DrawMemberPanelData(app, app.CurrPartySlot);
        PartyMenu_LoadMemberWindowTiles(app, app.CurrPartySlot);
        PartyMenu_DrawMemberStatusCondition(app, app.CurrPartySlot, (byte)(app.PartyMembers[app.CurrPartySlot].StatusIcon & 0xFFF));
    }

    // OFFSET 0x02085384
    private static int PartyMenuCB_UseItem_Basic(PartyMenuApplication app)
    {
        ApplyItemToCurrentMember(app, 0);
        RefreshCurrentMember(app);
        BufferUsedItemMessage(app, app.PartyMenu!.UsedItemID, 0);
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
        SoundPlayback.Sound_PlayEffect(SeqHPTransfer);
        app.Callback = PartyMenuCB_PrintThenWaitABPress;
        return StateExecCallback;
    }

    // OFFSET 0x02085424
    private static int PartyMenuCB_UseItem_SubtractEVs(PartyMenuApplication app)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, app.CurrPartySlot);
        byte[] evs = new byte[6];
        for (int i = 0; i < 6; i++)
        {
            evs[i] = (byte)Pokemon.Pokemon_GetValue(mon, MonDataHPEV + i, null);
        }
        byte friendship = (byte)Pokemon.Pokemon_GetValue(mon, MonDataFriendship, null);
        ApplyItemToCurrentMember(app, 0);
        RefreshCurrentMember(app);
        bool unchanged = true;
        for (int i = 0; i < 6; i++)
        {
            if (evs[i] != Pokemon.Pokemon_GetValue(mon, MonDataHPEV + i, null))
            {
                unchanged = false;
                break;
            }
        }
        if (unchanged)
        {
            BufferUsedItemMessage(app, app.PartyMenu.UsedItemID, 2);
        }
        else if (friendship != Pokemon.Pokemon_GetValue(mon, MonDataFriendship, null))
        {
            BufferUsedItemMessage(app, app.PartyMenu.UsedItemID, 0);
        }
        else
        {
            BufferUsedItemMessage(app, app.PartyMenu.UsedItemID, 1);
        }
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
        app.Callback = PartyMenuCB_PrintThenWaitABPress;
        return StateExecCallback;
    }

    private static void UpdateRevivedStatus(PartyMenuApplication app, Pokemon mon)
    {
        int status = Pokemon.Pokemon_GetSummaryStatus(mon);
        PartyMenu_DrawMemberStatusCondition(app, app.CurrPartySlot, (byte)status);
        if (status == StatusNone)
        {
            app.PartyMembers[app.CurrPartySlot].StatusIcon = StatusNone;
            PartyMenu_PrintMemberLevel(app, app.CurrPartySlot);
        }
        PartyMenu_UpdateSlotPalette(app, app.CurrPartySlot);
    }

    // OFFSET 0x020855C4
    private static int PartyMenuCB_UseItem_RestoreHP(PartyMenuApplication app)
    {
        ApplyItemToCurrentMember(app, 0);
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, app.CurrPartySlot);
        uint newHP = Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null);
        PartyMenuMember member = app.PartyMembers[app.CurrPartySlot];
        GameString format;
        if (member.CurHP == 0)
        {
            format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, 0x46)!;
        }
        else if (newHP == member.CurHP)
        {
            format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, 0x47)!;
        }
        else
        {
            format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, 0x40)!;
        }
        StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
        StringTemplate.StringTemplate_SetNumber(app.Template!, 1, (int)newHP - member.CurHP, 3, 0, 1);
        StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, format);
        GameString.String_Free(format);
        UpdateRevivedStatus(app, mon);
        app.Callback = PartyMenuCB_UpdateHPBar;
        SoundPlayback.Sound_PlayEffect(SeqHPTransfer);
        return StateExecCallback;
    }

    private static void StepMemberHPDisplay(PartyMenuApplication app, int slot)
    {
        PartyMenu_ClearMemberHP(app, slot);
        Bg.Window_FillTilemap(MemberWindow(app, slot, WinHealthbarOffset), 0);
        PartyMenu_PrintMemberCurrentHP(app, slot);
        PartyMenu_DrawMemberHealthbar(app, slot);
    }

    // OFFSET 0x02085704
    private static int PartyMenuCB_UpdateHPBar(PartyMenuApplication app)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, app.CurrPartySlot);
        uint newHP = Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null);
        PartyMenuMember member = app.PartyMembers[app.CurrPartySlot];
        if (newHP != member.CurHP)
        {
            member.CurHP++;
        }
        StepMemberHPDisplay(app, app.CurrPartySlot);
        if (newHP == member.CurHP)
        {
            PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
            app.Callback = PartyMenuCB_PrintThenWaitABPress;
        }
        return StateExecCallback;
    }

    // OFFSET 0x020857A8
    private static bool CheckItemSacredAsh(ushort item)
    {
        return Item.Item_LoadParam(item, ItemParamReviveAll, HeapID) != 0;
    }

    // OFFSET 0x020857C0
    private static byte FindNextFaintedMember(PartyMenuApplication app, int start)
    {
        if (start >= MaxPartySize)
        {
            return 0xFF;
        }
        for (int i = start; i < MaxPartySize; i++)
        {
            PartyMenuMember member = app.PartyMembers[i];
            if (member.IsPresent != 0 && member.CurHP == 0)
            {
                return (byte)i;
            }
        }
        return 0xFF;
    }

    // OFFSET 0x02085804
    private static int PartyMenuCB_HandleSacredAsh(PartyMenuApplication app)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        switch (app.StateAfterMessage)
        {
            case 0:
                app.CurrPartySlot = FindNextFaintedMember(app, 0);
                if (app.CurrPartySlot == 0xFF)
                {
                    MessageLoader.MessageLoader_GetString(app.MessageLoader!, TextItWontHaveAnyEffect, app.TmpString!);
                    PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
                    app.Callback = PartyMenuCB_PrintThenWaitABPress;
                    PartyMenu_UpdateCursor(app, 0, 1);
                    app.CurrPartySlot = SlotCancel;
                    return StateExecCallback;
                }
                if (app.CurrPartySlot != 0)
                {
                    PartyMenu_UpdateSlotPalette(app, 0);
                }
                goto case 1;
            case 1:
            {
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
                ItemEffects.Pokemon_ApplyItemEffects(mon, partyMenu.UsedItemID, 0, GetCurrentMapLabel(app), HeapID);
                Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null);
                GameString format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, 0x46)!;
                StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
                StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, format);
                GameString.String_Free(format);
                app.PartyMembers[app.CurrPartySlot].StatusIcon = StatusNone;
                PartyMenu_DrawMemberStatusCondition(app, app.CurrPartySlot, (byte)app.PartyMembers[app.CurrPartySlot].StatusIcon);
                PartyMenu_PrintMemberLevel(app, app.CurrPartySlot);
                PartyMenu_UpdateSlotPalette(app, app.CurrPartySlot);
                PartyMenu_UpdateCursor(app, app.CurrPartySlot, 1);
                SoundPlayback.Sound_PlayEffect(SeqHPTransfer);
                app.StateAfterMessage++;
                break;
            }
            case 2:
            {
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
                uint newHP = Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null);
                PartyMenuMember member = app.PartyMembers[app.CurrPartySlot];
                member.CurHP++;
                StepMemberHPDisplay(app, app.CurrPartySlot);
                if (newHP == member.CurHP)
                {
                    PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
                    app.StateAfterMessage++;
                }
                break;
            }
            case 3:
                if (Text.Text_IsPrinterActive(app.TextPrinterID) == 0 && (gSystem.PressedKeys & (KeyA | KeyB)) != 0)
                {
                    SoundPlayback.Sound_PlayEffect(SeqConfirm);
                    byte previous = app.CurrPartySlot;
                    app.CurrPartySlot = FindNextFaintedMember(app, previous + 1);
                    if (app.CurrPartySlot != 0xFF)
                    {
                        RenderWindow.Window_EraseMessageBox(app.Windows[WinLongMessage], 0);
                        PartyMenu_UpdateSlotPalette(app, previous);
                        app.StateAfterMessage = 1;
                    }
                    else
                    {
                        BagItems.Bag_TryRemoveItem(partyMenu.Bag!, partyMenu.UsedItemID, 1, HeapID);
                        partyMenu.MenuSelectionResult = 0;
                        app.CurrPartySlot = SlotCancel;
                        return StateFadeOut;
                    }
                }
                break;
        }
        return StateUseSacredAsh;
    }

    // OFFSET 0x02085A70
    private static int PartyMenuCB_UseItem_RareCandy(PartyMenuApplication app)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, app.CurrPartySlot);
        int[] statParams = { 0xA4, 0xA5, 0xA6, 0xA8, 0xA9, 0xA7 };
        for (int i = 0; i < 6; i++)
        {
            app.MonStats[i] = (ushort)Pokemon.Pokemon_GetValue(mon, statParams[i], null);
        }
        ApplyItemToCurrentMember(app, 0);
        PartyMenuMember member = app.PartyMembers[app.CurrPartySlot];
        member.Level = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataLevel, null);
        member.CurHP = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null);
        member.MaxHP = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMaxHP, null);
        GameString format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, 0xC1)!;
        StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
        StringTemplate.StringTemplate_SetNumber(app.Template!, 1, member.Level, 3, 0, 1);
        StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, format);
        GameString.String_Free(format);
        UpdateRevivedStatus(app, mon);
        app.Callback = PartyMenuCB_UpdateHPBar;
        PartyMenu_DrawMemberPanelData(app, app.CurrPartySlot);
        PartyMenu_LoadMemberWindowTiles(app, app.CurrPartySlot);
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
        app.Callback = PartyMenuCB_LevelUp;
        app.CallbackState = 0;
        return StateExecCallback;
    }

    private static bool ABPressedWithSound(PartyMenuApplication app)
    {
        if ((gSystem.PressedKeys & (KeyA | KeyB)) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqConfirm);
            return true;
        }
        return false;
    }

    // OFFSET 0x02085C50
    private static int PartyMenuCB_LevelUp(PartyMenuApplication app)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        switch (app.CallbackState)
        {
            case 0:
                if (Text.Text_IsPrinterActive(app.TextPrinterID) == 0 && ABPressedWithSound(app))
                {
                    PartyMenu_DrawLevelUpStatIncreases(app);
                    app.CallbackState = 1;
                }
                break;
            case 1:
                if (ABPressedWithSound(app))
                {
                    PartyMenu_DrawLevelUpNewStatValues(app);
                    app.CallbackState = 2;
                }
                break;
            case 2:
                if (ABPressedWithSound(app))
                {
                    PartyMenu_RemoveContextWindow(app);
                    app.CallbackState = 3;
                    partyMenu.LevelUpMoveIndex = 0;
                }
                break;
            case 3:
            {
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
                ushort result = Pokemon.Pokemon_LevelUpMove(mon, ref partyMenu.LevelUpMoveIndex, ref partyMenu.LearnedMove);
                if (result == 0)
                {
                    app.CallbackState = 6;
                    break;
                }
                if (result == LearnMoveAlreadyKnown)
                {
                    break;
                }
                StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
                StringTemplate.StringTemplate_SetMoveName(app.Template!, 1, partyMenu.LearnedMove);
                if (result == LearnMoveFull)
                {
                    FormatTmpString(app, 0x34);
                    PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 0);
                    app.OnYes = PartyMenuCB_LevelMove_ForgetMove;
                    app.OnNo = PartyMenuCB_LevelMove_PromptStopTrying;
                    app.StateAfterMessage = StateDrawYesNoChoice;
                    return StateShowMessageThenNextState;
                }
                FormatTmpString(app, 0xC2);
                PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 0);
                app.CallbackState = 4;
                break;
            }
            case 4:
                if (Text.Text_IsPrinterActive(app.TextPrinterID) == 0 && ABPressedWithSound(app))
                {
                    app.CallbackState = 3;
                }
                break;
            case 5:
                if ((gSystem.PressedKeys & (KeyA | KeyB)) != 0)
                {
                    Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
                    TeachMove(app, mon, partyMenu.SelectedMoveSlot);
                    GameString format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, 0x3D)!;
                    StringTemplate.StringTemplate_SetMoveName(app.Template!, 1, partyMenu.LearnedMove);
                    StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, format);
                    GameString.String_Free(format);
                    PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 0);
                    app.CallbackState = 4;
                }
                break;
            case 6:
            {
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
                int method = MapHeader.MapHeader_GetMapEvolutionMethod(CurrentMapID(partyMenu.FieldSystem));
                partyMenu.EvoTargetSpecies = (ushort)Pokemon.Pokemon_GetEvolutionTargetSpecies(partyMenu.Party, mon, 0, (ushort)method, out partyMenu.EvoType);
                partyMenu.MenuSelectionResult = (byte)(partyMenu.EvoTargetSpecies != 0 ? ExitCodeEvolveByLevel : 0);
                return StateFadeOut;
            }
        }
        return StateExecCallback;
    }

    private static int ExitAfterMoveChoice(PartyMenuApplication app, PartyMenuCallback promptStopTrying, int stateAfterMessage, bool setLevelUpCallback)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
        StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
        if (partyMenu.SelectedMoveSlot == MoveSlotNone)
        {
            StringTemplate.StringTemplate_SetMoveName(app.Template!, 1, partyMenu.LearnedMove);
            return promptStopTrying(app);
        }
        StringTemplate.StringTemplate_SetMoveName(app.Template!, 1, Pokemon.Pokemon_GetValue(mon, MonDataMove1 + partyMenu.SelectedMoveSlot, null));
        FormatTmpString(app, 0x3C);
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
        app.StateAfterMessage = (byte)stateAfterMessage;
        if (setLevelUpCallback)
        {
            app.CallbackState = 5;
        }
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x02085EF4
    private static int PartyMenuCB_LevelMove_Exit(PartyMenuApplication app)
    {
        app.Callback = PartyMenuCB_LevelUp;
        app.CallbackState = 3;
        return ExitAfterMoveChoice(app, PartyMenuCB_LevelMove_PromptStopTrying, StateExecCallback, true);
    }

    // OFFSET 0x02085FB4
    private static int PartyMenuCB_LevelMove_ForgetMove(PartyMenuApplication app)
    {
        FormatTmpString(app, 0x3B);
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 0);
        app.PartyMenu!.MenuSelectionResult = ExitCodeOverwriteMoveLevelUp;
        app.StateAfterMessage = StateWaitABPressBeforeFade;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x02086008
    private static int PartyMenuCB_LevelMove_PromptStopTrying(PartyMenuApplication app)
    {
        FormatTmpString(app, 0x37);
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
        app.OnYes = PartyMenuCB_LevelMove_StopTrying;
        app.OnNo = PartyMenuCB_LevelMove_WantsToLearn;
        app.StateAfterMessage = StateDrawYesNoChoice;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x02086060
    private static int PartyMenuCB_LevelMove_StopTrying(PartyMenuApplication app)
    {
        FormatTmpString(app, 0x3A);
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 0);
        app.StateAfterMessage = StateExecCallback;
        app.CallbackState = 4;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x020860AC
    private static int PartyMenuCB_LevelMove_WantsToLearn(PartyMenuApplication app)
    {
        FormatTmpString(app, 0x34);
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 0);
        app.OnYes = PartyMenuCB_LevelMove_ForgetMove;
        app.OnNo = PartyMenuCB_LevelMove_PromptStopTrying;
        app.StateAfterMessage = StateDrawYesNoChoice;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x02086104
    private static byte PartyMenu_TeachMove_Check(PartyMenuApplication app, Pokemon mon)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        int slot;
        for (slot = 0; slot < 4; slot++)
        {
            ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + slot, null);
            if (move == partyMenu.LearnedMove)
            {
                return 0xFD;
            }
            if (move == 0)
            {
                break;
            }
        }
        if (!Pokemon.Pokemon_CanLearnTM(mon, Item.Item_TMHMNumber(partyMenu.UsedItemID)))
        {
            return 0xFF;
        }
        if (slot == 4)
        {
            return 0xFE;
        }
        return (byte)slot;
    }

    private static int FinishTeachMove(PartyMenuApplication app, uint message)
    {
        FormatTmpString(app, message);
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
        app.PartyMenu!.MenuSelectionResult = 0;
        app.StateAfterMessage = StateWaitABPressBeforeFade;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x0208615C
    private static int PartyMenuCB_TeachMove(PartyMenuApplication app)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
        byte result = PartyMenu_TeachMove_Check(app, mon);
        StringTemplate.StringTemplate_SetNickname(app.Template!, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
        StringTemplate.StringTemplate_SetMoveName(app.Template!, 1, partyMenu.LearnedMove);
        if (result <= 3)
        {
            TeachMove(app, mon, result);
            return FinishTeachMove(app, 0x3D);
        }
        if (result == 0xFD)
        {
            return FinishTeachMove(app, 0x3F);
        }
        if (result == 0xFE)
        {
            FormatTmpString(app, 0x34);
            PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
            app.OnYes = PartyMenuCB_TeachMove_ForgetMove;
            app.OnNo = PartyMenuCB_TeachMove_PromptStopTrying;
            app.StateAfterMessage = StateDrawYesNoChoice;
            return StateShowMessageThenNextState;
        }
        if (result == 0xFF)
        {
            return FinishTeachMove(app, 0x3E);
        }
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x020862F8
    private static int PartyMenuCB_TeachMove_Exit(PartyMenuApplication app)
    {
        return ExitAfterMoveChoice(app, PartyMenuCB_TeachMove_PromptStopTrying, StateTeachMoveHandleInput, false);
    }

    // OFFSET 0x020863A0
    private static int PartyMenuCB_TeachMove_HandleInput(PartyMenuApplication app)
    {
        if ((gSystem.PressedKeys & (KeyA | KeyB)) == 0)
        {
            return StateTeachMoveHandleInput;
        }
        PartyMenu partyMenu = app.PartyMenu!;
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
        TeachMove(app, mon, partyMenu.SelectedMoveSlot);
        GameString format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, 0x3D)!;
        StringTemplate.StringTemplate_SetMoveName(app.Template!, 1, partyMenu.LearnedMove);
        StringTemplate.StringTemplate_Format(app.Template!, app.TmpString!, format);
        GameString.String_Free(format);
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 0);
        partyMenu.MenuSelectionResult = 0;
        app.StateAfterMessage = StateWaitABPressBeforeFade;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x02086438
    private static int PartyMenuCB_TeachMove_ForgetMove(PartyMenuApplication app)
    {
        FormatTmpString(app, 0x3B);
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 0);
        app.PartyMenu!.MenuSelectionResult = ExitCodeOverwriteMoveTMHM;
        app.StateAfterMessage = StateWaitABPressBeforeFade;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x0208648C
    private static int PartyMenuCB_TeachMove_PromptStopTrying(PartyMenuApplication app)
    {
        FormatTmpString(app, 0x37);
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
        app.OnYes = PartyMenuCB_TeachMove_StopTrying;
        app.OnNo = PartyMenuCB_TeachMove_WantsToLearn;
        app.StateAfterMessage = StateDrawYesNoChoice;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x020864E4
    private static int PartyMenuCB_TeachMove_StopTrying(PartyMenuApplication app)
    {
        FormatTmpString(app, 0x3A);
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 0);
        app.PartyMenu!.MenuSelectionResult = 0;
        app.StateAfterMessage = StateWaitABPressBeforeFade;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x02086538
    private static int PartyMenuCB_TeachMove_WantsToLearn(PartyMenuApplication app)
    {
        FormatTmpString(app, 0x34);
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 0);
        app.OnYes = PartyMenuCB_TeachMove_ForgetMove;
        app.OnNo = PartyMenuCB_TeachMove_PromptStopTrying;
        app.StateAfterMessage = StateDrawYesNoChoice;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x02086590
    private static void TeachMove(PartyMenuApplication app, Pokemon mon, int slot)
    {
        PartyMenu partyMenu = app.PartyMenu!;
        Pokemon.Pokemon_SetValue(mon, MonDataMove1 + slot, (uint)partyMenu.LearnedMove);
        Pokemon.Pokemon_SetValue(mon, MonDataMove1PPUps + slot, 0u);
        Pokemon.Pokemon_SetValue(mon, MonDataMove1PP + slot, MoveTable.MoveTable_CalcMaxPP(partyMenu.LearnedMove, 0));
        if (partyMenu.UsedItemID != 0)
        {
            if (Item.Item_IsHMMove(partyMenu.LearnedMove) == 0)
            {
                BagItems.Bag_TryRemoveItem(partyMenu.Bag!, partyMenu.UsedItemID, 1, HeapID);
            }
            Pokemon.Pokemon_UpdateFriendship(mon, FriendshipEventTMHM, GetCurrentMapLabel(app));
        }
    }

    // OFFSET 0x02086614
    private static int BufferLearnedMoveInSlot(PartyMenuApplication app, int slot)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(app.PartyMenu!.Party!, app.CurrPartySlot);
        ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + slot, null);
        GameString format = MessageLoader.MessageLoader_GetNewString(app.MessageLoader!, (uint)(slot + 0xA2))!;
        StringTemplate.StringTemplate_SetMoveName(app.Template!, 0, move);
        StringTemplate.StringTemplate_Format(app.Template!, app.TmpFormat!, format);
        GameString.String_Free(format);
        if (move == 0)
        {
            StringList.StringList_AddFromString(app.ContextMenuChoices!, app.TmpFormat, Menu.Header);
            return 0;
        }
        StringList.StringList_AddFromString(app.ContextMenuChoices!, app.TmpFormat, (uint)slot);
        return 1;
    }

    // OFFSET 0x020866A0
    private static void PartyMenu_MoveSelection_Display(PartyMenuApplication app, int restorePP)
    {
        PartyMenu_PrintMediumMessage(app, restorePP == 0 ? 0x29u : 0x28u, 1);
        app.ContextMenuChoices = StringList.StringList_New(4, HeapID);
        int count = BufferLearnedMoveInSlot(app, 0);
        count += BufferLearnedMoveInSlot(app, 1);
        count += BufferLearnedMoveInSlot(app, 2);
        count += BufferLearnedMoveInSlot(app, 3);
        MenuTemplate template = new()
        {
            Choices = app.ContextMenuChoices,
            Window = app.Windows[WinMoveList],
            FontID = 0,
            XSize = 1,
            YSize = 4,
            LineSpacing = 0,
            SuppressCursor = 0,
            LoopAround = (byte)count == 4 ? 1 : 0,
        };
        RenderWindow.Window_DrawStandardFrame(app.Windows[WinMoveList], 1, 1, 0xE);
        app.ContextMenu = Menu.Menu_NewAndCopyToVRAM(template, 8, 0, 0, HeapID, KeyB);
    }

    private static void CloseMoveList(PartyMenuApplication app)
    {
        RenderWindow.Window_EraseMessageBox(app.Windows[WinMediumMessage], 1);
        RenderWindow.Window_EraseStandardFrame(app.Windows[WinMoveList], 1);
        FreeContextMenu(app);
    }

    // OFFSET 0x02086774
    private static int PartyMenu_MoveSelection_HandleInput(PartyMenuApplication app)
    {
        uint selection = Menu.Menu_ProcessInput(app.ContextMenu!);
        if (selection == Menu.Cancel)
        {
            CloseMoveList(app);
            PartyMenu_PrintShortMessage(app, TextUseOnWhichMon, 1);
            return StateUseItem;
        }
        if (selection == Menu.NoChoice)
        {
            return StateWaitMoveListSelection;
        }
        CloseMoveList(app);
        PartyMenu partyMenu = app.PartyMenu!;
        if (ItemEffects.Party_ApplyItemEffectsToMember(partyMenu.Party!, partyMenu.UsedItemID, app.CurrPartySlot, (byte)selection, GetCurrentMapLabel(app), HeapID))
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(partyMenu.Party!, app.CurrPartySlot);
            BufferUsedItemMessage(app, partyMenu.UsedItemID, (int)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + (int)selection, null));
            BagItems.Bag_TryRemoveItem(partyMenu.Bag!, partyMenu.UsedItemID, 1, HeapID);
            SoundPlayback.Sound_PlayEffect(SeqHPTransfer);
        }
        else
        {
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, TextItWontHaveAnyEffect, app.TmpString!);
        }
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
        partyMenu.MenuSelectionResult = 0;
        app.StateAfterMessage = StateWaitABPressBeforeFade;
        return StateShowMessageThenNextState;
    }

    // OFFSET 0x020868B0
    private static void PartyMenu_GiveMail(PartyMenuApplication app)
    {
        RenderWindow.Window_EraseMessageBox(app.Windows[WinShortMessage], 1);
        if (app.PartyMembers[app.CurrPartySlot].HeldItem == 0)
        {
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x7F, app.TmpString!);
            PartyMenu_DrawMemberMail(app, app.CurrPartySlot);
        }
        else
        {
            MessageLoader.MessageLoader_GetString(app.MessageLoader!, 0x80, app.TmpString!);
            app.CurrPartySlot = SlotCancel;
        }
        PartyMenu_PrintLongMessage(app, 0xFFFFFFFF, 1);
        app.PartyMenu!.MenuSelectionResult = 0;
        app.StateAfterMessage = StateWaitABPressBeforeFade;
    }

    // OFFSET 0x02086930
    private static ushort GetCurrentMapLabel(PartyMenuApplication app)
    {
        return (ushort)MapHeader.MapHeader_GetMapLabelTextID(CurrentMapID(app.PartyMenu!.FieldSystem));
    }
}
