using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate bool FieldTaskFunc(FieldTask task);

public sealed class FieldTask
{
    public const int Size = 0x20;

    private const int HeapIDFieldTask = 0x20;

    public FieldTask? Previous;
    public FieldTaskFunc? Function;
    public int State;
    public object? Environment;
    public object? Unused10;
    public byte[]? Unused14;
    public FieldSystem FieldSystem = null!;
    public uint[] Dummy1C = null!;

    private sealed class ChildApplicationData
    {
        public int State;
        public ApplicationTemplate Template = null!;
        public object? Args;
    }

    // OFFSET 0x020508D4
    private static FieldTask FieldTask_CreateTaskManager(FieldSystem fieldSystem, FieldTaskFunc function, object? environment)
    {
        FieldTask task = new();
        task.Previous = null;
        task.Function = function;
        task.State = 0;
        task.Environment = environment;
        task.Unused10 = null;
        task.Unused14 = null;
        task.FieldSystem = fieldSystem;
        task.Dummy1C = new uint[1];
        return task;
    }

    // OFFSET 0x02050904
    public static FieldTask FieldSystem_CreateTask(FieldSystem fieldSystem, FieldTaskFunc function, object? environment)
    {
        if (fieldSystem.Task != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        FieldTask task = FieldTask_CreateTaskManager(fieldSystem, function, environment);
        fieldSystem.Task = task;
        return task;
    }

    // OFFSET 0x02050924
    public static void FieldTask_InitJump(FieldTask task, FieldTaskFunc function, object? environment)
    {
        task.Function = function;
        task.State = 0;
        task.Environment = environment;
        if (task.Unused14 != null)
        {
            Heap.Heap_Free(task.Unused14);
            task.Unused10 = null;
            task.Unused14 = null;
        }
    }

    // OFFSET 0x02050944
    public static FieldTask FieldTask_InitCall(FieldTask task, FieldTaskFunc function, object? environment)
    {
        FieldTask child = FieldTask_CreateTaskManager(task.FieldSystem, function, environment);
        child.Previous = task;
        task.FieldSystem.Task = child;
        return child;
    }

    // OFFSET 0x02050958
    public static bool FieldTask_Run(FieldSystem fieldSystem)
    {
        if (fieldSystem.Task == null)
        {
            return false;
        }
        FieldTask task = (FieldTask)fieldSystem.Task;
        while (task.Function!(task))
        {
            task = (FieldTask)fieldSystem.Task!;
            FieldTask? previous = task.Previous;
            if (task.Unused14 != null)
            {
                Heap.Heap_Free(task.Unused14);
            }
            fieldSystem.Task = previous;
            if (previous == null)
            {
                return true;
            }
            task = previous;
        }
        return false;
    }

    // OFFSET 0x020509A4
    public static bool FieldSystem_IsRunningTask(FieldSystem fieldSystem)
    {
        return fieldSystem.Task != null;
    }

    // OFFSET 0x020509B4
    public static bool FieldSystem_IsRunningApplication(FieldSystem fieldSystem)
    {
        if (FieldSystemCore.FieldSystem_HasParentProcess(fieldSystem) != 0 || FieldSystemCore.FieldSystem_HasChildProcess(fieldSystem) != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020509D4
    public static void FieldSystem_StartFieldMap(FieldSystem fieldSystem)
    {
        FieldSystemCore.FieldSystem_StartFieldMap(fieldSystem);
    }

    // OFFSET 0x020509DC
    public static bool FieldSystem_IsRunningFieldMap(FieldSystem fieldSystem)
    {
        if (FieldSystemCore.FieldSystem_IsRunningFieldMap(fieldSystem) != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020509F0
    private static bool FieldTask_RunChildApplication(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask_GetFieldSystem(task);
        ChildApplicationData data = (ChildApplicationData)FieldTask_GetEnv(task)!;
        switch (data.State)
        {
            case 0:
                FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, data.Template, data.Args);
                data.State++;
                break;
            case 1:
                if (!FieldSystem_IsRunningApplication(fieldSystem))
                {
                    return true;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x02050A38
    public static void FieldTask_RunApplication(FieldTask task, ApplicationTemplate template, object? args)
    {
        ChildApplicationData data = new();
        data.State = 0;
        data.Template = template;
        data.Args = args;
        FieldTask_InitCall(task, FieldTask_RunChildApplication, data);
    }

    // OFFSET 0x02050A60
    public static FieldSystem FieldTask_GetFieldSystem(FieldTask task)
    {
        return task.FieldSystem;
    }

    // OFFSET 0x02050A64
    public static object? FieldTask_GetEnv(FieldTask task)
    {
        return task.Environment;
    }

    // OFFSET 0x02050A68
    public static ref int FieldTask_GetState(FieldTask task)
    {
        return ref task.State;
    }

    // OFFSET 0x02050A6C
    public static uint FieldTask_GetDummy1CValue(FieldTask task)
    {
        return task.Dummy1C[0];
    }

    // OFFSET 0x02050A74
    public static bool FieldTask_RunBattleProcess(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask_GetFieldSystem(task);
        object? battleParams = FieldTask_GetEnv(task);
        ref int state = ref FieldTask_GetState(task);
        switch (state)
        {
            case 0:
                FieldSystemChildProcesses.FieldSystem_StartBattleProcess(fieldSystem, battleParams);
                state++;
                break;
            case 1:
                if (!FieldSystem_IsRunningApplication(fieldSystem))
                {
                    return true;
                }
                break;
        }
        return false;
    }
}
