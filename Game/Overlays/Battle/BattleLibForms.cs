namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const int HoldEffectMetronome = 0x69;
    private const int MetronomeMaxTurns = 10;
    private const int SpeciesCastform = 0x15F;
    private const int SpeciesCherrim = 0x1A5;
    private const int SpeciesGiratinaForm = 0x1E7;
    private const int AbilityForecast = 0x3B;
    private const uint FieldConditionWeatherMask = 0xF3;
    private const uint FieldConditionHail = 0xC0;
    private const int TypeNormal = 0;
    private const int TypeIce = 0xF;
    private const uint VolatileTransformed = 0x200000;
    private const uint BattleStatusDistortion = 0x80;
    private const uint BattleStatus2FormChanged = 0x4000000;
    private const int SubscriptFormChange = 0x106;
    private const int SubscriptGiratinaFormChange = 0x128;
    private const int ItemParamArceusPlate = 1;
    private const int MonDataHeldItemID = 6;
    private const int MonDataFormID = 0x70;

    private static bool Battler_MetronomeBlocked(BattleContext battleCtx, BattleMon mon)
    {
        return (mon.StatusVolatile & VolatileRampage) != 0 || (mon.StatusVolatile & VolatileUproar) != 0
            || (battleCtx.BattleStatusMask & BattleStatusChargeTurn) != 0 || (mon.StatusVolatile & VolatileMoveLocked) != 0;
    }

    // OFFSET 0x02259868
    public static void BattleSystem_UpdateMetronomeCount(BattleSystem battleSys, BattleContext battleCtx)
    {
        int attacker = battleCtx.Attacker;
        BattleMon mon = battleCtx.BattleMons[attacker];
        if (Battler_HeldItemEffect(battleCtx, attacker) == HoldEffectMetronome)
        {
            if (Battler_MetronomeBlocked(battleCtx, mon))
            {
                return;
            }
            if (battleCtx.MoveTemp == battleCtx.MetronomeMove[attacker])
            {
                if (mon.MoveEffectsData.MetronomeTurns < MetronomeMaxTurns)
                {
                    mon.MoveEffectsData.MetronomeTurns++;
                }
            }
            else
            {
                mon.MoveEffectsData.MetronomeTurns = 0;
                battleCtx.MetronomeMove[attacker] = (ushort)battleCtx.MoveTemp;
            }
        }
        else
        {
            mon.MoveEffectsData.MetronomeTurns = 0;
        }
    }

    // OFFSET 0x0225991C
    public static void BattleSystem_VerifyMetronomeCount(BattleSystem battleSys, BattleContext battleCtx)
    {
        int attacker = battleCtx.Attacker;
        BattleMon mon = battleCtx.BattleMons[attacker];
        if (Battler_HeldItemEffect(battleCtx, attacker) == HoldEffectMetronome)
        {
            if ((battleCtx.MoveStatusFlags & MoveStatusNoEffectMask) != 0
                && battleCtx.MoveTemp == battleCtx.MetronomeMove[attacker]
                && mon.MoveEffectsData.MetronomeTurns != 0
                && battleCtx.SelfTurnFlags[attacker].RepeatedMoveCount == 0
                && !Battler_MetronomeBlocked(battleCtx, mon))
            {
                mon.MoveEffectsData.MetronomeTurns--;
            }
        }
        else
        {
            mon.MoveEffectsData.MetronomeTurns = 0;
        }
    }

    private static bool BattleSystem_WeatherNegated(BattleSystem battleSys, BattleContext battleCtx)
    {
        return BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityAllAlive, 0, AbilityCloudNine) != 0
            || BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityAllAlive, 0, AbilityAirLock) != 0;
    }

    private static bool Castform_SetForm(BattleMon mon, byte type, int form)
    {
        if (mon.Type1 == type || mon.Type2 == type)
        {
            return false;
        }
        mon.Type1 = type;
        mon.Type2 = type;
        mon.FormNum = form;
        return true;
    }

    // OFFSET 0x02259B9C
    public static int BattleSystem_UpdateWeatherForms(BattleSystem battleSys, BattleContext battleCtx, ref int script)
    {
        int result = 0;
        for (int i = 0; i < BattleSystems.BattleSystem_GetMaxBattlers(battleSys); i++)
        {
            battleCtx.MsgBattlerTemp = battleCtx.MonSpeedOrder[i];
            int battler = battleCtx.MsgBattlerTemp;
            BattleMon mon = battleCtx.BattleMons[battler];
            if (mon.Species == SpeciesCastform && mon.CurHP != 0 && Battler_Ability(battleCtx, battler) == AbilityForecast)
            {
                bool changed = false;
                if (!BattleSystem_WeatherNegated(battleSys, battleCtx))
                {
                    uint field = battleCtx.FieldConditionsMask;
                    if ((field & FieldConditionWeatherMask) == 0 && Castform_SetForm(mon, TypeNormal, 0))
                    {
                        changed = true;
                    }
                    else if ((field & FieldConditionSun) != 0 && Castform_SetForm(mon, TypeFire, 1))
                    {
                        changed = true;
                    }
                    else if ((field & FieldConditionRain) != 0 && Castform_SetForm(mon, TypeWater, 2))
                    {
                        changed = true;
                    }
                    else if ((field & FieldConditionHail) != 0 && Castform_SetForm(mon, TypeIce, 3))
                    {
                        changed = true;
                    }
                }
                else if (Castform_SetForm(mon, TypeNormal, 0))
                {
                    changed = true;
                }
                if (changed)
                {
                    script = SubscriptFormChange;
                    result = 1;
                    break;
                }
            }
            if (mon.Species == SpeciesCherrim && mon.CurHP != 0)
            {
                bool changed = false;
                if (!BattleSystem_WeatherNegated(battleSys, battleCtx))
                {
                    uint field = battleCtx.FieldConditionsMask;
                    if ((field & FieldConditionWeatherMask) == 0 && mon.FormNum == 1)
                    {
                        mon.FormNum = 0;
                        changed = true;
                    }
                    else if ((field & FieldConditionSun) != 0 && mon.FormNum == 0)
                    {
                        mon.FormNum = 1;
                        changed = true;
                    }
                    else if ((field & FieldConditionRain) != 0 && mon.FormNum == 1)
                    {
                        mon.FormNum = 0;
                        changed = true;
                    }
                    else if ((field & FieldConditionHail) != 0 && mon.FormNum == 1)
                    {
                        mon.FormNum = 0;
                        changed = true;
                    }
                }
                else if (mon.FormNum == 1)
                {
                    mon.FormNum = 0;
                    changed = true;
                }
                if (changed)
                {
                    script = SubscriptFormChange;
                    result = 1;
                    break;
                }
            }
            if (mon.Species == SpeciesArceus && mon.CurHP != 0 && Battler_Ability(battleCtx, battler) == AbilityMultitype)
            {
                int form = (int)Pokemon.Pokemon_GetArceusTypeOf((ushort)Item.Item_LoadParam(mon.HeldItem, ItemParamArceusPlate, HeapIDBattleLib));
                if (mon.FormNum != form)
                {
                    mon.FormNum = form & 0x1F;
                    script = SubscriptFormChange;
                    result = 1;
                    break;
                }
            }
            if (mon.Species == SpeciesGiratinaForm && mon.CurHP != 0 && mon.FormNum == 1)
            {
                bool transformed = (mon.StatusVolatile & VolatileTransformed) != 0;
                if (transformed || ((BattleSystems.BattleSystem_GetBattleStatusMask(battleSys) & BattleStatusDistortion) == 0 && mon.HeldItem != ItemGriseousOrb))
                {
                    if (transformed)
                    {
                        Pokemon temp = Pokemon.Pokemon_New(HeapIDBattleLib);
                        int target;
                        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) != 0)
                        {
                            target = (int)battleCtx.BattlerActions[battler, 1];
                        }
                        else
                        {
                            target = battler ^ 1;
                        }
                        Pokemon source = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, target, battleCtx.SelectedPartySlot[target]);
                        Pokemon.Pokemon_Copy(source, temp);
                        Pokemon.Pokemon_SetValue(temp, MonDataHeldItemID, 0);
                        Pokemon.Pokemon_SetValue(temp, MonDataFormID, 0);
                        Pokemon.Pokemon_SetGiratinaFormByHeldItem(temp);
                        mon.Attack = (ushort)Pokemon.Pokemon_GetValue(temp, MonDataAttack, null);
                        mon.Defense = (ushort)Pokemon.Pokemon_GetValue(temp, MonDataDefense, null);
                        mon.Speed = (ushort)Pokemon.Pokemon_GetValue(temp, MonDataSpeed, null);
                        mon.SpAttack = (ushort)Pokemon.Pokemon_GetValue(temp, MonDataSpAttack, null);
                        mon.SpDefense = (ushort)Pokemon.Pokemon_GetValue(temp, MonDataSpDefense, null);
                        mon.Ability = (byte)Pokemon.Pokemon_GetValue(temp, MonDataAbility, null);
                        mon.FormNum = 0;
                        battleCtx.BattleStatusMask2 |= BattleStatus2FormChanged;
                        BattleIO.BattleIO_UpdatePartyMon(battleSys, battleCtx, battler);
                        script = SubscriptFormChange;
                    }
                    else
                    {
                        script = SubscriptGiratinaFormChange;
                    }
                    result = 1;
                    break;
                }
            }
        }
        return result;
    }
}
