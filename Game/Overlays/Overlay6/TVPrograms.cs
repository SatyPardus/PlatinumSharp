namespace PlatinumSharp.Game;

public static class TVPrograms
{
    public const int ProgramTypeNoData = 4;
    public const int TVProgramStateFinished = 0;
    public const int TVProgramStateAllPlayed = 1;
    public const int TVProgramStateNonePlayed = 2;
    public const int TVProgramStatePartlyPlayed = 3;

    private const int OverlayID = 6;
    private const int NarcTVSchedule = 0x7E;
    private const int ScheduleMember = 0;
    private const int ProgramMember = 1;
    private const int DaysPerWeek = 7;
    private const int SlotsPerHour = 4;
    private const int MinutesPerSlot = 15;
    private const int HoursPerDay = 24;
    private const int ScheduleHourShift = 0x13;
    private const int MaxProgramID = 9;
    private const int ProgramEntrySize = 0xC;
    private const int ProgramTypeCount = 6;
    private const int ProgramSegmentCount = 11;
    private const int SegmentListSize = 16;
    private const int MaxPlayedSegments = 4;
    private const uint ProgramMessagesTable = 0x02249470;
    private const int ProgramMessageCount = 7;
    private const int MessageWhichTimeOfDay = 0;
    private const int MessageWhichOpening = 1;
    private const int MessageWhichEnding = 2;
    private const int MessageWhichNoSegment = 3;
    private const int TimeOfDayMorning = 0;
    private const int TimeOfDayDay = 1;
    private const uint CommercialConditionFuncs = 0x022494B0;
    private const int CommercialConditionCount = 12;
    private const uint CommercialConditionIndices = 0x02249878;
    private const int CommercialCount = 0x23;
    private const int FirstArrivalCheck = 2;
    private const int BadgesForCommercial = 5;

    // OFFSET 0x022465FC
    public static void FieldSystem_SetTVProgramFinished(FieldSystem fieldSystem)
    {
        TVBroadcast.TVBroadcast_SetProgramFinished((TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(fieldSystem.SaveData!), 1);
    }

    // OFFSET 0x0224660C
    public static int FieldSystem_GetTVProgramState(FieldSystem fieldSystem)
    {
        TVBroadcast broadcast = (TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(fieldSystem.SaveData!);
        if (TVBroadcast.TVBroadcast_IsProgramFinished(broadcast) == 1)
        {
            return TVProgramStateFinished;
        }
        int played = TVBroadcast.TVBroadcast_CountPlayedSegments(broadcast);
        if (played == 0)
        {
            return TVProgramStateNonePlayed;
        }
        if (played == MaxPlayedSegments)
        {
            return TVProgramStateAllPlayed;
        }
        return TVProgramStatePartlyPlayed;
    }

    // OFFSET 0x0224663C
    private static int FieldSystem_GetCurrentTVProgramID(FieldSystem fieldSystem)
    {
        int day = (FieldTime.FieldSystem_GetDayOfWeek(fieldSystem) + 6) % DaysPerWeek;
        int hour = FieldTime.FieldSystem_GetHour(fieldSystem);
        int slot = FieldTime.FieldSystem_GetMinute(fieldSystem) / MinutesPerSlot + (hour + ScheduleHourShift) % HoursPerDay * SlotsPerHour;
        byte[] schedule = new byte[DaysPerWeek];
        NARC.NARC_ReadFromMemberByIndexPair(schedule, NarcTVSchedule, ScheduleMember, DaysPerWeek * slot, DaysPerWeek);
        int programID = schedule[day];
        if (programID == 0 || programID >= 10)
        {
            ErrorHandling.GF_AssertFail();
        }
        return programID;
    }

    // OFFSET 0x0224669C
    private static int TVProgram_GetType(int programID)
    {
        byte[] type = new byte[1];
        NARC.NARC_ReadFromMemberByIndexPair(type, NarcTVSchedule, ProgramMember, (programID - 1) * ProgramEntrySize, 1);
        if (type[0] >= ProgramTypeCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return type[0];
    }

    // OFFSET 0x022466C8
    private static void TVProgram_LoadSegmentIDs(int programID, byte[] segmentIDs)
    {
        NARC.NARC_ReadFromMemberByIndexPair(segmentIDs, NarcTVSchedule, ProgramMember, (programID - 1) * ProgramEntrySize + 1, ProgramSegmentCount);
    }

    // OFFSET 0x022466E8
    private static bool TVProgram_IsSegmentAvailable(FieldSystem fieldSystem, int programID)
    {
        return true;
    }

    // OFFSET 0x022466EC
    private static void TVProgram_GetAvailableSegmentIDs(int programID, FieldSystem fieldSystem, byte[] dest)
    {
        TVBroadcast broadcast = (TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(fieldSystem.SaveData!);
        byte[] segmentIDs = new byte[ProgramSegmentCount];
        TVProgram_LoadSegmentIDs(programID, segmentIDs);
        Array.Clear(dest, 0, ProgramSegmentCount);
        int count = 0;
        for (int i = 0; i < ProgramSegmentCount && segmentIDs[i] != 0; i++)
        {
            if (TVProgram_IsSegmentAvailable(fieldSystem, programID))
            {
                dest[count++] = segmentIDs[i];
            }
        }
    }

    // OFFSET 0x0224673C
    private static int TVSegmentList_PickRandom(FieldSystem fieldSystem, byte[] list)
    {
        byte[] candidates = new byte[ProgramSegmentCount];
        int count = 0;
        for (int i = 0; i < ProgramSegmentCount; i++)
        {
            if (list[i] != 0)
            {
                candidates[count++] = list[i];
            }
        }
        if (count == 0)
        {
            return 0;
        }
        if (count == 1)
        {
            return candidates[0];
        }
        return candidates[MathUtil.MTRNG_Next() % (uint)count];
    }

    // OFFSET 0x0224678C
    private static int TVBroadcast_FindProgramSegments(TVBroadcast broadcast, int programType, int segmentID, bool own, bool played, byte[] dest)
    {
        if (programType != ProgramTypeNoData && programType != ProgramTypeNoData + 1)
        {
            return TVBroadcast.TVBroadcast_FindSegments(broadcast, programType, segmentID, own, played, dest);
        }
        if (played)
        {
            return 0;
        }
        if (!own)
        {
            return 0;
        }
        if (!TVBroadcast.TVBroadcast_IsPlayedSegment(broadcast, segmentID))
        {
            dest[0] = (byte)segmentID;
            return 1;
        }
        return 0;
    }

    // OFFSET 0x022467DC
    private static int TVBroadcast_CollectSegments(int programType, FieldSystem fieldSystem, bool own, bool played, byte[] segmentIDs, byte[] dest)
    {
        TVBroadcast broadcast = (TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(fieldSystem.SaveData!);
        int count = 0;
        for (int s = 0; s < segmentIDs.Length && segmentIDs[s] != 0; s++)
        {
            byte[] found = new byte[SegmentListSize];
            if (TVBroadcast_FindProgramSegments(broadcast, programType, segmentIDs[s], own, played, found) == 0)
            {
                continue;
            }
            for (int i = 0; i < SegmentListSize && found[i] != 0; i++)
            {
                dest[count++] = found[i];
            }
        }
        return count;
    }

    // OFFSET 0x02246844
    private static void TVSegmentList_RemoveIneligible(FieldSystem fieldSystem, int programType, byte[] list)
    {
        for (int i = 0; i < SegmentListSize && list[i] != 0; i++)
        {
            TVEpisode episode = TVEpisodes.TVEpisode_New(fieldSystem, programType, list[i]);
            if (!TVSegments.TVProgram_IsSegmentEligible(programType, fieldSystem, episode))
            {
                list[i] = 0;
            }
            TVEpisodes.TVEpisode_Free(episode);
        }
    }

    // OFFSET 0x02246884
    private static void TVSegmentList_RemovePlayed(FieldSystem fieldSystem, byte[] list)
    {
        TVBroadcast broadcast = (TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(fieldSystem.SaveData!);
        for (int i = 0; list[i] != 0; i++)
        {
            if (TVBroadcast.TVBroadcast_IsPlayedSegment(broadcast, list[i]))
            {
                list[i] = 0;
            }
        }
    }

    // OFFSET 0x022468B0
    public static int FieldSystem_FindTVSegment(FieldSystem fieldSystem, bool own, bool played)
    {
        int programID = FieldSystem_GetCurrentTVProgramID(fieldSystem);
        int programType = TVProgram_GetType(programID);
        byte[] segmentIDs = new byte[ProgramSegmentCount];
        byte[] list = new byte[SegmentListSize];
        TVProgram_GetAvailableSegmentIDs(programID, fieldSystem, segmentIDs);
        if (TVBroadcast_CollectSegments(programType, fieldSystem, own, played, segmentIDs, list) == 0)
        {
            return 0;
        }
        TVSegmentList_RemoveIneligible(fieldSystem, programType, list);
        TVSegmentList_RemovePlayed(fieldSystem, list);
        return TVSegmentList_PickRandom(fieldSystem, list);
    }

    // OFFSET 0x02246920
    public static int FieldSystem_FindAnyTVSegment(FieldSystem fieldSystem)
    {
        int segment = FieldSystem_FindTVSegment(fieldSystem, false, false);
        if (segment == 0)
        {
            segment = FieldSystem_FindTVSegment(fieldSystem, true, false);
        }
        if (segment == 0)
        {
            segment = FieldSystem_FindTVSegment(fieldSystem, false, true);
        }
        if (segment == 0)
        {
            segment = FieldSystem_FindTVSegment(fieldSystem, true, true);
        }
        return segment;
    }

    // OFFSET 0x02246958
    private static byte[] FieldSystem_GetTVProgramMessages(FieldSystem fieldSystem)
    {
        int programID = FieldSystem_GetCurrentTVProgramID(fieldSystem);
        if (programID > MaxProgramID)
        {
            ErrorHandling.GF_AssertFail();
        }
        return RomData.Overlay(OverlayID, (uint)(ProgramMessagesTable + (programID - 1) * ProgramMessageCount), ProgramMessageCount);
    }

    // OFFSET 0x02246978
    public static int FieldSystem_GetTVProgramMessage(FieldSystem fieldSystem, int which)
    {
        TVBroadcast broadcast = (TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(fieldSystem.SaveData!);
        byte[] messages = FieldSystem_GetTVProgramMessages(fieldSystem);
        switch (which)
        {
            case MessageWhichTimeOfDay:
                switch (Rtc.GetTimeOfDay())
                {
                    case TimeOfDayMorning:
                        return messages[0];
                    case TimeOfDayDay:
                        return messages[1];
                }
                return messages[2];
            case MessageWhichOpening:
                return TVBroadcast.TVBroadcast_CountPlayedSegments(broadcast) == 0 ? messages[3] : messages[4];
            case MessageWhichEnding:
                return messages[5];
            case MessageWhichNoSegment:
                return messages[6];
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x022469E0
    public static bool FieldSystem_PrintTVSegment(FieldSystem fieldSystem, StringTemplate template, int segmentID, BufferView bankDest, BufferView messageDest)
    {
        int programType = TVProgram_GetType(FieldSystem_GetCurrentTVProgramID(fieldSystem));
        TVBroadcast.TVBroadcast_SetPlayedSegment((TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(fieldSystem.SaveData!), segmentID);
        TVEpisode episode = TVEpisodes.TVEpisode_New(fieldSystem, programType, segmentID);
        TVEpisodes.TVEpisode_IncrementTimesPlayed(episode);
        messageDest.W16(0, TVSegments.TVProgram_PrintSegment(programType, fieldSystem, template, episode, bankDest));
        TVEpisodes.TVEpisode_Free(episode);
        return true;
    }

    // OFFSET 0x02246A30
    private static bool TVCommercial_Always(FieldSystem fieldSystem)
    {
        return true;
    }

    // OFFSET 0x02246A34
    private static bool TVCommercial_Never(FieldSystem fieldSystem)
    {
        return false;
    }

    private static bool TVCommercial_ArrivedAt(FieldSystem fieldSystem, uint firstArrival)
    {
        return SystemFlags.SystemFlag_HandleFirstArrivalToZone(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), FirstArrivalCheck, firstArrival);
    }

    // OFFSET 0x02246A38
    private static bool TVCommercial_ArrivedAt7(FieldSystem fieldSystem)
    {
        return TVCommercial_ArrivedAt(fieldSystem, 7);
    }

    // OFFSET 0x02246A4C
    private static bool TVCommercial_ArrivedAt2(FieldSystem fieldSystem)
    {
        return TVCommercial_ArrivedAt(fieldSystem, 2);
    }

    // OFFSET 0x02246A60
    private static bool TVCommercial_ArrivedAt9(FieldSystem fieldSystem)
    {
        return TVCommercial_ArrivedAt(fieldSystem, 9);
    }

    // OFFSET 0x02246A74
    private static bool TVCommercial_ArrivedAt10(FieldSystem fieldSystem)
    {
        return TVCommercial_ArrivedAt(fieldSystem, 0xA);
    }

    // OFFSET 0x02246A88
    private static bool TVCommercial_ArrivedAt11(FieldSystem fieldSystem)
    {
        return TVCommercial_ArrivedAt(fieldSystem, 0xB);
    }

    // OFFSET 0x02246A9C
    private static bool TVCommercial_ArrivedAt17(FieldSystem fieldSystem)
    {
        return TVCommercial_ArrivedAt(fieldSystem, 0x11);
    }

    // OFFSET 0x02246AB0
    private static bool TVCommercial_HasFiveBadges(FieldSystem fieldSystem)
    {
        return TrainerInfo.TrainerInfo_BadgeCount(PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!)) >= BadgesForCommercial;
    }

    // OFFSET 0x02246AC8
    private static bool TVCommercial_LocalDexCompleted(FieldSystem fieldSystem)
    {
        return Pokedex.Pokedex_LocalDexCompleted(Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!));
    }

    // OFFSET 0x02246AD8
    private static bool TVCommercial_PokedexObtained(FieldSystem fieldSystem)
    {
        return Pokedex.Pokedex_IsObtained(Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!));
    }

    // OFFSET 0x02246AE8
    private static bool TVCommercial_GameCompleted(FieldSystem fieldSystem)
    {
        return SystemFlags.SystemFlag_CheckGameCompleted(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
    }

    private static bool TVCommercial_CheckCondition(uint function, FieldSystem fieldSystem)
    {
        switch (function & ~1u)
        {
            case 0x02246A30:
                return TVCommercial_Always(fieldSystem);
            case 0x02246A34:
                return TVCommercial_Never(fieldSystem);
            case 0x02246A38:
                return TVCommercial_ArrivedAt7(fieldSystem);
            case 0x02246A4C:
                return TVCommercial_ArrivedAt2(fieldSystem);
            case 0x02246A60:
                return TVCommercial_ArrivedAt9(fieldSystem);
            case 0x02246A74:
                return TVCommercial_ArrivedAt10(fieldSystem);
            case 0x02246A88:
                return TVCommercial_ArrivedAt11(fieldSystem);
            case 0x02246A9C:
                return TVCommercial_ArrivedAt17(fieldSystem);
            case 0x02246AB0:
                return TVCommercial_HasFiveBadges(fieldSystem);
            case 0x02246AC8:
                return TVCommercial_LocalDexCompleted(fieldSystem);
            case 0x02246AD8:
                return TVCommercial_PokedexObtained(fieldSystem);
            case 0x02246AE8:
                return TVCommercial_GameCompleted(fieldSystem);
        }
        throw Unported.Missing(function);
    }

    // OFFSET 0x02246AF8
    private static int FieldSystem_GetAvailableTVCommercials(FieldSystem fieldSystem, byte[] dest)
    {
        uint[] functions = RomData.OverlayU32(OverlayID, CommercialConditionFuncs, CommercialConditionCount);
        bool[] conditions = new bool[CommercialConditionCount];
        for (int i = 0; i < CommercialConditionCount; i++)
        {
            conditions[i] = TVCommercial_CheckCondition(functions[i], fieldSystem);
        }
        byte[] conditionIndices = RomData.Overlay(OverlayID, CommercialConditionIndices, CommercialCount);
        int count = 0;
        for (int i = 0; i < CommercialCount; i++)
        {
            if (conditions[conditionIndices[i]])
            {
                dest[count++] = (byte)i;
            }
        }
        return count;
    }

    // OFFSET 0x02246B40
    public static int FieldSystem_PickTVCommercial(FieldSystem fieldSystem)
    {
        byte[] commercials = new byte[0x24];
        int count = FieldSystem_GetAvailableTVCommercials(fieldSystem, commercials);
        if (count == 0)
        {
            return 0;
        }
        return commercials[MathUtil.MTRNG_Next() % (uint)count];
    }
}
