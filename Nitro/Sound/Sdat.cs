namespace PlatinumSharp.Nitro.Sound;

public sealed record SequenceInfo(int Index, string Name, int FileId, int Bank, int Volume, int Player = 0, int ChannelPriority = 64, int PlayerPriority = 64);

public sealed record BankInfo(int FileId, int[] WaveArcs);

public sealed class Sdat
{
    private readonly NitroRom rom;
    private readonly long start;
    private readonly byte[] fat;
    private readonly List<int> waveArcFiles = new();
    private readonly List<BankInfo?> bankInfos = new();
    private readonly Dictionary<int, Instrument?[]> banks = new();
    private readonly Dictionary<int, Sample[]> waveArcs = new();
    private readonly object gate = new();

    public List<SequenceInfo> Sequences { get; } = new();

    private Sdat(NitroRom rom, long start)
    {
        this.rom = rom;
        this.start = start;
        byte[] header = rom.Read(start, 0x40);
        if (Bin.Magic(header, 0) != "SDAT")
        {
            throw new InvalidDataException("Not an SDAT archive");
        }
        int symbSize = Bin.I32(header, 0x14);
        byte[]? symb = symbSize > 0 ? rom.Read(start + Bin.U32(header, 0x10), symbSize) : null;
        byte[] info = rom.Read(start + Bin.U32(header, 0x18), Bin.I32(header, 0x1C));
        fat = rom.Read(start + Bin.U32(header, 0x20), Bin.I32(header, 0x24));

        foreach ((int index, int entry) in Records(info, 0))
        {
            string name = SymbolName(symb, 0, index) ?? "SEQ_" + index;
            Sequences.Add(new SequenceInfo(index, name, Bin.U16(info, entry), Bin.U16(info, entry + 4), info[entry + 6], info[entry + 9], info[entry + 7], info[entry + 8]));
        }
        foreach ((int index, int entry) in Records(info, 2))
        {
            while (bankInfos.Count <= index)
            {
                bankInfos.Add(null);
            }
            int[] arcs = new int[4];
            for (int k = 0; k < 4; k++)
            {
                int arc = Bin.U16(info, entry + 4 + k * 2);
                arcs[k] = arc == 0xFFFF ? -1 : arc;
            }
            bankInfos[index] = new BankInfo(Bin.U16(info, entry), arcs);
        }
        foreach ((int index, int entry) in Records(info, 3))
        {
            while (waveArcFiles.Count <= index)
            {
                waveArcFiles.Add(-1);
            }
            waveArcFiles[index] = Bin.U16(info, entry);
        }
    }

    public static Sdat Open(NitroRom rom, string path)
    {
        return new Sdat(rom, rom.GetRange(rom.RequireFile(path)).Start);
    }

    private static IEnumerable<(int Index, int Entry)> Records(byte[] info, int record)
    {
        int list = Bin.I32(info, 8 + record * 4);
        int count = Bin.I32(info, list);
        for (int i = 0; i < count; i++)
        {
            int entry = Bin.I32(info, list + 4 + i * 4);
            if (entry != 0)
            {
                yield return (i, entry);
            }
        }
    }

    private static string? SymbolName(byte[]? symb, int record, int index)
    {
        if (symb == null)
        {
            return null;
        }
        int list = Bin.I32(symb, 8 + record * 4);
        if (list == 0 || index >= Bin.I32(symb, list))
        {
            return null;
        }
        int name = Bin.I32(symb, list + 4 + index * 4);
        return name == 0 ? null : Bin.CString(symb, name);
    }

    public byte[] ReadFile(int fileId)
    {
        int record = 12 + fileId * 16;
        return rom.Read(start + Bin.U32(fat, record), Bin.I32(fat, record + 4));
    }

    public int GetWaveArcFileId(int index)
    {
        return index >= 0 && index < waveArcFiles.Count ? waveArcFiles[index] : -1;
    }

    public int GetFileSize(int fileId)
    {
        return Bin.I32(fat, 12 + fileId * 16 + 4);
    }

    public SequenceInfo? FindSequence(int index)
    {
        foreach (SequenceInfo sequence in Sequences)
        {
            if (sequence.Index == index)
            {
                return sequence;
            }
        }
        return null;
    }

    public BankInfo? GetBankInfo(int index)
    {
        return index >= 0 && index < bankInfos.Count ? bankInfos[index] : null;
    }

    public Instrument?[] GetBank(int index)
    {
        lock (gate)
        {
            if (!banks.TryGetValue(index, out Instrument?[]? bank))
            {
                BankInfo? info = GetBankInfo(index);
                bank = info == null ? Array.Empty<Instrument?>() : Instrument.ParseBank(ReadFile(info.FileId));
                banks[index] = bank;
            }
            return bank;
        }
    }

    public Sample[] GetWaveArc(int index)
    {
        lock (gate)
        {
            if (!waveArcs.TryGetValue(index, out Sample[]? samples))
            {
                bool valid = index >= 0 && index < waveArcFiles.Count && waveArcFiles[index] >= 0;
                samples = valid ? Sample.ParseArchive(ReadFile(waveArcFiles[index])) : Array.Empty<Sample>();
                waveArcs[index] = samples;
            }
            return samples;
        }
    }
}
