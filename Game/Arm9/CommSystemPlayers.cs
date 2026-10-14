namespace PlatinumSharp.Game;

public static partial class CommSystem
{
    public const int MaxConnections = 8;

    // OFFSET 0x02035D78
    public static int CommSys_IsPlayerConnected(ushort netID)
    {
        if (sCommSys == null)
        {
            return 0;
        }
        throw Unported.Partial(0x02035D78);
    }

    // OFFSET 0x02035E18
    public static int CommSys_ConnectedCount()
    {
        int count = 0;
        for (int netID = 0; netID < MaxConnections; netID++)
        {
            if (CommSys_IsPlayerConnected((ushort)netID) != 0)
            {
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x02035E38
    public static int CommSys_IsInitialized()
    {
        if (sCommSys != null && CommManager.CommType_IsWiFi(CommManager.CommMan_GetCommType()))
        {
            return 1;
        }
        return WirelessManager.WirelessMan_IsInitialized() ? 1 : 0;
    }

    // OFFSET 0x0203608C
    public static ushort CommSys_CurNetId()
    {
        if (sCommSys == null)
        {
            return 0;
        }
        throw Unported.Partial(0x0203608C);
    }

    // OFFSET 0x020362F4
    public static int CommSys_GetPlayerSlot(int netID)
    {
        if (sCommSys == null)
        {
            return netID;
        }
        throw Unported.Partial(0x020362F4);
    }
}
