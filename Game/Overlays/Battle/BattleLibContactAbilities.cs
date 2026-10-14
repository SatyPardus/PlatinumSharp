namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const int AbilityDamp = 6;
    private const int AbilityLimber = 7;
    private const int AbilityStatic = 9;
    private const int AbilityOblivious = 0xC;
    private const int AbilityInsomnia = 0xF;
    private const int AbilityColorChange = 0x10;
    private const int AbilityImmunity = 0x11;
    private const int AbilityOwnTempo = 0x14;
    private const int AbilityRoughSkin = 0x18;
    private const int AbilityEffectSpore = 0x1B;
    private const int AbilityPoisonPoint = 0x26;
    private const int AbilityMagmaArmor = 0x28;
    private const int AbilityWaterVeil = 0x29;
    private const int AbilityFlameBody = 0x31;
    private const int AbilityCuteCharm = 0x38;
    private const int AbilityVitalSpirit = 0x48;
    private const int AbilityMagicGuard = 0x62;
    private const int AbilityAftermath = 0x6A;
    private const uint BattleStatus2UTurn = 0x10;
    private const uint VolatileAttract = 0xF0000;
    private const uint VolatileConfusion = 0x7;
    private const byte MoveFlagContact = 0x1;
    private const int SubscriptColorChange = 0xBC;
    private const int SubscriptRoughSkin = 0xBD;
    private const int SubscriptAftermath = 0xC1;
    private const int SubscriptAbilityCuresStatus = 0xDD;
    private const int SideEffectPoison = 0x16;
    private const int SideEffectParalyze = 0x1F;
    private const int SideEffectSleep = 0x12;
    private const int SideEffectBurn = 0x19;
    private const int SideEffectInfatuate = 0x6A;

    private static bool BattleContext_DefenderWasHitByContact(BattleContext battleCtx)
    {
        SelfTurnFlags flags = battleCtx.SelfTurnFlags[battleCtx.Defender];
        return (battleCtx.MoveStatusFlags & MoveStatusNoEffectMask) == 0
            && (battleCtx.BattleStatusMask & BattleStatusNoRedirect) == 0
            && (battleCtx.BattleStatusMask2 & BattleStatus2UTurn) == 0
            && (flags.PhysicalDamageTaken != 0 || flags.SpecialDamageTaken != 0)
            && (battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Flags & MoveFlagContact) != 0;
    }

    private static void BattleContext_SetContactSideEffect(BattleContext battleCtx)
    {
        battleCtx.SideEffectType = SideEffectTypeAbility;
        battleCtx.SideEffectMon = battleCtx.Attacker;
        battleCtx.MsgBattlerTemp = battleCtx.Defender;
    }

    // OFFSET 0x0225708C
    public static int BattleSystem_TriggerAbilityOnHit(BattleSystem battleSys, BattleContext battleCtx, ref int script)
    {
        int result = 0;
        if (battleCtx.Defender == BattlerNone)
        {
            return result;
        }
        if (Battler_SubstituteWasHit(battleCtx, battleCtx.Defender) == 1)
        {
            return result;
        }
        BattleMon attacker = battleCtx.BattleMons[battleCtx.Attacker];
        BattleMon defender = battleCtx.BattleMons[battleCtx.Defender];
        switch (Battler_Ability(battleCtx, battleCtx.Defender))
        {
            case AbilityStatic:
                if (attacker.CurHP != 0 && attacker.Status == 0 && BattleContext_DefenderWasHitByContact(battleCtx)
                    && BattleSystems.BattleSystem_RandNext(battleSys) % 10 < 3)
                {
                    BattleContext_SetContactSideEffect(battleCtx);
                    script = SideEffectParalyze;
                    result = 1;
                }
                break;
            case AbilityColorChange:
            {
                byte type;
                if (Battler_Ability(battleCtx, battleCtx.Attacker) == AbilityNormalize)
                {
                    type = 0;
                }
                else if (battleCtx.MoveType != 0)
                {
                    type = (byte)battleCtx.MoveType;
                }
                else
                {
                    type = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Type;
                }
                SelfTurnFlags flags = battleCtx.SelfTurnFlags[battleCtx.Defender];
                if (defender.CurHP != 0 && (battleCtx.MoveStatusFlags & MoveStatusNoEffectMask) == 0 && battleCtx.MoveCur != MoveStruggle
                    && (flags.PhysicalDamageTaken != 0 || flags.SpecialDamageTaken != 0)
                    && (battleCtx.BattleStatusMask2 & BattleStatus2UTurn) == 0
                    && battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Power != 0
                    && BattleMon_Get(battleCtx, battleCtx.Defender, BattleMonType1, null) != type
                    && BattleMon_Get(battleCtx, battleCtx.Defender, BattleMonType2, null) != type)
                {
                    script = SubscriptColorChange;
                    battleCtx.MsgTemp = type;
                    result = 1;
                }
                break;
            }
            case AbilityRoughSkin:
                if (attacker.CurHP != 0 && Battler_Ability(battleCtx, battleCtx.Attacker) != AbilityMagicGuard && BattleContext_DefenderWasHitByContact(battleCtx))
                {
                    battleCtx.HpCalcTemp = BattleSystem_Divide((int)attacker.MaxHP * -1, 8);
                    battleCtx.MsgBattlerTemp = battleCtx.Attacker;
                    script = SubscriptRoughSkin;
                    result = 1;
                }
                break;
            case AbilityEffectSpore:
                if (attacker.CurHP != 0 && attacker.Status == 0 && BattleContext_DefenderWasHitByContact(battleCtx)
                    && BattleSystems.BattleSystem_RandNext(battleSys) % 10 < 3)
                {
                    switch (BattleSystems.BattleSystem_RandNext(battleSys) % 3)
                    {
                        default:
                            script = SideEffectPoison;
                            break;
                        case 1:
                            script = SideEffectParalyze;
                            break;
                        case 2:
                            script = SideEffectSleep;
                            break;
                    }
                    BattleContext_SetContactSideEffect(battleCtx);
                    result = 1;
                }
                break;
            case AbilityPoisonPoint:
                if (attacker.CurHP != 0 && attacker.Status == 0 && BattleContext_DefenderWasHitByContact(battleCtx)
                    && BattleSystems.BattleSystem_RandNext(battleSys) % 10 < 3)
                {
                    BattleContext_SetContactSideEffect(battleCtx);
                    script = SideEffectPoison;
                    result = 1;
                }
                break;
            case AbilityFlameBody:
                if (attacker.CurHP != 0 && attacker.Status == 0 && BattleContext_DefenderWasHitByContact(battleCtx)
                    && BattleSystems.BattleSystem_RandNext(battleSys) % 10 < 3)
                {
                    BattleContext_SetContactSideEffect(battleCtx);
                    script = SideEffectBurn;
                    result = 1;
                }
                break;
            case AbilityCuteCharm:
                if (attacker.CurHP != 0 && (attacker.StatusVolatile & VolatileAttract) == 0 && BattleContext_DefenderWasHitByContact(battleCtx)
                    && defender.CurHP != 0 && BattleSystems.BattleSystem_RandNext(battleSys) % 10 < 3)
                {
                    BattleContext_SetContactSideEffect(battleCtx);
                    script = SideEffectInfatuate;
                    result = 1;
                }
                break;
            case AbilityAftermath:
                if (battleCtx.Defender == battleCtx.FaintedMon && Battler_Ability(battleCtx, battleCtx.Attacker) != AbilityMagicGuard
                    && BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityAllAlive, 0, AbilityDamp) == 0
                    && (battleCtx.BattleStatusMask2 & BattleStatus2UTurn) == 0
                    && attacker.CurHP != 0
                    && (battleCtx.MoveStatusFlags & MoveStatusNoEffectMask) == 0
                    && (battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Flags & MoveFlagContact) != 0)
                {
                    battleCtx.HpCalcTemp = BattleSystem_Divide((int)attacker.MaxHP * -1, 4);
                    battleCtx.MsgBattlerTemp = battleCtx.Attacker;
                    script = SubscriptAftermath;
                    result = 1;
                }
                break;
        }
        return result;
    }

    // OFFSET 0x02257628
    public static int BattleSystem_RecoverStatusByAbility(BattleSystem battleSys, BattleContext battleCtx, int battler, int skipLoad)
    {
        int result = 0;
        BattleMon mon = battleCtx.BattleMons[battler];
        switch (Battler_Ability(battleCtx, battler))
        {
            case AbilityImmunity:
                if ((mon.Status & MonConditionPoisonAny) != 0)
                {
                    battleCtx.MsgTemp = 1;
                    result = 1;
                }
                break;
            case AbilityOwnTempo:
                if ((mon.StatusVolatile & VolatileConfusion) != 0)
                {
                    battleCtx.MsgTemp = 5;
                    result = 1;
                }
                break;
            case AbilityLimber:
                if ((mon.Status & MonConditionParalysis) != 0)
                {
                    battleCtx.MsgTemp = 3;
                    result = 1;
                }
                break;
            case AbilityInsomnia:
            case AbilityVitalSpirit:
                if ((mon.Status & MonConditionSleep) != 0)
                {
                    battleCtx.MsgTemp = 0;
                    result = 1;
                }
                break;
            case AbilityWaterVeil:
                if ((mon.Status & MonConditionBurned) != 0)
                {
                    battleCtx.MsgTemp = 2;
                    result = 1;
                }
                break;
            case AbilityMagmaArmor:
                if ((mon.Status & MonConditionFrozen) != 0)
                {
                    battleCtx.MsgTemp = 4;
                    result = 1;
                }
                break;
            case AbilityOblivious:
                if ((mon.StatusVolatile & VolatileAttract) != 0)
                {
                    battleCtx.MsgTemp = 6;
                    result = 1;
                }
                break;
            case AbilityUnburden:
                if (mon.HeldItem != 0)
                {
                    mon.MoveEffectsData.CanUnburden = 1;
                }
                break;
        }
        if (result == 1)
        {
            battleCtx.MsgBattlerTemp = battler;
            battleCtx.MsgAbilityTemp = Battler_Ability(battleCtx, battler);
            if (skipLoad == 0)
            {
                BattleSystem_LoadScript(battleCtx, 1, SubscriptAbilityCuresStatus);
                battleCtx.CommandNext = battleCtx.Command;
                battleCtx.Command = CommandRunScriptState;
            }
        }
        return result;
    }

    // OFFSET 0x022577A4
    public static int Ability_ForbidsStatus(BattleContext battleCtx, int ability, uint status)
    {
        int result = 0;
        switch (ability)
        {
            case AbilityImmunity:
                if ((status & MonConditionPoisonAny) != 0)
                {
                    result = 1;
                }
                break;
            case AbilityLimber:
                if ((status & MonConditionParalysis) != 0)
                {
                    result = 1;
                }
                break;
            case AbilityInsomnia:
            case AbilityVitalSpirit:
                if ((status & MonConditionSleep) != 0)
                {
                    result = 1;
                }
                break;
            case AbilityWaterVeil:
                if ((status & MonConditionBurned) != 0)
                {
                    result = 1;
                }
                break;
            case AbilityMagmaArmor:
                if ((status & MonConditionFrozen) != 0)
                {
                    result = 1;
                }
                break;
        }
        return result;
    }
}
