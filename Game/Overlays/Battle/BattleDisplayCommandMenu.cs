using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class CommandSelectionTaskData
{
    public BattleSystem? BattleSys;
    public HealthBox? Healthbox;
    public byte Command;
    public byte Battler;
    public byte State;
    public byte Unused0B;
    public int Input;
    public readonly byte[] PlayerBallStatuses = new byte[6];
    public readonly byte[] OpponentBallStatuses = new byte[6];
    public readonly byte[] ExpPercents = new byte[7];
    public byte PartySlot;
    public readonly ushort[] Moves = new ushort[4];
    public readonly byte[] CurPP = new byte[4];
    public readonly byte[] MaxPP = new byte[4];
    public byte BattlerType;
    public byte PrinterID;
    public short CurHP;
    public ushort MaxHP;
    public byte BallStatus;
    public byte SwitchingMask;
}

public static partial class Battlers
{
    private const uint TaskPriorityIdleBob = 0x3F2;
    private const ushort IdleBobStartDegrees = 180;
    private const ushort IdleBobStep = 20;
    private const ushort IdleBobFullCircle = 360;
    private const int IdleBobAmplitude = 0x1800;
    private const int MonSpriteAttrYOffset = 4;
    private const uint BattleTypeNoIdleBob = BattleSystems.BattleTypeSafari | BattleSystems.BattleTypePalPark;

    // OFFSET 0x0225C260
    private static void Battler_CommandSetCommandSelection(BattleSystem battleSys, BattlerData battler)
    {
        BattleSystems.BattleSystem_SetUnread2440(battleSys, battler.Data[0x29]);
        BattleDisplay.BattleDisplay_ShowCommandMenu(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x02263B0C
    public static SavedCursorPosition Battler_GetCursorMemory(BattlerData battler)
    {
        return battler.SavedCursorPos;
    }

    // OFFSET 0x02264798
    public static void Battler_StartIdleBob(BattlerData battler, BattleSystem battleSys)
    {
        if (battler.SysTask == null && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeNoIdleBob) == 0)
        {
            battler.Degrees = IdleBobStartDegrees;
            battler.SysTask = SysTask.SysTask_Start(Battler_IdleBobTask, battler, TaskPriorityIdleBob);
        }
    }

    // OFFSET 0x022647D8
    public static void Battler_StopIdleBob(BattlerData battler)
    {
        if (battler.SysTask != null)
        {
            SysTask.SysTask_Done(battler.SysTask);
            battler.SysTask = null;
            battler.Degrees = 0;
            PokemonSprite.PokemonSprite_SetAttribute(battler.MonSprite!, MonSpriteAttrYOffset, 0);
        }
    }

    // OFFSET 0x02264800
    private static void Battler_IdleBobTask(SysTask task, object? param)
    {
        BattlerData battler = (BattlerData)param!;
        battler.Degrees += IdleBobStep;
        if (battler.Degrees >= IdleBobFullCircle)
        {
            battler.Degrees -= IdleBobFullCircle;
        }
        int sine = MathUtil.CalcSineDegrees(battler.Degrees);
        int offset = (int)(((long)sine * IdleBobAmplitude + 0x800) >> 12);
        PokemonSprite.PokemonSprite_SetAttribute(battler.MonSprite!, MonSpriteAttrYOffset, offset / 4096);
    }
}

public static partial class BattleDisplay
{
    private const int CommandTaskHeap = 5;
    private const int NarcBattleGraphicsDisplay = 7;
    private const int NarcBattleSubscreenGraphics = 0x1B;
    private const int CommandMenuPlayer = 1;
    private const int CommandMenuPartner = 2;
    private const int CommandMenuPlayerReady = 3;
    private const int CommandMenuPartnerReady = 4;
    private const int CommandMenuFirstBattle = 5;
    private const int CommandMenuFirstBattleReady = 6;
    private const int CommandMenuSafari = 7;
    private const int CommandMenuSafariReady = 8;
    private const int CommandMenuPalPark = 9;
    private const int CommandMenuPalParkReady = 10;
    private const int CommandMenuNone = 0;
    private const uint BattleStatusFirstBattle = 1;
    private const int PadButtonSelect = 0x0004;
    private const int CommandSelectSfx = 0x5DD;
    private const int CommandFight = 1;
    private const int CommandBag = 2;
    private const int CommandPokemon = 3;
    private const int CommandRun = 4;
    private const int CommandCancel = 0xFF;
    private const int MessageWhatWillDo = 0x399;
    private const int MessageSafariWhatWillDo = 0x39A;
    private const int MessagePalParkWhatWillDo = 0x4C6;
    private const int MessageCatchTutorialLowHP = 0x4CA;
    private const byte MessageTagsNickname = 2;
    private const byte MessageTagsNone = 0;
    private const byte MessageTagsTrainer = 8;
    private const int BallStatusFainted = 2;
    private const int BattlerTypePlayerSlot2 = 4;

    private static SysTaskFunc BattlerTaskFunc(int address)
    {
        return address switch
        {
            0x022604C8 => BattleDisplay_CommandSelectionTask,
            0x02260AB4 => BattleDisplay_AICommandSelectionTask,
            0x02260BAC => BattleDisplay_RecordedCommandSelectionTask,
            0x02260AE4 => BattleDisplay_IgnoreCommandTask,
            0x02260B04 => BattleDisplay_SafariEnemyCommandTask,
            0x02260B84 => BattleDisplay_FrontierEnemyCommandTask,
            _ => throw Missing((uint)address),
        };
    }

    // OFFSET 0x0225D5B8
    public static void BattleDisplay_ShowCommandMenu(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        CommandSelectionTaskData task = new();
        task.State = 0;
        task.Unused0B = 0;
        task.BattleSys = battleSys;
        task.Command = data[0];
        task.Battler = battler.Battler;
        task.BattlerType = battler.BattlerType;
        task.Healthbox = battler.Healthbox;
        task.PartySlot = data[1];
        task.CurHP = (short)BattleIO.R16(data, 0x24);
        task.MaxHP = (ushort)BattleIO.R16(data, 0x26);
        task.BallStatus = data[0x28];
        task.SwitchingMask = data[0x29];
        for (int i = 0; i < 6; i++)
        {
            task.PlayerBallStatuses[i] = data[8 + i];
            task.OpponentBallStatuses[i] = data[0xE + i];
        }
        for (int i = 0; i < 6; i++)
        {
            if (data[8 + i] == BallStatusFainted)
            {
                task.ExpPercents[i] = 0;
            }
            else
            {
                task.ExpPercents[i] = data[2 + i];
            }
        }
        for (int i = 0; i < 4; i++)
        {
            task.Moves[i] = (ushort)BattleIO.R16(data, 0x14 + i * 2);
            task.CurPP[i] = data[0x1C + i];
            task.MaxPP[i] = data[0x20 + i];
        }
        SysTask.SysTask_Start(BattlerTaskFunc(battler.TaskFuncs.SetCommandSelection), task, 0);
    }

    private static void BattleDisplay_LoadCommandMenu(CommandSelectionTaskData task, BattleSubscreen subscreen, int menuIndex, object? menuData)
    {
        NARC battleNarc = NARC.NARC_ctor(NarcBattleGraphicsDisplay, CommandTaskHeap);
        NARC subscreenNarc = NARC.NARC_ctor(NarcBattleSubscreenGraphics, CommandTaskHeap);
        BattleSubscreens.BattleSubscreen_LoadMenu(battleNarc, subscreenNarc, subscreen, menuIndex, 0, menuData);
        NARC.NARC_dtor(battleNarc);
        NARC.NARC_dtor(subscreenNarc);
    }

    // OFFSET 0x022604C8
    private static void BattleDisplay_CommandSelectionTask(SysTask sysTask, object? param)
    {
        CommandSelectionTaskData task = (CommandSelectionTaskData)param!;
        BattleSystems.BattleSystem_GetBgConfig(task.BattleSys!);
        BattleSubscreen subscreen = BattleSystems.BattleSystem_GetBattleSubscreen(task.BattleSys!)!;
        BattlerData battlerData = BattleSystems.BattleSystem_GetBattlerData(task.BattleSys!, task.Battler)!;
        uint battleType = BattleSystems.BattleSystem_GetBattleType(task.BattleSys!);
        HealthBox? partnerHealthbox;
        int partner = BattleSystems.BattleSystem_GetPartner(task.BattleSys!, task.Battler);
        if (partner != task.Battler)
        {
            partnerHealthbox = BattleSystems.BattleSystem_GetHealthbox(task.BattleSys!, partner);
        }
        else
        {
            partnerHealthbox = null;
        }
        switch (task.State)
        {
            case 0:
            {
                MoveDisplayInfo info = new();
                for (int i = 0; i < 4; i++)
                {
                    info.Move[i] = task.Moves[i];
                    info.CurPP[i] = task.CurPP[i];
                    info.MaxPP[i] = task.MaxPP[i];
                }
                BattleSubscreens.BattleSubscreen_SetMoveDisplayInfo(BattleSystems.BattleSystem_GetBattleSubscreen(task.BattleSys!)!, task.BattlerType, info);
                task.State++;
                goto case 1;
            }
            case 1:
            {
                MessageLoader loader = BattleSystems.BattleSystem_GetMessageLoader(task.BattleSys!)!;
                BattleMessage message = new();
                if ((battleType & BattleSystems.BattleTypeCatchTutorial) != 0)
                {
                    switch (BattleSystems.BattleSystem_GetCatchingTutorialLowHP(task.BattleSys!))
                    {
                        case 0:
                            message.Tags = MessageTagsNickname;
                            message.Params[0] = task.Battler | (task.PartySlot << 8);
                            message.Id = MessageWhatWillDo;
                            BattleMessages.BattleMessage_Print(task.BattleSys!, loader, message, 0);
                            task.State = 3;
                            return;
                        case 1:
                            message.Tags = MessageTagsNone;
                            message.Id = (ushort)(BattleSystems.BattleSystem_GetTrainerGender(task.BattleSys!, 0) + MessageCatchTutorialLowHP);
                            task.PrinterID = BattleMessages.BattleMessage_Print(task.BattleSys!, loader, message, BattleMessages.BattleSystem_GetTextSpeed(task.BattleSys!));
                            task.State = 2;
                            return;
                        default:
                            return;
                    }
                }
                if ((battleType & BattleSystems.BattleTypeSafari) != 0)
                {
                    message.Tags = MessageTagsTrainer;
                    message.Params[0] = BattleSystems.BattleSystem_GetTrainerID(task.BattleSys!, task.Battler);
                    message.Id = MessageSafariWhatWillDo;
                }
                else if ((battleType & BattleSystems.BattleTypePalPark) != 0)
                {
                    message.Tags = MessageTagsTrainer;
                    message.Params[0] = BattleSystems.BattleSystem_GetTrainerID(task.BattleSys!, task.Battler);
                    message.Id = MessagePalParkWhatWillDo;
                }
                else
                {
                    message.Tags = MessageTagsNickname;
                    message.Params[0] = task.Battler | (task.PartySlot << 8);
                    message.Id = MessageWhatWillDo;
                }
                BattleMessages.BattleMessage_Print(task.BattleSys!, loader, message, 0);
                task.State = 3;
                return;
            }
            case 2:
                if (Text.Text_IsPrinterActive(task.PrinterID) == 0)
                {
                    task.State = 3;
                }
                return;
            case 3:
                Healthboxes.Healthbox_StartArrowAnim(task.Healthbox!);
                Battlers.Battler_StartIdleBob(battlerData, task.BattleSys!);
                task.State = 4;
                goto case 4;
            case 4:
            {
                if (BattleSubscreens.BattleSubscreen_IsMenuIdle(subscreen) == 0)
                {
                    return;
                }
                NARC battleNarc = NARC.NARC_ctor(NarcBattleGraphicsDisplay, CommandTaskHeap);
                NARC subscreenNarc = NARC.NARC_ctor(NarcBattleSubscreenGraphics, CommandTaskHeap);
                ActionSelectData action = new();
                action.Battler = task.Battler;
                action.BattlerType = task.BattlerType;
                action.PartySlot = task.PartySlot;
                action.CurHp = task.CurHP;
                action.MaxHp = task.MaxHP;
                action.BallStatusBattler = task.BallStatus;
                action.IsWaitingForPartner = BattleSystems.BattleSystem_IsWaitingForPartner(task.BattleSys!, task.Battler, task.SwitchingMask);
                int menu;
                if (battlerData.CommandUIReady != 0)
                {
                    BattleSubscreens.BattleSubscreen_SetCommandMenuActive(subscreen, 1);
                    if ((BattleSystems.BattleSystem_GetBattleStatusMask(task.BattleSys!) & BattleStatusFirstBattle) != 0)
                    {
                        menu = CommandMenuFirstBattleReady;
                    }
                    else if ((BattleSystems.BattleSystem_GetBattleType(task.BattleSys!) & BattleSystems.BattleTypePalPark) != 0)
                    {
                        menu = CommandMenuPalParkReady;
                    }
                    else if ((BattleSystems.BattleSystem_GetBattleType(task.BattleSys!) & BattleSystems.BattleTypeSafari) != 0)
                    {
                        menu = CommandMenuSafariReady;
                    }
                    else if (action.BattlerType == BattlerTypePlayerSlot2 && (BattleSystems.BattleSystem_GetBattleType(task.BattleSys!) & BattleSystems.BattleTypeMulti) == 0)
                    {
                        menu = CommandMenuPartnerReady;
                    }
                    else
                    {
                        menu = CommandMenuPlayerReady;
                    }
                    BattleSubscreens.BattleSubscreen_LoadMenu(battleNarc, subscreenNarc, subscreen, menu, 0, action);
                    BattleSystems.BattleSystem_SetCommandSelectionFlags(task.BattleSys!, 1);
                }
                else
                {
                    if ((BattleSystems.BattleSystem_GetBattleStatusMask(task.BattleSys!) & BattleStatusFirstBattle) != 0)
                    {
                        menu = CommandMenuFirstBattle;
                    }
                    else if ((BattleSystems.BattleSystem_GetBattleType(task.BattleSys!) & BattleSystems.BattleTypePalPark) != 0)
                    {
                        menu = CommandMenuPalPark;
                    }
                    else if ((BattleSystems.BattleSystem_GetBattleType(task.BattleSys!) & BattleSystems.BattleTypeSafari) != 0)
                    {
                        menu = CommandMenuSafari;
                    }
                    else if (action.BattlerType != BattlerTypePlayerSlot2)
                    {
                        menu = CommandMenuPlayer;
                    }
                    else
                    {
                        menu = CommandMenuPartner;
                    }
                    BattleSubscreens.BattleSubscreen_LoadMenu(battleNarc, subscreenNarc, subscreen, menu, 0, action);
                    battlerData.CommandUIReady = 1;
                }
                BattleSubscreens.BattleSubscreen_SetPartyBallExpPercents(subscreen, task.ExpPercents);
                BattleSubscreens.BattleSubscreen_SetPartyBallStatuses(subscreen, task.PlayerBallStatuses, task.OpponentBallStatuses);
                BattleSubscreens.BattleSubscreen_ShowPartyBalls(subscreen);
                NARC.NARC_dtor(battleNarc);
                NARC.NARC_dtor(subscreenNarc);
                if (partnerHealthbox != null)
                {
                    Healthboxes.Healthbox_OnPartnerCommandStart(partnerHealthbox);
                }
                task.State = 5;
                return;
            }
            case 5:
                if ((Globals.gSystem.PressedKeysRepeatable & PadButtonSelect) != 0)
                {
                    for (int i = 0; i < BattleSystems.BattleSystem_GetMaxBattlers(task.BattleSys!); i++)
                    {
                        Healthboxes.Healthbox_ToggleNumberMode(BattleSystems.BattleSystem_GetBattlerData(task.BattleSys!, i)!.Healthbox);
                    }
                }
                task.Input = BattleSubscreens.BattleSubscreen_CheckTouchInput(subscreen);
                if (task.Input != TouchScreen.ButtonNotPressed)
                {
                    task.Unused0B = 10;
                    SoundPlayback.Sound_PlayEffect(CommandSelectSfx);
                    task.State = 6;
                }
                return;
            case 6:
                if (BattleSubscreens.BattleSubscreen_IsMenuIdle(subscreen) != 1 && task.Input != CommandFight)
                {
                    return;
                }
                switch (task.Input)
                {
                    case CommandFight:
                        if ((BattleSystems.BattleSystem_GetBattleType(task.BattleSys!) & (BattleSystems.BattleTypeSafari | BattleSystems.BattleTypePalPark)) != 0)
                        {
                            task.State = 7;
                        }
                        break;
                    case CommandBag:
                    case CommandPokemon:
                        BattleDisplay_LoadCommandMenu(task, subscreen, CommandMenuNone, null);
                        BattleSubscreens.BattleSubscreen_SetCommandMenuActive(subscreen, 0);
                        Healthboxes.Healthbox_OnPartnerCommandEnd(partnerHealthbox);
                        Healthboxes.Healthbox_StopArrowAnim(task.Healthbox!);
                        Battlers.Battler_StopIdleBob(battlerData);
                        break;
                    case CommandRun:
                    {
                        NARC battleNarc = NARC.NARC_ctor(NarcBattleGraphicsDisplay, CommandTaskHeap);
                        NARC subscreenNarc = NARC.NARC_ctor(NarcBattleSubscreenGraphics, CommandTaskHeap);
                        if (BattleSubscreens.BattleSubscreen_IsWaitingForPartner(subscreen) == 1)
                        {
                            Healthboxes.Healthbox_OnPartnerCommandEnd(partnerHealthbox);
                            task.Input = CommandCancel;
                        }
                        BattleSubscreens.BattleSubscreen_LoadMenu(battleNarc, subscreenNarc, subscreen, CommandMenuNone, 0, null);
                        NARC.NARC_dtor(battleNarc);
                        NARC.NARC_dtor(subscreenNarc);
                        break;
                    }
                    default:
                        ErrorHandling.GF_AssertFail();
                        break;
                }
                BattleSubscreens.BattleSubscreen_HidePartyBalls(subscreen);
                task.State = 8;
                return;
            case 7:
                if (BattleSubscreens.BattleSubscreen_IsMenuIdle(subscreen) == 1)
                {
                    BattleDisplay_LoadCommandMenu(task, subscreen, CommandMenuNone, null);
                    BattleSubscreens.BattleSubscreen_SetCommandMenuActive(subscreen, 0);
                    Healthboxes.Healthbox_OnPartnerCommandEnd(partnerHealthbox);
                    Healthboxes.Healthbox_StopArrowAnim(task.Healthbox!);
                    Battlers.Battler_StopIdleBob(battlerData);
                    BattleSubscreens.BattleSubscreen_HidePartyBalls(subscreen);
                    task.State = 8;
                }
                return;
            case 8:
                if (BattleSubscreens.BattleSubscreen_NoPendingTask(subscreen) == 1)
                {
                    BattleIO.BattleIO_CommandSelectionResult(task.BattleSys!, task.Battler, (uint)task.Input);
                    BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
                    SysTask.SysTask_Done(sysTask);
                }
                return;
        }
    }
}

public static partial class BattleSystems
{
    private const int BattlerSlotPlayerSecond = 4;
    private const int PartnerActionCommand = 0xE;
    private const int PartnerActionMaxMove = 0x10;

    // OFFSET 0x0223EC58
    public static byte BattleSystem_IsWaitingForPartner(BattleSystem battleSys, int battler, int switchingMask)
    {
        if (BattleSystem_BattlerSlot(battleSys, battler) == BattlerSlotPlayerSecond && (battleSys.BattleType & BattleTypeMulti) == 0)
        {
            if ((battleSys.BattleType & BattleTypeLink) != 0)
            {
                if ((HealthBar.FlagIndex(BattleSystem_GetPartner(battleSys, battler)) & switchingMask) == 0)
                {
                    return 1;
                }
                return 0;
            }
            int partnerMove = (ushort)BattleContexts.BattleContext_Get(battleSys, battleSys.BattleCtx!, BattleContexts.ParamActionTemp2, 0);
            if (BattleContexts.BattleContext_Get(battleSys, battleSys.BattleCtx!, BattleContexts.ParamActionTemp, 0) == PartnerActionCommand && partnerMove > PartnerActionMaxMove)
            {
                return 0;
            }
            if ((HealthBar.FlagIndex(0) & switchingMask) != 0)
            {
                return 0;
            }
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0223F35C
    public static HealthBox BattleSystem_GetHealthbox(BattleSystem battleSys, int battler)
    {
        return Battlers.Battler_GetHealthbox(battleSys.Battlers[battler]!);
    }

    // OFFSET 0x0223F858
    public static void BattleSystem_GetBattlersByType(BattleSystem battleSys, byte[] battlers)
    {
        for (int i = 0; i < battleSys.MaxBattlers; i++)
        {
            battlers[Battlers.Battler_GetType(battleSys.Battlers[i]!)] = (byte)i;
        }
    }
}
