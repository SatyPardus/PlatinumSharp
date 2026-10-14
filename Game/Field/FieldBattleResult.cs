namespace PlatinumSharp.Game;

public static class FieldBattleResult
{
    public const int Win = 1;
    public const int Lose = 2;
    public const int Draw = 3;
    public const int Captured = 4;

    // OFFSET 0x02052868
    public static int CheckPlayerWonBattle(int battleResult)
    {
        if (battleResult == Lose || battleResult == Draw)
        {
            return 0;
        }
        return 1;
    }

    // OFFSET 0x02052878
    public static int CheckPlayerLostBattle(int battleResult)
    {
        if (battleResult == Win || battleResult == Captured)
        {
            return 0;
        }
        return 1;
    }

    // OFFSET 0x02052888
    public static int CheckPlayerDidNotCapture(int battleResult)
    {
        if (battleResult == Captured)
        {
            return 0;
        }
        return 1;
    }
}
