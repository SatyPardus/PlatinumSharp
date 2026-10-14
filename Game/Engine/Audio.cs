using PlatinumSharp.Nitro;
using PlatinumSharp.Nitro.Sound;

namespace PlatinumSharp.Game;

public sealed class SoundHandle
{
    public SequencePlayer? Player;
    public int PlayerNumber = -1;
    public int SequenceID = -1;
    public bool Paused;
    public int Volume = 127;
    public int TargetVolume = 127;
    public int VolumeFrames;
    public int InitialVolume = 127;
    public int TrackPitchMask;
    public int Pitch;
    public int TrackPanMask;
    public int Pan;
    public int TempoRatio = 256;
    public int FadeOutFrames;
}

public sealed class WaveData
{
    public int Format;
    public bool Loop;
    public int Rate;
    public int LoopStart;
    public uint LoopLength;
    public byte[] Samples = Array.Empty<byte>();
}

public sealed class WaveOutChannel
{
    public int Channel;
    public int Format;
    public byte[]? Data;
    public bool Loop;
    public int LoopStart;
    public int Length;
    public int SampleRate;
    public int Volume;
    public int Speed;
    public int Pan;
    public double Position;
    public bool Playing;
}

public static class Audio
{
    public const int PlayerCount = 16;

    private static readonly SoundHandle?[] activeHandles = new SoundHandle?[PlayerCount];
    private static readonly HashSet<int> loadedSequences = new();
    private static readonly HashSet<int> loadedBanks = new();
    private static readonly HashSet<int> loadedWaveArcs = new();
    private static readonly HashSet<int> loadedGroups = new();
    private static readonly SoundDriver driver = new();
    private static readonly object gate = new();
    private static Sdat? archive;
    private static int heapLevel;
    private static int amplifier;
    private static int amplifierGain;

    public static Sdat Archive => archive ?? throw new InvalidOperationException("The sound archive has not been opened");

    public static int Amplifier => amplifier;

    public static int AmplifierGain => amplifierGain;

    public static void Init()
    {
        heapLevel = 0;
        Array.Clear(activeHandles);
        loadedSequences.Clear();
        loadedBanks.Clear();
        loadedWaveArcs.Clear();
        loadedGroups.Clear();
    }

    public static void OpenArchive(uint pathAddress)
    {
        string path = Bin.CString(RomData.Arm9(pathAddress, 64), 0);
        archive = Sdat.Open(RomData.Rom, path.StartsWith('/') ? path : "/" + path);
    }

    public static int SaveHeapState()
    {
        heapLevel++;
        return heapLevel;
    }

    public static void LoadHeapState(int level)
    {
        heapLevel = level;
    }

    public static bool LoadGroup(int group)
    {
        loadedGroups.Add(group);
        return true;
    }

    public static bool LoadSequence(int id)
    {
        loadedSequences.Add(id);
        return true;
    }

    public static bool LoadSequenceEx(int id, uint flags)
    {
        loadedSequences.Add(id);
        return true;
    }

    public static bool LoadWaveArc(int id)
    {
        loadedWaveArcs.Add(id);
        return true;
    }

    public static bool LoadBank(int id)
    {
        loadedBanks.Add(id);
        return true;
    }

    public static void InitHandle(SoundHandle handle)
    {
        lock (gate)
        {
            handle.Player?.Stop();
        }
        handle.Player = null;
        handle.FadeOutFrames = 0;
        handle.PlayerNumber = -1;
        handle.SequenceID = -1;
    }

    public static void ReleaseHandle(SoundHandle handle)
    {
        if (handle.PlayerNumber >= 0 && activeHandles[handle.PlayerNumber] == handle)
        {
            activeHandles[handle.PlayerNumber] = null;
        }
        InitHandle(handle);
    }

    public static void StopPlayer(int playerNumber, int fadeFrames)
    {
        SoundHandle? handle = activeHandles[playerNumber];
        if (handle != null)
        {
            StopSequence(handle, fadeFrames);
        }
    }

    public static void PauseHandle(SoundHandle handle, bool paused)
    {
        handle.Paused = paused;
        lock (gate)
        {
            handle.Player?.SetPaused(paused);
        }
    }

    public static void MoveVolume(SoundHandle handle, int targetVolume, int frames)
    {
        handle.TargetVolume = targetVolume;
        handle.VolumeFrames = frames;
        if (frames <= 0)
        {
            handle.Volume = targetVolume;
        }
    }

    private static readonly int[] allocatableChannels = new int[PlayerCount];
    private static readonly int[] playerVolumes = CreatePlayerVolumes();

    private static int[] CreatePlayerVolumes()
    {
        int[] volumes = new int[PlayerCount];
        Array.Fill(volumes, 127);
        return volumes;
    }

    public static void SetAllocatableChannels(int playerNumber, int channelMask)
    {
        allocatableChannels[playerNumber] = channelMask;
    }

    public static void SetPlayerVolume(int playerNumber, int volume)
    {
        playerVolumes[playerNumber] = volume;
    }

    public static int GetPlayerVolume(int playerNumber)
    {
        return playerVolumes[playerNumber];
    }

    public static int MasterVolume { get; private set; } = 127;

    public static void SetMasterVolume(int volume)
    {
        MasterVolume = volume;
    }

    public static uint GetTicks(SoundHandle handle)
    {
        return handle.Player != null ? (uint)handle.Player.TickCount : 0;
    }

    public static WaveData? GetWaveData(int fileId, int index)
    {
        byte[] file = Archive.ReadFile(fileId);
        if (file.Length < 0x40 || index >= Bin.I32(file, 0x38))
        {
            return null;
        }
        int start = Bin.I32(file, 0x3C + index * 4);
        int end = index + 1 < Bin.I32(file, 0x38) ? Bin.I32(file, 0x3C + (index + 1) * 4) : file.Length;
        byte[] samples = new byte[Math.Max(0, end - (start + 12))];
        Array.Copy(file, start + 12, samples, 0, samples.Length);
        return new WaveData
        {
            Format = file[start],
            Loop = file[start + 1] != 0,
            Rate = Bin.U16(file, start + 2),
            LoopStart = Bin.U16(file, start + 6),
            LoopLength = Bin.U32(file, start + 8),
            Samples = samples,
        };
    }

    public static void SetInitialVolume(SoundHandle handle, int volume)
    {
        handle.InitialVolume = volume;
    }

    public static bool StartSequence(SoundHandle handle, int playerNumber, int sequenceID)
    {
        SequenceInfo? info = Archive.FindSequence(sequenceID);
        if (info == null)
        {
            return false;
        }
        if (playerNumber < 0 || playerNumber >= PlayerCount)
        {
            playerNumber = info.Player;
        }
        Attach(handle, playerNumber, sequenceID, info);
        return true;
    }

    public static bool StartSequenceEx(SoundHandle handle, int playerNumber, int bank, int priority, int sequenceID)
    {
        SequenceInfo? info = Archive.FindSequence(sequenceID);
        if (info == null)
        {
            return false;
        }
        if (bank >= 0)
        {
            info = info with { Bank = bank };
        }
        if (playerNumber < 0 || playerNumber >= PlayerCount)
        {
            playerNumber = info.Player;
        }
        Attach(handle, playerNumber, sequenceID, info);
        return true;
    }

    private static void Attach(SoundHandle handle, int playerNumber, int sequenceID, SequenceInfo info)
    {
        Archive.GetBank(info.Bank);
        BankInfo? bankInfo = Archive.GetBankInfo(info.Bank);
        if (bankInfo != null)
        {
            foreach (int arc in bankInfo.WaveArcs)
            {
                if (arc >= 0)
                {
                    Archive.GetWaveArc(arc);
                }
            }
        }
        lock (gate)
        {
            SoundHandle? previous = activeHandles[playerNumber];
            if (previous != null && previous != handle)
            {
                ReleaseHandle(previous);
            }
            ReleaseHandle(handle);
            SequencePlayer player = new(driver, Archive, info);
            if (allocatableChannels[playerNumber] != 0)
            {
                player.SetAllocatableChannels(allocatableChannels[playerNumber]);
            }
            handle.Player = player;
            handle.PlayerNumber = playerNumber;
            handle.SequenceID = sequenceID;
            handle.Paused = false;
            handle.InitialVolume = info.Volume;
            handle.Volume = 127;
            handle.TargetVolume = 127;
            handle.VolumeFrames = 0;
            handle.FadeOutFrames = 0;
            handle.TempoRatio = 256;
            activeHandles[playerNumber] = handle;
            ApplyFader(handle);
        }
    }

    private static void ApplyFader(SoundHandle handle)
    {
        if (handle.Player == null || handle.PlayerNumber < 0)
        {
            return;
        }
        int fader = SoundDriver.Decibel[Math.Clamp(handle.InitialVolume, 0, 127)]
            + SoundDriver.Decibel[Math.Clamp(handle.Volume, 0, 127)]
            + SoundDriver.Decibel[Math.Clamp(playerVolumes[handle.PlayerNumber], 0, 127)];
        handle.Player.ExternalFader = Math.Max(fader, -32768);
        handle.Player.TempoRatio = handle.TempoRatio;
    }

    public static void StopSequence(SoundHandle handle, int fadeFrames)
    {
        if (handle.Player == null)
        {
            return;
        }
        if (fadeFrames <= 0)
        {
            ReleaseHandle(handle);
            return;
        }
        handle.TargetVolume = 0;
        handle.VolumeFrames = fadeFrames;
        handle.FadeOutFrames = fadeFrames;
    }

    public static void StopSequenceByNumber(int sequenceID, int fadeFrames)
    {
        foreach (SoundHandle? handle in activeHandles)
        {
            if (handle != null && handle.SequenceID == sequenceID)
            {
                StopSequence(handle, fadeFrames);
            }
        }
    }

    public static void StopAllSequences(int fadeFrames)
    {
        foreach (SoundHandle? handle in activeHandles)
        {
            if (handle != null)
            {
                StopSequence(handle, fadeFrames);
            }
        }
    }

    public static int CountPlayingSequences(int playerNumber)
    {
        SoundHandle? handle = activeHandles[playerNumber];
        return handle?.Player != null && !handle.Player.Finished ? 1 : 0;
    }

    public static SequenceInfo? GetSequenceInfo(int sequenceID)
    {
        return Archive.FindSequence(sequenceID);
    }

    public static int GetSequenceNumber(SoundHandle handle)
    {
        return handle.Player != null ? handle.SequenceID : -1;
    }

    public static BankInfo? GetBankInfo(int bank)
    {
        return Archive.GetBankInfo(bank);
    }

    public static void SetTrackPitch(SoundHandle handle, int tracks, int pitch)
    {
        handle.TrackPitchMask = tracks;
        handle.Pitch = pitch;
        lock (gate)
        {
            handle.Player?.SetTrackPitch(tracks, pitch);
        }
    }

    public static void SetTrackPan(SoundHandle handle, int tracks, int pan)
    {
        handle.TrackPanMask = tracks;
        handle.Pan = pan;
        lock (gate)
        {
            handle.Player?.SetTrackPan(tracks, pan);
        }
    }

    public static void SetTempoRatio(SoundHandle handle, int ratio)
    {
        handle.TempoRatio = ratio;
    }

    public static void SetMono(bool mono)
    {
        Mono = mono;
    }

    public static bool Mono { get; private set; }

    public static WaveOutChannel? AllocWaveOut(int channel)
    {
        WaveOutChannel wave = new() { Channel = channel };
        return wave;
    }

    public static void FreeWaveOut(WaveOutChannel wave)
    {
        wave.Playing = false;
        wave.Data = null;
    }

    public static bool StartWaveOut(WaveOutChannel wave, int format, byte[] data, bool loop, int loopStart, int length, int sampleRate, int volume, int speed, int pan)
    {
        wave.Format = format;
        wave.Data = data;
        wave.Loop = loop;
        wave.LoopStart = loopStart;
        wave.Length = length;
        wave.SampleRate = sampleRate;
        wave.Volume = volume;
        wave.Speed = speed;
        wave.Pan = pan;
        wave.Position = 0;
        wave.Playing = true;
        return true;
    }

    public static void StopWaveOut(WaveOutChannel wave)
    {
        wave.Playing = false;
    }

    public static bool IsWaveOutPlaying(WaveOutChannel wave)
    {
        return wave.Playing;
    }

    public static int GetWaveArcFileId(int waveArc)
    {
        return Archive.GetWaveArcFileId(waveArc);
    }

    public static int GetFileSize(int fileId)
    {
        return Archive.GetFileSize(fileId);
    }

    public static int ReadFile(int fileId, byte[] destination, int size)
    {
        byte[] data = Archive.ReadFile(fileId);
        int count = Math.Min(size, data.Length);
        Array.Copy(data, destination, count);
        return count;
    }

    public static bool CaptureActive { get; private set; }

    public static int ReverbVolume { get; private set; }

    public static Action<short[], short[], int>? CaptureEffect { get; private set; }

    public static bool StartReverb(int sampleRate, int volume)
    {
        CaptureActive = true;
        ReverbVolume = volume;
        return true;
    }

    public static void StopReverb(int frames)
    {
        CaptureActive = false;
    }

    public static void SetReverbVolume(int volume, int frames)
    {
        ReverbVolume = volume;
    }

    public static bool StartEffect(int sampleRate, Action<short[], short[], int> callback)
    {
        CaptureActive = true;
        CaptureEffect = callback;
        return true;
    }

    public static void StopEffect()
    {
        CaptureActive = false;
        CaptureEffect = null;
    }

    public static int StartMicAutoSampling(object parameters)
    {
        return 0;
    }

    public static int StopMicAutoSampling()
    {
        return 0;
    }

    public static int StartMicManualSampling(int type, object buffer, object? callback, object? argument)
    {
        return 0;
    }

    public static void Render(float[] buffer, int offset, int frames)
    {
        lock (gate)
        {
            driver.MasterVolume = MasterVolume;
            driver.Render(buffer, offset, frames);
        }
    }

    public static void Update()
    {
        lock (gate)
        {
            foreach (SoundHandle? handle in activeHandles)
            {
                if (handle == null)
                {
                    continue;
                }
                if (handle.Player != null && handle.Player.Finished)
                {
                    ReleaseHandle(handle);
                    continue;
                }
                if (handle.VolumeFrames > 0)
                {
                    handle.Volume += (handle.TargetVolume - handle.Volume) / handle.VolumeFrames;
                    handle.VolumeFrames--;
                    if (handle.VolumeFrames == 0)
                    {
                        handle.Volume = handle.TargetVolume;
                    }
                }
                if (handle.FadeOutFrames > 0)
                {
                    handle.FadeOutFrames--;
                    if (handle.FadeOutFrames == 0)
                    {
                        ReleaseHandle(handle);
                        continue;
                    }
                }
                ApplyFader(handle);
            }
        }
    }

    public static string Describe()
    {
        lock (gate)
        {
            System.Text.StringBuilder text = new();
            text.AppendLine($"master {MasterVolume}");
            for (int i = 0; i < PlayerCount; i++)
            {
                SoundHandle? handle = activeHandles[i];
                if (handle?.Player != null)
                {
                    text.AppendLine($"player {i} seq {handle.SequenceID} vol {handle.Volume} init {handle.InitialVolume} pv {playerVolumes[i]} fader {handle.Player.ExternalFader} tempo {handle.TempoRatio} alloc {allocatableChannels[i]:X}");
                }
            }
            foreach (SoundChannel channel in driver.Channels)
            {
                if (channel.Active)
                {
                    text.AppendLine($"ch {channel.Index} type {channel.Type} gain {channel.Gain} L {channel.GainLeft} R {channel.GainRight} step {channel.Step} key {channel.Key} vel {channel.Velocity} env {channel.Envelope} state {channel.EnvelopeState} ud {channel.UserDecay} ud2 {channel.UserDecay2} len {channel.Length}");
                }
            }
            return text.ToString();
        }
    }

    public static void InitMicrophone()
    {
    }

    public static void SetAmplifier(int value)
    {
        amplifier = value;
    }

    public static void SetAmplifierGain(int value)
    {
        amplifierGain = value;
    }
}
