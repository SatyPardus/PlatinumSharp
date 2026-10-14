namespace PlatinumSharp.Game;

public static partial class BattleControllers
{
    private const int SubSeqBeforeMove = 0x116;
    private const uint BattleTypeCatchTutorialAI = 0x400;
    private const int StatAccuracy = 6;
    private const int StatEvasion = 7;
    private const int StatStageNeutral = 6;
    private const int StatStageMax = 12;
    private const int AbilitySimple = 0x56;
    private const int AbilityUnaware = 0x6D;
    private const int AbilityCompoundEyes = 0xE;
    private const int AbilitySandVeil = 8;
    private const int AbilitySnowCloak = 0x51;
    private const int AbilityHustle = 0x37;
    private const int AbilityTangledFeet = 0x4D;
    private const int AbilityNoGuard = 0x63;
    private const int HoldEffectEvasionUp = 0x30;
    private const int HoldEffectAccuracyUp = 0x5D;
    private const int HoldEffectZoomLens = 0x68;
    private const uint VolatileForesight = 0x20000000;
    private const uint MoveEffectMiracleEye = 0x400000;
    private const uint FieldConditionRain = 3;
    private const uint FieldConditionSandstorm = 0xC;
    private const uint FieldConditionHail = 0xC0;
    private const uint FieldConditionFog = 0x8000;
    private const uint FieldConditionGravity = 0x7000;
    private const int EffectThunder = 0x98;
    private const int EffectBlizzard = 0x104;
    private const uint AccuracyTable = 0x0226EAD0;
    private const uint BattleStatusSkipAccuracy = 0x400;
    private const uint MoveStatusMissed = 1;
    private const uint MoveStatusProtected = 0x8000;
    private const uint MoveStatusSemiInvulnerable = 0x10000;
    private const uint MoveStatusIgnoreAccuracy = 0x400;
    private const byte MoveFlagProtect = 2;
    private const byte MoveFlagMagicCoat = 4;
    private const byte MoveFlagSnatch = 8;
    private const ushort MoveCurse = 0xAE;
    private const uint MoveEffectLockOn = 0x18;
    private const uint MoveEffectFlying = 0x40;
    private const uint MoveEffectShadowForce = 0x20000000;
    private const uint MoveEffectDigging = 0x80;
    private const uint MoveEffectDiving = 0x40000;
    private const uint BattleStatusHitFlying = 4;
    private const uint BattleStatusHitShadowForce = 0x80000;
    private const uint BattleStatusHitDigging = 8;
    private const uint BattleStatusHitDiving = 0x10;
    private const ushort RangeField = 0x80;
    private const uint MoveStatusFailureMask = 0x801FDA49;
    private const uint BattleStatusMagicCoat = 0x100000;
    private const int SubSeqMagicCoat = 0x8B;
    private const int SubSeqSnatch = 0x92;
    private const uint MultiHitSkipStatus = 4;
    private const uint MultiHitSkipObedience = 1;
    private const uint MultiHitSkipPP = 8;
    private const uint MultiHitSkipMagicCoat = 0x80;
    private const uint MultiHitSkipAccuracy = 0x20;
    private const uint MultiHitSkipProtect = 0x40;
    private const uint MultiHitSkipTypeImmunity = 2;
    private const uint MultiHitSkipImmunityAbility = 0x10;
    private const uint BattleStatus2MoveSelected = 0x40;
    private const int ScriptNarcMove = 0;
    private const int ControllerCommandTryMove = 0x17;
    private const int ControllerCommandDirectEffect = 0x18;
    private const int ControllerCommandCheckMoveResult = 0x19;
    private const int ControllerCommandMoveHit = 0x1A;
    private const int ControllerCommandAfterMoveHit = 0x1B;
    private const int ControllerCommandMultiHitMiss = 0x1C;
    private const int ControllerCommandNoMessage = 0x22;
    private const int SubSeqNoPP = 0x2A;
    private const int SubSeqMoveFailedMessage = 7;
    private const int SubSeqMoveHit = 1;
    private const uint MoveStatusMultiHitMissed = 0x4000;

    private static byte[]? accuracyTable;

    // OFFSET 0x0224EF00
    private static int BattleController_RunBeforeMoveScript(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqBeforeMove);
        battleCtx.CommandNext = battleCtx.Command;
        battleCtx.Command = ControllerCommandRunScript;
        return 1;
    }

    // OFFSET 0x0224EF20
    internal static int BattleController_CheckMoveHit(BattleSystem battleSys, BattleContext battleCtx, int attacker, int defender, int move)
    {
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeCatchTutorialAI) != 0)
        {
            return 0;
        }
        BattleContexts.Battler_Ability(battleCtx, attacker);
        int moveClass = battleCtx.AiContext.MoveTable[move].Class;
        sbyte accuracyStage = (sbyte)(battleCtx.BattleMons[attacker].StatBoosts[StatAccuracy] - StatStageNeutral);
        sbyte evasionStage = (sbyte)(StatStageNeutral - battleCtx.BattleMons[defender].StatBoosts[StatEvasion]);
        if (BattleContexts.Battler_Ability(battleCtx, attacker) == AbilitySimple)
        {
            accuracyStage = (sbyte)(accuracyStage * 2);
        }
        if (BattleContexts.Battler_IgnorableAbility(battleCtx, attacker, defender, AbilitySimple) == 1)
        {
            evasionStage = (sbyte)(evasionStage * 2);
        }
        if (BattleContexts.Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityUnaware) == 1)
        {
            accuracyStage = 0;
        }
        if (BattleContexts.Battler_Ability(battleCtx, attacker) == AbilityUnaware)
        {
            evasionStage = 0;
        }
        BattleMon defenderMon = battleCtx.BattleMons[defender];
        if (((defenderMon.StatusVolatile & VolatileForesight) != 0 || (defenderMon.MoveEffectsMask & MoveEffectMiracleEye) != 0) && evasionStage < 0)
        {
            evasionStage = 0;
        }
        sbyte stage = (sbyte)(evasionStage + StatStageNeutral + accuracyStage);
        if (stage < 0)
        {
            stage = 0;
        }
        if (stage > StatStageMax)
        {
            stage = StatStageMax;
        }
        ushort accuracy = battleCtx.AiContext.MoveTable[move].Accuracy;
        if (accuracy == 0)
        {
            return 0;
        }
        if ((battleCtx.BattleStatusMask & BattleStatusTypeChartCheck) != 0)
        {
            return 0;
        }
        if ((battleCtx.BattleStatusMask & BattleStatusSkipAccuracy) != 0)
        {
            return 0;
        }
        if (BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityField, 0, AbilityCloudNine) == 0
            && BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityField, 0, AbilityAirLock) == 0
            && (battleCtx.FieldConditionsMask & FieldConditionSun) != 0
            && battleCtx.AiContext.MoveTable[move].Effect == EffectThunder)
        {
            accuracy = 50;
        }
        accuracyTable ??= RomData.Overlay(16, AccuracyTable, (StatStageMax + 1) * 2);
        accuracy = (ushort)((uint)(accuracy * accuracyTable[stage * 2]) / accuracyTable[stage * 2 + 1]);
        if (BattleContexts.Battler_Ability(battleCtx, attacker) == AbilityCompoundEyes)
        {
            accuracy = (ushort)(accuracy * 130 / 100);
        }
        if (BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityField, 0, AbilityCloudNine) == 0
            && BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityField, 0, AbilityAirLock) == 0)
        {
            if ((battleCtx.FieldConditionsMask & FieldConditionSandstorm) != 0 && BattleContexts.Battler_IgnorableAbility(battleCtx, attacker, defender, AbilitySandVeil) == 1)
            {
                accuracy = (ushort)(accuracy * 80 / 100);
            }
            if ((battleCtx.FieldConditionsMask & FieldConditionHail) != 0 && BattleContexts.Battler_IgnorableAbility(battleCtx, attacker, defender, AbilitySnowCloak) == 1)
            {
                accuracy = (ushort)(accuracy * 80 / 100);
            }
            if ((battleCtx.FieldConditionsMask & FieldConditionFog) != 0)
            {
                accuracy = (ushort)(accuracy * 6 / 10);
            }
        }
        if (BattleContexts.Battler_Ability(battleCtx, attacker) == AbilityHustle && moveClass == 0)
        {
            accuracy = (ushort)(accuracy * 80 / 100);
        }
        if (BattleContexts.Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityTangledFeet) == 1 && (defenderMon.StatusVolatile & VolatileConfusion) != 0)
        {
            accuracy = (ushort)(accuracy * 50 / 100);
        }
        int itemEffect = BattleContexts.Battler_HeldItemEffect(battleCtx, defender);
        int itemPower = BattleContexts.Battler_HeldItemPower(battleCtx, defender, 0);
        if (itemEffect == HoldEffectEvasionUp)
        {
            accuracy = (ushort)(accuracy * (100 - itemPower) / 100);
        }
        itemEffect = BattleContexts.Battler_HeldItemEffect(battleCtx, attacker);
        itemPower = BattleContexts.Battler_HeldItemPower(battleCtx, attacker, 0);
        if (itemEffect == HoldEffectAccuracyUp)
        {
            accuracy = (ushort)(accuracy * (100 + itemPower) / 100);
        }
        if (itemEffect == HoldEffectZoomLens && BattleContexts.Battler_MovedThisTurn(battleCtx, defender) == 1)
        {
            accuracy = (ushort)(accuracy * (itemPower + 100) / 100);
        }
        MoveEffectsData effects = battleCtx.BattleMons[attacker].MoveEffectsData;
        if (effects.MicleBerry != 0)
        {
            effects.MicleBerry = 0;
            accuracy = (ushort)(accuracy * 120 / 100);
        }
        if ((battleCtx.FieldConditionsMask & FieldConditionGravity) != 0)
        {
            accuracy = (ushort)(accuracy * 10 / 6);
        }
        if ((int)(BattleSystems.BattleSystem_RandNext(battleSys) % 100) + 1 > accuracy)
        {
            battleCtx.MoveStatusFlags |= MoveStatusMissed;
        }
        return 0;
    }

    // OFFSET 0x0224F274
    internal static int BattleController_CheckProtectAndSemiInvulnerable(BattleSystem battleSys, BattleContext battleCtx, int attacker, int defender, int move)
    {
        if ((battleCtx.BattleStatusMask & BattleStatusTypeChartCheck) != 0)
        {
            return 0;
        }
        if (battleCtx.TurnFlags[defender].Protecting != 0 && (battleCtx.AiContext.MoveTable[move].Flags & MoveFlagProtect) != 0
            && (move != MoveCurse || BattleContexts.Move_IsGhostCurse(battleCtx, (ushort)move, attacker) == 1)
            && (BattleContexts.Move_IsMultiTurn(battleCtx, move) == 0 || (battleCtx.BattleStatusMask & BattleStatusMoveCalled) != 0))
        {
            BattleContexts.Battler_UnlockMoveChoice(battleSys, battleCtx, attacker);
            battleCtx.MoveStatusFlags |= MoveStatusProtected;
            return 0;
        }
        if ((battleCtx.BattleStatusMask & BattleStatusSkipAccuracy) == 0)
        {
            BattleMon defenderMon = battleCtx.BattleMons[defender];
            if (((defenderMon.MoveEffectsMask & MoveEffectLockOn) != 0 && defenderMon.MoveEffectsData.LockOnTarget == attacker)
                || BattleContexts.Battler_Ability(battleCtx, attacker) == AbilityNoGuard
                || BattleContexts.Battler_Ability(battleCtx, defender) == AbilityNoGuard)
            {
                battleCtx.MoveStatusFlags &= ~MoveStatusMissed;
                return 0;
            }
        }
        if (BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityField, 0, AbilityCloudNine) == 0
            && BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityField, 0, AbilityAirLock) == 0)
        {
            if ((battleCtx.FieldConditionsMask & FieldConditionRain) != 0 && battleCtx.AiContext.MoveTable[move].Effect == EffectThunder)
            {
                battleCtx.MoveStatusFlags &= ~MoveStatusMissed;
            }
            if ((battleCtx.FieldConditionsMask & FieldConditionHail) != 0 && battleCtx.AiContext.MoveTable[move].Effect == EffectBlizzard)
            {
                battleCtx.MoveStatusFlags &= ~MoveStatusMissed;
            }
        }
        if ((battleCtx.MoveStatusFlags & MoveStatusIgnoreAccuracy) == 0 && battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Range != RangeField)
        {
            uint mask = battleCtx.BattleStatusMask;
            uint effects = battleCtx.BattleMons[defender].MoveEffectsMask;
            if (((mask & BattleStatusHitFlying) == 0 && (effects & MoveEffectFlying) != 0)
                || ((mask & BattleStatusHitShadowForce) == 0 && (effects & MoveEffectShadowForce) != 0)
                || ((mask & BattleStatusHitDigging) == 0 && (effects & MoveEffectDigging) != 0)
                || ((mask & BattleStatusHitDiving) == 0 && (effects & MoveEffectDiving) != 0))
            {
                battleCtx.MoveStatusFlags |= MoveStatusSemiInvulnerable;
            }
        }
        return 0;
    }

    // OFFSET 0x0224F460
    private static int BattleController_CheckMagicCoatAndSnatch(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        if (battleCtx.Defender == 0xFF)
        {
            return 0;
        }
        uint failed = battleCtx.MoveStatusFlags & MoveStatusFailureMask;
        if (failed == 0)
        {
            TurnFlags defenderFlags = battleCtx.TurnFlags[battleCtx.Defender];
            if (defenderFlags.MagicCoat != 0 && (battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Flags & MoveFlagMagicCoat) != 0)
            {
                defenderFlags.MagicCoat = 0;
                battleCtx.MoveProtect[battleCtx.Attacker] = 0;
                battleCtx.MovePrevByBattler[battleCtx.Attacker] = (ushort)battleCtx.MoveTemp;
                battleCtx.MovePrev = battleCtx.MoveTemp;
                battleCtx.BattleStatusMask |= BattleStatusMagicCoat;
                BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqMagicCoat);
                battleCtx.CommandNext = battleCtx.Command;
                battleCtx.Command = ControllerCommandRunScript;
                BattleContexts.BattleSystem_DecPPForPressure(battleCtx, battleCtx.Defender, battleCtx.Attacker);
                return 1;
            }
        }
        for (int i = 0; i < maxBattlers; i++)
        {
            int battler = battleCtx.MonSpeedOrder[i];
            if (failed == 0 && battleCtx.TurnFlags[battler].Snatching != 0 && (battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Flags & MoveFlagSnatch) != 0)
            {
                battleCtx.MsgBattlerTemp = battler;
                battleCtx.TurnFlags[battler].Snatching = 0;
                if ((battleCtx.BattleStatusMask & BattleStatusMagicCoat) == 0)
                {
                    battleCtx.MoveProtect[battleCtx.Attacker] = 0;
                    battleCtx.MovePrevByBattler[battleCtx.Attacker] = (ushort)battleCtx.MoveTemp;
                    battleCtx.MovePrev = battleCtx.MoveTemp;
                    battleCtx.BattleStatusMask |= BattleStatusMagicCoat;
                }
                BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqSnatch);
                battleCtx.CommandNext = battleCtx.Command;
                battleCtx.Command = ControllerCommandRunScript;
                BattleContexts.BattleSystem_DecPPForPressure(battleCtx, battler, battleCtx.Attacker);
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0224F5E8
    private static void BattleController_BeforeMove(BattleSystem battleSys, BattleContext battleCtx)
    {
        switch (battleCtx.BeforeMoveCheckState)
        {
            case 0:
                BattleController_RunBeforeMoveScript(battleSys, battleCtx);
                battleCtx.BeforeMoveCheckState++;
                return;
            case 1:
                if ((battleCtx.MultiHitCheckFlags & MultiHitSkipStatus) == 0 && BattleController_CheckStatusConditions(battleSys, battleCtx) == 1)
                {
                    return;
                }
                battleCtx.BeforeMoveCheckState++;
                goto case 2;
            case 2:
                if ((battleCtx.MultiHitCheckFlags & MultiHitSkipObedience) == 0)
                {
                    int result = BattleController_CheckObedience(battleSys, battleCtx, out int script);
                    if (result != ObedienceResultNone)
                    {
                        switch (result)
                        {
                            case ObedienceResultEnd:
                                battleCtx.CommandNext = ControllerCommandMoveEnd;
                                break;
                            case ObedienceResultRepeat:
                                battleCtx.CommandNext = battleCtx.Command;
                                break;
                            case ObedienceResultHitSelf:
                                battleCtx.CommandNext = ControllerCommandHitSelf;
                                break;
                        }
                        battleCtx.Command = ControllerCommandRunScript;
                        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, script);
                        return;
                    }
                }
                battleCtx.BeforeMoveCheckState++;
                goto case 3;
            case 3:
                if ((battleCtx.MultiHitCheckFlags & MultiHitSkipPP) == 0 && BattleController_DecrementPP(battleSys, battleCtx) == 1)
                {
                    return;
                }
                battleCtx.BeforeMoveCheckState++;
                goto case 4;
            case 4:
                if (BattleController_CheckNoTarget(battleSys, battleCtx) == 1)
                {
                    return;
                }
                battleCtx.BeforeMoveCheckState++;
                goto case 5;
            case 5:
                if ((battleCtx.MultiHitCheckFlags & MultiHitSkipMagicCoat) == 0 && BattleController_CheckMagicCoatAndSnatch(battleSys, battleCtx) == 1)
                {
                    return;
                }
                battleCtx.BeforeMoveCheckState++;
                goto case 6;
            case 6:
                BattleContexts.BattleSystem_RedirectTarget(battleSys, battleCtx, battleCtx.Attacker, (ushort)battleCtx.MoveCur);
                battleCtx.BeforeMoveCheckState = 0;
                break;
        }
        if ((battleCtx.MoveStatusFlags & MoveStatusFailureMask) != 0)
        {
            battleCtx.Command = ControllerCommandCheckMoveResult;
        }
        else
        {
            battleCtx.BattleStatusMask2 |= BattleStatus2MoveSelected;
            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcMove, (int)battleCtx.MoveCur);
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = ControllerCommandTryMove;
            BattleContexts.BattleSystem_UpdateLastResort(battleSys, battleCtx);
        }
        BattleContexts.BattleSystem_UpdateMetronomeCount(battleSys, battleCtx);
    }

    // OFFSET 0x0224F734
    private static void BattleController_TryMove(BattleSystem battleSys, BattleContext battleCtx)
    {
        switch (battleCtx.TryMoveCheckState)
        {
            case 0:
                battleCtx.TryMoveCheckState++;
                if (BattleController_CheckNoTarget(battleSys, battleCtx) == 1)
                {
                    return;
                }
                goto case 1;
            case 1:
                battleCtx.TryMoveCheckState++;
                if (BattleContexts.BattleSystem_TriggerRedirectionAbilities(battleSys, battleCtx) == 1)
                {
                    return;
                }
                goto case 2;
            case 2:
                if ((battleCtx.MultiHitCheckFlags & MultiHitSkipAccuracy) == 0 && battleCtx.Defender != 0xFF
                    && BattleController_CheckMoveHit(battleSys, battleCtx, battleCtx.Attacker, battleCtx.Defender, (int)battleCtx.MoveCur) == 1)
                {
                    return;
                }
                battleCtx.TryMoveCheckState++;
                goto case 3;
            case 3:
                if ((battleCtx.MultiHitCheckFlags & MultiHitSkipProtect) == 0 && battleCtx.Defender != 0xFF
                    && BattleController_CheckProtectAndSemiInvulnerable(battleSys, battleCtx, battleCtx.Attacker, battleCtx.Defender, (int)battleCtx.MoveCur) == 1)
                {
                    return;
                }
                battleCtx.TryMoveCheckState++;
                goto case 4;
            case 4:
                if ((battleCtx.MultiHitCheckFlags & MultiHitSkipTypeImmunity) == 0 && battleCtx.Defender != 0xFF
                    && BattleController_CheckTypeImmunity(battleSys, battleCtx) == 1)
                {
                    return;
                }
                battleCtx.TryMoveCheckState++;
                goto case 5;
            case 5:
                if ((battleCtx.MultiHitCheckFlags & MultiHitSkipImmunityAbility) == 0 && battleCtx.Defender != 0xFF
                    && BattleController_CheckImmunityAbilities(battleSys, battleCtx) == 1)
                {
                    return;
                }
                battleCtx.TryMoveCheckState++;
                goto case 6;
            case 6:
                battleCtx.TryMoveCheckState = 0;
                break;
        }
        battleCtx.Command = ControllerCommandDirectEffect;
    }

    // OFFSET 0x0224F824
    private static void BattleController_CheckDirectSideEffect(BattleSystem battleSys, BattleContext battleCtx)
    {
        if (BattleContexts.BattleSystem_CheckDirectSideEffect(battleSys, battleCtx, out int script) == 1)
        {
            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, script);
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = ControllerCommandCheckMoveResult;
            return;
        }
        battleCtx.Command = ControllerCommandCheckMoveResult;
    }

    // OFFSET 0x0224F854
    private static void BattleController_CheckMoveResult(BattleSystem battleSys, BattleContext battleCtx)
    {
        uint flags = battleCtx.MoveStatusFlags;
        if ((flags & MoveStatusNoMessage) != 0)
        {
            battleCtx.Command = ControllerCommandNoMessage;
            return;
        }
        if ((flags & MoveStatusNoPP) != 0)
        {
            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqNoPP);
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = ControllerCommandMoveEnd;
            return;
        }
        if (battleCtx.MultiHitLoop != 0 && (flags & MoveStatusMissed) != 0)
        {
            battleCtx.MoveStatusFlags &= ~MoveStatusMissed;
            battleCtx.MoveStatusFlags |= MoveStatusMultiHitMissed;
            battleCtx.Command = ControllerCommandMultiHitMiss;
            return;
        }
        if ((flags & MoveStatusIneffectiveMask) != 0)
        {
            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqMoveFailedMessage);
            battleCtx.Command = ControllerCommandRunScript;
            battleCtx.CommandNext = ControllerCommandHitSelf;
            return;
        }
        battleCtx.Command = ControllerCommandMoveHit;
    }

    // OFFSET 0x0224F8D4
    private static void BattleController_MoveHit(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqMoveHit);
        battleCtx.Command = ControllerCommandRunScript;
        battleCtx.CommandNext = ControllerCommandAfterMoveHit;
    }
}
