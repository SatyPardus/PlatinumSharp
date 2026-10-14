namespace PlatinumSharp.Game;

public sealed class BattleSystem
{
    public const int Size = 0x2484;
    public const int MaxBattlerSlots = 4;

    public G3DPipelineBuffers? PipelineBuffers;
    public BgConfig? BgConfig;
    public Window[]? Windows;
    public MessageLoader? MsgLoader;
    public MessageLoader? AttackMsgLoader;
    public StringTemplate? StrFormatter;
    public GameString? MsgBuffer;
    public SysTask? TaskDrawSprites;
    public SysTask? TaskUpdateRedHPSound;
    public SysTask? TaskFlyInMessageBox;
    public PaletteData? PaletteData;
    public uint BattleType;
    public BattleContext? BattleCtx;
    public readonly BattlerData?[] Battlers = new BattlerData?[4];
    public int MaxBattlers;
    public readonly TrainerInfo?[] TrainerInfos = new TrainerInfo?[4];
    public Bag? Bag;
    public BagCursor? BagCursor;
    public Pokedex? Pokedex;
    public PCBoxes? PcBoxes;
    public readonly Party?[] Parties = new Party?[4];
    public readonly ChatotCry?[] ChatotCries = new ChatotCry?[4];
    public PokemonSpriteManager? MonSpriteMan;
    public BattleAnimSystem? BattleAnimSys;
    public SpriteSystem? SpriteSys;
    public SpriteManager? SpriteMan;
    public Poketch? Poketch;
    public CaptureAttempt? CaptureAttempt;
    public readonly ushort[] TrainerIDs = new ushort[4];
    public readonly byte[] Unwritten = new byte[4];
    public readonly byte[][] Trainers = { new byte[TrainerData.Size], new byte[TrainerData.Size], new byte[TrainerData.Size], new byte[TrainerData.Size] };
    public readonly Terrain[] Terrains = Enumerable.Range(0, 2).Select(_ => new Terrain()).ToArray();
    public BattleSubscreen? BtlSubscreen;
    public readonly PartyGauge?[] PartyGauges = new PartyGauge?[2];
    public FontSpecialCharsContext? SpecialCharsHP;
    public FontSpecialCharsContext? SpecialCharsLevel;
    public PaletteAnimator? PaletteAnimator;
    public Options? Options;
    public PalPad? PalPad;
    public WaitDial? WaitDial;
    public byte[]? SubscreenCursorOn;
    public object? TrainerIntroData;
    public PokemonAnimManager? MonAnimMan;
    public CellTransferState[]? CellTransferState;
    public readonly PokemonSpriteData[] PokemonSpriteDataArray = Enumerable.Range(0, 4).Select(_ => new PokemonSpriteData()).ToArray();
    public readonly BattleRecords UnusedBattleRecords = new();
    public GameRecords? Records;
    public byte[]? BgTileSnapshot;
    public ushort[]? BgPaletteSnapshot;
    public readonly byte[] ServerMessage = new byte[4096];
    public readonly byte[] ClientMessage = new byte[4096];
    public readonly ushort[] SavedBgPalettes = new ushort[112];
    public readonly ushort[] SavedObjPalettes = new ushort[112];
    public ushort ServerReadIndex;
    public ushort ServerWriteIndex;
    public ushort ServerEndIndex;
    public ushort ClientReadIndex;
    public ushort ClientWriteIndex;
    public ushort ClientEndIndex;
    public byte[]? LinkServerSenderState;
    public byte[]? LinkClientReceiverState;
    public byte BattleInitialized;
    public byte RenderMode;
    public byte CommandIsEndWait;
    public int PendingBlendReset;
    public int PendingBattleVRAMSetup;
    public int PendingSubMenuVRAMSetup;
    public int RedHPSoundFlag;
    public int RedHPSoundEffectDelay;
    public int BattleTerrain;
    public int Background;
    public int MapLabelTextID;
    public uint BattleStatusMask;
    public int Time;
    public int SafariBalls;
    public readonly byte[] BattleParticipantMask = new byte[4];
    public uint RulesetMask;
    public uint ResultMaskWord;

    public byte ResultMask
    {
        get => (byte)ResultMaskWord;
        set => ResultMaskWord = (ResultMaskWord & 0xFFFFFF00) | value;
    }

    public byte CatchingTutorialLowHP
    {
        get => (byte)(ResultMaskWord >> 8);
        set => ResultMaskWord = (ResultMaskWord & 0xFFFF00FF) | ((uint)value << 8);
    }

    public ushort BallsThrown
    {
        get => (ushort)(ResultMaskWord >> 16);
        set => ResultMaskWord = (ResultMaskWord & 0x0000FFFF) | ((uint)value << 16);
    }
    public int MapEvolutionMethod;
    public int VisitedContestHall;
    public int FieldWeather;
    public int MetBebe;
    public uint SeedLCRNG;
    public int YOffset;
    public int CaughtBattlerIdx;
    public int CommandSelectionFlags;
    public byte Unread2440;
    public byte OverlayFlags;
    public ushort NetworkID;
    public uint SeedRandNext;
    public uint SeedDTO;
    public readonly ushort[] RecordingCollectedPos = new ushort[4];
    public readonly ushort[] RecordingWritePos = new ushort[4];
    public readonly ushort[] RecordingAckPos = new ushort[4];
    public readonly int[] LinkPlayerPositions = new int[4];
    public int RecordingStopped;
    public int Padding24741;
    public SysTask? PlaybackStopButton;
    public readonly byte[] RecordedChatter = new byte[4];
}

public sealed class Terrain
{
    public ManagedSprite? ManagedSprite;
    public BattleSystem? BattleSys;
    public byte Side;
    public byte TerrainType;
    public short Unused0A;
    public short Unused0C;
}

public sealed class PokemonSpriteData
{
    public byte[]? Tiles;
    public int NarcID;
    public int Palette;
    public int YOffset;
}

public sealed class BattleRecords
{
    public int TotalTurns;
    public int TotalFainted;
    public int TotalDamage;
}

public sealed class BattleContext
{
    public readonly byte[] CurCommandState = new byte[4];
    public readonly byte[] NextCommandState = new byte[4];
    public int Command;
    public int CommandNext;
    public int FieldConditionCheckState;
    public int FieldConditionCheckTemp;
    public int MonConditionCheckState;
    public int MonConditionCheckTemp;
    public int SideConditionCheckState;
    public int SideConditionCheckTemp;
    public int TurnStartCheckState;
    public int TurnStartCheckTemp;
    public int AfterMoveHitCheckState;
    public int AfterMoveHitCheckTemp;
    public int AfterMoveMessageType;
    public int AfterMoveMessageState;
    public int AfterMoveEffectState;
    public int AfterMoveEffectTemp;
    public int BeforeMoveCheckState;
    public int TryMoveCheckState;
    public int StatusCheckState;
    public int AbilityCheckState;
    public int SwitchInCheckState;
    public int VanishedCheckTemp;
    public int Padding0060;
    public int Attacker;
    public int AttackerTemp;
    public int Defender;
    public int DefenderTemp;
    public int FaintedMon;
    public int SwitchedMon;
    public int SwitchedMonTemp;
    public int AbilityMon;
    public int MagicCoatMon;
    public int SideEffectType;
    public int SideEffectParam;
    public int SideEffectFlags;
    public int SideEffectMon;
    public int LastBattlerId;
    public int GainedExp;
    public int SharedExp;
    public readonly uint[] SideGetExpMask = new uint[2];
    public int ScriptNarc;
    public int ScriptFile;
    public int ScriptCursor;
    public int ScriptStackPointer;
    public readonly int[] ScriptStackNarc = new int[4];
    public readonly int[] ScriptStackFile = new int[4];
    public readonly int[] ScriptStackCursor = new int[4];
    public int TurnOrderCounter;
    public int WaitCounter;
    public readonly BattleMessage MsgBuffer = new();
    public int MsgBattlerTemp;
    public int MsgAttacker;
    public int MsgDefender;
    public int MsgMoveTemp;
    public int MsgItemTemp;
    public int MsgAbilityTemp;
    public int MsgTemp;
    public int CalcTemp;
    public int ScriptTemp;
    public readonly uint[] BattlerStatusFlags = new uint[4];
    public uint PayDayCount;
    public int TotalTurns;
    public readonly int[] TotalFainted = new int[4];
    public readonly int[] TotalDamage = new int[4];
    public int MeFirstTurnOrder;
    public object? TaskData;
    public object? TmpData;
    public uint FieldConditionsMask;
    public readonly FieldConditions FieldConditions = new();
    public readonly uint[] SideConditionsMask = new uint[2];
    public readonly SideConditions[] SideConditions = Enumerable.Range(0, 2).Select(_ => new SideConditions()).ToArray();
    public readonly TurnFlags[] TurnFlags = Enumerable.Range(0, 4).Select(_ => new TurnFlags()).ToArray();
    public readonly SelfTurnFlags[] SelfTurnFlags = Enumerable.Range(0, 4).Select(_ => new SelfTurnFlags()).ToArray();
    public readonly MoveFailFlags[] MoveFailFlags = Enumerable.Range(0, 4).Select(_ => new MoveFailFlags()).ToArray();
    public readonly AIContext AiContext = new();
    public uint[]? AiScriptTemp;
    public uint AiScriptTempAddress;
    public uint AiScriptCursor;
    public uint BattleStatusMask;
    public uint BattleStatusMask2;
    public int Damage;
    public int HitDamage;
    public int CriticalBoosts;
    public int CriticalMul;
    public int MovePower;
    public int PowerMul;
    public int HpCalcTemp;
    public int MoveType;
    public int MoveEffectChance;
    public int PrizeMoneyMul;
    public uint MoveStatusFlags;
    public uint SideEffectDirectFlags;
    public uint SideEffectIndirectFlags;
    public uint SideEffectAbilityFlags;
    public byte MultiHitCounter;
    public byte MultiHitNumHits;
    public byte BattlerCounter;
    public byte BeatUpCounter;
    public int MultiHitLoop;
    public uint MultiHitCheckFlags;
    public uint MultiHitAccuracyCheck;
    public readonly uint[] ClearVolatileStatus = new uint[4];
    public readonly byte[] SelectedPartySlot = new byte[4];
    public readonly byte[] SwitchedPartySlot = new byte[4];
    public readonly byte[] AiSwitchedPartySlot = new byte[4];
    public readonly uint[,] BattlerActions = new uint[4, 4];
    public readonly byte[] BattlerActionOrder = new byte[4];
    public readonly byte[] MonSpeedOrder = new byte[4];
    public readonly uint[] MonSpeedValues = new uint[4];
    public readonly byte[,,] IoQueue = new byte[4, 4, 16];
    public readonly byte[,] IoBuffer = new byte[4, 256];
    public readonly int[] BattleScript = new int[400];
    public readonly BattleMon[] BattleMons = Enumerable.Range(0, 4).Select(_ => new BattleMon()).ToArray();
    public uint MoveTemp;
    public uint MoveCur;
    public uint MovePrev;
    public readonly uint[] MoveLockedInto = new uint[4];
    public readonly ushort[] MoveProtect = new ushort[4];
    public readonly ushort[] MoveHit = new ushort[4];
    public readonly ushort[] MoveHitBattler = new ushort[4];
    public readonly ushort[] MoveHitType = new ushort[4];
    public readonly ushort[] MovePrevByBattler = new ushort[4];
    public readonly ushort[] MoveCopied = new ushort[4];
    public readonly ushort[,] MoveCopiedHit = new ushort[4, 4];
    public readonly ushort[] MoveSketched = new ushort[4];
    public readonly ushort[] MoveSelected = new ushort[4];
    public readonly ushort[] MoveSlot = new ushort[4];
    public readonly ushort[] Conversion2Move = new ushort[4];
    public readonly ushort[] Conversion2Battler = new ushort[4];
    public readonly ushort[] Conversion2Type = new ushort[4];
    public readonly ushort[] MetronomeMove = new ushort[4];
    public readonly int[] StoredDamage = new int[4];
    public readonly int[] LastHitByBattler = new int[4];
    public int BattlerSpeedTemp;
    public byte BattlersSwitchingMask;
    public byte LevelUpMons;
    public ushort Padding310A;
    public readonly ushort[] SpeedRand = new ushort[4];
    public int FlingTemp;
    public int FlingScript;
    public byte SafariCatchStage;
    public byte SafariEscapeCount;
    public byte RunAttempts;
    public byte BattleEndFlag;
    public byte Magnitude;
    public byte FieldWeatherChecked;
    public short HpTemp;
    public readonly ushort[] RecycleItem = new ushort[4];
    public readonly byte[,] PartyOrder = new byte[4, 6];
    public int ExpJinglePlayed;
    public int LinkBattleTimeout;
    public readonly byte[] RecordedCommandFlags = new byte[4];
    public int WaitingBattlers;
    public int BattleProgressFlag;
    public int Padding315401;
}

public sealed class BattleMessage
{
    public byte CommandCode;
    public byte Tags;
    public ushort Id;
    public readonly int[] Params = new int[6];
    public int Digits;
    public int Battler;
}

public sealed class FieldConditions
{
    public uint WeatherTurns;
    public readonly byte[] FutureSightTurns = new byte[4];
    public readonly byte[] WishTurns = new byte[4];
    public readonly ushort[] FutureSightMove = new ushort[4];
    public readonly int[] FutureSightAttacker = new int[4];
    public readonly int[] FutureSightDamage = new int[4];
    public readonly byte[] WishTarget = new byte[4];
}

public sealed class SideConditions
{
    public int ReflectUser;
    public int ReflectTurns;
    public int LightScreenUser;
    public int LightScreenTurns;
    public int MistUser;
    public int MistTurns;
    public int SafeguardUser;
    public int SafeguardTurns;
    public int FollowMe;
    public int FollowMeUser;
    public int KnockedOffItemsMask;
    public int Padding001D;
    public int SpikesLayers;
    public int ToxicSpikesLayers;
    public int Padding0404;
}

public sealed class TurnFlags
{
    public int Struggling;
    public int PpDecremented;
    public int Protecting;
    public int HelpingHand;
    public int MagicCoat;
    public int Snatching;
    public int Roosting;
    public int Fleeing;
    public int Enduring;
    public int Padding000A;
    public readonly int[] PhysicalDamageTakenFrom = new int[4];
    public int PhysicalDamageLastAttacker;
    public int PhysicalDamageAttackerMask;
    public readonly int[] SpecialDamageTakenFrom = new int[4];
    public int SpecialDamageLastAttacker;
    public int SpecialDamageAttackerMask;
    public int LastDamageTaken;
    public int LastAttacker;
    public int AssuranceDamageMask;
}

public sealed class SelfTurnFlags
{
    public int SkipPressureCheck;
    public int LightningRodActivated;
    public int StormDrainActivated;
    public int MoldBreakerActivated;
    public int TrickRoomActivated;
    public int FocusItemActivated;
    public int RepeatedMoveCount;
    public int Padding0009;
    public int PhysicalDamageTaken;
    public int PhysicalDamageLastAttacker;
    public int SpecialDamageTaken;
    public int SpecialDamageLastAttacker;
    public int StatusFlags;
    public int ShellBellDamageDealt;
}

public sealed class MoveFailFlags
{
    public int Paralyzed;
    public int NoEffect;
    public int Imprisoned;
    public int Infatuated;
    public int Disabled;
    public int Taunted;
    public int Flinched;
    public int Confused;
    public int Gravity;
    public int HealBlocked;
    public int Padding000A;
}

public sealed class AIContext
{
    public byte EvalStep;
    public byte MoveSlot;
    public ushort Move;
    public readonly sbyte[] MoveScore = new sbyte[4];
    public int CalcTemp;
    public uint ThinkingMask;
    public byte StateFlags;
    public byte ThinkingBitShift;
    public byte Padding0012;
    public byte Padding0013;
    public int Padding0014;
    public readonly byte[] MoveDamageRolls = new byte[4];
    public readonly ushort[,] BattlerMoves = new ushort[4, 4];
    public readonly byte[] BattlerAbilities = new byte[4];
    public readonly ushort[] BattlerHeldItems = new ushort[4];
    public readonly ushort[,] TrainerItems = new ushort[2, 4];
    public readonly uint[] ScriptStackPointer = new uint[8];
    public byte ScriptStackSize;
    public readonly byte[] TrainerItemCounts = new byte[2];
    public byte Attacker;
    public byte Defender;
    public readonly byte[] UsedItemType = new byte[2];
    public readonly byte[] UsedItemCondition = new byte[2];
    public readonly ushort[] UsedItem = new ushort[2];
    public readonly byte[] SelectedTarget = new byte[4];
    public readonly MoveData[] MoveTable = Enumerable.Range(0, 468).Select(_ => new MoveData()).ToArray();
    public byte[]? ItemTable;
    public readonly ushort[] Padding1DD0 = new ushort[4];
    public readonly ushort[] Padding1DD8 = new ushort[4];
}

public sealed class MoveData
{
    public ushort Effect;
    public byte Class;
    public byte Power;
    public byte Type;
    public byte Accuracy;
    public byte Pp;
    public byte EffectChance;
    public ushort Range;
    public sbyte Priority;
    public byte Flags;
    public readonly MoveContestData Contest = new();

    public byte[] ToBytes()
    {
        byte[] data = new byte[0x10];
        data[0] = (byte)Effect;
        data[1] = (byte)(Effect >> 8);
        data[2] = Class;
        data[3] = Power;
        data[4] = Type;
        data[5] = Accuracy;
        data[6] = Pp;
        data[7] = EffectChance;
        data[8] = (byte)Range;
        data[9] = (byte)(Range >> 8);
        data[0xA] = (byte)Priority;
        data[0xB] = Flags;
        data[0xC] = Contest.Effect;
        data[0xD] = Contest.Type;
        return data;
    }
}

public sealed class MoveContestData
{
    public byte Effect;
    public byte Type;
    public readonly byte[] Padding02 = new byte[2];
}

public sealed class BattleMon
{
    public ushort Species;
    public ushort Attack;
    public ushort Defense;
    public ushort Speed;
    public ushort SpAttack;
    public ushort SpDefense;
    public readonly ushort[] Moves = new ushort[4];
    public int HpIV;
    public int AttackIV;
    public int DefenseIV;
    public int SpeedIV;
    public int SpAttackIV;
    public int SpDefenseIV;
    public int IsEgg;
    public int HasNickname;
    public readonly sbyte[] StatBoosts = new sbyte[8];
    public int Weight;
    public byte Type1;
    public byte Type2;
    public int FormNum;
    public int IsShiny;
    public int Padding002606;
    public byte Ability;
    public int WeatherAbilityAnnounced;
    public int IntimidateAnnounced;
    public int TraceAnnounced;
    public int DownloadAnnounced;
    public int AnticipationAnnounced;
    public int ForewarnAnnounced;
    public int SlowStartAnnounced;
    public int SlowStartFinished;
    public int FriskAnnounced;
    public int MoldBreakerAnnounced;
    public int PressureAnnounced;
    public int Padding00280B;
    public readonly byte[] PpCur = new byte[4];
    public readonly byte[] PpUps = new byte[4];
    public byte Level;
    public byte Friendship;
    public readonly ushort[] Nickname = new ushort[11];
    public int CurHP;
    public uint MaxHP;
    public readonly ushort[] OTName = new ushort[8];
    public uint Exp;
    public uint Personality;
    public uint Status;
    public uint StatusVolatile;
    public uint OTId;
    public ushort HeldItem;
    public ushort Padding007A;
    public byte TimesDamaged;
    public byte TrainerMessageFlags;
    public int Gender;
    public int OTGender;
    public byte CapturedBall;
    public uint MoveEffectsMask;
    public uint MoveEffectsTemp;
    public readonly MoveEffectsData MoveEffectsData = new();
}

public sealed class MoveEffectsData
{
    public int DisabledTurns;
    public int EncoredTurns;
    public int ChargedTurns;
    public int TauntedTurns;
    public int ProtectSuccessTurns;
    public int PerishSongTurns;
    public int RolloutCount;
    public int FuryCutterCount;
    public int StockpileCount;
    public int StockpileDefBoosts;
    public int StockpileSpDefBoosts;
    public int Truant;
    public int FlashFire;
    public int LockOnTarget;
    public int MimickedMoveSlot;
    public int BindTarget;
    public int MeanLookTarget;
    public int LastResortCount;
    public int MagnetRiseTurns;
    public int HealBlockTurns;
    public int EmbargoTurns;
    public int CanUnburden;
    public int MetronomeTurns;
    public int MicleBerry;
    public int CustapBerry;
    public int QuickClaw;
    public int MeFirst;
    public int Padding00041F;
    public int RechargeTurnNumber;
    public int FakeOutTurnNumber;
    public int SlowStartTurnNumber;
    public int MeFirstTurnNumber;
    public int SubstituteHP;
    public uint TransformedPID;
    public ushort DisabledMove;
    public ushort BindingMove;
    public ushort EncoredMove;
    public ushort EncoredMoveSlot;
    public readonly ushort[] LastResortMoves = new ushort[4];
    public ushort ChoiceLockedMove;
    public ushort TransformedGender;
    public int ItemHPRecovery;
}

public sealed class BattlerData
{
    public readonly BattlerDataSysTaskFuncs TaskFuncs = new();
    public ManagedSprite? ManagedSprite;
    public PokemonSprite? MonSpriteBackup;
    public PokemonSprite? MonSprite;
    public Window? Unused24;
    public readonly HealthBox Healthbox = new();
    public readonly SavedCursorPosition SavedCursorPos = new();
    public BallRotation? BallRotation;
    public object? BtlMonObjData;
    public object? Unused8C;
    public readonly byte[] Data = new byte[256];
    public byte Battler;
    public byte BattlerType;
    public byte BootState;
    public byte CommandUIReady;
    public SysTask? SysTask;
    public ushort Degrees;
    public int IsAlternateSprite;
    public NARC? Narc;
    public byte MsgPending;
    public readonly byte[] Unused1A5 = new byte[3];
}

public sealed class BattlerDataSysTaskFuncs
{
    public int SetCommandSelection;
    public int ShowMoveSelectMenu;
    public int ShowTargetSelectMenu;
    public int ShowBagMenu;
    public int ShowPartyMenu;
    public int ShowYesNoMenu;
}

public sealed class HealthBox
{
    public byte State;
    public ManagedSprite? MainSprite;
    public ManagedSprite? ArrowSprite;
    public BattleSystem? BattleSys;
    public SysTask? Task10;
    public readonly Window Unused14 = new();
    public byte Battler;
    public byte Type;
    public byte SelectedPartySlot;
    public byte NumSafariBalls;
    public int CurHP;
    public int MaxHP;
    public int Damage;
    public int HpTemp;
    public int CurExp;
    public int MaxExp;
    public int ExpReward;
    public int ExpTemp;
    public byte Level;
    public byte Gender;
    public byte Status;
    public byte CaughtSpecies;
    public byte Command;
    public byte Delay;
    public byte ExpSoundTimer;
    public int ScrollDirection;
    public int DoneScrolling;
    public int Unused4F2;
    public int NumberMode;
    public int Padding;
    public SysTask? BobbingAnimTask;
    public ushort Degrees;
}

public sealed class SavedCursorPosition
{
    public sbyte ActionMenuX;
    public sbyte ActionMenuY;
    public sbyte MoveMenuX;
    public sbyte MoveMenuY;
    public sbyte TargetSelectX;
    public sbyte TargetSelectY;
    public sbyte TargetingLayout;
    public byte Padding;

    public void Clear()
    {
        ActionMenuX = 0;
        ActionMenuY = 0;
        MoveMenuX = 0;
        MoveMenuY = 0;
        TargetSelectX = 0;
        TargetSelectY = 0;
        TargetingLayout = 0;
        Padding = 0;
    }
}
