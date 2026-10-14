using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class BattleControllers
{
    public const int CommandFightEnd = 0x29;
    public const int CommandEndWait = 0x2C;

    private const int ResultMaskNoEnd = 0x40;

    private static readonly Action<BattleSystem, BattleContext>[] CommandFuncs =
    {
        BattleController_GetBattleMon,
        BattleController_StartEncounter,
        BattleController_TrainerMessage,
        BattleController_ShowBattleMon,
        BattleController_InitCommandSelection,
        BattleController_CommandSelectionInput,
        BattleController_DetermineActionOrder,
        BattleController_TurnStart,
        BattleController_SortSpeed,
        BattleController_CheckFieldConditions,
        BattleController_CheckMonConditions,
        BattleController_CheckSideConditions,
        BattleController_TurnEnd,
        BattleController_PrepareFightAction,
        BattleController_UseItem,
        BattleController_Switch,
        BattleController_Flee,
        BattleController_SafariBall,
        BattleController_SafariBait,
        BattleController_SafariRock,
        BattleController_SafariWatch,
        BattleController_RunScript,
        BattleController_BeforeMove,
        BattleController_TryMove,
        BattleController_CheckDirectSideEffect,
        BattleController_CheckMoveResult,
        BattleController_MoveHit,
        BattleController_UpdateHP,
        BattleController_AfterMoveMessage,
        BattleController_LeftoverState29,
        BattleController_AfterMoveEffects,
        BattleController_LoopMultiHit,
        BattleController_LeftoverState32,
        BattleController_LoopWhileFainted,
        BattleController_LoopSpreadMoves,
        BattleController_FaintAfterSelfdestruct,
        BattleController_TriggerAfterHitEffects,
        BattleController_LeftoverState37,
        BattleController_UpdateMoveBuffers,
        BattleController_MoveEnd,
        BattleController_CheckAnyFainted,
        BattleController_HandleResult,
        BattleController_ScreenWipe,
        BattleController_EndFight,
        BattleController_EndWait,
    };

    // OFFSET 0x0224B990
    public static int BattleController_Main(BattleSystem battleSys, BattleContext battleCtx)
    {
        if (battleCtx.BattleEndFlag == 0 && Battle.BattleSystem_GetResultMask(battleSys) != 0 && (Battle.BattleSystem_GetResultMask(battleSys) & ResultMaskNoEnd) == 0)
        {
            battleCtx.Command = CommandFightEnd;
        }
        CommandFuncs[battleCtx.Command](battleSys, battleCtx);
        if (battleCtx.Command == CommandEndWait)
        {
            return 1;
        }
        return 0;
    }
}
