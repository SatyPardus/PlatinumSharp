using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class MoveSelectTaskData
{
    public BattleSystem? BattleSys;
    public HealthBox? Healthbox;
    public int Input;
    public readonly ushort[] Moves = new ushort[4];
    public readonly byte[] CurPP = new byte[4];
    public readonly byte[] MaxPP = new byte[4];
    public byte Command;
    public byte Battler;
    public byte BattlerType;
    public byte PartySlot;
    public byte State;
    public ushort InvalidMoves;
}

public static partial class Battlers
{
    // OFFSET 0x0225C288
    private static void Battler_CommandShowMoveSelectMenu(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_ShowMoveSelectMenu(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }
}

public static partial class BattleDisplay
{
    private const int MoveSelectMenuIndex = 0xB;
    private const int MoveSelectCancel = 0xFF;
    private const uint BattleTypeAIMoveSelect = 0x101;
    private const int StopRecordingInvalidInput = 1;
    private const int StopRecordingDesync = 2;
    private const ushort MaxMoveID = 0x1D3;
    private const uint BattleTypeLinkMask = 4;
    private const int LinkMessageOffset = 0x67;

    private static SysTaskFunc MoveSelectTaskFunc(int address)
    {
        return address switch
        {
            0x02260C00 => BattleDisplay_MoveSelectTask,
            0x02260DB0 => BattleDisplay_AIMoveSelectTask,
            0x02260E78 => BattleDisplay_IgnoreMoveSelectTask,
            0x02260E98 => BattleDisplay_RecordedMoveSelectTask,
            _ => throw Missing((uint)address),
        };
    }

    // OFFSET 0x0225D698
    public static void BattleDisplay_ShowMoveSelectMenu(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        MoveSelectTaskData task = new();
        task.State = 0;
        task.BattleSys = battleSys;
        task.Command = data[0];
        task.Battler = battler.Battler;
        task.BattlerType = battler.BattlerType;
        task.Healthbox = battler.Healthbox;
        task.PartySlot = data[1];
        for (int i = 0; i < 4; i++)
        {
            task.Moves[i] = (ushort)BattleIO.R16(data, 4 + i * 2);
            task.CurPP[i] = data[0xC + i];
            task.MaxPP[i] = data[0x10 + i];
        }
        task.InvalidMoves = (ushort)BattleIO.R16(data, 2);
        SysTask.SysTask_Start(MoveSelectTaskFunc(battler.TaskFuncs.ShowMoveSelectMenu), task, 0);
    }

    // OFFSET 0x02260BAC
    private static void BattleDisplay_RecordedCommandSelectionTask(SysTask sysTask, object? param)
    {
        CommandSelectionTaskData task = (CommandSelectionTaskData)param!;
        if (BattleSystems.BattleSystem_ReadRecorded(task.BattleSys!, task.Battler, out byte command) == 1)
        {
            BattleDisplay_StopPlaybackDesync(task.BattleSys!);
        }
        if (command == 0 || command > CommandRun || command == CommandBag)
        {
            BattleDisplay_StopPlaybackInvalid(task.BattleSys!);
        }
        BattleIO.BattleIO_CommandSelectionResult(task.BattleSys!, task.Battler, command);
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x02260C00
    private static void BattleDisplay_MoveSelectTask(SysTask sysTask, object? param)
    {
        MoveSelectTaskData task = (MoveSelectTaskData)param!;
        BattleSystems.BattleSystem_GetBgConfig(task.BattleSys!);
        BattleSubscreen subscreen = BattleSystems.BattleSystem_GetBattleSubscreen(task.BattleSys!)!;
        BattlerData battlerData = BattleSystems.BattleSystem_GetBattlerData(task.BattleSys!, task.Battler)!;
        int partner = BattleSystems.BattleSystem_GetPartner(task.BattleSys!, task.Battler);
        HealthBox? partnerHealthbox = partner != task.Battler ? BattleSystems.BattleSystem_GetHealthbox(task.BattleSys!, partner) : null;
        switch (task.State)
        {
            case 0:
            {
                if (BattleSubscreens.BattleSubscreen_IsMenuIdle(subscreen) == 0)
                {
                    return;
                }
                BattleMessage message = new();
                message.Tags = MessageTagsNickname;
                message.Params[0] = task.Battler | (task.PartySlot << 8);
                message.Id = MessageWhatWillDo;
                BattleMessages.BattleMessage_Print(task.BattleSys!, BattleSystems.BattleSystem_GetMessageLoader(task.BattleSys!)!, message, 0);
                BattleSubscreens.BattleSubscreen_HidePartyBalls(subscreen);
                NARC battleNarc = NARC.NARC_ctor(NarcBattleGraphicsDisplay, CommandTaskHeap);
                NARC subscreenNarc = NARC.NARC_ctor(NarcBattleSubscreenGraphics, CommandTaskHeap);
                MoveSelectData moves = new();
                for (int i = 0; i < 4; i++)
                {
                    moves.MoveIDs[i] = task.Moves[i];
                    moves.CurrPP[i] = task.CurPP[i];
                    moves.MaxPP[i] = task.MaxPP[i];
                }
                moves.BattlerType = task.BattlerType;
                BattleSubscreens.BattleSubscreen_LoadMenu(battleNarc, subscreenNarc, subscreen, MoveSelectMenuIndex, 0, moves);
                NARC.NARC_dtor(battleNarc);
                NARC.NARC_dtor(subscreenNarc);
                task.State++;
                return;
            }
            case 1:
                task.Input = BattleSubscreens.BattleSubscreen_CheckTouchInput(subscreen);
                if (task.Input != TouchScreen.ButtonNotPressed)
                {
                    SoundPlayback.Sound_PlayEffect(CommandSelectSfx);
                    task.State++;
                }
                return;
            case 2:
                if (task.Input != MoveSelectCancel && (BattleSystems.BattleSystem_GetBattleType(task.BattleSys!) & BattleSystems.BattleTypeDoubles) == 0)
                {
                    BattleSubscreens.BattleSubscreen_SetCommandMenuActive(subscreen, 0);
                    Healthboxes.Healthbox_OnPartnerCommandEnd(partnerHealthbox);
                    Healthboxes.Healthbox_StopArrowAnim(task.Healthbox!);
                    Battlers.Battler_StopIdleBob(battlerData);
                }
                BattleIO.BattleIO_MoveSelectResult(task.BattleSys!, task.Battler, (uint)task.Input);
                task.State++;
                return;
            case 3:
                task.State++;
                goto default;
            default:
                if (BattleSubscreens.BattleSubscreen_NoPendingTask(subscreen) == 1)
                {
                    BattleSystems.BattleSystem_SetCatchingTutorialLowHP(task.BattleSys!, 1);
                    BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
                    SysTask.SysTask_Done(sysTask);
                }
                return;
        }
    }

    // OFFSET 0x02260DB0
    private static void BattleDisplay_AIMoveSelectTask(SysTask sysTask, object? param)
    {
        MoveSelectTaskData task = (MoveSelectTaskData)param!;
        uint battleType = BattleSystems.BattleSystem_GetBattleType(task.BattleSys!);
        BattleContext battleCtx = BattleSystems.BattleSystem_GetBattleContext(task.BattleSys!)!;
        int input;
        if ((battleType & BattleTypeAIMoveSelect) != 0
            || (BattleSystems.BattleSystem_GetBattleStatusMask(task.BattleSys!) & BattleStatusFirstBattle) != 0
            || BattleSystems.BattleSystem_GetBattlerSide(task.BattleSys!, task.Battler) == 0)
        {
            int slot = TrainerAI.TrainerAI_Main(task.BattleSys!, task.Battler);
            if (slot == 0xFF)
            {
                return;
            }
            input = slot + 1;
        }
        else
        {
            int[] usable = new int[4];
            int count = 0;
            for (int i = 0; i < 4; i++)
            {
                if ((HealthBar.FlagIndex(i) & task.InvalidMoves) == 0)
                {
                    usable[count] = i + 1;
                    count++;
                }
            }
            input = usable[BattleSystems.BattleSystem_RandNext(task.BattleSys!) % count];
            int defender = BattleContexts.BattleSystem_Defender(task.BattleSys!, battleCtx, task.Battler, task.Moves[input - 1], 1, 0);
            BattleContexts.BattleContext_Set(task.BattleSys!, battleCtx, BattleContexts.ParamAISelectedTarget, task.Battler, defender);
        }
        BattleIO.BattleIO_MoveSelectResult(task.BattleSys!, task.Battler, (uint)input);
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x02260E78
    private static void BattleDisplay_IgnoreMoveSelectTask(SysTask sysTask, object? param)
    {
        MoveSelectTaskData task = (MoveSelectTaskData)param!;
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x02260E98
    private static void BattleDisplay_RecordedMoveSelectTask(SysTask sysTask, object? param)
    {
        MoveSelectTaskData task = (MoveSelectTaskData)param!;
        if (BattleSystems.BattleSystem_ReadRecorded(task.BattleSys!, task.Battler, out byte slot) == 1)
        {
            BattleDisplay_StopPlaybackDesync(task.BattleSys!);
        }
        if (slot == 0 || slot > 4)
        {
            BattleDisplay_StopPlaybackInvalid(task.BattleSys!);
        }
        else
        {
            ushort move = (ushort)BattleContexts.BattleMon_Get(BattleSystems.BattleSystem_GetBattleContext(task.BattleSys!)!, task.Battler, BattleContexts.BattleMonMove1 + slot - 1, null);
            if (move == 0 || move > MaxMoveID)
            {
                BattleDisplay_StopPlaybackInvalid(task.BattleSys!);
            }
        }
        BattleIO.BattleIO_MoveSelectResult(task.BattleSys!, task.Battler, slot);
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x02264730
    private static void BattleDisplay_StopPlaybackInvalid(BattleSystem battleSys)
    {
        BattleSystems.BattleSystem_SetStopRecording(battleSys, StopRecordingInvalidInput);
        BattleContexts.BattleSystem_CheckRecordingStopped(battleSys, BattleSystems.BattleSystem_GetBattleContext(battleSys)!);
    }

    // OFFSET 0x0226474C
    private static void BattleDisplay_StopPlaybackDesync(BattleSystem battleSys)
    {
        BattleSystems.BattleSystem_SetStopRecording(battleSys, StopRecordingDesync);
        BattleContexts.BattleSystem_CheckRecordingStopped(battleSys, BattleSystems.BattleSystem_GetBattleContext(battleSys)!);
    }

    // OFFSET 0x02264768
    public static byte BattleDisplay_AdjustLinkMessage(BattleSystem battleSys, int battler, byte message)
    {
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeLinkMask) != 0
            && TrainerInfo.TrainerInfo_GameCode(BattleSystems.BattleSystem_GetTrainerInfo(battleSys, battler)) == 0
            && message <= 1)
        {
            message = (byte)(message + LinkMessageOffset);
        }
        return message;
    }
}
