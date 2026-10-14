using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed partial class Options
{
    public ushort Flags;
}

public sealed class SoundParam
{
    public int Value;
    public object? Reference;
}

public sealed class SoundSystem
{
    public const int ParamCount = 55;
    public const int HandleCount = 9;
    public const int HeapStateCount = 7;

    public const int ParamFanfareDelay = 7;
    public const int Param09 = 9;
    public const int ParamNextBGM = 11;
    public const int Param14 = 14;
    public const int ParamHeapStates = 23;
    public const int ParamChatotCry = 36;

    private const uint ArchivePathAddress = 0x020FECE0;
    private const int PlayerBGM = 7;
    private const int HandleBGM = 7;

    private static readonly SoundSystem sSoundSystem = new();
    private static int sState;
    private static int sField04;

    private static readonly int[] DelayedParams0 = { 41, 47 };
    private static readonly int[] DelayedParams1 = { 45, 51 };
    private static readonly int[] DelayedParams2 = { 42, 48 };
    private static readonly int[] DelayedParams3 = { 43, 49 };
    private static readonly int[] DelayedParams4 = { 44, 50 };
    private static readonly int[] DelayedCounters = { 46, 52 };

    public readonly SoundHandle[] Handles = CreateHandles();
    public readonly SoundParam[] Params = CreateParams();
    public ushort Unused0BCCFC;

    private static SoundHandle[] CreateHandles()
    {
        SoundHandle[] handles = new SoundHandle[HandleCount];
        for (int i = 0; i < HandleCount; i++)
        {
            handles[i] = new SoundHandle();
        }
        return handles;
    }

    private static SoundParam[] CreateParams()
    {
        SoundParam[] values = new SoundParam[ParamCount];
        for (int i = 0; i < ParamCount; i++)
        {
            values[i] = new SoundParam();
        }
        return values;
    }

    // OFFSET 0x02003B60
    public static void SoundSystem_Init(object chatotCry, Options options)
    {
        SoundSystem soundSys = SoundSystem_Get();
        Audio.Init();
        SoundSystem_InitMic();
        SoundSystem_InitHeapStates(soundSys);
        Audio.OpenArchive(ArchivePathAddress);
        SoundSystem_InitSoundHandles(soundSys);
        SoundSystem_LoadPersistentGroup(soundSys);
        sField04 = 0;
        soundSys.Params[ParamChatotCry].Reference = chatotCry;
        Sound.Sound_SetPlaybackMode((options.Flags >> 4) & 3);
    }

    // OFFSET 0x02003BD8
    public static void SoundSystem_Tick()
    {
        SoundSystem soundSys = SoundSystem_Get();
        if (!SoundSystem_IsFanfarePlaying())
        {
            SoundParam delay = soundSys.Params[ParamFanfareDelay];
            if (delay.Value > 0)
            {
                delay.Value--;
            }
            SoundSystem_UpdateState();
        }
        SoundChatot.Sound_UpdateChatotCry();
        for (int i = 0; i < 2; i++)
        {
            SoundParam counter = soundSys.Params[DelayedCounters[i]];
            if (counter.Value == 0)
            {
                continue;
            }
            counter.Value = (byte)(counter.Value - 1);
            if (counter.Value == 0)
            {
                SoundPlayback.Sound_PlayPokemonCryEx(
                    soundSys.Params[DelayedParams0[i]].Value,
                    (ushort)soundSys.Params[DelayedParams1[i]].Value,
                    soundSys.Params[DelayedParams2[i]].Value,
                    soundSys.Params[DelayedParams3[i]].Value,
                    soundSys.Params[DelayedParams4[i]].Value,
                    0);
            }
        }
        Audio.Update();
    }

    // OFFSET 0x02003C64
    private static void SoundSystem_UpdateState()
    {
        SoundSystem soundSys = SoundSystem_Get();
        switch (sState)
        {
            case 1:
                SoundSystem_SetState(2);
                break;
            case 3:
            case 4:
                if (SoundPlayback.Sound_IsFadeActive() == 0)
                {
                    SoundSystem_SetState(2);
                }
                break;
            case 5:
                if (SoundPlayback.Sound_IsFadeActive() == 0 && Sound.Sound_UpdateFollowUpWaitFrames() == 0)
                {
                    SoundSystem_StopBGM();
                    SoundPlayback.Sound_PlayBGM((ushort)soundSys.Params[ParamNextBGM].Value);
                }
                break;
            case 6:
                if (SoundPlayback.Sound_IsFadeActive() == 0 && Sound.Sound_UpdateFollowUpWaitFrames() == 0)
                {
                    SoundSystem_StopBGM();
                    SoundPlayback.Sound_PlayBGM((ushort)soundSys.Params[ParamNextBGM].Value);
                    SoundPlayback.Sound_FadeInBGM(0x7F, soundSys.Params[Param09].Value, 0);
                }
                break;
        }
    }

    // OFFSET 0x02003D0C
    public static void SoundSystem_SetState(int state)
    {
        SoundSystem soundSys = SoundSystem_Get();
        soundSys.Unused0BCCFC = 0;
        sState = state;
    }

    // OFFSET 0x02003D28
    private static bool SoundSystem_IsFanfarePlaying()
    {
        SoundSystem soundSys = SoundSystem_Get();
        if (Sound.Sound_GetNumberOfPlayingSequencesForPlayer(2) != 0)
        {
            return true;
        }
        if ((ushort)soundSys.Params[Param14].Value != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02003D54
    public static SoundSystem SoundSystem_Get()
    {
        return sSoundSystem;
    }

    // OFFSET 0x02003D5C
    public static SoundParam? SoundSystem_GetParam(int param)
    {
        SoundSystem soundSys = SoundSystem_Get();
        if (param < 0 || param >= ParamCount)
        {
            ErrorHandling.GF_AssertFail();
            return null;
        }
        return soundSys.Params[param];
    }

    // OFFSET 0x02004014
    public static int SoundSystem_SaveHeapState(SoundParam? state)
    {
        int level = Audio.SaveHeapState();
        if (level == -1)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (state != null)
        {
            state.Value = level;
        }
        return level;
    }

    // OFFSET 0x0200403C
    public static void SoundSystem_LoadHeapState(int state)
    {
        Audio.LoadHeapState(state);
    }

    // OFFSET 0x02004050
    public static bool SoundSystem_LoadSoundGroup(ushort group)
    {
        return Audio.LoadGroup(group);
    }

    // OFFSET 0x02004068
    public static bool SoundSystem_LoadSequence(ushort id)
    {
        return Audio.LoadSequence(id);
    }

    // OFFSET 0x02004080
    public static bool SoundSystem_LoadSequenceEx(ushort id, uint flags)
    {
        return Audio.LoadSequenceEx(id, flags);
    }

    // OFFSET 0x0200409C
    public static bool SoundSystem_LoadWaveArc(ushort id)
    {
        return Audio.LoadWaveArc(id);
    }

    // OFFSET 0x020040B4
    public static bool SoundSystem_LoadBank(ushort id)
    {
        return Audio.LoadBank(id);
    }

    // OFFSET 0x020040CC
    public static SoundHandle SoundSystem_GetSoundHandle(int type)
    {
        SoundSystem soundSys = SoundSystem_Get();
        if (type >= HandleCount)
        {
            ErrorHandling.GF_AssertFail();
            type = 0;
        }
        return soundSys.Handles[type];
    }

    // OFFSET 0x020040F0
    public static int SoundSystem_GetSoundHandleTypeFromPlayerID(int playerID)
    {
        switch (playerID)
        {
            case 0:
                return 1;
            case 1:
                return 0;
            case 2:
                return 2;
            case 3:
                return 3;
            case 4:
                return 4;
            case 5:
                return 5;
            case 6:
                return 6;
            case 7:
                return 7;
            default:
                ErrorHandling.GF_AssertFail();
                return 3;
        }
    }

    // OFFSET 0x0200413C
    private static void SoundSystem_InitHeapStates(SoundSystem soundSys)
    {
        soundSys.Unused0BCCFC = 0;
        foreach (SoundParam param in soundSys.Params)
        {
            param.Value = 0;
            param.Reference = null;
        }
        for (int i = 0; i < HeapStateCount; i++)
        {
            soundSys.Params[ParamHeapStates + i].Value = i + 1;
        }
    }

    // OFFSET 0x02004160
    private static void SoundSystem_InitSoundHandles(SoundSystem soundSys)
    {
        for (int i = 0; i < HandleCount; i++)
        {
            Audio.InitHandle(soundSys.Handles[i]);
        }
    }

    // OFFSET 0x0200417C
    private static void SoundSystem_LoadPersistentGroup(SoundSystem soundSys)
    {
        SoundSystem_SaveHeapState(soundSys.Params[ParamHeapStates]);
        SoundSystem_LoadSoundGroup(0);
        SoundSystem_SaveHeapState(soundSys.Params[ParamHeapStates + 1]);
    }

    // OFFSET 0x020041A0
    private static void SoundSystem_InitMic()
    {
        Audio.InitMicrophone();
        Audio.SetAmplifier(1);
        Audio.SetAmplifierGain(2);
    }

    // OFFSET 0x020041B4
    private static void SoundSystem_StopBGM()
    {
        Audio.StopPlayer(PlayerBGM, 0);
        Audio.ReleaseHandle(SoundSystem_GetSoundHandle(HandleBGM));
    }
}
