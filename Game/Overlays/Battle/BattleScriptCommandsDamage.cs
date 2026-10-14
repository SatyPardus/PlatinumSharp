namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int AbilityNormalize = 0x60;
    private const int HoldEffectLifeOrb = 0x62;
    private const int HoldEffectMetronome = 0x69;
    private const uint BattleStatusAttackMessagePrinted = 1;
    private const uint BattleStatus2AttackMessage = 4;
    private const uint BattleStatusMoveAnimationPlayed = 0x4000;
    private const int MoveTransform = 0x90;
    private const int SubSeqAnimationsOff = 0x123;
    private const byte MessageTagGlobal = 0x80;
    private const byte MessageTagLocal = 0x40;
    private const int BattlerSelectorSwitchedMonAfter = 0x16;
    private const int BattlerSelectorAttacker = 1;
    private const int BattlerSelectorDefender = 2;
    private const int BattlerSelectorMsgBattlerTemp = 0x15;
    private const int BattlerSelectorMsgTemp = 0xFF;
    private const int ActionCommandNoneScript = 0x27;
    private const int FriendshipEventFaint = 6;
    private const int FriendshipEventFaintLargeLevelGap = 8;
    private const int FriendshipLevelGap = 0x1E;

    private const int TagKindBattler = 0;
    private const int TagKindName = 1;
    private const int TagKindMove = 2;
    private const int TagKindItem = 3;
    private const int TagKindNum = 4;
    private const int TagKindType = 5;
    private const int TagKindAbility = 6;
    private const int TagKindStat = 7;
    private const int TagKindStatus = 8;
    private const int TagKindPoke = 9;
    private const int TagKindPoffin = 10;
    private const int TagKindFlavor = 11;
    private const int TagKindTrainerClass = 12;
    private const int TagKindTrainerName = 13;
    private const int TagKindRaw = 14;

    private static readonly int[][] MessageTagKinds =
    [
        [],
        [TagKindBattler],
        [TagKindName],
        [TagKindMove],
        [TagKindStat],
        [TagKindItem],
        [TagKindNum],
        [TagKindNum],
        [TagKindTrainerName],
        [TagKindName, TagKindName],
        [TagKindName, TagKindMove],
        [TagKindName, TagKindAbility],
        [TagKindName, TagKindStat],
        [TagKindName, TagKindType],
        [TagKindName, TagKindPoke],
        [TagKindName, TagKindItem],
        [TagKindName, TagKindPoffin],
        [TagKindName, TagKindNum],
        [TagKindName, TagKindTrainerName],
        [TagKindName, TagKindRaw],
        [TagKindMove, TagKindBattler],
        [TagKindMove, TagKindName],
        [TagKindMove, TagKindMove],
        [TagKindAbility, TagKindName],
        [TagKindItem, TagKindMove],
        [TagKindNum, TagKindNum],
        [TagKindTrainerName, TagKindTrainerName],
        [TagKindTrainerName, TagKindName],
        [TagKindTrainerName, TagKindItem],
        [TagKindTrainerName, TagKindNum],
        [TagKindTrainerClass, TagKindTrainerName],
        [TagKindName, TagKindName, TagKindMove],
        [TagKindName, TagKindName, TagKindAbility],
        [TagKindName, TagKindName, TagKindItem],
        [TagKindName, TagKindMove, TagKindMove],
        [TagKindName, TagKindMove, TagKindNum],
        [TagKindName, TagKindAbility, TagKindName],
        [TagKindName, TagKindAbility, TagKindMove],
        [TagKindName, TagKindAbility, TagKindItem],
        [TagKindName, TagKindAbility, TagKindStat],
        [TagKindName, TagKindAbility, TagKindType],
        [TagKindName, TagKindAbility, TagKindStatus],
        [TagKindName, TagKindAbility, TagKindNum],
        [TagKindName, TagKindItem, TagKindName],
        [TagKindName, TagKindItem, TagKindMove],
        [TagKindName, TagKindItem, TagKindStat],
        [TagKindName, TagKindItem, TagKindStatus],
        [TagKindName, TagKindRaw, TagKindRaw],
        [TagKindItem, TagKindName, TagKindFlavor],
        [TagKindTrainerName, TagKindName, TagKindName],
        [TagKindTrainerClass, TagKindTrainerName, TagKindName],
        [TagKindTrainerClass, TagKindTrainerName, TagKindItem],
        [TagKindName, TagKindAbility, TagKindName, TagKindMove],
        [TagKindName, TagKindAbility, TagKindName, TagKindAbility],
        [TagKindName, TagKindAbility, TagKindName, TagKindStat],
        [TagKindName, TagKindItem, TagKindName, TagKindItem],
        [TagKindTrainerName, TagKindName, TagKindTrainerName, TagKindName],
        [TagKindTrainerClass, TagKindTrainerName, TagKindName, TagKindName],
        [TagKindTrainerClass, TagKindTrainerName, TagKindName, TagKindTrainerName],
        [TagKindTrainerClass, TagKindTrainerName, TagKindTrainerClass, TagKindTrainerName],
        [TagKindTrainerClass, TagKindTrainerName, TagKindName, TagKindTrainerClass, TagKindTrainerName, TagKindName],
    ];

    // OFFSET 0x02241374
    private static void BattleScript_CalcMoveDamage(BattleSystem battleSys, BattleContext battleCtx)
    {
        int moveType;
        if (BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) == AbilityNormalize)
        {
            moveType = 0;
        }
        else if (battleCtx.MoveType != 0)
        {
            moveType = battleCtx.MoveType;
        }
        else
        {
            moveType = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Type;
        }
        int defender = battleCtx.Defender;
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, defender);
        battleCtx.Damage = BattleContexts.BattleSystem_CalcMoveDamage(battleSys, battleCtx, (int)battleCtx.MoveCur, battleCtx.SideConditionsMask[side], battleCtx.FieldConditionsMask, (ushort)battleCtx.MovePower, (byte)moveType, (byte)battleCtx.Attacker, (byte)defender, (byte)battleCtx.CriticalMul);
        battleCtx.Damage *= battleCtx.CriticalMul;
        if (BattleContexts.Battler_HeldItemEffect(battleCtx, battleCtx.Attacker) == HoldEffectLifeOrb)
        {
            battleCtx.Damage = battleCtx.Damage * (BattleContexts.Battler_HeldItemPower(battleCtx, battleCtx.Attacker, 0) + 100) / 100;
        }
        if (BattleContexts.Battler_HeldItemEffect(battleCtx, battleCtx.Attacker) == HoldEffectMetronome)
        {
            battleCtx.Damage = (int)((uint)(battleCtx.Damage * (battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.MetronomeTurns + 10)) / 10);
        }
        MoveEffectsData effects = battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData;
        if (effects.MeFirst != 0)
        {
            if (battleCtx.MeFirstTurnOrder == effects.MeFirstTurnNumber)
            {
                effects.MeFirstTurnNumber--;
            }
            if (battleCtx.MeFirstTurnOrder - effects.MeFirstTurnNumber < 2)
            {
                battleCtx.Damage = battleCtx.Damage * 15 / 10;
            }
            else
            {
                effects.MeFirst = 0;
            }
        }
    }

    // OFFSET 0x022414E0
    private static int BattleScript_CalcDamage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleScript_CalcMoveDamage(battleSys, battleCtx);
        battleCtx.Damage = BattleContexts.BattleSystem_CalcDamageVariance(battleSys, battleCtx, battleCtx.Damage);
        battleCtx.Damage *= -1;
        return 0;
    }

    // OFFSET 0x02241518
    private static int BattleScript_CalcMaxDamage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleScript_CalcMoveDamage(battleSys, battleCtx);
        battleCtx.Damage *= -1;
        return 0;
    }

    // OFFSET 0x02241544
    private static int BattleScript_PrintAttackMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        if ((battleCtx.BattleStatusMask & BattleStatusAttackMessagePrinted) == 0)
        {
            BattleIO.BattleIO_PrintAttackMessage(battleSys, battleCtx);
        }
        battleCtx.BattleStatusMask |= BattleStatusAttackMessagePrinted;
        battleCtx.BattleStatusMask2 |= BattleStatus2AttackMessage;
        return 0;
    }

    // OFFSET 0x02241584
    private static int BattleScript_PrintMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleMessageParams msgParams = new();
        BattleMessage msg = new();
        BattleScript_GetBattleMessage(battleCtx, msgParams);
        BattleMessage_Make(battleSys, battleCtx, msgParams, msg);
        BattleIO.BattleIO_PrintMessage(battleSys, battleCtx, msg);
        return 0;
    }

    // OFFSET 0x022415B8
    private static int BattleScript_PrintGlobalMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleMessageParams msgParams = new();
        BattleMessage msg = new();
        BattleScript_GetBattleMessage(battleCtx, msgParams);
        BattleMessage_Make(battleSys, battleCtx, msgParams, msg);
        msg.Tags |= MessageTagGlobal;
        BattleIO.BattleIO_PrintMessage(battleSys, battleCtx, msg);
        return 0;
    }

    // OFFSET 0x022415F8
    private static int BattleScript_PrintBufferedMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleIO.BattleIO_PrintMessage(battleSys, battleCtx, battleCtx.MsgBuffer);
        return 0;
    }

    // OFFSET 0x02241618
    private static int BattleScript_BufferMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleMessageParams msgParams = new();
        BattleScript_GetBattleMessage(battleCtx, msgParams);
        BattleMessage_Make(battleSys, battleCtx, msgParams, battleCtx.MsgBuffer);
        return 0;
    }

    // OFFSET 0x02241644
    private static int BattleScript_BufferLocalMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        BattleMessageParams msgParams = new();
        BattleMessage msg = new();
        BattleScript_GetBattleMessage(battleCtx, msgParams);
        BattleMessage_Make(battleSys, battleCtx, msgParams, msg);
        msg.Tags |= MessageTagLocal;
        msg.Battler = BattleScript_Battler(battleSys, battleCtx, selector);
        BattleIO.BattleIO_PrintMessage(battleSys, battleCtx, msg);
        return 0;
    }

    // OFFSET 0x02241698
    private static int BattleScript_PlayMoveAnimation(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        ushort move = (ushort)(selector == BattlerSelectorMsgTemp ? battleCtx.MsgMoveTemp : (int)battleCtx.MoveCur);
        if (((battleCtx.BattleStatusMask & BattleStatusMoveAnimationPlayed) == 0 && BattleSystems.BattleSystem_AreAnimationsOn(battleSys) == 1) || move == MoveTransform)
        {
            battleCtx.BattleStatusMask |= BattleStatusMoveAnimationPlayed;
            BattleIO.BattleIO_PlayMoveAnimation(battleSys, battleCtx, move);
        }
        if (BattleSystems.BattleSystem_AreAnimationsOn(battleSys) == 0)
        {
            BattleScript_Call(battleCtx, ScriptNarcSubSeq, SubSeqAnimationsOff);
        }
        return 0;
    }

    // OFFSET 0x02241714
    private static int BattleScript_PlayMoveAnimationOnMons(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int attackerSelector = BattleScript_Read(battleCtx);
        int defenderSelector = BattleScript_Read(battleCtx);
        ushort move = (ushort)(selector == BattlerSelectorMsgTemp ? battleCtx.MsgMoveTemp : (int)battleCtx.MoveCur);
        int attacker = BattleScript_Battler(battleSys, battleCtx, attackerSelector);
        int defender = BattleScript_Battler(battleSys, battleCtx, defenderSelector);
        if (((battleCtx.BattleStatusMask & BattleStatusMoveAnimationPlayed) == 0 && BattleSystems.BattleSystem_AreAnimationsOn(battleSys) == 1) || move == MoveTransform)
        {
            battleCtx.BattleStatusMask |= BattleStatusMoveAnimationPlayed;
            BattleIO.BattleIO_PlayMoveAnimationA2D(battleSys, battleCtx, move, attacker, defender);
        }
        if (BattleSystems.BattleSystem_AreAnimationsOn(battleSys) == 0)
        {
            BattleScript_Call(battleCtx, ScriptNarcSubSeq, SubSeqAnimationsOff);
        }
        return 0;
    }

    // OFFSET 0x022417C0
    private static int BattleScript_FlickerMon(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_FlickerBattler(battleSys, battler, battleCtx.MoveStatusFlags);
        return 0;
    }

    // OFFSET 0x022417F4
    private static int BattleScript_UpdateHealthBarValue(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleMon mon = battleCtx.BattleMons[battler];
        if (battleCtx.HpCalcTemp + mon.CurHP <= 0)
        {
            battleCtx.HitDamage = -mon.CurHP;
        }
        else
        {
            battleCtx.HitDamage = battleCtx.HpCalcTemp;
        }
        if (battleCtx.HitDamage < 0)
        {
            battleCtx.TotalDamage[battler] -= battleCtx.HitDamage;
        }
        mon.CurHP += battleCtx.HpCalcTemp;
        if (mon.CurHP < 0)
        {
            mon.CurHP = 0;
        }
        else if ((uint)mon.CurHP > mon.MaxHP)
        {
            mon.CurHP = (int)mon.MaxHP;
        }
        BattleContexts.BattleMon_CopyToParty(battleSys, battleCtx, battler);
        return 0;
    }

    // OFFSET 0x02241894
    private static int BattleScript_UpdateHealthBar(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_UpdateHPGauge(battleSys, battleCtx, battler);
        return 0;
    }

    // OFFSET 0x022418C0
    private static int BattleScript_TryFaintMon(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        if (battleCtx.BattleMons[battler].CurHP == 0)
        {
            battleCtx.FaintedMon = battler;
            battleCtx.BattleStatusMask |= HealthBar.FlagIndex(battler) << 24;
            battleCtx.TotalFainted[battler]++;
            BattleScript_UpdateFriendship(battleSys, battleCtx, battler);
        }
        return 0;
    }

    // OFFSET 0x02241924
    private static int BattleScript_PlayFaintAnimation(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleIO.BattleIO_PlayFaintAnimation(battleSys, battleCtx, battleCtx.FaintedMon);
        battleCtx.BattleStatusMask &= ~(HealthBar.FlagIndex(battleCtx.FaintedMon) << 24);
        battleCtx.BattleStatusMask2 |= HealthBar.FlagIndex(battleCtx.FaintedMon) << 28;
        battleCtx.BattlerActions[battleCtx.FaintedMon, 0] = ActionCommandNoneScript;
        BattleContexts.BattleSystem_CleanupFaintedMon(battleSys, battleCtx, battleCtx.FaintedMon);
        return 0;
    }

    // OFFSET 0x02241A20
    private static int BattleScript_PlaySound(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int sound = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        BattleIO.BattleIO_PlaySound(battleSys, battleCtx, sound, battler);
        return 0;
    }

    // OFFSET 0x0224ABEC
    private static void BattleScript_GetBattleMessage(BattleContext battleCtx, BattleMessageParams msgParams)
    {
        msgParams.Id = BattleScript_Read(battleCtx);
        msgParams.Tags = BattleScript_Read(battleCtx);
        int count = 0;
        if ((uint)msgParams.Tags <= 60)
        {
            count = MessageTagKinds[msgParams.Tags].Length;
        }
        for (int i = 0; i < count; i++)
        {
            msgParams.Params[i] = BattleScript_Read(battleCtx);
        }
    }

    // OFFSET 0x0224ACB8
    private static void BattleMessage_Make(BattleSystem battleSys, BattleContext battleCtx, BattleMessageParams msgParams, BattleMessage msg)
    {
        msg.Id = (ushort)msgParams.Id;
        msg.Tags = (byte)msgParams.Tags;
        if (msg.Tags > 60)
        {
            return;
        }
        int[] kinds = MessageTagKinds[msg.Tags];
        for (int i = 0; i < kinds.Length; i++)
        {
            int input = msgParams.Params[i];
            msg.Params[i] = kinds[i] switch
            {
                TagKindBattler => BattleScript_Battler(battleSys, battleCtx, input),
                TagKindName => BattleMessage_NameTag(battleSys, battleCtx, input),
                TagKindMove => BattleMessage_MoveTag(battleCtx, input),
                TagKindItem => BattleMessage_ItemTag(battleCtx, input),
                TagKindNum => BattleMessage_NumTag(battleCtx, input),
                TagKindType => BattleMessage_TypeTag(battleCtx, input),
                TagKindAbility => BattleMessage_AbilityTag(battleSys, battleCtx, input),
                TagKindStat => BattleMessage_StatTag(battleCtx, input),
                TagKindStatus => BattleMessage_StatusTag(battleCtx, input),
                TagKindPoke => BattleMessage_PokeTag(battleSys, battleCtx, input),
                TagKindPoffin => BattleMessage_PoffinTag(battleCtx, input),
                TagKindFlavor => BattleMessage_FlavorTag(battleCtx, input),
                TagKindTrainerClass => BattleMessage_TrainerClassTag(battleSys, battleCtx, input),
                TagKindTrainerName => BattleMessage_TrainerNameTag(battleSys, battleCtx, input),
                _ => input,
            };
        }
    }

    // OFFSET 0x0224B3B8
    private static int BattleMessage_NameTag(BattleSystem battleSys, BattleContext battleCtx, int battlerIn)
    {
        int battler = BattleScript_Battler(battleSys, battleCtx, battlerIn);
        if (battlerIn == BattlerSelectorSwitchedMonAfter)
        {
            return battler | (battleCtx.SwitchedPartySlot[battler] << 8);
        }
        return battler | (battleCtx.SelectedPartySlot[battler] << 8);
    }

    // OFFSET 0x0224B3E8
    private static int BattleMessage_MoveTag(BattleContext battleCtx, int battlerIn)
    {
        if (battlerIn == BattlerSelectorAttacker)
        {
            return (int)battleCtx.MoveCur;
        }
        if (battlerIn == BattlerSelectorMsgTemp)
        {
            return battleCtx.MsgMoveTemp;
        }
        return 0;
    }

    // OFFSET 0x0224B404
    private static int BattleMessage_ItemTag(BattleContext battleCtx, int battlerIn)
    {
        int item = 0;
        switch (battlerIn)
        {
            case BattlerSelectorAttacker:
                item = battleCtx.BattleMons[battleCtx.Attacker].HeldItem;
                BattleAI_SetHeldItem(battleCtx, (byte)battleCtx.Attacker, (ushort)item);
                break;
            case BattlerSelectorDefender:
                item = battleCtx.BattleMons[battleCtx.Defender].HeldItem;
                BattleAI_SetHeldItem(battleCtx, (byte)battleCtx.Defender, (ushort)item);
                break;
            case BattlerSelectorMsgBattlerTemp:
                item = battleCtx.BattleMons[battleCtx.MsgBattlerTemp].HeldItem;
                BattleAI_SetHeldItem(battleCtx, (byte)battleCtx.MsgBattlerTemp, (ushort)item);
                break;
            case BattlerSelectorMsgTemp:
                item = battleCtx.MsgItemTemp;
                break;
        }
        return item;
    }

    // OFFSET 0x0224B47C
    private static int BattleMessage_NumTag(BattleContext battleCtx, int battlerIn)
    {
        return battlerIn == BattlerSelectorMsgTemp ? battleCtx.MsgTemp : 0;
    }

    // OFFSET 0x0224B488
    private static int BattleMessage_TypeTag(BattleContext battleCtx, int battlerIn)
    {
        return battlerIn == BattlerSelectorMsgTemp ? battleCtx.MsgTemp : 0;
    }

    // OFFSET 0x0224B494
    private static int BattleMessage_AbilityTag(BattleSystem battleSys, BattleContext battleCtx, int battlerIn)
    {
        if (battlerIn == BattlerSelectorMsgTemp)
        {
            return battleCtx.MsgAbilityTemp;
        }
        int battler = BattleScript_Battler(battleSys, battleCtx, battlerIn);
        int ability = battleCtx.BattleMons[battler].Ability;
        BattleAI_SetAbility(battleCtx, (byte)battler, (byte)ability);
        return ability;
    }

    // OFFSET 0x0224B4C8
    private static int BattleMessage_StatTag(BattleContext battleCtx, int battlerIn)
    {
        return battlerIn == BattlerSelectorMsgTemp ? battleCtx.MsgTemp : 0;
    }

    // OFFSET 0x0224B4D4
    private static int BattleMessage_StatusTag(BattleContext battleCtx, int battlerIn)
    {
        return battlerIn == BattlerSelectorMsgTemp ? battleCtx.MsgTemp : 0;
    }

    // OFFSET 0x0224B4E0
    private static int BattleMessage_PokeTag(BattleSystem battleSys, BattleContext battleCtx, int battlerIn)
    {
        int battler = BattleScript_Battler(battleSys, battleCtx, battlerIn);
        return battler | (battleCtx.SelectedPartySlot[battler] << 8);
    }

    // OFFSET 0x0224B4F8
    private static int BattleMessage_PoffinTag(BattleContext battleCtx, int battlerIn)
    {
        return battlerIn == BattlerSelectorMsgTemp ? battleCtx.MsgTemp : 0;
    }

    // OFFSET 0x0224B504
    private static int BattleMessage_FlavorTag(BattleContext battleCtx, int battlerIn)
    {
        return battlerIn == BattlerSelectorMsgTemp ? battleCtx.MsgTemp : 0;
    }

    // OFFSET 0x0224B510
    private static int BattleMessage_TrainerClassTag(BattleSystem battleSys, BattleContext battleCtx, int battlerIn)
    {
        return BattleScript_Battler(battleSys, battleCtx, battlerIn);
    }

    // OFFSET 0x0224B518
    private static int BattleMessage_TrainerNameTag(BattleSystem battleSys, BattleContext battleCtx, int battlerIn)
    {
        return BattleScript_Battler(battleSys, battleCtx, battlerIn);
    }

    // OFFSET 0x0224B850
    private static void BattleScript_UpdateFriendship(BattleSystem battleSys, BattleContext battleCtx, int faintingBattler)
    {
        if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, faintingBattler) != 0)
        {
            return;
        }
        int opponent;
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeDoubles) != 0)
        {
            opponent = (byte)BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 3);
            int opponent2 = (byte)BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 5);
            if (battleCtx.BattleMons[opponent2].Level > battleCtx.BattleMons[opponent].Level)
            {
                opponent = opponent2;
            }
        }
        else
        {
            opponent = (byte)BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 1);
        }
        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, faintingBattler, battleCtx.SelectedPartySlot[faintingBattler]);
        int faintedLevel = battleCtx.BattleMons[faintingBattler].Level;
        int opponentLevel = battleCtx.BattleMons[opponent].Level;
        if (opponentLevel > faintedLevel)
        {
            if (opponentLevel - faintedLevel >= FriendshipLevelGap)
            {
                Pokemon.Pokemon_UpdateFriendship(mon, FriendshipEventFaintLargeLevelGap, (ushort)BattleSystems.BattleSystem_GetMapLabelTextID(battleSys));
            }
            else
            {
                Pokemon.Pokemon_UpdateFriendship(mon, FriendshipEventFaint, (ushort)BattleSystems.BattleSystem_GetMapLabelTextID(battleSys));
            }
        }
        else
        {
            Pokemon.Pokemon_UpdateFriendship(mon, FriendshipEventFaint, (ushort)BattleSystems.BattleSystem_GetMapLabelTextID(battleSys));
        }
    }

    // OFFSET 0x0224B928
    private static void BattleAI_SetAbility(BattleContext battleCtx, byte battler, byte ability)
    {
        battleCtx.AiContext.BattlerAbilities[battler] = ability;
    }

    // OFFSET 0x0224B934
    private static void BattleAI_SetHeldItem(BattleContext battleCtx, byte battler, ushort item)
    {
        battleCtx.AiContext.BattlerHeldItems[battler] = item;
    }
}

public sealed class BattleMessageParams
{
    public int Id;
    public int Tags;
    public readonly int[] Params = new int[6];
}

public static partial class BattleSystems
{
    private const uint BattleStatusNoRecordingAnimations = 0x10;
    private const int BattleSceneOn = 0;

    // OFFSET 0x0223EDAC
    public static int BattleSystem_AreAnimationsOn(BattleSystem battleSys)
    {
        if ((battleSys.BattleType & BattleTypeLink) != 0 && (battleSys.BattleStatusMask & BattleStatusNoRecordingAnimations) == 0)
        {
            return 1;
        }
        return Options.Options_BattleScene(battleSys.Options!) == BattleSceneOn ? 1 : 0;
    }
}

public static partial class BattleIO
{
    private const int CommandPlayMoveAnimation = 0x16;
    private const int CommandFlickerBattler = 0x17;
    private const int CommandUpdateHPGauge = 0x18;
    private const int CommandPlayFaintAnimation = 0x1A;
    private const int CommandPlaySound = 0x1B;
    private const int MoveAnimationSize = 0x58;
    private const int UpdateHPGaugeSize = 0x14;
    private const int FaintAnimationSize = 0x30;
    private const int AbilityCloudNineIO = 0xD;
    private const int AbilityAirLockIO = 0x4C;
    private const uint VolatileSubstitute = 0x1000000;
    private const uint VolatileTransformed = 0x200000;
    private const int GaugeSpeciesNidoranF = 0x1D;
    private const int GaugeSpeciesNidoranM = 0x20;
    private const int GaugeGenderNone = 2;
    private const int GaugeMonDataSpecies = 5;
    private const int GaugeMonDataLevel = 0xA1;

    // OFFSET 0x02265BBC
    public static void BattleIO_PlayMoveAnimation(BattleSystem battleSys, BattleContext battleCtx, ushort move)
    {
        byte[] message = new byte[MoveAnimationSize];
        BattleIO_SetMoveAnimation(battleSys, battleCtx, message, 0, 0, battleCtx.Attacker, battleCtx.Defender, move);
        BattleIO_Send(battleSys, RecipientBattler, battleCtx.Attacker, message, MoveAnimationSize);
    }

    // OFFSET 0x02265BEC
    public static void BattleIO_PlayMoveAnimationA2D(BattleSystem battleSys, BattleContext battleCtx, ushort move, int attacker, int defender)
    {
        byte[] message = new byte[MoveAnimationSize];
        BattleIO_SetMoveAnimation(battleSys, battleCtx, message, 0, 0, attacker, defender, move);
        BattleIO_Send(battleSys, RecipientBattler, attacker, message, MoveAnimationSize);
    }

    // OFFSET 0x02265C1C
    public static void BattleIO_FlickerBattler(BattleSystem battleSys, int battler, uint moveStatusFlags)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandFlickerBattler);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x02265C38
    public static void BattleIO_UpdateHPGauge(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, battleCtx.SelectedPartySlot[battler]);
        int species = (int)Pokemon.Pokemon_GetValue(mon, GaugeMonDataSpecies, null);
        int level = (int)Pokemon.Pokemon_GetValue(mon, GaugeMonDataLevel, null);
        BattleMon battleMon = battleCtx.BattleMons[battler];
        byte[] message = new byte[UpdateHPGaugeSize];
        W8(message, 0, CommandUpdateHPGauge);
        W8(message, 1, battleMon.Level);
        W16(message, 2, battleMon.CurHP);
        W16(message, 4, (int)battleMon.MaxHP);
        W32(message, 8, (uint)battleCtx.HpCalcTemp);
        if ((battleMon.Species == GaugeSpeciesNidoranF || battleMon.Species == GaugeSpeciesNidoranM) && battleMon.HasNickname == 0)
        {
            W8(message, 7, GaugeGenderNone);
        }
        else
        {
            W8(message, 7, battleMon.Gender & 0xF);
        }
        W32(message, 0xC, battleMon.Exp - Pokemon.Species_GetExpForLevel(species, level));
        W32(message, 0x10, Pokemon.Species_GetExpForLevel(species, level + 1) - Pokemon.Species_GetExpForLevel(species, level));
        BattleIO_Send(battleSys, RecipientBattler, battler, message, UpdateHPGaugeSize);
    }

    // OFFSET 0x02265D98
    public static void BattleIO_PlayFaintAnimation(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        byte[] message = new byte[FaintAnimationSize];
        BattleMon mon = battleCtx.BattleMons[battler];
        W8(message, 0, CommandPlayFaintAnimation);
        W16(message, 2, mon.Species);
        W8(message, 8, mon.FormNum & 0x1F);
        W8(message, 9, (mon.StatusVolatile & VolatileSubstitute) != 0 ? 1 : 0);
        W8(message, 0xA, (mon.StatusVolatile & VolatileTransformed) != 0 ? 1 : 0);
        if ((mon.StatusVolatile & VolatileTransformed) != 0)
        {
            W8(message, 1, mon.MoveEffectsData.TransformedGender);
            W32(message, 4, mon.MoveEffectsData.TransformedPID);
        }
        else
        {
            W8(message, 1, mon.Gender & 0xF);
            W32(message, 4, mon.Personality);
        }
        for (int i = 0; i < 4; i++)
        {
            BattleMon other = battleCtx.BattleMons[i];
            W16(message, 0xC + i * 2, other.Species);
            W8(message, 0x18 + i, other.IsShiny & 1);
            W8(message, 0x1C + i, other.FormNum & 0x1F);
            if ((other.StatusVolatile & VolatileTransformed) != 0)
            {
                W8(message, 0x14 + i, other.MoveEffectsData.TransformedGender);
                W32(message, 0x20 + i * 4, other.MoveEffectsData.TransformedPID);
            }
            else
            {
                W8(message, 0x14 + i, other.Gender & 0xF);
                W32(message, 0x20 + i * 4, other.Personality);
            }
        }
        BattleIO_Send(battleSys, RecipientBattler, battler, message, FaintAnimationSize);
    }

    // OFFSET 0x02265EAC
    public static void BattleIO_PlaySound(BattleSystem battleSys, BattleContext battleCtx, int sound, int battler)
    {
        byte[] message = new byte[4];
        W8(message, 0, CommandPlaySound);
        W16(message, 2, sound);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x02266B78
    public static void BattleIO_SetMoveAnimation(BattleSystem battleSys, BattleContext? battleCtx, byte[] message, int transferData, int transferValue, int attacker, int defender, ushort move)
    {
        W8(message, 0, CommandPlayMoveAnimation);
        W16(message, 2, move);
        W16(message, 0x14, attacker);
        W16(message, 0x16, defender);
        W32(message, 0x4C, (uint)transferData);
        W32(message, 0x50, (uint)transferValue);
        W32(message, 0x54, (uint)BattleSystems.BattleSystem_GetTerrain(battleSys));
        if (battleCtx == null)
        {
            return;
        }
        W32(message, 4, (uint)battleCtx.Damage);
        int power = battleCtx.MovePower;
        if (power == 0)
        {
            power = battleCtx.AiContext.MoveTable[move].Power;
        }
        W16(message, 8, power);
        BattleMon attackerMon = battleCtx.BattleMons[attacker];
        W16(message, 0xC, attackerMon.Friendship);
        if (BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, 8, 0, AbilityCloudNineIO) == 0
            && BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, 8, 0, AbilityAirLockIO) == 0)
        {
            W32(message, 0x10, battleCtx.FieldConditionsMask);
        }
        else
        {
            W32(message, 0x10, 0);
        }
        W16(message, 0xA, battleCtx.MoveEffectChance);
        int flags = 0;
        if ((attackerMon.StatusVolatile & VolatileSubstitute) != 0)
        {
            flags |= 1;
        }
        if ((attackerMon.StatusVolatile & VolatileTransformed) != 0)
        {
            flags |= 2;
        }
        W16(message, 0xE, flags);
        for (int i = 0; i < 4; i++)
        {
            BattleMon mon = battleCtx.BattleMons[i];
            W16(message, 0x18 + i * 2, mon.Species);
            W8(message, 0x24 + i, mon.IsShiny & 1);
            W8(message, 0x28 + i, mon.FormNum & 0x1F);
            W32(message, 0x3C + i * 4, mon.MoveEffectsMask);
            if ((mon.StatusVolatile & VolatileTransformed) != 0)
            {
                W8(message, 0x20 + i, mon.MoveEffectsData.TransformedGender);
                W32(message, 0x2C + i * 4, mon.MoveEffectsData.TransformedPID);
            }
            else
            {
                W8(message, 0x20 + i, mon.Gender & 0xF);
                W32(message, 0x2C + i * 4, mon.Personality);
            }
        }
    }
}
