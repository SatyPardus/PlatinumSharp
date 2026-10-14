namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const int AbilitySpeedBoost = 3;
    private const int AbilityVoltAbsorb = 0xA;
    private const int AbilityWaterAbsorb = 0xB;
    private const int AbilityFlashFire = 0x12;
    private const int AbilitySoundproof = 0x2B;
    private const int AbilityShedSkin = 0x3D;
    private const int AbilityMotorDrive = 0x4E;
    private const int AbilityDrySkin = 0x57;
    private const int TypeFire = 0xA;
    private const int MoveWillOWisp = 0x105;
    private const uint MonConditionFrozen = 0x20;
    private const uint MonConditionPoisonAny = 0xF88;
    private const uint MonConditionBurned = 0x10;
    private const int SubscriptAbsorbAndHeal = 0xB2;
    private const int SubscriptFlashFire = 0xB3;
    private const int SubscriptSoundproof = 0xB5;
    private const int SubscriptMotorDrive = 0xB6;
    private const int SubscriptAbilityStatChange = 0xC;
    private const int SubscriptShedSkin = 0xBE;
    private const int SideEffectSpeedUpOneStage = 0x11;
    private const uint SoundMoveTable = 0x02270B8C;
    private const int SoundMoveCount = 12;

    // OFFSET 0x02256148
    public static int BattleSystem_TriggerImmunityAbility(BattleContext battleCtx, int attacker, int defender)
    {
        int script = 0;
        int type;
        if (Battler_Ability(battleCtx, attacker) == AbilityNormalize)
        {
            type = 0;
        }
        else if (battleCtx.MoveType != 0)
        {
            type = battleCtx.MoveType;
        }
        else
        {
            type = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Type;
        }
        if (Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityVoltAbsorb) == 1 && type == TypeElectric && attacker != defender)
        {
            battleCtx.HpCalcTemp = BattleSystem_Divide((int)battleCtx.BattleMons[defender].MaxHP, 4);
            script = SubscriptAbsorbAndHeal;
        }
        if (Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityWaterAbsorb) == 1 && type == TypeWater
            && (battleCtx.BattleStatusMask & BattleStatusNoRedirect) == 0 && battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Power != 0)
        {
            battleCtx.HpCalcTemp = BattleSystem_Divide((int)battleCtx.BattleMons[defender].MaxHP, 4);
            script = SubscriptAbsorbAndHeal;
        }
        if (Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityFlashFire) == 1 && type == TypeFire
            && (battleCtx.BattleMons[defender].Status & MonConditionFrozen) == 0
            && (battleCtx.BattleStatusMask & BattleStatusNoRedirect) == 0
            && (battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Power != 0 || battleCtx.MoveCur == MoveWillOWisp))
        {
            script = SubscriptFlashFire;
        }
        if (Battler_IgnorableAbility(battleCtx, attacker, defender, AbilitySoundproof) == 1)
        {
            if (MoveTable_Contains(SoundMoveTable, SoundMoveCount, (ushort)battleCtx.MoveCur) != 0)
            {
                script = SubscriptSoundproof;
            }
        }
        if (Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityMotorDrive) == 1 && type == TypeElectric && attacker != defender)
        {
            script = SubscriptMotorDrive;
        }
        if (Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityDrySkin) == 1 && type == TypeWater
            && (battleCtx.BattleStatusMask & BattleStatusNoRedirect) == 0 && battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Power != 0)
        {
            battleCtx.HpCalcTemp = BattleSystem_Divide((int)battleCtx.BattleMons[defender].MaxHP, 4);
            script = SubscriptAbsorbAndHeal;
        }
        return script;
    }

    // OFFSET 0x022562E8
    public static int BattleSystem_TriggerTurnEndAbility(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        int result = 0;
        int script = 0;
        BattleMon mon = battleCtx.BattleMons[battler];
        switch (Battler_Ability(battleCtx, battler))
        {
            case AbilitySpeedBoost:
                if (mon.CurHP != 0 && mon.StatBoosts[StatSpeed] < StatBoostMax && battleCtx.TotalTurns + 1 != mon.MoveEffectsData.FakeOutTurnNumber)
                {
                    battleCtx.SideEffectParam = SideEffectSpeedUpOneStage;
                    battleCtx.SideEffectType = SideEffectTypeAbility;
                    battleCtx.SideEffectMon = battler;
                    script = SubscriptAbilityStatChange;
                    result = 1;
                }
                break;
            case AbilityShedSkin:
                if ((mon.Status & 0xFF) != 0 && mon.CurHP != 0 && BattleSystems.BattleSystem_RandNext(battleSys) % 10 < 3)
                {
                    if ((mon.Status & MonConditionSleep) != 0)
                    {
                        battleCtx.MsgTemp = 0;
                    }
                    else if ((mon.Status & MonConditionPoisonAny) != 0)
                    {
                        battleCtx.MsgTemp = 1;
                    }
                    else if ((mon.Status & MonConditionBurned) != 0)
                    {
                        battleCtx.MsgTemp = 2;
                    }
                    else if ((mon.Status & MonConditionParalysis) != 0)
                    {
                        battleCtx.MsgTemp = 3;
                    }
                    else
                    {
                        battleCtx.MsgTemp = 4;
                    }
                    battleCtx.MsgBattlerTemp = battler;
                    script = SubscriptShedSkin;
                    result = 1;
                }
                break;
        }
        if (result == 1)
        {
            BattleSystem_LoadScript(battleCtx, 1, script);
            battleCtx.CommandNext = battleCtx.Command;
            battleCtx.Command = CommandRunScriptState;
        }
        return result;
    }

    // OFFSET 0x022563F8
    public static int BattleSystem_Divide(int dividend, int divisor)
    {
        if (dividend == 0)
        {
            return 0;
        }
        int sign = dividend < 0 ? -1 : 1;
        dividend /= divisor;
        if (dividend == 0)
        {
            dividend = sign;
        }
        return dividend;
    }
}
