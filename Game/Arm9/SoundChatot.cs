using static PlatinumSharp.Game.Sound;
using static PlatinumSharp.Game.SoundPlayback;
using static PlatinumSharp.Game.SoundSystem;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class MicAutoParam
{
    public int Type;
    public byte[] Buffer = Array.Empty<byte>();
    public int Size;
    public int Rate;
    public int Loop;
    public object? Callback;
    public object? Argument;
}

public static class SoundChatot
{
    private const int ParamCryPlaying = 0x1E;
    private const int ParamUsingDefaultCry = 0x1F;
    private const int ParamChatotCry = 0x24;
    private const int WaveOutPrimary = 14;
    private const int CrySamples = 2000;
    private const int SpeciesChatot = 0x1B9;

    // OFFSET 0x02006224
    public static bool Sound_UpdateChatotCry()
    {
        SoundParam primaryAllocated = SoundSystem_GetParam(0x10)!;
        SoundParam playing = SoundSystem_GetParam(ParamCryPlaying)!;
        if ((byte)playing.Value != 1)
        {
            return false;
        }
        if ((byte)primaryAllocated.Value == 1)
        {
            if (!Sound_IsWaveOutPlaying(WaveOutPrimary))
            {
                Sound_StopChatotCry();
                return true;
            }
            return false;
        }
        Sound_StopChatotCry();
        return true;
    }

    // OFFSET 0x02006260
    public static bool Sound_IsRecordedChatotCryPlayable(object? cry)
    {
        SoundParam usingDefault = SoundSystem_GetParam(ParamUsingDefaultCry)!;
        SoundParam recordedCryDisabled = SoundSystem_GetParam(0x36)!;
        if (ChatotCry.ChatotCry_IsValid((ChatotCry)cry!) == 0)
        {
            return false;
        }
        if ((byte)recordedCryDisabled.Value == 1)
        {
            return false;
        }
        if ((byte)usingDefault.Value == 1)
        {
            return false;
        }
        return true;
    }

    // OFFSET 0x0200629C
    public static bool Sound_Impl_PlayChatotCry(object? cry, uint unused, int volume, int pan)
    {
        byte[] waveBuffer = Sound_GetWaveBuffer();
        SoundParam playing = SoundSystem_GetParam(ParamCryPlaying)!;
        if (!Sound_IsRecordedChatotCryPlayable(cry))
        {
            return false;
        }
        Sound_StopPokemonCries(0);
        Sound_StopChatotCry();
        Sound_AllocateWaveOutChannel(WaveOutPrimary);
        ushort variation = (ushort)(MathUtil.LCRNG_Next() % 0x2000);
        ChatotCry.ChatotCry_GetUpsampledAudio((sbyte[])(object)waveBuffer, ChatotCry.ChatotCry_GetRawAudio((ChatotCry)cry!));
        WaveOutParam param = new()
        {
            Handle = Sound_GetWaveOutHandle(WaveOutPrimary),
            Format = 0,
            Data = Sound_GetWaveBuffer(),
            Loop = 0,
            LoopStart = 0,
            DataLength = CrySamples,
            SampleRate = CrySamples,
            Volume = volume,
            Speed = variation + 0x8000,
            Pan = pan / 2 + 0x40,
        };
        bool result = Sound_PlayWaveOut(param, WaveOutPrimary);
        Sound_SetWaveOutVolume(WaveOutPrimary, volume);
        playing.Value = 1;
        Sound_SetUsingDefaultChatotCry(0);
        return result;
    }

    // OFFSET 0x02006350
    public static void Sound_StopChatotCry()
    {
        SoundParam primaryAllocated = SoundSystem_GetParam(0x10)!;
        SoundParam playing = SoundSystem_GetParam(ParamCryPlaying)!;
        if ((byte)primaryAllocated.Value == 1)
        {
            Sound_StopWaveOutReversed(WaveOutPrimary);
            Sound_FreeWaveOutChannel(WaveOutPrimary);
        }
        playing.Value = 0;
    }

    // OFFSET 0x0200637C
    public static int Sound_StartRecordingChatotCry()
    {
        MicAutoParam param = new()
        {
            Type = 2,
            Buffer = Sound_GetWaveBuffer(),
            Size = CrySamples,
        };
        if ((param.Size & 0x1F) != 0)
        {
            param.Size &= ~0x1F;
        }
        param.Rate = 0x4174;
        param.Loop = 0;
        param.Callback = null;
        param.Argument = null;
        return Sound_StartMicAutoSampling(param);
    }

    // OFFSET 0x020063B8
    public static int Sound_StopRecordingChatotCry()
    {
        return Sound_StopMicAutoSampling();
    }

    // OFFSET 0x020063C0
    public static void Sound_StoreRecordedChatotCry(object cry)
    {
        ChatotCry.ChatotCry_StoreAudio((ChatotCry)cry, (sbyte[])(object)Sound_GetWaveBuffer());
    }

    // OFFSET 0x020063D4
    public static void Sound_SetUsingDefaultChatotCry(byte value)
    {
        SoundSystem_GetParam(ParamUsingDefaultCry)!.Value = value;
    }

    // OFFSET 0x020063E4
    public static bool Sound_PlayChatotCry(object? cry, uint unused, int volume, int pan)
    {
        SoundParam stored = SoundSystem_GetParam(ParamChatotCry)!;
        bool result = Sound_Impl_PlayChatotCry(cry ?? stored.Reference, unused, volume, pan);
        if (!result)
        {
            Sound_SetUsingDefaultChatotCry(1);
            return Sound_PlayPokemonCryEx(0, SpeciesChatot, pan, volume, 0xB, 0);
        }
        return result;
    }

    // OFFSET 0x02006438
    public static bool Sound_PlayDelayedChatotCry(object? cry, uint unused, int volume, int pan, byte delay)
    {
        SoundParam stored = SoundSystem_GetParam(ParamChatotCry)!;
        bool result = Sound_Impl_PlayChatotCry(cry ?? stored.Reference, unused, volume, pan);
        if (!result)
        {
            Sound_SetUsingDefaultChatotCry(1);
            Sound_PlayDelayedPokemonCryEx(0, SpeciesChatot, pan, volume, 0xB, delay, 0);
            return true;
        }
        return result;
    }

    // OFFSET 0x02006494
    public static int Sound_GetChatterActivationParameter(object? chatotCry)
    {
        if (ChatotCry.ChatotCry_IsValid((ChatotCry)chatotCry!) == 0)
        {
            return 0;
        }
        int sample = (sbyte)ChatotCry.ChatotCry_GetRawAudio((ChatotCry)chatotCry!)[0xF];
        if (sample < -30)
        {
            return 1;
        }
        if (sample >= 30 && sample < 128)
        {
            return 2;
        }
        return 0;
    }

    // OFFSET 0x020064C8
    public static bool Sound_CanPlayChatotCry(int cryMod)
    {
        switch (cryMod)
        {
            case 0:
            case 1:
            case 5:
            case 11:
            case 12:
                return true;
            default:
                return false;
        }
    }
}
