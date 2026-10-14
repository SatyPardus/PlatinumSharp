namespace PlatinumSharp.Game;

public static class CardBackup
{
    public const int Size = 0x80000;
    public const int ResultSuccess = 0;
    public const int ResultTimeout = 4;
    public const int ResultError = 6;

    private static readonly byte[] image = CreateBlank();
    private static string? path;

    public static string? FilePath => path;

    public static bool Present { get; set; } = true;

    public static int ResultCode { get; private set; }

    public static bool Busy => false;

    private static byte[] CreateBlank()
    {
        byte[] blank = new byte[Size];
        Array.Fill(blank, (byte)0xFF);
        return blank;
    }

    public static void Open(string? file)
    {
        path = file;
        Array.Fill(image, (byte)0xFF);
        if (file != null && File.Exists(file))
        {
            byte[] stored = File.ReadAllBytes(file);
            Array.Copy(stored, image, Math.Min(stored.Length, Size));
        }
    }

    public static bool Identify(int type)
    {
        return Present;
    }

    public static bool Read(uint address, byte[] destination, int destinationOffset, uint size)
    {
        if (!Present || address + size > Size)
        {
            ResultCode = ResultError;
            return false;
        }
        Array.Copy(image, (int)address, destination, destinationOffset, (int)size);
        ResultCode = ResultSuccess;
        return true;
    }

    public static bool Write(uint address, byte[] source, int sourceOffset, uint size)
    {
        if (!Present || address + size > Size)
        {
            ResultCode = ResultError;
            return false;
        }
        Array.Copy(source, sourceOffset, image, (int)address, (int)size);
        if (path != null)
        {
            File.WriteAllBytes(path, image);
        }
        ResultCode = ResultSuccess;
        return true;
    }
}
