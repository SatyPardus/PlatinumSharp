using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class CharTransferAllocation
{
    public int Size;
    public int Offset;
    public ushort VramType;
    public ushort AtEnd;
}

public sealed class CharTransferTask
{
    public CharacterData? Data;
    public int VramType;
    public byte IsVramTransfer;
    public int ResourceID;
    public ImageProxy ImageProxy = new();
    public int BaseAddrMain;
    public int BaseAddrSub;
    public byte State;
    public int UseHardwareMappingType;
    public int AtEnd;
    public int HasRange;
    public int RangeSizeMain;
    public int RangeSizeSub;
}

public sealed class CharTransferTaskManager
{
    public CharTransferTask[] Tasks = Array.Empty<CharTransferTask>();
    public int Capacity;
    public int Count;
    public int OffsetMain;
    public int OffsetSub;
    public int VramSizeMain;
    public int VramSizeSub;
    public int RangeBaseMain;
    public int RangeBaseSub;
    public int NumBlocksMain;
    public int NumBlocksSub;
    public int BlockSizeMain;
    public int BlockSizeSub;
    public byte[]? BlockBufMain;
    public byte[]? BlockBufSub;
}

public static class CharTransfer
{
    private const int DisplayMain = 0;
    private const int DisplaySub = 1;
    private const uint ObjMappingMask = 0x00300010;
    private const uint ObjMappingClearMask = 0xFFCFFFEF;
    private const int StateUnused = 0;
    private const int StateInitialized = 1;
    private const int StateVramTransfer = 2;
    private const int StateHasRange = 3;
    private const int StateCopy = 4;
    private const int TransferMainObject = 0x13;
    private const int TransferSubObject = 0x23;
    private const int NoBlock = -1;

    private static CharTransferTaskManager? sTaskManager;

    // OFFSET 0x0201E86C
    public static void CharTransfer_Init(CharTransferTemplate template)
    {
        uint modeSub = Video.GetDisplayControl(DisplaySub) & ObjMappingMask;
        uint modeMain = Video.GetDisplayControl(DisplayMain) & ObjMappingMask;
        CharTransfer_InitWithVramModes(template, (int)modeMain, (int)modeSub);
    }

    // OFFSET 0x0201E88C
    public static void CharTransfer_InitWithVramModes(CharTransferTemplate template, int modeMain, int modeSub)
    {
        if (sTaskManager != null)
        {
            return;
        }
        sTaskManager = new CharTransferTaskManager();
        sTaskManager.Capacity = template.MaxTasks;
        sTaskManager.Tasks = new CharTransferTask[sTaskManager.Capacity];
        for (int i = 0; i < template.MaxTasks; i++)
        {
            sTaskManager.Tasks[i] = new CharTransferTask();
            CharTransferTask_Init(sTaskManager.Tasks[i]);
        }
        sTaskManager.BlockSizeMain = CharTransfer_GetBlockSize((uint)modeMain);
        sTaskManager.BlockSizeSub = CharTransfer_GetBlockSize((uint)modeSub);
        Video.SetDisplayControl(DisplayMain, (uint)modeMain | (Video.GetDisplayControl(DisplayMain) & ObjMappingClearMask));
        Video.SetDisplayControl(DisplaySub, (uint)modeSub | (Video.GetDisplayControl(DisplaySub) & ObjMappingClearMask));
        int numBlocksMain = CharTransfer_BytesToBlocks(template.SizeMain, sTaskManager.BlockSizeMain);
        int numBlocksSub = CharTransfer_BytesToBlocks(template.SizeSub, sTaskManager.BlockSizeSub);
        CharTransfer_AllocBlockBuffers(numBlocksMain, numBlocksSub, template.HeapID);
    }

    // OFFSET 0x0201E958
    public static void CharTransfer_Free()
    {
        if (sTaskManager == null)
        {
            return;
        }
        CharTransfer_FreeBlockBuffer(sTaskManager.BlockBufMain);
        CharTransfer_FreeBlockBuffer(sTaskManager.BlockBufSub);
        CharTransfer_ResetAllTasks();
        sTaskManager = null;
    }

    // OFFSET 0x0201E994
    public static void CharTransfer_ClearBuffers()
    {
        sTaskManager!.OffsetMain = 0;
        sTaskManager.OffsetSub = 0;
        CharTransfer_ClearBlockBuffer(sTaskManager.BlockBufMain);
        CharTransfer_ClearBlockBuffer(sTaskManager.BlockBufSub);
        CharTransfer_UpdateVramCapacities();
    }

    // OFFSET 0x0201E9C0
    public static void CharTransfer_ReserveVramRange(uint offset, uint size, int vramType)
    {
        if (vramType == G2d.VramMain)
        {
            CharTransfer_ClampRangeToBase(sTaskManager!.RangeBaseMain, (int)offset, (int)size, out int rangeOffset, out int rangeSize);
            if (rangeSize > 0)
            {
                CharTransfer_ReserveBlocks(G2d.VramMain, rangeOffset, 0, rangeSize, 0);
            }
        }
        else
        {
            CharTransfer_ClampRangeToBase(sTaskManager!.RangeBaseSub, (int)offset, (int)size, out int rangeOffset, out int rangeSize);
            if (rangeSize > 0)
            {
                CharTransfer_ReserveBlocks(G2d.VramSub, 0, rangeOffset, 0, rangeSize);
            }
        }
    }

    // OFFSET 0x0201EA24
    public static bool CharTransfer_Request(CharTransferTaskTemplate template)
    {
        if (CharTransfer_HasTask(template.ResourceID))
        {
            ErrorHandling.GF_AssertFail();
        }
        CharTransferTask? task = CharTransfer_FindUnusedTask();
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        if (!CharTransferTask_InitFromTemplate(template, task))
        {
            return false;
        }
        if (!CharTransferTask_ReserveAndTransfer(task))
        {
            CharTransfer_ResetTask(task.ResourceID);
            return false;
        }
        sTaskManager!.Count++;
        return true;
    }

    // OFFSET 0x0201EA7C
    public static bool CharTransfer_RequestWithHardwareMappingType(CharTransferTaskTemplate template)
    {
        if (CharTransfer_HasTask(template.ResourceID))
        {
            ErrorHandling.GF_AssertFail();
        }
        CharTransferTask? task = CharTransfer_FindUnusedTask();
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        if (!CharTransferTask_InitFromTemplate(template, task))
        {
            return false;
        }
        task.UseHardwareMappingType = 1;
        if (!CharTransferTask_ReserveAndTransfer(task))
        {
            CharTransfer_ResetTask(task.ResourceID);
            return false;
        }
        sTaskManager!.Count++;
        return true;
    }

    // OFFSET 0x0201EAD8
    public static bool CharTransfer_HasTask(int resourceID)
    {
        for (int i = 0; i < sTaskManager!.Capacity; i++)
        {
            if (sTaskManager.Tasks[i].ResourceID == resourceID)
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0201EB08
    public static void CharTransfer_ReplaceCharData(int resourceID, CharacterData? data)
    {
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        CharTransferTask? task = CharTransfer_FindTaskByResourceID(resourceID);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        task!.Data = data;
        if ((task.VramType & G2d.VramMain) != 0)
        {
            VramTransfer.VramTransfer_Request(TransferMainObject, (uint)task.BaseAddrMain, data!.RawData, data.SizeBytes);
        }
        if ((task.VramType & G2d.VramSub) != 0)
        {
            VramTransfer.VramTransfer_Request(TransferSubObject, (uint)task.BaseAddrSub, data!.RawData, data.SizeBytes);
        }
    }

    // OFFSET 0x0201EB50
    public static void CharTransfer_ResetTask(int resourceID)
    {
        bool isCopy = true;
        CharTransferTask? task;
        do
        {
            task = CharTransfer_FindTaskByResourceID(resourceID);
            if (task == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            if (task!.State == StateCopy)
            {
                CharTransfer_DeleteTask(task.ImageProxy);
            }
            else
            {
                isCopy = false;
            }
        } while (isCopy);
        if (task.State != StateUnused)
        {
            CharTransferTask_Reset(task);
            sTaskManager!.Count--;
        }
    }

    // OFFSET 0x0201EBA0
    public static void CharTransfer_ResetAllTasks()
    {
        for (int i = 0; i < sTaskManager!.Capacity; i++)
        {
            if (sTaskManager.Tasks[i].State != StateUnused)
            {
                CharTransferTask_Reset(sTaskManager.Tasks[i]);
                sTaskManager.Count--;
            }
        }
    }

    // OFFSET 0x0201EBDC
    public static ImageProxy? CharTransfer_GetImageProxy(int resourceID)
    {
        CharTransferTask? task = CharTransfer_FindTaskByResourceID(resourceID);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (task!.State == StateUnused)
        {
            return null;
        }
        return task.ImageProxy;
    }

    // OFFSET 0x0201EC00
    public static ImageProxy? CharTransfer_ResizeTaskRange(int resourceID, uint size)
    {
        CharTransferTask? task = CharTransfer_FindTaskByResourceID(resourceID);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (task!.State == StateUnused)
        {
            return null;
        }
        CharTransfer_FindAvailableRange(task.VramType, out int offsetMain, out int offsetSub, (int)size, out int sizeMain, out int sizeSub);
        if (task.State == StateHasRange)
        {
            return null;
        }
        task.State = StateHasRange;
        CharTransferTask_SetBaseAddrsFromRange(task, offsetMain, offsetSub);
        task.HasRange = 1;
        task.RangeSizeMain = sizeMain;
        task.RangeSizeSub = sizeSub;
        CharTransferTask_LoadVramTransfer(task);
        CharTransfer_ReserveBlocks(task.VramType, offsetMain, offsetSub, sizeMain, sizeSub);
        return task.ImageProxy;
    }

    // OFFSET 0x0201EC84
    public static ImageProxy? CharTransfer_CopyTask(ImageProxy? imageProxy)
    {
        CharTransferTask? source = CharTransfer_FindTaskByImageProxy(imageProxy);
        if (source == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        CharTransferTask? copy = CharTransfer_FindUnusedTask();
        if (copy == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (source!.State != StateHasRange)
        {
            return null;
        }
        CharTransferTask_Copy(copy!, source);
        copy!.State = StateCopy;
        int size;
        if ((copy.VramType & G2d.VramMain) != 0)
        {
            size = copy.RangeSizeMain;
        }
        else
        {
            size = copy.RangeSizeSub;
        }
        CharTransfer_FindAvailableRange(copy.VramType, out int offsetMain, out int offsetSub, size, out int sizeMain, out int sizeSub);
        CharTransferTask_SetBaseAddrsFromRange(copy, offsetMain, offsetSub);
        copy.HasRange = 1;
        copy.RangeSizeMain = sizeMain;
        copy.RangeSizeSub = sizeSub;
        CharTransferTask_LoadVramTransfer(copy);
        CharTransfer_ReserveBlocks(copy.VramType, offsetMain, offsetSub, sizeMain, sizeSub);
        return copy.ImageProxy;
    }

    // OFFSET 0x0201ED1C
    public static void CharTransfer_DeleteTask(ImageProxy imageProxy)
    {
        int i;
        for (i = 0; i < sTaskManager!.Capacity; i++)
        {
            CharTransferTask candidate = sTaskManager.Tasks[i];
            if ((candidate.State == StateHasRange || candidate.State == StateCopy) && ReferenceEquals(candidate.ImageProxy, imageProxy))
            {
                break;
            }
        }
        if (i >= sTaskManager.Capacity)
        {
            return;
        }
        CharTransferTask task = sTaskManager.Tasks[i];
        CharTransferTask_ClearRange(task);
        if (task.State == StateHasRange)
        {
            task.State = StateVramTransfer;
            return;
        }
        task.State = StateUnused;
        CharTransferTask_Init(task);
    }

    // OFFSET 0x0201ED94
    public static bool CharTransfer_AllocRange(int size, bool atEnd, int vramType, CharTransferAllocation allocation)
    {
        bool result;
        if (!atEnd)
        {
            result = CharTransfer_TryGetDestOffsets(size, vramType, out int offsetMain, out int offsetSub);
            if (result)
            {
                CharTransfer_UpdateVramOffsets(size, vramType);
                allocation.VramType = (ushort)vramType;
                allocation.Size = size;
                if (vramType == G2d.VramMain)
                {
                    allocation.Offset = offsetMain;
                }
                else
                {
                    allocation.Offset = offsetSub;
                }
                allocation.AtEnd = 0;
            }
        }
        else
        {
            result = CharTransfer_FindAvailableRange(vramType, out int offsetMain, out int offsetSub, size, out int sizeMain, out int sizeSub);
            if (result)
            {
                CharTransfer_ReserveBlocks(vramType, offsetMain, offsetSub, sizeMain, sizeSub);
                allocation.VramType = (ushort)vramType;
                if (vramType == G2d.VramMain)
                {
                    allocation.Size = sizeMain;
                    allocation.Offset = offsetMain + sTaskManager!.RangeBaseMain;
                }
                else
                {
                    allocation.Size = sizeSub;
                    allocation.Offset = offsetSub + sTaskManager!.RangeBaseSub;
                }
                allocation.AtEnd = 1;
            }
        }
        return result;
    }

    // OFFSET 0x0201EE28
    public static void CharTransfer_ClearRange(CharTransferAllocation allocation)
    {
        if (allocation.AtEnd == 0)
        {
            return;
        }
        if ((allocation.VramType & G2d.VramMain) != 0)
        {
            int blocks = CharTransfer_BytesToBlocks(allocation.Size, sTaskManager!.BlockSizeMain);
            int start = CharTransfer_BytesToBlocks(allocation.Offset - sTaskManager.RangeBaseMain, sTaskManager.BlockSizeMain);
            CharTransfer_ClearBlockRange((uint)start, (uint)blocks, sTaskManager.BlockBufMain);
        }
        if ((allocation.VramType & G2d.VramSub) != 0)
        {
            int blocks = CharTransfer_BytesToBlocks(allocation.Size, sTaskManager!.BlockSizeSub);
            int start = CharTransfer_BytesToBlocks(allocation.Offset - sTaskManager.RangeBaseSub, sTaskManager.BlockSizeSub);
            CharTransfer_ClearBlockRange((uint)start, (uint)blocks, sTaskManager.BlockBufSub);
        }
    }

    // OFFSET 0x0201EE9C
    public static CharTransferTaskManager? CharTransfer_PopTaskManager()
    {
        if (sTaskManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        CharTransferTaskManager? manager = sTaskManager;
        sTaskManager = null;
        return manager;
    }

    // OFFSET 0x0201EEB8
    public static void CharTransfer_PushTaskManager(CharTransferTaskManager? manager)
    {
        if (sTaskManager != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sTaskManager = manager;
    }

    // OFFSET 0x0201EED4
    private static void CharTransferTask_Init(CharTransferTask task)
    {
        task.Data = null;
        task.VramType = 0;
        task.IsVramTransfer = 0;
        task.ResourceID = -1;
        task.BaseAddrMain = 0;
        task.BaseAddrSub = 0;
        task.State = StateUnused;
        task.UseHardwareMappingType = 0;
        task.ImageProxy.Init();
    }

    // OFFSET 0x0201EEF8
    private static bool CharTransferTask_InitFromTemplate(CharTransferTaskTemplate template, CharTransferTask task)
    {
        task.Data = template.Data;
        task.ResourceID = template.ResourceID;
        task.VramType = template.VramType;
        task.IsVramTransfer = (byte)(task.Data!.CharacterFormat >> 8);
        task.AtEnd = template.AtEnd;
        task.HasRange = 0;
        task.RangeSizeMain = 0;
        task.RangeSizeSub = 0;
        return true;
    }

    // OFFSET 0x0201EF1C
    private static CharTransferTask? CharTransfer_FindTaskByImageProxy(ImageProxy? imageProxy)
    {
        for (int i = 0; i < sTaskManager!.Capacity; i++)
        {
            if (sTaskManager.Tasks[i].State != StateUnused && ReferenceEquals(sTaskManager.Tasks[i].ImageProxy, imageProxy))
            {
                return sTaskManager.Tasks[i];
            }
        }
        return null;
    }

    // OFFSET 0x0201EF68
    private static bool CharTransferTask_ReserveAndTransfer(CharTransferTask task)
    {
        bool result = true;
        if (task.IsVramTransfer != 0)
        {
            task.State = StateVramTransfer;
        }
        else
        {
            task.State = StateInitialized;
            if (task.AtEnd == 0)
            {
                result = CharTransferTask_ReserveAndTransferFromStart(task);
            }
            else
            {
                result = CharTransferTask_ReserveAndTransferFromEnd(task);
            }
        }
        return result;
    }

    // OFFSET 0x0201EF98
    private static bool CharTransferTask_ReserveAndTransferFromStart(CharTransferTask task)
    {
        bool result = CharTransfer_TryGetDestOffsets((int)task.Data!.SizeBytes, task.VramType, out int offsetMain, out int offsetSub);
        if (result)
        {
            CharTransferTask_SetBaseAddrs(task, offsetMain, offsetSub);
            CharTransfer_UpdateVramOffsets((int)task.Data.SizeBytes, task.VramType);
        }
        return result;
    }

    // OFFSET 0x0201EFCC
    private static bool CharTransferTask_ReserveAndTransferFromEnd(CharTransferTask task)
    {
        if (!CharTransfer_FindAvailableRange(task.VramType, out int offsetMain, out int offsetSub, (int)task.Data!.SizeBytes, out int sizeMain, out int sizeSub))
        {
            return false;
        }
        CharTransferTask_SetBaseAddrsFromRange(task, offsetMain, offsetSub);
        task.HasRange = 1;
        task.RangeSizeMain = sizeMain;
        task.RangeSizeSub = sizeSub;
        CharTransferTask_LoadImageMapping(task);
        CharTransfer_ReserveBlocks(task.VramType, offsetMain, offsetSub, sizeMain, sizeSub);
        return true;
    }

    // OFFSET 0x0201F024
    private static void CharTransferTask_Reset(CharTransferTask task)
    {
        if (task.HasRange != 0)
        {
            CharTransferTask_ClearRange(task);
        }
        CharTransferTask_Init(task);
    }

    // OFFSET 0x0201F03C
    private static CharTransferTask? CharTransfer_FindTaskByResourceID(int resourceID)
    {
        for (int i = 0; i < sTaskManager!.Capacity; i++)
        {
            if (sTaskManager.Tasks[i].ResourceID == resourceID)
            {
                return sTaskManager.Tasks[i];
            }
        }
        return null;
    }

    // OFFSET 0x0201F074
    private static uint CharTransferTask_SetMappingTypeFromHardware(CharTransferTask task, int vramType)
    {
        uint mapping;
        if (vramType == G2d.VramMain)
        {
            mapping = Video.GetDisplayControl(DisplayMain);
        }
        else
        {
            mapping = Video.GetDisplayControl(DisplaySub);
        }
        task.Data!.MappingType = mapping & ObjMappingMask;
        return task.Data.MappingType;
    }

    // OFFSET 0x0201F09C
    private static void CharTransferTask_SetBaseAddrs(CharTransferTask task, int offsetMain, int offsetSub)
    {
        task.BaseAddrMain = offsetMain;
        task.BaseAddrSub = offsetSub;
        if (task.IsVramTransfer == 0)
        {
            CharTransferTask_LoadImageMapping(task);
        }
    }

    // OFFSET 0x0201F0B0
    private static bool CharTransfer_FindAvailableRange(int vramType, out int offsetMain, out int offsetSub, int size, out int sizeMain, out int sizeSub)
    {
        offsetMain = 0;
        offsetSub = 0;
        sizeMain = 0;
        sizeSub = 0;
        if ((vramType & G2d.VramMain) != 0)
        {
            sizeMain = CharTransfer_AlignToBlockSize(size, sTaskManager!.BlockSizeMain, true);
            int blocks = CharTransfer_BytesToBlocks(sizeMain, sTaskManager.BlockSizeMain);
            offsetMain = CharTransfer_FindFreeBlockRun((uint)blocks, sTaskManager.BlockBufMain);
            if (offsetMain == NoBlock)
            {
                ErrorHandling.GF_AssertFail();
                return false;
            }
            offsetMain = CharTransfer_BlocksToBytes(offsetMain, sTaskManager.BlockSizeMain);
        }
        if ((vramType & G2d.VramSub) != 0)
        {
            sizeSub = CharTransfer_AlignToBlockSize(size, sTaskManager!.BlockSizeSub, true);
            int blocks = CharTransfer_BytesToBlocks(sizeSub, sTaskManager.BlockSizeSub);
            offsetSub = CharTransfer_FindFreeBlockRun((uint)blocks, sTaskManager.BlockBufSub);
            if (offsetSub == NoBlock)
            {
                ErrorHandling.GF_AssertFail();
                return false;
            }
            offsetSub = CharTransfer_BlocksToBytes(offsetSub, sTaskManager.BlockSizeSub);
        }
        return true;
    }

    // OFFSET 0x0201F15C
    private static void CharTransferTask_SetBaseAddrsFromRange(CharTransferTask task, int offsetMain, int offsetSub)
    {
        if ((task.VramType & G2d.VramMain) != 0)
        {
            task.BaseAddrMain = offsetMain + sTaskManager!.RangeBaseMain;
        }
        if ((task.VramType & G2d.VramSub) != 0)
        {
            task.BaseAddrSub = offsetSub + sTaskManager!.RangeBaseSub;
        }
    }

    // OFFSET 0x0201F18C
    private static void CharTransfer_ReserveBlocks(int vramType, int offsetMain, int offsetSub, int sizeMain, int sizeSub)
    {
        if ((vramType & G2d.VramMain) != 0)
        {
            int blocks = CharTransfer_BytesToBlocks(sizeMain, sTaskManager!.BlockSizeMain);
            int start = CharTransfer_BytesToBlocks(offsetMain, sTaskManager.BlockSizeMain);
            CharTransfer_SetBlockRange((uint)start, (uint)blocks, sTaskManager.BlockBufMain);
        }
        if ((vramType & G2d.VramSub) != 0)
        {
            int blocks = CharTransfer_BytesToBlocks(sizeSub, sTaskManager!.BlockSizeSub);
            int start = CharTransfer_BytesToBlocks(offsetSub, sTaskManager.BlockSizeSub);
            CharTransfer_SetBlockRange((uint)start, (uint)blocks, sTaskManager.BlockBufSub);
        }
    }

    // OFFSET 0x0201F1F4
    private static void CharTransferTask_LoadImageMapping(CharTransferTask task)
    {
        task.ImageProxy.Init();
        if (task.VramType != (G2d.VramMain | G2d.VramSub))
        {
            CharTransferTask_LoadImageMappingForScreen(task, task.VramType);
            return;
        }
        CharTransferTask_LoadImageMappingForScreen(task, G2d.VramMain);
        CharTransferTask_LoadImageMappingForScreen(task, G2d.VramSub);
    }

    // OFFSET 0x0201F220
    private static void CharTransferTask_LoadImageMappingForScreen(CharTransferTask task, int vramType)
    {
        uint mappingType;
        if (task.UseHardwareMappingType != 0)
        {
            mappingType = CharTransferTask_SetMappingTypeFromHardware(task, vramType);
        }
        else
        {
            mappingType = (uint)vramType;
        }
        int baseAddr;
        if (vramType == G2d.VramMain)
        {
            baseAddr = task.BaseAddrMain;
        }
        else
        {
            baseAddr = task.BaseAddrSub;
        }
        if (mappingType == 0)
        {
            G2d.LoadImageMapping(G2d.Layout2D, task.Data!, (uint)baseAddr, vramType, task.ImageProxy);
        }
        else
        {
            G2d.LoadImageMapping(G2d.Layout1D, task.Data!, (uint)baseAddr, vramType, task.ImageProxy);
        }
    }

    // OFFSET 0x0201F26C
    private static void CharTransferTask_LoadVramTransfer(CharTransferTask task)
    {
        task.ImageProxy.Init();
        if (task.VramType != (G2d.VramMain | G2d.VramSub))
        {
            CharTransferTask_LoadVramTransferForScreen(task, task.VramType);
            return;
        }
        CharTransferTask_LoadVramTransferForScreen(task, G2d.VramMain);
        CharTransferTask_LoadVramTransferForScreen(task, G2d.VramSub);
    }

    // OFFSET 0x0201F298
    private static void CharTransferTask_LoadVramTransferForScreen(CharTransferTask task, int vramType)
    {
        if (task.UseHardwareMappingType != 0)
        {
            CharTransferTask_SetMappingTypeFromHardware(task, vramType);
        }
        int baseAddr;
        if (vramType == G2d.VramMain)
        {
            baseAddr = task.BaseAddrMain;
        }
        else
        {
            baseAddr = task.BaseAddrSub;
        }
        G2d.LoadImageVramTransfer(task.Data!, (uint)baseAddr, vramType, task.ImageProxy);
    }

    // OFFSET 0x0201F2D0
    private static CharTransferTask? CharTransfer_FindUnusedTask()
    {
        for (int i = 0; i < sTaskManager!.Capacity; i++)
        {
            if (sTaskManager.Tasks[i].State == StateUnused)
            {
                return sTaskManager.Tasks[i];
            }
        }
        return null;
    }

    // OFFSET 0x0201F30C
    private static void CharTransfer_UpdateVramCapacities()
    {
        switch (Video.ObjectBank)
        {
            case 0x00:
                sTaskManager!.VramSizeMain = 0;
                break;
            case 0x20:
            case 0x40:
                sTaskManager!.VramSizeMain = 0x4000;
                break;
            case 0x60:
                sTaskManager!.VramSizeMain = 0x8000;
                break;
            case 0x10:
                sTaskManager!.VramSizeMain = 0x10000;
                break;
            case 0x30:
            case 0x50:
                sTaskManager!.VramSizeMain = 0x14000;
                break;
            case 0x70:
                sTaskManager!.VramSizeMain = 0x18000;
                break;
            case 0x01:
            case 0x02:
                sTaskManager!.VramSizeMain = 0x20000;
                break;
            case 0x03:
                sTaskManager!.VramSizeMain = 0x40000;
                break;
            default:
                sTaskManager!.VramSizeMain = 0;
                break;
        }
        switch (Video.SubObjectBank)
        {
            case 0x000:
                sTaskManager.VramSizeSub = 0;
                break;
            case 0x100:
                sTaskManager.VramSizeSub = 0x4000;
                break;
            case 0x008:
                sTaskManager.VramSizeSub = 0x20000;
                break;
            default:
                sTaskManager.VramSizeSub = 0;
                break;
        }
        int reservedMain = CharTransfer_BlocksToBytes(sTaskManager.NumBlocksMain, sTaskManager.BlockSizeMain);
        sTaskManager.RangeBaseMain = sTaskManager.VramSizeMain - reservedMain;
        int reservedSub = CharTransfer_BlocksToBytes(sTaskManager.NumBlocksSub, sTaskManager.BlockSizeSub);
        sTaskManager.RangeBaseSub = sTaskManager.VramSizeSub - reservedSub;
        if (!(sTaskManager.RangeBaseMain >= 0 && sTaskManager.RangeBaseSub >= 0))
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!(sTaskManager.RangeBaseMain >= 0 && sTaskManager.RangeBaseSub >= 0))
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x0201F460
    private static void CharTransfer_ClearBothBlockBuffers()
    {
        CharTransfer_ClearBlockBuffer(sTaskManager!.BlockBufMain);
        CharTransfer_ClearBlockBuffer(sTaskManager.BlockBufSub);
    }

    // OFFSET 0x0201F47C
    private static void CharTransfer_AllocBlockBuffers(int numBlocksMain, int numBlocksSub, int heapID)
    {
        sTaskManager!.NumBlocksMain = numBlocksMain;
        sTaskManager.NumBlocksSub = numBlocksSub;
        if (sTaskManager.NumBlocksMain != 0)
        {
            sTaskManager.BlockBufMain = new byte[(uint)numBlocksMain >> 3];
        }
        if (sTaskManager.NumBlocksSub != 0)
        {
            sTaskManager.BlockBufSub = new byte[(uint)numBlocksSub >> 3];
        }
        CharTransfer_ClearBothBlockBuffers();
    }

    // OFFSET 0x0201F4E4
    private static void CharTransfer_FreeBlockBuffer(byte[]? buffer)
    {
        if (buffer == null)
        {
            return;
        }
        if (ReferenceEquals(buffer, sTaskManager!.BlockBufMain))
        {
            sTaskManager.NumBlocksMain = 0;
        }
        else
        {
            sTaskManager.NumBlocksSub = 0;
        }
    }

    // OFFSET 0x0201F50C
    private static uint CharTransfer_GetNumBlocks(byte[] buffer)
    {
        if (ReferenceEquals(buffer, sTaskManager!.BlockBufMain))
        {
            return (uint)sTaskManager.NumBlocksMain;
        }
        return (uint)sTaskManager.NumBlocksSub;
    }

    // OFFSET 0x0201F524
    private static void CharTransfer_ClearBlockBuffer(byte[]? buffer)
    {
        if (buffer != null)
        {
            Array.Clear(buffer, 0, (int)(CharTransfer_GetNumBlocks(buffer) >> 3));
        }
    }

    // OFFSET 0x0201F53C
    private static void CharTransfer_SetBlockRange(uint start, uint count, byte[]? buffer)
    {
        if (buffer == null)
        {
            return;
        }
        uint max = CharTransfer_GetNumBlocks(buffer);
        uint end = start + count;
        for (uint i = start; i < end && i < max; i++)
        {
            CharTransfer_SplitBlockIndex((int)i, out int index, out byte bit);
            if ((buffer[index] & (1 << bit)) != 0)
            {
                ErrorHandling.GF_AssertFail();
            }
            buffer[index] |= (byte)(1 << bit);
        }
    }

    // OFFSET 0x0201F598
    private static int CharTransfer_FindFreeBlockRun(uint count, byte[]? buffer)
    {
        if (buffer != null)
        {
            uint max = CharTransfer_GetNumBlocks(buffer);
            for (uint i = 0; i < max; i++)
            {
                CharTransfer_SplitBlockIndex((int)i, out int index, out byte bit);
                byte mask = (byte)(1 << bit);
                uint j = 0;
                while ((buffer[index] & mask) == 0 && j <= count)
                {
                    uint block = i + j;
                    CharTransfer_SplitBlockIndex((int)block, out index, out bit);
                    mask = (byte)(1 << bit);
                    if (block >= max)
                    {
                        return NoBlock;
                    }
                    j++;
                }
                if (j > count)
                {
                    return (int)i;
                }
                i += j;
            }
        }
        return NoBlock;
    }

    // OFFSET 0x0201F620
    private static void CharTransfer_ClearBlockRange(uint start, uint count, byte[]? buffer)
    {
        if (buffer == null)
        {
            return;
        }
        uint end = start + count;
        for (uint i = start; i < end; i++)
        {
            CharTransfer_SplitBlockIndex((int)i, out int index, out byte bit);
            if ((buffer[index] & (1 << bit)) == 0)
            {
                ErrorHandling.GF_AssertFail();
            }
            buffer[index] &= (byte)((1 << bit) ^ 0xFF);
        }
    }

    // OFFSET 0x0201F670
    private static void CharTransferTask_ClearRange(CharTransferTask task)
    {
        if ((task.VramType & G2d.VramMain) != 0)
        {
            int location = G2d.GetImageLocation(task.ImageProxy, G2d.VramMain);
            int start = CharTransfer_BytesToBlocks(location - sTaskManager!.RangeBaseMain, sTaskManager.BlockSizeMain);
            int blocks = CharTransfer_BytesToBlocks(task.RangeSizeMain, sTaskManager.BlockSizeMain);
            CharTransfer_ClearBlockRange((uint)start, (uint)blocks, sTaskManager.BlockBufMain);
        }
        if ((task.VramType & G2d.VramSub) != 0)
        {
            int location = G2d.GetImageLocation(task.ImageProxy, G2d.VramSub);
            int start = CharTransfer_BytesToBlocks(location - sTaskManager!.RangeBaseSub, sTaskManager.BlockSizeSub);
            int blocks = CharTransfer_BytesToBlocks(task.RangeSizeSub, sTaskManager.BlockSizeSub);
            CharTransfer_ClearBlockRange((uint)start, (uint)blocks, sTaskManager.BlockBufSub);
        }
        task.HasRange = 0;
    }

    // OFFSET 0x0201F6F4
    public static int CharTransfer_GetBlockSize(uint vramMode)
    {
        switch (vramMode)
        {
            case 0x00000010:
                return 1;
            case 0x00100010:
                return 2;
            case 0x00200010:
                return 4;
            case 0x00300010:
                return 8;
            default:
                return 1;
        }
    }

    // OFFSET 0x0201F734
    private static int CharTransfer_AlignToBlockSize(int size, int blockSize, bool roundUp)
    {
        blockSize *= 32;
        int remainder = size % blockSize;
        if (remainder != 0)
        {
            size -= remainder;
            if (roundUp)
            {
                size += blockSize;
            }
        }
        return size;
    }

    // OFFSET 0x0201F754
    private static int CharTransfer_BytesToBlocks(int size, int blockSize)
    {
        return size / 32 / blockSize;
    }

    // OFFSET 0x0201F764
    private static int CharTransfer_BlocksToBytes(int blocks, int blockSize)
    {
        return blocks * blockSize * 32;
    }

    // OFFSET 0x0201F76C
    private static bool CharTransfer_TryGetDestOffsets(int size, int vramType, out int offsetMain, out int offsetSub)
    {
        bool result = true;
        offsetMain = 0;
        offsetSub = 0;
        if ((vramType & G2d.VramMain) != 0)
        {
            if ((uint)(sTaskManager!.OffsetMain + size) > (uint)sTaskManager.RangeBaseMain)
            {
                ErrorHandling.GF_AssertFail();
                result = false;
            }
            else
            {
                offsetMain = sTaskManager.OffsetMain;
            }
        }
        if ((vramType & G2d.VramSub) != 0)
        {
            if ((uint)(sTaskManager!.OffsetSub + size) > (uint)sTaskManager.RangeBaseSub)
            {
                ErrorHandling.GF_AssertFail();
                result = false;
            }
            else
            {
                offsetSub = sTaskManager.OffsetSub;
            }
        }
        return result;
    }

    // OFFSET 0x0201F7BC
    private static void CharTransfer_UpdateVramOffsets(int size, int vramType)
    {
        if ((vramType & G2d.VramMain) != 0)
        {
            sTaskManager!.OffsetMain += size;
            sTaskManager.OffsetMain = CharTransfer_AlignToBlockSize(sTaskManager.OffsetMain, sTaskManager.BlockSizeMain, true);
        }
        if ((vramType & G2d.VramSub) != 0)
        {
            sTaskManager!.OffsetSub += size;
            sTaskManager.OffsetSub = CharTransfer_AlignToBlockSize(sTaskManager.OffsetSub, sTaskManager.BlockSizeSub, true);
        }
    }

    // OFFSET 0x0201F80C
    private static void CharTransfer_SplitBlockIndex(int block, out int index, out byte bit)
    {
        index = block >> 3;
        bit = (byte)(block & 7);
    }

    // OFFSET 0x0201F818
    private static void CharTransfer_ClampRangeToBase(int rangeBase, int offset, int size, out int rangeOffset, out int rangeSize)
    {
        rangeOffset = offset - rangeBase;
        if (rangeOffset < 0)
        {
            rangeSize = size + rangeOffset;
            rangeOffset = 0;
        }
        else
        {
            rangeSize = size;
        }
    }

    private static void CharTransferTask_Copy(CharTransferTask destination, CharTransferTask source)
    {
        destination.Data = source.Data;
        destination.VramType = source.VramType;
        destination.IsVramTransfer = source.IsVramTransfer;
        destination.ResourceID = source.ResourceID;
        destination.ImageProxy.CopyFrom(source.ImageProxy);
        destination.BaseAddrMain = source.BaseAddrMain;
        destination.BaseAddrSub = source.BaseAddrSub;
        destination.State = source.State;
        destination.UseHardwareMappingType = source.UseHardwareMappingType;
        destination.AtEnd = source.AtEnd;
        destination.HasRange = source.HasRange;
        destination.RangeSizeMain = source.RangeSizeMain;
        destination.RangeSizeSub = source.RangeSizeSub;
    }
}
