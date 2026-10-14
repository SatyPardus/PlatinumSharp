namespace PlatinumSharp.Game;

public static partial class BattleSystems
{
    private const uint BattleStatusNoRecords = 0x10;
    private const int TrainerScoreBattle = 0x16;

    // OFFSET 0x0223F21C
    public static int BattleSystem_GetFieldWeather(BattleSystem battleSys)
    {
        return battleSys.FieldWeather;
    }

    // OFFSET 0x0223F228
    public static byte BattleSystem_GetCatchingTutorialLowHP(BattleSystem battleSys)
    {
        return battleSys.CatchingTutorialLowHP;
    }

    // OFFSET 0x0223F234
    public static void BattleSystem_SetCatchingTutorialLowHP(BattleSystem battleSys, byte value)
    {
        battleSys.CatchingTutorialLowHP = value;
    }

    // OFFSET 0x0223F240
    public static int BattleSystem_GetMetBebe(BattleSystem battleSys)
    {
        return battleSys.MetBebe;
    }

    // OFFSET 0x0223F24C
    public static void BattleSystem_IncrementRecord(BattleSystem battleSys, int record)
    {
        if ((battleSys.BattleStatusMask & BattleStatusNoRecords) == 0)
        {
            GameRecords.GameRecords_IncrementRecordValue(battleSys.Records!, record);
        }
    }

    // OFFSET 0x0223F268
    public static void BattleSystem_IncrementTrainerScore(BattleSystem battleSys)
    {
        if ((battleSys.BattleStatusMask & BattleStatusNoRecords) == 0)
        {
            GameRecords.GameRecords_IncrementTrainerScore(battleSys.Records!, TrainerScoreBattle);
        }
    }

    // OFFSET 0x0223F288
    public static void BattleSystem_SetCommandSelectionFlags(BattleSystem battleSys, int flags)
    {
        battleSys.CommandSelectionFlags = flags;
    }

    // OFFSET 0x0223F290
    public static void BattleSystem_SetUnread2440(BattleSystem battleSys, byte value)
    {
        battleSys.Unread2440 = value;
    }

    // OFFSET 0x0223F710
    public static int BattleSystem_RecordingStopped(BattleSystem battleSys)
    {
        return battleSys.RecordingStopped & 1;
    }
}

public static partial class BattleIO
{
    public const int CommandUpdatePartyMon = 0x27;
    public const int CommandPrintAlertMessage = 0x36;
    private const int UpdatePartyMonSize = 0x2C;
    private const uint StatusKeptMask = 0xFFFFF0FF;
    private const uint BattleStatus2FormChange = 0x4000000;
    private const uint BattleStatus2RecalcStats = 0x8000000;

    // OFFSET 0x022662FC
    public static void BattleIO_UpdatePartyMon(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        byte[] message = new byte[UpdatePartyMonSize];
        W8(message, 0, CommandUpdatePartyMon);
        W8(message, 1, (battleCtx.SelectedPartySlot[battler] & 0xF) | ((mon.MoveEffectsData.MimickedMoveSlot & 0xF) << 4));
        W16(message, 2, mon.CurHP);
        W16(message, 0xC, mon.HeldItem);
        W32(message, 8, (uint)battleCtx.SideConditions[BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler)].KnockedOffItemsMask);
        W16(message, 0x20, mon.FormNum);
        W32(message, 0x24, mon.Ability);
        for (int i = 0; i < 4; i++)
        {
            W16(message, 0xE + i * 2, mon.Moves[i]);
            W8(message, 0x16 + i, mon.PpCur[i]);
        }
        if ((short)mon.CurHP != 0)
        {
            W32(message, 4, mon.Status & StatusKeptMask);
        }
        else
        {
            W32(message, 4, 0);
        }
        W32(message, 0x1C, mon.StatusVolatile);
        if ((battleCtx.BattleStatusMask2 & BattleStatus2FormChange) != 0)
        {
            W16(message, 0x2A, 1);
            battleCtx.BattleStatusMask2 &= ~BattleStatus2FormChange;
        }
        else
        {
            W16(message, 0x2A, 0);
        }
        if ((battleCtx.BattleStatusMask2 & BattleStatus2RecalcStats) != 0)
        {
            W16(message, 0x28, 1);
            W16(message, 0x2A, 1);
            battleCtx.BattleStatusMask2 &= ~BattleStatus2RecalcStats;
        }
        else
        {
            W16(message, 0x28, 0);
        }
        BattleIO_Send(battleSys, RecipientBattler, battler, message, UpdatePartyMonSize);
    }

    // OFFSET 0x022666BC
    public static void BattleIO_PrintAlertMessage(BattleSystem battleSys, int battler, int tags, int messageID)
    {
        byte[] message = new byte[4];
        W8(message, 0, CommandPrintAlertMessage);
        W8(message, 1, tags);
        W16(message, 2, messageID);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }
}
