namespace PlatinumSharp.Game;

public delegate object? OverworldAnimContextFactory();

public delegate int OverworldAnimInitFunc(OverworldAnimManager animManager, object? context);

public delegate void OverworldAnimFunc(OverworldAnimManager animManager, object? context);

public sealed class OverworldAnimManagerFuncs
{
    public const int Size = 0x14;
    public const int ExtendedSize = 0x1C;

    public readonly uint ContextSize;
    public readonly OverworldAnimContextFactory? ContextFactory;
    public readonly OverworldAnimInitFunc Init;
    public readonly OverworldAnimFunc Exit;
    public readonly OverworldAnimFunc Tick;
    public readonly OverworldAnimFunc Render;
    public OverworldAnimFunc? Unused14;
    public OverworldAnimFunc? Unused18;

    public OverworldAnimManagerFuncs(uint contextSize, OverworldAnimContextFactory? contextFactory, OverworldAnimInitFunc init, OverworldAnimFunc exit, OverworldAnimFunc tick, OverworldAnimFunc render)
    {
        ContextSize = contextSize;
        ContextFactory = contextFactory;
        Init = init;
        Exit = exit;
        Tick = tick;
        Render = render;
    }
}

public sealed class OverworldAnimManagerList
{
    public const int Size = 0x10;

    public int Capacity;
    public int ActiveCount;
    public int HeapID;
    public OverworldAnimManager[]? Managers;
}

public sealed class OverworldAnimManager
{
    public const int Size = 0xE8;
    public const int ContextCapacity = 0xA0;

    public uint Flags;
    public int UserInt;
    public int Unused08;
    public object? UserData;
    public SysTask? Task;
    public OverworldAnimManagerList? List;
    public VecFx32 Position;
    public object? Context;
    public OverworldAnimInitFunc? InitFunc;
    public OverworldAnimFunc? TickFunc;
    public OverworldAnimFunc? RenderFunc;
    public OverworldAnimFunc? ExitFunc;
    public OverworldAnimFunc? Unused14Func;
    public OverworldAnimFunc? Unused18Func;
}

public static class OverworldAnimManagers
{
    private const uint FlagInUse = 1;
    private const uint FlagInitialized = 2;

    // OFFSET 0x020711EC
    public static OverworldAnimManagerList OverworldAnimManagerList_New(int heapID, int capacity)
    {
        OverworldAnimManagerList list = AllocList(heapID);
        list.Managers = AllocManagers(heapID, capacity);
        list.Capacity = capacity;
        list.HeapID = heapID;
        return list;
    }

    // OFFSET 0x0207120C
    public static void OverworldAnimManagerList_Free(OverworldAnimManagerList list)
    {
        FreeManagers(list);
        FreeList(list);
    }

    // OFFSET 0x0207121C
    public static void OverworldAnimManagerList_FinishAllAndFree(OverworldAnimManagerList list)
    {
        OverworldAnimManagerList_FinishAll(list);
        OverworldAnimManagerList_Free(list);
    }

    // OFFSET 0x0207122C
    public static OverworldAnimManager? OverworldAnimManagerList_InitManagerExtended(OverworldAnimManagerList list, OverworldAnimManagerFuncs funcs, VecFx32? position, int userInt, object? userData, int taskPriority)
    {
        int i = 0;
        int capacity = GetCapacity(list);
        OverworldAnimManager[] managers = GetManagers(list)!;
        OverworldAnimManager? animManager = null;
        while (i < capacity)
        {
            animManager = managers[i];
            if (!IsInUse(animManager))
            {
                break;
            }
            i++;
        }
        if (i >= capacity)
        {
            return null;
        }
        SetInUse(animManager!);
        SetUserInt(animManager!, userInt);
        SetUserData(animManager!, userData);
        SetList(animManager!, list);
        if (position != null)
        {
            OverworldAnimManager_SetPosition(animManager!, position.Value);
        }
        else
        {
            OverworldAnimManager_SetPosition(animManager!, default);
        }
        ClearContext(animManager!, funcs);
        SetInitFunc(animManager!, funcs.Init);
        SetExitFunc(animManager!, funcs.Exit);
        SetTickFunc(animManager!, funcs.Tick);
        SetRenderFunc(animManager!, funcs.Render);
        SetUnused14Func(animManager!, funcs.Unused14);
        SetUnused18Func(animManager!, funcs.Unused18);
        SysTask? task = StartTask(animManager!, taskPriority);
        if (task == null)
        {
            Reset(animManager!);
            return null;
        }
        SetTask(animManager!, task);
        if (CallInit(animManager!) == 0)
        {
            SysTask.SysTask_Done(task);
            Reset(animManager!);
            return null;
        }
        SetFlags(animManager!, FlagInitialized);
        list.ActiveCount++;
        return animManager;
    }

    // OFFSET 0x02071330
    public static OverworldAnimManager? OverworldAnimManagerList_InitManager(OverworldAnimManagerList list, OverworldAnimManagerFuncs funcs, VecFx32? position, int userInt, object? userData, int taskPriority)
    {
        OverworldAnimManagerFuncs extended = new(funcs.ContextSize, funcs.ContextFactory, funcs.Init, funcs.Exit, funcs.Tick, funcs.Render);
        extended.Unused14 = DefaultUnused14;
        extended.Unused18 = DefaultUnused18;
        return OverworldAnimManagerList_InitManagerExtended(list, extended, position, userInt, userData, taskPriority);
    }

    // OFFSET 0x0207136C
    public static void OverworldAnimManager_Finish(OverworldAnimManager? animManager)
    {
        if (animManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!IsInUse(animManager!))
        {
            return;
        }
        CallExit(animManager!);
        animManager!.List!.ActiveCount--;
        if (GetTask(animManager) != null)
        {
            SysTask.SysTask_Done(GetTask(animManager)!);
        }
        Reset(animManager);
    }

    // OFFSET 0x020713A4
    public static void OverworldAnimManagerList_FinishAll(OverworldAnimManagerList list)
    {
        int capacity = GetCapacity(list);
        OverworldAnimManager[] managers = GetManagers(list)!;
        int i = 0;
        do
        {
            if (IsInUse(managers[i]))
            {
                OverworldAnimManager_Finish(managers[i]);
            }
            i++;
        }
        while (--capacity != 0);
    }

    // OFFSET 0x020713D0
    public static void OverworldAnimManagerList_Render(OverworldAnimManagerList list)
    {
        int capacity = GetCapacity(list);
        OverworldAnimManager[] managers = GetManagers(list)!;
        int i = 0;
        do
        {
            if (CheckFlags(managers[i], FlagInUse | FlagInitialized) == (FlagInUse | FlagInitialized))
            {
                CallRender(managers[i]);
            }
            i++;
        }
        while (--capacity != 0);
    }

    // OFFSET 0x02071400
    private static SysTask? StartTask(OverworldAnimManager animManager, int taskPriority)
    {
        SysTask? task = SysTask.SysTask_Start(OverworldAnimManager_Task, animManager, (uint)taskPriority);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return task;
    }

    // OFFSET 0x02071420
    private static void OverworldAnimManager_Task(SysTask task, object? param)
    {
        OverworldAnimManager_CallTick((OverworldAnimManager)param!);
    }

    // OFFSET 0x0207142C
    private static OverworldAnimManagerList AllocList(int heapID)
    {
        return new OverworldAnimManagerList();
    }

    // OFFSET 0x02071450
    private static void FreeList(OverworldAnimManagerList list)
    {
        list.Managers = null;
    }

    // OFFSET 0x0207145C
    private static OverworldAnimManager[] AllocManagers(int heapID, int count)
    {
        OverworldAnimManager[] managers = new OverworldAnimManager[count];
        for (int i = 0; i < count; i++)
        {
            managers[i] = new OverworldAnimManager();
        }
        return managers;
    }

    // OFFSET 0x02071480
    private static void FreeManagers(OverworldAnimManagerList list)
    {
    }

    // OFFSET 0x02071490
    private static void Reset(OverworldAnimManager animManager)
    {
        animManager.Flags = 0;
        animManager.UserInt = 0;
        animManager.Unused08 = 0;
        animManager.UserData = null;
        animManager.Task = null;
        animManager.List = null;
        animManager.Position = default;
        animManager.Context = null;
        SetInitFunc(animManager, DefaultInit);
        SetExitFunc(animManager, DefaultExit);
        SetTickFunc(animManager, DefaultTick);
        SetRenderFunc(animManager, DefaultRender);
        SetUnused14Func(animManager, DefaultUnused14);
        SetUnused18Func(animManager, DefaultUnused18);
    }

    // OFFSET 0x020714E8
    private static int GetCapacity(OverworldAnimManagerList list)
    {
        return list.Capacity;
    }

    // OFFSET 0x020714EC
    private static OverworldAnimManager[]? GetManagers(OverworldAnimManagerList list)
    {
        return list.Managers;
    }

    // OFFSET 0x020714F0
    private static bool IsInUse(OverworldAnimManager animManager)
    {
        if ((animManager.Flags & FlagInUse) != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020714FC
    private static void SetInUse(OverworldAnimManager animManager)
    {
        animManager.Flags |= FlagInUse;
    }

    // OFFSET 0x02071508
    private static void SetFlags(OverworldAnimManager animManager, uint flags)
    {
        animManager.Flags |= flags;
    }

    // OFFSET 0x02071510
    private static uint CheckFlags(OverworldAnimManager animManager, uint flags)
    {
        return animManager.Flags & flags;
    }

    // OFFSET 0x02071518
    private static void SetInitFunc(OverworldAnimManager animManager, OverworldAnimInitFunc? func)
    {
        animManager.InitFunc = func;
    }

    // OFFSET 0x02071520
    private static int CallInit(OverworldAnimManager animManager)
    {
        return animManager.InitFunc!(animManager, OverworldAnimManager_GetContext(animManager));
    }

    // OFFSET 0x02071534
    private static void SetTickFunc(OverworldAnimManager animManager, OverworldAnimFunc? func)
    {
        animManager.TickFunc = func;
    }

    // OFFSET 0x0207153C
    public static void OverworldAnimManager_CallTick(OverworldAnimManager animManager)
    {
        animManager.TickFunc!(animManager, OverworldAnimManager_GetContext(animManager));
    }

    // OFFSET 0x02071550
    private static void SetRenderFunc(OverworldAnimManager animManager, OverworldAnimFunc? func)
    {
        animManager.RenderFunc = func;
    }

    // OFFSET 0x02071558
    private static void CallRender(OverworldAnimManager animManager)
    {
        animManager.RenderFunc!(animManager, OverworldAnimManager_GetContext(animManager));
    }

    // OFFSET 0x0207156C
    private static void SetExitFunc(OverworldAnimManager animManager, OverworldAnimFunc? func)
    {
        animManager.ExitFunc = func;
    }

    // OFFSET 0x02071574
    private static void CallExit(OverworldAnimManager animManager)
    {
        animManager.ExitFunc!(animManager, OverworldAnimManager_GetContext(animManager));
    }

    // OFFSET 0x02071588
    private static void SetUnused14Func(OverworldAnimManager animManager, OverworldAnimFunc? func)
    {
        animManager.Unused14Func = func;
    }

    // OFFSET 0x02071590
    private static void SetUnused18Func(OverworldAnimManager animManager, OverworldAnimFunc? func)
    {
        animManager.Unused18Func = func;
    }

    // OFFSET 0x02071598
    public static object? OverworldAnimManager_GetContext(OverworldAnimManager animManager)
    {
        return animManager.Context;
    }

    // OFFSET 0x0207159C
    private static void ClearContext(OverworldAnimManager animManager, OverworldAnimManagerFuncs funcs)
    {
        if (funcs.ContextSize > OverworldAnimManager.ContextCapacity)
        {
            ErrorHandling.GF_AssertFail();
        }
        animManager.Context = funcs.ContextFactory != null ? funcs.ContextFactory() : null;
    }

    // OFFSET 0x020715B0
    private static void SetUserInt(OverworldAnimManager animManager, int userInt)
    {
        animManager.UserInt = userInt;
    }

    // OFFSET 0x020715B4
    public static int OverworldAnimManager_GetUserInt(OverworldAnimManager animManager)
    {
        return animManager.UserInt;
    }

    // OFFSET 0x020715B8
    private static void SetUserData(OverworldAnimManager animManager, object? userData)
    {
        animManager.UserData = userData;
    }

    // OFFSET 0x020715BC
    public static object? OverworldAnimManager_GetUserData(OverworldAnimManager animManager)
    {
        return animManager.UserData;
    }

    // OFFSET 0x020715C0
    public static void OverworldAnimManager_SetUnused08(OverworldAnimManager animManager, int value)
    {
        animManager.Unused08 = value;
    }

    // OFFSET 0x020715C4
    public static int OverworldAnimManager_GetUnused08(OverworldAnimManager animManager)
    {
        return animManager.Unused08;
    }

    // OFFSET 0x020715C8
    private static void SetTask(OverworldAnimManager animManager, SysTask? task)
    {
        animManager.Task = task;
    }

    // OFFSET 0x020715CC
    private static SysTask? GetTask(OverworldAnimManager animManager)
    {
        return animManager.Task;
    }

    // OFFSET 0x020715D0
    private static void SetList(OverworldAnimManager animManager, OverworldAnimManagerList list)
    {
        animManager.List = list;
    }

    // OFFSET 0x020715D4
    public static void OverworldAnimManager_SetPosition(OverworldAnimManager animManager, in VecFx32 position)
    {
        animManager.Position = position;
    }

    // OFFSET 0x020715E4
    public static void OverworldAnimManager_GetPosition(OverworldAnimManager animManager, out VecFx32 position)
    {
        position = animManager.Position;
    }

    // OFFSET 0x020715F4
    private static int DefaultInit(OverworldAnimManager animManager, object? context)
    {
        return 1;
    }

    // OFFSET 0x020715F8
    private static void DefaultTick(OverworldAnimManager animManager, object? context)
    {
    }

    // OFFSET 0x020715FC
    private static void DefaultRender(OverworldAnimManager animManager, object? context)
    {
    }

    // OFFSET 0x02071600
    private static void DefaultExit(OverworldAnimManager animManager, object? context)
    {
    }

    // OFFSET 0x02071604
    private static void DefaultUnused14(OverworldAnimManager animManager, object? context)
    {
    }

    // OFFSET 0x02071608
    private static void DefaultUnused18(OverworldAnimManager animManager, object? context)
    {
    }
}
