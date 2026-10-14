namespace PlatinumSharp.Game;

public delegate bool PoketchAppInitFunction(out object? app, PoketchSystem poketchSys, BgConfig bgConfig, int appID);

public delegate void PoketchAppShutdownFunction(object? app);

public delegate void PoketchAppSaveFunction(object? app);

public sealed class PoketchSystem
{
    public byte SystemState;
    public byte SubState;
    public byte AppState;
    public byte TouchingScreen;
    public byte PlayerMoving;
    public byte AppChanging;
    public byte AppStarting;
    public byte PedometerUpdated;
    public uint ButtonState;
    public bool SkipApp;
    public uint AppSkipTimer;
    public int LoadedAppID;
    public int LoadedAppOverlayID;
    public PoketchGraphicsTaskData? TaskData;
    public readonly PoketchConstTaskData ConstTaskData = new();
    public object? AppData;
    public PoketchButtonManager? ButtonManager;
    public uint NextBtnTask;
    public uint LastBtnReleaseTask;
    public FieldMapContext? PoketchSysOwner;
    public SysTask? PostRenderTask;
    public PoketchAppInitFunction? CurrAppInit;
    public PoketchAppShutdownFunction? CurrAppShutdown;
    public PoketchAppSaveFunction? CurrAppSave;
    public object? AppSaveData;
    public BgConfig? Bgl;
    public OamManager? OamManager;
    public SaveData? SaveData;
    public Poketch? Poketch;
    public FieldSystem? FieldSystem;
    public uint ButtonDir;
}

public static class PoketchSystems
{
    public const int EventPlayerMoved = 1;
    public const int EventSave = 4;
    public const int EventPedometer = 5;

    private const int HeapIDParent = 3;
    private const int HeapIDPoketchApp = 7;
    private const int HeapIDPoketch = 8;
    private const uint PoketchHeapSize = 0xC000;
    private const uint MainTaskPriority = 0;
    private const uint PostRenderTaskPriority = 4;
    private const int TouchPadFrequency = 4;
    private const byte SystemStateInit = 0;
    private const byte SystemStateMain = 1;
    private const byte SystemStateChangeApp = 2;
    private const byte SystemStateShutdown = 3;
    private const byte SystemStateUnload = 4;
    private const byte AppStateNone = 0;
    private const byte AppStateLoading = 1;
    private const byte AppStateRunning = 2;
    private const byte AppStateShutdown = 3;
    private const uint MaxPedometerValue = 99999;
    private const int PoketchOverlay = 25;
    private const uint AppOverlayTable = 0x02255E7C;
    private const uint ButtonHitTable = 0x02255E60;
    private const uint NumButtons = 3;
    private const uint ButtonIndexScreen = 2;
    private const uint NoTask = 0xFFFFFFFF;
    private const ushort SeqPoketchBusy = 0x66E;
    private const uint AppSkipDelay = 0x1E;
    private const uint AppSwitchDelay = 3;
    private const int OverlayLoadTypeAsync = 2;

    private static readonly Action<PoketchSystem>[] sStateFuncs =
    {
        PoketchSystem_InitState,
        PoketchSystem_MainState,
        PoketchSystem_ChangeAppState,
        PoketchSystem_ShutdownState,
    };

    private static PoketchSystem PoketchSystem_Get()
    {
        return (PoketchSystem)FieldSystemCore.FieldSystem_GetPoketchSystem()!;
    }

    // OFFSET 0x02253CE8
    public static void PoketchSystem_Create(FieldSystem fieldSystem, FieldMapContext poketchSysOwner, SaveData saveData, BgConfig bgConfig, OamManager oamManager)
    {
        Heap.Heap_Create(HeapIDParent, HeapIDPoketchApp, PoketchHeapSize);
        Heap.Heap_Create(HeapIDParent, HeapIDPoketch, PoketchHeapSize);
        PoketchSystem poketchSys = new();
        poketchSysOwner.PoketchSystem = poketchSys;
        poketchSys.FieldSystem = fieldSystem;
        poketchSys.SaveData = saveData;
        poketchSys.Poketch = FieldPoketch.SaveData_GetPoketch(saveData);
        poketchSys.Bgl = bgConfig;
        poketchSys.OamManager = oamManager;
        if (!PoketchSystem_Init(poketchSys))
        {
            return;
        }
        TouchPad.EnableTouchPad();
        TouchPad.InitializeTouchPad(TouchPadFrequency);
        poketchSys.PoketchSysOwner = poketchSysOwner;
        poketchSys.PostRenderTask = SysTask.SysTask_Start(PoketchSystem_PostRender, poketchSys, PostRenderTaskPriority);
        SysTask.SysTask_Start(PoketchSystem_Main, poketchSys, MainTaskPriority);
    }

    // OFFSET 0x02253D5C
    public static void PoketchSystem_StartShutdown(PoketchSystem poketchSys)
    {
        if (poketchSys.SystemState != SystemStateShutdown && poketchSys.SystemState != SystemStateUnload)
        {
            PoketchSystem_SetSystemState(poketchSys, SystemStateShutdown);
        }
    }

    // OFFSET 0x02253D70
    public static bool PoketchSystem_IsSystemShutdown(PoketchSystem? poketchSys)
    {
        return poketchSys == null;
    }

    // OFFSET 0x02253D7C
    public static void PoketchSystem_SendEvent(PoketchSystem poketchSys, int eventID, uint value)
    {
        switch (eventID)
        {
            case EventPlayerMoved:
                poketchSys.PlayerMoving = 1;
                break;
            case EventPedometer:
            {
                uint steps = Poketch.Poketch_PedometerValue(poketchSys.Poketch!) + 1;
                if (steps > MaxPedometerValue)
                {
                    steps = 0;
                }
                Poketch.Poketch_SetPedometerValue(poketchSys.Poketch!, steps);
                poketchSys.PedometerUpdated = 1;
                break;
            }
            case EventSave:
                if (poketchSys.AppState == AppStateRunning && poketchSys.CurrAppSave != null)
                {
                    poketchSys.CurrAppSave(poketchSys.AppSaveData);
                }
                break;
        }
    }

    // OFFSET 0x02253DD4
    public static bool PoketchSystem_IsTapped(PoketchSystem poketchSys)
    {
        return poketchSys.TouchingScreen != 0;
    }

    // OFFSET 0x02253DD8
    public static int PoketchSystem_CurrentAppID(PoketchSystem poketchSys)
    {
        return poketchSys.LoadedAppID;
    }

    // OFFSET 0x02253DDC
    private static bool PoketchSystem_Init(PoketchSystem poketchSys)
    {
        if (!PoketchGraphics.PoketchGraphics_Main(out poketchSys.TaskData, poketchSys.ConstTaskData, poketchSys.OamManager!, poketchSys))
        {
            return false;
        }
        poketchSys.SystemState = SystemStateInit;
        poketchSys.SubState = 0;
        poketchSys.TouchingScreen = 0;
        poketchSys.PlayerMoving = 0;
        poketchSys.PedometerUpdated = 0;
        poketchSys.AppChanging = 0;
        poketchSys.AppStarting = 0;
        poketchSys.LoadedAppID = -1;
        poketchSys.AppState = AppStateNone;
        poketchSys.ButtonState = 0;
        poketchSys.ButtonDir = 0;
        return PoketchSystem_InitButtonManager(poketchSys);
    }

    // OFFSET 0x02253E20
    private static void PoketchSystem_Shutdown(PoketchSystem poketchSys)
    {
        SysTask.SysTask_Done(poketchSys.PostRenderTask!);
        PoketchSystem_FreeButtonManager(poketchSys);
        PoketchSystem_UnloadApp(poketchSys);
        PoketchGraphics.PoketchGraphics_Close(poketchSys.TaskData);
    }

    // OFFSET 0x02253E40
    private static void PoketchSystem_Main(SysTask task, object? param)
    {
        PoketchSystem poketchSys = (PoketchSystem)param!;
        if (poketchSys.SystemState < SystemStateUnload)
        {
            if (poketchSys.SystemState != SystemStateInit)
            {
                PoketchSystem_UpdateButtons(poketchSys);
            }
            sStateFuncs[poketchSys.SystemState](poketchSys);
            return;
        }
        if (poketchSys.SystemState != SystemStateUnload)
        {
            ErrorHandling.GF_AssertFail();
        }
        poketchSys.PoketchSysOwner!.PoketchSystem = null;
        PoketchSystem_Shutdown(poketchSys);
        SysTask.SysTask_Done(task);
        Heap.Heap_Destroy(HeapIDPoketchApp);
        Heap.Heap_Destroy(HeapIDPoketch);
    }

    // OFFSET 0x02253E94
    private static void PoketchSystem_PostRender(SysTask task, object? param)
    {
        PoketchSystem poketchSys = (PoketchSystem)param!;
        poketchSys.PlayerMoving = 0;
        poketchSys.PedometerUpdated = 0;
    }

    // OFFSET 0x02253E9C
    private static void PoketchSystem_SetSystemState(PoketchSystem poketchSys, byte state)
    {
        poketchSys.SystemState = state;
        poketchSys.SubState = 0;
    }

    // OFFSET 0x02253EA4
    private static void PoketchSystem_InitState(PoketchSystem poketchSys)
    {
        switch (poketchSys.SubState)
        {
            case 0:
                PoketchGraphics.PoketchGraphics_StartTask(poketchSys.TaskData!, PoketchGraphics.TaskSetup);
                poketchSys.SubState++;
                break;
            case 1:
                if (PoketchGraphics.PoketchGraphics_TaskIsNotActive(poketchSys.TaskData!, PoketchGraphics.TaskSetup))
                {
                    int appID = Poketch.Poketch_CurrentAppID(poketchSys.Poketch!);
                    PoketchSystem_LoadApp(poketchSys, appID);
                    PoketchSystem_InitApp(poketchSys, appID);
                    poketchSys.SubState++;
                }
                break;
            case 2:
                if (!PoketchSystem_IsAppRunning(poketchSys))
                {
                    break;
                }
                PoketchGraphics.PoketchGraphics_StartTask(poketchSys.TaskData!, PoketchGraphics.TaskOpen);
                poketchSys.SubState++;
                goto case 3;
            case 3:
                if (PoketchGraphics.PoketchGraphics_TaskIsNotActive(poketchSys.TaskData!, PoketchGraphics.TaskOpen))
                {
                    PoketchSystem_SetSystemState(poketchSys, SystemStateMain);
                }
                break;
        }
    }

    private static bool PoketchSystem_ButtonTapped(PoketchSystem poketchSys)
    {
        return poketchSys.ButtonState == PoketchButtons.ButtonStateTapped || poketchSys.ButtonState == PoketchButtons.ButtonStateTimer0;
    }

    private static int PoketchSystem_CycleApp(PoketchSystem poketchSys)
    {
        if (poketchSys.ButtonDir == 0)
        {
            return Poketch.Poketch_CyclePrevApp(poketchSys.Poketch!);
        }
        return Poketch.Poketch_CycleNextApp(poketchSys.Poketch!);
    }

    // OFFSET 0x02253F2C
    private static void PoketchSystem_MainState(PoketchSystem poketchSys)
    {
        switch (poketchSys.SubState)
        {
            case 0:
                if (PoketchSystem_IsRunningTask(poketchSys))
                {
                    break;
                }
                if (PoketchSystem_ButtonTapped(poketchSys))
                {
                    poketchSys.SkipApp = false;
                    poketchSys.AppChanging = 1;
                    PoketchGraphics.PoketchGraphics_StartTask(poketchSys.TaskData!, PoketchGraphics.TaskClose);
                    poketchSys.SubState++;
                }
                break;
            case 1:
                if (PoketchSystem_ButtonTapped(poketchSys))
                {
                    poketchSys.SkipApp = true;
                }
                if (!PoketchGraphics.PoketchGraphics_NoActiveTasks(poketchSys.TaskData!))
                {
                    break;
                }
                poketchSys.ConstTaskData.AppID = PoketchSystem_CycleApp(poketchSys);
                if (poketchSys.SkipApp)
                {
                    PoketchGraphics.PoketchGraphics_StartTask(poketchSys.TaskData!, PoketchGraphics.TaskLoadCursor);
                    poketchSys.AppSkipTimer = AppSkipDelay;
                    poketchSys.SkipApp = false;
                    poketchSys.SubState = 4;
                    break;
                }
                poketchSys.AppSkipTimer = AppSwitchDelay;
                poketchSys.SubState = 2;
                break;
            case 2:
                if (PoketchSystem_ButtonTapped(poketchSys))
                {
                    PoketchGraphics.PoketchGraphics_StartTask(poketchSys.TaskData!, PoketchGraphics.TaskLoadCursor);
                    poketchSys.AppSkipTimer = AppSkipDelay;
                    poketchSys.SkipApp = false;
                    poketchSys.SubState = 4;
                    break;
                }
                if (poketchSys.AppSkipTimer != 0)
                {
                    poketchSys.AppSkipTimer--;
                    break;
                }
                PoketchSystem_ShutdownApp(poketchSys);
                poketchSys.SubState = 3;
                break;
            case 3:
                if (PoketchSystem_IsAppShutdown(poketchSys))
                {
                    PoketchSystem_UnloadApp(poketchSys);
                    PersistentAppData.PersistentAppData_Clear();
                    PoketchSystem_SetSystemState(poketchSys, SystemStateChangeApp);
                }
                break;
            case 4:
                if (PoketchSystem_ButtonTapped(poketchSys))
                {
                    poketchSys.ConstTaskData.AppID = PoketchSystem_CycleApp(poketchSys);
                    poketchSys.AppSkipTimer = AppSkipDelay;
                    PoketchGraphics.PoketchGraphics_StartTask(poketchSys.TaskData!, PoketchGraphics.TaskUpdateCursor);
                    break;
                }
                if (poketchSys.AppSkipTimer != 0)
                {
                    poketchSys.AppSkipTimer--;
                    break;
                }
                PoketchGraphics.PoketchGraphics_StartTask(poketchSys.TaskData!, PoketchGraphics.TaskFreeCursor);
                PoketchSystem_ShutdownApp(poketchSys);
                poketchSys.SubState = 3;
                break;
        }
    }

    // OFFSET 0x0225406C
    private static void PoketchSystem_ChangeAppState(PoketchSystem poketchSys)
    {
        switch (poketchSys.SubState)
        {
            case 0:
            {
                poketchSys.AppStarting = 1;
                int appID = Poketch.Poketch_CurrentAppID(poketchSys.Poketch!);
                PoketchSystem_LoadApp(poketchSys, appID);
                PoketchSystem_InitApp(poketchSys, appID);
                poketchSys.SubState++;
                break;
            }
            case 1:
                if (PoketchSystem_IsAppRunning(poketchSys))
                {
                    PoketchGraphics.PoketchGraphics_StartTask(poketchSys.TaskData!, PoketchGraphics.TaskOpenAlt);
                    poketchSys.SubState++;
                }
                break;
            case 2:
                if (PoketchGraphics.PoketchGraphics_TaskIsNotActive(poketchSys.TaskData!, PoketchGraphics.TaskOpenAlt))
                {
                    poketchSys.AppChanging = 0;
                    poketchSys.AppStarting = 0;
                    PoketchSystem_SetSystemState(poketchSys, SystemStateMain);
                }
                break;
        }
    }

    // OFFSET 0x022540D8
    private static void PoketchSystem_ShutdownState(PoketchSystem poketchSys)
    {
        switch (poketchSys.SubState)
        {
            case 0:
                switch (poketchSys.AppState)
                {
                    case AppStateLoading:
                        poketchSys.SubState = 1;
                        break;
                    case AppStateRunning:
                        PoketchSystem_ShutdownApp(poketchSys);
                        poketchSys.SubState = 2;
                        break;
                    case AppStateNone:
                    case AppStateShutdown:
                        poketchSys.SubState = 2;
                        break;
                }
                break;
            case 1:
                if (PoketchSystem_IsAppRunning(poketchSys))
                {
                    PoketchSystem_ShutdownApp(poketchSys);
                    poketchSys.SubState = 2;
                }
                break;
            case 2:
                if (PoketchSystem_IsAppShutdown(poketchSys))
                {
                    PoketchGraphics.PoketchGraphics_StartTask(poketchSys.TaskData!, PoketchGraphics.TaskFreeBackgrounds);
                    poketchSys.SubState = 3;
                }
                break;
            case 3:
                if (PoketchGraphics.PoketchGraphics_NoActiveTasks(poketchSys.TaskData!))
                {
                    PoketchSystem_UnloadApp(poketchSys);
                    TouchPad.DisableTouchPad();
                    PoketchSystem_SetSystemState(poketchSys, SystemStateUnload);
                }
                break;
        }
    }

    // OFFSET 0x02254170
    private static void PoketchSystem_LoadApp(PoketchSystem poketchSys, int appID)
    {
        if (appID < 0 || appID >= Poketch.AppCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (poketchSys.LoadedAppID != -1)
        {
            return;
        }
        uint[] table = RomData.OverlayU32(PoketchOverlay, AppOverlayTable, Poketch.AppCount * 2);
        for (int i = 0; i < Poketch.AppCount; i++)
        {
            if (appID == (int)table[i * 2])
            {
                int overlayID = (int)table[i * 2 + 1];
                GameOverlay.Overlay_LoadByID(overlayID, OverlayLoadTypeAsync);
                poketchSys.LoadedAppID = appID;
                poketchSys.LoadedAppOverlayID = overlayID;
                return;
            }
        }
    }

    // OFFSET 0x022541BC
    private static void PoketchSystem_UnloadApp(PoketchSystem poketchSys)
    {
        if (poketchSys.LoadedAppID != -1)
        {
            GameOverlay.Overlay_UnloadByID(poketchSys.LoadedAppOverlayID);
            poketchSys.LoadedAppID = -1;
        }
    }

    // OFFSET 0x022541D8
    private static void PoketchSystem_InitApp(PoketchSystem poketchSys, int appID)
    {
        if (poketchSys.AppState != AppStateNone)
        {
            ErrorHandling.GF_AssertFail();
        }
        poketchSys.CurrAppInit!(out poketchSys.AppData, poketchSys, poketchSys.Bgl!, poketchSys.LoadedAppID);
        poketchSys.AppState = AppStateLoading;
    }

    // OFFSET 0x022541FC
    private static bool PoketchSystem_IsAppRunning(PoketchSystem poketchSys)
    {
        return poketchSys.AppState == AppStateRunning;
    }

    // OFFSET 0x0225420C
    private static void PoketchSystem_ShutdownApp(PoketchSystem poketchSys)
    {
        if (poketchSys.AppState != AppStateRunning)
        {
            ErrorHandling.GF_AssertFail();
        }
        poketchSys.CurrAppShutdown!(poketchSys.AppData);
        poketchSys.AppState = AppStateShutdown;
    }

    // OFFSET 0x02254228
    private static bool PoketchSystem_IsAppShutdown(PoketchSystem poketchSys)
    {
        return poketchSys.AppState == AppStateNone;
    }

    // OFFSET 0x02254238
    public static void PoketchSystem_SetAppFunctions(PoketchAppInitFunction initFunction, PoketchAppShutdownFunction shutdownFunction)
    {
        PoketchSystem poketchSys = PoketchSystem_Get();
        poketchSys.CurrAppInit = initFunction;
        poketchSys.CurrAppShutdown = shutdownFunction;
        poketchSys.CurrAppSave = null;
    }

    // OFFSET 0x0225424C
    public static void PoketchSystem_NotifyAppLoaded(PoketchSystem poketchSys)
    {
        if (poketchSys.AppState != AppStateLoading)
        {
            ErrorHandling.GF_AssertFail();
        }
        poketchSys.AppState = AppStateRunning;
    }

    // OFFSET 0x02254260
    public static void PoketchSystem_NotifyAppUnloaded(PoketchSystem poketchSys)
    {
        if (poketchSys.AppState != AppStateShutdown)
        {
            ErrorHandling.GF_AssertFail();
        }
        poketchSys.AppState = AppStateNone;
    }

    // OFFSET 0x02254274
    public static void PoketchSystem_SetSaveFunction(PoketchAppSaveFunction saveFunction, object? saveData)
    {
        PoketchSystem poketchSys = PoketchSystem_Get();
        poketchSys.CurrAppSave = saveFunction;
        poketchSys.AppSaveData = saveData;
    }

    // OFFSET 0x02254284
    private static bool PoketchSystem_InitButtonManager(PoketchSystem poketchSys)
    {
        TouchScreenHitTable[] hitTable = PoketchButtons.HitTableFromRom(PoketchOverlay, ButtonHitTable, (int)NumButtons);
        poketchSys.ButtonManager = PoketchButtons.PoketchButtonManager_New(hitTable, NumButtons, PoketchSystem_OnButtonEvent, poketchSys, HeapIDPoketchApp);
        if (poketchSys.ButtonManager == null)
        {
            return false;
        }
        PoketchButtons.PoketchButtonManager_SetButtonTimer(poketchSys.ButtonManager, 0, 0, 7);
        poketchSys.NextBtnTask = NoTask;
        poketchSys.LastBtnReleaseTask = NoTask;
        return true;
    }

    // OFFSET 0x022542C8
    private static void PoketchSystem_FreeButtonManager(PoketchSystem poketchSys)
    {
        PoketchButtons.PoketchButtonManager_Free(poketchSys.ButtonManager);
    }

    // OFFSET 0x022542D4
    private static void PoketchSystem_UpdateButtons(PoketchSystem poketchSys)
    {
        poketchSys.ButtonState = 0;
        PoketchButtons.PoketchButtonManager_Update(poketchSys.ButtonManager!);
    }

    // OFFSET 0x022542E4
    private static void PoketchSystem_OnButtonEvent(uint buttonIdx, uint buttonState, uint touchState, object? data)
    {
        PoketchSystem poketchSys = (PoketchSystem)data!;
        if (!PoketchSystem_IsRunningTask(poketchSys))
        {
            if (touchState == 0)
            {
                poketchSys.TouchingScreen = 0;
            }
            else if (touchState == 1)
            {
                poketchSys.TouchingScreen = 1;
            }
        }
        else
        {
            poketchSys.TouchingScreen = 0;
        }
        if (buttonIdx == ButtonIndexScreen)
        {
            if (PoketchSystem_IsRunningTask(poketchSys) && touchState == 1)
            {
                SoundPlayback.Sound_PlayEffect(SeqPoketchBusy);
            }
            return;
        }
        uint task = NoTask;
        if (touchState == 0)
        {
            task = buttonIdx == 0 ? PoketchGraphics.TaskButtonTopReleased : PoketchGraphics.TaskButtonBottomReleased;
        }
        else if (touchState == 1)
        {
            if (PoketchSystem_IsRunningTask(poketchSys) || poketchSys.AppStarting != 0)
            {
                task = buttonIdx == 0 ? PoketchGraphics.TaskButtonTopPressed : PoketchGraphics.TaskButtonBottomPressed;
                buttonState = 0;
            }
            else
            {
                task = buttonIdx == 0 ? PoketchGraphics.TaskButtonTopHeld : PoketchGraphics.TaskButtonBottomHeld;
            }
        }
        if (poketchSys.NextBtnTask != NoTask)
        {
            (task, poketchSys.NextBtnTask) = (poketchSys.NextBtnTask, task);
        }
        if (task != NoTask && PoketchSystem_ButtonSetTask(poketchSys.TaskData!, task))
        {
            if (task == PoketchGraphics.TaskButtonTopPressed || task == PoketchGraphics.TaskButtonTopHeld || task == PoketchGraphics.TaskButtonBottomPressed || task == PoketchGraphics.TaskButtonBottomHeld)
            {
                poketchSys.LastBtnReleaseTask = task;
            }
        }
        if (buttonState == PoketchButtons.ButtonStateReleased)
        {
            if (poketchSys.LastBtnReleaseTask == PoketchGraphics.TaskButtonTopHeld || poketchSys.LastBtnReleaseTask == PoketchGraphics.TaskButtonBottomHeld)
            {
                buttonState = PoketchButtons.ButtonStateTapped;
            }
        }
        else if (buttonState == PoketchButtons.ButtonStateTapped)
        {
            if (poketchSys.LastBtnReleaseTask == PoketchGraphics.TaskButtonTopPressed || poketchSys.LastBtnReleaseTask == PoketchGraphics.TaskButtonBottomPressed)
            {
                buttonState = 0;
            }
        }
        else if (buttonState == PoketchButtons.ButtonStateTimer0)
        {
            PoketchButtons.PoketchButtonManager_ResetButtonState(poketchSys.ButtonManager!, 0);
        }
        poketchSys.ButtonState = buttonState;
        poketchSys.ButtonDir = buttonIdx == 0 ? 0u : 1u;
    }

    // OFFSET 0x022543EC
    private static bool PoketchSystem_ButtonSetTask(PoketchGraphicsTaskData taskData, uint task)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!PoketchGraphics.PoketchGraphics_TaskIsNotActive(taskData, task))
            {
                return false;
            }
        }
        PoketchGraphics.PoketchGraphics_StartTask(taskData, task);
        return true;
    }

    // OFFSET 0x02254418
    public static PoketchGraphicsTaskData PoketchSystem_GetTaskData()
    {
        return PoketchSystem_Get().TaskData!;
    }

    // OFFSET 0x02254424
    public static void PoketchSystem_PlaySoundEffect(uint soundID)
    {
        PoketchSystem poketchSys = PoketchSystem_Get();
        if (poketchSys.AppChanging == 0 && !PoketchSystem_IsRunningTask(poketchSys))
        {
            SoundPlayback.Sound_PlayEffect((ushort)soundID);
        }
    }

    // OFFSET 0x02254444
    public static void PoketchSystem_PlayCry(uint species, uint form)
    {
        PoketchSystem poketchSys = PoketchSystem_Get();
        if (poketchSys.AppChanging == 0 && !PoketchSystem_IsRunningTask(poketchSys))
        {
            SoundPlayback.Sound_PlayPokemonCry((ushort)species, (byte)form);
        }
    }

    // OFFSET 0x0225446C
    public static bool PoketchSystem_GetDisplayHeldCoords(ref uint x, ref uint y)
    {
        PoketchSystem poketchSys = PoketchSystem_Get();
        if (poketchSys.AppChanging != 0 || PoketchSystem_IsRunningTask(poketchSys) || !TouchScreen.TouchScreen_GetHoldState(ref x, ref y))
        {
            return false;
        }
        return x - 16 < 0xBF && y - 16 < 0x9F;
    }

    // OFFSET 0x022544BC
    public static bool PoketchSystem_GetDisplayTappedCoords(ref uint x, ref uint y)
    {
        PoketchSystem poketchSys = PoketchSystem_Get();
        if (poketchSys.AppChanging != 0 || PoketchSystem_IsRunningTask(poketchSys) || !TouchScreen.TouchScreen_GetTapState(ref x, ref y))
        {
            return false;
        }
        return x - 16 < 0xBF && y - 16 < 0x9F;
    }

    // OFFSET 0x0225450C
    public static bool PoketchSystem_IsRunningTask(PoketchSystem poketchSys)
    {
        return FieldTask.FieldSystem_IsRunningTask(poketchSys.FieldSystem!);
    }

    // OFFSET 0x02254518
    public static void PoketchSystem_UpdateButtonManager(PoketchSystem poketchSys, PoketchButtonManager buttonManager)
    {
        if (!PoketchSystem_IsRunningTask(poketchSys) && poketchSys.AppChanging == 0)
        {
            PoketchButtons.PoketchButtonManager_Update(buttonManager);
        }
    }

    // OFFSET 0x02254534
    public static bool PoketchSystem_IsPlayerMoving(PoketchSystem poketchSys)
    {
        return poketchSys.PlayerMoving != 0;
    }

    // OFFSET 0x02254538
    public static bool PoketchSystem_PedometerUpdated(PoketchSystem poketchSys)
    {
        return poketchSys.PedometerUpdated != 0;
    }

    // OFFSET 0x0225453C
    public static FieldSystem PoketchSystem_GetFieldSystem(PoketchSystem poketchSys)
    {
        return poketchSys.FieldSystem!;
    }

    // OFFSET 0x02254540
    public static Poketch PoketchSystem_GetPoketchData(PoketchSystem poketchSys)
    {
        return poketchSys.Poketch!;
    }

    // OFFSET 0x02254544
    public static SaveData PoketchSystem_GetSaveData(PoketchSystem poketchSys)
    {
        return poketchSys.SaveData!;
    }

    // OFFSET 0x02254548
    public static int PoketchSystem_GetBorderColor(PoketchSystem poketchSys)
    {
        if (TrainerInfo.TrainerInfo_Gender(PlayerSave.SaveData_GetTrainerInfo(poketchSys.SaveData!)) == 1)
        {
            return 0;
        }
        return 1;
    }
}
