namespace PlatinumSharp.Game;

public sealed partial class MapObject
{
    public OverworldAnimManager? MovementEmoteAnimManager;
    public MapObject? MovementTypePartner;
    public OverworldAnimManager? MovementTypeAnimManager;
    public readonly MapObjectBillboardSlot BerryPatchBillboardSlot = new();
    public readonly MapObjectBillboardState BerryPatchBillboardState = new();
}

public static class MapObjectBuffer
{
    public static int U8(byte[] data, int offset)
    {
        return data[offset];
    }

    public static int S8(byte[] data, int offset)
    {
        return (sbyte)data[offset];
    }

    public static void W8(byte[] data, int offset, int value)
    {
        data[offset] = (byte)value;
    }

    public static int U16(byte[] data, int offset)
    {
        return data[offset] | (data[offset + 1] << 8);
    }

    public static int S16(byte[] data, int offset)
    {
        return (short)(data[offset] | (data[offset + 1] << 8));
    }

    public static void W16(byte[] data, int offset, int value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
    }

    public static int S32(byte[] data, int offset)
    {
        return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
    }

    public static void W32(byte[] data, int offset, int value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
        data[offset + 2] = (byte)(value >> 16);
        data[offset + 3] = (byte)(value >> 24);
    }
}
