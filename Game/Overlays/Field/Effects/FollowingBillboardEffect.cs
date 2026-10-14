namespace PlatinumSharp.Game;

public sealed class FollowingBillboardRenderer
{
    public const int Size = 0x4;

    public FieldEffectManager? FieldEffectManager;
}

public sealed class FollowingBillboardEffectParams
{
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public FollowingBillboardRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class FollowingBillboardEffectContext
{
    public const int Size = 0x28;

    public int State;
    public int GraphicsID;
    public int LocalID;
    public int MapHeaderID;
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public FollowingBillboardRenderer? Renderer;
    public MapObject? MapObject;
    public Billboard? Billboard;
}

public static class FollowingBillboardEffect
{
    private const uint RendererID = 0x15;
    private const int TaskPriorityOffset = 2;
    private const uint ModelID = 3;
    private const uint ModelMember = 0x56;
    private const uint AnimID = 2;
    private const uint AnimMember = 0xAE;
    private const uint TextureID = 2;
    private const uint TextureMember = 5;
    private const uint ResourceID = 3;
    private const int AnimFrames = 0xC;
    private const int Looping = 1;
    private const int DepthOffset = 0x8000;

    private static readonly BillboardAnim[] sAnims =
    {
        new() { StartFrame = 0, EndFrame = 0xC, StopAtEnd = 1 },
        new() { StartFrame = 0, EndFrame = 0, StopAtEnd = 2 },
    };

    private static readonly OverworldAnimManagerFuncs sFuncs = new(FollowingBillboardEffectContext.Size, () => new FollowingBillboardEffectContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    // OFFSET 0x021F3284
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        FollowingBillboardRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        Renderer_Load(renderer);
        return renderer;
    }

    // OFFSET 0x021F32A0
    public static void Renderer_Free(object? context)
    {
        FollowingBillboardRenderer renderer = (FollowingBillboardRenderer)context!;
        Renderer_Unload(renderer);
    }

    // OFFSET 0x021F32B0
    private static void Renderer_Load(FollowingBillboardRenderer renderer)
    {
        FieldEffects.FieldEffectManager_LoadBillboardModel(renderer.FieldEffectManager!, ModelID, ModelMember);
        FieldEffects.FieldEffectManager_LoadBillboardAnim(renderer.FieldEffectManager!, AnimID, AnimMember);
        FieldEffects.FieldEffectManager_LoadBillboardTexture(renderer.FieldEffectManager!, TextureID, TextureMember, 1);
        FieldEffects.FieldEffectManager_InitBillboardResources(renderer.FieldEffectManager!, ResourceID, ModelID, AnimID, TextureID, 0, sAnims);
    }

    // OFFSET 0x021F32F4
    private static void Renderer_Unload(FollowingBillboardRenderer renderer)
    {
        FieldEffects.FieldEffectManager_FreeBillboardModel(renderer.FieldEffectManager!, ModelID);
        FieldEffects.FieldEffectManager_FreeBillboardAnim(renderer.FieldEffectManager!, AnimID);
        FieldEffects.FieldEffectManager_FreeBillboardTexture(renderer.FieldEffectManager!, TextureID);
        FieldEffects.FieldEffectManager_ReleaseBillboardResources(renderer.FieldEffectManager!, ResourceID);
    }

    // OFFSET 0x021F331C
    public static void FollowingBillboardEffect_New(MapObject mapObject, int looping)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        FollowingBillboardEffectParams effectParams = new();
        effectParams.FieldSystem = MapObject.MapObject_FieldSystem(mapObject);
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.Renderer = (FollowingBillboardRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        VecFx32 position = default;
        effectParams.MapObject = mapObject;
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFuncs, position, looping, effectParams, priority);
    }

    // OFFSET 0x021F336C
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        FollowingBillboardEffectContext context = (FollowingBillboardEffectContext)contextObject!;
        VecFx32 position = default;
        FollowingBillboardEffectParams effectParams = (FollowingBillboardEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.FieldSystem = effectParams.FieldSystem;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.MapObject = effectParams.MapObject;
        context.GraphicsID = MapObject.MapObject_GetEffectiveGraphicsID(context.MapObject!);
        context.LocalID = MapObject.MapObject_GetLocalID(context.MapObject!);
        context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(context.MapObject!);
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
        context.Billboard = FieldEffects.FieldEffectManager_AddBillboardWithResourceID(context.FieldEffectManager!, ResourceID, position);
        return 1;
    }

    // OFFSET 0x021F33C4
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        FollowingBillboardEffectContext context = (FollowingBillboardEffectContext)contextObject!;
        Billboards.Billboard_Delete(context.Billboard);
    }

    // OFFSET 0x021F33D0
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        FollowingBillboardEffectContext context = (FollowingBillboardEffectContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        if (MapObject.MapObject_IsLocalObjectOfMapWithGraphics(mapObject, context.GraphicsID, context.LocalID, context.MapHeaderID) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        int looping = OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager);
        if (looping == Looping && MapObject.MapObject_IsStatus26On(mapObject) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        if (context.State == 0)
        {
            Billboards.Billboard_AdvanceAnim(context.Billboard, Fx32.One);
            if (Billboards.Billboard_GetAnimFrameNum(context.Billboard) / Fx32.One >= AnimFrames)
            {
                if (looping == 0)
                {
                    FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
                    return;
                }
                Billboards.Billboard_SetFrameNum(context.Billboard, 0);
            }
        }
    }

    // OFFSET 0x021F3448
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        FollowingBillboardEffectContext context = (FollowingBillboardEffectContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        if (MapObject.MapObject_IsLocalObjectOfMapWithGraphics(mapObject, context.GraphicsID, context.LocalID, context.MapHeaderID) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        position.Z += DepthOffset;
        Billboards.Billboard_SetPos(context.Billboard, position);
    }
}
