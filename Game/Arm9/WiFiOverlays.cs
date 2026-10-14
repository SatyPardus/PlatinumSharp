namespace PlatinumSharp.Game;

public static class WiFiOverlays
{
    private const int OverlayNetworkBase = 4;
    private const int OverlayNetworkSecure = 0x12;
    private const int OverlayNetworkLogin = 0x3C;
    private const int LoadOverlapping = 2;

    // OFFSET 0x02099550
    public static void WiFiOverlays_LoadBase()
    {
        GameOverlay.Overlay_LoadByID(OverlayNetworkBase, LoadOverlapping);
    }

    // OFFSET 0x02099560
    public static void WiFiOverlays_UnloadBase()
    {
        GameOverlay.Overlay_UnloadByID(OverlayNetworkBase);
    }

    // OFFSET 0x02099570
    public static void WiFiOverlays_LoadSecure()
    {
        GameOverlay.Overlay_LoadByID(OverlayNetworkSecure, LoadOverlapping);
    }

    // OFFSET 0x02099580
    public static void WiFiOverlays_UnloadSecure()
    {
        GameOverlay.Overlay_UnloadByID(OverlayNetworkSecure);
    }

    // OFFSET 0x020995B4
    public static void WiFiOverlays_LoadLogin()
    {
        GameOverlay.Overlay_LoadByID(OverlayNetworkLogin, LoadOverlapping);
    }

    // OFFSET 0x020995C4
    public static void WiFiOverlays_UnloadLogin()
    {
        GameOverlay.Overlay_UnloadByID(OverlayNetworkLogin);
    }
}
