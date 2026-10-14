using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class Easy3D
{
    private const int TextureInfoFlagOffset = 0x10;
    private const int PaletteInfoFlagOffset = 0x20;
    private const int TextureVramKeyOffset = 8;
    private const int LoadedFlag = 1;
    private const int Palette4ColorFlag = 0x8000;
    private const uint TextureVramSize = 2;
    private const uint PaletteVramSize = 4;
    private const int Bg0Layer = 0;
    private const int Bg0LayerMask = 1;
    private const ushort BgPriorityMask = 3;
    private const ushort Disp3DShadingMask = 0xCFFD;
    private const ushort Disp3DMask = 0xCFFF;
    private const ushort Disp3DAntiAlias = 0x10;
    private const ushort Disp3DAlphaTestMask = 0xCFFB;
    private const ushort Disp3DAlphaBlend = 8;
    private const ushort Disp3DEdgeMarkingMask = 0xCFDF;
    private const int ClearDepthMax = 0x7FFF;
    private const int ClearPolygonID = 63;

    private static G3DPipelineBuffers? sPipelineBuffers;

    private static G3dModel? GetFirstModel(G3dModelSet? modelSet)
    {
        if (modelSet == null)
        {
            return null;
        }
        byte[] file = modelSet.File;
        int dictionary = modelSet.Offset + 8;
        if (file[modelSet.Offset + 9] == 0)
        {
            return null;
        }
        int entries = dictionary + Bin.U16(file, modelSet.Offset + 0xE) + 4;
        return new G3dModel { File = file, Offset = modelSet.Offset + (int)Bin.U32(file, entries) };
    }

    private static void SetupResource(byte[] resource)
    {
        G3dTexture? texture = G3d.GetTexture(resource);
        if (texture != null)
        {
            if (!Easy3D_IsTextureUploadedToVRAM(texture))
            {
                G3d.SetupResource(resource);
            }
        }
    }

    // OFFSET 0x0201C970
    public static void Easy3D_LoadModelFromPath(byte heapID, string path, out G3dModel? model, out byte[]? resource)
    {
        resource = GameSystem.ReadFileToHeap(heapID, path);
        SetupResource(resource!);
        model = GetFirstModel(G3d.GetModelSet(resource!));
    }

    // OFFSET 0x0201C9CC
    public static void Easy3D_LoadModelFromResource(out G3dModel? model, byte[] resource)
    {
        SetupResource(resource);
        model = GetFirstModel(G3d.GetModelSet(resource));
    }

    // OFFSET 0x0201CA24
    public static void Easy3D_InitRenderObjFromPath(byte heapID, string path, G3dRenderObj renderObj, out G3dModel? model, out byte[]? resource)
    {
        Easy3D_LoadModelFromPath(heapID, path, out model, out resource);
        G3d.InitRenderObj(renderObj, model!);
    }

    // OFFSET 0x0201CA3C
    public static void Easy3D_InitRenderObjFromResource(G3dRenderObj renderObj, out G3dModel? model, byte[] resource)
    {
        Easy3D_LoadModelFromResource(out model, resource);
        G3d.InitRenderObj(renderObj, model!);
    }

    // OFFSET 0x0201CA5C
    public static bool Easy3D_IsTextureUploadedToVRAM(G3dTexture texture)
    {
        if ((Bin.U16(texture.File, texture.Offset + TextureInfoFlagOffset) & LoadedFlag) != 0)
        {
            return true;
        }
        if ((Bin.U16(texture.File, texture.Offset + PaletteInfoFlagOffset) & LoadedFlag) != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0201CA74
    public static void Easy3D_DrawRenderObj(G3dRenderObj renderObj, in VecFx32 position, in MtxFx33 rotation, in VecFx32 scale)
    {
        Render3D.SetBaseTranslation(position);
        Render3D.SetBaseRotation(rotation);
        Render3D.SetBaseScale(scale);
        Render3D.FlushGlobalState();
        G3d.Draw(renderObj);
    }

    // OFFSET 0x0201CAB0
    public static void Easy3D_DrawRenderObjSimple(G3dRenderObj renderObj, in VecFx32 position, in MtxFx33 rotation, in VecFx32 scale)
    {
        Render3D.SetBaseTranslation(position);
        Render3D.SetBaseRotation(rotation);
        Render3D.SetBaseScale(scale);
        Render3D.FlushGlobalState();
        G3d.Draw1Mat1Shp(renderObj.Model!, 0, 0, true);
        Render3D.FlushBuffer();
    }

    // OFFSET 0x0201CAF4
    public static void Easy3D_Init(byte heapID)
    {
        sPipelineBuffers = G3DPipeline.G3DPipeline_InitEx(heapID, G3DPipeline.VramManagerLinkedList, TextureVramSize, G3DPipeline.VramManagerLinkedList, PaletteVramSize, Easy3D_SetupEngine);
    }

    // OFFSET 0x0201CB1C
    private static void Easy3D_SetupEngine()
    {
        GXLayers.GXLayers_EngineAToggleLayers(Bg0LayerMask, 1);
        Video.SetBgControl(Bg0Layer, (ushort)((Video.GetBgControl(Bg0Layer) & ~BgPriorityMask) | 1));
        Render3D.Display3DControl &= Disp3DShadingMask;
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DAntiAlias);
        Render3D.Display3DControl &= Disp3DAlphaTestMask;
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & Disp3DMask) | Disp3DAlphaBlend);
        Render3D.Display3DControl &= Disp3DEdgeMarkingMask;
        Render3D.SetFog(false, 0, 0, 0);
        Render3D.SetClearColor(0, 0, ClearDepthMax, ClearPolygonID, false);
        Render3D.SetViewport(0, 0, 255, 191);
    }

    // OFFSET 0x0201CBA0
    public static void Easy3D_Shutdown()
    {
        G3DPipeline.G3DPipelineBuffers_Free(sPipelineBuffers!);
    }

    // OFFSET 0x0201CBB0
    public static bool Easy3D_BindTextureToResource(byte[] resource, G3dTexture? texture)
    {
        G3dModelSet? modelSet = G3d.GetModelSet(resource);
        if (texture != null)
        {
            G3d.BindModelSet(modelSet!, texture);
            return true;
        }
        return false;
    }

    // OFFSET 0x0201CBCC
    public static bool Easy3D_UploadTextureToVRAM(G3dTexture? texture)
    {
        if (texture == null)
        {
            return false;
        }
        Bin.W32(texture.File, texture.Offset + TextureVramKeyOffset, 0);
        bool textureAllocated = true;
        bool texture4x4Allocated = true;
        bool paletteAllocated = true;
        uint textureSize = G3d.GetTextureSize(texture);
        uint texture4x4Size = G3d.GetTexture4x4Size(texture);
        uint paletteSize = G3d.GetPaletteSize(texture);
        uint textureKey;
        if (textureSize != 0)
        {
            textureKey = Gfd.AllocTexVram(textureSize, 0, 0);
            if (textureKey == Gfd.InvalidKey)
            {
                textureAllocated = false;
            }
        }
        else
        {
            textureKey = Gfd.InvalidKey;
        }
        uint texture4x4Key;
        if (texture4x4Size != 0)
        {
            texture4x4Key = Gfd.AllocTexVram(texture4x4Size, 1, 0);
            if (texture4x4Key == Gfd.InvalidKey)
            {
                texture4x4Allocated = false;
            }
        }
        else
        {
            texture4x4Key = Gfd.InvalidKey;
        }
        uint paletteKey;
        if (paletteSize != 0)
        {
            int is4Color = Bin.U16(texture.File, texture.Offset + PaletteInfoFlagOffset) & Palette4ColorFlag;
            paletteKey = Gfd.AllocPlttVram(paletteSize, is4Color, 0);
            if (paletteKey == Gfd.InvalidKey)
            {
                paletteAllocated = false;
            }
        }
        else
        {
            paletteKey = Gfd.InvalidKey;
        }
        if (!textureAllocated)
        {
            if (Gfd.FreeTexVram(textureKey) != 0)
            {
                ErrorHandling.GF_AssertFail();
            }
            return false;
        }
        if (!texture4x4Allocated)
        {
            if (Gfd.FreeTexVram(texture4x4Key) != 0)
            {
                ErrorHandling.GF_AssertFail();
            }
            return false;
        }
        if (!paletteAllocated)
        {
            if (Gfd.FreePlttVram(paletteKey) != 0)
            {
                ErrorHandling.GF_AssertFail();
            }
            return false;
        }
        G3d.SetTextureKeys(texture, textureKey, texture4x4Key);
        G3d.SetPaletteKey(texture, paletteKey);
        G3d.LoadTexture(texture, true);
        G3d.LoadPalette(texture, true);
        return true;
    }
}
