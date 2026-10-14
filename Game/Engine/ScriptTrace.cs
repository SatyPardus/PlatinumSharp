namespace PlatinumSharp.Game;

public sealed class ScriptTraceRecord
{
    public int File = -1;
    public readonly SortedDictionary<string, int> Reads = new();
    public readonly SortedSet<int> Offsets = new();
}

public static class ScriptTrace
{
    private const ushort FirstSavedVar = 0x4000;
    private const ushort FirstSpecialVar = 0x8000;

    public static bool Enabled;

    private static readonly Dictionary<int, ScriptTraceRecord> sRecords = new();
    private static ScriptTraceRecord? sActive;

    public static void Clear()
    {
        sRecords.Clear();
        sActive = null;
    }

    public static ScriptTraceRecord? Take(int file)
    {
        sRecords.TryGetValue(file, out ScriptTraceRecord? record);
        Clear();
        return record;
    }

    public static void OnCommand(ScriptContext ctx)
    {
        if (!Enabled)
        {
            return;
        }
        if (!sRecords.TryGetValue(ctx.ScriptFile, out ScriptTraceRecord? record))
        {
            record = new ScriptTraceRecord { File = ctx.ScriptFile };
            sRecords[ctx.ScriptFile] = record;
        }
        record.Offsets.Add(ctx.ScriptPtr);
        sActive = record;
    }

    public static void OnVarRead(ushort varID, ushort value)
    {
        if (Enabled && sActive != null && varID >= FirstSavedVar && varID < FirstSpecialVar)
        {
            sActive.Reads.TryAdd($"v{varID:X4}", value);
        }
    }

    public static void OnFlagRead(ushort flagID, bool value)
    {
        if (Enabled)
        {
            sActive?.Reads.TryAdd($"f{flagID:X4}", value ? 1 : 0);
        }
    }
}
