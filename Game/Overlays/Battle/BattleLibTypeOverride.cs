namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const int MoveHiddenPower = 0xED;
    private const int MoveWeatherBall = 0x137;
    private const int MoveNaturalGift = 0x16B;
    private const int MoveJudgment = 0x1C1;
    private const int HoldEffectPlateFirst = 0x7E;
    private const uint FieldConditionAnyWeather = 0x80FF;
    private const uint FieldConditionSandstorm = 0xC;
    private const int TypeRock = 5;
    private const uint AnticipationIgnoredEffectTable = 0x0226EBBC;
    private const int AnticipationIgnoredEffectCount = 6;
    private const int SpeciesEgg = 0x1EE;
    private const int PartySlotNone = 6;
    private const byte AllSlotsExcluded = 0x3F;
    private const uint MoveStatusImmuneMask = 0x140808;
    private const int MonDataSpeciesEggID = 0xAE;

    private static readonly byte[] PlateTypes = { 0xA, 0xB, 0xD, 0xC, 0xF, 1, 3, 4, 2, 0xE, 6, 5, 7, 0x10, 0x11, 8 };

    private static bool Ability_CannotBeTraced(int ability)
    {
        return ability == AbilityForecast || ability == AbilityTrace || ability == AbilityMultitype;
    }

    // OFFSET 0x0225B840
    public static int BattleSystem_ChooseTraceTarget(BattleSystem battleSys, BattleContext battleCtx, int defender1, int defender2)
    {
        int result = BattlerNone;
        BattleMon mon1 = battleCtx.BattleMons[defender1];
        BattleMon mon2 = battleCtx.BattleMons[defender2];
        bool valid1 = !Ability_CannotBeTraced(mon1.Ability) && mon1.CurHP != 0;
        bool valid2 = !Ability_CannotBeTraced(mon2.Ability) && mon2.CurHP != 0;
        if (valid1 && valid2)
        {
            result = (BattleSystems.BattleSystem_RandNext(battleSys) & 1) != 0 ? defender2 : defender1;
        }
        else if (valid1)
        {
            result = defender1;
        }
        else if (valid2)
        {
            result = defender2;
        }
        return result;
    }

    // OFFSET 0x0225B8E4
    public static int Move_IsIgnoredByAnticipation(BattleContext battleCtx, ushort move)
    {
        ushort[] effects = RomData.OverlayU16(16, AnticipationIgnoredEffectTable, AnticipationIgnoredEffectCount);
        int effect = battleCtx.AiContext.MoveTable[move].Effect;
        for (int i = 0; i < AnticipationIgnoredEffectCount; i++)
        {
            if (effects[i] == effect)
            {
                return 1;
            }
        }
        return 0;
    }

    private static int HiddenPower_Type(int hp, int atk, int def, int speed, int spAtk, int spDef)
    {
        int bits = (hp & 1) | ((atk & 1) << 1) | ((def & 1) << 2) | ((speed & 1) << 3) | ((spAtk & 1) << 4) | ((spDef & 1) << 5);
        int type = bits * 15 / 63 + 1;
        if (type >= 9)
        {
            type++;
        }
        return type;
    }

    private static int Plate_Type(int holdEffect)
    {
        int index = holdEffect - HoldEffectPlateFirst;
        if (index < 0 || index >= PlateTypes.Length)
        {
            return 0;
        }
        return PlateTypes[index];
    }

    private static int WeatherBall_Type(BattleSystem battleSys, BattleContext battleCtx)
    {
        int type = 0;
        if (BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityAllAlive, 0, AbilityCloudNine) == 0
            && BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityAllAlive, 0, AbilityAirLock) == 0)
        {
            uint field = battleCtx.FieldConditionsMask;
            if ((field & FieldConditionAnyWeather) != 0)
            {
                if ((field & FieldConditionRain) != 0)
                {
                    type = TypeWater;
                }
                if ((field & FieldConditionSandstorm) != 0)
                {
                    type = TypeRock;
                }
                if ((field & FieldConditionSun) != 0)
                {
                    type = TypeFire;
                }
                if ((field & FieldConditionHail) != 0)
                {
                    type = TypeIce;
                }
            }
        }
        return type;
    }

    // OFFSET 0x0225B910
    public static int BattleSystem_MoveTypeOverride(BattleSystem battleSys, BattleContext battleCtx, int battler, int move)
    {
        switch (move)
        {
            case MoveNaturalGift:
                return Battler_NaturalGiftType(battleCtx, battler);
            case MoveJudgment:
                return Plate_Type(Battler_HeldItemEffect(battleCtx, battler));
            case MoveHiddenPower:
            {
                BattleMon mon = battleCtx.BattleMons[battler];
                return HiddenPower_Type(mon.HpIV, mon.AttackIV, mon.DefenseIV, mon.SpeedIV, mon.SpAttackIV, mon.SpDefenseIV);
            }
            case MoveWeatherBall:
                return WeatherBall_Type(battleSys, battleCtx);
        }
        return 0;
    }

    private static bool BattleAI_CanSwitchTo(BattleSystem battleSys, BattleContext battleCtx, int battler, int partner, int slot, out Pokemon mon)
    {
        mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, slot);
        ushort species = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEggID, null);
        return species != 0 && species != SpeciesEgg
            && Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null) != 0
            && slot != battleCtx.SelectedPartySlot[battler]
            && slot != battleCtx.SelectedPartySlot[partner]
            && slot != battleCtx.AiSwitchedPartySlot[battler]
            && slot != battleCtx.AiSwitchedPartySlot[partner];
    }

    // OFFSET 0x0225BA88
    public static int BattleAI_PostKOSwitchIn(BattleSystem battleSys, int inBattler)
    {
        BattleContext battleCtx = BattleSystems.BattleSystem_GetBattleContext(battleSys)!;
        byte battler = (byte)inBattler;
        byte partner;
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeTag) != 0 || (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeMulti) != 0)
        {
            partner = battler;
        }
        else
        {
            partner = (byte)BattleSystems.BattleSystem_GetPartner(battleSys, inBattler);
        }
        byte defender = (byte)BattleSystem_RandomOpponent(battleSys, battleCtx, inBattler);
        int partyCount = BattleSystems.BattleSystem_GetPartyCount(battleSys, inBattler);
        byte excluded = 0;
        byte score = 0;
        while (excluded != AllSlotsExcluded)
        {
            byte bestScore = 0;
            byte bestSlot = PartySlotNone;
            for (int i = 0; i < partyCount; i++)
            {
                if ((HealthBar.FlagIndex(i) & excluded) == 0 && BattleAI_CanSwitchTo(battleSys, battleCtx, battler, partner, i, out Pokemon candidate))
                {
                    byte defenderType1 = (byte)BattleMon_Get(battleCtx, defender, BattleMonType1, null);
                    byte defenderType2 = (byte)BattleMon_Get(battleCtx, defender, BattleMonType2, null);
                    byte monType1 = (byte)Pokemon.Pokemon_GetValue(candidate, MonDataType1, null);
                    byte monType2 = (byte)Pokemon.Pokemon_GetValue(candidate, MonDataType2, null);
                    score = (byte)BattleSystem_TypeMatchupMultiplier(monType1, defenderType1, defenderType2);
                    score = (byte)(score + BattleSystem_TypeMatchupMultiplier(monType2, defenderType1, defenderType2));
                    if (bestScore < score)
                    {
                        bestScore = score;
                        bestSlot = (byte)i;
                    }
                }
                else
                {
                    excluded = (byte)(excluded | HealthBar.FlagIndex(i));
                }
            }
            if (bestSlot != PartySlotNone)
            {
                Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, inBattler, bestSlot);
                int slot;
                for (slot = 0; slot < MoveSlotCount; slot++)
                {
                    ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + slot, null);
                    int type = BattleSystem_PartyMonMoveType(battleSys, battleCtx, mon, move);
                    if (move != 0)
                    {
                        uint effectiveness = 0;
                        BattleSystem_CalcEffectiveness(battleCtx, move, type,
                            (int)Pokemon.Pokemon_GetValue(mon, MonDataAbility, null),
                            Battler_Ability(battleCtx, defender),
                            Battler_HeldItemEffect(battleCtx, defender),
                            BattleMon_Get(battleCtx, defender, BattleMonType1, null),
                            BattleMon_Get(battleCtx, defender, BattleMonType2, null),
                            ref effectiveness);
                        if ((effectiveness & MoveStatusSuperEffective) != 0)
                        {
                            break;
                        }
                    }
                }
                if (slot == MoveSlotCount)
                {
                    excluded = (byte)(excluded | HealthBar.FlagIndex(bestSlot));
                }
                else
                {
                    return bestSlot;
                }
            }
            else
            {
                excluded = AllSlotsExcluded;
            }
        }
        byte bestDamage = 0;
        byte result = PartySlotNone;
        for (int i = 0; i < partyCount; i++)
        {
            if (!BattleAI_CanSwitchTo(battleSys, battleCtx, battler, partner, i, out Pokemon mon))
            {
                continue;
            }
            for (int slot = 0; slot < MoveSlotCount; slot++)
            {
                ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + slot, null);
                int type = BattleSystem_PartyMonMoveType(battleSys, battleCtx, mon, move);
                if (move != 0 && battleCtx.AiContext.MoveTable[move].Power != 1)
                {
                    int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, defender);
                    score = (byte)BattleSystem_CalcMoveDamage(battleSys, battleCtx, move, battleCtx.SideConditionsMask[side], battleCtx.FieldConditionsMask, 0, 0, battler, defender, 1);
                    uint effectiveness = 0;
                    score = (byte)BattleSystem_ApplyTypeChart(battleSys, battleCtx, move, type, inBattler, defender, score, ref effectiveness);
                    if ((effectiveness & MoveStatusImmuneMask) != 0)
                    {
                        score = 0;
                    }
                }
                if (bestDamage < score)
                {
                    bestDamage = score;
                    result = (byte)i;
                }
            }
        }
        return result;
    }

    // OFFSET 0x0225BE28
    public static int BattleAI_SwitchedSlot(BattleSystem battleSys, int battler)
    {
        return BattleSystems.BattleSystem_GetBattleContext(battleSys)!.AiSwitchedPartySlot[battler];
    }

    // OFFSET 0x0225BE3C
    public static int BattleSystem_PartyMonMoveType(BattleSystem battleSys, BattleContext battleCtx, Pokemon mon, int move)
    {
        switch (move)
        {
            case MoveNaturalGift:
                return BattleSystem_GetItemData(battleCtx, (ushort)Pokemon.Pokemon_GetValue(mon, MonDataHeldItemID, null), ItemParamNaturalGiftType);
            case MoveJudgment:
                return Plate_Type(BattleSystem_GetItemData(battleCtx, (ushort)Pokemon.Pokemon_GetValue(mon, MonDataHeldItemID, null), ItemParamHoldEffect));
            case MoveHiddenPower:
            {
                int spDef = (int)Pokemon.Pokemon_GetValue(mon, MonDataSpDefIV, null);
                int spAtk = (int)Pokemon.Pokemon_GetValue(mon, MonDataSpAtkIV, null);
                int speed = (int)Pokemon.Pokemon_GetValue(mon, MonDataSpeedIV, null);
                int def = (int)Pokemon.Pokemon_GetValue(mon, MonDataDefIV, null);
                int hp = (int)Pokemon.Pokemon_GetValue(mon, MonDataHpIV, null);
                int atk = (int)Pokemon.Pokemon_GetValue(mon, MonDataAtkIV, null);
                return HiddenPower_Type(hp, atk, def, speed, spAtk, spDef);
            }
            case MoveWeatherBall:
                return WeatherBall_Type(battleSys, battleCtx);
        }
        return 0;
    }
}
