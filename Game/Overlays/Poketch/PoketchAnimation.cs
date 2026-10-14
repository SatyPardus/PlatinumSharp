namespace PlatinumSharp.Game;

public sealed class PoketchAnimationData
{
    public int X;
    public int Y;
    public ushort AnimIdx;
    public byte Flags;
    public byte OamPriority;
    public byte Priority;
    public byte HasAffineTransform;

    public static PoketchAnimationData FromRom(int overlay, uint address)
    {
        byte[] raw = RomData.Overlay(overlay, address, 0x10);
        return new PoketchAnimationData
        {
            X = (int)Nitro.Bin.U32(raw, 0),
            Y = (int)Nitro.Bin.U32(raw, 4),
            AnimIdx = (ushort)Nitro.Bin.U16(raw, 8),
            Flags = raw[0xA],
            OamPriority = raw[0xB],
            Priority = raw[0xC],
            HasAffineTransform = raw[0xD],
        };
    }
}

public sealed class PoketchSpriteData
{
    public byte[]? CellFile;
    public byte[]? AnimFile;
    public CellDataBank? CellBank;
    public AnimBankData? AnimBank;
    public int HeapID;
}

public sealed class PoketchAnimatedSprite
{
    public CellDataBank? CellBank;
    public AnimBankData? AnimBank;
    public readonly CellAnimation CellAnim = new();
    public PoketchAnimatedSprite? Next;
    public PoketchAnimatedSprite? Prev;
    public int X;
    public int Y;
    public MtxFx22 Mtx;
    public ushort Priority;
    public ushort OamPriority;
    public ushort CParam;
    public byte Hidden;
    public byte HasAffineTransform;
    public ushort CharNo;
    public byte FlipH;
    public byte FlipV;
    public ushort Rotation;
    public byte Mosaic;
}

public sealed class PoketchAnimationManager
{
    public OamManager? OamMan;
    public PoketchAnimatedSprite?[] FreeSprites = Array.Empty<PoketchAnimatedSprite?>();
    public PoketchAnimatedSprite? Head;
    public PoketchAnimatedSprite[] Sprites = Array.Empty<PoketchAnimatedSprite>();
    public ushort Capacity;
    public ushort Count;
    public OamAttributes[] Oams = Array.Empty<OamAttributes>();
    public int HeapID;
}

public static class PoketchAnimation
{
    private const int TickSpeed = 0x2000;
    private const ushort SrtFlagScale = 2;
    private const ushort SrtFlagRotZ = 4;

    // OFFSET 0x02255540
    public static PoketchAnimationManager? PoketchAnimation_SetupAnimationManager(OamManager oamMan, int heapID)
    {
        PoketchAnimationManager animMan = new();
        animMan.OamMan = oamMan;
        animMan.HeapID = heapID;
        animMan.Capacity = (ushort)oamMan.RemainingCapacity;
        animMan.Oams = new OamAttributes[animMan.Capacity];
        animMan.Sprites = new PoketchAnimatedSprite[animMan.Capacity];
        for (int i = 0; i < animMan.Capacity; i++)
        {
            animMan.Sprites[i] = new PoketchAnimatedSprite();
        }
        animMan.FreeSprites = new PoketchAnimatedSprite?[animMan.Capacity];
        PoketchAnimation_InitFreeList(animMan.FreeSprites, animMan.Sprites, animMan.Capacity);
        animMan.Count = 0;
        animMan.Head = null;
        return animMan;
    }

    // OFFSET 0x022555E8
    public static void PoketchAnimation_FreeAnimationManager(PoketchAnimationManager? animMan)
    {
    }

    // OFFSET 0x02255614
    public static void PoketchAnimation_UpdateAnimations(PoketchAnimationManager animMan)
    {
        if (animMan.Count == 0)
        {
            return;
        }
        PoketchAnimatedSprite? sprite = animMan.Head;
        int oamIndex = 0;
        int remaining = animMan.Capacity;
        OamAttributes[] scratch = new OamAttributes[animMan.Capacity];
        while (sprite != null)
        {
            NnsG2d.TickCellAnimation(sprite.CellAnim, TickSpeed);
            if (sprite.Hidden == 0)
            {
                SrtControl srt = sprite.CellAnim.Srt;
                VecFx32 position = new() { X = sprite.X + (srt.Px << Fx32.Shift), Y = sprite.Y + (srt.Py << Fx32.Shift) };
                CellData cell = NnsG2d.GetCellData(sprite.CellBank, sprite.CellAnim.CurrentCell)!;
                int produced;
                if (sprite.HasAffineTransform == 0)
                {
                    produced = NnsG2d.MakeCellToOams(scratch, remaining, cell, null, position, 0, false);
                }
                else
                {
                    sprite.Mtx = new MtxFx22 { M00 = Fx32.One, M01 = 0, M10 = 0, M11 = Fx32.One };
                    if ((srt.EnableFlag & SrtFlagRotZ) != 0)
                    {
                        int angle = (ushort)(srt.RotZ + sprite.Rotation) >> 4;
                        PoketchAnimation_Rot22(ref sprite.Mtx, Render3D.Sin(angle), Render3D.Cos(angle));
                    }
                    else if (sprite.Rotation != 0)
                    {
                        int angle = sprite.Rotation >> 4;
                        PoketchAnimation_Rot22(ref sprite.Mtx, Render3D.Sin(angle), Render3D.Cos(angle));
                    }
                    if ((srt.EnableFlag & SrtFlagScale) != 0)
                    {
                        int scaleX = Fx32.Div(Fx32.One, srt.Sx);
                        int scaleY = Fx32.Div(Fx32.One, srt.Sy);
                        sprite.Mtx.M00 = Fx32.Mul(sprite.Mtx.M00, scaleX);
                        sprite.Mtx.M01 = Fx32.Mul(sprite.Mtx.M01, scaleX);
                        sprite.Mtx.M10 = Fx32.Mul(sprite.Mtx.M10, scaleY);
                        sprite.Mtx.M11 = Fx32.Mul(sprite.Mtx.M11, scaleY);
                    }
                    ushort affineIndex = animMan.OamMan!.EntryAffine(sprite.Mtx);
                    produced = NnsG2d.MakeCellToOams(scratch, remaining, cell, sprite.Mtx, position, affineIndex, true);
                }
                remaining -= produced;
                for (int i = 0; i < produced; i++)
                {
                    OamAttributes oam = scratch[i];
                    uint attr23 = oam.Attr2;
                    attr23 = (attr23 & 0xFFFFF3FF) | (uint)((sprite.OamPriority & 3) << 10);
                    attr23 = (attr23 & 0xFFFF0FFF) | (uint)((((attr23 >> 12) & 0xF) + sprite.CParam) << 12 & 0xF000);
                    attr23 = (attr23 & 0xFFFFFC00) | (uint)(((attr23 & 0x3FF) + sprite.CharNo) & 0x3FF);
                    uint attr01 = oam.Attr0 | ((uint)oam.Attr1 << 16);
                    attr01 = (attr01 & 0xEFFFFFFF) | ((((attr01 >> 28) & 1) ^ sprite.FlipH) << 28);
                    attr01 = (attr01 & 0xDFFFFFFF) | ((((attr01 >> 29) & 1) ^ sprite.FlipV) << 29);
                    attr01 = (attr01 & 0xFFFFEFFF) | ((((attr01 >> 12) & 1) ^ sprite.Mosaic) << 12);
                    animMan.Oams[oamIndex + i] = new OamAttributes { Attr0 = (ushort)attr01, Attr1 = (ushort)(attr01 >> 16), Attr2 = (ushort)attr23 };
                }
                oamIndex += produced;
            }
            sprite = sprite.Next;
        }
        if (oamIndex > 0)
        {
            animMan.OamMan!.EntryMany(animMan.Oams, oamIndex);
        }
    }

    private static void PoketchAnimation_Rot22(ref MtxFx22 mtx, int sin, int cos)
    {
        mtx.M00 = cos;
        mtx.M01 = sin;
        mtx.M10 = -sin;
        mtx.M11 = cos;
    }

    // OFFSET 0x02255810
    public static PoketchAnimatedSprite? PoketchAnimation_SetupNewAnimatedSprite(PoketchAnimationManager animMan, PoketchAnimationData animData, PoketchSpriteData spriteData)
    {
        PoketchAnimatedSprite? sprite = PoketchAnimation_AllocSprite(animMan);
        if (sprite == null)
        {
            return null;
        }
        sprite.OamPriority = animData.OamPriority;
        sprite.Priority = animData.Priority;
        PoketchAnimation_InsertSprite(animMan, sprite);
        sprite.CellBank = spriteData.CellBank;
        sprite.AnimBank = spriteData.AnimBank;
        NnsG2d.InitCellAnimation(sprite.CellAnim, NnsG2d.GetAnimSequenceByIdx(sprite.AnimBank, animData.AnimIdx), sprite.CellBank);
        sprite.X = animData.X;
        sprite.Y = animData.Y;
        sprite.CParam = 0;
        sprite.CharNo = 0;
        sprite.FlipH = (byte)((animData.Flags & 1) != 0 ? 1 : 0);
        sprite.FlipV = (byte)((animData.Flags & 2) != 0 ? 1 : 0);
        sprite.Mosaic = 0;
        sprite.Rotation = 0;
        sprite.HasAffineTransform = animData.HasAffineTransform;
        sprite.Hidden = 0;
        return sprite;
    }

    // OFFSET 0x022558B0
    public static void PoketchAnimation_RemoveAnimatedSprite(PoketchAnimationManager animMan, PoketchAnimatedSprite sprite)
    {
        PoketchAnimation_UnlinkSprite(animMan, sprite);
        PoketchAnimation_FreeSprite(animMan, sprite);
    }

    // OFFSET 0x022558C4
    public static void PoketchAnimation_UpdateAnimationIdx(PoketchAnimatedSprite sprite, uint animIdx)
    {
        NnsG2d.InitCellAnimation(sprite.CellAnim, NnsG2d.GetAnimSequenceByIdx(sprite.AnimBank, (ushort)animIdx), sprite.CellBank);
    }

    // OFFSET 0x022558E0
    public static bool PoketchAnimation_AnimationInactive(PoketchAnimatedSprite sprite)
    {
        return sprite.CellAnim.AnimCtrl.Active == 0;
    }

    // OFFSET 0x022558F0
    public static void PoketchAnimation_TranslateSprite(PoketchAnimatedSprite sprite, int x, int y)
    {
        sprite.X += x;
        sprite.Y += y;
    }

    // OFFSET 0x02255900
    public static void PoketchAnimation_SetSpritePosition(PoketchAnimatedSprite sprite, int x, int y)
    {
        sprite.X = x;
        sprite.Y = y;
    }

    // OFFSET 0x02255908
    public static void PoketchAnimation_GetSpritePosition(PoketchAnimatedSprite sprite, out int x, out int y)
    {
        x = sprite.X;
        y = sprite.Y;
    }

    // OFFSET 0x02255914
    public static void PoketchAnimation_HideSprite(PoketchAnimatedSprite sprite, bool hidden)
    {
        sprite.Hidden = (byte)(hidden ? 1 : 0);
    }

    // OFFSET 0x0225591C
    public static void PoketchAnimation_SetSpritePriority(PoketchAnimationManager animMan, PoketchAnimatedSprite sprite, uint priority)
    {
        PoketchAnimation_UnlinkSprite(animMan, sprite);
        sprite.Priority = (ushort)priority;
        PoketchAnimation_InsertSprite(animMan, sprite);
    }

    // OFFSET 0x02255938
    public static void PoketchAnimation_SetCParam(PoketchAnimatedSprite sprite, uint value)
    {
        sprite.CParam = (ushort)value;
    }

    // OFFSET 0x02255940
    public static void PoketchAnimation_SetSpriteCharNo(PoketchAnimatedSprite sprite, uint value)
    {
        sprite.CharNo = (ushort)value;
    }

    // OFFSET 0x02255948
    public static void PoketchAnimation_SetMosaic(PoketchAnimatedSprite sprite, bool mosaic)
    {
        sprite.Mosaic = (byte)(mosaic ? 1 : 0);
    }

    // OFFSET 0x02255950
    public static void PoketchAnimation_SetSpriteRotation(PoketchAnimatedSprite sprite, ushort rotation)
    {
        sprite.Rotation = rotation;
    }

    // OFFSET 0x02255958
    public static bool PoketchAnimation_LoadSpriteFromNARC(PoketchSpriteData spriteData, int narcID, uint spriteID, uint animID, int heapID)
    {
        spriteData.HeapID = heapID;
        spriteData.CellFile = Graphics.LoadCompressedMemberFromNARC(narcID, spriteID, heapID);
        spriteData.AnimFile = Graphics.LoadCompressedMemberFromNARC(narcID, animID, heapID);
        if (spriteData.CellFile != null && spriteData.AnimFile != null)
        {
            if (!G2d.GetUnpackedCellBank(spriteData.CellFile, out spriteData.CellBank))
            {
                return false;
            }
            if (!G2d.GetUnpackedAnimBank(spriteData.AnimFile, out spriteData.AnimBank))
            {
                return false;
            }
        }
        return true;
    }

    // OFFSET 0x022559B0
    public static void PoketchAnimation_FreeSpriteData(PoketchSpriteData spriteData)
    {
        spriteData.CellFile = null;
        spriteData.AnimFile = null;
    }

    // OFFSET 0x022559D8
    private static void PoketchAnimation_InitFreeList(PoketchAnimatedSprite?[] freeSprites, PoketchAnimatedSprite[] sprites, int count)
    {
        for (int i = 0; i < count; i++)
        {
            freeSprites[i] = sprites[i];
            PoketchAnimation_ResetLinks(sprites[i]);
        }
    }

    // OFFSET 0x022559FC
    private static void PoketchAnimation_ResetLinks(PoketchAnimatedSprite sprite)
    {
        sprite.Next = null;
        sprite.Prev = null;
    }

    // OFFSET 0x02255A04
    private static PoketchAnimatedSprite? PoketchAnimation_AllocSprite(PoketchAnimationManager animMan)
    {
        if (animMan.Count < animMan.Capacity)
        {
            return animMan.FreeSprites[animMan.Count++];
        }
        return null;
    }

    // OFFSET 0x02255A1C
    private static void PoketchAnimation_FreeSprite(PoketchAnimationManager animMan, PoketchAnimatedSprite sprite)
    {
        if (animMan.Count != 0)
        {
            PoketchAnimation_ResetLinks(sprite);
            animMan.Count--;
            animMan.FreeSprites[animMan.Count] = sprite;
        }
    }

    // OFFSET 0x02255A40
    private static void PoketchAnimation_InsertSprite(PoketchAnimationManager animMan, PoketchAnimatedSprite sprite)
    {
        PoketchAnimatedSprite? head = animMan.Head;
        if (head == null)
        {
            animMan.Head = sprite;
            sprite.Next = null;
            sprite.Prev = null;
            return;
        }
        PoketchAnimatedSprite current = head;
        while (true)
        {
            if (sprite.Priority < current.Priority)
            {
                if (current == head)
                {
                    animMan.Head = sprite;
                }
                sprite.Prev = current.Prev;
                if (sprite.Prev != null)
                {
                    sprite.Prev.Next = sprite;
                }
                sprite.Next = current;
                current.Prev = sprite;
                return;
            }
            if (current.Next == null)
            {
                current.Next = sprite;
                sprite.Prev = current;
                sprite.Next = null;
                return;
            }
            current = current.Next;
        }
    }

    // OFFSET 0x02255A98
    private static void PoketchAnimation_UnlinkSprite(PoketchAnimationManager animMan, PoketchAnimatedSprite sprite)
    {
        if (sprite.Prev != null)
        {
            if (sprite.Next != null)
            {
                sprite.Prev.Next = sprite.Next;
                sprite.Next.Prev = sprite.Prev;
            }
            else
            {
                sprite.Prev.Next = null;
            }
            return;
        }
        if (sprite.Next != null)
        {
            sprite.Next.Prev = null;
            animMan.Head = sprite.Next;
        }
        else
        {
            animMan.Head = null;
        }
    }
}
