namespace PlatinumSharp.Game;

public static partial class BattleDisplay
{
    private const uint SafariFleeTable = 0x0226F194;
    private const int SafariFleeStages = 13;
    private const int SpeciesDataFleeRate = 0x1A;
    private const int BattleMonSpecies = 0;
    private const int CommandSafariWatch = 5;

    // OFFSET 0x02260AB4
    private static void BattleDisplay_AICommandSelectionTask(SysTask sysTask, object? param)
    {
        CommandSelectionTaskData task = (CommandSelectionTaskData)param!;
        int command = TrainerAI.TrainerAI_PickCommand(task.BattleSys!, task.Battler);
        BattleIO.BattleIO_CommandSelectionResult(task.BattleSys!, task.Battler, (uint)command);
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x02260AE4
    private static void BattleDisplay_IgnoreCommandTask(SysTask sysTask, object? param)
    {
        CommandSelectionTaskData task = (CommandSelectionTaskData)param!;
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x02260B04
    private static void BattleDisplay_SafariEnemyCommandTask(SysTask sysTask, object? param)
    {
        CommandSelectionTaskData task = (CommandSelectionTaskData)param!;
        int stage = BattleSystems.BattleSystem_GetSafariEscapeCount(task.BattleSys!);
        int species = (ushort)BattleContexts.BattleMon_Get(BattleSystems.BattleSystem_GetBattleContext(task.BattleSys!)!, task.Battler, BattleMonSpecies, null);
        byte[] table = RomData.Overlay(16, SafariFleeTable, SafariFleeStages * 2);
        int fleeRate = (int)SpeciesData.SpeciesData_GetSpeciesValue(species, SpeciesDataFleeRate) * table[stage * 2] / table[stage * 2 + 1];
        if ((int)(BattleSystems.BattleSystem_RandNext(task.BattleSys!) % 255) > fleeRate)
        {
            BattleIO.BattleIO_CommandSelectionResult(task.BattleSys!, task.Battler, CommandSafariWatch);
        }
        else
        {
            BattleIO.BattleIO_CommandSelectionResult(task.BattleSys!, task.Battler, CommandRun);
        }
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x02260B84
    private static void BattleDisplay_FrontierEnemyCommandTask(SysTask sysTask, object? param)
    {
        CommandSelectionTaskData task = (CommandSelectionTaskData)param!;
        BattleIO.BattleIO_CommandSelectionResult(task.BattleSys!, task.Battler, CommandRun);
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
        SysTask.SysTask_Done(sysTask);
    }
}

public static partial class BattleSystems
{
    private const int ContextParamSafariEscapeCount = 5;

    // OFFSET 0x0223ED6C
    public static byte BattleSystem_GetSafariEscapeCount(BattleSystem battleSys)
    {
        if (battleSys.BattleCtx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return (byte)BattleContexts.BattleContext_Get(battleSys, battleSys.BattleCtx!, ContextParamSafariEscapeCount, 0);
    }
}
