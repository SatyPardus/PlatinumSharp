using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed partial class FieldMapContext
{
    public const int DynamicMapFeaturesDataOffset = 0x24;

    public object? DynamicMapFeaturesData;
}

public sealed class VillaFurnitureLayout
{
    public const int Size = 0x18;

    public ushort FurnitureID;
    public ushort HasCollision;
    public VecFx32 Position;
    public short X1;
    public short Z1;
    public short X2;
    public short Z2;
}

public sealed class VillaLoadedProp
{
    public ushort Loaded;
    public ushort PropIndex;
    public int ModelID;
    public MapProp? MapProp;
}

public sealed class VillaMapFeatures
{
    public const int PropCount = 23;

    public FieldSystem? FieldSystem;
    public byte[]? PersistedFeatures;
    public readonly VillaLoadedProp[] Props = new VillaLoadedProp[PropCount];

    public VillaMapFeatures()
    {
        for (int i = 0; i < PropCount; i++)
        {
            Props[i] = new VillaLoadedProp();
        }
    }
}

public static class VillaMap
{
    private const int OverlayID = 5;
    private const uint FurnitureModelsAddress = 0x0220188C;
    private const uint FurnitureScriptsAddress = 0x022018DC;
    private const uint FurnitureLayoutAddress = 0x0220192C;
    private const int FurnitureCount = 20;
    private const int LayoutCount = VillaMapFeatures.PropCount;
    private const int ScriptFacingNorthOnly = 0x2774;
    private const int DirNorth = 0;

    private static uint[]? sFurnitureModels;
    private static uint[]? sFurnitureScripts;
    private static VillaFurnitureLayout[]? sFurnitureLayout;

    private static uint[] FurnitureModels => sFurnitureModels ??= RomData.OverlayU32(OverlayID, FurnitureModelsAddress, FurnitureCount);

    private static uint[] FurnitureScripts => sFurnitureScripts ??= RomData.OverlayU32(OverlayID, FurnitureScriptsAddress, FurnitureCount);

    private static VillaFurnitureLayout[] FurnitureLayout
    {
        get
        {
            if (sFurnitureLayout == null)
            {
                byte[] data = RomData.Overlay(OverlayID, FurnitureLayoutAddress, LayoutCount * VillaFurnitureLayout.Size);
                VillaFurnitureLayout[] layout = new VillaFurnitureLayout[LayoutCount];
                for (int i = 0; i < LayoutCount; i++)
                {
                    int offset = i * VillaFurnitureLayout.Size;
                    VillaFurnitureLayout entry = new();
                    entry.FurnitureID = (ushort)Bin.U16(data, offset);
                    entry.HasCollision = (ushort)Bin.U16(data, offset + 2);
                    entry.Position.X = (int)Bin.U32(data, offset + 4);
                    entry.Position.Y = (int)Bin.U32(data, offset + 8);
                    entry.Position.Z = (int)Bin.U32(data, offset + 0xC);
                    entry.X1 = (short)Bin.U16(data, offset + 0x10);
                    entry.Z1 = (short)Bin.U16(data, offset + 0x12);
                    entry.X2 = (short)Bin.U16(data, offset + 0x14);
                    entry.Z2 = (short)Bin.U16(data, offset + 0x16);
                    layout[i] = entry;
                }
                sFurnitureLayout = layout;
            }
            return sFurnitureLayout;
        }
    }

    // OFFSET 0x021F8370
    public static void Villa_DynamicMapFeaturesInit(FieldSystem fieldSystem)
    {
        PersistedMapFeatures features = MiscSaveBlock.SaveData_GetPersistedMapFeatures(FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!);
        byte[] buffer = PersistedMapFeatures.PersistedMapFeatures_GetBuffer(features, DynamicMapFeatures.Villa);
        VillaMapFeatures villa = new();
        villa.FieldSystem = fieldSystem;
        villa.PersistedFeatures = buffer;
        fieldSystem.FieldMapContext!.DynamicMapFeaturesData = villa;
        for (int i = 0; i < FurnitureCount; i++)
        {
            if (Villa_OwnsFurniture(fieldSystem, i) == 1)
            {
                Villa_LoadFurniture(villa, i);
            }
        }
    }

    // OFFSET 0x021F83C0
    public static void Villa_DynamicMapFeaturesFree(FieldSystem fieldSystem)
    {
        fieldSystem.FieldMapContext!.DynamicMapFeaturesData = null;
    }

    // OFFSET 0x021F83D4
    public static int Villa_DynamicMapFeaturesCheckCollision(FieldSystem fieldSystem, int tileX, int tileZ, int height, ref int collides)
    {
        VillaFurnitureLayout[] layout = FurnitureLayout;
        for (int i = 0; i < LayoutCount; i++)
        {
            if (Villa_IsTileInFurniture(tileX, tileZ, layout[i], fieldSystem) == 1)
            {
                collides = 1;
                return 1;
            }
        }
        collides = 0;
        return 0;
    }

    // OFFSET 0x021F8410
    public static int Villa_TryInteractWithFurniture(FieldSystem fieldSystem, int tileX, int tileZ, int direction)
    {
        PersistedMapFeatures features = MiscSaveBlock.SaveData_GetPersistedMapFeatures(FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!);
        if (PersistedMapFeatures.PersistedMapFeatures_GetID(features) != DynamicMapFeatures.Villa)
        {
            return 0;
        }
        VillaFurnitureLayout[] layout = FurnitureLayout;
        for (int i = 0; i < LayoutCount; i++)
        {
            if (Villa_IsTileInFurniture(tileX, tileZ, layout[i], fieldSystem) != 1)
            {
                continue;
            }
            uint script = FurnitureScripts[layout[i].FurnitureID];
            if (script == ScriptFacingNorthOnly && direction != DirNorth)
            {
                continue;
            }
            ScriptManager.ScriptManager_Set(fieldSystem, (ushort)script, null);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021F8480
    private static void Villa_LoadFurniture(VillaMapFeatures villa, int furnitureID)
    {
        VecFx32 rotation = default;
        int modelID = (int)FurnitureModels[furnitureID];
        FieldSystem fieldSystem = villa.FieldSystem!;
        VillaFurnitureLayout[] layout = FurnitureLayout;
        for (int i = 0; i < LayoutCount; i++)
        {
            if (layout[i].FurnitureID != furnitureID)
            {
                continue;
            }
            for (int slot = 0; slot < VillaMapFeatures.PropCount; slot++)
            {
                VillaLoadedProp prop = villa.Props[slot];
                if (prop.Loaded != 0)
                {
                    continue;
                }
                prop.Loaded = 1;
                prop.ModelID = modelID;
                prop.PropIndex = MapProps.MapPropManager_LoadOne(fieldSystem.MapPropManager!, fieldSystem.AreaDataManager!, modelID, layout[i].Position, rotation, fieldSystem.MapPropAnimationManager);
                prop.MapProp = MapProps.MapPropManager_FindLoadedPropByModelID(fieldSystem.MapPropManager!, modelID);
                break;
            }
        }
    }

    // OFFSET 0x021F8508
    private static int Villa_OwnsFurniture(FieldSystem fieldSystem, int furnitureID)
    {
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        return SystemFlags.SystemFlag_HandleOwnsVillaFurniture(varsFlags, SystemFlags.HandleFlagCheck, furnitureID) ? 1 : 0;
    }

    // OFFSET 0x021F851C
    private static int Villa_IsTileInFurniture(int tileX, int tileZ, VillaFurnitureLayout layout, FieldSystem fieldSystem)
    {
        if (layout.HasCollision == 1
            && Villa_OwnsFurniture(fieldSystem, layout.FurnitureID) == 1
            && tileZ >= layout.Z1 && tileZ <= layout.Z2
            && tileX >= layout.X1 && tileX <= layout.X2)
        {
            return 1;
        }
        return 0;
    }
}
