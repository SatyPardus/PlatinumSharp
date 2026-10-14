namespace PlatinumSharp.Game;

public sealed class WarpArrowRenderer
{
    public const int Size = 0x6C;

    public FieldEffectManager? FieldEffectManager;
    public readonly Simple3DModel Model = new();
    public readonly G3dRenderObj RenderObj = new();
}

public sealed class WarpArrowEffectParams
{
    public FieldEffectManager? FieldEffectManager;
    public WarpArrowRenderer? Renderer;
    public PlayerAvatar? PlayerAvatar;
}

public sealed class WarpArrowEffectContext
{
    public const int Size = 0x30;

    public int Hidden;
    public int Direction;
    public byte TileBehavior;
    public int LocalID;
    public int MapHeaderID;
    public int FrameTimer;
    public int Frame;
    public FieldEffectManager? FieldEffectManager;
    public WarpArrowRenderer? Renderer;
    public PlayerAvatar? PlayerAvatar;
}

public static class WarpArrowEffect
{
    private const uint RendererID = 0xE;
    private const int ModelMember = 0x50;
    private const int TaskPriorityOffset = 2;
    private const int DirectionNone = -1;
    private const int DirectionCount = 4;
    private const int FrameCount = 2;
    private const int FrameDuration = 0xF;

    private static readonly VecFx32[] sFrameOffsets =
    {
        new() { X = 0, Y = 0, Z = 0 },
        new() { X = 0, Y = 0, Z = -0x2000 },
        new() { X = 0, Y = 0, Z = 0 },
        new() { X = 0, Y = 0, Z = 0x2000 },
        new() { X = 0, Y = 0, Z = 0 },
        new() { X = -0x2000, Y = 0, Z = 0 },
        new() { X = 0, Y = 0, Z = 0 },
        new() { X = 0x2000, Y = 0, Z = 0 },
    };

    private static readonly OverworldAnimManagerFuncs sFuncs = new(WarpArrowEffectContext.Size, () => new WarpArrowEffectContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    // OFFSET 0x021F61BC
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        WarpArrowRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        Renderer_LoadModel(renderer);
        return renderer;
    }

    // OFFSET 0x021F61D8
    public static void Renderer_Free(object? context)
    {
        WarpArrowRenderer renderer = (WarpArrowRenderer)context!;
        Renderer_FreeModel(renderer);
    }

    // OFFSET 0x021F61E8
    private static void Renderer_LoadModel(WarpArrowRenderer renderer)
    {
        FieldEffects.FieldEffectManager_LoadModel(renderer.FieldEffectManager!, renderer.Model, 0, ModelMember, 0);
        Simple3D.Simple3DRenderObj_Init(renderer.RenderObj, renderer.Model);
    }

    // OFFSET 0x021F620C
    private static void Renderer_FreeModel(WarpArrowRenderer renderer)
    {
        Simple3D.Simple3DModel_Free(renderer.Model);
    }

    // OFFSET 0x021F6218
    public static void WarpArrowEffect_New(PlayerAvatar playerAvatar)
    {
        VecFx32 position = default;
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar);
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        WarpArrowEffectParams effectParams = new();
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.Renderer = (WarpArrowRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        effectParams.PlayerAvatar = playerAvatar;
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFuncs, position, 0, effectParams, priority);
    }

    // OFFSET 0x021F6264
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        WarpArrowEffectContext context = (WarpArrowEffectContext)contextObject!;
        WarpArrowEffectParams effectParams = (WarpArrowEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.PlayerAvatar = effectParams.PlayerAvatar;
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(effectParams.PlayerAvatar!);
        context.Direction = DirectionNone;
        context.LocalID = MapObject.MapObject_GetLocalID(mapObject);
        context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(mapObject);
        return 1;
    }

    // OFFSET 0x021F629C
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
    }

    // OFFSET 0x021F62A0
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        WarpArrowEffectContext context = (WarpArrowEffectContext)contextObject!;
        PlayerAvatar playerAvatar = context.PlayerAvatar!;
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar);
        if (MapObject.MapObject_IsLocalObjectOfMap(mapObject, context.LocalID, context.MapHeaderID) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        int previousDirection = context.Direction;
        context.TileBehavior = (byte)MapObject.MapObject_GetCurrTileBehavior(mapObject);
        context.Direction = GetWarpEntranceDirection(context.TileBehavior);
        context.Hidden = 0;
        int direction = context.Direction;
        if (direction == DirectionNone || direction != PlayerAvatar.PlayerAvatar_GetFacingDir(playerAvatar))
        {
            context.FrameTimer = 0;
            context.Frame = 0;
            context.Hidden = 1;
            return;
        }
        if (previousDirection != direction)
        {
            context.FrameTimer = 0;
            context.Frame = 0;
        }
        int x = MapObject.MapObject_GetX(mapObject) + MapObjectMove.MapObject_GetDxFromDir(context.Direction);
        int z = MapObject.MapObject_GetZ(mapObject) + MapObjectMove.MapObject_GetDzFromDir(context.Direction);
        VecFx32 position = default;
        MapObjectMove.VecFx32_SetPosFromMapCoords(x, z, ref position);
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 objectPosition);
        position.Y = objectPosition.Y;
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
        context.FrameTimer++;
        if (context.FrameTimer > FrameDuration)
        {
            context.FrameTimer = 0;
            context.Frame = (context.Frame + 1) % FrameCount;
        }
    }

    // OFFSET 0x021F636C
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        WarpArrowEffectContext context = (WarpArrowEffectContext)contextObject!;
        if (context.Hidden == 1 || context.Direction == DirectionNone)
        {
            return;
        }
        VecFx32 scale = new() { X = Fx32.One, Y = Fx32.One, Z = Fx32.One };
        VecFx32 offset = sFrameOffsets[context.Direction * FrameCount + context.Frame];
        G3dRenderObj renderObj = context.Renderer!.RenderObj;
        int angle = 0;
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
        position.X += offset.X;
        position.Y += offset.Y;
        position.Z += offset.Z;
        Simple3D.Simple3DRenderObj_Draw(renderObj, position, scale, rotation);
    }

    // OFFSET 0x021F6414
    private static int GetWarpEntranceDirection(int tileBehavior)
    {
        if (TileBehavior.TileBehavior_IsWarpEntranceNorth(tileBehavior) != 0)
        {
            return 0;
        }
        if (TileBehavior.TileBehavior_IsWarpEntranceSouth(tileBehavior) != 0)
        {
            return 1;
        }
        if (TileBehavior.TileBehavior_IsWarpEntranceWest(tileBehavior) != 0)
        {
            return 2;
        }
        if (TileBehavior.TileBehavior_IsWarpEntranceEast(tileBehavior) != 0)
        {
            return 3;
        }
        return -1;
    }
}
