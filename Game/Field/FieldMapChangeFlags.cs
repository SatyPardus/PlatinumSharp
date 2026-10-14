namespace PlatinumSharp.Game;

public static class FieldMapChangeFlags
{
    private const int LocationMapIdOffset = 0x00;
    private const int LocationXOffset = 0x08;
    private const int LocationZOffset = 0x0C;
    private const int PlayerDataStateOffset = 0x04;
    private const int PlayerStateWalking = 0;
    private const int PlayerStateCycling = 1;
    private const int PlayerStateSurfing = 2;
    private const int FluteFactorNone = 0;
    private const int JournalHeapID = 0x20;

    // OFFSET 0x02070428
    public static void FieldSystem_SetTemporaryMapChange(FieldSystem fieldSystem, int temporaryMapChange)
    {
        fieldSystem.TemporaryMapChange = temporaryMapChange;
    }

    // OFFSET 0x02070430
    public static void FieldSystem_InitFlagsOnMapChange(FieldSystem fieldSystem)
    {
        CreateFlyLocationJournalEvent(fieldSystem);
        SystemFlags.SystemFlag_HandleForceBikingInGate(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), SystemFlags.HandleFlagClear);
        SystemFlags.SystemFlag_HandleStrengthActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), SystemFlags.HandleFlagClear);
        SpawnLocations.SpawnLocations_TryUnlockFlyLocationByMap(fieldSystem, (int)fieldSystem.Location!.U32(LocationMapIdOffset));
        SpecialEncounter.SpecialEncounter_SetFluteFactor(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!), FluteFactorNone);
        fieldSystem.WildEncounterAttempts = 0;
        if (!SystemFlags.SystemFlag_CheckSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)))
        {
            SpecialEncounter specialEncounter = SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!);
            RoamingPokemon.RoamingPokemon_UpdatePlayerRecentRoutes(specialEncounter, (int)fieldSystem.Location!.U32(LocationMapIdOffset));
            RoamingPokemon.RoamingPokemon_MoveAllLocations(specialEncounter);
        }
    }

    // OFFSET 0x02070494
    public static void FieldSystem_InitFlagsWarp(FieldSystem fieldSystem)
    {
        if (fieldSystem.TemporaryMapChange == 1)
        {
            return;
        }
        if (!CreateFlyLocationJournalEvent(fieldSystem))
        {
            CreateMapTransitionJournalEvent(fieldSystem);
        }
        SystemFlags.SystemFlag_HandleForceBikingInGate(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), SystemFlags.HandleFlagClear);
        SystemFlags.SystemFlag_ClearOnCyclingRoad(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
        SystemFlags.SystemFlag_HandleStrengthActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), SystemFlags.HandleFlagClear);
        SpawnLocations.SpawnLocations_TryUnlockFlyLocationByMap(fieldSystem, (int)fieldSystem.Location!.U32(LocationMapIdOffset));
        SpecialEncounter.SpecialEncounter_SetFluteFactor(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!), FluteFactorNone);
        fieldSystem.WildEncounterAttempts = 0;
        RoamingPokemon.RoamingPokemon_UpdatePlayerRecentRoutes(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!), (int)fieldSystem.Location!.U32(LocationMapIdOffset));
        if (!MapHeader.MapHeader_IsCave((int)fieldSystem.Location!.U32(LocationMapIdOffset)))
        {
            VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
            SystemFlags.SystemFlag_ClearFlashActive(varsFlags);
            SystemFlags.SystemFlag_ClearDefogActive(varsFlags);
        }
        BufferView playerData = FieldPlayerState.FieldPlayerState_GetPlayerData(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        if (playerData.U32(PlayerDataStateOffset) == PlayerStateCycling && !MapHeader.MapHeader_IsBikeAllowed((int)fieldSystem.Location!.U32(LocationMapIdOffset)))
        {
            playerData.W32(PlayerDataStateOffset, PlayerStateWalking);
        }
        else if (playerData.U32(PlayerDataStateOffset) == PlayerStateSurfing)
        {
            playerData.W32(PlayerDataStateOffset, PlayerStateWalking);
        }
        if (MapHeader.MapHeader_IsOnMainMatrix((int)fieldSystem.Location!.U32(LocationMapIdOffset)))
        {
            BufferView mapHistory = FieldPlayerState.FieldPlayerState_GetMapHistory(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
            OverworldMapHistory.OverworldMapHistory_PushViaWarp(mapHistory, (int)fieldSystem.Location!.U32(LocationXOffset), (int)fieldSystem.Location!.U32(LocationZOffset));
        }
    }

    // OFFSET 0x0207056C
    public static void FieldSystem_SetFlyFlags(FieldSystem fieldSystem)
    {
        SystemFlags.SystemFlag_ClearSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
        RoamingPokemon.RoamingPokemon_RandomizeAllLocations(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!));
    }

    // OFFSET 0x02070588
    public static void FieldSystem_SetTeleportFlags(FieldSystem fieldSystem)
    {
        SystemFlags.SystemFlag_ClearSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
        RoamingPokemon.RoamingPokemon_RandomizeAllLocations(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!));
    }

    // OFFSET 0x020705A4
    public static void FieldSystem_SetEscapeFlags(FieldSystem fieldSystem)
    {
        SystemFlags.SystemFlag_ClearSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
    }

    // OFFSET 0x020705B4
    public static void FieldSystem_ClearPartnerTrainer(FieldSystem fieldSystem)
    {
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        SystemFlags.SystemFlag_ClearHasPartner(varsFlags);
        SystemVars.SystemVars_SetPartnerTrainerID(varsFlags, 0);
    }

    // OFFSET 0x020705CC
    public static void FieldSystem_RandomizeRoamingPokemonLocations(FieldSystem fieldSystem)
    {
        RoamingPokemon.RoamingPokemon_RandomizeAllLocations(SpecialEncounter.SaveData_GetSpecialEncounters(fieldSystem.SaveData!));
    }

    // OFFSET 0x020705DC
    private static bool CreateFlyLocationJournalEvent(FieldSystem fieldSystem)
    {
        int flyLocation = SpawnLocations.SpawnLocations_GetMapFlyWarpId((int)fieldSystem.Location!.U32(LocationMapIdOffset));
        if (flyLocation != 0 && !SpawnLocations.SpawnLocations_CheckFlyLocationUnlocked(fieldSystem, flyLocation))
        {
            Journal.JournalEntry_CreateAndSaveEventArrivedInLocation(fieldSystem.JournalEntry, fieldSystem.Location!.U32(LocationMapIdOffset), JournalHeapID);
            return true;
        }
        return false;
    }

    // OFFSET 0x02070610
    private static bool CreateMapTransitionJournalEvent(FieldSystem fieldSystem)
    {
        BufferView previousLocation = FieldPlayerState.FieldPlayerState_GetPreviousLocation(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        int previousMap = (int)previousLocation.U32(LocationMapIdOffset);
        int currentMap = (int)fieldSystem.Location!.U32(LocationMapIdOffset);
        if (previousMap != currentMap)
        {
            Journal.JournalEntry_CreateAndSaveEventMapTransition(PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!), fieldSystem.JournalEntry, currentMap, previousMap, JournalHeapID);
            return true;
        }
        return false;
    }
}
