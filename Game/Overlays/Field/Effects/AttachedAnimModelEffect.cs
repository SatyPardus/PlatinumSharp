namespace PlatinumSharp.Game;

public sealed class AttachedAnimModelRenderer
{
    public const int Size = 0x3C;

    public FieldEffectManager? FieldEffectManager;
    public readonly Simple3DModel Model = new();
    public readonly Simple3DAnimation Animation = new();
}

public sealed class AttachedAnimModelEffectParams
{
    public FieldEffectManager? FieldEffectManager;
    public AttachedAnimModelRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class AttachedAnimModelEffectContext
{
    public const int Size = 0x94;

    public int LocalID;
    public int MapHeaderID;
    public int Finishing;
    public FieldEffectManager? FieldEffectManager;
    public AttachedAnimModelRenderer? Renderer;
    public MapObject? MapObject;
    public readonly Simple3DAnimation Animation = new();
    public readonly G3dRenderObj RenderObj = new();
}

public static class AttachedAnimModelEffect
{
    private const uint RendererID = 0xD;
    private const int ModelMember = 0x4E;
    private const int AnimationMember = 0xA5;
    private const int TaskPriorityOffset = 2;

    private static readonly OverworldAnimManagerFuncs sFuncs = new(AttachedAnimModelEffectContext.Size, () => new AttachedAnimModelEffectContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    // OFFSET 0x021F23D0
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        AttachedAnimModelRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        Renderer_Load(renderer);
        return renderer;
    }

    // OFFSET 0x021F23EC
    public static void Renderer_Free(object? context)
    {
        AttachedAnimModelRenderer renderer = (AttachedAnimModelRenderer)context!;
        Renderer_Unload(renderer);
    }

    // OFFSET 0x021F23FC
    private static void Renderer_Load(AttachedAnimModelRenderer renderer)
    {
        FieldEffects.FieldEffectManager_LoadModel(renderer.FieldEffectManager!, renderer.Model, 0, ModelMember, 0);
        FieldEffects.FieldEffectManager_LoadAnimation(renderer.FieldEffectManager!, renderer.Animation, 0, AnimationMember, 0);
    }

    // OFFSET 0x021F2424
    private static void Renderer_Unload(AttachedAnimModelRenderer renderer)
    {
        Simple3D.Simple3DModel_Free(renderer.Model);
        Simple3D.Simple3DAnimation_Free(renderer.Animation);
    }

    // OFFSET 0x021F2438
    public static OverworldAnimManager? AttachedAnimModelEffect_New(MapObject mapObject)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        AttachedAnimModelEffectParams effectParams = new();
        effectParams.Renderer = (AttachedAnimModelRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        VecFx32 position = default;
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.MapObject = mapObject;
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        return FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFuncs, position, 0, effectParams, priority);
    }

    // OFFSET 0x021F247C
    public static void AttachedAnimModelEffect_StartFinishing(OverworldAnimManager animManager)
    {
        AttachedAnimModelEffectContext context = (AttachedAnimModelEffectContext)OverworldAnimManagers.OverworldAnimManager_GetContext(animManager)!;
        context.Finishing = 1;
    }

    // OFFSET 0x021F2488
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        AttachedAnimModelEffectContext context = (AttachedAnimModelEffectContext)contextObject!;
        AttachedAnimModelEffectParams effectParams = (AttachedAnimModelEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.MapObject = effectParams.MapObject;
        AttachedAnimModelRenderer renderer = context.Renderer!;
        FieldEffects.FieldEffectManager_ApplyAnimCopyToModel(context.FieldEffectManager!, context.Animation, renderer.Model, renderer.Animation, 0);
        Simple3D.Simple3DRenderObj_InitWithAnimation(context.RenderObj, context.Renderer!.Model, context.Animation);
        context.LocalID = MapObject.MapObject_GetLocalID(context.MapObject!);
        context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(context.MapObject!);
        MapObject mapObject = context.MapObject!;
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        MapObject.MapObject_CopySpriteJumpOffset(mapObject, out VecFx32 jumpOffset);
        position.X += jumpOffset.X;
        position.Y += jumpOffset.Y;
        position.Z += jumpOffset.Z;
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
        return 1;
    }

    // OFFSET 0x021F2510
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        AttachedAnimModelEffectContext context = (AttachedAnimModelEffectContext)contextObject!;
        Simple3D.Simple3DAnimation_Free(context.Animation);
    }

    // OFFSET 0x021F251C
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        AttachedAnimModelEffectContext context = (AttachedAnimModelEffectContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        if (context.Finishing == 0)
        {
            if (MapObject.MapObject_IsLocalObjectOfMap(mapObject, context.LocalID, context.MapHeaderID) == 0)
            {
                FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
                return;
            }
        }
        else
        {
            if (Simple3D.Simple3DAnimation_IsFinished(context.Animation) == 1)
            {
                FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
                return;
            }
            Simple3D.Simple3DAnimation_Advance(context.Animation, Fx32.One, 0);
        }
        if (context.Finishing == 0)
        {
            MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
            MapObject.MapObject_CopySpriteJumpOffset(mapObject, out VecFx32 jumpOffset);
            position.X += jumpOffset.X;
            position.Y += jumpOffset.Y;
            position.Z += jumpOffset.Z;
            OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
        }
    }

    // OFFSET 0x021F25A4
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        AttachedAnimModelEffectContext context = (AttachedAnimModelEffectContext)contextObject!;
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        Simple3D.Simple3DRenderObj_DrawAtPosition(context.RenderObj, position);
    }
}
