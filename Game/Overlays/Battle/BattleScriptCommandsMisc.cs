namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const uint BattleStatusNoAttackMessage = 1;
    private const uint BattleStatusMoveAnimationsOff = 0x4000;
    private const uint BattleTypeDoubles = 2;
    private const uint BattleTypeTag = 0x10;
    private const uint BattleTypeFrontierDoubles = 0x4B;
    private const int NarcSubSeq = 1;
    private const int SubseqNoTarget = 0x119;
    private const int NarcMoveSeq = 0;
    private const int ResultWin = 1;
    private const int StatusEffectSleep = 0xF;
    private const int StatusEffectPoison = 0x10;
    private const int StatusEffectFirstAlways = 0x19;
    private const int TrainerDataSize = 0x34;
    private const int TrainerPartySize = 0x6C;
    private const uint PrizeMoneyTableAddress = 0x02270B20;
    private const int BattleOverlay = 16;

    // OFFSET 0x02242F8C
    private static int BattleScript_SetMirrorMove(BattleSystem battleSys, BattleContext battleCtx)
    {
        int move = 0;
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int attacker = battleCtx.Attacker;
        if (battleCtx.MoveCopied[attacker] != 0)
        {
            move = battleCtx.MoveCopied[attacker];
        }
        else if ((battleType & BattleTypeDoubles) != 0)
        {
            move = battleCtx.MoveCopiedHit[attacker, 3] + battleCtx.MoveCopiedHit[attacker, 2] + battleCtx.MoveCopiedHit[attacker, 0] + battleCtx.MoveCopiedHit[attacker, 1];
            if (move != 0)
            {
                do
                {
                    int index = BattleSystems.BattleSystem_RandNext(battleSys) % 4;
                    move = battleCtx.MoveCopiedHit[battleCtx.Attacker, index];
                }
                while (move == 0);
            }
        }
        if (move != 0 && BattleContexts.Move_CanBeEncored(battleCtx, (ushort)move) == 1)
        {
            battleCtx.BattleStatusMask &= ~BattleStatusNoAttackMessage;
            battleCtx.BattleStatusMask &= ~BattleStatusMoveAnimationsOff;
            battleCtx.MoveCur = (uint)move;
            battleCtx.Defender = BattleContexts.BattleSystem_Defender(battleSys, battleCtx, battleCtx.Attacker, (ushort)move, 1, 0);
            if (battleCtx.Defender == 0xFF)
            {
                battleCtx.CommandNext = 0x26;
                BattleScript_Jump(battleCtx, NarcSubSeq, SubseqNoTarget);
            }
            else
            {
                battleCtx.BattlerActions[battleCtx.Attacker, 1] = (uint)battleCtx.Defender;
                BattleScript_Jump(battleCtx, NarcMoveSeq, move);
            }
        }
        else
        {
            battleCtx.SelfTurnFlags[battleCtx.Attacker].SkipPressureCheck = 1;
        }
        return 0;
    }

    // OFFSET 0x022430F4
    private static int BattleScript_LockMoveChoice(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleContexts.Battler_LockMoveChoice(battleSys, battleCtx, battler);
        return 0;
    }

    // OFFSET 0x02243120
    private static int BattleScript_UnlockMoveChoice(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleContexts.Battler_UnlockMoveChoice(battleSys, battleCtx, battler);
        return 0;
    }

    // OFFSET 0x0224314C
    private static int BattleScript_SetHealthBoxStatusIcon(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int status = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        BattleIO.BattleIO_SetStatusIcon(battleSys, battler, status);
        return 0;
    }

    // OFFSET 0x02243184
    private static int BattleScript_PrintTrainerMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int msg = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        BattleIO.BattleIO_PrintTrainerMessage(battleSys, battler, msg);
        return 0;
    }

    // OFFSET 0x022431BC
    private static int BattleScript_CalcPrizeMoney(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        byte[] party = new byte[TrainerPartySize];
        byte[] trainer = new byte[TrainerDataSize];
        int trainerID = battleSys.TrainerIDs[battler];
        TrainerData.Trainer_Load(trainerID, trainer);
        TrainerData.Trainer_LoadParty(battleSys.TrainerIDs[battler], party);
        int partyType = trainer[0];
        int lastMon = trainer[3] - 1;
        int level = 0;
        switch (partyType)
        {
            case 0:
                level = (byte)PlatinumSharp.Nitro.Bin.U16(party, lastMon * 8 + 2);
                break;
            case 1:
                level = (byte)PlatinumSharp.Nitro.Bin.U16(party, lastMon * 0x10 + 2);
                break;
            case 2:
                level = (byte)PlatinumSharp.Nitro.Bin.U16(party, lastMon * 0xA + 2);
                break;
            case 3:
                level = (byte)PlatinumSharp.Nitro.Bin.U16(party, lastMon * 0x12 + 2);
                break;
        }
        int classMultiplier = RomData.Overlay(BattleOverlay, (uint)(PrizeMoneyTableAddress + trainer[1]), 1)[0];
        int money;
        uint battleType = battleSys.BattleType;
        if ((battleType & BattleTypeTag) != 0 || battleType == BattleTypeFrontierDoubles)
        {
            money = classMultiplier * (level * 4 * battleCtx.PrizeMoneyMul);
        }
        else if ((battleType & BattleTypeDoubles) != 0)
        {
            money = classMultiplier * (level * 4 * battleCtx.PrizeMoneyMul * 2);
        }
        else
        {
            money = classMultiplier * (level * 4 * battleCtx.PrizeMoneyMul);
        }
        return money;
    }

    // OFFSET 0x022432B4
    private static int BattleScript_PayPrizeMoney(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int money;
        if (battleSys.ResultMask == ResultWin)
        {
            money = BattleScript_CalcPrizeMoney(battleSys, battleCtx, 1);
            uint battleType = battleSys.BattleType;
            if ((battleType & BattleTypeTag) != 0 || battleType == BattleTypeFrontierDoubles)
            {
                money += BattleScript_CalcPrizeMoney(battleSys, battleCtx, 3);
            }
            TrainerInfo.TrainerInfo_GiveMoney(BattleSystems.BattleSystem_GetTrainerInfo(battleSys, 0), (uint)money);
        }
        else
        {
            money = (int)BattleSystems.BattleSystem_CalcMoneyPenalty(battleSys.Parties[0]!, battleSys.TrainerInfos[0]!);
            TrainerInfo.TrainerInfo_TakeMoney(BattleSystems.BattleSystem_GetTrainerInfo(battleSys, 0), (uint)money);
        }
        battleCtx.MsgTemp = money != 0 ? money : 0;
        return 0;
    }

    private static bool BattleScript_ShouldPlayStatusEffect(BattleSystem battleSys, int checkValue, int effect)
    {
        return BattleSystems.BattleSystem_AreAnimationsOn(battleSys) == 1 || checkValue == StatusEffectSleep || checkValue == StatusEffectPoison || (uint)(effect - StatusEffectFirstAlways) <= 1;
    }

    // OFFSET 0x02243334
    private static int BattleScript_PlayBattleAnimation(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int effect = BattleScript_Read(battleCtx);
        if (BattleScript_ShouldPlayStatusEffect(battleSys, effect, effect))
        {
            int battler = BattleScript_Battler(battleSys, battleCtx, selector);
            if (BattleContexts.Battler_CanShowStatusEffect(battleCtx, battler, effect) == 1)
            {
                BattleIO.BattleIO_PlayStatusEffect(battleSys, battleCtx, battler, effect);
            }
        }
        return 0;
    }

    // OFFSET 0x02243398
    private static int BattleScript_PlayBattleAnimationOnMons(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int attackerSelector = BattleScript_Read(battleCtx);
        int defenderSelector = BattleScript_Read(battleCtx);
        int effect = BattleScript_Read(battleCtx);
        if (BattleScript_ShouldPlayStatusEffect(battleSys, effect, effect))
        {
            int attacker = BattleScript_Battler(battleSys, battleCtx, attackerSelector);
            int defender = BattleScript_Battler(battleSys, battleCtx, defenderSelector);
            if (BattleContexts.Battler_CanShowStatusEffect(battleCtx, attacker, effect) == 1 && BattleContexts.Battler_CanShowStatusEffect(battleCtx, defender, effect) == 1)
            {
                BattleIO.BattleIO_PlayStatusEffectAToD(battleSys, battleCtx, attacker, defender, effect);
            }
        }
        return 0;
    }

    // OFFSET 0x02243424
    private static int BattleScript_PlayBattleAnimationFromVar(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int var = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        ref int effect = ref BattleScript_VarAddress(battleSys, battleCtx, var);
        if (BattleScript_ShouldPlayStatusEffect(battleSys, var, effect))
        {
            if (BattleContexts.Battler_CanShowStatusEffect(battleCtx, battler, effect) == 1)
            {
                BattleIO.BattleIO_PlayStatusEffect(battleSys, battleCtx, battler, effect);
            }
        }
        return 0;
    }

    // OFFSET 0x02243494
    private static int BattleScript_PrintRecallMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_PrintRecallMessage(battleSys, battleCtx, battler, battleCtx.SelectedPartySlot[battler]);
        return 0;
    }

    // OFFSET 0x022434CC
    private static int BattleScript_PrintSendOutHPMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_PrintSendOutMessage(battleSys, battleCtx, battler, battleCtx.SelectedPartySlot[battler]);
        return 0;
    }

    // OFFSET 0x0224355C
    private static int BattleScript_PrintBufferedTrainerMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_PrintTrainerMessage(battleSys, battler, battleCtx.MsgTemp);
        return 0;
    }
}

public static partial class BattleSystems
{
    private const uint BadgePenaltyTableAddress = 0x0226E564;

    // OFFSET 0x0223F904
    public static uint BattleSystem_CalcMoneyPenalty(Party party, TrainerInfo trainerInfo)
    {
        byte badges = (byte)TrainerInfo.TrainerInfo_BadgeCount(trainerInfo);
        int maxLevel = Pokemon.Party_GetMaxLevel(party);
        uint penalty = (uint)(RomData.Overlay(16, (uint)(BadgePenaltyTableAddress + badges), 1)[0] * (maxLevel * 4));
        uint money = TrainerInfo.TrainerInfo_Money(trainerInfo);
        if (penalty > money)
        {
            penalty = money;
        }
        return penalty;
    }
}

public sealed partial class Pokemon
{
    private const int ParamIsEggValue = 0x4C;

    // OFFSET 0x02077550
    public static int Party_GetMaxLevel(Party party)
    {
        int count = Party.Party_GetCurrentCount(party);
        int maxLevel = 1;
        for (int slot = 0; slot < count; slot++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, slot);
            if (Pokemon_GetValue(mon, ParamSpecies, null) != 0 && Pokemon_GetValue(mon, ParamIsEggValue, null) == 0)
            {
                byte level = (byte)Pokemon_GetValue(mon, ParamLevel, null);
                if (level > maxLevel)
                {
                    maxLevel = level;
                }
            }
        }
        return maxLevel;
    }
}

public static partial class BattleIO
{
    private const int CommandSetStatusIcon = 0x1E;
    private const int CommandPrintTrainerMessage = 0x1F;
    private const int CommandPrintRecallMessage = 0x20;
    private const int CommandPrintSendOutMessage = 0x21;

    // OFFSET 0x02265FB8
    public static void BattleIO_SetStatusIcon(BattleSystem battleSys, int battler, int status)
    {
        byte[] message = new byte[4];
        W8(message, 0, CommandSetStatusIcon);
        W8(message, 1, status);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x02265FD8
    public static void BattleIO_PrintTrainerMessage(BattleSystem battleSys, int battler, int msg)
    {
        byte[] message = new byte[4];
        W8(message, 0, CommandPrintTrainerMessage);
        W8(message, 1, msg);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x02265FF8
    public static void BattleIO_PlayStatusEffect(BattleSystem battleSys, BattleContext battleCtx, int battler, int effect)
    {
        byte[] message = new byte[MoveAnimationSize];
        BattleIO_SetMoveAnimation(battleSys, battleCtx, message, 1, effect, battler, battler, 0);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, MoveAnimationSize);
    }

    // OFFSET 0x02266028
    public static void BattleIO_PlayStatusEffectAToD(BattleSystem battleSys, BattleContext battleCtx, int attacker, int defender, int effect)
    {
        byte[] message = new byte[MoveAnimationSize];
        BattleIO_SetMoveAnimation(battleSys, battleCtx, message, 1, effect, attacker, defender, 0);
        BattleIO_Send(battleSys, RecipientBattler, attacker, message, MoveAnimationSize);
    }

    // OFFSET 0x02266058
    public static void BattleIO_PrintRecallMessage(BattleSystem battleSys, BattleContext battleCtx, int battler, int partySlot)
    {
        byte[] message = new byte[4];
        W8(message, 0, CommandPrintRecallMessage);
        W8(message, 1, partySlot);
        int hpTemp = battleCtx.HpTemp;
        W16(message, 2, (hpTemp - battleCtx.BattleMons[1].CurHP) * 100 / hpTemp);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x0226609C
    public static void BattleIO_PrintSendOutMessage(BattleSystem battleSys, BattleContext battleCtx, int battler, int partySlot)
    {
        byte[] message = new byte[4];
        W8(message, 0, CommandPrintSendOutMessage);
        W8(message, 1, partySlot);
        if (battleCtx.BattleMons[1].CurHP == 0)
        {
            W16(message, 2, 1000);
        }
        else
        {
            W16(message, 2, (int)((uint)(battleCtx.BattleMons[1].CurHP * 1000) / (uint)battleCtx.BattleMons[1].MaxHP));
        }
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }
}
