namespace PlatinumSharp.Game;

public sealed class EmoteIconRenderer
{
    public const int Size = 0xC;

    public short RefCount;
    public short AltRefCount;
    public byte Loaded;
    public byte AltLoaded;
    public byte FogDisabled;
    public FieldEffectManager? FieldEffectManager;
}

public sealed class EmoteIconEffectParams
{
    public int Kind;
    public FieldEffectManager? FieldEffectManager;
    public EmoteIconRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class EmoteIconEffectContext
{
    public const int Size = 0x44;

    public int State;
    public int Timer;
    public int LocalID;
    public int MapHeaderID;
    public int Hidden;
    public int Finished;
    public VecFx32 Offset;
    public int Unused24;
    public int Velocity;
    public int Unused2C;
    public int Kind;
    public FieldEffectManager? FieldEffectManager;
    public EmoteIconRenderer? Renderer;
    public MapObject? MapObject;
    public Billboard? Billboard;
}

public static class EmoteIconEffect
{
    public const int KindDefault = 0;
    public const int KindAlt = 1;

    private const uint RendererID = 0x14;
    private const uint ModelID = 1;
    private const uint ModelMember = 0x55;
    private const uint AnimID = 1;
    private const uint AnimMember = 0xAA;
    private const uint TextureID = 1;
    private const uint TextureMember = 4;
    private const uint ResourceID = 1;
    private const uint AltModelID = 0xA;
    private const uint AltModelMember = 0x6C;
    private const uint AltAnimID = 0xA;
    private const uint AltAnimMember = 0xAA;
    private const uint AltTextureID = 0xB;
    private const uint AltTextureMember = 0xD;
    private const uint AltResourceID = 0xC;
    private const int MaterialPolygonAttr = 0xC;
    private const uint PolygonAttrFogEnable = 0x8000;
    private const int InitialVelocity = 0x6000;
    private const int Gravity = 0x2000;
    private const int HoldFrames = 0x1E;
    private const int SoundEffect = 0x5DD;
    private const int HeightAboveSprite = 0x20000;
    private const int DepthOffset = 0x1000;

    private static readonly BillboardAnim[] sAnims =
    {
        new() { StartFrame = 0, EndFrame = 1, StopAtEnd = 0 },
        new() { StartFrame = 0, EndFrame = 0, StopAtEnd = 2 },
    };

    private static readonly uint[] sResourceIDs = { ResourceID, AltResourceID };

    private static readonly OverworldAnimManagerFuncs sFollowFuncs = new(EmoteIconEffectContext.Size, () => new EmoteIconEffectContext(), Follow_Init, Effect_Exit, Follow_Tick, Effect_Render);

    private static readonly OverworldAnimManagerFuncs sStaticFuncs = new(EmoteIconEffectContext.Size, () => new EmoteIconEffectContext(), Static_Init, Effect_Exit, Static_Tick, Effect_Render);

    private static readonly OverworldAnimManagerFuncs sDelayedFuncs = new(EmoteIconEffectContext.Size, () => new EmoteIconEffectContext(), Delayed_Init, Effect_Exit, Delayed_Tick, Effect_Render);

    // OFFSET 0x021F5A10
    public static object? Renderer_New(FieldEffectManager fieldEffectManager)
    {
        EmoteIconRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        return renderer;
    }

    // OFFSET 0x021F5A24
    public static void Renderer_Free(object? context)
    {
        EmoteIconRenderer renderer = (EmoteIconRenderer)context!;
        Renderer_UnloadDefault(renderer);
    }

    // OFFSET 0x021F5A34
    private static void Renderer_AddRefDefault(EmoteIconRenderer renderer)
    {
        renderer.RefCount++;
    }

    // OFFSET 0x021F5A40
    private static void Renderer_ReleaseDefault(EmoteIconRenderer renderer)
    {
        renderer.RefCount--;
        if (renderer.RefCount < 0)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021F5A58
    private static void Renderer_AddRefAlt(EmoteIconRenderer renderer)
    {
        renderer.AltRefCount++;
    }

    // OFFSET 0x021F5A64
    private static void Renderer_ReleaseAlt(EmoteIconRenderer renderer)
    {
        renderer.AltRefCount--;
        if (renderer.AltRefCount < 0)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021F5A7C
    private static void Renderer_AddRef(EmoteIconRenderer renderer, int kind)
    {
        if (kind == KindDefault)
        {
            Renderer_AddRefDefault(renderer);
            return;
        }
        Renderer_AddRefAlt(renderer);
    }

    // OFFSET 0x021F5A90
    private static void Renderer_Release(EmoteIconRenderer renderer, int kind)
    {
        if (kind == KindDefault)
        {
            Renderer_ReleaseDefault(renderer);
            return;
        }
        Renderer_ReleaseAlt(renderer);
    }

    // OFFSET 0x021F5AA4
    public static void FieldSystem_SetEmoteIconFogDisabled(FieldSystem fieldSystem, int fogDisabled)
    {
        EmoteIconRenderer renderer = (EmoteIconRenderer)FieldEffects.FieldEffectManager_GetRendererContext(fieldSystem.FieldEffectManager!, RendererID)!;
        renderer.FogDisabled = (byte)fogDisabled;
    }

    // OFFSET 0x021F5AB4
    private static void Renderer_LoadDefault(EmoteIconRenderer renderer)
    {
        if (renderer.Loaded == 0)
        {
            renderer.Loaded = 1;
            FieldEffects.FieldEffectManager_LoadBillboardModel(renderer.FieldEffectManager!, ModelID, ModelMember);
            FieldEffects.FieldEffectManager_LoadBillboardAnim(renderer.FieldEffectManager!, AnimID, AnimMember);
            FieldEffects.FieldEffectManager_LoadBillboardTexture(renderer.FieldEffectManager!, TextureID, TextureMember, 1);
            FieldEffects.FieldEffectManager_InitBillboardResources(renderer.FieldEffectManager!, ResourceID, ModelID, AnimID, TextureID, 0, sAnims);
        }
    }

    // OFFSET 0x021F5B00
    private static void Renderer_LoadAlt(EmoteIconRenderer renderer)
    {
        if (renderer.AltLoaded == 0)
        {
            renderer.AltLoaded = 1;
            FieldEffects.FieldEffectManager_LoadBillboardModel(renderer.FieldEffectManager!, AltModelID, AltModelMember);
            FieldEffects.FieldEffectManager_LoadBillboardAnim(renderer.FieldEffectManager!, AltAnimID, AltAnimMember);
            FieldEffects.FieldEffectManager_LoadBillboardTexture(renderer.FieldEffectManager!, AltTextureID, AltTextureMember, 1);
            FieldEffects.FieldEffectManager_InitBillboardResources(renderer.FieldEffectManager!, AltResourceID, AltModelID, AltAnimID, AltTextureID, 0, sAnims);
        }
    }

    // OFFSET 0x021F5B50
    private static void Renderer_UnloadDefault(EmoteIconRenderer renderer)
    {
        if (renderer.Loaded == 1)
        {
            renderer.Loaded = 0;
            FieldEffects.FieldEffectManager_FreeBillboardModel(renderer.FieldEffectManager!, ModelID);
            FieldEffects.FieldEffectManager_FreeBillboardAnim(renderer.FieldEffectManager!, AnimID);
            FieldEffects.FieldEffectManager_FreeBillboardTexture(renderer.FieldEffectManager!, TextureID);
            FieldEffects.FieldEffectManager_ReleaseBillboardResources(renderer.FieldEffectManager!, ResourceID);
        }
    }

    // OFFSET 0x021F5B80
    private static void Renderer_UnloadAlt(EmoteIconRenderer renderer)
    {
        if (renderer.AltLoaded == 1)
        {
            renderer.AltLoaded = 0;
            FieldEffects.FieldEffectManager_FreeBillboardModel(renderer.FieldEffectManager!, AltModelID);
            FieldEffects.FieldEffectManager_FreeBillboardAnim(renderer.FieldEffectManager!, AltAnimID);
            FieldEffects.FieldEffectManager_FreeBillboardTexture(renderer.FieldEffectManager!, AltTextureID);
            FieldEffects.FieldEffectManager_ReleaseBillboardResources(renderer.FieldEffectManager!, AltResourceID);
        }
    }

    // OFFSET 0x021F5BB0
    private static void Renderer_LoadDefaultIfUnused(EmoteIconRenderer renderer)
    {
        if (renderer.RefCount == 0)
        {
            Renderer_LoadDefault(renderer);
        }
    }

    // OFFSET 0x021F5BC0
    private static void Renderer_LoadAltIfUnused(EmoteIconRenderer renderer)
    {
        if (renderer.AltRefCount == 0)
        {
            Renderer_LoadAlt(renderer);
        }
    }

    // OFFSET 0x021F5BD0
    private static void Renderer_UnloadDefaultIfUnused(EmoteIconRenderer renderer)
    {
        if (renderer.RefCount == 0)
        {
            Renderer_UnloadDefault(renderer);
        }
    }

    // OFFSET 0x021F5BE0
    private static void Renderer_UnloadAltIfUnused(EmoteIconRenderer renderer)
    {
        if (renderer.AltRefCount == 0)
        {
            Renderer_UnloadAlt(renderer);
        }
    }

    // OFFSET 0x021F5BF0
    private static void Renderer_LoadIfUnused(EmoteIconRenderer renderer, int kind)
    {
        if (kind == KindDefault)
        {
            Renderer_LoadDefaultIfUnused(renderer);
            return;
        }
        Renderer_LoadAltIfUnused(renderer);
    }

    // OFFSET 0x021F5C04
    private static void Renderer_UnloadIfUnused(EmoteIconRenderer renderer, int kind)
    {
        if (kind == KindDefault)
        {
            Renderer_UnloadDefaultIfUnused(renderer);
            return;
        }
        Renderer_UnloadAltIfUnused(renderer);
    }

    // OFFSET 0x021F5C18
    private static Billboard? AddBillboard(FieldEffectManager fieldEffectManager, int kind, in VecFx32 position, int fogDisabled)
    {
        Billboard? billboard = FieldEffects.FieldEffectManager_AddBillboardWithResourceID(fieldEffectManager, sResourceIDs[kind], position);
        if (fogDisabled == 1)
        {
            G3d.SetMaterialParam(Billboards.Billboard_GetModel(billboard)!, MaterialPolygonAttr, PolygonAttrFogEnable, 0);
        }
        return billboard;
    }

    // OFFSET 0x021F5C4C
    public static int EmoteIconEffect_IsFinished(OverworldAnimManager animManager)
    {
        EmoteIconEffectContext context = (EmoteIconEffectContext)OverworldAnimManagers.OverworldAnimManager_GetContext(animManager)!;
        return context.Finished;
    }

    // OFFSET 0x021F5C58
    public static void EmoteIconEffect_Hide(OverworldAnimManager animManager)
    {
        EmoteIconEffectContext context = (EmoteIconEffectContext)OverworldAnimManagers.OverworldAnimManager_GetContext(animManager)!;
        context.Hidden = 1;
        if (context.Billboard != null)
        {
            Billboards.Billboard_SetDrawFlag(context.Billboard, 0);
        }
    }

    // OFFSET 0x021F5C70
    private static int Static_Init(OverworldAnimManager animManager, object? contextObject)
    {
        EmoteIconEffectContext context = (EmoteIconEffectContext)contextObject!;
        EmoteIconEffectParams effectParams = (EmoteIconEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.Kind = effectParams.Kind;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.MapObject = effectParams.MapObject;
        context.Velocity = InitialVelocity;
        Renderer_LoadIfUnused(context.Renderer!, context.Kind);
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        context.Billboard = AddBillboard(context.FieldEffectManager!, context.Kind, position, context.Renderer!.FogDisabled);
        Renderer_AddRef(context.Renderer, context.Kind);
        if (OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager) == 1)
        {
            SoundPlayback.Sound_PlayEffect(SoundEffect);
        }
        return 1;
    }

    // OFFSET 0x021F5CD4
    private static void Static_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        EmoteIconEffectContext context = (EmoteIconEffectContext)contextObject!;
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        switch (context.State)
        {
            case 0:
                context.Offset.Y += context.Velocity;
                if (context.Offset.Y != 0)
                {
                    context.Velocity -= Gravity;
                }
                else
                {
                    context.Velocity = 0;
                    context.State++;
                }
                break;
            case 1:
                context.Timer++;
                if (context.Timer >= HoldFrames)
                {
                    context.State++;
                    context.Timer = 0;
                    context.Finished = 1;
                }
                break;
        }
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
    }

    // OFFSET 0x021F5D38
    private static void Effect_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        EmoteIconEffectContext context = (EmoteIconEffectContext)contextObject!;
        Billboards.Billboard_Delete(context.Billboard);
        Renderer_Release(context.Renderer!, context.Kind);
        Renderer_UnloadIfUnused(context.Renderer!, context.Kind);
    }

    // OFFSET 0x021F5D54
    private static void Effect_Render(OverworldAnimManager animManager, object? contextObject)
    {
        EmoteIconEffectContext context = (EmoteIconEffectContext)contextObject!;
        if (context.Hidden != 1)
        {
            OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
            position.X += context.Offset.X;
            position.Y += context.Offset.Y;
            position.Z += context.Offset.Z;
            Billboards.Billboard_SetPos(context.Billboard, position);
        }
    }

    // OFFSET 0x021F5D8C
    public static OverworldAnimManager? EmoteIconEffect_New(MapObject mapObject, int kind, int playSound, int delayed)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        EmoteIconEffectParams effectParams = new();
        effectParams.Kind = kind;
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.Renderer = (EmoteIconRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, RendererID);
        effectParams.MapObject = mapObject;
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        int priority = MapObject.MapObject_GetTaskBasePriority(mapObject) + 1;
        OverworldAnimManagerFuncs funcs;
        if (delayed == 1)
        {
            funcs = sDelayedFuncs;
        }
        else
        {
            funcs = sFollowFuncs;
        }
        return FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, funcs, position, playSound, effectParams, priority);
    }

    // OFFSET 0x021F5DE4
    private static int Follow_Init(OverworldAnimManager animManager, object? contextObject)
    {
        EmoteIconEffectContext context = (EmoteIconEffectContext)contextObject!;
        EmoteIconEffectParams effectParams = (EmoteIconEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.Kind = effectParams.Kind;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.MapObject = effectParams.MapObject;
        context.LocalID = MapObject.MapObject_GetLocalID(context.MapObject!);
        context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(context.MapObject!);
        context.Velocity = InitialVelocity;
        Renderer_LoadIfUnused(context.Renderer!, context.Kind);
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        context.Billboard = AddBillboard(context.FieldEffectManager!, context.Kind, position, context.Renderer!.FogDisabled);
        Renderer_AddRef(context.Renderer, context.Kind);
        if (OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager) == 1)
        {
            SoundPlayback.Sound_PlayEffect(SoundEffect);
        }
        return 1;
    }

    // OFFSET 0x021F5E58
    private static void Follow_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        EmoteIconEffectContext context = (EmoteIconEffectContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        if (MapObject.MapObject_IsLocalObjectOfMap(mapObject, context.LocalID, context.MapHeaderID) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObjectRender.MapObject_GetSpritePos(mapObject, out VecFx32 position);
        position.Y += HeightAboveSprite;
        position.Z += DepthOffset;
        switch (context.State)
        {
            case 0:
                context.Offset.Y += context.Velocity;
                if (context.Offset.Y != 0)
                {
                    context.Velocity -= Gravity;
                }
                else
                {
                    context.Velocity = 0;
                    context.State++;
                }
                break;
            case 1:
                context.Timer++;
                if (context.Timer >= HoldFrames)
                {
                    context.State++;
                    context.Timer = 0;
                    context.Finished = 1;
                }
                break;
        }
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
    }

    // OFFSET 0x021F5EE0
    private static int Delayed_Init(OverworldAnimManager animManager, object? contextObject)
    {
        EmoteIconEffectContext context = (EmoteIconEffectContext)contextObject!;
        EmoteIconEffectParams effectParams = (EmoteIconEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.Kind = effectParams.Kind;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.MapObject = effectParams.MapObject;
        context.LocalID = MapObject.MapObject_GetLocalID(context.MapObject!);
        context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(context.MapObject!);
        context.Velocity = InitialVelocity;
        context.Hidden = 1;
        Renderer_LoadIfUnused(context.Renderer!, context.Kind);
        Renderer_AddRef(context.Renderer!, context.Kind);
        return 1;
    }

    // OFFSET 0x021F5F24
    private static void Delayed_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        EmoteIconEffectContext context = (EmoteIconEffectContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        if (MapObject.MapObject_IsLocalObjectOfMap(mapObject, context.LocalID, context.MapHeaderID) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        MapObjectRender.MapObject_GetSpritePos(mapObject, out VecFx32 position);
        position.Y += HeightAboveSprite;
        position.Z += DepthOffset;
        switch (context.State)
        {
            case 0:
            case 1:
                context.State++;
                break;
            case 2:
                context.Billboard = AddBillboard(context.FieldEffectManager!, context.Kind, position, context.Renderer!.FogDisabled);
                Billboards.Billboard_SetDrawFlag(context.Billboard, 0);
                context.State++;
                break;
            case 3:
                Billboards.Billboard_SetDrawFlag(context.Billboard, 1);
                context.Hidden = 0;
                if (OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager) == 1)
                {
                    SoundPlayback.Sound_PlayEffect(SoundEffect);
                }
                context.State++;
                goto case 4;
            case 4:
                context.Offset.Y += context.Velocity;
                if (context.Offset.Y != 0)
                {
                    context.Velocity -= Gravity;
                }
                else
                {
                    context.Velocity = 0;
                    context.State++;
                }
                break;
            case 5:
                context.Timer++;
                if (context.Timer >= HoldFrames)
                {
                    context.State++;
                    context.Timer = 0;
                    context.Finished = 1;
                }
                break;
            case 6:
                break;
        }
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
    }
}
