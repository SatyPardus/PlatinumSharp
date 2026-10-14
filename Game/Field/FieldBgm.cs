using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class FieldBgm
{
    public const int FadeModeShort = 0;
    public const int FadeModeLong = 1;
    public const int FadeModeLongWithWait = 2;
    public const int FadeModeLongAlternate = 3;

    private const int SceneNone = 0;
    private const int SceneField = 4;
    private const int PlayerStateCycling = 1;
    private const int PlayerStateSurfing = 2;
    private const int SeqSurfing = 0x47F;
    private const int SeqPokeRadar = 0x47E;
    private const int SeqBicycle = 0x4A5;
    private const int SeqEyesMeetDefault = 0x44D;
    private const int MapDistortionWorldFirst = 0x23D;
    private const int MapDistortionWorldRangeCount = 11;
    private const int MapInRangeOutsideDistortionWorld = 0x242;
    private const int MapRoute206 = 0x15E;
    private const int MapRoute206CyclingRoadSouthGate = 0x15F;
    private const int MapRoute206CyclingRoadNorthGate = 0x50;
    private const int CyclingRoadMinX = 0x12B;
    private const int CyclingRoadMaxX = 0x132;
    private const int CyclingRoadNorthZ = 0x240;
    private const int CyclingRoadSouthZ = 0x2A9;
    private const int LocationMapHeaderID = 0x00;
    private const int TrainerParamClass = 1;
    private const uint EyesMeetTableAddress = 0x020EC3E0;
    private const int EyesMeetTableCount = 0x4F;
    private const int MapChangeFadeOutFrames = 40;
    private const int CyclingFadeInFrames = 30;

    // OFFSET 0x020553DC
    public static void FieldBGM_Stop()
    {
        SoundPlayback.Sound_StopWaveOutAndSequences();
        Sound.Sound_ClearBGMPauseFlags();
        Sound.Sound_SetScene(SceneNone);
    }

    // OFFSET 0x020553F0
    public static void FieldBGM_SetOverride(FieldSystem fieldSystem, ushort bgmID)
    {
        BufferView specialBgm = FieldPlayerState.FieldPlayerState_GetSpecialBGM(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        specialBgm.W16(0, bgmID);
    }

    // OFFSET 0x02055404
    public static ushort FieldBGM_GetOverride(FieldSystem fieldSystem)
    {
        BufferView specialBgm = FieldPlayerState.FieldPlayerState_GetSpecialBGM(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        return specialBgm.U16(0);
    }

    // OFFSET 0x02055414
    public static void FieldBGM_ClearOverride(FieldSystem fieldSystem)
    {
        BufferView specialBgm = FieldPlayerState.FieldPlayerState_GetSpecialBGM(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        specialBgm.W16(0, 0);
    }

    // OFFSET 0x02055428
    public static ushort FieldBGM_GetEffective(FieldSystem fieldSystem, int mapHeaderID)
    {
        if (PlayerAvatar.PlayerAvatar_GetPlayerState(fieldSystem.PlayerAvatar) == PlayerStateSurfing)
        {
            if ((uint)(mapHeaderID - MapDistortionWorldFirst) >= MapDistortionWorldRangeCount || mapHeaderID == MapInRangeOutsideDistortionWorld)
            {
                return SeqSurfing;
            }
        }
        if (RadarChain.RadarChain_GetActive(fieldSystem.RadarChain) == 1)
        {
            return SeqPokeRadar;
        }
        ushort bgmID = FieldBGM_GetForMapHeader(fieldSystem, mapHeaderID);
        if (FieldBGM_GetOverride(fieldSystem) != 0)
        {
            bgmID = FieldBGM_GetOverride(fieldSystem);
        }
        return bgmID;
    }

    // OFFSET 0x020554A4
    public static ushort FieldBGM_GetForMapHeader(FieldSystem fieldSystem, int mapHeaderID)
    {
        ushort bgmID;
        if (!Rtc.IsNight())
        {
            bgmID = (ushort)MapHeader.MapHeader_GetDayMusicID(mapHeaderID);
        }
        else
        {
            bgmID = (ushort)MapHeader.MapHeader_GetNightMusicID(mapHeaderID);
        }
        ushort alternate = SystemFlags.SystemFlag_GetAltMusicForHeader(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), mapHeaderID);
        if (alternate != 0)
        {
            bgmID = alternate;
        }
        alternate = FieldBGM_GetAltMusicForCyclingRoad(fieldSystem, mapHeaderID);
        if (alternate != 0)
        {
            bgmID = alternate;
        }
        return bgmID;
    }

    // OFFSET 0x020554E8
    private static ushort FieldBGM_GetAltMusicForCyclingRoad(FieldSystem fieldSystem, int mapHeaderID)
    {
        BufferView previousLocation = FieldPlayerState.FieldPlayerState_GetPreviousLocation(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        int x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar);
        int z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar);
        if (mapHeaderID != MapRoute206)
        {
            return 0;
        }
        int previousMapHeaderID = (int)previousLocation.U32(LocationMapHeaderID);
        if (previousMapHeaderID == MapRoute206CyclingRoadNorthGate || previousMapHeaderID == MapRoute206CyclingRoadSouthGate)
        {
            if (x < CyclingRoadMinX)
            {
                return 0;
            }
            if (x > CyclingRoadMaxX)
            {
                return 0;
            }
            if (z == CyclingRoadNorthZ || z == CyclingRoadSouthZ)
            {
                return SeqBicycle;
            }
        }
        return 0;
    }

    // OFFSET 0x02055554
    public static bool FieldBGM_TryFadeOut(FieldSystem fieldSystem, int bgmID, int mode)
    {
        int playerState = PlayerAvatar.PlayerAvatar_GetPlayerState(fieldSystem.PlayerAvatar);
        if (Sound.Sound_IsBGMFixed() == 1)
        {
            return false;
        }
        if (bgmID == Sound.Sound_Impl_GetCurrentBGM())
        {
            return false;
        }
        Sound.Sound_ClearBGMPauseFlags();
        int fadeOutFrames = 0;
        int waitFrames = 0;
        FieldBGM_GetFadeOutAndWaitFrames(fieldSystem, mode, ref fadeOutFrames, ref waitFrames);
        if (playerState == PlayerStateCycling)
        {
            Sound.Sound_FadeToBGM(SceneField, (ushort)bgmID, fadeOutFrames, waitFrames, CyclingFadeInFrames, 0, null);
        }
        else
        {
            Sound.Sound_FadeOutAndPlayBGM(SceneField, (ushort)bgmID, fadeOutFrames, waitFrames, 0, null);
        }
        return true;
    }

    // OFFSET 0x020555CC
    private static void FieldBGM_GetFadeOutAndWaitFrames(FieldSystem fieldSystem, int mode, ref int fadeOutFrames, ref int waitFrames)
    {
        switch (mode)
        {
            case FadeModeShort:
                fadeOutFrames = 30;
                waitFrames = 0;
                break;
            case FadeModeLong:
                fadeOutFrames = 60;
                waitFrames = 0;
                break;
            case FadeModeLongWithWait:
                fadeOutFrames = 60;
                waitFrames = 15;
                break;
            case FadeModeLongAlternate:
                fadeOutFrames = 60;
                waitFrames = 0;
                break;
        }
    }

    // OFFSET 0x0205560C
    public static ushort FieldBGM_GetEyesMeetForTrainer(int trainerID)
    {
        byte trainerClass = (byte)TrainerData.Trainer_LoadParam(trainerID, TrainerParamClass);
        ushort[] table = RomData.Arm9U16(EyesMeetTableAddress, EyesMeetTableCount * 2);
        for (ushort i = 0; i < EyesMeetTableCount; i++)
        {
            if (trainerClass == table[i * 2])
            {
                return table[i * 2 + 1];
            }
        }
        return SeqEyesMeetDefault;
    }

    // OFFSET 0x02055644
    public static void FieldBGM_FadeOutForMapHeader(FieldSystem fieldSystem, int mapHeaderID)
    {
        if (Sound.Sound_IsBGMFixed() == 1)
        {
            return;
        }
        ushort currentBgm = Sound.Sound_Impl_GetCurrentBGM();
        if (currentBgm != FieldBGM_GetForMapHeader(fieldSystem, mapHeaderID))
        {
            SoundPlayback.Sound_FadeOutBGM(0, MapChangeFadeOutFrames);
        }
    }

    // OFFSET 0x02055670
    public static void FieldBGM_PlayForMapHeader(FieldSystem fieldSystem, int mapHeaderID)
    {
        if (Sound.Sound_IsBGMFixed() == 1)
        {
            return;
        }
        Sound.Sound_SetScene(SceneNone);
        ushort bgmID = FieldBGM_GetForMapHeader(fieldSystem, mapHeaderID);
        Sound.Sound_SetFieldBGM(bgmID);
        Sound.Sound_SetSceneAndPlayBGM(SceneField, bgmID, 1);
    }

    // OFFSET 0x020556A0
    public static void FieldBGM_PlayEffectiveForMapHeader(FieldSystem fieldSystem, int mapHeaderID)
    {
        ushort effectiveBgm = FieldBGM_GetEffective(fieldSystem, mapHeaderID);
        Sound.Sound_SetFieldBGM(FieldBGM_GetForMapHeader(fieldSystem, mapHeaderID));
        Sound.Sound_SetSceneAndPlayBGM(SceneField, effectiveBgm, 1);
    }
}
