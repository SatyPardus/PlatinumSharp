namespace PlatinumSharp.Game;

public static partial class Battle
{

    // OFFSET 0x0223BBD0
    public static int BattleSystem_Run(ApplicationManager appMan)
    {
        BattleSystem battleSys = (BattleSystem)ApplicationManager.ApplicationManager_Data(appMan)!;
        if ((battleSys.BattleType & BattleTypeLink) != 0 && (battleSys.BattleStatusMask & BattleStatusRecording) == 0)
        {
            if (battleSys.BattleInitialized != 0)
            {
                BattleControllers.BattleController_Main(battleSys, battleSys.BattleCtx!);
            }
            for (int i = 0; i < battleSys.MaxBattlers; i++)
            {
                Battlers.Battler_Update(battleSys, battleSys.Battlers[i]!);
            }
        }
        else
        {
            BattleSystem_RunLocalStep(battleSys);
            if (battleSys.CommandIsEndWait == 0)
            {
                BattleSystem_RunLocalStep(battleSys);
            }
        }
        return battleSys.CommandIsEndWait;
    }

    private static void BattleSystem_RunLocalStep(BattleSystem battleSys)
    {
        if (battleSys.BattleInitialized != 0)
        {
            battleSys.CommandIsEndWait = (byte)BattleControllers.BattleController_Main(battleSys, battleSys.BattleCtx!);
            BattleIO.BattleIO_UpdateQueue(battleSys, 1);
        }
        for (int i = 0; i < battleSys.MaxBattlers; i++)
        {
            Battlers.Battler_Update(battleSys, battleSys.Battlers[i]!);
            BattleIO.BattleIO_UpdateQueue(battleSys, 0);
        }
    }
}
