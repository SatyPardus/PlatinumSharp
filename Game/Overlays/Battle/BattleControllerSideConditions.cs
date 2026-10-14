namespace PlatinumSharp.Game;

public static partial class BattleControllers
{
    private const uint SideConditionFutureSight = 0x10;
    private const uint MoveEffectPerishSong = 0x20;
    private const uint FieldConditionTrickRoomMask = 0x70000;
    private const uint FieldConditionTrickRoomTurn = 0x10000;
    private const int MsgFutureSightHit = 0x1DB;
    private const int MsgTagNicknameMove = 0xA;
    private const int SubseqFutureSightDamage = 0x79;
    private const int SubseqPerishSongCount = 0x66;
    private const int SubseqTrickRoomEnd = 0xFB;
    private const int SubseqSafariWatch = 0xE5;
    private const int ControllerCommandTurnEnd = 0xC;
    private const int ControllerCommandTrainerMessage = 2;
    private const int ControllerCommandFieldConditionNext = 0x27;

    // OFFSET 0x0224D9C4
    private static void BattleController_CheckSideConditions(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        if (BattleController_AnyFainted(battleCtx, battleCtx.Command, battleCtx.Command, 1) == 1)
        {
            return;
        }
        BattleIO.BattleIO_ClearMessageBox(battleSys);
        switch (battleCtx.SideConditionCheckState)
        {
            case 0:
                while (battleCtx.SideConditionCheckTemp < maxBattlers)
                {
                    int battler = battleCtx.MonSpeedOrder[battleCtx.SideConditionCheckTemp];
                    if ((HealthBar.FlagIndex(battler) & battleCtx.BattlersSwitchingMask) != 0)
                    {
                        battleCtx.SideConditionCheckTemp++;
                        continue;
                    }
                    battleCtx.SideConditionCheckTemp++;
                    if (battleCtx.FieldConditions.FutureSightTurns[battler] != 0)
                    {
                        battleCtx.FieldConditions.FutureSightTurns[battler]--;
                        if (battleCtx.FieldConditions.FutureSightTurns[battler] == 0 && battleCtx.BattleMons[battler].CurHP != 0)
                        {
                            int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler);
                            battleCtx.SideConditionsMask[side] &= ~SideConditionFutureSight;
                            battleCtx.MsgBuffer.Id = MsgFutureSightHit;
                            battleCtx.MsgBuffer.Tags = MsgTagNicknameMove;
                            battleCtx.MsgBuffer.Params[0] = BattleContexts.BattleSystem_NicknameTag(battleCtx, battler);
                            battleCtx.MsgBuffer.Params[1] = battleCtx.FieldConditions.FutureSightMove[battler];
                            battleCtx.MsgBattlerTemp = battler;
                            battleCtx.MsgAttacker = battleCtx.FieldConditions.FutureSightAttacker[battler];
                            battleCtx.MsgMoveTemp = battleCtx.FieldConditions.FutureSightMove[battler];
                            battleCtx.HpCalcTemp = battleCtx.FieldConditions.FutureSightDamage[battler];
                            BattleController_RunSubseq(battleCtx, SubseqFutureSightDamage);
                            return;
                        }
                    }
                }
                battleCtx.SideConditionCheckState++;
                battleCtx.SideConditionCheckTemp = 0;
                goto case 1;
            case 1:
                while (battleCtx.SideConditionCheckTemp < maxBattlers)
                {
                    int battler = battleCtx.MonSpeedOrder[battleCtx.SideConditionCheckTemp];
                    if ((HealthBar.FlagIndex(battler) & battleCtx.BattlersSwitchingMask) != 0)
                    {
                        battleCtx.SideConditionCheckTemp++;
                        continue;
                    }
                    battleCtx.SideConditionCheckTemp++;
                    BattleMon mon = battleCtx.BattleMons[battler];
                    if ((mon.MoveEffectsMask & MoveEffectPerishSong) != 0)
                    {
                        if (mon.MoveEffectsData.PerishSongTurns == 0)
                        {
                            mon.MoveEffectsMask &= ~MoveEffectPerishSong;
                            battleCtx.MsgTemp = mon.MoveEffectsData.PerishSongTurns;
                            battleCtx.HpCalcTemp = -mon.CurHP;
                            battleCtx.BattleStatusMask |= BattleStatusMonConditionHP;
                        }
                        else
                        {
                            battleCtx.MsgTemp = mon.MoveEffectsData.PerishSongTurns;
                            mon.MoveEffectsData.PerishSongTurns = (mon.MoveEffectsData.PerishSongTurns - 1) & 3;
                        }
                        battleCtx.MsgBattlerTemp = battler;
                        BattleController_RunSubseq(battleCtx, SubseqPerishSongCount);
                        return;
                    }
                }
                battleCtx.SideConditionCheckState++;
                battleCtx.SideConditionCheckTemp = 0;
                goto case 2;
            case 2:
                if ((battleCtx.FieldConditionsMask & FieldConditionTrickRoomMask) != 0)
                {
                    battleCtx.FieldConditionsMask -= FieldConditionTrickRoomTurn;
                    if ((battleCtx.FieldConditionsMask & FieldConditionTrickRoomMask) == 0)
                    {
                        BattleController_RunSubseq(battleCtx, SubseqTrickRoomEnd);
                        return;
                    }
                }
                battleCtx.SideConditionCheckState++;
                battleCtx.SideConditionCheckTemp = 0;
                break;
        }
        battleCtx.SideConditionCheckState = 0;
        battleCtx.SideConditionCheckTemp = 0;
        battleCtx.Command = ControllerCommandTurnEnd;
    }

    // OFFSET 0x0224DC10
    private static void BattleController_TurnEnd(BattleSystem battleSys, BattleContext battleCtx)
    {
        if (BattleController_AnyExpPayout(battleCtx, battleCtx.Command, battleCtx.Command) == 1)
        {
            return;
        }
        if (BattleController_CheckBattleOver(battleSys, battleCtx) == 1)
        {
            return;
        }
        if (BattleController_ReplaceFainted(battleSys, battleCtx) == 1)
        {
            return;
        }
        battleCtx.TotalTurns++;
        battleCtx.MeFirstTurnOrder++;
        BattleContexts.BattleContext_InitCounters(battleCtx);
        BattleContexts.BattleSystem_InitTurnFlags(battleSys, battleCtx);
        battleCtx.Command = ControllerCommandTrainerMessage;
    }

    // OFFSET 0x0224E11C
    private static void BattleController_SafariWatch(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqSafariWatch);
        battleCtx.Attacker = 0;
        battleCtx.Defender = 1;
        battleCtx.Command = ControllerCommandRunScript;
        battleCtx.CommandNext = ControllerCommandFieldConditionNext;
    }
}
