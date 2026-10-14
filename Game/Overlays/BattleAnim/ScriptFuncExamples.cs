namespace PlatinumSharp.Game;

public sealed class ExampleContext
{
    public byte State;
    public BattleAnimSystem? System;
}

public sealed class SpriteExampleContext
{
    public byte State;
    public BattleAnimSystem? BattleAnimSystem;
    public SpriteSystem? SpriteSys;
    public SpriteManager? SpriteMgr;
    public ManagedSprite? MainSprite;
    public readonly ManagedSprite?[] Sprites = new ManagedSprite?[100];
}

public static partial class BattleAnimScriptFuncs
{
    private const uint ExampleTaskPriority = 0x1000;

    // OFFSET 0x022269C0
    private static void BattleAnimScriptFunc_Nop(BattleAnimSystem system)
    {
    }

    // OFFSET 0x022269C4
    private static void BattleAnimTask_AnimExample(SysTask task, object? param)
    {
        ExampleContext ctx = (ExampleContext)param!;
        switch (ctx.State)
        {
            case 0:
                ctx.State++;
                break;
            case 1:
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(ctx.System!, task);
                break;
        }
    }

    // OFFSET 0x022269EC
    private static void BattleAnimScriptFunc_AnimExample(BattleAnimSystem system)
    {
        ExampleContext ctx = new();
        ctx.State = 0;
        ctx.System = system;
        BattleAnimSystems.BattleAnimSystem_StartAnimTaskEx(system, BattleAnimTask_AnimExample, ctx, ExampleTaskPriority);
    }

    // OFFSET 0x02226A14
    private static void BattleAnimTask_SoundExample(SysTask task, object? param)
    {
        ExampleContext ctx = (ExampleContext)param!;
        switch (ctx.State)
        {
            case 0:
                ctx.State++;
                break;
            case 1:
                BattleAnimSystems.BattleAnimSystem_EndSoundTask(ctx.System!, task);
                break;
        }
    }

    // OFFSET 0x02226A3C
    private static void BattleAnimScriptFunc_SoundExample(BattleAnimSystem system)
    {
        ExampleContext ctx = new();
        ctx.State = 0;
        ctx.System = system;
        BattleAnimSystems.BattleAnimSystem_StartSoundTask(system, BattleAnimTask_SoundExample, ctx, ExampleTaskPriority);
    }

    // OFFSET 0x02226A64
    private static void BattleAnimTask_GenericExample(SysTask task, object? param)
    {
        ExampleContext ctx = (ExampleContext)param!;
        switch (ctx.State)
        {
            case 0:
                ctx.State++;
                break;
            case 1:
                SysTask.SysTask_Done(task);
                break;
        }
    }

    // OFFSET 0x02226A88
    private static void BattleAnimScriptFunc_GenericExample(BattleAnimSystem system)
    {
        ExampleContext ctx = new();
        ctx.State = 0;
        ctx.System = system;
        SysTask.SysTask_Start(BattleAnimTask_GenericExample, ctx, ExampleTaskPriority);
    }

    // OFFSET 0x02226AAC
    private static void BattleAnimTask_SpriteExample(SysTask task, object? param)
    {
        SpriteExampleContext ctx = (SpriteExampleContext)param!;
        switch (ctx.State)
        {
            case 0:
                SpriteSystem.Sprite_TickFrame(ctx.Sprites[0]!.Sprite!);
                SpriteSystem.Sprite_TickFrame(ctx.Sprites[1]!.Sprite!);
                SpriteSystem.Sprite_TickFrame(ctx.Sprites[2]!.Sprite!);
                SpriteSystem.Sprite_OffsetPositionXY(ctx.Sprites[0]!.Sprite!, 1, 0);
                SpriteSystem.Sprite_OffsetPositionXY(ctx.Sprites[1]!.Sprite!, -1, 0);
                SpriteSystem.Sprite_OffsetPositionXY(ctx.Sprites[2]!.Sprite!, 1, 1);
                SpriteSystem.SpriteSystem_DrawSprites(ctx.SpriteMgr);
                break;
            case 1:
            {
                BattleAnimSystem system = ctx.BattleAnimSystem!;
                SpriteSystem.Sprite_DeleteAndFreeResources(ctx.MainSprite!);
                BattleAnimSystems.BattleAnimSystem_EndAnimTask(system, task);
                break;
            }
        }
    }

    // OFFSET 0x02226B1C
    private static void BattleAnimSpriteFunc_SpriteExample(BattleAnimSystem system, SpriteSystem spriteSys, SpriteManager spriteMan, ManagedSprite sprite)
    {
        SpriteExampleContext ctx = new();
        ctx.State = 0;
        ctx.SpriteSys = spriteSys;
        ctx.SpriteMgr = spriteMan;
        ctx.MainSprite = sprite;
        ctx.BattleAnimSystem = system;
        SpriteTemplate template = BattleAnimSystems.BattleAnimSystem_GetLastSpriteTemplate(system);
        ctx.Sprites[0] = sprite;
        for (int i = 1; i < 3; i++)
        {
            ctx.Sprites[i] = SpriteSystem.SpriteSystem_NewSprite(ctx.SpriteSys, ctx.SpriteMgr, template);
        }
        BattleAnimSystems.BattleAnimSystem_StartAnimTaskEx(system, BattleAnimTask_SpriteExample, ctx, ExampleTaskPriority);
    }
}
