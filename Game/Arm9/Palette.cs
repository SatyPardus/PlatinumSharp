using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class PaletteFile
{
    public ushort[] Colors = Array.Empty<ushort>();
    public uint ByteSize;
    public int Format;
    public bool Extended;
}

public sealed class PaletteFadeControl
{
    public ushort UnfadedMask;
    public int Wait;
    public int Cur;
    public int End;
    public int NextRGB;
    public bool Sign;
    public int Step;
    public int WaitStep;
}

public sealed class PaletteBuffer
{
    public ushort[]? Unfaded;
    public ushort[]? Faded;
    public uint Size;
    public readonly PaletteFadeControl Fade = new();
}

public sealed class PaletteData
{
    public const int BufferCount = 14;
    public const int StandardBufferCount = 4;

    private const int StandardPaletteSize = 0x10;
    private const int ExtendedPaletteSize = 0x100;

    public readonly PaletteBuffer[] Buffers = CreateBuffers();
    public int SelectedFlag;
    public ushort SelectedBuffers;
    public ushort FadedBuffers;
    public bool FadeTaskActive;
    public bool AutoTransparent;
    public byte ForceExit;

    private static PaletteBuffer[] CreateBuffers()
    {
        PaletteBuffer[] buffers = new PaletteBuffer[BufferCount];
        for (int i = 0; i < BufferCount; i++)
        {
            buffers[i] = new PaletteBuffer();
        }
        return buffers;
    }

    // OFFSET 0x02002F38
    public static PaletteData PaletteData_New(int heapID)
    {
        return new PaletteData();
    }

    // OFFSET 0x02002F54
    public static void PaletteData_Free(PaletteData paletteData)
    {
    }

    // OFFSET 0x02002F5C
    public static void PaletteData_InitBuffer(PaletteData paletteData, int bufferID, ushort[]? unfaded, ushort[]? faded, uint size)
    {
        PaletteBuffer buffer = paletteData.Buffers[bufferID];
        buffer.Unfaded = unfaded;
        buffer.Faded = faded;
        buffer.Size = size;
    }

    // OFFSET 0x02002F70
    public static void PaletteData_AllocBuffer(PaletteData paletteData, int bufferID, uint size, int heapID)
    {
        ushort[] unfaded = new ushort[size / 2];
        ushort[] faded = new ushort[size / 2];
        PaletteData_InitBuffer(paletteData, bufferID, unfaded, faded, size);
    }

    // OFFSET 0x02002FA0
    public static void PaletteData_FreeBuffer(PaletteData paletteData, int bufferID)
    {
        PaletteBuffer buffer = paletteData.Buffers[bufferID];
        buffer.Unfaded = null;
        buffer.Faded = null;
    }

    // OFFSET 0x02002FBC
    public static void PaletteData_LoadBuffer(PaletteData paletteData, ushort[] src, int srcOffset, int bufferID, ushort destStart, ushort srcSize)
    {
        PaletteBuffer buffer = paletteData.Buffers[bufferID];
        Array.Copy(src, srcOffset, buffer.Unfaded!, destStart, srcSize / 2);
        Array.Copy(src, srcOffset, buffer.Faded!, destStart, srcSize / 2);
    }

    // OFFSET 0x02002FEC
    public static void PaletteData_LoadBufferFromFile(PaletteData paletteData, int narcID, int narcMemberIdx, int heapID, int bufferID, uint srcSize, ushort destStart, ushort srcStart)
    {
        PaletteFile? file = Graphics.GetPlttFile(narcID, narcMemberIdx, heapID);
        if (file == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (srcSize == 0)
        {
            srcSize = file!.ByteSize;
        }
        if (destStart * 2 + srcSize > paletteData.Buffers[bufferID].Size)
        {
            ErrorHandling.GF_AssertFail();
        }
        PaletteData_LoadBuffer(paletteData, file!.Colors, srcStart, bufferID, destStart, (ushort)srcSize);
    }

    // OFFSET 0x02003050
    public static void PaletteData_LoadBufferFromFileStart(PaletteData paletteData, int narcID, int narcMemberIdx, int heapID, int bufferID, uint srcSize, ushort destStart)
    {
        PaletteData_LoadBufferFromFile(paletteData, narcID, narcMemberIdx, heapID, bufferID, srcSize, destStart, 0);
    }

    // OFFSET 0x02003070
    public static void PaletteData_LoadBufferFromHardware(PaletteData paletteData, int bufferID, ushort start, uint size)
    {
        if (start * 2 + size > paletteData.Buffers[bufferID].Size)
        {
            ErrorHandling.GF_AssertFail();
        }
        ushort[] hardware;
        switch (bufferID)
        {
            case 0:
                hardware = HardwarePalette.GetHardwareMainBgPaletteAddress();
                break;
            case 1:
                hardware = HardwarePalette.GetHardwareSubBgPaletteAddress();
                break;
            case 2:
                hardware = HardwarePalette.GetHardwareMainObjPaletteAddress();
                break;
            case 3:
                hardware = HardwarePalette.GetHardwareSubObjPaletteAddress();
                break;
            default:
                ErrorHandling.GF_AssertFail();
                return;
        }
        PaletteData_LoadBuffer(paletteData, hardware, start, bufferID, start, (ushort)size);
    }

    // OFFSET 0x020030E4
    public static void LoadPaletteFromFile(int narcID, int narcMemberIdx, int heapID, uint size, ushort start, ushort[] dest)
    {
        PaletteFile? file = Graphics.GetPlttFile(narcID, narcMemberIdx, heapID);
        if (file == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (size == 0)
        {
            size = file!.ByteSize;
        }
        Array.Copy(file!.Colors, start, dest, 0, size / 2);
    }

    // OFFSET 0x02003120
    public static void PaletteData_CopyBuffer(PaletteData palette, int srcBufferID, ushort srcStart, int destBufferID, ushort destStart, ushort size)
    {
        PaletteBuffer source = palette.Buffers[srcBufferID];
        PaletteBuffer destination = palette.Buffers[destBufferID];
        Array.Copy(source.Unfaded!, srcStart, destination.Unfaded!, destStart, size / 2);
        Array.Copy(source.Unfaded!, srcStart, destination.Faded!, destStart, size / 2);
    }

    // OFFSET 0x02003164
    public static ushort[]? PaletteData_GetUnfadedBuffer(PaletteData palette, int bufferID)
    {
        return palette.Buffers[bufferID].Unfaded;
    }

    // OFFSET 0x0200316C
    public static ushort[]? PaletteData_GetFadedBuffer(PaletteData palette, int bufferID)
    {
        return palette.Buffers[bufferID].Faded;
    }

    // OFFSET 0x02003178
    public static byte PaletteData_StartFade(PaletteData paletteData, ushort buffersToFade, ushort palettesToFade, sbyte wait, byte cur, byte end, ushort target)
    {
        byte started = 0;
        ushort requested = palettesToFade;
        for (byte i = 0; i < BufferCount; i++)
        {
            if (IsMaskedOn(buffersToFade, i) == 1 && IsMaskedOn(paletteData.SelectedBuffers, i) == 0)
            {
                PaletteBuffer buffer = paletteData.Buffers[i];
                FilterMaskToValidPalettes(i, buffer, ref palettesToFade);
                SetTimedFadeParams(buffer.Fade, palettesToFade, wait, cur, end, target);
                FlagFadedPaletteBuffer(paletteData, i);
                if (i >= StandardBufferCount)
                {
                    ApplyBlendStepToPaletteBuffer(paletteData, i, ExtendedPaletteSize);
                }
                else
                {
                    ApplyBlendStepToPaletteBuffer(paletteData, i, StandardPaletteSize);
                }
                palettesToFade = requested;
                started = 1;
            }
        }
        if (started == 1)
        {
            paletteData.SelectedBuffers |= buffersToFade;
            if (!paletteData.FadeTaskActive)
            {
                paletteData.FadeTaskActive = true;
                paletteData.SelectedFlag = 1;
                paletteData.ForceExit = 0;
                SysTask.SysTask_Start(SysTask_FadePalette, paletteData, unchecked((uint)-2));
            }
        }
        return started;
    }

    // OFFSET 0x0200327C
    private static byte IsMaskedOn(ushort mask, ushort bit)
    {
        return (byte)((mask & (1 << bit)) != 0 ? 1 : 0);
    }

    // OFFSET 0x02003290
    private static void FlagFadedPaletteBuffer(PaletteData paletteData, ushort bufferID)
    {
        if (IsMaskedOn(paletteData.FadedBuffers, bufferID) == 1)
        {
            return;
        }
        paletteData.FadedBuffers = (ushort)((paletteData.FadedBuffers | (1 << bufferID)) & 0x3FFF);
    }

    // OFFSET 0x020032D0
    private static void FilterMaskToValidPalettes(int bufferID, PaletteBuffer buffer, ref ushort mask)
    {
        byte count = bufferID < StandardBufferCount ? (byte)(buffer.Size / 32) : (byte)(buffer.Size / 512);
        ushort valid = 0;
        for (byte i = 0; i < count; i++)
        {
            valid += (ushort)(1 << i);
        }
        mask &= valid;
    }

    // OFFSET 0x0200330C
    private static void SetTimedFadeParams(PaletteFadeControl fade, ushort unfadedMask, sbyte wait, byte cur, byte end, ushort target)
    {
        if (wait < 0)
        {
            fade.Step = (Math.Abs(wait) + 2) & 0xF;
            fade.Wait = 0;
        }
        else
        {
            fade.Step = 2;
            fade.Wait = wait & 0x3F;
        }
        fade.UnfadedMask = unfadedMask;
        fade.Cur = cur & 0x1F;
        fade.End = end & 0x1F;
        fade.NextRGB = target & 0x7FFF;
        fade.WaitStep = fade.Wait;
        fade.Sign = cur >= end;
    }

    // OFFSET 0x020033CC
    private static void SysTask_FadePalette(SysTask task, object? data)
    {
        PaletteData paletteData = (PaletteData)data!;
        if (paletteData.ForceExit == 1)
        {
            paletteData.ForceExit = 0;
            paletteData.FadedBuffers = 0;
            paletteData.SelectedBuffers = 0;
            paletteData.FadeTaskActive = false;
            SysTask.SysTask_Done(task);
            return;
        }
        if (paletteData.SelectedFlag != 1)
        {
            return;
        }
        paletteData.FadedBuffers = paletteData.SelectedBuffers;
        WaitAndApplyBlendStepToStdPaletteBuffers(paletteData);
        WaitAndApplyBlendStepToExtPaletteBuffers(paletteData);
        if (paletteData.SelectedBuffers == 0)
        {
            paletteData.FadeTaskActive = false;
            SysTask.SysTask_Done(task);
        }
    }

    // OFFSET 0x02003468
    private static void WaitAndApplyBlendStepToStdPaletteBuffers(PaletteData paletteData)
    {
        for (byte i = 0; i < StandardBufferCount; i++)
        {
            WaitAndApplyBlendStepToPaletteBuffer(paletteData, i, StandardPaletteSize);
        }
    }

    // OFFSET 0x02003488
    private static void WaitAndApplyBlendStepToExtPaletteBuffers(PaletteData paletteData)
    {
        for (byte i = StandardBufferCount; i < BufferCount; i++)
        {
            WaitAndApplyBlendStepToPaletteBuffer(paletteData, i, ExtendedPaletteSize);
        }
    }

    // OFFSET 0x020034A8
    private static void WaitAndApplyBlendStepToPaletteBuffer(PaletteData paletteData, ushort bufferID, ushort paletteSize)
    {
        if (IsMaskedOn(paletteData.SelectedBuffers, bufferID) == 0)
        {
            return;
        }
        PaletteFadeControl fade = paletteData.Buffers[bufferID].Fade;
        if (fade.WaitStep < fade.Wait)
        {
            fade.WaitStep++;
            return;
        }
        fade.WaitStep = 0;
        ApplyBlendStepToPaletteBuffer(paletteData, bufferID, paletteSize);
    }

    // OFFSET 0x02003504
    private static void ApplyBlendStepToPaletteBuffer(PaletteData paletteData, ushort bufferID, ushort paletteSize)
    {
        PaletteBuffer buffer = paletteData.Buffers[bufferID];
        int offset = 0;
        for (ushort i = 0; i < 16; i++)
        {
            if (IsMaskedOn(buffer.Fade.UnfadedMask, i) != 0)
            {
                ApplyBlendStepToSinglePalette(buffer.Unfaded!, buffer.Faded!, offset, buffer.Fade, paletteSize);
            }
            offset += paletteSize;
        }
        UpdateFadeBlendStep(paletteData, (byte)bufferID, buffer.Fade);
    }

    // OFFSET 0x02003560
    private static void ApplyBlendStepToSinglePalette(ushort[] unfaded, ushort[] faded, int offset, PaletteFadeControl fade, uint paletteSize)
    {
        for (uint i = 0; i < paletteSize; i++)
        {
            int color = unfaded[offset + i];
            int red = color & 0x1F;
            int green = (color >> 5) & 0x1F;
            int blue = (color >> 10) & 0x1F;
            int target = fade.NextRGB;
            byte newRed = (byte)(red + ((((target & 0x1F) - red) * fade.Cur) >> 4));
            byte newBlue = (byte)(blue + (((((target >> 10) & 0x1F) - blue) * fade.Cur) >> 4));
            byte newGreen = (byte)(green + (((((target >> 5) & 0x1F) - green) * fade.Cur) >> 4));
            faded[offset + i] = (ushort)((newBlue << 10) | (newGreen << 5) | newRed);
        }
    }

    // OFFSET 0x020035EC
    private static void UpdateFadeBlendStep(PaletteData paletteData, byte bufferID, PaletteFadeControl fade)
    {
        if (fade.Cur == fade.End)
        {
            int bit = 1 << bufferID;
            if ((paletteData.SelectedBuffers & bit) != 0)
            {
                paletteData.SelectedBuffers = (ushort)((paletteData.SelectedBuffers ^ bit) & 0x3FFF);
            }
            return;
        }
        if (!fade.Sign)
        {
            short next = (short)(fade.Cur + fade.Step);
            if (next > fade.End)
            {
                next = (short)fade.End;
            }
            fade.Cur = next & 0x1F;
        }
        else
        {
            short next = (short)(fade.Cur - fade.Step);
            if (next < fade.End)
            {
                next = (short)fade.End;
            }
            fade.Cur = next & 0x1F;
        }
    }

    // OFFSET 0x02003694
    public static void PaletteData_CommitFadedBuffers(PaletteData paletteData)
    {
        if (!paletteData.AutoTransparent && paletteData.SelectedFlag != 1)
        {
            return;
        }
        for (int i = 0; i < BufferCount; i++)
        {
            PaletteBuffer buffer = paletteData.Buffers[i];
            if (!paletteData.AutoTransparent)
            {
                if (buffer.Faded == null)
                {
                    continue;
                }
                if (IsMaskedOn(paletteData.FadedBuffers, (ushort)i) == 0)
                {
                    continue;
                }
            }
            if (buffer.Faded == null)
            {
                continue;
            }
            Video.LoadPalette(i, buffer.Faded, buffer.Size);
        }
        paletteData.FadedBuffers = paletteData.SelectedBuffers;
        if (paletteData.FadedBuffers == 0)
        {
            paletteData.SelectedFlag = 0;
        }
    }

    // OFFSET 0x0200384C
    public static ushort PaletteData_GetSelectedBuffersMask(PaletteData paletteData)
    {
        return paletteData.SelectedBuffers;
    }

    // OFFSET 0x02003858
    public static void PaletteData_SetAutoTransparent(PaletteData paletteData, int value)
    {
        paletteData.AutoTransparent = (value & 1) != 0;
    }

    // OFFSET 0x0200387C
    public static void PaletteData_SelectAll(PaletteData paletteData, byte value)
    {
        paletteData.SelectedFlag = value & 1;
        paletteData.SelectedBuffers = 0x3FFF;
    }

    // OFFSET 0x020038B0
    public static void PaletteData_FillBufferRange(PaletteData paletteData, int bufferID, int selector, ushort fillValue, ushort start, ushort end)
    {
        PaletteBuffer buffer = paletteData.Buffers[bufferID];
        if (end * 2 > buffer.Size)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (selector == 1 || selector == 2)
        {
            Array.Fill(buffer.Unfaded!, fillValue, start, end - start);
        }
        if (selector == 0 || selector == 2)
        {
            Array.Fill(buffer.Faded!, fillValue, start, end - start);
        }
    }

    // OFFSET 0x02003910
    public static ushort PaletteData_GetBufferIndexColor(PaletteData paletteData, int bufferID, int selector, ushort index)
    {
        if (selector == 1)
        {
            return paletteData.Buffers[bufferID].Unfaded![index];
        }
        if (selector == 0)
        {
            return paletteData.Buffers[bufferID].Faded![index];
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x0200393C
    public static void BlendPalette(ushort[] src, int srcOffset, ushort[] dest, int destOffset, ushort size, byte fraction, ushort target)
    {
        int targetRed = target & 0x1F;
        int targetGreen = (target >> 5) & 0x1F;
        int targetBlue = (target >> 10) & 0x1F;
        for (ushort i = 0; i < size; i++)
        {
            int color = src[srcOffset + i];
            int red = color & 0x1F;
            int green = (color >> 5) & 0x1F;
            int blue = (color >> 10) & 0x1F;
            int newBlue = blue + (((targetBlue - blue) * fraction) >> 4);
            int newRed = red + (((targetRed - red) * fraction) >> 4);
            int newGreen = green + (((targetGreen - green) * fraction) >> 4);
            dest[destOffset + i] = (ushort)((newBlue << 10) | (newGreen << 5) | newRed);
        }
    }

    // OFFSET 0x020039B0
    public static void PaletteData_Blend(PaletteData paletteData, int bufferID, ushort index, ushort size, byte fraction, ushort target)
    {
        PaletteBuffer buffer = paletteData.Buffers[bufferID];
        if (buffer.Unfaded == null || buffer.Faded == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        BlendPalette(buffer.Unfaded!, index, buffer.Faded!, index, size, fraction, target);
    }

    // OFFSET 0x020039F8
    public static void BlendPalettes(ushort[] sources, ushort[] dests, ushort toBlend, byte fraction, ushort target)
    {
        int offset = 0;
        while (toBlend != 0)
        {
            if ((toBlend & 1) != 0)
            {
                BlendPalette(sources, offset, dests, offset, 16, fraction, target);
            }
            toBlend >>= 1;
            offset += 16;
        }
    }

    // OFFSET 0x02003A2C
    public static void PaletteData_BlendMulti(PaletteData paletteData, int bufferID, ushort toBlend, byte fraction, ushort target)
    {
        PaletteBuffer buffer = paletteData.Buffers[bufferID];
        if (buffer.Unfaded == null || buffer.Faded == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        int index = 0;
        while (toBlend != 0)
        {
            if ((toBlend & 1) != 0)
            {
                PaletteData_Blend(paletteData, bufferID, (ushort)index, 16, fraction, target);
            }
            toBlend >>= 1;
            index += 16;
        }
    }

    // OFFSET 0x02003A8C
    public static void TintPalette(ushort[] palette, int numColorsToTint, int tintR, int tintG, int tintB)
    {
        for (int i = 0; i < numColorsToTint; i++)
        {
            int color = palette[i];
            int red = color & 0x1F;
            int green = (color >> 5) & 0x1F;
            int blue = (color >> 10) & 0x1F;
            int gray = (red * 0x4C + green * 0x97 + blue * 0x1D) >> 8;
            int newRed = (ushort)(tintR * gray) >> 8;
            int newGreen = (ushort)(tintG * gray) >> 8;
            int newBlue = (ushort)(tintB * gray) >> 8;
            if (newRed > 31)
            {
                newRed = 31;
            }
            if (newGreen > 31)
            {
                newGreen = 31;
            }
            if (newBlue > 31)
            {
                newBlue = 31;
            }
            palette[i] = (ushort)((newBlue << 10) | (newGreen << 5) | newRed);
        }
    }

    // OFFSET 0x02003B08
    public static void PaletteData_LoadBufferFromFileStartWithTint(PaletteData paletteData, int narcID, int narcMemberIdx, int heapID, int bufferID, uint size, ushort start, int r, int g, int b)
    {
        PaletteFile? file = Graphics.GetPlttFile(narcID, narcMemberIdx, heapID);
        if (file == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (size == 0)
        {
            size = file!.ByteSize;
        }
        TintPalette(file!.Colors, 16, r, g, b);
        PaletteData_LoadBuffer(paletteData, file.Colors, 0, bufferID, start, (ushort)size);
    }
}
