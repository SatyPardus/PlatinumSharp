using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class FieldOverworldWeather
{
    private const uint DailyWeatherTableAddress = 0x020E98F4;
    private const int DayCount = 366;
    private const int DailyWeatherTypes = 5;
    private const int FirstDailyWeatherType = 0x20;
    private const int February = 2;

    private static byte[]? sDailyWeather;

    // OFFSET 0x0203A944
    public static int FieldSystem_GetWeather(FieldSystem fieldSystem, int mapHeaderID)
    {
        int weather = MapHeader.MapHeader_GetWeatherType(mapHeaderID);
        if (weather < FirstDailyWeatherType)
        {
            return weather;
        }
        GameTime gameTime = SystemData.SaveData_GetGameTime(fieldSystem.SaveData!);
        int day = Rtc.DayNumberForDate(gameTime.Date) - 1;
        if (day < 0 || day >= DayCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((uint)gameTime.Date.Month > February)
        {
            uint year = (uint)gameTime.Date.Year;
            bool leapYear;
            if ((year & 3) == 0 && year % 100 != 0)
            {
                leapYear = true;
            }
            else if (year % 400 == 0)
            {
                leapYear = true;
            }
            else
            {
                leapYear = false;
            }
            if (!leapYear)
            {
                day++;
            }
        }
        if (FieldTime.FieldSystem_HasTimePenalty(fieldSystem) != 0)
        {
            day = 1;
        }
        sDailyWeather ??= RomData.Arm9(DailyWeatherTableAddress, DayCount * DailyWeatherTypes);
        return sDailyWeather[weather - FirstDailyWeatherType + day * DailyWeatherTypes];
    }
}
