using System.Buffers.Binary;
using System.Text;

namespace PlatinumSharp.Nitro;

public static class Bin
{
    public static int U16(byte[] d, int o) => BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(o));

    public static void W16(byte[] d, int o, ushort v) => BinaryPrimitives.WriteUInt16LittleEndian(d.AsSpan(o), v);
    public static void W32(byte[] d, int o, uint v) => BinaryPrimitives.WriteUInt32LittleEndian(d.AsSpan(o), v);

    public static int S16(byte[] d, int o) => BinaryPrimitives.ReadInt16LittleEndian(d.AsSpan(o));

    public static uint U32(byte[] d, int o) => BinaryPrimitives.ReadUInt32LittleEndian(d.AsSpan(o));

    public static int I32(byte[] d, int o) => BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(o));

    public static string Magic(byte[] d, int o)
    {
        if (d.Length < o + 4)
        {
            return "";
        }
        for (int i = 0; i < 4; i++)
        {
            if (d[o + i] < 0x20 || d[o + i] > 0x7E)
            {
                return "";
            }
        }
        return Encoding.ASCII.GetString(d, o, 4);
    }

    public static string CString(byte[] d, int o)
    {
        int end = o;
        while (end < d.Length && d[end] != 0)
        {
            end++;
        }
        return Encoding.ASCII.GetString(d, o, end - o);
    }

    public static int FindSection(byte[] d, string magic)
    {
        if (d.Length < 16)
        {
            return -1;
        }
        int pos = U16(d, 12);
        int count = U16(d, 14);
        for (int i = 0; i < count && pos + 8 <= d.Length; i++)
        {
            if (Magic(d, pos) == magic)
            {
                return pos;
            }
            int size = I32(d, pos + 4);
            if (size <= 0)
            {
                break;
            }
            pos += size;
        }
        return -1;
    }
}
