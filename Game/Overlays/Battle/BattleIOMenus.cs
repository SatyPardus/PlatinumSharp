namespace PlatinumSharp.Game;

public sealed partial class Pokemon
{
    private const int MonDataExpParam = 8;
    private const int MonDataLevelParam = 0xA1;

    // OFFSET 0x02075A10
    public static byte Pokemon_GetPercentToNextLevel(Pokemon mon)
    {
        int reencrypt = Pokemon_EnterDecryptionContext(mon);
        ushort species = (ushort)Pokemon_GetValue(mon, MonDataSpeciesParam, null);
        byte level = (byte)Pokemon_GetValue(mon, MonDataLevelParam, null);
        uint current = Species_GetExpForLevel(species, level);
        uint next = Species_GetExpForLevel(species, level + 1);
        uint exp = Pokemon_GetValue(mon, MonDataExpParam, null);
        Pokemon_ExitDecryptionContext(mon, reencrypt);
        return (byte)(100 * (exp - current) / (next - current));
    }
}

public static partial class BattleIO
{
    public const int CommandSetCommandSelection = 0xE;
    public const int CommandShowMoveSelectMenu = 0xF;
    public const int CommandShowTargetSelectMenu = 0x10;
    public const int CommandShowBagScreen = 0x11;
    public const int CommandShowPartyMenu = 0x12;
    public const int CommandShowYesNoMenu = 0x13;
    public const int CommandPrintAttackMessage = 0x14;
    public const int CommandPrintMessage = 0x15;
    public const int CommandSetAlertMessage = 0x25;
    public const int CommandClearTouchScreen = 0x2F;
    public const int CommandLinkWaitMessage = 0x37;
    public const int BattleMessageSize = 0x24;
    public const int BallStatusNone = 0;
    public const int BallStatusHealthy = 1;
    public const int BallStatusFainted = 2;
    public const int BallStatusAilment = 3;

    private const int CommandSelectionSize = 0x2C;
    private const int MoveSelectSize = 0x14;
    private const int TargetSelectSize = 0x24;
    private const int BagScreenSize = 0x24;
    private const int PartyMenuSize = 0x28;
    private const int YesNoMenuSize = 0xC;
    private const int AlertMessageSize = 0x28;
    private const int LinkWaitSize = 0x20;
    private const int MenuMonDataSpeciesEgg = 0xAE;
    private const int MenuMonDataCurHP = 0xA3;
    private const int MenuMonDataStatus = 0xA0;
    private const int MenuSpeciesEgg = 0x1EE;
    private const uint MenuBattleTypeDoubles = 0x2;
    private const uint MenuBattleTypeLink = 0x4;
    private const uint MenuBattleTypeMulti = 0x8;
    private const uint MenuBattleTypeTag = 0x10;
    private const uint MenuBattleTypeNoExp = 0x2A4;
    private const uint MenuBattleTypeLinkMulti = 0xC;
    private const uint MenuBattleTypeCatchingTutorial = 0x4A;
    private const uint MenuBattleStatusPlayback = 0x10;
    private const uint MenuMoveEffectSemiInvulnerable = 0x200400C0;
    private const uint MenuVolatileSubstitute = 0x1000000;
    private const int PartySlots = 6;

    public static void WriteBattleMessage(byte[] message, int offset, BattleMessage battleMsg)
    {
        W8(message, offset, battleMsg.CommandCode);
        W8(message, offset + 1, battleMsg.Tags);
        W16(message, offset + 2, battleMsg.Id);
        for (int i = 0; i < battleMsg.Params.Length; i++)
        {
            W32(message, offset + 4 + i * 4, (uint)battleMsg.Params[i]);
        }
        W32(message, offset + 0x1C, (uint)battleMsg.Digits);
        W32(message, offset + 0x20, (uint)battleMsg.Battler);
    }

    public static void ReadBattleMessage(byte[] message, int offset, BattleMessage battleMsg)
    {
        battleMsg.CommandCode = message[offset];
        battleMsg.Tags = message[offset + 1];
        battleMsg.Id = (ushort)R16(message, offset + 2);
        for (int i = 0; i < battleMsg.Params.Length; i++)
        {
            battleMsg.Params[i] = (int)R32(message, offset + 4 + i * 4);
        }
        battleMsg.Digits = (int)R32(message, offset + 0x1C);
        battleMsg.Battler = (int)R32(message, offset + 0x20);
    }

    private static int Pokemon_BallStatus(Pokemon mon)
    {
        if (Pokemon.Pokemon_GetValue(mon, MenuMonDataCurHP, null) != 0)
        {
            return Pokemon.Pokemon_GetValue(mon, MenuMonDataStatus, null) != 0 ? BallStatusAilment : BallStatusHealthy;
        }
        return BallStatusFainted;
    }

    private static void BattleIO_WriteBallStatus(BattleSystem battleSys, BattleContext battleCtx, int battler, byte[] message, int offset)
    {
        Party party = BattleSystems.BattleSystem_GetParty(battleSys, battler);
        int index = 0;
        for (int i = 0; i < Party.Party_GetCurrentCount(party); i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, battleCtx.PartyOrder[battler, i]);
            uint species = Pokemon.Pokemon_GetValue(mon, MenuMonDataSpeciesEgg, null);
            if (species != 0 && species != MenuSpeciesEgg)
            {
                W8(message, offset + index, Pokemon_BallStatus(mon));
                index++;
            }
        }
    }

    // OFFSET 0x02265330
    public static void BattleIO_SetCommandSelection(BattleSystem battleSys, BattleContext battleCtx, int battler, int partySlot)
    {
        byte[] message = new byte[CommandSelectionSize];
        BattleIO_ClearBuffer(BattleSystems.BattleSystem_GetBattleContext(battleSys)!, battler);
        int cannotPickMask = 0;
        for (int i = 0; i < BattleSystems.BattleSystem_GetMaxBattlers(battleSys); i++)
        {
            if (BattleContexts.Battler_CanPickCommand(battleCtx, i) == 0)
            {
                cannotPickMask |= (int)HealthBar.FlagIndex(i);
            }
        }
        W8(message, 0, CommandSetCommandSelection);
        W8(message, 1, partySlot);
        W8(message, 0x29, battleCtx.BattlersSwitchingMask | cannotPickMask);
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int partyBattler = (battleType & MenuBattleTypeDoubles) != 0 && (battleType & MenuBattleTypeMulti) == 0 ? battler & 1 : battler;
        Party party = BattleSystems.BattleSystem_GetParty(battleSys, partyBattler);
        int index = 0;
        for (int i = 0; i < Party.Party_GetCurrentCount(party); i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, battleCtx.PartyOrder[partyBattler, i]);
            uint species = Pokemon.Pokemon_GetValue(mon, MenuMonDataSpeciesEgg, null);
            if (species != 0 && species != MenuSpeciesEgg)
            {
                W8(message, 8 + index, Pokemon_BallStatus(mon));
                W8(message, 2 + index, (battleType & MenuBattleTypeNoExp) != 0 ? 0 : Pokemon.Pokemon_GetPercentToNextLevel(mon));
                index++;
            }
        }
        if ((battleType & MenuBattleTypeLinkMulti) == MenuBattleTypeLinkMulti || (battleType & MenuBattleTypeTag) != 0 || battleType == 0x4B || battleType == 0xCB)
        {
            int enemy = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) != 0
                ? BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 2)
                : BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 3);
            BattleIO_WriteBallStatus(battleSys, battleCtx, enemy, message, 0xE);
            enemy = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) != 0
                ? BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 4)
                : BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 5);
            BattleIO_WriteBallStatus(battleSys, battleCtx, enemy, message, 0xE + 3);
        }
        else
        {
            int enemy = BattleSystems.BattleSystem_GetEnemyInSlot(battleSys, battler, 2);
            BattleIO_WriteBallStatus(battleSys, battleCtx, enemy, message, 0xE);
        }
        for (int i = 0; i < 4; i++)
        {
            W16(message, 0x14 + i * 2, BattleContexts.BattleMon_Get(battleCtx, battler, BattleContexts.BattleMonMove1 + i, null));
            W8(message, 0x1C + i, BattleContexts.BattleMon_Get(battleCtx, battler, BattleContexts.BattleMonCurPP1 + i, null));
            W8(message, 0x20 + i, BattleContexts.BattleMon_Get(battleCtx, battler, BattleContexts.BattleMonMaxPP1 + i, null));
        }
        BattleMon battleMon = battleCtx.BattleMons[battler];
        W16(message, 0x24, battleMon.CurHP);
        W16(message, 0x26, (int)battleMon.MaxHP);
        if ((short)battleMon.CurHP != 0)
        {
            W8(message, 0x28, battleMon.Status != 0 ? BallStatusAilment : BallStatusHealthy);
        }
        else
        {
            W8(message, 0x28, BallStatusFainted);
        }
        BattleIO_Send(battleSys, RecipientBattler, battler, message, CommandSelectionSize);
    }

    private static void BattleIO_SendResult(BattleSystem battleSys, int battler, uint value)
    {
        byte[] message = new byte[4];
        W32(message, 0, value);
        BattleIO_Send(battleSys, RecipientServer, battler, message, 4);
    }

    // OFFSET 0x022656D4
    public static void BattleIO_CommandSelectionResult(BattleSystem battleSys, int battler, uint value)
    {
        BattleIO_SendResult(battleSys, battler, value);
    }

    // OFFSET 0x022656F0
    public static void BattleIO_ShowMoveSelectMenu(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        BattleIO_ClearBuffer(BattleSystems.BattleSystem_GetBattleContext(battleSys)!, battler);
        byte[] message = new byte[MoveSelectSize];
        BattleMon mon = battleCtx.BattleMons[battler];
        W8(message, 0, CommandShowMoveSelectMenu);
        W8(message, 1, battleCtx.SelectedPartySlot[battler]);
        for (int i = 0; i < 4; i++)
        {
            W16(message, 4 + i * 2, mon.Moves[i]);
            W8(message, 0xC + i, mon.PpCur[i]);
            W8(message, 0x10 + i, (int)MoveTable.MoveTable_CalcMaxPP(mon.Moves[i], mon.PpUps[i]));
        }
        W16(message, 2, BattleContexts.BattleSystem_CheckInvalidMoves(battleSys, battleCtx, battler, 0, -1));
        BattleIO_Send(battleSys, RecipientBattler, battler, message, MoveSelectSize);
    }

    // OFFSET 0x02265790
    public static void BattleIO_MoveSelectResult(BattleSystem battleSys, int battler, uint value)
    {
        BattleIO_SendResult(battleSys, battler, value);
    }

    // OFFSET 0x022657AC
    public static void BattleIO_ShowTargetSelectMenu(BattleSystem battleSys, BattleContext battleCtx, int range, int battler)
    {
        BattleIO_ClearBuffer(battleCtx, battler);
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        byte[] message = new byte[TargetSelectSize];
        W8(message, 0, CommandShowTargetSelectMenu);
        W16(message, 2, range);
        bool doubles = (battleType & MenuBattleTypeDoubles) != 0;
        W8(message, 1, !doubles || (battleType & MenuBattleTypeMulti) != 0 || battler >= 2 ? 1 : 0);
        for (int i = 0; i < 4; i++)
        {
            BattleMon mon = battleCtx.BattleMons[i];
            int entry = 4 + i * 8;
            if (mon.CurHP != 0)
            {
                W16(message, entry + 4, mon.CurHP);
                W16(message, entry + 6, (int)mon.MaxHP);
                int flags = 4;
                if ((mon.Species == SpeciesNidoranF || mon.Species == SpeciesNidoranM) && mon.HasNickname == 0)
                {
                    flags |= 2;
                }
                else
                {
                    flags |= mon.Gender & 3;
                }
                W8(message, entry + 1, flags);
                W8(message, entry, battleCtx.SelectedPartySlot[i]);
                W8(message, entry + 2, mon.Status != 0 ? BallStatusAilment : BallStatusHealthy);
            }
            else
            {
                W8(message, entry + 1, 0);
                W8(message, entry + 2, BallStatusFainted);
            }
        }
        BattleIO_Send(battleSys, RecipientBattler, battler, message, TargetSelectSize);
    }

    // OFFSET 0x022658CC
    public static void BattleIO_TargetSelectResult(BattleSystem battleSys, int battler, uint value)
    {
        BattleIO_SendResult(battleSys, battler, value);
    }

    private static void BattleIO_WriteBagTargetFlags(BattleContext battleCtx, byte[] message, int enemy)
    {
        BattleMon mon = battleCtx.BattleMons[enemy];
        W8(message, 1, 0);
        if ((mon.MoveEffectsMask & MenuMoveEffectSemiInvulnerable) != 0)
        {
            W8(message, 2, 1);
            W8(message, 3, 0);
        }
        else
        {
            W8(message, 2, 0);
            W8(message, 3, (mon.StatusVolatile & MenuVolatileSubstitute) != 0 ? 1 : 0);
        }
    }

    // OFFSET 0x022658E8
    public static void BattleIO_ShowBagScreen(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        BattleIO_ClearBuffer(battleCtx, battler);
        byte[] message = new byte[BagScreenSize];
        W8(message, 0, CommandShowBagScreen);
        for (int i = 0; i < 4; i++)
        {
            W8(message, 4 + i, battleCtx.SelectedPartySlot[i]);
            for (int j = 0; j < PartySlots; j++)
            {
                W8(message, 8 + i * PartySlots + j, battleCtx.PartyOrder[i, j]);
            }
            W8(message, 0x20 + i, battleCtx.BattleMons[i].MoveEffectsData.EmbargoTurns);
        }
        if (BattleSystems.BattleSystem_GetBattleType(battleSys) == MenuBattleTypeCatchingTutorial)
        {
            if ((HealthBar.FlagIndex(1) & battleCtx.BattlersSwitchingMask) == 0 && (HealthBar.FlagIndex(3) & battleCtx.BattlersSwitchingMask) == 0)
            {
                W8(message, 1, 1);
                W8(message, 2, 0);
                W8(message, 3, 0);
            }
            else if ((HealthBar.FlagIndex(1) & battleCtx.BattlersSwitchingMask) == 0)
            {
                BattleIO_WriteBagTargetFlags(battleCtx, message, 1);
            }
            else
            {
                BattleIO_WriteBagTargetFlags(battleCtx, message, 3);
            }
        }
        else if (BattleSystems.BattleSystem_GetBattleType(battleSys) == 0)
        {
            BattleIO_WriteBagTargetFlags(battleCtx, message, 1);
        }
        else
        {
            W8(message, 1, 0);
            W8(message, 2, 0);
            W8(message, 3, 0);
        }
        BattleIO_Send(battleSys, RecipientBattler, battler, message, BagScreenSize);
    }

    // OFFSET 0x02265A70
    public static void BattleIO_BagResult(BattleSystem battleSys, int battler, uint value)
    {
        BattleIO_SendResult(battleSys, battler, value);
    }

    // OFFSET 0x02265A8C
    public static void BattleIO_ShowPartyMenu(BattleSystem battleSys, BattleContext battleCtx, int battler, int listMode, int canSwitch, int doubleSwitchSlot)
    {
        BattleIO_ClearBuffer(battleCtx, battler);
        byte[] message = new byte[PartyMenuSize];
        W8(message, 0, CommandShowPartyMenu);
        W8(message, 1, battler);
        W8(message, 2, listMode);
        W32(message, 0x20, (uint)canSwitch);
        W8(message, 3, doubleSwitchSlot);
        W8(message, 0x24, battleCtx.BattlersSwitchingMask);
        for (int i = 0; i < 4; i++)
        {
            W8(message, 4 + i, battleCtx.SelectedPartySlot[i]);
            for (int j = 0; j < PartySlots; j++)
            {
                W8(message, 8 + i * PartySlots + j, battleCtx.PartyOrder[i, j]);
            }
        }
        BattleIO_Send(battleSys, RecipientBattler, battler, message, PartyMenuSize);
    }

    // OFFSET 0x02265B10
    public static void BattleIO_PartyMenuResult(BattleSystem battleSys, int battler, uint value)
    {
        BattleIO_SendResult(battleSys, battler, value);
    }

    // OFFSET 0x02265B2C
    public static void BattleIO_ShowYesNoMenu(BattleSystem battleSys, BattleContext battleCtx, int battler, int messageID, int type, int move, int nickname)
    {
        BattleIO_ClearBuffer(battleCtx, battler);
        byte[] message = new byte[YesNoMenuSize];
        W8(message, 0, CommandShowYesNoMenu);
        W16(message, 2, messageID);
        W8(message, 1, type);
        W32(message, 4, (uint)move);
        W32(message, 8, (uint)nickname);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, YesNoMenuSize);
    }

    // OFFSET 0x02265B68
    public static void BattleIO_PrintAttackMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        byte[] message = new byte[4];
        W8(message, 0, CommandPrintAttackMessage);
        W8(message, 1, battleCtx.SelectedPartySlot[battleCtx.Attacker]);
        W16(message, 2, (int)battleCtx.MoveCur);
        BattleIO_Send(battleSys, RecipientBattler, battleCtx.Attacker, message, 4);
    }

    // OFFSET 0x02265BA0
    public static void BattleIO_PrintMessage(BattleSystem battleSys, BattleContext battleCtx, BattleMessage battleMsg)
    {
        battleMsg.CommandCode = CommandPrintMessage;
        byte[] message = new byte[BattleMessageSize];
        WriteBattleMessage(message, 0, battleMsg);
        BattleIO_Send(battleSys, RecipientBattler, battleCtx.Attacker, message, BattleMessageSize);
    }

    // OFFSET 0x02266168
    public static void BattleIO_SetAlertMessage(BattleSystem battleSys, int battler, BattleMessage battleMsg)
    {
        BattleIO_ClearBuffer(BattleSystems.BattleSystem_GetBattleContext(battleSys)!, battler);
        byte[] message = new byte[AlertMessageSize];
        W8(message, 0, CommandSetAlertMessage);
        WriteBattleMessage(message, 4, battleMsg);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, AlertMessageSize);
    }

    // OFFSET 0x022665C8
    public static void BattleIO_ClearTouchScreen(BattleSystem battleSys, int battler)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandClearTouchScreen);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x022666E0
    public static void BattleIO_LinkWaitMessage(BattleSystem battleSys, int battler)
    {
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & MenuBattleTypeLink) == 0)
        {
            return;
        }
        throw Unported.Partial(0x022666E0);
    }
}
