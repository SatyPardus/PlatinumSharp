namespace PlatinumSharp.Game;

public sealed class ApplicationTemplate
{
    private static readonly Dictionary<uint, ApplicationTemplate> known = new();

    public uint Address { get; }
    public ApplicationFunc? Init;
    public ApplicationFunc? Main;
    public ApplicationFunc? Exit;
    public int OverlayID = -1;

    private ApplicationTemplate(uint address)
    {
        Address = address;
    }

    public static readonly Dictionary<uint, uint> Redirects = new();

    public static ApplicationTemplate At(uint address)
    {
        if (Redirects.TryGetValue(address, out uint redirected))
        {
            address = redirected;
        }
        if (!known.TryGetValue(address, out ApplicationTemplate? template))
        {
            template = new ApplicationTemplate(address);
            OverlayApplications.Resolve(template);
            known[address] = template;
        }
        return template;
    }
}

public sealed class ApplicationArgs
{
    public int Unused00;
    public int BootMode;
    public SaveData? SaveData;
}

public sealed class ApplicationState
{
    public int SavedBacklight;
    public int CardPulledOut;
    public int CurrentOverlay;
    public ApplicationManager? CurrentManager;
    public int NextOverlay;
    public ApplicationTemplate? NextTemplate;
    public readonly ApplicationArgs Args = new();
}

public sealed class SystemState
{
    public Action<object?>? VBlankCallback;
    public object? VBlankCallbackData;
    public HBlankCallback? HBlankCallback;
    public object? HBlankCallbackData;
    public Action? Unused10;
    public Action? Unused14;
    public SysTaskManager? MainTaskManager;
    public SysTaskManager? VBlankTaskManager;
    public SysTaskManager? PostVBlankTaskManager;
    public SysTaskManager? PrintTaskManager;
    public int FrameCounter;
    public int VBlankIntrCounter;
    public int ButtonMode;
    public int HeldKeysRaw;
    public int PressedKeysRaw;
    public int PressedKeysRepeatableRaw;
    public int HeldKeys;
    public int PressedKeys;
    public int PressedKeysRepeatable;
    public int AutorepeatTimer;
    public int AutorepeatRate;
    public int AutorepeatDelay;
    public ushort TouchX;
    public ushort TouchY;
    public ushort TouchPressed;
    public ushort TouchHeld;
    public byte TouchAutoSampling;
    public byte WhichScreenIs3D;
    public byte GbaCartridgeVersion;
    public byte InhibitSleep;
    public byte InhibitReset;
    public int ShowTitleScreenIntro;
    public HeapCanary? HeapCanary;
}

public sealed class Window
{
    public object BgConfig = null!;
    public byte BgLayer;
    public byte TilemapLeft;
    public byte TilemapTop;
    public byte Width;
    public byte Height;
    public byte Palette;
    public ushort BaseTile;
    public byte ColorMode;
    public byte[]? Pixels;
}

public struct RtcDate
{
    public int Year;
    public int Month;
    public int Day;
    public int Week;
}

public struct RtcTime
{
    public int Hour;
    public int Minute;
    public int Second;
}

public static class Platform
{
    public static int BootType;
    public static bool IsFolded;
    public static bool HardwareInitialized;
    public static bool InInterrupt;
    public static long OwnerRtcOffset;
    public static readonly byte[] MacAddress = { 0x00, 0x09, 0xBF, 0x00, 0x00, 0x01 };
    public static byte OwnerBirthMonth = 1;
    public static byte OwnerBirthDay = 1;
    public static uint MainArenaUsed;
    public static bool CardEnabled = true;
    public static uint SharedGameCode;
    public static ushort SharedMakerCode = 0x3130;

    public static void Terminate()
    {
        throw new InvalidOperationException("The system was halted");
    }
    public static uint MainExArenaUsed;

    public static bool ReserveMainArena(uint size)
    {
        MainArenaUsed += size;
        return true;
    }

    public static bool ReserveMainExArena(uint size)
    {
        MainExArenaUsed += size;
        return true;
    }

    public static void InitHardware()
    {
        HardwareInitialized = true;
    }

    public static byte[] GetLowEntropyData()
    {
        byte[] data = new byte[32];
        BitConverter.TryWriteBytes(data.AsSpan(0), DateTime.Now.Ticks);
        BitConverter.TryWriteBytes(data.AsSpan(8), Environment.TickCount64);
        return data;
    }
}

public static class Globals
{
    public static readonly ApplicationState sApplication = new();
    public static readonly SystemState gSystem = new();
}
