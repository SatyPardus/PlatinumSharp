namespace PlatinumSharp.Game;

public static partial class BattleDisplay
{
    private const uint HealthboxTaskPriority = 1000;

    // OFFSET 0x0225D4A8
    public static void BattleDisplay_SlideHealthboxIn(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        HealthBox healthbox = battler.Healthbox;
        healthbox.State = 0;
        healthbox.BattleSys = battleSys;
        healthbox.Battler = battler.Battler;
        healthbox.Type = Healthboxes.Healthbox_Type(battler.BattlerType, BattleSystems.BattleSystem_GetBattleType(battleSys));
        healthbox.Command = data[0];
        healthbox.CurHP = (short)BattleIO.R16(data, 2);
        healthbox.MaxHP = BattleIO.R16(data, 4);
        healthbox.Level = data[1];
        healthbox.Gender = (byte)((data[7] >> 5) & 3);
        healthbox.Damage = 0;
        healthbox.CurExp = (int)BattleIO.R32(data, 8);
        healthbox.MaxExp = (int)BattleIO.R32(data, 0xC);
        healthbox.SelectedPartySlot = data[6];
        healthbox.Status = (byte)(data[7] & 0x1F);
        healthbox.CaughtSpecies = (byte)(data[7] >> 7);
        healthbox.Delay = data[0x14];
        healthbox.NumSafariBalls = (byte)BattleIO.R32(data, 0x10);
        Healthboxes.Healthbox_Enable(healthbox, 0);
        Healthboxes.Healthbox_DrawInfo(healthbox, healthbox.CurHP, Healthboxes.InfoAll);
        healthbox.Task10 = SysTask.SysTask_Start(BattleDisplay_SlideHealthboxInTask, healthbox, HealthboxTaskPriority);
    }

    // OFFSET 0x0225D570
    public static void BattleDisplay_SlideHealthboxOut(BattleSystem battleSys, BattlerData battler)
    {
        HealthBox healthbox = battler.Healthbox;
        healthbox.State = 0;
        healthbox.BattleSys = battleSys;
        healthbox.Battler = battler.Battler;
        healthbox.Command = battler.Data[0];
        Healthboxes.Healthbox_Scroll(healthbox, 1);
        healthbox.Task10 = SysTask.SysTask_Start(BattleDisplay_SlideHealthboxOutTask, healthbox, HealthboxTaskPriority);
    }

    // OFFSET 0x0226040C
    private static void BattleDisplay_SlideHealthboxInTask(SysTask task, object? param)
    {
        HealthBox healthbox = (HealthBox)param!;
        switch (healthbox.State)
        {
            case 0:
                if (healthbox.Delay > 0)
                {
                    healthbox.Delay--;
                    break;
                }
                Healthboxes.Healthbox_Scroll(healthbox, 0);
                Healthboxes.Healthbox_Enable(healthbox, 1);
                healthbox.State++;
                break;
            case 1:
                if (healthbox.DoneScrolling == 1)
                {
                    healthbox.State++;
                }
                break;
            default:
                BattleIO.BattleIO_ClearMessage(healthbox.BattleSys!, healthbox.Battler, healthbox.Command);
                healthbox.Task10 = null;
                SysTask.SysTask_Done(task);
                break;
        }
    }

    // OFFSET 0x02260480
    private static void BattleDisplay_SlideHealthboxOutTask(SysTask task, object? param)
    {
        HealthBox healthbox = (HealthBox)param!;
        switch (healthbox.State)
        {
            case 0:
                if (healthbox.DoneScrolling == 1)
                {
                    healthbox.State++;
                }
                break;
            default:
                BattleIO.BattleIO_ClearMessage(healthbox.BattleSys!, healthbox.Battler, healthbox.Command);
                healthbox.Task10 = null;
                SysTask.SysTask_Done(task);
                Healthboxes.Healthbox_Enable(healthbox, 0);
                break;
        }
    }
}
