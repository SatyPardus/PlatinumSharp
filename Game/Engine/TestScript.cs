using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed class TestScript
{
    public const string StateDirectory = @"E:\Poketest\tools\tests\states";
    public const uint OpeningTemplate = 0x021D788C;
    public const uint ContinueTemplate = 0x021D0F70;
    public const uint NewSaveTemplate = 0x021D0F90;
    public const uint RowanIntroTemplate = 0x020F6824;
    public const uint NewGameTemplate = 0x021D0F80;

    private const int KeyA = 0x001;
    private const int KeySelect = 0x004;
    private const int KeyStart = 0x008;
    private const int NameColumns = 10;
    private const int NameTapPeriod = 14;
    private const int KeyboardLeft = 35;
    private const int KeyboardTop = 100;
    private const int KeyboardCellSize = 16;
    private const int KeyboardRowHeight = 19;
    private const int OkButtonX = 216;
    private const int OkButtonY = 70;
    private const ushort LowerCaseButtonX = 72;
    private const ushort LowerCaseButtonY = 70;
    private const int KeyRight = 0x010;
    private const int KeyLeft = 0x020;
    private const int KeyUp = 0x040;
    private const int KeyDown = 0x080;
    private const int PulsePeriod = 16;
    private const int PulseHold = 4;
    private const int IdleFramesToFinishTalk = 40;
    private const int StuckFrames = 50;
    private const int DetourFrames = 40;
    private const int GotoTimeout = 2400;
    private const int NoScript = 0xFFFF;
    private const int MaxInterruptions = 2;
    private const int MaxSameScript = 3;
    private const int ExploreObjectRange = 40;
    private const int DefaultExploreActions = 30;
    private const string ExploreLogPath = @"E:\Poketest\tools\tests\task_log.txt";
    private const int PushFrames = 24;
    private const int IdleTimeout = 180;
    private const int MaxDetours = 8;
    private const int FieldTimeout = 3000;
    private const int TalkTimeout = 1500;
    private const int SaveResultFrames = 30;
    private const int TouchFrames = 6;
    private const int AppTimeout = 1500;

    public static Action<string>? Screenshot;

    private readonly List<string[]> steps = new();
    private int index;
    private int stepFrame;
    private int idleFrames;
    private int stuckFrames;
    private int detourFrames;
    private int detours;
    private bool generatedStep;
    private int exploreActions;
    private int exploreHash;
    private int exploreMap;
    private readonly HashSet<(int Map, int LocalID)> talked = new();
    private readonly HashSet<string> unreachable = new();
    private readonly HashSet<(int Map, int Script)> triggered = new();
    private bool sawBusy;
    private int loggedStep = -1;
    private int repeatedScript = -1;
    private readonly Dictionary<int, int> scriptStarts = new();
    private int loggedScript = -1;
    private string loggedText = "";

    public string LastDialogue { get; private set; } = "";
    private bool previousBusy;
    private int busyStarts;
    private readonly Dictionary<(int Map, int X, int Z), int> warpVisits = new();
    private int pushFrames;
    private int navigateFrames;
    private int pathIndex;
    private bool arrived;
    private bool talkStarted;
    private List<(int X, int Z)>? path;
    private readonly List<(int X, int Y)> nameKeys = new();
    private long progressKey;
    private int progressFrames;
    private int lastX = -1;
    private int lastZ = -1;

    public string Status { get; private set; } = "";
    public bool Finished => index >= steps.Count;
    public bool Failed { get; private set; }

    public TestScript(string path)
    {
        foreach (string raw in File.ReadAllLines(path))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }
            steps.Add(line.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }

    public static string PrepareBackup(string? stateName)
    {
        Directory.CreateDirectory(StateDirectory);
        string work = Path.Combine(StateDirectory, "_work.sav");
        File.Delete(work);
        if (stateName != null)
        {
            File.Copy(Path.Combine(StateDirectory, stateName + ".sav"), work);
            ApplicationTemplate.Redirects[OpeningTemplate] = ContinueTemplate;
        }
        return work;
    }

    public void SkipToState(string stateName)
    {
        for (int i = 0; i < steps.Count; i++)
        {
            if (steps[i][0] == "save" && steps[i][1] == stateName)
            {
                index = i + 1;
                return;
            }
        }
        Fail($"the test script has no 'save {stateName}' step");
    }

    private static string ApplicationName()
    {
        return sApplication.CurrentManager?.Main?.Method.DeclaringType?.Name ?? "none";
    }

    private static bool FieldState(out FieldSystem? field, out int map, out int x, out int z, out bool busy)
    {
        field = null;
        map = 0;
        x = 0;
        z = 0;
        busy = true;
        if (sApplication.CurrentManager?.Data is not FieldSystem fieldSystem || fieldSystem.PlayerAvatar == null || fieldSystem.PlayerAvatar.MapObject == null || fieldSystem.Location == null)
        {
            return false;
        }
        field = fieldSystem;
        map = (int)fieldSystem.Location.U32(0);
        x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar);
        z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar);
        busy = fieldSystem.Task != null || fieldSystem.RunningFieldMap == 0;
        return true;
    }

    private static string BusyReason()
    {
        if (sApplication.CurrentManager?.Data is not FieldSystem fieldSystem)
        {
            return "not in the field yet";
        }
        if (fieldSystem.Task == null)
        {
            return fieldSystem.RunningFieldMap == 0 ? "map loading" : "idle";
        }
        FieldTask task = fieldSystem.Task;
        string name = task.Function?.Method.Name ?? "?";
        if (task.Environment is ScriptManager scriptManager)
        {
            ScriptContext? ctx = scriptManager.Contexts[0];
            return $"script {scriptManager.ScriptID} ({ctx?.ShouldResume?.Method.Name ?? "running"})";
        }
        return name;
    }

    private bool NoProgress()
    {
        long key = 0;
        if (sApplication.CurrentManager?.Data is FieldSystem fieldSystem)
        {
            key = fieldSystem.Location?.U32(0) ?? 0;
            if (fieldSystem.PlayerAvatar != null && fieldSystem.PlayerAvatar.MapObject != null)
            {
                VecFx32 position = PlayerAvatar.PlayerAvatar_GetPos(fieldSystem.PlayerAvatar);
                key = key * 31 + position.X + position.Z * 7L;
            }
            if (fieldSystem.ProcessManager?.Child?.Data is BattleSystem battleSys && battleSys.BattleCtx != null)
            {
                BattleContext battleCtx = battleSys.BattleCtx;
                key = key * 31 + battleCtx.Command;
                key = key * 31 + battleCtx.TotalTurns;
                for (int i = 0; i < 4; i++)
                {
                    key = key * 31 + battleCtx.BattleMons[i].CurHP;
                }
            }
            for (FieldTask? task = fieldSystem.Task; task != null; task = task.Previous)
            {
                key = key * 31 + task.State + (task.Function?.Method.MetadataToken ?? 0);
                if (task.Environment is ScriptManager scriptManager)
                {
                    key = key * 31 + scriptManager.ScriptID + (scriptManager.Contexts[0]?.ScriptPtr ?? 0) * 13L + (scriptManager.Contexts[1]?.ScriptPtr ?? 0) * 17L;
                }
            }
        }
        if (key != progressKey)
        {
            progressKey = key;
            progressFrames = 0;
        }
        progressFrames++;
        return progressFrames > TalkTimeout;
    }

    private void BuildNameKeys(string text, bool confirm)
    {
        nameKeys.Clear();
        bool lower = false;
        foreach (char c in text)
        {
            if (char.IsLower(c) && !lower)
            {
                nameKeys.Add((LowerCaseButtonX, LowerCaseButtonY));
                nameKeys.Add((0, 0));
                lower = true;
            }
            int letter = char.ToUpperInvariant(c) - 'A';
            nameKeys.Add((KeyboardLeft + letter % NameColumns * KeyboardCellSize, KeyboardTop + letter / NameColumns * KeyboardRowHeight));
        }
        if (confirm)
        {
            nameKeys.Add((0, 0));
            nameKeys.Add((OkButtonX, OkButtonY));
        }
    }

    private int BusyKeys(string label, int x, int z)
    {
        Status = $"{label}\n  at ({x},{z}); pressing A, game is busy with: {BusyReason()}";
        if (NoProgress())
        {
            Fail($"{label}\n  FAILED: no progress for {TalkTimeout} frames, busy with: {BusyReason()}");
        }
        return Pulse();
    }

    private int Navigate(FieldSystem field, string label, int map, int x, int z, Func<int, int, bool> isGoal, string description)
    {
        if (x == lastX && z == lastZ)
        {
            stuckFrames++;
        }
        else
        {
            stuckFrames = 0;
            path = null;
        }
        lastX = x;
        lastZ = z;
        if (stuckFrames > StuckFrames && path != null && pathIndex + 1 < path.Count)
        {
            TestWorld.BlockedEdges.Add((x, z, path[pathIndex + 1].X, path[pathIndex + 1].Z));
            stuckFrames = 0;
            detours++;
            path = null;
        }
        if (detours > MaxDetours)
        {
            Fail($"{label}\n  FAILED: kept getting blocked on the way to {description}, stuck at ({x},{z}) on map {map}");
            return 0;
        }
        if (path == null)
        {
            path = TestWorld.FindPath(field, x, z, isGoal);
            pathIndex = 0;
            if (path == null)
            {
                Fail($"{label}\n  FAILED: no walkable route from ({x},{z}) to {description} on map {map}");
                return 0;
            }
        }
        if (path.Count < 2)
        {
            return 0;
        }
        Status = $"{label}\n  walking to {description}: at ({x},{z}) on map {map}, {path.Count - 1} tiles to go";
        navigateFrames++;
        if (navigateFrames > GotoTimeout)
        {
            Fail($"{label}\n  FAILED: could not reach {description}, stuck at ({x},{z})");
        }
        return TestWorld.KeyToward(x, z, path[1].X, path[1].Z);
    }

    private static string KeyName(int key)
    {
        switch (key)
        {
            case TestWorld.KeyRight:
                return "Right";
            case TestWorld.KeyLeft:
                return "Left";
            case TestWorld.KeyUp:
                return "Up";
            case TestWorld.KeyDown:
                return "Down";
        }
        return "nothing";
    }

    private int Pulse()
    {
        return stepFrame % PulsePeriod < PulseHold ? KeyA : 0;
    }

    private void Fail(string status)
    {
        if (generatedStep)
        {
            string reason = status.Contains("FAILED:") ? status.Substring(status.IndexOf("FAILED:") + 8) : status;
            Log($"# gave up: {string.Join(' ', steps[index])} ({reason})");
            unreachable.Add(string.Join(' ', steps[index]) + "@" + exploreMap);
            steps.RemoveAt(index);
            generatedStep = false;
            ResetStep();
            return;
        }
        Failed = true;
        Status = status;
        index = steps.Count;
    }

    private void LogDialogue()
    {
        if (sApplication.CurrentManager?.Data is not FieldSystem fieldSystem)
        {
            return;
        }
        for (FieldTask? task = fieldSystem.Task; task != null; task = task.Previous)
        {
            if (task.Environment is ScriptManager scriptManager)
            {
                if (scriptManager.ScriptID != loggedScript)
                {
                    loggedScript = scriptManager.ScriptID;
                    scriptStarts.TryGetValue(loggedScript, out int starts);
                    scriptStarts[loggedScript] = starts + 1;
                    if (starts + 1 > MaxSameScript)
                    {
                        repeatedScript = loggedScript;
                    }
                    MapObject? target = scriptManager.TargetObject;
                    Log($"    script {scriptManager.ScriptID} started" + (target != null ? $" (talking to object {MapObject.MapObject_GetLocalID(target)})" : ""));
                }
                if (scriptManager.IsMessageBoxOpen != 0)
                {
                    string text = TestWorld.Decode(scriptManager.MessageBuffer);
                    if (text.Length > 0 && text != loggedText)
                    {
                        loggedText = text;
                        LastDialogue = text;
                        Log($"    says: {text}");
                    }
                }
                return;
            }
        }
        if (loggedScript != -1)
        {
            loggedScript = -1;
            loggedText = "";
            Log("    script ended");
        }
    }

    private static void Log(string line)
    {
        Directory.CreateDirectory(StateDirectory);
        File.AppendAllText(ExploreLogPath, line + Environment.NewLine);
    }

    private static int StateHash(FieldSystem field)
    {
        byte[] buffer = VarsFlags.SaveData_GetVarsFlags(field.SaveData!).Buffer;
        int hash = 17;
        for (int i = 0; i < VarsFlags.Size; i++)
        {
            hash = hash * 31 + buffer[i];
        }
        return hash;
    }

    private string? ChooseExploreAction(FieldSystem field, int map, int x, int z)
    {
        int hash = StateHash(field);
        if (hash != exploreHash)
        {
            if (exploreHash != 0)
            {
                Log("# story state changed (flags or variables)");
            }
            exploreHash = hash;
            talked.Clear();
            triggered.Clear();
            unreachable.Clear();
        }
        exploreMap = map;
        MapHeaderData? header = field.MapHeaderData as MapHeaderData;
        if (header != null && header.CoordEvents != null)
        {
            foreach (CoordEvent coordEvent in header.CoordEvents)
            {
                if (FieldSystemVarsFlags.FieldSystem_TryGetVar(field, coordEvent.Var) != coordEvent.Value || triggered.Contains((map, coordEvent.Script)))
                {
                    continue;
                }
                for (int dz = 0; dz < Math.Max(1, (int)coordEvent.Length); dz++)
                {
                    for (int dx = 0; dx < Math.Max(1, (int)coordEvent.Width); dx++)
                    {
                        string action = $"goto {coordEvent.X + dx} {coordEvent.Z + dz}";
                        if (!unreachable.Contains(action + "@" + map) && !(coordEvent.X + dx == x && coordEvent.Z + dz == z))
                        {
                            triggered.Add((map, coordEvent.Script));
                            return action;
                        }
                    }
                }
            }
        }
        MapObject? player = field.PlayerAvatar?.MapObject;
        MapObject? nearest = null;
        int nearestDistance = int.MaxValue;
        foreach (MapObject mapObject in TestWorld.Objects(field))
        {
            int localID = MapObject.MapObject_GetLocalID(mapObject);
            if (mapObject == player || MapObject.MapObject_GetScript(mapObject) == NoScript || talked.Contains((map, localID)) || unreachable.Contains($"talkto {localID}@{map}"))
            {
                continue;
            }
            int distance = Math.Abs(MapObject.MapObject_GetX(mapObject) - x) + Math.Abs(MapObject.MapObject_GetZ(mapObject) - z);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = mapObject;
            }
        }
        if (nearest != null && nearestDistance <= ExploreObjectRange)
        {
            int localID = MapObject.MapObject_GetLocalID(nearest);
            talked.Add((map, localID));
            return $"talkto {localID}";
        }
        if (header != null && header.WarpEvents != null)
        {
            WarpEvent? best = null;
            int bestVisits = int.MaxValue;
            int bestDistance = int.MaxValue;
            foreach (WarpEvent warp in header.WarpEvents)
            {
                string action = $"warp {warp.DestHeaderID} {warp.X} {warp.Z}";
                if (warp.DestHeaderID == map || unreachable.Contains(action + "@" + map))
                {
                    continue;
                }
                warpVisits.TryGetValue((map, warp.X, warp.Z), out int visits);
                int distance = Math.Abs(warp.X - x) + Math.Abs(warp.Z - z);
                if (visits < bestVisits || (visits == bestVisits && distance < bestDistance))
                {
                    best = warp;
                    bestVisits = visits;
                    bestDistance = distance;
                }
            }
            if (best != null)
            {
                warpVisits[(map, best.X, best.Z)] = bestVisits + 1;
                return $"warp {best.DestHeaderID} {best.X} {best.Z}";
            }
        }
        return null;
    }

    private void ResetStep()
    {
        stepFrame = 0;
        idleFrames = 0;
        stuckFrames = 0;
        detourFrames = 0;
        detours = 0;
        pushFrames = 0;
        navigateFrames = 0;
        scriptStarts.Clear();
        busyStarts = 0;
        sawBusy = false;
        arrived = false;
        talkStarted = false;
        path = null;
        TestWorld.BlockedEdges.Clear();
        progressFrames = 0;
        lastX = -1;
        lastZ = -1;
    }

    private void Next()
    {
        if (generatedStep)
        {
            Log(string.Join(' ', steps[index]));
            steps.RemoveAt(index);
            generatedStep = false;
            exploreActions++;
            ResetStep();
            return;
        }
        index++;
        stepFrame = 0;
        idleFrames = 0;
        stuckFrames = 0;
        detourFrames = 0;
        detours = 0;
        pushFrames = 0;
        navigateFrames = 0;
        arrived = false;
        talkStarted = false;
        path = null;
        TestWorld.BlockedEdges.Clear();
        progressFrames = 0;
        lastX = -1;
        lastZ = -1;
    }

    public int Update()
    {
        if (Finished)
        {
            Status = Failed ? Status : "test script finished";
            return 0;
        }
        string[] step = steps[index];
        string label = $"test step {index + 1}/{steps.Count}: {string.Join(' ', step)}";
        int keys = 0;
        stepFrame++;
        bool inField = FieldState(out FieldSystem? field, out int map, out int x, out int z, out bool busy);
        if (index != loggedStep)
        {
            loggedStep = index;
            Log($"> step {index + 1}: {string.Join(' ', step)}" + (inField ? $"   [map {map} at ({x},{z})]" : ""));
        }
        LogDialogue();
        if (repeatedScript >= 0)
        {
            int script = repeatedScript;
            repeatedScript = -1;
            scriptStarts.Clear();
            Fail($"{label}\n  FAILED: script {script} fired {MaxSameScript + 1} times during this step (last said: {LastDialogue})");
            return 0;
        }
        if (inField && busy && !previousBusy)
        {
            busyStarts++;
        }
        previousBusy = inField && busy;
        if (generatedStep && busyStarts > MaxInterruptions && inField && !busy)
        {
            Fail($"{label}\n  FAILED: kept being interrupted by the same event on the way");
            return 0;
        }
        switch (step[0])
        {
            case "wait":
                Status = $"{label}\n  waiting {stepFrame}/{step[1]} frames";
                if (stepFrame >= int.Parse(step[1]))
                {
                    Next();
                }
                break;
            case "key":
                keys = Convert.ToInt32(step[1], 16);
                Status = $"{label}\n  holding keys {keys:X3} {stepFrame}/{step[2]}";
                if (stepFrame >= int.Parse(step[2]))
                {
                    Next();
                }
                break;
            case "tap":
                keys = stepFrame <= PulseHold ? Convert.ToInt32(step[1], 16) : 0;
                Status = $"{label}\n  pressing keys {Convert.ToInt32(step[1], 16):X3} once";
                if (stepFrame >= (step.Length > 2 ? int.Parse(step[2]) : PulsePeriod))
                {
                    Next();
                }
                break;
            case "touch":
            {
                int frames = step.Length > 3 ? int.Parse(step[3]) : TouchFrames;
                bool down = stepFrame <= frames;
                Input.TouchX = ushort.Parse(step[1]);
                Input.TouchY = ushort.Parse(step[2]);
                Input.Touching = down;
                Status = $"{label}\n  touching the lower screen at ({step[1]},{step[2]})";
                if (stepFrame >= frames + TouchFrames)
                {
                    Next();
                }
                break;
            }
            case "app":
                keys = step.Length > 2 ? (stepFrame % PulsePeriod < PulseHold ? Convert.ToInt32(step[2], 16) : 0) : 0;
                Status = $"{label}\n  waiting for screen '{step[1]}' (now '{ApplicationName()}')" + (keys != 0 || step.Length > 2 ? $", pressing {step[2]}" : "");
                if (ApplicationName() == step[1])
                {
                    Next();
                }
                else if (stepFrame > AppTimeout)
                {
                    Fail($"{label}\n  FAILED: screen '{step[1]}' never came up, still on '{ApplicationName()}'");
                }
                break;
            case "until":
            {
                object? data = sApplication.CurrentManager?.Data;
                object? value = data?.GetType().GetField(step[1])?.GetValue(data);
                keys = step.Length > 3 && stepFrame % PulsePeriod < PulseHold ? Convert.ToInt32(step[3], 16) : 0;
                Status = $"{label}\n  waiting for {ApplicationName()}.{step[1]} to become {step[2]} (now {value ?? "?"})" + (step.Length > 3 ? $", pressing {step[3]}" : "");
                if (value != null && Convert.ToInt64(value) == long.Parse(step[2]))
                {
                    Next();
                }
                else if (stepFrame > AppTimeout)
                {
                    Fail($"{label}\n  FAILED: {ApplicationName()}.{step[1]} never became {step[2]} (stuck at {value ?? "?"})");
                }
                break;
            }
            case "type":
            case "name":
            {
                if (stepFrame == 1)
                {
                    BuildNameKeys(step[1], step[0] == "name");
                }
                int tap = (stepFrame - 1) / NameTapPeriod;
                Status = $"{label}\n  typing '{step[1]}' on the touch keyboard ({Math.Min(tap, nameKeys.Count)}/{nameKeys.Count} touches)";
                if (tap >= nameKeys.Count)
                {
                    Next();
                    break;
                }
                bool pressed = (stepFrame - 1) % NameTapPeriod < PulseHold;
                if (nameKeys[tap].X != 0)
                {
                    Input.TouchX = (ushort)nameKeys[tap].X;
                    Input.TouchY = (ushort)nameKeys[tap].Y;
                    Input.Touching = pressed;
                }
                else
                {
                    Input.Touching = false;
                }
                break;
            }
            case "shot":
                Status = $"{label}\n  taking a screenshot";
                if (stepFrame >= 2)
                {
                    Screenshot?.Invoke(step[1]);
                    Next();
                }
                break;
            case "field":
                Status = $"{label}\n  waiting for the field ({BusyReason()})";
                if (inField)
                {
                    Next();
                }
                else if (stepFrame > FieldTimeout)
                {
                    Fail($"{label}\n  FAILED: the field never started");
                }
                break;
            case "talk":
                keys = Pulse();
                idleFrames = inField && !busy ? idleFrames + 1 : 0;
                Status = $"{label}\n  pressing A; game is busy with: {BusyReason()}";
                if (idleFrames >= IdleFramesToFinishTalk)
                {
                    Next();
                }
                else if (NoProgress())
                {
                    Fail($"{label}\n  FAILED: no progress for {TalkTimeout} frames, busy with: {BusyReason()}");
                }
                break;
            case "map":
                keys = busy ? Pulse() : 0;
                Status = $"{label}\n  waiting for map {step[1]} (now {map}, {BusyReason()})";
                if (inField && !busy && map == int.Parse(step[1]))
                {
                    Next();
                    break;
                }
                idleFrames = inField && !busy ? idleFrames + 1 : 0;
                if (idleFrames > IdleTimeout)
                {
                    Fail($"{label}\n  FAILED: nothing is happening, still on map {map} at ({x},{z})");
                }
                break;
            case "save":
                Status = $"{label}\n  saving the game as state '{step[1]}'";
                if (!inField || busy)
                {
                    keys = Pulse();
                    if (NoProgress())
                    {
                        Fail($"{label}\n  FAILED: the game never became idle");
                    }
                    break;
                }
                if (stepFrame < SaveResultFrames)
                {
                    break;
                }
                if (!FieldSave.FieldSystem_Save(field!) || CardBackup.FilePath == null)
                {
                    Fail($"{label}\n  FAILED: the game's save routine reported an error");
                    break;
                }
                File.Copy(CardBackup.FilePath, Path.Combine(StateDirectory, step[1] + ".sav"), true);
                Next();
                break;
            case "goto":
            {
                int targetX = int.Parse(step[1]);
                int targetZ = int.Parse(step[2]);
                if (!inField || busy)
                {
                    sawBusy = true;
                    keys = BusyKeys(label, x, z);
                    break;
                }
                if ((x == targetX && z == targetZ) || (generatedStep && sawBusy))
                {
                    Next();
                    break;
                }
                keys = Navigate(field!, label, map, x, z, (tx, tz) => tx == targetX && tz == targetZ, $"({targetX},{targetZ})");
                break;
            }
            case "warp":
            {
                int destination = int.Parse(step[1]);
                if (!inField || busy)
                {
                    keys = BusyKeys(label, x, z);
                    break;
                }
                if (map == destination)
                {
                    Next();
                    break;
                }
                WarpEvent? warp = null;
                if (field!.MapHeaderData is MapHeaderData header && header.WarpEvents != null)
                {
                    foreach (WarpEvent candidate in header.WarpEvents)
                    {
                        if (step.Length > 3 && (candidate.X != int.Parse(step[2]) || candidate.Z != int.Parse(step[3])))
                        {
                            continue;
                        }
                        if (candidate.DestHeaderID == destination && (warp == null || Math.Abs(candidate.X - x) + Math.Abs(candidate.Z - z) < Math.Abs(warp.X - x) + Math.Abs(warp.Z - z)))
                        {
                            warp = candidate;
                        }
                    }
                }
                if (warp == null)
                {
                    Fail($"{label}\n  FAILED: map {map} has no warp to map {destination}");
                    break;
                }
                int warpX = warp.X;
                int warpZ = warp.Z;
                int push = TestWorld.WarpPushKey(field, warpX, warpZ);
                bool isDoor = push == TestWorld.KeyUp && !TestWorld.Walkable(field, warpX, warpZ);
                int standX = isDoor ? warpX : warpX;
                int standZ = isDoor ? warpZ + 1 : warpZ;
                if (push == 0)
                {
                    push = TestWorld.KeyToward(x, z, warpX, warpZ);
                    if (x == warpX && z == warpZ)
                    {
                        int[] pushes = { TestWorld.KeyDown, TestWorld.KeyUp, TestWorld.KeyRight, TestWorld.KeyLeft };
                        push = pushes.FirstOrDefault(key => !TestWorld.Walkable(field, warpX + TestWorld.Offset(key).X, warpZ + TestWorld.Offset(key).Z), TestWorld.KeyDown);
                    }
                    else if (Math.Abs(x - warpX) + Math.Abs(z - warpZ) == 1)
                    {
                        standX = x;
                        standZ = z;
                    }
                }
                if (x == standX && z == standZ)
                {
                    keys = push;
                    pushFrames++;
                    Status = $"{label}\n  at the warp ({warpX},{warpZ}), pushing {KeyName(push)} to go through";
                    if (pushFrames > PushFrames * 6)
                    {
                        Fail($"{label}\n  FAILED: pushing {KeyName(push)} at the warp ({warpX},{warpZ}) did nothing");
                    }
                    break;
                }
                pushFrames = 0;
                int goalX = standX;
                int goalZ = standZ;
                bool exact = TestWorld.FindPath(field, x, z, (tx, tz) => tx == goalX && tz == goalZ) != null;
                keys = Navigate(field, label, map, x, z, (tx, tz) => exact ? tx == goalX && tz == goalZ : Math.Abs(tx - goalX) + Math.Abs(tz - goalZ) == 1, $"the warp to map {destination} at ({warpX},{warpZ})");
                if (!exact && Math.Abs(x - goalX) + Math.Abs(z - goalZ) == 1 && !Failed)
                {
                    keys = TestWorld.KeyToward(x, z, goalX, goalZ);
                }
                break;
            }
            case "explore":
            {
                int limit = step.Length > 1 ? int.Parse(step[1]) : DefaultExploreActions;
                if (!inField || busy)
                {
                    keys = BusyKeys(label, x, z);
                    break;
                }
                if (exploreActions >= limit)
                {
                    exploreActions = 0;
                    Next();
                    break;
                }
                string? action = ChooseExploreAction(field!, map, x, z);
                if (action == null)
                {
                    Fail($"{label}\n  FAILED: nothing left to try on map {map} (no reachable trigger, object or warp)");
                    break;
                }
                Status = $"{label}\n  explorer chose: {action}";
                steps.Insert(index, action.Split(' '));
                generatedStep = true;
                ResetStep();
                break;
            }
            case "talkto":
            {
                int localID = int.Parse(step[1]);
                if (!inField)
                {
                    keys = BusyKeys(label, x, z);
                    break;
                }
                if (busy)
                {
                    talkStarted |= arrived;
                    keys = BusyKeys(label, x, z);
                    idleFrames = 0;
                    break;
                }
                if (talkStarted)
                {
                    idleFrames++;
                    Status = $"{label}\n  conversation finished";
                    if (idleFrames >= IdleFramesToFinishTalk)
                    {
                        Next();
                    }
                    break;
                }
                MapObject? target = TestWorld.ObjectByLocalID(field!, localID);
                if (target == null)
                {
                    Fail($"{label}\n  FAILED: there is no object {localID} on map {map}");
                    break;
                }
                int objectX = MapObject.MapObject_GetX(target);
                int objectZ = MapObject.MapObject_GetZ(target);
                if (Math.Abs(x - objectX) + Math.Abs(z - objectZ) == 1)
                {
                    arrived = true;
                    int facing = TestWorld.KeyToward(x, z, objectX, objectZ);
                    keys = stepFrame % PulsePeriod < PulseHold ? (pushFrames % 2 == 0 ? facing : KeyA) : 0;
                    if (stepFrame % PulsePeriod == 0)
                    {
                        pushFrames++;
                    }
                    Status = $"{label}\n  next to object {localID} at ({objectX},{objectZ}), facing it and pressing A";
                    if (pushFrames > 40)
                    {
                        Fail($"{label}\n  FAILED: object {localID} did not react to A");
                    }
                    break;
                }
                arrived = false;
                keys = Navigate(field!, label, map, x, z, (tx, tz) => Math.Abs(tx - objectX) + Math.Abs(tz - objectZ) == 1, $"object {localID} at ({objectX},{objectZ})");
                break;
            }
            default:
                Fail($"unknown test step: {string.Join(' ', step)}");
                break;
        }
        return keys;
    }
}
