namespace PlatinumSharp.Game;

public class RawSaveBlock : BufferView, ISaveBlock
{
    public readonly int Length;

    public RawSaveBlock(int length)
        : base(new byte[length], 0)
    {
        Length = length;
    }

    public void WriteTo(byte[] body, int offset)
    {
        Array.Copy(Buffer, 0, body, offset, Length);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Array.Copy(body, offset, Buffer, 0, Length);
    }
}
