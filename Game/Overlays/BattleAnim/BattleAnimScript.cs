using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class BattleAnimBattlerContext
{
    public SpriteSystem? SpriteSystem;
    public BgConfig? BgConfig;
    public PaletteData? PaletteData;
    public readonly PokemonSpriteData?[] PokemonSpriteData = new PokemonSpriteData?[4];
    public readonly byte[] BattlerTypes = new byte[4];
    public readonly PokemonSprite?[] BattlerSprites = new PokemonSprite?[4];
    public uint BattleType;
    public readonly ushort[] Species = new ushort[4];
    public readonly byte[] Genders = new byte[4];
    public readonly byte[] ShinyFlags = new byte[4];
    public readonly byte[] Forms = new byte[4];
    public readonly uint[] Personalities = new uint[4];
    public readonly uint[] MoveEffects = new uint[4];
    public int MoveArcID;
    public readonly BattleAnimBgRefs BattleBgRefs = new();
    public object? ChatotCry;
    public byte[]? BgTiles;
    public ushort[]? BgPaletteBuffer;
}

public sealed class BattlerSpriteContext
{
    public int TargetBattler;
    public int SourceBattler;
    public readonly PokemonSpriteData?[] PokemonSpriteData = new PokemonSpriteData?[4];
    public readonly PokemonSprite?[] Sprites = new PokemonSprite?[4];
    public readonly ushort[] Species = new ushort[4];
    public readonly byte[] Genders = new byte[4];
    public readonly byte[] ShinyFlags = new byte[4];
    public readonly byte[] Forms = new byte[4];
    public readonly uint[] Personalities = new uint[4];
    public readonly byte[] Types = new byte[4];
}

public sealed class SpriteTrackingData
{
    public byte FrameCount;
    public byte Interval;
    public PokemonSprite? Sprite;
}

public sealed class SpriteTrackingTask
{
    public ManagedSprite? Sprite;
    public readonly SpriteTrackingData Data = new();
    public SysTask? Task;
}

public sealed class BgTrackingTask
{
    public BgConfig? Bg;
    public readonly SpriteTrackingData Data = new();
    public SysTask? Task;
}

public static partial class BattleAnimSystems
{
    private const int TaskKindEffect = 1;
    private const int TaskKindSound = 2;
    private const uint AnimTaskPriority = 0x44C;
    private const int MoveSecretPower = 0x122;
    private const int MoveTriAttack = 0xA1;
    private const int MovePound = 1;
    private const int NumValidMoves = 0x1D3;
    private const int TerrainMax = 0x18;
    private const uint SecretPowerTableAddress = 0x02238680;
    private const int SecretPowerTableCount = 13;
    private const int Overlay = 12;
    private const int ScriptCommandCount = 0x55;
    private const int MaxSfxWaitFrames = 0x5A;
    private const int TrackingTaskBg = 4;
    private const int TrackingTaskCount = 5;
    private const int BattleBgBase = 2;
    private const int BattleBgEffect = 3;
    private const int MonSpriteXCenter = 0;
    private const int MonSpriteYCenter = 1;
    private const int MonSpriteHide = 6;
    private const int MonSpriteShadowHeight = 0x29;
    private const int MonSpriteFrameHalf = 0x28;
    private const int MonSpriteTileBytes = 0xC80;
    private const int MonSpriteBgClearBytes = 0x1900;
    private const int MonBgTilemapDefault = 0x108;
    private const int MonBgTilemapSymmetric = 0x109;
    private const int TrackingTaskPriority = 0x1001;
    private const int DrawTaskPriority = 0x1000;
    private const int DummyResourceBase = 0x4E21;
    private const int DummyResourceBattlerStride = 0x1388;
    private const int ArcBattBg = 0;
    private const int ArcBattObj = 1;
    private const int ArcWeChar = 2;
    private const int ArcWeCell = 3;
    private const int ArcWeCellAnm = 4;
    private const int ArcWePltt = 5;
    private const int SpriteResourceCount = 6;
    private const int PlttBufMainBg = 0;
    private const int PlttBufMainObj = 2;
    private const int PaletteSizeBytes = 0x20;
    private const int VramTypeMain = 1;
    private const int RoleAttacker = 0;
    private const int RoleDefender = 1;
    private const int RoleAttackerPartner = 2;
    private const int RoleDefenderPartner = 3;
    private const int RolePlayer1 = 4;
    private const int RoleEnemy1 = 5;
    private const int RolePlayer2 = 6;
    private const int RoleEnemy2 = 7;
    private const int BattlerSidePlayer = 3;
    private const int BattlerSideEnemy = 4;
    private const uint ParticleResIdxForMoveAddress = 0x022386B4;
    private const uint ParticleResIdxForFriendlyFireAddress = 0x02238744;
    private const uint WeatherConditionsAddress = 0x022385A4;
    private const int ParticleResourceCount = 6;
    private const uint BattleTypeDoublesAnim = 2;
    private const uint MoveEffectSemiInvulnerable = 0x400C0;
    private const int DebugParticleNarc = 0x64;
    private const int ExtraBgDefault = 7;
    private const int BaseBgPalettesBattle = 0xFF;
    private const int BlendPlaneNone = 0;
    private const int BlendPlaneAll = 0x3F;

    private static readonly Action<BattleAnimSystem>[] ScriptCommands =
    {
        BattleAnimScriptCmd_Delay,
        BattleAnimScriptCmd_WaitForAnimTasks,
        BattleAnimScriptCmd_BeginLoop,
        BattleAnimScriptCmd_EndLoop,
        BattleAnimScriptCmd_End,
        BattleAnimScriptCmd_PlaySoundEffect,
        BattleAnimScriptCmd_Nop0,
        BattleAnimScriptCmd_Nop1,
        BattleAnimScriptCmd_SetBG0BG1AlphaBlending,
        BattleAnimScriptCmd_SetDefaultAlphaBlending,
        BattleAnimScriptCmd_Call,
        BattleAnimScriptCmd_Return,
        BattleAnimScriptCmd_SetVar,
        BattleAnimScriptCmd_JumpIfEffectChanceOdd,
        BattleAnimScriptCmd_JumpIfEffectChance,
        BattleAnimScriptCmd_Jump,
        BattleAnimScriptCmd_SwitchBg,
        BattleAnimScriptCmd_SetBgSwitchVar,
        BattleAnimScriptCmd_RestoreBg,
        BattleAnimScriptCmd_WaitForPartialBgSwitch,
        BattleAnimScriptCmd_WaitForBgSwitch,
        BattleAnimScriptCmd_SetBg,
        BattleAnimScriptCmd_PlayPannedSoundEffect,
        BattleAnimScriptCmd_PanSoundEffects,
        BattleAnimScriptCmd_PlayMovingSoundEffectAtkDef,
        BattleAnimScriptCmd_PlayLoopedSoundEffect,
        BattleAnimScriptCmd_PlayDelayedSoundEffect,
        BattleAnimScriptCmd_Nop2,
        BattleAnimScriptCmd_Nop3,
        BattleAnimScriptCmd_WaitForSoundEffects,
        BattleAnimScriptCmd_JumpIfEqual,
        BattleAnimScriptCmd_LoadPokemonSpriteIntoBg,
        BattleAnimScriptCmd_RemovePokemonSpriteFromBg,
        BattleAnimScriptCmd_JumpIfUnk01,
        BattleAnimScriptCmd_SwitchBgEx,
        BattleAnimScriptCmd_PlayMovingSoundEffectNoCorrection,
        BattleAnimScriptCmd_PlayMovingSoundEffectAtkDef2,
        BattleAnimScriptCmd_Nop4,
        BattleAnimScriptCmd_Nop5,
        BattleAnimScriptCmd_Nop6,
        BattleAnimScriptCmd_Nop7,
        BattleAnimScriptCmd_Nop8,
        BattleAnimScriptCmd_Nop9,
        BattleAnimScriptCmd_Nop10,
        BattleAnimScriptCmd_StopSoundEffect,
        BattleAnimScriptCmd_CallFunc,
        BattleAnimScriptCmd_CreateEmitter,
        BattleAnimScriptCmd_CreateEmitterEx,
        BattleAnimScriptCmd_CreateEmitterForMove,
        BattleAnimScriptCmd_CreateEmitterForFriendlyFire,
        BattleAnimScriptCmd_WaitForAllEmitters,
        BattleAnimScriptCmd_LoadParticleSystem,
        BattleAnimScriptCmd_LoadDebugParticleSystem,
        BattleAnimScriptCmd_UnloadParticleSystem,
        BattleAnimScriptCmd_Nop11,
        BattleAnimScriptCmd_SetExtraParams,
        BattleAnimScriptCmd_InitPokemonSpriteManager,
        BattleAnimScriptCmd_LoadPokemonSpriteDummyResources,
        BattleAnimScriptCmd_AddPokemonSprite,
        BattleAnimScriptCmd_FreePokemonSpriteManager,
        BattleAnimScriptCmd_RemovePokemonSprite,
        BattleAnimScriptCmd_CancelTrackingTask,
        BattleAnimScriptCmd_SetCameraProjection,
        BattleAnimScriptCmd_SetCameraFlip,
        BattleAnimScriptCmd_JumpIfBattlerSide,
        BattleAnimScriptCmd_PlayPokemonCry,
        BattleAnimScriptCmd_WaitForPokemonCries,
        BattleAnimScriptCmd_ResetVars,
        BattleAnimScriptCmd_StartBattlerSlideIn,
        BattleAnimScriptCmd_StartBattlerSlideOut,
        BattleAnimScriptCmd_JumpIfWeather,
        BattleAnimScriptCmd_JumpIfContest,
        BattleAnimScriptCmd_JumpIfFriendlyFire,
        BattleAnimScriptCmd_InitSpriteManager,
        BattleAnimScriptCmd_LoadCharResObj,
        BattleAnimScriptCmd_LoadPlttRes,
        BattleAnimScriptCmd_LoadCellResObj,
        BattleAnimScriptCmd_LoadAnimResObj,
        BattleAnimScriptCmd_AddSpriteWithFunc,
        BattleAnimScriptCmd_AddSprite,
        BattleAnimScriptCmd_FreeSpriteManager,
        BattleAnimScriptCmd_SetPokemonSpriteVisible,
        BattleAnimScriptCmd_StartPokemonSpriteDrawTask,
        BattleAnimScriptCmd_StopPokemonSpriteDrawTask,
        BattleAnimScriptCmd_WaitForLRX,
    };

    private static uint ReadWord(BattleAnimSystem system)
    {
        return system.ScriptData![system.ScriptPtr];
    }

    private static void Next(BattleAnimSystem system)
    {
        system.ScriptPtr++;
    }

    private static void JumpBy(BattleAnimSystem system, uint offset)
    {
        system.ScriptPtr += (int)offset;
    }

    private static void JumpTo(BattleAnimSystem system, uint address)
    {
        throw Partial(address);
    }

    // OFFSET 0x0221FC20
    private static void BattleAnimScript_WaitForDelay(BattleAnimSystem system)
    {
        if (system.ScriptDelay == 0)
        {
            system.ExecuteAnimScriptFunc = BattleAnimScript_Execute;
        }
        else
        {
            system.ScriptDelay--;
        }
    }

    // OFFSET 0x0221FC44
    private static void BattleAnimScript_Execute(BattleAnimSystem system)
    {
        do
        {
            Action<BattleAnimSystem>? cmd = BattleAnimSystem_GetScriptCmd(system.ScriptData![system.ScriptPtr]);
            cmd!(system);
        } while (system.ScriptDelay == 0 && system.MoveActive == 1);
    }

    // OFFSET 0x0221FC68
    private static SysTask BattleAnimSystem_StartTask(int kind, BattleAnimSystem system, SysTaskFunc func, object? param, uint priority)
    {
        switch (kind)
        {
            case TaskKindEffect:
                system.ActiveAnimTasks++;
                break;
            case TaskKindSound:
                system.ActiveSoundTasks++;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return SysTask.SysTask_Start(func, param, priority);
    }

    // OFFSET 0x0221FCA4
    private static void BattleAnimSystem_EndTask(int kind, BattleAnimSystem system, SysTask task)
    {
        switch (kind)
        {
            case TaskKindEffect:
                system.ActiveAnimTasks--;
                break;
            case TaskKindSound:
                system.ActiveSoundTasks--;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x0221FE30
    public static int BattleAnimSystem_StartMove(BattleAnimSystem system, byte[] moveAnim, ushort move, BattleAnimBattlerContext battlerContext)
    {
        BattleAnimSystem_SetDefaultAlphaBlending();
        if (BattleAnimSystem_IsActive(system) == 0)
        {
            return 0;
        }
        for (int i = 0; i < BattleAnimContext.MaxParticleSystems; i++)
        {
            system.CameraProjections[i] = 1;
            system.ParticleSystemCameraFlip[i] = 0;
        }
        for (int i = 0; i < BattleAnimSystem.ScriptVarCount; i++)
        {
            system.ScriptVars[i] = 0;
        }
        for (int i = 0; i < BattleAnimSystem.LoopStackDepth; i++)
        {
            system.LoopStack[i].IsActive = 0;
            system.LoopStack[i].StartPos = 0;
            system.LoopStack[i].CurIteration = 0;
            system.LoopStack[i].MaxIterations = 0;
        }
        BattleAnimContext context = system.Context!;
        context.Command = moveAnim[0];
        context.Unknown01 = moveAnim[1];
        context.Move = (ushort)BattleIO.R16(moveAnim, 2);
        context.Damage = (int)BattleIO.R32(moveAnim, 4);
        context.Power = (ushort)BattleIO.R16(moveAnim, 8);
        context.Friendship = (ushort)BattleIO.R16(moveAnim, 0xC);
        context.FieldConditions = BattleIO.R32(moveAnim, 0x10);
        context.EffectChance = (ushort)BattleIO.R16(moveAnim, 0xA);
        context.Terrain = (ushort)BattleIO.R32(moveAnim, 0x54);
        context.Attacker = (ushort)BattleIO.R16(moveAnim, 0x14);
        context.Defender = (ushort)BattleIO.R16(moveAnim, 0x16);
        context.IsTransformed = (BattleIO.R16(moveAnim, 0xE) >> 1) & 1;
        context.SpriteSystem = battlerContext.SpriteSystem;
        if (battlerContext.SpriteSystem == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        system.BgConfig = battlerContext.BgConfig;
        system.PaletteData = battlerContext.PaletteData;
        context.BattleType = battlerContext.BattleType;
        for (int i = 0; i < BattleAnimContext.MaxBattlers; i++)
        {
            context.PokemonSpriteData[i] = battlerContext.PokemonSpriteData[i];
            context.BattlerTypes[i] = battlerContext.BattlerTypes[i];
            context.BattlerSprites[i] = battlerContext.BattlerSprites[i];
            context.BattlerSpecies[i] = battlerContext.Species[i];
            context.BattlerGenders[i] = battlerContext.Genders[i];
            context.BattlerShinyFlags[i] = battlerContext.ShinyFlags[i];
            context.BattlerForms[i] = battlerContext.Forms[i];
            context.BattlerPersonalities[i] = battlerContext.Personalities[i];
            context.BattlerMoveEffects[i] = battlerContext.MoveEffects[i];
        }
        system.BattleBgRefs.NarcID = battlerContext.BattleBgRefs.NarcID;
        system.BattleBgRefs.TilesNarcMemberIdx = battlerContext.BattleBgRefs.TilesNarcMemberIdx;
        system.BattleBgRefs.PaletteNarcMemberIdx = battlerContext.BattleBgRefs.PaletteNarcMemberIdx;
        system.BattleBgRefs.TilemapNarcMemberIdx = battlerContext.BattleBgRefs.TilemapNarcMemberIdx;
        system.BattleBgRefs.PaletteDestStart = battlerContext.BattleBgRefs.PaletteDestStart;
        system.BattleBgRefs.PaletteSrcSize = battlerContext.BattleBgRefs.PaletteSrcSize;
        context.ChatotCry = (ChatotCry?)battlerContext.ChatotCry;
        context.BgPaletteBuffer = battlerContext.BgPaletteBuffer;
        context.BgTiles = battlerContext.BgTiles;
        int moveID = move;
        if (moveID == MoveSecretPower)
        {
            uint[] terrainMoveTable = RomData.OverlayU32(Overlay, SecretPowerTableAddress, SecretPowerTableCount);
            if (context.Terrain >= TerrainMax || context.Terrain >= SecretPowerTableCount)
            {
                moveID = MoveTriAttack;
            }
            else
            {
                moveID = (int)terrainMoveTable[context.Terrain];
            }
        }
        if (moveID == 0 || moveID > NumValidMoves)
        {
            moveID = MovePound;
        }
        system.MoveArcID = battlerContext.MoveArcID;
        byte[]? data = NARC.NARC_AllocAndReadWholeMemberByIndexPair(system.MoveArcID, moveID, system.HeapID);
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
            return 0;
        }
        system.ScriptData = new uint[data.Length / 4];
        for (int i = 0; i < system.ScriptData.Length; i++)
        {
            system.ScriptData[i] = Nitro.Bin.U32(data, i * 4);
        }
        system.ScriptPtr = 0;
        for (int i = 0; i < BattleAnimSystem.BgLayerCount; i++)
        {
            system.BgLayerPriorities[i] = Bg.Bg_GetPriority(system.BgConfig!, i);
        }
        for (int i = 0; i < BattleAnimSystem.SpriteCount; i++)
        {
            system.Sprites[i] = null;
        }
        for (int i = 0; i < BattleAnimSystem.PokemonSpriteCount; i++)
        {
            system.PokemonSprites[i] = null;
            system.UsedPokemonSprites[i] = 0;
        }
        system.BgAnim = null;
        system.ExecuteAnimScriptFunc = BattleAnimScript_Execute;
        system.ScriptDelay = 0;
        if (BattleAnimSystem_IsContest(system) == 1)
        {
            system.BaseBgPalettes = ExtraBgDefault;
        }
        else
        {
            system.BaseBgPalettes = BaseBgPalettesBattle;
        }
        system.MoveActive = 1;
        return 1;
    }

    // OFFSET 0x0222016C
    public static int BattleAnimSystem_ExecuteScript(BattleAnimSystem system)
    {
        if (BattleAnimSystem_IsActive(system) == 0)
        {
            return 0;
        }
        system.ExecuteAnimScriptFunc!(system);
        return 1;
    }

    // OFFSET 0x02220188
    public static int BattleAnimSystem_IsMoveActive(BattleAnimSystem system)
    {
        return system.MoveActive == 1 ? 1 : 0;
    }

    // OFFSET 0x02220198
    public static int BattleAnimSystem_FreeScriptData(BattleAnimSystem system)
    {
        if (system.ScriptData == null)
        {
            ErrorHandling.GF_AssertFail();
            return 0;
        }
        system.ScriptData = null;
        return 1;
    }

    // OFFSET 0x022201CC
    public static SysTask BattleAnimSystem_StartAnimTaskEx(BattleAnimSystem system, SysTaskFunc func, object? param, uint priority)
    {
        return BattleAnimSystem_StartTask(TaskKindEffect, system, func, param, priority);
    }

    // OFFSET 0x022201E8
    public static SysTask BattleAnimSystem_StartAnimTask(BattleAnimSystem system, SysTaskFunc func, object? param)
    {
        return BattleAnimSystem_StartTask(TaskKindEffect, system, func, param, AnimTaskPriority);
    }

    // OFFSET 0x02220204
    public static SysTask BattleAnimSystem_StartSoundTask(BattleAnimSystem system, SysTaskFunc func, object? param, uint priority)
    {
        return BattleAnimSystem_StartTask(TaskKindSound, system, func, param, priority);
    }

    // OFFSET 0x02220220
    public static void BattleAnimSystem_EndAnimTask(BattleAnimSystem system, SysTask task)
    {
        BattleAnimSystem_EndTask(TaskKindEffect, system, task);
    }

    // OFFSET 0x02220230
    public static void BattleAnimSystem_EndSoundTask(BattleAnimSystem system, SysTask task)
    {
        BattleAnimSystem_EndTask(TaskKindSound, system, task);
    }

    // OFFSET 0x02220240
    public static ushort BattleAnimSystem_GetAttacker(BattleAnimSystem system)
    {
        return system.Context!.Attacker;
    }

    // OFFSET 0x02220248
    public static ushort BattleAnimSystem_GetDefender(BattleAnimSystem system)
    {
        return system.Context!.Defender;
    }

    // OFFSET 0x02220250
    public static ParticleSystem? BattleAnimSystem_GetCurrentParticleSystem(BattleAnimSystem system)
    {
        return system.Context!.ParticleSystems[system.Context.CurrentParticleSystem];
    }

    // OFFSET 0x02220260
    public static ParticleSystem? BattleAnimSystem_GetParticleSystem(BattleAnimSystem system, int index)
    {
        return system.Context!.ParticleSystems[index];
    }

    // OFFSET 0x0222026C
    public static SplEmitter? BattleAnimSystem_GetEmitter(BattleAnimSystem system, int index)
    {
        return system.Context!.Emitters[index];
    }

    // OFFSET 0x02220278
    public static BgConfig BattleAnimSystem_GetBgConfig(BattleAnimSystem system)
    {
        return system.BgConfig!;
    }

    // OFFSET 0x02220280
    public static int BattleAnimSystem_GetScriptVar(BattleAnimSystem system, int id)
    {
        if (id >= BattleAnimSystem.ScriptVarCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return system.ScriptVars[id];
    }

    // OFFSET 0x02220298
    public static ManagedSprite BattleAnimSystem_GetSprite(BattleAnimSystem system, int index)
    {
        if (index >= BattleAnimSystem.SpriteCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (system == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (system!.Sprites[index] == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return system.Sprites[index]!;
    }

    // OFFSET 0x022202C0
    public static ManagedSprite BattleAnimSystem_GetPokemonSprite(BattleAnimSystem system, int index)
    {
        if (index >= BattleAnimSystem.PokemonSpriteCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (system == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (system!.PokemonSprites[index] == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return system.PokemonSprites[index]!;
    }

    // OFFSET 0x022202EC
    public static SpriteManager BattleAnimSystem_GetPokemonSpriteManager(BattleAnimSystem system)
    {
        if (system == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return system!.PokemonSpriteManager!;
    }

    // OFFSET 0x02220300
    public static SpriteManager BattleAnimSystem_GetPrimarySpriteManager(BattleAnimSystem system)
    {
        return system.SpriteManagers[0]!;
    }

    // OFFSET 0x02220308
    public static SpriteSystem BattleAnimSystem_GetSpriteSystem(BattleAnimSystem system)
    {
        return system.Context!.SpriteSystem!;
    }

    // OFFSET 0x02220474
    public static void BattleAnimSystem_SetDefaultAlphaBlending()
    {
        Video.SetBlendAlpha(0, BlendPlaneNone, BlendPlaneAll, 8, 8);
    }

    // OFFSET 0x0222048C
    private static void BattleAnimScriptCmd_WaitForLRX(BattleAnimSystem system)
    {
        system.ScriptDelay = 1;
        if ((Globals.gSystem.HeldKeys & 0x200) != 0 && (Globals.gSystem.HeldKeys & 0x100) != 0 && (Globals.gSystem.PressedKeys & 0x400) != 0)
        {
            Next(system);
            system.ScriptDelay = 0;
        }
    }

    // OFFSET 0x022204C4
    private static void BattleAnimScriptCmd_Delay(BattleAnimSystem system)
    {
        Next(system);
        system.ScriptDelay = (byte)ReadWord(system);
        Next(system);
        system.ExecuteAnimScriptFunc = BattleAnimScript_WaitForDelay;
    }

    // OFFSET 0x022204E4
    private static void BattleAnimScriptCmd_WaitForAnimTasks(BattleAnimSystem system)
    {
        if (system.ActiveAnimTasks == 0)
        {
            Next(system);
            system.ScriptDelay = 0;
        }
        else
        {
            system.ScriptDelay = 1;
        }
    }

    // OFFSET 0x02220504
    private static void BattleAnimScriptCmd_SetVar(BattleAnimSystem system)
    {
        Next(system);
        uint id = ReadWord(system);
        Next(system);
        uint value = ReadWord(system);
        Next(system);
        if (id < BattleAnimSystem.ScriptVarCount)
        {
            system.ScriptVars[id] = (int)value;
        }
    }

    // OFFSET 0x02220524
    private static void BattleAnimScriptCmd_ResetVars(BattleAnimSystem system)
    {
        Next(system);
        for (int i = 0; i < BattleAnimSystem.ScriptVarCount; i++)
        {
            system.ScriptVars[i] = 0;
        }
    }

    // OFFSET 0x02220540
    public static int BattleAnimSystem_GetMoveInfo(BattleAnimSystem system, int info)
    {
        BattleAnimContext context = system.Context!;
        switch (info)
        {
            case 0:
                return context.Damage;
            case 1:
                return context.Power;
            case 2:
                return context.Friendship;
            case 3:
                return (int)context.FieldConditions;
            case 4:
                return context.EffectChance;
            case 5:
                return context.Terrain;
            default:
                return 0;
        }
    }

    // OFFSET 0x02220590
    public static void BattlerSpriteContext_InitFromSystem(BattleAnimSystem system, BattlerSpriteContext battlerSpriteCtx, int animParticipantMode)
    {
        BattleAnimContext context = system.Context!;
        for (int battler = 0; battler < BattleAnimContext.MaxBattlers; battler++)
        {
            battlerSpriteCtx.PokemonSpriteData[battler] = context.PokemonSpriteData[battler];
            battlerSpriteCtx.Sprites[battler] = context.BattlerSprites[battler];
            battlerSpriteCtx.Species[battler] = context.BattlerSpecies[battler];
            battlerSpriteCtx.Genders[battler] = context.BattlerGenders[battler];
            battlerSpriteCtx.ShinyFlags[battler] = context.BattlerShinyFlags[battler];
            battlerSpriteCtx.Forms[battler] = context.BattlerForms[battler];
            battlerSpriteCtx.Personalities[battler] = context.BattlerPersonalities[battler];
            battlerSpriteCtx.Types[battler] = context.BattlerTypes[battler];
        }
        switch (animParticipantMode)
        {
            case 0:
                battlerSpriteCtx.TargetBattler = context.Attacker;
                battlerSpriteCtx.SourceBattler = context.Defender;
                break;
            case 1:
            case 2:
            case 3:
                battlerSpriteCtx.TargetBattler = context.Attacker;
                battlerSpriteCtx.SourceBattler = context.Attacker;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
    }

    // OFFSET 0x022206A4
    private static void BattleAnimScriptCmd_StartBattlerSlideIn(BattleAnimSystem system)
    {
        Next(system);
        int animParticipantMode = (int)ReadWord(system);
        Next(system);
        BattlerSpriteContext battlerSpriteCtx = new();
        BattlerSpriteContext_InitFromSystem(system, battlerSpriteCtx, animParticipantMode);
        if (animParticipantMode == 2)
        {
            BattlerSlide.BattlerSlide_StartPartnerSlideIn(battlerSpriteCtx, system.HeapID);
        }
        else if (animParticipantMode == 3)
        {
            BattlerSlide.BattlerSlide_StartAllySlideIn(battlerSpriteCtx, system.HeapID);
        }
        else
        {
            BattlerSlide.BattlerSlide_StartSlideIn(battlerSpriteCtx, system.HeapID);
        }
    }

    // OFFSET 0x022206E8
    private static void BattleAnimScriptCmd_StartBattlerSlideOut(BattleAnimSystem system)
    {
        Next(system);
        int animParticipantMode = (int)ReadWord(system);
        Next(system);
        BattlerSpriteContext battlerSpriteCtx = new();
        BattlerSpriteContext_InitFromSystem(system, battlerSpriteCtx, animParticipantMode);
        BattlerSlide.BattlerSlide_StartSlideOut(battlerSpriteCtx, system.HeapID);
    }

    // OFFSET 0x0222070C
    private static void BattleAnimScriptCmd_BeginLoop(BattleAnimSystem system)
    {
        Next(system);
        for (int i = 0; i < BattleAnimSystem.LoopStackDepth; i++)
        {
            if (system.LoopStack[i].IsActive == 1)
            {
                continue;
            }
            system.LoopStack[i].IsActive = 1;
            system.LoopStack[i].CurIteration = 0;
            system.LoopStack[i].MaxIterations = (byte)ReadWord(system);
            Next(system);
            system.LoopStack[i].StartPos = system.ScriptPtr;
            return;
        }
    }

    // OFFSET 0x0222074C
    private static void BattleAnimScriptCmd_EndLoop(BattleAnimSystem system)
    {
        Next(system);
        for (int i = BattleAnimSystem.LoopStackDepth - 1; i >= 0; i--)
        {
            if (system.LoopStack[i].IsActive == 0)
            {
                continue;
            }
            system.LoopStack[i].CurIteration++;
            if (system.LoopStack[i].CurIteration == system.LoopStack[i].MaxIterations)
            {
                system.LoopStack[i].IsActive = 0;
            }
            else
            {
                system.ScriptPtr = system.LoopStack[i].StartPos;
            }
            return;
        }
    }

    // OFFSET 0x02220798
    private static void BattleAnimScriptCmd_End(BattleAnimSystem system)
    {
        BattleAnimContext context = system.Context!;
        if (system.EndDelayFrames < 1)
        {
            system.ScriptDelay = 1;
            system.EndDelayFrames++;
            return;
        }
        int totalEmitters = 0;
        for (int i = 0; i < BattleAnimContext.MaxParticleSystems; i++)
        {
            if (context.ParticleSystems[i] != null)
            {
                totalEmitters += ParticleSystem.ParticleSystem_GetActiveEmitterCount(context.ParticleSystems[i]!);
            }
        }
        if (totalEmitters != 0 || system.ActiveAnimTasks != 0 || system.ActiveSoundTasks != 0)
        {
            system.ScriptDelay = 1;
            system.SoundEffectWaitTimer = 0;
            return;
        }
        if (SoundPlayback.Sound_IsAnyEffectPlaying())
        {
            system.SoundEffectWaitTimer++;
            if (system.SoundEffectWaitTimer > MaxSfxWaitFrames)
            {
                system.SoundEffectWaitTimer = 0;
                system.ScriptDelay = 0;
            }
            else
            {
                system.ScriptDelay = 1;
                return;
            }
        }
        system.SoundEffectWaitTimer = 0;
        system.EndDelayFrames = 0;
        for (int i = 0; i < BattleAnimSystem.CallStackDepth; i++)
        {
            system.CallStack[i] = 0;
        }
        for (int i = 0; i < BattleAnimSystem.LoopStackDepth; i++)
        {
            system.LoopStack[i].StartPos = 0;
            system.LoopStack[i].CurIteration = 0;
            system.LoopStack[i].MaxIterations = 0;
            system.LoopStack[i].IsActive = 0;
        }
        for (int i = 0; i < BattleAnimSystem.SpriteManagerCount; i++)
        {
            if (system.SpriteManagers[i] != null)
            {
                SpriteSystem.SpriteSystem_FreeResourcesAndManager(context.SpriteSystem!, system.SpriteManagers[i]!);
            }
            system.SpriteManagers[i] = null;
        }
        for (int i = 0; i < TrackingTaskCount; i++)
        {
            BattleAnimSystem_CancelTrackingTask(system, i);
        }
        for (int i = 0; i < BattleAnimContext.MaxParticleSystems; i++)
        {
            if (context.ParticleSystems[i] != null)
            {
                BattleParticlePlayers.BattleParticleUtil_Free(context.ParticleSystems[i]!);
                context.ParticleSystems[i] = null;
            }
        }
        if (system.BgAnim != null)
        {
            system.BgAnim = null;
        }
        if (BattleAnimSystem_IsContest(system) == 0)
        {
            Battle.Battle_ClearBlendControl();
            Bg.Bg_ClearTilesRange((byte)BattleAnimSystem_GetBgLayer(system, 1), 0x4000, 0, BattleAnimSystem_GetHeapID(system));
            Bg.Bg_ClearTilemap(BattleAnimSystem_GetBgConfig(system), BattleAnimSystem_GetBgLayer(system, 1));
            Bg.Bg_ToggleLayer(BattleBgBase, 1);
        }
        else
        {
            throw Partial(0x022413D8);
        }
        for (int i = 0; i < BattleAnimSystem.BgLayerCount; i++)
        {
            Bg.Bg_SetPriority((byte)i, system.BgLayerPriorities[i]);
        }
        Bg.Bg_SetOffset(system.BgConfig!, BattleBgBase, 0, 0);
        Bg.Bg_SetOffset(system.BgConfig!, BattleBgBase, 3, 0);
        Bg.Bg_SetOffset(system.BgConfig!, BattleBgEffect, 0, 0);
        Bg.Bg_SetOffset(system.BgConfig!, BattleBgEffect, 3, 0);
        system.MoveActive = 0;
    }

    private static void SetEmitterCamera(BattleAnimSystem system, int psIndex)
    {
        ParticleSystem particleSystem = system.Context!.ParticleSystems[psIndex]!;
        if (system.ParticleSystemCameraFlip[psIndex] != 0)
        {
            ParticleSystem.ParticleSystem_GetCameraUp(particleSystem, out VecFx32 up);
            up.Y *= -1;
            ParticleSystem.ParticleSystem_SetCameraUp(particleSystem, in up);
        }
    }

    // OFFSET 0x022209A8
    private static void BattleAnimScriptCmd_CreateEmitter(BattleAnimSystem system)
    {
        Next(system);
        int psIndex = (int)ReadWord(system);
        Next(system);
        int resourceID = (int)ReadWord(system);
        Next(system);
        int callbackID = (int)ReadWord(system);
        Next(system);
        system.Context!.CurrentParticleSystem = psIndex;
        SetEmitterCamera(system, psIndex);
        ParticleSystem.ParticleSystem_SetCameraProjection(system.Context.ParticleSystems[psIndex]!, system.CameraProjections[psIndex]);
        system.Context.Emitters[0] = BattleParticleUtil.BattleParticleUtil_CreateEmitter(system.Context.ParticleSystems[psIndex]!, resourceID, callbackID, system);
    }

    // OFFSET 0x02220A3C
    private static void BattleAnimScriptCmd_CreateEmitterEx(BattleAnimSystem system)
    {
        Next(system);
        int psIndex = (int)ReadWord(system);
        Next(system);
        int emitterIndex = (int)ReadWord(system);
        Next(system);
        int resourceID = (int)ReadWord(system);
        Next(system);
        int callbackID = (int)ReadWord(system);
        Next(system);
        system.Context!.CurrentParticleSystem = psIndex;
        SetEmitterCamera(system, psIndex);
        ParticleSystem.ParticleSystem_SetCameraProjection(system.Context.ParticleSystems[psIndex]!, system.CameraProjections[psIndex]);
        system.Context.Emitters[emitterIndex] = BattleParticleUtil.BattleParticleUtil_CreateEmitter(system.Context.ParticleSystems[psIndex]!, resourceID, callbackID, system);
    }

    // OFFSET 0x02220ADC
    private static int BattleAnimSystem_GetParticleResIdxForMove(BattleAnimSystem system)
    {
        uint[] indexTable = RomData.OverlayU32(Overlay, ParticleResIdxForMoveAddress, 36);
        int attacker = BattleAnimSystem_GetAttacker(system);
        int defender = BattleAnimSystem_GetDefender(system);
        int attackerType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, attacker);
        int defenderType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, defender);
        int index = (int)indexTable[attackerType * 6 + defenderType];
        if (index == 0xFF)
        {
            ErrorHandling.GF_AssertFail();
        }
        return index - 1;
    }

    // OFFSET 0x02220B34
    private static int BattleAnimSystem_GetParticleResIdxForFriendlyFire(BattleAnimSystem system)
    {
        uint[] indexTable = RomData.OverlayU32(Overlay, ParticleResIdxForFriendlyFireAddress, 36);
        int attacker = BattleAnimSystem_GetAttacker(system);
        int defender = BattleAnimSystem_GetDefender(system);
        int attackerType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, attacker);
        int defenderType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, defender);
        int index = (int)indexTable[attackerType * 6 + defenderType];
        if (index == 0xFF)
        {
            ErrorHandling.GF_AssertFail();
        }
        return index;
    }

    // OFFSET 0x02220B8C
    private static void BattleAnimScriptCmd_CreateEmitterForMove(BattleAnimSystem system)
    {
        uint[] resourceTable = new uint[ParticleResourceCount];
        Next(system);
        int psIndex = (int)ReadWord(system);
        Next(system);
        for (int i = 0; i < ParticleResourceCount; i++)
        {
            resourceTable[i] = ReadWord(system);
            Next(system);
        }
        int callbackID = (int)ReadWord(system);
        Next(system);
        system.Context!.CurrentParticleSystem = psIndex;
        SetEmitterCamera(system, psIndex);
        int resIdx = BattleAnimSystem_GetParticleResIdxForMove(system);
        ParticleSystem.ParticleSystem_SetCameraProjection(system.Context.ParticleSystems[psIndex]!, system.CameraProjections[psIndex]);
        system.Context.Emitters[0] = BattleParticleUtil.BattleParticleUtil_CreateEmitter(system.Context.ParticleSystems[psIndex]!, (int)resourceTable[resIdx], callbackID, system);
    }

    // OFFSET 0x02220C44
    private static void BattleAnimScriptCmd_CreateEmitterForFriendlyFire(BattleAnimSystem system)
    {
        uint[] resourceTable = new uint[4];
        Next(system);
        int psIndex = (int)ReadWord(system);
        Next(system);
        for (int i = 0; i < 4; i++)
        {
            resourceTable[i] = ReadWord(system);
            Next(system);
        }
        int callbackID = (int)ReadWord(system);
        Next(system);
        system.Context!.CurrentParticleSystem = psIndex;
        SetEmitterCamera(system, psIndex);
        int resIdx = BattleAnimSystem_GetParticleResIdxForFriendlyFire(system);
        ParticleSystem.ParticleSystem_SetCameraProjection(system.Context.ParticleSystems[psIndex]!, system.CameraProjections[psIndex]);
        system.Context.Emitters[0] = BattleParticleUtil.BattleParticleUtil_CreateEmitter(system.Context.ParticleSystems[psIndex]!, (int)resourceTable[resIdx], callbackID, system);
    }

    // OFFSET 0x02220CFC
    private static void BattleAnimScriptCmd_WaitForAllEmitters(BattleAnimSystem system)
    {
        int activeEmitters = 0;
        for (int i = 0; i < BattleAnimContext.MaxParticleSystems; i++)
        {
            if (system.Context!.ParticleSystems[i] == null)
            {
                continue;
            }
            activeEmitters += ParticleSystem.ParticleSystem_GetActiveEmitterCount(system.Context.ParticleSystems[i]!);
        }
        if (activeEmitters == 0)
        {
            Next(system);
            system.ScriptDelay = 0;
        }
        else
        {
            system.ScriptDelay = 1;
        }
    }

    // OFFSET 0x02220D3C
    private static void BattleAnimScriptCmd_LoadParticleSystem(BattleAnimSystem system)
    {
        Next(system);
        int psIndex = (int)ReadWord(system);
        Next(system);
        if (system.Context!.ParticleSystems[psIndex] != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        int memberIndex = (int)ReadWord(system);
        Next(system);
        system.Context.ParticleSystems[psIndex] = BattleParticlePlayers.BattleParticleUtil_CreateFromMember(system.HeapID, memberIndex, 0);
        system.ScriptDelay = 2;
        system.ExecuteAnimScriptFunc = BattleAnimScript_WaitForDelay;
    }

    // OFFSET 0x02220D90
    private static void BattleAnimScriptCmd_LoadDebugParticleSystem(BattleAnimSystem system)
    {
        Next(system);
        int psIndex = (int)ReadWord(system);
        Next(system);
        Next(system);
        if (system.Context!.ParticleSystems[psIndex] != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        int memberIndex = (int)ReadWord(system);
        Next(system);
        system.Context.ParticleSystems[psIndex] = BattleParticlePlayers.BattleParticleUtil_CreateFromNarc(system.HeapID, DebugParticleNarc, memberIndex, 0);
        system.ScriptDelay = 2;
        system.ExecuteAnimScriptFunc = BattleAnimScript_WaitForDelay;
    }

    // OFFSET 0x02220DE8
    private static void BattleAnimScriptCmd_UnloadParticleSystem(BattleAnimSystem system)
    {
        Next(system);
        int psIndex = (int)ReadWord(system);
        Next(system);
        BattleParticlePlayers.BattleParticleUtil_Free(system.Context!.ParticleSystems[psIndex]!);
        system.Context.ParticleSystems[psIndex] = null;
    }

    // OFFSET 0x02220E14
    private static void BattleAnimScriptCmd_Call(BattleAnimSystem system)
    {
        Next(system);
        for (int i = 0; i < BattleAnimSystem.CallStackDepth; i++)
        {
            if (system.CallStack[i] != 0)
            {
                continue;
            }
            system.CallStack[i] = system.ScriptPtr + 1;
            JumpBy(system, ReadWord(system));
            return;
        }
    }

    // OFFSET 0x02220E44
    private static void BattleAnimScriptCmd_Return(BattleAnimSystem system)
    {
        Next(system);
        for (int i = BattleAnimSystem.CallStackDepth - 1; i >= 0; i--)
        {
            if (system.CallStack[i] == 0)
            {
                continue;
            }
            system.ScriptPtr = system.CallStack[i];
            system.CallStack[i] = 0;
            return;
        }
    }

    // OFFSET 0x02220E70
    private static void BattleAnimScriptCmd_JumpIfEqual(BattleAnimSystem system)
    {
        Next(system);
        uint id = ReadWord(system);
        Next(system);
        uint value = ReadWord(system);
        Next(system);
        if (value == (uint)system.ScriptVars[id])
        {
            JumpTo(system, ReadWord(system));
        }
        else
        {
            Next(system);
        }
    }

    // OFFSET 0x02220EA8
    private static void BattleAnimScriptCmd_JumpIfUnk01(BattleAnimSystem system)
    {
        Next(system);
        if (BattleAnimContext_GetUnk01(system.Context!) != 0)
        {
            JumpTo(system, ReadWord(system));
        }
        else
        {
            Next(system);
        }
    }

    // OFFSET 0x02220ED0
    private static void BattleAnimScriptCmd_CallFunc(BattleAnimSystem system)
    {
        Next(system);
        uint funcID = ReadWord(system);
        Next(system);
        Action<BattleAnimSystem>? func = BattleAnimScriptFuncs.BattleAnimScript_GetFunc(funcID);
        uint argCount = ReadWord(system);
        Next(system);
        int i;
        for (i = 0; i < argCount; i++)
        {
            system.ScriptVars[i] = (int)ReadWord(system);
            Next(system);
        }
        for (; i < BattleAnimSystem.ScriptVarCount; i++)
        {
            system.ScriptVars[i] = 0;
        }
        func!(system);
    }

    // OFFSET 0x02220F30
    private static void BattleAnimScriptCmd_JumpIfEffectChanceOdd(BattleAnimSystem system)
    {
        Next(system);
        if ((system.Context!.EffectChance & 1) != 0)
        {
            Next(system);
        }
        JumpBy(system, ReadWord(system));
    }

    // OFFSET 0x02220F5C
    private static void BattleAnimScriptCmd_JumpIfBattlerSide(BattleAnimSystem system)
    {
        Next(system);
        int battler = (int)ReadWord(system);
        Next(system);
        int side;
        if (battler == 0)
        {
            side = BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, system.Context!.Attacker);
        }
        else
        {
            side = BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, system.Context!.Defender);
        }
        if (side == BattlerSideEnemy)
        {
            Next(system);
        }
        JumpBy(system, ReadWord(system));
    }

    // OFFSET 0x02220FA0
    private static void BattleAnimScriptCmd_JumpIfWeather(BattleAnimSystem system)
    {
        uint[] weatherConditions = RomData.OverlayU32(Overlay, WeatherConditionsAddress, 4);
        Next(system);
        uint fieldConditions = system.Context!.FieldConditions;
        if (fieldConditions != 0)
        {
            Next(system);
            for (int i = 0; i < 4; i++)
            {
                if ((fieldConditions & weatherConditions[i]) != 0)
                {
                    break;
                }
                Next(system);
            }
        }
        JumpBy(system, ReadWord(system));
    }

    // OFFSET 0x02220FFC
    private static void BattleAnimScriptCmd_JumpIfContest(BattleAnimSystem system)
    {
        Next(system);
        if (BattleAnimSystem_IsContest(system) == 1)
        {
            JumpBy(system, ReadWord(system));
        }
        else
        {
            Next(system);
        }
    }

    // OFFSET 0x02221024
    private static void BattleAnimScriptCmd_JumpIfFriendlyFire(BattleAnimSystem system)
    {
        Next(system);
        int attackerSide = BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, system.Context!.Attacker);
        int defenderSide = BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, system.Context.Defender);
        if (attackerSide == defenderSide)
        {
            JumpBy(system, ReadWord(system));
        }
        else
        {
            Next(system);
        }
    }

    // OFFSET 0x02221064
    private static void BattleAnimScriptCmd_JumpIfEffectChance(BattleAnimSystem system)
    {
        Next(system);
        int effectChance = (int)ReadWord(system);
        Next(system);
        if (effectChance == system.Context!.EffectChance)
        {
            JumpBy(system, ReadWord(system));
        }
        else
        {
            Next(system);
        }
    }

    // OFFSET 0x02221098
    private static void BattleAnimScriptCmd_Jump(BattleAnimSystem system)
    {
        Next(system);
        JumpBy(system, ReadWord(system));
    }

    private static int FindBattlerOfTypes(BattleAnimSystem system, int typeA, int typeB)
    {
        int result = 0xFF;
        for (int battler = 0; battler < 4; battler++)
        {
            int battlerType = BattleAnimSystem_GetBattlerType(system, battler);
            if (battlerType == typeA || battlerType == typeB)
            {
                result = battler;
                break;
            }
        }
        if (result == 0xFF)
        {
            result = 0;
        }
        return result;
    }

    // OFFSET 0x022210A8
    private static int BattleAnimSystem_GetBattlerWithRole(BattleAnimSystem system, int role)
    {
        int result = 0;
        switch (role)
        {
            case RoleAttacker:
                result = system.Context!.Attacker;
                break;
            case RoleDefender:
                result = system.Context!.Defender;
                break;
            case RoleAttackerPartner:
                result = BattleAnimUtil.BattleAnimUtil_GetAlliedBattler(system, system.Context!.Attacker);
                break;
            case RoleDefenderPartner:
                result = BattleAnimUtil.BattleAnimUtil_GetAlliedBattler(system, system.Context!.Defender);
                break;
            case RolePlayer1:
                result = FindBattlerOfTypes(system, 0, 2);
                break;
            case RoleEnemy1:
                result = FindBattlerOfTypes(system, 1, 3);
                break;
            case RolePlayer2:
                result = FindBattlerOfTypes(system, 4, 4);
                break;
            case RoleEnemy2:
                result = FindBattlerOfTypes(system, 5, 5);
                break;
        }
        return result;
    }

    private static short TrackedY(PokemonSprite sprite)
    {
        short y = (short)PokemonSprite.PokemonSprite_GetAttribute(sprite, MonSpriteYCenter);
        return (short)(y - PokemonSprite.PokemonSprite_GetAttribute(sprite, MonSpriteShadowHeight));
    }

    // OFFSET 0x0222118C
    private static void BattleAnimSystem_SpriteTrackingTaskFunc(SysTask task, object? param)
    {
        SpriteTrackingTask trackingTask = (SpriteTrackingTask)param!;
        if (trackingTask.Data.Interval != 0)
        {
            trackingTask.Data.FrameCount++;
            if (trackingTask.Data.FrameCount != trackingTask.Data.Interval)
            {
                return;
            }
            trackingTask.Data.FrameCount = 0;
        }
        short x = (short)PokemonSprite.PokemonSprite_GetAttribute(trackingTask.Data.Sprite!, MonSpriteXCenter);
        short y = TrackedY(trackingTask.Data.Sprite!);
        SpriteSystem.ManagedSprite_SetPositionXY(trackingTask.Sprite!, x, y);
    }

    // OFFSET 0x022211D8
    private static void BattleAnimSystem_BgTrackingTaskFunc(SysTask task, object? param)
    {
        BgTrackingTask trackingTask = (BgTrackingTask)param!;
        if (trackingTask.Data.Interval != 0)
        {
            trackingTask.Data.FrameCount++;
            if (trackingTask.Data.FrameCount != trackingTask.Data.Interval)
            {
                return;
            }
            trackingTask.Data.FrameCount = 0;
        }
        short x = (short)PokemonSprite.PokemonSprite_GetAttribute(trackingTask.Data.Sprite!, MonSpriteXCenter);
        short y = TrackedY(trackingTask.Data.Sprite!);
        Bg.Bg_SetOffset(trackingTask.Bg!, BattleBgBase, 0, -(short)(x - MonSpriteFrameHalf));
        Bg.Bg_SetOffset(trackingTask.Bg!, BattleBgBase, 3, -(short)(y - MonSpriteFrameHalf));
    }

    // OFFSET 0x02221238
    public static void BattleAnimSystem_CancelTrackingTask(BattleAnimSystem system, int task)
    {
        if (task == TrackingTaskBg)
        {
            if (system.BgTrackingTask != null)
            {
                SysTask.SysTask_Done(system.BgTrackingTask.Task!);
                system.BgTrackingTask = null;
            }
        }
        else if (system.SpriteTrackingTasks[task] != null)
        {
            SysTask.SysTask_Done(system.SpriteTrackingTasks[task]!.Task!);
            system.SpriteTrackingTasks[task] = null;
        }
    }

    // OFFSET 0x02221284
    private static void BattleAnimScriptCmd_Nop0(BattleAnimSystem system)
    {
    }

    // OFFSET 0x02221288
    private static void BattleAnimScriptCmd_Nop1(BattleAnimSystem system)
    {
    }

    // OFFSET 0x0222128C
    private static void BattleAnimScriptCmd_LoadPokemonSpriteIntoBg(BattleAnimSystem system)
    {
        Next(system);
        int battlerRole = (int)ReadWord(system);
        Next(system);
        int trackBattler = (int)ReadWord(system);
        Next(system);
        int battler = BattleAnimSystem_GetBattlerWithRole(system, battlerRole);
        PokemonSpriteData spriteData = system.Context!.PokemonSpriteData[battler]!;
        int narcID = spriteData.NarcID;
        int paletteIndex = spriteData.Palette;
        byte[] charData = spriteData.Tiles!;
        int form = system.Context.BattlerForms[battler];
        int memberIndex;
        if (BattleAnimSystem_IsContest(system) == 1 && IsFormSymmetrical(BattleAnimSystem_GetBattlerSpecies(system, battler), form) == 1)
        {
            memberIndex = MonBgTilemapSymmetric;
        }
        else
        {
            memberIndex = MonBgTilemapDefault;
        }
        byte[] bgTiles = Bg.Bg_GetCharPtr(BattleBgBase);
        Array.Clear(bgTiles, 0, MonSpriteBgClearBytes);
        Bg.Bg_ToggleLayer(BattleBgBase, 0);
        Bg.Bg_LoadTiles(system.BgConfig!, BattleBgBase, charData, MonSpriteTileBytes, 0);
        PaletteData.PaletteData_LoadBufferFromFileStart(system.PaletteData!, narcID, paletteIndex, system.HeapID, PlttBufMainBg, 0, 0x80);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(system.Arcs[ArcBattBg]!, (uint)memberIndex, system.BgConfig!, BattleBgBase, 0, 0, 0, system.HeapID);
        if (trackBattler == 1)
        {
            BgTrackingTask trackingTask = new();
            system.BgTrackingTask = trackingTask;
            trackingTask.Bg = system.BgConfig;
            trackingTask.Data.Sprite = BattleAnimSystem_GetBattlerSprite(system, battler);
            trackingTask.Data.FrameCount = 0;
            trackingTask.Data.Interval = 0;
            trackingTask.Task = SysTask.SysTask_Start(BattleAnimSystem_BgTrackingTaskFunc, trackingTask, TrackingTaskPriority);
        }
        PokemonSprite battlerSprite = BattleAnimSystem_GetBattlerSprite(system, battler)!;
        short x = (short)PokemonSprite.PokemonSprite_GetAttribute(battlerSprite, MonSpriteXCenter);
        short y = TrackedY(battlerSprite);
        Bg.Bg_SetOffset(system.BgConfig!, BattleBgBase, 0, -(short)(x - MonSpriteFrameHalf));
        Bg.Bg_SetOffset(system.BgConfig!, BattleBgBase, 3, -(short)(y - MonSpriteFrameHalf));
        Bg.Bg_ToggleLayer(BattleBgBase, 1);
        Bg.Bg_SetPriority(BattleBgBase, (byte)BattleAnimSystem_GetPokemonSpritePriority(system));
    }

    // OFFSET 0x02221424
    private static void BattleAnimScriptCmd_RemovePokemonSpriteFromBg(BattleAnimSystem system)
    {
        Next(system);
        Next(system);
        byte[] bgTiles = Bg.Bg_GetCharPtr(BattleBgBase);
        Array.Clear(bgTiles, 0, MonSpriteBgClearBytes);
        BattleAnimSystem_CancelTrackingTask(system, TrackingTaskBg);
    }

    // OFFSET 0x0222144C
    private static void BattleAnimScriptCmd_InitPokemonSpriteManager(BattleAnimSystem system)
    {
        int[] caps = { BattleAnimSystem.PokemonSpriteCount, BattleAnimSystem.PokemonSpriteCount, BattleAnimSystem.PokemonSpriteCount, BattleAnimSystem.PokemonSpriteCount, 0, 0 };
        Next(system);
        system.PokemonSpriteManager = SpriteSystem.SpriteManager_New(system.Context!.SpriteSystem);
        SpriteSystem.SpriteSystem_InitSprites(system.Context.SpriteSystem, system.PokemonSpriteManager, BattleAnimSystem.PokemonSpriteCount);
        SpriteUtil.SetSubScreenViewRect(SpriteSystem.SpriteSystem_GetRenderer(system.Context.SpriteSystem!), 0, 0x110000);
        SpriteSystem.SpriteSystem_InitManagerWithCapacities(system.Context.SpriteSystem, system.PokemonSpriteManager, caps);
    }

    private static int DummyResourceID(BattleAnimSystem system, int resID)
    {
        return DummyResourceBase + resID + system.Context!.Attacker * DummyResourceBattlerStride;
    }

    // OFFSET 0x022214C4
    private static void BattleAnimScriptCmd_LoadPokemonSpriteDummyResources(BattleAnimSystem system)
    {
        Next(system);
        int resID = (int)ReadWord(system);
        Next(system);
        int id = DummyResourceID(system, resID);
        SpriteSystem spriteSystem = system.Context!.SpriteSystem!;
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSystem, system.PokemonSpriteManager!, system.Arcs[ArcBattObj]!, 0x4C, 0, VramTypeMain, id);
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(system.PaletteData!, PlttBufMainObj, spriteSystem, system.PokemonSpriteManager!, system.Arcs[ArcBattObj]!, 0x4B, 0, 1, VramTypeMain, id);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(spriteSystem, system.PokemonSpriteManager!, system.Arcs[ArcBattObj]!, 0x4D, 0, id);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(spriteSystem, system.PokemonSpriteManager!, system.Arcs[ArcBattObj]!, 0x4E, 0, id);
    }

    // OFFSET 0x02221580
    private static void BattleAnimScriptCmd_AddPokemonSprite(BattleAnimSystem system)
    {
        Next(system);
        int battlerRole = (int)ReadWord(system);
        Next(system);
        int trackBattler = (int)ReadWord(system);
        Next(system);
        int spriteID = (int)ReadWord(system);
        Next(system);
        int resID = (int)ReadWord(system);
        Next(system);
        int resourceID = DummyResourceID(system, resID);
        int battler = BattleAnimSystem_GetBattlerWithRole(system, battlerRole);
        PokemonSpriteData spriteData = system.Context!.PokemonSpriteData[battler]!;
        int narcID = spriteData.NarcID;
        int paletteIndex = spriteData.Palette;
        byte[]? charData = spriteData.Tiles;
        int battlerForm = system.Context.BattlerForms[battler];
        PokemonSprite? battlerSprite = BattleAnimSystem_GetBattlerSprite(system, battler);
        short battlerX = 0;
        short battlerY = 0;
        if (battlerSprite != null)
        {
            battlerX = (short)PokemonSprite.PokemonSprite_GetAttribute(battlerSprite, MonSpriteXCenter);
            battlerY = TrackedY(battlerSprite);
        }
        SpriteTemplate template = new();
        template.X = battlerX;
        template.Y = battlerY;
        template.Z = 0;
        template.AnimIdx = 0;
        template.Priority = 100;
        template.PlttIdx = 0;
        template.VramType = VramTypeMain;
        template.BgPriority = 1;
        template.VramTransfer = 0;
        for (int i = 0; i < 4; i++)
        {
            template.Resources[i] = resourceID;
        }
        template.Resources[4] = 0;
        template.Resources[5] = 0;
        ManagedSprite sprite = SpriteSystem.SpriteSystem_NewSprite(system.Context.SpriteSystem!, system.PokemonSpriteManager!, template)!;
        if (battlerSprite == null)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(sprite, 0);
        }
        else if (PokemonSprite.PokemonSprite_GetAttribute(battlerSprite, MonSpriteHide) == 1)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(sprite, 0);
        }
        if (BattleAnimSystem_IsContest(system) == 1 && IsFormSymmetrical(BattleAnimSystem_GetBattlerSpecies(system, battler), battlerForm) == 1)
        {
            SpriteSystem.ManagedSprite_SetFlipMode(sprite, 1);
        }
        if (BattleAnimSystem_GetBattlerSprite(system, battler) != null)
        {
            ImageProxy proxy = Sprite.Sprite_GetImageProxy(sprite.Sprite!);
            VramTransfer.VramTransfer_Request(0x13, (uint)G2d.GetImageLocation(proxy, VramTypeMain), charData!, MonSpriteTileBytes);
        }
        if (BattleAnimSystem_GetBattlerSprite(system, battler) != null)
        {
            ImagePaletteProxy proxy = Sprite.Sprite_GetPaletteProxy(sprite.Sprite!);
            uint offset = PlttTransfer.PlttTransfer_GetPlttOffset(proxy, VramTypeMain);
            PaletteData.PaletteData_LoadBufferFromFileStart(system.PaletteData!, narcID, paletteIndex, system.HeapID, PlttBufMainObj, PaletteSizeBytes, (ushort)(offset * 16));
        }
        if (system.PokemonSprites[spriteID] != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        system.PokemonSprites[spriteID] = sprite;
        system.UsedPokemonSprites[spriteID] = 1;
        if (trackBattler == 1)
        {
            if (BattleAnimSystem_GetBattlerSprite(system, battler) == null)
            {
                return;
            }
            SpriteTrackingTask trackingTask = new();
            system.SpriteTrackingTasks[spriteID] = trackingTask;
            trackingTask.Sprite = sprite;
            trackingTask.Data.Sprite = BattleAnimSystem_GetBattlerSprite(system, battler);
            trackingTask.Data.FrameCount = 0;
            trackingTask.Data.Interval = 0;
            trackingTask.Task = SysTask.SysTask_Start(BattleAnimSystem_SpriteTrackingTaskFunc, trackingTask, TrackingTaskPriority);
        }
    }

    // OFFSET 0x022217B4
    private static void BattleAnimScriptCmd_FreePokemonSpriteManager(BattleAnimSystem system)
    {
        Next(system);
        if (system.PokemonSpriteManager != null)
        {
            SpriteSystem.SpriteSystem_FreeResourcesAndManager(system.Context!.SpriteSystem!, system.PokemonSpriteManager);
        }
        system.PokemonSpriteManager = null;
    }

    // OFFSET 0x022217E0
    private static void BattleAnimScriptCmd_RemovePokemonSprite(BattleAnimSystem system)
    {
        Next(system);
        int spriteID = (int)ReadWord(system);
        Next(system);
        if (system.PokemonSprites[spriteID] != null)
        {
            SpriteSystem.Sprite_DeleteAndFreeResources(system.PokemonSprites[spriteID]!);
        }
        system.UsedPokemonSprites[spriteID] = 0;
        system.PokemonSprites[spriteID] = null;
    }

    // OFFSET 0x02221810
    private static void BattleAnimPokemonSprite_DrawTask(SysTask task, object? param)
    {
        BattleAnimPokemonSpriteDrawContext ctx = (BattleAnimPokemonSpriteDrawContext)param!;
        if (ctx.Active == 0)
        {
            SysTask.SysTask_Done(task);
            return;
        }
        if (ctx.BattleAnimSys!.PokemonSpriteManager != null)
        {
            SpriteSystem.SpriteSystem_DrawSprites(ctx.SpriteManager);
        }
    }

    // OFFSET 0x02221834
    private static void BattleAnimScriptCmd_StartPokemonSpriteDrawTask(BattleAnimSystem system)
    {
        Next(system);
        int role = (int)ReadWord(system);
        Next(system);
        int slotIdx = (int)ReadWord(system);
        Next(system);
        int spriteID = (int)ReadWord(system);
        Next(system);
        BattleAnimPokemonSpriteDrawContext drawCtx = system.PokemonSpriteDrawContexts[slotIdx];
        drawCtx.BattleAnimSys = system;
        drawCtx.SpriteManager = system.PokemonSpriteManager;
        drawCtx.Sprite = system.PokemonSprites[spriteID];
        drawCtx.Active = 1;
        SpriteSystem.ManagedSprite_SetDrawFlag(drawCtx.Sprite!, 0);
        if (BattleAnimSystem_IsDoubleBattle(system) != 1)
        {
            return;
        }
        int attackerType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, BattleAnimSystem_GetAttacker(system));
        int defenderType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, BattleAnimSystem_GetDefender(system));
        int battler = BattleAnimSystem_GetBattlerWithRole(system, role);
        PokemonSprite? battlerSprite = BattleAnimSystem_GetBattlerSprite(system, battler);
        int isHidden = battlerSprite != null ? PokemonSprite.PokemonSprite_GetAttribute(battlerSprite, MonSpriteHide) : 0;
        SpriteSystem.ManagedSprite_SetDrawFlag(drawCtx.Sprite!, isHidden == 1 ? 0 : 1);
        ManagedSprite sprite = drawCtx.Sprite!;
        switch (role)
        {
            case RoleAttacker:
                if (attackerType == 3 || attackerType == 4)
                {
                    SpriteSystem.ManagedSprite_SetPriority(sprite, 1);
                }
                else
                {
                    SpriteSystem.ManagedSprite_SetDrawFlag(sprite, 0);
                    SpriteSystem.ManagedSprite_SetPriority(sprite, 0xFF);
                }
                break;
            case RoleAttackerPartner:
                if (attackerType == 5 || attackerType == 2)
                {
                    SpriteSystem.ManagedSprite_SetPriority(sprite, 1);
                }
                else
                {
                    SpriteSystem.ManagedSprite_SetDrawFlag(sprite, 0);
                    SpriteSystem.ManagedSprite_SetPriority(sprite, 0xFF);
                }
                break;
            case RoleDefender:
                switch (defenderType)
                {
                    case 2:
                    case 5:
                        SpriteSystem.ManagedSprite_SetPriority(sprite, 0xFF);
                        break;
                    case 3:
                    case 4:
                        SpriteSystem.ManagedSprite_SetPriority(sprite, 1);
                        break;
                }
                break;
            case RoleDefenderPartner:
                switch (defenderType)
                {
                    case 2:
                    case 5:
                        SpriteSystem.ManagedSprite_SetPriority(sprite, 1);
                        break;
                    case 3:
                    case 4:
                        SpriteSystem.ManagedSprite_SetPriority(sprite, 0xFF);
                        break;
                }
                break;
        }
        SysTask.SysTask_Start(BattleAnimPokemonSprite_DrawTask, drawCtx, DrawTaskPriority);
    }

    // OFFSET 0x022219E8
    private static void BattleAnimScriptCmd_StopPokemonSpriteDrawTask(BattleAnimSystem system)
    {
        Next(system);
        int slotIdx = (int)ReadWord(system);
        Next(system);
        system.PokemonSpriteDrawContexts[slotIdx].Active = 0;
    }

    // OFFSET 0x02221A00
    private static void BattleAnimScriptCmd_CancelTrackingTask(BattleAnimSystem system)
    {
        Next(system);
        int taskID = (int)ReadWord(system);
        Next(system);
        BattleAnimSystem_CancelTrackingTask(system, taskID);
    }

    // OFFSET 0x02221A14
    private static void BattleAnimScriptCmd_SetCameraProjection(BattleAnimSystem system)
    {
        Next(system);
        int psIndex = (int)ReadWord(system);
        Next(system);
        int projection = (int)ReadWord(system);
        Next(system);
        system.CameraProjections[psIndex] = (byte)projection;
    }

    // OFFSET 0x02221A30
    private static void BattleAnimScriptCmd_SetCameraFlip(BattleAnimSystem system)
    {
        Next(system);
        int psIndex = (int)ReadWord(system);
        Next(system);
        int flip = (int)ReadWord(system);
        Next(system);
        system.ParticleSystemCameraFlip[psIndex] = (byte)flip;
    }

    // OFFSET 0x02221A4C
    private static void BattleAnimScriptCmd_Nop7(BattleAnimSystem system)
    {
    }

    // OFFSET 0x02221A50
    private static void BattleAnimScriptCmd_Nop8(BattleAnimSystem system)
    {
    }

    // OFFSET 0x0222293C
    private static void BattleAnimScriptCmd_Nop4(BattleAnimSystem system)
    {
    }

    // OFFSET 0x02222940
    private static void BattleAnimScriptCmd_Nop5(BattleAnimSystem system)
    {
    }

    // OFFSET 0x02222944
    private static void BattleAnimScriptCmd_Nop6(BattleAnimSystem system)
    {
    }

    // OFFSET 0x02222948
    private static void BattleAnimScriptCmd_Nop9(BattleAnimSystem system)
    {
    }

    // OFFSET 0x0222294C
    private static void BattleAnimScriptCmd_Nop10(BattleAnimSystem system)
    {
    }

    // OFFSET 0x02222CAC
    private static void BattleAnimScriptCmd_SetBG0BG1AlphaBlending(BattleAnimSystem system)
    {
        Next(system);
        ushort ev1 = (ushort)ReadWord(system);
        Next(system);
        ushort ev2 = (ushort)ReadWord(system);
        Next(system);
        Video.SetBlendAlpha(1, 1, 2, ev1, ev2);
    }

    // OFFSET 0x02222CDC
    private static void BattleAnimScriptCmd_SetDefaultAlphaBlending(BattleAnimSystem system)
    {
        BattleAnimSystem_SetDefaultAlphaBlending();
    }

    // OFFSET 0x02222CE4
    private static void BattleAnimScriptCmd_Nop2(BattleAnimSystem system)
    {
    }

    // OFFSET 0x02222CE8
    private static void BattleAnimScriptCmd_InitSpriteManager(BattleAnimSystem system)
    {
        Next(system);
        int id = (int)ReadWord(system);
        Next(system);
        int maxSprites = (int)ReadWord(system);
        Next(system);
        if (system.SpriteManagers[id] != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        system.SpriteManagers[id] = SpriteSystem.SpriteManager_New(system.Context!.SpriteSystem);
        if (system.SpriteManagers[id] == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteSystem.SpriteSystem_InitSprites(system.Context.SpriteSystem, system.SpriteManagers[id], maxSprites);
        SpriteUtil.SetSubScreenViewRect(SpriteSystem.SpriteSystem_GetRenderer(system.Context.SpriteSystem!), 0, 0x110000);
        int[] caps = new int[SpriteResourceCount];
        for (int i = 0; i < SpriteResourceCount; i++)
        {
            caps[i] = (int)ReadWord(system);
            Next(system);
        }
        SpriteSystem.SpriteSystem_InitManagerWithCapacities(system.Context.SpriteSystem, system.SpriteManagers[id], caps);
    }

    private static int ScriptResourceID(int memberIndex)
    {
        return memberIndex + 5000;
    }

    // OFFSET 0x02222D84
    private static void BattleAnimScriptCmd_LoadCharResObj(BattleAnimSystem system)
    {
        Next(system);
        int id = (int)ReadWord(system);
        Next(system);
        int memberIndex = (int)ReadWord(system);
        Next(system);
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(system.Context!.SpriteSystem!, system.SpriteManagers[id]!, system.Arcs[ArcWeChar]!, memberIndex, 1, VramTypeMain, ScriptResourceID(memberIndex));
    }

    // OFFSET 0x02222DCC
    private static void BattleAnimScriptCmd_LoadPlttRes(BattleAnimSystem system)
    {
        Next(system);
        int id = (int)ReadWord(system);
        Next(system);
        int memberIndex = (int)ReadWord(system);
        Next(system);
        int paletteIndex = (int)ReadWord(system);
        Next(system);
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(system.PaletteData!, PlttBufMainObj, system.Context!.SpriteSystem!, system.SpriteManagers[id]!, system.Arcs[ArcWePltt]!, memberIndex, 0, VramTypeMain, paletteIndex, ScriptResourceID(memberIndex));
    }

    // OFFSET 0x02222E2C
    private static void BattleAnimScriptCmd_LoadCellResObj(BattleAnimSystem system)
    {
        Next(system);
        int id = (int)ReadWord(system);
        Next(system);
        int memberIndex = (int)ReadWord(system);
        Next(system);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(system.Context!.SpriteSystem!, system.SpriteManagers[id]!, system.Arcs[ArcWeCell]!, memberIndex, 1, ScriptResourceID(memberIndex));
    }

    // OFFSET 0x02222E74
    private static void BattleAnimScriptCmd_LoadAnimResObj(BattleAnimSystem system)
    {
        Next(system);
        int id = (int)ReadWord(system);
        Next(system);
        int memberIndex = (int)ReadWord(system);
        Next(system);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(system.Context!.SpriteSystem!, system.SpriteManagers[id]!, system.Arcs[ArcWeCellAnm]!, memberIndex, 1, ScriptResourceID(memberIndex));
    }

    private static SpriteTemplate DefenderSpriteTemplate(BattleAnimSystem system)
    {
        SpriteTemplate template = new();
        int defender = BattleAnimSystem_GetDefender(system);
        template.X = (short)BattleAnimUtil.BattleAnimUtil_GetBattlerPos(system, defender, 0);
        template.Y = (short)BattleAnimUtil.BattleAnimUtil_GetBattlerPos(system, defender, 1);
        template.Z = 0;
        template.AnimIdx = 0;
        template.Priority = 100;
        template.PlttIdx = 0;
        template.VramType = VramTypeMain;
        template.BgPriority = 1;
        template.VramTransfer = 0;
        for (int i = 0; i < SpriteResourceCount; i++)
        {
            template.Resources[i] = ScriptResourceID((int)ReadWord(system));
            Next(system);
        }
        return template;
    }

    private static void CopyTemplate(SpriteTemplate source, SpriteTemplate dest)
    {
        dest.X = source.X;
        dest.Y = source.Y;
        dest.Z = source.Z;
        dest.AnimIdx = source.AnimIdx;
        dest.Priority = source.Priority;
        dest.PlttIdx = source.PlttIdx;
        dest.VramType = source.VramType;
        Array.Copy(source.Resources, dest.Resources, SpriteResourceCount);
        dest.BgPriority = source.BgPriority;
        dest.VramTransfer = source.VramTransfer;
    }

    // OFFSET 0x02222EBC
    private static void BattleAnimScriptCmd_AddSpriteWithFunc(BattleAnimSystem system)
    {
        Next(system);
        int id = (int)ReadWord(system);
        Next(system);
        uint funcID = ReadWord(system);
        Next(system);
        SpriteTemplate template = DefenderSpriteTemplate(system);
        CopyTemplate(template, system.LastSpriteTemplate);
        ManagedSprite? sprite = SpriteSystem.SpriteSystem_NewSprite(system.Context!.SpriteSystem!, system.SpriteManagers[id]!, template);
        int argCount = (int)ReadWord(system);
        Next(system);
        int i;
        for (i = 0; i < argCount; i++)
        {
            system.ScriptVars[i] = (int)ReadWord(system);
            Next(system);
        }
        for (; i < BattleAnimSystem.ScriptVarCount; i++)
        {
            system.ScriptVars[i] = 0;
        }
        Action<BattleAnimSystem, SpriteSystem, SpriteManager, ManagedSprite>? func = BattleAnimScriptFuncs.BattleAnimScript_GetSpriteFunc(funcID);
        func!(system, system.Context.SpriteSystem!, system.SpriteManagers[id]!, sprite!);
    }

    // OFFSET 0x02222FC8
    private static void BattleAnimScriptCmd_AddSprite(BattleAnimSystem system)
    {
        Next(system);
        int id = (int)ReadWord(system);
        Next(system);
        int spriteID = (int)ReadWord(system);
        Next(system);
        SpriteTemplate template = DefenderSpriteTemplate(system);
        CopyTemplate(template, system.LastSpriteTemplate);
        ManagedSprite? sprite = SpriteSystem.SpriteSystem_NewSprite(system.Context!.SpriteSystem!, system.SpriteManagers[id]!, template);
        if (system.Sprites[spriteID] != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        system.Sprites[spriteID] = sprite;
    }

    // OFFSET 0x0222307C
    private static void BattleAnimScriptCmd_FreeSpriteManager(BattleAnimSystem system)
    {
        Next(system);
        int id = (int)ReadWord(system);
        Next(system);
        if (system.SpriteManagers[id] != null)
        {
            SpriteSystem.SpriteSystem_FreeResourcesAndManager(system.Context!.SpriteSystem!, system.SpriteManagers[id]!);
        }
        system.SpriteManagers[id] = null;
    }

    // OFFSET 0x022230A8
    private static void BattleAnimScriptCmd_SetPokemonSpriteVisible(BattleAnimSystem system)
    {
        Next(system);
        int spriteID = (int)ReadWord(system);
        Next(system);
        int flag = (int)ReadWord(system);
        Next(system);
        SpriteSystem.ManagedSprite_SetDrawFlag(system.PokemonSprites[spriteID]!, flag);
    }

    // OFFSET 0x022230CC
    private static void BattleAnimScriptCmd_SetExtraParams(BattleAnimSystem system)
    {
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x022230D4
    private static void BattleAnimScriptCmd_PlayPokemonCry(BattleAnimSystem system)
    {
        Next(system);
        int modulation = (int)ReadWord(system);
        Next(system);
        short pan = (short)ReadWord(system);
        pan = BattleAnimSound_CorrectPanDirection(system, (sbyte)pan);
        Next(system);
        int volume = (int)ReadWord(system);
        Next(system);
        BattleAnimContext context = system.Context!;
        int species = context.BattlerSpecies[context.Attacker];
        int form = context.BattlerForms[context.Attacker];
        PokemonSpriteMetadata.Pokemon_PlayCry(context.ChatotCry, modulation, (ushort)species, form, pan, volume, context.IsTransformed, system.HeapID);
    }

    // OFFSET 0x02223134
    private static void BattleAnimScriptCmd_WaitForPokemonCries(BattleAnimSystem system)
    {
        if (SoundPlayback.Sound_IsPokemonCryPlaying() == 0)
        {
            Next(system);
            int fadeOutFrames = (int)ReadWord(system);
            Next(system);
            system.ScriptDelay = 0;
            SoundPlayback.Sound_StopPokemonCries(fadeOutFrames);
        }
        else
        {
            system.ScriptDelay = 1;
        }
    }

    // OFFSET 0x02223160
    private static void BattleAnimScriptCmd_Nop11(BattleAnimSystem system)
    {
    }

    // OFFSET 0x02223164
    public static Action<BattleAnimSystem>? BattleAnimSystem_GetScriptCmd(uint id)
    {
        if (id > ScriptCommandCount)
        {
            return null;
        }
        return ScriptCommands[id];
    }

    // OFFSET 0x02223178
    public static int BattleAnimContext_GetUnk01(BattleAnimContext ctx)
    {
        return ctx.Unknown01;
    }

    // OFFSET 0x0222325C
    public static int BattleAnimSystem_GetExtraParams(BattleAnimSystem system, int[] parameters, int count)
    {
        Next(system);
        int available = (int)ReadWord(system);
        Next(system);
        if (available != count)
        {
            ErrorHandling.GF_AssertFail();
            return 0;
        }
        for (int i = 0; i < count; i++)
        {
            parameters[i] = (int)ReadWord(system);
            Next(system);
        }
        return 1;
    }

    // OFFSET 0x0222329C
    public static SpriteTemplate BattleAnimSystem_GetLastSpriteTemplate(BattleAnimSystem system)
    {
        SpriteTemplate template = new();
        CopyTemplate(system.LastSpriteTemplate, template);
        return template;
    }

    // OFFSET 0x022232B8
    public static int BattleAnimSystem_GetBattlerType(BattleAnimSystem system, int battler)
    {
        return system.Context!.BattlerTypes[battler];
    }

    // OFFSET 0x022232C4
    public static int BattleAnimSystem_GetBattlerGender(BattleAnimSystem system, int battler)
    {
        return system.Context!.BattlerGenders[battler];
    }

    // OFFSET 0x022232D0
    public static int BattleAnimSystem_GetBattlerSpecies(BattleAnimSystem system, int battler)
    {
        return system.Context!.BattlerSpecies[battler];
    }

    // OFFSET 0x022232E0
    public static int BattleAnimSystem_GetBattlerForm(BattleAnimSystem system, int battler)
    {
        return system.Context!.BattlerForms[battler];
    }

    // OFFSET 0x022232EC
    public static int BattleAnimSystem_GetBattlerPersonality(BattleAnimSystem system, int battler)
    {
        return (int)system.Context!.BattlerPersonalities[battler];
    }

    // OFFSET 0x022232FC
    public static PokemonSprite? BattleAnimSystem_GetBattlerSprite(BattleAnimSystem system, int battler)
    {
        PokemonSprite? sprite = system.Context!.BattlerSprites[battler];
        if (sprite == null)
        {
            return null;
        }
        return PokemonSprite.PokemonSprite_IsActive(sprite) ? sprite : null;
    }

    // OFFSET 0x0222332C
    public static PaletteData BattleAnimSystem_GetPaletteData(BattleAnimSystem system)
    {
        return system.PaletteData!;
    }

    // OFFSET 0x02223334
    public static int BattleAnimSystem_GetBattlerSpritePaletteIndex(BattleAnimSystem system, int battler)
    {
        return system.Context!.PokemonSpriteData[battler]!.Palette;
    }

    // OFFSET 0x02223344
    public static int BattleAnimSystem_GetBattlerSpriteNarcID(BattleAnimSystem system, int battler)
    {
        return system.Context!.PokemonSpriteData[battler]!.NarcID;
    }

    // OFFSET 0x02223354
    public static int BattleAnimSystem_GetBattlerSpriteOffset(BattleAnimSystem system, int battler)
    {
        return system.Context!.PokemonSpriteData[battler]!.YOffset;
    }

    // OFFSET 0x02223364
    public static int BattleAnimSystem_IsDoubleBattle(BattleAnimSystem system)
    {
        return (system.Context!.BattleType & BattleTypeDoublesAnim) != 0 ? 1 : 0;
    }

    // OFFSET 0x0222337C
    public static int BattleAnimSystem_IsBattlerSemiInvulnerable(BattleAnimSystem system, int battler)
    {
        return (system.Context!.BattlerMoveEffects[battler] & MoveEffectSemiInvulnerable) != 0 ? 1 : 0;
    }

    // OFFSET 0x0222339C
    public static int BattleAnimSystem_GetPokemonSpritePriority(BattleAnimSystem system)
    {
        return BattleAnimSystem_IsContest(system) == 1 ? 2 : 1;
    }

    // OFFSET 0x022233B0
    public static int BattleAnimSystem_GetBgLayer(BattleAnimSystem system, int bg)
    {
        int[] bgLayers = { 1, BattleBgBase, BattleBgEffect };
        BattleAnimSystem_IsContest(system);
        return bgLayers[bg];
    }

    // OFFSET 0x022233EC
    public static int BattleAnimSystem_GetBgID(BattleAnimSystem system, int bg)
    {
        int[] bgIDs = { 1, BattleBgBase, BattleBgEffect };
        BattleAnimSystem_IsContest(system);
        return bgIDs[bg];
    }

    // OFFSET 0x02223428
    public static int BattleAnimSystem_GetBgPriority(BattleAnimSystem system, int bg)
    {
        switch (bg)
        {
            case 0:
                BattleAnimSystem_IsContest(system);
                return 0;
            case 1:
                BattleAnimSystem_IsContest(system);
                return 1;
            case 2:
                BattleAnimSystem_IsContest(system);
                return 3;
            case 3:
                return BattleAnimSystem_GetPokemonSpritePriority(system);
        }
        return 0;
    }

    // OFFSET 0x02223460
    public static void BattleAnimSystem_LoadBattleBgTiles(BattleAnimSystem system, int layer)
    {
        Bg.Bg_LoadTiles(system.BgConfig!, layer, system.Context!.BgTiles!, 0x10000, 0);
    }

    // OFFSET 0x02223488
    public static void BattleAnimSystem_LoadBattleBgPaletteBuffer(BattleAnimSystem system)
    {
        PaletteData.PaletteData_LoadBuffer(system.PaletteData!, system.Context!.BgPaletteBuffer!, 0, PlttBufMainBg, 0, PaletteSizeBytes * 16);
    }

    // OFFSET 0x022234A8
    public static int BattleAnimSystem_ShouldBattlerSpriteBeFlipped(BattleAnimSystem system, int battlerRole)
    {
        int battler = BattleAnimSystem_GetBattlerWithRole(system, battlerRole);
        int form = system.Context!.BattlerForms[battler];
        if (BattleAnimSystem_IsContest(system) == 1 && IsFormSymmetrical(BattleAnimSystem_GetBattlerSpecies(system, battler), form) == 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x022384F0
    public static int IsFormSymmetrical(int species, int form)
    {
        switch (species)
        {
            case 0x63:
            case 0x78:
            case 0x7D:
            case 0x9F:
            case 0xC9:
            case 0xD7:
            case 0x13B:
            case 0x14F:
            case 0x167:
            case 0x185:
            case 0x196:
            case 0x197:
            case 0x1D3:
            case 0x1D4:
                return 0;
            case 0x1EC:
                if (form == 1)
                {
                    return 0;
                }
                break;
        }
        return 1;
    }
}
