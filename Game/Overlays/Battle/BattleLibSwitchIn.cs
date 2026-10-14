namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const int SwitchInStateFieldWeather = 0;
    private const int SwitchInStateTrace = 1;
    private const int SwitchInStateWeatherAbilities = 2;
    private const int SwitchInStateIntimidate = 3;
    private const int SwitchInStateDownload = 4;
    private const int SwitchInStateAnticipation = 5;
    private const int SwitchInStateForewarn = 6;
    private const int SwitchInStateFrisk = 7;
    private const int SwitchInStateSlowStart = 8;
    private const int SwitchInStateMoldBreaker = 9;
    private const int SwitchInStatePressure = 10;
    private const int SwitchInStateFormChange = 11;
    private const int SwitchInStateAmuletCoin = 12;
    private const int SwitchInStateStatusAbilities = 13;
    private const int SwitchInStateHeldItems = 14;
    private const int SwitchInStateDone = 15;
    private const int AbilityDrizzle = 2;
    private const int AbilityIntimidate = 0x16;
    private const int AbilityTrace = 0x24;
    private const int AbilitySandStream = 0x2D;
    private const int AbilityPressure = 0x2E;
    private const int AbilityDrought = 0x46;
    private const int AbilityDownload = 0x58;
    private const int AbilityAnticipation = 0x6B;
    private const int AbilityForewarn = 0x6C;
    private const int AbilitySnowWarning = 0x75;
    private const int AbilityFrisk = 0x77;
    private const int ItemGriseousOrb = 0x70;
    private const int HoldEffectAmuletCoin = 0x3A;
    private const uint FieldConditionRainPermanent = 0x2;
    private const uint FieldConditionSandPermanent = 0x8;
    private const uint FieldConditionSunPermanent = 0x20;
    private const uint FieldConditionHailPermanent = 0x80;
    private const uint VolatileSubstitute = 0x1000000;
    private const int MoveEffectOneHitKO = 0x26;
    private const int MoveEffectCounter = 0x59;
    private const int MoveEffectMirrorCoat = 0x90;
    private const int MoveEffectMetalBurst = 0xE3;
    private const int ForewarnPowerOneHitKO = 0x96;
    private const int ForewarnPowerCounter = 0x78;
    private const int ForewarnPowerVariable = 0x50;
    private const int SubscriptOverworldRain = 0x10F;
    private const int SubscriptOverworldHail = 0x110;
    private const int SubscriptOverworldSand = 0x111;
    private const int SubscriptOverworldFog = 0x112;
    private const int SubscriptOverworldSun = 0x126;
    private const int SubscriptOverworldTrickRoom = 0x127;
    private const int SubscriptTrace = 0xBB;
    private const int SubscriptDrizzle = 0xB7;
    private const int SubscriptSandStream = 0xB8;
    private const int SubscriptDrought = 0xB9;
    private const int SubscriptSnowWarning = 0xFC;
    private const int SubscriptIntimidate = 0xBA;
    private const int SubscriptAnticipation = 0xC2;
    private const int SubscriptForewarn = 0xC3;
    private const int SubscriptFrisk = 0xFD;
    private const int SubscriptSlowStart = 0xC4;
    private const int SubscriptSlowStartEnd = 0xC5;
    private const int SubscriptMoldBreaker = 0xB1;
    private const int SubscriptPressure = 0x11D;
    private const int SideEffectAttackUpOneStage = 0xF;
    private const int SideEffectSpAttackUpOneStage = 0x12;
    private const int StatDefense = 2;
    private const int StatSpDefense = 5;
    private const int WeatherSunny = 0x3E9;
    private const int WeatherTrickRoom = 0x3EA;

    private static void Forewarn_Consider(BattleSystem battleSys, BattleContext battleCtx, ref int maxPower, int power, ushort move)
    {
        if (maxPower < power || (maxPower == power && (BattleSystems.BattleSystem_RandNext(battleSys) & 1) != 0))
        {
            maxPower = power;
            battleCtx.MsgMoveTemp = move;
        }
    }

    // OFFSET 0x02256414
    public static int BattleSystem_TriggerEffectOnSwitch(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        int triggered = 0;
        int script = 0;
        int i;
        do
        {
            switch (battleCtx.SwitchInCheckState)
            {
                case SwitchInStateFieldWeather:
                    if (battleCtx.FieldWeatherChecked == 0)
                    {
                        switch (BattleSystems.BattleSystem_GetFieldWeather(battleSys))
                        {
                            case 2:
                            case 3:
                            case 4:
                                script = SubscriptOverworldRain;
                                triggered = 1;
                                break;
                            case 5:
                            case 6:
                            case 7:
                                script = SubscriptOverworldHail;
                                triggered = 1;
                                break;
                            case 10:
                                script = SubscriptOverworldSand;
                                triggered = 1;
                                break;
                            case 14:
                            case 15:
                                script = SubscriptOverworldFog;
                                triggered = 1;
                                break;
                            case WeatherSunny:
                                script = SubscriptOverworldSun;
                                triggered = 1;
                                break;
                            case WeatherTrickRoom:
                                script = SubscriptOverworldTrickRoom;
                                triggered = 1;
                                break;
                        }
                        if (triggered == 1)
                        {
                            battleCtx.FieldWeatherChecked = 1;
                        }
                    }
                    battleCtx.SwitchInCheckState++;
                    break;
                case SwitchInStateTrace:
                    for (i = 0; i < maxBattlers; i++)
                    {
                        int battler = battleCtx.MonSpeedOrder[i];
                        BattleMon mon = battleCtx.BattleMons[battler];
                        int enemy1 = BattleSystems.BattleSystem_GetEnemyInSlot(battleSys, battler, 0);
                        int enemy2 = BattleSystems.BattleSystem_GetEnemyInSlot(battleSys, battler, 2);
                        battleCtx.MsgDefender = BattleSystem_ChooseTraceTarget(battleSys, battleCtx, enemy1, enemy2);
                        if (mon.TraceAnnounced == 0 && battleCtx.MsgDefender != BattlerNone && mon.CurHP != 0 && mon.HeldItem != ItemGriseousOrb
                            && battleCtx.BattleMons[battleCtx.MsgDefender].CurHP != 0 && Battler_Ability(battleCtx, battler) == AbilityTrace)
                        {
                            mon.TraceAnnounced = 1;
                            battleCtx.MsgBattlerTemp = battler;
                            script = SubscriptTrace;
                            triggered = 1;
                            break;
                        }
                    }
                    if (i == maxBattlers)
                    {
                        battleCtx.SwitchInCheckState++;
                    }
                    break;
                case SwitchInStateWeatherAbilities:
                    for (i = 0; i < maxBattlers; i++)
                    {
                        int battler = battleCtx.MonSpeedOrder[i];
                        BattleMon mon = battleCtx.BattleMons[battler];
                        if (mon.WeatherAbilityAnnounced == 0 && mon.CurHP != 0)
                        {
                            switch (Battler_Ability(battleCtx, battler))
                            {
                                case AbilityDrizzle:
                                    mon.WeatherAbilityAnnounced = 1;
                                    if ((battleCtx.FieldConditionsMask & FieldConditionRainPermanent) == 0)
                                    {
                                        script = SubscriptDrizzle;
                                        triggered = 1;
                                    }
                                    break;
                                case AbilitySandStream:
                                    mon.WeatherAbilityAnnounced = 1;
                                    if ((battleCtx.FieldConditionsMask & FieldConditionSandPermanent) == 0)
                                    {
                                        script = SubscriptSandStream;
                                        triggered = 1;
                                    }
                                    break;
                                case AbilityDrought:
                                    mon.WeatherAbilityAnnounced = 1;
                                    if ((battleCtx.FieldConditionsMask & FieldConditionSunPermanent) == 0)
                                    {
                                        script = SubscriptDrought;
                                        triggered = 1;
                                    }
                                    break;
                                case AbilitySnowWarning:
                                    mon.WeatherAbilityAnnounced = 1;
                                    if ((battleCtx.FieldConditionsMask & FieldConditionHailPermanent) == 0)
                                    {
                                        script = SubscriptSnowWarning;
                                        triggered = 1;
                                    }
                                    break;
                            }
                        }
                        if (triggered == 1)
                        {
                            battleCtx.MsgBattlerTemp = battler;
                            break;
                        }
                    }
                    if (i == maxBattlers)
                    {
                        battleCtx.SwitchInCheckState++;
                    }
                    break;
                case SwitchInStateIntimidate:
                    for (i = 0; i < maxBattlers; i++)
                    {
                        int battler = battleCtx.MonSpeedOrder[i];
                        BattleMon mon = battleCtx.BattleMons[battler];
                        if (mon.IntimidateAnnounced == 0 && mon.CurHP != 0 && Battler_Ability(battleCtx, battler) == AbilityIntimidate)
                        {
                            mon.IntimidateAnnounced = 1;
                            battleCtx.MsgBattlerTemp = battler;
                            script = SubscriptIntimidate;
                            triggered = 1;
                            break;
                        }
                    }
                    if (i == maxBattlers)
                    {
                        battleCtx.SwitchInCheckState++;
                    }
                    break;
                case SwitchInStateDownload:
                    for (i = 0; i < maxBattlers; i++)
                    {
                        int battler = battleCtx.MonSpeedOrder[i];
                        BattleMon mon = battleCtx.BattleMons[battler];
                        if (mon.DownloadAnnounced != 0 || mon.CurHP == 0 || Battler_Ability(battleCtx, battler) != AbilityDownload)
                        {
                            continue;
                        }
                        int sumDefense = 0;
                        int sumSpDefense = 0;
                        statStages ??= RomData.Overlay(16, StatStageTable, 13 * 2);
                        for (int j = 0; j < maxBattlers; j++)
                        {
                            BattleMon other = battleCtx.BattleMons[j];
                            if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) != BattleSystems.BattleSystem_GetBattlerSide(battleSys, j)
                                && (other.StatusVolatile & VolatileSubstitute) == 0 && other.CurHP != 0)
                            {
                                int stage = other.StatBoosts[StatDefense];
                                sumDefense += other.Defense * statStages[stage * 2] / statStages[stage * 2 + 1];
                                stage = other.StatBoosts[StatSpDefense];
                                sumSpDefense += other.SpDefense * statStages[stage * 2] / statStages[stage * 2 + 1];
                            }
                        }
                        mon.DownloadAnnounced = 1;
                        if (sumDefense + sumSpDefense != 0)
                        {
                            battleCtx.SideEffectParam = sumDefense >= sumSpDefense ? SideEffectSpAttackUpOneStage : SideEffectAttackUpOneStage;
                            battleCtx.SideEffectType = SideEffectTypeAbility;
                            battleCtx.SideEffectMon = battler;
                            script = SubscriptAbilityStatChange;
                            triggered = 1;
                            break;
                        }
                    }
                    if (i == maxBattlers)
                    {
                        battleCtx.SwitchInCheckState++;
                    }
                    break;
                case SwitchInStateAnticipation:
                    for (i = 0; i < maxBattlers; i++)
                    {
                        int battler = battleCtx.MonSpeedOrder[i];
                        BattleMon mon = battleCtx.BattleMons[battler];
                        if (mon.AnticipationAnnounced != 0 || mon.CurHP == 0 || Battler_Ability(battleCtx, battler) != AbilityAnticipation)
                        {
                            continue;
                        }
                        mon.AnticipationAnnounced = 1;
                        for (int j = 0; j < maxBattlers; j++)
                        {
                            BattleMon other = battleCtx.BattleMons[j];
                            if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) == BattleSystems.BattleSystem_GetBattlerSide(battleSys, j) || other.CurHP == 0)
                            {
                                continue;
                            }
                            for (int k = 0; k < MoveSlotCount; k++)
                            {
                                ushort move = other.Moves[k];
                                if (move == 0)
                                {
                                    continue;
                                }
                                uint effectiveness = 0;
                                battleCtx.Damage = BattleSystem_ApplyTypeChart(battleSys, battleCtx, move, 0, j, battler, battleCtx.Damage, ref effectiveness);
                                if ((effectiveness & MoveStatusIneffective) == 0 && Move_IsIgnoredByAnticipation(battleCtx, move) == 0
                                    && ((effectiveness & MoveStatusSuperEffective) != 0
                                        || (battleCtx.AiContext.MoveTable[move].Effect == MoveEffectOneHitKO && mon.Level <= other.Level)))
                                {
                                    triggered = 1;
                                    break;
                                }
                            }
                            if (triggered == 1)
                            {
                                break;
                            }
                        }
                        if (triggered == 1)
                        {
                            battleCtx.MsgBattlerTemp = battler;
                            script = SubscriptAnticipation;
                        }
                        break;
                    }
                    if (i == maxBattlers)
                    {
                        battleCtx.SwitchInCheckState++;
                    }
                    break;
                case SwitchInStateForewarn:
                    for (i = 0; i < maxBattlers; i++)
                    {
                        int battler = battleCtx.MonSpeedOrder[i];
                        BattleMon mon = battleCtx.BattleMons[battler];
                        if (mon.ForewarnAnnounced != 0 || mon.CurHP == 0 || Battler_Ability(battleCtx, battler) != AbilityForewarn)
                        {
                            continue;
                        }
                        mon.ForewarnAnnounced = 1;
                        int maxPower = 0;
                        int hpSum = 0;
                        for (int j = 0; j < maxBattlers; j++)
                        {
                            BattleMon other = battleCtx.BattleMons[j];
                            if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) == BattleSystems.BattleSystem_GetBattlerSide(battleSys, j) || other.CurHP == 0)
                            {
                                continue;
                            }
                            hpSum += other.CurHP;
                            for (int k = 0; k < MoveSlotCount; k++)
                            {
                                ushort move = other.Moves[k];
                                int power = battleCtx.AiContext.MoveTable[move].Power;
                                if (power == 1)
                                {
                                    switch (battleCtx.AiContext.MoveTable[move].Effect)
                                    {
                                        case MoveEffectOneHitKO:
                                            Forewarn_Consider(battleSys, battleCtx, ref maxPower, ForewarnPowerOneHitKO, move);
                                            break;
                                        case MoveEffectCounter:
                                        case MoveEffectMirrorCoat:
                                        case MoveEffectMetalBurst:
                                            Forewarn_Consider(battleSys, battleCtx, ref maxPower, ForewarnPowerCounter, move);
                                            break;
                                        default:
                                            Forewarn_Consider(battleSys, battleCtx, ref maxPower, ForewarnPowerVariable, move);
                                            break;
                                    }
                                }
                                else
                                {
                                    Forewarn_Consider(battleSys, battleCtx, ref maxPower, power, move);
                                }
                            }
                        }
                        if (maxPower != 0)
                        {
                            battleCtx.MsgBattlerTemp = battler;
                            script = SubscriptForewarn;
                            triggered = 1;
                        }
                        else if (hpSum != 0)
                        {
                            int defender = BattleSystem_RandomOpponent(battleSys, battleCtx, battler);
                            int moveCount = Battler_CountMoves(battleSys, battleCtx, defender);
                            battleCtx.MsgMoveTemp = battleCtx.BattleMons[defender].Moves[BattleSystems.BattleSystem_RandNext(battleSys) % moveCount];
                            battleCtx.MsgBattlerTemp = battler;
                            script = SubscriptForewarn;
                            triggered = 1;
                        }
                        break;
                    }
                    if (i == maxBattlers)
                    {
                        battleCtx.SwitchInCheckState++;
                    }
                    break;
                case SwitchInStateFrisk:
                    for (i = 0; i < maxBattlers; i++)
                    {
                        int battler = battleCtx.MonSpeedOrder[i];
                        BattleMon mon = battleCtx.BattleMons[battler];
                        if (mon.FriskAnnounced == 0 && mon.CurHP != 0 && Battler_Ability(battleCtx, battler) == AbilityFrisk)
                        {
                            mon.FriskAnnounced = 1;
                            if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) != 0)
                            {
                                int[] enemies =
                                {
                                    BattleSystems.BattleSystem_GetEnemyInSlot(battleSys, battler, 0),
                                    BattleSystems.BattleSystem_GetEnemyInSlot(battleSys, battler, 2),
                                };
                                BattleMon enemy1 = battleCtx.BattleMons[enemies[0]];
                                BattleMon enemy2 = battleCtx.BattleMons[enemies[1]];
                                if (enemy1.CurHP != 0 && enemy1.HeldItem != 0 && enemy2.CurHP != 0 && enemy2.HeldItem != 0)
                                {
                                    battleCtx.MsgItemTemp = battleCtx.BattleMons[enemies[BattleSystems.BattleSystem_RandNext(battleSys) & 1]].HeldItem;
                                    triggered = 1;
                                }
                                else if (enemy1.CurHP != 0 && enemy1.HeldItem != 0)
                                {
                                    battleCtx.MsgItemTemp = enemy1.HeldItem;
                                    triggered = 1;
                                }
                                else if (enemy2.CurHP != 0 && enemy2.HeldItem != 0)
                                {
                                    battleCtx.MsgItemTemp = enemy2.HeldItem;
                                    triggered = 1;
                                }
                            }
                            else
                            {
                                BattleMon enemy = battleCtx.BattleMons[battler ^ 1];
                                if (enemy.CurHP != 0 && enemy.HeldItem != 0)
                                {
                                    battleCtx.MsgItemTemp = enemy.HeldItem;
                                    triggered = 1;
                                }
                            }
                        }
                        if (triggered == 1)
                        {
                            battleCtx.MsgBattlerTemp = battler;
                            script = SubscriptFrisk;
                            break;
                        }
                    }
                    if (i == maxBattlers)
                    {
                        battleCtx.SwitchInCheckState++;
                    }
                    break;
                case SwitchInStateSlowStart:
                    for (i = 0; i < maxBattlers; i++)
                    {
                        int battler = battleCtx.MonSpeedOrder[i];
                        BattleMon mon = battleCtx.BattleMons[battler];
                        if (mon.SlowStartAnnounced == 0 && mon.CurHP != 0 && Battler_Ability(battleCtx, battler) == AbilitySlowStart
                            && battleCtx.TotalTurns <= mon.MoveEffectsData.SlowStartTurnNumber)
                        {
                            mon.SlowStartAnnounced = 1;
                            battleCtx.MsgBattlerTemp = battler;
                            script = SubscriptSlowStart;
                            triggered = 1;
                            break;
                        }
                        if (mon.SlowStartFinished == 0 && mon.CurHP != 0 && Battler_Ability(battleCtx, battler) == AbilitySlowStart
                            && battleCtx.TotalTurns - mon.MoveEffectsData.SlowStartTurnNumber == SlowStartTurns)
                        {
                            mon.SlowStartFinished = 1;
                            battleCtx.MsgBattlerTemp = battler;
                            script = SubscriptSlowStartEnd;
                            triggered = 1;
                            break;
                        }
                    }
                    if (i == maxBattlers)
                    {
                        battleCtx.SwitchInCheckState++;
                    }
                    break;
                case SwitchInStateMoldBreaker:
                    for (i = 0; i < maxBattlers; i++)
                    {
                        int battler = battleCtx.MonSpeedOrder[i];
                        BattleMon mon = battleCtx.BattleMons[battler];
                        if (mon.MoldBreakerAnnounced == 0 && mon.CurHP != 0 && Battler_Ability(battleCtx, battler) == AbilityMoldBreaker)
                        {
                            mon.MoldBreakerAnnounced = 1;
                            battleCtx.MsgBattlerTemp = battler;
                            script = SubscriptMoldBreaker;
                            triggered = 1;
                            break;
                        }
                    }
                    if (i == maxBattlers)
                    {
                        battleCtx.SwitchInCheckState++;
                    }
                    break;
                case SwitchInStatePressure:
                    for (i = 0; i < maxBattlers; i++)
                    {
                        int battler = battleCtx.MonSpeedOrder[i];
                        BattleMon mon = battleCtx.BattleMons[battler];
                        if (mon.PressureAnnounced == 0 && mon.CurHP != 0 && Battler_Ability(battleCtx, battler) == AbilityPressure)
                        {
                            mon.PressureAnnounced = 1;
                            battleCtx.MsgBattlerTemp = battler;
                            script = SubscriptPressure;
                            triggered = 1;
                            break;
                        }
                    }
                    if (i == maxBattlers)
                    {
                        battleCtx.SwitchInCheckState++;
                    }
                    break;
                case SwitchInStateFormChange:
                    if (BattleSystem_UpdateWeatherForms(battleSys, battleCtx, ref script) == 1)
                    {
                        triggered = 1;
                    }
                    else
                    {
                        battleCtx.SwitchInCheckState++;
                    }
                    break;
                case SwitchInStateAmuletCoin:
                    for (i = 0; i < maxBattlers; i++)
                    {
                        int battler = battleCtx.MonSpeedOrder[i];
                        if (BattleSystem_GetItemData(battleCtx, battleCtx.BattleMons[battler].HeldItem, ItemParamHoldEffect) == HoldEffectAmuletCoin)
                        {
                            battleCtx.PrizeMoneyMul = 2;
                        }
                    }
                    battleCtx.SwitchInCheckState++;
                    break;
                case SwitchInStateStatusAbilities:
                    for (i = 0; i < maxBattlers; i++)
                    {
                        int battler = battleCtx.MonSpeedOrder[i];
                        if (BattleSystem_RecoverStatusByAbility(battleSys, battleCtx, battler, 1) == 1)
                        {
                            script = SubscriptAbilityCuresStatus;
                            triggered = 1;
                            break;
                        }
                    }
                    if (i == maxBattlers)
                    {
                        battleCtx.SwitchInCheckState++;
                    }
                    break;
                case SwitchInStateHeldItems:
                    for (i = 0; i < maxBattlers; i++)
                    {
                        int battler = battleCtx.MonSpeedOrder[i];
                        if (BattleSystem_TriggerHeldItemOnSwitchIn(battleSys, battleCtx, battler, ref script) == 1)
                        {
                            battleCtx.MsgBattlerTemp = battler;
                            triggered = 1;
                            break;
                        }
                    }
                    if (i == maxBattlers)
                    {
                        battleCtx.SwitchInCheckState++;
                    }
                    break;
                case SwitchInStateDone:
                    battleCtx.SwitchInCheckState = 0;
                    triggered = 2;
                    break;
            }
        } while (triggered == 0);
        return script;
    }
}
