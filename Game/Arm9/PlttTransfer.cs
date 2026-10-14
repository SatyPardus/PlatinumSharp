using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class PlttTransferTask
{
    public PaletteFile? Data;
    public int VramType;
    public int PaletteCount;
    public int ResourceID;
    public ImagePaletteProxy PaletteProxy = new();
    public uint BaseAddrMain;
    public uint BaseAddrSub;
    public byte Initialized;
}

public sealed class PlttTransferTaskManager
{
    public PlttTransferTask[] Tasks = Array.Empty<PlttTransferTask>();
    public int Capacity;
    public int Count;
    public uint OffsetMain;
    public uint OffsetSub;
    public uint ExtPlttOffsetMain;
    public uint ExtPlttOffsetSub;
    public uint ExtPlttVramSizeMain;
    public uint ExtPlttVramSizeSub;
    public ushort ReservedMaskMain;
    public ushort ReservedMaskSub;
}

public static class PlttTransfer
{
    private const int PaletteSizeShift = 5;
    private const int SlotCount = 16;
    private const uint StandardPlttVramSize = 0x200;
    private const uint ExtPlttVramSize = 0x2000;
    private const int TransferMainObjectPalette = 0xE;
    private const int TransferSubObjectPalette = 0x1E;
    private const int Format256Colors = 4;
    private const int BankObjExtPltt = 5;
    private const int BankSubObjExtPltt = 7;
    private const uint NoSlot = 0xFFFFFFFF;

    private static PlttTransferTaskManager? sTaskManager;

    // OFFSET 0x0201F834
    public static void PlttTransfer_Init(int capacity, int heapID)
    {
        if (sTaskManager != null)
        {
            return;
        }
        sTaskManager = new PlttTransferTaskManager();
        sTaskManager.Capacity = capacity;
        sTaskManager.Tasks = new PlttTransferTask[capacity];
        for (int i = 0; i < capacity; i++)
        {
            sTaskManager.Tasks[i] = new PlttTransferTask();
            PlttTransferTask_Init(sTaskManager.Tasks[i]);
        }
    }

    // OFFSET 0x0201F890
    public static void PlttTransfer_MarkReservedSlots(uint reservedMask, int vramType)
    {
        if (vramType == G2d.VramMain)
        {
            sTaskManager!.ReservedMaskMain |= (ushort)reservedMask;
        }
        else if (vramType == G2d.VramSub)
        {
            sTaskManager!.ReservedMaskSub |= (ushort)reservedMask;
        }
    }

    // OFFSET 0x0201F8B4
    public static void PlttTransfer_Free()
    {
        if (sTaskManager != null)
        {
            PlttTransfer_ResetAllTasks();
            sTaskManager = null;
        }
    }

    // OFFSET 0x0201F8E0
    private static void PlttTransfer_OnOutOfSpace()
    {
    }

    // OFFSET 0x0201F8E4
    public static void PlttTransfer_Clear()
    {
        sTaskManager!.OffsetMain = 0;
        sTaskManager.OffsetSub = 0;
        sTaskManager.ExtPlttOffsetMain = 0;
        sTaskManager.ExtPlttOffsetSub = 0;
        PlttTransfer_UpdateExtPlttVramSizes();
        PlttTransfer_ClearReservedMasks(sTaskManager);
    }

    // OFFSET 0x0201F90C
    public static bool PlttTransfer_RequestWholeRange(PlttTransferTaskTemplate template)
    {
        PlttTransferTask? task = PlttTransfer_FindUnusedTask();
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        if (!PlttTransferTask_InitFromTemplate(template, task))
        {
            return false;
        }
        if (!PlttTransferTask_ReserveAndTransferWholeRange(template, task))
        {
            PlttTransfer_ResetTask(template.ResourceID);
            return false;
        }
        PlttTransferTask_ReserveSlots(task);
        return true;
    }

    // OFFSET 0x0201F950
    public static bool PlttTransfer_RequestFreeSpace(PlttTransferTaskTemplate template)
    {
        PlttTransferTask? task = PlttTransfer_FindUnusedTask();
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        if (!PlttTransferTask_InitFromTemplate(template, task))
        {
            return false;
        }
        if (!PlttTransferTask_ReserveAndTransferFreeSpace(template, task))
        {
            PlttTransfer_ResetTask(template.ResourceID);
            return false;
        }
        return true;
    }

    // OFFSET 0x0201F990
    public static void PlttTransfer_ReplacePlttData(int resourceID, PaletteFile? data)
    {
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        PlttTransferTask? task = PlttTransfer_FindTaskByResourceID(resourceID);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        task!.Data = data;
        if ((task.VramType & G2d.VramMain) != 0)
        {
            VramTransfer.VramTransfer_Request(TransferMainObjectPalette, task.BaseAddrMain, data!.Colors, (uint)(task.PaletteCount << PaletteSizeShift));
        }
        if ((task.VramType & G2d.VramSub) != 0)
        {
            VramTransfer.VramTransfer_Request(TransferSubObjectPalette, task.BaseAddrSub, data!.Colors, (uint)(task.PaletteCount << PaletteSizeShift));
        }
    }

    // OFFSET 0x0201F9DC
    public static bool PlttTransfer_HasTask(int resourceID)
    {
        return PlttTransfer_FindTaskByResourceID(resourceID) != null;
    }

    // OFFSET 0x0201F9F0
    public static void PlttTransfer_ResetTask(int resourceID)
    {
        PlttTransferTask? task = PlttTransfer_FindTaskByResourceID(resourceID);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (task!.Initialized == 1)
        {
            PlttTransferTask_ClearSlots(task);
            PlttTransferTask_Reset(task);
        }
    }

    // OFFSET 0x0201FA18
    public static void PlttTransfer_ResetAllTasks()
    {
        for (int i = 0; i < sTaskManager!.Capacity; i++)
        {
            if (sTaskManager.Tasks[i].Initialized == 1)
            {
                PlttTransferTask_ClearSlots(sTaskManager.Tasks[i]);
                PlttTransferTask_Reset(sTaskManager.Tasks[i]);
            }
        }
    }

    // OFFSET 0x0201FA58
    public static ImagePaletteProxy? PlttTransfer_GetPaletteProxy(int resourceID)
    {
        PlttTransferTask? task = PlttTransfer_FindTaskByResourceID(resourceID);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
            return null;
        }
        if (task.Initialized == 1)
        {
            return task.PaletteProxy;
        }
        return null;
    }

    // OFFSET 0x0201FA80
    public static ImagePaletteProxy? PlttTransfer_ToggleExtPalette(int resourceID, ImageProxy imageProxy)
    {
        PlttTransferTask? task = PlttTransfer_FindTaskByResourceID(resourceID);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
            return null;
        }
        if (task.Initialized != 1)
        {
            return null;
        }
        if (task.Data!.Extended)
        {
            imageProxy.ExtendedPalette = 1;
        }
        return task.PaletteProxy;
    }

    // OFFSET 0x0201FAB4
    public static uint PlttTransfer_GetPlttOffset(ImagePaletteProxy paletteProxy, int vramType)
    {
        uint paletteSize;
        if (paletteProxy.Palette!.Extended)
        {
            paletteSize = 0x200;
        }
        else if (paletteProxy.Palette.Format == Format256Colors)
        {
            paletteSize = 0;
        }
        else
        {
            paletteSize = 0x20;
        }
        if (paletteSize != 0)
        {
            return (uint)G2d.GetImagePaletteLocation(paletteProxy, vramType) / paletteSize;
        }
        return 0;
    }

    // OFFSET 0x0201FAE4
    private static void PlttTransferTask_Reset(PlttTransferTask task)
    {
        PlttTransferTask_Init(task);
    }

    // OFFSET 0x0201FAEC
    private static bool PlttTransferTask_InitFromTemplate(PlttTransferTaskTemplate template, PlttTransferTask task)
    {
        task.Data = template.Data;
        if (PlttTransfer_HasTask(template.ResourceID))
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        task.ResourceID = template.ResourceID;
        task.VramType = template.VramType;
        task.Initialized = 1;
        task.PaletteCount = template.PaletteCount;
        return true;
    }

    // OFFSET 0x0201FB20
    private static void PlttTransferTask_Init(PlttTransferTask task)
    {
        task.Data = null;
        task.VramType = 0;
        task.PaletteCount = 0;
        task.BaseAddrMain = 0;
        task.BaseAddrSub = 0;
        task.Initialized = 0;
        task.ResourceID = -1;
        task.PaletteProxy.Init();
    }

    // OFFSET 0x0201FB3C
    private static bool PlttTransferTask_ReserveAndTransferWholeRange(PlttTransferTaskTemplate template, PlttTransferTask task)
    {
        if (task.Data!.Extended)
        {
            PlttTransferTask_TryGetDestOffsets(task, sTaskManager!.ExtPlttOffsetMain, sTaskManager.ExtPlttOffsetSub, sTaskManager.ExtPlttVramSizeMain, sTaskManager.ExtPlttVramSizeSub);
            PlttTransferTask_LoadWholeRange(task);
            PlttTransferTask_AdvanceOffsets(task, ref sTaskManager.ExtPlttOffsetMain, ref sTaskManager.ExtPlttOffsetSub);
        }
        else
        {
            PlttTransferTask_TryGetDestOffsets(task, sTaskManager!.OffsetMain, sTaskManager.OffsetSub, StandardPlttVramSize, StandardPlttVramSize);
            PlttTransferTask_LoadWholeRange(task);
            PlttTransferTask_AdvanceOffsets(task, ref sTaskManager.OffsetMain, ref sTaskManager.OffsetSub);
        }
        return true;
    }

    // OFFSET 0x0201FB94
    private static bool PlttTransferTask_ReserveAndTransferFreeSpace(PlttTransferTaskTemplate template, PlttTransferTask task)
    {
        uint offsetMain = 0;
        uint offsetSub = 0;
        if (task.Data!.Extended)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((task.VramType & G2d.VramMain) != 0)
        {
            offsetMain = PlttTransfer_FindAvailableSlot(sTaskManager!.ReservedMaskMain, task.PaletteCount);
            if (offsetMain == NoSlot)
            {
                return false;
            }
        }
        if ((task.VramType & G2d.VramSub) != 0)
        {
            offsetSub = PlttTransfer_FindAvailableSlot(sTaskManager!.ReservedMaskSub, task.PaletteCount);
            if (offsetSub == NoSlot)
            {
                return false;
            }
        }
        if ((task.VramType & G2d.VramMain) != 0)
        {
            task.BaseAddrMain = offsetMain;
        }
        if ((task.VramType & G2d.VramSub) != 0)
        {
            task.BaseAddrSub = offsetSub;
        }
        task.Data.ByteSize = (uint)(task.PaletteCount << PaletteSizeShift);
        PlttTransferTask_LoadPalette(task);
        PlttTransferTask_ReserveSlots(task);
        return true;
    }

    // OFFSET 0x0201FC18
    private static PlttTransferTask? PlttTransfer_FindTaskByResourceID(int resourceID)
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

    // OFFSET 0x0201FC50
    private static PlttTransferTask? PlttTransfer_FindUnusedTask()
    {
        for (int i = 0; i < sTaskManager!.Capacity; i++)
        {
            if (sTaskManager.Tasks[i].Initialized == 0)
            {
                return sTaskManager.Tasks[i];
            }
        }
        return null;
    }

    // OFFSET 0x0201FC8C
    private static void PlttTransfer_UpdateExtPlttVramSizes()
    {
        switch (Video.Banks[BankObjExtPltt])
        {
            case 0x20:
            case 0x40:
                sTaskManager!.ExtPlttVramSizeMain = ExtPlttVramSize;
                break;
            default:
                sTaskManager!.ExtPlttVramSizeMain = 0;
                break;
        }
        switch (Video.Banks[BankSubObjExtPltt])
        {
            case 0x100:
                sTaskManager.ExtPlttVramSizeSub = ExtPlttVramSize;
                break;
            default:
                sTaskManager.ExtPlttVramSizeSub = 0;
                break;
        }
    }

    // OFFSET 0x0201FCD4
    private static void PlttTransferTask_LoadWholeRange(PlttTransferTask task)
    {
        task.Data!.ByteSize = (uint)(task.PaletteCount << PaletteSizeShift);
        PlttTransferTask_LoadPalette(task);
    }

    // OFFSET 0x0201FCE4
    private static void PlttTransferTask_LoadPalette(PlttTransferTask task)
    {
        task.PaletteProxy.Init();
        if ((task.VramType & G2d.VramMain) != 0)
        {
            G2d.LoadPalette(task.Data!, null, task.BaseAddrMain, G2d.VramMain, task.PaletteProxy);
        }
        if ((task.VramType & G2d.VramSub) != 0)
        {
            G2d.LoadPalette(task.Data!, null, task.BaseAddrSub, G2d.VramSub, task.PaletteProxy);
        }
    }

    // OFFSET 0x0201FD18
    private static void PlttTransfer_SetSlots(ref ushort reservedMask, int count, int start)
    {
        for (int i = 0; i < count; i++)
        {
            reservedMask |= (ushort)(1 << (start + i));
        }
    }

    // OFFSET 0x0201FD3C
    private static void PlttTransfer_ClearSlots(ref ushort reservedMask, int count, int start)
    {
        for (int i = 0; i < count; i++)
        {
            reservedMask &= (ushort)~(1 << (start + i));
        }
    }

    // OFFSET 0x0201FD5C
    private static uint PlttTransfer_FindAvailableSlot(ushort reservedMask, int count)
    {
        int start = 0;
        do
        {
            int length = 0;
            while ((reservedMask & (1 << (start + length))) == 0 && length < count)
            {
                if (start + length >= SlotCount)
                {
                    break;
                }
                length++;
            }
            if (length >= count)
            {
                break;
            }
            start += length + 1;
        } while (start < SlotCount);
        if (start >= SlotCount)
        {
            return NoSlot;
        }
        return (uint)(start << PaletteSizeShift);
    }

    // OFFSET 0x0201FD9C
    private static void PlttTransfer_ClearReservedMasks(PlttTransferTaskManager manager)
    {
        manager.ReservedMaskMain = 0;
        manager.ReservedMaskSub = 0;
    }

    // OFFSET 0x0201FDA4
    private static void PlttTransferTask_ReserveSlots(PlttTransferTask task)
    {
        if ((task.VramType & G2d.VramMain) != 0)
        {
            PlttTransfer_SetSlots(ref sTaskManager!.ReservedMaskMain, task.PaletteCount, (int)(task.BaseAddrMain >> PaletteSizeShift));
        }
        if ((task.VramType & G2d.VramSub) != 0)
        {
            PlttTransfer_SetSlots(ref sTaskManager!.ReservedMaskSub, task.PaletteCount, (int)(task.BaseAddrSub >> PaletteSizeShift));
        }
    }

    // OFFSET 0x0201FDE0
    private static void PlttTransferTask_ClearSlots(PlttTransferTask task)
    {
        if ((task.VramType & G2d.VramMain) != 0)
        {
            PlttTransfer_ClearSlots(ref sTaskManager!.ReservedMaskMain, task.PaletteCount, (int)(task.BaseAddrMain >> PaletteSizeShift));
        }
        if ((task.VramType & G2d.VramSub) != 0)
        {
            PlttTransfer_ClearSlots(ref sTaskManager!.ReservedMaskSub, task.PaletteCount, (int)(task.BaseAddrSub >> PaletteSizeShift));
        }
    }

    // OFFSET 0x0201FE1C
    private static bool PlttTransferTask_TryGetDestOffsets(PlttTransferTask task, uint offsetMain, uint offsetSub, uint sizeMain, uint sizeSub)
    {
        bool result = true;
        if ((task.VramType & G2d.VramMain) != 0)
        {
            if (offsetMain + (uint)(task.PaletteCount << PaletteSizeShift) > sizeMain)
            {
                ErrorHandling.GF_AssertFail();
                PlttTransfer_OnOutOfSpace();
                result = false;
            }
            else
            {
                task.BaseAddrMain = offsetMain;
            }
        }
        if ((task.VramType & G2d.VramSub) != 0)
        {
            if (offsetSub + (uint)(task.PaletteCount << PaletteSizeShift) > sizeSub)
            {
                ErrorHandling.GF_AssertFail();
                PlttTransfer_OnOutOfSpace();
                result = false;
            }
            else
            {
                task.BaseAddrSub = offsetSub;
            }
        }
        return result;
    }

    // OFFSET 0x0201FE68
    private static void PlttTransferTask_AdvanceOffsets(PlttTransferTask task, ref uint offsetMain, ref uint offsetSub)
    {
        if ((task.VramType & G2d.VramMain) != 0)
        {
            offsetMain += (uint)(task.PaletteCount << PaletteSizeShift);
        }
        if ((task.VramType & G2d.VramSub) != 0)
        {
            offsetSub += (uint)(task.PaletteCount << PaletteSizeShift);
        }
    }
}
