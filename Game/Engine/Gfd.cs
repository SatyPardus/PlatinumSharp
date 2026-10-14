namespace PlatinumSharp.Game;

public delegate uint GfdAllocTexVramFunc(uint size, int is4x4Comp, uint option);

public delegate int GfdFreeTexVramFunc(uint key);

public delegate uint GfdAllocPlttVramFunc(uint size, int is4Pltt, uint option);

public delegate int GfdFreePlttVramFunc(uint key);

public static class Gfd
{
    public const uint InvalidKey = 0;

    public static GfdAllocTexVramFunc AllocTexVram { get; set; } = (_, _, _) => InvalidKey;

    public static GfdFreeTexVramFunc FreeTexVram { get; set; } = _ => -1;

    public static GfdAllocPlttVramFunc AllocPlttVram { get; set; } = (_, _, _) => InvalidKey;

    public static GfdFreePlttVramFunc FreePlttVram { get; set; } = _ => -1;

    public static uint MakeTexKey(uint address, uint size, bool is4x4Comp)
    {
        return (address >> 3) | ((size >> 4) << 16) | (is4x4Comp ? 0x80000000u : 0);
    }

    public static uint GetTexKeyAddress(uint key)
    {
        return (key & 0xFFFF) << 3;
    }

    public static uint GetTexKeySize(uint key)
    {
        return ((key >> 16) & 0x7FFF) << 4;
    }

    public static uint MakePlttKey(uint address, uint size)
    {
        return (address >> 3) | ((size >> 3) << 16);
    }

    public static uint GetPlttKeyAddress(uint key)
    {
        return (key & 0xFFFF) << 3;
    }

    public static uint GetPlttKeySize(uint key)
    {
        return (key >> 16) << 3;
    }
}

public sealed class VramTransferTask
{
    public int Type;
    public uint Destination;
    public Array? Source;
    public uint Size;
}

public static class GfdVramTransfer
{
    public const int TexVram = 0;
    public const int TexPalette16 = 1;
    public const int TexPalette4 = 2;
    public const int Bg0CharMain = 4;
    public const int Bg0ScreenMain = 8;
    public const int Bg2BitmapMain = 12;
    public const int ObjPaletteMain = 14;
    public const int BgPaletteMain = 15;
    public const int ObjExtPaletteMain = 16;
    public const int BgExtPaletteMain = 17;
    public const int ObjOamMain = 18;
    public const int ObjCharMain = 19;
    public const int SubOffset = 16;

    private static VramTransferTask[] tasks = Array.Empty<VramTransferTask>();
    private static int count;

    public static event Action<VramTransferTask>? UnhandledTransfer;

    public static void Init(VramTransferTask[] storage, uint capacity)
    {
        tasks = storage;
        count = 0;
    }

    public static bool Register(int type, uint destination, Array source, uint size)
    {
        if (count >= tasks.Length)
        {
            return false;
        }
        VramTransferTask task = tasks[count++];
        task.Type = type;
        task.Destination = destination;
        task.Source = source;
        task.Size = size;
        return true;
    }

    public static void DoTransfer()
    {
        for (int i = 0; i < count; i++)
        {
            Apply(tasks[i]);
        }
        count = 0;
    }

    private static void Apply(VramTransferTask task)
    {
        int type = task.Type;
        int display = 0;
        if (type > ObjCharMain)
        {
            type -= SubOffset;
            display = 1;
        }
        if (type >= Bg0CharMain && type < Bg0CharMain + 4 && task.Source is byte[] characters)
        {
            Video.LoadBgChar(display * 4 + (type - Bg0CharMain), characters, 0, task.Destination, task.Size);
        }
        else if (type >= Bg0ScreenMain && type < Bg0ScreenMain + 4 && task.Source is byte[] screen)
        {
            Video.LoadBgScreen(display * 4 + (type - Bg0ScreenMain), screen, 0, task.Destination, task.Size);
        }
        else if (type == ObjCharMain && task.Source is byte[] objects)
        {
            Video.LoadObjectTiles(display, objects, task.Destination, task.Size);
        }
        else if (type == ObjPaletteMain && task.Source is ushort[] objectColors)
        {
            Video.LoadPaletteRange(display == 0 ? Video.MainObject : Video.SubObject, objectColors, 0, task.Destination, task.Size);
        }
        else if (type == BgPaletteMain && task.Source is ushort[] backgroundColors)
        {
            Video.LoadPaletteRange(display == 0 ? Video.MainBackground : Video.SubBackground, backgroundColors, 0, task.Destination, task.Size);
        }
        else if (type == TexVram && task.Source is byte[] texture)
        {
            Video.LoadTexture(texture, task.Destination, task.Size);
        }
        else if ((type == TexPalette16 || type == TexPalette4) && task.Source is ushort[] textureColors)
        {
            Video.LoadTexturePalette(textureColors, task.Destination, task.Size);
        }
        else
        {
            UnhandledTransfer?.Invoke(task);
        }
    }
}
