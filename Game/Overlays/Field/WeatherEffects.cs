using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void WeatherParticleFunc(WeatherParticle particle);

public delegate void WeatherSpawnFunc(WeatherEffect effect, int count);

public sealed class WeatherCounter
{
    public int Value;
}

public sealed class WeatherLinearValue
{
    public const int Size = 0x14;

    public int Current;
    public int Start;
    public int Delta;
    public int Step;
    public int StepCount;
}

public sealed class WeatherFxValue
{
    public const int Size = 0x10;

    public int Current;
    public int Start;
    public int Delta;
    public short Step;
    public short StepCount;
}

public sealed class WeatherSpawner
{
    public const int Size = 0x1C;

    public WeatherEffect? Effect;
    public short Count;
    public short Timer;
    public short Interval;
    public short CountTimer;
    public short CountTarget;
    public short IntervalTarget;
    public short IntervalStep;
    public short CountPeriod;
    public int CountStep;
    public WeatherSpawnFunc? Spawn;
}

public sealed class WeatherFogTable
{
    public const int Size = 0x30;
    public const int TableSize = 0x20;

    public FogManager? FogManager;
    public readonly byte[] Table = new byte[TableSize];
    public int Level;
    public int Timer;
    public short Period;
    public short Direction;
}

public sealed class WeatherFogFade
{
    public const int Size = 0x68;

    public FogManager? FogManager;
    public readonly WeatherLinearValue Offset = new();
    public readonly WeatherLinearValue Red = new();
    public readonly WeatherLinearValue Green = new();
    public readonly WeatherLinearValue Blue = new();
    public readonly WeatherLinearValue Slope = new();
}

public sealed class WeatherParticle
{
    public const int Size = 0x3C;
    public const int WorkCapacity = 10;

    public WeatherEffectManager? Manager;
    public Sprite? Sprite;
    public int[]? Work;
    public readonly int[] WorkBuffer = new int[WorkCapacity];
    public readonly WeatherCounter?[] WorkRefs = new WeatherCounter?[WorkCapacity];
    public WeatherParticle? Next;
    public WeatherParticle? Prev;
}

public sealed class WeatherEffectResources
{
    public const int Size = 0x64;
    public const int TypeCount = 4;

    public readonly SpriteResource?[] Resources = new SpriteResource?[TypeCount];
    public AffineSpriteListTemplate Template = new();
    public SpriteResourcesHeader Header = new();
}

public sealed class WeatherEffect
{
    public const int Size = 0xBA8;
    public const int ParticleCount = 0x30;

    public WeatherEffectManager? Manager;
    public WeatherEffectTableEntry? Entry;
    public WeatherEffectResources? Resources;
    public readonly WeatherParticle Sentinel = new();
    public readonly WeatherParticle[] Particles = new WeatherParticle[ParticleCount];
    public SysTask? Task;
    public VecFx32 CameraTarget;
    public object? Work;
    public int SoundPlaying;
    public ushort SoundID;
    public ushort State;
    public ushort FogMode;
    public ushort Request;

    public WeatherEffect()
    {
        for (int i = 0; i < ParticleCount; i++)
        {
            Particles[i] = new WeatherParticle();
        }
    }
}

public sealed class WeatherEffectTableEntry
{
    public const int Size = 0x1C;

    public ushort GraphicsID;
    public ushort BgID;
    public int WorkSize;
    public WeatherEffect? Effect;
    public WeatherEffectResources? Resources;
    public ushort State;
    public ushort LoadStep;
    public SysTask? LoadTask;
    public uint MainFuncAddress;
    public SysTaskFunc? MainFunc;
}

public sealed class WeatherEffectSprites
{
    public const int ResourceTypeCount = 4;

    public readonly SpriteResourceCollection?[] Collections = new SpriteResourceCollection?[ResourceTypeCount];
    public SpriteResourceTable[]? Tables;
    public readonly G2dRenderer Renderer = new();
    public readonly RenderSurface Surface = new();
    public SpriteList? SpriteList;
    public SysTask? UpdateTask;
}

public sealed class WeatherEffectManager
{
    public const int Size = 0x10C;

    public WeatherEffectTableEntry[]? Effects;
    public uint[]? BgMembers;
    public readonly WeatherEffectSprites Sprites = new();
    public FieldSystem? FieldSystem;
    public NARC? Narc;
}

public static partial class WeatherEffects
{
    public const int WeatherCount = 0x1F;
    public const int StateLoading = 1;
    public const int StateLoaded = 2;
    public const int StateRunning = 3;

    private const int Overlay = 5;
    private const uint EffectTableAddress = 0x02201D78;
    private const uint BgMemberTableAddress = 0x021F8D90;
    private const int BgMemberTableCount = 11;
    private const int BgMemberPalette = 0;
    private const int BgMemberTiles = 1;
    private const int BgMemberTilemap = 2;
    private const int HeapIDField = 4;
    private const int NarcWeather = 0x41;
    private const int ResourceCapacity = 0x1F;
    private const int SpriteListCapacity = 0x60;
    private const uint SpriteUpdatePriority = 10;
    private const uint LoadTaskPriority = 1;
    private const uint EffectTaskPriority = 4;
    private const int RendererZOffset = -0x1000;
    private const int SurfaceWidth = 0xFF000;
    private const int SurfaceHeight = 0xC0000;
    private const int SurfaceMain = 1;
    private const uint NarcMemberTilesTable = 0x3F;
    private const uint NarcMemberPaletteTable = 0x40;
    private const uint NarcMemberCellTable = 0x3D;
    private const uint NarcMemberAnimTable = 0x3E;
    private const int ResourceTiles = 0;
    private const int ResourcePalette = 1;
    private const int ResourceCells = 2;
    private const int ResourceAnims = 3;
    private const ushort NoID = 0xFFFF;
    private const int LayerBg0 = 0;
    private const int LayerBg2 = 2;
    private const int LayerMaskBg2 = 4;
    private const int BlendPlaneBg2 = 4;
    private const int BlendPlaneBg0Obj = 0x21;
    private const int DisplayMain = 0;
    private const int ControlPriorityMask = 3;
    private const int WrapWidth = 0x13F000;
    private const int WrapHeight = 0x100000;
    private const int WrapMargin = -0x40000;
    private const int EffectStateInit = 0;
    private const int EffectStateFadeIn = 1;
    private const int EffectStateStart = 2;
    private const int EffectStateMain = 3;
    private const int EffectStateFadeOut = 4;
    private const int EffectStateEnd = 5;
    private const int RequestFadeOut = 5;
    private const int FogModeNone = 0;
    private const int FogModeTable = 1;
    private const int FogModeInterpolate = 2;
    private const int CommandLoad = 0;
    private const int CommandLoadAsync = 1;
    private const int CommandFadeIn = 2;
    private const int CommandStart = 3;
    private const int CommandCrossFadeIn = 4;
    private const int CommandFadeOut = 5;
    private const int CommandForceEnd = 6;
    private const int CommandCrossFadeOut = 7;
    private const int CommandUnload = 8;
    private const int FogTableSteps = 0x7F;

    private static WeatherEffectTableEntry[]? sEffects;

    private static WeatherEffectTableEntry[] EffectTable
    {
        get
        {
            if (sEffects == null)
            {
                byte[] data = RomData.Overlay(Overlay, EffectTableAddress, WeatherCount * WeatherEffectTableEntry.Size);
                WeatherEffectTableEntry[] effects = new WeatherEffectTableEntry[WeatherCount];
                for (int i = 0; i < WeatherCount; i++)
                {
                    int offset = i * WeatherEffectTableEntry.Size;
                    uint address = PlatinumSharp.Nitro.Bin.U32(data, offset + 0x18) & ~1u;
                    effects[i] = new WeatherEffectTableEntry
                    {
                        GraphicsID = (ushort)PlatinumSharp.Nitro.Bin.U16(data, offset),
                        BgID = (ushort)PlatinumSharp.Nitro.Bin.U16(data, offset + 2),
                        WorkSize = (int)PlatinumSharp.Nitro.Bin.U32(data, offset + 4),
                        MainFuncAddress = address,
                        MainFunc = ResolveMainFunc(address),
                    };
                }
                sEffects = effects;
            }
            return sEffects;
        }
    }

    private static SysTaskFunc ResolveMainFunc(uint address)
    {
        return address switch
        {
            0x021D7604 => WeatherEffect_NoneMain,
            0x021D7658 => WeatherEffect_RainMain,
            0x021D79F0 => WeatherEffect_SnowMain,
            0x021D7E54 => WeatherEffect_HeavySnowMain,
            0x021D823C => WeatherEffect_HeavySnowBgMain,
            0x021D84D4 => WeatherEffect_LightFogMain,
            0x021D8638 => WeatherEffect_DenseFogMain,
            0x021D879C => WeatherEffect_WhiteFogBgMain,
            0x021D8948 => WeatherEffect_SandMain,
            0x021D8D08 => WeatherEffect_SandstormMain,
            0x021D8FF8 => WeatherEffect_BlizzardMain,
            0x021D9464 => WeatherEffect_DiamondDustMain,
            0x021D97E8 => WeatherEffect_FlickerBgMain,
            0x021D9F0C => WeatherEffect_ThunderstormMain,
            0x021D9FF8 => WeatherEffect_HeavyRainMain,
            0x021DA244 => WeatherEffect_AshMain,
            0x021DA748 => WeatherEffect_LightningMain,
            0x021DA8A0 => WeatherEffect_RisingMain,
            0x021DAB78 => WeatherEffect_OverlayBgMain,
            0x021DAC68 => WeatherEffect_FaintOverlayBgMain,
            0x021DAD38 => WeatherEffect_DarkFogBgMain,
            0x021DAEC0 => WeatherEffect_PaleFogBgMain,
            0x021DB04C => WeatherEffect_CircleWindowMain,
            0x021DB144 => WeatherEffect_ScrollingFogBgMain,
            0x021DB3A8 => WeatherEffect_PulsingBgStrongMain,
            0x021DB3C4 => WeatherEffect_PulsingBgWeakMain,
            0x021DB3E0 => WeatherEffect_FogColorAMain,
            0x021DB40C => WeatherEffect_FogColorBMain,
            0x021DB438 => WeatherEffect_FogColorCMain,
            0x021DB460 => WeatherEffect_FogColorDMain,
            0x021DB48C => WeatherEffect_FogColorEMain,
            _ => (task, param) => throw Missing(address),
        };
    }

    private static object? CreateWork(WeatherEffectTableEntry entry)
    {
        switch (entry.MainFuncAddress)
        {
            case 0x021D7658:
            case 0x021D79F0:
            case 0x021D7E54:
            case 0x021D823C:
            case 0x021D8948:
            case 0x021D8D08:
            case 0x021D8FF8:
            case 0x021D9464:
            case 0x021D9F0C:
            case 0x021D9FF8:
            case 0x021DA244:
            case 0x021DA8A0:
                return new WeatherParticleWork();
            case 0x021D84D4:
            case 0x021D8638:
            case 0x021DB3A8:
            case 0x021DB3C4:
            case 0x021DB3E0:
            case 0x021DB40C:
            case 0x021DB438:
            case 0x021DB460:
            case 0x021DB48C:
                return new WeatherFogWork();
            case 0x021D879C:
            case 0x021DAB78:
            case 0x021DAC68:
            case 0x021DAD38:
            case 0x021DAEC0:
            case 0x021DB144:
                return new WeatherOverlayWork();
            case 0x021D97E8:
            case 0x021DA748:
                return new WeatherIntWork();
            case 0x021DB04C:
                return new WeatherCircleWork();
            default:
                return new WeatherIntWork();
        }
    }

    private static void SetBgPriority(int layer, int priority)
    {
        Video.SetBgControl(layer, (ushort)((Video.GetBgControl(layer) & ~ControlPriorityMask) | priority));
    }

    private static void SetBg2Offset(int x, int y)
    {
        Video.SetBgOffset(LayerBg2, (uint)((x & 0x1FF) | ((y << 16) & 0x1FF0000)));
    }

    private static void ResetBgLayer()
    {
        GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 0);
        SetBgPriority(LayerBg2, 3);
        SetBgPriority(LayerBg0, 1);
        Video.SetBlendControl(DisplayMain, 0);
    }

    // OFFSET 0x021D61D0
    private static void WeatherEffectSprites_Init(WeatherEffectSprites sprites)
    {
        RenderView.RenderView_InitRenderer(sprites.Renderer, RendererZOffset);
        ViewRect viewRect = new() { X = 0, Y = 0, Width = SurfaceWidth, Height = SurfaceHeight };
        RenderOam.RenderOam_InitSurface(sprites.Surface, viewRect, SurfaceMain, sprites.Renderer);
        for (int i = 0; i < WeatherEffectSprites.ResourceTypeCount; i++)
        {
            sprites.Collections[i] = SpriteResourceCollection.SpriteResourceCollection_New(ResourceCapacity, i, HeapIDField);
        }
        sprites.Tables = new SpriteResourceTable[WeatherEffectSprites.ResourceTypeCount];
        for (int i = 0; i < WeatherEffectSprites.ResourceTypeCount; i++)
        {
            sprites.Tables[i] = new SpriteResourceTable();
        }
        WeatherEffectSprites_LoadTable(sprites.Tables, ResourceTiles, NarcMemberTilesTable);
        WeatherEffectSprites_LoadTable(sprites.Tables, ResourcePalette, NarcMemberPaletteTable);
        WeatherEffectSprites_LoadTable(sprites.Tables, ResourceCells, NarcMemberCellTable);
        WeatherEffectSprites_LoadTable(sprites.Tables, ResourceAnims, NarcMemberAnimTable);
        SpriteListParams listParams = new() { MaxElements = SpriteListCapacity, Renderer = sprites.Renderer, HeapID = HeapIDField };
        sprites.SpriteList = Sprite.SpriteList_New(listParams);
        sprites.UpdateTask = SysTask.SysTask_Start(WeatherEffectSprites_UpdateTask, sprites, SpriteUpdatePriority);
    }

    // OFFSET 0x021D6284
    private static void WeatherEffectSprites_UpdateTask(SysTask task, object? param)
    {
        WeatherEffectSprites sprites = (WeatherEffectSprites)param!;
        Sprite.SpriteList_Update(sprites.SpriteList);
    }

    // OFFSET 0x021D6290
    private static void WeatherEffectSprites_LoadTable(SpriteResourceTable[] tables, int index, uint narcMember)
    {
        SpriteResourceTable table = SpriteResourceTable.SpriteResourceTable_GetArrayElement(tables, index);
        byte[] data = Graphics.LoadMemberFromNARC(NarcWeather, narcMember, 0, HeapIDField, 1)!;
        SpriteResourceTable.SpriteResourceTable_LoadFromBinary(data, table, HeapIDField);
        Heap.Heap_Free(data);
    }

    // OFFSET 0x021D62BC
    private static void WeatherEffectSprites_Free(WeatherEffectSprites sprites)
    {
        for (int i = 0; i < WeatherEffectSprites.ResourceTypeCount; i++)
        {
            SpriteResourceTable.SpriteResourceTable_Clear(SpriteResourceTable.SpriteResourceTable_GetArrayElement(sprites.Tables!, i));
            SpriteResourceCollection.SpriteResourceCollection_Delete(sprites.Collections[i]);
        }
        sprites.Tables = null;
        Sprite.SpriteList_Delete(sprites.SpriteList);
        sprites.SpriteList = null;
        SysTask.SysTask_Done(sprites.UpdateTask!);
        sprites.UpdateTask = null;
    }

    // OFFSET 0x021D630C
    private static void WeatherSprite_SetPositionWrapped(Sprite sprite, ref VecFx32 position)
    {
        if (position.X > WrapWidth)
        {
            position.X %= WrapWidth;
        }
        else if (position.X < WrapMargin)
        {
            position.X += WrapWidth;
        }
        if (position.Y > WrapHeight)
        {
            position.Y %= WrapHeight;
        }
        else if (position.Y < WrapMargin)
        {
            position.Y += WrapHeight;
        }
        Sprite.Sprite_SetPosition(sprite, position);
    }

    // OFFSET 0x021D6364
    public static WeatherEffectManager WeatherEffectManager_New(FieldSystem fieldSystem)
    {
        WeatherEffectManager manager = new();
        manager.FieldSystem = fieldSystem;
        WeatherEffectSprites_Init(manager.Sprites);
        manager.Effects = EffectTable;
        manager.BgMembers = RomData.OverlayU32(Overlay, BgMemberTableAddress, BgMemberTableCount * 3);
        manager.Narc = NARC.NARC_ctor(NarcWeather, HeapIDField);
        return manager;
    }

    // OFFSET 0x021D63A4
    public static void WeatherEffectManager_Free(ref WeatherEffectManager? managerRef)
    {
        WeatherEffectManager? manager = managerRef;
        if (manager == null)
        {
            return;
        }
        for (int i = 0; i < WeatherCount; i++)
        {
            WeatherEffectManager_Unload(manager, i);
        }
        FogManager.FogManager_ApplyParameters(manager.FieldSystem!.FogManager!, FogManager.ParameterEnabled, 0, 0, 0, 0);
        SetBgPriority(LayerBg0, 1);
        GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 0);
        WeatherEffectSprites_Free(manager.Sprites);
        NARC.NARC_dtor(manager.Narc!);
        managerRef = null;
    }

    // OFFSET 0x021D6418
    public static int WeatherEffectManager_Command(WeatherEffectManager manager, int command, int weather)
    {
        int result = 1;
        if (weather > WeatherCount)
        {
            return 0;
        }
        switch (command)
        {
            case CommandLoad:
                result = WeatherEffectManager_Load(manager, weather);
                break;
            case CommandLoadAsync:
                result = WeatherEffectManager_LoadAsync(manager, weather);
                break;
            case CommandFadeIn:
                result = WeatherEffectManager_Start(manager, weather, EffectStateInit, FogModeTable);
                break;
            case CommandStart:
                result = WeatherEffectManager_Start(manager, weather, EffectStateStart, FogModeTable);
                break;
            case CommandCrossFadeIn:
                if (FogManager.FogManager_IsEnabled(manager.FieldSystem!.FogManager!) == 1)
                {
                    result = WeatherEffectManager_Start(manager, weather, EffectStateInit, FogModeInterpolate);
                }
                else
                {
                    result = WeatherEffectManager_Start(manager, weather, EffectStateInit, FogModeTable);
                }
                break;
            case CommandFadeOut:
                WeatherEffectManager_RequestFadeOut(manager, weather, FogModeTable);
                break;
            case CommandForceEnd:
                WeatherEffectManager_ForceEnd(manager, weather);
                break;
            case CommandCrossFadeOut:
                WeatherEffectManager_RequestFadeOut(manager, weather, FogModeNone);
                break;
            case CommandUnload:
                WeatherEffectManager_Unload(manager, weather);
                break;
        }
        return result;
    }

    // OFFSET 0x021D64D0
    public static int WeatherEffectManager_GetEffectState(WeatherEffectManager manager, int weather)
    {
        if (weather >= WeatherCount)
        {
            return 0;
        }
        return manager.Effects![weather].State;
    }

    // OFFSET 0x021D64E4
    private static void Weather_SetBgBlendAlpha(int bgCoefficient, int otherCoefficient)
    {
        Video.SetBlendAlpha(DisplayMain, BlendPlaneBg2, BlendPlaneBg0Obj, bgCoefficient, otherCoefficient);
    }

    // OFFSET 0x021D64FC
    private static void WeatherLinearValue_Init(WeatherLinearValue value, int start, int end, int stepCount)
    {
        value.Current = start;
        value.Start = start;
        value.Delta = end - start;
        value.StepCount = stepCount;
        value.Step = 0;
    }

    // OFFSET 0x021D650C
    private static int WeatherLinearValue_Step(WeatherLinearValue value)
    {
        value.Current = value.Delta * value.Step / value.StepCount + value.Start;
        if (value.Step + 1 <= value.StepCount)
        {
            value.Step++;
            return 0;
        }
        value.Step = value.StepCount;
        return 1;
    }

    // OFFSET 0x021D6538
    private static void WeatherFxValue_Init(WeatherFxValue value, int start, int end, int stepCount)
    {
        value.Current = start;
        value.Start = start;
        value.Delta = end - start;
        value.StepCount = (short)stepCount;
        value.Step = 0;
    }

    // OFFSET 0x021D6548
    private static int WeatherFxValue_Step(WeatherFxValue value)
    {
        value.Current = Fx32.Div(Fx32.Mul(value.Delta, value.Step), value.StepCount) + value.Start;
        if (value.Step + 1 <= value.StepCount)
        {
            value.Step++;
            return 0;
        }
        value.Step = value.StepCount;
        return 1;
    }

    // OFFSET 0x021D6594
    private static void WeatherEffectManager_LoadSpriteResources(WeatherEffectManager manager, ushort graphicsID, WeatherEffectResources resources)
    {
        WeatherEffectManager_LoadCells(manager, graphicsID, resources);
        WeatherEffectManager_LoadAnims(manager, graphicsID, resources);
        WeatherEffectManager_LoadTiles(manager, graphicsID, resources);
        WeatherEffectManager_LoadPalette(manager, graphicsID, resources);
    }

    // OFFSET 0x021D65C0
    private static SpriteResource? WeatherEffectManager_LoadResource(SpriteResourceTable[] tables, int type, int index, SpriteResourceCollection? collection, NARC narc, int allocAtEnd)
    {
        SpriteResourceTable table = SpriteResourceTable.SpriteResourceTable_GetArrayElement(tables, type);
        int memberIndex = SpriteResourceTable.SpriteResourceTable_GetNARCEntryMemberIndex(table, index);
        int compressed = SpriteResourceTable.SpriteResourceTable_IsNARCEntryCompressed(table, index);
        int vramType = SpriteResourceTable.SpriteResourceTable_GetEntryVRAMType(table, index);
        int paletteIndex = SpriteResourceTable.SpriteResourceTable_GetPaletteIndex(table, index);
        int id = SpriteResourceTable.SpriteResourceTable_GetEntryID(table, index);
        SpriteResource? resource = null;
        switch (type)
        {
            case ResourceTiles:
                resource = SpriteResourceCollection.SpriteResourceCollection_AddTilesFromEx(collection, narc, memberIndex, compressed, id, vramType, HeapIDField, allocAtEnd);
                break;
            case ResourcePalette:
                resource = SpriteResourceCollection.SpriteResourceCollection_AddPaletteFromEx(collection, narc, memberIndex, compressed, id, vramType, paletteIndex, HeapIDField, allocAtEnd);
                break;
            case ResourceCells:
                resource = SpriteResourceCollection.SpriteResourceCollection_AddFrom(collection, narc, memberIndex, compressed, id, ResourceCells, HeapIDField);
                break;
            case ResourceAnims:
                resource = SpriteResourceCollection.SpriteResourceCollection_AddFrom(collection, narc, memberIndex, compressed, id, ResourceAnims, HeapIDField);
                break;
        }
        return resource;
    }

    // OFFSET 0x021D6690
    private static void WeatherEffectManager_FreeSpriteResources(WeatherEffectManager manager, ushort graphicsID, WeatherEffectResources resources)
    {
        if (graphicsID == NoID)
        {
            return;
        }
        if (resources.Resources[ResourceTiles] != null)
        {
            SpriteTransfer.SpriteTransfer_ResetCharTransfer(resources.Resources[ResourceTiles]);
        }
        if (resources.Resources[ResourcePalette] != null)
        {
            SpriteTransfer.SpriteTransfer_ResetPlttTransfer(resources.Resources[ResourcePalette]);
        }
        for (int i = 0; i < WeatherEffectResources.TypeCount; i++)
        {
            if (resources.Resources[i] != null)
            {
                SpriteResourceCollection.SpriteResourceCollection_Remove(manager.Sprites.Collections[i], resources.Resources[i]!);
            }
        }
    }

    // OFFSET 0x021D66D0
    private static int WeatherEffectManager_Load(WeatherEffectManager manager, int weather)
    {
        WeatherEffectTableEntry entry = manager.Effects![weather];
        if (entry.Effect == null)
        {
            if (WeatherEffectManager_AllocEffect(manager, entry) == 0)
            {
                return 0;
            }
            if (WeatherEffectManager_LoadGraphics(manager, entry) == 0)
            {
                entry.Effect = null;
                return 0;
            }
            entry.Effect!.Resources = entry.Resources;
            if (entry.GraphicsID != NoID)
            {
                WeatherEffect_CreateSprites(entry.Effect);
            }
            WeatherEffectManager_LoadBg(manager, entry.BgID);
            entry.State = StateLoaded;
        }
        return 1;
    }

    // OFFSET 0x021D6730
    private static int WeatherEffectManager_LoadAsync(WeatherEffectManager manager, int weather)
    {
        WeatherEffectTableEntry entry = manager.Effects![weather];
        if (entry.Effect != null)
        {
            return 1;
        }
        if (WeatherEffectManager_AllocEffect(manager, entry) == 0)
        {
            return 0;
        }
        entry.LoadTask = SysTask.SysTask_Start(WeatherEffectManager_LoadTask, entry, LoadTaskPriority);
        entry.State = StateLoading;
        entry.LoadStep = 0;
        return 1;
    }

    // OFFSET 0x021D676C
    private static int WeatherEffectManager_Start(WeatherEffectManager manager, int weather, int state, int fogMode)
    {
        WeatherEffectTableEntry entry = manager.Effects![weather];
        if (entry.GraphicsID != NoID && entry.Resources == null)
        {
            return 0;
        }
        if (entry.Effect == null)
        {
            return 0;
        }
        if (entry.State != StateLoaded)
        {
            return 0;
        }
        WeatherEffect effect = entry.Effect;
        effect.Task = SysTask.SysTask_Start(entry.MainFunc!, effect, EffectTaskPriority);
        if (effect.Task == null)
        {
            return 0;
        }
        entry.State = StateRunning;
        effect.State = (ushort)state;
        effect.Request = 0;
        effect.Sentinel.Next = effect.Sentinel;
        effect.Sentinel.Prev = effect.Sentinel;
        effect.FogMode = (ushort)fogMode;
        effect.SoundPlaying = 0;
        effect.CameraTarget = Render3D.CameraTarget;
        if (entry.WorkSize > 0)
        {
            effect.Work = CreateWork(entry);
        }
        else
        {
            effect.Work = null;
        }
        if (entry.BgID != NoID)
        {
            GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 0);
            SetBgPriority(LayerBg2, 1);
            SetBgPriority(LayerBg0, 2);
        }
        return 1;
    }

    // OFFSET 0x021D6868
    private static void WeatherEffectManager_RequestFadeOut(WeatherEffectManager manager, int weather, int fogMode)
    {
        WeatherEffectTableEntry entry = manager.Effects![weather];
        if (entry.State == StateRunning)
        {
            entry.Effect!.Request = RequestFadeOut;
            entry.Effect.FogMode = (ushort)fogMode;
        }
    }

    // OFFSET 0x021D6890
    private static void WeatherEffectManager_ForceEnd(WeatherEffectManager manager, int weather)
    {
        WeatherEffectTableEntry entry = manager.Effects![weather];
        if (WeatherEffectManager_GetEffectState(manager, weather) == StateRunning)
        {
            entry.Effect!.State = EffectStateEnd;
            entry.MainFunc!(null!, entry.Effect);
        }
    }

    // OFFSET 0x021D68B8
    private static void WeatherEffectManager_Unload(WeatherEffectManager manager, int weather)
    {
        WeatherEffectTableEntry entry = manager.Effects![weather];
        if (entry.BgID != NoID)
        {
            ResetBgLayer();
        }
        if (entry.Resources != null)
        {
            WeatherEffectManager_FreeSpriteResources(manager, entry.GraphicsID, entry.Resources);
            entry.Resources = null;
            if (entry.LoadTask != null)
            {
                SysTask.SysTask_Done(entry.LoadTask);
                entry.LoadTask = null;
            }
        }
        if (entry.Effect != null)
        {
            WeatherParticleList_RemoveAll(entry.Effect.Sentinel);
            if (entry.GraphicsID != NoID)
            {
                WeatherEffect_DeleteSprites(entry.Effect);
            }
            if (entry.Effect.SoundPlaying == 1)
            {
                WeatherEffect_StopSound(entry.Effect);
            }
            if (entry.Effect.Work != null)
            {
                entry.Effect.Work = null;
            }
            if (entry.State == StateLoading)
            {
                if (entry.LoadTask != null)
                {
                    SysTask.SysTask_Done(entry.LoadTask);
                }
            }
            else if (entry.State == StateRunning)
            {
                SysTask.SysTask_Done(entry.Effect.Task!);
            }
            entry.Effect = null;
        }
        FogManager.FogManager_ApplyParameters(manager.FieldSystem!.FogManager!, FogManager.ParameterEnabled, 0, 0, 0, 0);
    }

    // OFFSET 0x021D69B8
    private static void WeatherEffectTableEntry_Finish(WeatherEffectTableEntry entry)
    {
        if (entry.BgID != NoID)
        {
            ResetBgLayer();
        }
        if (entry.Effect != null)
        {
            WeatherParticleList_RemoveAll(entry.Effect.Sentinel);
            if (entry.Effect.SoundPlaying == 1)
            {
                WeatherEffect_StopSound(entry.Effect);
            }
            if (entry.State == StateRunning)
            {
                SysTask.SysTask_Done(entry.Effect.Task!);
                entry.State = StateLoaded;
            }
        }
    }

    // OFFSET 0x021D6A2C
    private static void WeatherEffectManager_LoadBg(WeatherEffectManager manager, ushort bgID)
    {
        WeatherEffectManager_LoadBgPalette(manager, bgID);
        WeatherEffectManager_LoadBgTiles(manager, bgID);
        WeatherEffectManager_LoadBgTilemap(manager, bgID);
    }

    // OFFSET 0x021D6A48
    private static int WeatherEffectManager_LoadGraphics(WeatherEffectManager manager, WeatherEffectTableEntry entry)
    {
        if (entry.GraphicsID != NoID && entry.Resources == null)
        {
            if (WeatherEffectTableEntry_AllocResources(entry) == 0)
            {
                return 0;
            }
            WeatherEffectManager_LoadSpriteResources(manager, entry.GraphicsID, entry.Resources!);
            WeatherEffectManager_InitSpriteTemplate(manager, entry);
        }
        return 1;
    }

    // OFFSET 0x021D6A84
    private static void WeatherEffectManager_LoadTask(SysTask task, object? param)
    {
        WeatherEffectTableEntry entry = (WeatherEffectTableEntry)param!;
        WeatherEffectManager manager = entry.Effect!.Manager!;
        switch (entry.LoadStep)
        {
            case 0:
                if (WeatherEffectTableEntry_AllocResources(entry) == 0)
                {
                    ErrorHandling.GF_AssertFail();
                }
                entry.LoadStep++;
                break;
            case 1:
                WeatherEffectManager_LoadCells(manager, entry.GraphicsID, entry.Resources);
                entry.LoadStep++;
                break;
            case 2:
                WeatherEffectManager_LoadBgPalette(manager, entry.BgID);
                entry.LoadStep++;
                break;
            case 3:
                WeatherEffectManager_LoadAnims(manager, entry.GraphicsID, entry.Resources);
                entry.LoadStep++;
                break;
            case 4:
                WeatherEffectManager_LoadBgTiles(manager, entry.BgID);
                entry.LoadStep++;
                break;
            case 5:
                WeatherEffectManager_LoadTiles(manager, entry.GraphicsID, entry.Resources);
                entry.LoadStep++;
                break;
            case 6:
                WeatherEffectManager_LoadBgTilemap(manager, entry.BgID);
                entry.LoadStep++;
                break;
            case 7:
                WeatherEffectManager_LoadPalette(manager, entry.GraphicsID, entry.Resources);
                entry.LoadStep++;
                break;
            case 8:
                WeatherEffectManager_InitSpriteTemplate(manager, entry);
                entry.Effect.Resources = entry.Resources;
                if (entry.GraphicsID != NoID)
                {
                    WeatherEffect_CreateSprites(entry.Effect);
                }
                entry.State = StateLoaded;
                entry.LoadStep = 0;
                entry.LoadTask = null;
                SysTask.SysTask_Done(task);
                break;
        }
    }

    // OFFSET 0x021D6B60
    private static int WeatherEffectManager_AllocEffect(WeatherEffectManager manager, WeatherEffectTableEntry entry)
    {
        if (entry.Effect != null)
        {
            return 1;
        }
        WeatherEffect effect = new();
        entry.Effect = effect;
        effect.Manager = manager;
        effect.State = 0;
        effect.Request = 0;
        effect.Sentinel.Next = effect.Sentinel;
        effect.Sentinel.Prev = effect.Sentinel;
        effect.Work = null;
        effect.Entry = entry;
        return 1;
    }

    // OFFSET 0x021D6BC4
    private static int WeatherEffectTableEntry_AllocResources(WeatherEffectTableEntry entry)
    {
        if (entry.GraphicsID != NoID)
        {
            if (entry.Resources != null)
            {
                return 1;
            }
            entry.Resources = new WeatherEffectResources();
        }
        return 1;
    }

    // OFFSET 0x021D6BFC
    private static void WeatherEffectManager_LoadCells(WeatherEffectManager manager, ushort graphicsID, WeatherEffectResources? resources)
    {
        if (graphicsID != NoID)
        {
            resources!.Resources[ResourceCells] = WeatherEffectManager_LoadResource(manager.Sprites.Tables!, ResourceCells, graphicsID, manager.Sprites.Collections[ResourceCells], manager.Narc!, 0);
        }
    }

    // OFFSET 0x021D6C30
    private static void WeatherEffectManager_LoadAnims(WeatherEffectManager manager, ushort graphicsID, WeatherEffectResources? resources)
    {
        if (graphicsID != NoID)
        {
            resources!.Resources[ResourceAnims] = WeatherEffectManager_LoadResource(manager.Sprites.Tables!, ResourceAnims, graphicsID, manager.Sprites.Collections[ResourceAnims], manager.Narc!, 0);
        }
    }

    // OFFSET 0x021D6C64
    private static void WeatherEffectManager_LoadTiles(WeatherEffectManager manager, ushort graphicsID, WeatherEffectResources? resources)
    {
        if (graphicsID != NoID)
        {
            resources!.Resources[ResourceTiles] = WeatherEffectManager_LoadResource(manager.Sprites.Tables!, ResourceTiles, graphicsID, manager.Sprites.Collections[ResourceTiles], manager.Narc!, 1);
            SpriteTransfer.SpriteTransfer_RequestCharAtEnd(resources.Resources[ResourceTiles]);
            SpriteResource.SpriteResource_ReleaseData(resources.Resources[ResourceTiles]);
        }
    }

    // OFFSET 0x021D6CA0
    private static void WeatherEffectManager_LoadPalette(WeatherEffectManager manager, ushort graphicsID, WeatherEffectResources? resources)
    {
        if (graphicsID != NoID)
        {
            resources!.Resources[ResourcePalette] = WeatherEffectManager_LoadResource(manager.Sprites.Tables!, ResourcePalette, graphicsID, manager.Sprites.Collections[ResourcePalette], manager.Narc!, 1);
            SpriteTransfer.SpriteTransfer_RequestPlttFreeSpace(resources.Resources[ResourcePalette]);
            SpriteResource.SpriteResource_ReleaseData(resources.Resources[ResourcePalette]);
        }
    }

    // OFFSET 0x021D6CDC
    private static void WeatherEffectManager_InitSpriteTemplate(WeatherEffectManager manager, WeatherEffectTableEntry entry)
    {
        if (entry.GraphicsID == NoID)
        {
            return;
        }
        WeatherEffectResources resources = entry.Resources!;
        WeatherEffectManager_InitResourcesHeader(resources.Header, manager, resources.Resources, 0, 1);
        AffineSpriteListTemplate template = new();
        template.List = manager.Sprites.SpriteList;
        template.ResourceData = resources.Header;
        template.AffineScale = new VecFx32 { X = Fx32.One, Y = Fx32.One, Z = Fx32.One };
        template.VramType = 1;
        resources.Template = template;
    }

    // OFFSET 0x021D6D34
    private static void WeatherEffect_CreateSprites(WeatherEffect effect)
    {
        for (int i = 0; i < WeatherEffect.ParticleCount; i++)
        {
            Sprite? sprite = Sprite.SpriteList_AddAffine(effect.Resources!.Template);
            effect.Particles[i].Sprite = sprite;
            if (sprite == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            Sprite.Sprite_SetDrawFlag(sprite!, 0);
        }
    }

    // OFFSET 0x021D6D64
    private static void WeatherEffect_DeleteSprites(WeatherEffect effect)
    {
        for (int i = 0; i < WeatherEffect.ParticleCount; i++)
        {
            if (effect.Particles[i].Sprite != null)
            {
                Sprite.Sprite_Delete(effect.Particles[i].Sprite!);
                effect.Particles[i].Sprite = null;
            }
        }
    }

    // OFFSET 0x021D6D84
    private static void WeatherEffectManager_LoadBgPalette(WeatherEffectManager manager, ushort bgID)
    {
        if (bgID == NoID)
        {
            return;
        }
        byte[] data = NARC.NARC_AllocAndReadWholeMember(manager.Narc!, manager.BgMembers![bgID * 3 + BgMemberPalette], HeapIDField);
        G2d.GetUnpackedPaletteData(data, out PaletteFile? palette);
        Bg.Bg_LoadPalette(LayerBg2, palette!.Colors, 0, 0x20, 0xC0);
        Heap.Heap_Free(data);
    }

    // OFFSET 0x021D6DCC
    private static void WeatherEffectManager_LoadBgTiles(WeatherEffectManager manager, ushort bgID)
    {
        if (bgID == NoID)
        {
            return;
        }
        byte[] data = NARC.NARC_AllocAndReadWholeMember(manager.Narc!, manager.BgMembers![bgID * 3 + BgMemberTiles], HeapIDField);
        G2d.GetUnpackedCharacterData(data, out CharacterData? charData);
        Bg.Bg_LoadTiles(manager.FieldSystem!.BgConfig!, LayerBg2, charData!.RawData, charData.SizeBytes, 0);
        Heap.Heap_Free(data);
    }

    // OFFSET 0x021D6E20
    private static void WeatherEffectManager_LoadBgTilemap(WeatherEffectManager manager, ushort bgID)
    {
        if (bgID == NoID)
        {
            return;
        }
        GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 0);
        byte[] data = NARC.NARC_AllocAndReadWholeMember(manager.Narc!, manager.BgMembers![bgID * 3 + BgMemberTilemap], HeapIDField);
        if (data == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        G2d.GetUnpackedScreenData(data!, out ScreenData? screenData);
        BgConfig bgConfig = manager.FieldSystem!.BgConfig!;
        Bg.Bg_CopyTilemapBufferRangeToVRAM(bgConfig, LayerBg2, screenData!.RawData, screenData.SizeBytes, 0);
        Bg.Bg_LoadTilemapBuffer(bgConfig, LayerBg2, screenData.RawData, screenData.SizeBytes);
        Bg.Bg_ChangeTilemapRectPalette(bgConfig, LayerBg2, 0, 0, 0x20, 0x20, 6);
        Bg.Bg_CopyTilemapBufferToVRAM(bgConfig, LayerBg2);
        Heap.Heap_Free(data!);
    }

    // OFFSET 0x021D6EC8
    private static void WeatherParticle_AllocWork(WeatherParticle particle, int size)
    {
        if (particle.Work != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (size <= 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if ((uint)size > WeatherParticle.WorkCapacity * 4)
        {
            ErrorHandling.GF_AssertFail();
        }
        particle.Work = particle.WorkBuffer;
    }

    // OFFSET 0x021D6EF0
    private static void WeatherParticle_FreeWork(WeatherParticle particle)
    {
        particle.Work = null;
        Array.Clear(particle.WorkBuffer);
        Array.Clear(particle.WorkRefs);
    }

    // OFFSET 0x021D6F00
    private static WeatherParticle? WeatherEffect_AddParticle(WeatherEffect effect, int workSize)
    {
        WeatherEffectManager? manager = effect.Manager;
        WeatherParticle? particle = WeatherEffect_FindFreeParticle(effect);
        if (particle == null)
        {
            return null;
        }
        particle.Manager = manager;
        particle.Next = effect.Sentinel;
        particle.Prev = effect.Sentinel.Prev;
        effect.Sentinel.Prev!.Next = particle;
        effect.Sentinel.Prev = particle;
        WeatherParticle_AllocWork(particle, workSize);
        if (particle.Work == null)
        {
            return null;
        }
        if (particle.Sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        Sprite.Sprite_SetDrawFlag(particle.Sprite!, 1);
        return particle;
    }

    // OFFSET 0x021D6F4C
    private static void WeatherEffectManager_InitResourcesHeader(SpriteResourcesHeader header, WeatherEffectManager manager, SpriteResource?[] resources, int vramTransfer, int priority)
    {
        int[] ids = new int[WeatherEffectResources.TypeCount];
        for (int i = 0; i < WeatherEffectResources.TypeCount; i++)
        {
            ids[i] = SpriteResource.SpriteResource_GetID(resources[i]);
        }
        SpriteUtil.SpriteResourcesHeader_Init(
            header,
            ids[ResourceTiles],
            ids[ResourcePalette],
            ids[ResourceCells],
            ids[ResourceAnims],
            -1,
            -1,
            vramTransfer,
            priority,
            manager.Sprites.Collections[ResourceTiles],
            manager.Sprites.Collections[ResourcePalette],
            manager.Sprites.Collections[ResourceCells],
            manager.Sprites.Collections[ResourceAnims],
            null,
            null);
    }

    // OFFSET 0x021D6FA8
    private static void WeatherParticle_Remove(WeatherParticle particle)
    {
        particle.Prev!.Next = particle.Next;
        particle.Next!.Prev = particle.Prev;
        Sprite.Sprite_SetDrawFlag(particle.Sprite!, 0);
        WeatherParticle_FreeWork(particle);
        particle.Manager = null;
        particle.Next = null;
        particle.Prev = null;
    }

    // OFFSET 0x021D6FD8
    private static void WeatherParticleList_RemoveAll(WeatherParticle sentinel)
    {
        WeatherParticle particle = sentinel.Next!;
        while (particle != sentinel)
        {
            WeatherParticle next = particle.Next!;
            WeatherParticle_Remove(particle);
            particle = next;
        }
    }

    // OFFSET 0x021D6FF0
    private static void WeatherParticleList_ForEach(WeatherParticle sentinel, WeatherParticleFunc func)
    {
        WeatherParticle particle = sentinel.Next!;
        WeatherParticle? next = particle.Next;
        while (particle != sentinel)
        {
            func(particle);
            particle = next!;
            next = next!.Next;
        }
    }

    // OFFSET 0x021D700C
    private static void WeatherEffect_AfterUpdate(WeatherEffect effect)
    {
    }

    // OFFSET 0x021D7010
    private static VecFx32 WeatherParticle_GetPosition(WeatherParticle particle)
    {
        return Sprite.Sprite_GetPosition(particle.Sprite!);
    }

    // OFFSET 0x021D7028
    private static void WeatherEffect_CalcCameraScroll(out int scrollX, out int scrollY, WeatherEffect effect)
    {
        VecFx32 target = Render3D.CameraTarget;
        int deltaX = target.X - effect.CameraTarget.X;
        int deltaZ = target.Z - effect.CameraTarget.Z;
        int aspectRatio = Fx32.Div(4 * Fx32.One, 3 * Fx32.One);
        Camera camera = effect.Manager!.FieldSystem!.Camera!;
        FxUtil.CalcLinearFov(Camera.Camera_GetFOV(camera), Camera.Camera_GetDistance(camera), aspectRatio, out int width, out int height);
        width = Fx32.Div(width, 0x100000);
        if (deltaZ <= 0)
        {
            height = Fx32.Div(height, 0xBE8D0);
        }
        else
        {
            height = Fx32.Div(height, 0xBE811);
        }
        int sign = Fx32.One;
        if (deltaX < 0)
        {
            sign = -Fx32.One;
            deltaX = Fx32.Mul(deltaX, sign);
        }
        int x = Fx32.Div(deltaX, width);
        if (sign < 0)
        {
            x = Fx32.Mul(x, sign);
        }
        sign = Fx32.One;
        if (deltaZ < 0)
        {
            sign = -Fx32.One;
            deltaZ = Fx32.Mul(deltaZ, sign);
        }
        int y = Fx32.Div(deltaZ, height);
        if (sign < 0)
        {
            y = Fx32.Mul(y, sign);
        }
        if (x + y != 0)
        {
            effect.CameraTarget = target;
        }
        scrollX = x;
        scrollY = y;
    }

    // OFFSET 0x021D717C
    private static void WeatherEffect_ScrollParticles(WeatherEffect effect, out int scrollX, out int scrollY)
    {
        WeatherEffect_ScrollParticlesFx(effect, out int x, out int y);
        scrollX = x >> Fx32.Shift;
        if (scrollX < 0)
        {
            scrollX += Fx32.One;
        }
        scrollY = y >> Fx32.Shift;
        if (scrollY < 0)
        {
            scrollY++;
        }
    }

    // OFFSET 0x021D71B4
    private static void WeatherEffect_ScrollParticlesFx(WeatherEffect effect, out int scrollX, out int scrollY)
    {
        WeatherEffect_CalcCameraScroll(out int x, out int y, effect);
        WeatherParticle particle = effect.Sentinel.Next!;
        while (particle != effect.Sentinel)
        {
            VecFx32 position = WeatherParticle_GetPosition(particle);
            position.X -= x;
            position.Y -= y;
            WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
            particle = particle.Next!;
        }
        scrollX = x;
        scrollY = y;
    }

    // OFFSET 0x021D7210
    private static void WeatherSpawner_Init(WeatherSpawner spawner, WeatherEffect effect, int count, int interval, int countTarget, int intervalTarget, int intervalStep, int countPeriod, int countStep, WeatherSpawnFunc spawn)
    {
        spawner.Effect = effect;
        spawner.Count = (short)count;
        spawner.Timer = 0;
        spawner.Interval = (short)interval;
        spawner.CountTimer = 0;
        spawner.CountTarget = (short)countTarget;
        spawner.IntervalTarget = (short)intervalTarget;
        spawner.IntervalStep = (short)intervalStep;
        spawner.CountPeriod = (short)countPeriod;
        spawner.CountStep = countStep;
        spawner.Spawn = spawn;
    }

    // OFFSET 0x021D7238
    private static void WeatherSpawner_SetTargets(WeatherSpawner spawner, int countTarget, int intervalTarget, int intervalStep, int countStep)
    {
        spawner.CountTarget = (short)countTarget;
        spawner.IntervalTarget = (short)intervalTarget;
        spawner.IntervalStep = (short)intervalStep;
        spawner.CountStep = countStep;
    }

    // OFFSET 0x021D7244
    private static int WeatherSpawner_Update(WeatherSpawner spawner)
    {
        int result = 0;
        if (spawner.IntervalStep == 0)
        {
            result |= 1;
        }
        else
        {
            bool decreasing = spawner.IntervalStep <= 0;
            if ((spawner.Interval >= spawner.IntervalTarget && !decreasing) || (spawner.Interval <= spawner.IntervalTarget && decreasing))
            {
                result |= 1;
                spawner.Interval = spawner.IntervalTarget;
            }
        }
        if (spawner.CountStep == 0)
        {
            result |= 2;
        }
        else
        {
            bool decreasing = spawner.CountStep <= 0;
            if ((spawner.Count >= spawner.CountTarget && !decreasing) || (spawner.Count <= spawner.CountTarget && decreasing))
            {
                result |= 2;
                spawner.Count = spawner.CountTarget;
            }
        }
        spawner.Timer--;
        if (spawner.Timer <= 0)
        {
            spawner.Spawn!(spawner.Effect!, spawner.Count);
            spawner.Timer = spawner.Interval;
            if ((result & 1) == 0)
            {
                spawner.Interval += spawner.IntervalStep;
            }
            if ((result & 2) == 0)
            {
                spawner.CountTimer++;
                if (spawner.CountTimer >= spawner.CountPeriod)
                {
                    spawner.CountTimer = 0;
                    spawner.Count = (short)(spawner.Count + spawner.CountStep);
                }
            }
        }
        return result;
    }

    // OFFSET 0x021D7308
    private static void WeatherFog_StartFadeIn(WeatherFogFade fogFade, WeatherFogTable fogTable, FogManager fogManager, int slope, int offset, ushort color, int period, int mode)
    {
        fogTable.FogManager = fogManager;
        if (mode == FogModeTable)
        {
            WeatherFog_Apply(fogManager, slope, offset, color);
            WeatherFogTable_Clear(fogTable);
            WeatherFogTable_Start(fogTable, period, 1);
        }
        else if (mode == FogModeInterpolate)
        {
            WeatherFogFade_Init(fogFade, fogManager, slope, offset, color, period * FogTableSteps);
        }
    }

    // OFFSET 0x021D735C
    private static int WeatherFog_UpdateFadeIn(WeatherFogFade fogFade, WeatherFogTable fogTable, int mode)
    {
        int result = 1;
        if (mode == FogModeTable)
        {
            result = WeatherFogTable_Update(fogTable);
        }
        else if (mode == FogModeInterpolate)
        {
            if (WeatherFogFade_Step(fogFade) == 0)
            {
                result = 0;
            }
        }
        return result;
    }

    // OFFSET 0x021D7384
    private static void WeatherFog_Apply(FogManager fogManager, int slope, int offset, ushort color)
    {
        FogManager.FogManager_ApplyParameters(fogManager, FogManager.ParameterAll, 1, 0, slope, offset);
        FogManager.FogManager_ApplyColor(fogManager, FogManager.ParameterAll, color, 0x1F);
    }

    // OFFSET 0x021D73B0
    private static void WeatherFogFade_Init(WeatherFogFade fogFade, FogManager fogManager, int slope, int offset, ushort color, int stepCount)
    {
        int currentSlope = FogManager.FogManager_GetSlope(fogManager);
        int currentOffset = FogManager.FogManager_GetOffset(fogManager);
        int currentColor = FogManager.FogManager_GetColor(fogManager);
        fogFade.FogManager = fogManager;
        WeatherLinearValue_Init(fogFade.Offset, currentOffset, offset, stepCount);
        WeatherLinearValue_Init(fogFade.Red, currentColor & 0x1F, color & 0x1F, stepCount);
        WeatherLinearValue_Init(fogFade.Green, (currentColor >> 5) & 0x1F, (color >> 5) & 0x1F, stepCount);
        WeatherLinearValue_Init(fogFade.Blue, (currentColor >> 10) & 0x1F, (color >> 10) & 0x1F, stepCount);
        WeatherLinearValue_Init(fogFade.Slope, currentSlope, slope, stepCount);
    }

    // OFFSET 0x021D7434
    private static int WeatherFogFade_Step(WeatherFogFade fogFade)
    {
        int result = WeatherLinearValue_Step(fogFade.Offset);
        WeatherLinearValue_Step(fogFade.Red);
        WeatherLinearValue_Step(fogFade.Green);
        WeatherLinearValue_Step(fogFade.Blue);
        WeatherLinearValue_Step(fogFade.Slope);
        ushort color = (ushort)(fogFade.Red.Current | (fogFade.Green.Current << 5) | (fogFade.Blue.Current << 10));
        WeatherFog_Apply(fogFade.FogManager!, fogFade.Slope.Current, fogFade.Offset.Current, color);
        return result;
    }

    // OFFSET 0x021D7480
    private static void WeatherFogTable_Clear(WeatherFogTable fogTable)
    {
        for (int i = 0; i < WeatherFogTable.TableSize; i++)
        {
            fogTable.Table[i] = 0;
        }
        FogManager.FogManager_ApplyDensityTable(fogTable.FogManager!, fogTable.Table);
    }

    // OFFSET 0x021D749C
    private static void WeatherFogTable_Start(WeatherFogTable fogTable, int period, int fadeIn)
    {
        fogTable.Level = FogTableSteps;
        fogTable.Timer = 0;
        fogTable.Period = (short)period;
        if (fadeIn == 1)
        {
            fogTable.Direction = 1;
        }
        else
        {
            fogTable.Direction = -1;
        }
    }

    // OFFSET 0x021D74B8
    private static int WeatherFogTable_Update(WeatherFogTable fogTable)
    {
        int result = WeatherFogTable_Step(fogTable);
        if (fogTable.Timer == 0)
        {
            FogManager.FogManager_ApplyDensityTable(fogTable.FogManager!, fogTable.Table);
        }
        return result;
    }

    // OFFSET 0x021D74D4
    private static void WeatherFogTable_SetFull(WeatherFogTable fogTable)
    {
        fogTable.Level = 1;
        fogTable.Timer = 0;
        fogTable.Period = 0;
        fogTable.Direction = 1;
        WeatherFogTable_Fill(fogTable);
        FogManager.FogManager_ApplyDensityTable(fogTable.FogManager!, fogTable.Table);
    }

    // OFFSET 0x021D74F4
    private static int WeatherFogTable_Step(WeatherFogTable fogTable)
    {
        int result = 0;
        if (fogTable.Direction == 0)
        {
            result = 1;
        }
        else if (fogTable.Level <= 0)
        {
            result = 1;
        }
        else
        {
            fogTable.Timer++;
            if (fogTable.Timer >= fogTable.Period)
            {
                fogTable.Timer = 0;
                WeatherFogTable_Fill(fogTable);
                fogTable.Level--;
                if (fogTable.Level <= 0)
                {
                    result = 1;
                }
            }
        }
        return result;
    }

    // OFFSET 0x021D7534
    private static void WeatherFogTable_Fill(WeatherFogTable fogTable)
    {
        int level = fogTable.Level;
        if (fogTable.Direction < 0)
        {
            level = FogTableSteps - level;
        }
        int first = level / 4;
        for (int i = WeatherFogTable.TableSize - 1; i > first; i--)
        {
            fogTable.Table[i] = (byte)((i - first) << 2);
        }
    }

    // OFFSET 0x021D7568
    private static void WeatherEffect_Prefill(WeatherEffect effect, WeatherSpawnFunc spawn, int count, int period, int increment, WeatherParticleFunc move)
    {
        spawn(effect, count);
        int steps = 0;
        WeatherParticle particle = effect.Sentinel.Next!;
        WeatherParticle? next = particle.Next;
        for (int i = 0; i < count; i++)
        {
            if (particle == effect.Sentinel)
            {
                break;
            }
            for (int j = 0; j < steps; j++)
            {
                move(particle);
                if (particle.Work == null)
                {
                    break;
                }
            }
            particle = next!;
            next = particle.Next;
            if (i >= period && i % period == 0)
            {
                steps += increment;
            }
        }
    }

    // OFFSET 0x021D75E4
    private static WeatherParticle? WeatherEffect_FindFreeParticle(WeatherEffect effect)
    {
        for (int i = 0; i < WeatherEffect.ParticleCount; i++)
        {
            if (effect.Particles[i].Manager == null)
            {
                return effect.Particles[i];
            }
        }
        return null;
    }

    // OFFSET 0x021D7604
    private static void WeatherEffect_NoneMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        switch (effect.State)
        {
            case EffectStateInit:
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                effect.State = EffectStateMain;
                break;
            case EffectStateStart:
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (effect.Request == RequestFadeOut)
                {
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                effect.State = EffectStateEnd;
                break;
            case EffectStateEnd:
                WeatherEffectTableEntry_Finish(effect.Entry!);
                break;
        }
    }

    // OFFSET 0x021DB4B8
    private static void WeatherEffect_PlaySound(WeatherEffect effect, int seqID)
    {
        if (effect.SoundPlaying != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        effect.SoundPlaying = 1;
        effect.SoundID = (ushort)seqID;
        SoundPlayback.Sound_PlayEffect((ushort)seqID);
    }

    // OFFSET 0x021DB4E4
    private static void WeatherEffect_StopSound(WeatherEffect effect)
    {
        SoundPlayback.Sound_StopEffect(effect.SoundID, 0);
        effect.SoundPlaying = 0;
    }
}
