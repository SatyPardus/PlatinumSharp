namespace PlatinumSharp.Viewer;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        string? romPath = args.Length > 0 ? args[0] : null;
        if (romPath == null)
        {
            using OpenFileDialog dialog = new() { Filter = "Nintendo DS ROM (*.nds)|*.nds" };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            romPath = dialog.FileName;
        }
        Application.Run(new MainForm(romPath));
    }
}
