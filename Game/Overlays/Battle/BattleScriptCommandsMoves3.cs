namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const uint VolatileRecharge = 0x1000;
    private const int TypeMatchupRandom = 0xFFFF;
    private const int Conversion2Attempts = 1000;
    private const int MultiplierNotVeryEffective = 5;
    private const int MoveStruggle = 0xA5;
    private const int MoveSketch = 0xA6;
    private const int MoveChatter = 0x1C0;
    private const int MoveFocusPunch = 0x108;
    private const int MoveUproar = 0xFD;
    private const int AllMovesInvalid = 0xF;
    private const int CheckInvalidNoPP = ~2;
    private const uint FlailPowerTableAddress = 0x0226E584;
    private const int FlailTableCount = 6;
    private const int FlailBarSize = 0x40;

    private static bool BattleScript_TryConversion2Type(BattleContext battleCtx, int type, byte moveType, byte vsType, byte multiplier)
    {
        if (moveType != type || multiplier > MultiplierNotVeryEffective)
        {
            return false;
        }
        if (vsType == BattleContexts.BattleMon_Get(battleCtx, battleCtx.Attacker, BattleMonType1, null) || vsType == BattleContexts.BattleMon_Get(battleCtx, battleCtx.Attacker, BattleMonType2, null))
        {
            return false;
        }
        battleCtx.BattleMons[battleCtx.Attacker].Type1 = vsType;
        battleCtx.BattleMons[battleCtx.Attacker].Type2 = vsType;
        battleCtx.MsgTemp = vsType;
        return true;
    }

    // OFFSET 0x022442F0
    private static int BattleScript_TryConversion2(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        if (BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) == AbilityMultitype)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        ushort lastMove = battleCtx.Conversion2Move[battleCtx.Attacker];
        if (lastMove != 0 && battleCtx.Conversion2Battler[battleCtx.Attacker] != 0xFF)
        {
            if (BattleContexts.Move_IsMultiTurn(battleCtx, lastMove) != 0 && (battleCtx.BattleMons[battleCtx.Conversion2Battler[battleCtx.Attacker]].StatusVolatile & VolatileRecharge) != 0)
            {
                BattleScript_Iter(battleCtx, jumpOnFail);
                return 0;
            }
            int type = battleCtx.Conversion2Type[battleCtx.Attacker];
            byte moveType;
            byte vsType;
            byte multiplier;
            for (int i = 0; i < Conversion2Attempts; i++)
            {
                BattleContexts.BattleSystem_TypeMatchup(battleSys, TypeMatchupRandom, out moveType, out vsType, out multiplier);
                if (BattleScript_TryConversion2Type(battleCtx, type, moveType, vsType, multiplier))
                {
                    return 0;
                }
            }
            int index = 0;
            if (BattleContexts.BattleSystem_TypeMatchup(battleSys, index, out moveType, out vsType, out multiplier) == 1)
            {
                do
                {
                    if (BattleScript_TryConversion2Type(battleCtx, type, moveType, vsType, multiplier))
                    {
                        return 0;
                    }
                    index++;
                }
                while (BattleContexts.BattleSystem_TypeMatchup(battleSys, index, out moveType, out vsType, out multiplier) == 1);
            }
        }
        BattleScript_Iter(battleCtx, jumpOnFail);
        return 0;
    }

    // OFFSET 0x022444B0
    private static int BattleScript_TrySketch(BattleSystem battleSys, BattleContext battleCtx)
    {
        int sketchSlot = -1;
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        BattleMon attackerMon = battleCtx.BattleMons[battleCtx.Attacker];
        ushort move;
        if ((attackerMon.StatusVolatile & VolatileTransformed) != 0
            || (move = battleCtx.MoveSketched[battleCtx.Defender]) == MoveStruggle
            || move == MoveSketch
            || move == MoveChatter
            || move == 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        int i;
        for (i = 0; i < 4; i++)
        {
            if (attackerMon.Moves[i] != MoveSketch && move == attackerMon.Moves[i])
            {
                break;
            }
            if (attackerMon.Moves[i] == MoveSketch && sketchSlot == -1)
            {
                sketchSlot = i;
            }
        }
        if (i != 4)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        attackerMon.Moves[sketchSlot] = move;
        battleCtx.BattleMons[battleCtx.Attacker].PpCur[sketchSlot] = battleCtx.AiContext.MoveTable[battleCtx.MoveSketched[battleCtx.Defender]].Pp;
        BattleIO.BattleIO_UpdatePartyMon(battleSys, battleCtx, battleCtx.Attacker);
        battleCtx.MsgMoveTemp = battleCtx.MoveSketched[battleCtx.Defender];
        if (battleCtx.MsgMoveTemp == MoveLastResort)
        {
            battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.LastResortCount = 0;
        }
        return 0;
    }

    // OFFSET 0x022445D4
    private static int BattleScript_TrySleepTalk(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int invalidMoves = 0;
        for (int i = 0; i < 4; i++)
        {
            ushort move = battleCtx.BattleMons[battleCtx.Attacker].Moves[i];
            if (BattleContexts.Move_IsInvoker(move) != 0 || move == MoveFocusPunch || move == MoveUproar || move == MoveChatter || BattleContexts.Move_IsMultiTurn(battleCtx, move) != 0)
            {
                invalidMoves |= (int)HealthBar.FlagIndex(i);
            }
        }
        invalidMoves = BattleContexts.BattleSystem_CheckInvalidMoves(battleSys, battleCtx, battleCtx.Attacker, invalidMoves, CheckInvalidNoPP);
        if (invalidMoves == AllMovesInvalid)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        int slot;
        do
        {
            slot = BattleSystems.BattleSystem_RandNext(battleSys) % 4;
        }
        while ((HealthBar.FlagIndex(slot) & invalidMoves) != 0);
        battleCtx.MsgMoveTemp = battleCtx.BattleMons[battleCtx.Attacker].Moves[slot];
        return 0;
    }

    // OFFSET 0x022446A0
    private static int BattleScript_CalcFlailPower(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleMon attackerMon = battleCtx.BattleMons[battleCtx.Attacker];
        int fill = HealthBar.HealthBar_Fill((uint)attackerMon.CurHP, (uint)attackerMon.MaxHP, FlailBarSize);
        byte[] table = RomData.Overlay(BattleOverlay, FlailPowerTableAddress, FlailTableCount * 2);
        int i;
        for (i = 0; i < FlailTableCount; i++)
        {
            if (fill <= table[i * 2])
            {
                break;
            }
        }
        battleCtx.MovePower = table[i * 2 + 1];
        return 0;
    }
}
