namespace PlatinumSharp.Game;

public static class Boot
{
    private const uint ExpectedGameCode = 0x4A414441;
    private const ushort ExpectedMakerCode = 0x3130;

    // OFFSET 0x02024358
    public static void CheckForMemoryTampering()
    {
        if (!Platform.CardEnabled)
        {
            Platform.Terminate();
            return;
        }
        if (Platform.SharedGameCode == 0)
        {
            Platform.SharedGameCode = ExpectedGameCode;
        }
        if (Platform.SharedGameCode != ExpectedGameCode || Platform.SharedMakerCode != ExpectedMakerCode)
        {
            Platform.Terminate();
        }
    }
}
