namespace PlatinumSharp.Game;

public static partial class NnsG2d
{
    public const int VramType3D = 0;
    public const int VramType2DMain = 1;
    public const int VramType2DSub = 2;
    public const int VramTypeCount = 3;
    public const uint VramAddrNone = 0xFFFFFFFF;

    private static readonly int[] sCellTransferTypes = { 0x00, 0x13, 0x23 };

    private static CellTransferRequestFunc? sCellTransferRequestFunc;
    private static int sCellTransferStateCount;
    private static CellTransferState[]? sCellTransferStates;

    // OFFSET 0x020A8AAC
    private static void VramLocation_Init(uint[] location)
    {
        for (int i = 0; i < VramTypeCount; i++)
        {
            location[i] = VramAddrNone;
        }
    }

    // OFFSET 0x020A8AC8
    private static void VramLocation_Set(uint[] location, int type, uint address)
    {
        location[type] = address;
    }

    // OFFSET 0x020A8AD0
    private static uint VramLocation_Get(uint[] location, int type)
    {
        return location[type];
    }

    // OFFSET 0x020AB164
    public static CellTransferState GetCellTransferState(uint handle)
    {
        return sCellTransferStates![handle];
    }

    private static void CellTransferState_Clear(CellTransferState state)
    {
        VramLocation_Init(state.VramLocation);
        state.DestMaxSize = 0;
        state.Source2D = null;
        state.Source3D = null;
        state.SourceSize = 0;
        state.Active = 0;
        state.DrawnFlags = 0;
        state.RequestedFlags = 0;
        state.SourceOffset = 0;
        state.SizeBytes = 0;
    }

    // OFFSET 0x020AB17C
    public static void InitCellTransferState(uint handle, uint dest3D, uint destMain, uint destSub, uint destMaxSize, byte[]? source2D, byte[]? source3D, uint sourceSize)
    {
        CellTransferState state = sCellTransferStates![handle];
        VramLocation_Init(state.VramLocation);
        if (dest3D != VramAddrNone)
        {
            VramLocation_Set(state.VramLocation, VramType3D, dest3D);
        }
        if (destMain != VramAddrNone)
        {
            VramLocation_Set(state.VramLocation, VramType2DMain, destMain);
        }
        if (destSub != VramAddrNone)
        {
            VramLocation_Set(state.VramLocation, VramType2DSub, destSub);
        }
        state.DestMaxSize = destMaxSize;
        state.Source2D = source2D;
        state.Source3D = source3D;
        state.SourceSize = sourceSize;
    }

    // OFFSET 0x020AB220
    public static void InitCellTransferStateManager(CellTransferState[] states, int count, CellTransferRequestFunc requestFunc)
    {
        sCellTransferRequestFunc = requestFunc;
        sCellTransferStateCount = count;
        sCellTransferStates = states;
        for (int i = 0; i < count; i++)
        {
            CellTransferState_Clear(states[i]);
        }
    }

    // OFFSET 0x020AB284
    public static void UpdateCellTransferStateManager()
    {
        for (int i = 0; i < sCellTransferStateCount; i++)
        {
            CellTransferState state = sCellTransferStates![i];
            if (state.Active == 0)
            {
                continue;
            }
            for (int type = 0; type < VramTypeCount; type++)
            {
                bool needed = false;
                if ((state.RequestedFlags & (1u << type)) != 0)
                {
                    if ((state.DrawnFlags & (1u << type)) != 0)
                    {
                        needed = true;
                    }
                }
                if (!needed)
                {
                    continue;
                }
                byte[]? source;
                if (type == VramType3D)
                {
                    source = state.Source3D;
                }
                else
                {
                    source = state.Source2D;
                }
                uint destination = VramLocation_Get(state.VramLocation, type);
                byte[] buffer = new byte[state.SizeBytes];
                if (source != null)
                {
                    int available = Math.Max(0, Math.Min(buffer.Length, source.Length - (int)state.SourceOffset));
                    Array.Copy(source, (int)state.SourceOffset, buffer, 0, available);
                }
                if (sCellTransferRequestFunc!(sCellTransferTypes[type], destination, buffer, state.SizeBytes))
                {
                    state.RequestedFlags &= ~(1u << type);
                }
            }
            state.DrawnFlags = 0;
        }
    }

    // OFFSET 0x020AB3A8
    public static void SetCellTransferStateRequested(uint handle, uint sourceOffset, uint size)
    {
        CellTransferState state = GetCellTransferState(handle);
        state.RequestedFlags = 0xFFFFFFFF;
        state.SourceOffset = sourceOffset;
        state.SizeBytes = size;
    }

    // OFFSET 0x020AB3CC
    public static uint GetNewCellTransferStateHandle()
    {
        for (int i = 0; i < sCellTransferStateCount; i++)
        {
            if (sCellTransferStates![i].Active != 1)
            {
                sCellTransferStates[i].Active = 1;
                return (uint)i;
            }
        }
        return 0xFFFFFFFF;
    }

    // OFFSET 0x020AB424
    public static void FreeCellTransferStateHandle(uint handle)
    {
        CellTransferState_Clear(sCellTransferStates![handle]);
    }
}
