namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int FuryCutterMax = 5;
    private const int GenderNone = 2;
    private const uint VolatileAttraction = 0xF0000;
    private const int VolatileAttractionShift = 16;
    private const uint SideConditionSafeguard = 8;
    private const int SafeguardTurns = 5;
    private const int TagNicknameSingle = 1;
    private const int MsgSafeguardVeil = 0xC6;

    // OFFSET 0x02245568
    private static int BattleScript_CalcFuryCutterPower(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        MoveEffectsData effects = battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData;
        if (effects.FuryCutterCount < FuryCutterMax)
        {
            effects.FuryCutterCount = (effects.FuryCutterCount + 1) & 7;
        }
        battleCtx.MovePower = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Power;
        for (int i = 1; i < battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.FuryCutterCount; i++)
        {
            battleCtx.MovePower *= 2;
        }
        return 0;
    }

    // OFFSET 0x022455F8
    private static int BattleScript_TryAttract(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        BattleMon target = battleCtx.BattleMons[battleCtx.SideEffectMon];
        int targetGender = target.Gender & 0xF;
        int sourceGender = battleCtx.BattleMons[battleCtx.MsgBattlerTemp].Gender & 0xF;
        if (sourceGender == targetGender || (target.StatusVolatile & VolatileAttraction) != 0 || sourceGender == GenderNone || targetGender == GenderNone)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else
        {
            battleCtx.BattleMons[battleCtx.SideEffectMon].StatusVolatile |= HealthBar.FlagIndex(battleCtx.MsgBattlerTemp) << VolatileAttractionShift;
        }
        return 0;
    }

    // OFFSET 0x0224567C
    private static int BattleScript_TrySafeguard(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
        if ((battleCtx.SideConditionsMask[side] & SideConditionSafeguard) != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            battleCtx.MoveStatusFlags |= MoveStatusFailed;
            return 0;
        }
        battleCtx.SideConditionsMask[side] |= SideConditionSafeguard;
        battleCtx.SideConditions[side].SafeguardTurns = SafeguardTurns;
        battleCtx.SideConditions[side].SafeguardUser = battleCtx.Attacker & 3;
        battleCtx.MsgBuffer.Tags = TagNicknameSingle;
        battleCtx.MsgBuffer.Params[0] = battleCtx.Attacker;
        battleCtx.MsgBuffer.Id = MsgSafeguardVeil;
        return 0;
    }

    // OFFSET 0x02245710
    private static int BattleScript_Present(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpIfHeal = BattleScript_Read(battleCtx);
        int rand = BattleSystems.BattleSystem_RandNext(battleSys) & 0xFF;
        if (rand < 0x66)
        {
            battleCtx.MovePower = 40;
        }
        else if (rand < 0xB2)
        {
            battleCtx.MovePower = 80;
        }
        else if (rand < 0xCC)
        {
            battleCtx.MovePower = 120;
        }
        else
        {
            battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)battleCtx.BattleMons[battleCtx.Defender].MaxHP, 4);
            BattleScript_Iter(battleCtx, jumpIfHeal);
        }
        return 0;
    }

    // OFFSET 0x02245784
    private static int BattleScript_CalcMagnitudePower(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        if (battleCtx.Magnitude == 0)
        {
            battleCtx.Magnitude = (byte)(BattleSystems.BattleSystem_RandNext(battleSys) % 100);
            if (battleCtx.Magnitude < 5)
            {
                battleCtx.MovePower = 10;
                battleCtx.Magnitude = 4;
            }
            else if (battleCtx.Magnitude < 15)
            {
                battleCtx.MovePower = 30;
                battleCtx.Magnitude = 5;
            }
            else if (battleCtx.Magnitude < 35)
            {
                battleCtx.MovePower = 50;
                battleCtx.Magnitude = 6;
            }
            else if (battleCtx.Magnitude < 65)
            {
                battleCtx.MovePower = 70;
                battleCtx.Magnitude = 7;
            }
            else if (battleCtx.Magnitude < 85)
            {
                battleCtx.MovePower = 90;
                battleCtx.Magnitude = 8;
            }
            else if (battleCtx.Magnitude < 95)
            {
                battleCtx.MovePower = 110;
                battleCtx.Magnitude = 9;
            }
            else
            {
                battleCtx.MovePower = 150;
                battleCtx.Magnitude = 10;
            }
        }
        battleCtx.MsgTemp = battleCtx.Magnitude;
        return 0;
    }

    // OFFSET 0x0224582C
    private static int BattleScript_TryReplaceFaintedMon(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int setSwitching = BattleScript_Read(battleCtx);
        int jumpIfNone = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        if (BattleContexts.BattleSystem_AnyReplacementMons(battleSys, battleCtx, battler) == 0)
        {
            BattleScript_Iter(battleCtx, jumpIfNone);
        }
        else if (setSwitching == 1)
        {
            battleCtx.BattlerStatusFlags[battler] |= BattlerStatusSwitching;
        }
        return 0;
    }
}
