namespace PlatinumSharp.Game;

public static class PersistentAppData
{
    private const int BufferSize = 0x3000;
    private const int NoKey = -1;

    private static readonly byte[] sBuffer = new byte[BufferSize];
    private static int sKey = NoKey;

    // OFFSET 0x02099D44
    public static void PersistentAppData_Clear()
    {
        sKey = NoKey;
    }

    // OFFSET 0x02099D54
    public static void PersistentAppData_Save16(int key, byte[] source, int size)
    {
        if ((uint)size >= BufferSize)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        Array.Copy(source, sBuffer, size);
        sKey = key;
    }

    // OFFSET 0x02099D7C
    public static bool PersistentAppData_Load16(int key, byte[] destination, int size)
    {
        if ((uint)size >= BufferSize)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        if (key != sKey)
        {
            return false;
        }
        Array.Copy(sBuffer, destination, size);
        return true;
    }

    // OFFSET 0x02099DA8
    public static void PersistentAppData_Save32(int key, byte[] source, int size)
    {
        if ((uint)size >= BufferSize)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        Array.Copy(source, sBuffer, size);
        sKey = key;
    }

    // OFFSET 0x02099DD0
    public static bool PersistentAppData_Load32(int key, byte[] destination, int size)
    {
        if ((uint)size >= BufferSize)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        if (key != sKey)
        {
            return false;
        }
        Array.Copy(sBuffer, destination, size);
        return true;
    }
}
