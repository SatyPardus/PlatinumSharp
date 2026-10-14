using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class Timer
{
    private const int CounterBits = 16;
    private const long OverflowMask = 0x0000FFFFFFFFFFFF;
    private const int PrescalerShift = 6;
    private const ulong TicksPerMillisecond = 0x82EA;
    private const ulong TicksPerSecond = 0x01FF6210;

    private static int sNeedReset;
    private static ulong sOverflowCount;

    // OFFSET 0x02022844
    public static void Timer_Start()
    {
        sOverflowCount = 0;
        sNeedReset = 0;
        HardwareTimer.Restart();
    }

    // OFFSET 0x02022878
    private static void Timer_OnOverflow()
    {
        sOverflowCount++;
        if (sNeedReset != 0)
        {
            HardwareTimer.Restart();
            sNeedReset = 0;
        }
    }

    // OFFSET 0x020228C8
    private static ulong Timer_ReadTimestamp()
    {
        long count = HardwareTimer.ReadCount();
        ulong overflows = (ulong)(count >> CounterBits);
        while (sOverflowCount < overflows)
        {
            Timer_OnOverflow();
        }
        ulong ticks = sOverflowCount & OverflowMask;
        return (ticks << CounterBits) | (ushort)count;
    }

    // OFFSET 0x0202293C
    public static ulong Timer_GetCurrentTimestamp()
    {
        return Timer_ReadTimestamp();
    }

    // OFFSET 0x02022944
    public static ulong Timer_TicksToMilliSeconds(ulong ticks)
    {
        return (ticks << PrescalerShift) / TicksPerMillisecond;
    }

    // OFFSET 0x0202295C
    public static ulong Timer_TicksToSeconds(ulong ticks)
    {
        return (ticks << PrescalerShift) / TicksPerSecond;
    }
}

public static class ErrorHandling
{
    // OFFSET 0x02022974
    public static void GF_AssertFail()
    {
        if (CommManager.CommMan_IsInitialized() != 0)
        {
            if (!Platform.InInterrupt)
            {
                sub_0209B49C();
            }
        }
    }
}
