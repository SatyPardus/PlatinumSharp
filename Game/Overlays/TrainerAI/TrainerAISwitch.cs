namespace PlatinumSharp.Game;

public static partial class TrainerAI
{
    private const int AbilityWonderGuard = 0x19;
    private const int AbilityFlashFire = 0x12;
    private const int AbilityWaterAbsorb = 0xB;
    private const int AbilityVoltAbsorb = 0xA;
    private const int AbilityNaturalCure = 0x1E;
    private const int TypeElectric = 0xD;
    private const int TypeSteel = 8;
    private const uint MoveStatusSuperEffective = 0x2;
    private const uint MoveStatusNotEffective = 0x8;
    private const uint MoveStatusNotVeryEffective = 0x4;
    private const uint VolatileTrapped = 0x400E000;
    private const uint MoveEffectIngrain = 0x400;
    private const uint MoveEffectEmbargo = 0x4000000;
    private const uint StatusSleep = 0x7;
    private const uint StatusPoison = 0x8;
    private const uint StatusBurn = 0x10;
    private const uint StatusFreeze = 0x20;
    private const uint StatusParalysis = 0x40;
    private const uint StatusToxic = 0x80;
    private const uint VolatileConfusion = 0x7;
    private const uint SideConditionMist = 0x40;
    private const int CountAbilityOtherSide = 2;
    private const int CountAbilityOtherSideAlive = 6;
    private const int MonDataType1 = 0xB1;
    private const int MonDataType2 = 0xB2;
    private const int BattlerSlotPlayerSecond = 4;
    private const uint BattleTypeLinkTagMask = 0x4B;
    private const int CommandFight = 1;
    private const int CommandItem = 2;
    private const int CommandSwitch = 3;
    private const ushort ItemFullRestore = 0x17;
    private const int ItemParamHealHP = 0x26;
    private const int ItemParamHPRestore = 0x36;
    private const int ItemParamHealSleep = 0xF;
    private const int ItemParamHealPoison = 0x10;
    private const int ItemParamHealBurn = 0x11;
    private const int ItemParamHealFreeze = 0x12;
    private const int ItemParamHealParalysis = 0x13;
    private const int ItemParamHealConfusion = 0x14;
    private const int ItemParamGuardSpec = 0x16;
    private const int ItemParamAttackBoost = 0x1B;
    private const int ItemParamDefenseBoost = 0x1C;
    private const int ItemParamSpAttackBoost = 0x1D;
    private const int ItemParamSpDefenseBoost = 0x1E;
    private const int ItemParamSpeedBoost = 0x1F;
    private const int ItemParamAccuracyBoost = 0x20;
    private const byte UsedItemTypeFullRestore = 0;
    private const byte UsedItemTypeHealHP = 1;
    private const byte UsedItemTypeStatus = 2;
    private const byte UsedItemTypeStatBoost = 3;
    private const byte UsedItemTypeGuardSpec = 4;
    private const byte UsedItemTypeNone = 5;

    private static int AI_SwitchPartner(BattleSystem battleSys, int battler)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        if ((battleType & BattleSystems.BattleTypeTag) != 0 || (battleType & BattleSystems.BattleTypeMulti) != 0)
        {
            return (byte)battler;
        }
        return (byte)BattleSystems.BattleSystem_GetPartner(battleSys, battler);
    }

    private static bool AI_IsSwitchCandidate(BattleContext battleCtx, Pokemon mon, int slot, int battler, int partner)
    {
        return Pokemon.Pokemon_GetValue(mon, MonDataCurHP, null) != 0
            && Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEgg, null) != 0
            && Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEgg, null) != SpeciesEgg
            && slot != battleCtx.SelectedPartySlot[battler]
            && slot != battleCtx.SelectedPartySlot[partner]
            && slot != battleCtx.AiSwitchedPartySlot[battler]
            && slot != battleCtx.AiSwitchedPartySlot[partner];
    }

    private static uint AI_PartyMoveEffectiveness(BattleContext battleCtx, Pokemon mon, int move, int type, int defender)
    {
        uint flags = 0;
        BattleContexts.BattleSystem_CalcEffectiveness(battleCtx, move, type, (int)Pokemon.Pokemon_GetValue(mon, MonDataAbility, null),
            BattleContexts.Battler_Ability(battleCtx, defender), BattleContexts.Battler_HeldItemEffect(battleCtx, defender),
            BattleContexts.BattleMon_Get(battleCtx, defender, BattleContexts.BattleMonType1, null), BattleContexts.BattleMon_Get(battleCtx, defender, BattleContexts.BattleMonType2, null), ref flags);
        return flags;
    }

    // OFFSET 0x0222342C
    private static int AI_CannotDamageWonderGuard(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeDoubles) != 0)
        {
            return 0;
        }
        int opponent = battler ^ 1;
        if (battleCtx.BattleMons[opponent].Ability != AbilityWonderGuard)
        {
            return 0;
        }
        for (int i = 0; i < MaxMoves; i++)
        {
            ushort move = battleCtx.BattleMons[battler].Moves[i];
            int type = TrainerAI_MoveType(battleSys, battleCtx, battler, move);
            if (move != 0)
            {
                uint flags = 0;
                BattleContexts.BattleSystem_ApplyTypeChart(battleSys, battleCtx, move, type, battler, opponent, 0, ref flags);
                if ((flags & MoveStatusSuperEffective) != 0)
                {
                    return 0;
                }
            }
        }
        for (int i = 0; i < BattleSystems.BattleSystem_GetPartyCount(battleSys, battler); i++)
        {
            Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, i);
            if (Pokemon.Pokemon_GetValue(mon, MonDataCurHP, null) == 0
                || Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEgg, null) == 0
                || Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEgg, null) == SpeciesEgg
                || i == battleCtx.SelectedPartySlot[battler])
            {
                continue;
            }
            for (int j = 0; j < MaxMoves; j++)
            {
                ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + j, null);
                int type = BattleContexts.BattleSystem_PartyMonMoveType(battleSys, battleCtx, mon, move);
                if (move != 0
                    && (AI_PartyMoveEffectiveness(battleCtx, mon, move, type, opponent) & MoveStatusSuperEffective) != 0
                    && BattleSystems.BattleSystem_RandNext(battleSys) % 3 < 2)
                {
                    battleCtx.AiSwitchedPartySlot[battler] = (byte)i;
                    return 1;
                }
            }
        }
        return 0;
    }

    // OFFSET 0x022235F0
    private static int AI_OnlyIneffectiveMoves(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        int defender1;
        int defender2;
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeDoubles) != 0)
        {
            defender1 = 0;
            defender2 = 2;
        }
        else
        {
            defender1 = 0;
            defender2 = 0;
        }
        int damagingMoves = 0;
        for (int i = 0; i < MaxMoves; i++)
        {
            ushort move = battleCtx.BattleMons[battler].Moves[i];
            int type = TrainerAI_MoveType(battleSys, battleCtx, battler, move);
            if (move == 0 || battleCtx.AiContext.MoveTable[move].Power == 0)
            {
                continue;
            }
            damagingMoves++;
            uint flags = 0;
            if (battleCtx.BattleMons[defender1].CurHP != 0)
            {
                BattleContexts.BattleSystem_ApplyTypeChart(battleSys, battleCtx, move, type, battler, defender1, 0, ref flags);
            }
            if ((flags & MoveStatusNotEffective) == 0)
            {
                return 0;
            }
            flags = 0;
            if (battleCtx.BattleMons[defender2].CurHP != 0)
            {
                BattleContexts.BattleSystem_ApplyTypeChart(battleSys, battleCtx, move, type, battler, defender2, 0, ref flags);
            }
            if ((flags & MoveStatusNotEffective) == 0)
            {
                return 0;
            }
        }
        if (damagingMoves < 2)
        {
            return 0;
        }
        int slotBattler = (byte)battler;
        int partner = AI_SwitchPartner(battleSys, battler);
        int count = BattleSystems.BattleSystem_GetPartyCount(battleSys, battler);
        for (int i = 0; i < count; i++)
        {
            Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, i);
            if (!AI_IsSwitchCandidate(battleCtx, mon, i, slotBattler, partner))
            {
                continue;
            }
            for (int j = 0; j < MaxMoves; j++)
            {
                ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + j, null);
                int type = BattleContexts.BattleSystem_PartyMonMoveType(battleSys, battleCtx, mon, move);
                if (move == 0 || battleCtx.AiContext.MoveTable[move].Power == 0)
                {
                    continue;
                }
                uint flags = 0;
                if (battleCtx.BattleMons[defender1].CurHP != 0)
                {
                    flags = AI_PartyMoveEffectiveness(battleCtx, mon, move, type, defender1);
                }
                if ((flags & MoveStatusSuperEffective) != 0 && BattleSystems.BattleSystem_RandNext(battleSys) % 3 < 2)
                {
                    battleCtx.AiSwitchedPartySlot[battler] = (byte)i;
                    return 1;
                }
                flags = 0;
                if (battleCtx.BattleMons[defender2].CurHP != 0)
                {
                    flags = AI_PartyMoveEffectiveness(battleCtx, mon, move, type, defender2);
                }
                if ((flags & MoveStatusSuperEffective) != 0 && BattleSystems.BattleSystem_RandNext(battleSys) % 3 < 2)
                {
                    battleCtx.AiSwitchedPartySlot[battler] = (byte)i;
                    return 1;
                }
            }
        }
        for (int i = 0; i < count; i++)
        {
            Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, i);
            if (!AI_IsSwitchCandidate(battleCtx, mon, i, slotBattler, partner))
            {
                continue;
            }
            for (int j = 0; j < MaxMoves; j++)
            {
                ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + j, null);
                int type = BattleContexts.BattleSystem_PartyMonMoveType(battleSys, battleCtx, mon, move);
                if (move == 0 || battleCtx.AiContext.MoveTable[move].Power == 0)
                {
                    continue;
                }
                uint flags = 0;
                if (battleCtx.BattleMons[defender1].CurHP != 0)
                {
                    flags = AI_PartyMoveEffectiveness(battleCtx, mon, move, type, defender1);
                }
                if (flags == 0 && BattleSystems.BattleSystem_RandNext(battleSys) % 2 == 0)
                {
                    battleCtx.AiSwitchedPartySlot[battler] = (byte)i;
                    return 1;
                }
                flags = 0;
                if (battleCtx.BattleMons[defender2].CurHP != 0)
                {
                    flags = AI_PartyMoveEffectiveness(battleCtx, mon, move, type, defender2);
                }
                if (flags == 0 && BattleSystems.BattleSystem_RandNext(battleSys) % 2 == 0)
                {
                    battleCtx.AiSwitchedPartySlot[battler] = (byte)i;
                    return 1;
                }
            }
        }
        return 0;
    }

    private static bool AI_HasSuperEffectiveMoveAgainst(BattleSystem battleSys, BattleContext battleCtx, int battler, int defender, int alwaysSwitch)
    {
        for (int i = 0; i < MaxMoves; i++)
        {
            ushort move = battleCtx.BattleMons[battler].Moves[i];
            int type = TrainerAI_MoveType(battleSys, battleCtx, battler, move);
            if (move == 0)
            {
                continue;
            }
            uint flags = 0;
            BattleContexts.BattleSystem_ApplyTypeChart(battleSys, battleCtx, move, type, battler, defender, 0, ref flags);
            if ((flags & MoveStatusSuperEffective) != 0)
            {
                if (alwaysSwitch != 0)
                {
                    return true;
                }
                if (BattleSystems.BattleSystem_RandNext(battleSys) % 10 != 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // OFFSET 0x02223B34
    private static int AI_HasSuperEffectiveMove(BattleSystem battleSys, BattleContext battleCtx, int battler, int alwaysSwitch)
    {
        byte opponent = (byte)BattleSystems.BattleSystem_GetBattlerOfType(battleSys, (byte)(BattleSystems.BattleSystem_BattlerSlot(battleSys, battler) ^ 1));
        if ((HealthBar.FlagIndex(opponent) & battleCtx.BattlersSwitchingMask) == 0
            && AI_HasSuperEffectiveMoveAgainst(battleSys, battleCtx, battler, opponent, alwaysSwitch))
        {
            return 1;
        }
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeDoubles) == 0)
        {
            return 0;
        }
        byte partner = (byte)BattleSystems.BattleSystem_GetPartner(battleSys, opponent);
        if ((HealthBar.FlagIndex(partner) & battleCtx.BattlersSwitchingMask) == 0
            && AI_HasSuperEffectiveMoveAgainst(battleSys, battleCtx, battler, partner, alwaysSwitch))
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02223C8C
    private static int AI_HasAbsorbAbilityInParty(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        if (AI_HasSuperEffectiveMove(battleSys, battleCtx, battler, 1) != 0 && BattleSystems.BattleSystem_RandNext(battleSys) % 3 != 0)
        {
            return 0;
        }
        ushort move = battleCtx.MoveHit[battler];
        if (move == 0)
        {
            return 0;
        }
        MoveData data = battleCtx.AiContext.MoveTable[move];
        if (data.Power == 0)
        {
            return 0;
        }
        int absorbAbility;
        switch (data.Type)
        {
            case TypeFire:
                absorbAbility = AbilityFlashFire;
                break;
            case TypeWater:
                absorbAbility = AbilityWaterAbsorb;
                break;
            case TypeElectric:
                absorbAbility = AbilityVoltAbsorb;
                break;
            default:
                return 0;
        }
        if (absorbAbility == BattleContexts.Battler_Ability(battleCtx, battler))
        {
            return 0;
        }
        int slotBattler = (byte)battler;
        int partner = AI_SwitchPartner(battleSys, battler);
        int count = BattleSystems.BattleSystem_GetPartyCount(battleSys, battler);
        for (int i = 0; i < count; i++)
        {
            Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, i);
            if (AI_IsSwitchCandidate(battleCtx, mon, i, slotBattler, partner)
                && absorbAbility == (byte)Pokemon.Pokemon_GetValue(mon, MonDataAbility, null)
                && (BattleSystems.BattleSystem_RandNext(battleSys) & 1) != 0)
            {
                battleCtx.AiSwitchedPartySlot[battler] = (byte)i;
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x02223E10
    private static int AI_HasPartyMemberWithSuperEffectiveMove(BattleSystem battleSys, BattleContext battleCtx, int battler, uint checkEffectiveness, byte rand)
    {
        ushort hitMove = battleCtx.MoveHit[battler];
        if (hitMove == 0 || battleCtx.MoveHitBattler[battler] == 0xFF)
        {
            return 0;
        }
        if (battleCtx.AiContext.MoveTable[hitMove].Power == 0)
        {
            return 0;
        }
        int slotBattler = (byte)battler;
        int partner = AI_SwitchPartner(battleSys, battler);
        int count = BattleSystems.BattleSystem_GetPartyCount(battleSys, battler);
        int hitter = battleCtx.MoveHitBattler[battler];
        for (int i = 0; i < count; i++)
        {
            Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, i);
            if (!AI_IsSwitchCandidate(battleCtx, mon, i, slotBattler, partner))
            {
                continue;
            }
            uint flags = 0;
            int hitType = TrainerAI_MoveType(battleSys, battleCtx, hitter, hitMove);
            BattleContexts.BattleSystem_CalcEffectiveness(battleCtx, hitMove, hitType, BattleContexts.Battler_Ability(battleCtx, hitter),
                (int)Pokemon.Pokemon_GetValue(mon, MonDataAbility, null),
                BattleContexts.BattleSystem_GetItemData(battleCtx, (ushort)Pokemon.Pokemon_GetValue(mon, MonDataHeldItem, null), ItemParamHoldEffect),
                (int)Pokemon.Pokemon_GetValue(mon, MonDataType1, null), (int)Pokemon.Pokemon_GetValue(mon, MonDataType2, null), ref flags);
            if ((flags & checkEffectiveness) == 0)
            {
                continue;
            }
            for (int j = 0; j < MaxMoves; j++)
            {
                ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + j, null);
                int type = BattleContexts.BattleSystem_PartyMonMoveType(battleSys, battleCtx, mon, move);
                if (move != 0
                    && (AI_PartyMoveEffectiveness(battleCtx, mon, move, type, hitter) & MoveStatusSuperEffective) != 0
                    && BattleSystems.BattleSystem_RandNext(battleSys) % rand == 0)
                {
                    battleCtx.AiSwitchedPartySlot[battler] = (byte)i;
                    return 1;
                }
            }
        }
        return 0;
    }

    // OFFSET 0x02224070
    private static int AI_IsAsleepWithNaturalCure(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        if ((mon.Status & StatusSleep) == 0 || BattleContexts.Battler_Ability(battleCtx, battler) != AbilityNaturalCure || (uint)mon.CurHP < mon.MaxHP / 2)
        {
            return 0;
        }
        if (battleCtx.MoveHit[battler] == 0 && (BattleSystems.BattleSystem_RandNext(battleSys) & 1) != 0)
        {
            battleCtx.AiSwitchedPartySlot[battler] = PartySlotNone;
            return 1;
        }
        if (battleCtx.AiContext.MoveTable[battleCtx.MoveHit[battler]].Power == 0 && (BattleSystems.BattleSystem_RandNext(battleSys) & 1) != 0)
        {
            battleCtx.AiSwitchedPartySlot[battler] = PartySlotNone;
            return 1;
        }
        if (AI_HasPartyMemberWithSuperEffectiveMove(battleSys, battleCtx, battler, MoveStatusNotEffective, 1) != 0)
        {
            return 1;
        }
        if (AI_HasPartyMemberWithSuperEffectiveMove(battleSys, battleCtx, battler, MoveStatusNotVeryEffective, 1) != 0)
        {
            return 1;
        }
        if ((BattleSystems.BattleSystem_RandNext(battleSys) & 1) != 0)
        {
            battleCtx.AiSwitchedPartySlot[battler] = PartySlotNone;
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0222416C
    private static int AI_IsHeavilyStatBoosted(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        byte sum = 0;
        for (int i = 0; i < BattleStatCount; i++)
        {
            int stage = battleCtx.BattleMons[battler].StatBoosts[i];
            if (stage > StatStageNeutral)
            {
                sum = (byte)(sum + stage - StatStageNeutral);
            }
        }
        return sum >= 4 ? 1 : 0;
    }

    // OFFSET 0x022241A4
    private static int TrainerAI_ShouldSwitch(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        if ((mon.StatusVolatile & VolatileTrapped) != 0 || (mon.MoveEffectsMask & MoveEffectIngrain) != 0)
        {
            return 0;
        }
        if (BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityOtherSide, battler, AbilityShadowTag) != 0
            || BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityOtherSide, battler, AbilityArenaTrap) != 0)
        {
            return 0;
        }
        if (BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityOtherSideAlive, battler, AbilityMagnetPull) != 0
            && (BattleContexts.BattleMon_Get(battleCtx, battler, BattleContexts.BattleMonType1, null) == TypeSteel
                || BattleContexts.BattleMon_Get(battleCtx, battler, BattleContexts.BattleMonType2, null) == TypeSteel))
        {
            return 0;
        }
        int available = 0;
        int slotBattler = (byte)battler;
        int partner = AI_SwitchPartner(battleSys, battler);
        int count = BattleSystems.BattleSystem_GetPartyCount(battleSys, battler);
        for (int i = 0; i < count; i++)
        {
            if (AI_IsSwitchCandidate(battleCtx, BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, i), i, slotBattler, partner))
            {
                available++;
            }
        }
        if (available == 0)
        {
            return 0;
        }
        if (AI_PerishSongKO(battleCtx, battler) != 0)
        {
            return 1;
        }
        if (AI_CannotDamageWonderGuard(battleSys, battleCtx, battler) != 0)
        {
            return 1;
        }
        if (AI_OnlyIneffectiveMoves(battleSys, battleCtx, battler) != 0)
        {
            return 1;
        }
        if (AI_HasAbsorbAbilityInParty(battleSys, battleCtx, battler) != 0)
        {
            return 1;
        }
        if (AI_IsAsleepWithNaturalCure(battleSys, battleCtx, battler) != 0)
        {
            return 1;
        }
        if (AI_HasSuperEffectiveMove(battleSys, battleCtx, battler, 0) != 0)
        {
            return 0;
        }
        if (AI_IsHeavilyStatBoosted(battleSys, battleCtx, battler) != 0)
        {
            return 0;
        }
        if (AI_HasPartyMemberWithSuperEffectiveMove(battleSys, battleCtx, battler, MoveStatusNotEffective, 2) != 0)
        {
            return 1;
        }
        if (AI_HasPartyMemberWithSuperEffectiveMove(battleSys, battleCtx, battler, MoveStatusNotVeryEffective, 3) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x022243BC
    public static int TrainerAI_PickCommand(BattleSystem battleSys, int battler)
    {
        BattleContext battleCtx = battleSys.BattleCtx!;
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        if ((battleType & BattleSystems.BattleTypeTrainer) == 0 && BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) != 0)
        {
            return CommandFight;
        }
        if (TrainerAI_ShouldSwitch(battleSys, battleCtx, battler) != 0)
        {
            if (battleCtx.AiSwitchedPartySlot[battler] == PartySlotNone)
            {
                int slot = BattleContexts.BattleAI_PostKOSwitchIn(battleSys, battler);
                if (slot == PartySlotNone)
                {
                    int slotBattler = (byte)battler;
                    int partner = AI_SwitchPartner(battleSys, battler);
                    int count = BattleSystems.BattleSystem_GetPartyCount(battleSys, battler);
                    for (slot = 0; slot < count; slot++)
                    {
                        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, slot);
                        if (Pokemon.Pokemon_GetValue(mon, MonDataCurHP, null) != 0
                            && slot != battleCtx.SelectedPartySlot[slotBattler]
                            && slot != battleCtx.SelectedPartySlot[partner]
                            && slot != battleCtx.AiSwitchedPartySlot[slotBattler]
                            && slot != battleCtx.AiSwitchedPartySlot[partner])
                        {
                            break;
                        }
                    }
                }
                battleCtx.AiSwitchedPartySlot[battler] = (byte)slot;
            }
            return CommandSwitch;
        }
        if (TrainerAI_ShouldUseItem(battleSys, battler) != 0)
        {
            return CommandItem;
        }
        return CommandFight;
    }

    private static bool AI_ItemHasParam(BattleContext battleCtx, ushort item, int param)
    {
        return BattleContexts.BattleSystem_GetItemData(battleCtx, item, param) != 0;
    }

    private static void AI_UseStatusItem(BattleContext battleCtx, int side, int condition)
    {
        battleCtx.AiContext.UsedItemCondition[side] |= (byte)HealthBar.FlagIndex(condition);
        battleCtx.AiContext.UsedItemType[side] = UsedItemTypeStatus;
    }

    // OFFSET 0x022244B0
    private static int TrainerAI_ShouldUseItem(BattleSystem battleSys, int battler)
    {
        int result = 0;
        byte alive = 0;
        BattleContext battleCtx = battleSys.BattleCtx!;
        AIContext ai = battleCtx.AiContext;
        int side = battler >> 1;
        ai.UsedItemCondition[side] = 0;
        if ((battleSys.BattleType & BattleTypeLinkTagMask) == BattleTypeLinkTagMask && BattleSystems.BattleSystem_BattlerSlot(battleSys, battler) == BattlerSlotPlayerSecond)
        {
            return 0;
        }
        BattleMon mon = battleCtx.BattleMons[battler];
        if ((mon.MoveEffectsMask & MoveEffectEmbargo) != 0)
        {
            return 0;
        }
        Party party = BattleSystems.BattleSystem_GetParty(battleSys, battler);
        for (int i = 0; i < Party.Party_GetCurrentCount(party); i++)
        {
            Pokemon partyMon = Party.Party_GetPokemonBySlotIndex(party, i);
            if (Pokemon.Pokemon_GetValue(partyMon, MonDataCurHP, null) != 0
                && Pokemon.Pokemon_GetValue(partyMon, MonDataSpeciesEgg, null) != 0
                && Pokemon.Pokemon_GetValue(partyMon, MonDataSpeciesEgg, null) != SpeciesEgg)
            {
                alive++;
            }
        }
        for (int i = 0; i < MaxMoves; i++)
        {
            if (i != 0 && alive > ai.TrainerItemCounts[side] - i + 1)
            {
                continue;
            }
            ushort item = ai.TrainerItems[side, i];
            if (item == 0)
            {
                continue;
            }
            if (item == ItemFullRestore)
            {
                if ((uint)mon.CurHP < mon.MaxHP / 4 && mon.CurHP != 0)
                {
                    ai.UsedItemType[side] = UsedItemTypeFullRestore;
                    result = 1;
                }
            }
            else if (AI_ItemHasParam(battleCtx, item, ItemParamHealHP))
            {
                byte amount = (byte)BattleContexts.BattleSystem_GetItemData(battleCtx, item, ItemParamHPRestore);
                if (amount != 0 && mon.CurHP != 0 && ((uint)mon.CurHP < mon.MaxHP / 4 || mon.MaxHP - (uint)mon.CurHP > amount))
                {
                    ai.UsedItemType[side] = UsedItemTypeHealHP;
                    result = 1;
                }
            }
            else if (AI_ItemHasParam(battleCtx, item, ItemParamHealSleep))
            {
                if ((mon.Status & StatusSleep) != 0)
                {
                    AI_UseStatusItem(battleCtx, side, 5);
                    result = 1;
                }
            }
            else if (AI_ItemHasParam(battleCtx, item, ItemParamHealPoison))
            {
                if ((mon.Status & StatusPoison) != 0 || (mon.Status & StatusToxic) != 0)
                {
                    AI_UseStatusItem(battleCtx, side, 4);
                    result = 1;
                }
            }
            else if (AI_ItemHasParam(battleCtx, item, ItemParamHealBurn))
            {
                if ((mon.Status & StatusBurn) != 0)
                {
                    AI_UseStatusItem(battleCtx, side, 3);
                    result = 1;
                }
            }
            else if (AI_ItemHasParam(battleCtx, item, ItemParamHealFreeze))
            {
                if ((mon.Status & StatusFreeze) != 0)
                {
                    AI_UseStatusItem(battleCtx, side, 2);
                    result = 1;
                }
            }
            else if (AI_ItemHasParam(battleCtx, item, ItemParamHealParalysis))
            {
                if ((mon.Status & StatusParalysis) != 0)
                {
                    AI_UseStatusItem(battleCtx, side, 1);
                    result = 1;
                }
            }
            else if (AI_ItemHasParam(battleCtx, item, ItemParamHealConfusion))
            {
                if ((mon.StatusVolatile & VolatileConfusion) != 0)
                {
                    AI_UseStatusItem(battleCtx, side, 0);
                    result = 1;
                }
            }
            else if (mon.MoveEffectsData.FakeOutTurnNumber - battleCtx.TotalTurns < 0)
            {
                ai.UsedItemType[side] = UsedItemTypeNone;
            }
            else if (AI_ItemHasParam(battleCtx, item, ItemParamAttackBoost))
            {
                ai.UsedItemCondition[side] = 1;
                ai.UsedItemType[side] = UsedItemTypeStatBoost;
                result = 1;
            }
            else if (AI_ItemHasParam(battleCtx, item, ItemParamDefenseBoost))
            {
                ai.UsedItemCondition[side] = 2;
                ai.UsedItemType[side] = UsedItemTypeStatBoost;
                result = 1;
            }
            else if (AI_ItemHasParam(battleCtx, item, ItemParamSpAttackBoost))
            {
                ai.UsedItemCondition[side] = 4;
                ai.UsedItemType[side] = UsedItemTypeStatBoost;
                result = 1;
            }
            else if (AI_ItemHasParam(battleCtx, item, ItemParamSpDefenseBoost))
            {
                ai.UsedItemCondition[side] = 5;
                ai.UsedItemType[side] = UsedItemTypeStatBoost;
                result = 1;
            }
            else if (AI_ItemHasParam(battleCtx, item, ItemParamSpeedBoost))
            {
                ai.UsedItemCondition[side] = 3;
                ai.UsedItemType[side] = UsedItemTypeStatBoost;
                result = 1;
            }
            else if (AI_ItemHasParam(battleCtx, item, ItemParamAccuracyBoost))
            {
                ai.UsedItemCondition[side] = 6;
                ai.UsedItemType[side] = UsedItemTypeStatBoost;
                result = 1;
            }
            else if (AI_ItemHasParam(battleCtx, item, ItemParamGuardSpec))
            {
                if ((battleCtx.SideConditionsMask[1] & SideConditionMist) == 0)
                {
                    ai.UsedItemType[side] = UsedItemTypeGuardSpec;
                    result = 1;
                }
            }
            if (result == 1)
            {
                ai.UsedItem[side] = item;
                ai.TrainerItems[side, i] = 0;
            }
        }
        return result;
    }
}
