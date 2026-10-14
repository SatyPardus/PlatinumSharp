namespace PlatinumSharp.Game;

public struct TouchScreenButtonStatus
{
    public byte State;
    public byte Touched;
    public ushort HeldFrames;
}

public sealed class TouchScreenActions
{
    private const int StateIdle = 0;
    private const int StateHeld = 1;
    private const ushort MaxHeldFrames = 0xFFFF;

    public TouchScreenHitTable[]? HitTable;
    public uint HitTableLength;
    public TouchScreenHitTableAction? Action;
    public object? Context;
    public TouchScreenButtonStatus[]? Status;

    private delegate int ButtonStateFunc(ref TouchScreenButtonStatus status, bool touched, bool tapped);

    private static readonly ButtonStateFunc[] sButtonStateFuncs = { TouchScreenButton_UpdateIdle, TouchScreenButton_UpdateHeld };

    // OFFSET 0x02023FCC
    public static TouchScreenActions TouchScreenActions_RegisterHandler(TouchScreenHitTable[] hitTable, uint hitTableLength, TouchScreenHitTableAction action, object? context, int heapID)
    {
        if (hitTableLength == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        TouchScreenActions actions = new();
        actions.HitTable = hitTable;
        actions.HitTableLength = hitTableLength;
        actions.Action = action;
        actions.Context = context;
        actions.Status = new TouchScreenButtonStatus[hitTableLength];
        for (uint i = 0; i < hitTableLength; i++)
        {
            TouchScreenButton_Init(ref actions.Status[i]);
        }
        return actions;
    }

    // OFFSET 0x02024028
    private static void TouchScreenButton_Init(ref TouchScreenButtonStatus status)
    {
        status.State = StateIdle;
        status.Touched = 0;
        status.HeldFrames = 0;
    }

    // OFFSET 0x02024034
    public static void TouchScreenActions_Free(TouchScreenActions? actions)
    {
        if (actions == null)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x0202404C
    public static void TouchScreenActions_HandleAction(TouchScreenActions actions)
    {
        bool touched = TouchScreen.TouchScreen_Touched();
        bool tapped;
        if (touched)
        {
            tapped = TouchScreen.TouchScreen_Tapped();
            for (uint i = 0; i < actions.HitTableLength; i++)
            {
                bool hit;
                if (actions.Status![i].Touched != 0)
                {
                    hit = TouchScreen.TouchScreen_LocationHeld(actions.HitTable![i]);
                }
                else
                {
                    hit = TouchScreen.TouchScreen_LocationPressed(actions.HitTable![i]);
                }
                actions.Status[i].Touched = (byte)(hit ? 1 : 0);
            }
        }
        else
        {
            tapped = false;
            for (uint i = 0; i < actions.HitTableLength; i++)
            {
                actions.Status![i].Touched = 0;
            }
        }
        for (uint i = 0; i < actions.HitTableLength; i++)
        {
            int state = sButtonStateFuncs[actions.Status![i].State](ref actions.Status[i], touched, tapped);
            if (state != TouchScreen.ButtonNotPressed)
            {
                actions.Action!(i, state, actions.Context);
            }
        }
    }

    // OFFSET 0x020240F4
    private static int TouchScreenButton_UpdateIdle(ref TouchScreenButtonStatus status, bool touched, bool tapped)
    {
        if (status.Touched != 0 && tapped)
        {
            TouchScreenButton_SetState(ref status, StateHeld);
            return TouchScreen.ButtonPressed;
        }
        return TouchScreen.ButtonNotPressed;
    }

    // OFFSET 0x02024110
    private static int TouchScreenButton_UpdateHeld(ref TouchScreenButtonStatus status, bool touched, bool tapped)
    {
        if (status.Touched != 0)
        {
            if (status.HeldFrames < MaxHeldFrames)
            {
                status.HeldFrames++;
            }
            return TouchScreen.ButtonHeld;
        }
        if (touched)
        {
            TouchScreenButton_SetState(ref status, StateIdle);
            return TouchScreen.ButtonHeldOutOfBounds;
        }
        TouchScreenButton_SetState(ref status, StateIdle);
        return TouchScreen.ButtonReleased;
    }

    // OFFSET 0x02024144
    private static void TouchScreenButton_SetState(ref TouchScreenButtonStatus status, byte state)
    {
        status.State = state;
        status.HeldFrames = 0;
    }
}

public sealed class BillboardGfxSequence
{
    public ushort[] FrameStarts = Array.Empty<ushort>();
    public byte[] TextureIndices = Array.Empty<byte>();
    public byte[] PaletteIndices = Array.Empty<byte>();
    public uint FrameCount;

    // OFFSET 0x0202414C
    public static ushort BillboardGfxSequence_GetTexPlttIndexAt(BillboardGfxSequence gfxSequence, ushort frame)
    {
        uint index = 0;
        while (index < gfxSequence.FrameCount - 1)
        {
            if (gfxSequence.FrameStarts[index + 1] > frame)
            {
                break;
            }
            index++;
        }
        byte textureIndex = gfxSequence.TextureIndices[index];
        byte paletteIndex = gfxSequence.PaletteIndices[index];
        return (ushort)(textureIndex | (paletteIndex << 8));
    }

    // OFFSET 0x02024184
    public static void BillboardGfxSequence_SetData(byte[] data, int offset, BillboardGfxSequence gfxSequence)
    {
        gfxSequence.FrameCount = BitConverter.ToUInt32(data, offset);
        int count = (int)gfxSequence.FrameCount;
        int frames = offset + 4;
        gfxSequence.FrameStarts = new ushort[count];
        for (int i = 0; i < count; i++)
        {
            gfxSequence.FrameStarts[i] = BitConverter.ToUInt16(data, frames + i * 2);
        }
        int textures = frames + count * 2;
        gfxSequence.TextureIndices = new byte[count];
        Array.Copy(data, textures, gfxSequence.TextureIndices, 0, count);
        gfxSequence.PaletteIndices = new byte[count];
        Array.Copy(data, textures + count, gfxSequence.PaletteIndices, 0, count);
    }
}

public static class G3SwapRequest
{
    private static int sPending;
    private static int sSortMode;
    private static int sBufferMode;

    // OFFSET 0x0202419C
    public static void G3_InitSwapRequest()
    {
        sPending = 0;
        sSortMode = 0;
        sBufferMode = 0;
        sPending = 0;
    }

    // OFFSET 0x020241B4
    public static void G3_ResetG3X()
    {
        Render3D.Reset();
    }

    // OFFSET 0x020241BC
    public static void G3_RequestSwapBuffers(int sortMode, int bufferMode)
    {
        sSortMode = sortMode;
        sBufferMode = bufferMode;
        sPending = 1;
    }

    // OFFSET 0x020241CC
    public static void G3_ProcessSwapRequest()
    {
        if (sPending != 0)
        {
            Render3D.SwapBuffers(sSortMode, sBufferMode);
            sPending = 0;
        }
    }
}

public static class HardwarePalette
{
    private const uint StandardPaletteSize = 0x200;

    // OFFSET 0x020241F0
    public static ushort[] GetHardwareMainBgPaletteAddress()
    {
        return Video.GetPalette(Video.MainBackground);
    }

    // OFFSET 0x020241F8
    public static uint GetHardwareMainBgPaletteSize()
    {
        return StandardPaletteSize;
    }

    // OFFSET 0x02024200
    public static ushort[] GetHardwareSubBgPaletteAddress()
    {
        return Video.GetPalette(Video.SubBackground);
    }

    // OFFSET 0x02024208
    public static uint GetHardwareSubBgPaletteSize()
    {
        return StandardPaletteSize;
    }

    // OFFSET 0x02024210
    public static ushort[] GetHardwareMainObjPaletteAddress()
    {
        return Video.GetPalette(Video.MainObject);
    }

    // OFFSET 0x02024218
    public static ushort[] GetHardwareSubObjPaletteAddress()
    {
        return Video.GetPalette(Video.SubObject);
    }
}
