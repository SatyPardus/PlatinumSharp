using System.Runtime.CompilerServices;

namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    public const int VarCount = 70;

    private static int sNullVar;

    private static ref int AsInt(ref uint value)
    {
        return ref Unsafe.As<uint, int>(ref value);
    }

    // OFFSET 0x02248AD0
    public static int BattleScript_Read(BattleContext battleCtx)
    {
        int value = battleCtx.BattleScript[battleCtx.ScriptCursor];
        battleCtx.ScriptCursor++;
        return value;
    }

    // OFFSET 0x02248AF0
    public static void BattleScript_Iter(BattleContext battleCtx, int distance)
    {
        battleCtx.ScriptCursor += distance;
    }

    // OFFSET 0x02248B00
    public static void BattleScript_Jump(BattleContext battleCtx, int narcID, int file)
    {
        BattleContexts.BattleSystem_LoadScript(battleCtx, narcID, file);
    }

    // OFFSET 0x02248B08
    public static void BattleScript_Call(BattleContext battleCtx, int narcID, int file)
    {
        BattleContexts.BattleSystem_CallScript(battleCtx, narcID, file);
    }

    // OFFSET 0x02248B10
    public static ref int BattleScript_VarAddress(BattleSystem battleSys, BattleContext battleCtx, int var)
    {
        switch (var)
        {
            case 0:
                return ref AsInt(ref battleSys.BattleType);
            case 1:
                return ref battleCtx.CriticalBoosts;
            case 2:
                return ref AsInt(ref battleCtx.SideEffectDirectFlags);
            case 3:
                return ref AsInt(ref battleCtx.SideEffectIndirectFlags);
            case 4:
                return ref AsInt(ref battleCtx.SideEffectAbilityFlags);
            case 5:
                return ref battleCtx.SideEffectType;
            case 6:
                return ref AsInt(ref battleCtx.BattleStatusMask);
            case 7:
                return ref AsInt(ref battleCtx.FieldConditionsMask);
            case 8:
                return ref battleCtx.PowerMul;
            case 9:
                return ref battleCtx.CalcTemp;
            case 10:
                return ref AsInt(ref battleCtx.MoveStatusFlags);
            case 11:
                return ref AsInt(ref battleCtx.SideConditionsMask[BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker)]);
            case 12:
                return ref AsInt(ref battleCtx.SideConditionsMask[BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Defender)]);
            case 13:
                return ref AsInt(ref battleCtx.SideConditionsMask[BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.SideEffectMon)]);
            case 14:
                return ref battleCtx.Damage;
            case 15:
                return ref battleCtx.Attacker;
            case 16:
                return ref battleCtx.Defender;
            case 17:
                return ref battleCtx.SideEffectMon;
            case 18:
                return ref battleCtx.FaintedMon;
            case 19:
                return ref battleCtx.SwitchedMon;
            case 20:
                return ref battleCtx.MsgBattlerTemp;
            case 21:
                return ref battleCtx.StoredDamage[battleCtx.Attacker];
            case 22:
                return ref battleCtx.MsgTemp;
            case 23:
                return ref AsInt(ref battleCtx.PayDayCount);
            case 24:
                return ref AsInt(ref battleCtx.MoveCur);
            case 25:
                return ref battleCtx.TotalTurns;
            case 26:
                return ref battleCtx.MsgAttacker;
            case 27:
                return ref battleCtx.MsgDefender;
            case 28:
                return ref AsInt(ref battleCtx.MoveTemp);
            case 29:
                return ref battleCtx.LastBattlerId;
            case 30:
                return ref battleCtx.MovePower;
            case 31:
                return ref battleCtx.AfterMoveMessageType;
            case 32:
                return ref battleCtx.HpCalcTemp;
            case 33:
                return ref AsInt(ref battleSys.ResultMaskWord);
            case 34:
                return ref battleCtx.SideEffectParam;
            case 35:
                return ref battleCtx.MsgMoveTemp;
            case 36:
                return ref battleCtx.MsgItemTemp;
            case 37:
                return ref battleCtx.MsgAbilityTemp;
            case 38:
                return ref AsInt(ref battleCtx.FieldConditions.WeatherTurns);
            case 39:
                return ref battleCtx.BattlerSpeedTemp;
            case 40:
                return ref battleCtx.MultiHitLoop;
            case 41:
                return ref battleCtx.TurnFlags[battleCtx.Attacker].PhysicalDamageAttackerMask;
            case 42:
                return ref battleCtx.TurnFlags[battleCtx.Attacker].SpecialDamageAttackerMask;
            case 43:
                return ref battleCtx.ScriptTemp;
            case 44:
                return ref battleCtx.CriticalMul;
            case 45:
                return ref battleCtx.TurnFlags[battleCtx.Attacker].LastDamageTaken;
            case 46:
                return ref battleCtx.TurnFlags[battleCtx.Defender].LastDamageTaken;
            case 47:
                return ref battleCtx.SelfTurnFlags[battleCtx.Attacker].StatusFlags;
            case 48:
                return ref battleCtx.SelfTurnFlags[battleCtx.Defender].StatusFlags;
            case 49:
                return ref battleCtx.SelfTurnFlags[battleCtx.SideEffectMon].StatusFlags;
            case 50:
                return ref battleCtx.FlingTemp;
            case 51:
                return ref battleCtx.FlingScript;
            case 52:
                return ref AsInt(ref battleSys.BattleStatusMask);
            case 53:
                return ref AsInt(ref battleCtx.MoveLockedInto[battleCtx.Attacker]);
            case 54:
                return ref battleCtx.HitDamage;
            case 55:
                return ref battleSys.SafariBalls;
            case 56:
                return ref battleCtx.SwitchedMonTemp;
            case 57:
                return ref battleCtx.MoveType;
            case 58:
                return ref battleCtx.MoveEffectChance;
            case 59:
                return ref AsInt(ref battleSys.RulesetMask);
            case 60:
                return ref AsInt(ref battleCtx.BattleStatusMask2);
            case 61:
                return ref battleCtx.TurnOrderCounter;
            case 62:
                return ref battleSys.MaxBattlers;
            case 63:
                return ref battleCtx.AttackerTemp;
            case 64:
                return ref battleCtx.DefenderTemp;
            case 65:
                return ref battleCtx.SelfTurnFlags[battleCtx.Defender].PhysicalDamageTaken;
            case 66:
                return ref battleCtx.TurnFlags[battleCtx.MsgBattlerTemp].AssuranceDamageMask;
            case 67:
                return ref battleCtx.TurnFlags[battleCtx.Defender].AssuranceDamageMask;
            case 68:
                return ref battleCtx.SelfTurnFlags[battleCtx.Attacker].ShellBellDamageDealt;
            case 69:
                return ref battleCtx.WaitingBattlers;
        }
        sNullVar = 0;
        return ref sNullVar;
    }
}
