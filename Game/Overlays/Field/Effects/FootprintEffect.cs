using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class FootprintRenderer
{
    public const int Size = 0x824;
    public const int ModelCount = 10;

    public FieldEffectManager? FieldEffectManager;
    public readonly Simple3DModel[] Models = new Simple3DModel[ModelCount];
    public readonly Simple3DModel[] AltModels = new Simple3DModel[ModelCount];
    public readonly G3dRenderObj[] RenderObjs = new G3dRenderObj[ModelCount];
    public readonly G3dRenderObj[] AltRenderObjs = new G3dRenderObj[ModelCount];

    public FootprintRenderer()
    {
        for (int i = 0; i < ModelCount; i++)
        {
            Models[i] = new Simple3DModel();
            AltModels[i] = new Simple3DModel();
            RenderObjs[i] = new G3dRenderObj();
            AltRenderObjs[i] = new G3dRenderObj();
        }
    }
}

public sealed class FootprintEffectParams
{
    public int FootprintType;
    public Simple3DModel? Model;
    public G3dRenderObj? RenderObj;
}

public sealed class FootprintEffectContext
{
    public const int Size = 0x1C;

    public int State;
    public int Timer;
    public int Hidden;
    public int Alpha;
    public int FootprintType;
    public Simple3DModel? Model;
    public G3dRenderObj? RenderObj;
}

public sealed class TerrainModelRenderer
{
    public const int Size = 0x13C;
    public const int ModelCount = 3;

    public FieldEffectManager? FieldEffectManager;
    public readonly G3dRenderObj[] RenderObjs = { new(), new(), new() };
    public readonly Simple3DModel[] Models = { new(), new(), new() };
}

public sealed class TerrainModelEffectParams
{
    public FieldEffectManager? FieldEffectManager;
    public TerrainModelRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class TerrainModelEffectContext
{
    public const int Size = 0x1C;

    public int LocalID;
    public int MapHeaderID;
    public int Hidden;
    public int ModelIndex;
    public FieldEffectManager? FieldEffectManager;
    public TerrainModelRenderer? Renderer;
    public MapObject? MapObject;
}

public sealed class SpriteBurstRenderer
{
    public const int Size = 0xC;

    public int RefCount;
    public int Loaded;
    public FieldEffectManager? FieldEffectManager;
}

public sealed class SpriteBurstEffectParams
{
    public FieldEffectManager? FieldEffectManager;
    public SpriteBurstRenderer? Renderer;
}

public sealed class SpriteBurstEffectContext
{
    public const int Size = 0x1C;

    public int Unused00;
    public int Step;
    public int StepFinished;
    public FieldEffectManager? FieldEffectManager;
    public SpriteBurstRenderer? Renderer;
    public Billboard? Billboard;
}

public static class FootprintEffect
{
    public const int FootprintTypeStraight = 0;
    public const int FootprintTypeTurn = 1;
    public const int FootprintTypeAltStraight = 2;
    public const int FootprintTypeAltTurn = 3;
    public const int FootprintTypeAlt = 4;

    private const uint FootprintRendererID = 0xB;
    private const uint TerrainModelRendererID = 0xC;
    private const uint SpriteBurstRendererID = 0x1C;
    private const int TaskPriorityOffset = 2;
    private const int DirectionCount = 4;
    private const int FootprintVisibleFrames = 0x10;
    private const int FootprintAlphaMax = 0x1F;
    private const int FootprintAlphaStep = 2;
    private const uint PolygonAlphaMask = 0x1F0000;
    private const int MaterialPolygonAttr = 0xC;
    private const uint StatusHidden = 0x200;
    private const uint SpriteBurstModelID = 0xB;
    private const uint SpriteBurstModelMember = 0x6D;
    private const uint SpriteBurstAnimID = 0xB;
    private const uint SpriteBurstAnimMember = 0xB5;
    private const uint SpriteBurstTextureID = 0xC;
    private const uint SpriteBurstTextureMember = 0xE;
    private const uint SpriteBurstResourceID = 0xD;
    private const int SpriteBurstTaskPriority = 0xFF;
    private const int SpriteBurstStepCount = 5;

    private static readonly int[] sModelMembers = { 0x3B, 0x3C, 0x3D, 0x3E, 0x3F, 0x40, 0x41, 0x42, 0x43, 0x44 };

    private static readonly int[] sAltModelMembers = { 0x5D, 0x5E, 0x5F, 0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66 };

    private static readonly int[] sTurnModelIndices =
    {
        4, 4, 7, 6,
        4, 4, 9, 8,
        8, 6, 5, 5,
        9, 7, 5, 5,
    };

    private static readonly int[] sTerrainModelMembers = { 0x4D, 0x4C, 0x4B };

    private static readonly int[] sSpriteBurstStepFrames = { 0x1000, 0x1000, 0x2000, 0x2000, 0x1000 };

    private static readonly BillboardAnim[] sSpriteBurstAnims =
    {
        new() { StartFrame = 0, EndFrame = 9, StopAtEnd = 1 },
        new() { StartFrame = 0, EndFrame = 0, StopAtEnd = 2 },
    };

    private static readonly OverworldAnimManagerFuncs sFootprintFuncs = new(FootprintEffectContext.Size, () => new FootprintEffectContext(), Footprint_Init, Footprint_Exit, Footprint_Tick, Footprint_Render);

    private static readonly OverworldAnimManagerFuncs sTerrainModelFuncs = new(TerrainModelEffectContext.Size, () => new TerrainModelEffectContext(), TerrainModel_Init, TerrainModel_Exit, TerrainModel_Tick, TerrainModel_Render);

    private static readonly OverworldAnimManagerFuncs sSpriteBurstFuncs = new(SpriteBurstEffectContext.Size, () => new SpriteBurstEffectContext(), SpriteBurst_Init, SpriteBurst_Exit, SpriteBurst_Tick, SpriteBurst_Render);

    // OFFSET 0x021F1CC8
    public static object? FootprintRenderer_New(FieldEffectManager fieldEffectManager)
    {
        FootprintRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        FootprintRenderer_LoadModels(renderer);
        return renderer;
    }

    // OFFSET 0x021F1CE8
    public static void FootprintRenderer_Free(object? context)
    {
        FootprintRenderer renderer = (FootprintRenderer)context!;
        FootprintRenderer_FreeModels(renderer);
    }

    // OFFSET 0x021F1CF8
    private static void FootprintRenderer_LoadModels(FootprintRenderer renderer)
    {
        for (uint i = 0; i < FootprintRenderer.ModelCount; i++)
        {
            FieldEffects.FieldEffectManager_LoadModel(renderer.FieldEffectManager!, renderer.Models[i], 0, sModelMembers[i], 0);
            Simple3D.Simple3DRenderObj_Init(renderer.RenderObjs[i], renderer.Models[i]);
            FieldEffects.FieldEffectManager_LoadModel(renderer.FieldEffectManager!, renderer.AltModels[i], 0, sAltModelMembers[i], 0);
            Simple3D.Simple3DRenderObj_Init(renderer.AltRenderObjs[i], renderer.AltModels[i]);
        }
    }

    // OFFSET 0x021F1D80
    private static void FootprintRenderer_FreeModels(FootprintRenderer renderer)
    {
        for (int i = 0; i < FootprintRenderer.ModelCount; i++)
        {
            Simple3D.Simple3DModel_Free(renderer.Models[i]);
            Simple3D.Simple3DModel_Free(renderer.AltModels[i]);
        }
    }

    // OFFSET 0x021F1DA4
    private static G3dRenderObj? FootprintRenderer_GetRenderObj(FootprintRenderer renderer, int footprintType, int direction, int previousDirection)
    {
        G3dRenderObj? renderObj = null;
        switch (footprintType)
        {
            case FootprintTypeStraight:
                renderObj = renderer.RenderObjs[direction];
                break;
            case FootprintTypeTurn:
                renderObj = renderer.RenderObjs[sTurnModelIndices[previousDirection * DirectionCount + direction]];
                break;
            case FootprintTypeAltStraight:
            case FootprintTypeAlt:
                renderObj = renderer.AltRenderObjs[direction];
                break;
            case FootprintTypeAltTurn:
                renderObj = renderer.AltRenderObjs[sTurnModelIndices[previousDirection * DirectionCount + direction]];
                break;
        }
        if (renderObj == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return renderObj;
    }

    // OFFSET 0x021F1E24
    private static Simple3DModel? FootprintRenderer_GetModel(FootprintRenderer renderer, int footprintType, int direction, int previousDirection)
    {
        Simple3DModel? model = null;
        switch (footprintType)
        {
            case FootprintTypeStraight:
                model = renderer.Models[direction];
                break;
            case FootprintTypeTurn:
                model = renderer.Models[sTurnModelIndices[previousDirection * DirectionCount + direction]];
                break;
            case FootprintTypeAltStraight:
            case FootprintTypeAlt:
                model = renderer.AltModels[direction];
                break;
            case FootprintTypeAltTurn:
                model = renderer.AltModels[sTurnModelIndices[previousDirection * DirectionCount + direction]];
                break;
        }
        if (model == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return model;
    }

    // OFFSET 0x021F1E8C
    public static OverworldAnimManager? FootprintEffect_NewStraight(MapObject mapObject)
    {
        return FootprintEffect_New(mapObject, FootprintTypeStraight);
    }

    // OFFSET 0x021F1E98
    public static OverworldAnimManager? FootprintEffect_NewTurn(MapObject mapObject)
    {
        return FootprintEffect_New(mapObject, FootprintTypeTurn);
    }

    // OFFSET 0x021F1EA4
    public static OverworldAnimManager? FootprintEffect_NewAltStraight(MapObject mapObject)
    {
        return FootprintEffect_New(mapObject, FootprintTypeAltStraight);
    }

    // OFFSET 0x021F1EB0
    public static OverworldAnimManager? FootprintEffect_NewAltTurn(MapObject mapObject)
    {
        return FootprintEffect_New(mapObject, FootprintTypeAltTurn);
    }

    // OFFSET 0x021F1EBC
    public static OverworldAnimManager? FootprintEffect_NewAlt(MapObject mapObject)
    {
        return FootprintEffect_New(mapObject, FootprintTypeAlt);
    }

    // OFFSET 0x021F1EC8
    public static OverworldAnimManager? FootprintEffect_NewNone(MapObject mapObject)
    {
        return null;
    }

    // OFFSET 0x021F1ECC
    private static OverworldAnimManager? FootprintEffect_New(MapObject mapObject, int footprintType)
    {
        int x = MapObject.MapObject_GetXPrev(mapObject);
        int z = MapObject.MapObject_GetZPrev(mapObject);
        int y = MapObject.MapObject_GetPosY(mapObject);
        int direction = MapObject.MapObject_GetFacingDir(mapObject);
        int previousDirection = MapObject.MapObject_GetPrevFacingDir(mapObject);
        int priority = MapObject.MapObject_CalculateTaskPriority(mapObject, TaskPriorityOffset);
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        FootprintRenderer renderer = (FootprintRenderer)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, FootprintRendererID)!;
        FootprintEffectParams effectParams = new();
        effectParams.FootprintType = footprintType;
        effectParams.RenderObj = FootprintRenderer_GetRenderObj(renderer, footprintType, direction, previousDirection);
        effectParams.Model = FootprintRenderer_GetModel(renderer, footprintType, direction, previousDirection);
        VecFx32 position = default;
        MapObjectMove.VecFx32_SetPosFromMapCoords(x, z, ref position);
        position.Y = y;
        switch (footprintType)
        {
            case 0:
            case 1:
            case 4:
            case 5:
                position.Y = y - 0x8000;
                if ((uint)(footprintType - 2) <= 1)
                {
                    position.Z -= 0x2000;
                }
                else
                {
                    position.Z += 0x1000;
                }
                break;
            case 2:
            case 3:
                position.Y = y - 0xA000;
                if ((uint)(footprintType - 2) <= 1)
                {
                    position.Z -= 0x2000;
                }
                else
                {
                    position.Z += 0x2000;
                }
                break;
        }
        return FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sFootprintFuncs, position, 0, effectParams, priority);
    }

    // OFFSET 0x021F1FB8
    private static int Footprint_Init(OverworldAnimManager animManager, object? contextObject)
    {
        FootprintEffectContext context = (FootprintEffectContext)contextObject!;
        context.FootprintType = OverworldAnimManagers.OverworldAnimManager_GetUserInt(animManager);
        FootprintEffectParams effectParams = (FootprintEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.RenderObj = effectParams.RenderObj;
        context.Model = effectParams.Model;
        context.Alpha = FootprintAlphaMax;
        context.FootprintType = effectParams.FootprintType;
        return 1;
    }

    // OFFSET 0x021F1FE0
    private static void Footprint_Exit(OverworldAnimManager animManager, object? contextObject)
    {
    }

    // OFFSET 0x021F1FE4
    private static void Footprint_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        FootprintEffectContext context = (FootprintEffectContext)contextObject!;
        switch (context.State)
        {
            case 0:
                context.Timer++;
                if (context.Timer >= FootprintVisibleFrames)
                {
                    context.State++;
                }
                break;
            case 1:
                context.Alpha -= FootprintAlphaStep;
                if (context.Alpha < 0)
                {
                    FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
                }
                break;
        }
    }

    // OFFSET 0x021F2014
    private static void Footprint_Render(OverworldAnimManager animManager, object? contextObject)
    {
        FootprintEffectContext context = (FootprintEffectContext)contextObject!;
        if (context.Hidden == 0)
        {
            OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
            G3d.SetMaterialPolygonAttrMask(context.Model!.Model!, true, PolygonAlphaMask);
            G3d.SetMaterialParam(context.Model.Model!, MaterialPolygonAttr, PolygonAlphaMask, (uint)context.Alpha << 16);
            Simple3D.Simple3DRenderObj_DrawAtPosition(context.RenderObj!, position);
        }
    }

    // OFFSET 0x021F204C
    public static object? TerrainModelRenderer_New(FieldEffectManager fieldEffectManager)
    {
        TerrainModelRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        TerrainModelRenderer_LoadModels(renderer);
        return renderer;
    }

    // OFFSET 0x021F2068
    public static void TerrainModelRenderer_Free(object? context)
    {
        TerrainModelRenderer renderer = (TerrainModelRenderer)context!;
        TerrainModelRenderer_FreeModels(renderer);
    }

    // OFFSET 0x021F2078
    private static void TerrainModelRenderer_LoadModels(TerrainModelRenderer renderer)
    {
        for (int i = 0; i < TerrainModelRenderer.ModelCount; i++)
        {
            FieldEffects.FieldEffectManager_LoadModel(renderer.FieldEffectManager!, renderer.Models[i], 0, sTerrainModelMembers[i], 0);
            Simple3D.Simple3DRenderObj_Init(renderer.RenderObjs[i], renderer.Models[i]);
        }
    }

    // OFFSET 0x021F20B8
    private static void TerrainModelRenderer_FreeModels(TerrainModelRenderer renderer)
    {
        for (int i = 0; i < TerrainModelRenderer.ModelCount; i++)
        {
            Simple3D.Simple3DModel_Free(renderer.Models[i]);
        }
    }

    // OFFSET 0x021F20D4
    public static OverworldAnimManager? TerrainModelEffect_New(MapObject mapObject)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        TerrainModelEffectParams effectParams = new();
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.Renderer = (TerrainModelRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, TerrainModelRendererID);
        effectParams.MapObject = mapObject;
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        int priority = MapObject.MapObject_GetTaskBasePriority(mapObject) + 1;
        return FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sTerrainModelFuncs, position, 0, effectParams, priority);
    }

    // OFFSET 0x021F2118
    private static int TerrainModel_Init(OverworldAnimManager animManager, object? contextObject)
    {
        TerrainModelEffectContext context = (TerrainModelEffectContext)contextObject!;
        TerrainModelEffectParams effectParams = (TerrainModelEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        context.MapObject = effectParams.MapObject;
        context.LocalID = MapObject.MapObject_GetLocalID(context.MapObject!);
        context.MapHeaderID = MapObject.MapObject_GetMapHeaderID(context.MapObject!);
        return 1;
    }

    // OFFSET 0x021F2144
    private static void TerrainModel_Exit(OverworldAnimManager animManager, object? contextObject)
    {
    }

    // OFFSET 0x021F2148
    private static void TerrainModel_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        TerrainModelEffectContext context = (TerrainModelEffectContext)contextObject!;
        MapObject mapObject = context.MapObject!;
        if (MapObject.MapObject_IsLocalObjectOfMap(mapObject, context.LocalID, context.MapHeaderID) == 0)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
            return;
        }
        context.Hidden = 0;
        if (MapObject.MapObject_CheckStatusFlag(mapObject, StatusHidden)
            || BerryPatchManager.BerryPatches_GetGrowthStage(MapObject.MapObject_FieldSystem(mapObject)!, mapObject) == 0)
        {
            context.Hidden = 1;
            return;
        }
        context.ModelIndex = BerryPatchManager.BerryPatches_GetMoisture(MapObject.MapObject_FieldSystem(mapObject)!, mapObject);
        MapObject.MapObject_CopyPos(mapObject, out VecFx32 position);
        OverworldAnimManagers.OverworldAnimManager_SetPosition(animManager, position);
    }

    // OFFSET 0x021F21B8
    private static void TerrainModel_Render(OverworldAnimManager animManager, object? contextObject)
    {
        TerrainModelEffectContext context = (TerrainModelEffectContext)contextObject!;
        if (context.Hidden != 1)
        {
            OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
            Simple3D.Simple3DRenderObj_DrawAtPosition(context.Renderer!.RenderObjs[context.ModelIndex], position);
        }
    }

    // OFFSET 0x021F21E0
    public static object? SpriteBurstRenderer_New(FieldEffectManager fieldEffectManager)
    {
        SpriteBurstRenderer renderer = new();
        renderer.FieldEffectManager = fieldEffectManager;
        return renderer;
    }

    // OFFSET 0x021F21F4
    public static void SpriteBurstRenderer_Free(object? context)
    {
        SpriteBurstRenderer renderer = (SpriteBurstRenderer)context!;
        SpriteBurstRenderer_Unload(renderer);
    }

    // OFFSET 0x021F2204
    private static void SpriteBurstRenderer_AddRef(SpriteBurstRenderer renderer)
    {
        renderer.RefCount++;
    }

    // OFFSET 0x021F220C
    private static void SpriteBurstRenderer_Release(SpriteBurstRenderer renderer)
    {
        renderer.RefCount--;
        if (renderer.RefCount < 0)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x021F221C
    private static void SpriteBurstRenderer_Load(SpriteBurstRenderer renderer)
    {
        if (renderer.Loaded == 0)
        {
            renderer.Loaded = 1;
            FieldEffects.FieldEffectManager_LoadBillboardModel(renderer.FieldEffectManager!, SpriteBurstModelID, SpriteBurstModelMember);
            FieldEffects.FieldEffectManager_LoadBillboardAnim(renderer.FieldEffectManager!, SpriteBurstAnimID, SpriteBurstAnimMember);
            FieldEffects.FieldEffectManager_LoadBillboardTexture(renderer.FieldEffectManager!, SpriteBurstTextureID, SpriteBurstTextureMember, 1);
            FieldEffects.FieldEffectManager_InitBillboardResources(renderer.FieldEffectManager!, SpriteBurstResourceID, SpriteBurstModelID, SpriteBurstAnimID, SpriteBurstTextureID, 0, sSpriteBurstAnims);
        }
    }

    // OFFSET 0x021F226C
    private static void SpriteBurstRenderer_Unload(SpriteBurstRenderer renderer)
    {
        if (renderer.Loaded == 1)
        {
            renderer.Loaded = 0;
            FieldEffects.FieldEffectManager_FreeBillboardModel(renderer.FieldEffectManager!, SpriteBurstModelID);
            FieldEffects.FieldEffectManager_FreeBillboardAnim(renderer.FieldEffectManager!, SpriteBurstAnimID);
            FieldEffects.FieldEffectManager_FreeBillboardTexture(renderer.FieldEffectManager!, SpriteBurstTextureID);
            FieldEffects.FieldEffectManager_ReleaseBillboardResources(renderer.FieldEffectManager!, SpriteBurstResourceID);
        }
    }

    // OFFSET 0x021F229C
    private static void SpriteBurstRenderer_LoadIfUnused(SpriteBurstRenderer renderer)
    {
        if (renderer.RefCount == 0)
        {
            SpriteBurstRenderer_Load(renderer);
        }
    }

    // OFFSET 0x021F22AC
    private static void SpriteBurstRenderer_UnloadIfUnused(SpriteBurstRenderer renderer)
    {
        if (renderer.RefCount == 0)
        {
            SpriteBurstRenderer_Unload(renderer);
        }
    }

    // OFFSET 0x021F22BC
    public static OverworldAnimManager? SpriteBurstEffect_New(MapObject mapObject)
    {
        FieldEffectManager fieldEffectManager = FieldEffects.MapObject_GetFieldEffectManager(mapObject)!;
        MapObjectRender.MapObject_GetSpritePos(mapObject, out VecFx32 position);
        position.Z += 0x8000;
        SpriteBurstEffectParams effectParams = new();
        effectParams.FieldEffectManager = fieldEffectManager;
        effectParams.Renderer = (SpriteBurstRenderer?)FieldEffects.FieldEffectManager_GetRendererContext(fieldEffectManager, SpriteBurstRendererID);
        return FieldEffects.FieldEffectManager_InitAnimManager(fieldEffectManager, sSpriteBurstFuncs, position, 0, effectParams, SpriteBurstTaskPriority);
    }

    // OFFSET 0x021F2304
    private static int SpriteBurst_Init(OverworldAnimManager animManager, object? contextObject)
    {
        SpriteBurstEffectContext context = (SpriteBurstEffectContext)contextObject!;
        SpriteBurstEffectParams effectParams = (SpriteBurstEffectParams)OverworldAnimManagers.OverworldAnimManager_GetUserData(animManager)!;
        context.FieldEffectManager = effectParams.FieldEffectManager;
        context.Renderer = effectParams.Renderer;
        SpriteBurstRenderer_LoadIfUnused(context.Renderer!);
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        context.Billboard = FieldEffects.FieldEffectManager_AddBillboardWithResourceID(context.FieldEffectManager!, SpriteBurstResourceID, position);
        SpriteBurstRenderer_AddRef(context.Renderer!);
        return 1;
    }

    // OFFSET 0x021F2344
    private static void SpriteBurst_Exit(OverworldAnimManager animManager, object? contextObject)
    {
        SpriteBurstEffectContext context = (SpriteBurstEffectContext)contextObject!;
        Billboards.Billboard_Delete(context.Billboard);
        SpriteBurstRenderer_Release(context.Renderer!);
        SpriteBurstRenderer_UnloadIfUnused(context.Renderer!);
    }

    // OFFSET 0x021F235C
    private static void SpriteBurst_Tick(OverworldAnimManager animManager, object? contextObject)
    {
        SpriteBurstEffectContext context = (SpriteBurstEffectContext)contextObject!;
        if (context.StepFinished == 1)
        {
            context.StepFinished = 0;
            context.Step++;
            if (context.Step >= SpriteBurstStepCount)
            {
                FieldEffects.FieldEffectManager_FinishAnimManager(animManager);
                return;
            }
            Billboards.Billboard_SetFrameNum(context.Billboard, 0);
        }
        if (Billboards.Billboard_AdvanceAnim(context.Billboard, sSpriteBurstStepFrames[context.Step]) == 1)
        {
            context.StepFinished = 1;
        }
    }

    // OFFSET 0x021F23B8
    private static void SpriteBurst_Render(OverworldAnimManager animManager, object? contextObject)
    {
        SpriteBurstEffectContext context = (SpriteBurstEffectContext)contextObject!;
        OverworldAnimManagers.OverworldAnimManager_GetPosition(animManager, out VecFx32 position);
        Billboards.Billboard_SetPos(context.Billboard, position);
    }
}
