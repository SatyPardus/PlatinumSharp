using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class FieldSystemChildProcesses
{
    public const uint BattleTemplate = 0x020EA358;
    public const uint JournalDisplayTemplate = 0x020EA2E8;

    // OFFSET 0x0203D1B8
    public static int FieldBattle_Init(ApplicationManager appMan, ref int state)
    {
        return 1;
    }

    // OFFSET 0x0203D1BC
    public static int FieldBattle_Main(ApplicationManager appMan, ref int state)
    {
        if (Battle.Battle_Main(appMan, ref state) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0203D1D0
    public static int FieldBattle_Exit(ApplicationManager appMan, ref int state)
    {
        return 1;
    }

    // OFFSET 0x0203D1D4
    public static void FieldSystem_StartBattleProcess(FieldSystem fieldSystem, object? battleParams)
    {
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(BattleTemplate), battleParams);
    }

    // OFFSET 0x0203D30C
    public static void FieldSystem_OpenJournalDisplay(FieldSystem fieldSystem, object? unused)
    {
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(JournalDisplayTemplate), fieldSystem.SaveData);
    }
}
