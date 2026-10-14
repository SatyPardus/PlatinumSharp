using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class CellTransferState
{
    public readonly uint[] VramLocation = new uint[3];
    public uint DestMaxSize;
    public byte[]? Source2D;
    public byte[]? Source3D;
    public uint SourceSize;
    public int Active;
    public uint DrawnFlags;
    public uint RequestedFlags;
    public uint SourceOffset;
    public uint SizeBytes;
}

public delegate bool CellTransferRequestFunc(int type, uint destAddr, Array buffer, uint size);

public static class CellTransfer
{
    // OFFSET 0x0201DCC8
    public static CellTransferState[] CellTransfer_New(int capacity, int heapID)
    {
        CellTransferState[] transferStates = new CellTransferState[capacity];
        for (int i = 0; i < capacity; i++)
        {
            transferStates[i] = new CellTransferState();
        }
        NnsG2d.InitCellTransferStateManager(transferStates, capacity, CellTransfer_RequestTransfer);
        return transferStates;
    }

    // OFFSET 0x0201DCE8
    public static void CellTransfer_Update()
    {
        NnsG2d.UpdateCellTransferStateManager();
    }

    // OFFSET 0x0201DCF0
    public static void CellTransfer_Free(CellTransferState[]? transferStates)
    {
    }

    // OFFSET 0x0201DCF8
    private static bool CellTransfer_RequestTransfer(int type, uint destAddr, Array buffer, uint size)
    {
        return VramTransfer.VramTransfer_Request(type, destAddr, buffer, size);
    }
}
