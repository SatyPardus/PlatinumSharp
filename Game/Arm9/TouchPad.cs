using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed class TouchPadDataBuffer
{
    public const int Capacity = 8;

    public ushort BufferSize;
    public readonly TouchPadData[] Buffer = new TouchPadData[Capacity];
}

public static class TouchPad
{
    public const int WriteMethodNone = 0;
    public const int WriteMethodChangedWrap = 1;
    public const int WriteMethodNone2 = 2;
    public const int WriteMethodChangedStop = 3;
    public const int WriteMethodAllWrap = 4;
    public const int WriteMethodAllStop = 5;
    public const uint ResultOk = 1;
    public const uint ResultError = 2;
    public const uint ResultNotActive = 3;
    public const uint ResultBufferFull = 0xFFFFFFFF;

    private const int ModeOff = 0;
    private const int ModeExternalBuffer = 1;
    private const int ModeNoBuffer = 2;
    private const int InternalBufferSize = 9;
    private const int MaxFrequency = 5;
    private const int MaxRetries = 5;

    private static TouchPadData[]? sExternalBuffer;
    private static uint sExternalBufferSize;
    private static uint sSamplesPerFrame;
    private static readonly TouchPadData[] sInternalBuffer = new TouchPadData[InternalBufferSize];
    private static uint sExternalBufferWriteIndex;
    private static ushort sMode;
    private static ushort sSuspended;

    // OFFSET 0x0201E3D8
    public static void EnableTouchPad()
    {
        if (sSuspended != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        sMode = ModeOff;
        sSuspended = 0;
    }

    // OFFSET 0x0201E3F4
    public static uint InitializeTouchPadWithExternalBuffer(TouchPadData[] externalBuffer, uint externalBufferSize, uint autoSamplingFrequency)
    {
        if (sSuspended != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (autoSamplingFrequency >= MaxFrequency || autoSamplingFrequency == 0)
        {
            return 0;
        }
        if (sMode != ModeOff)
        {
            return 0;
        }
        uint result = TouchPad_StartAutoSampling(autoSamplingFrequency);
        if (result == ResultOk)
        {
            TouchPad_SetState(ModeExternalBuffer, 1, externalBuffer, externalBufferSize, 0, autoSamplingFrequency << 1);
            return 1;
        }
        return result;
    }

    // OFFSET 0x0201E450
    public static uint InitializeTouchPad(uint autoSamplingFrequency)
    {
        if (sSuspended != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (autoSamplingFrequency >= MaxFrequency || autoSamplingFrequency == 0)
        {
            return 0;
        }
        if (sMode != ModeOff)
        {
            return 0;
        }
        uint result = TouchPad_StartAutoSampling(autoSamplingFrequency);
        if (result == ResultOk)
        {
            TouchPad_SetState(ModeNoBuffer, 1, null, 0, 0, autoSamplingFrequency << 1);
            return 1;
        }
        return result;
    }

    // OFFSET 0x0201E4A4
    private static void TouchPad_SetState(ushort mode, byte autoSampling, TouchPadData[]? externalBuffer, uint externalBufferSize, uint writeIndex, uint samplesPerFrame)
    {
        sMode = mode;
        gSystem.TouchAutoSampling = autoSampling;
        sExternalBuffer = externalBuffer;
        sExternalBufferSize = externalBufferSize;
        sSamplesPerFrame = samplesPerFrame;
        sExternalBufferWriteIndex = writeIndex;
        TouchPad_ClearTouchFlags(sInternalBuffer, InternalBufferSize);
    }

    // OFFSET 0x0201E4D8
    private static void TouchPad_ClearTouchFlags(TouchPadData[] buffer, int count)
    {
        for (int i = 0; i < count; i++)
        {
            buffer[i].Touch = 0;
        }
    }

    // OFFSET 0x0201E4EC
    private static uint TouchPad_StopAutoSampling()
    {
        uint retries = 0;
        if (sMode == ModeOff)
        {
            return ResultOk;
        }
        bool failed;
        do
        {
            failed = !Input.StopAutoSampling();
            if (failed)
            {
                retries++;
            }
        } while (failed && retries <= MaxRetries);
        if (retries > MaxRetries)
        {
            return ResultError;
        }
        return ResultOk;
    }

    // OFFSET 0x0201E530
    public static uint DisableTouchPad()
    {
        if (sSuspended != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        uint result = TouchPad_StopAutoSampling();
        if (result == ResultOk)
        {
            TouchPad_SetState(ModeOff, 0, null, 0, 0, 0);
        }
        return result;
    }

    // OFFSET 0x0201E564
    public static uint WriteAutoSamplingDataToBuffer(TouchPadDataBuffer? outBuffer, int externalBufferWriteMethod, uint changeThreshold)
    {
        uint result = ResultNotActive;
        if (sSuspended != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (sMode != ModeOff)
        {
            uint latestIndex = Input.LatestAutoSampleIndex;
            ConvertTouchPadDataToScreenSpace(sInternalBuffer, InternalBufferSize);
            if (outBuffer != null)
            {
                TouchPad_CopySamplesToBuffer(outBuffer, latestIndex);
            }
            if (sMode == ModeExternalBuffer)
            {
                result = TouchPad_WriteToExternalBuffer(externalBufferWriteMethod, latestIndex, changeThreshold);
            }
            else
            {
                result = ResultOk;
            }
        }
        return result;
    }

    // OFFSET 0x0201E5C0
    public static void ConvertTouchPadDataToScreenSpace(TouchPadData[] touchPadDataBuffer, uint touchPadDataBufferSize)
    {
        for (uint i = 0; i < touchPadDataBufferSize; i++)
        {
            Input.GetCalibratedPoint(touchPadDataBuffer[i], out TouchPadData calibrated);
            touchPadDataBuffer[i].X = calibrated.X;
            touchPadDataBuffer[i].Y = calibrated.Y;
            touchPadDataBuffer[i].Touch = calibrated.Touch;
            touchPadDataBuffer[i].Validity = calibrated.Validity;
        }
    }

    // OFFSET 0x0201E5FC
    public static void AfterSleep()
    {
        if (sSuspended == 0)
        {
            return;
        }
        if (sMode == ModeOff)
        {
            return;
        }
        if (TouchPad_StartAutoSampling(sSamplesPerFrame >> 1) != ResultOk)
        {
            ErrorHandling.GF_AssertFail();
        }
        sSuspended = 0;
    }

    // OFFSET 0x0201E630
    public static void BeforeSleep()
    {
        if (sSuspended == 1)
        {
            return;
        }
        if (sMode == ModeOff)
        {
            return;
        }
        if (TouchPad_StopAutoSampling() != ResultOk)
        {
            ErrorHandling.GF_AssertFail();
        }
        sSuspended = 1;
    }

    // OFFSET 0x0201E658
    private static uint TouchPad_StartAutoSampling(uint frequency)
    {
        int retries = 0;
        bool failed;
        do
        {
            failed = !Input.StartAutoSampling((ushort)frequency, sInternalBuffer, InternalBufferSize);
            if (failed)
            {
                retries++;
            }
        } while (failed && retries <= MaxRetries);
        if (retries > MaxRetries)
        {
            return ResultError;
        }
        return ResultOk;
    }

    // OFFSET 0x0201E69C
    private static uint TouchPad_WriteToExternalBuffer(int method, uint latestIndex, uint changeThreshold)
    {
        switch (method)
        {
            case WriteMethodChangedWrap:
            case WriteMethodChangedStop:
                return TouchPad_WriteChangedSamples(method, latestIndex, changeThreshold);
            case WriteMethodAllWrap:
            case WriteMethodAllStop:
                return TouchPad_WriteAllSamples(method, latestIndex);
            default:
                return ResultOk;
        }
    }

    private static int TouchPad_GetSampleIndex(uint latestIndex, uint sample)
    {
        short index = (short)(sample + (latestIndex - sSamplesPerFrame) + 1);
        if (index < 0)
        {
            index = (short)(index + InternalBufferSize);
        }
        return index;
    }

    // OFFSET 0x0201E6CC
    private static uint TouchPad_WriteChangedSamples(int method, uint latestIndex, uint changeThreshold)
    {
        for (uint i = 0; i < sSamplesPerFrame; i++)
        {
            TouchPadData sample = sInternalBuffer[TouchPad_GetSampleIndex(latestIndex, i)];
            if (sample.Touch != 1 || sample.Validity != 0)
            {
                continue;
            }
            TouchPadData previous = default;
            if (sExternalBufferWriteIndex != 0)
            {
                previous = sExternalBuffer![sExternalBufferWriteIndex - 1];
            }
            uint deltaX;
            if (previous.X >= sample.X)
            {
                deltaX = (uint)(previous.X - sample.X);
            }
            else
            {
                deltaX = (uint)(sample.X - previous.X);
            }
            uint deltaY;
            if (previous.Y >= sample.Y)
            {
                deltaY = (uint)(previous.Y - sample.Y);
            }
            else
            {
                deltaY = (uint)(sample.Y - previous.Y);
            }
            if (deltaX < changeThreshold && deltaY < changeThreshold)
            {
                continue;
            }
            sExternalBuffer![sExternalBufferWriteIndex] = sample;
            sExternalBufferWriteIndex++;
            if (sExternalBufferWriteIndex >= sExternalBufferSize)
            {
                if (method == WriteMethodChangedWrap)
                {
                    sExternalBufferWriteIndex %= sExternalBufferSize;
                }
                else
                {
                    return ResultBufferFull;
                }
            }
        }
        return sExternalBufferWriteIndex;
    }

    // OFFSET 0x0201E784
    private static uint TouchPad_WriteAllSamples(int method, uint latestIndex)
    {
        for (uint i = 0; i < sSamplesPerFrame; i++)
        {
            sExternalBuffer![sExternalBufferWriteIndex] = sInternalBuffer[TouchPad_GetSampleIndex(latestIndex, i)];
            sExternalBufferWriteIndex++;
            if (sExternalBufferWriteIndex >= sExternalBufferSize)
            {
                if (method == WriteMethodAllWrap)
                {
                    sExternalBufferWriteIndex %= sExternalBufferSize;
                }
                else
                {
                    return ResultBufferFull;
                }
            }
        }
        return sExternalBufferWriteIndex;
    }

    // OFFSET 0x0201E7FC
    private static void TouchPad_CopySamplesToBuffer(TouchPadDataBuffer outBuffer, uint latestIndex)
    {
        outBuffer.BufferSize = 0;
        for (int i = 0; i < TouchPadDataBuffer.Capacity; i++)
        {
            outBuffer.Buffer[i] = default;
        }
        for (uint i = 0; i < sSamplesPerFrame; i++)
        {
            TouchPadData sample = sInternalBuffer[TouchPad_GetSampleIndex(latestIndex, i)];
            if (sample.Validity == 0)
            {
                outBuffer.Buffer[outBuffer.BufferSize] = sample;
                outBuffer.BufferSize++;
            }
        }
    }
}
