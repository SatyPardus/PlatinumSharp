namespace PlatinumSharp.Nitro.Sound;

public sealed class SequencePlayer
{
    private const int TrackCount = 16;
    private const int StackDepth = 3;
    private const int InstrumentPcm = 1;
    private const int InstrumentPsg = 2;
    private const int InstrumentNoise = 3;
    private const int InstrumentDirectPcm = 4;

    private static readonly int[] globalVariables = CreateVariables();

    private enum ArgKind
    {
        U8,
        S16,
        VarLen,
    }

    private sealed class Track
    {
        public bool Active;
        public bool NoteWait = true;
        public bool Tie;
        public bool NoteFinishWait;
        public bool Porta;
        public bool Compare = true;
        public int ChannelMask = 0xFFFF;
        public int Program;
        public int Priority = 64;
        public int Volume = 127;
        public int Volume2 = 127;
        public int Pan;
        public int ExtPan;
        public int PitchBend;
        public int ExtPitch;
        public int Attack = 255;
        public int Decay = 255;
        public int Sustain = 255;
        public int Release = 255;
        public int PanRange = 127;
        public int BendRange = 2;
        public int PortaKey = 60;
        public int PortaTime;
        public int SweepPitch;
        public int Transpose;
        public int ModTarget;
        public int ModSpeed = 16;
        public int ModDepth;
        public int ModRange = 1;
        public int ModDelay;
        public int Pos;
        public int Wait;
        public int StackCount;
        public readonly int[] StackPos = new int[StackDepth];
        public readonly int[] StackLoop = new int[StackDepth];
        public readonly List<SoundChannel> Channels = new();
        public readonly Action<SoundChannel> ChannelFreed;

        public Track()
        {
            ChannelFreed = channel => Channels.Remove(channel);
        }
    }

    private readonly SoundDriver driver;
    private readonly bool ownsDriver;
    private readonly Sdat sdat;
    private readonly byte[] seq;
    private readonly int dataBase;
    private readonly Instrument?[] bank;
    private readonly BankInfo? bankInfo;
    private readonly Track[] tracks = new Track[TrackCount];
    private readonly int[] variables = CreateVariables();
    private int tempo = 120;
    private int tempoCounter = 240;
    private int volume = 127;
    private bool paused;

    public bool Finished { get; private set; }

    public long TickCount { get; private set; }

    public int TempoRatio { get; set; } = 256;

    public int Priority { get; set; } = 64;

    public int ExternalFader { get; set; }

    public int VoiceCount => driver.ActiveChannelCount;

    public SequencePlayer(Sdat sdat, SequenceInfo info, int sampleRate = 44100)
        : this(new SoundDriver(sampleRate), sdat, info, true)
    {
    }

    public SequencePlayer(SoundDriver driver, Sdat sdat, SequenceInfo info)
        : this(driver, sdat, info, false)
    {
    }

    private SequencePlayer(SoundDriver driver, Sdat sdat, SequenceInfo info, bool ownsDriver)
    {
        this.driver = driver;
        this.ownsDriver = ownsDriver;
        this.sdat = sdat;
        seq = sdat.ReadFile(info.FileId);
        dataBase = Bin.I32(seq, 0x18);
        bankInfo = sdat.GetBankInfo(info.Bank);
        bank = sdat.GetBank(info.Bank);
        if (bankInfo != null)
        {
            foreach (int arc in bankInfo.WaveArcs)
            {
                if (arc >= 0)
                {
                    sdat.GetWaveArc(arc);
                }
            }
        }
        Priority = info.ChannelPriority;
        ExternalFader = SoundDriver.Decibel[Math.Clamp(info.Volume, 0, 127)];
        for (int i = 0; i < tracks.Length; i++)
        {
            tracks[i] = new Track();
        }
        tracks[0].Active = true;
        tracks[0].Pos = dataBase;
        driver.Add(this);
    }

    private static int[] CreateVariables()
    {
        int[] values = new int[16];
        Array.Fill(values, -1);
        return values;
    }

    public int Render(float[] buffer, int offset, int frames)
    {
        return driver.Render(buffer, offset, frames);
    }

    public void SetPaused(bool pause)
    {
        if (paused == pause)
        {
            return;
        }
        paused = pause;
        if (pause)
        {
            foreach (Track t in tracks)
            {
                ReleaseAll(t, 127);
                FreeAll(t);
            }
        }
    }

    public void Stop()
    {
        if (Finished)
        {
            return;
        }
        foreach (Track t in tracks)
        {
            if (t.Active)
            {
                CloseTrack(t);
            }
        }
        Finished = true;
        if (!ownsDriver)
        {
            driver.Remove(this);
        }
    }

    public void SetTrackPitch(int mask, int pitch)
    {
        for (int i = 0; i < TrackCount; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                tracks[i].ExtPitch = pitch;
            }
        }
    }

    public void SetTrackPan(int mask, int pan)
    {
        for (int i = 0; i < TrackCount; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                tracks[i].ExtPan = pan;
            }
        }
    }

    public void SetAllocatableChannels(int mask)
    {
        foreach (Track t in tracks)
        {
            t.ChannelMask = mask == 0 ? 0xFFFF : mask;
        }
    }

    public void Frame()
    {
        if (Finished || paused)
        {
            return;
        }
        int ticks = 0;
        while (tempoCounter >= 240)
        {
            tempoCounter -= 240;
            ticks++;
        }
        for (int i = 0; i < ticks; i++)
        {
            if (StepTicks())
            {
                Finished = true;
                return;
            }
        }
        tempoCounter += (tempo * TempoRatio) >> 8;
        foreach (Track t in tracks)
        {
            if (t.Active)
            {
                UpdateChannels(t, true);
            }
        }
    }

    private bool StepTicks()
    {
        bool active = false;
        TickCount++;
        foreach (Track t in tracks)
        {
            if (!t.Active)
            {
                continue;
            }
            if (StepTrack(t))
            {
                active = true;
            }
            else
            {
                CloseTrack(t);
            }
        }
        return !active;
    }

    private void CloseTrack(Track t)
    {
        ReleaseAll(t, -1);
        FreeAll(t);
        t.Active = false;
    }

    private void ReleaseAll(Track t, int release)
    {
        UpdateChannels(t, false);
        foreach (SoundChannel channel in t.Channels)
        {
            if (!channel.Active)
            {
                continue;
            }
            if (release >= 0)
            {
                channel.Release = SoundDriver.ConvertFall(release & 0xFF);
            }
            channel.Priority = 1;
            SoundDriver.ReleaseChannel(channel);
        }
    }

    private static void FreeAll(Track t)
    {
        foreach (SoundChannel channel in t.Channels)
        {
            channel.Released = null;
            channel.Owner = null;
        }
        t.Channels.Clear();
    }

    private void UpdateChannels(Track t, bool release)
    {
        int decay = SoundDriver.DecibelSquare[t.Volume] + SoundDriver.DecibelSquare[t.Volume2] + SoundDriver.DecibelSquare[volume];
        if (decay < -32768)
        {
            decay = -32768;
        }
        int pitch = t.ExtPitch + ((t.PitchBend * (t.BendRange << 6)) >> 7);
        int pan = t.Pan;
        if (t.PanRange != 127)
        {
            pan = (pan * t.PanRange + 64) >> 7;
        }
        pan = Math.Clamp(pan + t.ExtPan, -128, 127);
        foreach (SoundChannel channel in t.Channels)
        {
            channel.UserDecay2 = ExternalFader;
            if (channel.EnvelopeState == SoundChannel.EnvelopeRelease)
            {
                continue;
            }
            channel.UserDecay = decay;
            channel.UserPitch = pitch;
            channel.UserPan = pan;
            channel.PanRange = t.PanRange;
            channel.LfoTarget = t.ModTarget;
            channel.LfoSpeed = t.ModSpeed;
            channel.LfoDepth = t.ModDepth;
            channel.LfoRange = t.ModRange;
            channel.LfoDelay = t.ModDelay;
            if (channel.Length == 0 && release)
            {
                channel.Priority = 1;
                SoundDriver.ReleaseChannel(channel);
            }
        }
    }

    private bool StepTrack(Track t)
    {
        foreach (SoundChannel channel in t.Channels)
        {
            if (channel.Length > 0)
            {
                channel.Length--;
            }
            if (!channel.AutoSweep && channel.SweepCounter < channel.SweepLength)
            {
                channel.SweepCounter++;
            }
        }
        if (t.NoteFinishWait)
        {
            if (t.Channels.Count > 0)
            {
                return true;
            }
            t.NoteFinishWait = false;
        }
        if (t.Wait > 0)
        {
            t.Wait--;
            if (t.Wait > 0)
            {
                return true;
            }
        }
        int guard = 0;
        while (t.Wait == 0 && !t.NoteFinishWait)
        {
            if (++guard > 8192)
            {
                return false;
            }
            try
            {
                if (!Step(t))
                {
                    return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }
        return true;
    }

    private int ReadU24(Track t)
    {
        int value = seq[t.Pos] | (seq[t.Pos + 1] << 8) | (seq[t.Pos + 2] << 16);
        t.Pos += 3;
        return value;
    }

    private int Variable(int index)
    {
        index &= 31;
        return index < 16 ? variables[index] : globalVariables[index - 16];
    }

    private void SetVariable(int index, int value)
    {
        index &= 31;
        value = (short)value;
        if (index < 16)
        {
            variables[index] = value;
        }
        else
        {
            globalVariables[index - 16] = value;
        }
    }

    private int Arg(Track t, int mode, ArgKind kind)
    {
        if (mode == 1)
        {
            int min = Bin.S16(seq, t.Pos);
            int max = Bin.S16(seq, t.Pos + 2);
            t.Pos += 4;
            return ((driver.Random() * (max - min + 1)) >> 16) + min;
        }
        if (mode == 2)
        {
            return Variable(seq[t.Pos++]);
        }
        if (kind == ArgKind.U8)
        {
            return seq[t.Pos++];
        }
        if (kind == ArgKind.S16)
        {
            int value = Bin.S16(seq, t.Pos);
            t.Pos += 2;
            return value;
        }
        int result = 0;
        int b;
        do
        {
            b = seq[t.Pos++];
            result = (result << 7) | (b & 0x7F);
        }
        while ((b & 0x80) != 0);
        return result;
    }

    private bool Step(Track t)
    {
        bool run = true;
        int mode = 0;
        int cmd = seq[t.Pos++];
        if (cmd == 0xA2)
        {
            run = t.Compare;
            cmd = seq[t.Pos++];
        }
        if (cmd == 0xA0)
        {
            mode = 1;
            cmd = seq[t.Pos++];
        }
        if (cmd == 0xA1)
        {
            mode = 2;
            cmd = seq[t.Pos++];
        }
        if (cmd < 0x80)
        {
            int velocity = seq[t.Pos++];
            int length = Arg(t, mode, ArgKind.VarLen);
            int key = Math.Clamp(cmd + t.Transpose, 0, 127);
            if (run)
            {
                StartNote(t, key, velocity, length > 0 ? length : -1);
                t.PortaKey = key;
                if (t.NoteWait)
                {
                    t.Wait = length;
                    if (length == 0)
                    {
                        t.NoteFinishWait = true;
                    }
                }
            }
            return true;
        }
        if (cmd >= 0xB0 && cmd <= 0xBD)
        {
            int index = seq[t.Pos++];
            int value = Arg(t, mode, ArgKind.S16);
            if (run)
            {
                VariableOp(t, cmd, index, value);
            }
            return true;
        }
        if (cmd >= 0xC0 && cmd <= 0xD7)
        {
            int value = Arg(t, mode, ArgKind.U8);
            if (run)
            {
                Control(t, cmd, value);
            }
            return true;
        }
        switch (cmd)
        {
            case 0x80:
            {
                int value = Arg(t, mode, ArgKind.VarLen);
                if (run)
                {
                    t.Wait = value;
                }
                break;
            }
            case 0x81:
            {
                int value = Arg(t, mode, ArgKind.VarLen);
                if (run)
                {
                    t.Program = value & 0x7FFF;
                }
                break;
            }
            case 0x93:
            {
                int index = seq[t.Pos++] & 15;
                int target = dataBase + ReadU24(t);
                if (run && !tracks[index].Active && tracks[index] != t)
                {
                    tracks[index].Active = true;
                    tracks[index].Pos = target;
                    tracks[index].ChannelMask = t.ChannelMask;
                }
                break;
            }
            case 0x94:
            {
                int target = dataBase + ReadU24(t);
                if (run)
                {
                    t.Pos = target;
                }
                break;
            }
            case 0x95:
            {
                int target = dataBase + ReadU24(t);
                if (run && t.StackCount < StackDepth)
                {
                    t.StackPos[t.StackCount] = t.Pos;
                    t.StackCount++;
                    t.Pos = target;
                }
                break;
            }
            case 0xE0:
            {
                int value = Arg(t, mode, ArgKind.S16);
                if (run)
                {
                    t.ModDelay = value;
                }
                break;
            }
            case 0xE1:
            {
                int value = Arg(t, mode, ArgKind.S16);
                if (run)
                {
                    tempo = value;
                }
                break;
            }
            case 0xE3:
            {
                int value = Arg(t, mode, ArgKind.S16);
                if (run)
                {
                    t.SweepPitch = value;
                }
                break;
            }
            case 0xFC:
                if (run && t.StackCount > 0)
                {
                    int count = t.StackLoop[t.StackCount - 1];
                    if (count != 0)
                    {
                        count--;
                        if (count == 0)
                        {
                            t.StackCount--;
                            break;
                        }
                    }
                    t.StackLoop[t.StackCount - 1] = count;
                    t.Pos = t.StackPos[t.StackCount - 1];
                }
                break;
            case 0xFD:
                if (run && t.StackCount > 0)
                {
                    t.StackCount--;
                    t.Pos = t.StackPos[t.StackCount];
                }
                break;
            case 0xFE:
                t.Pos += 2;
                break;
            case 0xFF:
                if (run)
                {
                    return false;
                }
                break;
            default:
                return false;
        }
        return true;
    }

    private void VariableOp(Track t, int cmd, int index, int value)
    {
        int current = Variable(index);
        switch (cmd)
        {
            case 0xB0:
                SetVariable(index, value);
                break;
            case 0xB1:
                SetVariable(index, current + value);
                break;
            case 0xB2:
                SetVariable(index, current - value);
                break;
            case 0xB3:
                SetVariable(index, current * value);
                break;
            case 0xB4:
                if (value != 0)
                {
                    SetVariable(index, current / value);
                }
                break;
            case 0xB5:
                SetVariable(index, value >= 0 ? current << value : current >> -value);
                break;
            case 0xB6:
            {
                bool negative = value < 0;
                int range = negative ? -value : value;
                int random = (driver.Random() * (range + 1)) >> 16;
                SetVariable(index, negative ? -random : random);
                break;
            }
            case 0xB8:
                t.Compare = current == value;
                break;
            case 0xB9:
                t.Compare = current >= value;
                break;
            case 0xBA:
                t.Compare = current > value;
                break;
            case 0xBB:
                t.Compare = current <= value;
                break;
            case 0xBC:
                t.Compare = current < value;
                break;
            case 0xBD:
                t.Compare = current != value;
                break;
        }
    }

    private void Control(Track t, int cmd, int value)
    {
        switch (cmd)
        {
            case 0xC0:
                t.Pan = (value & 0xFF) - 64;
                break;
            case 0xC1:
                t.Volume = value & 127;
                break;
            case 0xC2:
                volume = value & 127;
                break;
            case 0xC3:
                t.Transpose = (sbyte)value;
                break;
            case 0xC4:
                t.PitchBend = (sbyte)value;
                break;
            case 0xC5:
                t.BendRange = value & 0xFF;
                break;
            case 0xC6:
                t.Priority = value & 0xFF;
                break;
            case 0xC7:
                t.NoteWait = (value & 1) != 0;
                break;
            case 0xC8:
                t.Tie = (value & 1) != 0;
                ReleaseAll(t, -1);
                FreeAll(t);
                break;
            case 0xC9:
                t.PortaKey = Math.Clamp(value + t.Transpose, 0, 127);
                t.Porta = true;
                break;
            case 0xCA:
                t.ModDepth = value & 0xFF;
                break;
            case 0xCB:
                t.ModSpeed = value & 0xFF;
                break;
            case 0xCC:
                t.ModTarget = value & 0xFF;
                break;
            case 0xCD:
                t.ModRange = value & 0xFF;
                break;
            case 0xCE:
                t.Porta = (value & 1) != 0;
                break;
            case 0xCF:
                t.PortaTime = value & 0xFF;
                break;
            case 0xD0:
                t.Attack = value & 0xFF;
                break;
            case 0xD1:
                t.Decay = value & 0xFF;
                break;
            case 0xD2:
                t.Sustain = value & 0xFF;
                break;
            case 0xD3:
                t.Release = value & 0xFF;
                break;
            case 0xD4:
                if (t.StackCount < StackDepth)
                {
                    t.StackPos[t.StackCount] = t.Pos;
                    t.StackLoop[t.StackCount] = value & 0xFF;
                    t.StackCount++;
                }
                break;
            case 0xD5:
                t.Volume2 = value & 127;
                break;
        }
    }

    private void StartNote(Track t, int key, int velocity, int length)
    {
        SoundChannel? channel = null;
        if (t.Tie && t.Channels.Count > 0)
        {
            channel = t.Channels[^1];
            channel.Key = key;
            channel.Velocity = Math.Clamp(velocity, 0, 127);
        }
        if (channel == null)
        {
            if (t.Program >= bank.Length)
            {
                return;
            }
            NoteDef? def = bank[t.Program]?.Resolve(key);
            if (def == null)
            {
                return;
            }
            int mask;
            int type;
            Sample? sample = null;
            switch (def.Type)
            {
                case InstrumentPcm:
                case InstrumentDirectPcm:
                {
                    int arc = bankInfo != null && def.WaveArc < 4 ? bankInfo.WaveArcs[def.WaveArc] : -1;
                    Sample[] samples = sdat.GetWaveArc(arc);
                    if (def.Wave >= samples.Length || samples[def.Wave].Data.Length == 0)
                    {
                        return;
                    }
                    sample = samples[def.Wave];
                    type = SoundChannel.TypePcm;
                    mask = 0xFFFF;
                    break;
                }
                case InstrumentPsg:
                    type = SoundChannel.TypePsg;
                    mask = SoundDriver.PsgMask;
                    break;
                case InstrumentNoise:
                    type = SoundChannel.TypeNoise;
                    mask = SoundDriver.NoiseMask;
                    break;
                default:
                    return;
            }
            mask &= t.ChannelMask;
            channel = driver.Allocate(mask, Math.Min(255, Priority + t.Priority), t.ChannelFreed, t);
            if (channel == null)
            {
                return;
            }
            int noteLength = t.Tie ? -1 : length;
            int release = def.Release;
            if (release == 0xFF)
            {
                noteLength = -1;
                release = 0;
            }
            driver.Start(channel, type, sample, def.Wave & 7, key, def.Root, Math.Clamp(velocity, 0, 127), noteLength, def.Attack, def.Decay, def.Sustain, release, def.Pan);
            t.Channels.Add(channel);
        }
        if (t.Attack != 255)
        {
            channel.Attack = SoundDriver.ConvertAttack(t.Attack);
        }
        if (t.Decay != 255)
        {
            channel.Decay = SoundDriver.ConvertFall(t.Decay);
        }
        if (t.Sustain != 255)
        {
            channel.Sustain = Math.Clamp(t.Sustain, 0, 127);
        }
        if (t.Release != 255)
        {
            channel.Release = SoundDriver.ConvertFall(t.Release);
        }
        channel.SweepPitch = t.SweepPitch;
        if (t.Porta)
        {
            channel.SweepPitch += (short)((t.PortaKey - key) << 6);
        }
        if (t.PortaTime == 0)
        {
            channel.SweepLength = length;
            channel.AutoSweep = false;
        }
        else
        {
            int time = t.PortaTime * t.PortaTime;
            time *= Math.Abs(channel.SweepPitch);
            channel.SweepLength = time >> 11;
            channel.AutoSweep = true;
        }
        channel.SweepCounter = 0;
    }

    public static void SaveWav(string path, float[] interleaved, int sampleRate)
    {
        using BinaryWriter writer = new(File.Create(path));
        int dataBytes = interleaved.Length * 2;
        writer.Write("RIFF"u8);
        writer.Write(36 + dataBytes);
        writer.Write("WAVEfmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)2);
        writer.Write(sampleRate);
        writer.Write(sampleRate * 4);
        writer.Write((short)4);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(dataBytes);
        foreach (float sample in interleaved)
        {
            writer.Write((short)(Math.Clamp(sample, -1f, 1f) * 32767));
        }
    }
}
