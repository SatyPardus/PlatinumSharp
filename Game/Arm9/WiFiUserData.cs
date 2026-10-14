namespace PlatinumSharp.Game;

public static class WiFiUserData
{
    private const uint InitialGameCode = 0x4144414A;

    // OFFSET 0x02038FFC
    public static int WiFi_InitLibrary(int heapID)
    {
        WiFiOverlays.WiFiOverlays_LoadBase();
        WiFiOverlays.WiFiOverlays_LoadLogin();
        int result = Dwc.Init();
        WiFiOverlays.WiFiOverlays_UnloadBase();
        WiFiOverlays.WiFiOverlays_UnloadLogin();
        return result;
    }

    // OFFSET 0x02039034
    public static void WiFiList_InitUserData(WiFiList wiFiList)
    {
        byte[] userData = WiFiList.WiFiList_GetUserData(wiFiList);
        if (!Dwc.CheckUserData(userData))
        {
            Dwc.CreateUserData(userData, InitialGameCode);
            Dwc.ClearDirtyFlag(userData);
        }
    }
}
