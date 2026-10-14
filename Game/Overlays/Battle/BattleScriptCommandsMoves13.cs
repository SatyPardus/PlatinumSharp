namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int GyroBallMax = 150;
    private const uint TrumpCardPowerTableAddress = 0x0226E574;
    private const int TrumpCardMaxIndex = 4;

    // OFFSET 0x02246B68
    private static int BattleScript_ApplyTypeEffectiveness(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        battleCtx.Damage = BattleContexts.BattleSystem_ApplyTypeChart(battleSys, battleCtx, (int)battleCtx.MoveCur, battleCtx.MoveType, battleCtx.Attacker, battleCtx.Defender, battleCtx.Damage, ref battleCtx.MoveStatusFlags);
        return 0;
    }

    private static int BattleScript_GetTurnFlag(TurnFlags flags, int flag)
    {
        switch (flag)
        {
            case 0:
                return flags.Struggling & 1;
            case 1:
                return flags.PpDecremented & 1;
            case 2:
                return flags.Protecting & 1;
            case 3:
                return flags.HelpingHand & 1;
            case 4:
                return flags.MagicCoat & 1;
            case 5:
                return flags.Snatching & 1;
            case 6:
                return flags.Roosting & 1;
        }
        return -1;
    }

    // OFFSET 0x02246BB0
    private static int BattleScript_IfTurnFlag(BattleSystem battleSys, BattleContext battleCtx)
    {
        int result = 0;
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int flag = BattleScript_Read(battleCtx);
        int value = BattleScript_Read(battleCtx);
        int jump = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        if ((uint)flag <= 6 && value == BattleScript_GetTurnFlag(battleCtx.TurnFlags[battler], flag))
        {
            result = 1;
        }
        if (result != 0)
        {
            BattleScript_Iter(battleCtx, jump);
        }
        return 0;
    }

    // OFFSET 0x02246CB4
    private static int BattleScript_SetTurnFlag(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int flag = BattleScript_Read(battleCtx);
        int value = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        TurnFlags flags = battleCtx.TurnFlags[battler];
        switch (flag)
        {
            case 0:
                flags.Struggling = value & 1;
                break;
            case 1:
                flags.PpDecremented = value & 1;
                break;
            case 2:
                flags.Protecting = value & 1;
                break;
            case 3:
                flags.HelpingHand = value & 1;
                break;
            case 4:
                flags.MagicCoat = value & 1;
                break;
            case 5:
                flags.Snatching = value & 1;
                break;
            case 6:
                flags.Roosting = value & 1;
                break;
        }
        return 0;
    }

    // OFFSET 0x02246DAC
    private static int BattleScript_CalcGyroBallPower(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        battleCtx.MovePower = (int)(battleCtx.MonSpeedValues[battleCtx.Defender] * 25 / battleCtx.MonSpeedValues[battleCtx.Attacker]) + 1;
        if (battleCtx.MovePower > GyroBallMax)
        {
            battleCtx.MovePower = GyroBallMax;
        }
        return 0;
    }

    // OFFSET 0x02246DF0
    private static int BattleScript_TryMetalBurst(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int attackerSide = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
        int lastAttackerSide = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.TurnFlags[battleCtx.Attacker].LastAttacker);
        TurnFlags flags = battleCtx.TurnFlags[battleCtx.Attacker];
        if (flags.LastDamageTaken == 0 || attackerSide == lastAttackerSide || battleCtx.BattleMons[flags.LastAttacker].CurHP == 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        battleCtx.Damage = flags.LastDamageTaken * 15 / 10;
        SideConditions conditions = battleCtx.SideConditions[lastAttackerSide];
        if (conditions.FollowMe != 0 && battleCtx.BattleMons[conditions.FollowMeUser].CurHP != 0)
        {
            battleCtx.Defender = conditions.FollowMeUser;
        }
        else
        {
            battleCtx.Defender = battleCtx.TurnFlags[battleCtx.Attacker].LastAttacker;
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
        return 0;
    }

    // OFFSET 0x02246EE4
    private static int BattleScript_CalcPaybackPower(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        if (battleCtx.BattlerActions[battleCtx.Defender, 0] == ControllerCommandNone)
        {
            battleCtx.MovePower = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Power * 2;
        }
        else
        {
            battleCtx.MovePower = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Power;
        }
        return 0;
    }

    // OFFSET 0x02246F2C
    private static int BattleScript_CalcTrumpCardPower(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int pp = battleCtx.BattleMons[battleCtx.Attacker].PpCur[battleCtx.MoveSlot[battleCtx.Attacker]];
        if (pp > TrumpCardMaxIndex)
        {
            pp = TrumpCardMaxIndex;
        }
        battleCtx.MovePower = RomData.Overlay(BattleOverlay, (uint)(TrumpCardPowerTableAddress + pp), 1)[0];
        return 0;
    }
}
