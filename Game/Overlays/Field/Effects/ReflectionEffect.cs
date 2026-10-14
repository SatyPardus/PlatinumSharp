namespace PlatinumSharp.Game;

public sealed class ReflectionRenderer
{
    public const int Size = 0x4;

    public FieldEffectManager? FieldEffectManager;
}

public sealed class ReflectionEffectParams
{
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public ReflectionRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class ReflectionEffectContext
{
    public const int Size = 0x38;

    public int LocalID;
    public int MapHeaderID;
    public int GraphicsID;
    public int BillboardAdded;
    public int ReflectionType;
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public ReflectionRenderer? Renderer;
    public MapObject? MapObject;
    public Billboard? Billboard;
    public VecFx32 Scale;
    public int ScaleStep;
}

public sealed class BillboardReflectionEffectParams
{
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public ReflectionRenderer? Renderer;
    public readonly BillboardResources Resources = new();
    public Billboard? SourceBillboard;
}

public sealed class BillboardReflectionEffectContext
{
    public const int Size = 0x38;

    public int ReflectionType;
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public ReflectionRenderer? Renderer;
    public readonly BillboardResources Resources = new();
    public Billboard? SourceBillboard;
    public Billboard? Billboard;
    public VecFx32 Scale;
    public int ScaleStep;
    public VecFx32 BasePosition;
}

public static class ReflectionEffect
{
    private const uint RendererID = 1;
    private const uint BillboardModelID = 2;
    private const uint BillboardModelMember = 0x14;
    private const int TaskPriorityOffset = 2;
    private const int ReflectionTypeStill = 2;
    private const int ScaleStepInitial = 0x40;
    private const int ScaleMax = 0x1200;
    private const int ScaleMin = 0xE00;
    private const int DepthOffset = 0x7000;

    private static readonly int[] sHeightOffsets = { 0xC000, 0x10000, 0xC000 };

    private static readonly int[] sBillboardHeightOffsets = { 0xC000, 0x10000, 0xC000 };

    private static readonly OverworldAnimManagerFuncs sFuncs = new(ReflectionEffectContext.Size, () => new ReflectionEffectContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    private static readonly OverworldAnimManagerFuncs sBillboardFuncs = new(BillboardReflectionEffectContext.Size, () => new BillboardReflectionEffectContext(), BillboardEffect_Init, BillboardEffect_Exit, BillboardEffect_Tick, BillboardEffect_Render);

    // OFFSET 0x021F17B8
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        ReflectionRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        Renderer_LoadModel(renderer);
        return renderer;
    }

    // OFFSET 0x021F17D4
    public static void Renderer_Free(object? context)
    {
        ReflectionRenderer renderer = (ReflectionRenderer)context!;
        Renderer_FreeModel(renderer);
    }

    // OFFSET 0x021F17E4
    private static void Renderer_LoadModel(ReflectionRenderer renderer)
    {
        FieldEffects.FieldEffectManager_LoadBillboardModel(renderer.FieldEffectManager!, BillboardModelID, BillboardModelMember);
    }

    // OFFSET 0x021F17F4
    private static void Renderer_FreeModel(ReflectionRenderer renderer)
    {
        FieldEffects.FieldEffectManager_FreeBillboardModel(renderer.FieldEffectManager!, BillboardModelID);
    }

    // OFFSET 0x021F1800
    public static OverworldAnimManager? ReflectionEffect_New(MapObject mapObject, int reflectionType)
    {
        ReflectionEffectParams effectParams = new();
        effectParams.FieldSystem = MapObject.MapObject_FieldSystem(mapObject);
        effectParams.FieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject);
        effectParams.Renderer = (ReflectionRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(effectParams.FieldEffectManager!, RendererID);
        effectParams.MapObject = mapObject;
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        return FieldEffects.FieldEffectManager_InitAnimManager(effectParams.FieldEffectManager!, sFuncs, position, reflectionType, effectParams, priority);
    }

    // OFFSET 0x021F184C
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        ReflectionEffectContext context = (ReflectionEffectContext)contextObject!;
        ReflectionEffectParams effectParams = (ReflectionEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.FieldSystem = effectParams.FieldSystem;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.MapObject = effectParams.MapObject;
        context.ReflectionType = OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager);
        context.LocalID = MapObject.MapObject_GetLocalID(context.MapObject!);
        context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(context.MapObject!);
        context.GraphicsID = MapObject.MapObject_GetGraphicsID(context.MapObject!);
        if (BerryPatchGraphics.BerryPatchGraphics_IsBerryPatch(context.GraphicsID) == 1)
        {
            context.GraphicsID = BerryPatchGraphics.BerryPatchGraphics_GetCurrentGraphicsResourceID(context.MapObject!);
        }
        context.Scale.X = Fx32.One;
        context.Scale.Y = Fx32.One;
        context.Scale.Z = Fx32.One;
        context.ScaleStep = ScaleStepInitial;
        if (context.ReflectionType == ReflectionTypeStill)
        {
            context.ScaleStep = 0;
        }
        Effect_CalculatePosition(context, context.MapObject!, out VecFx32 position);
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
        Effect_TryAddBillboard(animManager, context);
        return 1;
    }

    // OFFSET 0x021F18D0
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        ReflectionEffectContext context = (ReflectionEffectContext)contextObject!;
        if (context.BillboardAdded == 1)
        {
            Billboards.Billboard_Delete(context.Billboard);
        }
    }

    // OFFSET 0x021F18E0
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        ReflectionEffectContext context = (ReflectionEffectContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        int graphicsID = MapObject.MapObject_GetGraphicsID(mapObject);
        if (BerryPatchGraphics.BerryPatchGraphics_IsBerryPatch(graphicsID) == 1)
        {
            graphicsID = BerryPatchGraphics.BerryPatchGraphics_GetCurrentGraphicsResourceID(context.MapObject!);
        }
        if (context.GraphicsID != graphicsID
            || MapObject.MapObject_IsLocalObjectOfMap(mapObject, context.LocalID, context.MapHeaderID) == 0
            || MapObject.MapObject_IsStatus24On(mapObject) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        context.Scale.X += context.ScaleStep;
        if (context.Scale.X >= ScaleMax)
        {
            context.Scale.X = ScaleMax;
            context.ScaleStep = -context.ScaleStep;
        }
        else if (context.Scale.X <= ScaleMin)
        {
            context.Scale.X = ScaleMin;
            context.ScaleStep = -context.ScaleStep;
        }
        Effect_CalculatePosition(context, mapObject, out VecFx32 position);
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
        if (context.BillboardAdded == 0)
        {
            Effect_TryAddBillboard(animManager, context);
        }
    }

    // OFFSET 0x021F1978
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        ReflectionEffectContext context = (ReflectionEffectContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        int graphicsID = MapObject.MapObject_GetGraphicsID(mapObject);
        if (BerryPatchGraphics.BerryPatchGraphics_IsBerryPatch(graphicsID) == 1)
        {
            graphicsID = BerryPatchGraphics.BerryPatchGraphics_GetCurrentGraphicsResourceID(context.MapObject!);
        }
        if (context.GraphicsID != graphicsID
            || MapObject.MapObject_IsLocalObjectOfMap(mapObject, context.LocalID, context.MapHeaderID) == 0
            || MapObject.MapObject_IsStatus24On(mapObject) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        if (context.BillboardAdded != 0)
        {
            if (MapObject.MapObject_IsHidden(mapObject) == 1)
            {
                Billboards.Billboard_SetDrawFlag(context.Billboard, 0);
            }
            else
            {
                Billboards.Billboard_SetDrawFlag(context.Billboard, 1);
            }
            OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
            Billboards.Billboard_SetPos(context.Billboard, position);
            Billboards.Billboard_SetScale(context.Billboard, context.Scale);
            Billboard? source = MapObjectDrawHandlers.MapObject_GetBillboard(context.MapObject!);
            Billboards.Billboard_SetAnimNum(context.Billboard, Billboards.Billboard_GetAnimNum(source));
            Billboards.Billboard_SetFrameNum(context.Billboard, Billboards.Billboard_GetFrameNum(source));
        }
    }

    // OFFSET 0x021F1A24
    private static void Effect_CalculatePosition(ReflectionEffectContext context, MapObject mapObject, out VecFx32 position)
    {
        MapObject.MapObject_CopySpriteJumpOffset(mapObject, out position);
        int depth = -position.Z;
        int jumpX = position.X;
        MapObject.MapObject_CopyPos(mapObject, out position);
        int heightFound = MapObjectMove.MapObject_RecalculatePositionHeight(context.FieldSystem!, ref position);
        position.X += jumpX;
        position.Z += depth - DepthOffset;
        if (heightFound == 0)
        {
            position.Y = 0;
            return;
        }
        position.Y -= sHeightOffsets[context.ReflectionType];
    }

    // OFFSET 0x021F1A8C
    private static void Effect_TryAddBillboard(OverworldAnimManager animManager, ReflectionEffectContext context)
    {
        BillboardResources resources = new();
        if (MapObjectRenderResources.MapObjectMan_CopyResources(MapObject.MapObject_MapObjectManager(context.MapObject!)!, (uint)context.GraphicsID, resources) != 0
            && MapObjectRenderResources.MapObject_IsBillboardPending(context.MapObject!) != 1)
        {
            resources.ModelRes = FieldEffects.FieldEffectManager_GetBillboardModel(context.FieldEffectManager!, BillboardModelID);
            OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
            context.Billboard = FieldEffects.FieldEffectManager_AddBillboard(context.FieldEffectManager!, resources, position);
            context.BillboardAdded = 1;
        }
    }

    // OFFSET 0x021F1AD8
    private static void BillboardEffect_AddBillboard(OverworldAnimManager animManager, BillboardReflectionEffectContext context)
    {
        BillboardResources resources = new();
        resources.CopyFrom(context.Resources);
        resources.ModelRes = FieldEffects.FieldEffectManager_GetBillboardModel(context.FieldEffectManager!, BillboardModelID);
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        context.Billboard = FieldEffects.FieldEffectManager_AddBillboard(context.FieldEffectManager!, resources, position);
        Billboard? source = context.SourceBillboard;
        Billboards.Billboard_SetAnimNum(context.Billboard, Billboards.Billboard_GetAnimNum(source));
        Billboards.Billboard_SetAnimFrameNum(context.Billboard, Billboards.Billboard_GetAnimFrameNum(source));
        Billboards.Billboard_SetFrameNum(context.Billboard, Billboards.Billboard_GetFrameNum(source));
        Billboards.Billboard_AdvanceAnim(context.Billboard, 0);
        Billboards.Billboard_TryRequestVRAMTransfer(context.Billboard!);
    }

    // OFFSET 0x021F1B4C
    private static void BillboardEffect_CalculatePosition(BillboardReflectionEffectContext context, out VecFx32 position)
    {
        position = context.BasePosition;
        int heightFound = MapObjectMove.MapObject_RecalculatePositionHeight(context.FieldSystem!, ref position);
        position.Z -= DepthOffset;
        if (heightFound == 0)
        {
            position.Y = 0;
            return;
        }
        position.Y -= sBillboardHeightOffsets[context.ReflectionType];
    }

    // OFFSET 0x021F1BA4
    public static OverworldAnimManager? ReflectionEffect_NewForBillboard(FieldEffectManager fieldEffectManager, BillboardResources resources, Billboard? billboard, in VecFx32 position, int reflectionType, int priority)
    {
        BillboardReflectionEffectParams effectParams = new();
        effectParams.FieldSystem = FieldEffects.FieldEffectManager_GetFieldSystem(fieldEffectManager);
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.Renderer = (ReflectionRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        effectParams.Resources.CopyFrom(resources);
        effectParams.SourceBillboard = billboard;
        return FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sBillboardFuncs, position, reflectionType, effectParams, priority);
    }

    // OFFSET 0x021F1BEC
    private static int BillboardEffect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        BillboardReflectionEffectContext context = (BillboardReflectionEffectContext)contextObject!;
        BillboardReflectionEffectParams effectParams = (BillboardReflectionEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.FieldSystem = effectParams.FieldSystem;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.Resources.CopyFrom(effectParams.Resources);
        context.SourceBillboard = effectParams.SourceBillboard;
        context.ReflectionType = OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager);
        context.Scale.X = Fx32.One;
        context.Scale.Y = Fx32.One;
        context.Scale.Z = Fx32.One;
        context.ScaleStep = ScaleStepInitial;
        if (context.ReflectionType == ReflectionTypeStill)
        {
            context.ScaleStep = 0;
        }
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out context.BasePosition);
        BillboardEffect_CalculatePosition(context, out VecFx32 position);
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
        BillboardEffect_AddBillboard(animManager, context);
        OverworldAnimManagers.OverworldAnimManager_CallTick(animManager);
        return 1;
    }

    // OFFSET 0x021F1C54
    private static void BillboardEffect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        BillboardReflectionEffectContext context = (BillboardReflectionEffectContext)contextObject!;
        Billboards.Billboard_Delete(context.Billboard);
    }

    // OFFSET 0x021F1C60
    private static void BillboardEffect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        BillboardReflectionEffectContext context = (BillboardReflectionEffectContext)contextObject!;
        context.Scale.X += context.ScaleStep;
        if (context.Scale.X >= ScaleMax)
        {
            context.Scale.X = ScaleMax;
            context.ScaleStep = -context.ScaleStep;
        }
        else if (context.Scale.X <= ScaleMin)
        {
            context.Scale.X = ScaleMin;
            context.ScaleStep = -context.ScaleStep;
        }
        BillboardEffect_CalculatePosition(context, out VecFx32 position);
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
    }

    // OFFSET 0x021F1CA4
    private static void BillboardEffect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        BillboardReflectionEffectContext context = (BillboardReflectionEffectContext)contextObject!;
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        Billboards.Billboard_SetPos(context.Billboard, position);
        Billboards.Billboard_SetScale(context.Billboard, context.Scale);
    }
}
