namespace PlatinumSharp.Game;

public sealed class TVEpisode
{
    public const int NameLength = 8;

    public byte SegmentID;
    public byte GameVersion;
    public byte Language;
    public byte Gender;
    public readonly ushort[] TrainerName = new ushort[NameLength];
    public TVSegmentInstance? Instance;
}

public static class TVEpisodes
{
    private const int LanguageEnglish = 2;
    private const int GameVersionPlatinum = 0xC;

    // OFFSET 0x02246444
    private static void TVEpisode_Clear(TVEpisode episode)
    {
        episode.SegmentID = 0;
        episode.GameVersion = 0;
        episode.Language = 0;
        episode.Gender = 0;
        Array.Clear(episode.TrainerName);
        episode.Instance = null;
    }

    // OFFSET 0x02246450
    private static void TVEpisode_SetGender(TVEpisode episode, int gender)
    {
        episode.Gender = (byte)gender;
    }

    // OFFSET 0x02246454
    private static void TVEpisode_SetLanguage(TVEpisode episode, int language)
    {
        episode.Language = (byte)language;
    }

    // OFFSET 0x02246458
    private static void TVEpisode_SetGameVersion(TVEpisode episode, int gameVersion)
    {
        episode.GameVersion = (byte)gameVersion;
    }

    // OFFSET 0x0224645C
    private static void TVEpisode_SetTrainerName(TVEpisode episode, ushort[] name)
    {
        CharCode.CharCode_CopyNumChars(episode.TrainerName, 0, name, 0, TVEpisode.NameLength);
    }

    // OFFSET 0x02246468
    private static void TVEpisode_SetDetails(TVEpisode episode, TVSegmentInstance instance)
    {
        episode.SegmentID = (byte)TVSegmentInstance.TVSegmentInstance_GetSegmentID(instance);
        episode.Instance = instance;
    }

    // OFFSET 0x0224647C
    public static void TVEpisode_IncrementTimesPlayed(TVEpisode episode)
    {
        if (episode.Instance != null)
        {
            TVSegmentInstance.TVSegmentInstance_IncrementPlayCount(episode.Instance);
        }
    }

    // OFFSET 0x0224648C
    public static int TVEpisode_GetGender(TVEpisode episode)
    {
        return episode.Gender;
    }

    // OFFSET 0x02246490
    public static int TVEpisode_GetLanguage(TVEpisode episode)
    {
        return episode.Language;
    }

    // OFFSET 0x02246494
    public static ushort[] TVEpisode_GetTrainerName(TVEpisode episode)
    {
        return episode.TrainerName;
    }

    // OFFSET 0x02246498
    public static byte[] TVEpisode_GetSegment(TVEpisode episode)
    {
        return TVSegmentInstance.TVSegmentInstance_GetData(episode.Instance!);
    }

    // OFFSET 0x022464A4
    public static int TVEpisode_GetSegmentID(TVEpisode episode)
    {
        return episode.SegmentID;
    }

    // OFFSET 0x022464A8
    private static TVEpisode CreateTVEpisodeFromWifiEpisode(FieldSystem fieldSystem, TVWifiEpisode wifiEpisode)
    {
        TVEpisode episode = new TVEpisode();
        TVEpisode_Clear(episode);
        TVEpisode_SetTrainerName(episode, TVWifiEpisode.TVWifiEpisode_GetTrainerName(wifiEpisode));
        TVEpisode_SetLanguage(episode, TVWifiEpisode.TVWifiEpisode_GetHeaderByte6(wifiEpisode));
        TVEpisode_SetGameVersion(episode, TVWifiEpisode.TVWifiEpisode_GetHeaderByte5(wifiEpisode));
        TVEpisode_SetDetails(episode, TVWifiEpisode.TVWifiEpisode_GetSegment(wifiEpisode));
        return episode;
    }

    // OFFSET 0x022464F8
    private static TVEpisode CreateTVEpisodeFromSegmentInstance(FieldSystem fieldSystem, TVSegmentInstance instance)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!);
        TVEpisode episode = new TVEpisode();
        TVEpisode_Clear(episode);
        TVEpisode_SetTrainerName(episode, TrainerInfo.TrainerInfo_Name(trainerInfo));
        TVEpisode_SetGender(episode, (int)TrainerInfo.TrainerInfo_Gender(trainerInfo));
        TVEpisode_SetLanguage(episode, LanguageEnglish);
        TVEpisode_SetGameVersion(episode, GameVersionPlatinum);
        TVEpisode_SetDetails(episode, TVSegmentInstance.TVSegmentInstance_Get(instance));
        return episode;
    }

    // OFFSET 0x02246550
    private static TVEpisode CreateEmptyTVEpisode(FieldSystem fieldSystem, int segmentID)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!);
        TVEpisode episode = new TVEpisode();
        TVEpisode_Clear(episode);
        episode.SegmentID = (byte)segmentID;
        TVEpisode_SetTrainerName(episode, TrainerInfo.TrainerInfo_Name(trainerInfo));
        TVEpisode_SetGender(episode, (int)TrainerInfo.TrainerInfo_Gender(trainerInfo));
        TVEpisode_SetLanguage(episode, LanguageEnglish);
        TVEpisode_SetGameVersion(episode, GameVersionPlatinum);
        episode.Instance = null;
        return episode;
    }

    // OFFSET 0x022465A0
    public static TVEpisode TVEpisode_New(FieldSystem fieldSystem, int programType, int segmentID)
    {
        TVBroadcast broadcast = (TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(fieldSystem.SaveData!);
        if ((uint)(programType - TVPrograms.ProgramTypeNoData) <= 1)
        {
            return CreateEmptyTVEpisode(fieldSystem, segmentID);
        }
        if (!TVBroadcast.TVBroadcast_IsOwnSegmentIndex(segmentID))
        {
            return CreateTVEpisodeFromWifiEpisode(fieldSystem, TVBroadcast.TVBroadcast_GetWifiEpisode(broadcast, programType, segmentID));
        }
        return CreateTVEpisodeFromSegmentInstance(fieldSystem, TVBroadcast.TVBroadcast_GetOwnSegment(broadcast, programType, segmentID));
    }

    // OFFSET 0x022465F4
    public static void TVEpisode_Free(TVEpisode episode)
    {
    }
}
