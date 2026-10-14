namespace PlatinumSharp.Game;

public struct TouchPadData
{
    public ushort X;
    public ushort Y;
    public ushort Touch;
    public ushort Validity;
}

public static class Input
{
    public const int KeyA = 0x0001;
    public const int KeyB = 0x0002;
    public const int KeySelect = 0x0004;
    public const int KeyStart = 0x0008;
    public const int KeyRight = 0x0010;
    public const int KeyLeft = 0x0020;
    public const int KeyUp = 0x0040;
    public const int KeyDown = 0x0080;
    public const int KeyR = 0x0100;
    public const int KeyL = 0x0200;
    public const int KeyX = 0x0400;
    public const int KeyY = 0x0800;
    public const int KeyDebug = 0x2000;
    public const int KeyMask = 0x2FFF;

    public const int TouchValid = 0;
    public const int TouchInvalidX = 1;
    public const int TouchInvalidY = 2;
    public const int TouchInvalidXY = 3;

    public static int Keys { get; set; }

    public static bool Touching { get; set; }

    public static ushort TouchX { get; set; }

    public static ushort TouchY { get; set; }

    public static bool TouchInitialized { get; private set; }

    public static void InitTouch()
    {
        TouchInitialized = true;
    }

    private static TouchPadData[]? autoSamplingBuffer;
    private static int autoSamplingCount;

    public static ushort AutoSamplingFrequency { get; private set; }

    public static ushort LatestAutoSampleIndex { get; private set; }

    public static bool StartAutoSampling(ushort frequency, TouchPadData[] buffer, int count)
    {
        autoSamplingBuffer = buffer;
        autoSamplingCount = count;
        AutoSamplingFrequency = frequency;
        LatestAutoSampleIndex = 0;
        return true;
    }

    public static bool StopAutoSampling()
    {
        autoSamplingBuffer = null;
        autoSamplingCount = 0;
        AutoSamplingFrequency = 0;
        return true;
    }

    public static void PushAutoSample()
    {
        if (autoSamplingBuffer == null || autoSamplingCount == 0)
        {
            return;
        }
        LatestAutoSampleIndex = (ushort)((LatestAutoSampleIndex + 1) % autoSamplingCount);
        autoSamplingBuffer[LatestAutoSampleIndex] = new TouchPadData
        {
            X = TouchX,
            Y = TouchY,
            Touch = (ushort)(Touching ? 1 : 0),
            Validity = TouchValid,
        };
    }

    public static void GetCalibratedPoint(in TouchPadData raw, out TouchPadData point)
    {
        point = raw;
    }

    public static void GetCalibratedPoint(out ushort x, out ushort y, out ushort touch, out ushort validity)
    {
        x = TouchX;
        y = TouchY;
        touch = (ushort)(Touching ? 1 : 0);
        validity = TouchValid;
    }
}
