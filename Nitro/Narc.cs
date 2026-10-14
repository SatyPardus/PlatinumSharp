namespace PlatinumSharp.Nitro;

public sealed class Narc
{
    private readonly NitroRom rom;
    private readonly byte[] entries;
    private readonly long dataStart;

    public int Count { get; }

    private Narc(NitroRom rom, byte[] entries, int count, long dataStart)
    {
        this.rom = rom;
        this.entries = entries;
        this.dataStart = dataStart;
        Count = count;
    }

    public static Narc Open(NitroRom rom, string path)
    {
        (uint start, uint _) = rom.GetRange(rom.RequireFile(path));
        byte[] header = rom.Read(start, 16);
        if (Bin.Magic(header, 0) != "NARC")
        {
            throw new InvalidDataException(path + " is not a NARC archive");
        }
        long btaf = start + Bin.U16(header, 12);
        byte[] btafHeader = rom.Read(btaf, 12);
        int btafSize = Bin.I32(btafHeader, 4);
        int count = Bin.U16(btafHeader, 8);
        byte[] entries = rom.Read(btaf + 12, count * 8);
        long btnf = btaf + btafSize;
        int btnfSize = Bin.I32(rom.Read(btnf, 8), 4);
        long gmif = btnf + btnfSize;
        return new Narc(rom, entries, count, gmif + 8);
    }

    public int GetSize(int member)
    {
        return (int)(Bin.U32(entries, member * 8 + 4) - Bin.U32(entries, member * 8));
    }

    public byte[] ReadMember(int member)
    {
        return rom.Read(dataStart + Bin.U32(entries, member * 8), GetSize(member));
    }

    public byte[] ReadMemberHead(int member, int count)
    {
        return rom.Read(dataStart + Bin.U32(entries, member * 8), Math.Min(count, GetSize(member)));
    }
}
