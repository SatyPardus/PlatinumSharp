namespace PlatinumSharp.Game;

public sealed class MapObjectModelSlot
{
    public const int Size = 0x6C;

    public uint ID;
    public readonly Simple3DModel Model = new();
    public readonly G3dRenderObj RenderObj = new();
}

public sealed class MapObjectModelRenderer
{
    public const int Size = 0xC;

    public FieldEffectManager? FieldEffectManager;
    public uint SlotCount;
    public MapObjectModelSlot[]? Slots;
}

public sealed class MapObjectModelEffectParams
{
    public int GraphicsID;
    public FieldEffectManager? FieldEffectManager;
    public MapObject? MapObject;
}

public sealed class MapObjectModelEffectContext
{
    public const int Size = 0x28;

    public int LocalID;
    public int MapHeaderID;
    public VecFx32 Offset;
    public G3dRenderObj? RenderObj;
    public int GraphicsID;
    public FieldEffectManager? FieldEffectManager;
    public MapObject? MapObject;
}

public static class MapObjectModelEffect
{
    private const uint RendererID = 0x1D;
    private const uint SlotFree = 0xFFFF;
    private const int TaskPriorityOffset = 2;
    private const int DefaultModelCount = 9;

    private static readonly uint[] sDefaultModels =
    {
        0x5B, 0x45,
        0x5C, 0x46,
        0x5D, 0x47,
        0x5E, 0x48,
        0x5F, 0x49,
        0x60, 0x4A,
        0xB7, 0x4F,
        0xD1, 0x6E,
        0x106, 0x95,
    };

    private static readonly OverworldAnimManagerFuncs sFuncs = new(MapObjectModelEffectContext.Size, () => new MapObjectModelEffectContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    // OFFSET 0x021F10E8
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        MapObjectModelRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        return renderer;
    }

    // OFFSET 0x021F10FC
    public static void Renderer_Free(object? context)
    {
        MapObjectModelRenderer renderer = (MapObjectModelRenderer)context!;
        Renderer_FreeSlots(renderer);
        if (renderer.Slots != null)
        {
            renderer.Slots = null;
        }
    }

    // OFFSET 0x021F1118
    private static void Slot_Load(MapObjectModelSlot slot, uint id, uint memberIndex, FieldEffectManager fieldEffectManager)
    {
        slot.ID = id;
        FieldEffects.FieldEffectManager_LoadModel(fieldEffectManager, slot.Model, 0, (int)memberIndex, 0);
        Simple3D.Simple3DRenderObj_Init(slot.RenderObj, slot.Model);
    }

    // OFFSET 0x021F113C
    private static void Slot_Free(MapObjectModelSlot slot)
    {
        slot.ID = SlotFree;
        Simple3D.Simple3DModel_Free(slot.Model);
    }

    // OFFSET 0x021F114C
    private static void Renderer_FreeSlots(MapObjectModelRenderer renderer)
    {
        for (uint i = 0; i < renderer.SlotCount; i++)
        {
            MapObjectModelSlot slot = renderer.Slots![i];
            if (slot.ID != SlotFree)
            {
                Slot_Free(slot);
            }
        }
    }

    // OFFSET 0x021F1174
    private static MapObjectModelSlot? Renderer_FindSlot(MapObjectModelRenderer renderer, uint id)
    {
        uint remaining = renderer.SlotCount;
        int i = 0;
        do
        {
            MapObjectModelSlot slot = renderer.Slots![i];
            if (slot.ID == id)
            {
                return slot;
            }
            i++;
        }
        while (--remaining != 0);
        return null;
    }

    // OFFSET 0x021F1188
    public static void FieldEffectManager_AllocMapObjectModelSlots(FieldEffectManager fieldEffectManager, uint slotCount)
    {
        MapObjectModelRenderer renderer = (MapObjectModelRenderer)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID)!;
        if (slotCount == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        renderer.SlotCount = slotCount;
        renderer.Slots = new MapObjectModelSlot[slotCount];
        for (uint i = 0; i < slotCount; i++)
        {
            renderer.Slots[i] = new MapObjectModelSlot();
            renderer.Slots[i].ID = SlotFree;
        }
    }

    // OFFSET 0x021F11C0
    public static void FieldEffectManager_LoadMapObjectModel(FieldEffectManager fieldEffectManager, uint id, uint memberIndex)
    {
        MapObjectModelRenderer renderer = (MapObjectModelRenderer)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID)!;
        if (Renderer_FindSlot(renderer, id) != null)
        {
            return;
        }
        MapObjectModelSlot? slot = Renderer_FindSlot(renderer, SlotFree);
        if (slot == null)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        Slot_Load(slot, id, memberIndex, fieldEffectManager);
    }

    // OFFSET 0x021F11FC
    public static G3dRenderObj? FieldEffectManager_GetMapObjectModelRenderObj(FieldEffectManager fieldEffectManager, uint id)
    {
        MapObjectModelRenderer renderer = (MapObjectModelRenderer)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID)!;
        MapObjectModelSlot? slot = Renderer_FindSlot(renderer, id);
        if (slot == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return slot!.RenderObj;
    }

    // OFFSET 0x021F121C
    public static OverworldAnimManager? MapObjectModelEffect_New(MapObject mapObject, in VecFx32 offset)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        MapObjectModelEffectParams effectParams = new();
        effectParams.GraphicsID = MapObject.MapObject_GetGraphicsID(mapObject);
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.MapObject = mapObject;
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        return FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFuncs, offset, 0, effectParams, priority);
    }

    // OFFSET 0x021F1258
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        MapObjectModelEffectContext context = (MapObjectModelEffectContext)contextObject!;
        MapObjectModelEffectParams effectParams = (MapObjectModelEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.GraphicsID = effectParams.GraphicsID;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.MapObject = effectParams.MapObject;
        context.LocalID = MapObject.MapObject_GetLocalID(context.MapObject!);
        context.RenderObj = FieldEffectManager_GetMapObjectModelRenderObj(context.FieldEffectManager!, (uint)context.GraphicsID);
        if (MapObject.MapObject_IsAlias(context.MapObject!) == 1)
        {
            context.MapHeaderID = MapObject.MapObject_GetAliasMapHeaderID(context.MapObject!);
        }
        else
        {
            context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(context.MapObject!);
        }
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out context.Offset);
        return 1;
    }

    // OFFSET 0x021F12A8
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
    }

    // OFFSET 0x021F12AC
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        MapObjectModelEffectContext context = (MapObjectModelEffectContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        if (MapObject.MapObject_IsLocalObjectOfMap(mapObject, context.LocalID, context.MapHeaderID) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        MapObject.MapObject_CopySpriteJumpOffset(mapObject, out VecFx32 jumpOffset);
        position.X += jumpOffset.X + context.Offset.X;
        position.Y += jumpOffset.Y + context.Offset.Y;
        position.Z += jumpOffset.Z + context.Offset.Z;
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
    }

    // OFFSET 0x021F1310
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        MapObjectModelEffectContext context = (MapObjectModelEffectContext)contextObject!;
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        Simple3D.Simple3DRenderObj_DrawAtPosition(context.RenderObj!, position);
    }

    // OFFSET 0x021F1328
    public static void FieldEffectManager_LoadDefaultMapObjectModels(FieldEffectManager fieldEffectManager)
    {
        FieldEffectManager_AllocMapObjectModelSlots(fieldEffectManager, DefaultModelCount);
        for (int i = 0; i < DefaultModelCount; i++)
        {
            FieldEffectManager_LoadMapObjectModel(fieldEffectManager, sDefaultModels[i * 2], sDefaultModels[i * 2 + 1]);
        }
    }
}
