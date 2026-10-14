namespace PlatinumSharp.Game;

public sealed class BattleBagContext
{
    public const int NumPockets = 5;

    public BattleSystem? BattleSys;
    public TrainerInfo? TrainerInfo;
    public Bag? Bag;
    public int HeapID;
    public int Battler;
    public int IsInCatchTutorial;
    public uint EmbargoRemainingTurns;
    public ushort SelectedBattleBagItem;
    public byte SelectedBattleBagPocket;
    public byte LastUsedItemPocket;
    public ushort LastUsedItem;
    public byte HasTwoOpponents;
    public byte OpponentHidden;
    public byte OpponentSubstituted;
    public byte IsCursorEnabled;
    public byte BattleBagExited;
    public readonly byte[] PocketCurrentPagePositions = new byte[NumPockets];
    public readonly byte[] PocketCurrentPages = new byte[NumPockets];
}

public sealed class BattleBagItem
{
    public ushort Item;
    public ushort Quantity;
}

public sealed class BattleBag
{
    public const int Size = 0x115C;
    public const int NumSprites = 6;
    public const int PocketSize = 36;
    public const int ButtonStateMax = 4;
    public const int ButtonStateMaxCannotDisable = 3;
    public const int MenuPocketButtonSizeTiles = 16 * 9;
    public const int ConfirmButtonSizeTiles = 26 * 5;
    public const int CancelButtonSizeTiles = 5 * 5;
    public const int PocketItemButtonSizeTiles = 16 * 6;
    public const int PrevNextButtonSizeTiles = 5 * 5;
    public const int MenuPocketIconSizeTiles = 4 * 4;

    public BattleBagContext? Context;
    public BgConfig? Background;
    public PaletteData? Palette;
    public FontSpecialCharsContext? Unused1;
    public MessageLoader? MessageLoader;
    public StringTemplate? StringTemplate;
    public GameString? String;
    public readonly Window MessageBoxWindow = new();
    public Window[]? Windows;
    public byte NumWindows;
    public byte UseAltPocketMenuWindows;
    public byte TextPrinterID;
    public BattleSubMenuCursor? Cursor;
    public TouchIndicator? CatchTutorialCursor;
    public readonly BattleBagItem[][] Items = Enumerable.Range(0, BattleBagContext.NumPockets).Select(_ => Enumerable.Range(0, PocketSize).Select(_ => new BattleBagItem()).ToArray()).ToArray();
    public SpriteManager? SpriteManager;
    public readonly ManagedSprite?[] Sprites = new ManagedSprite?[NumSprites];
    public readonly ushort[][] MenuPocketButtonData = Buttons(ButtonStateMaxCannotDisable, MenuPocketButtonSizeTiles);
    public readonly ushort[][] UseItemButtonData = Buttons(ButtonStateMax, ConfirmButtonSizeTiles);
    public readonly ushort[][] CancelButtonData = Buttons(ButtonStateMaxCannotDisable, CancelButtonSizeTiles);
    public readonly ushort[][] PocketItemButtonData = Buttons(ButtonStateMax, PocketItemButtonSizeTiles);
    public readonly ushort[][] PocketPrevPageButtonData = Buttons(ButtonStateMax, PrevNextButtonSizeTiles);
    public readonly ushort[][] PocketNextPageButtonData = Buttons(ButtonStateMax, PrevNextButtonSizeTiles);
    public readonly ushort[][] MenuScreenRecoverHPPocketIconData = Buttons(ButtonStateMaxCannotDisable, MenuPocketIconSizeTiles);
    public readonly ushort[][] MenuScreenRecoverStatusPocketIconData = Buttons(ButtonStateMaxCannotDisable, MenuPocketIconSizeTiles);
    public readonly ushort[][] MenuScreenPokeBallsPocketIconData = Buttons(ButtonStateMaxCannotDisable, MenuPocketIconSizeTiles);
    public readonly ushort[][] MenuScreenBattleItemsIconData = Buttons(ButtonStateMaxCannotDisable, MenuPocketIconSizeTiles);
    public byte PressedButtonState;
    public byte Unused2;
    public byte PressedButton;
    public byte IsAButtonPressed;
    public byte PressedButtonScreen;
    public readonly byte[] Unused4 = new byte[8];
    public byte CurrentState;
    public byte QueuedState;
    public byte CurrentScreen;
    public byte CurrentBattlePocket;
    public sbyte QueuedBattlePocketPageChange;
    public readonly byte[] NumBattlePocketItems = new byte[BattleBagContext.NumPockets];
    public readonly byte[] NumBattlePocketPages = new byte[BattleBagContext.NumPockets];
    public byte CatchTutorialState;
    public byte CatchTutorialTickCount;

    private static ushort[][] Buttons(int states, int tiles)
    {
        ushort[][] data = new ushort[states][];
        for (int i = 0; i < states; i++)
        {
            data[i] = new ushort[tiles];
        }
        return data;
    }
}
