namespace PlatinumSharp.Game;

public static partial class BattleControllers
{
    private const uint SideConditionReflect = 1;
    private const uint SideConditionLightScreen = 2;
    private const uint SideConditionSafeguard = 8;
    private const uint SideConditionMist = 0x40;
    private const uint SideConditionTailwind = 0x300;
    private const uint SideConditionTailwindTurn = 0x100;
    private const uint SideConditionLuckyChant = 0x7000;
    private const uint SideConditionLuckyChantTurn = 0x1000;
    private const uint FieldConditionGravityTurn = 0x1000;
    private const uint FieldConditionRainPermanentFlag = 2;
    private const uint FieldConditionSandPermanentFlag = 8;
    private const uint FieldConditionSunPermanentFlag = 0x20;
    private const uint FieldConditionHailPermanentFlag = 0x80;
    private const int MoveReflect = 0x73;
    private const int MoveLightScreen = 0x71;
    private const int MoveMist = 0x36;
    private const int SubseqSideConditionEnd = 0x32;
    private const int SubseqSafeguardEnd = 0x6E;
    private const int SubseqTailwindEnd = 0xE9;
    private const int SubseqLuckyChantEnd = 0xFA;
    private const int SubseqWishHeal = 0x88;
    private const int SubseqWeatherContinues = 0x68;
    private const int SubseqRainEnd = 0xEA;
    private const int SubseqSandstormEnd = 0xEB;
    private const int SubseqSunEnd = 0xEC;
    private const int SubseqHailEnd = 0xED;
    private const int SubseqGravityEnd = 0xEE;
    private const int MsgWishCameTrue = 0x215;
    private const int MsgRainContinues = 0x321;
    private const int MsgSandstormRages = 0x325;
    private const int MsgSunlightStrong = 0x328;
    private const int MsgHailContinues = 0x32B;
    private const int MsgFogDeep = 0x32D;
    private const int WeatherAnimRain = 0x13;
    private const int WeatherAnimSandstorm = 0x15;
    private const int WeatherAnimSun = 0x16;
    private const int WeatherAnimHail = 0x14;
    private const int WeatherAnimFog = 0x12;
    private const int ControllerCommandCheckMonConditions = 0xA;

    private enum FieldCheckResult
    {
        Continue,
        Found,
        Done,
    }

    private static FieldCheckResult BattleController_CheckSideTimer(BattleSystem battleSys, BattleContext battleCtx, uint flag, Func<SideConditions, int> getTurns, Action<SideConditions, int> setTurns, Action<SideConditions, int> onEnd, int script)
    {
        FieldCheckResult state = FieldCheckResult.Continue;
        while (battleCtx.FieldConditionCheckTemp < 2)
        {
            int side = battleCtx.FieldConditionCheckTemp;
            if ((battleCtx.SideConditionsMask[side] & flag) != 0)
            {
                SideConditions sideConditions = battleCtx.SideConditions[side];
                setTurns(sideConditions, (getTurns(sideConditions) - 1) & 7);
                if (getTurns(sideConditions) == 0)
                {
                    battleCtx.SideConditionsMask[side] &= ~flag;
                    onEnd(sideConditions, side);
                    BattleController_RunSubseq(battleCtx, script);
                    battleCtx.MsgBattlerTemp = BattleContexts.BattleSystem_SideToBattler(battleSys, battleCtx, side);
                    state = FieldCheckResult.Found;
                }
            }
            battleCtx.FieldConditionCheckTemp++;
            if (state != FieldCheckResult.Continue)
            {
                break;
            }
        }
        if (state == FieldCheckResult.Continue)
        {
            battleCtx.FieldConditionCheckState++;
            battleCtx.FieldConditionCheckTemp = 0;
        }
        return state;
    }

    private static FieldCheckResult BattleController_CheckSideCounter(BattleSystem battleSys, BattleContext battleCtx, uint mask, uint turn, int script)
    {
        FieldCheckResult state = FieldCheckResult.Continue;
        while (battleCtx.FieldConditionCheckTemp < 2)
        {
            int side = battleCtx.FieldConditionCheckTemp;
            if ((battleCtx.SideConditionsMask[side] & mask) != 0)
            {
                battleCtx.SideConditionsMask[side] -= turn;
                if ((battleCtx.SideConditionsMask[side] & mask) == 0)
                {
                    BattleController_RunSubseq(battleCtx, script);
                    battleCtx.MsgBattlerTemp = BattleContexts.BattleSystem_SideToBattler(battleSys, battleCtx, side);
                    state = FieldCheckResult.Found;
                }
            }
            battleCtx.FieldConditionCheckTemp++;
            if (state != FieldCheckResult.Continue)
            {
                break;
            }
        }
        if (state == FieldCheckResult.Continue)
        {
            battleCtx.FieldConditionCheckState++;
            battleCtx.FieldConditionCheckTemp = 0;
        }
        return state;
    }

    private static void BattleController_WeatherContinues(BattleContext battleCtx, int message)
    {
        battleCtx.MsgBuffer.Id = (ushort)message;
        battleCtx.MsgBuffer.Tags = 0;
        BattleController_RunSubseq(battleCtx, SubseqWeatherContinues);
    }

    private static FieldCheckResult BattleController_CheckWeather(BattleContext battleCtx, uint mask, uint permanent, int message, int endScript, int anim)
    {
        FieldCheckResult state = FieldCheckResult.Continue;
        if ((battleCtx.FieldConditionsMask & mask) != 0)
        {
            if ((battleCtx.FieldConditionsMask & permanent) != 0)
            {
                BattleController_WeatherContinues(battleCtx, message);
            }
            else
            {
                battleCtx.FieldConditions.WeatherTurns--;
                if (battleCtx.FieldConditions.WeatherTurns == 0)
                {
                    BattleController_RunSubseq(battleCtx, endScript);
                }
                else
                {
                    BattleController_WeatherContinues(battleCtx, message);
                }
            }
            battleCtx.ScriptTemp = anim;
            state = FieldCheckResult.Found;
        }
        battleCtx.FieldConditionCheckState++;
        return state;
    }

    // OFFSET 0x0224C794
    private static void BattleController_CheckFieldConditions(BattleSystem battleSys, BattleContext battleCtx)
    {
        FieldCheckResult state = FieldCheckResult.Continue;
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        do
        {
            if (BattleController_AnyFainted(battleCtx, battleCtx.Command, battleCtx.Command, 1) == 1
                || BattleController_AnyExpPayout(battleCtx, battleCtx.Command, battleCtx.Command) == 1
                || BattleController_CheckBattleOver(battleSys, battleCtx) == 1)
            {
                return;
            }
            switch (battleCtx.FieldConditionCheckState)
            {
                case 0:
                    state = BattleController_CheckSideTimer(battleSys, battleCtx, SideConditionReflect, s => s.ReflectTurns, (s, v) => s.ReflectTurns = v, (s, side) => battleCtx.MsgMoveTemp = MoveReflect, SubseqSideConditionEnd);
                    break;
                case 1:
                    state = BattleController_CheckSideTimer(battleSys, battleCtx, SideConditionLightScreen, s => s.LightScreenTurns, (s, v) => s.LightScreenTurns = v, (s, side) => battleCtx.MsgMoveTemp = MoveLightScreen, SubseqSideConditionEnd);
                    break;
                case 2:
                    state = BattleController_CheckSideTimer(battleSys, battleCtx, SideConditionMist, s => s.MistTurns, (s, v) => s.MistTurns = v, (s, side) => battleCtx.MsgMoveTemp = MoveMist, SubseqSideConditionEnd);
                    break;
                case 3:
                    state = BattleController_CheckSideTimer(battleSys, battleCtx, SideConditionSafeguard, s => s.SafeguardTurns, (s, v) => s.SafeguardTurns = v, (s, side) => battleCtx.MsgBattlerTemp = s.SafeguardUser, SubseqSafeguardEnd);
                    break;
                case 4:
                    state = BattleController_CheckSideCounter(battleSys, battleCtx, SideConditionTailwind, SideConditionTailwindTurn, SubseqTailwindEnd);
                    break;
                case 5:
                    state = BattleController_CheckSideCounter(battleSys, battleCtx, SideConditionLuckyChant, SideConditionLuckyChantTurn, SubseqLuckyChantEnd);
                    break;
                case 6:
                    while (battleCtx.FieldConditionCheckTemp < maxBattlers)
                    {
                        int battler = battleCtx.MonSpeedOrder[battleCtx.FieldConditionCheckTemp];
                        if (battleCtx.FieldConditions.WishTurns[battler] != 0)
                        {
                            battleCtx.FieldConditions.WishTurns[battler]--;
                            if (battleCtx.FieldConditions.WishTurns[battler] == 0 && battleCtx.BattleMons[battler].CurHP != 0)
                            {
                                battleCtx.MsgBattlerTemp = battler;
                                battleCtx.MsgBuffer.Tags = 2;
                                battleCtx.MsgBuffer.Id = MsgWishCameTrue;
                                battleCtx.MsgBuffer.Params[0] = battler | (battleCtx.FieldConditions.WishTarget[battler] << 8);
                                battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)battleCtx.BattleMons[battler].MaxHP, 2);
                                BattleController_RunSubseq(battleCtx, SubseqWishHeal);
                                state = FieldCheckResult.Found;
                            }
                        }
                        battleCtx.FieldConditionCheckTemp++;
                        if (state != FieldCheckResult.Continue)
                        {
                            break;
                        }
                    }
                    if (state == FieldCheckResult.Continue)
                    {
                        battleCtx.FieldConditionCheckState++;
                        battleCtx.FieldConditionCheckTemp = 0;
                    }
                    break;
                case 7:
                    state = BattleController_CheckWeather(battleCtx, FieldConditionRain, FieldConditionRainPermanentFlag, MsgRainContinues, SubseqRainEnd, WeatherAnimRain);
                    break;
                case 8:
                    state = BattleController_CheckWeather(battleCtx, FieldConditionSandstorm, FieldConditionSandPermanentFlag, MsgSandstormRages, SubseqSandstormEnd, WeatherAnimSandstorm);
                    break;
                case 9:
                    state = BattleController_CheckWeather(battleCtx, FieldConditionSun, FieldConditionSunPermanentFlag, MsgSunlightStrong, SubseqSunEnd, WeatherAnimSun);
                    break;
                case 10:
                    state = BattleController_CheckWeather(battleCtx, FieldConditionHail, FieldConditionHailPermanentFlag, MsgHailContinues, SubseqHailEnd, WeatherAnimHail);
                    break;
                case 11:
                    if ((battleCtx.FieldConditionsMask & FieldConditionFog) != 0)
                    {
                        BattleController_WeatherContinues(battleCtx, MsgFogDeep);
                        battleCtx.ScriptTemp = WeatherAnimFog;
                        state = FieldCheckResult.Found;
                    }
                    battleCtx.FieldConditionCheckState++;
                    break;
                case 12:
                    if ((battleCtx.FieldConditionsMask & FieldConditionGravity) != 0)
                    {
                        battleCtx.FieldConditionsMask -= FieldConditionGravityTurn;
                        if ((battleCtx.FieldConditionsMask & FieldConditionGravity) == 0)
                        {
                            BattleController_RunSubseq(battleCtx, SubseqGravityEnd);
                            state = FieldCheckResult.Found;
                        }
                    }
                    battleCtx.FieldConditionCheckState++;
                    break;
                case 13:
                    state = FieldCheckResult.Done;
                    break;
            }
        }
        while (state == FieldCheckResult.Continue);
        if (state == FieldCheckResult.Found)
        {
            BattleIO.BattleIO_ClearMessageBox(battleSys);
        }
        if (state == FieldCheckResult.Done)
        {
            battleCtx.FieldConditionCheckState = 0;
            battleCtx.Command = ControllerCommandCheckMonConditions;
        }
    }
}
