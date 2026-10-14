using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class TileModelRenderer
{
    public const int Size = 0x1A4;
    public const int ModelCount = 4;

    public FieldEffectManager? FieldEffectManager;
    public readonly Simple3DModel[] Models = { new(), new(), new(), new() };
    public readonly G3dRenderObj[] RenderObjs = { new(), new(), new(), new() };
}

public sealed class TileModelEffectParams
{
    public int ModelIndex;
    public FieldEffectManager? FieldEffectManager;
    public TileModelRenderer? Renderer;
    public MapObject? MapObject;
    public G3dRenderObj? RenderObj;
}

public sealed class TileModelEffectContext
{
    public const int Size = 0x24;

    public int Unused00;
    public int Hidden;
    public int LocalID;
    public int MapHeaderID;
    public int ModelIndex;
    public FieldEffectManager? FieldEffectManager;
    public TileModelRenderer? Renderer;
    public MapObject? MapObject;
    public G3dRenderObj? RenderObj;
}

public static class TileModelEffect
{
    private const uint RendererID = 0x1A;
    private const int TaskPriorityOffset = 2;

    private static readonly int[] sModelMembers = { 0x67, 0x68, 0x69, 0x6A };

    private static readonly OverworldAnimManagerFuncs sFuncs = new(TileModelEffectContext.Size, () => new TileModelEffectContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    // OFFSET 0x021F3D00
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        TileModelRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        Renderer_LoadModels(renderer);
        return renderer;
    }

    // OFFSET 0x021F3D1C
    public static void Renderer_Free(object? context)
    {
        TileModelRenderer renderer = (TileModelRenderer)context!;
        Renderer_FreeModels(renderer);
    }

    // OFFSET 0x021F3D2C
    private static void Renderer_LoadModels(TileModelRenderer renderer)
    {
        for (int i = 0; i < TileModelRenderer.ModelCount; i++)
        {
            FieldEffects.FieldEffectManager_LoadModel(renderer.FieldEffectManager!, renderer.Models[i], 0, sModelMembers[i], 0);
            Simple3D.Simple3DRenderObj_Init(renderer.RenderObjs[i], renderer.Models[i]);
        }
    }

    // OFFSET 0x021F3D6C
    private static void Renderer_FreeModels(TileModelRenderer renderer)
    {
        for (int i = 0; i < TileModelRenderer.ModelCount; i++)
        {
            Simple3D.Simple3DModel_Free(renderer.Models[i]);
        }
    }

    // OFFSET 0x021F3D84
    private static G3dRenderObj Renderer_GetRenderObj(TileModelRenderer renderer, int modelIndex)
    {
        return renderer.RenderObjs[modelIndex];
    }

    // OFFSET 0x021F3D90
    public static OverworldAnimManager? TileModelEffect_New(MapObject mapObject, int modelIndex)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        FieldSystem fieldSystem = FieldEffects.FieldEffectManager_GetFieldSystem(fieldEffectManager)!;
        int x = MapObject.MapObject_GetX(mapObject);
        int z = MapObject.MapObject_GetZ(mapObject);
        VecFx32 position = default;
        MapObjectMove.VecFx32_SetPosFromMapCoords(x, z, ref position);
        MapObjectMove.MapObject_RecalculatePositionHeight(fieldSystem, ref position);
        TileModelEffectParams effectParams = new();
        effectParams.ModelIndex = modelIndex;
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.Renderer = (TileModelRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        effectParams.MapObject = mapObject;
        effectParams.RenderObj = Renderer_GetRenderObj(effectParams.Renderer!, modelIndex);
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        return FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFuncs, position, 0, effectParams, priority);
    }

    // OFFSET 0x021F3E00
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        TileModelEffectContext context = (TileModelEffectContext)contextObject!;
        TileModelEffectParams effectParams = (TileModelEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        MapObject mapObject = effectParams.MapObject!;
        context.ModelIndex = effectParams.ModelIndex;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.MapObject = effectParams.MapObject;
        context.RenderObj = effectParams.RenderObj;
        context.LocalID = MapObject.MapObject_GetLocalID(mapObject);
        context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(mapObject);
        return 1;
    }

    // OFFSET 0x021F3E30
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        TileModelEffectContext context = (TileModelEffectContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        if (MapObject.MapObject_IsLocalObjectOfMap(mapObject, context.LocalID, context.MapHeaderID) == 1)
        {
            sub_0206A218(mapObject, null);
        }
    }

    // OFFSET 0x021F3E50
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
    }

    // OFFSET 0x021F3E54
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        TileModelEffectContext context = (TileModelEffectContext)contextObject!;
        if (context.Hidden != 1)
        {
            OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
            Simple3D.Simple3DRenderObj_DrawAtPosition(context.RenderObj!, position);
        }
    }
}
