using System.Diagnostics;

namespace PlatinumSharp.Game;

public static class HardwareTimer
{
    public const long SystemClock = 33513982;
    public const int Prescaler = 64;

    private static readonly Stopwatch stopwatch = new();

    public static void Restart()
    {
        stopwatch.Restart();
    }

    public static long ReadCount()
    {
        if (!stopwatch.IsRunning)
        {
            return 0;
        }
        return (long)(stopwatch.Elapsed.TotalSeconds * SystemClock / Prescaler);
    }
}
