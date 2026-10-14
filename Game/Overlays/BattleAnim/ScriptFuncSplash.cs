namespace PlatinumSharp.Game;

public sealed class SplashContext
{
    public byte State;
    public byte Bounce;
    public byte Loops;
    public short BaseY;
    public short SpriteOffset;
    public BattleAnimSystem? System;
    public PokemonSprite? Sprite;
    public readonly XYTransformContext Scale = new();
    public readonly XYTransformContext Pos = new();
}

public static partial class BattleAnimScriptFuncs
{
    private const uint SplashScaleTable = 0x0223A0C1;
    private const int SplashScaleEntrySize = 5;
    private const uint SplashPosTable = 0x0223A0A4;
    private const int SplashPosEntrySize = 3;
    private const int SplashBounces = 3;
    private const int SplashLoops = 3;
    private const short SplashScaleReference = 100;

    // OFFSET 0x0222BE80
    private static void BattleAnimTask_Splash(SysTask task, object? param)
    {
        SplashContext ctx = (SplashContext)param!;
        switch (ctx.State)
        {
            case 0:
            {
                byte[] scale = RomData.Overlay(12, (uint)(SplashScaleTable + ctx.Bounce * SplashScaleEntrySize), SplashScaleEntrySize);
                BattleAnimUtil.ScaleLerpContext_InitXY(ctx.Scale, scale[0], scale[1], scale[2], scale[3], SplashScaleReference, scale[4]);
                byte[] pos = RomData.Overlay(12, (uint)(SplashPosTable + ctx.Bounce * SplashPosEntrySize), SplashPosEntrySize);
                BattleAnimUtil.PosLerpContext_Init(ctx.Pos, 0, 0, (short)(ctx.BaseY + pos[0]), (short)(ctx.BaseY + pos[1]), pos[2]);
                ctx.Bounce++;
                ctx.State++;
                break;
            }
            case 1:
            {
                int done = 0;
                if (BattleAnimUtil.ScaleLerpContext_UpdateXY(ctx.Scale) == 0)
                {
                    done++;
                }
                if (BattleAnimUtil.PosLerpContext_Update(ctx.Pos) == 0)
                {
                    done++;
                }
                if (done >= 2)
                {
                    if (ctx.Bounce >= SplashBounces)
                    {
                        ctx.Loops++;
                        ctx.Bounce = 0;
                        if (ctx.Loops >= SplashLoops)
                        {
                            ctx.State++;
                        }
                    }
                    else
                    {
                        ctx.State = 0;
                    }
                }
                PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteScaleX, ctx.Scale.X);
                PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteScaleY, ctx.Scale.Y);
                BattleAnimUtil.BattleAnimUtil_SetPokemonSpriteAnchoredPosition(ctx.Sprite!, ctx.Pos.Y, ctx.SpriteOffset, ctx.Scale.Data[4], 0);
                break;
            }
            default:
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.System!, task);
                break;
        }
    }

    // OFFSET 0x0222BFA8
    private static void BattleAnimScriptFunc_Splash(BattleAnimSystem system)
    {
        SplashContext ctx = new();
        ctx.State = 0;
        ctx.Bounce = 0;
        ctx.System = system;
        ctx.Sprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(ctx.System, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
        ctx.BaseY = (short)PokemonSprite.PokemonSprite_GetAttribute(ctx.Sprite!, 1);
        ctx.SpriteOffset = (short)BattleAnimSystems.BattleAnimSystem_GetBattlerSpriteOffset(ctx.System, BattleAnimSystems.BattleAnimSystem_GetAttacker(ctx.System));
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(ctx.System, BattleAnimTask_Splash, ctx);
    }
}
