using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class MapHeader
{
    private const int MapNameNarc = 0x1A;
    private const int MapNameBank = 0x1B1;
    public const int Count = 0x251;
    public const int EncountersNone = 0xFFFF;

    public const int MapTypeTown = 1;
    public const int MapTypeRoute = 2;
    public const int MapTypeCave = 3;
    public const int MapTypeBuilding = 4;
    public const int MapTypePokemonCenter = 5;

    private const uint MapHeaderTableAddress = 0x020E601C;
    private const int MapHeaderSize = 0x18;
    private const uint PokemonCenter2FTableAddress = 0x020E5FA4;
    private const int PokemonCenter2FCount = 0x12;
    private const uint MapEvolutionMethodTableAddress = 0x020E5FC8;
    private const int MapEvolutionMethodTableLength = 0x2A;
    private const uint GameVersionAddress = 0x020E4C40;
    private const int VersionPearl = 0xB;
    private const int MapHeaderIDUnionRoom = 0x1D2;
    private const int MapHeaderIDLakeWithFeebas = 0xDB;
    private const int MapHeaderIDTrophyGarden = 0x11F;
    private const int MapHeaderIDAmitySquare = 0xFD;
    private const int MapHeaderIDSpearPillar = 0xDC;
    private const int MapHeaderIDSpearPillarVariants = 0x248;

    private static byte[]? sMapHeaders;

    private static int Field8(int mapHeaderID, int offset)
    {
        sMapHeaders ??= RomData.Arm9(MapHeaderTableAddress, Count * MapHeaderSize);
        return sMapHeaders[(MapHeader_IDBoundsCheck(mapHeaderID) * MapHeaderSize) + offset];
    }

    private static int Field16(int mapHeaderID, int offset)
    {
        sMapHeaders ??= RomData.Arm9(MapHeaderTableAddress, Count * MapHeaderSize);
        return Bin.U16(sMapHeaders, (MapHeader_IDBoundsCheck(mapHeaderID) * MapHeaderSize) + offset);
    }

    // OFFSET 0x0203A024
    public static int MapHeader_IDBoundsCheck(int mapHeaderID)
    {
        if ((uint)mapHeaderID >= Count)
        {
            ErrorHandling.GF_AssertFail();
            return 3;
        }
        return mapHeaderID;
    }

    // OFFSET 0x0203A038
    public static int MapHeader_GetAreaDataArchiveID(int mapHeaderID)
    {
        return Field8(mapHeaderID, 0);
    }

    // OFFSET 0x0203A04C
    public static int MapHeader_GetPreloadedMapObjectsArchiveID(int mapHeaderID)
    {
        return Field8(mapHeaderID, 1);
    }

    // OFFSET 0x0203A060
    public static int MapHeader_GetMapMatrixID(int mapHeaderID)
    {
        int mapMatrixID = Field16(mapHeaderID, 2);
        if (mapMatrixID == 0x16)
        {
            if (RomData.Arm9(GameVersionAddress, 1)[0] == VersionPearl)
            {
                mapMatrixID = 0x17;
            }
        }
        return mapMatrixID;
    }

    // OFFSET 0x0203A088
    public static int MapHeader_GetMsgArchiveID(int mapHeaderID)
    {
        return Field16(mapHeaderID, 8);
    }

    // OFFSET 0x0203A09C
    public static int MapHeader_GetScriptsArchiveID(int mapHeaderID)
    {
        return Field16(mapHeaderID, 4);
    }

    // OFFSET 0x0203A0B0
    public static int MapHeader_GetInitScriptsArchiveID(int mapHeaderID)
    {
        return Field16(mapHeaderID, 6);
    }

    // OFFSET 0x0203A0C4
    public static int MapHeader_GetDayMusicID(int mapHeaderID)
    {
        return Field16(mapHeaderID, 0xA);
    }

    // OFFSET 0x0203A0D8
    public static int MapHeader_GetNightMusicID(int mapHeaderID)
    {
        return Field16(mapHeaderID, 0xC);
    }

    // OFFSET 0x0203A0EC
    public static bool MapHeader_HasWildEncounters(int mapHeaderID)
    {
        return Field16(mapHeaderID, 0xE) != EncountersNone;
    }

    // OFFSET 0x0203A110
    public static int MapHeader_GetWildEncountersArchiveID(int mapHeaderID)
    {
        return Field16(mapHeaderID, 0xE);
    }

    // OFFSET 0x0203A124
    public static int MapHeader_GetEventsArchiveID(int mapHeaderID)
    {
        return Field16(mapHeaderID, 0x10);
    }

    // OFFSET 0x0203A138
    public static int MapHeader_GetMapLabelTextID(int mapHeaderID)
    {
        return Field16(mapHeaderID, 0x12) & 0xFF;
    }

    // OFFSET 0x0203A154
    public static int MapHeader_GetMapLabelWindowID(int mapHeaderID)
    {
        return (Field16(mapHeaderID, 0x12) >> 8) & 0xFF;
    }

    // OFFSET 0x0203A174
    public static int MapHeader_GetWeatherType(int mapHeaderID)
    {
        return Field8(mapHeaderID, 0x14);
    }

    // OFFSET 0x0203A188
    public static int MapHeader_GetCameraType(int mapHeaderID)
    {
        return Field8(mapHeaderID, 0x15);
    }

    // OFFSET 0x0203A19C
    public static int MapHeader_GetBattleBG(int mapHeaderID)
    {
        return (Field16(mapHeaderID, 0x16) >> 7) & 0x1F;
    }

    // OFFSET 0x0203A1B8
    public static bool MapHeader_IsEscapeRopeAllowed(int mapHeaderID)
    {
        return ((Field16(mapHeaderID, 0x16) >> 14) & 1) != 0;
    }

    // OFFSET 0x0203A1D4
    public static bool MapHeader_IsFlyAllowed(int mapHeaderID)
    {
        return ((Field16(mapHeaderID, 0x16) >> 15) & 1) != 0;
    }

    // OFFSET 0x0203A1F0
    public static bool MapHeader_IsBikeAllowed(int mapHeaderID)
    {
        return ((Field16(mapHeaderID, 0x16) >> 12) & 1) != 0;
    }

    // OFFSET 0x0203A20C
    public static int MapHeader_GetMapType(int mapHeaderID)
    {
        return Field16(mapHeaderID, 0x16) & 0x7F;
    }

    // OFFSET 0x0203A228
    public static bool MapHeader_IsTeleportAllowed(int mapHeaderID)
    {
        if (!MapHeader_IsFlyAllowed(mapHeaderID))
        {
            return false;
        }
        if (MapHeader_GetMapType(mapHeaderID) != MapTypeTown)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0203A24C
    public static bool MapHeader_IsOnMainMatrix(int mapHeaderID)
    {
        return MapHeader_GetMapMatrixID(mapHeaderID) == 0;
    }

    // OFFSET 0x0203A260
    public static bool MapHeader_IsPokemonCenter(int mapHeaderID)
    {
        return MapHeader_GetMapType(mapHeaderID) == MapTypePokemonCenter;
    }

    // OFFSET 0x0203A274
    public static bool MapHeader_IsCave(int mapHeaderID)
    {
        return MapHeader_GetMapType(mapHeaderID) == MapTypeCave;
    }

    // OFFSET 0x0203A288
    public static bool MapHeader_IsBuilding(int mapHeaderID)
    {
        if (MapHeader_GetMapType(mapHeaderID) == MapTypeBuilding || MapHeader_GetMapType(mapHeaderID) == MapTypePokemonCenter)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0203A2A8
    public static bool MapHeader_IsOutdoors(int mapHeaderID)
    {
        if (MapHeader_GetMapType(mapHeaderID) == MapTypeTown || MapHeader_GetMapType(mapHeaderID) == MapTypeRoute)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0203A2C8
    public static bool MapHeader_IsPokemonCenter2(int mapHeaderID)
    {
        if (MapHeader_IsPokemonCenter(mapHeaderID))
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0203A2DC
    public static bool MapHeader_IsUnionRoom(int mapHeaderID)
    {
        return mapHeaderID == MapHeaderIDUnionRoom;
    }

    // OFFSET 0x0203A2F0
    public static bool MapHeader_HasFeebasTiles(int mapHeaderID)
    {
        return mapHeaderID == MapHeaderIDLakeWithFeebas;
    }

    // OFFSET 0x0203A2FC
    public static bool MapHeader_IsTrophyGarden(int mapHeaderID)
    {
        return mapHeaderID == MapHeaderIDTrophyGarden;
    }

    // OFFSET 0x0203A310
    public static bool MapHeader_IsAmitySquare(int mapHeaderID)
    {
        return mapHeaderID == MapHeaderIDAmitySquare;
    }

    // OFFSET 0x0203A31C
    public static bool MapHeader_IsAzureFluteAllowed(int mapHeaderID)
    {
        if (mapHeaderID == MapHeaderIDSpearPillar || (uint)(mapHeaderID - MapHeaderIDSpearPillarVariants) <= 1)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0203A334
    public static bool MapHeader_IsPokemonCenter2F(int mapHeaderID)
    {
        ushort[] table = RomData.Arm9U16(PokemonCenter2FTableAddress, PokemonCenter2FCount);
        for (uint i = 0; i < PokemonCenter2FCount; i++)
        {
            if (mapHeaderID == table[i])
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0203A354
    public static int MapHeader_GetMapEvolutionMethod(int mapHeaderID)
    {
        ushort[] table = RomData.Arm9U16(MapEvolutionMethodTableAddress, MapEvolutionMethodTableLength);
        for (uint i = 0; i < MapEvolutionMethodTableLength; i += 2)
        {
            if (mapHeaderID == table[i])
            {
                return table[i + 1];
            }
        }
        return 0;
    }

    // OFFSET 0x02071D10
    public static void MapHeader_LoadName(int mapHeaderID, int heapID, GameString dst)
    {
        MessageLoader loader = MessageLoader.MessageLoader_Init(1, MapNameNarc, MapNameBank, heapID)!;
        LocationName.LocationName_LoadAndGetWidth(loader, MapHeader_GetMapLabelTextID(mapHeaderID), dst);
        MessageLoader.MessageLoader_Free(loader);
    }
}
