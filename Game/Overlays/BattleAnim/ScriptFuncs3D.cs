namespace PlatinumSharp.Game;

public sealed class ShakeAndScaleAttackerContext
{
    public byte State;
    public short SpriteY;
    public int SpriteHeight;
    public byte Timer;
    public byte HoldFrames;
    public int Scale2X;
    public int Scale2Y;
    public int Scale2Frames;
    public BattleAnimSystem? BattleAnimSys;
    public PokemonSprite? Sprite;
    public readonly XYTransformContext Scale = new();
    public readonly XYTransformContext Shake = new();
}

public sealed class SuperpowerContext
{
    public readonly BattleAnimScriptFuncCommon Common = new();
    public ManagedSprite? Sprite;
}

public sealed class MoveBattlerContext
{
    public int Frames;
    public readonly BattleAnimSpriteInfo SpriteInfo = new();
    public readonly BattleAnimScriptFuncCommon Common = new();
    public readonly XYTransformContext Pos = new();
    public BattleAnimSystem? BattleAnimSys;
    public short OffsetX;
    public short OffsetY;
}

public sealed class RevolveBattlerContext
{
    public readonly BattleAnimScriptFuncCommon Common = new();
    public readonly BattleAnimSpriteInfo[] SpriteInfo = { new(), new(), new(), new() };
    public readonly XYTransformContext Revs = new();
}

public sealed class MoveBattlerOffScreenContext
{
    public readonly BattleAnimScriptFuncCommon Common = new();
    public readonly BattleAnimSpriteInfo[] SpriteInfo = { new(), new(), new(), new() };
    public readonly XYTransformContext Pos = new();
    public short UnusedX;
    public short UnusedY;
    public short DestX;
    public short DestY;
}

public static partial class BattleAnimScriptFuncs
{
    private const int BaseScaleXY = 100;
    private const int ShakeAndScaleExtentX = 2;
    private const int ShakeAndScaleExtentY = 0;
    private const int ShakeAndScaleInterval = 0;
    private const int ShakeAndScaleAmount = 10;
    private const int OvalRadiusYInt = -8;
    private const int MonSpriteFrameWidth = 0x50;
    private const int LcdWidth = 256;
    private const int MoveBattlerOffScreen = 0;
    private const int OamModeObjWnd = 2;
    private const int WndMaskObj = 4;
    private const int WndPlaneBg0 = 1;
    private const int WndPlaneBg1 = 2;
    private const int WndPlaneBg2 = 4;
    private const int WndPlaneBg3 = 8;
    private const int WndPlaneObj = 0x10;

    // OFFSET 0x02228520
    private static void BattleAnimScriptFunc_MoveBattlerX2(BattleAnimSystem system)
    {
        MoveBattlerXContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<MoveBattlerXContext>(system);
        ctx.BattleAnimSys = system;
        ctx.Frames = ScriptVar(system, 0);
        ctx.Offset = (short)ScriptVar(system, 1);
        int target = ScriptVar(system, 2);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        int battler = GetMoveTargetBattler(system, target);
        if (battler == 0xFF)
        {
            return;
        }
        ctx.SpriteInfo.MonSprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, battler);
        BattleAnimUtil.BattleAnimUtil_GetMonSpritePos(ctx.SpriteInfo.MonSprite, out ctx.SpriteInfo.X, out ctx.SpriteInfo.Y);
        int dir = BattleAnimUtil.BattleAnimUtil_GetTransformDirectionX(system, battler);
        if (dir <= 0)
        {
            ctx.Offset = (short)-ctx.Offset;
        }
        BattleAnimSystems.BattleAnimSystem_IsContest(system);
        SysTask task = BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_MoveBattlerX2, ctx);
        BattleAnimTask_MoveBattlerX2(task, ctx);
    }

    private static void ApplyAnchoredScale(ShakeAndScaleAttackerContext ctx)
    {
        PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteScaleX, ctx.Scale.X);
        PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteScaleY, ctx.Scale.Y);
    }

    private static void ApplyAnchoredY(ShakeAndScaleAttackerContext ctx)
    {
        short offset = BattleAnimUtil.BattleAnimUtil_GetGroundAnchoredScaleOffset(ctx.SpriteY, ctx.SpriteHeight, ctx.Scale.Data[XYCurY]);
        PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteYCenter, ctx.SpriteY + offset);
    }

    private static void ResetAnchoredScale(ShakeAndScaleAttackerContext ctx)
    {
        PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteScaleX, MonAffineScaleOne);
        PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteScaleY, MonAffineScaleOne);
        PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteYCenter, ctx.SpriteY);
    }

    // OFFSET 0x0222862C
    private static void BattleAnimTask_ShakeAndScaleAttacker(SysTask task, object? param)
    {
        ShakeAndScaleAttackerContext ctx = (ShakeAndScaleAttackerContext)param!;
        switch (ctx.State)
        {
            case 0:
                if (BattleAnimUtil.ScaleLerpContext_Update(ctx.Scale) == 1)
                {
                    BattleAnimUtil.ShakeContext_Update(ctx.Shake);
                    ApplyAnchoredScale(ctx);
                    PokemonSprite.PokemonSprite_AddAttribute(ctx.Sprite!, MonSpriteXCenter, ctx.Shake.X);
                    ApplyAnchoredY(ctx);
                }
                else
                {
                    ctx.State++;
                }
                break;
            case 1:
                if (++ctx.Timer > ctx.HoldFrames)
                {
                    BattleAnimUtil.ScaleLerpContext_Init(ctx.Scale, (short)ScriptVar(ctx.BattleAnimSys!, 0), BaseScaleXY, (short)ScriptVar(ctx.BattleAnimSys!, 1), ScriptVar(ctx.BattleAnimSys!, 3));
                    ctx.State++;
                }
                break;
            case 2:
                if (BattleAnimUtil.ScaleLerpContext_Update(ctx.Scale) == 1)
                {
                    ApplyAnchoredScale(ctx);
                    ApplyAnchoredY(ctx);
                }
                else
                {
                    ctx.State++;
                }
                break;
            case 3:
                if (BattleAnimUtil.ScaleLerpContext_Update(ctx.Scale) == 1)
                {
                    ApplyAnchoredScale(ctx);
                    ApplyAnchoredY(ctx);
                }
                else
                {
                    ResetAnchoredScale(ctx);
                    ctx.State++;
                }
                break;
            default:
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.BattleAnimSys!, task);
                break;
        }
    }

    // OFFSET 0x022287CC
    private static void BattleAnimScriptFunc_ShakeAndScaleAttacker(BattleAnimSystem system)
    {
        ShakeAndScaleAttackerContext ctx = new();
        ctx.State = 0;
        ctx.BattleAnimSys = system;
        ctx.Sprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
        ctx.Timer = 0;
        ctx.HoldFrames = (byte)ScriptVar(system, 4);
        ctx.SpriteY = (short)PokemonSprite.PokemonSprite_GetAttribute(ctx.Sprite!, MonSpriteYCenter);
        ctx.SpriteHeight = BattleAnimSystems.BattleAnimSystem_GetBattlerSpriteOffset(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
        BattleAnimUtil.ScaleLerpContext_Init(ctx.Scale, BaseScaleXY, BaseScaleXY, (short)ScriptVar(system, 0), ScriptVar(system, 2));
        BattleAnimUtil.ShakeContext_Init(ctx.Shake, ShakeAndScaleExtentX, ShakeAndScaleExtentY, ShakeAndScaleInterval, ShakeAndScaleAmount);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_ShakeAndScaleAttacker, ctx);
    }

    // OFFSET 0x02228868
    private static void BattleAnimTask_ShakeAndScaleAttacker2(SysTask task, object? param)
    {
        ShakeAndScaleAttackerContext ctx = (ShakeAndScaleAttackerContext)param!;
        switch (ctx.State)
        {
            case 0:
                if (BattleAnimUtil.ScaleLerpContext_UpdateXY(ctx.Scale) == 1)
                {
                    BattleAnimUtil.ShakeContext_Update(ctx.Shake);
                    ApplyAnchoredScale(ctx);
                    PokemonSprite.PokemonSprite_AddAttribute(ctx.Sprite!, MonSpriteXCenter, ctx.Shake.X);
                    ApplyAnchoredY(ctx);
                }
                else
                {
                    ctx.State++;
                }
                break;
            case 1:
                if (++ctx.Timer > ctx.HoldFrames)
                {
                    short sx = (short)(ctx.Scale2X >> 16);
                    short ex = (short)(ctx.Scale2X & 0xFF);
                    short sy = (short)(ctx.Scale2Y >> 16);
                    short ey = (short)(ctx.Scale2Y & 0xFF);
                    BattleAnimUtil.ScaleLerpContext_InitXY(ctx.Scale, sx, ex, sy, ey, BaseScaleXY, ctx.Scale2Frames);
                    ctx.State++;
                }
                break;
            case 2:
                if (BattleAnimUtil.ScaleLerpContext_UpdateXY(ctx.Scale) == 1)
                {
                    ApplyAnchoredScale(ctx);
                    ApplyAnchoredY(ctx);
                }
                else
                {
                    ctx.State++;
                }
                break;
            case 3:
                if (BattleAnimUtil.ScaleLerpContext_UpdateXY(ctx.Scale) == 1)
                {
                    ApplyAnchoredScale(ctx);
                    ApplyAnchoredY(ctx);
                }
                else
                {
                    ResetAnchoredScale(ctx);
                    ctx.State++;
                }
                break;
            default:
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.BattleAnimSys!, task);
                break;
        }
    }

    // OFFSET 0x02228A0C
    private static void BattleAnimScriptFunc_ShakeAndScaleAttacker2(BattleAnimSystem system)
    {
        ShakeAndScaleAttackerContext ctx = new();
        ctx.State = 0;
        ctx.BattleAnimSys = system;
        ctx.Sprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
        ctx.Timer = 0;
        ctx.HoldFrames = (byte)ScriptVar(system, 6);
        ctx.Scale2X = ScriptVar(system, 3);
        ctx.Scale2Y = ScriptVar(system, 4);
        ctx.Scale2Frames = ScriptVar(system, 5);
        BattleAnimUtil.BattleAnimUtil_GetBattlerDefaultPos2(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system), out _, out ctx.SpriteY);
        ctx.SpriteHeight = BattleAnimSystems.BattleAnimSystem_GetBattlerSpriteOffset(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
        ctx.SpriteY += (short)ctx.SpriteHeight;
        short sx = (short)(ScriptVar(system, 0) >> 16);
        short ex = (short)(ScriptVar(system, 0) & 0xFF);
        short sy = (short)(ScriptVar(system, 1) >> 16);
        short ey = (short)(ScriptVar(system, 1) & 0xFF);
        BattleAnimUtil.ScaleLerpContext_InitXY(ctx.Scale, sx, ex, sy, ey, BaseScaleXY, ScriptVar(system, 2));
        BattleAnimUtil.ShakeContext_Init(ctx.Shake, ShakeAndScaleExtentX, ShakeAndScaleExtentY, ShakeAndScaleInterval, ShakeAndScaleAmount);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_ShakeAndScaleAttacker2, ctx);
    }

    // OFFSET 0x02228B10
    private static void BattleAnimTask_Superpower(SysTask task, object? param)
    {
        SuperpowerContext ctx = (SuperpowerContext)param!;
        Video.SetVisibleWindows(0, 0);
        BattleAnimSystems.BattleAnimSystem_UnloadBaseBg(ctx.Common.BattleAnimSys!, BattleBgBase);
        BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
    }

    // OFFSET 0x02228B40
    private static void BattleAnimScriptFunc_Superpower(BattleAnimSystem system)
    {
        SuperpowerContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<SuperpowerContext>(system);
        ScriptVar(system, 0);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        BattleAnimSystems.BattleAnimSystem_LoadBaseBg(system, BattleBgBase);
        ctx.Sprite = BattleAnimSystems.BattleAnimSystem_GetPokemonSprite(system, 0);
        SpriteSystem.ManagedSprite_SetExplicitOamMode(ctx.Sprite, OamModeObjWnd);
        SpriteSystem.ManagedSprite_SetAffineOverwriteMode(ctx.Sprite, AffineOverwriteDouble);
        SpriteSystem.ManagedSprite_SetAffineScale(ctx.Sprite, 1.2f, 1.2f);
        Video.SetVisibleWindows(0, WndMaskObj);
        Video.SetWindowOutside(0, WndPlaneBg0 | WndPlaneBg1 | WndPlaneBg3 | WndPlaneObj);
        Video.SetObjWindowInside(0, WndPlaneBg0 | WndPlaneBg1 | WndPlaneBg2);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_Superpower, ctx);
    }

    // OFFSET 0x02228BD0
    private static void BattleAnimTask_MoveBattler(SysTask task, object? param)
    {
        MoveBattlerContext ctx = (MoveBattlerContext)param!;
        switch (ctx.Common.State)
        {
            case 0:
                BattleAnimUtil.PosLerpContext_Init(ctx.Pos, ctx.SpriteInfo.X, (short)(ctx.SpriteInfo.X + ctx.OffsetX), ctx.SpriteInfo.Y, (short)(ctx.SpriteInfo.Y + ctx.OffsetY), (ushort)ctx.Frames);
                ctx.Common.State++;
                goto case 1;
            case 1:
                if (BattleAnimUtil.PosLerpContext_Update(ctx.Pos) == 0)
                {
                    ctx.Common.State++;
                }
                PokemonSprite.PokemonSprite_SetAttribute(ctx.SpriteInfo.MonSprite!, MonSpriteXCenter, ctx.Pos.X);
                PokemonSprite.PokemonSprite_SetAttribute(ctx.SpriteInfo.MonSprite!, MonSpriteYCenter, ctx.Pos.Y);
                break;
            default:
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.BattleAnimSys!, task);
                break;
        }
    }

    // OFFSET 0x02228C6C
    private static void BattleAnimScriptFunc_MoveBattler(BattleAnimSystem system)
    {
        MoveBattlerContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<MoveBattlerContext>(system);
        ctx.BattleAnimSys = system;
        ctx.Frames = ScriptVar(system, 0);
        ctx.OffsetX = (short)ScriptVar(system, 1);
        ctx.OffsetY = (short)ScriptVar(system, 2);
        int target = ScriptVar(system, 3);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        int battler = GetMoveTargetBattler(system, target);
        if (battler == 0xFF)
        {
            return;
        }
        ctx.SpriteInfo.MonSprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, battler);
        if (ctx.SpriteInfo.MonSprite == null)
        {
            return;
        }
        BattleAnimUtil.BattleAnimUtil_GetMonSpritePos(ctx.SpriteInfo.MonSprite, out ctx.SpriteInfo.X, out ctx.SpriteInfo.Y);
        int dir = BattleAnimUtil.BattleAnimUtil_GetTransformDirectionX(system, battler);
        if (dir <= 0)
        {
            ctx.OffsetX = (short)-ctx.OffsetX;
            ctx.OffsetY = (short)-ctx.OffsetY;
        }
        if (BattleAnimSystems.BattleAnimSystem_IsContest(system) == 1)
        {
            ctx.OffsetY = (short)-ctx.OffsetY;
        }
        SysTask task = BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_MoveBattler, ctx);
        BattleAnimTask_MoveBattler(task, ctx);
    }

    // OFFSET 0x02228DB8
    private static void BattleAnimTask_RevolveBattler(SysTask task, object? param)
    {
        RevolveBattlerContext ctx = (RevolveBattlerContext)param!;
        BattleAnimSpriteInfo info = ctx.SpriteInfo[0];
        if (BattleAnimUtil.RevolutionContext_UpdateAndApplyToMon(ctx.Revs, info.X, info.Y, info.MonSprite!) == 0)
        {
            PokemonSprite.PokemonSprite_SetAttribute(info.MonSprite!, MonSpriteXCenter, info.X);
            PokemonSprite.PokemonSprite_SetAttribute(info.MonSprite!, MonSpriteYCenter, info.Y + OvalRadiusYInt);
            BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
        }
    }

    // OFFSET 0x02228E00
    private static void BattleAnimScriptFunc_RevolveBattler(BattleAnimSystem system)
    {
        RevolveBattlerContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<RevolveBattlerContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        BattleAnimUtil.BattleAnimUtil_GetBattlerSprites(system, ScriptVar(system, 0), ctx.SpriteInfo, out _);
        ctx.SpriteInfo[0].Y -= OvalRadiusYInt;
        BattleAnimUtil.RevolutionContext_InitOvalRevolutions(ctx.Revs, ScriptVar(system, 1), ScriptVar(system, 2));
        ctx.Revs.Data[2] /= 2;
        ctx.Revs.Data[4] /= 2;
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_RevolveBattler, ctx);
    }

    // OFFSET 0x02228E78
    private static void BattleAnimTask_MoveBattlerOffScreen(SysTask task, object? param)
    {
        MoveBattlerOffScreenContext ctx = (MoveBattlerOffScreenContext)param!;
        switch (ctx.Common.State)
        {
            case 0:
                if (BattleAnimUtil.PosLerpContext_Update(ctx.Pos) == 0)
                {
                    ctx.Common.State++;
                }
                PokemonSprite.PokemonSprite_SetAttribute(ctx.SpriteInfo[0].MonSprite!, MonSpriteXCenter, ctx.Pos.X);
                PokemonSprite.PokemonSprite_SetAttribute(ctx.SpriteInfo[0].MonSprite!, MonSpriteYCenter, ctx.Pos.Y);
                break;
            default:
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
                break;
        }
    }

    private static int GetRoleBattler(BattleAnimSystem system, int target)
    {
        switch (target)
        {
            case AnimAttacker:
                return BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
            case AnimAttackerPartner:
                return BattleAnimUtil.BattleAnimUtil_GetAlliedBattler(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
            case AnimDefender:
                return BattleAnimSystems.BattleAnimSystem_GetDefender(system);
            case AnimDefenderPartner:
                return BattleAnimUtil.BattleAnimUtil_GetAlliedBattler(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system));
            default:
                ErrorHandling.GF_AssertFail();
                return 0;
        }
    }

    private static void SetOffScreenDest(MoveBattlerOffScreenContext ctx, int battler)
    {
        ctx.DestX = 0;
        ctx.DestY = 0;
        if (BattleAnimUtil.BattleAnimUtil_GetBattlerSide(ctx.Common.BattleAnimSys!, battler) == BattlerSidePlayer)
        {
            ctx.DestX = -MonSpriteFrameWidth;
        }
        else
        {
            ctx.DestX = LcdWidth + MonSpriteFrameWidth;
        }
    }

    // OFFSET 0x02228EC0
    private static void BattleAnimScriptFunc_MoveBattlerOffScreen(BattleAnimSystem system)
    {
        MoveBattlerOffScreenContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<MoveBattlerOffScreenContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        int target = ScriptVar(system, 0);
        int frames = ScriptVar(system, 1);
        int battler = GetRoleBattler(system, target);
        BattleAnimUtil.BattleAnimUtil_GetBattlerDefaultPos(system, battler, out ctx.UnusedX, out ctx.UnusedY);
        BattleAnimUtil.BattleAnimUtil_GetBattlerSprites(system, target, ctx.SpriteInfo, out _);
        SetOffScreenDest(ctx, battler);
        BattleAnimUtil.PosLerpContext_Init(ctx.Pos, ctx.SpriteInfo[0].X, ctx.DestX, ctx.SpriteInfo[0].Y, ctx.SpriteInfo[0].Y, (ushort)frames);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_MoveBattlerOffScreen, ctx);
    }

    // OFFSET 0x02228FB4
    private static void BattleAnimScriptFunc_MoveBattlerOnOrOffScreen(BattleAnimSystem system)
    {
        MoveBattlerOffScreenContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<MoveBattlerOffScreenContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        int mode = ScriptVar(system, 0);
        int target = ScriptVar(system, 1);
        int frames = ScriptVar(system, 2);
        int battler = GetRoleBattler(system, target);
        BattleAnimUtil.BattleAnimUtil_GetBattlerDefaultPos(system, battler, out ctx.UnusedX, out ctx.UnusedY);
        BattleAnimUtil.BattleAnimUtil_GetBattlerSprites(system, target, ctx.SpriteInfo, out _);
        SetOffScreenDest(ctx, battler);
        if (mode == MoveBattlerOffScreen)
        {
            BattleAnimUtil.PosLerpContext_Init(ctx.Pos, ctx.SpriteInfo[0].X, ctx.DestX, ctx.SpriteInfo[0].Y, ctx.SpriteInfo[0].Y, (ushort)frames);
        }
        else
        {
            short battlerX = BattleAnimUtil.BattleAnimUtil_GetBattlerPos(system, battler, 0);
            BattleAnimUtil.PosLerpContext_Init(ctx.Pos, ctx.DestX, battlerX, ctx.SpriteInfo[0].Y, ctx.SpriteInfo[0].Y, (ushort)frames);
        }
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_MoveBattlerOffScreen, ctx);
    }

    // OFFSET 0x022290DC
    private static void BattleAnimScriptFunc_MoveBattlerToDefaultPos(BattleAnimSystem system)
    {
        BattleAnimSpriteInfo[] spriteInfo = { new(), new(), new(), new() };
        int target = ScriptVar(system, 0);
        int battler = GetRoleBattler(system, target);
        BattleAnimUtil.BattleAnimUtil_GetBattlerDefaultPos(system, battler, out short destX, out short destY);
        BattleAnimUtil.BattleAnimUtil_GetBattlerSprites(system, target, spriteInfo, out _);
        short height = (short)BattleAnimSystems.BattleAnimSystem_GetBattlerSpriteOffset(system, battler);
        PokemonSprite.PokemonSprite_SetAttribute(spriteInfo[0].MonSprite!, MonSpriteXCenter, destX);
        PokemonSprite.PokemonSprite_SetAttribute(spriteInfo[0].MonSprite!, MonSpriteYCenter, destY + height);
    }
}
