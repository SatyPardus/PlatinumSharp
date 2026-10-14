using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class BattleControllers
{
    public const int CommandGetBattleMon = 0;
    public const int CommandStartEncounter = 1;
    public const int CommandTrainerMessage = 2;
    public const int CommandShowBattleMon = 3;
    public const int CommandInitCommandSelection = 4;
    public const int CommandCommandSelectionInput = 5;
    public const int CommandRunScript = 0x15;

    private const int SubSeqStartEncounter = 0;

    // OFFSET 0x0224BA1C
    private static void BattleController_GetBattleMon(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        for (int i = 0; i < maxBattlers; i++)
        {
            BattleContexts.BattleSystem_InitBattleMon(battleSys, battleCtx, i, battleCtx.SelectedPartySlot[i]);
        }
        battleCtx.HpTemp = (short)battleCtx.BattleMons[1].CurHP;
        battleCtx.Command = CommandStartEncounter;
    }

    // OFFSET 0x0224F5CC
    private static void BattleController_RunScript(BattleSystem battleSys, BattleContext battleCtx)
    {
        if (BattleScripts.BattleScript_Exec(battleSys, battleCtx) == 1)
        {
            battleCtx.ScriptCursor = 0;
            battleCtx.Command = battleCtx.CommandNext;
        }
    }

    // OFFSET 0x0224BA60
    private static void BattleController_StartEncounter(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleContexts.BattleSystem_LoadScript(battleCtx, BattleContexts.NarcBattleSubSeq, SubSeqStartEncounter);
        battleCtx.Command = CommandRunScript;
        battleCtx.CommandNext = CommandTrainerMessage;
    }
}
