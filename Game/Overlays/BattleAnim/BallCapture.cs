namespace PlatinumSharp.Game;

public sealed class BallCaptureEffectParams
{
    public short X;
    public short Y;
    public int BallID;
    public int HeapID;
    public int Emitter;
    public int UseDefaultBall;
}

public sealed class BallCaptureEffect
{
    public short X;
    public short Y;
    public int BallID;
    public int HeapID;
    public int Emitter;
    public int UseDefaultBall;
    public ParticleSystem? ParticleSys;
    public int EmitterCount;
    public int ParticleMember;
}

public static partial class BallRotations
{
    private const uint BallCaptureParticleTable = 0x0223AE10;
    private const int BallCaptureParticleEntrySize = 0xC;
    private const int BallIDDefaultCapture = 0x113;
    private const int NarcBallCaptureParticles = 0x63;
    private const int CaptureEmitterAll = 0xFF;
    private const int CapturePositionScale = 0xAC;
    private const uint CaptureDropTable = 0x0223ACF0;
    private const int CaptureDropEntrySize = 6;
    private const uint CaptureDropSoundTable = 0x0223ABE4;
    private const int CaptureDropSoundCount = 5;
    private const uint CaptureShakeTable = 0x0223AC38;
    private const int CaptureShakeRowSize = 0x18;
    private const int CaptureShakeRows = 3;
    private const short CaptureTableEnd = 0xFF;
    private const ushort SeqBallShake = 0x5FE;
    private const int CaptureSoundPan = 0x75;
    private const int PaletteBufferMainObjFade = 4;
    private const ushort CaptureFlashColor = 0x37F;

    private static readonly Func<BallRotation, int>[] CaptureSignalFuncs =
    {
        BallRotation_CaptureIdle,
        BallRotation_CaptureThrow,
        BallRotation_CaptureThrowDone,
        BallRotation_CaptureOpen,
        BallRotation_CaptureOpenDone,
        BallRotation_CaptureDeflect,
        BallRotation_CaptureDeflectDone,
        BallRotation_CaptureDropStart,
        BallRotation_CaptureFlashStart,
        BallRotation_CaptureFlash,
        BallRotation_CaptureFallStart,
        BallRotation_CaptureFall,
        BallRotation_CaptureFallDone,
        BallRotation_CaptureBounce,
        BallRotation_CaptureBounceDone,
        BallRotation_CaptureSettle,
        BallRotation_CaptureShakeStart,
        BallRotation_CaptureShake,
        BallRotation_CaptureShakeDone,
        BallRotation_CaptureBreakOutStart,
        BallRotation_CaptureLockStart,
        BallRotation_CaptureLock,
        BallRotation_CaptureLockDone,
        BallRotation_CaptureDim,
        BallRotation_CaptureFadeOutStart,
        BallRotation_CaptureFadeOut,
        BallRotation_CaptureFadeOutDone,
        BallRotation_CaptureFinished,
    };

    // OFFSET 0x02235F8C
    private static int Ball_GetCaptureParticleMember(int ballID)
    {
        int index = Ball_GetResourceIndex(ballID);
        return (int)RomData.OverlayU32(12, (uint)(BallCaptureParticleTable + index * BallCaptureParticleEntrySize), 1)[0];
    }

    // OFFSET 0x02235FA0
    private static int Ball_GetCaptureEmitterCount(int ballID)
    {
        int index = Ball_GetResourceIndex(ballID);
        return (int)RomData.OverlayU32(12, (uint)(BallCaptureParticleTable + index * BallCaptureParticleEntrySize + 8), 1)[0];
    }

    // OFFSET 0x02235FB4
    private static int Ball_GetCaptureLockEmitter(int ballID)
    {
        int index = Ball_GetResourceIndex(ballID);
        return (int)RomData.OverlayU32(12, (uint)(BallCaptureParticleTable + index * BallCaptureParticleEntrySize + 4), 1)[0];
    }

    // OFFSET 0x02236648
    private static void BallCaptureEffect_EmitterCallback(SplEmitter emitter)
    {
        BallCaptureEffect effect = (BallCaptureEffect)ParticleSystem.ParticleSystem_GetEmitterCallbackParam()!;
        int y = (short)(100 - effect.Y) * CapturePositionScale;
        int x = (short)(effect.X - 0x81) * CapturePositionScale;
        VecFx32 basePos = emitter.Resource!.Header.EmitterBasePos;
        emitter.Position.X = x + basePos.X;
        emitter.Position.Y = y + basePos.Y;
        emitter.Position.Z = basePos.Z;
    }

    // OFFSET 0x02236690
    private static BallCaptureEffect BallCaptureEffect_New(BallCaptureEffectParams effectParams)
    {
        BallCaptureEffect effect = new();
        effect.X = effectParams.X;
        effect.Y = effectParams.Y;
        effect.BallID = effectParams.BallID;
        effect.HeapID = effectParams.HeapID;
        effect.Emitter = effectParams.Emitter;
        effect.UseDefaultBall = effectParams.UseDefaultBall;
        if (effect.UseDefaultBall != 0)
        {
            effect.ParticleMember = Ball_GetCaptureParticleMember(BallIDDefaultCapture);
            effect.EmitterCount = Ball_GetCaptureEmitterCount(BallIDDefaultCapture);
        }
        else
        {
            effect.ParticleMember = Ball_GetCaptureParticleMember(effect.BallID);
            effect.EmitterCount = Ball_GetCaptureEmitterCount(effect.BallID);
        }
        effect.ParticleSys = BattleParticlePlayers.BattleParticleUtil_CreateFromNarc(effect.HeapID, NarcBallCaptureParticles, effect.ParticleMember, 0);
        return effect;
    }

    // OFFSET 0x022366F0
    private static void BallCaptureEffect_Start(BallCaptureEffect effect)
    {
        if (effect.Emitter == CaptureEmitterAll)
        {
            if (effect.UseDefaultBall != 0)
            {
                for (int i = 0; i < effect.EmitterCount; i++)
                {
                    ParticleSystem.ParticleSystem_CreateEmitterWithCallback(effect.ParticleSys!, i, BallCaptureEffect_EmitterCallback, effect);
                }
            }
            else
            {
                for (int i = 0; i < effect.EmitterCount; i++)
                {
                    if (i != Ball_GetCaptureLockEmitter(effect.BallID))
                    {
                        ParticleSystem.ParticleSystem_CreateEmitterWithCallback(effect.ParticleSys!, i, BallCaptureEffect_EmitterCallback, effect);
                    }
                }
            }
        }
        else
        {
            ParticleSystem.ParticleSystem_CreateEmitterWithCallback(effect.ParticleSys!, effect.Emitter, BallCaptureEffect_EmitterCallback, effect);
        }
        ParticleSystem.ParticleSystem_SetCameraProjection(effect.ParticleSys!, 1);
    }

    // OFFSET 0x02236764
    private static int BallCaptureEffect_IsActive(BallCaptureEffect effect)
    {
        if (ParticleSystem.ParticleSystem_GetActiveEmitterCount(effect.ParticleSys!) == 0)
        {
            BattleParticlePlayers.BattleParticleUtil_Free(effect.ParticleSys!);
            return 0;
        }
        return 1;
    }

    // OFFSET 0x02236780
    private static void BallCaptureEffect_Free(BallCaptureEffect? effect)
    {
    }

    // OFFSET 0x022368E4
    private static void BallRotation_SetCaptureSignal(BallRotation rotation, int signal)
    {
        rotation.Signal = signal;
        rotation.State = 0;
        rotation.SubState = 0;
        rotation.Counter = 0;
    }

    private static void BallRotation_FadeSprite(BallRotation rotation, sbyte wait, byte start, byte end, ushort color)
    {
        uint offset = SpriteSystem.ManagedSprite_GetExplicitPaletteOffset(rotation.Sprite!);
        PaletteData.PaletteData_StartFade(rotation.Template.PaletteData!, PaletteBufferMainObjFade, (ushort)(1 << (int)offset), wait, start, end, color);
    }

    // OFFSET 0x022368F0
    private static int BallRotation_CaptureIdle(BallRotation rotation)
    {
        return 1;
    }

    // OFFSET 0x022368F4
    private static int BallRotation_CaptureThrow(BallRotation rotation)
    {
        if (BallRotation_ThrowMode(rotation) == 0)
        {
            BallRotation_SetCaptureSignal(rotation, 2);
        }
        return 1;
    }

    // OFFSET 0x0223690C
    private static int BallRotation_CaptureThrowDone(BallRotation rotation)
    {
        BallRotation_SetCaptureSignal(rotation, 3);
        return 1;
    }

    // OFFSET 0x02236918
    private static int BallRotation_CaptureOpen(BallRotation rotation)
    {
        switch (rotation.State)
        {
            case 0:
            {
                SpriteSystem.ManagedSprite_SetAnim(rotation.Sprite!, 1);
                BallCaptureEffectParams effectParams = new();
                effectParams.BallID = rotation.Template.BallID;
                effectParams.HeapID = rotation.Template.HeapID;
                effectParams.Emitter = CaptureEmitterAll;
                effectParams.UseDefaultBall = 0;
                SpriteSystem.ManagedSprite_GetPositionXY(rotation.Sprite!, out effectParams.X, out effectParams.Y);
                rotation.ObjSprites = BattlerObjSpritesFuncs.BattlerObjSprites_NewAll(rotation.Template.BattleSys!, rotation.Template.HeapID);
                rotation.CaptureEffect = BallCaptureEffect_New(effectParams);
                rotation.State++;
                break;
            }
            case 1:
                if (SpriteSystem.ManagedSprite_GetAnimationFrame(rotation.Sprite!) >= 2)
                {
                    rotation.TickAnimation = 0;
                    rotation.State++;
                }
                break;
            case 2:
                BallCaptureEffect_Start(rotation.CaptureEffect!);
                BattlerObjSpritesFuncs.BattlerObjSprites_Free(rotation.ObjSprites!);
                rotation.State++;
                break;
            case 3:
                if (BallCaptureEffect_IsActive(rotation.CaptureEffect!) == 0)
                {
                    SpriteSystem.ManagedSprite_SetAnimationFrame(rotation.Sprite!, 0);
                    BallCaptureEffect_Free(rotation.CaptureEffect);
                    rotation.State++;
                }
                break;
            default:
                BallRotation_SetCaptureSignal(rotation, 4);
                break;
        }
        return 1;
    }

    // OFFSET 0x022369F8
    private static int BallRotation_CaptureOpenDone(BallRotation rotation)
    {
        return 1;
    }

    // OFFSET 0x022369FC
    private static int BallRotation_CaptureDeflect(BallRotation rotation)
    {
        if (rotation.SubState == 0)
        {
            SpriteSystem.ManagedSprite_GetPositionXY(rotation.Sprite!, out rotation.X, out rotation.Y);
            rotation.TargetX = 0x3C;
            rotation.TargetY = 0xB4;
            rotation.Frames = 10;
            rotation.ArcHeight = 12;
            rotation.SubState++;
        }
        else if (BallRotation_ThrowMode(rotation) == 0)
        {
            BallRotation_SetCaptureSignal(rotation, 6);
        }
        return 1;
    }

    // OFFSET 0x02236A50
    private static int BallRotation_CaptureDeflectDone(BallRotation rotation)
    {
        return 1;
    }

    // OFFSET 0x02236A54
    private static int BallRotation_CaptureDropStart(BallRotation rotation)
    {
        BallRotation_SetCaptureSignal(rotation, 8);
        return 1;
    }

    // OFFSET 0x02236A60
    private static int BallRotation_CaptureFlashStart(BallRotation rotation)
    {
        BallRotation_SetCaptureSignal(rotation, 9);
        return 1;
    }

    // OFFSET 0x02236A6C
    private static int BallRotation_CaptureFlash(BallRotation rotation)
    {
        switch (rotation.State)
        {
            case 0:
                BallRotation_FadeSprite(rotation, -1, 0, 12, CaptureFlashColor);
                rotation.State++;
                break;
            case 1:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(rotation.Template.PaletteData!) == 0)
                {
                    BallRotation_FadeSprite(rotation, -1, 12, 0, CaptureFlashColor);
                    rotation.State++;
                }
                break;
            default:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(rotation.Template.PaletteData!) == 0)
                {
                    BallRotation_SetCaptureSignal(rotation, 0xA);
                }
                break;
        }
        return 1;
    }

    // OFFSET 0x02236B14
    private static int BallRotation_CaptureFallStart(BallRotation rotation)
    {
        BallRotation_SetCaptureSignal(rotation, 0xB);
        return 1;
    }

    // OFFSET 0x02236B20
    private static int BallRotation_CaptureFall(BallRotation rotation)
    {
        switch (rotation.State)
        {
            case 0:
                rotation.SubState++;
                if (rotation.SubState >= 1)
                {
                    SpriteSystem.ManagedSprite_GetPositionXY(rotation.Sprite!, out short x, out short y);
                    BattleAnimUtil.PosLerpContext_Init(rotation.LinearContext, x, x, y, (short)(y + 0x20), 10);
                    rotation.State++;
                }
                break;
            case 1:
                if (BattleAnimUtil.PosLerpContext_UpdateAndApplyToSprite(rotation.LinearContext, rotation.Sprite!) == 0)
                {
                    rotation.State++;
                    BallRotation_SetCaptureSignal(rotation, 0xD);
                }
                break;
        }
        return 1;
    }

    // OFFSET 0x02236B94
    private static int BallRotation_CaptureFallDone(BallRotation rotation)
    {
        return 1;
    }

    // OFFSET 0x02236B98
    private static int BallRotation_CaptureBounce(BallRotation rotation)
    {
        switch (rotation.State)
        {
            case 0:
                SpriteSystem.ManagedSprite_SetAnim(rotation.Sprite!, 1);
                SpriteSystem.ManagedSprite_SetAnimationFrame(rotation.Sprite!, 0);
                rotation.State++;
                goto case 1;
            case 1:
            {
                ushort[] entry = RomData.OverlayU16(12, (uint)(CaptureDropTable + rotation.SubState * CaptureDropEntrySize), 3);
                short dy = (short)entry[1];
                SpriteSystem.ManagedSprite_SetAnimationFrame(rotation.Sprite!, entry[2]);
                rotation.SubState++;
                uint[] sounds = RomData.OverlayU32(12, CaptureDropSoundTable, CaptureDropSoundCount * 2);
                for (int i = 0; i < CaptureDropSoundCount; i++)
                {
                    if (rotation.SubState == (int)sounds[i * 2])
                    {
                        SoundPlayback.Sound_PlayPannedEffect((ushort)sounds[i * 2 + 1], CaptureSoundPan);
                    }
                }
                if (rotation.SubState > 0x14 || dy == CaptureTableEnd)
                {
                    rotation.State++;
                }
                else
                {
                    SpriteSystem.ManagedSprite_OffsetPositionXY(rotation.Sprite!, 0, dy);
                }
                break;
            }
            default:
                BallRotation_SetCaptureSignal(rotation, 0xE);
                break;
        }
        return 1;
    }

    // OFFSET 0x02236C34
    private static int BallRotation_CaptureBounceDone(BallRotation rotation)
    {
        return 1;
    }

    // OFFSET 0x02236C38
    private static int BallRotation_CaptureSettle(BallRotation rotation)
    {
        rotation.SubState++;
        if (rotation.SubState >= 0xE)
        {
            rotation.State++;
            rotation.SubState = 0;
            BallRotation_SetCaptureSignal(rotation, 0x10);
        }
        return 1;
    }

    // OFFSET 0x02236C58
    private static int BallRotation_CaptureShakeStart(BallRotation rotation)
    {
        BallRotation_SetCaptureSignal(rotation, 0x11);
        return 1;
    }

    // OFFSET 0x02236C64
    private static int BallRotation_CaptureShake(BallRotation rotation)
    {
        switch (rotation.State)
        {
            case 0:
                if (rotation.Unknown18 >= CaptureShakeRows)
                {
                    ErrorHandling.GF_AssertFail();
                }
                rotation.State++;
                goto case 1;
            case 1:
            {
                short dx = (short)RomData.OverlayU16(12, (uint)(CaptureShakeTable + rotation.Unknown18 * CaptureShakeRowSize + rotation.SubState * 2), 1)[0];
                rotation.SubState++;
                if (rotation.SubState > 0xB || dx == CaptureTableEnd)
                {
                    rotation.State++;
                    rotation.Unknown18++;
                    SpriteSystem.ManagedSprite_SetAffineZRotation(rotation.Sprite!, 0);
                    BallRotation_SetCaptureSignal(rotation, 0x12);
                    return 1;
                }
                if (rotation.SubState == 5)
                {
                    SoundPlayback.Sound_PlayPannedEffect(SeqBallShake, CaptureSoundPan);
                }
                SpriteSystem.ManagedSprite_OffsetPositionXY(rotation.Sprite!, dx, 0);
                SpriteSystem.ManagedSprite_OffsetAffineZRotation(rotation.Sprite!, dx * 0x1FFFE / 360);
                break;
            }
        }
        return 1;
    }

    // OFFSET 0x02236CFC
    private static int BallRotation_CaptureShakeDone(BallRotation rotation)
    {
        return 1;
    }

    // OFFSET 0x02236D00
    private static int BallRotation_CaptureBreakOutStart(BallRotation rotation)
    {
        BallRotation_SetCaptureSignal(rotation, 0x14);
        return 1;
    }

    // OFFSET 0x02236D0C
    private static int BallRotation_CaptureLockStart(BallRotation rotation)
    {
        BallRotation_SetCaptureSignal(rotation, 0x1B);
        return 1;
    }

    // OFFSET 0x02236D18
    private static int BallRotation_CaptureLock(BallRotation rotation)
    {
        switch (rotation.State)
        {
            case 0:
                BallRotation_FadeSprite(rotation, -5, 0, 10, 0);
                rotation.State++;
                break;
            case 1:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(rotation.Template.PaletteData!) == 0)
                {
                    BallCaptureEffectParams effectParams = new();
                    effectParams.BallID = rotation.Template.BallID;
                    effectParams.HeapID = rotation.Template.HeapID;
                    effectParams.Emitter = Ball_GetCaptureLockEmitter(rotation.Template.BallID);
                    effectParams.UseDefaultBall = 0;
                    SpriteSystem.ManagedSprite_GetPositionXY(rotation.Sprite!, out effectParams.X, out effectParams.Y);
                    rotation.CaptureEffect = BallCaptureEffect_New(effectParams);
                    rotation.State++;
                }
                break;
            case 2:
                BallCaptureEffect_Start(rotation.CaptureEffect!);
                rotation.State++;
                break;
            case 3:
                if (BallCaptureEffect_IsActive(rotation.CaptureEffect!) == 0)
                {
                    SpriteSystem.ManagedSprite_SetAnimationFrame(rotation.Sprite!, 0);
                    BallCaptureEffect_Free(rotation.CaptureEffect);
                    rotation.State++;
                }
                break;
            default:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(rotation.Template.PaletteData!) == 0)
                {
                    BallRotation_SetCaptureSignal(rotation, 0x16);
                }
                break;
        }
        return 1;
    }

    // OFFSET 0x02236E08
    private static int BallRotation_CaptureLockDone(BallRotation rotation)
    {
        return 1;
    }

    // OFFSET 0x02236E0C
    private static int BallRotation_CaptureDim(BallRotation rotation)
    {
        switch (rotation.State)
        {
            case 0:
                BallRotation_FadeSprite(rotation, -5, 10, 0, 0);
                rotation.State++;
                break;
            case 1:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(rotation.Template.PaletteData!) == 0)
                {
                    rotation.State++;
                }
                break;
            default:
                BallRotation_SetCaptureSignal(rotation, 0x18);
                break;
        }
        return 1;
    }

    // OFFSET 0x02236E70
    private static int BallRotation_CaptureFadeOutStart(BallRotation rotation)
    {
        BallRotation_SetCaptureSignal(rotation, 0x19);
        return 1;
    }

    // OFFSET 0x02236E7C
    private static int BallRotation_CaptureFadeOut(BallRotation rotation)
    {
        switch (rotation.State)
        {
            case 0:
                SpriteSystem.ManagedSprite_SetExplicitOamMode(rotation.Sprite!, 1);
                rotation.State++;
                goto case 1;
            case 1:
                if (rotation.Alpha != 0)
                {
                    rotation.Alpha--;
                    rotation.AlphaStep++;
                }
                else
                {
                    rotation.Alpha = 0;
                    rotation.AlphaStep = 0xF;
                    SpriteSystem.ManagedSprite_SetDrawFlag(rotation.Sprite!, 0);
                    rotation.State++;
                }
                Video.SetBlendAlphaCoefficients(DisplayMain, (ushort)(rotation.Alpha | (rotation.AlphaStep << 8)));
                break;
            default:
                BallRotation_SetCaptureSignal(rotation, 0x1A);
                break;
        }
        return 1;
    }

    // OFFSET 0x02236F00
    private static int BallRotation_CaptureFadeOutDone(BallRotation rotation)
    {
        BallRotation_SetCaptureSignal(rotation, 0x1B);
        return 1;
    }

    // OFFSET 0x02236F0C
    private static int BallRotation_CaptureFinished(BallRotation rotation)
    {
        return 0;
    }

    // OFFSET 0x02236F10
    private static int BallRotation_CaptureMode(BallRotation rotation)
    {
        return CaptureSignalFuncs[rotation.Signal](rotation);
    }
}
