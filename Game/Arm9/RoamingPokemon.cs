using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class RoamingPokemon
{
    public const int RouteCount = 0x1D;

    private const uint RouteMapIDsAddress = 0x020EFB44;
    private const uint AdjacentRoutesAddress = 0x020EFBB8;
    private const int AdjacentRoutesEntrySize = 0xC;
    private const int AdjacentRoutesEntryWords = 6;
    private const int RandomRouteDivisor = 0x8D4;
    private const int MoveRandomChance = 0x10;
    private const int HeapID = 4;
    private const int SpeciesArticuno = 0x90;
    private const int SpeciesZapdos = 0x91;
    private const int SpeciesMoltres = 0x92;
    private const int SpeciesMesprit = 0x1E1;
    private const int SpeciesCresselia = 0x1E8;
    private const int SpeciesDarkrai = 0x1EB;
    private const int MonDataPersonality = 0;
    private const int MonDataCurrentHP = 0xA4;
    private const int MonDataCombinedIVs = 0xAF;
    private const int RandomIVs = 0x20;
    private const int OTIDTypePreset = 1;

    // OFFSET 0x0206C33C
    public static void RoamingPokemon_MoveToRandomMap(SpecialEncounter specialEncounter, byte roamerSlot)
    {
        int playerPreviousMap = SpecialEncounter.SpecialEncounter_GetPlayerPreviousMap(specialEncounter);
        MoveRoamerRandom(specialEncounter, roamerSlot, playerPreviousMap);
    }

    // OFFSET 0x0206C354
    public static void RoamingPokemon_RandomizeAllLocations(SpecialEncounter specialEncounter)
    {
        for (byte slot = 0; slot < SpecialEncounter.RoamerCount; slot++)
        {
            if (SpecialEncounter.SpecialEncounter_IsRoamerActive(specialEncounter, slot) != 0)
            {
                RoamingPokemon_MoveToRandomMap(specialEncounter, slot);
            }
        }
    }

    // OFFSET 0x0206C37C
    public static void RoamingPokemon_MoveAllLocations(SpecialEncounter specialEncounter)
    {
        for (byte slot = 0; slot < SpecialEncounter.RoamerCount; slot++)
        {
            if (SpecialEncounter.SpecialEncounter_IsRoamerActive(specialEncounter, slot) != 0)
            {
                int roll = (MathUtil.LCRNG_Next() << 4) >> 16;
                if (roll >= MoveRandomChance)
                {
                    ErrorHandling.GF_AssertFail();
                }
                if (roll == 0)
                {
                    RoamingPokemon_MoveToRandomMap(specialEncounter, slot);
                }
                else
                {
                    MoveRoamerNearby(specialEncounter, slot, SpecialEncounter.SpecialEncounter_GetPlayerPreviousMap(specialEncounter));
                }
            }
        }
    }

    // OFFSET 0x0206C3C8
    public static int RoamingPokemon_GetRouteFromId(byte routeId)
    {
        if (routeId >= RouteCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return (int)RomData.Arm9U32(RouteMapIDsAddress, RouteCount)[routeId];
    }

    // OFFSET 0x0206C3E0
    public static bool RoamingPokemon_AnyRoamersActive(SpecialEncounter specialEncounter)
    {
        for (byte slot = 0; slot < SpecialEncounter.RoamerCount; slot++)
        {
            if (SpecialEncounter.SpecialEncounter_IsRoamerActive(specialEncounter, slot) != 0)
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0206C404
    public static void RoamingPokemon_UpdatePlayerRecentRoutes(SpecialEncounter specialEncounter, int newMap)
    {
        if (RoamingPokemon_AnyRoamersActive(specialEncounter))
        {
            SpecialEncounter.SpecialEncounter_UpdateRecentRoutes(specialEncounter, newMap);
        }
    }

    // OFFSET 0x0206C41C
    public static void RoamingPokemon_ActivateSlot(SaveData saveData, byte slot)
    {
        SpecialEncounter specialEncounter = SpecialEncounter.SaveData_GetSpecialEncounters(saveData);
        Roamer roamer = SpecialEncounter.SpecialEncounter_GetRoamer(specialEncounter, slot);
        int species;
        int level;
        switch (slot)
        {
            case 0:
                species = SpeciesMesprit;
                level = 50;
                break;
            case 1:
                species = SpeciesCresselia;
                level = 50;
                break;
            case 2:
                species = SpeciesDarkrai;
                level = 40;
                break;
            case 3:
                species = SpeciesMoltres;
                level = 60;
                break;
            case 4:
                species = SpeciesZapdos;
                level = 60;
                break;
            case 5:
                species = SpeciesArticuno;
                level = 60;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                return;
        }
        Roamer.Roamer_SetData(roamer, Roamer.DataSpecies, (uint)species);
        Roamer.Roamer_SetData(roamer, Roamer.DataLevel, (uint)level);
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(saveData);
        Pokemon mon = Pokemon.Pokemon_New(HeapID);
        Pokemon.Pokemon_Init(mon);
        uint trainerID = TrainerInfo.TrainerInfo_ID_LowHalf(trainerInfo);
        Pokemon.Pokemon_InitWith(mon, species, level, RandomIVs, 0, 0, OTIDTypePreset, trainerID);
        Roamer.Roamer_SetData(roamer, Roamer.DataStatus, 0);
        Roamer.Roamer_SetData(roamer, Roamer.DataActive, 1);
        Roamer.Roamer_SetData(roamer, Roamer.DataIVs, Pokemon.Pokemon_GetValue(mon, MonDataCombinedIVs, null));
        Roamer.Roamer_SetData(roamer, Roamer.DataPersonality, Pokemon.Pokemon_GetValue(mon, MonDataPersonality, null));
        Roamer.Roamer_SetData(roamer, Roamer.DataCurrentHP, Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null));
        MoveRoamerRandom(specialEncounter, slot, SpecialEncounter.SpecialEncounter_GetPlayerPreviousMap(specialEncounter));
    }

    // OFFSET 0x0206C538
    private static void MoveRoamerRandom(SpecialEncounter specialEncounter, byte roamerSlot, int playerPreviousMap)
    {
        uint[] routeMapIDs = RomData.Arm9U32(RouteMapIDsAddress, RouteCount);
        int currentMap = (int)routeMapIDs[SpecialEncounter.SpecialEncounter_GetRoamerRouteIndex(specialEncounter, roamerSlot)];
        while (true)
        {
            ushort routeIndex = (ushort)(MathUtil.LCRNG_Next() / RandomRouteDivisor);
            if (routeIndex >= RouteCount)
            {
                ErrorHandling.GF_AssertFail();
            }
            int map = (int)routeMapIDs[(byte)routeIndex];
            if (map == playerPreviousMap || map == currentMap)
            {
                continue;
            }
            SetNewMapLocation(specialEncounter, roamerSlot, (byte)routeIndex, map);
            return;
        }
    }

    // OFFSET 0x0206C588
    private static void MoveRoamerNearby(SpecialEncounter specialEncounter, byte roamerSlot, int playerPreviousMap)
    {
        uint[] routeMapIDs = RomData.Arm9U32(RouteMapIDsAddress, RouteCount);
        int currentRouteIndex = SpecialEncounter.SpecialEncounter_GetRoamerRouteIndex(specialEncounter, roamerSlot);
        ushort[] adjacent = RomData.Arm9U16((uint)(AdjacentRoutesAddress + currentRouteIndex * AdjacentRoutesEntrySize), AdjacentRoutesEntryWords);
        if (adjacent[0] == 1)
        {
            byte onlyRouteIndex = (byte)adjacent[1];
            int onlyMap = (int)routeMapIDs[onlyRouteIndex];
            if (onlyMap == playerPreviousMap)
            {
                MoveRoamerRandom(specialEncounter, roamerSlot, playerPreviousMap);
            }
            else
            {
                SetNewMapLocation(specialEncounter, roamerSlot, onlyRouteIndex, onlyMap);
            }
            return;
        }
        while (true)
        {
            ushort count = adjacent[0];
            if (count == 0)
            {
                ErrorHandling.GF_AssertFail();
            }
            ushort pick;
            if (count <= 1)
            {
                pick = 0;
            }
            else
            {
                ushort divisor = (ushort)(0xFFFF / count + 1);
                pick = (ushort)(MathUtil.LCRNG_Next() / divisor);
                if (pick >= count)
                {
                    ErrorHandling.GF_AssertFail();
                }
            }
            byte routeIndex = (byte)adjacent[1 + (byte)pick];
            int map = (int)routeMapIDs[routeIndex];
            if (map == playerPreviousMap)
            {
                continue;
            }
            SetNewMapLocation(specialEncounter, roamerSlot, routeIndex, map);
            return;
        }
    }

    // OFFSET 0x0206C638
    private static void SetNewMapLocation(SpecialEncounter specialEncounter, byte roamerSlot, byte newMapIndex, int newMapId)
    {
        Roamer roamer = SpecialEncounter.SpecialEncounter_GetRoamer(specialEncounter, roamerSlot);
        SpecialEncounter.SpecialEncounter_SetRoamerRouteIndex(specialEncounter, roamerSlot, newMapIndex);
        Roamer.Roamer_SetData(roamer, Roamer.DataMapID, (uint)newMapId);
    }
}
