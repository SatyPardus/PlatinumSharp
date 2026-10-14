namespace PlatinumSharp.Nitro;

public static class Lz
{
    public static byte[] Unpack(byte[] data)
    {
        if (data.Length > 4 && (data[0] == 0x10 || data[0] == 0x11) && Bin.Magic(data, 0) == "")
        {
            return TryDecompress(data) ?? data;
        }
        return data;
    }

    public static byte[]? TryDecompress(byte[] d)
    {
        int size = (int)(Bin.U32(d, 0) >> 8);
        if (size <= 0 || size > 0x1000000)
        {
            return null;
        }
        bool extended = d[0] == 0x11;
        byte[] output = new byte[size];
        int src = 4;
        int dst = 0;
        try
        {
            while (dst < size)
            {
                int flags = d[src++];
                for (int bit = 0; bit < 8 && dst < size; bit++, flags <<= 1)
                {
                    if ((flags & 0x80) == 0)
                    {
                        output[dst++] = d[src++];
                        continue;
                    }
                    int b0 = d[src++];
                    int b1 = d[src++];
                    int length;
                    int disp;
                    if (!extended)
                    {
                        length = (b0 >> 4) + 3;
                        disp = (((b0 & 0xF) << 8) | b1) + 1;
                    }
                    else if (b0 >> 4 == 0)
                    {
                        int b2 = d[src++];
                        length = (((b0 & 0xF) << 4) | (b1 >> 4)) + 0x11;
                        disp = (((b1 & 0xF) << 8) | b2) + 1;
                    }
                    else if (b0 >> 4 == 1)
                    {
                        int b2 = d[src++];
                        int b3 = d[src++];
                        length = (((b0 & 0xF) << 12) | (b1 << 4) | (b2 >> 4)) + 0x111;
                        disp = (((b2 & 0xF) << 8) | b3) + 1;
                    }
                    else
                    {
                        length = (b0 >> 4) + 1;
                        disp = (((b0 & 0xF) << 8) | b1) + 1;
                    }
                    if (disp > dst)
                    {
                        return null;
                    }
                    for (int i = 0; i < length && dst < size; i++, dst++)
                    {
                        output[dst] = output[dst - disp];
                    }
                }
            }
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
        return output;
    }
}
