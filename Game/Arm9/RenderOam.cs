using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate bool OamRegisterFunc(OamAttributes oam, ushort affineIndex);

public delegate ushort OamAffineRegisterFunc(MtxFx22 matrix);

public delegate bool SurfaceCullFunc(CellData cell, in MtxFx32 matrix, in ViewRect viewRect);

public sealed class RenderOamManager
{
    public readonly OamManager Main = new();
    public readonly OamManager Sub = new();
    public int HeapID;
}

public static class RenderOam
{
    private const int ScreenMain = 0;
    private const int ScreenSub = 1;
    private const int SurfaceMain = 1;
    private const int ReservedOam = 4;
    private const int ReservedAffine = 1;

    private static RenderOamManager? sOam;

    // OFFSET 0x0200A784
    public static void RenderOam_Init(int mainOamStart, int mainOamCount, int mainAffineStart, int mainAffineCount, int subOamStart, int subOamCount, int subAffineStart, int subAffineCount, int heapID)
    {
        int oamStart = mainOamStart;
        int affineStart = mainAffineStart;
        if (mainOamStart < ReservedOam)
        {
            oamStart = ReservedOam;
            if (mainOamCount > 0x7C)
            {
                mainOamCount -= ReservedOam - mainOamStart;
            }
        }
        if (mainAffineStart < ReservedAffine)
        {
            affineStart = ReservedAffine;
            if (mainAffineCount > 0x1E)
            {
                mainAffineCount -= ReservedAffine - mainAffineStart;
            }
        }
        InitOamManager(oamStart, mainOamCount, affineStart, mainAffineCount, subOamStart, subOamCount, subAffineStart, subAffineCount, heapID);
    }

    // OFFSET 0x0200A7C8
    private static void InitOamManager(int mainOamStart, int mainOamCount, int mainAffineStart, int mainAffineCount, int subOamStart, int subOamCount, int subAffineStart, int subAffineCount, int heapID)
    {
        if (sOam != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sOam = new RenderOamManager();
        sOam.HeapID = heapID;
        if (!OamManager.Create(sOam.Main, (ushort)mainOamStart, (ushort)mainOamCount, (ushort)mainAffineStart, (ushort)mainAffineCount, ScreenMain))
        {
            ErrorHandling.GF_AssertFail();
        }
        if (!OamManager.Create(sOam.Sub, (ushort)subOamStart, (ushort)subOamCount, (ushort)subAffineStart, (ushort)subAffineCount, ScreenSub))
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x0200A858
    public static void RenderOam_Transfer()
    {
        if (sOam != null)
        {
            sOam.Main.ApplyAndReset();
            sOam.Sub.ApplyAndReset();
        }
    }

    // OFFSET 0x0200A878
    public static void RenderOam_Free()
    {
        if (sOam == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        RenderOam_ClearMain(sOam!.HeapID);
        RenderOam_ClearSub(sOam.HeapID);
        sOam = null;
    }

    // OFFSET 0x0200A8B0
    public static void RenderOam_InitSurface(RenderSurface surface, ViewRect viewRect, int type, G2dRenderer renderer)
    {
        if (sOam == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (type == SurfaceMain)
        {
            RenderView.RenderView_InitSurface(surface, viewRect, AddToMainOamManager, AddAffineToMainOamManager, RenderView.RenderView_IsCellVisible, type, renderer);
        }
        else
        {
            RenderView.RenderView_InitSurface(surface, viewRect, AddToSubOamManager, AddAffineToSubOamManager, RenderView.RenderView_IsCellVisible, type, renderer);
        }
    }

    // OFFSET 0x0200A914
    public static OamManager RenderOam_GetScreenOam(int screen)
    {
        if (sOam == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (screen == ScreenMain)
        {
            return sOam!.Main;
        }
        return sOam!.Sub;
    }

    // OFFSET 0x0200A93C
    public static void RenderOam_ClearMain(int heapID)
    {
        Sprite.Sprite_ClearMainOam(heapID);
    }

    // OFFSET 0x0200A944
    public static void RenderOam_ClearSub(int heapID)
    {
        Sprite.Sprite_ClearSubOam(heapID);
    }

    // OFFSET 0x0200A94C
    private static bool AddToMainOamManager(OamAttributes oam, ushort affineIndex)
    {
        bool result = sOam!.Main.Entry(oam, affineIndex);
        if (!result)
        {
            ErrorHandling.GF_AssertFail();
        }
        return result;
    }

    // OFFSET 0x0200A96C
    private static bool AddToSubOamManager(OamAttributes oam, ushort affineIndex)
    {
        bool result = sOam!.Sub.Entry(oam, affineIndex);
        if (!result)
        {
            ErrorHandling.GF_AssertFail();
        }
        return result;
    }

    // OFFSET 0x0200A990
    private static ushort AddAffineToMainOamManager(MtxFx22 matrix)
    {
        ushort result = sOam!.Main.EntryAffine(matrix);
        if (result == OamManager.AffineNone)
        {
            ErrorHandling.GF_AssertFail();
        }
        return result;
    }

    // OFFSET 0x0200A9B4
    private static ushort AddAffineToSubOamManager(MtxFx22 matrix)
    {
        ushort result = sOam!.Sub.EntryAffine(matrix);
        if (result == OamManager.AffineNone)
        {
            ErrorHandling.GF_AssertFail();
        }
        return result;
    }
}
