namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const int SoundSceneField = 4;
    private const int SoundSceneSub63 = 0x3F;
    private const int SoundSceneSub22 = 0x16;
    private const int SeqSub22 = 0x4C2;
    private const int BgmMaxVolume = 0x7F;

    private static void RegisterSound(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x020482FC] = ScrCmd_PlayMusic;
        ported[0x0204830C] = ScrCmd_StopMusic;
        ported[0x02048320] = ScrCmd_PlayDefaultMusic;
        ported[0x02048338] = ScrCmd_SetMusicOverride;
        ported[0x02048350] = ScrCmd_FadeOutMusic;
        ported[0x0204838C] = ScrCmd_FadeInMusic;
        ported[0x020483B0] = ScrCmd_SetMusicPlayerPaused;
        ported[0x020483CC] = ScrCmd_SetFieldMusic;
        ported[0x020483E0] = ScrCmd_SetMusicFixed;
        ported[0x020483F4] = ScrCmd_PlaySE;
        ported[0x02048410] = ScrCmd_StopSE;
        ported[0x0204842C] = ScrCmd_WaitSE;
        ported[0x0204846C] = ScrCmd_PlayCry;
        ported[0x020484A0] = ScrCmd_WaitCry;
        ported[0x020484C4] = ScrCmd_PlayFanfare;
        ported[0x020484D4] = ScrCmd_WaitFanfare;
        ported[0x020484F8] = ScrCmd_CheckChatotCryPlayable;
        ported[0x02048530] = ScrCmd_StartChatotCryRecording;
        ported[0x0204855C] = ScrCmd_StopChatotCryRecording;
        ported[0x02048568] = ScrCmd_StoreChatotCry;
        ported[0x0204857C] = ScrCmd_SetSoundScene63;
        ported[0x0204858C] = ScrCmd_SetInitialVolumeForSequence;
        ported[0x020485C0] = ScrCmd_SetSoundScene22;
        ported[0x020485D4] = ScrCmd_SetFieldSceneAndPlayMusic;
        ported[0x020485F4] = ScrCmd_GetCurrentMusic;
    }

    // OFFSET 0x020482FC
    private static bool ScrCmd_PlayMusic(ScriptContext ctx)
    {
        SoundPlayback.Sound_PlayBGM(ScriptContext.ScriptContext_ReadHalfWord(ctx));
        return false;
    }

    // OFFSET 0x0204830C
    private static bool ScrCmd_StopMusic(ScriptContext ctx)
    {
        ScriptContext.ScriptContext_ReadHalfWord(ctx);
        SoundPlayback.Sound_StopBGM(Sound.Sound_Impl_GetCurrentBGM(), 0);
        return false;
    }

    // OFFSET 0x02048320
    private static bool ScrCmd_PlayDefaultMusic(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        SoundPlayback.Sound_PlayBGM(FieldBgm.FieldBGM_GetForMapHeader(fieldSystem, (int)fieldSystem.Location!.U32(LocationMapIdOffset)));
        return false;
    }

    // OFFSET 0x02048338
    private static bool ScrCmd_SetMusicOverride(ScriptContext ctx)
    {
        ushort bgmID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        FieldBgm.FieldBGM_SetOverride(ctx.FieldSystem, bgmID);
        return false;
    }

    // OFFSET 0x02048350
    private static bool ScrCmd_FadeOutMusic(ScriptContext ctx)
    {
        ushort targetVolume = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort frames = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        SoundPlayback.Sound_FadeOutBGM(targetVolume, frames);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMusicFade);
        return true;
    }

    // OFFSET 0x02048378
    private static bool ScriptContext_WaitForMusicFade(ScriptContext ctx)
    {
        if (SoundPlayback.Sound_IsFadeActive() == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0204838C
    private static bool ScrCmd_FadeInMusic(ScriptContext ctx)
    {
        ushort frames = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        SoundPlayback.Sound_FadeInBGM(BgmMaxVolume, frames, 0);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMusicFade);
        return true;
    }

    // OFFSET 0x020483B0
    private static bool ScrCmd_SetMusicPlayerPaused(ScriptContext ctx)
    {
        byte playerID = ScriptContext.ScriptContext_ReadByte(ctx);
        byte paused = ScriptContext.ScriptContext_ReadByte(ctx);
        Sound.Sound_SetBGMPlayerPaused(playerID, paused);
        return false;
    }

    // OFFSET 0x020483CC
    private static bool ScrCmd_SetFieldMusic(ScriptContext ctx)
    {
        SoundPlayback.Sound_SetBGM(SoundSceneField, ScriptContext.ScriptContext_ReadHalfWord(ctx));
        return false;
    }

    // OFFSET 0x020483E0
    private static bool ScrCmd_SetMusicFixed(ScriptContext ctx)
    {
        Sound.Sound_SetBGMFixed(ScriptContext.ScriptContext_ReadByte(ctx));
        return false;
    }

    // OFFSET 0x020483F4
    private static bool ScrCmd_PlaySE(ScriptContext ctx)
    {
        SoundPlayback.Sound_PlayEffect(VarValue(ctx));
        return false;
    }

    // OFFSET 0x02048410
    private static bool ScrCmd_StopSE(ScriptContext ctx)
    {
        SoundPlayback.Sound_StopEffect(VarValue(ctx), 0);
        return false;
    }

    // OFFSET 0x0204842C
    private static bool ScrCmd_WaitSE(ScriptContext ctx)
    {
        ctx.Data[0] = VarValue(ctx);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForSE);
        return true;
    }

    // OFFSET 0x02048454
    private static bool ScriptContext_WaitForSE(ScriptContext ctx)
    {
        if (SoundPlayback.Sound_IsEffectPlaying((ushort)ctx.Data[0]) == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0204846C
    private static bool ScrCmd_PlayCry(ScriptContext ctx)
    {
        ushort species = VarValue(ctx);
        VarValue(ctx);
        SoundPlayback.Sound_PlayPokemonCry(species, 0);
        return false;
    }

    // OFFSET 0x020484A0
    private static bool ScrCmd_WaitCry(ScriptContext ctx)
    {
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForCry);
        return true;
    }

    // OFFSET 0x020484B0
    private static bool ScriptContext_WaitForCry(ScriptContext ctx)
    {
        if (SoundPlayback.Sound_IsPokemonCryPlaying() == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020484C4
    private static bool ScrCmd_PlayFanfare(ScriptContext ctx)
    {
        SoundPlayback.Sound_PlayFanfare(ScriptContext.ScriptContext_ReadHalfWord(ctx));
        return false;
    }

    // OFFSET 0x020484D4
    private static bool ScrCmd_WaitFanfare(ScriptContext ctx)
    {
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForFanfare);
        return true;
    }

    // OFFSET 0x020484E4
    private static bool ScriptContext_WaitForFanfare(ScriptContext ctx)
    {
        if (SoundPlayback.Sound_IsBGMPausedByFanfare() == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020484F8
    private static bool ScrCmd_CheckChatotCryPlayable(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        if (SoundChatot.Sound_IsRecordedChatotCryPlayable(ChatotCry.SaveData_GetChatotCry(ctx.FieldSystem.SaveData!)))
        {
            destination.W16(0, 1);
            return false;
        }
        destination.W16(0, 0);
        return false;
    }

    // OFFSET 0x02048530
    private static bool ScrCmd_StartChatotCryRecording(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        if (SoundChatot.Sound_StartRecordingChatotCry() == 0)
        {
            destination.W16(0, 1);
            return false;
        }
        destination.W16(0, 0);
        return false;
    }

    // OFFSET 0x0204855C
    private static bool ScrCmd_StopChatotCryRecording(ScriptContext ctx)
    {
        SoundChatot.Sound_StopRecordingChatotCry();
        return true;
    }

    // OFFSET 0x02048568
    private static bool ScrCmd_StoreChatotCry(ScriptContext ctx)
    {
        SoundChatot.Sound_StoreRecordedChatotCry(ChatotCry.SaveData_GetChatotCry(ctx.FieldSystem.SaveData!));
        return true;
    }

    // OFFSET 0x0204857C
    private static bool ScrCmd_SetSoundScene63(ScriptContext ctx)
    {
        Sound.Sound_SetSceneAndPlayBGM(SoundSceneSub63, 0, 0);
        return true;
    }

    // OFFSET 0x0204858C
    private static bool ScrCmd_SetInitialVolumeForSequence(ScriptContext ctx)
    {
        ushort seqID = VarValue(ctx);
        ushort volume = VarValue(ctx);
        Sound.Sound_SetInitialVolumeForSequence(seqID, volume);
        return false;
    }

    // OFFSET 0x020485C0
    private static bool ScrCmd_SetSoundScene22(ScriptContext ctx)
    {
        Sound.Sound_SetSceneAndPlayBGM(SoundSceneSub22, SeqSub22, 1);
        return true;
    }

    // OFFSET 0x020485D4
    private static bool ScrCmd_SetFieldSceneAndPlayMusic(ScriptContext ctx)
    {
        ushort bgmID = VarValue(ctx);
        Sound.Sound_SetSceneAndPlayBGM(SoundSceneField, bgmID, 1);
        return true;
    }

    // OFFSET 0x020485F4
    private static bool ScrCmd_GetCurrentMusic(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, Sound.Sound_Impl_GetCurrentBGM());
        return false;
    }
}
