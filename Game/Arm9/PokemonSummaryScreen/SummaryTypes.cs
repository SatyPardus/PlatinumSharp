namespace PlatinumSharp.Game;

public sealed class SpiderGraphRectangle
{
    public VecFx16 TopLeft;
    public VecFx16 TopRight;
    public VecFx16 BottomLeft;
    public VecFx16 BottomRight;
}

public sealed class SpiderGraph
{
    public const int MaxRects = 4;

    public readonly SpiderGraphRectangle[] CurrRects = NewRects();
    public readonly SpiderGraphRectangle[] DeltaRects = NewRects();
    public readonly SpiderGraphRectangle[] MaxRectsData = NewRects();
    public uint State;

    private static SpiderGraphRectangle[] NewRects()
    {
        SpiderGraphRectangle[] rects = new SpiderGraphRectangle[MaxRects];
        for (int i = 0; i < rects.Length; i++)
        {
            rects[i] = new SpiderGraphRectangle();
        }
        return rects;
    }
}

public sealed class PokemonSummaryMonData
{
    public const int LearnedMovesMax = 4;

    public GameString? SpeciesName;
    public GameString? Nickname;
    public GameString? OTName;
    public ushort Species;
    public ushort HeldItem;
    public byte Type1;
    public byte Type2;
    public byte Level;
    public byte HideGender;
    public byte Gender;
    public byte CaughtBall;
    public uint OTID;
    public uint CurExp;
    public uint CurLevelExp;
    public uint NextLevelExp;
    public ushort CurHP;
    public ushort MaxHP;
    public ushort Attack;
    public ushort Defense;
    public ushort SpAttack;
    public ushort SpDefense;
    public ushort Speed;
    public byte Ability;
    public byte Nature;
    public readonly ushort[] Moves = new ushort[LearnedMovesMax];
    public readonly byte[] CurPP = new byte[LearnedMovesMax];
    public readonly byte[] MaxPP = new byte[LearnedMovesMax];
    public byte OTGender;
    public byte Cool;
    public byte Beauty;
    public byte Cute;
    public byte Smart;
    public byte Tough;
    public byte Sheen;
    public byte PreferredFlavor;
    public ushort Markings;
    public ushort Form;
    public uint Status;
    public byte IsEgg;
    public byte IsShiny;
    public byte Pokerus;
    public readonly uint[] Ribbons = new uint[4];
}

public sealed class PokemonSummaryMonSpriteData
{
    public const int MaxAnimationFrames = 10;

    public Camera? Camera;
    public PokemonSpriteManager? SpriteManager;
    public readonly SpriteAnimFrame[] Frames = new SpriteAnimFrame[MaxAnimationFrames];
    public PokemonAnimManager? MonAnimMan;
    public PokemonSprite? Sprite;
    public bool Flip;
}

public sealed class PokemonSummaryScreen
{
    public const int Size = 0x6B4;
    public const int StaticWindowMax = 36;
    public const int SpriteMax = 77;

    public BgConfig? BgConfig;
    public readonly Window[] StaticWindows = NewWindows(StaticWindowMax);
    public Window[]? ExtraWindows;
    public uint NumExtraWindows;
    public PokemonSummary? Data;
    public readonly PokemonSummaryMonData MonData = new();
    public readonly PokemonSummaryMonSpriteData MonSprite = new();
    public readonly SpiderGraph Graph = new();
    public SpriteSystem? SpriteSys;
    public SpriteManager? SpriteMan;
    public readonly Sprite?[] Sprites = new Sprite?[SpriteMax];
    public readonly ManagedSprite?[] ManagedSprites = new ManagedSprite?[SpriteMax];
    public FontSpecialCharsContext? SpecialChars;
    public MessageLoader? MsgLoader;
    public MessageLoader? RibbonLoader;
    public StringTemplate? StrFormatter;
    public GameString? String;
    public GameString? PlayerName;
    public MessageLoader? MoveNameLoader;
    public NARC? NarcPlPokeData;
    public sbyte Page;
    public byte Cursor;
    public byte CursorTmp;
    public byte PageState;
    public byte SubscreenType;
    public byte SubscreenExit;
    public byte SheenState;
    public byte SheenMax;
    public byte SheenPos;
    public byte SheenCount;
    public byte ButtonAnimFrame;
    public byte ButtonState;
    public byte ButtonCurrent;
    public byte RibbonCol;
    public byte RibbonRow;
    public byte RibbonMax;
    public byte RibbonID;

    private static Window[] NewWindows(int count)
    {
        Window[] windows = new Window[count];
        for (int i = 0; i < count; i++)
        {
            windows[i] = new Window();
        }
        return windows;
    }
}

public static partial class PokemonSummaryScreenApp
{
    public const uint PokemonSummaryScreenTemplate = 0x020F410C;
    private const int HeapIDSummary = 0x13;
}
