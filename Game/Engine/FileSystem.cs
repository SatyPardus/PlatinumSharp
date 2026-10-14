using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class FsFile
{
    internal uint Start;
    internal uint End;
    public uint Position;
    public bool IsOpen;
}

public static class FileSystem
{
    public const int SeekSet = 0;
    public const int SeekCurrent = 1;
    public const int SeekEnd = 2;

    public static int CardBytesPerVBlank = 0x4000;

    private const int CardPageSize = 0x200;

    private static int cardBytesRead;
    private static bool raisingCardVBlank;

    private static void ElapseCardRead(int byteCount)
    {
        if (CardBytesPerVBlank <= 0)
        {
            return;
        }
        cardBytesRead += (byteCount + CardPageSize - 1) & ~(CardPageSize - 1);
        while (cardBytesRead >= CardBytesPerVBlank)
        {
            cardBytesRead -= CardBytesPerVBlank;
            if (!raisingCardVBlank)
            {
                raisingCardVBlank = true;
                try
                {
                    Interrupts.Raise(Interrupts.VBlank);
                }
                finally
                {
                    raisingCardVBlank = false;
                }
            }
        }
    }

    public static void InitFile(FsFile file)
    {
        file.Start = 0;
        file.End = 0;
        file.Position = 0;
        file.IsOpen = false;
    }

    public static bool OpenFile(FsFile file, string path)
    {
        int id = RomData.Rom.FindFile(path.StartsWith('/') ? path : "/" + path);
        if (id < 0)
        {
            return false;
        }
        (file.Start, file.End) = RomData.Rom.GetRange(id);
        file.Position = 0;
        file.IsOpen = true;
        return true;
    }

    public static int GetLength(FsFile file)
    {
        return (int)(file.End - file.Start);
    }

    public static void CloseFile(FsFile file)
    {
        file.IsOpen = false;
    }

    public static void SeekFile(FsFile file, int offset, int origin)
    {
        long position = origin switch
        {
            SeekCurrent => file.Position + offset,
            SeekEnd => file.End - file.Start + offset,
            _ => offset,
        };
        file.Position = (uint)Math.Clamp(position, 0, file.End - file.Start);
    }

    public static int ReadFile(FsFile file, byte[] destination, int destinationOffset, int length)
    {
        int available = (int)(file.End - file.Start - file.Position);
        int count = Math.Min(length, available);
        if (count <= 0)
        {
            return 0;
        }
        byte[] data = RomData.Rom.Read(file.Start + file.Position, count);
        Array.Copy(data, 0, destination, destinationOffset, count);
        file.Position += (uint)count;
        ElapseCardRead(count);
        return count;
    }

    public static ushort ReadU16(FsFile file)
    {
        byte[] buffer = new byte[2];
        ReadFile(file, buffer, 0, 2);
        return (ushort)Bin.U16(buffer, 0);
    }

    public static uint ReadU32(FsFile file)
    {
        byte[] buffer = new byte[4];
        ReadFile(file, buffer, 0, 4);
        return Bin.U32(buffer, 0);
    }
}
