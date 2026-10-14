namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int StatStageMax = 12;
    private const int SideEffectSharplyRaiseBase = 0x27;
    private const uint SideEffectDirectFlagSet = 0x80000000;
    private const int AbilityNaturalCure = 0x1E;
    private const int MonDataAbility = 0xA;
    private const int MonDataStatus = 0xA0;
    private const int NoPartySlot = 6;
    private const uint SelfTurnSubstituteHit = 8;
    private const uint MoveStatusHeldOn = 0x100;

    // OFFSET 0x022484D0
    private static int BattleScript_BoostRandomStatBy2(BattleSystem battleSys, BattleContext battleCtx)
    {
        int[] stats = new int[8];
        int count = 0;
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        BattleMon defender = battleCtx.BattleMons[battleCtx.Defender];
        for (int stat = 1; stat < 8; stat++)
        {
            if (defender.StatBoosts[stat] < StatStageMax)
            {
                stats[count] = stat - 1;
                count++;
            }
        }
        if (count != 0)
        {
            int index = BattleSystems.BattleSystem_RandNext(battleSys) % count;
            battleCtx.SideEffectDirectFlags = (uint)(stats[index] + SideEffectSharplyRaiseBase);
            battleCtx.SideEffectDirectFlags |= SideEffectDirectFlagSet;
        }
        else
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x02248550
    private static int BattleScript_RemoveItem(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        battleCtx.RecycleItem[battler] = battleCtx.BattleMons[battler].HeldItem;
        battleCtx.BattleMons[battler].HeldItem = 0;
        BattleContexts.BattleMon_CopyToParty(battleSys, battleCtx, battler);
        return 0;
    }

    // OFFSET 0x0224859C
    private static int BattleScript_TryRecycle(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        ushort item = battleCtx.RecycleItem[battleCtx.Attacker];
        if (item != 0)
        {
            battleCtx.MsgItemTemp = item;
            battleCtx.RecycleItem[battleCtx.Attacker] = 0;
        }
        else
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x022485E0
    private static int BattleScript_TriggerHeldItemOnHit(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpIfNone = BattleScript_Read(battleCtx);
        if (BattleContexts.BattleSystem_TriggerHeldItemOnHit(battleSys, battleCtx, ref battleCtx.ScriptTemp) == 0)
        {
            BattleScript_Iter(battleCtx, jumpIfNone);
        }
        return 0;
    }

    // OFFSET 0x02248614
    private static int BattleScript_PrintBattleResultMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleIO.BattleIO_PrintBattleResultMessage(battleSys);
        return 0;
    }

    // OFFSET 0x0224862C
    private static int BattleScript_PrintEscapeMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleIO.BattleIO_PrintEscapeMessage(battleSys, battleCtx);
        return 0;
    }

    // OFFSET 0x02248648
    private static int BattleScript_PrintForfeitMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleIO.BattleIO_PrintForfeitMessage(battleSys);
        return 0;
    }

    // OFFSET 0x02248660
    private static int BattleScript_CheckHoldOnWith1HP(BattleSystem battleSys, BattleContext battleCtx)
    {
        int holdOn = 0;
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        int itemEffect = BattleContexts.Battler_HeldItemEffect(battleCtx, battler);
        int itemPower = BattleContexts.Battler_HeldItemPower(battleCtx, battler, 0);
        if (itemEffect == 0x41 && BattleSystems.BattleSystem_RandNext(battleSys) % 100 < itemPower)
        {
            holdOn = 1;
        }
        if (itemEffect == 0x67 && battleCtx.BattleMons[battler].CurHP == battleCtx.BattleMons[battler].MaxHP)
        {
            holdOn = 1;
        }
        if (holdOn != 0)
        {
            int curHP = battleCtx.BattleMons[battler].CurHP;
            if (battleCtx.HpCalcTemp + curHP <= 0)
            {
                battleCtx.HpCalcTemp = -(curHP - 1);
                battleCtx.MoveStatusFlags |= MoveStatusHeldOn;
            }
        }
        return 0;
    }

    // OFFSET 0x02248708
    private static int BattleScript_TryRestoreStatusOnSwitch(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        if (battleCtx.BattleMons[battler].CurHP == 0 || battleCtx.SelectedPartySlot[battler] == NoPartySlot)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, battleCtx.SelectedPartySlot[battler]);
        int ability = (int)Pokemon.Pokemon_GetValue(mon, MonDataAbility, null);
        uint status = Pokemon.Pokemon_GetValue(mon, MonDataStatus, null);
        if (battleCtx.BattleMons[battler].Ability != AbilityNaturalCure && BattleContexts.Ability_ForbidsStatus(battleCtx, ability, status) == 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x022487A4
    private static int BattleScript_CheckSubstitute(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int jumpIfSubstitute = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        if ((battleCtx.BattleMons[battler].StatusVolatile & VolatileSubstitute) != 0 || (battleCtx.SelfTurnFlags[battler].StatusFlags & SelfTurnSubstituteHit) != 0)
        {
            BattleScript_Iter(battleCtx, jumpIfSubstitute);
        }
        return 0;
    }
}

public static partial class BattleIO
{
    private const int CommandPrintBattleResultMessage = 0x3B;
    private const int CommandPrintEscapeMessage = 0x3C;
    private const int CommandPrintForfeitMessage = 0x3D;
    private const int EndMessageSize = 0x20;
    private const uint EscapeAction = 0x10;
    private const int EndRecordingMax = 0x1C;

    // OFFSET 0x02266820
    public static void BattleIO_PrintBattleResultMessage(BattleSystem battleSys)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandPrintBattleResultMessage);
        BattleIO_Send(battleSys, RecipientBattler, 0, message, 4);
    }

    private static void BattleIO_AttachRecording(BattleSystem battleSys, uint battleType, byte[] message)
    {
        if ((battleType & 4) != 0 && BattleRecording.BattleRecording_Exists() == 1 && (battleSys.BattleStatusMask & 0x10) == 0)
        {
            W16(message, 2, BattleSystems.BattleSystem_CollectRecordingData(battleSys, message, 4));
            if ((ushort)Bin16(message, 2) >= EndRecordingMax)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
    }

    // OFFSET 0x0226683C
    public static void BattleIO_PrintEscapeMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        byte[] message = new byte[0x24];
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        W8(message, 0, CommandPrintEscapeMessage);
        W8(message, 1, 0);
        W16(message, 2, 0);
        for (int battler = 0; battler < BattleSystems.BattleSystem_GetMaxBattlers(battleSys); battler++)
        {
            if (battleCtx.BattlerActions[battler, 0] == EscapeAction)
            {
                W8(message, 1, (int)(HealthBar.FlagIndex(battler) | message[1]));
            }
        }
        BattleIO_AttachRecording(battleSys, battleType, message);
        BattleIO_Send(battleSys, RecipientBattler, 0, message, EndMessageSize);
    }

    // OFFSET 0x022668D0
    public static void BattleIO_PrintForfeitMessage(BattleSystem battleSys)
    {
        byte[] message = new byte[0x24];
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        W8(message, 0, CommandPrintForfeitMessage);
        W16(message, 2, 0);
        BattleIO_AttachRecording(battleSys, battleType, message);
        BattleIO_Send(battleSys, RecipientBattler, 0, message, EndMessageSize);
    }
}
