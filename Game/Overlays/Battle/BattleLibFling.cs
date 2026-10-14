namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const int FlingEffectRestoreStats = 24;
    private const int FlingEffectCureInfatuation = 25;
    private const int FlingEffectFlinch = 26;
    private const int FlingEffectParalyze = 27;
    private const int FlingEffectPoison = 28;
    private const int FlingEffectBadlyPoison = 29;
    private const int FlingEffectBurn = 30;
    private const int SideEffectFlinch = 0xE;

    // OFFSET 0x02259204
    public static int BattleSystem_FlingItem(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        int effect = Battler_ItemFlingEffect(battleCtx, battler);
        int power = Battler_HeldItemPower(battleCtx, battler, HeldItemPowerEmbargoOnly);
        battleCtx.MovePower = Battler_ItemFlingPower(battleCtx, battler);
        battleCtx.FlingScript = 0;
        battleCtx.SideEffectType = 0;
        if (battleCtx.MovePower == 0)
        {
            return 0;
        }
        BattleMon mon = battleCtx.BattleMons[battleCtx.Defender];
        switch (effect)
        {
            case PluckEffectRestoreHP:
                battleCtx.FlingTemp = power;
                battleCtx.FlingScript = SubscriptItemRestoreHP;
                break;
            case PluckEffectRestoreHPPercent:
                battleCtx.FlingTemp = BattleSystem_Divide((int)mon.MaxHP * power, 100);
                battleCtx.FlingScript = SubscriptItemRestoreHP;
                break;
            case PluckEffectCureParalysis:
                if ((mon.Status & MonConditionParalysis) != 0)
                {
                    battleCtx.FlingScript = SubscriptItemCureParalysis;
                }
                break;
            case PluckEffectCureSleep:
                if ((mon.Status & MonConditionSleep) != 0)
                {
                    battleCtx.FlingScript = SubscriptItemCureSleep;
                }
                break;
            case PluckEffectCurePoison:
                if ((mon.Status & MonConditionPoisonAny) != 0)
                {
                    battleCtx.FlingScript = SubscriptItemCurePoison;
                }
                break;
            case PluckEffectCureBurn:
                if ((mon.Status & MonConditionBurned) != 0)
                {
                    battleCtx.FlingScript = SubscriptItemCureBurn;
                }
                break;
            case PluckEffectCureFreeze:
                if ((mon.Status & MonConditionFrozen) != 0)
                {
                    battleCtx.FlingScript = SubscriptItemCureFreeze;
                }
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
                if (maxDiff != 0)
                {
                    BattleMon_AddVal(mon, BattleMonCurPP1 + bestSlot, power);
                    BattleMon_CopyToParty(battleSys, battleCtx, battleCtx.Defender);
                    battleCtx.MsgMoveTemp = mon.Moves[bestSlot];
                    battleCtx.FlingScript = SubscriptItemRestorePP;
                }
                break;
            }
            case PluckEffectCureConfusion:
                if ((mon.StatusVolatile & VolatileConfusion) != 0)
                {
                    battleCtx.FlingScript = SubscriptItemCureConfusion;
                }
                break;
            case PluckEffectCureAll:
                if ((mon.Status & 0xFF) != 0 || (mon.StatusVolatile & VolatileConfusion) != 0)
                {
                    if ((mon.Status & MonConditionParalysis) != 0)
                    {
                        battleCtx.FlingScript = SubscriptItemCureParalysis;
                    }
                    if ((mon.Status & MonConditionSleep) != 0)
                    {
                        battleCtx.FlingScript = SubscriptItemCureSleep;
                    }
                    if ((mon.Status & MonConditionPoisonAny) != 0)
                    {
                        battleCtx.FlingScript = SubscriptItemCurePoison;
                    }
                    if ((mon.Status & MonConditionBurned) != 0)
                    {
                        battleCtx.FlingScript = SubscriptItemCureBurn;
                    }
                    if ((mon.Status & MonConditionFrozen) != 0)
                    {
                        battleCtx.FlingScript = SubscriptItemCureFreeze;
                    }
                    if ((mon.StatusVolatile & VolatileConfusion) != 0)
                    {
                        battleCtx.FlingScript = SubscriptItemCureConfusion;
                    }
                    if ((mon.Status & 0xFF) != 0 && (mon.StatusVolatile & VolatileConfusion) != 0)
                    {
                        battleCtx.FlingScript = SubscriptItemCureMultiple;
                    }
                }
                break;
            case >= PluckEffectFlavorFirst and <= PluckEffectFlavorLast:
            {
                int flavor = effect - PluckEffectFlavorFirst;
                battleCtx.FlingTemp = BattleSystem_Divide((int)mon.MaxHP, power);
                battleCtx.MsgTemp = flavor;
                battleCtx.FlingScript = Pokemon.Pokemon_GetFlavorAffinityOf(mon.Personality, flavor) == -1 ? SubscriptItemDislikedFlavor : SubscriptItemRestoreHP;
                break;
            }
            case >= PluckEffectStatFirst and <= PluckEffectStatLast:
            {
                int stat = effect - PluckEffectStatFirst + 1;
                if (mon.StatBoosts[stat] < StatBoostMax)
                {
                    battleCtx.MsgTemp = stat;
                    battleCtx.FlingScript = SubscriptItemRaiseStat;
                }
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
                    battleCtx.FlingScript = SubscriptItemSharplyRaiseStat;
                }
                break;
            }
            case PluckEffectCritical:
                if ((mon.StatusVolatile & VolatileFocusEnergy) == 0)
                {
                    battleCtx.FlingScript = SubscriptItemRaiseCritical;
                }
                break;
            case PluckEffectAccuracy:
                battleCtx.FlingScript = SubscriptItemRaiseAccuracy;
                break;
            case FlingEffectRestoreStats:
                for (int stat = 0; stat < mon.StatBoosts.Length; stat++)
                {
                    if (mon.StatBoosts[stat] < StatBoostNeutralStage)
                    {
                        mon.StatBoosts[stat] = StatBoostNeutralStage;
                        battleCtx.FlingScript = SubscriptItemRestoreStats;
                    }
                }
                break;
            case FlingEffectCureInfatuation:
                if ((mon.StatusVolatile & VolatileAttract) != 0)
                {
                    battleCtx.MsgTemp = 6;
                    battleCtx.FlingScript = SubscriptItemCureInfatuation;
                }
                break;
            case FlingEffectFlinch:
                battleCtx.SideEffectMon = battler;
                battleCtx.SideEffectType = SideEffectTypeIndirect;
                battleCtx.FlingScript = SideEffectFlinch;
                break;
            case FlingEffectParalyze:
                battleCtx.SideEffectMon = battler;
                battleCtx.SideEffectType = SideEffectTypeIndirect;
                battleCtx.FlingScript = SideEffectParalyze;
                break;
            case FlingEffectPoison:
                battleCtx.SideEffectMon = battler;
                battleCtx.SideEffectType = SideEffectTypeIndirect;
                battleCtx.FlingScript = SideEffectPoison;
                break;
            case FlingEffectBadlyPoison:
                battleCtx.SideEffectMon = battler;
                battleCtx.SideEffectType = SideEffectTypeIndirect;
                battleCtx.FlingScript = SideEffectBadlyPoison;
                break;
            case FlingEffectBurn:
                battleCtx.SideEffectMon = battler;
                battleCtx.SideEffectType = SideEffectTypeIndirect;
                battleCtx.FlingScript = SideEffectBurn;
                break;
        }
        if ((mon.MoveEffectsMask & MoveEffectEmbargo) != 0)
        {
            battleCtx.FlingScript = 0;
        }
        else
        {
            battleCtx.MsgItemTemp = battleCtx.BattleMons[battler].HeldItem;
            if (battleCtx.SideEffectType == 0 && battleCtx.FlingScript != 0)
            {
                battleCtx.SelfTurnFlags[battleCtx.Attacker].StatusFlags |= (int)SelfTurnPluckedBerry;
            }
            battleCtx.MsgBattlerTemp = battleCtx.Defender;
        }
        return 1;
    }
}
