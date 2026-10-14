namespace PlatinumSharp.Game;

public static class WirelessManager
{
    public static object? sWirelessMan;
    public static object? sWirelessDriver;

    // OFFSET 0x020334B8
    public static bool WirelessDriver_IsInitialized()
    {
        if (sWirelessDriver != null)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02033D94
    public static void WirelessMan_SetPause(int pause)
    {
        if (sWirelessMan != null)
        {
            throw Unported.Partial(0x02033D94);
        }
    }

    // OFFSET 0x02033E1C
    public static bool WirelessMan_IsInitialized()
    {
        if (sWirelessMan != null)
        {
            return true;
        }
        return false;
    }
}

public static partial class CommSystem
{
    public static object? sCommSys;
    public static object? sCommQueueWork;

    // OFFSET 0x020349EC
    public static int CommSys_Update()
    {
        CommManager.CommMan_ProcessCallbacks();
        if (sCommSys != null)
        {
            throw Unported.Partial(0x020349EC);
        }
        WirelessManager.WirelessMan_SetPause(0);
        CommManager.CommMan_CheckError(0);
        CommSys_FlushQueueWork();
        return 1;
    }

    // OFFSET 0x02036314
    public static int CommSys_IsVoiceChatActive()
    {
        if (CommManager.CommType_IsWiFi(CommManager.CommMan_GetCommType()))
        {
            throw Unported.Partial(0x02036314);
        }
        return 0;
    }

    // OFFSET 0x020363D0
    public static void CommSys_RequestSuspend()
    {
        if (sCommSys != null)
        {
            throw Unported.Partial(0x020363D0);
        }
    }

    // OFFSET 0x0203650C
    public static void CommSys_FlushQueueWork()
    {
        if (sCommQueueWork != null)
        {
            throw Unported.Partial(0x0203650C);
        }
    }
}

public static class CommManager
{
    public const int CommTypeNone = 1;

    public static object? sCommMan;

    // OFFSET 0x020326EC
    public static bool CommType_IsWiFi(int commType)
    {
        switch (commType)
        {
            case 0x13:
            case 0x14:
            case 0x15:
            case 0x16:
            case 0x17:
            case 0x18:
            case 0x19:
            case 0x1D:
            case 0x21:
            case 0x22:
            case 0x23:
            case 0x24:
                return true;
        }
        return false;
    }

    // OFFSET 0x02036780
    public static int CommMan_IsInitialized()
    {
        if (sCommMan != null)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02036C50
    public static void CommMan_ProcessCallbacks()
    {
        if (sCommMan != null)
        {
            throw Unported.Partial(0x02036C50);
        }
        if (CommMan_IsWiFi() != 0)
        {
            throw Unported.Partial(0x02036C50);
        }
        if (WirelessManager.WirelessMan_IsInitialized())
        {
            throw Unported.Partial(0x02036C50);
        }
    }

    // OFFSET 0x02037DB0
    public static int CommMan_Suspend()
    {
        CommSystem.CommSys_RequestSuspend();
        if (sCommMan == null)
        {
            return 1;
        }
        throw Unported.Partial(0x02037DB0);
    }

    // OFFSET 0x02038938
    public static bool CommMan_IsActive()
    {
        if (sCommMan != null)
        {
            throw Unported.Partial(0x02038938);
        }
        return false;
    }

    // OFFSET 0x0203895C
    public static int CommMan_GetCommType()
    {
        if (sCommMan != null)
        {
            throw Unported.Partial(0x0203895C);
        }
        return CommTypeNone;
    }

    // OFFSET 0x02038EB4
    public static int CommMan_IsInitializedNotPoketch()
    {
        if (sCommMan != null)
        {
            throw Unported.Partial(0x02038EB4);
        }
        return CommSystem.CommSys_IsInitialized();
    }

    // OFFSET 0x020389B8
    public static int CommMan_IsWiFi()
    {
        return CommType_IsWiFi(CommMan_GetCommType()) ? 1 : 0;
    }

    // OFFSET 0x020389D8
    public static int CommMan_GetErrorState()
    {
        if (GameSystem.HeapCanaryOK())
        {
            return 0;
        }
        if (sCommMan != null)
        {
            throw Unported.Partial(0x020389D8);
        }
        return 0;
    }

    // OFFSET 0x02038AE0
    public static int CommMan_SetErrorState(int errorState)
    {
        if (sCommMan != null)
        {
            throw Unported.Partial(0x02038AE0);
        }
        return 0;
    }

    // OFFSET 0x02038A1C
    public static void CommMan_DrawErrorHook(int heapID, BgConfig? bgConfig)
    {
    }

    // OFFSET 0x02038A20
    public static void CommMan_CheckError(int unused)
    {
        if (!CommMan_IsActive())
        {
            return;
        }
        throw Unported.Partial(0x02038A20);
    }
}
