namespace PlatinumSharp.Game;

public static class PokemonSummaryShared
{
    // OFFSET 0x0208C324
    public static int PokemonSummaryScreen_ShowContestData(SaveData saveData)
    {
        return SystemFlags.SystemFlag_CheckContestHallVisited(VarsFlags.SaveData_GetVarsFlags(saveData)) ? 1 : 0;
    }
}
