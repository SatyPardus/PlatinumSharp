using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class BDHCHeader
{
    public int PointsCount;
    public int NormalsCount;
    public int ConstantsCount;
    public int PlatesCount;
    public int StripsCount;
    public int AccessListCount;
}

public sealed class BDHCLoaderContext
{
    public readonly BDHCHeader Header = new();
    public byte SubTask;
    public byte[]? Buffer;
    public BDHC? Bdhc;
    public bool KillLoadTask;
    public MapLoadTasksState? TasksState;
    public NARC? Narc;
    public int BdhcSize;
}

public sealed class BDHC
{
    public const int NullOffset = -1;

    public byte[]? Data;
    public int PlatesOffset;
    public int ConstantsOffset;
    public int StripsOffset;
    public int AccessListOffset;
    public int PointsOffset;
    public int NormalsOffset;
    public bool Loaded;
    public int StripsCount;

    private const int MagicSize = 4;
    private const int PointSize = 8;
    private const int NormalSize = 12;
    private const int ConstantSize = 4;
    private const int PlateSize = 8;
    private const int StripSize = 8;
    private const int AccessListEntrySize = 2;
    private const int PlateFirstPointOffset = 0;
    private const int PlateSecondPointOffset = 2;
    private const int PlateNormalOffset = 4;
    private const int PlateConstantOffset = 6;
    private const int StripElementCountOffset = 4;
    private const int StripStartIndexOffset = 6;
    private const int MaxCandidates = 10;
    private const int MaxBufferSize = 0x9000;
    private const byte SubTaskPrepare = 0;
    private const byte SubTaskLoad = 1;
    private const byte SubTaskEnd = 2;
    private const uint TaskPriority = 1;

    private static void ReadInto(NARC narc, int size, byte[] dest, int offset)
    {
        byte[] temp = new byte[size];
        NARC.NARC_ReadFile(narc, (uint)size, temp);
        Buffer.BlockCopy(temp, 0, dest, offset, size);
    }

    private static int ReadU16(NARC narc)
    {
        byte[] temp = new byte[2];
        NARC.NARC_ReadFile(narc, 2, temp);
        return Bin.U16(temp, 0);
    }

    private static int DivideFx(int numerator, int denominator)
    {
        if (denominator == 0)
        {
            return 0;
        }
        long quotient = ((long)numerator << 32) / denominator;
        return (int)((quotient + 0x80000) >> 20);
    }

    // OFFSET 0x021EEC68
    private static bool BDHC_IsPointInBoundingBox(int firstX, int firstZ, int secondX, int secondZ, int pointX, int pointZ)
    {
        int minX;
        int maxX;
        if (firstX <= secondX)
        {
            minX = firstX;
            maxX = secondX;
        }
        else
        {
            minX = secondX;
            maxX = firstX;
        }
        int minZ;
        int maxZ;
        if (firstZ <= secondZ)
        {
            minZ = firstZ;
            maxZ = secondZ;
        }
        else
        {
            minZ = secondZ;
            maxZ = firstZ;
        }
        if (minX <= pointX && pointX <= maxX && minZ <= pointZ && pointZ <= maxZ)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021EECB8
    private static void BDHC_GetPointsFromPlate(BDHC bdhc, ushort plateIndex, out int firstX, out int firstZ, out int secondX, out int secondZ)
    {
        byte[] data = bdhc.Data!;
        int plate = bdhc.PlatesOffset + (plateIndex * PlateSize);
        int first = bdhc.PointsOffset + (Bin.U16(data, plate + PlateFirstPointOffset) * PointSize);
        firstX = Bin.I32(data, first);
        firstZ = Bin.I32(data, first + 4);
        int second = bdhc.PointsOffset + (Bin.U16(data, plate + PlateSecondPointOffset) * PointSize);
        secondX = Bin.I32(data, second);
        secondZ = Bin.I32(data, second + 4);
    }

    // OFFSET 0x021EECE8
    private static void BDHC_GetNormalFromPlate(BDHC bdhc, ushort plateIndex, out VecFx32 normal)
    {
        byte[] data = bdhc.Data!;
        int plate = bdhc.PlatesOffset + (plateIndex * PlateSize);
        int offset = bdhc.NormalsOffset + (Bin.U16(data, plate + PlateNormalOffset) * NormalSize);
        normal.X = Bin.I32(data, offset);
        normal.Y = Bin.I32(data, offset + 4);
        normal.Z = Bin.I32(data, offset + 8);
    }

    // OFFSET 0x021EED08
    private static void BDHC_GetConstantFromPlate(BDHC bdhc, ushort plateIndex, out int constant)
    {
        byte[] data = bdhc.Data!;
        int plate = bdhc.PlatesOffset + (plateIndex * PlateSize);
        constant = Bin.I32(data, bdhc.ConstantsOffset + (Bin.U16(data, plate + PlateConstantOffset) * ConstantSize));
    }

    // OFFSET 0x021EED20
    private static void BDHC_InitCandidateObjectHeightsArray(int[] heights)
    {
        for (int i = 0; i < MaxCandidates; i++)
        {
            heights[i] = 0;
        }
    }

    // OFFSET 0x021EED38
    private static bool BDHC_FindStripIndexByScanline(BDHC bdhc, ushort stripsCount, int scanline, out ushort stripIndex)
    {
        stripIndex = 0;
        if (stripsCount == 0)
        {
            return false;
        }
        if (stripsCount == 1)
        {
            stripIndex = 0;
            return true;
        }
        byte[] data = bdhc.Data!;
        int high = stripsCount - 1;
        int low = 0;
        int middle = high / 2;
        while (true)
        {
            if (Bin.I32(data, bdhc.StripsOffset + (middle * StripSize)) > scanline)
            {
                if (high - 1 > low)
                {
                    high = middle;
                    middle = (low + middle) / 2;
                }
                else
                {
                    stripIndex = (ushort)middle;
                    return true;
                }
            }
            else
            {
                if (low + 1 < high)
                {
                    low = middle;
                    middle = (middle + high) / 2;
                }
                else
                {
                    stripIndex = (ushort)(middle + 1);
                    return true;
                }
            }
        }
    }

    // OFFSET 0x021EED9C
    public static bool CalculateObjectHeight(int objectHeight, int objectX, int objectZ, BDHC bdhc, out int newObjectHeight)
    {
        newObjectHeight = 0;
        if (!bdhc.Loaded)
        {
            return false;
        }
        int[] heights = new int[MaxCandidates];
        int candidateCount = 0;
        BDHC_InitCandidateObjectHeightsArray(heights);
        if (!BDHC_FindStripIndexByScanline(bdhc, (ushort)bdhc.StripsCount, objectZ, out ushort stripIndex))
        {
            return false;
        }
        byte[] data = bdhc.Data!;
        int strip = bdhc.StripsOffset + (stripIndex * StripSize);
        int elementCount = Bin.U16(data, strip + StripElementCountOffset);
        int startIndex = Bin.U16(data, strip + StripStartIndexOffset);
        for (ushort i = 0; i < elementCount; i++)
        {
            ushort plateIndex = (ushort)Bin.U16(data, bdhc.AccessListOffset + ((startIndex + i) * AccessListEntrySize));
            BDHC_GetPointsFromPlate(bdhc, plateIndex, out int firstX, out int firstZ, out int secondX, out int secondZ);
            if (BDHC_IsPointInBoundingBox(firstX, firstZ, secondX, secondZ, objectX, objectZ))
            {
                BDHC_GetNormalFromPlate(bdhc, plateIndex, out VecFx32 normal);
                BDHC_GetConstantFromPlate(bdhc, plateIndex, out int constant);
                int productX = (int)((((long)normal.X * objectX) + 0x800) >> 12);
                int productZ = (int)((((long)normal.Z * objectZ) + 0x800) >> 12);
                heights[candidateCount] = DivideFx(-(constant + (productX + productZ)), normal.Y);
                candidateCount++;
                if (candidateCount >= MaxCandidates)
                {
                    break;
                }
            }
        }
        if (candidateCount > 1)
        {
            int closest = 0;
            int smallestDifference = FxUtil.FX_Max(objectHeight, heights[0]) - FxUtil.FX_Min(objectHeight, heights[0]);
            for (ushort i = 1; i < candidateCount; i++)
            {
                int difference = FxUtil.FX_Max(objectHeight, heights[i]) - FxUtil.FX_Min(objectHeight, heights[i]);
                if (smallestDifference > difference)
                {
                    smallestDifference = difference;
                    closest = i;
                }
            }
            newObjectHeight = heights[closest];
            return true;
        }
        if (candidateCount == 1)
        {
            newObjectHeight = heights[0];
            return true;
        }
        if (candidateCount != 0)
        {
            newObjectHeight = heights[0];
            return true;
        }
        return false;
    }

    // OFFSET 0x021EEF34
    private static void BDHC_LoadHeader(NARC narc, BDHCHeader header)
    {
        header.PointsCount = 0;
        header.NormalsCount = 0;
        header.ConstantsCount = 0;
        header.PlatesCount = 0;
        header.StripsCount = 0;
        header.AccessListCount = 0;
        byte[] magic = new byte[MagicSize];
        NARC.NARC_ReadFile(narc, MagicSize, magic);
        header.PointsCount = ReadU16(narc);
        header.NormalsCount = ReadU16(narc);
        header.ConstantsCount = ReadU16(narc);
        header.PlatesCount = ReadU16(narc);
        header.StripsCount = ReadU16(narc);
        header.AccessListCount = ReadU16(narc);
    }

    // OFFSET 0x021EEF98
    private static void BDHC_PrepareBuffers(BDHCHeader header, BDHC bdhc, byte[]? buffer)
    {
        bdhc.Data = buffer;
        int offset = 0;
        bdhc.PointsOffset = offset;
        offset += header.PointsCount * PointSize;
        bdhc.NormalsOffset = offset;
        offset += header.NormalsCount * NormalSize;
        bdhc.ConstantsOffset = offset;
        offset += header.ConstantsCount * ConstantSize;
        bdhc.PlatesOffset = offset;
        offset += header.PlatesCount * PlateSize;
        bdhc.StripsOffset = offset;
        offset += header.StripsCount * StripSize;
        bdhc.AccessListOffset = offset;
        offset += header.AccessListCount * AccessListEntrySize;
        if (offset > MaxBufferSize)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021EEFF8
    private static void BDHC_LoadPoints(NARC narc, BDHC bdhc, BDHCHeader header)
    {
        ReadInto(narc, header.PointsCount * PointSize, bdhc.Data!, bdhc.PointsOffset);
    }

    // OFFSET 0x021EF008
    private static void BDHC_LoadNormals(NARC narc, BDHC bdhc, BDHCHeader header)
    {
        ReadInto(narc, header.NormalsCount * NormalSize, bdhc.Data!, bdhc.NormalsOffset);
    }

    // OFFSET 0x021EF01C
    private static void BDHC_LoadConstants(NARC narc, BDHC bdhc, BDHCHeader header)
    {
        ReadInto(narc, header.ConstantsCount * ConstantSize, bdhc.Data!, bdhc.ConstantsOffset);
    }

    // OFFSET 0x021EF02C
    private static void BDHC_LoadPlates(NARC narc, BDHC bdhc, BDHCHeader header)
    {
        ReadInto(narc, header.PlatesCount * PlateSize, bdhc.Data!, bdhc.PlatesOffset);
    }

    // OFFSET 0x021EF03C
    private static void BDHC_LoadStrips(NARC narc, BDHC bdhc, BDHCHeader header)
    {
        ReadInto(narc, header.StripsCount * StripSize, bdhc.Data!, bdhc.StripsOffset);
    }

    // OFFSET 0x021EF04C
    private static void BDHC_LoadAccessList(NARC narc, BDHC bdhc, BDHCHeader header)
    {
        ReadInto(narc, header.AccessListCount * AccessListEntrySize, bdhc.Data!, bdhc.AccessListOffset);
    }

    // OFFSET 0x021EF05C
    private static void BDHC_LazyLoadTask(SysTask sysTask, object? sysTaskParam)
    {
        BDHCLoaderContext context = (BDHCLoaderContext)sysTaskParam!;
        if (context.KillLoadTask)
        {
            context.SubTask = SubTaskEnd;
        }
        bool advance = false;
        switch (context.SubTask)
        {
            case SubTaskPrepare:
                if (context.TasksState!.MapModelTaskRunning != 0)
                {
                    advance = false;
                }
                else
                {
                    BDHC_LoadHeader(context.Narc!, context.Header);
                    context.Bdhc!.StripsCount = context.Header.StripsCount;
                    BDHC_PrepareBuffers(context.Header, context.Bdhc, context.Buffer);
                    advance = true;
                }
                break;
            case SubTaskLoad:
                BDHC_LoadPoints(context.Narc!, context.Bdhc!, context.Header);
                BDHC_LoadNormals(context.Narc!, context.Bdhc!, context.Header);
                BDHC_LoadConstants(context.Narc!, context.Bdhc!, context.Header);
                BDHC_LoadPlates(context.Narc!, context.Bdhc!, context.Header);
                BDHC_LoadStrips(context.Narc!, context.Bdhc!, context.Header);
                BDHC_LoadAccessList(context.Narc!, context.Bdhc!, context.Header);
                advance = true;
                break;
            case SubTaskEnd:
                context.TasksState!.BdhcTaskRunning = 0;
                SysTask.SysTask_Done(sysTask);
                return;
        }
        if (advance)
        {
            context.SubTask++;
            if (context.SubTask == SubTaskEnd)
            {
                context.Bdhc!.Loaded = true;
            }
        }
    }

    // OFFSET 0x021EF13C
    public static BDHC BDHC_New()
    {
        BDHC bdhc = new();
        bdhc.PointsOffset = NullOffset;
        bdhc.NormalsOffset = NullOffset;
        bdhc.PlatesOffset = NullOffset;
        bdhc.StripsOffset = NullOffset;
        bdhc.AccessListOffset = NullOffset;
        bdhc.Loaded = false;
        bdhc.StripsCount = 0;
        return bdhc;
    }

    // OFFSET 0x021EF158
    public static void BDHC_Load(NARC narc, int bdhcSize, BDHC bdhc, byte[]? buffer)
    {
        BDHCHeader header = new();
        BDHC_LoadHeader(narc, header);
        bdhc.StripsCount = header.StripsCount;
        BDHC_PrepareBuffers(header, bdhc, buffer);
        BDHC_LoadPoints(narc, bdhc, header);
        BDHC_LoadNormals(narc, bdhc, header);
        BDHC_LoadConstants(narc, bdhc, header);
        BDHC_LoadPlates(narc, bdhc, header);
        BDHC_LoadStrips(narc, bdhc, header);
        BDHC_LoadAccessList(narc, bdhc, header);
        bdhc.Loaded = true;
    }

    // OFFSET 0x021EF1D0
    public static void BDHC_Free(BDHC? bdhc)
    {
    }

    // OFFSET 0x021EF1DC
    public static void BDHC_Reset(BDHC? bdhc)
    {
        if (bdhc != null)
        {
            bdhc.Loaded = false;
            bdhc.PointsOffset = NullOffset;
            bdhc.NormalsOffset = NullOffset;
            bdhc.PlatesOffset = NullOffset;
            bdhc.StripsOffset = NullOffset;
            bdhc.AccessListOffset = NullOffset;
        }
    }

    // OFFSET 0x021EF1F0
    public static SysTask? BDHC_LazyLoad(NARC landDataNARC, int bdhcSize, BDHC? bdhc, MapLoadTasksState tasksState, byte[]? buffer)
    {
        BDHCLoaderContext context = new();
        context.SubTask = SubTaskPrepare;
        context.Narc = landDataNARC;
        context.BdhcSize = bdhcSize;
        context.Bdhc = bdhc;
        context.TasksState = tasksState;
        context.KillLoadTask = false;
        context.Buffer = buffer;
        return SysTask.SysTask_Start(BDHC_LazyLoadTask, context, TaskPriority);
    }

    // OFFSET 0x021EF23C
    public static void BDHC_KillLoad(SysTask sysTask)
    {
        BDHCLoaderContext context = (BDHCLoaderContext)SysTaskManager.SysTask_GetParam(sysTask)!;
        context.KillLoadTask = true;
    }

    // OFFSET 0x021EF248
    public static void BDHC_MarkNotLoaded(BDHC bdhc)
    {
        bdhc.Loaded = false;
    }
}
