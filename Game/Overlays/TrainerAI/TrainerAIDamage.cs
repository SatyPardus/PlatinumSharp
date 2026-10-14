namespace PlatinumSharp.Game;

public static partial class TrainerAI
{
    private const ushort MoveSonicBoom = 0x31;
    private const ushort MoveLowKick = 0x43;
    private const ushort MoveSeismicToss = 0x45;
    private const ushort MoveDragonRage = 0x52;
    private const ushort MoveNightShade = 0x65;
    private const ushort MovePsywave = 0x95;
    private const ushort MoveReturn = 0xD8;
    private const ushort MoveFrustration = 0xDA;
    private const ushort MoveMagnitude = 0xDE;
    private const ushort MoveHiddenPower = 0xED;
    private const ushort MoveWeatherBall = 0x137;
    private const ushort MoveGyroBall = 0x168;
    private const ushort MoveNaturalGift = 0x16B;
    private const ushort MoveGrassKnot = 0x1BF;
    private const ushort MoveJudgment = 0x1C1;
    private const int AbilityKlutz = 0x67;
    private const int AbilityCloudNine = 0xD;
    private const int AbilityAirLock = 0x4C;
    private const int ItemParamHoldEffect = 1;
    private const int ItemParamNaturalGiftPower = 0xB;
    private const int ItemParamNaturalGiftType = 0xC;
    private const int HoldEffectFirstPlate = 0x7E;
    private const int PlateCount = 16;
    private const int CountAbilityFieldMode = 8;
    private const uint FieldConditionWeatherMask = 0x80FF;
    private const uint BattleStatusIgnoreType = 0x800;
    private const uint WeightTable = 0x0222EE60;
    private const int WeightTableDefaultPower = 120;
    private const int GyroBallMaxPower = 150;
    private const int TypeMystery = 9;
    private const int TypeWater = 0xB;
    private const int TypeRock = 5;
    private const int TypeFire = 0xA;
    private const int TypeIce = 0xF;
    private const uint MoveEffectPerishSong = 0x20;
    private const byte PartySlotNone = 6;

    private static readonly int[] PlateTypes = { 0xA, 0xB, 0xD, 0xC, 0xF, 1, 3, 4, 2, 0xE, 6, 5, 7, 0x10, 0x11, 8 };
    private static readonly int[] MagnitudeThresholds = { 5, 15, 35, 65, 85, 95 };
    private static readonly int[] MagnitudePowers = { 10, 30, 50, 70, 90, 110 };

    private static int AI_PlateType(int holdEffect)
    {
        int index = holdEffect - HoldEffectFirstPlate;
        if ((uint)index >= PlateCount)
        {
            return 0;
        }
        return PlateTypes[index];
    }

    private static int AI_HiddenPowerType(int hp, int attack, int defense, int speed, int spAttack, int spDefense)
    {
        int bits = (hp & 1) | ((attack & 1) << 1) | ((defense & 1) << 2) | ((speed & 1) << 3) | ((spAttack & 1) << 4) | ((spDefense & 1) << 5);
        int type = bits * 15 / 63 + 1;
        if (type >= TypeMystery)
        {
            type++;
        }
        return type;
    }

    // OFFSET 0x02222E84
    private static int TrainerAI_CalcDamage(BattleSystem battleSys, BattleContext battleCtx, ushort move, ushort heldItem, byte[] ivs, int attacker, int ability, int embargoTurns, byte variance)
    {
        AIContext ai = battleCtx.AiContext;
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, ai.Defender);
        int power = 0;
        int type = 0;
        int damage = 0;
        uint flags = 0;
        switch (move)
        {
            case MoveReturn:
                power = battleCtx.BattleMons[attacker].Friendship * 10 / 25;
                break;
            case MoveDragonRage:
                damage = 40;
                break;
            case MoveSonicBoom:
                damage = 20;
                break;
            case MoveLowKick:
            case MoveGrassKnot:
            {
                ushort[] table = RomData.OverlayU16(Overlay, WeightTable, 0x18);
                int weight = battleCtx.BattleMons[ai.Defender].Weight;
                int i = 0;
                while (table[i * 2] < weight)
                {
                    i++;
                    if (table[i * 2] == TableEnd)
                    {
                        break;
                    }
                }
                power = table[i * 2] != TableEnd ? table[i * 2 + 1] : WeightTableDefaultPower;
                break;
            }
            case MoveSeismicToss:
            case MoveNightShade:
                damage = battleCtx.BattleMons[attacker].Level;
                break;
            case MovePsywave:
            {
                int roll = (int)(BattleSystems.BattleSystem_RandNext(battleSys) % 11);
                damage = (roll + 5) * battleCtx.BattleMons[attacker].Level / 10;
                break;
            }
            case MoveGyroBall:
                power = (int)(battleCtx.MonSpeedValues[ai.Defender] * 25 / battleCtx.MonSpeedValues[attacker]) + 1;
                if (power > GyroBallMaxPower)
                {
                    power = GyroBallMaxPower;
                }
                type = 0;
                break;
            case MoveFrustration:
                power = (255 - battleCtx.BattleMons[attacker].Friendship) * 10 / 25;
                break;
            case MoveMagnitude:
            {
                int roll = (int)(BattleSystems.BattleSystem_RandNext(battleSys) % 100);
                power = 150;
                for (int i = 0; i < MagnitudeThresholds.Length; i++)
                {
                    if (roll < MagnitudeThresholds[i])
                    {
                        power = MagnitudePowers[i];
                        break;
                    }
                }
                type = 0;
                break;
            }
            case MoveHiddenPower:
            {
                int bits = ((ivs[0] & 2) >> 1) | (ivs[1] & 2) | ((ivs[2] & 2) << 1) | ((ivs[3] & 2) << 2) | ((ivs[4] & 2) << 3) | ((ivs[5] & 2) << 4);
                power = bits * 40 / 63 + 30;
                type = AI_HiddenPowerType(ivs[0], ivs[1], ivs[2], ivs[3], ivs[4], ivs[5]);
                break;
            }
            case MoveNaturalGift:
                if (ability != AbilityKlutz && embargoTurns == 0)
                {
                    power = BattleContexts.BattleSystem_GetItemData(battleCtx, heldItem, ItemParamNaturalGiftPower);
                    if (power != 0)
                    {
                        type = BattleContexts.BattleSystem_GetItemData(battleCtx, heldItem, ItemParamNaturalGiftType);
                    }
                }
                break;
            case MoveJudgment:
                if (ability != AbilityKlutz && embargoTurns == 0)
                {
                    type = AI_PlateType(BattleContexts.BattleSystem_GetItemData(battleCtx, heldItem, ItemParamHoldEffect));
                }
                break;
        }
        if (damage == 0)
        {
            damage = BattleContexts.BattleSystem_CalcMoveDamage(battleSys, battleCtx, move, battleCtx.SideConditionsMask[side], battleCtx.FieldConditionsMask, (ushort)power, (byte)type, (byte)attacker, ai.Defender, 1);
        }
        else
        {
            battleCtx.BattleStatusMask |= BattleStatusIgnoreType;
        }
        damage = BattleContexts.BattleSystem_ApplyTypeChart(battleSys, battleCtx, move, type, attacker, ai.Defender, damage, ref flags);
        battleCtx.BattleStatusMask &= ~BattleStatusIgnoreType;
        if ((flags & MoveStatusIneffective) != 0)
        {
            return 0;
        }
        return BattleContexts.BattleSystem_Divide(damage * variance, 100);
    }

    // OFFSET 0x0222327C
    private static int TrainerAI_MoveType(BattleSystem battleSys, BattleContext battleCtx, int battler, int move)
    {
        int type = 0;
        switch (move)
        {
            case MoveWeatherBall:
                if (BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityFieldMode, 0, AbilityCloudNine) == 0
                    && BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityFieldMode, 0, AbilityAirLock) == 0
                    && (battleCtx.FieldConditionsMask & FieldConditionWeatherMask) != 0)
                {
                    uint field = battleCtx.FieldConditionsMask;
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
                break;
            case MoveNaturalGift:
                type = BattleContexts.Battler_NaturalGiftType(battleCtx, battler);
                break;
            case MoveJudgment:
                type = AI_PlateType(BattleContexts.Battler_HeldItemEffect(battleCtx, battler));
                break;
            case MoveHiddenPower:
            {
                BattleMon mon = battleCtx.BattleMons[battler];
                type = AI_HiddenPowerType(mon.HpIV, mon.AttackIV, mon.DefenseIV, mon.SpeedIV, mon.SpAttackIV, mon.SpDefenseIV);
                break;
            }
        }
        return type;
    }

    // OFFSET 0x022233F4
    private static int AI_PerishSongKO(BattleContext battleCtx, int battler)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        if ((mon.MoveEffectsMask & MoveEffectPerishSong) != 0 && mon.MoveEffectsData.PerishSongTurns == 0)
        {
            battleCtx.AiSwitchedPartySlot[battler] = PartySlotNone;
            return 1;
        }
        return 0;
    }
}
