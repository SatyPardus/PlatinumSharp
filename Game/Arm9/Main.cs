using static PlatinumSharp.Game.Globals;
using static PlatinumSharp.Game.Font;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class Main
{
    private const int NoOverlay = -1;
    private const int SoftResetKeys = 0x30C;

    // OFFSET 0x02000C88
    public static void NitroMain()
    {
        GameSystem.InitSystem();
        GameSystem.InitVRAM();
        GameSystem.InitKeypadAndTouchpad();
        GameSystem.SetGBACartridgeVersion(0);
        PowerManager.GetBackLight(out sApplication.SavedBacklight, out _);
        G3SwapRequest.G3_InitSwapRequest();
        Rtc.InitRTC();
        InitApplication();
        Fonts_Init();
        Font_InitManager(0, 3);
        Font_InitManager(1, 3);
        Font_InitManager(3, 3);
        sApplication.Args.Unused00 = -1;
        sApplication.Args.SaveData = SaveData.SaveData_Init();
        object chatotCry = ChatotCry.SaveData_GetChatotCry(sApplication.Args.SaveData);
        SoundSystem.SoundSystem_Init(chatotCry, PlayerSave.SaveData_GetOptions(sApplication.Args.SaveData));
        Timer.Timer_Start();
        if (WiFiUserData.WiFi_InitLibrary(3) == 3)
        {
            sub_02039A64(3, 0);
        }
        if (SaveData.SaveData_BackupExists(sApplication.Args.SaveData) == 0)
        {
            sub_0209A74C(0);
        }
        else
        {
            switch (Platform.BootType)
            {
                case 0:
                    sApplication.Args.BootMode = 0;
                    EnqueueApplication(0x4D, ApplicationTemplate.At(0x021D788C));
                    break;
                case 1:
                    ScreenFadeControl.SetScreenColorBrightness(0, 0);
                    ScreenFadeControl.SetScreenColorBrightness(1, 0);
                    sApplication.Args.BootMode = 1;
                    EnqueueApplication(0x39, ApplicationTemplate.At(0x021D0F70));
                    break;
                default:
                    ErrorHandling.GF_AssertFail();
                    break;
            }
        }
        gSystem.ShowTitleScreenIntro = 1;
        gSystem.VBlankIntrCounter = 0;
        InitRNG();
        BrightnessController.BrightnessController_ResetAllControllers();
        PlayTimeManager.PlayTime_FlagNotStarted();
        sApplication.CardPulledOut = 0;
        while (true)
        {
            CheckHeapCanary();
            HandleConsoleFold();
            GameSystem.ReadKeypadAndTouchpad();
            if ((gSystem.HeldKeysRaw & SoftResetKeys) == SoftResetKeys && gSystem.InhibitReset == 0)
            {
                SoftReset(0);
            }
            if (CommSystem.CommSys_Update() != 0)
            {
                CheckHeapCanary();
                RunApplication();
                SysTaskManager.SysTaskManager_ExecuteTasks(gSystem.MainTaskManager!);
                SysTaskManager.SysTaskManager_ExecuteTasks(gSystem.PrintTaskManager!);
                if (gSystem.VBlankIntrCounter == 0)
                {
                    Interrupts.Wait(true, Interrupts.VBlank);
                    gSystem.FrameCounter++;
                }
            }
            Rtc.UpdateRTC();
            PlayTimeManager.PlayTime_IncrementTimer();
            G3SwapRequest.G3_ProcessSwapRequest();
            SysTaskManager.SysTaskManager_ExecuteTasks(gSystem.PrintTaskManager!);
            Interrupts.Wait(true, Interrupts.VBlank);
            gSystem.FrameCounter++;
            gSystem.VBlankIntrCounter = 0;
            BrightnessController.BrightnessController_Update();
            ScreenFadeControl.ExecScreenFade();
            gSystem.VBlankCallback?.Invoke(gSystem.VBlankCallbackData);
            SoundSystem.SoundSystem_Tick();
            SysTaskManager.SysTaskManager_ExecuteTasks(gSystem.PostVBlankTaskManager!);
        }
    }

    // OFFSET 0x02000E3C
    private static void InitApplication()
    {
        sApplication.CurrentOverlay = NoOverlay;
        sApplication.CurrentManager = null;
        sApplication.NextOverlay = NoOverlay;
        sApplication.NextTemplate = null;
    }

    // OFFSET 0x02000E54
    private static void RunApplication()
    {
        if (sApplication.CurrentManager == null)
        {
            if (sApplication.NextTemplate == null)
            {
                return;
            }
            if (sApplication.NextOverlay != NoOverlay)
            {
                GameOverlay.Overlay_LoadByID(sApplication.NextOverlay, GameOverlay.LoadNormal);
            }
            sApplication.CurrentOverlay = sApplication.NextOverlay;
            sApplication.CurrentManager = ApplicationManager.ApplicationManager_New(sApplication.NextTemplate, sApplication.Args, 0);
            sApplication.NextOverlay = NoOverlay;
            sApplication.NextTemplate = null;
        }
        if (ApplicationManager.ApplicationManager_Exec(sApplication.CurrentManager))
        {
            ApplicationManager.ApplicationManager_Free(sApplication.CurrentManager);
            sApplication.CurrentManager = null;
            if (sApplication.CurrentOverlay != NoOverlay)
            {
                GameOverlay.Overlay_UnloadByID(sApplication.CurrentOverlay);
            }
        }
    }

    // OFFSET 0x02000EC4
    public static void EnqueueApplication(int overlayID, ApplicationTemplate template)
    {
        if (sApplication.NextTemplate != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sApplication.NextOverlay = overlayID;
        sApplication.NextTemplate = template;
    }

    // OFFSET 0x02000EE4
    private static void WaitFrame()
    {
        CommSystem.CommSys_Update();
        Interrupts.Wait(true, Interrupts.VBlank);
        gSystem.FrameCounter++;
        gSystem.VBlankIntrCounter = 0;
        gSystem.VBlankCallback?.Invoke(gSystem.VBlankCallbackData);
    }

    // OFFSET 0x02000F10
    private static void TrySystemReset(int resetParam)
    {
        if (sub_02038AB8() != 0 && sub_020CD050() == 1)
        {
            sub_020C3EE4(resetParam);
        }
        WaitFrame();
    }

    // OFFSET 0x02000F30
    private static void CheckHeapCanary()
    {
        int state = CommManager.CommMan_GetErrorState();
        switch (state)
        {
            case 1:
                HeapCanaryFailed(1, state);
                break;
            case 2:
                HeapCanaryFailed(0, state);
                break;
            case 3:
                HeapCanaryFailed(1, state);
                break;
        }
    }

    // OFFSET 0x02000F60
    private static void SoftReset(int resetParam)
    {
        ScreenFadeControl.SetScreenColorBrightness(0, 0x7FFF);
        ScreenFadeControl.SetScreenColorBrightness(1, 0x7FFF);
        if (CommManager.CommMan_Suspend() != 0)
        {
            SaveData.SaveData_SaveStateCancel(SaveData.SaveData_Ptr());
        }
        while (true)
        {
            HandleConsoleFold();
            TrySystemReset(resetParam);
        }
    }

    // OFFSET 0x02000F94
    private static void HeapCanaryFailed(int resetParam, int param1)
    {
        if (param1 == 3)
        {
            sub_02039834(0, 3, 0);
        }
        else if (resetParam == 0)
        {
            if (CommManager.CommMan_IsWiFi() == 1)
            {
                sub_02039834(0, 6, 0);
            }
            else
            {
                sub_02039834(0, 2, 0);
            }
        }
        else
        {
            if (CommManager.CommMan_IsWiFi() == 1)
            {
                sub_02039834(0, 5, 0);
            }
            else
            {
                sub_02039834(0, 0, 0);
            }
        }
        CommManager.CommMan_Suspend();
        WaitFrame();
        SoundSystem.SoundSystem_Tick();
        int elapsed = 0;
        while (true)
        {
            HandleConsoleFold();
            GameSystem.ReadKeypadAndTouchpad();
            if (elapsed >= 30 && (gSystem.PressedKeys & 1) != 0)
            {
                break;
            }
            WaitFrame();
            if (elapsed < 30)
            {
                elapsed++;
            }
        }
        SoftReset(resetParam);
    }

    // OFFSET 0x02001028
    public static void InitRNG()
    {
        Rtc.GetCurrentDateTime(out RtcDate date, out RtcTime time);
        uint seed = (uint)(((time.Minute + time.Second) << 24)
            + ((time.Hour << 16) + (date.Year + (((date.Month << 8) * date.Day) << 16))));
        seed += (uint)gSystem.FrameCounter;
        MathUtil.MTRNG_SetSeed(seed);
        MathUtil.LCRNG_SetSeed(seed);
    }

    // OFFSET 0x0200106C
    public static void HandleConsoleFold()
    {
        if (!Platform.IsFolded)
        {
            PowerManager.GetBackLight(out int top, out _);
            if (top == 0)
            {
                PowerManager.SetBackLight(2, sApplication.SavedBacklight);
            }
            return;
        }
        if (gSystem.InhibitSleep != 0)
        {
            PowerManager.GetBackLight(out int top, out _);
            if (top == 1)
            {
                PowerManager.SetBackLight(2, 0);
            }
            return;
        }
        TouchPad.BeforeSleep();
        if (sub_020D09B8() == 1)
        {
            sApplication.CardPulledOut = 1;
        }
        int cardPulledOut = sApplication.CardPulledOut;
        while (true)
        {
            int trigger = 0xC;
            if (gSystem.GbaCartridgeVersion != 0 && cardPulledOut == 0)
            {
                trigger = 0xC | 0x10;
            }
            PowerManager.GoSleepMode(trigger, 0, 0);
            if (sub_020CD9CC() != 0)
            {
                PowerManager.ForceToPowerOff();
                break;
            }
            if (!Platform.IsFolded)
            {
                break;
            }
            sApplication.CardPulledOut = 1;
            cardPulledOut = 1;
        }
        TouchPad.AfterSleep();
    }
}
