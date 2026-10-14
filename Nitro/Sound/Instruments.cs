namespace PlatinumSharp.Nitro.Sound;

public sealed class Sample
{
    private static readonly int[] IndexTable = { -1, -1, -1, -1, 2, 4, 6, 8 };

    private static readonly int[] StepTable =
    {
        7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21, 23, 25, 28, 31, 34, 37, 41, 45, 50, 55, 60, 66, 73, 80, 88, 97,
        107, 118, 130, 143, 157, 173, 190, 209, 230, 253, 279, 307, 337, 371, 408, 449, 494, 544, 598, 658, 724, 796,
        876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066, 2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428,
        4871, 5358, 5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899, 15289, 16818, 18500, 20350,
        22385, 24623, 27086, 29794, 32767,
    };

    public float[] Data { get; private init; } = Array.Empty<float>();
    public int Rate { get; private init; }
    public bool Loop { get; private init; }
    public int LoopStart { get; private init; }

    public static Sample[] ParseArchive(byte[] d)
    {
        int count = Bin.I32(d, 0x38);
        Sample[] samples = new Sample[count];
        for (int i = 0; i < count; i++)
        {
            int offset = Bin.I32(d, 0x3C + i * 4);
            samples[i] = ParseWave(d, offset);
        }
        return samples;
    }

    private static Sample ParseWave(byte[] d, int o)
    {
        int type = d[o];
        bool loop = d[o + 1] != 0;
        int rate = Bin.U16(d, o + 2);
        int loopWords = Bin.U16(d, o + 6);
        int lengthWords = Bin.I32(d, o + 8);
        int dataStart = o + 12;
        int bytes = Math.Min((loopWords + lengthWords) * 4, d.Length - dataStart);
        float[] data;
        int loopStart;
        if (type == 0)
        {
            data = new float[bytes];
            for (int i = 0; i < bytes; i++)
            {
                data[i] = (sbyte)d[dataStart + i] / 128f;
            }
            loopStart = loopWords * 4;
        }
        else if (type == 1)
        {
            data = new float[bytes / 2];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Bin.S16(d, dataStart + i * 2) / 32768f;
            }
            loopStart = loopWords * 2;
        }
        else
        {
            data = DecodeAdpcm(d, dataStart, bytes);
            loopStart = Math.Max(0, (loopWords * 4 - 4) * 2);
        }
        return new Sample
        {
            Data = data,
            Rate = rate,
            Loop = loop && loopStart < data.Length,
            LoopStart = loopStart,
        };
    }

    private static float[] DecodeAdpcm(byte[] d, int start, int bytes)
    {
        if (bytes < 4)
        {
            return Array.Empty<float>();
        }
        int sample = Bin.S16(d, start);
        int index = Math.Clamp(Bin.U16(d, start + 2) & 0x7F, 0, 88);
        float[] data = new float[(bytes - 4) * 2];
        for (int i = 0; i < data.Length; i++)
        {
            int nibble = (d[start + 4 + (i >> 1)] >> ((i & 1) * 4)) & 0xF;
            int step = StepTable[index];
            int diff = step >> 3;
            if ((nibble & 1) != 0)
            {
                diff += step >> 2;
            }
            if ((nibble & 2) != 0)
            {
                diff += step >> 1;
            }
            if ((nibble & 4) != 0)
            {
                diff += step;
            }
            sample = (nibble & 8) != 0 ? Math.Max(sample - diff, -32767) : Math.Min(sample + diff, 32767);
            index = Math.Clamp(index + IndexTable[nibble & 7], 0, 88);
            data[i] = sample / 32768f;
        }
        return data;
    }
}

public sealed record NoteDef(int Type, int Wave, int WaveArc, int Root, int Attack, int Decay, int Sustain, int Release, int Pan)
{
    public static NoteDef Parse(byte[] d, int o, int type)
    {
        return new NoteDef(type, Bin.U16(d, o), Bin.U16(d, o + 2), d[o + 4], d[o + 5], d[o + 6], d[o + 7], d[o + 8], d[o + 9]);
    }
}

public sealed class Instrument
{
    private readonly int low;
    private readonly int[] upperKeys;
    private readonly NoteDef?[] notes;

    private Instrument(int low, int[] upperKeys, NoteDef?[] notes)
    {
        this.low = low;
        this.upperKeys = upperKeys;
        this.notes = notes;
    }

    public NoteDef? Resolve(int key)
    {
        if (upperKeys.Length > 0)
        {
            for (int i = 0; i < upperKeys.Length; i++)
            {
                if (key <= upperKeys[i])
                {
                    return notes[i];
                }
            }
            return null;
        }
        if (low < 0)
        {
            return notes[0];
        }
        int index = key - low;
        return index >= 0 && index < notes.Length ? notes[index] : null;
    }

    public static Instrument?[] ParseBank(byte[] d)
    {
        int count = Bin.I32(d, 0x38);
        Instrument?[] bank = new Instrument?[count];
        for (int i = 0; i < count; i++)
        {
            int type = d[0x3C + i * 4];
            int o = Bin.U16(d, 0x3D + i * 4);
            if (type == 0)
            {
                continue;
            }
            if (type < 16)
            {
                bank[i] = new Instrument(-1, Array.Empty<int>(), new[] { NoteDef.Parse(d, o, type) });
            }
            else if (type == 16)
            {
                int lo = d[o];
                int hi = d[o + 1];
                NoteDef?[] notes = new NoteDef?[Math.Max(0, hi - lo + 1)];
                for (int k = 0; k < notes.Length; k++)
                {
                    notes[k] = ParseEntry(d, o + 2 + k * 12);
                }
                bank[i] = new Instrument(lo, Array.Empty<int>(), notes);
            }
            else if (type == 17)
            {
                List<int> keys = new();
                for (int k = 0; k < 8 && d[o + k] != 0; k++)
                {
                    keys.Add(d[o + k]);
                }
                NoteDef?[] notes = new NoteDef?[keys.Count];
                for (int k = 0; k < notes.Length; k++)
                {
                    notes[k] = ParseEntry(d, o + 8 + k * 12);
                }
                bank[i] = new Instrument(-1, keys.ToArray(), notes);
            }
        }
        return bank;
    }

    private static NoteDef? ParseEntry(byte[] d, int o)
    {
        int type = Bin.U16(d, o);
        return type == 0 ? null : NoteDef.Parse(d, o + 2, type);
    }
}
