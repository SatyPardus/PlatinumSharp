namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const uint SideConditionSpikes = 4;
    private const int SpikesMaxLayers = 3;
    private const uint MoveEffectPerishSong = 0x20;
    private const int PerishSongTurns = 3;

    // OFFSET 0x02244F54
    private static int BattleScript_TrySpikes(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker) ^ 1;
        if (battleCtx.SideConditions[side].SpikesLayers == SpikesMaxLayers)
        {
            battleCtx.SelfTurnFlags[battleCtx.Attacker].SkipPressureCheck = 1;
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else
        {
            battleCtx.SideConditionsMask[side] |= SideConditionSpikes;
            battleCtx.SideConditions[side].SpikesLayers = (battleCtx.SideConditions[side].SpikesLayers + 1) & 3;
        }
        return 0;
    }

    // OFFSET 0x02244FD8
    private static int BattleScript_CheckSpikes(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int jumpIfNone = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler);
        int layers = battleCtx.SideConditions[side].SpikesLayers;
        if (layers != 0 && battleCtx.BattleMons[battler].CurHP != 0)
        {
            battleCtx.HpCalcTemp = (5 - layers) * 2;
            battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)battleCtx.BattleMons[battler].MaxHP * -1, battleCtx.HpCalcTemp);
        }
        else
        {
            BattleScript_Iter(battleCtx, jumpIfNone);
        }
        return 0;
    }

    // OFFSET 0x0224505C
    private static int BattleScript_TryPerishSong(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        int count = 0;
        battleCtx.CalcTemp = maxBattlers;
        for (int battler = 0; battler < maxBattlers; battler++)
        {
            BattleMon mon = battleCtx.BattleMons[battler];
            if ((mon.MoveEffectsMask & MoveEffectPerishSong) != 0 || mon.CurHP == 0 || BattleContexts.Battler_IgnorableAbility(battleCtx, battleCtx.Attacker, battler, AbilitySoundproof) == 1)
            {
                count++;
            }
            else
            {
                mon.MoveEffectsMask |= MoveEffectPerishSong;
                mon.MoveEffectsData.PerishSongTurns = PerishSongTurns;
            }
        }
        if (count == maxBattlers)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x0224510C
    private static int BattleScript_GetMonBySpeedOrder(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int var = BattleScript_Read(battleCtx);
        ref int data = ref BattleScript_VarAddress(battleSys, battleCtx, var);
        data = battleCtx.MonSpeedOrder[battleCtx.BattlerSpeedTemp];
        return 0;
    }

    // OFFSET 0x02245144
    private static int BattleScript_GoToIfValidMon(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int var = BattleScript_Read(battleCtx);
        int jump = BattleScript_Read(battleCtx);
        ref int data = ref BattleScript_VarAddress(battleSys, battleCtx, var);
        if ((uint)data < (uint)BattleSystems.BattleSystem_GetMaxBattlers(battleSys))
        {
            BattleScript_Iter(battleCtx, jump);
        }
        return 0;
    }
}
