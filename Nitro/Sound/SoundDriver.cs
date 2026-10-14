namespace PlatinumSharp.Nitro.Sound;

public sealed class SoundChannel
{
    public const int EnvelopeAttack = 0;
    public const int EnvelopeDecay = 1;
    public const int EnvelopeSustain = 2;
    public const int EnvelopeRelease = 3;
    public const int TypePcm = 0;
    public const int TypePsg = 1;
    public const int TypeNoise = 2;
    public const int Silence = -92544;

    public int Index;
    public bool Active;
    public int Type;
    public Sample? Sample;
    public int Duty;
    public int Key;
    public int OriginalKey;
    public int Velocity;
    public int Attack;
    public int Decay;
    public int Sustain;
    public int Release;
    public int EnvelopeState;
    public int Envelope;
    public int Priority;
    public int Length;
    public int InitPan;
    public int UserPan;
    public int PanRange = 127;
    public int UserDecay;
    public int UserDecay2;
    public int UserPitch;
    public int SweepPitch;
    public int SweepLength;
    public int SweepCounter;
    public bool AutoSweep;
    public int LfoTarget;
    public int LfoSpeed;
    public int LfoDepth;
    public int LfoRange;
    public int LfoDelay;
    public int LfoCounter;
    public int LfoDelayCounter;
    public object? Owner;
    public Action<SoundChannel>? Released;
    public double Position;
    public double Step;
    public float GainLeft;
    public float GainRight;
    public float Gain;
    public int NoiseState;
    public float NoiseOutput;
    public bool Ended;
}

public sealed class SoundDriver
{
    public const int ChannelCount = 16;
    public const double FrameRate = 33513982.0 / 64 / 2728;
    private const float OutputGain = 0.4f;
    private const int PsgChannels = 0x3F00;
    private const int NoiseChannels = 0xC000;

    private static readonly int[] ChannelOrder = { 4, 5, 6, 7, 2, 0, 3, 1, 8, 9, 10, 11, 14, 12, 15, 13 };

    private static readonly int[] AttackTable =
    {
        0x00, 0x01, 0x05, 0x0E, 0x1A, 0x26, 0x33, 0x3F, 0x49, 0x54, 0x5C, 0x64, 0x6D, 0x74, 0x7B, 0x7F, 0x84, 0x89, 0x8F,
    };

    public static readonly int[] DecibelSquare = BuildDecibels(400);
    public static readonly int[] Decibel = BuildDecibels(200);
    private static readonly int[] SinTable = BuildSin();

    private readonly SoundChannel[] channels = new SoundChannel[ChannelCount];
    private readonly List<SequencePlayer> players = new();
    private readonly int sampleRate;
    private uint randomState = 0x12345678;
    private double countdown;

    public int MasterVolume { get; set; } = 127;

    public int SampleRate => sampleRate;

    public IReadOnlyList<SoundChannel> Channels => channels;

    public SoundDriver(int sampleRate = 44100)
    {
        this.sampleRate = sampleRate;
        for (int i = 0; i < ChannelCount; i++)
        {
            channels[i] = new SoundChannel { Index = i };
        }
    }

    private static int[] BuildDecibels(int scale)
    {
        int[] table = new int[128];
        table[0] = -32768;
        for (int i = 1; i < 128; i++)
        {
            table[i] = Math.Max(-(scale == 400 ? 722 : 421), (int)Math.Round(scale * Math.Log10(i / 127.0)));
        }
        return table;
    }

    private static int[] BuildSin()
    {
        int[] table = new int[33];
        for (int i = 0; i <= 32; i++)
        {
            table[i] = (int)Math.Round(127 * Math.Sin(i * Math.PI / 64));
        }
        return table;
    }

    private static int SinIndex(int index)
    {
        if (index < 32)
        {
            return SinTable[index];
        }
        if (index < 64)
        {
            return SinTable[64 - index];
        }
        if (index < 96)
        {
            return -SinTable[index - 64];
        }
        return -SinTable[128 - index];
    }

    public int Random()
    {
        randomState = randomState * 1664525 + 1013904223;
        return (int)(randomState >> 16);
    }

    public void Add(SequencePlayer player)
    {
        players.Add(player);
    }

    public void Remove(SequencePlayer player)
    {
        players.Remove(player);
    }

    public int ActiveChannelCount
    {
        get
        {
            int count = 0;
            foreach (SoundChannel channel in channels)
            {
                if (channel.Active)
                {
                    count++;
                }
            }
            return count;
        }
    }

    public static int ConvertAttack(int attack)
    {
        attack = Math.Clamp(attack, 0, 127);
        return attack < 109 ? 255 - attack : AttackTable[127 - attack];
    }

    public static int ConvertFall(int fall)
    {
        fall = Math.Clamp(fall, 0, 127);
        if (fall == 127)
        {
            return 0xFFFF;
        }
        if (fall == 126)
        {
            return 0x3C00;
        }
        if (fall < 50)
        {
            return fall * 2 + 1;
        }
        return 0x1E00 / (126 - fall);
    }

    public SoundChannel? Allocate(int mask, int priority, Action<SoundChannel>? released, object? owner)
    {
        SoundChannel? chosen = null;
        foreach (int index in ChannelOrder)
        {
            if ((mask & (1 << index)) == 0)
            {
                continue;
            }
            SoundChannel candidate = channels[index];
            if (chosen == null)
            {
                chosen = candidate;
                continue;
            }
            if (candidate.Priority > chosen.Priority)
            {
                continue;
            }
            if (candidate.Priority == chosen.Priority && candidate.Gain >= chosen.Gain)
            {
                continue;
            }
            chosen = candidate;
        }
        if (chosen == null || priority < chosen.Priority)
        {
            return null;
        }
        if (chosen.Active)
        {
            Free(chosen);
        }
        chosen.Released = released;
        chosen.Owner = owner;
        chosen.Priority = priority;
        return chosen;
    }

    public void Free(SoundChannel channel)
    {
        Action<SoundChannel>? released = channel.Released;
        channel.Released = null;
        channel.Owner = null;
        channel.Active = false;
        channel.Priority = 0;
        channel.Gain = 0;
        channel.GainLeft = 0;
        channel.GainRight = 0;
        released?.Invoke(channel);
    }

    public void Start(SoundChannel channel, int type, Sample? sample, int duty, int key, int originalKey, int velocity, int length, int attack, int decay, int sustain, int release, int pan)
    {
        channel.Type = type;
        channel.Sample = sample;
        channel.Duty = duty;
        channel.Key = key;
        channel.OriginalKey = originalKey;
        channel.Velocity = velocity;
        channel.Length = length;
        channel.Attack = ConvertAttack(attack);
        channel.Decay = ConvertFall(decay);
        channel.Sustain = Math.Clamp(sustain, 0, 127);
        channel.Release = ConvertFall(release);
        channel.InitPan = pan - 64;
        channel.EnvelopeState = SoundChannel.EnvelopeAttack;
        channel.Envelope = SoundChannel.Silence;
        channel.UserDecay = 0;
        channel.UserDecay2 = 0;
        channel.UserPitch = 0;
        channel.UserPan = 0;
        channel.PanRange = 127;
        channel.SweepPitch = 0;
        channel.SweepLength = 0;
        channel.SweepCounter = 0;
        channel.AutoSweep = true;
        channel.LfoTarget = 0;
        channel.LfoSpeed = 16;
        channel.LfoDepth = 0;
        channel.LfoRange = 1;
        channel.LfoDelay = 0;
        channel.LfoCounter = 0;
        channel.LfoDelayCounter = 0;
        channel.Position = 0;
        channel.Step = 0;
        channel.Gain = 0;
        channel.GainLeft = 0;
        channel.GainRight = 0;
        channel.NoiseState = 0x7FFF;
        channel.NoiseOutput = 0;
        channel.Ended = false;
        channel.Active = true;
    }

    public static void ReleaseChannel(SoundChannel channel)
    {
        channel.EnvelopeState = SoundChannel.EnvelopeRelease;
    }

    private static int UpdateEnvelope(SoundChannel channel)
    {
        switch (channel.EnvelopeState)
        {
            case SoundChannel.EnvelopeAttack:
                channel.Envelope = -((-channel.Envelope * channel.Attack) >> 8);
                if (channel.Envelope == 0)
                {
                    channel.EnvelopeState = SoundChannel.EnvelopeDecay;
                }
                break;
            case SoundChannel.EnvelopeDecay:
            {
                int sustain = DecibelSquare[channel.Sustain] << 7;
                channel.Envelope -= channel.Decay;
                if (channel.Envelope <= sustain)
                {
                    channel.Envelope = sustain;
                    channel.EnvelopeState = SoundChannel.EnvelopeSustain;
                }
                break;
            }
            case SoundChannel.EnvelopeRelease:
                channel.Envelope -= channel.Release;
                break;
        }
        return channel.Envelope >> 7;
    }

    private static int UpdateSweep(SoundChannel channel)
    {
        if (channel.SweepPitch == 0 || channel.SweepCounter >= channel.SweepLength)
        {
            return 0;
        }
        long result = (long)channel.SweepPitch * (channel.SweepLength - channel.SweepCounter) / channel.SweepLength;
        if (channel.AutoSweep)
        {
            channel.SweepCounter++;
        }
        return (int)result;
    }

    private static int UpdateLfo(SoundChannel channel)
    {
        long value = 0;
        if (channel.LfoDepth != 0 && channel.LfoDelayCounter >= channel.LfoDelay)
        {
            value = (long)SinIndex((channel.LfoCounter >> 8) & 0x7F) * channel.LfoDepth * channel.LfoRange;
            if (channel.LfoTarget == 1)
            {
                value = (value * 60) >> 14;
            }
            else
            {
                value >>= 8;
            }
        }
        if (channel.LfoDelayCounter < channel.LfoDelay)
        {
            channel.LfoDelayCounter++;
        }
        else
        {
            int counter = channel.LfoCounter + (channel.LfoSpeed << 6);
            int high = counter >> 8;
            while (high >= 0x80)
            {
                high -= 0x80;
            }
            channel.LfoCounter = (counter & 0xFF) | (high << 8);
        }
        return (int)value;
    }

    private void UpdateChannels()
    {
        foreach (SoundChannel channel in channels)
        {
            if (!channel.Active)
            {
                continue;
            }
            if (channel.Ended)
            {
                Free(channel);
                continue;
            }
            int volume = DecibelSquare[channel.Velocity];
            int pitch = (channel.Key - channel.OriginalKey) << 6;
            int pan = 0;
            volume += UpdateEnvelope(channel);
            pitch += UpdateSweep(channel);
            volume += channel.UserDecay;
            volume += channel.UserDecay2;
            pitch += channel.UserPitch;
            int lfo = UpdateLfo(channel);
            switch (channel.LfoTarget)
            {
                case 0:
                    pitch += lfo;
                    break;
                case 1:
                    if (volume > -32768)
                    {
                        volume += lfo;
                    }
                    break;
                case 2:
                    pan += lfo;
                    break;
            }
            pan += channel.InitPan;
            if (channel.PanRange != 127)
            {
                pan = (pan * channel.PanRange + 64) >> 7;
            }
            pan += channel.UserPan;
            if (channel.EnvelopeState == SoundChannel.EnvelopeRelease && volume <= -723)
            {
                Free(channel);
                continue;
            }
            float gain = volume <= -723 ? 0 : (float)Math.Pow(10, Math.Min(volume, 0) / 200.0);
            pan = Math.Clamp(pan + 64, 0, 127);
            channel.Gain = gain;
            channel.GainLeft = gain * (127 - pan) / 127f;
            channel.GainRight = gain * pan / 127f;
            double ratio = Math.Pow(2, pitch / 768.0);
            if (channel.Type == SoundChannel.TypePcm)
            {
                channel.Step = channel.Sample!.Rate * ratio / sampleRate;
            }
            else if (channel.Type == SoundChannel.TypePsg)
            {
                channel.Step = 261.6255653 * ratio / sampleRate;
            }
            else
            {
                channel.Step = 261.6255653 * 8 * ratio / sampleRate;
            }
        }
    }

    private float NextSample(SoundChannel channel)
    {
        if (channel.Type == SoundChannel.TypePsg)
        {
            channel.Position += channel.Step;
            channel.Position -= Math.Floor(channel.Position);
            return channel.Position * 8 < 7 - channel.Duty ? -1f : 1f;
        }
        if (channel.Type == SoundChannel.TypeNoise)
        {
            channel.Position += channel.Step;
            while (channel.Position >= 1)
            {
                channel.Position -= 1;
                bool carry = (channel.NoiseState & 1) != 0;
                channel.NoiseState >>= 1;
                if (carry)
                {
                    channel.NoiseState ^= 0x6000;
                    channel.NoiseOutput = -1f;
                }
                else
                {
                    channel.NoiseOutput = 1f;
                }
            }
            return channel.NoiseOutput;
        }
        Sample sample = channel.Sample!;
        float[] data = sample.Data;
        int length = data.Length;
        if (channel.Position >= length)
        {
            int span = length - sample.LoopStart;
            if (!sample.Loop || span <= 0)
            {
                channel.Ended = true;
                return 0;
            }
            channel.Position = sample.LoopStart + (channel.Position - sample.LoopStart) % span;
        }
        int index = (int)channel.Position;
        int next = index + 1;
        if (next >= length)
        {
            next = sample.Loop ? sample.LoopStart : index;
        }
        float frac = (float)(channel.Position - index);
        channel.Position += channel.Step;
        return data[index] + (data[next] - data[index]) * frac;
    }

    private void Frame()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].Frame();
        }
        players.RemoveAll(p => p.Finished);
        UpdateChannels();
    }

    public int Render(float[] buffer, int offset, int frames)
    {
        float master = MasterVolume / 127f * OutputGain;
        for (int f = 0; f < frames; f++)
        {
            countdown -= 1;
            if (countdown <= 0)
            {
                countdown += sampleRate / FrameRate;
                Frame();
            }
            float left = 0;
            float right = 0;
            foreach (SoundChannel channel in channels)
            {
                if (!channel.Active || channel.Ended)
                {
                    continue;
                }
                float s = NextSample(channel);
                left += s * channel.GainLeft;
                right += s * channel.GainRight;
            }
            buffer[offset + f * 2] = Math.Clamp(left * master, -1f, 1f);
            buffer[offset + f * 2 + 1] = Math.Clamp(right * master, -1f, 1f);
        }
        return frames;
    }

    public static int PsgMask => PsgChannels;

    public static int NoiseMask => NoiseChannels;
}
