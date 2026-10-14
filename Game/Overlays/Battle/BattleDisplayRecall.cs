namespace PlatinumSharp.Game;

public static partial class Battlers
{
    // OFFSET 0x0225C3F8
    private static void Battler_CommandPrintRecallMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PrintRecallMessage(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C40C
    private static void Battler_CommandPrintSwitchInMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PrintSwitchInMessage(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C454
    private static void Battler_CommandAlertMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PrintAlertMessage(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C468
    private static void Battler_CommandRefreshHPGauge(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_RefreshHPGauge(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }
}

public static partial class BattleDisplay
{
    private const int MessageLinkRecall = 0x3DE;
    private const int MessageTrainerRecall = 0x3DD;
    private const int MessageRecallFainted = 0x3D8;
    private const int MessageRecallLowHP = 0x3D9;
    private const int MessageRecallHalfHP = 0x3DC;
    private const int MessageRecallMostHP = 0x3DA;
    private const int MessageRecallFullHP = 0x3DB;
    private const int MessageSwitchInHuge = 0x3D6;
    private const int MessageSwitchInLarge = 0x3D7;
    private const int MessageSwitchInMedium = 0x3D5;
    private const int MessageSwitchInSmall = 0x3D4;
    private const int MessageSwitchInDefault = 0x3D3;
    private const int MessageTrainerSwitchIn = 0x3CC;
    private const int MessageLinkSwitchIn = 0x3CE;
    private const int TagLinkTrainerNickname = 0x1B;
    private const int TagTrainerClassNameNickname = 0x32;
    private const int TagMonNickname = 2;
    private const int AlertWaitFrames = 0x28;
    private const uint HPGaugeRefreshFlags = 0xFFFFFFDF;

    // OFFSET 0x0225DDD8
    public static void BattleDisplay_PrintRecallMessage(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        BattleMessage message = new();
        BattleDisplay_BuildRecallMessage(battleSys, battler, data, message);
        BattleDisplay_StartMessageWait(battleSys, battler, data[0], message);
    }

    // OFFSET 0x0225DE30
    public static void BattleDisplay_PrintSwitchInMessage(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        BattleMessage message = new();
        BattleDisplay_BuildSwitchInMessage(battleSys, battler, data, message);
        BattleDisplay_StartMessageWait(battleSys, battler, data[0], message);
    }

    // OFFSET 0x0225DF6C
    public static void BattleDisplay_PrintAlertMessage(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        if (battler.BootState == Battlers.BootStatePlayer)
        {
            MessageLoader msgLoader = BattleSystems.BattleSystem_GetMessageLoader(battleSys)!;
            AlertMessageTask task = new();
            task.BattleSys = battleSys;
            task.Command = data[0];
            task.Battler = battler.Battler;
            task.State = 0;
            BattleMessage message = new();
            BattleIO.ReadBattleMessage(data, 4, message);
            task.PrinterID = BattleMessages.BattleMessage_Print(battleSys, msgLoader, message, BattleMessages.BattleSystem_GetTextSpeed(battleSys));
            SysTask.SysTask_Start(BattleDisplay_AlertMessageTask, task, 0);
        }
        else if (battler.BootState == Battlers.BootStateAI)
        {
            BattleIO.BattleIO_SendAlertDone(battleSys, battler.Battler);
            BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, data[0]);
        }
        else
        {
            if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeLink) == 0)
            {
                BattleIO.BattleIO_SendAlertDone(battleSys, battler.Battler);
            }
            BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, data[0]);
        }
    }

    // OFFSET 0x0225E008
    public static void BattleDisplay_RefreshHPGauge(BattleSystem battleSys, BattlerData battler, byte[] data)
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
        healthbox.NumSafariBalls = (byte)BattleIO.R32(data, 0x10);
        Healthboxes.Healthbox_DrawInfo(healthbox, healthbox.CurHP, HPGaugeRefreshFlags);
        BattleIO.BattleIO_ClearMessage(healthbox.BattleSys, healthbox.Battler, healthbox.Command);
    }

    // OFFSET 0x02262FC0
    private static void BattleDisplay_AlertMessageTask(SysTask sysTask, object? param)
    {
        AlertMessageTask task = (AlertMessageTask)param!;
        switch (task.State)
        {
            case 0:
                if (Text.Text_IsPrinterActive(task.PrinterID) == 0)
                {
                    task.State++;
                    task.Timer = 0;
                }
                break;
            case 1:
                task.Timer++;
                if (task.Timer == AlertWaitFrames)
                {
                    BattleIO.BattleIO_SendAlertDone(task.BattleSys!, task.Battler);
                    BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
                    SysTask.SysTask_Done(sysTask);
                }
                break;
        }
    }

    // OFFSET 0x02263C1C
    private static void BattleDisplay_BuildRecallMessage(BattleSystem battleSys, BattlerData battler, byte[] data, BattleMessage message)
    {
        int mon = battler.Battler | (data[1] << 8);
        if ((battler.BattlerType & 1) != 0)
        {
            if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeLink) != 0)
            {
                message.Id = MessageLinkRecall;
                message.Tags = TagLinkTrainerNickname;
                message.Params[0] = battler.Battler;
                message.Params[1] = mon;
            }
            else
            {
                message.Id = MessageTrainerRecall;
                message.Tags = TagTrainerClassNameNickname;
                message.Params[0] = battler.Battler;
                message.Params[1] = battler.Battler;
                message.Params[2] = mon;
            }
            return;
        }
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) == 0 && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeLink) == 0)
        {
            int hpPercent = BattleIO.R16(data, 2);
            if (hpPercent == 0)
            {
                message.Id = MessageRecallFainted;
            }
            else if (hpPercent < 25)
            {
                message.Id = MessageRecallLowHP;
            }
            else if (hpPercent < 50)
            {
                message.Id = MessageRecallHalfHP;
            }
            else if (hpPercent < 75)
            {
                message.Id = MessageRecallMostHP;
            }
            else
            {
                message.Id = MessageRecallFullHP;
            }
        }
        else
        {
            message.Id = MessageRecallLowHP;
        }
        message.Tags = TagMonNickname;
        message.Params[0] = mon;
    }

    // OFFSET 0x02263CF0
    private static void BattleDisplay_BuildSwitchInMessage(BattleSystem battleSys, BattlerData battler, byte[] data, BattleMessage message)
    {
        int mon = battler.Battler | (data[1] << 8);
        if ((battler.BattlerType & 1) != 0)
        {
            if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeLink) == 0)
            {
                message.Id = MessageTrainerSwitchIn;
                message.Tags = TagTrainerClassNameNickname;
                message.Params[0] = battler.Battler;
                message.Params[1] = battler.Battler;
                message.Params[2] = mon;
            }
            else
            {
                message.Id = MessageLinkSwitchIn;
                message.Tags = TagLinkTrainerNickname;
                message.Params[0] = battler.Battler;
                message.Params[1] = mon;
            }
            return;
        }
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) == 0 && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeLink) == 0)
        {
            int damage = BattleIO.R16(data, 2);
            if (damage < 100)
            {
                message.Id = MessageSwitchInHuge;
            }
            else if (damage < 325)
            {
                message.Id = MessageSwitchInLarge;
            }
            else if (damage < 550)
            {
                message.Id = MessageSwitchInMedium;
            }
            else if (damage < 775)
            {
                message.Id = MessageSwitchInSmall;
            }
            else
            {
                message.Id = MessageSwitchInDefault;
            }
        }
        else
        {
            message.Id = MessageSwitchInDefault;
        }
        message.Tags = TagMonNickname;
        message.Params[0] = mon;
    }
}

public static partial class BattleIO
{
    private const int AlertDoneSignal = 1;

    // OFFSET 0x022661B0
    public static void BattleIO_SendAlertDone(BattleSystem battleSys, int battler)
    {
        byte[] message = new byte[4];
        W32(message, 0, AlertDoneSignal);
        BattleIO_Send(battleSys, 0, battler, message, 4);
    }
}
