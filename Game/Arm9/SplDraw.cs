using System.Numerics;

namespace PlatinumSharp.Game;

public static class SplDraw
{
    private const uint PolygonAttrCullNone = 0xC0;
    private const uint TexGenTexCoord = 0x40000000;
    private const int TextureFormatPalette4 = 2;
    private const int PrimitiveQuads = 1;
    private const int DirectionThreshold = 0xCCD;
    private const int ScaleAnimDirXY = 0;
    private const int ScaleAnimDirX = 1;
    private const int ScaleAnimDirY = 2;

    private static readonly SplRotateFunc[] sRotationFuncs =
    {
        SPLUtil_RotateY,
        SPLUtil_RotateXYZ,
    };

    private static readonly SplDrawPlaneFunc[] sDrawPlaneFuncs =
    {
        SPLUtil_DrawXYPlane,
        SPLUtil_DrawXZPlane,
    };

    private static bool SetPolygonAttr(SplManager manager, SplParticle particle)
    {
        int alpha = (particle.BaseAlpha * (particle.AnimAlpha + 1)) >> 5;
        Render3D.SetPolygonAttr(manager.MiscPolygonAttr | PolygonAttrCullNone | ((uint)particle.CurrentPolygonID << 24) | ((uint)alpha << 16));
        return alpha != 0;
    }

    private static void SetColor(SplManager manager, SplParticle particle)
    {
        int particleColor = particle.Color;
        int emitterColor = manager.RenderEmitter!.Color;
        int red = ((particleColor & 0x1F) * (emitterColor & 0x1F)) >> 5;
        int green = ((particleColor & 0x3E0) * (emitterColor & 0x3E0)) >> 15;
        int blue = ((particleColor & 0x7C00) * (emitterColor & 0x7C00)) >> 25;
        Render3D.SetColor((ushort)(red | (green << 5) | (blue << 10)));
    }

    private static void ComputeScale(SplResourceHeader header, SplParticle particle, out int scaleX, out int scaleY)
    {
        scaleX = Fx32.Mul(particle.BaseScale, header.AspectRatio);
        scaleY = particle.BaseScale;
        switch (header.ScaleAnimDir)
        {
            case ScaleAnimDirXY:
                scaleX = Fx32.Mul(scaleX, particle.AnimScale);
                scaleY = Fx32.Mul(scaleY, particle.AnimScale);
                break;
            case ScaleAnimDirX:
                scaleX = Fx32.Mul(scaleX, particle.AnimScale);
                break;
            case ScaleAnimDirY:
                scaleY = Fx32.Mul(scaleY, particle.AnimScale);
                break;
        }
    }

    private static void GetSinCos(SplParticle particle, out int sin, out int cos)
    {
        int index = particle.Rotation >> 4;
        sin = Render3D.Sin(index);
        cos = Render3D.Cos(index);
    }

    private static VecFx32 GetPosition(SplResourceHeader header, SplParticle particle)
    {
        VecFx32 position;
        position.X = particle.Position.X + particle.EmitterPos.X;
        position.Y = particle.Position.Y + particle.EmitterPos.Y;
        position.Z = particle.Position.Z + particle.EmitterPos.Z;
        if (header.UseViewSpace)
        {
            position.X -= header.EmitterBasePos.X;
            position.Y -= header.EmitterBasePos.Y;
            position.Z -= header.EmitterBasePos.Z;
        }
        return position;
    }

    private static void ComputeDirection(SplParticle particle, bool faceEmitter, out MtxFx43 direction)
    {
        SplMath.Identity43(out direction);
        VecFx32 forward;
        if (!faceEmitter)
        {
            SplMath.Normalize(particle.Velocity, out forward);
        }
        else
        {
            SplMath.Normalize(particle.Position, out forward);
            forward.X = -forward.X;
            forward.Y = -forward.Y;
            forward.Z = -forward.Z;
        }
        VecFx32 up = new() { X = 0, Y = Fx32.One, Z = 0 };
        int dot = SplMath.Dot(forward, up);
        if (dot > DirectionThreshold || dot < -DirectionThreshold)
        {
            up.X = Fx32.One;
            up.Y = 0;
            up.Z = 0;
        }
        SplMath.Cross(forward, up, out VecFx32 axis1);
        SplMath.Cross(forward, axis1, out VecFx32 axis2);
        direction.M00 = axis1.X;
        direction.M01 = axis1.Y;
        direction.M02 = axis1.Z;
        direction.M10 = forward.X;
        direction.M11 = forward.Y;
        direction.M12 = forward.Z;
        direction.M20 = axis2.X;
        direction.M21 = axis2.Y;
        direction.M22 = axis2.Z;
    }

    private static void LoadPolygonTransform(SplManager manager, SplParticle particle, ref MtxFx43 transform)
    {
        SplResourceHeader header = manager.RenderEmitter!.Resource!.Header;
        VecFx32 position = GetPosition(header, particle);
        transform.M30 = position.X;
        transform.M31 = position.Y;
        transform.M32 = position.Z;
        if (!header.UseViewSpace)
        {
            SplMath.LoadMtx43(manager.ViewMatrix);
            SplMath.MultMtx43(transform);
            return;
        }
        SplMath.LoadIdentity();
        Render3D.Translate(header.EmitterBasePos.X, header.EmitterBasePos.Y, header.EmitterBasePos.Z);
        SplMath.MultMtx43(manager.ViewMatrix);
        SplMath.MultMtx43(transform);
    }

    private static void LoadBillboardTransform(SplResourceHeader header, in MtxFx43 transform)
    {
        SplMath.LoadIdentity();
        if (header.UseViewSpace)
        {
            Render3D.Translate(header.EmitterBasePos.X, header.EmitterBasePos.Y, header.EmitterBasePos.Z);
        }
        SplMath.MultMtx43(transform);
    }

    private static void BuildBillboard(SplManager manager, SplParticle particle, int scaleX, int scaleY, out MtxFx43 transform)
    {
        SplResourceHeader header = manager.RenderEmitter!.Resource!.Header;
        VecFx32 position = GetPosition(header, particle);
        SplMath.MultVec43(position, manager.ViewMatrix, out position);
        GetSinCos(particle, out int sin, out int cos);
        transform.M00 = Fx32.Mul(cos, scaleX);
        transform.M01 = Fx32.Mul(sin, scaleX);
        transform.M02 = 0;
        transform.M10 = Fx32.Mul(-sin, scaleY);
        transform.M11 = Fx32.Mul(cos, scaleY);
        transform.M12 = 0;
        transform.M20 = 0;
        transform.M21 = 0;
        transform.M22 = Fx32.One;
        transform.M30 = position.X;
        transform.M31 = position.Y;
        transform.M32 = position.Z;
    }

    private static bool BuildDirectionalBillboard(SplManager manager, SplParticle particle, int scaleX, int scaleY, out MtxFx43 transform)
    {
        transform = default;
        SplResourceHeader header = manager.RenderEmitter!.Resource!.Header;
        MtxFx43 view = manager.ViewMatrix;
        VecFx32 position = GetPosition(header, particle);
        VecFx32 direction = particle.Velocity;
        VecFx32 viewZ = new() { X = view.M02, Y = view.M12, Z = view.M22 };
        SplMath.Cross(direction, viewZ, out direction);
        if (direction.X == 0 && direction.Y == 0 && direction.Z == 0)
        {
            return false;
        }
        SplMath.Normalize(direction, out direction);
        SplMath.Copy43To33(view, out MtxFx33 viewRotation);
        SplMath.MultVec33(direction, viewRotation, out direction);
        SplMath.MultVec43(position, view, out position);
        SplMath.Normalize(particle.Velocity, out VecFx32 velocity);
        int dot = Fx32.Mul(velocity.Z, -view.M22) + (Fx32.Mul(velocity.X, -view.M02) + Fx32.Mul(velocity.Y, -view.M12));
        if (dot < 0)
        {
            dot = -dot;
        }
        int stretch = Fx32.Mul(Fx32.One - dot, header.DbbScale) + Fx32.One;
        scaleY = Fx32.Mul(scaleY, stretch);
        transform.M00 = Fx32.Mul(direction.X, scaleX);
        transform.M01 = Fx32.Mul(direction.Y, scaleX);
        transform.M02 = 0;
        transform.M10 = Fx32.Mul(-direction.Y, scaleY);
        transform.M11 = Fx32.Mul(direction.X, scaleY);
        transform.M12 = 0;
        transform.M20 = 0;
        transform.M21 = 0;
        transform.M22 = Fx32.One;
        transform.M30 = position.X;
        transform.M31 = position.Y;
        transform.M32 = position.Z;
        return true;
    }

    private static float Vtx10(int value)
    {
        return ((short)value >> 6) / 64f;
    }

    private static float TexCoordValue(int value)
    {
        return (short)(value >> 8) / 16f;
    }

    // OFFSET 0x0209DC64
    public static void SPLUtil_SetTexture_Stub(SplTexture texture)
    {
    }

    // OFFSET 0x0209DC68
    public static void SPLUtil_SetTexture(SplTexture texture)
    {
        uint param = texture.Param;
        uint format = param & 0xF;
        uint sizeS = (param >> 4) & 0xF;
        uint sizeT = (param >> 8) & 0xF;
        uint repeat = (param >> 12) & 3;
        uint flip = (param >> 14) & 3;
        uint paletteColor0 = (param >> 16) & 1;
        Render3D.SetTexImageParam((texture.TexAddr >> 3) | (format << 26) | TexGenTexCoord | (sizeS << 20) | (sizeT << 23) | (repeat << 16) | (flip << 18) | (paletteColor0 << 29));
        Render3D.SetPaletteBase(texture.PalAddr >> (4 - (format == TextureFormatPalette4 ? 1 : 0)));
        Render3D.TextureMatrix = Matrix4x4.CreateScale(texture.Width, texture.Height, 0);
    }

    // OFFSET 0x0209DD54
    public static void SPLDraw_Child_DirectionalPolygon(SplManager manager, SplParticle particle)
    {
        if (!SetPolygonAttr(manager, particle))
        {
            return;
        }
        SplEmitter emitter = manager.RenderEmitter!;
        SplResourceHeader header = emitter.Resource!.Header;
        SplChildResource child = emitter.Resource.ChildResource!;
        GetSinCos(particle, out int sin, out int cos);
        sRotationFuncs[child.PolygonRotAxis](sin, cos, out MtxFx43 rotation);
        ComputeDirection(particle, child.DpolFaceEmitter, out MtxFx43 direction);
        SplMath.Concat43(rotation, direction, out rotation);
        ComputeScale(header, particle, out int scaleX, out int scaleY);
        SplMath.Scale43(out MtxFx43 scale, scaleX, scaleY, scaleY);
        SplMath.Concat43(rotation, scale, out MtxFx43 transform);
        LoadPolygonTransform(manager, particle, ref transform);
        SetColor(manager, particle);
        sDrawPlaneFuncs[child.PolygonReferencePlane](emitter.ChildTextureS, emitter.ChildTextureT, 0, 0);
    }

    // OFFSET 0x0209E1D4
    public static void SPLDraw_DirectionalPolygon(SplManager manager, SplParticle particle)
    {
        if (!SetPolygonAttr(manager, particle))
        {
            return;
        }
        SplEmitter emitter = manager.RenderEmitter!;
        SplResourceHeader header = emitter.Resource!.Header;
        GetSinCos(particle, out int sin, out int cos);
        sRotationFuncs[header.PolygonRotAxis](sin, cos, out MtxFx43 rotation);
        ComputeDirection(particle, header.DpolFaceEmitter, out MtxFx43 direction);
        SplMath.Concat43(rotation, direction, out rotation);
        ComputeScale(header, particle, out int scaleX, out int scaleY);
        SplMath.Scale43(out MtxFx43 scale, scaleX, scaleY, scaleY);
        SplMath.Concat43(scale, rotation, out MtxFx43 transform);
        LoadPolygonTransform(manager, particle, ref transform);
        SetColor(manager, particle);
        sDrawPlaneFuncs[header.PolygonReferencePlane](emitter.TextureS, emitter.TextureT, header.PolygonX, header.PolygonY);
    }

    // OFFSET 0x0209E650
    public static void SPLDraw_Child_Polygon(SplManager manager, SplParticle particle)
    {
        if (!SetPolygonAttr(manager, particle))
        {
            return;
        }
        SplEmitter emitter = manager.RenderEmitter!;
        SplResourceHeader header = emitter.Resource!.Header;
        SplChildResource child = emitter.Resource.ChildResource!;
        GetSinCos(particle, out int sin, out int cos);
        sRotationFuncs[child.PolygonRotAxis](sin, cos, out MtxFx43 rotation);
        ComputeScale(header, particle, out int scaleX, out int scaleY);
        SplMath.Scale43(out MtxFx43 scale, scaleX, scaleY, scaleY);
        SplMath.Concat43(rotation, scale, out MtxFx43 transform);
        LoadPolygonTransform(manager, particle, ref transform);
        SetColor(manager, particle);
        sDrawPlaneFuncs[child.PolygonReferencePlane](emitter.ChildTextureS, emitter.ChildTextureT, 0, 0);
    }

    // OFFSET 0x0209E9A0
    public static void SPLDraw_Polygon(SplManager manager, SplParticle particle)
    {
        if (!SetPolygonAttr(manager, particle))
        {
            return;
        }
        SplEmitter emitter = manager.RenderEmitter!;
        SplResourceHeader header = emitter.Resource!.Header;
        GetSinCos(particle, out int sin, out int cos);
        sRotationFuncs[header.PolygonRotAxis](sin, cos, out MtxFx43 rotation);
        ComputeScale(header, particle, out int scaleX, out int scaleY);
        SplMath.Scale43(out MtxFx43 scale, scaleX, scaleY, scaleY);
        SplMath.Concat43(scale, rotation, out MtxFx43 transform);
        LoadPolygonTransform(manager, particle, ref transform);
        SetColor(manager, particle);
        sDrawPlaneFuncs[header.PolygonReferencePlane](emitter.TextureS, emitter.TextureT, header.PolygonX, header.PolygonY);
    }

    // OFFSET 0x0209ECF0
    public static void SPLDraw_Child_DirectionalBillboard(SplManager manager, SplParticle particle)
    {
        if (!SetPolygonAttr(manager, particle))
        {
            return;
        }
        SplEmitter emitter = manager.RenderEmitter!;
        SplResourceHeader header = emitter.Resource!.Header;
        ComputeScale(header, particle, out int scaleX, out int scaleY);
        if (!BuildDirectionalBillboard(manager, particle, scaleX, scaleY, out MtxFx43 transform))
        {
            return;
        }
        LoadBillboardTransform(header, transform);
        SetColor(manager, particle);
        SPLUtil_DrawXYPlane(emitter.ChildTextureS, emitter.ChildTextureT, 0, 0);
    }

    // OFFSET 0x0209F3D0
    public static void SPLDraw_DirectionalBillboard(SplManager manager, SplParticle particle)
    {
        if (!SetPolygonAttr(manager, particle))
        {
            return;
        }
        SplEmitter emitter = manager.RenderEmitter!;
        SplResourceHeader header = emitter.Resource!.Header;
        ComputeScale(header, particle, out int scaleX, out int scaleY);
        if (!BuildDirectionalBillboard(manager, particle, scaleX, scaleY, out MtxFx43 transform))
        {
            return;
        }
        LoadBillboardTransform(header, transform);
        SetColor(manager, particle);
        SPLUtil_DrawXYPlane(emitter.TextureS, emitter.TextureT, header.PolygonX, header.PolygonY);
    }

    // OFFSET 0x0209FAB8
    public static void SPLDraw_Child_Billboard(SplManager manager, SplParticle particle)
    {
        if (!SetPolygonAttr(manager, particle))
        {
            return;
        }
        SplEmitter emitter = manager.RenderEmitter!;
        SplResourceHeader header = emitter.Resource!.Header;
        ComputeScale(header, particle, out int scaleX, out int scaleY);
        BuildBillboard(manager, particle, scaleX, scaleY, out MtxFx43 transform);
        LoadBillboardTransform(header, transform);
        SetColor(manager, particle);
        SPLUtil_DrawXYPlane(emitter.ChildTextureS, emitter.ChildTextureT, 0, 0);
    }

    // OFFSET 0x0209FF0C
    public static void SPLDraw_Billboard(SplManager manager, SplParticle particle)
    {
        if (!SetPolygonAttr(manager, particle))
        {
            return;
        }
        SplEmitter emitter = manager.RenderEmitter!;
        SplResourceHeader header = emitter.Resource!.Header;
        int scaleX = Fx32.Mul(particle.BaseScale, header.AspectRatio);
        int scaleY = particle.BaseScale;
        if (header.ScaleAnimDir == ScaleAnimDirXY)
        {
            scaleX = Fx32.Mul(scaleX, particle.AnimScale);
            scaleY = Fx32.Mul(scaleY, particle.AnimScale);
        }
        else if (header.ScaleAnimDir == ScaleAnimDirX)
        {
            scaleX = Fx32.Mul(scaleX, particle.AnimScale);
        }
        else
        {
            scaleY = Fx32.Mul(scaleY, particle.AnimScale);
        }
        BuildBillboard(manager, particle, scaleX, scaleY, out MtxFx43 transform);
        LoadBillboardTransform(header, transform);
        SetColor(manager, particle);
        SPLUtil_DrawXYPlane(emitter.TextureS, emitter.TextureT, header.PolygonX, header.PolygonY);
    }

    // OFFSET 0x020A0358
    public static void SPLUtil_RotateY(int sin, int cos, out MtxFx43 matrix)
    {
        matrix.M00 = cos;
        matrix.M10 = 0;
        matrix.M20 = sin;
        matrix.M30 = 0;
        matrix.M01 = 0;
        matrix.M11 = Fx32.One;
        matrix.M21 = 0;
        matrix.M31 = 0;
        matrix.M02 = -sin;
        matrix.M12 = 0;
        matrix.M22 = cos;
        matrix.M32 = 0;
    }

    // OFFSET 0x020A0398
    public static void SPLUtil_RotateXYZ(int sin, int cos, out MtxFx43 matrix)
    {
        int common = Fx32.Mul(Fx32.One - cos, 0x555);
        int offset = Fx32.Mul(sin, 0x93D);
        int diagonal = common + cos;
        int upper = common + offset;
        int lower = common - offset;
        matrix.M00 = diagonal;
        matrix.M10 = upper;
        matrix.M20 = lower;
        matrix.M30 = 0;
        matrix.M01 = lower;
        matrix.M11 = diagonal;
        matrix.M21 = upper;
        matrix.M31 = 0;
        matrix.M02 = upper;
        matrix.M12 = lower;
        matrix.M22 = diagonal;
        matrix.M32 = 0;
    }

    // OFFSET 0x020A0444
    public static void SPLUtil_DrawXZPlane(int s, int t, int offsetX, int offsetZ)
    {
        float left = Vtx10(offsetX - Fx32.One);
        float top = Vtx10(offsetZ + Fx32.One);
        float right = Vtx10(offsetX + Fx32.One);
        float bottom = Vtx10(offsetZ - Fx32.One);
        float texS = TexCoordValue(s);
        float texT = TexCoordValue(t);
        Render3D.Begin(PrimitiveQuads);
        Render3D.TexCoord(0, 0);
        Render3D.Vertex(left, 0, top);
        Render3D.TexCoord(texS, 0);
        Render3D.Vertex(right, 0, top);
        Render3D.TexCoord(texS, texT);
        Render3D.Vertex(right, 0, bottom);
        Render3D.TexCoord(0, texT);
        Render3D.Vertex(left, 0, bottom);
        Render3D.End();
    }

    // OFFSET 0x020A0500
    public static void SPLUtil_DrawXYPlane(int s, int t, int offsetX, int offsetY)
    {
        float top = Vtx10(offsetY + Fx32.One);
        float left = Vtx10(offsetX - Fx32.One);
        float right = Vtx10(offsetX + Fx32.One);
        float bottom = Vtx10(offsetY - Fx32.One);
        float texS = TexCoordValue(s);
        float texT = TexCoordValue(t);
        Render3D.Begin(PrimitiveQuads);
        Render3D.TexCoord(0, 0);
        Render3D.Vertex(left, top, 0);
        Render3D.TexCoord(texS, 0);
        Render3D.Vertex(right, top, 0);
        Render3D.TexCoord(texS, texT);
        Render3D.Vertex(right, bottom, 0);
        Render3D.TexCoord(0, texT);
        Render3D.Vertex(left, bottom, 0);
        Render3D.End();
    }
}
