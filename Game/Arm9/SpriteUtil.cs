using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public struct ViewRect
{
    public int X;
    public int Y;
    public int Width;
    public int Height;
}

public sealed class RenderSurface
{
    public ViewRect ViewRect;
    public int Active;
    public int Type;
    public RndCoreCellCallback? BeforeCell;
    public RndCoreCellCallback? AfterCell;
    public RndCoreOamCallback? BeforeOam;
    public RndCoreOamCallback? AfterOam;
    public OamRegisterFunc? OamRegister;
    public OamAffineRegisterFunc? OamAffineRegister;
    public RenderSurface? Next;
    public SurfaceCullFunc? VisibilityCulling;
    public RenderCellCallback? BeforeDrawCell;
    public RenderCellCallback? AfterDrawCell;
    public RenderOamCallback? BeforeDrawOam;
    public RenderOamCallback? AfterDrawOam;
}

public sealed class G2dRenderer
{
    public readonly RenderSurface[] Surfaces = { new RenderSurface(), new RenderSurface() };
    public RenderSurface? CurrentTargetSurface;
    public int AffineOverwriteMode;
    public ImageProxy? ImageProxy;
    public ImagePaletteProxy? PaletteProxy;
    public uint Base2DCharOffset;
    public uint BaseTexAddr3D;
    public uint BasePltAddr3D;
    public OamRegisterFunc? OamRegister;
    public OamAffineRegisterFunc? OamAffineRegister;
    public int FlipFlag;
    public MtxCacheEntry? CurrentMtxCache;
    public bool HasCurrentMtx;
    public MtxFx32 CurrentMtx;
    public int DrawEnable;
    public int ZFor3D;
    public uint CurrentOam01;
    public uint CurrentOam23;
    public readonly int[] MtxFor3D = new int[12];
    public RenderSurface? TargetSurfaceList;
    public RenderSurface? CurrentSurface;
    public ushort[]? PaletteSwapTable;
    public uint OpzHint;
    public int SpriteZOffsetStep;
    public uint OverwriteEnableFlag;
    public ushort OverwritePriority;
    public ushort OverwritePlttNo;
    public int OverwriteObjMode;
    public int OverwriteMosaicFlag;
    public ushort OverwritePlttNoOffset;
}

public sealed class SpriteListParams
{
    public int MaxElements;
    public G2dRenderer? Renderer;
    public int HeapID;
}

public sealed class SpriteResourcesHeader
{
    public ImageProxy? ImageProxy;
    public CharacterData? CharData;
    public ImagePaletteProxy? PaletteProxy;
    public CellDataBank? CellBank;
    public AnimBankData? CellAnimBank;
    public MultiCellDataBank? MultiCellBank;
    public MultiCellAnimBankData? MultiCellAnimBank;
    public int IsVRamTransfer;
    public byte Priority;
}

public sealed class SpriteResourcesHeaderList
{
    public SpriteResourcesHeader[]? Headers;
    public int Count;
}

public static class SpriteUtil
{
    private const int ScreenWidth = 255 << 12;
    private const int ScreenHeight = 192 << 12;
    private const int ResdatEntrySize = 0x20;
    private const int EndOfResdat = -2;
    private const int MainScreen = 1;
    private const int SubScreen = 2;
    private const int ObjMapping1D32K = 0x10;
    private const int ObjMapping1D64K = 0x100010;
    private const int ObjMapping1D128K = 0x200010;

    // OFFSET 0x020093B4
    public static void SpriteResourcesHeader_Init(
        SpriteResourcesHeader? header,
        int charResourceID,
        int plttResourceID,
        int cellResourceID,
        int animResourceID,
        int multiCellResourceID,
        int multiAnimResourceID,
        int vramTransfer,
        int priority,
        SpriteResourceCollection? charResources,
        SpriteResourceCollection? plttResources,
        SpriteResourceCollection? cellResources,
        SpriteResourceCollection? animResources,
        SpriteResourceCollection? multiCellResources,
        SpriteResourceCollection? multiAnimResources)
    {
        SpriteResource? animResource = null;
        SpriteResource? multiCellResource = null;
        SpriteResource? multiAnimResource = null;
        if (charResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (plttResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (animResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (cellResources == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (header == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResource? charResource = SpriteResourceCollection.SpriteResourceCollection_Find(charResources, charResourceID);
        if (charResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResource? plttResource = SpriteResourceCollection.SpriteResourceCollection_Find(plttResources, plttResourceID);
        if (plttResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteResource? cellResource = SpriteResourceCollection.SpriteResourceCollection_Find(cellResources, cellResourceID);
        if (cellResource == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (animResources != null && animResourceID != -1)
        {
            animResource = SpriteResourceCollection.SpriteResourceCollection_Find(animResources, animResourceID);
            if (animResource == null)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        if (multiCellResources != null)
        {
            if (multiCellResourceID != -1)
            {
                multiCellResource = SpriteResourceCollection.SpriteResourceCollection_Find(multiCellResources, multiCellResourceID);
            }
            if (multiAnimResourceID != -1)
            {
                multiAnimResource = SpriteResourceCollection.SpriteResourceCollection_Find(multiAnimResources, multiAnimResourceID);
            }
        }
        ImageProxy? imageProxy;
        if (vramTransfer != 0)
        {
            imageProxy = SpriteTransfer.SpriteTransfer_GetCellTransferProxy(charResource!, cellResource!);
            if (imageProxy == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            header!.CharData = SpriteResource.SpriteResource_GetTileData(charResource!);
        }
        else
        {
            imageProxy = SpriteTransfer.SpriteTransfer_GetImageProxy(charResource!);
            if (imageProxy == null)
            {
                ErrorHandling.GF_AssertFail();
            }
            header!.CharData = null;
        }
        header.PaletteProxy = SpriteTransfer.SpriteTransfer_GetPaletteProxy(plttResource!, imageProxy);
        header.ImageProxy = imageProxy;
        header.CellBank = SpriteResource.SpriteResource_GetSpriteData(cellResource!);
        header.CellAnimBank = animResource != null ? SpriteResource.SpriteResource_GetSpriteAnimData(animResource) : null;
        if (multiCellResource != null)
        {
            header.MultiCellBank = SpriteResource.SpriteResource_GetMultiSpriteData(multiCellResource);
            header.MultiCellAnimBank = SpriteResource.SpriteResource_GetMultiSpriteAnimData(multiAnimResource!);
        }
        else
        {
            header.MultiCellBank = null;
            header.MultiCellAnimBank = null;
        }
        header.IsVRamTransfer = vramTransfer;
        header.Priority = (byte)priority;
    }

    // OFFSET 0x020094F0
    public static void SpriteResourcesHeader_Clear(SpriteResourcesHeader header)
    {
        SpriteTransfer.SpriteTransfer_DeleteCharTransfer(header.ImageProxy);
        header.ImageProxy = null;
        header.CharData = null;
        header.PaletteProxy = null;
        header.CellBank = null;
        header.CellAnimBank = null;
        header.MultiCellBank = null;
        header.MultiCellAnimBank = null;
        header.IsVRamTransfer = 0;
        header.Priority = 0;
    }

    // OFFSET 0x02009508
    public static SpriteResourcesHeaderList SpriteResourcesHeaderList_NewFromResdat(
        byte[] resdat,
        int heapID,
        SpriteResourceCollection? charResources,
        SpriteResourceCollection? plttResources,
        SpriteResourceCollection? cellResources,
        SpriteResourceCollection? animResources,
        SpriteResourceCollection? multiCellResources,
        SpriteResourceCollection? multiAnimResources)
    {
        int count = 0;
        while (Bin.I32(resdat, count * ResdatEntrySize) != EndOfResdat)
        {
            count++;
        }
        SpriteResourcesHeaderList list = new();
        list.Headers = new SpriteResourcesHeader[count];
        list.Count = count;
        for (int i = 0; i < list.Count; i++)
        {
            int entry = i * ResdatEntrySize;
            list.Headers[i] = new SpriteResourcesHeader();
            SpriteResourcesHeader_Init(
                list.Headers[i],
                Bin.I32(resdat, entry),
                Bin.I32(resdat, entry + 4),
                Bin.I32(resdat, entry + 8),
                Bin.I32(resdat, entry + 0xC),
                Bin.I32(resdat, entry + 0x10),
                Bin.I32(resdat, entry + 0x14),
                Bin.I32(resdat, entry + 0x18),
                Bin.I32(resdat, entry + 0x1C),
                charResources,
                plttResources,
                cellResources,
                animResources,
                multiCellResources,
                multiAnimResources);
        }
        return list;
    }

    // OFFSET 0x020095A8
    public static void SpriteResourcesHeaderList_Free(SpriteResourcesHeaderList? list)
    {
        if (list == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        list!.Headers = null;
    }

    // OFFSET 0x020095C4
    public static SpriteList SpriteList_InitRendering(int maxElements, G2dRenderer renderer, int heapID)
    {
        RenderView.RenderView_InitRenderer(renderer, -0x1000);
        ViewRect mainRect = new() { X = 0, Y = 0, Width = ScreenWidth, Height = ScreenHeight };
        RenderOam.RenderOam_InitSurface(renderer.Surfaces[0], mainRect, MainScreen, renderer);
        ViewRect subRect = new() { X = 0, Y = ScreenHeight, Width = ScreenWidth, Height = ScreenHeight };
        RenderOam.RenderOam_InitSurface(renderer.Surfaces[1], subRect, SubScreen, renderer);
        SpriteListParams listParams = new() { MaxElements = maxElements, Renderer = renderer, HeapID = heapID };
        return Sprite.SpriteList_New(listParams);
    }

    // OFFSET 0x0200962C
    public static void SetMainScreenViewRect(G2dRenderer renderer, int x, int y)
    {
        ViewRect rect = new() { X = x, Y = y, Width = ScreenWidth, Height = ScreenHeight };
        RenderView.RenderView_SetViewRect(renderer.Surfaces[0], rect);
    }

    // OFFSET 0x0200964C
    public static void SetSubScreenViewRect(G2dRenderer renderer, int x, int y)
    {
        ViewRect rect = new() { X = x, Y = y, Width = ScreenWidth, Height = ScreenHeight };
        RenderView.RenderView_SetViewRect(renderer.Surfaces[1], rect);
    }

    // OFFSET 0x0200966C
    public static void ReserveVramForWirelessIconChars(int vramType, int mapping)
    {
        switch (mapping)
        {
            case ObjMapping1D32K:
                if (Video.ObjectBank == 0x40 || Video.ObjectBank == 0x20)
                {
                    CharTransfer.CharTransfer_ReserveVramRange(0x3E00, 0x200, vramType);
                }
                else
                {
                    CharTransfer.CharTransfer_ReserveVramRange(0x7E00, 0x200, vramType);
                }
                break;
            case ObjMapping1D64K:
                CharTransfer.CharTransfer_ReserveVramRange(0xFE00, 0x200, vramType);
                break;
            case ObjMapping1D128K:
                if (Video.ObjectBank == 0x30 || Video.ObjectBank == 0x50)
                {
                    CharTransfer.CharTransfer_ReserveVramRange(0x13E00, 0x200, vramType);
                }
                else
                {
                    CharTransfer.CharTransfer_ReserveVramRange(0x1FE00, 0x200, vramType);
                }
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
    }

    // OFFSET 0x02009704
    public static void ReserveSlotsForWirelessIconPalette(int vramType)
    {
        PlttTransfer.PlttTransfer_MarkReservedSlots(0xC000, vramType);
    }
}
