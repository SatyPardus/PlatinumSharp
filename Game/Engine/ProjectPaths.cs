namespace PlatinumSharp.Game;

public static class ProjectPaths
{
    private const string WorkspaceVariable = "PLATINUMSHARP_WORKSPACE";
    private const string RomVariable = "PLATINUMSHARP_ROM";
    private const string SolutionFile = "PlatinumSharp.sln";
    private const string RepositoryFolder = "PlatinumSharp";

    public static readonly string Workspace = FindWorkspace();

    public static string Tools => Path.Combine(Workspace, "tools");
    public static string Tests => Path.Combine(Tools, "tests");
    public static string States => Path.Combine(Tests, "states");
    public static string Shots => Path.Combine(Tests, "shots");
    public static string ScriptData => Path.Combine(Tools, "scriptdata.json");
    public static string Dumps => Path.Combine(Workspace, "dumps");
    public static string DashboardLive => Path.Combine(Workspace, "dashboard", "live");

    public static string? DefaultRom
    {
        get
        {
            string? configured = Environment.GetEnvironmentVariable(RomVariable);
            if (!string.IsNullOrEmpty(configured))
            {
                return configured;
            }
            try
            {
                return Directory.EnumerateFiles(Workspace, "*.nds").OrderBy(file => file).FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    private static string FindWorkspace()
    {
        string? configured = Environment.GetEnvironmentVariable(WorkspaceVariable);
        if (!string.IsNullOrEmpty(configured))
        {
            return Path.GetFullPath(configured);
        }
        for (DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, SolutionFile)))
            {
                return directory.Parent?.FullName ?? directory.FullName;
            }
            if (File.Exists(Path.Combine(directory.FullName, RepositoryFolder, SolutionFile)))
            {
                return directory.FullName;
            }
        }
        return Directory.GetCurrentDirectory();
    }
}
