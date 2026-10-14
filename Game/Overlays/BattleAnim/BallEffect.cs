namespace PlatinumSharp.Game;

public sealed class BallEffect
{
    public const int MaxParticleSystems = 9;

    public int HeapID;
    public int Type;
    public int BallOverride;
    public Pokemon? Mon;
    public int Count;
    public readonly ParticleSystem?[] ParticleSystems = new ParticleSystem?[MaxParticleSystems];
    public readonly object?[] Seals = new object?[MaxParticleSystems];
    public readonly int[] Started = new int[MaxParticleSystems];
    public SysTask? Task;
    public int Active;
    public NARC? Narc;
    public int State;
    public int HasCapsule;
    public int BallID;
    public int CapsuleID;
}

public static class BallEffects
{
    private const int MonDataBallCapsuleID = 0xA2;
    private const int MonDataPokeball = 0x9B;
    private const int MaxCapsuleID = 0xD;
    private const int NarcBallParticles = 0x63;
    private const uint BallParticleTable = 0x0223AD70;
    private const int BallParticleEntrySize = 8;
    private const uint ParticlePositionTable = 0x0223A260;
    private const int ParticlePositionRowSize = 0x120;
    private const int VecSize = 0xC;
    private const uint TaskPriority = 1000;
    private const int StateLoaded = 0xFF;
    private const int PositionScale = 0xAC;
    private const int ContestTypePlayer = 6;
    private const int ContestTypeEnemy = 7;

    // OFFSET 0x022353CC
    public static void BattleParticle_GetPosition(int battlerType, out VecFx32 position, int isContest, int cameraProjection, int variant)
    {
        if (isContest == 1)
        {
            switch (battlerType)
            {
                case 0:
                    battlerType = ContestTypePlayer;
                    break;
                case 1:
                    battlerType = ContestTypeEnemy;
                    break;
                default:
                    ErrorHandling.GF_AssertFail();
                    break;
            }
        }
        uint address = (uint)(ParticlePositionTable + battlerType * ParticlePositionRowSize + (variant * 2 + cameraProjection) * VecSize);
        byte[] raw = RomData.Overlay(12, address, VecSize);
        position.X = (int)Nitro.Bin.U32(raw, 0);
        position.Y = (int)Nitro.Bin.U32(raw, 4);
        position.Z = (int)Nitro.Bin.U32(raw, 8);
    }

    // OFFSET 0x02235448
    public static void BattleParticle_GetDefaultPosition(int battlerType, out VecFx32 position, int isContest, int cameraProjection)
    {
        BattleParticle_GetPosition(battlerType, out position, isContest, cameraProjection, 0);
    }

    // OFFSET 0x02235758
    public static void BattleParticle_GetBattlerPosition(int battlerType, out VecFx32 position, int isContest, int cameraProjection)
    {
        BattleParticle_GetDefaultPosition(battlerType, out position, isContest, cameraProjection);
    }

    // OFFSET 0x02235F64
    public static int Ball_GetParticleMember(int ballID)
    {
        int index = BallRotations.Ball_GetResourceIndex(ballID);
        return (int)Nitro.Bin.U32(RomData.Overlay(12, (uint)(BallParticleTable + index * BallParticleEntrySize), 4), 0);
    }

    // OFFSET 0x02235F78
    public static int Ball_GetEmitterCount(int ballID)
    {
        int index = BallRotations.Ball_GetResourceIndex(ballID);
        return (int)Nitro.Bin.U32(RomData.Overlay(12, (uint)(BallParticleTable + index * BallParticleEntrySize + 4), 4), 0);
    }

    // OFFSET 0x02235FE0
    private static BallEffect BallEffect_Alloc(int heapID)
    {
        BallEffect effect = new();
        effect.HeapID = heapID;
        effect.Active = 0;
        return effect;
    }

    // OFFSET 0x02236004
    public static BallEffect BallEffect_New(int heapID, int battlerType, int ballOverride, Pokemon mon)
    {
        BallEffect effect = BallEffect_Alloc(heapID);
        BallEffect_Init(effect, battlerType, ballOverride, mon);
        effect.Count = 0;
        for (int i = 0; i < BallEffect.MaxParticleSystems; i++)
        {
            effect.ParticleSystems[i] = null;
            effect.Seals[i] = null;
            effect.Started[i] = 0;
        }
        return effect;
    }

    // OFFSET 0x02236030
    private static void BallEffect_Init(BallEffect effect, int battlerType, int ballOverride, Pokemon mon)
    {
        effect.Type = battlerType;
        effect.BallOverride = ballOverride;
        effect.Mon = mon;
        effect.CapsuleID = (int)Pokemon.Pokemon_GetValue(mon, MonDataBallCapsuleID, null);
        if (effect.CapsuleID >= MaxCapsuleID)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (effect.CapsuleID == 0)
        {
            effect.HasCapsule = 0;
            if (effect.BallOverride != 0)
            {
                effect.BallID = effect.BallOverride;
            }
            else
            {
                effect.BallID = (int)Pokemon.Pokemon_GetValue(mon, MonDataPokeball, null);
            }
            return;
        }
        effect.HasCapsule = 1;
    }

    // OFFSET 0x02236320
    public static void BallEffect_Load(BallEffect effect)
    {
        if (effect.HasCapsule == 0)
        {
            int member = Ball_GetParticleMember(effect.BallID);
            effect.Count = 1;
            effect.ParticleSystems[0] = BattleParticlePlayers.BattleParticleUtil_CreateFromNarc(effect.HeapID, NarcBallParticles, member, 0);
            effect.State = StateLoaded;
            return;
        }
        throw Unported.Partial(0x02236320);
    }

    // OFFSET 0x02236374
    public static int BallEffect_IsLoaded(BallEffect effect)
    {
        return effect.State == StateLoaded ? 1 : 0;
    }

    // OFFSET 0x02236384
    private static void BallEffect_StartWithCallback(BallEffect effect, SplEmitterCallback callback)
    {
        effect.Active = 1;
        BallRotations.BattleAnim_SetDefaultAlphaBlending();
        BallEffect_CreateEmitters(effect, callback);
        effect.Task = SysTask.SysTask_Start(BallEffect_UpdateTask, effect, TaskPriority);
    }

    // OFFSET 0x022363B4
    public static void BallEffect_Start(BallEffect effect)
    {
        BallEffect_StartWithCallback(effect, BallEffect_EmitterCallback);
    }

    // OFFSET 0x022363C4
    public static int BallEffect_IsActive(BallEffect effect)
    {
        return effect.Active;
    }

    // OFFSET 0x022363CC
    private static void BallEffect_UpdateTask(SysTask task, object? param)
    {
        BallEffect effect = (BallEffect)param!;
        int busy = 0;
        if (effect.Count > BallEffect.MaxParticleSystems)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < effect.Count; i++)
        {
            if (effect.ParticleSystems[i] == null)
            {
                continue;
            }
            if (ParticleSystem.ParticleSystem_GetActiveEmitterCount(effect.ParticleSystems[i]!) == 0 && effect.Started[i] != 0)
            {
                BattleParticlePlayers.BattleParticleUtil_Free(effect.ParticleSystems[i]!);
                effect.ParticleSystems[i] = null;
                effect.Started[i] = 0;
            }
            else
            {
                busy = 1;
            }
        }
        if (busy == 0)
        {
            effect.Active = 0;
            SysTask.SysTask_Done(task);
        }
    }

    // OFFSET 0x02236428
    public static void BallEffect_Free(BallEffect effect)
    {
    }

    // OFFSET 0x0223646C
    private static void BallEffect_CreateEmitters(BallEffect effect, SplEmitterCallback callback)
    {
        if (effect.HasCapsule == 0)
        {
            int count = Ball_GetEmitterCount(effect.BallID);
            for (int i = 0; i < count; i++)
            {
                ParticleSystem.ParticleSystem_CreateEmitterWithCallback(effect.ParticleSystems[0]!, i, callback, effect);
            }
            effect.Started[0] = 1;
            ParticleSystem.ParticleSystem_SetCameraProjection(effect.ParticleSystems[0]!, 1);
            return;
        }
        throw Unported.Partial(0x0223646C);
    }

    // OFFSET 0x02236520
    private static void BallEffect_GetPosition(int type, out VecFx32 position)
    {
        BattleParticle_GetBattlerPosition(type, out position, 0, 1);
        int x;
        int y;
        switch (type)
        {
            case 0:
                x = 0;
                y = -16;
                break;
            case 1:
                x = -15;
                y = -25;
                break;
            case 2:
                x = -8;
                y = -12;
                break;
            case 3:
                x = -14;
                y = -25;
                break;
            case 4:
                x = -8;
                y = -12;
                break;
            default:
                x = -16;
                y = -29;
                break;
        }
        x += 10;
        y += 0x20;
        position.X -= x * PositionScale;
        position.Y -= y * PositionScale;
    }

    // OFFSET 0x02236598
    private static void BallEffect_EmitterCallback(SplEmitter emitter)
    {
        BallEffect effect = (BallEffect)ParticleSystem.ParticleSystem_GetEmitterCallbackParam()!;
        BallEffect_GetPosition(effect.Type, out VecFx32 position);
        VecFx32 basePos = emitter.Resource!.Header.EmitterBasePos;
        emitter.Position.X = position.X + basePos.X;
        emitter.Position.Y = position.Y + basePos.Y;
        emitter.Position.Z = position.Z + basePos.Z;
    }
}
