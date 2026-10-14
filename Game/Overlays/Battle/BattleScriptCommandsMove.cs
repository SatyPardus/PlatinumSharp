using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int ScriptNarcMove = 0;
    private const int ScriptNarcSubSeq = 1;
    private const int ScriptNarcEffect = 0x1E;
    private const uint BattleStatusSkipCritical = 0x400;
    private const uint BattleStatusFirstBattle = 1;
    private const uint BattleStatusMoveFailed = 1;
    private const uint BattleStatusMoveIgnore = 0x4000;
    private const int ContextParamSideConditions = 0;
    private const int ControllerCommandMoveEnd = 0x26;
    private const int SubSeqNoTarget = 0x119;
    private const uint BattleTypeNoExp = 0x2A4;
    private const int MonDataSpecies = 5;
    private const int MonDataCurHP = 0xA3;
    private const int MonDataHeldItem = 6;
    private const int ItemParamHoldEffect = 1;
    private const int HoldEffectExpShare = 0x33;
    private const int SpeciesDataBaseExp = 9;
    private const int GetExpTaskSize = 0x58;

    // OFFSET 0x02241B08
    private static int BattleScript_CompareMonDataToValue(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int op = BattleScript_Read(battleCtx);
        int selector = BattleScript_Read(battleCtx);
        int param = BattleScript_Read(battleCtx);
        int value = BattleScript_Read(battleCtx);
        int jump = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        int data = BattleContexts.BattleMon_Get(battleCtx, battler, param, null);
        if (!BattleScript_Compare(op, data, value))
        {
            jump = 0;
        }
        if (jump != 0)
        {
            BattleScript_Iter(battleCtx, jump);
        }
        return 0;
    }

    // OFFSET 0x02241BC0
    private static int BattleScript_FadeOutBattle(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleIO.BattleIO_FadeOutBattle(battleSys, battleCtx);
        return 0;
    }

    // OFFSET 0x02241BDC
    private static int BattleScript_GoToSubscript(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int file = BattleScript_Read(battleCtx);
        BattleScript_Jump(battleCtx, ScriptNarcSubSeq, file);
        return 0;
    }

    // OFFSET 0x02241BFC
    private static int BattleScript_GoToEffectScript(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleScript_Jump(battleCtx, ScriptNarcEffect, battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Effect);
        return 0;
    }

    // OFFSET 0x02241C28
    private static int BattleScript_GoToMoveScript(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int inspectTarget = BattleScript_Read(battleCtx);
        battleCtx.BattleStatusMask &= ~BattleStatusMoveFailed;
        battleCtx.BattleStatusMask &= ~BattleStatusMoveIgnore;
        battleCtx.MoveCur = (uint)battleCtx.MsgMoveTemp;
        if (inspectTarget == 0)
        {
            battleCtx.Defender = BattleContexts.BattleSystem_Defender(battleSys, battleCtx, battleCtx.Attacker, (ushort)battleCtx.MsgMoveTemp, 1, 0);
            BattleContexts.BattleSystem_RedirectTarget(battleSys, battleCtx, battleCtx.Attacker, (ushort)battleCtx.MsgMoveTemp);
            battleCtx.BattlerActions[battleCtx.Attacker, 1] = (uint)battleCtx.Defender;
        }
        if (battleCtx.Defender == 0xFF)
        {
            battleCtx.CommandNext = ControllerCommandMoveEnd;
            BattleScript_Jump(battleCtx, ScriptNarcSubSeq, SubSeqNoTarget);
        }
        else
        {
            BattleScript_Jump(battleCtx, ScriptNarcMove, (int)battleCtx.MoveCur);
        }
        return 0;
    }

    // OFFSET 0x02241CD0
    private static int BattleScript_CalcCrit(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeCatchTutorial) != 0
            || (BattleSystems.BattleSystem_GetBattleStatusMask(battleSys) & BattleStatusFirstBattle) != 0)
        {
            battleCtx.CriticalMul = 1;
        }
        else
        {
            uint sideConditions = (uint)BattleContexts.BattleContext_Get(battleSys, battleCtx, ContextParamSideConditions, battleCtx.Defender);
            battleCtx.CriticalMul = BattleContexts.BattleSystem_CalcCriticalMulti(battleSys, battleCtx, battleCtx.Attacker, battleCtx.Defender, battleCtx.CriticalBoosts, sideConditions);
        }
        return 0;
    }

    // OFFSET 0x02241D34
    private static int BattleScript_CalcExpGain(BattleSystem battleSys, BattleContext battleCtx)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        BattlerData battlerData = BattleSystems.BattleSystem_GetBattlerData(battleSys, battleCtx.FaintedMon)!;
        BattleScript_Iter(battleCtx, 1);
        int jump = BattleScript_Read(battleCtx);
        if ((battlerData.BattlerType & 1) == 0 || (battleType & BattleTypeNoExp) != 0)
        {
            BattleScript_Iter(battleCtx, jump);
            return 0;
        }
        int participants = 0;
        int expShare = 0;
        for (int i = 0; i < Party.Party_GetCurrentCount(BattleSystems.BattleSystem_GetParty(battleSys, 0)); i++)
        {
            Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, 0, i);
            if (Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null) != 0 && Pokemon.Pokemon_GetValue(mon, MonDataCurHP, null) != 0)
            {
                if ((HealthBar.FlagIndex(i) & battleCtx.SideGetExpMask[(battleCtx.FaintedMon >> 1) & 1]) != 0)
                {
                    participants++;
                }
                if (BattleContexts.BattleSystem_GetItemData(battleCtx, (ushort)Pokemon.Pokemon_GetValue(mon, MonDataHeldItem, null), ItemParamHoldEffect) == HoldEffectExpShare)
                {
                    expShare++;
                }
            }
        }
        BattleMon fainted = battleCtx.BattleMons[battleCtx.FaintedMon];
        ushort baseExp = (ushort)((ushort)SpeciesData.SpeciesData_GetSpeciesValue(fainted.Species, SpeciesDataBaseExp) * fainted.Level / 7);
        if (expShare != 0)
        {
            int half = baseExp >> 1;
            battleCtx.GainedExp = half / participants;
            if (battleCtx.GainedExp == 0)
            {
                battleCtx.GainedExp = 1;
            }
            battleCtx.SharedExp = half / expShare;
            if (battleCtx.SharedExp == 0)
            {
                battleCtx.SharedExp = 1;
            }
        }
        else
        {
            battleCtx.GainedExp = baseExp / participants;
            if (battleCtx.GainedExp == 0)
            {
                battleCtx.GainedExp = 1;
            }
            battleCtx.SharedExp = 0;
        }
        return 0;
    }

    // OFFSET 0x02241EB0
    private static int BattleScript_StartGetExpTask(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        GetExpTask task = new();
        battleCtx.TaskData = task;
        task.BattleSys = battleSys;
        task.BattleCtx = battleCtx;
        task.State = 0;
        task.ExpPartySlot = 0;
        SysTask.SysTask_Start(BattleScript_GetExpTask, task, 0);
        return 0;
    }

    // OFFSET 0x02241EF0
    private static int BattleScript_WaitGetExpTask(BattleSystem battleSys, BattleContext battleCtx)
    {
        if (battleCtx.TaskData == null)
        {
            BattleScript_Iter(battleCtx, 1);
        }
        battleCtx.BattleProgressFlag = 1;
        return 0;
    }

    // OFFSET 0x02241F1C
    private static int BattleScript_Dummy2A(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleScript_Read(battleCtx);
        return 0;
    }

}

public sealed class GetExpTask
{
    public BattleSystem? BattleSys;
    public BattleContext? BattleCtx;
    public ManagedSprite? LevelUpFrame;
    public ManagedSprite? MonIcon;
    public FontOAM? LevelText;
    public readonly CharTransferAllocation LevelTextChars = new();
    public int State;
    public int PrinterID;
    public int Timer;
    public int LearnsetIndex;
    public int ExpDelta;
    public int Move;
    public int ForgetSlot;
    public int ExpPartySlot;
    public FontOAMManager? FontOAMMan;
}

public static partial class BattleIO
{
    private const int CommandFadeOutBattle = 0x1C;

    // OFFSET 0x02265ECC
    public static void BattleIO_FadeOutBattle(BattleSystem battleSys, BattleContext battleCtx)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandFadeOutBattle);
        BattleIO_Send(battleSys, RecipientBattler, 0, message, 4);
    }
}
