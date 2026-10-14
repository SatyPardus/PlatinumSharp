namespace PlatinumSharp.Game;

public static class Cartridge
{
    public const uint Interrupt = 1 << 13;

    public static void Init()
    {
    }

    public static bool IsAgbCartridge()
    {
        return false;
    }

    public static bool IsExisting()
    {
        return true;
    }

    public static bool IsPulledOut()
    {
        return false;
    }

    public static uint GetAgbGameCode()
    {
        return 0;
    }

    public static ushort GetAgbMakerCode()
    {
        return 0;
    }

    public static void Enable(bool enable)
    {
    }

    public static bool Read(uint address, byte[] destination, int length)
    {
        return false;
    }

    public static bool Read32(uint address, out uint value)
    {
        value = 0;
        return false;
    }

    public static ushort IdentifyAgbBackup(int type)
    {
        return 1;
    }
}
