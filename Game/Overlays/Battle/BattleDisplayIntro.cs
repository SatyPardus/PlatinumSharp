namespace PlatinumSharp.Game;

public sealed class BattleIntroCurtain
{
    public BattleSystem? BattleSys;
    public BattleParticleHolder? Holder;
    public BattleParticlePlayer? FirstPlayer;
    public BattleParticlePlayer? SecondPlayer;
    public byte State;
    public byte Counter;
    public byte FadeStep;
}

public static partial class BattleDisplay
{
    private const int HeapIDBattle = 5;
    private const uint IntroColorTable = 0x0226F1D0;
    private const uint IntroParticleTable = 0x0226F31C;
    private const int SeqIntroFirst = 0x84F;
    private const int SeqIntroSecond = 0x850;
    private const int PaletteBufferMainBg = 0;
    private const int PaletteBufferSubBg = 1;
    private const int PaletteBufferMainObj = 2;
    private const ushort FadeMainBg = 1;
    private const ushort FadeMainObj = 4;
    private const ushort FadeSubScreen = 0xA;
    private const uint BattleTypeTagWithPartner = 0x4A;
    private const uint BattleTypeDoubles = 2;
    private const uint BattleTypeTrainer = 1;
    private const int IntroFadeSteps = 0x10;
    private const int SubPaletteColors = 0x100;

    private const int IntroStateStart = 0;
    private const int IntroStateRun = 1;
    private const int IntroStateWaitParticles = 2;
    private const int IntroStateEnd = 3;
    private const int IntroStateAbort = 4;
    private const int IntroStateRestore = 5;

    // OFFSET 0x0225CBB8
    public static void BattleDisplay_StartIntro(BattleSystem battleSys, BattlerData battler)
    {
        BattleIntroCurtain curtain = new();
        curtain.BattleSys = battleSys;
        curtain.State = 0;
        curtain.Counter = 0;
        curtain.FadeStep = 0;
        SysTask.SysTask_Start(BattleDisplay_IntroTask, curtain, 0);
    }

    private static void BattleDisplay_RestoreIntroPalettes(BattleIntroCurtain curtain, PaletteData paletteData)
    {
        BattleSystem battleSys = curtain.BattleSys!;
        Array.Copy(BattleSystems.BattleSystem_GetSavedBgPalettes(battleSys), PaletteData.PaletteData_GetUnfadedBuffer(paletteData, PaletteBufferMainBg)!, 0xE0 / 2);
        ushort[] objPalettes = PaletteData.PaletteData_GetUnfadedBuffer(paletteData, PaletteBufferMainObj)!;
        if (BattleSystems.BattleSystem_GetBattleType(battleSys) == BattleTypeTagWithPartner)
        {
            Array.Copy(BattleSystems.BattleSystem_GetSavedObjPalettes(battleSys), objPalettes, 0xA0 / 2);
        }
        else if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) != 0)
        {
            Array.Copy(BattleSystems.BattleSystem_GetSavedObjPalettes(battleSys), objPalettes, 0xE0 / 2);
        }
        else if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeTrainer) != 0)
        {
            Array.Copy(BattleSystems.BattleSystem_GetSavedObjPalettes(battleSys), objPalettes, 0xA0 / 2);
        }
        else
        {
            Array.Copy(BattleSystems.BattleSystem_GetSavedObjPalettes(battleSys), objPalettes, 0x80 / 2);
        }
    }

    // OFFSET 0x02263014
    private static void BattleDisplay_IntroTask(SysTask task, object? param)
    {
        BattleIntroCurtain curtain = (BattleIntroCurtain)param!;
        BattleSystems.BattleSystem_GetBgConfig(curtain.BattleSys!);
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(curtain.BattleSys!)!;
        int terrain = BattleSystems.BattleSystem_GetTerrain(curtain.BattleSys!);
        int background = BattleSystems.BattleSystem_GetBackground(curtain.BattleSys!);
        ushort color = (ushort)Nitro.Bin.U32(RomData.Overlay(16, (uint)(IntroColorTable + background * 4), 4), 0);
        curtain.Counter++;
        switch (curtain.State)
        {
            case IntroStateStart:
            {
                curtain.Holder = BattleParticlePlayers.BattleParticleHolder_New(HeapIDBattle, null);
                BattleParticlePlayerParams playerParams = new();
                playerParams.Member = RomData.Overlay(16, (uint)(IntroParticleTable + terrain), 1)[0];
                playerParams.CameraProjection = 1;
                curtain.FirstPlayer = BattleParticlePlayers.BattleParticleHolder_AddPlayer(curtain.Holder, playerParams);
                playerParams.Member++;
                playerParams.CameraProjection = 1;
                curtain.SecondPlayer = BattleParticlePlayers.BattleParticleHolder_AddPlayer(curtain.Holder, playerParams);
                BattleParticlePlayers.BattleParticlePlayer_Start(curtain.FirstPlayer!);
                SoundPlayback.Sound_PlayEffect(SeqIntroFirst);
                curtain.State = IntroStateRun;
                goto case IntroStateRun;
            }
            case IntroStateRun:
                if (curtain.Counter == 10)
                {
                    PaletteData.PaletteData_StartFade(paletteData, FadeMainBg, 0xF3FF, 0, 0, IntroFadeSteps, color);
                    PaletteData.PaletteData_StartFade(paletteData, FadeMainObj, 0x3FFF, 0, 0, IntroFadeSteps, color);
                }
                if (curtain.Counter >= 10 && curtain.FadeStep < IntroFadeSteps)
                {
                    curtain.FadeStep++;
                    if (curtain.FadeStep > IntroFadeSteps)
                    {
                        curtain.FadeStep = IntroFadeSteps;
                    }
                    ushort[] faded = PaletteData.PaletteData_GetFadedBuffer(paletteData, PaletteBufferSubBg)!;
                    for (ushort i = 0; i < SubPaletteColors; i++)
                    {
                        int level = ((curtain.FadeStep * 0x1F) << 12) >> 16;
                        faded[i] = (ushort)((level << 10) | (level << 5) | level);
                    }
                }
                if (curtain.Counter == 0x14)
                {
                    BattleParticlePlayers.BattleParticlePlayer_Start(curtain.SecondPlayer!);
                }
                if (curtain.Counter == 0x17)
                {
                    SoundPlayback.Sound_PlayEffect(SeqIntroSecond);
                }
                if (curtain.Counter == 0x1C)
                {
                    BattleDisplay_RestoreIntroPalettes(curtain, paletteData);
                    PaletteData.PaletteData_StartFade(paletteData, FadeMainBg, 0xF3FF, 0, IntroFadeSteps, 0, color);
                    PaletteData.PaletteData_StartFade(paletteData, FadeMainObj, 0x3FFF, 0, IntroFadeSteps, 0, color);
                    PaletteData.PaletteData_StartFade(paletteData, FadeSubScreen, 0xFFFF, 0, IntroFadeSteps, 0, color);
                }
                if (curtain.Counter >= 0x32)
                {
                    PaletteData.PaletteData_StartFade(paletteData, FadeMainBg, 0xC00, 0, IntroFadeSteps, 0, 0);
                    curtain.State++;
                }
                break;
            case IntroStateWaitParticles:
                if (BattleParticlePlayers.BattleParticlePlayer_IsActive(curtain.SecondPlayer!) != 0)
                {
                    break;
                }
                BattleParticlePlayers.BattleParticleHolder_Free(curtain.Holder!);
                curtain.State++;
                break;
            case IntroStateEnd:
                PaletteAnimator.PaletteAnimator_SetMode(BattleSystems.BattleSystem_GetPaletteAnimator(curtain.BattleSys!)!, 0);
                SysTask.SysTask_Done(task);
                break;
            case IntroStateAbort:
                BattleParticlePlayers.BattleParticleHolder_StopAll(curtain.Holder!);
                BattleParticlePlayers.BattleParticleHolder_Free(curtain.Holder!);
                curtain.State++;
                goto case IntroStateRestore;
            case IntroStateRestore:
                BattleDisplay_RestoreIntroPalettes(curtain, paletteData);
                PaletteData.PaletteData_StartFade(paletteData, FadeMainBg, 0xFFFF, 0, 0, 0, 0x7FFF);
                PaletteData.PaletteData_StartFade(paletteData, FadeMainObj, 0x3FFF, 0, 0, 0, 0xFFFF);
                PaletteData.PaletteData_StartFade(paletteData, FadeSubScreen, 0xFFFF, 0, 0, 0, 0x7FFF);
                curtain.State = IntroStateEnd;
                break;
        }
    }
}

public static partial class BattleSystems
{
    // OFFSET 0x0223F204
    public static ushort[] BattleSystem_GetSavedBgPalettes(BattleSystem battleSys)
    {
        return battleSys.SavedBgPalettes;
    }

    // OFFSET 0x0223F210
    public static ushort[] BattleSystem_GetSavedObjPalettes(BattleSystem battleSys)
    {
        return battleSys.SavedObjPalettes;
    }

    // OFFSET 0x0223F4E8
    public static uint BattleSystem_GetSeedDTO(BattleSystem battleSys)
    {
        return battleSys.SeedDTO;
    }

    // OFFSET 0x0223F4F4
    public static void BattleSystem_SetSeedDTO(BattleSystem battleSys, uint seed)
    {
        battleSys.SeedDTO = seed;
    }
}
