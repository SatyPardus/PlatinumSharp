namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const uint BattleTypeMulti = 0x8;
    private const uint BattleTypeTag = 0x10;
    private const uint BattleTypeCannotFleeMask = 0x2A4;
    private const int HoldEffectSmokeBall = 0x3F;
    private const int AbilityShadowTag = 0x17;
    private const int AbilityMagnetPull = 0x2A;
    private const int AbilityRunAway = 0x32;
    private const int AbilityTruant = 0x36;
    private const int AbilityArenaTrap = 0x47;
    private const int TypeSteel = 8;
    private const uint VolatileTrappedMask = 0x0400E000;
    private const uint MoveEffectImprison = 0x2000;
    private const int MessageAbilityPreventsEscape = 0x27;
    private const int MessageCannotEscape = 0x31A;
    private const int MessageFailedToEscape = 0x62;
    private const int FleeingWithItem = 1;
    private const int FleeingWithAbility = 2;
    private const int RunAttemptBonus = 30;
    private const uint BattleStatusLoopReset = 0xFFEFFFFF;
    private const uint GravityMoveTable = 0x0226EBD4;
    private const int GravityMoveCount = 6;
    private const uint HealBlockMoveTable = 0x0226EBFA;
    private const int HealBlockMoveCount = 14;
    private const int MoveLastResort = 0x183;
    private const int MonDataSpeciesID = 5;
    private const int MonDataCurrentHP = 0xA3;

    // OFFSET 0x02255B10
    public static int BattleSystem_AnyReplacementMons(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        int count = 0;
        int result = 0;
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        Party party = BattleSystems.BattleSystem_GetParty(battleSys, battler);
        int partyCount = BattleSystems.BattleSystem_GetPartyCount(battleSys, battler);
        int needed = 1;
        int slot1 = battleCtx.SelectedPartySlot[battler];
        int slot2 = slot1;
        if ((battleType & BattleTypeMulti) != 0 || ((battleType & BattleTypeTag) != 0 && (BattleSystems.BattleSystem_BattlerSlot(battleSys, battler) & 1) != 0))
        {
            slot2 = slot1;
        }
        else if ((battleType & BattleTypeDoubles) != 0)
        {
            slot2 = battleCtx.SelectedPartySlot[BattleSystems.BattleSystem_GetPartner(battleSys, battler)];
        }
        for (int i = 0; i < partyCount; i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, i);
            if (Pokemon.Pokemon_GetValue(mon, MonDataSpeciesID, null) != 0
                && Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) == 0
                && Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null) != 0
                && slot1 != i && slot2 != i)
            {
                count++;
            }
        }
        if (count >= needed)
        {
            result = 1;
        }
        return result;
    }

    private static int Battler_TrappedByAbility(BattleContext battleCtx, int battlerCount, int ability, BattleMessage? msgOut)
    {
        if (msgOut == null)
        {
            return 1;
        }
        msgOut.Tags = 0xB;
        msgOut.Id = MessageAbilityPreventsEscape;
        msgOut.Params[0] = BattleSystem_NicknameTag(battleCtx, battlerCount);
        msgOut.Params[1] = ability;
        return 1;
    }

    // OFFSET 0x02255C00
    public static int Battler_IsTrapped(BattleSystem battleSys, BattleContext battleCtx, int battler, BattleMessage? msgOut)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int itemEffect = Battler_HeldItemEffect(battleCtx, battler);
        if (itemEffect == HoldEffectSmokeBall || (battleType & BattleTypeCannotFleeMask) != 0 || Battler_Ability(battleCtx, battler) == AbilityRunAway)
        {
            return 0;
        }
        BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler);
        BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        int count = BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityAllAliveExceptMe, battler, AbilityShadowTag);
        if (count != 0 && Battler_Ability(battleCtx, battler) != AbilityShadowTag)
        {
            return Battler_TrappedByAbility(battleCtx, count, AbilityShadowTag, msgOut);
        }
        count = BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityOtherSideAlive, battler, AbilityArenaTrap);
        if (count != 0)
        {
            if ((battleCtx.FieldConditionsMask & FieldConditionGravity) == 0 && itemEffect != HoldEffectIronBall)
            {
                if (Battler_Ability(battleCtx, battler) != AbilityLevitate
                    && battleCtx.BattleMons[battler].MoveEffectsData.MagnetRiseTurns == 0
                    && BattleMon_Get(battleCtx, battler, BattleMonType1, null) != TypeFlying
                    && BattleMon_Get(battleCtx, battler, BattleMonType2, null) != TypeFlying)
                {
                    return Battler_TrappedByAbility(battleCtx, count, AbilityArenaTrap, msgOut);
                }
            }
            else
            {
                return Battler_TrappedByAbility(battleCtx, count, AbilityArenaTrap, msgOut);
            }
        }
        count = BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityOtherSideAlive, battler, AbilityMagnetPull);
        if (count != 0 && (BattleMon_Get(battleCtx, battler, BattleMonType1, null) == TypeSteel || BattleMon_Get(battleCtx, battler, BattleMonType2, null) == TypeSteel))
        {
            return Battler_TrappedByAbility(battleCtx, count, AbilityMagnetPull, msgOut);
        }
        BattleMon mon = battleCtx.BattleMons[battler];
        if ((mon.StatusVolatile & VolatileTrappedMask) != 0 || (mon.MoveEffectsMask & MoveEffectIngrain) != 0)
        {
            if (msgOut == null)
            {
                return 1;
            }
            msgOut.Tags = 0;
            msgOut.Id = MessageCannotEscape;
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02255DE8
    public static int Battler_CanEscape(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int itemEffect = Battler_HeldItemEffect(battleCtx, battler);
        int result = 0;
        if (itemEffect == HoldEffectSmokeBall)
        {
            battleCtx.TurnFlags[battler].Fleeing = FleeingWithItem;
            result = 1;
        }
        else if ((battleType & BattleTypeCannotFleeMask) != 0)
        {
            result = 1;
        }
        else if (Battler_Ability(battleCtx, battler) == AbilityRunAway)
        {
            battleCtx.TurnFlags[battler].Fleeing = FleeingWithAbility;
            result = 1;
        }
        else
        {
            int speed = battleCtx.BattleMons[battler].Speed;
            int enemySpeed = battleCtx.BattleMons[battler ^ 1].Speed;
            if (speed < enemySpeed)
            {
                byte chance = (byte)(speed * 128 / enemySpeed + battleCtx.RunAttempts * RunAttemptBonus);
                if (chance > BattleSystems.BattleSystem_RandNext(battleSys) % 256)
                {
                    result = 1;
                }
            }
            else
            {
                result = 1;
            }
            if (result == 0)
            {
                BattleIO.BattleIO_PrintAlertMessage(battleSys, battler, 0, MessageFailedToEscape);
            }
            battleCtx.RunAttempts++;
        }
        return result;
    }

    // OFFSET 0x02255EC0
    public static int Battler_CheckTruant(BattleContext battleCtx, int battler)
    {
        if (Battler_Ability(battleCtx, battler) == AbilityTruant && (battleCtx.TotalTurns & 1) != battleCtx.BattleMons[battler].MoveEffectsData.Truant)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02255EF4
    public static int Move_Imprisoned(BattleSystem battleSys, BattleContext battleCtx, int battler, ushort move)
    {
        int result = 0;
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler);
        for (int i = 0; i < maxBattlers; i++)
        {
            if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, i) != side && (battleCtx.BattleMons[i].MoveEffectsMask & MoveEffectImprison) != 0)
            {
                if (Battler_SlotForMove(battleCtx.BattleMons[i], move) != MoveSlotCount)
                {
                    result = 1;
                }
            }
        }
        return result;
    }

    // OFFSET 0x02255F68
    public static int BattleSystem_AnyBattlerWithMoveEffect(BattleSystem battleSys, BattleContext battleCtx, uint effectMask)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        for (int i = 0; i < maxBattlers; i++)
        {
            if ((battleCtx.BattleMons[i].MoveEffectsMask & effectMask) != 0)
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x02255F94
    public static void BattleSystem_SetupLoop(BattleSystem battleSys, BattleContext battleCtx)
    {
        battleCtx.MoveStatusFlags = 0;
        battleCtx.CriticalMul = 1;
        battleCtx.BattleStatusMask &= BattleStatusLoopReset;
    }

    // OFFSET 0x02255FBC
    public static void BattleSystem_SortMonSpeedOrder(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        for (int i = 0; i < maxBattlers; i++)
        {
            battleCtx.MonSpeedOrder[i] = (byte)i;
        }
        for (int i = 0; i < maxBattlers - 1; i++)
        {
            for (int j = i + 1; j < maxBattlers; j++)
            {
                byte battler1 = battleCtx.MonSpeedOrder[i];
                byte battler2 = battleCtx.MonSpeedOrder[j];
                if (BattleSystem_CompareBattlerSpeed(battleSys, battleCtx, battler1, battler2, 1) != 0)
                {
                    battleCtx.MonSpeedOrder[i] = battler2;
                    battleCtx.MonSpeedOrder[j] = battler1;
                }
            }
        }
    }

    private static int MoveTable_Contains(uint table, int count, ushort move)
    {
        byte[] raw = RomData.Overlay(16, table, count * 2);
        for (int i = 0; i < count; i++)
        {
            if (Nitro.Bin.U16(raw, i * 2) == move)
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x02256044
    public static int Move_FailsInHighGravity(BattleSystem battleSys, BattleContext battleCtx, int battler, ushort move)
    {
        if ((battleCtx.FieldConditionsMask & FieldConditionGravity) != 0)
        {
            return MoveTable_Contains(GravityMoveTable, GravityMoveCount, move);
        }
        return 0;
    }

    // OFFSET 0x02256078
    public static int Move_HealBlocked(BattleSystem battleSys, BattleContext battleCtx, int battler, ushort move)
    {
        if (battleCtx.BattleMons[battler].MoveEffectsData.HealBlockTurns != 0)
        {
            return MoveTable_Contains(HealBlockMoveTable, HealBlockMoveCount, move);
        }
        return 0;
    }

    // OFFSET 0x022560B0
    public static void BattleSystem_UpdateLastResort(BattleSystem battleSys, BattleContext battleCtx)
    {
        uint move = battleCtx.MoveTemp;
        if (move == MoveLastResort)
        {
            return;
        }
        MoveEffectsData effects = battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData;
        int count = effects.LastResortCount;
        if (count == MoveSlotCount)
        {
            return;
        }
        for (int i = 0; i < count; i++)
        {
            if (effects.LastResortMoves[i] == move)
            {
                return;
            }
        }
        effects.LastResortMoves[count] = (ushort)move;
        effects.LastResortCount = (effects.LastResortCount + 1) & 7;
    }

    // OFFSET 0x02256128
    public static int Battler_CountMoves(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        int count;
        for (count = 0; count < MoveSlotCount; count++)
        {
            if (battleCtx.BattleMons[battler].Moves[count] == 0)
            {
                break;
            }
        }
        return count;
    }
}
