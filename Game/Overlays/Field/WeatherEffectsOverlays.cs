namespace PlatinumSharp.Game;

public sealed class WeatherFogWork
{
    public const int Size = 0xC0;

    public readonly WeatherFogTable FogTable = new();
    public readonly WeatherFogFade FogFade = new();
    public int Delay;
    public int Phase;
}

public sealed class WeatherOverlayWork
{
    public const int Size = 0xB8;

    public readonly WeatherLinearValue Alpha = new();
    public readonly WeatherFogTable FogTable = new();
    public readonly WeatherFogFade FogFade = new();
    public int ScrollX;
    public int ScrollY;
    public ushort ScrollTimer;
}

public sealed class WeatherIntWork
{
    public const int Size = 0x28;

    public readonly int[] Values = new int[Size / 4];
}

public sealed class WeatherCircleWork
{
    public const int Size = 0x62C;
    public const int LineCount = 0xC0;

    public short CenterX;
    public short CenterY;
    public readonly WeatherFxValue Radius = new();
    public HBlankSystem? HBlankSystem;
    public HBlankTask? HBlankTask;
    public readonly BufferManager?[] Buffers = new BufferManager?[2];
    public SysTask? SwapTask;
    public int Active;
    public readonly ushort[] LeftA = new ushort[LineCount];
    public readonly ushort[] LeftB = new ushort[LineCount];
    public readonly ushort[] RightA = new ushort[LineCount];
    public readonly ushort[] RightB = new ushort[LineCount];
}

public static partial class WeatherEffects
{
    private const uint PulsingFogDensityTable = 0x021F8E14;
    private const uint ScrollingFogWaveTable = 0x02201D38;
    private const int SeqThunderNear = 0x63D;
    private const int SeqThunderFar = 0x63C;
    private const int BgOffsetSetX = 0;
    private const int BgOffsetSetY = 3;
    private const int Window1 = 1;
    private const int WindowMask1 = 2;
    private const uint CircleSetupPriority = 0x78;
    private const uint CircleFreePriority = 0x80;
    private const uint CircleSwapPriority = 0x400;

    private static void FogWorkStartFull(WeatherEffect effect, WeatherFogTable fogTable, int slope, int offset, ushort color)
    {
        ParticleStartFog(effect, fogTable, slope, offset, color);
    }

    private static void FogOnlyMain(WeatherEffect effect, int offset)
    {
        WeatherFogWork work = (WeatherFogWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherFog_StartFadeIn(work.FogFade, work.FogTable, EffectFog(effect), 3, offset, 0x6B5A, 2, effect.FogMode);
                work.Delay = 0x10;
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (work.Delay > 0)
                {
                    work.Delay--;
                }
                else if (WeatherFog_UpdateFadeIn(work.FogFade, work.FogTable, effect.FogMode) == 1)
                {
                    effect.State = EffectStateMain;
                }
                break;
            case EffectStateStart:
                FogWorkStartFull(effect, work.FogTable, 3, offset, 0x6B5A);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (effect.Request == RequestFadeOut)
                {
                    if (effect.FogMode != 0)
                    {
                        WeatherFogTable_Start(work.FogTable, 2, 0);
                    }
                    work.Delay = 0x10;
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                if (work.Delay > 0)
                {
                    work.Delay--;
                }
                else
                {
                    int fogDone = 1;
                    if (effect.FogMode != 0)
                    {
                        fogDone = WeatherFogTable_Update(work.FogTable);
                    }
                    if (fogDone == 1)
                    {
                        effect.State = EffectStateEnd;
                    }
                }
                break;
            case EffectStateEnd:
                ParticleEnd(effect, work.FogTable);
                break;
        }
    }

    private static void FogBgMain(WeatherEffect effect, int slope, int offset, ushort color, int alpha, bool setPriority)
    {
        WeatherOverlayWork work = (WeatherOverlayWork)effect.Work!;
        int fogDone;
        int alphaDone;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherFog_StartFadeIn(work.FogFade, work.FogTable, EffectFog(effect), slope, offset, color, setPriority ? 2 : 1, effect.FogMode);
                WeatherLinearValue_Init(work.Alpha, 0, alpha, 0x1E);
                Weather_SetBgBlendAlpha(0, 0x10);
                if (setPriority)
                {
                    SetBgPriority(LayerBg2, 3);
                }
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                fogDone = WeatherFogTable_Update(work.FogTable);
                alphaDone = WeatherLinearValue_Step(work.Alpha);
                Weather_SetBgBlendAlpha(work.Alpha.Current, 0x10 - work.Alpha.Current);
                if (fogDone == 1 && alphaDone == 1)
                {
                    effect.State = EffectStateMain;
                }
                break;
            case EffectStateStart:
                FogWorkStartFull(effect, work.FogTable, slope, offset, color);
                Weather_SetBgBlendAlpha(alpha, 0x10 - alpha);
                if (setPriority)
                {
                    SetBgPriority(LayerBg2, 3);
                }
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (effect.Request == RequestFadeOut)
                {
                    if (effect.FogMode != 0)
                    {
                        WeatherFogTable_Start(work.FogTable, 1, 0);
                    }
                    WeatherLinearValue_Init(work.Alpha, alpha, 0, 0x1E);
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                fogDone = 1;
                if (effect.FogMode != 0)
                {
                    fogDone = WeatherFogTable_Update(work.FogTable);
                }
                alphaDone = WeatherLinearValue_Step(work.Alpha);
                Weather_SetBgBlendAlpha(work.Alpha.Current, 0x10 - work.Alpha.Current);
                if (fogDone == 1 && alphaDone == 1)
                {
                    effect.State = EffectStateEnd;
                }
                break;
            case EffectStateEnd:
                ParticleEnd(effect, work.FogTable);
                break;
        }
    }

    // OFFSET 0x021D84D4
    private static void WeatherEffect_LightFogMain(SysTask task, object? param)
    {
        FogOnlyMain((WeatherEffect)param!, 0x716F);
    }

    // OFFSET 0x021D8638
    private static void WeatherEffect_DenseFogMain(SysTask task, object? param)
    {
        FogOnlyMain((WeatherEffect)param!, 0x658F);
    }

    // OFFSET 0x021D879C
    private static void WeatherEffect_WhiteFogBgMain(SysTask task, object? param)
    {
        FogBgMain((WeatherEffect)param!, 5, 0x7A0F, 0x7FFF, 0x10, true);
    }

    // OFFSET 0x021D97E8
    private static void WeatherEffect_FlickerBgMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        int[] work = ((WeatherIntWork)effect.Work!).Values;
        switch (effect.State)
        {
            case EffectStateInit:
                work[0] = 0;
                work[1] = 0;
                Weather_SetBgBlendAlpha(0, 0x10);
                SetBg2Offset(0, 0x20);
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                work[0]++;
                if (work[0] >= 10)
                {
                    work[0] = 0;
                    work[1]++;
                    Weather_SetBgBlendAlpha(work[1], 0x10);
                    if (work[1] >= 10)
                    {
                        work[2] = (int)(MathUtil.MTRNG_Next() % 0x14) + 10;
                        work[3] = (int)(MathUtil.MTRNG_Next() % 3) + 5;
                        work[4] = 1;
                        effect.State = EffectStateMain;
                    }
                }
                break;
            case EffectStateStart:
                work[0] = 0;
                work[1] = 10;
                SetBg2Offset(0, 0x20);
                Weather_SetBgBlendAlpha(work[1], 0x10);
                work[2] = (int)(MathUtil.MTRNG_Next() % 0x14) + 10;
                work[3] = (int)(MathUtil.MTRNG_Next() % 3) + 5;
                work[4] = 1;
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                work[0]++;
                if (work[0] >= work[2])
                {
                    work[0] = 0;
                    if (work[4] == 1)
                    {
                        work[1]--;
                        if (work[1] <= work[3])
                        {
                            work[2] = (int)(MathUtil.MTRNG_Next() % 0xF) + 10;
                            work[3] = (int)(MathUtil.MTRNG_Next() % 3) + 7;
                            work[4] = 0;
                        }
                    }
                    else
                    {
                        work[1]++;
                        if (work[1] >= work[3])
                        {
                            work[2] = (int)(MathUtil.MTRNG_Next() % 0x14) + 10;
                            work[3] = (int)(MathUtil.MTRNG_Next() % 3) + 5;
                            work[4] = 1;
                        }
                    }
                }
                Weather_SetBgBlendAlpha(work[1], 0x10);
                if (effect.Request == RequestFadeOut)
                {
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                work[0]++;
                if (work[0] >= 10)
                {
                    work[0] = 0;
                    work[1]--;
                    Weather_SetBgBlendAlpha(work[1], 0x10);
                    if (work[1] <= 0)
                    {
                        effect.State = EffectStateEnd;
                    }
                }
                break;
            case EffectStateEnd:
                WeatherEffectTableEntry_Finish(effect.Entry!);
                break;
        }
    }

    // OFFSET 0x021D9C20
    private static void WeatherEffect_PulsingBgMain(SysTask task, WeatherEffect effect, int alpha, int step, int start, uint peak)
    {
        WeatherFogWork work = (WeatherFogWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
            case EffectStateStart:
                if (effect.FogMode != 0)
                {
                    WeatherFog_Apply(EffectFog(effect), 5, 0x6FAF, 0);
                    FogManager.FogManager_ApplyDensityTable(EffectFog(effect), RomData.Overlay(Overlay, PulsingFogDensityTable, WeatherFogTable.TableSize));
                    Weather_SetBgBlendAlpha(alpha, 0x10 - alpha);
                    GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                    work.Delay = start;
                    work.Phase = 0;
                }
                effect.State = (ushort)(effect.State == EffectStateInit ? EffectStateFadeIn : EffectStateMain);
                break;
            case EffectStateFadeIn:
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (work.Phase == 0)
                {
                    work.Delay -= step;
                    if (work.Delay <= 0)
                    {
                        work.Phase = 1;
                    }
                }
                else if (work.Phase == 1)
                {
                    work.Delay++;
                    if ((uint)work.Delay >= peak)
                    {
                        work.Phase = 2;
                    }
                }
                else
                {
                    work.Delay--;
                    if (work.Delay <= 0)
                    {
                        work.Phase = 1;
                    }
                }
                int coefficient = alpha + work.Delay / 0x80;
                Weather_SetBgBlendAlpha(coefficient, 0x10 - coefficient);
                if (effect.Request == RequestFadeOut)
                {
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                Weather_SetBgBlendAlpha(0, 0x10);
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 0);
                effect.State = EffectStateEnd;
                break;
            case EffectStateEnd:
                if (effect.FogMode != 0)
                {
                    FogManager.FogManager_ApplyParameters(EffectFog(effect), FogManager.ParameterEnabled, 0, 0, 0, 0);
                }
                WeatherEffectTableEntry_Finish(effect.Entry!);
                break;
        }
    }

    // OFFSET 0x021D9DFC
    private static void WeatherEffect_FogColorMain(WeatherEffect effect, WeatherFogWork work, int slope, int offset, ushort color, int fadeInPeriod, int fadeOutPeriod)
    {
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherFog_StartFadeIn(work.FogFade, work.FogTable, EffectFog(effect), slope, offset, color, fadeInPeriod, effect.FogMode);
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (WeatherFog_UpdateFadeIn(work.FogFade, work.FogTable, effect.FogMode) == 1)
                {
                    effect.State = EffectStateMain;
                }
                break;
            case EffectStateStart:
                FogWorkStartFull(effect, work.FogTable, slope, offset, color);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (effect.Request == RequestFadeOut)
                {
                    if (effect.FogMode != 0)
                    {
                        WeatherFogTable_Start(work.FogTable, fadeOutPeriod, 0);
                    }
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                int fogDone = 1;
                if (effect.FogMode != 0)
                {
                    fogDone = WeatherFogTable_Update(work.FogTable);
                }
                if (fogDone == 1)
                {
                    effect.State = EffectStateEnd;
                }
                break;
            case EffectStateEnd:
                ParticleEnd(effect, work.FogTable);
                break;
        }
    }

    // OFFSET 0x021DA748
    private static void WeatherEffect_LightningMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        int[] work = ((WeatherIntWork)effect.Work!).Values;
        switch (effect.State)
        {
            case EffectStateInit:
            case EffectStateFadeIn:
            case EffectStateStart:
                work[0] = 0;
                work[4] = 0;
                work[1] = 0;
                Weather_SetBgBlendAlpha(0, 0x1F);
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (work[4] >= 0)
                {
                    work[4]--;
                }
                else
                {
                    switch (work[1])
                    {
                        case 0:
                            work[1] = 1;
                            work[5] = 0;
                            work[6] = (int)(MathUtil.MTRNG_Next() % 0x1E0) + 0xC8;
                            work[7] = 0;
                            if (MathUtil.MTRNG_Next() % 3 != 0)
                            {
                                work[2] = 0x24;
                                SoundPlayback.Sound_PlayEffect(SeqThunderNear);
                            }
                            else
                            {
                                work[2] = 0xC8;
                                SoundPlayback.Sound_PlayEffect(SeqThunderFar);
                            }
                            break;
                        case 1:
                            work[5]++;
                            work[7] += work[6];
                            if (work[5] >= 2)
                            {
                                work[1] = 2;
                            }
                            Weather_SetBgBlendAlpha(work[7] / 100, 0x1F);
                            break;
                        case 2:
                            work[7] -= work[2];
                            if (work[7] <= 0)
                            {
                                work[1] = 0;
                                work[7] = 0;
                                if (work[2] == 0xC8)
                                {
                                    work[4] = (int)(MathUtil.MTRNG_Next() % 0xF);
                                }
                                else
                                {
                                    uint random = MathUtil.MTRNG_Next();
                                    work[4] = work[6] * 0x32 / 100 + (int)(random % 0x78);
                                }
                            }
                            Weather_SetBgBlendAlpha(work[7] / 100, 0x1F);
                            break;
                    }
                }
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

    // OFFSET 0x021DAB78
    private static void WeatherEffect_OverlayBgMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherOverlayWork work = (WeatherOverlayWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherLinearValue_Init(work.Alpha, 0, 0x10, 8);
                Weather_SetBgBlendAlpha(0, 0x10);
                SetBgPriority(LayerBg2, 3);
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                {
                    int done = WeatherLinearValue_Step(work.Alpha);
                    Weather_SetBgBlendAlpha(work.Alpha.Current, 0x10 - work.Alpha.Current);
                    if (done == 1)
                    {
                        effect.State = EffectStateMain;
                    }
                }
                break;
            case EffectStateStart:
                Weather_SetBgBlendAlpha(0x10, 0);
                SetBgPriority(LayerBg2, 3);
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (effect.Request == RequestFadeOut)
                {
                    WeatherLinearValue_Init(work.Alpha, 0x10, 0, 8);
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                {
                    int done = WeatherLinearValue_Step(work.Alpha);
                    Weather_SetBgBlendAlpha(work.Alpha.Current, 0x10 - work.Alpha.Current);
                    if (done == 1)
                    {
                        effect.State = EffectStateEnd;
                    }
                }
                break;
            case EffectStateEnd:
                WeatherEffectTableEntry_Finish(effect.Entry!);
                break;
        }
    }

    // OFFSET 0x021DAC68
    private static void WeatherEffect_FaintOverlayBgMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherOverlayWork work = (WeatherOverlayWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherLinearValue_Init(work.Alpha, 0, 4, 8);
                Weather_SetBgBlendAlpha(0, 0x10);
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                {
                    int done = WeatherLinearValue_Step(work.Alpha);
                    Weather_SetBgBlendAlpha(work.Alpha.Current, 0x10 - work.Alpha.Current);
                    if (done != 0)
                    {
                        effect.State = EffectStateMain;
                    }
                }
                break;
            case EffectStateStart:
                Weather_SetBgBlendAlpha(4, 0xC);
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (effect.Request == RequestFadeOut)
                {
                    WeatherLinearValue_Init(work.Alpha, 4, 0, 8);
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                {
                    int done = WeatherLinearValue_Step(work.Alpha);
                    Weather_SetBgBlendAlpha(work.Alpha.Current, 0x10 - work.Alpha.Current);
                    if (done != 0)
                    {
                        effect.State = EffectStateEnd;
                    }
                }
                break;
            case EffectStateEnd:
                WeatherEffectTableEntry_Finish(effect.Entry!);
                break;
        }
    }

    // OFFSET 0x021DAD38
    private static void WeatherEffect_DarkFogBgMain(SysTask task, object? param)
    {
        FogBgMain((WeatherEffect)param!, 7, 0x764F, 0, 9, false);
    }

    // OFFSET 0x021DAEC0
    private static void WeatherEffect_PaleFogBgMain(SysTask task, object? param)
    {
        FogBgMain((WeatherEffect)param!, 6, 0x7555, 0x7FFF, 9, false);
    }

    // OFFSET 0x021DB04C
    private static void WeatherEffect_CircleWindowMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        FieldSystem fieldSystem = effect.Manager!.FieldSystem!;
        WeatherCircleWork work = (WeatherCircleWork)effect.Work!;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherCircle_Init(work, fieldSystem.FieldMapContext!.HBlankSystem!);
                WeatherCircle_Start(work, 0x100000, 0x20000, 0x80, 0x54, 0x1E);
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                if (WeatherCircle_Update(work) == 1)
                {
                    GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                    WeatherCircle_QueueFree(work);
                    effect.State = EffectStateMain;
                }
                break;
            case EffectStateStart:
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (effect.Request == RequestFadeOut)
                {
                    WeatherCircle_Init(work, fieldSystem.FieldMapContext!.HBlankSystem!);
                    WeatherCircle_Start(work, 0x20000, 0x100000, 0x80, 0x54, 0x1E);
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 0);
                if (WeatherCircle_Update(work) == 1)
                {
                    effect.State = EffectStateEnd;
                }
                break;
            case EffectStateEnd:
                WeatherCircle_Free(work);
                WeatherEffectTableEntry_Finish(effect.Entry!);
                break;
        }
    }

    // OFFSET 0x021DB144
    private static void WeatherEffect_ScrollingFogBgMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherOverlayWork work = (WeatherOverlayWork)effect.Work!;
        int fogDone;
        int alphaDone;
        switch (effect.State)
        {
            case EffectStateInit:
                WeatherLinearValue_Init(work.Alpha, 0, 7, 8);
                Weather_SetBgBlendAlpha(0, 0x10);
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                WeatherFog_StartFadeIn(work.FogFade, work.FogTable, EffectFog(effect), 3, 0x692F, 0x7FFF, 1, effect.FogMode);
                work.ScrollX = 0;
                work.ScrollY = 0;
                work.ScrollTimer = 0;
                effect.State = EffectStateFadeIn;
                break;
            case EffectStateFadeIn:
                fogDone = WeatherFog_UpdateFadeIn(work.FogFade, work.FogTable, effect.FogMode);
                alphaDone = WeatherLinearValue_Step(work.Alpha);
                Weather_SetBgBlendAlpha(work.Alpha.Current, 0x10 - work.Alpha.Current);
                if (alphaDone == fogDone && alphaDone == 1)
                {
                    effect.State = EffectStateMain;
                }
                break;
            case EffectStateStart:
                Weather_SetBgBlendAlpha(7, 9);
                FogWorkStartFull(effect, work.FogTable, 3, 0x692F, 0x7FFF);
                GXLayers.GXLayers_EngineAToggleLayers(LayerMaskBg2, 1);
                work.ScrollX = 0;
                work.ScrollY = 0;
                work.ScrollTimer = 0;
                effect.State = EffectStateMain;
                break;
            case EffectStateMain:
                if (effect.Request == RequestFadeOut)
                {
                    WeatherLinearValue_Init(work.Alpha, 7, 0, 8);
                    if (effect.FogMode != 0)
                    {
                        WeatherFogTable_Start(work.FogTable, 1, 0);
                    }
                    effect.State = EffectStateFadeOut;
                }
                break;
            case EffectStateFadeOut:
                alphaDone = WeatherLinearValue_Step(work.Alpha);
                fogDone = WeatherFogTable_Update(work.FogTable);
                Weather_SetBgBlendAlpha(work.Alpha.Current, 0x10 - work.Alpha.Current);
                if (alphaDone == fogDone && alphaDone == 1)
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
            WeatherEffect_ScrollParticlesFx(effect, out int scrollX, out int scrollY);
            scrollY = Fx32.Mul(scrollY, 0xC00);
            work.ScrollX += scrollX;
            work.ScrollY += scrollY;
            work.ScrollTimer++;
            if (work.ScrollTimer >= 0x200)
            {
                work.ScrollTimer = 0;
            }
            BgConfig bgConfig = effect.Manager!.FieldSystem!.BgConfig!;
            byte wave = RomData.Overlay(Overlay, ScrollingFogWaveTable, 0x40)[work.ScrollTimer >> 3];
            Bg.Bg_ScheduleScroll(bgConfig, LayerBg2, BgOffsetSetX, (work.ScrollX >> Fx32.Shift) + wave);
            Bg.Bg_ScheduleScroll(bgConfig, LayerBg2, BgOffsetSetY, work.ScrollY >> Fx32.Shift);
        }
    }

    // OFFSET 0x021DB3A8
    private static void WeatherEffect_PulsingBgStrongMain(SysTask task, object? param)
    {
        WeatherEffect_PulsingBgMain(task, (WeatherEffect)param!, 4, 4, 0x500, 0x100);
    }

    // OFFSET 0x021DB3C4
    private static void WeatherEffect_PulsingBgWeakMain(SysTask task, object? param)
    {
        WeatherEffect_PulsingBgMain(task, (WeatherEffect)param!, 6, 1, 0x400, 0x100);
    }

    // OFFSET 0x021DB3E0
    private static void WeatherEffect_FogColorAMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherEffect_FogColorMain(effect, (WeatherFogWork)effect.Work!, 3, 0x65EF, 0x1842, 0, 0);
    }

    // OFFSET 0x021DB40C
    private static void WeatherEffect_FogColorBMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherEffect_FogColorMain(effect, (WeatherFogWork)effect.Work!, 2, 0x672F, 0x7B2D, 0, 0);
    }

    // OFFSET 0x021DB438
    private static void WeatherEffect_FogColorCMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherEffect_FogColorMain(effect, (WeatherFogWork)effect.Work!, 2, 0x672F, 0x14, 0, 0);
    }

    // OFFSET 0x021DB460
    private static void WeatherEffect_FogColorDMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherEffect_FogColorMain(effect, (WeatherFogWork)effect.Work!, 2, 0x672F, 0x5000, 0, 0);
    }

    // OFFSET 0x021DB48C
    private static void WeatherEffect_FogColorEMain(SysTask task, object? param)
    {
        WeatherEffect effect = (WeatherEffect)param!;
        WeatherEffect_FogColorMain(effect, (WeatherFogWork)effect.Work!, 1, 0x4B6F, 0x421, 0, 0);
    }

    // OFFSET 0x021DB500
    private static void WeatherCircle_ComputeLines(WeatherCircleWork work)
    {
        ushort[] left = (ushort[])BufferManager.BufferManager_GetWriteBuffer(work.Buffers[0])!;
        ushort[] right = (ushort[])BufferManager.BufferManager_GetWriteBuffer(work.Buffers[1])!;
        for (int y = 0; y < WeatherCircleWork.LineCount; y++)
        {
            int lineLeft;
            int lineRight;
            if (y <= work.CenterY || y > work.CenterY * 2)
            {
                WeatherCircle_CalcLine(work.Radius.Current, work.CenterX, work.CenterY, y, out lineLeft, out lineRight);
            }
            else
            {
                lineLeft = left[work.CenterY * 2 - y];
                lineRight = right[work.CenterY * 2 - y];
            }
            left[y] = (ushort)lineLeft;
            right[y] = (ushort)lineRight;
        }
    }

    // OFFSET 0x021DB588
    private static void WeatherCircle_CalcLine(int radius, int centerX, int centerY, int y, out int left, out int right)
    {
        int distance = y - centerY;
        if (distance < 0)
        {
            distance = -distance;
        }
        if (distance >= radius)
        {
            left = 0;
            right = 0;
            return;
        }
        int distanceFx = distance << Fx32.Shift;
        int half = Fx32.Sqrt(Fx32.Mul(radius, radius) - Fx32.Mul(distanceFx, distanceFx)) >> Fx32.Shift;
        left = centerX - half;
        if (left < 0)
        {
            left = 0;
        }
        right = left + half * 2;
        if (right > 0xFF)
        {
            right = 0xFF;
        }
    }

    // OFFSET 0x021DB614
    private static void WeatherCircle_Init(WeatherCircleWork work, HBlankSystem hBlankSystem)
    {
        work.CenterX = 0;
        work.CenterY = 0;
        work.Radius.Current = 0;
        work.Radius.Start = 0;
        work.Radius.Delta = 0;
        work.Radius.Step = 0;
        work.Radius.StepCount = 0;
        work.HBlankTask = null;
        Array.Clear(work.LeftA);
        Array.Clear(work.LeftB);
        work.Buffers[0] = BufferManager.BufferManager_New(HeapIDField, work.LeftA, work.LeftB);
        work.Buffers[1] = BufferManager.BufferManager_New(HeapIDField, work.RightA, work.RightB);
        for (int i = 0; i < WeatherCircleWork.LineCount; i++)
        {
            work.RightA[i] = 0xFF;
            work.RightB[i] = 0xFF;
        }
        work.HBlankSystem = hBlankSystem;
        SysTask.SysTask_ExecuteAfterVBlank(WeatherCircle_SetupWindowTask, work, CircleSetupPriority);
        work.SwapTask = SysTask.SysTask_ExecuteAfterVBlank(WeatherCircle_SwapTask, work, CircleSwapPriority);
        work.Active = 0;
    }

    // OFFSET 0x021DB690
    private static void WeatherCircle_Free(WeatherCircleWork work)
    {
        Video.SetVisibleWindows(DisplayMain, Video.GetVisibleWindows(DisplayMain) & ~WindowMask1);
        HBlankSystem.HBlankTask_Delete(work.HBlankTask!);
        SysTask.SysTask_Done(work.SwapTask!);
        BufferManager.BufferManager_Delete(work.Buffers[0]);
        BufferManager.BufferManager_Delete(work.Buffers[1]);
        work.CenterX = 0;
        work.CenterY = 0;
        work.Radius.Current = 0;
        work.Radius.Start = 0;
        work.Radius.Delta = 0;
        work.Radius.Step = 0;
        work.Radius.StepCount = 0;
        work.HBlankSystem = null;
        work.HBlankTask = null;
        work.Buffers[0] = null;
        work.Buffers[1] = null;
        work.SwapTask = null;
        work.Active = 0;
        Array.Clear(work.LeftA);
        Array.Clear(work.LeftB);
        Array.Clear(work.RightA);
        Array.Clear(work.RightB);
    }

    // OFFSET 0x021DB6E0
    private static void WeatherCircle_Start(WeatherCircleWork work, int startRadius, int endRadius, int centerX, int centerY, int stepCount)
    {
        WeatherFxValue_Init(work.Radius, startRadius, endRadius, stepCount);
        work.CenterX = (short)centerX;
        work.CenterY = (short)centerY;
        work.Active = 1;
        WeatherCircle_ComputeLines(work);
    }

    // OFFSET 0x021DB700
    private static int WeatherCircle_Update(WeatherCircleWork work)
    {
        if (work.Active == 0)
        {
            WeatherCircle_CopyReadToWrite(work);
            return 1;
        }
        int done = WeatherFxValue_Step(work.Radius);
        WeatherCircle_ComputeLines(work);
        if (done == 1)
        {
            work.Active = 0;
        }
        return done;
    }

    // OFFSET 0x021DB72C
    private static void WeatherCircle_HBlank(HBlankTask task, object? param)
    {
        WeatherCircleWork work = (WeatherCircleWork)param!;
        int line = Video.VCount;
        ushort[] left = (ushort[])BufferManager.BufferManager_GetReadBuffer(work.Buffers[0])!;
        ushort[] right = (ushort[])BufferManager.BufferManager_GetReadBuffer(work.Buffers[1])!;
        if (line >= WeatherCircleWork.LineCount)
        {
            return;
        }
        line++;
        if (line >= WeatherCircleWork.LineCount)
        {
            line -= WeatherCircleWork.LineCount;
        }
        if (Video.InHBlank)
        {
            Video.SetWindowHorizontal(DisplayMain, Window1, (byte)left[line], (byte)right[line]);
            Video.SetWindowVertical(DisplayMain, Window1, 0, WeatherCircleWork.LineCount);
        }
    }

    // OFFSET 0x021DB78C
    private static void WeatherCircle_SwapTask(SysTask task, object? param)
    {
        WeatherCircleWork work = (WeatherCircleWork)param!;
        for (int i = 0; i < 2; i++)
        {
            BufferManager.BufferManager_SwapBuffers(work.Buffers[i]);
        }
    }

    // OFFSET 0x021DB7A4
    private static void WeatherCircle_QueueFree(WeatherCircleWork work)
    {
        SysTask.SysTask_ExecuteAfterVBlank(WeatherCircle_FreeTask, work, CircleFreePriority);
    }

    // OFFSET 0x021DB7B8
    private static void WeatherCircle_FreeTask(SysTask task, object? param)
    {
        WeatherCircle_Free((WeatherCircleWork)param!);
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x021DB7CC
    private static void WeatherCircle_CopyReadToWrite(WeatherCircleWork work)
    {
        for (int i = 0; i < 2; i++)
        {
            ushort[] source = (ushort[])BufferManager.BufferManager_GetReadBuffer(work.Buffers[i])!;
            ushort[] destination = (ushort[])BufferManager.BufferManager_GetWriteBuffer(work.Buffers[i])!;
            Array.Copy(source, destination, WeatherCircleWork.LineCount);
        }
    }

    // OFFSET 0x021DB7F8
    private static void WeatherCircle_SetupWindowTask(SysTask task, object? param)
    {
        WeatherCircleWork work = (WeatherCircleWork)param!;
        int visibleWindows = Video.GetVisibleWindows(DisplayMain);
        Video.SetWindowInside(DisplayMain, Window1, 0x3F);
        Video.SetWindowOutside(DisplayMain, 0x3E);
        Video.SetWindowHorizontal(DisplayMain, Window1, 0, 0xFF);
        Video.SetWindowVertical(DisplayMain, Window1, 0, WeatherCircleWork.LineCount);
        Video.SetVisibleWindows(DisplayMain, visibleWindows | WindowMask1);
        work.HBlankTask = HBlankSystem.HBlankSystem_StartTask(work.HBlankSystem!, WeatherCircle_HBlank, work);
        SysTask.SysTask_Done(task);
    }
}
