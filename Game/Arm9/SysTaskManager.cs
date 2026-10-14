using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void SysTaskFunc(SysTask task, object? param);

public sealed class SysTask
{
    public SysTaskManager? Manager;
    public SysTask? Prev;
    public SysTask? Next;
    public uint Priority;
    public object? Param;
    public SysTaskFunc? Callback;
    public int Deferred;

    // OFFSET 0x0200D9E8
    public static SysTask? SysTask_Start(SysTaskFunc callback, object? param, uint priority)
    {
        return SysTaskManager.SysTaskManager_AddTask(Globals.gSystem.MainTaskManager!, callback, param, priority);
    }

    // OFFSET 0x0200DA04
    public static SysTask? SysTask_ExecuteOnVBlank(SysTaskFunc callback, object? param, uint priority)
    {
        return SysTaskManager.SysTaskManager_AddTask(Globals.gSystem.VBlankTaskManager!, callback, param, priority);
    }

    // OFFSET 0x0200DA20
    public static SysTask? SysTask_CreateOnPrintQueue(SysTaskFunc callback, object? param, uint priority)
    {
        return SysTaskManager.SysTaskManager_AddTask(Globals.gSystem.PrintTaskManager!, callback, param, priority);
    }

    // OFFSET 0x0200DA3C
    public static SysTask? SysTask_ExecuteAfterVBlank(SysTaskFunc callback, object? param, uint priority)
    {
        return SysTaskManager.SysTaskManager_AddTask(Globals.gSystem.PostVBlankTaskManager!, callback, param, priority);
    }

    // OFFSET 0x0200DA58
    public static void SysTask_Done(SysTask task)
    {
        SysTaskManager.SysTask_Delete(task);
    }
}

public sealed class SysTaskManager
{
    public ushort MaxTasks;
    public ushort StackPointer;
    public readonly SysTask Sentinel = new();
    public SysTask[] Stack = Array.Empty<SysTask>();
    public SysTask[] Tasks = Array.Empty<SysTask>();
    public int IsAdding;
    public SysTask? Current;
    public SysTask? NextTask;

    // OFFSET 0x0201CCF0
    private static void SysTaskManager_InitTask(SysTaskManager manager, SysTask task)
    {
        task.Manager = manager;
        task.Next = manager.Sentinel;
        task.Prev = manager.Sentinel;
        task.Priority = 0;
        task.Param = null;
        task.Callback = null;
    }

    // OFFSET 0x0201CD04
    private static void SysTaskManager_InitTasks(SysTaskManager manager)
    {
        for (int i = 0; i < manager.MaxTasks; i++)
        {
            SysTaskManager_InitTask(manager, manager.Tasks[i]);
            manager.Stack[i] = manager.Tasks[i];
        }
        manager.StackPointer = 0;
    }

    // OFFSET 0x0201CD38
    private static SysTask? SysTaskManager_AllocTask(SysTaskManager manager)
    {
        if (manager.StackPointer == manager.MaxTasks)
        {
            return null;
        }
        SysTask task = manager.Stack[manager.StackPointer];
        manager.StackPointer++;
        return task;
    }

    // OFFSET 0x0201CD54
    private static bool SysTaskManager_FreeTask(SysTaskManager manager, SysTask task)
    {
        if (manager.StackPointer == 0)
        {
            return false;
        }
        task.Manager = manager;
        task.Next = manager.Sentinel;
        task.Prev = manager.Sentinel;
        task.Priority = 0;
        task.Param = null;
        task.Callback = null;
        manager.StackPointer--;
        manager.Stack[manager.StackPointer] = task;
        return true;
    }

    // OFFSET 0x0201CD80
    public static uint SysTaskManager_GetRequiredSize(uint maxTasks)
    {
        return maxTasks * 0x20 + 0x34;
    }

    // OFFSET 0x0201CD88
    public static SysTaskManager SysTaskManager_Init(uint maxTasks, SysTaskManager? memory)
    {
        if (memory == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SysTaskManager manager = memory!;
        manager.Stack = new SysTask[maxTasks];
        manager.Tasks = new SysTask[maxTasks];
        for (int i = 0; i < maxTasks; i++)
        {
            manager.Tasks[i] = new SysTask();
        }
        manager.MaxTasks = (ushort)maxTasks;
        manager.StackPointer = 0;
        manager.IsAdding = 0;
        SysTaskManager_InternalInit(manager);
        return manager;
    }

    // OFFSET 0x0201CDB4
    public static void SysTaskManager_InternalInit(SysTaskManager manager)
    {
        SysTaskManager_InitTasks(manager);
        manager.Sentinel.Manager = manager;
        manager.Sentinel.Next = manager.Sentinel;
        manager.Sentinel.Prev = manager.Sentinel;
        manager.Sentinel.Priority = 0;
        manager.Sentinel.Param = null;
        manager.Sentinel.Callback = null;
        manager.Current = manager.Sentinel.Next;
    }

    // OFFSET 0x0201CDD4
    public static void SysTaskManager_ExecuteTasks(SysTaskManager manager)
    {
        if (manager.IsAdding != 0)
        {
            return;
        }
        manager.Current = manager.Sentinel.Next;
        while (manager.Current != manager.Sentinel)
        {
            manager.NextTask = manager.Current!.Next;
            if (manager.Current.Deferred == 0)
            {
                manager.Current.Callback?.Invoke(manager.Current, manager.Current.Param);
            }
            else
            {
                manager.Current.Deferred = 0;
            }
            manager.Current = manager.NextTask;
        }
        manager.Current.Callback = null;
    }

    // OFFSET 0x0201CE14
    public static SysTask? SysTaskManager_AddTask(SysTaskManager manager, SysTaskFunc callback, object? param, uint priority)
    {
        manager.IsAdding = 1;
        SysTask? task = SysTaskManager_InternalAddTask(manager, callback, param, priority);
        manager.IsAdding = 0;
        return task;
    }

    // OFFSET 0x0201CE28
    private static SysTask? SysTaskManager_InternalAddTask(SysTaskManager manager, SysTaskFunc callback, object? param, uint priority)
    {
        SysTask? task = SysTaskManager_AllocTask(manager);
        if (task == null)
        {
            return null;
        }
        task.Priority = priority;
        task.Param = param;
        task.Callback = callback;
        if (manager.Current!.Callback != null)
        {
            task.Deferred = manager.Current.Priority <= priority ? 1 : 0;
        }
        else
        {
            task.Deferred = 0;
        }
        for (SysTask iter = manager.Sentinel.Next!; iter != manager.Sentinel; iter = iter.Next!)
        {
            if (iter.Priority > task.Priority)
            {
                task.Prev = iter.Prev;
                task.Next = iter;
                iter.Prev!.Next = task;
                iter.Prev = task;
                if (iter == manager.NextTask)
                {
                    manager.NextTask = task;
                }
                return task;
            }
        }
        if (manager.NextTask == manager.Sentinel)
        {
            manager.NextTask = task;
        }
        task.Prev = manager.Sentinel.Prev;
        task.Next = manager.Sentinel;
        manager.Sentinel.Prev!.Next = task;
        manager.Sentinel.Prev = task;
        return task;
    }

    // OFFSET 0x0201CEA8
    public static void SysTask_Delete(SysTask task)
    {
        SysTaskManager manager = task.Manager!;
        if (manager.NextTask == task)
        {
            manager.NextTask = task.Next;
        }
        task.Prev!.Next = task.Next;
        task.Next!.Prev = task.Prev;
        SysTaskManager_FreeTask(task.Manager!, task);
    }

    // OFFSET 0x0201CECC
    public static void SysTask_SetCallback(SysTask task, SysTaskFunc? callback)
    {
        task.Callback = callback;
    }

    // OFFSET 0x0201CED0
    public static object? SysTask_GetParam(SysTask task)
    {
        return task.Param;
    }

    // OFFSET 0x0201CED4
    public static uint SysTask_GetPriority(SysTask task)
    {
        return task.Priority;
    }
}
