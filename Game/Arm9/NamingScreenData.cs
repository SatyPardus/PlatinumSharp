namespace PlatinumSharp.Game;

public sealed class NamingScreenCursor
{
    public int X;
    public int Y;
    public int PrevX;
    public int PrevY;
    public int DeltaX;
    public int HasCharacterBeenEntered;
    public int IgnoreInput;
}

public sealed class NamingScreenSubspritePosParams
{
    public Sprite? Parent;
    public Sprite? Child;
    public int Dx;
    public int Index;
}

public sealed class NamingScreenOverlayWiggleParams
{
    public Sprite? OverlaySprite;
    public int State;
    public int OverlayXPosition;
    public int OverlayYPosition;
}

public sealed class NamingScreenData
{
    public const int Rows = 6;
    public const int Columns = 13;
    public const int WindowCount = 10;
    public const int FollowTaskCount = 7;

    public int Type;
    public int PlayerGenderOrMonSpecies;
    public int MonForm;
    public int MaxChars;
    public int MonGender;
    public int PrintedFromBattleGMM;
    public Options? Options;
    public readonly NamingScreenCursor KeyboardCursor = new();
    public ushort PlttGlowEffectAngle;
    public readonly ushort[] KeyboardChars = new ushort[Rows * Columns];
    public readonly ushort[] EntryBuf = new ushort[32];
    public readonly ushort[] EntryBufBak = new ushort[32];
    public ushort TextCursorPos;
    public readonly ushort[] TmpBuf = new ushort[3];
    public BgConfig? BgConfig;
    public StringTemplate? StrTemplate;
    public MessageLoader? NamingScreenMsgLoader;
    public MessageLoader? GenericNamesMsgLoader;
    public MessageLoader? BattleStringsMsgLoader;
    public GameString? PromptString;
    public GameString? LastCharString;
    public GameString? BattleMessageString;
    public GameString? GroupTextString;
    public SpriteList? SpriteList;
    public readonly G2dRenderer G2dRenderer = new();
    public readonly SpriteResourceCollection?[] SpriteResourceCollections = new SpriteResourceCollection?[4];
    public readonly SpriteResource?[] SpriteResources = new SpriteResource?[8];
    public readonly SpriteResourcesHeader MainSpriteResourceHeader = new();
    public readonly SpriteResourcesHeader SubSpriteResourceHeader = new();
    public readonly Sprite?[] UiSprites = new Sprite?[14];
    public readonly Sprite?[] TextCursorSprites = new Sprite?[12];
    public readonly Sprite?[] EntitySprites = new Sprite?[2];
    public readonly SysTask?[] SpriteFollowTasks = new SysTask?[FollowTaskCount];
    public readonly Window[] Windows = NewWindows();
    public int BattleMessagePrinter;
    public int State;
    public int CurrentCharsIdx;
    public int FreeCharsBgLayer;
    public readonly VecFx32[] CharsPosition = new VecFx32[2];
    public int Unused484;
    public int Unused488;
    public int Unused48C;
    public readonly int[] SpritesToUpdate = new int[FollowTaskCount];
    public byte[]? CharDataAlloc;
    public CharacterData? CharData;
    public byte[]? MonIconRawCharData;
    public CharacterData? MonIconCharData;
    public byte[]? MonIconRawPlttData;
    public PaletteFile? PlttData;
    public readonly byte[] PixelBuf = new byte[256];
    public PaletteAnimator? TouchscreenIconBlinker;
    public int IsTouchInput;
    public int DelayUpdateCounter;

    private static Window[] NewWindows()
    {
        Window[] windows = new Window[WindowCount];
        for (int i = 0; i < WindowCount; i++)
        {
            windows[i] = new Window();
        }
        return windows;
    }
}
