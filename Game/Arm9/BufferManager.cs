using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class BufferManager
{
    public const int DmaType16Bit = 0;
    public const int DmaType32Bit = 1;

    public readonly Array?[] Buffers = new Array?[2];
    public byte WriteBuffer;
    public byte Mode;

    // OFFSET 0x02013B10
    public static BufferManager BufferManager_New(int heapID, Array buffer1, Array buffer2)
    {
        BufferManager bufferManager = new();
        bufferManager.Mode = 1;
        bufferManager.Buffers[0] = buffer1;
        bufferManager.Buffers[1] = buffer2;
        return bufferManager;
    }

    // OFFSET 0x02013B40
    public static void BufferManager_Delete(BufferManager? bufferManager)
    {
        if (bufferManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02013B54
    public static Array? BufferManager_GetWriteBuffer(BufferManager? bufferManager)
    {
        if (bufferManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return bufferManager!.Buffers[bufferManager.WriteBuffer];
    }

    // OFFSET 0x02013B68
    public static Array? BufferManager_GetReadBuffer(BufferManager? bufferManager)
    {
        if (bufferManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return bufferManager!.Buffers[bufferManager.WriteBuffer ^ 1];
    }

    // OFFSET 0x02013B80
    public static void BufferManager_SetMode(BufferManager? bufferManager, byte mode)
    {
        if (bufferManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        bufferManager!.Mode = mode;
    }

    // OFFSET 0x02013B94
    public static void BufferManager_SwapBuffers(BufferManager? bufferManager)
    {
        if (bufferManager != null && bufferManager.Mode != 0)
        {
            bufferManager.WriteBuffer ^= 1;
        }
    }

    // OFFSET 0x02013BA8
    public static void BufferManager_StopDMA()
    {
        Dma.Wait(0);
        Dma.Stop(0);
    }

    // OFFSET 0x02013BB8
    public static void BufferManager_StartDMA(Array source, uint destination, int size, int dmaType)
    {
        if (dmaType == DmaType32Bit)
        {
            Dma.StartHBlankCopy32(0, source, destination, size);
        }
        else
        {
            Dma.StartHBlankCopy16(0, source, destination, size);
        }
    }
}
