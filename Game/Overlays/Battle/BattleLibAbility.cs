namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    public const int CountAbilitySameSide = 0;
    public const int CountAbilitySameSideAlive = 1;
    public const int CountAbilityOtherSide = 2;
    public const int CountAbilityOtherSideAlive = 3;
    public const int CountAbilityOtherSideAliveMask = 4;
    public const int CountAbilityAll = 5;
    public const int CountAbilityAllExceptMe = 6;
    public const int CountAbilityFirstExceptMe = 7;
    public const int CountAbilityAllAlive = 8;
    public const int CountAbilityAllAliveExceptMe = 9;
    public const int CountAliveExceptMe = 0;
    public const int CountAliveMySide = 1;

    private const uint MoveStatusIneffective = 0x8;
    private const uint MoveEffectGastroAcid = 0x200000;
    private const uint BattleStatusMoldBreaker = 0x800000;
    private const int TypeGhost = 7;
    private const int MoveCurse = 0xAE;
    private const int TypeChartEntryCount = 0x70;
    private const int NeutralMultiplier = 40;

    // OFFSET 0x022552D4
    public static void BattleSystem_CalcEffectiveness(BattleContext battleCtx, int move, int inType, int attackerAbility, int defenderAbility, int defenderItemEffect, int defenderType1, int defenderType2, ref uint moveStatusMask)
    {
        if (move == MoveStruggle)
        {
            return;
        }
        int type;
        if (attackerAbility == AbilityNormalize)
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
        if (attackerAbility != AbilityMoldBreaker && defenderAbility == AbilityLevitate && type == TypeGround
            && (battleCtx.FieldConditionsMask & FieldConditionGravity) == 0 && defenderItemEffect != HoldEffectIronBall)
        {
            moveStatusMask |= MoveStatusIneffective;
        }
        else
        {
            byte[] chart = TypeChart();
            for (int i = 0; chart[i * 3] != TypeChartEnd; i++)
            {
                if (chart[i * 3] == TypeChartForesight)
                {
                    if (attackerAbility == AbilityScrappy)
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
                if (defenderType1 == defendingType && BattleSystem_BasicTypeMatchupApplies(battleCtx, defenderItemEffect, i) == 1)
                {
                    BattleSystem_UpdateEffectivenessFlags(chart[i * 3 + 2], ref moveStatusMask);
                }
                if (defenderType2 == defendingType && defenderType1 != defenderType2 && BattleSystem_BasicTypeMatchupApplies(battleCtx, defenderItemEffect, i) == 1)
                {
                    BattleSystem_UpdateEffectivenessFlags(chart[i * 3 + 2], ref moveStatusMask);
                }
            }
        }
        if (attackerAbility != AbilityMoldBreaker && defenderAbility == AbilityWonderGuard && BattleContext_MoveExecutesThisTurn(battleCtx, move) != 0
            && ((moveStatusMask & MoveStatusSuperEffective) == 0 || (moveStatusMask & 6) == 6))
        {
            moveStatusMask |= MoveStatusIneffective;
        }
    }

    // OFFSET 0x022553F8
    private static int BattleSystem_BasicTypeMatchupApplies(BattleContext battleCtx, int itemEffect, int chartEntry)
    {
        byte[] chart = TypeChart();
        int defendingType = chart[chartEntry * 3 + 1];
        int multiplier = chart[chartEntry * 3 + 2];
        int result = 1;
        if (itemEffect == HoldEffectIronBall && defendingType == TypeFlying && multiplier == 0)
        {
            result = 0;
        }
        if ((battleCtx.FieldConditionsMask & FieldConditionGravity) != 0 && defendingType == TypeFlying && multiplier == 0)
        {
            result = 0;
        }
        return result;
    }

    // OFFSET 0x02255448
    private static void BattleSystem_UpdateEffectivenessFlags(int multiplier, ref uint moveStatusMask)
    {
        switch (multiplier)
        {
            case 0:
                moveStatusMask |= MoveStatusIneffective;
                moveStatusMask &= ~MoveStatusNotVeryEffective;
                moveStatusMask &= ~MoveStatusSuperEffective;
                break;
            case 5:
                if ((moveStatusMask & MoveStatusSuperEffective) != 0)
                {
                    moveStatusMask &= ~MoveStatusSuperEffective;
                }
                else
                {
                    moveStatusMask |= MoveStatusNotVeryEffective;
                }
                break;
            case 20:
                if ((moveStatusMask & MoveStatusNotVeryEffective) != 0)
                {
                    moveStatusMask &= ~MoveStatusNotVeryEffective;
                }
                else
                {
                    moveStatusMask |= MoveStatusSuperEffective;
                }
                break;
        }
    }

    // OFFSET 0x02255498
    public static int Battler_MoveFailed(BattleContext battleCtx, int battler)
    {
        MoveFailFlags flags = battleCtx.MoveFailFlags[battler];
        if (flags.Paralyzed != 0 || flags.NoEffect != 0 || flags.Imprisoned != 0 || flags.Infatuated != 0 || flags.Disabled != 0
            || flags.Taunted != 0 || flags.Flinched != 0 || flags.Gravity != 0 || flags.Confused != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x022554E0
    public static byte BattleSystem_NumBattlersAlive(BattleSystem battleSys, BattleContext battleCtx, int mode, int battler)
    {
        byte count = 0;
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        switch (mode)
        {
            case CountAliveExceptMe:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if (i != battler && battleCtx.BattleMons[i].CurHP != 0)
                    {
                        count++;
                    }
                }
                break;
            case CountAliveMySide:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, i) == BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler)
                        && battleCtx.BattleMons[i].CurHP != 0)
                    {
                        count++;
                    }
                }
                break;
        }
        return count;
    }

    // OFFSET 0x02255560
    public static int BattleSystem_NicknameTag(BattleContext battleCtx, int battler)
    {
        return battler | (battleCtx.SelectedPartySlot[battler] << 8);
    }

    // OFFSET 0x02255570
    public static ushort Battler_SelectedMove(BattleContext battleCtx, int battler)
    {
        ushort move = 0;
        if (battleCtx.BattlerActions[battler, 3] == ActionFight && battleCtx.BattlerActions[battler, 2] != 0)
        {
            move = battleCtx.BattleMons[battler].Moves[battleCtx.BattlerActions[battler, 2] - 1];
        }
        return move;
    }

    // OFFSET 0x022555A4
    public static int BattleSystem_CountAbility(BattleSystem battleSys, BattleContext battleCtx, int mode, int battler, int ability)
    {
        int result = 0;
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        switch (mode)
        {
            case CountAbilitySameSide:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, i) == BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler)
                        && Battler_Ability(battleCtx, i) == ability)
                    {
                        result++;
                    }
                }
                break;
            case CountAbilitySameSideAlive:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, i) == BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler)
                        && battleCtx.BattleMons[i].CurHP != 0 && Battler_Ability(battleCtx, i) == ability)
                    {
                        result++;
                    }
                }
                break;
            case CountAbilityOtherSide:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, i) != BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler)
                        && Battler_Ability(battleCtx, i) == ability)
                    {
                        result++;
                    }
                }
                break;
            case CountAbilityOtherSideAlive:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, i) != BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler)
                        && battleCtx.BattleMons[i].CurHP != 0 && Battler_Ability(battleCtx, i) == ability)
                    {
                        result++;
                    }
                }
                break;
            case CountAbilityOtherSideAliveMask:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, i) != BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler)
                        && battleCtx.BattleMons[i].CurHP != 0 && Battler_Ability(battleCtx, i) == ability)
                    {
                        result |= (int)HealthBar.FlagIndex(i);
                    }
                }
                break;
            case CountAbilityAll:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if (Battler_Ability(battleCtx, i) == ability)
                    {
                        result++;
                    }
                }
                break;
            case CountAbilityAllExceptMe:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if (i != battler && Battler_Ability(battleCtx, i) == ability)
                    {
                        result++;
                    }
                }
                break;
            case CountAbilityFirstExceptMe:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if (i != battler && Battler_Ability(battleCtx, i) == ability)
                    {
                        result = i + 1;
                        break;
                    }
                }
                break;
            case CountAbilityAllAlive:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if (Battler_Ability(battleCtx, i) == ability && battleCtx.BattleMons[i].CurHP != 0)
                    {
                        result++;
                    }
                }
                break;
            case CountAbilityAllAliveExceptMe:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if (i != battler && Battler_Ability(battleCtx, i) == ability && battleCtx.BattleMons[i].CurHP != 0)
                    {
                        result++;
                    }
                }
                break;
        }
        return result;
    }

    // OFFSET 0x0225582C
    public static int Move_IsMultiTurn(BattleContext battleCtx, int move)
    {
        switch (battleCtx.AiContext.MoveTable[move].Effect)
        {
            case 0x1A:
            case 0x27:
            case 0x4B:
            case 0x91:
            case 0x97:
            case 0x9B:
            case 0xFF:
            case 0x100:
            case 0x107:
            case 0x110:
                return 1;
        }
        return 0;
    }

    // OFFSET 0x0225588C
    public static int BattleSystem_TypeMatchup(BattleSystem battleSys, int index, out byte attackingType, out byte defendingType, out byte multiplier)
    {
        int result = 1;
        if (index >= TypeChartEntryCount)
        {
            index = BattleSystems.BattleSystem_RandNext(battleSys) % TypeChartEntryCount;
            result = 0;
        }
        byte[] chart = TypeChart();
        attackingType = chart[index * 3];
        defendingType = chart[index * 3 + 1];
        multiplier = chart[index * 3 + 2];
        return result;
    }

    // OFFSET 0x022558CC
    public static int BattleSystem_TypeMatchupMultiplier(byte attackingType, byte defendingType1, byte defendingType2)
    {
        int multiplier = NeutralMultiplier;
        byte[] chart = TypeChart();
        for (int i = 0; chart[i * 3] != TypeChartEnd; i++)
        {
            if (chart[i * 3] != attackingType)
            {
                continue;
            }
            int defendingType = chart[i * 3 + 1];
            if (defendingType1 == defendingType)
            {
                multiplier = multiplier * chart[i * 3 + 2] / 10;
            }
            if (defendingType2 == defendingType && defendingType1 != defendingType2)
            {
                multiplier = multiplier * chart[i * 3 + 2] / 10;
            }
        }
        return multiplier;
    }

    // OFFSET 0x02255918
    public static int Move_IsInvoker(ushort move)
    {
        if (move == 0 || move == 0xD6 || move == 0x17F || move == 0x112 || move == 0x17E || (ushort)(move - 0x76) <= 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02255950
    public static int Move_IsGhostCurse(BattleContext battleCtx, ushort move, int battler)
    {
        if (move == MoveCurse && (BattleMon_Get(battleCtx, battler, BattleMonType1, null) == TypeGhost || BattleMon_Get(battleCtx, battler, BattleMonType2, null) == TypeGhost))
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02255980
    public static int Battler_CanLoseItem(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        int result = 0;
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler);
        BattleMon mon = battleCtx.BattleMons[battler];
        if (mon.HeldItem != 0
            && (HealthBar.FlagIndex(battleCtx.SelectedPartySlot[battler]) & (uint)battleCtx.SideConditions[side].KnockedOffItemsMask) == 0
            && Item.Item_IsMail(mon.HeldItem) == 0)
        {
            result = 1;
        }
        return result;
    }

    // OFFSET 0x022559DC
    public static int Battler_HeldItemIsNotMail(BattleContext battleCtx, int battler)
    {
        return Item.Item_IsMail(battleCtx.BattleMons[battler].HeldItem) == 0 ? 1 : 0;
    }

    // OFFSET 0x022559FC
    public static int BattleSystem_ForceSwitchSucceeds(BattleSystem battleSys, BattleContext battleCtx)
    {
        int result = 0;
        int defenderLevel = battleCtx.BattleMons[battleCtx.Defender].Level;
        if (battleCtx.BattleMons[battleCtx.Attacker].Level >= defenderLevel)
        {
            result = 1;
        }
        else
        {
            int roll = (((BattleSystems.BattleSystem_RandNext(battleSys) & 0xFF) * (battleCtx.BattleMons[battleCtx.Attacker].Level + defenderLevel)) >> 8) + 1;
            if (roll > defenderLevel / 4)
            {
                result = 1;
            }
        }
        return result;
    }

    // OFFSET 0x02255A4C
    public static byte Battler_Ability(BattleContext battleCtx, int battler)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        if ((mon.MoveEffectsMask & MoveEffectGastroAcid) != 0 && mon.Ability != AbilityMultitype)
        {
            return 0;
        }
        if ((battleCtx.FieldConditionsMask & FieldConditionGravity) != 0 && mon.Ability == AbilityLevitate)
        {
            return 0;
        }
        if ((mon.MoveEffectsMask & MoveEffectIngrain) != 0 && mon.Ability == AbilityLevitate)
        {
            return 0;
        }
        return mon.Ability;
    }

    // OFFSET 0x02255AB4
    public static int Battler_IgnorableAbility(BattleContext battleCtx, int attacker, int defender, int ability)
    {
        int result = 0;
        if (Battler_Ability(battleCtx, attacker) != AbilityMoldBreaker)
        {
            if (Battler_Ability(battleCtx, defender) == ability)
            {
                result = 1;
            }
        }
        else if (Battler_Ability(battleCtx, defender) == ability && battleCtx.SelfTurnFlags[attacker].MoldBreakerActivated == 0)
        {
            battleCtx.SelfTurnFlags[attacker].MoldBreakerActivated = 1;
            battleCtx.BattleStatusMask |= BattleStatusMoldBreaker;
        }
        return result;
    }
}
