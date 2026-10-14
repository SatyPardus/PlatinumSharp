using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class RankingEntry
{
    public const int Size = 0x18;
    public const int NameLength = 8;

    private const int NameOffset = 8;

    public uint GroupID;
    public uint Value;
    public readonly ushort[] PlayerName = new ushort[NameLength];

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, GroupID);
        Bin.W32(body, offset + 4, Value);
        for (int i = 0; i < NameLength; i++)
        {
            Bin.W16(body, offset + NameOffset + i * 2, PlayerName[i]);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        GroupID = Bin.U32(body, offset);
        Value = Bin.U32(body, offset + 4);
        for (int i = 0; i < NameLength; i++)
        {
            PlayerName[i] = (ushort)Bin.U16(body, offset + NameOffset + i * 2);
        }
    }

    public void CopyFrom(RankingEntry source)
    {
        GroupID = source.GroupID;
        Value = source.Value;
        Array.Copy(source.PlayerName, PlayerName, NameLength);
    }

    // OFFSET 0x0202E858
    public static void RankingEntry_Clear(RankingEntry entry)
    {
        entry.GroupID = 0;
        entry.Value = 0;
        CharCode.CharCode_FillWithEOS(entry.PlayerName, 0, NameLength);
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryRankings);
    }

    // OFFSET 0x0202E870
    public static bool RankingEntry_HasPlayerName(RankingEntry entry)
    {
        if (CharCode.CharCode_Length(entry.PlayerName, 0) != 0)
        {
            return true;
        }
        return false;
    }
}

public sealed class RecordPlayerInfo
{
    public uint GroupID;
    public uint Value;
    public GameString? PlayerName;
}

public sealed class RecordPlayersInfo
{
    public const int MaxPlayers = 6;

    public int Count;
    public readonly RecordPlayerInfo[] Players = new RecordPlayerInfo[MaxPlayers];

    public RecordPlayersInfo()
    {
        for (int i = 0; i < MaxPlayers; i++)
        {
            Players[i] = new RecordPlayerInfo();
        }
    }
}

public sealed class Rankings : ISaveBlock
{
    public const int Size = 0xEA0;
    public const int StatCount = 13;
    public const int ListCount = StatCount * 2;
    public const int EntriesPerList = 6;
    public const int MaxParticipants = 5;
    public const int ListSize = EntriesPerList * RankingEntry.Size;

    private const uint CategoryTable = 0x020E5CD0;
    private const int CategoryTableSize = 8;
    private const uint StatRecordTable = 0x020E5CD8;
    private const int FirstFrontierStat = 0;
    private const int LastFrontierStat = 4;
    private const int StatAverageBattles = 5;
    private const int StatContestWins = 10;
    private const int StatContestWinRate = 11;
    private const int RecordAverageDivisor = 0xF;
    private const int RecordAverageTotal = 0x1D;
    private const int RecordContestWinsA = 0x5C;
    private const int RecordContestWinsB = 0x5D;
    private const int RecordContestEntriesA = 0x5A;
    private const int RecordContestEntriesB = 0x5B;
    private const int FrontierRecordAll = 0xFF;
    private const int Percent = 100;

    public readonly RankingEntry[][] Lists = new RankingEntry[ListCount][];

    private static byte[]? sCategories;
    private static uint[]? sStatRecords;

    private sealed class MergeWork
    {
        public int OwnCount;
        public int OtherCount;
        public int Total;
        public readonly RankingEntry?[] Sorted = new RankingEntry?[MaxParticipants + EntriesPerList];
        public readonly RankingEntry?[] Own = new RankingEntry?[EntriesPerList];
        public readonly RankingEntry[] Copy = new RankingEntry[EntriesPerList];

        public MergeWork()
        {
            for (int i = 0; i < EntriesPerList; i++)
            {
                Copy[i] = new RankingEntry();
            }
        }
    }

    public Rankings()
    {
        for (int list = 0; list < ListCount; list++)
        {
            Lists[list] = new RankingEntry[EntriesPerList];
            for (int i = 0; i < EntriesPerList; i++)
            {
                Lists[list][i] = new RankingEntry();
            }
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        for (int list = 0; list < ListCount; list++)
        {
            for (int i = 0; i < EntriesPerList; i++)
            {
                Lists[list][i].WriteTo(body, offset + list * ListSize + i * RankingEntry.Size);
            }
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int list = 0; list < ListCount; list++)
        {
            for (int i = 0; i < EntriesPerList; i++)
            {
                Lists[list][i].ReadFrom(body, offset + list * ListSize + i * RankingEntry.Size);
            }
        }
    }

    // OFFSET 0x0202E840
    public static byte Rankings_GetCategoryStatCount(int category)
    {
        sCategories ??= RomData.Arm9(CategoryTable, CategoryTableSize);
        return sCategories[category * 2];
    }

    // OFFSET 0x0202E84C
    public static byte Rankings_GetCategoryFirstStat(int category)
    {
        sCategories ??= RomData.Arm9(CategoryTable, CategoryTableSize);
        return sCategories[category * 2 + 1];
    }

    // OFFSET 0x0202E88C
    public static void Rankings_Init(Rankings rankings)
    {
        for (int list = 0; list < ListCount; list++)
        {
            for (int i = 0; i < EntriesPerList; i++)
            {
                RankingEntry.RankingEntry_Clear(rankings.Lists[list][i]);
            }
        }
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryRankings);
    }

    // OFFSET 0x0202E8C0
    public static Rankings SaveData_GetRankings(SaveData saveData)
    {
        SaveData.SaveData_Checksum(SaveDataTable.EntryRankings);
        return (Rankings)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryRankings);
    }

    // OFFSET 0x0202E8D4
    public static void Rankings_RemoveEntry(Rankings rankings, int listID, byte entryID)
    {
        if (entryID >= EntriesPerList)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        RankingEntry[] list = rankings.Lists[listID];
        for (int i = entryID; i < EntriesPerList - 1; i++)
        {
            list[i].CopyFrom(list[i + 1]);
        }
        RankingEntry.RankingEntry_Clear(list[EntriesPerList - 1]);
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryRankings);
    }

    // OFFSET 0x0202E91C
    public static int RankingsEntries_Size()
    {
        return StatCount * RankingEntry.Size;
    }

    // OFFSET 0x0202E924
    private static uint[] Rankings_GetPlayerStats(SaveData saveData, int heapID)
    {
        GameRecords records = GameRecords.SaveData_GetGameRecords(saveData);
        object frontier = BattleFrontierSave.SaveData_GetBattleFrontier(saveData);
        uint[] stats = new uint[StatCount];
        sStatRecords ??= RomData.Arm9U32(StatRecordTable, StatCount);
        for (int i = 0; i < StatCount; i++)
        {
            if (i == StatAverageBattles)
            {
                uint divisor = GameRecords.GameRecords_GetRecordValue(records, RecordAverageDivisor);
                if (divisor != 0)
                {
                    divisor = GameRecords.GameRecords_GetRecordValue(records, RecordAverageTotal) / divisor;
                }
                stats[i] = divisor;
            }
            else if (i == StatContestWins)
            {
                uint wins = GameRecords.GameRecords_GetRecordValue(records, RecordContestWinsA);
                stats[i] = wins + GameRecords.GameRecords_GetRecordValue(records, RecordContestWinsB);
            }
            else if (i == StatContestWinRate)
            {
                uint entries = GameRecords.GameRecords_GetRecordValue(records, RecordContestEntriesA);
                entries += GameRecords.GameRecords_GetRecordValue(records, RecordContestEntriesB);
                if (entries != 0)
                {
                    entries = stats[StatContestWins] * Percent / entries;
                }
                stats[i] = entries;
            }
            else if (i >= FirstFrontierStat && i <= LastFrontierStat)
            {
                stats[i] = sub_02030698(frontier, sStatRecords[i], FrontierRecordAll);
            }
            else
            {
                stats[i] = GameRecords.GameRecords_GetRecordValue(records, (int)sStatRecords[i]);
            }
        }
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryRankings);
        return stats;
    }

    // OFFSET 0x0202E9FC
    public static RankingEntry[] SaveData_GetRankingEntries(SaveData saveData, int heapID)
    {
        TrainerInfo info = PlayerSave.SaveData_GetTrainerInfo(saveData);
        RankingEntry[] entries = new RankingEntry[StatCount];
        uint groupID = RecordMixedRNG.RecordMixedRNG_GetEntrySeed(RecordMixedRNG.SaveData_GetRecordMixedRNG(saveData), RecordMixedRNG.EntryActive);
        GameString name = TrainerInfo.TrainerInfo_NameNewString(info, heapID);
        uint[] stats = Rankings_GetPlayerStats(saveData, heapID);
        for (int i = 0; i < StatCount; i++)
        {
            entries[i] = new RankingEntry();
            entries[i].GroupID = groupID;
            GameString.String_ToChars(name, entries[i].PlayerName, RankingEntry.NameLength);
            entries[i].Value = stats[i];
        }
        GameString.String_Free(name);
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryRankings);
        return entries;
    }

    // OFFSET 0x0202EA80
    private static void Rankings_ClearList(RankingEntry[] list)
    {
        for (int i = 0; i < EntriesPerList; i++)
        {
            RankingEntry.RankingEntry_Clear(list[i]);
        }
    }

    // OFFSET 0x0202EA98
    private static bool RankingEntry_IsSamePlayer(RankingEntry first, RankingEntry second)
    {
        if (first.GroupID != second.GroupID)
        {
            return false;
        }
        if (CharCode.CharCode_Compare(first.PlayerName, 0, second.PlayerName, 0) == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202EABC
    private static bool Rankings_IsPlayerInMerge(MergeWork work, RankingEntry entry)
    {
        for (int i = 0; i < work.OtherCount; i++)
        {
            if (RankingEntry_IsSamePlayer(work.Sorted[i]!, entry))
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0202EAEC
    private static void Rankings_MergeList(Rankings rankings, MergeWork work, uint groupID, int stat, bool groupOnly, RankingEntry[][] rankingData, byte participantCount, int heapID)
    {
        work.OwnCount = 0;
        work.OtherCount = 0;
        work.Total = 0;
        Array.Clear(work.Sorted);
        Array.Clear(work.Own);
        RankingEntry[] list;
        if (!groupOnly)
        {
            list = rankings.Lists[stat];
            for (int i = 0; i < EntriesPerList; i++)
            {
                work.Copy[i].CopyFrom(list[i]);
            }
            for (int i = 0; i < participantCount; i++)
            {
                work.Sorted[work.OtherCount] = rankingData[i][stat];
                work.OtherCount++;
            }
            for (int i = 0; i < EntriesPerList; i++)
            {
                if (RankingEntry.RankingEntry_HasPlayerName(work.Copy[i]) && !Rankings_IsPlayerInMerge(work, work.Copy[i]))
                {
                    work.Own[work.OwnCount] = work.Copy[i];
                    work.OwnCount++;
                }
            }
        }
        else
        {
            list = rankings.Lists[stat + StatCount];
            for (int i = 0; i < EntriesPerList; i++)
            {
                work.Copy[i].CopyFrom(list[i]);
            }
            for (int i = 0; i < participantCount; i++)
            {
                RankingEntry entry = rankingData[i][stat];
                if (entry.GroupID != 0 && groupID == entry.GroupID)
                {
                    work.Sorted[work.OtherCount] = entry;
                    work.OtherCount++;
                }
            }
            for (int i = 0; i < EntriesPerList; i++)
            {
                if (RankingEntry.RankingEntry_HasPlayerName(work.Copy[i]) && groupID == work.Copy[i].GroupID && !Rankings_IsPlayerInMerge(work, work.Copy[i]))
                {
                    work.Own[work.OwnCount] = work.Copy[i];
                    work.OwnCount++;
                }
            }
        }
        work.Total = work.OtherCount + work.OwnCount;
        for (int i = 0; i < work.OwnCount; i++)
        {
            work.Sorted[work.OtherCount + i] = work.Own[i];
        }
        for (int i = 0; i < work.Total - 1; i++)
        {
            for (int j = work.Total - 1; j > i; j--)
            {
                if (work.Sorted[i]!.Value < work.Sorted[j]!.Value)
                {
                    (work.Sorted[i], work.Sorted[j]) = (work.Sorted[j], work.Sorted[i]);
                }
            }
        }
        Rankings_ClearList(list);
        for (int i = 0; i < EntriesPerList && i < work.Total; i++)
        {
            list[i].CopyFrom(work.Sorted[i]!);
        }
    }

    // OFFSET 0x0202ECB0
    private static void Rankings_UpdateList(Rankings rankings, uint groupID, int stat, RankingEntry[][] rankingData, byte participantCount, int heapID)
    {
        MergeWork work = new();
        Rankings_MergeList(rankings, work, groupID, stat, false, rankingData, participantCount, heapID);
        if (groupID != 0)
        {
            Rankings_MergeList(rankings, work, groupID, stat, true, rankingData, participantCount, heapID);
        }
    }

    // OFFSET 0x0202ED0C
    public static void SaveData_UpdateRankings(SaveData saveData, int participantIndex, byte participantCount, RankingEntry[]?[] rankingData, int heapID)
    {
        Rankings rankings = SaveData_GetRankings(saveData);
        uint groupID = RecordMixedRNG.RecordMixedRNG_GetEntrySeed(RecordMixedRNG.SaveData_GetRecordMixedRNG(saveData), RecordMixedRNG.EntryActive);
        RankingEntry[][] others = new RankingEntry[MaxParticipants][];
        byte count = 0;
        for (byte i = 0; i < participantCount; i++)
        {
            if (i != participantIndex && rankingData[i] != null)
            {
                others[count] = rankingData[i]!;
                count++;
            }
        }
        if (count == 0)
        {
            return;
        }
        for (byte stat = 0; stat < StatCount; stat++)
        {
            Rankings_UpdateList(rankings, groupID, stat, others, count, heapID);
        }
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryRankings);
    }

    // OFFSET 0x0202ED8C
    public static RecordPlayersInfo Rankings_GetCurrentPlayerInfo(SaveData saveData, int category, int heapID)
    {
        TrainerInfo info = PlayerSave.SaveData_GetTrainerInfo(saveData);
        RecordPlayersInfo playersInfo = new();
        uint groupID = RecordMixedRNG.RecordMixedRNG_GetEntrySeed(RecordMixedRNG.SaveData_GetRecordMixedRNG(saveData), RecordMixedRNG.EntryActive);
        uint[] stats = Rankings_GetPlayerStats(saveData, heapID);
        playersInfo.Count = Rankings_GetCategoryStatCount(category);
        int firstStat = Rankings_GetCategoryFirstStat(category);
        for (int i = 0; i < playersInfo.Count; i++)
        {
            playersInfo.Players[i].GroupID = groupID;
            playersInfo.Players[i].Value = stats[firstStat + i];
            playersInfo.Players[i].PlayerName = TrainerInfo.TrainerInfo_NameNewString(info, heapID);
        }
        return playersInfo;
    }

    // OFFSET 0x0202EE10
    public static RecordPlayersInfo Rankings_GetConnectedPlayersInfo(Rankings rankings, int listID, int heapID)
    {
        RecordPlayersInfo playersInfo = new();
        RankingEntry[] list = rankings.Lists[listID];
        for (int i = 0; i < EntriesPerList; i++)
        {
            if (!RankingEntry.RankingEntry_HasPlayerName(list[i]))
            {
                continue;
            }
            RecordPlayerInfo player = playersInfo.Players[playersInfo.Count];
            player.GroupID = list[i].GroupID;
            player.Value = list[i].Value;
            player.PlayerName = GameString.String_Init(RankingEntry.NameLength, heapID);
            GameString.String_CopyChars(player.PlayerName, list[i].PlayerName);
            playersInfo.Count++;
        }
        return playersInfo;
    }

    // OFFSET 0x0202EE8C
    public static void RecordPlayersInfo_Free(RecordPlayersInfo playersInfo)
    {
        for (int i = 0; i < RecordPlayersInfo.MaxPlayers; i++)
        {
            if (playersInfo.Players[i].PlayerName != null)
            {
                GameString.String_Free(playersInfo.Players[i].PlayerName);
            }
            playersInfo.Players[i].GroupID = 0;
            playersInfo.Players[i].Value = 0;
            playersInfo.Players[i].PlayerName = null;
        }
        playersInfo.Count = 0;
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryRankings);
    }
}
