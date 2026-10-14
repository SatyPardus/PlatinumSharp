using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class Battlers
{
    // OFFSET 0x0225C2C4
    private static void Battler_CommandShowPartyMenu(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_InitTaskShowPartyMenu(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }
}

public static partial class BattleDisplay
{
    private const int PartyHeapIDBattle = 5;
    private const int PartyCancelSlot = 6;
    private const int PartyNoSlot = 6;
    private const uint PartyBattleTypeDoubles = 2;
    private const uint PartyBattleType2vs2 = 8;
    private const uint PartyBattleTypeMultiMask = 0xC;
    private const uint PartyBattleTypeLinkMulti = 0xCB;
    private const uint PartyBattleTypeTag = 0x10;
    private const int PartyBattlerTypeSolo = 2;
    private const int PartyBattlerTypePartner = 4;
    private const int PartyMonDataHP = 0xA3;
    private const int PartyMonDataSpeciesEgg = 0xAE;
    private const int PartySpeciesEgg = 0x1EE;

    private static SysTaskFunc PartyMenuTaskFunc(int address)
    {
        return address switch
        {
            0x02261E8C => Task_PlayerShowPartyMenu,
            0x02262258 => Task_TrainerShowPartyMenu,
            0x0226230C => Task_LinkShowPartyMenu,
            0x0226232C => Task_RecordingShowPartyMenu,
            _ => throw Missing((uint)address),
        };
    }

    // OFFSET 0x0225D840
    public static void BattleDisplay_InitTaskShowPartyMenu(BattleSystem battleSys, BattlerData battler, byte[] message)
    {
        PartyMenuData data = new();
        data.State = 0;
        data.BattleSys = battleSys;
        data.Command = message[0];
        data.Battler = message[1];
        data.ListMode = message[2];
        data.CanSwitch = (int)BattleIO.R32(message, 0x20);
        data.DoublesSelection = message[3];
        data.SelectedBattleBagItem = 0;
        data.BattlersSwitchingMask = message[0x24];
        for (int i = 0; i < 4; i++)
        {
            data.PartySlots[i] = message[4 + i];
            for (int j = 0; j < 6; j++)
            {
                data.PartyOrder[i][j] = message[8 + i * 6 + j];
            }
        }
        SysTask.SysTask_Start(PartyMenuTaskFunc(battler.TaskFuncs.ShowPartyMenu), data, 0);
    }

    private static void BattleDisplay_FillPartyMenuParty(PartyMenuData data)
    {
        BattlePartyContext partyCtx = data.BattlePartyCtx!;
        uint battleType = BattleSystems.BattleSystem_GetBattleType(data.BattleSys!);
        if ((battleType & PartyBattleTypeMultiMask) == PartyBattleTypeMultiMask || BattleSystems.BattleSystem_GetBattleType(data.BattleSys!) == PartyBattleTypeLinkMulti)
        {
            int firstBattler;
            int secondBattler;
            if (BattleSystems.BattleSystem_BattlerSlot(data.BattleSys!, data.Battler) == PartyBattlerTypeSolo)
            {
                firstBattler = data.Battler;
                secondBattler = BattleSystems.BattleSystem_GetPartner(data.BattleSys!, firstBattler);
            }
            else
            {
                firstBattler = BattleSystems.BattleSystem_GetPartner(data.BattleSys!, data.Battler);
                secondBattler = data.Battler;
            }
            Pokemon blank = Pokemon.Pokemon_New(PartyHeapIDBattle);
            for (int i = 0; i < 6; i++)
            {
                Party.Party_AddPokemon(partyCtx.Party!, blank);
            }
            for (int i = 0; i < BattleSystems.BattleSystem_GetPartyCount(data.BattleSys!, firstBattler); i++)
            {
                Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(data.BattleSys!, firstBattler, data.PartyOrder[firstBattler][i]);
                Pokemon.Pokemon_Copy(mon, Party.Party_GetPokemonBySlotIndex(partyCtx.Party!, i * 2));
                partyCtx.PokemonPartySlots[i * 2] = data.PartyOrder[firstBattler][i];
            }
            for (int i = 0; i < BattleSystems.BattleSystem_GetPartyCount(data.BattleSys!, secondBattler); i++)
            {
                Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(data.BattleSys!, secondBattler, data.PartyOrder[secondBattler][i]);
                Pokemon.Pokemon_Copy(mon, Party.Party_GetPokemonBySlotIndex(partyCtx.Party!, i * 2 + 1));
                partyCtx.PokemonPartySlots[i * 2 + 1] = data.PartyOrder[secondBattler][i];
            }
            partyCtx.SelectedPartyIndex = BattleSystems.BattleSystem_BattlerSlot(data.BattleSys!, data.Battler) == PartyBattlerTypePartner ? (byte)1 : (byte)0;
            return;
        }
        int battler;
        if ((BattleSystems.BattleSystem_GetBattleType(data.BattleSys!) & PartyBattleTypeDoubles) != 0 && (BattleSystems.BattleSystem_GetBattleType(data.BattleSys!) & PartyBattleType2vs2) == 0)
        {
            battler = data.Battler & 1;
        }
        else
        {
            battler = data.Battler;
        }
        partyCtx.SelectedPartyIndex = BattleSystems.BattleSystem_BattlerSlot(data.BattleSys!, data.Battler) == PartyBattlerTypePartner ? (byte)1 : (byte)0;
        Party party = BattleSystems.BattleSystem_GetParty(data.BattleSys!, data.Battler);
        for (int i = 0; i < Party.Party_GetCurrentCount(party); i++)
        {
            Party.Party_AddPokemon(partyCtx.Party!, BattleSystems.BattleSystem_GetPartyPokemon(data.BattleSys!, battler, data.PartyOrder[battler][i]));
            partyCtx.PokemonPartySlots[i] = data.PartyOrder[battler][i];
        }
    }

    // OFFSET 0x02261E8C
    private static void Task_PlayerShowPartyMenu(SysTask task, object? param)
    {
        PartyMenuData data = (PartyMenuData)param!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(data.BattleSys!)!;
        switch (data.State)
        {
            case 0:
            {
                Window window = BattleSystems.BattleSystem_GetWindow(data.BattleSys!, 0);
                Bg.Window_FillTilemap(window, 0xFF);
                Bg.Window_LoadTiles(window);
                data.IsCursorEnabled = BattleSubscreens.BattleSubscreen_GetSuppressActivationSfx(BattleSystems.BattleSystem_GetBattleSubscreen(data.BattleSys!)!);
                PaletteAnimator.PaletteAnimator_SetMode(BattleSystems.BattleSystem_GetPaletteAnimator(data.BattleSys!)!, 1);
                PaletteData.PaletteData_StartFade(paletteData, 5, 0xC00, -8, 0, 7, 0);
                PaletteData.PaletteData_StartFade(paletteData, 0xA, 0xFFFF, -8, 0, 0x10, 0);
                data.State++;
                break;
            }
            case 1:
            {
                if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
                {
                    break;
                }
                BattleSystems.BattleSystem_EnterSubMenu(data.BattleSys!);
                data.BattlePartyCtx = new BattlePartyContext();
                data.BattlePartyCtx.Party = Party.Party_New(PartyHeapIDBattle);
                BattleDisplay_FillPartyMenuParty(data);
                BattlePartyContext partyCtx = data.BattlePartyCtx;
                partyCtx.BattleSys = data.BattleSys;
                partyCtx.HeapID = PartyHeapIDBattle;
                partyCtx.BattlePartyExited = 0;
                partyCtx.MoveToLearn = (ushort)data.CanSwitch;
                partyCtx.DoubleBattleFirstSelectedPartySlot = data.DoublesSelection;
                partyCtx.BattlePartyMode = data.ListMode;
                partyCtx.SelectedBattleBagItem = data.SelectedBattleBagItem;
                partyCtx.Battler = data.Battler;
                partyCtx.IsCursorEnabled = data.IsCursorEnabled;
                if ((HealthBar.FlagIndex(data.Battler) & data.BattlersSwitchingMask) != 0)
                {
                    partyCtx.PlayerPokemonPartySlot = PartyNoSlot;
                }
                else
                {
                    partyCtx.PlayerPokemonPartySlot = data.PartySlots[data.Battler];
                }
                if ((BattleSystems.BattleSystem_GetBattleType(data.BattleSys!) & PartyBattleType2vs2) != 0)
                {
                    partyCtx.PartnerPokemonPartySlot = PartyNoSlot;
                }
                else if ((HealthBar.FlagIndex(BattleSystems.BattleSystem_GetPartner(data.BattleSys!, data.Battler)) & data.BattlersSwitchingMask) == 0)
                {
                    partyCtx.PartnerPokemonPartySlot = data.PartySlots[BattleSystems.BattleSystem_GetPartner(data.BattleSys!, data.Battler)];
                }
                else
                {
                    partyCtx.PartnerPokemonPartySlot = PartyNoSlot;
                }
                BattleParties.BattlePartyTask_Start(partyCtx);
                data.State++;
                break;
            }
            case 2:
                if (data.BattlePartyCtx!.BattlePartyExited == 0)
                {
                    break;
                }
                BattleSystems.BattleSystem_ExitSubMenu(data.BattleSys!);
                BattleSubscreens.BattleSubscreen_SetSuppressActivationSfx(BattleSystems.BattleSystem_GetBattleSubscreen(data.BattleSys!)!, data.BattlePartyCtx.IsCursorEnabled);
                PaletteData.PaletteData_StartFade(paletteData, 5, 0xC00, -8, 7, 0, 0);
                PaletteData.PaletteData_StartFade(paletteData, 0xA, 0xFFFF, -8, 0x10, 0, 0);
                data.State++;
                break;
            case 3:
            {
                if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
                {
                    break;
                }
                PaletteAnimator.PaletteAnimator_SetMode(BattleSystems.BattleSystem_GetPaletteAnimator(data.BattleSys!)!, 0);
                BattlePartyContext partyCtx = data.BattlePartyCtx!;
                if (partyCtx.SelectedPartyIndex == PartyCancelSlot)
                {
                    BattleIO.BattleIO_PartyMenuResult(data.BattleSys!, data.Battler, 0xFF);
                }
                else
                {
                    BattleIO.BattleIO_PartyMenuResult(data.BattleSys!, data.Battler, (uint)(partyCtx.PokemonPartySlots[partyCtx.SelectedPartyIndex] + 1));
                }
                BattleIO.BattleIO_ClearMessage(data.BattleSys!, data.Battler, data.Command);
                SysTask.SysTask_Done(task);
                break;
            }
        }
    }

    // OFFSET 0x02262258
    private static void Task_TrainerShowPartyMenu(SysTask task, object? param)
    {
        PartyMenuData data = (PartyMenuData)param!;
        uint battleType = BattleSystems.BattleSystem_GetBattleType(data.BattleSys!);
        int battler = data.Battler;
        int partner;
        if ((battleType & PartyBattleTypeTag) != 0 || (battleType & PartyBattleType2vs2) != 0)
        {
            partner = battler;
        }
        else
        {
            partner = BattleSystems.BattleSystem_GetPartner(data.BattleSys!, battler);
        }
        int slot = BattleContexts.BattleAI_SwitchedSlot(data.BattleSys!, battler);
        if (slot == PartyNoSlot)
        {
            slot = BattleContexts.BattleAI_PostKOSwitchIn(data.BattleSys!, battler);
            if (slot == PartyNoSlot)
            {
                Party party = BattleSystems.BattleSystem_GetParty(data.BattleSys!, data.Battler);
                for (slot = 0; slot < Party.Party_GetCurrentCount(party); slot++)
                {
                    Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(data.BattleSys!, data.Battler, slot);
                    if (Pokemon.Pokemon_GetValue(mon, PartyMonDataHP, null) != 0 && slot != data.PartySlots[battler] && slot != data.PartySlots[partner])
                    {
                        break;
                    }
                }
            }
        }
        BattleIO.BattleIO_PartyMenuResult(data.BattleSys!, data.Battler, (uint)(slot + 1));
        BattleIO.BattleIO_ClearMessage(data.BattleSys!, data.Battler, data.Command);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x0226230C
    private static void Task_LinkShowPartyMenu(SysTask task, object? param)
    {
        PartyMenuData data = (PartyMenuData)param!;
        BattleIO.BattleIO_ClearMessage(data.BattleSys!, data.Battler, data.Command);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x0226232C
    private static void Task_RecordingShowPartyMenu(SysTask task, object? param)
    {
        PartyMenuData data = (PartyMenuData)param!;
        if (BattleSystems.BattleSystem_ReadRecorded(data.BattleSys!, data.Battler, out byte value) == 1)
        {
            BattleDisplay_StopPlaybackDesync(data.BattleSys!);
        }
        if (value == 0 || value > 6)
        {
            BattleDisplay_StopPlaybackInvalid(data.BattleSys!);
        }
        else
        {
            int slot = value - 1;
            Party party = BattleSystems.BattleSystem_GetParty(data.BattleSys!, data.Battler);
            if (slot == data.PartySlots[data.Battler] || slot == data.DoublesSelection)
            {
                BattleDisplay_StopPlaybackInvalid(data.BattleSys!);
            }
            else if (value > Party.Party_GetCurrentCount(party))
            {
                BattleDisplay_StopPlaybackInvalid(data.BattleSys!);
            }
            else
            {
                Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(data.BattleSys!, data.Battler, value - 1);
                if (Pokemon.Pokemon_GetValue(mon, PartyMonDataHP, null) == 0)
                {
                    BattleDisplay_StopPlaybackInvalid(data.BattleSys!);
                }
                if (Pokemon.Pokemon_GetValue(mon, PartyMonDataSpeciesEgg, null) == PartySpeciesEgg)
                {
                    BattleDisplay_StopPlaybackInvalid(data.BattleSys!);
                }
            }
        }
        BattleIO.BattleIO_PartyMenuResult(data.BattleSys!, data.Battler, value);
        BattleIO.BattleIO_ClearMessage(data.BattleSys!, data.Battler, data.Command);
        SysTask.SysTask_Done(task);
    }
}
