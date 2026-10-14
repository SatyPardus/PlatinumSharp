using static PlatinumSharp.Game.Globals;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class Location : BufferView
{
    public const int Size = 0x14;
    public const int WarpIdNone = -1;

    private const int MapIdOffset = 0x00;
    private const int WarpIdOffset = 0x04;
    private const int XOffset = 0x08;
    private const int ZOffset = 0x0C;
    private const int FaceDirectionOffset = 0x10;

    public Location()
        : base(new byte[Size], 0)
    {
    }

    public Location(BufferView view)
        : base(view.Buffer, view.Offset)
    {
    }

    public Location(int mapId, int warpId, int x, int z, int faceDirection)
        : this()
    {
        MapId = mapId;
        WarpId = warpId;
        X = x;
        Z = z;
        FaceDirection = faceDirection;
    }

    public int MapId
    {
        get => (int)U32(MapIdOffset);
        set => W32(MapIdOffset, (uint)value);
    }

    public int WarpId
    {
        get => (int)U32(WarpIdOffset);
        set => W32(WarpIdOffset, (uint)value);
    }

    public int X
    {
        get => (int)U32(XOffset);
        set => W32(XOffset, (uint)value);
    }

    public int Z
    {
        get => (int)U32(ZOffset);
        set => W32(ZOffset, (uint)value);
    }

    public int FaceDirection
    {
        get => (int)U32(FaceDirectionOffset);
        set => W32(FaceDirectionOffset, (uint)value);
    }

    public static Location Of(BufferView view)
    {
        return view as Location ?? new Location(view);
    }

    public void Assign(BufferView source)
    {
        CopyFrom(0, source.Buffer, source.Offset, Size);
    }
}

public sealed class MapLoadMode
{
    public const int Size = 8;

    public int FieldBottomScreen;
    public int SkipMapAttributes;
    public int UseSimpleTerrainCollisions;
    public int WhichScreenIs3D;
    public int UseSeparateTerrainAttributes;
    public int Unknown20;
    public int SeparateTerrainAttributesBlockCount;
    public uint FieldMapHeapSize;

    public MapLoadMode(uint address)
    {
        uint[] words = RomData.Arm9U32(address, 2);
        FieldBottomScreen = (int)(words[0] & 0xF);
        SkipMapAttributes = (int)((words[0] >> 4) & 0xF);
        UseSimpleTerrainCollisions = (int)((words[0] >> 8) & 0xF);
        WhichScreenIs3D = (int)((words[0] >> 12) & 0xF);
        UseSeparateTerrainAttributes = (int)((words[0] >> 16) & 0xF);
        Unknown20 = (int)((words[0] >> 20) & 0xF);
        SeparateTerrainAttributesBlockCount = (int)(words[0] >> 24);
        FieldMapHeapSize = words[1];
    }
}

public sealed class MapChangeData
{
    public int State;
    public Location NextLocation = new();
    public object? Unknown18;
}

public sealed class MapChangeSubData
{
    public int State;
    public Location NextLocation = new();
}

public sealed class MapChangeFromErrorData
{
    public int FinishedFlag;
    public Location Location = new();
}

public static class FieldMapChange
{
    private const int HeapIDFieldSystem = 0xB;
    private const int MapHeaderBattleTower = 0x146;
    private const int MapHeaderBattleTowerLast = 0x14B;
    private const int MapHeaderBattleTowerBattleSalon = 0x1ED;
    private const int MapHeaderUnionRoom = 0x1D2;
    private const int MapLoadTypeOverworld = 0;
    private const int MapLoadTypeUnion = 2;
    private const int MapLoadTypeBattleTower = 4;
    private const int MapLoadTypeMax = 5;
    private const uint MapLoadModeTable = 0x020EC3A8;
    private const int FieldBottomScreenNone = 5;
    private const int WeatherClear = 0;
    private const int WeatherFog = 14;
    private const int WeatherDarkFlash = 16;
    private const int InitScriptOnTransition = 2;
    private const int HiddenLocationSpringPath = 2;
    private const int HiddenLocationSeabreakPath = 3;
    private const int HandleFlagCheck = 2;
    private const int MapObjectMax = 0x40;
    private const int MapObjectTaskPriority = 5;
    private const int DirectionSouth = 1;
    private const int WarpDestinationSpecial = 0x100;
    private const int UnionRoomEntranceX = 7;
    private const int UnionRoomEntranceZ = 6;
    private const int UnionRoomExitX = 8;
    private const int UnionRoomExitZ = 2;
    private const int UnionRoomSpawnX = 8;
    private const int UnionRoomSpawnZ = 14;
    private const int PlayerDataStateOffset = 4;
    private const int SoundEffectStairs = 0x603;
    private const int ScreenMain = 0;
    private const int ScreenSub = 1;
    private const int ColorBlack = 0;

    private static Location CurrentLocation(FieldSystem fieldSystem)
    {
        return Location.Of(fieldSystem.Location!);
    }

    // OFFSET 0x020530C8
    private static void FieldMapChange_UpdateBattleTowerLoadType(FieldSystem fieldSystem)
    {
        int mapId = CurrentLocation(fieldSystem).MapId;
        bool battleTower;
        if (mapId > MapHeaderBattleTowerLast)
        {
            battleTower = mapId == MapHeaderBattleTowerBattleSalon;
        }
        else
        {
            battleTower = mapId >= MapHeaderBattleTower;
        }
        if (!battleTower && fieldSystem.MapLoadType == MapLoadTypeBattleTower)
        {
            fieldSystem.MapLoadType = MapLoadTypeOverworld;
        }
        if (battleTower)
        {
            fieldSystem.MapLoadType = MapLoadTypeBattleTower;
        }
    }

    // OFFSET 0x0205311C
    private static void FieldMapChange_SetNewLocation(FieldSystem fieldSystem, Location? nextLocation)
    {
        FieldPlayerState playerState = FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!);
        Location previous = Location.Of(FieldPlayerState.FieldPlayerState_GetPreviousLocation(playerState));
        Location location = CurrentLocation(fieldSystem);
        if (nextLocation != null)
        {
            previous.Assign(location);
            location.Assign(nextLocation);
        }
        MapHeaderData.MapHeaderData_Load(fieldSystem, location.MapId);
        if (location.WarpId != Location.WarpIdNone)
        {
            WarpEvent warp = MapHeaderData.MapHeaderData_GetWarpEventByIndex(fieldSystem, location.WarpId)!;
            location.X = warp.X;
            location.Z = warp.Z;
            if (warp.DestWarpID == WarpDestinationSpecial)
            {
                Location special = Location.Of(FieldPlayerState.FieldPlayerState_GetSpecialLocation(playerState));
                Location entrance = Location.Of(FieldPlayerState.FieldPlayerState_GetEntranceLocation(playerState));
                special.Assign(entrance);
            }
        }
    }

    // OFFSET 0x020531A0
    public static void FieldMapChange_Set3DDisplay(FieldSystem fieldSystem)
    {
        if (fieldSystem.MapLoadType >= MapLoadTypeMax)
        {
            ErrorHandling.GF_AssertFail();
        }
        gSystem.WhichScreenIs3D = (byte)fieldSystem.MapLoadMode!.WhichScreenIs3D;
    }

    // OFFSET 0x020531C0
    public static void FieldMapChange_UpdateGameData(FieldSystem fieldSystem, bool noWarp)
    {
        int mapId = CurrentLocation(fieldSystem).MapId;
        FieldPlayerState playerState = FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!);
        FieldBgm.FieldBGM_ClearOverride(fieldSystem);
        FieldSystemVarsFlags.FieldSystem_ClearLocalFlags(fieldSystem);
        if (!noWarp)
        {
            FieldMapChangeFlags.FieldSystem_InitFlagsWarp(fieldSystem);
        }
        else
        {
            FieldMapChangeFlags.FieldSystem_InitFlagsOnMapChange(fieldSystem);
        }
        SystemVars.SystemVars_ResetVsSeeker(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
        if (!noWarp)
        {
            FieldTime.FieldSystem_UpdateGameTime(fieldSystem);
        }
        if (!noWarp)
        {
            PersistedMapFeatures.PersistedMapFeatures_Init(MiscSaveBlock.SaveData_GetPersistedMapFeatures(fieldSystem.SaveData!));
        }
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        int weather = (ushort)FieldOverworldWeather.FieldSystem_GetWeather(fieldSystem, mapId);
        if ((weather == WeatherFog && SystemFlags.SystemFlag_CheckDefogActive(varsFlags)) || (weather == WeatherDarkFlash && SystemFlags.SystemFlag_CheckFlashActive(varsFlags)))
        {
            weather = WeatherClear;
        }
        FieldPlayerState.FieldPlayerState_SetWeather(playerState, weather);
        if (noWarp)
        {
            if (FieldPlayerState.FieldPlayerState_GetCameraType(playerState) != MapHeader.MapHeader_GetCameraType(mapId))
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        else
        {
            FieldPlayerState.FieldPlayerState_SetCameraType(playerState, MapHeader.MapHeader_GetCameraType(mapId));
        }
        if (!noWarp)
        {
            int blackOutWarpId = (ushort)SpawnLocations.SpawnLocations_GetMapBlackOutWarpId(mapId);
            if (blackOutWarpId != 0)
            {
                FieldPlayerState.FieldPlayerState_SetBlackOutWarpId(playerState, blackOutWarpId);
            }
        }
        ScriptManager.FieldSystem_RunInitScript(fieldSystem, InitScriptOnTransition);
        fieldSystem.WildEncounterAttempts = 0;
        fieldSystem.WildMonsDefeated = 0;
    }

    // OFFSET 0x020532A8
    public static void FieldMapChange_UpdateGameDataDistortionWorld(FieldSystem fieldSystem, bool noWarp)
    {
        int mapId = CurrentLocation(fieldSystem).MapId;
        FieldPlayerState playerState = FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!);
        FieldBgm.FieldBGM_ClearOverride(fieldSystem);
        FieldSystemVarsFlags.FieldSystem_ClearLocalFlags(fieldSystem);
        if (!noWarp)
        {
            FieldMapChangeFlags.FieldSystem_InitFlagsWarp(fieldSystem);
        }
        else
        {
            FieldMapChangeFlags.FieldSystem_InitFlagsOnMapChange(fieldSystem);
        }
        SystemVars.SystemVars_ResetVsSeeker(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!));
        if (!noWarp)
        {
            FieldTime.FieldSystem_UpdateGameTime(fieldSystem);
        }
        if (!noWarp)
        {
            PersistedMapFeatures.PersistedMapFeatures_Init(MiscSaveBlock.SaveData_GetPersistedMapFeatures(fieldSystem.SaveData!));
        }
        if (!noWarp)
        {
            int blackOutWarpId = (ushort)SpawnLocations.SpawnLocations_GetMapBlackOutWarpId(mapId);
            if (blackOutWarpId != 0)
            {
                FieldPlayerState.FieldPlayerState_SetBlackOutWarpId(playerState, blackOutWarpId);
            }
        }
        fieldSystem.WildEncounterAttempts = 0;
        fieldSystem.WildMonsDefeated = 0;
    }

    // OFFSET 0x02053320
    private static void FieldMapChange_CreateObjects(FieldSystem fieldSystem)
    {
        fieldSystem.MapObjectManager = MapObjectManager.MapObjectMan_New(fieldSystem, MapObjectMax, MapObjectTaskPriority);
        int gender = (int)TrainerInfo.TrainerInfo_Gender(PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!));
        BufferView playerData = FieldPlayerState.FieldPlayerState_GetPlayerData(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        Location location = CurrentLocation(fieldSystem);
        fieldSystem.PlayerAvatar = PlayerAvatar.PlayerAvatar_New(fieldSystem.MapObjectManager, location.X, location.Z, location.FaceDirection, (int)playerData.U32(PlayerDataStateOffset), gender, 0, playerData);
        MapHeaderData.MapHeaderData_LoadObjectEventsIntoMapObjects(fieldSystem);
        MapObjectManager.MapObjectMan_StopAllMovement(fieldSystem.MapObjectManager);
    }

    // OFFSET 0x02053374
    private static void FieldMapChange_DeleteObjects(FieldSystem fieldSystem)
    {
        PlayerAvatar.PlayerAvatar_Free(fieldSystem.PlayerAvatar);
        MapObjectManager.MapObjectMan_DeleteAll(fieldSystem.MapObjectManager);
        MapObjectManager.MapObjectMan_Delete(fieldSystem.MapObjectManager);
    }

    // OFFSET 0x0205338C
    private static void FieldMapChange_LoadObjects(FieldSystem fieldSystem)
    {
        fieldSystem.MapObjectManager = MapObjectManager.MapObjectMan_New(fieldSystem, MapObjectMax, MapObjectTaskPriority);
        FieldOverworldSave.FieldSystem_LoadObjects(fieldSystem);
        BufferView playerData = FieldPlayerState.FieldPlayerState_GetPlayerData(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        int gender = (int)TrainerInfo.TrainerInfo_Gender(PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!));
        fieldSystem.PlayerAvatar = PlayerAvatar.PlayerAvatar_NewLoad(fieldSystem.MapObjectManager, playerData, gender);
        MapObjectManager.MapObjectMan_StopAllMovement(fieldSystem.MapObjectManager);
    }

    // OFFSET 0x020533CC
    private static void FieldMapChange_InitTerrainCollisionManager(FieldSystem fieldSystem)
    {
        FieldMapChange_UpdateBattleTowerLoadType(fieldSystem);
        if (fieldSystem.TerrainCollisionManager != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapMatrix mapMatrix = (MapMatrix)fieldSystem.MapMatrix!;
        MapMatrix.MapMatrix_Load(CurrentLocation(fieldSystem).MapId, mapMatrix);
        if (SystemVars.SystemVars_CheckHiddenLocation(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), HiddenLocationSeabreakPath))
        {
            MapMatrix.MapMatrix_RevealSeabreakPath(mapMatrix);
        }
        if (!SystemVars.SystemVars_CheckHiddenLocation(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), HiddenLocationSpringPath))
        {
            MapMatrix.MapMatrix_RevealSpringPath(mapMatrix);
        }
        if (fieldSystem.MapLoadType >= MapLoadTypeMax)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapLoadMode mode = new(MapLoadModeTable + (uint)(fieldSystem.MapLoadType * MapLoadMode.Size));
        fieldSystem.MapLoadMode = mode;
        fieldSystem.SkipMapAttributes = mode.SkipMapAttributes;
        fieldSystem.BottomScreen = mode.FieldBottomScreen;
        TerrainCollisionManager.TerrainCollisionManager_Init(out fieldSystem.TerrainCollisionManager, mode.UseSimpleTerrainCollisions);
        if (mode.UseSeparateTerrainAttributes != 0)
        {
            TerrainAttributes.TerrainAttributes_New(fieldSystem, mode.SeparateTerrainAttributesBlockCount);
        }
    }

    // OFFSET 0x02053468
    private static void FieldMapChange_RemoveTerrainCollisionManager(FieldSystem fieldSystem)
    {
        if (fieldSystem.TerrainCollisionManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        fieldSystem.TerrainCollisionManager = null;
        fieldSystem.BottomScreen = FieldBottomScreenNone;
        if (fieldSystem.MapLoadMode!.UseSeparateTerrainAttributes != 0)
        {
            TerrainAttributes.TerrainAttributes_Free(fieldSystem);
        }
        fieldSystem.MapLoadMode = null;
    }

    // OFFSET 0x02053494
    public static void FieldMapChange_SaveJournalTitleForCurrentMap(FieldSystem fieldSystem)
    {
        if (fieldSystem.JournalEntry != null)
        {
            uint title = Journal.JournalEntry_CreateTitle((ushort)CurrentLocation(fieldSystem).MapId, HeapIDFieldSystem);
            Journal.JournalEntry_SaveData(fieldSystem.JournalEntry, title, Journal.DataDate);
        }
    }

    // OFFSET 0x020534BC
    private static void FieldMapChange_SaveJournalTitleForSpecialLocation(FieldSystem fieldSystem)
    {
        if (fieldSystem.JournalEntry != null)
        {
            Location special = Location.Of(FieldPlayerState.FieldPlayerState_GetSpecialLocation(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!)));
            uint title = Journal.JournalEntry_CreateTitle((ushort)special.MapId, HeapIDFieldSystem);
            Journal.JournalEntry_SaveData(fieldSystem.JournalEntry, title, Journal.DataDate);
        }
    }

    // OFFSET 0x020534EC
    public static void Location_SetToPlayerLocation(Location location, FieldSystem fieldSystem)
    {
        int z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar);
        int x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar);
        location.MapId = CurrentLocation(fieldSystem).MapId;
        location.WarpId = Location.WarpIdNone;
        location.X = x;
        location.Z = z;
        location.FaceDirection = DirectionSouth;
    }

    // OFFSET 0x02053518
    private static bool FieldSystem_IsSaveInUnionRoom(FieldSystem fieldSystem)
    {
        Location location = CurrentLocation(fieldSystem);
        if (MapHeader.MapHeader_IsPokemonCenter2F(location.MapId) && location.X == UnionRoomEntranceX && location.Z == UnionRoomEntranceZ)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02053540
    private static void FieldSystem_SetLocationToUnionRoomExit(FieldSystem fieldSystem)
    {
        Location special = Location.Of(FieldPlayerState.FieldPlayerState_GetSpecialLocation(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!)));
        VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        special.MapId = CurrentLocation(fieldSystem).MapId;
        special.WarpId = Location.WarpIdNone;
        special.X = UnionRoomExitX;
        special.Z = UnionRoomExitZ;
        special.FaceDirection = DirectionSouth;
    }

    // OFFSET 0x02053570
    private static bool FieldTask_LoadNewGameSpawn(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        ref int state = ref FieldTask.FieldTask_GetState(task);
        switch (state)
        {
            case 0:
                FieldMapChange_SetNewLocation(fieldSystem, CurrentLocation(fieldSystem));
                FieldMapChange_InitTerrainCollisionManager(fieldSystem);
                FieldMapChange_UpdateGameData(fieldSystem, false);
                FieldMapChange_CreateObjects(fieldSystem);
                state++;
                break;
            case 1:
                FieldTransition.FieldTransition_StartMapAndFadeIn(task);
                state++;
                break;
            case 2:
                return true;
        }
        return false;
    }

    // OFFSET 0x020535CC
    public static void FieldSystem_SetLoadNewGameSpawnTask(FieldSystem fieldSystem)
    {
        fieldSystem.MapLoadType = MapLoadTypeOverworld;
        ScriptManager.FieldSystem_InitNewGameState(fieldSystem);
        FieldTask.FieldSystem_CreateTask(fieldSystem, FieldTask_LoadNewGameSpawn, null);
    }

    // OFFSET 0x020535E8
    private static bool FieldTask_LoadSavedGameMap(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        ref int state = ref FieldTask.FieldTask_GetState(task);
        switch (state)
        {
            case 0:
                Underground.Underground_UpdateGiftPenaltyState(fieldSystem.SaveData!);
                int journalAcquired = SystemFlags.SystemFlag_HandleJournalAcquired(varsFlags, HandleFlagCheck) ? 1 : 0;
                if (Journal.Journal_CheckOpenOnContinue(Journal.SaveData_GetJournal(fieldSystem.SaveData!), journalAcquired))
                {
                    FieldSystemChildProcesses.FieldSystem_OpenJournalDisplay(fieldSystem, null);
                    state = 4;
                    break;
                }
                goto case 1;
            case 1:
                journalAcquired = SystemFlags.SystemFlag_HandleJournalAcquired(varsFlags, HandleFlagCheck) ? 1 : 0;
                fieldSystem.JournalEntry = Journal.Journal_GetSavedPage(Journal.SaveData_GetJournal(fieldSystem.SaveData!), journalAcquired);
                if (SystemFlags.SystemFlag_CheckCommunicationClubAccessible(varsFlags))
                {
                    FieldPlayerState playerState = FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!);
                    if (FieldSystem_IsSaveInUnionRoom(fieldSystem))
                    {
                        FieldSystem_SetLocationToUnionRoomExit(fieldSystem);
                    }
                    SystemFlags.SystemFlag_ClearCommunicationClubAccessible(varsFlags);
                    FieldMapChange_SetNewLocation(fieldSystem, Location.Of(FieldPlayerState.FieldPlayerState_GetSpecialLocation(playerState)));
                    FieldMapChange_InitTerrainCollisionManager(fieldSystem);
                    FieldMapChange_UpdateGameData(fieldSystem, false);
                    FieldMapChange_CreateObjects(fieldSystem);
                }
                else
                {
                    FieldMapChange_SetNewLocation(fieldSystem, null);
                    FieldMapChange_InitTerrainCollisionManager(fieldSystem);
                    FieldTime.FieldSystem_UpdateGameTime(fieldSystem);
                    FieldMapChange_LoadObjects(fieldSystem);
                }
                FieldMapChange_SaveJournalTitleForCurrentMap(fieldSystem);
                FieldMapChangeFlags.FieldSystem_RandomizeRoamingPokemonLocations(fieldSystem);
                state = 2;
                break;
            case 2:
                FieldTransition.FieldTransition_StartMapAndFadeIn(task);
                state = 3;
                break;
            case 3:
                return true;
            case 4:
                if (!FieldTask.FieldSystem_IsRunningApplication(fieldSystem))
                {
                    state = 1;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x02053704
    public static void FieldSystem_SetLoadSavedGameMapTask(FieldSystem fieldSystem)
    {
        fieldSystem.MapLoadType = MapLoadTypeOverworld;
        FieldTask.FieldSystem_CreateTask(fieldSystem, FieldTask_LoadSavedGameMap, null);
    }

    // OFFSET 0x02053718
    private static bool FieldTask_LoadMapFromError(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        MapChangeFromErrorData data = (MapChangeFromErrorData)FieldTask.FieldTask_GetEnv(task)!;
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        ref int state = ref FieldTask.FieldTask_GetState(task);
        switch (state)
        {
            case 0:
                ScreenFadeControl.SetScreenColorBrightness(ScreenMain, ColorBlack);
                ScreenFadeControl.SetScreenColorBrightness(ScreenSub, ColorBlack);
                Underground.Underground_UpdateGiftPenaltyState(fieldSystem.SaveData!);
                int journalAcquired = SystemFlags.SystemFlag_HandleJournalAcquired(varsFlags, HandleFlagCheck) ? 1 : 0;
                fieldSystem.JournalEntry = Journal.Journal_GetSavedPage(Journal.SaveData_GetJournal(fieldSystem.SaveData!), journalAcquired);
                state++;
                break;
            case 1:
                FieldMapChange_SetNewLocation(fieldSystem, data.Location);
                FieldMapChange_InitTerrainCollisionManager(fieldSystem);
                FieldMapChange_UpdateGameData(fieldSystem, false);
                FieldMapChange_CreateObjects(fieldSystem);
                FieldMapChange_SaveJournalTitleForSpecialLocation(fieldSystem);
                state++;
                break;
            case 2:
                fieldSystem.CommUnionRoom = sub_0205B33C(fieldSystem);
                fieldSystem.Unknown80 = sub_0205C22C(fieldSystem.CommUnionRoom);
                FieldTransition.FieldTransition_StartMap(task);
                state++;
                break;
            case 3:
                data.FinishedFlag = 0;
                PlayerFieldMoves.FieldSystem_StartWarpAnimation(fieldSystem, 0, data);
                state++;
                break;
            case 4:
                if (data.FinishedFlag != 0)
                {
                    state++;
                }
                break;
            case 5:
                return true;
        }
        return false;
    }

    // OFFSET 0x02053808
    public static void FieldSystem_StartLoadMapFromErrorTask(FieldSystem fieldSystem)
    {
        if (!MapHeader.MapHeader_IsUnionRoom(CurrentLocation(fieldSystem).MapId))
        {
            if (FieldSystem_IsSaveInUnionRoom(fieldSystem))
            {
                VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
                FieldSystem_SetLocationToUnionRoomExit(fieldSystem);
                SystemFlags.SystemFlag_SetCommunicationClubAccessible(varsFlags);
            }
            else
            {
                FieldSystem_SetLoadSavedGameMapTask(fieldSystem);
                return;
            }
        }
        MapChangeFromErrorData data = new();
        data.FinishedFlag = 0;
        data.Location.MapId = MapHeaderUnionRoom;
        data.Location.WarpId = Location.WarpIdNone;
        data.Location.X = UnionRoomSpawnX;
        data.Location.Z = UnionRoomSpawnZ;
        data.Location.FaceDirection = 0;
        fieldSystem.MapLoadType = MapLoadTypeUnion;
        FieldTask.FieldSystem_CreateTask(fieldSystem, FieldTask_LoadMapFromError, data);
    }

    // OFFSET 0x02053878
    private static bool FieldTask_ChangeMap(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        MapChangeData data = (MapChangeData)FieldTask.FieldTask_GetEnv(task)!;
        switch (data.State)
        {
            case 0:
                SoundPlayback.Sound_PlayEffect(SoundEffectStairs);
                FieldBgm.FieldBGM_FadeOutForMapHeader(fieldSystem, data.NextLocation.MapId);
                FieldTransition.FieldTransition_FadeOutAndFinishMap(task);
                data.State++;
                break;
            case 1:
                FieldTask_ChangeMapByLocation(task, data.NextLocation);
                data.State++;
                break;
            case 2:
                if (SoundPlayback.Sound_IsFadeActive() == 0)
                {
                    FieldBgm.FieldBGM_PlayForMapHeader(fieldSystem, data.NextLocation.MapId);
                    FieldTransition.FieldTransition_StartMapAndFadeIn(task);
                    data.State++;
                }
                break;
            case 3:
                return true;
        }
        return false;
    }

    // OFFSET 0x02053900
    public static void FieldSystem_StartChangeMapTask(FieldTask task, Location nextLocation)
    {
        MapChangeData data = new();
        data.State = 0;
        data.NextLocation.Assign(nextLocation);
        FieldTask.FieldTask_InitCall(task, FieldTask_ChangeMap, data);
    }

    // OFFSET 0x02053930
    private static bool FieldTask_ChangeMapSub(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        MapChangeSubData data = (MapChangeSubData)FieldTask.FieldTask_GetEnv(task)!;
        switch (data.State)
        {
            case 0:
                FieldMapChange_DeleteObjects(fieldSystem);
                FieldMapChange_RemoveTerrainCollisionManager(fieldSystem);
                data.State++;
                break;
            case 1:
                FieldMapChange_SetNewLocation(fieldSystem, data.NextLocation);
                FieldMapChange_InitTerrainCollisionManager(fieldSystem);
                FieldMapChange_UpdateGameData(fieldSystem, false);
                RadarChain.RadarChain_Clear(fieldSystem.RadarChain);
                data.State++;
                break;
            case 2:
                FieldMapChange_CreateObjects(fieldSystem);
                return true;
        }
        return false;
    }

    // OFFSET 0x020539A0
    public static void FieldTask_ChangeMapByLocation(FieldTask task, Location nextLocation)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        MapChangeSubData data = new();
        if (FieldSystemCore.FieldSystem_HasParentProcess(fieldSystem) != 0)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        data.State = 0;
        data.NextLocation.Assign(nextLocation);
        FieldTask.FieldTask_InitCall(task, FieldTask_ChangeMapSub, data);
    }

    // OFFSET 0x020539E8
    public static void FieldTask_ChangeMapToLocation(FieldTask task, int mapId, int warpId, int x, int z, int faceDirection)
    {
        Location location = new(mapId, warpId, x, z, faceDirection);
        FieldTask_ChangeMapByLocation(task, location);
    }

    // OFFSET 0x02053A04
    private static bool FieldTask_ChangeMapFull(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        MapChangeSubData data = (MapChangeSubData)FieldTask.FieldTask_GetEnv(task)!;
        switch (data.State)
        {
            case 0:
                FieldBgm.FieldBGM_FadeOutForMapHeader(fieldSystem, data.NextLocation.MapId);
                FieldTransition.FieldTransition_FinishMap(task);
                data.State++;
                break;
            case 1:
                FieldTask_ChangeMapByLocation(task, data.NextLocation);
                data.State++;
                break;
            case 2:
                if (SoundPlayback.Sound_IsFadeActive() == 0)
                {
                    FieldBgm.FieldBGM_PlayForMapHeader(fieldSystem, data.NextLocation.MapId);
                    FieldTransition.FieldTransition_StartMap(task);
                    data.State++;
                }
                break;
            case 3:
                return true;
        }
        return false;
    }

    // OFFSET 0x02053A80
    public static void FieldTask_StartMapChangeFull(FieldTask task, int mapId, int warpId, int x, int z, int faceDirection)
    {
        MapChangeSubData data = new();
        data.State = 0;
        data.NextLocation.MapId = mapId;
        data.NextLocation.WarpId = warpId;
        data.NextLocation.X = x;
        data.NextLocation.Z = z;
        data.NextLocation.FaceDirection = faceDirection;
        FieldTask.FieldTask_InitCall(task, FieldTask_ChangeMapFull, data);
    }
}
