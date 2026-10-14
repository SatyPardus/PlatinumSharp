namespace PlatinumSharp.Game;

public static partial class TrainerAI
{
    private const uint MoveEffectGastroAcid = 0x200000;
    private const byte AbilityShadowTag = 0x17;
    private const byte AbilityMagnetPull = 0x2A;
    private const byte AbilityArenaTrap = 0x47;
    private const int SpeciesDataAbility1 = 0x18;
    private const int SpeciesDataAbility2 = 0x19;
    private const int MonDataCurHP = 0xA3;
    private const int MonDataSpeciesEgg = 0xAE;
    private const int SpeciesEgg = 0x1EE;
    private const int DamageScoreNone = 0;
    private const int DamageScoreNotHighest = 1;
    private const int DamageScoreHighest = 2;

    private static readonly Action<BattleSystem, BattleContext>[] commands =
    {
        AICmd_IfRandomLessThan,
        AICmd_IfRandomGreaterThan,
        AICmd_IfRandomEqualTo,
        AICmd_IfRandomNotEqualTo,
        AICmd_AddToMoveScore,
        AICmd_IfHPPercentLessThan,
        AICmd_IfHPPercentGreaterThan,
        AICmd_IfHPPercentEqualTo,
        AICmd_IfHPPercentNotEqualTo,
        AICmd_IfStatus,
        AICmd_IfNotStatus,
        AICmd_IfVolatileStatus,
        AICmd_IfNotVolatileStatus,
        AICmd_IfMoveEffect,
        AICmd_IfNotMoveEffect,
        AICmd_IfSideCondition,
        AICmd_IfNotSideCondition,
        AICmd_IfLoadedLessThan,
        AICmd_IfLoadedGreaterThan,
        AICmd_IfLoadedEqualTo,
        AICmd_IfLoadedNotEqualTo,
        AICmd_IfLoadedMask,
        AICmd_IfLoadedNotMask,
        AICmd_IfMoveEqualTo,
        AICmd_IfMoveNotEqualTo,
        AICmd_IfLoadedInTable,
        AICmd_IfLoadedNotInTable,
        AICmd_IfAttackerHasDamagingMoves,
        AICmd_IfAttackerHasNoDamagingMoves,
        AICmd_LoadTurnCount,
        AICmd_LoadTypeFrom,
        AICmd_LoadMovePower,
        AICmd_FlagMoveDamageScore,
        AICmd_LoadBattlerPreviousMove,
        AICmd_IfTempEqualTo,
        AICmd_IfTempNotEqualTo,
        AICmd_IfSpeedCompareEqualTo,
        AICmd_IfSpeedCompareNotEqualTo,
        AICmd_CountAlivePartyBattlers,
        AICmd_LoadCurrentMove,
        AICmd_LoadCurrentMoveEffect,
        AICmd_LoadBattlerAbility,
        AICmd_CalcMaxEffectiveness,
        AICmd_IfMoveEffectivenessEquals,
        AICmd_IfPartyMemberStatus,
        AICmd_IfPartyMemberNotStatus,
        AICmd_LoadCurrentWeather,
        AICmd_IfCurrentMoveEffectEqualTo,
        AICmd_IfCurrentMoveEffectNotEqualTo,
        AICmd_IfStatStageLessThan,
        AICmd_IfStatStageGreaterThan,
        AICmd_IfStatStageEqualTo,
        AICmd_IfStatStageNotEqualTo,
        AICmd_IfCurrentMoveKills,
        AICmd_IfCurrentMoveDoesNotKill,
        AICmd_IfMoveKnown,
        AICmd_IfMoveNotKnown,
        AICmd_IfMoveEffectKnown,
        AICmd_IfMoveEffectNotKnown,
        AICmd_IfBattlerUnderEffect,
        AICmd_IfCurrentMoveMatchesEffect,
        AICmd_Escape,
        AICmd_Dummy3E,
        AICmd_Dummy3F,
        AICmd_LoadHeldItem,
        AICmd_LoadHeldItemEffect,
        AICmd_LoadGender,
        AICmd_LoadIsFirstTurnInBattle,
        AICmd_LoadStockpileCount,
        AICmd_LoadBattleType,
        AICmd_LoadRecycleItem,
        AICmd_LoadTypeOfLoadedMove,
        AICmd_LoadPowerOfLoadedMove,
        AICmd_LoadEffectOfLoadedMove,
        AICmd_LoadProtectChain,
        AICmd_PushAndGoTo,
        AICmd_GoTo,
        AICmd_PopOrEnd,
        AICmd_IfLevel,
        AICmd_IfTargetIsTaunted,
        AICmd_IfTargetIsNotTaunted,
        AICmd_IfTargetIsPartner,
        AICmd_FlagBattlerIsType,
        AICmd_CheckBattlerAbility,
        AICmd_IfActivatedFlashFire,
        AICmd_IfHeldItemEqualTo,
        AICmd_IfFieldConditionsMask,
        AICmd_LoadSpikesLayers,
        AICmd_IfAnyPartyMemberIsWounded,
        AICmd_IfAnyPartyMemberUsedPP,
        AICmd_LoadFlingPower,
        AICmd_LoadCurrentMovePP,
        AICmd_IfCanUseLastResort,
        AICmd_LoadCurrentMoveClass,
        AICmd_LoadDefenderLastUsedMoveClass,
        AICmd_LoadBattlerSpeedRank,
        AICmd_LoadBattlerTurnCount,
        AICmd_IfPartyMemberDealsMoreDamage,
        AICmd_IfHasSuperEffectiveMove,
        AICmd_IfBattlerDealsMoreDamage,
        AICmd_SumPositiveStatStages,
        AICmd_DiffStatStages,
        AICmd_IfBattlerHasHigherStat,
        AICmd_IfBattlerHasLowerStat,
        AICmd_IfBattlerHasEqualStat,
        AICmd_CheckIfHighestDamageWithPartner,
        AICmd_IfBattlerFainted,
        AICmd_IfBattlerNotFainted,
        AICmd_LoadAbility,
    };

    private static int AI_Random(BattleSystem battleSys)
    {
        return (int)(BattleSystems.BattleSystem_RandNext(battleSys) % 256);
    }

    private static uint AI_HPPercent(BattleContext battleCtx, int battler)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        return (uint)(mon.CurHP * 100) / mon.MaxHP;
    }

    // OFFSET 0x02220184
    private static void AICmd_IfRandomLessThan(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int value = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (AI_Random(battleSys) < value)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x022201C4
    private static void AICmd_IfRandomGreaterThan(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int value = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (AI_Random(battleSys) > value)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02220204
    private static void AICmd_IfRandomEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int value = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (value == AI_Random(battleSys))
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02220244
    private static void AICmd_IfRandomNotEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int value = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (value != AI_Random(battleSys))
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02220284
    private static void AICmd_AddToMoveScore(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        int value = AIScript_Read(battleCtx);
        ai.MoveScore[ai.MoveSlot] = (sbyte)(ai.MoveScore[ai.MoveSlot] + value);
        if (ai.MoveScore[ai.MoveSlot] < 0)
        {
            ai.MoveScore[ai.MoveSlot] = 0;
        }
    }

    private static void AI_IfHPPercent(BattleContext battleCtx, Func<uint, uint, bool> compare)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        uint value = (uint)AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        if (compare(AI_HPPercent(battleCtx, battler), value))
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x022202B8
    private static void AICmd_IfHPPercentLessThan(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfHPPercent(battleCtx, (percent, value) => percent < value);
    }

    // OFFSET 0x02220310
    private static void AICmd_IfHPPercentGreaterThan(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfHPPercent(battleCtx, (percent, value) => percent > value);
    }

    // OFFSET 0x02220368
    private static void AICmd_IfHPPercentEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfHPPercent(battleCtx, (percent, value) => percent == value);
    }

    // OFFSET 0x022203C0
    private static void AICmd_IfHPPercentNotEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfHPPercent(battleCtx, (percent, value) => percent != value);
    }

    private static void AI_IfBattlerMask(BattleContext battleCtx, Func<BattleMon, uint> field, bool set)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        uint mask = (uint)AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        if (((field(battleCtx.BattleMons[battler]) & mask) != 0) == set)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02220418
    private static void AICmd_IfStatus(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfBattlerMask(battleCtx, mon => mon.Status, true);
    }

    // OFFSET 0x02220464
    private static void AICmd_IfNotStatus(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfBattlerMask(battleCtx, mon => mon.Status, false);
    }

    // OFFSET 0x022204B0
    private static void AICmd_IfVolatileStatus(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfBattlerMask(battleCtx, mon => mon.StatusVolatile, true);
    }

    // OFFSET 0x022204FC
    private static void AICmd_IfNotVolatileStatus(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfBattlerMask(battleCtx, mon => mon.StatusVolatile, false);
    }

    // OFFSET 0x02220548
    private static void AICmd_IfMoveEffect(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfBattlerMask(battleCtx, mon => mon.MoveEffectsMask, true);
    }

    // OFFSET 0x02220590
    private static void AICmd_IfNotMoveEffect(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfBattlerMask(battleCtx, mon => mon.MoveEffectsMask, false);
    }

    private static void AI_IfSideCondition(BattleSystem battleSys, BattleContext battleCtx, bool set)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        uint mask = (uint)AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler);
        if (((battleCtx.SideConditionsMask[side] & mask) != 0) == set)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x022205D8
    private static void AICmd_IfSideCondition(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfSideCondition(battleSys, battleCtx, true);
    }

    // OFFSET 0x02220628
    private static void AICmd_IfNotSideCondition(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfSideCondition(battleSys, battleCtx, false);
    }

    private static void AI_IfLoaded(BattleContext battleCtx, Func<int, int, bool> compare)
    {
        AIScript_Iter(battleCtx, 1);
        int value = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (compare(battleCtx.AiContext.CalcTemp, value))
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02220678
    private static void AICmd_IfLoadedLessThan(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfLoaded(battleCtx, (temp, value) => temp < value);
    }

    // OFFSET 0x022206A8
    private static void AICmd_IfLoadedGreaterThan(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfLoaded(battleCtx, (temp, value) => temp > value);
    }

    // OFFSET 0x022206D8
    private static void AICmd_IfLoadedEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfLoaded(battleCtx, (temp, value) => temp == value);
    }

    // OFFSET 0x02220708
    private static void AICmd_IfLoadedNotEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfLoaded(battleCtx, (temp, value) => temp != value);
    }

    // OFFSET 0x02220738
    private static void AICmd_IfLoadedMask(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfLoaded(battleCtx, (temp, value) => (temp & value) != 0);
    }

    // OFFSET 0x02220768
    private static void AICmd_IfLoadedNotMask(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfLoaded(battleCtx, (temp, value) => (temp & value) == 0);
    }

    // OFFSET 0x02220798
    private static void AICmd_IfMoveEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int move = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (battleCtx.AiContext.Move == move)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x022207C8
    private static void AICmd_IfMoveNotEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int move = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (battleCtx.AiContext.Move != move)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x022207F8
    private static void AICmd_IfLoadedInTable(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int table = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        int value = AIScript_ReadOffset(battleCtx, table);
        while (value != -1)
        {
            if (battleCtx.AiContext.CalcTemp == value)
            {
                AIScript_Iter(battleCtx, jump);
                return;
            }
            table++;
            value = AIScript_ReadOffset(battleCtx, table);
        }
    }

    // OFFSET 0x0222084C
    private static void AICmd_IfLoadedNotInTable(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int table = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        int value = AIScript_ReadOffset(battleCtx, table);
        while (value != -1)
        {
            if (battleCtx.AiContext.CalcTemp == value)
            {
                return;
            }
            table++;
            value = AIScript_ReadOffset(battleCtx, table);
        }
        AIScript_Iter(battleCtx, jump);
    }

    private static int AI_FirstDamagingMove(BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        BattleMon mon = battleCtx.BattleMons[ai.Attacker];
        int i;
        for (i = 0; i < MaxMoves; i++)
        {
            if (mon.Moves[i] != 0 && ai.MoveTable[mon.Moves[i]].Power != 0)
            {
                break;
            }
        }
        return i;
    }

    // OFFSET 0x0222089C
    private static void AICmd_IfAttackerHasDamagingMoves(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int jump = AIScript_Read(battleCtx);
        if (AI_FirstDamagingMove(battleCtx) < MaxMoves)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02220900
    private static void AICmd_IfAttackerHasNoDamagingMoves(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int jump = AIScript_Read(battleCtx);
        if (AI_FirstDamagingMove(battleCtx) == MaxMoves)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02220964
    private static void AICmd_LoadTurnCount(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        battleCtx.AiContext.CalcTemp = battleCtx.TotalTurns;
    }

    // OFFSET 0x02220980
    private static void AICmd_LoadTypeFrom(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        switch (AIScript_Read(battleCtx))
        {
            case 1:
                ai.CalcTemp = BattleContexts.BattleMon_Get(battleCtx, ai.Attacker, BattleContexts.BattleMonType1, null);
                break;
            case 0:
                ai.CalcTemp = BattleContexts.BattleMon_Get(battleCtx, ai.Defender, BattleContexts.BattleMonType1, null);
                break;
            case 3:
                ai.CalcTemp = BattleContexts.BattleMon_Get(battleCtx, ai.Attacker, BattleContexts.BattleMonType2, null);
                break;
            case 2:
                ai.CalcTemp = BattleContexts.BattleMon_Get(battleCtx, ai.Defender, BattleContexts.BattleMonType2, null);
                break;
            case 4:
                ai.CalcTemp = ai.MoveTable[ai.Move].Type;
                break;
            case 6:
                ai.CalcTemp = BattleContexts.BattleMon_Get(battleCtx, BattleSystems.BattleSystem_GetPartner(battleSys, ai.Attacker), BattleContexts.BattleMonType1, null);
                break;
            case 5:
                ai.CalcTemp = BattleContexts.BattleMon_Get(battleCtx, BattleSystems.BattleSystem_GetPartner(battleSys, ai.Defender), BattleContexts.BattleMonType1, null);
                break;
            case 8:
                ai.CalcTemp = BattleContexts.BattleMon_Get(battleCtx, BattleSystems.BattleSystem_GetPartner(battleSys, ai.Attacker), BattleContexts.BattleMonType2, null);
                break;
            case 7:
                ai.CalcTemp = BattleContexts.BattleMon_Get(battleCtx, BattleSystems.BattleSystem_GetPartner(battleSys, ai.Defender), BattleContexts.BattleMonType1, null);
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
    }

    // OFFSET 0x02220AB4
    private static void AICmd_FlagBattlerIsType(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int type = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        if (type == BattleContexts.BattleMon_Get(battleCtx, battler, BattleContexts.BattleMonType1, null)
            || type == BattleContexts.BattleMon_Get(battleCtx, battler, BattleContexts.BattleMonType2, null))
        {
            battleCtx.AiContext.CalcTemp = 1;
        }
        else
        {
            battleCtx.AiContext.CalcTemp = 0;
        }
    }

    // OFFSET 0x02220B10
    private static void AICmd_LoadMovePower(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        battleCtx.AiContext.CalcTemp = battleCtx.AiContext.MoveTable[battleCtx.AiContext.Move].Power;
    }

    private static byte[] AI_BattlerIVs(BattleContext battleCtx, int battler)
    {
        byte[] ivs = new byte[6];
        for (int i = 0; i < 6; i++)
        {
            ivs[i] = (byte)BattleContexts.BattleMon_Get(battleCtx, battler, BattleContexts.BattleMonHpIV + i, null);
        }
        return ivs;
    }

    // OFFSET 0x02220B34
    private static void AICmd_FlagMoveDamageScore(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        int varyDamage = AIScript_Read(battleCtx);
        if (!AI_MoveDealsDamage(ai, ai.Move))
        {
            ai.CalcTemp = DamageScoreNone;
            return;
        }
        byte[] ivs = AI_BattlerIVs(battleCtx, ai.Attacker);
        int attacker = ai.Attacker;
        BattleMon mon = battleCtx.BattleMons[attacker];
        int[] damage = new int[MaxMoves];
        int ability = BattleContexts.Battler_Ability(battleCtx, attacker);
        TrainerAI_CalcAllDamage(battleSys, battleCtx, attacker, mon.Moves, damage, mon.HeldItem, ivs, ability, mon.MoveEffectsData.EmbargoTurns, varyDamage);
        int current = damage[ai.MoveSlot];
        int i;
        for (i = 0; i < MaxMoves; i++)
        {
            if (damage[i] > current)
            {
                break;
            }
        }
        ai.CalcTemp = i == MaxMoves ? DamageScoreHighest : DamageScoreNotHighest;
    }

    // OFFSET 0x02220C70
    private static void AICmd_LoadBattlerPreviousMove(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        byte battler = AIScript_Battler(battleCtx, (byte)AIScript_Read(battleCtx));
        battleCtx.AiContext.CalcTemp = battleCtx.MovePrevByBattler[battler];
    }

    // OFFSET 0x02220CA4
    private static void AICmd_IfTempEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int value = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (value == battleCtx.AiContext.CalcTemp)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02220CD4
    private static void AICmd_IfTempNotEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int value = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (value != battleCtx.AiContext.CalcTemp)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02220D04
    private static void AICmd_IfSpeedCompareEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int value = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (value == BattleContexts.BattleSystem_CompareBattlerSpeed(battleSys, battleCtx, battleCtx.AiContext.Attacker, battleCtx.AiContext.Defender, 1))
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02220D48
    private static void AICmd_IfSpeedCompareNotEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int value = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (value != BattleContexts.BattleSystem_CompareBattlerSpeed(battleSys, battleCtx, battleCtx.AiContext.Attacker, battleCtx.AiContext.Defender, 1))
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02220D8C
    private static void AICmd_CountAlivePartyBattlers(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        battleCtx.AiContext.CalcTemp = 0;
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        Party party = BattleSystems.BattleSystem_GetParty(battleSys, battler);
        int slot1;
        int slot2;
        if ((battleSys.BattleType & BattleSystems.BattleTypeDoubles) != 0)
        {
            slot1 = battleCtx.SelectedPartySlot[battler];
            slot2 = battleCtx.SelectedPartySlot[BattleSystems.BattleSystem_GetPartner(battleSys, battler)];
        }
        else
        {
            slot1 = battleCtx.SelectedPartySlot[battler];
            slot2 = slot1;
        }
        for (int i = 0; i < BattleSystems.BattleSystem_GetPartyCount(battleSys, battler); i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, i);
            if (i != slot1 && i != slot2
                && Pokemon.Pokemon_GetValue(mon, MonDataCurHP, null) != 0
                && Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEgg, null) != 0
                && Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEgg, null) != SpeciesEgg)
            {
                battleCtx.AiContext.CalcTemp++;
            }
        }
    }

    // OFFSET 0x02220E68
    private static void AICmd_LoadCurrentMove(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        battleCtx.AiContext.CalcTemp = battleCtx.AiContext.Move;
    }

    // OFFSET 0x02220E84
    private static void AICmd_LoadCurrentMoveEffect(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        battleCtx.AiContext.CalcTemp = battleCtx.AiContext.MoveTable[battleCtx.AiContext.Move].Effect;
    }

    // OFFSET 0x02220EA8
    private static void AICmd_LoadBattlerAbility(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        BattleMon mon = battleCtx.BattleMons[battler];
        if ((mon.MoveEffectsMask & MoveEffectGastroAcid) != 0)
        {
            ai.CalcTemp = 0;
            return;
        }
        if (ai.Attacker == battler || inBattler == AIBattlerAttackerPartner)
        {
            ai.CalcTemp = mon.Ability;
            return;
        }
        if (ai.BattlerAbilities[battler] != 0)
        {
            ai.CalcTemp = ai.BattlerAbilities[battler];
            return;
        }
        if (mon.Ability == AbilityShadowTag || mon.Ability == AbilityMagnetPull || mon.Ability == AbilityArenaTrap)
        {
            ai.CalcTemp = mon.Ability;
            return;
        }
        int ability1 = (int)SpeciesData.SpeciesData_GetSpeciesValue(mon.Species, SpeciesDataAbility1);
        int ability2 = (int)SpeciesData.SpeciesData_GetSpeciesValue(mon.Species, SpeciesDataAbility2);
        if (ability1 != 0 && ability2 != 0)
        {
            ai.CalcTemp = (BattleSystems.BattleSystem_RandNext(battleSys) & 1) != 0 ? ability1 : ability2;
        }
        else if (ability1 != 0)
        {
            ai.CalcTemp = ability1;
        }
        else
        {
            ai.CalcTemp = ability2;
        }
    }
}
