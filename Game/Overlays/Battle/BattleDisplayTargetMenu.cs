using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class TargetSelectTask
{
    public BattleSystem? BattleSys;
    public HealthBox? Healthbox;
    public int Input;
    public byte Command;
    public byte Battler;
    public byte BattlerType;
    public byte State;
    public readonly byte[] Targets = new byte[0x20];
    public ushort Range;
    public byte IsLastBattler;
}

public static partial class Battlers
{
    // OFFSET 0x0225C29C
    private static void Battler_CommandShowTargetSelectMenu(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_ShowTargetSelectMenu(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }
}

public static partial class BattleDisplay
{
    private const int TargetSelectMenuIndex = 0xC;
    private const int SeqTargetSelect = 0x5DD;
    private const int TargetCancel = 0xFF;
    private const int BattleContextParamAITarget = 0xB;
    private const int RangeSingleTarget = 0;
    private const int RangeAllyOrUser = 0x200;
    private const uint BattleTypeDoublesFlag = 2;

    private static SysTaskFunc TargetSelectTaskFunc(int address)
    {
        return address switch
        {
            0x02260F14 => BattleDisplay_TargetSelectTask,
            0x022610A8 => BattleDisplay_AITargetSelectTask,
            0x022610EC => BattleDisplay_IgnoreTargetSelectTask,
            0x0226110C => BattleDisplay_RecordedTargetSelectTask,
            _ => throw Missing((uint)address),
        };
    }

    // OFFSET 0x0225D708
    public static void BattleDisplay_ShowTargetSelectMenu(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        TargetSelectTask task = new();
        task.State = 0;
        task.BattleSys = battleSys;
        task.Command = data[0];
        task.Battler = battler.Battler;
        task.BattlerType = battler.BattlerType;
        task.Range = (ushort)BattleIO.R16(data, 2);
        task.Healthbox = battler.Healthbox;
        task.IsLastBattler = data[1];
        byte[] battlerTypes = new byte[4];
        BattleSystems.BattleSystem_GetBattlerTypes(battleSys, battlerTypes);
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleSystems.BattleSystem_GetBattleType(battleSys);
        for (int i = 0; i < maxBattlers; i++)
        {
            Array.Copy(data, 4 + i * 8, task.Targets, i * 8, 8);
        }
        SysTask.SysTask_Start(TargetSelectTaskFunc(battler.TaskFuncs.ShowTargetSelectMenu), task, 0);
    }

    private static TargetSelectData BattleDisplay_BuildTargetSelectData(TargetSelectTask task)
    {
        TargetSelectData menuData = new();
        for (int i = 0; i < 4; i++)
        {
            TargetMonData target = menuData.TargetMonData[i];
            target.PartySlot = task.Targets[i * 8];
            target.Gender = task.Targets[i * 8 + 1] & 3;
            target.IsPresent = (task.Targets[i * 8 + 1] >> 2) & 1;
            target.StockStatus = task.Targets[i * 8 + 2];
            target.Padding03 = task.Targets[i * 8 + 3];
            target.CurHP = (short)BattleIO.R16(task.Targets, i * 8 + 4);
            target.MaxHP = (ushort)BattleIO.R16(task.Targets, i * 8 + 6);
        }
        menuData.BattlerType = task.BattlerType;
        menuData.TargetingLayout = (byte)BattleSubscreens.BattleSubscreen_TargetingLayoutForRange(task.Range, task.BattlerType);
        return menuData;
    }

    // OFFSET 0x02260F14
    private static void BattleDisplay_TargetSelectTask(SysTask sysTask, object? param)
    {
        TargetSelectTask task = (TargetSelectTask)param!;
        BattleSystems.BattleSystem_GetBgConfig(task.BattleSys!);
        BattlerData battlerData = BattleSystems.BattleSystem_GetBattlerData(task.BattleSys!, task.Battler)!;
        BattleSubscreen subscreen = BattleSystems.BattleSystem_GetBattleSubscreen(task.BattleSys!)!;
        HealthBox? partnerHealthbox = null;
        int partner = BattleSystems.BattleSystem_GetPartner(task.BattleSys!, task.Battler);
        if (partner != task.Battler)
        {
            partnerHealthbox = BattleSystems.BattleSystem_GetHealthbox(task.BattleSys!, partner);
        }
        switch (task.State)
        {
            case 0:
                {
                    if (BattleSubscreens.BattleSubscreen_IsMenuIdle(subscreen) == 0)
                    {
                        return;
                    }
                    NARC subscreenNarc = NARC.NARC_ctor(7, HeapIDBattle);
                    NARC battleNarc = NARC.NARC_ctor(0x1B, HeapIDBattle);
                    BattleSubscreens.BattleSubscreen_LoadMenu(subscreenNarc, battleNarc, subscreen, TargetSelectMenuIndex, 0, BattleDisplay_BuildTargetSelectData(task));
                    NARC.NARC_dtor(subscreenNarc);
                    NARC.NARC_dtor(battleNarc);
                    task.State++;
                    return;
                }
            case 1:
                task.Input = BattleSubscreens.BattleSubscreen_CheckTouchInput(subscreen);
                if (task.Input != TouchScreen.ButtonNotPressed)
                {
                    SoundPlayback.Sound_PlayEffect(SeqTargetSelect);
                    task.State++;
                }
                return;
            case 2:
                if (task.Input != TargetCancel)
                {
                    Healthboxes.Healthbox_StopArrowAnim(task.Healthbox!);
                    Battlers.Battler_StopIdleBob(battlerData);
                    Healthboxes.Healthbox_OnPartnerCommandEnd(partnerHealthbox);
                    if (task.IsLastBattler == 1)
                    {
                        BattleSubscreens.BattleSubscreen_SetCommandMenuActive(subscreen, 0);
                    }
                }
                task.State++;
                return;
            case 3:
                task.State++;
                break;
        }
        if (BattleSubscreens.BattleSubscreen_NoPendingTask(subscreen) != 1)
        {
            return;
        }
        int input = task.Input;
        uint battleType = BattleSystems.BattleSystem_GetBattleType(task.BattleSys!);
        if (input != TargetCancel)
        {
            byte[] battlers = new byte[4];
            BattleSystems.BattleSystem_GetBattlersByType(task.BattleSys!, battlers);
            if ((battleType & BattleTypeDoublesFlag) != 0)
            {
                input = battlers[task.Input + 1] + 1;
            }
            else
            {
                input = battlers[task.Input - 1] + 1;
            }
        }
        BattleIO.BattleIO_TargetSelectResult(task.BattleSys!, task.Battler, (uint)input);
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x022610A8
    private static void BattleDisplay_AITargetSelectTask(SysTask sysTask, object? param)
    {
        TargetSelectTask task = (TargetSelectTask)param!;
        int target = BattleContexts.BattleContext_Get(task.BattleSys!, BattleSystems.BattleSystem_GetBattleContext(task.BattleSys!)!, BattleContextParamAITarget, task.Battler) + 1;
        if (target >= 5)
        {
            ErrorHandling.GF_AssertFail();
        }
        BattleIO.BattleIO_TargetSelectResult(task.BattleSys!, task.Battler, (uint)target);
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x022610EC
    private static void BattleDisplay_IgnoreTargetSelectTask(SysTask sysTask, object? param)
    {
        TargetSelectTask task = (TargetSelectTask)param!;
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x0226110C
    private static void BattleDisplay_RecordedTargetSelectTask(SysTask sysTask, object? param)
    {
        TargetSelectTask task = (TargetSelectTask)param!;
        if (BattleSystems.BattleSystem_ReadRecorded(task.BattleSys!, task.Battler, out byte data) == 1)
        {
            BattleDisplay_StopPlaybackDesync(task.BattleSys!);
        }
        if (data == 0 || data > 4)
        {
            BattleDisplay_StopPlaybackInvalid(task.BattleSys!);
        }
        else
        {
            int target = data - 1;
            if (task.Range == RangeSingleTarget)
            {
                if (task.Battler == target)
                {
                    BattleDisplay_StopPlaybackInvalid(task.BattleSys!);
                }
            }
            else if (task.Range == RangeAllyOrUser)
            {
                if (BattleSystems.BattleSystem_GetBattlerSide(task.BattleSys!, task.Battler) != BattleSystems.BattleSystem_GetBattlerSide(task.BattleSys!, target))
                {
                    BattleDisplay_StopPlaybackInvalid(task.BattleSys!);
                }
            }
        }
        BattleIO.BattleIO_TargetSelectResult(task.BattleSys!, task.Battler, data);
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }
}
