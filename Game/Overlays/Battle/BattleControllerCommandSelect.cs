using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class Battle
{
    private const int OverlayTrainerAI = 14;

    // OFFSET 0x0223B748
    public static void Battle_SetOverlay(BattleSystem battleSys, int flag)
    {
        if (battleSys.OverlayFlags == flag)
        {
            ErrorHandling.GF_AssertFail();
        }
        battleSys.OverlayFlags = (byte)flag;
        if (flag == 0)
        {
            GameOverlay.Overlay_UnloadByID(OverlayTrainerAI);
            GameOverlay.Overlay_LoadByID(OverlayBattleAnim, OverlayLoadAsync);
        }
        else
        {
            GameOverlay.Overlay_UnloadByID(OverlayBattleAnim);
            GameOverlay.Overlay_LoadByID(OverlayTrainerAI, OverlayLoadAsync);
        }
    }
}

public static partial class BattleSystems
{
    private const uint BattleStatusPlayback = 0x10;
    private const int RecordingBufferSize = 0x400;
    private const int ContextParamCommand = 0xD;
    private const int ContextParamCommandNext = 0xE;
    private const int ControllerCommandRecordingStopped = 0x2B;
    private const int HeapIDBattleSystem = 5;

    // OFFSET 0x0223F500
    public static void BattleSystem_Record(BattleSystem battleSys, int battler, byte data)
    {
        if ((battleSys.BattleStatusMask & BattleStatusPlayback) == 0 && battleSys.RecordingAckPos[battler] < RecordingBufferSize)
        {
            BattleRecording.BattleRecording_WriteCommand(battler, battleSys.RecordingAckPos[battler], data);
            battleSys.RecordingAckPos[battler]++;
        }
    }

    // OFFSET 0x0223F530
    public static int BattleSystem_ReadRecorded(BattleSystem battleSys, int battler, out byte data)
    {
        int result = 0;
        data = 0xFF;
        if ((battleSys.BattleStatusMask & BattleStatusPlayback) != 0)
        {
            if (battleSys.RecordingAckPos[battler] < RecordingBufferSize)
            {
                data = BattleRecording.BattleRecording_ReadCommand(battler, battleSys.RecordingAckPos[battler]);
                battleSys.RecordingAckPos[battler]++;
            }
            else
            {
                result = 1;
            }
        }
        return result;
    }

    // OFFSET 0x0223F720
    public static void BattleSystem_SetStopRecording(BattleSystem battleSys, int flag)
    {
        if ((battleSys.BattleStatusMask & BattleStatusPlayback) == 0 || (battleSys.RecordingStopped & 1) != 0)
        {
            return;
        }
        if (BattleContexts.BattleContext_Get(battleSys, battleSys.BattleCtx!, ContextParamCommand, 0) == ControllerCommandRecordingStopped)
        {
            return;
        }
        if (BattleContexts.BattleContext_Get(battleSys, battleSys.BattleCtx!, ContextParamCommandNext, 0) == ControllerCommandRecordingStopped)
        {
            return;
        }
        BattleSubscreens.BattleSubscreen_ShowRecordingStoppedMessage(battleSys.BtlSubscreen!, flag);
        ScreenFadeControl.StartScreenFade(3, 0, 0, 0, 0x10, 2, 5);
        SoundPlayback.Sound_StopWaveOutAndSequences();
        Sound.Sound_SetMasterVolume(0);
        battleSys.RecordingStopped = (battleSys.RecordingStopped & ~1) | 1;
    }

    // OFFSET 0x0223F7A4
    public static int BattleSystem_CanStopPlayback(BattleSystem battleSys)
    {
        if ((battleSys.BattleStatusMask & BattleStatusPlayback) == 0 || (battleSys.RecordingStopped & 1) != 0)
        {
            return 0;
        }
        if (BattleContexts.BattleContext_Get(battleSys, battleSys.BattleCtx!, ContextParamCommand, 0) == ControllerCommandRecordingStopped)
        {
            return 0;
        }
        if (BattleContexts.BattleContext_Get(battleSys, battleSys.BattleCtx!, ContextParamCommandNext, 0) == ControllerCommandRecordingStopped)
        {
            return 0;
        }
        return 1;
    }

    // OFFSET 0x0223F7E8
    public static void BattleSystem_ShowStopPlaybackButton(BattleSystem battleSys)
    {
        if ((battleSys.BattleStatusMask & BattleStatusPlayback) != 0 && battleSys.PlaybackStopButton == null)
        {
            battleSys.PlaybackStopButton = BattleSubscreens.BattleSubscreen_StartPlaybackStopButton(battleSys);
        }
    }
}

public static partial class BattleControllers
{
    private const int CommandBeforeTurn = 6;
    private const int SelectStateInit = 0;
    private const int SelectStateCommandInput = 1;
    private const int SelectStateWaitForOthers = 2;
    private const int SelectStateMoveInit = 3;
    private const int SelectStateMoveInput = 4;
    private const int SelectStateTargetInit = 5;
    private const int SelectStateTargetInput = 6;
    private const int SelectStateItemInit = 7;
    private const int SelectStateItemInput = 8;
    private const int SelectStatePartyInit = 9;
    private const int SelectStatePartyInput = 0xA;
    private const int SelectStateRunInit = 0xB;
    private const int SelectStateRunInput = 0xC;
    private const int SelectStateEnd = 0xD;
    private const int SelectStateWait = 0xE;
    private const int SelectStateAlertMessageWait = 0xF;
    private const int SelectStateStruggleMessage = 0x10;
    private const int SelectStateClearTouchScreen = 0x11;
    private const int ActionNone = 0xD;
    private const int ActionItem = 0xE;
    private const int ActionParty = 0xF;
    private const int ActionRun = 0x10;
    private const int ActionSafariBall = 0x11;
    private const int ActionSafariBait = 0x12;
    private const int ActionSafariRock = 0x13;
    private const int ActionSafariWatch = 0x14;
    private const int ActionSwitchedIn = 0x27;
    private const int InputFight = 1;
    private const int InputItem = 2;
    private const int InputParty = 3;
    private const int InputRun = 4;
    private const int InputCancel = 0xFF;
    private const int MoveMenuCancel = 4;
    private const int AllMovesInvalid = 0xF;
    private const uint SelectBattleTypeTrainer = 0x1;
    private const uint SelectBattleTypeDoubles = 0x2;
    private const uint SelectBattleTypeLink = 0x4;
    private const uint SelectBattleTypeMulti = 0x8;
    private const uint SelectBattleTypeSafari = 0x20;
    private const uint SelectBattleTypeFrontier = 0x80;
    private const uint SelectBattleTypePalPark = 0x200;
    private const uint SelectBattleTypeNoItems = 0x84;
    private const uint SelectBattleStatusPlayback = 0x10;
    private const byte RecordedFlagCommand = 1;
    private const byte RecordedFlagMove = 2;
    private const byte RecordedFlagTarget = 4;
    private const int SubscriptTrainerMessage = 0x29;
    private const int MessageItemsNotAllowed = 0x251;
    private const int MessageNoRunningFromTrainer = 0x319;
    private const int MessageNoMovesLeft = 0x260;
    private const int MessageForfeitPrompt = 0x3BB;
    private const int NoDoubleSwitchSlot = 6;
    private const int MoveCurseID = 0xAE;
    private const int RangeCurseUser = 0x10;
    private const int RangeAllyOnly = 0x100;
    private const int RangeSelfTargetMask = 0x251;
    private const int BootStatePlayer = 1;

    // OFFSET 0x0224BA78
    private static void BattleController_TrainerMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        if (BattleContexts.BattleSystem_CheckTrainerMessage(battleSys, battleCtx) != 0)
        {
            BattleContexts.BattleSystem_LoadScript(battleCtx, 1, SubscriptTrainerMessage);
            battleCtx.Command = CommandRunScript;
            battleCtx.CommandNext = CommandShowBattleMon;
        }
        else
        {
            battleCtx.Command = CommandShowBattleMon;
        }
        BattleContexts.BattleSystem_SortMonSpeedOrder(battleSys, battleCtx);
    }

    // OFFSET 0x0224BAA8
    private static void BattleController_ShowBattleMon(BattleSystem battleSys, BattleContext battleCtx)
    {
        int script = BattleContexts.BattleSystem_TriggerEffectOnSwitch(battleSys, battleCtx);
        if (script != 0)
        {
            BattleContexts.BattleSystem_LoadScript(battleCtx, 1, script);
            battleCtx.CommandNext = battleCtx.Command;
            battleCtx.Command = CommandRunScript;
            return;
        }
        BattleContexts.BattleSystem_SortMonSpeedOrder(battleSys, battleCtx);
        BattleSystems.BattleSystem_ShowStopPlaybackButton(battleSys);
        battleCtx.Command = CommandInitCommandSelection;
    }

    // OFFSET 0x0224BADC
    private static void BattleController_InitCommandSelection(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        for (int i = 0; i < maxBattlers; i++)
        {
            battleCtx.CurCommandState[i] = SelectStateInit;
            battleCtx.BattleMons[i].MoveEffectsTemp = battleCtx.BattleMons[i].MoveEffectsMask;
            battleCtx.RecordedCommandFlags[i] = 0;
        }
        BattleSystems.BattleSystem_SetCommandSelectionFlags(battleSys, 0);
        Battle.Battle_SetOverlay(battleSys, 1);
        battleCtx.Command = CommandCommandSelectionInput;
    }

    private static void BattleController_StopForInvalidRecording(BattleSystem battleSys)
    {
        BattleSystems.BattleSystem_SetStopRecording(battleSys, 1);
        BattleContexts.BattleSystem_CheckRecordingStopped(battleSys, BattleSystems.BattleSystem_GetBattleContext(battleSys)!);
    }

    // OFFSET 0x0224BB28
    private static void BattleController_CommandSelectionInput(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int doneCount = 0;
        BattleMessage msg = new();
        for (int i = 0; i < maxBattlers; i++)
        {
            switch (battleCtx.CurCommandState[i])
            {
                case SelectStateInit:
                    if ((battleType & SelectBattleTypeDoubles) != 0 && (battleType & SelectBattleTypeMulti) == 0
                        && ((i == 2 && battleCtx.CurCommandState[0] != SelectStateWait) || (i == 3 && battleCtx.CurCommandState[1] != SelectStateWait)))
                    {
                        break;
                    }
                    if ((HealthBar.FlagIndex(i) & battleCtx.BattlersSwitchingMask) != 0)
                    {
                        battleCtx.CurCommandState[i] = SelectStateEnd;
                        battleCtx.BattlerActions[i, 0] = ActionSwitchedIn;
                        break;
                    }
                    if (BattleContexts.Battler_CanPickCommand(battleCtx, i) == 0)
                    {
                        battleCtx.TurnFlags[i].PpDecremented = 1;
                        battleCtx.CurCommandState[i] = SelectStateEnd;
                        battleCtx.BattlerActions[i, 0] = ActionNone;
                        break;
                    }
                    if (Battlers.Battler_GetBootState(BattleSystems.BattleSystem_GetBattlerData(battleSys, i)!) == BootStatePlayer || battleCtx.TotalTurns != 0)
                    {
                        BattleIO.BattleIO_SetCommandSelection(battleSys, battleCtx, i, battleCtx.SelectedPartySlot[i]);
                        battleCtx.CurCommandState[i] = SelectStateCommandInput;
                    }
                    else
                    {
                        battleCtx.CurCommandState[i] = SelectStateWaitForOthers;
                    }
                    break;
                case SelectStateWaitForOthers:
                {
                    int j;
                    for (j = 0; j < maxBattlers; j++)
                    {
                        if (j == i)
                        {
                            continue;
                        }
                        if (Battlers.Battler_GetBootState(BattleSystems.BattleSystem_GetBattlerData(battleSys, j)!) != BootStatePlayer)
                        {
                            continue;
                        }
                        if (battleCtx.CurCommandState[j] != SelectStateWait)
                        {
                            break;
                        }
                    }
                    if (j != maxBattlers)
                    {
                        break;
                    }
                    BattleIO.BattleIO_SetCommandSelection(battleSys, battleCtx, i, battleCtx.SelectedPartySlot[i]);
                    battleCtx.CurCommandState[i] = SelectStateCommandInput;
                    goto case SelectStateCommandInput;
                }
                case SelectStateCommandInput:
                {
                    int input = BattleContexts.BattleContext_IOBufferVal(battleCtx, i);
                    if (input == 0)
                    {
                        break;
                    }
                    battleCtx.BattlerActions[i, 3] = battleCtx.IoBuffer[i, 0];
                    if ((battleType & SelectBattleTypePalPark) != 0)
                    {
                        switch (input)
                        {
                            case 1:
                                battleCtx.CurCommandState[i] = SelectStateClearTouchScreen;
                                battleCtx.NextCommandState[i] = SelectStateEnd;
                                battleCtx.BattlerActions[i, 0] = ActionSafariBall;
                                break;
                            case 4:
                                battleCtx.CurCommandState[i] = SelectStateClearTouchScreen;
                                battleCtx.NextCommandState[i] = SelectStateEnd;
                                battleCtx.BattlerActions[i, 0] = ActionRun;
                                break;
                        }
                        break;
                    }
                    if ((battleType & SelectBattleTypeSafari) != 0)
                    {
                        switch (input)
                        {
                            case 1:
                                battleCtx.CurCommandState[i] = SelectStateClearTouchScreen;
                                battleCtx.NextCommandState[i] = SelectStateEnd;
                                battleCtx.BattlerActions[i, 0] = ActionSafariBall;
                                break;
                            case 2:
                                battleCtx.CurCommandState[i] = SelectStateClearTouchScreen;
                                battleCtx.NextCommandState[i] = SelectStateEnd;
                                battleCtx.BattlerActions[i, 0] = ActionSafariBait;
                                break;
                            case 3:
                                battleCtx.CurCommandState[i] = SelectStateClearTouchScreen;
                                battleCtx.NextCommandState[i] = SelectStateEnd;
                                battleCtx.BattlerActions[i, 0] = ActionSafariRock;
                                break;
                            case 4:
                                battleCtx.CurCommandState[i] = SelectStateClearTouchScreen;
                                battleCtx.NextCommandState[i] = SelectStateEnd;
                                battleCtx.BattlerActions[i, 0] = ActionRun;
                                break;
                            case 5:
                                battleCtx.CurCommandState[i] = SelectStateEnd;
                                battleCtx.BattlerActions[i, 0] = ActionSafariWatch;
                                break;
                        }
                        break;
                    }
                    if (input != InputCancel)
                    {
                        battleCtx.RecordedCommandFlags[i] |= RecordedFlagCommand;
                    }
                    switch (BattleContexts.BattleContext_IOBufferVal(battleCtx, i))
                    {
                        case InputFight:
                            if (BattleContexts.BattleSystem_CheckInvalidMoves(battleSys, battleCtx, i, 0, -1) == AllMovesInvalid)
                            {
                                battleCtx.TurnFlags[i].Struggling = 1;
                                if ((BattleSystems.BattleSystem_GetBattleStatusMask(battleSys) & SelectBattleStatusPlayback) != 0)
                                {
                                    battleCtx.CurCommandState[i] = SelectStateEnd;
                                }
                                else
                                {
                                    battleCtx.CurCommandState[i] = SelectStateClearTouchScreen;
                                    battleCtx.NextCommandState[i] = SelectStateStruggleMessage;
                                }
                            }
                            else if (battleCtx.BattleMons[i].MoveEffectsData.EncoredMove != 0)
                            {
                                battleCtx.MoveSlot[i] = battleCtx.BattleMons[i].MoveEffectsData.EncoredMoveSlot;
                                battleCtx.MoveSelected[i] = battleCtx.BattleMons[i].MoveEffectsData.EncoredMove;
                                battleCtx.BattlerActions[i, 2] = 0;
                                if ((BattleSystems.BattleSystem_GetBattleStatusMask(battleSys) & SelectBattleStatusPlayback) != 0)
                                {
                                    battleCtx.CurCommandState[i] = SelectStateEnd;
                                }
                                else
                                {
                                    battleCtx.CurCommandState[i] = SelectStateClearTouchScreen;
                                    battleCtx.NextCommandState[i] = SelectStateEnd;
                                }
                            }
                            else
                            {
                                battleCtx.CurCommandState[i] = SelectStateMoveInit;
                            }
                            battleCtx.BattlerActions[i, 0] = ActionNone;
                            break;
                        case InputItem:
                            if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & SelectBattleTypeNoItems) != 0)
                            {
                                msg.Tags = 0;
                                msg.Id = MessageItemsNotAllowed;
                                BattleIO.BattleIO_SetAlertMessage(battleSys, i, msg);
                                battleCtx.CurCommandState[i] = SelectStateAlertMessageWait;
                                battleCtx.NextCommandState[i] = SelectStateInit;
                            }
                            else
                            {
                                battleCtx.BattlerActions[i, 0] = ActionItem;
                                battleCtx.CurCommandState[i] = SelectStateItemInit;
                            }
                            break;
                        case InputParty:
                            battleCtx.BattlerActions[i, 0] = ActionParty;
                            battleCtx.CurCommandState[i] = SelectStatePartyInit;
                            break;
                        case InputRun:
                            battleCtx.BattlerActions[i, 0] = ActionRun;
                            battleCtx.CurCommandState[i] = SelectStateRunInit;
                            break;
                        case InputCancel:
                            if ((battleType & SelectBattleTypeLink) != 0)
                            {
                                BattleIO.BattleIO_StopHealthboxArrow(battleSys, i);
                                battleCtx.CurCommandState[i] = SelectStateInit;
                                battleCtx.CurCommandState[BattleSystems.BattleSystem_GetPartner(battleSys, i)] = SelectStateInit;
                            }
                            else if ((battleType & SelectBattleTypeDoubles) != 0 && i == 2)
                            {
                                BattleIO.BattleIO_StopHealthboxArrow(battleSys, i);
                                battleCtx.CurCommandState[0] = SelectStateInit;
                                battleCtx.CurCommandState[2] = SelectStateInit;
                            }
                            break;
                    }
                    break;
                }
                case SelectStateMoveInit:
                    BattleIO.BattleIO_ShowMoveSelectMenu(battleSys, battleCtx, i);
                    battleCtx.CurCommandState[i] = SelectStateMoveInput;
                    goto case SelectStateMoveInput;
                case SelectStateMoveInput:
                    if (BattleContexts.BattleContext_IOBufferVal(battleCtx, i) == InputCancel)
                    {
                        battleCtx.CurCommandState[i] = SelectStateInit;
                        break;
                    }
                    if (BattleContexts.BattleContext_IOBufferVal(battleCtx, i) == 0)
                    {
                        break;
                    }
                    if (battleCtx.IoBuffer[i, 0] - 1 == MoveMenuCancel)
                    {
                        battleCtx.BattlerActions[i, 0] = ActionRun;
                        battleCtx.CurCommandState[i] = SelectStateRunInit;
                        break;
                    }
                    if (BattleContexts.BattleSystem_CanUseMove(battleSys, battleCtx, i, battleCtx.IoBuffer[i, 0] - 1, msg) == 0)
                    {
                        if ((BattleSystems.BattleSystem_GetBattleStatusMask(battleSys) & SelectBattleStatusPlayback) != 0)
                        {
                            BattleController_StopForInvalidRecording(battleSys);
                        }
                        else
                        {
                            BattleIO.BattleIO_SetAlertMessage(battleSys, i, msg);
                            battleCtx.CurCommandState[i] = SelectStateAlertMessageWait;
                            battleCtx.NextCommandState[i] = SelectStateMoveInit;
                        }
                    }
                    else
                    {
                        battleCtx.BattlerActions[i, 2] = battleCtx.IoBuffer[i, 0];
                        battleCtx.MoveSlot[i] = (ushort)(battleCtx.IoBuffer[i, 0] - 1);
                        battleCtx.MoveSelected[i] = battleCtx.BattleMons[i].Moves[battleCtx.MoveSlot[i]];
                        battleCtx.CurCommandState[i] = SelectStateTargetInit;
                        battleCtx.RecordedCommandFlags[i] |= RecordedFlagMove;
                    }
                    break;
                case SelectStateTargetInit:
                {
                    int target = (int)battleCtx.BattlerActions[i, 1];
                    if (BattleController_NeedsTargetSelection(battleSys, battleCtx, (byte)i, battleType, out int range, battleCtx.MoveSlot[i], ref target) != 0)
                    {
                        battleCtx.BattlerActions[i, 1] = (uint)target;
                        BattleIO.BattleIO_ShowTargetSelectMenu(battleSys, battleCtx, range, i);
                        battleCtx.CurCommandState[i] = SelectStateTargetInput;
                    }
                    else
                    {
                        battleCtx.BattlerActions[i, 1] = (uint)target;
                        battleCtx.CurCommandState[i] = SelectStateEnd;
                    }
                    break;
                }
                case SelectStateTargetInput:
                    if (BattleContexts.BattleContext_IOBufferVal(battleCtx, i) == InputCancel)
                    {
                        battleCtx.CurCommandState[i] = SelectStateMoveInit;
                        break;
                    }
                    if (BattleContexts.BattleContext_IOBufferVal(battleCtx, i) == 0)
                    {
                        break;
                    }
                    battleCtx.BattlerActions[i, 1] = (uint)(battleCtx.IoBuffer[i, 0] - 1);
                    battleCtx.CurCommandState[i] = SelectStateEnd;
                    battleCtx.RecordedCommandFlags[i] |= RecordedFlagTarget;
                    break;
                case SelectStateItemInit:
                    BattleIO.BattleIO_ShowBagScreen(battleSys, battleCtx, i);
                    battleCtx.CurCommandState[i] = SelectStateItemInput;
                    goto case SelectStateItemInput;
                case SelectStateItemInput:
                    if (BattleContexts.BattleContext_IOBufferVal(battleCtx, i) == InputCancel)
                    {
                        battleCtx.CurCommandState[i] = SelectStateInit;
                        break;
                    }
                    if (BattleContexts.BattleContext_IOBufferVal(battleCtx, i) == 0)
                    {
                        break;
                    }
                    battleCtx.BattlerActions[i, 2] = (uint)(battleCtx.IoBuffer[i, 0] | (battleCtx.IoBuffer[i, 1] << 8) | (battleCtx.IoBuffer[i, 2] << 16) | (battleCtx.IoBuffer[i, 3] << 24));
                    battleCtx.CurCommandState[i] = SelectStateEnd;
                    break;
                case SelectStatePartyInit:
                {
                    int doubleSwitchSlot = NoDoubleSwitchSlot;
                    int trapped = BattleContexts.Battler_IsTrappedForSwitch(battleSys, battleCtx, i);
                    if (BattleSystems.BattleSystem_BattlerSlot(battleSys, i) == 4 || BattleSystems.BattleSystem_BattlerSlot(battleSys, i) == 5)
                    {
                        if (battleType == 3 || battleType == 7 || battleType == 0x83 || (battleType == 0x13 && BattleSystems.BattleSystem_BattlerSlot(battleSys, i) == 4))
                        {
                            int partner = BattleSystems.BattleSystem_GetPartner(battleSys, i);
                            if (battleCtx.BattlerActions[partner, 0] == ActionParty)
                            {
                                doubleSwitchSlot = (int)battleCtx.BattlerActions[partner, 2];
                            }
                        }
                    }
                    BattleIO.BattleIO_ShowPartyMenu(battleSys, battleCtx, i, 0, trapped, doubleSwitchSlot);
                    battleCtx.CurCommandState[i] = SelectStatePartyInput;
                    goto case SelectStatePartyInput;
                }
                case SelectStatePartyInput:
                    if (BattleContexts.BattleContext_IOBufferVal(battleCtx, i) == InputCancel)
                    {
                        battleCtx.CurCommandState[i] = SelectStateInit;
                        break;
                    }
                    if (BattleContexts.BattleContext_IOBufferVal(battleCtx, i) == 0)
                    {
                        break;
                    }
                    battleCtx.BattlerActions[i, 2] = (uint)(battleCtx.IoBuffer[i, 0] - 1);
                    battleCtx.SwitchedPartySlot[i] = (byte)(battleCtx.IoBuffer[i, 0] - 1);
                    battleCtx.CurCommandState[i] = SelectStateEnd;
                    break;
                case SelectStateRunInit:
                    if ((battleType & SelectBattleTypeFrontier) != 0)
                    {
                        BattleIO.BattleIO_ShowYesNoMenu(battleSys, battleCtx, i, MessageForfeitPrompt, 0, 0, 0);
                        battleCtx.CurCommandState[i] = SelectStateRunInput;
                        break;
                    }
                    if ((battleType & SelectBattleTypeTrainer) != 0 && (battleType & SelectBattleTypeLink) == 0)
                    {
                        if ((BattleSystems.BattleSystem_GetBattleStatusMask(battleSys) & SelectBattleStatusPlayback) != 0)
                        {
                            BattleController_StopForInvalidRecording(battleSys);
                        }
                        else
                        {
                            msg.Tags = 0;
                            msg.Id = MessageNoRunningFromTrainer;
                            BattleIO.BattleIO_SetAlertMessage(battleSys, i, msg);
                            battleCtx.CurCommandState[i] = SelectStateAlertMessageWait;
                            battleCtx.NextCommandState[i] = SelectStateInit;
                        }
                        break;
                    }
                    if (BattleContexts.Battler_IsTrapped(battleSys, battleCtx, i, msg) != 0)
                    {
                        if ((BattleSystems.BattleSystem_GetBattleStatusMask(battleSys) & SelectBattleStatusPlayback) != 0)
                        {
                            BattleController_StopForInvalidRecording(battleSys);
                        }
                        else
                        {
                            BattleIO.BattleIO_SetAlertMessage(battleSys, i, msg);
                            battleCtx.CurCommandState[i] = SelectStateAlertMessageWait;
                            battleCtx.NextCommandState[i] = SelectStateInit;
                        }
                        break;
                    }
                    battleCtx.CurCommandState[i] = SelectStateRunInput;
                    break;
                case SelectStateRunInput:
                    if ((battleType & SelectBattleTypeFrontier) != 0)
                    {
                        if (BattleContexts.BattleContext_IOBufferVal(battleCtx, i) != 0)
                        {
                            if (BattleContexts.BattleContext_IOBufferVal(battleCtx, i) == InputCancel)
                            {
                                battleCtx.CurCommandState[i] = SelectStateInit;
                            }
                            else
                            {
                                battleCtx.CurCommandState[i] = SelectStateEnd;
                            }
                        }
                    }
                    else
                    {
                        battleCtx.CurCommandState[i] = SelectStateEnd;
                    }
                    if ((battleType & SelectBattleTypeDoubles) != 0 && (battleType & SelectBattleTypeMulti) == 0 && battleCtx.CurCommandState[i] == SelectStateEnd)
                    {
                        battleCtx.CurCommandState[BattleSystems.BattleSystem_GetPartner(battleSys, i)] = SelectStateEnd;
                    }
                    break;
                case SelectStateEnd:
                    BattleIO.BattleIO_StopHealthboxArrow(battleSys, i);
                    if (battleType == 7)
                    {
                        if (battleCtx.CurCommandState[BattleSystems.BattleSystem_GetPartner(battleSys, i)] == SelectStateWait)
                        {
                            BattleIO.BattleIO_LinkWaitMessage(battleSys, i);
                        }
                    }
                    else
                    {
                        BattleIO.BattleIO_LinkWaitMessage(battleSys, i);
                    }
                    battleCtx.CurCommandState[i] = SelectStateWait;
                    goto case SelectStateWait;
                case SelectStateWait:
                    doneCount++;
                    break;
                case SelectStateAlertMessageWait:
                    if (BattleContexts.BattleContext_IOBufferVal(battleCtx, i) != 0)
                    {
                        BattleIO.BattleIO_ClearBuffer(battleCtx, i);
                        battleCtx.CurCommandState[i] = battleCtx.NextCommandState[i];
                    }
                    break;
                case SelectStateStruggleMessage:
                    msg.Tags = 2;
                    msg.Id = MessageNoMovesLeft;
                    msg.Params[0] = BattleContexts.BattleSystem_NicknameTag(battleCtx, i);
                    BattleIO.BattleIO_SetAlertMessage(battleSys, i, msg);
                    battleCtx.CurCommandState[i] = SelectStateAlertMessageWait;
                    battleCtx.NextCommandState[i] = SelectStateEnd;
                    break;
                case SelectStateClearTouchScreen:
                    BattleIO.BattleIO_ClearTouchScreen(battleSys, i);
                    battleCtx.CurCommandState[i] = battleCtx.NextCommandState[i];
                    break;
            }
        }
        if (doneCount == maxBattlers)
        {
            BattleController_RecordCommands(battleSys, battleCtx);
            Battle.Battle_SetOverlay(battleSys, 0);
            battleCtx.Command = CommandBeforeTurn;
            for (int i = 0; i < maxBattlers; i++)
            {
                if (battleCtx.BattlerActions[i, 0] == ActionParty)
                {
                    BattleContexts.BattleContext_SwitchPartyOrder(battleSys, battleCtx, i, battleCtx.SwitchedPartySlot[i]);
                }
            }
        }
    }

    // OFFSET 0x02250DE4
    private static int BattleController_NeedsTargetSelection(BattleSystem battleSys, BattleContext battleCtx, byte battler, uint battleType, out int range, int moveSlot, ref int target)
    {
        ushort move = battleCtx.BattleMons[battler].Moves[moveSlot];
        if (move == MoveCurseID && BattleContexts.Move_IsGhostCurse(battleCtx, move, battler) == 0)
        {
            range = RangeCurseUser;
        }
        else
        {
            range = battleCtx.AiContext.MoveTable[move].Range;
        }
        if ((battleType & SelectBattleTypeDoubles) != 0)
        {
            if (range == RangeAllyOnly)
            {
                return (HealthBar.FlagIndex(BattleSystems.BattleSystem_GetPartner(battleSys, battler)) & battleCtx.BattlersSwitchingMask) != 0 ? 0 : 1;
            }
            return 1;
        }
        if ((range & RangeSelfTargetMask) != 0)
        {
            target = battler;
        }
        else
        {
            target = battler ^ 1;
        }
        return 0;
    }

    // OFFSET 0x02251694
    private static void BattleController_RecordCommands(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        for (int i = 0; i < maxBattlers; i++)
        {
            uint action = battleCtx.BattlerActions[i, 0];
            if (action == ActionSwitchedIn)
            {
                continue;
            }
            byte flags = battleCtx.RecordedCommandFlags[i];
            if ((flags & RecordedFlagCommand) != 0)
            {
                BattleSystems.BattleSystem_Record(battleSys, i, (byte)(action - 0xC));
            }
            switch (action)
            {
                case ActionNone:
                    if ((flags & RecordedFlagMove) != 0)
                    {
                        BattleSystems.BattleSystem_Record(battleSys, i, (byte)battleCtx.BattlerActions[i, 2]);
                    }
                    if ((flags & RecordedFlagTarget) != 0)
                    {
                        BattleSystems.BattleSystem_Record(battleSys, i, (byte)(battleCtx.BattlerActions[i, 1] + 1));
                    }
                    break;
                case ActionItem:
                    BattleSystems.BattleSystem_Record(battleSys, i, (byte)battleCtx.BattlerActions[i, 2]);
                    BattleSystems.BattleSystem_Record(battleSys, i, 0);
                    break;
                case ActionParty:
                    BattleSystems.BattleSystem_Record(battleSys, i, (byte)(battleCtx.BattlerActions[i, 2] + 1));
                    break;
                case ActionRun:
                    BattleSystems.BattleSystem_Record(battleSys, i, 1);
                    break;
            }
        }
    }
}
