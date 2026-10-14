using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class PlayTimeManager
{
    private static int sTimerStarted;
    private static PlayTime? sPlayTime;
    private static ulong sLastSeconds;
    private static ulong sUnused;
    private static ulong sStartTick;

    // OFFSET 0x02017428
    public static void PlayTime_FlagNotStarted()
    {
        sTimerStarted = 0;
    }

    // OFFSET 0x02017434
    public static void PlayTime_Start(PlayTime playTime)
    {
        sTimerStarted = 1;
        sUnused = 0;
        sLastSeconds = 0;
        sPlayTime = playTime;
        sStartTick = Timer.Timer_GetCurrentTimestamp();
    }

    // OFFSET 0x02017458
    public static void PlayTime_IncrementTimer()
    {
        if (sTimerStarted == 0)
        {
            return;
        }
        ulong seconds = Timer.Timer_TicksToSeconds(Timer.Timer_GetCurrentTimestamp() - sStartTick);
        if (sLastSeconds < seconds)
        {
            PlayTime.PlayTime_Increment(sPlayTime!, (uint)(seconds - sLastSeconds));
            sLastSeconds = seconds;
        }
    }
}
