namespace PlatinumSharp.Game;

public sealed class FogManager
{
    public const int ParameterEnabled = 1 << 0;
    public const int ParameterMode = 1 << 1;
    public const int ParameterSlope = 1 << 2;
    public const int ParameterOffset = 1 << 3;
    public const int ParameterColor = 1 << 4;
    public const int ParameterAlpha = 1 << 5;
    public const int ParameterAll = -1;
    public const int DensityTableSize = 32;
    public const int Size = 0x38;

    public int Enabled;
    public int Mode;
    public int Slope;
    public int Offset;
    public ushort Color;
    public int Alpha;
    public readonly byte[] DensityTable = new byte[DensityTableSize];

    // OFFSET 0x021D57BC
    public static FogManager FogManager_New()
    {
        return new FogManager();
    }

    // OFFSET 0x021D57D8
    public static void FogManager_Free(ref FogManager? fogManager)
    {
        fogManager = null;
    }

    // OFFSET 0x021D57EC
    public static int FogManager_IsEnabled(FogManager fogManager)
    {
        return fogManager.Enabled;
    }

    // OFFSET 0x021D57F0
    public static int FogManager_GetSlope(FogManager fogManager)
    {
        return fogManager.Slope;
    }

    // OFFSET 0x021D57F4
    public static int FogManager_GetOffset(FogManager fogManager)
    {
        return fogManager.Offset;
    }

    // OFFSET 0x021D57F8
    public static ushort FogManager_GetColor(FogManager fogManager)
    {
        return fogManager.Color;
    }

    // OFFSET 0x021D57FC
    public static void FogManager_ApplyParameters(FogManager fogManager, int parameterFlags, int enabled, int mode, int slope, int offset)
    {
        if ((parameterFlags & ParameterEnabled) != 0)
        {
            fogManager.Enabled = enabled;
        }
        if ((parameterFlags & ParameterMode) != 0)
        {
            fogManager.Mode = mode;
        }
        if ((parameterFlags & ParameterSlope) != 0)
        {
            fogManager.Slope = slope;
        }
        if ((parameterFlags & ParameterOffset) != 0)
        {
            fogManager.Offset = offset;
        }
        Render3D.SetFog(fogManager.Enabled != 0, fogManager.Mode, fogManager.Slope, fogManager.Offset);
    }

    // OFFSET 0x021D5834
    public static void FogManager_ApplyColor(FogManager fogManager, int parameterFlags, ushort color, int alpha)
    {
        if ((parameterFlags & ParameterColor) != 0)
        {
            fogManager.Color = color;
        }
        if ((parameterFlags & ParameterAlpha) != 0)
        {
            fogManager.Alpha = alpha;
        }
        Render3D.SetFogColor(fogManager.Color | ((uint)fogManager.Alpha << 16));
    }

    // OFFSET 0x021D585C
    public static void FogManager_ApplyDensityTable(FogManager fogManager, byte[] densityTable)
    {
        Array.Copy(densityTable, fogManager.DensityTable, DensityTableSize);
        uint[] table = new uint[DensityTableSize / 4];
        for (int i = 0; i < table.Length; i++)
        {
            table[i] = (uint)(fogManager.DensityTable[i * 4] | (fogManager.DensityTable[i * 4 + 1] << 8) | (fogManager.DensityTable[i * 4 + 2] << 16) | (fogManager.DensityTable[i * 4 + 3] << 24));
        }
        Render3D.SetFogTable(table);
    }
}
