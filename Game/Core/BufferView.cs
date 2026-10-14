using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public class BufferView
{
    public readonly byte[] Buffer;
    public readonly int Offset;

    public BufferView(byte[] buffer, int offset)
    {
        Buffer = buffer;
        Offset = offset;
    }

    public byte U8(int position)
    {
        return Buffer[Offset + position];
    }

    public void W8(int position, int value)
    {
        Buffer[Offset + position] = (byte)value;
    }

    public ushort U16(int position)
    {
        return (ushort)Bin.U16(Buffer, Offset + position);
    }

    public void W16(int position, int value)
    {
        Bin.W16(Buffer, Offset + position, (ushort)value);
    }

    public uint U32(int position)
    {
        return Bin.U32(Buffer, Offset + position);
    }

    public void W32(int position, uint value)
    {
        Bin.W32(Buffer, Offset + position, value);
    }

    public void Clear(int position, int length)
    {
        Array.Clear(Buffer, Offset + position, length);
    }

    public void CopyTo(int position, byte[] destination, int destinationOffset, int length)
    {
        Array.Copy(Buffer, Offset + position, destination, destinationOffset, length);
    }

    public void CopyFrom(int position, byte[] source, int sourceOffset, int length)
    {
        Array.Copy(source, sourceOffset, Buffer, Offset + position, length);
    }

    public int GetBits(int position, int shift, int mask)
    {
        return (Buffer[Offset + position] >> shift) & mask;
    }

    public void SetBits(int position, int shift, int mask, int value)
    {
        Buffer[Offset + position] = (byte)((Buffer[Offset + position] & ~(mask << shift)) | ((value & mask) << shift));
    }
}
