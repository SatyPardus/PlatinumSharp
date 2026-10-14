using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class VramTransferManager
{
    public uint Capacity;
    public uint Count;
    public VramTransferTask[] Tasks = Array.Empty<VramTransferTask>();
}

public static class VramTransfer
{
    private static VramTransferManager? sTransferManager;

    // OFFSET 0x0201DBEC
    public static void VramTransfer_New(uint capacity, int heapID)
    {
        if (sTransferManager != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sTransferManager = new VramTransferManager();
        sTransferManager.Tasks = new VramTransferTask[capacity];
        for (int i = 0; i < capacity; i++)
        {
            sTransferManager.Tasks[i] = new VramTransferTask();
        }
        sTransferManager.Capacity = capacity;
        sTransferManager.Count = 0;
        GfdVramTransfer.Init(sTransferManager.Tasks, sTransferManager.Capacity);
    }

    // OFFSET 0x0201DC3C
    public static void VramTransfer_Free()
    {
        if (sTransferManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sTransferManager = null;
    }

    // OFFSET 0x0201DC68
    public static bool VramTransfer_Request(int type, uint destAddr, Array buffer, uint size)
    {
        if (sTransferManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sTransferManager!.Count++;
        if (sTransferManager.Count >= sTransferManager.Capacity)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        return GfdVramTransfer.Register(type, destAddr, buffer, size);
    }

    // OFFSET 0x0201DCAC
    public static void VramTransfer_Process()
    {
        if (sTransferManager != null)
        {
            GfdVramTransfer.DoTransfer();
            sTransferManager.Count = 0;
        }
    }
}
