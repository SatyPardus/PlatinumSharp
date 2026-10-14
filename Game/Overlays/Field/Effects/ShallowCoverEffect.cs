namespace PlatinumSharp.Game;

public sealed class ShallowCoverRenderer
{
    public const int Size = 0x4;

    public FieldEffectManager? FieldEffectManager;
}

public sealed class ShallowCoverEffectParams
{
    public int X;
    public int Y;
    public int Z;
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public ShallowCoverRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class ShallowCoverEffectContext
{
    public const int Size = 0x38;

    public int State;
    public int GraphicsID;
    public int LocalID;
    public int MapHeaderID;
    public int HeightFound;
    public int X;
    public int Y;
    public int Z;
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public ShallowCoverRenderer? Renderer;
    public MapObject? MapObject;
    public Billboard? Billboard;
}

public static class ShallowCoverEffect
{
    private const uint RendererID = 0x19;
    private const int TaskPriorityOffset = 2;
    private const uint ModelID = 8;
    private const uint ModelMember = 0x5A;
    private const uint AnimID = 8;
    private const uint AnimMember = 0xB1;
    private const uint TextureID = 9;
    private const uint TextureMember = 9;
    private const uint ResourceID = 0xA;
    private const int EnterFrames = 2;
    private const int StateEnter = 0;
    private const int StateSettle = 1;
    private const int StateIdle = 2;

    private static readonly BillboardAnim[] sAnims =
    {
        new() { StartFrame = 0, EndFrame = 2, StopAtEnd = 0 },
        new() { StartFrame = 0, EndFrame = 0, StopAtEnd = 2 },
    };

    private static readonly OverworldAnimManagerFuncs sFuncs = new(ShallowCoverEffectContext.Size, () => new ShallowCoverEffectContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    // OFFSET 0x021F3A50
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        ShallowCoverRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        Renderer_Load(renderer);
        return renderer;
    }

    // OFFSET 0x021F3A6C
    public static void Renderer_Free(object? context)
    {
        ShallowCoverRenderer renderer = (ShallowCoverRenderer)context!;
        Renderer_Unload(renderer);
    }

    // OFFSET 0x021F3A7C
    private static void Renderer_Load(ShallowCoverRenderer renderer)
    {
        FieldEffects.FieldEffectManager_LoadBillboardModel(renderer.FieldEffectManager!, ModelID, ModelMember);
        FieldEffects.FieldEffectManager_LoadBillboardAnim(renderer.FieldEffectManager!, AnimID, AnimMember);
        FieldEffects.FieldEffectManager_LoadBillboardTexture(renderer.FieldEffectManager!, TextureID, TextureMember, 1);
        FieldEffects.FieldEffectManager_InitBillboardResources(renderer.FieldEffectManager!, ResourceID, ModelID, AnimID, TextureID, 0, sAnims);
    }

    // OFFSET 0x021F3AC4
    private static void Renderer_Unload(ShallowCoverRenderer renderer)
    {
        FieldEffects.FieldEffectManager_FreeBillboardModel(renderer.FieldEffectManager!, ModelID);
        FieldEffects.FieldEffectManager_FreeBillboardAnim(renderer.FieldEffectManager!, AnimID);
        FieldEffects.FieldEffectManager_FreeBillboardTexture(renderer.FieldEffectManager!, TextureID);
        FieldEffects.FieldEffectManager_ReleaseBillboardResources(renderer.FieldEffectManager!, ResourceID);
    }

    // OFFSET 0x021F3AEC
    public static void ShallowCoverEffect_New(MapObject mapObject, int skipEnter)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        ShallowCoverEffectParams effectParams = new();
        effectParams.X = MapObject.MapObject_GetX(mapObject);
        effectParams.Y = MapObject.MapObject_GetY(mapObject);
        effectParams.Z = MapObject.MapObject_GetZ(mapObject);
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.FieldSystem = FieldEffects.FieldEffectManager_GetFieldSystem(fieldEffectManager);
        effectParams.Renderer = (ShallowCoverRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        effectParams.MapObject = mapObject;
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFuncs, position, skipEnter, effectParams, priority);
    }

    // OFFSET 0x021F3B54
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        ShallowCoverEffectContext context = (ShallowCoverEffectContext)contextObject!;
        ShallowCoverEffectParams effectParams = (ShallowCoverEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.X = effectParams.X;
        context.Y = effectParams.Y;
        context.Z = effectParams.Z;
        context.FieldSystem = effectParams.FieldSystem;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.MapObject = effectParams.MapObject;
        context.GraphicsID = MapObject.MapObject_GetEffectiveGraphicsID(context.MapObject!);
        context.LocalID = MapObject.MapObject_GetLocalID(context.MapObject!);
        context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(context.MapObject!);
        VecFx32 position;
        position.X = context.X << 16;
        position.Z = context.Z << 16;
        position.Y = MapObject.MapObject_GetPosY(context.MapObject!);
        context.HeightFound = MapObjectMove.MapObject_RecalculatePositionHeight(context.FieldSystem!, ref position);
        position.X += 0x7800;
        position.Y -= 0x8000;
        position.Z += 0xD000;
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
        context.Billboard = FieldEffects.FieldEffectManager_AddBillboardWithResourceID(context.FieldEffectManager!, ResourceID, position);
        if (OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager) == 0)
        {
            context.State = StateSettle;
        }
        return 1;
    }

    // OFFSET 0x021F3BF4
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        ShallowCoverEffectContext context = (ShallowCoverEffectContext)contextObject!;
        Billboards.Billboard_Delete(context.Billboard);
    }

    // OFFSET 0x021F3C00
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        ShallowCoverEffectContext context = (ShallowCoverEffectContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        if (MapObject.MapObject_IsLocalObjectOfMapWithGraphics(mapObject, context.GraphicsID, context.LocalID, context.MapHeaderID) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        if (context.HeightFound == 0)
        {
            OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 current);
            VecFx32 position;
            position.X = context.X << 16;
            position.Z = context.Z << 16;
            position.Y = current.Y;
            context.HeightFound = MapObjectMove.MapObject_RecalculatePositionHeight(context.FieldSystem!, ref position);
            if (context.HeightFound == 1)
            {
                current.Y = position.Y;
                OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, current);
            }
        }
        switch (context.State)
        {
            case StateEnter:
                Billboards.Billboard_AdvanceAnim(context.Billboard, Fx32.One);
                if (Billboards.Billboard_GetAnimFrameNum(context.Billboard) / Fx32.One >= EnterFrames)
                {
                    context.State = StateSettle;
                }
                break;
            case StateSettle:
                Billboards.Billboard_SetFrameNum(context.Billboard, EnterFrames << Fx32.Shift);
                Billboards.Billboard_AdvanceAnim(context.Billboard, 0);
                context.State = StateIdle;
                goto case StateIdle;
            case StateIdle:
            {
                int x = MapObject.MapObject_GetX(mapObject);
                int z = MapObject.MapObject_GetZ(mapObject);
                if (context.X != x || context.Z != z)
                {
                    FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
                }
                break;
            }
        }
    }

    // OFFSET 0x021F3CCC
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        ShallowCoverEffectContext context = (ShallowCoverEffectContext)contextObject!;
        if (MapObject.MapObject_IsLocalObjectOfMap(context.MapObject!, context.LocalID, context.MapHeaderID) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        Billboards.Billboard_SetPos(context.Billboard, position);
    }
}
