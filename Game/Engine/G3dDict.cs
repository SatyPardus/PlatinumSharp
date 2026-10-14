using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class G3dDict
{
    public const int NameLength = 16;

    public static int GetEntryCount(byte[] file, int dict)
    {
        return file[dict + 1];
    }

    public static int GetEntryByIndex(byte[] file, int dict, int index)
    {
        if (index >= file[dict + 1])
        {
            return -1;
        }
        int entries = dict + Bin.U16(file, dict + 6);
        return entries + 4 + Bin.U16(file, entries) * index;
    }

    public static int GetEntryByName(byte[] file, int dict, byte[] name)
    {
        int count = file[dict + 1];
        int entries = dict + Bin.U16(file, dict + 6);
        int names = entries + Bin.U16(file, entries + 2);
        for (int i = 0; i < count; i++)
        {
            if (file.AsSpan(names + i * NameLength, NameLength).SequenceEqual(name))
            {
                return entries + 4 + Bin.U16(file, entries) * i;
            }
        }
        return -1;
    }
}
