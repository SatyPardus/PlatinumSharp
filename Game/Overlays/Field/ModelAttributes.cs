namespace PlatinumSharp.Game;

public sealed class ModelAttributes
{
    public const int LightCount = 4;
    public const int LightVector1 = 1 << 0;
    public const int LightVector2 = 1 << 1;
    public const int LightVector3 = 1 << 2;
    public const int LightVector4 = 1 << 3;
    public const int LightColor1 = 1 << 4;
    public const int LightColor2 = 1 << 5;
    public const int LightColor3 = 1 << 6;
    public const int LightColor4 = 1 << 7;
    public const int DiffuseReflectColor = 1 << 8;
    public const int AmbientReflectColor = 1 << 9;
    public const int SpecularReflectColor = 1 << 10;
    public const int EmissionColor = 1 << 11;
    public const int LightEnableFlags = 1 << 12;
    public const int PolygonAttrs = 1 << 12;
    public const int PolygonModeAttr = 1 << 13;
    public const int CullModeAttr = 1 << 14;
    public const int PolygonIDAttr = 1 << 15;
    public const int AlphaAttr = 1 << 16;
    public const int MiscFog = 1 << 17;
    public const int MiscDepthTestDecal = 1 << 18;
    public const int MiscDisp1Dot = 1 << 19;
    public const int MiscFarClipping = 1 << 20;
    public const int MiscXluDepthUpdate = 1 << 21;
    public const int LastBit = 1 << 22;
    public const int Size = 0x48;

    private const int MaterialDiffuseAmbient = 0x04;
    private const int MaterialSpecularEmission = 0x08;
    private const int MaterialPolygonAttr = 0x0C;
    private const uint ColorLowMask = 0x00007FFF;
    private const uint ColorHighMask = 0x7FFF0000;
    private const uint PolygonLightMask = 0x0000000F;
    private const uint PolygonModeMask = 0x00000030;
    private const uint PolygonCullMask = 0x000000C0;
    private const uint PolygonIDMask = 0x3F000000;
    private const uint PolygonAlphaMask = 0x001F0000;
    private const uint PolygonFog = 0x8000;
    private const uint PolygonDepthTestDecal = 0x4000;
    private const uint PolygonDisp1Dot = 0x2000;
    private const uint PolygonFarClipping = 0x1000;
    private const uint PolygonXluDepthUpdate = 0x0800;

    public readonly VecFx16[] LightVectors = new VecFx16[LightCount];
    public readonly ushort[] LightColors = new ushort[LightCount];
    public ushort DiffuseReflect;
    public ushort AmbientReflect;
    public ushort SpecularReflect;
    public ushort Emission;
    public int SetDiffuseColorAsVertexColor;
    public int EnableSpecularReflectShininessTable;
    public int EnabledLightsMask;
    public int PolygonMode;
    public int CullMode;
    public int PolygonID;
    public int Alpha;
    public int MiscFlags;

    // OFFSET 0x021D5878
    public static ModelAttributes ModelAttributes_New()
    {
        return new ModelAttributes();
    }

    // OFFSET 0x021D5894
    public static void ModelAttributes_Free(ref ModelAttributes? modelAttributes)
    {
        modelAttributes = null;
    }

    // OFFSET 0x021D58A8
    public static void ModelAttributes_ApplyGlobal(ModelAttributes modelAttributes, int attributes)
    {
        for (int i = 0; i < LightCount; i++)
        {
            if ((attributes & (1 << i)) != 0)
            {
                Render3D.SetGlobalLightVector(i, modelAttributes.LightVectors[i].X, modelAttributes.LightVectors[i].Y, modelAttributes.LightVectors[i].Z);
            }
            if ((attributes & (1 << (i + 4))) != 0)
            {
                Render3D.SetGlobalLightColor(i, modelAttributes.LightColors[i]);
            }
        }
        if ((attributes & (DiffuseReflectColor | AmbientReflectColor)) != 0)
        {
            Render3D.SetGlobalDiffuseAmbient(modelAttributes.DiffuseReflect, modelAttributes.AmbientReflect, modelAttributes.SetDiffuseColorAsVertexColor != 0);
        }
        if ((attributes & (SpecularReflectColor | EmissionColor)) != 0)
        {
            Render3D.SetGlobalSpecularEmission(modelAttributes.SpecularReflect, modelAttributes.Emission, modelAttributes.EnableSpecularReflectShininessTable != 0);
        }
        if (attributes >= PolygonAttrs)
        {
            Render3D.SetGlobalPolygonAttr(modelAttributes.EnabledLightsMask, modelAttributes.PolygonMode, modelAttributes.CullMode, modelAttributes.PolygonID, modelAttributes.Alpha, modelAttributes.MiscFlags);
        }
    }

    // OFFSET 0x021D5948
    public static void ModelAttributes_ApplyToModel(ModelAttributes modelAttributes, G3dModel model, int attributes)
    {
        if ((attributes & DiffuseReflectColor) != 0)
        {
            G3d.SetMaterialParam(model, MaterialDiffuseAmbient, ColorLowMask, modelAttributes.DiffuseReflect);
        }
        if ((attributes & AmbientReflectColor) != 0)
        {
            G3d.SetMaterialParam(model, MaterialDiffuseAmbient, ColorHighMask, (uint)modelAttributes.AmbientReflect << 16);
        }
        if ((attributes & SpecularReflectColor) != 0)
        {
            G3d.SetMaterialParam(model, MaterialSpecularEmission, ColorLowMask, modelAttributes.SpecularReflect);
        }
        if ((attributes & EmissionColor) != 0)
        {
            G3d.SetMaterialParam(model, MaterialSpecularEmission, ColorHighMask, (uint)modelAttributes.Emission << 16);
        }
        if ((attributes & LightEnableFlags) != 0)
        {
            G3d.SetMaterialParam(model, MaterialPolygonAttr, PolygonLightMask, (uint)modelAttributes.EnabledLightsMask);
        }
        if ((attributes & PolygonModeAttr) != 0)
        {
            G3d.SetMaterialParam(model, MaterialPolygonAttr, PolygonModeMask, (uint)modelAttributes.PolygonMode << 4);
        }
        if ((attributes & CullModeAttr) != 0)
        {
            G3d.SetMaterialParam(model, MaterialPolygonAttr, PolygonCullMask, (uint)modelAttributes.CullMode << 6);
        }
        if ((attributes & PolygonIDAttr) != 0)
        {
            G3d.SetMaterialParam(model, MaterialPolygonAttr, PolygonIDMask, (uint)modelAttributes.PolygonID << 24);
        }
        if ((attributes & AlphaAttr) != 0)
        {
            G3d.SetMaterialParam(model, MaterialPolygonAttr, PolygonAlphaMask, (uint)modelAttributes.Alpha << 16);
        }
        if ((attributes & MiscFog) != 0)
        {
            G3d.SetMaterialParam(model, MaterialPolygonAttr, PolygonFog, (uint)modelAttributes.MiscFlags & PolygonFog);
        }
        if ((attributes & MiscDepthTestDecal) != 0)
        {
            G3d.SetMaterialParam(model, MaterialPolygonAttr, PolygonDepthTestDecal, (uint)modelAttributes.MiscFlags & PolygonDepthTestDecal);
        }
        if ((attributes & MiscDisp1Dot) != 0)
        {
            G3d.SetMaterialParam(model, MaterialPolygonAttr, PolygonDisp1Dot, (uint)modelAttributes.MiscFlags & PolygonDisp1Dot);
        }
        if ((attributes & MiscFarClipping) != 0)
        {
            G3d.SetMaterialParam(model, MaterialPolygonAttr, PolygonFarClipping, (uint)modelAttributes.MiscFlags & PolygonFarClipping);
        }
        if ((attributes & MiscXluDepthUpdate) != 0)
        {
            G3d.SetMaterialParam(model, MaterialPolygonAttr, PolygonXluDepthUpdate, (uint)modelAttributes.MiscFlags & PolygonXluDepthUpdate);
        }
    }

    // OFFSET 0x021D5A50
    public static void ModelAttributes_SetLightVector(ModelAttributes modelAttributes, int index, short x, short y, short z)
    {
        modelAttributes.LightVectors[index].X = x;
        modelAttributes.LightVectors[index].Y = y;
        modelAttributes.LightVectors[index].Z = z;
        Render3D.SetGlobalLightVector(index, modelAttributes.LightVectors[index].X, modelAttributes.LightVectors[index].Y, modelAttributes.LightVectors[index].Z);
    }

    // OFFSET 0x021D5A78
    public static void ModelAttributes_SetLightColor(ModelAttributes modelAttributes, int index, ushort lightColor)
    {
        modelAttributes.LightColors[index] = lightColor;
        Render3D.SetGlobalLightColor(index, modelAttributes.LightColors[index]);
    }

    // OFFSET 0x021D5A8C
    public static void ModelAttributes_SetDiffuseReflection(ModelAttributes modelAttributes, ushort diffuseReflectColor, int setDiffuseColorAsVertexColor, int applyGlobal)
    {
        modelAttributes.DiffuseReflect = diffuseReflectColor;
        modelAttributes.SetDiffuseColorAsVertexColor = setDiffuseColorAsVertexColor;
        if (applyGlobal == 1)
        {
            ModelAttributes_ApplyGlobal(modelAttributes, DiffuseReflectColor);
        }
    }

    // OFFSET 0x021D5AA0
    public static void ModelAttributes_SetAmbientReflection(ModelAttributes modelAttributes, ushort ambientReflectColor, int applyGlobal)
    {
        modelAttributes.AmbientReflect = ambientReflectColor;
        if (applyGlobal == 1)
        {
            ModelAttributes_ApplyGlobal(modelAttributes, AmbientReflectColor);
        }
    }

    // OFFSET 0x021D5AB4
    public static void ModelAttributes_SetSpecularReflection(ModelAttributes modelAttributes, ushort specularReflectColor, int enableSpecularReflectShininessTable, int applyGlobal)
    {
        modelAttributes.SpecularReflect = specularReflectColor;
        modelAttributes.EnableSpecularReflectShininessTable = enableSpecularReflectShininessTable;
        if (applyGlobal == 1)
        {
            ModelAttributes_ApplyGlobal(modelAttributes, SpecularReflectColor);
        }
    }

    // OFFSET 0x021D5AC8
    public static void ModelAttributes_SetEmissionColor(ModelAttributes modelAttributes, ushort emissionColor, int applyGlobal)
    {
        modelAttributes.Emission = emissionColor;
        if (applyGlobal == 1)
        {
            ModelAttributes_ApplyGlobal(modelAttributes, EmissionColor);
        }
    }

    // OFFSET 0x021D5ADC
    public static void ModelAttributes_SetPolygonMode(ModelAttributes modelAttributes, int polygonMode, int applyGlobal)
    {
        modelAttributes.PolygonMode = polygonMode;
        if (applyGlobal == 1)
        {
            ModelAttributes_ApplyGlobal(modelAttributes, PolygonModeAttr);
        }
    }

    // OFFSET 0x021D5AF0
    public static void ModelAttributes_SetCullMode(ModelAttributes modelAttributes, int cullMode, int applyGlobal)
    {
        modelAttributes.CullMode = cullMode;
        if (applyGlobal == 1)
        {
            ModelAttributes_ApplyGlobal(modelAttributes, CullModeAttr);
        }
    }

    // OFFSET 0x021D5B04
    public static void ModelAttributes_SetAlpha(ModelAttributes modelAttributes, int alpha, int applyGlobal)
    {
        modelAttributes.Alpha = alpha;
        if (applyGlobal == 1)
        {
            ModelAttributes_ApplyGlobal(modelAttributes, AlphaAttr);
        }
    }

    // OFFSET 0x021D5B18
    public static void ModelAttributes_SetMiscAttrEnabled(ModelAttributes modelAttributes, int miscFlagsMask, int enable, int applyGlobal)
    {
        if (enable == 1)
        {
            if ((modelAttributes.MiscFlags & miscFlagsMask) != 0)
            {
                return;
            }
        }
        else if ((modelAttributes.MiscFlags & miscFlagsMask) == 0)
        {
            return;
        }
        modelAttributes.MiscFlags ^= miscFlagsMask;
        if (applyGlobal == 1)
        {
            ModelAttributes_ApplyGlobal(modelAttributes, PolygonAttrs);
        }
    }
}
