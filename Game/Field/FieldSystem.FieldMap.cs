namespace PlatinumSharp.Game;

public sealed class MapObjectsToPreload
{
    public const int MaxCount = 24;
    public const int Sentinel = 0xFFFF;

    public ushort Count;
    public readonly int[] IDs = new int[MaxCount];
}

public sealed partial class FieldMapContext
{
    public const int RenderTaskManagerOffset = 0x04;
    public const int WeatherSystemOffset = 0x0C;
    public const int TextureResourceManagerOffset = 0x10;
    public const int BerryPatchManagerOffset = 0x18;
    public const int HBlankSystemOffset = 0x1C;
    public const int HBlankScreenEffectOffset = 0x20;
    public const int Size = 0x28;

    public RenderTaskManager? RenderTaskManager;
    public WeatherSystem? WeatherSystem;
    public FieldTextureManager? TextureResourceManager;
    public BerryPatchManager? BerryPatchManager;
    public HBlankSystem? HBlankSystem;
    public FieldPoisonEffect? HBlankScreenEffect;
}

public sealed partial class FieldSystem
{
    public const int CameraRollEnabledOffset = 0x20;
    public const int AreaDataManagerOffset = 0x30;
    public const int MapObjectsToPreloadOffset = 0x34;
    public const int FieldEffectManagerOffset = 0x40;
    public const int AreaModelAttributesOffset = 0x44;
    public const int FogManagerOffset = 0x48;
    public const int AreaLightManagerOffset = 0x4C;
    public const int MapPropAnimationManagerOffset = 0x50;
    public const int MapPropOneShotAnimationManagerOffset = 0x54;
    public const int SignpostOffset = 0x64;
    public const int DynamicTerrainHeightManagerOffset = 0xA0;
    public const int MapPropManagerOffset = 0xA4;
    public const int HoneyTreeShakeListOffset = 0xA8;
    public const int FieldMapUpdateFlagsOffset = 0xC0;

    public int CameraRollEnabled;
    public AreaDataManager? AreaDataManager;
    public MapObjectsToPreload? MapObjectsToPreload;
    public FieldEffectManager? FieldEffectManager;
    public ModelAttributes? AreaModelAttributes;
    public FogManager? FogManager;
    public AreaLightManager? AreaLightManager;
    public MapPropAnimationManager? MapPropAnimationManager;
    public MapPropOneShotAnimationManager? MapPropOneShotAnimationManager;
    public Signpost? Signpost;
    public DynamicTerrainHeightManager? DynamicTerrainHeightManager;
    public MapPropManager? MapPropManager;
    public HoneyTreeShakeList? HoneyTreeShakeList;
    public uint FieldMapUpdateFlags;
}
