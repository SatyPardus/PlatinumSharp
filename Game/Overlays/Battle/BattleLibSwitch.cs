namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const int BattlerActionSwitchedIn = 0x27;
    private const uint VolatileMeanLook = 0x4000000;
    private const uint VolatileBind = 0xE000;
    private const uint VolatileFlinch = 0x8;
    private const uint VolatileBatonPassMask = 0x15100007;
    private const uint MoveEffectLockOn = 0x18;
    private const uint MoveEffectLockOnPassed = 0x10;
    private const uint MoveEffectBatonPassMask = 0x0FA3843F;
    private const uint MoveEffectPowerTrick = 0x800000;
    private const uint MonConditionSleep = 0x7;
    private const uint BattlerStatusSwitching = 0x1;

    private static void MoveEffectsData_Clear(MoveEffectsData effects)
    {
        effects.DisabledTurns = 0;
        effects.EncoredTurns = 0;
        effects.ChargedTurns = 0;
        effects.TauntedTurns = 0;
        effects.ProtectSuccessTurns = 0;
        effects.PerishSongTurns = 0;
        effects.RolloutCount = 0;
        effects.FuryCutterCount = 0;
        effects.StockpileCount = 0;
        effects.StockpileDefBoosts = 0;
        effects.StockpileSpDefBoosts = 0;
        effects.Truant = 0;
        effects.FlashFire = 0;
        effects.LockOnTarget = 0;
        effects.MimickedMoveSlot = 0;
        effects.BindTarget = 0;
        effects.MeanLookTarget = 0;
        effects.LastResortCount = 0;
        effects.MagnetRiseTurns = 0;
        effects.HealBlockTurns = 0;
        effects.EmbargoTurns = 0;
        effects.CanUnburden = 0;
        effects.MetronomeTurns = 0;
        effects.MicleBerry = 0;
        effects.CustapBerry = 0;
        effects.QuickClaw = 0;
        effects.MeFirst = 0;
        effects.Padding00041F = 0;
        effects.RechargeTurnNumber = 0;
        effects.FakeOutTurnNumber = 0;
        effects.SlowStartTurnNumber = 0;
        effects.MeFirstTurnNumber = 0;
        effects.SubstituteHP = 0;
        effects.TransformedPID = 0;
        effects.DisabledMove = 0;
        effects.BindingMove = 0;
        effects.EncoredMove = 0;
        effects.EncoredMoveSlot = 0;
        for (int i = 0; i < effects.LastResortMoves.Length; i++)
        {
            effects.LastResortMoves[i] = 0;
        }
        effects.ChoiceLockedMove = 0;
        effects.TransformedGender = 0;
        effects.ItemHPRecovery = 0;
    }

    private static void TurnFlags_Clear(TurnFlags flags)
    {
        flags.Struggling = 0;
        flags.PpDecremented = 0;
        flags.Protecting = 0;
        flags.HelpingHand = 0;
        flags.MagicCoat = 0;
        flags.Snatching = 0;
        flags.Roosting = 0;
        flags.Fleeing = 0;
        flags.Enduring = 0;
        flags.Padding000A = 0;
        for (int i = 0; i < 4; i++)
        {
            flags.PhysicalDamageTakenFrom[i] = 0;
            flags.SpecialDamageTakenFrom[i] = 0;
        }
        flags.PhysicalDamageLastAttacker = 0;
        flags.PhysicalDamageAttackerMask = 0;
        flags.SpecialDamageLastAttacker = 0;
        flags.SpecialDamageAttackerMask = 0;
        flags.LastDamageTaken = 0;
        flags.LastAttacker = 0;
        flags.AssuranceDamageMask = 0;
    }

    private static void MoveFailFlags_Clear(MoveFailFlags flags)
    {
        flags.Paralyzed = 0;
        flags.NoEffect = 0;
        flags.Imprisoned = 0;
        flags.Infatuated = 0;
        flags.Disabled = 0;
        flags.Taunted = 0;
        flags.Flinched = 0;
        flags.Confused = 0;
        flags.Gravity = 0;
        flags.HealBlocked = 0;
        flags.Padding000A = 0;
    }

    private static void BattleContext_ResetBattlerMoveMemory(BattleSystem battleSys, BattleContext battleCtx, int battler, int maxBattlers)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        mon.MoveEffectsData.FakeOutTurnNumber = battleCtx.TotalTurns + 1;
        mon.MoveEffectsData.SlowStartTurnNumber = battleCtx.TotalTurns + 1;
        mon.MoveEffectsData.Truant = (battleCtx.TotalTurns + 1) & 1;
        battleCtx.MoveProtect[battler] = 0;
        battleCtx.MoveHit[battler] = 0;
        battleCtx.MoveHitBattler[battler] = BattlerNone;
        battleCtx.MoveHitType[battler] = 0;
        battleCtx.MovePrevByBattler[battler] = 0;
        battleCtx.MoveCopied[battler] = 0;
        for (int i = 0; i < 4; i++)
        {
            battleCtx.MoveCopiedHit[battler, i] = 0;
        }
        battleCtx.MoveSketched[battler] = 0;
        battleCtx.Conversion2Move[battler] = 0;
        battleCtx.Conversion2Battler[battler] = 0;
        battleCtx.Conversion2Type[battler] = 0;
        battleCtx.MetronomeMove[battler] = 0;
        battleCtx.FieldConditionsMask &= ~(HealthBar.FlagIndex(battler) << 8);
    }

    private static void BattleContext_ForgetCopiedMoves(BattleSystem battleSys, BattleContext battleCtx, int battler, int maxBattlers)
    {
        for (int i = 0; i < maxBattlers; i++)
        {
            if (i != battler && BattleSystems.BattleSystem_GetBattlerSide(battleSys, i) != BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler))
            {
                battleCtx.MoveCopied[i] = 0;
            }
            battleCtx.MoveCopiedHit[i, battler] = 0;
        }
    }

    // OFFSET 0x0225433C
    public static void BattleSystem_InitSwitchedInMon(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        MoveEffectsData effects = mon.MoveEffectsData;
        int substituteHP = effects.SubstituteHP;
        int lockOnTarget = effects.LockOnTarget;
        int perishSongTurns = effects.PerishSongTurns;
        int meanLookTarget = effects.MeanLookTarget;
        int magnetRiseTurns = effects.MagnetRiseTurns;
        int embargoTurns = effects.EmbargoTurns;
        int healBlockTurns = effects.HealBlockTurns;
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleSystems.BattleSystem_GetBattleType(battleSys);
        battleCtx.BattlerActions[battler, 0] = BattlerActionSwitchedIn;
        if ((battleCtx.BattleStatusMask & BattleStatusBatonPass) == 0)
        {
            for (int i = 0; i < maxBattlers; i++)
            {
                BattleMon other = battleCtx.BattleMons[i];
                if ((other.StatusVolatile & VolatileMeanLook) != 0 && other.MoveEffectsData.MeanLookTarget == battler)
                {
                    other.StatusVolatile &= ~VolatileMeanLook;
                }
                if ((other.MoveEffectsMask & MoveEffectLockOn) != 0 && other.MoveEffectsData.LockOnTarget == battler)
                {
                    other.MoveEffectsMask &= ~MoveEffectLockOn;
                    other.MoveEffectsData.LockOnTarget = 0;
                }
            }
            mon.StatusVolatile = 0;
            mon.MoveEffectsMask = 0;
        }
        else
        {
            mon.StatusVolatile &= VolatileBatonPassMask;
            mon.MoveEffectsMask &= MoveEffectBatonPassMask;
            for (int i = 0; i < maxBattlers; i++)
            {
                BattleMon other = battleCtx.BattleMons[i];
                if ((other.MoveEffectsMask & MoveEffectLockOn) != 0 && other.MoveEffectsData.LockOnTarget == battler)
                {
                    other.MoveEffectsMask &= ~MoveEffectLockOn;
                    other.MoveEffectsMask |= MoveEffectLockOnPassed;
                }
            }
        }
        for (int i = 0; i < maxBattlers; i++)
        {
            BattleMon other = battleCtx.BattleMons[i];
            if ((other.StatusVolatile & (HealthBar.FlagIndex(battler) << 16)) != 0)
            {
                other.StatusVolatile &= ~(HealthBar.FlagIndex(battler) << 16);
            }
            if ((other.StatusVolatile & VolatileBind) != 0 && other.MoveEffectsData.BindTarget == battler)
            {
                other.StatusVolatile &= ~VolatileBind;
            }
        }
        MoveEffectsData_Clear(effects);
        if ((battleCtx.BattleStatusMask & BattleStatusBatonPass) != 0)
        {
            effects.SubstituteHP = substituteHP;
            effects.LockOnTarget = lockOnTarget;
            effects.PerishSongTurns = perishSongTurns;
            effects.MeanLookTarget = meanLookTarget;
            effects.MagnetRiseTurns = magnetRiseTurns;
            effects.EmbargoTurns = embargoTurns;
            effects.HealBlockTurns = healBlockTurns;
        }
        BattleContext_ResetBattlerMoveMemory(battleSys, battleCtx, battler, maxBattlers);
        if ((mon.MoveEffectsMask & MoveEffectPowerTrick) != 0)
        {
            (mon.Attack, mon.Defense) = (mon.Defense, mon.Attack);
        }
        BattleContext_ForgetCopiedMoves(battleSys, battleCtx, battler, maxBattlers);
        AI_ClearKnownMoves(battleCtx, (byte)battler);
        AI_ClearKnownAbility(battleCtx, (byte)battler);
        AI_ClearKnownItem(battleCtx, (byte)battler);
    }

    // OFFSET 0x02254744
    public static void BattleSystem_CleanupFaintedMon(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        for (int i = 0; i < mon.StatBoosts.Length; i++)
        {
            mon.StatBoosts[i] = 6;
        }
        mon.StatusVolatile = 0;
        mon.MoveEffectsMask = 0;
        for (int i = 0; i < maxBattlers; i++)
        {
            BattleMon other = battleCtx.BattleMons[i];
            if ((other.StatusVolatile & VolatileMeanLook) != 0 && other.MoveEffectsData.MeanLookTarget == battler)
            {
                other.StatusVolatile &= ~VolatileMeanLook;
            }
            if ((other.StatusVolatile & (HealthBar.FlagIndex(battler) << 16)) != 0)
            {
                other.StatusVolatile &= ~(HealthBar.FlagIndex(battler) << 16);
            }
            if ((other.StatusVolatile & VolatileBind) != 0 && other.MoveEffectsData.BindTarget == battler)
            {
                other.StatusVolatile &= ~VolatileBind;
            }
        }
        MoveEffectsData_Clear(mon.MoveEffectsData);
        TurnFlags_Clear(battleCtx.TurnFlags[battler]);
        BattleContext_ResetBattlerMoveMemory(battleSys, battleCtx, battler, maxBattlers);
        BattleContext_ForgetCopiedMoves(battleSys, battleCtx, battler, maxBattlers);
        battleCtx.BattlerStatusFlags[battler] &= ~BattlerStatusSwitching;
        AI_ClearKnownMoves(battleCtx, (byte)battler);
        AI_ClearKnownAbility(battleCtx, (byte)battler);
        AI_ClearKnownItem(battleCtx, (byte)battler);
    }

    // OFFSET 0x02254990
    public static void BattleSystem_InitTurnFlags(BattleSystem battleSys, BattleContext battleCtx)
    {
        for (int i = 0; i < 4; i++)
        {
            BattleMon mon = battleCtx.BattleMons[i];
            TurnFlags_Clear(battleCtx.TurnFlags[i]);
            MoveFailFlags_Clear(battleCtx.MoveFailFlags[i]);
            mon.StatusVolatile &= ~VolatileFlinch;
            if (mon.MoveEffectsData.RechargeTurnNumber + 1 < battleCtx.TotalTurns)
            {
                mon.StatusVolatile &= ~VolatileRecharging;
            }
            if ((mon.Status & MonConditionSleep) != 0 && (mon.StatusVolatile & VolatileMoveLocked) != 0)
            {
                Battler_UnlockMoveChoice(battleSys, battleCtx, i);
            }
            if ((mon.Status & MonConditionSleep) != 0 && (mon.StatusVolatile & VolatileRampage) != 0)
            {
                mon.StatusVolatile &= ~VolatileRampage;
            }
        }
        battleCtx.SideConditions[0].FollowMe = 0;
        battleCtx.SideConditions[1].FollowMe = 0;
    }

    // OFFSET 0x0225B80C
    public static void AI_ClearKnownMoves(BattleContext battleCtx, byte battler)
    {
        for (int i = 0; i < 4; i++)
        {
            battleCtx.AiContext.BattlerMoves[battler, i] = 0;
        }
    }

    // OFFSET 0x0225B824
    public static void AI_ClearKnownAbility(BattleContext battleCtx, byte battler)
    {
        battleCtx.AiContext.BattlerAbilities[battler] = 0;
    }

    // OFFSET 0x0225B830
    public static void AI_ClearKnownItem(BattleContext battleCtx, byte battler)
    {
        battleCtx.AiContext.BattlerHeldItems[battler] = 0;
    }
}
