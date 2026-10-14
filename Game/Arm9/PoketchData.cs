namespace PlatinumSharp.Game;

public sealed partial class Poketch
{
    private const int ScreenColorShift = 3;
    private const int ScreenColorCount = 8;

    // OFFSET 0x02056830
    public static int Poketch_CurrentAppID(Poketch poketch)
    {
        return (sbyte)poketch.U8(CurrentAppOffset);
    }

    // OFFSET 0x02056838
    public static int Poketch_CycleNextApp(Poketch poketch)
    {
        int start = (sbyte)poketch.U8(CurrentAppOffset);
        int app = start;
        while (true)
        {
            app++;
            if (app >= AppCount)
            {
                app = 0;
            }
            if (app == start || poketch.U8(RegisteredAppsOffset + app) != 0)
            {
                break;
            }
        }
        poketch.W8(CurrentAppOffset, app);
        return (sbyte)poketch.U8(CurrentAppOffset);
    }

    // OFFSET 0x02056860
    public static int Poketch_CyclePrevApp(Poketch poketch)
    {
        int start = (sbyte)poketch.U8(CurrentAppOffset);
        int app = start;
        while (true)
        {
            app--;
            if (app < 0)
            {
                app = AppCount - 1;
            }
            if (app == start || poketch.U8(RegisteredAppsOffset + app) != 0)
            {
                break;
            }
        }
        poketch.W8(CurrentAppOffset, app);
        return (sbyte)poketch.U8(CurrentAppOffset);
    }

    // OFFSET 0x02056888
    public static int Poketch_GetScreenColor(Poketch poketch)
    {
        if (poketch == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return (poketch!.U8(0) & FlagScreenColor) >> ScreenColorShift;
    }

    // OFFSET 0x0205689C
    public static void Poketch_SetScreenColor(Poketch poketch, int color)
    {
        if (poketch == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((uint)color >= ScreenColorCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        poketch!.W8(0, (poketch.U8(0) & ~FlagScreenColor) | (((byte)color << ScreenColorShift) & FlagScreenColor));
    }

    // OFFSET 0x020568C8
    public static uint Poketch_PedometerValue(Poketch poketch)
    {
        return poketch.U32(StepCountOffset);
    }

    // OFFSET 0x020568CC
    public static void Poketch_SetPedometerValue(Poketch poketch, uint value)
    {
        if ((poketch.U8(0) & FlagPedometerRegistered) != 0)
        {
            poketch.W32(StepCountOffset, value);
        }
    }
}
