using System.Text;

namespace PlatinumSharp.Nitro;

public sealed class NitroRom : IDisposable
{
    private readonly FileStream stream;
    private readonly object gate = new();
    private readonly byte[] fat;
    private readonly Dictionary<string, int> pathToId = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> paths = new();

    public string Title { get; }
    public string GameCode { get; }
    public int Revision { get; }
    public IReadOnlyList<string> Paths => paths;
    public int FileCount => fat.Length / 8;

    public NitroRom(string path)
    {
        stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        byte[] header = Read(0, 0x200);
        Title = Encoding.ASCII.GetString(header, 0, 12).TrimEnd('\0');
        GameCode = Encoding.ASCII.GetString(header, 0x0C, 4);
        Revision = header[0x1E];
        byte[] fnt = Read(Bin.U32(header, 0x40), Bin.I32(header, 0x44));
        fat = Read(Bin.U32(header, 0x48), Bin.I32(header, 0x4C));
        WalkDirectory(fnt, 0xF000, "");
        paths.Sort(StringComparer.OrdinalIgnoreCase);
    }

    private void WalkDirectory(byte[] fnt, int dirId, string prefix)
    {
        int entry = (dirId & 0xFFF) * 8;
        int pos = Bin.I32(fnt, entry);
        int fileId = Bin.U16(fnt, entry + 4);
        while (true)
        {
            int typeLen = fnt[pos++];
            if (typeLen == 0)
            {
                break;
            }
            int length = typeLen & 0x7F;
            string name = Encoding.ASCII.GetString(fnt, pos, length);
            pos += length;
            if ((typeLen & 0x80) != 0)
            {
                int subId = Bin.U16(fnt, pos);
                pos += 2;
                WalkDirectory(fnt, subId, prefix + "/" + name);
            }
            else
            {
                string full = prefix + "/" + name;
                pathToId[full] = fileId;
                paths.Add(full);
                fileId++;
            }
        }
    }

    public byte[] Read(long offset, int count)
    {
        byte[] buffer = new byte[count];
        lock (gate)
        {
            stream.Position = offset;
            stream.ReadExactly(buffer, 0, count);
        }
        return buffer;
    }

    public int FindFile(string path)
    {
        return pathToId.TryGetValue(path, out int id) ? id : -1;
    }

    public (uint Start, uint End) GetRange(int fileId)
    {
        return (Bin.U32(fat, fileId * 8), Bin.U32(fat, fileId * 8 + 4));
    }

    public int GetSize(string path)
    {
        (uint start, uint end) = GetRange(RequireFile(path));
        return (int)(end - start);
    }

    public byte[] ReadFile(int fileId)
    {
        (uint start, uint end) = GetRange(fileId);
        return Read(start, (int)(end - start));
    }

    public byte[] ReadFile(string path)
    {
        return ReadFile(RequireFile(path));
    }

    public int RequireFile(string path)
    {
        int id = FindFile(path);
        if (id < 0)
        {
            throw new FileNotFoundException("File not found in ROM", path);
        }
        return id;
    }

    public void Dispose()
    {
        stream.Dispose();
    }
}
