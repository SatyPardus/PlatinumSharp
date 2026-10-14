namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int NarcSubroutineScripts = 1;
    private const int StatBoostCount = 8;
    private const sbyte StatBoostNeutral = 6;
    private const uint VolatileFocusEnergy = 0x100000;

    // OFFSET 0x02242F1C
    private static int BattleScript_GoTo(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int distance = BattleScript_Read(battleCtx);
        BattleScript_Iter(battleCtx, distance);
        return 0;
    }

    // OFFSET 0x02242F3C
    private static int BattleScript_CallSubroutine(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int file = BattleScript_Read(battleCtx);
        BattleScript_Call(battleCtx, NarcSubroutineScripts, file);
        return 0;
    }

    // OFFSET 0x02242F5C
    private static int BattleScript_CallSubroutineFromVar(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int var = BattleScript_Read(battleCtx);
        int file = BattleScript_VarAddress(battleSys, battleCtx, var);
        BattleScript_Call(battleCtx, NarcSubroutineScripts, file);
        return 0;
    }

    // OFFSET 0x022430A4
    private static int BattleScript_ResetAllStatChanges(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        for (int i = 0; i < maxBattlers; i++)
        {
            for (int stat = 0; stat < StatBoostCount; stat++)
            {
                battleCtx.BattleMons[i].StatBoosts[stat] = StatBoostNeutral;
            }
            battleCtx.BattleMons[i].StatusVolatile &= ~VolatileFocusEnergy;
        }
        return 0;
    }
}
