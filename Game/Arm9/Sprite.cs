using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class SpriteList
{
    public Sprite[]? Sprites;
    public int Capacity;
    public Sprite?[]? FreeSprites;
    public int FreeSpriteHead;
    public Sprite Sentinel = new();
    public G2dRenderer? Renderer;
    public byte[]? RawAnimData;
    public AnimBankData? DefaultAnimBank;
    public int Active;
}

public sealed class Sprite
{
    public const int TypeNone = 0;
    public const int TypeCell = 1;
    public const int TypeMultiCell = 2;
    public const int TypeVRamCell = 3;
    public const int OverwritePriority = 1;
    public const int OverwritePalette = 2;
    public const int OverwriteMosaic = 4;
    public const int OverwriteOamMode = 8;
    public const int OverwritePaletteOffset = 16;

    private const int DefaultAnimSpeed = 2 * Fx32.One;
    private const string DefaultAnimPath = "data/clact_default.NANR";
    private const uint HiddenOamAttributes = 0x2C0;
    private const int Format256Colors = 4;
    private const uint UserAnimAttrBlock = 0x55414154;
    private const int UserAnimSequenceAttrSize = 12;

    public VecFx32 Position;
    public VecFx32 AffineTranslation;
    public VecFx32 AffineScale;
    public ushort AffineZRotation;
    public byte AffineOverwriteMode;
    public byte FlipMode;
    public byte OverwriteFlags;
    public byte ExplicitPalette;
    public byte ExplicitPaletteOffset;
    public int ExplicitMosaic;
    public int ExplicitOamMode;
    public byte Draw;
    public byte Animate;
    public int AnimSpeed;
    public SpriteList? List;
    public CellDataBank? CellBank;
    public AnimBankData? CellAnimBank;
    public CellAnimation CellAnim = new();
    public uint TransferHandle;
    public MultiCellAnimation MultiCellAnim = new();
    public MultiCellDataBank? MultiCellBank;
    public MultiCellAnimBankData? MultiCellAnimBank;
    public G2dNode[]? Nodes;
    public CellAnimation[]? CellAnims;
    public ImageProxy ImageProxy = new();
    public ImagePaletteProxy PaletteProxy = new();
    public int Type;
    public ushort ActiveAnimID;
    public byte ExplicitPriority;
    public ushort Priority;
    public int VramType;
    public Sprite? Prev;
    public Sprite? Next;

    private delegate void SpriteDrawFunc(SpriteList list, Sprite sprite);
    private delegate void SpriteAnimFunc(Sprite sprite);

    private static readonly SpriteDrawFunc[] sDrawFuncs = { Sprite_DrawNone, Sprite_Draw };
    private static readonly SpriteAnimFunc[] sAnimFuncs = { Sprite_AnimateNone, Sprite_TickAnim };

    // OFFSET 0x020218BC
    public static SpriteList SpriteList_New(SpriteListParams listParams)
    {
        if (listParams == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (listParams!.Renderer == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteList list = new();
        SpriteList_Reset(list);
        list.Sprites = new Sprite[listParams.MaxElements];
        for (int i = 0; i < listParams.MaxElements; i++)
        {
            list.Sprites[i] = new Sprite();
        }
        list.Capacity = listParams.MaxElements;
        list.FreeSprites = new Sprite?[listParams.MaxElements];
        SpriteList_InitFreeSprites(list);
        Sprite_Reset(list.Sentinel);
        list.Sentinel.Prev = list.Sentinel;
        list.Sentinel.Next = list.Sentinel;
        list.Renderer = listParams.Renderer;
        list.RawAnimData = GameSystem.ReadFileToHeap(listParams.HeapID, DefaultAnimPath);
        G2d.GetUnpackedAnimBank(list.RawAnimData!, out list.DefaultAnimBank);
        list.Active = 1;
        return list;
    }

    // OFFSET 0x02021964
    public static bool SpriteList_Delete(SpriteList? list)
    {
        if (list == null)
        {
            return false;
        }
        if (list.Sprites == null)
        {
            return true;
        }
        SpriteList_DeleteAll(list);
        list.RawAnimData = null;
        list.FreeSprites = null;
        list.Sprites = null;
        SpriteList_Reset(list);
        return true;
    }

    // OFFSET 0x020219A4
    public static bool SpriteList_SetActive(SpriteList? list, byte active)
    {
        if (list == null)
        {
            return false;
        }
        if (list.Sprites == null)
        {
            return false;
        }
        list.Active = active;
        return true;
    }

    // OFFSET 0x020219C0
    public static bool SpriteList_DeleteAll(SpriteList? list)
    {
        if (list == null)
        {
            return false;
        }
        if (list.Sprites == null)
        {
            return true;
        }
        Sprite sprite = list.Sentinel.Next!;
        while (sprite != list.Sentinel)
        {
            Sprite next = sprite.Next!;
            Sprite_Delete(sprite);
            sprite = next;
        }
        return true;
    }

    // OFFSET 0x020219F8
    public static void SpriteList_Update(SpriteList? list)
    {
        if (list == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (list!.Active == 0)
        {
            return;
        }
        for (Sprite sprite = list.Sentinel.Next!; sprite != list.Sentinel; sprite = sprite.Next!)
        {
            sDrawFuncs[sprite.Draw](list, sprite);
            sAnimFuncs[sprite.Animate](sprite);
        }
    }

    // OFFSET 0x02021A50
    private static void SpriteList_Reset(SpriteList list)
    {
        list.Sprites = null;
        list.Capacity = 0;
        list.FreeSprites = null;
        list.FreeSpriteHead = 0;
        list.Renderer = null;
        Sprite_Reset(list.Sentinel);
        list.Active = 0;
    }

    // OFFSET 0x02021A78
    public static void Sprite_Reset(Sprite sprite)
    {
        sprite.Position = default;
        sprite.AffineTranslation = default;
        sprite.AffineScale = default;
        sprite.AffineZRotation = 0;
        sprite.AffineOverwriteMode = 0;
        sprite.FlipMode = 0;
        sprite.OverwriteFlags = 0;
        sprite.ExplicitPalette = 0;
        sprite.ExplicitPaletteOffset = 0;
        sprite.ExplicitMosaic = 0;
        sprite.Draw = 0;
        sprite.Animate = 0;
        sprite.AnimSpeed = 0;
        sprite.List = null;
        sprite.CellBank = null;
        sprite.CellAnimBank = null;
        sprite.CellAnim = new CellAnimation();
        sprite.TransferHandle = 0;
        sprite.MultiCellAnim = new MultiCellAnimation();
        sprite.MultiCellBank = null;
        sprite.MultiCellAnimBank = null;
        sprite.Nodes = null;
        sprite.CellAnims = null;
        sprite.Type = TypeNone;
        sprite.ActiveAnimID = 0;
        sprite.ExplicitPriority = 0;
        sprite.Priority = 0;
        sprite.VramType = 0;
        sprite.Prev = null;
        sprite.Next = null;
        sprite.ImageProxy.Init();
        sprite.PaletteProxy.Init();
        sprite.ExplicitOamMode = 0;
    }

    // OFFSET 0x02021AA0
    public static Sprite? SpriteList_AddAffine(AffineSpriteListTemplate template)
    {
        Sprite? sprite = SpriteList_AllocSprite(template.List!);
        if (sprite == null)
        {
            return null;
        }
        sprite.List = template.List;
        sprite.ActiveAnimID = 0;
        sprite.Position = template.Position;
        sprite.AffineScale = template.AffineScale;
        sprite.AffineZRotation = template.AffineZRotation;
        sprite.VramType = template.VramType;
        sprite.Priority = (ushort)template.Priority;
        sprite.AffineOverwriteMode = 0;
        sprite.FlipMode = 0;
        sprite.ExplicitMosaic = 0;
        sprite.ExplicitOamMode = 0;
        sprite.OverwriteFlags = OverwritePriority | OverwritePaletteOffset;
        NnsG2d.SetRendererAffineOverwriteMode(template.List!.Renderer!, sprite.AffineOverwriteMode);
        NnsG2d.SetRendererFlipMode(template.List.Renderer!, sprite.FlipMode & 1, sprite.FlipMode & 2);
        sprite.Draw = 1;
        sprite.Animate = 0;
        sprite.AnimSpeed = DefaultAnimSpeed;
        if (!Sprite_InitResources(template.List, template.ResourceData!, sprite, template.HeapID))
        {
            Sprite_Delete(sprite);
            return null;
        }
        sprite.ExplicitPaletteOffset = (byte)Sprite_GetPaletteIndex(sprite.PaletteProxy, sprite.VramType);
        sprite.ExplicitPalette = sprite.ExplicitPaletteOffset;
        SpriteList_Insert(template.List, sprite);
        return sprite;
    }

    // OFFSET 0x02021B90
    public static Sprite? SpriteList_Add(SpriteListTemplate template)
    {
        AffineSpriteListTemplate affineTemplate = new();
        affineTemplate.List = template.List;
        affineTemplate.ResourceData = template.ResourceData;
        affineTemplate.Position = template.Position;
        affineTemplate.AffineScale.X = Fx32.One;
        affineTemplate.AffineScale.Y = Fx32.One;
        affineTemplate.AffineScale.Z = Fx32.One;
        affineTemplate.AffineZRotation = 0;
        affineTemplate.Priority = template.Priority;
        affineTemplate.VramType = template.VramType;
        affineTemplate.HeapID = template.HeapID;
        return SpriteList_AddAffine(affineTemplate);
    }

    // OFFSET 0x02021BD4
    public static void Sprite_Delete(Sprite sprite)
    {
        if (sprite.Type == TypeNone)
        {
            return;
        }
        if (sprite.Prev != null)
        {
            Sprite_Unlink(sprite);
        }
        if (sprite.Type == TypeVRamCell)
        {
            if (G2d.GetImageLocation(sprite.ImageProxy, sprite.VramType) != -1)
            {
                NnsG2d.FreeCellTransferStateHandle(sprite.TransferHandle);
            }
        }
        if (sprite.Type == TypeMultiCell)
        {
            if (sprite.Nodes != null)
            {
                sprite.Nodes = null;
            }
            if (sprite.CellAnims != null)
            {
                sprite.CellAnims = null;
            }
        }
        sprite.Type = TypeNone;
        SpriteList_FreeSprite(sprite.List!, sprite);
    }

    // OFFSET 0x02021C50
    public static void Sprite_SetPosition(Sprite sprite, in VecFx32 position)
    {
        sprite.Position = position;
    }

    // OFFSET 0x02021C60
    public static void Sprite_SetAffineTranslation(Sprite sprite, in VecFx32 translation)
    {
        sprite.AffineTranslation = translation;
    }

    // OFFSET 0x02021C70
    public static void Sprite_SetAffineScale(Sprite sprite, in VecFx32 scale)
    {
        sprite.AffineScale = scale;
    }

    // OFFSET 0x02021C80
    public static void Sprite_SetAffineScaleEx(Sprite sprite, in VecFx32 scale, int mode)
    {
        Sprite_SetAffineScale(sprite, scale);
        Sprite_SetAffineOverwriteMode(sprite, mode);
    }

    // OFFSET 0x02021C94
    public static void Sprite_SetAffineZRotation(Sprite sprite, ushort angle)
    {
        sprite.AffineZRotation = angle;
    }

    // OFFSET 0x02021C98
    public static void Sprite_SetAffineZRotationEx(Sprite sprite, ushort angle, int mode)
    {
        Sprite_SetAffineZRotation(sprite, angle);
        Sprite_SetAffineOverwriteMode(sprite, mode);
    }

    // OFFSET 0x02021CAC
    public static void Sprite_SetDrawFlag(Sprite sprite, int draw)
    {
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (draw >= 2)
        {
            ErrorHandling.GF_AssertFail();
        }
        sprite!.Draw = (byte)draw;
    }

    // OFFSET 0x02021CC8
    public static void Sprite_SetAnimateFlag(Sprite sprite, int animate)
    {
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (animate >= 2)
        {
            ErrorHandling.GF_AssertFail();
        }
        sprite!.Animate = (byte)animate;
    }

    // OFFSET 0x02021CE4
    public static void Sprite_SetAnimSpeed(Sprite sprite, int speed)
    {
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sprite!.AnimSpeed = speed;
    }

    // OFFSET 0x02021CF8
    public static void Sprite_SetAffineOverwriteMode(Sprite sprite, int mode)
    {
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sprite!.AffineOverwriteMode = (byte)mode;
    }

    // OFFSET 0x02021D0C
    public static void Sprite_SetFlipMode(Sprite sprite, int mode)
    {
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sprite!.FlipMode = (byte)mode;
        sprite.AffineOverwriteMode = 0;
    }

    // OFFSET 0x02021D28
    public static VecFx32 Sprite_GetPosition(Sprite sprite)
    {
        return sprite.Position;
    }

    // OFFSET 0x02021D2C
    public static VecFx32 Sprite_GetAffineScale(Sprite sprite)
    {
        return sprite.AffineScale;
    }

    // OFFSET 0x02021D30
    public static ushort Sprite_GetAffineZRotation(Sprite sprite)
    {
        return sprite.AffineZRotation;
    }

    // OFFSET 0x02021D34
    public static int Sprite_GetDrawFlag(Sprite sprite)
    {
        return sprite.Draw;
    }

    // OFFSET 0x02021D3C
    public static int Sprite_GetAnimateFlag(Sprite sprite)
    {
        return sprite.Animate;
    }

    // OFFSET 0x02021D44
    public static uint Sprite_GetAnimCount(Sprite sprite)
    {
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (sprite!.Type == TypeCell || sprite.Type == TypeVRamCell)
        {
            return (uint)Bin.U16(sprite.CellAnimBank!.File, sprite.CellAnimBank.SectionOffset + 8);
        }
        return (uint)Bin.U16(sprite.MultiCellAnimBank!.File, sprite.MultiCellAnimBank.SectionOffset + 8);
    }

    // OFFSET 0x02021D6C
    public static void Sprite_SetAnim(Sprite sprite, uint animID)
    {
        if (Sprite_GetAnimCount(sprite) <= animID)
        {
            ErrorHandling.GF_AssertFail();
        }
        sprite.ActiveAnimID = (ushort)animID;
        if (sprite.Type == TypeCell || sprite.Type == TypeVRamCell)
        {
            AnimSequence? sequence = NnsG2d.GetAnimSequenceByIdx(sprite.CellAnimBank, (ushort)animID);
            NnsG2d.SetCellAnimationSequence(sprite.CellAnim, sequence);
            sprite.CellAnim.AnimCtrl.Active = 1;
        }
        else
        {
            AnimSequence? sequence = NnsG2d.GetAnimSequenceByIdx(sprite.MultiCellAnimBank, (ushort)animID);
            NnsG2d.SetAnimSequenceToMCAnimation(sprite.MultiCellAnim, sequence);
            sprite.MultiCellAnim.AnimCtrl.Active = 1;
        }
    }

    // OFFSET 0x02021DCC
    public static void Sprite_SetAnimNoRestart(Sprite sprite, uint animID)
    {
        if (sprite.ActiveAnimID != animID)
        {
            Sprite_SetAnim(sprite, animID);
        }
    }

    // OFFSET 0x02021DE0
    public static void Sprite_RestartAnim(Sprite sprite)
    {
        if (sprite.Type == TypeCell || sprite.Type == TypeVRamCell)
        {
            NnsG2d.ResetAnimCtrlState(sprite.CellAnim.AnimCtrl);
            sprite.CellAnim.AnimCtrl.Active = 1;
            Sprite_SetAnimFrame(sprite, 0);
        }
        else
        {
            NnsG2d.ResetAnimCtrlState(sprite.MultiCellAnim.AnimCtrl);
            sprite.MultiCellAnim.AnimCtrl.Active = 1;
            Sprite_SetAnimFrame(sprite, 0);
        }
    }

    // OFFSET 0x02021E24
    public static uint Sprite_GetActiveAnim(Sprite sprite)
    {
        return sprite.ActiveAnimID;
    }

    // OFFSET 0x02021E2C
    public static void Sprite_UpdateAnim(Sprite sprite, int frames)
    {
        if (sprite.Type == TypeCell || sprite.Type == TypeVRamCell)
        {
            NnsG2d.TickCellAnimation(sprite.CellAnim, frames);
        }
        else
        {
            NnsG2d.TickMCAnimation(sprite.MultiCellAnim, frames);
        }
    }

    // OFFSET 0x02021E50
    public static void Sprite_SetAnimFrame(Sprite sprite, ushort frame)
    {
        if (sprite.Type == TypeCell || sprite.Type == TypeVRamCell)
        {
            NnsG2d.SetCellAnimationCurrentFrame(sprite.CellAnim, frame);
        }
        else
        {
            NnsG2d.SetMCAnimationCurrentFrame(sprite.MultiCellAnim, frame);
        }
    }

    private static AnimController Sprite_GetAnimController(Sprite sprite)
    {
        if (sprite.Type == TypeMultiCell)
        {
            return sprite.MultiCellAnim.AnimCtrl;
        }
        return sprite.CellAnim.AnimCtrl;
    }

    // OFFSET 0x02021E74
    public static ushort Sprite_GetAnimFrame(Sprite sprite)
    {
        return NnsG2d.GetAnimCtrlCurrentFrame(Sprite_GetAnimController(sprite));
    }

    // OFFSET 0x02021E80
    public static void Sprite_SetExplicitPriority(Sprite sprite, byte priority)
    {
        sprite.ExplicitPriority = priority;
    }

    // OFFSET 0x02021E88
    public static int Sprite_GetExplicitPriority(Sprite sprite)
    {
        return sprite.ExplicitPriority;
    }

    // OFFSET 0x02021E90
    public static void Sprite_SetExplicitPalette(Sprite sprite, uint palette)
    {
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sprite!.ExplicitPalette = (byte)palette;
        sprite.OverwriteFlags |= OverwritePalette;
        sprite.OverwriteFlags &= unchecked((byte)~OverwritePaletteOffset);
    }

    // OFFSET 0x02021EC4
    public static void Sprite_SetExplicitPaletteWithOffset(Sprite sprite, uint palette)
    {
        Sprite_SetExplicitPalette(sprite, palette);
        sprite.ExplicitPalette += (byte)Sprite_GetPaletteIndex(sprite.PaletteProxy, sprite.VramType);
    }

    // OFFSET 0x02021EE8
    public static uint Sprite_GetExplicitPalette(Sprite sprite)
    {
        return sprite.ExplicitPalette;
    }

    // OFFSET 0x02021EF0
    public static void Sprite_SetExplicitPaletteOffset(Sprite sprite, uint paletteOffset)
    {
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sprite!.ExplicitPaletteOffset = (byte)paletteOffset;
        sprite.OverwriteFlags |= OverwritePaletteOffset;
        sprite.OverwriteFlags &= unchecked((byte)~OverwritePalette);
    }

    // OFFSET 0x02021F24
    public static void Sprite_SetExplicitPaletteOffsetAutoAdjust(Sprite sprite, uint paletteOffset)
    {
        Sprite_SetExplicitPaletteOffset(sprite, paletteOffset);
        sprite.ExplicitPaletteOffset += (byte)Sprite_GetPaletteIndex(sprite.PaletteProxy, sprite.VramType);
    }

    // OFFSET 0x02021F48
    public static uint Sprite_GetExplicitPaletteOffset(Sprite sprite)
    {
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return sprite!.ExplicitPaletteOffset;
    }

    // OFFSET 0x02021F58
    public static void Sprite_SetPriority(Sprite sprite, uint priority)
    {
        SpriteList list = sprite.List!;
        sprite.Priority = (ushort)priority;
        Sprite_Unlink(sprite);
        SpriteList_Insert(list, sprite);
    }

    // OFFSET 0x02021F74
    public static uint Sprite_GetPriority(Sprite sprite)
    {
        return sprite.Priority;
    }

    // OFFSET 0x02021F7C
    public static void Sprite_SetImageProxy(Sprite sprite, ImageProxy imageProxy)
    {
        sprite.ImageProxy.CopyFrom(imageProxy);
    }

    // OFFSET 0x02021F98
    public static ImageProxy Sprite_GetImageProxy(Sprite sprite)
    {
        return sprite.ImageProxy;
    }

    // OFFSET 0x02021F9C
    public static ImagePaletteProxy Sprite_GetPaletteProxy(Sprite sprite)
    {
        return sprite.PaletteProxy;
    }

    // OFFSET 0x02021FA0
    public static void Sprite_SetMosaicFlag(Sprite sprite, int mosaic)
    {
        sprite.ExplicitMosaic = mosaic;
        if (mosaic == 1)
        {
            sprite.OverwriteFlags |= OverwriteMosaic;
        }
        else
        {
            sprite.OverwriteFlags ^= OverwriteMosaic;
        }
    }

    // OFFSET 0x02021FC8
    public static int Sprite_GetVRamType(Sprite sprite)
    {
        return sprite.VramType;
    }

    // OFFSET 0x02021FD0
    public static bool Sprite_IsAnimated(Sprite sprite)
    {
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return Sprite_GetAnimController(sprite!).Active != 0;
    }

    // OFFSET 0x02021FE0
    public static void Sprite_SetExplicitOAMMode(Sprite sprite, int mode)
    {
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sprite!.ExplicitOamMode = mode;
        if (mode == 0)
        {
            sprite.OverwriteFlags ^= OverwriteOamMode;
        }
        else
        {
            sprite.OverwriteFlags |= OverwriteOamMode;
        }
    }

    // OFFSET 0x02022014
    public static void Sprite_ClearMainOam(int heapID)
    {
        OamManager.FillScreen(0, HiddenOamAttributes);
    }

    // OFFSET 0x0202204C
    public static void Sprite_ClearSubOam(int heapID)
    {
        OamManager.FillScreen(1, HiddenOamAttributes);
    }

    // OFFSET 0x0202207C
    public static uint Sprite_GetUserAttrForAnimFrame(Sprite sprite, uint animID, uint frame)
    {
        byte[] file;
        int bankData;
        if (sprite.Type == TypeCell || sprite.Type == TypeVRamCell)
        {
            file = sprite.CellAnimBank!.File;
            bankData = sprite.CellAnimBank.SectionOffset + 8;
        }
        else
        {
            file = sprite.MultiCellAnimBank!.File;
            bankData = sprite.MultiCellAnimBank.SectionOffset + 8;
        }
        int extendedData = (int)Bin.U32(file, bankData + 0x14);
        if (extendedData == 0)
        {
            return 0;
        }
        int block = bankData + extendedData;
        if (Bin.U32(file, block) != UserAnimAttrBlock)
        {
            return 0;
        }
        int attrBank = block + 8;
        if ((ushort)animID >= Bin.U16(file, attrBank))
        {
            return 0;
        }
        int sequenceAttr = attrBank + (int)Bin.U32(file, attrBank + 4) + (ushort)animID * UserAnimSequenceAttrSize;
        if (Bin.U16(file, sequenceAttr) == 0)
        {
            return 0;
        }
        int frameAttr = attrBank + (int)Bin.U32(file, sequenceAttr + 8) + ((ushort)frame << 2);
        int attr = attrBank + (int)Bin.U32(file, frameAttr);
        return Bin.U32(file, attr);
    }

    // OFFSET 0x020220F4
    public static uint Sprite_GetUserAttrForCurrentAnimFrame(Sprite sprite)
    {
        uint animID = Sprite_GetActiveAnim(sprite);
        ushort frame = Sprite_GetAnimFrame(sprite);
        return Sprite_GetUserAttrForAnimFrame(sprite, animID, frame);
    }

    // OFFSET 0x02022110
    private static bool Sprite_InitResources(SpriteList list, SpriteResourcesHeader header, Sprite sprite, int heapID)
    {
        sprite.Type = SpriteResourcesHeader_GetSpriteType(header);
        sprite.ImageProxy.CopyFrom(header.ImageProxy!);
        sprite.PaletteProxy.Palette = header.PaletteProxy!.Palette;
        sprite.PaletteProxy.CompressInfo = header.PaletteProxy.CompressInfo;
        sprite.PaletteProxy.BaseAddress = header.PaletteProxy.BaseAddress;
        sprite.PaletteProxy.VramType = header.PaletteProxy.VramType;
        Array.Copy(header.PaletteProxy.Locations, sprite.PaletteProxy.Locations, 3);
        Sprite_SetCellBank(header.CellBank, sprite);
        if (header.CellAnimBank != null)
        {
            Sprite_SetCellAnimBank(header.CellAnimBank, sprite);
        }
        else
        {
            Sprite_SetCellAnimBank(list.DefaultAnimBank, sprite);
        }
        if (sprite.Type == TypeMultiCell)
        {
            Sprite_SetMultiCellBank(header.MultiCellBank, sprite);
            Sprite_SetMultiCellAnimBank(header.MultiCellAnimBank, sprite);
            Sprite_InitMultiCellAnim(sprite, heapID);
        }
        else if (sprite.Type == TypeVRamCell)
        {
            Sprite_InitVRamCellAnim(header, sprite, heapID);
        }
        else
        {
            Sprite_InitCellAnim(sprite, heapID);
        }
        sprite.ExplicitPriority = header.Priority;
        return true;
    }

    // OFFSET 0x020221B8
    private static int SpriteResourcesHeader_GetSpriteType(SpriteResourcesHeader header)
    {
        if (header.MultiCellBank != null)
        {
            return TypeMultiCell;
        }
        if (header.IsVRamTransfer == 1)
        {
            return TypeVRamCell;
        }
        return TypeCell;
    }

    // OFFSET 0x020221D0
    private static void Sprite_SetCellBank(CellDataBank? cellBank, Sprite sprite)
    {
        sprite.CellBank = cellBank;
    }

    // OFFSET 0x020221D4
    private static void Sprite_SetCellAnimBank(AnimBankData? cellAnimBank, Sprite sprite)
    {
        sprite.CellAnimBank = cellAnimBank;
    }

    // OFFSET 0x020221D8
    private static void Sprite_SetMultiCellBank(MultiCellDataBank? multiCellBank, Sprite sprite)
    {
        sprite.MultiCellBank = multiCellBank;
    }

    // OFFSET 0x020221E0
    private static void Sprite_SetMultiCellAnimBank(MultiCellAnimBankData? multiCellAnimBank, Sprite sprite)
    {
        sprite.MultiCellAnimBank = multiCellAnimBank;
    }

    // OFFSET 0x020221E8
    private static void Sprite_InitCellAnim(Sprite sprite, int heapID)
    {
        AnimSequence? sequence = NnsG2d.GetAnimSequenceByIdx(sprite.CellAnimBank, 0);
        NnsG2d.InitCellAnimation(sprite.CellAnim, sequence, sprite.CellBank);
    }

    // OFFSET 0x02022208
    private static void Sprite_InitVRamCellAnim(SpriteResourcesHeader header, Sprite sprite, int heapID)
    {
        CharacterData charData = header.CharData!;
        sprite.TransferHandle = NnsG2d.GetNewCellTransferStateHandle();
        AnimSequence? sequence = NnsG2d.GetAnimSequenceByIdx(sprite.CellAnimBank, 0);
        uint destMain = (uint)G2d.GetImageLocation(sprite.ImageProxy, G2d.VramMain);
        uint destSub = (uint)G2d.GetImageLocation(sprite.ImageProxy, G2d.VramSub);
        NnsG2d.InitCellAnimationVramTransfered(sprite.CellAnim, sequence, sprite.CellBank, sprite.TransferHandle, 0xFFFFFFFF, destMain, destSub, charData.RawData, null, charData.SizeBytes);
    }

    // OFFSET 0x02022264
    private static void Sprite_InitMultiCellAnim(Sprite sprite, int heapID)
    {
        AnimSequence? sequence = NnsG2d.GetAnimSequenceByIdx(sprite.MultiCellAnimBank, 0);
        ushort numNodes = NnsG2d.GetMCBankNumNodesRequired(sprite.MultiCellBank);
        sprite.Nodes = new G2dNode[numNodes];
        sprite.CellAnims = new CellAnimation[numNodes];
        for (int i = 0; i < numNodes; i++)
        {
            sprite.Nodes[i] = new G2dNode();
            sprite.CellAnims[i] = new CellAnimation();
        }
        NnsG2d.InitMCAnimation(sprite.MultiCellAnim, sprite.Nodes, sprite.CellAnims, numNodes, sprite.CellAnimBank, sprite.CellBank, sprite.MultiCellBank);
        NnsG2d.SetAnimSequenceToMCAnimation(sprite.MultiCellAnim, sequence);
    }

    // OFFSET 0x020222C4
    private static uint Sprite_GetPaletteIndex(ImagePaletteProxy paletteProxy, int vramType)
    {
        uint paletteSize;
        if (paletteProxy.Palette != null && paletteProxy.Palette.Extended)
        {
            paletteSize = 0x200;
        }
        else if (paletteProxy.Palette != null && paletteProxy.Palette.Format == Format256Colors)
        {
            paletteSize = 0;
        }
        else
        {
            paletteSize = 0x20;
        }
        if (paletteSize != 0)
        {
            return (uint)G2d.GetImagePaletteLocation(paletteProxy, vramType) / paletteSize;
        }
        return 0;
    }

    // OFFSET 0x020222F4
    private static void Sprite_Draw(SpriteList list, Sprite sprite)
    {
        VecFx32 position = sprite.Position;
        G2dRenderer renderer = list.Renderer!;
        NnsG2d.SetRendererImageProxy(renderer, sprite.ImageProxy, sprite.PaletteProxy);
        NnsG2d.BeginRendering(renderer);
        NnsG2d.PushMtx();
        NnsG2d.SetRendererAffineOverwriteMode(renderer, sprite.AffineOverwriteMode);
        if (sprite.AffineOverwriteMode == 0)
        {
            NnsG2d.SetRendererFlipMode(renderer, sprite.FlipMode & 1, sprite.FlipMode & 2);
        }
        else
        {
            NnsG2d.SetRendererFlipMode(renderer, 0, 0);
        }
        NnsG2d.Translate(position.X, position.Y, position.Z);
        if (sprite.AffineOverwriteMode != 0)
        {
            NnsG2d.Translate(sprite.AffineTranslation.X, sprite.AffineTranslation.Y, sprite.AffineTranslation.Z);
            NnsG2d.Scale(sprite.AffineScale.X, sprite.AffineScale.Y, sprite.AffineScale.Z);
            NnsG2d.RotZ(Render3D.Sin(sprite.AffineZRotation >> 4), Render3D.Cos(sprite.AffineZRotation >> 4));
            NnsG2d.Translate(-sprite.AffineTranslation.X, -sprite.AffineTranslation.Y, -sprite.AffineTranslation.Z);
        }
        renderer.OverwriteEnableFlag |= sprite.OverwriteFlags;
        renderer.OverwriteEnableFlag &= sprite.OverwriteFlags;
        renderer.OverwritePlttNo = sprite.ExplicitPalette;
        renderer.OverwritePlttNoOffset = sprite.ExplicitPaletteOffset;
        renderer.OverwriteMosaicFlag = sprite.ExplicitMosaic;
        renderer.OverwriteObjMode = sprite.ExplicitOamMode;
        renderer.OverwritePriority = sprite.ExplicitPriority;
        if (sprite.Type == TypeCell || sprite.Type == TypeVRamCell)
        {
            NnsG2d.DrawCellAnimation(sprite.CellAnim);
        }
        else
        {
            NnsG2d.DrawMultiCellAnimation(sprite.MultiCellAnim);
        }
        NnsG2d.PopMtx();
        NnsG2d.EndRendering();
    }

    // OFFSET 0x02022450
    private static void Sprite_DrawNone(SpriteList list, Sprite sprite)
    {
    }

    // OFFSET 0x02022454
    private static void Sprite_TickAnim(Sprite sprite)
    {
        Sprite_UpdateAnim(sprite, sprite.AnimSpeed);
    }

    // OFFSET 0x02022460
    private static void Sprite_AnimateNone(Sprite sprite)
    {
    }

    // OFFSET 0x02022464
    private static void SpriteList_Insert(SpriteList list, Sprite sprite)
    {
        Sprite sentinel = list.Sentinel;
        if (sentinel.Next == sentinel)
        {
            sentinel.Next = sprite;
            sentinel.Prev = sprite;
            sprite.Prev = sentinel;
            sprite.Next = sentinel;
            return;
        }
        Sprite last = sentinel.Prev!;
        if (last.Priority <= sprite.Priority)
        {
            sprite.Prev = last;
            last.Next = sprite;
            sprite.Next = sentinel;
            sentinel.Prev = sprite;
            return;
        }
        for (Sprite current = sentinel.Next!; current != sentinel; current = current.Next!)
        {
            if (current.Priority > sprite.Priority)
            {
                current.Prev!.Next = sprite;
                sprite.Prev = current.Prev;
                current.Prev = sprite;
                sprite.Next = current;
                return;
            }
        }
    }

    // OFFSET 0x020224FC
    private static void Sprite_Unlink(Sprite sprite)
    {
        sprite.Prev!.Next = sprite.Next;
        sprite.Next!.Prev = sprite.Prev;
    }

    // OFFSET 0x02022518
    private static void SpriteList_InitFreeSprites(SpriteList list)
    {
        for (int i = 0; i < list.Capacity; i++)
        {
            Sprite_Reset(list.Sprites![i]);
            list.FreeSprites![i] = list.Sprites[i];
        }
        list.FreeSpriteHead = 0;
    }

    // OFFSET 0x02022550
    private static Sprite? SpriteList_AllocSprite(SpriteList list)
    {
        if (list.FreeSpriteHead >= list.Capacity)
        {
            return null;
        }
        Sprite? sprite = list.FreeSprites![list.FreeSpriteHead];
        list.FreeSpriteHead++;
        return sprite;
    }

    // OFFSET 0x0202256C
    private static bool SpriteList_FreeSprite(SpriteList list, Sprite sprite)
    {
        if (list.FreeSpriteHead <= 0)
        {
            return false;
        }
        Sprite_Reset(sprite);
        list.FreeSpriteHead--;
        list.FreeSprites![list.FreeSpriteHead] = sprite;
        return true;
    }
}
