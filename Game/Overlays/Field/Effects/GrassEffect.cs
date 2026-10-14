namespace PlatinumSharp.Game;

public sealed class GrassRenderer
{
    public const int Size = 0x4;

    public FieldEffectManager? FieldEffectManager;
}

public sealed class GrassEffectParams
{
    public int X;
    public int Y;
    public int Z;
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public GrassRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class GrassEffectContext
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
    public GrassRenderer? Renderer;
    public MapObject? MapObject;
    public Billboard? Billboard;
}

public sealed class GrassRustleEffectParams
{
    public int X;
    public int Y;
    public int Z;
    public FieldEffectManager? FieldEffectManager;
    public GrassRenderer? Renderer;
}

public sealed class GrassRustleEffectContext
{
    public const int Size = 0x20;

    public int State;
    public int Finished;
    public int X;
    public int Y;
    public int Z;
    public FieldEffectManager? FieldEffectManager;
    public GrassRenderer? Renderer;
    public Billboard? Billboard;
}

public sealed class GrassShakeEffectParams
{
    public int X;
    public int Z;
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public GrassRenderer? Renderer;
}

public sealed class GrassShakeEffectContext
{
    public const int Size = 0x28;

    public int State;
    public int Finished;
    public int ShakeType;
    public int X;
    public int Z;
    public FieldSystem? FieldSystem;
    public FieldEffectManager? FieldEffectManager;
    public GrassRenderer? Renderer;
    public Billboard? Billboard;
}

public static class GrassEffect
{
    private const uint RendererID = 0x13;
    private const int TaskPriorityOffset = 2;
    private const uint GrassModelID = 0;
    private const uint GrassModelMember = 0x53;
    private const uint ShakeModelID = 5;
    private const uint ShakeModelMember = 0x54;
    private const uint GrassAnimID = 0;
    private const uint GrassAnimMember = 0xAA;
    private const uint ShakeAnimID = 3;
    private const uint ShakeAnimMember = 0xAB;
    private const uint ShakeRareAnimID = 4;
    private const uint ShakeRareAnimMember = 0xAC;
    private const uint ShakeShinyAnimID = 5;
    private const uint ShakeShinyAnimMember = 0xAD;
    private const uint GrassTextureID = 0;
    private const uint GrassTextureMember = 0;
    private const uint ShakeTextureID = 4;
    private const uint ShakeTextureMember = 1;
    private const uint ShakeRareTextureID = 5;
    private const uint ShakeRareTextureMember = 2;
    private const uint ShakeShinyTextureID = 6;
    private const uint ShakeShinyTextureMember = 3;
    private const uint GrassResourceID = 0;
    private const uint ShakeResourceID = 5;
    private const uint ShakeRareResourceID = 6;
    private const uint ShakeShinyResourceID = 7;
    private const int RustleFrames = 0xC;
    private const int StateRustle = 0;
    private const int StateSettle = 1;
    private const int StateIdle = 2;
    private const int ShakeTaskPriority = 0xFF;

    private static readonly BillboardAnim[] sGrassAnims =
    {
        new() { StartFrame = 0, EndFrame = 0xC, StopAtEnd = 0 },
        new() { StartFrame = 0, EndFrame = 0, StopAtEnd = 2 },
    };

    private static readonly BillboardAnim[] sShakeAnims =
    {
        new() { StartFrame = 0, EndFrame = 0x28, StopAtEnd = 1 },
        new() { StartFrame = 0, EndFrame = 0, StopAtEnd = 2 },
    };

    private static readonly BillboardAnim[] sShakeRareAnims =
    {
        new() { StartFrame = 0, EndFrame = 0x18, StopAtEnd = 1 },
        new() { StartFrame = 0, EndFrame = 0, StopAtEnd = 2 },
    };

    private static readonly BillboardAnim[] sShakeShinyAnims =
    {
        new() { StartFrame = 0, EndFrame = 0x28, StopAtEnd = 1 },
        new() { StartFrame = 0, EndFrame = 0, StopAtEnd = 2 },
    };

    private static readonly OverworldAnimManagerFuncs sGrassFuncs = new(GrassEffectContext.Size, () => new GrassEffectContext(), Grass_Init, Grass_Exit, Grass_Tick, Grass_Render);

    private static readonly OverworldAnimManagerFuncs sRustleFuncs = new(GrassRustleEffectContext.Size, () => new GrassRustleEffectContext(), Rustle_Init, Rustle_Exit, Rustle_Tick, Rustle_Render);

    private static readonly OverworldAnimManagerFuncs sShakeFuncs = new(GrassShakeEffectContext.Size, () => new GrassShakeEffectContext(), Shake_Init, Shake_Exit, Shake_Tick, Shake_Render);

    // OFFSET 0x021F2D20
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        GrassRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        Renderer_Load(renderer);
        return renderer;
    }

    // OFFSET 0x021F2D3C
    public static void Renderer_Free(object? context)
    {
        GrassRenderer renderer = (GrassRenderer)context!;
        Renderer_Unload(renderer);
    }

    // OFFSET 0x021F2D4C
    private static void Renderer_Load(GrassRenderer renderer)
    {
        FieldEffectManager fieldEffectManager = renderer.FieldEffectManager!;
        FieldEffects.FieldEffectManager_LoadBillboardModel(fieldEffectManager, GrassModelID, GrassModelMember);
        FieldEffects.FieldEffectManager_LoadBillboardModel(fieldEffectManager, ShakeModelID, ShakeModelMember);
        FieldEffects.FieldEffectManager_LoadBillboardAnim(fieldEffectManager, GrassAnimID, GrassAnimMember);
        FieldEffects.FieldEffectManager_LoadBillboardAnim(fieldEffectManager, ShakeAnimID, ShakeAnimMember);
        FieldEffects.FieldEffectManager_LoadBillboardAnim(fieldEffectManager, ShakeRareAnimID, ShakeRareAnimMember);
        FieldEffects.FieldEffectManager_LoadBillboardAnim(fieldEffectManager, ShakeShinyAnimID, ShakeShinyAnimMember);
        FieldEffects.FieldEffectManager_LoadBillboardTexture(fieldEffectManager, GrassTextureID, GrassTextureMember, 1);
        FieldEffects.FieldEffectManager_LoadBillboardTexture(fieldEffectManager, ShakeTextureID, ShakeTextureMember, 1);
        FieldEffects.FieldEffectManager_LoadBillboardTexture(fieldEffectManager, ShakeRareTextureID, ShakeRareTextureMember, 1);
        FieldEffects.FieldEffectManager_LoadBillboardTexture(fieldEffectManager, ShakeShinyTextureID, ShakeShinyTextureMember, 1);
        FieldEffects.FieldEffectManager_InitBillboardResources(fieldEffectManager, GrassResourceID, GrassModelID, GrassAnimID, GrassTextureID, 0, sGrassAnims);
        FieldEffects.FieldEffectManager_InitBillboardResources(fieldEffectManager, ShakeResourceID, ShakeModelID, ShakeAnimID, ShakeTextureID, 0, sShakeAnims);
        FieldEffects.FieldEffectManager_InitBillboardResources(fieldEffectManager, ShakeRareResourceID, ShakeModelID, ShakeRareAnimID, ShakeRareTextureID, 0, sShakeRareAnims);
        FieldEffects.FieldEffectManager_InitBillboardResources(fieldEffectManager, ShakeShinyResourceID, ShakeModelID, ShakeShinyAnimID, ShakeShinyTextureID, 0, sShakeShinyAnims);
    }

    // OFFSET 0x021F2E2C
    private static void Renderer_Unload(GrassRenderer renderer)
    {
        FieldEffectManager fieldEffectManager = renderer.FieldEffectManager!;
        FieldEffects.FieldEffectManager_FreeBillboardModel(fieldEffectManager, GrassModelID);
        FieldEffects.FieldEffectManager_FreeBillboardModel(fieldEffectManager, ShakeModelID);
        FieldEffects.FieldEffectManager_FreeBillboardAnim(fieldEffectManager, GrassAnimID);
        FieldEffects.FieldEffectManager_FreeBillboardAnim(fieldEffectManager, ShakeAnimID);
        FieldEffects.FieldEffectManager_FreeBillboardAnim(fieldEffectManager, ShakeRareAnimID);
        FieldEffects.FieldEffectManager_FreeBillboardAnim(fieldEffectManager, ShakeShinyAnimID);
        FieldEffects.FieldEffectManager_FreeBillboardTexture(fieldEffectManager, GrassTextureID);
        FieldEffects.FieldEffectManager_FreeBillboardTexture(fieldEffectManager, ShakeTextureID);
        FieldEffects.FieldEffectManager_FreeBillboardTexture(fieldEffectManager, ShakeRareTextureID);
        FieldEffects.FieldEffectManager_FreeBillboardTexture(fieldEffectManager, ShakeShinyTextureID);
        FieldEffects.FieldEffectManager_ReleaseBillboardResources(fieldEffectManager, GrassResourceID);
        FieldEffects.FieldEffectManager_ReleaseBillboardResources(fieldEffectManager, ShakeResourceID);
        FieldEffects.FieldEffectManager_ReleaseBillboardResources(fieldEffectManager, ShakeRareResourceID);
        FieldEffects.FieldEffectManager_ReleaseBillboardResources(fieldEffectManager, ShakeShinyResourceID);
    }

    // OFFSET 0x021F2EA4
    public static void GrassEffect_New(MapObject mapObject, int skipRustle)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        GrassEffectParams effectParams = new();
        effectParams.X = MapObject.MapObject_GetX(mapObject);
        effectParams.Y = MapObject.MapObject_GetY(mapObject);
        effectParams.Z = MapObject.MapObject_GetZ(mapObject);
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.FieldSystem = FieldEffects.FieldEffectManager_GetFieldSystem(fieldEffectManager);
        effectParams.Renderer = (GrassRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        effectParams.MapObject = mapObject;
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sGrassFuncs, position, skipRustle, effectParams, priority);
    }

    // OFFSET 0x021F2F0C
    private static int Grass_Init(OverworldAnimManager animManager, object? contextObject)
    {
        GrassEffectContext context = (GrassEffectContext)contextObject!;
        GrassEffectParams effectParams = (GrassEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
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
        context.Billboard = FieldEffects.FieldEffectManager_AddBillboardWithResourceID(context.FieldEffectManager!, GrassResourceID, position);
        if (OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager) == 0)
        {
            context.State = StateSettle;
        }
        return 1;
    }

    // OFFSET 0x021F2FA0
    private static void Grass_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        GrassEffectContext context = (GrassEffectContext)contextObject!;
        Billboards.Billboard_Delete(context.Billboard);
    }

    // OFFSET 0x021F2FAC
    private static void Grass_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        GrassEffectContext context = (GrassEffectContext)contextObject!;
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
            case StateRustle:
                Billboards.Billboard_AdvanceAnim(context.Billboard, Fx32.One);
                if (Billboards.Billboard_GetAnimFrameNum(context.Billboard) / Fx32.One >= RustleFrames)
                {
                    context.State = StateSettle;
                }
                break;
            case StateSettle:
                Billboards.Billboard_SetFrameNum(context.Billboard, RustleFrames << Fx32.Shift);
                context.State = StateIdle;
                goto case StateIdle;
            case StateIdle:
            {
                if (MapObject.MapObject_IsLocalObjectOfMapWithGraphics(mapObject, context.GraphicsID, context.LocalID, context.MapHeaderID) == 0)
                {
                    FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
                    return;
                }
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

    // OFFSET 0x021F308C
    private static void Grass_Render(OverworldAnimManager animManager, object? contextObject)
    {
        GrassEffectContext context = (GrassEffectContext)contextObject!;
        if (MapObject.MapObject_IsLocalObjectOfMapWithGraphics(context.MapObject!, context.GraphicsID, context.LocalID, context.MapHeaderID) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        Billboards.Billboard_SetPos(context.Billboard, position);
    }

    // OFFSET 0x021F30C4
    private static int Rustle_Init(OverworldAnimManager animManager, object? contextObject)
    {
        GrassRustleEffectContext context = (GrassRustleEffectContext)contextObject!;
        GrassRustleEffectParams effectParams = (GrassRustleEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.X = effectParams.X;
        context.Y = effectParams.Y;
        context.Z = effectParams.Z;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        context.Billboard = FieldEffects.FieldEffectManager_AddBillboardWithResourceID(context.FieldEffectManager!, GrassResourceID, position);
        return 1;
    }

    // OFFSET 0x021F30FC
    private static void Rustle_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        GrassRustleEffectContext context = (GrassRustleEffectContext)contextObject!;
        Billboards.Billboard_Delete(context.Billboard);
    }

    // OFFSET 0x021F3108
    private static void Rustle_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        GrassRustleEffectContext context = (GrassRustleEffectContext)contextObject!;
        if (context.State == 0)
        {
            Billboards.Billboard_AdvanceAnim(context.Billboard, Fx32.One);
            if (Billboards.Billboard_GetAnimFrameNum(context.Billboard) / Fx32.One >= RustleFrames)
            {
                context.Finished = 1;
                context.State = 1;
            }
        }
    }

    // OFFSET 0x021F313C
    private static void Rustle_Render(OverworldAnimManager animManager, object? contextObject)
    {
        GrassRustleEffectContext context = (GrassRustleEffectContext)contextObject!;
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        Billboards.Billboard_SetPos(context.Billboard, position);
    }

    // OFFSET 0x021F3154
    public static OverworldAnimManager? GrassShakeEffect_New(FieldSystem fieldSystem, int x, int z, int shakeType)
    {
        FieldEffectManager fieldEffectManager = fieldSystem.FieldEffectManager!;
        GrassShakeEffectParams effectParams = new();
        effectParams.FieldSystem = fieldSystem;
        effectParams.X = x;
        effectParams.Z = z;
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.Renderer = (GrassRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        VecFx32 position;
        position.X = (x << 16) + 0x8000;
        position.Y = 0;
        position.Z = (z << 16) + 0x9000;
        return FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sShakeFuncs, position, shakeType, effectParams, ShakeTaskPriority);
    }

    // OFFSET 0x021F31A8
    public static int GrassShakeEffect_IsFinished(OverworldAnimManager animManager)
    {
        GrassShakeEffectContext context = (GrassShakeEffectContext)OverworldAnimManagers.OverworldAnimManager_GetContext(animManager)!;
        return context.Finished;
    }

    // OFFSET 0x021F31B4
    private static int Shake_Init(OverworldAnimManager animManager, object? contextObject)
    {
        GrassShakeEffectContext context = (GrassShakeEffectContext)contextObject!;
        GrassShakeEffectParams effectParams = (GrassShakeEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.X = effectParams.X;
        context.Z = effectParams.Z;
        context.FieldSystem = effectParams.FieldSystem;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.ShakeType = OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager);
        VecFx32 position;
        position.X = (context.X << 16) + 0x8000;
        position.Z = (context.Z << 16) + 0x9000;
        position.Y = 0;
        MapObjectMove.MapObject_RecalculatePositionHeight(context.FieldSystem!, ref position);
        position.Y += 0x1000;
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
        uint resourceID;
        switch (context.ShakeType)
        {
            case 0:
                resourceID = ShakeResourceID;
                break;
            case 1:
                resourceID = ShakeRareResourceID;
                break;
            default:
                resourceID = ShakeShinyResourceID;
                break;
        }
        context.Billboard = FieldEffects.FieldEffectManager_AddBillboardWithResourceID(context.FieldEffectManager!, resourceID, position);
        return 1;
    }

    // OFFSET 0x021F3238
    private static void Shake_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        GrassShakeEffectContext context = (GrassShakeEffectContext)contextObject!;
        Billboards.Billboard_Delete(context.Billboard);
    }

    // OFFSET 0x021F3244
    private static void Shake_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        GrassShakeEffectContext context = (GrassShakeEffectContext)contextObject!;
        if (context.State == 0)
        {
            if (Billboards.Billboard_AdvanceAnim(context.Billboard, Fx32.One) == 1)
            {
                context.Finished = 1;
                context.State++;
            }
        }
    }

    // OFFSET 0x021F326C
    private static void Shake_Render(OverworldAnimManager animManager, object? contextObject)
    {
        GrassShakeEffectContext context = (GrassShakeEffectContext)contextObject!;
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        Billboards.Billboard_SetPos(context.Billboard, position);
    }
}
