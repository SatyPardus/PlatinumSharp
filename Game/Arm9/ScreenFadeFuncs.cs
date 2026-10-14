using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class BrightnessFadeData
{
    public int Duration;
    public int StepsPerFrame;
    public int Counter;
    public int Current;
    public int Target;
    public int Step;
    public int Screen;
}

public sealed class FadeLineBuffer
{
    public const int Lines = 0xC0;
    public const int Size = Lines * 2;
    public const int Right = Lines;

    public readonly short[] Current = new short[Size];
    public readonly short[] Pending = new short[Size];
    public int Index;
}

public class FadeLineBuffers
{
    public FadeLineBuffer[]? Buffers;
    public int Count;
    public int Screen;
}

public sealed class CircleFadeData : FadeLineBuffers
{
    public int Radius;
    public int CenterX;
    public int CenterY;
    public int Step;
    public int Duration;
    public int StepsPerFrame;
    public int Counter;
    public int HeapID;
    public int Deferred;
    public HardwareWindowSettings? Settings;
    public ScreenFadeHBlanks? HBlanks;
}

public sealed class FadeWindowHalf
{
    public const int Size = 0xC0;

    public readonly byte[] Pending = new byte[Size];
    public readonly byte[] Current = new byte[Size];
    public object? Source;
}

public class FadeWindowData
{
    public readonly FadeWindowHalf[] Halves = { new FadeWindowHalf(), new FadeWindowHalf() };
    public byte Type;
    public byte Screen;
}

public sealed class WedgeFadeData : FadeLineBuffers
{
    public int Angle;
    public int Step;
    public int Duration;
    public int StepsPerFrame;
    public int Counter;
    public int Deferred;
    public HardwareWindowSettings? Settings;
    public ScreenFadeHBlanks? HBlanks;
    public int HeapID;
}

public sealed class HexFadeData : FadeLineBuffers
{
    public int Scale;
    public int Angle;
    public int Step;
    public int Duration;
    public int StepsPerFrame;
    public int Counter;
    public int Deferred;
    public HardwareWindowSettings? Settings;
    public ScreenFadeHBlanks? HBlanks;
    public int HeapID;
}

public sealed class BandFadeTemplate
{
    public byte[] From = Array.Empty<byte>();
    public byte[] To = Array.Empty<byte>();
    public ushort Count;
    public ushort Window;
    public byte Inside;
    public byte Outside;
    public ushort Deferred;
}

public sealed class FadeBand
{
    public readonly int[] Current = new int[4];
    public readonly int[] Step = new int[4];
    public readonly int[] Target = new int[4];
}

public sealed class BandFadeData : FadeLineBuffers
{
    public FadeBand[]? Bands;
    public int BandCount;
    public int Duration;
    public int StepsPerFrame;
    public int Counter;
    public int Deferred;
    public HardwareWindowSettings? Settings;
    public ScreenFadeHBlanks? HBlanks;
    public int HeapID;
}

public sealed class FanFadeData : FadeLineBuffers
{
    public int Angle;
    public int AngleStart;
    public int AngleDelta;
    public int Duration;
    public int StepIndex;
    public int StepsPerFrame;
    public int Counter;
    public int Deferred;
    public int HeapID;
    public HardwareWindowSettings? Settings;
    public ScreenFadeHBlanks? HBlanks;
}

public sealed class PatternFadeTemplate
{
    public byte[] Table = Array.Empty<byte>();
    public ushort Count;
    public ushort Mode;
}

public sealed class PatternFadeData : FadeWindowData
{
    public byte[] Table = Array.Empty<byte>();
    public int Count;
    public int Duration;
    public int StepIndex;
    public int StepsPerFrame;
    public int Counter;
    public int Mode;
    public int HeapID;
    public HardwareWindowSettings? Settings;
    public ScreenFadeHBlanks? HBlanks;
}

public sealed class WindowFadeData
{
    public readonly int[] Current = new int[4];
    public readonly int[] Step = new int[4];
    public readonly int[] Target = new int[4];
    public int Screen;
    public int Window;
    public int Duration;
    public int StepsPerFrame;
    public int Counter;
    public int Deferred;
    public HardwareWindowSettings? Settings;
}

public sealed class CombinedFadeTemplate
{
    public byte[] Window = Array.Empty<byte>();
    public PatternFadeTemplate Pattern = new();
    public int Ratio;
}

public sealed class CombinedFadeData
{
    public readonly WindowFadeData Window = new();
    public readonly PatternFadeData Pattern = new();
    public CombinedFadeTemplate? Template;
    public byte SplitDuration;
    public byte Phase;
    public byte Order;
}

public static class ScreenFadeFuncs
{
    private const ushort ColorWhite = 0x7FFF;
    private const ushort ColorBlack = 0;
    private const int BrightnessMax = 16;
    private const int FractionShift = 7;
    private const int WindowTemplateSize = 12;
    private const int WedgeTemplateSize = 8;
    private const int WedgeWindow = 4;
    private const int WedgeInside = 5;
    private const int WedgeOutside = 6;
    private const int WedgeDeferred = 7;
    private const int ScreenCenterX = 0x80;
    private const int QuarterTurn = 0x3FFF;
    private const int FanInside = 4;
    private const int FanOutside = 5;
    private const int FanDeferred = 6;
    private const int MaxHalfWidth = 0x7F;
    private const int ScreenRight = 0xFF;
    private const int BandCount = 4;
    private const int HexScale = 0x80000;
    private const int HalfLines = FadeLineBuffer.Lines / 2;
    private const int TemplateWindow = 8;
    private const int TemplateInside = 9;
    private const int TemplateOutside = 10;
    private const int TemplateDeferred = 11;

    public static bool Dispatch(uint address, ScreenFade fade)
    {
        switch (address)
        {
            case 0x0200F85C:
                return ScreenFade_BrightnessOut(fade);
            case 0x0200F878:
                return ScreenFade_BrightnessIn(fade);
            case 0x0200F980:
                return ScreenFade_WindowOut1(fade);
            case 0x0200F9AC:
                return ScreenFade_WindowIn1(fade);
            case 0x0200FAC0:
                return ScreenFade_WindowOut2(fade);
            case 0x0200FAEC:
                return ScreenFade_WindowIn2(fade);
            case 0x0200FB18:
                return ScreenFade_DoubleWindowOut(fade);
            case 0x0200FB4C:
                return ScreenFade_DoubleWindowIn(fade);
            case 0x0200FC84:
                return ScreenFade_WindowOut3(fade);
            case 0x0200FCB0:
                return ScreenFade_WindowIn3(fade);
            case 0x0200FCDC:
                return ScreenFade_WindowOut4(fade);
            case 0x0200FD08:
                return ScreenFade_WindowIn4(fade);
            case 0x0200FB7C:
                return ScreenFade_CircleOut1(fade);
            case 0x0200FBA8:
                return ScreenFade_CircleIn1(fade);
            case 0x0200FBD4:
                return ScreenFade_CircleOut2(fade);
            case 0x0200FC00:
                return ScreenFade_CircleIn2(fade);
            case 0x0200FC2C:
                return ScreenFade_WedgeOut(fade);
            case 0x0200FC58:
                return ScreenFade_WedgeIn(fade);
            case 0x0200FD34:
                return ScreenFade_HexOut(fade);
            case 0x0200FD60:
                return ScreenFade_HexIn(fade);
            case 0x0200FD8C:
                return ScreenFade_BandOut(fade);
            case 0x0200FDE0:
                return ScreenFade_BandIn(fade);
            case 0x0200FEA4:
                return ScreenFade_FanOut(fade);
            case 0x0200FEEC:
                return ScreenFade_FanIn(fade);
            case 0x0200FFBC:
                return ScreenFade_CircleOut3(fade);
            case 0x0200FFE8:
                return ScreenFade_CircleIn3(fade);
            case 0x02010014:
                return ScreenFade_WindowOut5(fade);
            case 0x02010040:
                return ScreenFade_WindowIn5(fade);
            case 0x0200FF30:
                return ScreenFade_CrossOut(fade);
            case 0x0200FF78:
                return ScreenFade_CrossIn(fade);
            case 0x0200F898:
                return ScreenFade_PatternOut1(fade);
            case 0x0200F8D4:
                return ScreenFade_PatternIn1(fade);
            case 0x0200F90C:
                return ScreenFade_PatternOut2(fade);
            case 0x0200F948:
                return ScreenFade_PatternIn2(fade);
            case 0x0200F9D8:
                return ScreenFade_PatternOut3(fade);
            case 0x0200FA14:
                return ScreenFade_PatternIn3(fade);
            case 0x0200FA4C:
                return ScreenFade_PatternOut4(fade);
            case 0x0200FA88:
                return ScreenFade_PatternIn4(fade);
            case 0x0200FE30:
                return ScreenFade_PatternOut5(fade);
            case 0x0200FE6C:
                return ScreenFade_PatternIn5(fade);
            case 0x0201006C:
                return ScreenFade_CombinedOut(fade);
            case 0x020100A8:
                return ScreenFade_CombinedIn(fade);
            default:
                return ScreenFadeFunc(address, fade);
        }
    }

    // OFFSET 0x0200F85C
    public static bool ScreenFade_BrightnessOut(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            fade.FadesOut = 1;
            fade.UsesBrightness = 1;
            BrightnessFade_Init(fade, 1);
            return false;
        }
        return BrightnessFade_Update(fade);
    }

    // OFFSET 0x0200F878
    public static bool ScreenFade_BrightnessIn(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            fade.FadesOut = 0;
            fade.UsesBrightness = 1;
            BrightnessFade_Init(fade, 0);
            return false;
        }
        return BrightnessFade_Update(fade);
    }

    // OFFSET 0x020100E0
    private static int CalcTangent(ushort angle)
    {
        int index = angle >> 4;
        return Fx32.Div(Render3D.Sin(index), Render3D.Cos(index));
    }

    // OFFSET 0x020100FC
    private static int CalcSlopeOffset(ushort angle, int x)
    {
        return Fx32.Mul(CalcTangent(angle), x << Fx32.Shift) >> Fx32.Shift;
    }

    // OFFSET 0x02010124
    private static void FillSlopeOffsets(ushort angle, int[] offsets, int end, int start)
    {
        int tangent = CalcTangent(angle);
        for (int i = start; i < end; i++)
        {
            offsets[i] = Fx32.Mul(tangent, i << Fx32.Shift) >> Fx32.Shift;
        }
    }

    // OFFSET 0x02010178
    private static int CalcSlopeHeight(ushort angle, int width)
    {
        int tangent = CalcTangent(angle);
        return Fx32.Div((width / 2) << Fx32.Shift, tangent);
    }

    // OFFSET 0x02010190
    private static int CalcStep(int start, int end, int steps)
    {
        return ((end - start) << FractionShift) / steps;
    }

    // OFFSET 0x020101A0
    private static int AddClampToByte(int value, int delta)
    {
        int result = value + delta;
        if (result < 0)
        {
            result = 0;
        }
        if (result > 0xFF)
        {
            result = 0xFF;
        }
        return result;
    }

    // OFFSET 0x020101B0
    private static void AddRectStep(int[] values, int[] deltas)
    {
        values[0] += deltas[0];
        values[1] += deltas[1];
        values[2] += deltas[2];
        values[3] += deltas[3];
    }

    // OFFSET 0x020101D4
    private static void InitRectInterpolation(int[] current, int[] target, int[] step, byte[] from, byte[] to, int steps)
    {
        for (int i = 0; i < 4; i++)
        {
            current[i] = from[i] << FractionShift;
        }
        for (int i = 0; i < 4; i++)
        {
            target[i] = to[i];
        }
        for (int i = 0; i < 4; i++)
        {
            step[i] = CalcStep(from[i], to[i], steps);
        }
    }

    // OFFSET 0x02010238
    private static void BrightnessFade_Init(ScreenFade fade, int fadeOut)
    {
        BrightnessFadeData data = new();
        fade.Data = data;
        int start;
        int end;
        if (fadeOut == 0)
        {
            if (fade.Color == ColorWhite)
            {
                start = BrightnessMax;
                end = 0;
            }
            else if (fade.Color == ColorBlack)
            {
                start = -BrightnessMax;
                end = 0;
            }
            else
            {
                start = -BrightnessMax;
                end = 0;
                ErrorHandling.GF_AssertFail();
            }
        }
        else if (fade.Color == ColorWhite)
        {
            start = 0;
            end = BrightnessMax;
        }
        else if (fade.Color == ColorBlack)
        {
            start = 0;
            end = -BrightnessMax;
        }
        else
        {
            start = 0;
            end = -BrightnessMax;
            ErrorHandling.GF_AssertFail();
        }
        ScreenFadeControl.SetScreenMasterBrightness(fade.Screen, start);
        data.Duration = fade.Duration;
        data.StepsPerFrame = fade.StepsPerFrame;
        data.Counter = 0;
        data.Current = start << FractionShift;
        data.Target = end << FractionShift;
        data.Step = CalcStep(start, end, fade.Duration);
        data.Screen = fade.Screen;
        fade.State++;
    }

    // OFFSET 0x020102D8
    private static bool BrightnessFade_Update(ScreenFade fade)
    {
        bool done = false;
        switch (fade.State)
        {
            case 1:
                if (BrightnessFade_Step((BrightnessFadeData)fade.Data!))
                {
                    fade.State++;
                }
                break;
            case 2:
                fade.Data = null;
                fade.State++;
                done = true;
                break;
            case 3:
                done = true;
                break;
        }
        return done;
    }

    // OFFSET 0x02010318
    private static bool BrightnessFade_Step(BrightnessFadeData data)
    {
        bool done = false;
        data.Counter++;
        if (data.Counter < data.StepsPerFrame)
        {
            return done;
        }
        data.Counter = 0;
        int remaining = data.Duration - 1;
        if (remaining > 0)
        {
            data.Duration = remaining;
            data.Current += data.Step;
        }
        else
        {
            data.Current = data.Target;
            done = true;
        }
        ScreenFadeControl.SetScreenMasterBrightness(data.Screen, data.Current / (1 << FractionShift));
        return done;
    }

    // OFFSET 0x02010588
    private static void FadeLineBuffers_Init(FadeLineBuffers buffers, int mode, int screen, int heapID)
    {
        if (mode == 0 || mode == 1)
        {
            buffers.Buffers = new[] { new FadeLineBuffer() };
            buffers.Count = 1;
            buffers.Screen = screen;
            buffers.Buffers[0].Index = mode;
        }
        else if (mode == 2)
        {
            buffers.Buffers = new[] { new FadeLineBuffer(), new FadeLineBuffer() };
            buffers.Count = 2;
            buffers.Screen = screen;
            for (int i = 0; i < 2; i++)
            {
                buffers.Buffers[i].Index = i;
            }
        }
    }

    // OFFSET 0x020105EC
    private static void FadeLineBuffers_Free(FadeLineBuffers buffers)
    {
        FadeLineBuffers_FreeBuffers(buffers);
    }

    // OFFSET 0x020105F4
    private static void FadeLineBuffers_FreeBuffers(FadeLineBuffers buffers)
    {
        buffers.Buffers = null;
    }

    // OFFSET 0x02010604
    private static FadeLineBuffer FadeLineBuffers_Get(FadeLineBuffers buffers, int index)
    {
        if (buffers.Count <= index)
        {
            ErrorHandling.GF_AssertFail();
        }
        return buffers.Buffers![index];
    }

    // OFFSET 0x02010624
    private static void Task_SwapLineBuffers(SysTask task, object? param)
    {
        FadeLineBuffers buffers = (FadeLineBuffers)param!;
        for (int i = 0; i < buffers.Count; i++)
        {
            FadeLineBuffer buffer = FadeLineBuffers_Get(buffers, i);
            Array.Copy(buffer.Pending, buffer.Current, FadeLineBuffer.Size);
        }
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x02010658
    private static void FinishWindowFade(int enable, HardwareWindowSettings settings, int screen)
    {
        if (enable == 0)
        {
            HardwareWindow.RequestVisibleHardwareWindows(settings, 0, screen);
            return;
        }
        HardwareWindow.RequestVisibleHardwareWindows(settings, 1, screen);
        HardwareWindow.RequestHardwareWindowMaskInsidePlane(settings, 0x3F, 0, 0, screen);
        HardwareWindow.RequestHardwareWindowDimensions(settings, 0, 0, 0, 0, 0, screen);
        HardwareWindow.RequestHardwareWindowMaskOutsidePlane(settings, 0x20, 0, screen);
    }

    // OFFSET 0x020106A8
    private static void SetupFadeWindow(HardwareWindowSettings settings, int inside, int outside, int window, int screen, int x1, int x2, int y1, int y2, int deferred)
    {
        if (deferred == 0)
        {
            HardwareWindow.SetHardwareWindowMaskInsidePlane(inside, 0, window, screen);
            HardwareWindow.SetHardwareWindowMaskOutsidePlane(outside, 0, screen);
            HardwareWindow.SetHardwareWindowDimensions(x1, x2, y1, y2, window, screen);
            return;
        }
        HardwareWindow.RequestHardwareWindowMaskInsidePlane(settings, inside, 0, window, screen);
        HardwareWindow.RequestHardwareWindowMaskOutsidePlane(settings, outside, 0, screen);
        HardwareWindow.RequestHardwareWindowDimensions(settings, x1, x2, y1, y2, window, screen);
    }

    // OFFSET 0x02010710
    private static void SetFadeVisibleWindows(HardwareWindowSettings settings, int mask, int screen, int deferred)
    {
        if (deferred == 0)
        {
            HardwareWindow.SetVisibleHardwareWindows(mask, screen);
        }
        else
        {
            HardwareWindow.RequestVisibleHardwareWindows(settings, mask, screen);
        }
    }

    // OFFSET 0x02010728
    private static void FadeWindowData_Init(FadeWindowData data, byte screen, byte type, object? source0, object? source1)
    {
        for (int i = 0; i < 2; i++)
        {
            Array.Clear(data.Halves[i].Pending);
            Array.Clear(data.Halves[i].Current);
            data.Halves[i].Source = null;
        }
        if (type == 1)
        {
            data.Halves[0].Source = source0;
            data.Type = type;
            data.Screen = screen;
        }
        else
        {
            data.Halves[0].Source = source0;
            data.Halves[1].Source = source1;
            data.Type = type;
            data.Screen = screen;
        }
    }

    // OFFSET 0x0201076C
    private static void RequestSwapWindowFlags(FadeWindowData data)
    {
        SysTask.SysTask_ExecuteAfterVBlank(Task_SwapWindowFlags, data, 0x3FF);
    }

    // OFFSET 0x02010784
    private static void EnableWindowFlagHBlank(ScreenFadeHBlanks hblanks, FadeWindowData data, int heapID)
    {
        ScreenFadeControl.RequestEnableScreenHBlank(hblanks, data, HBlank_ApplyWindowFlags, data.Screen, heapID);
    }

    // OFFSET 0x0201079C
    private static void DisableWindowFlagHBlank(ScreenFadeHBlanks hblanks, FadeWindowData data, int heapID)
    {
        ScreenFadeControl.RequestDisableScreenHBlank(hblanks, data.Screen, heapID);
    }

    // OFFSET 0x020107AC
    private static void Task_SwapWindowFlags(SysTask task, object? param)
    {
        FadeWindowData data = (FadeWindowData)param!;
        for (int i = 0; i < 2; i++)
        {
            Array.Copy(data.Halves[i].Pending, data.Halves[i].Current, FadeWindowHalf.Size);
        }
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x0200F980
    public static bool ScreenFade_WindowOut1(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            WindowFade_Init(fade, RomData.Arm9(0x020E51F0, WindowTemplateSize));
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return WindowFade_Update(fade);
    }

    // OFFSET 0x0200F9AC
    public static bool ScreenFade_WindowIn1(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            WindowFade_Init(fade, RomData.Arm9(0x020E5214, WindowTemplateSize));
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return WindowFade_Update(fade);
    }

    // OFFSET 0x0200FAC0
    public static bool ScreenFade_WindowOut2(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            WindowFade_Init(fade, RomData.Arm9(0x020E5238, WindowTemplateSize));
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return WindowFade_Update(fade);
    }

    // OFFSET 0x0200FAEC
    public static bool ScreenFade_WindowIn2(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            WindowFade_Init(fade, RomData.Arm9(0x020E5244, WindowTemplateSize));
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return WindowFade_Update(fade);
    }

    // OFFSET 0x0200FB18
    public static bool ScreenFade_DoubleWindowOut(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            DoubleWindowFade_Init(fade, RomData.Arm9(0x020E5250, WindowTemplateSize), RomData.Arm9(0x020E525C, WindowTemplateSize));
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return DoubleWindowFade_Update(fade);
    }

    // OFFSET 0x0200FB4C
    public static bool ScreenFade_DoubleWindowIn(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            DoubleWindowFade_Init(fade, RomData.Arm9(0x020E5268, WindowTemplateSize), RomData.Arm9(0x020E5280, WindowTemplateSize));
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return DoubleWindowFade_Update(fade);
    }

    // OFFSET 0x0200FC84
    public static bool ScreenFade_WindowOut3(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            WindowFade_Init(fade, RomData.Arm9(0x020E51D8, WindowTemplateSize));
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return WindowFade_Update(fade);
    }

    // OFFSET 0x0200FCB0
    public static bool ScreenFade_WindowIn3(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            WindowFade_Init(fade, RomData.Arm9(0x020E51E4, WindowTemplateSize));
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return WindowFade_Update(fade);
    }

    // OFFSET 0x0200FCDC
    public static bool ScreenFade_WindowOut4(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            WindowFade_Init(fade, RomData.Arm9(0x020E51FC, WindowTemplateSize));
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return WindowFade_Update(fade);
    }

    // OFFSET 0x0200FD08
    public static bool ScreenFade_WindowIn4(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            WindowFade_Init(fade, RomData.Arm9(0x020E5220, WindowTemplateSize));
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return WindowFade_Update(fade);
    }

    // OFFSET 0x02010CF4
    private static void WindowFade_Init(ScreenFade fade, byte[] template)
    {
        WindowFadeData data = new();
        fade.Data = data;
        WindowFade_Setup(data, template, fade.Duration, fade.StepsPerFrame, fade.Screen, fade.HardwareWindows!);
        if (template[TemplateWindow] == 0)
        {
            SetFadeVisibleWindows(fade.HardwareWindows!, 1, data.Screen, data.Deferred);
        }
        else
        {
            SetFadeVisibleWindows(fade.HardwareWindows!, 2, data.Screen, data.Deferred);
        }
        fade.State++;
    }

    // OFFSET 0x02010D44
    private static bool WindowFade_Update(ScreenFade fade)
    {
        bool done = false;
        switch (fade.State)
        {
            case 1:
                WindowFadeData data = (WindowFadeData)fade.Data!;
                if (WindowFade_Step(data))
                {
                    FinishWindowFade(data.Deferred, fade.HardwareWindows!, fade.Screen);
                    fade.State++;
                }
                break;
            case 2:
                fade.Data = null;
                fade.State++;
                done = true;
                break;
            case 3:
                done = true;
                break;
        }
        return done;
    }

    // OFFSET 0x02010D94
    private static void DoubleWindowFade_Init(ScreenFade fade, byte[] template0, byte[] template1)
    {
        WindowFadeData[] data = { new WindowFadeData(), new WindowFadeData() };
        fade.Data = data;
        WindowFade_Setup(data[0], template0, fade.Duration, fade.StepsPerFrame, fade.Screen, fade.HardwareWindows!);
        WindowFade_Setup(data[1], template1, fade.Duration, fade.StepsPerFrame, fade.Screen, fade.HardwareWindows!);
        SetFadeVisibleWindows(fade.HardwareWindows!, 3, fade.Screen, data[0].Deferred);
        fade.State++;
    }

    // OFFSET 0x02010DEC
    private static bool DoubleWindowFade_Update(ScreenFade fade)
    {
        bool done = false;
        switch (fade.State)
        {
            case 1:
                WindowFadeData[] data = (WindowFadeData[])fade.Data!;
                int finished = WindowFade_Step(data[0]) ? 1 : 0;
                finished += WindowFade_Step(data[1]) ? 1 : 0;
                if (finished == 2)
                {
                    FinishWindowFade(data[0].Deferred, fade.HardwareWindows!, fade.Screen);
                    fade.State++;
                }
                break;
            case 2:
                fade.Data = null;
                fade.State++;
                done = true;
                break;
            case 3:
                done = true;
                break;
        }
        return done;
    }

    // OFFSET 0x02010E48
    private static void WindowFade_Setup(WindowFadeData data, byte[] template, int duration, int stepsPerFrame, int screen, HardwareWindowSettings settings)
    {
        byte[] from = { template[0], template[1], template[2], template[3] };
        byte[] to = { template[4], template[5], template[6], template[7] };
        InitRectInterpolation(data.Current, data.Target, data.Step, from, to, duration);
        data.Screen = screen;
        data.Window = template[TemplateWindow];
        data.Duration = duration;
        data.StepsPerFrame = stepsPerFrame;
        data.Counter = 0;
        data.Settings = settings;
        data.Deferred = template[TemplateDeferred];
        SetupFadeWindow(settings, template[TemplateInside], template[TemplateOutside], template[TemplateWindow], screen, template[0], template[1], template[2], template[3], data.Deferred);
    }

    // OFFSET 0x02010EA4
    private static bool WindowFade_Step(WindowFadeData data)
    {
        const int Unit = 1 << FractionShift;
        data.Counter++;
        if (data.Counter < data.StepsPerFrame)
        {
            return false;
        }
        data.Counter = 0;
        int remaining = data.Duration - 1;
        if (remaining <= 0)
        {
            HardwareWindow.RequestHardwareWindowDimensions(data.Settings!, data.Target[0], data.Target[1], data.Target[2], data.Target[3], data.Window, data.Screen);
            return true;
        }
        data.Duration = remaining;
        AddRectStep(data.Current, data.Step);
        HardwareWindow.RequestHardwareWindowDimensions(data.Settings!, data.Current[0] / Unit, data.Current[1] / Unit, data.Current[2] / Unit, data.Current[3] / Unit, data.Window, data.Screen);
        return false;
    }

    // OFFSET 0x0200FB7C
    public static bool ScreenFade_CircleOut1(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            CircleFade_Init(fade, RomData.Arm9(0x020E528C, WindowTemplateSize));
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return CircleFade_Update(fade);
    }

    // OFFSET 0x0200FBA8
    public static bool ScreenFade_CircleIn1(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            CircleFade_Init(fade, RomData.Arm9(0x020E519C, WindowTemplateSize));
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return CircleFade_Update(fade);
    }

    // OFFSET 0x0200FBD4
    public static bool ScreenFade_CircleOut2(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            CircleFade_Init(fade, RomData.Arm9(0x020E51A8, WindowTemplateSize));
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return CircleFade_Update(fade);
    }

    // OFFSET 0x0200FC00
    public static bool ScreenFade_CircleIn2(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            CircleFade_Init(fade, RomData.Arm9(0x020E51B4, WindowTemplateSize));
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return CircleFade_Update(fade);
    }

    // OFFSET 0x02010F2C
    private static void CircleFade_Init(ScreenFade fade, byte[] template)
    {
        CircleFadeData data = new();
        fade.Data = data;
        CircleFade_Setup(data, template, fade.Duration, fade.StepsPerFrame, fade.Screen, fade.HardwareWindows!, fade.HBlanks!, fade.HeapID);
        fade.State++;
    }

    // OFFSET 0x02010F64
    private static bool CircleFade_Update(ScreenFade fade)
    {
        bool done = false;
        switch (fade.State)
        {
            case 1:
                CircleFadeData data = (CircleFadeData)fade.Data!;
                if (CircleFade_Step(data))
                {
                    FinishWindowFade(data.Deferred, data.Settings!, fade.Screen);
                    fade.State++;
                }
                break;
            case 2:
                FadeLineBuffers_Free((CircleFadeData)fade.Data!);
                fade.Data = null;
                fade.State++;
                done = true;
                break;
            case 3:
                done = true;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return done;
    }

    // OFFSET 0x02010FC0
    private static void CircleFade_Setup(CircleFadeData data, byte[] template, int duration, int stepsPerFrame, int screen, HardwareWindowSettings settings, ScreenFadeHBlanks hblanks, int heapID)
    {
        short radiusStart = (short)(template[0] | (template[1] << 8));
        short radiusEnd = (short)(template[2] | (template[3] << 8));
        short centerX = (short)(template[4] | (template[5] << 8));
        short centerY = (short)(template[6] | (template[7] << 8));
        int step = CalcStep(radiusStart, radiusEnd, duration);
        FadeLineBuffers_Init(data, template[TemplateWindow], screen, heapID);
        data.Radius = radiusStart << FractionShift;
        data.CenterX = centerX;
        data.CenterY = centerY;
        data.Step = step;
        data.Duration = duration;
        data.StepsPerFrame = stepsPerFrame;
        data.Counter = 0;
        data.Settings = settings;
        data.HBlanks = hblanks;
        data.HeapID = heapID;
        data.Deferred = template[TemplateDeferred];
        CircleFade_FillLines(data);
        SysTask.SysTask_ExecuteAfterVBlank(Task_SwapLineBuffers, data, 0x3FF);
        FadeLineBuffer buffer = FadeLineBuffers_Get(data, 0);
        SetupFadeWindow(settings, template[TemplateInside], template[TemplateOutside], template[TemplateWindow], screen, buffer.Pending[0], 0, buffer.Pending[FadeLineBuffer.Right], FadeLineBuffer.Lines, data.Deferred);
        if (template[TemplateWindow] == 0)
        {
            SetFadeVisibleWindows(settings, 1, screen, data.Deferred);
        }
        else
        {
            SetFadeVisibleWindows(settings, 2, screen, data.Deferred);
        }
        ScreenFadeControl.RequestEnableScreenHBlank(data.HBlanks, data, HBlank_ApplyLineWindows, screen, heapID);
    }

    // OFFSET 0x0201109C
    private static bool CircleFade_Step(CircleFadeData data)
    {
        data.Counter++;
        if (data.Counter < data.StepsPerFrame)
        {
            return false;
        }
        data.Counter = 0;
        int remaining = data.Duration - 1;
        if (remaining <= 0)
        {
            ScreenFadeControl.RequestDisableScreenHBlank(data.HBlanks!, data.Screen, data.HeapID);
            return true;
        }
        data.Duration = remaining;
        data.Radius += data.Step;
        CircleFade_FillLines(data);
        SysTask.SysTask_ExecuteAfterVBlank(Task_SwapLineBuffers, data, 0x3FF);
        return false;
    }

    // OFFSET 0x020110EC
    private static void CircleFade_CalcLineEdges(int radius, int centerX, int centerY, int line, out int left, out int right)
    {
        int whole = radius / (1 << FractionShift);
        int distance = line - centerY;
        if (distance < 0)
        {
            distance = -distance;
        }
        if (distance >= whole)
        {
            left = 0;
            right = 0;
            return;
        }
        int distanceFx = distance << Fx32.Shift;
        int radiusFx = whole << Fx32.Shift;
        int half = Fx32.Sqrt(Fx32.Mul(radiusFx, radiusFx) - Fx32.Mul(distanceFx, distanceFx)) >> Fx32.Shift;
        left = centerX - half;
        if (left < 0)
        {
            left = 0;
        }
        right = left + half * 2;
        if (right > 0xFF)
        {
            right = 0xFF;
        }
    }

    // OFFSET 0x02011180
    private static void CircleFade_FillLines(CircleFadeData data)
    {
        FadeLineBuffer buffer = FadeLineBuffers_Get(data, 0);
        for (int line = 0; line < FadeLineBuffer.Lines; line++)
        {
            int left;
            int right;
            if (line <= data.CenterY)
            {
                CircleFade_CalcLineEdges(data.Radius, data.CenterX, data.CenterY, line, out left, out right);
            }
            else if (line <= data.CenterY * 2)
            {
                int mirrored = data.CenterY * 2 - line;
                left = buffer.Pending[mirrored];
                right = buffer.Pending[FadeLineBuffer.Right + mirrored];
            }
            else
            {
                CircleFade_CalcLineEdges(data.Radius, data.CenterX, data.CenterY, line, out left, out right);
            }
            buffer.Pending[line] = (short)left;
            buffer.Pending[FadeLineBuffer.Right + line] = (short)right;
        }
    }

    // OFFSET 0x0200FC2C
    public static bool ScreenFade_WedgeOut(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            WedgeFade_Init(fade, RomData.Arm9(0x020E514C, WedgeTemplateSize));
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return WedgeFade_Update(fade);
    }

    // OFFSET 0x0200FC58
    public static bool ScreenFade_WedgeIn(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            WedgeFade_Init(fade, RomData.Arm9(0x020E5184, WedgeTemplateSize));
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return WedgeFade_Update(fade);
    }

    // OFFSET 0x02011204
    private static void WedgeFade_Init(ScreenFade fade, byte[] template)
    {
        WedgeFadeData data = new();
        fade.Data = data;
        WedgeFade_Setup(data, template, fade.Duration, fade.StepsPerFrame, fade.Screen, fade.HardwareWindows!, fade.HBlanks!, fade.HeapID);
        fade.State++;
    }

    // OFFSET 0x0201123C
    private static bool WedgeFade_Update(ScreenFade fade)
    {
        bool done = false;
        switch (fade.State)
        {
            case 1:
                WedgeFadeData data = (WedgeFadeData)fade.Data!;
                if (WedgeFade_Step(data))
                {
                    FinishWindowFade(data.Deferred, data.Settings!, fade.Screen);
                    fade.State++;
                }
                break;
            case 2:
                FadeLineBuffers_Free((WedgeFadeData)fade.Data!);
                fade.Data = null;
                fade.State++;
                done = true;
                break;
            case 3:
                done = true;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return done;
    }

    // OFFSET 0x02011298
    private static void WedgeFade_Setup(WedgeFadeData data, byte[] template, int duration, int stepsPerFrame, int screen, HardwareWindowSettings settings, ScreenFadeHBlanks hblanks, int heapID)
    {
        ushort angleStart = (ushort)(template[0] | (template[1] << 8));
        ushort angleEnd = (ushort)(template[2] | (template[3] << 8));
        data.Step = CalcStep(angleStart, angleEnd, duration);
        FadeLineBuffers_Init(data, template[WedgeWindow], screen, heapID);
        data.Angle = angleStart << FractionShift;
        data.Duration = duration;
        data.StepsPerFrame = stepsPerFrame;
        data.Counter = 0;
        data.Settings = settings;
        data.HBlanks = hblanks;
        data.HeapID = heapID;
        data.Deferred = template[WedgeDeferred];
        WedgeFade_FillLines(data);
        SysTask.SysTask_ExecuteAfterVBlank(Task_SwapLineBuffers, data, 0x3FF);
        FadeLineBuffer buffer = FadeLineBuffers_Get(data, 0);
        SetupFadeWindow(settings, template[WedgeInside], template[WedgeOutside], template[WedgeWindow], screen, buffer.Pending[0], 0, buffer.Pending[FadeLineBuffer.Right], FadeLineBuffer.Lines, data.Deferred);
        if (template[WedgeWindow] == 0)
        {
            SetFadeVisibleWindows(settings, 1, screen, data.Deferred);
        }
        else
        {
            SetFadeVisibleWindows(settings, 2, screen, data.Deferred);
        }
        ScreenFadeControl.RequestEnableScreenHBlank(data.HBlanks, data, HBlank_ApplyLineWindows, screen, heapID);
    }

    // OFFSET 0x02011360
    private static bool WedgeFade_Step(WedgeFadeData data)
    {
        data.Counter++;
        if (data.Counter < data.StepsPerFrame)
        {
            return false;
        }
        data.Counter = 0;
        int remaining = data.Duration - 1;
        if (remaining <= 0)
        {
            ScreenFadeControl.RequestDisableScreenHBlank(data.HBlanks!, data.Screen, data.HeapID);
            return true;
        }
        data.Duration = remaining;
        data.Angle += data.Step;
        WedgeFade_FillLines(data);
        SysTask.SysTask_ExecuteAfterVBlank(Task_SwapLineBuffers, data, 0x3FF);
        return false;
    }

    // OFFSET 0x020113B0
    private static void WedgeFade_FillLines(WedgeFadeData data)
    {
        FadeLineBuffer buffer = FadeLineBuffers_Get(data, 0);
        int[] offsets = new int[FadeLineBuffer.Lines];
        FillSlopeOffsets((ushort)(data.Angle / (1 << FractionShift)), offsets, FadeLineBuffer.Lines, 0);
        for (int line = 0; line < FadeLineBuffer.Lines; line++)
        {
            buffer.Pending[line] = (short)AddClampToByte(ScreenCenterX, -offsets[line]);
            buffer.Pending[FadeLineBuffer.Right + line] = (short)AddClampToByte(ScreenCenterX, offsets[line]);
        }
    }

    // OFFSET 0x0200FD34
    public static bool ScreenFade_HexOut(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            HexFade_Init(fade, RomData.Arm9(0x020E5174, WedgeTemplateSize));
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return HexFade_Update(fade);
    }

    // OFFSET 0x0200FD60
    public static bool ScreenFade_HexIn(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            HexFade_Init(fade, RomData.Arm9(0x020E516C, WedgeTemplateSize));
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return HexFade_Update(fade);
    }

    // OFFSET 0x02011408
    private static void HexFade_Init(ScreenFade fade, byte[] template)
    {
        HexFadeData data = new();
        fade.Data = data;
        HexFade_Setup(data, template, fade.Duration, fade.StepsPerFrame, fade.Screen, fade.HardwareWindows!, fade.HBlanks!, fade.HeapID);
        fade.State++;
    }

    // OFFSET 0x02011440
    private static bool HexFade_Update(ScreenFade fade)
    {
        bool done = false;
        switch (fade.State)
        {
            case 1:
                HexFadeData data = (HexFadeData)fade.Data!;
                if (HexFade_Step(data))
                {
                    FinishWindowFade(data.Deferred, data.Settings!, fade.Screen);
                    fade.State++;
                }
                break;
            case 2:
                FadeLineBuffers_Free((HexFadeData)fade.Data!);
                fade.Data = null;
                fade.State++;
                done = true;
                break;
            case 3:
                done = true;
                break;
        }
        return done;
    }

    // OFFSET 0x02011494
    private static void HexFade_Setup(HexFadeData data, byte[] template, int duration, int stepsPerFrame, int screen, HardwareWindowSettings settings, ScreenFadeHBlanks hblanks, int heapID)
    {
        ushort angleStart = (ushort)(template[0] | (template[1] << 8));
        ushort angleEnd = (ushort)(template[2] | (template[3] << 8));
        int step = (angleEnd - angleStart) / duration;
        FadeLineBuffers_Init(data, template[WedgeWindow], screen, heapID);
        data.Scale = HexScale;
        data.Angle = angleStart;
        data.Step = step;
        data.Duration = duration;
        data.StepsPerFrame = stepsPerFrame;
        data.Counter = 0;
        data.Settings = settings;
        data.HBlanks = hblanks;
        data.HeapID = heapID;
        data.Deferred = template[WedgeDeferred];
        HexFade_FillLines(data);
        SysTask.SysTask_ExecuteAfterVBlank(Task_SwapLineBuffers, data, 0x3FF);
        FadeLineBuffer buffer = FadeLineBuffers_Get(data, 0);
        SetupFadeWindow(settings, template[WedgeInside], template[WedgeOutside], template[WedgeWindow], screen, buffer.Pending[HalfLines], 0, buffer.Pending[FadeLineBuffer.Right + HalfLines], FadeLineBuffer.Lines, data.Deferred);
        if (template[WedgeWindow] == 0)
        {
            SetFadeVisibleWindows(settings, 1, screen, data.Deferred);
        }
        else
        {
            SetFadeVisibleWindows(settings, 2, screen, data.Deferred);
        }
        ScreenFadeControl.RequestEnableScreenHBlank(data.HBlanks, data, HBlank_ApplyLineWindows, screen, heapID);
    }

    // OFFSET 0x02011568
    private static bool HexFade_Step(HexFadeData data)
    {
        data.Counter++;
        if (data.Counter < data.StepsPerFrame)
        {
            return false;
        }
        data.Counter = 0;
        int remaining = data.Duration - 1;
        if (remaining <= 0)
        {
            ScreenFadeControl.RequestDisableScreenHBlank(data.HBlanks!, data.Screen, data.HeapID);
            return true;
        }
        data.Duration = remaining;
        data.Angle += data.Step;
        HexFade_FillLines(data);
        SysTask.SysTask_ExecuteAfterVBlank(Task_SwapLineBuffers, data, 0x3FF);
        return false;
    }

    // OFFSET 0x020115B8
    private static void HexFade_FillLines(HexFadeData data)
    {
        FadeLineBuffer buffer = FadeLineBuffers_Get(data, 0);
        int halfWidth = Fx32.Mul(Render3D.Sin(data.Angle >> 4), data.Scale) >> Fx32.Shift;
        int slant = 0xB4 - (halfWidth * 2 / 0x15 + 1) * 2;
        int angle = 0xFFFF * slant / 0x168 / 2;
        int height = CalcSlopeHeight((ushort)angle, 0x100) >> Fx32.Shift;
        if (height >= FadeLineBuffer.Lines)
        {
            ErrorHandling.GF_AssertFail();
        }
        int[] offsets = new int[FadeLineBuffer.Lines];
        FillSlopeOffsets((ushort)angle, offsets, height, 0);
        for (int line = 0; line < HalfLines; line++)
        {
            int index = height - (line + 1);
            int width = halfWidth;
            if (index > 0 && offsets[index] > halfWidth)
            {
                width = offsets[index];
            }
            short left = (short)AddClampToByte(ScreenCenterX, -width);
            short right = (short)AddClampToByte(ScreenCenterX, width);
            int mirrored = FadeLineBuffer.Lines - 1 - line;
            buffer.Pending[line] = left;
            buffer.Pending[FadeLineBuffer.Right + line] = right;
            buffer.Pending[mirrored] = left;
            buffer.Pending[FadeLineBuffer.Right + mirrored] = right;
        }
    }

    // OFFSET 0x0200FD8C
    public static bool ScreenFade_BandOut(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            BandFadeTemplate template = new()
            {
                From = RomData.Arm9(0x020E52A4, BandCount * 4),
                To = RomData.Arm9(0x020E52B4, BandCount * 4),
                Count = BandCount,
                Window = 0,
                Inside = 0x3F,
                Outside = 0x20,
                Deferred = 1,
            };
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            BandFade_Init(fade, template);
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return BandFade_Update(fade);
    }

    // OFFSET 0x0200FDE0
    public static bool ScreenFade_BandIn(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            BandFadeTemplate template = new()
            {
                From = RomData.Arm9(0x020E52C4, BandCount * 4),
                To = RomData.Arm9(0x020E52D4, BandCount * 4),
                Count = BandCount,
                Window = 0,
                Inside = 0x3F,
                Outside = 0x20,
                Deferred = 0,
            };
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            BandFade_Init(fade, template);
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return BandFade_Update(fade);
    }

    // OFFSET 0x020116A0
    private static void BandFade_Init(ScreenFade fade, BandFadeTemplate template)
    {
        BandFadeData data = new();
        fade.Data = data;
        BandFade_Setup(data, template, fade.Duration, fade.StepsPerFrame, fade.Screen, fade.HardwareWindows!, fade.HBlanks!, fade.HeapID);
        fade.State++;
    }

    // OFFSET 0x020116D8
    private static bool BandFade_Update(ScreenFade fade)
    {
        bool done = false;
        switch (fade.State)
        {
            case 1:
                BandFadeData data = (BandFadeData)fade.Data!;
                if (BandFade_Step(data))
                {
                    FinishWindowFade(data.Deferred, data.Settings!, fade.Screen);
                    fade.State++;
                }
                break;
            case 2:
                BandFade_FreeBands((BandFadeData)fade.Data!);
                FadeLineBuffers_Free((BandFadeData)fade.Data!);
                fade.Data = null;
                fade.State++;
                done = true;
                break;
            case 3:
                done = true;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return done;
    }

    // OFFSET 0x02011738
    private static void BandFade_Setup(BandFadeData data, BandFadeTemplate template, int duration, int stepsPerFrame, int screen, HardwareWindowSettings settings, ScreenFadeHBlanks hblanks, int heapID)
    {
        data.Bands = new FadeBand[template.Count];
        data.BandCount = template.Count;
        for (int i = 0; i < template.Count; i++)
        {
            FadeBand band = new();
            data.Bands[i] = band;
            byte[] from = { template.From[i * 4], template.From[i * 4 + 1], template.From[i * 4 + 2], template.From[i * 4 + 3] };
            byte[] to = { template.To[i * 4], template.To[i * 4 + 1], template.To[i * 4 + 2], template.To[i * 4 + 3] };
            InitRectInterpolation(band.Current, band.Target, band.Step, from, to, duration);
        }
        FadeLineBuffers_Init(data, template.Window, screen, heapID);
        data.Duration = duration;
        data.StepsPerFrame = stepsPerFrame;
        data.Counter = 0;
        data.Settings = settings;
        data.HBlanks = hblanks;
        data.HeapID = heapID;
        data.Deferred = template.Deferred;
        BandFade_FillLines(data);
        SysTask.SysTask_ExecuteAfterVBlank(Task_SwapLineBuffers, data, 0x3FF);
        FadeLineBuffer buffer = FadeLineBuffers_Get(data, 0);
        SetupFadeWindow(settings, template.Inside, template.Outside, template.Window, screen, buffer.Pending[0], 0, buffer.Pending[FadeLineBuffer.Right], FadeLineBuffer.Lines, data.Deferred);
        if (template.Window == 0)
        {
            SetFadeVisibleWindows(data.Settings, 1, screen, data.Deferred);
        }
        else
        {
            SetFadeVisibleWindows(data.Settings, 2, screen, data.Deferred);
        }
        ScreenFadeControl.RequestEnableScreenHBlank(data.HBlanks, data, HBlank_ApplyLineWindows, screen, heapID);
    }

    // OFFSET 0x0201184C
    private static bool BandFade_Step(BandFadeData data)
    {
        data.Counter++;
        if (data.Counter < data.StepsPerFrame)
        {
            return false;
        }
        data.Counter = 0;
        int remaining = data.Duration - 1;
        if (remaining <= 0)
        {
            ScreenFadeControl.RequestDisableScreenHBlank(data.HBlanks!, data.Screen, data.HeapID);
            return true;
        }
        data.Duration = remaining;
        BandFade_AdvanceBands(data);
        BandFade_FillLines(data);
        SysTask.SysTask_ExecuteAfterVBlank(Task_SwapLineBuffers, data, 0x3FF);
        return false;
    }

    // OFFSET 0x0201189C
    private static void BandFade_FreeBands(BandFadeData data)
    {
        data.Bands = null;
    }

    // OFFSET 0x020118AC
    private static void BandFade_FillLines(BandFadeData data)
    {
        FadeLineBuffer buffer = FadeLineBuffers_Get(data, 0);
        Array.Clear(buffer.Pending);
        for (int i = data.BandCount - 1; i >= 0; i--)
        {
            BandFade_FillBandLines(data, data.Bands![i]);
        }
    }

    // OFFSET 0x020118E0
    private static void BandFade_FillBandLines(BandFadeData data, FadeBand band)
    {
        const int Unit = 1 << FractionShift;
        FadeLineBuffer buffer = FadeLineBuffers_Get(data, 0);
        int left = band.Current[0] / Unit;
        int right = band.Current[2] / Unit;
        int bottom = band.Current[3] / Unit;
        int top = band.Current[1] / Unit;
        for (int line = top; line < bottom; line++)
        {
            buffer.Pending[line] = (short)left;
            buffer.Pending[FadeLineBuffer.Right + line] = (short)right;
        }
    }

    // OFFSET 0x02011938
    private static void BandFade_AdvanceBands(BandFadeData data)
    {
        for (int i = 0; i < data.BandCount; i++)
        {
            AddRectStep(data.Bands![i].Current, data.Bands[i].Step);
        }
    }

    // OFFSET 0x0200FEA4
    public static bool ScreenFade_FanOut(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            FanFade_Init(fade, RomData.Arm9(0x020E5144, WedgeTemplateSize));
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return FanFade_Update(fade);
    }

    // OFFSET 0x0200FEEC
    public static bool ScreenFade_FanIn(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            FanFade_Init(fade, RomData.Arm9(0x020E5154, WedgeTemplateSize));
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return FanFade_Update(fade);
    }

    // OFFSET 0x0200FFBC
    public static bool ScreenFade_CircleOut3(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            CircleFade_Init(fade, RomData.Arm9(0x020E5274, WindowTemplateSize));
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return CircleFade_Update(fade);
    }

    // OFFSET 0x0200FFE8
    public static bool ScreenFade_CircleIn3(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            CircleFade_Init(fade, RomData.Arm9(0x020E51CC, WindowTemplateSize));
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return CircleFade_Update(fade);
    }

    // OFFSET 0x02010014
    public static bool ScreenFade_WindowOut5(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            WindowFade_Init(fade, RomData.Arm9(0x020E5208, WindowTemplateSize));
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return WindowFade_Update(fade);
    }

    // OFFSET 0x02010040
    public static bool ScreenFade_WindowIn5(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            WindowFade_Init(fade, RomData.Arm9(0x020E522C, WindowTemplateSize));
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return WindowFade_Update(fade);
    }

    // OFFSET 0x02011960
    private static void FanFade_Init(ScreenFade fade, byte[] template)
    {
        FanFadeData data = new();
        fade.Data = data;
        FanFade_Setup(data, template, fade.Duration, fade.StepsPerFrame, fade.Screen, fade.HardwareWindows!, fade.HBlanks!, fade.HeapID);
        fade.State++;
    }

    // OFFSET 0x020119A0
    private static bool FanFade_Update(ScreenFade fade)
    {
        bool done = false;
        switch (fade.State)
        {
            case 1:
                FanFadeData data = (FanFadeData)fade.Data!;
                if (FanFade_Step(data))
                {
                    FinishWindowFade(data.Deferred, data.Settings!, fade.Screen);
                    fade.State++;
                }
                break;
            case 2:
                FanFade_Cleanup((FanFadeData)fade.Data!);
                FadeLineBuffers_Free((FanFadeData)fade.Data!);
                fade.Data = null;
                fade.State++;
                done = true;
                break;
            case 3:
                done = true;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return done;
    }

    // OFFSET 0x02011A00
    private static void FanFade_Setup(FanFadeData data, byte[] template, int duration, int stepsPerFrame, int screen, HardwareWindowSettings settings, ScreenFadeHBlanks hblanks, int heapID)
    {
        ushort angleStart = (ushort)(template[0] | (template[1] << 8));
        ushort angleEnd = (ushort)(template[2] | (template[3] << 8));
        data.Angle = 0;
        data.AngleStart = angleStart;
        data.AngleDelta = angleEnd - angleStart;
        FadeLineBuffers_Init(data, 2, screen, heapID);
        data.Duration = duration;
        data.StepIndex = 0;
        data.StepsPerFrame = stepsPerFrame;
        data.Counter = 0;
        data.Settings = settings;
        data.HBlanks = hblanks;
        data.HeapID = heapID;
        data.Deferred = template[FanDeferred] | (template[FanDeferred + 1] << 8);
        FanFade_CalcAngle(data, data.StepIndex, data.Duration);
        FanFade_FillLines(data);
        SysTask.SysTask_ExecuteAfterVBlank(Task_SwapLineBuffers, data, 0x3FF);
        FadeLineBuffer first = FadeLineBuffers_Get(data, 0);
        FadeLineBuffer second = FadeLineBuffers_Get(data, 1);
        SetupFadeWindow(settings, template[FanInside], template[FanOutside], 0, screen, first.Pending[0], 0, first.Pending[FadeLineBuffer.Right], FadeLineBuffer.Lines, data.Deferred);
        SetupFadeWindow(settings, template[FanInside], template[FanOutside], 1, screen, second.Pending[0], 0, second.Pending[FadeLineBuffer.Right], FadeLineBuffer.Lines, data.Deferred);
        SetFadeVisibleWindows(settings, 3, screen, data.Deferred);
        ScreenFadeControl.RequestEnableScreenHBlank(data.HBlanks, data, HBlank_ApplyLineWindows, screen, heapID);
    }

    // OFFSET 0x02011AFC
    private static bool FanFade_Step(FanFadeData data)
    {
        data.Counter++;
        if (data.Counter < data.StepsPerFrame)
        {
            return false;
        }
        data.Counter = 0;
        int next = data.StepIndex + 1;
        if (next > data.Duration)
        {
            ScreenFadeControl.RequestDisableScreenHBlank(data.HBlanks!, data.Screen, data.HeapID);
            return true;
        }
        data.StepIndex = next;
        FanFade_CalcAngle(data, next, data.Duration);
        FanFade_FillLines(data);
        SysTask.SysTask_ExecuteAfterVBlank(Task_SwapLineBuffers, data, 0x3FF);
        return false;
    }

    // OFFSET 0x02011B54
    private static void FanFade_Cleanup(FanFadeData data)
    {
    }

    // OFFSET 0x02011B58
    private static void FanFade_FillLines(FanFadeData data)
    {
        ushort angle = (ushort)(data.Angle % QuarterTurn);
        FadeLineBuffer first = FadeLineBuffers_Get(data, 0);
        FadeLineBuffer second = FadeLineBuffers_Get(data, 1);
        for (int line = 0; line < FadeLineBuffer.Lines / 2; line++)
        {
            int mirrored = FadeLineBuffer.Lines - 1 - line;
            if (data.Angle < QuarterTurn)
            {
                int width = CalcSlopeOffset(angle, FadeLineBuffer.Lines / 2 - line);
                if (width > MaxHalfWidth)
                {
                    width = MaxHalfWidth;
                }
                first.Pending[mirrored] = (short)(ScreenCenterX - width);
                first.Pending[FadeLineBuffer.Right + mirrored] = ScreenCenterX;
                second.Pending[line] = ScreenCenterX;
                second.Pending[FadeLineBuffer.Right + line] = (short)(width + ScreenCenterX);
            }
            else
            {
                first.Pending[mirrored] = 0;
                first.Pending[FadeLineBuffer.Right + mirrored] = ScreenCenterX;
                second.Pending[line] = ScreenCenterX;
                second.Pending[FadeLineBuffer.Right + line] = ScreenRight;
            }
        }
        ushort remaining = (ushort)(QuarterTurn - angle);
        for (int line = FadeLineBuffer.Lines / 2; line < FadeLineBuffer.Lines; line++)
        {
            int mirrored = FadeLineBuffer.Lines - 1 - line;
            if (data.Angle < QuarterTurn)
            {
                first.Pending[mirrored] = ScreenCenterX;
                first.Pending[FadeLineBuffer.Right + mirrored] = ScreenCenterX;
                second.Pending[line] = ScreenCenterX;
                second.Pending[FadeLineBuffer.Right + line] = ScreenCenterX;
            }
            else
            {
                int width = CalcSlopeOffset(remaining, line - FadeLineBuffer.Lines / 2);
                if (width > MaxHalfWidth)
                {
                    width = MaxHalfWidth;
                }
                first.Pending[mirrored] = 0;
                first.Pending[FadeLineBuffer.Right + mirrored] = (short)(ScreenCenterX - width);
                second.Pending[line] = (short)(width + ScreenCenterX);
                second.Pending[FadeLineBuffer.Right + line] = ScreenRight;
            }
        }
    }

    // OFFSET 0x02011C7C
    private static void FanFade_CalcAngle(FanFadeData data, int step, int duration)
    {
        data.Angle = data.AngleDelta * step / duration + data.AngleStart;
    }

    // OFFSET 0x0200FF30
    public static bool ScreenFade_CrossOut(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            CrossFade_Init(fade, RomData.Arm9(0x020E5164, WedgeTemplateSize));
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return CrossFade_Update(fade);
    }

    // OFFSET 0x0200FF78
    public static bool ScreenFade_CrossIn(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            CrossFade_Init(fade, RomData.Arm9(0x020E5134, WedgeTemplateSize));
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return CrossFade_Update(fade);
    }

    // OFFSET 0x02011C94
    private static void CrossFade_Init(ScreenFade fade, byte[] template)
    {
        FanFadeData data = new();
        fade.Data = data;
        CrossFade_Setup(data, template, fade.Duration, fade.StepsPerFrame, fade.Screen, fade.HardwareWindows!, fade.HBlanks!, fade.HeapID);
        fade.State++;
    }

    // OFFSET 0x02011CD4
    private static bool CrossFade_Update(ScreenFade fade)
    {
        bool done = false;
        switch (fade.State)
        {
            case 1:
                FanFadeData data = (FanFadeData)fade.Data!;
                if (CrossFade_Step(data))
                {
                    FinishWindowFade(data.Deferred, data.Settings!, fade.Screen);
                    fade.State++;
                }
                break;
            case 2:
                CrossFade_Cleanup((FanFadeData)fade.Data!);
                FadeLineBuffers_Free((FanFadeData)fade.Data!);
                fade.Data = null;
                fade.State++;
                done = true;
                break;
            case 3:
                done = true;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return done;
    }

    // OFFSET 0x02011D34
    private static void CrossFade_Setup(FanFadeData data, byte[] template, int duration, int stepsPerFrame, int screen, HardwareWindowSettings settings, ScreenFadeHBlanks hblanks, int heapID)
    {
        ushort angleStart = (ushort)(template[0] | (template[1] << 8));
        ushort angleEnd = (ushort)(template[2] | (template[3] << 8));
        data.Angle = angleStart;
        data.AngleStart = angleStart;
        data.AngleDelta = angleEnd - angleStart;
        FadeLineBuffers_Init(data, 2, screen, heapID);
        data.Duration = duration;
        data.StepIndex = 0;
        data.StepsPerFrame = stepsPerFrame;
        data.Counter = 0;
        data.Settings = settings;
        data.HBlanks = hblanks;
        data.HeapID = heapID;
        data.Deferred = template[FanDeferred] | (template[FanDeferred + 1] << 8);
        CrossFade_FillLines(data);
        SysTask.SysTask_ExecuteAfterVBlank(Task_SwapLineBuffers, data, 0x3FF);
        FadeLineBuffers_Get(data, 0);
        FadeLineBuffers_Get(data, 1);
        SetupFadeWindow(settings, template[FanInside], template[FanOutside], 0, screen, 0, 0, ScreenRight, FadeLineBuffer.Lines, data.Deferred);
        SetupFadeWindow(settings, template[FanInside], template[FanOutside], 1, screen, 0, 0, ScreenRight, FadeLineBuffer.Lines, data.Deferred);
        SetFadeVisibleWindows(settings, 3, screen, data.Deferred);
        ScreenFadeControl.RequestEnableScreenHBlank(data.HBlanks, data, HBlank_ApplyLineWindows, screen, heapID);
    }

    // OFFSET 0x02011E04
    private static bool CrossFade_Step(FanFadeData data)
    {
        data.Counter++;
        if (data.Counter < data.StepsPerFrame)
        {
            return false;
        }
        data.Counter = 0;
        int next = data.StepIndex + 1;
        if (next > data.Duration)
        {
            ScreenFadeControl.RequestDisableScreenHBlank(data.HBlanks!, data.Screen, data.HeapID);
            return true;
        }
        data.StepIndex = next;
        CrossFade_CalcAngle(data, next, data.Duration);
        CrossFade_FillLines(data);
        SysTask.SysTask_ExecuteAfterVBlank(Task_SwapLineBuffers, data, 0x3FF);
        return false;
    }

    // OFFSET 0x02011E5C
    private static void CrossFade_Cleanup(FanFadeData data)
    {
    }

    // OFFSET 0x02011E60
    private static void CrossFade_FillLines(FanFadeData data)
    {
        ushort angle = (ushort)data.Angle;
        ushort complement = (ushort)(QuarterTurn - angle);
        FadeLineBuffer first = FadeLineBuffers_Get(data, 0);
        FadeLineBuffer second = FadeLineBuffers_Get(data, 1);
        for (int line = 0; line < FadeLineBuffer.Lines / 2; line++)
        {
            int height = FadeLineBuffer.Lines / 2 - line;
            int inner = CalcSlopeOffset(angle, height);
            int outer = CalcSlopeOffset(complement, height);
            if (inner > MaxHalfWidth)
            {
                inner = MaxHalfWidth;
            }
            if (outer > MaxHalfWidth)
            {
                outer = MaxHalfWidth;
            }
            int mirrored = FadeLineBuffer.Lines - 1 - line;
            short firstLeft = (short)(ScreenCenterX - outer);
            short firstRight = (short)(ScreenCenterX - inner);
            first.Pending[line] = firstLeft;
            first.Pending[FadeLineBuffer.Right + line] = firstRight;
            first.Pending[mirrored] = firstLeft;
            first.Pending[FadeLineBuffer.Right + mirrored] = firstRight;
            short secondLeft = (short)(inner + ScreenCenterX);
            short secondRight = (short)(outer + ScreenCenterX);
            second.Pending[line] = secondLeft;
            second.Pending[FadeLineBuffer.Right + line] = secondRight;
            second.Pending[mirrored] = secondLeft;
            second.Pending[FadeLineBuffer.Right + mirrored] = secondRight;
        }
    }

    // OFFSET 0x02011F2C
    private static void CrossFade_CalcAngle(FanFadeData data, int step, int duration)
    {
        data.Angle = data.AngleDelta * step / duration + data.AngleStart;
    }

    // OFFSET 0x0200F898
    public static bool ScreenFade_PatternOut1(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            PatternFadeTemplate template = LoadPatternTemplate(0x020E5128, 0x021007CC);
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            PatternFade_Init(fade, template);
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return PatternFade_Update(fade);
    }

    // OFFSET 0x0200F8D4
    public static bool ScreenFade_PatternIn1(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            PatternFadeTemplate template = LoadPatternTemplate(0x020E5120, 0x021007BC);
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            PatternFade_Init(fade, template);
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return PatternFade_Update(fade);
    }

    // OFFSET 0x0200F90C
    public static bool ScreenFade_PatternOut2(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            PatternFadeTemplate template = LoadPatternTemplate(0x020E511C, 0x021007B4);
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            PatternFade_Init(fade, template);
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return PatternFade_Update(fade);
    }

    // OFFSET 0x0200F948
    public static bool ScreenFade_PatternIn2(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            PatternFadeTemplate template = LoadPatternTemplate(0x020E5124, 0x021007E4);
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            PatternFade_Init(fade, template);
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return PatternFade_Update(fade);
    }

    // OFFSET 0x0200F9D8
    public static bool ScreenFade_PatternOut3(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            PatternFadeTemplate template = LoadPatternTemplate(0x020E515C, 0x021007DC);
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            PatternFade_Init(fade, template);
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return PatternFade_Update(fade);
    }

    // OFFSET 0x0200FA14
    public static bool ScreenFade_PatternIn3(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            PatternFadeTemplate template = LoadPatternTemplate(0x020E518C, 0x021007EC);
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            PatternFade_Init(fade, template);
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return PatternFade_Update(fade);
    }

    // OFFSET 0x0200FA4C
    public static bool ScreenFade_PatternOut4(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            PatternFadeTemplate template = LoadPatternTemplate(0x020E513C, 0x021007C4);
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            PatternFade_Init(fade, template);
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return PatternFade_Update(fade);
    }

    // OFFSET 0x0200FA88
    public static bool ScreenFade_PatternIn4(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            PatternFadeTemplate template = LoadPatternTemplate(0x020E5194, 0x021007F4);
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            PatternFade_Init(fade, template);
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return PatternFade_Update(fade);
    }

    // OFFSET 0x0200FE30
    public static bool ScreenFade_PatternOut5(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            PatternFadeTemplate template = LoadPatternTemplate(0x020E5298, 0x021007D4);
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            PatternFade_Init(fade, template);
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return PatternFade_Update(fade);
    }

    // OFFSET 0x0200FE6C
    public static bool ScreenFade_PatternIn5(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            PatternFadeTemplate template = LoadPatternTemplate(0x020E51C0, 0x021007FC);
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            PatternFade_Init(fade, template);
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return PatternFade_Update(fade);
    }

    private static PatternFadeTemplate LoadPatternTemplate(uint tableAddress, uint templateAddress)
    {
        ushort[] fields = RomData.Arm9U16(templateAddress + 4, 2);
        return new PatternFadeTemplate
        {
            Table = RomData.Arm9(tableAddress, fields[0] * 4),
            Count = fields[0],
            Mode = fields[1],
        };
    }

    // OFFSET 0x02011F44
    private static void PatternFade_Init(ScreenFade fade, PatternFadeTemplate template)
    {
        PatternFadeData data = new();
        fade.Data = data;
        PatternFade_Setup(data, template, fade.Duration, fade.StepsPerFrame, fade.Screen, fade.HardwareWindows!, fade.HBlanks!, fade.HeapID);
        fade.State++;
    }

    // OFFSET 0x02011F88
    private static bool PatternFade_Update(ScreenFade fade)
    {
        bool done = false;
        switch (fade.State)
        {
            case 1:
                PatternFadeData data = (PatternFadeData)fade.Data!;
                if (PatternFade_Step(data))
                {
                    FinishWindowFade(data.Mode, data.Settings!, fade.Screen);
                    fade.State++;
                }
                break;
            case 2:
                PatternFade_Cleanup((PatternFadeData)fade.Data!);
                fade.Data = null;
                fade.State++;
                done = true;
                break;
            case 3:
                done = true;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return done;
    }

    // OFFSET 0x02011FE8
    private static void PatternFade_Setup(PatternFadeData data, PatternFadeTemplate template, int duration, int stepsPerFrame, int screen, HardwareWindowSettings settings, ScreenFadeHBlanks hblanks, int heapID)
    {
        FadeWindowData_Init(data, (byte)screen, 1, null, null);
        byte initial = template.Mode == 0 ? (byte)1 : (byte)0;
        Array.Fill(data.Halves[0].Pending, initial);
        Array.Fill(data.Halves[0].Current, initial);
        data.Table = template.Table;
        data.Count = template.Count;
        data.Mode = template.Mode;
        data.HeapID = heapID;
        data.Duration = duration;
        data.StepIndex = 0;
        data.StepsPerFrame = stepsPerFrame;
        data.Counter = 0;
        data.Settings = settings;
        data.HBlanks = hblanks;
        EnableWindowFlagHBlank(hblanks, data, heapID);
        if (template.Mode == 1)
        {
            SetupFadeWindow(settings, 0x20, 0x3F, 0, screen, 0, 0, 0, 0, template.Mode);
        }
        else
        {
            SetupFadeWindow(settings, 0x3F, 0x20, 0, screen, 0, 0, 0, 0, template.Mode);
        }
        SetFadeVisibleWindows(settings, 1, screen, data.Mode);
    }

    // OFFSET 0x020120D4
    private static bool PatternFade_Step(PatternFadeData data)
    {
        data.Counter++;
        if (data.Counter < data.StepsPerFrame)
        {
            return false;
        }
        data.Counter = 0;
        if (data.StepIndex + 1 > data.Duration)
        {
            DisableWindowFlagHBlank(data.HBlanks!, data, data.HeapID);
            return true;
        }
        data.StepIndex++;
        PatternFade_FillFlags(data);
        RequestSwapWindowFlags(data);
        return false;
    }

    // OFFSET 0x02012134
    private static void PatternFade_Cleanup(PatternFadeData data)
    {
    }

    // OFFSET 0x02012138
    private static void PatternFade_FillFlags(PatternFadeData data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            PatternFade_FillEntryFlags(data.Table, i * 4, data.Halves[0].Pending, data.StepIndex, data.Duration);
        }
    }

    // OFFSET 0x02012174
    private static void PatternFade_FillEntryFlags(byte[] table, int entry, byte[] flags, int step, int duration)
    {
        int from = table[entry];
        int to = table[entry + 1];
        int initial = table[entry + 2] | (table[entry + 3] << 8);
        int boundary = (to - from) * step / duration + from;
        int start;
        int end;
        int value;
        if (from <= to)
        {
            start = from;
            end = to;
            value = initial;
        }
        else
        {
            start = to;
            end = from;
            value = initial == 0 ? 1 : 0;
        }
        for (int line = start; line < end; line++)
        {
            if (line == boundary)
            {
                value = value == 0 ? 1 : 0;
            }
            flags[line] = (byte)value;
        }
    }

    // OFFSET 0x0201006C
    public static bool ScreenFade_CombinedOut(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            CombinedFadeTemplate template = LoadCombinedTemplate(0x020E517C, 0x02100804);
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            CombinedFade_Init(fade, template);
            fade.FadesOut = 1;
            fade.UsesBrightness = 0;
            return false;
        }
        return CombinedFade_Update(fade);
    }

    // OFFSET 0x020100A8
    public static bool ScreenFade_CombinedIn(ScreenFade fade)
    {
        if (fade.State == 0)
        {
            CombinedFadeTemplate template = LoadCombinedTemplate(0x020E512C, 0x0210081C);
            ScreenFadeControl.SetScreenBackgroundColor(fade.Color);
            CombinedFade_Init(fade, template);
            fade.FadesOut = 0;
            fade.UsesBrightness = 0;
            return false;
        }
        return CombinedFade_Update(fade);
    }

    private static CombinedFadeTemplate LoadCombinedTemplate(uint tableAddress, uint templateAddress)
    {
        return new CombinedFadeTemplate
        {
            Window = RomData.Arm9(templateAddress, WindowTemplateSize),
            Pattern = LoadPatternTemplate(tableAddress, templateAddress + WindowTemplateSize),
            Ratio = (int)RomData.Arm9U32(templateAddress + 0x14, 1)[0],
        };
    }

    // OFFSET 0x020121C4
    private static void CombinedFade_Init(ScreenFade fade, CombinedFadeTemplate template)
    {
        CombinedFadeData data = new();
        fade.Data = data;
        if (template.Window[TemplateDeferred] == 0)
        {
            CombinedFade_SetupWindowFirst(data, template, fade.Duration, fade.StepsPerFrame, fade.Screen, fade.HardwareWindows!, fade.HBlanks!, fade.HeapID);
        }
        else
        {
            CombinedFade_SetupPatternFirst(data, template, fade.Duration, fade.StepsPerFrame, fade.Screen, fade.HardwareWindows!, fade.HBlanks!, fade.HeapID);
        }
        fade.State++;
    }

    // OFFSET 0x02012228
    private static bool CombinedFade_Update(ScreenFade fade)
    {
        bool done = false;
        switch (fade.State)
        {
            case 1:
                CombinedFadeData data = (CombinedFadeData)fade.Data!;
                bool finished;
                if (data.Order == 0)
                {
                    finished = CombinedFade_StepWindowFirst(data, fade);
                }
                else
                {
                    finished = CombinedFade_StepPatternFirst(data, fade);
                }
                if (finished)
                {
                    FinishWindowFade(fade.FadesOut, fade.HardwareWindows!, fade.Screen);
                    fade.State++;
                }
                break;
            case 2:
                fade.Data = null;
                fade.State++;
                done = true;
                break;
            case 3:
                done = true;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return done;
    }

    // OFFSET 0x02012290
    private static void CombinedFade_SetupWindowFirst(CombinedFadeData data, CombinedFadeTemplate template, int duration, int stepsPerFrame, int screen, HardwareWindowSettings settings, ScreenFadeHBlanks hblanks, int heapID)
    {
        int windowDuration = Fx32.Mul(duration << Fx32.Shift, template.Ratio) >> Fx32.Shift;
        data.SplitDuration = (byte)(duration - windowDuration);
        data.Template = template;
        data.Order = template.Window[TemplateDeferred];
        WindowFade_Setup(data.Window, template.Window, windowDuration, stepsPerFrame, screen, settings);
        if (template.Window[TemplateWindow] == 0)
        {
            SetFadeVisibleWindows(settings, 1, screen, template.Window[TemplateDeferred]);
        }
        else
        {
            SetFadeVisibleWindows(settings, 2, screen, template.Window[TemplateDeferred]);
        }
        data.Phase = 0;
    }

    // OFFSET 0x02012310
    private static bool CombinedFade_StepWindowFirst(CombinedFadeData data, ScreenFade fade)
    {
        bool done = false;
        switch (data.Phase)
        {
            case 0:
                if (WindowFade_Step(data.Window))
                {
                    data.Phase++;
                    PatternFade_Setup(data.Pattern, data.Template!.Pattern, data.SplitDuration, fade.StepsPerFrame, fade.Screen, fade.HardwareWindows!, fade.HBlanks!, fade.HeapID);
                }
                break;
            case 1:
                if (PatternFade_Step(data.Pattern))
                {
                    done = true;
                    data.Phase++;
                }
                break;
            case 2:
                done = true;
                break;
        }
        return done;
    }

    // OFFSET 0x02012384
    private static void CombinedFade_SetupPatternFirst(CombinedFadeData data, CombinedFadeTemplate template, int duration, int stepsPerFrame, int screen, HardwareWindowSettings settings, ScreenFadeHBlanks hblanks, int heapID)
    {
        data.SplitDuration = (byte)(Fx32.Mul(duration << Fx32.Shift, template.Ratio) >> Fx32.Shift);
        int patternDuration = duration - data.SplitDuration;
        data.Template = template;
        data.Order = template.Window[TemplateDeferred];
        PatternFade_Setup(data.Pattern, template.Pattern, patternDuration, stepsPerFrame, screen, settings, hblanks, heapID);
        data.Phase = 0;
    }

    // OFFSET 0x020123F4
    private static bool CombinedFade_StepPatternFirst(CombinedFadeData data, ScreenFade fade)
    {
        bool done = false;
        switch (data.Phase)
        {
            case 0:
                if (PatternFade_Step(data.Pattern))
                {
                    data.Phase++;
                    CombinedFadeTemplate template = data.Template!;
                    WindowFade_Setup(data.Window, template.Window, data.SplitDuration, fade.StepsPerFrame, fade.Screen, fade.HardwareWindows!);
                    if (template.Window[TemplateWindow] == 0)
                    {
                        SetFadeVisibleWindows(fade.HardwareWindows!, 1, fade.Screen, template.Window[TemplateDeferred]);
                    }
                    else
                    {
                        SetFadeVisibleWindows(fade.HardwareWindows!, 2, fade.Screen, template.Window[TemplateDeferred]);
                    }
                }
                break;
            case 1:
                if (WindowFade_Step(data.Window))
                {
                    done = true;
                    data.Phase++;
                }
                break;
            case 2:
                done = true;
                break;
        }
        return done;
    }

    private static void ApplyLineWindow(FadeLineBuffers buffers, FadeLineBuffer buffer, int line)
    {
        short left = buffer.Current[line];
        short right = buffer.Current[FadeLineBuffer.Right + line];
        if (!Video.InHBlank)
        {
            return;
        }
        int display = buffers.Screen == 0 ? 0 : 1;
        int window = buffer.Index == 0 ? 0 : 1;
        Video.SetWindowHorizontal(display, window, (byte)left, (byte)right);
        Video.SetWindowVertical(display, window, 0, FadeLineBuffer.Lines);
    }

    // OFFSET 0x0201035C
    private static void HBlank_ApplyLineWindows(object? param)
    {
        FadeLineBuffers? buffers = (FadeLineBuffers?)param;
        if (buffers == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        int vcount = Video.VCount;
        if (vcount >= FadeLineBuffer.Lines)
        {
            return;
        }
        int line = vcount + 1;
        if (line > FadeLineBuffer.Lines - 1)
        {
            line -= FadeLineBuffer.Lines;
        }
        if (buffers!.Count == 1)
        {
            ApplyLineWindow(buffers, FadeLineBuffers_Get(buffers, 0), line);
        }
        else
        {
            ApplyLineWindow(buffers, FadeLineBuffers_Get(buffers, 0), line);
            ApplyLineWindow(buffers, FadeLineBuffers_Get(buffers, 1), line);
        }
    }

    private static void ApplyWindowFlag(FadeWindowData data, FadeWindowHalf half, int line)
    {
        const byte ShowAll = 0x3F;
        const byte EffectOnly = 0x20;
        int display = data.Screen == 0 ? 0 : 1;
        int window = half.Source == null ? 0 : 1;
        if (half.Current[line] == 0)
        {
            if (Video.InHBlank)
            {
                Video.SetWindowOutside(display, ShowAll);
            }
            if (Video.InHBlank)
            {
                Video.SetWindowInside(display, window, EffectOnly);
            }
        }
        else
        {
            if (Video.InHBlank)
            {
                Video.SetWindowOutside(display, EffectOnly);
            }
            if (Video.InHBlank)
            {
                Video.SetWindowInside(display, window, ShowAll);
            }
        }
    }

    // OFFSET 0x020107D8
    private static void HBlank_ApplyWindowFlags(object? param)
    {
        FadeWindowData? data = (FadeWindowData?)param;
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        int vcount = Video.VCount;
        if (vcount >= FadeWindowHalf.Size)
        {
            return;
        }
        int line = vcount + 1;
        if (line > FadeWindowHalf.Size - 1)
        {
            line -= FadeWindowHalf.Size;
        }
        if (data!.Type == 1)
        {
            ApplyWindowFlag(data, data.Halves[0], line);
        }
        else
        {
            ApplyWindowFlag(data, data.Halves[0], line);
            ApplyWindowFlag(data, data.Halves[1], line);
        }
    }
}
