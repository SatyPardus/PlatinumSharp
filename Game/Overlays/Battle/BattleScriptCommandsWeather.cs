namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int CountAllBattlers = 8;
    private const int AbilityCloudNine = 0xD;
    private const int AbilityAirLock = 0x4C;
    private const int AbilitySandVeil = 8;
    private const int AbilityDrySkin = 0x57;
    private const int AbilitySolarPower = 0x5E;
    private const int AbilityIceBody = 0x73;
    private const int AbilitySnowCloak = 0x51;
    private const int AbilityRainDish = 0x2C;
    private const int AbilityHydration = 0x5D;
    private const uint FieldConditionRain = 3;
    private const uint FieldConditionSandstorm = 0xC;
    private const uint FieldConditionSun = 0x30;
    private const uint FieldConditionHail = 0xC0;
    private const uint MoveEffectHidden = 0x40080;
    private const int TypeGround = 4;
    private const int TypeRock = 5;
    private const int TypeSteel = 8;
    private const int TypeIce = 0xF;
    private const int MoveSandstorm = 0xC9;
    private const int MoveHail = 0x102;
    private const uint StatusSleep = 7;
    private const uint StatusPoisonAny = 0xF88;
    private const uint StatusBurn = 0x10;
    private const uint StatusParalysis = 0x40;
    private const uint VolatileMoveLocked = 0x1000;
    private const uint VolatileDefenseCurl = 0x40000000;
    private const int RolloutTurns = 5;

    // OFFSET 0x02245188
    private static int BattleScript_EndOfTurnWeatherEffect(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        battleCtx.ScriptTemp = 0;
        battleCtx.HpCalcTemp = 0;
        int type1 = BattleContexts.BattleMon_Get(battleCtx, battler, BattleMonType1, null);
        int type2 = BattleContexts.BattleMon_Get(battleCtx, battler, BattleMonType2, null);
        if (BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAllBattlers, 0, AbilityCloudNine) != 0 || BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAllBattlers, 0, AbilityAirLock) != 0)
        {
            return 0;
        }
        BattleMon mon = battleCtx.BattleMons[battler];
        if ((battleCtx.FieldConditionsMask & FieldConditionSandstorm) != 0
            && type1 != TypeRock && type2 != TypeRock
            && type1 != TypeSteel && type2 != TypeSteel
            && type1 != TypeGround && type2 != TypeGround
            && mon.CurHP != 0
            && BattleContexts.Battler_Ability(battleCtx, battler) != AbilitySandVeil
            && (mon.MoveEffectsMask & MoveEffectHidden) == 0)
        {
            battleCtx.MsgMoveTemp = MoveSandstorm;
            battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)mon.MaxHP * -1, 16);
        }
        if ((battleCtx.FieldConditionsMask & FieldConditionSun) != 0 && mon.CurHP != 0 && (mon.MoveEffectsMask & MoveEffectHidden) == 0)
        {
            if (BattleContexts.Battler_Ability(battleCtx, battler) == AbilityDrySkin || BattleContexts.Battler_Ability(battleCtx, battler) == AbilitySolarPower)
            {
                battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)mon.MaxHP * -1, 8);
            }
            if (BattleContexts.Battler_Ability(battleCtx, battler) == AbilitySolarPower)
            {
                battleCtx.ScriptTemp = 2;
            }
        }
        if ((battleCtx.FieldConditionsMask & FieldConditionHail) != 0 && mon.CurHP != 0 && (mon.MoveEffectsMask & MoveEffectHidden) == 0)
        {
            if (BattleContexts.Battler_Ability(battleCtx, battler) == AbilityIceBody)
            {
                if ((uint)mon.CurHP < mon.MaxHP)
                {
                    battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)mon.MaxHP, 16);
                }
            }
            else if (type1 != TypeIce && type2 != TypeIce && BattleContexts.Battler_Ability(battleCtx, battler) != AbilitySnowCloak)
            {
                battleCtx.MsgMoveTemp = MoveHail;
                battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)mon.MaxHP * -1, 16);
            }
        }
        if ((battleCtx.FieldConditionsMask & FieldConditionRain) != 0)
        {
            if (mon.CurHP != 0 && (uint)mon.CurHP < mon.MaxHP && BattleContexts.Battler_Ability(battleCtx, battler) == AbilityRainDish)
            {
                battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)mon.MaxHP, 16);
            }
            if (mon.CurHP != 0 && (uint)mon.CurHP < mon.MaxHP && BattleContexts.Battler_Ability(battleCtx, battler) == AbilityDrySkin)
            {
                battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)mon.MaxHP, 8);
            }
            if (mon.CurHP != 0 && (mon.Status & 0xFF) != 0 && BattleContexts.Battler_Ability(battleCtx, battler) == AbilityHydration)
            {
                if ((mon.Status & StatusSleep) != 0)
                {
                    battleCtx.MsgTemp = 0;
                }
                else if ((mon.Status & StatusPoisonAny) != 0)
                {
                    battleCtx.MsgTemp = 1;
                }
                else if ((mon.Status & StatusBurn) != 0)
                {
                    battleCtx.MsgTemp = 2;
                }
                else if ((mon.Status & StatusParalysis) != 0)
                {
                    battleCtx.MsgTemp = 3;
                }
                else
                {
                    battleCtx.MsgTemp = 4;
                }
                battleCtx.ScriptTemp = 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0224544C
    private static int BattleScript_CalcRolloutPower(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        battleCtx.SelfTurnFlags[battleCtx.Attacker].RepeatedMoveCount = battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.RolloutCount;
        if ((battleCtx.BattleMons[battleCtx.Attacker].StatusVolatile & VolatileMoveLocked) == 0)
        {
            BattleContexts.Battler_LockMoveChoice(battleSys, battleCtx, battleCtx.Attacker);
            battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.RolloutCount = RolloutTurns;
        }
        MoveEffectsData effects = battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData;
        effects.RolloutCount = (effects.RolloutCount - 1) & 7;
        if (battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.RolloutCount == 0)
        {
            BattleContexts.Battler_UnlockMoveChoice(battleSys, battleCtx, battleCtx.Attacker);
        }
        battleCtx.MovePower = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Power;
        int doublings = RolloutTurns - battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.RolloutCount;
        for (int i = 1; i < doublings; i++)
        {
            battleCtx.MovePower *= 2;
        }
        if ((battleCtx.BattleMons[battleCtx.Attacker].StatusVolatile & VolatileDefenseCurl) != 0)
        {
            battleCtx.MovePower *= 2;
        }
        return 0;
    }
}
