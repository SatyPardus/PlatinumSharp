namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int WringOutMax = 120;
    private const int PunishmentBase = 60;
    private const int PunishmentPerStage = 20;
    private const int PunishmentMax = 200;
    private const int StatStageNeutral = 6;
    private const int SideCheckHave = 0;
    private const int SideCheckNotHave = 1;
    private const int SideCheckClear = 2;
    private const uint SideConditionMistFlag = 0x40;

    private static ushort BattleScript_DefenderPlannedMove(BattleContext battleCtx)
    {
        BattleMon defender = battleCtx.BattleMons[battleCtx.Defender];
        ushort move = defender.MoveEffectsData.EncoredMove;
        if (move == 0 || move != defender.Moves[defender.MoveEffectsData.EncoredMoveSlot])
        {
            move = BattleContexts.Battler_SelectedMove(battleCtx, battleCtx.Defender);
        }
        return move;
    }

    // OFFSET 0x02246F70
    private static int BattleScript_CalcWringOutPower(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleMon defender = battleCtx.BattleMons[battleCtx.Defender];
        battleCtx.MovePower = (int)((uint)(WringOutMax * defender.CurHP) / defender.MaxHP) + 1;
        return 0;
    }

    // OFFSET 0x02246FA8
    private static int BattleScript_TryMeFirst(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        ushort move = BattleScript_DefenderPlannedMove(battleCtx);
        if (battleCtx.BattlerActions[battleCtx.Defender, 0] != ControllerCommandNone
            && battleCtx.TurnFlags[battleCtx.Defender].Struggling == 0
            && BattleContexts.Move_CanBeMeFirsted(battleCtx, move) == 1
            && battleCtx.AiContext.MoveTable[move].Power != 0)
        {
            battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.MeFirst = 1;
            battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.MeFirstTurnNumber = battleCtx.MeFirstTurnOrder;
            battleCtx.MsgMoveTemp = move;
        }
        else
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x02247064
    private static int BattleScript_TryCopycat(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        if (BattleContexts.Move_IsInvoker((ushort)battleCtx.MovePrev) == 0
            && battleCtx.MovePrev != 0
            && BattleContexts.Move_CanBeMetronomed(battleSys, battleCtx, battleCtx.Attacker, (ushort)battleCtx.MovePrev) == 1)
        {
            battleCtx.MsgMoveTemp = (int)battleCtx.MovePrev;
        }
        else
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x022470C0
    private static int BattleScript_CalcPunishmentPower(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int sum = 0;
        BattleMon defender = battleCtx.BattleMons[battleCtx.Defender];
        for (int stat = 0; stat < 8; stat++)
        {
            if (defender.StatBoosts[stat] > StatStageNeutral)
            {
                sum += defender.StatBoosts[stat] - StatStageNeutral;
            }
        }
        battleCtx.MovePower = PunishmentBase + PunishmentPerStage * sum;
        if (battleCtx.MovePower > PunishmentMax)
        {
            battleCtx.MovePower = PunishmentMax;
        }
        return 0;
    }

    // OFFSET 0x02247118
    private static int BattleScript_TrySuckerPunch(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        ushort move = BattleScript_DefenderPlannedMove(battleCtx);
        if (battleCtx.BattlerActions[battleCtx.Defender, 0] == ControllerCommandNone
            || (battleCtx.AiContext.MoveTable[move].Power == 0 && battleCtx.TurnFlags[battleCtx.Defender].Struggling == 0))
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x02247194
    private static int BattleScript_CheckSideCondition(BattleSystem battleSys, BattleContext battleCtx)
    {
        int value = 0;
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int op = BattleScript_Read(battleCtx);
        int condition = BattleScript_Read(battleCtx);
        int jump = BattleScript_Read(battleCtx);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, BattleScript_Battler(battleSys, battleCtx, selector));
        SideConditions conditions = battleCtx.SideConditions[side];
        if (op == SideCheckHave || op == SideCheckNotHave)
        {
            switch (condition)
            {
                case 0:
                    value = conditions.ReflectTurns;
                    break;
                case 1:
                    value = conditions.LightScreenTurns;
                    break;
                case 2:
                    value = conditions.MistTurns;
                    break;
                case 3:
                    value = conditions.SafeguardTurns;
                    break;
                case 4:
                    value = conditions.SpikesLayers;
                    break;
                case 5:
                    value = conditions.ToxicSpikesLayers;
                    break;
            }
        }
        else if (op == SideCheckClear)
        {
            switch (condition)
            {
                case 0:
                    conditions.ReflectTurns = 0;
                    battleCtx.SideConditionsMask[side] &= ~SideConditionReflect;
                    break;
                case 1:
                    conditions.LightScreenTurns = 0;
                    battleCtx.SideConditionsMask[side] &= ~SideConditionLightScreen;
                    break;
                case 2:
                    conditions.MistTurns = 0;
                    battleCtx.SideConditionsMask[side] &= ~SideConditionMistFlag;
                    break;
                case 3:
                    conditions.SafeguardTurns = 0;
                    battleCtx.SideConditionsMask[side] &= ~SideConditionSafeguard;
                    break;
                case 4:
                    conditions.SpikesLayers = 0;
                    battleCtx.SideConditionsMask[side] &= ~SideConditionSpikes;
                    break;
                case 5:
                    conditions.ToxicSpikesLayers = 0;
                    battleCtx.SideConditionsMask[side] &= ~SideConditionToxicSpikes;
                    break;
            }
        }
        if (op == SideCheckHave && value == 0)
        {
            BattleScript_Iter(battleCtx, jump);
        }
        if (op == SideCheckNotHave && value != 0)
        {
            BattleScript_Iter(battleCtx, jump);
        }
        return 0;
    }

    // OFFSET 0x02247378
    private static int BattleScript_TryFeint(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        if (battleCtx.TurnFlags[battleCtx.Defender].Protecting == 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x022473A8
    private static int BattleScript_CheckCanShareStatus(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        BattleMon defender = battleCtx.BattleMons[battleCtx.Defender];
        if (defender.Status != 0 || (defender.StatusVolatile & VolatileSubstitute) != 0 || battleCtx.BattleMons[battleCtx.Attacker].Status == 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }
}
