namespace PlatinumSharp.Game;

public sealed class ValueLerpContext
{
    public int Value;
    public readonly int[] Data = new int[5];
}

public sealed class AfterimageContext
{
    public const int MaxAfterimages = 4;

    public short X;
    public short Y;
    public Func<XYTransformContext, int>? TransformFunc;
    public ushort Delay;
    public ushort DelayCounter;
    public byte Count;
    public byte ActiveCount;
    public byte Mode;
    public readonly XYTransformContext[] Transforms = { new(), new(), new(), new() };
    public readonly ManagedSprite?[] Sprites = new ManagedSprite?[MaxAfterimages];
}

public sealed class AlphaFadeContext
{
    public readonly XYTransformContext Lerp = new();
    public int Done;
}

public sealed class VBlankDMAController
{
    public SysTask? OnVBlankTask;
    public SysTask? PostVBlankTask;
    public int DoDMA;
    public int SwapBuffers;
    public Action<object?>? DmaFunc;
    public Action<object?>? SwapBufferFunc;
    public object? Param;
}

public sealed class CustomBgScrollContext
{
    public readonly VBlankDMAController DmaController = new();
    public BufferManager? BufferManager;
    public readonly uint[] Buffer1 = new uint[192];
    public readonly uint[] Buffer2 = new uint[192];
    public uint OffsetReg;
}

public sealed class BgScrollContext
{
    public readonly VBlankDMAController DmaController = new();
    public ScreenScrollManager? ScreenScrollMgr;
}

public static partial class BattleAnimUtil
{
    private const int AnimPositionMonX = 0;
    private const int AnimPositionMonY = 1;
    private const int AnimPositionParticleX = 2;
    private const int AnimPositionParticleY = 3;
    private const int MonSpriteXCenter = 0;
    private const int MonSpriteYCenter = 1;
    private const int MonSpriteScaleX = 0xC;
    private const int MonSpriteScaleY = 0xD;
    private const int MonAffineScaleOne = 0x100;
    private const int MonSpriteFrameHeight = 0x50;
    private const int ShakeCounter = 0;
    private const int ShakeInterval = 1;
    private const int ShakeRemaining = 2;
    private const int ShakeCycle = 3;
    private const int ShakeExtentX = 4;
    private const int ShakeExtentY = 5;
    private const int ShakePrevX = 6;
    private const int ShakePrevY = 7;
    private const int MaxCyclesPerShake = 4;
    private const int XYSteps = 0;
    private const int XYStepSize = 1;
    private const int XYStepSizeX = 1;
    private const int XYStepSizeY = 2;
    private const int XYCurX = 3;
    private const int XYCurY = 4;
    private const int AnchorTop = 1;
    private const int AfterimageTransformPosition = 0;
    private const int BattlerSidePlayer = 3;
    private const int BattlerSideEnemy = 4;
    private const int BufferManagerDma32 = 1;

    private static int Modf(int value, out int integerPart)
    {
        integerPart = value >= 0 ? value & ~(Fx32.One - 1) : -((-value) & ~(Fx32.One - 1));
        return value - integerPart;
    }

    // OFFSET 0x02225898
    public static void BattleAnimUtil_GetBattlerDefaultPos2(BattleAnimSystem system, int battler, out short x, out short y)
    {
        if (BattleAnimSystems.BattleAnimSystem_IsContest(system) == 1)
        {
            BallRotations.BattleAnim_GetBattlerPosition(0, battler + 2, out x, out y);
        }
        else if (BattleAnimSystems.BattleAnimSystem_IsDoubleBattle(system) == 1)
        {
            BallRotations.BattleAnim_GetBattlerPosition(1, battler - 2, out x, out y);
        }
        else
        {
            BallRotations.BattleAnim_GetBattlerPosition(0, battler, out x, out y);
        }
    }

    // OFFSET 0x022258E0
    public static short BattleAnimUtil_GetBattlerPos(BattleAnimSystem system, int battler, int posType)
    {
        int battlerType = BattleAnimUtil_GetBattlerType(system, battler);
        int isDoubles = BattleAnimSystems.BattleAnimSystem_IsDoubleBattle(system) == 1 ? 1 : 0;
        if (BattleAnimSystems.BattleAnimSystem_IsContest(system) == 1)
        {
            battlerType += 2;
        }
        BallRotations.BattleAnim_GetBattlerPosition(isDoubles, battlerType, out short x, out short y);
        switch (posType)
        {
            case AnimPositionMonX:
            case AnimPositionParticleX:
                return x;
            case AnimPositionMonY:
            case AnimPositionParticleY:
                return y;
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x02225950
    public static byte BattleAnimUtil_GetSpritePalette(ManagedSprite sprite)
    {
        return (byte)PlttTransfer.PlttTransfer_GetPlttOffset(Sprite.Sprite_GetPaletteProxy(sprite.Sprite!), 1);
    }

    // OFFSET 0x02225964
    public static int BattleAnimUtil_GetTransformDirectionX(BattleAnimSystem system, int battler)
    {
        int dir = 1;
        int side = BattleAnimUtil_GetBattlerSide(system, battler);
        if (BattleAnimSystems.BattleAnimSystem_IsContest(system) != 0)
        {
            if (side == BattlerSidePlayer)
            {
                dir = -1;
            }
        }
        else if (side == BattlerSideEnemy)
        {
            dir = -1;
        }
        return dir;
    }

    // OFFSET 0x0222598C
    public static int BattleAnimUtil_GetTransformDirectionY(BattleAnimSystem system, int battler)
    {
        return BattleAnimUtil_GetBattlerSide(system, battler) == BattlerSideEnemy ? -1 : 1;
    }

    // OFFSET 0x022259FC
    public static void ScaleLerpContext_ApplyToSprite(XYTransformContext ctx, ManagedSprite sprite)
    {
        ScaleLerpContext_GetAffineScale(ctx, out float scaleX, out float scaleY);
        SpriteSystem.ManagedSprite_SetAffineScale(sprite, scaleX, scaleY);
    }

    // OFFSET 0x02225A18
    public static void XYTransformContext_ApplyPosOffsetToMon(XYTransformContext ctx, PokemonSprite sprite, short cx, short cy)
    {
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteXCenter, cx + ctx.X);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteYCenter, cy + ctx.Y);
    }

    // OFFSET 0x02225A3C
    public static void ScaleLerpContext_ApplyToMon(XYTransformContext ctx, PokemonSprite sprite)
    {
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteScaleX, ctx.X);
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteScaleY, ctx.Y);
    }

    // OFFSET 0x02225A8C
    public static void RevolutionContext_InitWithStepSize(XYTransformContext ctx, ushort sx, ushort ex, ushort sy, ushort ey, int rx, int ry, ushort stepSizeX)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (sx > ex)
        {
            stepSizeX = (ushort)-stepSizeX;
        }
        short stepSizeSigned = (short)stepSizeX;
        ctx!.Data[0] = BattleAnimUtil_CalcStepCount(sx * Fx32.One, ex * Fx32.One, stepSizeSigned * Fx32.One);
        ctx.Data[1] = sx;
        ctx.Data[2] = rx;
        ctx.Data[3] = sy;
        ctx.Data[4] = ry;
        ctx.Data[5] = stepSizeSigned;
        ctx.Data[6] = (ey - sy) / ctx.Data[0];
    }

    // OFFSET 0x02225B78
    public static int RevolutionContext_UpdateAndApplyToSprite(XYTransformContext ctx, short cx, short cy, ManagedSprite sprite)
    {
        if (RevolutionContext_Update(ctx) != 0)
        {
            XYTransformContext_ApplyPosOffsetToSprite(ctx, sprite, cx, cy);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02225BA0
    public static int RevolutionContext_UpdateAndApplyToMon(XYTransformContext ctx, short cx, short cy, PokemonSprite sprite)
    {
        if (RevolutionContext_Update(ctx) != 0)
        {
            XYTransformContext_ApplyPosOffsetToMon(ctx, sprite, cx, cy);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02225C74
    public static int PosLerpContext_UpdateAndApplyToMon(XYTransformContext ctx, PokemonSprite sprite)
    {
        if (PosLerpContext_Update(ctx) != 0)
        {
            XYTransformContext_ApplyPosOffsetToMon(ctx, sprite, 0, 0);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02225D50
    public static void ValueLerpContext_Init(ValueLerpContext ctx, int start, int end, int steps)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        ctx!.Value = start;
        ctx.Data[0] = steps;
        ctx.Data[1] = BattleAnimUtil_CalcStepSize(start * Fx32.One, end * Fx32.One, steps) >> Fx32.Shift;
    }

    // OFFSET 0x02225D78
    public static void ValueLerpContext_InitFX32(ValueLerpContext ctx, short start, short end, int steps)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        ctx!.Value = start;
        ctx.Data[0] = steps;
        ctx.Data[1] = BattleAnimUtil_CalcStepSize(start * Fx32.One, end * Fx32.One, steps);
        ctx.Data[2] = start * Fx32.One;
    }

    // OFFSET 0x02225DA0
    public static int ValueLerpContext_Update(ValueLerpContext ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (ctx!.Data[0] != 0)
        {
            ctx.Value += ctx.Data[1];
            ctx.Data[0]--;
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02225DC8
    public static int ValueLerpContext_UpdateFX32(ValueLerpContext ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (ctx!.Data[0] != 0)
        {
            ctx.Data[2] += ctx.Data[1];
            ctx.Value = ctx.Data[2] >> Fx32.Shift;
            ctx.Data[0]--;
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02225DF4
    public static void ValueLerpContext_InitSinusoidal(ValueLerpContext ctx, ushort start, ushort end, int amplitude, int steps)
    {
        ctx.Data[0] = steps;
        ctx.Data[1] = start;
        ctx.Data[2] = amplitude;
        ctx.Data[3] = (end - start) / steps;
    }

    // OFFSET 0x02225E0C
    public static int ValueLerpContext_UpdateSinusoidal(ValueLerpContext ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (ctx!.Data[0] != 0)
        {
            ctx.Data[1] += ctx.Data[3];
            ctx.Data[1] &= 0xFFFF;
            ctx.Data[0]--;
            ctx.Value = Fx32.Mul(Render3D.Cos(ctx.Data[1] >> 4), ctx.Data[2]) >> Fx32.Shift;
            return 1;
        }
        return 0;
    }

    private static int RelativeScale(int scale, int reference)
    {
        return (scale << 8) / reference;
    }

    // OFFSET 0x02225E68
    public static void ScaleLerpContext_Init(XYTransformContext ctx, short startScale, short refScale, short endScale, int steps)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        ctx!.Data[XYSteps] = steps;
        int start = RelativeScale(startScale, refScale);
        int end = RelativeScale(endScale, refScale);
        ctx.Data[XYStepSize] = BattleAnimUtil_CalcStepSize(start << 12, end << 12, steps);
        ctx.X = (short)start;
        ctx.Y = (short)start;
        ctx.Data[XYCurX] = ctx.X << 12;
        ctx.Data[XYCurY] = ctx.Y << 12;
    }

    // OFFSET 0x02225EB8
    public static int ScaleLerpContext_Update(XYTransformContext ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (ctx!.Data[XYSteps] != 0)
        {
            ctx.Data[XYSteps]--;
            ctx.Data[XYCurX] += ctx.Data[XYStepSize];
            ctx.Data[XYCurY] += ctx.Data[XYStepSize];
            ctx.X = (short)(ctx.Data[XYCurX] >> Fx32.Shift);
            ctx.Y = (short)(ctx.Data[XYCurY] >> Fx32.Shift);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02225EF0
    public static void ScaleLerpContext_InitXY(XYTransformContext ctx, short sx, short ex, short sy, short ey, short reference, int steps)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        ctx!.Data[XYSteps] = steps;
        ctx.Data[XYStepSizeX] = BattleAnimUtil_CalcStepSize(RelativeScale(sx, reference) << 12, RelativeScale(ex, reference) << 12, steps);
        ctx.Data[XYStepSizeY] = BattleAnimUtil_CalcStepSize(RelativeScale(sy, reference) << 12, RelativeScale(ey, reference) << 12, steps);
        ctx.X = (short)RelativeScale(sx, reference);
        ctx.Y = (short)RelativeScale(sy, reference);
        ctx.Data[XYCurX] = ctx.X << 12;
        ctx.Data[XYCurY] = ctx.Y << 12;
    }

    // OFFSET 0x02225F6C
    public static int ScaleLerpContext_UpdateXY(XYTransformContext ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (ctx!.Data[XYSteps] != 0)
        {
            ctx.Data[XYSteps]--;
            ctx.Data[XYCurX] += ctx.Data[XYStepSizeX];
            ctx.Data[XYCurY] += ctx.Data[XYStepSizeY];
            ctx.X = (short)(ctx.Data[XYCurX] >> Fx32.Shift);
            ctx.Y = (short)(ctx.Data[XYCurY] >> Fx32.Shift);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02225FA4
    public static void ScaleLerpContext_GetAffineScale(XYTransformContext ctx, out float x, out float y)
    {
        x = ctx.X / 256.0f;
        y = ctx.Y / 256.0f;
    }

    // OFFSET 0x02225FD4
    public static short BattleAnimUtil_GetGroundAnchoredScaleOffset(short baseY, int height, int scaleY)
    {
        int availableHeight = (MonSpriteFrameHeight - height * 2) << 12;
        int scaled = Fx32.Mul(availableHeight, scaleY);
        int relative = Fx32.Div(scaled, MonAffineScaleOne << 12);
        int diff = availableHeight - relative;
        int fraction = Modf(diff, out int integerPart);
        if (fraction != 0)
        {
            fraction += Fx32.One / 2;
        }
        diff = integerPart + fraction;
        return (short)((diff >> Fx32.Shift) / 2);
    }

    // OFFSET 0x02226024
    public static void BattleAnimUtil_SetPokemonSpriteAnchoredPosition(PokemonSprite sprite, short y, short height, int scale, int anchor)
    {
        short offset = BattleAnimUtil_GetGroundAnchoredScaleOffset(y, height, scale);
        if (anchor == AnchorTop)
        {
            offset = (short)-offset;
            y = (short)(y - height);
        }
        PokemonSprite.PokemonSprite_SetAttribute(sprite, MonSpriteYCenter, y + offset);
    }

    // OFFSET 0x0222605C
    public static void BattleAnimUtil_SetSpriteAnchoredPosition(ManagedSprite sprite, short y, short height, int scale, int anchor)
    {
        short offset = BattleAnimUtil_GetGroundAnchoredScaleOffset(y, height, scale);
        if (anchor == AnchorTop)
        {
            offset = (short)-offset;
            y = (short)(y - height);
        }
        SpriteSystem.ManagedSprite_GetPositionXY(sprite, out short curX, out _);
        SpriteSystem.ManagedSprite_SetPositionXY(sprite, curX, (short)(y + offset));
    }

    // OFFSET 0x022260A8
    public static int ScaleLerpContext_UpdateAndApplyToSprite(XYTransformContext ctx, ManagedSprite sprite)
    {
        if (ScaleLerpContext_Update(ctx) != 0)
        {
            ScaleLerpContext_ApplyToSprite(ctx, sprite);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x022260C8
    public static int ScaleLerpContext_UpdateAndApplyToMon(XYTransformContext ctx, PokemonSprite sprite)
    {
        if (ScaleLerpContext_Update(ctx) != 0)
        {
            ScaleLerpContext_ApplyToMon(ctx, sprite);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x022260E8
    public static int ScaleLerpContext_UpdateXYAndApplyToMon(XYTransformContext ctx, PokemonSprite sprite)
    {
        if (ScaleLerpContext_UpdateXY(ctx) != 0)
        {
            ScaleLerpContext_ApplyToMon(ctx, sprite);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02226108
    public static void ShakeContext_Init(XYTransformContext ctx, short extentX, short extentY, int interval, int amount)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        ctx!.Data[ShakeCounter] = interval;
        ctx.Data[ShakeInterval] = interval;
        ctx.Data[ShakeRemaining] = amount;
        ctx.Data[ShakeCycle] = 0;
        ctx.Data[ShakeExtentX] = extentX;
        ctx.Data[ShakeExtentY] = extentY;
        ctx.Data[ShakePrevX] = -extentX;
        ctx.Data[ShakePrevY] = -extentY;
        ctx.X = 0;
        ctx.Y = 0;
    }

    private static short ShakeFlip(short cur, ref int prev)
    {
        int prevVal = prev;
        prev = cur;
        return prevVal == 0 ? (short)0 : (short)-prevVal;
    }

    // OFFSET 0x02226138
    public static int ShakeContext_Update(XYTransformContext ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (ctx!.Data[ShakeRemaining] == 0)
        {
            return 0;
        }
        ctx.Data[ShakeCounter]++;
        if (ctx.Data[ShakeCounter] >= ctx.Data[ShakeInterval])
        {
            ctx.Data[ShakeCounter] = 0;
            ctx.X = ShakeFlip(ctx.X, ref ctx.Data[ShakePrevX]);
            ctx.Y = ShakeFlip(ctx.Y, ref ctx.Data[ShakePrevY]);
            if (++ctx.Data[ShakeCycle] >= MaxCyclesPerShake)
            {
                ctx.Data[ShakeCycle] = 0;
                ctx.Data[ShakeRemaining]--;
            }
        }
        return 1;
    }

    // OFFSET 0x0222619C
    public static int ShakeContext_UpdateAndApplyToMon(XYTransformContext ctx, short cx, short cy, PokemonSprite sprite)
    {
        if (ShakeContext_Update(ctx) != 0)
        {
            XYTransformContext_ApplyPosOffsetToMon(ctx, sprite, cx, cy);
            return 1;
        }
        return 0;
    }

    private static void CopyTransform(XYTransformContext source, XYTransformContext dest)
    {
        dest.X = source.X;
        dest.Y = source.Y;
        Array.Copy(source.Data, dest.Data, XYTransformContext.DataCount);
    }

    // OFFSET 0x022261C4
    public static void Afterimage_Init(AfterimageContext ctx, XYTransformContext transformCtx, Func<XYTransformContext, int> transformFunc, short x, short y, ushort delay, byte count, byte mode, ManagedSprite? sprite0, ManagedSprite? sprite1, ManagedSprite? sprite2, ManagedSprite? sprite3)
    {
        ctx.X = x;
        ctx.Y = y;
        ctx.TransformFunc = transformFunc;
        ctx.Delay = delay;
        ctx.DelayCounter = 0;
        ctx.Count = count;
        ctx.ActiveCount = 0;
        ctx.Mode = mode;
        for (int i = 0; i < count; i++)
        {
            CopyTransform(transformCtx, ctx.Transforms[i]);
        }
        ctx.Sprites[0] = sprite0;
        ctx.Sprites[1] = sprite1;
        ctx.Sprites[2] = sprite2;
        ctx.Sprites[3] = sprite3;
        for (int i = 0; i < ctx.Count; i++)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(ctx.Sprites[i]!, 0);
        }
    }

    // OFFSET 0x02226274
    public static int Afterimage_Update(AfterimageContext ctx)
    {
        int[] active = new int[AfterimageContext.MaxAfterimages];
        for (int i = 0; i < AfterimageContext.MaxAfterimages; i++)
        {
            active[i] = 1;
        }
        if (ctx.Delay <= ctx.DelayCounter)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(ctx.Sprites[ctx.ActiveCount]!, 1);
            ctx.ActiveCount++;
            ctx.DelayCounter = 0;
        }
        if (ctx.ActiveCount < ctx.Count)
        {
            ctx.DelayCounter++;
        }
        for (int i = 0; i < ctx.ActiveCount; i++)
        {
            active[i] = ctx.TransformFunc!(ctx.Transforms[i]);
            if (active[i] != 0)
            {
                if (ctx.Mode == AfterimageTransformPosition)
                {
                    SpriteSystem.ManagedSprite_SetPositionXY(ctx.Sprites[i]!, (short)(ctx.X + ctx.Transforms[i].X), (short)(ctx.Y + ctx.Transforms[i].Y));
                }
                else
                {
                    ScaleLerpContext_GetAffineScale(ctx.Transforms[i], out float sx, out float sy);
                    SpriteSystem.ManagedSprite_SetAffineScale(ctx.Sprites[i]!, sx, sy);
                }
            }
            else
            {
                SpriteSystem.ManagedSprite_SetDrawFlag(ctx.Sprites[i]!, 0);
            }
        }
        for (int i = 0; i < ctx.Count; i++)
        {
            if (active[i] == 1)
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x022263A4
    public static void RevolutionContext_InitOvalRevolutions(XYTransformContext ctx, int revs, int stepsPerRev)
    {
        RevolutionContext_Init(ctx, 0, 0xFFFF, 0, 0xFFFF, 0x20000, -0x8000, stepsPerRev);
        ctx.Data[0] *= revs;
    }

    // OFFSET 0x022263DC
    private static void AlphaFadeContext_Update(SysTask task, object? param)
    {
        AlphaFadeContext ctx = (AlphaFadeContext)param!;
        if (PosLerpContext_Update(ctx.Lerp) == 0)
        {
            ctx.Done = 1;
            SysTask.SysTask_Done(task);
            return;
        }
        if (ctx.Lerp.X < 0)
        {
            ctx.Lerp.X = 0;
        }
        if (ctx.Lerp.Y < 0)
        {
            ctx.Lerp.Y = 0;
        }
        Video.SetBlendAlphaCoefficients(0, (ushort)((ctx.Lerp.Y << 8) | ctx.Lerp.X));
    }

    // OFFSET 0x02226424
    public static void AlphaFadeContext_Init(AlphaFadeContext ctx, short startEv1, short endEv1, short startEv2, short endEv2, int steps)
    {
        PosLerpContext_Init(ctx.Lerp, startEv1, endEv1, startEv2, endEv2, (ushort)steps);
        ctx.Done = 0;
        SysTask.SysTask_Start(AlphaFadeContext_Update, ctx, 0);
    }

    // OFFSET 0x02226454
    public static int AlphaFadeContext_IsDone(AlphaFadeContext ctx)
    {
        return ctx.Done;
    }

    // OFFSET 0x02226458
    private static void VBlankDMAController_PostVBlank(SysTask task, object? param)
    {
        VBlankDMAController dmaController = (VBlankDMAController)param!;
        if (dmaController.DoDMA != 0)
        {
            dmaController.SwapBuffers = 1;
            dmaController.DmaFunc!(dmaController.Param);
        }
        else
        {
            BufferManager.BufferManager_StopDMA();
        }
    }

    // OFFSET 0x02226474
    private static void VBlankDMAController_OnVBlank(SysTask task, object? param)
    {
        VBlankDMAController dmaController = (VBlankDMAController)param!;
        if (dmaController.DoDMA != 0 && dmaController.SwapBuffers != 0)
        {
            dmaController.SwapBufferFunc!(dmaController.Param);
            dmaController.SwapBuffers = 0;
        }
    }

    // OFFSET 0x02226490
    private static void VBlankDMAController_Init(VBlankDMAController dmaController, object? param, Action<object?> dmaFunc, Action<object?> swapBuffersFunc)
    {
        if (dmaController == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        dmaController!.DoDMA = 1;
        dmaController.SwapBuffers = 0;
        dmaController.Param = param;
        dmaController.DmaFunc = dmaFunc;
        dmaController.SwapBufferFunc = swapBuffersFunc;
        dmaController.PostVBlankTask = SysTask.SysTask_ExecuteAfterVBlank(VBlankDMAController_PostVBlank, dmaController, 0);
        dmaController.OnVBlankTask = SysTask.SysTask_ExecuteOnVBlank(VBlankDMAController_OnVBlank, dmaController, 0);
    }

    // OFFSET 0x022264D0
    private static void VBlankDMAController_Deinit(VBlankDMAController dmaController)
    {
        if (dmaController == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (dmaController!.PostVBlankTask != null)
        {
            SysTask.SysTask_Done(dmaController.PostVBlankTask);
        }
        if (dmaController.OnVBlankTask != null)
        {
            SysTask.SysTask_Done(dmaController.OnVBlankTask);
        }
        BufferManager.BufferManager_StopDMA();
    }

    // OFFSET 0x022264F4
    private static void VBlankDMAController_DisableDMA(VBlankDMAController dmaController)
    {
        if (dmaController == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        dmaController!.DoDMA = 0;
    }

    // OFFSET 0x02226504
    private static void CustomBgScrollContext_DoDMAImpl(CustomBgScrollContext ctx)
    {
        Array buffer = BufferManager.BufferManager_GetReadBuffer(ctx.BufferManager)!;
        BufferManager.BufferManager_StopDMA();
        BufferManager.BufferManager_StartDMA(buffer, ctx.OffsetReg, 4, BufferManagerDma32);
    }

    // OFFSET 0x02226528
    private static void CustomBgScrollContext_SwapBuffers(object? param)
    {
        CustomBgScrollContext ctx = (CustomBgScrollContext)param!;
        BufferManager.BufferManager_SwapBuffers(ctx.BufferManager);
        CustomBgScrollContext_DoDMAImpl(ctx);
    }

    // OFFSET 0x0222653C
    private static void CustomBgScrollContext_DoDMA(object? param)
    {
        CustomBgScrollContext_DoDMAImpl((CustomBgScrollContext)param!);
    }

    // OFFSET 0x02226544
    public static CustomBgScrollContext CustomBgScrollContext_New(uint offsetReg, uint initValue, int heapID)
    {
        CustomBgScrollContext ctx = new();
        ctx.BufferManager = BufferManager.BufferManager_New(heapID, ctx.Buffer1, ctx.Buffer2);
        if (ctx.BufferManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        ctx.OffsetReg = offsetReg;
        Array.Fill(ctx.Buffer1, initValue);
        Array.Fill(ctx.Buffer2, initValue);
        VBlankDMAController_Init(ctx.DmaController, ctx, CustomBgScrollContext_SwapBuffers, CustomBgScrollContext_DoDMA);
        return ctx;
    }

    // OFFSET 0x022265C0
    public static void CustomBgScrollContext_Free(CustomBgScrollContext ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        VBlankDMAController_Deinit(ctx!.DmaController);
        if (ctx.BufferManager != null)
        {
            BufferManager.BufferManager_Delete(ctx.BufferManager);
        }
    }

    // OFFSET 0x022265E4
    public static uint[] CustomBgScrollContext_GetWriteBuffer(CustomBgScrollContext ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return (uint[])BufferManager.BufferManager_GetWriteBuffer(ctx!.BufferManager)!;
    }

    // OFFSET 0x022265F8
    public static void CustomBgScrollContext_Stop(CustomBgScrollContext ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        VBlankDMAController_DisableDMA(ctx!.DmaController);
    }

    // OFFSET 0x0222660C
    private static void BgScrollContext_SwapBuffers(object? param)
    {
        BgScrollContext ctx = (BgScrollContext)param!;
        ScreenScrollManager.ScreenScrollManager_SwapBuffers(ctx.ScreenScrollMgr);
        ScreenScrollManager.ScreenScrollManager_RestartDMA(ctx.ScreenScrollMgr);
    }

    // OFFSET 0x02226620
    private static void BgScrollContext_DoDMA(object? param)
    {
        BgScrollContext ctx = (BgScrollContext)param!;
        ScreenScrollManager.ScreenScrollManager_RestartDMA(ctx.ScreenScrollMgr);
    }

    // OFFSET 0x0222662C
    public static BgScrollContext BgScrollContext_New(byte startY, byte endY, ushort angleIncrement, int amplitude, short speed, int bgID, uint priority, uint initValue, int heapID)
    {
        BgScrollContext ctx = new();
        uint offsetReg = BattleAnimUtil_GetHOffsetRegisterForBg(bgID);
        ctx.ScreenScrollMgr = ScreenScrollManager.ScreenScrollManager_New(heapID);
        ScreenScrollManager.ScreenScrollManager_ScrollX(ctx.ScreenScrollMgr, startY, endY, angleIncrement, amplitude, speed, offsetReg, initValue, priority);
        VBlankDMAController_Init(ctx.DmaController, ctx, BgScrollContext_SwapBuffers, BgScrollContext_DoDMA);
        return ctx;
    }

    // OFFSET 0x0222669C
    public static void BgScrollContext_Free(BgScrollContext ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        VBlankDMAController_Deinit(ctx!.DmaController);
        if (ctx.ScreenScrollMgr != null)
        {
            ScreenScrollManager.ScreenScrollManager_Delete(ctx.ScreenScrollMgr);
        }
    }

    // OFFSET 0x022266C0
    public static uint[] BgScrollContext_GetWriteBuffer(BgScrollContext ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return ScreenScrollManager.ScreenScrollManager_GetWriteBuffer(ctx!.ScreenScrollMgr!);
    }

    // OFFSET 0x022266D4
    public static void BgScrollContext_Stop(BgScrollContext ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        VBlankDMAController_DisableDMA(ctx!.DmaController);
    }

    // OFFSET 0x022266E8
    public static int BattleAnimUtil_MakeBgOffsetValue(ushort x, ushort y)
    {
        return (y << 16) | x;
    }

    // OFFSET 0x022266F0
    public static uint BattleAnimUtil_GetHOffsetRegisterForBg(int bgID)
    {
        switch (bgID)
        {
            case 0:
                return 0x04000010;
            case 1:
                return 0x04000014;
            case 2:
                return 0x04000018;
            case 3:
                return 0x0400001C;
        }
        return 0;
    }

    // OFFSET 0x02226728
    public static void BattleAnimUtil_GetXYMean(short x1, short y1, short x2, short y2, out short mx, out short my)
    {
        mx = (short)((x1 + x2) / 2);
        my = (short)((y1 + y2) / 2);
    }

    // OFFSET 0x02226744
    public static void BattleAnimUtil_DistanceBetween(short x1, short y1, short x2, short y2, out int distance)
    {
        short dx = (short)(x1 - x2);
        short dy = (short)-(y1 - y2);
        distance = Fx32.Sqrt((dy * dy + dx * dx) * Fx32.One);
    }

    // OFFSET 0x0222676C
    public static void BattleAnimUtil_CalcAngleFromPoints(short x1, short y1, short x2, short y2, out ushort angle)
    {
        short dx = (short)(x1 - x2);
        short ndy = (short)-(y1 - y2);
        angle = Mtx.Atan2Idx(ndy * Fx32.One, dx * Fx32.One);
        if (angle > 0 && ndy < 0)
        {
            angle = (ushort)((angle - 0x8000) * 0xFFFF);
        }
    }

    // OFFSET 0x022267A8
    public static int BattleAnimUtil_StepValue(ref int value, int target, int step)
    {
        if (step < 0)
        {
            if (value + step > target)
            {
                value += step;
                return 0;
            }
        }
        else if (value + step < target)
        {
            value += step;
            return 0;
        }
        value = target;
        return 1;
    }

    // OFFSET 0x022268DC
    private static void BattleAnimUtil_ConvertColorsToGrayscale(ushort[] colors, int colorCount)
    {
        for (int i = 0; i < colorCount; i++)
        {
            int r = colors[i] & 0x1F;
            int g = (colors[i] >> 5) & 0x1F;
            int b = (colors[i] >> 10) & 0x1F;
            int y = (r * 0x4C + g * 0x97 + b * 0x1D) >> 8;
            colors[i] = (ushort)((y << 10) | (y << 5) | y);
        }
    }

    // OFFSET 0x02226924
    public static void BattleAnimUtil_MakeBgPalsGrayscale(BattleAnimSystem system)
    {
        PaletteData paletteData = BattleAnimSystems.BattleAnimSystem_GetPaletteData(system);
        ushort[] buffer = PaletteData.PaletteData_GetFadedBuffer(paletteData, 0)!;
        if (BattleAnimSystems.BattleAnimSystem_IsContest(system) == 1)
        {
            BattleAnimUtil_ConvertColorsToGrayscale(buffer, 0x30);
        }
        else
        {
            BattleAnimUtil_ConvertColorsToGrayscale(buffer, 0x80);
        }
    }

    // OFFSET 0x02226954
    public static void BattleAnimUtil_ReturnBgPalsToNormal(BattleAnimSystem system)
    {
        PaletteData paletteData = BattleAnimSystems.BattleAnimSystem_GetPaletteData(system);
        if (BattleAnimSystems.BattleAnimSystem_IsContest(system) == 1)
        {
            PaletteData.PaletteData_CopyBuffer(paletteData, 0, 0, 0, 0, 0x60);
        }
        else
        {
            PaletteData.PaletteData_CopyBuffer(paletteData, 0, 0, 0, 0, 0x100);
        }
    }

    // OFFSET 0x02235254
    public static int BattleAnimUtil_GetBattlerType(BattleAnimSystem system, int battler)
    {
        return BattleAnimSystems.BattleAnimSystem_GetBattlerType(system, battler);
    }

    // OFFSET 0x0223525C
    public static int BattleAnimUtil_GetBattlerSide(BattleAnimSystem system, int battler)
    {
        int side = 0;
        switch (BattleAnimSystems.BattleAnimSystem_GetBattlerType(system, battler))
        {
            case 0:
            case 2:
            case 4:
                side = BattlerSidePlayer;
                break;
            case 1:
            case 3:
            case 5:
                side = BattlerSideEnemy;
                break;
        }
        return side;
    }

    // OFFSET 0x02235288
    public static int BattleAnimUtil_GetBattlerOfType(BattleAnimSystem system, int type)
    {
        for (int i = 0; i < 4; i++)
        {
            if (BattleAnimSystems.BattleAnimSystem_GetBattlerType(system, i) == type)
            {
                return i;
            }
        }
        return 0;
    }

    // OFFSET 0x022352AC
    public static int BattleAnimUtil_GetAlliedBattler(BattleAnimSystem system, int battler)
    {
        int battlerType = BattleAnimSystems.BattleAnimSystem_GetBattlerType(system, battler);
        if (battlerType <= 1)
        {
            return battlerType;
        }
        for (int i = 0; i < 4; i++)
        {
            int otherType = BattleAnimSystems.BattleAnimSystem_GetBattlerType(system, i);
            switch (battlerType)
            {
                case 2:
                    if (otherType == 4)
                    {
                        return i;
                    }
                    break;
                case 3:
                    if (otherType == 5)
                    {
                        return i;
                    }
                    break;
                case 4:
                    if (otherType == 2)
                    {
                        return i;
                    }
                    break;
                case 5:
                    if (otherType == 3)
                    {
                        return i;
                    }
                    break;
            }
        }
        ErrorHandling.GF_AssertFail();
        return battlerType;
    }
}
