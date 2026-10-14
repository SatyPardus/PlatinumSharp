namespace PlatinumSharp.Game;

public sealed class BattlePartyContext
{
    public Party? Party;
    public object? Unused1;
    public BattleSystem? BattleSys;
    public int HeapID;
    public byte Unused2;
    public byte SelectedPartyIndex;
    public byte DoubleBattleFirstSelectedPartySlot;
    public byte Unused3;
    public byte PlayerPokemonPartySlot;
    public byte PartnerPokemonPartySlot;
    public readonly uint[] EmbargoRemainingTurns = new uint[2];
    public ushort CurrentDamage;
    public ushort SelectedBattleBagItem;
    public ushort MoveToLearn;
    public int Battler;
    public readonly byte[] PokemonPartySlots = new byte[6];
    public byte IsCursorEnabled;
    public byte SelectedBattleBagPocket;
    public byte SelectedMoveSlot;
    public byte BattlePartyMode;
    public byte BattlePartyExited;
}

public sealed class BattlePartyPokemonMove
{
    public ushort Move;
    public byte CurrentPP;
    public byte MaxPP;
    public byte Type;
    public byte Class;
    public byte Accuracy;
    public byte Power;
}

public sealed class BattlePartyPokemon
{
    public Pokemon? Mon;
    public ushort Species;
    public ushort Attack;
    public ushort Defence;
    public ushort Speed;
    public ushort SpAtk;
    public ushort SpDef;
    public ushort CurHP;
    public ushort MaxHP;
    public byte Type1;
    public byte Type2;
    public byte Level;
    public byte HideGenderMarker;
    public byte Gender;
    public byte SummaryStatus;
    public byte IsEgg;
    public ushort Ability;
    public ushort HeldItem;
    public uint Exp;
    public uint CurrentLevelBaseExp;
    public uint NextLevelExp;
    public byte Cool;
    public byte Beauty;
    public byte Cute;
    public byte Smart;
    public byte Tough;
    public byte Mail;
    public byte Form;
    public readonly BattlePartyPokemonMove[] Moves = Enumerable.Range(0, 4).Select(_ => new BattlePartyPokemonMove()).ToArray();
}

public sealed class BattleParty
{
    public const int Size = 0x2090;
    public const int NumSprites = 38;
    public const int NumMessageBoxWindows = 2;
    public const int ButtonStateMax = 4;
    public const int ButtonStateMaxCannotDisable = 3;
    public const int PartyPokemonButtonSizeTiles = 16 * 6;
    public const int ShiftPokemonButtonSizeTiles = 30 * 17;
    public const int SummaryCheckMovesButtonSizeTiles = 13 * 5;
    public const int MoveButtonSizeTiles = 16 * 6;
    public const int ContestStatsButtonSizeTiles = 9 * 4;
    public const int MoveSummaryScreenMoveButtonSizeTiles = 5 * 2;
    public const int PrevNextButtonSizeTiles = 5 * 5;
    public const int CancelButtonSizeTiles = 5 * 5;
    public const int ConfirmButtonSizeTiles = 26 * 5;

    public BattlePartyContext? Context;
    public readonly BattlePartyPokemon[] PartyPokemon = Enumerable.Range(0, 6).Select(_ => new BattlePartyPokemon()).ToArray();
    public BgConfig? Background;
    public PaletteData? Palette;
    public readonly ushort[][] PartyPokemonButtonData = Buttons(ButtonStateMax, PartyPokemonButtonSizeTiles);
    public readonly ushort[][] AltPartyPokemonButtonData = Buttons(ButtonStateMax, PartyPokemonButtonSizeTiles);
    public readonly ushort[][] SummaryCheckMovesButtonData = Buttons(ButtonStateMax, SummaryCheckMovesButtonSizeTiles);
    public readonly ushort[][] PrevPokemonButtonData = Buttons(ButtonStateMax, PrevNextButtonSizeTiles);
    public readonly ushort[][] NextPokemonButtonData = Buttons(ButtonStateMax, PrevNextButtonSizeTiles);
    public readonly ushort[][] CancelButtonData = Buttons(ButtonStateMax, CancelButtonSizeTiles);
    public readonly ushort[][] ShiftPokemonButtonData = Buttons(ButtonStateMaxCannotDisable, ShiftPokemonButtonSizeTiles);
    public readonly ushort[][] MoveButtonData = Buttons(ButtonStateMax, MoveButtonSizeTiles);
    public readonly ushort[][] LearnMoveConfirmButtonData = Buttons(ButtonStateMaxCannotDisable, ConfirmButtonSizeTiles);
    public readonly ushort[][] ContestStatsButtonData = Buttons(ButtonStateMaxCannotDisable, ContestStatsButtonSizeTiles);
    public readonly ushort[][] MoveSummaryScreenMoveButtonData = Buttons(ButtonStateMaxCannotDisable, MoveSummaryScreenMoveButtonSizeTiles);
    public readonly ushort[] ScreenPaletteData = new ushort[0x20];
    public byte PressedButtonState;
    public byte Unused1;
    public byte PressedButton;
    public byte Unused2;
    public byte UseAltButtons;
    public byte IsAButtonPressed;
    public FontSpecialCharsContext? Unused3;
    public MessageLoader? MessageLoader;
    public StringTemplate? StringTemplate;
    public GameString? String;
    public SpriteManager? SpriteManager;
    public readonly ManagedSprite?[] Sprites = new ManagedSprite?[NumSprites];
    public readonly Window[] MessageBoxWindows = Enumerable.Range(0, NumMessageBoxWindows).Select(_ => new Window()).ToArray();
    public Window[]? Windows;
    public byte NumWindows;
    public byte UseAltSummaryWindows;
    public byte PartySlotLearningMove;
    public byte DisplayingContestStats;
    public byte HasVisitedContestHall;
    public byte CurrentState;
    public byte QueuedState;
    public byte CurrentScreen;
    public byte TextPrinterID;
    public byte UseItemState;
    public ushort SelectedPokemonCurrentHP;
    public readonly ushort[] SelectedPokemonCurrentMovePPs = new ushort[4];
    public BattleSubMenuCursor? Cursor;
    public byte SelectPokemonPreviousScreenButton;
    public byte LearnMovePreviousScreenButton;
    public byte ConfirmLearnMovePreviousScreenButton;
    public byte BattlerSlot;

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
