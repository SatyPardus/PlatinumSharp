namespace PlatinumSharp.Game;

public static partial class BattleControllers
{
    private const uint MoveEffectIngrain = 0x400;
    private const uint MoveEffectAquaRing = 0x1000000;
    private const uint MoveEffectLeechSeed = 4;
    private const uint MoveEffectLeechSeedTarget = 3;
    private const uint MoveEffectLockOnTurn = 8;
    private const uint MoveEffectCharge = 0x200;
    private const uint MoveEffectYawn = 0x1800;
    private const uint MoveEffectYawnTurn = 0x800;
    private const uint MonStatusSleep = 7;
    private const uint MonStatusPoison = 8;
    private const uint MonStatusBurn = 0x10;
    private const uint MonStatusToxic = 0x80;
    private const uint MonStatusToxicCounter = 0xF00;
    private const uint MonStatusToxicCounterInc = 0x100;
    private const uint VolatileThrashTurn = 0x400;
    private const uint VolatileUproar = 0x70;
    private const uint VolatileUproarTurn = 0x10;
    private const uint VolatileBind = 0xE000;
    private const uint VolatileBindTurn = 0x2000;
    private const uint VolatileNightmare = 0x8000000;
    private const uint VolatileCurse = 0x10000000;
    private const uint BattleStatusMonConditionHP = 0x40;
    private const int AbilityMagicGuardValue = 0x62;
    private const int AbilitySoundproofValue = 0x2B;
    private const int AbilityBadDreams = 0x7B;
    private const int CountOpponentsWithAbility = 4;
    private const int MoveAquaRing = 0x188;
    private const int SideEffectTypeYawn = 4;
    private const int SubseqHealBlocked = 0xEF;
    private const int SubseqIngrainHeal = 0x89;
    private const int SubseqAquaRingHeal = 0xA9;
    private const int SubseqLeechSeedDrain = 0x45;
    private const int SubseqPoisonDamage = 0x17;
    private const int SubseqBurnDamage = 0x1A;
    private const int SubseqNightmareDamage = 0x5E;
    private const int SubseqCurseDamage = 0x62;
    private const int SubseqBindDamage = 0x3B;
    private const int SubseqBindEnd = 0x3C;
    private const int SubseqBadDreamsDamage = 0x107;
    private const int SubseqUproarWakeUp = 0x13;
    private const int SubseqUproarContinues = 0xF0;
    private const int SubseqUproarEnd = 0xF1;
    private const int SubseqThrashConfusion = 0x33;
    private const int SubseqDisableEnd = 0x48;
    private const int SubseqEncoreEnd = 0x4A;
    private const int SubseqTauntEnd = 0x120;
    private const int SubseqMagnetRiseEnd = 0xF2;
    private const int SubseqHealBlockEnd = 0xF3;
    private const int SubseqEmbargoEnd = 0xF4;
    private const int SubseqYawnSleep = 0x12;
    private const int ControllerCommandCheckSideConditions = 0xB;

    private static int BattleController_TickTimer(int turns, Action<int> setTurns)
    {
        if (turns != 0)
        {
            setTurns((turns - 1) & 7);
            return (turns - 1) & 7;
        }
        return -1;
    }

    // OFFSET 0x0224CF7C
    private static void BattleController_CheckMonConditions(BattleSystem battleSys, BattleContext battleCtx)
    {
        int state = 0;
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        if (BattleController_AnyFainted(battleCtx, battleCtx.Command, battleCtx.Command, 1) == 1
            || BattleController_AnyExpPayout(battleCtx, battleCtx.Command, battleCtx.Command) == 1
            || BattleController_CheckBattleOver(battleSys, battleCtx) == 1)
        {
            return;
        }
        while (battleCtx.MonConditionCheckTemp < maxBattlers)
        {
            int battler = battleCtx.MonSpeedOrder[battleCtx.MonConditionCheckTemp];
            if ((HealthBar.FlagIndex(battler) & battleCtx.BattlersSwitchingMask) != 0)
            {
                battleCtx.MonConditionCheckTemp++;
                continue;
            }
            BattleMon mon = battleCtx.BattleMons[battler];
            MoveEffectsData effects = mon.MoveEffectsData;
            switch (battleCtx.MonConditionCheckState)
            {
                case 0:
                    if ((mon.MoveEffectsMask & MoveEffectIngrain) != 0 && mon.CurHP != mon.MaxHP && mon.CurHP != 0)
                    {
                        battleCtx.MsgBattlerTemp = battler;
                        BattleController_RunSubseq(battleCtx, effects.HealBlockTurns != 0 ? SubseqHealBlocked : SubseqIngrainHeal);
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 1:
                    if ((mon.MoveEffectsMask & MoveEffectAquaRing) != 0 && mon.CurHP != mon.MaxHP && mon.CurHP != 0)
                    {
                        if (effects.HealBlockTurns != 0)
                        {
                            battleCtx.MsgBattlerTemp = battler;
                            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqHealBlocked);
                        }
                        else
                        {
                            battleCtx.MsgBattlerTemp = battler;
                            battleCtx.MsgMoveTemp = MoveAquaRing;
                            battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)mon.MaxHP, 16);
                            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqAquaRingHeal);
                        }
                        battleCtx.CommandNext = battleCtx.Command;
                        battleCtx.Command = ControllerCommandRunScript;
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 2:
                    if (BattleContexts.BattleSystem_TriggerTurnEndAbility(battleSys, battleCtx, battler) == 1)
                    {
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 3:
                    if (BattleContexts.BattleSystem_TriggerHeldItem(battleSys, battleCtx, battler) == 1)
                    {
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 4:
                    if (BattleContexts.BattleSystem_TriggerLeftovers(battleSys, battleCtx, battler) == 1)
                    {
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 5:
                    if ((mon.MoveEffectsMask & MoveEffectLeechSeed) != 0
                        && battleCtx.BattleMons[mon.MoveEffectsMask & MoveEffectLeechSeedTarget].CurHP != 0
                        && BattleContexts.Battler_Ability(battleCtx, battler) != AbilityMagicGuardValue
                        && mon.CurHP != 0)
                    {
                        battleCtx.MsgAttacker = (int)(mon.MoveEffectsMask & MoveEffectLeechSeedTarget);
                        battleCtx.MsgDefender = battler;
                        BattleController_RunSubseq(battleCtx, SubseqLeechSeedDrain);
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 6:
                    if ((mon.Status & MonStatusPoison) != 0 && mon.CurHP != 0)
                    {
                        battleCtx.MsgBattlerTemp = battler;
                        battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)mon.MaxHP * -1, 8);
                        BattleController_RunSubseq(battleCtx, SubseqPoisonDamage);
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 7:
                    if ((mon.Status & MonStatusToxic) != 0 && mon.CurHP != 0)
                    {
                        battleCtx.MsgBattlerTemp = battler;
                        battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)mon.MaxHP, 16);
                        if ((mon.Status & MonStatusToxicCounter) != MonStatusToxicCounter)
                        {
                            mon.Status += MonStatusToxicCounterInc;
                        }
                        battleCtx.HpCalcTemp *= (int)((mon.Status & MonStatusToxicCounter) >> 8);
                        battleCtx.HpCalcTemp *= -1;
                        BattleController_RunSubseq(battleCtx, SubseqPoisonDamage);
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 8:
                    if ((mon.Status & MonStatusBurn) != 0 && mon.CurHP != 0)
                    {
                        battleCtx.MsgBattlerTemp = battler;
                        BattleController_RunSubseq(battleCtx, SubseqBurnDamage);
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 9:
                    if ((mon.StatusVolatile & VolatileNightmare) != 0 && mon.CurHP != 0)
                    {
                        if ((mon.Status & MonStatusSleep) != 0)
                        {
                            battleCtx.MsgBattlerTemp = battler;
                            BattleController_RunSubseq(battleCtx, SubseqNightmareDamage);
                            state = 1;
                        }
                        else
                        {
                            mon.StatusVolatile &= ~VolatileNightmare;
                        }
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 10:
                    if ((mon.StatusVolatile & VolatileCurse) != 0 && mon.CurHP != 0)
                    {
                        battleCtx.MsgBattlerTemp = battler;
                        BattleController_RunSubseq(battleCtx, SubseqCurseDamage);
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 11:
                    if ((mon.StatusVolatile & VolatileBind) != 0 && mon.CurHP != 0)
                    {
                        mon.StatusVolatile -= VolatileBindTurn;
                        if ((mon.StatusVolatile & VolatileBind) != 0)
                        {
                            battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)mon.MaxHP * -1, 16);
                            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqBindDamage);
                        }
                        else
                        {
                            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqBindEnd);
                        }
                        battleCtx.MsgMoveTemp = effects.BindingMove;
                        battleCtx.MsgBattlerTemp = battler;
                        battleCtx.CommandNext = battleCtx.Command;
                        battleCtx.Command = ControllerCommandRunScript;
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 12:
                    battleCtx.ScriptTemp = BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountOpponentsWithAbility, battler, AbilityBadDreams);
                    if ((mon.Status & MonStatusSleep) != 0
                        && BattleContexts.Battler_Ability(battleCtx, battler) != AbilityMagicGuardValue
                        && mon.CurHP != 0
                        && battleCtx.ScriptTemp != 0)
                    {
                        battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)mon.MaxHP * -1, 8);
                        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqBadDreamsDamage);
                        battleCtx.BattleStatusMask |= BattleStatusMonConditionHP;
                        battleCtx.MsgBattlerTemp = battler;
                        battleCtx.CommandNext = battleCtx.Command;
                        battleCtx.Command = ControllerCommandRunScript;
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 13:
                    if ((mon.StatusVolatile & VolatileUproar) != 0)
                    {
                        int i;
                        for (i = 0; i < maxBattlers; i++)
                        {
                            BattleMon sleeper = battleCtx.BattleMons[i];
                            if ((sleeper.Status & MonStatusSleep) != 0 && sleeper.CurHP != 0 && BattleContexts.Battler_Ability(battleCtx, i) != AbilitySoundproofValue)
                            {
                                battleCtx.MsgBattlerTemp = i;
                                BattleController_RunSubseq(battleCtx, SubseqUproarWakeUp);
                                break;
                            }
                        }
                        if (i != maxBattlers)
                        {
                            state = 2;
                            break;
                        }
                        int script;
                        mon.StatusVolatile -= VolatileUproarTurn;
                        if (BattleContexts.Battler_MoveFailed(battleCtx, battler) != 0)
                        {
                            mon.StatusVolatile &= ~VolatileUproar;
                            script = SubseqUproarEnd;
                            battleCtx.FieldConditionsMask &= ~(HealthBar.FlagIndex(battler) << 8);
                        }
                        else if ((mon.StatusVolatile & VolatileUproar) != 0)
                        {
                            script = SubseqUproarContinues;
                        }
                        else
                        {
                            mon.StatusVolatile &= ~VolatileUproar;
                            script = SubseqUproarEnd;
                            battleCtx.FieldConditionsMask &= ~(HealthBar.FlagIndex(battler) << 8);
                        }
                        battleCtx.MsgBattlerTemp = battler;
                        BattleController_RunSubseq(battleCtx, script);
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 14:
                    if ((mon.StatusVolatile & VolatileThrash) != 0)
                    {
                        mon.StatusVolatile -= VolatileThrashTurn;
                        if (BattleContexts.Battler_MoveFailed(battleCtx, battler) != 0)
                        {
                            mon.StatusVolatile &= ~VolatileThrash;
                        }
                        else if ((mon.StatusVolatile & VolatileThrash) == 0 && (mon.StatusVolatile & VolatileConfusion) == 0)
                        {
                            battleCtx.SideEffectMon = battler;
                            BattleController_RunSubseq(battleCtx, SubseqThrashConfusion);
                            state = 1;
                        }
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 15:
                    if (effects.DisabledMove != 0)
                    {
                        int slot;
                        for (slot = 0; slot < 4; slot++)
                        {
                            if (effects.DisabledMove == mon.Moves[slot])
                            {
                                break;
                            }
                        }
                        if (slot == 4)
                        {
                            effects.DisabledTurns = 0;
                        }
                        if (effects.DisabledTurns != 0)
                        {
                            effects.DisabledTurns = (effects.DisabledTurns - 1) & 7;
                        }
                        else
                        {
                            effects.DisabledMove = 0;
                            battleCtx.MsgBattlerTemp = battler;
                            BattleController_RunSubseq(battleCtx, SubseqDisableEnd);
                            state = 1;
                        }
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 16:
                    if (effects.EncoredMove != 0)
                    {
                        int slot;
                        for (slot = 0; slot < 4; slot++)
                        {
                            if (effects.EncoredMove == mon.Moves[slot])
                            {
                                break;
                            }
                        }
                        if (slot == 4 || mon.PpCur[slot] == 0)
                        {
                            effects.EncoredTurns = 0;
                        }
                        if (effects.EncoredTurns != 0)
                        {
                            effects.EncoredTurns = (effects.EncoredTurns - 1) & 7;
                        }
                        else
                        {
                            effects.EncoredMove = 0;
                            battleCtx.MsgBattlerTemp = battler;
                            BattleController_RunSubseq(battleCtx, SubseqEncoreEnd);
                            state = 1;
                        }
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 17:
                    if ((mon.MoveEffectsMask & MoveEffectLockOn) != 0)
                    {
                        mon.MoveEffectsMask -= MoveEffectLockOnTurn;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 18:
                    if (effects.ChargedTurns != 0)
                    {
                        effects.ChargedTurns = (effects.ChargedTurns - 1) & 3;
                        if (effects.ChargedTurns == 0)
                        {
                            mon.MoveEffectsMask &= ~MoveEffectCharge;
                        }
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 19:
                    if (BattleController_TickTimer(effects.TauntedTurns, v => effects.TauntedTurns = v) == 0)
                    {
                        battleCtx.MsgBattlerTemp = battler;
                        BattleController_RunSubseq(battleCtx, SubseqTauntEnd);
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 20:
                    if (BattleController_TickTimer(effects.MagnetRiseTurns, v => effects.MagnetRiseTurns = v) == 0)
                    {
                        battleCtx.MsgBattlerTemp = battler;
                        BattleController_RunSubseq(battleCtx, SubseqMagnetRiseEnd);
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 21:
                    if (BattleController_TickTimer(effects.HealBlockTurns, v => effects.HealBlockTurns = v) == 0)
                    {
                        battleCtx.MsgBattlerTemp = battler;
                        BattleController_RunSubseq(battleCtx, SubseqHealBlockEnd);
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 22:
                    if (BattleController_TickTimer(effects.EmbargoTurns, v => effects.EmbargoTurns = v) == 0)
                    {
                        battleCtx.MsgBattlerTemp = battler;
                        BattleController_RunSubseq(battleCtx, SubseqEmbargoEnd);
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 23:
                    if ((mon.MoveEffectsMask & MoveEffectYawn) != 0)
                    {
                        mon.MoveEffectsMask -= MoveEffectYawnTurn;
                        if ((mon.MoveEffectsMask & MoveEffectYawn) == 0)
                        {
                            battleCtx.SideEffectMon = battler;
                            battleCtx.SideEffectType = SideEffectTypeYawn;
                            BattleController_RunSubseq(battleCtx, SubseqYawnSleep);
                            state = 1;
                        }
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 24:
                    {
                        int script = 0;
                        if (BattleContexts.BattleSystem_TriggerHeldItemOnSwitchIn(battleSys, battleCtx, battler, ref script) == 1)
                        {
                            battleCtx.MsgBattlerTemp = battler;
                            BattleController_RunSubseq(battleCtx, script);
                            state = 1;
                        }
                        battleCtx.MonConditionCheckState++;
                        break;
                    }
                case 25:
                    if (BattleContexts.BattleSystem_TriggerDetrimentalHeldItem(battleSys, battleCtx, battler) == 1)
                    {
                        state = 1;
                    }
                    battleCtx.MonConditionCheckState++;
                    break;
                case 26:
                    battleCtx.MonConditionCheckState = 0;
                    battleCtx.MonConditionCheckTemp++;
                    break;
            }
            if (state != 0)
            {
                BattleIO.BattleIO_ClearMessageBox(battleSys);
                return;
            }
        }
        battleCtx.MonConditionCheckState = 0;
        battleCtx.MonConditionCheckTemp = 0;
        battleCtx.Command = ControllerCommandCheckSideConditions;
    }
}
