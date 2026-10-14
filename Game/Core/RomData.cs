using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class RomData
{
    private static NitroRom? rom;
    private static uint arm9Offset;
    private static uint arm9Address;

    public static NitroRom Rom => rom ?? throw new InvalidOperationException("No ROM has been opened");

    public static void Open(NitroRom source)
    {
        rom = source;
        byte[] header = source.Read(0, 0x40);
        arm9Offset = Bin.U32(header, 0x20);
        arm9Address = Bin.U32(header, 0x28);
    }

    public static byte[] Arm9(uint address, int length)
    {
        return Rom.Read(arm9Offset + (address - arm9Address), length);
    }

    public static byte[] Overlay(int overlayID, uint address, int length)
    {
        return OverlaySystem.ReadData(overlayID, address, length);
    }

    public static uint[] OverlayU32(int overlayID, uint address, int count)
    {
        byte[] raw = Overlay(overlayID, address, count * 4);
        uint[] values = new uint[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = BitConverter.ToUInt32(raw, i * 4);
        }
        return values;
    }

    public static ushort[] OverlayU16(int overlayID, uint address, int count)
    {
        byte[] raw = Overlay(overlayID, address, count * 2);
        ushort[] values = new ushort[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = BitConverter.ToUInt16(raw, i * 2);
        }
        return values;
    }

    public static ushort[] Arm9U16(uint address, int count)
    {
        byte[] raw = Arm9(address, count * 2);
        ushort[] values = new ushort[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = (ushort)Bin.U16(raw, i * 2);
        }
        return values;
    }

    public static uint[] Arm9U32(uint address, int count)
    {
        byte[] raw = Arm9(address, count * 4);
        uint[] values = new uint[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = Bin.U32(raw, i * 4);
        }
        return values;
    }
}
