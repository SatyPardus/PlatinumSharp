namespace PlatinumSharp.Game;

public static partial class BattleControllers
{
    private const uint BattleTypeNoObedience = 0x84;
    private const uint BattleTypeAIPartner = 0x40;
    private const int BattlerSlotPlayer2 = 4;
    private const ushort MoveBide = 0x75;
    private const ushort MoveSnore = 0xAD;
    private const ushort MoveSleepTalk = 0xD6;
    private const ushort MovePound = 1;
    private const uint BattleStatusMoveCalled = 0x200;
    private const uint BattleStatusFailed = 2;
    private const int ObedienceMaxBadges = 8;
    private const int ObedienceUseSleepMove = 0xFE;
    private const int ObedienceLoafing = 0xFF;
    private const int ObedienceOtherMove = 0x100;
    private const int ObedienceNap = 0x101;
    private const int ObedienceHitSelf = 0x102;
    private const int ObedienceResultNone = 0;
    private const int ObedienceResultEnd = 1;
    private const int ObedienceResultRepeat = 2;
    private const int ObedienceResultHitSelf = 3;
    private const uint MultiHitCalledOtherMove = 1;
    private const int ActionParamTarget = 1;
    private const int AbilityVitalSpirit = 0x48;
    private const int AbilityInsomnia = 0xF;
    private const uint FieldConditionUproar = 0xF00;
    private const ushort MoveImprison = 0x11E;
    private const int AbilityPressure = 0x2E;
    private const int CountAbilityAllExceptOwn = 9;
    private const int CountAbilityOtherSideAlive = 3;
    private const uint MoveStatusNoPP = 0x200;
    private const uint VolatileBide = 0x1000;
    private const uint VolatileThrash = 0xC00;
    private const int SubSeqNoTarget = 0x119;
    private const int ControllerCommandMoveEnd = 0x26;
    private const int ControllerCommandHitSelf = 0x21;
    private const int AbilityCloudNine = 0xD;
    private const int AbilityAirLock = 0x4C;
    private const int CountAbilityField = 8;
    private const int EffectSolarBeam = 0x97;
    private const uint FieldConditionSun = 0x30;
    private const int HoldEffectPowerHerb = 0x63;
    private const ushort RangeAllies = 0x10;
    private const ushort RangeUser = 0x20;
    private const uint BattleStatusIgnoreTypeChart = 0x8000;
    private const uint BattleStatusTypeChartCheck = 0x20;
    private const ushort MoveThunderWave = 0x56;
    private const uint MoveStatusNoEffect = 8;
    private const uint VolatileFlinch = 8;
    private const uint VolatileRecharge = 0x400000;
    private const uint VolatileConfusion = 7;
    private const uint VolatileAttract = 0xF0000;
    private const uint VolatileBideTurns = 0x300;
    private const uint VolatileUnknown25 = 0x2000000;
    private const uint MoveEffectUnknown14 = 0x4000;
    private const uint StatusFreeze = 0x20;
    private const uint StatusParalysis = 0x40;
    private const int AbilitySoundproof = 0x2B;
    private const int AbilityEarlyBird = 0x30;
    private const int AbilityMagicGuard = 0x62;
    private const int EffectThawFlameWheel = 0x7D;
    private const int EffectThawSacredFire = 0xFD;
    private const int StatusCheckDone = 3;
    private const int StatusCheckEndTurn = 1;
    private const int StatusCheckContinue = 2;
    private const uint MoveStatusIneffectiveMask = 0x1FD849;
    private const int SubSeqImmunityWaterAbsorbFail = 0xB5;

    // OFFSET 0x0224E13C
    private static int BattleController_CheckObedience(BattleSystem battleSys, BattleContext battleCtx, out int script)
    {
        script = 0;
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        TrainerInfo trainerInfo = BattleSystems.BattleSystem_GetTrainerInfo(battleSys, 0);
        if ((battleType & BattleTypeNoObedience) != 0)
        {
            return ObedienceResultNone;
        }
        if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker) != 0)
        {
            return ObedienceResultNone;
        }
        if ((battleType & BattleTypeAIPartner) != 0 && BattleSystems.BattleSystem_BattlerSlot(battleSys, battleCtx.Attacker) == BattlerSlotPlayer2)
        {
            return ObedienceResultNone;
        }
        if (BattleContexts.BattleSystem_TrainerIsOT(battleSys, battleCtx) == 1)
        {
            return ObedienceResultNone;
        }
        if (BattleContexts.Battler_CanPickCommand(battleCtx, battleCtx.Attacker) == 0)
        {
            return ObedienceResultNone;
        }
        if (battleCtx.MoveCur == MoveBide && (battleCtx.BattleStatusMask & BattleStatusMoveCalled) != 0)
        {
            return ObedienceResultNone;
        }
        if (TrainerInfo.TrainerInfo_BadgeCount(trainerInfo) >= ObedienceMaxBadges)
        {
            return ObedienceResultNone;
        }
        int cap = 10;
        if (TrainerInfo.TrainerInfo_BadgeCount(trainerInfo) >= 2)
        {
            cap = 30;
        }
        if (TrainerInfo.TrainerInfo_BadgeCount(trainerInfo) >= 4)
        {
            cap = 50;
        }
        if (TrainerInfo.TrainerInfo_BadgeCount(trainerInfo) >= 6)
        {
            cap = 70;
        }
        BattleMon mon = battleCtx.BattleMons[battleCtx.Attacker];
        if (mon.Level <= cap)
        {
            return ObedienceResultNone;
        }
        int roll = (int)(BattleSystems.BattleSystem_RandNext(battleSys) & 0xFF) * (cap + mon.Level) >> 8;
        if (roll < cap)
        {
            return ObedienceResultNone;
        }
        if (battleCtx.MoveCur == MoveRage)
        {
            mon.StatusVolatile &= ~VolatileRage;
        }
        mon = battleCtx.BattleMons[battleCtx.Attacker];
        if ((mon.Status & StatusSleep) != 0 && (battleCtx.MoveCur == MoveSnore || battleCtx.MoveCur == MoveSleepTalk))
        {
            script = ObedienceUseSleepMove;
            return ObedienceResultEnd;
        }
        int level = mon.Level;
        roll = (int)(BattleSystems.BattleSystem_RandNext(battleSys) & 0xFF) * (cap + level) >> 8;
        if (roll < cap)
        {
            int invalid = BattleContexts.BattleSystem_CheckInvalidMoves(battleSys, battleCtx, battleCtx.Attacker, (int)HealthBar.FlagIndex(battleCtx.MoveSlot[battleCtx.Attacker]), -1);
            if (invalid == 0xF)
            {
                script = ObedienceLoafing;
                return ObedienceResultEnd;
            }
            int slot;
            do
            {
                slot = (int)(BattleSystems.BattleSystem_RandNext(battleSys) & 3);
            } while ((HealthBar.FlagIndex(slot) & invalid) != 0);
            battleCtx.MoveSlot[battleCtx.Attacker] = (ushort)slot;
            battleCtx.MoveTemp = battleCtx.BattleMons[battleCtx.Attacker].Moves[battleCtx.MoveSlot[battleCtx.Attacker]];
            battleCtx.MoveCur = battleCtx.MoveTemp;
            battleCtx.Defender = BattleContexts.BattleSystem_Defender(battleSys, battleCtx, battleCtx.Attacker, (ushort)battleCtx.MoveTemp, 1, 0);
            int target = battleCtx.Defender;
            if (target == 0xFF)
            {
                target = BattleContexts.BattleSystem_RandomOpponent(battleSys, battleCtx, battleCtx.Attacker);
            }
            battleCtx.BattlerActions[battleCtx.Attacker, ActionParamTarget] = (uint)target;
            script = ObedienceOtherMove;
            battleCtx.MultiHitCheckFlags |= MultiHitCalledOtherMove;
            return ObedienceResultRepeat;
        }
        int diff = (byte)(level - cap);
        int roll2 = (int)(BattleSystems.BattleSystem_RandNext(battleSys) & 0xFF);
        if (roll2 < diff
            && (battleCtx.BattleMons[battleCtx.Attacker].Status & 0xFF) == 0
            && BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) != AbilityVitalSpirit
            && BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) != AbilityInsomnia
            && (battleCtx.FieldConditionsMask & FieldConditionUproar) == 0)
        {
            script = ObedienceNap;
            return ObedienceResultEnd;
        }
        if (roll2 - diff < diff)
        {
            battleCtx.Defender = battleCtx.Attacker;
            battleCtx.MsgBattlerTemp = battleCtx.Attacker;
            battleCtx.HpCalcTemp = BattleContexts.BattleSystem_CalcMoveDamage(battleSys, battleCtx, MovePound, 0, 0, 40, 0, (byte)battleCtx.Attacker, (byte)battleCtx.Attacker, 1);
            battleCtx.HpCalcTemp = BattleContexts.BattleSystem_CalcDamageVariance(battleSys, battleCtx, battleCtx.HpCalcTemp);
            battleCtx.HpCalcTemp *= -1;
            script = ObedienceHitSelf;
            battleCtx.BattleStatusMask |= BattleStatusFailed;
            return ObedienceResultHitSelf;
        }
        script = ObedienceLoafing;
        return ObedienceResultEnd;
    }

    // OFFSET 0x0224E458
    private static int BattleController_DecrementPP(BattleSystem battleSys, BattleContext battleCtx)
    {
        int pressure = 1;
        int attacker = battleCtx.Attacker;
        if (battleCtx.SelfTurnFlags[attacker].SkipPressureCheck == 0 && battleCtx.Defender != 0xFF)
        {
            uint move = battleCtx.MoveTemp;
            if (move == MoveImprison)
            {
                pressure += BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityOtherSideAlive, attacker, AbilityPressure);
            }
            else
            {
                switch (battleCtx.AiContext.MoveTable[move].Range)
                {
                    case 0x08:
                    case 0x40:
                        pressure += BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityAllExceptOwn, attacker, AbilityPressure);
                        break;
                    case 0x04:
                    case 0x80:
                        pressure += BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityOtherSideAlive, attacker, AbilityPressure);
                        break;
                    case 0x10:
                    case 0x20:
                    case 0x100:
                    case 0x200:
                        break;
                    default:
                        if (attacker != battleCtx.Defender && BattleContexts.Battler_Ability(battleCtx, battleCtx.Defender) == AbilityPressure)
                        {
                            pressure++;
                        }
                        break;
                }
            }
        }
        int slot = BattleContexts.Battler_SlotForMove(battleCtx.BattleMons[battleCtx.Attacker], (ushort)battleCtx.MoveTemp);
        TurnFlags turnFlags = battleCtx.TurnFlags[battleCtx.Attacker];
        BattleMon mon = battleCtx.BattleMons[battleCtx.Attacker];
        if (turnFlags.PpDecremented == 0 && turnFlags.Struggling == 0)
        {
            turnFlags.PpDecremented = 1;
            if (mon.PpCur[slot] != 0 && slot < 4)
            {
                mon.PpCur[slot] = (byte)(mon.PpCur[slot] > pressure ? mon.PpCur[slot] - pressure : 0);
                BattleContexts.BattleMon_CopyToParty(battleSys, battleCtx, battleCtx.Attacker);
            }
            else
            {
                battleCtx.MoveStatusFlags |= MoveStatusNoPP;
            }
        }
        else if (slot < 4 && mon.PpCur[slot] == 0
            && (battleCtx.BattleStatusMask & BattleStatusMoveCalled) == 0
            && (mon.StatusVolatile & VolatileBide) == 0
            && (mon.StatusVolatile & VolatileThrash) == 0
            && (battleCtx.FieldConditionsMask & (HealthBar.FlagIndex(battleCtx.Attacker) << 8)) == 0)
        {
            battleCtx.MoveStatusFlags |= MoveStatusNoPP;
        }
        return 0;
    }

    // OFFSET 0x0224E5F4
    private static int BattleController_CheckNoTarget(BattleSystem battleSys, BattleContext battleCtx)
    {
        int failed = 0;
        int solarBeamSun = 0;
        if ((battleCtx.Defender == 0xFF && BattleContexts.Move_IsMultiTurn(battleCtx, (int)battleCtx.MoveCur) == 0)
            || (battleCtx.Defender == 0xFF && BattleContexts.Move_IsMultiTurn(battleCtx, (int)battleCtx.MoveCur) == 1
                && ((battleCtx.BattleMons[battleCtx.Attacker].StatusVolatile & VolatileBide) != 0 || (battleCtx.BattleStatusMask & BattleStatusMoveCalled) != 0)))
        {
            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqNoTarget);
            battleCtx.CommandNext = ControllerCommandMoveEnd;
            battleCtx.Command = ControllerCommandRunScript;
            failed = 1;
        }
        if (BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityField, 0, AbilityCloudNine) == 0
            && BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityField, 0, AbilityAirLock) == 0
            && battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Effect == EffectSolarBeam
            && (battleCtx.FieldConditionsMask & FieldConditionSun) != 0)
        {
            solarBeamSun = 1;
        }
        if (battleCtx.Defender == 0xFF && BattleContexts.Move_IsMultiTurn(battleCtx, (int)battleCtx.MoveCur) == 1 && failed == 0 && solarBeamSun == 0
            && BattleContexts.Battler_HeldItemEffect(battleCtx, battleCtx.Attacker) != HoldEffectPowerHerb
            && (battleCtx.BattleMons[battleCtx.Attacker].StatusVolatile & VolatileBide) == 0)
        {
            battleCtx.Defender = battleCtx.Attacker;
        }
        return failed;
    }

    // OFFSET 0x0224E6F4
    private static int BattleController_CheckTypeImmunity(BattleSystem battleSys, BattleContext battleCtx)
    {
        uint move = battleCtx.MoveCur;
        MoveData data = battleCtx.AiContext.MoveTable[move];
        bool check = data.Range != RangeAllies && data.Range != RangeUser && data.Power != 0
            && (battleCtx.BattleStatusMask & BattleStatusIgnoreTypeChart) == 0
            && (battleCtx.BattleStatusMask & BattleStatusTypeChartCheck) == 0;
        if (check || move == MoveThunderWave)
        {
            battleCtx.Damage = BattleContexts.BattleSystem_ApplyTypeChart(battleSys, battleCtx, (int)move, battleCtx.MoveType, battleCtx.Attacker, battleCtx.Defender, battleCtx.Damage, ref battleCtx.MoveStatusFlags);
            if ((battleCtx.MoveStatusFlags & MoveStatusNoEffect) != 0)
            {
                battleCtx.MoveFailFlags[battleCtx.Attacker].NoEffect = 1;
            }
        }
        return 0;
    }

    private static void BattleController_LoadStatusScript(BattleContext battleCtx, int script, bool endTurn)
    {
        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, script);
        if (endTurn)
        {
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = ControllerCommandMoveEnd;
        }
        else
        {
            battleCtx.CommandNext = battleCtx.Command;
            battleCtx.Command = ControllerCommandRunScript;
        }
    }

    // OFFSET 0x0224E784
    private static int BattleController_CheckStatusConditions(BattleSystem battleSys, BattleContext battleCtx)
    {
        int result = 0;
        int effect = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Effect;
        do
        {
            int attacker = battleCtx.Attacker;
            BattleMon mon = battleCtx.BattleMons[attacker];
            MoveFailFlags failFlags = battleCtx.MoveFailFlags[attacker];
            switch (battleCtx.StatusCheckState)
            {
                case 0:
                    mon.StatusVolatile &= ~VolatileUnknown25;
                    mon.MoveEffectsMask &= ~MoveEffectUnknown14;
                    battleCtx.StatusCheckState++;
                    break;
                case 1:
                    if ((mon.Status & StatusSleep) != 0)
                    {
                        if ((battleCtx.FieldConditionsMask & FieldConditionUproar) != 0 && BattleContexts.Battler_Ability(battleCtx, attacker) != AbilitySoundproof)
                        {
                            battleCtx.MsgBattlerTemp = battleCtx.Attacker;
                            BattleController_LoadStatusScript(battleCtx, 0x13, false);
                            result = StatusCheckContinue;
                        }
                        else if (!(battleCtx.MoveCur != MoveSleepTalk && battleCtx.MoveTemp == MoveSleepTalk))
                        {
                            int decrement = BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) == AbilityEarlyBird ? 2 : 1;
                            mon = battleCtx.BattleMons[battleCtx.Attacker];
                            if ((mon.Status & StatusSleep) < decrement)
                            {
                                mon.Status &= ~StatusSleep;
                            }
                            else
                            {
                                mon.Status -= (uint)decrement;
                            }
                            if ((battleCtx.BattleMons[battleCtx.Attacker].Status & StatusSleep) != 0)
                            {
                                if (battleCtx.MoveCur != MoveSnore && battleCtx.MoveTemp != MoveSleepTalk)
                                {
                                    BattleController_LoadStatusScript(battleCtx, 0x14, true);
                                    result = StatusCheckContinue;
                                }
                            }
                            else
                            {
                                battleCtx.MsgBattlerTemp = battleCtx.Attacker;
                                BattleController_LoadStatusScript(battleCtx, 0x13, false);
                                result = StatusCheckContinue;
                            }
                        }
                    }
                    battleCtx.StatusCheckState++;
                    break;
                case 2:
                    if ((mon.Status & StatusFreeze) != 0)
                    {
                        if (BattleSystems.BattleSystem_RandNext(battleSys) % 5 != 0)
                        {
                            if (effect != EffectThawFlameWheel && effect != EffectThawSacredFire)
                            {
                                BattleController_LoadStatusScript(battleCtx, 0x1C, true);
                                result = StatusCheckEndTurn;
                            }
                        }
                        else
                        {
                            battleCtx.MsgBattlerTemp = battleCtx.Attacker;
                            BattleController_LoadStatusScript(battleCtx, 0x1D, false);
                            result = StatusCheckContinue;
                        }
                    }
                    battleCtx.StatusCheckState++;
                    break;
                case 3:
                    if (BattleContexts.Battler_CheckTruant(battleCtx, attacker) == 1)
                    {
                        BattleController_LoadStatusScript(battleCtx, 0xBF, true);
                        result = StatusCheckEndTurn;
                    }
                    battleCtx.StatusCheckState++;
                    break;
                case 4:
                    if ((mon.StatusVolatile & VolatileRecharge) != 0)
                    {
                        mon.StatusVolatile &= ~VolatileRecharge;
                        BattleController_LoadStatusScript(battleCtx, 0x41, true);
                        result = StatusCheckEndTurn;
                    }
                    battleCtx.StatusCheckState++;
                    break;
                case 5:
                    if ((mon.StatusVolatile & VolatileFlinch) != 0)
                    {
                        mon.StatusVolatile &= ~VolatileFlinch;
                        failFlags.Flinched = 1;
                        BattleController_LoadStatusScript(battleCtx, 0xF, true);
                        result = StatusCheckEndTurn;
                    }
                    battleCtx.StatusCheckState++;
                    break;
                case 6:
                    if (battleCtx.MoveTemp == mon.MoveEffectsData.DisabledMove)
                    {
                        failFlags.Disabled = 1;
                        BattleController_LoadStatusScript(battleCtx, 0x47, true);
                        result = StatusCheckEndTurn;
                    }
                    battleCtx.StatusCheckState++;
                    break;
                case 7:
                    if (mon.MoveEffectsData.TauntedTurns != 0 && battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Power == 0)
                    {
                        failFlags.Taunted = 1;
                        BattleController_LoadStatusScript(battleCtx, 0x85, true);
                        result = StatusCheckEndTurn;
                    }
                    battleCtx.StatusCheckState++;
                    break;
                case 8:
                    if (BattleContexts.Move_Imprisoned(battleSys, battleCtx, attacker, (ushort)battleCtx.MoveTemp) != 0)
                    {
                        failFlags.Imprisoned = 1;
                        BattleController_LoadStatusScript(battleCtx, 0x90, true);
                        result = StatusCheckEndTurn;
                    }
                    battleCtx.StatusCheckState++;
                    break;
                case 9:
                    if (BattleContexts.Move_FailsInHighGravity(battleSys, battleCtx, attacker, (ushort)battleCtx.MoveCur) != 0)
                    {
                        failFlags.Gravity = 1;
                        BattleController_LoadStatusScript(battleCtx, 0x9D, true);
                        result = StatusCheckEndTurn;
                    }
                    battleCtx.StatusCheckState++;
                    break;
                case 10:
                    if (BattleContexts.Move_HealBlocked(battleSys, battleCtx, attacker, (ushort)battleCtx.MoveCur) != 0)
                    {
                        failFlags.HealBlocked = 1;
                        BattleController_LoadStatusScript(battleCtx, 0xAE, true);
                        result = StatusCheckEndTurn;
                    }
                    battleCtx.StatusCheckState++;
                    break;
                case 11:
                    battleCtx.StatusCheckState++;
                    if ((mon.StatusVolatile & VolatileConfusion) != 0)
                    {
                        mon.StatusVolatile--;
                        if ((battleCtx.BattleMons[battleCtx.Attacker].StatusVolatile & VolatileConfusion) != 0)
                        {
                            if ((BattleSystems.BattleSystem_RandNext(battleSys) & 1) != 0)
                            {
                                BattleController_LoadStatusScript(battleCtx, 0x26, false);
                                result = StatusCheckContinue;
                            }
                            else
                            {
                                battleCtx.MoveFailFlags[battleCtx.Attacker].Confused = 1;
                                battleCtx.Defender = battleCtx.Attacker;
                                battleCtx.MsgBattlerTemp = battleCtx.Attacker;
                                battleCtx.HpCalcTemp = BattleContexts.BattleSystem_CalcMoveDamage(battleSys, battleCtx, MoveStruggle, 0, 0, 40, 0, (byte)battleCtx.Attacker, (byte)battleCtx.Attacker, 1);
                                battleCtx.HpCalcTemp = BattleContexts.BattleSystem_CalcDamageVariance(battleSys, battleCtx, battleCtx.HpCalcTemp);
                                battleCtx.HpCalcTemp *= -1;
                                BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, 0x27);
                                battleCtx.Command = ControllerCommandRunScript;
                                battleCtx.CommandNext = ControllerCommandHitSelf;
                                result = StatusCheckEndTurn;
                            }
                        }
                        else
                        {
                            BattleController_LoadStatusScript(battleCtx, 0x28, false);
                            result = StatusCheckContinue;
                        }
                    }
                    break;
                case 12:
                    if ((mon.Status & StatusParalysis) != 0 && BattleContexts.Battler_Ability(battleCtx, attacker) != AbilityMagicGuard
                        && BattleSystems.BattleSystem_RandNext(battleSys) % 4 == 0)
                    {
                        battleCtx.MoveFailFlags[battleCtx.Attacker].Paralyzed = 1;
                        BattleController_LoadStatusScript(battleCtx, 0x20, true);
                        result = StatusCheckEndTurn;
                    }
                    battleCtx.StatusCheckState++;
                    break;
                case 13:
                    if ((mon.StatusVolatile & VolatileAttract) != 0)
                    {
                        battleCtx.MsgBattlerTemp = MathUtil.MathUtil_LowestSetBit((mon.StatusVolatile & VolatileAttract) >> 16);
                        if ((BattleSystems.BattleSystem_RandNext(battleSys) & 1) != 0)
                        {
                            BattleController_LoadStatusScript(battleCtx, 0x6B, false);
                            result = StatusCheckContinue;
                        }
                        else
                        {
                            battleCtx.MoveFailFlags[battleCtx.Attacker].Infatuated = 1;
                            BattleController_LoadStatusScript(battleCtx, 0x6C, true);
                            result = StatusCheckEndTurn;
                        }
                    }
                    battleCtx.StatusCheckState++;
                    break;
                case 14:
                    battleCtx.StatusCheckState++;
                    if ((mon.StatusVolatile & VolatileBideTurns) != 0)
                    {
                        mon.StatusVolatile -= 0x100;
                        if ((battleCtx.BattleMons[battleCtx.Attacker].StatusVolatile & VolatileBideTurns) == 0 && battleCtx.StoredDamage[battleCtx.Attacker] != 0)
                        {
                            battleCtx.Damage = battleCtx.StoredDamage[battleCtx.Attacker] * 2;
                            int target = battleCtx.LastHitByBattler[battleCtx.Attacker];
                            if (battleCtx.BattleMons[target].CurHP != 0)
                            {
                                battleCtx.Defender = target;
                            }
                            else
                            {
                                battleCtx.Defender = BattleContexts.BattleSystem_RandomOpponent(battleSys, battleCtx, battleCtx.Attacker);
                                if (battleCtx.BattleMons[battleCtx.Defender].CurHP == 0)
                                {
                                    BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, 0x11A);
                                    battleCtx.CommandNext = ControllerCommandMoveEnd;
                                    battleCtx.Command = ControllerCommandRunScript;
                                    result = StatusCheckContinue;
                                    break;
                                }
                            }
                        }
                        BattleController_LoadStatusScript(battleCtx, 0x24, false);
                        result = StatusCheckContinue;
                    }
                    break;
                case 15:
                    if ((mon.Status & StatusFreeze) != 0 && (effect == EffectThawFlameWheel || effect == EffectThawSacredFire))
                    {
                        BattleController_LoadStatusScript(battleCtx, 0x1E, false);
                        result = StatusCheckContinue;
                    }
                    battleCtx.StatusCheckState++;
                    break;
                default:
                    battleCtx.StatusCheckState = 0;
                    result = StatusCheckDone;
                    break;
            }
        } while (result == 0);
        BattleContexts.BattleMon_CopyToParty(battleSys, battleCtx, battleCtx.Attacker);
        if (result == StatusCheckEndTurn)
        {
            battleCtx.BattleStatusMask |= BattleStatusFailed;
            battleCtx.MoveStatusFlags |= MoveStatusNoMessage;
        }
        return result == StatusCheckDone ? 0 : 1;
    }

    // OFFSET 0x0224EE88
    private static int BattleController_CheckImmunityAbilities(BattleSystem battleSys, BattleContext battleCtx)
    {
        int result = 0;
        do
        {
            switch (battleCtx.AbilityCheckState)
            {
                case 0:
                {
                    int script = BattleContexts.BattleSystem_TriggerImmunityAbility(battleCtx, battleCtx.Attacker, battleCtx.Defender);
                    if ((script != 0 && (battleCtx.MoveStatusFlags & MoveStatusIneffectiveMask) == 0) || script == SubSeqImmunityWaterAbsorbFail)
                    {
                        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, script);
                        result = 1;
                        battleCtx.CommandNext = battleCtx.Command;
                        battleCtx.Command = ControllerCommandRunScript;
                        battleCtx.MoveStatusFlags |= MoveStatusNoMessage;
                    }
                    battleCtx.AbilityCheckState++;
                    break;
                }
                case 1:
                    battleCtx.AbilityCheckState = 0;
                    result = 2;
                    break;
            }
        } while (result == 0);
        return result == 2 ? 0 : 1;
    }
}
