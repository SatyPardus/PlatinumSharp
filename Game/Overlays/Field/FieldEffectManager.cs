using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate object? FieldEffectRendererInitFunc(FieldEffectManager fieldEffectManager);

public delegate void FieldEffectRendererFreeFunc(object? context);

public sealed class FieldEffectRendererFuncs
{
    public const int Size = 0xC;

    public readonly uint ID;
    public readonly FieldEffectRendererInitFunc Init;
    public readonly FieldEffectRendererFreeFunc Free;

    public FieldEffectRendererFuncs(uint id, FieldEffectRendererInitFunc init, FieldEffectRendererFreeFunc free)
    {
        ID = id;
        Init = init;
        Free = free;
    }
}

public sealed class FieldEffectRenderer
{
    public const int Size = 0x8;

    public uint ID;
    public object? Context;
}

public sealed class FieldEffectBillboardSlot
{
    public const int Size = 0x8;

    public uint ID;
    public BillboardResources Resources = new();
}

public sealed class FieldEffectBillboardManager
{
    public const int HeapIDOffset = 0x00;
    public const int ResourceCapacityOffset = 0x04;
    public const int ModelCapacityOffset = 0x06;
    public const int AnimCapacityOffset = 0x08;
    public const int TextureCapacityOffset = 0x0A;
    public const int BillboardListOffset = 0x0C;
    public const int ModelHeapOffset = 0x10;
    public const int AnimHeapOffset = 0x14;
    public const int TextureManagerOffset = 0x18;
    public const int SlotsOffset = 0x1C;
    public const int ResourcesOffset = 0x20;
    public const int Size = 0x24;

    public int HeapID;
    public ushort ResourceCapacity;
    public ushort ModelCapacity;
    public ushort AnimCapacity;
    public ushort TextureCapacity;
    public BillboardList? BillboardList;
    public ResourceHeap? ModelHeap;
    public ResourceHeap? AnimHeap;
    public TextureResourceManager? TextureManager;
    public FieldEffectBillboardSlot[]? Slots;
}

public sealed class FieldEffectTextureUpload
{
    public const int Size = 0xC;

    public int State;
    public uint TextureID;
    public TextureResourceManager? TextureManager;
}

public sealed class FieldEffectManager
{
    public const int HeapIDOffset = 0x00;
    public const int RendererCountOffset = 0x04;
    public const int AnimManagerCountOffset = 0x08;
    public const int FieldSystemOffset = 0x10;
    public const int RenderersOffset = 0x14;
    public const int NarcOffset = 0x18;
    public const int AnimManagerListOffset = 0x1C;
    public const int BillboardManagerOffset = 0x20;
    public const int Size = 0x24;

    public int HeapID;
    public uint RendererCount;
    public uint AnimManagerCount;
    public FieldSystem? FieldSystem;
    public FieldEffectRenderer[]? Renderers;
    public NARC? Narc;
    public OverworldAnimManagerList? AnimManagerList;
    public FieldEffectBillboardManager? BillboardManager;
}

public static class FieldEffects
{
    public const uint RendererNone = 0x22;

    private const int Overlay = 5;
    private const int NarcFieldEffects = 0x6B;
    private const uint BillboardSlotFree = 0x11;
    private const int HeapIDBillboardModels = 0x45;
    private const int HeapIDBillboardAnims = 0x46;
    private const uint TextureUploadPriority = 0xFF;
    private const int TextureUploadPending = 1;
    private const int ResourceWithoutTextureKeys = 1;
    private const int BillboardScale = 0x1000;

    private static readonly FieldEffectRendererFuncs[] sRendererFuncs =
    {
        new(0, ShadowEffect.Renderer_New, ShadowEffect.Renderer_Free),
        new(1, ReflectionEffect.Renderer_New, ReflectionEffect.Renderer_Free),
        new(2, ov5_021F4DAC, ov5_021F4DC8),
        new(3, ov5_021F4E08, ov5_021F4E28),
        new(4, ov5_021F5060, ov5_021F507C),
        new(5, ov5_021F50BC, ov5_021F50D8),
        new(6, ov5_021F5284, ov5_021F52A0),
        new(7, ov5_021F5428, ov5_021F5444),
        new(8, ov5_021F55CC, ov5_021F55E8),
        new(9, ov5_021F575C, ov5_021F5778),
        new(10, ov5_021F5894, ov5_021F58B0),
        new(11, FootprintEffect.FootprintRenderer_New, FootprintEffect.FootprintRenderer_Free),
        new(12, FootprintEffect.TerrainModelRenderer_New, FootprintEffect.TerrainModelRenderer_Free),
        new(13, AttachedAnimModelEffect.Renderer_New, AttachedAnimModelEffect.Renderer_Free),
        new(14, WarpArrowEffect.Renderer_New, WarpArrowEffect.Renderer_Free),
        new(15, SurfMount.SurfMountRenderer_New, SurfMount.SurfMountRenderer_Free),
        new(16, DirectionalMountEffect.Renderer_New, DirectionalMountEffect.Renderer_Free),
        new(17, RippleEffect.Renderer_New, RippleEffect.Renderer_Free),
        new(18, SplashEffect.Renderer_New, SplashEffect.Renderer_Free),
        new(19, GrassEffect.Renderer_New, GrassEffect.Renderer_Free),
        new(20, EmoteIconEffect.Renderer_New, EmoteIconEffect.Renderer_Free),
        new(21, FollowingBillboardEffect.Renderer_New, FollowingBillboardEffect.Renderer_Free),
        new(22, CarriedIconEffect.Renderer_New, CarriedIconEffect.Renderer_Free),
        new(23, JumpDustEffect.Renderer_New, JumpDustEffect.Renderer_Free),
        new(24, LongGrassEffect.Renderer_New, LongGrassEffect.Renderer_Free),
        new(25, ShallowCoverEffect.Renderer_New, ShallowCoverEffect.Renderer_Free),
        new(26, TileModelEffect.Renderer_New, TileModelEffect.Renderer_Free),
        new(27, TilePuffEffect.Renderer_New, TilePuffEffect.Renderer_Free),
        new(28, FootprintEffect.SpriteBurstRenderer_New, FootprintEffect.SpriteBurstRenderer_Free),
        new(29, MapObjectModelEffect.Renderer_New, MapObjectModelEffect.Renderer_Free),
        new(30, ov5_021F4018, ov5_021F4034),
        new(31, ov5_021F428C, ov5_021F42A8),
        new(32, ov5_021F47B0, ov5_021F47CC),
        new(33, ov5_021F8560, ov5_021F857C),
    };

    // OFFSET 0x021DF440
    public static FieldEffectManager FieldEffectManager_New(FieldSystem fieldSystem, int rendererCount, int heapID)
    {
        FieldEffectManager fieldEffectManager = new();
        fieldEffectManager.HeapID = heapID;
        fieldEffectManager.RendererCount = (uint)rendererCount;
        fieldEffectManager.FieldSystem = fieldSystem;
        fieldEffectManager.Renderers = new FieldEffectRenderer[rendererCount];
        for (int i = 0; i < rendererCount; i++)
        {
            fieldEffectManager.Renderers[i] = new FieldEffectRenderer();
        }
        ResetAllRenderers(fieldEffectManager);
        OpenArchive(fieldEffectManager);
        return fieldEffectManager;
    }

    // OFFSET 0x021DF47C
    public static void FieldEffectManager_InitAnimManagerList(object? fieldEffectManagerObject, int animManagerCount)
    {
        FieldEffectManager fieldEffectManager = (FieldEffectManager)fieldEffectManagerObject!;
        fieldEffectManager.AnimManagerCount = (uint)animManagerCount;
        InitAnimManagerList(fieldEffectManager);
    }

    // OFFSET 0x021DF488
    public static void FieldEffectManager_InitBillboardManager(object? fieldEffectManagerObject, int heapID, int resourceCapacity, int modelCapacity, int animCapacity, int textureCapacity, int modelHeapSize, int animHeapSize, int unused)
    {
        InitBillboardManager((FieldEffectManager)fieldEffectManagerObject!, heapID, (uint)resourceCapacity, (uint)modelCapacity, (uint)animCapacity, (uint)textureCapacity, (uint)modelHeapSize, (uint)animHeapSize, (uint)unused);
    }

    // OFFSET 0x021DF4A8
    public static void FieldEffectManager_InitRenderer(FieldEffectManager fieldEffectManager, uint id)
    {
        if (GetRenderer(fieldEffectManager, id) != null)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        InitRenderer(fieldEffectManager, id);
    }

    // OFFSET 0x021DF4C8
    public static void FieldEffectManager_InitRenderers(object? fieldEffectManagerObject, uint rendererListAddress)
    {
        FieldEffectManager fieldEffectManager = (FieldEffectManager)fieldEffectManagerObject!;
        uint id = RomData.OverlayU32(Overlay, rendererListAddress, 1)[0];
        while (id != RendererNone)
        {
            FieldEffectManager_InitRenderer(fieldEffectManager, id);
            rendererListAddress += 4;
            id = RomData.OverlayU32(Overlay, rendererListAddress, 1)[0];
        }
    }

    // OFFSET 0x021DF4E4
    public static bool FieldEffectManager_HasRenderer(FieldEffectManager fieldEffectManager, uint id)
    {
        if (GetRenderer(fieldEffectManager, id) != null)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021DF4F8
    public static void FieldEffectManager_Render(object? fieldEffectManagerObject)
    {
        RenderFieldEffects((FieldEffectManager)fieldEffectManagerObject!);
    }

    // OFFSET 0x021DF500
    public static void FieldEffectManager_Free(object? fieldEffectManagerObject)
    {
        FieldEffectManager fieldEffectManager = (FieldEffectManager)fieldEffectManagerObject!;
        FinishAndFreeAnimations(fieldEffectManager);
        FreeRenderers(fieldEffectManager);
        FreeBillboardManager(fieldEffectManager);
        CloseArchive(fieldEffectManager);
        fieldEffectManager.Renderers = null;
    }

    // OFFSET 0x021DF528
    public static byte[]? FieldEffectManager_HeapAlloc(FieldEffectManager fieldEffectManager, uint size, int allocAtEnd)
    {
        if (allocAtEnd == 0)
        {
            return Heap.Heap_Alloc(fieldEffectManager.HeapID, size);
        }
        return Heap.Heap_AllocAtEnd(fieldEffectManager.HeapID, size);
    }

    // OFFSET 0x021DF53C
    public static byte[]? FieldEffectManager_HeapAllocInit(FieldEffectManager fieldEffectManager, uint size, int allocAtEnd, uint initValue)
    {
        byte[]? data = FieldEffectManager_HeapAlloc(fieldEffectManager, size, allocAtEnd);
        Array.Fill(data!, (byte)initValue, 0, (int)size);
        return data;
    }

    // OFFSET 0x021DF554
    public static void FieldEffectManager_HeapFree(byte[] data)
    {
        Heap.Heap_Free(data);
    }

    // OFFSET 0x021DF55C
    public static object? FieldEffectManager_GetRendererContext(FieldEffectManager fieldEffectManager, uint id)
    {
        FieldEffectRenderer? renderer = GetRenderer(fieldEffectManager, id);
        if (renderer == null)
        {
            ErrorHandling.GF_AssertFail();
            return null;
        }
        return renderer.Context;
    }

    // OFFSET 0x021DF574
    public static FieldSystem? FieldEffectManager_GetFieldSystem(FieldEffectManager fieldEffectManager)
    {
        return fieldEffectManager.FieldSystem;
    }

    // OFFSET 0x021DF578
    public static FieldEffectManager? MapObject_GetFieldEffectManager(MapObject mapObject)
    {
        return (FieldEffectManager?)MapObject.MapObject_FieldSystem(mapObject)!.FieldEffectManager;
    }

    // OFFSET 0x021DF584
    private static int GetHeapID(FieldEffectManager fieldEffectManager)
    {
        return fieldEffectManager.HeapID;
    }

    // OFFSET 0x021DF588
    private static void OpenArchive(FieldEffectManager fieldEffectManager)
    {
        fieldEffectManager.Narc = NARC.NARC_ctor(NarcFieldEffects, GetHeapID(fieldEffectManager));
    }

    // OFFSET 0x021DF59C
    private static void CloseArchive(FieldEffectManager fieldEffectManager)
    {
        NARC.NARC_dtor(fieldEffectManager.Narc!);
    }

    // OFFSET 0x021DF5A8
    public static uint FieldEffectManager_GetNARCMemberSize(FieldEffectManager fieldEffectManager, uint memberIndex)
    {
        return NARC.NARC_GetMemberSize(fieldEffectManager.Narc!, memberIndex);
    }

    // OFFSET 0x021DF5B4
    public static void FieldEffectManager_ReadNARCWholeMember(FieldEffectManager fieldEffectManager, uint memberIndex, byte[] dest)
    {
        NARC.NARC_ReadWholeMember(fieldEffectManager.Narc!, memberIndex, dest);
    }

    // OFFSET 0x021DF5C0
    public static byte[] FieldEffectManager_AllocAndReadNARCWholeMember(FieldEffectManager fieldEffectManager, uint memberIndex, int allocAtEnd)
    {
        uint size = NARC.NARC_GetMemberSize(fieldEffectManager.Narc!, memberIndex);
        byte[] data = FieldEffectManager_HeapAlloc(fieldEffectManager, size, allocAtEnd)!;
        NARC.NARC_ReadWholeMember(fieldEffectManager.Narc!, memberIndex, data);
        return data;
    }

    // OFFSET 0x021DF5E8
    private static void ResetAllRenderers(FieldEffectManager fieldEffectManager)
    {
        for (uint i = 0; i < fieldEffectManager.RendererCount; i++)
        {
            FieldEffectRenderer_Reset(fieldEffectManager.Renderers![i]);
        }
    }

    // OFFSET 0x021DF600
    private static void InitRenderer(FieldEffectManager fieldEffectManager, uint id)
    {
        FieldEffectRendererFuncs funcs = GetRendererFuncs(id)!;
        object? context = funcs.Init(fieldEffectManager);
        FieldEffectRenderer renderer = FindFreeRenderer(fieldEffectManager)!;
        FieldEffectRenderer_Init(renderer, id, context);
    }

    // OFFSET 0x021DF628
    private static void FieldEffectRenderer_Free(FieldEffectManager fieldEffectManager, FieldEffectRenderer renderer)
    {
        FieldEffectRendererFuncs funcs = GetRendererFuncs(renderer.ID)!;
        funcs.Free(renderer.Context);
        FieldEffectRenderer_Reset(renderer);
    }

    // OFFSET 0x021DF644
    private static void FreeRenderers(FieldEffectManager fieldEffectManager)
    {
        for (uint i = 0; i < fieldEffectManager.RendererCount; i++)
        {
            FieldEffectRenderer renderer = fieldEffectManager.Renderers![i];
            if (!FieldEffectRenderer_IsInvalid(renderer))
            {
                FieldEffectRenderer_Free(fieldEffectManager, renderer);
            }
        }
    }

    // OFFSET 0x021DF66C
    private static FieldEffectRenderer? FindFreeRenderer(FieldEffectManager fieldEffectManager)
    {
        for (uint i = 0; i < fieldEffectManager.RendererCount; i++)
        {
            FieldEffectRenderer renderer = fieldEffectManager.Renderers![i];
            if (FieldEffectRenderer_IsInvalid(renderer))
            {
                return renderer;
            }
        }
        ErrorHandling.GF_AssertFail();
        return null;
    }

    // OFFSET 0x021DF694
    private static FieldEffectRenderer? GetRenderer(FieldEffectManager fieldEffectManager, uint id)
    {
        for (uint i = 0; i < fieldEffectManager.RendererCount; i++)
        {
            FieldEffectRenderer renderer = fieldEffectManager.Renderers![i];
            if (renderer.ID == id)
            {
                return renderer;
            }
        }
        return null;
    }

    // OFFSET 0x021DF6AC
    private static void FieldEffectRenderer_Reset(FieldEffectRenderer renderer)
    {
        renderer.ID = RendererNone;
        renderer.Context = null;
    }

    // OFFSET 0x021DF6B8
    private static void FieldEffectRenderer_Init(FieldEffectRenderer renderer, uint id, object? context)
    {
        renderer.ID = id;
        renderer.Context = context;
    }

    // OFFSET 0x021DF6C0
    private static bool FieldEffectRenderer_IsInvalid(FieldEffectRenderer renderer)
    {
        if (renderer.ID == RendererNone)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021DF6D0
    private static FieldEffectRendererFuncs? GetRendererFuncs(uint id)
    {
        for (int i = 0; i < sRendererFuncs.Length; i++)
        {
            if (sRendererFuncs[i].ID == id)
            {
                return sRendererFuncs[i];
            }
        }
        ErrorHandling.GF_AssertFail();
        return null;
    }

    // OFFSET 0x021DF6F8
    private static void InitAnimManagerList(FieldEffectManager fieldEffectManager)
    {
        fieldEffectManager.AnimManagerList = OverworldAnimManagers.OverworldAnimManagerList_New(fieldEffectManager.HeapID, (int)fieldEffectManager.AnimManagerCount);
    }

    // OFFSET 0x021DF708
    private static void FinishAndFreeAnimations(FieldEffectManager fieldEffectManager)
    {
        if (fieldEffectManager.AnimManagerList != null)
        {
            OverworldAnimManagers.OverworldAnimManagerList_FinishAllAndFree(fieldEffectManager.AnimManagerList);
            fieldEffectManager.AnimManagerList = null;
        }
    }

    // OFFSET 0x021DF71C
    private static void RenderFieldEffects(FieldEffectManager fieldEffectManager)
    {
        if (fieldEffectManager.AnimManagerList != null)
        {
            OverworldAnimManagers.OverworldAnimManagerList_Render(fieldEffectManager.AnimManagerList);
        }
    }

    // OFFSET 0x021DF72C
    public static OverworldAnimManager? FieldEffectManager_InitAnimManager(FieldEffectManager fieldEffectManager, OverworldAnimManagerFuncs funcs, in VecFx32 initialPosition, int userInt, object? userData, int taskPriority)
    {
        OverworldAnimManager? animManager = OverworldAnimManagers.OverworldAnimManagerList_InitManager(fieldEffectManager.AnimManagerList!, funcs, initialPosition, userInt, userData, taskPriority);
        if (animManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return animManager;
    }

    // OFFSET 0x021DF74C
    public static void FieldEffectManager_FinishAnimManager(OverworldAnimManager? animManager)
    {
        OverworldAnimManagers.OverworldAnimManager_Finish(animManager);
    }

    // OFFSET 0x021DF754
    private static void InitBillboardManager(FieldEffectManager fieldEffectManager, int heapID, uint resourceCapacity, uint modelCapacity, uint animCapacity, uint textureCapacity, uint modelHeapSize, uint animHeapSize, uint unused)
    {
        FieldEffectBillboardManager billboardManager = new();
        fieldEffectManager.BillboardManager = billboardManager;
        billboardManager.HeapID = heapID;
        billboardManager.ResourceCapacity = (ushort)resourceCapacity;
        billboardManager.ModelCapacity = (ushort)modelCapacity;
        billboardManager.AnimCapacity = (ushort)animCapacity;
        billboardManager.TextureCapacity = (ushort)textureCapacity;
        billboardManager.ModelHeap = ResourceHeap.ResourceHeap_New(heapID, HeapIDBillboardModels, modelHeapSize, modelCapacity);
        billboardManager.AnimHeap = ResourceHeap.ResourceHeap_New(heapID, HeapIDBillboardAnims, animHeapSize, animCapacity);
        billboardManager.TextureManager = TextureResourceManager.TextureResourceManager_New((int)textureCapacity, heapID);
        BillboardManager_InitSlots(fieldEffectManager, billboardManager, resourceCapacity);
        BillboardListParams listParams = new();
        listParams.MaxElements = (int)resourceCapacity;
        listParams.HeapID = GetHeapID(fieldEffectManager);
        billboardManager.BillboardList = Billboards.BillboardList_New(listParams);
    }

    // OFFSET 0x021DF7C4
    private static void FreeBillboardManager(FieldEffectManager fieldEffectManager)
    {
        FieldEffectBillboardManager? billboardManager = fieldEffectManager.BillboardManager;
        if (billboardManager != null)
        {
            Billboards.BillboardList_Delete(billboardManager.BillboardList);
            BillboardManager_FreeSlots(billboardManager);
            ResourceHeap.ResourceHeap_Free(billboardManager.ModelHeap!);
            ResourceHeap.ResourceHeap_Free(billboardManager.AnimHeap!);
            TextureResourceManager.TextureResourceManager_Delete(billboardManager.TextureManager);
            fieldEffectManager.BillboardManager = null;
        }
    }

    // OFFSET 0x021DF7F8
    public static Billboard? FieldEffectManager_AddBillboard(FieldEffectManager fieldEffectManager, BillboardResources? resources, VecFx32 position)
    {
        BillboardTemplate template = new();
        template.List = fieldEffectManager.BillboardManager!.BillboardList;
        template.Resources = resources;
        template.Pos = position;
        template.Scale = new VecFx32 { X = BillboardScale, Y = BillboardScale, Z = BillboardScale };
        Billboard? billboard = Billboards.BillboardList_Append(template);
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (billboard != null)
        {
            G3d.SetMaterialParam(Billboards.Billboard_GetModel(billboard), 0xC, 0x8000, 0x8000);
            AreaLight.AreaLight_UseGlobalModelAttributes(Billboards.Billboard_GetModel(billboard)!);
        }
        return billboard;
    }

    // OFFSET 0x021DF84C
    public static Billboard? FieldEffectManager_AddBillboardWithResourceID(FieldEffectManager fieldEffectManager, uint resourceID, VecFx32 position)
    {
        BillboardResources? resources = BillboardManager_GetResources(fieldEffectManager.BillboardManager!, resourceID);
        return FieldEffectManager_AddBillboard(fieldEffectManager, resources, position);
    }

    // OFFSET 0x021DF864
    public static BillboardResources? FieldEffectManager_InitBillboardResources(FieldEffectManager fieldEffectManager, uint resourceID, uint modelID, uint animID, uint textureID, int withoutTextureKeys, BillboardAnim[]? anims)
    {
        FieldEffectBillboardManager billboardManager = fieldEffectManager.BillboardManager!;
        byte[]? modelData = ResourceHeap.ResourceHeap_GetItemData(billboardManager.ModelHeap!, modelID);
        byte[]? animData = ResourceHeap.ResourceHeap_GetItemData(billboardManager.AnimHeap!, animID);
        BillboardGfxSequence gfxSequence = new();
        BillboardGfxSequence.BillboardGfxSequence_SetData(animData!, 0, gfxSequence);
        TextureResource? textureResource = TextureResourceManager.TextureResourceManager_FindTextureResource(billboardManager.TextureManager, (int)textureID);
        if (textureResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        G3dTexture? texture = TextureResourceManager.TextureResource_GetUnderlyingResource(textureResource);
        if (withoutTextureKeys == ResourceWithoutTextureKeys)
        {
            textureResource = null;
        }
        BillboardResources? resources = BillboardManager_SetResources(billboardManager, resourceID, modelData, gfxSequence, texture, textureResource, anims);
        if (resources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return resources;
    }

    // OFFSET 0x021DF8C8
    private static void BillboardManager_InitSlots(FieldEffectManager fieldEffectManager, FieldEffectBillboardManager billboardManager, uint count)
    {
        billboardManager.Slots = new FieldEffectBillboardSlot[count];
        for (uint i = 0; i < count; i++)
        {
            billboardManager.Slots[i] = new FieldEffectBillboardSlot();
            billboardManager.Slots[i].ID = BillboardSlotFree;
        }
    }

    // OFFSET 0x021DF8FC
    private static void BillboardManager_FreeSlots(FieldEffectBillboardManager billboardManager)
    {
        billboardManager.Slots = null;
    }

    // OFFSET 0x021DF910
    private static void BillboardManager_ReleaseSlot(FieldEffectBillboardManager billboardManager, uint resourceID)
    {
        for (int i = 0; i < billboardManager.ResourceCapacity; i++)
        {
            FieldEffectBillboardSlot slot = billboardManager.Slots![i];
            if (slot.ID == resourceID)
            {
                slot.ID = BillboardSlotFree;
                return;
            }
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x021DF930
    private static BillboardResources? BillboardManager_SetResources(FieldEffectBillboardManager billboardManager, uint resourceID, byte[]? modelData, BillboardGfxSequence gfxSequence, G3dTexture? texture, TextureResource? textureResource, BillboardAnim[]? anims)
    {
        for (int i = 0; i < billboardManager.ResourceCapacity; i++)
        {
            FieldEffectBillboardSlot slot = billboardManager.Slots![i];
            if (slot.ID == resourceID)
            {
                return slot.Resources;
            }
        }
        BillboardResources? resources = null;
        for (int i = 0; i < billboardManager.ResourceCapacity; i++)
        {
            FieldEffectBillboardSlot slot = billboardManager.Slots![i];
            if (slot.ID == BillboardSlotFree)
            {
                slot.ID = resourceID;
                resources = slot.Resources;
                break;
            }
        }
        if (resources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        resources!.CopyFrom(new BillboardResources());
        resources.ModelRes = modelData;
        Billboards.CopyGfxSequence(gfxSequence, resources.GfxSequence);
        resources.Texture = texture;
        if (textureResource != null)
        {
            resources.TexKey = TextureResourceManager.TextureResource_GetTexKey(textureResource);
            resources.Tex4x4Key = TextureResourceManager.TextureResource_GetTex4x4Key(textureResource);
            resources.PlttKey = TextureResourceManager.TextureResource_GetPlttKey(textureResource);
        }
        resources.Anims = anims;
        return resources;
    }

    // OFFSET 0x021DF9B4
    private static BillboardResources? BillboardManager_GetResources(FieldEffectBillboardManager billboardManager, uint resourceID)
    {
        for (int i = 0; i < billboardManager.ResourceCapacity; i++)
        {
            FieldEffectBillboardSlot slot = billboardManager.Slots![i];
            if (slot.ID == resourceID)
            {
                return slot.Resources;
            }
        }
        ErrorHandling.GF_AssertFail();
        return null;
    }

    // OFFSET 0x021DF9D4
    public static void FieldEffectManager_ReleaseBillboardResources(FieldEffectManager fieldEffectManager, uint resourceID)
    {
        BillboardManager_ReleaseSlot(fieldEffectManager.BillboardManager!, resourceID);
    }

    // OFFSET 0x021DF9E0
    public static void FieldEffectManager_LoadBillboardModel(FieldEffectManager fieldEffectManager, uint modelID, uint memberIndex)
    {
        ResourceHeap.ResourceHeap_LoadMemberFromNARC(fieldEffectManager.BillboardManager!.ModelHeap!, modelID, fieldEffectManager.Narc!, memberIndex, 0);
    }

    // OFFSET 0x021DF9FC
    public static byte[]? FieldEffectManager_GetBillboardModel(FieldEffectManager fieldEffectManager, uint modelID)
    {
        return ResourceHeap.ResourceHeap_GetItemData(fieldEffectManager.BillboardManager!.ModelHeap!, modelID);
    }

    // OFFSET 0x021DFA08
    public static void FieldEffectManager_FreeBillboardModel(FieldEffectManager fieldEffectManager, uint modelID)
    {
        ResourceHeap.ResourceHeap_FreeItem(fieldEffectManager.BillboardManager!.ModelHeap!, modelID);
    }

    // OFFSET 0x021DFA14
    public static void FieldEffectManager_LoadBillboardAnim(FieldEffectManager fieldEffectManager, uint animID, uint memberIndex)
    {
        ResourceHeap.ResourceHeap_LoadMemberFromNARC(fieldEffectManager.BillboardManager!.AnimHeap!, animID, fieldEffectManager.Narc!, memberIndex, 0);
    }

    // OFFSET 0x021DFA30
    public static void FieldEffectManager_FreeBillboardAnim(FieldEffectManager fieldEffectManager, uint animID)
    {
        ResourceHeap.ResourceHeap_FreeItem(fieldEffectManager.BillboardManager!.AnimHeap!, animID);
    }

    // OFFSET 0x021DFA3C
    public static void FieldEffectManager_LoadBillboardTexture(FieldEffectManager fieldEffectManager, uint textureID, uint memberIndex, int discardAfterUpload)
    {
        FieldEffectBillboardManager billboardManager = fieldEffectManager.BillboardManager!;
        byte[] data = FieldEffectManager_AllocAndReadNARCWholeMember(fieldEffectManager, memberIndex, 1);
        int heapID = GetHeapID(fieldEffectManager);
        if (TextureResourceManager.TextureResourceManager_AddTextureAndAllocVRAM(billboardManager.TextureManager, data, (int)textureID, discardAfterUpload, heapID) == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        UploadTextureDuringVBlank(fieldEffectManager, textureID, billboardManager.TextureManager);
    }

    // OFFSET 0x021DFA7C
    public static void FieldEffectManager_FreeBillboardTexture(FieldEffectManager fieldEffectManager, uint textureID)
    {
        TextureResourceManager.TextureResourceManager_RemoveTextureWithID(fieldEffectManager.BillboardManager!.TextureManager, (int)textureID);
    }

    // OFFSET 0x021DFA88
    private static void UploadTextureDuringVBlank(FieldEffectManager fieldEffectManager, uint textureID, TextureResourceManager? textureManager)
    {
        FieldEffectTextureUpload upload = new();
        upload.State = TextureUploadPending;
        upload.TextureID = textureID;
        upload.TextureManager = textureManager;
        SysTask.SysTask_ExecuteOnVBlank(UploadTextureTask, upload, TextureUploadPriority);
        SysTask.SysTask_ExecuteAfterVBlank(DiscardTextureDataTask, upload, TextureUploadPriority);
    }

    // OFFSET 0x021DFAC0
    private static void UploadTextureTask(SysTask task, object? param)
    {
        FieldEffectTextureUpload upload = (FieldEffectTextureUpload)param!;
        TextureResourceManager.TextureResourceManager_UploadTextureWithID(upload.TextureManager, (int)upload.TextureID);
        upload.State = TextureUploadPending;
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x021DFADC
    private static void DiscardTextureDataTask(SysTask task, object? param)
    {
        FieldEffectTextureUpload upload = (FieldEffectTextureUpload)param!;
        if (upload.State == TextureUploadPending)
        {
            TextureResourceManager.TextureResourceManager_DiscardTextureDataWithID(upload.TextureManager, (int)upload.TextureID);
            SysTask.SysTask_Done(task);
        }
    }

    // OFFSET 0x021DFB00
    public static void FieldEffectManager_LoadModel(FieldEffectManager fieldEffectManager, Simple3DModel model, int modelIndex, int memberIndex, int allocAtEnd)
    {
        Simple3D.Simple3DModel_LoadFromNARC(model, modelIndex, fieldEffectManager.Narc!, memberIndex, fieldEffectManager.HeapID, allocAtEnd);
        Simple3D.Simple3DModel_BindTextureOnVBlank(model);
    }

    // OFFSET 0x021DFB24
    public static void FieldEffectManager_LoadAnimation(FieldEffectManager fieldEffectManager, Simple3DAnimation animation, int unused, int memberIndex, int allocAtEnd)
    {
        Simple3D.Simple3DAnimation_LoadFromNARC(animation, unused, fieldEffectManager.Narc!, memberIndex, fieldEffectManager.HeapID, allocAtEnd);
    }

    // OFFSET 0x021DFB40
    public static void FieldEffectManager_ApplyAnimCopyToModel(FieldEffectManager fieldEffectManager, Simple3DAnimation destAnimation, Simple3DModel model, Simple3DAnimation sourceAnimation, int unused)
    {
        Simple3D.Simple3DAnimation_CopyAndBindToModel(destAnimation, model, sourceAnimation, unused, fieldEffectManager.HeapID);
    }
}
