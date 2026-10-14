namespace PlatinumSharp.Game;

public sealed class FieldProcessManager
{
    public const int ParentOffset = 0x00;
    public const int ChildOffset = 0x04;
    public const int PauseOffset = 0x08;
    public const int KillOffset = 0x0C;
    public const int Size = 0x10;

    public ApplicationManager? Parent;
    public ApplicationManager? Child;
    public int Pause;
    public int Kill;
}

public sealed partial class FieldMapContext
{
    public const int MapNamePopUpOffset = 0x08;
    public const int PoketchSystemOffset = 0x14;

    public object? MapNamePopUp;
    public object? PoketchSystem;
}

public sealed partial class FieldInput
{
    public const int PressedKeysOffset = 0x04;
    public const int HeldKeysOffset = 0x06;
    public const int Size = 0x08;

    public ushort PressedKeys;
    public ushort HeldKeys;
}

public sealed partial class FieldSystem
{
    public const int ProcessManagerOffset = 0x00;
    public const int FieldMapContextOffset = 0x04;
    public const int BgConfigOffset = 0x08;
    public const int SaveDataOffset = 0x0C;
    public const int TaskOffset = 0x10;
    public const int LocationOffset = 0x1C;
    public const int LandDataManagerOffset = 0x28;
    public const int MapMatrixOffset = 0x2C;
    public const int PlayerAvatarOffset = 0x3C;
    public const int RunningFieldMapOffset = 0x68;
    public const int MapLoadTypeOffset = 0x70;
    public const int RadarChainOffset = 0x94;
    public const int BagCursorOffset = 0x98;
    public const int PokedexMemoryOffset = 0xB4;
    public const int BattleSubscreenCursorOnOffset = 0xBC;
    public const int Size = 0xCC;

    public FieldProcessManager? ProcessManager;
    public FieldMapContext? FieldMapContext;
    public BgConfig? BgConfig;
    public SaveData? SaveData;
    public FieldTask? Task;
    public BufferView? Location;
    public LandDataManager? LandDataManager;
    public MapMatrix? MapMatrix;
    public PlayerAvatar? PlayerAvatar;
    public int RunningFieldMap;
    public int MapLoadType;
    public ushort WildEncounterSteps;
    public RadarChain? RadarChain;
    public BagCursor? BagCursor;
    public PokedexMemory? PokedexMemory;
    public byte[]? BattleSubscreenCursorOn;
}
