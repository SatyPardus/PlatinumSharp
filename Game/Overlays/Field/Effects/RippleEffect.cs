namespace PlatinumSharp.Game;

public sealed class RippleRenderer
{
    public const int Size = 0x3C;

    public FieldEffectManager? FieldEffectManager;
    public readonly Simple3DModel Model = new();
    public readonly Simple3DAnimation Animation = new();
}

public sealed class RippleEffectParams
{
    public FieldEffectManager? FieldEffectManager;
    public RippleRenderer? Renderer;
}

public sealed class RippleEffectContext
{
    public const int Size = 0x80;

    public readonly Simple3DAnimation Animation = new();
    public readonly G3dRenderObj RenderObj = new();
    public FieldEffectManager? FieldEffectManager;
    public RippleRenderer? Renderer;
}

public static class RippleEffect
{
    private const uint RendererID = 0x11;
    private const int ModelMember = 0xF;
    private const int AnimationMember = 0xA0;
    private const int TaskPriorityOffset = 2;

    private static readonly OverworldAnimManagerFuncs sFuncs = new(RippleEffectContext.Size, () => new RippleEffectContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    // OFFSET 0x021F2A7C
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        RippleRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        Renderer_Load(renderer);
        return renderer;
    }

    // OFFSET 0x021F2A98
    public static void Renderer_Free(object? context)
    {
        RippleRenderer renderer = (RippleRenderer)context!;
        Renderer_Unload(renderer);
    }

    // OFFSET 0x021F2AA8
    private static void Renderer_Load(RippleRenderer renderer)
    {
        FieldEffects.FieldEffectManager_LoadModel(renderer.FieldEffectManager!, renderer.Model, 0, ModelMember, 0);
        FieldEffects.FieldEffectManager_LoadAnimation(renderer.FieldEffectManager!, renderer.Animation, 0, AnimationMember, 0);
    }

    // OFFSET 0x021F2AD0
    private static void Renderer_Unload(RippleRenderer renderer)
    {
        Simple3D.Simple3DModel_Free(renderer.Model);
        Simple3D.Simple3DAnimation_Free(renderer.Animation);
    }

    // OFFSET 0x021F2AE4
    public static void RippleEffect_New(MapObject mapObject, int x, int y, int z)
    {
        RippleEffectParams effectParams = new();
        effectParams.FieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject);
        effectParams.Renderer = (RippleRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(effectParams.FieldEffectManager!, RendererID);
        VecFx32 position = default;
        MapObjectMove.VecFx32_SetPosFromMapCoords(x, z, ref position);
        position.Z += (y << 3) + 0x1000;
        position.Y = (MapObject.MapObject_GetYPrev(mapObject) << 15) - 0x8000;
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        FieldEffects.FieldEffectManager_InitAnimManager(effectParams.FieldEffectManager!, sFuncs, position, 0, effectParams, priority);
    }

    // OFFSET 0x021F2B48
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        RippleEffectContext context = (RippleEffectContext)contextObject!;
        RippleEffectParams effectParams = (RippleEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        FieldEffects.FieldEffectManager_ApplyAnimCopyToModel(context.FieldEffectManager!, context.Animation, context.Renderer!.Model, context.Renderer.Animation, 0);
        Simple3D.Simple3DRenderObj_InitWithAnimation(context.RenderObj, context.Renderer.Model, context.Animation);
        return 1;
    }

    // OFFSET 0x021F2B80
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        RippleEffectContext context = (RippleEffectContext)contextObject!;
        Simple3D.Simple3DAnimation_Free(context.Animation);
    }

    // OFFSET 0x021F2B8C
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        RippleEffectContext context = (RippleEffectContext)contextObject!;
        if (Simple3D.Simple3DAnimation_IsFinished(context.Animation) == 1)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        Simple3D.Simple3DAnimation_Advance(context.Animation, Fx32.One, 0);
    }

    // OFFSET 0x021F2BB4
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        RippleEffectContext context = (RippleEffectContext)contextObject!;
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        Simple3D.Simple3DRenderObj_DrawAtPosition(context.RenderObj, position);
    }
}
