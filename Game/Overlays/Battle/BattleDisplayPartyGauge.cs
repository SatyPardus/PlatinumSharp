namespace PlatinumSharp.Game;

public sealed class PartyGaugeTask
{
    public BattleSystem? BattleSys;
    public byte Command;
    public byte Battler;
    public byte BattlerType;
    public byte State;
    public readonly byte[] Status = new byte[PartyGauge.BallCount];
    public byte Mode;
}

public static partial class BattleDisplay
{
    private const int PartyGaugeStatusOffset = 2;

    private static PartyGaugeTask BattleDisplay_NewPartyGaugeTask(BattleSystem battleSys, BattlerData battler, byte[] data, bool copyStatus, byte mode)
    {
        PartyGaugeTask task = new();
        task.State = 0;
        task.BattleSys = battleSys;
        task.Command = data[0];
        task.Battler = battler.Battler;
        task.BattlerType = battler.BattlerType;
        if (copyStatus)
        {
            for (int i = 0; i < PartyGauge.BallCount; i++)
            {
                task.Status[i] = data[PartyGaugeStatusOffset + i];
            }
        }
        task.Mode = mode;
        return task;
    }

    // OFFSET 0x0225E134
    public static void BattleDisplay_ShowPartyGaugeAtStart(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        SysTask.SysTask_Start(BattleDisplay_ShowPartyGaugeTask, BattleDisplay_NewPartyGaugeTask(battleSys, battler, data, true, 0), 0);
    }

    // OFFSET 0x0225E17C
    public static void BattleDisplay_HidePartyGaugeAtStart(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        SysTask.SysTask_Start(BattleDisplay_HidePartyGaugeTask, BattleDisplay_NewPartyGaugeTask(battleSys, battler, data, false, 0), 0);
    }

    // OFFSET 0x0225E1B4
    public static void BattleDisplay_ShowPartyGauge(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        SysTask.SysTask_Start(BattleDisplay_ShowPartyGaugeTask, BattleDisplay_NewPartyGaugeTask(battleSys, battler, data, true, 1), 0);
    }

    // OFFSET 0x0225E200
    public static void BattleDisplay_HidePartyGauge(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        SysTask.SysTask_Start(BattleDisplay_HidePartyGaugeTask, BattleDisplay_NewPartyGaugeTask(battleSys, battler, data, false, 1), 0);
    }

    // OFFSET 0x0226354C
    private static void BattleDisplay_ShowPartyGaugeTask(SysTask sysTask, object? param)
    {
        PartyGaugeTask task = (PartyGaugeTask)param!;
        int side = BattleSystems.BattleSystem_GetBattlerSide(task.BattleSys!, task.Battler) == 0 ? PartyGauges.SidePlayer : PartyGauges.SideEnemy;
        switch (task.State)
        {
            case 0:
            {
                int mode;
                int position;
                if (task.Mode == 0)
                {
                    mode = 0;
                    position = 1;
                }
                else
                {
                    mode = 1;
                    switch (task.BattlerType)
                    {
                        case 3:
                            position = 0;
                            break;
                        case 5:
                            position = 2;
                            break;
                        default:
                            position = 1;
                            break;
                    }
                }
                SpriteSystem spriteSys = BattleSystems.BattleSystem_GetSpriteSystem(task.BattleSys!)!;
                SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(task.BattleSys!)!;
                PartyGauge gauge = PartyGauges.PartyGauge_Show(task.Status, 0, side, mode, position, spriteSys, spriteMan);
                BattleSystems.BattleSystem_SetPartyGauge(task.BattleSys!, side, gauge);
                task.State++;
                break;
            }
            case 1:
                if (PartyGauges.PartyGauge_ShowIsDone(BattleSystems.BattleSystem_GetPartyGauge(task.BattleSys!, side)!) == 1)
                {
                    task.State++;
                }
                break;
            default:
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }

    // OFFSET 0x02263604
    private static void BattleDisplay_HidePartyGaugeTask(SysTask sysTask, object? param)
    {
        PartyGaugeTask task = (PartyGaugeTask)param!;
        int side = BattleSystems.BattleSystem_GetBattlerSide(task.BattleSys!, task.Battler) == 0 ? PartyGauges.SidePlayer : PartyGauges.SideEnemy;
        PartyGauge? gauge = BattleSystems.BattleSystem_GetPartyGauge(task.BattleSys!, side);
        switch (task.State)
        {
            case 0:
            {
                int mode = task.Mode == 0 ? 0 : 1;
                PartyGauges.PartyGauge_Hide(gauge, mode, mode);
                task.State++;
                break;
            }
            case 1:
                if (PartyGauges.PartyGauge_HideIsDone(gauge!) == 1)
                {
                    PartyGauges.PartyGauge_Free(gauge!);
                    BattleSystems.BattleSystem_SetPartyGauge(task.BattleSys!, side, null);
                    task.State++;
                }
                break;
            default:
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }
}
