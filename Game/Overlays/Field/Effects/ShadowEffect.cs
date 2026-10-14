namespace PlatinumSharp.Game;

public sealed class ShadowRenderer
{
    public const int Size = 0x160;
    public const int ModelCount = 3;

    public int State;
    public int TimeOfDay;
    public int Alpha;
    public VecFx32 Scale;
    public FieldEffectManager? FieldEffectManager;
    public SysTask? Task;
    public readonly Simple3DModel[] Models = { new(), new(), new() };
    public readonly G3dRenderObj[] RenderObjs = { new(), new(), new() };
}

public sealed class ShadowEffectParams
{
    public FieldEffectManager? FieldEffectManager;
    public ShadowRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class ShadowEffectContext
{
    public const int Size = 0x20;

    public int GraphicsID;
    public int LocalID;
    public int MapHeaderID;
    public int Hidden;
    public int ModelIndex;
    public FieldEffectManager? FieldEffectManager;
    public ShadowRenderer? Renderer;
    public MapObject? MapObject;
}

public static class ShadowEffect
{
    private const uint RendererID = 0;
    private const int TaskPriorityOffset = 2;
    private const int StateInit = 0;
    private const int StateWaitTimeChange = 1;
    private const int StateTransition = 2;
    private const int ScaleStep = 0x10;
    private const int AlphaStep = 0x200;
    private const uint PolygonAlphaMask = 0x1F0000;
    private const int MaterialPolygonAttr = 0xC;
    private const uint ManagerStatusNoShadows = 8;
    private const uint StatusHiddenMask = 0x100200;
    private const int OffsetX = 0x800;
    private const int OffsetY = 0x4000;
    private const int OffsetZ = 0x1000;

    private static readonly int[] sModelMembers = { 0x11, 0x12, 0x13 };

    private static readonly int[] sAlphaByTimeOfDay = { 0xE000, 0x12000, 0x12000, 0x8000, 0x4000 };

    private static readonly VecFx32[] sScaleByTimeOfDay =
    {
        new() { X = 0x1000, Y = 0x1000, Z = 0x1000 },
        new() { X = 0x1400, Y = 0x1000, Z = 0x1400 },
        new() { X = 0x1400, Y = 0x1000, Z = 0x1000 },
        new() { X = 0x1200, Y = 0x1000, Z = 0x1000 },
        new() { X = 0xE00, Y = 0x1000, Z = 0xE00 },
    };

    private static readonly MtxFx33 sIdentity = new() { M00 = 0x1000, M11 = 0x1000, M22 = 0x1000 };

    private static readonly OverworldAnimManagerFuncs sFuncs = new(ShadowEffectContext.Size, () => new ShadowEffectContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    private static readonly OverworldAnimManagerFuncs sFuncsWithModel = new(ShadowEffectContext.Size, () => new ShadowEffectContext(), Effect_Init, Effect_Exit, EffectWithModel_Tick, EffectWithModel_Render);

    // OFFSET 0x021F134C
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        ShadowRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        Renderer_LoadModels(renderer);
        Renderer_StartTask(renderer);
        return renderer;
    }

    // OFFSET 0x021F1370
    public static void Renderer_Free(object? context)
    {
        ShadowRenderer renderer = (ShadowRenderer)context!;
        Renderer_StopTask(renderer);
        Renderer_FreeModels(renderer);
    }

    // OFFSET 0x021F1388
    private static void Renderer_StartTask(ShadowRenderer renderer)
    {
        FieldSystem fieldSystem = FieldEffects.FieldEffectManager_GetFieldSystem(renderer.FieldEffectManager!)!;
        int priority = MapObjectManager.MapObjectMan_GetTaskBasePriority(fieldSystem.MapObjectManager!) - 1;
        renderer.Task = SysTask.SysTask_Start(Renderer_Task, renderer, (uint)priority);
    }

    // OFFSET 0x021F13AC
    private static void Renderer_StopTask(ShadowRenderer renderer)
    {
        SysTask.SysTask_Done(renderer.Task!);
    }

    // OFFSET 0x021F13B8
    private static void Renderer_GetScale(ShadowRenderer renderer, out VecFx32 scale)
    {
        scale = renderer.Scale;
    }

    // OFFSET 0x021F13C8
    private static int FxToInt(int value)
    {
        return value / Fx32.One;
    }

    // OFFSET 0x021F13D4
    private static void Renderer_ApplyTimeOfDay(ShadowRenderer renderer)
    {
        renderer.Alpha = sAlphaByTimeOfDay[renderer.TimeOfDay];
        renderer.Scale = sScaleByTimeOfDay[renderer.TimeOfDay];
    }

    // OFFSET 0x021F1400
    private static void Approach(ref int value, int target, int step)
    {
        if (value < target)
        {
            value += step;
            if (value > target)
            {
                value = target;
            }
        }
        else if (value > target)
        {
            value -= step;
            if (value < target)
            {
                value = target;
            }
        }
    }

    // OFFSET 0x021F1424
    private static void Renderer_Task(SysTask task, object? param)
    {
        ShadowRenderer renderer = (ShadowRenderer)param!;
        int timeOfDay = Rtc.GetTimeOfDay();
        switch (renderer.State)
        {
            case StateInit:
                renderer.TimeOfDay = timeOfDay;
                Renderer_ApplyTimeOfDay(renderer);
                Renderer_SetAlpha(renderer, FxToInt(renderer.Alpha));
                renderer.State++;
                break;
            case StateWaitTimeChange:
                if (renderer.TimeOfDay == timeOfDay)
                {
                    break;
                }
                renderer.State++;
                goto case StateTransition;
            case StateTransition:
            {
                int targetAlpha = sAlphaByTimeOfDay[timeOfDay];
                VecFx32 targetScale = sScaleByTimeOfDay[timeOfDay];
                Approach(ref renderer.Scale.X, targetScale.X, ScaleStep);
                Approach(ref renderer.Scale.Y, targetScale.Y, ScaleStep);
                Approach(ref renderer.Scale.Z, targetScale.Z, ScaleStep);
                Approach(ref renderer.Alpha, targetAlpha, AlphaStep);
                Renderer_SetAlpha(renderer, FxToInt(renderer.Alpha));
                if (targetScale.X == renderer.Scale.X && targetScale.Y == renderer.Scale.Y && targetScale.Z == renderer.Scale.Z && targetAlpha == renderer.Alpha)
                {
                    renderer.TimeOfDay = timeOfDay;
                    renderer.State = StateWaitTimeChange;
                }
                break;
            }
        }
    }

    // OFFSET 0x021F14FC
    private static void Renderer_LoadModels(ShadowRenderer renderer)
    {
        for (int i = 0; i < ShadowRenderer.ModelCount; i++)
        {
            FieldEffects.FieldEffectManager_LoadModel(renderer.FieldEffectManager!, renderer.Models[i], 0, sModelMembers[i], 0);
            Simple3D.Simple3DRenderObj_Init(renderer.RenderObjs[i], renderer.Models[i]);
        }
    }

    // OFFSET 0x021F153C
    private static void Renderer_FreeModels(ShadowRenderer renderer)
    {
        for (int i = 0; i < ShadowRenderer.ModelCount; i++)
        {
            Simple3D.Simple3DModel_Free(renderer.Models[i]);
        }
    }

    // OFFSET 0x021F1554
    private static void Renderer_SetAlpha(ShadowRenderer renderer, int alpha)
    {
        G3d.SetMaterialPolygonAttrMask(renderer.Models[0].Model!, true, PolygonAlphaMask);
        G3d.SetMaterialParam(renderer.Models[0].Model!, MaterialPolygonAttr, PolygonAlphaMask, (uint)alpha << 16);
    }

    // OFFSET 0x021F1570
    public static OverworldAnimManager? ShadowEffect_New(MapObject mapObject)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        ShadowEffectParams effectParams = new();
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.Renderer = (ShadowRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        effectParams.MapObject = mapObject;
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        return FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFuncs, position, 0, effectParams, priority);
    }

    // OFFSET 0x021F15B4
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        ShadowEffectContext context = (ShadowEffectContext)contextObject!;
        ShadowEffectParams effectParams = (ShadowEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.MapObject = effectParams.MapObject;
        context.ModelIndex = OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager);
        context.GraphicsID = MapObject.MapObject_GetGraphicsID(context.MapObject!);
        context.LocalID = MapObject.MapObject_GetLocalID(context.MapObject!);
        if (MapObject.MapObject_IsAlias(context.MapObject!) == 1)
        {
            context.MapHeaderID = MapObject.MapObject_GetAliasMapHeaderID(context.MapObject!);
        }
        else
        {
            context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(context.MapObject!);
        }
        return 1;
    }

    // OFFSET 0x021F1600
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
    }

    // OFFSET 0x021F1604
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        ShadowEffectContext context = (ShadowEffectContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        if (MapObject.MapObject_IsLocalObjectOfMapWithGraphics(mapObject, context.GraphicsID, context.LocalID, context.MapHeaderID) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        if (MapObject.MapObject_CheckManagerStatus(mapObject, ManagerStatusNoShadows) != 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        context.Hidden = 0;
        if (MapObject.MapObject_CheckStatusFlag(mapObject, StatusHiddenMask))
        {
            context.Hidden = 1;
            return;
        }
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
    }

    // OFFSET 0x021F1670
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        ShadowEffectContext context = (ShadowEffectContext)contextObject!;
        if (context.Hidden == 0)
        {
            MtxFx33 rotation = sIdentity;
            ShadowRenderer renderer = context.Renderer!;
            Renderer_GetScale(renderer, out VecFx32 scale);
            OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
            position.X -= OffsetX;
            position.Y -= OffsetY;
            position.Z += OffsetZ;
            Simple3D.Simple3DRenderObj_Draw(renderer.RenderObjs[0], position, scale, rotation);
        }
    }

    // OFFSET 0x021F16D4
    public static OverworldAnimManager? ShadowEffect_NewWithModel(MapObject mapObject, int modelIndex)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        ShadowEffectParams effectParams = new();
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.Renderer = (ShadowRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        effectParams.MapObject = mapObject;
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        return FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFuncsWithModel, position, modelIndex, effectParams, priority);
    }

    // OFFSET 0x021F1718
    private static void EffectWithModel_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        ShadowEffectContext context = (ShadowEffectContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        if (MapObject.MapObject_IsLocalObjectOfMapWithGraphics(mapObject, context.GraphicsID, context.LocalID, context.MapHeaderID) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        context.Hidden = 0;
        if (MapObject.MapObject_CheckStatusFlag(mapObject, StatusHiddenMask))
        {
            context.Hidden = 1;
            return;
        }
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
    }

    // OFFSET 0x021F176C
    private static void EffectWithModel_Render(OverworldAnimManager animManager, object? contextObject)
    {
        ShadowEffectContext context = (ShadowEffectContext)contextObject!;
        if (context.Hidden != 1)
        {
            int modelIndex = OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager);
            ShadowRenderer renderer = context.Renderer!;
            OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
            position.X -= OffsetX;
            position.Y -= OffsetY;
            position.Z += OffsetZ;
            Simple3D.Simple3DRenderObj_DrawAtPosition(renderer.RenderObjs[modelIndex], position);
        }
    }
}
