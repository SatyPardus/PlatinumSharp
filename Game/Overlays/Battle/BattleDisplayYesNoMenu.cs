using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class YesNoMenuTask
{
    public BattleSystem? BattleSys;
    public HealthBox? Healthbox;
    public int Input;
    public byte Command;
    public byte Battler;
    public byte State;
    public byte Type;
    public int MessageID;
    public int Nickname;
    public ushort Move;
    public ushort PrinterID;
}

public static partial class Battlers
{
    // OFFSET 0x0225C2D8
    private static void Battler_CommandShowYesNoMenu(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_ShowYesNoMenu(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }
}

public static partial class BattleDisplay
{
    private const int YesNoTypeGiveUpMove = 5;
    private const int YesNoTagNickname = 0x82;
    private const int YesNoMenuBase = 0xD;
    private const int YesYesResult = 1;

    private static SysTaskFunc YesNoTaskFunc(int address)
    {
        return address switch
        {
            0x022623F0 => BattleDisplay_YesNoMenuTask,
            0x0226262C => BattleDisplay_AIYesNoMenuTask,
            0x0226264C => BattleDisplay_IgnoreYesNoMenuTask,
            0x0226266C => BattleDisplay_RecordedYesNoMenuTask,
            _ => throw Missing((uint)address),
        };
    }

    // OFFSET 0x0225D8AC
    public static void BattleDisplay_ShowYesNoMenu(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        YesNoMenuTask task = new();
        task.State = 0;
        task.BattleSys = battleSys;
        task.Command = data[0];
        task.Battler = battler.Battler;
        task.Healthbox = battler.Healthbox;
        task.MessageID = BattleIO.R16(data, 2);
        task.Type = data[1];
        task.Move = (ushort)BattleIO.R32(data, 4);
        task.Nickname = (int)BattleIO.R32(data, 8);
        SysTask.SysTask_Start(YesNoTaskFunc(battler.TaskFuncs.ShowYesNoMenu), task, 0);
    }

    // OFFSET 0x022623F0
    private static void BattleDisplay_YesNoMenuTask(SysTask sysTask, object? param)
    {
        YesNoMenuTask task = (YesNoMenuTask)param!;
        BattleSystems.BattleSystem_GetBgConfig(task.BattleSys!);
        BattleSubscreen subscreen = BattleSystems.BattleSystem_GetBattleSubscreen(task.BattleSys!)!;
        BattlerData battlerData = BattleSystems.BattleSystem_GetBattlerData(task.BattleSys!, task.Battler)!;
        HealthBox? partnerHealthbox = null;
        int partner = BattleSystems.BattleSystem_GetPartner(task.BattleSys!, task.Battler);
        if (partner != task.Battler)
        {
            partnerHealthbox = BattleSystems.BattleSystem_GetHealthbox(task.BattleSys!, partner);
        }
        switch (task.State)
        {
            case 0:
                if (BattleSubscreens.BattleSubscreen_IsMenuIdle(subscreen) == 0)
                {
                    break;
                }
                if (task.MessageID != 0)
                {
                    MessageLoader msgLoader = BattleSystems.BattleSystem_GetMessageLoader(task.BattleSys!)!;
                    BattleMessage msg = new();
                    if (task.Type == YesNoTypeGiveUpMove)
                    {
                        msg.Tags = YesNoTagNickname;
                        msg.Params[0] = task.Nickname;
                    }
                    else
                    {
                        msg.Tags = 0;
                    }
                    msg.Id = (ushort)task.MessageID;
                    task.PrinterID = BattleMessages.BattleMessage_Print(task.BattleSys!, msgLoader, msg, BattleMessages.BattleSystem_GetTextSpeed(task.BattleSys!));
                }
                task.State = 1;
                break;
            case 1:
                {
                    if (Text.Text_IsPrinterActive((byte)task.PrinterID) != 0 && task.MessageID != 0)
                    {
                        break;
                    }
                    NARC subscreenNarc = NARC.NARC_ctor(7, HeapIDBattle);
                    NARC battleNarc = NARC.NARC_ctor(0x1B, HeapIDBattle);
                    BattleSubscreens.BattleSubscreen_HidePartyBalls(subscreen);
                    GiveUpMoveData menuData = new();
                    menuData.Move = task.Move;
                    switch (task.Type)
                    {
                        case 0:
                        case YesNoTypeGiveUpMove:
                            BattleSubscreens.BattleSubscreen_LoadMenu(subscreenNarc, battleNarc, subscreen, YesNoMenuBase, 0, menuData);
                            break;
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            BattleSubscreens.BattleSubscreen_LoadMenu(subscreenNarc, battleNarc, subscreen, YesNoMenuBase + task.Type, 0, menuData);
                            break;
                        default:
                            ErrorHandling.GF_AssertFail();
                            break;
                    }
                    task.State = 2;
                    NARC.NARC_dtor(subscreenNarc);
                    NARC.NARC_dtor(battleNarc);
                    break;
                }
            case 2:
                task.Input = BattleSubscreens.BattleSubscreen_CheckTouchInput(subscreen);
                if (task.Input != TouchScreen.ButtonNotPressed)
                {
                    SoundPlayback.Sound_PlayEffect(SeqTargetSelect);
                    task.State = 3;
                }
                break;
            case 3:
                {
                    if (BattleSubscreens.BattleSubscreen_IsMenuIdle(subscreen) != 1)
                    {
                        break;
                    }
                    NARC subscreenNarc = NARC.NARC_ctor(7, HeapIDBattle);
                    NARC battleNarc = NARC.NARC_ctor(0x1B, HeapIDBattle);
                    Healthboxes.Healthbox_StopArrowAnim(task.Healthbox!);
                    Battlers.Battler_StopIdleBob(battlerData);
                    Healthboxes.Healthbox_OnPartnerCommandEnd(partnerHealthbox);
                    BattleSubscreens.BattleSubscreen_HidePartyBalls(subscreen);
                    BattleSubscreens.BattleSubscreen_LoadMenu(subscreenNarc, battleNarc, subscreen, 0, 0, null);
                    if (task.Input == YesYesResult)
                    {
                        BattleSubscreens.BattleSubscreen_SetCommandMenuActive(subscreen, 0);
                    }
                    task.State = 4;
                    NARC.NARC_dtor(subscreenNarc);
                    NARC.NARC_dtor(battleNarc);
                    break;
                }
            case 4:
                if (BattleSubscreens.BattleSubscreen_NoPendingTask(subscreen) == 1)
                {
                    BattleIO.BattleIO_CommandSelectionResult(task.BattleSys!, task.Battler, (uint)task.Input);
                    BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
                    SysTask.SysTask_Done(sysTask);
                }
                break;
        }
    }

    // OFFSET 0x0226262C
    private static void BattleDisplay_AIYesNoMenuTask(SysTask sysTask, object? param)
    {
        YesNoMenuTask task = (YesNoMenuTask)param!;
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x0226264C
    private static void BattleDisplay_IgnoreYesNoMenuTask(SysTask sysTask, object? param)
    {
        YesNoMenuTask task = (YesNoMenuTask)param!;
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x0226266C
    private static void BattleDisplay_RecordedYesNoMenuTask(SysTask sysTask, object? param)
    {
        YesNoMenuTask task = (YesNoMenuTask)param!;
        if (BattleSystems.BattleSystem_ReadRecorded(task.BattleSys!, task.Battler, out byte data) == 1)
        {
            BattleDisplay_StopPlaybackDesync(task.BattleSys!);
        }
        if (data != 0xFF && data != 1)
        {
            BattleDisplay_StopPlaybackInvalid(task.BattleSys!);
        }
        BattleIO.BattleIO_CommandSelectionResult(task.BattleSys!, task.Battler, data);
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }
}
