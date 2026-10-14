namespace PlatinumSharp.Game;

public struct VecFx16
{
    public short X;
    public short Y;
    public short Z;
}

public struct MtxFx43
{
    public int M00;
    public int M01;
    public int M02;
    public int M10;
    public int M11;
    public int M12;
    public int M20;
    public int M21;
    public int M22;
    public int M30;
    public int M31;
    public int M32;
}

public delegate uint SplAllocFunc(uint size);

public delegate uint SplTexVramAllocFunc(uint size, int is4x4Comp);

public delegate uint SplPalVramAllocFunc(uint size, int is4Pal);

public delegate void SplEmitterCallback(SplEmitter emitter);

public delegate void SplEmitterUpdateCallback(SplEmitter emitter, int type);

public delegate void SplDrawFunc(SplManager manager, SplParticle particle);

public delegate void SplSetTextureFunc(SplTexture texture);

public delegate void SplAnimFunc(SplParticle particle, SplResource resource, int lifeRate);

public delegate void SplRotateFunc(int sin, int cos, out MtxFx43 matrix);

public delegate void SplDrawPlaneFunc(int s, int t, int offsetX, int offsetY);

public enum SplBehaviorType
{
    Gravity,
    Random,
    Magnet,
    Spin,
    CollisionPlane,
    Convergence,
}

public sealed class SplGravityBehavior
{
    public VecFx16 Magnitude;
    public ushort Padding;
}

public sealed class SplRandomBehavior
{
    public VecFx16 Magnitude;
    public ushort ApplyInterval;
}

public sealed class SplMagnetBehavior
{
    public VecFx32 Target;
    public short Force;
    public ushort Padding;
}

public sealed class SplSpinBehavior
{
    public ushort Angle;
    public ushort Axis;
}

public sealed class SplCollisionPlaneBehavior
{
    public int Y;
    public short Elasticity;
    public ushort CollisionType;
}

public sealed class SplConvergenceBehavior
{
    public VecFx32 Target;
    public short Force;
    public ushort Padding;
}

public sealed class SplBehavior
{
    public SplBehaviorType ApplyFunc;
    public object? Object;
}

public sealed class SplResourceHeader
{
    public const int Size = 0x58;

    public uint Flags;
    public VecFx32 EmitterBasePos;
    public int EmissionCount;
    public int Radius;
    public int Length;
    public VecFx16 Axis;
    public ushort Color;
    public int InitVelPosAmplifier;
    public int InitVelAxisAmplifier;
    public int BaseScale;
    public short AspectRatio;
    public ushort StartDelay;
    public short MinRotation;
    public short MaxRotation;
    public ushort InitAngle;
    public ushort Reserved;
    public ushort EmitterLifeTime;
    public ushort ParticleLifeTime;
    public byte BaseScaleAttenuation;
    public byte LifeTimeAttenuation;
    public byte InitVelAttenuation;
    public byte EmissionInterval;
    public byte BaseAlpha;
    public byte AirResistance;
    public byte TextureIndex;
    public byte LoopFrames;
    public ushort DbbScale;
    public byte TextureTileCountS;
    public byte TextureTileCountT;
    public byte ScaleAnimDir;
    public bool DpolFaceEmitter;
    public bool FlipTextureS;
    public bool FlipTextureT;
    public short PolygonX;
    public short PolygonY;
    public uint UserData;

    public int EmissionType => (int)(Flags & 0xF);

    public int DrawType => (int)((Flags >> 4) & 3);

    public int CircleAxis => (int)((Flags >> 6) & 3);

    public bool HasScaleAnim => (Flags & (1u << 8)) != 0;

    public bool HasColorAnim => (Flags & (1u << 9)) != 0;

    public bool HasAlphaAnim => (Flags & (1u << 10)) != 0;

    public bool HasTexAnim => (Flags & (1u << 11)) != 0;

    public bool HasRotation => (Flags & (1u << 12)) != 0;

    public bool RandomInitAngle => (Flags & (1u << 13)) != 0;

    public bool SelfMaintaining => (Flags & (1u << 14)) != 0;

    public bool FollowEmitter => (Flags & (1u << 15)) != 0;

    public bool HasChildResource => (Flags & (1u << 16)) != 0;

    public int PolygonRotAxis => (int)((Flags >> 17) & 3);

    public int PolygonReferencePlane => (int)((Flags >> 19) & 1);

    public bool RandomizeLoopedAnim => (Flags & (1u << 20)) != 0;

    public bool DrawChildrenFirst => (Flags & (1u << 21)) != 0;

    public bool HideParent => (Flags & (1u << 22)) != 0;

    public bool UseViewSpace => (Flags & (1u << 23)) != 0;

    public bool HasGravityBehavior => (Flags & (1u << 24)) != 0;

    public bool HasRandomBehavior => (Flags & (1u << 25)) != 0;

    public bool HasMagnetBehavior => (Flags & (1u << 26)) != 0;

    public bool HasSpinBehavior => (Flags & (1u << 27)) != 0;

    public bool HasCollisionPlaneBehavior => (Flags & (1u << 28)) != 0;

    public bool HasConvergenceBehavior => (Flags & (1u << 29)) != 0;

    public bool HasFixedPolygonID => (Flags & (1u << 30)) != 0;

    public bool ChildHasFixedPolygonID => (Flags & (1u << 31)) != 0;
}

public sealed class SplScaleAnim
{
    public const int Size = 0xC;

    public short Start;
    public short Mid;
    public short End;
    public byte CurveIn;
    public byte CurveOut;
    public bool Loop;
}

public sealed class SplColorAnim
{
    public const int Size = 0xC;

    public ushort Start;
    public ushort End;
    public byte CurveIn;
    public byte CurvePeak;
    public byte CurveOut;
    public bool RandomStartColor;
    public bool Loop;
    public bool Interpolate;
}

public sealed class SplAlphaAnim
{
    public const int Size = 8;

    public byte Start;
    public byte Mid;
    public byte End;
    public byte RandomRange;
    public bool Loop;
    public byte CurveIn;
    public byte CurveOut;
}

public sealed class SplTexAnim
{
    public const int Size = 0xC;
    public const int MaxFrames = 8;

    public readonly byte[] Textures = new byte[MaxFrames];
    public byte FrameCount;
    public byte Step;
    public bool RandomizeInit;
    public bool Loop;
}

public sealed class SplChildResource
{
    public const int Size = 0x14;

    public ushort Flags;
    public short RandomInitVelMag;
    public short EndScale;
    public ushort LifeTime;
    public byte VelocityRatio;
    public byte ScaleRatio;
    public ushort Color;
    public byte EmissionCount;
    public byte EmissionDelay;
    public byte EmissionInterval;
    public byte Texture;
    public byte TextureTileCountS;
    public byte TextureTileCountT;
    public bool FlipTextureS;
    public bool FlipTextureT;
    public bool DpolFaceEmitter;

    public bool UsesBehaviors => (Flags & (1 << 0)) != 0;

    public bool HasScaleAnim => (Flags & (1 << 1)) != 0;

    public bool HasAlphaAnim => (Flags & (1 << 2)) != 0;

    public int RotationType => (Flags >> 3) & 3;

    public bool FollowEmitter => (Flags & (1 << 5)) != 0;

    public bool UseChildColor => (Flags & (1 << 6)) != 0;

    public int DrawType => (Flags >> 7) & 3;

    public int PolygonRotAxis => (Flags >> 9) & 3;

    public int PolygonReferencePlane => (Flags >> 11) & 1;
}

public sealed class SplResource
{
    public const int Size = 0x20;

    public SplResourceHeader Header = new();
    public SplScaleAnim? ScaleAnim;
    public SplColorAnim? ColorAnim;
    public SplAlphaAnim? AlphaAnim;
    public SplTexAnim? TexAnim;
    public SplChildResource? ChildResource;
    public SplBehavior?[]? Behaviors;
    public ushort BehaviorCount;
}

public sealed class SplTextureResource
{
    public const int Size = 0x20;

    public byte[] Data = Array.Empty<byte>();
    public int Offset;
    public uint ID;
    public uint Param;
    public uint TextureSize;
    public uint PaletteOffset;
    public uint PaletteSize;
    public uint ResourceSize;
}

public sealed class SplTexture
{
    public const int Size = 0x14;

    public SplTextureResource? Resource;
    public uint TexAddr;
    public uint PalAddr;
    public uint Param;
    public ushort Width;
    public ushort Height;
}

public class SplNode<T>
    where T : SplNode<T>
{
    public T? Next;
    public T? Prev;
}

public class SplList<T>
    where T : SplNode<T>
{
    public T? First;
    public int Count;
    public T? Last;
}

public sealed class SplParticle : SplNode<SplParticle>
{
    public const int Size = 0x44;

    public VecFx32 Position;
    public VecFx32 Velocity;
    public ushort Rotation;
    public short AngularVelocity;
    public ushort LifeTime;
    public ushort Age;
    public ushort LoopTimeFactor;
    public ushort LifeTimeFactor;
    public byte Texture;
    public byte LifeRateOffset;
    public ushort Visibility;
    public int BaseScale;
    public short AnimScale;
    public ushort Color;
    public VecFx32 EmitterPos;

    public int BaseAlpha
    {
        get => Visibility & 0x1F;
        set => Visibility = (ushort)((Visibility & ~0x1F) | (value & 0x1F));
    }

    public int AnimAlpha
    {
        get => (Visibility >> 5) & 0x1F;
        set => Visibility = (ushort)((Visibility & ~0x3E0) | ((value & 0x1F) << 5));
    }

    public int CurrentPolygonID
    {
        get => (Visibility >> 10) & 0x3F;
        set => Visibility = (ushort)((Visibility & ~0xFC00) | ((value & 0x3F) << 10));
    }
}

public sealed class SplParticleList : SplList<SplParticle>
{
}

public sealed class SplEmitter : SplNode<SplEmitter>
{
    public const int Size = 0x9C;
    public const uint StateTerminate = 1 << 0;
    public const uint StateEmissionPaused = 1 << 1;
    public const uint StatePaused = 1 << 2;
    public const uint StateRenderingDisabled = 1 << 3;
    public const uint StateStarted = 1 << 4;

    public readonly SplParticleList Particles = new();
    public readonly SplParticleList ChildParticles = new();
    public SplResource? Resource;
    public uint State;
    public VecFx32 Position;
    public VecFx32 Velocity;
    public VecFx32 ParticleInitVelocity;
    public ushort Age;
    public short EmissionCountFractional;
    public VecFx16 Axis;
    public ushort InitAngle;
    public int EmissionCount;
    public int Radius;
    public int Length;
    public int InitVelPositionAmplifier;
    public int InitVelAxisAmplifier;
    public int BaseScale;
    public ushort ParticleLifeTime;
    public ushort Color;
    public int CollisionPlaneHeight;
    public short TextureS;
    public short TextureT;
    public short ChildTextureS;
    public short ChildTextureT;
    public uint Misc;
    public VecFx16 CrossAxis1;
    public VecFx16 CrossAxis2;
    public SplEmitterUpdateCallback? UpdateCallback;
    public object? UserDataPtr;
    public uint UserData;

    public int EmissionInterval
    {
        get => (int)(Misc & 0xFF);
        set => Misc = (Misc & ~0xFFu) | ((uint)value & 0xFF);
    }

    public int BaseAlpha
    {
        get => (int)((Misc >> 8) & 0xFF);
        set => Misc = (Misc & ~0xFF00u) | (((uint)value & 0xFF) << 8);
    }

    public int UpdateCycle
    {
        get => (int)((Misc >> 16) & 7);
        set => Misc = (Misc & ~0x70000u) | (((uint)value & 7) << 16);
    }
}

public sealed class SplEmitterList : SplList<SplEmitter>
{
}

public sealed class SplManager
{
    public const int Size = 0x4C;

    public SplAllocFunc? Alloc;
    public readonly SplEmitterList ActiveEmitters = new();
    public readonly SplEmitterList InactiveEmitters = new();
    public readonly SplParticleList InactiveParticles = new();
    public SplResource[]? Resources;
    public SplTexture[]? Textures;
    public ushort ResCount;
    public ushort TexCount;
    public ushort MaxEmitters;
    public ushort MaxParticles;
    public uint PolygonID;
    public uint MiscPolygonAttr;
    public SplEmitter? RenderEmitter;
    public MtxFx43 ViewMatrix;
    public ushort CurrentCycle;
    public ushort Reserved;

    public uint PolygonIDMin
    {
        get => PolygonID & 0x3F;
        set => PolygonID = (PolygonID & ~0x3Fu) | (value & 0x3F);
    }

    public uint PolygonIDMax
    {
        get => (PolygonID >> 6) & 0x3F;
        set => PolygonID = (PolygonID & ~0xFC0u) | ((value & 0x3F) << 6);
    }

    public uint PolygonIDCurrent
    {
        get => (PolygonID >> 12) & 0x3F;
        set => PolygonID = (PolygonID & ~0x3F000u) | ((value & 0x3F) << 12);
    }

    public uint PolygonIDFix
    {
        get => (PolygonID >> 18) & 0x3F;
        set => PolygonID = (PolygonID & ~0xFC0000u) | ((value & 0x3F) << 18);
    }

    public uint DrawOrder
    {
        get => (PolygonID >> 24) & 1;
        set => PolygonID = (PolygonID & ~0x1000000u) | ((value & 1) << 24);
    }
}
