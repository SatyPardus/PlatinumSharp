using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class SplManagers
{
    private const ushort Disp3DAckMask = 0x3000;
    private const ushort Disp3DAlphaBlend = 1 << 3;
    private const int TextureFormatPalette4 = 2;
    private const int TextureFormatComp4x4 = 5;
    private const int FileResCountOffset = 8;
    private const int FileTexCountOffset = 0xA;
    private const int FileHeaderSize = 0x20;
    private const int BehaviorEntrySize = 8;
    private const int GravityBehaviorSize = 8;
    private const int RandomBehaviorSize = 8;
    private const int MagnetBehaviorSize = 0x10;
    private const int SpinBehaviorSize = 4;
    private const int CollisionPlaneBehaviorSize = 8;
    private const int ConvergenceBehaviorSize = 0x10;

    private static readonly SplDrawFunc[] sChildDrawFuncs =
    {
        SplDraw.SPLDraw_Child_Billboard,
        SplDraw.SPLDraw_Child_DirectionalBillboard,
        SplDraw.SPLDraw_Child_Polygon,
        SplDraw.SPLDraw_Child_DirectionalPolygon,
    };

    private static readonly SplDrawFunc[] sDrawFuncs =
    {
        SplDraw.SPLDraw_Billboard,
        SplDraw.SPLDraw_DirectionalBillboard,
        SplDraw.SPLDraw_Polygon,
        SplDraw.SPLDraw_DirectionalPolygon,
    };

    private static VecFx32 ReadVecFx32(byte[] data, int offset)
    {
        return new VecFx32 { X = Bin.I32(data, offset), Y = Bin.I32(data, offset + 4), Z = Bin.I32(data, offset + 8) };
    }

    private static VecFx16 ReadVecFx16(byte[] data, int offset)
    {
        return new VecFx16 { X = (short)Bin.S16(data, offset), Y = (short)Bin.S16(data, offset + 2), Z = (short)Bin.S16(data, offset + 4) };
    }

    private static SplResourceHeader ReadHeader(byte[] data, int offset)
    {
        uint misc = Bin.U32(data, offset + 0x48);
        uint flip = Bin.U32(data, offset + 0x4C);
        return new SplResourceHeader
        {
            Flags = Bin.U32(data, offset),
            EmitterBasePos = ReadVecFx32(data, offset + 4),
            EmissionCount = Bin.I32(data, offset + 0x10),
            Radius = Bin.I32(data, offset + 0x14),
            Length = Bin.I32(data, offset + 0x18),
            Axis = ReadVecFx16(data, offset + 0x1C),
            Color = (ushort)Bin.U16(data, offset + 0x22),
            InitVelPosAmplifier = Bin.I32(data, offset + 0x24),
            InitVelAxisAmplifier = Bin.I32(data, offset + 0x28),
            BaseScale = Bin.I32(data, offset + 0x2C),
            AspectRatio = (short)Bin.S16(data, offset + 0x30),
            StartDelay = (ushort)Bin.U16(data, offset + 0x32),
            MinRotation = (short)Bin.S16(data, offset + 0x34),
            MaxRotation = (short)Bin.S16(data, offset + 0x36),
            InitAngle = (ushort)Bin.U16(data, offset + 0x38),
            Reserved = (ushort)Bin.U16(data, offset + 0x3A),
            EmitterLifeTime = (ushort)Bin.U16(data, offset + 0x3C),
            ParticleLifeTime = (ushort)Bin.U16(data, offset + 0x3E),
            BaseScaleAttenuation = data[offset + 0x40],
            LifeTimeAttenuation = data[offset + 0x41],
            InitVelAttenuation = data[offset + 0x42],
            EmissionInterval = data[offset + 0x44],
            BaseAlpha = data[offset + 0x45],
            AirResistance = data[offset + 0x46],
            TextureIndex = data[offset + 0x47],
            LoopFrames = data[offset + 0x48],
            DbbScale = (ushort)((misc >> 8) & 0xFFFF),
            TextureTileCountS = (byte)((misc >> 24) & 3),
            TextureTileCountT = (byte)((misc >> 26) & 3),
            ScaleAnimDir = (byte)((misc >> 28) & 7),
            DpolFaceEmitter = (misc >> 31) != 0,
            FlipTextureS = (flip & 1) != 0,
            FlipTextureT = (flip & 2) != 0,
            PolygonX = (short)Bin.S16(data, offset + 0x50),
            PolygonY = (short)Bin.S16(data, offset + 0x52),
            UserData = Bin.U32(data, offset + 0x54),
        };
    }

    private static SplScaleAnim ReadScaleAnim(byte[] data, int offset)
    {
        return new SplScaleAnim
        {
            Start = (short)Bin.S16(data, offset),
            Mid = (short)Bin.S16(data, offset + 2),
            End = (short)Bin.S16(data, offset + 4),
            CurveIn = data[offset + 6],
            CurveOut = data[offset + 7],
            Loop = (Bin.U16(data, offset + 8) & 1) != 0,
        };
    }

    private static SplColorAnim ReadColorAnim(byte[] data, int offset)
    {
        int flags = Bin.U16(data, offset + 8);
        return new SplColorAnim
        {
            Start = (ushort)Bin.U16(data, offset),
            End = (ushort)Bin.U16(data, offset + 2),
            CurveIn = data[offset + 4],
            CurvePeak = data[offset + 5],
            CurveOut = data[offset + 6],
            RandomStartColor = (flags & 1) != 0,
            Loop = (flags & 2) != 0,
            Interpolate = (flags & 4) != 0,
        };
    }

    private static SplAlphaAnim ReadAlphaAnim(byte[] data, int offset)
    {
        int alpha = Bin.U16(data, offset);
        int flags = Bin.U16(data, offset + 2);
        return new SplAlphaAnim
        {
            Start = (byte)(alpha & 0x1F),
            Mid = (byte)((alpha >> 5) & 0x1F),
            End = (byte)((alpha >> 10) & 0x1F),
            RandomRange = (byte)(flags & 0xFF),
            Loop = (flags & 0x100) != 0,
            CurveIn = data[offset + 4],
            CurveOut = data[offset + 5],
        };
    }

    private static SplTexAnim ReadTexAnim(byte[] data, int offset)
    {
        uint param = Bin.U32(data, offset + 8);
        SplTexAnim texAnim = new()
        {
            FrameCount = (byte)(param & 0xFF),
            Step = (byte)((param >> 8) & 0xFF),
            RandomizeInit = (param & (1u << 16)) != 0,
            Loop = (param & (1u << 17)) != 0,
        };
        Array.Copy(data, offset, texAnim.Textures, 0, SplTexAnim.MaxFrames);
        return texAnim;
    }

    private static SplChildResource ReadChildResource(byte[] data, int offset)
    {
        uint misc = Bin.U32(data, offset + 0x10);
        return new SplChildResource
        {
            Flags = (ushort)Bin.U16(data, offset),
            RandomInitVelMag = (short)Bin.S16(data, offset + 2),
            EndScale = (short)Bin.S16(data, offset + 4),
            LifeTime = (ushort)Bin.U16(data, offset + 6),
            VelocityRatio = data[offset + 8],
            ScaleRatio = data[offset + 9],
            Color = (ushort)Bin.U16(data, offset + 0xA),
            EmissionCount = data[offset + 0xC],
            EmissionDelay = data[offset + 0xD],
            EmissionInterval = data[offset + 0xE],
            Texture = data[offset + 0xF],
            TextureTileCountS = (byte)(misc & 3),
            TextureTileCountT = (byte)((misc >> 2) & 3),
            FlipTextureS = (misc & (1u << 4)) != 0,
            FlipTextureT = (misc & (1u << 5)) != 0,
            DpolFaceEmitter = (misc & (1u << 6)) != 0,
        };
    }

    // OFFSET 0x0209C400
    public static void SPLManager_DeleteAllEmitters(SplManager manager)
    {
        SplEmitter? emitter = manager.ActiveEmitters.First;
        while (emitter != null)
        {
            SplEmitter? next = emitter.Next;
            SPLManager_DeleteEmitter(manager, emitter);
            emitter = next;
        }
    }

    // OFFSET 0x0209C444
    public static void SPLManager_DeleteEmitter(SplManager manager, SplEmitter emitter)
    {
        SplParticle? particle = SplLists.SPLList_PopFront(emitter.Particles);
        while (particle != null)
        {
            SplLists.SPLList_PushFront(manager.InactiveParticles, particle);
            particle = SplLists.SPLList_PopFront(emitter.Particles);
        }
        particle = SplLists.SPLList_PopFront(emitter.ChildParticles);
        while (particle != null)
        {
            SplLists.SPLList_PushFront(manager.InactiveParticles, particle);
            particle = SplLists.SPLList_PopFront(emitter.ChildParticles);
        }
        SplLists.SPLList_Erase(manager.ActiveEmitters, emitter);
        SplLists.SPLList_PushFront(manager.InactiveEmitters, emitter);
    }

    // OFFSET 0x0209C4D8
    public static SplEmitter? SPLManager_CreateEmitterWithCallback(SplManager manager, int resourceID, SplEmitterCallback? callback)
    {
        SplEmitter? emitter = null;
        if (manager.InactiveEmitters.First != null)
        {
            VecFx32 position = default;
            emitter = SplLists.SPLList_PopFront(manager.InactiveEmitters)!;
            SplEmitters.SPLEmitter_Init(emitter, manager.Resources![resourceID], position);
            callback?.Invoke(emitter);
            SplLists.SPLList_PushFront(manager.ActiveEmitters, emitter);
            if (emitter.Resource!.Header.SelfMaintaining)
            {
                emitter = null;
            }
        }
        return emitter;
    }

    // OFFSET 0x0209C56C
    public static SplEmitter? SPLManager_CreateEmitter(SplManager manager, int resourceID, in VecFx32 position)
    {
        SplEmitter? emitter = null;
        if (manager.InactiveEmitters.First != null)
        {
            emitter = SplLists.SPLList_PopFront(manager.InactiveEmitters)!;
            SplEmitters.SPLEmitter_Init(emitter, manager.Resources![resourceID], position);
            SplLists.SPLList_PushFront(manager.ActiveEmitters, emitter);
            if (emitter.Resource!.Header.SelfMaintaining)
            {
                emitter = null;
            }
        }
        return emitter;
    }

    // OFFSET 0x0209C5E0
    public static void SPLManager_Draw(SplManager manager)
    {
        Render3D.Display3DControl = (ushort)((Render3D.Display3DControl & ~Disp3DAckMask) | Disp3DAlphaBlend);
        manager.ViewMatrix = SplMath.FromMatrix(Render3D.View);
        if (manager.DrawOrder == 0)
        {
            SplEmitter? emitter = manager.ActiveEmitters.First;
            while (emitter != null)
            {
                manager.RenderEmitter = emitter;
                if ((emitter.State & SplEmitter.StateRenderingDisabled) == 0)
                {
                    SPLManager_DoDraw(manager);
                }
                emitter = emitter.Next;
            }
        }
        else
        {
            SplEmitter? emitter = manager.ActiveEmitters.Last;
            while (emitter != null)
            {
                manager.RenderEmitter = emitter;
                if ((emitter.State & SplEmitter.StateRenderingDisabled) == 0)
                {
                    SPLManager_DoDraw(manager);
                }
                emitter = emitter.Prev;
            }
        }
    }

    // OFFSET 0x0209C6A8
    public static void SPLManager_Update(SplManager manager)
    {
        SplEmitter? emitter = manager.ActiveEmitters.First;
        while (emitter != null)
        {
            SplResourceHeader header = emitter.Resource!.Header;
            SplEmitter? next = emitter.Next;
            if ((emitter.State & SplEmitter.StateStarted) == 0 && emitter.Age >= header.StartDelay)
            {
                emitter.State |= SplEmitter.StateStarted;
                emitter.Age = 0;
            }
            if ((emitter.State & SplEmitter.StatePaused) == 0)
            {
                int updateCycle = emitter.UpdateCycle;
                if (updateCycle == 0 || manager.CurrentCycle == updateCycle - 1)
                {
                    SplEmitters.SPLEmitter_Update(manager, emitter);
                }
            }
            bool expired = header.SelfMaintaining
                && header.EmitterLifeTime != 0
                && (emitter.State & SplEmitter.StateStarted) != 0
                && emitter.Age > header.EmitterLifeTime;
            if ((expired || (emitter.State & SplEmitter.StateTerminate) != 0)
                && emitter.Particles.Count == 0
                && emitter.ChildParticles.Count == 0)
            {
                SplLists.SPLList_PushFront(manager.InactiveEmitters, SplLists.SPLList_Erase(manager.ActiveEmitters, emitter));
            }
            emitter = next;
        }
        manager.CurrentCycle++;
        if (manager.CurrentCycle > 1)
        {
            manager.CurrentCycle = 0;
        }
    }

    // OFFSET 0x0209C7E0
    public static bool SPLManager_UploadPalettes(SplManager manager)
    {
        return SPLManager_UploadPalettesEx(manager, SPLUtil_AllocPaletteVRAM);
    }

    // OFFSET 0x0209C7F4
    public static bool SPLManager_UploadTextures(SplManager manager)
    {
        return SPLManager_UploadTexturesEx(manager, SPLUtil_AllocTextureVRAM);
    }

    // OFFSET 0x0209C808
    public static bool SPLManager_UploadPalettesEx(SplManager manager, SplPalVramAllocFunc vramAlloc)
    {
        for (int i = 0; i < manager.TexCount; i++)
        {
            SplTexture texture = manager.Textures![i];
            SplTextureResource resource = texture.Resource!;
            uint address = 0;
            if (resource.PaletteSize != 0)
            {
                address = vramAlloc(resource.PaletteSize, (resource.Param & 0xF) == TextureFormatPalette4 ? 1 : 0);
                int source = resource.Offset + (int)resource.PaletteOffset;
                ushort[] colors = new ushort[resource.PaletteSize / 2];
                for (int color = 0; color < colors.Length; color++)
                {
                    colors[color] = (ushort)Bin.U16(resource.Data, source + color * 2);
                }
                Video.LoadTexturePalette(colors, address, resource.PaletteSize);
            }
            texture.PalAddr = address;
        }
        return true;
    }

    // OFFSET 0x0209C8BC
    public static bool SPLManager_UploadTexturesEx(SplManager manager, SplTexVramAllocFunc vramAlloc)
    {
        for (int i = 0; i < manager.TexCount; i++)
        {
            SplTexture texture = manager.Textures![i];
            SplTextureResource resource = texture.Resource!;
            uint param = resource.Param;
            if ((param & (1u << 17)) != 0)
            {
                texture.TexAddr = manager.Textures[(param >> 18) & 0xFF].TexAddr;
                continue;
            }
            uint address = vramAlloc(resource.TextureSize, (param & 0xF) == TextureFormatComp4x4 ? 1 : 0);
            byte[] pixels = new byte[resource.TextureSize];
            Array.Copy(resource.Data, resource.Offset + SplTextureResource.Size, pixels, 0, pixels.Length);
            Video.LoadTexture(pixels, address, resource.TextureSize);
            texture.TexAddr = address;
        }
        return true;
    }

    // OFFSET 0x0209C988
    public static void SPLManager_LoadResources(SplManager manager, byte[] data)
    {
        manager.ResCount = (ushort)Bin.U16(data, FileResCountOffset);
        manager.TexCount = (ushort)Bin.U16(data, FileTexCountOffset);
        int offset = FileHeaderSize;
        manager.Alloc!((uint)(manager.ResCount * SplResource.Size));
        manager.Resources = new SplResource[manager.ResCount];
        for (int i = 0; i < manager.ResCount; i++)
        {
            SplResource resource = new();
            manager.Resources[i] = resource;
            resource.Header = ReadHeader(data, offset);
            offset += SplResourceHeader.Size;
            SplResourceHeader header = resource.Header;
            if (header.HasScaleAnim)
            {
                resource.ScaleAnim = ReadScaleAnim(data, offset);
                offset += SplScaleAnim.Size;
            }
            if (header.HasColorAnim)
            {
                resource.ColorAnim = ReadColorAnim(data, offset);
                offset += SplColorAnim.Size;
            }
            if (header.HasAlphaAnim)
            {
                resource.AlphaAnim = ReadAlphaAnim(data, offset);
                offset += SplAlphaAnim.Size;
            }
            if (header.HasTexAnim)
            {
                resource.TexAnim = ReadTexAnim(data, offset);
                offset += SplTexAnim.Size;
            }
            if (header.HasChildResource)
            {
                resource.ChildResource = ReadChildResource(data, offset);
                offset += SplChildResource.Size;
            }
            int behaviorCount = (header.HasGravityBehavior ? 1 : 0)
                + (header.HasRandomBehavior ? 1 : 0)
                + (header.HasMagnetBehavior ? 1 : 0)
                + (header.HasSpinBehavior ? 1 : 0)
                + (header.HasCollisionPlaneBehavior ? 1 : 0)
                + (header.HasConvergenceBehavior ? 1 : 0);
            resource.BehaviorCount = (ushort)behaviorCount;
            if (behaviorCount == 0)
            {
                resource.Behaviors = null;
                continue;
            }
            manager.Alloc((uint)(behaviorCount * BehaviorEntrySize));
            SplBehavior?[] behaviors = new SplBehavior?[behaviorCount];
            resource.Behaviors = behaviors;
            int index = 0;
            if (header.HasGravityBehavior)
            {
                SplGravityBehavior gravity = new()
                {
                    Magnitude = ReadVecFx16(data, offset),
                    Padding = (ushort)Bin.U16(data, offset + 6),
                };
                behaviors[index++] = new SplBehavior { ApplyFunc = SplBehaviorType.Gravity, Object = gravity };
                offset += GravityBehaviorSize;
            }
            if (header.HasRandomBehavior)
            {
                SplRandomBehavior random = new()
                {
                    Magnitude = ReadVecFx16(data, offset),
                    ApplyInterval = (ushort)Bin.U16(data, offset + 6),
                };
                behaviors[index++] = new SplBehavior { ApplyFunc = SplBehaviorType.Random, Object = random };
                offset += RandomBehaviorSize;
            }
            if (header.HasMagnetBehavior)
            {
                SplMagnetBehavior magnet = new()
                {
                    Target = ReadVecFx32(data, offset),
                    Force = (short)Bin.S16(data, offset + 0xC),
                    Padding = (ushort)Bin.U16(data, offset + 0xE),
                };
                behaviors[index++] = new SplBehavior { ApplyFunc = SplBehaviorType.Magnet, Object = magnet };
                offset += MagnetBehaviorSize;
            }
            if (header.HasSpinBehavior)
            {
                SplSpinBehavior spin = new()
                {
                    Angle = (ushort)Bin.U16(data, offset),
                    Axis = (ushort)Bin.U16(data, offset + 2),
                };
                behaviors[index++] = new SplBehavior { ApplyFunc = SplBehaviorType.Spin, Object = spin };
                offset += SpinBehaviorSize;
            }
            if (header.HasCollisionPlaneBehavior)
            {
                SplCollisionPlaneBehavior collision = new()
                {
                    Y = Bin.I32(data, offset),
                    Elasticity = (short)Bin.S16(data, offset + 4),
                    CollisionType = (ushort)Bin.U16(data, offset + 6),
                };
                behaviors[index++] = new SplBehavior { ApplyFunc = SplBehaviorType.CollisionPlane, Object = collision };
                offset += CollisionPlaneBehaviorSize;
            }
            if (header.HasConvergenceBehavior)
            {
                SplConvergenceBehavior convergence = new()
                {
                    Target = ReadVecFx32(data, offset),
                    Force = (short)Bin.S16(data, offset + 0xC),
                    Padding = (ushort)Bin.U16(data, offset + 0xE),
                };
                behaviors[index] = new SplBehavior { ApplyFunc = SplBehaviorType.Convergence, Object = convergence };
                offset += ConvergenceBehaviorSize;
            }
        }
        manager.Alloc((uint)(manager.TexCount * SplTexture.Size));
        manager.Textures = new SplTexture[manager.TexCount];
        for (int i = 0; i < manager.TexCount; i++)
        {
            SplTextureResource textureResource = new()
            {
                Data = data,
                Offset = offset,
                ID = Bin.U32(data, offset),
                Param = Bin.U32(data, offset + 4),
                TextureSize = Bin.U32(data, offset + 8),
                PaletteOffset = Bin.U32(data, offset + 0xC),
                PaletteSize = Bin.U32(data, offset + 0x10),
                ResourceSize = Bin.U32(data, offset + 0x1C),
            };
            uint param = textureResource.Param;
            manager.Textures[i] = new SplTexture
            {
                Resource = textureResource,
                Width = (ushort)(1 << (int)(((param >> 4) & 0xF) + 3)),
                Height = (ushort)(1 << (int)(((param >> 8) & 0xF) + 3)),
                Param = param,
            };
            offset += (int)textureResource.ResourceSize;
        }
    }

    // OFFSET 0x0209CD00
    public static SplManager SPLManager_New(SplAllocFunc alloc, ushort maxEmitters, ushort maxParticles, ushort fixPolygonID, ushort minPolygonID, ushort maxPolygonID)
    {
        alloc(SplManager.Size);
        SplManager manager = new();
        manager.MaxEmitters = maxEmitters;
        manager.MaxParticles = maxParticles;
        manager.PolygonIDMin = minPolygonID;
        manager.PolygonIDMax = maxPolygonID;
        manager.PolygonIDCurrent = manager.PolygonIDMin;
        manager.PolygonIDFix = fixPolygonID;
        manager.DrawOrder = 0;
        manager.PolygonID &= ~0xFE000000;
        manager.Alloc = alloc;
        manager.ActiveEmitters.Count = 0;
        manager.InactiveEmitters.Count = 0;
        manager.InactiveParticles.Count = 0;
        manager.ActiveEmitters.First = null;
        manager.InactiveEmitters.First = null;
        manager.InactiveParticles.First = null;
        manager.MiscPolygonAttr = 0;
        manager.CurrentCycle = 0;
        alloc((uint)(maxEmitters * SplEmitter.Size));
        for (int i = 0; i < maxEmitters; i++)
        {
            SplLists.SPLList_PushFront(manager.InactiveEmitters, new SplEmitter());
        }
        alloc((uint)(maxParticles * SplParticle.Size));
        for (int i = 0; i < maxParticles; i++)
        {
            SplLists.SPLList_PushFront(manager.InactiveParticles, new SplParticle());
        }
        manager.Resources = null;
        manager.Textures = null;
        manager.TexCount = 0;
        manager.ResCount = manager.TexCount;
        return manager;
    }

    // OFFSET 0x0209CE90
    public static uint SPLUtil_AllocPaletteVRAM(uint size, int is4Pal)
    {
        return (Gfd.AllocPlttVram(size, is4Pal, 0) & 0xFFFF) << 3;
    }

    // OFFSET 0x0209CEC8
    public static uint SPLUtil_AllocTextureVRAM(uint size, int is4x4Comp)
    {
        return (Gfd.AllocTexVram(size, is4x4Comp, 0) & 0xFFFF) << 3;
    }

    // OFFSET 0x0209CF00
    public static void SPLManager_DoDraw(SplManager manager)
    {
        SplResourceHeader header = manager.RenderEmitter!.Resource!.Header;
        if (header.DrawChildrenFirst)
        {
            SPLManager_DrawChildParticles(manager);
            if (!header.HideParent)
            {
                SPLManager_DrawParticles(manager);
            }
            return;
        }
        if (!header.HideParent)
        {
            SPLManager_DrawParticles(manager);
        }
        SPLManager_DrawChildParticles(manager);
    }

    // OFFSET 0x0209CF7C
    public static void SPLManager_DrawChildParticles(SplManager manager)
    {
        SplEmitter emitter = manager.RenderEmitter!;
        SplResource resource = emitter.Resource!;
        if (!resource.Header.HasChildResource)
        {
            return;
        }
        SplChildResource child = resource.ChildResource!;
        SplDraw.SPLUtil_SetTexture(manager.Textures![child.Texture]);
        SplDrawFunc drawFunc = sChildDrawFuncs[child.DrawType];
        SplParticle? particle = emitter.ChildParticles.First;
        while (particle != null)
        {
            drawFunc(manager, particle);
            particle = particle.Next;
        }
    }

    // OFFSET 0x0209D064
    public static void SPLManager_DrawParticles(SplManager manager)
    {
        SplEmitter emitter = manager.RenderEmitter!;
        SplResourceHeader header = emitter.Resource!.Header;
        SplDraw.SPLUtil_SetTexture(manager.Textures![header.TextureIndex]);
        SplDrawFunc drawFunc = sDrawFuncs[header.DrawType];
        SplSetTextureFunc setTexFunc = header.HasTexAnim ? SplDraw.SPLUtil_SetTexture : SplDraw.SPLUtil_SetTexture_Stub;
        SplParticle? particle = emitter.Particles.First;
        while (particle != null)
        {
            setTexFunc(manager.Textures[particle.Texture]);
            drawFunc(manager, particle);
            particle = particle.Next;
        }
    }
}
