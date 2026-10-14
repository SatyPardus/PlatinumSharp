namespace PlatinumSharp.Game;

public static class SplEmitters
{
    public const int CallbackPreUpdate = 0;
    public const int CallbackPostUpdate = 1;

    private const int EmissionTypePoint = 0;
    private const int EmissionTypeSphereSurface = 1;
    private const int EmissionTypeCircleBorder = 2;
    private const int EmissionTypeCircleBorderUniform = 3;
    private const int EmissionTypeSphere = 4;
    private const int EmissionTypeCircle = 5;
    private const int EmissionTypeCylinderSurface = 6;
    private const int EmissionTypeCylinder = 7;
    private const int EmissionTypeHemisphereSurface = 8;
    private const int EmissionTypeHemisphere = 9;
    private const int CircleAxisZ = 0;
    private const int CircleAxisY = 1;
    private const int CircleAxisX = 2;
    private const int ChildRotationNone = 0;
    private const int ChildRotationInheritAngle = 1;
    private const int ChildRotationInheritAngleAndVelocity = 2;
    private const int MaxAnimFuncs = 4;
    private const int AirResistanceBase = 0x180;
    private const int FullAlpha = 31;
    private const ushort DefaultColor = 0x7FFF;
    private const int NoCollisionPlaneHeight = int.MinValue;

    private static readonly SplAnimFunc sAnimScale = SplAnim.SPLAnim_Scale;
    private static readonly SplAnimFunc sAnimColor = SplAnim.SPLAnim_Color;
    private static readonly SplAnimFunc sAnimAlpha = SplAnim.SPLAnim_Alpha;
    private static readonly SplAnimFunc sAnimTexture = SplAnim.SPLAnim_Texture;
    private static readonly SplAnimFunc sAnimChildScale = SplAnim.SPLAnim_ChildScale;
    private static readonly SplAnimFunc sAnimChildAlpha = SplAnim.SPLAnim_ChildAlpha;
    private static ushort[]? sDefaultUp;

    private static void AssignPolygonID(SplManager manager, SplParticle particle, bool useFixedID)
    {
        if (useFixedID)
        {
            particle.CurrentPolygonID = (int)manager.PolygonIDFix;
            return;
        }
        particle.CurrentPolygonID = (int)manager.PolygonIDCurrent;
        manager.PolygonIDCurrent = manager.PolygonIDCurrent + 1;
        if (manager.PolygonIDCurrent > manager.PolygonIDMax)
        {
            manager.PolygonIDCurrent = manager.PolygonIDMin;
        }
    }

    private static void Integrate(SplParticle particle, SplEmitter emitter, int airResistance, in VecFx32 acceleration)
    {
        particle.Rotation = (ushort)(particle.Rotation + particle.AngularVelocity);
        particle.Velocity.X = (particle.Velocity.X * airResistance) >> 9;
        particle.Velocity.Y = (particle.Velocity.Y * airResistance) >> 9;
        particle.Velocity.Z = (particle.Velocity.Z * airResistance) >> 9;
        particle.Velocity.X += acceleration.X;
        particle.Velocity.Y += acceleration.Y;
        particle.Velocity.Z += acceleration.Z;
        particle.Position.X += particle.Velocity.X + emitter.Velocity.X;
        particle.Position.Y += particle.Velocity.Y + emitter.Velocity.Y;
        particle.Position.Z += particle.Velocity.Z + emitter.Velocity.Z;
    }

    // OFFSET 0x0209D150
    public static void SPLEmitter_Update(SplManager manager, SplEmitter emitter)
    {
        SplResource resource = emitter.Resource!;
        SplChildResource? child = resource.ChildResource;
        SplResourceHeader header = resource.Header;
        uint flags = header.Flags;
        int animCount = 0;
        int airResistance = header.AirResistance + AirResistanceBase;
        int behaviorCount = resource.BehaviorCount;
        SplAnimFunc?[] animFuncs = new SplAnimFunc?[MaxAnimFuncs];
        Span<int> animLoops = stackalloc int[MaxAnimFuncs];
        Span<int> lifeRates = stackalloc int[2];
        emitter.UpdateCallback?.Invoke(emitter, CallbackPreUpdate);
        if ((header.EmitterLifeTime == 0 || emitter.Age < header.EmitterLifeTime)
            && SplMath.Mod(emitter.Age, emitter.EmissionInterval) == 0
            && (emitter.State & SplEmitter.StateTerminate) == 0
            && (emitter.State & SplEmitter.StateEmissionPaused) == 0
            && (emitter.State & SplEmitter.StateStarted) != 0)
        {
            SPLEmitter_EmitParticles(emitter, manager.InactiveParticles);
        }
        if ((flags & (1u << 8)) != 0)
        {
            animFuncs[animCount] = sAnimScale;
            animLoops[animCount] = resource.ScaleAnim!.Loop ? 1 : 0;
            animCount++;
        }
        if ((flags & (1u << 9)) != 0 && !resource.ColorAnim!.RandomStartColor)
        {
            animFuncs[animCount] = sAnimColor;
            animLoops[animCount] = resource.ColorAnim.Loop ? 1 : 0;
            animCount++;
        }
        if ((flags & (1u << 10)) != 0)
        {
            animFuncs[animCount] = sAnimAlpha;
            animLoops[animCount] = resource.AlphaAnim!.Loop ? 1 : 0;
            animCount++;
        }
        if ((flags & (1u << 11)) != 0 && !resource.TexAnim!.RandomizeInit)
        {
            animFuncs[animCount] = sAnimTexture;
            animLoops[animCount] = resource.TexAnim.Loop ? 1 : 0;
            animCount++;
        }
        bool followEmitter = (flags & (1u << 15)) != 0;
        bool hasChildResource = (flags & (1u << 16)) != 0;
        SplParticle? particle = emitter.Particles.First;
        while (particle != null)
        {
            SplParticle? next = particle.Next;
            lifeRates[0] = (byte)((particle.LifeTimeFactor * particle.Age) >> 8);
            lifeRates[1] = (byte)(particle.LifeRateOffset + ((particle.LoopTimeFactor * particle.Age) >> 8));
            for (int i = 0; i < animCount; i++)
            {
                animFuncs[i]!(particle, resource, lifeRates[animLoops[i]]);
            }
            VecFx32 acceleration = default;
            if (followEmitter)
            {
                particle.EmitterPos = emitter.Position;
            }
            for (int i = 0; i < behaviorCount; i++)
            {
                SplBehaviors.Apply(resource.Behaviors![i]!, particle, ref acceleration, emitter);
            }
            Integrate(particle, emitter, airResistance, acceleration);
            if (hasChildResource)
            {
                int delay = Fx32.Mul(particle.LifeTime << Fx32.Shift, child!.EmissionDelay << Fx32.Shift) >> 8;
                int elapsed = (particle.Age << Fx32.Shift) - delay;
                if (elapsed >= 0 && SplMath.Mod(elapsed >> Fx32.Shift, child.EmissionInterval) == 0)
                {
                    SPLEmitter_EmitChildren(particle, emitter, manager.InactiveParticles);
                }
            }
            AssignPolygonID(manager, particle, emitter.Resource!.Header.HasFixedPolygonID);
            particle.Age++;
            if (particle.Age > particle.LifeTime)
            {
                SplLists.SPLList_PushFront(manager.InactiveParticles, SplLists.SPLList_Erase(emitter.Particles, particle));
            }
            particle = next;
        }
        if (hasChildResource)
        {
            animCount = 0;
            if (child!.HasScaleAnim)
            {
                animFuncs[animCount] = sAnimChildScale;
                animLoops[animCount] = 0;
                animCount++;
            }
            if (child.HasAlphaAnim)
            {
                animFuncs[animCount] = sAnimChildAlpha;
                animLoops[animCount] = 0;
                animCount++;
            }
            if (!child.UsesBehaviors)
            {
                behaviorCount = 0;
            }
            particle = emitter.ChildParticles.First;
            while (particle != null)
            {
                SplParticle? next = particle.Next;
                int lifeRate = (byte)SplMath.Div(particle.Age << 8, particle.LifeTime);
                for (int i = 0; i < animCount; i++)
                {
                    animFuncs[i]!(particle, resource, lifeRate);
                }
                VecFx32 acceleration = default;
                if (child.FollowEmitter)
                {
                    particle.EmitterPos = emitter.Position;
                }
                for (int i = 0; i < behaviorCount; i++)
                {
                    SplBehaviors.Apply(resource.Behaviors![i]!, particle, ref acceleration, emitter);
                }
                Integrate(particle, emitter, airResistance, acceleration);
                AssignPolygonID(manager, particle, emitter.Resource!.Header.ChildHasFixedPolygonID);
                particle.Age++;
                if (particle.Age > particle.LifeTime)
                {
                    SplLists.SPLList_PushFront(manager.InactiveParticles, SplLists.SPLList_Erase(emitter.ChildParticles, particle));
                }
                particle = next;
            }
        }
        emitter.Age++;
        emitter.UpdateCallback?.Invoke(emitter, CallbackPostUpdate);
    }

    // OFFSET 0x0209D998
    public static void SPLEmitter_Init(SplEmitter emitter, SplResource resource, in VecFx32 position)
    {
        emitter.Resource = resource;
        emitter.State = 0;
        SplResourceHeader header = resource.Header;
        emitter.Position.X = position.X + header.EmitterBasePos.X;
        emitter.Position.Y = position.Y + header.EmitterBasePos.Y;
        emitter.Position.Z = position.Z + header.EmitterBasePos.Z;
        emitter.ParticleInitVelocity = default;
        emitter.Velocity = default;
        emitter.Age = 0;
        emitter.EmissionCountFractional = 0;
        emitter.Axis = header.Axis;
        emitter.InitAngle = header.InitAngle;
        emitter.EmissionCount = header.EmissionCount;
        emitter.Radius = header.Radius;
        emitter.Length = header.Length;
        emitter.InitVelPositionAmplifier = header.InitVelPosAmplifier;
        emitter.InitVelAxisAmplifier = header.InitVelAxisAmplifier;
        emitter.BaseScale = header.BaseScale;
        emitter.ParticleLifeTime = header.ParticleLifeTime;
        emitter.Color = DefaultColor;
        emitter.EmissionInterval = header.EmissionInterval;
        emitter.BaseAlpha = header.BaseAlpha;
        emitter.UpdateCycle = 0;
        emitter.Misc &= 0x7FFFF;
        emitter.CollisionPlaneHeight = NoCollisionPlaneHeight;
        emitter.TextureS = (short)(Fx32.One << header.TextureTileCountS);
        emitter.TextureT = (short)(Fx32.One << header.TextureTileCountT);
        if (header.FlipTextureS)
        {
            emitter.TextureS = (short)-emitter.TextureS;
        }
        if (header.FlipTextureT)
        {
            emitter.TextureT = (short)-emitter.TextureT;
        }
        if (header.HasChildResource)
        {
            SplChildResource child = resource.ChildResource!;
            emitter.ChildTextureS = (short)(Fx32.One << child.TextureTileCountS);
            emitter.ChildTextureT = (short)(Fx32.One << child.TextureTileCountT);
            if (child.FlipTextureS)
            {
                emitter.ChildTextureS = (short)-emitter.ChildTextureS;
            }
            if (child.FlipTextureT)
            {
                emitter.ChildTextureT = (short)-emitter.ChildTextureT;
            }
        }
        emitter.Prev = null;
        emitter.Next = null;
        emitter.ChildParticles.First = null;
        emitter.Particles.First = null;
        emitter.ChildParticles.Count = 0;
        emitter.Particles.Count = 0;
        emitter.UpdateCallback = null;
        emitter.UserDataPtr = null;
        emitter.UserData = 0;
    }

    // OFFSET 0x020A05BC
    public static void SPLEmitter_EmitChildren(SplParticle particle, SplEmitter emitter, SplParticleList list)
    {
        SplChildResource child = emitter.Resource!.ChildResource!;
        int velocityRatio = Fx32.Mul(child.VelocityRatio << Fx32.Shift, 0x10);
        for (int i = 0; i < child.EmissionCount; i++)
        {
            SplParticle? spawned = SplLists.SPLList_PopFront(list);
            if (spawned == null)
            {
                return;
            }
            SplLists.SPLList_PushFront(emitter.ChildParticles, spawned);
            spawned.Position = particle.Position;
            spawned.Velocity.X = Fx32.Mul(particle.Velocity.X, velocityRatio) + SplRandom.Range(child.RandomInitVelMag);
            spawned.Velocity.Y = Fx32.Mul(particle.Velocity.Y, velocityRatio) + SplRandom.Range(child.RandomInitVelMag);
            spawned.Velocity.Z = Fx32.Mul(particle.Velocity.Z, velocityRatio) + SplRandom.Range(child.RandomInitVelMag);
            spawned.EmitterPos = particle.EmitterPos;
            spawned.BaseScale = (((particle.BaseScale * particle.AnimScale) >> Fx32.Shift) * (child.ScaleRatio + 1)) >> 6;
            spawned.AnimScale = Fx32.One;
            spawned.Color = child.UseChildColor ? child.Color : particle.Color;
            spawned.BaseAlpha = (particle.BaseAlpha * (particle.AnimAlpha + 1)) >> 5;
            spawned.AnimAlpha = FullAlpha;
            switch (child.RotationType)
            {
                case ChildRotationNone:
                    spawned.Rotation = 0;
                    spawned.AngularVelocity = 0;
                    break;
                case ChildRotationInheritAngle:
                    spawned.Rotation = particle.Rotation;
                    spawned.AngularVelocity = 0;
                    break;
                case ChildRotationInheritAngleAndVelocity:
                    spawned.Rotation = particle.Rotation;
                    spawned.AngularVelocity = particle.AngularVelocity;
                    break;
            }
            spawned.LifeTime = child.LifeTime;
            spawned.Age = 0;
            spawned.Texture = child.Texture;
            spawned.LoopTimeFactor = (ushort)SplMath.Div(0xFFFF, particle.LifeTime >> 1);
            spawned.LifeTimeFactor = (ushort)SplMath.Div(0xFFFF, particle.LifeTime);
            spawned.LifeRateOffset = 0;
        }
    }

    // OFFSET 0x020A08DC
    public static void SPLEmitter_EmitParticles(SplEmitter emitter, SplParticleList list)
    {
        SplResource resource = emitter.Resource!;
        SplResourceHeader header = resource.Header;
        int total = emitter.EmissionCount + emitter.EmissionCountFractional;
        emitter.EmissionCountFractional = (short)(total & 0xFFF);
        int count = total >> Fx32.Shift;
        int emissionType = header.EmissionType;
        if (emissionType == EmissionTypeCircleBorder || emissionType == EmissionTypeCircleBorderUniform || (uint)(emissionType - EmissionTypeCircle) <= 4)
        {
            SPLEmitter_ComputeOrthogonalAxes(emitter);
        }
        int angle = 0;
        for (int i = 0; i < count; i++)
        {
            SplParticle? particle = SplLists.SPLList_PopFront(list);
            if (particle == null)
            {
                return;
            }
            SplLists.SPLList_PushFront(emitter.Particles, particle);
            VecFx32 offset;
            switch (header.EmissionType)
            {
                case EmissionTypePoint:
                    particle.Position = default;
                    break;
                case EmissionTypeSphereSurface:
                    SplRandom.SPLRandom_VecFx32(out particle.Position);
                    particle.Position.X = Fx32.Mul(particle.Position.X, emitter.Radius);
                    particle.Position.Y = Fx32.Mul(particle.Position.Y, emitter.Radius);
                    particle.Position.Z = Fx32.Mul(particle.Position.Z, emitter.Radius);
                    break;
                case EmissionTypeCircleBorder:
                    SplRandom.SPLRandom_VecFx32_XY(out offset);
                    offset.X = Fx32.Mul(offset.X, emitter.Radius);
                    offset.Y = Fx32.Mul(offset.Y, emitter.Radius);
                    offset.Z = 0;
                    SPLUtil_TiltCoordinates(out particle.Position, offset, emitter);
                    break;
                case EmissionTypeCircleBorderUniform:
                {
                    int index = SplMath.Div(angle, count) >> 4;
                    angle += 0x10000;
                    offset.X = Fx32.Mul(Render3D.Sin(index), emitter.Radius);
                    offset.Y = Fx32.Mul(Render3D.Cos(index), emitter.Radius);
                    offset.Z = 0;
                    SPLUtil_TiltCoordinates(out particle.Position, offset, emitter);
                    break;
                }
                case EmissionTypeSphere:
                {
                    SplRandom.SPLRandom_VecFx32(out particle.Position);
                    int factorX = SplRandom.UnitRange();
                    particle.Position.X = Fx32.Mul(Fx32.Mul(particle.Position.X, emitter.Radius), factorX);
                    int factorY = SplRandom.UnitRange();
                    particle.Position.Y = Fx32.Mul(Fx32.Mul(particle.Position.Y, emitter.Radius), factorY);
                    int factorZ = SplRandom.UnitRange();
                    particle.Position.Z = Fx32.Mul(Fx32.Mul(particle.Position.Z, emitter.Radius), factorZ);
                    break;
                }
                case EmissionTypeCircle:
                {
                    SplRandom.SPLRandom_VecFx32_XY(out offset);
                    int factorX = SplRandom.UnitRange();
                    offset.X = Fx32.Mul(Fx32.Mul(offset.X, emitter.Radius), factorX);
                    int factorY = SplRandom.UnitRange();
                    offset.Y = Fx32.Mul(Fx32.Mul(offset.Y, emitter.Radius), factorY);
                    SPLUtil_TiltCoordinates(out particle.Position, offset, emitter);
                    break;
                }
                case EmissionTypeCylinderSurface:
                    SplRandom.SPLRandom_VecFx32_XY(out particle.Velocity);
                    offset.X = Fx32.Mul(particle.Velocity.X, emitter.Radius);
                    offset.Y = Fx32.Mul(particle.Velocity.Y, emitter.Radius);
                    offset.Z = SplRandom.Range(emitter.Length);
                    SPLUtil_TiltCoordinates(out particle.Position, offset, emitter);
                    break;
                case EmissionTypeCylinder:
                {
                    SplRandom.SPLRandom_VecFx32_XY(out particle.Velocity);
                    int factorX = SplRandom.UnitRange();
                    offset.X = Fx32.Mul(Fx32.Mul(particle.Velocity.X, emitter.Radius), factorX);
                    int factorY = SplRandom.UnitRange();
                    offset.Y = Fx32.Mul(Fx32.Mul(particle.Velocity.Y, emitter.Radius), factorY);
                    offset.Z = SplRandom.Range(emitter.Length);
                    SPLUtil_TiltCoordinates(out particle.Position, offset, emitter);
                    break;
                }
                case EmissionTypeHemisphereSurface:
                {
                    SplRandom.SPLRandom_VecFx32(out particle.Position);
                    SplMath.Cross(emitter.CrossAxis1, emitter.CrossAxis2, out VecFx16 normal16);
                    VecFx32 normal = new() { X = normal16.X, Y = normal16.Y, Z = normal16.Z };
                    if (SplMath.Dot(normal, particle.Position) <= 0)
                    {
                        particle.Position.X = -particle.Position.X;
                        particle.Position.Y = -particle.Position.Y;
                        particle.Position.Z = -particle.Position.Z;
                    }
                    particle.Position.X = Fx32.Mul(particle.Position.X, emitter.Radius);
                    particle.Position.Y = Fx32.Mul(particle.Position.Y, emitter.Radius);
                    particle.Position.Z = Fx32.Mul(particle.Position.Z, emitter.Radius);
                    break;
                }
                case EmissionTypeHemisphere:
                {
                    SplRandom.SPLRandom_VecFx32(out particle.Position);
                    SplMath.Cross(emitter.CrossAxis1, emitter.CrossAxis2, out VecFx16 normal16);
                    VecFx32 normal = new() { X = normal16.X, Y = normal16.Y, Z = normal16.Z };
                    if (SplMath.Dot(normal, particle.Position) < 0)
                    {
                        particle.Position.X = -particle.Position.X;
                        particle.Position.Y = -particle.Position.Y;
                        particle.Position.Z = -particle.Position.Z;
                    }
                    int factorX = (SplRandom.UnitRange() >> 1) + 0x800;
                    particle.Position.X = Fx32.Mul(Fx32.Mul(particle.Position.X, emitter.Radius), factorX);
                    int factorY = (SplRandom.UnitRange() >> 1) + 0x800;
                    particle.Position.Y = Fx32.Mul(Fx32.Mul(particle.Position.Y, emitter.Radius), factorY);
                    int factorZ = (SplRandom.UnitRange() >> 1) + 0x800;
                    particle.Position.Z = Fx32.Mul(Fx32.Mul(particle.Position.Z, emitter.Radius), factorZ);
                    break;
                }
            }
            int positionMagnitude = SplRandom.Attenuate(emitter.InitVelPositionAmplifier, header.InitVelAttenuation);
            int axisMagnitude = SplRandom.Attenuate(emitter.InitVelAxisAmplifier, header.InitVelAttenuation);
            VecFx32 direction;
            if (header.EmissionType == EmissionTypeCylinderSurface)
            {
                VecFx32 radial;
                radial.X = Fx32.Mul(particle.Velocity.X, emitter.CrossAxis1.X) + Fx32.Mul(particle.Velocity.Y, emitter.CrossAxis2.X);
                radial.Y = Fx32.Mul(particle.Velocity.X, emitter.CrossAxis1.Y) + Fx32.Mul(particle.Velocity.Y, emitter.CrossAxis2.Y);
                radial.Z = Fx32.Mul(particle.Velocity.X, emitter.CrossAxis1.Z) + Fx32.Mul(particle.Velocity.Y, emitter.CrossAxis2.Z);
                SplMath.Normalize(radial, out direction);
            }
            else if (particle.Position.X == 0 && particle.Position.Y == 0 && particle.Position.Z == 0)
            {
                SplRandom.SPLRandom_VecFx32(out direction);
            }
            else
            {
                SplMath.Normalize(particle.Position, out direction);
            }
            particle.Velocity.X = emitter.ParticleInitVelocity.X + (Fx32.Mul(direction.X, positionMagnitude) + Fx32.Mul(emitter.Axis.X, axisMagnitude));
            particle.Velocity.Y = emitter.ParticleInitVelocity.Y + (Fx32.Mul(direction.Y, positionMagnitude) + Fx32.Mul(emitter.Axis.Y, axisMagnitude));
            particle.Velocity.Z = emitter.ParticleInitVelocity.Z + (Fx32.Mul(direction.Z, positionMagnitude) + Fx32.Mul(emitter.Axis.Z, axisMagnitude));
            particle.EmitterPos = emitter.Position;
            particle.BaseScale = SplRandom.Attenuate(emitter.BaseScale, header.BaseScaleAttenuation);
            particle.AnimScale = Fx32.One;
            if (header.HasColorAnim && resource.ColorAnim!.RandomStartColor)
            {
                uint index = (SplRandom.Next() >> 20) % 3;
                particle.Color = index switch
                {
                    0 => resource.ColorAnim.Start,
                    1 => header.Color,
                    _ => resource.ColorAnim.End,
                };
            }
            else
            {
                particle.Color = header.Color;
            }
            particle.BaseAlpha = emitter.BaseAlpha;
            particle.AnimAlpha = FullAlpha;
            if (header.RandomInitAngle)
            {
                particle.Rotation = (ushort)SplRandom.Next();
            }
            else
            {
                particle.Rotation = emitter.InitAngle;
            }
            if (!header.HasRotation)
            {
                particle.AngularVelocity = 0;
            }
            else
            {
                int random = (int)(SplRandom.Next() >> 20);
                particle.AngularVelocity = (short)(((header.MaxRotation - header.MinRotation) * random + (header.MinRotation << Fx32.Shift)) >> Fx32.Shift);
            }
            int lifeRandom = (int)(SplRandom.Next() >> 24);
            particle.LifeTime = (ushort)(((emitter.ParticleLifeTime * (0xFF - ((header.LifeTimeAttenuation * lifeRandom) >> 8))) >> 8) + 1);
            particle.Age = 0;
            if (header.HasTexAnim && resource.TexAnim!.RandomizeInit)
            {
                uint frame = SplMath.ModU(SplRandom.Next() >> 20, resource.TexAnim.FrameCount);
                particle.Texture = resource.TexAnim.Textures[frame];
            }
            else if (header.HasTexAnim && !resource.TexAnim!.RandomizeInit)
            {
                particle.Texture = resource.TexAnim.Textures[0];
            }
            else
            {
                particle.Texture = header.TextureIndex;
            }
            particle.LoopTimeFactor = (ushort)SplMath.Div(0xFFFF, header.LoopFrames);
            particle.LifeTimeFactor = (ushort)SplMath.Div(0xFFFF, particle.LifeTime);
            particle.LifeRateOffset = 0;
            if (header.RandomizeLoopedAnim)
            {
                particle.LifeRateOffset = (byte)(SplRandom.Next() >> 24);
            }
        }
    }

    // OFFSET 0x020A1608
    public static void SPLUtil_TiltCoordinates(out VecFx32 result, in VecFx32 source, SplEmitter emitter)
    {
        SplMath.Cross(emitter.CrossAxis1, emitter.CrossAxis2, out VecFx16 normal);
        SplMath.Normalize(normal, out normal);
        VecFx32 tilted;
        tilted.X = Fx32.Mul(source.Z, normal.X) + (Fx32.Mul(source.X, emitter.CrossAxis1.X) + Fx32.Mul(source.Y, emitter.CrossAxis2.X));
        tilted.Y = Fx32.Mul(source.Z, normal.Y) + (Fx32.Mul(source.X, emitter.CrossAxis1.Y) + Fx32.Mul(source.Y, emitter.CrossAxis2.Y));
        tilted.Z = Fx32.Mul(source.Z, normal.Z) + (Fx32.Mul(source.X, emitter.CrossAxis1.Z) + Fx32.Mul(source.Y, emitter.CrossAxis2.Z));
        result = tilted;
    }

    // OFFSET 0x020A1768
    public static void SPLEmitter_ComputeOrthogonalAxes(SplEmitter emitter)
    {
        sDefaultUp ??= RomData.Arm9U16(0x02100DB0, 3);
        VecFx16 up = new() { X = (short)sDefaultUp[0], Y = (short)sDefaultUp[1], Z = (short)sDefaultUp[2] };
        VecFx16 axis;
        switch (emitter.Resource!.Header.CircleAxis)
        {
            case CircleAxisZ:
                axis = new VecFx16 { X = 0, Y = 0, Z = Fx32.One };
                break;
            case CircleAxisY:
                axis = new VecFx16 { X = 0, Y = Fx32.One, Z = 0 };
                break;
            case CircleAxisX:
                axis = new VecFx16 { X = Fx32.One, Y = 0, Z = 0 };
                break;
            default:
                SplMath.Normalize(emitter.Axis, out axis);
                break;
        }
        int dot = SplMath.Dot(up, axis);
        if (dot == Fx32.One || dot == -Fx32.One)
        {
            up = new VecFx16 { X = Fx32.One, Y = 0, Z = 0 };
        }
        emitter.CrossAxis1.X = (short)(Fx32.Mul(axis.Y, up.Z) - Fx32.Mul(axis.Z, up.Y));
        emitter.CrossAxis1.Y = (short)(Fx32.Mul(axis.Z, up.X) - Fx32.Mul(axis.X, up.Z));
        emitter.CrossAxis1.Z = (short)(Fx32.Mul(axis.X, up.Y) - Fx32.Mul(axis.Y, up.X));
        emitter.CrossAxis2.X = (short)(Fx32.Mul(axis.Y, emitter.CrossAxis1.Z) - Fx32.Mul(axis.Z, emitter.CrossAxis1.Y));
        emitter.CrossAxis2.Y = (short)(Fx32.Mul(axis.Z, emitter.CrossAxis1.X) - Fx32.Mul(axis.X, emitter.CrossAxis1.Z));
        emitter.CrossAxis2.Z = (short)(Fx32.Mul(axis.X, emitter.CrossAxis1.Y) - Fx32.Mul(axis.Y, emitter.CrossAxis1.X));
        SplMath.Normalize(emitter.CrossAxis1, out emitter.CrossAxis1);
        SplMath.Normalize(emitter.CrossAxis2, out emitter.CrossAxis2);
    }
}
