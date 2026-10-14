namespace PlatinumSharp.Game;

public static class OverlayInitializers
{
    public static void Register()
    {
        OverlaySystem.RegisterInitializer(26, DigitalWatch.DigitalWatch_RegisterAppFunctions);
    }
}
