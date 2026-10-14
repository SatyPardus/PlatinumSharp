namespace PlatinumSharp.Game;

public sealed class XYTransformContext
{
    public const int DataCount = 8;

    public short X;
    public short Y;
    public readonly int[] Data = new int[DataCount];
}

public sealed class PaletteFadeContext
{
    public int Active;
    public SysTask? Task;
    public int BufferID;
    public ushort Offset;
    public ushort Count;
    public ushort Color;
    public byte StartFraction;
    public byte EndFraction;
    public byte CurFraction;
    public sbyte Step;
    public sbyte Wait;
    public sbyte WaitCounter;
    public PaletteData? PaletteData;
}

public static partial class BattleAnimUtil
{
    private const int LinearSteps = 0;
    private const int LinearStepX = 1;
    private const int LinearStepY = 2;
    private const int LinearX = 3;
    private const int LinearY = 4;
    private const int RevolutionSteps = 0;
    private const int RevolutionAngleX = 1;
    private const int RevolutionRadiusX = 2;
    private const int RevolutionAngleY = 3;
    private const int RevolutionRadiusY = 4;
    private const int RevolutionStepX = 5;
    private const int RevolutionStepY = 6;
    private const int FxShift = 12;
    private const int FxOne = 0x1000;
    private const int ParabolaStartAngle = 0x3FFF;
    private const int ParabolaEndAngle = 0xBFFF;

    // OFFSET 0x022259A0
    public static int BattleAnimUtil_CalcStepSize(int start, int end, int steps)
    {
        return Fx32.Div(end - start, steps << FxShift);
    }

    // OFFSET 0x022259AC
    public static int BattleAnimUtil_CalcStepCount(int start, int end, int stepSize)
    {
        int count = Fx32.Div(end - start, stepSize);
        if ((count & (FxOne - 1)) != 0)
        {
            count += FxOne;
        }
        if (count < 0)
        {
            count = -count;
        }
        return count >> FxShift;
    }

    // OFFSET 0x022259DC
    public static void XYTransformContext_ApplyPosOffsetToSprite(XYTransformContext ctx, ManagedSprite sprite, short baseX, short baseY)
    {
        SpriteSystem.ManagedSprite_SetPositionXY(sprite, (short)(baseX + ctx.X), (short)(baseY + ctx.Y));
    }

    // OFFSET 0x02225A5C
    public static void RevolutionContext_Init(XYTransformContext ctx, int startAngleX, int endAngleX, int startAngleY, ushort endAngleY, int radiusX, int radiusY, int steps)
    {
        ctx.Data[RevolutionSteps] = steps;
        ctx.Data[RevolutionAngleX] = startAngleX;
        ctx.Data[RevolutionRadiusX] = radiusX;
        ctx.Data[RevolutionAngleY] = startAngleY;
        ctx.Data[RevolutionRadiusY] = radiusY;
        ctx.Data[RevolutionStepX] = (endAngleX - startAngleX) / steps;
        ctx.Data[RevolutionStepY] = (endAngleY - startAngleY) / steps;
    }

    // OFFSET 0x02225AE0
    public static int RevolutionContext_Update(XYTransformContext? ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (ctx!.Data[RevolutionSteps] == 0)
        {
            return 0;
        }
        ctx.Data[RevolutionAngleX] += ctx.Data[RevolutionStepX];
        ctx.Data[RevolutionAngleY] += ctx.Data[RevolutionStepY];
        ctx.Data[RevolutionAngleX] &= 0xFFFF;
        ctx.Data[RevolutionAngleY] &= 0xFFFF;
        ctx.Data[RevolutionSteps]--;
        ctx.X = (short)(Fx32.Mul(Render3D.Sin(ctx.Data[RevolutionAngleX] >> 4), ctx.Data[RevolutionRadiusX]) >> FxShift);
        ctx.Y = (short)(Fx32.Mul(Render3D.Cos(ctx.Data[RevolutionAngleY] >> 4), ctx.Data[RevolutionRadiusY]) >> FxShift);
        return 1;
    }

    // OFFSET 0x02225BC8
    public static void PosLerpContext_Init(XYTransformContext? ctx, short startX, short endX, short startY, short endY, ushort steps)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        ctx!.X = startX;
        ctx.Y = startY;
        ctx.Data[LinearSteps] = steps;
        ctx.Data[LinearStepX] = BattleAnimUtil_CalcStepSize(startX << FxShift, endX << FxShift, steps);
        ctx.Data[LinearStepY] = BattleAnimUtil_CalcStepSize(startY << FxShift, endY << FxShift, steps);
        ctx.Data[LinearX] = startX << FxShift;
        ctx.Data[LinearY] = startY << FxShift;
    }

    // OFFSET 0x02225C14
    public static int PosLerpContext_Update(XYTransformContext? ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (ctx!.Data[LinearSteps] == 0)
        {
            return 0;
        }
        ctx.Data[LinearX] += ctx.Data[LinearStepX];
        ctx.Data[LinearY] += ctx.Data[LinearStepY];
        ctx.X = (short)(ctx.Data[LinearX] >> FxShift);
        ctx.Y = (short)(ctx.Data[LinearY] >> FxShift);
        ctx.Data[LinearSteps]--;
        return 1;
    }

    // OFFSET 0x02225C50
    public static int PosLerpContext_UpdateAndApplyToSprite(XYTransformContext ctx, ManagedSprite sprite)
    {
        if (PosLerpContext_Update(ctx) != 0)
        {
            XYTransformContext_ApplyPosOffsetToSprite(ctx, sprite, 0, 0);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02225C98
    public static void ParabolicContext_Init(XYTransformContext linear, XYTransformContext revolution, short startX, short endX, short startY, short endY, ushort steps, int height)
    {
        PosLerpContext_Init(linear, startX, endX, startY, endY, steps);
        revolution.X = 0;
        revolution.Y = 0;
        RevolutionContext_Init(revolution, 0, 0, ParabolaStartAngle, ParabolaEndAngle, 0, height, steps);
    }

    // OFFSET 0x02225CE4
    public static int ParabolicContext_Update(XYTransformContext? linear, XYTransformContext? revolution)
    {
        if (linear == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (revolution == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        int linearActive = PosLerpContext_Update(linear);
        int revolutionActive = RevolutionContext_Update(revolution);
        linear!.X += revolution!.X;
        linear.Y += revolution.Y;
        if (linearActive == revolutionActive && linearActive == 0)
        {
            return 0;
        }
        return 1;
    }

    // OFFSET 0x02225D2C
    public static int ParabolicContext_UpdateAndApplyToSprite(XYTransformContext linear, XYTransformContext revolution, ManagedSprite sprite)
    {
        if (ParabolicContext_Update(linear, revolution) != 0)
        {
            XYTransformContext_ApplyPosOffsetToSprite(linear, sprite, 0, 0);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x022267D4
    private static void PaletteFadeContext_Task(SysTask task, object? param)
    {
        PaletteFadeContext ctx = (PaletteFadeContext)param!;
        if (ctx.Active == 0)
        {
            return;
        }
        ctx.WaitCounter++;
        if (ctx.WaitCounter < ctx.Wait)
        {
            return;
        }
        ctx.WaitCounter = 0;
        PaletteData.PaletteData_Blend(ctx.PaletteData!, ctx.BufferID, ctx.Offset, ctx.Count, ctx.CurFraction, ctx.Color);
        if (ctx.CurFraction == ctx.EndFraction)
        {
            ctx.Active = 0;
            return;
        }
        sbyte next = (sbyte)(ctx.CurFraction + ctx.Step);
        if (ctx.Step > 0)
        {
            if (next > ctx.EndFraction)
            {
                ctx.CurFraction = ctx.EndFraction;
            }
            else
            {
                ctx.CurFraction = (byte)(ctx.CurFraction + ctx.Step);
            }
        }
        else if (next < ctx.EndFraction)
        {
            ctx.CurFraction = ctx.EndFraction;
        }
        else
        {
            ctx.CurFraction = (byte)(ctx.CurFraction + ctx.Step);
        }
    }

    // OFFSET 0x02226848
    public static int PaletteFadeContext_IsActive(PaletteFadeContext? ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return ctx!.Active;
    }

    // OFFSET 0x02226858
    public static void PaletteFadeContext_Free(PaletteFadeContext? ctx)
    {
        if (ctx == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SysTask.SysTask_Done(ctx!.Task!);
    }

    // OFFSET 0x02226870
    public static PaletteFadeContext PaletteFadeContext_New(PaletteData paletteData, int heapID, int bufferID, ushort offset, ushort count, sbyte wait, sbyte step, byte startFraction, byte endFraction, ushort color, uint taskPriority)
    {
        PaletteFadeContext ctx = new();
        ctx.PaletteData = paletteData;
        ctx.BufferID = bufferID;
        ctx.Offset = offset;
        ctx.Count = count;
        ctx.Color = color;
        ctx.StartFraction = startFraction;
        ctx.EndFraction = endFraction;
        ctx.CurFraction = startFraction;
        ctx.Wait = wait;
        ctx.WaitCounter = wait;
        if (ctx.StartFraction < ctx.EndFraction)
        {
            ctx.Step = step;
        }
        else
        {
            ctx.Step = (sbyte)-step;
        }
        ctx.Active = 1;
        ctx.Task = SysTask.SysTask_Start(PaletteFadeContext_Task, ctx, taskPriority);
        return ctx;
    }
}
