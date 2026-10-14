namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const uint BattleStatusIgnoreType = 0x400;
    private const int AbilitySturdy = 5;
    private const int AbilityNoGuard = 0x63;
    private const uint MoveStatusSturdy = 0x80000;
    private const uint MoveEffectSemiInvulnerable = 0x18;
    private const uint MoveStatusOneHitKO = 0x20;
    private const uint MoveStatusMissed = 1;
    private const uint MoveStatusLevelTooLow = 0x1000;
    private const uint MoveStatusIgnoredSemiInvulnerable = 0x400;
    private const uint VolatileTransformed = 0x200000;
    private const uint VolatileSubstitute = 0x1000000;
    private const int MoveMimic = 0x66;
    private const int MoveLastResort = 0x183;
    private const int MimicPPMax = 5;
    private const int NumMoves = 0x1D3;

    // OFFSET 0x02243B38
    private static int BattleScript_TryOHKOMove(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        battleCtx.BattleStatusMask |= BattleStatusIgnoreType;
        if (BattleContexts.Battler_IgnorableAbility(battleCtx, battleCtx.Attacker, battleCtx.Defender, AbilitySturdy) == 1)
        {
            battleCtx.MoveStatusFlags |= MoveStatusSturdy;
            return 0;
        }
        BattleMon attackerMon = battleCtx.BattleMons[battleCtx.Attacker];
        BattleMon defenderMon = battleCtx.BattleMons[battleCtx.Defender];
        bool hit;
        if ((defenderMon.MoveEffectsMask & MoveEffectSemiInvulnerable) == 0 && BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) != AbilityNoGuard && BattleContexts.Battler_Ability(battleCtx, battleCtx.Defender) != AbilityNoGuard)
        {
            ushort accuracy = (ushort)(battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Accuracy + battleCtx.BattleMons[battleCtx.Attacker].Level - battleCtx.BattleMons[battleCtx.Defender].Level);
            hit = BattleSystems.BattleSystem_RandNext(battleSys) % 100 < accuracy && battleCtx.BattleMons[battleCtx.Attacker].Level >= battleCtx.BattleMons[battleCtx.Defender].Level;
        }
        else
        {
            defenderMon = battleCtx.BattleMons[battleCtx.Defender];
            if ((defenderMon.MoveEffectsData.LockOnTarget == battleCtx.Attacker && (defenderMon.MoveEffectsMask & MoveEffectSemiInvulnerable) != 0) || BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) == AbilityNoGuard || BattleContexts.Battler_Ability(battleCtx, battleCtx.Defender) == AbilityNoGuard)
            {
                hit = battleCtx.BattleMons[battleCtx.Attacker].Level >= battleCtx.BattleMons[battleCtx.Defender].Level;
            }
            else
            {
                ushort accuracy = (ushort)(battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Accuracy + battleCtx.BattleMons[battleCtx.Attacker].Level - battleCtx.BattleMons[battleCtx.Defender].Level);
                hit = BattleSystems.BattleSystem_RandNext(battleSys) % 100 < accuracy && battleCtx.BattleMons[battleCtx.Attacker].Level >= battleCtx.BattleMons[battleCtx.Defender].Level;
            }
            battleCtx.MoveStatusFlags |= MoveStatusIgnoredSemiInvulnerable;
        }
        if (hit)
        {
            battleCtx.Damage = -battleCtx.BattleMons[battleCtx.Defender].CurHP;
            battleCtx.MoveStatusFlags |= MoveStatusOneHitKO;
        }
        else if (battleCtx.BattleMons[battleCtx.Attacker].Level >= battleCtx.BattleMons[battleCtx.Defender].Level)
        {
            battleCtx.MoveStatusFlags |= MoveStatusMissed;
        }
        else
        {
            battleCtx.MoveStatusFlags |= MoveStatusLevelTooLow;
        }
        return 0;
    }

    // OFFSET 0x02243D2C
    private static int BattleScript_DivideVarByValue(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int var = BattleScript_Read(battleCtx);
        int value = BattleScript_Read(battleCtx);
        ref int data = ref BattleScript_VarAddress(battleSys, battleCtx, var);
        data = BattleContexts.BattleSystem_Divide(data, value);
        return 0;
    }

    // OFFSET 0x02243D64
    private static int BattleScript_DivideVarByVar(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int var1 = BattleScript_Read(battleCtx);
        int var2 = BattleScript_Read(battleCtx);
        ref int data = ref BattleScript_VarAddress(battleSys, battleCtx, var1);
        int divisor = BattleScript_VarAddress(battleSys, battleCtx, var2);
        int sign = data < 0 ? -1 : 1;
        data = data / divisor;
        if (data == 0)
        {
            data = sign;
        }
        return 0;
    }

    // OFFSET 0x02243DBC
    private static int BattleScript_TryMimic(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        BattleMon attackerMon = battleCtx.BattleMons[battleCtx.Attacker];
        ushort move;
        if (BattleContexts.Move_CanBeMimicked(battleCtx.MovePrevByBattler[battleCtx.Defender]) == 0
            || (attackerMon.StatusVolatile & VolatileTransformed) != 0
            || (battleCtx.BattleMons[battleCtx.Defender].StatusVolatile & VolatileSubstitute) != 0
            || (move = battleCtx.MovePrevByBattler[battleCtx.Defender]) == 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        int mimicSlot = -1;
        int i;
        for (i = 0; i < 4; i++)
        {
            if (attackerMon.Moves[i] == move)
            {
                break;
            }
            if (attackerMon.Moves[i] == MoveMimic && mimicSlot == -1)
            {
                mimicSlot = i;
            }
        }
        if (i != 4)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        battleCtx.MsgMoveTemp = move;
        battleCtx.BattleMons[battleCtx.Attacker].Moves[mimicSlot] = (ushort)battleCtx.MsgMoveTemp;
        int pp = battleCtx.AiContext.MoveTable[battleCtx.MsgMoveTemp].Pp;
        if (pp >= MimicPPMax)
        {
            pp = MimicPPMax;
        }
        battleCtx.BattleMons[battleCtx.Attacker].PpCur[mimicSlot] = (byte)pp;
        MoveEffectsData effects = battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData;
        effects.MimickedMoveSlot = (int)((HealthBar.FlagIndex(mimicSlot) | (uint)effects.MimickedMoveSlot) & 0xF);
        if (battleCtx.MsgMoveTemp == MoveLastResort)
        {
            battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.LastResortCount = 0;
        }
        return 0;
    }

    // OFFSET 0x02243EF4
    private static int BattleScript_Metronome(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        ushort move;
        while (true)
        {
            move = (ushort)(BattleSystems.BattleSystem_RandNext(battleSys) % NumMoves + 1);
            int i;
            for (i = 0; i < 4; i++)
            {
                if (move == battleCtx.BattleMons[battleCtx.Attacker].Moves[i])
                {
                    break;
                }
            }
            if (i != 4)
            {
                continue;
            }
            if (BattleContexts.Move_CanBeMetronomed(battleSys, battleCtx, battleCtx.Attacker, move) != 0)
            {
                break;
            }
        }
        battleCtx.MsgMoveTemp = move;
        return 0;
    }
}
