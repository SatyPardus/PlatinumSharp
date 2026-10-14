using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class BikeSlopeWatcherData
{
    public int X;
    public int Z;
}

public static class BikeSlopeWatcher
{
    private const uint TaskPriority = 2;
    private const ushort TaskDataSize = 8;

    private static readonly RenderTaskTemplate sTemplate = new()
    {
        Priority = TaskPriority,
        DataSize = TaskDataSize,
        DataFactory = () => new BikeSlopeWatcherData(),
        Init = BikeSlopeWatcher_Init,
        Free = BikeSlopeWatcher_Free,
        Update = BikeSlopeWatcher_Update,
        Draw = null,
    };

    // OFFSET 0x021EE75C
    private static void BikeSlopeWatcher_Init(RenderTask task, FieldSystem fieldSystem, object? data)
    {
        BikeSlopeWatcherData watcher = (BikeSlopeWatcherData)data!;
        watcher.X = 0;
        watcher.Z = 0;
    }

    // OFFSET 0x021EE764
    private static void BikeSlopeWatcher_Free(RenderTask task, FieldSystem fieldSystem, object? data)
    {
    }

    // OFFSET 0x021EE768
    private static void BikeSlopeWatcher_Update(RenderTask task, FieldSystem fieldSystem, object? data)
    {
        BikeSlopeWatcherData watcher = (BikeSlopeWatcherData)data!;
        int x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        int z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        if (watcher.X == x && watcher.Z == z)
        {
            return;
        }
        watcher.X = x;
        watcher.Z = z;
        int tileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
        int slopeTop;
        if (TileBehavior.TileBehavior_IsBikeSlopeBottom(tileBehavior) != 0)
        {
            slopeTop = 0;
        }
        else if (TileBehavior.TileBehavior_IsBikeSlopeTop(tileBehavior) != 0)
        {
            slopeTop = 1;
        }
        else
        {
            return;
        }
        DoorAnimations.FieldSystem_StartBikeSlopeAnimation(x, z, slopeTop, fieldSystem);
    }

    // OFFSET 0x021EE7C0
    public static void BikeSlopeWatcher_Start(FieldSystem fieldSystem)
    {
        RenderTaskManager.RenderTaskManager_AddTask(fieldSystem.FieldMapContext!.RenderTaskManager!, sTemplate);
    }
}
