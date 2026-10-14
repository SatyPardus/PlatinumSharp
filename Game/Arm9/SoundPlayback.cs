using static PlatinumSharp.Game.Sound;
using static PlatinumSharp.Game.SoundSystem;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class SoundPlayback
{
    private const int PlayerField = 1;
    private const int PlayerBGM = 7;
    private const int NoPlayer = 0xFF;
    private const int WaveOutPrimary = 14;
    private const int WaveOutSecondary = 15;
    private const int FirstEffectHandle = 3;
    private const int EffectHandleCount = 4;
    private const int HandleCry = 1;
    private const int HandleCryEcho = 8;
    private const int CrySequence = 2;
    private const int CryChatot = 0x1B9;
    private const int CryShayminSky = 0x1EE;
    private const int CryLast = 0x1EF;
    private const int SpeciesShaymin = 0x1EC;
    private const int HandleFanfare = 2;

    // OFFSET 0x02005474
    public static bool Sound_PlayBasicBGM(ushort seqID)
    {
        int handleType = SoundSystem_GetSoundHandleTypeFromPlayerID(Sound_GetPlayerForSequence(seqID));
        bool result = Audio.StartSequence(SoundSystem_GetSoundHandle(handleType), -1, seqID);
        Sound_Impl_HandleBGMChange(seqID, handleType);
        return result;
    }

    // OFFSET 0x0200549C
    public static bool Sound_PlayBGM(ushort bgmID)
    {
        byte player = Sound_GetPlayerForSequence(bgmID);
        int handleType = SoundSystem_GetSoundHandleTypeFromPlayerID(player);
        bool result;
        if (player == PlayerBGM)
        {
            result = Sound_Impl_PlayBGM(bgmID, player, handleType);
        }
        else if (player == PlayerField)
        {
            result = Sound_Impl_PlayFieldBGM(bgmID, player, handleType);
        }
        else
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        Sound_SetFieldBGMBankState(0);
        Sound_Impl_HandleBGMChange(bgmID, handleType);
        return result;
    }

    // OFFSET 0x020054EC
    private static void Sound_Impl_HandleBGMChange(ushort seqID, int handleType)
    {
        Sound_SetCurrentBGM(seqID);
        Sound_AdjustVolumeForVoiceChatEx(seqID, handleType);
        SoundSystem_SetState(1);
    }

    // OFFSET 0x02005508
    private static bool Sound_Impl_PlayBGM(ushort seqID, byte playerID, int handleType)
    {
        SoundSystem_LoadHeapState(Sound_GetHeapState(3));
        SoundSystem_LoadSequence(seqID);
        SoundSystem_SaveHeapState(SoundSystem_GetParam(0x1B));
        return Audio.StartSequence(SoundSystem_GetSoundHandle(handleType), -1, seqID);
    }

    // OFFSET 0x02005538
    private static bool Sound_Impl_PlayFieldBGM(ushort seqID, byte playerID, int handleType)
    {
        SoundSystem_GetParam(0x13);
        SoundParam fieldBGM = SoundSystem_GetParam(0x20)!;
        int current = Sound_GetSequenceIDFromSoundHandle(SoundSystem_GetSoundHandle(0));
        Sound_LoadSoundDataForFieldBGM(seqID, Sound_GetBankIDFromSequenceID(current));
        SoundHandle handle = SoundSystem_GetSoundHandle(handleType);
        int bank = Sound_GetBankIDFromSequenceID((ushort)fieldBGM.Value);
        return Audio.StartSequenceEx(handle, -1, bank, -1, seqID);
    }

    // OFFSET 0x02005588
    public static bool Sound_SetBGM(byte scene, ushort seqID)
    {
        if (scene != 4)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        if (Sound_GetPlayerForSequence(seqID) != PlayerBGM)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        SoundSystem_LoadSequenceEx(seqID, 1);
        bool result = Audio.StartSequence(SoundSystem_GetSoundHandle(7), -1, seqID);
        Sound_SetCurrentBGM(seqID);
        SoundSystem_SetState(1);
        return result;
    }

    // OFFSET 0x020055D0
    public static void Sound_StopBGM(ushort bgmID, int fadeOutFrames)
    {
        Audio.StopSequenceByNumber(bgmID, fadeOutFrames);
        byte player = Sound_GetPlayerForSequence(bgmID);
        if (player != NoPlayer)
        {
            Audio.ReleaseHandle(SoundSystem_GetSoundHandle(SoundSystem_GetSoundHandleTypeFromPlayerID(player)));
        }
        Sound_Impl_ResetBGM();
    }

    // OFFSET 0x020055F4
    private static void Sound_Impl_ResetBGM()
    {
        Sound_SetCurrentBGM(0);
        Sound_SetNextBGM(0);
        SoundSystem_SetState(0);
    }

    // OFFSET 0x0200560C
    public static void Sound_FadeInBGM(int targetVolume, int frames, int fadeInType)
    {
        byte player = Sound_GetPlayerForSequence(Sound_Impl_GetCurrentBGM());
        if (player == NoPlayer)
        {
            return;
        }
        int handleType = SoundSystem_GetSoundHandleTypeFromPlayerID(player);
        if (fadeInType == 0)
        {
            Sound_FadeVolumeForHandle(handleType, 0, 0);
        }
        Sound_FadeVolumeForHandle(handleType, targetVolume, frames);
        Sound_SetFadeCounter(frames);
        SoundSystem_SetState(3);
    }

    // OFFSET 0x0200564C
    public static void Sound_FadeOutBGM(int targetVolume, int frames)
    {
        byte player = Sound_GetPlayerForSequence(Sound_Impl_GetCurrentBGM());
        if (player == NoPlayer)
        {
            return;
        }
        if (Sound_IsFadeActive() == 0)
        {
            Sound_FadeVolumeForHandle(SoundSystem_GetSoundHandleTypeFromPlayerID(player), targetVolume, frames);
            Sound_SetFadeCounter(frames);
        }
        SoundSystem_SetState(4);
    }

    // OFFSET 0x02005684
    public static int Sound_IsFadeActive()
    {
        return (ushort)SoundSystem_GetParam(7)!.Value;
    }

    // OFFSET 0x02005690
    public static int Sound_IsSequencePlaying(ushort seqID)
    {
        return Sound_GetNumberOfPlayingSequencesForPlayer(Sound_GetPlayerForSequence(seqID));
    }

    // OFFSET 0x0200569C
    public static void Sound_StopWaveOutAndSequences()
    {
        SoundParam primaryAllocated = SoundSystem_GetParam(0x10)!;
        SoundParam secondaryAllocated = SoundSystem_GetParam(0x11)!;
        Audio.StopAllSequences(0);
        if ((byte)primaryAllocated.Value == 1)
        {
            Sound_StopWaveOut(WaveOutPrimary);
        }
        if ((byte)secondaryAllocated.Value == 1)
        {
            Sound_StopWaveOut(WaveOutSecondary);
        }
        SoundSystem_SetState(0);
    }

    // OFFSET 0x020056D4
    public static void Sound_StopAll()
    {
        SoundParam primaryAllocated = SoundSystem_GetParam(0x10)!;
        SoundParam secondaryAllocated = SoundSystem_GetParam(0x11)!;
        Audio.StopSequence(SoundSystem_GetSoundHandle(7), 0);
        Sound_Impl_ResetBGM();
        for (int i = 0; i < 4; i++)
        {
            Sound_StopEffectFromHandle(i + 3, 0);
        }
        Sound_StopPokemonCries(0);
        if ((byte)primaryAllocated.Value == 1)
        {
            Sound_StopWaveOut(WaveOutPrimary);
        }
        if ((byte)secondaryAllocated.Value == 1)
        {
            Sound_StopWaveOut(WaveOutSecondary);
        }
    }

    // OFFSET 0x02005728
    public static bool Sound_PlayPannedEffect(ushort seqID, int pan)
    {
        bool result = Sound_PlayEffect(seqID);
        Sound_PanEffect(seqID, 0xFFFF, pan);
        return result;
    }

    // OFFSET 0x02005748
    public static bool Sound_PlayEffect(int seqID)
    {
        ushort sequence = (ushort)seqID;
        int handleType = SoundSystem_GetSoundHandleTypeFromPlayerID(Sound_GetPlayerForSequence(sequence));
        bool result = Audio.StartSequence(SoundSystem_GetSoundHandle(handleType), -1, sequence);
        Sound_AdjustVolumeForVoiceChatEx(sequence, handleType);
        return result;
    }

    // OFFSET 0x02005770
    public static bool Sound_PlayEffectOnPlayer(ushort seqID, int playerID)
    {
        int handleType = SoundSystem_GetSoundHandleTypeFromPlayerID(playerID);
        bool result = Audio.StartSequenceEx(SoundSystem_GetSoundHandle(handleType), playerID, -1, -1, seqID);
        Sound_AdjustVolumeForVoiceChatEx(seqID, handleType);
        return result;
    }

    // OFFSET 0x020057A4
    public static void Sound_StopEffect(ushort seqID, int fadeOutFrames)
    {
        Audio.StopSequenceByNumber(seqID, fadeOutFrames);
    }

    // OFFSET 0x020057AC
    public static void Sound_StopEffectFromHandle(int handleType, int fadeOutFrames)
    {
        Audio.StopSequence(SoundSystem_GetSoundHandle(handleType), fadeOutFrames);
    }

    // OFFSET 0x020057BC
    public static void Sound_StopAllEffects(int fadeOutFrames)
    {
        for (int i = 0; i < EffectHandleCount; i++)
        {
            Sound_StopEffectFromHandle(FirstEffectHandle + i, 0);
        }
    }

    // OFFSET 0x020057D4
    public static int Sound_IsEffectPlaying(ushort seqID)
    {
        return Sound_GetNumberOfPlayingSequencesForPlayer(Sound_GetPlayerForSequence(seqID));
    }

    // OFFSET 0x020057E0
    public static bool Sound_IsAnyEffectPlaying()
    {
        for (int i = 0; i < EffectHandleCount; i++)
        {
            if (Sound_GetNumberOfPlayingSequencesForPlayer(FirstEffectHandle + i) == 1)
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x020057FC
    public static void Sound_PanEffect(ushort seqID, ushort tracks, int pan)
    {
        int handleType = SoundSystem_GetSoundHandleTypeFromPlayerID(Sound_GetPlayerForSequence(seqID));
        Audio.SetTrackPan(SoundSystem_GetSoundHandle(handleType), tracks, pan);
    }

    // OFFSET 0x02005818
    public static void Sound_PanAllEffects(int pan)
    {
        int first = SoundSystem_GetSoundHandleTypeFromPlayerID(FirstEffectHandle);
        for (int i = 0; i < EffectHandleCount; i++)
        {
            Audio.SetTrackPan(SoundSystem_GetSoundHandle(first + i), 0xFFFF, pan);
        }
    }

    // OFFSET 0x02005844
    public static bool Sound_PlayPokemonCry(ushort species, byte form)
    {
        SoundParam param18 = SoundSystem_GetParam(0x12)!;
        SoundParam chatotCry = SoundSystem_GetParam(0x24)!;
        SoundParam twoCriesAllowed = SoundSystem_GetParam(0x35)!;
        int cry = species;
        if (Sound_Impl_IsShayminSkyForm(species, form) == 1)
        {
            cry = CryShayminSky;
        }
        if (cry != CryShayminSky && (cry > CryLast || cry == 0))
        {
            cry = 1;
        }
        if (cry == CryChatot && SoundChatot.Sound_Impl_PlayChatotCry(chatotCry.Reference, 0, 0x7F, 0))
        {
            SoundChatot.Sound_SetUsingDefaultChatotCry(0);
            return true;
        }
        bool result;
        if ((byte)param18.Value == 0)
        {
            if ((byte)twoCriesAllowed.Value == 0)
            {
                Sound_StopPokemonCries(0);
            }
            result = Audio.StartSequenceEx(SoundSystem_GetSoundHandle(HandleCry), -1, cry, -1, CrySequence);
            Sound_AdjustVolumeForVoiceChatEx(cry, HandleCry);
        }
        else
        {
            result = Audio.StartSequenceEx(SoundSystem_GetSoundHandle(HandleCryEcho), -1, cry, -1, CrySequence);
            Sound_AdjustVolumeForVoiceChatEx(cry, HandleCryEcho);
        }
        SoundChatot.Sound_SetUsingDefaultChatotCry(0);
        return result;
    }

    // OFFSET 0x0200590C
    public static bool Sound_PlayDelayedPokemonCry(ushort species, byte delay, byte form)
    {
        Sound_PlayDelayedPokemonCryEx(0, species, 0, 0x7F, 0xB, delay, form);
        return true;
    }

    // OFFSET 0x0200592C
    public static void Sound_StopPokemonCries(int fadeOutFrames)
    {
        SoundParam primaryAllocated = SoundSystem_GetParam(0x10)!;
        SoundParam secondaryAllocated = SoundSystem_GetParam(0x11)!;
        SoundSystem_GetParam(0xF);
        Audio.StopSequence(SoundSystem_GetSoundHandle(HandleCry), fadeOutFrames);
        Audio.StopSequence(SoundSystem_GetSoundHandle(HandleCryEcho), fadeOutFrames);
        if ((byte)primaryAllocated.Value == 1)
        {
            Sound_StopWaveOutReversed(WaveOutPrimary);
            Sound_FreeWaveOutChannel(WaveOutPrimary);
        }
        if ((byte)secondaryAllocated.Value == 1)
        {
            Sound_StopWaveOutReversed(WaveOutSecondary);
            Sound_FreeWaveOutChannel(WaveOutSecondary);
        }
        SoundChatot.Sound_StopChatotCry();
        Sound_ClearPokemonCryParams();
    }

    // OFFSET 0x0200598C
    public static int Sound_IsPokemonCryPlaying()
    {
        SoundParam primaryAllocated = SoundSystem_GetParam(0x10)!;
        SoundParam secondaryAllocated = SoundSystem_GetParam(0x11)!;
        SoundSystem_GetParam(0xF);
        SoundSystem_GetParam(0x2E);
        if ((byte)primaryAllocated.Value == 1)
        {
            return Sound_IsWaveOutPlaying(WaveOutPrimary) ? 1 : 0;
        }
        if ((byte)secondaryAllocated.Value == 1)
        {
            return Sound_IsWaveOutPlaying(WaveOutSecondary) ? 1 : 0;
        }
        return Sound_GetNumberOfPlayingSequencesForPlayer(0);
    }

    // OFFSET 0x020059D0
    public static bool Sound_PlayPokemonCryEx(int cryMod, ushort species, int pan, int volume, int heapID, byte form)
    {
        SoundParam primaryAllocated = SoundSystem_GetParam(0x10)!;
        SoundParam secondaryAllocated = SoundSystem_GetParam(0x11)!;
        SoundParam param18 = SoundSystem_GetParam(0x12)!;
        SoundParam param30 = SoundSystem_GetParam(0x1E)!;
        SoundSystem_GetParam(0x24);
        ushort cry = species;
        if (Sound_Impl_IsShayminSkyForm(species, form) == 1)
        {
            cry = CryShayminSky;
        }
        if (cry != CryShayminSky && (cry > CryLast || cry == 0))
        {
            cry = 1;
        }
        int wavePan = pan / 2 + 0x40;
        int echoVolume = volume - 0x1E;
        if (echoVolume <= 0)
        {
            echoVolume = 1;
        }
        param18.Value = 0;
        if ((byte)primaryAllocated.Value == 1)
        {
            Sound_StopWaveOutReversed(WaveOutPrimary);
            Sound_FreeWaveOutChannel(WaveOutPrimary);
        }
        if ((byte)secondaryAllocated.Value == 1)
        {
            Sound_StopWaveOutReversed(WaveOutSecondary);
            Sound_FreeWaveOutChannel(WaveOutSecondary);
        }
        if (cry == CryChatot)
        {
            switch (cryMod)
            {
                case 0:
                case 1:
                case 2:
                case 5:
                case 11:
                case 12:
                    Sound_PlayPokemonCry(cry, form);
                    if ((byte)param30.Value != 0 && (byte)primaryAllocated.Value == 1)
                    {
                        Sound_SetWaveOutPan(WaveOutPrimary, (byte)wavePan);
                        Sound_SetWaveOutVolume(WaveOutPrimary, volume);
                    }
                    else
                    {
                        Sound_SetPanForHandle(HandleCry, 0xFFFF, pan);
                        Sound_Impl_SetPokemonCryVolume(cry, HandleCry, volume);
                    }
                    return true;
                default:
                    SoundChatot.Sound_SetUsingDefaultChatotCry(1);
                    break;
            }
        }
        switch (cryMod)
        {
            case 0:
                PlayBasicCry(cry, form, pan, volume);
                break;
            case 1:
                PlayBasicCry(cry, form, pan, volume);
                Sound_SetPokemonCryDuration(0x14, heapID);
                break;
            case 2:
                PlayBasicCry(cry, form, pan, volume);
                Sound_SetPitchForHandle(HandleCry, 0xFFFF, 0x40);
                Sound_Impl_PlayPokemonCryEcho(cry, 0x14, form);
                Sound_SetPanForHandle(HandleCryEcho, 0xFFFF, pan);
                Sound_Impl_SetPokemonCryVolume(cry, HandleCryEcho, echoVolume);
                break;
            case 3:
                PlayBasicCry(cry, form, pan, volume);
                Sound_SetPokemonCryDuration(0x1E, heapID);
                Sound_SetPitchForHandle(HandleCry, 0xFFFF, 0xC0);
                Sound_Impl_PlayPokemonCryEcho(cry, 0x10, form);
                Sound_SetPanForHandle(HandleCryEcho, 0xFFFF, pan);
                Sound_Impl_SetPokemonCryVolume(cry, HandleCryEcho, echoVolume);
                break;
            case 4:
                Sound_AllocateWaveOutChannel(WaveOutPrimary);
                Sound_PlayWaveOutReversed(cry, volume, wavePan, WaveOutPrimary, heapID);
                Sound_SetWaveOutPan(WaveOutPrimary, (byte)wavePan);
                Sound_SetPokemonCryDuration(0xF, heapID);
                Sound_SetWaveOutSpeed(WaveOutPrimary, 0x8600);
                Sound_PlayPokemonCryReversedEcho(cry, -0x40, echoVolume, wavePan, heapID);
                Sound_SetWaveOutSpeed(WaveOutSecondary, 0x8600);
                break;
            case 5:
                PlayBasicCry(cry, form, pan, volume);
                Sound_SetPitchForHandle(HandleCry, 0xFFFF, -224);
                break;
            case 6:
                PlayBasicCry(cry, form, pan, volume);
                Sound_SetPitchForHandle(HandleCry, 0xFFFF, 0x2C);
                Sound_Impl_PlayPokemonCryEcho(cry, -0x40, form);
                Sound_SetPanForHandle(HandleCryEcho, 0xFFFF, pan);
                Sound_Impl_SetPokemonCryVolume(cry, HandleCryEcho, echoVolume);
                break;
            case 7:
                PlayBasicCry(cry, form, pan, volume);
                Sound_SetPokemonCryDuration(0xB, heapID);
                Sound_SetPitchForHandle(HandleCry, 0xFFFF, -128);
                break;
            case 8:
                PlayBasicCry(cry, form, pan, volume);
                Sound_SetPokemonCryDuration(0x3C, heapID);
                Sound_SetPitchForHandle(HandleCry, 0xFFFF, 0x3C);
                break;
            case 9:
                Sound_AllocateWaveOutChannel(WaveOutPrimary);
                Sound_PlayWaveOutReversed(cry, volume, wavePan, WaveOutPrimary, heapID);
                Sound_SetWaveOutPan(WaveOutPrimary, (byte)wavePan);
                Sound_SetPokemonCryDuration(0xD, heapID);
                Sound_SetWaveOutSpeed(WaveOutPrimary, 0x6800);
                break;
            case 10:
                PlayBasicCry(cry, form, pan, volume);
                Sound_SetPokemonCryDuration(0x64, heapID);
                Sound_SetPitchForHandle(HandleCry, 0xFFFF, -44);
                break;
            case 11:
                PlayBasicCry(cry, form, pan, volume);
                Sound_SetPitchForHandle(HandleCry, 0xFFFF, -96);
                break;
            case 12:
                PlayBasicCry(cry, form, pan, volume);
                Sound_SetPokemonCryDuration(0x14, heapID);
                Sound_SetPitchForHandle(HandleCry, 0xFFFF, -96);
                break;
            case 13:
                Sound_PlayPokemonCry(cry, form);
                Sound_Impl_SetPokemonCryVolume(cry, HandleCry, 0x7F);
                Sound_Impl_PlayPokemonCryEcho(cry, 0x14, form);
                Sound_SetPanForHandle(HandleCryEcho, 0xFFFF, pan);
                Sound_FadeVolumeForHandle(HandleCryEcho, volume, 0);
                break;
            case 14:
                Sound_PlayPokemonCry(cry, form);
                break;
        }
        return true;
    }

    private static void PlayBasicCry(ushort cry, byte form, int pan, int volume)
    {
        Sound_PlayPokemonCry(cry, form);
        Sound_SetPanForHandle(HandleCry, 0xFFFF, pan);
        Sound_Impl_SetPokemonCryVolume(cry, HandleCry, volume);
    }

    // OFFSET 0x02005E4C
    private static void Sound_Impl_SetPokemonCryVolume(ushort species, int handleType, int volume)
    {
        Sound_SetInitialVolumeForHandle(handleType, volume);
        Sound_AdjustVolumeForVoiceChatEx(species, handleType);
    }

    // OFFSET 0x02005E64
    public static void Sound_SetPokemonCryDuration(int duration, int heapID)
    {
        SoundParam taskParam = SoundSystem_GetParam(0x23)!;
        Sound_Impl_DestroyCryDurationTask();
        CryDurationData data = new() { Duration = duration };
        data.Task = SysTask.SysTask_Start(Sound_Impl_CryDurationTask, data, 0);
        taskParam.Reference = data.Task;
    }

    // OFFSET 0x02005EB0
    private static void Sound_Impl_CryDurationTask(SysTask task, object? arg)
    {
        CryDurationData data = (CryDurationData)arg!;
        SoundParam primaryAllocated = SoundSystem_GetParam(0x10)!;
        SoundParam secondaryAllocated = SoundSystem_GetParam(0x11)!;
        if (data.Duration == 10)
        {
            Sound_FadeVolumeForHandle(HandleCry, 0, data.Duration);
            Sound_FadeVolumeForHandle(HandleCryEcho, 0, data.Duration);
        }
        data.Duration--;
        if (Sound_IsPokemonCryPlaying() == 0)
        {
            data.Duration = 0;
        }
        if (data.Duration > 0)
        {
            return;
        }
        Sound_StopPokemonCries(0);
        if ((byte)primaryAllocated.Value == 1)
        {
            Sound_StopWaveOutReversed(WaveOutPrimary);
            Sound_FreeWaveOutChannel(WaveOutPrimary);
        }
        if ((byte)secondaryAllocated.Value == 1)
        {
            Sound_StopWaveOutReversed(WaveOutSecondary);
            Sound_FreeWaveOutChannel(WaveOutSecondary);
        }
        Sound_Impl_DestroyCryDurationTask();
    }

    // OFFSET 0x02005F24
    public static void Sound_Impl_DestroyCryDurationTask()
    {
        SoundParam taskParam = SoundSystem_GetParam(0x23)!;
        if (taskParam.Reference != null)
        {
            SysTaskManager.SysTask_GetParam((SysTask)taskParam.Reference);
            SysTask.SysTask_Done((SysTask)taskParam.Reference);
        }
        taskParam.Reference = null;
    }

    // OFFSET 0x02005F4C
    public static void Sound_PlayDelayedPokemonCryEx(int cryMod, ushort species, int pan, int volume, int heapID, byte delay, byte form)
    {
        SoundParam slot = SoundSystem_GetParam(6)!;
        SoundParam twoCriesAllowed = SoundSystem_GetParam(0x35)!;
        int first = (byte)slot.Value == 0 ? 0x29 : 0x2F;
        SoundParam savedMod = SoundSystem_GetParam(first)!;
        SoundParam savedSpecies = SoundSystem_GetParam(first + 4)!;
        SoundParam savedPan = SoundSystem_GetParam(first + 1)!;
        SoundParam savedVolume = SoundSystem_GetParam(first + 2)!;
        SoundParam savedHeap = SoundSystem_GetParam(first + 3)!;
        SoundParam savedDelay = SoundSystem_GetParam(first + 5)!;
        if ((byte)twoCriesAllowed.Value == 1)
        {
            slot.Value = (byte)(slot.Value ^ 1);
        }
        if (Sound_Impl_IsShayminSkyForm(species, form) == 1)
        {
            species = CryShayminSky;
        }
        if (species == 0)
        {
            return;
        }
        if (delay == 0)
        {
            Sound_PlayPokemonCryEx(cryMod, species, pan, volume, heapID, form);
            return;
        }
        savedMod.Value = cryMod;
        savedSpecies.Value = species;
        savedPan.Value = pan;
        savedVolume.Value = volume;
        savedHeap.Value = heapID;
        savedDelay.Value = delay;
    }

    // OFFSET 0x02006038
    private static int Sound_Impl_IsShayminSkyForm(ushort species, byte form)
    {
        if (species == SpeciesShaymin && form == 1)
        {
            return 1;
        }
        if (species == CryShayminSky)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0200605C
    public static void Sound_ClearPokemonCryParams()
    {
        for (int param = 0x29; param <= 0x34; param++)
        {
            SoundSystem_GetParam(param)!.Value = 0;
        }
    }

    // OFFSET 0x020060EC
    private static bool Sound_Impl_PlayPokemonCryEcho(ushort species, int pitch, byte form)
    {
        SoundSystem_GetParam(0x12)!.Value = 1;
        SoundChatot.Sound_SetUsingDefaultChatotCry(1);
        bool result = Sound_PlayPokemonCry(species, form);
        Sound_SetPitchForHandle(HandleCryEcho, 0xFFFF, pitch);
        return result;
    }

    // OFFSET 0x02006120
    private static bool Sound_PlayPokemonCryReversedEcho(ushort species, int unused, int volume, int pan, int heapID)
    {
        SoundSystem_GetParam(0x12)!.Value = 1;
        Sound_AllocateWaveOutChannel(WaveOutSecondary);
        return Sound_PlayWaveOutReversed(species, volume, pan, WaveOutSecondary, heapID);
    }

    // OFFSET 0x02006150
    public static bool Sound_PlayFanfare(ushort seqID)
    {
        Sound_GetBankInfoForSequence(seqID);
        Sound_Impl_SetFanfareDelay(seqID);
        byte player = Sound_GetPlayerForSequence(Sound_Impl_GetCurrentBGM());
        if (player != NoPlayer)
        {
            Sound_SetBGMPlayerPaused(player, 1);
        }
        SoundSystem_SaveHeapState(SoundSystem_GetParam(0x1D));
        SoundSystem_LoadSequenceEx(seqID, 3);
        bool result = Audio.StartSequence(SoundSystem_GetSoundHandle(HandleFanfare), -1, seqID);
        Sound_AdjustVolumeForVoiceChatEx(seqID, HandleFanfare);
        return result;
    }

    // OFFSET 0x0200619C
    public static int Sound_UpdateFanfareDelay()
    {
        SoundParam delay = SoundSystem_GetParam(0xE)!;
        if (Sound_GetNumberOfPlayingSequencesForPlayer(2) != 0)
        {
            return 1;
        }
        if ((ushort)delay.Value != 0)
        {
            delay.Value = (ushort)(delay.Value - 1);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020061C8
    private static void Sound_Impl_StopFanfare(int frames)
    {
        Audio.StopSequence(SoundSystem_GetSoundHandle(HandleFanfare), frames);
        SoundSystem_LoadHeapState(Sound_GetHeapState(6));
    }

    // OFFSET 0x020061E4
    public static int Sound_IsBGMPausedByFanfare()
    {
        SoundSystem_GetParam(0xE);
        if (Sound_UpdateFanfareDelay() == 1)
        {
            return 1;
        }
        Sound_Impl_StopFanfare(0);
        byte player = Sound_GetPlayerForSequence(Sound_Impl_GetCurrentBGM());
        if (player != NoPlayer)
        {
            Sound_SetBGMPlayerPaused(player, 0);
        }
        return 0;
    }

    // OFFSET 0x02006214
    private static void Sound_Impl_SetFanfareDelay(ushort unused)
    {
        SoundSystem_GetParam(0xE)!.Value = 0xF;
    }
}

public sealed class CryDurationData
{
    public int Duration;
    public SysTask? Task;
}
