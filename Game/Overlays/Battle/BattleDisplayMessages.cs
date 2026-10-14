namespace PlatinumSharp.Game;

public sealed class MessageWaitTask
{
    public BattleSystem? BattleSys;
    public byte Command;
    public byte Battler;
    public byte PrinterID;
}

public static partial class BattleDisplay
{
    private const int MessageEncounterTrainer = 0x3C9;
    private const int MessageTrainerSentOutMon = 0x3CC;
    private const int MessageTrainerSentOutTwoMons = 0x3CD;
    private const int MessageLinkTrainerSentOutMon = 0x3CE;
    private const int MessageLinkTrainerSentOutTwoMons = 0x3CF;
    private const int MessageLinkTrainersSentOutMons = 0x3D0;
    private const int MessageLinkPartnerSentOutMons = 0x3D1;
    private const int MessageGoTwoMons = 0x3D2;
    private const int MessageGoMon = 0x3D3;
    private const int MessageTrainersSentOutMons = 0x3DF;
    private const int MessagePartnerSentOutMons = 0x3E1;
    private const int SendOutSlotOffset = 4;
    private const int MessageEncounterLinkTrainer = 0x3CA;
    private const int MessageEncounterLinkTrainers = 0x3CB;
    private const int MessageEncounterTwoTrainers = 0x3E0;
    private const int TagTrainerNames = 0x1A;
    private const int TagTrainerClassAndName = 0x1E;
    private const int TagTwoTrainerClassesAndNames = 0x3B;
    private const uint BattleTypeFrontierFlag = 0x80;

    // OFFSET 0x0223F29C
    public static WaitDial? BattleSystem_GetWaitDial(BattleSystem battleSys)
    {
        return battleSys.WaitDial;
    }

    // OFFSET 0x0223F2A4
    public static void BattleSystem_SetWaitDial(BattleSystem battleSys, WaitDial? waitDial)
    {
        battleSys.WaitDial = waitDial;
    }

    private static void BattleDisplay_StartMessageWait(BattleSystem battleSys, BattlerData battler, int command, BattleMessage message)
    {
        MessageLoader msgLoader = BattleSystems.BattleSystem_GetMessageLoader(battleSys)!;
        MessageWaitTask task = new();
        task.BattleSys = battleSys;
        task.Command = (byte)command;
        task.Battler = battler.Battler;
        task.PrinterID = BattleMessages.BattleMessage_Print(battleSys, msgLoader, message, BattleMessages.BattleSystem_GetTextSpeed(battleSys));
        SysTask.SysTask_Start(BattleDisplay_MessageWaitTask, task, 0);
    }

    // OFFSET 0x0225DE88
    public static void BattleDisplay_PrintEncounterMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleMessage message = new();
        BattleDisplay_BuildEncounterMessage(battleSys, battler, message);
        BattleDisplay_StartMessageWait(battleSys, battler, BattleIO.CommandEncounterMessage, message);
    }

    // OFFSET 0x0225DEDC
    public static void BattleDisplay_PrintSendOutMessage(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        BattleMessage message = new();
        BattleDisplay_BuildSendOutMessage(battleSys, battler, data, message);
        BattleDisplay_StartMessageWait(battleSys, battler, data[0], message);
    }

    private static int SendOutMon(byte[] data, int battler)
    {
        return (data[SendOutSlotOffset + battler] << 8) | battler;
    }

    // OFFSET 0x02263E7C
    private static void BattleDisplay_BuildSendOutMessage(BattleSystem battleSys, BattlerData battler, byte[] data, BattleMessage message)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int first;
        int second;
        if ((battler.BattlerType & 1) != 0)
        {
            bool doubles = (battleType & BattleTypeDoubles) != 0;
            first = battler.Battler;
            second = doubles ? BattleSystems.BattleSystem_GetPartner(battleSys, first) : first;
            if ((battleType & BattleTypeLink) != 0)
            {
                if ((battleType & BattleTypeFrontierFlag) != 0)
                {
                    message.Id = MessageTrainersSentOutMons;
                    message.Tags = 0x3C;
                    message.Params[0] = first;
                    message.Params[1] = first;
                    message.Params[2] = SendOutMon(data, first);
                    message.Params[3] = second;
                    message.Params[4] = second;
                    message.Params[5] = SendOutMon(data, second);
                }
                else if ((battleType & BattleTypeMulti) != 0)
                {
                    message.Id = MessageLinkTrainersSentOutMons;
                    message.Tags = 0x38;
                    message.Params[0] = first;
                    message.Params[1] = SendOutMon(data, first);
                    message.Params[2] = second;
                    message.Params[3] = SendOutMon(data, second);
                }
                else if (doubles)
                {
                    message.Id = MessageLinkTrainerSentOutTwoMons;
                    message.Tags = 0x31;
                    message.Params[0] = first;
                    message.Params[1] = SendOutMon(data, first);
                    message.Params[2] = SendOutMon(data, second);
                }
                else
                {
                    message.Id = MessageLinkTrainerSentOutMon;
                    message.Tags = 0x1B;
                    message.Params[0] = first;
                    message.Params[1] = SendOutMon(data, first);
                }
            }
            else if ((battleType & BattleTypeTag) != 0 || (battleType & BattleTypeMulti) != 0)
            {
                message.Id = MessageTrainersSentOutMons;
                message.Tags = 0x3C;
                message.Params[0] = first;
                message.Params[1] = first;
                message.Params[2] = SendOutMon(data, first);
                message.Params[3] = second;
                message.Params[4] = second;
                message.Params[5] = SendOutMon(data, second);
            }
            else if (doubles)
            {
                message.Id = MessageTrainerSentOutTwoMons;
                message.Tags = 0x39;
                message.Params[0] = first;
                message.Params[1] = first;
                message.Params[2] = SendOutMon(data, first);
                message.Params[3] = SendOutMon(data, second);
            }
            else
            {
                message.Id = MessageTrainerSentOutMon;
                message.Tags = 0x32;
                message.Params[0] = first;
                message.Params[1] = first;
                message.Params[2] = SendOutMon(data, first);
            }
            return;
        }
        bool link = (battleType & BattleTypeLink) != 0;
        bool multi = (battleType & BattleTypeMulti) != 0;
        if (link)
        {
            if (multi)
            {
                throw Unported.Partial(0x02263E7C);
            }
            if ((battleType & BattleTypeDoubles) != 0)
            {
                first = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 2);
                second = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 4);
            }
            else
            {
                first = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 0);
                second = first;
            }
        }
        else if (multi)
        {
            first = BattleSystems.BattleSystem_GetPartner(battleSys, battler.Battler);
            second = battler.Battler;
        }
        else if ((battleType & BattleTypeDoubles) != 0)
        {
            first = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 2);
            second = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 4);
        }
        else
        {
            first = battler.Battler;
            second = first;
        }
        if (link && multi)
        {
            message.Id = MessageLinkPartnerSentOutMons;
            message.Tags = 0x31;
            message.Params[0] = first;
            message.Params[1] = SendOutMon(data, first);
            message.Params[2] = SendOutMon(data, second);
        }
        else if (!link && multi)
        {
            message.Id = MessagePartnerSentOutMons;
            message.Tags = 0x39;
            message.Params[0] = first;
            message.Params[1] = first;
            message.Params[2] = SendOutMon(data, first);
            message.Params[3] = SendOutMon(data, second);
        }
        else if ((battleType & BattleTypeDoubles) != 0)
        {
            message.Id = MessageGoTwoMons;
            message.Tags = 9;
            message.Params[0] = SendOutMon(data, first);
            message.Params[1] = SendOutMon(data, second);
        }
        else
        {
            message.Id = MessageGoMon;
            message.Tags = 2;
            message.Params[0] = SendOutMon(data, first);
        }
    }

    // OFFSET 0x02262F7C
    private static void BattleDisplay_MessageWaitTask(SysTask sysTask, object? param)
    {
        MessageWaitTask task = (MessageWaitTask)param!;
        if (BattleSystem_GetWaitDial(task.BattleSys!) != null)
        {
            RenderWindow.DestroyWaitDial(BattleSystem_GetWaitDial(task.BattleSys!)!);
            BattleSystem_SetWaitDial(task.BattleSys!, null);
        }
        if (Text.Text_IsPrinterActive(task.PrinterID) == 0)
        {
            BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
            SysTask.SysTask_Done(sysTask);
        }
    }

    // OFFSET 0x02263DD0
    private static void BattleDisplay_BuildEncounterMessage(BattleSystem battleSys, BattlerData battler, BattleMessage message)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int first;
        int second;
        if ((battleType & BattleTypeDoubles) != 0)
        {
            first = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 3);
            second = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 5);
        }
        else
        {
            first = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 1);
            second = first;
        }
        if ((battleType & BattleTypeLink) != 0)
        {
            if ((battleType & BattleTypeFrontierFlag) != 0)
            {
                message.Id = MessageEncounterTwoTrainers;
                message.Tags = TagTwoTrainerClassesAndNames;
                message.Params[0] = first;
                message.Params[1] = first;
                message.Params[2] = second;
                message.Params[3] = second;
            }
            else if ((battleType & BattleTypeMulti) != 0)
            {
                message.Id = MessageEncounterLinkTrainers;
                message.Tags = TagTrainerNames;
                message.Params[0] = first;
                message.Params[1] = second;
            }
            else
            {
                message.Id = MessageEncounterLinkTrainer;
                message.Tags = TagTrainerNames;
                message.Params[0] = first;
            }
        }
        else if ((battleType & BattleTypeTag) != 0 || (battleType & BattleTypeMulti) != 0)
        {
            message.Id = MessageEncounterTwoTrainers;
            message.Tags = TagTwoTrainerClassesAndNames;
            message.Params[0] = first;
            message.Params[1] = first;
            message.Params[2] = second;
            message.Params[3] = second;
        }
        else
        {
            message.Id = MessageEncounterTrainer;
            message.Tags = TagTrainerClassAndName;
            message.Params[0] = first;
            message.Params[1] = first;
        }
    }
}
