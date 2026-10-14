namespace PlatinumSharp.Game;

public static class Interrupts
{
    public const uint VBlank = 1 << 0;
    public const uint HBlank = 1 << 1;
    public const uint FifoReceive = 1 << 18;

    private static readonly Dictionary<uint, Action> handlers = new();
    private static uint enabled;

    public static bool MasterEnable { get; set; } = true;

    public static uint Enabled => enabled;

    public static uint VBlankCount { get; private set; }

    public static Action<uint>? WaitHook { get; set; }

    public static void SetHandler(uint mask, Action? handler)
    {
        if (handler == null)
        {
            handlers.Remove(mask);
        }
        else
        {
            handlers[mask] = handler;
        }
    }

    public static uint Enable(uint mask)
    {
        uint previous = enabled;
        enabled |= mask;
        return previous;
    }

    public static uint Disable(uint mask)
    {
        uint previous = enabled;
        enabled &= ~mask;
        return previous;
    }

    public static void Raise(uint mask)
    {
        if ((mask & VBlank) != 0)
        {
            VBlankCount++;
        }
        if (MasterEnable && (enabled & mask) != 0 && handlers.TryGetValue(mask, out Action? handler))
        {
            handler();
        }
    }

    public static void Wait(bool clear, uint mask)
    {
        WaitHook?.Invoke(mask);
        Raise(mask);
    }
}
