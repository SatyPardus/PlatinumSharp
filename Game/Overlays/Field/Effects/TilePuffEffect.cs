namespace PlatinumSharp.Game;

public sealed class TilePuffRenderer
{
    public const int Size = 0x4;

    public FieldEffectManager? FieldEffectManager;
}

public sealed class TilePuffEffectParams
{
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public TilePuffRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class TilePuffEffectContext
{
    public const int Size = 0x24;

    public int State;
    public int LocalID;
    public int MapHeaderID;
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public TilePuffRenderer? Renderer;
    public MapObject? MapObject;
    public Billboard? Billboard;
}

public static class TilePuffEffect
{
    private const uint RendererID = 0x1B;
    private const int TaskPriorityOffset = 2;
    private const uint ModelID = 9;
    private const uint ModelMember = 0x6B;
    private const uint AnimID = 9;
    private const uint AnimMember = 0xB4;
    private const uint TextureID = 0xA;
    private const uint TextureMember = 0xC;
    private const uint ResourceID = 0xB;
    private const int AnimFrames = 7;
    private const int DepthOffset = 0x8000;

    private static readonly BillboardAnim[] sAnims =
    {
        new() { StartFrame = 0, EndFrame = 7, StopAtEnd = 1 },
        new() { StartFrame = 0, EndFrame = 0, StopAtEnd = 2 },
    };

    private static readonly OverworldAnimManagerFuncs sFuncs = new(TilePuffEffectContext.Size, () => new TilePuffEffectContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    // OFFSET 0x021F3E74
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        TilePuffRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        Renderer_Load(renderer);
        return renderer;
    }

    // OFFSET 0x021F3E90
    public static void Renderer_Free(object? context)
    {
        TilePuffRenderer renderer = (TilePuffRenderer)context!;
        Renderer_Unload(renderer);
    }

    // OFFSET 0x021F3EA0
    private static void Renderer_Load(TilePuffRenderer renderer)
    {
        FieldEffects.FieldEffectManager_LoadBillboardModel(renderer.FieldEffectManager!, ModelID, ModelMember);
        FieldEffects.FieldEffectManager_LoadBillboardAnim(renderer.FieldEffectManager!, AnimID, AnimMember);
        FieldEffects.FieldEffectManager_LoadBillboardTexture(renderer.FieldEffectManager!, TextureID, TextureMember, 1);
        FieldEffects.FieldEffectManager_InitBillboardResources(renderer.FieldEffectManager!, ResourceID, ModelID, AnimID, TextureID, 0, sAnims);
    }

    // OFFSET 0x021F3EE8
    private static void Renderer_Unload(TilePuffRenderer renderer)
    {
        FieldEffects.FieldEffectManager_FreeBillboardModel(renderer.FieldEffectManager!, ModelID);
        FieldEffects.FieldEffectManager_FreeBillboardAnim(renderer.FieldEffectManager!, AnimID);
        FieldEffects.FieldEffectManager_FreeBillboardTexture(renderer.FieldEffectManager!, TextureID);
        FieldEffects.FieldEffectManager_ReleaseBillboardResources(renderer.FieldEffectManager!, ResourceID);
    }

    // OFFSET 0x021F3F10
    public static void TilePuffEffect_New(MapObject mapObject)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        TilePuffEffectParams effectParams = new();
        effectParams.FieldSystem = MapObject.MapObject_FieldSystem(mapObject);
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.Renderer = (TilePuffRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        effectParams.MapObject = mapObject;
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        int x = MapObject.MapObject_GetX(mapObject);
        int z = MapObject.MapObject_GetZ(mapObject);
        MapObjectMove.VecFx32_SetPosFromMapCoords(x, z, ref position);
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFuncs, position, 0, effectParams, priority);
    }

    // OFFSET 0x021F3F74
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        TilePuffEffectContext context = (TilePuffEffectContext)contextObject!;
        TilePuffEffectParams effectParams = (TilePuffEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.FieldSystem = effectParams.FieldSystem;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.MapObject = effectParams.MapObject;
        context.LocalID = MapObject.MapObject_GetLocalID(context.MapObject!);
        context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(context.MapObject!);
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        context.Billboard = FieldEffects.FieldEffectManager_AddBillboardWithResourceID(context.FieldEffectManager!, ResourceID, position);
        return 1;
    }

    // OFFSET 0x021F3FB8
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        TilePuffEffectContext context = (TilePuffEffectContext)contextObject!;
        Billboards.Billboard_Delete(context.Billboard);
    }

    // OFFSET 0x021F3FC4
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        TilePuffEffectContext context = (TilePuffEffectContext)contextObject!;
        if (context.State == 0)
        {
            Billboards.Billboard_AdvanceAnim(context.Billboard, Fx32.One);
            if (Billboards.Billboard_GetAnimFrameNum(context.Billboard) / Fx32.One >= AnimFrames)
            {
                FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            }
        }
    }

    // OFFSET 0x021F3FF4
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        TilePuffEffectContext context = (TilePuffEffectContext)contextObject!;
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        position.Z += DepthOffset;
        Billboards.Billboard_SetPos(context.Billboard, position);
    }
}
