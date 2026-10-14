using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class RenderView
{
    private const int CellBoundingRadiusMask = 0x3F;
    private const int CellHasBoundsShift = 11;
    private const int CellBoundsOffset = 8;

    // OFFSET 0x0202309C
    public static void RenderView_InitRenderer(G2dRenderer renderer, int zOffset)
    {
        NnsG2d.InitRenderer(renderer);
        renderer.SpriteZOffsetStep = zOffset;
    }

    // OFFSET 0x020230AC
    public static void RenderView_InitSurface(RenderSurface surface, in ViewRect viewRect, OamRegisterFunc oamRegister, OamAffineRegisterFunc affineRegister, SurfaceCullFunc cull, int type, G2dRenderer? renderer)
    {
        NnsG2d.InitRenderSurface(surface);
        surface.ViewRect = viewRect;
        surface.OamRegister = oamRegister;
        surface.OamAffineRegister = affineRegister;
        surface.VisibilityCulling = cull;
        surface.Type = type;
        if (renderer != null)
        {
            NnsG2d.AddRendererTargetSurface(renderer, surface);
        }
    }

    // OFFSET 0x020230E0
    public static void RenderView_SetViewRect(RenderSurface surface, in ViewRect viewRect)
    {
        surface.ViewRect = viewRect;
    }

    // OFFSET 0x020230F0
    public static bool RenderView_IsCellVisible(CellData cell, in MtxFx32 matrix, in ViewRect viewRect)
    {
        int x = matrix.M20 - viewRect.X;
        int y = matrix.M21 - viewRect.Y;
        int cellAttr = cell.CellAttr;
        int radius = (byte)((cellAttr & CellBoundingRadiusMask) << 2);
        int minY;
        int maxY;
        int minX;
        int maxX;
        if (((cellAttr >> CellHasBoundsShift) & 1) == 1)
        {
            int bounds = cell.Offset + CellBoundsOffset;
            minY = (short)Bin.U16(cell.File, bounds + 6) << Fx32.Shift;
            maxY = (short)Bin.U16(cell.File, bounds + 2) << Fx32.Shift;
            minX = (short)Bin.U16(cell.File, bounds + 4) << Fx32.Shift;
            maxX = (short)Bin.U16(cell.File, bounds) << Fx32.Shift;
        }
        else
        {
            minY = -radius << Fx32.Shift;
            maxY = radius << Fx32.Shift;
            minX = -radius << Fx32.Shift;
            maxX = radius << Fx32.Shift;
        }
        int top = y + Fx32.Mul(minY, matrix.M01) + Fx32.Mul(minY, matrix.M11);
        int bottom = y + Fx32.Mul(maxY, matrix.M01) + Fx32.Mul(maxY, matrix.M11);
        int left = x + Fx32.Mul(minX, matrix.M00) + Fx32.Mul(minX, matrix.M10);
        int right = x + Fx32.Mul(maxX, matrix.M00) + Fx32.Mul(maxX, matrix.M10);
        if (bottom < top)
        {
            (bottom, top) = (top, bottom);
        }
        if (right < left)
        {
            (right, left) = (left, right);
        }
        if (bottom > 0 && top < viewRect.Height && right > 0 && left < viewRect.Width)
        {
            return true;
        }
        return false;
    }
}
