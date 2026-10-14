using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class TextureResource
{
    public Resource? Resource;
    public uint TexKey;
    public uint Tex4x4Key;
    public uint PlttKey;
    public byte[]? TextureData;
    public ushort DataDiscarded;
    public ushort DiscardAfterUpload;
}

public sealed class TextureResourceManager
{
    public ResourceCollection? Collection;
    public TextureResource[]? Textures;

    private const int TextureDataOffset = 0x14;
    private const int TexturePaletteInfoFlagOffset = 0x20;
    private const int Palette4ColorFlag = 0x8000;

    // OFFSET 0x02022BD8
    public static TextureResourceManager TextureResourceManager_New(int maxTextures, int heapID)
    {
        TextureResourceManager manager = new();
        manager.Collection = ResourceCollection.ResourceCollection_New(maxTextures, heapID);
        manager.Textures = new TextureResource[maxTextures];
        for (int i = 0; i < maxTextures; i++)
        {
            manager.Textures[i] = new TextureResource();
            TextureResource_Reset(manager.Textures[i]);
        }
        return manager;
    }

    // OFFSET 0x02022C1C
    public static void TextureResourceManager_Delete(TextureResourceManager? manager)
    {
        if (manager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        TextureResourceManager_Clear(manager);
        ResourceCollection.ResourceCollection_Delete(manager!.Collection);
        manager.Textures = null;
    }

    // OFFSET 0x02022C40
    public static bool TextureResourceManager_IsIDUnused(TextureResourceManager? manager, int id)
    {
        if (manager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return ResourceCollection.ResourceCollection_IsIDUnused(manager!.Collection, id);
    }

    // OFFSET 0x02022C58
    public static TextureResource TextureResourceManager_AddTexture(TextureResourceManager? manager, byte[] data, int id, int discardAfterUpload, int heapID)
    {
        if (manager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        TextureResource textureResource = TextureResourceManager_FindFreeTexture(manager!)!;
        textureResource.DiscardAfterUpload = (ushort)discardAfterUpload;
        byte[] resourceData;
        if (textureResource.DiscardAfterUpload == 1)
        {
            resourceData = CopyTextureHeader(data, heapID);
            textureResource.TextureData = data;
        }
        else
        {
            textureResource.TextureData = null;
            resourceData = data;
        }
        textureResource.Resource = ResourceCollection.ResourceCollection_Add(manager!.Collection, resourceData, id);
        return textureResource;
    }

    // OFFSET 0x02022C9C
    public static TextureResource TextureResourceManager_AddTextureAndAllocVRAM(TextureResourceManager? manager, byte[] data, int id, int discardAfterUpload, int heapID)
    {
        TextureResource textureResource = TextureResourceManager_AddTexture(manager, data, id, discardAfterUpload, heapID);
        TextureResource_AllocVRAM(textureResource);
        return textureResource;
    }

    // OFFSET 0x02022CB4
    public static void TextureResourceManager_RemoveTexture(TextureResourceManager? manager, TextureResource? textureResource)
    {
        if (manager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (textureResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (textureResource!.DiscardAfterUpload == 1 && textureResource.DataDiscarded == 0)
        {
            Heap.Heap_Free(textureResource.TextureData!);
            textureResource.DiscardAfterUpload = 0;
        }
        if (textureResource.Resource != null)
        {
            ResourceCollection.ResourceCollection_Remove(manager!.Collection, textureResource.Resource);
        }
        if (textureResource.TexKey != Gfd.InvalidKey)
        {
            if (Gfd.FreeTexVram(textureResource.TexKey) != 0)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        if (textureResource.Tex4x4Key != Gfd.InvalidKey)
        {
            if (Gfd.FreeTexVram(textureResource.Tex4x4Key) != 0)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        if (textureResource.PlttKey != Gfd.InvalidKey)
        {
            if (Gfd.FreePlttVram(textureResource.PlttKey) != 0)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        TextureResource_Reset(textureResource);
    }

    // OFFSET 0x02022D38
    public static void TextureResourceManager_RemoveTextureWithID(TextureResourceManager? manager, int id)
    {
        if (manager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        TextureResource? textureResource = TextureResourceManager_FindTextureResource(manager, id);
        TextureResourceManager_RemoveTexture(manager, textureResource);
    }

    // OFFSET 0x02022D58
    public static void TextureResourceManager_Clear(TextureResourceManager? manager)
    {
        if (manager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (manager!.Textures == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < manager.Collection!.Capacity; i++)
        {
            if (manager.Textures![i].Resource != null)
            {
                TextureResourceManager_RemoveTexture(manager, manager.Textures[i]);
            }
        }
    }

    // OFFSET 0x02022D98
    public static TextureResource? TextureResourceManager_FindTextureResource(TextureResourceManager? manager, int id)
    {
        if (manager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < manager!.Collection!.Capacity; i++)
        {
            if (manager.Textures![i].Resource != null)
            {
                if (TextureResource_GetID(manager.Textures[i]) == id)
                {
                    return manager.Textures[i];
                }
            }
        }
        return null;
    }

    // OFFSET 0x02022DE0
    public static int TextureResource_GetID(TextureResource? textureResource)
    {
        if (textureResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return ResourceCollection.Resource_GetID(textureResource!.Resource);
    }

    // OFFSET 0x02022DF4
    public static G3dTexture? TextureResource_GetUnderlyingResource(TextureResource? textureResource)
    {
        if (textureResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return TextureResource_GetResourceTexture(textureResource!);
    }

    // OFFSET 0x02022E08
    public static void TextureResource_UploadToVRAM(TextureResource? textureResource)
    {
        if (textureResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (textureResource!.DataDiscarded != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (textureResource.PlttKey == Gfd.InvalidKey)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        G3dTexture texture = TextureResource_GetFullTexture(textureResource)!;
        UploadTexture(texture, textureResource);
    }

    // OFFSET 0x02022E38
    public static void TextureResourceManager_UploadTextureWithID(TextureResourceManager? manager, int id)
    {
        if (manager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        TextureResource? textureResource = TextureResourceManager_FindTextureResource(manager, id);
        TextureResource_UploadToVRAM(textureResource);
    }

    // OFFSET 0x02022E54
    public static void TextureResource_DiscardTextureData(TextureResource? textureResource)
    {
        if (textureResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (textureResource!.DiscardAfterUpload == 0)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        if (textureResource.DataDiscarded != 0)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        G3dTexture fullTexture = TextureResource_GetFullTexture(textureResource)!;
        ReleaseTextureKeys(fullTexture);
        G3dTexture headerTexture = TextureResource_GetResourceTexture(textureResource)!;
        SetTextureKeys(headerTexture, textureResource.TexKey, textureResource.Tex4x4Key, textureResource.PlttKey);
        Heap.Heap_Free(textureResource.TextureData!);
        textureResource.TextureData = null;
        textureResource.DataDiscarded = 1;
    }

    // OFFSET 0x02022EA0
    public static void TextureResourceManager_DiscardTextureDataWithID(TextureResourceManager? manager, int id)
    {
        if (manager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        TextureResource? textureResource = TextureResourceManager_FindTextureResource(manager, id);
        TextureResource_DiscardTextureData(textureResource);
    }

    // OFFSET 0x02022EBC
    public static void TextureResource_AllocVRAM(TextureResource? textureResource)
    {
        if (textureResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (textureResource!.DataDiscarded != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (textureResource.PlttKey != Gfd.InvalidKey)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        G3dTexture texture = TextureResource_GetFullTexture(textureResource)!;
        AllocTextureKeys(texture, ref textureResource.TexKey, ref textureResource.Tex4x4Key, ref textureResource.PlttKey);
    }

    // OFFSET 0x02022EF4
    public static uint TextureResource_GetTexKey(TextureResource? textureResource)
    {
        if (textureResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return textureResource!.TexKey;
    }

    // OFFSET 0x02022F04
    public static uint TextureResource_GetTex4x4Key(TextureResource? textureResource)
    {
        if (textureResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return textureResource!.Tex4x4Key;
    }

    // OFFSET 0x02022F14
    public static uint TextureResource_GetPlttKey(TextureResource? textureResource)
    {
        if (textureResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return textureResource!.PlttKey;
    }

    // OFFSET 0x02022F24
    public static int TextureResource_GetHeaderSize(byte[] data)
    {
        return GetTextureHeaderSize(data);
    }

    // OFFSET 0x02022F2C
    private static TextureResource? TextureResourceManager_FindFreeTexture(TextureResourceManager manager)
    {
        for (int i = 0; i < manager.Collection!.Capacity; i++)
        {
            if (manager.Textures![i].Resource == null)
            {
                return manager.Textures[i];
            }
        }
        return null;
    }

    // OFFSET 0x02022F5C
    private static void TextureResource_Reset(TextureResource textureResource)
    {
        textureResource.Resource = null;
        textureResource.TexKey = 0;
        textureResource.Tex4x4Key = 0;
        textureResource.PlttKey = 0;
        textureResource.DataDiscarded = 0;
        textureResource.TextureData = null;
        textureResource.DataDiscarded = 0;
    }

    // OFFSET 0x02022F70
    private static G3dTexture? TextureResource_GetResourceTexture(TextureResource textureResource)
    {
        byte[] data = ResourceCollection.Resource_GetData(textureResource.Resource)!;
        return G3d.GetTexture(data);
    }

    // OFFSET 0x02022F80
    private static G3dTexture? TextureResource_GetFullTexture(TextureResource textureResource)
    {
        byte[] data;
        if (textureResource.DiscardAfterUpload == 0)
        {
            data = ResourceCollection.Resource_GetData(textureResource.Resource)!;
        }
        else
        {
            data = textureResource.TextureData!;
        }
        return G3d.GetTexture(data);
    }

    // OFFSET 0x02022F98
    private static void AllocTextureKeys(G3dTexture texture, ref uint texKey, ref uint tex4x4Key, ref uint plttKey)
    {
        uint textureSize = G3d.GetTextureSize(texture);
        uint texture4x4Size = G3d.GetTexture4x4Size(texture);
        uint paletteSize = G3d.GetPaletteSize(texture);
        if (textureSize != 0)
        {
            texKey = Gfd.AllocTexVram(textureSize, 0, 0);
        }
        if (texture4x4Size != 0)
        {
            tex4x4Key = Gfd.AllocTexVram(texture4x4Size, 1, 0);
        }
        if (paletteSize != 0)
        {
            int is4Color = Bin.U16(texture.File, texture.Offset + TexturePaletteInfoFlagOffset) & Palette4ColorFlag;
            plttKey = Gfd.AllocPlttVram(paletteSize, is4Color, 0);
        }
    }

    // OFFSET 0x02023008
    private static void UploadTexture(G3dTexture texture, TextureResource textureResource)
    {
        SetTextureKeys(texture, textureResource.TexKey, textureResource.Tex4x4Key, textureResource.PlttKey);
        G3d.LoadTexture(texture, true);
        G3d.LoadPalette(texture, true);
    }

    // OFFSET 0x02023034
    private static void SetTextureKeys(G3dTexture texture, uint texKey, uint tex4x4Key, uint plttKey)
    {
        G3d.SetTextureKeys(texture, texKey, tex4x4Key);
        G3d.SetPaletteKey(texture, plttKey);
    }

    // OFFSET 0x02023048
    private static void ReleaseTextureKeys(G3dTexture texture)
    {
        G3d.ReleaseTextureKeys(texture, out _, out _);
        G3d.ReleasePaletteKey(texture);
    }

    // OFFSET 0x02023060
    private static byte[] CopyTextureHeader(byte[] data, int heapID)
    {
        int size = GetTextureHeaderSize(data);
        byte[] copy = Heap.Heap_Alloc(heapID, (uint)size)!;
        Array.Copy(data, copy, size);
        return copy;
    }

    // OFFSET 0x02023084
    private static int GetTextureHeaderSize(byte[] data)
    {
        G3dTexture? texture = G3d.GetTexture(data);
        if (texture == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return texture!.Offset + Bin.I32(texture.File, texture.Offset + TextureDataOffset);
    }
}
