using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class FieldTime
{
    private const int SecondsPerMinute = 60;

    // OFFSET 0x020559DC
    public static void FieldSystem_UpdateGameTime(FieldSystem fieldSystem)
    {
        GameTime gameTime = SystemData.SaveData_GetGameTime(fieldSystem.SaveData!);
        if (gameTime.Canary != 0)
        {
            Rtc.GetCurrentDateTime(out RtcDate currentDate, out RtcTime currentTime);
            FieldSystem_UpdateGameDay(fieldSystem, gameTime, ref currentDate);
            FieldSystem_UpdateGameMinutes(fieldSystem, gameTime, ref currentDate, ref currentTime);
        }
    }

    // OFFSET 0x02055A14
    private static void FieldSystem_UpdateGameDay(FieldSystem fieldSystem, GameTime gameTime, ref RtcDate currentDate)
    {
        uint day = (uint)RealTimeClock.ConvertDateToDay(currentDate);
        if (day < gameTime.Day)
        {
            gameTime.Day = day;
            return;
        }
        if (day > gameTime.Day)
        {
            FieldSystem_HandleDaysPassed(fieldSystem, (int)(day - gameTime.Day));
            gameTime.Day = day;
        }
    }

    // OFFSET 0x02055A3C
    private static void FieldSystem_UpdateGameMinutes(FieldSystem fieldSystem, GameTime gameTime, ref RtcDate currentDate, ref RtcTime currentTime)
    {
        long currentSeconds = RealTimeClock.ConvertDateTimeToSecond(currentDate, currentTime);
        long savedSeconds = RealTimeClock.ConvertDateTimeToSecond(gameTime.Date, gameTime.Time);
        if (currentSeconds < savedSeconds)
        {
            gameTime.Date = currentDate;
            gameTime.Time = currentTime;
            return;
        }
        int minutes = (int)((currentSeconds - savedSeconds) / SecondsPerMinute);
        if (minutes > 0)
        {
            SystemData.GameTime_DecrementPenalty(gameTime, (uint)minutes);
            FieldSystem_HandleMinutesPassed(fieldSystem, minutes, ref currentTime);
            gameTime.Date = currentDate;
            gameTime.Time = currentTime;
        }
    }

    // OFFSET 0x02055AC0
    private static void FieldSystem_HandleDaysPassed(FieldSystem fieldSystem, int daysPassed)
    {
        Underground.Underground_HandleDailyEvents(FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!, daysPassed);
        FieldSystemVarsFlags.FieldSystem_ClearDailyFlags(fieldSystem);
        FieldTimeEvents.BadgePolish_DecayByDays(fieldSystem.SaveData!, daysPassed);
        RecordMixedRNG.RecordMixedRNG_AdvanceEntries(RecordMixedRNG.SaveData_GetRecordMixedRNG(fieldSystem.SaveData!), (uint)daysPassed);
        SpecialEncounter specialEncounter = SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!);
        uint mixedRecord = RecordMixedRNG.RecordMixedRNG_GetRand(RecordMixedRNG.SaveData_GetRecordMixedRNG(fieldSystem.SaveData!));
        SpecialEncounter.SpecialEncounter_SetMixedRecordDailies(specialEncounter, mixedRecord);
        FieldTimeEvents.Party_UpdatePokerusStatus(Party.SaveData_GetParty(fieldSystem.SaveData!), daysPassed);
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        int deadline = SystemVars.SystemVars_GetNewsPressDeadline(varsFlags);
        if (deadline > daysPassed)
        {
            deadline = (ushort)(deadline - daysPassed);
        }
        else
        {
            deadline = 0;
        }
        SystemVars.SystemVars_SetNewsPressDeadline(varsFlags, (ushort)deadline);
        SystemVars.SystemVars_SynchronizeJubilifeLotteryTrainerID(fieldSystem.SaveData!, (ushort)daysPassed);
        SystemVars.SystemVars_InitDailyRandomLevel(fieldSystem.SaveData!);
        SystemVars.SystemVars_UpdateVillaVisitor(fieldSystem.SaveData!);
        FieldSystemVarsFlags.FieldSystem_ClearDailyHiddenItemFlags(fieldSystem);
        BattleTowerSeed.BattleTowerSeed_Reset(fieldSystem.SaveData!);
        WiFiHistory.WiFiHistory_UpdateGeonetCommunicationMap(WiFiHistory.SaveData_WiFiHistory(fieldSystem.SaveData!));
        FieldTimeEvents.TVBroadcast_HandleDailyEvents(fieldSystem.SaveData!);
    }

    // OFFSET 0x02055B64
    private static void FieldSystem_HandleMinutesPassed(FieldSystem fieldSystem, int minutes, ref RtcTime currentTime)
    {
        BerryPatchManager.BerryPatches_ElapseTime(fieldSystem, minutes);
        SpecialEncounter.SpecialEncounter_DecrementHoneyTreeTimers(fieldSystem.SaveData!, minutes);
        int hasTimePenalty = FieldSystem_HasTimePenalty(fieldSystem);
        Underground.Underground_ProgressGiftPenalty(fieldSystem.SaveData!, minutes, hasTimePenalty);
        TVBroadcast.TVBroadcast_UpdateProgramTimeSlot((TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(fieldSystem.SaveData!), minutes, currentTime.Minute);
        FieldTimeEvents.Party_UpdateShayminFormForTime(Party.SaveData_GetParty(fieldSystem.SaveData!), minutes, ref currentTime);
    }

    // OFFSET 0x02055BA8
    public static int FieldSystem_GetTimeOfDay(FieldSystem fieldSystem)
    {
        GameTime gameTime = SystemData.SaveData_GetGameTime(fieldSystem.SaveData!);
        return Rtc.TimeOfDayForHour(gameTime.Time.Hour);
    }

    // OFFSET 0x02055BB8
    public static int FieldSystem_GetMonth(FieldSystem fieldSystem)
    {
        GameTime gameTime = SystemData.SaveData_GetGameTime(fieldSystem.SaveData!);
        return gameTime.Date.Month;
    }

    // OFFSET 0x02055BC4
    public static int FieldSystem_GetDayOfMonth(FieldSystem fieldSystem)
    {
        GameTime gameTime = SystemData.SaveData_GetGameTime(fieldSystem.SaveData!);
        return gameTime.Date.Day;
    }

    // OFFSET 0x02055BD0
    public static int FieldSystem_GetDayOfWeek(FieldSystem fieldSystem)
    {
        GameTime gameTime = SystemData.SaveData_GetGameTime(fieldSystem.SaveData!);
        return gameTime.Date.Week;
    }

    // OFFSET 0x02055BDC
    public static int FieldSystem_GetHour(FieldSystem fieldSystem)
    {
        GameTime gameTime = SystemData.SaveData_GetGameTime(fieldSystem.SaveData!);
        return gameTime.Time.Hour;
    }

    // OFFSET 0x02055BE8
    public static int FieldSystem_GetMinute(FieldSystem fieldSystem)
    {
        GameTime gameTime = SystemData.SaveData_GetGameTime(fieldSystem.SaveData!);
        return gameTime.Time.Minute;
    }

    // OFFSET 0x02055BF4
    public static void FieldSystem_GetStartTimestamp(FieldSystem fieldSystem, out RtcDate date, out RtcTime time)
    {
        GameTime gameTime = SystemData.SaveData_GetGameTime(fieldSystem.SaveData!);
        RealTimeClock.ConvertSecondToDateTime(out date, out time, gameTime.StartTimestamp);
    }

    // OFFSET 0x02055C10
    public static void FieldSystem_GetFirstCompletionTimestamp(FieldSystem fieldSystem, out RtcDate date, out RtcTime time)
    {
        GameTime gameTime = SystemData.SaveData_GetGameTime(fieldSystem.SaveData!);
        RealTimeClock.ConvertSecondToDateTime(out date, out time, gameTime.FirstCompletionTimestamp);
    }

    // OFFSET 0x02055C2C
    public static void FieldSystem_SetFirstCompletionTimestamp(FieldSystem fieldSystem)
    {
        GameTime gameTime = SystemData.SaveData_GetGameTime(fieldSystem.SaveData!);
        gameTime.FirstCompletionTimestamp = Rtc.GetTimestamp();
    }

    // OFFSET 0x02055C40
    public static int FieldSystem_HasTimePenalty(FieldSystem fieldSystem)
    {
        return SystemData.GameTime_HasPenalty(SystemData.SaveData_GetGameTime(fieldSystem.SaveData!)) ? 1 : 0;
    }
}
