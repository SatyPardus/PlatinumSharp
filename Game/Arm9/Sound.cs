using PlatinumSharp.Nitro.Sound;
using static PlatinumSharp.Game.SoundSystem;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class WaveOutParam
{
    public SoundParam Handle = null!;
    public int Format;
    public byte[] Data = Array.Empty<byte>();
    public int Loop;
    public int LoopStart;
    public int DataLength;
    public int SampleRate;
    public int Volume;
    public int Speed;
    public int Pan;
}

public static class Sound
{
    private const int ParamBGMFixed = 5;
    private const int ParamCurrentBGM = 10;
    private const int ParamNextBGM = 11;
    private const int ParamMainScene = 21;
    private const int ParamSubScene = 22;
    private const int ParamFieldBGM = 32;
    private const int FirstSubScene = 0x33;
    private const int WaveOutPrimary = 14;
    private const int WaveOutSecondary = 15;

    private const int CrySpecialWaveArc = 0x1B9;
    private const int CryLastWaveArc = 0x1EF;

    private static readonly byte[] sWaveBuffer = new byte[0xADC];
    private static byte[]? sAmplitudeThresholds;
    private static int sPlaybackMode;

    private static readonly ushort[] sFieldSequences =
    {
        0x577, 0x579, 0x57B, 0x576, 0x57C, 0x57E, 0x650, 0x56A, 0x722,
        0x56B, 0x56C, 0x56D, 0x56F, 0x63E, 0x643, 0x572, 0x569,
    };

    // OFFSET 0x020041CC
    public static void Sound_SetBGMFixed(byte value)
    {
        SoundSystem_GetParam(ParamBGMFixed)!.Value = value;
    }

    // OFFSET 0x020041DC
    public static byte Sound_IsBGMFixed()
    {
        return (byte)SoundSystem_GetParam(ParamBGMFixed)!.Value;
    }

    // OFFSET 0x020041E8
    public static void Sound_SetCurrentBGM(ushort bgmID)
    {
        SoundSystem_GetParam(ParamCurrentBGM)!.Value = bgmID;
        Sound_SetNextBGM(0);
    }

    // OFFSET 0x020041FC
    public static ushort Sound_Impl_GetCurrentBGM()
    {
        return (ushort)SoundSystem_GetParam(ParamCurrentBGM)!.Value;
    }

    // OFFSET 0x02004208
    public static void Sound_SetNextBGM(ushort bgmID)
    {
        SoundSystem_GetParam(ParamNextBGM)!.Value = bgmID;
    }

    // OFFSET 0x02004218
    public static ushort Sound_GetNextBGM()
    {
        return (ushort)SoundSystem_GetParam(ParamNextBGM)!.Value;
    }

    // OFFSET 0x02004224
    public static void Sound_SetFieldBGM(ushort bgmID)
    {
        SoundSystem_GetParam(ParamFieldBGM)!.Value = bgmID;
    }

    // OFFSET 0x02004234
    public static void Sound_SetScene(byte scene)
    {
        SoundParam mainScene = SoundSystem_GetParam(ParamMainScene)!;
        SoundParam subScene = SoundSystem_GetParam(ParamSubScene)!;
        if (scene < FirstSubScene)
        {
            mainScene.Value = scene;
            subScene.Value = 0;
        }
        else
        {
            subScene.Value = scene;
        }
    }

    // OFFSET 0x02004258
    public static void Sound_SetSubScene(byte scene)
    {
        SoundSystem_GetParam(ParamMainScene);
        SoundSystem_GetParam(ParamSubScene)!.Value = scene;
    }

    // OFFSET 0x0200426C
    public static int Sound_LoadSoundEffectsForScene(byte scene)
    {
        int result = 0;
        switch (scene)
        {
            case 1:
            case 4:
            case 8:
            case 9:
            case 10:
            case 11:
            case 15:
            case 17:
            case 20:
            case 22:
            case 23:
                result = LoadGroup(1);
                break;
            case 2:
            case 5:
            case 13:
            case 18:
                result = LoadGroup(2);
                break;
            case 3:
                result = LoadGroup(0xD);
                break;
            case 6:
                result = LoadGroup(0xB);
                break;
            case 7:
            case 53:
                result = LoadGroup(9);
                break;
            case 12:
            case 14:
                result = LoadGroup(0xE);
                break;
            case 16:
                LoadGroup(1);
                result = LoadGroup(8);
                break;
            case 19:
                result = LoadGroup(1);
                foreach (ushort sequence in sFieldSequences)
                {
                    SoundSystem_LoadSequenceEx(sequence, 1);
                }
                break;
            case 21:
                result = LoadBankAndWaveArc(0x302);
                break;
            case 51:
            case 60:
                result = LoadGroup(3);
                break;
            case 52:
            case 61:
                result = LoadGroup(5);
                break;
            case 54:
                result = LoadGroup(0xA);
                break;
            case 55:
            case 67:
                result = LoadBankAndWaveArc(0x2F7);
                break;
            case 56:
                result = LoadGroup(0xC);
                break;
            case 57:
                result = LoadGroup(7);
                break;
            case 58:
                result = LoadGroup(8);
                break;
            case 59:
            case 62:
                result = LoadGroup(0xF);
                break;
            case 63:
                result = LoadGroup(4);
                break;
            case 64:
                result = LoadBankAndWaveArc(0x2FF);
                break;
            case 65:
                result = LoadBankAndWaveArc(0x2FE);
                break;
            case 66:
                result = LoadGroup(6);
                break;
            case 68:
                result = LoadBankAndWaveArc(0x301);
                break;
        }
        return result;
    }

    // OFFSET 0x02004550
    public static int Sound_SetSceneAndPlayBGM(byte scene, ushort bgmID, int unused)
    {
        SoundParam mainScene = SoundSystem_GetParam(ParamMainScene)!;
        SoundParam subScene = SoundSystem_GetParam(ParamSubScene)!;
        SoundParam param14 = SoundSystem_GetParam(14)!;
        if (scene < FirstSubScene)
        {
            if ((byte)mainScene.Value == scene)
            {
                return 0;
            }
        }
        else if ((byte)subScene.Value == scene)
        {
            return 0;
        }
        Sound_SetScene(scene);
        switch (scene)
        {
            case 1:
                Sound_ConfigureBGMChannelsAndReverb(1);
                Sound_Impl_PlayCutsceneBGM(scene, bgmID, unused);
                break;
            case 2:
                Sound_ConfigureBGMChannelsAndReverb(0);
                Sound_Impl_PlayCutsceneBGM(scene, bgmID, unused);
                break;
            case 4:
                Sound_ConfigureBGMChannelsAndReverb(0);
                Sound_Impl_PlayFieldBGM(bgmID, unused);
                param14.Value = 0;
                break;
            case 5:
                Sound_Impl_PlayBattleBGM(bgmID, unused);
                break;
            case 6:
                Sound_Impl_PlayContestBGM(bgmID, unused);
                break;
            case 7:
                Sound_Impl_PlayScene7BGM(bgmID, unused);
                break;
            case 11:
                Sound_Impl_PlayScene11BGM(bgmID, unused);
                break;
            case 14:
                Sound_ConfigureBGMChannelsAndReverb(2);
                Sound_Impl_PlayCutsceneBGM(scene, bgmID, unused);
                break;
            case 3:
            case 8:
            case 9:
            case 10:
            case 12:
            case 13:
            case 15:
            case 16:
            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
                Sound_Impl_PlayCutsceneBGM(scene, bgmID, unused);
                break;
            default:
                if (scene >= FirstSubScene && scene <= 68)
                {
                    Sound_Impl_LoadSubSceneSoundData(scene);
                }
                break;
        }
        return 1;
    }

    // OFFSET 0x020046CC
    private static void Sound_LoadSoundEffectsForSceneWithState(byte scene)
    {
        SoundSystem_LoadHeapState(SoundSystem_GetParam(0x18)!.Value);
        SoundSystem_SaveHeapState(SoundSystem_GetParam(0x19));
        Sound_LoadSoundEffectsForScene(scene);
        SoundSystem_SaveHeapState(SoundSystem_GetParam(0x1A));
    }

    // OFFSET 0x020046F8
    private static void Sound_Impl_PlayFieldBGM(ushort bgmID, int unused)
    {
        SoundParam paused = SoundSystem_GetParam(0xC)!;
        SoundSystem_GetParam(0x18);
        SoundSystem_GetParam(ParamFieldBGM);
        int current = Sound_GetSequenceIDFromSoundHandle(SoundSystem_GetSoundHandle(0));
        if ((byte)paused.Value == 0 && current == bgmID && Sound_GetNextBGM() != 0x480)
        {
            return;
        }
        Sound_SetFieldBGMBankState(1);
        SoundPlayback.Sound_StopAll();
        if (current != bgmID)
        {
            Sound_SetBGMPlayerPaused(1, 0);
            SoundPlayback.Sound_StopWaveOutAndSequences();
        }
        if ((byte)paused.Value == 1)
        {
            SoundSystem_LoadHeapState(Sound_GetHeapState(2));
            Sound_LoadSoundEffectsForScene(4);
            SoundSystem_SaveHeapState(SoundSystem_GetParam(0x1A));
            if (current != bgmID)
            {
                Sound_SetBGMPlayerPaused(1, 0);
            }
            Sound_Impl_ResumeAndSwitchFieldBGM(bgmID, (ushort)current);
            return;
        }
        SoundPlayback.Sound_PlayBGM(bgmID);
    }

    // OFFSET 0x0200478C
    private static void Sound_Impl_ResumeAndSwitchFieldBGM(ushort bgmID, ushort unused)
    {
        SoundParam fieldBGM = SoundSystem_GetParam(ParamFieldBGM)!;
        int bank = Sound_GetBankIDFromSequenceID((ushort)fieldBGM.Value);
        if (bank != 0x2BD && bank != 0x2BF)
        {
            SoundSystem_LoadSequenceEx(bgmID, 4);
            ErrorHandling.GF_AssertFail();
        }
        else
        {
            SoundSystem_LoadSequenceEx((ushort)fieldBGM.Value, 6);
        }
        SoundSystem_SaveHeapState(SoundSystem_GetParam(0x1B));
        Sound_SetBGMPlayerPaused(1, 0);
        SoundPlayback.Sound_FadeInBGM(0x7F, 0x28, 0);
        Sound_SetFieldBGMBankState(0);
    }

    // OFFSET 0x020047E8
    public static void Sound_LoadSoundDataForFieldBGM(ushort seqID, ushort currentBankID)
    {
        SoundParam param19 = SoundSystem_GetParam(0x13)!;
        SoundParam fieldBGM = SoundSystem_GetParam(ParamFieldBGM)!;
        if ((byte)param19.Value != 1 && currentBankID != 0)
        {
            return;
        }
        SoundSystem_LoadHeapState(Sound_GetHeapState(1));
        Sound_SetSubScene(0);
        SoundSystem_LoadSequenceEx((ushort)fieldBGM.Value, 2);
        SoundSystem_SaveHeapState(SoundSystem_GetParam(0x19));
        Sound_LoadSoundEffectsForScene(4);
        SoundSystem_SaveHeapState(SoundSystem_GetParam(0x1A));
        int bank = Sound_GetBankIDFromSequenceID((ushort)fieldBGM.Value);
        if (bank != 0x2BD && bank != 0x2BF)
        {
            SoundSystem_LoadSequenceEx(seqID, 4);
            ErrorHandling.GF_AssertFail();
        }
        else
        {
            SoundSystem_LoadSequenceEx((ushort)fieldBGM.Value, 4);
        }
        SoundSystem_SaveHeapState(SoundSystem_GetParam(0x1B));
    }

    // OFFSET 0x02004874
    private static void Sound_Impl_PlayBattleBGM(ushort bgmID, int unused)
    {
        SoundSystem_GetParam(0x18);
        Sound_Impl_PauseOrStopFieldBGM();
        SoundSystem_LoadHeapState(Sound_GetHeapState(2));
        Sound_LoadSoundEffectsForScene(5);
        SoundSystem_SaveHeapState(SoundSystem_GetParam(0x1A));
        Sound_SetFieldBGMBankState(1);
        SoundPlayback.Sound_PlayBGM(bgmID);
    }

    // OFFSET 0x020048AC
    private static void Sound_Impl_PlayScene11BGM(ushort bgmID, int unused)
    {
        SoundSystem_GetParam(0x18);
        SoundPlayback.Sound_StopWaveOutAndSequences();
        Sound_ClearBGMPauseFlags();
        Sound_LoadSoundEffectsForSceneWithState(4);
        SoundPlayback.Sound_PlayBGM(bgmID);
    }

    // OFFSET 0x020048CC
    public static void Sound_Impl_PlayContestBGM(ushort bgmID, int unused)
    {
        SoundSystem_GetParam(0x18);
        SoundPlayback.Sound_StopWaveOutAndSequences();
        Sound_LoadSoundEffectsForSceneWithState(6);
        Sound_SetFieldBGMBankState(1);
        SoundPlayback.Sound_PlayBGM(bgmID);
    }

    // OFFSET 0x020048F0
    public static void Sound_Impl_PlayScene7BGM(ushort bgmID, int unused)
    {
        SoundSystem_GetParam(0x18);
        SoundPlayback.Sound_StopWaveOutAndSequences();
        Sound_LoadSoundEffectsForSceneWithState(7);
        Sound_SetFieldBGMBankState(1);
        SoundPlayback.Sound_PlayBGM(bgmID);
    }

    // OFFSET 0x02004914
    private static void Sound_Impl_LoadSubSceneSoundData(byte scene)
    {
        Sound_LoadHeapStateBGM();
        Sound_LoadSoundEffectsForScene(scene);
        SoundSystem_SaveHeapState(SoundSystem_GetParam(0x1C));
    }

    // OFFSET 0x02004930
    private static void Sound_Impl_PlayCutsceneBGM(byte scene, ushort bgmID, int unused)
    {
        SoundSystem_GetParam(0x18);
        SoundPlayback.Sound_StopWaveOutAndSequences();
        Sound_LoadSoundEffectsForSceneWithState(scene);
        SoundPlayback.Sound_PlayBGM(bgmID);
    }

    // OFFSET 0x02004950
    public static void Sound_SwapBGM(ushort bgmID)
    {
        SoundSystem_Get();
        Sound_Impl_PauseOrStopFieldBGM();
        SoundPlayback.Sound_PlayBGM(bgmID);
    }

    // OFFSET 0x02004964
    public static void Sound_LoadHeapStateBGM()
    {
        SoundSystem_LoadHeapState(Sound_GetHeapState(4));
    }

    // OFFSET 0x02004974
    public static int Sound_GetHeapState(int state)
    {
        SoundSystem_Get();
        if (state >= HeapStateCount)
        {
            ErrorHandling.GF_AssertFail();
            return SoundSystem_GetParam(0x1B)!.Value;
        }
        return SoundSystem_GetParam(ParamHeapStates + state)!.Value;
    }

    // OFFSET 0x020049F4
    public static void Sound_SetBGMPlayerPaused(byte playerID, int paused)
    {
        SoundParam flag;
        int handleType;
        if (playerID == 1)
        {
            flag = SoundSystem_GetParam(0xC)!;
            handleType = 0;
        }
        else if (playerID == 7)
        {
            flag = SoundSystem_GetParam(0xD)!;
            handleType = 7;
        }
        else
        {
            return;
        }
        if (paused == 0)
        {
            Sound_SetCurrentBGM((ushort)Sound_GetSequenceIDFromSoundHandle(SoundSystem_GetSoundHandle(handleType)));
        }
        Audio.PauseHandle(SoundSystem_GetSoundHandle(handleType), paused != 0);
        flag.Value = (byte)paused;
    }

    // OFFSET 0x02004A3C
    public static void Sound_ClearBGMPauseFlags()
    {
        SoundParam player = SoundSystem_GetParam(0xC)!;
        SoundParam fanfare = SoundSystem_GetParam(0xD)!;
        player.Value = 0;
        fanfare.Value = 0;
    }

    // OFFSET 0x02004A54
    public static void Sound_FadeVolumeForHandle(int handleType, int targetVolume, int frames)
    {
        Audio.MoveVolume(SoundSystem_GetSoundHandle(handleType), targetVolume, frames);
    }

    // OFFSET 0x02004A68
    public static void Sound_SetInitialVolumeForHandle(int handleType, int volume)
    {
        if (volume < 0)
        {
            volume = 0;
        }
        if (volume > 127)
        {
            volume = 127;
        }
        Audio.SetInitialVolume(SoundSystem_GetSoundHandle(handleType), volume);
    }

    // OFFSET 0x02004A84
    public static void Sound_AdjustVolumeForVoiceChat(int seqID)
    {
        byte player = Sound_GetPlayerForSequence((ushort)seqID);
        Sound_AdjustVolumeForVoiceChatEx(seqID, SoundSystem_GetSoundHandleTypeFromPlayerID(player));
    }

    // OFFSET 0x02004AA0
    public static void Sound_AdjustVolumeForVoiceChatEx(int seqID, int handleType)
    {
        SequenceInfo? info = Audio.GetSequenceInfo(seqID);
        int volume;
        if (handleType == 1 || handleType == 8)
        {
            volume = 127;
        }
        else
        {
            if (info == null)
            {
                return;
            }
            volume = (byte)info.Volume;
        }
        if (CommSystem.CommSys_IsVoiceChatActive() == 1)
        {
            Sound_SetInitialVolumeForHandle(handleType, volume / 5);
        }
    }

    // OFFSET 0x02004AD4
    public static void Sound_SetInitialVolumeForSequence(ushort seqID, int volume)
    {
        byte player = Sound_GetPlayerForSequence(seqID);
        Sound_SetInitialVolumeForHandle(SoundSystem_GetSoundHandleTypeFromPlayerID(player), volume);
    }

    // OFFSET 0x02004AE8
    public static bool Sound_PlaySequenceWithPlayer(int handleType, int playerID, ushort seqID)
    {
        return Audio.StartSequence(SoundSystem_GetSoundHandle(handleType), playerID, seqID);
    }

    // OFFSET 0x02004B04
    public static int Sound_GetNumberOfPlayingSequencesForPlayer(int playerID)
    {
        if (playerID < 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        return Audio.CountPlayingSequences(playerID);
    }

    // OFFSET 0x02004B18
    public static byte Sound_GetPlayerForSequence(ushort seqID)
    {
        if (seqID == 0)
        {
            return 0xFF;
        }
        SequenceInfo? info = Audio.GetSequenceInfo(seqID);
        if (info == null)
        {
            return 0xFF;
        }
        return (byte)info.Player;
    }

    // OFFSET 0x02004B34
    public static int Sound_GetSequenceIDFromSoundHandle(SoundHandle handle)
    {
        return Audio.GetSequenceNumber(handle);
    }

    // OFFSET 0x02004B3C
    public static BankInfo? Sound_GetBankInfoForSequence(int seqID)
    {
        return Audio.GetBankInfo(Sound_GetBankIDFromSequenceID(seqID));
    }

    // OFFSET 0x02004B48
    public static ushort Sound_GetBankIDFromSequenceID(int seqID)
    {
        SequenceInfo? info = Audio.GetSequenceInfo(seqID);
        if (info == null)
        {
            return 0;
        }
        return (ushort)info.Bank;
    }

    // OFFSET 0x02004B5C
    public static int Sound_StartMicAutoSampling(object param)
    {
        return Audio.StartMicAutoSampling(param);
    }

    // OFFSET 0x02004B64
    public static int Sound_StopMicAutoSampling()
    {
        SoundSystem_Get();
        return Audio.StopMicAutoSampling();
    }

    // OFFSET 0x02004B70
    public static int Sound_StartMicManualSampling(int type, object buffer, object? callback, object? param)
    {
        return Audio.StartMicManualSampling(type, buffer, callback, param);
    }

    // OFFSET 0x02004B78
    public static SoundParam Sound_GetWaveOutHandle(int channel)
    {
        SoundSystem_Get();
        SoundParam primaryAllocated = SoundSystem_GetParam(0x10)!;
        SoundParam secondaryAllocated = SoundSystem_GetParam(0x11)!;
        if (channel != WaveOutPrimary && channel != WaveOutSecondary)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (channel == WaveOutPrimary && (byte)primaryAllocated.Value == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (channel == WaveOutSecondary && (byte)secondaryAllocated.Value == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (channel == WaveOutPrimary)
        {
            return SoundSystem_GetParam(0)!;
        }
        return SoundSystem_GetParam(1)!;
    }

    // OFFSET 0x02004BCC
    public static bool Sound_AllocateWaveOutChannel(int channel)
    {
        SoundSystem_Get();
        SoundParam primaryAllocated = SoundSystem_GetParam(0x10)!;
        SoundParam secondaryAllocated = SoundSystem_GetParam(0x11)!;
        if (channel != WaveOutPrimary && channel != WaveOutSecondary)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (channel == WaveOutPrimary)
        {
            if ((byte)primaryAllocated.Value == 0)
            {
                SoundParam handle = SoundSystem_GetParam(0)!;
                handle.Reference = Audio.AllocWaveOut(channel);
                if (handle.Reference == null)
                {
                    return false;
                }
                primaryAllocated.Value = 1;
            }
            else
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        else if ((byte)secondaryAllocated.Value == 0)
        {
            SoundParam handle = SoundSystem_GetParam(1)!;
            handle.Reference = Audio.AllocWaveOut(channel);
            if (handle.Reference == null)
            {
                return false;
            }
            secondaryAllocated.Value = 1;
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        return true;
    }

    // OFFSET 0x02004C4C
    public static void Sound_FreeWaveOutChannel(int channel)
    {
        SoundSystem_Get();
        SoundParam primaryAllocated = SoundSystem_GetParam(0x10)!;
        SoundParam secondaryAllocated = SoundSystem_GetParam(0x11)!;
        if (channel != WaveOutPrimary && channel != WaveOutSecondary)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        SoundParam allocated = channel == WaveOutPrimary ? primaryAllocated : secondaryAllocated;
        if ((byte)allocated.Value == 1)
        {
            Audio.FreeWaveOut((WaveOutChannel)Sound_GetWaveOutHandle(channel).Reference!);
            allocated.Value = 0;
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02004CB4
    public static bool Sound_PlayWaveOut(WaveOutParam param, int channel)
    {
        bool started = Audio.StartWaveOut(
            (WaveOutChannel)param.Handle.Reference!,
            param.Format,
            param.Data,
            param.Loop != 0,
            param.LoopStart,
            param.DataLength,
            param.SampleRate,
            param.Volume,
            param.Speed,
            param.Pan);
        if (!started)
        {
            Sound_FreeWaveOutChannel(channel);
        }
        return started;
    }

    // OFFSET 0x02004CF4
    public static void Sound_StopWaveOut(int channel)
    {
        Audio.StopWaveOut((WaveOutChannel)Sound_GetWaveOutHandle(channel).Reference!);
    }

    // OFFSET 0x02004D04
    public static bool Sound_IsWaveOutPlaying(int channel)
    {
        return Audio.IsWaveOutPlaying((WaveOutChannel)Sound_GetWaveOutHandle(channel).Reference!);
    }

    // OFFSET 0x02004D14
    public static void Sound_SetWaveOutPan(int channel, byte pan)
    {
        if (pan > 127)
        {
            pan = 127;
        }
        ((WaveOutChannel)Sound_GetWaveOutHandle(channel).Reference!).Pan = pan;
    }

    // OFFSET 0x02004D2C
    public static void Sound_SetWaveOutSpeed(int channel, uint speed)
    {
        ((WaveOutChannel)Sound_GetWaveOutHandle(channel).Reference!).Speed = (int)speed;
    }

    // OFFSET 0x02004D40
    public static void Sound_SetWaveOutVolume(int channel, int volume)
    {
        if (CommSystem.CommSys_IsVoiceChatActive() == 1)
        {
            ((WaveOutChannel)Sound_GetWaveOutHandle(channel).Reference!).Volume = volume / 5;
        }
        else
        {
            ((WaveOutChannel)Sound_GetWaveOutHandle(channel).Reference!).Volume = volume;
        }
    }

    // OFFSET 0x02004D78
    public static bool Sound_PlayWaveOutReversed(ushort waveArcID, int volume, int pan, int channel, int heapID)
    {
        SoundSystem_Get();
        SoundParam buffer = SoundSystem_GetParam(0x22)!;
        if (channel != WaveOutPrimary && channel != WaveOutSecondary)
        {
            ErrorHandling.GF_AssertFail();
        }
        int fileId = Audio.GetWaveArcFileId(waveArcID);
        if (fileId < 0)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        int size = Audio.GetFileSize(fileId);
        if (size == 0)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        if (channel == WaveOutPrimary)
        {
            byte[] data = new byte[size];
            buffer.Reference = data;
            if (Audio.ReadFile(fileId, data, size) == -1)
            {
                ErrorHandling.GF_AssertFail();
                return false;
            }
            Sound_Impl_ReverseBuffer(data, (uint)size);
        }
        WaveOutParam param = new()
        {
            Handle = Sound_GetWaveOutHandle(channel),
            Format = 0,
            Data = (byte[])buffer.Reference!,
            Loop = 0,
            LoopStart = 0,
            DataLength = size,
            SampleRate = 0x3443,
            Volume = volume,
            Speed = 0x6000,
            Pan = pan,
        };
        bool result = Sound_PlayWaveOut(param, channel);
        Sound_SetWaveOutVolume(channel, volume);
        SoundSystem_GetParam(0xF)!.Value = 1;
        return result;
    }

    // OFFSET 0x02004E64
    private static void Sound_Impl_ReverseBuffer(byte[] buffer, uint size)
    {
        for (uint i = 0; i < size / 2; i++)
        {
            uint other = size - 1 - i;
            byte value = buffer[i];
            buffer[i] = buffer[other];
            buffer[other] = value;
        }
    }

    // OFFSET 0x02004E84
    public static void Sound_StopWaveOutReversed(int channel)
    {
        SoundSystem_Get();
        SoundParam reversed = SoundSystem_GetParam(0xF)!;
        SoundParam buffer = SoundSystem_GetParam(0x22)!;
        if (channel != WaveOutPrimary && channel != WaveOutSecondary)
        {
            ErrorHandling.GF_AssertFail();
        }
        Sound_StopWaveOut(channel);
        if ((byte)reversed.Value == 1)
        {
            reversed.Value = 0;
            buffer.Reference = null;
        }
    }

    // OFFSET 0x02004EC0
    public static bool Sound_IsCaptureActive()
    {
        return Audio.CaptureActive;
    }

    // OFFSET 0x02004EC8
    public static bool Sound_StartReverb(int volume)
    {
        SoundSystem_Get();
        SoundSystem_GetParam(3);
        return Audio.StartReverb(16000, volume);
    }

    // OFFSET 0x02004EEC
    public static void Sound_StopReverb(int frames)
    {
        Audio.StopReverb(frames);
    }

    // OFFSET 0x02004EF4
    public static void Sound_SetReverbVolume(int targetVolume, int frames)
    {
        Audio.SetReverbVolume(targetVolume, frames);
    }

    // OFFSET 0x02004EFC
    public static bool Sound_StartFilter()
    {
        SoundSystem_Get();
        SoundParam state = SoundSystem_GetParam(4)!;
        state.Value = 0;
        short[] history = new short[16];
        state.Reference = history;
        SoundSystem_GetParam(3);
        return Audio.StartEffect(22000, (left, right, length) => Sound_Impl_FilterCallback(left, right, (uint)length, 1, history));
    }

    // OFFSET 0x02004F44
    public static void Sound_StopFilter()
    {
        Audio.StopEffect();
    }

    // OFFSET 0x02004F4C
    public static void Sound_SetFilterSize(int size)
    {
        SoundParam filterSize = SoundSystem_GetParam(0x14)!;
        if (size > 8)
        {
            size = 8;
        }
        else if (size < 0)
        {
            size = 0;
        }
        filterSize.Value = (byte)size;
    }

    // OFFSET 0x02004F68
    public static void Sound_SetPitchForHandle(int handleType, ushort tracks, int pitch)
    {
        Audio.SetTrackPitch(SoundSystem_GetSoundHandle(handleType), tracks, pitch);
    }

    // OFFSET 0x02004F7C
    public static void Sound_SetPitchForSequence(ushort seqID, ushort tracks, int pitch)
    {
        byte player = Sound_GetPlayerForSequence(seqID);
        Sound_SetPitchForHandle(SoundSystem_GetSoundHandleTypeFromPlayerID(player), tracks, pitch);
    }

    // OFFSET 0x02004F94
    public static void Sound_SetPanForHandle(int handleType, ushort tracks, int pan)
    {
        Audio.SetTrackPan(SoundSystem_GetSoundHandle(handleType), tracks, pan);
    }

    // OFFSET 0x02004FA8
    public static void Sound_SetTempoRatioForHandle(int handleType, int tempoRatio)
    {
        Audio.SetTempoRatio(SoundSystem_GetSoundHandle(handleType), tempoRatio);
    }

    // OFFSET 0x02004FB8
    public static void Sound_SetPlaybackMode(int mode)
    {
        Audio.SetMono(mode != 0);
        sPlaybackMode = mode;
    }

    // OFFSET 0x02004FCC
    public static void Sound_SetFadeCounter(int frames)
    {
        SoundSystem_GetParam(7)!.Value = frames;
    }

    // OFFSET 0x02004FDC
    public static void Sound_SetFollowUpWaitFrames(int frames)
    {
        SoundSystem_GetParam(8)!.Value = frames;
    }

    // OFFSET 0x02004FEC
    public static int Sound_UpdateFollowUpWaitFrames()
    {
        SoundParam frames = SoundSystem_GetParam(8)!;
        if ((ushort)frames.Value == 0)
        {
            frames.Value = 0;
            return 0;
        }
        frames.Value = (ushort)(frames.Value - 1);
        return frames.Value;
    }

    // OFFSET 0x0200500C
    public static void Sound_SetMasterVolume(int volume)
    {
        Audio.SetMasterVolume(volume);
    }

    // OFFSET 0x02005014
    public static byte[] Sound_GetWaveBuffer()
    {
        return sWaveBuffer;
    }

    // OFFSET 0x0200501C
    public static void Sound_SetFieldBGMBankState(int state)
    {
        SoundSystem_GetParam(0x13)!.Value = (byte)state;
    }

    // OFFSET 0x0200502C
    public static bool Sound_FadeOutAndPlayBGM(byte unused1, ushort bgmID, int fadeOutFrames, int waitFrames, byte bankState, object? unused2)
    {
        SoundParam subScene = SoundSystem_GetParam(ParamSubScene)!;
        Sound_Impl_FadeToBGM(unused1, bgmID, fadeOutFrames, waitFrames, bankState, unused2);
        subScene.Value = 0;
        SoundSystem_SetState(5);
        return true;
    }

    // OFFSET 0x02005068
    public static bool Sound_FadeToBGM(byte unused1, ushort bgmID, int fadeOutFrames, int waitFrames, int fadeInFrames, byte bankState, object? unused2)
    {
        SoundParam fadeIn = SoundSystem_GetParam(9)!;
        Sound_Impl_FadeToBGM(unused1, bgmID, fadeOutFrames, waitFrames, bankState, unused2);
        fadeIn.Value = fadeInFrames;
        SoundSystem_SetState(6);
        return true;
    }

    // OFFSET 0x020050A4
    private static void Sound_Impl_FadeToBGM(byte unused1, ushort bgmID, int fadeOutFrames, int waitFrames, byte bankState, object? unused2)
    {
        SoundParam bankInfo = SoundSystem_GetParam(2)!;
        SoundPlayback.Sound_FadeOutBGM(0, fadeOutFrames);
        Sound_SetCurrentBGM(0);
        Sound_SetNextBGM(bgmID);
        Sound_SetFollowUpWaitFrames(waitFrames);
        bankInfo.Reference = Sound_GetBankInfoForSequence(bgmID);
        Sound_SetFieldBGMBankState(bankState);
    }

    // OFFSET 0x020050E0
    public static byte[]? Sound_WaveData_GetSamples(WaveData? data)
    {
        if (data == null)
        {
            return null;
        }
        return data.Samples;
    }

    // OFFSET 0x020050EC
    public static uint Sound_WaveData_GetLoopLength(WaveData? data)
    {
        if (data == null)
        {
            return 0;
        }
        return data.LoopLength;
    }

    // OFFSET 0x020050F8
    public static WaveData? Sound_LoadPokedexDataForSpecies(int species)
    {
        SoundSystem_LoadHeapState(Sound_GetHeapState(5));
        ushort waveArc = (ushort)species;
        if (waveArc != CrySpecialWaveArc && (species > CryLastWaveArc || species == 0))
        {
            waveArc = 1;
        }
        SoundSystem_LoadWaveArc(waveArc);
        return Sound_Impl_GetWaveDataForSpecies(waveArc);
    }

    // OFFSET 0x02005130
    private static WaveData? Sound_Impl_GetWaveDataForSpecies(int species)
    {
        SoundParam current = SoundSystem_GetParam(0x21)!;
        int waveArc = (ushort)species;
        if (waveArc != CrySpecialWaveArc && (species > CryLastWaveArc || species == 0))
        {
            waveArc = 1;
        }
        int fileId = Audio.GetWaveArcFileId(waveArc);
        if (fileId < 0)
        {
            ErrorHandling.GF_AssertFail();
            return null;
        }
        WaveData? data = Audio.GetWaveData(fileId, 0);
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
            return null;
        }
        current.Reference = data;
        return data;
    }

    // OFFSET 0x02005188
    public static uint Sound_GetNumberOfPlayedCrySamples(int unused, WaveData data, int pitch)
    {
        uint ticks = Sound_GetTicksForHandle(1);
        uint loopLength = data.LoopLength;
        int step = (pitch / 0x300) * 2;
        int factor;
        if (step < 0)
        {
            factor = -(0x20 / step);
        }
        else if (step == 0)
        {
            factor = 0x20;
        }
        else
        {
            factor = step << 5;
        }
        uint samples = ticks * (uint)factor;
        if (samples >= loopLength)
        {
            return 0;
        }
        return samples;
    }

    // OFFSET 0x020051C4
    public static uint Sound_GetTicksForHandle(int handleType)
    {
        return Audio.GetTicks(SoundSystem_GetSoundHandle(handleType));
    }

    // OFFSET 0x020051D0
    public static void Sound_WaveData_AccumulateAmplitudes(WaveData data, byte[] amplitudes, int numAmplitudes, int pitch)
    {
        sAmplitudeThresholds ??= RomData.Arm9(0x020E4D0C, 9);
        byte[] samples = data.Samples;
        uint end = Sound_GetNumberOfPlayedCrySamples(1, data, pitch);
        int start = (int)end - 100;
        if (start < 0)
        {
            start = 0;
        }
        int run = 0;
        int bucket = 0;
        sbyte sign = samples[start] < 0x80 ? (sbyte)1 : (sbyte)-1;
        for (uint i = (uint)start; i < end; i += 2)
        {
            bool crossed = false;
            byte sample = samples[i];
            if (sign > 0)
            {
                if (sample > 0x80)
                {
                    crossed = true;
                }
                else
                {
                    run++;
                }
            }
            else if (sample < 0x80)
            {
                crossed = true;
            }
            else
            {
                run++;
            }
            if (!crossed)
            {
                continue;
            }
            for (int k = 0; k < 9; k++)
            {
                if (run < sAmplitudeThresholds[k])
                {
                    bucket = k;
                    break;
                }
            }
            if (bucket >= numAmplitudes)
            {
                bucket = numAmplitudes - 1;
            }
            if (run != 0)
            {
                amplitudes[numAmplitudes - 1 - bucket]++;
            }
            run = 0;
            sign = samples[i] < 0x80 ? (sbyte)1 : (sbyte)-1;
        }
        for (int k = 0; k < numAmplitudes; k++)
        {
            if (amplitudes[k] >= 10)
            {
                amplitudes[k] = 9;
            }
        }
        for (int k = 0; k < numAmplitudes - 1; k++)
        {
            if (amplitudes[k] == 0 && amplitudes[k + 1] != 0)
            {
                amplitudes[k] = (byte)(amplitudes[k + 1] >> 1);
                return;
            }
        }
    }

    // OFFSET 0x020052C8
    private static void Sound_Impl_FilterCallback(short[] bufferL, short[] bufferR, uint length, int format, short[] state)
    {
        SoundParam filterSize = SoundSystem_GetParam(0x14)!;
        if (format != 1)
        {
            length >>= 1;
        }
        int size = (byte)filterSize.Value;
        if (size == 0)
        {
            return;
        }
        short[] saved = new short[size * 2];
        for (int i = 0; i < size; i++)
        {
            saved[i * 2] = bufferL[length - size + i];
            saved[i * 2 + 1] = bufferR[length - size + i];
        }
        for (int i = (int)length - 1; i >= size - 1; i--)
        {
            int sum = 0;
            for (int k = 0; k < size; k++)
            {
                sum += bufferL[i - k];
            }
            bufferL[i] = (short)(sum / size);
            sum = 0;
            for (int k = 0; k < size; k++)
            {
                sum += bufferR[i - k];
            }
            bufferR[i] = (short)(sum / size);
        }
        for (int i = 0; i < size; i++)
        {
            state[i * 2] = saved[i * 2];
            state[i * 2 + 1] = saved[i * 2 + 1];
        }
    }

    // OFFSET 0x020053C0
    private static void Sound_SetBGMAllocatableChannels(ushort channels)
    {
        Audio.SetAllocatableChannels(7, channels);
    }

    // OFFSET 0x020053CC
    public static void Sound_ConfigureBGMChannelsAndReverb(int config)
    {
        if (config == 0)
        {
            Sound_SetBGMAllocatableChannels(0x7FF);
            Sound_StopReverb(0);
        }
        else if (config == 1)
        {
            Sound_SetBGMAllocatableChannels(0x7FFF);
            Sound_StartReverb(30);
        }
        else
        {
            Sound_SetBGMAllocatableChannels(0x7FFF);
            Sound_StartReverb(15);
        }
        Sound_IsCaptureActive();
    }

    // OFFSET 0x0200540C
    private static void Sound_Impl_PauseOrStopFieldBGM()
    {
        if (SoundPlayback.Sound_IsFadeActive() == 0
            && Sound_GetSequenceIDFromSoundHandle(SoundSystem_GetSoundHandle(0)) != -1
            && Sound_Impl_GetCurrentBGM() != 0x47E)
        {
            SoundPlayback.Sound_StopAll();
            Sound_SetBGMPlayerPaused(1, 1);
            return;
        }
        SoundPlayback.Sound_StopWaveOutAndSequences();
    }

    // OFFSET 0x0200544C
    public static void Sound_SetPlayerVolume(int playerID, int volume)
    {
        Audio.SetPlayerVolume(playerID, volume);
    }

    // OFFSET 0x02005454
    public static void Sound_Set2PokemonCriesAllowed(int allowed)
    {
        SoundSystem_GetParam(0x35)!.Value = (byte)allowed;
    }

    // OFFSET 0x02005464
    public static void Sound_SetRecordedChatotCryDisabled(int disabled)
    {
        SoundSystem_GetParam(0x36)!.Value = (byte)disabled;
    }

    private static int LoadGroup(ushort group)
    {
        return SoundSystem_LoadSoundGroup(group) ? 1 : 0;
    }

    private static int LoadBankAndWaveArc(ushort id)
    {
        SoundSystem_LoadBank(id);
        return SoundSystem_LoadWaveArc(id) ? 1 : 0;
    }
}
