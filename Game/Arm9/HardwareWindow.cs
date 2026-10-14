namespace PlatinumSharp.Game;

public sealed class VisibleWindowsRequest
{
    public int Mask;
    public int Screen;
}

public sealed class WindowInsideRequest
{
    public int Mask;
    public int Effect;
    public int Window;
    public int Screen;
}

public sealed class WindowOutsideRequest
{
    public int Mask;
    public int Effect;
    public int Screen;
}

public sealed class WindowDimensionsRequest
{
    public short Left;
    public short Top;
    public short Right;
    public short Bottom;
    public int Window;
    public int Screen;
}

public sealed class HardwareWindowSettings
{
    public readonly VisibleWindowsRequest[] Visible = { new(), new() };
    public readonly WindowInsideRequest[,] Inside = { { new(), new() }, { new(), new() } };
    public readonly WindowOutsideRequest[] Outside = { new(), new() };
    public readonly WindowDimensionsRequest[,] Dimensions = { { new(), new() }, { new(), new() } };
}

public static class HardwareWindow
{
    private const int ScreenMain = 0;
    private const int DisplayMain = 0;
    private const int DisplaySub = 1;
    private const int EffectBit = 0x20;

    // OFFSET 0x02012480
    public static void SetVisibleHardwareWindows(int mask, int screen)
    {
        if (screen == ScreenMain)
        {
            Video.SetVisibleWindows(DisplayMain, mask);
        }
        else
        {
            Video.SetVisibleWindows(DisplaySub, mask);
        }
    }

    // OFFSET 0x020124AC
    public static void SetHardwareWindowMaskInsidePlane(int mask, int effect, int window, int screen)
    {
        int value = mask;
        if (effect != 0)
        {
            value |= EffectBit;
        }
        if (window == 0)
        {
            if (screen == ScreenMain)
            {
                Video.SetWindowInside(DisplayMain, 0, (byte)value);
            }
            else
            {
                Video.SetWindowInside(DisplaySub, 0, (byte)value);
            }
        }
        else if (screen == ScreenMain)
        {
            Video.SetWindowInside(DisplayMain, 1, (byte)value);
        }
        else
        {
            Video.SetWindowInside(DisplaySub, 1, (byte)value);
        }
    }

    // OFFSET 0x02012534
    public static void SetHardwareWindowMaskOutsidePlane(int mask, int effect, int screen)
    {
        int value = mask;
        if (effect != 0)
        {
            value |= EffectBit;
        }
        if (screen == ScreenMain)
        {
            Video.SetWindowOutside(DisplayMain, (byte)value);
        }
        else
        {
            Video.SetWindowOutside(DisplaySub, (byte)value);
        }
    }

    // OFFSET 0x02012574
    public static void SetHardwareWindowDimensions(int left, int top, int right, int bottom, int window, int screen)
    {
        int display = screen == ScreenMain ? DisplayMain : DisplaySub;
        int index = window == 0 ? 0 : 1;
        Video.SetWindowHorizontal(display, index, (byte)left, (byte)right);
        Video.SetWindowVertical(display, index, (byte)top, (byte)bottom);
    }

    // OFFSET 0x02012634
    public static void RequestVisibleHardwareWindows(HardwareWindowSettings settings, int mask, int screen)
    {
        VisibleWindowsRequest request = settings.Visible[screen];
        request.Mask = mask;
        request.Screen = screen;
        SysTask.SysTask_ExecuteAfterVBlank(Task_SetVisibleHardwareWindows, request, 1);
    }

    // OFFSET 0x02012650
    public static void RequestHardwareWindowMaskInsidePlane(HardwareWindowSettings settings, int mask, int effect, int window, int screen)
    {
        WindowInsideRequest request = settings.Inside[screen, window];
        request.Mask = mask;
        request.Effect = effect;
        request.Window = window;
        request.Screen = screen;
        SysTask.SysTask_ExecuteAfterVBlank(Task_SetHardwareWindowMaskInsidePlane, request, 1);
    }

    // OFFSET 0x02012678
    public static void RequestHardwareWindowMaskOutsidePlane(HardwareWindowSettings settings, int mask, int effect, int screen)
    {
        WindowOutsideRequest request = settings.Outside[screen];
        request.Mask = mask;
        request.Effect = effect;
        request.Screen = screen;
        SysTask.SysTask_ExecuteAfterVBlank(Task_SetHardwareWindowMaskOutsidePlane, request, 1);
    }

    // OFFSET 0x02012698
    public static void RequestHardwareWindowDimensions(HardwareWindowSettings settings, int left, int top, int right, int bottom, int window, int screen)
    {
        WindowDimensionsRequest request = settings.Dimensions[screen, window];
        request.Left = (short)left;
        request.Top = (short)top;
        request.Right = (short)right;
        request.Bottom = (short)bottom;
        request.Window = window;
        request.Screen = screen;
        SysTask.SysTask_ExecuteAfterVBlank(Task_SetHardwareWindowDimensions, request, 1);
    }

    // OFFSET 0x020126CC
    private static void Task_SetVisibleHardwareWindows(SysTask task, object? param)
    {
        VisibleWindowsRequest request = (VisibleWindowsRequest)param!;
        SetVisibleHardwareWindows(request.Mask, request.Screen);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x020126E0
    private static void Task_SetHardwareWindowMaskInsidePlane(SysTask task, object? param)
    {
        WindowInsideRequest request = (WindowInsideRequest)param!;
        SetHardwareWindowMaskInsidePlane(request.Mask, request.Effect, request.Window, request.Screen);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x020126FC
    private static void Task_SetHardwareWindowMaskOutsidePlane(SysTask task, object? param)
    {
        WindowOutsideRequest request = (WindowOutsideRequest)param!;
        SetHardwareWindowMaskOutsidePlane(request.Mask, request.Effect, request.Screen);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x02012714
    private static void Task_SetHardwareWindowDimensions(SysTask task, object? param)
    {
        WindowDimensionsRequest request = (WindowDimensionsRequest)param!;
        SetHardwareWindowDimensions(request.Left, request.Top, request.Right, request.Bottom, request.Window, request.Screen);
        SysTask.SysTask_Done(task);
    }
}
