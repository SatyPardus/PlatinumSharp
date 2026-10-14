using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static partial class NnsG2d
{
    private const uint OamWidthTable = 0x020F9670;
    private const uint OamHeightTable = 0x020F9658;
    private const uint OamFlipMask = 0x30000000;
    private const uint OamAffineClearMask = 0xC1FFFCFF;
    private const uint OamPositionClearMask = 0xFE00FF00;

    private static int OamDimension(uint table, uint attr01)
    {
        uint shapeSize = attr01 & 0xC000C000;
        int shape = (int)((shapeSize & 0xC000) >> 14);
        int size = (int)((shapeSize & 0xC0000000) >> 30);
        return RomData.Arm9U16((uint)(table + shape * 8 + size * 2), 1)[0];
    }

    // OFFSET 0x020A8DA0
    public static int MakeCellToOams(OamAttributes[] dstOams, int numDstOam, CellData cell, MtxFx22? mtx, VecFx32? baseTrans, ushort affineIndex, bool doubleAffine)
    {
        int count = numDstOam;
        if (count >= cell.NumOamAttrs)
        {
            count = cell.NumOamAttrs;
        }
        for (int i = 0; i < count; i++)
        {
            int src = cell.OamArray + i * OamAttrSize;
            uint attr01 = Bin.U32(cell.File, src);
            ushort attr2 = (ushort)Bin.U16(cell.File, src + 4);
            if (mtx == null && baseTrans == null)
            {
                dstOams[i] = new OamAttributes { Attr0 = (ushort)attr01, Attr1 = (ushort)(attr01 >> 16), Attr2 = attr2 };
                continue;
            }
            int xPixels = (short)((attr01 & 0x01FF0000) >> 16);
            if (xPixels > 0xFF)
            {
                xPixels = (short)(xPixels | 0xFF00);
            }
            int x = xPixels << Fx32.Shift;
            int yPixels = (int)(attr01 & 0xFF);
            if (yPixels > 0x7F)
            {
                yPixels = (short)(yPixels | 0xFF00);
            }
            int y = yPixels << Fx32.Shift;
            if (mtx != null)
            {
                MtxFx22 m = mtx.Value;
                uint sourceMode = attr01 & OamAffineModeMask;
                if (sourceMode != OamAffineNormal && sourceMode != OamAffineDouble)
                {
                    sourceMode |= attr01 & OamFlipMask;
                }
                if (sourceMode == OamAffineDouble)
                {
                    x += OamDimension(OamWidthTable, attr01) << 11;
                    y += OamDimension(OamHeightTable, attr01) << 11;
                }
                int transformedX = (int)(((long)m.M10 * y + 0x1000 + (long)m.M00 * x) >> Fx32.Shift);
                int transformedY = (int)(((long)m.M11 * y + 0x1000 + (long)m.M01 * x) >> Fx32.Shift);
                x = transformedX;
                y = transformedY;
                uint mode = doubleAffine ? OamAffineDouble : OamAffineNormal;
                attr01 = (attr01 & OamAffineClearMask) | mode | ((uint)affineIndex << 25);
                int halfWidth = OamDimension(OamWidthTable, attr01) >> 1;
                int halfHeight = OamDimension(OamHeightTable, attr01) >> 1;
                x += m.M00 * halfWidth - (halfWidth << Fx32.Shift) + m.M10 * halfHeight;
                y += m.M01 * halfWidth - (halfHeight << Fx32.Shift) + m.M11 * halfHeight;
                if (mode == OamAffineDouble)
                {
                    x -= halfWidth << Fx32.Shift;
                    y -= halfHeight << Fx32.Shift;
                }
            }
            if (baseTrans != null)
            {
                x += baseTrans.Value.X;
                y += baseTrans.Value.Y;
            }
            attr01 = (attr01 & OamPositionClearMask) | (uint)(((y + 0x800) >> Fx32.Shift) & 0xFF) | (((uint)((x + 0x800) >> Fx32.Shift) << 23) >> 7);
            dstOams[i] = new OamAttributes { Attr0 = (ushort)attr01, Attr1 = (ushort)(attr01 >> 16), Attr2 = attr2 };
        }
        return count;
    }
}
