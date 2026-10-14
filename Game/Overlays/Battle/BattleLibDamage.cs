namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const int AbilityHugePower = 0x25;
    private const int AbilityThickFat = 0x2F;
    private const int AbilityHustle = 0x37;
    private const int AbilityPlus = 0x39;
    private const int AbilityMinus = 0x3A;
    private const int AbilityGuts = 0x3E;
    private const int AbilityMarvelScale = 0x3F;
    private const int AbilityOvergrow = 0x41;
    private const int AbilityBlaze = 0x42;
    private const int AbilityTorrent = 0x43;
    private const int AbilitySwarm = 0x44;
    private const int AbilityPurePower = 0x4A;
    private const int AbilityRivalry = 0x4F;
    private const int AbilityHeatproof = 0x55;
    private const int AbilityIronFist = 0x59;
    private const int AbilitySolarPower = 0x5E;
    private const int AbilityTechnician = 0x65;
    private const int AbilityUnaware = 0x6D;
    private const int AbilityFlowerGift = 0x7A;
    private const int HoldEffectGriseousOrbBoost = 2;
    private const int HoldEffectAdamantOrb = 3;
    private const int HoldEffectLustrousOrb = 4;
    private const int HoldEffectSoulDew = 0x3C;
    private const int HoldEffectDeepSeaTooth = 0x3D;
    private const int HoldEffectDeepSeaScale = 0x3E;
    private const int HoldEffectLightBall = 0x47;
    private const int HoldEffectMetalPowder = 0x5A;
    private const int HoldEffectThickClub = 0x5B;
    private const int HoldEffectMuscleBand = 0x5E;
    private const int HoldEffectWiseGlasses = 0x5F;
    private const int SpeciesPikachu = 0x19;
    private const int SpeciesCubone = 0x68;
    private const int SpeciesClamperl = 0x16E;
    private const int SpeciesLatias = 0x17C;
    private const int SpeciesDialga = 0x1E3;
    private const int SpeciesPalkia = 0x1E4;
    private const int TypeBug = 6;
    private const int TypeGrass = 0xC;
    private const int TypeDragon = 0x10;
    private const int TypeMask = 0x3F;
    private const int GenderNone = 2;
    private const int MoveSolarBeam = 0x4C;
    private const int MoveEffectHalveDefense = 7;
    private const int MoveEffectBrickBreak = 0xBA;
    private const int MoveClassPhysical = 0;
    private const int MoveClassSpecial = 1;
    private const uint MoveEffectCharge = 0x200;
    private const uint MoveEffectMudSport = 0x10000;
    private const uint MoveEffectWaterSport = 0x20000;
    private const uint SideConditionReflect = 0x1;
    private const uint SideConditionLightScreen = 0x2;
    private const uint FieldConditionSolarBeamWeak = 0x80CF;
    private const uint BattleTypeFrontier = 0x80;
    private const int TechnicianMaxPower = 60;
    private const uint TypeBoostItemTable = 0x0226EC92;
    private const int TypeBoostItemCount = 0x21;
    private const uint PunchMoveTable = 0x0226EC16;
    private const int PunchMoveCount = 15;
    private const int BattleContextTotalTurns = 3;

    private static byte[]? typeBoostItems;

    private static int Damage_ClampStage(int stage)
    {
        stage = (sbyte)(stage * 2);
        if (stage < -6)
        {
            stage = -6;
        }
        if (stage > 6)
        {
            stage = 6;
        }
        return stage;
    }

    private static int Damage_ApplyStage(int value, int stage)
    {
        return value * statStages![stage * 2] / statStages[stage * 2 + 1];
    }

    private static int Damage_ApplyScreen(BattleSystem battleSys, BattleContext battleCtx, uint battleType, int defender, int damage)
    {
        if ((battleType & BattleTypeDoubles) != 0 && BattleSystem_NumBattlersAlive(battleSys, battleCtx, CountAliveMySide, defender) == 2)
        {
            return damage * 2 / 3;
        }
        return damage / 2;
    }

    // OFFSET 0x0225A280
    public static int BattleSystem_CalcMoveDamage(BattleSystem battleSys, BattleContext battleCtx, int move, uint sideConditions, uint fieldConditions, ushort inPower, byte inType, byte attacker, byte defender, byte criticalMul)
    {
        int damage = 0;
        if (criticalMul < 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        statStages ??= RomData.Overlay(16, StatStageTable, 13 * 2);
        ushort attackStat = (ushort)BattleMon_Get(battleCtx, attacker, BattleMonAttack, null);
        ushort defenseStat = (ushort)BattleMon_Get(battleCtx, defender, BattleMonDefense, null);
        ushort spAttackStat = (ushort)BattleMon_Get(battleCtx, attacker, BattleMonSpAttack, null);
        ushort spDefenseStat = (ushort)BattleMon_Get(battleCtx, defender, BattleMonSpDefense, null);
        int attackStage = (sbyte)(BattleMon_Get(battleCtx, attacker, BattleMonStatBoost + 1, null) - 6);
        int defenseStage = (sbyte)(BattleMon_Get(battleCtx, defender, BattleMonStatBoost + 2, null) - 6);
        int spAttackStage = (sbyte)(BattleMon_Get(battleCtx, attacker, BattleMonStatBoost + 4, null) - 6);
        int spDefenseStage = (sbyte)(BattleMon_Get(battleCtx, defender, BattleMonStatBoost + 5, null) - 6);
        byte level = (byte)BattleMon_Get(battleCtx, attacker, BattleMonLevel, null);
        ushort attackerSpecies = (ushort)BattleMon_Get(battleCtx, attacker, BattleMonSpecies, null);
        ushort defenderSpecies = (ushort)BattleMon_Get(battleCtx, defender, BattleMonSpecies, null);
        int attackerHP = (short)BattleMon_Get(battleCtx, attacker, BattleMonCurHP, null);
        BattleMon_Get(battleCtx, defender, BattleMonCurHP, null);
        int attackerMaxHP = (ushort)BattleMon_Get(battleCtx, attacker, BattleMonMaxHP, null);
        BattleMon_Get(battleCtx, defender, BattleMonMaxHP, null);
        uint attackerStatus = (uint)BattleMon_Get(battleCtx, attacker, BattleMonStatus, null);
        uint defenderStatus = (uint)BattleMon_Get(battleCtx, defender, BattleMonStatus, null);
        int attackerAbility = Battler_Ability(battleCtx, attacker);
        Battler_Ability(battleCtx, defender);
        byte attackerGender = (byte)BattleMon_Get(battleCtx, attacker, BattleMonGender, null);
        byte defenderGender = (byte)BattleMon_Get(battleCtx, defender, BattleMonGender, null);
        BattleMon_Get(battleCtx, attacker, BattleMonType1, null);
        byte defenderType1 = (byte)BattleMon_Get(battleCtx, defender, BattleMonType1, null);
        BattleMon_Get(battleCtx, attacker, BattleMonType2, null);
        byte defenderType2 = (byte)BattleMon_Get(battleCtx, defender, BattleMonType2, null);
        ushort item = Battler_HeldItem(battleCtx, attacker);
        int attackerItemEffect = BattleSystem_GetItemData(battleCtx, item, ItemParamHoldEffect);
        int attackerItemPower = BattleSystem_GetItemData(battleCtx, item, ItemParamHoldPower);
        item = Battler_HeldItem(battleCtx, defender);
        int defenderItemEffect = BattleSystem_GetItemData(battleCtx, item, ItemParamHoldEffect);
        BattleSystem_GetItemData(battleCtx, item, ItemParamHoldPower);
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        MoveData moveData = battleCtx.AiContext.MoveTable[move];
        ushort power = inPower != 0 ? inPower : moveData.Power;
        byte type;
        if (attackerAbility == AbilityNormalize)
        {
            type = 0;
        }
        else if (inType == 0)
        {
            type = moveData.Type;
        }
        else
        {
            type = (byte)(inType & TypeMask);
        }
        if (battleCtx.PowerMul < 10)
        {
            ErrorHandling.GF_AssertFail();
        }
        power = (ushort)(power * battleCtx.PowerMul / 10);
        if ((battleCtx.BattleMons[attacker].MoveEffectsMask & MoveEffectCharge) != 0 && type == TypeElectric)
        {
            power = (ushort)(power * 2);
        }
        if (battleCtx.TurnFlags[attacker].HelpingHand != 0)
        {
            power = (ushort)(power * 15 / 10);
        }
        if (attackerAbility == AbilityTechnician && move != MoveStruggle && power <= TechnicianMaxPower)
        {
            power = (ushort)(power * 15 / 10);
        }
        int moveClass = moveData.Class;
        if (attackerAbility == AbilityHugePower || attackerAbility == AbilityPurePower)
        {
            attackStat = (ushort)(attackStat * 2);
        }
        if (attackerAbility == AbilitySlowStart)
        {
            int totalTurns = BattleContext_Get(battleSys, battleCtx, BattleContextTotalTurns, 0);
            if (totalTurns - BattleMon_Get(battleCtx, attacker, 0x59, null) < SlowStartTurns)
            {
                attackStat = (ushort)(attackStat / 2);
            }
        }
        typeBoostItems ??= RomData.Overlay(16, TypeBoostItemTable, TypeBoostItemCount * 2);
        for (int i = 0; i < TypeBoostItemCount; i++)
        {
            if (attackerItemEffect == typeBoostItems[i * 2] && type == typeBoostItems[i * 2 + 1])
            {
                power = (ushort)(power * (100 + attackerItemPower) / 100);
                break;
            }
        }
        if (attackerItemEffect == HoldEffectChoiceBand)
        {
            attackStat = (ushort)(attackStat * 150 / 100);
        }
        if (attackerItemEffect == HoldEffectChoiceSpecs)
        {
            spAttackStat = (ushort)(spAttackStat * 150 / 100);
        }
        if (attackerItemEffect == HoldEffectSoulDew && (battleType & BattleTypeFrontier) == 0 && (ushort)(attackerSpecies - SpeciesLatias) <= 1)
        {
            spAttackStat = (ushort)(spAttackStat * 150 / 100);
        }
        if (defenderItemEffect == HoldEffectSoulDew && (battleType & BattleTypeFrontier) == 0 && (ushort)(defenderSpecies - SpeciesLatias) <= 1)
        {
            spDefenseStat = (ushort)(spDefenseStat * 150 / 100);
        }
        if (attackerItemEffect == HoldEffectDeepSeaTooth && attackerSpecies == SpeciesClamperl)
        {
            spAttackStat = (ushort)(spAttackStat * 2);
        }
        if (defenderItemEffect == HoldEffectDeepSeaScale && defenderSpecies == SpeciesClamperl)
        {
            spDefenseStat = (ushort)(spDefenseStat * 2);
        }
        if (attackerItemEffect == HoldEffectLightBall && attackerSpecies == SpeciesPikachu)
        {
            power = (ushort)(power * 2);
        }
        if (defenderItemEffect == HoldEffectMetalPowder && defenderSpecies == SpeciesDitto)
        {
            defenseStat = (ushort)(defenseStat * 2);
        }
        if (attackerItemEffect == HoldEffectThickClub && (ushort)(attackerSpecies - SpeciesCubone) <= 1)
        {
            attackStat = (ushort)(attackStat * 2);
        }
        if (attackerItemEffect == HoldEffectAdamantOrb && (type == TypeDragon || type == TypeSteel) && attackerSpecies == SpeciesDialga)
        {
            power = (ushort)(power * (100 + attackerItemPower) / 100);
        }
        if (attackerItemEffect == HoldEffectLustrousOrb && (type == TypeDragon || type == TypeWater) && attackerSpecies == SpeciesPalkia)
        {
            power = (ushort)(power * (100 + attackerItemPower) / 100);
        }
        if (attackerItemEffect == HoldEffectGriseousOrbBoost && (type == TypeDragon || type == TypeGhost)
            && ((uint)BattleMon_Get(battleCtx, attacker, BattleMonStatusVolatile, null) & VolatileTransformed) == 0
            && attackerSpecies == SpeciesGiratinaForm)
        {
            power = (ushort)(power * (100 + attackerItemPower) / 100);
        }
        if (attackerItemEffect == HoldEffectMuscleBand && moveClass == MoveClassPhysical)
        {
            power = (ushort)(power * (100 + attackerItemPower) / 100);
        }
        if (attackerItemEffect == HoldEffectWiseGlasses && moveClass == MoveClassSpecial)
        {
            attackerItemPower += 100;
            power = (ushort)(power * attackerItemPower / 100);
        }
        if (Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityThickFat) == 1 && (type == TypeFire || type == TypeIce))
        {
            power = (ushort)(power / 2);
        }
        if (attackerAbility == AbilityHustle)
        {
            attackStat = (ushort)(attackStat * 150 / 100);
        }
        if (attackerAbility == AbilityGuts && attackerStatus != 0)
        {
            attackStat = (ushort)(attackStat * 150 / 100);
        }
        if (Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityMarvelScale) == 1 && defenderStatus != 0)
        {
            defenseStat = (ushort)(defenseStat * 150 / 100);
        }
        if (attackerAbility == AbilityPlus && BattleSystem_CountAbility(battleSys, battleCtx, CountAbilitySameSideAlive, attacker, AbilityMinus) != 0)
        {
            spAttackStat = (ushort)(spAttackStat * 150 / 100);
        }
        if (attackerAbility == AbilityMinus && BattleSystem_CountAbility(battleSys, battleCtx, CountAbilitySameSideAlive, attacker, AbilityPlus) != 0)
        {
            spAttackStat = (ushort)(spAttackStat * 150 / 100);
        }
        if (type == TypeElectric && BattleSystem_AnyBattlerWithMoveEffect(battleSys, battleCtx, MoveEffectMudSport) != 0)
        {
            power = (ushort)(power / 2);
        }
        if (type == TypeFire && BattleSystem_AnyBattlerWithMoveEffect(battleSys, battleCtx, MoveEffectWaterSport) != 0)
        {
            power = (ushort)(power / 2);
        }
        if (type == TypeGrass && attackerAbility == AbilityOvergrow && attackerHP <= attackerMaxHP / 3)
        {
            power = (ushort)(power * 150 / 100);
        }
        if (type == TypeFire && attackerAbility == AbilityBlaze && attackerHP <= attackerMaxHP / 3)
        {
            power = (ushort)(power * 150 / 100);
        }
        if (type == TypeWater && attackerAbility == AbilityTorrent && attackerHP <= attackerMaxHP / 3)
        {
            power = (ushort)(power * 150 / 100);
        }
        if (type == TypeBug && attackerAbility == AbilitySwarm && attackerHP <= attackerMaxHP / 3)
        {
            power = (ushort)(power * 150 / 100);
        }
        if (type == TypeFire && Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityHeatproof) == 1)
        {
            power = (ushort)(power / 2);
        }
        if (type == TypeFire && Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityDrySkin) == 1)
        {
            power = (ushort)(power * 125 / 100);
        }
        if (attackerAbility == AbilitySimple)
        {
            attackStage = Damage_ClampStage(attackStage);
            spAttackStage = Damage_ClampStage(spAttackStage);
        }
        if (Battler_IgnorableAbility(battleCtx, attacker, defender, AbilitySimple) == 1)
        {
            defenseStage = Damage_ClampStage(defenseStage);
            spDefenseStage = Damage_ClampStage(spDefenseStage);
        }
        if (Battler_IgnorableAbility(battleCtx, attacker, defender, AbilityUnaware) == 1)
        {
            attackStage = 0;
            spAttackStage = 0;
        }
        if (attackerAbility == AbilityUnaware)
        {
            defenseStage = 0;
            spDefenseStage = 0;
        }
        attackStage = (sbyte)(attackStage + 6);
        defenseStage = (sbyte)(defenseStage + 6);
        spAttackStage = (sbyte)(spAttackStage + 6);
        spDefenseStage = (sbyte)(spDefenseStage + 6);
        if (attackerAbility == AbilityRivalry && attackerGender == defenderGender && attackerGender != GenderNone && defenderGender != GenderNone)
        {
            power = (ushort)(power * 125 / 100);
        }
        if (attackerAbility == AbilityRivalry && attackerGender != defenderGender && attackerGender != GenderNone && defenderGender != GenderNone)
        {
            power = (ushort)(power * 75 / 100);
        }
        ushort[] punchMoves = RomData.OverlayU16(16, PunchMoveTable, PunchMoveCount);
        for (int i = 0; i < PunchMoveCount; i++)
        {
            if (move == punchMoves[i] && attackerAbility == AbilityIronFist)
            {
                power = (ushort)(power * 12 / 10);
                break;
            }
        }
        if (!BattleSystem_WeatherNegated(battleSys, battleCtx))
        {
            uint sun = fieldConditions & FieldConditionSun;
            if (sun != 0 && attackerAbility == AbilitySolarPower)
            {
                spAttackStat = (ushort)(spAttackStat * 15 / 10);
            }
            if ((fieldConditions & FieldConditionSandstorm) != 0 && (defenderType1 == TypeRock || defenderType2 == TypeRock))
            {
                spDefenseStat = (ushort)(spDefenseStat * 15 / 10);
            }
            if (sun != 0 && BattleSystem_CountAbility(battleSys, battleCtx, CountAbilitySameSideAlive, attacker, AbilityFlowerGift) != 0)
            {
                attackStat = (ushort)(attackStat * 15 / 10);
            }
            if (sun != 0 && Battler_Ability(battleCtx, attacker) != AbilityMoldBreaker
                && BattleSystem_CountAbility(battleSys, battleCtx, CountAbilitySameSideAlive, defender, AbilityFlowerGift) != 0)
            {
                spDefenseStat = (ushort)(spDefenseStat * 15 / 10);
            }
        }
        int effect = moveData.Effect;
        if (effect == MoveEffectHalveDefense)
        {
            defenseStat = (ushort)(defenseStat / 2);
        }
        if (moveClass == MoveClassPhysical)
        {
            if (criticalMul > 1)
            {
                damage = attackStage > 6 ? Damage_ApplyStage(attackStat, attackStage) : attackStat;
            }
            else
            {
                damage = Damage_ApplyStage(attackStat, attackStage);
            }
            damage *= power;
            damage *= level * 2 / 5 + 2;
            int defense = defenseStat;
            if (criticalMul > 1)
            {
                if (defenseStage < 6)
                {
                    defense = Damage_ApplyStage(defense, defenseStage);
                }
            }
            else
            {
                defense = Damage_ApplyStage(defense, defenseStage);
            }
            damage /= defense;
            damage /= 50;
            if ((attackerStatus & MonConditionBurned) != 0 && attackerAbility != AbilityGuts)
            {
                damage /= 2;
            }
            if ((sideConditions & SideConditionReflect) != 0 && criticalMul == 1 && effect != MoveEffectBrickBreak)
            {
                damage = Damage_ApplyScreen(battleSys, battleCtx, battleType, defender, damage);
            }
        }
        else if (moveClass == MoveClassSpecial)
        {
            if (criticalMul > 1)
            {
                damage = spAttackStage > 6 ? Damage_ApplyStage(spAttackStat, spAttackStage) : spAttackStat;
            }
            else
            {
                damage = Damage_ApplyStage(spAttackStat, spAttackStage);
            }
            damage *= power;
            damage *= level * 2 / 5 + 2;
            int defense = spDefenseStat;
            if (criticalMul > 1)
            {
                if (spDefenseStage < 6)
                {
                    defense = Damage_ApplyStage(defense, spDefenseStage);
                }
            }
            else
            {
                defense = Damage_ApplyStage(defense, spDefenseStage);
            }
            damage /= defense;
            damage /= 50;
            if ((sideConditions & SideConditionLightScreen) != 0 && criticalMul == 1 && effect != MoveEffectBrickBreak)
            {
                damage = Damage_ApplyScreen(battleSys, battleCtx, battleType, defender, damage);
            }
        }
        bool doubles = (battleType & BattleTypeDoubles) != 0;
        if (doubles && moveData.Range == RangeAdjacentOpponents && BattleSystem_NumBattlersAlive(battleSys, battleCtx, CountAliveMySide, defender) == 2)
        {
            damage = damage * 3 / 4;
        }
        if (doubles && moveData.Range == RangeAllAdjacent && BattleSystem_NumBattlersAlive(battleSys, battleCtx, CountAliveExceptMe, defender) >= 2)
        {
            damage = damage * 3 / 4;
        }
        if (!BattleSystem_WeatherNegated(battleSys, battleCtx))
        {
            if ((fieldConditions & FieldConditionRain) != 0)
            {
                if (type == TypeFire)
                {
                    damage /= 2;
                }
                else if (type == TypeWater)
                {
                    damage = damage * 15 / 10;
                }
            }
            if ((fieldConditions & FieldConditionSolarBeamWeak) != 0 && move == MoveSolarBeam)
            {
                damage /= 2;
            }
            if ((fieldConditions & FieldConditionSun) != 0)
            {
                if (type == TypeFire)
                {
                    damage = damage * 15 / 10;
                }
                else if (type == TypeWater)
                {
                    damage /= 2;
                }
            }
        }
        if (BattleMon_Get(battleCtx, attacker, 0x49, null) != 0 && type == TypeFire)
        {
            damage = damage * 15 / 10;
        }
        return damage + 2;
    }
}
