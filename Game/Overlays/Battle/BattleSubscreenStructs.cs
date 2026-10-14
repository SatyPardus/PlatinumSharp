namespace PlatinumSharp.Game;

public sealed class BattleSubscreen
{
    public BattleSystem? BattleSys;
    public byte[]? SuppressActivationSfxPtr;
    public SysTask? AnimatePartyBallsTask;
    public SysTask? MenuTransitionTask;
    public SysTask? Unused10;
    public readonly UnusedStructOv1602268A14Sub1 Unused14 = new();
    public readonly ActiveMenuData ActiveMenuData = new();
    public readonly byte[]?[] TilemapBuffers = new byte[]?[7];
    public ushort[]? SubscreenPaletteBuf;
    public ushort[]? NormalMovePalBuf;
    public ushort[]? SpeedUpMovePalBuf;
    public SysTask? SpeedUpPaletteTask;
    public readonly MoveDisplayData[] MoveDisplayData = Enumerable.Range(0, 4).Select(_ => new MoveDisplayData()).ToArray();
    public FontOAMManager? FontOAMManager;
    public readonly FontOAMEntry[] FontOAMEntry = Enumerable.Range(0, 13).Select(_ => new FontOAMEntry()).ToArray();
    public object? Unused5B8;
    public readonly ManagedSprite?[] PlayerPartyBallSprites = new ManagedSprite?[6];
    public readonly ManagedSprite?[] OpponentPartyBallSprites = new ManagedSprite?[6];
    public readonly ManagedSprite?[] MoveSelectSprites = new ManagedSprite?[4];
    public readonly ManagedSprite?[] CategoryIconSprites = new ManagedSprite?[4];
    public readonly ManagedSprite?[] TargetSelectPokeIconSprites = new ManagedSprite?[4];
    public readonly SysTask?[] PokeIconAnimTasks = new SysTask?[4];
    public readonly PartyBallAnimState[] PartyBallAnimState = Enumerable.Range(0, 6).Select(_ => new PartyBallAnimState()).ToArray();
    public SysTask? Unused664;
    public byte Unused668;
    public byte Unused669;
    public byte BattlerType;
    public sbyte ActiveMenuConfigIndex;
    public byte TargetingLayout;
    public byte TrainerGender;
    public byte IsPanelSliding;
    public byte HasCancelButton;
    public byte Unused670;
    public short Unused672;
    public byte PulseCursorDirection;
    public short PulseCursorBlendValue;
    public SysTask? PulseCursorTask;
    public readonly MenuButtonPressAnimState MenuButtonPressAnimState = new();
    public readonly UnusedStructOv1602268A14Sub4 Unused6A0 = new();
    public int PanelScrollPos;
    public int SlideSpeed;
    public int TargetOffset;
    public BattleCursor? CursorRenderer;
    public readonly MenuCursor Cursor = new();
    public byte SuppressActivationSfx;
    public byte IsWaitingForPartner;
    public readonly BattleSubscreenCatchTutorialState CatchTutorialState = new();
}

public sealed class UnusedStructOv1602268A14Sub1
{
    public object? Unk00;
    public byte Unk04;
    public sbyte Unk05;
}

public sealed class ActiveMenuData
{
    public readonly ActionSelectData ActionData = new();
    public readonly MoveSelectData MoveData = new();
    public readonly TargetSelectData TargetSelectData = new();
    public readonly GiveUpMoveData GiveUpMoveData = new();
}

public sealed class ActionSelectData
{
    public byte BattlerType;
    public byte Battler;
    public byte PartySlot;
    public byte BallStatusBattler;
    public short CurHp;
    public ushort MaxHp;
    public byte IsWaitingForPartner;
}

public sealed class MoveSelectData
{
    public readonly ushort[] MoveIDs = new ushort[4];
    public readonly byte[] CurrPP = new byte[4];
    public readonly byte[] MaxPP = new byte[4];
    public byte BattlerType;
}

public sealed class TargetSelectData
{
    public readonly TargetMonData[] TargetMonData = Enumerable.Range(0, 4).Select(_ => new TargetMonData()).ToArray();
    public byte BattlerType;
    public byte TargetingLayout;
}

public sealed class TargetMonData
{
    public byte PartySlot;
    public int Gender;
    public int IsPresent;
    public byte StockStatus;
    public byte Padding03;
    public short CurHP;
    public ushort MaxHP;
}

public sealed class GiveUpMoveData
{
    public ushort Move;
    public ushort Unused02;
}

public sealed class MoveDisplayData
{
    public readonly MoveDisplayInfo MoveDisplayInfo = new();
    public readonly byte[]?[] MoveIcons = new byte[]?[4];
    public readonly TextWindowLayout[] MoveNameWindows = Enumerable.Range(0, 4).Select(_ => new TextWindowLayout()).ToArray();
    public readonly TextWindowLayout[] PpCountWindows = Enumerable.Range(0, 4).Select(_ => new TextWindowLayout()).ToArray();
    public readonly TextWindowLayout[] PpLabelWindows = Enumerable.Range(0, 4).Select(_ => new TextWindowLayout()).ToArray();
}

public sealed class MoveDisplayInfo
{
    public readonly ushort[] Move = new ushort[4];
    public readonly ushort[] CurPP = new ushort[4];
    public readonly ushort[] MaxPP = new ushort[4];
}

public sealed class TextWindowLayout
{
    public readonly Window Window = new();
    public ushort TileWidth;
    public ushort PixelWidth;
}

public sealed class FontOAMEntry
{
    public FontOAM? FontOAM;
    public readonly CharTransferAllocation Allocation = new();
    public ushort TextWidth;
}

public sealed class PartyBallAnimState
{
    public short XOffset;
    public byte ExpPercent;
    public byte Phase;
    public byte Unused04;
    public byte Delay;
    public byte FrameCount;
    public byte Duration;
    public byte BounceCount;
}

public sealed class MenuButtonPressAnimState
{
    public short Phase;
    public short WaitTimer;
    public readonly MenuButtonPressAnimStateField Field = new();
}

public sealed class MenuButtonPressAnimStateField
{
    public readonly MenuButtonPressAnimStateFieldButtonPressAnimationState ButtonPressAnimationState = new();
    public readonly MenuButtonPressAnimStateFieldTargetSelectAnimationState TargetSelectAnimationState = new();
}

public sealed class MenuButtonPressAnimStateFieldButtonPressAnimationState
{
    public short[]? Keyframes;
    public byte[]? SubscreenTilemapRect;
    public int SelectedButton;
    public byte SrcBufIndex;
    public byte FontOAMIdx;
    public byte SpriteIndex;
    public byte Unread;
    public byte FreeSprites;
    public readonly CoordinatesS16 ScreenPos = new();
}

public sealed class CoordinatesS16
{
    public short X;
    public readonly CoordinatesS16Field Field = new();
}

public sealed class CoordinatesS16Field
{
    public short Y;
    public short Z;
}

public sealed class MenuButtonPressAnimStateFieldTargetSelectAnimationState
{
    public readonly object?[] Unused00 = new object?[4];
    public object? Unused10;
    public object? Unused14;
    public object? Unused18;
    public int SelectedTarget;
}

public sealed class UnusedStructOv1602268A14Sub4
{
    public int Unk00;
    public short Unk04;
    public short Unk06;
    public byte Unk08;
    public byte Unk09;
}

public sealed class MenuCursor
{
    public byte IsActive;
    public sbyte Y;
    public sbyte X;
    public byte Unused03;
}

public sealed class BattleSubscreenCatchTutorialState
{
    public object? Indicator;
    public byte Step;
    public byte Phase;
    public byte Delay;
}
