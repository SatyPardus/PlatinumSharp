namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    public const int CompareSpeedFaster = 0;
    public const int CompareSpeedSlower = 1;
    public const int CompareSpeedTie = 2;

    private const int AbilityCloudNine = 0xD;
    private const int AbilitySwiftSwim = 0x21;
    private const int AbilityChlorophyll = 0x22;
    private const int AbilityAirLock = 0x4C;
    private const int AbilityGluttony = 0x52;
    private const int AbilityUnburden = 0x54;
    private const int AbilitySimple = 0x56;
    private const int AbilityQuickFeet = 0x5F;
    private const int AbilityStall = 0x64;
    private const int AbilitySlowStart = 0x70;
    private const int HoldEffectCustapBerry = 0x2D;
    private const int HoldEffectQuickClaw = 0x34;
    private const int HoldEffectQuickPowder = 0x66;
    private const int HoldEffectLaggingTail = 0x6B;
    private const int HoldEffectChoiceScarf = 0x73;
    private const int SpeciesDitto = 0x84;
    private const int MoveStruggle = 0xA5;
    private const int ActionFight = 1;
    private const int StatSpeed = 3;
    private const uint FieldConditionRain = 0x3;
    private const uint FieldConditionSun = 0x30;
    private const uint FieldConditionTrickRoom = 0x70000;
    private const uint SideConditionTailwind = 0x300;
    private const uint MonConditionParalysis = 0x40;
    private const uint StatStageTable = 0x0226EBE0;
    private const uint SpeedHalvingItemTable = 0x0226EBA8;
    private const int SpeedHalvingItemCount = 8;
    private const int ItemParamHoldEffect = 1;
    private const int CountAbilityAllExceptFainted = 8;
    private const int SlowStartTurns = 5;

    private static byte[]? statStages;
    private static byte[]? speedHalvingItems;

    private static uint BattleMon_ApplySpeedModifiers(BattleSystem battleSys, BattleContext battleCtx, int battler, uint speed, int ability, int holdEffect, bool weatherActive)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        if (weatherActive)
        {
            if ((ability == AbilitySwiftSwim && (battleCtx.FieldConditionsMask & FieldConditionRain) != 0)
                || (ability == AbilityChlorophyll && (battleCtx.FieldConditionsMask & FieldConditionSun) != 0))
            {
                speed *= 2;
            }
        }
        speedHalvingItems ??= RomData.Overlay(16, SpeedHalvingItemTable, SpeedHalvingItemCount);
        for (int i = 0; i < SpeedHalvingItemCount; i++)
        {
            if (speedHalvingItems[i] == BattleSystem_GetItemData(battleCtx, mon.HeldItem, ItemParamHoldEffect))
            {
                speed /= 2;
                break;
            }
        }
        if (holdEffect == HoldEffectChoiceScarf)
        {
            speed = speed * 15 / 10;
        }
        if (holdEffect == HoldEffectQuickPowder && mon.Species == SpeciesDitto)
        {
            speed *= 2;
        }
        if (ability == AbilityQuickFeet && (mon.Status & 0xFF) != 0)
        {
            speed = speed * 15 / 10;
        }
        else if ((mon.Status & MonConditionParalysis) != 0)
        {
            speed /= 4;
        }
        if (ability == AbilitySlowStart && battleCtx.TotalTurns - mon.MoveEffectsData.SlowStartTurnNumber < SlowStartTurns)
        {
            speed /= 2;
        }
        if (ability == AbilityUnburden && mon.MoveEffectsData.CanUnburden != 0 && mon.HeldItem == 0)
        {
            speed *= 2;
        }
        if ((battleCtx.SideConditionsMask[BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler)] & SideConditionTailwind) != 0)
        {
            speed *= 2;
        }
        return speed;
    }

    private static int BattleMon_SpeedStage(BattleContext battleCtx, int battler)
    {
        int stage = battleCtx.BattleMons[battler].StatBoosts[StatSpeed];
        if (Battler_Ability(battleCtx, battler) == AbilitySimple)
        {
            stage = (stage - 6) * 2 + 6;
            if (stage > 12)
            {
                stage = 12;
            }
            if (stage < 0)
            {
                stage = 0;
            }
        }
        return stage;
    }

    private static bool BattleMon_CheckPriorityItems(BattleContext battleCtx, int battler, int holdEffect, int holdPower, int ignoreQuickClaw)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        bool first = false;
        if (holdEffect == HoldEffectQuickClaw)
        {
            if (battleCtx.SpeedRand[battler] % (100 / holdPower) == 0)
            {
                first = true;
                if (ignoreQuickClaw == 0)
                {
                    mon.MoveEffectsData.QuickClaw = 1;
                }
            }
        }
        if (holdEffect == HoldEffectCustapBerry)
        {
            if (Battler_Ability(battleCtx, battler) == AbilityGluttony)
            {
                holdPower /= 2;
            }
            if ((uint)mon.CurHP <= mon.MaxHP / (uint)holdPower)
            {
                first = true;
                if (ignoreQuickClaw == 0)
                {
                    mon.MoveEffectsData.CustapBerry = 1;
                }
            }
        }
        return first;
    }

    // OFFSET 0x02252EC8
    public static byte BattleSystem_CompareBattlerSpeed(BattleSystem battleSys, BattleContext battleCtx, int battler1, int battler2, int ignoreQuickClaw)
    {
        byte result = CompareSpeedFaster;
        int priority1 = 0;
        int priority2 = 0;
        BattleMon mon1 = battleCtx.BattleMons[battler1];
        BattleMon mon2 = battleCtx.BattleMons[battler2];
        if (mon1.CurHP == 0 && mon2.CurHP != 0)
        {
            return CompareSpeedSlower;
        }
        if (mon1.CurHP != 0 && mon2.CurHP == 0)
        {
            return CompareSpeedFaster;
        }
        int ability1 = Battler_Ability(battleCtx, battler1);
        int ability2 = Battler_Ability(battleCtx, battler2);
        int holdEffect1 = (byte)Battler_HeldItemEffect(battleCtx, battler1);
        int holdPower1 = (byte)Battler_HeldItemPower(battleCtx, battler1, 0);
        int holdEffect2 = (byte)Battler_HeldItemEffect(battleCtx, battler2);
        int holdPower2 = (byte)Battler_HeldItemPower(battleCtx, battler2, 0);
        int stage1 = BattleMon_SpeedStage(battleCtx, battler1);
        int stage2 = BattleMon_SpeedStage(battleCtx, battler2);
        statStages ??= RomData.Overlay(16, StatStageTable, 13 * 2);
        uint speed1 = (uint)(mon1.Speed * statStages[stage1 * 2] / statStages[stage1 * 2 + 1]);
        uint speed2 = (uint)(mon2.Speed * statStages[stage2 * 2] / statStages[stage2 * 2 + 1]);
        bool weatherActive = BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityAllExceptFainted, 0, AbilityCloudNine) == 0
            && BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityAllExceptFainted, 0, AbilityAirLock) == 0;
        speed1 = BattleMon_ApplySpeedModifiers(battleSys, battleCtx, battler1, speed1, ability1, holdEffect1, weatherActive);
        bool quickClaw1 = BattleMon_CheckPriorityItems(battleCtx, battler1, holdEffect1, holdPower1, ignoreQuickClaw);
        bool laggingTail1 = holdEffect1 == HoldEffectLaggingTail;
        speed2 = BattleMon_ApplySpeedModifiers(battleSys, battleCtx, battler2, speed2, ability2, holdEffect2, weatherActive);
        bool quickClaw2 = BattleMon_CheckPriorityItems(battleCtx, battler2, holdEffect2, holdPower2, ignoreQuickClaw);
        bool laggingTail2 = holdEffect2 == HoldEffectLaggingTail;
        battleCtx.MonSpeedValues[battler1] = speed1;
        battleCtx.MonSpeedValues[battler2] = speed2;
        if (ignoreQuickClaw == 0)
        {
            int move1 = 0;
            int move2 = 0;
            uint action1 = battleCtx.BattlerActions[battler1, 3];
            uint action2 = battleCtx.BattlerActions[battler2, 3];
            int moveSlot1 = battleCtx.MoveSlot[battler1];
            int moveSlot2 = battleCtx.MoveSlot[battler2];
            if (action1 == ActionFight)
            {
                move1 = battleCtx.TurnFlags[battler1].Struggling != 0 ? MoveStruggle : (ushort)BattleMon_Get(battleCtx, battler1, BattleMonMove1 + moveSlot1, null);
            }
            if (action2 == ActionFight)
            {
                move2 = battleCtx.TurnFlags[battler2].Struggling != 0 ? MoveStruggle : (ushort)BattleMon_Get(battleCtx, battler2, BattleMonMove1 + moveSlot2, null);
            }
            priority1 = battleCtx.AiContext.MoveTable[move1].Priority;
            priority2 = battleCtx.AiContext.MoveTable[move2].Priority;
        }
        if (priority1 == priority2)
        {
            if (quickClaw1 && quickClaw2)
            {
                if (speed1 < speed2)
                {
                    result = CompareSpeedSlower;
                }
                else if (speed1 == speed2 && (BattleSystems.BattleSystem_RandNext(battleSys) & 1) != 0)
                {
                    result = CompareSpeedTie;
                }
            }
            else if (!quickClaw1 && quickClaw2)
            {
                result = CompareSpeedSlower;
            }
            else if (quickClaw1 && !quickClaw2)
            {
                result = CompareSpeedFaster;
            }
            else if (laggingTail1 && laggingTail2)
            {
                if (speed1 > speed2)
                {
                    result = CompareSpeedSlower;
                }
                else if (speed1 == speed2 && (BattleSystems.BattleSystem_RandNext(battleSys) & 1) != 0)
                {
                    result = CompareSpeedTie;
                }
            }
            else if (laggingTail1 && !laggingTail2)
            {
                result = CompareSpeedSlower;
            }
            else if (!laggingTail1 && laggingTail2)
            {
                result = CompareSpeedFaster;
            }
            else if (ability1 == AbilityStall && ability2 == AbilityStall)
            {
                if (speed1 > speed2)
                {
                    result = CompareSpeedSlower;
                }
                else if (speed1 == speed2 && (BattleSystems.BattleSystem_RandNext(battleSys) & 1) != 0)
                {
                    result = CompareSpeedTie;
                }
            }
            else if (ability1 == AbilityStall && ability2 != AbilityStall)
            {
                result = CompareSpeedSlower;
            }
            else if (ability1 != AbilityStall && ability2 == AbilityStall)
            {
                result = CompareSpeedFaster;
            }
            else if ((battleCtx.FieldConditionsMask & FieldConditionTrickRoom) != 0)
            {
                if (speed1 > speed2)
                {
                    result = CompareSpeedSlower;
                }
                if (speed1 == speed2 && (BattleSystems.BattleSystem_RandNext(battleSys) & 1) != 0)
                {
                    result = CompareSpeedTie;
                }
            }
            else
            {
                if (speed1 < speed2)
                {
                    result = CompareSpeedSlower;
                }
                if (speed1 == speed2 && (BattleSystems.BattleSystem_RandNext(battleSys) & 1) != 0)
                {
                    result = CompareSpeedTie;
                }
            }
        }
        else if (priority1 < priority2)
        {
            result = CompareSpeedSlower;
        }
        return result;
    }
}
