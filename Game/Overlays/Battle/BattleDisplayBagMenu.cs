using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class PartyMenuData
{
    public BattleSystem? BattleSys;
    public BattlePartyContext? BattlePartyCtx;
    public byte Command;
    public byte Battler;
    public byte State;
    public byte ListMode;
    public readonly byte[] PartySlots = new byte[4];
    public int CanSwitch;
    public ushort SelectedBattleBagItem;
    public byte DoublesSelection;
    public byte IsCursorEnabled;
    public byte BattlersSwitchingMask;
    public readonly byte[][] PartyOrder = Enumerable.Range(0, 4).Select(_ => new byte[6]).ToArray();
}

public sealed class BagMenuData
{
    public BattleSystem? BattleSys;
    public BattleBagContext? BattleBagCtx;
    public PartyMenuData? PartyMenuData;
    public byte Command;
    public byte Battler;
    public byte State;
    public byte BattlerType;
    public byte IsCursorEnabled;
    public byte MsgIdx;
    public ushort StateAfterDelay;
    public byte HasTwoOpponents;
    public byte SemiInvulnerable;
    public byte Substitute;
    public byte Delay;
    public readonly byte[][] PartyOrder = Enumerable.Range(0, 4).Select(_ => new byte[6]).ToArray();
    public readonly byte[] EmbargoTurns = new byte[4];
}

public static partial class Battlers
{
    // OFFSET 0x0225C2B0
    private static void Battler_CommandShowBagMenu(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_InitTaskShowBagMenu(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }
}

public static partial class BattleDisplay
{
    private const int BagHeapIDBattle = 5;
    private const int BagPocketRecoverHP = 0;
    private const int BagPocketRecoverStatus = 1;
    private const int BagPocketPokeBalls = 2;
    private const int BagPocketBattleItems = 3;
    private const int BagPartyModeUseItem = 2;
    private const int BagPartyCancel = 6;
    private const int BagMsgUsedTheItem = 0x4B6;
    private const int BagMsgPokemonsStatRose = 0x4B3;
    private const int BagMsgWhiteFog = 0x4B4;
    private const int BagMsgGettingPumped = 0x4B5;
    private const int BagMsgHPRestored = 0x4BE;
    private const int BagMsgBecameHealthy = 0x4E2;
    private const int BagMsgCuredGeneric = 0x4CD;
    private const int BagTagNone = 0;
    private const int BagTagNickname = 2;
    private const int BagTagItem = 5;
    private const int BagTagNicknameStat = 0xC;
    private const int BagTagNicknameNum = 0x11;
    private const int BagItemFullRestore = 0x17;
    private const int BagItemRevive = 0x1C;
    private const int BagItemGuardSpec = 0x37;
    private const int BagItemDireHit = 0x38;
    private const int BagItemPokeDoll = 0x3F;
    private const int BagItemParamHPRestore = 0x26;
    private const int BagSeqHeal = 0x5EC;
    private const int BagPanLeft = -0x75;
    private const int BagMoveMist = 0x36;
    private const int BagMoveFocusEnergy = 0x74;
    private const int BagMessageDelay = 0x1E;
    private const uint BagBattleTypeDoubles = 2;
    private const uint BagBattleType2vs2 = 8;
    private const uint BagBattleTypeTrainerDoubles = 3;
    private const uint BagBattleTypeTagDoubles = 0x13;
    private const int BagMonDataStatus = 0xA0;
    private const int BagMonDataHP = 0xA3;
    private const int BagMonDataMaxHP = 0xA4;
    private const uint BagHealthboxInfoStatus = 0x100;
    private static readonly ushort[] BagStatusCureMessages = { 0x4BA, 0x4B7, 0x4B9, 0x4BB, 0x4B8, 0x4BC, 0x4BD };
    private static readonly byte[] BagXItemStats = { 1, 2, 3, 6, 4, 5 };

    private static SysTaskFunc BagMenuTaskFunc(int address)
    {
        return address switch
        {
            0x022611DC => Task_PlayerShowBagMenu,
            0x02261D50 => Task_TrainerShowBagMenu,
            0x02261DA8 => Task_LinkShowBagMenu,
            0x02261DE0 => Task_RecordingShowBagMenu,
            _ => throw Missing((uint)address),
        };
    }

    // OFFSET 0x0225D794
    public static void BattleDisplay_InitTaskShowBagMenu(BattleSystem battleSys, BattlerData battler, byte[] message)
    {
        BagMenuData data = new();
        data.PartyMenuData = new PartyMenuData();
        data.PartyMenuData.BattlePartyCtx = new BattlePartyContext();
        data.PartyMenuData.BattlePartyCtx.Party = Party.Party_New(BagHeapIDBattle);
        data.State = 0;
        data.BattleSys = battleSys;
        data.Command = message[0];
        data.Battler = battler.Battler;
        data.BattlerType = battler.BattlerType;
        data.HasTwoOpponents = message[1];
        data.SemiInvulnerable = message[2];
        data.Substitute = message[3];
        for (int i = 0; i < 4; i++)
        {
            data.PartyMenuData.PartySlots[i] = message[4 + i];
            for (int j = 0; j < 6; j++)
            {
                data.PartyOrder[i][j] = message[8 + i * 6 + j];
            }
            data.EmbargoTurns[i] = message[0x20 + i];
        }
        SysTask.SysTask_Start(BagMenuTaskFunc(battler.TaskFuncs.ShowBagMenu), data, 0);
    }

    private static uint BagItemUse(ushort item, byte category, byte target)
    {
        return (uint)(item | (category << 16) | (target << 24));
    }

    private static void PlayBagMoveAnimation(BagMenuData data, int transferData, int transferValue, int battler, ushort move)
    {
        byte[] moveAnim = new byte[0x58];
        BattleIO.BattleIO_SetMoveAnimation(data.BattleSys!, null, moveAnim, transferData, transferValue, battler, battler, move);
        BattleDisplay_StartMoveAnimation(data.BattleSys!, BattleSystems.BattleSystem_GetBattlerData(data.BattleSys!, battler)!, BattleSystems.BattleSystem_GetBattleAnimSystem(data.BattleSys!)!, moveAnim);
    }

    private static void PrintBagMessage(BagMenuData data, BattleMessage message)
    {
        MessageLoader msgLoader = BattleSystems.BattleSystem_GetMessageLoader(data.BattleSys!)!;
        data.MsgIdx = BattleMessages.BattleMessage_Print(data.BattleSys!, msgLoader, message, BattleMessages.BattleSystem_GetTextSpeed(data.BattleSys!));
        data.Delay = BagMessageDelay;
    }

    private static bool BagTargetIsActive(BagMenuData data)
    {
        BattlePartyContext partyCtx = data.PartyMenuData!.BattlePartyCtx!;
        if (partyCtx.SelectedPartyIndex < 2)
        {
            uint battleType = BattleSystems.BattleSystem_GetBattleType(data.BattleSys!);
            if (battleType == BagBattleTypeTrainerDoubles)
            {
                return true;
            }
            battleType = BattleSystems.BattleSystem_GetBattleType(data.BattleSys!);
            if (battleType == BagBattleTypeTagDoubles)
            {
                return true;
            }
        }
        return partyCtx.SelectedPartyIndex < 1;
    }

    private static void SetBagStateAfterDelay(BagMenuData data, ushort animatedState, ushort silentState)
    {
        if (BattleSystems.BattleSystem_AreAnimationsOn(data.BattleSys!) == 1)
        {
            data.StateAfterDelay = animatedState;
        }
        else
        {
            SoundPlayback.Sound_PlayPannedEffect(BagSeqHeal, BagPanLeft);
            data.StateAfterDelay = silentState;
        }
    }

    // OFFSET 0x022611DC
    private static void Task_PlayerShowBagMenu(SysTask task, object? param)
    {
        BagMenuData data = (BagMenuData)param!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(data.BattleSys!)!;
        switch (data.State)
        {
            case 0:
                data.IsCursorEnabled = BattleSubscreens.BattleSubscreen_GetSuppressActivationSfx(BattleSystems.BattleSystem_GetBattleSubscreen(data.BattleSys!)!);
                PaletteAnimator.PaletteAnimator_SetMode(BattleSystems.BattleSystem_GetPaletteAnimator(data.BattleSys!)!, 1);
                PaletteData.PaletteData_StartFade(paletteData, 5, 0xC00, -8, 0, 7, 0);
                PaletteData.PaletteData_StartFade(paletteData, 0xA, 0xFFFF, -8, 0, 0x10, 0);
                data.State++;
                break;
            case 1:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
                {
                    break;
                }
                BattleSystems.BattleSystem_EnterSubMenu(data.BattleSys!);
                data.BattleBagCtx = new BattleBagContext();
                data.BattleBagCtx.BattleSys = data.BattleSys;
                data.BattleBagCtx.TrainerInfo = BattleSystems.BattleSystem_GetTrainerInfo(data.BattleSys!, data.Battler);
                data.BattleBagCtx.HeapID = BagHeapIDBattle;
                data.BattleBagCtx.BattleBagExited = 0;
                data.BattleBagCtx.Bag = BattleSystems.BattleSystem_GetBag(data.BattleSys!);
                data.BattleBagCtx.Battler = data.Battler;
                data.BattleBagCtx.IsCursorEnabled = data.IsCursorEnabled;
                data.BattleBagCtx.HasTwoOpponents = data.HasTwoOpponents;
                data.BattleBagCtx.OpponentHidden = data.SemiInvulnerable;
                data.BattleBagCtx.OpponentSubstituted = data.Substitute;
                data.BattleBagCtx.EmbargoRemainingTurns = data.EmbargoTurns[data.Battler];
                BattleBags.BattleBagTask_Start(data.BattleBagCtx);
                data.State = 3;
                break;
            case 2:
                data.BattleBagCtx!.IsCursorEnabled = data.IsCursorEnabled;
                BattleBags.BattleBagTask_Start(data.BattleBagCtx);
                data.State++;
                goto case 3;
            case 3:
            {
                BattleBagContext bagCtx = data.BattleBagCtx!;
                if (bagCtx.BattleBagExited == 0)
                {
                    break;
                }
                bagCtx.BattleBagExited = 0;
                data.IsCursorEnabled = bagCtx.IsCursorEnabled;
                if (bagCtx.SelectedBattleBagItem == 0)
                {
                    data.State = 6;
                    break;
                }
                switch (bagCtx.SelectedBattleBagPocket)
                {
                    case BagPocketRecoverHP:
                    case BagPocketRecoverStatus:
                        data.State = 4;
                        break;
                    case BagPocketPokeBalls:
                    case BagPocketBattleItems:
                        data.State = 6;
                        break;
                }
                break;
            }
            case 4:
            {
                Party party = BattleSystems.BattleSystem_GetParty(data.BattleSys!, data.Battler);
                int battler;
                if ((BattleSystems.BattleSystem_GetBattleType(data.BattleSys!) & BagBattleTypeDoubles) != 0 && (BattleSystems.BattleSystem_GetBattleType(data.BattleSys!) & BagBattleType2vs2) == 0)
                {
                    battler = data.Battler & 1;
                }
                else
                {
                    battler = data.Battler;
                }
                PartyMenuData partyMenuData = data.PartyMenuData!;
                BattlePartyContext partyCtx = partyMenuData.BattlePartyCtx!;
                Party.Party_InitWithCapacity(partyCtx.Party!, 6);
                for (int i = 0; i < Party.Party_GetCurrentCount(party); i++)
                {
                    Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(data.BattleSys!, battler, data.PartyOrder[battler][i]);
                    Party.Party_AddPokemon(partyCtx.Party!, mon);
                    partyCtx.PokemonPartySlots[i] = data.PartyOrder[battler][i];
                }
                partyCtx.BattleSys = data.BattleSys;
                partyCtx.HeapID = BagHeapIDBattle;
                partyCtx.SelectedPartyIndex = 0;
                partyCtx.BattlePartyExited = 0;
                partyCtx.MoveToLearn = 0;
                partyCtx.BattlePartyMode = BagPartyModeUseItem;
                partyCtx.SelectedBattleBagItem = data.BattleBagCtx!.SelectedBattleBagItem;
                partyCtx.SelectedBattleBagPocket = data.BattleBagCtx.SelectedBattleBagPocket;
                partyCtx.Battler = data.BattleBagCtx.Battler;
                partyCtx.IsCursorEnabled = data.IsCursorEnabled;
                partyCtx.PlayerPokemonPartySlot = partyMenuData.PartySlots[data.Battler];
                partyCtx.PartnerPokemonPartySlot = partyMenuData.PartySlots[BattleSystems.BattleSystem_GetPartner(data.BattleSys!, data.Battler)];
                if (data.BattlerType == 4)
                {
                    partyCtx.EmbargoRemainingTurns[0] = data.EmbargoTurns[BattleSystems.BattleSystem_GetPartner(data.BattleSys!, data.Battler)];
                    partyCtx.EmbargoRemainingTurns[1] = data.EmbargoTurns[data.Battler];
                }
                else
                {
                    partyCtx.EmbargoRemainingTurns[0] = data.EmbargoTurns[data.Battler];
                    partyCtx.EmbargoRemainingTurns[1] = data.EmbargoTurns[BattleSystems.BattleSystem_GetPartner(data.BattleSys!, data.Battler)];
                }
                BattleParties.BattlePartyTask_Start(partyCtx);
                data.State++;
                break;
            }
            case 5:
            {
                BattlePartyContext partyCtx = data.PartyMenuData!.BattlePartyCtx!;
                if (partyCtx.BattlePartyExited == 0)
                {
                    break;
                }
                data.IsCursorEnabled = partyCtx.IsCursorEnabled;
                partyCtx.BattlePartyExited = 0;
                data.State = partyCtx.SelectedPartyIndex == BagPartyCancel ? (byte)2 : (byte)6;
                break;
            }
            case 6:
                BattleSystems.BattleSystem_ExitSubMenu(data.BattleSys!);
                BattleSubscreens.BattleSubscreen_SetSuppressActivationSfx(BattleSystems.BattleSystem_GetBattleSubscreen(data.BattleSys!)!, data.IsCursorEnabled);
                PaletteData.PaletteData_StartFade(paletteData, 5, 0xC00, -8, 7, 0, 0);
                PaletteData.PaletteData_StartFade(paletteData, 0xA, 0xFFFF, -8, 0x10, 0, 0);
                data.State++;
                break;
            case 7:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
                {
                    break;
                }
                BagMenuSelectUseState(data);
                break;
            case 8:
            {
                BattleBagContext bagCtx = data.BattleBagCtx!;
                uint used;
                if (bagCtx.SelectedBattleBagItem == 0)
                {
                    used = BagItemUse(0xFF, 0, 0);
                }
                else
                {
                    byte target = 0;
                    if (bagCtx.SelectedBattleBagPocket <= BagPocketRecoverStatus)
                    {
                        BattlePartyContext partyCtx = data.PartyMenuData!.BattlePartyCtx!;
                        target = (byte)(partyCtx.PokemonPartySlots[partyCtx.SelectedPartyIndex] + 1);
                    }
                    used = BagItemUse(bagCtx.SelectedBattleBagItem, bagCtx.SelectedBattleBagPocket, target);
                }
                BattleIO.BattleIO_BagResult(data.BattleSys!, data.Battler, used);
                BattleIO.BattleIO_ClearMessage(data.BattleSys!, data.Battler, data.Command);
                SysTask.SysTask_Done(task);
                break;
            }
            case 9:
            {
                BattleMessage message = new();
                message.Id = BagMsgUsedTheItem;
                message.Tags = BagTagItem;
                message.Params[0] = data.BattleBagCtx!.SelectedBattleBagItem;
                PrintBagMessage(data, message);
                data.State++;
                break;
            }
            case 10:
                if (Text.Text_IsPrinterActive(data.MsgIdx) != 0)
                {
                    break;
                }
                data.Delay--;
                if (data.Delay == 0)
                {
                    data.State = (byte)data.StateAfterDelay;
                }
                break;
            case 11:
                PlayBagMoveAnimation(data, 1, 9, data.Battler, 0);
                data.State++;
                break;
            case 13:
                if (data.BattleBagCtx!.SelectedBattleBagItem == BagItemGuardSpec)
                {
                    PlayBagMoveAnimation(data, 0, 0, data.Battler, BagMoveMist);
                }
                else if (data.BattleBagCtx.SelectedBattleBagItem == BagItemDireHit)
                {
                    PlayBagMoveAnimation(data, 0, 0, data.Battler, BagMoveFocusEnergy);
                }
                else
                {
                    PlayBagMoveAnimation(data, 1, 0xC, data.Battler, 0);
                }
                data.State++;
                break;
            case 15:
            {
                BattleMessage message = new();
                message.Id = BagMsgPokemonsStatRose;
                message.Tags = BagTagNicknameStat;
                message.Params[0] = data.Battler | (data.PartyMenuData!.PartySlots[data.Battler] << 8);
                int index = data.BattleBagCtx!.SelectedBattleBagItem - BagItemGuardSpec;
                if (index == 0)
                {
                    message.Id = BagMsgWhiteFog;
                    message.Tags = BagTagNone;
                }
                else if (index == 1)
                {
                    message.Id = BagMsgGettingPumped;
                    message.Tags = BagTagNickname;
                }
                else if (index >= 2 && index <= 7)
                {
                    message.Params[1] = BagXItemStats[index - 2];
                }
                PrintBagMessage(data, message);
                data.State++;
                break;
            }
            case 12:
            case 14:
            case 18:
            case 20:
            case 26:
            case 28:
            {
                BattleAnimSystem animSys = BattleSystems.BattleSystem_GetBattleAnimSystem(data.BattleSys!)!;
                BattleAnimSystems.BattleAnimSystem_ExecuteScript(animSys);
                if (BattleAnimSystems.BattleAnimSystem_IsMoveActive(BattleSystems.BattleSystem_GetBattleAnimSystem(data.BattleSys!)!) != 0)
                {
                    break;
                }
                BattleAnimSystems.BattleAnimSystem_FreeScriptData(BattleSystems.BattleSystem_GetBattleAnimSystem(data.BattleSys!)!);
                data.State++;
                break;
            }
            case 16:
            case 24:
            case 30:
                if (Text.Text_IsPrinterActive(data.MsgIdx) != 0)
                {
                    break;
                }
                data.Delay--;
                if (data.Delay != 0)
                {
                    break;
                }
                Battle.Battle_SetOverlay(data.BattleSys!, 1);
                data.State = 8;
                break;
            case 17:
            case 25:
                PlayBagMoveAnimation(data, 1, 9, data.PartyMenuData!.BattlePartyCtx!.SelectedPartyIndex * 2, 0);
                data.State++;
                break;
            case 19:
            case 27:
                PlayBagMoveAnimation(data, 1, 0xE, data.PartyMenuData!.BattlePartyCtx!.SelectedPartyIndex * 2, 0);
                data.State++;
                break;
            case 21:
            {
                BattlePartyContext partyCtx = data.PartyMenuData!.BattlePartyCtx!;
                int battler = partyCtx.SelectedPartyIndex * 2;
                int slot = partyCtx.PokemonPartySlots[partyCtx.SelectedPartyIndex];
                HealthBox healthbox = BattleSystems.BattleSystem_GetHealthbox(data.BattleSys!, battler);
                healthbox.State = 0;
                healthbox.Type = Healthboxes.Healthbox_Type(BattleSystems.BattleSystem_BattlerSlot(data.BattleSys!, battler), BattleSystems.BattleSystem_GetBattleType(data.BattleSys!));
                Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(data.BattleSys!, battler, slot);
                healthbox.CurHP = (int)Pokemon.Pokemon_GetValue(mon, BagMonDataHP, null) - data.PartyMenuData.BattlePartyCtx!.CurrentDamage;
                healthbox.MaxHP = (int)Pokemon.Pokemon_GetValue(mon, BagMonDataMaxHP, null);
                healthbox.Damage = data.PartyMenuData.BattlePartyCtx.CurrentDamage;
                if (Pokemon.Pokemon_GetValue(mon, BagMonDataStatus, null) == 0)
                {
                    healthbox.Status = 0;
                }
                Healthboxes.Healthbox_CalcHP(healthbox, healthbox.Damage);
                data.State++;
                break;
            }
            case 22:
            {
                HealthBox healthbox = BattleSystems.BattleSystem_GetHealthbox(data.BattleSys!, data.PartyMenuData!.BattlePartyCtx!.SelectedPartyIndex * 2);
                if (Healthboxes.Healthbox_UpdateHP(healthbox) != -1)
                {
                    break;
                }
                Healthboxes.Healthbox_DrawInfo(healthbox, 0, BagHealthboxInfoStatus);
                data.State++;
                break;
            }
            case 23:
            {
                BattlePartyContext partyCtx = data.PartyMenuData!.BattlePartyCtx!;
                int battler = partyCtx.SelectedPartyIndex * 2;
                BattleMessage message = new();
                if (partyCtx.CurrentDamage != 0)
                {
                    message.Id = BagMsgHPRestored;
                    message.Tags = BagTagNicknameNum;
                    message.Params[0] = battler | (data.PartyMenuData.PartySlots[battler] << 8);
                    message.Params[1] = partyCtx.CurrentDamage;
                }
                else
                {
                    message.Id = BagMsgBecameHealthy;
                    message.Tags = BagTagNickname;
                    message.Params[0] = battler | (data.PartyMenuData.PartySlots[battler] << 8);
                }
                PrintBagMessage(data, message);
                data.State++;
                break;
            }
            case 29:
            {
                BattlePartyContext partyCtx = data.PartyMenuData!.BattlePartyCtx!;
                int battler = partyCtx.SelectedPartyIndex * 2;
                HealthBox healthbox = BattleSystems.BattleSystem_GetHealthbox(data.BattleSys!, battler);
                Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(data.BattleSys!, battler, partyCtx.PokemonPartySlots[partyCtx.SelectedPartyIndex]);
                if (Pokemon.Pokemon_GetValue(mon, BagMonDataStatus, null) == 0)
                {
                    healthbox.Status = 0;
                }
                Healthboxes.Healthbox_DrawInfo(healthbox, healthbox.CurHP, BagHealthboxInfoStatus);
                BattleMessage message = new();
                message.Tags = BagTagNickname;
                message.Params[0] = battler | (data.PartyMenuData.PartySlots[battler] << 8);
                int effect = 0;
                int msgType = 0;
                for (int i = 0; i < BagStatusCureMessages.Length; i++)
                {
                    if (Item.Item_LoadParam(data.BattleBagCtx!.SelectedBattleBagItem, 0xF + i, BagHeapIDBattle) != 0)
                    {
                        msgType = i;
                        effect++;
                    }
                }
                if (effect != 1)
                {
                    message.Id = BagMsgCuredGeneric;
                }
                else
                {
                    message.Id = BagStatusCureMessages[msgType];
                }
                PrintBagMessage(data, message);
                data.State++;
                break;
            }
        }
    }

    private static void BagMenuSelectUseState(BagMenuData data)
    {
        PaletteAnimator.PaletteAnimator_SetMode(BattleSystems.BattleSystem_GetPaletteAnimator(data.BattleSys!)!, 0);
        BattleBagContext bagCtx = data.BattleBagCtx!;
        if (bagCtx.SelectedBattleBagItem == 0)
        {
            data.State = 8;
        }
        else
        {
            data.State = 9;
            ushort item = bagCtx.SelectedBattleBagItem;
            switch (bagCtx.SelectedBattleBagPocket)
            {
                case BagPocketRecoverStatus:
                    if ((ushort)(item - BagItemRevive) <= 1)
                    {
                        data.State = 8;
                    }
                    else if (BagTargetIsActive(data))
                    {
                        if (item == BagItemFullRestore)
                        {
                            SetBagStateAfterDelay(data, 0x11, 0x15);
                        }
                        else
                        {
                            SetBagStateAfterDelay(data, 0x19, 0x1D);
                        }
                    }
                    else
                    {
                        data.State = 8;
                    }
                    break;
                case BagPocketRecoverHP:
                    if (BagTargetIsActive(data) && Item.Item_LoadParam(item, BagItemParamHPRestore, BagHeapIDBattle) != 0)
                    {
                        SetBagStateAfterDelay(data, 0x11, 0x15);
                    }
                    else
                    {
                        data.State = 8;
                    }
                    break;
                case BagPocketBattleItems:
                    if ((ushort)(item - BagItemPokeDoll) <= 1)
                    {
                        data.State = 8;
                    }
                    else if (item == BagItemGuardSpec)
                    {
                        SetBagStateAfterDelay(data, 0xD, 0xF);
                    }
                    else
                    {
                        SetBagStateAfterDelay(data, 0xB, 0xF);
                    }
                    break;
                case BagPocketPokeBalls:
                    data.State = 8;
                    break;
            }
        }
        if (data.State != 8)
        {
            Battle.Battle_SetOverlay(data.BattleSys!, 0);
        }
    }

    // OFFSET 0x02261D50
    private static void Task_TrainerShowBagMenu(SysTask task, object? param)
    {
        BagMenuData data = (BagMenuData)param!;
        BattleIO.BattleIO_BagResult(data.BattleSys!, data.Battler, BagItemUse(1, 0, 0));
        BattleIO.BattleIO_ClearMessage(data.BattleSys!, data.Battler, data.Command);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x02261DA8
    private static void Task_LinkShowBagMenu(SysTask task, object? param)
    {
        BagMenuData data = (BagMenuData)param!;
        BattleIO.BattleIO_ClearMessage(data.BattleSys!, data.Battler, data.Command);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x02261DE0
    private static void Task_RecordingShowBagMenu(SysTask task, object? param)
    {
        BagMenuData data = (BagMenuData)param!;
        if (BattleSystems.BattleSystem_ReadRecorded(data.BattleSys!, data.Battler, out byte value) == 1)
        {
            BattleDisplay_StopPlaybackDesync(data.BattleSys!);
        }
        ushort item = value;
        if (BattleSystems.BattleSystem_ReadRecorded(data.BattleSys!, data.Battler, out value) == 1)
        {
            BattleDisplay_StopPlaybackDesync(data.BattleSys!);
        }
        item = (ushort)((value << 8) | item);
        if (BattleSystems.BattleSystem_ReadRecorded(data.BattleSys!, data.Battler, out value) == 1)
        {
            BattleDisplay_StopPlaybackDesync(data.BattleSys!);
        }
        BattleIO.BattleIO_BagResult(data.BattleSys!, data.Battler, BagItemUse(item, (byte)(value & 0xF), 0));
        BattleIO.BattleIO_ClearMessage(data.BattleSys!, data.Battler, data.Command);
        SysTask.SysTask_Done(task);
    }
}
