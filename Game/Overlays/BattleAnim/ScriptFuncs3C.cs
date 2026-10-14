namespace PlatinumSharp.Game;

public sealed class ScalePokemonSpriteContext
{
    public BattleAnimSystem? BattleAnimSys;
    public SpriteManager? PokemonSpriteManager;
    public int State;
    public short BaseY;
    public short BattlerHeight;
    public ManagedSprite? Sprite;
    public readonly XYTransformContext Scale = new();
    public int SpriteAlpha;
    public int StartScale;
    public int EndScale;
    public int RefScale;
    public int Cycles;
    public int ScaleFrames;
    public int Flip;
}

public sealed class ScaleBattlerSpriteContext
{
    public BattleAnimSystem? BattleAnimSys;
    public int State;
    public short BattlerY;
    public short BattlerSpriteHeight;
    public PokemonSprite? MonSprite;
    public readonly XYTransformContext Scale = new();
    public int StartX;
    public int EndX;
    public int StartY;
    public int EndY;
    public int Reference;
    public int Cycles;
    public int ScaleFrames;
    public int Hold;
}

public sealed class AlphaFadePokemonSpriteContext
{
    public BattleAnimSystem? BattleAnimSys;
    public SpriteManager? PokemonSpriteManager;
    public readonly AlphaFadeContext Alpha = new();
}

public sealed class BlinkAttackerContext
{
    public int Count;
    public int Timer;
    public int Interval;
    public readonly BattleAnimSpriteInfo SpriteInfo = new();
    public readonly BattleAnimScriptFuncCommon Common = new();
    public BattleAnimSystem? BattleAnimSys;
}

public sealed class MoveBattlerXContext
{
    public int Frames;
    public short Offset;
    public readonly BattleAnimSpriteInfo SpriteInfo = new();
    public readonly BattleAnimScriptFuncCommon Common = new();
    public readonly XYTransformContext Pos = new();
    public BattleAnimSystem? BattleAnimSys;
}

public static partial class BattleAnimScriptFuncs
{
    private const int MonSpriteShadowHeight = 0x29;
    private const int ScalePokemonSpriteBgAlpha = 16;
    private const int HwSpritePriority = 100;
    private const int HwSpriteExpPriority = 1;
    private const int OamModeXlu = 1;
    private const int AnchorBottom = 0;
    private const int BattlerSpriteAttacker = AnimAttacker | AnimBattlerSprites;
    private const int BattlerSpriteAttackerPartner = AnimAttackerPartner | AnimBattlerSprites;
    private const int BattlerSpriteDefender = AnimDefender | AnimBattlerSprites;
    private const int BattlerSpriteDefenderPartner = AnimDefenderPartner | AnimBattlerSprites;
    private const int RoleAttacker = 0;
    private const int RoleDefender = 1;
    private const int XYCurY = 4;

    private static void ApplyPokemonSpriteScale(ScalePokemonSpriteContext ctx)
    {
        BattleAnimUtil.ScaleLerpContext_GetAffineScale(ctx.Scale, out float scaleX, out float scaleY);
        if (ctx.Flip == 1)
        {
            scaleX = -scaleX;
        }
        SpriteSystem.ManagedSprite_SetAffineScale(ctx.Sprite!, scaleX, scaleY);
    }

    // OFFSET 0x02227B4C
    private static void BattleAnimTask_ScalePokemonSprite(SysTask task, object? param)
    {
        ScalePokemonSpriteContext ctx = (ScalePokemonSpriteContext)param!;
        int active;
        switch (ctx.State)
        {
            case 0:
                BattleAnimUtil.ScaleLerpContext_Init(ctx.Scale, (short)ctx.StartScale, (short)ctx.RefScale, (short)ctx.EndScale, ctx.ScaleFrames >> 16);
                ApplyPokemonSpriteScale(ctx);
                ctx.State++;
                break;
            case 1:
                active = BattleAnimUtil.ScaleLerpContext_Update(ctx.Scale);
                ApplyPokemonSpriteScale(ctx);
                if (active != 0)
                {
                    BattleAnimUtil.BattleAnimUtil_SetSpriteAnchoredPosition(ctx.Sprite!, ctx.BaseY, ctx.BattlerHeight, ctx.Scale.Data[XYCurY], AnchorBottom);
                }
                else
                {
                    ctx.State++;
                }
                break;
            case 2:
                BattleAnimUtil.ScaleLerpContext_Init(ctx.Scale, (short)ctx.EndScale, (short)ctx.RefScale, (short)ctx.StartScale, ctx.ScaleFrames & 0xFFFF);
                ctx.State++;
                break;
            case 3:
                active = BattleAnimUtil.ScaleLerpContext_Update(ctx.Scale);
                ApplyPokemonSpriteScale(ctx);
                if (active != 0)
                {
                    BattleAnimUtil.BattleAnimUtil_SetSpriteAnchoredPosition(ctx.Sprite!, ctx.BaseY, ctx.BattlerHeight, ctx.Scale.Data[XYCurY], AnchorBottom);
                }
                else
                {
                    ctx.Cycles--;
                    ctx.State = ctx.Cycles <= 0 ? ctx.State + 1 : 0;
                }
                break;
            case 4:
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.BattleAnimSys!, task);
                return;
        }
        SpriteSystem.SpriteSystem_DrawSprites(ctx.PokemonSpriteManager);
    }

    // OFFSET 0x02227CBC
    private static void BattleAnimScriptFunc_ScalePokemonSprite(BattleAnimSystem system)
    {
        ScalePokemonSpriteContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<ScalePokemonSpriteContext>(system);
        ctx.BattleAnimSys = system;
        ctx.PokemonSpriteManager = BattleAnimSystems.BattleAnimSystem_GetPokemonSpriteManager(system);
        PokemonSprite battlerSprite;
        if (ScriptVar(system, 0) == RoleAttacker)
        {
            battlerSprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system))!;
            ctx.BattlerHeight = (short)BattleAnimSystems.BattleAnimSystem_GetBattlerSpriteOffset(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
        }
        else
        {
            battlerSprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system))!;
            ctx.BattlerHeight = (short)BattleAnimSystems.BattleAnimSystem_GetBattlerSpriteOffset(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system));
        }
        ctx.BaseY = (short)PokemonSprite.PokemonSprite_GetAttribute(battlerSprite, MonSpriteYCenter);
        ctx.BaseY -= (short)PokemonSprite.PokemonSprite_GetAttribute(battlerSprite, MonSpriteShadowHeight);
        ctx.SpriteAlpha = ScriptVar(system, 1);
        ctx.StartScale = ScriptVar(system, 2);
        ctx.EndScale = ScriptVar(system, 3);
        ctx.RefScale = ScriptVar(system, 4);
        ctx.Cycles = ScriptVar(system, 5);
        ctx.ScaleFrames = ScriptVar(system, 6);
        ctx.Sprite = BattleAnimSystems.BattleAnimSystem_GetPokemonSprite(system, ScriptVar(system, 7));
        SpriteSystem.ManagedSprite_SetPriority(ctx.Sprite, HwSpritePriority);
        SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.Sprite, HwSpriteExpPriority);
        SpriteSystem.ManagedSprite_SetExplicitOamMode(ctx.Sprite, OamModeXlu);
        SpriteSystem.ManagedSprite_SetAffineOverwriteMode(ctx.Sprite, AffineOverwriteDouble);
        BattleAnimUtil.BattleAnimUtil_SetSpriteBgBlending(system, ctx.SpriteAlpha, ScalePokemonSpriteBgAlpha - ctx.SpriteAlpha);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_ScalePokemonSprite, ctx);
        int battlerRole = ScriptVar(system, 0) == RoleAttacker ? RoleAttacker : RoleDefender;
        if (BattleAnimSystems.BattleAnimSystem_ShouldBattlerSpriteBeFlipped(system, battlerRole) == 1)
        {
            ctx.Flip = 1;
        }
    }

    // OFFSET 0x02227DE0
    private static void BattleAnimTask_ScaleBattlerSprite(SysTask task, object? param)
    {
        ScaleBattlerSpriteContext ctx = (ScaleBattlerSpriteContext)param!;
        switch (ctx.State)
        {
            case 0:
                BattleAnimUtil.ScaleLerpContext_InitXY(ctx.Scale, (short)ctx.StartX, (short)ctx.EndX, (short)ctx.StartY, (short)ctx.EndY, (short)ctx.Reference, ctx.ScaleFrames >> 16);
                BattleAnimUtil.ScaleLerpContext_UpdateAndApplyToMon(ctx.Scale, ctx.MonSprite!);
                ctx.State++;
                break;
            case 1:
                if (BattleAnimUtil.ScaleLerpContext_UpdateXYAndApplyToMon(ctx.Scale, ctx.MonSprite!) != 0)
                {
                    BattleAnimUtil.BattleAnimUtil_SetPokemonSpriteAnchoredPosition(ctx.MonSprite!, ctx.BattlerY, ctx.BattlerSpriteHeight, ctx.Scale.Data[XYCurY], AnchorBottom);
                }
                else
                {
                    ctx.State++;
                }
                break;
            case 2:
                if (ctx.Hold != 0)
                {
                    ctx.Hold--;
                    break;
                }
                ctx.State++;
                goto case 3;
            case 3:
                BattleAnimUtil.ScaleLerpContext_InitXY(ctx.Scale, (short)ctx.EndX, (short)ctx.StartX, (short)ctx.EndY, (short)ctx.StartY, (short)ctx.Reference, ctx.ScaleFrames & 0xFFFF);
                BattleAnimUtil.ScaleLerpContext_UpdateXYAndApplyToMon(ctx.Scale, ctx.MonSprite!);
                ctx.State++;
                break;
            case 4:
                if (BattleAnimUtil.ScaleLerpContext_UpdateXYAndApplyToMon(ctx.Scale, ctx.MonSprite!) != 0)
                {
                    BattleAnimUtil.BattleAnimUtil_SetPokemonSpriteAnchoredPosition(ctx.MonSprite!, ctx.BattlerY, ctx.BattlerSpriteHeight, ctx.Scale.Data[XYCurY], AnchorBottom);
                }
                else
                {
                    ctx.Cycles--;
                    ctx.State = ctx.Cycles <= 0 ? ctx.State + 1 : 0;
                }
                break;
            case 5:
                ResetMonScale(ctx.MonSprite!);
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.BattleAnimSys!, task);
                return;
        }
    }

    // OFFSET 0x02227F30
    private static void BattleAnimScriptFunc_ScaleBattlerSprite(BattleAnimSystem system)
    {
        BattleAnimSpriteInfo[] sprites = { new(), new(), new(), new() };
        ScaleBattlerSpriteContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<ScaleBattlerSpriteContext>(system);
        ctx.BattleAnimSys = system;
        int target = ScriptVar(system, 0);
        switch (target)
        {
            case BattlerSpriteAttacker:
            case BattlerSpriteAttackerPartner:
                ctx.BattlerSpriteHeight = (short)BattleAnimSystems.BattleAnimSystem_GetBattlerSpriteOffset(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
                BattleAnimUtil.BattleAnimUtil_GetBattlerSprites(system, target, sprites, out _);
                ctx.MonSprite = sprites[0].MonSprite;
                break;
            case BattlerSpriteDefender:
            case BattlerSpriteDefenderPartner:
                ctx.BattlerSpriteHeight = (short)BattleAnimSystems.BattleAnimSystem_GetBattlerSpriteOffset(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system));
                BattleAnimUtil.BattleAnimUtil_GetBattlerSprites(system, target, sprites, out _);
                ctx.MonSprite = sprites[0].MonSprite;
                break;
            case AnimSpecificBattler | AnimAttacker:
                ctx.MonSprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, FindBattlerOfType(system, 0, 2));
                break;
            case AnimSpecificBattler | AnimAttackerPartner:
                ctx.MonSprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, FindBattlerOfType(system, 4, 4));
                break;
            case AnimSpecificBattler | AnimDefender:
                ctx.MonSprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, FindBattlerOfType(system, 1, 3));
                break;
            case AnimSpecificBattler | AnimDefenderPartner:
                ctx.MonSprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, FindBattlerOfType(system, 5, 5));
                break;
        }
        if (ctx.MonSprite == null)
        {
            return;
        }
        ctx.BattlerY = (short)PokemonSprite.PokemonSprite_GetAttribute(ctx.MonSprite, MonSpriteYCenter);
        ctx.BattlerSpriteHeight = (short)PokemonSprite.PokemonSprite_GetAttribute(ctx.MonSprite, MonSpriteShadowHeight);
        ctx.BattlerSpriteHeight += 16;
        ctx.StartX = ScriptVar(system, 1);
        ctx.EndX = ScriptVar(system, 2);
        ctx.StartY = ScriptVar(system, 3);
        ctx.EndY = ScriptVar(system, 4);
        ctx.Reference = ScriptVar(system, 5);
        ctx.ScaleFrames = ScriptVar(system, 7);
        ctx.Cycles = ScriptVar(system, 6) & 0xFFFF;
        ctx.Hold = ScriptVar(system, 6) >> 16;
        SysTask task = BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_ScaleBattlerSprite, ctx);
        BattleAnimTask_ScaleBattlerSprite(task, ctx);
    }

    // OFFSET 0x0222813C
    private static void BattleAnimTask_AlphaFadePokemonSprite(SysTask task, object? param)
    {
        AlphaFadePokemonSpriteContext ctx = (AlphaFadePokemonSpriteContext)param!;
        if (BattleAnimUtil.AlphaFadeContext_IsDone(ctx.Alpha) != 0)
        {
            BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.BattleAnimSys!, task);
        }
        else
        {
            SpriteSystem.SpriteSystem_DrawSprites(ctx.PokemonSpriteManager);
        }
    }

    // OFFSET 0x02228168
    private static void BattleAnimScriptFunc_AlphaFadePokemonSprite(BattleAnimSystem system)
    {
        AlphaFadePokemonSpriteContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<AlphaFadePokemonSpriteContext>(system);
        ctx.BattleAnimSys = system;
        ctx.PokemonSpriteManager = BattleAnimSystems.BattleAnimSystem_GetPokemonSpriteManager(system);
        int targets = ScriptVar(system, 0);
        for (int i = 0; i < 4; i++)
        {
            if ((targets & (1 << i)) != 0)
            {
                ManagedSprite sprite = BattleAnimSystems.BattleAnimSystem_GetPokemonSprite(system, i);
                SpriteSystem.ManagedSprite_SetExplicitOamMode(sprite, OamModeXlu);
            }
        }
        int spriteStartAlpha = ScriptVar(system, 1);
        int bgStartAlpha = ScriptVar(system, 3);
        BattleAnimUtil.BattleAnimUtil_SetSpriteBgBlending(system, spriteStartAlpha, bgStartAlpha);
        BattleAnimUtil.AlphaFadeContext_Init(ctx.Alpha, (short)spriteStartAlpha, (short)ScriptVar(system, 2), (short)bgStartAlpha, (short)ScriptVar(system, 4), ScriptVar(system, 5));
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_AlphaFadePokemonSprite, ctx);
    }

    // OFFSET 0x02228214
    private static void BattleAnimScriptFunc_HideBattler(BattleAnimSystem system)
    {
        int hide = ScriptVar(system, 1);
        BattleAnimSpriteInfo[] spriteInfo = { new(), new(), new(), new() };
        BattleAnimUtil.BattleAnimUtil_GetBattlerSprites(system, ScriptVar(system, 0), spriteInfo, out int spriteCount);
        for (int i = 0; i < spriteCount; i++)
        {
            if (BattleAnimSystems.BattleAnimSystem_IsBattlerSemiInvulnerable(system, spriteInfo[0].Battler) == 1 && hide == 0)
            {
                continue;
            }
            PokemonSprite.PokemonSprite_SetAttribute(spriteInfo[0].MonSprite!, MonSpriteHide, hide);
        }
    }

    // OFFSET 0x02228268
    private static void BattleAnimTask_BlinkAttacker(SysTask task, object? param)
    {
        BlinkAttackerContext ctx = (BlinkAttackerContext)param!;
        if (ctx.Timer >= ctx.Interval)
        {
            if (ctx.Count == 0)
            {
                PokemonSprite.PokemonSprite_SetAttribute(ctx.SpriteInfo.MonSprite!, MonSpriteHide, 0);
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.BattleAnimSys!, task);
            }
            else
            {
                int hide = PokemonSprite.PokemonSprite_GetAttribute(ctx.SpriteInfo.MonSprite!, MonSpriteHide);
                PokemonSprite.PokemonSprite_SetAttribute(ctx.SpriteInfo.MonSprite!, MonSpriteHide, hide ^ 1);
                ctx.Count--;
                ctx.Timer = 0;
            }
        }
        else
        {
            ctx.Timer++;
        }
    }

    // OFFSET 0x022282BC
    private static void BattleAnimScriptFunc_BlinkAttacker(BattleAnimSystem system)
    {
        BlinkAttackerContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<BlinkAttackerContext>(system);
        ctx.BattleAnimSys = system;
        ctx.Count = ScriptVar(system, 0) * 2;
        ctx.Interval = ScriptVar(system, 1);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.SpriteInfo.MonSprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_BlinkAttacker, ctx);
    }

    private static int GetMoveTargetBattler(BattleAnimSystem system, int target)
    {
        int battler = 0xFF;
        switch (target)
        {
            case BattlerSpriteAttacker:
                battler = BattleAnimSystems.BattleAnimSystem_GetAttacker(system);
                break;
            case BattlerSpriteAttackerPartner:
                if (BattleAnimSystems.BattleAnimSystem_IsDoubleBattle(system) == 1)
                {
                    battler = BattleAnimUtil.BattleAnimUtil_GetAlliedBattler(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
                }
                break;
            case BattlerSpriteDefender:
                battler = BattleAnimSystems.BattleAnimSystem_GetDefender(system);
                break;
            case BattlerSpriteDefenderPartner:
                if (BattleAnimSystems.BattleAnimSystem_IsDoubleBattle(system) == 1)
                {
                    battler = BattleAnimUtil.BattleAnimUtil_GetAlliedBattler(system, BattleAnimSystems.BattleAnimSystem_GetDefender(system));
                }
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return battler;
    }

    // OFFSET 0x0222830C
    private static void BattleAnimTask_MoveBattlerX(SysTask task, object? param)
    {
        MoveBattlerXContext ctx = (MoveBattlerXContext)param!;
        switch (ctx.Common.State)
        {
            case 0:
                BattleAnimUtil.PosLerpContext_Init(ctx.Pos, ctx.SpriteInfo.X, (short)(ctx.SpriteInfo.X + ctx.Offset), ctx.SpriteInfo.Y, ctx.SpriteInfo.Y, (ushort)ctx.Frames);
                ctx.Common.State++;
                break;
            case 1:
                if (BattleAnimUtil.PosLerpContext_Update(ctx.Pos) == 0)
                {
                    ctx.Common.State++;
                }
                PokemonSprite.PokemonSprite_SetAttribute(ctx.SpriteInfo.MonSprite!, MonSpriteXCenter, ctx.Pos.X);
                break;
            default:
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.BattleAnimSys!, task);
                break;
        }
    }

    // OFFSET 0x02228394
    private static void BattleAnimScriptFunc_MoveBattlerX(BattleAnimSystem system)
    {
        MoveBattlerXContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<MoveBattlerXContext>(system);
        ctx.BattleAnimSys = system;
        ctx.Frames = ScriptVar(system, 0);
        ctx.Offset = (short)ScriptVar(system, 1);
        int battler = GetMoveTargetBattler(system, ScriptVar(system, 2));
        if (battler == 0xFF)
        {
            return;
        }
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.SpriteInfo.MonSprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, battler);
        BattleAnimUtil.BattleAnimUtil_GetMonSpritePos(ctx.SpriteInfo.MonSprite, out ctx.SpriteInfo.X, out ctx.SpriteInfo.Y);
        int dir = BattleAnimUtil.BattleAnimUtil_GetTransformDirectionX(system, battler);
        if (dir <= 0)
        {
            ctx.Offset = (short)-ctx.Offset;
        }
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(system, BattleAnimTask_MoveBattlerX, ctx);
    }

    // OFFSET 0x02228488
    private static void BattleAnimTask_MoveBattlerX2(SysTask task, object? param)
    {
        MoveBattlerXContext ctx = (MoveBattlerXContext)param!;
        switch (ctx.Common.State)
        {
            case 0:
                if (ctx.SpriteInfo.MonSprite == null)
                {
                    ctx.Common.State = 0xFF;
                    break;
                }
                BattleAnimUtil.PosLerpContext_Init(ctx.Pos, ctx.SpriteInfo.X, (short)(ctx.SpriteInfo.X + ctx.Offset), ctx.SpriteInfo.Y, ctx.SpriteInfo.Y, (ushort)ctx.Frames);
                ctx.Common.State++;
                goto case 1;
            case 1:
                if (BattleAnimUtil.PosLerpContext_Update(ctx.Pos) == 0)
                {
                    ctx.Common.State++;
                }
                PokemonSprite.PokemonSprite_SetAttribute(ctx.SpriteInfo.MonSprite!, MonSpriteXCenter, ctx.Pos.X);
                break;
            default:
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.BattleAnimSys!, task);
                break;
        }
    }
}
