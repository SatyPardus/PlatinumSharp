namespace PlatinumSharp.Game;

public sealed class SpriteShakeInfo
{
    public short ExtentX;
    public short ExtentY;
    public short Interval;
    public short Amount;
    public int Targets;
}

public sealed class ShakeContext
{
    public int SpriteCount;
    public readonly BattleAnimScriptFuncCommon Common = new();
    public readonly SpriteShakeInfo ShakeInfo = new();
    public readonly BattleAnimSpriteInfo[] Sprites = { new(), new(), new(), new() };
    public readonly XYTransformContext Shake = new();
}

public sealed class ScrollCustomBgContext
{
    public int Reverse;
    public int ScrollFinished;
    public ushort TargetBgAlpha;
    public short X;
    public short Y;
    public short StepX;
    public short StepY;
    public ushort SlowDownTime;
    public ushort Timer;
    public ushort BgAlpha;
    public ushort OtherAlpha;
    public readonly BattleAnimScriptFuncCommon Common = new();
}

public sealed class MuddyWaterContext
{
    public int Reverse;
    public int ScrollFinished;
    public ushort Unused;
    public short X;
    public short Y;
    public short StepX;
    public short StepY;
    public ushort SlowDownTime;
    public ushort Timer;
    public ushort BgAlpha;
    public ushort OtherAlpha;
    public readonly BattleAnimScriptFuncCommon Common = new();
}

public static partial class BattleAnimScriptFuncs
{
    private const int AnimBattlerSprites = 0x100;
    private const int AnimPokemonSprites = 0x200;
    private const int AnimBackground = 0x400;
    private const int AnimSpecificBattler = 0x800;
    private const int AnimAttacker = 2;
    private const int AnimAttackerPartner = 4;
    private const int AnimDefender = 8;
    private const int AnimDefenderPartner = 0x10;
    private const int BattleBgBase = 2;
    private const int BattleBgEffect = 3;
    private const int MonSpriteXCenter = 0;
    private const int MonSpriteYCenter = 1;
    private const int MonSpriteHide = 6;
    private const int MonSpriteScaleX = 0xC;
    private const int MonSpriteScaleY = 0xD;
    private const int MonAffineScaleOne = 0x100;
    private const int BattlerSideEnemy = 4;
    private const int BattlerSidePlayer = 3;
    private const int NarcBattBg = 7;
    private const int PlttBufMainBg = 0;
    private const int PaletteSizeBytes = 0x20;
    private const int BlendBg0 = 1;
    private const int BlendBg2 = 4;
    private const int BlendBg3 = 8;
    private const int BlendObj = 0x10;
    private const int BlendBd = 0x20;
    private const int ScrollBgAlphaStep = 2;
    private const int ScrollBgOtherStartAlpha = 31;
    private const int ScrollBgOtherTargetAlpha = 7;
    private const int ScrollBgMaxY = 512;
    private const int ScrollBgMinY = -412;
    private const int ScrollBgStartYOffset = 84;
    private const int MuddyWaterBgEndAlpha = 16;
    private const int MuddyWaterOtherEndAlpha = 4;
    private const int MuddyWaterOtherStartAlpha = 16;

    private static int ScriptVar(BattleAnimSystem system, int id)
    {
        return BattleAnimSystems.BattleAnimSystem_GetScriptVar(system, id);
    }

    private static bool MaskSet(int value, int mask)
    {
        return BattleAnimUtil.BattleAnimUtil_IsMaskSet(value, mask);
    }

    private static void SetBlendAlpha(int first, int second, int ev1, int ev2)
    {
        Video.SetBlendAlpha(0, first, second, ev1, ev2);
    }

    private static void ChangeBlendAlpha(int ev1, int ev2)
    {
        Video.SetBlendAlphaCoefficients(0, (ushort)((ev2 << 8) | ev1));
    }

    // OFFSET 0x02226B84
    private static void ResetMonScale(PokemonSprite sprite)
    {
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteScaleX, MonAffineScaleOne);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteScaleY, MonAffineScaleOne);
    }

    // OFFSET 0x02226BA0
    private static void BattleAnimTask_Shake(SysTask task, object? param)
    {
        ShakeContext ctx = (ShakeContext)param!;
        switch (ctx.Common.State)
        {
            case 0:
                BattleAnimUtil.ShakeContext_Init(ctx.Shake, ctx.ShakeInfo.ExtentX, ctx.ShakeInfo.ExtentY, ctx.ShakeInfo.Interval, ctx.ShakeInfo.Amount);
                ctx.Common.State++;
                break;
            case 1:
                if (BattleAnimUtil.ShakeContext_Update(ctx.Shake) == 0)
                {
                    ctx.Common.State++;
                    break;
                }
                if (MaskSet(ctx.ShakeInfo.Targets, AnimBattlerSprites))
                {
                    for (int i = 0; i < ctx.SpriteCount; i++)
                    {
                        if (ctx.Sprites[i].MonSprite == null)
                        {
                            continue;
                        }
                        PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprites[i].MonSprite!, MonSpriteXCenter, ctx.Sprites[i].X + ctx.Shake.X);
                        PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprites[i].MonSprite!, MonSpriteYCenter, ctx.Sprites[i].Y + ctx.Shake.Y);
                    }
                }
                else if (MaskSet(ctx.ShakeInfo.Targets, AnimPokemonSprites))
                {
                    for (int i = 0; i < ctx.SpriteCount; i++)
                    {
                        if (ctx.Sprites[i].HwSprite == null)
                        {
                            continue;
                        }
                        SpriteSystem.ManagedSprite_SetPositionXY(ctx.Sprites[i].HwSprite!, (short)(ctx.Sprites[i].X + ctx.Shake.X), (short)(ctx.Sprites[i].Y + ctx.Shake.Y));
                        SpriteSystem.ManagedSprite_TickFrame(ctx.Sprites[i].HwSprite!);
                    }
                    SpriteSystem.SpriteSystem_DrawSprites(ctx.Common.PokemonSpriteManager);
                }
                if (MaskSet(ctx.ShakeInfo.Targets, AnimBackground))
                {
                    Bg.Bg_SetOffset(ctx.Common.BgConfig!, BattleBgEffect, 0, ctx.Shake.X);
                }
                break;
            default:
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
                break;
        }
    }

    // OFFSET 0x02226CB0
    private static void BattleAnimScriptFunc_Shake(BattleAnimSystem system)
    {
        ShakeContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<ShakeContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.ShakeInfo.ExtentX = (short)ScriptVar(system, 0);
        ctx.ShakeInfo.ExtentY = (short)ScriptVar(system, 1);
        ctx.ShakeInfo.Interval = (short)ScriptVar(system, 2);
        ctx.ShakeInfo.Amount = (short)ScriptVar(system, 3);
        ctx.ShakeInfo.Targets = ScriptVar(system, 4);
        if (MaskSet(ctx.ShakeInfo.Targets, AnimBattlerSprites))
        {
            BattleAnimUtil.BattleAnimUtil_GetBattlerSprites(system, ctx.ShakeInfo.Targets, ctx.Sprites, out ctx.SpriteCount);
        }
        else if (MaskSet(ctx.ShakeInfo.Targets, AnimPokemonSprites))
        {
            BattleAnimUtil.BattleAnimUtil_GetPokemonSprites(system, ctx.ShakeInfo.Targets, ctx.Sprites, out ctx.SpriteCount);
        }
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(ctx.Common.BattleAnimSys!, BattleAnimTask_Shake, ctx);
    }

    private static void ScrollBg(BgConfig bgConfig, ref int scrollFinished, ref short x, ref short y, ref short stepX, ref short stepY, ushort slowDownTime, ref ushort timer)
    {
        if ((stepY > 0 && y >= ScrollBgMaxY) || (stepY < 0 && y <= ScrollBgMinY))
        {
            scrollFinished = 1;
        }
        x += stepX;
        y += stepY;
        if (slowDownTime < timer)
        {
            if (stepY < 0)
            {
                stepY++;
            }
            else
            {
                stepY = 0;
            }
            if (stepX < 0)
            {
                stepX++;
            }
            else
            {
                stepX = 0;
            }
            timer = 0;
        }
        else
        {
            timer++;
        }
        Bg.Bg_SetOffset(bgConfig, BattleBgBase, 0, x);
        Bg.Bg_SetOffset(bgConfig, BattleBgBase, 3, y);
    }

    // OFFSET 0x02226D38
    private static void BattleAnimTask_ScrollCustomBg(SysTask task, object? param)
    {
        ScrollCustomBgContext ctx = (ScrollCustomBgContext)param!;
        switch (ctx.Common.State)
        {
            case 0:
                SetBlendAlpha(BlendBg2, BlendBd | BlendObj | BlendBg0 | BlendBg3, ctx.BgAlpha, ctx.OtherAlpha);
                Bg.Bg_ToggleLayer(BattleBgBase, 1);
                ctx.Common.State++;
                goto case 1;
            case 1:
            {
                int completed = 0;
                if (ctx.BgAlpha < ctx.TargetBgAlpha - ScrollBgAlphaStep)
                {
                    ctx.BgAlpha += ScrollBgAlphaStep;
                }
                else
                {
                    completed++;
                }
                if (ctx.OtherAlpha > ScrollBgOtherTargetAlpha + ScrollBgAlphaStep)
                {
                    ctx.OtherAlpha -= ScrollBgAlphaStep;
                }
                else
                {
                    completed++;
                }
                if (completed == 2)
                {
                    ctx.BgAlpha = ctx.TargetBgAlpha;
                    ctx.OtherAlpha = ScrollBgOtherTargetAlpha;
                    ctx.Common.State++;
                }
                ChangeBlendAlpha(ctx.BgAlpha, ctx.OtherAlpha);
                break;
            }
            case 2:
                if (ctx.ScrollFinished != 0)
                {
                    ctx.Common.State++;
                }
                break;
            case 3:
            {
                int completed = 0;
                if (ctx.BgAlpha > ScrollBgAlphaStep)
                {
                    ctx.BgAlpha -= ScrollBgAlphaStep;
                }
                else
                {
                    completed++;
                }
                if (ctx.OtherAlpha < ScrollBgOtherStartAlpha - ScrollBgAlphaStep)
                {
                    ctx.OtherAlpha += ScrollBgAlphaStep;
                }
                else
                {
                    completed++;
                }
                if (completed == 2)
                {
                    ctx.BgAlpha = 0;
                    ctx.OtherAlpha = ScrollBgOtherStartAlpha;
                    ctx.Common.State++;
                }
                ChangeBlendAlpha(ctx.BgAlpha, ctx.OtherAlpha);
                break;
            }
            default:
                Bg.Bg_ToggleLayer(BattleBgBase, 0);
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
                return;
        }
        ScrollBg(ctx.Common.BgConfig!, ref ctx.ScrollFinished, ref ctx.X, ref ctx.Y, ref ctx.StepX, ref ctx.StepY, ctx.SlowDownTime, ref ctx.Timer);
    }

    private static void LoadCustomBg(BattleAnimSystem system, BattleAnimScriptFuncCommon common, int bgID)
    {
        int heapID = BattleAnimSystems.BattleAnimSystem_GetHeapID(system);
        Graphics.Graphics_LoadTilesToBgLayer(NarcBattBg, (uint)BattleAnimSystems.BattleAnimSystem_GetBgNarcMemberIndex(bgID, 0), common.BgConfig!, BattleBgBase, 0, 0, 1, heapID);
        PaletteData.PaletteData_LoadBufferFromFileStart(common.PaletteData!, NarcBattBg, BattleAnimSystems.BattleAnimSystem_GetBgNarcMemberIndex(bgID, 1), heapID, PlttBufMainBg, PaletteSizeBytes, 0x90);
        Bg.Bg_ClearTilemap(common.BgConfig!, BattleBgBase);
        int member = 2;
        if (BattleAnimSystems.BattleAnimSystem_IsContest(system) == 1)
        {
            member = 4;
        }
        else if (BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system)) == BattlerSideEnemy)
        {
            member = 3;
        }
        Graphics.Graphics_LoadTilemapToBgLayer(NarcBattBg, (uint)BattleAnimSystems.BattleAnimSystem_GetBgNarcMemberIndex(bgID, member), common.BgConfig!, BattleBgBase, 0, 0, 1, heapID);
    }

    // OFFSET 0x02226EB0
    private static void BattleAnimScriptFunc_ScrollCustomBg(BattleAnimSystem system)
    {
        ScrollCustomBgContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<ScrollCustomBgContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.X = (short)ScriptVar(system, 1);
        ctx.Y = (short)ScriptVar(system, 2);
        ctx.StepX = (short)ScriptVar(system, 3);
        ctx.StepY = (short)ScriptVar(system, 4);
        ctx.Reverse = ScriptVar(system, 5);
        ctx.TargetBgAlpha = (ushort)ScriptVar(system, 6);
        ctx.SlowDownTime = (ushort)ScriptVar(system, 7);
        ctx.Timer = 0;
        if (ctx.Reverse != 0 && BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system)) == BattlerSideEnemy)
        {
            ctx.X = (short)-ctx.X;
            ctx.Y = (short)-ctx.Y;
            ctx.StepX = (short)-ctx.StepX;
            ctx.StepY = (short)-ctx.StepY;
            ctx.Y -= ScrollBgStartYOffset;
        }
        else
        {
            ctx.Y += ScrollBgStartYOffset;
        }
        if (BattleAnimSystems.BattleAnimSystem_IsContest(system) == 1)
        {
            ctx.StepX = (short)-ctx.StepX;
        }
        ctx.BgAlpha = 0;
        ctx.OtherAlpha = ScrollBgOtherStartAlpha;
        ctx.ScrollFinished = 0;
        Bg.Bg_ToggleLayer(BattleBgBase, 0);
        LoadCustomBg(system, ctx.Common, ScriptVar(system, 0));
        Bg.Bg_SetOffset(ctx.Common.BgConfig!, BattleBgBase, 0, ctx.X);
        Bg.Bg_SetOffset(ctx.Common.BgConfig!, BattleBgBase, 3, ctx.Y);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(ctx.Common.BattleAnimSys!, BattleAnimTask_ScrollCustomBg, ctx);
    }

    // OFFSET 0x02227064
    private static void BattleAnimTask_MuddyWater(SysTask task, object? param)
    {
        MuddyWaterContext ctx = (MuddyWaterContext)param!;
        switch (ctx.Common.State)
        {
            case 0:
                SetBlendAlpha(BlendBg2, BlendBd | BlendObj | BlendBg0 | BlendBg3, ctx.BgAlpha, ctx.OtherAlpha);
                Bg.Bg_ToggleLayer(BattleBgBase, 1);
                ctx.Common.State++;
                goto case 1;
            case 1:
            {
                int completed = 0;
                if (ctx.BgAlpha < MuddyWaterBgEndAlpha)
                {
                    ctx.BgAlpha += ScrollBgAlphaStep;
                }
                else
                {
                    completed++;
                }
                if (ctx.OtherAlpha > MuddyWaterOtherEndAlpha)
                {
                    ctx.OtherAlpha -= ScrollBgAlphaStep;
                }
                else
                {
                    completed++;
                }
                if (completed == 2)
                {
                    ctx.BgAlpha = MuddyWaterBgEndAlpha;
                    ctx.OtherAlpha = MuddyWaterOtherEndAlpha;
                    ctx.Common.State++;
                }
                ChangeBlendAlpha(ctx.BgAlpha, ctx.OtherAlpha);
                break;
            }
            case 2:
                if (ctx.ScrollFinished != 0)
                {
                    ctx.Common.State++;
                }
                break;
            case 3:
            {
                int completed = 0;
                if (ctx.BgAlpha > 2)
                {
                    ctx.BgAlpha -= ScrollBgAlphaStep;
                }
                else
                {
                    completed++;
                }
                if (ctx.OtherAlpha < MuddyWaterOtherStartAlpha)
                {
                    ctx.OtherAlpha += ScrollBgAlphaStep;
                }
                else
                {
                    completed++;
                }
                if (completed == 2)
                {
                    ctx.BgAlpha = 0;
                    ctx.OtherAlpha = 31;
                    ctx.Common.State++;
                }
                ChangeBlendAlpha(ctx.BgAlpha, ctx.OtherAlpha);
                break;
            }
            default:
                Bg.Bg_ToggleLayer(BattleBgBase, 0);
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.Common.BattleAnimSys!, task);
                return;
        }
        ScrollBg(ctx.Common.BgConfig!, ref ctx.ScrollFinished, ref ctx.X, ref ctx.Y, ref ctx.StepX, ref ctx.StepY, ctx.SlowDownTime, ref ctx.Timer);
    }

    // OFFSET 0x022271D8
    private static void BattleAnimScriptFunc_MuddyWater(BattleAnimSystem system)
    {
        MuddyWaterContext ctx = BattleAnimUtil.BattleAnimUtil_Alloc<MuddyWaterContext>(system);
        BattleAnimUtil.BattleAnimSystem_GetCommonData(system, ctx.Common);
        ctx.X = (short)ScriptVar(system, 1);
        ctx.Y = (short)ScriptVar(system, 2);
        ctx.StepX = (short)ScriptVar(system, 3);
        ctx.StepY = (short)ScriptVar(system, 4);
        ctx.Reverse = ScriptVar(system, 5);
        ctx.Unused = (ushort)ScriptVar(system, 6);
        ctx.SlowDownTime = (ushort)ScriptVar(system, 7);
        ctx.Timer = 0;
        if (ctx.Reverse != 0 && BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system)) == BattlerSideEnemy)
        {
            ctx.X = (short)-ctx.X;
            ctx.Y = (short)-ctx.Y;
            ctx.StepX = (short)-ctx.StepX;
            ctx.StepY = (short)-ctx.StepY;
            ctx.Y -= ScrollBgStartYOffset;
        }
        else
        {
            ctx.Y += ScrollBgStartYOffset;
        }
        if (BattleAnimSystems.BattleAnimSystem_IsContest(system) == 1)
        {
            ctx.StepX = (short)-ctx.StepX;
        }
        ctx.BgAlpha = 0;
        ctx.OtherAlpha = MuddyWaterOtherStartAlpha;
        ctx.ScrollFinished = 0;
        Bg.Bg_ToggleLayer(BattleBgBase, 0);
        LoadCustomBg(system, ctx.Common, ScriptVar(system, 0));
        Bg.Bg_SetOffset(ctx.Common.BgConfig!, BattleBgBase, 0, ctx.X);
        Bg.Bg_SetOffset(ctx.Common.BgConfig!, BattleBgBase, 3, ctx.Y);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(ctx.Common.BattleAnimSys!, BattleAnimTask_MuddyWater, ctx);
    }
}
