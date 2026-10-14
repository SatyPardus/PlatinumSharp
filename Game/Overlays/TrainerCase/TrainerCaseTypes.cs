namespace PlatinumSharp.Game;

public sealed class TrainerCaseSpriteData
{
    public const int NumSprites = TrainerCase.MaxBadges * 3 + 1;

    public SpriteList? SpriteList;
    public readonly G2dRenderer G2dRenderer = new();
    public readonly SpriteResourceCollection?[] SpriteResourceCollections = new SpriteResourceCollection?[4];
    public readonly SpriteResource?[] SpriteResources = new SpriteResource?[4];
    public readonly Sprite?[] Sprites = new Sprite?[NumSprites];
    public readonly byte[]?[] BadgePaletteBuffers = new byte[]?[TrainerCase.MaxBadges];
    public readonly PaletteFile?[] BadgePalettes = new PaletteFile?[TrainerCase.MaxBadges];
}

public sealed class BadgePolishingState
{
    public sbyte PreviousXDirection;
    public sbyte PreviousYDirection;
    public sbyte CurrentXDirection;
    public sbyte CurrentYDirection;
    public byte SoundEffectIndex;
}

public sealed class TrainerCaseApp
{
    public const int Size = 0x3320;
    public const int NumWindows = 11;
    public const int SignatureBytes = 0x3000;
    public const int NumChimePlayers = 4;

    public BgConfig? BgConfig;
    public readonly Window[] Windows = NewWindows();
    public TrainerCase? TrainerCase;
    public readonly byte[] Signature = new byte[SignatureBytes];
    public byte[]? TrainerSprite;
    public CharacterData? TrainerCharacterData;
    public bool ViewingBack;
    public bool PolishingEnabled;
    public int CardXScale;
    public int CardYScale;
    public int BadgeCaseCoverYScale;
    public int TouchedRectangleIndex;
    public bool BadgeCaseCoverMoving;
    public int LastTouchedX;
    public int LastTouchedY;
    public readonly TrainerCaseSpriteData SpriteData = new();
    public int CardScalingSpeed;
    public bool BadgeCaseButtonPushed;
    public readonly byte[] BadgeChimeState = new byte[NumChimePlayers * 2];
    public byte[]? TrainerScreenDataNSCRBuffer;
    public ScreenData? TrainerScreenData;
    public readonly BadgePolishingState BadgePolishingState = new();
    public GameString? UnusedString;
    public GameString? ColonString;
    public readonly byte[] PolishingProgress = new byte[TrainerCase.MaxBadges];
    public byte BadgeCaseOpenState;
    public byte BadgeCaseTransitionIndex;
    public byte SubState;
    public byte BadgeCaseButtonAnimTimer;
    public byte BadgeCaseButtonAnimIndex;
    public byte BadgeCaseButtonState;
    public byte Timer;

    private static Window[] NewWindows()
    {
        Window[] windows = new Window[NumWindows];
        for (int i = 0; i < windows.Length; i++)
        {
            windows[i] = new Window();
        }
        return windows;
    }
}
