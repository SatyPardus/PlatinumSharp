namespace PlatinumSharp.Game;

public sealed class FadePokemonSpriteContext
{
    public readonly BattleAnimScriptFuncCommon Common = new();
    public readonly BattleAnimSpriteInfo[] SpriteInfo = { new(), new(), new(), new() };
    public PaletteFadeContext? PaletteFade;
}

public sealed class BattlerPartialDrawTestContext
{
    public readonly BattleAnimScriptFuncCommon Common = new();
    public readonly BattleAnimSpriteInfo[] SpriteInfo = { new(), new(), new(), new() };
    public int XOffset;
    public int YOffset;
    public int Width;
    public int Height;
    public short Unused;
    public short Step;
    public int StepFrames;
    public int Mode;
    public int Timer;
    public int Y;
    public int DrawHeight;
}

public sealed class BattlerPartialDrawContext
{
    public readonly BattleAnimScriptFuncCommon Common = new();
    public short Step;
    public int StepInterval;
    public int Timer;
    public readonly BattleAnimSpriteInfo[] SpriteInfo = { new(), new(), new(), new() };
    public int YOffset;
    public int Height;
    public int Y;
    public int BaseY;
}

public sealed class ShakeBgContext
{
    public byte State;
    public byte Iteration;
    public int ExtentX;
    public int ExtentY;
    public int Interval;
    public int Amount;
    public int Cycles;
    public int BgLayer;
    public int ResetX;
    public int ResetY;
    public BgConfig? BgConfig;
    public BattleAnimSystem? BattleAnimSys;
    public SpriteManager? PokemonSpriteManager;
    public readonly XYTransformContext Shake = new();
}

public sealed class PixelatePokemonSpriteContext
{
    public readonly BattleAnimScriptFuncCommon Common = new();
    public ManagedSprite? Sprite;
    public short ScaleStep;
    public int ScaleX;
    public int ScaleY;
    public int TargetScale;
}

public sealed class EmitterAnimationContext
{
    public int EmitterID;
    public short StartDelay;
    public short Frames;
    public int Radius;
    public short OffsetX;
    public short OffsetY;
    public int Mode;
    public int StartBattler;
    public int EndBattler;
    public int Frame;
    public int SkipFrames;
    public int MaxFrames;
    public int Timer;
    public int Angle;
    public int Curve;
    public ParticleSystem? ParticleSys;
    public SplEmitter? Emitter;
    public readonly BattleAnimScriptFuncCommon Common = new();
    public readonly XYTransformContext[] Transforms = { new(), new() };
    public readonly ValueLerpContext Unused = new();
}

public sealed class EmitterRevolutionContext
{
    public int EmitterID;
    public int Sx;
    public int Sy;
    public int Ex;
    public int Ey;
    public int Rx;
    public int Ry;
    public int Frames;
    public int Mode;
    public int Battler;
    public ParticleSystem? ParticleSys;
    public SplEmitter? Emitter;
    public readonly BattleAnimScriptFuncCommon Common = new();
    public readonly XYTransformContext Revs = new();
    public VecFx32 BattlerPos;
}

public sealed class SpriteAnimContext
{
    public readonly BattleAnimScriptFuncCommon Common = new();
    public ManagedSprite? Sprite;
    public readonly ValueLerpContext Tilt = new();
    public int State;
    public byte SpriteAlpha;
    public byte BgAlpha;
    public readonly XYTransformContext[] Pos = { new(), new() };
}

public sealed class SetPokemonSpritePriorityContext
{
    public readonly BattleAnimScriptFuncCommon Common = new();
    public ManagedSprite? Sprite;
    public int SpriteID;
    public int MaxFrames;
    public int Bg;
    public int SpritePriority;
    public int Mode;
    public int WindowType;
    public int StepCount;
    public int SinkFullyStartFrame;
}

public sealed class RenderPokemonSpritesContext
{
    public readonly BattleAnimScriptFuncCommon Common = new();
    public readonly ManagedSprite?[] Sprites = new ManagedSprite?[4];
    public int Frames;
}

public sealed class ScrollSwitchedBgContext
{
    public int Timer;
    public int Frames;
    public readonly BattleAnimScriptFuncCommon Common = new();
    public BgScrollContext? BgScroll;
}

public static partial class BattleAnimScriptFuncs
{
    private const int MonSpritePartialDraw = 0xE;
    private const int MonSpriteDrawHeight = 0x12;
    private const int MonSpriteFrameHeight = 0x50;
    private const int FadePokemonSpriteTaskPriority = 1100;
    private const int PlttBufMainObj = 2;
    private const int PixelateMinScale = 0;
    private const int PixelateMaxScale = 15;
    private const int EmitterDefaultFrames = 0xFF;
    private const int EmitterModeAtkToDef = 0;
    private const int EmitterModeDefToAtk = 1;
    private const int EmitterFromTop = 0;
    private const int EmitterRevolutionModeAttacker = 0;
    private const int ParticlePixelFactor = 172;
    private const int ShakeBgTargetEffect = 0;
    private const uint FollowMeOffsetsAddress = 0x0223A03C;
    private const short FollowMeOffsetSwitchPrio = 0;
    private const short FollowMeOffsetEnd = 0xFF;
    private const int FollowMeTiltAngle = 0xE38;
    private const int FollowMeTiltFrames = 4;
    private const int FissureContestX = 72;
    private const int FissureContestY = 32;
    private const int FissurePlayerY = 126;
    private const int FissureEnemyY = 32;
    private const int EscapeItemOffsetX = -30;
    private const int EscapeItemOffsetY = 160;
    private const int EscapeItemDestX = 112;
    private const int EscapeItemDestY = 112;
    private const int EscapeItemMoveFrames = 21;
    private const int EscapeItemMoveRadius = 64 << 12;
    private const int EscapeItemMinAlpha = 0;
    private const int EscapeItemMaxAlpha = 16;
    private const int DarkVoidStepY = 4;
    private const int DarkVoidStepYL = 8;
    private const int DarkVoidMaxY = 130;
    private const int DarkVoidSinkMinFrame = 35;
    private const int DarkVoidSinkRngFrame = 5;
    private const int PriorityModeDefault = 0;
    private const int BattleAnimBgNone = 0xFF;
    private const int BattleAnimDefaultPriority = 0xFF;
    private const int RoleAttackerPartner = 2;
    private const int RoleDefenderPartner = 3;
    private const uint RenderPriorityTableAddress = 0x0223B120;
    private const int RenderDefaultFrames = 3;
    private const int Overlay = 12;

    private static ushort DegToIdx(int degrees)
    {
        return (ushort)(degrees * 0xFFFF / 360);
    }

    // OFFSET 0x02229184
    private static void BattleAnimTask_FadePokemonSprite(SysTask task, object? param)
    {
        FadePokemonSpriteContext ctx = (FadePokemonSpriteContext)param!;
        if (BattleAnimUtil.PaletteFadeContext_IsActive(ctx.PaletteFade) == 0)
        {
            BattleAnimUtil.PaletteFadeContext_Free(ctx.PaletteFade);
            BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
        }
    }

    // OFFSET 0x022291AC
    private static void BattleAnimScriptFunc_FadePokemonSprite(BattleAnimSystem system)
    {
        FadePokemonSpriteContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<FadePokemonSpriteContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        int target = ScriptVar(system, 0);
        int stepFrames = ScriptVar(system, 1);
        int step = ScriptVar(system, 2);
        int startFrac = ScriptVar(system, 3);
        int endFrac = ScriptVar(system, 4);
        int color = ScriptVar(system, 5);
        BattleAnimUtil.BattleAnimUtil_GetPokemonSprites(system, target, ctx.SpriteInfo, out _);
        uint palDest = PlttTransfer.PlttTransfer_GetPlttOffset(Sprite.Sprite_GetPaletteProxy(ctx.SpriteInfo[0].HwSprite!.Sprite!), 1);
        ctx.PaletteFade = BattleAnimUtil.PaletteFadeContext_New(ctx.Common.PaletteData!, BattleAnimSystems.BattleAnimSystem_GetHeapID(system), PlttBufMainObj, (ushort)(palDest * 16), 16, (sbyte)stepFrames, (sbyte)step, (byte)startFrac, (byte)endFrac, (ushort)color, FadePokemonSpriteTaskPriority);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_FadePokemonSprite, ctx);
    }

    // OFFSET 0x02229278
    private static void BattleAnimTask_BattlerPartialDrawTest(SysTask task, object? param)
    {
        BattlerPartialDrawTestContext ctx = (BattlerPartialDrawTestContext)param!;
        PokemonSprite sprite = ctx.SpriteInfo[0].MonSprite!;
        switch (ctx.Common.State)
        {
            case 0:
                if (++ctx.Timer < ctx.StepFrames)
                {
                    return;
                }
                ctx.Timer = 0;
                if (ctx.Mode == 0)
                {
                    if (ctx.DrawHeight < 0)
                    {
                        ctx.DrawHeight = 0;
                    }
                    if (ctx.DrawHeight == 0)
                    {
                        ctx.Common.State++;
                    }
                }
                else
                {
                    if (ctx.DrawHeight > MonSpriteFrameHeight)
                    {
                        ctx.DrawHeight = MonSpriteFrameHeight;
                    }
                    if (ctx.DrawHeight == MonSpriteFrameHeight)
                    {
                        ctx.Common.State++;
                    }
                }
                PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteDrawHeight, ctx.DrawHeight);
                PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteYCenter, ctx.Y);
                ctx.Y -= ctx.Step;
                ctx.DrawHeight += ctx.Step;
                break;
            default:
                ResetMonScale(sprite);
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
                break;
        }
    }

    // OFFSET 0x02229304
    private static void BattleAnimScriptFunc_BattlerPartialDrawTest(BattleAnimSystem system)
    {
        BattlerPartialDrawTestContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<BattlerPartialDrawTestContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        int target = ScriptVar(system, 0);
        BattleAnimUtil.BattleAnimUtil_GetBattlerSprites(system, target, ctx.SpriteInfo, out _);
        int battler = GetRoleBattler(system, target);
        PokemonSprite sprite = ctx.SpriteInfo[0].MonSprite!;
        ctx.Mode = ScriptVar(system, 1);
        if (ctx.Mode == 0)
        {
            ctx.Y = PokemonSprite.PokemonSprite_GetAttribute(sprite, MonSpriteYCenter);
            ctx.DrawHeight = MonSpriteFrameHeight - PokemonSprite.PokemonSprite_GetAttribute(sprite, MonSpriteDrawHeight);
            ctx.Unused = (short)ScriptVar(system, 2);
            ctx.Step = (short)ScriptVar(system, 3);
            ctx.Step = (short)-ctx.Step;
        }
        else
        {
            ctx.Y = PokemonSprite.PokemonSprite_GetAttribute(sprite, MonSpriteYCenter);
            ctx.DrawHeight = PokemonSprite.PokemonSprite_GetAttribute(sprite, MonSpriteDrawHeight);
            ctx.Unused = (short)ScriptVar(system, 2);
            ctx.Step = (short)ScriptVar(system, 3);
        }
        ctx.XOffset = 0;
        ctx.YOffset = 0;
        ctx.Width = MonSpriteFrameWidth;
        ctx.Height = MonSpriteFrameHeight - BattleAnimSystems.BattleAnimSystem_GetBattlerSpriteOffset(system, battler);
        ctx.StepFrames = ScriptVar(system, 4);
        ctx.Timer = 0;
        PokemonSprite.PokemonSprite_SetPartialDraw(sprite, ctx.XOffset, ctx.YOffset, ctx.Width, ctx.Height);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_BattlerPartialDrawTest, ctx);
    }

    // OFFSET 0x02229454
    private static int GetBattlerFromBattlerFlags(BattleAnimSystem system, int battlerFlags)
    {
        int battler = 0;
        switch (battlerFlags)
        {
            case AnimAttacker:
                battler = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
                break;
            case AnimAttackerPartner:
                battler = BattleAnimUtil.BattleAnimUtil_GetAlliedBattler(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
                break;
            case AnimDefender:
                battler = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
                break;
            case AnimDefenderPartner:
                battler = BattleAnimUtil.BattleAnimUtil_GetAlliedBattler(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system));
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return battler;
    }

    private static void PartialDrawCleanup(BattlerPartialDrawContext ctx, SysTask task)
    {
        PokemonSprite sprite = ctx.SpriteInfo[0].MonSprite!;
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpritePartialDraw, 0);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteYCenter, ctx.BaseY);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteDrawHeight, ctx.Height);
        ResetMonScale(sprite);
        BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
    }

    // OFFSET 0x022294AC
    private static void BattleAnimTask_BattlerPartialDrawAppear(SysTask task, object? param)
    {
        BattlerPartialDrawContext ctx = (BattlerPartialDrawContext)param!;
        PokemonSprite sprite = ctx.SpriteInfo[0].MonSprite!;
        switch (ctx.Common.State)
        {
            case 0:
                if (++ctx.Timer < ctx.StepInterval)
                {
                    break;
                }
                ctx.Timer = 0;
                ctx.Y += ctx.Step;
                PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteYCenter, ctx.Y);
                ctx.YOffset -= ctx.Step;
                if (ctx.YOffset > ctx.Height)
                {
                    ctx.YOffset = ctx.Height;
                    ctx.Common.State++;
                }
                PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteDrawHeight, ctx.YOffset);
                break;
            case 1:
                PartialDrawCleanup(ctx, task);
                break;
        }
    }

    // OFFSET 0x02229538
    private static void BattleAnimTask_BattlerPartialDrawDisappear(SysTask task, object? param)
    {
        BattlerPartialDrawContext ctx = (BattlerPartialDrawContext)param!;
        PokemonSprite sprite = ctx.SpriteInfo[0].MonSprite!;
        switch (ctx.Common.State)
        {
            case 0:
                if (++ctx.Timer < ctx.StepInterval)
                {
                    break;
                }
                ctx.Timer = 0;
                ctx.Y += ctx.Step;
                PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteYCenter, ctx.Y);
                ctx.YOffset -= ctx.Step;
                if (ctx.YOffset < 0)
                {
                    ctx.YOffset = 0;
                    ctx.Common.State++;
                }
                PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteDrawHeight, ctx.YOffset);
                break;
            case 1:
                PartialDrawCleanup(ctx, task);
                break;
        }
    }

    // OFFSET 0x022295C4
    private static void BattleAnimTask_BattlerPartialDrawSketch(SysTask task, object? param)
    {
        BattlerPartialDrawContext ctx = (BattlerPartialDrawContext)param!;
        PokemonSprite sprite = ctx.SpriteInfo[0].MonSprite!;
        switch (ctx.Common.State)
        {
            case 0:
                if (++ctx.Timer < ctx.StepInterval)
                {
                    break;
                }
                ctx.Timer = 0;
                ctx.YOffset -= ctx.Step;
                ctx.Height += ctx.Step;
                if (ctx.YOffset < 0)
                {
                    ctx.YOffset = 0;
                    ctx.Common.State++;
                }
                PokemonSprite.PokemonSprite_SetPartialDraw(sprite, 0, ctx.YOffset, MonSpriteFrameWidth, ctx.Height);
                break;
            case 1:
                PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpritePartialDraw, 0);
                ResetMonScale(sprite);
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
                break;
        }
    }

    // OFFSET 0x02229638
    private static void BattleAnimScriptFunc_BattlerPartialDraw(BattleAnimSystem system)
    {
        BattlerPartialDrawContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<BattlerPartialDrawContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.Step = (short)ScriptVar(system, 3);
        ctx.StepInterval = ScriptVar(system, 4);
        int target = ScriptVar(system, 0);
        BattleAnimUtil.BattleAnimUtil_GetBattlerSprites(system, target, ctx.SpriteInfo, out _);
        int battler = GetBattlerFromBattlerFlags(system, target);
        PokemonSprite sprite = ctx.SpriteInfo[0].MonSprite!;
        if (ScriptVar(system, 5) != 0)
        {
            ctx.YOffset = BattleAnimSystems.BattleAnimSystem_GetBattlerSpriteOffset(system, battler);
            ctx.YOffset = MonSpriteFrameHeight - ctx.YOffset;
            ctx.Height = 0;
            PokemonSprite.PokemonSprite_SetPartialDraw(sprite, 0, ctx.YOffset, MonSpriteFrameWidth, ctx.Height);
            BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_BattlerPartialDrawSketch, ctx);
            return;
        }
        ctx.Y = PokemonSprite.PokemonSprite_GetAttribute(sprite, MonSpriteYCenter);
        ctx.BaseY = ctx.Y;
        ctx.YOffset = BattleAnimSystems.BattleAnimSystem_GetBattlerSpriteOffset(system, battler);
        ctx.YOffset = MonSpriteFrameHeight - ctx.YOffset;
        ctx.Height = ctx.YOffset;
        if (ctx.Step > 0)
        {
            PokemonSprite.PokemonSprite_SetPartialDraw(sprite, 0, 0, MonSpriteFrameWidth, ctx.YOffset);
            BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_BattlerPartialDrawDisappear, ctx);
        }
        else
        {
            ctx.YOffset = 0;
            ctx.Y += ctx.Height;
            PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteYCenter, ctx.Y);
            PokemonSprite.PokemonSprite_SetPartialDraw(sprite, 0, 0, MonSpriteFrameHeight, ctx.YOffset);
            BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_BattlerPartialDrawAppear, ctx);
        }
    }

    // OFFSET 0x02229738
    private static void BattleAnimTask_ShakeBg(SysTask task, object? param)
    {
        ShakeBgContext ctx = (ShakeBgContext)param!;
        switch (ctx.State)
        {
            case 0:
                BattleAnimUtil.ShakeContext_Init(ctx.Shake, (short)ctx.ExtentX, (short)ctx.ExtentY, ctx.Interval, ctx.Amount);
                ctx.State++;
                goto case 1;
            case 1:
                if (BattleAnimUtil.ShakeContext_Update(ctx.Shake) == 0)
                {
                    if (ctx.Iteration >= ctx.Cycles)
                    {
                        ctx.State++;
                    }
                    else
                    {
                        ctx.State--;
                    }
                    if (ctx.ResetX != 0)
                    {
                        Bg.Bg_SetOffset(ctx.BgConfig!, ctx.BgLayer, 0, 0);
                    }
                    if (ctx.ResetY != 0)
                    {
                        Bg.Bg_SetOffset(ctx.BgConfig!, ctx.BgLayer, 3, 0);
                    }
                    ctx.Iteration++;
                }
                else
                {
                    if (ctx.Shake.X != 0)
                    {
                        ctx.ResetX = 1;
                        Bg.Bg_SetOffset(ctx.BgConfig!, ctx.BgLayer, 0, ctx.Shake.X);
                    }
                    if (ctx.Shake.Y != 0)
                    {
                        ctx.ResetY = 1;
                        Bg.Bg_SetOffset(ctx.BgConfig!, ctx.BgLayer, 3, ctx.Shake.Y);
                    }
                }
                break;
            default:
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.BattleAnimSys!, task);
                break;
        }
    }

    // OFFSET 0x02229810
    private static void BattleAnimScriptFunc_ShakeBg(BattleAnimSystem system)
    {
        ShakeBgContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<ShakeBgContext>(system);
        ctx.State = 0;
        ctx.Iteration = 0;
        ctx.BattleAnimSys = system;
        ctx.PokemonSpriteManager = BattleAnimSystems.BattleAnimSystem_GetPokemonSpriteManager(system);
        ctx.BgConfig = BattleAnimSystems.BattleAnimSystem_GetBgConfig(system);
        ctx.ExtentX = ScriptVar(system, 0);
        ctx.ExtentY = ScriptVar(system, 1);
        ctx.Interval = ScriptVar(system, 2);
        ctx.Amount = ScriptVar(system, 3);
        ctx.Cycles = ScriptVar(system, 4);
        ctx.BgLayer = ScriptVar(system, 5) == ShakeBgTargetEffect ? BattleBgEffect : BattleBgBase;
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_ShakeBg, ctx);
    }

    // OFFSET 0x0222988C
    private static void BattleAnimTask_PixelatePokemonSprite(SysTask task, object? param)
    {
        PixelatePokemonSpriteContext ctx = (PixelatePokemonSpriteContext)param!;
        if (ctx.ScaleX == ctx.TargetScale || ctx.ScaleY == ctx.TargetScale)
        {
            BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
            return;
        }
        if (ctx.ScaleStep < 0)
        {
            if (ctx.ScaleX + ctx.ScaleStep > PixelateMinScale)
            {
                ctx.ScaleX += ctx.ScaleStep;
                ctx.ScaleY += ctx.ScaleStep;
            }
            else
            {
                ctx.ScaleX = ctx.TargetScale;
                ctx.ScaleY = ctx.TargetScale;
            }
        }
        else if (ctx.ScaleX + ctx.ScaleStep < PixelateMaxScale)
        {
            ctx.ScaleX += ctx.ScaleStep;
            ctx.ScaleY += ctx.ScaleStep;
        }
        else
        {
            ctx.ScaleX = ctx.TargetScale;
            ctx.ScaleY = ctx.TargetScale;
        }
        SpriteSystem.ManagedSprite_TickFrame(ctx.Sprite!);
        Video.SetObjMosaicSize(0, ctx.ScaleX, ctx.ScaleY);
        SpriteSystem.SpriteSystem_DrawSprites(ctx.Common.PokemonSpriteManager);
    }

    // OFFSET 0x02229908
    private static void BattleAnimScriptFunc_PixelatePokemonSprite(BattleAnimSystem system)
    {
        PixelatePokemonSpriteContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<PixelatePokemonSpriteContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.ScaleStep = (short)ScriptVar(system, 1);
        ctx.ScaleX = ScriptVar(system, 2);
        ctx.ScaleY = ScriptVar(system, 3);
        ctx.Sprite = BattleAnimSystems.BattleAnimSystem_GetPokemonSprite(system, ScriptVar(system, 0));
        SpriteSystem.ManagedSprite_SetDrawFlag(ctx.Sprite, 1);
        ctx.TargetScale = ctx.ScaleStep < PixelateMinScale ? PixelateMinScale : PixelateMaxScale;
        SpriteSystem.ManagedSprite_SetMosaicFlag(ctx.Sprite, 1);
        SysTask task = BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_PixelatePokemonSprite, ctx);
        BattleAnimTask_PixelatePokemonSprite(task, ctx);
    }

    // OFFSET 0x02229980
    private static void EmitterAnimationContext_Init(BattleAnimSystem system, EmitterAnimationContext ctx)
    {
        ctx.EmitterID = ScriptVar(system, 0);
        ctx.OffsetX = (short)ScriptVar(system, 1);
        ctx.OffsetY = (short)ScriptVar(system, 2);
        ctx.StartDelay = (short)ScriptVar(system, 3);
        ctx.Frames = (short)ScriptVar(system, 4);
        ctx.Radius = ScriptVar(system, 5);
        ctx.Mode = ScriptVar(system, 6);
        ctx.Curve = ScriptVar(system, 8);
        int parameters = ScriptVar(system, 7);
        ctx.Frame = 0;
        ctx.SkipFrames = (int)(((uint)parameters & 0xFFFF0000) >> 16);
        ctx.MaxFrames = parameters & 0xFFFF;
        if (ctx.MaxFrames == 0)
        {
            ctx.MaxFrames = EmitterDefaultFrames;
        }
        ctx.Emitter = BattleAnimSystems.BattleAnimSystem_GetEmitter(system, ctx.EmitterID);
        ctx.ParticleSys = BattleAnimSystems.BattleAnimSystem_GetCurrentParticleSystem(system);
        if (ctx.Mode == EmitterModeAtkToDef)
        {
            ctx.StartBattler = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
            ctx.EndBattler = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
        }
        else if (ctx.Mode == EmitterModeDefToAtk)
        {
            ctx.StartBattler = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
            ctx.EndBattler = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
        }
        if (ctx.Emitter == null)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02229A50
    private static int IsEmitterActive(SplEmitter emitter)
    {
        int active = 0;
        SplParticle? particle = emitter.Particles.First;
        int age = 0;
        int lifeTime = 0;
        while (particle != null)
        {
            age = particle.Age;
            lifeTime = particle.LifeTime;
            particle = particle.Next;
        }
        if (age != lifeTime)
        {
            active = 1;
        }
        return active;
    }

    private static void SetEmitterXY(SplEmitter emitter, int x, int y)
    {
        emitter.Position.X = x;
        emitter.Position.Y = y;
    }

    // OFFSET 0x02229A6C
    private static void BattleAnimTask_MoveEmitterA2BLinear(SysTask task, object? param)
    {
        EmitterAnimationContext ctx = (EmitterAnimationContext)param!;
        int emitterActive = IsEmitterActive(ctx.Emitter!);
        if (ctx.Timer < ctx.StartDelay)
        {
            ctx.Timer++;
            return;
        }
        if (BattleAnimUtil.PosLerpContext_Update(ctx.Transforms[0]) == 0 && emitterActive == 0)
        {
            ParticleSystem.ParticleSystem_DeleteEmitter(ctx.ParticleSys!, ctx.Emitter!);
            BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
            BattleAnimUtil.BattleAnimUtil_Free(ctx);
            return;
        }
        if ((ctx.SkipFrames > ctx.Frame || ctx.MaxFrames < ctx.Frame) && ctx.MaxFrames != EmitterDefaultFrames)
        {
            return;
        }
        SetEmitterXY(ctx.Emitter!, ctx.Transforms[0].X * ParticlePixelFactor, ctx.Transforms[0].Y * ParticlePixelFactor);
        if (ctx.Curve != 0)
        {
            ctx.Angle += 360 / ctx.Frames;
            ctx.Emitter!.Position.Y = ctx.Transforms[0].Y * ParticlePixelFactor + MathUtil.CalcSineDegrees_Wraparound((ushort)ctx.Angle);
        }
    }

    private static void FinishEmitterSetup(EmitterAnimationContext ctx, SysTaskFunc task, Func<XYTransformContext, XYTransformContext, int> skip)
    {
        for (int i = 0; i < ctx.SkipFrames; i++)
        {
            skip(ctx.Transforms[0], ctx.Transforms[1]);
        }
        if (ctx.MaxFrames != EmitterDefaultFrames)
        {
            ctx.Frame = ctx.MaxFrames + 1;
        }
        SetEmitterXY(ctx.Emitter!, ctx.Transforms[0].X * ParticlePixelFactor, ctx.Transforms[0].Y * ParticlePixelFactor);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(ctx.Common.BattleAnimSys!, task, ctx);
    }

    // OFFSET 0x02229B28
    private static void BattleAnimScriptFunc_MoveEmitterA2BLinear(BattleAnimSystem system)
    {
        EmitterAnimationContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<EmitterAnimationContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        EmitterAnimationContext_Init(system, ctx);
        int dir = BattleAnimUtil.BattleAnimUtil_GetTransformDirectionX(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
        BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, ctx.StartBattler, out VecFx32 startPos);
        BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, ctx.EndBattler, out VecFx32 endPos);
        BattleAnimUtil.PosLerpContext_Init(ctx.Transforms[0], (short)(startPos.X / ParticlePixelFactor), (short)(endPos.X / ParticlePixelFactor + ctx.OffsetX * dir), (short)(startPos.Y / ParticlePixelFactor), (short)(endPos.Y / ParticlePixelFactor + ctx.OffsetY * dir), (ushort)ctx.Frames);
        if (dir > 0)
        {
            BattleAnimUtil.ValueLerpContext_Init(ctx.Unused, DegToIdx(20) * dir, DegToIdx(130) * dir, 10);
        }
        else
        {
            BattleAnimUtil.ValueLerpContext_Init(ctx.Unused, DegToIdx(90) * dir, DegToIdx(130) * dir, 10);
        }
        FinishEmitterSetup(ctx, BattleAnimTask_MoveEmitterA2BLinear, (a, b) => BattleAnimUtil.PosLerpContext_Update(a));
    }

    // OFFSET 0x02229C5C
    private static void BattleAnimScriptFunc_MoveEmitterViewportTop(BattleAnimSystem system)
    {
        EmitterAnimationContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<EmitterAnimationContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.EmitterID = ScriptVar(system, 0);
        ctx.Mode = ScriptVar(system, 1);
        int type = ScriptVar(system, 2);
        ctx.Frames = (short)ScriptVar(system, 3);
        ctx.StartDelay = (short)ScriptVar(system, 4);
        ctx.Timer = 0;
        int parameters = ScriptVar(system, 5);
        ctx.Frame = 0;
        ctx.SkipFrames = (int)(((uint)parameters & 0xFFFF0000) >> 16);
        ctx.MaxFrames = parameters & 0xFFFF;
        if (ctx.MaxFrames == 0)
        {
            ctx.MaxFrames = EmitterDefaultFrames;
        }
        ctx.Emitter = BattleAnimSystems.BattleAnimSystem_GetEmitter(system, ctx.EmitterID);
        ctx.ParticleSys = BattleAnimSystems.BattleAnimSystem_GetCurrentParticleSystem(system);
        if (ctx.Mode == EmitterModeAtkToDef)
        {
            ctx.StartBattler = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
            ctx.EndBattler = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
        }
        else
        {
            ctx.StartBattler = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
            ctx.EndBattler = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
        }
        if (ctx.Emitter == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, ctx.StartBattler, out VecFx32 startPos);
        BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, ctx.EndBattler, out VecFx32 endPos);
        if (type == EmitterFromTop)
        {
            BattleAnimUtil.BattleAnimUtil_GetParticleViewportTopPosition(out startPos);
            startPos.X = endPos.X;
        }
        else
        {
            BattleAnimUtil.BattleAnimUtil_GetParticleViewportTopPosition(out endPos);
            endPos.X = startPos.X;
        }
        BattleAnimUtil.PosLerpContext_Init(ctx.Transforms[0], (short)(startPos.X / ParticlePixelFactor), (short)(endPos.X / ParticlePixelFactor), (short)(startPos.Y / ParticlePixelFactor), (short)(endPos.Y / ParticlePixelFactor), (ushort)ctx.Frames);
        FinishEmitterSetup(ctx, BattleAnimTask_MoveEmitterA2BLinear, (a, b) => BattleAnimUtil.PosLerpContext_Update(a));
    }

    // OFFSET 0x02229DF0
    private static void BattleAnimTask_MoveEmitterA2BParabolic(SysTask task, object? param)
    {
        EmitterAnimationContext ctx = (EmitterAnimationContext)param!;
        int emitterActive = IsEmitterActive(ctx.Emitter!);
        if (BattleAnimUtil.ParabolicContext_Update(ctx.Transforms[0], ctx.Transforms[1]) == 0 && emitterActive == 0)
        {
            ParticleSystem.ParticleSystem_DeleteEmitter(ctx.ParticleSys!, ctx.Emitter!);
            BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
            BattleAnimUtil.BattleAnimUtil_Free(ctx);
            return;
        }
        SetEmitterXY(ctx.Emitter!, ctx.Transforms[0].X * ParticlePixelFactor, ctx.Transforms[0].Y * ParticlePixelFactor);
    }

    // OFFSET 0x02229E54
    private static void BattleAnimScriptFunc_MoveEmitterA2BParabolic(BattleAnimSystem system)
    {
        EmitterAnimationContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<EmitterAnimationContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        EmitterAnimationContext_Init(system, ctx);
        int dir = BattleAnimUtil.BattleAnimUtil_GetTransformDirectionX(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
        BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, ctx.StartBattler, out VecFx32 startPos);
        BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, ctx.EndBattler, out VecFx32 endPos);
        BattleAnimUtil.ParabolicContext_Init(ctx.Transforms[0], ctx.Transforms[1], (short)(startPos.X / ParticlePixelFactor), (short)(endPos.X / ParticlePixelFactor + ctx.OffsetX * dir), (short)(startPos.Y / ParticlePixelFactor), (short)(endPos.Y / ParticlePixelFactor + ctx.OffsetY * dir), (ushort)ctx.Frames, ctx.Radius * -Fx32.One);
        if (dir > 0)
        {
            BattleAnimUtil.ValueLerpContext_Init(ctx.Unused, DegToIdx(20) * dir, DegToIdx(130) * dir, 10);
        }
        else
        {
            BattleAnimUtil.ValueLerpContext_Init(ctx.Unused, DegToIdx(90) * dir, DegToIdx(130) * dir, 10);
        }
        FinishEmitterSetup(ctx, BattleAnimTask_MoveEmitterA2BParabolic, (a, b) => BattleAnimUtil.ParabolicContext_Update(a, b));
    }

    // OFFSET 0x02229F9C
    private static void BattleAnimTask_RevolveEmitter(SysTask task, object? param)
    {
        EmitterRevolutionContext ctx = (EmitterRevolutionContext)param!;
        int emitterActive = IsEmitterActive(ctx.Emitter!);
        if (BattleAnimUtil.RevolutionContext_Update(ctx.Revs) == 0 && emitterActive == 0)
        {
            ParticleSystem.ParticleSystem_DeleteEmitter(ctx.ParticleSys!, ctx.Emitter!);
            BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
            return;
        }
        SetEmitterXY(ctx.Emitter!, ctx.BattlerPos.X + ctx.Revs.X * ParticlePixelFactor, ctx.BattlerPos.Y + ctx.Revs.Y * ParticlePixelFactor);
    }

    // OFFSET 0x0222A00C
    private static void BattleAnimScriptFunc_RevolveEmitter(BattleAnimSystem system)
    {
        EmitterRevolutionContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<EmitterRevolutionContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.EmitterID = ScriptVar(system, 0);
        ctx.Sx = ScriptVar(system, 1);
        ctx.Ex = ScriptVar(system, 2);
        ctx.Sy = ScriptVar(system, 3);
        ctx.Ey = ScriptVar(system, 4);
        ctx.Rx = ScriptVar(system, 5);
        ctx.Ry = ScriptVar(system, 6);
        ctx.Frames = ScriptVar(system, 7);
        ctx.Mode = ScriptVar(system, 8);
        ctx.Battler = ctx.Mode == EmitterRevolutionModeAttacker ? BattleAnimSystems.BattleAnimSystem_GetAttacker(system) : BattleAnimSystems.BattleAnimSystem_GetDefender(system);
        BattleAnimUtil.BattleAnimUtil_GetBattlerWorldPos_Normal(system, ctx.Battler, out ctx.BattlerPos);
        ctx.Emitter = BattleAnimSystems.BattleAnimSystem_GetEmitter(system, ctx.EmitterID);
        ctx.ParticleSys = BattleAnimSystems.BattleAnimSystem_GetParticleSystem(system, ScriptVar(system, 9));
        BattleAnimUtil.RevolutionContext_Init(ctx.Revs, DegToIdx(ctx.Sx), DegToIdx(ctx.Ex), DegToIdx(ctx.Sy), DegToIdx(ctx.Ey), ctx.Rx * Fx32.One, ctx.Ry * Fx32.One, ctx.Frames);
        BattleAnimUtil.RevolutionContext_Update(ctx.Revs);
        SetEmitterXY(ctx.Emitter!, ctx.BattlerPos.X + ctx.Revs.X * ParticlePixelFactor, ctx.BattlerPos.Y + ctx.Revs.Y * ParticlePixelFactor);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_RevolveEmitter, ctx);
    }

    // OFFSET 0x0222A178
    private static void BattleAnimTask_OffsetAndAnimateSprite(SysTask task, object? param)
    {
        SpriteAnimContext ctx = (SpriteAnimContext)param!;
        if (!SpriteSystem.ManagedSprite_IsAnimated(ctx.Sprite!))
        {
            SpriteSystem.Sprite_DeleteAndFreeResources(ctx.Sprite!);
            BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
            return;
        }
        SpriteSystem.ManagedSprite_TickFrame(ctx.Sprite!);
        SpriteSystem.SpriteSystem_DrawSprites(ctx.Common.PrimarySpriteManager);
    }

    // OFFSET 0x0222A1AC
    private static void BattleAnimSpriteFunc_OffsetAndAnimate(BattleAnimSystem system, SpriteSystem spriteSys, SpriteManager spriteMan, ManagedSprite sprite)
    {
        SpriteAnimContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<SpriteAnimContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.Sprite = sprite;
        short x = (short)ScriptVar(system, 0);
        short y = (short)ScriptVar(system, 1);
        SpriteSystem.ManagedSprite_OffsetPositionXY(sprite, x, y);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_OffsetAndAnimateSprite, ctx);
    }

    // OFFSET 0x0222A1F4
    private static void BattleAnimTask_FollowMeSprite(SysTask task, object? param)
    {
        SpriteAnimContext ctx = (SpriteAnimContext)param!;
        switch (ctx.Common.State)
        {
            case 0:
            {
                short offsetX = (short)RomData.OverlayU16(Overlay, (uint)(FollowMeOffsetsAddress + ctx.State * 2), 1)[0];
                ctx.State++;
                if (offsetX != FollowMeOffsetEnd)
                {
                    if (offsetX == FollowMeOffsetSwitchPrio)
                    {
                        int explicitPrio = SpriteSystem.ManagedSprite_GetExplicitPriority(ctx.Sprite!);
                        if (explicitPrio == BattleAnimSystems.BattleAnimSystem_GetPokemonSpritePriority(ctx.Common.BattleAnimSys!))
                        {
                            SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.Sprite!, BattleAnimSystems.BattleAnimSystem_GetBgPriority(ctx.Common.BattleAnimSys!, 2));
                        }
                        else
                        {
                            SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.Sprite!, BattleAnimSystems.BattleAnimSystem_GetPokemonSpritePriority(ctx.Common.BattleAnimSys!));
                        }
                    }
                    SpriteSystem.ManagedSprite_OffsetPositionXY(ctx.Sprite!, offsetX, 0);
                }
                else
                {
                    SpriteSystem.ManagedSprite_SetAffineOverwriteMode(ctx.Sprite!, AffineOverwriteDouble);
                    BattleAnimUtil.ValueLerpContext_Init(ctx.Tilt, 0, FollowMeTiltAngle, FollowMeTiltFrames);
                    SpriteSystem.ManagedSprite_SetAffineTranslation(ctx.Sprite!, -8, 16);
                    ctx.State = 0;
                    ctx.Common.State++;
                }
                break;
            }
            case 1:
                if (BattleAnimUtil.ValueLerpContext_Update(ctx.Tilt) == 1)
                {
                    SpriteSystem.ManagedSprite_SetAffineZRotation(ctx.Sprite!, (ushort)ctx.Tilt.Value);
                }
                else if (ctx.State > 5)
                {
                    ctx.Common.State++;
                }
                else
                {
                    ctx.State++;
                    switch (ctx.State)
                    {
                        case 1:
                        case 3:
                            BattleAnimUtil.ValueLerpContext_Init(ctx.Tilt, FollowMeTiltAngle, -FollowMeTiltAngle, FollowMeTiltFrames);
                            break;
                        case 2:
                        case 4:
                            BattleAnimUtil.ValueLerpContext_Init(ctx.Tilt, -FollowMeTiltAngle, FollowMeTiltAngle, FollowMeTiltFrames);
                            break;
                        case 5:
                            BattleAnimUtil.ValueLerpContext_Init(ctx.Tilt, FollowMeTiltAngle, 0, FollowMeTiltFrames / 2);
                            break;
                    }
                }
                break;
            default:
                SpriteSystem.Sprite_DeleteAndFreeResources(ctx.Sprite!);
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
                return;
        }
        SpriteSystem.ManagedSprite_TickFrame(ctx.Sprite!);
        SpriteSystem.SpriteSystem_DrawSprites(ctx.Common.PrimarySpriteManager);
    }

    // OFFSET 0x0222A34C
    private static void BattleAnimSpriteFunc_FollowMe(BattleAnimSystem system, SpriteSystem spriteSys, SpriteManager spriteMan, ManagedSprite sprite)
    {
        SpriteAnimContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<SpriteAnimContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.Sprite = sprite;
        ctx.State = 0;
        int defender = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
        short defenderX = BattleAnimUtil.BattleAnimUtil_GetBattlerPos(system, defender, 0);
        short defenderY = BattleAnimUtil.BattleAnimUtil_GetBattlerPos(system, defender, 1);
        SpriteSystem.ManagedSprite_SetPositionXY(ctx.Sprite, defenderX, defenderY);
        SpriteSystem.ManagedSprite_SetAnim(ctx.Sprite, BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system)) == BattlerSideEnemy ? 1u : 0u);
        SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.Sprite, BattleAnimSystems.BattleAnimSystem_GetPokemonSpritePriority(system));
        SpriteSystem.ManagedSprite_OffsetPositionXY(ctx.Sprite, 0, 12);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_FollowMeSprite, ctx);
    }

    // OFFSET 0x0222A3DC
    private static void BattleAnimTask_FissureSprite(SysTask task, object? param)
    {
        BattleAnimTask_OffsetAndAnimateSprite(task, param);
    }

    // OFFSET 0x0222A410
    private static void BattleAnimSpriteFunc_Fissure(BattleAnimSystem system, SpriteSystem spriteSys, SpriteManager spriteMan, ManagedSprite sprite)
    {
        SpriteAnimContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<SpriteAnimContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.Sprite = sprite;
        int anim;
        int defender = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
        short spriteX = BattleAnimUtil.BattleAnimUtil_GetBattlerPos(system, defender, 0);
        short spriteY;
        BattleAnimUtil.BattleAnimUtil_GetBattlerPos(system, defender, 1);
        if (BattleAnimSystems.BattleAnimSystem_IsContest(system) == 1)
        {
            spriteX = FissureContestX;
            spriteY = FissureContestY;
            anim = 0;
        }
        else if (BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, defender) == BattlerSidePlayer)
        {
            spriteY = FissurePlayerY;
            anim = 1;
        }
        else
        {
            spriteY = FissureEnemyY;
            anim = 0;
        }
        SpriteSystem.ManagedSprite_SetPositionXY(ctx.Sprite, spriteX, spriteY);
        SpriteSystem.ManagedSprite_SetAnim(ctx.Sprite, (uint)anim);
        SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.Sprite, BattleAnimSystems.BattleAnimSystem_GetBgPriority(system, 2));
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_FissureSprite, ctx);
    }

    // OFFSET 0x0222A4A0
    private static void BattleAnimTask_EscapeItemSprite(SysTask task, object? param)
    {
        SpriteAnimContext ctx = (SpriteAnimContext)param!;
        switch (ctx.Common.State)
        {
            case 0:
                BattleAnimUtil.ParabolicContext_Init(ctx.Pos[0], ctx.Pos[1], EscapeItemOffsetX, EscapeItemDestX, EscapeItemOffsetY, EscapeItemDestY, EscapeItemMoveFrames, EscapeItemMoveRadius);
                ctx.Common.State++;
                break;
            case 1:
                if (BattleAnimUtil.ParabolicContext_UpdateAndApplyToSprite(ctx.Pos[0], ctx.Pos[1], ctx.Sprite!) == 0)
                {
                    ctx.Common.State++;
                }
                break;
            case 2:
                if (!SpriteSystem.ManagedSprite_IsAnimated(ctx.Sprite!))
                {
                    SpriteSystem.ManagedSprite_SetExplicitOamMode(ctx.Sprite!, OamModeXlu);
                    ctx.SpriteAlpha = EscapeItemMaxAlpha;
                    ctx.BgAlpha = EscapeItemMinAlpha;
                    BattleAnimUtil.BattleAnimUtil_SetSpriteBgBlending(ctx.Common.BattleAnimSys!, ctx.SpriteAlpha, ctx.BgAlpha);
                    ctx.Common.State++;
                }
                break;
            case 3:
                if (ctx.SpriteAlpha > EscapeItemMinAlpha)
                {
                    ctx.SpriteAlpha--;
                }
                if (ctx.BgAlpha < EscapeItemMaxAlpha)
                {
                    ctx.BgAlpha++;
                }
                ChangeBlendAlpha(ctx.SpriteAlpha, ctx.BgAlpha);
                if (ctx.SpriteAlpha == EscapeItemMinAlpha)
                {
                    ctx.Common.State++;
                }
                break;
            default:
                SpriteSystem.Sprite_DeleteAndFreeResources(ctx.Sprite!);
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
                return;
        }
        SpriteSystem.ManagedSprite_TickFrame(ctx.Sprite!);
        SpriteSystem.SpriteSystem_DrawSprites(ctx.Common.PrimarySpriteManager);
    }

    // OFFSET 0x0222A5C0
    private static void BattleAnimSpriteFunc_EscapeItem(BattleAnimSystem system, SpriteSystem spriteSys, SpriteManager spriteMan, ManagedSprite sprite)
    {
        SpriteAnimContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<SpriteAnimContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.Sprite = sprite;
        SpriteSystem.ManagedSprite_SetPositionXY(ctx.Sprite, EscapeItemOffsetX, EscapeItemOffsetY);
        SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.Sprite, BattleAnimSystems.BattleAnimSystem_GetBgPriority(system, 2));
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_EscapeItemSprite, ctx);
    }

    // OFFSET 0x0222A604
    private static void BattleAnimScriptFunc_SetBgGrayscale(BattleAnimSystem system)
    {
        if (ScriptVar(system, 0) != 0)
        {
            BattleAnimUtil.BattleAnimUtil_MakeBgPalsGrayscale(system);
        }
        else
        {
            BattleAnimUtil.BattleAnimUtil_ReturnBgPalsToNormal(system);
        }
    }

    private static void DarkVoidStep(SetPokemonSpritePriorityContext ctx, int required, int step, bool random)
    {
        if (random)
        {
            if (MathUtil.LCRNG_Next() % 2 != 0 && ctx.StepCount == required)
            {
                ctx.StepCount++;
                SpriteSystem.ManagedSprite_OffsetPositionXY(ctx.Sprite!, 0, (short)step);
            }
            return;
        }
        if (ctx.StepCount != required + 1)
        {
            ctx.StepCount++;
            SpriteSystem.ManagedSprite_OffsetPositionXY(ctx.Sprite!, 0, (short)step);
        }
    }

    // OFFSET 0x0222A624
    private static void SetPokemonSpritePriorityContext_DoDarkVoidEffects(SetPokemonSpritePriorityContext ctx)
    {
        switch (ctx.Common.State)
        {
            case 0:
                Video.SetVisibleWindows(0, 1);
                Video.SetWindowInside(0, 0, (byte)(WndPlaneBg0 | WndPlaneBg1 | WndPlaneBg2 | WndPlaneBg3 | 0x20));
                Video.SetWindowOutside(0, (byte)(WndPlaneBg0 | WndPlaneBg1 | WndPlaneBg2 | WndPlaneBg3 | WndPlaneObj | 0x20));
                if (ctx.WindowType == 0)
                {
                    Video.SetWindowHorizontal(0, 0, 0, 128);
                    Video.SetWindowVertical(0, 0, 160, 192);
                }
                else
                {
                    Video.SetWindowHorizontal(0, 0, 128, 0);
                    Video.SetWindowVertical(0, 0, 86, 192);
                }
                ctx.SinkFullyStartFrame = DarkVoidSinkMinFrame + MathUtil.LCRNG_Next() % DarkVoidSinkRngFrame;
                break;
            case 5:
            case 6:
                DarkVoidStep(ctx, 0, DarkVoidStepY, true);
                break;
            case 7:
                DarkVoidStep(ctx, 0, DarkVoidStepY, false);
                break;
            case 10:
            case 11:
                DarkVoidStep(ctx, 1, DarkVoidStepY, true);
                break;
            case 12:
                DarkVoidStep(ctx, 1, DarkVoidStepY, false);
                break;
            case 15:
            case 16:
                DarkVoidStep(ctx, 2, DarkVoidStepY, true);
                break;
            case 17:
                DarkVoidStep(ctx, 2, DarkVoidStepY, false);
                break;
            case 22:
            case 23:
                DarkVoidStep(ctx, 3, DarkVoidStepYL, true);
                break;
            case 24:
                DarkVoidStep(ctx, 3, DarkVoidStepYL, false);
                break;
            default:
                if (ctx.Common.State > ctx.SinkFullyStartFrame)
                {
                    if (ctx.StepCount < 20)
                    {
                        SpriteSystem.ManagedSprite_OffsetPositionXY(ctx.Sprite!, 0, DarkVoidStepY);
                        SpriteSystem.ManagedSprite_GetPositionXY(ctx.Sprite!, out _, out short y);
                        if (y > DarkVoidMaxY)
                        {
                            SpriteSystem.ManagedSprite_SetDrawFlag(ctx.Sprite!, 0);
                        }
                        ctx.StepCount++;
                    }
                    else
                    {
                        SpriteSystem.ManagedSprite_SetDrawFlag(ctx.Sprite!, 0);
                    }
                }
                break;
        }
    }

    // OFFSET 0x0222A878
    private static void BattleAnimTask_SetPokemonSpritePriority(SysTask task, object? param)
    {
        SetPokemonSpritePriorityContext ctx = (SetPokemonSpritePriorityContext)param!;
        if (ctx.Mode != PriorityModeDefault)
        {
            SetPokemonSpritePriorityContext_DoDarkVoidEffects(ctx);
        }
        ctx.Common.State++;
        if (ctx.Common.State >= ctx.MaxFrames)
        {
            Video.SetVisibleWindows(0, 0);
            Video.SetWindowInside(0, 0, 0);
            Video.SetWindowOutside(0, 0);
            Video.SetWindowHorizontal(0, 0, 0, 0);
            Video.SetWindowVertical(0, 0, 0, 0);
            SpriteSystem.ManagedSprite_SetDrawFlag(ctx.Sprite!, 0);
            SpriteSystem.SpriteSystem_DrawSprites(ctx.Common.PokemonSpriteManager);
            BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
            return;
        }
        SpriteSystem.ManagedSprite_TickFrame(ctx.Sprite!);
        SpriteSystem.SpriteSystem_DrawSprites(ctx.Common.PokemonSpriteManager);
    }

    private static void SetPriorityByType(ManagedSprite sprite, int type, int slot1Player, int slot1Enemy, int slot2Player, int slot2Enemy)
    {
        switch (type)
        {
            case 2:
                SpriteSystem.ManagedSprite_SetPriority(sprite, (uint)slot1Player);
                break;
            case 3:
                SpriteSystem.ManagedSprite_SetPriority(sprite, (uint)slot1Enemy);
                break;
            case 4:
                SpriteSystem.ManagedSprite_SetPriority(sprite, (uint)slot2Player);
                break;
            case 5:
                SpriteSystem.ManagedSprite_SetPriority(sprite, (uint)slot2Enemy);
                break;
        }
    }

    // OFFSET 0x0222A8F4
    private static void BattleAnimScriptFunc_SetPokemonSpritePriority(BattleAnimSystem system)
    {
        SetPokemonSpritePriorityContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<SetPokemonSpritePriorityContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.SpriteID = ScriptVar(system, 0);
        ctx.MaxFrames = ScriptVar(system, 1);
        ctx.Bg = ScriptVar(system, 2);
        ctx.SpritePriority = ScriptVar(system, 3);
        ctx.Mode = ScriptVar(system, 5);
        ctx.WindowType = ScriptVar(system, 6);
        ctx.Sprite = BattleAnimSystems.BattleAnimSystem_GetPokemonSprite(system, ctx.SpriteID);
        ctx.StepCount = 0;
        if (ctx.Bg != BattleAnimBgNone)
        {
            SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.Sprite, BattleAnimSystems.BattleAnimSystem_GetBgPriority(system, ctx.Bg));
        }
        if (ctx.SpritePriority != BattleAnimDefaultPriority)
        {
            SpriteSystem.ManagedSprite_SetPriority(ctx.Sprite, (uint)ctx.SpritePriority);
        }
        if (BattleAnimSystems.BattleAnimSystem_IsDoubleBattle(system) != 1 && (ScriptVar(system, 4) == RoleAttackerPartner || ScriptVar(system, 4) == RoleDefenderPartner))
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(ctx.Sprite, 0);
            return;
        }
        int battlerRole = ScriptVar(system, 4);
        int attacker = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
        int defender = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
        if (ctx.SpritePriority != BattleAnimDefaultPriority)
        {
            int attackerType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, attacker);
            int defenderType = BattleAnimUtil.BattleAnimUtil_GetBattlerType(system, defender);
            switch (battlerRole)
            {
                case RoleAttacker:
                    SetPriorityByType(ctx.Sprite, attackerType, 20, 10, 10, 20);
                    break;
                case RoleAttackerPartner:
                    SetPriorityByType(ctx.Sprite, attackerType, 10, 20, 20, 10);
                    break;
                case RoleDefender:
                    SetPriorityByType(ctx.Sprite, defenderType, 20, 10, 10, 20);
                    break;
                case RoleDefenderPartner:
                    SetPriorityByType(ctx.Sprite, defenderType, 10, 20, 20, 10);
                    break;
            }
        }
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_SetPokemonSpritePriority, ctx);
    }

    // OFFSET 0x0222AB10
    private static void BattleAnimTask_RenderPokemonSprites(SysTask task, object? param)
    {
        RenderPokemonSpritesContext ctx = (RenderPokemonSpritesContext)param!;
        if (ctx.Frames == 0)
        {
            BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
            return;
        }
        ctx.Frames--;
        for (int i = 0; i < 4; i++)
        {
            if (ctx.Sprites[i] == null)
            {
                continue;
            }
            SpriteSystem.ManagedSprite_TickFrame(ctx.Sprites[i]!);
        }
        SpriteSystem.SpriteSystem_DrawSprites(ctx.Common.PokemonSpriteManager);
    }

    // OFFSET 0x0222AB50
    private static void BattleAnimScriptFunc_RenderPokemonSprites(BattleAnimSystem system)
    {
        RenderPokemonSpritesContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<RenderPokemonSpritesContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.Frames = ScriptVar(system, 0) == 0 ? RenderDefaultFrames : ScriptVar(system, 0);
        uint[] priorityTable = RomData.OverlayU32(Overlay, RenderPriorityTableAddress, 4);
        for (int battler = 0; battler < 4; battler++)
        {
            ctx.Sprites[battler] = system.PokemonSprites[battler];
            if (ctx.Sprites[battler] != null)
            {
                SpriteSystem.ManagedSprite_SetPriority(ctx.Sprites[battler]!, priorityTable[battler]);
            }
        }
        SysTask task = BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_RenderPokemonSprites, ctx);
        BattleAnimTask_RenderPokemonSprites(task, ctx);
    }

    // OFFSET 0x0222ABBC
    private static void BattleAnimTask_ScrollSwitchedBg(SysTask task, object? param)
    {
        ScrollSwitchedBgContext ctx = (ScrollSwitchedBgContext)param!;
        switch (ctx.Common.State)
        {
            case 0:
                ctx.Timer = 0;
                ctx.BgScroll = BattleAnimUtil.BgScrollContext_New(0, 160, DegToIdx(1), 32 << 12, 200, BattleAnimSystems.BattleAnimSystem_GetBgID(ctx.Common.BattleAnimSys!, 2), 0, (uint)BattleAnimUtil.BattleAnimUtil_MakeBgOffsetValue(0, 0), BattleAnimSystems.BattleAnimSystem_GetHeapID(ctx.Common.BattleAnimSys!));
                ctx.Common.State++;
                break;
            case 1:
                ctx.Timer++;
                if (ctx.Timer < ctx.Frames)
                {
                    break;
                }
                BattleAnimUtil.BgScrollContext_Free(ctx.BgScroll!);
                ctx.Common.State++;
                goto default;
            default:
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
                return;
        }
    }

    // OFFSET 0x0222AC40
    private static void BattleAnimScriptFunc_ScrollSwitchedBg(BattleAnimSystem system)
    {
        ScrollSwitchedBgContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<ScrollSwitchedBgContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.Frames = ScriptVar(system, 0);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_ScrollSwitchedBg, ctx);
    }
}
