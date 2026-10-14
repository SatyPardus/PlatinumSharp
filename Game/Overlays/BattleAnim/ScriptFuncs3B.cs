namespace PlatinumSharp.Game;

public sealed class RotateMonContext
{
    public byte State;
    public BattleAnimSystem? System;
    public PokemonSprite? Sprite;
    public readonly ValueLerpContext LerpCtx = new();
    public int Start;
    public int Steps;
    public short CenterX;
    public short CenterY;
}

public sealed class SubmissionContext
{
    public BattleAnimSystem? BattleAnimSys;
    public readonly XYTransformContext Rev = new();
    public PokemonSprite? Sprite;
    public short BaseX;
    public short BaseY;
}

public sealed class SwaggerSpriteContext
{
    public BattleAnimSystem? BattleAnimSys;
    public SpriteSystem? SpriteSys;
    public SpriteManager? SpriteMan;
    public int State;
    public ManagedSprite? Sprite;
    public readonly XYTransformContext Scale = new();
    public int Dir;
    public int ScaleStep;
    public int Delay;
    public int PopState;
    public short DefenderX;
    public short DefenderY;
}

public sealed class FadeBgContext
{
    public BattleAnimSystem? BattleAnimSys;
    public PaletteData? PaletteData;
}

public sealed class FadeBattlerSpriteContext
{
    public BattleAnimSystem? BattleAnimSys;
    public PokemonSprite? Sprite;
    public int State;
    public int EndDelay;
    public int FadeDelay;
    public int Color;
    public int Alpha;
    public int HoldDelay;
}

public static partial class BattleAnimScriptFuncs
{
    private const int MonSpriteRotationZ = 9;
    private const int MonSpriteXPivot = 0xA;
    private const int MonSpriteYPivot = 0xB;
    private const int MonSpriteFrameHalf = 0x28;
    private const int OvalRadiusYPixels = -8;
    private const int SwaggerStartScale = 10;
    private const int SwaggerRefScale = 10;
    private const int SwaggerEndScale = 14;
    private const int SwaggerScaleFrames = 4;
    private const int SwaggerSettleScale = 12;
    private const int SwaggerSettleFrames = 2;
    private const int SwaggerPopOffsetX = 24;
    private const int SwaggerPop1OffsetY = -16;
    private const int SwaggerPop2OffsetY = -24;
    private const int SwaggerPopDelay = 4;
    private const int AffineOverwriteDouble = 2;
    private const ushort PaletteFlagMonSprite = 0x100;
    private const ushort PaletteFlagEffect = 0x200;
    private const ushort PlttBufMainBgF = 1;

    // OFFSET 0x02227390
    private static void BattleAnimTask_RotateMon(SysTask task, object? param)
    {
        RotateMonContext ctx = (RotateMonContext)param!;
        switch (ctx.State)
        {
            case 0:
            case 1:
                if (BattleAnimUtil.ValueLerpContext_Update(ctx.LerpCtx) == 1)
                {
                    PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteRotationZ, (ushort)ctx.LerpCtx.Value);
                }
                else
                {
                    BattleAnimUtil.ValueLerpContext_Init(ctx.LerpCtx, ctx.LerpCtx.Value, ctx.Start, ctx.Steps);
                    ctx.State++;
                }
                break;
            case 2:
                PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteXPivot, 0);
                PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteYPivot, 0);
                PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteRotationZ, 0);
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.System!, task);
                break;
        }
    }

    // OFFSET 0x02227408
    private static void BattleAnimScriptFunc_RotateMon(BattleAnimSystem system)
    {
        RotateMonContext ctx = new();
        ctx.State = 0;
        ctx.System = system;
        ctx.Steps = ScriptVar(system, 2);
        ctx.Start = ScriptVar(system, 0);
        int type = ScriptVar(system, 3);
        int battler = type == 0 || type == 2 ? BattleAnimSystems.BattleAnimSystem_GetDefender(system) : BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
        BattleAnimUtil.ValueLerpContext_Init(ctx.LerpCtx, ctx.Start, ScriptVar(system, 1), ctx.Steps);
        int dir = BattleAnimUtil.BattleAnimUtil_GetTransformDirectionX(system, battler);
        ctx.LerpCtx.Data[1] *= dir;
        if (type == 2)
        {
            dir = -dir;
        }
        ctx.Sprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, battler);
        if (ScriptVar(system, 3) == 1)
        {
            ctx.CenterX = (short)ScriptVar(system, 4);
            ctx.CenterY = (short)ScriptVar(system, 5);
        }
        else
        {
            ctx.CenterX = (short)(MonSpriteFrameHalf * dir);
            ctx.CenterY = MonSpriteFrameHalf;
        }
        PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteXPivot, ctx.CenterX);
        PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteYPivot, ctx.CenterY);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_RotateMon, ctx);
    }

    // OFFSET 0x022274E4
    private static void BattleAnimTask_Submission(SysTask task, object? param)
    {
        SubmissionContext ctx = (SubmissionContext)param!;
        if (BattleAnimUtil.RevolutionContext_UpdateAndApplyToMon(ctx.Rev, ctx.BaseX, ctx.BaseY, ctx.Sprite!) == 0)
        {
            PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteXCenter, ctx.BaseX);
            PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteYCenter, ctx.BaseY + OvalRadiusYPixels);
            PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteRotationZ, 0);
            BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.BattleAnimSys!, task);
        }
    }

    // OFFSET 0x02227534
    private static void BattleAnimScriptFunc_Submission(BattleAnimSystem system)
    {
        SubmissionContext ctx = new();
        ctx.BattleAnimSys = system;
        BattleAnimUtil.RevolutionContext_InitOvalRevolutions(ctx.Rev, ScriptVar(system, 0), ScriptVar(system, 1));
        switch (ScriptVar(system, 2))
        {
            case AnimAttacker:
                ctx.Sprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
                ctx.Rev.Data[2] = -ctx.Rev.Data[2];
                break;
            case AnimAttackerPartner:
                ctx.Sprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimUtil.BattleAnimUtil_GetAlliedBattler(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system)));
                ctx.Rev.Data[2] = -ctx.Rev.Data[2];
                break;
            case AnimDefender:
                ctx.Sprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system));
                break;
            case AnimDefenderPartner:
                ctx.Sprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimUtil.BattleAnimUtil_GetAlliedBattler(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system)));
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        ctx.BaseX = (short)PokemonSprite.PokemonSprite_GetAttribute(ctx.Sprite!, MonSpriteXCenter);
        ctx.BaseY = (short)PokemonSprite.PokemonSprite_GetAttribute(ctx.Sprite!, MonSpriteYCenter);
        ctx.BaseY -= OvalRadiusYPixels;
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_Submission, ctx);
    }

    // OFFSET 0x02227620
    private static void SwaggerSprite_StartPop(SwaggerSpriteContext ctx, short x, short y)
    {
        SpriteSystem.ManagedSprite_SetDrawFlag(ctx.Sprite!, 1);
        SpriteSystem.ManagedSprite_SetPositionXY(ctx.Sprite!, x, y);
        ctx.ScaleStep = 0;
        BattleAnimUtil.ScaleLerpContext_Init(ctx.Scale, SwaggerStartScale, SwaggerRefScale, SwaggerEndScale, SwaggerScaleFrames);
    }

    // OFFSET 0x02227658
    private static int SwaggerSprite_UpdatePop(SwaggerSpriteContext ctx)
    {
        if (BattleAnimUtil.ScaleLerpContext_UpdateAndApplyToSprite(ctx.Scale, ctx.Sprite!) == 0)
        {
            if (ctx.ScaleStep != 0)
            {
                SpriteSystem.ManagedSprite_SetDrawFlag(ctx.Sprite!, 0);
                return 1;
            }
            ctx.ScaleStep = 1;
            BattleAnimUtil.ScaleLerpContext_Init(ctx.Scale, SwaggerEndScale, SwaggerRefScale, SwaggerSettleScale, SwaggerSettleFrames);
        }
        return 0;
    }

    // OFFSET 0x0222769C
    private static int SwaggerSprite_UpdatePops(SwaggerSpriteContext ctx)
    {
        int done = 0;
        switch (ctx.PopState)
        {
            case 0:
                SwaggerSprite_StartPop(ctx, (short)(ctx.DefenderX + SwaggerPopOffsetX * ctx.Dir), (short)(ctx.DefenderY + SwaggerPop1OffsetY));
                ctx.PopState++;
                break;
            case 1:
                if (SwaggerSprite_UpdatePop(ctx) != 0)
                {
                    ctx.PopState++;
                }
                break;
            case 2:
                ctx.Delay--;
                if (ctx.Delay < 0)
                {
                    ctx.PopState++;
                    SwaggerSprite_StartPop(ctx, (short)(ctx.DefenderX - SwaggerPopOffsetX * ctx.Dir), (short)(ctx.DefenderY + SwaggerPop2OffsetY));
                }
                break;
            case 3:
                if (SwaggerSprite_UpdatePop(ctx) != 0)
                {
                    ctx.PopState++;
                }
                break;
            case 4:
                done = 1;
                break;
        }
        return done;
    }

    // OFFSET 0x02227738
    private static void BattleAnimTask_SwaggerSprite(SysTask task, object? param)
    {
        SwaggerSpriteContext ctx = (SwaggerSpriteContext)param!;
        switch (ctx.State)
        {
            case 0:
                if (SwaggerSprite_UpdatePops(ctx) != 0)
                {
                    ctx.State++;
                }
                break;
            case 1:
                SpriteSystem.Sprite_DeleteAndFreeResources(ctx.Sprite!);
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.BattleAnimSys!, task);
                return;
        }
        SpriteSystem.SpriteSystem_DrawSprites(ctx.SpriteMan);
    }

    // OFFSET 0x0222777C
    private static void BattleAnimSpriteFunc_Swagger(BattleAnimSystem system, SpriteSystem spriteSys, SpriteManager spriteMan, ManagedSprite sprite)
    {
        SwaggerSpriteContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<SwaggerSpriteContext>(system);
        ctx.BattleAnimSys = system;
        ctx.SpriteSys = spriteSys;
        ctx.SpriteMan = spriteMan;
        ctx.Sprite = sprite;
        SpriteSystem.ManagedSprite_SetPriority(ctx.Sprite, 100);
        SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.Sprite, 1);
        SpriteSystem.ManagedSprite_SetAffineOverwriteMode(ctx.Sprite, AffineOverwriteDouble);
        SpriteSystem.ManagedSprite_SetDrawFlag(ctx.Sprite, 0);
        ctx.Delay = SwaggerPopDelay;
        ctx.Dir = BattleAnimUtil.BattleAnimUtil_GetTransformDirectionX(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
        PokemonSprite monSprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system))!;
        ctx.DefenderX = (short)PokemonSprite.PokemonSprite_GetAttribute(monSprite, MonSpriteXCenter);
        ctx.DefenderY = (short)PokemonSprite.PokemonSprite_GetAttribute(monSprite, MonSpriteYCenter);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_SwaggerSprite, ctx);
    }

    // OFFSET 0x02227808
    private static void BattleAnimTask_FadeBg(SysTask task, object? param)
    {
        FadeBgContext ctx = (FadeBgContext)param!;
        if (PaletteData.PaletteData_GetSelectedBuffersMask(ctx.PaletteData!) == 0)
        {
            BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.BattleAnimSys!, task);
        }
    }

    // OFFSET 0x02227828
    private static void BattleAnimScriptFunc_FadeBg(BattleAnimSystem system)
    {
        FadeBgContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<FadeBgContext>(system);
        ctx.BattleAnimSys = system;
        ctx.PaletteData = BattleAnimSystems.BattleAnimSystem_GetPaletteData(system);
        ushort paletteToFade = 0;
        switch (ScriptVar(system, 0))
        {
            case 0:
                paletteToFade = (ushort)BattleAnimSystems.BattleAnimSystem_GetBaseBgPalettes(system);
                break;
            case 1:
                paletteToFade = PaletteFlagMonSprite;
                break;
            case 2:
                paletteToFade = PaletteFlagEffect;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        PaletteData.PaletteData_StartFade(ctx.PaletteData, PlttBufMainBgF, paletteToFade, (sbyte)ScriptVar(system, 1), (byte)ScriptVar(system, 2), (byte)ScriptVar(system, 3), (ushort)ScriptVar(system, 4));
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_FadeBg, ctx);
    }

    // OFFSET 0x022278D0
    private static void BattleAnimTask_FadeBattlerSprite(SysTask task, object? param)
    {
        FadeBattlerSpriteContext ctx = (FadeBattlerSpriteContext)param!;
        switch (ctx.State)
        {
            case 0:
                PokemonSprite.PokemonSprite_StartFade(ctx.Sprite!, 0, ctx.Alpha, ctx.FadeDelay, ctx.Color);
                ctx.State++;
                break;
            case 1:
                if (!PokemonSprite.PokemonSprite_IsFadeActive(ctx.Sprite!))
                {
                    ctx.State++;
                }
                break;
            case 2:
                if (ctx.HoldDelay != 0)
                {
                    ctx.HoldDelay--;
                    break;
                }
                ctx.State++;
                goto case 3;
            case 3:
                PokemonSprite.PokemonSprite_StartFade(ctx.Sprite!, ctx.Alpha, 0, ctx.FadeDelay, ctx.Color);
                ctx.State++;
                break;
            case 4:
                if (!PokemonSprite.PokemonSprite_IsFadeActive(ctx.Sprite!))
                {
                    ctx.EndDelay--;
                    ctx.State = ctx.EndDelay <= 0 ? ctx.State + 1 : 0;
                }
                break;
            case 5:
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.BattleAnimSys!, task);
                return;
        }
    }

    private static int FindBattlerOfType(BattleAnimSystem system, int typeA, int typeB)
    {
        int target = 0xFF;
        for (int battler = 0; battler < 4; battler++)
        {
            int type = BattleAnimSystems.BattleAnimSystem_GetBattlerType(system, battler);
            if (type == typeA || type == typeB)
            {
                target = battler;
                break;
            }
        }
        return target == 0xFF ? 0 : target;
    }

    private static PokemonSprite? GetTargetBattlerSprite(BattleAnimSystem system, int target)
    {
        switch (target)
        {
            case AnimAttacker:
                return BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
            case AnimAttackerPartner:
                if (BattleAnimSystems.BattleAnimSystem_IsDoubleBattle(system) == 1)
                {
                    return BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimUtil.BattleAnimUtil_GetAlliedBattler(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system)));
                }
                return null;
            case AnimDefender:
                return BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system));
            case AnimDefenderPartner:
                if (BattleAnimSystems.BattleAnimSystem_IsDoubleBattle(system) == 1)
                {
                    return BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimUtil.BattleAnimUtil_GetAlliedBattler(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system)));
                }
                return null;
            case AnimSpecificBattler | AnimAttacker:
                return BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, FindBattlerOfType(system, 0, 2));
            case AnimSpecificBattler | AnimAttackerPartner:
                return BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, FindBattlerOfType(system, 4, 4));
            case AnimSpecificBattler | AnimDefender:
                return BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, FindBattlerOfType(system, 1, 3));
            case AnimSpecificBattler | AnimDefenderPartner:
                return BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, FindBattlerOfType(system, 5, 5));
            default:
                ErrorHandling.GF_AssertFail();
                return null;
        }
    }

    // OFFSET 0x0222797C
    private static void BattleAnimScriptFunc_FadeBattlerSprite(BattleAnimSystem system)
    {
        FadeBattlerSpriteContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<FadeBattlerSpriteContext>(system);
        ctx.BattleAnimSys = system;
        ctx.Sprite = GetTargetBattlerSprite(system, ScriptVar(system, 0));
        if (ctx.Sprite == null)
        {
            return;
        }
        ctx.FadeDelay = ScriptVar(system, 1);
        ctx.EndDelay = ScriptVar(system, 2);
        ctx.Color = ScriptVar(system, 3);
        ctx.Alpha = ScriptVar(system, 4);
        ctx.HoldDelay = ScriptVar(system, 5);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_FadeBattlerSprite, ctx);
    }
}
