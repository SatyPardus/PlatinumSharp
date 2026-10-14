namespace PlatinumSharp.Game;

public delegate int PartyMenuCallback(PartyMenuApplication app);

public sealed class PartyMenuMember
{
    public GameString? Name;
    public ushort Species;
    public ushort CurHP;
    public ushort MaxHP;
    public ushort Level;
    public ushort HeldItem;
    public ushort StatusIcon;
    public bool HideGenderMarker;
    public byte Gender;
    public bool IsContestEligible;
    public byte IsEgg;
    public byte Form;
    public ushort BallSeal;
    public sbyte PanelXPos;
    public sbyte PanelYPos;
    public short SpriteXDelta;
    public short SpriteYDelta;
    public short StatusXPos;
    public short StatusYPos;
    public short ItemXPos;
    public short ItemYPos;
    public Sprite? Sprite;
    public byte IconAnimFrame;
    public byte IsPresent;
}

public sealed class PartyMenuFormChange
{
    public int State;
    public int ElapsedFrames;
    public int FramesBeforeFormChange;
    public int Species;
    public uint NarcIndex;
    public int PartySlot;
    public object? ParticleSystem;
}

public sealed class PartyOrderSwitchData
{
    public readonly byte[][] BufPanels = { new byte[PartyMenuApplication.MemberPanelBytes], new byte[PartyMenuApplication.MemberPanelBytes] };
    public readonly byte[][] BufWindows = { new byte[PartyMenuApplication.MemberPanelBytes], new byte[PartyMenuApplication.MemberPanelBytes] };
    public readonly byte[] Slots = new byte[2];
    public readonly byte[] Sides = new byte[2];
    public byte InProgress;
    public byte State;
    public byte ScrollTicker;
}

public sealed class PartyMenuApplication
{
    public const int NumWindows = 37;
    public const int NumStrings = 20;
    public const int NumSprites = 29;
    public const int MemberPanelBytes = 0xC0;
    public const int NumMenuColors = 0x80;

    public BgConfig? BgConfig;
    public readonly Window[] Windows = NewWindows(NumWindows);
    public readonly Window[] MenuWindows = NewWindows(1);
    public readonly byte[] LeadMemberPanel = new byte[MemberPanelBytes];
    public readonly byte[] BackMemberPanel = new byte[MemberPanelBytes];
    public readonly byte[] NoneMemberPanel = new byte[MemberPanelBytes];
    public readonly ushort[] Colors = new ushort[NumMenuColors];
    public PartyMenu? PartyMenu;
    public SpriteSystem? SpriteSystem;
    public SpriteManager? SpriteMan;
    public readonly Sprite?[] Sprites = new Sprite?[NumSprites];
    public FontSpecialCharsContext? SpecialChars;
    public MessageLoader? MessageLoader;
    public StringTemplate? Template;
    public GameString? TmpString;
    public GameString? TmpFormat;
    public readonly GameString?[] MenuStrings = new GameString?[NumStrings];
    public StringListEntry[]? ContextMenuChoices;
    public Menu? ContextMenu;
    public readonly PartyMenuMember[] PartyMembers = NewMembers();
    public GridMenuCursorPosition[]? CursorPosTable;
    public readonly PartyOrderSwitchData OrderSwitch = new();
    public PartyMenuCallback? Callback;
    public PartyMenuCallback? OnYes;
    public PartyMenuCallback? OnNo;
    public byte TouchButtonState;
    public byte TouchButtonSlot;
    public byte StateAfterMessage;
    public byte SelectTargetSlot;
    public bool InTargetSlotMode;
    public bool HideCancel;
    public byte TextPrinterID;
    public byte CurrPartySlot;
    public byte PrevPartySlot;
    public byte CallbackState;
    public readonly ushort[] MonStats = new ushort[6];
    public HeightWeightData? HeightWeight;
    public PartyMenuFormChange? FormChanger;
    public G3DPipelineBuffers? FormChange3DPipeline;

    private static Window[] NewWindows(int count)
    {
        Window[] windows = new Window[count];
        for (int i = 0; i < count; i++)
        {
            windows[i] = new Window();
        }
        return windows;
    }

    private static PartyMenuMember[] NewMembers()
    {
        PartyMenuMember[] members = new PartyMenuMember[6];
        for (int i = 0; i < members.Length; i++)
        {
            members[i] = new PartyMenuMember();
        }
        return members;
    }
}
