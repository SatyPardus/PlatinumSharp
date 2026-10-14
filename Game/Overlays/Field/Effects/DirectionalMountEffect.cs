namespace PlatinumSharp.Game;

public sealed class DirectionalMountRenderer
{
    public const int Size = 0x7C;

    public int RefCount;
    public int Loaded;
    public FieldEffectManager? FieldEffectManager;
    public readonly Simple3DModel Model = new();
    public readonly G3dRenderObj RenderObj = new();
}

public sealed class DirectionalMountParams
{
    public int Direction;
    public FieldEffectManager? FieldEffectManager;
    public FieldSystem? FieldSystem;
    public DirectionalMountRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class DirectionalMountContext
{
    public const int Size = 0x24;

    public int Hidden;
    public int Direction;
    public int FrameCounter;
    public int SyncPosition;
    public int InitialDirection;
    public FieldEffectManager? FieldEffectManager;
    public FieldSystem? FieldSystem;
    public DirectionalMountRenderer? Renderer;
    public MapObject? MapObject;
}

public static class DirectionalMountEffect
{
    private const uint RendererID = 0x10;
    private const int ModelMember = 0x52;
    private const int TaskPriorityOffset = 2;
    private const int TileCenter = 0x8000;

    private static readonly OverworldAnimManagerFuncs sFuncs = new(DirectionalMountContext.Size, () => new DirectionalMountContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    // OFFSET 0x021F2850
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        DirectionalMountRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        return renderer;
    }

    // OFFSET 0x021F2864
    public static void Renderer_Free(object? context)
    {
        DirectionalMountRenderer renderer = (DirectionalMountRenderer)context!;
        Renderer_Unload(renderer);
    }

    // OFFSET 0x021F2874
    private static void Renderer_AddRef(DirectionalMountRenderer renderer)
    {
        renderer.RefCount++;
    }

    // OFFSET 0x021F287C
    private static void Renderer_Release(DirectionalMountRenderer renderer)
    {
        renderer.RefCount--;
        if (renderer.RefCount < 0)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021F288C
    private static void Renderer_Load(DirectionalMountRenderer renderer)
    {
        if (renderer.Loaded == 0)
        {
            FieldEffects.FieldEffectManager_LoadModel(renderer.FieldEffectManager!, renderer.Model, 0, ModelMember, 1);
            Simple3D.Simple3DRenderObj_Init(renderer.RenderObj, renderer.Model);
            renderer.Loaded = 1;
        }
    }

    // OFFSET 0x021F28C0
    private static void Renderer_Unload(DirectionalMountRenderer renderer)
    {
        if (renderer.Loaded == 1)
        {
            renderer.Loaded = 0;
            Simple3D.Simple3DModel_Free(renderer.Model);
        }
    }

    // OFFSET 0x021F28D4
    private static void Renderer_LoadIfUnused(DirectionalMountRenderer renderer)
    {
        if (renderer.RefCount == 0)
        {
            Renderer_Load(renderer);
        }
    }

    // OFFSET 0x021F28E4
    private static void Renderer_UnloadIfUnused(DirectionalMountRenderer renderer)
    {
        if (renderer.RefCount == 0)
        {
            Renderer_Unload(renderer);
        }
    }

    // OFFSET 0x021F28F4
    public static OverworldAnimManager? DirectionalMountEffect_New(MapObject mapObject, int x, int z, int direction, int syncPosition)
    {
        VecFx32 position = default;
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        DirectionalMountParams mountParams = new();
        mountParams.FieldEffectManager = fieldEffectManager;
        mountParams.Direction = direction;
        mountParams.FieldSystem = MapObject.MapObject_FieldSystem(mapObject);
        mountParams.Renderer = (DirectionalMountRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        mountParams.MapObject = mapObject;
        if (syncPosition == 0)
        {
            position.X = (x << 16) + TileCenter;
            position.Z = (z << 16) + TileCenter;
            MapObjectMove.MapObject_RecalculatePositionHeight(mountParams.FieldSystem!, ref position);
        }
        else
        {
            MapObject.MapObject_CopyPos(mapObject, out position);
        }
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        return FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFuncs, position, syncPosition, mountParams, priority);
    }

    // OFFSET 0x021F2974
    public static void DirectionalMountEffect_SetSyncPos(OverworldAnimManager animManager, int syncPosition)
    {
        DirectionalMountContext context = (DirectionalMountContext)OverworldAnimManagers.OverworldAnimManager_GetContext(animManager)!;
        context.SyncPosition = syncPosition;
    }

    // OFFSET 0x021F2980
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        DirectionalMountContext context = (DirectionalMountContext)contextObject!;
        DirectionalMountParams mountParams = (DirectionalMountParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.InitialDirection = mountParams.Direction;
        context.FieldEffectManager = mountParams.FieldEffectManager;
        context.FieldSystem = mountParams.FieldSystem;
        context.Renderer = mountParams.Renderer;
        context.MapObject = mountParams.MapObject;
        context.Direction = mountParams.Direction;
        context.SyncPosition = OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager);
        Renderer_LoadIfUnused(context.Renderer!);
        Renderer_AddRef(context.Renderer!);
        return 1;
    }

    // OFFSET 0x021F29BC
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        DirectionalMountContext context = (DirectionalMountContext)contextObject!;
        Renderer_Release(context.Renderer!);
        Renderer_UnloadIfUnused(context.Renderer!);
    }

    // OFFSET 0x021F29D0
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        DirectionalMountContext context = (DirectionalMountContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        if (context.SyncPosition != 0)
        {
            VecFx32 spriteOffset = default;
            MapObject.MapObject_SetSpritePosOffset(mapObject, spriteOffset);
            MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
            OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
        }
    }

    // OFFSET 0x021F2A04
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        DirectionalMountContext context = (DirectionalMountContext)contextObject!;
        if (context.Hidden == 1)
        {
            return;
        }
        context.FrameCounter++;
        VecFx32 scale = new() { X = Fx32.One, Y = Fx32.One, Z = Fx32.One };
        int angle = 0;
        G3dRenderObj renderObj = context.Renderer!.RenderObj;
        switch (context.Direction)
        {
            case 0:
                angle = 0xB4;
                break;
            case 1:
                break;
            case 2:
                angle = 0x10E;
                break;
            case 3:
                angle = 0x5A;
                break;
        }
        FxUtil.MTX_Rot33Angles(out MtxFx33 rotation, 0, (ushort)angle, 0);
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        Simple3D.Simple3DRenderObj_Draw(renderObj, position, scale, rotation);
    }
}
