namespace PlatinumSharp.Game;

public static class PowerManager
{
    public const int ScreenTop = 0;
    public const int ScreenBottom = 1;
    public const int ScreenBoth = 2;

    private static int topBacklight = 1;
    private static int bottomBacklight = 1;

    public static bool PoweredOff { get; private set; }

    public static Action<int>? SleepHook { get; set; }

    public static void GetBackLight(out int top, out int bottom)
    {
        top = topBacklight;
        bottom = bottomBacklight;
    }

    public static void SetBackLight(int screen, int state)
    {
        if (screen == ScreenTop || screen == ScreenBoth)
        {
            topBacklight = state;
        }
        if (screen == ScreenBottom || screen == ScreenBoth)
        {
            bottomBacklight = state;
        }
    }

    public static void GoSleepMode(int trigger, int logic, int keyPattern)
    {
        SleepHook?.Invoke(trigger);
    }

    public static void ForceToPowerOff()
    {
        PoweredOff = true;
    }
}
