using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public delegate void BillboardCallback(Billboard billboard, object? param);

public sealed class BillboardAnim
{
    public const int Size = 0xC;

    public int StartFrame;
    public int EndFrame;
    public int StopAtEnd;
}

public sealed class BillboardListParams
{
    public int MaxElements;
    public int HeapID;
}

public sealed class BillboardResources
{
    public byte[]? ModelRes;
    public G3dTexture? Texture;
    public BillboardAnim[]? Anims;
    public BillboardGfxSequence GfxSequence = new();
    public uint TexKey;
    public uint Tex4x4Key;
    public uint PlttKey;

    public void CopyFrom(BillboardResources source)
    {
        ModelRes = source.ModelRes;
        Texture = source.Texture;
        Anims = source.Anims;
        Billboards.CopyGfxSequence(source.GfxSequence, GfxSequence);
        TexKey = source.TexKey;
        Tex4x4Key = source.Tex4x4Key;
        PlttKey = source.PlttKey;
    }
}

public sealed class BillboardTemplate
{
    public BillboardList? List;
    public BillboardResources? Resources;
    public VecFx32 Pos;
    public VecFx32 Scale;
}

public sealed class Billboard
{
    public VecFx32 Pos;
    public VecFx32 Scale;
    public MtxFx33[]? RotMatrix;
    public object? CallbackParam;
    public BillboardCallback? Callback;
    public byte Draw;
    public BillboardList? List;
    public BillboardAnim[]? Anims;
    public G3dRenderObj RenderObj = new();
    public G3dModelSet? ModelSet;
    public G3dModel? Model;
    public G3dTexture? Texture;
    public G3dTexture? AnimTexture;
    public uint TexKey;
    public uint Tex4x4Key;
    public uint PlttKey;
    public BillboardGfxSequence GfxSequence = new();
    public BillboardVRAMTransfer? VramTransfer;
    public byte State;
    public ushort AnimNum;
    public int FrameNum;
    public Billboard? Next;
    public Billboard? Prev;
}

public sealed class BillboardList
{
    public byte Active;
    public byte Draw;
    public byte RedrawState;
    public Billboard[]? Billboards;
    public int MaxElements;
    public Billboard Sentinel = new();
    public Billboard?[]? FreeStack;
    public int FreeStackTop;
    public HeapAllocator? Allocator;
    public BillboardListVRAMTransfer? VramTransfer;
}

public static class Billboards
{
    public const int StateNone = 0;
    public const int StateInitializing = 1;
    public const int StateVramTransfer = 2;
    public const int StateSharedTexture = 3;

    private const int RedrawNone = 0;
    private const int RedrawRequested = 1;
    private const int RedrawDone = 2;
    private const int ResourceValueModel = 0;
    private const int ResourceValueTexture = 1;
    private const int ModelMaterialsOffset = 0x08;
    private const int MaterialDictOffset = 0x04;
    private const int MaterialTexImageParamOffset = 0x14;
    private const int MaterialPaletteBaseOffset = 0x1C;
    private const int TextureDictOffset = 0x3C;
    private const int TexturePaletteInfoFlagOffset = 0x20;
    private const int TexturePaletteDictOffset = 0x34;
    private const int Palette4ColorFlag = 0x8000;
    private const int DictBoundFlag = 1;
    private const uint TexImageParamKeepMask = 0xC00F0000;
    private const uint MaxTextureAddress = 0xFFFF;
    private const int MaxPaletteBase = 0x1FFF;

    private static BillboardList[]? sLists;
    private static int sListCount;

    public static void CopyGfxSequence(BillboardGfxSequence source, BillboardGfxSequence dest)
    {
        dest.FrameStarts = source.FrameStarts;
        dest.TextureIndices = source.TextureIndices;
        dest.PaletteIndices = source.PaletteIndices;
        dest.FrameCount = source.FrameCount;
    }

    private static void ReleaseModelSet(G3dModelSet modelSet)
    {
        byte[] file = modelSet.File;
        int count = G3dDict.GetEntryCount(file, modelSet.Offset + 8);
        for (int i = 0; i < count; i++)
        {
            G3dModel? model = G3d.GetModelByIndex(modelSet, i);
            if (model == null)
            {
                continue;
            }
            int materials = model.Offset + Bin.I32(file, model.Offset + ModelMaterialsOffset);
            int textureDict = materials + Bin.U16(file, materials);
            int textureCount = G3dDict.GetEntryCount(file, textureDict);
            for (int j = 0; j < textureCount; j++)
            {
                int entry = G3dDict.GetEntryByIndex(file, textureDict, j);
                if ((file[entry + 3] & DictBoundFlag) == 0)
                {
                    continue;
                }
                int indices = materials + Bin.U16(file, entry);
                for (int k = 0; k < file[entry + 2]; k++)
                {
                    int materialEntry = G3dDict.GetEntryByIndex(file, materials + MaterialDictOffset, file[indices + k]);
                    int material = materials + Bin.I32(file, materialEntry);
                    Bin.W32(file, material + MaterialTexImageParamOffset, Bin.U32(file, material + MaterialTexImageParamOffset) & TexImageParamKeepMask);
                }
                file[entry + 3] = (byte)(file[entry + 3] & ~DictBoundFlag);
            }
            int paletteDict = materials + Bin.U16(file, materials + 2);
            int paletteCount = G3dDict.GetEntryCount(file, paletteDict);
            for (int j = 0; j < paletteCount; j++)
            {
                int entry = G3dDict.GetEntryByIndex(file, paletteDict, j);
                file[entry + 3] = (byte)(file[entry + 3] & ~DictBoundFlag);
            }
        }
    }

    // OFFSET 0x02020AEC
    private static void BillboardList_Reset(BillboardList list)
    {
        list.Active = 0;
        list.Draw = 0;
        list.Billboards = null;
        list.MaxElements = 0;
        list.FreeStack = null;
        list.FreeStackTop = 0;
        list.Allocator = null;
        list.VramTransfer = null;
        list.RedrawState = RedrawNone;
    }

    // OFFSET 0x02020B14
    public static void Billboard_Reset(Billboard billboard)
    {
        billboard.List = null;
        billboard.Anims = null;
        billboard.ModelSet = null;
        billboard.Model = null;
        billboard.Texture = null;
        billboard.AnimTexture = null;
        billboard.GfxSequence = new BillboardGfxSequence();
        billboard.TexKey = 0;
        billboard.Tex4x4Key = 0;
        billboard.PlttKey = 0;
        billboard.VramTransfer = null;
        billboard.Pos.X = 0;
        billboard.Pos.Y = 0;
        billboard.Pos.Z = 0;
        billboard.Scale.X = Fx32.One;
        billboard.Scale.Y = Fx32.One;
        billboard.Scale.Z = Fx32.One;
        billboard.RotMatrix = null;
        billboard.AnimNum = 0;
        billboard.State = StateNone;
        billboard.Next = null;
        billboard.Prev = null;
        billboard.Draw = 0;
        billboard.Callback = null;
    }

    // OFFSET 0x02020B90
    public static void BillboardLists_Create(int count, int heapID)
    {
        if (sLists != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sLists = new BillboardList[count];
        sListCount = count;
        for (int i = 0; i < count; i++)
        {
            sLists[i] = new BillboardList();
            BillboardList_Reset(sLists[i]);
        }
    }

    // OFFSET 0x02020BD0
    public static void BillboardLists_Delete()
    {
        for (int i = 0; i < sListCount; i++)
        {
            BillboardList_Delete(sLists![i]);
        }
        sLists = null;
        sListCount = 0;
    }

    // OFFSET 0x02020C08
    public static void BillboardLists_Draw()
    {
        for (int i = 0; i < sListCount; i++)
        {
            BillboardList list = sLists![i];
            if (list.Draw == 1)
            {
                BillboardList_Draw(list);
            }
            if (list.RedrawState == RedrawRequested)
            {
                list.RedrawState = RedrawDone;
            }
        }
    }

    // OFFSET 0x02020C44
    public static BillboardList? BillboardList_New(BillboardListParams listParams)
    {
        BillboardList? list = FindFirstFreeBillboardList();
        if (list == null)
        {
            ErrorHandling.GF_AssertFail();
            return null;
        }
        list.Active = 1;
        list.Draw = 1;
        list.Billboards = new Billboard[listParams.MaxElements];
        for (int i = 0; i < listParams.MaxElements; i++)
        {
            list.Billboards[i] = new Billboard();
        }
        list.MaxElements = listParams.MaxElements;
        Billboard_Reset(list.Sentinel);
        list.Sentinel.Next = list.Sentinel;
        list.Sentinel.Prev = list.Sentinel;
        list.FreeStack = new Billboard?[listParams.MaxElements];
        BillboardList_InitBillboards(list);
        list.Allocator = new HeapAllocator();
        Heap.HeapExp_FndInitAllocator(list.Allocator, listParams.HeapID, 4);
        list.VramTransfer = BillboardVramTransfer.BillboardListVRAMTransfer_New(listParams.MaxElements, listParams.HeapID);
        return list;
    }

    // OFFSET 0x02020CCC
    public static bool BillboardList_Delete(BillboardList? list)
    {
        if (list == null)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        if (list.Active != 0)
        {
            BillboardList_DeleteAll(list);
            BillboardVramTransfer.BillboardListVRAMTransfer_Free(list.VramTransfer!);
            BillboardList_Reset(list);
        }
        return true;
    }

    // OFFSET 0x02020D14
    public static bool BillboardList_DeleteAll(BillboardList? list)
    {
        if (list == null)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        if (list.Active != 0)
        {
            Billboard billboard = list.Sentinel.Next!;
            while (billboard != list.Sentinel)
            {
                Billboard next = billboard.Next!;
                Billboard_Delete(billboard);
                billboard = next;
            }
        }
        return true;
    }

    // OFFSET 0x02020D50
    public static bool BillboardList_IsRedrawing(BillboardList? list)
    {
        if (list == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return list!.RedrawState != RedrawNone;
    }

    // OFFSET 0x02020D68
    public static void BillboardList_ResetRedraw(BillboardList list)
    {
        if (list.RedrawState == RedrawDone)
        {
            list.RedrawState = RedrawNone;
        }
    }

    // OFFSET 0x02020D74
    private static BillboardList? FindFirstFreeBillboardList()
    {
        for (int i = 0; i < sListCount; i++)
        {
            if (sLists![i].Active == 0)
            {
                return sLists[i];
            }
        }
        return null;
    }

    // OFFSET 0x02020DA8
    private static void BillboardList_Draw(BillboardList? list)
    {
        if (list == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        MtxFx33 identity = default;
        identity.M00 = Fx32.One;
        identity.M11 = Fx32.One;
        identity.M22 = Fx32.One;
        Billboard sentinel = list!.Sentinel;
        Billboard billboard = sentinel.Next!;
        while (billboard != sentinel)
        {
            if (billboard.Draw == 1)
            {
                if (billboard.Callback != null)
                {
                    billboard.Callback(billboard, billboard.CallbackParam);
                }
                Billboard_SetKeysAndBindModelSet(billboard);
                if (billboard.State == StateSharedTexture)
                {
                    UpdateTexPlttAddressesForCurrentFrame(billboard);
                }
                else if (billboard.State == StateVramTransfer)
                {
                    Billboard_TryRequestVRAMTransfer(billboard);
                }
                if (billboard.RotMatrix != null)
                {
                    Easy3D.Easy3D_DrawRenderObj(billboard.RenderObj, in billboard.Pos, in billboard.RotMatrix[0], in billboard.Scale);
                }
                else
                {
                    Easy3D.Easy3D_DrawRenderObj(billboard.RenderObj, in billboard.Pos, in identity, in billboard.Scale);
                }
                ReleaseVRAMKeysAndModelSet(billboard);
            }
            billboard = billboard.Next!;
        }
    }

    // OFFSET 0x02020E28
    private static void AllocResourcesAndAppendBillboard(Billboard billboard, BillboardResources resources)
    {
        BillboardList list = billboard.List!;
        Billboard_ResetVRAMTransfer(list, billboard);
        Billboard_AllocTextureAndSetModel(billboard, resources);
        Billboard_InitAnimAndRequestTransfer(list, billboard, resources);
        if (billboard.State == StateInitializing)
        {
            AppendBillboardToList(list.Sentinel, billboard);
        }
        billboard.State = StateVramTransfer;
        billboard.Anims = resources.Anims;
        billboard.AnimNum = 0;
        billboard.FrameNum = 0;
    }

    // OFFSET 0x02020E78
    private static void ResetResourcesAndAppendBillboard(Billboard billboard, BillboardResources resources)
    {
        BillboardList list = billboard.List!;
        Billboard_ResetVRAMTransfer(list, billboard);
        if (billboard.State == StateVramTransfer)
        {
            FreeVRAMKeys(ref billboard.TexKey, ref billboard.Tex4x4Key, ref billboard.PlttKey);
        }
        billboard.TexKey = resources.TexKey;
        billboard.Tex4x4Key = resources.Tex4x4Key;
        billboard.PlttKey = resources.PlttKey;
        SetBillboardModelSetAndAnimTexture(billboard, resources);
        SetBillboardGfxSeqAndResetVRAMTransfer(billboard, resources);
        if (billboard.State == StateInitializing)
        {
            AppendBillboardToList(list.Sentinel, billboard);
        }
        billboard.State = StateSharedTexture;
        billboard.Anims = resources.Anims;
        billboard.AnimNum = 0;
        billboard.FrameNum = 0;
    }

    // OFFSET 0x02020EF8
    private static void AllocTextureVRAM(G3dTexture texture, ref uint texKey, ref uint tex4x4Key, ref uint plttKey)
    {
        uint textureSize = G3d.GetTextureSize(texture);
        uint texture4x4Size = G3d.GetTexture4x4Size(texture);
        uint paletteSize = G3d.GetPaletteSize(texture);
        if (textureSize != 0)
        {
            texKey = Gfd.AllocTexVram(textureSize, 0, 0);
            if (texKey == Gfd.InvalidKey)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        else
        {
            texKey = Gfd.InvalidKey;
        }
        if (texture4x4Size != 0)
        {
            tex4x4Key = Gfd.AllocTexVram(texture4x4Size, 1, 0);
            if (tex4x4Key == Gfd.InvalidKey)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        else
        {
            tex4x4Key = Gfd.InvalidKey;
        }
        if (paletteSize != 0)
        {
            int is4Color = Bin.U16(texture.File, texture.Offset + TexturePaletteInfoFlagOffset) & Palette4ColorFlag;
            plttKey = Gfd.AllocPlttVram(paletteSize, is4Color, 0);
            if (plttKey == Gfd.InvalidKey)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        else
        {
            plttKey = Gfd.InvalidKey;
        }
    }

    // OFFSET 0x02020F90
    private static void ReleaseVRAMKeys(G3dTexture texture, out uint texKey, out uint tex4x4Key, out uint plttKey)
    {
        G3d.ReleaseTextureKeys(texture, out texKey, out tex4x4Key);
        plttKey = G3d.ReleasePaletteKey(texture);
    }

    // OFFSET 0x02020FA4
    private static void SetKeysAndBindModelSet(G3dTexture texture, G3dModelSet modelSet, uint texKey, uint tex4x4Key, uint plttKey)
    {
        G3d.SetTextureKeys(texture, texKey, tex4x4Key);
        G3d.SetPaletteKey(texture, plttKey);
        G3d.BindModelSet(modelSet, texture);
    }

    // OFFSET 0x02020FC8
    private static void FreeVRAMKeys(ref uint texKey, ref uint tex4x4Key, ref uint plttKey)
    {
        if (texKey != Gfd.InvalidKey)
        {
            Gfd.FreeTexVram(texKey);
        }
        if (tex4x4Key != Gfd.InvalidKey)
        {
            Gfd.FreeTexVram(tex4x4Key);
        }
        if (plttKey != Gfd.InvalidKey)
        {
            Gfd.FreePlttVram(plttKey);
        }
    }

    // OFFSET 0x02020FFC
    private static bool AreTexturesSameSize(G3dTexture? texture1, G3dTexture? texture2)
    {
        if (texture1 == null || texture2 == null)
        {
            return false;
        }
        uint textureSize1 = G3d.GetTextureSize(texture1);
        uint texture4x4Size1 = G3d.GetTexture4x4Size(texture1);
        uint paletteSize1 = G3d.GetPaletteSize(texture1);
        uint textureSize2 = G3d.GetTextureSize(texture2);
        uint texture4x4Size2 = G3d.GetTexture4x4Size(texture2);
        uint paletteSize2 = G3d.GetPaletteSize(texture2);
        if (textureSize1 != textureSize2 || texture4x4Size1 != texture4x4Size2 || paletteSize1 != paletteSize2)
        {
            return false;
        }
        return true;
    }

    // OFFSET 0x0202105C
    private static void Billboard_ResetVRAMTransfer(BillboardList list, Billboard billboard)
    {
        if (billboard.VramTransfer != null)
        {
            BillboardVramTransfer.BillboardVRAMTransfer_Reset(billboard.VramTransfer);
            billboard.VramTransfer = null;
        }
    }

    // OFFSET 0x02021078
    private static void Billboard_AllocTextureAndSetModel(Billboard billboard, BillboardResources resources)
    {
        G3dTexture? oldTexture = billboard.Texture;
        billboard.ModelSet = RetrieveBillboardResources(resources, out billboard.Model, out billboard.Texture, true);
        G3d.InitRenderObj(billboard.RenderObj, billboard.Model!);
        bool sameSize;
        if (billboard.State != StateSharedTexture)
        {
            sameSize = AreTexturesSameSize(oldTexture, billboard.Texture);
        }
        else
        {
            sameSize = false;
        }
        if (!sameSize)
        {
            if (billboard.State == StateVramTransfer)
            {
                FreeVRAMKeys(ref billboard.TexKey, ref billboard.Tex4x4Key, ref billboard.PlttKey);
            }
            AllocTextureVRAM(billboard.Texture!, ref billboard.TexKey, ref billboard.Tex4x4Key, ref billboard.PlttKey);
        }
    }

    // OFFSET 0x020210F4
    private static void Billboard_InitAnimAndRequestTransfer(BillboardList list, Billboard billboard, BillboardResources resources)
    {
        billboard.AnimTexture = BillboardResources_GetTexture(resources);
        CopyGfxSequence(resources.GfxSequence, billboard.GfxSequence);
        billboard.VramTransfer = BillboardVramTransfer.BillboardListVRAMTransfer_NewBillboardTransfer(list.VramTransfer!, billboard.GfxSequence, billboard.AnimTexture!, billboard.TexKey, billboard.PlttKey, billboard.FrameNum);
    }

    // OFFSET 0x02021148
    private static void SetBillboardModelSetAndAnimTexture(Billboard billboard, BillboardResources resources)
    {
        billboard.ModelSet = RetrieveBillboardResources(resources, out billboard.Model, out billboard.Texture, true);
        G3d.InitRenderObj(billboard.RenderObj, billboard.Model!);
        billboard.AnimTexture = BillboardResources_GetTexture(resources);
    }

    // OFFSET 0x0202117C
    private static void SetBillboardGfxSeqAndResetVRAMTransfer(Billboard billboard, BillboardResources resources)
    {
        CopyGfxSequence(resources.GfxSequence, billboard.GfxSequence);
        billboard.VramTransfer = null;
    }

    // OFFSET 0x0202119C
    public static Billboard? BillboardList_Append(BillboardTemplate template)
    {
        BillboardList? list = template.List;
        if (list == null)
        {
            return null;
        }
        Billboard? billboard = BillboardList_AllocBillboard(list);
        if (billboard == null)
        {
            return null;
        }
        billboard.List = list;
        billboard.Pos = template.Pos;
        billboard.Scale = template.Scale;
        billboard.AnimNum = 0;
        billboard.Draw = 1;
        billboard.State = StateInitializing;
        SetAndAppendBillboard(billboard, template.Resources!);
        return billboard;
    }

    // OFFSET 0x020211FC
    public static bool Billboard_Delete(Billboard? billboard)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (billboard!.State == StateInitializing)
        {
            ErrorHandling.GF_AssertFail();
        }
        BillboardList list = billboard.List!;
        if (billboard.State == StateNone)
        {
            return false;
        }
        BillboardList_Remove(billboard);
        if (billboard.State == StateVramTransfer)
        {
            FreeVRAMKeys(ref billboard.TexKey, ref billboard.Tex4x4Key, ref billboard.PlttKey);
        }
        Billboard_ResetVRAMTransfer(list, billboard);
        BillboardList_FreeBillboard(list, billboard);
        list.RedrawState = RedrawRequested;
        return true;
    }

    // OFFSET 0x0202125C
    public static void BillboardResources_Set(BillboardResources resources, byte[]? modelRes, G3dTexture? texture, BillboardAnim[]? anims, BillboardGfxSequence gfxSequence, uint texKey, uint tex4x4Key, uint plttKey)
    {
        resources.ModelRes = modelRes;
        resources.Texture = texture;
        resources.Anims = anims;
        CopyGfxSequence(gfxSequence, resources.GfxSequence);
        resources.TexKey = texKey;
        resources.Tex4x4Key = tex4x4Key;
        resources.PlttKey = plttKey;
    }

    // OFFSET 0x02021284
    public static void BillboardResources_SetWithoutKeys(BillboardResources resources, byte[]? modelRes, G3dTexture? texture, BillboardAnim[]? anims, BillboardGfxSequence gfxSequence)
    {
        resources.ModelRes = modelRes;
        resources.Texture = texture;
        resources.Anims = anims;
        CopyGfxSequence(gfxSequence, resources.GfxSequence);
        resources.TexKey = Gfd.InvalidKey;
        resources.Tex4x4Key = Gfd.InvalidKey;
        resources.PlttKey = Gfd.InvalidKey;
    }

    // OFFSET 0x020212A8
    public static void Billboard_SetPos(Billboard? billboard, in VecFx32 pos)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        billboard!.Pos = pos;
    }

    // OFFSET 0x020212C0
    public static ref VecFx32 Billboard_GetPos(Billboard? billboard)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return ref billboard!.Pos;
    }

    // OFFSET 0x020212D0
    public static void Billboard_SetScale(Billboard? billboard, in VecFx32 scale)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        billboard!.Scale = scale;
    }

    // OFFSET 0x020212EC
    public static ref VecFx32 Billboard_GetScale(Billboard? billboard)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return ref billboard!.Scale;
    }

    // OFFSET 0x020212FC
    public static void Billboard_SetRotMatrix(Billboard? billboard, MtxFx33[]? rotMatrix)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        billboard!.RotMatrix = rotMatrix;
    }

    // OFFSET 0x02021310
    public static MtxFx33[]? Billboard_GetRotMatrix(Billboard? billboard)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return billboard!.RotMatrix;
    }

    // OFFSET 0x02021320
    public static void Billboard_SetDrawFlag(Billboard? billboard, byte draw)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        billboard!.Draw = draw;
    }

    // OFFSET 0x02021334
    public static byte Billboard_GetDrawFlag(Billboard? billboard)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return billboard!.Draw;
    }

    // OFFSET 0x02021344
    public static void Billboard_SetAnimNum(Billboard? billboard, int animNum)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        billboard!.AnimNum = (ushort)animNum;
    }

    // OFFSET 0x02021358
    public static int Billboard_GetAnimNum(Billboard? billboard)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return billboard!.AnimNum;
    }

    // OFFSET 0x02021368
    public static int Billboard_AdvanceAnim(Billboard? billboard, int numFrames)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return GetAndAdvanceAnim(billboard!, numFrames);
    }

    // OFFSET 0x02021380
    public static void Billboard_SetFrameNum(Billboard? billboard, int frameNum)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        billboard!.FrameNum = frameNum;
    }

    // OFFSET 0x02021394
    public static int Billboard_GetFrameNum(Billboard? billboard)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return billboard!.FrameNum;
    }

    // OFFSET 0x020213A4
    public static void Billboard_SetAnimFrameNum(Billboard? billboard, int animFrameNum)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        billboard!.FrameNum = GetAnimStartFrame(billboard, billboard.AnimNum);
        billboard.FrameNum += animFrameNum;
    }

    // OFFSET 0x020213D4
    public static int Billboard_GetAnimFrameNum(Billboard? billboard)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        int startFrame = GetAnimStartFrame(billboard!, billboard!.AnimNum);
        return billboard.FrameNum - startFrame;
    }

    // OFFSET 0x020213F4
    public static G3dModel? Billboard_GetModel(Billboard? billboard)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return billboard!.Model;
    }

    // OFFSET 0x02021404
    public static int Billboard_GetState(Billboard? billboard)
    {
        if (billboard == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return billboard!.State;
    }

    // OFFSET 0x02021414
    public static void Billboard_TryRequestVRAMTransfer(Billboard billboard)
    {
        if (billboard.State == StateVramTransfer)
        {
            BillboardVramTransfer.BillboardVRAMTransfer_Request(billboard.VramTransfer!, billboard.FrameNum);
        }
    }

    // OFFSET 0x02021430
    public static G3dModel? Billboard_GetModel2(Billboard billboard)
    {
        return billboard.Model;
    }

    // OFFSET 0x02021438
    public static G3dTexture? Billboard_GetTexture(Billboard billboard)
    {
        return billboard.Texture;
    }

    // OFFSET 0x02021440
    public static G3dRenderObj Billboard_GetRenderObj(Billboard billboard)
    {
        return billboard.RenderObj;
    }

    // OFFSET 0x02021444
    public static void Billboard_SetCallback(Billboard billboard, BillboardCallback? callback, object? callbackParam)
    {
        billboard.CallbackParam = callbackParam;
        billboard.Callback = callback;
    }

    // OFFSET 0x0202144C
    private static void UpdateTexPlttAddressesForCurrentFrame(Billboard billboard)
    {
        ushort indices = BillboardGfxSequence.BillboardGfxSequence_GetTexPlttIndexAt(billboard.GfxSequence, (ushort)(billboard.FrameNum >> Fx32.Shift));
        byte textureIdx = (byte)indices;
        byte plttIdx = (byte)(indices >> 8);
        UpdateModelTextureAddresses(billboard.Model!, billboard.AnimTexture!, textureIdx);
        UpdateModelPlttAddresses(billboard.Model!, billboard.AnimTexture!, plttIdx);
    }

    // OFFSET 0x0202149C
    private static void UpdateModelTextureAddresses(G3dModel model, G3dTexture texture, byte textureIdx)
    {
        byte[] file = model.File;
        int materials = model.Offset + Bin.I32(file, model.Offset + ModelMaterialsOffset);
        int texToMatDict = materials + Bin.U16(file, materials);
        int textureEntry = G3dDict.GetEntryByIndex(texture.File, texture.Offset + TextureDictOffset, textureIdx);
        uint textureAddress = Bin.U32(texture.File, textureEntry) & 0xFFFF;
        for (int i = 0; i < file[texToMatDict + 1]; i++)
        {
            int entry = G3dDict.GetEntryByIndex(file, texToMatDict, i);
            if ((file[entry + 3] & DictBoundFlag) != 0)
            {
                UpdateMaterialTextureAddresses(file, materials, entry, textureAddress);
            }
        }
    }

    // OFFSET 0x02021524
    private static void UpdateMaterialTextureAddresses(byte[] file, int materials, int texToMatEntry, uint textureAddress)
    {
        int indices = materials + Bin.U16(file, texToMatEntry);
        for (int i = 0; i < file[texToMatEntry + 2]; i++)
        {
            int materialEntry = G3dDict.GetEntryByIndex(file, materials + MaterialDictOffset, file[indices + i]);
            int material = materials + Bin.I32(file, materialEntry);
            uint param = Bin.U32(file, material + MaterialTexImageParamOffset);
            if ((param & 0xFFFF) + textureAddress > MaxTextureAddress)
            {
                ErrorHandling.GF_AssertFail();
            }
            Bin.W32(file, material + MaterialTexImageParamOffset, param + textureAddress);
        }
    }

    // OFFSET 0x020215A0
    private static void UpdateModelPlttAddresses(G3dModel model, G3dTexture texture, byte plttIdx)
    {
        byte[] file = model.File;
        int materials = model.Offset + Bin.I32(file, model.Offset + ModelMaterialsOffset);
        int plttToMatDict = materials + Bin.U16(file, materials + 2);
        int paletteDict = texture.Offset + Bin.U16(texture.File, texture.Offset + TexturePaletteDictOffset);
        int paletteEntry = G3dDict.GetEntryByIndex(texture.File, paletteDict, plttIdx);
        uint plttAddress = (uint)Bin.U16(texture.File, paletteEntry);
        if ((Bin.U16(texture.File, paletteEntry + 2) & 1) == 0)
        {
            plttAddress >>= 1;
        }
        for (uint i = 0; i < file[plttToMatDict + 1]; i++)
        {
            int entry = G3dDict.GetEntryByIndex(file, plttToMatDict, (int)i);
            if ((file[entry + 3] & DictBoundFlag) != 0)
            {
                UpdateMaterialPlttAddresses(file, materials, entry, plttAddress);
            }
        }
    }

    // OFFSET 0x0202162C
    private static void UpdateMaterialPlttAddresses(byte[] file, int materials, int plttToMatEntry, uint plttAddress)
    {
        int indices = materials + Bin.U16(file, plttToMatEntry);
        for (uint i = 0; i < file[plttToMatEntry + 2]; i++)
        {
            int materialEntry = G3dDict.GetEntryByIndex(file, materials + MaterialDictOffset, file[indices + i]);
            int material = materials + Bin.I32(file, materialEntry);
            int paletteBase = Bin.U16(file, material + MaterialPaletteBaseOffset);
            if ((paletteBase & MaxPaletteBase) + plttAddress > MaxPaletteBase)
            {
                ErrorHandling.GF_AssertFail();
            }
            Bin.W16(file, material + MaterialPaletteBaseOffset, (ushort)(paletteBase + plttAddress));
        }
    }

    // OFFSET 0x020216A8
    private static void BillboardList_InitBillboards(BillboardList list)
    {
        for (int i = 0; i < list.MaxElements; i++)
        {
            Billboard_Reset(list.Billboards![i]);
            list.FreeStack![i] = list.Billboards[i];
        }
        list.FreeStackTop = 0;
    }

    // OFFSET 0x020216E0
    private static Billboard? BillboardList_AllocBillboard(BillboardList list)
    {
        if (list.FreeStackTop >= list.MaxElements)
        {
            return null;
        }
        Billboard? billboard = list.FreeStack![list.FreeStackTop];
        list.FreeStackTop++;
        return billboard;
    }

    // OFFSET 0x0202170C
    private static bool BillboardList_FreeBillboard(BillboardList list, Billboard billboard)
    {
        if (list.FreeStackTop <= 0)
        {
            return false;
        }
        Billboard_Reset(billboard);
        list.FreeStackTop--;
        list.FreeStack![list.FreeStackTop] = billboard;
        return true;
    }

    // OFFSET 0x02021744
    private static void AppendBillboardToList(Billboard sentinel, Billboard newBillboard)
    {
        newBillboard.Prev = sentinel.Prev;
        sentinel.Prev!.Next = newBillboard;
        newBillboard.Next = sentinel;
        sentinel.Prev = newBillboard;
    }

    // OFFSET 0x02021768
    private static void BillboardList_Remove(Billboard billboard)
    {
        billboard.Prev!.Next = billboard.Next;
        billboard.Next!.Prev = billboard.Prev;
    }

    // OFFSET 0x02021788
    private static G3dModelSet? RetrieveBillboardResources(BillboardResources resources, out G3dModel? model, out G3dTexture? texture, bool wantTexture)
    {
        byte[] modelRes = (byte[])BillboardResources_GetValue(resources, ResourceValueModel)!;
        G3dModelSet? modelSet = G3d.GetModelSet(modelRes);
        model = modelSet == null ? null : G3d.GetModelByIndex(modelSet, 0);
        texture = null;
        if (wantTexture)
        {
            texture = G3d.GetTexture(modelRes);
        }
        return modelSet;
    }

    // OFFSET 0x020217D4
    private static G3dTexture? BillboardResources_GetTexture(BillboardResources resources)
    {
        return (G3dTexture?)BillboardResources_GetValue(resources, ResourceValueTexture);
    }

    // OFFSET 0x020217E0
    private static void SetAndAppendBillboard(Billboard billboard, BillboardResources resources)
    {
        if (resources.TexKey == Gfd.InvalidKey)
        {
            AllocResourcesAndAppendBillboard(billboard, resources);
        }
        else
        {
            ResetResourcesAndAppendBillboard(billboard, resources);
        }
    }

    // OFFSET 0x020217F4
    private static int GetAnimStartFrame(Billboard billboard, int animNum)
    {
        BillboardAnim anim = Billboard_GetAnimAt(billboard.Anims!, animNum);
        return anim.StartFrame << Fx32.Shift;
    }

    // OFFSET 0x02021804
    private static int GetAndAdvanceAnim(Billboard billboard, int numFrames)
    {
        BillboardAnim anim = Billboard_GetAnimAt(billboard.Anims!, billboard.AnimNum);
        return AdvanceAnim(anim, ref billboard.FrameNum, numFrames);
    }

    // OFFSET 0x02021824
    private static int AdvanceAnim(BillboardAnim anim, ref int currentFrame, int numFrames)
    {
        int startFrame = anim.StartFrame << Fx32.Shift;
        int finished = 0;
        if (startFrame > currentFrame || (anim.EndFrame << Fx32.Shift) < currentFrame)
        {
            currentFrame = startFrame;
        }
        else
        {
            int endFrame = anim.EndFrame << Fx32.Shift;
            int nextFrame = currentFrame + numFrames;
            if (endFrame < nextFrame)
            {
                if (anim.StopAtEnd == 0)
                {
                    currentFrame = startFrame;
                }
                else
                {
                    finished = 1;
                    currentFrame = endFrame;
                }
            }
            else
            {
                currentFrame = nextFrame;
            }
        }
        return finished;
    }

    // OFFSET 0x0202185C
    private static void Billboard_SetKeysAndBindModelSet(Billboard billboard)
    {
        SetKeysAndBindModelSet(billboard.Texture!, billboard.ModelSet!, billboard.TexKey, billboard.Tex4x4Key, billboard.PlttKey);
    }

    // OFFSET 0x0202187C
    private static void ReleaseVRAMKeysAndModelSet(Billboard billboard)
    {
        ReleaseModelSet(billboard.ModelSet!);
        ReleaseVRAMKeys(billboard.Texture!, out _, out _, out _);
    }

    // OFFSET 0x0202189C
    private static object? BillboardResources_GetValue(BillboardResources resources, int value)
    {
        switch (value)
        {
            case ResourceValueModel:
                return resources.ModelRes;
            case ResourceValueTexture:
                return resources.Texture;
            default:
                return null;
        }
    }

    // OFFSET 0x020218B4
    private static BillboardAnim Billboard_GetAnimAt(BillboardAnim[] anims, int index)
    {
        return anims[index];
    }
}
