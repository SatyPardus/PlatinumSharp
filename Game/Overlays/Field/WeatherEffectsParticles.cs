namespace PlatinumSharp.Game;

public sealed class WeatherParticleWork
{
    public const int Size = 0xDC;

    public readonly WeatherSpawner Spawner = new();
    public readonly WeatherFogTable FogTable = new();
    public readonly WeatherFogFade FogFade = new();
    public int Delay;
    public readonly WeatherCounter Cycle = new();
    public readonly WeatherCounter Scroll = new();
    public readonly WeatherCounter Wind = new();
    public readonly WeatherCounter ScrollTimer = new();
    public readonly WeatherCounter Ending = new();
}

public static partial class WeatherEffects
{
    private const uint HeavySnowPeriodTable = 0x021F8CCC;
    private const uint HeavySnowSpeedTable = 0x021F8CBC;
    private const uint SandFallSpeedTable = 0x021F8D70;
    private const uint SandWindTable = 0x021F8D30;
    private const uint SandstormWindTable = 0x021F8D10;
    private const uint BlizzardWindTable = 0x021F8CDC;
    private const uint BlizzardFallTable = 0x021F8CEC;
    private const uint HeavyRainSpeedTable = 0x021F8CFC;
    private const int SeqRain = 0x639;
    private const int SeqHeavyRain = 0x63B;
    private const int WeatherLightning = 0x11;
    private const int SpawnerDone = 3;
    private const int OffscreenTopMax = -0x11C;
    private const int OffscreenTopMin = -0x128;
    private const int OffscreenBottomMin = 0xD4;
    private const int OffscreenBottomMax = 0xE8;

    private static int[] OverlayInts(uint address, int count)
    {
        uint[] values = RomData.OverlayU32(Overlay, address, count);
        int[] result = new int[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = (int)values[i];
        }
        return result;
    }

    private static FogManager EffectFog(WeatherEffect effect)
    {
        return effect.Manager!.FieldSystem!.FogManager!;
    }

    private static bool EffectHasNoParticles(WeatherEffect effect)
    {
        return effect.Sentinel.Next == effect.Sentinel;
    }

    private static bool ParticleFadeInStep(WeatherEffect effect, WeatherParticleWork work)
    {
        int spawnerState = WeatherSpawner_Update(work.Spawner);
        if (work.Delay > 0)
        {
            work.Delay--;
            return false;
        }
        return WeatherFog_UpdateFadeIn(work.FogFade, work.FogTable, effect.FogMode) == 1 && spawnerState == SpawnerDone;
    }

    private static bool ParticleFadeOutStep(WeatherEffect effect, WeatherParticleWork work)
    {
        int spawnerState = WeatherSpawner_Update(work.Spawner);
        if (work.Delay > 0)
        {
            work.Delay--;
            return false;
        }
        int fogDone = 1;
        if (effect.FogMode != 0)
        {
            fogDone = WeatherFogTable_Update(work.FogTable);
        }
        return fogDone == 1 && spawnerState == SpawnerDone;
    }

    private static bool ParticleSpawnTimer(WeatherSpawner spawner)
    {
        short timer = spawner.Timer;
        spawner.Timer = (short)(timer - 1);
        return timer <= 0;
    }

    private static void ParticleStartFog(WeatherEffect effect, WeatherFogTable fogTable, int slope, int offset, ushort color)
    {
        if (effect.FogMode != 0)
        {
            fogTable.FogManager = EffectFog(effect);
            WeatherFog_Apply(fogTable.FogManager, slope, offset, color);
            WeatherFogTable_SetFull(fogTable);
        }
    }

    private static void ParticleEnd(WeatherEffect effect, WeatherFogTable fogTable)
    {
        if (effect.FogMode != 0)
        {
            FogManager.FogManager_ApplyParameters(fogTable.FogManager!, FogManager.ParameterEnabled, 0, 0, 0, 0);
        }
        WeatherEffectTableEntry_Finish(effect.Entry!);
    }

    private static bool EffectIsActive(WeatherEffect effect)
    {
        return effect.State != EffectStateEnd && effect.State != EffectStateInit;
    }

    private static bool IsOffscreenY(int y)
    {
        return (y < OffscreenTopMax && y > OffscreenTopMin) || (y > OffscreenBottomMin && y < OffscreenBottomMax);
    }

    // OFFSET 0x021D7658
    private static void WeatherEffect_RainMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherParticleWork work = (WeatherParticleWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherSpawner_Init(work.Spawner, effect, 1, 8, 4, 0, -1, 1, 1, WeatherEffect_RainSpawn);
                WeatherFog_StartFadeIn(work.FogFade, work.FogTable, EffectFog(effect), 3, 0x726F, 0x6B5A, 1, effect.FogMode);
                work.Delay = 0;
                WeatherEffect_PlaySound(effect, SeqRain);
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (ParticleFadeInStep(effect, work))
                {
                    effect.State = EffectStateMain;
                }
                break;
            case EffectStateStart:
                WeatherSpawner_Init(work.Spawner, effect, 4, 0, 4, 0, -1, 1, 1, WeatherEffect_RainSpawn);
                ParticleStartFog(effect, work.FogTable, 3, 0x726F, 0x6B5A);
                WeatherEffect_Prefill(effect, WeatherEffect_RainSpawn, 0x14, 0xA, 1, WeatherParticle_RainMove);
                WeatherEffect_PlaySound(effect, SeqRain);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (ParticleSpawnTimer(work.Spawner))
                {
                    WeatherEffect_RainSpawn(effect, work.Spawner.Count);
                    work.Spawner.Timer = work.Spawner.Interval;
                }
                if (effect.Request == RequestFadeOut)
                {
                    WeatherSpawner_SetTargets(work.Spawner, 0, 8, 1, -1);
                    if (effect.FogMode != 0)
                    {
                        WeatherFogTable_Start(work.FogTable, 1, 0);
                    }
                    work.Delay = 0;
                    effect.State = EffectStateFadeOut;
                    WeatherEffect_StopSound(effect);
                }
                break;
            case EffectStateFadeOut:
                if (ParticleFadeOutStep(effect, work) && EffectHasNoParticles(effect))
                {
                    effect.State = EffectStateEnd;
                }
                break;
            case EffectStateEnd:
                ParticleEnd(effect, work.FogTable);
                break;
        }
        if (EffectIsActive(effect))
        {
            WeatherParticleList_ForEach(effect.Sentinel, WeatherParticle_RainMove);
            WeatherEffect_ScrollParticles(effect, out _, out _);
            WeatherEffect_AfterUpdate(effect);
        }
    }

    // OFFSET 0x021D78A4
    private static void WeatherEffect_RainSpawn(WeatherEffect effect, int count)
    {
        for (int i = 0; i < count; i++)
        {
            WeatherParticle? particle = WeatherEffect_AddParticle(effect, 0x20);
            if (particle == null)
            {
                break;
            }
            int[] work = particle.Work!;
            uint random = MathUtil.MTRNG_Next();
            work[0] = 0;
            int type = (int)(random % 3);
            Sprite.Sprite_SetAnimFrame(particle.Sprite!, (ushort)type);
            int variation = (int)(random % 0x14);
            work[2] = variation + (type + 1) * 10;
            if (type == 2)
            {
                work[2] += 10;
            }
            work[4] = variation / -5 + -5 * (type + 1);
            if (type == 2)
            {
                work[4] -= 5;
            }
            work[3] = 0;
            work[1] = type + 1;
            VecFx32 position = new()
            {
                X = ((int)(random % 0x10E) + type * 0xF) << Fx32.Shift,
                Y = -0x60000,
                Z = 0,
            };
            WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
        }
    }

    // OFFSET 0x021D7960
    private static void WeatherParticle_RainMove(WeatherParticle particle)
    {
        int[] work = particle.Work!;
        VecFx32 position = WeatherParticle_GetPosition(particle);
        switch (work[3])
        {
            case 0:
                position.X += work[4] << 13;
                position.Y += work[2] << 13;
                work[0] += 2;
                if (work[0] > work[1])
                {
                    if (MathUtil.MTRNG_Next() % 10 < 7)
                    {
                        work[3] = 2;
                    }
                    else
                    {
                        work[3] = 1;
                        work[0] = 4;
                        Sprite.Sprite_SetAnimFrame(particle.Sprite!, 3);
                    }
                }
                WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
                break;
            case 1:
                if (work[0]-- <= 0)
                {
                    work[3] = 2;
                }
                break;
            case 2:
                WeatherParticle_Remove(particle);
                break;
        }
    }

    // OFFSET 0x021D79F0
    private static void WeatherEffect_SnowMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherParticleWork work = (WeatherParticleWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherSpawner_Init(work.Spawner, effect, 1, 0x18, 1, 0xE, -5, 1, 0, WeatherEffect_SnowSpawn);
                work.Cycle.Value = 0;
                WeatherFog_StartFadeIn(work.FogFade, work.FogTable, EffectFog(effect), 3, 0x726F, 0x6B5A, 1, effect.FogMode);
                work.Delay = 0x10;
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (ParticleFadeInStep(effect, work))
                {
                    effect.State = EffectStateMain;
                }
                break;
            case EffectStateStart:
                WeatherSpawner_Init(work.Spawner, effect, 1, 0xE, 1, 0xE, -5, 1, 0, WeatherEffect_SnowSpawn);
                work.Cycle.Value = 0;
                work.Scroll.Value = 0;
                ParticleStartFog(effect, work.FogTable, 3, 0x726F, 0x6B5A);
                WeatherEffect_Prefill(effect, WeatherEffect_SnowSpawn, 0x14, 2, 0x18, WeatherParticle_SnowMove);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (ParticleSpawnTimer(work.Spawner))
                {
                    WeatherEffect_SnowSpawn(effect, work.Spawner.Count);
                    work.Spawner.Timer = work.Spawner.Interval;
                }
                if (effect.Request == RequestFadeOut)
                {
                    WeatherSpawner_SetTargets(work.Spawner, 0, 0x18, 5, -1);
                    if (effect.FogMode != 0)
                    {
                        WeatherFogTable_Start(work.FogTable, 2, 0);
                    }
                    work.Delay = 0x20;
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                if (ParticleFadeOutStep(effect, work))
                {
                    work.Scroll.Value = 1;
                    if (EffectHasNoParticles(effect))
                    {
                        effect.State = EffectStateEnd;
                    }
                }
                break;
            case EffectStateEnd:
                ParticleEnd(effect, work.FogTable);
                break;
        }
        if (EffectIsActive(effect))
        {
            WeatherParticleList_ForEach(effect.Sentinel, WeatherParticle_SnowMove);
            WeatherEffect_ScrollParticles(effect, out _, out _);
            WeatherEffect_AfterUpdate(effect);
        }
    }

    // OFFSET 0x021D7C40
    private static void WeatherEffect_SnowSpawn(WeatherEffect effect, int count)
    {
        WeatherParticleWork effectWork = (WeatherParticleWork)effect.Work!;
        if (effectWork.Cycle.Value == 1)
        {
            count <<= 1;
        }
        for (int i = 0; i < count; i++)
        {
            WeatherParticle? particle = WeatherEffect_AddParticle(effect, 0x28);
            if (particle == null)
            {
                break;
            }
            int[] work = particle.Work!;
            Sprite.Sprite_SetAnimFrame(particle.Sprite!, (ushort)(MathUtil.MTRNG_Next() & 3));
            work[4] = 10;
            work[5] = 0;
            work[6] = 0;
            work[8] = 0;
            int random = (int)MathUtil.MTRNG_Next();
            work[9] = random % 0x3C + 4;
            if (random % 2 == 0)
            {
                work[1] = 1;
            }
            else
            {
                work[1] = -1;
            }
            work[2] = (int)(MathUtil.MTRNG_Next() & 1) + 4;
            work[3] = (int)(MathUtil.MTRNG_Next() & 1) + 1;
            particle.WorkRefs[0] = effectWork.Cycle;
            particle.WorkRefs[7] = effectWork.Scroll;
            VecFx32 position = default;
            position.X = ((int)(MathUtil.MTRNG_Next() % 0x19E) - 0x20) << Fx32.Shift;
            if (effectWork.Cycle.Value == 1 && i >= count / 2)
            {
                position.Y = (-0x28 - (int)(MathUtil.MTRNG_Next() % 0x14)) << Fx32.Shift;
            }
            else
            {
                position.Y = (-8 - (int)(MathUtil.MTRNG_Next() % 0x14)) << Fx32.Shift;
            }
            WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
        }
    }

    // OFFSET 0x021D7D54
    private static void WeatherParticle_SnowDrift(WeatherParticle particle, int[] work)
    {
        VecFx32 position = WeatherParticle_GetPosition(particle);
        if ((ushort)work[5] >= work[2])
        {
            position.X += work[1] << Fx32.Shift;
            work[4]++;
            work[5] = (int)((uint)work[5] & 0xFFFF0000);
            if (work[4] < 10)
            {
                work[2]--;
            }
            else
            {
                work[2]++;
            }
            if (work[4] >= 0x14)
            {
                work[4] = 0;
                work[1] = -work[1];
            }
        }
        if ((work[5] >> 16) >= work[3])
        {
            position.Y += Fx32.One;
            work[5] = (ushort)work[5];
        }
        WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
        int y = position.Y >> Fx32.Shift;
        work[6] = (work[6] + 1) % 100;
        work[5] = work[5] + 1 + 0x10000;
        if (IsOffscreenY(y))
        {
            particle.WorkRefs[0]!.Value = 1;
            WeatherParticle_Remove(particle);
        }
    }

    // OFFSET 0x021D7E20
    private static void WeatherParticle_SnowMove(WeatherParticle particle)
    {
        int[] work = particle.Work!;
        if (work[8] == 0)
        {
            if (particle.WorkRefs[7]!.Value == 1)
            {
                work[8] = 1;
            }
        }
        else if (work[8] == 1)
        {
            work[9]--;
            if (work[9] < 0)
            {
                WeatherParticle_Remove(particle);
                return;
            }
        }
        WeatherParticle_SnowDrift(particle, work);
    }

    // OFFSET 0x021D7E54
    private static void WeatherEffect_HeavySnowMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherParticleWork work = (WeatherParticleWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherSpawner_Init(work.Spawner, effect, 1, 0x1E, 6, 3, -5, 2, 1, WeatherEffect_HeavySnowSpawn);
                WeatherFog_StartFadeIn(work.FogFade, work.FogTable, EffectFog(effect), 3, 0x6D6F, 0x6318, 2, effect.FogMode);
                work.Delay = 8;
                work.Cycle.Value = 0;
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (ParticleFadeInStep(effect, work))
                {
                    effect.State = EffectStateMain;
                }
                break;
            case EffectStateStart:
                WeatherSpawner_Init(work.Spawner, effect, 6, 3, 6, 3, -5, 2, 1, WeatherEffect_HeavySnowSpawn);
                ParticleStartFog(effect, work.FogTable, 3, 0x6D6F, 0x6318);
                work.Cycle.Value = 0;
                WeatherEffect_Prefill(effect, WeatherEffect_HeavySnowSpawn, 0x14, 2, 3, WeatherParticle_HeavySnowMove);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (ParticleSpawnTimer(work.Spawner))
                {
                    WeatherEffect_HeavySnowSpawn(effect, work.Spawner.Count);
                    work.Spawner.Timer = work.Spawner.Interval;
                }
                if (effect.Request == RequestFadeOut)
                {
                    WeatherSpawner_SetTargets(work.Spawner, 0, 0x1E, 5, -3);
                    if (effect.FogMode != 0)
                    {
                        WeatherFogTable_Start(work.FogTable, 1, 0);
                    }
                    work.Delay = 0;
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                if (ParticleFadeOutStep(effect, work) && EffectHasNoParticles(effect))
                {
                    effect.State = EffectStateEnd;
                }
                break;
            case EffectStateEnd:
                ParticleEnd(effect, work.FogTable);
                break;
        }
        if (EffectIsActive(effect))
        {
            WeatherParticleList_ForEach(effect.Sentinel, WeatherParticle_HeavySnowMove);
            WeatherEffect_ScrollParticles(effect, out _, out _);
            WeatherEffect_AfterUpdate(effect);
        }
    }

    // OFFSET 0x021D8098
    private static void WeatherEffect_HeavySnowSpawn(WeatherEffect effect, int count)
    {
        int[] periods = OverlayInts(HeavySnowPeriodTable, 4);
        int[] speeds = OverlayInts(HeavySnowSpeedTable, 4);
        WeatherParticleWork effectWork = (WeatherParticleWork)effect.Work!;
        for (int i = 0; i < count; i++)
        {
            WeatherParticle? particle = WeatherEffect_AddParticle(effect, 0x20);
            if (particle == null)
            {
                break;
            }
            int[] work = particle.Work!;
            effectWork.Cycle.Value++;
            if (effectWork.Cycle.Value >= 0x320)
            {
                effectWork.Cycle.Value = 0;
            }
            int phase = effectWork.Cycle.Value / 0xC8;
            work[5] = periods[phase];
            work[0] = 0;
            work[1] = (int)(MathUtil.MTRNG_Next() % 0x2A) + 4;
            int frame = (work[1] - 4) / 0xF;
            Sprite.Sprite_SetAnimFrame(particle.Sprite!, (ushort)frame);
            work[4] = -(frame + 1);
            work[2] = (frame + 1) * speeds[phase];
            work[3] = 0;
            VecFx32 position = new()
            {
                X = ((1 - frame) * -0x14 + (int)(MathUtil.MTRNG_Next() % 0x1A4)) << Fx32.Shift,
                Y = -8 << Fx32.Shift,
                Z = 0,
            };
            WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
        }
    }

    // OFFSET 0x021D81BC
    private static void WeatherParticle_HeavySnowMove(WeatherParticle particle)
    {
        int[] work = particle.Work!;
        VecFx32 position = WeatherParticle_GetPosition(particle);
        switch (work[3])
        {
            case 0:
                position.X += work[4] << Fx32.Shift;
                position.Y += work[2] << Fx32.Shift;
                if (work[0]++ > work[1])
                {
                    work[3] = 1;
                }
                if (work[0] % work[5] == 0)
                {
                    work[4]--;
                    if (work[2] > 1)
                    {
                        work[2]--;
                    }
                }
                WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
                break;
            case 1:
                WeatherParticle_Remove(particle);
                break;
        }
    }

    // OFFSET 0x021D823C
    private static void WeatherEffect_HeavySnowBgMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherParticleWork work = (WeatherParticleWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherSpawner_Init(work.Spawner, effect, 1, 0x1E, 6, 3, -5, 2, 1, WeatherEffect_HeavySnowSpawn);
                WeatherFog_StartFadeIn(work.FogFade, work.FogTable, EffectFog(effect), 3, 0x6F6F, 0x6318, 2, effect.FogMode);
                work.Delay = 8;
                work.Cycle.Value = 0;
                work.Scroll.Value = 0;
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (ParticleFadeInStep(effect, work))
                {
                    effect.State = EffectStateMain;
                }
                break;
            case EffectStateStart:
                WeatherSpawner_Init(work.Spawner, effect, 6, 3, 6, 3, -5, 2, 1, WeatherEffect_HeavySnowSpawn);
                ParticleStartFog(effect, work.FogTable, 3, 0x6F6F, 0x6318);
                work.Cycle.Value = 0;
                work.Scroll.Value = 0;
                WeatherEffect_Prefill(effect, WeatherEffect_HeavySnowSpawn, 0x14, 2, 3, WeatherParticle_HeavySnowMove);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (ParticleSpawnTimer(work.Spawner))
                {
                    WeatherEffect_HeavySnowSpawn(effect, work.Spawner.Count);
                    work.Spawner.Timer = work.Spawner.Interval;
                }
                if (effect.Request == RequestFadeOut)
                {
                    WeatherSpawner_SetTargets(work.Spawner, 0, 0x1E, 5, -3);
                    if (effect.FogMode != 0)
                    {
                        WeatherFogTable_Start(work.FogTable, 1, 0);
                    }
                    work.Delay = 0;
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                if (ParticleFadeOutStep(effect, work) && EffectHasNoParticles(effect))
                {
                    effect.State = EffectStateEnd;
                }
                break;
            case EffectStateEnd:
                ParticleEnd(effect, work.FogTable);
                break;
        }
        if (EffectIsActive(effect))
        {
            WeatherParticleList_ForEach(effect.Sentinel, WeatherParticle_HeavySnowMove);
            WeatherEffect_ScrollParticles(effect, out _, out _);
            WeatherEffect_AfterUpdate(effect);
            work.Scroll.Value = (work.Scroll.Value + 6) % 0x100;
            SetBg2Offset(work.Scroll.Value, -work.Scroll.Value);
        }
    }

    // OFFSET 0x021D8948
    private static void WeatherEffect_SandMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherParticleWork work = (WeatherParticleWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherSpawner_Init(work.Spawner, effect, 1, 0xF, 8, 1, -2, 4, 2, WeatherEffect_SandSpawn);
                WeatherFog_StartFadeIn(work.FogFade, work.FogTable, EffectFog(effect), 3, 0x6EEF, 0x169A, 1, effect.FogMode);
                work.Delay = 0;
                work.Cycle.Value = 0;
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (ParticleFadeInStep(effect, work))
                {
                    effect.State = EffectStateMain;
                }
                break;
            case EffectStateStart:
                WeatherSpawner_Init(work.Spawner, effect, 8, 1, 8, 1, -2, 4, 2, WeatherEffect_SandSpawn);
                ParticleStartFog(effect, work.FogTable, 3, 0x6EEF, 0x169A);
                work.Cycle.Value = 0;
                WeatherEffect_Prefill(effect, WeatherEffect_SandSpawn, 0x18, 2, 2, WeatherParticle_SandMove);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (ParticleSpawnTimer(work.Spawner))
                {
                    WeatherEffect_SandSpawn(effect, work.Spawner.Count);
                    work.Spawner.Timer = work.Spawner.Interval;
                }
                if (effect.Request == RequestFadeOut)
                {
                    WeatherSpawner_SetTargets(work.Spawner, 0, 0xF, 2, -3);
                    if (effect.FogMode != 0)
                    {
                        WeatherFogTable_Start(work.FogTable, 1, 0);
                    }
                    work.Delay = 0x1F;
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                if (ParticleFadeOutStep(effect, work) && EffectHasNoParticles(effect))
                {
                    effect.State = EffectStateEnd;
                }
                break;
            case EffectStateEnd:
                ParticleEnd(effect, work.FogTable);
                break;
        }
        if (EffectIsActive(effect))
        {
            WeatherParticleList_ForEach(effect.Sentinel, WeatherParticle_SandMove);
            WeatherEffect_ScrollParticles(effect, out _, out _);
            WeatherEffect_AfterUpdate(effect);
        }
    }

    // OFFSET 0x021D8B88
    private static void WeatherEffect_SandSpawn(WeatherEffect effect, int count)
    {
        WeatherParticleWork effectWork = (WeatherParticleWork)effect.Work!;
        effectWork.Cycle.Value = (effectWork.Cycle.Value + 1) % 0x140;
        int phase = effectWork.Cycle.Value / 0x28;
        int[] fallSpeeds = OverlayInts(SandFallSpeedTable, 8);
        int[] winds = OverlayInts(SandWindTable, 8);
        for (int i = 0; i < count; i++)
        {
            WeatherParticle? particle = WeatherEffect_AddParticle(effect, 0x20);
            if (particle == null)
            {
                break;
            }
            int[] work = particle.Work!;
            work[0] = 0;
            int life = (int)(MathUtil.MTRNG_Next() % 0x14);
            work[1] = life + 0xF;
            int frame = 3 - life / 6;
            work[2] = (frame + 1) * fallSpeeds[phase];
            work[4] = (frame + 1) * winds[phase];
            work[3] = 0;
            work[5] = winds[phase];
            if (MathUtil.MTRNG_Next() % 0x3E8 == 0x309)
            {
                frame = 4;
                work[2] += work[2] / 2;
            }
            Sprite.Sprite_SetAnimFrame(particle.Sprite!, (ushort)frame);
            VecFx32 position = WeatherParticle_GetPosition(particle);
            position.X = ((int)(MathUtil.MTRNG_Next() % 0x18) + 0x106) << Fx32.Shift;
            position.Y = ((int)(MathUtil.MTRNG_Next() % 0xC0) - 0x40) << Fx32.Shift;
            WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
        }
    }

    // OFFSET 0x021D8C90
    private static void WeatherParticle_SandMove(WeatherParticle particle)
    {
        int[] work = particle.Work!;
        VecFx32 position = WeatherParticle_GetPosition(particle);
        switch (work[3])
        {
            case 0:
                position.X += work[4] << Fx32.Shift;
                position.Y += work[2] << Fx32.Shift;
                if (work[0] % 5 == 0)
                {
                    work[4] += work[5];
                }
                if (work[0]++ > work[1])
                {
                    work[3] = 1;
                }
                WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
                break;
            case 1:
                WeatherParticle_Remove(particle);
                break;
        }
    }

    // OFFSET 0x021D8D08
    private static void WeatherEffect_SandstormMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherParticleWork work = (WeatherParticleWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherSpawner_Init(work.Spawner, effect, 1, 0xF, 8, 1, -2, 4, 2, WeatherEffect_SandSpawn);
                WeatherFog_StartFadeIn(work.FogFade, work.FogTable, EffectFog(effect), 3, 0x6EEF, 0x169A, 1, effect.FogMode);
                work.Delay = 0;
                work.Cycle.Value = 0;
                work.Scroll.Value = 0;
                work.Wind.Value = 0;
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (ParticleFadeInStep(effect, work))
                {
                    effect.State = EffectStateMain;
                    GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                }
                break;
            case EffectStateStart:
                WeatherSpawner_Init(work.Spawner, effect, 8, 1, 8, 1, -2, 4, 2, WeatherEffect_SandSpawn);
                ParticleStartFog(effect, work.FogTable, 3, 0x6EEF, 0x169A);
                work.Cycle.Value = 0;
                work.Scroll.Value = 0;
                work.Wind.Value = 0;
                WeatherEffect_Prefill(effect, WeatherEffect_SandSpawn, 0x18, 2, 2, WeatherParticle_SandMove);
                effect.State = EffectStateMain;
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                break;
            case EffectStateMain:
                if (ParticleSpawnTimer(work.Spawner))
                {
                    work.Wind.Value = OverlayInts(SandstormWindTable, 8)[work.Cycle.Value / 0x28];
                    if (work.Wind.Value <= -6)
                    {
                        WeatherEffect_SandSpawn(effect, work.Spawner.Count << 1);
                    }
                    else
                    {
                        WeatherEffect_SandSpawn(effect, work.Spawner.Count);
                    }
                    work.Spawner.Timer = work.Spawner.Interval;
                }
                if (effect.Request == RequestFadeOut)
                {
                    WeatherSpawner_SetTargets(work.Spawner, 0, 0xF, 2, -3);
                    if (effect.FogMode != 0)
                    {
                        WeatherFogTable_Start(work.FogTable, 1, 0);
                    }
                    work.Delay = 0x1F;
                    effect.State = EffectStateFadeOut;
                    GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 0);
                }
                break;
            case EffectStateFadeOut:
                if (ParticleFadeOutStep(effect, work) && EffectHasNoParticles(effect))
                {
                    effect.State = EffectStateEnd;
                }
                break;
            case EffectStateEnd:
                ParticleEnd(effect, work.FogTable);
                break;
        }
        if (EffectIsActive(effect))
        {
            WeatherParticleList_ForEach(effect.Sentinel, WeatherParticle_SandMove);
            WeatherEffect_ScrollParticles(effect, out int scrollX, out int scrollY);
            WeatherEffect_AfterUpdate(effect);
            work.Scroll.Value = (work.Scroll.Value + 6) % 0x100;
            SetBg2Offset(work.Scroll.Value * 2 - scrollX, scrollY - work.Scroll.Value);
        }
    }

    // OFFSET 0x021D8FF8
    private static void WeatherEffect_BlizzardMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherParticleWork work = (WeatherParticleWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherSpawner_Init(work.Spawner, effect, 1, 0x1E, 10, 1, -4, 2, 3, WeatherEffect_BlizzardSpawn);
                WeatherFog_StartFadeIn(work.FogFade, work.FogTable, EffectFog(effect), 3, 0x6B6F, 0x6318, 1, effect.FogMode);
                work.Delay = 0x10;
                work.Cycle.Value = 0;
                work.Scroll.Value = 0;
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (ParticleFadeInStep(effect, work))
                {
                    effect.State = EffectStateMain;
                    GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                }
                break;
            case EffectStateStart:
                WeatherSpawner_Init(work.Spawner, effect, 10, 1, 10, 1, -4, 2, 3, WeatherEffect_BlizzardSpawn);
                ParticleStartFog(effect, work.FogTable, 3, 0x6B6F, 0x6318);
                work.Cycle.Value = 0;
                work.Scroll.Value = 0;
                WeatherEffect_Prefill(effect, WeatherEffect_BlizzardSpawn, 0x14, 2, 2, WeatherParticle_BlizzardMove);
                effect.State = EffectStateMain;
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                break;
            case EffectStateMain:
                if (ParticleSpawnTimer(work.Spawner))
                {
                    if (OverlayInts(BlizzardWindTable, 4)[work.Cycle.Value / 0x200] <= -8)
                    {
                        WeatherEffect_BlizzardSpawn(effect, work.Spawner.Count << 1);
                    }
                    else
                    {
                        WeatherEffect_BlizzardSpawn(effect, work.Spawner.Count);
                    }
                    work.Spawner.Timer = work.Spawner.Interval;
                }
                if (effect.Request == RequestFadeOut)
                {
                    WeatherSpawner_SetTargets(work.Spawner, 0, 0xC, 4, -4);
                    if (effect.FogMode != 0)
                    {
                        WeatherFogTable_Start(work.FogTable, 1, 0);
                    }
                    work.Delay = 0x14;
                    effect.State = EffectStateFadeOut;
                    GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 0);
                }
                break;
            case EffectStateFadeOut:
                if (ParticleFadeOutStep(effect, work) && EffectHasNoParticles(effect))
                {
                    effect.State = EffectStateEnd;
                }
                break;
            case EffectStateEnd:
                ParticleEnd(effect, work.FogTable);
                break;
        }
        if (EffectIsActive(effect))
        {
            WeatherParticleList_ForEach(effect.Sentinel, WeatherParticle_BlizzardMove);
            WeatherEffect_AfterUpdate(effect);
            work.Scroll.Value = (work.Scroll.Value + 0xC) % 0x100;
            SetBg2Offset(work.Scroll.Value * 2, -work.Scroll.Value);
        }
    }

    // OFFSET 0x021D92C4
    private static void WeatherEffect_BlizzardSpawn(WeatherEffect effect, int count)
    {
        WeatherParticleWork effectWork = (WeatherParticleWork)effect.Work!;
        effectWork.Cycle.Value++;
        if (effectWork.Cycle.Value >= 0x800)
        {
            effectWork.Cycle.Value = 0;
        }
        int phase = effectWork.Cycle.Value / 0x200;
        int[] winds = OverlayInts(BlizzardWindTable, 4);
        int[] falls = OverlayInts(BlizzardFallTable, 4);
        count <<= 2;
        for (int i = 0; i < count; i++)
        {
            WeatherParticle? particle = WeatherEffect_AddParticle(effect, 0x20);
            if (particle == null)
            {
                break;
            }
            int[] work = particle.Work!;
            work[0] = 0;
            work[1] = (int)(MathUtil.MTRNG_Next() % 6) + 0x12;
            int frame = (int)(MathUtil.MTRNG_Next() & 3);
            Sprite.Sprite_SetAnimFrame(particle.Sprite!, (ushort)frame);
            work[4] = (frame + 1) * winds[phase];
            work[2] = (frame + 1) * falls[phase];
            work[3] = 0;
            if (frame == 3)
            {
                work[4] += winds[phase];
                work[2] += falls[phase];
            }
            work[5] = winds[phase];
            VecFx32 position = default;
            position.X = ((int)(MathUtil.MTRNG_Next() % 0x18) + 0x100) << Fx32.Shift;
            position.Y = ((int)(MathUtil.MTRNG_Next() % 0xA8) - 0x20) << Fx32.Shift;
            position.Z = 0;
            WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
        }
    }

    // OFFSET 0x021D93DC
    private static void WeatherParticle_BlizzardMove(WeatherParticle particle)
    {
        int[] work = particle.Work!;
        VecFx32 position = WeatherParticle_GetPosition(particle);
        switch (work[3])
        {
            case 0:
                position.X += work[4] << Fx32.Shift;
                position.Y += work[2] << Fx32.Shift;
                if (work[0]++ > work[1])
                {
                    work[3] = 1;
                }
                if (work[0] % 4 == 0)
                {
                    work[4] += work[5];
                    if (work[2] > 1)
                    {
                        work[2]--;
                    }
                }
                WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
                break;
            case 1:
                WeatherParticle_Remove(particle);
                break;
        }
    }

    // OFFSET 0x021D9464
    private static void WeatherEffect_DiamondDustMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherParticleWork work = (WeatherParticleWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherSpawner_Init(work.Spawner, effect, 2, 0x10, 0x14, 2, -2, 4, 2, WeatherEffect_DiamondDustSpawn);
                WeatherFog_StartFadeIn(work.FogFade, work.FogTable, EffectFog(effect), 3, 0x716F, 0x6B5A, 1, effect.FogMode);
                work.Delay = 0;
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (ParticleFadeInStep(effect, work))
                {
                    effect.State = EffectStateMain;
                }
                break;
            case EffectStateStart:
                WeatherSpawner_Init(work.Spawner, effect, 0x14, 2, 0x14, 2, -2, 4, 2, WeatherEffect_DiamondDustSpawn);
                ParticleStartFog(effect, work.FogTable, 3, 0x716F, 0x6B5A);
                WeatherEffect_Prefill(effect, WeatherEffect_DiamondDustSpawn, 0x14, 0xA, 1, WeatherParticle_DiamondDustMove);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (ParticleSpawnTimer(work.Spawner))
                {
                    WeatherEffect_DiamondDustSpawn(effect, work.Spawner.Count);
                    work.Spawner.Timer = work.Spawner.Interval;
                }
                if (effect.Request == RequestFadeOut)
                {
                    WeatherSpawner_SetTargets(work.Spawner, 0, 0x10, 6, -10);
                    if (effect.FogMode != 0)
                    {
                        WeatherFogTable_Start(work.FogTable, 1, 0);
                    }
                    work.Delay = 0x14;
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                if (ParticleFadeInStep(effect, work) && EffectHasNoParticles(effect))
                {
                    effect.State = EffectStateEnd;
                }
                break;
            case EffectStateEnd:
                ParticleEnd(effect, work.FogTable);
                break;
        }
        if (EffectIsActive(effect))
        {
            WeatherParticleList_ForEach(effect.Sentinel, WeatherParticle_DiamondDustMove);
            WeatherEffect_ScrollParticles(effect, out _, out _);
            WeatherEffect_AfterUpdate(effect);
        }
    }

    // OFFSET 0x021D9690
    private static void WeatherEffect_DiamondDustSpawn(WeatherEffect effect, int count)
    {
        for (int i = 0; i < count; i++)
        {
            WeatherParticle? particle = WeatherEffect_AddParticle(effect, 0x20);
            if (particle == null)
            {
                break;
            }
            int[] work = particle.Work!;
            work[0] = 0;
            work[1] = (int)(MathUtil.MTRNG_Next() % 5) + 7;
            if ((MathUtil.MTRNG_Next() % 0x3E8) % 2 == 0)
            {
                work[2] = 1;
            }
            else
            {
                work[2] = -1;
            }
            work[3] = 1;
            work[4] = (int)(MathUtil.MTRNG_Next() % 6) + 3;
            work[5] = (int)(MathUtil.MTRNG_Next() % 5) + 4;
            int frame = (int)(MathUtil.MTRNG_Next() % 0x14);
            VecFx32 position = WeatherParticle_GetPosition(particle);
            position.X = ((int)(MathUtil.MTRNG_Next() % 0x180) - 0x40) << Fx32.Shift;
            position.Y = ((int)(MathUtil.MTRNG_Next() & 0xFF) - 8) << Fx32.Shift;
            position.Z = 0;
            WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
            int y = position.Y >> Fx32.Shift;
            int x = position.X >> Fx32.Shift;
            int top = 0x32 - x / 3;
            int range = 0xCE - x / 3;
            int bottom;
            if (range < 0)
            {
                bottom = top - (int)(MathUtil.MTRNG_Next() % (uint)(-range));
            }
            else
            {
                bottom = top + (int)(MathUtil.MTRNG_Next() % (uint)range);
            }
            if (top <= y && bottom >= y)
            {
                work[1] <<= 1;
            }
            else
            {
                frame = (int)(MathUtil.MTRNG_Next() & 3);
            }
            Sprite.Sprite_SetAnimFrame(particle.Sprite!, (ushort)frame);
        }
    }

    // OFFSET 0x021D97C0
    private static void WeatherParticle_DiamondDustMove(WeatherParticle particle)
    {
        int[] work = particle.Work!;
        WeatherParticle_GetPosition(particle);
        work[0]++;
        if (work[0] >= work[1])
        {
            WeatherParticle_Remove(particle);
        }
    }

    // OFFSET 0x021D9984
    private static void WeatherEffect_HeavyRainInitFadeIn(WeatherEffect effect, WeatherParticleWork work)
    {
        WeatherSpawner_Init(work.Spawner, effect, 1, 0xF, 10, 0, -3, 2, 1, WeatherEffect_HeavyRainSpawn);
        WeatherFog_StartFadeIn(work.FogFade, work.FogTable, EffectFog(effect), 3, 0x6F6F, 0x6B5A, 1, effect.FogMode);
        work.Delay = 0;
        work.Cycle.Value = 0;
        work.Scroll.Value = 0;
        WeatherEffect_PlaySound(effect, SeqHeavyRain);
    }

    // OFFSET 0x021D9A0C
    private static int WeatherEffect_HeavyRainUpdateFadeIn(WeatherEffect effect, WeatherParticleWork work)
    {
        return ParticleFadeInStep(effect, work) ? 1 : 0;
    }

    // OFFSET 0x021D9A58
    private static void WeatherEffect_HeavyRainInitStart(WeatherEffect effect, WeatherParticleWork work)
    {
        WeatherSpawner_Init(work.Spawner, effect, 10, 0, 10, 0, -3, 2, 1, WeatherEffect_HeavyRainSpawn);
        ParticleStartFog(effect, work.FogTable, 3, 0x6F6F, 0x6B5A);
        work.Cycle.Value = 0;
        work.Scroll.Value = 0;
        WeatherEffect_Prefill(effect, WeatherEffect_HeavyRainSpawn, 0x14, 5, 1, WeatherParticle_HeavyRainMove);
        WeatherEffect_PlaySound(effect, SeqHeavyRain);
    }

    // OFFSET 0x021D9AEC
    private static void WeatherEffect_HeavyRainUpdateMain(WeatherEffect effect, WeatherParticleWork work)
    {
        work.Scroll.Value = (work.Scroll.Value + 1) % 300;
        if (ParticleSpawnTimer(work.Spawner))
        {
            WeatherEffect_HeavyRainSpawn(effect, work.Spawner.Count);
            work.Spawner.Timer = work.Spawner.Interval;
        }
    }

    // OFFSET 0x021D9B28
    private static void WeatherEffect_HeavyRainBeginFadeOut(WeatherEffect effect, WeatherParticleWork work)
    {
        WeatherSpawner_SetTargets(work.Spawner, 0, 0xF, 3, -3);
        if (effect.FogMode != 0)
        {
            WeatherFogTable_Start(work.FogTable, 1, 0);
        }
        work.Delay = 0;
        WeatherEffect_StopSound(effect);
    }

    // OFFSET 0x021D9B68
    private static int WeatherEffect_HeavyRainUpdateFadeOut(WeatherEffect effect, WeatherParticleWork work)
    {
        if (ParticleFadeOutStep(effect, work) && EffectHasNoParticles(effect))
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021D9BC0
    private static void WeatherEffect_HeavyRainEnd(WeatherEffect effect, WeatherParticleWork work)
    {
        ParticleEnd(effect, work.FogTable);
    }

    // OFFSET 0x021D9BEC
    private static void WeatherEffect_HeavyRainUpdateParticles(WeatherEffect effect)
    {
        if (EffectIsActive(effect))
        {
            WeatherParticleList_ForEach(effect.Sentinel, WeatherParticle_HeavyRainMove);
            WeatherEffect_ScrollParticles(effect, out _, out _);
            WeatherEffect_AfterUpdate(effect);
        }
    }

    // OFFSET 0x021D9F0C
    private static void WeatherEffect_ThunderstormMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherParticleWork work = (WeatherParticleWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherEffect_HeavyRainInitFadeIn(effect, work);
                WeatherEffectManager_Command(effect.Manager!, CommandLoad, WeatherLightning);
                WeatherEffectManager_Command(effect.Manager!, CommandStart, WeatherLightning);
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (WeatherEffect_HeavyRainUpdateFadeIn(effect, work) != 0)
                {
                    effect.State = EffectStateMain;
                }
                break;
            case EffectStateStart:
                WeatherEffect_HeavyRainInitStart(effect, work);
                WeatherEffectManager_Command(effect.Manager!, CommandLoad, WeatherLightning);
                WeatherEffectManager_Command(effect.Manager!, CommandStart, WeatherLightning);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                WeatherEffect_HeavyRainUpdateMain(effect, work);
                if (effect.Request == RequestFadeOut)
                {
                    WeatherEffect_HeavyRainBeginFadeOut(effect, work);
                    WeatherEffectManager_Command(effect.Manager!, CommandFadeOut, WeatherLightning);
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                if (WeatherEffect_HeavyRainUpdateFadeOut(effect, work) != 0)
                {
                    effect.State = EffectStateEnd;
                }
                break;
            case EffectStateEnd:
                WeatherEffect_HeavyRainEnd(effect, work);
                WeatherEffectManager_Command(effect.Manager!, CommandUnload, WeatherLightning);
                break;
        }
        WeatherEffect_HeavyRainUpdateParticles(effect);
    }

    // OFFSET 0x021D9FF8
    private static void WeatherEffect_HeavyRainMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherParticleWork work = (WeatherParticleWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherEffect_HeavyRainInitFadeIn(effect, work);
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (WeatherEffect_HeavyRainUpdateFadeIn(effect, work) != 0)
                {
                    effect.State = EffectStateMain;
                }
                break;
            case EffectStateStart:
                WeatherEffect_HeavyRainInitStart(effect, work);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                WeatherEffect_HeavyRainUpdateMain(effect, work);
                if (effect.Request == RequestFadeOut)
                {
                    WeatherEffect_HeavyRainBeginFadeOut(effect, work);
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                if (WeatherEffect_HeavyRainUpdateFadeOut(effect, work) != 0)
                {
                    effect.State = EffectStateEnd;
                }
                break;
            case EffectStateEnd:
                WeatherEffect_HeavyRainEnd(effect, work);
                break;
        }
        WeatherEffect_HeavyRainUpdateParticles(effect);
    }

    // OFFSET 0x021DA0A8
    private static void WeatherEffect_HeavyRainSpawn(WeatherEffect effect, int count)
    {
        WeatherParticleWork effectWork = (WeatherParticleWork)effect.Work!;
        int[] speeds = OverlayInts(HeavyRainSpeedTable, 5);
        for (int i = 0; i < count; i++)
        {
            WeatherParticle? particle = WeatherEffect_AddParticle(effect, 0x20);
            if (particle == null)
            {
                break;
            }
            int[] work = particle.Work!;
            uint random = MathUtil.MTRNG_Next();
            work[0] = 0;
            int frame = (int)(random % 3);
            Sprite.Sprite_SetAnimFrame(particle.Sprite!, (ushort)frame);
            work[4] = -0x18 * (frame + 1);
            work[2] = 0x18 * (frame + 1);
            work[4] *= speeds[effectWork.Scroll.Value / 0x3C];
            work[2] *= speeds[effectWork.Scroll.Value / 0x3C];
            work[3] = 0;
            work[1] = (int)(random & 3);
            work[1] /= speeds[effectWork.Scroll.Value / 0x3C];
            VecFx32 position = default;
            position.X = (int)(random & 0x1FF) << Fx32.Shift;
            position.Y = ((int)(random % 0x30) - 0x50) << Fx32.Shift;
            position.Z = 0;
            WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
        }
    }

    // OFFSET 0x021DA1A8
    private static void WeatherParticle_HeavyRainMove(WeatherParticle particle)
    {
        int[] work = particle.Work!;
        VecFx32 position = WeatherParticle_GetPosition(particle);
        switch (work[3])
        {
            case 0:
                position.X += work[4] << 13;
                position.Y += work[2] << 13;
                work[0] += 2;
                if (work[0] > work[1])
                {
                    if (MathUtil.MTRNG_Next() % 10 < 5)
                    {
                        work[3] = 2;
                    }
                    else
                    {
                        work[3] = 1;
                        work[0] = 4;
                        Sprite.Sprite_SetAnimFrame(particle.Sprite!, 3);
                    }
                }
                WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
                break;
            case 1:
                if (work[0]-- <= 0)
                {
                    work[3] = 2;
                }
                break;
            case 2:
                WeatherParticle_Remove(particle);
                break;
        }
    }

    // OFFSET 0x021DA244
    private static void WeatherEffect_AshMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherParticleWork work = (WeatherParticleWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherSpawner_Init(work.Spawner, effect, 1, 0xC, 1, 6, -1, 4, 0, WeatherEffect_AshSpawn);
                work.Cycle.Value = 0;
                work.Scroll.Value = 0;
                work.ScrollTimer.Value = 0;
                work.Ending.Value = 0;
                WeatherFog_StartFadeIn(work.FogFade, work.FogTable, EffectFog(effect), 3, 0x6F2F, 0x3A94, 1, effect.FogMode);
                work.Delay = 0;
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (ParticleFadeInStep(effect, work))
                {
                    effect.State = EffectStateMain;
                    GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                }
                break;
            case EffectStateStart:
                WeatherSpawner_Init(work.Spawner, effect, 1, 6, 1, 6, -1, 4, 0, WeatherEffect_AshSpawn);
                work.Cycle.Value = 0;
                work.Scroll.Value = 0;
                work.ScrollTimer.Value = 0;
                work.Ending.Value = 0;
                ParticleStartFog(effect, work.FogTable, 3, 0x6F2F, 0x3A94);
                WeatherEffect_Prefill(effect, WeatherEffect_AshSpawn, 0x14, 2, 0x10, WeatherParticle_AshMove);
                effect.State = EffectStateMain;
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                break;
            case EffectStateMain:
                if (ParticleSpawnTimer(work.Spawner))
                {
                    WeatherEffect_AshSpawn(effect, work.Spawner.Count);
                    work.Spawner.Timer = work.Spawner.Interval;
                }
                if (effect.Request == RequestFadeOut)
                {
                    WeatherSpawner_SetTargets(work.Spawner, 0, 0xC, 1, -1);
                    if (effect.FogMode != 0)
                    {
                        WeatherFogTable_Start(work.FogTable, 1, 0);
                    }
                    work.Delay = 9;
                    effect.State = EffectStateFadeOut;
                    work.Ending.Value = 1;
                    GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 0);
                }
                break;
            case EffectStateFadeOut:
                if (ParticleFadeOutStep(effect, work) && EffectHasNoParticles(effect))
                {
                    effect.State = EffectStateEnd;
                }
                break;
            case EffectStateEnd:
                ParticleEnd(effect, work.FogTable);
                break;
        }
        if (EffectIsActive(effect))
        {
            WeatherParticleList_ForEach(effect.Sentinel, WeatherParticle_AshMove);
            WeatherEffect_ScrollParticles(effect, out int scrollX, out int scrollY);
            WeatherEffect_AfterUpdate(effect);
            short x = (short)((short)(work.Scroll.Value >> 16) + scrollX);
            short y = (short)((short)work.Scroll.Value - scrollY * 5);
            if (x < 0)
            {
                x = (short)(x + 0x100);
            }
            else if (x >= 0x100)
            {
                x = (short)(x - 0x100);
            }
            if (y < 0)
            {
                y = (short)(y + 0x800);
            }
            work.ScrollTimer.Value += 2;
            if (work.ScrollTimer.Value > 0x3C)
            {
                work.ScrollTimer.Value = 0;
                x = (short)((x + 0x20) % 0x100);
            }
            y = (short)((y + 2) % 0x800);
            SetBg2Offset(x, -y / 5);
            work.Scroll.Value = (x << 16) | (ushort)y;
        }
    }

    // OFFSET 0x021DA5A0
    private static void WeatherEffect_AshSpawn(WeatherEffect effect, int count)
    {
        WeatherParticleWork effectWork = (WeatherParticleWork)effect.Work!;
        if (effectWork.Cycle.Value == 1)
        {
            count <<= 1;
        }
        for (int i = 0; i < count; i++)
        {
            WeatherParticle? particle = WeatherEffect_AddParticle(effect, 0x20);
            if (particle == null)
            {
                break;
            }
            int[] work = particle.Work!;
            Sprite.Sprite_SetAnimFrame(particle.Sprite!, (ushort)(MathUtil.MTRNG_Next() & 3));
            work[4] = 10;
            work[5] = 0;
            if ((int)MathUtil.MTRNG_Next() % 2 == 0)
            {
                work[1] = 1;
            }
            else
            {
                work[1] = -1;
            }
            MathUtil.MTRNG_Next();
            work[3] = 1;
            particle.WorkRefs[0] = effectWork.Cycle;
            particle.WorkRefs[6] = effectWork.Ending;
            work[7] = (int)(MathUtil.MTRNG_Next() % 0x14) + 10;
            VecFx32 position = WeatherParticle_GetPosition(particle);
            position.X = ((int)(MathUtil.MTRNG_Next() % 0x19E) - 0x20) << Fx32.Shift;
            if (effectWork.Cycle.Value == 1 && i >= count / 2)
            {
                position.Y = (-0x28 - (int)(MathUtil.MTRNG_Next() % 0x14)) << Fx32.Shift;
            }
            else
            {
                position.Y = (-8 - (int)(MathUtil.MTRNG_Next() % 0x14)) << Fx32.Shift;
            }
            position.Z = 0;
            WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
        }
    }

    // OFFSET 0x021DA6BC
    private static void WeatherParticle_AshMove(WeatherParticle particle)
    {
        int[] work = particle.Work!;
        VecFx32 position = WeatherParticle_GetPosition(particle);
        bool remove = false;
        if (work[5] >= work[3])
        {
            position.Y += Fx32.One;
            work[5] = 0;
            WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
        }
        work[5]++;
        int y = position.Y >> Fx32.Shift;
        if (particle.WorkRefs[6]!.Value == 1)
        {
            work[7]--;
            if (work[7] <= 0)
            {
                remove = true;
            }
        }
        if (IsOffscreenY(y))
        {
            remove = true;
        }
        if (remove)
        {
            particle.WorkRefs[0]!.Value = 1;
            WeatherParticle_Remove(particle);
        }
    }

    // OFFSET 0x021DA8A0
    private static void WeatherEffect_RisingMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherParticleWork work = (WeatherParticleWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherSpawner_Init(work.Spawner, effect, 1, 0x23, 4, 0xF, -2, 5, 1, WeatherEffect_RisingSpawn);
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (WeatherSpawner_Update(work.Spawner) == SpawnerDone)
                {
                    effect.State = EffectStateMain;
                }
                break;
            case EffectStateStart:
                WeatherSpawner_Init(work.Spawner, effect, 1, 4, 0xF, 0xF, -2, 5, 1, WeatherEffect_RisingSpawn);
                WeatherEffect_Prefill(effect, WeatherEffect_RisingSpawn, 0x10, 2, 1, WeatherParticle_RisingMove);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (ParticleSpawnTimer(work.Spawner))
                {
                    WeatherEffect_RisingSpawn(effect, work.Spawner.Count);
                    work.Spawner.Timer = work.Spawner.Interval;
                }
                if (effect.Request == RequestFadeOut)
                {
                    WeatherSpawner_SetTargets(work.Spawner, 0, 0x23, 2, -3);
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                if (WeatherSpawner_Update(work.Spawner) == SpawnerDone && EffectHasNoParticles(effect))
                {
                    effect.State = EffectStateEnd;
                }
                break;
            case EffectStateEnd:
                WeatherEffectTableEntry_Finish(effect.Entry!);
                break;
        }
        if (EffectIsActive(effect))
        {
            WeatherParticleList_ForEach(effect.Sentinel, WeatherParticle_RisingMove);
            WeatherEffect_ScrollParticles(effect, out _, out _);
            WeatherEffect_AfterUpdate(effect);
        }
    }

    // OFFSET 0x021DA9DC
    private static void WeatherEffect_RisingSpawn(WeatherEffect effect, int count)
    {
        for (int i = 0; i < count; i++)
        {
            WeatherParticle? particle = WeatherEffect_AddParticle(effect, 0x20);
            if (particle == null)
            {
                break;
            }
            int[] work = particle.Work!;
            int frame = (int)(MathUtil.MTRNG_Next() % 0xE);
            Sprite.Sprite_SetAnimFrame(particle.Sprite!, (ushort)frame);
            int size = frame / 4;
            work[0] = ((int)(MathUtil.MTRNG_Next() % 0x19) + 8) * (size + 1);
            work[1] = 0x10 / (size + 1);
            work[2] = 0;
            work[3] = (int)(MathUtil.MTRNG_Next() & 1);
            work[4] = 1;
            VecFx32 position = default;
            switch (size + 1)
            {
                case 1:
                case 2:
                    position.X = (int)(MathUtil.MTRNG_Next() & 0x1FF) - 0x80;
                    position.Y = (int)(MathUtil.MTRNG_Next() % 0xC0) + 8;
                    break;
                case 3:
                    position.X = (int)(MathUtil.MTRNG_Next() & 0x1FF) - 0x80;
                    position.Y = (int)(MathUtil.MTRNG_Next() & 0x7F) + 0x40;
                    break;
                case 4:
                    position.X = (int)(MathUtil.MTRNG_Next() & 0x1FF) - 0x80;
                    position.Y = (int)(MathUtil.MTRNG_Next() & 0x1F) + 0xA0;
                    break;
            }
            position.Z = 0;
            position.X <<= Fx32.Shift;
            position.Y <<= Fx32.Shift;
            WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
        }
    }

    // OFFSET 0x021DAADC
    private static void WeatherParticle_RisingMove(WeatherParticle particle)
    {
        int[] work = particle.Work!;
        VecFx32 position = WeatherParticle_GetPosition(particle);
        work[2] += work[1];
        if (work[0] > 0)
        {
            work[0]--;
            position.Y -= (work[2] / 100) << Fx32.Shift;
        }
        else
        {
            position.Y -= (work[2] / 0x32) << Fx32.Shift;
        }
        work[4]--;
        if (work[4] <= 0)
        {
            work[4] = 1;
            if (work[3] == 0)
            {
                position.X += 2 * Fx32.One;
                work[3] = 1;
            }
            else
            {
                position.X -= 2 * Fx32.One;
                work[3] = 0;
            }
        }
        WeatherSprite_SetPositionWrapped(particle.Sprite!, ref position);
        if ((position.Y >> Fx32.Shift) <= -0x10)
        {
            WeatherParticle_Remove(particle);
        }
    }
}
