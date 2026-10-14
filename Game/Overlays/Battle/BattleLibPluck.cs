namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const int HoldEffectShedShellSwitch = 0x7B;
    private const int ItemParamFlingPower = 0xA;
    private const uint MoveEffectEmbargo = 0x4000000;
    private const uint SelfTurnPluckedBerry = 0x2;
    private const int PluckEffectCureParalysis = 1;
    private const int PluckEffectCureSleep = 2;
    private const int PluckEffectCurePoison = 3;
    private const int PluckEffectCureBurn = 4;
    private const int PluckEffectCureFreeze = 5;
    private const int PluckEffectRestorePP = 6;
    private const int PluckEffectRestoreHP = 7;
    private const int PluckEffectCureConfusion = 8;
    private const int PluckEffectCureAll = 9;
    private const int PluckEffectRestoreHPPercent = 10;
    private const int PluckEffectFlavorFirst = 11;
    private const int PluckEffectFlavorLast = 15;
    private const int PluckEffectStatFirst = 16;
    private const int PluckEffectStatLast = 20;
    private const int PluckEffectCritical = 21;
    private const int PluckEffectRandomStat = 22;
    private const int PluckEffectAccuracy = 23;

    // OFFSET 0x02258B80
    public static int Battler_ItemFlingPower(BattleContext battleCtx, int battler)
    {
        if (battleCtx.BattleMons[battler].MoveEffectsData.EmbargoTurns != 0)
        {
            return 0;
        }
        return BattleSystem_GetItemData(battleCtx, battleCtx.BattleMons[battler].HeldItem, ItemParamFlingPower);
    }

    // OFFSET 0x02258BA8
    public static int Battler_IsTrappedForSwitch(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        int result = 0;
        if (Battler_HeldItemEffect(battleCtx, battler) == HoldEffectShedShellSwitch)
        {
            return result;
        }
        BattleMon mon = battleCtx.BattleMons[battler];
        if ((mon.StatusVolatile & VolatileTrappedMask) != 0 || (mon.MoveEffectsMask & MoveEffectIngrain) != 0)
        {
            result = 1;
        }
        if (Battler_Ability(battleCtx, battler) != AbilityShadowTag
            && BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityOtherSideAlive, battler, AbilityShadowTag) != 0)
        {
            result = 1;
        }
        else if ((BattleMon_Get(battleCtx, battler, BattleMonType1, null) == TypeSteel || BattleMon_Get(battleCtx, battler, BattleMonType2, null) == TypeSteel)
            && BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityOtherSideAlive, battler, AbilityMagnetPull) != 0)
        {
            result = 1;
        }
        bool grounded = Battler_Ability(battleCtx, battler) != AbilityLevitate
            && mon.MoveEffectsData.MagnetRiseTurns == 0
            && BattleMon_Get(battleCtx, battler, BattleMonType1, null) != TypeFlying
            && BattleMon_Get(battleCtx, battler, BattleMonType2, null) != TypeFlying;
        if (grounded || Battler_HeldItemEffect(battleCtx, battler) == HoldEffectIronBall || (battleCtx.FieldConditionsMask & FieldConditionGravity) != 0)
        {
            if (BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityOtherSideAlive, battler, AbilityArenaTrap) != 0)
            {
                result = 1;
            }
        }
        return result;
    }

    // OFFSET 0x02258CB4
    public static int BattleSystem_PluckBerry(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        int result = 0;
        int script = 0;
        int effect = Battler_ItemPluckEffect(battleCtx, battler);
        int power = Battler_HeldItemPower(battleCtx, battler, HeldItemPowerNoEmbargoCheck);
        if (Battler_SubstituteWasHit(battleCtx, battleCtx.Defender) == 1)
        {
            return result;
        }
        BattleMon mon = battleCtx.BattleMons[battleCtx.Attacker];
        switch (effect)
        {
            case PluckEffectRestoreHP:
                if ((uint)mon.CurHP != mon.MaxHP)
                {
                    battleCtx.HpCalcTemp = power;
                    script = SubscriptItemRestoreHP;
                }
                result = 1;
                break;
            case PluckEffectRestoreHPPercent:
                if ((uint)mon.CurHP != mon.MaxHP)
                {
                    battleCtx.HpCalcTemp = BattleSystem_Divide((int)mon.MaxHP * power, 100);
                    script = SubscriptItemRestoreHP;
                }
                result = 1;
                break;
            case PluckEffectCureParalysis:
                if ((mon.Status & MonConditionParalysis) != 0)
                {
                    script = SubscriptItemCureParalysis;
                }
                result = 1;
                break;
            case PluckEffectCureSleep:
                if ((mon.Status & MonConditionSleep) != 0)
                {
                    script = SubscriptItemCureSleep;
                }
                result = 1;
                break;
            case PluckEffectCurePoison:
                if ((mon.Status & MonConditionPoisonAny) != 0)
                {
                    script = SubscriptItemCurePoison;
                }
                result = 1;
                break;
            case PluckEffectCureBurn:
                if ((mon.Status & MonConditionBurned) != 0)
                {
                    script = SubscriptItemCureBurn;
                }
                result = 1;
                break;
            case PluckEffectCureFreeze:
                if ((mon.Status & MonConditionFrozen) != 0)
                {
                    script = SubscriptItemCureFreeze;
                }
                result = 1;
                break;
            case PluckEffectRestorePP:
            {
                int maxDiff = 0;
                int bestSlot = 0;
                for (int slot = 0; slot < MoveSlotCount; slot++)
                {
                    if (mon.Moves[slot] != 0)
                    {
                        int diff = (int)MoveTable.MoveTable_CalcMaxPP(mon.Moves[slot], mon.PpUps[slot]) - mon.PpCur[slot];
                        if (diff > maxDiff)
                        {
                            maxDiff = diff;
                            bestSlot = slot;
                        }
                    }
                }
                BattleMon_AddVal(mon, BattleMonCurPP1 + bestSlot, power);
                BattleMon_CopyToParty(battleSys, battleCtx, battleCtx.Attacker);
                battleCtx.MsgMoveTemp = mon.Moves[bestSlot];
                script = SubscriptItemRestorePP;
                result = 1;
                break;
            }
            case PluckEffectCureConfusion:
                if ((mon.StatusVolatile & VolatileConfusion) != 0)
                {
                    script = SubscriptItemCureConfusion;
                }
                result = 1;
                break;
            case PluckEffectCureAll:
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
                }
                result = 1;
                break;
            case >= PluckEffectFlavorFirst and <= PluckEffectFlavorLast:
                if ((uint)mon.CurHP != mon.MaxHP)
                {
                    int flavor = effect - PluckEffectFlavorFirst;
                    battleCtx.HpCalcTemp = BattleSystem_Divide((int)mon.MaxHP, power);
                    battleCtx.MsgTemp = flavor;
                    script = Pokemon.Pokemon_GetFlavorAffinityOf(mon.Personality, flavor) == -1 ? SubscriptItemDislikedFlavor : SubscriptItemRestoreHP;
                }
                result = 1;
                break;
            case >= PluckEffectStatFirst and <= PluckEffectStatLast:
            {
                int stat = effect - PluckEffectStatFirst + 1;
                if (mon.StatBoosts[stat] < StatBoostMax)
                {
                    battleCtx.MsgTemp = stat;
                    script = SubscriptItemRaiseStat;
                }
                result = 1;
                break;
            }
            case PluckEffectRandomStat:
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
                }
                result = 1;
                break;
            }
            case PluckEffectCritical:
                if ((mon.StatusVolatile & VolatileFocusEnergy) == 0)
                {
                    script = SubscriptItemRaiseCritical;
                }
                result = 1;
                break;
            case PluckEffectAccuracy:
                script = SubscriptItemRaiseAccuracy;
                result = 1;
                break;
            default:
                if (Item.Item_IsBerry(battleCtx.BattleMons[battler].HeldItem) == 1)
                {
                    result = 1;
                }
                break;
        }
        if (result == 1)
        {
            if (Battler_Ability(battleCtx, battleCtx.Attacker) == AbilityKlutz || (mon.MoveEffectsMask & MoveEffectEmbargo) != 0)
            {
                battleCtx.ScriptTemp = 0;
            }
            else
            {
                battleCtx.ScriptTemp = script;
            }
            battleCtx.MsgItemTemp = battleCtx.BattleMons[battler].HeldItem;
            battleCtx.SelfTurnFlags[battleCtx.Attacker].StatusFlags |= (int)SelfTurnPluckedBerry;
        }
        return result;
    }
}
