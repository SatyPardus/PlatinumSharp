namespace PlatinumSharp.Game;

public static class CommSystemField
{
    // OFFSET 0x02035EA8
    public static void CommSys_EnableSendMovementData()
    {
        if (CommSystem.sCommSys != null)
        {
            throw Unported.Partial(0x02035EA8);
        }
    }

    // OFFSET 0x02035EC8
    public static void CommSys_DisableSendMovementData()
    {
        if (CommSystem.sCommSys != null)
        {
            throw Unported.Partial(0x02035EC8);
        }
    }

    // OFFSET 0x02036180
    public static int CommSys_IsAlone()
    {
        if (CommSystem.sCommSys != null)
        {
            throw Unported.Partial(0x02036180);
        }
        return 0;
    }
}

public static class CommPlayerManager
{
    public static object? sCommPlayerManager;

    // OFFSET 0x0205805C
    public static void CommPlayerMan_UpdateField(FieldSystem fieldSystem, int inputEnabled)
    {
        if (sCommPlayerManager == null)
        {
            return;
        }
        throw Unported.Partial(0x0205805C);
    }

    // OFFSET 0x02058C40
    public static int CommPlayerMan_CanProcessFieldInput()
    {
        if (CommSystemField.CommSys_IsAlone() != 0)
        {
            return 1;
        }
        if (sCommPlayerManager == null)
        {
            return 0;
        }
        throw Unported.Partial(0x02058C40);
    }

    // OFFSET 0x020595A4
    public static void CommPlayerMan_SyncLinkPlayerMapObjects()
    {
        if (sCommPlayerManager == null)
        {
            return;
        }
        throw Unported.Partial(0x020595A4);
    }
}

public static class UndergroundManager
{
    public static object? sUndergroundManager;

    // OFFSET 0x0205A080
    public static SecretBase? UndergroundMan_GetOccupiedSecretBase(SaveData saveData)
    {
        if (sUndergroundManager == null)
        {
            return null;
        }
        throw Unported.Partial(0x0205A080);
    }

    // OFFSET 0x0205A0A0
    public static void UndergroundMan_EndBottomScreen(FieldSystem fieldSystem)
    {
        if (sUndergroundManager != null)
        {
            throw Unported.Partial(0x0205A0A0);
        }
    }

    // OFFSET 0x0205A0BC
    public static void UndergroundMan_InitBottomScreen(FieldSystem fieldSystem)
    {
        if (sUndergroundManager != null)
        {
            throw Unported.Partial(0x0205A0BC);
        }
    }
}
