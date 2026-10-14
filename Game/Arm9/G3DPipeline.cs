using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void G3DPipelineSetupCallback();

public sealed class G3DPipelineBuffers
{
    public int HeapID;
    public byte[]? PlttBuffer;
    public byte[]? TexBuffer;
}

public static class G3DPipeline
{
    public const int VramManagerLinkedList = 0;
    public const int VramManagerFrame = 1;

    private const int SortModeAuto = 0;
    private const int BufferModeW = 1;
    private const int Bg0Layer = 0;
    private const int Bg0LayerMask = 1;
    private const ushort BgPriorityMask = 3;
    private const ushort Disp3DShadingMask = 0xCFFD;
    private const ushort Disp3DAntiAliasMask = 0xCFEF;
    private const ushort Disp3DAlphaTestMask = 0xCFFB;
    private const ushort Disp3DAlphaBlendMask = 0xCFF7;
    private const ushort Disp3DEdgeMarkingMask = 0xCFDF;
    private const int ClearDepthMax = 0x7FFF;
    private const int ClearPolygonID = 63;

    // OFFSET 0x02024220
    public static G3DPipelineBuffers G3DPipeline_InitEx(int heapID, int texVramManagerType, uint texVramSize, int plttVramManagerType, uint plttVramSize, G3DPipelineSetupCallback? setup)
    {
        G3DPipelineBuffers buffers = new();
        buffers.HeapID = heapID;
        Render3D.Reset();
        Render3D.SwapBuffers(SortModeAuto, BufferModeW);
        if (texVramManagerType == VramManagerLinkedList)
        {
            uint workSize = NnsGfd.GetLnkTexVramManagerWorkSize(texVramSize << 7);
            buffers.TexBuffer = new byte[workSize];
            NnsGfd.InitLnkTexVramManager(texVramSize << 17, 0, buffers.TexBuffer, workSize, true);
        }
        else
        {
            NnsGfd.InitFrmTexVramManager((ushort)texVramSize, true);
        }
        if (plttVramManagerType == VramManagerLinkedList)
        {
            uint workSize = NnsGfd.GetLnkPlttVramManagerWorkSize(plttVramSize << 8);
            buffers.PlttBuffer = new byte[workSize];
            NnsGfd.InitLnkPlttVramManager(plttVramSize << 13, buffers.PlttBuffer, workSize, true);
        }
        else
        {
            NnsGfd.InitFrmTexVramManager((ushort)(plttVramSize << 13), true);
        }
        if (setup != null)
        {
            setup();
        }
        else
        {
            G3DPipeline_DefaultSetup();
        }
        return buffers;
    }

    // OFFSET 0x020242C4
    public static void G3DPipelineBuffers_Free(G3DPipelineBuffers buffers)
    {
        buffers.PlttBuffer = null;
        buffers.TexBuffer = null;
    }

    // OFFSET 0x020242DC
    private static void G3DPipeline_DefaultSetup()
    {
        GXLayers.GXLayers_EngineAToggleLayers(Bg0LayerMask, 1);
        Video.SetBgControl(Bg0Layer, (ushort)(Video.GetBgControl(Bg0Layer) & ~BgPriorityMask));
        Render3D.Display3DControl &= Disp3DShadingMask;
        Render3D.Display3DControl &= Disp3DAntiAliasMask;
        Render3D.Display3DControl &= Disp3DAlphaTestMask;
        Render3D.Display3DControl &= Disp3DAlphaBlendMask;
        Render3D.Display3DControl &= Disp3DEdgeMarkingMask;
        Render3D.SetFog(false, 0, 0, 0);
        Render3D.SetClearColor(0, 0, ClearDepthMax, ClearPolygonID, false);
        Render3D.SetViewport(0, 0, 255, 191);
    }
}
