namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const int AbilitySynchronize = 0x1C;
    private const uint BattleStatusSynchronize = 0x80;
    private const uint SelfTurnInfatuated = 0x4;
    private const int HoldEffectDestinyKnot = 0x6C;
    private const int SideEffectTypeHeldItem = 5;
    private const int HoldEffectRestoreHP = 1;
    private const int HoldEffectCureParalysis = 5;
    private const int HoldEffectCureSleep = 6;
    private const int HoldEffectCurePoison = 7;
    private const int HoldEffectCureBurn = 8;
    private const int HoldEffectCureFreeze = 9;
    private const int HoldEffectRestorePP = 10;
    private const int HoldEffectCureConfusion = 11;
    private const int HoldEffectCureAll = 12;
    private const int HoldEffectRestoreHPPercent = 13;
    private const int HoldEffectFlavorBerryFirst = 14;
    private const int HoldEffectFlavorBerryLast = 18;
    private const int HoldEffectPinchStatFirst = 36;
    private const int HoldEffectPinchStatLast = 40;
    private const int HoldEffectPinchCritical = 41;
    private const int HoldEffectPinchRandomStat = 42;
    private const int HoldEffectPinchAccuracy = 44;
    private const int HoldEffectRestoreStats = 49;
    private const int HoldEffectCureInfatuation = 54;
    private const int SubscriptItemRestoreHP = 0xC6;
    private const int SubscriptItemCureParalysis = 0xC7;
    private const int SubscriptItemCureSleep = 0xC8;
    private const int SubscriptItemCurePoison = 0xC9;
    private const int SubscriptItemCureBurn = 0xCA;
    private const int SubscriptItemCureFreeze = 0xCB;
    private const int SubscriptItemRestorePP = 0xCC;
    private const int SubscriptItemCureConfusion = 0xCD;
    private const int SubscriptItemCureMultiple = 0xCE;
    private const int SubscriptItemDislikedFlavor = 0xCF;
    private const int SubscriptItemRaiseStat = 0xD0;
    private const int SubscriptItemRaiseCritical = 0xD1;
    private const int SubscriptItemSharplyRaiseStat = 0xD2;
    private const int SubscriptItemRestoreStats = 0xD3;
    private const int SubscriptItemCureInfatuation = 0xD4;
    private const int SubscriptItemRaiseAccuracy = 0x109;
    private const int PinchStatCount = 5;

    // OFFSET 0x02257808
    public static int BattleSystem_SynchronizeStatus(BattleSystem battleSys, BattleContext battleCtx, int controllerCommand)
    {
        int result = 0;
        int script = 0;
        if (battleCtx.Defender != BattlerNone && Battler_Ability(battleCtx, battleCtx.Defender) == AbilitySynchronize
            && battleCtx.Defender == battleCtx.SideEffectMon && (battleCtx.BattleStatusMask & BattleStatusSynchronize) != 0)
        {
            battleCtx.MsgBattlerTemp = battleCtx.Defender;
            battleCtx.SideEffectMon = battleCtx.Attacker;
            result = 1;
        }
        else if (Battler_Ability(battleCtx, battleCtx.Attacker) == AbilitySynchronize
            && battleCtx.Attacker == battleCtx.SideEffectMon && (battleCtx.BattleStatusMask & BattleStatusSynchronize) != 0)
        {
            battleCtx.MsgBattlerTemp = battleCtx.Attacker;
            battleCtx.SideEffectMon = battleCtx.Defender;
            result = 1;
        }
        if (result == 1)
        {
            uint status = battleCtx.BattleMons[battleCtx.MsgBattlerTemp].Status;
            if ((status & MonConditionPoisonAny) != 0)
            {
                script = SideEffectPoison;
            }
            else if ((status & MonConditionBurned) != 0)
            {
                script = SideEffectBurn;
            }
            else if ((status & MonConditionParalysis) != 0)
            {
                script = SideEffectParalyze;
            }
            if (script != 0)
            {
                battleCtx.SideEffectType = SideEffectTypeAbility;
                BattleSystem_LoadScript(battleCtx, 1, script);
                battleCtx.CommandNext = controllerCommand;
                battleCtx.Command = CommandRunScriptState;
                return result;
            }
        }
        result = BattleSystem_UpdateWeatherForms(battleSys, battleCtx, ref script);
        if (result == 1)
        {
            BattleSystem_LoadScript(battleCtx, 1, script);
            battleCtx.CommandNext = controllerCommand;
            battleCtx.Command = CommandRunScriptState;
            return result;
        }
        if (battleCtx.Defender != BattlerNone && Battler_HeldItemEffect(battleCtx, battleCtx.Defender) == HoldEffectDestinyKnot
            && battleCtx.Defender == battleCtx.SideEffectMon && (battleCtx.SelfTurnFlags[battleCtx.Defender].StatusFlags & SelfTurnInfatuated) != 0)
        {
            battleCtx.MsgBattlerTemp = battleCtx.Defender;
            battleCtx.SideEffectMon = battleCtx.Attacker;
            result = 1;
        }
        else if (Battler_HeldItemEffect(battleCtx, battleCtx.Attacker) == HoldEffectDestinyKnot
            && battleCtx.Attacker == battleCtx.SideEffectMon && (battleCtx.SelfTurnFlags[battleCtx.Attacker].StatusFlags & SelfTurnInfatuated) != 0)
        {
            battleCtx.MsgBattlerTemp = battleCtx.Attacker;
            battleCtx.SideEffectMon = battleCtx.Defender;
            result = 1;
        }
        if (result == 1)
        {
            script = SideEffectInfatuate;
            battleCtx.SideEffectType = SideEffectTypeHeldItem;
            BattleSystem_LoadScript(battleCtx, 1, script);
            battleCtx.CommandNext = controllerCommand;
            battleCtx.Command = CommandRunScriptState;
            return result;
        }
        return 0;
    }

    private static int Battler_PinchItemPower(BattleContext battleCtx, int battler, int itemPower)
    {
        if (Battler_Ability(battleCtx, battler) == AbilityGluttony)
        {
            itemPower /= 2;
        }
        return itemPower;
    }

    private static bool Battler_InPinch(BattleMon mon, int itemPower)
    {
        return (uint)mon.CurHP <= mon.MaxHP / (uint)itemPower;
    }

    // OFFSET 0x022579A4
    public static int BattleSystem_TriggerHeldItem(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        int script = 0;
        int result = BattleSystem_CheckHeldItemTrigger(battleSys, battleCtx, battler, ref script);
        if (result == 1)
        {
            battleCtx.MsgBattlerTemp = battler;
            battleCtx.MsgItemTemp = Battler_HeldItem(battleCtx, battler);
            BattleSystem_LoadScript(battleCtx, 1, script);
            battleCtx.CommandNext = battleCtx.Command;
            battleCtx.Command = CommandRunScriptState;
        }
        return result;
    }

    // OFFSET 0x02258104
    public static int BattleSystem_TriggerHeldItemOnSwitchIn(BattleSystem battleSys, BattleContext battleCtx, int battler, ref int script)
    {
        int result = BattleSystem_CheckHeldItemTrigger(battleSys, battleCtx, battler, ref script);
        if (result == 1)
        {
            battleCtx.MsgItemTemp = Battler_HeldItem(battleCtx, battler);
        }
        return result;
    }

    private static int BattleSystem_CheckHeldItemTrigger(BattleSystem battleSys, BattleContext battleCtx, int battler, ref int script)
    {
        int result = 0;
        int itemEffect = Battler_HeldItemEffect(battleCtx, battler);
        int itemPower = Battler_HeldItemPower(battleCtx, battler, 0);
        BattleMon mon = battleCtx.BattleMons[battler];
        if (mon.CurHP == 0)
        {
            return result;
        }
        switch (itemEffect)
        {
            case HoldEffectRestoreHP:
                if ((uint)mon.CurHP <= mon.MaxHP / 2)
                {
                    battleCtx.HpCalcTemp = itemPower;
                    script = SubscriptItemRestoreHP;
                    result = 1;
                }
                break;
            case HoldEffectRestoreHPPercent:
                if ((uint)mon.CurHP <= mon.MaxHP / 2)
                {
                    battleCtx.HpCalcTemp = BattleSystem_Divide((int)mon.MaxHP * itemPower, 100);
                    script = SubscriptItemRestoreHP;
                    result = 1;
                }
                break;
            case HoldEffectCureParalysis:
                if ((mon.Status & MonConditionParalysis) != 0)
                {
                    script = SubscriptItemCureParalysis;
                    result = 1;
                }
                break;
            case HoldEffectCureSleep:
                if ((mon.Status & MonConditionSleep) != 0)
                {
                    script = SubscriptItemCureSleep;
                    result = 1;
                }
                break;
            case HoldEffectCurePoison:
                if ((mon.Status & MonConditionPoisonAny) != 0)
                {
                    script = SubscriptItemCurePoison;
                    result = 1;
                }
                break;
            case HoldEffectCureBurn:
                if ((mon.Status & MonConditionBurned) != 0)
                {
                    script = SubscriptItemCureBurn;
                    result = 1;
                }
                break;
            case HoldEffectCureFreeze:
                if ((mon.Status & MonConditionFrozen) != 0)
                {
                    script = SubscriptItemCureFreeze;
                    result = 1;
                }
                break;
            case HoldEffectRestorePP:
            {
                int slot;
                for (slot = 0; slot < MoveSlotCount; slot++)
                {
                    if (mon.Moves[slot] != 0 && mon.PpCur[slot] == 0)
                    {
                        break;
                    }
                }
                if (slot != MoveSlotCount)
                {
                    BattleMon_AddVal(mon, BattleMonCurPP1 + slot, itemPower);
                    BattleMon_CopyToParty(battleSys, battleCtx, battler);
                    battleCtx.MsgMoveTemp = mon.Moves[slot];
                    script = SubscriptItemRestorePP;
                    result = 1;
                }
                break;
            }
            case HoldEffectCureConfusion:
                if ((mon.StatusVolatile & VolatileConfusion) != 0)
                {
                    script = SubscriptItemCureConfusion;
                    result = 1;
                }
                break;
            case HoldEffectCureAll:
                if ((mon.Status & 0xFF) != 0 || (mon.StatusVolatile & VolatileConfusion) != 0)
                {
                    if ((mon.Status & MonConditionParalysis) != 0)
                    {
                        script = SubscriptItemCureParalysis;
                    }
                    if ((mon.Status & MonConditionSleep) != 0)
                    {
                        script = SubscriptItemCureSleep;
                    }
                    if ((mon.Status & MonConditionPoisonAny) != 0)
                    {
                        script = SubscriptItemCurePoison;
                    }
                    if ((mon.Status & MonConditionBurned) != 0)
                    {
                        script = SubscriptItemCureBurn;
                    }
                    if ((mon.Status & MonConditionFrozen) != 0)
                    {
                        script = SubscriptItemCureFreeze;
                    }
                    if ((mon.StatusVolatile & VolatileConfusion) != 0)
                    {
                        script = SubscriptItemCureConfusion;
                    }
                    if ((mon.Status & 0xFF) != 0 && (mon.StatusVolatile & VolatileConfusion) != 0)
                    {
                        script = SubscriptItemCureMultiple;
                    }
                    result = 1;
                }
                break;
            case >= HoldEffectFlavorBerryFirst and <= HoldEffectFlavorBerryLast:
                if ((uint)mon.CurHP <= mon.MaxHP / 2)
                {
                    int flavor = itemEffect - HoldEffectFlavorBerryFirst;
                    battleCtx.HpCalcTemp = BattleSystem_Divide((int)mon.MaxHP, itemPower);
                    battleCtx.MsgTemp = flavor;
                    script = Pokemon.Pokemon_GetFlavorAffinityOf(mon.Personality, flavor) == -1 ? SubscriptItemDislikedFlavor : SubscriptItemRestoreHP;
                    result = 1;
                }
                break;
            case >= HoldEffectPinchStatFirst and <= HoldEffectPinchStatLast:
            {
                int stat = itemEffect - HoldEffectPinchStatFirst + 1;
                itemPower = Battler_PinchItemPower(battleCtx, battler, itemPower);
                if (Battler_InPinch(mon, itemPower) && mon.StatBoosts[stat] < StatBoostMax)
                {
                    battleCtx.MsgTemp = stat;
                    script = SubscriptItemRaiseStat;
                    result = 1;
                }
                break;
            }
            case HoldEffectPinchCritical:
                itemPower = Battler_PinchItemPower(battleCtx, battler, itemPower);
                if (Battler_InPinch(mon, itemPower) && (mon.StatusVolatile & VolatileFocusEnergy) == 0)
                {
                    script = SubscriptItemRaiseCritical;
                    result = 1;
                }
                break;
            case HoldEffectPinchRandomStat:
                itemPower = Battler_PinchItemPower(battleCtx, battler, itemPower);
                if (Battler_InPinch(mon, itemPower))
                {
                    int stat;
                    for (stat = 0; stat < PinchStatCount; stat++)
                    {
                        if (mon.StatBoosts[1 + stat] < StatBoostMax)
                        {
                            break;
                        }
                    }
                    if (stat != PinchStatCount)
                    {
                        do
                        {
                            stat = BattleSystems.BattleSystem_RandNext(battleSys) % PinchStatCount;
                        } while (mon.StatBoosts[1 + stat] == StatBoostMax);
                        battleCtx.MsgTemp = stat + 1;
                        script = SubscriptItemSharplyRaiseStat;
                        result = 1;
                    }
                }
                break;
            case HoldEffectRestoreStats:
                for (int stat = 0; stat < mon.StatBoosts.Length; stat++)
                {
                    if (mon.StatBoosts[stat] < StatBoostNeutralStage)
                    {
                        mon.StatBoosts[stat] = StatBoostNeutralStage;
                        result = 1;
                    }
                }
                if (result == 1)
                {
                    script = SubscriptItemRestoreStats;
                }
                break;
            case HoldEffectCureInfatuation:
                if ((mon.StatusVolatile & VolatileAttract) != 0)
                {
                    battleCtx.MsgTemp = 6;
                    script = SubscriptItemCureInfatuation;
                    result = 1;
                }
                break;
            case HoldEffectPinchAccuracy:
                itemPower = Battler_PinchItemPower(battleCtx, battler, itemPower);
                if (Battler_InPinch(mon, itemPower))
                {
                    script = SubscriptItemRaiseAccuracy;
                    result = 1;
                }
                break;
        }
        return result;
    }
}
