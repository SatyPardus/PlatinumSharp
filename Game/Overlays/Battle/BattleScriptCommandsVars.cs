using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class BattleScriptTaskData
{
    public BattleSystem? BattleSys;
    public BattleContext? BattleCtx;
    public int SeqNum;
    public int Flag;
    public int Ball;
}

public static partial class BattleScripts
{
    private const uint BattlerStatusSwitching = 1;
    private const uint BattleTypeLinkDoubles = 6;
    private const int LinkWaitingShift = 28;
    private const uint LinkWaitingMask = 0xF0000000;
    private const int SelectorAttacker = 1;
    private const int SelectorSwitchedMon = 6;
    private const int SelectorForcedOut = 0x12;
    private const int SelectorAllBattlers = 0;
    private const int PartySizeMax = 6;
    private const int AbilitySkillLink = 0x5C;
    private const int OpSet = 7;
    private const int OpAdd = 8;
    private const int OpSub = 9;
    private const int OpFlagOn = 10;
    private const int OpFlagOff = 11;
    private const int OpMul = 12;
    private const int OpDiv = 13;
    private const int OpLeftShift = 14;
    private const int OpRightShift = 15;
    private const int OpFlagIndex = 16;
    private const int OpGet = 17;
    private const int OpSubToZero = 18;
    private const int OpXor = 19;
    private const int OpAnd = 20;
    private const int SubscriptAttackDown2 = 0x2E;
    private const int SubscriptAttackUp2 = 0x27;
    private const int SubscriptAttackDown1 = 0x16;
    private const int SubscriptAttackUp1 = 0xF;
    private const int AnimationStatBoost = 0xC;
    private const int AnimationStatDrop = 0xD;
    private const uint FailStatStageChange = 0x20000;
    private const int MaxStatStage = 12;
    private const int MinStatStage = 0;
    private const int SideEffectDirect = 1;
    private const int SideEffectIndirect = 2;
    private const int SideEffectAbility = 3;
    private const int SideEffectHeldItem = 5;
    private const uint SideEffectCannotPrevent = 0x8000000;
    private const uint VolatileSubstituteScript = 0x1000000;
    private const int AbilityClearBody = 0x1D;
    private const int AbilityWhiteSmoke = 0x49;
    private const int AbilityKeenEye = 0x33;
    private const int AbilityHyperCutter = 0x34;
    private const int AbilityShieldDust = 0x13;
    private const int StatAccuracy = 6;
    private const int TagNickname = 2;
    private const int TagNicknameAbility = 0xB;
    private const int TagNicknameStat = 0xC;
    private const int TagNicknameAbilityStat = 0x27;
    private const int TagNicknameItemStat = 0x2D;
    private const int TagNicknameAbilityNicknameAbility = 0x35;
    private const int TagNicknameAbilityNicknameStat = 0x36;
    private const int MsgStatWontGoHigher = 0x8E;
    private const int MsgStatWontGoLower = 0x91;
    private const int MsgProtectedByMist = 0x111;
    private const int MsgAbilityRaisedStat = 0x26E;
    private const int MsgAbilityCutsStat = 0x296;
    private const int MsgAbilityPreventsStatLoss = 0x29D;
    private const int MsgAbilityPreventsBufferStatLoss = 0x2C0;
    private const int MsgAbilitySuppressedAbility = 0x2D7;
    private const int MsgStatRose = 0x2EE;
    private const int MsgStatSharplyRose = 0x2F1;
    private const int MsgItemRaisedStat = 0x2F4;
    private const int MsgStatFell = 0x2FA;
    private const int MsgStatHarshlyFell = 0x2FD;
    private const int BattleMonAbility = 0x1A;
    private const int CheckHave = 0;

    // OFFSET 0x02241F34
    private static int BattleScript_ShowParty(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        uint switchingMask = 0;
        int battler;
        for (battler = 0; battler < maxBattlers; battler++)
        {
            if ((battleCtx.BattlerStatusFlags[battler] & BattlerStatusSwitching) != 0)
            {
                switchingMask |= HealthBar.FlagIndex(battler);
                BattleIO.BattleIO_ShowPartyMenu(battleSys, battleCtx, battler, 1, 0, 6);
            }
        }
        for (battler = 0; battler < maxBattlers; battler++)
        {
            if (BattleSystems.BattleSystem_GetBattleType(battleSys) == BattleTypeLinkDoubles)
            {
                int partner = BattleSystems.BattleSystem_GetPartner(battleSys, battler);
                if ((switchingMask & HealthBar.FlagIndex(battler)) == 0 && (switchingMask & HealthBar.FlagIndex(partner)) == 0)
                {
                    switchingMask |= HealthBar.FlagIndex(battler);
                    BattleIO.BattleIO_LinkWaitMessage(battleSys, battler);
                }
            }
            else if ((switchingMask & HealthBar.FlagIndex(battler)) == 0)
            {
                BattleIO.BattleIO_LinkWaitMessage(battleSys, battler);
            }
        }
        for (battler = 0; battler < maxBattlers; battler++)
        {
            if ((battleCtx.BattlerStatusFlags[battler] & BattlerStatusSwitching) != 0)
            {
                battleCtx.SwitchedMon = battler;
                break;
            }
        }
        return 0;
    }

    // OFFSET 0x0224200C
    private static int BattleScript_WaitMonSelection(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        int totalSwitching = 0;
        int battler;
        for (battler = 0; battler < maxBattlers; battler++)
        {
            if ((battleCtx.BattlerStatusFlags[battler] & BattlerStatusSwitching) != 0)
            {
                totalSwitching++;
            }
        }
        for (battler = 0; battler < maxBattlers; battler++)
        {
            if ((battleCtx.BattlerStatusFlags[battler] & BattlerStatusSwitching) != 0 && BattleContexts.BattleContext_IOBufferVal(battleCtx, battler) != 0)
            {
                battleCtx.SwitchedPartySlot[battler] = (byte)(battleCtx.IoBuffer[battler, 0] - 1);
                totalSwitching--;
                if ((battleCtx.BattleStatusMask2 & (HealthBar.FlagIndex(battler) << LinkWaitingShift)) == 0)
                {
                    battleCtx.BattleStatusMask2 |= HealthBar.FlagIndex(battler) << LinkWaitingShift;
                    BattleIO.BattleIO_LinkWaitMessage(battleSys, battler);
                }
            }
        }
        if (totalSwitching == 0)
        {
            for (battler = 0; battler < maxBattlers; battler++)
            {
                if ((battleCtx.BattlerStatusFlags[battler] & BattlerStatusSwitching) != 0 && BattleContexts.BattleContext_IOBufferVal(battleCtx, battler) != 0)
                {
                    BattleSystems.BattleSystem_Record(battleSys, battler, battleCtx.IoBuffer[battler, 0]);
                }
            }
            battleCtx.BattleStatusMask2 &= ~LinkWaitingMask;
            BattleScript_Iter(battleCtx, 1);
        }
        battleCtx.BattleProgressFlag = 1;
        return 0;
    }

    // OFFSET 0x02242134
    private static int BattleScript_SwitchAndUpdateMon(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int inBattler = BattleScript_Read(battleCtx);
        int battler = 0;
        switch (inBattler)
        {
            case SelectorAttacker:
                battler = battleCtx.Attacker;
                break;
            case SelectorSwitchedMon:
                battler = battleCtx.SwitchedMon;
                break;
            case SelectorForcedOut:
                battler = battleCtx.Defender;
                break;
        }
        battleCtx.BattlerStatusFlags[battler] &= ~BattlerStatusSwitching;
        battleCtx.BattlersSwitchingMask = (byte)(battleCtx.BattlersSwitchingMask & ~HealthBar.FlagIndex(battler));
        battleCtx.SelectedPartySlot[battler] = battleCtx.SwitchedPartySlot[battler];
        battleCtx.SwitchedPartySlot[battler] = PartySizeMax;
        BattleContexts.BattleSystem_InitBattleMon(battleSys, battleCtx, battler, battleCtx.SelectedPartySlot[battler]);
        BattleContexts.BattleContext_SwitchPartyOrder(battleSys, battleCtx, battler, battleCtx.SelectedPartySlot[battler]);
        battleCtx.HpTemp = (short)battleCtx.BattleMons[1].CurHP;
        BattleContexts.BattleSystem_InitSwitchedInMon(battleSys, battleCtx, battler);
        return 0;
    }

    // OFFSET 0x022421D4
    private static int BattleScript_GoToIfAnySwitches(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int jump = BattleScript_Read(battleCtx);
        for (int i = 0; i < maxBattlers; i++)
        {
            if ((battleCtx.BattlerStatusFlags[i] & BattlerStatusSwitching) != 0)
            {
                battleCtx.SwitchedMon = i;
                BattleScript_Iter(battleCtx, jump);
                break;
            }
        }
        return 0;
    }

    // OFFSET 0x0224221C
    private static int BattleScript_StartCatchMonTask(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int safariCapture = BattleScript_Read(battleCtx);
        BattleScriptTaskData data = new();
        battleCtx.TaskData = data;
        data.BattleSys = battleSys;
        data.BattleCtx = battleCtx;
        data.SeqNum = 0;
        data.Flag = safariCapture;
        data.Ball = battleCtx.MsgItemTemp;
        SysTask.SysTask_Start(BattleScript_CatchMonTask, data, 0);
        return 0;
    }

    // OFFSET 0x0224226C
    private static int BattleScript_WaitCatchMonTask(BattleSystem battleSys, BattleContext battleCtx)
    {
        if (battleCtx.TaskData == null)
        {
            BattleScript_Iter(battleCtx, 1);
        }
        battleCtx.BattleProgressFlag = 1;
        return 0;
    }

    // OFFSET 0x02242298
    private static int BattleScript_SetMultiHit(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int hits = BattleScript_Read(battleCtx);
        int flags = BattleScript_Read(battleCtx);
        if (battleCtx.MultiHitNumHits == 0)
        {
            if (hits == 0)
            {
                if (BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) == AbilitySkillLink)
                {
                    hits = 5;
                }
                else
                {
                    hits = (int)(BattleSystems.BattleSystem_RandNext(battleSys) & 3);
                    if (hits < 2)
                    {
                        hits += 2;
                    }
                    else
                    {
                        hits = (int)(BattleSystems.BattleSystem_RandNext(battleSys) & 3) + 2;
                    }
                }
            }
            battleCtx.MultiHitCounter = (byte)hits;
            battleCtx.MultiHitNumHits = (byte)hits;
            battleCtx.MultiHitAccuracyCheck = (uint)flags;
        }
        return 0;
    }

    private static int ApplyOpcode(int op, int current, ref int source)
    {
        switch (op)
        {
            case OpSet:
                return source;
            case OpAdd:
                return current + source;
            case OpSub:
                return current - source;
            case OpFlagOn:
                return current | source;
            case OpFlagOff:
                return current & ~source;
            case OpMul:
                return current * source;
            case OpDiv:
                return current / source;
            case OpLeftShift:
                return current << source;
            case OpRightShift:
                return (int)((uint)current >> source);
            case OpFlagIndex:
                return (int)HealthBar.FlagIndex(source);
            case OpSubToZero:
            {
                int value = current - source;
                return value < 0 ? 0 : value;
            }
            case OpXor:
                return current ^ source;
            case OpAnd:
                return current & source;
            default:
                ErrorHandling.GF_AssertFail();
                return current;
        }
    }

    // OFFSET 0x0224230C
    private static int BattleScript_UpdateVar(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int op = BattleScript_Read(battleCtx);
        int dstVar = BattleScript_Read(battleCtx);
        int srcVal = BattleScript_Read(battleCtx);
        ref int var = ref BattleScript_VarAddress(battleSys, battleCtx, dstVar);
        if (op == OpGet)
        {
            ErrorHandling.GF_AssertFail();
            return 0;
        }
        var = ApplyOpcode(op, var, ref srcVal);
        return 0;
    }

    private static void SetupNicknameStatMsg(BattleContext battleCtx, int msgID, int statOffset)
    {
        battleCtx.MsgBuffer.Id = (ushort)msgID;
        battleCtx.MsgBuffer.Tags = TagNicknameStat;
        battleCtx.MsgBuffer.Params[0] = BattleContexts.BattleSystem_NicknameTag(battleCtx, battleCtx.SideEffectMon);
        battleCtx.MsgBuffer.Params[1] = 1 + statOffset;
    }

    private static void SetupNicknameAbilityStatMsg(BattleContext battleCtx, int msgID, int statOffset)
    {
        battleCtx.MsgBuffer.Id = (ushort)msgID;
        battleCtx.MsgBuffer.Tags = TagNicknameAbilityStat;
        battleCtx.MsgBuffer.Params[0] = BattleContexts.BattleSystem_NicknameTag(battleCtx, battleCtx.SideEffectMon);
        battleCtx.MsgBuffer.Params[1] = battleCtx.BattleMons[battleCtx.SideEffectMon].Ability;
        battleCtx.MsgBuffer.Params[2] = 1 + statOffset;
    }

    private static void SetupNicknameAbilityNicknameAbilityMsg(BattleContext battleCtx, int msgID)
    {
        battleCtx.MsgBuffer.Id = (ushort)msgID;
        battleCtx.MsgBuffer.Tags = TagNicknameAbilityNicknameAbility;
        battleCtx.MsgBuffer.Params[0] = BattleContexts.BattleSystem_NicknameTag(battleCtx, battleCtx.SideEffectMon);
        battleCtx.MsgBuffer.Params[1] = battleCtx.BattleMons[battleCtx.SideEffectMon].Ability;
        battleCtx.MsgBuffer.Params[2] = BattleContexts.BattleSystem_NicknameTag(battleCtx, battleCtx.Attacker);
        battleCtx.MsgBuffer.Params[3] = battleCtx.BattleMons[battleCtx.Attacker].Ability;
    }

    private static bool AbilityBlocksSpecificStatReduction(BattleContext battleCtx, int statOffset, int ability, int stat)
    {
        return BattleContexts.Battler_IgnorableAbility(battleCtx, battleCtx.Attacker, battleCtx.SideEffectMon, ability) == 1 && 1 + statOffset == stat;
    }

    private static int StageFailed(BattleContext battleCtx, int jumpBlocked, int jumpNoChange, int statOffset, int msgID)
    {
        battleCtx.BattleStatusMask |= FailStatStageChange;
        if (battleCtx.SideEffectType == SideEffectIndirect || battleCtx.SideEffectType == SideEffectAbility)
        {
            BattleScript_Iter(battleCtx, jumpBlocked);
        }
        else
        {
            SetupNicknameStatMsg(battleCtx, msgID, statOffset);
            BattleScript_Iter(battleCtx, jumpNoChange);
        }
        return 0;
    }

    // OFFSET 0x02242400
    private static int BattleScript_ChangeStatStage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleMon mon = battleCtx.BattleMons[battleCtx.SideEffectMon];
        BattleScript_Iter(battleCtx, 1);
        int jumpNoChange = BattleScript_Read(battleCtx);
        int jumpBlocked = BattleScript_Read(battleCtx);
        int jumpBlockedBySubstitute = BattleScript_Read(battleCtx);
        int result = 0;
        int statOffset;
        int stageChange;
        battleCtx.BattleStatusMask &= ~FailStatStageChange;
        if (battleCtx.SideEffectParam >= SubscriptAttackDown2)
        {
            statOffset = battleCtx.SideEffectParam - SubscriptAttackDown2;
            stageChange = -2;
            battleCtx.ScriptTemp = AnimationStatDrop;
        }
        else if (battleCtx.SideEffectParam >= SubscriptAttackUp2)
        {
            statOffset = battleCtx.SideEffectParam - SubscriptAttackUp2;
            stageChange = 2;
            battleCtx.ScriptTemp = AnimationStatBoost;
        }
        else if (battleCtx.SideEffectParam >= SubscriptAttackDown1)
        {
            statOffset = battleCtx.SideEffectParam - SubscriptAttackDown1;
            stageChange = -1;
            battleCtx.ScriptTemp = AnimationStatDrop;
        }
        else
        {
            statOffset = battleCtx.SideEffectParam - SubscriptAttackUp1;
            stageChange = 1;
            battleCtx.ScriptTemp = AnimationStatBoost;
        }
        int stat = 1 + statOffset;
        if (stageChange > 0)
        {
            if (mon.StatBoosts[stat] == MaxStatStage)
            {
                battleCtx.BattleStatusMask |= FailStatStageChange;
                if (battleCtx.SideEffectType == SideEffectIndirect || battleCtx.SideEffectType == SideEffectAbility)
                {
                    BattleScript_Iter(battleCtx, jumpBlocked);
                }
                else
                {
                    SetupNicknameStatMsg(battleCtx, MsgStatWontGoHigher, statOffset);
                    BattleScript_Iter(battleCtx, jumpNoChange);
                }
                return 0;
            }
            if (battleCtx.SideEffectType == SideEffectAbility)
            {
                SetupNicknameAbilityStatMsg(battleCtx, MsgAbilityRaisedStat, statOffset);
            }
            else if (battleCtx.SideEffectType == SideEffectHeldItem)
            {
                battleCtx.MsgBuffer.Id = MsgItemRaisedStat;
                battleCtx.MsgBuffer.Tags = TagNicknameItemStat;
                battleCtx.MsgBuffer.Params[0] = BattleContexts.BattleSystem_NicknameTag(battleCtx, battleCtx.SideEffectMon);
                battleCtx.MsgBuffer.Params[1] = battleCtx.MsgItemTemp;
                battleCtx.MsgBuffer.Params[2] = stat;
            }
            else
            {
                SetupNicknameStatMsg(battleCtx, stageChange == 1 ? MsgStatRose : MsgStatSharplyRose, statOffset);
            }
            mon.StatBoosts[stat] = (sbyte)(mon.StatBoosts[stat] + stageChange);
            if (mon.StatBoosts[stat] > MaxStatStage)
            {
                mon.StatBoosts[stat] = MaxStatStage;
            }
            return 0;
        }
        if (((uint)battleCtx.SideEffectFlags & SideEffectCannotPrevent) == 0)
        {
            if (battleCtx.Attacker != battleCtx.SideEffectMon)
            {
                if (battleCtx.SideConditions[BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.SideEffectMon)].MistTurns != 0)
                {
                    battleCtx.MsgBuffer.Id = MsgProtectedByMist;
                    battleCtx.MsgBuffer.Tags = TagNickname;
                    battleCtx.MsgBuffer.Params[0] = BattleContexts.BattleSystem_NicknameTag(battleCtx, battleCtx.SideEffectMon);
                    result = 1;
                }
                else if (BattleContexts.Battler_IgnorableAbility(battleCtx, battleCtx.Attacker, battleCtx.SideEffectMon, AbilityClearBody) == 1 || BattleContexts.Battler_IgnorableAbility(battleCtx, battleCtx.Attacker, battleCtx.SideEffectMon, AbilityWhiteSmoke) == 1)
                {
                    if (battleCtx.SideEffectType == SideEffectAbility)
                    {
                        SetupNicknameAbilityNicknameAbilityMsg(battleCtx, MsgAbilitySuppressedAbility);
                    }
                    else
                    {
                        battleCtx.MsgBuffer.Id = MsgAbilityPreventsStatLoss;
                        battleCtx.MsgBuffer.Tags = TagNicknameAbility;
                        battleCtx.MsgBuffer.Params[0] = BattleContexts.BattleSystem_NicknameTag(battleCtx, battleCtx.SideEffectMon);
                        battleCtx.MsgBuffer.Params[1] = battleCtx.BattleMons[battleCtx.SideEffectMon].Ability;
                    }
                    result = 1;
                }
                else if (AbilityBlocksSpecificStatReduction(battleCtx, statOffset, AbilityKeenEye, StatAccuracy) || AbilityBlocksSpecificStatReduction(battleCtx, statOffset, AbilityHyperCutter, 1))
                {
                    if (battleCtx.SideEffectType == SideEffectAbility)
                    {
                        SetupNicknameAbilityNicknameAbilityMsg(battleCtx, MsgAbilitySuppressedAbility);
                    }
                    else
                    {
                        SetupNicknameAbilityStatMsg(battleCtx, MsgAbilityPreventsBufferStatLoss, statOffset);
                    }
                    result = 1;
                }
                else if (mon.StatBoosts[stat] == MinStatStage)
                {
                    return StageFailed(battleCtx, jumpBlocked, jumpNoChange, statOffset, MsgStatWontGoLower);
                }
                else if (BattleContexts.Battler_IgnorableAbility(battleCtx, battleCtx.Attacker, battleCtx.SideEffectMon, AbilityShieldDust) == 1 && battleCtx.SideEffectType == SideEffectIndirect)
                {
                    result = 1;
                }
                else if ((battleCtx.BattleMons[battleCtx.SideEffectMon].StatusVolatile & VolatileSubstituteScript) != 0)
                {
                    result = 2;
                }
            }
            else if (mon.StatBoosts[stat] == MinStatStage)
            {
                return StageFailed(battleCtx, jumpBlocked, jumpNoChange, statOffset, MsgStatWontGoLower);
            }
            if (result == 2 && battleCtx.SideEffectType == SideEffectDirect)
            {
                BattleScript_Iter(battleCtx, jumpBlockedBySubstitute);
                return 0;
            }
            if (result != 0 && battleCtx.SideEffectType == SideEffectIndirect)
            {
                BattleScript_Iter(battleCtx, jumpBlocked);
                return 0;
            }
            if (result != 0)
            {
                BattleScript_Iter(battleCtx, jumpNoChange);
                return 0;
            }
        }
        if (battleCtx.SideEffectType == SideEffectAbility)
        {
            battleCtx.MsgBuffer.Id = MsgAbilityCutsStat;
            battleCtx.MsgBuffer.Tags = TagNicknameAbilityNicknameStat;
            battleCtx.MsgBuffer.Params[0] = BattleContexts.BattleSystem_NicknameTag(battleCtx, battleCtx.Attacker);
            battleCtx.MsgBuffer.Params[1] = battleCtx.BattleMons[battleCtx.Attacker].Ability;
            battleCtx.MsgBuffer.Params[2] = BattleContexts.BattleSystem_NicknameTag(battleCtx, battleCtx.SideEffectMon);
            battleCtx.MsgBuffer.Params[3] = stat;
        }
        else
        {
            SetupNicknameStatMsg(battleCtx, stageChange == -1 ? MsgStatFell : MsgStatHarshlyFell, statOffset);
        }
        mon.StatBoosts[stat] = (sbyte)(mon.StatBoosts[stat] + stageChange);
        if (mon.StatBoosts[stat] < MinStatStage)
        {
            mon.StatBoosts[stat] = MinStatStage;
        }
        return 0;
    }

    // OFFSET 0x02242A14
    private static int BattleScript_UpdateMonData(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int op = BattleScript_Read(battleCtx);
        int inBattler = BattleScript_Read(battleCtx);
        int paramID = BattleScript_Read(battleCtx);
        int srcVal = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, inBattler);
        int monData = BattleContexts.BattleMon_Get(battleCtx, battler, paramID, null);
        if (op == OpGet)
        {
            ErrorHandling.GF_AssertFail();
        }
        else
        {
            monData = ApplyOpcode(op, monData, ref srcVal);
        }
        if (paramID == BattleMonAbility)
        {
            BattleAI_SetAbility(battleCtx, (byte)battler, (byte)monData);
        }
        BattleContexts.BattleMon_Set(battleCtx, battler, paramID, monData);
        BattleContexts.BattleMon_CopyToParty(battleSys, battleCtx, battler);
        return 0;
    }

    // OFFSET 0x02242B38
    private static int BattleScript_ClearVolatileStatus(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int inBattler = BattleScript_Read(battleCtx);
        int volStatus = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, inBattler);
        battleCtx.ClearVolatileStatus[battler] |= (uint)volStatus;
        return 0;
    }

    // OFFSET 0x02242B74
    private static int BattleScript_ToggleVanish(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int inBattler = BattleScript_Read(battleCtx);
        int toggle = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, inBattler);
        BattleIO.BattleIO_ToggleVanish(battleSys, battler, toggle);
        return 0;
    }

    // OFFSET 0x02242BAC
    private static int BattleScript_CheckAbility(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int op = BattleScript_Read(battleCtx);
        int inBattler = BattleScript_Read(battleCtx);
        int ability = BattleScript_Read(battleCtx);
        int jump = BattleScript_Read(battleCtx);
        if (inBattler == SelectorAllBattlers)
        {
            int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
            for (int battler = 0; battler < maxBattlers; battler++)
            {
                if (op == CheckHave)
                {
                    if (BattleContexts.Battler_Ability(battleCtx, battler) == ability)
                    {
                        BattleScript_Iter(battleCtx, jump);
                        battleCtx.AbilityMon = battler;
                        break;
                    }
                }
                else if (BattleContexts.Battler_Ability(battleCtx, battler) == ability)
                {
                    break;
                }
            }
            return 0;
        }
        int target = BattleScript_Battler(battleSys, battleCtx, inBattler);
        if (op == CheckHave)
        {
            if (BattleContexts.Battler_Ability(battleCtx, target) == ability)
            {
                BattleScript_Iter(battleCtx, jump);
                battleCtx.AbilityMon = target;
            }
        }
        else if (BattleContexts.Battler_Ability(battleCtx, target) != ability)
        {
            BattleScript_Iter(battleCtx, jump);
            battleCtx.AbilityMon = target;
        }
        return 0;
    }

    // OFFSET 0x02242C6C
    private static int BattleScript_Random(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int range = BattleScript_Read(battleCtx) + 1;
        int offset = BattleScript_Read(battleCtx);
        battleCtx.CalcTemp = (int)(BattleSystems.BattleSystem_RandNext(battleSys) % range) + offset;
        return 0;
    }

    // OFFSET 0x02242CA4
    private static int BattleScript_UpdateVarFromVar(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int op = BattleScript_Read(battleCtx);
        int dstVar = BattleScript_Read(battleCtx);
        int srcVar = BattleScript_Read(battleCtx);
        ref int dstData = ref BattleScript_VarAddress(battleSys, battleCtx, dstVar);
        ref int srcData = ref BattleScript_VarAddress(battleSys, battleCtx, srcVar);
        if (op == OpGet)
        {
            srcData = dstData;
            return 0;
        }
        dstData = ApplyOpcode(op, dstData, ref srcData);
        return 0;
    }

    // OFFSET 0x02242DBC
    private static int BattleScript_UpdateMonDataFromVar(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int op = BattleScript_Read(battleCtx);
        int inBattler = BattleScript_Read(battleCtx);
        int paramID = BattleScript_Read(battleCtx);
        int var = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, inBattler);
        int monData = BattleContexts.BattleMon_Get(battleCtx, battler, paramID, null);
        ref int varData = ref BattleScript_VarAddress(battleSys, battleCtx, var);
        if (op == OpGet)
        {
            varData = monData;
        }
        else
        {
            monData = ApplyOpcode(op, monData, ref varData);
        }
        if (paramID == BattleMonAbility)
        {
            BattleAI_SetAbility(battleCtx, (byte)battler, (byte)monData);
        }
        BattleContexts.BattleMon_Set(battleCtx, battler, paramID, monData);
        BattleContexts.BattleMon_CopyToParty(battleSys, battleCtx, battler);
        return 0;
    }

    private static void BattleScript_CatchMonTask(SysTask task, object? param)
    {
        throw Missing(0x02249B80);
    }
}

public static partial class BattleIO
{
    private const int CommandToggleVanish = 0x1D;
    private const int ToggleVanishSize = 0x28;

    // OFFSET 0x02265EE8
    public static void BattleIO_ToggleVanish(BattleSystem battleSys, int battler, int toggle)
    {
        BattleContext battleCtx = battleSys.BattleCtx!;
        byte[] message = new byte[ToggleVanishSize];
        W8(message, 0, CommandToggleVanish);
        W8(message, 1, toggle);
        W8(message, 2, (battleCtx.BattleMons[battler].StatusVolatile & VolatileSubstitute) != 0 ? 1 : 0);
        for (int i = 0; i < 4; i++)
        {
            BattleMon mon = battleCtx.BattleMons[i];
            W16(message, 4 + i * 2, mon.Species);
            W8(message, 0x10 + i, mon.IsShiny & 1);
            W8(message, 0x14 + i, mon.FormNum & 0x1F);
            if ((mon.StatusVolatile & VolatileTransformed) != 0)
            {
                W8(message, 0xC + i, mon.MoveEffectsData.TransformedGender);
                W32(message, 0x18 + i * 4, mon.MoveEffectsData.TransformedPID);
            }
            else
            {
                W8(message, 0xC + i, mon.Gender & 0xF);
                W32(message, 0x18 + i * 4, mon.Personality);
            }
        }
        BattleIO_Send(battleSys, RecipientBattler, battler, message, ToggleVanishSize);
    }
}
