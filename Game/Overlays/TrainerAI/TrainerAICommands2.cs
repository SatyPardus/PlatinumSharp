namespace PlatinumSharp.Game;

public static partial class TrainerAI
{
    private const int TypeChartBaseDamage = 40;
    private const uint MoveStatusIneffective = 0x140808;
    private const int MonDataStatus = 0xA0;
    private const int MonDataMaxHP = 0xA4;
    private const int MonDataMove1 = 0x36;
    private const int MonDataMove1CurPP = 0x3A;
    private const int MonDataMove1MaxPP = 0x42;
    private const int MonDataHPIV = 0x46;
    private const int MonDataHeldItem = 6;
    private const int MonDataAbility = 0xA;
    private const uint FieldConditionRain = 0x3;
    private const uint FieldConditionSandstorm = 0xC;
    private const uint FieldConditionSun = 0x30;
    private const uint FieldConditionHail = 0xC0;
    private const uint FieldConditionFog = 0x8000;
    private const int WeatherNone = 0;
    private const int WeatherSun = 1;
    private const int WeatherRain = 2;
    private const int WeatherSandstorm = 3;
    private const int WeatherHail = 4;
    private const int WeatherFog = 5;
    private const int SideConditionSpikes = 4;
    private const int SideConditionToxicSpikes = 0x400;
    private const ushort MoveProtect = 0xB6;
    private const ushort MoveDetect = 0xC5;
    private const ushort MoveEndure = 0xCB;
    private const int StatStageNeutral = 6;
    private const int BattleStatCount = 8;
    private const int StatHP = 0;
    private const int StatAttack = 1;
    private const int StatDefense = 2;
    private const int StatSpeed = 3;
    private const int StatSpAttack = 4;
    private const int StatSpDefense = 5;

    private static int AI_TypeChartMultiplier(BattleSystem battleSys, BattleContext battleCtx, int move)
    {
        AIContext ai = battleCtx.AiContext;
        uint flags = 0;
        int type = TrainerAI_MoveType(battleSys, battleCtx, ai.Attacker, move);
        int multiplier = BattleContexts.BattleSystem_ApplyTypeChart(battleSys, battleCtx, move, type, ai.Attacker, ai.Defender, TypeChartBaseDamage, ref flags);
        switch (multiplier)
        {
            case 120:
                multiplier = 80;
                break;
            case 240:
                multiplier = 160;
                break;
            case 30:
                multiplier = 20;
                break;
            case 15:
                multiplier = 10;
                break;
        }
        if ((flags & MoveStatusIneffective) != 0)
        {
            multiplier = 0;
        }
        return multiplier;
    }

    // OFFSET 0x02220F88
    private static void AICmd_CheckBattlerAbility(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int ability = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        BattleMon mon = battleCtx.BattleMons[battler];
        int known;
        if ((mon.MoveEffectsMask & MoveEffectGastroAcid) != 0)
        {
            known = 0;
        }
        else if (inBattler == AIBattlerDefender || inBattler == AIBattlerDefenderPartner)
        {
            if (ai.BattlerAbilities[battler] != 0)
            {
                known = ai.BattlerAbilities[battler];
                ai.CalcTemp = known;
            }
            else
            {
                known = mon.Ability;
                if (known != AbilityShadowTag && known != AbilityMagnetPull && known != AbilityArenaTrap)
                {
                    int ability1 = (int)SpeciesData.SpeciesData_GetSpeciesValue(mon.Species, SpeciesDataAbility1);
                    int ability2 = (int)SpeciesData.SpeciesData_GetSpeciesValue(mon.Species, SpeciesDataAbility2);
                    known = ability1;
                    if (ability1 != 0 && ability2 != 0)
                    {
                        if (ability1 == ability || ability2 == ability)
                        {
                            known = 0;
                        }
                    }
                    else if (ability1 == 0)
                    {
                        known = ability2;
                    }
                }
            }
        }
        else
        {
            known = mon.Ability;
        }
        if (known == 0)
        {
            ai.CalcTemp = 2;
        }
        else if (known == ability)
        {
            ai.CalcTemp = 1;
        }
        else
        {
            ai.CalcTemp = 0;
        }
    }

    // OFFSET 0x02221060
    private static void AICmd_CalcMaxEffectiveness(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        ai.CalcTemp = 0;
        for (int i = 0; i < MaxMoves; i++)
        {
            ushort move = battleCtx.BattleMons[ai.Attacker].Moves[i];
            if (move == 0)
            {
                TrainerAI_MoveType(battleSys, battleCtx, ai.Attacker, move);
                continue;
            }
            int multiplier = AI_TypeChartMultiplier(battleSys, battleCtx, move);
            if ((uint)ai.CalcTemp < (uint)multiplier)
            {
                ai.CalcTemp = multiplier;
            }
        }
    }

    // OFFSET 0x02221114
    private static void AICmd_IfMoveEffectivenessEquals(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int value = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (AI_TypeChartMultiplier(battleSys, battleCtx, battleCtx.AiContext.Move) == value)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    private static void AI_PartySlots(BattleSystem battleSys, BattleContext battleCtx, int battler, out int slot1, out int slot2)
    {
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
    }

    private static bool AI_IsReserveMon(Pokemon mon, int slot, int slot1, int slot2)
    {
        return slot != slot1 && slot != slot2
            && Pokemon.Pokemon_GetValue(mon, MonDataCurHP, null) != 0
            && Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEgg, null) != 0
            && Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEgg, null) != SpeciesEgg;
    }

    private static void AI_IfPartyMemberStatus(BattleSystem battleSys, BattleContext battleCtx, bool set)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        uint mask = (uint)AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        AI_PartySlots(battleSys, battleCtx, battler, out int slot1, out int slot2);
        Party party = BattleSystems.BattleSystem_GetParty(battleSys, battler);
        for (int i = 0; i < BattleSystems.BattleSystem_GetPartyCount(battleSys, battler); i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, i);
            if (AI_IsReserveMon(mon, i, slot1, slot2) && ((Pokemon.Pokemon_GetValue(mon, MonDataStatus, null) & mask) != 0) == set)
            {
                AIScript_Iter(battleCtx, jump);
                return;
            }
        }
    }

    // OFFSET 0x022211AC
    private static void AICmd_IfPartyMemberStatus(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfPartyMemberStatus(battleSys, battleCtx, true);
    }

    // OFFSET 0x022212A0
    private static void AICmd_IfPartyMemberNotStatus(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfPartyMemberStatus(battleSys, battleCtx, false);
    }

    // OFFSET 0x02221394
    private static void AICmd_LoadCurrentWeather(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        ai.CalcTemp = WeatherNone;
        if ((battleCtx.FieldConditionsMask & FieldConditionRain) != 0)
        {
            ai.CalcTemp = WeatherRain;
        }
        if ((battleCtx.FieldConditionsMask & FieldConditionSandstorm) != 0)
        {
            ai.CalcTemp = WeatherSandstorm;
        }
        if ((battleCtx.FieldConditionsMask & FieldConditionSun) != 0)
        {
            ai.CalcTemp = WeatherSun;
        }
        if ((battleCtx.FieldConditionsMask & FieldConditionHail) != 0)
        {
            ai.CalcTemp = WeatherHail;
        }
        if ((battleCtx.FieldConditionsMask & FieldConditionFog) != 0)
        {
            ai.CalcTemp = WeatherFog;
        }
    }

    // OFFSET 0x0222140C
    private static void AICmd_IfCurrentMoveEffectEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int effect = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (effect == battleCtx.AiContext.MoveTable[battleCtx.AiContext.Move].Effect)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02221444
    private static void AICmd_IfCurrentMoveEffectNotEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int effect = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (effect != battleCtx.AiContext.MoveTable[battleCtx.AiContext.Move].Effect)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    private static void AI_IfStatStage(BattleContext battleCtx, Func<int, int, bool> compare)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int stat = AIScript_Read(battleCtx);
        int value = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        if (compare(battleCtx.BattleMons[battler].StatBoosts[stat], value))
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x0222147C
    private static void AICmd_IfStatStageLessThan(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfStatStage(battleCtx, (stage, value) => stage < value);
    }

    // OFFSET 0x022214D0
    private static void AICmd_IfStatStageGreaterThan(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfStatStage(battleCtx, (stage, value) => stage > value);
    }

    // OFFSET 0x02221524
    private static void AICmd_IfStatStageEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfStatStage(battleCtx, (stage, value) => value == stage);
    }

    // OFFSET 0x02221578
    private static void AICmd_IfStatStageNotEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfStatStage(battleCtx, (stage, value) => value != stage);
    }

    private static bool AI_CurrentMoveDamage(BattleSystem battleSys, BattleContext battleCtx, int useRoll, out int damage)
    {
        AIContext ai = battleCtx.AiContext;
        int variance = useRoll == 1 ? ai.MoveDamageRolls[ai.MoveSlot] : 100;
        damage = 0;
        if (!AI_MoveDealsDamage(ai, ai.Move))
        {
            return false;
        }
        byte[] ivs = AI_BattlerIVs(battleCtx, ai.Attacker);
        int attacker = ai.Attacker;
        int ability = BattleContexts.Battler_Ability(battleCtx, attacker);
        BattleMon mon = battleCtx.BattleMons[attacker];
        damage = TrainerAI_CalcDamage(battleSys, battleCtx, ai.Move, mon.HeldItem, ivs, attacker, ability, mon.MoveEffectsData.EmbargoTurns, (byte)variance);
        return true;
    }

    // OFFSET 0x022215CC
    private static void AICmd_IfCurrentMoveKills(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int useRoll = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (AI_CurrentMoveDamage(battleSys, battleCtx, useRoll, out int damage)
            && (uint)battleCtx.BattleMons[battleCtx.AiContext.Defender].CurHP <= (uint)damage)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x022216F8
    private static void AICmd_IfCurrentMoveDoesNotKill(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int useRoll = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (AI_CurrentMoveDamage(battleSys, battleCtx, useRoll, out int damage)
            && (uint)battleCtx.BattleMons[battleCtx.AiContext.Defender].CurHP > (uint)damage)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    private static int AI_FindMove(ushort[] moves, int move)
    {
        int i;
        for (i = 0; i < MaxMoves; i++)
        {
            if (move == moves[i])
            {
                break;
            }
        }
        return i;
    }

    private static ushort[] AI_KnownMoves(AIContext ai, int battler)
    {
        ushort[] moves = new ushort[MaxMoves];
        for (int i = 0; i < MaxMoves; i++)
        {
            moves[i] = ai.BattlerMoves[battler, i];
        }
        return moves;
    }

    private static void AI_IfMoveKnown(BattleContext battleCtx, bool known)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int move = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        int index;
        switch (inBattler)
        {
            case AIBattlerAttacker:
                index = AI_FindMove(battleCtx.BattleMons[battler].Moves, move);
                break;
            case AIBattlerAttackerPartner:
                if (battleCtx.BattleMons[battler].CurHP == 0)
                {
                    return;
                }
                index = AI_FindMove(battleCtx.BattleMons[battler].Moves, move);
                break;
            case AIBattlerDefender:
                index = AI_FindMove(AI_KnownMoves(battleCtx.AiContext, battler), move);
                break;
            default:
                return;
        }
        if ((index < MaxMoves) == known)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02221824
    private static void AICmd_IfMoveKnown(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfMoveKnown(battleCtx, true);
    }

    // OFFSET 0x022218E4
    private static void AICmd_IfMoveNotKnown(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfMoveKnown(battleCtx, false);
    }

    private static int AI_FindMoveEffect(AIContext ai, ushort[] moves, int effect)
    {
        int i;
        for (i = 0; i < MaxMoves; i++)
        {
            if (moves[i] != 0 && effect == ai.MoveTable[moves[i]].Effect)
            {
                break;
            }
        }
        return i;
    }

    private static void AI_IfMoveEffectKnown(BattleContext battleCtx, bool known)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int effect = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        int index;
        switch (inBattler)
        {
            case AIBattlerAttacker:
                index = AI_FindMoveEffect(ai, battleCtx.BattleMons[battler].Moves, effect);
                break;
            case AIBattlerDefender:
                index = AI_FindMoveEffect(ai, AI_KnownMoves(ai, battler), effect);
                break;
            default:
                return;
        }
        if ((index < MaxMoves) == known)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x022219A4
    private static void AICmd_IfMoveEffectKnown(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfMoveEffectKnown(battleCtx, true);
    }

    // OFFSET 0x02221A48
    private static void AICmd_IfMoveEffectNotKnown(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfMoveEffectKnown(battleCtx, false);
    }

    // OFFSET 0x02221AEC
    private static void AICmd_IfBattlerUnderEffect(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int effect = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        MoveEffectsData data = battleCtx.BattleMons[battler].MoveEffectsData;
        switch (effect)
        {
            case 0:
                if (data.DisabledTurns != 0)
                {
                    AIScript_Iter(battleCtx, jump);
                }
                break;
            case 1:
                if (data.EncoredTurns != 0)
                {
                    AIScript_Iter(battleCtx, jump);
                }
                break;
        }
    }

    // OFFSET 0x02221B5C
    private static void AICmd_IfCurrentMoveMatchesEffect(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        int effect = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        MoveEffectsData data = battleCtx.BattleMons[ai.Attacker].MoveEffectsData;
        switch (effect)
        {
            case 0:
                if (ai.Move == data.DisabledMove)
                {
                    AIScript_Iter(battleCtx, jump);
                }
                break;
            case 1:
                if (ai.Move == data.EncoredMove)
                {
                    AIScript_Iter(battleCtx, jump);
                }
                break;
        }
    }

    // OFFSET 0x02221BCC
    private static void AICmd_Escape(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        battleCtx.AiContext.StateFlags |= StateFlagMoveDone | StateFlagFlee | StateFlagEnd;
    }

    // OFFSET 0x02221BE8
    private static void AICmd_Dummy3E(BattleSystem battleSys, BattleContext battleCtx)
    {
    }

    // OFFSET 0x02221BEC
    private static void AICmd_Dummy3F(BattleSystem battleSys, BattleContext battleCtx)
    {
    }

    // OFFSET 0x02221BF0
    private static void AICmd_LoadHeldItem(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        byte battler = AIScript_Battler(battleCtx, (byte)AIScript_Read(battleCtx));
        battleCtx.AiContext.CalcTemp = battleCtx.BattleMons[battler].HeldItem;
    }

    // OFFSET 0x02221C24
    private static void AICmd_LoadHeldItemEffect(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        byte battler = AIScript_Battler(battleCtx, (byte)AIScript_Read(battleCtx));
        if (ai.Attacker != battler)
        {
            ai.CalcTemp = BattleContexts.BattleSystem_GetItemData(battleCtx, ai.BattlerHeldItems[battler], 1);
        }
        else
        {
            ai.CalcTemp = BattleContexts.BattleSystem_GetItemData(battleCtx, battleCtx.BattleMons[battler].HeldItem, 1);
        }
    }

    // OFFSET 0x02221C88
    private static void AICmd_IfHeldItemEqualTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int item = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        ushort held = (battler & 1) == (ai.Attacker & 1) ? battleCtx.BattleMons[battler].HeldItem : ai.BattlerHeldItems[battler];
        if (held == item)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02221CF0
    private static void AICmd_IfFieldConditionsMask(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        uint mask = (uint)AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if ((battleCtx.FieldConditionsMask & mask) != 0)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02221D20
    private static void AICmd_LoadSpikesLayers(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int condition = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler);
        switch (condition)
        {
            case SideConditionSpikes:
                battleCtx.AiContext.CalcTemp = battleCtx.SideConditions[side].SpikesLayers;
                break;
            case SideConditionToxicSpikes:
                battleCtx.AiContext.CalcTemp = battleCtx.SideConditions[side].ToxicSpikesLayers;
                break;
        }
    }

    // OFFSET 0x02221D88
    private static void AICmd_IfAnyPartyMemberIsWounded(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        for (int i = 0; i < BattleSystems.BattleSystem_GetPartyCount(battleSys, battler); i++)
        {
            Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, i);
            if (i != battleCtx.SelectedPartySlot[battler]
                && Pokemon.Pokemon_GetValue(mon, MonDataCurHP, null) != Pokemon.Pokemon_GetValue(mon, MonDataMaxHP, null))
            {
                AIScript_Iter(battleCtx, jump);
                return;
            }
        }
    }

    // OFFSET 0x02221E18
    private static void AICmd_IfAnyPartyMemberUsedPP(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        for (int i = 0; i < BattleSystems.BattleSystem_GetPartyCount(battleSys, battler); i++)
        {
            Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, i);
            if (i == battleCtx.SelectedPartySlot[battler])
            {
                continue;
            }
            int j;
            for (j = 0; j < MaxMoves; j++)
            {
                if (Pokemon.Pokemon_GetValue(mon, MonDataMove1CurPP + j, null) != Pokemon.Pokemon_GetValue(mon, MonDataMove1MaxPP + j, null))
                {
                    AIScript_Iter(battleCtx, jump);
                    break;
                }
            }
            if (j != MaxMoves)
            {
                return;
            }
        }
    }

    // OFFSET 0x02221EBC
    private static void AICmd_LoadFlingPower(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        byte battler = AIScript_Battler(battleCtx, (byte)AIScript_Read(battleCtx));
        battleCtx.AiContext.CalcTemp = BattleContexts.Battler_ItemFlingPower(battleCtx, battler);
    }

    // OFFSET 0x02221EEC
    private static void AICmd_LoadCurrentMovePP(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        ai.CalcTemp = battleCtx.BattleMons[ai.Attacker].PpCur[ai.MoveSlot];
    }

    // OFFSET 0x02221F1C
    private static void AICmd_IfCanUseLastResort(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        int count = BattleContexts.Battler_CountMoves(battleSys, battleCtx, battler);
        if ((uint)battleCtx.BattleMons[battler].MoveEffectsData.LastResortCount >= (uint)(count - 1) && count > 1)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02221F78
    private static void AICmd_LoadCurrentMoveClass(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        battleCtx.AiContext.CalcTemp = battleCtx.AiContext.MoveTable[battleCtx.AiContext.Move].Class;
    }

    // OFFSET 0x02221F9C
    private static void AICmd_LoadDefenderLastUsedMoveClass(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        ai.CalcTemp = ai.MoveTable[battleCtx.MovePrevByBattler[ai.Defender]].Class;
    }

    // OFFSET 0x02221FCC
    private static void AICmd_LoadBattlerSpeedRank(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        byte battler = AIScript_Battler(battleCtx, (byte)AIScript_Read(battleCtx));
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        int[] order = new int[MaxBattlers];
        for (int i = 0; i < maxBattlers; i++)
        {
            order[i] = i;
        }
        for (int i = 0; i < maxBattlers - 1; i++)
        {
            for (int j = i + 1; j < maxBattlers; j++)
            {
                int first = order[i];
                int second = order[j];
                if (BattleContexts.BattleSystem_CompareBattlerSpeed(battleSys, battleCtx, first, second, 1) != 0)
                {
                    order[i] = second;
                    order[j] = first;
                }
            }
        }
        for (int i = 0; i < maxBattlers; i++)
        {
            if (battler == order[i])
            {
                battleCtx.AiContext.CalcTemp = i;
                return;
            }
        }
    }

    // OFFSET 0x02222090
    private static void AICmd_LoadBattlerTurnCount(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        byte battler = AIScript_Battler(battleCtx, (byte)AIScript_Read(battleCtx));
        battleCtx.AiContext.CalcTemp = battleCtx.TotalTurns - battleCtx.BattleMons[battler].MoveEffectsData.FakeOutTurnNumber;
    }

    // OFFSET 0x022220CC
    private static void AICmd_IfPartyMemberDealsMoreDamage(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int varyDamage = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        int attacker = battleCtx.AiContext.Attacker;
        byte[] ivs = AI_BattlerIVs(battleCtx, attacker);
        int[] damage = new int[MaxMoves];
        BattleMon battleMon = battleCtx.BattleMons[attacker];
        int maxDamage = TrainerAI_CalcAllDamage(battleSys, battleCtx, attacker, battleMon.Moves, damage, battleMon.HeldItem, ivs, BattleContexts.Battler_Ability(battleCtx, attacker), battleMon.MoveEffectsData.EmbargoTurns, varyDamage);
        for (int i = 0; i < BattleSystems.BattleSystem_GetPartyCount(battleSys, attacker); i++)
        {
            if (i == battleCtx.SelectedPartySlot[attacker])
            {
                continue;
            }
            Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, attacker, i);
            if (Pokemon.Pokemon_GetValue(mon, MonDataCurHP, null) == 0
                || Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEgg, null) == 0
                || Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEgg, null) == SpeciesEgg)
            {
                continue;
            }
            ushort[] moves = new ushort[MaxMoves];
            for (int j = 0; j < MaxMoves; j++)
            {
                moves[j] = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + j, null);
            }
            for (int j = 0; j < 6; j++)
            {
                ivs[j] = (byte)Pokemon.Pokemon_GetValue(mon, MonDataHPIV + j, null);
            }
            ushort heldItem = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataHeldItem, null);
            int ability = (int)Pokemon.Pokemon_GetValue(mon, MonDataAbility, null);
            if (TrainerAI_CalcAllDamage(battleSys, battleCtx, attacker, moves, damage, heldItem, ivs, ability, 0, varyDamage) > maxDamage)
            {
                AIScript_Iter(battleCtx, jump);
                return;
            }
        }
    }

    // OFFSET 0x02222260
    private static void AICmd_IfHasSuperEffectiveMove(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int jump = AIScript_Read(battleCtx);
        if (AI_HasSuperEffectiveMove(battleSys, battleCtx, battleCtx.AiContext.Attacker, 1) == 1)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02222298
    private static void AICmd_IfBattlerDealsMoreDamage(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int varyDamage = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        int attacker = ai.Attacker;
        byte[] ivs = AI_BattlerIVs(battleCtx, attacker);
        int[] damage = new int[MaxMoves];
        BattleMon attackerMon = battleCtx.BattleMons[attacker];
        int maxDamage = TrainerAI_CalcAllDamage(battleSys, battleCtx, attacker, attackerMon.Moves, damage, attackerMon.HeldItem, ivs, BattleContexts.Battler_Ability(battleCtx, attacker), attackerMon.MoveEffectsData.EmbargoTurns, varyDamage);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        int variance = varyDamage == 1 ? ai.MoveDamageRolls[ai.MoveSlot] : 100;
        BattleMon mon = battleCtx.BattleMons[battler];
        int ability = BattleContexts.Battler_Ability(battleCtx, battler);
        int battlerDamage = TrainerAI_CalcDamage(battleSys, battleCtx, battleCtx.MovePrevByBattler[battler], mon.HeldItem, ivs, battler, ability, mon.MoveEffectsData.EmbargoTurns, (byte)variance);
        if (battlerDamage > maxDamage)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x022223B0
    private static void AICmd_SumPositiveStatStages(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        byte battler = AIScript_Battler(battleCtx, (byte)AIScript_Read(battleCtx));
        battleCtx.AiContext.CalcTemp = 0;
        for (int i = 0; i < BattleStatCount; i++)
        {
            int stage = battleCtx.BattleMons[battler].StatBoosts[i];
            if (stage > StatStageNeutral)
            {
                battleCtx.AiContext.CalcTemp += stage - StatStageNeutral;
            }
        }
    }

    // OFFSET 0x02222400
    private static void AICmd_DiffStatStages(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int stat = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        ai.CalcTemp = battleCtx.BattleMons[battler].StatBoosts[stat] - battleCtx.BattleMons[ai.Attacker].StatBoosts[stat];
    }

    private static void AI_IfBattlerStat(BattleContext battleCtx, Func<int, int, bool> compare)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int stat = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        TrainerAI_GetStats(battleCtx, battler, out int attackerStat, out int battlerStat, stat);
        if (compare(attackerStat, battlerStat))
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02222450
    private static void AICmd_IfBattlerHasHigherStat(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfBattlerStat(battleCtx, (attackerStat, battlerStat) => attackerStat < battlerStat);
    }

    // OFFSET 0x022224A4
    private static void AICmd_IfBattlerHasLowerStat(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfBattlerStat(battleCtx, (attackerStat, battlerStat) => attackerStat > battlerStat);
    }

    // OFFSET 0x022224F8
    private static void AICmd_IfBattlerHasEqualStat(BattleSystem battleSys, BattleContext battleCtx)
    {
        AI_IfBattlerStat(battleCtx, (attackerStat, battlerStat) => attackerStat == battlerStat);
    }

    // OFFSET 0x0222254C
    private static void TrainerAI_GetStats(BattleContext battleCtx, int battler, out int attackerStat, out int battlerStat, int stat)
    {
        BattleMon attacker = battleCtx.BattleMons[battleCtx.AiContext.Attacker];
        BattleMon mon = battleCtx.BattleMons[battler];
        switch (stat)
        {
            case StatHP:
                attackerStat = attacker.CurHP;
                battlerStat = mon.CurHP;
                return;
            case StatAttack:
                attackerStat = attacker.Attack;
                battlerStat = mon.Attack;
                return;
            case StatDefense:
                attackerStat = attacker.Defense;
                battlerStat = mon.Defense;
                return;
            case StatSpeed:
                attackerStat = attacker.Speed;
                battlerStat = mon.Speed;
                return;
            case StatSpAttack:
                attackerStat = attacker.SpAttack;
                battlerStat = mon.SpAttack;
                return;
            case StatSpDefense:
                attackerStat = attacker.SpDefense;
                battlerStat = mon.SpDefense;
                return;
        }
        ErrorHandling.GF_AssertFail();
        attackerStat = 0;
        battlerStat = 0;
    }

    // OFFSET 0x02222648
    private static void AICmd_CheckIfHighestDamageWithPartner(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        int varyDamage = AIScript_Read(battleCtx);
        if (!AI_MoveDealsDamage(ai, ai.Move))
        {
            ai.CalcTemp = DamageScoreNone;
            return;
        }
        int battler = ai.Attacker;
        int current = 0;
        int[] damage = new int[MaxMoves];
        for (int k = 0; k < 2; k++)
        {
            byte[] ivs = AI_BattlerIVs(battleCtx, battler);
            BattleMon mon = battleCtx.BattleMons[battler];
            TrainerAI_CalcAllDamage(battleSys, battleCtx, battler, mon.Moves, damage, mon.HeldItem, ivs, BattleContexts.Battler_Ability(battleCtx, battler), mon.MoveEffectsData.EmbargoTurns, varyDamage);
            battler = BattleSystems.BattleSystem_GetPartner(battleSys, ai.Attacker);
            if (k == 0)
            {
                current = damage[ai.MoveSlot];
            }
            int i;
            for (i = 0; i < MaxMoves; i++)
            {
                if (damage[i] > current)
                {
                    break;
                }
            }
            if (i != MaxMoves)
            {
                ai.CalcTemp = DamageScoreNotHighest;
                return;
            }
            ai.CalcTemp = DamageScoreHighest;
        }
    }

    // OFFSET 0x022227A4
    private static void AICmd_IfBattlerFainted(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (inBattler == AIBattlerAttacker)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (inBattler == AIBattlerDefender)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((HealthBar.FlagIndex(AIScript_Battler(battleCtx, (byte)inBattler)) & battleCtx.BattlersSwitchingMask) != 0)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x022227F4
    private static void AICmd_IfBattlerNotFainted(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        if (inBattler == AIBattlerAttacker)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (inBattler == AIBattlerDefender)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((HealthBar.FlagIndex(AIScript_Battler(battleCtx, (byte)inBattler)) & battleCtx.BattlersSwitchingMask) == 0)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02222844
    private static void AICmd_LoadGender(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        byte battler = AIScript_Battler(battleCtx, (byte)AIScript_Read(battleCtx));
        battleCtx.AiContext.CalcTemp = battleCtx.BattleMons[battler].Gender;
    }

    // OFFSET 0x0222287C
    private static void AICmd_LoadIsFirstTurnInBattle(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        byte battler = AIScript_Battler(battleCtx, (byte)AIScript_Read(battleCtx));
        battleCtx.AiContext.CalcTemp = battleCtx.BattleMons[battler].MoveEffectsData.FakeOutTurnNumber < battleCtx.TotalTurns ? 0 : 1;
    }

    // OFFSET 0x022228C8
    private static void AICmd_LoadStockpileCount(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        byte battler = AIScript_Battler(battleCtx, (byte)AIScript_Read(battleCtx));
        battleCtx.AiContext.CalcTemp = battleCtx.BattleMons[battler].MoveEffectsData.StockpileCount;
    }

    // OFFSET 0x02222900
    private static void AICmd_LoadBattleType(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        battleCtx.AiContext.CalcTemp = (int)battleSys.BattleType;
    }

    // OFFSET 0x02222918
    private static void AICmd_LoadRecycleItem(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        byte battler = AIScript_Battler(battleCtx, (byte)AIScript_Read(battleCtx));
        battleCtx.AiContext.CalcTemp = battleCtx.RecycleItem[battler];
    }

    // OFFSET 0x0222294C
    private static void AICmd_LoadTypeOfLoadedMove(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        battleCtx.AiContext.CalcTemp = battleCtx.AiContext.MoveTable[battleCtx.AiContext.CalcTemp].Type;
    }

    // OFFSET 0x0222296C
    private static void AICmd_LoadPowerOfLoadedMove(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        battleCtx.AiContext.CalcTemp = battleCtx.AiContext.MoveTable[battleCtx.AiContext.CalcTemp].Power;
    }

    // OFFSET 0x0222298C
    private static void AICmd_LoadEffectOfLoadedMove(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        battleCtx.AiContext.CalcTemp = battleCtx.AiContext.MoveTable[battleCtx.AiContext.CalcTemp].Effect;
    }

    // OFFSET 0x022229AC
    private static void AICmd_LoadProtectChain(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        byte battler = AIScript_Battler(battleCtx, (byte)AIScript_Read(battleCtx));
        ushort move = battleCtx.MoveProtect[battler];
        if (move != MoveProtect && move != MoveDetect && move != MoveEndure)
        {
            battleCtx.AiContext.CalcTemp = 0;
            return;
        }
        battleCtx.AiContext.CalcTemp = battleCtx.BattleMons[battler].MoveEffectsData.ProtectSuccessTurns;
    }

    // OFFSET 0x02222A08
    private static void AICmd_PushAndGoTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int offset = AIScript_Read(battleCtx);
        AIScript_PushCursor(battleSys, battleCtx, offset);
    }

    // OFFSET 0x02222A28
    private static void AICmd_GoTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int offset = AIScript_Read(battleCtx);
        AIScript_Iter(battleCtx, offset);
    }

    // OFFSET 0x02222A44
    private static void AICmd_PopOrEnd(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        if (AIScript_PopCursor(battleSys, battleCtx) != 1)
        {
            battleCtx.AiContext.StateFlags |= StateFlagMoveDone;
        }
    }

    // OFFSET 0x02222A6C
    private static void AICmd_IfLevel(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        AIScript_Iter(battleCtx, 1);
        int compare = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte attackerLevel = battleCtx.BattleMons[ai.Attacker].Level;
        byte defenderLevel = battleCtx.BattleMons[ai.Defender].Level;
        switch (compare)
        {
            case 0:
                if (attackerLevel > defenderLevel)
                {
                    AIScript_Iter(battleCtx, jump);
                }
                break;
            case 1:
                if (attackerLevel < defenderLevel)
                {
                    AIScript_Iter(battleCtx, jump);
                }
                break;
            case 2:
                if (attackerLevel == defenderLevel)
                {
                    AIScript_Iter(battleCtx, jump);
                }
                break;
        }
    }

    // OFFSET 0x02222B10
    private static void AICmd_IfTargetIsTaunted(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int jump = AIScript_Read(battleCtx);
        if (battleCtx.BattleMons[battleCtx.AiContext.Defender].MoveEffectsData.TauntedTurns != 0)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02222B48
    private static void AICmd_IfTargetIsNotTaunted(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int jump = AIScript_Read(battleCtx);
        if (battleCtx.BattleMons[battleCtx.AiContext.Defender].MoveEffectsData.TauntedTurns == 0)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02222B80
    private static void AICmd_IfTargetIsPartner(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int jump = AIScript_Read(battleCtx);
        if ((battleCtx.AiContext.Attacker & 1) == (battleCtx.AiContext.Defender & 1))
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02222BB4
    private static void AICmd_IfActivatedFlashFire(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        int inBattler = AIScript_Read(battleCtx);
        int jump = AIScript_Read(battleCtx);
        byte battler = AIScript_Battler(battleCtx, (byte)inBattler);
        if (battleCtx.BattleMons[battler].MoveEffectsData.FlashFire != 0)
        {
            AIScript_Iter(battleCtx, jump);
        }
    }

    // OFFSET 0x02222BF8
    private static void AICmd_LoadAbility(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIScript_Iter(battleCtx, 1);
        byte battler = AIScript_Battler(battleCtx, (byte)AIScript_Read(battleCtx));
        battleCtx.AiContext.CalcTemp = BattleContexts.Battler_Ability(battleCtx, battler);
    }
}
