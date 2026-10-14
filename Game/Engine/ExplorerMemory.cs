using System.Text.Json;

namespace PlatinumSharp.Game;

public sealed class ScriptObservation
{
    public Dictionary<string, int> Reads { get; set; } = new();
    public int Count { get; set; }
}

public sealed class ExplorerMemory
{
    public static readonly string DefaultPath = Environment.GetEnvironmentVariable("EXPLORE_MEMORY") ?? Path.Combine(ProjectPaths.Tests, "explore_memory.json");
    private const int MaxObservationsPerScript = 6;

    public HashSet<int> Maps { get; set; } = new();
    public Dictionary<string, List<ScriptObservation>> Scripts { get; set; } = new();
    public Dictionary<string, int> Exits { get; set; } = new();
    public Dictionary<string, SortedSet<int>> Coverage { get; set; } = new();
    public HashSet<string> Fails { get; set; } = new();
    public Dictionary<int, int> HealMaps { get; set; } = new();
    public Dictionary<string, int> Links { get; set; } = new();

    public static ExplorerMemory Load(string? path = null)
    {
        path ??= DefaultPath;
        try
        {
            if (File.Exists(path))
            {
                return JsonSerializer.Deserialize<ExplorerMemory>(File.ReadAllText(path)) ?? new ExplorerMemory();
            }
        }
        catch (Exception)
        {
        }
        return new ExplorerMemory();
    }

    public void Save(string? path = null)
    {
        path ??= DefaultPath;
        File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = false }));
    }

    public bool Seen(string key)
    {
        return Scripts.ContainsKey(key);
    }

    public bool Exhausted(string key, Func<string, int?> current)
    {
        if (!Scripts.TryGetValue(key, out List<ScriptObservation>? observations))
        {
            return false;
        }
        int total = 0;
        foreach (ScriptObservation observation in observations)
        {
            total += observation.Count;
            bool same = true;
            foreach (var read in observation.Reads)
            {
                int? value = current(read.Key);
                if (value != null && value != read.Value)
                {
                    same = false;
                    break;
                }
            }
            if (same)
            {
                return true;
            }
        }
        return total >= MaxObservationsPerScript;
    }

    public int Observe(string key, int file, ScriptTraceRecord record, Func<string, bool> keep)
    {
        Dictionary<string, int> reads = record.Reads.Where(read => keep(read.Key)).ToDictionary(read => read.Key, read => read.Value);
        if (!Scripts.TryGetValue(key, out List<ScriptObservation>? observations))
        {
            observations = new List<ScriptObservation>();
            Scripts[key] = observations;
        }
        ScriptObservation? match = observations.FirstOrDefault(o => o.Reads.Count == reads.Count && o.Reads.All(r => reads.TryGetValue(r.Key, out int v) && v == r.Value));
        if (match == null)
        {
            match = new ScriptObservation { Reads = reads };
            observations.Add(match);
        }
        match.Count++;
        string fileKey = file.ToString();
        if (!Coverage.TryGetValue(fileKey, out SortedSet<int>? offsets))
        {
            offsets = new SortedSet<int>();
            Coverage[fileKey] = offsets;
        }
        int before = offsets.Count;
        offsets.UnionWith(record.Offsets);
        return offsets.Count - before;
    }

    public int TotalCoverage()
    {
        return Coverage.Values.Sum(offsets => offsets.Count);
    }
}
