namespace PlatinumSharp.Game;

public delegate void RenderTaskFunc(RenderTask task, FieldSystem fieldSystem, object? data);

public sealed class RenderTaskTemplate
{
    public const int Size = 0x18;

    public uint Priority;
    public ushort DataSize;
    public Func<object>? DataFactory;
    public RenderTaskFunc? Init;
    public RenderTaskFunc? Free;
    public RenderTaskFunc? Update;
    public RenderTaskFunc? Draw;
}

public sealed class RenderTask
{
    public const int Size = 0x14;

    public RenderTaskManager? Manager;
    public SysTask? UpdateTask;
    public SysTask? DrawTask;
    public RenderTaskTemplate? Template;
    public object? Data;
}

public sealed class RenderTaskManager
{
    public const int Size = 0x14;

    public FieldSystem? FieldSystem;
    public int HeapID;
    public int Capacity;
    public RenderTask[] Tasks = Array.Empty<RenderTask>();
    public SysTaskManager? DrawTasks;

    // OFFSET 0x021D1A94
    public static RenderTaskManager RenderTaskManager_New(FieldSystem fieldSystem, int heapID, int capacity)
    {
        RenderTaskManager manager = new();
        manager.FieldSystem = fieldSystem;
        manager.HeapID = heapID;
        manager.Capacity = capacity;
        manager.Tasks = new RenderTask[capacity];
        for (int i = 0; i < capacity; i++)
        {
            manager.Tasks[i] = new RenderTask();
        }
        manager.DrawTasks = SysTaskManager.SysTaskManager_Init((uint)capacity, new SysTaskManager());
        return manager;
    }

    // OFFSET 0x021D1AE4
    public static void RenderTaskManager_Free(RenderTaskManager manager)
    {
        for (int i = 0; i < manager.Capacity; i++)
        {
            RenderTask_Delete(manager.Tasks[i]);
        }
        manager.Tasks = Array.Empty<RenderTask>();
        manager.DrawTasks = null;
    }

    // OFFSET 0x021D1B18
    public static void RenderTaskManager_Draw(RenderTaskManager manager)
    {
        SysTaskManager.SysTaskManager_ExecuteTasks(manager.DrawTasks!);
    }

    // OFFSET 0x021D1B24
    private static void RenderTask_UpdateCallback(SysTask sysTask, object? param)
    {
        RenderTask task = (RenderTask)param!;
        if (FieldSystemCore.FieldSystem_IsRunningFieldMap(task.Manager!.FieldSystem!) != 0 && task.Template!.Update != null)
        {
            task.Template.Update(task, task.Manager.FieldSystem!, task.Data);
        }
    }

    // OFFSET 0x021D1B48
    private static void RenderTask_DrawCallback(SysTask sysTask, object? param)
    {
        RenderTask task = (RenderTask)param!;
        if (FieldSystemCore.FieldSystem_IsRunningFieldMap(task.Manager!.FieldSystem!) != 0 && task.Template!.Draw != null)
        {
            task.Template.Draw(task, task.Manager.FieldSystem!, task.Data);
        }
    }

    // OFFSET 0x021D1B6C
    public static RenderTask? RenderTaskManager_AddTask(RenderTaskManager manager, RenderTaskTemplate template)
    {
        for (int i = 0; i < manager.Capacity; i++)
        {
            RenderTask task = manager.Tasks[i];
            if (task.UpdateTask != null)
            {
                continue;
            }
            task.UpdateTask = SysTask.SysTask_Start(RenderTask_UpdateCallback, task, template.Priority);
            task.DrawTask = SysTaskManager.SysTaskManager_AddTask(manager.DrawTasks!, RenderTask_DrawCallback, task, template.Priority);
            task.Manager = manager;
            task.Template = template;
            if (task.UpdateTask == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            if (task.DrawTask == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            if (template.DataSize != 0)
            {
                task.Data = template.DataFactory != null ? template.DataFactory() : new byte[template.DataSize];
            }
            if (template.Init != null)
            {
                template.Init(task, manager.FieldSystem!, task.Data);
            }
            return task;
        }
        ErrorHandling.GF_AssertFail();
        return null;
    }

    // OFFSET 0x021D1BEC
    public static void RenderTask_Delete(RenderTask task)
    {
        if (task.UpdateTask == null)
        {
            return;
        }
        if (task.Template!.Free != null)
        {
            task.Template.Free(task, task.Manager!.FieldSystem!, task.Data);
        }
        SysTask.SysTask_Done(task.UpdateTask);
        SysTask.SysTask_Done(task.DrawTask!);
        task.Manager = null;
        task.UpdateTask = null;
        task.DrawTask = null;
        task.Template = null;
        task.Data = null;
    }

    // OFFSET 0x021D1C2C
    public static object? RenderTask_GetData(RenderTask task)
    {
        return task.Data;
    }
}
