using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class RtcState
{
    public int Initialized;
    public int Busy;
    public int UpdateCounter;
    public int Result;
    public RtcDate Date;
    public RtcTime Time;
    public RtcDate PendingDate;
    public RtcTime PendingTime;

    public void Clear()
    {
        Initialized = 0;
        Busy = 0;
        UpdateCounter = 0;
        Result = 0;
        Date = default;
        Time = default;
        PendingDate = default;
        PendingTime = default;
    }
}

public static class Rtc
{
    public const int TimeOfDayMorning = 0;
    public const int TimeOfDayDay = 1;
    public const int TimeOfDayTwilight = 2;
    public const int TimeOfDayNight = 3;
    public const int TimeOfDayLateNight = 4;

    private const int UpdateInterval = 10;
    private const long MaxTimestamp = 0xBC19137F;

    private static readonly RtcState sRtcState = new();

    private static ushort[]? sDaysBeforeMonth;
    private static byte[]? sTimeOfDayByHour;

    private static ushort[] DaysBeforeMonth => sDaysBeforeMonth ??= RomData.Arm9U16(0x020E53FE, 13);

    private static byte[] TimeOfDayByHour => sTimeOfDayByHour ??= RomData.Arm9(0x020E5418, 24);

    // OFFSET 0x0201378C
    public static void InitRTC()
    {
        RealTimeClock.Init();
        sRtcState.Clear();
        sRtcState.Initialized = 0;
        sRtcState.Busy = 0;
        sRtcState.UpdateCounter = 0;
        RequestDateTime(sRtcState);
    }

    // OFFSET 0x020137C4
    public static void UpdateRTC()
    {
        if (sRtcState.Busy == 0)
        {
            sRtcState.UpdateCounter++;
            if (sRtcState.UpdateCounter > UpdateInterval)
            {
                sRtcState.UpdateCounter = 0;
                RequestDateTime(sRtcState);
            }
        }
    }

    // OFFSET 0x020137EC
    private static void OnDateTimeReceived(int result, object? arg)
    {
        RtcState state = (RtcState)arg!;
        state.Result = result;
        if (result != RealTimeClock.ResultSuccess)
        {
            ErrorHandling.GF_AssertFail();
        }
        state.Initialized = 1;
        state.Date = state.PendingDate;
        state.Time = state.PendingTime;
        state.Busy = 0;
    }

    // OFFSET 0x02013824
    private static void RequestDateTime(RtcState state)
    {
        state.Busy = 1;
        state.Result = RealTimeClock.GetDateTimeAsync(out state.PendingDate, out state.PendingTime, OnDateTimeReceived, state);
        if (state.Result != RealTimeClock.ResultSuccess)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x0201384C
    public static void GetCurrentDateTime(out RtcDate date, out RtcTime time)
    {
        if (sRtcState.Initialized != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        date = sRtcState.Date;
        time = sRtcState.Time;
    }

    // OFFSET 0x02013880
    public static void RTC_GetCurrentTime(out RtcTime time)
    {
        if (sRtcState.Initialized != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        time = sRtcState.Time;
    }

    // OFFSET 0x020138A4
    public static void GetCurrentDate(out RtcDate date)
    {
        if (sRtcState.Initialized != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        date = sRtcState.Date;
    }

    // OFFSET 0x020138C8
    public static int GetSecondsSinceMidnight()
    {
        return sRtcState.Time.Second + (sRtcState.Time.Minute * 60 + sRtcState.Time.Hour * 3600);
    }

    // OFFSET 0x020138EC
    public static long GetTimestamp()
    {
        return RealTimeClock.ConvertDateTimeToSecond(sRtcState.Date, sRtcState.Time);
    }

    // OFFSET 0x02013900
    public static int DayNumberForDate(in RtcDate date)
    {
        int dayNumber = date.Day + DaysBeforeMonth[date.Month];
        if ((uint)date.Month >= 3)
        {
            int year = date.Year;
            if ((year % 4 == 0 && year % 100 != 0) || year % 400 == 0)
            {
                dayNumber++;
            }
        }
        return dayNumber;
    }

    // OFFSET 0x02013948
    public static bool IsNight()
    {
        int timeOfDay = GetTimeOfDay();
        if (timeOfDay == TimeOfDayNight || timeOfDay == TimeOfDayLateNight)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02013960
    public static int GetTimeOfDay()
    {
        RTC_GetCurrentTime(out RtcTime time);
        return TimeOfDayForHour(time.Hour);
    }

    // OFFSET 0x02013974
    public static int TimeOfDayForHour(int hour)
    {
        if (hour < 0 || hour >= 24)
        {
            ErrorHandling.GF_AssertFail();
        }
        return TimeOfDayByHour[hour];
    }

    // OFFSET 0x0201398C
    public static long TimeElapsed(long since, long until)
    {
        uint[] rawDate = RomData.Arm9U32(0x020E53F0, 4);
        uint[] rawTime = RomData.Arm9U32(0x020E53E4, 3);
        RtcDate maxDate = new() { Year = (int)rawDate[0], Month = (int)rawDate[1], Day = (int)rawDate[2], Week = (int)rawDate[3] };
        RtcTime maxTime = new() { Hour = (int)rawTime[0], Minute = (int)rawTime[1], Second = (int)rawTime[2] };
        long maxTimestamp = RealTimeClock.ConvertDateTimeToSecond(maxDate, maxTime);
        if (maxTimestamp != MaxTimestamp)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (since < until)
        {
            return until - since;
        }
        return until + (MaxTimestamp - since);
    }
}
