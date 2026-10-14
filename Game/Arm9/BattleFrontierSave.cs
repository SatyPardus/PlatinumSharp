using System.Buffers.Binary;

namespace PlatinumSharp.Game;

public sealed class WifiBattleTowerRecord : BufferView
{
    public const int Size = 0x168;

    private const int RankOffset = 3;

    public WifiBattleTowerRecord(byte[] buffer, int offset)
        : base(buffer, offset)
    {
    }

    // OFFSET 0x0202D06C
    public static void WifiBattleTowerRecord_Init(WifiBattleTowerRecord record)
    {
        record.Clear(0, Size);
        record.W8(RankOffset, 1);
    }
}

public sealed class FrontierEasyChatMessages : BufferView
{
    public const int SentenceCount = 4;
    public const int SentenceSize = 8;

    public FrontierEasyChatMessages(byte[] buffer, int offset)
        : base(buffer, offset)
    {
    }

    // OFFSET 0x0202D080
    public static void FrontierEasyChatMessages_Init(FrontierEasyChatMessages messages)
    {
        for (int i = 0; i < SentenceCount; i++)
        {
            EasyChatSentence sentence = new();
            EasyChatSentence.EasyChatSentence_SetDefaultFrontierFields(sentence, i);
            messages.W16(i * SentenceSize, sentence.Type);
            messages.W16(i * SentenceSize + 2, sentence.ID);
            messages.W16(i * SentenceSize + 4, sentence.Words[0]);
            messages.W16(i * SentenceSize + 6, sentence.Words[1]);
        }
    }
}

public sealed class WifiBattleTowerDownloadData : BufferView
{
    public const int Size = 0xB3C;

    public WifiBattleTowerDownloadData(byte[] buffer, int offset)
        : base(buffer, offset)
    {
    }

    // OFFSET 0x0202D0AC
    public static void WifiBattleTowerDownloadData_Init(WifiBattleTowerDownloadData downloadData)
    {
        downloadData.Clear(0, Size);
    }
}

public sealed class BattleFrontierSave : ISaveBlock
{
    public const int Size = 0x1628;

    private const int TowerRecordOffset = 0x954;
    private const int MessagesOffset = 0xABC;
    private const int DownloadDataOffset = 0xADC;
    private const int FactoryFlagsOffset = 0x1618;
    private const int HallFlagsOffset = 0x161C;
    private const int CastleFlagsOffset = 0x1620;
    private const int ArcadeFlagsOffset = 0x1624;

    public readonly byte[] Raw = new byte[Size];

    public void WriteTo(byte[] body, int offset)
    {
        Array.Copy(Raw, 0, body, offset, Size);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Array.Copy(body, offset, Raw, 0, Size);
    }

    // OFFSET 0x02030624
    public static void BattleFrontier_Init(BattleFrontierSave frontier)
    {
        Array.Clear(frontier.Raw);
        WifiBattleTowerRecord.WifiBattleTowerRecord_Init(new WifiBattleTowerRecord(frontier.Raw, TowerRecordOffset));
        FrontierEasyChatMessages.FrontierEasyChatMessages_Init(new FrontierEasyChatMessages(frontier.Raw, MessagesOffset));
        WifiBattleTowerDownloadData.WifiBattleTowerDownloadData_Init(new WifiBattleTowerDownloadData(frontier.Raw, DownloadDataOffset));
        BattleFactoryStreakFlags.BattleFactoryStreakFlags_Init(new BattleFactoryStreakFlags(frontier.Raw, FactoryFlagsOffset));
        BattleHallStreakFlags.BattleHallStreakFlags_Init(new BattleHallStreakFlags(frontier.Raw, HallFlagsOffset));
        BattleCastlePersistentSave.BattleCastlePersistentSave_Init(new BattleCastlePersistentSave(frontier.Raw, CastleFlagsOffset));
        BattleArcadeStreakFlags.BattleArcadeStreakFlags_Init(new BattleArcadeStreakFlags(frontier.Raw, ArcadeFlagsOffset));
    }

    private const int StatsFlagsStart = 0x64;
    private const int StatsFriendStart = 0x70;
    private const int FriendStatsOffset = 0xE0;
    private const int FriendStatsSize = 0x40;

    // OFFSET 0x02030698
    public static ushort BattleFrontierSave_GetStat(BattleFrontierSave frontier, int statIndex, int hostFriendID)
    {
        if (statIndex < StatsFriendStart)
        {
            if (statIndex >= StatsFlagsStart)
            {
                if (hostFriendID >= 0x10)
                {
                    statIndex++;
                    hostFriendID -= 0x10;
                }
                ushort flags = BinaryPrimitives.ReadUInt16LittleEndian(frontier.Raw.AsSpan(statIndex * 2));
                return (ushort)((flags >> hostFriendID) & 1);
            }
            return BinaryPrimitives.ReadUInt16LittleEndian(frontier.Raw.AsSpan(statIndex * 2));
        }
        if (hostFriendID == -1)
        {
            ErrorHandling.GF_AssertFail();
            return 0;
        }
        return BinaryPrimitives.ReadUInt16LittleEndian(frontier.Raw.AsSpan(hostFriendID * FriendStatsSize + (statIndex - StatsFriendStart) * 2 + FriendStatsOffset));
    }

    // OFFSET 0x0203068C
    public static BattleFrontierSave SaveData_GetBattleFrontier(SaveData saveData)
    {
        return (BattleFrontierSave)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryFrontier);
    }
}
