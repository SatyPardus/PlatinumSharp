namespace PlatinumSharp.Game;

public sealed class PoketchActiveTaskList
{
    public const uint Magic = 0x12345678;
    public const uint NoTask = 0xFFFFFFFF;

    public uint Count;
    public uint Check;
    public uint[] Slots = Array.Empty<uint>();
}

public sealed class PoketchTaskManager
{
    public uint TaskID;
    public uint State;
    public object? TaskData;
    public BufferView? ExtraData;
    public object? ConstTaskData;
    public SysTask? SysTask;
}

public sealed class PoketchTask
{
    public uint TaskID;
    public SysTaskFunc? Func;
    public int ExtraDataSize;

    public PoketchTask(uint taskID, SysTaskFunc? func, int extraDataSize)
    {
        TaskID = taskID;
        Func = func;
        ExtraDataSize = extraDataSize;
    }
}

public static class PoketchTasks
{
    private const uint ColorRedMask = 0x7C00;
    private const uint ColorGreenMask = 0x3E0;
    private const uint ColorBlueMask = 0x1F;
    private const uint LuminanceTable = 0x02256078;
    private const int PoketchOverlay = 25;
    private const int HeapIDPoketch = 8;
    private const int NarcPokemonIcons = 0x13;
    private const uint IconSizeTable = 0x02256074;
    private const uint IconOffsetTable = 0x02256070;

    // OFFSET 0x02255090
    public static void PoketchTask_InitActiveTaskList(PoketchActiveTaskList activeList, uint numTaskSlots)
    {
        activeList.Count = numTaskSlots;
        activeList.Check = PoketchActiveTaskList.Magic;
        activeList.Slots = new uint[numTaskSlots];
        for (int i = 0; i < numTaskSlots; i++)
        {
            activeList.Slots[i] = PoketchActiveTaskList.NoTask;
        }
    }

    // OFFSET 0x022550B0
    private static bool PoketchTask_AddActiveTask(PoketchActiveTaskList activeList, uint taskID)
    {
        if (activeList.Check != PoketchActiveTaskList.Magic)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < activeList.Count; i++)
        {
            if (activeList.Slots[i] == PoketchActiveTaskList.NoTask)
            {
                activeList.Slots[i] = taskID;
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x022550F0
    private static void PoketchTask_RemoveActiveTask(PoketchActiveTaskList activeList, uint taskID)
    {
        if (activeList.Check != PoketchActiveTaskList.Magic)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < activeList.Count; i++)
        {
            if (activeList.Slots[i] == taskID)
            {
                activeList.Slots[i] = PoketchActiveTaskList.NoTask;
                return;
            }
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x02255130
    public static bool PoketchTask_TaskIsNotActive(PoketchActiveTaskList activeList, uint taskID)
    {
        for (int i = 0; i < activeList.Count; i++)
        {
            if (activeList.Slots[i] == taskID)
            {
                return false;
            }
        }
        return true;
    }

    // OFFSET 0x02255154
    public static bool PoketchTask_NoActiveTasks(PoketchActiveTaskList activeList)
    {
        for (int i = 0; i < activeList.Count; i++)
        {
            if (activeList.Slots[i] != PoketchActiveTaskList.NoTask)
            {
                return false;
            }
        }
        return true;
    }

    // OFFSET 0x0225517C
    public static void PoketchTask_Start(PoketchTask[] appTasks, uint taskID, object? taskData, object? constTaskData, PoketchActiveTaskList activeList, uint taskPriority, int heapID)
    {
        for (int i = 0; appTasks[i].TaskID != PoketchActiveTaskList.NoTask; i++)
        {
            if (taskID != appTasks[i].TaskID)
            {
                continue;
            }
            PoketchTaskManager taskMan = new();
            taskMan.ExtraData = appTasks[i].ExtraDataSize != 0 ? new BufferView(new byte[appTasks[i].ExtraDataSize], 0) : null;
            if (!PoketchTask_AddActiveTask(activeList, taskID))
            {
                return;
            }
            taskMan.TaskData = taskData;
            taskMan.State = 0;
            taskMan.TaskID = taskID;
            taskMan.ConstTaskData = constTaskData;
            taskMan.SysTask = SysTask.SysTask_Start(appTasks[i].Func!, taskMan, taskPriority);
            if (taskMan.SysTask == null)
            {
                ErrorHandling.GF_AssertFail();
                return;
            }
            appTasks[i].Func!(taskMan.SysTask, taskMan);
            return;
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x02255224
    public static void PoketchTask_EndTask(PoketchActiveTaskList activeList, PoketchTaskManager taskMan)
    {
        PoketchTask_RemoveActiveTask(activeList, taskMan.TaskID);
        SysTask.SysTask_Done(taskMan.SysTask!);
    }

    // OFFSET 0x0225523C
    public static object? PoketchTask_GetTaskData(PoketchTaskManager taskMan)
    {
        return taskMan.TaskData;
    }

    // OFFSET 0x02255240
    public static object? PoketchTask_GetConstTaskData(PoketchTaskManager taskMan)
    {
        return taskMan.ConstTaskData;
    }

    // OFFSET 0x02255244
    public static BufferView? PoketchTask_GetExtraData(PoketchTaskManager taskMan)
    {
        return taskMan.ExtraData;
    }

    // OFFSET 0x02255248
    public static uint PoketchTask_GetState(PoketchTaskManager taskMan)
    {
        return taskMan.State;
    }

    // OFFSET 0x0225524C
    public static void PoketchTask_IncrementState(PoketchTaskManager taskMan)
    {
        taskMan.State++;
    }

    // OFFSET 0x02255254
    public static void PoketchTask_SetState(PoketchTaskManager taskMan, uint state)
    {
        taskMan.State = state;
    }

    // OFFSET 0x02255258
    public static void PoketchTask_UpdateTilemap2x2(ushort[] tileBuffer, uint x, uint y, uint bufferWidth, uint tileIdx, uint tilesetWidth, uint palette)
    {
        uint index = x + y * bufferWidth;
        uint paletteBits = palette << 12;
        tileBuffer[index] = (ushort)(tileIdx | paletteBits);
        tileBuffer[index + 1] = (ushort)((tileIdx + 1) | paletteBits);
        uint nextRow = tileIdx + tilesetWidth;
        tileBuffer[index + bufferWidth] = (ushort)(nextRow | paletteBits);
        tileBuffer[index + bufferWidth + 1] = (ushort)((nextRow + 1) | paletteBits);
    }

    // OFFSET 0x02255290
    public static void PoketchTask_MapToActivePaletteFromLuminance(ushort[] rawData, int offset, uint numPaletteEntries)
    {
        ushort[] palette = new ushort[16];
        PoketchGraphics.PoketchGraphics_CopyActivePalette(palette);
        byte[] luminanceSlots = RomData.Overlay(PoketchOverlay, LuminanceTable, 4);
        for (uint i = 0; i < numPaletteEntries; i++)
        {
            uint color = rawData[offset + i];
            uint red = (color & ColorRedMask) >> 10;
            uint green = (color & ColorGreenMask) >> 5;
            uint blue = color & ColorBlueMask;
            uint luminance = (red * 0x72 + blue * 0x12B + green * 0x24B) / 1000;
            uint slot = luminance >> 3;
            if (slot > 3)
            {
                slot = 3;
            }
            rawData[offset + i] = palette[luminanceSlots[slot]];
        }
    }

    // OFFSET 0x02255308
    public static void PoketchTask_FillPaletteFromActivePaletteSlot(uint slot, uint offset)
    {
        ushort[] palette = new ushort[16];
        ushort[] fill = new ushort[16];
        PoketchGraphics.PoketchGraphics_CopyActivePalette(palette);
        for (int i = 0; i < 16; i++)
        {
            fill[i] = palette[slot];
        }
        Video.LoadPaletteRange(Video.SubObject, fill, 0, offset * 0x20, 0x20);
    }

    // OFFSET 0x02255360
    public static void PoketchTask_LoadPokemonIconLuminancePalette(uint offset)
    {
        byte[]? file = Graphics.Graphics_GetPlttData(NarcPokemonIcons, (uint)PokemonIcon.PokeIconPalettesFileIndex(), out PaletteFile? palette, HeapIDPoketch);
        if (file == null)
        {
            return;
        }
        PoketchTask_MapToActivePaletteFromLuminance(palette!.Colors, 0, 0x40);
        Video.LoadPaletteRange(Video.SubObject, palette.Colors, 0, offset * 0x20, 0x80);
    }

    // OFFSET 0x022553A0
    public static void PoketchTask_LoadPokemonIcons(uint offset, uint[] iconIdxList, uint numIcons, bool isLarge)
    {
        int sizeIndex = isLarge ? 1 : 0;
        ushort charSize = RomData.OverlayU16(PoketchOverlay, (uint)(IconSizeTable + sizeIndex * 2), 1)[0];
        ushort charOffset = RomData.OverlayU16(PoketchOverlay, (uint)(IconOffsetTable + sizeIndex * 2), 1)[0];
        NARC narc = NARC.NARC_ctor(NarcPokemonIcons, HeapIDPoketch);
        uint destination = offset * 0x20;
        for (uint i = 0; i < numIcons; i++)
        {
            byte[] buffer = new byte[charSize];
            NARC.NARC_ReadFromMember(narc, iconIdxList[i], 0, charSize, buffer);
            G2d.GetUnpackedCharacterData(buffer, out CharacterData? character);
            Video.LoadObjectTiles(1, character!.RawData, destination, charOffset);
            destination += charOffset;
        }
        NARC.NARC_dtor(narc);
    }
}
