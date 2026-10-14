using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class OpeningParticle
{
    public byte Active;
    public byte Timer;
    public Sprite? Sprite;
    public SysTask? Task;
}

public sealed class OpeningParticleEmitter
{
    public byte Count;
    public byte Interval;
    public byte Repeats;
    public byte Done;
    public byte Timer;
    public byte Mode;
    public SysTask? Task;
    public OpeningParticle[]? Particles;
}

public sealed class OpeningParticleSet
{
    public int Max;
    public OpeningParticle[] Particles = Array.Empty<OpeningParticle>();
}

public sealed class OpeningEffects
{
    public const int ParticleCount = 10;
    public const int EmitterCount = 3;
    public const int ResourceTypeCount = 4;

    private const int HeapID = 0x4C;
    private const int Overlay = 77;
    private const uint CharTransferTemplateAddress = 0x021D7920;
    private const uint ResourceCapacitiesAddress = 0x021D7930;
    private const int GraphicsArchive = 0x80;
    private const int TilesMember = 4;
    private const int PaletteMember = 3;
    private const int CellsMember = 5;
    private const int AnimationsMember = 6;
    private const int ResourceTiles = 0;
    private const int ResourcePalette = 1;
    private const int ResourceCells = 2;
    private const int ResourceAnimations = 3;
    private const uint ObjMappingMask = 0x00300010;
    private const uint ObjMapping1D32K = 0x10;
    private const int LayerObj = 0x10;
    private const int FirstEmitterFrame = 180;
    private const int SecondEmitterFrame = 285;
    private const int EmitterCountPerBurst = 3;
    private const int EmitterInterval = 3;
    private const int EmitterRepeats = 3;
    private const int EmitterPriority = 10;
    private const int ParticlePriority = 20;
    private const int FallingLifetime = 80;
    private const int TwinkleLifetime = 8;
    private const int FallSpeed = 3;
    private const int ParticleSpacing = 0x2000;

    public SpriteList? List;
    public readonly G2dRenderer Renderer = new();
    public readonly SpriteResourceCollection?[] Collections = new SpriteResourceCollection?[ResourceTypeCount];
    public readonly SpriteResource?[] Resources = new SpriteResource?[ResourceTypeCount];
    public readonly OpeningParticle[] Particles = new OpeningParticle[ParticleCount];
    public readonly OpeningParticleEmitter[] Emitters = new OpeningParticleEmitter[EmitterCount];

    public OpeningEffects()
    {
        for (int i = 0; i < ParticleCount; i++)
        {
            Particles[i] = new OpeningParticle();
        }
        for (int i = 0; i < EmitterCount; i++)
        {
            Emitters[i] = new OpeningParticleEmitter();
        }
    }

    // OFFSET 0x021D6670
    private static void OpeningEffects_InitTransfers()
    {
        byte[] raw = RomData.Overlay(Overlay, CharTransferTemplateAddress, 0x10);
        CharTransferTemplate template = new()
        {
            MaxTasks = (int)Bin.U32(raw, 0),
            SizeMain = (int)Bin.U32(raw, 4),
            SizeSub = (int)Bin.U32(raw, 8),
            HeapID = (int)Bin.U32(raw, 0xC),
        };
        CharTransfer.CharTransfer_Init(template);
        PlttTransfer.PlttTransfer_Init(1, HeapID);
        CharTransfer.CharTransfer_ClearBuffers();
        PlttTransfer.PlttTransfer_Clear();
    }

    // OFFSET 0x021D66A0
    private static void OpeningEffects_BuildTemplate(OpeningEffects effects, AffineSpriteListTemplate template, SpriteResourcesHeader header)
    {
        SpriteUtil.SpriteResourcesHeader_Init(
            header,
            0,
            0,
            0,
            0,
            -1,
            -1,
            0,
            0,
            effects.Collections[ResourceTiles],
            effects.Collections[ResourcePalette],
            effects.Collections[ResourceCells],
            effects.Collections[ResourceAnimations],
            null,
            null);
        template.List = effects.List;
        template.ResourceData = header;
        template.Position = new VecFx32 { X = 0, Y = 0, Z = 0 };
        template.AffineScale = new VecFx32 { X = Fx32.One, Y = Fx32.One, Z = Fx32.One };
        template.AffineZRotation = 0;
        template.Priority = 1;
        template.VramType = G2d.VramMain;
        template.HeapID = HeapID;
    }

    // OFFSET 0x021D670C
    public static OpeningEffects OpeningEffects_New()
    {
        OpeningEffects effects = new();
        OpeningEffects_InitRendering(effects);
        OpeningEffects_CreateSprites(effects);
        return effects;
    }

    // OFFSET 0x021D6734
    public static OpeningParticleSet OpeningParticleSet_New(int max)
    {
        OpeningParticleSet set = new();
        set.Particles = new OpeningParticle[max];
        for (int i = 0; i < max; i++)
        {
            set.Particles[i] = new OpeningParticle();
        }
        set.Max = max;
        return set;
    }

    // OFFSET 0x021D6760
    public static void OpeningParticleSet_Free(OpeningParticleSet set)
    {
        for (int i = 0; i < set.Max; i++)
        {
            if (set.Particles[i].Task != null)
            {
                SysTask.SysTask_Done(set.Particles[i].Task!);
            }
        }
    }

    // OFFSET 0x021D6794
    public static OpeningParticle OpeningParticleSet_GetParticle(OpeningParticleSet set, int index)
    {
        if (index >= set.Max)
        {
            ErrorHandling.GF_AssertFail();
        }
        return set.Particles[index];
    }

    // OFFSET 0x021D67B0
    public static void OpeningEffects_Free(OpeningEffects effects)
    {
        OpeningEffects_FreeRendering(effects);
        for (byte i = 0; i < EmitterCount; i++)
        {
            if (effects.Emitters[i].Task != null)
            {
                SysTask.SysTask_Done(effects.Emitters[i].Task!);
            }
        }
        for (byte i = 0; i < ParticleCount; i++)
        {
            if (effects.Particles[i].Task != null)
            {
                SysTask.SysTask_Done(effects.Particles[i].Task!);
            }
        }
    }

    // OFFSET 0x021D6800
    private static void OpeningEffects_InitRendering(OpeningEffects effects)
    {
        Video.SetDisplayControl(0, (Video.GetDisplayControl(0) & ~ObjMappingMask) | ObjMapping1D32K);
        Video.SetDisplayControl(1, (Video.GetDisplayControl(1) & ~ObjMappingMask) | ObjMapping1D32K);
        OpeningEffects_InitTransfers();
        OamManager.InitModule();
        RenderOam.RenderOam_Init(0, 0x80, 0, 0x20, 0, 0x80, 0, 0x20, HeapID);
        effects.List = SpriteUtil.SpriteList_InitRendering(ParticleCount, effects.Renderer, HeapID);
        byte[] capacities = RomData.Overlay(Overlay, ResourceCapacitiesAddress, ResourceTypeCount);
        for (int i = 0; i < ResourceTypeCount; i++)
        {
            effects.Collections[i] = SpriteResourceCollection.SpriteResourceCollection_New(capacities[i], i, HeapID);
        }
        effects.Resources[ResourceTiles] = SpriteResourceCollection.SpriteResourceCollection_AddTiles(effects.Collections[ResourceTiles], GraphicsArchive, TilesMember, 0, 0, 3, HeapID);
        effects.Resources[ResourcePalette] = SpriteResourceCollection.SpriteResourceCollection_AddPalette(effects.Collections[ResourcePalette], GraphicsArchive, PaletteMember, 0, 0, 3, 1, HeapID);
        effects.Resources[ResourceCells] = SpriteResourceCollection.SpriteResourceCollection_Add(effects.Collections[ResourceCells], GraphicsArchive, CellsMember, 0, 0, ResourceCells, HeapID);
        effects.Resources[ResourceAnimations] = SpriteResourceCollection.SpriteResourceCollection_Add(effects.Collections[ResourceAnimations], GraphicsArchive, AnimationsMember, 0, 0, ResourceAnimations, HeapID);
        SpriteTransfer.SpriteTransfer_RequestChar(effects.Resources[ResourceTiles]);
        SpriteTransfer.SpriteTransfer_RequestPlttWholeRange(effects.Resources[ResourcePalette]);
        GXLayers.GXLayers_EngineAToggleLayers(LayerObj, 1);
        GXLayers.GXLayers_EngineBToggleLayers(LayerObj, 1);
    }

    // OFFSET 0x021D691C
    private static void OpeningEffects_FreeRendering(OpeningEffects effects)
    {
        SpriteTransfer.SpriteTransfer_ResetCharTransfer(effects.Resources[ResourceTiles]);
        SpriteTransfer.SpriteTransfer_ResetPlttTransfer(effects.Resources[ResourcePalette]);
        for (byte i = 0; i < ResourceTypeCount; i++)
        {
            SpriteResourceCollection.SpriteResourceCollection_Delete(effects.Collections[i]);
        }
        Sprite.SpriteList_Delete(effects.List);
        RenderOam.RenderOam_Free();
        CharTransfer.CharTransfer_Free();
        PlttTransfer.PlttTransfer_Free();
    }

    // OFFSET 0x021D6964
    private static void OpeningEffects_CreateSprites(OpeningEffects effects)
    {
        int offset = 0;
        for (int i = 0; i < ParticleCount; i++)
        {
            AffineSpriteListTemplate template = new();
            SpriteResourcesHeader header = new();
            OpeningEffects_BuildTemplate(effects, template, header);
            template.Position.X = offset;
            template.Position.Y = offset;
            effects.Particles[i].Sprite = Sprite.SpriteList_AddAffine(template);
            Sprite.Sprite_SetAnimateFlag(effects.Particles[i].Sprite!, 0);
            Sprite.Sprite_SetDrawFlag(effects.Particles[i].Sprite!, 0);
            offset += ParticleSpacing;
        }
    }

    // OFFSET 0x021D69B4
    private static void OpeningEffects_Draw(OpeningEffects effects)
    {
        Sprite.SpriteList_Update(effects.List);
    }

    // OFFSET 0x021D69C0
    public static void OpeningEffects_Update(OpeningEffects effects, int frame)
    {
        if (frame == FirstEmitterFrame)
        {
            OpeningEffects_StartEmitter(effects.Emitters[0], effects, EmitterCountPerBurst, EmitterInterval, EmitterRepeats, 0);
        }
        else if (frame == SecondEmitterFrame)
        {
            OpeningEffects_StartEmitter(effects.Emitters[1], effects, EmitterCountPerBurst, EmitterInterval, EmitterRepeats, 1);
        }
        OpeningEffects_Draw(effects);
    }

    // OFFSET 0x021D6A0C
    private static void OpeningEffects_StartEmitter(OpeningParticleEmitter emitter, OpeningEffects effects, byte count, byte interval, byte repeats, byte mode)
    {
        emitter.Count = count;
        emitter.Interval = interval;
        emitter.Repeats = repeats;
        emitter.Timer = 0;
        emitter.Done = 0;
        emitter.Particles = effects.Particles;
        emitter.Mode = mode;
        emitter.Task = SysTask.SysTask_Start(OpeningEffects_EmitterTask, emitter, EmitterPriority);
    }

    // OFFSET 0x021D6A44
    private static void OpeningEffects_SpawnFalling(OpeningParticle[] particles, byte count, byte mode)
    {
        int baseY = mode == 0 ? 0x40 : 0x20;
        for (byte i = 0; i < count; i++)
        {
            OpeningParticle particle = OpeningEffects_AllocParticle(particles, ParticleCount)!;
            VecFx32 position = Sprite.Sprite_GetPosition(particle.Sprite!);
            position.X = (MathUtil.LCRNG_Next() % 128 + 0x40) << Fx32.Shift;
            position.Y = (baseY + MathUtil.LCRNG_Next() % 64) << Fx32.Shift;
            Sprite.Sprite_SetPosition(particle.Sprite!, position);
            Sprite.Sprite_SetAnimateFlag(particle.Sprite!, 1);
            Sprite.Sprite_SetDrawFlag(particle.Sprite!, 1);
            particle.Task = SysTask.SysTask_Start(OpeningEffects_FallingTask, particle, ParticlePriority);
        }
    }

    // OFFSET 0x021D6ADC
    private static OpeningParticle? OpeningEffects_AllocParticle(OpeningParticle[] particles, int max)
    {
        for (byte i = 0; i < ParticleCount; i++)
        {
            if (particles[i].Active == 0)
            {
                particles[i].Active = 1;
                particles[i].Timer = 0;
                return particles[i];
            }
        }
        ErrorHandling.GF_AssertFail();
        return null;
    }

    // OFFSET 0x021D6B0C
    private static void OpeningEffects_EmitterTask(SysTask task, object? param)
    {
        OpeningParticleEmitter emitter = (OpeningParticleEmitter)param!;
        byte timer = emitter.Timer;
        emitter.Timer++;
        if (timer < emitter.Interval)
        {
            return;
        }
        OpeningEffects_SpawnFalling(emitter.Particles!, emitter.Count, emitter.Mode);
        emitter.Timer = 0;
        emitter.Done++;
        if (emitter.Done >= emitter.Repeats)
        {
            SysTask.SysTask_Done(task);
            emitter.Task = null;
        }
    }

    // OFFSET 0x021D6B48
    private static void OpeningEffects_FallingTask(SysTask task, object? param)
    {
        OpeningParticle particle = (OpeningParticle)param!;
        VecFx32 position = Sprite.Sprite_GetPosition(particle.Sprite!);
        position.Y += FallSpeed << Fx32.Shift;
        Sprite.Sprite_SetPosition(particle.Sprite!, position);
        particle.Timer++;
        if (particle.Timer >= FallingLifetime)
        {
            SysTask.SysTask_Done(task);
            particle.Task = null;
            particle.Active = 0;
            Sprite.Sprite_SetAnimateFlag(particle.Sprite!, 0);
            Sprite.Sprite_SetDrawFlag(particle.Sprite!, 0);
        }
    }

    // OFFSET 0x021D6BA0
    public static void OpeningParticleSet_SpawnPair(OpeningParticleSet set, int hidden)
    {
        OpeningParticleSet_Spawn(set, 2, hidden);
    }

    // OFFSET 0x021D6BAC
    public static void OpeningParticleSet_Spawn(OpeningParticleSet set, int count, int hidden)
    {
        for (byte i = 0; i < count; i++)
        {
            OpeningParticle particle = OpeningEffects_AllocParticle(set.Particles, set.Max)!;
            VecFx32 position = Sprite.Sprite_GetPosition(particle.Sprite!);
            position.X = (MathUtil.LCRNG_Next() % 224 + 10) << Fx32.Shift;
            position.Y = (MathUtil.LCRNG_Next() % 56 + 250) << Fx32.Shift;
            Sprite.Sprite_SetPosition(particle.Sprite!, position);
            Sprite.Sprite_SetAnimateFlag(particle.Sprite!, 1);
            if (hidden == 0)
            {
                Sprite.Sprite_SetDrawFlag(particle.Sprite!, 1);
            }
            else
            {
                Sprite.Sprite_SetDrawFlag(particle.Sprite!, 0);
            }
            particle.Task = SysTask.SysTask_Start(OpeningEffects_TwinkleTask, particle, ParticlePriority);
        }
    }

    // OFFSET 0x021D6C44
    private static void OpeningEffects_TwinkleTask(SysTask task, object? param)
    {
        OpeningParticle particle = (OpeningParticle)param!;
        particle.Timer++;
        if (particle.Timer >= TwinkleLifetime)
        {
            SysTask.SysTask_Done(task);
            particle.Task = null;
            particle.Active = 0;
            Sprite.Sprite_SetAnimateFlag(particle.Sprite!, 0);
            Sprite.Sprite_SetDrawFlag(particle.Sprite!, 0);
        }
    }
}
