using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class BattleScriptTaskData
{
    public BattleSystem? BattleSys;
    public BattleContext? BattleCtx;
    public int SeqNum;
    public int Flag;
    public int Ball;
    public BallRotation? BallRotation;
    public byte MsgIdx;
    public int Counter;
    public int Shakes;
    public int ShakesLeft;
    public object? DisplayData;
    public object? DisplayArgs;
}

public static partial class BattleScripts
{
    private const uint BattlerStatusSwitching = 1;
    private const int BallThrowTargetBase = 0x4E20;
    private const int HeapIDBattle = 5;
    private const uint BattleTypeCatchingDemo = 0x600;
    private const ushort SeqBallThrow = 0x70A;
    private const ushort SeqBallDeflect = 0x5E6;
    private const ushort SeqBallOpen = 0x708;
    private const ushort SeqBallLock = 0x709;
    private const ushort SeqTouchConfirm = 0x5DC;
    private const int BallSoundPan = 0x75;
    private const ushort BgmCaught = 0x467;
    private const int MsgGotcha = 0x363;
    private const int MsgGiveNickname = 0x364;
    private const int MsgDexRegistered = 0x367;
    private const int MsgTrainerBlocked = 0x35B;
    private const int MsgDontBeAThief = 0x35C;
    private const int MsgBrokeFreeBase = 0x35F;
    private const int MsgSentToBox = 0x496;
    private const int MsgSentToOtherBox = 0x498;
    private const int MonDataNicknameString = 0x78;
    private const int MonDataMove1PP = 0x3A;
    private const int MonDataMove1MaxPP = 0x42;
    private const int MonSpriteX = 0;
    private const int PadButtonA = 1;
    private const int NamingScreenTypePokemon = 1;
    private const int NicknameMaxChars = 10;
    private const uint NamingScreenTemplate = 0x020F2DAC;
    private const int RecordNicknamedMons = 0x31;
    private const byte BattleResultCaught = 4;
    private const int MaxShakes = 4;
    private const int ItemMasterBall = 1;
    private const int ItemSafariBall = 5;
    private const int ItemNetBall = 6;
    private const int ItemDiveBall = 7;
    private const int ItemNestBall = 8;
    private const int ItemRepeatBall = 9;
    private const int ItemTimerBall = 10;
    private const int ItemDuskBall = 13;
    private const int ItemQuickBall = 15;
    private const uint SafariCatchRateTable = 0x0226E638;
    private const uint BallRateTable = 0x0226E570;
    private const int SpeciesDataCatchRate = 8;
    private const int TypeBug = 6;
    private const int TerrainCave = 5;
    private const int TerrainWater = 7;
    private const int TimeNight = 3;
    private const int TimeLateNight = 4;
    private const uint StatusPoison = 8;
    private const uint StatusFreeze = 0x20;
    private const uint StatusToxic = 0x80;
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

    private static void BattleScript_PrintCatchMessage(BattleScriptTaskData data, MessageLoader msgLoader, int tags, int id, params int[] messageParams)
    {
        BattleMessage message = new();
        message.Tags = (byte)tags;
        message.Id = (ushort)id;
        for (int i = 0; i < messageParams.Length; i++)
        {
            message.Params[i] = messageParams[i];
        }
        data.MsgIdx = BattleMessages.BattleMessage_Print(data.BattleSys!, msgLoader, message, BattleMessages.BattleSystem_GetTextSpeed(data.BattleSys!));
    }

    private static void BattleScript_FadeOutCatchScene(BattleScriptTaskData data, PaletteData paletteData, PokemonSpriteManager monSpriteMan, ushort buffers)
    {
        PaletteAnimator.PaletteAnimator_SetMode(BattleSystems.BattleSystem_GetPaletteAnimator(data.BattleSys!)!, 1);
        PaletteData.PaletteData_StartFade(paletteData, buffers, 0xFFFF, 1, 0, 16, 0);
        PokemonSpriteManager.PokemonSpriteManager_StartFadeAll(monSpriteMan, 0, 16, 0, 0);
    }

    private static bool BattleScript_CatchMessageDone(BattleScriptTaskData data)
    {
        if (Text.Text_IsPrinterActive(data.MsgIdx) != 0)
        {
            return false;
        }
        data.Counter--;
        return data.Counter == 0;
    }

    private static void BattleScript_EndCatchMonTask(SysTask task, BattleScriptTaskData data)
    {
        data.BattleCtx!.TaskData = null;
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x02249B80
    private static void BattleScript_CatchMonTask(SysTask task, object? param)
    {
        BattleScriptTaskData data = (BattleScriptTaskData)param!;
        BattleSystem battleSys = data.BattleSys!;
        BattleContext battleCtx = data.BattleCtx!;
        MessageLoader msgLoader = BattleSystems.BattleSystem_GetMessageLoader(battleSys)!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(battleSys)!;
        PokemonSpriteManager monSpriteMan = BattleSystems.BattleSystem_GetPokemonSpriteManager(battleSys)!;
        int battler = 1;
        if ((HealthBar.FlagIndex(battler) & battleCtx.BattlersSwitchingMask) != 0)
        {
            battler = 3;
        }
        switch (data.SeqNum)
        {
            case 0:
                if (data.Flag == 0)
                {
                    BallThrow template = new();
                    template.Mode = 3;
                    template.HeapID = HeapIDBattle;
                    template.Target = battler + BallThrowTargetBase;
                    template.BallID = data.Ball;
                    template.SpriteSys = BattleSystems.BattleSystem_GetSpriteSystem(battleSys);
                    template.PaletteData = BattleSystems.BattleSystem_GetPaletteData(battleSys);
                    template.BgPrio = 1;
                    template.Surface = 0;
                    template.BattleSys = battleSys;
                    if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) != 0)
                    {
                        template.Type = battler == 1 ? 0x10 : 0x11;
                    }
                    else
                    {
                        template.Type = 0xF;
                    }
                    data.BallRotation = BallRotations.BallRotation_New(template);
                }
                else
                {
                    BattlerData battlerData = BattleSystems.BattleSystem_GetBattlerData(battleSys, 0)!;
                    if (BallRotations.BallRotation_GetMode(battlerData.BallRotation) == BallRotations.ModeHold)
                    {
                        return;
                    }
                    data.BallRotation = battlerData.BallRotation;
                    battlerData.BallRotation = null;
                }
                data.SeqNum = 1;
                SoundPlayback.Sound_PlayEffect(SeqBallThrow);
                battleSys.BallsThrown++;
                BallRotations.BallRotation_SetCatchMode(data.BallRotation!, 0);
                return;
            case 1:
                if (BallRotations.BallRotation_UpdateCatchMode(data.BallRotation!) != 0)
                {
                    return;
                }
                if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeTrainer) != 0)
                {
                    SoundPlayback.Sound_PlayPannedEffect(SeqBallDeflect, BallSoundPan);
                    BallRotations.BallRotation_SetCatchMode(data.BallRotation!, 2);
                    data.SeqNum = 0x19;
                    return;
                }
                SoundPlayback.Sound_PlayPannedEffect(SeqBallOpen, BallSoundPan);
                BallRotations.BallRotation_SetCatchMode(data.BallRotation!, 1);
                data.SeqNum = 2;
                data.Counter = 0x17;
                return;
            case 2:
                data.Counter--;
                if (data.Counter != 0)
                {
                    return;
                }
                BattleIO.BattleIO_ReturnToBall(battleSys, battler, data.Ball);
                data.Shakes = BattleScript_CalcCatchShakes(battleSys, battleCtx);
                data.ShakesLeft = data.Shakes < MaxShakes ? data.Shakes : 3;
                data.SeqNum = 3;
                return;
            case 3:
                if (BallRotations.BallRotation_UpdateCatchMode(data.BallRotation!) != 0 || BattleIO.BattleIO_QueueIsEmpty(battleCtx) == 0)
                {
                    return;
                }
                BallRotations.BallRotation_SetCatchMode(data.BallRotation!, 3);
                data.SeqNum = 4;
                return;
            case 4:
                if (BallRotations.BallRotation_UpdateCatchMode(data.BallRotation!) != 0)
                {
                    return;
                }
                data.SeqNum = 5;
                return;
            case 5:
                if (data.ShakesLeft == 0)
                {
                    if (data.Shakes == MaxShakes)
                    {
                        data.SeqNum = 7;
                        data.Counter = 12;
                        return;
                    }
                    data.SeqNum = 0x1C;
                    return;
                }
                BallRotations.BallRotation_SetCatchMode(data.BallRotation!, 4);
                data.SeqNum = 6;
                data.Counter = 12;
                return;
            case 6:
                if (BallRotations.BallRotation_UpdateCatchMode(data.BallRotation!) != 0)
                {
                    return;
                }
                data.Counter--;
                if (data.Counter != 0)
                {
                    return;
                }
                data.ShakesLeft--;
                data.SeqNum = 5;
                return;
            case 7:
                data.Counter--;
                if (data.Counter != 0)
                {
                    return;
                }
                BallRotations.BallRotation_SetCatchMode(data.BallRotation!, 6);
                SoundPlayback.Sound_PlayPannedEffect(SeqBallLock, BallSoundPan);
                data.SeqNum = 8;
                return;
            case 8:
                if (BallRotations.BallRotation_UpdateCatchMode(data.BallRotation!) != 0)
                {
                    return;
                }
                BattleScript_PrintCatchMessage(data, msgLoader, 0x82, MsgGotcha, battler);
                data.Counter = 0x1E;
                data.SeqNum = 9;
                SoundPlayback.Sound_PlayBGM(BgmCaught);
                Battle.BattleSystem_SetRedHPSoundFlag(battleSys, 2);
                return;
            case 9:
                if (Text.Text_IsPrinterActive(data.MsgIdx) != 0)
                {
                    return;
                }
                data.SeqNum = 0xA;
                BallRotations.BallRotation_SetCatchMode(data.BallRotation!, 7);
                return;
            case 0xA:
            {
                if (BallRotations.BallRotation_UpdateCatchMode(data.BallRotation!) != 0)
                {
                    return;
                }
                data.Counter--;
                if (data.Counter != 0)
                {
                    return;
                }
                Battle.BattleSystem_SetCaughtBattlerIdx(battleSys, battler);
                Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, battleCtx.SelectedPartySlot[battler]);
                if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeCatchingDemo) != 0)
                {
                    BattleContexts.BattleSystem_SetPokemonCatchData(battleSys, battleCtx, BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, battleCtx.SelectedPartySlot[battler]));
                    BattleScript_FadeOutCatchScene(data, paletteData, monSpriteMan, 0xF);
                    data.SeqNum = 0x20;
                    return;
                }
                if (BattleSystems.BattleSystem_CaughtSpecies(battleSys, (int)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null)) != 0)
                {
                    BattleScript_FadeOutCatchScene(data, paletteData, monSpriteMan, 5);
                    data.SeqNum = 0x10;
                    return;
                }
                BattleScript_PrintCatchMessage(data, msgLoader, 0x82, MsgDexRegistered, battler);
                data.Counter = 0x1E;
                data.SeqNum = 0xB;
                BattleSystems.BattleSystem_IncrementTrainerScore(battleSys);
                return;
            }
            case 0xB:
                if (!BattleScript_CatchMessageDone(data))
                {
                    return;
                }
                data.SeqNum = 0xC;
                PaletteData.PaletteData_StartFade(paletteData, 5, 0xFFFF, 1, 0, 16, 0);
                PokemonSpriteManager.PokemonSpriteManager_StartFadeAll(monSpriteMan, 0, 16, 0, 0);
                PaletteAnimator.PaletteAnimator_SetMode(BattleSystems.BattleSystem_GetPaletteAnimator(battleSys)!, 1);
                return;
            case 0xC:
            {
                if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
                {
                    return;
                }
                BallRotations.BallRotation_Free(data.BallRotation);
                PokemonSpriteManager.PokemonSpriteManager_DeleteAll(monSpriteMan);
                Battle.BattleSystem_FreeMainBackgrounds(battleSys);
                BattleTerrainSprites.TerrainSprite_SetDrawFlag(BattleSystems.BattleSystem_GetTerrainSprite(battleSys, 0), 0);
                BattleTerrainSprites.TerrainSprite_SetDrawFlag(BattleSystems.BattleSystem_GetTerrainSprite(battleSys, 1), 0);
                BattleDisplay.Battler_SetTrainerSpriteDrawFlag(BattleSystems.BattleSystem_GetBattlerData(battleSys, 0)!, 0);
                PokedexEntryScreenParams screenParams = new();
                screenParams.BgConfig = BattleSystems.BattleSystem_GetBgConfig(battleSys);
                screenParams.PaletteData = BattleSystems.BattleSystem_GetPaletteData(battleSys);
                screenParams.MonSpriteMan = monSpriteMan;
                screenParams.HeapID = HeapIDBattle;
                screenParams.Mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, battleCtx.SelectedPartySlot[battler]);
                screenParams.DexMode = DexMode.Pokedex_GetDexMode(BattleSystems.BattleSystem_GetPokedex(battleSys)!);
                data.DisplayArgs = CharTransfer.CharTransfer_PopTaskManager();
                data.DisplayData = PokedexEntryScreen_New(screenParams);
                data.SeqNum = 0xD;
                return;
            }
            case 0xD:
                if (!PokedexEntryScreen_IsReady(data.DisplayData))
                {
                    return;
                }
                if ((Globals.gSystem.PressedKeys & PadButtonA) != 0)
                {
                    data.SeqNum = 0xE;
                }
                else if (TouchScreen.TouchScreen_Tapped())
                {
                    SoundPlayback.Sound_PlayEffect(SeqTouchConfirm);
                    data.SeqNum = 0xE;
                }
                if (data.SeqNum == 0xE)
                {
                    PaletteData.PaletteData_StartFade(paletteData, 5, 0xFFFF, 1, 0, 16, 0);
                    PokedexEntryScreen_StartExit(data.DisplayData, false);
                }
                return;
            case 0xE:
            {
                PokemonSprite sprite = PokedexEntryScreen_GetSprite(data.DisplayData);
                PokemonSprite.PokemonSprite_AddAttribute(sprite, MonSpriteX, 4);
                if (PokemonSprite.PokemonSprite_GetAttribute(sprite, MonSpriteX) >= 0x80)
                {
                    PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteX, 0x80);
                    data.SeqNum = 0xF;
                }
                return;
            }
            case 0xF:
                PokedexEntryScreen_Free(data.DisplayData);
                CharTransfer.CharTransfer_PushTaskManager((CharTransferTaskManager?)data.DisplayArgs);
                Battle.BattleSystem_RestoreMainBackgrounds(battleSys);
                PaletteData.PaletteData_StartFade(paletteData, 5, 0xFFFF, 1, 16, 0, 0);
                data.SeqNum = 0x11;
                return;
            case 0x10:
            {
                if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
                {
                    return;
                }
                Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, battleCtx.SelectedPartySlot[battler]);
                BallRotations.BallRotation_Free(data.BallRotation);
                PokemonSpriteManager.PokemonSpriteManager_DeleteAll(monSpriteMan);
                BattleDisplay.Battler_SetTrainerSpriteDrawFlag(BattleSystems.BattleSystem_GetBattlerData(battleSys, 0)!, 0);
                Battle.BattleSystem_FreeMainBackgrounds(battleSys);
                Battle.BattleSystem_RestoreMainBackgrounds(battleSys);
                PokemonSpriteTemplate spriteTemplate = new();
                PokemonSpriteTemplates.Pokemon_BuildSpriteTemplate(spriteTemplate, mon, 2);
                PokemonSpriteManager.PokemonSpriteManager_CreateSprite(monSpriteMan, spriteTemplate, 0x80, 0x48, 0, 0, null, null);
                PaletteData.PaletteData_StartFade(paletteData, 5, 0xFFFF, 1, 16, 0, 0);
                PokemonSpriteManager.PokemonSpriteManager_StartFadeAll(monSpriteMan, 16, 0, 0, 0);
                data.SeqNum = 0x11;
                return;
            }
            case 0x11:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
                {
                    return;
                }
                data.SeqNum = 0x12;
                PaletteAnimator.PaletteAnimator_SetMode(BattleSystems.BattleSystem_GetPaletteAnimator(battleSys)!, 0);
                PaletteData.PaletteData_SetAutoTransparent(paletteData, 1);
                return;
            case 0x12:
                BattleIO.BattleIO_ShowYesNoMenu(battleSys, battleCtx, 0, MsgGiveNickname, 5, 0, battleCtx.SelectedPartySlot[battler] | battler);
                data.SeqNum++;
                return;
            case 0x13:
            {
                if (BattleContexts.BattleContext_IOBufferVal(battleCtx, 0) == 0)
                {
                    return;
                }
                if (BattleContexts.BattleContext_IOBufferVal(battleCtx, 0) == 0xFF)
                {
                    data.SeqNum = 0x16;
                    return;
                }
                BattleScript_FadeOutCatchScene(data, paletteData, monSpriteMan, 0xF);
                data.SeqNum = 0x14;
                return;
            }
            case 0x14:
            {
                if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
                {
                    return;
                }
                ScreenFadeControl.SetScreenColorBrightness(0, 0);
                ScreenFadeControl.SetScreenColorBrightness(1, 0);
                Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, battleCtx.SelectedPartySlot[battler]);
                int species = (int)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
                NamingScreenArgs args = NamingScreenArgs.NamingScreenArgs_Init(HeapIDBattle, NamingScreenTypePokemon, species, NicknameMaxChars, BattleSystems.BattleSystem_GetOptions(battleSys));
                data.DisplayArgs = args;
                args.BattleMsgID = BattleSystems.BattleSystem_GetPartyCount(battleSys, 0) >= PartySizeMax ? BattleSystems.BattleSystem_GetMetBebe(battleSys) + MsgSentToBox : 0;
                args.MonForm = (int)Pokemon.Pokemon_GetValue(mon, MonDataForm, null);
                args.PcBoxes = BattleSystems.BattleSystem_GetPCBoxes(battleSys);
                args.MonGender = (int)Pokemon.Pokemon_GetValue(mon, MonDataGender, null);
                data.DisplayData = ApplicationManager.ApplicationManager_New(ApplicationTemplate.At(NamingScreenTemplate), args, HeapIDBattle);
                data.SeqNum = 0x15;
                Battle.BattleSystem_FreeHealthboxes(battleSys);
                for (int i = 0; i < BattleSystems.BattleSystem_GetMaxBattlers(battleSys); i++)
                {
                    BattlerData battlerData = BattleSystems.BattleSystem_GetBattlerData(battleSys, i)!;
                    if (battlerData.ManagedSprite != null)
                    {
                        SpriteSystem.Sprite_DeleteAndFreeResources(battlerData.ManagedSprite);
                        battlerData.ManagedSprite = null;
                    }
                }
                Battle.BattleSystem_FreeGraphics(battleSys);
                BattleSystems.BattleSystem_SetRenderMode(battleSys, 1);
                return;
            }
            case 0x15:
            {
                if (!ApplicationManager.ApplicationManager_Exec((ApplicationManager)data.DisplayData!))
                {
                    return;
                }
                NamingScreenArgs args = (NamingScreenArgs)data.DisplayArgs!;
                Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, battleCtx.SelectedPartySlot[battler]);
                if (args.ReturnCode == 0)
                {
                    Pokemon.Pokemon_SetValue(mon, MonDataNicknameString, args.TextInputStr);
                    BattleSystems.BattleSystem_IncrementRecord(battleSys, RecordNicknamedMons);
                }
                NamingScreenArgs.NamingScreenArgs_Free(args);
                ApplicationManager.ApplicationManager_Free((ApplicationManager)data.DisplayData!);
                BattleSystems.BattleSystem_SetRenderMode(battleSys, 2);
                data.SeqNum = 0x17;
                return;
            }
            case 0x16:
            case 0x17:
            {
                if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
                {
                    return;
                }
                Party party = BattleSystems.BattleSystem_GetParty(battleSys, 0);
                Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, battleCtx.SelectedPartySlot[battler]);
                BattleSystems.BattleSystem_RegisterCaughtPokemon(battleSys, battler);
                BattleContexts.BattleSystem_SetPokemonCatchData(battleSys, battleCtx, mon);
                BattleSystems.BattleSystem_AddToPoketchHistory(battleSys, mon);
                BattleSystems.BattleSystem_RecordCaptureAttempt(battleSys, mon);
                BattleIO.BattleIO_PrintAlertMessage(battleSys, 0, 0, 9);
                if (Party.Party_AddPokemon(party, mon))
                {
                    if (data.SeqNum == 0x16)
                    {
                        BattleScript_FadeOutCatchScene(data, paletteData, monSpriteMan, 0xF);
                    }
                    data.SeqNum = 0x20;
                    return;
                }
                PCBoxes boxes = BattleSystems.BattleSystem_GetPCBoxes(battleSys)!;
                int currentBox = PCBoxes.PCBoxes_GetCurrentBoxID(boxes);
                int box = PCBoxes.PCBoxes_FirstEmptyBox(boxes);
                PCBoxes.PCBoxes_SetCurrentBox(boxes, box);
                for (int i = 0; i < 4; i++)
                {
                    int maxPP = (int)Pokemon.Pokemon_GetValue(mon, MonDataMove1MaxPP + i, null);
                    Pokemon.Pokemon_SetValue(mon, MonDataMove1PP + i, maxPP);
                }
                if (Pokemon.Pokemon_SetGiratinaFormByHeldItem(mon) != -1)
                {
                    BattleSystems.BattleSystem_RegisterCaughtPokemon(battleSys, battler);
                }
                PCBoxes.PCBoxes_TryStoreBoxMonInBox(boxes, box, Pokemon.Pokemon_GetBoxPokemon(mon));
                if (data.SeqNum != 0x16)
                {
                    data.SeqNum = 0x20;
                    return;
                }
                if (currentBox == box)
                {
                    BattleScript_PrintCatchMessage(data, msgLoader, 0x93, BattleSystems.BattleSystem_GetMetBebe(battleSys) + MsgSentToBox, battler, currentBox);
                }
                else
                {
                    BattleScript_PrintCatchMessage(data, msgLoader, 0xAF, BattleSystems.BattleSystem_GetMetBebe(battleSys) + MsgSentToOtherBox, battler, currentBox, box);
                }
                data.Counter = 0x1E;
                data.SeqNum = 0x18;
                return;
            }
            case 0x18:
                if (!BattleScript_CatchMessageDone(data))
                {
                    return;
                }
                BattleScript_FadeOutCatchScene(data, paletteData, monSpriteMan, 0xF);
                data.SeqNum = 0x20;
                return;
            case 0x19:
                if (BallRotations.BallRotation_UpdateCatchMode(data.BallRotation!) != 0)
                {
                    return;
                }
                BallRotations.BallRotation_Free(data.BallRotation);
                BattleScript_PrintCatchMessage(data, msgLoader, 0, MsgTrainerBlocked);
                data.Counter = 0x1E;
                data.SeqNum = 0x1A;
                return;
            case 0x1A:
                if (!BattleScript_CatchMessageDone(data))
                {
                    return;
                }
                BattleScript_PrintCatchMessage(data, msgLoader, 0, MsgDontBeAThief);
                data.Counter = 0x1E;
                data.SeqNum = 0x1B;
                return;
            case 0x1B:
                if (!BattleScript_CatchMessageDone(data))
                {
                    return;
                }
                BattleScript_EndCatchMonTask(task, data);
                return;
            case 0x1C:
                BattleIO.BattleIO_ShowPokemon(battleSys, battler, data.Ball, 1);
                data.SeqNum = 0x1D;
                data.Counter = 2;
                return;
            case 0x1D:
                data.Counter--;
                if (data.Counter != 0)
                {
                    return;
                }
                BallRotations.BallRotation_Free(data.BallRotation);
                data.SeqNum = 0x1E;
                return;
            case 0x1E:
                if (BattleIO.BattleIO_QueueIsEmpty(battleCtx) == 0)
                {
                    return;
                }
                BattleScript_PrintCatchMessage(data, msgLoader, 0, data.Shakes + MsgBrokeFreeBase);
                data.Counter = 0x1E;
                data.SeqNum = 0x1F;
                return;
            case 0x1F:
                if (!BattleScript_CatchMessageDone(data))
                {
                    return;
                }
                BattleScript_EndCatchMonTask(task, data);
                return;
            case 0x20:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
                {
                    return;
                }
                if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeCatchingDemo) != 0)
                {
                    BallRotations.BallRotation_Free(data.BallRotation);
                    PokemonSpriteManager.PokemonSpriteManager_DeleteAll(monSpriteMan);
                }
                battleSys.ResultMask = BattleResultCaught;
                BattleScript_EndCatchMonTask(task, data);
                return;
        }
    }

    // OFFSET 0x0224A724
    private static int BattleScript_CalcCatchShakes(BattleSystem battleSys, BattleContext battleCtx)
    {
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeCatchingDemo) != 0)
        {
            return MaxShakes;
        }
        BattleMon defender = battleCtx.BattleMons[battleCtx.Defender];
        uint catchRate;
        if (battleCtx.MsgItemTemp == ItemSafariBall)
        {
            byte[] stage = RomData.Overlay(16, (uint)(SafariCatchRateTable + battleCtx.SafariCatchStage * 2), 2);
            catchRate = SpeciesData.SpeciesData_GetSpeciesValue(defender.Species, SpeciesDataCatchRate) * stage[0] / stage[1];
        }
        else
        {
            catchRate = SpeciesData.SpeciesData_GetSpeciesValue(defender.Species, SpeciesDataCatchRate);
        }
        int type1 = BattleContexts.BattleMon_Get(battleCtx, battleCtx.Defender, BattleContexts.BattleMonType1, null);
        int type2 = BattleContexts.BattleMon_Get(battleCtx, battleCtx.Defender, BattleContexts.BattleMonType2, null);
        uint ballRate = 10;
        int ball = battleCtx.MsgItemTemp;
        if (ball <= ItemSafariBall)
        {
            ballRate = RomData.Overlay(16, (uint)(BallRateTable + ball - 2), 1)[0];
        }
        else
        {
            switch (ball)
            {
                case ItemNetBall:
                    if (type1 == TypeWater || type2 == TypeWater || type1 == TypeBug || type2 == TypeBug)
                    {
                        ballRate = 30;
                    }
                    break;
                case ItemDiveBall:
                    if (BattleSystems.BattleSystem_GetTerrain(battleSys) == TerrainWater)
                    {
                        ballRate = 35;
                    }
                    break;
                case ItemNestBall:
                    if (defender.Level < 40)
                    {
                        ballRate = (uint)(40 - defender.Level);
                        if (ballRate < 10)
                        {
                            ballRate = 10;
                        }
                    }
                    break;
                case ItemRepeatBall:
                    if (BattleSystems.BattleSystem_CaughtSpecies(battleSys, defender.Species) == 1)
                    {
                        ballRate = 30;
                    }
                    break;
                case ItemTimerBall:
                    ballRate = (uint)(battleCtx.TotalTurns + 10);
                    if (ballRate > 40)
                    {
                        ballRate = 40;
                    }
                    break;
                case ItemDuskBall:
                    if (BattleSystems.BattleSystem_GetTime(battleSys) == TimeNight || BattleSystems.BattleSystem_GetTime(battleSys) == TimeLateNight || BattleSystems.BattleSystem_GetTerrain(battleSys) == TerrainCave)
                    {
                        ballRate = 35;
                    }
                    break;
                case ItemQuickBall:
                    if (battleCtx.TotalTurns < 1)
                    {
                        ballRate = 40;
                    }
                    break;
            }
        }
        uint maxHP3 = defender.MaxHP * 3;
        uint rate = catchRate * ballRate / 10;
        rate = (maxHP3 - (uint)defender.CurHP * 2) * rate / maxHP3;
        if ((defender.Status & (StatusSleep | StatusFreeze)) != 0)
        {
            rate *= 2;
        }
        if ((defender.Status & (StatusPoison | StatusBurn | StatusParalysis | StatusToxic)) != 0)
        {
            rate = rate * 15 / 10;
        }
        int shakes;
        if (rate >= 0xFF)
        {
            shakes = MaxShakes;
        }
        else
        {
            uint ratio = MathCoprocessor.Div32(0xFF0000, (int)rate);
            uint shakeThreshold = 0xFFFF0 / MathCoprocessor.Sqrt32(MathCoprocessor.Sqrt32(ratio));
            shakes = 0;
            while (BattleSystems.BattleSystem_RandNext(battleSys) < shakeThreshold)
            {
                shakes++;
                if (shakes >= MaxShakes)
                {
                    break;
                }
            }
            if (battleCtx.MsgItemTemp == ItemMasterBall)
            {
                shakes = MaxShakes;
            }
        }
        return shakes;
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
