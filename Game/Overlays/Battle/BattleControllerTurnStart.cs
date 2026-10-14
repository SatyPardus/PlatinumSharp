namespace PlatinumSharp.Game;

public static partial class BattleControllers
{
    private const uint BattleTypeNoActionOrder = 0x220;
    private const uint BattleTypeLinkActionOrder = 4;
    private const int ActionCommand = 3;
    private const int ActionSelectedFight = 1;
    private const int ActionSelectedItem = 2;
    private const int ActionSelectedSwitch = 3;
    private const int ActionSelectedRun = 4;
    private const int ActionOrderRunFirst = 5;
    private const int ControllerCommandTurnStart = 7;
    private const int ControllerCommandSortSpeed = 8;
    private const int ControllerCommandFirstAction = 9;
    private const int ControllerCommandRunScript = 0x15;
    private const uint StatusSleep = 7;
    private const ushort MoveFocusPunch = 0x108;
    private const ushort MoveRage = 0x63;
    private const uint VolatileRage = 0x800000;
    private const int ScriptNarcSubSeq = 1;
    private const int SubSeqFocusPunchStart = 0xE8;
    private const int ActionCommandNone = 0x27;
    private const int TurnStartLoop = 0;
    private const int TurnStartDone = 2;

    // OFFSET 0x0224C448
    private static void BattleController_DetermineActionOrder(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int flag = 0;
        int runner = 0;
        if ((battleType & BattleTypeNoActionOrder) != 0)
        {
            for (int i = 0; i < maxBattlers; i++)
            {
                battleCtx.BattlerActionOrder[i] = (byte)i;
            }
            battleCtx.Command = ControllerCommandTurnStart;
            return;
        }
        if ((battleType & BattleTypeLinkActionOrder) != 0)
        {
            for (int i = 0; i < maxBattlers; i++)
            {
                if (battleCtx.BattlerActions[i, ActionCommand] == ActionSelectedRun)
                {
                    flag = ActionOrderRunFirst;
                    runner = i;
                    break;
                }
            }
        }
        else
        {
            if (battleCtx.BattlerActions[0, ActionCommand] == ActionSelectedRun)
            {
                runner = 0;
                flag = ActionOrderRunFirst;
            }
            if (battleCtx.BattlerActions[2, ActionCommand] == ActionSelectedRun)
            {
                runner = 2;
                flag = ActionOrderRunFirst;
            }
        }
        if (flag == ActionOrderRunFirst)
        {
            battleCtx.BattlerActionOrder[0] = (byte)runner;
            int count = 1;
            for (int i = 0; i < maxBattlers; i++)
            {
                if (i != runner)
                {
                    battleCtx.BattlerActionOrder[count] = (byte)i;
                    count++;
                }
            }
            battleCtx.Command = ControllerCommandTurnStart;
            return;
        }
        int next = 0;
        for (int i = 0; i < maxBattlers; i++)
        {
            uint action = battleCtx.BattlerActions[i, ActionCommand];
            if (action - ActionSelectedItem <= 1)
            {
                battleCtx.BattlerActionOrder[next] = (byte)i;
                next++;
            }
        }
        for (int i = 0; i < maxBattlers; i++)
        {
            uint action = battleCtx.BattlerActions[i, ActionCommand];
            if (action != ActionSelectedItem && action != ActionSelectedSwitch)
            {
                battleCtx.BattlerActionOrder[next] = (byte)i;
                next++;
            }
        }
        for (int i = 0; i < maxBattlers - 1; i++)
        {
            for (int j = i + 1; j < maxBattlers; j++)
            {
                int first = battleCtx.BattlerActionOrder[i];
                int second = battleCtx.BattlerActionOrder[j];
                uint firstAction = battleCtx.BattlerActions[first, ActionCommand];
                if (firstAction == battleCtx.BattlerActions[second, ActionCommand])
                {
                    int ignoreQuickClaw = firstAction == ActionSelectedFight ? 0 : 1;
                    if (BattleContexts.BattleSystem_CompareBattlerSpeed(battleSys, battleCtx, first, second, ignoreQuickClaw) != 0)
                    {
                        battleCtx.BattlerActionOrder[i] = (byte)second;
                        battleCtx.BattlerActionOrder[j] = (byte)first;
                    }
                }
            }
        }
        battleCtx.Command = ControllerCommandTurnStart;
    }

    // OFFSET 0x0224C5C4
    private static void BattleController_TurnStart(BattleSystem battleSys, BattleContext battleCtx)
    {
        int result = TurnStartLoop;
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        do
        {
            switch (battleCtx.TurnStartCheckState)
            {
                case 0:
                    while (battleCtx.TurnStartCheckTemp < maxBattlers)
                    {
                        int battler = battleCtx.BattlerActionOrder[battleCtx.TurnStartCheckTemp];
                        if ((HealthBar.FlagIndex(battler) & battleCtx.BattlersSwitchingMask) != 0)
                        {
                            battleCtx.TurnStartCheckTemp++;
                            continue;
                        }
                        battleCtx.TurnStartCheckTemp++;
                        if ((battleCtx.BattleMons[battler].Status & StatusSleep) == 0
                            && BattleContexts.Battler_SelectedMove(battleCtx, battler) == MoveFocusPunch
                            && BattleContexts.Battler_CheckTruant(battleCtx, battler) == 0
                            && battleCtx.TurnFlags[battler].Struggling == 0)
                        {
                            BattleIO.BattleIO_ClearMessageBox(battleSys);
                            battleCtx.MsgBattlerTemp = battler;
                            BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubSeqFocusPunchStart);
                            battleCtx.CommandNext = battleCtx.Command;
                            battleCtx.Command = ControllerCommandRunScript;
                            return;
                        }
                    }
                    battleCtx.TurnStartCheckTemp = 0;
                    battleCtx.TurnStartCheckState++;
                    break;
                case 1:
                    for (int i = 0; i < maxBattlers; i++)
                    {
                        BattleMon mon = battleCtx.BattleMons[i];
                        if ((mon.StatusVolatile & VolatileRage) != 0 && BattleContexts.Battler_SelectedMove(battleCtx, i) != MoveRage)
                        {
                            mon.StatusVolatile &= ~VolatileRage;
                        }
                    }
                    battleCtx.TurnStartCheckState++;
                    break;
                case 2:
                    for (int i = 0; i < 4; i++)
                    {
                        battleCtx.SpeedRand[i] = (ushort)BattleSystems.BattleSystem_RandNext(battleSys);
                    }
                    battleCtx.TurnStartCheckState++;
                    break;
                case 3:
                    battleCtx.TurnStartCheckState = 0;
                    result = TurnStartDone;
                    break;
            }
        } while (result == TurnStartLoop);
        battleCtx.Command = ControllerCommandSortSpeed;
    }

    // OFFSET 0x0224C718
    private static void BattleController_SortSpeed(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        if (BattleContexts.BattleSystem_CheckRecordingStopped(battleSys, battleCtx) != 0)
        {
            return;
        }
        battleCtx.WaitingBattlers = 0;
        for (int i = 0; i < maxBattlers; i++)
        {
            if (battleCtx.BattlerActions[i, 0] != ActionCommandNone)
            {
                battleCtx.WaitingBattlers++;
            }
        }
        BattleContexts.BattleSystem_SortMonSpeedOrder(battleSys, battleCtx);
        if (battleCtx.TurnOrderCounter == maxBattlers)
        {
            battleCtx.TurnOrderCounter = 0;
            battleCtx.Command = ControllerCommandFirstAction;
            return;
        }
        battleCtx.Command = (int)battleCtx.BattlerActions[battleCtx.BattlerActionOrder[battleCtx.TurnOrderCounter], 0];
    }
}

public static partial class BattleIO
{
    private const int CommandClearMessageBox = 0x42;

    // OFFSET 0x02266AA0
    public static void BattleIO_ClearMessageBox(BattleSystem battleSys)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandClearMessageBox);
        BattleIO_Send(battleSys, RecipientBattler, 0, message, 4);
    }
}

public static partial class Battlers
{
    // OFFSET 0x0225CB80
    private static void Battler_CommandClearMessageBox(BattleSystem battleSys, BattlerData battler)
    {
        Window window = BattleSystems.BattleSystem_GetWindow(battleSys, 0);
        Bg.Window_FillTilemap(window, 0xFF);
        Bg.Window_LoadTiles(window);
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, 0x42);
        Battler_ClearCommand(battler);
    }
}
