namespace PlatinumSharp.Game;

public static class FieldItemHelpers
{
    private const int FirstTMHM = 0x148;
    private const int LastTMHM = 0x1AB;

    // OFFSET 0x0205E060
    public static ushort Item_IsTMHM(ushort item)
    {
        if (item >= FirstTMHM && item <= LastTMHM)
        {
            return 1;
        }
        return 0;
    }
}
