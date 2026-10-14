using static PlatinumSharp.Game.Globals;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class FieldSystemCore
{
    private const int HeapIDApplication = 3;
    private const int HeapIDSystem = 0;
    private const int HeapIDFieldSystem = 0xB;
    private const int HeapIDFieldTask = 0x20;
    private const int HeapIDFieldSmall = 0x5B;
    private const uint FieldSystemHeapSize = 0x1C000;
    private const uint FieldTaskHeapSize = 0x4000;
    private const uint FieldSmallHeapSize = 0x300;
    private const int FieldMapOverlay = 5;
    private const uint FieldMapTemplate = 0x021F89B0;
    private const int TitleScreenOverlay = 0x4D;
    private const uint TitleScreenTemplate = 0x021D742C;
    private const uint DefaultReturnLocationAddress = 0x020EA140;
    private const int LocationSize = 0x14;
    private const uint MapHeaderUnionRoom = 0x146;
    private const int MapLoadTypeOverworld = 0;
    private const int MapLoadTypeUnderground = 1;
    private const int MapLoadTypeUnion = 2;
    private const int MapLoadTypeColosseum = 3;
    private const int MapLoadTypeBattleTower = 4;
    private const int KeyA = 1;
    private const int DirectionNone = -1;
    private const int SignpostCommandHide = 4;
    private const int PoketchEventPlayerMoved = 0;

    private static FieldSystem? sFieldSystem;

    // OFFSET 0x0203CC84
    public static int FieldSystem_InitContinue(ApplicationManager appMan, ref int state)
    {
        ApplicationArgs args = (ApplicationArgs)ApplicationManager.ApplicationManager_Args(appMan)!;
        FieldSystem fieldSystem = FieldSystem_Init(appMan);
        sFieldSystem = fieldSystem;
        if (args.BootMode != 0)
        {
            FieldMapChange.FieldSystem_StartLoadMapFromErrorTask(fieldSystem);
        }
        else
        {
            FieldMapChange.FieldSystem_SetLoadSavedGameMapTask(fieldSystem);
        }
        args.BootMode = 0;
        return 1;
    }

    // OFFSET 0x0203CCB4
    public static int FieldSystem_InitNewGame(ApplicationManager appMan, ref int state)
    {
        FieldSystem fieldSystem = FieldSystem_Init(appMan);
        sFieldSystem = fieldSystem;
        FieldMapChange.FieldSystem_SetLoadNewGameSpawnTask(fieldSystem);
        return 1;
    }

    // OFFSET 0x0203CCCC
    public static int FieldSystem_Main(ApplicationManager appMan, ref int state)
    {
        FieldSystem fieldSystem = (FieldSystem)ApplicationManager.ApplicationManager_Data(appMan)!;
        if (FieldSystem_RunFrame(fieldSystem) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0203CCE4
    public static int FieldSystem_Exit(ApplicationManager appMan, ref int state)
    {
        FieldSystem_Teardown(appMan);
        Main.EnqueueApplication(TitleScreenOverlay, ApplicationTemplate.At(TitleScreenTemplate));
        return 1;
    }

    // OFFSET 0x0203CD00
    public static void FieldSystem_StartFieldMap(FieldSystem fieldSystem)
    {
        if (fieldSystem.ProcessManager!.Child != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (fieldSystem.ProcessManager.Parent != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        GameOverlay.Overlay_LoadByID(FieldMapOverlay, GameOverlay.LoadNoInitAsync);
        fieldSystem.RunningFieldMap = 0;
        fieldSystem.ProcessManager.Pause = 0;
        fieldSystem.ProcessManager.Parent = ApplicationManager.ApplicationManager_New(ApplicationTemplate.At(FieldMapTemplate), fieldSystem, HeapIDFieldSystem);
    }

    // OFFSET 0x0203CD44
    public static void FieldSystem_FlagNotRunningFieldMap(FieldSystem fieldSystem)
    {
        fieldSystem.RunningFieldMap = 0;
    }

    // OFFSET 0x0203CD4C
    public static int FieldSystem_HasParentProcess(FieldSystem fieldSystem)
    {
        if (fieldSystem.ProcessManager!.Parent != null)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0203CD5C
    public static int FieldSystem_IsRunningFieldMap(FieldSystem fieldSystem)
    {
        if (fieldSystem.ProcessManager!.Parent != null && fieldSystem.RunningFieldMap != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0203CD74
    public static int FieldSystem_HasChildProcess(FieldSystem fieldSystem)
    {
        if (fieldSystem.ProcessManager!.Child != null)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0203CD84
    public static void FieldSystem_StartChildProcess(FieldSystem fieldSystem, ApplicationTemplate template, object? args)
    {
        if (fieldSystem.ProcessManager!.Child != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        FieldSystem_FlagNotRunningFieldMap(fieldSystem);
        fieldSystem.ProcessManager.Child = ApplicationManager.ApplicationManager_New(template, args, HeapIDFieldSystem);
    }

    // OFFSET 0x0203CDB0
    private static FieldSystem FieldSystem_Init(ApplicationManager appMan)
    {
        Heap.Heap_Create(HeapIDApplication, HeapIDFieldSystem, FieldSystemHeapSize);
        Heap.Heap_Create(HeapIDApplication, HeapIDFieldTask, FieldTaskHeapSize);
        Heap.Heap_Create(HeapIDSystem, HeapIDFieldSmall, FieldSmallHeapSize);
        FieldSystem fieldSystem = new();
        appMan.Data = fieldSystem;
        FieldProcessManager processManager = new();
        fieldSystem.ProcessManager = processManager;
        processManager.Parent = null;
        processManager.Child = null;
        processManager.Pause = 0;
        processManager.Kill = 0;
        fieldSystem.SaveData = ((ApplicationArgs)ApplicationManager.ApplicationManager_Args(appMan)!).SaveData;
        fieldSystem.Task = null;
        fieldSystem.Location = FieldPlayerState.FieldPlayerState_GetPlayerLocation(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        fieldSystem.MapMatrix = MapMatrix.MapMatrix_New();
        MapHeaderData.MapHeaderData_Init(fieldSystem, HeapIDFieldSystem);
        fieldSystem.BagCursor = BagCursor.BagCursor_New(HeapIDFieldSystem);
        fieldSystem.RadarChain = RadarChain.RadarChain_Init(HeapIDFieldSystem);
        RadarChain.RadarChain_Clear(fieldSystem.RadarChain);
        fieldSystem.PokedexMemory = PokedexMemory.PokedexMemory_New(HeapIDFieldSystem);
        fieldSystem.BattleSubscreenCursorOn = BattleSubscreenCursor.BattleSubscreenCursor_New(HeapIDFieldSystem);
        return fieldSystem;
    }

    // OFFSET 0x0203CE6C
    private static void FieldSystem_Teardown(ApplicationManager appMan)
    {
        FieldSystem fieldSystem = (FieldSystem)ApplicationManager.ApplicationManager_Data(appMan)!;
        MapMatrix.MapMatrix_Free(fieldSystem.MapMatrix);
        MapHeaderData.MapHeaderData_Free(fieldSystem);
        fieldSystem.BagCursor = null;
        RadarChain.RadarChain_Free(fieldSystem.RadarChain);
        PokedexMemory.PokedexMemory_Free(fieldSystem.PokedexMemory);
        BattleSubscreenCursor.BattleSubscreenCursor_Free(fieldSystem.BattleSubscreenCursorOn);
        fieldSystem.ProcessManager = null;
        ApplicationManager.ApplicationManager_FreeData(appMan);
        Heap.Heap_Destroy(HeapIDFieldSmall);
        Heap.Heap_Destroy(HeapIDFieldSystem);
        Heap.Heap_Destroy(HeapIDFieldTask);
    }

    // OFFSET 0x0203CECC
    private static void FieldSystem_ExecuteAndCleanupIfDone(ref ApplicationManager? appMan)
    {
        if (appMan != null && ApplicationManager.ApplicationManager_Exec(appMan))
        {
            ApplicationManager.ApplicationManager_Free(appMan);
            appMan = null;
        }
    }

    // OFFSET 0x0203CEEC
    private static int FieldSystem_RunFrame(FieldSystem fieldSystem)
    {
        FieldSystem_HandleFieldInput(fieldSystem);
        if (FieldTask.FieldTask_Run(fieldSystem) && fieldSystem.FieldMapContext != null)
        {
            FieldPoketch.FieldSystem_SendPoketchEvent(fieldSystem, PoketchEventPlayerMoved, 0);
        }
        FieldProcessManager processManager = fieldSystem.ProcessManager!;
        if (processManager.Parent != null)
        {
            FieldSystem_ExecuteAndCleanupIfDone(ref processManager.Parent);
            if (fieldSystem.ProcessManager!.Parent == null)
            {
                GameOverlay.Overlay_UnloadByID(FieldMapOverlay);
            }
        }
        else if (processManager.Child != null)
        {
            FieldSystem_ExecuteAndCleanupIfDone(ref processManager.Child);
        }
        processManager = fieldSystem.ProcessManager!;
        if (processManager.Kill != 0 && fieldSystem.Task == null && processManager.Parent == null && processManager.Child == null)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0203CF5C
    private static void FieldSystem_HandleFieldInput(FieldSystem fieldSystem)
    {
        FieldInput input = new();
        int processInput = 0;
        if (fieldSystem.ProcessManager!.Pause == 0 && fieldSystem.RunningFieldMap != 0 && !FieldTask.FieldSystem_IsRunningTask(fieldSystem))
        {
            processInput = 1;
        }
        if (processInput != 0)
        {
            PlayerMove.PlayerAvatar_UpdatePlayerMoveState(fieldSystem.PlayerAvatar);
            FieldControl.FieldInput_Update(input, fieldSystem, (ushort)gSystem.PressedKeys, (ushort)gSystem.HeldKeys);
        }
        int mapLoadType = fieldSystem.MapLoadType;
        if (fieldSystem.Location!.U32(0) == MapHeaderUnionRoom)
        {
            mapLoadType = MapLoadTypeOverworld;
        }
        switch (mapLoadType)
        {
            case MapLoadTypeUnderground:
                if (processInput != 0)
                {
                    if (CommPlayerManager.CommPlayerMan_CanProcessFieldInput() != 0)
                    {
                        FieldControl.FieldInput_Process_Underground(input, fieldSystem);
                    }
                }
                CommPlayerManager.CommPlayerMan_UpdateField(fieldSystem, processInput);
                return;
            case MapLoadTypeColosseum:
                if (processInput != 0)
                {
                    if (CommPlayerManager.CommPlayerMan_CanProcessFieldInput() != 0)
                    {
                        if (FieldControl.FieldInput_Process_Colosseum(input, fieldSystem) == 1)
                        {
                            processInput = 0;
                        }
                    }
                    else
                    {
                        processInput = 0;
                    }
                }
                CommPlayerManager.CommPlayerMan_UpdateField(fieldSystem, processInput);
                return;
            case MapLoadTypeUnion:
                if (processInput == 0)
                {
                    return;
                }
                if (FieldControl.FieldInput_Process_UnionRoom(input, fieldSystem) == 1)
                {
                    return;
                }
                PlayerMove.PlayerAvatar_MoveMain(fieldSystem.PlayerAvatar, fieldSystem.LandDataManager, DirectionNone, input.PressedKeys, input.HeldKeys, 0);
                return;
            case MapLoadTypeBattleTower:
                if (processInput == 0)
                {
                    return;
                }
                if (FieldControl.FieldInput_Process_BattleTower(input, fieldSystem) == 1)
                {
                    MapNamePopUps.MapNamePopUp_Hide(fieldSystem.FieldMapContext!.MapNamePopUp);
                    Signposts.Signpost_DoCommand(fieldSystem, SignpostCommandHide);
                    PlayerAvatarRequestState.PlayerAvatar_RestoreWalkingGraphics(fieldSystem.PlayerAvatar);
                    FieldPoketch.FieldSystem_SendPoketchEvent(fieldSystem, PoketchEventPlayerMoved, 1);
                    return;
                }
                FieldSystem_MovePlayer(fieldSystem, input);
                return;
            default:
                if (processInput == 0)
                {
                    return;
                }
                if (FieldControl.FieldInput_Process(input, fieldSystem) == 1)
                {
                    MapNamePopUps.MapNamePopUp_Hide(fieldSystem.FieldMapContext!.MapNamePopUp);
                    Signposts.Signpost_DoCommand(fieldSystem, SignpostCommandHide);
                    PlayerMove.PlayerAvatar_ClearMoveState(fieldSystem.PlayerAvatar);
                    PlayerAvatarRequestState.PlayerAvatar_RestoreWalkingGraphics(fieldSystem.PlayerAvatar);
                    FieldPoketch.FieldSystem_SendPoketchEvent(fieldSystem, PoketchEventPlayerMoved, 1);
                    return;
                }
                FieldSystem_MovePlayer(fieldSystem, input);
                return;
        }
    }

    private static void FieldSystem_MovePlayer(FieldSystem fieldSystem, FieldInput input)
    {
        if ((gSystem.PressedKeys & KeyA) != 0)
        {
            MapNamePopUps.MapNamePopUp_Hide(fieldSystem.FieldMapContext!.MapNamePopUp);
        }
        int poketchTapped = 0;
        object? poketchSystem = FieldSystem_GetPoketchSystem();
        if (poketchSystem != null)
        {
            poketchTapped = PoketchSystems.PoketchSystem_IsTapped((PoketchSystem)poketchSystem) ? 1 : 0;
        }
        PlayerMove.PlayerAvatar_MoveMain(fieldSystem.PlayerAvatar, fieldSystem.LandDataManager, DirectionNone, input.PressedKeys, input.HeldKeys, poketchTapped);
    }

    // OFFSET 0x0203D128
    public static void FieldSystem_PauseProcessing()
    {
        sFieldSystem!.ProcessManager!.Pause = 1;
        CommSystemField.CommSys_DisableSendMovementData();
    }

    // OFFSET 0x0203D140
    public static void FieldSystem_ResumeProcessing()
    {
        sFieldSystem!.ProcessManager!.Pause = 0;
        CommSystemField.CommSys_EnableSendMovementData();
    }

    // OFFSET 0x0203D158
    public static object? FieldSystem_GetPoketchSystem()
    {
        if (sFieldSystem!.FieldMapContext == null)
        {
            return null;
        }
        return sFieldSystem.FieldMapContext.PoketchSystem;
    }

    // OFFSET 0x0203D170
    public static BgConfig? FieldSystem_GetBgConfig(FieldSystem fieldSystem)
    {
        return fieldSystem.BgConfig;
    }

    // OFFSET 0x0203D174
    public static SaveData? FieldSystem_GetSaveData(FieldSystem fieldSystem)
    {
        return fieldSystem.SaveData;
    }

    // OFFSET 0x0203D190
    public static void Location_SetDefaultReturnPoint(BufferView location)
    {
        location.CopyFrom(0, RomData.Arm9(DefaultReturnLocationAddress, LocationSize), 0, LocationSize);
    }
}
