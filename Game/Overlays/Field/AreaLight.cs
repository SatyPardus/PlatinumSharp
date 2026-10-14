namespace PlatinumSharp.Game;

public sealed class AreaLightTemplate
{
    public const int Size = 0x30;

    public uint EndTime;
    public byte ValidLightsMask;
    public readonly ushort[] LightColors = new ushort[ModelAttributes.LightCount];
    public readonly VecFx16[] LightVectors = new VecFx16[ModelAttributes.LightCount];
    public ushort DiffuseReflectColor;
    public ushort AmbientReflectColor;
    public ushort SpecularReflectColor;
    public ushort EmissionColor;
}

public sealed class AreaLightManager
{
    public const int Size = 0x14;

    public uint TemplateCount;
    public AreaLightTemplate[]? Templates;
    public int ActiveTemplateIndex;
    public ModelAttributes? AreaModelAttributes;
    public int ApplyToAreaModelAttributes;
}

public static class AreaLight
{
    private const int HeapIDField = 4;
    private const int NarcAreaLight = 0xB0;
    private const int ArchiveCount = 4;
    private const int LineBufferSize = 0x100;
    private const int LinesPerTemplate = 10;
    private const byte LineTerminator = 0x0D;
    private const byte ValueSeparator = 0x2C;
    private const byte EndMarker0 = 0x45;
    private const byte EndMarker1 = 0x4F;
    private const byte EndMarker2 = 0x46;
    private const ushort InvalidLightColor = 0xFFFF;
    private const int LightVectorMax = 0x1000;
    private const int LightVectorMin = -0x1000;
    private const int MaterialFlagDiffuse = 0x40;
    private const int MaterialFlagAmbient = 0x80;
    private const int MaterialFlagSpecular = 0x200;
    private const int MaterialFlagEmission = 0x400;

    // OFFSET 0x021D521C
    public static AreaLightManager AreaLightManager_New(ModelAttributes? areaModelAttributes, byte archiveID)
    {
        if (archiveID >= ArchiveCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        AreaLightManager manager = new();
        manager.AreaModelAttributes = areaModelAttributes;
        manager.TemplateCount = AreaLight_LoadTemplates(archiveID, out manager.Templates);
        manager.ActiveTemplateIndex = 0;
        int halfSeconds = Rtc.GetSecondsSinceMidnight() / 2;
        for (uint i = 0; i < manager.TemplateCount; i++)
        {
            if (manager.Templates![i].EndTime > (uint)halfSeconds)
            {
                manager.ActiveTemplateIndex = (int)i;
                break;
            }
        }
        manager.ApplyToAreaModelAttributes = 1;
        AreaLightManager_ApplyActiveTemplate(manager);
        return manager;
    }

    // OFFSET 0x021D5278
    public static void AreaLightManager_Free(ref AreaLightManager? areaLightManager)
    {
        AreaLight_FreeTemplates(ref areaLightManager!.Templates);
        areaLightManager = null;
    }

    // OFFSET 0x021D5298
    public static void AreaLightManager_UpdateActiveTemplate(AreaLightManager? areaLightManager)
    {
        if (areaLightManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        AreaLightManager manager = areaLightManager!;
        int halfSeconds = Rtc.GetSecondsSinceMidnight() / 2;
        if (manager.TemplateCount <= 1)
        {
            return;
        }
        int previousEndTime;
        if (manager.ActiveTemplateIndex - 1 >= 0)
        {
            previousEndTime = (int)manager.Templates![manager.ActiveTemplateIndex - 1].EndTime;
        }
        else
        {
            previousEndTime = 0;
        }
        int endTime = (int)manager.Templates![manager.ActiveTemplateIndex].EndTime;
        if (halfSeconds < endTime && halfSeconds >= previousEndTime)
        {
            return;
        }
        manager.ActiveTemplateIndex++;
        if ((uint)manager.ActiveTemplateIndex >= manager.TemplateCount)
        {
            manager.ActiveTemplateIndex = 0;
        }
        if (manager.ApplyToAreaModelAttributes != 0)
        {
            AreaLightManager_ApplyActiveTemplate(manager);
        }
    }

    // OFFSET 0x021D52F4
    public static void AreaLightTemplate_ApplyToModelAttributes(AreaLightTemplate template, ModelAttributes modelAttributes)
    {
        for (int i = 0; i < ModelAttributes.LightCount; i++)
        {
            if ((template.ValidLightsMask & (1 << i)) != 0)
            {
                ModelAttributes.ModelAttributes_SetLightVector(modelAttributes, i, template.LightVectors[i].X, template.LightVectors[i].Y, template.LightVectors[i].Z);
                ModelAttributes.ModelAttributes_SetLightColor(modelAttributes, i, template.LightColors[i]);
            }
            else
            {
                ModelAttributes.ModelAttributes_SetLightVector(modelAttributes, i, 0, 0, 0);
                ModelAttributes.ModelAttributes_SetLightColor(modelAttributes, i, 0);
            }
        }
        ModelAttributes.ModelAttributes_SetDiffuseReflection(modelAttributes, template.DiffuseReflectColor, 0, 0);
        ModelAttributes.ModelAttributes_SetAmbientReflection(modelAttributes, template.AmbientReflectColor, 1);
        ModelAttributes.ModelAttributes_SetSpecularReflection(modelAttributes, template.SpecularReflectColor, 0, 0);
        ModelAttributes.ModelAttributes_SetEmissionColor(modelAttributes, template.EmissionColor, 1);
    }

    // OFFSET 0x021D538C
    private static void AreaLightManager_ApplyActiveTemplate(AreaLightManager manager)
    {
        AreaLightTemplate_ApplyToModelAttributes(manager.Templates![manager.ActiveTemplateIndex], manager.AreaModelAttributes!);
    }

    // OFFSET 0x021D53A4
    public static void AreaLight_UseGlobalModelAttributes(G3dModel model)
    {
        G3d.SetMaterialFlags(model, false, MaterialFlagDiffuse);
        G3d.SetMaterialFlags(model, false, MaterialFlagAmbient);
        G3d.SetMaterialFlags(model, false, MaterialFlagSpecular);
        G3d.SetMaterialFlags(model, false, MaterialFlagEmission);
    }

    private static bool IsEndMarker(byte[] line)
    {
        return line[0] == EndMarker0 && line[1] == EndMarker1 && line[2] == EndMarker2;
    }

    // OFFSET 0x021D53D4
    private static uint AreaLight_LoadTemplates(int archiveID, out AreaLightTemplate[]? templates)
    {
        byte[] line = new byte[LineBufferSize];
        byte[] value = new byte[LineBufferSize];
        byte[] file = Graphics.LoadMemberFromNARC(NarcAreaLight, (uint)archiveID, 0, HeapIDField, 0)!;
        Array.Resize(ref file, file.Length + LineBufferSize);
        int count = 0;
        int position = 0;
        do
        {
            position = AsciiUtil.Ascii_CopyToTerminator(file, position, line, LineTerminator);
            if (!IsEndMarker(line))
            {
                for (int i = 1; i < LinesPerTemplate; i++)
                {
                    position = AsciiUtil.Ascii_CopyToTerminator(file, position, line, LineTerminator);
                }
                count++;
            }
        }
        while (!IsEndMarker(line));
        templates = new AreaLightTemplate[count];
        for (int i = 0; i < count; i++)
        {
            templates[i] = new AreaLightTemplate();
        }
        position = 0;
        for (int i = 0; i < count; i++)
        {
            AreaLightTemplate template = templates[i];
            position = AsciiUtil.Ascii_CopyToTerminator(file, position, line, LineTerminator);
            AsciiUtil.Ascii_CopyToTerminator(line, 0, value, ValueSeparator);
            template.EndTime = (uint)AsciiUtil.Ascii_ConvertToInt(value);
            for (int light = 0; light < ModelAttributes.LightCount; light++)
            {
                position = AreaLight_ReadLight(file, position, out template.LightColors[light], ref template.LightVectors[light]);
                if (template.LightColors[light] != InvalidLightColor)
                {
                    template.ValidLightsMask |= (byte)(1 << light);
                }
                else
                {
                    template.LightColors[light] = 0;
                }
            }
            position = AreaLight_ReadColor(file, position, out template.DiffuseReflectColor);
            position = AreaLight_ReadColor(file, position, out template.AmbientReflectColor);
            position = AreaLight_ReadColor(file, position, out template.SpecularReflectColor);
            position = AreaLight_ReadColor(file, position, out template.EmissionColor);
            position = AsciiUtil.Ascii_CopyToTerminator(file, position, line, LineTerminator);
        }
        return (uint)count;
    }

    // OFFSET 0x021D555C
    private static void AreaLight_FreeTemplates(ref AreaLightTemplate[]? templates)
    {
        templates = null;
    }

    // OFFSET 0x021D5570
    private static int AreaLight_ReadLight(byte[] file, int position, out ushort lightColor, ref VecFx16 lightVector)
    {
        byte[] line = new byte[LineBufferSize];
        byte[] value = new byte[LineBufferSize];
        position = AsciiUtil.Ascii_CopyToTerminator(file, position, line, LineTerminator);
        int linePosition = AsciiUtil.Ascii_CopyToTerminator(line, 0, value, ValueSeparator);
        if (AsciiUtil.Ascii_ConvertToInt(value) == 1)
        {
            ushort[] channels = new ushort[3];
            for (int i = 0; i < 3; i++)
            {
                linePosition = AsciiUtil.Ascii_CopyToTerminator(line, linePosition, value, ValueSeparator);
                channels[i] = (ushort)AsciiUtil.Ascii_ConvertToInt(value);
            }
            lightColor = (ushort)((channels[1] << 5) | channels[0] | (channels[2] << 10));
            int[] components = new int[3];
            for (int i = 0; i < 3; i++)
            {
                linePosition = AsciiUtil.Ascii_CopyToTerminator(line, linePosition, value, ValueSeparator);
                components[i] = AsciiUtil.Ascii_ConvertToInt(value);
            }
            lightVector.X = (short)components[0];
            lightVector.Y = (short)components[1];
            lightVector.Z = (short)components[2];
            if (lightVector.X > LightVectorMax)
            {
                lightVector.X = LightVectorMax;
            }
            if (lightVector.X < LightVectorMin)
            {
                lightVector.X = LightVectorMin;
            }
            if (lightVector.Y > LightVectorMax)
            {
                lightVector.Y = LightVectorMax;
            }
            if (lightVector.Y < LightVectorMin)
            {
                lightVector.Y = LightVectorMin;
            }
            if (lightVector.Z > LightVectorMax)
            {
                lightVector.Z = LightVectorMax;
            }
            if (lightVector.Z < LightVectorMin)
            {
                lightVector.Z = LightVectorMin;
            }
        }
        else
        {
            lightColor = InvalidLightColor;
        }
        return position;
    }

    // OFFSET 0x021D5660
    private static int AreaLight_ReadColor(byte[] file, int position, out ushort color)
    {
        byte[] line = new byte[LineBufferSize];
        byte[] value = new byte[LineBufferSize];
        position = AsciiUtil.Ascii_CopyToTerminator(file, position, line, LineTerminator);
        int linePosition = 0;
        ushort[] channels = new ushort[3];
        for (int i = 0; i < 3; i++)
        {
            linePosition = AsciiUtil.Ascii_CopyToTerminator(line, linePosition, value, ValueSeparator);
            channels[i] = (ushort)AsciiUtil.Ascii_ConvertToInt(value);
        }
        color = (ushort)((channels[2] << 10) | (channels[1] << 5) | channels[0]);
        return position;
    }
}
