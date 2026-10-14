namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    public const int CheckInvalidNoMove = 0x1;
    public const int CheckInvalidNoPP = 0x2;
    public const int CheckInvalidDisabled = 0x4;
    public const int CheckInvalidTorment = 0x8;
    public const int CheckInvalidTaunt = 0x10;
    public const int CheckInvalidImprison = 0x20;
    public const int CheckInvalidGravity = 0x40;
    public const int CheckInvalidHealBlock = 0x80;
    public const int CheckInvalidChoiceItem = 0x200;
    public const uint MoveStatusSuperEffective = 0x2;
    public const uint MoveStatusNotVeryEffective = 0x4;
    public const uint MoveStatusLevitated = 0x800;
    public const uint MoveStatusWonderGuard = 0x40000;
    public const uint MoveStatusMagnetRise = 0x100000;

    private const uint VolatileTorment = 0x80000000;
    private const uint VolatileForesight = 0x20000000;
    private const uint MoveEffectIngrain = 0x400;
    private const uint MoveEffectMiracleEye = 0x400000;
    private const uint FieldConditionGravity = 0x7000;
    private const int HoldEffectChoiceBand = 0x37;
    private const int HoldEffectChoiceSpecs = 0x7D;
    private const int HoldEffectExpertBelt = 0x60;
    private const int HoldEffectIronBall = 0x6A;
    private const int AbilityWonderGuard = 0x19;
    private const int AbilityLevitate = 0x1A;
    private const int AbilityAdaptability = 0x5B;
    private const int AbilityTintedLens = 0x6E;
    private const int AbilityFilter = 0x6F;
    private const int AbilityScrappy = 0x71;
    private const int AbilitySolidRock = 0x74;
    private const int TypeFlying = 2;
    private const int TypeGround = 4;
    private const int TypeDark = 0x11;
    private const int TypeChartEnd = 0xFF;
    private const int TypeChartForesight = 0xFE;
    private const uint TypeChartTable = 0x0226ECD4;
    private const int TypeChartMaxEntries = 0x71;
    private const int MoveSlotCount = 4;
    private const int MessageMoveDisabled = 0x261;
    private const int MessageMoveTormented = 0x264;
    private const int MessageMoveTaunted = 0x265;
    private const int MessageMoveImprisoned = 0x268;
    private const int MessageMoveGravity = 0x3E9;
    private const int MessageMoveHealBlock = 0x421;
    private const int MessageMoveChoiceItem = 0x38F;
    private const int MessageMoveNoPP = 0x337;
    private const int MoveHealBlock = 0x179;

    private static byte[]? typeChart;

    private static byte[] TypeChart()
    {
        return typeChart ??= RomData.Overlay(16, TypeChartTable, TypeChartMaxEntries * 3);
    }

    // OFFSET 0x02254A6C
    public static int BattleSystem_CheckInvalidMoves(BattleSystem battleSys, BattleContext battleCtx, int battler, int invalidMoves, int opMask)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        MoveEffectsData effects = mon.MoveEffectsData;
        int holdEffect = Battler_HeldItemEffect(battleCtx, battler);
        for (int slot = 0; slot < MoveSlotCount; slot++)
        {
            ushort move = mon.Moves[slot];
            if (move == 0 && (opMask & CheckInvalidNoMove) != 0)
            {
                invalidMoves |= (int)HealthBar.FlagIndex(slot);
            }
            if (mon.PpCur[slot] == 0 && (opMask & CheckInvalidNoPP) != 0)
            {
                invalidMoves |= (int)HealthBar.FlagIndex(slot);
            }
            if (effects.DisabledMove == move && (opMask & CheckInvalidDisabled) != 0)
            {
                invalidMoves |= (int)HealthBar.FlagIndex(slot);
            }
            if (battleCtx.MovePrevByBattler[battler] == move && (opMask & CheckInvalidTorment) != 0 && (mon.StatusVolatile & VolatileTorment) != 0)
            {
                invalidMoves |= (int)HealthBar.FlagIndex(slot);
            }
            if (effects.TauntedTurns != 0 && (opMask & CheckInvalidTaunt) != 0 && battleCtx.AiContext.MoveTable[move].Power == 0)
            {
                invalidMoves |= (int)HealthBar.FlagIndex(slot);
            }
            if (Move_Imprisoned(battleSys, battleCtx, battler, move) != 0 && (opMask & CheckInvalidImprison) != 0)
            {
                invalidMoves |= (int)HealthBar.FlagIndex(slot);
            }
            if (Move_FailsInHighGravity(battleSys, battleCtx, battler, move) != 0 && (opMask & CheckInvalidGravity) != 0)
            {
                invalidMoves |= (int)HealthBar.FlagIndex(slot);
            }
            if (Move_HealBlocked(battleSys, battleCtx, battler, move) != 0 && (opMask & CheckInvalidHealBlock) != 0)
            {
                invalidMoves |= (int)HealthBar.FlagIndex(slot);
            }
            if (effects.EncoredMove != 0 && effects.EncoredMove != move)
            {
                invalidMoves |= (int)HealthBar.FlagIndex(slot);
            }
            if ((holdEffect == HoldEffectChoiceBand || holdEffect == HoldEffectChoiceScarf || holdEffect == HoldEffectChoiceSpecs) && (opMask & CheckInvalidChoiceItem) != 0)
            {
                if (Battler_SlotForMove(mon, effects.ChoiceLockedMove) == MoveSlotCount)
                {
                    effects.ChoiceLockedMove = 0;
                }
                else if (effects.ChoiceLockedMove != 0 && effects.ChoiceLockedMove != move)
                {
                    invalidMoves |= (int)HealthBar.FlagIndex(slot);
                }
            }
        }
        return invalidMoves;
    }

    // OFFSET 0x02254CA8
    public static int BattleSystem_CanUseMove(BattleSystem battleSys, BattleContext battleCtx, int battler, int moveSlot, BattleMessage msgOut)
    {
        int result = 1;
        uint slotFlag = HealthBar.FlagIndex(moveSlot);
        BattleMon mon = battleCtx.BattleMons[battler];
        if ((BattleSystem_CheckInvalidMoves(battleSys, battleCtx, battler, 0, CheckInvalidDisabled) & slotFlag) != 0)
        {
            msgOut.Tags = 0xA;
            msgOut.Id = MessageMoveDisabled;
            msgOut.Params[0] = BattleSystem_NicknameTag(battleCtx, battler);
            msgOut.Params[1] = mon.Moves[moveSlot];
            result = 0;
        }
        else if ((BattleSystem_CheckInvalidMoves(battleSys, battleCtx, battler, 0, CheckInvalidTorment) & slotFlag) != 0)
        {
            msgOut.Tags = 2;
            msgOut.Id = MessageMoveTormented;
            msgOut.Params[0] = BattleSystem_NicknameTag(battleCtx, battler);
            result = 0;
        }
        else if ((BattleSystem_CheckInvalidMoves(battleSys, battleCtx, battler, 0, CheckInvalidTaunt) & slotFlag) != 0)
        {
            msgOut.Tags = 0xA;
            msgOut.Id = MessageMoveTaunted;
            msgOut.Params[0] = BattleSystem_NicknameTag(battleCtx, battler);
            msgOut.Params[1] = mon.Moves[moveSlot];
            result = 0;
        }
        else if ((BattleSystem_CheckInvalidMoves(battleSys, battleCtx, battler, 0, CheckInvalidImprison) & slotFlag) != 0)
        {
            msgOut.Tags = 0xA;
            msgOut.Id = MessageMoveImprisoned;
            msgOut.Params[0] = BattleSystem_NicknameTag(battleCtx, battler);
            msgOut.Params[1] = mon.Moves[moveSlot];
            result = 0;
        }
        else if ((BattleSystem_CheckInvalidMoves(battleSys, battleCtx, battler, 0, CheckInvalidGravity) & slotFlag) != 0)
        {
            msgOut.Tags = 0xA;
            msgOut.Id = MessageMoveGravity;
            msgOut.Params[0] = BattleSystem_NicknameTag(battleCtx, battler);
            msgOut.Params[1] = mon.Moves[moveSlot];
            result = 0;
        }
        else if ((BattleSystem_CheckInvalidMoves(battleSys, battleCtx, battler, 0, CheckInvalidHealBlock) & slotFlag) != 0)
        {
            msgOut.Tags = 0x22;
            msgOut.Id = MessageMoveHealBlock;
            msgOut.Params[0] = BattleSystem_NicknameTag(battleCtx, battler);
            msgOut.Params[1] = MoveHealBlock;
            msgOut.Params[2] = mon.Moves[moveSlot];
            result = 0;
        }
        else if ((BattleSystem_CheckInvalidMoves(battleSys, battleCtx, battler, 0, CheckInvalidChoiceItem) & slotFlag) != 0)
        {
            msgOut.Tags = 0x18;
            msgOut.Id = MessageMoveChoiceItem;
            msgOut.Params[0] = mon.HeldItem;
            msgOut.Params[1] = mon.MoveEffectsData.ChoiceLockedMove;
            result = 0;
        }
        else if ((BattleSystem_CheckInvalidMoves(battleSys, battleCtx, battler, 0, CheckInvalidNoPP) & slotFlag) != 0)
        {
            result = 0;
            msgOut.Tags = 0;
            msgOut.Id = MessageMoveNoPP;
        }
        return result;
    }

    // OFFSET 0x02254EE0
    public static int Battler_SlotForMove(BattleMon mon, ushort move)
    {
        int slot;
        for (slot = 0; slot < MoveSlotCount; slot++)
        {
            if (mon.Moves[slot] == move)
            {
                break;
            }
        }
        return slot;
    }

    // OFFSET 0x02254EF4
    private static int BattleSystem_TypeMatchupApplies(BattleContext battleCtx, int attacker, int defender, int chartEntry)
    {
        byte[] chart = TypeChart();
        int defendingType = chart[chartEntry * 3 + 1];
        int multiplier = chart[chartEntry * 3 + 2];
        int result = 1;
        if (Battler_HeldItemEffect(battleCtx, defender) == HoldEffectIronBall || (battleCtx.BattleMons[defender].MoveEffectsMask & MoveEffectIngrain) != 0)
        {
            if (defendingType == TypeFlying && multiplier == 0)
            {
                result = 0;
            }
        }
        if (battleCtx.TurnFlags[defender].Roosting != 0 && defendingType == TypeFlying)
        {
            result = 0;
        }
        if ((battleCtx.FieldConditionsMask & FieldConditionGravity) != 0 && defendingType == TypeFlying && multiplier == 0)
        {
            result = 0;
        }
        if ((battleCtx.BattleMons[defender].MoveEffectsMask & MoveEffectMiracleEye) != 0 && defendingType == TypeDark && multiplier == 0)
        {
            result = 0;
        }
        return result;
    }

    // OFFSET 0x02254FA8
    public static int BattleSystem_ApplyTypeChart(BattleSystem battleSys, BattleContext battleCtx, int move, int inType, int attacker, int defender, int damage, ref uint moveStatusMask)
    {
        if (move == MoveStruggle)
        {
            return damage;
        }
        int attackerEffect = (byte)Battler_HeldItemEffect(battleCtx, attacker);
        int attackerPower = (byte)Battler_HeldItemPower(battleCtx, attacker, 0);
        int defenderEffect = (byte)Battler_HeldItemEffect(battleCtx, defender);
        Battler_HeldItemPower(battleCtx, defender, 0);
        int type;
        if (Battler_Ability(battleCtx, attacker) == AbilityNormalize)
        {
            type = 0;
        }
        else if (inType != 0)
        {
            type = (byte)inType;
        }
        else
        {
            type = battleCtx.AiContext.MoveTable[move].Type;
        }
        int power = battleCtx.AiContext.MoveTable[move].Power;
        if ((battleCtx.BattleStatusMask & BattleStatusIgnoreTypeEffectiveness) == 0
            && (BattleMon_Get(battleCtx, attacker, BattleMonType1, null) == type || BattleMon_Get(battleCtx, attacker, BattleMonType2, null) == type))
        {
            if (Battler_Ability(battleCtx, attacker) == AbilityAdaptability)
            {
                damage *= 2;
            }
            else
            {
                damage = damage * 15 / 10;
            }
        }
        if (Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityLevitate) == 1 && type == TypeGround && defenderEffect != HoldEffectIronBall)
        {
            moveStatusMask |= MoveStatusLevitated;
        }
        else if (battleCtx.BattleMons[defender].MoveEffectsData.MagnetRiseTurns != 0
            && (battleCtx.BattleMons[defender].MoveEffectsMask & MoveEffectIngrain) == 0
            && type == TypeGround && defenderEffect != HoldEffectIronBall)
        {
            moveStatusMask |= MoveStatusMagnetRise;
        }
        else
        {
            byte[] chart = TypeChart();
            for (int i = 0; chart[i * 3] != TypeChartEnd; i++)
            {
                if (chart[i * 3] == TypeChartForesight)
                {
                    if ((battleCtx.BattleMons[defender].StatusVolatile & VolatileForesight) != 0 || Battler_Ability(battleCtx, attacker) == AbilityScrappy)
                    {
                        break;
                    }
                    continue;
                }
                if (chart[i * 3] != type)
                {
                    continue;
                }
                int defendingType = chart[i * 3 + 1];
                if (BattleMon_Get(battleCtx, defender, BattleMonType1, null) == defendingType && BattleSystem_TypeMatchupApplies(battleCtx, attacker, defender, i) == 1)
                {
                    damage = BattleSystem_ApplyTypeMultiplier(battleCtx, attacker, chart[i * 3 + 2], damage, power, ref moveStatusMask);
                }
                if (BattleMon_Get(battleCtx, defender, BattleMonType2, null) == defendingType
                    && BattleMon_Get(battleCtx, defender, BattleMonType1, null) != BattleMon_Get(battleCtx, defender, BattleMonType2, null)
                    && BattleSystem_TypeMatchupApplies(battleCtx, attacker, defender, i) == 1)
                {
                    damage = BattleSystem_ApplyTypeMultiplier(battleCtx, attacker, chart[i * 3 + 2], damage, power, ref moveStatusMask);
                }
            }
        }
        if (Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityWonderGuard) == 1
            && BattleContext_MoveExecutesThisTurn(battleCtx, move) != 0
            && ((moveStatusMask & MoveStatusSuperEffective) == 0 || (moveStatusMask & 6) == 6)
            && power != 0)
        {
            moveStatusMask |= MoveStatusWonderGuard;
        }
        else if ((battleCtx.BattleStatusMask & BattleStatusIgnoreTypeEffectiveness) == 0 && (battleCtx.BattleStatusMask & BattleStatusIgnoreTypeImmunity) == 0)
        {
            if ((moveStatusMask & MoveStatusSuperEffective) != 0 && power != 0)
            {
                if (Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityFilter) == 1 || Battler_IgnorableAbility(battleCtx, attacker, defender, AbilitySolidRock) == 1)
                {
                    damage = BattleSystem_Divide(damage * 3, 4);
                }
                if (attackerEffect == HoldEffectExpertBelt)
                {
                    damage = damage * (100 + attackerPower) / 100;
                }
            }
            if ((moveStatusMask & MoveStatusNotVeryEffective) != 0 && power != 0 && Battler_Ability(battleCtx, attacker) == AbilityTintedLens)
            {
                damage *= 2;
            }
        }
        else
        {
            moveStatusMask &= ~MoveStatusSuperEffective;
            moveStatusMask &= ~MoveStatusNotVeryEffective;
        }
        return damage;
    }
}
