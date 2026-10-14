namespace PlatinumSharp.Game;

public sealed class CarriedIconRenderer
{
    public const int Size = 0x4;

    public FieldEffectManager? FieldEffectManager;
}

public sealed class CarriedIconEffectParams
{
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public CarriedIconRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class CarriedIconEffectContext
{
    public const int Size = 0x24;

    public int Unused00;
    public int LocalID;
    public int MapHeaderID;
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public CarriedIconRenderer? Renderer;
    public MapObject? MapObject;
    public Billboard? Billboard;
}

public static class CarriedIconEffect
{
    private const uint RendererID = 0x16;
    private const int TaskPriorityOffset = 2;
    private const uint ModelID = 4;
    private const uint ModelMember = 0x57;
    private const uint SharedAnimID = 0;
    private const uint TextureID = 3;
    private const uint TextureMember = 6;
    private const uint ResourceID = 4;
    private const int DepthOffset = 0xC000;

    private static readonly BillboardAnim[] sAnims =
    {
        new() { StartFrame = 0, EndFrame = 1, StopAtEnd = 1 },
        new() { StartFrame = 0, EndFrame = 0, StopAtEnd = 2 },
    };

    private static readonly OverworldAnimManagerFuncs sFuncs = new(CarriedIconEffectContext.Size, () => new CarriedIconEffectContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    // OFFSET 0x021F600C
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        CarriedIconRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        Renderer_Load(renderer);
        return renderer;
    }

    // OFFSET 0x021F6028
    public static void Renderer_Free(object? context)
    {
        CarriedIconRenderer renderer = (CarriedIconRenderer)context!;
        Renderer_Unload(renderer);
    }

    // OFFSET 0x021F6038
    private static void Renderer_Load(CarriedIconRenderer renderer)
    {
        FieldEffects.FieldEffectManager_LoadBillboardModel(renderer.FieldEffectManager!, ModelID, ModelMember);
        FieldEffects.FieldEffectManager_LoadBillboardTexture(renderer.FieldEffectManager!, TextureID, TextureMember, 1);
        FieldEffects.FieldEffectManager_InitBillboardResources(renderer.FieldEffectManager!, ResourceID, ModelID, SharedAnimID, TextureID, 0, sAnims);
    }

    // OFFSET 0x021F6074
    private static void Renderer_Unload(CarriedIconRenderer renderer)
    {
        FieldEffects.FieldEffectManager_FreeBillboardModel(renderer.FieldEffectManager!, ModelID);
        FieldEffects.FieldEffectManager_FreeBillboardTexture(renderer.FieldEffectManager!, TextureID);
        FieldEffects.FieldEffectManager_ReleaseBillboardResources(renderer.FieldEffectManager!, ResourceID);
    }

    // OFFSET 0x021F6094
    public static OverworldAnimManager? CarriedIconEffect_New(MapObject mapObject)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        CarriedIconEffectParams effectParams = new();
        effectParams.FieldSystem = MapObject.MapObject_FieldSystem(mapObject);
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.Renderer = (CarriedIconRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        VecFx32 position = default;
        effectParams.MapObject = mapObject;
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        return FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFuncs, position, 0, effectParams, priority);
    }

    // OFFSET 0x021F60E4
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        CarriedIconEffectContext context = (CarriedIconEffectContext)contextObject!;
        VecFx32 position = default;
        CarriedIconEffectParams effectParams = (CarriedIconEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.FieldSystem = effectParams.FieldSystem;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.MapObject = effectParams.MapObject;
        context.LocalID = MapObject.MapObject_GetLocalID(context.MapObject!);
        context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(context.MapObject!);
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
        context.Billboard = FieldEffects.FieldEffectManager_AddBillboardWithResourceID(context.FieldEffectManager!, ResourceID, position);
        return 1;
    }

    // OFFSET 0x021F6134
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        CarriedIconEffectContext context = (CarriedIconEffectContext)contextObject!;
        Billboards.Billboard_Delete(context.Billboard);
    }

    // OFFSET 0x021F6140
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        CarriedIconEffectContext context = (CarriedIconEffectContext)contextObject!;
        if (MapObject.MapObject_IsLocalObjectOfMap(context.MapObject!, context.LocalID, context.MapHeaderID) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
        }
    }

    // OFFSET 0x021F615C
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        CarriedIconEffectContext context = (CarriedIconEffectContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        if (MapObject.MapObject_IsLocalObjectOfMap(mapObject, context.LocalID, context.MapHeaderID) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        MapObject.MapObject_CopySpriteJumpOffset(mapObject, out VecFx32 jumpOffset);
        position.X += jumpOffset.X;
        position.Y += jumpOffset.Y;
        position.Z += jumpOffset.Z;
        position.Z += DepthOffset;
        Billboards.Billboard_SetPos(context.Billboard, position);
    }
}
