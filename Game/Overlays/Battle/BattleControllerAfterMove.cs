using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class BattleControllers
{
    private const int ControllerCommandAfterMoveMessage = 0x1C;
    private const int ControllerCommandAfterMoveEffects = 0x1E;
    private const int ControllerCommandLoopMultiHit = 0x1F;
    private const int ControllerCommandLoopWhileFainted = 0x21;
    private const int ControllerCommandLoopSpreadMoves = 0x22;
    private const int ControllerCommandFaintAfterSelfdestruct = 0x23;
    private const int ControllerCommandTriggerAfterHitEffects = 0x24;
    private const int ControllerCommandUpdateMoveBuffers = 0x26;
    private const int ControllerCommandCheckAnyFainted = 0x27;
    private const int ControllerCommandHandleResult = 0x2B;
    private const int ControllerCommandEndWait = 0x2C;
    private const int ControllerCommandNone = 0x27;
    private const int ControllerCommandUseMove = 0x16;
    private const uint MoveStatusOneHitKO = 0x20;
    private const uint MoveStatusMultiHitInterrupt = 0x4000;
    private const uint MoveStatusNoEffectMask = 0x1FD849;
    private const uint MoveStatusNoEffectFlinch = 0x801FDA49;
    private const uint MoveStatusEndured = 0x80;
    private const uint MoveStatusFocusItem = 0x100;
    private const uint VolatileSubstitute = 0x1000000;
    private const uint SelfTurnStatusSubstituteHit = 8;
    private const int EffectFalseSwipe = 0x65;
    private const int HoldEffectFocusBand = 0x41;
    private const int HoldEffectFocusSash = 0x67;
    private const int HoldEffectKingsRock = 0x38;
    private const int HoldEffectChoiceBand = 0x37;
    private const int HoldEffectChoiceScarf = 0x73;
    private const int HoldEffectChoiceSpecs = 0x7D;
    private const int AlertMessageAllyHit = 0x61;
    private const int SubseqSubstituteHit = 0x5A;
    private const int SubseqUpdateHP = 2;
    private const int SubseqCriticalHit = 0x10;
    private const int SubseqFollowupMessage = 0x15;
    private const int SubseqMoveResultMessage = 0x125;
    private const int SubseqRageBuilding = 0xF5;
    private const int SubseqFlinch = 0xE;
    private const int SubseqMultiHitMessage = 0x11;
    private const int SubseqThawOut = 0x1D;
    private const int SubseqWakeUpFromUproar = 0x11F;
    private const int SubseqFaintMon = 6;
    private const int SubseqFaintMonWithMessage = 0x53;
    private const int SubseqGiveExp = 0x114;
    private const int SubseqSelfdestructFaint = 0x115;
    private const int SubseqWinBattle = 4;
    private const int SubseqLoseBattle = 5;
    private const uint BattleStatusMultiHitSemiInvuln = 0x4000;
    private const uint BattleStatusFaintedMask = 0xF000000;
    private const uint BattleStatusSelfdestructMask = 0xF0000000;
    private const uint BattleStatus2ExpMask = 0xF0000000;
    private const uint BattleStatus2MagicCoat = 8;
    private const uint BattleStatusMoveFailed = 0x100000;
    private const uint BattleStatusSkipMoveBuffers = 0x20;
    private const uint BattleStatus2MoveHit = 4;
    private const uint BattleStatus2UTurn = 0x10;
    private const uint BattleStatus2BatonPass = 0x40;
    private const uint BattleStatusSingleTarget = 2;
    private const uint BattleStatusUpdateHP = 0x2000;
    private const uint MoveEffectUproarWake = 0x200400C0;
    private const int AbilityNormalize = 0x60;
    private const int TypeFire = 0xA;
    private const int MoveUTurn = 0x171;
    private const int MoveBatonPass = 0xE2;
    private const int RangeAllOpponents = 4;
    private const int RangeAllExceptUser = 8;
    private const int MultiHitCheckSpread = 0xD;
    private const uint BattleTypeNoFieldEffects = 0x220;
    private const uint BattleTypeLinkEnd = 4;
    private const int ResultLose = 0x80;
    private const int CommCommandBattleEnd = 0x16;
    private const uint MoveFlagCopyable = 0x10;
    private const uint MoveFlagKingsRock = 0x20;

    // OFFSET 0x0224F8EC
    private static void BattleController_UpdateHP(BattleSystem battleSys, BattleContext battleCtx)
    {
        if ((battleCtx.MoveStatusFlags & MoveStatusOneHitKO) != 0)
        {
            battleCtx.Damage = (int)battleCtx.BattleMons[battleCtx.Defender].MaxHP * -1;
        }
        if (battleCtx.Damage == 0)
        {
            battleCtx.Command = ControllerCommandAfterMoveMessage;
            return;
        }
        int itemEffect = BattleContexts.Battler_HeldItemEffect(battleCtx, battleCtx.Defender);
        int itemPower = BattleContexts.Battler_HeldItemPower(battleCtx, battleCtx.Defender, 0);
        if (battleCtx.Damage >= 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker) == BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Defender))
        {
            BattleIO.BattleIO_PrintAlertMessage(battleSys, battleCtx.Attacker, 0, AlertMessageAllyHit);
        }
        battleCtx.LastHitByBattler[battleCtx.Defender] = battleCtx.Attacker;
        BattleMon defender = battleCtx.BattleMons[battleCtx.Defender];
        if ((defender.StatusVolatile & VolatileSubstitute) != 0 && battleCtx.Damage < 0)
        {
            int substituteHP = defender.MoveEffectsData.SubstituteHP;
            if (battleCtx.Damage + substituteHP <= 0)
            {
                battleCtx.SelfTurnFlags[battleCtx.Attacker].ShellBellDamageDealt -= substituteHP;
                battleCtx.BattleMons[battleCtx.Defender].StatusVolatile &= ~VolatileSubstitute;
                battleCtx.HitDamage = -battleCtx.BattleMons[battleCtx.Defender].MoveEffectsData.SubstituteHP;
                battleCtx.BattleMons[battleCtx.Defender].MoveEffectsData.SubstituteHP = 0;
            }
            else
            {
                battleCtx.SelfTurnFlags[battleCtx.Attacker].ShellBellDamageDealt += battleCtx.Damage;
                battleCtx.BattleMons[battleCtx.Defender].MoveEffectsData.SubstituteHP += battleCtx.Damage;
                battleCtx.HitDamage = battleCtx.Damage;
            }
            battleCtx.SelfTurnFlags[battleCtx.Defender].StatusFlags |= (int)SelfTurnStatusSubstituteHit;
            battleCtx.MsgBattlerTemp = battleCtx.Defender;
            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqSubstituteHit);
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = ControllerCommandAfterMoveMessage;
            return;
        }
        if (battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Effect == EffectFalseSwipe && battleCtx.Damage + defender.CurHP <= 0)
        {
            battleCtx.Damage = -(defender.CurHP - 1);
        }
        if (battleCtx.TurnFlags[battleCtx.Defender].Enduring == 0)
        {
            if (itemEffect == HoldEffectFocusBand && BattleSystems.BattleSystem_RandNext(battleSys) % 100 < itemPower)
            {
                battleCtx.SelfTurnFlags[battleCtx.Defender].FocusItemActivated = 1;
            }
            if (itemEffect == HoldEffectFocusSash && battleCtx.BattleMons[battleCtx.Defender].CurHP == battleCtx.BattleMons[battleCtx.Defender].MaxHP)
            {
                battleCtx.SelfTurnFlags[battleCtx.Defender].FocusItemActivated = 1;
            }
        }
        if ((battleCtx.TurnFlags[battleCtx.Defender].Enduring != 0 || battleCtx.SelfTurnFlags[battleCtx.Defender].FocusItemActivated != 0)
            && battleCtx.Damage + battleCtx.BattleMons[battleCtx.Defender].CurHP <= 0)
        {
            battleCtx.Damage = -(battleCtx.BattleMons[battleCtx.Defender].CurHP - 1);
            if (battleCtx.TurnFlags[battleCtx.Defender].Enduring != 0)
            {
                battleCtx.MoveStatusFlags |= MoveStatusEndured;
            }
            else
            {
                battleCtx.MoveStatusFlags |= MoveStatusFocusItem;
            }
        }
        battleCtx.StoredDamage[battleCtx.Defender] += battleCtx.Damage;
        if (battleCtx.BattleMons[battleCtx.Defender].TimesDamaged < 0xFF)
        {
            battleCtx.BattleMons[battleCtx.Defender].TimesDamaged++;
        }
        int moveClass = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Class;
        if (moveClass == 0)
        {
            battleCtx.TurnFlags[battleCtx.Defender].PhysicalDamageTakenFrom[battleCtx.Attacker] = battleCtx.Damage;
            battleCtx.TurnFlags[battleCtx.Defender].PhysicalDamageLastAttacker = battleCtx.Attacker;
            battleCtx.TurnFlags[battleCtx.Defender].PhysicalDamageAttackerMask |= (int)HealthBar.FlagIndex(battleCtx.Attacker);
            battleCtx.SelfTurnFlags[battleCtx.Defender].PhysicalDamageTaken = battleCtx.Damage;
            battleCtx.SelfTurnFlags[battleCtx.Defender].PhysicalDamageLastAttacker = battleCtx.Attacker;
        }
        else if (moveClass == 1)
        {
            battleCtx.TurnFlags[battleCtx.Defender].SpecialDamageTakenFrom[battleCtx.Attacker] = battleCtx.Damage;
            battleCtx.TurnFlags[battleCtx.Defender].SpecialDamageLastAttacker = battleCtx.Attacker;
            battleCtx.TurnFlags[battleCtx.Defender].SpecialDamageAttackerMask |= (int)HealthBar.FlagIndex(battleCtx.Attacker);
            battleCtx.SelfTurnFlags[battleCtx.Defender].SpecialDamageTaken = battleCtx.Damage;
            battleCtx.SelfTurnFlags[battleCtx.Defender].SpecialDamageLastAttacker = battleCtx.Attacker;
        }
        int curHP = battleCtx.BattleMons[battleCtx.Defender].CurHP;
        if (battleCtx.Damage + curHP <= 0)
        {
            battleCtx.SelfTurnFlags[battleCtx.Attacker].ShellBellDamageDealt += -curHP;
        }
        else
        {
            battleCtx.SelfTurnFlags[battleCtx.Attacker].ShellBellDamageDealt += battleCtx.Damage;
        }
        battleCtx.TurnFlags[battleCtx.Defender].LastDamageTaken = battleCtx.Damage;
        battleCtx.TurnFlags[battleCtx.Defender].LastAttacker = battleCtx.Attacker;
        battleCtx.MsgBattlerTemp = battleCtx.Defender;
        battleCtx.HpCalcTemp = battleCtx.Damage;
        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqUpdateHP);
        battleCtx.Command = ControllerCommandRunScript;
        battleCtx.CommandNext = ControllerCommandAfterMoveMessage;
        battleCtx.BattleStatusMask |= BattleStatusUpdateHP;
    }

    private static void BattleController_RunSubseq(BattleContext battleCtx, int script)
    {
        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, script);
        battleCtx.CommandNext = battleCtx.Command;
        battleCtx.Command = ControllerCommandRunScript;
    }

    // OFFSET 0x0224FD00
    private static void BattleController_AfterMoveMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        int script;
        if (battleCtx.AfterMoveMessageType == 0)
        {
            switch (battleCtx.AfterMoveMessageState)
            {
                case 0:
                    battleCtx.AfterMoveMessageState++;
                    if (BattleController_CriticalMessage(battleSys, battleCtx) == 1)
                    {
                        return;
                    }
                    goto case 1;
                case 1:
                    battleCtx.AfterMoveMessageState++;
                    if (BattleController_FollowupMessage(battleSys, battleCtx) == 1)
                    {
                        return;
                    }
                    goto case 2;
                case 2:
                    battleCtx.AfterMoveMessageState++;
                    if (BattleContexts.BattleSystem_CheckIndirectSideEffect(battleSys, battleCtx, out script) == 1 && (battleCtx.MoveStatusFlags & MoveStatusNoEffectMask) == 0)
                    {
                        BattleController_RunSubseq(battleCtx, script);
                        return;
                    }
                    goto case 3;
                case 3:
                    battleCtx.AfterMoveMessageState++;
                    BattleController_RunSubseq(battleCtx, SubseqMoveResultMessage);
                    return;
                case 4:
                    battleCtx.AfterMoveMessageState++;
                    if (BattleController_RageBuilding(battleSys, battleCtx) == 1)
                    {
                        return;
                    }
                    goto case 5;
                case 5:
                    battleCtx.AfterMoveMessageState++;
                    script = 0;
                    if (BattleContexts.BattleSystem_TriggerAbilityOnHit(battleSys, battleCtx, ref script) == 1)
                    {
                        BattleController_RunSubseq(battleCtx, script);
                        return;
                    }
                    goto case 6;
                case 6:
                    battleCtx.AfterMoveMessageState++;
                    if (BattleController_CheckExtraFlinch(battleSys, battleCtx) == 1)
                    {
                        return;
                    }
                    break;
            }
        }
        else if (battleCtx.AfterMoveMessageType == 1)
        {
            switch (battleCtx.AfterMoveMessageState)
            {
                case 0:
                    battleCtx.AfterMoveMessageState++;
                    if (BattleController_CriticalMessage(battleSys, battleCtx) == 1)
                    {
                        return;
                    }
                    goto case 1;
                case 1:
                    battleCtx.AfterMoveMessageState++;
                    if (BattleContexts.BattleSystem_CheckIndirectSideEffect(battleSys, battleCtx, out script) == 1 && (battleCtx.MoveStatusFlags & MoveStatusNoEffectMask) == 0)
                    {
                        BattleController_RunSubseq(battleCtx, script);
                        return;
                    }
                    goto case 2;
                case 2:
                    battleCtx.AfterMoveMessageState++;
                    BattleController_RunSubseq(battleCtx, SubseqMoveResultMessage);
                    return;
                case 3:
                    battleCtx.AfterMoveMessageState++;
                    if (BattleController_RageBuilding(battleSys, battleCtx) == 1)
                    {
                        return;
                    }
                    goto case 4;
                case 4:
                    battleCtx.AfterMoveMessageState++;
                    script = 0;
                    if (BattleContexts.BattleSystem_TriggerAbilityOnHit(battleSys, battleCtx, ref script) == 1)
                    {
                        BattleController_RunSubseq(battleCtx, script);
                        return;
                    }
                    goto case 5;
                case 5:
                    battleCtx.AfterMoveMessageState++;
                    if (BattleController_FollowupMessage(battleSys, battleCtx) == 1)
                    {
                        return;
                    }
                    goto case 6;
                case 6:
                    battleCtx.AfterMoveMessageState++;
                    if (BattleController_CheckExtraFlinch(battleSys, battleCtx) == 1)
                    {
                        return;
                    }
                    break;
            }
        }
        battleCtx.AfterMoveMessageState = 0;
        battleCtx.Command = ControllerCommandAfterMoveEffects;
    }

    // OFFSET 0x0224FEE0
    private static void BattleController_LeftoverState29(BattleSystem battleSys, BattleContext battleCtx)
    {
    }

    // OFFSET 0x0224FEE4
    private static void BattleController_AfterMoveEffects(BattleSystem battleSys, BattleContext battleCtx)
    {
        int script;
        switch (battleCtx.AfterMoveEffectState)
        {
            case 0:
            {
                bool woke = false;
                while (battleCtx.AfterMoveEffectTemp < BattleSystems.BattleSystem_GetMaxBattlers(battleSys))
                {
                    BattleMon mon = battleCtx.BattleMons[battleCtx.AfterMoveEffectTemp];
                    if ((mon.MoveEffectsMask & MoveEffectUproarWake) == 0 && (mon.MoveEffectsTemp & MoveEffectUproarWake) != 0)
                    {
                        mon.MoveEffectsTemp &= ~MoveEffectUproarWake;
                        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqWakeUpFromUproar);
                        battleCtx.MsgBattlerTemp = battleCtx.AfterMoveEffectTemp;
                        battleCtx.CommandNext = battleCtx.Command;
                        battleCtx.Command = ControllerCommandRunScript;
                        woke = true;
                    }
                    battleCtx.AfterMoveEffectTemp++;
                    if (woke)
                    {
                        return;
                    }
                }
                battleCtx.AfterMoveEffectState++;
                battleCtx.AfterMoveEffectTemp = 0;
                goto case 1;
            }
            case 1:
                battleCtx.AfterMoveEffectState++;
                if (BattleContexts.BattleSystem_SynchronizeStatus(battleSys, battleCtx, battleCtx.Command) == 1)
                {
                    return;
                }
                goto case 2;
            case 2:
                script = BattleContexts.BattleSystem_TriggerEffectOnSwitch(battleSys, battleCtx);
                if (script != 0)
                {
                    BattleController_RunSubseq(battleCtx, script);
                    return;
                }
                battleCtx.AfterMoveEffectState++;
                goto case 3;
            case 3:
                battleCtx.AfterMoveEffectState++;
                if (BattleContexts.BattleSystem_TriggerHeldItem(battleSys, battleCtx, battleCtx.Attacker) == 1)
                {
                    return;
                }
                goto case 4;
            case 4:
                battleCtx.AfterMoveEffectState++;
                if (battleCtx.Defender != 0xFF && BattleContexts.BattleSystem_TriggerHeldItem(battleSys, battleCtx, battleCtx.Defender) == 1)
                {
                    return;
                }
                goto case 5;
            case 5:
                battleCtx.AfterMoveEffectState++;
                script = 0;
                if (BattleContexts.BattleSystem_TriggerHeldItemOnHit(battleSys, battleCtx, ref script) == 1)
                {
                    BattleController_RunSubseq(battleCtx, script);
                    return;
                }
                goto case 6;
            case 6:
            {
                int moveType;
                if (BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) == AbilityNormalize)
                {
                    moveType = 0;
                }
                else if (battleCtx.MoveType != 0)
                {
                    moveType = battleCtx.MoveType;
                }
                else
                {
                    moveType = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Type;
                }
                battleCtx.AfterMoveEffectState++;
                int defender = battleCtx.Defender;
                if (defender != 0xFF
                    && (battleCtx.BattleMons[defender].Status & StatusFreeze) != 0
                    && (battleCtx.MoveStatusFlags & MoveStatusMultiHitInterrupt) == 0
                    && defender != battleCtx.Attacker
                    && (battleCtx.SelfTurnFlags[defender].PhysicalDamageTaken != 0 || battleCtx.SelfTurnFlags[defender].SpecialDamageTaken != 0)
                    && battleCtx.BattleMons[defender].CurHP != 0
                    && moveType == TypeFire)
                {
                    battleCtx.MsgBattlerTemp = defender;
                    BattleController_RunSubseq(battleCtx, SubseqThawOut);
                    return;
                }
                goto case 7;
            }
            case 7:
            {
                bool triggered = false;
                while (battleCtx.AfterMoveEffectTemp < BattleSystems.BattleSystem_GetMaxBattlers(battleSys))
                {
                    int battler = battleCtx.MonSpeedOrder[battleCtx.AfterMoveEffectTemp];
                    if ((HealthBar.FlagIndex(battler) & battleCtx.BattlersSwitchingMask) != 0)
                    {
                        battleCtx.AfterMoveEffectTemp++;
                        continue;
                    }
                    battleCtx.AfterMoveEffectTemp++;
                    script = 0;
                    if (BattleContexts.BattleSystem_TriggerHeldItemOnSwitchIn(battleSys, battleCtx, battler, ref script) == 1)
                    {
                        battleCtx.MsgBattlerTemp = battler;
                        BattleController_RunSubseq(battleCtx, script);
                        triggered = true;
                        break;
                    }
                }
                if (!triggered)
                {
                    battleCtx.AfterMoveEffectState++;
                    battleCtx.AfterMoveEffectTemp = 0;
                }
                break;
            }
        }
        battleCtx.AfterMoveEffectState = 0;
        battleCtx.AfterMoveEffectTemp = 0;
        battleCtx.Command = ControllerCommandLoopMultiHit;
    }

    // OFFSET 0x0225016C
    private static void BattleController_LeftoverState32(BattleSystem battleSys, BattleContext battleCtx)
    {
    }

    // OFFSET 0x02250170
    private static void BattleController_LoopMultiHit(BattleSystem battleSys, BattleContext battleCtx)
    {
        if (battleCtx.MultiHitNumHits == 0)
        {
            battleCtx.Command = ControllerCommandLoopWhileFainted;
            return;
        }
        if (battleCtx.FaintedMon == 0xFF
            && (battleCtx.BattleMons[battleCtx.Attacker].Status & StatusSleep) == 0
            && (battleCtx.MoveStatusFlags & MoveStatusMultiHitInterrupt) == 0)
        {
            battleCtx.MultiHitCounter--;
            if (battleCtx.MultiHitCounter != 0)
            {
                battleCtx.MultiHitLoop = 1;
                BattleContexts.BattleSystem_SetupLoop(battleSys, battleCtx);
                battleCtx.BattleStatusMask &= ~BattleStatusMultiHitSemiInvuln;
                battleCtx.MultiHitCheckFlags = battleCtx.MultiHitAccuracyCheck;
                BattleContexts.BattleSystem_LoadScript(battleCtx, 0, (int)battleCtx.MoveCur);
                battleCtx.Command = ControllerCommandRunScript;
                battleCtx.CommandNext = ControllerCommandTryMove;
            }
            else
            {
                battleCtx.MsgTemp = battleCtx.MultiHitNumHits;
                BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqMultiHitMessage);
                battleCtx.Command = ControllerCommandRunScript;
                battleCtx.CommandNext = ControllerCommandLoopWhileFainted;
            }
        }
        else
        {
            if (battleCtx.FaintedMon != 0xFF || (battleCtx.BattleMons[battleCtx.Attacker].Status & StatusSleep) != 0)
            {
                battleCtx.MsgTemp = battleCtx.MultiHitNumHits - battleCtx.MultiHitCounter + 1;
            }
            else
            {
                battleCtx.MsgTemp = battleCtx.MultiHitNumHits - battleCtx.MultiHitCounter;
            }
            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqMultiHitMessage);
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = ControllerCommandLoopWhileFainted;
        }
        BattleIO.BattleIO_ClearMessageBox(battleSys);
    }

    // OFFSET 0x02250270
    private static void BattleController_LoopWhileFainted(BattleSystem battleSys, BattleContext battleCtx)
    {
        if ((battleCtx.BattleStatusMask & BattleStatusFaintedMask) != 0)
        {
            BattleController_AnyFainted(battleCtx, ControllerCommandLoopWhileFainted, ControllerCommandLoopWhileFainted, 0);
            return;
        }
        battleCtx.Command = ControllerCommandLoopSpreadMoves;
    }

    // OFFSET 0x02250298
    private static void BattleController_LoopSpreadMoves(BattleSystem battleSys, BattleContext battleCtx)
    {
        if ((battleCtx.BattleStatusMask2 & BattleStatus2MagicCoat) != 0)
        {
            battleCtx.BattleStatusMask2 &= ~BattleStatus2MagicCoat;
            battleCtx.Defender = battleCtx.Attacker;
            battleCtx.Attacker = battleCtx.MagicCoatMon;
        }
        BattleController_UpdateFlagsWhenHit(battleSys, battleCtx);
        ushort range = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Range;
        if (range == RangeAllOpponents && (battleCtx.BattleStatusMask & BattleStatusSingleTarget) == 0 && battleCtx.BattlerCounter < BattleSystems.BattleSystem_GetMaxBattlers(battleSys))
        {
            battleCtx.MultiHitCheckFlags = MultiHitCheckSpread;
            BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
            int attackerIsEnemy = Battlers.Battler_GetType(BattleSystems.BattleSystem_GetBattlerData(battleSys, battleCtx.Attacker)!) & 1;
            do
            {
                int battler = battleCtx.MonSpeedOrder[battleCtx.BattlerCounter++];
                if ((HealthBar.FlagIndex(battler) & battleCtx.BattlersSwitchingMask) == 0 && battleCtx.BattleMons[battler].CurHP != 0)
                {
                    BattlerData data = BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!;
                    if ((attackerIsEnemy != 0 && (Battlers.Battler_GetType(data) & 1) == 0) || (attackerIsEnemy == 0 && (Battlers.Battler_GetType(data) & 1) != 0))
                    {
                        BattleContexts.BattleSystem_SetupLoop(battleSys, battleCtx);
                        battleCtx.Defender = battler;
                        battleCtx.Command = ControllerCommandUseMove;
                        break;
                    }
                }
            }
            while (battleCtx.BattlerCounter < BattleSystems.BattleSystem_GetMaxBattlers(battleSys));
            BattleIO.BattleIO_ClearMessageBox(battleSys);
            return;
        }
        range = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Range;
        if (range == RangeAllExceptUser && (battleCtx.BattleStatusMask & BattleStatusSingleTarget) == 0 && battleCtx.BattlerCounter < BattleSystems.BattleSystem_GetMaxBattlers(battleSys))
        {
            battleCtx.MultiHitCheckFlags = MultiHitCheckSpread;
            BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
            do
            {
                int battler = battleCtx.MonSpeedOrder[battleCtx.BattlerCounter++];
                if ((HealthBar.FlagIndex(battler) & battleCtx.BattlersSwitchingMask) == 0 && battleCtx.BattleMons[battler].CurHP != 0 && battler != battleCtx.Attacker)
                {
                    BattleContexts.BattleSystem_SetupLoop(battleSys, battleCtx);
                    battleCtx.Defender = battler;
                    battleCtx.Command = ControllerCommandUseMove;
                    break;
                }
            }
            while (battleCtx.BattlerCounter < BattleSystems.BattleSystem_GetMaxBattlers(battleSys));
            BattleIO.BattleIO_ClearMessageBox(battleSys);
            return;
        }
        battleCtx.Command = ControllerCommandFaintAfterSelfdestruct;
    }

    // OFFSET 0x02250438
    private static void BattleController_FaintAfterSelfdestruct(BattleSystem battleSys, BattleContext battleCtx)
    {
        uint mask = battleCtx.BattleStatusMask & BattleStatusSelfdestructMask;
        if (mask != 0)
        {
            battleCtx.FaintedMon = MathUtil.MathUtil_LowestSetBit(mask >> 28);
            battleCtx.BattleStatusMask &= 0x0FFFFFFF;
            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqSelfdestructFaint);
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = ControllerCommandTriggerAfterHitEffects;
            return;
        }
        battleCtx.Command = ControllerCommandTriggerAfterHitEffects;
    }

    // OFFSET 0x02250480
    private static void BattleController_TriggerAfterHitEffects(BattleSystem battleSys, BattleContext battleCtx)
    {
        if (BattleController_TriggerAfterMoveHitEffects(battleSys, battleCtx) != 1)
        {
            battleCtx.Command = ControllerCommandUpdateMoveBuffers;
        }
    }

    // OFFSET 0x02250494
    private static void BattleController_LeftoverState37(BattleSystem battleSys, BattleContext battleCtx)
    {
    }

    // OFFSET 0x02250498
    private static void BattleController_UpdateMoveBuffers(BattleSystem battleSys, BattleContext battleCtx)
    {
        byte itemEffect = (byte)BattleContexts.Battler_HeldItemEffect(battleCtx, battleCtx.Attacker);
        if ((battleCtx.BattleStatusMask & BattleStatusSkipMoveBuffers) != 0 || (battleCtx.BattleStatusMask2 & BattleStatus2MoveHit) != 0)
        {
            if (itemEffect == HoldEffectChoiceBand || itemEffect == HoldEffectChoiceScarf || itemEffect == HoldEffectChoiceSpecs)
            {
                uint move = battleCtx.MoveTemp;
                if (move != MoveStruggle
                    && !(move == MoveUTurn && (battleCtx.BattleStatusMask2 & BattleStatus2UTurn) != 0)
                    && !(move == MoveBatonPass && (battleCtx.BattleStatusMask2 & BattleStatus2BatonPass) != 0))
                {
                    battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.ChoiceLockedMove = (ushort)move;
                }
            }
            else
            {
                battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.ChoiceLockedMove = 0;
            }
        }
        if ((battleCtx.BattleStatusMask & BattleStatusMoveFailed) == 0)
        {
            if ((battleCtx.BattleStatusMask2 & BattleStatus2MoveHit) != 0)
            {
                battleCtx.MoveProtect[battleCtx.Attacker] = (ushort)battleCtx.MoveCur;
                battleCtx.MovePrev = battleCtx.MoveTemp;
            }
            else
            {
                battleCtx.MoveProtect[battleCtx.Attacker] = 0;
                battleCtx.MovePrev = 0;
            }
            if ((battleCtx.BattleStatusMask2 & BattleStatus2BatonPass) != 0)
            {
                battleCtx.MovePrevByBattler[battleCtx.Attacker] = (ushort)battleCtx.MoveTemp;
            }
            else
            {
                battleCtx.MovePrevByBattler[battleCtx.Attacker] = 0;
            }
        }
        if ((battleCtx.BattleStatusMask2 & BattleStatus2MoveHit) != 0)
        {
            battleCtx.MoveSketched[battleCtx.Attacker] = (ushort)battleCtx.MoveTemp;
        }
        BattleController_UpdateFlagsWhenHit(battleSys, battleCtx);
        BattleContexts.BattleSystem_VerifyMetronomeCount(battleSys, battleCtx);
        battleCtx.Command = ControllerCommandCheckAnyFainted;
    }

    // OFFSET 0x022505C4
    private static void BattleController_MoveEnd(BattleSystem battleSys, BattleContext battleCtx)
    {
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeNoFieldEffects) == 0)
        {
            if (BattleContexts.BattleSystem_RecoverStatusByAbility(battleSys, battleCtx, battleCtx.Attacker, 0) == 1)
            {
                return;
            }
            if (battleCtx.Defender != 0xFF && BattleContexts.BattleSystem_RecoverStatusByAbility(battleSys, battleCtx, battleCtx.Defender, 0) == 1)
            {
                return;
            }
            if (BattleController_AnyExpPayout(battleCtx, battleCtx.Command, battleCtx.Command) == 1)
            {
                return;
            }
            if (BattleController_CheckBattleOver(battleSys, battleCtx) == 1)
            {
                return;
            }
            int script = BattleContexts.BattleSystem_TriggerEffectOnSwitch(battleSys, battleCtx);
            if (script != 0)
            {
                BattleController_RunSubseq(battleCtx, script);
                return;
            }
            if (BattleController_ToggleSemiInvulnMons(battleSys, battleCtx) == 1)
            {
                return;
            }
            BattleController_ClearFlags(battleSys, battleCtx);
        }
        battleCtx.BattlerActions[battleCtx.BattlerActionOrder[battleCtx.TurnOrderCounter], 0] = ControllerCommandNone;
        if (battleCtx.SelfTurnFlags[battleCtx.Attacker].TrickRoomActivated != 0)
        {
            BattleContexts.BattleSystem_SortBattlerActionOrder(battleSys, battleCtx);
            BattleContexts.BattleSystem_SortMonSpeedOrder(battleSys, battleCtx);
            battleCtx.TurnOrderCounter = 0;
        }
        else
        {
            battleCtx.TurnOrderCounter++;
        }
        BattleContexts.BattleContext_InitCounters(battleCtx);
        battleCtx.Command = ControllerCommandSortSpeed;
    }

    // OFFSET 0x022506A4
    private static void BattleController_CheckAnyFainted(BattleSystem battleSys, BattleContext battleCtx)
    {
        if (BattleController_AnyFainted(battleCtx, battleCtx.Command, battleCtx.Command, 1) != 1)
        {
            battleCtx.Command = ControllerCommandNone;
        }
    }

    // OFFSET 0x022506C0
    private static void BattleController_HandleResult(BattleSystem battleSys, BattleContext battleCtx)
    {
        if ((Battle.BattleSystem_GetResultMask(battleSys) & ResultLose) != 0)
        {
            battleCtx.Command = ControllerCommandHandleResult;
        }
        else if (Battle.BattleSystem_GetResultMask(battleSys) == 2 || Battle.BattleSystem_GetResultMask(battleSys) == 3)
        {
            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqLoseBattle);
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = ControllerCommandHandleResult;
        }
        else if (Battle.BattleSystem_GetResultMask(battleSys) == 1)
        {
            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqWinBattle);
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = ControllerCommandHandleResult;
        }
        else if (Battle.BattleSystem_GetResultMask(battleSys) == 4)
        {
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = ControllerCommandHandleResult;
        }
        else if (Battle.BattleSystem_GetResultMask(battleSys) == 5)
        {
            battleCtx.Command = ControllerCommandHandleResult;
        }
        battleCtx.BattleEndFlag = 1;
    }

    // OFFSET 0x0225074C
    private static void BattleController_ScreenWipe(BattleSystem battleSys, BattleContext battleCtx)
    {
        if (ScreenFadeControl.IsScreenFadeDone())
        {
            battleCtx.Command = ControllerCommandHandleResult;
        }
    }

    // OFFSET 0x02250760
    private static void BattleController_EndFight(BattleSystem battleSys, BattleContext battleCtx)
    {
        uint isLink = BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeLinkEnd;
        if (isLink == 0)
        {
            Party party = BattleSystems.BattleSystem_GetParty(battleSys, 0);
            Pokemon.Party_GivePokerusAtRandom(party);
            Pokemon.Party_SpreadPokerus(party);
        }
        if (isLink != 0)
        {
            CommSystem.CommSys_SendEmptyData(CommCommandBattleEnd);
        }
        battleCtx.Command = ControllerCommandEndWait;
    }

    // OFFSET 0x02250798
    private static void BattleController_EndWait(BattleSystem battleSys, BattleContext battleCtx)
    {
    }

    // OFFSET 0x02250E8C
    private static void BattleController_ClearFlags(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        for (int battler = 0; battler < maxBattlers; battler++)
        {
            battleCtx.BattleMons[battler].StatusVolatile &= ~battleCtx.ClearVolatileStatus[battler];
            battleCtx.ClearVolatileStatus[battler] = 0;
        }
        battleCtx.MoveHit[battleCtx.Attacker] = 0;
        battleCtx.Conversion2Move[battleCtx.Attacker] = 0;
    }

    // OFFSET 0x02250EF4
    private static int BattleController_AnyFainted(BattleContext battleCtx, int nextCmd, int nextCmdNoFainted, int onlyFaint)
    {
        int i = 0;
        uint flag = HealthBar.FlagIndex(battleCtx.MonSpeedOrder[0]) << 24;
        if ((battleCtx.BattleStatusMask & BattleStatusFaintedMask) != 0)
        {
            if ((battleCtx.BattleStatusMask & flag) == 0)
            {
                do
                {
                    i++;
                    flag = HealthBar.FlagIndex(battleCtx.MonSpeedOrder[i]) << 24;
                }
                while ((battleCtx.BattleStatusMask & flag) == 0);
            }
            battleCtx.BattleStatusMask &= ~flag;
            battleCtx.FaintedMon = MathUtil.MathUtil_LowestSetBit((uint)((int)flag >> 24));
            if (onlyFaint == 1)
            {
                BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqFaintMon);
            }
            else
            {
                BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqFaintMonWithMessage);
            }
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = nextCmd;
            battleCtx.BattlerActions[battleCtx.FaintedMon, 0] = ControllerCommandNone;
            return 1;
        }
        battleCtx.Command = nextCmdNoFainted;
        return 0;
    }

    // OFFSET 0x02250F98
    private static int BattleController_AnyExpPayout(BattleContext battleCtx, int nextCmd, int nextCmdNoExp)
    {
        if ((battleCtx.BattleStatusMask2 & BattleStatus2ExpMask) != 0)
        {
            uint flag = 0x10000000;
            while ((battleCtx.BattleStatusMask2 & flag) == 0)
            {
                flag <<= 1;
            }
            battleCtx.BattleStatusMask2 &= ~flag;
            battleCtx.FaintedMon = MathUtil.MathUtil_LowestSetBit((uint)((int)flag >> 28));
            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqGiveExp);
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = nextCmd;
            return 1;
        }
        battleCtx.Command = nextCmdNoExp;
        return 0;
    }

    // OFFSET 0x02250FF4
    private static void BattleController_UpdateFlagsWhenHit(BattleSystem battleSys, BattleContext battleCtx)
    {
        int moveType;
        if (BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) == AbilityNormalize)
        {
            moveType = 0;
        }
        else
        {
            moveType = battleCtx.MoveType;
            if (moveType == 0)
            {
                moveType = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Type;
            }
        }
        uint moveTemp = battleCtx.MoveTemp;
        if ((battleCtx.AiContext.MoveTable[moveTemp].Flags & MoveFlagCopyable) != 0
            && (battleCtx.BattleStatusMask & BattleStatusMoveFailed) == 0
            && battleCtx.Defender != 0xFF
            && (battleCtx.BattleStatusMask2 & BattleStatus2MoveHit) != 0)
        {
            battleCtx.MoveCopied[battleCtx.Defender] = (ushort)moveTemp;
            battleCtx.MoveCopiedHit[battleCtx.Defender, battleCtx.Attacker] = (ushort)battleCtx.MoveTemp;
        }
        if (battleCtx.Defender == 0xFF)
        {
            return;
        }
        byte itemEffect = (byte)BattleContexts.Battler_HeldItemEffect(battleCtx, battleCtx.Defender);
        if (((battleCtx.BattleStatusMask & BattleStatusSkipMoveBuffers) != 0 || (battleCtx.BattleStatusMask2 & BattleStatus2MoveHit) != 0)
            && itemEffect != HoldEffectChoiceBand && itemEffect != HoldEffectChoiceScarf && itemEffect != HoldEffectChoiceSpecs)
        {
            battleCtx.BattleMons[battleCtx.Defender].MoveEffectsData.ChoiceLockedMove = 0;
        }
        if ((battleCtx.BattleStatusMask & BattleStatusMoveFailed) != 0)
        {
            return;
        }
        if ((battleCtx.BattleStatusMask2 & BattleStatus2MoveHit) != 0)
        {
            battleCtx.MoveHit[battleCtx.Defender] = (ushort)battleCtx.MoveCur;
            battleCtx.MoveHitBattler[battleCtx.Defender] = (ushort)battleCtx.Attacker;
            battleCtx.MoveHitType[battleCtx.Defender] = (ushort)moveType;
            battleCtx.MovePrev = battleCtx.MoveTemp;
        }
        else
        {
            battleCtx.MoveHit[battleCtx.Defender] = 0;
            battleCtx.MoveHitBattler[battleCtx.Defender] = 0xFF;
            battleCtx.MoveHitType[battleCtx.Defender] = 0;
            battleCtx.MovePrev = 0;
        }
        if ((battleCtx.BattleStatusMask2 & BattleStatus2BatonPass) != 0 && (battleCtx.MoveStatusFlags & MoveStatusNoEffectFlinch) == 0)
        {
            uint move = battleCtx.MoveCur;
            ushort range = battleCtx.AiContext.MoveTable[move].Range;
            if (range == 0x10 || range == 0x20 || range == 0x40 || range == 0x80 || range == 0x100 || range == 0x200)
            {
                battleCtx.Conversion2Move[battleCtx.Defender] = 0;
                battleCtx.Conversion2Battler[battleCtx.Defender] = 0xFF;
                battleCtx.Conversion2Type[battleCtx.Defender] = 0;
                return;
            }
            battleCtx.Conversion2Move[battleCtx.Defender] = (ushort)move;
            battleCtx.Conversion2Battler[battleCtx.Defender] = (ushort)battleCtx.Attacker;
            battleCtx.Conversion2Type[battleCtx.Defender] = (ushort)moveType;
            return;
        }
        battleCtx.Conversion2Move[battleCtx.Defender] = 0;
        battleCtx.Conversion2Battler[battleCtx.Defender] = 0xFF;
        battleCtx.Conversion2Type[battleCtx.Defender] = 0;
    }

    // OFFSET 0x022511FC
    private static int BattleController_CriticalMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleController_RunSubseq(battleCtx, SubseqCriticalHit);
        return 1;
    }

    // OFFSET 0x02251218
    private static int BattleController_FollowupMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        int result = 0;
        if (battleCtx.MoveStatusFlags != 0)
        {
            if (battleCtx.MultiHitNumHits == 0)
            {
                result = 1;
            }
            else if (battleCtx.FaintedMon != 0xFF || battleCtx.MultiHitCounter == 1 || (battleCtx.MoveStatusFlags & MoveStatusMultiHitInterrupt) != 0)
            {
                result = 1;
            }
        }
        if (result == 1)
        {
            BattleController_RunSubseq(battleCtx, SubseqFollowupMessage);
        }
        return result;
    }

    // OFFSET 0x0225126C
    private static int BattleController_RageBuilding(BattleSystem battleSys, BattleContext battleCtx)
    {
        int defender = battleCtx.Defender;
        if (defender == 0xFF)
        {
            return 0;
        }
        BattleMon mon = battleCtx.BattleMons[defender];
        if ((mon.StatusVolatile & VolatileRage) != 0
            && (battleCtx.MoveStatusFlags & MoveStatusMultiHitInterrupt) == 0
            && defender != battleCtx.Attacker
            && mon.CurHP != 0
            && (battleCtx.SelfTurnFlags[defender].PhysicalDamageTaken != 0 || battleCtx.SelfTurnFlags[defender].SpecialDamageTaken != 0)
            && mon.StatBoosts[1] < 12)
        {
            mon.StatBoosts[1]++;
            BattleController_RunSubseq(battleCtx, SubseqRageBuilding);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x022512F8
    private static int BattleController_CheckExtraFlinch(BattleSystem battleSys, BattleContext battleCtx)
    {
        int itemEffect = BattleContexts.Battler_HeldItemEffect(battleCtx, battleCtx.Attacker);
        int itemPower = BattleContexts.Battler_HeldItemPower(battleCtx, battleCtx.Attacker, 0);
        int defender = battleCtx.Defender;
        if (defender != 0xFF
            && itemEffect == HoldEffectKingsRock
            && (battleCtx.MoveStatusFlags & MoveStatusNoEffectFlinch) == 0
            && (battleCtx.SelfTurnFlags[defender].PhysicalDamageTaken != 0 || battleCtx.SelfTurnFlags[defender].SpecialDamageTaken != 0)
            && BattleSystems.BattleSystem_RandNext(battleSys) % 100 < itemPower
            && (battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Flags & MoveFlagKingsRock) != 0
            && battleCtx.BattleMons[battleCtx.Defender].CurHP != 0)
        {
            battleCtx.SideEffectMon = battleCtx.Defender;
            battleCtx.SideEffectType = 2;
            BattleController_RunSubseq(battleCtx, SubseqFlinch);
            return 1;
        }
        return 0;
    }
}

public sealed partial class Pokemon
{
    private const int PokerusParam = 0x9A;
    private const int PokerusChance = 0x4000;

    // OFFSET 0x020776B0
    public static void Party_GivePokerusAtRandom(Party party)
    {
        int count = Party.Party_GetCurrentCount(party);
        ushort rand = MathUtil.LCRNG_Next();
        if (rand != PokerusChance && rand != PokerusChance * 2 && rand != PokerusChance * 3)
        {
            return;
        }
        Pokemon mon;
        int slot;
        do
        {
            slot = MathUtil.LCRNG_Next() % count;
            mon = Party.Party_GetPokemonBySlotIndex(party, slot);
        }
        while (Pokemon_GetValue(mon, ParamSpecies, null) == 0 || Pokemon_GetValue(mon, 0x4C, null) != 0);
        if (Party_MaskMonsWithPokerus(party, (byte)HealthBar.FlagIndex(slot)) != 0)
        {
            return;
        }
        byte pokerus;
        do
        {
            pokerus = (byte)MathUtil.LCRNG_Next();
        }
        while ((pokerus & 7) == 0);
        if ((pokerus & 0xF0) != 0)
        {
            pokerus &= 7;
        }
        pokerus |= (byte)(pokerus << 4);
        pokerus &= 0xF3;
        pokerus++;
        Pokemon_SetValue(mon, PokerusParam, pokerus);
    }

    // OFFSET 0x02077758
    public static byte Party_MaskMonsWithPokerus(Party party, byte mask)
    {
        int slot = 0;
        int flag = 1;
        byte result = 0;
        if (mask != 0)
        {
            do
            {
                if ((mask & 1) != 0 && Pokemon_GetValue(Party.Party_GetPokemonBySlotIndex(party, slot), PokerusParam, null) != 0)
                {
                    result = (byte)(result | flag);
                }
                slot++;
                flag <<= 1;
                mask = (byte)(mask >> 1);
            }
            while (mask != 0);
        }
        else if (Pokemon_GetValue(Party.Party_GetPokemonBySlotIndex(party, slot), PokerusParam, null) != 0)
        {
            result++;
        }
        return result;
    }

    // OFFSET 0x0207782C
    public static void Party_SpreadPokerus(Party party)
    {
        int count = Party.Party_GetCurrentCount(party);
        if (MathUtil.LCRNG_Next() % 3 != 0)
        {
            return;
        }
        for (int slot = 0; slot < count; slot++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, slot);
            if (Pokemon_GetValue(mon, ParamSpecies, null) == 0)
            {
                continue;
            }
            byte pokerus = (byte)Pokemon_GetValue(mon, PokerusParam, null);
            if ((pokerus & 0xF) == 0)
            {
                continue;
            }
            if (slot != 0)
            {
                Pokemon prev = Party.Party_GetPokemonBySlotIndex(party, slot - 1);
                if ((Pokemon_GetValue(prev, PokerusParam, null) & 0xF0) == 0)
                {
                    Pokemon_SetValue(prev, PokerusParam, pokerus);
                }
            }
            if (slot < count - 1)
            {
                Pokemon next = Party.Party_GetPokemonBySlotIndex(party, slot + 1);
                if ((Pokemon_GetValue(next, PokerusParam, null) & 0xF0) == 0)
                {
                    Pokemon_SetValue(next, PokerusParam, pokerus);
                    slot++;
                }
            }
        }
    }
}

public static partial class CommSystem
{
    // OFFSET 0x020359DC
    public static int CommSys_SendData(int cmd, byte[]? data, int size)
    {
        ushort netID = CommSys_CurNetId();
        if (CommSys_IsPlayerConnected(netID) == 0 && CommSystemField.CommSys_IsAlone() == 0)
        {
            return 0;
        }
        throw Missing(0x02032498);
    }

    // OFFSET 0x020360DC
    public static int CommSys_SendEmptyData(int cmd)
    {
        return CommSys_SendData(cmd, null, 0);
    }
}
