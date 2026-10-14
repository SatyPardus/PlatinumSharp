namespace PlatinumSharp.Game;

public sealed class SplashRenderer
{
    public const int Size = 0x3C;

    public FieldEffectManager? FieldEffectManager;
    public readonly Simple3DModel Model = new();
    public readonly Simple3DAnimation Animation = new();
}

public sealed class SplashEffectParams
{
    public FieldEffectManager? FieldEffectManager;
    public SplashRenderer? Renderer;
}

public sealed class SplashEffectContext
{
    public const int Size = 0x80;

    public readonly Simple3DAnimation Animation = new();
    public readonly G3dRenderObj RenderObj = new();
    public FieldEffectManager? FieldEffectManager;
    public SplashRenderer? Renderer;
}

public static class SplashEffect
{
    private const uint RendererID = 0x12;
    private const int ModelMember = 0x10;
    private const int AnimationMember = 0xA1;
    private const int TaskPriorityOffset = 2;

    private static readonly OverworldAnimManagerFuncs sFuncs = new(SplashEffectContext.Size, () => new SplashEffectContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    // OFFSET 0x021F2BD0
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        SplashRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        Renderer_Load(renderer);
        return renderer;
    }

    // OFFSET 0x021F2BEC
    public static void Renderer_Free(object? context)
    {
        SplashRenderer renderer = (SplashRenderer)context!;
        Renderer_Unload(renderer);
    }

    // OFFSET 0x021F2BFC
    private static void Renderer_Load(SplashRenderer renderer)
    {
        FieldEffects.FieldEffectManager_LoadModel(renderer.FieldEffectManager!, renderer.Model, 0, ModelMember, 0);
        FieldEffects.FieldEffectManager_LoadAnimation(renderer.FieldEffectManager!, renderer.Animation, 0, AnimationMember, 0);
    }

    // OFFSET 0x021F2C24
    private static void Renderer_Unload(SplashRenderer renderer)
    {
        Simple3D.Simple3DModel_Free(renderer.Model);
        Simple3D.Simple3DAnimation_Free(renderer.Animation);
    }

    // OFFSET 0x021F2C38
    public static void SplashEffect_New(MapObject mapObject, int x, int y, int z)
    {
        SplashEffectParams effectParams = new();
        effectParams.FieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject);
        effectParams.Renderer = (SplashRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(effectParams.FieldEffectManager!, RendererID);
        VecFx32 position = default;
        MapObjectMove.VecFx32_SetPosFromMapCoords(x, z, ref position);
        position.Z += y << 3;
        position.Y = (MapObject.MapObject_GetYPrev(mapObject) << 15) - 0x8000;
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        FieldEffects.FieldEffectManager_InitAnimManager(effectParams.FieldEffectManager!, sFuncs, position, 0, effectParams, priority);
    }

    // OFFSET 0x021F2C98
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        SplashEffectContext context = (SplashEffectContext)contextObject!;
        SplashEffectParams effectParams = (SplashEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        FieldEffects.FieldEffectManager_ApplyAnimCopyToModel(context.FieldEffectManager!, context.Animation, context.Renderer!.Model, context.Renderer.Animation, 0);
        Simple3D.Simple3DRenderObj_InitWithAnimation(context.RenderObj, context.Renderer.Model, context.Animation);
        return 1;
    }

    // OFFSET 0x021F2CD0
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        SplashEffectContext context = (SplashEffectContext)contextObject!;
        Simple3D.Simple3DAnimation_Free(context.Animation);
    }

    // OFFSET 0x021F2CDC
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        SplashEffectContext context = (SplashEffectContext)contextObject!;
        if (Simple3D.Simple3DAnimation_IsFinished(context.Animation) == 1)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        Simple3D.Simple3DAnimation_Advance(context.Animation, Fx32.One, 0);
    }

    // OFFSET 0x021F2D04
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        SplashEffectContext context = (SplashEffectContext)contextObject!;
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        Simple3D.Simple3DRenderObj_DrawAtPosition(context.RenderObj, position);
    }
}
