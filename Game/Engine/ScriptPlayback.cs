using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public static class ScriptPlayback
{
    public const int NoFile = -1;
    public const int NoCommand = -1;
    public const int NoObject = -1;

    private const int FaceSouth = 1;
    private const int SignpostCommandHide = 4;

    private sealed class WarpRequest
    {
        public int Map;
        public int Warp;
        public int X;
        public int Z;
    }

    private static readonly object gate = new();
    private static WarpRequest? pendingWarp;
    private static int pendingScript = NoCommand;
    private static int pendingObject = NoObject;
    private static int pendingSteps;

    public static volatile bool Enabled;
    public static volatile bool Paused;
    public static volatile int CurrentFile = NoFile;
    public static volatile int CurrentOffset;
    public static volatile int BlockedCommand = NoCommand;
    public static volatile int CommandsRun;
    public static volatile bool ScriptActive;
    public static volatile bool InField;
    public static volatile bool FieldIdle;
    public static volatile int Map;
    public static volatile int PlayerX;
    public static volatile int PlayerZ;
    public static volatile int RunningScript;
    public static volatile string Dialogue = "";

    public static bool Hold(ScriptContext ctx)
    {
        if (!Enabled || ctx.Scripts == null || ctx.ScriptPtr < 0 || ctx.ScriptPtr + 1 >= ctx.Scripts.Length)
        {
            return false;
        }
        int command = ctx.Scripts[ctx.ScriptPtr] | (ctx.Scripts[ctx.ScriptPtr + 1] << 8);
        CurrentFile = ctx.ScriptFile;
        CurrentOffset = ctx.ScriptPtr;
        if (command < ScriptCommands.TableSize && !ScriptCommands.IsPorted(command))
        {
            BlockedCommand = command;
            Paused = true;
            return true;
        }
        if (Paused)
        {
            lock (gate)
            {
                if (pendingSteps > 0)
                {
                    pendingSteps--;
                    CommandsRun++;
                    return false;
                }
            }
            return true;
        }
        CommandsRun++;
        return false;
    }

    public static void StepCommand()
    {
        lock (gate)
        {
            pendingSteps++;
        }
    }

    public static void RequestWarp(int map, int warp, int x, int z)
    {
        lock (gate)
        {
            pendingWarp = new WarpRequest { Map = map, Warp = warp, X = x, Z = z };
        }
    }

    public static void RequestScript(int scriptID, int objectID)
    {
        lock (gate)
        {
            pendingScript = scriptID;
            pendingObject = objectID;
        }
    }

    public static void Update()
    {
        if (sApplication.CurrentManager?.Data is not FieldSystem fieldSystem || fieldSystem.PlayerAvatar == null || fieldSystem.PlayerAvatar.MapObject == null || fieldSystem.Location == null)
        {
            InField = false;
            FieldIdle = false;
            ScriptActive = false;
            return;
        }
        InField = true;
        Map = (int)fieldSystem.Location.U32(0);
        PlayerX = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar);
        PlayerZ = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar);
        bool idle = fieldSystem.Task == null && fieldSystem.RunningFieldMap != 0;
        FieldIdle = idle;
        bool active = false;
        for (FieldTask? task = fieldSystem.Task; task != null; task = task.Previous)
        {
            if (task.Environment is ScriptManager scriptManager)
            {
                active = true;
                RunningScript = scriptManager.ScriptID;
                if (scriptManager.IsMessageBoxOpen != 0)
                {
                    string text = TestWorld.Decode(scriptManager.MessageBuffer);
                    if (text.Length > 0)
                    {
                        Dialogue = text;
                    }
                }
                break;
            }
        }
        ScriptActive = active;
        if (!active)
        {
            CurrentFile = NoFile;
            BlockedCommand = NoCommand;
        }
        if (!idle)
        {
            return;
        }
        WarpRequest? warp;
        int script;
        int objectID;
        lock (gate)
        {
            warp = pendingWarp;
            pendingWarp = null;
            script = warp == null ? pendingScript : NoCommand;
            objectID = pendingObject;
            if (warp == null)
            {
                pendingScript = NoCommand;
            }
        }
        if (warp == null && script == NoCommand)
        {
            return;
        }
        MapNamePopUps.MapNamePopUp_Hide(fieldSystem.FieldMapContext!.MapNamePopUp);
        Signposts.Signpost_DoCommand(fieldSystem, SignpostCommandHide);
        if (warp != null)
        {
            FieldTask.FieldSystem_CreateTask(fieldSystem, WarpTask, warp);
            return;
        }
        if (script != NoCommand)
        {
            MapObject? target = objectID == NoObject ? null : TestWorld.ObjectByLocalID(fieldSystem, objectID);
            Dialogue = "";
            ScriptManager.ScriptManager_Set(fieldSystem, (ushort)script, target);
        }
    }

    private static bool WarpTask(FieldTask task)
    {
        WarpRequest request = (WarpRequest)FieldTask.FieldTask_GetEnv(task)!;
        ref int state = ref FieldTask.FieldTask_GetState(task);
        if (state == 0)
        {
            state++;
            FieldMapChange.FieldTask_StartMapChangeFull(task, request.Map, request.Warp, request.X, request.Z, FaceSouth);
            return false;
        }
        return true;
    }
}
