using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class ScreenScrollManager
{
    public const int BufferSize = 0xC0;

    private const int ScrollPosScale = 100;
    private const int ScrollPosLimit = BufferSize * ScrollPosScale;
    private const int ScrollYPhase = 0xB4;

    public readonly uint[] Buffer1 = new uint[BufferSize];
    public readonly uint[] Buffer2 = new uint[BufferSize];
    public BufferManager? BufferManager;
    public SysTask? ScrollTask;
    public int Unused;
    public readonly short[] SineTable = new short[BufferSize];
    public byte Start;
    public byte End;
    public uint OffsetRegisterAddr;
    public uint InitValue;
    public short ScrollPos;
    public short ScrollSpeed;

    // OFFSET 0x02013BE0
    public static ScreenScrollManager ScreenScrollManager_New(int heapID)
    {
        ScreenScrollManager screenScrollMgr = new();
        screenScrollMgr.BufferManager = BufferManager.BufferManager_New(heapID, screenScrollMgr.Buffer1, screenScrollMgr.Buffer2);
        return screenScrollMgr;
    }

    // OFFSET 0x02013C10
    public static void ScreenScrollManager_ScrollX(ScreenScrollManager? screenScrollMgr, byte start, byte end, ushort angleIncrement, int amplitude, short speed, uint offsetRegisterAddr, uint initValue, uint priority)
    {
        if (screenScrollMgr == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (screenScrollMgr!.ScrollTask != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        screenScrollMgr.Start = start;
        screenScrollMgr.End = end;
        screenScrollMgr.ScrollPos = 0;
        screenScrollMgr.ScrollSpeed = speed;
        screenScrollMgr.OffsetRegisterAddr = offsetRegisterAddr;
        screenScrollMgr.InitValue = initValue;
        ScreenScrollManager_CreateSineTable(screenScrollMgr.SineTable, BufferSize, angleIncrement, amplitude);
        screenScrollMgr.ScrollTask = SysTask.SysTask_Start(ScreenScrollManager_ScrollXTask, screenScrollMgr, priority);
        Array.Fill(screenScrollMgr.Buffer1, screenScrollMgr.InitValue);
        Array.Fill(screenScrollMgr.Buffer2, screenScrollMgr.InitValue);
    }

    // OFFSET 0x02013CA4
    public static void ScreenScrollManager_ScrollY(ScreenScrollManager? screenScrollMgr, byte start, byte end, ushort angleIncrement, int amplitude, short speed, uint offsetRegisterAddr, uint initValue, uint priority)
    {
        if (screenScrollMgr == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (screenScrollMgr!.ScrollTask != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        screenScrollMgr.Start = start;
        screenScrollMgr.End = end;
        screenScrollMgr.ScrollPos = 0;
        screenScrollMgr.ScrollSpeed = speed;
        screenScrollMgr.OffsetRegisterAddr = offsetRegisterAddr;
        screenScrollMgr.InitValue = initValue;
        ScreenScrollManager_CreateSineTable(screenScrollMgr.SineTable, BufferSize, angleIncrement, amplitude);
        screenScrollMgr.ScrollTask = SysTask.SysTask_Start(ScreenScrollManager_ScrollYTask, screenScrollMgr, priority);
        Array.Fill(screenScrollMgr.Buffer1, screenScrollMgr.InitValue);
        Array.Fill(screenScrollMgr.Buffer2, screenScrollMgr.InitValue);
    }

    // OFFSET 0x02013D38
    public static void ScreenScrollManager_Stop(ScreenScrollManager? screenScrollMgr)
    {
        if (screenScrollMgr == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (screenScrollMgr!.ScrollTask != null)
        {
            SysTask.SysTask_Done(screenScrollMgr.ScrollTask);
            screenScrollMgr.ScrollTask = null;
            uint[] buffer = (uint[])BufferManager.BufferManager_GetWriteBuffer(screenScrollMgr.BufferManager)!;
            Array.Fill(buffer, (byte)screenScrollMgr.InitValue * 0x01010101u);
        }
    }

    // OFFSET 0x02013D74
    public static void ScreenScrollManager_Delete(ScreenScrollManager? screenScrollMgr)
    {
        ScreenScrollManager_Stop(screenScrollMgr);
        ScreenScrollManager_StopDMA();
        BufferManager.BufferManager_Delete(screenScrollMgr!.BufferManager);
    }

    // OFFSET 0x02013D94
    public static uint[] ScreenScrollManager_GetWriteBuffer(ScreenScrollManager screenScrollMgr)
    {
        return (uint[])BufferManager.BufferManager_GetWriteBuffer(screenScrollMgr.BufferManager)!;
    }

    // OFFSET 0x02013DA4
    public static void ScreenScrollManager_SwapBuffers(ScreenScrollManager? screenScrollMgr)
    {
        if (screenScrollMgr == null || screenScrollMgr.ScrollTask == null)
        {
            return;
        }
        BufferManager.BufferManager_SwapBuffers(screenScrollMgr.BufferManager);
        uint[] buffer = (uint[])BufferManager.BufferManager_GetWriteBuffer(screenScrollMgr.BufferManager)!;
        Array.Fill(buffer, screenScrollMgr.InitValue);
    }

    // OFFSET 0x02013DDC
    public static void ScreenScrollManager_RestartDMA(ScreenScrollManager? screenScrollMgr)
    {
        if (screenScrollMgr == null || screenScrollMgr.ScrollTask == null)
        {
            return;
        }
        ScreenScrollManager_StopDMA();
        ScreenScrollManager_StartDMA(screenScrollMgr);
    }

    // OFFSET 0x02013DFC
    public static void ScreenScrollManager_CreateSineTable(short[] dest, uint entryCount, ushort angleIncrement, int amplitude)
    {
        ushort angle = 0;
        for (uint i = 0; i < entryCount; i++)
        {
            int value = (int)(((long)Render3D.Sin(angle >> 4) * amplitude + 0x800) >> Fx32.Shift);
            dest[i] = (short)(value >> Fx32.Shift);
            angle += angleIncrement;
        }
    }

    // OFFSET 0x02013E58
    private static void ScreenScrollManager_ScrollXTask(SysTask task, object? param)
    {
        ScreenScrollManager screenScrollMgr = (ScreenScrollManager)param!;
        uint[] buffer = (uint[])BufferManager.BufferManager_GetWriteBuffer(screenScrollMgr.BufferManager)!;
        byte sineIndex = (byte)(screenScrollMgr.ScrollPos / ScrollPosScale);
        for (int line = screenScrollMgr.Start; line <= screenScrollMgr.End; line++)
        {
            uint value = buffer[line];
            short x = (short)((short)value + screenScrollMgr.SineTable[sineIndex]);
            ushort y = (ushort)(value >> 16);
            buffer[line] = ((uint)y << 16) | (ushort)x;
            sineIndex = (byte)((sineIndex + 1) % BufferSize);
        }
        ScreenScrollManager_AdvanceScrollPos(screenScrollMgr);
    }

    // OFFSET 0x02013F04
    private static void ScreenScrollManager_ScrollYTask(SysTask task, object? param)
    {
        ScreenScrollManager screenScrollMgr = (ScreenScrollManager)param!;
        uint[] buffer = (uint[])BufferManager.BufferManager_GetWriteBuffer(screenScrollMgr.BufferManager)!;
        byte sineIndex = (byte)(screenScrollMgr.ScrollPos / ScrollPosScale);
        for (int line = screenScrollMgr.Start; line <= screenScrollMgr.End; line++)
        {
            uint value = buffer[line];
            ushort y = (ushort)((ushort)(value >> 16) + screenScrollMgr.SineTable[(sineIndex + ScrollYPhase) % BufferSize]);
            short x = (short)value;
            buffer[line] = ((uint)y << 16) | (ushort)x;
            sineIndex = (byte)((sineIndex + 1) % BufferSize);
        }
        ScreenScrollManager_AdvanceScrollPos(screenScrollMgr);
    }

    private static void ScreenScrollManager_AdvanceScrollPos(ScreenScrollManager screenScrollMgr)
    {
        screenScrollMgr.ScrollPos += screenScrollMgr.ScrollSpeed;
        if (screenScrollMgr.ScrollPos >= ScrollPosLimit)
        {
            screenScrollMgr.ScrollPos = (short)(screenScrollMgr.ScrollPos % ScrollPosLimit);
        }
        else if (screenScrollMgr.ScrollPos < 0)
        {
            screenScrollMgr.ScrollPos += ScrollPosLimit;
        }
    }

    // OFFSET 0x02013FC8
    private static void ScreenScrollManager_StopDMA()
    {
        BufferManager.BufferManager_StopDMA();
    }

    // OFFSET 0x02013FD0
    private static void ScreenScrollManager_StartDMA(ScreenScrollManager? screenScrollMgr)
    {
        if (screenScrollMgr == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        uint[] buffer = (uint[])BufferManager.BufferManager_GetReadBuffer(screenScrollMgr!.BufferManager)!;
        BufferManager.BufferManager_StartDMA(buffer, screenScrollMgr.OffsetRegisterAddr, 4, BufferManager.DmaType32Bit);
    }
}
