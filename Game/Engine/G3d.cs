using System.Numerics;
using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class G3d
{
    public const int AnimJoint = 1;
    public const int AnimTextureSrt = 2;
    public const int AnimTexturePattern = 3;
    public const int AnimVisibility = 4;
    public const int AnimMaterialColor = 5;

    private const float FixedOne = 4096f;
    private const int ModelSbc = 0x04;
    private const int ModelMaterials = 0x08;
    private const int ModelShapes = 0x0C;
    private const int ModelEnvelopes = 0x10;
    private const int ModelNodeCount = 0x17;
    private const int ModelMaterialCount = 0x18;
    private const int ModelPosScale = 0x1C;
    private const int ModelInvPosScale = 0x20;
    private const int ModelNodes = 0x40;
    private const int MatDiffAmb = 0x04;
    private const int MatSpecEmi = 0x08;
    private const int MatPolyAttr = 0x0C;
    private const int MatPolyAttrMask = 0x10;
    private const int MatTexImageParam = 0x14;
    private const int MatPaletteBase = 0x1C;
    private const int MatFlag = 0x1E;
    private const int MatOrigWidth = 0x20;
    private const int MatOrigHeight = 0x22;
    private const int MatTexMatrixData = 0x2C;
    private const int MatFlagTexMatrixUse = 0x0001;
    private const int MatFlagScaleOne = 0x0002;
    private const int MatFlagRotZero = 0x0004;
    private const int MatFlagTransZero = 0x0008;
    private const int MatFlagDiffuse = 0x0040;
    private const int MatFlagAmbient = 0x0080;
    private const int MatFlagVertexColor = 0x0100;
    private const int MatFlagSpecular = 0x0200;
    private const int MatFlagEmission = 0x0400;
    private const int MatFlagShininess = 0x0800;
    private const int NodeTransZero = 0x0001;
    private const int NodeRotZero = 0x0002;
    private const int NodeScaleOne = 0x0004;
    private const int NodePivotExists = 0x0008;
    private const int NodePivotNegative = 0x0100;
    private const int NodeSignReverseC = 0x0200;
    private const int NodeSignReverseD = 0x0400;
    private const int TexInfoKey = 0x08;
    private const int TexInfoSize = 0x0C;
    private const int TexInfoDict = 0x0E;
    private const int TexInfoFlag = 0x10;
    private const int TexInfoData = 0x14;
    private const int Tex4x4Flag = 0x20;
    private const int TexLoadedFlag = 1;
    private const int Tex4x4Key = 0x18;
    private const int Tex4x4Size = 0x1C;
    private const int Tex4x4Data = 0x24;
    private const int Tex4x4Index = 0x28;
    private const int PlttKey = 0x2C;
    private const int PlttSize = 0x30;
    private const int PlttDict = 0x34;
    private const int PlttData = 0x38;
    private const uint TexParamKeepMask = 0xC00F0000;
    private const int FormatCompressed = 5;

    private static readonly byte[,] pivotElements =
    {
        { 4, 5, 7, 8 }, { 3, 5, 6, 8 }, { 3, 4, 6, 7 },
        { 1, 2, 7, 8 }, { 0, 2, 6, 8 }, { 0, 1, 6, 7 },
        { 1, 2, 4, 5 }, { 0, 2, 3, 5 }, { 0, 1, 3, 4 },
    };

    private sealed class PatternBinding
    {
        public int[] MaterialData = Array.Empty<int>();
        public uint[] TextureParams = Array.Empty<uint>();
        public int[] PaletteBases = Array.Empty<int>();
    }

    private static int FindBlock(byte[] file, string magic)
    {
        if (file.Length < 0x10)
        {
            return -1;
        }
        int blocks = Bin.U16(file, 0x0E);
        for (int i = 0; i < blocks; i++)
        {
            int offset = Bin.I32(file, 0x10 + i * 4);
            if (offset + 4 <= file.Length && Bin.Magic(file, offset) == magic)
            {
                return offset;
            }
        }
        return -1;
    }

    public static G3dModelSet? GetModelSet(byte[] resource)
    {
        int block = FindBlock(resource, "MDL0");
        return block < 0 ? null : new G3dModelSet { File = resource, Offset = block };
    }

    public static G3dTexture? GetTexture(byte[] resource)
    {
        int block = FindBlock(resource, "TEX0");
        return block < 0 ? null : new G3dTexture { File = resource, Offset = block };
    }

    public static G3dModel? GetModelByIndex(G3dModelSet set, int index)
    {
        int entry = G3dDict.GetEntryByIndex(set.File, set.Offset + 8, index);
        return entry < 0 ? null : new G3dModel { File = set.File, Offset = set.Offset + Bin.I32(set.File, entry) };
    }

    public static G3dAnim? GetAnimByIndex(byte[] resource, int index)
    {
        if (resource.Length < 0x14)
        {
            return null;
        }
        int block = Bin.I32(resource, 0x10);
        int entry = G3dDict.GetEntryByIndex(resource, block + 8, index);
        if (entry < 0)
        {
            return null;
        }
        int offset = block + Bin.I32(resource, entry);
        return new G3dAnim { File = resource, Offset = offset, NumFrames = (ushort)Bin.U16(resource, offset + 4) };
    }

    public static uint GetTextureSize(G3dTexture texture)
    {
        return (uint)Bin.U16(texture.File, texture.Offset + TexInfoSize) << 3;
    }

    public static uint GetTexture4x4Size(G3dTexture texture)
    {
        return (uint)Bin.U16(texture.File, texture.Offset + Tex4x4Size) << 3;
    }

    public static uint GetPaletteSize(G3dTexture texture)
    {
        return (uint)Bin.U16(texture.File, texture.Offset + PlttSize) << 3;
    }

    public static void SetTextureKeys(G3dTexture texture, uint key, uint key4x4)
    {
        Bin.W32(texture.File, texture.Offset + TexInfoKey, key);
        Bin.W32(texture.File, texture.Offset + Tex4x4Key, key4x4);
    }

    public static void SetPaletteKey(G3dTexture texture, uint key)
    {
        Bin.W32(texture.File, texture.Offset + PlttKey, key);
    }

    public static void ReleaseTextureKeys(G3dTexture texture, out uint key, out uint key4x4)
    {
        key = Bin.U32(texture.File, texture.Offset + TexInfoKey);
        key4x4 = Bin.U32(texture.File, texture.Offset + Tex4x4Key);
        SetTextureKeys(texture, 0, 0);
        Bin.W16(texture.File, texture.Offset + TexInfoFlag, (ushort)(Bin.U16(texture.File, texture.Offset + TexInfoFlag) & ~TexLoadedFlag));
        Bin.W16(texture.File, texture.Offset + Tex4x4Flag, (ushort)(Bin.U16(texture.File, texture.Offset + Tex4x4Flag) & ~TexLoadedFlag));
    }

    public static uint ReleasePaletteKey(G3dTexture texture)
    {
        uint key = Bin.U32(texture.File, texture.Offset + PlttKey);
        SetPaletteKey(texture, 0);
        return key;
    }

    private static void CopyToTextures(byte[] file, int source, uint address, int size)
    {
        byte[] memory = Video.Textures;
        size = Math.Min(size, Math.Min(file.Length - source, memory.Length - (int)address));
        if (size > 0)
        {
            Array.Copy(file, source, memory, address, size);
        }
    }

    public static void LoadTexture(G3dTexture texture)
    {
        byte[] file = texture.File;
        int block = texture.Offset;
        uint size = GetTextureSize(texture);
        if (size != 0)
        {
            uint address = Gfd.GetTexKeyAddress(Bin.U32(file, block + TexInfoKey));
            CopyToTextures(file, block + Bin.I32(file, block + TexInfoData), address, (int)size);
            Bin.W16(file, block + TexInfoFlag, (ushort)(Bin.U16(file, block + TexInfoFlag) | TexLoadedFlag));
        }
        uint size4x4 = GetTexture4x4Size(texture);
        if (size4x4 != 0)
        {
            uint address = Gfd.GetTexKeyAddress(Bin.U32(file, block + Tex4x4Key));
            CopyToTextures(file, block + Bin.I32(file, block + Tex4x4Data), address, (int)size4x4);
            uint indexAddress = 0x20000 + ((address & 0x1FFFF) >> 1) + (address >= 0x40000 ? 0x10000u : 0);
            CopyToTextures(file, block + Bin.I32(file, block + Tex4x4Index), indexAddress, (int)(size4x4 >> 1));
            Bin.W16(file, block + Tex4x4Flag, (ushort)(Bin.U16(file, block + Tex4x4Flag) | TexLoadedFlag));
        }
    }

    public static void LoadTexture(G3dTexture texture, bool markReleasable)
    {
        LoadTexture(texture);
    }

    public static void LoadPalette(G3dTexture texture, bool markReleasable)
    {
        LoadPalette(texture);
    }

    public static void LoadPalette(G3dTexture texture)
    {
        byte[] file = texture.File;
        int block = texture.Offset;
        uint size = GetPaletteSize(texture);
        if (size == 0)
        {
            return;
        }
        uint address = Gfd.GetPlttKeyAddress(Bin.U32(file, block + PlttKey));
        int source = block + Bin.I32(file, block + PlttData);
        ushort[] palette = Video.TexturePalettes;
        int count = (int)Math.Min(size / 2, Math.Min((file.Length - source) / 2, palette.Length - (int)(address / 2)));
        for (int i = 0; i < count; i++)
        {
            palette[address / 2 + i] = (ushort)Bin.U16(file, source + i * 2);
        }
    }

    private static int MaterialCount(G3dModel model)
    {
        return model.File[model.Offset + ModelMaterialCount];
    }

    private static int MaterialsOffset(G3dModel model)
    {
        return model.Offset + Bin.I32(model.File, model.Offset + ModelMaterials);
    }

    private static int MaterialData(G3dModel model, int index)
    {
        int materials = MaterialsOffset(model);
        int entry = G3dDict.GetEntryByIndex(model.File, materials + 4, index);
        return entry < 0 ? -1 : materials + Bin.I32(model.File, entry);
    }

    private static int FindMaterialByName(G3dModel model, byte[] file, int nameOffset)
    {
        int materials = MaterialsOffset(model);
        byte[] name = new byte[G3dDict.NameLength];
        Array.Copy(file, nameOffset, name, 0, G3dDict.NameLength);
        int count = G3dDict.GetEntryCount(model.File, materials + 4);
        int target = G3dDict.GetEntryByName(model.File, materials + 4, name);
        if (target < 0)
        {
            return -1;
        }
        for (int i = 0; i < count; i++)
        {
            if (G3dDict.GetEntryByIndex(model.File, materials + 4, i) == target)
            {
                return i;
            }
        }
        return -1;
    }

    private static int DictNameOffset(byte[] file, int dict, int index)
    {
        int entries = dict + Bin.U16(file, dict + 6);
        return entries + Bin.U16(file, entries + 2) + index * G3dDict.NameLength;
    }

    private static bool TextureParams(G3dTexture texture, byte[] nameFile, int nameOffset, out uint param, out uint extra)
    {
        byte[] file = texture.File;
        int block = texture.Offset;
        byte[] name = new byte[G3dDict.NameLength];
        Array.Copy(nameFile, nameOffset, name, 0, G3dDict.NameLength);
        int entry = G3dDict.GetEntryByName(file, block + Bin.U16(file, block + TexInfoDict), name);
        if (entry < 0)
        {
            param = 0;
            extra = 0;
            return false;
        }
        param = Bin.U32(file, entry);
        extra = Bin.U32(file, entry + 4);
        bool compressed = ((param >> 26) & 7) == FormatCompressed;
        uint key = Bin.U32(file, block + (compressed ? Tex4x4Key : TexInfoKey));
        param += Gfd.GetTexKeyAddress(key) >> 3;
        return true;
    }

    private static bool PaletteBase(G3dTexture texture, byte[] nameFile, int nameOffset, out int paletteBase)
    {
        byte[] file = texture.File;
        int block = texture.Offset;
        byte[] name = new byte[G3dDict.NameLength];
        Array.Copy(nameFile, nameOffset, name, 0, G3dDict.NameLength);
        int entry = G3dDict.GetEntryByName(file, block + Bin.U16(file, block + PlttDict), name);
        if (entry < 0)
        {
            paletteBase = 0;
            return false;
        }
        int offset = Bin.U16(file, entry);
        bool fourColor = (Bin.U16(file, entry + 2) & 1) != 0;
        uint address = Gfd.GetPlttKeyAddress(Bin.U32(file, block + PlttKey));
        paletteBase = fourColor ? offset + (int)(address >> 3) : (offset >> 1) + (int)(address >> 4);
        return true;
    }

    public static void BindModel(G3dModel model, G3dTexture texture)
    {
        byte[] file = model.File;
        int materials = MaterialsOffset(model);
        int textureDict = materials + Bin.U16(file, materials);
        int paletteDict = materials + Bin.U16(file, materials + 2);
        int textureCount = G3dDict.GetEntryCount(file, textureDict);
        for (int i = 0; i < textureCount; i++)
        {
            int entry = G3dDict.GetEntryByIndex(file, textureDict, i);
            if (!TextureParams(texture, file, DictNameOffset(file, textureDict, i), out uint param, out uint extra))
            {
                continue;
            }
            int list = materials + Bin.U16(file, entry);
            int count = file[entry + 2];
            file[entry + 3] |= 1;
            for (int k = 0; k < count; k++)
            {
                int material = MaterialData(model, file[list + k]);
                if (material < 0)
                {
                    continue;
                }
                uint current = Bin.U32(file, material + MatTexImageParam);
                Bin.W32(file, material + MatTexImageParam, (current & TexParamKeepMask) | (param & ~TexParamKeepMask));
                Bin.W16(file, material + MatOrigWidth, (ushort)(extra & 0x7FF));
                Bin.W16(file, material + MatOrigHeight, (ushort)((extra >> 11) & 0x7FF));
            }
        }
        int paletteCount = G3dDict.GetEntryCount(file, paletteDict);
        for (int i = 0; i < paletteCount; i++)
        {
            int entry = G3dDict.GetEntryByIndex(file, paletteDict, i);
            if (!PaletteBase(texture, file, DictNameOffset(file, paletteDict, i), out int paletteBase))
            {
                continue;
            }
            int list = materials + Bin.U16(file, entry);
            int count = file[entry + 2];
            file[entry + 3] |= 1;
            for (int k = 0; k < count; k++)
            {
                int material = MaterialData(model, file[list + k]);
                if (material >= 0)
                {
                    Bin.W16(file, material + MatPaletteBase, (ushort)paletteBase);
                }
            }
        }
    }

    public static void BindModelSet(G3dModelSet set, G3dTexture texture)
    {
        int count = G3dDict.GetEntryCount(set.File, set.Offset + 8);
        for (int i = 0; i < count; i++)
        {
            G3dModel? model = GetModelByIndex(set, i);
            if (model != null)
            {
                BindModel(model, texture);
            }
        }
    }

    public static void SetupResource(byte[] resource)
    {
        G3dModelSet? set = GetModelSet(resource);
        G3dTexture? texture = GetTexture(resource);
        if (texture == null)
        {
            return;
        }
        uint size = GetTextureSize(texture);
        uint size4x4 = GetTexture4x4Size(texture);
        uint paletteSize = GetPaletteSize(texture);
        uint key = size != 0 ? Gfd.AllocTexVram(size, 0, 0) : 0;
        uint key4x4 = size4x4 != 0 ? Gfd.AllocTexVram(size4x4, 1, 0) : 0;
        uint paletteKey = paletteSize != 0 ? Gfd.AllocPlttVram(paletteSize, Bin.U16(texture.File, texture.Offset + PlttSize + 2) & 1, 0) : 0;
        SetTextureKeys(texture, key, key4x4);
        SetPaletteKey(texture, paletteKey);
        LoadTexture(texture);
        LoadPalette(texture);
        if (set != null)
        {
            BindModelSet(set, texture);
        }
    }

    public static void SetMaterialFlags(G3dModel model, bool set, int flags)
    {
        int count = MaterialCount(model);
        for (int i = 0; i < count; i++)
        {
            int material = MaterialData(model, i);
            if (material < 0)
            {
                continue;
            }
            int value = Bin.U16(model.File, material + MatFlag);
            value = set ? value | flags : value & ~flags;
            Bin.W16(model.File, material + MatFlag, (ushort)value);
        }
    }

    public static void SetMaterialPolygonAttrMask(G3dModel model, bool set, uint mask)
    {
        int count = MaterialCount(model);
        for (int i = 0; i < count; i++)
        {
            int material = MaterialData(model, i);
            if (material < 0)
            {
                continue;
            }
            uint value = Bin.U32(model.File, material + MatPolyAttrMask);
            value = set ? value | mask : value & ~mask;
            Bin.W32(model.File, material + MatPolyAttrMask, value);
        }
    }

    public static void SetMaterialParam(G3dModel model, int wordOffset, uint mask, uint value)
    {
        int count = MaterialCount(model);
        for (int i = 0; i < count; i++)
        {
            int material = MaterialData(model, i);
            if (material < 0)
            {
                continue;
            }
            uint word = Bin.U32(model.File, material + wordOffset);
            Bin.W32(model.File, material + wordOffset, (word & ~mask) | value);
        }
    }

    public static void InitRenderObj(G3dRenderObj renderObj, G3dModel model)
    {
        renderObj.Model = model;
        renderObj.AnimObjs.Clear();
    }

    public static void AddAnimObj(G3dRenderObj renderObj, G3dAnimObj animObj)
    {
        if (!renderObj.AnimObjs.Contains(animObj))
        {
            renderObj.AnimObjs.Add(animObj);
        }
    }

    public static void RemoveAnimObj(G3dRenderObj renderObj, G3dAnimObj animObj)
    {
        renderObj.AnimObjs.Remove(animObj);
    }

    public static G3dAnimObj AllocAnimObj(G3dAllocator allocator, G3dAnim anim, G3dModel model)
    {
        return new G3dAnimObj();
    }

    public static void FreeAnimObj(G3dAllocator allocator, G3dAnimObj animObj)
    {
    }

    private static int AnimKind(G3dAnim anim)
    {
        byte[] file = anim.File;
        int offset = anim.Offset;
        char category = (char)file[offset];
        if (category == 'J')
        {
            return AnimJoint;
        }
        if (category == 'V')
        {
            return AnimVisibility;
        }
        char first = (char)file[offset + 2];
        char second = (char)file[offset + 3];
        bool hasT = first == 'T' || second == 'T';
        bool hasP = first == 'P' || second == 'P';
        if (hasT && hasP)
        {
            return AnimTexturePattern;
        }
        if (hasT)
        {
            return AnimTextureSrt;
        }
        return AnimMaterialColor;
    }

    public static void InitAnimObj(G3dAnimObj animObj, G3dAnim anim, G3dModel model, G3dTexture? texture)
    {
        animObj.ResAnm = anim;
        animObj.Frame = 0;
        animObj.Kind = AnimKind(anim);
        animObj.Binding = null;
        byte[] file = anim.File;
        int offset = anim.Offset;
        int materialCount = MaterialCount(model);
        if (animObj.Kind == AnimTextureSrt)
        {
            int[] map = new int[materialCount];
            Array.Fill(map, -1);
            int dict = offset + 8;
            int count = G3dDict.GetEntryCount(file, dict);
            for (int i = 0; i < count; i++)
            {
                int material = FindMaterialByName(model, file, DictNameOffset(file, dict, i));
                if (material >= 0)
                {
                    map[material] = G3dDict.GetEntryByIndex(file, dict, i);
                }
            }
            animObj.Binding = map;
        }
        else if (animObj.Kind == AnimTexturePattern)
        {
            PatternBinding binding = new();
            binding.MaterialData = new int[materialCount];
            Array.Fill(binding.MaterialData, -1);
            int textureCount = file[offset + 6];
            int paletteCount = file[offset + 7];
            int textureNames = offset + Bin.U16(file, offset + 8);
            int paletteNames = offset + Bin.U16(file, offset + 10);
            binding.TextureParams = new uint[textureCount];
            binding.PaletteBases = new int[paletteCount];
            if (texture != null)
            {
                for (int i = 0; i < textureCount; i++)
                {
                    TextureParams(texture, file, textureNames + i * G3dDict.NameLength, out binding.TextureParams[i], out _);
                }
                for (int i = 0; i < paletteCount; i++)
                {
                    PaletteBase(texture, file, paletteNames + i * G3dDict.NameLength, out binding.PaletteBases[i]);
                }
            }
            int dict = offset + 12;
            int count = G3dDict.GetEntryCount(file, dict);
            for (int i = 0; i < count; i++)
            {
                int material = FindMaterialByName(model, file, DictNameOffset(file, dict, i));
                if (material >= 0)
                {
                    binding.MaterialData[material] = G3dDict.GetEntryByIndex(file, dict, i);
                }
            }
            animObj.Binding = binding;
        }
    }

    private static Matrix4x4 PivotMatrix(int pivot, bool negative, bool reverseC, bool reverseD, float a, float b)
    {
        Span<float> m = stackalloc float[9];
        m.Clear();
        if (pivot > 8)
        {
            pivot = 8;
        }
        m[pivot] = negative ? -1 : 1;
        m[pivotElements[pivot, 0]] = a;
        m[pivotElements[pivot, 1]] = b;
        m[pivotElements[pivot, 2]] = reverseC ? -b : b;
        m[pivotElements[pivot, 3]] = reverseD ? -a : a;
        return new Matrix4x4(m[0], m[1], m[2], 0, m[3], m[4], m[5], 0, m[6], m[7], m[8], 0, 0, 0, 0, 1);
    }

    private static Matrix4x4 NodeMatrix(G3dModel model, int node)
    {
        byte[] file = model.File;
        int nodes = model.Offset + ModelNodes;
        int entry = G3dDict.GetEntryByIndex(file, nodes, node);
        if (entry < 0)
        {
            return Matrix4x4.Identity;
        }
        int data = nodes + Bin.I32(file, entry);
        int flag = Bin.U16(file, data);
        int cursor = data + 4;
        Vector3 translation = Vector3.Zero;
        Vector3 scale = Vector3.One;
        Matrix4x4 rotation = Matrix4x4.Identity;
        if ((flag & NodeTransZero) == 0)
        {
            translation = new Vector3(Bin.I32(file, cursor) / FixedOne, Bin.I32(file, cursor + 4) / FixedOne, Bin.I32(file, cursor + 8) / FixedOne);
            cursor += 12;
        }
        if ((flag & NodeRotZero) == 0)
        {
            if ((flag & NodePivotExists) != 0)
            {
                float a = Bin.S16(file, cursor) / FixedOne;
                float b = Bin.S16(file, cursor + 2) / FixedOne;
                rotation = PivotMatrix((flag >> 4) & 0xF, (flag & NodePivotNegative) != 0, (flag & NodeSignReverseC) != 0, (flag & NodeSignReverseD) != 0, a, b);
                cursor += 4;
            }
            else
            {
                float m00 = Bin.S16(file, data + 2) / FixedOne;
                Span<float> m = stackalloc float[8];
                for (int i = 0; i < 8; i++)
                {
                    m[i] = Bin.S16(file, cursor + i * 2) / FixedOne;
                }
                rotation = new Matrix4x4(m00, m[0], m[1], 0, m[2], m[3], m[4], 0, m[5], m[6], m[7], 0, 0, 0, 0, 1);
                cursor += 16;
            }
        }
        if ((flag & NodeScaleOne) == 0)
        {
            scale = new Vector3(Bin.I32(file, cursor) / FixedOne, Bin.I32(file, cursor + 4) / FixedOne, Bin.I32(file, cursor + 8) / FixedOne);
        }
        Matrix4x4 result = Matrix4x4.CreateScale(scale) * rotation;
        result.M41 = translation.X;
        result.M42 = translation.Y;
        result.M43 = translation.Z;
        return result;
    }

    private static float SampleTrack(byte[] file, int anim, uint info, uint value, int frame, bool scalePair)
    {
        int data = anim + (int)value;
        bool fx16 = (info & 0x20000000) != 0;
        int last = (int)((info >> 16) & 0x1FFF);
        int step = (int)(info >> 30);
        int stride = (fx16 ? 2 : 4) * (scalePair ? 2 : 1);
        float Read(int index)
        {
            int at = data + index * stride;
            if (at < 0 || at + (fx16 ? 2 : 4) > file.Length)
            {
                return 0;
            }
            return (fx16 ? Bin.S16(file, at) : Bin.I32(file, at)) / FixedOne;
        }
        if (step == 0)
        {
            return Read(frame);
        }
        int shift = step == 1 ? 1 : 2;
        int mask = (1 << shift) - 1;
        if ((frame & mask) == 0)
        {
            return Read(frame >> shift);
        }
        if (frame > last)
        {
            return Read((last >> shift) + (frame & mask));
        }
        float a = Read(frame >> shift);
        float b = Read((frame >> shift) + 1);
        float t = (frame & mask) / (float)(mask + 1);
        return a + (b - a) * t;
    }

    private static Matrix4x4 RotationByIndex(byte[] file, int anim, int index)
    {
        if ((index & 0x8000) != 0)
        {
            int data = anim + Bin.I32(file, anim + 0x0C) + (index & 0x7FFF) * 6;
            int info = Bin.U16(file, data);
            float a = Bin.S16(file, data + 2) / FixedOne;
            float b = Bin.S16(file, data + 4) / FixedOne;
            return PivotMatrix(info & 0xF, (info & 0x10) != 0, (info & 0x20) != 0, (info & 0x40) != 0, a, b);
        }
        int offset = anim + Bin.I32(file, anim + 0x10) + index * 10;
        Span<int> raw = stackalloc int[5];
        for (int i = 0; i < 5; i++)
        {
            raw[i] = Bin.S16(file, offset + i * 2);
        }
        float m00 = (raw[0] >> 3) / FixedOne;
        float m01 = (raw[1] >> 3) / FixedOne;
        float m02 = (raw[2] >> 3) / FixedOne;
        float m10 = (raw[3] >> 3) / FixedOne;
        float m11 = (raw[4] >> 3) / FixedOne;
        int packed = (raw[0] & 7) | ((raw[1] & 7) << 3) | ((raw[2] & 7) << 6) | ((raw[3] & 7) << 9) | ((raw[4] & 7) << 12);
        float m12 = ((short)(packed << 3) >> 3) / FixedOne;
        float m20 = m01 * m12 - m02 * m11;
        float m21 = m02 * m10 - m00 * m12;
        float m22 = m00 * m11 - m01 * m10;
        return new Matrix4x4(m00, m01, m02, 0, m10, m11, m12, 0, m20, m21, m22, 0, 0, 0, 0, 1);
    }

    private static Matrix4x4 SampleRotation(byte[] file, int anim, uint info, uint value, int frame)
    {
        int data = anim + (int)value;
        int last = (int)((info >> 16) & 0x1FFF);
        int step = (int)(info >> 30);
        int Index(int at)
        {
            int position = data + at * 2;
            return position >= 0 && position + 2 <= file.Length ? Bin.U16(file, position) : 0;
        }
        if (step == 0)
        {
            return RotationByIndex(file, anim, Index(frame));
        }
        int shift = step == 1 ? 1 : 2;
        int mask = (1 << shift) - 1;
        if ((frame & mask) == 0)
        {
            return RotationByIndex(file, anim, Index(frame >> shift));
        }
        if (frame > last)
        {
            return RotationByIndex(file, anim, Index((last >> shift) + (frame & mask)));
        }
        Matrix4x4 a = RotationByIndex(file, anim, Index(frame >> shift));
        Matrix4x4 b = RotationByIndex(file, anim, Index((frame >> shift) + 1));
        float t = (frame & mask) / (float)(mask + 1);
        Matrix4x4 mixed = Matrix4x4.Lerp(a, b, t);
        Vector3 x = Vector3.Normalize(new Vector3(mixed.M11, mixed.M12, mixed.M13));
        Vector3 y = new(mixed.M21, mixed.M22, mixed.M23);
        Vector3 z = Vector3.Normalize(Vector3.Cross(x, y));
        y = Vector3.Cross(z, x);
        if (float.IsNaN(x.X) || float.IsNaN(z.X))
        {
            return a;
        }
        return new Matrix4x4(x.X, x.Y, x.Z, 0, y.X, y.Y, y.Z, 0, z.X, z.Y, z.Z, 0, 0, 0, 0, 1);
    }

    private static bool JointMatrix(G3dModel model, G3dAnimObj animObj, int node, out Matrix4x4 result)
    {
        G3dAnim anim = animObj.ResAnm!;
        byte[] file = anim.File;
        int offset = anim.Offset;
        result = Matrix4x4.Identity;
        int nodeCount = Bin.U16(file, offset + 6);
        if (node >= nodeCount)
        {
            return false;
        }
        int frames = Math.Max(1, (int)anim.NumFrames);
        int frame = Math.Clamp(animObj.Frame >> 12, 0, frames - 1);
        int tagOffset = offset + Bin.U16(file, offset + 0x14 + node * 2);
        uint tag = Bin.U32(file, tagOffset);
        if ((tag & 1) != 0)
        {
            return true;
        }
        Matrix4x4 baseMatrix = NodeMatrix(model, node);
        int cursor = tagOffset + 4;
        Vector3 translation = Vector3.Zero;
        Vector3 scale = Vector3.One;
        Matrix4x4 rotation = Matrix4x4.Identity;
        if ((tag & 6) == 0)
        {
            for (int axis = 0; axis < 3; axis++)
            {
                float value;
                if ((tag & (8u << axis)) != 0)
                {
                    value = Bin.I32(file, cursor) / FixedOne;
                    cursor += 4;
                }
                else
                {
                    value = SampleTrack(file, offset, Bin.U32(file, cursor), Bin.U32(file, cursor + 4), frame, false);
                    cursor += 8;
                }
                if (axis == 0)
                {
                    translation.X = value;
                }
                else if (axis == 1)
                {
                    translation.Y = value;
                }
                else
                {
                    translation.Z = value;
                }
            }
        }
        else if ((tag & 4) != 0)
        {
            translation = new Vector3(baseMatrix.M41, baseMatrix.M42, baseMatrix.M43);
        }
        bool baseRotation = (tag & 0x80) != 0;
        if ((tag & 0xC0) == 0)
        {
            if ((tag & 0x100) != 0)
            {
                rotation = RotationByIndex(file, offset, (int)(Bin.U32(file, cursor) & 0xFFFF));
                cursor += 4;
            }
            else
            {
                rotation = SampleRotation(file, offset, Bin.U32(file, cursor), Bin.U32(file, cursor + 4), frame);
                cursor += 8;
            }
        }
        bool baseScale = (tag & 0x400) != 0;
        if ((tag & 0x600) == 0)
        {
            for (int axis = 0; axis < 3; axis++)
            {
                float value;
                if ((tag & (0x800u << axis)) != 0)
                {
                    value = Bin.I32(file, cursor) / FixedOne;
                    cursor += 8;
                }
                else
                {
                    value = SampleTrack(file, offset, Bin.U32(file, cursor), Bin.U32(file, cursor + 4), frame, true);
                    cursor += 8;
                }
                if (axis == 0)
                {
                    scale.X = value;
                }
                else if (axis == 1)
                {
                    scale.Y = value;
                }
                else
                {
                    scale.Z = value;
                }
            }
        }
        if (baseRotation || baseScale)
        {
            Vector3 baseScaleVector = new(
                new Vector3(baseMatrix.M11, baseMatrix.M12, baseMatrix.M13).Length(),
                new Vector3(baseMatrix.M21, baseMatrix.M22, baseMatrix.M23).Length(),
                new Vector3(baseMatrix.M31, baseMatrix.M32, baseMatrix.M33).Length());
            if (baseScale)
            {
                scale = baseScaleVector;
            }
            if (baseRotation)
            {
                rotation = baseMatrix;
                rotation.M41 = 0;
                rotation.M42 = 0;
                rotation.M43 = 0;
                rotation = Matrix4x4.CreateScale(
                    baseScaleVector.X != 0 ? 1 / baseScaleVector.X : 1,
                    baseScaleVector.Y != 0 ? 1 / baseScaleVector.Y : 1,
                    baseScaleVector.Z != 0 ? 1 / baseScaleVector.Z : 1) * rotation;
            }
        }
        result = Matrix4x4.CreateScale(scale) * rotation;
        result.M41 = translation.X;
        result.M42 = translation.Y;
        result.M43 = translation.Z;
        return true;
    }

    private static float SampleSrt(byte[] file, int anim, int entry, int component, int frame, out float second)
    {
        uint info = Bin.U32(file, entry + component * 8);
        uint value = Bin.U32(file, entry + component * 8 + 4);
        bool rotation = component == 2;
        second = 0;
        bool constant = (info & 0x20000000) != 0;
        bool fx16 = (info & 0x10000000) != 0;
        if (constant)
        {
            if (rotation)
            {
                second = (short)(value >> 16) / FixedOne;
                return (short)(value & 0xFFFF) / FixedOne;
            }
            return (int)value / FixedOne;
        }
        int data = anim + (int)value;
        int last = (int)(info & 0xFFFF);
        int step = (int)(info >> 30);
        int stride = rotation ? 4 : fx16 ? 2 : 4;
        float Read(int index, int part)
        {
            int at = data + index * stride + part * 2;
            if (at < 0 || at + 2 > file.Length)
            {
                return 0;
            }
            if (rotation || fx16)
            {
                return Bin.S16(file, at) / FixedOne;
            }
            return Bin.I32(file, at) / FixedOne;
        }
        int index;
        float t = 0;
        if (step == 0)
        {
            index = frame;
        }
        else
        {
            int shift = step == 1 ? 1 : 2;
            int mask = (1 << shift) - 1;
            if ((frame & mask) == 0)
            {
                index = frame >> shift;
            }
            else if (frame > last)
            {
                index = (last >> shift) + (frame & mask);
            }
            else
            {
                index = frame >> shift;
                t = (frame & mask) / (float)(mask + 1);
            }
        }
        float first = Read(index, 0);
        if (rotation)
        {
            second = Read(index, 1);
        }
        if (t != 0)
        {
            first += (Read(index + 1, 0) - first) * t;
            if (rotation)
            {
                second += (Read(index + 1, 1) - second) * t;
            }
        }
        return first;
    }

    private static Matrix4x4 TextureMatrix(float scaleS, float scaleT, float sin, float cos, float transS, float transT, float width, float height)
    {
        Matrix4x4 m = Matrix4x4.Identity;
        float ratio = width != 0 ? height / width : 1;
        m.M11 = scaleS * cos;
        m.M12 = -scaleS * sin * ratio;
        m.M21 = scaleT * sin / ratio;
        m.M22 = scaleT * cos;
        float offsetS = scaleS * ((-sin - cos + 1) * 0.5f - transS) * width;
        float offsetT = (scaleT * ((sin - cos - 1) * 0.5f + transT) + 1) * height;
        if (sin == 0 && cos == 1)
        {
            offsetS = -scaleS * transS * width;
            offsetT = (scaleT * transT - scaleT + 1) * height;
        }
        m.M41 = offsetS * 16;
        m.M42 = offsetT * 16;
        return m;
    }

    private static void ApplyMaterial(G3dRenderObj? renderObj, G3dModel model, int index)
    {
        byte[] file = model.File;
        int material = MaterialData(model, index);
        if (material < 0)
        {
            return;
        }
        int flag = Bin.U16(file, material + MatFlag);
        uint diffAmb = Bin.U32(file, material + MatDiffAmb);
        uint specEmi = Bin.U32(file, material + MatSpecEmi);
        uint polyAttr = Bin.U32(file, material + MatPolyAttr);
        uint polyAttrMask = Bin.U32(file, material + MatPolyAttrMask);
        uint texParam = Bin.U32(file, material + MatTexImageParam);
        int paletteBase = Bin.U16(file, material + MatPaletteBase);
        uint globalDiffAmb = Render3D.GlobalDiffuseAmbient;
        uint globalSpecEmi = Render3D.GlobalSpecularEmission;
        if ((flag & MatFlagDiffuse) == 0)
        {
            diffAmb = (diffAmb & ~0x7FFFu) | (globalDiffAmb & 0x7FFF);
        }
        if ((flag & MatFlagAmbient) == 0)
        {
            diffAmb = (diffAmb & ~0x7FFF0000u) | (globalDiffAmb & 0x7FFF0000);
        }
        if ((flag & MatFlagVertexColor) == 0)
        {
            diffAmb = (diffAmb & ~0x8000u) | (globalDiffAmb & 0x8000);
        }
        if ((flag & MatFlagSpecular) == 0)
        {
            specEmi = (specEmi & ~0x7FFFu) | (globalSpecEmi & 0x7FFF);
        }
        if ((flag & MatFlagEmission) == 0)
        {
            specEmi = (specEmi & ~0x7FFF0000u) | (globalSpecEmi & 0x7FFF0000);
        }
        if ((flag & MatFlagShininess) == 0)
        {
            specEmi = (specEmi & ~0x8000u) | (globalSpecEmi & 0x8000);
        }
        polyAttr = (polyAttr & polyAttrMask) | (Render3D.GlobalPolygonAttr & ~polyAttrMask);
        float width = Bin.U16(file, material + MatOrigWidth);
        float height = Bin.U16(file, material + MatOrigHeight);
        float scaleS = 1;
        float scaleT = 1;
        float sin = 0;
        float cos = 1;
        float transS = 0;
        float transT = 0;
        bool useMatrix = (flag & MatFlagTexMatrixUse) != 0;
        int cursor = material + MatTexMatrixData;
        if ((flag & MatFlagScaleOne) == 0)
        {
            scaleS = Bin.I32(file, cursor) / FixedOne;
            scaleT = Bin.I32(file, cursor + 4) / FixedOne;
            cursor += 8;
        }
        if ((flag & MatFlagRotZero) == 0)
        {
            sin = Bin.S16(file, cursor) / FixedOne;
            cos = Bin.S16(file, cursor + 2) / FixedOne;
            cursor += 4;
        }
        if ((flag & MatFlagTransZero) == 0)
        {
            transS = Bin.I32(file, cursor) / FixedOne;
            transT = Bin.I32(file, cursor + 4) / FixedOne;
        }
        if (renderObj != null)
        {
            foreach (G3dAnimObj animObj in renderObj.AnimObjs)
            {
                G3dAnim? anim = animObj.ResAnm;
                if (anim == null)
                {
                    continue;
                }
                int frames = Math.Max(1, (int)anim.NumFrames);
                int frame = Math.Clamp(animObj.Frame >> 12, 0, frames - 1);
                if (animObj.Kind == AnimTextureSrt && animObj.Binding is int[] map && index < map.Length && map[index] >= 0)
                {
                    int entry = map[index];
                    scaleS = SampleSrt(anim.File, anim.Offset, entry, 0, frame, out _);
                    scaleT = SampleSrt(anim.File, anim.Offset, entry, 1, frame, out _);
                    sin = SampleSrt(anim.File, anim.Offset, entry, 2, frame, out cos);
                    transS = SampleSrt(anim.File, anim.Offset, entry, 3, frame, out _);
                    transT = SampleSrt(anim.File, anim.Offset, entry, 4, frame, out _);
                    useMatrix = true;
                }
                else if (animObj.Kind == AnimTexturePattern && animObj.Binding is PatternBinding binding && index < binding.MaterialData.Length && binding.MaterialData[index] >= 0)
                {
                    int entry = binding.MaterialData[index];
                    int count = Bin.U16(anim.File, entry);
                    int values = anim.Offset + Bin.U16(anim.File, entry + 6);
                    int chosen = -1;
                    for (int i = 0; i < count; i++)
                    {
                        if (Bin.U16(anim.File, values + i * 4) <= frame)
                        {
                            chosen = i;
                        }
                    }
                    if (chosen >= 0)
                    {
                        int textureIndex = anim.File[values + chosen * 4 + 2];
                        int paletteIndex = anim.File[values + chosen * 4 + 3];
                        if (textureIndex < binding.TextureParams.Length && binding.TextureParams[textureIndex] != 0)
                        {
                            texParam = (texParam & TexParamKeepMask) | (binding.TextureParams[textureIndex] & ~TexParamKeepMask);
                        }
                        if (paletteIndex < binding.PaletteBases.Length)
                        {
                            paletteBase = binding.PaletteBases[paletteIndex];
                        }
                    }
                }
            }
        }
        Render3D.SetDiffuseAmbient(diffAmb);
        Render3D.SetSpecularEmission(specEmi);
        Render3D.SetPolygonAttr(polyAttr);
        if (useMatrix)
        {
            texParam = (texParam & ~0xC0000000u) | 0x40000000u;
            Render3D.TextureMatrix = TextureMatrix(scaleS, scaleT, sin, cos, transS, transT, width, height);
        }
        Render3D.SetTexImageParam(texParam);
        Render3D.SetPaletteBase((uint)paletteBase);
    }

    private static void DrawShape(G3dModel model, int index)
    {
        byte[] file = model.File;
        int shapes = model.Offset + Bin.I32(file, model.Offset + ModelShapes);
        int entry = G3dDict.GetEntryByIndex(file, shapes, index);
        if (entry < 0)
        {
            return;
        }
        int shape = shapes + Bin.I32(file, entry);
        Render3D.Execute(file, shape + Bin.I32(file, shape + 8), Bin.I32(file, shape + 12));
    }

    private static Matrix4x4 EnvelopeMatrix(G3dModel model, int node)
    {
        byte[] file = model.File;
        int data = model.Offset + Bin.I32(file, model.Offset + ModelEnvelopes) + node * 84;
        Span<float> m = stackalloc float[12];
        for (int i = 0; i < 12; i++)
        {
            m[i] = Bin.I32(file, data + i * 4) / FixedOne;
        }
        return new Matrix4x4(m[0], m[1], m[2], 0, m[3], m[4], m[5], 0, m[6], m[7], m[8], 0, m[9], m[10], m[11], 1);
    }

    private static void Billboard(bool keepY)
    {
        Matrix4x4 m = Render3D.PositionMatrix;
        Vector3 x = new(m.M11, m.M12, m.M13);
        Vector3 y = new(m.M21, m.M22, m.M23);
        Vector3 z = new(m.M31, m.M32, m.M33);
        float sx = x.Length();
        float sy = y.Length();
        float sz = z.Length();
        Vector3 newX = new(1, 0, 0);
        Vector3 newY = new(0, 1, 0);
        Vector3 newZ = new(0, 0, 1);
        if (keepY && sy > 0)
        {
            newY = y / sy;
            Vector3 cross = Vector3.Cross(newY, new Vector3(0, 0, 1));
            if (cross.LengthSquared() > 1e-8f)
            {
                newX = Vector3.Normalize(cross);
                newZ = Vector3.Cross(newX, newY);
            }
        }
        newX *= sx;
        newY *= sy;
        newZ *= sz;
        m.M11 = newX.X;
        m.M12 = newX.Y;
        m.M13 = newX.Z;
        m.M21 = newY.X;
        m.M22 = newY.Y;
        m.M23 = newY.Z;
        m.M31 = newZ.X;
        m.M32 = newZ.Y;
        m.M33 = newZ.Z;
        Render3D.PositionMatrix = m;
    }

    private static bool NodeVisible(G3dRenderObj renderObj, int node, bool visible)
    {
        foreach (G3dAnimObj animObj in renderObj.AnimObjs)
        {
            G3dAnim? anim = animObj.ResAnm;
            if (anim == null || animObj.Kind != AnimVisibility)
            {
                continue;
            }
            byte[] file = anim.File;
            int offset = anim.Offset;
            int nodeCount = Bin.U16(file, offset + 6);
            int frames = Math.Max(1, (int)anim.NumFrames);
            int frame = Math.Clamp(animObj.Frame >> 12, 0, frames - 1);
            int bit = frame * nodeCount + node;
            int at = offset + 12 + (bit >> 5) * 4;
            if (node < nodeCount && at + 4 <= file.Length)
            {
                visible = (Bin.U32(file, at) & (1u << (bit & 31))) != 0;
            }
        }
        return visible;
    }

    public static void Draw(G3dRenderObj renderObj)
    {
        G3dModel? model = renderObj.Model;
        if (model == null)
        {
            return;
        }
        byte[] file = model.File;
        int sbc = model.Offset + Bin.I32(file, model.Offset + ModelSbc);
        int end = model.Offset + Bin.I32(file, model.Offset + ModelMaterials);
        float posScale = Bin.I32(file, model.Offset + ModelPosScale) / FixedOne;
        float invPosScale = Bin.I32(file, model.Offset + ModelInvPosScale) / FixedOne;
        G3dAnimObj? joint = null;
        foreach (G3dAnimObj animObj in renderObj.AnimObjs)
        {
            if (animObj.Kind == AnimJoint && animObj.ResAnm != null)
            {
                joint = animObj;
            }
        }
        bool visible = true;
        int cursor = sbc;
        int guard = 0;
        while (cursor < end && cursor < file.Length && guard++ < 65536)
        {
            int op = file[cursor];
            int command = op & 0x1F;
            int option = op >> 5;
            switch (command)
            {
                case 0:
                    cursor += 1;
                    break;
                case 1:
                    return;
                case 2:
                    visible = NodeVisible(renderObj, file[cursor + 1], (file[cursor + 2] & 1) != 0);
                    cursor += 3;
                    break;
                case 3:
                    if (visible)
                    {
                        Render3D.RestoreMatrix(file[cursor + 1]);
                    }
                    cursor += 2;
                    break;
                case 4:
                    if (visible)
                    {
                        ApplyMaterial(renderObj, model, file[cursor + 1]);
                    }
                    cursor += 2;
                    break;
                case 5:
                    if (visible)
                    {
                        DrawShape(model, file[cursor + 1]);
                    }
                    cursor += 2;
                    break;
                case 6:
                {
                    int node = file[cursor + 1];
                    int extra = cursor + 4;
                    int store = -1;
                    int restore = -1;
                    if ((option & 1) != 0)
                    {
                        store = file[extra++];
                    }
                    if ((option & 2) != 0)
                    {
                        restore = file[extra++];
                    }
                    if (restore >= 0)
                    {
                        Render3D.RestoreMatrix(restore);
                    }
                    Matrix4x4 local;
                    if (joint == null || !JointMatrix(model, joint, node, out local))
                    {
                        local = NodeMatrix(model, node);
                    }
                    Render3D.Multiply(local);
                    if (store >= 0)
                    {
                        Render3D.StoreMatrix(store);
                    }
                    cursor = extra;
                    break;
                }
                case 7:
                case 8:
                {
                    int extra = cursor + 2;
                    int store = -1;
                    int restore = -1;
                    if ((option & 1) != 0)
                    {
                        store = file[extra++];
                    }
                    if ((option & 2) != 0)
                    {
                        restore = file[extra++];
                    }
                    if (restore >= 0)
                    {
                        Render3D.RestoreMatrix(restore);
                    }
                    Billboard(command == 8);
                    if (store >= 0)
                    {
                        Render3D.StoreMatrix(store);
                    }
                    cursor = extra;
                    break;
                }
                case 9:
                {
                    int store = file[cursor + 1];
                    int count = file[cursor + 2];
                    Matrix4x4 sum = default;
                    for (int i = 0; i < count; i++)
                    {
                        int source = file[cursor + 3 + i * 3];
                        int node = file[cursor + 4 + i * 3];
                        float ratio = file[cursor + 5 + i * 3] / 256f;
                        Matrix4x4 term = EnvelopeMatrix(model, node) * Render3D.StackPosition(source);
                        sum += term * ratio;
                    }
                    sum.M14 = 0;
                    sum.M24 = 0;
                    sum.M34 = 0;
                    sum.M44 = 1;
                    Render3D.PositionMatrix = sum;
                    Matrix4x4 rotation = sum;
                    rotation.M41 = 0;
                    rotation.M42 = 0;
                    rotation.M43 = 0;
                    Render3D.VectorMatrix = rotation;
                    Render3D.StoreMatrix(store);
                    cursor += 3 + count * 3;
                    break;
                }
                case 10:
                    if (visible)
                    {
                        int address = cursor + Bin.I32(file, cursor + 1);
                        Render3D.Execute(file, address, Bin.I32(file, cursor + 5));
                    }
                    cursor += 9;
                    break;
                case 11:
                {
                    float scale = (option & 1) != 0 ? invPosScale : posScale;
                    Render3D.PositionMatrix = Matrix4x4.CreateScale(scale) * Render3D.PositionMatrix;
                    cursor += 1;
                    break;
                }
                case 12:
                case 13:
                    cursor += 3;
                    break;
                default:
                    return;
            }
        }
    }

    public static void Draw1Mat1Shp(G3dModel model, uint materialIndex, uint shapeIndex, bool sendMaterial)
    {
        if (sendMaterial)
        {
            ApplyMaterial(null, model, (int)materialIndex);
        }
        float posScale = Bin.I32(model.File, model.Offset + ModelPosScale) / FixedOne;
        Matrix4x4 saved = Render3D.PositionMatrix;
        Render3D.PositionMatrix = Matrix4x4.CreateScale(posScale) * saved;
        DrawShape(model, (int)shapeIndex);
        Render3D.PositionMatrix = saved;
    }
}
