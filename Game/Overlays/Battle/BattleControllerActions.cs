namespace PlatinumSharp.Game;

public static partial class BattleControllers
{
    private const ushort MoveStruggle = 0xA5;
    private const int EncoredTurnsMask = 0x38;
    private const int ControllerCommandExecuteMove = 0x16;
    private const int ControllerCommandUpdateFieldCondition = 0x27;
    private const int ControllerCommandSwitchIn = 0x28;
    private const int ControllerCommandFleeEnd = 0x2B;
    private const int ActionParamItem = 2;
    private const uint MoveStatusNoMessage = 0x80000000;
    private const int SubSeqUseFullRestore = 0x121;
    private const int SubSeqUseHealHPItem = 0x10B;
    private const int SubSeqUseStatusItem = 0x10C;
    private const int SubSeqUseStatBoostItem = 0x10D;
    private const int SubSeqUseGuardSpec = 0x10E;
    private const int SubSeqUseEscapeItem = 0xDE;
    private const int SubSeqUseItem = 0xDF;
    private const int SubSeqThrowBall = 0xB;
    private const int SubSeqSwitch = 9;
    private const int SubSeqEnemyTrapped = 0x11E;
    private const int SubSeqEnemyFlee = 0xE6;
    private const int SubSeqFlee = 3;
    private const int SubSeqCannotFlee = 8;
    private const int SubSeqSafariBall = 0x113;
    private const int SubSeqSafariBait = 0xE3;
    private const int SubSeqSafariRock = 0xE4;
    private const int UsedItemTypeFullRestore = 0;
    private const int UsedItemTypeHealHP = 1;
    private const int UsedItemTypeStatus = 2;
    private const int UsedItemTypeStatBoost = 3;
    private const int UsedItemTypeGuardSpec = 4;
    private const int UsedConditionConfusion = 1;
    private const int UsedConditionStatusMask = 0x3E;
    private const int MsgTempConfusionAndStatus = 6;
    private const ushort ItemPokeDoll = 0x3F;
    private const int BagPocketBalls = 2;
    private const uint BattleTypeNoBallConsume = BattleSystems.BattleTypeTrainer | BattleSystems.BattleTypeCatchTutorial;
    private const int ItemSafariBall = 5;
    private const int SafariStageMax = 12;
    private const int HeapIDBattle = 5;
    private const uint VolatileTrapped = 0x400E000;

    // OFFSET 0x0224DC68
    private static void BattleController_PrepareFightAction(BattleSystem battleSys, BattleContext battleCtx)
    {
        int moveChanged = 0;
        int battler = battleCtx.BattlerActionOrder[battleCtx.TurnOrderCounter];
        battleCtx.Attacker = battler;
        BattleMon mon = battleCtx.BattleMons[battler];
        MoveEffectsData effects = mon.MoveEffectsData;
        if (battleCtx.TurnFlags[battler].Struggling != 0)
        {
            battleCtx.MoveTemp = MoveStruggle;
            moveChanged = 1;
        }
        else if (effects.EncoredMove != 0 && effects.EncoredMove == mon.Moves[effects.EncoredMoveSlot])
        {
            battleCtx.MoveTemp = effects.EncoredMove;
            moveChanged = 1;
        }
        else if (effects.EncoredMove != 0)
        {
            battleCtx.MoveTemp = mon.Moves[effects.EncoredMoveSlot];
            effects.EncoredMove = 0;
            effects.EncoredMoveSlot = 0;
            effects.EncoredTurns = 0;
            moveChanged = 1;
        }
        else if (BattleContexts.Battler_CanPickCommand(battleCtx, battler) == 0)
        {
            battleCtx.MoveTemp = (uint)battleCtx.MoveLockedInto[battler];
        }
        else
        {
            ushort move = mon.Moves[battleCtx.MoveSlot[battler]];
            if (battleCtx.MoveSelected[battler] != move)
            {
                moveChanged = 1;
            }
            battleCtx.MoveTemp = move;
        }
        battleCtx.MoveCur = battleCtx.MoveTemp;
        battleCtx.Command = ControllerCommandExecuteMove;
        battleCtx.Defender = BattleContexts.BattleSystem_Defender(battleSys, battleCtx, battleCtx.Attacker, (ushort)battleCtx.MoveTemp, moveChanged, 0);
        BattleIO.BattleIO_ClearMessageBox(battleSys);
    }

    // OFFSET 0x0224DDA8
    private static void BattleController_UseItem(BattleSystem battleSys, BattleContext battleCtx)
    {
        int battler = battleCtx.BattlerActionOrder[battleCtx.TurnOrderCounter];
        battleCtx.Attacker = battler;
        battleCtx.Defender = BattleContexts.BattleSystem_RandomOpponent(battleSys, battleCtx, battler);
        int script = 0;
        if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) != 0)
        {
            int side = battleCtx.Attacker >> 1;
            AIContext ai = battleCtx.AiContext;
            switch (ai.UsedItemType[side])
            {
                case UsedItemTypeFullRestore:
                    script = SubSeqUseFullRestore;
                    break;
                case UsedItemTypeHealHP:
                    script = SubSeqUseHealHPItem;
                    break;
                case UsedItemTypeStatus:
                {
                    int condition = ai.UsedItemCondition[side];
                    if ((condition & UsedConditionConfusion) != 0 && (condition & UsedConditionStatusMask) != 0)
                    {
                        battleCtx.MsgTemp = MsgTempConfusionAndStatus;
                    }
                    else
                    {
                        battleCtx.MsgTemp = MathUtil.MathUtil_LowestSetBit((uint)condition);
                    }
                    script = SubSeqUseStatusItem;
                    break;
                }
                case UsedItemTypeStatBoost:
                    battleCtx.MsgTemp = ai.UsedItemCondition[side];
                    script = SubSeqUseStatBoostItem;
                    break;
                case UsedItemTypeGuardSpec:
                    script = SubSeqUseGuardSpec;
                    break;
            }
            battleCtx.MsgItemTemp = ai.UsedItem[battleCtx.Attacker >> 1];
        }
        else
        {
            ushort item = (ushort)(battleCtx.BattlerActions[battleCtx.Attacker, ActionParamItem] & 0xFFFF);
            int pocket = (int)((battleCtx.BattlerActions[battleCtx.Attacker, ActionParamItem] >> 16) & 0xFF);
            switch (pocket)
            {
                case 0:
                case 1:
                case 3:
                    script = (ushort)(item - ItemPokeDoll) <= 1 ? SubSeqUseEscapeItem : SubSeqUseItem;
                    break;
                case BagPocketBalls:
                    script = SubSeqThrowBall;
                    if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeTrainer) == 0
                        && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeCatchTutorial) == 0)
                    {
                        BagItems.Bag_TryRemoveItem(BattleSystems.BattleSystem_GetBag(battleSys)!, item, 1, HeapIDBattle);
                        BagCursor.BagCursor_SetLastUsedBattleItem(BattleSystems.BattleSystem_GetBagCursor(battleSys)!, item, (ushort)pocket);
                    }
                    break;
            }
            battleCtx.MsgItemTemp = item;
        }
        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, script);
        battleCtx.Command = ControllerCommandRunScript;
        battleCtx.CommandNext = ControllerCommandUpdateFieldCondition;
        battleCtx.MoveStatusFlags |= MoveStatusNoMessage;
    }

    // OFFSET 0x0224DF08
    private static void BattleController_Switch(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqSwitch);
        int battler = battleCtx.BattlerActionOrder[battleCtx.TurnOrderCounter];
        battleCtx.Attacker = battler;
        battleCtx.SwitchedMon = battler;
        battleCtx.Command = ControllerCommandRunScript;
        battleCtx.CommandNext = ControllerCommandSwitchIn;
        battleCtx.ScriptTemp = 0;
        battleCtx.MoveStatusFlags |= MoveStatusNoMessage;
    }

    // OFFSET 0x0224DF4C
    private static void BattleController_Flee(BattleSystem battleSys, BattleContext battleCtx)
    {
        int battler = battleCtx.BattlerActionOrder[battleCtx.TurnOrderCounter];
        battleCtx.Attacker = battler;
        if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) != 0
            && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeLink) == 0)
        {
            if ((battleCtx.BattleMons[battleCtx.Attacker].StatusVolatile & VolatileTrapped) != 0)
            {
                BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqEnemyTrapped);
                battleCtx.ScriptCursor = 0;
                battleCtx.Command = ControllerCommandRunScript;
                battleCtx.CommandNext = ControllerCommandUpdateFieldCondition;
                return;
            }
            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqEnemyFlee);
            battleCtx.ScriptCursor = 0;
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = ControllerCommandFleeEnd;
            return;
        }
        if (BattleContexts.Battler_CanEscape(battleSys, battleCtx, battleCtx.Attacker) != 0)
        {
            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqFlee);
            battleCtx.ScriptCursor = 0;
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = ControllerCommandFleeEnd;
            return;
        }
        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqCannotFlee);
        battleCtx.ScriptCursor = 0;
        battleCtx.Command = ControllerCommandRunScript;
        battleCtx.CommandNext = ControllerCommandUpdateFieldCondition;
    }

    // OFFSET 0x0224E010
    private static void BattleController_SafariBall(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqSafariBall);
        battleCtx.Attacker = 0;
        battleCtx.Defender = 1;
        battleCtx.Command = ControllerCommandRunScript;
        battleCtx.CommandNext = ControllerCommandUpdateFieldCondition;
        battleCtx.MsgItemTemp = ItemSafariBall;
        BattleSystems.BattleSystem_SetSafariBalls(battleSys, BattleSystems.BattleSystem_GetSafariBalls(battleSys) - 1);
        BattleIO.BattleIO_RefreshHPGauge(battleSys, battleCtx, 0);
    }

    // OFFSET 0x0224E058
    private static void BattleController_SafariBait(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqSafariBait);
        battleCtx.Attacker = 0;
        battleCtx.Defender = 1;
        battleCtx.Command = ControllerCommandRunScript;
        battleCtx.CommandNext = ControllerCommandUpdateFieldCondition;
        battleCtx.ScriptTemp = (int)(BattleSystems.BattleSystem_RandNext(battleSys) % 10);
        if (battleCtx.SafariCatchStage < SafariStageMax)
        {
            battleCtx.SafariCatchStage++;
        }
        if (battleCtx.ScriptTemp != 0 && battleCtx.SafariEscapeCount < SafariStageMax)
        {
            battleCtx.SafariEscapeCount++;
        }
    }

    // OFFSET 0x0224E0B8
    private static void BattleController_SafariRock(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqSafariRock);
        battleCtx.Attacker = 0;
        battleCtx.Defender = 1;
        battleCtx.Command = ControllerCommandRunScript;
        battleCtx.CommandNext = ControllerCommandUpdateFieldCondition;
        battleCtx.ScriptTemp = (int)(BattleSystems.BattleSystem_RandNext(battleSys) % 10);
        if (battleCtx.SafariEscapeCount != 0)
        {
            battleCtx.SafariEscapeCount--;
        }
        if (battleCtx.ScriptTemp != 0)
        {
            battleCtx.MsgTemp = 1;
            if (battleCtx.SafariCatchStage != 0)
            {
                battleCtx.SafariCatchStage--;
            }
        }
    }
}

public static partial class BattleIO
{
    private const int CommandRefreshHPGauge = 0x26;
    private const int RefreshHPGaugeSize = 0x14;
    private const int RefreshMonDataSpecies = 5;
    private const int RefreshMonDataLevel = 0xA1;
    private const int RefreshGenderNone = 2;
    private const int RefreshSpeciesNidoranF = 0x1D;
    private const int RefreshSpeciesNidoranM = 0x20;

    // OFFSET 0x022661CC
    public static void BattleIO_RefreshHPGauge(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, battleCtx.SelectedPartySlot[battler]);
        int species = (int)Pokemon.Pokemon_GetValue(mon, RefreshMonDataSpecies, null);
        int level = (int)Pokemon.Pokemon_GetValue(mon, RefreshMonDataLevel, null);
        BattleMon battleMon = battleCtx.BattleMons[battler];
        byte[] message = new byte[RefreshHPGaugeSize];
        W8(message, 0, CommandRefreshHPGauge);
        W8(message, 1, battleMon.Level);
        W16(message, 2, battleMon.CurHP);
        W16(message, 4, (int)battleMon.MaxHP);
        W8(message, 6, battleCtx.SelectedPartySlot[battler]);
        int info = BattleContexts.Battler_StatusIcon(battleCtx, battler) & 0x1F;
        if ((battleMon.Species == RefreshSpeciesNidoranF || battleMon.Species == RefreshSpeciesNidoranM) && battleMon.HasNickname == 0)
        {
            info |= RefreshGenderNone << 5;
        }
        else
        {
            info |= (battleMon.Gender & 3) << 5;
        }
        W32(message, 8, battleMon.Exp - Pokemon.Species_GetExpForLevel(species, level));
        W32(message, 0xC, Pokemon.Species_GetExpForLevel(species, level + 1) - Pokemon.Species_GetExpForLevel(species, level));
        info |= (BattleSystems.BattleSystem_CaughtSpecies(battleSys, battleMon.Species) & 1) << 7;
        W8(message, 7, info);
        W32(message, 0x10, (uint)BattleSystems.BattleSystem_GetSafariBalls(battleSys));
        BattleIO_Send(battleSys, RecipientBattler, battler, message, RefreshHPGaugeSize);
    }
}

public static partial class MathUtil
{
    // OFFSET 0x020787EC
    public static int MathUtil_LowestSetBit(uint value)
    {
        int bit;
        uint mask = 1;
        for (bit = 0; bit < 32; bit++)
        {
            if ((value & mask) != 0)
            {
                break;
            }
            mask <<= 1;
        }
        return bit;
    }
}
