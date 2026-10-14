namespace PlatinumSharp.Game;

public sealed class LongGrassRenderer
{
    public const int Size = 0x4;

    public FieldEffectManager? FieldEffectManager;
}

public sealed class LongGrassEffectParams
{
    public int X;
    public int Y;
    public int Z;
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public LongGrassRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class LongGrassEffectContext
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
    public LongGrassRenderer? Renderer;
    public MapObject? MapObject;
    public Billboard? Billboard;
}

public static class LongGrassEffect
{
    private const uint RendererID = 0x18;
    private const int TaskPriorityOffset = 2;
    private const uint ModelID = 7;
    private const uint ModelMember = 0x59;
    private const uint AnimID = 7;
    private const uint AnimMember = 0xB0;
    private const uint TextureID = 8;
    private const uint TextureMember = 8;
    private const uint ResourceID = 9;
    private const int EnterFrames = 0xC;
    private const int StateEnter = 0;
    private const int StateSettle = 1;
    private const int StateIdle = 2;

    private static readonly BillboardAnim[] sAnims =
    {
        new() { StartFrame = 0, EndFrame = 0xC, StopAtEnd = 0 },
        new() { StartFrame = 0, EndFrame = 0, StopAtEnd = 2 },
    };

    private static readonly OverworldAnimManagerFuncs sFuncs = new(LongGrassEffectContext.Size, () => new LongGrassEffectContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    // OFFSET 0x021F37A8
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        LongGrassRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        Renderer_Load(renderer);
        return renderer;
    }

    // OFFSET 0x021F37C4
    public static void Renderer_Free(object? context)
    {
        LongGrassRenderer renderer = (LongGrassRenderer)context!;
        Renderer_Unload(renderer);
    }

    // OFFSET 0x021F37D4
    private static void Renderer_Load(LongGrassRenderer renderer)
    {
        FieldEffects.FieldEffectManager_LoadBillboardModel(renderer.FieldEffectManager!, ModelID, ModelMember);
        FieldEffects.FieldEffectManager_LoadBillboardAnim(renderer.FieldEffectManager!, AnimID, AnimMember);
        FieldEffects.FieldEffectManager_LoadBillboardTexture(renderer.FieldEffectManager!, TextureID, TextureMember, 1);
        FieldEffects.FieldEffectManager_InitBillboardResources(renderer.FieldEffectManager!, ResourceID, ModelID, AnimID, TextureID, 0, sAnims);
    }

    // OFFSET 0x021F381C
    private static void Renderer_Unload(LongGrassRenderer renderer)
    {
        FieldEffects.FieldEffectManager_FreeBillboardModel(renderer.FieldEffectManager!, ModelID);
        FieldEffects.FieldEffectManager_FreeBillboardAnim(renderer.FieldEffectManager!, AnimID);
        FieldEffects.FieldEffectManager_FreeBillboardTexture(renderer.FieldEffectManager!, TextureID);
        FieldEffects.FieldEffectManager_ReleaseBillboardResources(renderer.FieldEffectManager!, ResourceID);
    }

    // OFFSET 0x021F3844
    public static void LongGrassEffect_New(MapObject mapObject, int skipEnter)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        LongGrassEffectParams effectParams = new();
        effectParams.X = MapObject.MapObject_GetX(mapObject);
        effectParams.Y = MapObject.MapObject_GetY(mapObject);
        effectParams.Z = MapObject.MapObject_GetZ(mapObject);
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.FieldSystem = FieldEffects.FieldEffectManager_GetFieldSystem(fieldEffectManager);
        effectParams.Renderer = (LongGrassRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        effectParams.MapObject = mapObject;
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFuncs, position, skipEnter, effectParams, priority);
    }

    // OFFSET 0x021F38AC
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        LongGrassEffectContext context = (LongGrassEffectContext)contextObject!;
        LongGrassEffectParams effectParams = (LongGrassEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
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
        position.X += 0x8000;
        position.Z += 0x12000;
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
        context.Billboard = FieldEffects.FieldEffectManager_AddBillboardWithResourceID(context.FieldEffectManager!, ResourceID, position);
        if (OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager) == 0)
        {
            context.State = StateSettle;
        }
        return 1;
    }

    // OFFSET 0x021F3940
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        LongGrassEffectContext context = (LongGrassEffectContext)contextObject!;
        Billboards.Billboard_Delete(context.Billboard);
    }

    // OFFSET 0x021F394C
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        LongGrassEffectContext context = (LongGrassEffectContext)contextObject!;
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

    // OFFSET 0x021F3A18
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        LongGrassEffectContext context = (LongGrassEffectContext)contextObject!;
        if (MapObject.MapObject_IsLocalObjectOfMapWithGraphics(context.MapObject!, context.GraphicsID, context.LocalID, context.MapHeaderID) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        Billboards.Billboard_SetPos(context.Billboard, position);
    }
}
