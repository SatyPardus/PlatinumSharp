using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public struct GFXTestBox
{
    public short Width;
    public short Height;
    public short Depth;
    public int XScale;
    public int YScale;
    public int ZScale;
}

public struct GFXRawBox
{
    public short X;
    public short Y;
    public short Z;
    public short Width;
    public short Height;
    public short Depth;
}

public static class GfxBoxTest
{
    private const int ModelInfoOffset = 0x14;
    private const int ModelBoxOffset = 0x18;
    private const int ModelBoxScaleOffset = 0x24;
    private const uint MaxDimension = 0x7FFF;

    // OFFSET 0x0201CED8
    public static uint GFXBoxTest_IsModelInView(G3dModel model, in VecFx32 position, in MtxFx33 rotation, in VecFx32 scale)
    {
        VecFx32 translation = position;
        byte[] file = model.File;
        int info = model.Offset + ModelInfoOffset;
        GFXBoxTest_MakeRawBox(
            (short)Bin.U16(file, info + ModelBoxOffset),
            (short)Bin.U16(file, info + ModelBoxOffset + 2),
            (short)Bin.U16(file, info + ModelBoxOffset + 4),
            (short)Bin.U16(file, info + ModelBoxOffset + 6),
            (short)Bin.U16(file, info + ModelBoxOffset + 8),
            (short)Bin.U16(file, info + ModelBoxOffset + 10),
            out GFXRawBox box);
        Render3D.SetBaseTranslation(translation);
        Render3D.SetBaseRotation(rotation);
        Render3D.SetBaseScale(scale);
        Render3D.FlushGlobalState();
        Render3D.PushMatrix();
        int boxScale = (int)Bin.U32(file, info + ModelBoxScaleOffset);
        Render3D.Scale(boxScale, boxScale, boxScale);
        uint result = GFXBoxTest_RunBoxTest(box);
        Render3D.PopMatrix(1);
        return result;
    }

    // OFFSET 0x0201CF7C
    public static uint GFXBoxTest_IsBoxAtPositionInView(in VecFx32 position, in GFXTestBox box)
    {
        GFXBoxTest_MakeRawBox(0, 0, 0, box.Width, box.Height, box.Depth, out GFXRawBox rawBox);
        Render3D.SetBaseTranslation(position);
        Render3D.FlushGlobalState();
        Render3D.PushMatrix();
        Render3D.Scale(box.XScale << Fx32.Shift, box.YScale << Fx32.Shift, box.ZScale << Fx32.Shift);
        uint result = GFXBoxTest_RunBoxTest(rawBox);
        Render3D.PopMatrix(1);
        return result;
    }

    // OFFSET 0x0201CFEC
    public static void GFXBoxTest_MakeBox(int width, int height, int depth, out GFXTestBox box)
    {
        box = default;
        GFXBoxTest_NormalizeDimension((uint)width, out box.Width, out box.XScale);
        GFXBoxTest_NormalizeDimension((uint)height, out box.Height, out box.YScale);
        GFXBoxTest_NormalizeDimension((uint)depth, out box.Depth, out box.ZScale);
    }

    // OFFSET 0x0201D018
    private static void GFXBoxTest_MakeRawBox(short x, short y, short z, short width, short height, short depth, out GFXRawBox box)
    {
        box.X = x;
        box.Y = y;
        box.Z = z;
        box.Width = width;
        box.Height = height;
        box.Depth = depth;
    }

    // OFFSET 0x0201D03C
    private static uint GFXBoxTest_RunBoxTest(in GFXRawBox box)
    {
        return Render3D.BoxTest(box.X, box.Y, box.Z, box.Width, box.Height, box.Depth) ? 1u : 0u;
    }

    // OFFSET 0x0201D090
    private static void GFXBoxTest_NormalizeDimension(uint value, out short dimension, out int scale)
    {
        byte shifts = 0;
        while (value >= MaxDimension)
        {
            shifts++;
            value >>= 1;
        }
        dimension = (short)value;
        scale = 1;
        for (int i = 0; i < shifts; i++)
        {
            scale <<= 1;
        }
    }
}
