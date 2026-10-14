namespace PlatinumSharp.Game;

public sealed class QuickAttackContext
{
    public BattleAnimSystem? System;
    public SpriteManager? SpriteMan;
    public PokemonSprite? Sprite;
    public readonly XYTransformContext Revolution = new();
    public readonly ManagedSprite?[] Afterimages = new ManagedSprite?[2];
    public readonly AfterimageContext Afterimage = new();
    public short X;
    public short Y;
    public short ShadowHeight;
}

public static partial class BattleAnimScriptFuncs
{
    private const int QuickAttackYOffset = 8;
    private const int QuickAttackSpritePriority = 100;
    private const int MonSpriteShadowHeightAttr = 0x29;

    // OFFSET 0x0222F700
    private static void BattleAnimTask_QuickAttack(SysTask task, object? param)
    {
        QuickAttackContext ctx = (QuickAttackContext)param!;
        int[] active = new int[3];
        active[0] = BattleAnimUtil.RevolutionContext_UpdateAndApplyToMon(ctx.Revolution, ctx.X, ctx.Y, ctx.Sprite!);
        active[1] = BattleAnimUtil.Afterimage_Update(ctx.Afterimage);
        active[2] = 0;
        SpriteSystem.SpriteSystem_DrawSprites(ctx.SpriteMan);
        for (int i = 0; i < 3; i++)
        {
            if (active[i] == 1)
            {
                return;
            }
        }
        PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, 0, ctx.X);
        PokemonSprite.PokemonSprite_SetAttribute(ctx.Sprite!, 1, ctx.Y - QuickAttackYOffset);
        BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.System!, task);
    }

    // OFFSET 0x0222F770
    private static void BattleAnimScriptFunc_QuickAttack(BattleAnimSystem system)
    {
        QuickAttackContext ctx = new();
        ctx.System = system;
        ctx.SpriteMan = BattleAnimSystems.BattleAnimSystem_GetPokemonSpriteManager(system);
        ctx.Sprite = BattleAnimSystems.BattleAnimSystem_GetBattlerSprite(system, BattleAnimSystems.BattleAnimSystem_GetAttacker(system));
        ctx.X = (short)PokemonSprite.PokemonSprite_GetAttribute(ctx.Sprite!, 0);
        ctx.Y = (short)PokemonSprite.PokemonSprite_GetAttribute(ctx.Sprite!, 1);
        ctx.ShadowHeight = (short)PokemonSprite.PokemonSprite_GetAttribute(ctx.Sprite!, MonSpriteShadowHeightAttr);
        ctx.Y += QuickAttackYOffset;
        BattleAnimUtil.RevolutionContext_InitOvalRevolutions(ctx.Revolution, 1, 8);
        XYTransformContext afterimageRevolution = new();
        BattleAnimUtil.RevolutionContext_InitOvalRevolutions(afterimageRevolution, 1, 10);
        int dir = BattleAnimUtil.BattleAnimUtil_GetTransformDirectionX(ctx.System, BattleAnimSystems.BattleAnimSystem_GetAttacker(ctx.System));
        ctx.Revolution.Data[2] *= dir;
        afterimageRevolution.Data[2] *= dir;
        for (int i = 0; i < 2; i++)
        {
            ctx.Afterimages[i] = BattleAnimSystems.BattleAnimSystem_GetPokemonSprite(ctx.System, i);
            SpriteSystem.ManagedSprite_SetPriority(ctx.Afterimages[i]!, QuickAttackSpritePriority);
            SpriteSystem.ManagedSprite_SetExplicitPriority(ctx.Afterimages[i]!, 1);
            SpriteSystem.ManagedSprite_SetExplicitOamMode(ctx.Afterimages[i]!, 1);
        }
        BattleAnimUtil.Afterimage_Init(ctx.Afterimage, afterimageRevolution, BattleAnimUtil.RevolutionContext_Update, ctx.X, (short)(ctx.Y - ctx.ShadowHeight), 2, 2, 0, ctx.Afterimages[0], ctx.Afterimages[1], null, null);
        BattleAnimUtil.BattleAnimUtil_SetSpriteBgBlending(ctx.System, -1, -1);
        BattleAnimSystems.BattleAnimSystem_StartAnimTask(ctx.System, BattleAnimTask_QuickAttack, ctx);
    }
}
