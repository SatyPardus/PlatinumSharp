using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class SpawnLocations
{
    public const int Count = 20;

    private const uint TableAddress = 0x020E97B4;
    private const int EntrySize = 0x10;
    private const int BlackOutMapIdOffset = 0x00;
    private const int BlackOutXOffset = 0x02;
    private const int BlackOutZOffset = 0x04;
    private const int FlyMapIdOffset = 0x06;
    private const int FlyXOffset = 0x08;
    private const int FlyZOffset = 0x0A;
    private const int IsWarpPositionOffset = 0x0C;
    private const int UnlockOnMapEntryOffset = 0x0D;
    private const int FirstArrivalOffset = 0x0E;
    private const int FaceDirectionNorth = 0;
    private const int FaceDirectionSouth = 1;
    private const int FlagOperationSet = 1;
    private const int FlagOperationCheck = 2;
    private const int MapBlockTiles = 32;

    private static byte[]? sTable;

    private static byte[] Table
    {
        get
        {
            sTable ??= RomData.Arm9(TableAddress, Count * EntrySize);
            return sTable;
        }
    }

    private static int Entry16(int index, int offset)
    {
        return Bin.U16(Table, index * EntrySize + offset);
    }

    private static int Entry8(int index, int offset)
    {
        return Table[index * EntrySize + offset];
    }

    // OFFSET 0x0203A7D8
    private static int SpawnLocations_IdToIndex(int spawnId)
    {
        if (spawnId <= 0 || (uint)spawnId > Count)
        {
            ErrorHandling.GF_AssertFail();
            spawnId = 1;
        }
        return spawnId - 1;
    }

    // OFFSET 0x0203A7F0
    public static void Location_InitFly(int flyDestination, BufferView location)
    {
        int index = SpawnLocations_IdToIndex(flyDestination);
        location.W32(0x00, (uint)Entry16(index, FlyMapIdOffset));
        location.W32(0x04, unchecked((uint)Location.WarpIdNone));
        location.W32(0x08, (uint)Entry16(index, FlyXOffset));
        location.W32(0x0C, (uint)Entry16(index, FlyZOffset));
        location.W32(0x10, FaceDirectionSouth);
    }

    // OFFSET 0x0203A824
    public static void Location_InitBlackOut(int blackOutDestination, BufferView location)
    {
        int index = SpawnLocations_IdToIndex(blackOutDestination);
        location.W32(0x00, (uint)Entry16(index, BlackOutMapIdOffset));
        location.W32(0x04, unchecked((uint)Location.WarpIdNone));
        location.W32(0x08, (uint)Entry16(index, BlackOutXOffset));
        location.W32(0x0C, (uint)Entry16(index, BlackOutZOffset));
        location.W32(0x10, FaceDirectionNorth);
    }

    // OFFSET 0x0203A858
    public static int SpawnLocations_GetMapBlackOutWarpId(int mapHeaderID)
    {
        for (uint i = 0; i < Count; i++)
        {
            if (mapHeaderID == Entry16((int)i, BlackOutMapIdOffset) && Entry8((int)i, IsWarpPositionOffset) != 0)
            {
                return (int)i + 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0203A87C
    public static int SpawnLocations_GetMapFlyWarpId(int mapHeaderID)
    {
        for (uint i = 0; i < Count; i++)
        {
            if (mapHeaderID == Entry16((int)i, FlyMapIdOffset) && Entry8((int)i, IsWarpPositionOffset) != 0)
            {
                return (int)i + 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0203A8A0
    public static int SpawnLocations_GetSpawnIdByMapAndCoords(int mapHeaderID, int x, int z)
    {
        int blockX = x / MapBlockTiles;
        int blockZ = z / MapBlockTiles;
        int spawnId = 0;
        for (uint i = 0; i < Count; i++)
        {
            if (mapHeaderID == Entry16((int)i, FlyMapIdOffset))
            {
                spawnId = (int)i + 1;
                if (blockX == Entry16((int)i, FlyXOffset) / MapBlockTiles && blockZ == Entry16((int)i, FlyZOffset) / MapBlockTiles)
                {
                    return spawnId;
                }
            }
        }
        return spawnId;
    }

    // OFFSET 0x0203A8E8
    public static void SpawnLocations_TryUnlockFlyLocationByMap(FieldSystem fieldSystem, int mapHeaderID)
    {
        for (uint i = 0; i < Count; i++)
        {
            if (mapHeaderID == Entry16((int)i, FlyMapIdOffset) && Entry8((int)i, UnlockOnMapEntryOffset) != 0)
            {
                SystemFlags.SystemFlag_HandleFirstArrivalToZone(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), FlagOperationSet, (uint)Entry16((int)i, FirstArrivalOffset));
                return;
            }
        }
    }

    // OFFSET 0x0203A920
    public static bool SpawnLocations_CheckFlyLocationUnlocked(FieldSystem fieldSystem, int flyLocation)
    {
        int index = SpawnLocations_IdToIndex(flyLocation);
        return SystemFlags.SystemFlag_HandleFirstArrivalToZone(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), FlagOperationCheck, (uint)Entry16(index, FirstArrivalOffset));
    }
}
