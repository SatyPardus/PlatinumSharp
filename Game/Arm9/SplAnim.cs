namespace PlatinumSharp.Game;

public static class SplAnim
{
    private const int MaxLifeRate = 0xFF;

    private static ushort PackColor(int red, int green, int blue)
    {
        return (ushort)(red | (green << 5) | (blue << 10));
    }

    // OFFSET 0x020A19F0
    public static void SPLAnim_ChildAlpha(SplParticle particle, SplResource resource, int lifeRate)
    {
        particle.AnimAlpha = ((MaxLifeRate - lifeRate) * 31) / MaxLifeRate;
    }

    // OFFSET 0x020A1A48
    public static void SPLAnim_ChildScale(SplParticle particle, SplResource resource, int lifeRate)
    {
        int endScale = resource.ChildResource!.EndScale;
        particle.AnimScale = (short)(endScale + ((endScale - Fx32.One) * (lifeRate - MaxLifeRate)) / MaxLifeRate);
    }

    // OFFSET 0x020A1A94
    public static void SPLAnim_Texture(SplParticle particle, SplResource resource, int lifeRate)
    {
        SplTexAnim texAnim = resource.TexAnim!;
        int frameCount = texAnim.FrameCount;
        int step = texAnim.Step;
        int threshold = 0;
        for (int i = 0; i < frameCount; i++)
        {
            if (lifeRate < threshold + step)
            {
                particle.Texture = texAnim.Textures[i];
                return;
            }
            threshold += step;
        }
    }

    // OFFSET 0x020A1AF8
    public static void SPLAnim_Alpha(SplParticle particle, SplResource resource, int lifeRate)
    {
        SplAlphaAnim alphaAnim = resource.AlphaAnim!;
        int curveIn = alphaAnim.CurveIn;
        int curveOut = alphaAnim.CurveOut;
        int alpha;
        if (lifeRate < curveIn)
        {
            int start = alphaAnim.Start;
            alpha = SplMath.Div(lifeRate * (alphaAnim.Mid - start), curveIn) + start;
        }
        else if (lifeRate < curveOut)
        {
            alpha = alphaAnim.Mid;
        }
        else
        {
            int end = alphaAnim.End;
            alpha = SplMath.Div((lifeRate - MaxLifeRate) * (end - alphaAnim.Mid), MaxLifeRate - curveOut) + end;
        }
        int random = (int)(SplRandom.Next() >> 24);
        int scaled = alpha * (MaxLifeRate - ((alphaAnim.RandomRange * random) >> 8));
        particle.AnimAlpha = (int)(((uint)scaled << 8) >> 16);
    }

    // OFFSET 0x020A1BD4
    public static void SPLAnim_Color(SplParticle particle, SplResource resource, int lifeRate)
    {
        SplColorAnim colorAnim = resource.ColorAnim!;
        SplResourceHeader header = resource.Header;
        int curveIn = colorAnim.CurveIn;
        int curvePeak = colorAnim.CurvePeak;
        int curveOut = colorAnim.CurveOut;
        if (lifeRate < curveIn)
        {
            particle.Color = colorAnim.Start;
            return;
        }
        if (lifeRate < curvePeak)
        {
            int peak = header.Color;
            int start = colorAnim.Start;
            int peakRed = peak & 0x1F;
            int peakGreen = (peak >> 5) & 0x1F;
            int peakBlue = (peak >> 10) & 0x1F;
            int startRed = start & 0x1F;
            int startGreen = (start >> 5) & 0x1F;
            int startBlue = (start >> 10) & 0x1F;
            if (!colorAnim.Interpolate)
            {
                particle.Color = PackColor(peakRed, peakGreen, peakBlue);
                return;
            }
            int elapsed = lifeRate - curveIn;
            int duration = curvePeak - curveIn;
            int blue = SplMath.Div(elapsed * (peakBlue - startBlue), duration);
            int red = SplMath.Div(elapsed * (peakRed - startRed), duration);
            int green = SplMath.Div(elapsed * (peakGreen - startGreen), duration);
            particle.Color = (ushort)((startRed + red) | ((startGreen + green) << 5) | ((startBlue + blue) << 10));
            return;
        }
        if (lifeRate < curveOut)
        {
            int end = colorAnim.End;
            int peak = header.Color;
            int endRed = end & 0x1F;
            int endGreen = (end >> 5) & 0x1F;
            int endBlue = (end >> 10) & 0x1F;
            int peakRed = peak & 0x1F;
            int peakGreen = (peak >> 5) & 0x1F;
            int peakBlue = (peak >> 10) & 0x1F;
            if (!colorAnim.Interpolate)
            {
                particle.Color = PackColor(endRed, endGreen, endBlue);
                return;
            }
            int elapsed = lifeRate - curvePeak;
            int duration = curveOut - curvePeak;
            int blue = SplMath.Div(elapsed * (endBlue - peakBlue), duration);
            int red = SplMath.Div(elapsed * (endRed - peakRed), duration);
            int green = SplMath.Div(elapsed * (endGreen - peakGreen), duration);
            particle.Color = (ushort)((peakRed + red) | ((peakGreen + green) << 5) | ((peakBlue + blue) << 10));
            return;
        }
        particle.Color = colorAnim.End;
    }

    // OFFSET 0x020A1DA0
    public static void SPLAnim_Scale(SplParticle particle, SplResource resource, int lifeRate)
    {
        SplScaleAnim scaleAnim = resource.ScaleAnim!;
        int curveIn = scaleAnim.CurveIn;
        int curveOut = scaleAnim.CurveOut;
        if (lifeRate < curveIn)
        {
            int start = scaleAnim.Start;
            particle.AnimScale = (short)(start + SplMath.Div(lifeRate * (scaleAnim.Mid - start), curveIn));
            return;
        }
        if (lifeRate < curveOut)
        {
            particle.AnimScale = scaleAnim.Mid;
            return;
        }
        int end = scaleAnim.End;
        particle.AnimScale = (short)(end + SplMath.Div((lifeRate - MaxLifeRate) * (end - scaleAnim.Mid), MaxLifeRate - curveOut));
    }
}
