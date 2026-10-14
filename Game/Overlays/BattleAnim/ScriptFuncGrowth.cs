namespace PlatinumSharp.Game;

public sealed class GrowthContext
{
    public byte State;
    public byte Pulses;
    public byte FadeToggle;
    public BattleAnimSystem? System;
    public PokemonSprite? Sprite;
    public readonly XYTransformContext Scale = new();
}

public static partial class BattleAnimScriptFuncs
{
    private const uint GrowthScaleTable = 0x0223A098;
    private const int GrowthScaleEntrySize = 3;
    private const int GrowthPulses = 4;
    private const short GrowthScaleReference = 100;
    private const int GrowthFadeAlpha = 6;
    private const int GrowthFadeColor = 0x7FFF;

    // OFFSET 0x0222B914
    private static void BattleAnimTask_Growth(SysTask task, object? param)
    {
        GrowthContext ctx = (GrowthContext)param!;
        switch (ctx.State)
        {
            case 0:
            {
                byte[] scale = RomData.Overlay(12, (uint)(GrowthScaleTable + ctx.FadeToggle * GrowthScaleEntrySize), GrowthScaleEntrySize);
                BattleAnimUtil.ScaleLerpContext_Init(ctx.Scale, scale[0], GrowthScaleReference, scale[1], scale[2]);
                if (ctx.FadeToggle == 0)
                {
                    PokemonSprite.PokemonSprite_StartFade(ctx.Sprite!, 0, GrowthFadeAlpha, 0, GrowthFadeColor);
                }
                else
                {
                    PokemonSprite.PokemonSprite_StartFade(ctx.Sprite!, GrowthFadeAlpha, 0, 0, GrowthFadeColor);
                }
                ctx.FadeToggle ^= 1;
                ctx.Pulses++;
                ctx.State++;
                break;
            }
            case 1:
                if (BattleAnimUtil.ScaleLerpContext_Update(ctx.Scale) == 0 && !PokemonSprite.PokemonSprite_IsFadeActive(ctx.Sprite!))
                {
                    if (ctx.Pulses < GrowthPulses)
                    {
                        ctx.State--;
                    }
                    else
                    {
                        ctx.State++;
                    }
                }
                PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteScaleX, ctx.Scale.X);
                PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, MonSpriteScaleY, ctx.Scale.Y);
                break;
            default:
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.System!, task);
                break;
        }
    }

    // OFFSET 0x0222B9E0
    private static void BattleAnimScriptFunc_Growth(BattleAnimSystem system)
    {
        GrowthContext ctx = new();
        ctx.State = 0;
        ctx.FadeToggle = 0;
        ctx.Pulses = 0;
        ctx.System = system;
        ctx.Sprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(ctx.System, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(ctx.System, BattleAnimTask_Growth, ctx);
    }
}
