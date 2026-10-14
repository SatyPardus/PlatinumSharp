using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void HBlankCallback(object? param);

public sealed class ScreenFadeHBlanks
{
    public readonly object?[] Params = new object?[2];
    public readonly HBlankCallback?[] Callbacks = new HBlankCallback?[2];
    public readonly int[] Enabled = new int[2];

    public void Clear()
    {
        for (int i = 0; i < 2; i++)
        {
            Params[i] = null;
            Callbacks[i] = null;
            Enabled[i] = 0;
        }
    }
}

public sealed class ScreenFade
{
    public int Type;
    public int Duration;
    public int StepsPerFrame;
    public int State;
    public int Screen;
    public object? Data;
    public HardwareWindowSettings? HardwareWindows;
    public ScreenFadeHBlanks? HBlanks;
    public int HeapID;
    public ushort Color;
    public int FadesOut;
    public int UsesBrightness;

    public void Clear()
    {
        Type = 0;
        Duration = 0;
        StepsPerFrame = 0;
        State = 0;
        Screen = 0;
        Data = null;
        HardwareWindows = null;
        HBlanks = null;
        HeapID = 0;
        Color = 0;
        FadesOut = 0;
        UsesBrightness = 0;
    }
}

public sealed class ScreenFadeManager
{
    public int Mode;
    public int MainActive;
    public int SubActive;
    public int MainEnabled;
    public int SubEnabled;
    public readonly ScreenFade Main = new();
    public readonly ScreenFade Sub = new();
    public readonly ScreenFadeHBlanks HBlanks = new();
    public HardwareWindowSettings HardwareWindows = new();
    public ushort Active;
    public byte MainResetPending;
    public byte SubResetPending;
    public ushort SavedColor;
}

public sealed class EnableHBlankRequest
{
    public ScreenFadeHBlanks? HBlanks;
    public object? Param;
    public HBlankCallback? Callback;
    public int Screen;
}

public sealed class DisableHBlankRequest
{
    public ScreenFadeHBlanks? HBlanks;
    public int Screen;
}

public static class ScreenFadeControl
{
    public const int ScreenMain = 0;
    public const int ScreenSub = 1;

    private const ushort ColorSaved = 0xFFFF;
    private const ushort ColorWhite = 0x7FFF;
    private const ushort ColorBlack = 0;
    private const int BrightnessMax = 16;
    private const int FadeStateFinish = 2;
    private const uint HBlankTaskPriority = 0x400;
    private const uint FadeFuncTableAddress = 0x020E5074;
    private const int FadeFuncCount = 42;

    private const int ModeBothTogether = 0;
    private const int ModeMainThenSub = 1;
    private const int ModeSubThenMain = 2;

    private static readonly ScreenFadeManager sScreenFadeManager = new();
    private static uint[]? sFadeFuncs;

    // OFFSET 0x0200F174
    public static void StartScreenFade(int mode, int typeMain, int typeSub, ushort color, int duration, int stepsPerFrame, int heapID)
    {
        if (duration == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (stepsPerFrame == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (sScreenFadeManager.Active != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        ZeroScreenFadeManager(sScreenFadeManager);
        SetupScreenFadeParams(mode, sScreenFadeManager);
        ClearHBlanks(sScreenFadeManager.HBlanks);
        ushort fadeColor = GetFadeColor(sScreenFadeManager, color);
        InitScreenFade(sScreenFadeManager.Main, typeMain, duration, stepsPerFrame, 0, null, ScreenMain, sScreenFadeManager.HardwareWindows, sScreenFadeManager.HBlanks, heapID, fadeColor);
        InitScreenFade(sScreenFadeManager.Sub, typeSub, duration, stepsPerFrame, 0, null, ScreenSub, sScreenFadeManager.HardwareWindows, sScreenFadeManager.HBlanks, heapID, fadeColor);
        sScreenFadeManager.Active = 1;
        TryScreenFadeFunc(ref sScreenFadeManager.MainActive, sScreenFadeManager.Main);
        TryScreenFadeFunc(ref sScreenFadeManager.SubActive, sScreenFadeManager.Sub);
        if (sScreenFadeManager.MainEnabled != 0)
        {
            RequestResetScreenMasterBrightness(sScreenFadeManager.Main);
            sScreenFadeManager.MainResetPending = 1;
        }
        if (sScreenFadeManager.SubEnabled != 0)
        {
            RequestResetScreenMasterBrightness(sScreenFadeManager.Sub);
            sScreenFadeManager.SubResetPending = 1;
        }
    }

    // OFFSET 0x0200F27C
    public static void ExecScreenFade()
    {
        if (sScreenFadeManager.Active != 0)
        {
            if (TryScreenFade(sScreenFadeManager, sScreenFadeManager.Main, sScreenFadeManager.Sub))
            {
                ResetScreenFadeManager(sScreenFadeManager);
            }
        }
    }

    // OFFSET 0x0200F2AC
    public static bool IsScreenFadeDone()
    {
        if (sScreenFadeManager.Active == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0200F2C0
    public static void FinishScreenFade()
    {
        DisableScreenHBlank(sScreenFadeManager.HBlanks, ScreenMain);
        DisableScreenHBlank(sScreenFadeManager.HBlanks, ScreenSub);
        if (sScreenFadeManager.MainActive != 0)
        {
            sScreenFadeManager.Main.State = FadeStateFinish;
        }
        if (sScreenFadeManager.SubActive != 0)
        {
            sScreenFadeManager.Sub.State = FadeStateFinish;
        }
        TryScreenFadeFunc(ref sScreenFadeManager.MainActive, sScreenFadeManager.Main);
        TryScreenFadeFunc(ref sScreenFadeManager.SubActive, sScreenFadeManager.Sub);
        sScreenFadeManager.Active = 0;
        sScreenFadeManager.MainResetPending = 0;
        sScreenFadeManager.SubResetPending = 0;
        ZeroScreenFadeManager(sScreenFadeManager);
    }

    // OFFSET 0x0200F32C
    public static void ResetVisibleHardwareWindows(int screen)
    {
        HardwareWindow.SetVisibleHardwareWindows(0, screen);
    }

    // OFFSET 0x0200F338
    public static void ResetScreenMasterBrightness(int screen)
    {
        SetScreenMasterBrightness(screen, 0);
    }

    // OFFSET 0x0200F344
    public static void SetScreenColorBrightness(int screen, ushort color)
    {
        if (color == ColorSaved)
        {
            color = sScreenFadeManager.SavedColor;
        }
        int brightness;
        if (color == ColorWhite)
        {
            brightness = BrightnessMax;
        }
        else
        {
            brightness = -BrightnessMax;
        }
        SetScreenMasterBrightness(screen, brightness);
    }

    // OFFSET 0x0200F370
    public static void SetColorBrightness(ushort color)
    {
        if (color == ColorSaved)
        {
            color = sScreenFadeManager.SavedColor;
        }
        int brightness;
        if (color == ColorWhite)
        {
            brightness = BrightnessMax;
        }
        else
        {
            brightness = -BrightnessMax;
        }
        SetScreenMasterBrightness(ScreenMain, brightness);
        SetScreenMasterBrightness(ScreenSub, brightness);
        sScreenFadeManager.SavedColor = color;
    }

    // OFFSET 0x0200F3B0
    public static void SetupScreenFadeRegisters(int screen, ushort color)
    {
        if (color == ColorSaved)
        {
            color = sScreenFadeManager.SavedColor;
        }
        if (screen == ScreenMain)
        {
            Video.LoadPaletteRange(Video.MainBackground, new[] { color }, 0, 0, 2);
        }
        else
        {
            Video.LoadPaletteRange(Video.SubBackground, new[] { color }, 0, 0, 2);
        }
        HardwareWindow.RequestVisibleHardwareWindows(sScreenFadeManager.HardwareWindows, 1, screen);
        HardwareWindow.RequestHardwareWindowMaskInsidePlane(sScreenFadeManager.HardwareWindows, 0x3F, 0, 0, screen);
        HardwareWindow.RequestHardwareWindowDimensions(sScreenFadeManager.HardwareWindows, 0, 0, 0, 0, 0, screen);
        HardwareWindow.RequestHardwareWindowMaskOutsidePlane(sScreenFadeManager.HardwareWindows, 0x20, 0, screen);
    }

    // OFFSET 0x0200F42C
    public static void SetScreenBackgroundColor(ushort color)
    {
        Video.LoadPaletteRange(Video.MainBackground, new[] { color }, 0, 0, 2);
        Video.LoadPaletteRange(Video.SubBackground, new[] { color }, 0, 0, 2);
    }

    // OFFSET 0x0200F44C
    public static void SetScreenMasterBrightness(int screen, int brightness)
    {
        if (screen == ScreenMain)
        {
            Video.SetMasterBrightness(ScreenMain, brightness);
        }
        else
        {
            Video.SetMasterBrightness(ScreenSub, brightness);
        }
    }

    // OFFSET 0x0200F46C
    private static void ResetScreenFadeManager(ScreenFadeManager manager)
    {
        manager.Active = 0;
        manager.SavedColor = GetSavedFadeColor(manager);
        if (manager.MainEnabled != 0)
        {
            ResetWindowScreenFade(manager.Main);
            if (manager.Main.FadesOut == 0)
            {
                manager.MainResetPending = 0;
            }
        }
        if (manager.SubEnabled != 0)
        {
            ResetWindowScreenFade(manager.Sub);
            if (manager.Main.FadesOut == 0)
            {
                manager.SubResetPending = 0;
            }
        }
        ZeroScreenFadeManager(manager);
    }

    // OFFSET 0x0200F4C4
    private static bool TryScreenFade(ScreenFadeManager manager, ScreenFade main, ScreenFade sub)
    {
        switch (manager.Mode)
        {
            case ModeBothTogether:
                TryScreenFadeFunc(ref manager.MainActive, main);
                TryScreenFadeFunc(ref manager.SubActive, sub);
                break;
            case ModeMainThenSub:
                if (manager.MainActive != 0)
                {
                    TryScreenFadeFunc(ref manager.MainActive, main);
                }
                else
                {
                    TryScreenFadeFunc(ref manager.SubActive, sub);
                }
                break;
            case ModeSubThenMain:
                if (manager.SubActive != 0)
                {
                    TryScreenFadeFunc(ref manager.SubActive, sub);
                }
                else
                {
                    TryScreenFadeFunc(ref manager.MainActive, main);
                }
                break;
        }
        if (manager.MainActive == 0 && manager.SubActive == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0200F534
    private static void TryScreenFadeFunc(ref int active, ScreenFade fade)
    {
        if (active != 0)
        {
            if (CallScreenFadeFunc(fade))
            {
                active = 0;
            }
        }
    }

    // OFFSET 0x0200F550
    private static bool CallScreenFadeFunc(ScreenFade fade)
    {
        sFadeFuncs ??= RomData.Arm9U32(FadeFuncTableAddress, FadeFuncCount);
        return ScreenFadeFuncs.Dispatch(sFadeFuncs[fade.Type] & ~1u, fade);
    }

    // OFFSET 0x0200F564
    private static void SetupScreenFadeParams(int mode, ScreenFadeManager manager)
    {
        switch (mode)
        {
            case 0:
                InitScreenFadeParams(manager, ModeBothTogether, 1, 1);
                break;
            case 1:
                InitScreenFadeParams(manager, ModeMainThenSub, 1, 1);
                break;
            case 2:
                InitScreenFadeParams(manager, ModeSubThenMain, 1, 1);
                break;
            case 3:
                InitScreenFadeParams(manager, ModeMainThenSub, 1, 0);
                break;
            case 4:
                InitScreenFadeParams(manager, ModeSubThenMain, 0, 1);
                break;
        }
    }

    // OFFSET 0x0200F5C8
    private static void InitScreenFadeParams(ScreenFadeManager manager, int mode, int main, int sub)
    {
        manager.Mode = mode;
        manager.MainActive = main;
        manager.SubActive = sub;
        manager.MainEnabled = main;
        manager.SubEnabled = sub;
    }

    // OFFSET 0x0200F5D4
    private static void InitScreenFade(ScreenFade fade, int type, int duration, int stepsPerFrame, int state, object? data, int screen, HardwareWindowSettings hardwareWindows, ScreenFadeHBlanks hblanks, int heapID, ushort color)
    {
        fade.Type = type;
        fade.Duration = duration;
        fade.StepsPerFrame = stepsPerFrame;
        fade.State = state;
        fade.Data = data;
        fade.Screen = screen;
        fade.HardwareWindows = hardwareWindows;
        fade.HBlanks = hblanks;
        fade.HeapID = heapID;
        fade.Color = color;
    }

    // OFFSET 0x0200F600
    private static void ClearHBlanks(ScreenFadeHBlanks hblanks)
    {
        for (int i = 0; i < 2; i++)
        {
            hblanks.Params[i] = null;
            hblanks.Callbacks[i] = DummyHBlankCallback;
            hblanks.Enabled[i] = 0;
        }
    }

    // OFFSET 0x0200F61C
    private static void RunHBlankCallbacks(object? param)
    {
        ScreenFadeHBlanks hblanks = (ScreenFadeHBlanks)param!;
        for (int i = 0; i < 2; i++)
        {
            hblanks.Callbacks[i]!(hblanks.Params[i]);
        }
    }

    // OFFSET 0x0200F634
    private static void EnableScreenHBlank(ScreenFadeHBlanks hblanks, object? param, HBlankCallback? callback, int screen)
    {
        bool registered = true;
        if (hblanks.Enabled[screen] != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (hblanks.Callbacks[screen] == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (hblanks.Enabled[0] == 0 && hblanks.Enabled[1] == 0)
        {
            registered = GameSystem.SetHBlankCallback(RunHBlankCallbacks, hblanks);
        }
        if (!registered)
        {
            ErrorHandling.GF_AssertFail();
        }
        hblanks.Params[screen] = param;
        if (callback != null)
        {
            hblanks.Callbacks[screen] = callback;
        }
        else
        {
            hblanks.Callbacks[screen] = DummyHBlankCallback;
        }
        hblanks.Enabled[screen] = 1;
    }

    // OFFSET 0x0200F6AC
    private static void DisableScreenHBlank(ScreenFadeHBlanks hblanks, int screen)
    {
        hblanks.Enabled[screen] = 0;
        if (hblanks.Enabled[0] == 0 && hblanks.Enabled[1] == 0)
        {
            GameSystem.DisableHBlank();
        }
        hblanks.Callbacks[screen] = DummyHBlankCallback;
        hblanks.Params[screen] = null;
    }

    // OFFSET 0x0200F6D8
    public static void RequestEnableScreenHBlank(ScreenFadeHBlanks hblanks, object? param, HBlankCallback? callback, int screen, int heapID)
    {
        EnableHBlankRequest request = new()
        {
            HBlanks = hblanks,
            Param = param,
            Callback = callback,
            Screen = screen,
        };
        SysTask.SysTask_ExecuteAfterVBlank(Task_EnableScreenHBlank, request, HBlankTaskPriority);
    }

    // OFFSET 0x0200F704
    public static void RequestDisableScreenHBlank(ScreenFadeHBlanks hblanks, int screen, int heapID)
    {
        DisableHBlankRequest request = new()
        {
            HBlanks = hblanks,
            Screen = screen,
        };
        SysTask.SysTask_ExecuteAfterVBlank(Task_DisableScreenHBlank, request, HBlankTaskPriority);
    }

    // OFFSET 0x0200F728
    private static void Task_EnableScreenHBlank(SysTask task, object? param)
    {
        EnableHBlankRequest request = (EnableHBlankRequest)param!;
        EnableScreenHBlank(request.HBlanks!, request.Param, request.Callback, request.Screen);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x0200F748
    private static void Task_DisableScreenHBlank(SysTask task, object? param)
    {
        DisableHBlankRequest request = (DisableHBlankRequest)param!;
        DisableScreenHBlank(request.HBlanks!, request.Screen);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x0200F764
    private static void DummyHBlankCallback(object? param)
    {
    }

    // OFFSET 0x0200F768
    private static ushort GetFadeColor(ScreenFadeManager manager, ushort color)
    {
        if (color == ColorSaved)
        {
            color = manager.SavedColor;
        }
        return color;
    }

    // OFFSET 0x0200F77C
    private static ushort GetSavedFadeColor(ScreenFadeManager manager)
    {
        ScreenFade fade;
        if (manager.MainEnabled == 1)
        {
            fade = manager.Main;
        }
        else
        {
            fade = manager.Sub;
        }
        if (fade.FadesOut == 1)
        {
            return fade.Color;
        }
        return manager.SavedColor;
    }

    // OFFSET 0x0200F7A0
    private static void Task_ResetScreenMasterBrightness(SysTask task, object? param)
    {
        ScreenFade fade = (ScreenFade)param!;
        SetScreenMasterBrightness(fade.Screen, 0);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x0200F7B4
    private static void RequestResetScreenMasterBrightness(ScreenFade fade)
    {
        if (fade.FadesOut == 0 && (fade.Color == ColorWhite || fade.Color == ColorBlack) && fade.UsesBrightness == 0)
        {
            SysTask.SysTask_ExecuteAfterVBlank(Task_ResetScreenMasterBrightness, fade, HBlankTaskPriority);
        }
    }

    // OFFSET 0x0200F7E4
    private static void ResetWindowScreenFade(ScreenFade fade)
    {
        if (fade.FadesOut == 1 && (fade.Color == ColorWhite || fade.Color == ColorBlack) && fade.UsesBrightness == 0)
        {
            SetScreenColorBrightness(fade.Screen, fade.Color);
            ResetVisibleHardwareWindows(fade.Screen);
        }
    }

    // OFFSET 0x0200F814
    private static void ZeroScreenFadeManager(ScreenFadeManager manager)
    {
        manager.Mode = 0;
        manager.MainActive = 0;
        manager.SubActive = 0;
        manager.MainEnabled = 0;
        manager.SubEnabled = 0;
        manager.Main.Clear();
        manager.Sub.Clear();
        manager.HBlanks.Clear();
        manager.HardwareWindows = new HardwareWindowSettings();
    }
}
