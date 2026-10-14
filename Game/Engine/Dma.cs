namespace PlatinumSharp.Game;

public sealed class HBlankTransfer
{
    public Array Source = Array.Empty<uint>();
    public uint Destination;
    public int Size;
    public int UnitBytes;
}

public static class Dma
{
    public const int ChannelCount = 4;

    private static readonly HBlankTransfer?[] hblankTransfers = new HBlankTransfer?[ChannelCount];

    public static event Action<int>? HBlankTransferChanged;

    public static HBlankTransfer? GetHBlankTransfer(int channel)
    {
        return hblankTransfers[channel];
    }

    public static void StartHBlankCopy32(int channel, Array source, uint destination, int size)
    {
        hblankTransfers[channel] = new HBlankTransfer { Source = source, Destination = destination, Size = size, UnitBytes = 4 };
        HBlankTransferChanged?.Invoke(channel);
    }

    public static void StartHBlankCopy16(int channel, Array source, uint destination, int size)
    {
        hblankTransfers[channel] = new HBlankTransfer { Source = source, Destination = destination, Size = size, UnitBytes = 2 };
        HBlankTransferChanged?.Invoke(channel);
    }

    public static void Wait(int channel)
    {
    }

    public static void Stop(int channel)
    {
        hblankTransfers[channel] = null;
        HBlankTransferChanged?.Invoke(channel);
    }
}
