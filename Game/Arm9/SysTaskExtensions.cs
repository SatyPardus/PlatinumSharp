using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class SysTaskExtensions
{
    // OFFSET 0x0200679C
    public static SysTask? SysTask_StartAndAllocateParam<T>(SysTaskFunc callback, int paramSize, uint priority, int heapID) where T : class, new()
    {
        object? param = null;
        if (paramSize != 0)
        {
            param = new T();
        }
        return SysTask.SysTask_Start(callback, param, priority);
    }

    // OFFSET 0x020067D0
    public static void SysTask_FinishAndFreeParam(SysTask task)
    {
        SysTaskManager.SysTask_GetParam(task);
        SysTask.SysTask_Done(task);
    }
}
