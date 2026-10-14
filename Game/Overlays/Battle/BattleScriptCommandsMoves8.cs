namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const uint MoveEffectLeechSeed = 4;
    private const uint MoveEffectLeechSeedBattler = 3;
    private const uint SideConditionToxicSpikes = 0x400;
    private const uint SideConditionStealthRock = 0x80;
    private const uint SideConditionFutureSight = 0x10;
    private const int SubseqBindRelease = 0x74;
    private const int SubseqHazardsBlownAway = 0x75;
    private const int MoveLeechSeed = 0x49;
    private const int MoveSpikes = 0xBF;
    private const int MoveToxicSpikes = 0x186;
    private const int MoveStealthRock = 0x1BE;
    private const uint FieldConditionWeatherAny = 0x80FF;
    private const int FutureSightTurns = 3;

    // OFFSET 0x02245890
    private static int BattleScript_RapidSpin(BattleSystem battleSys, BattleContext battleCtx)
    {
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
        BattleMon attacker = battleCtx.BattleMons[battleCtx.Attacker];
        if ((attacker.StatusVolatile & VolatileBindTurns) != 0)
        {
            attacker.StatusVolatile &= ~VolatileBindTurns;
            battleCtx.MsgBattlerTemp = attacker.MoveEffectsData.BindTarget;
            battleCtx.MsgMoveTemp = attacker.MoveEffectsData.BindingMove;
            BattleScript_Call(battleCtx, NarcSubSeq, SubseqBindRelease);
            return 0;
        }
        if ((attacker.MoveEffectsMask & MoveEffectLeechSeed) != 0)
        {
            attacker.MoveEffectsMask &= ~MoveEffectLeechSeed;
            battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsMask &= ~MoveEffectLeechSeedBattler;
            battleCtx.MsgMoveTemp = MoveLeechSeed;
            BattleScript_Call(battleCtx, NarcSubSeq, SubseqHazardsBlownAway);
            return 0;
        }
        SideConditions conditions = battleCtx.SideConditions[side];
        if (conditions.SpikesLayers != 0)
        {
            battleCtx.SideConditionsMask[side] &= ~SideConditionSpikes;
            conditions.SpikesLayers = 0;
            battleCtx.MsgMoveTemp = MoveSpikes;
            BattleScript_Call(battleCtx, NarcSubSeq, SubseqHazardsBlownAway);
            return 0;
        }
        if (conditions.ToxicSpikesLayers != 0)
        {
            battleCtx.SideConditionsMask[side] &= ~SideConditionToxicSpikes;
            conditions.ToxicSpikesLayers = 0;
            battleCtx.MsgMoveTemp = MoveToxicSpikes;
            BattleScript_Call(battleCtx, NarcSubSeq, SubseqHazardsBlownAway);
            return 0;
        }
        if ((battleCtx.SideConditionsMask[side] & SideConditionStealthRock) != 0)
        {
            battleCtx.SideConditionsMask[side] &= ~SideConditionStealthRock;
            battleCtx.MsgMoveTemp = MoveStealthRock;
            BattleScript_Call(battleCtx, NarcSubSeq, SubseqHazardsBlownAway);
            return 0;
        }
        BattleScript_Iter(battleCtx, 1);
        return 0;
    }

    // OFFSET 0x022459D8
    private static int BattleScript_WeatherHPRecovery(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleMon attacker = battleCtx.BattleMons[battleCtx.Attacker];
        if ((battleCtx.FieldConditionsMask & FieldConditionWeatherAny) == 0
            || BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAllBattlers, 0, AbilityCloudNine) != 0
            || BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAllBattlers, 0, AbilityAirLock) != 0)
        {
            battleCtx.HpCalcTemp = (int)(attacker.MaxHP >> 1);
        }
        else if ((battleCtx.FieldConditionsMask & FieldConditionSun) != 0)
        {
            battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)(attacker.MaxHP * 20), 30);
        }
        else
        {
            battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)attacker.MaxHP, 4);
        }
        return 0;
    }

    // OFFSET 0x02245A7C
    private static int BattleScript_CalcHiddenPowerParams(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleMon mon = battleCtx.BattleMons[battleCtx.Attacker];
        battleCtx.MovePower = ((mon.HpIV & 2) >> 1)
            | (mon.AttackIV & 2)
            | ((mon.DefenseIV & 2) << 1)
            | ((mon.SpeedIV & 2) << 2)
            | ((mon.SpAttackIV & 2) << 3)
            | ((mon.SpDefenseIV & 2) << 4);
        battleCtx.MoveType = (mon.HpIV & 1)
            | ((mon.AttackIV & 1) << 1)
            | ((mon.DefenseIV & 1) << 2)
            | ((mon.SpeedIV & 1) << 3)
            | ((mon.SpAttackIV & 1) << 4)
            | ((mon.SpDefenseIV & 1) << 5);
        battleCtx.MovePower = battleCtx.MovePower * 40 / 63 + 30;
        battleCtx.MoveType = battleCtx.MoveType * 15 / 63 + 1;
        if (battleCtx.MoveType >= TypeMystery)
        {
            battleCtx.MoveType++;
        }
        return 0;
    }

    // OFFSET 0x02245B68
    private static int BattleScript_CopyStatStages(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        for (int stat = 0; stat < 8; stat++)
        {
            battleCtx.BattleMons[battleCtx.Attacker].StatBoosts[stat] = battleCtx.BattleMons[battleCtx.Defender].StatBoosts[stat];
        }
        battleCtx.BattleMons[battleCtx.Attacker].StatusVolatile |= battleCtx.BattleMons[battleCtx.Defender].StatusVolatile & VolatileFocusEnergy;
        return 0;
    }

    // OFFSET 0x02245BC8
    private static int BattleScript_TryFutureSight(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        FieldConditions field = battleCtx.FieldConditions;
        if (field.FutureSightTurns[battleCtx.Defender] != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Defender);
        battleCtx.SideConditionsMask[side] |= SideConditionFutureSight;
        field.FutureSightTurns[battleCtx.Defender] = FutureSightTurns;
        field.FutureSightMove[battleCtx.Defender] = (ushort)battleCtx.MoveCur;
        field.FutureSightAttacker[battleCtx.Defender] = battleCtx.Attacker;
        int damage = BattleContexts.BattleSystem_CalcMoveDamage(battleSys, battleCtx, (int)battleCtx.MoveCur, battleCtx.SideConditionsMask[side], battleCtx.FieldConditionsMask, 0, 0, (byte)battleCtx.Attacker, (byte)battleCtx.Defender, 1);
        field.FutureSightDamage[battleCtx.Defender] = BattleContexts.BattleSystem_CalcDamageVariance(battleSys, battleCtx, damage * -1);
        if (battleCtx.TurnFlags[battleCtx.Attacker].HelpingHand != 0)
        {
            field.FutureSightDamage[battleCtx.Defender] = field.FutureSightDamage[battleCtx.Defender] * 15 / 10;
        }
        return 0;
    }
}
