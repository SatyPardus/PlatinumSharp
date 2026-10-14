namespace PlatinumSharp.Game;

public sealed class SpriteToOAMTask
{
    public BattleSystem? BattleSys;
    public BattlerData? Battler;
    public PokemonSprite? MonSprite;
    public byte Command;
    public byte BattlerId;
    public byte State;
}

public static partial class Battlers
{
    private const int CommandIncrementRecord = 0x37;
    private const int RecordLinkData = 0x37;

    // OFFSET 0x0225C940
    private static void Battler_CommandIncrementRecord(BattleSystem battleSys, BattlerData battler)
    {
        byte[] data = battler.Data;
        if (data[1] == 0)
        {
            if (battler.BootState == 0)
            {
                BattleSystems.BattleSystem_IncrementRecord(battleSys, BattleIO.R16(data, 2));
            }
        }
        else if (battler.BootState != 0)
        {
            BattleSystems.BattleSystem_IncrementRecord(battleSys, BattleIO.R16(data, 2));
        }
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, data[0]);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C988
    private static void Battler_CommandLinkWaitMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleSystems.BattleSystem_ApplyRecordingData(battleSys, BattleIO.R16(battler.Data, 2), battler.Data, 4);
        BattleDisplay.BattleDisplay_PrintLinkWaitMessage(battleSys, battler);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C9AC
    private static void Battler_CommandRestoreSprite(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_RestoreSprite(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C9C0
    private static void Battler_CommandSpriteToOAM(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_SpriteToOAM(battleSys, battler);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C9D0
    private static void Battler_CommandOAMToSprite(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_OAMToSprite(battleSys, battler);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C9E0
    private static void Battler_CommandPrintResultMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PrintResultMessage(battleSys, battler);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C9F0
    private static void Battler_CommandPrintEscapeMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleSystems.BattleSystem_ApplyRecordingData(battleSys, BattleIO.R16(battler.Data, 2), battler.Data, 4);
        BattleDisplay.BattleDisplay_PrintEscapeMessage(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225CA14
    private static void Battler_CommandPrintForfeitMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleSystems.BattleSystem_ApplyRecordingData(battleSys, BattleIO.R16(battler.Data, 2), battler.Data, 4);
        BattleDisplay.BattleDisplay_PrintForfeitMessage(battleSys, battler);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225CA38
    private static void Battler_CommandRefreshSprite(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_RefreshSprite(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225CA4C
    private static void Battler_CommandPlayMoveHitSound(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PlayMoveHitSound(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225CA60
    private static void Battler_CommandPlayMusic(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PlayMusic(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225CA74
    private static void Battler_CommandSetLinkBattleResult(BattleSystem battleSys, BattlerData battler)
    {
        byte[] data = battler.Data;
        int playerHP = 0;
        int enemyHP = 0;
        BattleSystems.BattleSystem_ApplyRecordingData(battleSys, BattleIO.R16(data, 2), data, 8);
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & 0x80) != 0)
        {
            Battle.BattleSystem_SetResultMask(battleSys, (byte)BattleIO.R32(data, 4));
        }
        else
        {
            for (int battlerId = 0; battlerId < BattleSystems.BattleSystem_GetMaxBattlers(battleSys); battlerId++)
            {
                Party party = BattleSystems.BattleSystem_GetParty(battleSys, battlerId);
                for (int slot = 0; slot < Party.Party_GetCurrentCount(party); slot++)
                {
                    Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, slot);
                    if (Pokemon.Pokemon_GetValue(mon, 5, null) == 0 || Pokemon.Pokemon_GetValue(mon, 0x4C, null) != 0)
                    {
                        continue;
                    }
                    if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battlerId) != 0)
                    {
                        enemyHP += (int)Pokemon.Pokemon_GetValue(mon, 0xA3, null);
                    }
                    else
                    {
                        playerHP += (int)Pokemon.Pokemon_GetValue(mon, 0xA3, null);
                    }
                }
            }
            if (playerHP == 0 && enemyHP == 0)
            {
                Battle.BattleSystem_SetResultMask(battleSys, 3);
            }
            else if (playerHP == 0)
            {
                Battle.BattleSystem_SetResultMask(battleSys, 2);
            }
            else
            {
                Battle.BattleSystem_SetResultMask(battleSys, 1);
            }
        }
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, data[0]);
        Battler_ClearCommand(battler);
    }
}

public static partial class BattleDisplay
{
    private const int MsgLinkWait = 0x39B;
    private const int CommandLinkWaitMessage = 0x37;
    private const int CommandResultMessage = 0x3B;
    private const int CommandEscapeMessage = 0x3C;
    private const int CommandForfeitMessage = 0x3D;
    private const int SlideHeap = 5;
    private const int PanMoveHitEnemy = 0x75;
    private const ushort SeqMoveHitNormal = 0x6FE;
    private const ushort SeqMoveHitNotVeryEffective = 0x6FD;
    private const ushort SeqMoveHitSuperEffective = 0x6FC;
    private const int ResultTagsSingle = 8;
    private const int ResultTagsDouble = 0x1A;
    private const int BattleTypeFrontierMulti = 8;

    // OFFSET 0x0225E23C
    public static void BattleDisplay_PrintLinkWaitMessage(BattleSystem battleSys, BattlerData battler)
    {
        if (battler.BootState == 0)
        {
            MessageLoader msgLoader = BattleSystems.BattleSystem_GetMessageLoader(battleSys)!;
            BattleMessage message = new();
            message.Id = MsgLinkWait;
            message.Tags = 0;
            BattleMessages.BattleMessage_Print(battleSys, msgLoader, message, 0);
            Window window = BattleSystems.BattleSystem_GetWindow(battleSys, 0);
            BattleSystem_SetWaitDial(battleSys, RenderWindow.Window_AddWaitDial(window, 1));
        }
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, CommandLinkWaitMessage);
    }

    // OFFSET 0x0225E294
    public static void BattleDisplay_RestoreSprite(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        BattlerSpriteContext ctx = new();
        BattleDisplay_InitBattlerSpriteContext(battleSys, data, ctx, battler.Battler);
        BattlerSlide.BattlerSlide_StartSlideOut(ctx, SlideHeap);
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, data[0]);
    }

    // OFFSET 0x0225E2C8
    public static void BattleDisplay_SpriteToOAM(BattleSystem battleSys, BattlerData battler)
    {
        SpriteToOAMTask task = new();
        task.State = 0;
        task.BattleSys = battleSys;
        task.Battler = battler;
        task.Command = battler.Data[0];
        task.BattlerId = battler.Battler;
        task.MonSprite = battler.MonSprite;
        SysTask.SysTask_Start(BattleDisplay_SpriteToOAMTask, task, 0);
    }

    // OFFSET 0x0225E300
    public static void BattleDisplay_OAMToSprite(BattleSystem battleSys, BattlerData battler)
    {
        SpriteToOAMTask task = new();
        task.State = 0;
        task.BattleSys = battleSys;
        task.Battler = battler;
        task.Command = battler.Data[0];
        task.BattlerId = battler.Battler;
        task.MonSprite = battler.MonSprite;
        SysTask.SysTask_Start(BattleDisplay_OAMToSpriteTask, task, 0);
    }

    private static void BattleDisplay_PrintLoadedMessage(BattleSystem battleSys, BattlerData battler, BattleMessage message, int command)
    {
        MessageLoader msgLoader = BattleSystems.BattleSystem_GetMessageLoader(battleSys)!;
        MessageWaitTask task = new();
        task.BattleSys = battleSys;
        task.Command = (byte)command;
        task.Battler = battler.Battler;
        task.PrinterID = BattleMessages.BattleMessage_Print(battleSys, msgLoader, message, BattleMessages.BattleSystem_GetTextSpeed(battleSys));
        SysTask.SysTask_Start(BattleDisplay_MessageWaitTask, task, 0);
    }

    // OFFSET 0x0225E338
    public static void BattleDisplay_PrintResultMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleMessage message = new();
        BattleDisplay_LoadResultMessage(battleSys, battler, message);
        BattleDisplay_PrintLoadedMessage(battleSys, battler, message, CommandResultMessage);
    }

    // OFFSET 0x0225E38C
    public static void BattleDisplay_PrintEscapeMessage(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        BattleMessage message = new();
        BattleDisplay_LoadEscapeMessage(battleSys, battler, data, message);
        BattleDisplay_PrintLoadedMessage(battleSys, battler, message, CommandEscapeMessage);
    }

    // OFFSET 0x0225E3E0
    public static void BattleDisplay_PrintForfeitMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleMessage message = new();
        BattleDisplay_LoadForfeitMessage(battleSys, battler, message);
        BattleDisplay_PrintLoadedMessage(battleSys, battler, message, CommandForfeitMessage);
    }

    // OFFSET 0x0225E434
    public static void BattleDisplay_RefreshSprite(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        BattlerSpriteContext ctx = new();
        BattleDisplay_InitBattlerSpriteContext(battleSys, data, ctx, battler.Battler);
        BattlerSlide.BattlerSlide_StartPartnerSlideIn(ctx, SlideHeap);
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, data[0]);
    }

    // OFFSET 0x0225E468
    public static void BattleDisplay_PlayMoveHitSound(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        int pan = (battler.BattlerType & 1) != 0 ? PanMoveHitEnemy : -PanMoveHitEnemy;
        switch (data[1])
        {
            case 0:
                SoundPlayback.Sound_PlayPannedEffect(SeqMoveHitNormal, pan);
                break;
            case 1:
                SoundPlayback.Sound_PlayPannedEffect(SeqMoveHitNotVeryEffective, pan);
                break;
            case 2:
                SoundPlayback.Sound_PlayPannedEffect(SeqMoveHitSuperEffective, pan);
                break;
        }
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, data[0]);
    }

    // OFFSET 0x0225E4C8
    public static void BattleDisplay_PlayMusic(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        SoundPlayback.Sound_PlayBGM((ushort)BattleIO.R16(data, 2));
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, data[0]);
    }

    // OFFSET 0x02263688
    private static void BattleDisplay_SpriteToOAMTask(SysTask sysTask, object? param)
    {
        SpriteToOAMTask task = (SpriteToOAMTask)param!;
        if (task.State == 0)
        {
            if (task.Battler!.BtlMonObjData != null)
            {
                ErrorHandling.GF_AssertFail();
            }
            task.Battler.BtlMonObjData = BattlerObjSpritesFuncs.BattlerObjSprites_New(task.BattleSys!, SlideHeap, task.BattlerId);
            task.State++;
            return;
        }
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.BattlerId, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x022636D4
    private static void BattleDisplay_OAMToSpriteTask(SysTask sysTask, object? param)
    {
        SpriteToOAMTask task = (SpriteToOAMTask)param!;
        if (task.Battler!.BtlMonObjData == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        BattlerObjSprites sprites = (BattlerObjSprites)task.Battler.BtlMonObjData!;
        if (BattlerObjSpritesFuncs.BattlerObjSprites_GetBattlerType(sprites) == 3 && task.State < 5)
        {
            task.State++;
            return;
        }
        BattlerObjSpritesFuncs.BattlerObjSprites_Delete(sprites, task.BattlerId);
        task.Battler.BtlMonObjData = null;
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.BattlerId, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x022641B4
    private static void BattleDisplay_LoadResultMessage(BattleSystem battleSys, BattlerData battler, BattleMessage message)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int result = Battle.BattleSystem_GetResultMask(battleSys);
        int first;
        int second;
        if ((battleType & BattleTypeDoubles) != 0)
        {
            first = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 3);
            second = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 5);
        }
        else
        {
            first = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 1);
            second = first;
        }
        ushort doubleID;
        ushort singleID;
        switch (result)
        {
            case 1:
                doubleID = 0x312;
                singleID = 0x311;
                break;
            case 2:
                doubleID = 0x314;
                singleID = 0x313;
                break;
            case 3:
                doubleID = 0x316;
                singleID = 0x315;
                break;
            default:
                return;
        }
        if ((battleType & BattleTypeFrontierMulti) != 0)
        {
            message.Id = doubleID;
            message.Tags = ResultTagsDouble;
            message.Params[0] = first;
            message.Params[1] = second;
        }
        else
        {
            message.Id = singleID;
            message.Tags = ResultTagsSingle;
            message.Params[0] = first;
        }
    }

    // OFFSET 0x02264270
    private static void BattleDisplay_LoadEscapeMessage(BattleSystem battleSys, BattlerData battler, byte[] data, BattleMessage message)
    {
        int playerEscaped = 0;
        int enemyEscaped = 0;
        for (int battlerId = 0; battlerId < BattleSystems.BattleSystem_GetMaxBattlers(battleSys); battlerId++)
        {
            if ((HealthBar.FlagIndex(battlerId) & data[1]) == 0)
            {
                continue;
            }
            if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battlerId) != 0)
            {
                enemyEscaped++;
            }
            else
            {
                playerEscaped++;
            }
        }
        if (playerEscaped != 0 && enemyEscaped != 0)
        {
            message.Id = 0x30D;
            message.Tags = 0;
            Battle.BattleSystem_SetResultMask(battleSys, 0xC3);
            return;
        }
        if (playerEscaped != 0)
        {
            message.Id = 0x30D;
            message.Tags = 0;
            Battle.BattleSystem_SetResultMask(battleSys, 0xC2);
            return;
        }
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeFrontierMulti) != 0)
        {
            message.Id = 0x318;
            message.Tags = ResultTagsDouble;
            message.Params[0] = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 3);
            message.Params[1] = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 5);
        }
        else if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) != 0)
        {
            message.Id = 0x317;
            message.Tags = ResultTagsSingle;
            message.Params[0] = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 3);
        }
        else
        {
            message.Id = 0x317;
            message.Tags = ResultTagsSingle;
            message.Params[0] = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 1);
        }
        Battle.BattleSystem_SetResultMask(battleSys, 0xC1);
    }

    // OFFSET 0x02264360
    private static void BattleDisplay_LoadForfeitMessage(BattleSystem battleSys, BattlerData battler, BattleMessage message)
    {
        message.Id = 0x3BC;
        message.Tags = ResultTagsSingle;
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeLinkMask) != 0)
        {
            ushort networkID = BattleSystems.BattleSystem_GetNetworkID(battleSys);
            if (BattleSystems.BattleSystem_GetLinkPlayerPosition(battleSys, networkID) != 0)
            {
                message.Params[0] = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 4);
            }
            else
            {
                message.Params[0] = BattleSystems.BattleSystem_GetBattlerOfType(battleSys, 2);
            }
        }
        else
        {
            message.Params[0] = battler.Battler;
        }
        Battle.BattleSystem_SetResultMask(battleSys, 0xC2);
    }
}

public static partial class BattleSystems
{
    // OFFSET 0x0223F6D4
    private static byte BattleSystem_ReadRecordingByte(byte[] data, int baseOffset, ref byte position, ref ushort remaining)
    {
        remaining--;
        byte value = data[baseOffset + position];
        position++;
        return value;
    }

    // OFFSET 0x0223F638
    public static void BattleSystem_ApplyRecordingData(BattleSystem battleSys, int size, byte[] data, int offset)
    {
        byte position = 0;
        ushort remaining = (ushort)size;
        if ((battleSys.BattleType & 4) == 0 || battleSys.BattleInitialized != 0 || remaining == 0)
        {
            return;
        }
        do
        {
            byte battler = BattleSystem_ReadRecordingByte(data, offset, ref position, ref remaining);
            byte count = BattleSystem_ReadRecordingByte(data, offset, ref position, ref remaining);
            for (int i = 0; i < count; i++)
            {
                byte command = BattleSystem_ReadRecordingByte(data, offset, ref position, ref remaining);
                BattleRecording.BattleRecording_WriteCommand(battler, i + battleSys.RecordingWritePos[battler], command);
            }
            battleSys.RecordingWritePos[battler] = (ushort)(battleSys.RecordingWritePos[battler] + count);
        }
        while (remaining != 0);
    }

    // OFFSET 0x0223F6F0
    public static int BattleSystem_GetLinkPlayerPosition(BattleSystem battleSys, int networkID)
    {
        return battleSys.LinkPlayerPositions[networkID];
    }
}

public static partial class BattlerObjSpritesFuncs
{
    // OFFSET 0x022237A4
    public static void BattlerObjSprites_Delete(BattlerObjSprites sprites, int battler)
    {
        for (int i = 0; i < BattlerObjSprites.MaxBattlers; i++)
        {
            if (sprites.Sprites[i] != null)
            {
                SpriteSystem.SpriteManager_UnloadCharObjById(sprites.SpriteMan!, sprites.ResourceIDs[i]);
                SpriteSystem.SpriteManager_UnloadPlttObjById(sprites.SpriteMan!, sprites.ResourceIDs[i]);
                SpriteSystem.Sprite_DeleteAndFreeResources(sprites.Sprites[i]!);
            }
        }
    }

    // OFFSET 0x022237D8
    public static int BattlerObjSprites_GetBattlerType(BattlerObjSprites sprites)
    {
        if (sprites == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return sprites!.BattlerTypes[sprites.Battler];
    }
}
