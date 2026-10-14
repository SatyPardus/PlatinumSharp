using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static partial class TrainerData
{
    private const int NarcTrainerMessages = 0x3B;
    private const int NarcTrainerMessageOffsets = 0x89;
    private const int TrainerMessageEntrySize = 4;

    // OFFSET 0x02079280
    public static int Trainer_HasMessageType(int trainerID, int messageType, int heapID)
    {
        int result = 0;
        uint size = NARC.NARC_GetMemberSizeByIndexPair(NarcTrainerMessages, 0);
        byte[] offsetRaw = new byte[2];
        NARC.NARC_ReadFromMemberByIndexPair(offsetRaw, NarcTrainerMessageOffsets, 0, trainerID * 2, 2);
        ushort offset = (ushort)Bin.U16(offsetRaw, 0);
        NARC narc = NARC.NARC_ctor(NarcTrainerMessages, heapID);
        byte[] entry = new byte[TrainerMessageEntrySize];
        while (offset != size)
        {
            NARC.NARC_ReadFromMember(narc, 0, offset, TrainerMessageEntrySize, entry);
            int entryTrainer = Bin.U16(entry, 0);
            if (entryTrainer == trainerID && Bin.U16(entry, 2) == messageType)
            {
                result = 1;
                break;
            }
            if (entryTrainer != trainerID)
            {
                break;
            }
            offset += TrainerMessageEntrySize;
        }
        NARC.NARC_dtor(narc);
        return result;
    }
}

public static partial class TrainerData
{
    private const int MessageArchiveMsgData = 0x1A;
    private const uint TrainerMessagesBank = 0x269;

    // OFFSET 0x020792F8
    public static void Trainer_LoadMessage(int trainerID, int messageType, GameString dest, int heapID)
    {
        uint size = NARC.NARC_GetMemberSizeByIndexPair(NarcTrainerMessages, 0);
        byte[] offsetRaw = new byte[2];
        NARC.NARC_ReadFromMemberByIndexPair(offsetRaw, NarcTrainerMessageOffsets, 0, trainerID * 2, 2);
        ushort offset = (ushort)Bin.U16(offsetRaw, 0);
        NARC narc = NARC.NARC_ctor(NarcTrainerMessages, heapID);
        byte[] entry = new byte[TrainerMessageEntrySize];
        while (offset != size)
        {
            NARC.NARC_ReadFromMember(narc, 0, offset, TrainerMessageEntrySize, entry);
            if (Bin.U16(entry, 0) == trainerID && Bin.U16(entry, 2) == messageType)
            {
                MessageBank.MessageBank_GetStringFromNARC(MessageArchiveMsgData, TrainerMessagesBank, (uint)(offset / TrainerMessageEntrySize), heapID, dest);
                break;
            }
            offset += TrainerMessageEntrySize;
        }
        NARC.NARC_dtor(narc);
        if (offset == size)
        {
            GameString.String_Clear(dest);
        }
    }
}
