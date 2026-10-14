namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const int AbilityBattleArmor = 4;
    private const int AbilityShellArmor = 0x4B;
    private const int AbilitySniper = 0x61;
    private const int AbilitySuperLuck = 0x69;
    private const int HoldEffectCritBoost = 0x43;
    private const int HoldEffectChanseyCritBoost = 0x59;
    private const int HoldEffectFarfetchdCritBoost = 0x5C;
    private const int HoldEffectShellBell = 0x58;
    private const int HoldEffectLifeOrb = 0x62;
    private const int SpeciesChansey = 0x71;
    private const int SpeciesFarfetchd = 0x53;
    private const uint SideConditionLuckyChant = 0x7000;
    private const uint MoveEffectNoCritical = 0x8000;
    private const uint MoveEffectSemiInvulnerable = 0x200400C0;
    private const uint BattleStatusMoveHit = 0x2000;
    private const int MaxCriticalStage = 4;
    private const uint CriticalRateTable = 0x0226EBA0;
    private const uint MoveBanTable = 0x0226EC5C;
    private const int MoveBanTableMaxEntries = 0x40;
    private const ushort MoveBanMimicEnd = 0xFFFE;
    private const ushort MoveBanMetronomeEnd = 0xFFFF;
    private const uint EncoreBanTable = 0x0226EBB0;
    private const uint MeFirstBanTable = 0x0226EBC8;
    private const int EffectBanCount = 6;
    private const uint SubstituteVisibleEffectTable = 0x0226EC34;
    private const int SubstituteVisibleEffectCount = 10;
    private const int MoveClassStatus = 2;
    private const int SubscriptLifeOrbRecoil = 0xD6;
    private const int CommandRecordingStopped = 0x2A;

    private static byte[]? criticalRates;

    // OFFSET 0x0225AEB8
    public static int BattleSystem_CalcDamageVariance(BattleSystem battleSys, BattleContext battleCtx, int damage)
    {
        if (damage != 0)
        {
            damage = damage * (100 - BattleSystems.BattleSystem_RandNext(battleSys) % 16) / 100;
            if (damage == 0)
            {
                damage = 1;
            }
        }
        return damage;
    }

    // OFFSET 0x0225AEE4
    public static int BattleSystem_CalcCriticalMulti(BattleSystem battleSys, BattleContext battleCtx, int attacker, int defender, int criticalStage, uint sideConditions)
    {
        int multiplier = 1;
        int itemEffect = BattleSystem_GetItemData(battleCtx, Battler_HeldItem(battleCtx, attacker), ItemParamHoldEffect);
        BattleMon mon = battleCtx.BattleMons[attacker];
        uint defenderEffects = battleCtx.BattleMons[defender].MoveEffectsMask;
        int stage = (mon.Ability == AbilitySuperLuck ? 1 : 0)
            + criticalStage
            + (itemEffect == HoldEffectCritBoost ? 1 : 0)
            + ((mon.StatusVolatile & VolatileFocusEnergy) != 0 ? 2 : 0)
            + (itemEffect == HoldEffectChanseyCritBoost && mon.Species == SpeciesChansey ? 2 : 0)
            + (itemEffect == HoldEffectFarfetchdCritBoost && mon.Species == SpeciesFarfetchd ? 2 : 0);
        stage = (ushort)stage;
        if (stage > MaxCriticalStage)
        {
            stage = MaxCriticalStage;
        }
        criticalRates ??= RomData.Overlay(16, CriticalRateTable, MaxCriticalStage + 1);
        if (BattleSystems.BattleSystem_RandNext(battleSys) % criticalRates[stage] == 0)
        {
            if (Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityBattleArmor) == 0
                && Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityShellArmor) == 0
                && (sideConditions & SideConditionLuckyChant) == 0
                && (defenderEffects & MoveEffectNoCritical) == 0)
            {
                multiplier = 2;
            }
        }
        if (multiplier == 2 && Battler_Ability(battleCtx, attacker) == AbilitySniper)
        {
            multiplier = 3;
        }
        return multiplier;
    }

    private static int MoveBanTable_ReachesTerminator(ushort move, ushort terminator)
    {
        ushort[] table = RomData.OverlayU16(16, MoveBanTable, MoveBanTableMaxEntries);
        int i = 0;
        while (true)
        {
            if (move == table[i])
            {
                break;
            }
            i++;
            if (table[i] == terminator)
            {
                break;
            }
        }
        return table[i] == terminator ? 1 : 0;
    }

    // OFFSET 0x0225AFF4
    public static int Move_CanBeMimicked(ushort move)
    {
        return MoveBanTable_ReachesTerminator(move, MoveBanMimicEnd);
    }

    // OFFSET 0x0225B02C
    public static int Move_CanBeMetronomed(BattleSystem battleSys, BattleContext battleCtx, int battler, ushort move)
    {
        if (Move_FailsInHighGravity(battleSys, battleCtx, battler, move) == 1 || Move_HealBlocked(battleSys, battleCtx, battler, move) == 1)
        {
            return 0;
        }
        return MoveBanTable_ReachesTerminator(move, MoveBanMetronomeEnd);
    }

    private static int Move_EffectNotInTable(BattleContext battleCtx, ushort move, uint table)
    {
        ushort[] moves = RomData.OverlayU16(16, table, EffectBanCount);
        int effect = battleCtx.AiContext.MoveTable[move].Effect;
        int i;
        for (i = 0; i < EffectBanCount; i++)
        {
            if (effect == battleCtx.AiContext.MoveTable[moves[i]].Effect)
            {
                break;
            }
        }
        return i == EffectBanCount ? 1 : 0;
    }

    // OFFSET 0x0225B084
    public static int Move_CanBeEncored(BattleContext battleCtx, ushort move)
    {
        return Move_EffectNotInTable(battleCtx, move, EncoreBanTable);
    }

    // OFFSET 0x0225B0C0
    public static int Move_CanBeMeFirsted(BattleContext battleCtx, ushort move)
    {
        return Move_EffectNotInTable(battleCtx, move, MeFirstBanTable);
    }

    // OFFSET 0x0225B0FC
    public static int BattleSystem_GetItemData(BattleContext battleCtx, ushort item, int param)
    {
        return Item.Item_Get(battleCtx.AiContext.ItemTable!, Item.ItemTable_Index(Item.Item_FileID(item, 0)), param);
    }

    // OFFSET 0x0225B120
    public static int BattleSystem_SideToBattler(BattleSystem battleSys, BattleContext battleCtx, int side)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        int battler;
        for (battler = 0; battler < maxBattlers; battler++)
        {
            if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) == side)
            {
                break;
            }
        }
        return battler;
    }

    // OFFSET 0x0225B148
    public static void BattleSystem_SortBattlerActionOrder(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        for (int i = 0; i < maxBattlers - 1; i++)
        {
            for (int j = i + 1; j < maxBattlers; j++)
            {
                byte battler1 = battleCtx.BattlerActionOrder[i];
                byte battler2 = battleCtx.BattlerActionOrder[j];
                if (battleCtx.BattlerActions[battler1, 3] == battleCtx.BattlerActions[battler2, 3])
                {
                    int ignoreQuickClaw = battleCtx.BattlerActions[battler1, 3] == ActionFight ? 0 : 1;
                    if (BattleSystem_CompareBattlerSpeed(battleSys, battleCtx, battler1, battler2, ignoreQuickClaw) != 0)
                    {
                        battleCtx.BattlerActionOrder[i] = battler2;
                        battleCtx.BattlerActionOrder[j] = battler1;
                    }
                }
            }
        }
    }

    // OFFSET 0x0225B1DC
    public static int Battler_CanShowStatusEffect(BattleContext battleCtx, int battler, int effect)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        if ((mon.StatusVolatile & VolatileSubstitute) == 0 && (mon.MoveEffectsMask & MoveEffectSemiInvulnerable) == 0)
        {
            return 1;
        }
        uint[] effects = RomData.OverlayU32(16, SubstituteVisibleEffectTable, SubstituteVisibleEffectCount);
        for (int i = 0; i < SubstituteVisibleEffectCount; i++)
        {
            if (effect == effects[i])
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0225B228
    public static int BattleSystem_TriggerHeldItemAfterMove(BattleSystem battleSys, BattleContext battleCtx, ref int script)
    {
        int result = 0;
        int attackerEffect = Battler_HeldItemEffect(battleCtx, battleCtx.Attacker);
        int attackerPower = Battler_HeldItemPower(battleCtx, battleCtx.Attacker, 0);
        int defenderEffect = Battler_HeldItemEffect(battleCtx, battleCtx.Defender);
        int defenderPower = Battler_HeldItemPower(battleCtx, battleCtx.Defender, 0);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
        BattleMon attacker = battleCtx.BattleMons[battleCtx.Attacker];
        SelfTurnFlags attackerFlags = battleCtx.SelfTurnFlags[battleCtx.Attacker];
        SelfTurnFlags defenderFlags = battleCtx.SelfTurnFlags[battleCtx.Defender];
        if (attackerEffect == HoldEffectShellBell && (battleCtx.BattleStatusMask & BattleStatusMoveHit) != 0
            && attackerFlags.ShellBellDamageDealt != 0 && battleCtx.Attacker != battleCtx.Defender
            && (uint)attacker.CurHP < attacker.MaxHP && attacker.CurHP != 0)
        {
            battleCtx.HpCalcTemp = BattleSystem_Divide(-attackerFlags.ShellBellDamageDealt, attackerPower);
            battleCtx.MsgBattlerTemp = battleCtx.Attacker;
            script = SubscriptItemRecoverHP;
            result = 1;
        }
        if (attackerEffect == HoldEffectLifeOrb && Battler_Ability(battleCtx, battleCtx.Attacker) != AbilityMagicGuard
            && (battleCtx.BattleStatusMask & BattleStatusMoveHit) != 0
            && battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Class != MoveClassStatus
            && attacker.CurHP != 0)
        {
            battleCtx.HpCalcTemp = BattleSystem_Divide((int)attacker.MaxHP * -1, 10);
            battleCtx.MsgBattlerTemp = battleCtx.Attacker;
            script = SubscriptLifeOrbRecoil;
            result = 1;
        }
        if (defenderEffect == HoldEffectJabocaBerry && attacker.CurHP != 0 && Battler_Ability(battleCtx, battleCtx.Attacker) != AbilityMagicGuard
            && defenderFlags.PhysicalDamageTaken != 0)
        {
            battleCtx.HpCalcTemp = BattleSystem_Divide((int)attacker.MaxHP * -1, defenderPower);
            script = SubscriptItemRecoilDamage;
            result = 1;
        }
        if (defenderEffect == HoldEffectStickyBarb && attacker.CurHP != 0 && attacker.HeldItem == 0
            && (HealthBar.FlagIndex(battleCtx.SelectedPartySlot[battleCtx.Attacker]) & (uint)battleCtx.SideConditions[side].KnockedOffItemsMask) == 0
            && (defenderFlags.PhysicalDamageTaken != 0 || defenderFlags.SpecialDamageTaken != 0)
            && (battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Flags & MoveFlagContact) != 0)
        {
            script = SubscriptStickyBarbTransfer;
            result = 1;
        }
        return result;
    }

    // OFFSET 0x0225B408
    public static void BattleSystem_DecPPForPressure(BattleContext battleCtx, int attacker, int defender)
    {
        if (defender != BattlerNone && Battler_Ability(battleCtx, defender) == AbilityPressure)
        {
            int slot = battleCtx.MoveSlot[attacker];
            if (battleCtx.BattleMons[attacker].PpCur[slot] != 0)
            {
                battleCtx.BattleMons[attacker].PpCur[slot]--;
            }
        }
    }

    // OFFSET 0x0225B444
    public static int BattleSystem_CheckRecordingStopped(BattleSystem battleSys, BattleContext battleCtx)
    {
        if (BattleSystems.BattleSystem_RecordingStopped(battleSys) != 0)
        {
            battleCtx.Command = CommandRecordingStopped;
            return 1;
        }
        return 0;
    }
}
