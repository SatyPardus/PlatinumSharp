namespace PlatinumSharp.Game;

public sealed class BattleAnimLoop
{
    public int StartPos;
    public byte CurIteration;
    public byte MaxIterations;
    public int IsActive;
}

public sealed class BattleAnimPokemonSpriteDrawContext
{
    public BattleAnimSystem? BattleAnimSys;
    public SpriteManager? SpriteManager;
    public ManagedSprite? Sprite;
    public int Active;
}

public sealed class BattleAnimBgRefs
{
    public int NarcID;
    public int TilesNarcMemberIdx;
    public int PaletteNarcMemberIdx;
    public int TilemapNarcMemberIdx;
    public int PaletteDestStart;
    public int PaletteSrcSize;
}

public sealed class BattleAnimContext
{
    public const int MaxParticleSystems = 16;
    public const int MaxEmitters = 20;
    public const int MaxBattlers = 4;

    public byte Command;
    public byte Unknown01;
    public ushort Move;
    public int Damage;
    public ushort Power;
    public ushort Friendship;
    public uint FieldConditions;
    public ushort EffectChance;
    public ushort Terrain;
    public ushort Attacker;
    public ushort Defender;
    public int CurrentParticleSystem;
    public readonly ParticleSystem?[] ParticleSystems = new ParticleSystem?[MaxParticleSystems];
    public readonly SplEmitter?[] Emitters = new SplEmitter?[MaxEmitters];
    public SpriteSystem? SpriteSystem;
    public readonly PokemonSpriteData?[] PokemonSpriteData = new PokemonSpriteData?[MaxBattlers];
    public readonly byte[] BattlerTypes = new byte[MaxBattlers];
    public readonly PokemonSprite?[] BattlerSprites = new PokemonSprite?[MaxBattlers];
    public uint BattleType;
    public readonly ushort[] BattlerSpecies = new ushort[MaxBattlers];
    public readonly byte[] BattlerGenders = new byte[MaxBattlers];
    public readonly byte[] BattlerShinyFlags = new byte[MaxBattlers];
    public readonly byte[] BattlerForms = new byte[MaxBattlers];
    public readonly uint[] BattlerPersonalities = new uint[MaxBattlers];
    public readonly uint[] BattlerMoveEffects = new uint[MaxBattlers];
    public ChatotCry? ChatotCry;
    public byte[]? BgTiles;
    public ushort[]? BgPaletteBuffer;
    public int IsTransformed;

    public void Clear()
    {
        Command = 0;
        Unknown01 = 0;
        Move = 0;
        Damage = 0;
        Power = 0;
        Friendship = 0;
        FieldConditions = 0;
        EffectChance = 0;
        Terrain = 0;
        Attacker = 0;
        Defender = 0;
        CurrentParticleSystem = 0;
        Array.Clear(ParticleSystems);
        Array.Clear(Emitters);
        SpriteSystem = null;
        Array.Clear(PokemonSpriteData);
        Array.Clear(BattlerTypes);
        Array.Clear(BattlerSprites);
        BattleType = 0;
        Array.Clear(BattlerSpecies);
        Array.Clear(BattlerGenders);
        Array.Clear(BattlerShinyFlags);
        Array.Clear(BattlerForms);
        Array.Clear(BattlerPersonalities);
        Array.Clear(BattlerMoveEffects);
        ChatotCry = null;
        BgTiles = null;
        BgPaletteBuffer = null;
        IsTransformed = 0;
    }
}

public sealed class BattleAnimSystem
{
    public const int CallStackDepth = 3;
    public const int LoopStackDepth = 3;
    public const int DrawContextCount = 2;
    public const int CameraCount = 16;
    public const int ScriptVarCount = 10;
    public const int SpriteManagerCount = 4;
    public const int SpriteCount = 10;
    public const int PokemonSpriteCount = 5;
    public const int ArcCount = 6;
    public const int BgLayerCount = 4;

    public int HeapID;
    public int MoveArcID;
    public int IsContest;
    public int IsActive;
    public int MoveActive;
    public uint[]? ScriptData;
    public int ScriptPtr;
    public readonly int[] CallStack = new int[CallStackDepth];
    public readonly BattleAnimLoop[] LoopStack = { new(), new(), new() };
    public readonly BattleAnimPokemonSpriteDrawContext[] PokemonSpriteDrawContexts = { new(), new() };
    public readonly byte[] CameraProjections = new byte[CameraCount];
    public readonly byte[] ParticleSystemCameraFlip = new byte[CameraCount];
    public byte Unused88;
    public byte ScriptDelay;
    public ushort ActiveAnimTasks;
    public ushort ActiveSoundTasks;
    public readonly int[] ScriptVars = new int[ScriptVarCount];
    public Action<BattleAnimSystem>? ExecuteAnimScriptFunc;
    public BattleAnimContext? Context;
    public BgConfig? BgConfig;
    public PaletteData? PaletteData;
    public readonly SpriteManager?[] SpriteManagers = new SpriteManager?[SpriteManagerCount];
    public readonly ManagedSprite?[] Sprites = new ManagedSprite?[SpriteCount];
    public SpriteTemplate LastSpriteTemplate = new();
    public SpriteManager? PokemonSpriteManager;
    public readonly ManagedSprite?[] PokemonSprites = new ManagedSprite?[PokemonSpriteCount];
    public readonly int[] UsedPokemonSprites = new int[PokemonSpriteCount];
    public readonly SpriteTrackingTask?[] SpriteTrackingTasks = new SpriteTrackingTask?[PokemonSpriteCount];
    public BgTrackingTask? BgTrackingTask;
    public byte BgSwitchState;
    public byte SoundEffectWaitTimer;
    public byte EndDelayFrames;
    public byte Unused17B;
    public BattleBgAnim? BgAnim;
    public readonly BattleAnimBgRefs BattleBgRefs = new();
    public int BaseBgPalettes;
    public readonly byte[] BgLayerPriorities = new byte[BgLayerCount];
    public readonly NARC?[] Arcs = new NARC?[ArcCount];
}

public static partial class BattleAnimSystems
{
    private const int NarcBattleGraphics = 7;
    private const int NarcBattleSubscreen = 0x1B;
    private const int NarcAnimTiles = 0x15;
    private const int NarcAnimCells = 0x16;
    private const int NarcAnimCellAnims = 0x17;
    private const int NarcAnimPalettes = 0x18;

    // OFFSET 0x0221FCDC
    public static BattleAnimSystem BattleAnimSystem_New(int heapID)
    {
        BattleAnimSystem system = new();
        system.HeapID = heapID;
        system.IsContest = 0;
        system.Context = new BattleAnimContext();
        system.Arcs[0] = NARC.NARC_ctor(NarcBattleGraphics, system.HeapID);
        system.Arcs[1] = NARC.NARC_ctor(NarcBattleSubscreen, system.HeapID);
        system.Arcs[2] = NARC.NARC_ctor(NarcAnimTiles, system.HeapID);
        system.Arcs[3] = NARC.NARC_ctor(NarcAnimCells, system.HeapID);
        system.Arcs[4] = NARC.NARC_ctor(NarcAnimCellAnims, system.HeapID);
        system.Arcs[5] = NARC.NARC_ctor(NarcAnimPalettes, system.HeapID);
        system.Context.Clear();
        system.IsActive = 0;
        system.ScriptPtr = 0;
        for (int i = 0; i < BattleAnimSystem.SpriteManagerCount; i++)
        {
            system.SpriteManagers[i] = null;
        }
        for (int i = 0; i < BattleAnimSystem.PokemonSpriteCount; i++)
        {
            system.SpriteTrackingTasks[i] = null;
        }
        system.BgTrackingTask = null;
        system.IsActive = 1;
        return system;
    }

    // OFFSET 0x0221FDC0
    public static void BattleAnimSystem_SetIsContest(BattleAnimSystem system, int isContest)
    {
        if (system == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        system!.IsContest = isContest;
    }

    // OFFSET 0x0221FDD4
    public static int BattleAnimSystem_IsContest(BattleAnimSystem system)
    {
        if (system == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return system!.IsContest;
    }

    // OFFSET 0x0221FDE4
    public static int BattleAnimSystem_GetHeapID(BattleAnimSystem system)
    {
        if (system == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return system!.HeapID;
    }

    // OFFSET 0x0221FDF4
    public static int BattleAnimSystem_Delete(BattleAnimSystem? system)
    {
        if (BattleAnimSystem_IsActive(system) == 0)
        {
            return 0;
        }
        for (int i = 0; i < BattleAnimSystem.ArcCount; i++)
        {
            NARC.NARC_dtor(system!.Arcs[i]!);
        }
        system!.Context = null;
        return 1;
    }

    // OFFSET 0x022201B4
    public static int BattleAnimSystem_IsActive(BattleAnimSystem? system)
    {
        if (system == null)
        {
            return 0;
        }
        return system.IsActive == 1 ? 1 : 0;
    }
}
