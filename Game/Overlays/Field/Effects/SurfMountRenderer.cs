namespace PlatinumSharp.Game;

public sealed class SurfMountRenderer
{
    public const int Size = 0x6C;

    public FieldEffectManager? FieldEffectManager;
    public readonly Simple3DModel Model = new();
    public readonly G3dRenderObj RenderObj = new();
}

public sealed class SurfMountParams
{
    public int Direction;
    public FieldEffectManager? FieldEffectManager;
    public SurfMountRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class SurfMountContext
{
    public const int Size = 0x34;

    public int Hidden;
    public int Direction;
    public int DrawDirection;
    public int LocalID;
    public int MapHeaderID;
    public int FrameCounter;
    public int SyncPosition;
    public int BobOffset;
    public int BobStep;
    public int InitialDirection;
    public FieldEffectManager? FieldEffectManager;
    public SurfMountRenderer? Renderer;
    public MapObject? MapObject;
}

public static class SurfMount
{
    private const uint RendererID = 0xF;
    private const int ModelMember = 0x51;
    private const int TaskPriorityOffset = 2;
    private const int TileCenter = 0x8000;
    private const int BobInitial = 0x1000;
    private const int BobStepInitial = 0x400;
    private const int BobMax = 0x4000;
    private const int BobMin = 0x1000;
    private const int RiderHeight = 0x7000;
    private const int RiderDepth = 0x4000;
    private const int DirectionNone = -1;

    private static readonly OverworldAnimManagerFuncs sFuncs = new(SurfMountContext.Size, () => new SurfMountContext(), SurfMount_Init, SurfMount_Exit, SurfMount_Tick, SurfMount_Render);

    // OFFSET 0x021F25C0
    public static object? SurfMountRenderer_New(FieldEffectManager fieldEffectManager)
    {
        SurfMountRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        SurfMountRenderer_LoadModel(renderer);
        return renderer;
    }

    // OFFSET 0x021F25DC
    public static void SurfMountRenderer_Free(object? context)
    {
        SurfMountRenderer renderer = (SurfMountRenderer)context!;
        SurfMountRenderer_FreeModel(renderer);
    }

    // OFFSET 0x021F25EC
    private static void SurfMountRenderer_LoadModel(SurfMountRenderer renderer)
    {
        FieldEffects.FieldEffectManager_LoadModel(renderer.FieldEffectManager!, renderer.Model, 0, ModelMember, 0);
        Simple3D.Simple3DRenderObj_Init(renderer.RenderObj, renderer.Model);
    }

    // OFFSET 0x021F2610
    private static void SurfMountRenderer_FreeModel(SurfMountRenderer renderer)
    {
        Simple3D.Simple3DModel_Free(renderer.Model);
    }

    // OFFSET 0x021F261C
    public static OverworldAnimManager? SurfMountRenderer_HandleSurfBegin(MapObject mapObject, int x, int z, int direction, int syncPosition)
    {
        VecFx32 position = default;
        SurfMountParams mountParams = new();
        mountParams.Direction = direction;
        mountParams.FieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject);
        mountParams.Renderer = (SurfMountRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(mountParams.FieldEffectManager!, RendererID);
        mountParams.MapObject = mapObject;
        if (syncPosition == 0)
        {
            FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
            position.X = (x << 16) + TileCenter;
            position.Z = (z << 16) + TileCenter;
            MapObjectMove.MapObject_RecalculatePositionHeight(fieldSystem, ref position);
        }
        else
        {
            VecFx32 spriteOffset = new() { X = 0, Y = RiderHeight, Z = RiderDepth };
            MapObject.MapObject_CopyPos(mapObject, out position);
            MapObject.MapObject_SetSpritePosOffset(mapObject, spriteOffset);
        }
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        return FieldEffects.FieldEffectManager_InitAnimManager(mountParams.FieldEffectManager!, sFuncs, position, syncPosition, mountParams, priority);
    }

    // OFFSET 0x021F26A8
    private static int SurfMount_Init(OverworldAnimManager animManager, object? contextObject)
    {
        SurfMountContext context = (SurfMountContext)contextObject!;
        SurfMountParams mountParams = (SurfMountParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        MapObject mapObject = mountParams.MapObject!;
        context.InitialDirection = mountParams.Direction;
        context.FieldEffectManager = mountParams.FieldEffectManager;
        context.Renderer = mountParams.Renderer;
        context.MapObject = mountParams.MapObject;
        context.LocalID = MapObject.MapObject_GetLocalID(mapObject);
        context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(mapObject);
        context.DrawDirection = mountParams.Direction;
        context.SyncPosition = OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager);
        context.BobOffset = BobInitial;
        context.BobStep = BobStepInitial;
        return 1;
    }

    // OFFSET 0x021F26F0
    private static void SurfMount_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        SurfMountContext context = (SurfMountContext)contextObject!;
        VecFx32 spriteOffset = default;
        MapObject.MapObject_SetSpritePosOffset(context.MapObject!, spriteOffset);
    }

    // OFFSET 0x021F2708
    private static void SurfMount_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        SurfMountContext context = (SurfMountContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        if (MapObject.MapObject_IsLocalObjectOfMap(mapObject, context.LocalID, context.MapHeaderID) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        context.Hidden = 0;
        context.Direction = MapObject.MapObject_GetFacingDir(mapObject);
        if (context.Direction == DirectionNone)
        {
            context.Hidden = 1;
            return;
        }
        if (context.SyncPosition != 0)
        {
            context.DrawDirection = context.Direction;
            context.BobOffset += context.BobStep;
            if (context.BobOffset >= BobMax)
            {
                context.BobOffset = BobMax;
                context.BobStep = -context.BobStep;
            }
            else if (context.BobOffset <= BobMin)
            {
                context.BobOffset = BobMin;
                context.BobStep = -context.BobStep;
            }
            VecFx32 spriteOffset;
            spriteOffset.X = 0;
            spriteOffset.Y = context.BobOffset + RiderHeight;
            spriteOffset.Z = RiderDepth;
            MapObject.MapObject_SetSpritePosOffset(mapObject, spriteOffset);
            MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
            position.Y += context.BobOffset - BobInitial;
            OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
        }
    }

    // OFFSET 0x021F27B4
    private static void SurfMount_Render(OverworldAnimManager animManager, object? contextObject)
    {
        SurfMountContext context = (SurfMountContext)contextObject!;
        if (context.Hidden == 1 || context.DrawDirection == DirectionNone)
        {
            return;
        }
        context.FrameCounter++;
        VecFx32 scale = new() { X = Fx32.One, Y = Fx32.One, Z = Fx32.One };
        int angle = 0;
        G3dRenderObj renderObj = context.Renderer!.RenderObj;
        switch (context.DrawDirection)
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

    // OFFSET 0x021F2838
    public static void SurfMountRenderer_SetSyncPos(OverworldAnimManager animManager, int syncPosition)
    {
        SurfMountContext context = (SurfMountContext)OverworldAnimManagers.OverworldAnimManager_GetContext(animManager)!;
        context.SyncPosition = syncPosition;
        context.BobOffset = BobInitial;
        context.BobStep = BobStepInitial;
    }
}
