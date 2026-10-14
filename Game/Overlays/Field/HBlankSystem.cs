namespace PlatinumSharp.Game;

public delegate void HBlankTaskCallback(HBlankTask task, object? param);

public sealed class HBlankTask
{
    public const int Size = 0x0C;

    public int Used;
    public object? Param;
    public HBlankTaskCallback? Callback;
}

public sealed class HBlankSystem
{
    public const int MaxTasks = 2;
    public const int Size = 0x1C;

    public int Active;
    public readonly HBlankTask[] Tasks = new HBlankTask[MaxTasks];

    public HBlankSystem()
    {
        for (int i = 0; i < MaxTasks; i++)
        {
            Tasks[i] = new HBlankTask();
        }
    }

    // OFFSET 0x021EF3A8
    public static HBlankSystem HBlankSystem_New(int heapID)
    {
        HBlankSystem hBlankSystem = new();
        HBlankSystem_Init(hBlankSystem);
        return hBlankSystem;
    }

    // OFFSET 0x021EF3BC
    public static void HBlankSystem_Delete(HBlankSystem? hBlankSystem)
    {
        if (hBlankSystem != null)
        {
            if (hBlankSystem.Active == 1)
            {
                HBlankSystem_Stop(hBlankSystem);
            }
            HBlankSystem_Init(hBlankSystem);
        }
    }

    // OFFSET 0x021EF3DC
    public static void HBlankSystem_Start(HBlankSystem hBlankSystem)
    {
        if (!GameSystem.SetHBlankCallback(HBlankSystem_RunTasks, hBlankSystem))
        {
            ErrorHandling.GF_AssertFail();
        }
        hBlankSystem.Active = 1;
    }

    // OFFSET 0x021EF3FC
    public static void HBlankSystem_Stop(HBlankSystem hBlankSystem)
    {
        if (!GameSystem.SetHBlankCallback(null, null))
        {
            ErrorHandling.GF_AssertFail();
        }
        hBlankSystem.Active = 0;
    }

    // OFFSET 0x021EF418
    public static HBlankTask? HBlankSystem_StartTask(HBlankSystem hBlankSystem, HBlankTaskCallback callback, object? param)
    {
        HBlankTask? task = HBlankSystem_FindFreeTask(hBlankSystem);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (task != null)
        {
            task.Param = param;
            task.Callback = callback;
            task.Used = 1;
        }
        return task;
    }

    // OFFSET 0x021EF43C
    public static void HBlankTask_Delete(HBlankTask task)
    {
        HBlankTask_Reset(task);
    }

    // OFFSET 0x021EF444
    private static void HBlankSystem_Init(HBlankSystem hBlankSystem)
    {
        hBlankSystem.Active = 0;
        for (int i = 0; i < MaxTasks; i++)
        {
            HBlankTask_Reset(hBlankSystem.Tasks[i]);
        }
    }

    // OFFSET 0x021EF46C
    private static void HBlankTask_Reset(HBlankTask task)
    {
        task.Used = 0;
        task.Callback = HBlankTask_DummyCallback;
        task.Param = null;
    }

    // OFFSET 0x021EF47C
    private static void HBlankSystem_RunTasks(object? data)
    {
        HBlankSystem hBlankSystem = (HBlankSystem)data!;
        for (int i = 0; i < MaxTasks; i++)
        {
            HBlankTask task = hBlankSystem.Tasks[i];
            task.Callback!(task, task.Param);
        }
    }

    // OFFSET 0x021EF498
    private static void HBlankTask_DummyCallback(HBlankTask task, object? param)
    {
    }

    // OFFSET 0x021EF49C
    private static HBlankTask? HBlankSystem_FindFreeTask(HBlankSystem hBlankSystem)
    {
        for (int i = 0; i < MaxTasks; i++)
        {
            if (hBlankSystem.Tasks[i].Used == 0)
            {
                return hBlankSystem.Tasks[i];
            }
        }
        return null;
    }
}
