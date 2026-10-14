using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class ParticleSystem
{
    public const int MaxParticleSystems = 16;
    public const int MaxTextureKeys = 16;
    public const int MaxPaletteKeys = 16;
    public const int VramAutoReleaseNone = 0;
    public const int VramAutoReleaseTextureFrm = 1 << 0;
    public const int VramAutoReleaseTextureLnk = 1 << 1;
    public const int VramAutoReleasePaletteFrm = 1 << 2;
    public const int VramAutoReleasePaletteLnk = 1 << 3;

    private const int MaxEmitters = 0x14;
    private const int MaxParticles = 0xC8;
    private const int FixedPolygonID = 5;
    private const int MinPolygonID = 6;
    private const int MaxPolygonID = 0x3F;
    private const ushort DefaultCameraFov = 0x2000;
    private const uint UploadTaskPriority = 5;

    public SplManager? Manager;
    public byte[]? Resource;
    public SplEmitter? LastAddedEmitter;
    public byte[]? HeapStart;
    public uint Heap;
    public uint HeapEnd;
    public SplTexVramAllocFunc? TexAllocFunc;
    public SplPalVramAllocFunc? PalAllocFunc;
    public Camera? Camera;
    public VecFx32 Unused1;
    public ushort CameraFov;
    public VecFx32 CameraPos;
    public VecFx32 CameraUp;
    public VecFx32 CameraTarget;
    public readonly uint[] TextureKeys = new uint[MaxTextureKeys];
    public readonly uint[] PaletteKeys = new uint[MaxPaletteKeys];
    public byte VramAutoRelease;
    public byte Unused2;
    public byte ID;
    public byte CameraProjection;

    private static ParticleSystem? sUploadingParticleSystem;
    private static object? sEmitterCallbackParam;
    private static readonly ParticleSystem?[] sParticleSystems = new ParticleSystem?[MaxParticleSystems];

    private static readonly SplAllocFunc[] sAllocFuncs =
    {
        ParticleSystem_AllocFromHeap0,
        ParticleSystem_AllocFromHeap1,
        ParticleSystem_AllocFromHeap2,
        ParticleSystem_AllocFromHeap3,
        ParticleSystem_AllocFromHeap4,
        ParticleSystem_AllocFromHeap5,
        ParticleSystem_AllocFromHeap6,
        ParticleSystem_AllocFromHeap7,
        ParticleSystem_AllocFromHeap8,
        ParticleSystem_AllocFromHeap9,
        ParticleSystem_AllocFromHeap10,
        ParticleSystem_AllocFromHeap11,
        ParticleSystem_AllocFromHeap12,
        ParticleSystem_AllocFromHeap13,
        ParticleSystem_AllocFromHeap14,
        ParticleSystem_AllocFromHeap15,
    };

    private static VecFx32 LoadVec(uint address)
    {
        uint[] raw = RomData.Arm9U32(address, 3);
        return new VecFx32 { X = (int)raw[0], Y = (int)raw[1], Z = (int)raw[2] };
    }

    private static VecFx32 DefaultCameraUp => LoadVec(0x020E5430);

    private static VecFx32 DefaultCameraTarget => LoadVec(0x020E543C);

    private static VecFx32 DefaultCameraPos => LoadVec(0x020E5448);

    private static uint AllocFromHeap(int id, uint size)
    {
        ParticleSystem particleSystem = sParticleSystems[id]!;
        uint start = particleSystem.Heap;
        uint end = start + size;
        uint remainder = end & 3;
        if (remainder != 0)
        {
            end += 4 - remainder;
        }
        particleSystem.Heap = end;
        if (end >= particleSystem.HeapEnd)
        {
            ErrorHandling.GF_AssertFail();
        }
        return start;
    }

    // OFFSET 0x02014000
    public static void ParticleSystem_ZeroAll()
    {
        for (int i = 0; i < MaxParticleSystems; i++)
        {
            sParticleSystems[i] = null;
        }
    }

    // OFFSET 0x02014014
    public static ParticleSystem? ParticleSystem_New(SplTexVramAllocFunc? texAllocFunc, SplPalVramAllocFunc? palAllocFunc, byte[] heap, int heapSize, int hasCamera, int heapID)
    {
        int id;
        for (id = 0; id < MaxParticleSystems; id++)
        {
            if (sParticleSystems[id] == null)
            {
                break;
            }
        }
        if (id >= MaxParticleSystems)
        {
            return null;
        }
        ParticleSystem particleSystem = new();
        particleSystem.TexAllocFunc = texAllocFunc;
        particleSystem.PalAllocFunc = palAllocFunc;
        particleSystem.CameraPos = DefaultCameraPos;
        particleSystem.CameraUp = DefaultCameraUp;
        particleSystem.CameraTarget = DefaultCameraTarget;
        Array.Clear(heap, 0, heapSize);
        particleSystem.HeapStart = heap;
        particleSystem.Heap = 0;
        particleSystem.HeapEnd = (uint)heapSize;
        particleSystem.ID = (byte)id;
        sParticleSystems[id] = particleSystem;
        if (hasCamera == 1)
        {
            particleSystem.Camera = Camera.Camera_Alloc(heapID);
            particleSystem.Unused1 = default;
            particleSystem.CameraFov = DefaultCameraFov;
            Camera.Camera_InitWithTargetAndPosition(() => DefaultCameraTarget, DefaultCameraPos, particleSystem.CameraFov, 0, false, particleSystem.Camera);
            particleSystem.CameraProjection = 0;
            Camera.Camera_SetAsActive(particleSystem.Camera);
        }
        particleSystem.Manager = SplManagers.SPLManager_New(sAllocFuncs[id], MaxEmitters, MaxParticles, FixedPolygonID, MinPolygonID, MaxPolygonID);
        ParticleSystem_SetCameraUp(particleSystem, DefaultCameraUp);
        return particleSystem;
    }

    // OFFSET 0x0201411C
    public static void ParticleSystem_Free(ParticleSystem particleSystem)
    {
        ParticleSystem_DeleteAllEmitters(particleSystem);
        if ((particleSystem.VramAutoRelease & VramAutoReleaseTextureFrm) != 0)
        {
            NnsGfd.SetFrmTexVramState(particleSystem.TextureKeys);
        }
        else if ((particleSystem.VramAutoRelease & VramAutoReleaseTextureLnk) != 0)
        {
            for (int i = 0; i < MaxTextureKeys; i++)
            {
                if (particleSystem.TextureKeys[i] != 0)
                {
                    NnsGfd.FreeLnkTexVram(particleSystem.TextureKeys[i]);
                    particleSystem.TextureKeys[i] = 0;
                }
            }
        }
        if ((particleSystem.VramAutoRelease & VramAutoReleasePaletteFrm) != 0)
        {
            NnsGfd.SetFrmPlttVramState(particleSystem.PaletteKeys);
        }
        else if ((particleSystem.VramAutoRelease & VramAutoReleasePaletteLnk) != 0)
        {
            for (int i = 0; i < MaxPaletteKeys; i++)
            {
                if (particleSystem.PaletteKeys[i] != 0)
                {
                    NnsGfd.FreeLnkPlttVram(particleSystem.PaletteKeys[i]);
                    particleSystem.PaletteKeys[i] = 0;
                }
            }
        }
        particleSystem.VramAutoRelease = VramAutoReleaseNone;
        particleSystem.LastAddedEmitter = null;
        if (particleSystem.Resource != null)
        {
            particleSystem.Resource = null;
        }
        for (int i = 0; i < MaxParticleSystems; i++)
        {
            if (sParticleSystems[i] == particleSystem)
            {
                sParticleSystems[i] = null;
                break;
            }
        }
        if (particleSystem.Camera != null)
        {
            Camera.Camera_Delete(particleSystem.Camera);
        }
    }

    // OFFSET 0x020141E4
    public static void ParticleSystem_FreeAll()
    {
        for (int i = 0; i < MaxParticleSystems; i++)
        {
            if (sParticleSystems[i] != null)
            {
                ParticleSystem_Free(sParticleSystems[i]!);
            }
        }
    }

    // OFFSET 0x02014204
    private static uint ParticleSystem_AllocFromHeap0(uint size)
    {
        return AllocFromHeap(0, size);
    }

    // OFFSET 0x02014230
    private static uint ParticleSystem_AllocFromHeap1(uint size)
    {
        return AllocFromHeap(1, size);
    }

    // OFFSET 0x0201425C
    private static uint ParticleSystem_AllocFromHeap2(uint size)
    {
        return AllocFromHeap(2, size);
    }

    // OFFSET 0x02014288
    private static uint ParticleSystem_AllocFromHeap3(uint size)
    {
        return AllocFromHeap(3, size);
    }

    // OFFSET 0x020142B4
    private static uint ParticleSystem_AllocFromHeap4(uint size)
    {
        return AllocFromHeap(4, size);
    }

    // OFFSET 0x020142E0
    private static uint ParticleSystem_AllocFromHeap5(uint size)
    {
        return AllocFromHeap(5, size);
    }

    // OFFSET 0x0201430C
    private static uint ParticleSystem_AllocFromHeap6(uint size)
    {
        return AllocFromHeap(6, size);
    }

    // OFFSET 0x02014338
    private static uint ParticleSystem_AllocFromHeap7(uint size)
    {
        return AllocFromHeap(7, size);
    }

    // OFFSET 0x02014364
    private static uint ParticleSystem_AllocFromHeap8(uint size)
    {
        return AllocFromHeap(8, size);
    }

    // OFFSET 0x02014390
    private static uint ParticleSystem_AllocFromHeap9(uint size)
    {
        return AllocFromHeap(9, size);
    }

    // OFFSET 0x020143BC
    private static uint ParticleSystem_AllocFromHeap10(uint size)
    {
        return AllocFromHeap(10, size);
    }

    // OFFSET 0x020143E8
    private static uint ParticleSystem_AllocFromHeap11(uint size)
    {
        return AllocFromHeap(11, size);
    }

    // OFFSET 0x02014414
    private static uint ParticleSystem_AllocFromHeap12(uint size)
    {
        return AllocFromHeap(12, size);
    }

    // OFFSET 0x02014440
    private static uint ParticleSystem_AllocFromHeap13(uint size)
    {
        return AllocFromHeap(13, size);
    }

    // OFFSET 0x0201446C
    private static uint ParticleSystem_AllocFromHeap14(uint size)
    {
        return AllocFromHeap(14, size);
    }

    // OFFSET 0x02014498
    private static uint ParticleSystem_AllocFromHeap15(uint size)
    {
        return AllocFromHeap(15, size);
    }

    // OFFSET 0x020144C4
    public static byte[] ParticleSystem_LoadResourceFromNARC(int narcID, int memberIndex, int heapID)
    {
        return NARC.NARC_AllocAndReadWholeMemberByIndexPair(narcID, memberIndex, heapID);
    }

    // OFFSET 0x020144CC
    public static void ParticleSystem_SetResource(ParticleSystem particleSystem, byte[] resource, int autoRelease, int uploadImmediately)
    {
        if (particleSystem.Manager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (particleSystem.Resource != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        particleSystem.VramAutoRelease = (byte)autoRelease;
        if ((autoRelease & VramAutoReleaseTextureFrm) != 0)
        {
            NnsGfd.GetFrmTexVramState(particleSystem.TextureKeys);
        }
        else if ((autoRelease & VramAutoReleaseTextureLnk) != 0)
        {
            for (int i = 0; i < MaxTextureKeys; i++)
            {
                particleSystem.TextureKeys[i] = 0;
            }
        }
        if ((autoRelease & VramAutoReleasePaletteFrm) != 0)
        {
            NnsGfd.GetFrmPlttVramState(particleSystem.PaletteKeys);
        }
        else if ((autoRelease & VramAutoReleasePaletteLnk) != 0)
        {
            for (int i = 0; i < MaxPaletteKeys; i++)
            {
                particleSystem.PaletteKeys[i] = 0;
            }
        }
        particleSystem.Resource = resource;
        if (uploadImmediately == 1)
        {
            ParticleSystem_UploadResource(particleSystem);
            return;
        }
        SysTask.SysTask_ExecuteAfterVBlank(ParticleSystem_UploadResourceTask, particleSystem, UploadTaskPriority);
    }

    // OFFSET 0x02014560
    private static void ParticleSystem_UploadResource(ParticleSystem particleSystem)
    {
        SplManagers.SPLManager_LoadResources(particleSystem.Manager!, particleSystem.Resource!);
        sUploadingParticleSystem = particleSystem;
        if (particleSystem.TexAllocFunc == null)
        {
            SplManagers.SPLManager_UploadTextures(particleSystem.Manager!);
        }
        else
        {
            SplManagers.SPLManager_UploadTexturesEx(particleSystem.Manager!, particleSystem.TexAllocFunc);
        }
        if (particleSystem.PalAllocFunc == null)
        {
            SplManagers.SPLManager_UploadPalettes(particleSystem.Manager!);
        }
        else
        {
            SplManagers.SPLManager_UploadPalettesEx(particleSystem.Manager!, particleSystem.PalAllocFunc);
        }
        sUploadingParticleSystem = null;
    }

    // OFFSET 0x020145A0
    private static void ParticleSystem_UploadResourceTask(SysTask task, object? param)
    {
        ParticleSystem_UploadResource((ParticleSystem)param!);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x020145B4
    public static void ParticleSystem_RegisterTextureKey(uint key)
    {
        if (key == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (sUploadingParticleSystem == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < MaxTextureKeys; i++)
        {
            if (sUploadingParticleSystem!.TextureKeys[i] == 0)
            {
                sUploadingParticleSystem.TextureKeys[i] = key;
                return;
            }
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x020145F4
    public static void ParticleSystem_RegisterPaletteKey(uint key)
    {
        if (key == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (sUploadingParticleSystem == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < MaxPaletteKeys; i++)
        {
            if (sUploadingParticleSystem!.PaletteKeys[i] == 0)
            {
                sUploadingParticleSystem.PaletteKeys[i] = key;
                return;
            }
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x02014638
    public static void ParticleSystem_Draw(ParticleSystem particleSystem)
    {
        if (particleSystem.Camera != null)
        {
            Camera.Camera_ComputeProjectionMatrix(particleSystem.CameraProjection, particleSystem.Camera);
            Camera.Camera_SetAsActive(particleSystem.Camera);
            Camera.Camera_ComputeViewMatrix();
        }
        Render3D.FlushGlobalState();
        SplManagers.SPLManager_Draw(particleSystem.Manager!);
        if (particleSystem.Camera != null)
        {
            Camera.Camera_ClearActive();
        }
        Render3D.FlushGlobalState();
    }

    // OFFSET 0x02014674
    public static void ParticleSystem_Update(ParticleSystem particleSystem)
    {
        SplManagers.SPLManager_Update(particleSystem.Manager!);
    }

    // OFFSET 0x02014680
    public static int ParticleSystem_GetActiveAmount()
    {
        int count = 0;
        for (int i = 0; i < MaxParticleSystems; i++)
        {
            if (sParticleSystems[i] != null)
            {
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x0201469C
    public static int ParticleSystem_DrawAll()
    {
        int count = 0;
        for (int i = 0; i < MaxParticleSystems; i++)
        {
            if (sParticleSystems[i] != null)
            {
                ParticleSystem_Draw(sParticleSystems[i]!);
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x020146C0
    public static int ParticleSystem_UpdateAll()
    {
        int count = 0;
        for (int i = 0; i < MaxParticleSystems; i++)
        {
            if (sParticleSystems[i] != null)
            {
                ParticleSystem_Update(sParticleSystems[i]!);
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x020146E4
    public static SplEmitter? ParticleSystem_CreateEmitter(ParticleSystem particleSystem, int resourceID, in VecFx32 position)
    {
        particleSystem.LastAddedEmitter = SplManagers.SPLManager_CreateEmitter(particleSystem.Manager!, resourceID, position);
        return particleSystem.LastAddedEmitter;
    }

    // OFFSET 0x020146F4
    public static SplEmitter? ParticleSystem_CreateEmitterWithCallback(ParticleSystem particleSystem, int resourceID, SplEmitterCallback? callback, object? param)
    {
        sEmitterCallbackParam = param;
        SplEmitter? emitter = SplManagers.SPLManager_CreateEmitterWithCallback(particleSystem.Manager!, resourceID, callback);
        sEmitterCallbackParam = null;
        particleSystem.LastAddedEmitter = emitter;
        return emitter;
    }

    // OFFSET 0x02014710
    public static int ParticleSystem_GetActiveEmitterCount(ParticleSystem particleSystem)
    {
        return particleSystem.Manager!.ActiveEmitters.Count;
    }

    // OFFSET 0x02014718
    public static void ParticleSystem_DeleteAllEmitters(ParticleSystem particleSystem)
    {
        SplManagers.SPLManager_DeleteAllEmitters(particleSystem.Manager!);
    }

    // OFFSET 0x02014724
    public static void ParticleSystem_DeleteEmitter(ParticleSystem particleSystem, SplEmitter emitter)
    {
        SplManagers.SPLManager_DeleteEmitter(particleSystem.Manager!, emitter);
    }

    // OFFSET 0x02014730
    public static byte[]? ParticleSystem_GetHeapStart(ParticleSystem particleSystem)
    {
        return particleSystem.HeapStart;
    }

    // OFFSET 0x02014734
    public static void ParticleSystem_GetCameraUp(ParticleSystem particleSystem, out VecFx32 up)
    {
        up = particleSystem.CameraUp;
    }

    // OFFSET 0x02014744
    public static void ParticleSystem_SetCameraUp(ParticleSystem particleSystem, in VecFx32 up)
    {
        particleSystem.CameraUp = up;
        Camera.Camera_SetUp(up, particleSystem.Camera!);
    }

    // OFFSET 0x02014764
    public static object? ParticleSystem_GetEmitterCallbackParam()
    {
        return sEmitterCallbackParam;
    }

    // OFFSET 0x02014770
    public static void ParticleSystem_GetDefaultCameraUp(out VecFx32 up)
    {
        up = DefaultCameraUp;
    }

    // OFFSET 0x02014784
    public static Camera? ParticleSystem_GetCamera(ParticleSystem particleSystem)
    {
        return particleSystem.Camera;
    }

    // OFFSET 0x02014788
    public static void ParticleSystem_SetCameraProjection(ParticleSystem particleSystem, int projection)
    {
        particleSystem.CameraProjection = (byte)projection;
    }

    // OFFSET 0x02014790
    public static byte ParticleSystem_GetCameraProjection(ParticleSystem particleSystem)
    {
        return particleSystem.CameraProjection;
    }

    // OFFSET 0x02014798
    public static void ParticleSystem_GetEmitterAxis(SplEmitter emitter, out VecFx16 axis)
    {
        axis = emitter.Axis;
    }

    // OFFSET 0x020147B0
    public static void ParticleSystem_SetEmitterEmissionCount(SplEmitter emitter, int count)
    {
        emitter.Resource!.Header.EmissionCount = count;
    }

    // OFFSET 0x020147B8
    private static object? ParticleSystem_FindEmitterBehavior(SplEmitter emitter, int behaviorType)
    {
        SplResource resource = emitter.Resource!;
        int count = resource.BehaviorCount;
        if (count == 0)
        {
            return null;
        }
        for (int i = 0; i < count; i++)
        {
            SplBehavior? behavior = resource.Behaviors![i];
            if (behavior == null)
            {
                continue;
            }
            switch (behaviorType)
            {
                case 0:
                    if (behavior.ApplyFunc == SplBehaviorType.Gravity)
                    {
                        return behavior.Object;
                    }
                    break;
                case 1:
                    if (behavior.ApplyFunc == SplBehaviorType.Random)
                    {
                        return behavior.Object;
                    }
                    break;
                case 2:
                    if (behavior.ApplyFunc == SplBehaviorType.Magnet)
                    {
                        return behavior.Object;
                    }
                    break;
                case 3:
                    if (behavior.ApplyFunc == SplBehaviorType.Spin)
                    {
                        return behavior.Object;
                    }
                    break;
                case 4:
                    if (behavior.ApplyFunc == SplBehaviorType.CollisionPlane)
                    {
                        return behavior.Object;
                    }
                    break;
                case 5:
                    if (behavior.ApplyFunc == SplBehaviorType.Convergence)
                    {
                        return behavior.Object;
                    }
                    break;
                default:
                    return null;
            }
        }
        return null;
    }

    // OFFSET 0x02014874
    public static void ParticleSystem_SetEmitterGravityMagnitude(SplEmitter emitter, in VecFx16 magnitude)
    {
        SplGravityBehavior? gravity = (SplGravityBehavior?)ParticleSystem_FindEmitterBehavior(emitter, 0);
        if (gravity != null)
        {
            gravity.Magnitude = magnitude;
        }
    }

    // OFFSET 0x02014890
    public static void ParticleSystem_SetEmitterMagnetTarget(SplEmitter emitter, in VecFx32 target)
    {
        SplMagnetBehavior? magnet = (SplMagnetBehavior?)ParticleSystem_FindEmitterBehavior(emitter, 2);
        if (magnet != null)
        {
            magnet.Target = target;
        }
    }

    // OFFSET 0x020148A8
    public static void ParticleSystem_GetEmitterMagnetTarget(SplEmitter emitter, out VecFx32 target)
    {
        SplMagnetBehavior? magnet = (SplMagnetBehavior?)ParticleSystem_FindEmitterBehavior(emitter, 2);
        if (magnet == null)
        {
            target = default;
            return;
        }
        target = magnet.Target;
    }

    // OFFSET 0x020148DC
    public static void ParticleSystem_SetEmitterMagnetForce(SplEmitter emitter, short force)
    {
        SplMagnetBehavior? magnet = (SplMagnetBehavior?)ParticleSystem_FindEmitterBehavior(emitter, 2);
        if (magnet != null)
        {
            magnet.Force = force;
        }
    }

    // OFFSET 0x020148F4
    public static void ParticleSystem_GetEmitterMagnetForce(SplEmitter emitter, out short force)
    {
        SplMagnetBehavior? magnet = (SplMagnetBehavior?)ParticleSystem_FindEmitterBehavior(emitter, 2);
        if (magnet == null)
        {
            force = 0;
            return;
        }
        force = magnet.Force;
    }

    // OFFSET 0x02014910
    public static void ParticleSystem_SetEmitterSpinAngle(SplEmitter emitter, ushort angle)
    {
        SplSpinBehavior? spin = (SplSpinBehavior?)ParticleSystem_FindEmitterBehavior(emitter, 3);
        if (spin != null)
        {
            spin.Angle = angle;
        }
    }

    // OFFSET 0x02014924
    public static void ParticleSystem_GetEmitterSpinAngle(SplEmitter emitter, out ushort angle)
    {
        SplSpinBehavior? spin = (SplSpinBehavior?)ParticleSystem_FindEmitterBehavior(emitter, 3);
        if (spin == null)
        {
            angle = 0;
            return;
        }
        angle = spin.Angle;
    }

    // OFFSET 0x02014940
    public static void ParticleSystem_SetEmitterSpinAxis(SplEmitter emitter, ushort axis)
    {
        SplSpinBehavior? spin = (SplSpinBehavior?)ParticleSystem_FindEmitterBehavior(emitter, 3);
        if (spin != null)
        {
            spin.Axis = axis;
        }
    }

    // OFFSET 0x02014954
    public static void ParticleSystem_GetEmitterSpinAxis(SplEmitter emitter, out ushort axis)
    {
        SplSpinBehavior? spin = (SplSpinBehavior?)ParticleSystem_FindEmitterBehavior(emitter, 3);
        if (spin == null)
        {
            axis = 0;
            return;
        }
        axis = spin.Axis;
    }

    // OFFSET 0x02014970
    public static void ParticleSystem_SetEmitterConvergenceTarget(SplEmitter emitter, in VecFx32 target)
    {
        SplConvergenceBehavior? convergence = (SplConvergenceBehavior?)ParticleSystem_FindEmitterBehavior(emitter, 5);
        if (convergence != null)
        {
            convergence.Target = target;
        }
    }

    // OFFSET 0x02014988
    public static void ParticleSystem_GetEmitterConvergenceTarget(SplEmitter emitter, out VecFx32 target)
    {
        SplConvergenceBehavior? convergence = (SplConvergenceBehavior?)ParticleSystem_FindEmitterBehavior(emitter, 5);
        if (convergence == null)
        {
            target = default;
            return;
        }
        target = convergence.Target;
    }

    // OFFSET 0x020149BC
    public static void ParticleSystem_SetEmitterConvergenceForce(SplEmitter emitter, short force)
    {
        SplConvergenceBehavior? convergence = (SplConvergenceBehavior?)ParticleSystem_FindEmitterBehavior(emitter, 5);
        if (convergence != null)
        {
            convergence.Force = force;
        }
    }

    // OFFSET 0x020149D4
    public static void ParticleSystem_GetEmitterConvergenceForce(SplEmitter emitter, out short force)
    {
        SplConvergenceBehavior? convergence = (SplConvergenceBehavior?)ParticleSystem_FindEmitterBehavior(emitter, 5);
        if (convergence == null)
        {
            force = 0;
            return;
        }
        force = convergence.Force;
    }
}
