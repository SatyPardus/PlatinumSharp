namespace PlatinumSharp.Game;

public sealed class JumpDustRenderer
{
    public const int Size = 0x8;

    public int VariantsLoaded;
    public FieldEffectManager? FieldEffectManager;
}

public sealed class JumpDustEffectParams
{
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public JumpDustRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class JumpDustEffectContext
{
    public const int Size = 0x24;

    public int State;
    public int LocalID;
    public int MapHeaderID;
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public JumpDustRenderer? Renderer;
    public MapObject? MapObject;
    public Billboard? Billboard;
}

public static class JumpDustEffect
{
    private const uint RendererID = 0x17;
    private const int TaskPriorityOffset = 2;
    private const uint ModelID = 6;
    private const uint ModelMember = 0x58;
    private const uint AnimID = 6;
    private const uint AnimMember = 0xAF;
    private const uint TextureID = 7;
    private const uint TextureMember = 7;
    private const uint ResourceID = 8;
    private const uint VariantModelID1 = 0xC;
    private const uint VariantModelMember1 = 0x9F;
    private const uint VariantModelID2 = 0xD;
    private const uint VariantModelMember2 = 0x9D;
    private const uint VariantModelID3 = 0xE;
    private const uint VariantModelMember3 = 0x9E;
    private const uint VariantResourceID1 = 0xE;
    private const uint VariantResourceID2 = 0xF;
    private const uint VariantResourceID3 = 0x10;
    private const int AnimFrames = 9;
    private const int DepthOffset = 0x8000;

    private static readonly BillboardAnim[] sAnims =
    {
        new() { StartFrame = 0, EndFrame = 9, StopAtEnd = 1 },
        new() { StartFrame = 0, EndFrame = 0, StopAtEnd = 2 },
    };

    private static readonly uint[] sResourceIDs = { ResourceID, VariantResourceID1, VariantResourceID2, VariantResourceID3 };

    private static readonly VecFx32[] sVariantOffsets =
    {
        new() { X = 0, Y = 0, Z = 0x8000 },
        new() { X = 0xB000, Y = 0xE000, Z = 0x11000 },
        new() { X = -0xB000, Y = 0xE000, Z = 0x11000 },
        new() { X = 0, Y = 0x20000, Z = 0x8000 },
    };

    private static readonly OverworldAnimManagerFuncs sFuncs = new(JumpDustEffectContext.Size, () => new JumpDustEffectContext(), Effect_Init, Effect_Exit, Effect_Tick, Effect_Render);

    // OFFSET 0x021F348C
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        JumpDustRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        Renderer_Load(renderer);
        return renderer;
    }

    // OFFSET 0x021F34A8
    public static void Renderer_Free(object? context)
    {
        JumpDustRenderer renderer = (JumpDustRenderer)context!;
        Renderer_Unload(renderer);
    }

    // OFFSET 0x021F34B8
    public static void FieldEffectManager_LoadJumpDustVariants(FieldEffectManager fieldEffectManager)
    {
        JumpDustRenderer renderer = (JumpDustRenderer)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID)!;
        if (renderer.VariantsLoaded != 1)
        {
            renderer.VariantsLoaded = 1;
            Renderer_LoadVariants(renderer);
        }
    }

    // OFFSET 0x021F34D0
    private static void Renderer_Load(JumpDustRenderer renderer)
    {
        FieldEffects.FieldEffectManager_LoadBillboardModel(renderer.FieldEffectManager!, ModelID, ModelMember);
        FieldEffects.FieldEffectManager_LoadBillboardAnim(renderer.FieldEffectManager!, AnimID, AnimMember);
        FieldEffects.FieldEffectManager_LoadBillboardTexture(renderer.FieldEffectManager!, TextureID, TextureMember, 1);
        FieldEffects.FieldEffectManager_InitBillboardResources(renderer.FieldEffectManager!, ResourceID, ModelID, AnimID, TextureID, 0, sAnims);
    }

    // OFFSET 0x021F3518
    private static void Renderer_LoadVariants(JumpDustRenderer renderer)
    {
        FieldEffects.FieldEffectManager_LoadBillboardModel(renderer.FieldEffectManager!, VariantModelID1, VariantModelMember1);
        FieldEffects.FieldEffectManager_LoadBillboardModel(renderer.FieldEffectManager!, VariantModelID2, VariantModelMember2);
        FieldEffects.FieldEffectManager_LoadBillboardModel(renderer.FieldEffectManager!, VariantModelID3, VariantModelMember3);
        FieldEffects.FieldEffectManager_InitBillboardResources(renderer.FieldEffectManager!, VariantResourceID1, VariantModelID1, AnimID, TextureID, 0, sAnims);
        FieldEffects.FieldEffectManager_InitBillboardResources(renderer.FieldEffectManager!, VariantResourceID2, VariantModelID2, AnimID, TextureID, 0, sAnims);
        FieldEffects.FieldEffectManager_InitBillboardResources(renderer.FieldEffectManager!, VariantResourceID3, VariantModelID3, AnimID, TextureID, 0, sAnims);
    }

    // OFFSET 0x021F358C
    private static void Renderer_Unload(JumpDustRenderer renderer)
    {
        FieldEffects.FieldEffectManager_FreeBillboardModel(renderer.FieldEffectManager!, ModelID);
        FieldEffects.FieldEffectManager_FreeBillboardAnim(renderer.FieldEffectManager!, AnimID);
        FieldEffects.FieldEffectManager_FreeBillboardTexture(renderer.FieldEffectManager!, TextureID);
        FieldEffects.FieldEffectManager_ReleaseBillboardResources(renderer.FieldEffectManager!, ResourceID);
        if (renderer.VariantsLoaded == 1)
        {
            FieldEffects.FieldEffectManager_FreeBillboardModel(renderer.FieldEffectManager!, VariantModelID1);
            FieldEffects.FieldEffectManager_FreeBillboardModel(renderer.FieldEffectManager!, VariantModelID2);
            FieldEffects.FieldEffectManager_FreeBillboardModel(renderer.FieldEffectManager!, VariantModelID3);
            FieldEffects.FieldEffectManager_ReleaseBillboardResources(renderer.FieldEffectManager!, VariantResourceID1);
            FieldEffects.FieldEffectManager_ReleaseBillboardResources(renderer.FieldEffectManager!, VariantResourceID2);
            FieldEffects.FieldEffectManager_ReleaseBillboardResources(renderer.FieldEffectManager!, VariantResourceID3);
        }
    }

    // OFFSET 0x021F35E8
    private static void BuildParams(FieldEffectManager fieldEffectManager, MapObject mapObject, JumpDustEffectParams effectParams, out VecFx32 position, out int priority)
    {
        effectParams.FieldSystem = MapObject.MapObject_FieldSystem(mapObject);
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.Renderer = (JumpDustRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        effectParams.MapObject = mapObject;
        MapObject.MapObject_CopyPos(mapObject, out position);
        int x = MapObject.MapObject_GetX(mapObject);
        int z = MapObject.MapObject_GetZ(mapObject);
        MapObjectMove.VecFx32_SetPosFromMapCoords(x, z, ref position);
        priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
    }

    // OFFSET 0x021F3638
    public static void JumpDustEffect_New(MapObject mapObject)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        JumpDustEffectParams effectParams = new();
        BuildParams(fieldEffectManager, mapObject, effectParams, out VecFx32 position, out int priority);
        position.Z += DepthOffset;
        FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFuncs, position, 0, effectParams, priority);
    }

    // OFFSET 0x021F3678
    public static void JumpDustEffect_NewVariant(MapObject mapObject, int variant)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        JumpDustEffectParams effectParams = new();
        BuildParams(fieldEffectManager, mapObject, effectParams, out VecFx32 position, out int priority);
        if (effectParams.Renderer!.VariantsLoaded != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        position.X += sVariantOffsets[variant].X;
        position.Y += sVariantOffsets[variant].Y;
        position.Z += sVariantOffsets[variant].Z;
        FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFuncs, position, variant, effectParams, priority);
    }

    // OFFSET 0x021F36F4
    private static int Effect_Init(OverworldAnimManager animManager, object? contextObject)
    {
        JumpDustEffectContext context = (JumpDustEffectContext)contextObject!;
        JumpDustEffectParams effectParams = (JumpDustEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.FieldSystem = effectParams.FieldSystem;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.MapObject = effectParams.MapObject;
        context.LocalID = MapObject.MapObject_GetLocalID(context.MapObject!);
        context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(context.MapObject!);
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        int variant = OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager);
        context.Billboard = FieldEffects.FieldEffectManager_AddBillboardWithResourceID(context.FieldEffectManager!, sResourceIDs[variant], position);
        return 1;
    }

    // OFFSET 0x021F3754
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        JumpDustEffectContext context = (JumpDustEffectContext)contextObject!;
        Billboards.Billboard_Delete(context.Billboard);
    }

    // OFFSET 0x021F3760
    private static void Effect_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        JumpDustEffectContext context = (JumpDustEffectContext)contextObject!;
        if (context.State == 0)
        {
            Billboards.Billboard_AdvanceAnim(context.Billboard, Fx32.One);
            if (Billboards.Billboard_GetAnimFrameNum(context.Billboard) / Fx32.One >= AnimFrames)
            {
                FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            }
        }
    }

    // OFFSET 0x021F3790
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        JumpDustEffectContext context = (JumpDustEffectContext)contextObject!;
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        Billboards.Billboard_SetPos(context.Billboard, position);
    }
}
