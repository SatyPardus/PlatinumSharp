using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class TVSegmentInstance
{
    public const int Size = 0x30;
    public const int DataSize = 0x28;
    public const int MaxPlayCount = 3;

    private const int DateOffset = 4;
    private const int DataOffset = 8;

    public byte SegmentID;
    public byte PlayCount;
    public ushort Padding;
    public uint Date;
    public readonly byte[] Data = new byte[DataSize];

    public void WriteTo(byte[] body, int offset)
    {
        body[offset] = SegmentID;
        body[offset + 1] = PlayCount;
        Bin.W16(body, offset + 2, Padding);
        Bin.W32(body, offset + DateOffset, Date);
        Array.Copy(Data, 0, body, offset + DataOffset, DataSize);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        SegmentID = body[offset];
        PlayCount = body[offset + 1];
        Padding = (ushort)Bin.U16(body, offset + 2);
        Date = Bin.U32(body, offset + DateOffset);
        Array.Copy(body, offset + DataOffset, Data, 0, DataSize);
    }

    public void Clear()
    {
        SegmentID = 0;
        PlayCount = 0;
        Padding = 0;
        Date = 0;
        Array.Clear(Data);
    }

    // OFFSET 0x0202E3F4
    public static void TVSegmentInstance_Set(TVSegmentInstance instance, int segmentID, byte[] data)
    {
        Rtc.GetCurrentDate(out RtcDate date);
        instance.Date = ((uint)date.Year << 24) | (((uint)date.Month & 0xFF) << 16) | (((uint)date.Day & 0xFF) << 8) | (uint)date.Week;
        instance.SegmentID = (byte)segmentID;
        instance.PlayCount = 0;
        Array.Copy(data, instance.Data, DataSize);
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryTVBroadcast);
    }

    // OFFSET 0x0202E4D8
    public static TVSegmentInstance TVSegmentInstance_Get(TVSegmentInstance instance)
    {
        return instance;
    }

    // OFFSET 0x0202E55C
    public static int TVSegmentInstance_GetSegmentID(TVSegmentInstance instance)
    {
        return instance.SegmentID;
    }

    // OFFSET 0x0202E560
    public static void TVSegmentInstance_IncrementPlayCount(TVSegmentInstance instance)
    {
        if (instance.PlayCount < MaxPlayCount)
        {
            instance.PlayCount++;
        }
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryTVBroadcast);
    }

    // OFFSET 0x0202E574
    public static byte[] TVSegmentInstance_GetData(TVSegmentInstance instance)
    {
        return instance.Data;
    }
}

public sealed class TVWifiEpisode
{
    public const int Size = 0x48;
    public const int HeaderSize = 8;
    public const int NameLength = 8;

    private const int NameOffset = 8;
    private const int SegmentOffset = 0x18;

    public readonly byte[] Header = new byte[HeaderSize];
    public readonly ushort[] TrainerName = new ushort[NameLength];
    public readonly TVSegmentInstance Segment = new();

    public void WriteTo(byte[] body, int offset)
    {
        Array.Copy(Header, 0, body, offset, HeaderSize);
        for (int i = 0; i < NameLength; i++)
        {
            Bin.W16(body, offset + NameOffset + i * 2, TrainerName[i]);
        }
        Segment.WriteTo(body, offset + SegmentOffset);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Array.Copy(body, offset, Header, 0, HeaderSize);
        for (int i = 0; i < NameLength; i++)
        {
            TrainerName[i] = (ushort)Bin.U16(body, offset + NameOffset + i * 2);
        }
        Segment.ReadFrom(body, offset + SegmentOffset);
    }

    // OFFSET 0x0202E4C8
    public static ushort[] TVWifiEpisode_GetTrainerName(TVWifiEpisode episode)
    {
        return episode.TrainerName;
    }

    // OFFSET 0x0202E4CC
    public static int TVWifiEpisode_GetHeaderByte6(TVWifiEpisode episode)
    {
        return episode.Header[6];
    }

    // OFFSET 0x0202E4D0
    public static int TVWifiEpisode_GetHeaderByte5(TVWifiEpisode episode)
    {
        return episode.Header[5];
    }

    // OFFSET 0x0202E4D4
    public static TVSegmentInstance TVWifiEpisode_GetSegment(TVWifiEpisode episode)
    {
        return episode.Segment;
    }
}

public sealed class TVBroadcast : ISaveBlock
{
    public const int Size = 0xBC8;
    public const int PlayedSegmentCount = 4;
    public const int OwnSegmentCount = 4;
    public const int ProgramTypeCount = 6;
    public const int ProgramType1 = 1;
    public const int ProgramType2 = 2;
    public const int ProgramType3 = 3;
    public const int WifiIndexFlag = 0x80;

    private const int TimeSlotOffset = 4;
    private const int FinishedOffset = 8;
    private const int Type2SegmentsOffset = 0xC;
    private const int Type3SegmentsOffset = 0xCC;
    private const int Type1SegmentsOffset = 0x18C;
    private const int WifiType2EpisodesOffset = 0x24C;
    private const int WifiType3EpisodesOffset = 0x6CC;
    private const int WifiType1EpisodesOffset = 0x90C;
    private const int WifiType2EpisodeCount = 16;
    private const int WifiType3EpisodeCount = 8;
    private const int WifiType1EpisodeCount = 8;
    private const int ShowcasedPokemonOffset = 0xB4C;
    private const int Section0B56Offset = 0xB56;
    private const int Section0B7AOffset = 0xB7A;
    private const int Section0B7COffset = 0xB7C;
    private const int SafariGameOffset = 0xB80;
    private const int Section0B88Offset = 0xB88;
    private const int Section0BA6Offset = 0xBA6;
    private const int Section0BBCOffset = 0xBBC;
    private const int Section0BC4Offset = 0xBC4;
    private const int TimeSlotMinutes = 15;

    public readonly byte[] PlayedSegments = new byte[PlayedSegmentCount];
    public int ProgramTimeSlot;
    public byte ProgramFinished;
    public readonly TVSegmentInstance[] Type2Segments = NewSegments(OwnSegmentCount);
    public readonly TVSegmentInstance[] Type3Segments = NewSegments(OwnSegmentCount);
    public readonly TVSegmentInstance[] Type1Segments = NewSegments(OwnSegmentCount);
    public readonly TVWifiEpisode[] WifiType2Episodes = NewEpisodes(WifiType2EpisodeCount);
    public readonly TVWifiEpisode[] WifiType3Episodes = NewEpisodes(WifiType3EpisodeCount);
    public readonly TVWifiEpisode[] WifiType1Episodes = NewEpisodes(WifiType1EpisodeCount);
    public readonly byte[] ShowcasedPokemon = new byte[Section0B56Offset - ShowcasedPokemonOffset];
    public readonly byte[] Section0B56 = new byte[Section0B7AOffset - Section0B56Offset];
    public readonly byte[] Section0B7A = new byte[Section0B7COffset - Section0B7AOffset];
    public readonly byte[] Section0B7C = new byte[SafariGameOffset - Section0B7COffset];
    public readonly byte[] SafariGame = new byte[Section0B88Offset - SafariGameOffset];
    public readonly byte[] Section0B88 = new byte[Section0BA6Offset - Section0B88Offset];
    public readonly byte[] Section0BA6 = new byte[Section0BBCOffset - Section0BA6Offset];
    public readonly byte[] Section0BBC = new byte[Section0BC4Offset - Section0BBCOffset];
    public readonly byte[] Section0BC4 = new byte[Size - Section0BC4Offset];

    private static TVSegmentInstance[] NewSegments(int count)
    {
        TVSegmentInstance[] segments = new TVSegmentInstance[count];
        for (int i = 0; i < count; i++)
        {
            segments[i] = new TVSegmentInstance();
        }
        return segments;
    }

    private static TVWifiEpisode[] NewEpisodes(int count)
    {
        TVWifiEpisode[] episodes = new TVWifiEpisode[count];
        for (int i = 0; i < count; i++)
        {
            episodes[i] = new TVWifiEpisode();
        }
        return episodes;
    }

    private static void WriteSegments(TVSegmentInstance[] segments, byte[] body, int offset)
    {
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i].WriteTo(body, offset + i * TVSegmentInstance.Size);
        }
    }

    private static void ReadSegments(TVSegmentInstance[] segments, byte[] body, int offset)
    {
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i].ReadFrom(body, offset + i * TVSegmentInstance.Size);
        }
    }

    private static void WriteEpisodes(TVWifiEpisode[] episodes, byte[] body, int offset)
    {
        for (int i = 0; i < episodes.Length; i++)
        {
            episodes[i].WriteTo(body, offset + i * TVWifiEpisode.Size);
        }
    }

    private static void ReadEpisodes(TVWifiEpisode[] episodes, byte[] body, int offset)
    {
        for (int i = 0; i < episodes.Length; i++)
        {
            episodes[i].ReadFrom(body, offset + i * TVWifiEpisode.Size);
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        Array.Copy(PlayedSegments, 0, body, offset, PlayedSegmentCount);
        Bin.W32(body, offset + TimeSlotOffset, (uint)ProgramTimeSlot);
        body[offset + FinishedOffset] = ProgramFinished;
        WriteSegments(Type2Segments, body, offset + Type2SegmentsOffset);
        WriteSegments(Type3Segments, body, offset + Type3SegmentsOffset);
        WriteSegments(Type1Segments, body, offset + Type1SegmentsOffset);
        WriteEpisodes(WifiType2Episodes, body, offset + WifiType2EpisodesOffset);
        WriteEpisodes(WifiType3Episodes, body, offset + WifiType3EpisodesOffset);
        WriteEpisodes(WifiType1Episodes, body, offset + WifiType1EpisodesOffset);
        Array.Copy(ShowcasedPokemon, 0, body, offset + ShowcasedPokemonOffset, ShowcasedPokemon.Length);
        Array.Copy(Section0B56, 0, body, offset + Section0B56Offset, Section0B56.Length);
        Array.Copy(Section0B7A, 0, body, offset + Section0B7AOffset, Section0B7A.Length);
        Array.Copy(Section0B7C, 0, body, offset + Section0B7COffset, Section0B7C.Length);
        Array.Copy(SafariGame, 0, body, offset + SafariGameOffset, SafariGame.Length);
        Array.Copy(Section0B88, 0, body, offset + Section0B88Offset, Section0B88.Length);
        Array.Copy(Section0BA6, 0, body, offset + Section0BA6Offset, Section0BA6.Length);
        Array.Copy(Section0BBC, 0, body, offset + Section0BBCOffset, Section0BBC.Length);
        Array.Copy(Section0BC4, 0, body, offset + Section0BC4Offset, Section0BC4.Length);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Array.Copy(body, offset, PlayedSegments, 0, PlayedSegmentCount);
        ProgramTimeSlot = (int)Bin.U32(body, offset + TimeSlotOffset);
        ProgramFinished = body[offset + FinishedOffset];
        ReadSegments(Type2Segments, body, offset + Type2SegmentsOffset);
        ReadSegments(Type3Segments, body, offset + Type3SegmentsOffset);
        ReadSegments(Type1Segments, body, offset + Type1SegmentsOffset);
        ReadEpisodes(WifiType2Episodes, body, offset + WifiType2EpisodesOffset);
        ReadEpisodes(WifiType3Episodes, body, offset + WifiType3EpisodesOffset);
        ReadEpisodes(WifiType1Episodes, body, offset + WifiType1EpisodesOffset);
        Array.Copy(body, offset + ShowcasedPokemonOffset, ShowcasedPokemon, 0, ShowcasedPokemon.Length);
        Array.Copy(body, offset + Section0B56Offset, Section0B56, 0, Section0B56.Length);
        Array.Copy(body, offset + Section0B7AOffset, Section0B7A, 0, Section0B7A.Length);
        Array.Copy(body, offset + Section0B7COffset, Section0B7C, 0, Section0B7C.Length);
        Array.Copy(body, offset + SafariGameOffset, SafariGame, 0, SafariGame.Length);
        Array.Copy(body, offset + Section0B88Offset, Section0B88, 0, Section0B88.Length);
        Array.Copy(body, offset + Section0BA6Offset, Section0BA6, 0, Section0BA6.Length);
        Array.Copy(body, offset + Section0BBCOffset, Section0BBC, 0, Section0BBC.Length);
        Array.Copy(body, offset + Section0BC4Offset, Section0BC4, 0, Section0BC4.Length);
    }

    // OFFSET 0x0202E2D4
    public static void TVBroadcast_Init(TVBroadcast broadcast)
    {
        broadcast.ReadFrom(new byte[Size], 0);
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryTVBroadcast);
    }

    // OFFSET 0x0202E2EC
    public static void TVBroadcast_ClearOwnSegments(TVBroadcast broadcast)
    {
        for (int i = 0; i < OwnSegmentCount; i++)
        {
            broadcast.Type2Segments[i].Clear();
            broadcast.Type3Segments[i].Clear();
            broadcast.Type1Segments[i].Clear();
        }
        TVBroadcast_ClearWatchProgress(broadcast);
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryTVBroadcast);
    }

    // OFFSET 0x0202E324
    public static void TVBroadcast_UpdateProgramTimeSlot(TVBroadcast broadcast, int deltaMinutes, int currentMinute)
    {
        if (broadcast.ProgramTimeSlot > deltaMinutes)
        {
            broadcast.ProgramTimeSlot -= deltaMinutes;
        }
        else
        {
            broadcast.ProgramTimeSlot = TimeSlotMinutes - currentMinute % TimeSlotMinutes;
            if (broadcast.ProgramTimeSlot == 0)
            {
                broadcast.ProgramTimeSlot = TimeSlotMinutes;
            }
            broadcast.ProgramFinished = 0;
            for (int i = 0; i < PlayedSegmentCount; i++)
            {
                broadcast.PlayedSegments[i] = 0;
            }
        }
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryTVBroadcast);
    }

    // OFFSET 0x0202E35C
    public static void TVBroadcast_ClearWatchProgress(TVBroadcast broadcast)
    {
        TVBroadcast_SetProgramFinished(broadcast, 0);
        TVBroadcast_ClearPlayedSegments(broadcast);
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryTVBroadcast);
    }

    // OFFSET 0x0202E374
    public static void TVBroadcast_SetProgramFinished(TVBroadcast broadcast, int finished)
    {
        broadcast.ProgramFinished = (byte)finished;
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryTVBroadcast);
    }

    // OFFSET 0x0202E380
    public static int TVBroadcast_IsProgramFinished(TVBroadcast broadcast)
    {
        return broadcast.ProgramFinished;
    }

    // OFFSET 0x0202E384
    public static void TVBroadcast_SetPlayedSegment(TVBroadcast broadcast, int segmentID)
    {
        for (int i = 0; i < PlayedSegmentCount; i++)
        {
            if (broadcast.PlayedSegments[i] == 0)
            {
                broadcast.PlayedSegments[i] = (byte)segmentID;
                SaveData.SaveData_SetChecksum(SaveDataTable.EntryTVBroadcast);
                return;
            }
        }
        ErrorHandling.GF_AssertFail();
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryTVBroadcast);
    }

    // OFFSET 0x0202E3AC
    private static void TVBroadcast_ClearPlayedSegments(TVBroadcast broadcast)
    {
        for (int i = 0; i < PlayedSegmentCount; i++)
        {
            broadcast.PlayedSegments[i] = 0;
        }
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryTVBroadcast);
    }

    // OFFSET 0x0202E3C4
    public static bool TVBroadcast_IsPlayedSegment(TVBroadcast broadcast, int segmentID)
    {
        for (int i = 0; i < PlayedSegmentCount; i++)
        {
            if (segmentID == broadcast.PlayedSegments[i])
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0202E3DC
    public static int TVBroadcast_CountPlayedSegments(TVBroadcast broadcast)
    {
        int count = 0;
        for (int i = 0; i < PlayedSegmentCount; i++)
        {
            if (broadcast.PlayedSegments[i] != 0)
            {
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x0202E43C
    public static bool TVBroadcast_SaveSegmentData(TVBroadcast broadcast, int programType, int segmentID, byte[] data)
    {
        TVSegmentInstance[] segments = TVBroadcast_GetOwnSegments(broadcast, programType)!;
        for (int i = 0; i < OwnSegmentCount; i++)
        {
            if (segmentID == segments[i].SegmentID)
            {
                if (segments[i].PlayCount >= TVSegmentInstance.MaxPlayCount)
                {
                    TVSegmentInstance.TVSegmentInstance_Set(segments[i], segmentID, data);
                    return true;
                }
                return false;
            }
        }
        for (int i = 0; i < OwnSegmentCount; i++)
        {
            if (segments[i].SegmentID == 0)
            {
                TVSegmentInstance.TVSegmentInstance_Set(segments[i], segmentID, data);
                return true;
            }
        }
        for (int i = 0; i < OwnSegmentCount; i++)
        {
            if (segments[i].PlayCount >= TVSegmentInstance.MaxPlayCount)
            {
                TVSegmentInstance.TVSegmentInstance_Set(segments[i], segmentID, data);
                return true;
            }
        }
        SaveData.SaveData_SetChecksum(SaveDataTable.EntryTVBroadcast);
        return false;
    }

    // OFFSET 0x0202E4DC
    private static TVSegmentInstance[]? TVBroadcast_GetOwnSegments(TVBroadcast broadcast, int programType)
    {
        TVSegmentInstance[]? segments = null;
        switch (programType)
        {
            case 0:
                break;
            case ProgramType1:
                segments = broadcast.Type1Segments;
                break;
            case ProgramType2:
                segments = broadcast.Type2Segments;
                break;
            case ProgramType3:
                segments = broadcast.Type3Segments;
                break;
            case 4:
            case 5:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return segments;
    }

    // OFFSET 0x0202E518
    private static TVWifiEpisode[]? TVBroadcast_GetWifiEpisodes(TVBroadcast broadcast, int programType)
    {
        TVWifiEpisode[]? episodes = null;
        switch (programType)
        {
            case 0:
                break;
            case ProgramType1:
                episodes = broadcast.WifiType1Episodes;
                break;
            case ProgramType2:
                episodes = broadcast.WifiType2Episodes;
                break;
            case ProgramType3:
                episodes = broadcast.WifiType3Episodes;
                break;
            case 4:
            case 5:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return episodes;
    }

    // OFFSET 0x0202E578
    private static int TVBroadcast_FindOwnSegments(TVSegmentInstance[] segments, int count, int segmentID, bool played, byte[] indices)
    {
        int found = 0;
        for (int i = 0; i < count; i++)
        {
            if (segmentID != segments[i].SegmentID)
            {
                continue;
            }
            byte playCount = segments[i].PlayCount;
            if (played && playCount != 0)
            {
                indices[found] = (byte)(i + 1);
                found++;
            }
            else if (!played && playCount == 0)
            {
                indices[found] = (byte)(i + 1);
                found++;
            }
        }
        return found;
    }

    // OFFSET 0x0202E5B8
    private static int TVBroadcast_FindWifiSegments(TVWifiEpisode[] episodes, int count, int segmentID, bool played, byte[] indices)
    {
        int found = 0;
        for (int i = 0; i < count; i++)
        {
            if (segmentID != episodes[i].Segment.SegmentID)
            {
                continue;
            }
            byte playCount = episodes[i].Segment.PlayCount;
            if (played && playCount != 0)
            {
                indices[found] = (byte)TVBroadcast_MakeWifiSegmentIndex(i + 1);
                found++;
            }
            else if (!played && playCount == 0)
            {
                indices[found] = (byte)TVBroadcast_MakeWifiSegmentIndex(i + 1);
                found++;
            }
        }
        return found;
    }

    // OFFSET 0x0202E614
    public static int TVBroadcast_FindSegments(TVBroadcast broadcast, int programType, int segmentID, bool own, bool played, byte[] indices)
    {
        switch (programType)
        {
            case ProgramType2:
                if (own)
                {
                    return TVBroadcast_FindOwnSegments(broadcast.Type2Segments, OwnSegmentCount, segmentID, played, indices);
                }
                return TVBroadcast_FindWifiSegments(broadcast.WifiType2Episodes, WifiType2EpisodeCount, segmentID, played, indices);
            case ProgramType3:
                if (own)
                {
                    return TVBroadcast_FindOwnSegments(broadcast.Type3Segments, OwnSegmentCount, segmentID, played, indices);
                }
                return TVBroadcast_FindWifiSegments(broadcast.WifiType3Episodes, WifiType3EpisodeCount, segmentID, played, indices);
            case ProgramType1:
                if (own)
                {
                    return TVBroadcast_FindOwnSegments(broadcast.Type1Segments, OwnSegmentCount, segmentID, played, indices);
                }
                return TVBroadcast_FindWifiSegments(broadcast.WifiType1Episodes, WifiType1EpisodeCount, segmentID, played, indices);
        }
        return 0;
    }

    // OFFSET 0x0202E6B0
    public static bool TVBroadcast_CanSaveSegment(TVBroadcast broadcast, int programType, int segmentID)
    {
        byte[] indices = new byte[OwnSegmentCount];
        if (TVBroadcast_FindSegments(broadcast, programType, segmentID, true, false, indices) != 0)
        {
            return false;
        }
        if (TVBroadcast_FindSegments(broadcast, programType, segmentID, true, true, indices) != 0)
        {
            for (int i = 0; i < OwnSegmentCount && indices[i] != 0; i++)
            {
                TVSegmentInstance instance = TVBroadcast_GetOwnSegment(broadcast, programType, indices[i]);
                if (instance.PlayCount >= TVSegmentInstance.MaxPlayCount)
                {
                    return true;
                }
            }
            return false;
        }
        for (int i = 1; i <= OwnSegmentCount; i++)
        {
            TVSegmentInstance instance = TVBroadcast_GetOwnSegment(broadcast, programType, i);
            if (instance.SegmentID == 0 || instance.PlayCount >= TVSegmentInstance.MaxPlayCount)
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0202E768
    public static TVSegmentInstance TVBroadcast_GetOwnSegment(TVBroadcast broadcast, int programType, int index)
    {
        if (!TVBroadcast_IsOwnSegmentIndex(index))
        {
            ErrorHandling.GF_AssertFail();
        }
        TVSegmentInstance[] segments = TVBroadcast_GetOwnSegments(broadcast, programType)!;
        return segments[(index & ~WifiIndexFlag) - 1];
    }

    // OFFSET 0x0202E794
    public static TVWifiEpisode TVBroadcast_GetWifiEpisode(TVBroadcast broadcast, int programType, int index)
    {
        if (TVBroadcast_IsOwnSegmentIndex(index))
        {
            ErrorHandling.GF_AssertFail();
        }
        TVWifiEpisode[] episodes = TVBroadcast_GetWifiEpisodes(broadcast, programType)!;
        return episodes[(index & ~WifiIndexFlag) - 1];
    }

    // OFFSET 0x0202E7C0
    public static bool TVBroadcast_IsOwnSegmentIndex(int index)
    {
        if ((index & WifiIndexFlag) == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202E7D0
    public static int TVBroadcast_MakeWifiSegmentIndex(int index)
    {
        return index | WifiIndexFlag;
    }

    // OFFSET 0x0202E7D8
    public static byte[] TVBroadcast_GetShowcasedPokemon(TVBroadcast broadcast)
    {
        return broadcast.ShowcasedPokemon;
    }

    // OFFSET 0x0202E7E4
    public static byte[] TVBroadcast_GetSection0B56(TVBroadcast broadcast)
    {
        return broadcast.Section0B56;
    }

    // OFFSET 0x0202E7F0
    public static byte[] TVBroadcast_GetSection0B7A(TVBroadcast broadcast)
    {
        return broadcast.Section0B7A;
    }

    // OFFSET 0x0202E7FC
    public static byte[] TVBroadcast_GetSection0B7C(TVBroadcast broadcast)
    {
        return broadcast.Section0B7C;
    }

    // OFFSET 0x0202E808
    public static byte[] TVBroadcast_GetSafariGameData(TVBroadcast broadcast)
    {
        return broadcast.SafariGame;
    }

    // OFFSET 0x0202E810
    public static byte[] TVBroadcast_GetSection0B88(TVBroadcast broadcast)
    {
        return broadcast.Section0B88;
    }

    // OFFSET 0x0202E81C
    public static byte[] TVBroadcast_GetSection0BA6(TVBroadcast broadcast)
    {
        return broadcast.Section0BA6;
    }

    // OFFSET 0x0202E828
    public static byte[] TVBroadcast_GetSection0BBC(TVBroadcast broadcast)
    {
        return broadcast.Section0BBC;
    }

    // OFFSET 0x0202E834
    public static byte[] TVBroadcast_GetSection0BC4(TVBroadcast broadcast)
    {
        return broadcast.Section0BC4;
    }
}
