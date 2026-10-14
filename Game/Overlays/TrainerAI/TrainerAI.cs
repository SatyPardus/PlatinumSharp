using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class TrainerAI
{
    private const int Overlay = 14;
    private const uint ScriptEnd = 0x0222EE78;
    private const uint IgnoredEffectsTable = 0x0222EE90;
    private const uint AlwaysDamagingEffectsTable = 0x0222EE78;
    private const ushort TableEnd = 0xFFFF;
    private const int MaxMoves = 4;
    private const int MaxBattlers = 4;
    private const sbyte InitialScore = 100;
    private const int ScriptStackMax = 8;
    private const uint ThinkingMaskRoaming = 0x20000000;
    private const uint ThinkingMaskDoubles = 0x80;
    private const uint BattleTypeRoaming = 0x100;
    private const byte StateFlagMoveDone = 0x01;
    private const byte StateFlagFlee = 0x02;
    private const byte StateFlagSafari = 0x04;
    private const byte StateFlagEnd = 0x08;
    private const byte StateFlagPartnerSet = 0x10;
    private const int ActionFlee = 4;
    private const int ActionSafari = 5;
    private const int EvalStepInit = 0;
    private const int EvalStepRun = 1;
    private const int EvalStepDone = 2;
    private const int AIBattlerDefender = 0;
    private const int AIBattlerAttacker = 1;
    private const int AIBattlerDefenderPartner = 2;
    private const int AIBattlerAttackerPartner = 3;
    private const ushort RangeUserOrAlly = 0x200;
    private const ushort MoveCurse = 0xAE;
    private const int DamageRollRange = 16;
    private const int NoTarget = -1;

    private static ushort[]? ignoredEffects;
    private static ushort[]? alwaysDamagingEffects;

    private static uint[] Script(BattleContext battleCtx)
    {
        battleCtx.AiScriptTemp ??= RomData.OverlayU32(Overlay, battleCtx.AiScriptTempAddress, (int)(ScriptEnd - battleCtx.AiScriptTempAddress) / 4);
        return battleCtx.AiScriptTemp;
    }

    private static ushort[] EffectTable(uint address)
    {
        List<ushort> entries = new();
        for (uint i = 0; ; i++)
        {
            ushort entry = RomData.OverlayU16(Overlay, address + i * 2, 1)[0];
            entries.Add(entry);
            if (entry == TableEnd)
            {
                break;
            }
        }
        return entries.ToArray();
    }

    private static int EffectTableIndex(ushort[] table, int effect)
    {
        int i = 0;
        while (table[i] != effect)
        {
            i++;
            if (table[i] == TableEnd)
            {
                break;
            }
        }
        return i;
    }

    private static bool AI_MoveDealsDamage(AIContext ai, int move)
    {
        ignoredEffects ??= EffectTable(IgnoredEffectsTable);
        alwaysDamagingEffects ??= EffectTable(AlwaysDamagingEffectsTable);
        int effect = ai.MoveTable[move].Effect;
        int ignored = EffectTableIndex(ignoredEffects, effect);
        int always = EffectTableIndex(alwaysDamagingEffects, effect);
        if (alwaysDamagingEffects[always] != TableEnd)
        {
            return true;
        }
        return move != 0 && ignoredEffects[ignored] == TableEnd && ai.MoveTable[move].Power > 1;
    }

    // OFFSET 0x0221FC20
    public static void TrainerAI_Init(BattleSystem battleSys, BattleContext battleCtx, int battler, int initScore)
    {
        AIContext ai = battleCtx.AiContext;
        ai.EvalStep = 0;
        ai.MoveSlot = 0;
        ai.Move = 0;
        Array.Clear(ai.MoveScore);
        ai.CalcTemp = 0;
        ai.ThinkingMask = 0;
        ai.StateFlags = 0;
        ai.ThinkingBitShift = 0;
        ai.Padding0012 = 0;
        ai.Padding0013 = 0;
        ai.Padding0014 = 0;
        Array.Clear(ai.MoveDamageRolls);
        for (int i = 0; i < MaxMoves; i++)
        {
            ai.MoveScore[i] = (initScore & 1) != 0 ? InitialScore : (sbyte)0;
            initScore = (byte)(initScore >> 1);
        }
        int invalidMoves = (byte)BattleContexts.BattleSystem_CheckInvalidMoves(battleSys, battleCtx, battler, 0, -1);
        for (int i = 0; i < MaxMoves; i++)
        {
            if ((HealthBar.FlagIndex(i) & invalidMoves) != 0)
            {
                ai.MoveScore[i] = 0;
            }
            ai.MoveDamageRolls[i] = (byte)(100 - (int)(BattleSystems.BattleSystem_RandNext(battleSys) % DamageRollRange));
        }
        ai.ScriptStackSize = 0;
        if ((battleSys.BattleType & BattleTypeRoaming) != 0)
        {
            ai.ThinkingMask = ThinkingMaskRoaming;
        }
        else
        {
            ai.ThinkingMask = Nitro.Bin.U32(battleSys.Trainers[battler], TrainerData.AIMaskOffset);
        }
        if ((battleSys.BattleType & BattleSystems.BattleTypeDoubles) != 0)
        {
            ai.ThinkingMask |= ThinkingMaskDoubles;
        }
    }

    // OFFSET 0x0221FCF4
    public static byte TrainerAI_Main(BattleSystem battleSys, int battler)
    {
        BattleContext battleCtx = battleSys.BattleCtx!;
        AIContext ai = battleCtx.AiContext;
        if ((ai.StateFlags & StateFlagPartnerSet) == 0)
        {
            ai.Attacker = (byte)battler;
            ai.Defender = (byte)BattleContexts.BattleSystem_RandomOpponent(battleSys, battleCtx, battler);
            TrainerAI_Init(battleSys, battleCtx, ai.Attacker, 0xF);
        }
        if ((battleSys.BattleType & BattleSystems.BattleTypeDoubles) == 0)
        {
            return TrainerAI_MainSingles(battleSys, battleCtx);
        }
        return TrainerAI_MainDoubles(battleSys, battleCtx);
    }

    private static int TrainerAI_PickBestMove(BattleSystem battleSys, BattleContext battleCtx, out byte maxScore)
    {
        AIContext ai = battleCtx.AiContext;
        byte[] maxSlots = new byte[MaxMoves];
        maxScore = (byte)ai.MoveScore[0];
        maxSlots[0] = 0;
        int count = 1;
        BattleMon mon = battleCtx.BattleMons[ai.Attacker];
        for (int i = 1; i < MaxMoves; i++)
        {
            if (mon.Moves[i] == 0)
            {
                continue;
            }
            int score = ai.MoveScore[i];
            if (maxScore == score)
            {
                maxSlots[count] = (byte)i;
                count++;
            }
            if (maxScore < score)
            {
                maxScore = (byte)score;
                count = 1;
                maxSlots[0] = (byte)i;
            }
        }
        return maxSlots[BattleSystems.BattleSystem_RandNext(battleSys) % count];
    }

    private static void TrainerAI_RunThinking(BattleSystem battleSys, BattleContext battleCtx, uint mask)
    {
        AIContext ai = battleCtx.AiContext;
        while (mask != 0)
        {
            if ((mask & 1) != 0)
            {
                if ((ai.StateFlags & StateFlagPartnerSet) == 0)
                {
                    ai.EvalStep = EvalStepInit;
                }
                TrainerAI_EvalMoves(battleSys, battleCtx);
            }
            mask >>= 1;
            ai.ThinkingBitShift++;
            ai.MoveSlot = 0;
        }
    }

    // OFFSET 0x0221FD44
    private static byte TrainerAI_MainSingles(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        TrainerAI_RecordLastMove(battleSys, battleCtx);
        while (ai.ThinkingMask != 0)
        {
            if ((ai.ThinkingMask & 1) != 0)
            {
                if ((ai.StateFlags & StateFlagPartnerSet) == 0)
                {
                    ai.EvalStep = EvalStepInit;
                }
                TrainerAI_EvalMoves(battleSys, battleCtx);
            }
            ai.ThinkingMask >>= 1;
            ai.ThinkingBitShift++;
            ai.MoveSlot = 0;
        }
        int result;
        if ((ai.StateFlags & StateFlagFlee) != 0)
        {
            result = ActionFlee;
        }
        else if ((ai.StateFlags & StateFlagSafari) != 0)
        {
            result = ActionSafari;
        }
        else
        {
            result = TrainerAI_PickBestMove(battleSys, battleCtx, out _);
        }
        ai.SelectedTarget[ai.Attacker] = ai.Defender;
        return (byte)result;
    }

    // OFFSET 0x0221FE38
    private static byte TrainerAI_MainDoubles(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        sbyte[] moveSlots = new sbyte[MaxBattlers];
        short[] scores = new short[MaxBattlers];
        for (int i = 0; i < MaxBattlers; i++)
        {
            if (i == ai.Attacker || battleCtx.BattleMons[i].CurHP == 0)
            {
                moveSlots[i] = NoTarget;
                scores[i] = NoTarget;
                continue;
            }
            TrainerAI_Init(battleSys, battleCtx, ai.Attacker, 0xF);
            ai.Defender = (byte)i;
            if ((i & 1) != (ai.Attacker & 1))
            {
                TrainerAI_RecordLastMove(battleSys, battleCtx);
            }
            ai.ThinkingBitShift = 0;
            ai.MoveSlot = 0;
            int mask = (int)ai.ThinkingMask;
            while (mask != 0)
            {
                if ((mask & 1) != 0)
                {
                    if ((ai.StateFlags & StateFlagPartnerSet) == 0)
                    {
                        ai.EvalStep = EvalStepInit;
                    }
                    TrainerAI_EvalMoves(battleSys, battleCtx);
                }
                mask >>= 1;
                ai.ThinkingBitShift++;
                ai.MoveSlot = 0;
            }
            if ((ai.StateFlags & StateFlagFlee) != 0)
            {
                moveSlots[i] = ActionFlee;
            }
            else if ((ai.StateFlags & StateFlagSafari) != 0)
            {
                moveSlots[i] = ActionSafari;
            }
            else
            {
                moveSlots[i] = (sbyte)TrainerAI_PickBestMove(battleSys, battleCtx, out byte maxScore);
                scores[i] = maxScore;
                if (i == (ai.Attacker ^ 2) && scores[i] < InitialScore)
                {
                    scores[i] = NoTarget;
                }
            }
        }
        byte[] targets = new byte[MaxBattlers];
        short best = scores[0];
        targets[0] = 0;
        int count = 1;
        for (int i = 1; i < MaxBattlers; i++)
        {
            if (best == scores[i])
            {
                targets[count] = (byte)i;
                count++;
            }
            if (best < scores[i])
            {
                best = scores[i];
                targets[0] = (byte)i;
                count = 1;
            }
        }
        ai.SelectedTarget[ai.Attacker] = targets[BattleSystems.BattleSystem_RandNext(battleSys) % count];
        int slot = moveSlots[ai.SelectedTarget[ai.Attacker]];
        ushort move = battleCtx.BattleMons[ai.Attacker].Moves[slot];
        if (ai.MoveTable[move].Range == RangeUserOrAlly && BattleSystems.BattleSystem_GetBattlerSide(battleSys, ai.SelectedTarget[ai.Attacker]) == 0)
        {
            ai.SelectedTarget[ai.Attacker] = ai.Attacker;
        }
        if (move == MoveCurse && BattleContexts.Move_IsGhostCurse(battleCtx, move, ai.Attacker) == 0)
        {
            ai.SelectedTarget[ai.Attacker] = ai.Attacker;
        }
        return (byte)slot;
    }

    // OFFSET 0x02220078
    private static void TrainerAI_EvalMoves(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        while (ai.EvalStep != EvalStepDone)
        {
            switch (ai.EvalStep)
            {
                case EvalStepInit:
                {
                    battleCtx.AiScriptCursor = Script(battleCtx)[ai.ThinkingBitShift];
                    BattleMon mon = battleCtx.BattleMons[ai.Attacker];
                    ai.Move = mon.PpCur[ai.MoveSlot] == 0 ? (ushort)0 : mon.Moves[ai.MoveSlot];
                    ai.EvalStep++;
                    break;
                }
                case EvalStepRun:
                    if (ai.Move != 0)
                    {
                        AICommand(Script(battleCtx)[battleCtx.AiScriptCursor])(battleSys, battleCtx);
                    }
                    else
                    {
                        ai.MoveScore[ai.MoveSlot] = 0;
                        ai.StateFlags |= StateFlagMoveDone;
                    }
                    if ((ai.StateFlags & StateFlagMoveDone) != 0)
                    {
                        ai.MoveSlot++;
                        if (ai.MoveSlot < MaxMoves && (ai.StateFlags & StateFlagEnd) == 0)
                        {
                            ai.EvalStep = EvalStepInit;
                        }
                        else
                        {
                            ai.EvalStep++;
                        }
                        ai.StateFlags &= unchecked((byte)~StateFlagMoveDone);
                    }
                    break;
            }
        }
    }

    // OFFSET 0x02222C28
    private static void AIScript_PushCursor(BattleSystem battleSys, BattleContext battleCtx, int offset)
    {
        AIContext ai = battleCtx.AiContext;
        ai.ScriptStackPointer[ai.ScriptStackSize++] = battleCtx.AiScriptCursor;
        AIScript_Iter(battleCtx, offset);
        if (ai.ScriptStackSize > ScriptStackMax)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02222C60
    private static int AIScript_PopCursor(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        if (ai.ScriptStackSize == 0)
        {
            return 0;
        }
        ai.ScriptStackSize--;
        battleCtx.AiScriptCursor = ai.ScriptStackPointer[ai.ScriptStackSize];
        return 1;
    }

    // OFFSET 0x02222C88
    private static void TrainerAI_RecordLastMove(BattleSystem battleSys, BattleContext battleCtx)
    {
        AIContext ai = battleCtx.AiContext;
        int defender = ai.Defender;
        ushort prev = battleCtx.MovePrevByBattler[defender];
        for (int i = 0; i < MaxMoves; i++)
        {
            if (prev == ai.BattlerMoves[defender, i])
            {
                return;
            }
            if (ai.BattlerMoves[defender, i] == 0)
            {
                ai.BattlerMoves[defender, i] = prev;
                return;
            }
        }
    }

    // OFFSET 0x02222CF0
    private static int AIScript_Read(BattleContext battleCtx)
    {
        int value = (int)Script(battleCtx)[battleCtx.AiScriptCursor];
        battleCtx.AiScriptCursor++;
        return value;
    }

    // OFFSET 0x02222D10
    private static int AIScript_ReadOffset(BattleContext battleCtx, int offset)
    {
        return (int)Script(battleCtx)[battleCtx.AiScriptCursor + offset];
    }

    // OFFSET 0x02222D24
    private static void AIScript_Iter(BattleContext battleCtx, int count)
    {
        battleCtx.AiScriptCursor = (uint)(battleCtx.AiScriptCursor + count);
    }

    // OFFSET 0x02222D34
    private static byte AIScript_Battler(BattleContext battleCtx, byte inBattler)
    {
        AIContext ai = battleCtx.AiContext;
        switch (inBattler)
        {
            case AIBattlerAttacker:
                return ai.Attacker;
            case AIBattlerAttackerPartner:
                return (byte)(ai.Attacker ^ 2);
            case AIBattlerDefenderPartner:
                return (byte)(ai.Defender ^ 2);
        }
        return ai.Defender;
    }

    // OFFSET 0x02222D7C
    private static int TrainerAI_CalcAllDamage(BattleSystem battleSys, BattleContext battleCtx, int attacker, ushort[] moves, int[] damageVals, ushort heldItem, byte[] ivs, int ability, int embargoTurns, int varyDamage)
    {
        AIContext ai = battleCtx.AiContext;
        int max = 0;
        for (int i = 0; i < MaxMoves; i++)
        {
            if (AI_MoveDealsDamage(ai, moves[i]))
            {
                int variance = varyDamage == 1 ? ai.MoveDamageRolls[i] : 100;
                damageVals[i] = TrainerAI_CalcDamage(battleSys, battleCtx, moves[i], heldItem, ivs, attacker, ability, embargoTurns, (byte)variance);
            }
            else
            {
                damageVals[i] = 0;
            }
        }
        for (int i = 0; i < MaxMoves; i++)
        {
            if (max < damageVals[i])
            {
                max = damageVals[i];
            }
        }
        return max;
    }

    private static Action<BattleSystem, BattleContext> AICommand(uint command)
    {
        return command < commands.Length ? commands[command] : (battleSys, battleCtx) => throw Missing(0x0222EEAC + command * 4);
    }
}
