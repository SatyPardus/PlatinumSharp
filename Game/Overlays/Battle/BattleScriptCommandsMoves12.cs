namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const uint MoveEffectGrudge = 0x4000;
    private const uint WeightPowerTableAddress = 0x0226E5F0;
    private const int WeightTableEnd = 0xFFFF;
    private const int WeightPowerMax = 0x78;
    private const int TypeWater = 0xB;
    private const int TypeFire = 0xA;
    private const uint StatusSleepOrFreeze = 0x27;
    private const int EffectPursuit = 0x80;
    private const int AbilityPressure = 0x2E;
    private const int PursuitPowerMul = 20;
    private const int HoldEffectChoiceBand = 0x37;
    private const int HoldEffectChoiceScarf = 0x73;
    private const int HoldEffectChoiceSpecs = 0x7D;

    // OFFSET 0x022467A0
    private static int BattleScript_TryGrudge(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Defender);
        int fainted = battleCtx.FaintedMon;
        if ((battleCtx.BattleMons[fainted].MoveEffectsMask & MoveEffectGrudge) != 0
            && BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker) != BattleSystems.BattleSystem_GetBattlerSide(battleSys, fainted)
            && battleCtx.BattleMons[battleCtx.Attacker].CurHP != 0
            && battleCtx.MoveTemp != MoveStruggle)
        {
            int slot = battleCtx.MoveSlot[battleCtx.Attacker];
            battleCtx.BattleMons[battleCtx.Attacker].PpCur[slot] = 0;
            battleCtx.MsgMoveTemp = battleCtx.BattleMons[battleCtx.Attacker].Moves[slot];
            BattleContexts.BattleMon_CopyToParty(battleSys, battleCtx, battleCtx.Attacker);
        }
        else
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x02246848
    private static int BattleScript_TrySnatch(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        for (int battler = 0; battler < BattleSystems.BattleSystem_GetMaxBattlers(battleSys); battler++)
        {
        }
        if (battleCtx.WaitingBattlers == 1)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else
        {
            battleCtx.TurnFlags[battleCtx.Attacker].Snatching = 1;
        }
        return 0;
    }

    // OFFSET 0x022468A4
    private static int BattleScript_CalcWeightBasedPower(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int weight = battleCtx.BattleMons[battleCtx.Defender].Weight;
        ushort[] table = RomData.OverlayU16(BattleOverlay, WeightPowerTableAddress, 12);
        int i = 0;
        while (table[i * 2] < weight)
        {
            i++;
            if (table[i * 2] == WeightTableEnd)
            {
                break;
            }
        }
        if (table[i * 2] != WeightTableEnd)
        {
            battleCtx.MovePower = table[i * 2 + 1];
        }
        else
        {
            battleCtx.MovePower = WeightPowerMax;
        }
        return 0;
    }

    // OFFSET 0x02246904
    private static int BattleScript_CalcWeatherBallParams(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        if (BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAllBattlers, 0, AbilityCloudNine) != 0 || BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAllBattlers, 0, AbilityAirLock) != 0)
        {
            return 0;
        }
        if ((battleCtx.FieldConditionsMask & FieldConditionWeatherAny) != 0)
        {
            battleCtx.MovePower = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Power * 2;
            if ((battleCtx.FieldConditionsMask & FieldConditionRain) != 0)
            {
                battleCtx.MoveType = TypeWater;
            }
            if ((battleCtx.FieldConditionsMask & FieldConditionSandstorm) != 0)
            {
                battleCtx.MoveType = TypeRock;
            }
            if ((battleCtx.FieldConditionsMask & FieldConditionSun) != 0)
            {
                battleCtx.MoveType = TypeFire;
            }
            if ((battleCtx.FieldConditionsMask & FieldConditionHail) != 0)
            {
                battleCtx.MoveType = TypeIce;
            }
        }
        else
        {
            battleCtx.MovePower = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Power;
        }
        return 0;
    }

    // OFFSET 0x022469C4
    private static int BattleScript_TryPursuit(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        ushort move = 0;
        int battler;
        for (battler = 0; battler < maxBattlers; battler++)
        {
            BattleMon mon = battleCtx.BattleMons[battler];
            if (battleCtx.BattlerActions[battler, 0] == ControllerCommandNone
                || mon.CurHP == 0
                || (mon.Status & StatusSleepOrFreeze) != 0
                || BattleContexts.Battler_CheckTruant(battleCtx, battler) != 0
                || BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) == BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.SwitchedMon))
            {
                continue;
            }
            move = mon.MoveEffectsData.EncoredMove;
            if (move == 0 || move != mon.Moves[mon.MoveEffectsData.EncoredMoveSlot])
            {
                move = BattleContexts.Battler_SelectedMove(battleCtx, battler);
            }
            if (move == 0)
            {
                continue;
            }
            int slot = BattleContexts.Battler_SlotForMove(mon, move);
            if (battleCtx.AiContext.MoveTable[move].Effect != EffectPursuit || mon.PpCur[slot] == 0)
            {
                continue;
            }
            mon.PpCur[slot]--;
            if (BattleContexts.Battler_Ability(battleCtx, battleCtx.SwitchedMon) == AbilityPressure && mon.PpCur[slot] != 0)
            {
                mon.PpCur[slot]--;
            }
            BattleContexts.BattleSystem_SetupLoop(battleSys, battleCtx);
            battleCtx.Attacker = battler;
            battleCtx.Defender = battleCtx.SwitchedMon;
            battleCtx.PowerMul = PursuitPowerMul;
            battleCtx.MoveCur = move;
            battleCtx.MovePrevByBattler[battler] = move;
            battleCtx.BattlerActions[battler, 0] = ControllerCommandNone;
            BattleContexts.BattleMon_CopyToParty(battleSys, battleCtx, battler);
            break;
        }
        if (battler == maxBattlers)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        int itemEffect = BattleContexts.Battler_HeldItemEffect(battleCtx, battleCtx.Attacker);
        BattleContexts.Battler_HeldItemPower(battleCtx, battleCtx.Attacker, 0);
        if (itemEffect == HoldEffectChoiceBand || itemEffect == HoldEffectChoiceScarf || itemEffect == HoldEffectChoiceSpecs)
        {
            battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.ChoiceLockedMove = move;
        }
        return 0;
    }
}
