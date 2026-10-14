namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const uint BattleTypeLink = 4;
    private const int TypeRockStealth = 5;
    private const int AbilitySereneGrace = 0x20;
    private const int SpeciesChatot = 0x1B9;
    private const uint BattleStatusRecording = 0x10;
    private const int PartyMenuSwitchSlot = 6;

    // OFFSET 0x02247C04
    private static int BattleScript_WaitYesNoResult(BattleSystem battleSys, BattleContext battleCtx)
    {
        int result = BattleContexts.BattleContext_IOBufferVal(battleCtx, 0);
        if (result != 0)
        {
            BattleScript_Iter(battleCtx, 1);
            int jumpOnYes = BattleScript_Read(battleCtx);
            int jumpOnNo = BattleScript_Read(battleCtx);
            if (result == 0xFF)
            {
                BattleScript_Iter(battleCtx, jumpOnNo);
            }
            else
            {
                BattleScript_Iter(battleCtx, jumpOnYes);
            }
            BattleSystems.BattleSystem_Record(battleSys, 0, (byte)result);
        }
        battleCtx.BattleProgressFlag = 1;
        return 0;
    }

    // OFFSET 0x02247C64
    private static int BattleScript_ChoosePokemonMenu(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        BattleIO.BattleIO_ShowPartyMenu(battleSys, battleCtx, 0, 0, 0, PartyMenuSwitchSlot);
        battleCtx.SwitchedMon = 0;
        return 0;
    }

    // OFFSET 0x02247C94
    private static int BattleScript_WaitPokemonMenuResult(BattleSystem battleSys, BattleContext battleCtx)
    {
        int result = BattleContexts.BattleContext_IOBufferVal(battleCtx, 0);
        if (result != 0)
        {
            BattleScript_Iter(battleCtx, 1);
            int jumpOnCancel = BattleScript_Read(battleCtx);
            if (result == 0xFF)
            {
                BattleScript_Iter(battleCtx, jumpOnCancel);
            }
            else
            {
                battleCtx.SwitchedPartySlot[0] = (byte)(result - 1);
            }
        }
        battleCtx.BattleProgressFlag = 1;
        return 0;
    }

    // OFFSET 0x02247CE0
    private static int BattleScript_SetLinkBattleResult(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeLink) != 0)
        {
            BattleIO.BattleIO_SetLinkBattleResult(battleSys);
        }
        return 0;
    }

    // OFFSET 0x02247D04
    private static int BattleScript_CheckStealthRock(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int jumpIfNone = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler);
        int type1 = BattleContexts.BattleMon_Get(battleCtx, battler, BattleMonType1, null);
        int type2 = BattleContexts.BattleMon_Get(battleCtx, battler, BattleMonType2, null);
        if ((battleCtx.SideConditionsMask[side] & SideConditionStealthRock) == 0 || battleCtx.BattleMons[battler].CurHP == 0)
        {
            BattleScript_Iter(battleCtx, jumpIfNone);
            return 0;
        }
        switch (BattleContexts.BattleSystem_TypeMatchupMultiplier(TypeRockStealth, (byte)type1, (byte)type2))
        {
            case 0xA0:
                battleCtx.HpCalcTemp = 2;
                break;
            case 0x50:
                battleCtx.HpCalcTemp = 4;
                break;
            case 0x28:
                battleCtx.HpCalcTemp = 8;
                break;
            case 0x14:
                battleCtx.HpCalcTemp = 16;
                break;
            case 0xA:
                battleCtx.HpCalcTemp = 32;
                break;
            case 0:
                BattleScript_Iter(battleCtx, jumpIfNone);
                return 0;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        battleCtx.HpCalcTemp = BattleContexts.BattleSystem_Divide((int)battleCtx.BattleMons[battler].MaxHP * -1, battleCtx.HpCalcTemp);
        return 0;
    }

    // OFFSET 0x02247E10
    private static int BattleScript_CheckEffectActivation(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int chance;
        if (BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) == AbilitySereneGrace)
        {
            chance = (ushort)(battleCtx.AiContext.MoveTable[battleCtx.MoveCur].EffectChance * 2);
        }
        else
        {
            chance = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].EffectChance;
        }
        if (chance == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (BattleSystems.BattleSystem_RandNext(battleSys) % 100 < chance && battleCtx.BattleMons[battleCtx.SideEffectMon].CurHP != 0)
        {
            return 0;
        }
        BattleScript_Iter(battleCtx, jumpOnFail);
        return 0;
    }

    // OFFSET 0x02247E98
    private static int BattleScript_CheckChatterActivation(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        BattleMon attacker = battleCtx.BattleMons[battleCtx.Attacker];
        if (attacker.Species != SpeciesChatot || battleCtx.BattleMons[battleCtx.Defender].CurHP == 0 || (attacker.StatusVolatile & VolatileTransformed) != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        int param;
        if ((BattleSystems.BattleSystem_GetBattleStatusMask(battleSys) & BattleStatusRecording) == 0)
        {
            param = SoundChatot.Sound_GetChatterActivationParameter(BattleDisplay.BattleSystem_GetChatotCry(battleSys, battleCtx.Attacker));
        }
        else
        {
            param = BattleSystems.BattleSystem_GetRecordedChatter(battleSys, battleCtx.Attacker);
        }
        int chance;
        switch (param)
        {
            case 1:
                chance = 10;
                break;
            case 2:
                chance = 30;
                break;
            default:
                chance = 0;
                break;
        }
        if (BattleSystems.BattleSystem_RandNext(battleSys) % 100 > chance)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x02247F44
    private static int BattleScript_GetCurrentMoveData(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int param = BattleScript_Read(battleCtx);
        battleCtx.CalcTemp = (int)MoveTable.MoveTable_GetParam(battleCtx.AiContext.MoveTable[battleCtx.MoveCur].ToBytes(), param);
        return 0;
    }

    // OFFSET 0x02247F7C
    private static int BattleScript_SetMosaic(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int param = BattleScript_Read(battleCtx);
        int wait = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        BattleIO.BattleIO_SetMosaic(battleSys, battler, param, wait);
        return 0;
    }

    // OFFSET 0x02247FBC
    private static int BattleScript_ChangeForm(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_ChangeWeatherForm(battleSys, battler);
        return 0;
    }

    // OFFSET 0x02248000
    private static int BattleScript_UseBagItem(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleSystems.BattleSystem_UseBagItem(battleSys, battler, battleCtx.SelectedPartySlot[battler], 0, battleCtx.MsgItemTemp);
        return 0;
    }
}

public static partial class BattleSystems
{
    // OFFSET 0x0223F810
    public static int BattleSystem_GetRecordedChatter(BattleSystem battleSys, int battler)
    {
        uint battleType = battleSys.BattleType;
        if ((battleType & 8) != 0 || ((battleType & 0x10) != 0 && (BattleSystem_BattlerSlot(battleSys, battler) & 1) != 0))
        {
            return battleSys.RecordedChatter[battler];
        }
        if ((battleSys.BattleType & 2) != 0)
        {
            return battleSys.RecordedChatter[battler & 1];
        }
        return battleSys.RecordedChatter[battler];
    }

    // OFFSET 0x0223F58C
    public static int BattleSystem_CollectRecordingData(BattleSystem battleSys, byte[] data, int offset)
    {
        byte count = 0;
        for (byte battler = 0; battler < battleSys.MaxBattlers; battler++)
        {
            if (battleSys.RecordingAckPos[battler] == battleSys.RecordingCollectedPos[battler])
            {
                continue;
            }
            data[offset + count] = battler;
            count++;
            data[offset + count] = (byte)(battleSys.RecordingAckPos[battler] - battleSys.RecordingCollectedPos[battler]);
            count++;
            for (byte i = 0; i < battleSys.RecordingAckPos[battler] - battleSys.RecordingCollectedPos[battler]; i++)
            {
                data[offset + count] = BattleRecording.BattleRecording_ReadCommand(battler, i + battleSys.RecordingCollectedPos[battler]);
                count++;
            }
            battleSys.RecordingCollectedPos[battler] = battleSys.RecordingAckPos[battler];
        }
        return count;
    }
}

public static partial class BattleIO
{
    private const int CommandSetMosaic = 0x2C;
    private const int CommandChangeWeatherForm = 0x2D;
    private const int CommandSetLinkBattleResult = 0x41;
    private const int ChangeWeatherFormSize = 0xC;
    private const int LinkBattleResultSize = 0x24;
    private const int LinkRecordingMax = 0x1C;

    // OFFSET 0x022664F8
    public static void BattleIO_SetMosaic(BattleSystem battleSys, int battler, int param, int wait)
    {
        byte[] message = new byte[4];
        W8(message, 0, CommandSetMosaic);
        W8(message, 1, param);
        W8(message, 2, wait);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x0226651C
    public static void BattleIO_ChangeWeatherForm(BattleSystem battleSys, int battler)
    {
        byte[] message = new byte[ChangeWeatherFormSize];
        BattleMon mon = battleSys.BattleCtx!.BattleMons[battler];
        W8(message, 0, CommandChangeWeatherForm);
        W16(message, 2, mon.Species);
        W8(message, 5, mon.IsShiny & 1);
        if ((mon.StatusVolatile & 0x200000) != 0)
        {
            W8(message, 4, mon.MoveEffectsData.TransformedGender);
            W32(message, 8, mon.MoveEffectsData.TransformedPID);
        }
        else
        {
            W8(message, 4, mon.Gender & 0xF);
            W32(message, 8, mon.Personality);
        }
        W8(message, 1, mon.FormNum & 0x1F);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, ChangeWeatherFormSize);
    }

    // OFFSET 0x02266A38
    public static void BattleIO_SetLinkBattleResult(BattleSystem battleSys)
    {
        byte[] message = new byte[0x24];
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        W8(message, 0, CommandSetLinkBattleResult);
        W32(message, 4, Battle.BattleSystem_GetResultMask(battleSys));
        W16(message, 2, 0);
        if ((battleType & 4) != 0 && BattleRecording.BattleRecording_Exists() == 1 && (battleSys.BattleStatusMask & 0x10) == 0)
        {
            W16(message, 2, BattleSystems.BattleSystem_CollectRecordingData(battleSys, message, 8));
            if ((ushort)Bin16(message, 2) > LinkRecordingMax)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        BattleIO_Send(battleSys, RecipientBattler, 0, message, LinkBattleResultSize);
    }

    private static int Bin16(byte[] message, int offset)
    {
        return message[offset] | (message[offset + 1] << 8);
    }
}

public static partial class BattleRecording
{
    // OFFSET 0x0202F250
    public static int BattleRecording_Exists()
    {
        return sBattleRecording != null ? 1 : 0;
    }
}
