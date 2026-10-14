namespace PlatinumSharp.Game;

public delegate void RtcCallback(int result, object? arg);

public static class RealTimeClock
{
    public const int ResultSuccess = 0;

    private static readonly DateTime Epoch = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

    public static Func<DateTime> Now { get; set; } = () => DateTime.Now;

    public static void Init()
    {
    }

    public static int GetDateTimeAsync(out RtcDate date, out RtcTime time, RtcCallback callback, object? arg)
    {
        DateTime now = Now();
        date.Year = now.Year % 100;
        date.Month = now.Month;
        date.Day = now.Day;
        date.Week = (int)now.DayOfWeek;
        time.Hour = now.Hour;
        time.Minute = now.Minute;
        time.Second = now.Second;
        callback(ResultSuccess, arg);
        return ResultSuccess;
    }

    public static int ConvertDateToDay(in RtcDate date)
    {
        if (date.Year < 0 || date.Year > 99 || date.Month < 1 || date.Month > 12
            || date.Day < 1 || date.Day > DateTime.DaysInMonth(2000 + date.Year, date.Month))
        {
            return -1;
        }
        DateTime value = new(2000 + date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
        return (int)(value - Epoch).TotalDays;
    }

    public static long ConvertDateTimeToSecond(in RtcDate date, in RtcTime time)
    {
        if (date.Year < 0 || date.Year > 99 || date.Month < 1 || date.Month > 12
            || date.Day < 1 || date.Day > DateTime.DaysInMonth(2000 + date.Year, date.Month)
            || time.Hour < 0 || time.Hour > 23 || time.Minute < 0 || time.Minute > 59
            || time.Second < 0 || time.Second > 59)
        {
            return -1;
        }
        DateTime value = new(2000 + date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, DateTimeKind.Unspecified);
        return (long)(value - Epoch).TotalSeconds;
    }

    public static void ConvertSecondToDateTime(out RtcDate date, out RtcTime time, long seconds)
    {
        DateTime value = Epoch.AddSeconds(seconds);
        date.Year = value.Year - 2000;
        date.Month = value.Month;
        date.Day = value.Day;
        date.Week = (int)value.DayOfWeek;
        time.Hour = value.Hour;
        time.Minute = value.Minute;
        time.Second = value.Second;
    }
}
