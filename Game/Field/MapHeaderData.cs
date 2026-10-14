using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class BgEvent
{
    public const int Size = 0x14;

    public ushort Script;
    public ushort Type;
    public int X;
    public int Z;
    public int Y;
    public ushort PlayerFacingDir;
}

public sealed class ObjectEvent
{
    public const int Size = 0x20;

    public ushort LocalID;
    public ushort GraphicsID;
    public ushort MovementType;
    public ushort TrainerType;
    public ushort HiddenFlag;
    public ushort Script;
    public short Dir;
    public readonly ushort[] Data = new ushort[3];
    public short MovementRangeX;
    public short MovementRangeZ;
    public ushort X;
    public ushort Z;
    public int Y;
}

public sealed class WarpEvent
{
    public const int Size = 0xC;

    public ushort X;
    public ushort Z;
    public ushort DestHeaderID;
    public ushort DestWarpID;
}

public sealed class CoordEvent
{
    public const int Size = 0x10;

    public ushort Script;
    public ushort X;
    public ushort Z;
    public ushort Width;
    public ushort Length;
    public ushort Y;
    public ushort Value;
    public ushort Var;
}

public sealed class MapHeaderData
{
    public const int EventsBufferSize = 0x800;
    public const int InitScriptsSize = 0x100;
    public const int WildEncountersSize = 0x1A8;

    private const int NarcEvents = 0x20;
    private const int NarcInitScripts = 0xA;
    private const int NarcWildEncounters = 0xA1;

    public uint NumBgEvents;
    public uint NumObjectEvents;
    public uint NumWarpEvents;
    public uint NumCoordEvents;
    public BgEvent[]? BgEvents;
    public ObjectEvent[]? ObjectEvents;
    public WarpEvent[]? WarpEvents;
    public CoordEvent[]? CoordEvents;
    public readonly byte[] EventsBuffer = new byte[EventsBufferSize];
    public readonly byte[] InitScripts = new byte[InitScriptsSize];
    public readonly byte[] WildEncounters = new byte[WildEncountersSize];

    // OFFSET 0x0203A378
    public static void MapHeaderData_Init(FieldSystem fieldSystem, int heapID)
    {
        if (fieldSystem.MapHeaderData != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        fieldSystem.MapHeaderData = new MapHeaderData();
    }

    // OFFSET 0x0203A398
    public static void MapHeaderData_Free(FieldSystem fieldSystem)
    {
        if (fieldSystem.MapHeaderData == null)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x0203A3B0
    public static void MapHeaderData_Load(FieldSystem fieldSystem, int mapHeaderID)
    {
        if (fieldSystem.MapHeaderData == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapHeaderData_LoadEvents(fieldSystem.MapHeaderData!, mapHeaderID);
        MapHeaderData_ParseEvents(fieldSystem.MapHeaderData!);
        MapHeaderData_LoadWildEncounters(fieldSystem.MapHeaderData!.WildEncounters, mapHeaderID);
        MapHeaderData_LoadInitScripts(fieldSystem.MapHeaderData!, mapHeaderID);
    }

    // OFFSET 0x0203A3E8
    private static void MapHeaderData_LoadEvents(MapHeaderData data, int mapHeaderID)
    {
        int eventsArchiveID = MapHeader.MapHeader_GetEventsArchiveID(mapHeaderID);
        if (NARC.NARC_GetMemberSizeByIndexPair(NarcEvents, eventsArchiveID) >= EventsBufferSize)
        {
            ErrorHandling.GF_AssertFail();
        }
        NARC.NARC_ReadWholeMemberByIndexPair(data.EventsBuffer, NarcEvents, eventsArchiveID);
    }

    // OFFSET 0x0203A418
    public static void MapHeaderData_LoadObjectEventsIntoMapObjects(FieldSystem fieldSystem)
    {
        MapHeaderData? data = fieldSystem.MapHeaderData;
        uint numObjectEvents = data!.NumObjectEvents;
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (numObjectEvents != 0)
        {
            MapObjectManager.MapObjectMan_AddAllFromObjectEvents(fieldSystem.MapObjectManager, (int)fieldSystem.Location!.U32(0), (int)numObjectEvents, fieldSystem.MapHeaderData!.ObjectEvents);
        }
    }

    // OFFSET 0x0203A440
    public static BgEvent[]? MapHeaderData_GetBgEvents(FieldSystem fieldSystem)
    {
        return fieldSystem.MapHeaderData!.BgEvents;
    }

    // OFFSET 0x0203A448
    public static int MapHeaderData_GetNumBgEvents(FieldSystem fieldSystem)
    {
        return (int)fieldSystem.MapHeaderData!.NumBgEvents;
    }

    // OFFSET 0x0203A450
    public static WarpEvent? MapHeaderData_GetWarpEventByIndex(FieldSystem fieldSystem, int index)
    {
        MapHeaderData data = fieldSystem.MapHeaderData!;
        if ((uint)index >= data.NumWarpEvents)
        {
            return null;
        }
        return data.WarpEvents![index];
    }

    // OFFSET 0x0203A468
    public static int MapHeaderData_GetIndexOfWarpEventAtPos(FieldSystem fieldSystem, int x, int z)
    {
        MapHeaderData data = fieldSystem.MapHeaderData!;
        for (uint i = 0; i < data.NumWarpEvents; i++)
        {
            if (x == data.WarpEvents![i].X && z == data.WarpEvents[i].Z)
            {
                return (int)i;
            }
        }
        return -1;
    }

    // OFFSET 0x0203A4A4
    public static int MapHeaderData_GetNumCoordEvents(FieldSystem fieldSystem)
    {
        return (int)fieldSystem.MapHeaderData!.NumCoordEvents;
    }

    // OFFSET 0x0203A4AC
    public static CoordEvent[]? MapHeaderData_GetCoordEvents(FieldSystem fieldSystem)
    {
        return fieldSystem.MapHeaderData!.CoordEvents;
    }

    // OFFSET 0x0203A4B4
    public static uint MapHeaderData_GetNumObjectEvents(FieldSystem fieldSystem)
    {
        return fieldSystem.MapHeaderData!.NumObjectEvents;
    }

    // OFFSET 0x0203A4BC
    public static ObjectEvent[]? MapHeaderData_GetObjectEvents(FieldSystem fieldSystem)
    {
        return fieldSystem.MapHeaderData!.ObjectEvents;
    }

    // OFFSET 0x0203A4C4
    public static bool MapHeaderData_SetObjectEventPos(FieldSystem fieldSystem, int localID, int x, int z)
    {
        MapHeaderData data = fieldSystem.MapHeaderData!;
        uint numObjectEvents = data.NumObjectEvents;
        ObjectEvent[]? objectEvents = data.ObjectEvents;
        for (uint i = 0; i < numObjectEvents; i++)
        {
            if (localID == objectEvents![i].LocalID)
            {
                objectEvents[i].X = (ushort)x;
                objectEvents[i].Z = (ushort)z;
                return true;
            }
        }
        ErrorHandling.GF_AssertFail();
        return false;
    }

    // OFFSET 0x0203A4FC
    public static bool MapHeaderData_SetObjectEventDir(FieldSystem fieldSystem, int localID, int dir)
    {
        MapHeaderData data = fieldSystem.MapHeaderData!;
        uint numObjectEvents = data.NumObjectEvents;
        ObjectEvent[]? objectEvents = data.ObjectEvents;
        for (uint i = 0; i < numObjectEvents; i++)
        {
            if (localID == objectEvents![i].LocalID)
            {
                objectEvents[i].Dir = (short)dir;
                return true;
            }
        }
        ErrorHandling.GF_AssertFail();
        return false;
    }

    // OFFSET 0x0203A52C
    public static bool MapHeaderData_SetObjectEventMovementType(FieldSystem fieldSystem, int localID, int movementType)
    {
        MapHeaderData data = fieldSystem.MapHeaderData!;
        uint numObjectEvents = data.NumObjectEvents;
        ObjectEvent[]? objectEvents = data.ObjectEvents;
        for (uint i = 0; i < numObjectEvents; i++)
        {
            if (localID == objectEvents![i].LocalID)
            {
                objectEvents[i].MovementType = (ushort)movementType;
                return true;
            }
        }
        ErrorHandling.GF_AssertFail();
        return false;
    }

    // OFFSET 0x0203A55C
    public static bool MapHeaderData_SetWarpEventPos(FieldSystem fieldSystem, int index, int x, int z)
    {
        WarpEvent warpEvent = fieldSystem.MapHeaderData!.WarpEvents![index];
        warpEvent.X = (ushort)x;
        warpEvent.Z = (ushort)z;
        return true;
    }

    // OFFSET 0x0203A574
    public static bool MapHeaderData_SetWarpEventDestHeaderID(FieldSystem fieldSystem, int index, int destHeaderID)
    {
        fieldSystem.MapHeaderData!.WarpEvents![index].DestHeaderID = (ushort)destHeaderID;
        return true;
    }

    // OFFSET 0x0203A584
    public static bool MapHeaderData_SetWarpEventDestWarpID(FieldSystem fieldSystem, int index, int destWarpID)
    {
        fieldSystem.MapHeaderData!.WarpEvents![index].DestWarpID = (ushort)destWarpID;
        return true;
    }

    // OFFSET 0x0203A594
    public static bool MapHeaderData_SetBgEventPos(FieldSystem fieldSystem, int index, int x, int z)
    {
        BgEvent bgEvent = MapHeaderData_GetBgEvents(fieldSystem)![index];
        bgEvent.X = x;
        bgEvent.Z = z;
        return true;
    }

    // OFFSET 0x0203A5B0
    private static void MapHeaderData_ParseEvents(MapHeaderData data)
    {
        byte[] buffer = data.EventsBuffer;
        int position = 0;
        data.NumBgEvents = Bin.U32(buffer, position);
        position += 4;
        if (data.NumBgEvents != 0)
        {
            data.BgEvents = new BgEvent[data.NumBgEvents];
            for (int i = 0; i < data.BgEvents.Length; i++)
            {
                int offset = position + (i * BgEvent.Size);
                data.BgEvents[i] = new BgEvent
                {
                    Script = (ushort)Bin.U16(buffer, offset),
                    Type = (ushort)Bin.U16(buffer, offset + 2),
                    X = Bin.I32(buffer, offset + 4),
                    Z = Bin.I32(buffer, offset + 8),
                    Y = Bin.I32(buffer, offset + 0xC),
                    PlayerFacingDir = (ushort)Bin.U16(buffer, offset + 0x10),
                };
            }
        }
        else
        {
            data.BgEvents = null;
        }
        position += (int)data.NumBgEvents * BgEvent.Size;
        data.NumObjectEvents = Bin.U32(buffer, position);
        position += 4;
        if (data.NumObjectEvents != 0)
        {
            data.ObjectEvents = new ObjectEvent[data.NumObjectEvents];
            for (int i = 0; i < data.ObjectEvents.Length; i++)
            {
                int offset = position + (i * ObjectEvent.Size);
                ObjectEvent objectEvent = new()
                {
                    LocalID = (ushort)Bin.U16(buffer, offset),
                    GraphicsID = (ushort)Bin.U16(buffer, offset + 2),
                    MovementType = (ushort)Bin.U16(buffer, offset + 4),
                    TrainerType = (ushort)Bin.U16(buffer, offset + 6),
                    HiddenFlag = (ushort)Bin.U16(buffer, offset + 8),
                    Script = (ushort)Bin.U16(buffer, offset + 0xA),
                    Dir = (short)Bin.S16(buffer, offset + 0xC),
                    MovementRangeX = (short)Bin.S16(buffer, offset + 0x14),
                    MovementRangeZ = (short)Bin.S16(buffer, offset + 0x16),
                    X = (ushort)Bin.U16(buffer, offset + 0x18),
                    Z = (ushort)Bin.U16(buffer, offset + 0x1A),
                    Y = Bin.I32(buffer, offset + 0x1C),
                };
                objectEvent.Data[0] = (ushort)Bin.U16(buffer, offset + 0xE);
                objectEvent.Data[1] = (ushort)Bin.U16(buffer, offset + 0x10);
                objectEvent.Data[2] = (ushort)Bin.U16(buffer, offset + 0x12);
                data.ObjectEvents[i] = objectEvent;
            }
        }
        else
        {
            data.ObjectEvents = null;
        }
        position += (int)data.NumObjectEvents * ObjectEvent.Size;
        data.NumWarpEvents = Bin.U32(buffer, position);
        position += 4;
        if (data.NumWarpEvents != 0)
        {
            data.WarpEvents = new WarpEvent[data.NumWarpEvents];
            for (int i = 0; i < data.WarpEvents.Length; i++)
            {
                int offset = position + (i * WarpEvent.Size);
                data.WarpEvents[i] = new WarpEvent
                {
                    X = (ushort)Bin.U16(buffer, offset),
                    Z = (ushort)Bin.U16(buffer, offset + 2),
                    DestHeaderID = (ushort)Bin.U16(buffer, offset + 4),
                    DestWarpID = (ushort)Bin.U16(buffer, offset + 6),
                };
            }
        }
        else
        {
            data.WarpEvents = null;
        }
        position += (int)data.NumWarpEvents * WarpEvent.Size;
        data.NumCoordEvents = Bin.U32(buffer, position);
        position += 4;
        if (data.NumCoordEvents != 0)
        {
            data.CoordEvents = new CoordEvent[data.NumCoordEvents];
            for (int i = 0; i < data.CoordEvents.Length; i++)
            {
                int offset = position + (i * CoordEvent.Size);
                data.CoordEvents[i] = new CoordEvent
                {
                    Script = (ushort)Bin.U16(buffer, offset),
                    X = (ushort)Bin.U16(buffer, offset + 2),
                    Z = (ushort)Bin.U16(buffer, offset + 4),
                    Width = (ushort)Bin.U16(buffer, offset + 6),
                    Length = (ushort)Bin.U16(buffer, offset + 8),
                    Y = (ushort)Bin.U16(buffer, offset + 0xA),
                    Value = (ushort)Bin.U16(buffer, offset + 0xC),
                    Var = (ushort)Bin.U16(buffer, offset + 0xE),
                };
            }
        }
        else
        {
            data.CoordEvents = null;
        }
    }

    // OFFSET 0x0203A614
    public static void MapHeaderData_LoadWildEncounters(byte[] wildEncounters, int mapHeaderID)
    {
        Array.Clear(wildEncounters, 0, WildEncountersSize);
        if (MapHeader.MapHeader_HasWildEncounters(mapHeaderID))
        {
            NARC.NARC_ReadWholeMemberByIndexPair(wildEncounters, NarcWildEncounters, MapHeader.MapHeader_GetWildEncountersArchiveID(mapHeaderID));
        }
    }

    // OFFSET 0x0203A640
    public static byte[] MapHeaderData_GetWildEncounters(FieldSystem fieldSystem)
    {
        return fieldSystem.MapHeaderData!.WildEncounters;
    }

    // OFFSET 0x0203A64C
    private static void MapHeaderData_LoadInitScripts(MapHeaderData data, int mapHeaderID)
    {
        int initScriptsArchiveID = MapHeader.MapHeader_GetInitScriptsArchiveID(mapHeaderID);
        Array.Clear(data.InitScripts, 0, InitScriptsSize);
        if (NARC.NARC_GetMemberSizeByIndexPair(NarcInitScripts, initScriptsArchiveID) >= InitScriptsSize)
        {
            ErrorHandling.GF_AssertFail();
        }
        NARC.NARC_ReadWholeMemberByIndexPair(data.InitScripts, NarcInitScripts, initScriptsArchiveID);
    }

    // OFFSET 0x0203A68C
    public static byte[] MapHeaderData_GetInitScriptBytes(FieldSystem fieldSystem)
    {
        if (fieldSystem.MapHeaderData == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return fieldSystem.MapHeaderData!.InitScripts;
    }

    // OFFSET 0x0203A6A4
    public static bool MapHeaderData_IsPosFreeOfObjectEvents(FieldSystem fieldSystem, int x, int z)
    {
        MapHeaderData data = fieldSystem.MapHeaderData!;
        for (uint i = 0; i < data.NumObjectEvents; i++)
        {
            if (x == data.ObjectEvents![i].X && z == data.ObjectEvents[i].Z)
            {
                return false;
            }
        }
        return true;
    }
}
