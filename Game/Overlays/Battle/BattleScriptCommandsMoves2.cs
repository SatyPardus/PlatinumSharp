namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int DisableTurnsBase = 3;
    private const int EncoreTurnsBase = 3;

    // OFFSET 0x02243F54
    private static int BattleScript_TryDisable(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int moveSlot = BattleContexts.Battler_SlotForMove(battleCtx.BattleMons[battleCtx.Defender], battleCtx.MovePrevByBattler[battleCtx.Defender]);
        BattleMon defenderMon = battleCtx.BattleMons[battleCtx.Defender];
        if (defenderMon.MoveEffectsData.DisabledMove == 0 && moveSlot != 4 && defenderMon.PpCur[moveSlot] != 0 && battleCtx.MovePrevByBattler[battleCtx.Defender] != 0)
        {
            battleCtx.MsgMoveTemp = battleCtx.MovePrevByBattler[battleCtx.Defender];
            battleCtx.BattleMons[battleCtx.Defender].MoveEffectsData.DisabledMove = (ushort)battleCtx.MsgMoveTemp;
            int rand = BattleSystems.BattleSystem_RandNext(battleSys);
            battleCtx.BattleMons[battleCtx.Defender].MoveEffectsData.DisabledTurns = (rand % 4 + DisableTurnsBase) & 7;
        }
        else
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    private static void BattleScript_TryReflectDamage(BattleSystem battleSys, BattleContext battleCtx, int lastAttacker, int damageTaken)
    {
        int attackerSide = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
        int lastAttackerSide = BattleSystems.BattleSystem_GetBattlerSide(battleSys, lastAttacker);
        if (damageTaken != 0 && attackerSide != lastAttackerSide && battleCtx.BattleMons[lastAttacker].CurHP != 0)
        {
            battleCtx.Damage = damageTaken * 2;
            SideConditions conditions = battleCtx.SideConditions[lastAttackerSide];
            if (conditions.FollowMe != 0 && battleCtx.BattleMons[conditions.FollowMeUser].CurHP != 0)
            {
                battleCtx.Defender = conditions.FollowMeUser;
            }
            else
            {
                battleCtx.Defender = lastAttacker;
            }
            if (battleCtx.BattleMons[battleCtx.Defender].CurHP == 0)
            {
                battleCtx.Defender = BattleContexts.BattleSystem_RandomOpponent(battleSys, battleCtx, battleCtx.Attacker);
                if (battleCtx.BattleMons[battleCtx.Defender].CurHP == 0)
                {
                    battleCtx.CommandNext = 0x26;
                    BattleScript_Jump(battleCtx, NarcSubSeq, SubseqNoTarget);
                }
            }
            BattleContexts.BattleSystem_DecPPForPressure(battleCtx, battleCtx.Attacker, battleCtx.Defender);
        }
        else
        {
            battleCtx.SelfTurnFlags[battleCtx.Attacker].SkipPressureCheck = 1;
            battleCtx.MoveStatusFlags |= MoveStatusFailed;
        }
    }

    // OFFSET 0x02244010
    private static int BattleScript_Counter(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        TurnFlags flags = battleCtx.TurnFlags[battleCtx.Attacker];
        int lastAttacker = flags.PhysicalDamageLastAttacker;
        BattleScript_TryReflectDamage(battleSys, battleCtx, lastAttacker, flags.PhysicalDamageTakenFrom[lastAttacker]);
        return 0;
    }

    // OFFSET 0x0224410C
    private static int BattleScript_MirrorCoat(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        TurnFlags flags = battleCtx.TurnFlags[battleCtx.Attacker];
        int lastAttacker = flags.SpecialDamageLastAttacker;
        BattleScript_TryReflectDamage(battleSys, battleCtx, lastAttacker, flags.SpecialDamageTakenFrom[lastAttacker]);
        return 0;
    }

    // OFFSET 0x02244208
    private static int BattleScript_TryEncore(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int moveSlot = BattleContexts.Battler_SlotForMove(battleCtx.BattleMons[battleCtx.Defender], battleCtx.MovePrevByBattler[battleCtx.Defender]);
        if (BattleContexts.Move_CanBeEncored(battleCtx, battleCtx.MovePrevByBattler[battleCtx.Defender]) == 0)
        {
            moveSlot = 4;
        }
        BattleMon defenderMon = battleCtx.BattleMons[battleCtx.Defender];
        if (defenderMon.MoveEffectsData.EncoredMove == 0 && moveSlot != 4 && defenderMon.PpCur[moveSlot] != 0 && battleCtx.MovePrevByBattler[battleCtx.Defender] != 0)
        {
            battleCtx.MsgMoveTemp = battleCtx.MovePrevByBattler[battleCtx.Defender];
            battleCtx.BattleMons[battleCtx.Defender].MoveEffectsData.EncoredMove = (ushort)battleCtx.MsgMoveTemp;
            battleCtx.BattleMons[battleCtx.Defender].MoveEffectsData.EncoredMoveSlot = (ushort)moveSlot;
            int rand = BattleSystems.BattleSystem_RandNext(battleSys);
            battleCtx.BattleMons[battleCtx.Defender].MoveEffectsData.EncoredTurns = (rand % 5 + EncoreTurnsBase) & 7;
        }
        else
        {
            battleCtx.MoveStatusFlags |= MoveStatusFailed;
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }
}
