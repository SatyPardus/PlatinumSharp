namespace PlatinumSharp.Game;

public static partial class BattleIO
{
    public const int RecipientServer = 0;
    public const int RecipientBattler = 1;
    public const int RecipientQueue = 2;
    public const int QueueSize = 16;
    public const int MessageHeaderSize = 4;
    public const int MessageBufferSize = 0x1000;
    public const int CommandSetupBattleUI = 1;
    private const int LinkTimeoutFrames = 0x708;
    public const int CommandPokemonSendOut = 3;
    public const int PokemonSendOutSize = 0x74;
    public const int CommandDeleteTrainerSprite = 7;
    public const int CommandTrainerEncounter = 8;
    public const int CommandThrowTrainerBall = 9;
    public const int CommandSlideTrainerOut = 0xA;
    public const int CommandSlideTrainerIn = 0xB;
    public const int CommandEncounterMessage = 0x22;
    public const int CommandSendOutMessage = 0x23;
    public const int CommandShowPartyGaugeAtStart = 0x30;
    public const int CommandHidePartyGaugeAtStart = 0x31;
    public const int CommandShowPartyGauge = 0x32;
    public const int CommandHidePartyGauge = 0x33;
    public const int CommandLoadPartyGaugeGraphics = 0x34;

    private const int MonDataSpeciesEgg = 0xAE;
    private const int MonDataCurHP = 0xA3;
    private const int MonDataStatus = 0xA0;
    private const uint SpeciesEgg = 0x1EE;
    public const int CommandFreePartyGaugeGraphics = 0x35;

    private const int TrainerDataClassOffset = 1;

    private const uint BattleStatusRecording = 0x10;

    // OFFSET 0x0223ED60
    public static byte BattleSystem_IsInitialized(BattleSystem battleSys)
    {
        return battleSys.BattleInitialized;
    }

    // OFFSET 0x02251F44
    public static void BattleIO_EnqueueVal(BattleContext battleCtx, int recipient, int battler, int command)
    {
        int i;
        for (i = 0; i < QueueSize; i++)
        {
            if (battleCtx.IoQueue[recipient, battler, i] == 0)
            {
                battleCtx.IoQueue[recipient, battler, i] = (byte)command;
                break;
            }
        }
        if (i >= QueueSize)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02251F80
    public static void BattleIO_DequeueVal(BattleContext battleCtx, int recipient, int battler, int command)
    {
        if (command == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        int i;
        for (i = 0; i < QueueSize; i++)
        {
            if (battleCtx.IoQueue[recipient, battler, i] == command)
            {
                battleCtx.IoQueue[recipient, battler, i] = 0;
                break;
            }
        }
        if (i >= QueueSize)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    public static void W8(byte[] message, int offset, int value)
    {
        message[offset] = (byte)value;
    }

    public static void W16(byte[] message, int offset, int value)
    {
        message[offset] = (byte)value;
        message[offset + 1] = (byte)(value >> 8);
    }

    public static void W32(byte[] message, int offset, uint value)
    {
        message[offset] = (byte)value;
        message[offset + 1] = (byte)(value >> 8);
        message[offset + 2] = (byte)(value >> 16);
        message[offset + 3] = (byte)(value >> 24);
    }

    public static int R8(byte[] message, int offset)
    {
        return message[offset];
    }

    public static int R16(byte[] message, int offset)
    {
        return message[offset] | (message[offset + 1] << 8);
    }

    public static uint R32(byte[] message, int offset)
    {
        return (uint)(message[offset] | (message[offset + 1] << 8) | (message[offset + 2] << 16) | (message[offset + 3] << 24));
    }

    // OFFSET 0x0226485C
    private static void BattleIO_Write(BattleSystem battleSys, int recipient, int battler, byte[] data, int size)
    {
        byte[] message;
        if (recipient == RecipientBattler)
        {
            message = BattleSystems.BattleSystem_GetClientMessage(battleSys);
            ref ushort writeIndex = ref BattleSystems.BattleSystem_GetClientWriteIndex(battleSys);
            ref ushort endIndex = ref BattleSystems.BattleSystem_GetClientEndIndex(battleSys);
            BattleIO_WriteTo(message, ref writeIndex, ref endIndex, recipient, battler, data, size);
        }
        else
        {
            message = BattleSystems.BattleSystem_GetServerMessage(battleSys);
            ref ushort writeIndex = ref BattleSystems.BattleSystem_GetServerWriteIndex(battleSys);
            ref ushort endIndex = ref BattleSystems.BattleSystem_GetServerEndIndex(battleSys);
            BattleIO_WriteTo(message, ref writeIndex, ref endIndex, recipient, battler, data, size);
        }
    }

    private static void BattleIO_WriteTo(byte[] message, ref ushort writeIndex, ref ushort endIndex, int recipient, int battler, byte[] data, int size)
    {
        if (writeIndex + MessageHeaderSize + 1 + size > MessageBufferSize)
        {
            endIndex = writeIndex;
            writeIndex = 0;
        }
        message[writeIndex++] = (byte)recipient;
        message[writeIndex++] = (byte)battler;
        message[writeIndex++] = (byte)size;
        message[writeIndex++] = (byte)(size >> 8);
        for (int i = 0; i < size; i++)
        {
            message[writeIndex++] = data[i];
        }
    }

    // OFFSET 0x02264A04
    public static void BattleIO_Send(BattleSystem battleSys, int recipient, int battler, byte[] data, int size)
    {
        if ((battleSys.BattleType & BattleSystems.BattleTypeLink) != 0 && (battleSys.BattleStatusMask & BattleStatusRecording) == 0)
        {
            throw Unported.Partial(0x02264A04);
        }
        if (recipient == RecipientBattler)
        {
            BattleIO_EnqueueVal(battleSys.BattleCtx!, 0, battler, data[0]);
        }
        BattleIO_Write(battleSys, recipient, battler, data, size);
    }

    // OFFSET 0x02264A8C
    public static void BattleIO_SetupBattleUI(BattleSystem battleSys, int battler)
    {
        byte[] message = new byte[8];
        W32(message, 0, CommandSetupBattleUI);
        W32(message, 4, BattleSystems.BattleSystem_GetSeedDTO(battleSys));
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 8);
    }

    private static void BattleIO_WriteMonHeader(BattleSystem battleSys, int battler, byte[] message)
    {
        BattleMon mon = battleSys.BattleCtx!.BattleMons[battler];
        W8(message, 1, (mon.Gender & 3) | ((mon.IsShiny & 1) << 2) | ((mon.FormNum & 0x1F) << 3));
        W16(message, 2, mon.Species);
        W32(message, 4, mon.Personality);
    }

    private static void BattleIO_WriteMonMoves(BattleSystem battleSys, int battler, byte[] message, int offset)
    {
        BattleContext battleCtx = battleSys.BattleCtx!;
        for (int i = 0; i < 4; i++)
        {
            W16(message, offset + i * 2, BattleContexts.BattleMon_Get(battleCtx, battler, BattleContexts.BattleMonMove1 + i, null));
            W16(message, offset + 8 + i * 2, BattleContexts.BattleMon_Get(battleCtx, battler, BattleContexts.BattleMonCurPP1 + i, null));
            W16(message, offset + 0x10 + i * 2, BattleContexts.BattleMon_Get(battleCtx, battler, BattleContexts.BattleMonMaxPP1 + i, null));
        }
        ushort[] nickname = new ushort[11];
        BattleContexts.BattleMon_Get(battleCtx, battler, BattleContexts.BattleMonNickname, nickname);
        for (int i = 0; i < nickname.Length; i++)
        {
            W16(message, offset + 0x18 + i * 2, nickname[i]);
        }
    }

    // OFFSET 0x02264BB4
    public static void BattleIO_PokemonSendOut(BattleSystem battleSys, int battler)
    {
        BattleContext battleCtx = battleSys.BattleCtx!;
        byte[] message = new byte[PokemonSendOutSize];
        W8(message, 0, CommandPokemonSendOut);
        BattleIO_WriteMonHeader(battleSys, battler, message);
        W32(message, 8, (uint)BattleContexts.BattleSystem_GetCryModulation(battleCtx, battler, BattleSystems.BattleSystem_BattlerSlot(battleSys, battler), 1));
        int slot = battleCtx.SelectedPartySlot[battler];
        W32(message, 0xC, (uint)slot);
        W32(message, 0x10, battleCtx.BattleMons[battler].CapturedBall);
        W32(message, 0x48, battleCtx.SelectedPartySlot[BattleSystems.BattleSystem_GetPartner(battleSys, battler)]);
        BattleSystems.BattleSystem_SetParticipant(battleSys, battler, slot);
        BattleIO_WriteMonMoves(battleSys, battler, message, 0x18);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, PokemonSendOutSize);
    }

    // OFFSET 0x02265108
    public static void BattleIO_DeleteTrainerSprite(BattleSystem battleSys, int battler)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandDeleteTrainerSprite);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x02265124
    public static void BattleIO_TrainerEncounter(BattleSystem battleSys, int battler)
    {
        byte[] message = new byte[4];
        W8(message, 0, CommandTrainerEncounter);
        W16(message, 2, battleSys.Trainers[battler][TrainerDataClassOffset]);
        W8(message, 1, battleSys.Unwritten[battler]);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x02265154
    public static void BattleIO_ThrowTrainerBall(BattleSystem battleSys, int battler, int throwType)
    {
        byte[] message = new byte[4];
        W8(message, 0, CommandThrowTrainerBall);
        W8(message, 1, throwType);
        W16(message, 2, battleSys.BattleCtx!.SelectedPartySlot[BattleSystems.BattleSystem_GetPartner(battleSys, battler)]);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x0226518C
    public static void BattleIO_SlideTrainerOut(BattleSystem battleSys, int battler)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandSlideTrainerOut);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x022651A8
    public static void BattleIO_SlideTrainerIn(BattleSystem battleSys, int battler, int position)
    {
        byte[] message = new byte[8];
        W8(message, 0, CommandSlideTrainerIn);
        W16(message, 2, battleSys.Trainers[battler][TrainerDataClassOffset]);
        W8(message, 1, battleSys.Unwritten[battler]);
        W32(message, 4, (uint)position);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 8);
    }

    // OFFSET 0x022660E8
    public static void BattleIO_EncounterMessage(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandEncounterMessage);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x02266100
    public static void BattleIO_SendOutMessage(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        byte[] message = new byte[8];
        W8(message, 0, CommandSendOutMessage);
        for (int i = 0; i < BattleSystems.BattleSystem_GetMaxBattlers(battleSys); i++)
        {
            message[4 + i] = battleCtx.SelectedPartySlot[i];
        }
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 8);
    }

    private static void BattleIO_SendPartyGauge(BattleSystem battleSys, int battler, int command)
    {
        byte[] message = new byte[8];
        BattleIO_BuildPartyGaugeData(battleSys, battleSys.BattleCtx!, message, command, battler);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 8);
    }

    // OFFSET 0x022665E4
    public static void BattleIO_ShowPartyGaugeAtStart(BattleSystem battleSys, int battler)
    {
        BattleIO_SendPartyGauge(battleSys, battler, CommandShowPartyGaugeAtStart);
    }

    // OFFSET 0x0226660C
    public static void BattleIO_HidePartyGaugeAtStart(BattleSystem battleSys, int battler)
    {
        BattleIO_SendPartyGauge(battleSys, battler, CommandHidePartyGaugeAtStart);
    }

    // OFFSET 0x02266634
    public static void BattleIO_ShowPartyGauge(BattleSystem battleSys, int battler)
    {
        BattleIO_SendPartyGauge(battleSys, battler, CommandShowPartyGauge);
    }

    // OFFSET 0x0226665C
    public static void BattleIO_HidePartyGauge(BattleSystem battleSys, int battler)
    {
        BattleIO_SendPartyGauge(battleSys, battler, CommandHidePartyGauge);
    }

    // OFFSET 0x02266684
    public static void BattleIO_LoadPartyGaugeGraphics(BattleSystem battleSys)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandLoadPartyGaugeGraphics);
        BattleIO_Send(battleSys, RecipientBattler, 0, message, 4);
    }

    // OFFSET 0x022666A0
    public static void BattleIO_FreePartyGaugeGraphics(BattleSystem battleSys)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandFreePartyGaugeGraphics);
        BattleIO_Send(battleSys, RecipientBattler, 0, message, 4);
    }

    // OFFSET 0x02266ABC
    public static void BattleIO_ClearMessage(BattleSystem battleSys, int battler, int command)
    {
        byte[] message = new byte[4];
        W8(message, 0, command);
        W8(message, 1, CommSystem.CommSys_CurNetId());
        BattleIO_Send(battleSys, RecipientQueue, battler, message, 4);
    }

    private static int BattleIO_FillPartyGaugeStatus(BattleContext battleCtx, Party party, int orderRow, byte[] message, int count)
    {
        for (int i = 0; i < Party.Party_GetCurrentCount(party); i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, battleCtx.PartyOrder[orderRow, i]);
            uint species = Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEgg, null);
            if (species == 0 || species == SpeciesEgg)
            {
                continue;
            }
            if (Pokemon.Pokemon_GetValue(mon, MonDataCurHP, null) != 0)
            {
                if (Pokemon.Pokemon_GetValue(mon, MonDataStatus, null) != 0)
                {
                    message[2 + count] = PartyGauges.StatusAilment;
                }
                else
                {
                    message[2 + count] = PartyGauges.StatusHealthy;
                }
            }
            else
            {
                message[2 + count] = PartyGauges.StatusFainted;
            }
            count++;
        }
        return count;
    }

    // OFFSET 0x02266CF0
    private static void BattleIO_BuildPartyGaugeData(BattleSystem battleSys, BattleContext battleCtx, byte[] message, int command, int battler)
    {
        Array.Clear(message, 0, 8);
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        message[0] = (byte)command;
        if ((battleType & 0xC) == 0xC
            || ((battleType & BattleSystems.BattleTypeTag) != 0 && BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) != 0)
            || (battleType == 0x4B && BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) != 0)
            || battleType == 0xCB)
        {
            int first;
            if (BattleSystems.BattleSystem_BattlerSlot(battleSys, battler) == 2 || BattleSystems.BattleSystem_BattlerSlot(battleSys, battler) == 3)
            {
                first = battler;
                battler = BattleSystems.BattleSystem_GetPartner(battleSys, first);
            }
            else
            {
                first = BattleSystems.BattleSystem_GetPartner(battleSys, battler);
            }
            BattleIO_FillPartyGaugeStatus(battleCtx, BattleSystems.BattleSystem_GetParty(battleSys, first), first, message, 0);
            BattleIO_FillPartyGaugeStatus(battleCtx, BattleSystems.BattleSystem_GetParty(battleSys, battler), battler, message, 3);
            return;
        }
        if ((battleType & BattleSystems.BattleTypeDoubles) != 0 && (battleType & BattleSystems.BattleTypeMulti) == 0)
        {
            battler &= 1;
        }
        BattleIO_FillPartyGaugeStatus(battleCtx, BattleSystems.BattleSystem_GetParty(battleSys, battler), battler, message, 0);
    }

    // OFFSET 0x02251FC8
    public static int BattleIO_QueueIsEmpty(BattleContext battleCtx)
    {
        int sum = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                for (int k = 0; k < QueueSize; k++)
                {
                    sum += battleCtx.IoQueue[i, j, k];
                }
            }
        }
        if (sum == 0)
        {
            battleCtx.LinkBattleTimeout = 0;
        }
        return sum == 0 ? 1 : 0;
    }

    // OFFSET 0x0225201C
    public static void BattleIO_UpdateTimeout(BattleContext battleCtx)
    {
        battleCtx.LinkBattleTimeout++;
        if (battleCtx.LinkBattleTimeout > LinkTimeoutFrames)
        {
            CommManager.CommMan_SetErrorState(1);
        }
    }

    // OFFSET 0x02252040
    public static void BattleIO_ClearBuffer(BattleContext battleCtx, int battler)
    {
        for (int i = 0; i < 0x100; i++)
        {
            battleCtx.IoBuffer[battler, i] = 0;
        }
    }

    // OFFSET 0x022648F4
    private static int BattleIO_RecvMessage(BattleSystem battleSys, byte[] message, int offset)
    {
        int recipient = message[offset];
        int battler = message[offset + 1];
        int size = message[offset + 2] | (message[offset + 3] << 8);
        int received = 0;
        if (recipient == RecipientServer)
        {
            BattleContext battleCtx = battleSys.BattleCtx!;
            if (battleCtx.IoBuffer[battler, 0] == 0)
            {
                for (int i = 0; i < size; i++)
                {
                    battleCtx.IoBuffer[battler, i] = message[offset + MessageHeaderSize + i];
                }
                received = 1;
            }
        }
        else if (recipient == RecipientBattler)
        {
            BattlerData battlerData = battleSys.Battlers[battler]!;
            if (battlerData.Data[0] == 0)
            {
                for (int i = 0; i < size; i++)
                {
                    battlerData.Data[i] = message[offset + MessageHeaderSize + i];
                }
                received = 1;
            }
        }
        else if (recipient == RecipientQueue)
        {
            int command = message[offset + 4];
            int queueRecipient = message[offset + 5];
            if (BattleSystem_IsInitialized(battleSys) != 0)
            {
                BattleIO_DequeueVal(battleSys.BattleCtx!, queueRecipient, battler, command);
            }
            received = 1;
        }
        return received;
    }

    // OFFSET 0x02264988
    public static void BattleIO_UpdateQueue(BattleSystem battleSys, int isClient)
    {
        byte[] message;
        if (isClient == 1)
        {
            message = BattleSystems.BattleSystem_GetClientMessage(battleSys);
            ref ushort readIndex = ref BattleSystems.BattleSystem_GetClientReadIndex(battleSys);
            ref ushort writeIndex = ref BattleSystems.BattleSystem_GetClientWriteIndex(battleSys);
            ref ushort endIndex = ref BattleSystems.BattleSystem_GetClientEndIndex(battleSys);
            BattleIO_ProcessQueue(battleSys, message, ref readIndex, ref writeIndex, ref endIndex);
        }
        else
        {
            message = BattleSystems.BattleSystem_GetServerMessage(battleSys);
            ref ushort readIndex = ref BattleSystems.BattleSystem_GetServerReadIndex(battleSys);
            ref ushort writeIndex = ref BattleSystems.BattleSystem_GetServerWriteIndex(battleSys);
            ref ushort endIndex = ref BattleSystems.BattleSystem_GetServerEndIndex(battleSys);
            BattleIO_ProcessQueue(battleSys, message, ref readIndex, ref writeIndex, ref endIndex);
        }
    }

    private static void BattleIO_ProcessQueue(BattleSystem battleSys, byte[] message, ref ushort readIndex, ref ushort writeIndex, ref ushort endIndex)
    {
        if (readIndex == writeIndex)
        {
            return;
        }
        if (readIndex == endIndex)
        {
            readIndex = 0;
            endIndex = 0;
        }
        if (BattleIO_RecvMessage(battleSys, message, readIndex) == 1)
        {
            int size = message[readIndex + 2] | (message[readIndex + 3] << 8);
            readIndex = (ushort)(readIndex + size + MessageHeaderSize);
        }
    }
}
