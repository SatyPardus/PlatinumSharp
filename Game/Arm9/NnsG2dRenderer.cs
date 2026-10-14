using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public struct MtxFx32
{
    public int M00;
    public int M01;
    public int M10;
    public int M11;
    public int M20;
    public int M21;
}

public sealed class CellData
{
    public byte[] File = Array.Empty<byte>();
    public int Offset;
    public int OamArray;

    public int NumOamAttrs => Bin.U16(File, Offset);

    public int CellAttr => Bin.U16(File, Offset + 2);
}

public sealed class MtxCacheEntry
{
    public MtxFx22 Matrix;
    public readonly ushort[] AffineIndex = new ushort[8];
}

public delegate void RndCoreCellCallback(G2dRenderer core, CellData cell);

public delegate void RndCoreOamCallback(G2dRenderer core, CellData cell, ushort oamIndex);

public delegate void RenderCellCallback(G2dRenderer renderer, RenderSurface surface, CellData cell, in MtxFx32 matrix);

public delegate void RenderOamCallback(G2dRenderer renderer, RenderSurface surface, CellData cell, ushort oamIndex, in MtxFx32 matrix);

public static partial class NnsG2d
{
    public const int SurfaceType3D = 0;
    public const int SurfaceTypeMain2D = 1;
    public const int SurfaceTypeSub2D = 2;
    public const int SurfaceTypeMax = 3;
    public const int OpzHintSimpleMatrix = 1;
    public const int OpzHintSingleSurface = 2;
    public const int AffineOverwriteNone = 0;
    public const int AffineOverwriteNormal = 1;
    public const int AffineOverwriteDouble = 2;
    public const int OverwritePriority = 1;
    public const int OverwritePaletteNo = 2;
    public const int OverwriteMosaic = 4;
    public const int OverwriteObjMode = 8;
    public const int OverwritePaletteNoOffset = 16;
    public const ushort AffineIndexNone = 0xFFFE;

    private const int MtxStackSize = 32;
    private const int MtxCacheSize = 32;
    private const int MultiCellMtxCacheSize = 256;
    private const ushort NoSRIndex = 0xFFFE;
    private const ushort AffineIndexUnregistered = 0xFFFF;
    private const int MtxStateNone = 0;
    private const int MtxStateChanged = 1;
    private const int MtxStateInherited = 2;
    private const int MtxStateCached = 3;
    private const int OamAttrSize = 6;
    private const uint OamShapeSizeMask = 0xC000C000;
    private const uint OamAffineModeMask = 0x300;
    private const uint OamAffineNormal = 0x100;
    private const uint OamAffineDouble = 0x300;
    private const uint OamFlipH = 0x10000000;
    private const uint OamFlipV = 0x20000000;

    private struct MtxState
    {
        public ushort CacheIdx;
        public ushort GroupID;
        public ushort State;
        public ushort Padding;
    }

    private static readonly ushort[] sOamHeights = { 8, 16, 32, 64, 8, 8, 16, 32, 16, 32, 32, 64 };
    private static readonly ushort[] sOamWidths = { 8, 16, 32, 64, 16, 32, 32, 64, 8, 8, 16, 32 };

    private static G2dRenderer? sCurrentRenderer;
    private static int sMtxStackPos;
    private static int sSimpleMatrixMode;
    private static ushort sMtxGroupCount;
    private static ushort sMtxCacheCount;
    private static readonly int[] sMtxZ = new int[MtxStackSize];
    private static readonly MtxState[] sMtxState = new MtxState[MtxStackSize];
    private static readonly MtxFx32[] sMtxStack = new MtxFx32[MtxStackSize];
    private static readonly MtxFx32[] sMtxStack2D = new MtxFx32[MtxStackSize];
    private static readonly MtxCacheEntry[] sMtxCache = CreateMtxCache();
    private static ushort sMultiCellAnimIdx;
    private static readonly MtxCacheEntry?[] sMultiCellMtxCache = new MtxCacheEntry?[MultiCellMtxCacheSize];
    private static int sDrawingMultiCell;
    private static ushort sFirstSRIndex = NoSRIndex;
    private static uint sCurrentTransferHandle = NoTransferHandle;
    private static MtxFx32 sSimpleMatrix = new() { M00 = Fx32.One, M11 = Fx32.One };
    private static G2dRenderer? sCoreInstance;
    private static int sAutoZOffset;
    private static int sAutoZEnabled;
    private static int sAutoZStep = -Fx32.One;

    private static MtxCacheEntry[] CreateMtxCache()
    {
        MtxCacheEntry[] cache = new MtxCacheEntry[MtxCacheSize];
        for (int i = 0; i < MtxCacheSize; i++)
        {
            cache[i] = new MtxCacheEntry();
        }
        return cache;
    }

    private static MtxFx32 IdentityMtx()
    {
        return new MtxFx32 { M00 = Fx32.One, M11 = Fx32.One };
    }

    private static MtxFx32 GetCurrentMtx()
    {
        if (sSimpleMatrixMode != 0)
        {
            sSimpleMatrix.M20 = sMtxStack[sMtxStackPos].M20;
            sSimpleMatrix.M21 = sMtxStack[sMtxStackPos].M21;
            return sSimpleMatrix;
        }
        return sMtxStack[sMtxStackPos];
    }

    private static MtxFx32 GetCurrentMtx2D()
    {
        if (sSimpleMatrixMode != 0)
        {
            sSimpleMatrix.M20 = sMtxStack[sMtxStackPos].M20;
            sSimpleMatrix.M21 = sMtxStack[sMtxStackPos].M21;
            return sSimpleMatrix;
        }
        return sMtxStack2D[sMtxStackPos];
    }

    private static bool IsSRTransformEnabled()
    {
        return sSimpleMatrixMode == 0 && sFirstSRIndex != NoSRIndex;
    }

    private static void ResetAffineIndices(MtxCacheEntry entry)
    {
        Array.Fill(entry.AffineIndex, AffineIndexUnregistered);
    }

    public static CellData? GetCellData(CellDataBank? bank, int cell)
    {
        if (bank == null || cell == 0)
        {
            return null;
        }
        byte[] file = bank.File;
        int data = bank.SectionOffset + BlockHeaderSize;
        int head = data + (int)Bin.U32(file, data + 4);
        int cellSize = (Bin.U16(file, data + 2) & 1) != 0 ? CellWithBoundsSize : CellSize;
        int oamBase = head + Bin.U16(file, data) * cellSize;
        return new CellData { File = file, Offset = cell, OamArray = oamBase + (int)Bin.U32(file, cell + 4) };
    }

    // OFFSET 0x020A8180
    private static ushort PaletteSwapTable_Get(ushort[] table, uint palette)
    {
        return table[palette];
    }

    // OFFSET 0x020A9778
    private static void Renderer_ResetCurrentMtx()
    {
        if (sSimpleMatrixMode != 0)
        {
            sMtxStack[sMtxStackPos].M20 = 0;
            sMtxStack[sMtxStackPos].M21 = 0;
        }
        else
        {
            sMtxStack[sMtxStackPos] = IdentityMtx();
            sMtxStack2D[sMtxStackPos] = sMtxStack[sMtxStackPos];
        }
        sMtxZ[sMtxStackPos] = 0;
    }

    // OFFSET 0x020A9818
    private static void Renderer_StoreCurrentMtxToCache()
    {
        ref MtxState state = ref sMtxState[(ushort)sMtxStackPos];
        if (state.State != MtxStateChanged && state.State != MtxStateInherited)
        {
            return;
        }
        ushort groupID = state.GroupID;
        ushort cacheIdx = sMtxCacheCount;
        if (cacheIdx < MtxCacheSize - 1)
        {
            sMtxCacheCount = (ushort)(cacheIdx + 1);
        }
        state.CacheIdx = cacheIdx;
        ResetAffineIndices(sMtxCache[cacheIdx]);
        MtxFx32 matrix = GetCurrentMtx2D();
        MtxCacheEntry entry = sMtxCache[sMtxState[(ushort)sMtxStackPos].CacheIdx];
        entry.Matrix.M00 = matrix.M00;
        entry.Matrix.M01 = matrix.M01;
        entry.Matrix.M10 = matrix.M10;
        entry.Matrix.M11 = matrix.M11;
        for (int i = (ushort)sMtxStackPos; i >= 0; i--)
        {
            if (sMtxState[i].GroupID != groupID)
            {
                return;
            }
            sMtxState[i].State = MtxStateCached;
            sMtxState[i].CacheIdx = cacheIdx;
        }
    }

    // OFFSET 0x020A995C
    private static void Renderer_BeforeCell(G2dRenderer core, CellData cell)
    {
        RenderSurface surface = sCurrentRenderer!.CurrentSurface!;
        if (surface.VisibilityCulling != null)
        {
            MtxFx32 matrix = GetCurrentMtx();
            if (!surface.VisibilityCulling(cell, matrix, surface.ViewRect))
            {
                core.DrawEnable = 0;
                return;
            }
            core.DrawEnable = 1;
        }
        if (surface.BeforeDrawCell != null)
        {
            MtxFx32 matrix = GetCurrentMtx();
            surface.BeforeDrawCell(sCurrentRenderer, surface, cell, matrix);
        }
    }

    // OFFSET 0x020A9A70
    private static void Renderer_AfterCell(G2dRenderer core, CellData cell)
    {
        RenderSurface surface = sCurrentRenderer!.CurrentSurface!;
        if (surface.AfterDrawCell != null)
        {
            MtxFx32 matrix = GetCurrentMtx();
            surface.AfterDrawCell(sCurrentRenderer, surface, cell, matrix);
        }
    }

    // OFFSET 0x020A9AF4
    private static void Renderer_BeforeOam(G2dRenderer core, CellData cell, ushort oamIndex)
    {
        ushort[]? swapTable = Renderer_GetPaletteSwapTable(sCurrentRenderer!);
        if (swapTable != null)
        {
            uint palette = PaletteSwapTable_Get(swapTable, (core.CurrentOam23 & 0xF000) >> 12);
            core.CurrentOam23 = (core.CurrentOam23 & ~0xF000u) | ((palette << 28) >> 16);
        }
        G2dRenderer renderer = sCurrentRenderer!;
        if (renderer.OverwriteEnableFlag != 0)
        {
            if ((renderer.OverwriteEnableFlag & OverwritePriority) != 0)
            {
                core.CurrentOam23 = (core.CurrentOam23 & ~0xC00u) | (((uint)renderer.OverwritePriority << 30) >> 20);
            }
            if ((renderer.OverwriteEnableFlag & OverwritePaletteNo) != 0)
            {
                core.CurrentOam23 = (core.CurrentOam23 & ~0xF000u) | (((uint)renderer.OverwritePlttNo << 28) >> 16);
            }
            if ((renderer.OverwriteEnableFlag & OverwritePaletteNoOffset) != 0)
            {
                uint palette = renderer.OverwritePlttNoOffset + ((core.CurrentOam23 << 16) >> 28);
                core.CurrentOam23 = (core.CurrentOam23 & ~0xF000u) | ((palette << 28) >> 16);
            }
            if ((renderer.OverwriteEnableFlag & OverwriteMosaic) != 0)
            {
                if (renderer.OverwriteMosaicFlag != 0)
                {
                    core.CurrentOam01 |= 0x1000;
                }
                else
                {
                    core.CurrentOam01 &= ~0x1000u;
                }
            }
            if ((renderer.OverwriteEnableFlag & OverwriteObjMode) != 0)
            {
                core.CurrentOam01 = (core.CurrentOam01 & ~0xC00u) | ((uint)renderer.OverwriteObjMode << 10);
            }
        }
        RenderSurface surface = renderer.CurrentSurface!;
        if (surface.BeforeDrawOam != null)
        {
            MtxFx32 matrix = GetCurrentMtx();
            surface.BeforeDrawOam(renderer, surface, cell, oamIndex, matrix);
        }
    }

    // OFFSET 0x020A9CE0
    private static void Renderer_AfterOam(G2dRenderer core, CellData cell, ushort oamIndex)
    {
        RenderSurface surface = sCurrentRenderer!.CurrentSurface!;
        if (surface.AfterDrawOam != null)
        {
            MtxFx32 matrix = GetCurrentMtx();
            surface.AfterDrawOam(sCurrentRenderer, surface, cell, oamIndex, matrix);
        }
    }

    private static MtxCacheEntry? Renderer_GetCurrentMtxCache()
    {
        if (!IsSRTransformEnabled())
        {
            return null;
        }
        if (sDrawingMultiCell != 0)
        {
            MtxCacheEntry? cached = sMultiCellMtxCache[sMultiCellAnimIdx];
            if (cached == null)
            {
                Renderer_StoreCurrentMtxToCache();
                cached = sMtxCache[sMtxState[(ushort)sMtxStackPos].CacheIdx];
                sMultiCellMtxCache[sMultiCellAnimIdx] = cached;
            }
            return cached;
        }
        Renderer_StoreCurrentMtxToCache();
        return sMtxCache[sMtxState[(ushort)sMtxStackPos].CacheIdx];
    }

    private static void Renderer_DrawCellToCore(CellData cell)
    {
        if (sCurrentTransferHandle != NoTransferHandle)
        {
            RndCore_DrawCellVramTransfer(cell, sCurrentTransferHandle);
        }
        else
        {
            RndCore_DrawCell(cell);
        }
    }

    // OFFSET 0x020A9D74
    private static void Renderer_DrawCell(CellData cell)
    {
        G2dRenderer renderer = sCurrentRenderer!;
        RenderSurface? surface = renderer.TargetSurfaceList;
        if ((renderer.OpzHint & OpzHintSingleSurface) != 0)
        {
            if (surface!.Type != SurfaceType3D)
            {
                MtxCacheEntry? cache = Renderer_GetCurrentMtxCache();
                RndCore_SetCurrentMtx2D(GetCurrentMtx(), cache);
                Renderer_DrawCellToCore(cell);
            }
            else
            {
                RndCore_SetZ(renderer, sMtxZ[sMtxStackPos]);
                RndCore_SetCurrentMtx3D(GetCurrentMtx());
                Renderer_DrawCellToCore(cell);
            }
            return;
        }
        for (; surface != null; surface = surface.Next)
        {
            if (surface.Active == 0)
            {
                continue;
            }
            G2dRenderer current = sCurrentRenderer!;
            if (surface.Type != SurfaceType3D)
            {
                current.CurrentSurface = surface;
                RndCore_SetCurrentSurface(current, surface);
                RndCore_SetOamRegisterFuncs(current, surface.OamRegister, surface.OamAffineRegister);
                RndCore_BeginRendering(current);
                MtxCacheEntry? cache = Renderer_GetCurrentMtxCache();
                RndCore_SetCurrentMtx2D(GetCurrentMtx(), cache);
                Renderer_DrawCellToCore(cell);
                sCurrentRenderer!.CurrentSurface = null;
                RndCore_EndRendering();
            }
            else
            {
                current.CurrentSurface = surface;
                RndCore_SetCurrentSurface(current, surface);
                RndCore_BeginRendering(current);
                RndCore_SetZ(renderer, sMtxZ[sMtxStackPos]);
                RndCore_SetCurrentMtx3D(GetCurrentMtx());
                Renderer_DrawCellToCore(cell);
                sCurrentRenderer!.CurrentSurface = null;
                RndCore_EndRendering();
            }
        }
    }

    private static void Renderer_ApplySrt(SrtControl srt)
    {
        if ((srt.EnableFlag & SrtTrans) != 0)
        {
            int x = srt.Px;
            int y = srt.Py;
            if (RndCore_GetFlipH(sCurrentRenderer!))
            {
                x = -x;
            }
            if (RndCore_GetFlipV(sCurrentRenderer!))
            {
                y = -y;
            }
            Translate(x << Fx32.Shift, y << Fx32.Shift, 0);
        }
        if ((srt.EnableFlag & SrtRotZ) != 0)
        {
            RotZ(Render3D.Sin(srt.RotZ >> 4), Render3D.Cos(srt.RotZ >> 4));
        }
        if ((srt.EnableFlag & SrtScale) != 0)
        {
            Scale(srt.Sx, srt.Sy, Fx32.One);
        }
    }

    private static void Renderer_DrawCellOfAnimation(CellAnimation anim)
    {
        CellData? cell = GetCellData(anim.CellBank, anim.CurrentCell);
        if (cell == null)
        {
            return;
        }
        if (anim.TransferHandle != NoTransferHandle)
        {
            sCurrentTransferHandle = anim.TransferHandle;
            Renderer_DrawCell(cell);
            sCurrentTransferHandle = NoTransferHandle;
        }
        else
        {
            Renderer_DrawCell(cell);
        }
    }

    // OFFSET 0x020AA1C4
    private static void Renderer_DrawCellAnimation(CellAnimation anim)
    {
        if (anim.Srt.EnableFlag == 0)
        {
            Renderer_DrawCellOfAnimation(anim);
            return;
        }
        PushMtx();
        Renderer_ApplySrt(anim.Srt);
        Renderer_DrawCellOfAnimation(anim);
        PopMtx();
    }

    // OFFSET 0x020AA320
    private static void Renderer_DrawNode(G2dNode node)
    {
        if (node.Visible == 0)
        {
            return;
        }
        CellAnimation anim = node.Content!;
        PushMtx();
        Renderer_ApplySrt(node.Srt);
        Renderer_DrawCellAnimation(anim);
        PopMtx();
    }

    private static void Renderer_ResetMtxState()
    {
        sMtxCacheCount = 0;
        sMtxGroupCount = 0;
        Array.Clear(sMtxState);
    }

    // OFFSET 0x020AA3F4
    public static void InitRenderer(G2dRenderer renderer)
    {
        RndCore_Init(renderer);
        renderer.TargetSurfaceList = null;
        renderer.CurrentSurface = null;
        renderer.PaletteSwapTable = null;
        renderer.OpzHint = 0;
        renderer.SpriteZOffsetStep = 0;
        renderer.OverwriteEnableFlag = 0;
        renderer.OverwritePriority = 0;
        renderer.OverwritePlttNo = 0;
        renderer.OverwriteObjMode = 0;
        renderer.OverwriteMosaicFlag = 0;
        renderer.OverwritePlttNoOffset = 0;
        sMtxStackPos = 0;
        sFirstSRIndex = NoSRIndex;
        for (int i = 0; i < MtxCacheSize; i++)
        {
            ResetAffineIndices(sMtxCache[i]);
        }
        Renderer_ResetMtxState();
        sSimpleMatrixMode = 0;
    }

    // OFFSET 0x020AA4B0
    public static void AddRendererTargetSurface(G2dRenderer renderer, RenderSurface surface)
    {
        surface.Next = renderer.TargetSurfaceList;
        renderer.TargetSurfaceList = surface;
    }

    // OFFSET 0x020AA4C0
    public static void InitRenderSurface(RenderSurface surface)
    {
        surface.ViewRect = default;
        surface.OamRegister = null;
        surface.OamAffineRegister = null;
        surface.Next = null;
        surface.VisibilityCulling = null;
        surface.BeforeDrawCell = null;
        surface.AfterDrawCell = null;
        surface.BeforeDrawOam = null;
        surface.AfterDrawOam = null;
        surface.Active = 1;
        surface.Type = SurfaceTypeMax;
        surface.BeforeCell = Renderer_BeforeCell;
        surface.AfterCell = Renderer_AfterCell;
        surface.BeforeOam = Renderer_BeforeOam;
        surface.AfterOam = Renderer_AfterOam;
    }

    // OFFSET 0x020AA51C
    public static void BeginRendering(G2dRenderer renderer)
    {
        sCurrentRenderer = renderer;
        sMtxStackPos = 0;
        sFirstSRIndex = NoSRIndex;
        for (int i = 0; i < sMtxCacheCount; i++)
        {
            ResetAffineIndices(sMtxCache[i]);
        }
        Renderer_ResetMtxState();
        Render3D.PushMatrix();
        Render3D.Identity();
        Renderer_ResetCurrentMtx();
    }

    // OFFSET 0x020AA5C4
    public static void EndRendering()
    {
        Render3D.PopMatrix(1);
        G2dRenderer renderer = sCurrentRenderer!;
        if (renderer.OpzHint != 0)
        {
            if ((renderer.OpzHint & OpzHintSimpleMatrix) != 0)
            {
                sSimpleMatrixMode = 0;
            }
            if ((renderer.OpzHint & OpzHintSingleSurface) != 0)
            {
                renderer.CurrentSurface = null;
                RndCore_EndRendering();
            }
            sCurrentRenderer!.OpzHint = 0;
        }
        sCurrentRenderer = null;
    }

    // OFFSET 0x020AA630
    public static void DrawCellAnimation(CellAnimation anim)
    {
        if (sCurrentRenderer!.SpriteZOffsetStep != 0)
        {
            int previousStep = SoftSprite_GetAutoZOffsetStep();
            SoftSprite_SetAutoZOffsetEnabled(1);
            SoftSprite_SetAutoZOffsetStep(sCurrentRenderer.SpriteZOffsetStep);
            Renderer_DrawCellAnimation(anim);
            SoftSprite_SetAutoZOffsetEnabled(0);
            SoftSprite_SetAutoZOffsetStep(previousStep);
            SoftSprite_ResetAutoZOffset();
            return;
        }
        Renderer_DrawCellAnimation(anim);
    }

    private static void Renderer_DrawMultiCellContents(MultiCellAnimation anim)
    {
        byte[] file = anim.MultiCellBank!.File;
        int bankData = anim.MultiCellBank.SectionOffset + BlockHeaderSize;
        int multiCell = anim.CurrentMultiCell;
        int numNodes = Bin.U16(file, multiCell);
        if (anim.McType != McTypeShareCellAnim)
        {
            for (int i = 0; i < numNodes; i++)
            {
                Renderer_DrawNode(anim.Nodes![i]);
            }
            return;
        }
        int hierarchy = bankData + (int)Bin.U32(file, bankData + 8) + (int)Bin.U32(file, multiCell + 4);
        for (int i = 0; i < numNodes; i++)
        {
            int nodeInfo = hierarchy + i * HierarchySize;
            ushort cellAnimIdx = (ushort)((Bin.U16(file, nodeInfo + 6) & 0xFF00) >> 8);
            sMultiCellAnimIdx = cellAnimIdx;
            PushMtx();
            int x = (short)Bin.U16(file, nodeInfo + 2);
            int y = (short)Bin.U16(file, nodeInfo + 4);
            if (RndCore_GetFlipH(sCurrentRenderer!))
            {
                x = -x;
            }
            if (RndCore_GetFlipV(sCurrentRenderer!))
            {
                y = -y;
            }
            Translate(x << Fx32.Shift, y << Fx32.Shift, 0);
            Renderer_DrawCellAnimation(anim.SharedCellAnims![cellAnimIdx]);
            PopMtx();
        }
    }

    // OFFSET 0x020AA698
    private static void Renderer_DrawMultiCell(MultiCellAnimation anim)
    {
        if (anim.CurrentMultiCell == 0)
        {
            return;
        }
        if (anim.McType == McTypeShareCellAnim)
        {
            int numCellAnims = Bin.U16(anim.MultiCellBank!.File, anim.CurrentMultiCell + 2);
            for (int i = 0; i < numCellAnims; i++)
            {
                sMultiCellMtxCache[i] = null;
            }
            sDrawingMultiCell = 1;
        }
        if (sCurrentRenderer!.SpriteZOffsetStep != 0)
        {
            int previousStep = SoftSprite_GetAutoZOffsetStep();
            SoftSprite_SetAutoZOffsetEnabled(1);
            SoftSprite_SetAutoZOffsetStep(sCurrentRenderer.SpriteZOffsetStep);
            Renderer_DrawMultiCellContents(anim);
            SoftSprite_SetAutoZOffsetEnabled(0);
            SoftSprite_SetAutoZOffsetStep(previousStep);
            SoftSprite_ResetAutoZOffset();
        }
        else
        {
            Renderer_DrawMultiCellContents(anim);
        }
        if (anim.McType == McTypeShareCellAnim)
        {
            sDrawingMultiCell = 0;
        }
    }

    // OFFSET 0x020AA96C
    public static void DrawMultiCellAnimation(MultiCellAnimation anim)
    {
        if (anim.Srt.EnableFlag == 0)
        {
            Renderer_DrawMultiCell(anim);
            return;
        }
        PushMtx();
        Renderer_ApplySrt(anim.Srt);
        Renderer_DrawMultiCell(anim);
        PopMtx();
    }

    private static void Renderer_CopyMtx(int from, int to)
    {
        if (sSimpleMatrixMode != 0)
        {
            sMtxStack[to].M20 = sMtxStack[from].M20;
            sMtxStack[to].M21 = sMtxStack[from].M21;
            sMtxZ[to] = sMtxZ[from];
            return;
        }
        sMtxStack[to] = sMtxStack[from];
        sMtxStack2D[to] = sMtxStack2D[from];
        sMtxZ[to] = sMtxZ[from];
    }

    // OFFSET 0x020AAA48
    public static void PushMtx()
    {
        if ((sCurrentRenderer!.OpzHint & OpzHintSimpleMatrix) != 0)
        {
            int next = sMtxStackPos + 1;
            if (next < MtxStackSize)
            {
                Renderer_CopyMtx(sMtxStackPos, next);
                sMtxStackPos = next;
            }
            return;
        }
        ushort previous = (ushort)sMtxStackPos;
        int pushed = sMtxStackPos + 1;
        if (pushed < MtxStackSize)
        {
            Renderer_CopyMtx(sMtxStackPos, pushed);
            sMtxStackPos = pushed;
        }
        ushort current = (ushort)sMtxStackPos;
        MtxState source = sMtxState[previous];
        sMtxState[current].CacheIdx = source.CacheIdx;
        sMtxState[current].GroupID = source.GroupID;
        sMtxState[current].State = source.State;
        sMtxState[current].Padding = source.Padding;
        if (source.State == MtxStateChanged)
        {
            sMtxState[current].State = MtxStateInherited;
        }
        else
        {
            sMtxState[current].State = source.State;
        }
    }

    // OFFSET 0x020AAC6C
    public static void PopMtx()
    {
        if (sMtxStackPos <= 0)
        {
            return;
        }
        sMtxStackPos--;
        if (sFirstSRIndex > sMtxStackPos)
        {
            sFirstSRIndex = NoSRIndex;
        }
    }

    // OFFSET 0x020AACBC
    public static void Translate(int x, int y, int z)
    {
        if (sSimpleMatrixMode != 0)
        {
            sMtxStack[sMtxStackPos].M20 += x;
            sMtxZ[sMtxStackPos] += z;
            sMtxStack[sMtxStackPos].M21 += y;
            return;
        }
        MtxFx32 translation = new() { M00 = Fx32.One, M11 = Fx32.One, M20 = x, M21 = y };
        MtxFx32_Concat(translation, sMtxStack[sMtxStackPos], out sMtxStack[sMtxStackPos]);
        MtxFx32_Concat(translation, sMtxStack2D[sMtxStackPos], out sMtxStack2D[sMtxStackPos]);
        sMtxZ[sMtxStackPos] += z;
    }

    private static void Renderer_MarkSRTransform()
    {
        if (!IsSRTransformEnabled())
        {
            sFirstSRIndex = (ushort)sMtxStackPos;
        }
        ref MtxState state = ref sMtxState[(ushort)sMtxStackPos];
        switch (state.State)
        {
            case MtxStateNone:
            case MtxStateInherited:
            case MtxStateCached:
                state.GroupID = sMtxGroupCount++;
                state.State = MtxStateChanged;
                break;
        }
    }

    // OFFSET 0x020AADB8
    public static void Scale(int x, int y, int z)
    {
        MtxFx32 scale = new() { M00 = x, M11 = y };
        MtxFx32_Concat(scale, sMtxStack[sMtxStackPos], out sMtxStack[sMtxStackPos]);
        scale.M00 = Fx32.Div(Fx32.One, x);
        scale.M01 = 0;
        scale.M10 = 0;
        scale.M11 = Fx32.Div(Fx32.One, y);
        scale.M20 = 0;
        scale.M21 = 0;
        MtxFx32_Concat(scale, sMtxStack2D[sMtxStackPos], out sMtxStack2D[sMtxStackPos]);
        Renderer_MarkSRTransform();
    }

    // OFFSET 0x020AAF0C
    public static void RotZ(int sin, int cos)
    {
        MtxFx32 rotation = new() { M00 = cos, M01 = sin, M10 = -sin, M11 = cos };
        MtxFx32_Concat(rotation, sMtxStack[sMtxStackPos], out sMtxStack[sMtxStackPos]);
        MtxFx32_Concat(rotation, sMtxStack2D[sMtxStackPos], out sMtxStack2D[sMtxStackPos]);
        Renderer_MarkSRTransform();
    }

    // OFFSET 0x020AB02C
    private static ushort[]? Renderer_GetPaletteSwapTable(G2dRenderer renderer)
    {
        return renderer.PaletteSwapTable;
    }

    // OFFSET 0x020AB034
    public static void SetRendererImageProxy(G2dRenderer renderer, ImageProxy imageProxy, ImagePaletteProxy paletteProxy)
    {
        RndCore_SetImageProxy(renderer, imageProxy, paletteProxy);
    }

    // OFFSET 0x020AB040
    private static void MtxFx32_Concat(in MtxFx32 a, in MtxFx32 b, out MtxFx32 result)
    {
        MtxFx32 product;
        product.M00 = (int)(((long)a.M01 * b.M10 + (long)a.M00 * b.M00) >> Fx32.Shift);
        product.M01 = (int)(((long)a.M01 * b.M11 + (long)a.M00 * b.M01) >> Fx32.Shift);
        product.M10 = (int)(((long)a.M11 * b.M10 + (long)a.M10 * b.M00) >> Fx32.Shift);
        product.M11 = (int)(((long)a.M11 * b.M11 + (long)a.M10 * b.M01) >> Fx32.Shift);
        product.M20 = b.M20 + (int)(((long)a.M21 * b.M10 + (long)a.M20 * b.M00) >> Fx32.Shift);
        product.M21 = b.M21 + (int)(((long)a.M21 * b.M11 + (long)a.M20 * b.M01) >> Fx32.Shift);
        result = product;
    }

    // OFFSET 0x020AB6D8
    private static void SoftSprite_SetAutoZOffsetEnabled(int enabled)
    {
        sAutoZEnabled = enabled;
    }

    // OFFSET 0x020AB6E8
    private static void SoftSprite_ResetAutoZOffset()
    {
        sAutoZOffset = 0;
    }

    // OFFSET 0x020AB6FC
    private static void SoftSprite_SetAutoZOffsetStep(int step)
    {
        sAutoZStep = step;
    }

    // OFFSET 0x020AB70C
    private static int SoftSprite_GetAutoZOffsetStep()
    {
        return sAutoZStep;
    }

    private static int OamWidth(uint attr)
    {
        uint shapeSize = attr & OamShapeSizeMask;
        return sOamWidths[(int)((shapeSize & 0xC000) >> 14) * 4 + (int)((shapeSize & 0xC0000000) >> 30)];
    }

    private static int OamHeight(uint attr)
    {
        uint shapeSize = attr & OamShapeSizeMask;
        return sOamHeights[(int)((shapeSize & 0xC000) >> 14) * 4 + (int)((shapeSize & 0xC0000000) >> 30)];
    }

    private static uint OamAffineMode(uint attr)
    {
        uint mode = attr & OamAffineModeMask;
        if (mode != OamAffineNormal && mode != OamAffineDouble)
        {
            mode |= attr & 0x30000000;
        }
        return mode;
    }

    private static int OamX(uint attr)
    {
        int x = (int)((attr & 0x01FF0000) >> 16);
        if (x > 0xFF)
        {
            x = (short)(x | 0xFF00);
        }
        return x;
    }

    private static int OamY(uint attr)
    {
        int y = (int)(attr & 0xFF);
        if (y > 0x7F)
        {
            y = (short)(y | 0xFF00);
        }
        return y;
    }

    // OFFSET 0x020AB96C
    private static void RndCore_TransformOamPosition(in MtxFx22 matrix, ref uint attr, int baseX, int baseY)
    {
        int x = OamX(attr) << Fx32.Shift;
        int y = OamY(attr) << Fx32.Shift;
        if (OamAffineMode(attr) == OamAffineDouble)
        {
            x += OamWidth(attr) << 11;
            y += OamHeight(attr) << 11;
        }
        int transformedX = (int)(((long)matrix.M10 * y + 0x1000 + (long)matrix.M00 * x) >> Fx32.Shift);
        int transformedY = (int)(((long)matrix.M11 * y + 0x1000 + (long)matrix.M01 * x) >> Fx32.Shift);
        x = transformedX;
        y = transformedY;
        int overwriteMode = sCoreInstance!.AffineOverwriteMode;
        if (overwriteMode != AffineOverwriteNone)
        {
            uint mode = overwriteMode == AffineOverwriteDouble ? OamAffineDouble : OamAffineNormal;
            attr = (attr & 0xC1FFFCFF) | mode;
        }
        uint affineMode = OamAffineMode(attr);
        int halfWidth = OamWidth(attr) >> 1;
        int halfHeight = OamHeight(attr) >> 1;
        x += matrix.M00 * halfWidth - (halfWidth << Fx32.Shift) + matrix.M10 * halfHeight;
        y += matrix.M01 * halfWidth - (halfHeight << Fx32.Shift) + matrix.M11 * halfHeight;
        if (affineMode == OamAffineDouble)
        {
            x -= halfWidth << Fx32.Shift;
            y -= halfHeight << Fx32.Shift;
        }
        x += baseX;
        y += baseY;
        attr = (attr & 0xFE00FF00) | (uint)((y >> Fx32.Shift) & 0xFF) | (((uint)(x >> Fx32.Shift) << 23) >> 7);
    }

    private static void RndCore_LoadOam(G2dRenderer core, CellData cell, int index)
    {
        int oam = cell.OamArray + index * OamAttrSize;
        uint attr0 = (uint)Bin.U16(cell.File, oam);
        uint attr1 = (uint)Bin.U16(cell.File, oam + 2);
        uint attr2 = (uint)Bin.U16(cell.File, oam + 4);
        core.CurrentOam01 = attr0 | (attr1 << 16);
        core.CurrentOam23 = (core.CurrentOam23 & 0xFFFF0000) | attr2;
    }

    private static uint RndCore_FlipOamAttributes(G2dRenderer core, uint attr)
    {
        uint flipH = (RndCore_GetFlipH(core) ? 1u : 0u) ^ ((attr << 3) >> 31);
        uint flipV = (RndCore_GetFlipV(core) ? 1u : 0u) ^ ((attr << 2) >> 31);
        attr = (attr & ~OamFlipH) | (flipH << 28);
        attr = (attr & ~OamFlipV) | (flipV << 29);
        return attr;
    }

    // OFFSET 0x020ABC2C
    private static void RndCore_DrawCellToSurface2D(RenderSurface surface, CellData cell)
    {
        G2dRenderer core = sCoreInstance!;
        MtxFx32 matrix = core.HasCurrentMtx ? core.CurrentMtx : IdentityMtx();
        int baseX = matrix.M20 - surface.ViewRect.X;
        int baseY = matrix.M21 - surface.ViewRect.Y;
        uint baseCharOffset = sCoreInstance!.Base2DCharOffset;
        bool affine = sCoreInstance.FlipFlag == 0 && sCoreInstance.CurrentMtxCache != null;
        int numOamAttrs = cell.NumOamAttrs;
        for (ushort i = 0; i < numOamAttrs; i++)
        {
            sCoreInstance!.DrawEnable = 1;
            RndCore_LoadOam(core, cell, i);
            if (surface.BeforeOam != null)
            {
                surface.BeforeOam(sCoreInstance, cell, i);
            }
            if (sCoreInstance.DrawEnable == 0)
            {
                continue;
            }
            core.CurrentOam23 = (core.CurrentOam23 & 0xFFFFFC00) | ((baseCharOffset + ((core.CurrentOam23 << 22) >> 22)) & 0x3FF);
            ushort affineIndex;
            if (affine)
            {
                uint attr = core.CurrentOam01;
                int flip = (int)(((attr << 3) >> 31) | (((attr << 2) >> 31) << 1));
                MtxCacheEntry cache = sCoreInstance.CurrentMtxCache!;
                int slot = flip * 2 + surface.Type - 1;
                affineIndex = cache.AffineIndex[slot];
                if (affineIndex == AffineIndexUnregistered)
                {
                    if (flip == 0)
                    {
                        affineIndex = sCoreInstance.OamAffineRegister!(cache.Matrix);
                    }
                    else
                    {
                        MtxFx22 flipped = cache.Matrix;
                        if ((flip & 1) != 0)
                        {
                            flipped.M00 = -flipped.M00;
                            flipped.M01 = -flipped.M01;
                        }
                        if ((flip & 2) != 0)
                        {
                            flipped.M10 = -flipped.M10;
                            flipped.M11 = -flipped.M11;
                        }
                        affineIndex = sCoreInstance.OamAffineRegister!(flipped);
                    }
                    cache.AffineIndex[slot] = affineIndex;
                }
                MtxFx22 rotation = new() { M00 = matrix.M00, M01 = matrix.M01, M10 = matrix.M10, M11 = matrix.M11 };
                RndCore_TransformOamPosition(rotation, ref core.CurrentOam01, baseX, baseY);
            }
            else
            {
                affineIndex = AffineIndexNone;
                if (sCoreInstance.FlipFlag != 0)
                {
                    uint original = core.CurrentOam01;
                    core.CurrentOam01 = RndCore_FlipOamAttributes(sCoreInstance, core.CurrentOam01);
                    if (RndCore_GetFlipH(sCoreInstance))
                    {
                        uint attr = core.CurrentOam01;
                        uint x = (uint)-(OamWidth(original) + (int)((attr << 7) >> 23));
                        core.CurrentOam01 = (attr & 0xFE00FFFF) | ((x << 23) >> 7);
                    }
                    if (RndCore_GetFlipV(sCoreInstance))
                    {
                        uint attr = core.CurrentOam01;
                        uint y = (uint)-(OamHeight(original) + (int)((attr << 24) >> 24));
                        core.CurrentOam01 = (attr & ~0xFFu) | (y & 0xFF);
                    }
                }
                uint positioned = core.CurrentOam01;
                uint movedX = (uint)((baseX >> Fx32.Shift) + (int)((positioned << 7) >> 23));
                positioned = (positioned & 0xFE00FFFF) | ((movedX << 23) >> 7);
                uint movedY = (uint)((baseY >> Fx32.Shift) + (int)((positioned << 24) >> 24));
                positioned = (positioned & ~0xFFu) | (movedY & 0xFF);
                core.CurrentOam01 = positioned;
            }
            OamAttributes oam = new()
            {
                Attr0 = (ushort)core.CurrentOam01,
                Attr1 = (ushort)(core.CurrentOam01 >> 16),
                Attr2 = (ushort)core.CurrentOam23,
            };
            if (!sCoreInstance.OamRegister!(oam, affineIndex))
            {
                return;
            }
            if (surface.AfterOam != null)
            {
                surface.AfterOam(sCoreInstance, cell, i);
            }
        }
    }

    // OFFSET 0x020AC354
    private static void RndCore_Init(G2dRenderer core)
    {
        core.CurrentTargetSurface = null;
        core.AffineOverwriteMode = AffineOverwriteDouble;
        core.ImageProxy = null;
        core.PaletteProxy = null;
        core.Base2DCharOffset = 0;
        core.BaseTexAddr3D = 0;
        core.BasePltAddr3D = 0;
        core.OamRegister = null;
        core.OamAffineRegister = null;
        core.FlipFlag = 0;
        core.CurrentMtxCache = null;
        core.HasCurrentMtx = false;
        core.CurrentMtx = default;
        core.ZFor3D = 0;
        core.CurrentOam01 = 0;
        core.CurrentOam23 = 0;
        Array.Clear(core.MtxFor3D);
        core.DrawEnable = 1;
    }

    // OFFSET 0x020AC394
    private static void RndCore_SetImageProxy(G2dRenderer core, ImageProxy imageProxy, ImagePaletteProxy paletteProxy)
    {
        core.ImageProxy = imageProxy;
        core.PaletteProxy = paletteProxy;
    }

    // OFFSET 0x020AC3A0
    private static void RndCore_SetOamRegisterFuncs(G2dRenderer core, OamRegisterFunc? oamRegister, OamAffineRegisterFunc? affineRegister)
    {
        core.OamRegister = oamRegister;
        core.OamAffineRegister = affineRegister;
    }

    // OFFSET 0x020AC3AC
    public static void SetRendererAffineOverwriteMode(G2dRenderer core, int mode)
    {
        core.AffineOverwriteMode = mode;
    }

    // OFFSET 0x020AC3B4
    private static void RndCore_SetCurrentMtx3D(in MtxFx32 matrix)
    {
        G2dRenderer core = sCoreInstance!;
        core.CurrentMtx = matrix;
        core.HasCurrentMtx = true;
        core.MtxFor3D[0] = matrix.M00;
        core.MtxFor3D[1] = matrix.M01;
        core.MtxFor3D[2] = 0;
        core.MtxFor3D[3] = matrix.M10;
        core.MtxFor3D[4] = matrix.M11;
        core.MtxFor3D[5] = 0;
        core.MtxFor3D[6] = 0;
        core.MtxFor3D[7] = 0;
        core.MtxFor3D[8] = Fx32.One;
        core.MtxFor3D[9] = matrix.M20;
        core.MtxFor3D[10] = matrix.M21;
        core.MtxFor3D[11] = core.ZFor3D;
    }

    // OFFSET 0x020AC41C
    private static void RndCore_SetCurrentMtx2D(in MtxFx32 matrix, MtxCacheEntry? cache)
    {
        sCoreInstance!.CurrentMtx = matrix;
        sCoreInstance.HasCurrentMtx = true;
        sCoreInstance.CurrentMtxCache = cache;
    }

    // OFFSET 0x020AC434
    private static void RndCore_SetZ(G2dRenderer core, int z)
    {
        core.ZFor3D = z;
    }

    // OFFSET 0x020AC43C
    private static void RndCore_SetCurrentSurface(G2dRenderer core, RenderSurface surface)
    {
        core.CurrentTargetSurface = surface;
    }

    // OFFSET 0x020AC444
    private static bool RndCore_GetFlipH(G2dRenderer core)
    {
        return (core.FlipFlag & 1) != 0;
    }

    // OFFSET 0x020AC458
    private static bool RndCore_GetFlipV(G2dRenderer core)
    {
        return (core.FlipFlag & 2) != 0;
    }

    // OFFSET 0x020AC46C
    public static void SetRendererFlipMode(G2dRenderer core, int flipH, int flipV)
    {
        if (flipH != 0)
        {
            core.FlipFlag |= 1;
        }
        else
        {
            core.FlipFlag &= ~1;
        }
        if (flipV != 0)
        {
            core.FlipFlag |= 2;
        }
        else
        {
            core.FlipFlag &= ~2;
        }
    }

    // OFFSET 0x020AC498
    private static void RndCore_BeginRendering(G2dRenderer core)
    {
        sCoreInstance = core;
        RenderSurface surface = core.CurrentTargetSurface!;
        int type = surface.Type;
        if (type == SurfaceType3D)
        {
            Render3D.PushProjection();
            Render3D.TranslateProjection(-surface.ViewRect.X, -surface.ViewRect.Y, 0);
            ImageProxy? texture = sCoreInstance.ImageProxy;
            if (texture != null && G2d.GetImageLocation(texture, G2d.Vram3D) != -1)
            {
                core.BaseTexAddr3D = (uint)G2d.GetImageLocation(texture, G2d.Vram3D);
            }
            else
            {
                core.BaseTexAddr3D = 0;
            }
            ImagePaletteProxy? texturePalette = sCoreInstance.PaletteProxy;
            if (texturePalette != null && G2d.GetImagePaletteLocation(texturePalette, G2d.Vram3D) != -1)
            {
                core.BasePltAddr3D = (uint)G2d.GetImagePaletteLocation(texturePalette, G2d.Vram3D);
            }
            else
            {
                core.BasePltAddr3D = 0;
            }
            return;
        }
        ImageProxy? image = core.ImageProxy;
        uint baseCharOffset = 0;
        if (image != null && G2d.GetImageLocation(image, type) != -1)
        {
            uint location = (uint)G2d.GetImageLocation(image, type);
            uint mappingType = image.Character != null ? image.Character.MappingType : 0;
            int shift = (int)((mappingType & 0x700000) >> 20) + 5;
            baseCharOffset = location >> shift;
        }
        sCoreInstance.Base2DCharOffset = baseCharOffset;
    }

    // OFFSET 0x020AC5A4
    private static void RndCore_EndRendering()
    {
        G2dRenderer core = sCoreInstance!;
        if (core.CurrentTargetSurface!.Type == SurfaceType3D)
        {
            Render3D.PopProjection();
        }
        else
        {
            core.Base2DCharOffset = 0;
            sCoreInstance!.HasCurrentMtx = false;
            sCoreInstance.CurrentMtxCache = null;
        }
        sCoreInstance = null;
    }

    private static void RndCore_DrawCellToSurface(RenderSurface surface, CellData cell)
    {
        switch (surface.Type)
        {
            case SurfaceType3D:
                sub_020AC28C(surface, cell);
                break;
            case SurfaceTypeMain2D:
            case SurfaceTypeSub2D:
                RndCore_DrawCellToSurface2D(surface, cell);
                break;
        }
    }

    // OFFSET 0x020AC608
    private static void RndCore_DrawCell(CellData cell)
    {
        RenderSurface surface = sCoreInstance!.CurrentTargetSurface!;
        if (surface.Active == 0)
        {
            return;
        }
        sCoreInstance.DrawEnable = 1;
        if (surface.BeforeCell != null)
        {
            surface.BeforeCell(sCoreInstance, cell);
        }
        if (sCoreInstance!.DrawEnable != 0)
        {
            RndCore_DrawCellToSurface(surface, cell);
        }
        if (surface.AfterCell != null)
        {
            surface.AfterCell(sCoreInstance!, cell);
        }
    }

    // OFFSET 0x020AC6BC
    private static void RndCore_DrawCellVramTransfer(CellData cell, uint transferHandle)
    {
        RenderSurface surface = sCoreInstance!.CurrentTargetSurface!;
        if (surface.Active == 0)
        {
            return;
        }
        sCoreInstance.DrawEnable = 1;
        if (surface.BeforeCell != null)
        {
            surface.BeforeCell(sCoreInstance, cell);
        }
        if (sCoreInstance!.DrawEnable != 0)
        {
            if (transferHandle != NoTransferHandle)
            {
                CellTransferState state = GetCellTransferState(transferHandle);
                state.DrawnFlags = (state.DrawnFlags & ~(1u << surface.Type)) | (1u << surface.Type);
            }
            RndCore_DrawCellToSurface(surface, cell);
        }
        if (surface.AfterCell != null)
        {
            surface.AfterCell(sCoreInstance!, cell);
        }
    }
}
