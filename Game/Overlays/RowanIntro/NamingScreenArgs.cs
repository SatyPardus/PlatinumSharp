namespace PlatinumSharp.Game;

public sealed class NamingScreenArgs
{
    public int Type;
    public int PlayerGenderOrMonSpecies;
    public int MonForm;
    public int MaxChars;
    public int MonGender;
    public int ReturnCode;
    public GameString? TextInputStr;
    public readonly ushort[] NameInputRaw = new ushort[20];
    public int BattleMsgID;
    public object? PcBoxes;
    public Options? Options;

    private const uint TextInputSize = 0x20;
    private const ushort CharEndOfString = 0xFFFF;

    // OFFSET 0x0208712C
    public static NamingScreenArgs NamingScreenArgs_Init(int heapID, int type, int playerGenderOrMonSpecies, int maxChars, Options? options)
    {
        NamingScreenArgs args = new();
        args.Type = type;
        args.PlayerGenderOrMonSpecies = playerGenderOrMonSpecies;
        args.MaxChars = maxChars;
        args.ReturnCode = 0;
        args.NameInputRaw[0] = CharEndOfString;
        args.TextInputStr = GameString.String_Init(TextInputSize, heapID);
        args.BattleMsgID = 0;
        args.PcBoxes = null;
        args.MonGender = 0;
        args.Options = options;
        args.MonForm = 0;
        return args;
    }

    // OFFSET 0x0208716C
    public static void NamingScreenArgs_Free(NamingScreenArgs? args)
    {
        if (args?.TextInputStr == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (args == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        GameString.String_Free(args!.TextInputStr);
    }
}
