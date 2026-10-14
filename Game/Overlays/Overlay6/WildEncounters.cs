using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class WildEncounters
{
    public const int EncounterTypeGrass = 0;
    public const int EncounterTypeSurf = 1;

    private const int GrassEncounterRateOffset = 0x00;
    private const int SurfEncounterRateOffset = 0xCC;

    // OFFSET 0x02240D5C
    public static int WildEncounters_TryWildEncounter(FieldSystem fieldSystem)
    {
        int playerX = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        int playerZ = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        byte tileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, playerX, playerZ);
        byte encounterRate = WildEncounters_GetEncounterRate(fieldSystem, tileBehavior, out byte encounterType);
        if (encounterRate == 0)
        {
            return 0;
        }
        throw Unported.Partial(0x02240D5C);
    }

    // OFFSET 0x02241874
    private static byte WildEncounters_GetEncounterRate(FieldSystem fieldSystem, byte tileBehavior, out byte encounterType)
    {
        encounterType = 0;
        if (TileBehavior.TileBehavior_HasEncounters(tileBehavior) != 0)
        {
            if (TileBehavior.TileBehavior_IsSurfable(tileBehavior) != 0)
            {
                encounterType = EncounterTypeSurf;
                return (byte)WildEncounters_GetSurfEncounterRate(fieldSystem);
            }
            encounterType = EncounterTypeGrass;
            return (byte)WildEncounters_GetGrassEncounterRate(fieldSystem);
        }
        return 0;
    }

    // OFFSET 0x0224214C
    private static int WildEncounters_GetGrassEncounterRate(FieldSystem fieldSystem)
    {
        return (int)Bin.U32(MapHeaderData.MapHeaderData_GetWildEncounters(fieldSystem), GrassEncounterRateOffset);
    }

    // OFFSET 0x02242158
    private static int WildEncounters_GetSurfEncounterRate(FieldSystem fieldSystem)
    {
        return (int)Bin.U32(MapHeaderData.MapHeaderData_GetWildEncounters(fieldSystem), SurfEncounterRateOffset);
    }
}
