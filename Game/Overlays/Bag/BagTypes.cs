namespace PlatinumSharp.Game;

public delegate int BagItemUseCallback(BagController controller);

public sealed class BagPocketSelector
{
    public byte NextPocket;
    public byte NextPocketDirection;
    public byte AnimStage;
    public byte AnimFrame;
    public byte PocketChangeState;
    public byte PocketChangeFrame;
    public byte PressedPocketBtnIdx;
    public byte ButtonState;
    public byte TouchState;
    public bool RedrawSelectorIcons;
}

public sealed class BagPocketHighlight
{
    public byte IsMoving;
    public byte Direction;
    public byte CurrentStep;
    public readonly int[] Positions = new int[8];
}

public sealed class BagController
{
    public const int NumWindows = 11;
    public const int NumSprites = 12;
    public const int NumItemActions = 12;
    public const int LargestPocketSize = 165;

    public BgConfig? BgConfig;
    public readonly Window[] Windows = NewWindows(NumWindows);
    public readonly Window ItemActionsWindow = new();
    public BagContext? BagCtx;
    public Bag? Bag;
    public TrainerInfo? TrainerInfo;
    public Options? Options;
    public NARC? BagGraphicsNARC;
    public SpriteSystem? SpriteSystem;
    public SpriteManager? SpriteMan;
    public readonly ManagedSprite?[] Sprites = new ManagedSprite?[NumSprites];
    public FontSpecialCharsContext? SpecialChars;
    public MessageLoader? BagStringsLoader;
    public StringTemplate? StrTemplate;
    public MessageLoader? ItemNamesLoader;
    public MessageLoader? MoveNamesLoader;
    public readonly GameString?[] ItemActionStrings = new GameString?[NumItemActions];
    public StringListEntry[]? ItemActionChoices;
    public Menu? Menu;
    public ListMenu? ItemList;
    public StringListEntry[]? ItemListEntries;
    public readonly GameString?[] ItemNames = new GameString?[LargestPocketSize];
    public GameString? StringBuffer;
    public GameString? ItemCountX;
    public GameString? ItemCountNumberFmt;
    public readonly GameString?[] PocketNames = new GameString?[BagContext.PocketMax];
    public byte NumPockets;
    public byte TrainerGender;
    public byte MsgBoxPrinterID;
    public byte PocketSelectorIconsX;
    public byte PocketSelectorIconsSpacing;
    public readonly BagPocketSelector PocketSelector = new();
    public readonly BagPocketHighlight PocketHighlight = new();
    public byte NextPocket;
    public byte HideDescription;
    public byte IsMovingItem;
    public byte MovedItemPos;
    public uint MovedItemID;
    public byte PocketSelectorArrowsAnimStep;
    public byte PocketSelectorArrowsAnimTimer;
    public byte CursorSoundIdx;
    public byte ItemUseTaskState;
    public BagItemUseCallback? ItemUseCallback;
    public short SelectedItemCount;
    public ushort SelectedItemCountLimit;
    public uint SoldItemPrice;
    public byte DialButtonAnimStep;
    public byte DialButtonAnimFrame;
    public byte IsTouchingDial;
    public int QueuedScrollRemainder;
    public short QueuedScroll;
    public short DialRotation;
    public ushort PreviousTouchX;
    public ushort PreviousTouchY;
    public object? ItemUseTaskData;

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
