namespace PlatinumSharp.Game;

public static partial class BattleControllers
{
    private const int HoldEffectShellBell = 0x58;
    private const int HoldEffectLifeOrb = 0x62;
    private const uint BattleStatus2NoRecoil = 0x10;
    private const int MoveClassStatus = 2;
    private const int SubseqShellBell = 0xD5;
    private const int SubseqLifeOrb = 0xD6;
    private const int LifeOrbDivisor = 10;

    // OFFSET 0x022513B0
    private static int BattleController_ToggleSemiInvulnMons(BattleSystem battleSys, BattleContext battleCtx)
    {
        int result = 0;
        while (battleCtx.VanishedCheckTemp < BattleSystems.BattleSystem_GetMaxBattlers(battleSys))
        {
            BattleMon mon = battleCtx.BattleMons[battleCtx.VanishedCheckTemp];
            if ((mon.MoveEffectsMask & MoveEffectUproarWake) == 0 && (mon.MoveEffectsTemp & MoveEffectUproarWake) != 0)
            {
                mon.MoveEffectsTemp &= ~MoveEffectUproarWake;
                BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqWakeUpFromUproar);
                battleCtx.MsgBattlerTemp = battleCtx.VanishedCheckTemp;
                result = 1;
                battleCtx.CommandNext = battleCtx.Command;
                battleCtx.Command = ControllerCommandRunScript;
            }
            battleCtx.VanishedCheckTemp++;
            if (result == 1)
            {
                break;
            }
        }
        if (result == 0)
        {
            battleCtx.VanishedCheckTemp = 0;
        }
        return result;
    }

    // OFFSET 0x0225143C
    private static int BattleController_TriggerAfterMoveHitEffects(BattleSystem battleSys, BattleContext battleCtx)
    {
        int result = 0;
        BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        int itemEffect = BattleContexts.Battler_HeldItemEffect(battleCtx, battleCtx.Attacker);
        int itemPower = BattleContexts.Battler_HeldItemPower(battleCtx, battleCtx.Attacker, 0);
        if (BattleController_AnyFainted(battleCtx, battleCtx.Command, battleCtx.Command, 1) == 1)
        {
            return 1;
        }
        do
        {
            switch (battleCtx.AfterMoveHitCheckState)
            {
                case 0:
                {
                    BattleMon attacker = battleCtx.BattleMons[battleCtx.Attacker];
                    if ((attacker.StatusVolatile & VolatileRage) != 0 && battleCtx.MoveCur != MoveRage)
                    {
                        attacker.StatusVolatile &= ~VolatileRage;
                    }
                    battleCtx.AfterMoveHitCheckState++;
                    break;
                }
                case 1:
                {
                    int defender = battleCtx.Defender;
                    if (defender != 0xFF
                        && itemEffect == HoldEffectShellBell
                        && (battleCtx.BattleStatusMask2 & BattleStatus2NoRecoil) == 0
                        && (battleCtx.BattleStatusMask & BattleStatusUpdateHP) != 0)
                    {
                        int dealt = battleCtx.SelfTurnFlags[battleCtx.Attacker].ShellBellDamageDealt;
                        BattleMon attacker = battleCtx.BattleMons[battleCtx.Attacker];
                        if (dealt != 0 && battleCtx.Attacker != defender && (uint)attacker.CurHP < attacker.MaxHP && attacker.CurHP != 0)
                        {
                            battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide(-dealt, itemPower);
                            battleCtx.MsgBattlerTemp = battleCtx.Attacker;
                            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqShellBell);
                            result = 1;
                            battleCtx.CommandNext = battleCtx.Command;
                            battleCtx.Command = ControllerCommandRunScript;
                        }
                    }
                    battleCtx.AfterMoveHitCheckState++;
                    break;
                }
                case 2:
                    if (itemEffect == HoldEffectLifeOrb
                        && BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) != AbilityMagicGuard
                        && (battleCtx.BattleStatusMask2 & BattleStatus2NoRecoil) == 0
                        && (battleCtx.BattleStatusMask & BattleStatusUpdateHP) != 0
                        && battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Class != MoveClassStatus
                        && battleCtx.BattleMons[battleCtx.Attacker].CurHP != 0)
                    {
                        battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)battleCtx.BattleMons[battleCtx.Attacker].MaxHP * -1, LifeOrbDivisor);
                        battleCtx.MsgBattlerTemp = battleCtx.Attacker;
                        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqLifeOrb);
                        result = 1;
                        battleCtx.CommandNext = battleCtx.Command;
                        battleCtx.Command = ControllerCommandRunScript;
                    }
                    battleCtx.AfterMoveHitCheckState++;
                    break;
                case 3:
                    battleCtx.AfterMoveHitCheckState = 0;
                    battleCtx.AfterMoveHitCheckTemp = 0;
                    result = 2;
                    break;
            }
        }
        while (result == 0);
        return result == 1 ? 1 : 0;
    }
}
