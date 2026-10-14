using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public struct OverlayInfo
{
    public uint Id;
    public uint RamAddress;
    public uint RamSize;
    public uint BssSize;
}

public static class OverlaySystem
{
    private const int EntrySize = 32;

    private static readonly HashSet<int> loaded = new();
    private static readonly Dictionary<int, Action> initializers = new();

    static OverlaySystem()
    {
        OverlayInitializers.Register();
    }
    private static byte[]? table;
    private static uint defaultDma;

    private static byte[] Table
    {
        get
        {
            if (table == null)
            {
                byte[] header = RomData.Rom.Read(0, 0x60);
                table = RomData.Rom.Read(Bin.U32(header, 0x50), Bin.I32(header, 0x54));
            }
            return table;
        }
    }

    public static IReadOnlyCollection<int> Loaded => loaded;

    private static readonly Dictionary<int, byte[]> images = new();

    private static byte[] DecompressImage(byte[] data)
    {
        int length = data.Length;
        if (length < 8)
        {
            return data;
        }
        uint extra = Bin.U32(data, length - 4);
        if (extra == 0)
        {
            return data;
        }
        uint packed = Bin.U32(data, length - 8);
        int headerSize = (int)(packed >> 24);
        int encodedSize = (int)(packed & 0xFFFFFF);
        byte[] result = new byte[length + extra];
        Array.Copy(data, result, length);
        int start = length - encodedSize;
        int source = length - headerSize;
        int destination = result.Length;
        while (source > start)
        {
            byte flags = result[--source];
            for (int bit = 0; bit < 8 && source > start; bit++)
            {
                if ((flags & 0x80) == 0)
                {
                    result[--destination] = result[--source];
                }
                else
                {
                    int info = result[--source] << 8;
                    info |= result[--source];
                    int count = (info >> 12) + 3;
                    int displacement = (info & 0xFFF) + 3;
                    for (int i = 0; i < count; i++)
                    {
                        destination--;
                        result[destination] = result[destination + displacement];
                    }
                }
                flags <<= 1;
            }
        }
        return result;
    }

    public static byte[] Image(int overlayID)
    {
        if (images.TryGetValue(overlayID, out byte[]? image))
        {
            return image;
        }
        byte[] entries = Table;
        for (int offset = 0; offset + EntrySize <= entries.Length; offset += EntrySize)
        {
            if (Bin.U32(entries, offset) != (uint)overlayID)
            {
                continue;
            }
            byte[] data = RomData.Rom.ReadFile((int)Bin.U32(entries, offset + 0x18));
            if ((Bin.U32(entries, offset + 0x1C) & 0x01000000) != 0)
            {
                data = DecompressImage(data);
            }
            images[overlayID] = data;
            return data;
        }
        throw new InvalidOperationException($"Overlay {overlayID} does not exist");
    }

    public static byte[] ReadData(int overlayID, uint address, int length)
    {
        LoadInfo(out OverlayInfo info, overlayID);
        byte[] result = new byte[length];
        Array.Copy(Image(overlayID), (int)(address - info.RamAddress), result, 0, length);
        return result;
    }

    public static void RegisterInitializer(int overlayID, Action initializer)
    {
        initializers[overlayID] = initializer;
    }

    public static bool LoadInfo(out OverlayInfo info, int overlayID)
    {
        info = default;
        byte[] entries = Table;
        for (int offset = 0; offset + EntrySize <= entries.Length; offset += EntrySize)
        {
            if (Bin.U32(entries, offset) != (uint)overlayID)
            {
                continue;
            }
            info.Id = (uint)overlayID;
            info.RamAddress = Bin.U32(entries, offset + 4);
            info.RamSize = Bin.U32(entries, offset + 8);
            info.BssSize = Bin.U32(entries, offset + 12);
            return true;
        }
        return false;
    }

    public static bool LoadImage(in OverlayInfo info)
    {
        loaded.Add((int)info.Id);
        return true;
    }

    public static void Start(in OverlayInfo info)
    {
        if (initializers.TryGetValue((int)info.Id, out Action? initializer))
        {
            initializer();
        }
    }

    public static bool Load(int overlayID)
    {
        if (!LoadInfo(out OverlayInfo info, overlayID))
        {
            return false;
        }
        LoadImage(info);
        Start(info);
        return true;
    }

    public static bool Unload(int overlayID)
    {
        return loaded.Remove(overlayID);
    }

    public static uint SetDefaultDma(uint value)
    {
        uint previous = defaultDma;
        defaultDma = value;
        return previous;
    }
}
