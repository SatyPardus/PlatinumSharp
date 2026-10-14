using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class SoftwareSpriteManagerTemplate
{
    public int NumSprites;
    public int NumChars;
    public int NumPalettes;
    public int HeapID;
}

public sealed class SoftwareSpriteCharsTemplate
{
    public SoftwareSpriteManager SoftSpriteMan = null!;
    public CharacterData CharsData = null!;
}

public sealed class SoftwareSpritePaletteTemplate
{
    public SoftwareSpriteManager SoftSpriteMan = null!;
    public PaletteFile PaletteData = null!;
    public uint PaletteSlot;
}

public sealed class SoftwareSpriteTemplate
{
    public SoftwareSpriteManager SoftSpriteMan = null!;
    public SoftwareSpriteChars Chars = null!;
    public SoftwareSpritePalette Palette = null!;
    public short XPos;
    public short YPos;
    public ushort Rotation;
    public int Alpha;
    public int Priority;
    public ushort PaletteSlot;
}

public sealed class SoftwareSpriteChars
{
    public uint Key;
    public readonly ImageProxy ImageProxy = new();
}

public sealed class SoftwareSpritePalette
{
    public uint Key;
    public readonly ImagePaletteProxy PaletteProxy = new();
}

public sealed class SoftwareSpriteManager
{
    public SoftwareSprite[]? Sprites;
    public int NumSprites;
    public SoftwareSpriteChars[]? Chars;
    public int NumChars;
    public SoftwareSpritePalette[]? Palettes;
    public int NumPalettes;
}

public sealed class SoftwareSprite
{
    private const int FlipHorizontal = 0;
    private const int Format4Color = 2;
    private const int PaletteSlotBytes = 0x20;
    private const byte MaxAlpha = 0x1F;
    private const ushort WhiteDiffuse = 0x7FFF;
    private const uint AmbientWhiteVertexColor = 0x7FFF8000;
    private const uint SpecularGray = 0x4210;
    private const uint PolygonAttrBothSides = 0xC0;
    private const uint TexCoordModeTexCoord = 1u << 30;

    public short X;
    public short Y;
    public short CenterX;
    public short CenterY;
    public short Width;
    public short Height;
    public int ScaleX;
    public int ScaleY;
    public int Priority;
    public uint Rotation;
    public int Active;
    public int Visible;
    public ImageProxy? Attributes;
    public uint CharsVramAddress;
    public uint PaletteVramAddress;
    public uint PaletteSlot;
    public int FlipH;
    public int FlipV;
    public ushort Diffuse;
    public byte Alpha;

    // OFFSET 0x02015064
    public static SoftwareSpriteManager SoftwareSpriteManager_New(SoftwareSpriteManagerTemplate template)
    {
        SoftwareSpriteManager softSpriteMan = new();
        softSpriteMan.Sprites = SoftwareSpriteManager_AllocSprites(template.NumSprites, template.HeapID);
        softSpriteMan.NumSprites = template.NumSprites;
        softSpriteMan.Chars = SoftwareSpriteManager_AllocChars(template.NumChars, template.HeapID);
        softSpriteMan.NumChars = template.NumChars;
        softSpriteMan.Palettes = SoftwareSpriteManager_AllocPalettes(template.NumPalettes, template.HeapID);
        softSpriteMan.NumPalettes = template.NumPalettes;
        return softSpriteMan;
    }

    // OFFSET 0x020150A8
    public static void SoftwareSpriteManager_Free(SoftwareSpriteManager? softSpriteMan)
    {
        if (softSpriteMan == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (softSpriteMan!.Sprites == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (softSpriteMan.Chars == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (softSpriteMan.Palettes == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        softSpriteMan.Sprites = null;
        softSpriteMan.Chars = null;
        softSpriteMan.Palettes = null;
    }

    // OFFSET 0x020150EC
    public static void SoftwareSpriteManager_DrawVisible(SoftwareSpriteManager softSpriteMan)
    {
        Render3D.PushMatrix();
        for (int i = 0; i < softSpriteMan.NumSprites; i++)
        {
            if (softSpriteMan.Sprites![i].Visible != 0)
            {
                SoftwareSprite_Draw(softSpriteMan.Sprites[i]);
            }
        }
        Render3D.PopMatrix(1);
    }

    // OFFSET 0x02015128
    public static SoftwareSpriteChars SoftwareSprite_LoadChars(SoftwareSpriteCharsTemplate template)
    {
        SoftwareSpriteChars? chars = SoftwareSpriteManager_FindFreeChars(template.SoftSpriteMan);
        if (chars == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (template.CharsData.MappingType != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        chars!.Key = SoftwareSprite_AllocTexVram(template.CharsData);
        if (chars.Key == Gfd.InvalidKey)
        {
            ErrorHandling.GF_AssertFail();
        }
        SoftwareSprite_UploadChars(template.CharsData, chars.Key, chars.ImageProxy);
        return chars;
    }

    // OFFSET 0x02015164
    public static void SoftwareSprite_FreeChars(SoftwareSpriteChars chars)
    {
        Gfd.FreeTexVram(chars.Key);
        SoftwareSpriteChars_Init(chars);
    }

    // OFFSET 0x0201517C
    public static void SoftwareSpriteManager_FreeAllChars(SoftwareSpriteManager softSpriteMan)
    {
        for (int i = 0; i < softSpriteMan.NumChars; i++)
        {
            if (softSpriteMan.Chars![i].Key != Gfd.InvalidKey)
            {
                SoftwareSprite_FreeChars(softSpriteMan.Chars[i]);
            }
        }
    }

    // OFFSET 0x020151A4
    public static SoftwareSpritePalette SoftwareSprite_LoadPalette(SoftwareSpritePaletteTemplate template)
    {
        SoftwareSpritePalette? palette = SoftwareSpriteManager_FindFreePalette(template.SoftSpriteMan);
        if (palette == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        palette!.Key = SoftwareSprite_AllocPlttVram(template.PaletteSlot);
        if (palette.Key == Gfd.InvalidKey)
        {
            ErrorHandling.GF_AssertFail();
        }
        SoftwareSprite_UploadPalette(template.PaletteData, palette.Key, palette.PaletteProxy);
        return palette;
    }

    // OFFSET 0x020151D4
    public static void SoftwareSprite_FreePalette(SoftwareSpritePalette palette)
    {
        Gfd.FreePlttVram(palette.Key);
        SoftwareSpritePalette_Init(palette);
    }

    // OFFSET 0x020151EC
    public static void SoftwareSpriteManager_FreeAllPalettes(SoftwareSpriteManager softSpriteMan)
    {
        for (int i = 0; i < softSpriteMan.NumPalettes; i++)
        {
            if (softSpriteMan.Palettes![i].Key != Gfd.InvalidKey)
            {
                SoftwareSprite_FreePalette(softSpriteMan.Palettes[i]);
            }
        }
    }

    // OFFSET 0x02015214
    public static SoftwareSprite SoftwareSprite_Load(SoftwareSpriteTemplate template)
    {
        SoftwareSprite? sprite = SoftwareSpriteManager_FindFreeSprite(template.SoftSpriteMan);
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SoftwareSprite_InitFromTemplate(sprite!, template);
        sprite!.Active = 1;
        sprite.Visible = 1;
        return sprite;
    }

    // OFFSET 0x02015238
    public static void SoftwareSprite_Reset(SoftwareSprite sprite)
    {
        SoftwareSprite_Init(sprite);
    }

    // OFFSET 0x02015240
    public static void SoftwareSprite_SetVisible(SoftwareSprite? sprite, int visible)
    {
        if (sprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        sprite!.Visible = visible;
    }

    // OFFSET 0x02015254
    public static void SoftwareSprite_SetPosition(SoftwareSprite sprite, short x, short y)
    {
        sprite.X = x;
        sprite.Y = y;
    }

    // OFFSET 0x0201525C
    public static (short X, short Y) SoftwareSprite_GetPosition(SoftwareSprite sprite)
    {
        return (sprite.X, sprite.Y);
    }

    // OFFSET 0x02015268
    public static void SoftwareSprite_SetCenter(SoftwareSprite sprite, short x, short y)
    {
        sprite.CenterX = x;
        sprite.CenterY = y;
    }

    // OFFSET 0x02015270
    public static void SoftwareSprite_SetScalingFactors(SoftwareSprite sprite, int scaleX, int scaleY)
    {
        sprite.ScaleX = scaleX;
        sprite.ScaleY = scaleY;
    }

    // OFFSET 0x02015278
    public static void SoftwareSprite_SetDimensions(SoftwareSprite sprite, int width, int height)
    {
        sprite.Width = (short)width;
        sprite.Height = (short)height;
    }

    // OFFSET 0x02015280
    public static (short X, short Y) SoftwareSprite_GetDimensions(SoftwareSprite sprite)
    {
        return (sprite.Width, sprite.Height);
    }

    // OFFSET 0x0201528C
    public static void SoftwareSprite_SetPriority(SoftwareSprite sprite, int priority)
    {
        sprite.Priority = priority;
    }

    // OFFSET 0x02015290
    public static int SoftwareSprite_GetPriority(SoftwareSprite sprite)
    {
        return sprite.Priority;
    }

    // OFFSET 0x02015294
    public static void SoftwareSprite_SetAlpha(SoftwareSprite sprite, byte alpha)
    {
        sprite.Alpha = alpha;
    }

    // OFFSET 0x0201529C
    public static void SoftwareSprite_SetDiffuse(SoftwareSprite sprite, ushort diffuse)
    {
        sprite.Diffuse = diffuse;
    }

    // OFFSET 0x020152A0
    public static void SoftwareSprite_SetAttributes(SoftwareSprite sprite, ImageProxy? attributes)
    {
        sprite.Attributes = attributes;
    }

    // OFFSET 0x020152A4
    public static void SoftwareSprite_SetVRAMAddress_Chars(SoftwareSprite sprite, uint address)
    {
        sprite.CharsVramAddress = address;
    }

    // OFFSET 0x020152A8
    public static void SoftwareSprite_SetVRAMAddress_Palette(SoftwareSprite sprite, uint address)
    {
        sprite.PaletteVramAddress = address;
    }

    // OFFSET 0x020152AC
    public static void SoftwareSprite_SetPaletteSlot(SoftwareSprite sprite, uint slot)
    {
        sprite.PaletteSlot = slot;
    }

    // OFFSET 0x020152B0
    public static void SoftwareSprite_SetFlip(SoftwareSprite sprite, int direction, int flip)
    {
        if (direction == FlipHorizontal)
        {
            sprite.FlipH = flip;
        }
        else
        {
            sprite.FlipV = flip;
        }
    }

    // OFFSET 0x020152BC
    public static void SoftwareSprite_SetRotation(SoftwareSprite sprite, ushort rotation)
    {
        sprite.Rotation = rotation;
    }

    // OFFSET 0x020152C0
    private static void SoftwareSprite_Init(SoftwareSprite sprite)
    {
        sprite.X = 0;
        sprite.Y = 0;
        sprite.CenterX = 0;
        sprite.CenterY = 0;
        sprite.Width = 0;
        sprite.Height = 0;
        sprite.ScaleX = 0;
        sprite.ScaleY = 0;
        sprite.Priority = 0;
        sprite.Rotation = 0;
        sprite.Active = 0;
        sprite.Visible = 0;
        sprite.Attributes = null;
        sprite.CharsVramAddress = 0;
        sprite.PaletteVramAddress = 0;
        sprite.PaletteSlot = 0;
        sprite.FlipH = 0;
        sprite.FlipV = 0;
        sprite.Alpha = MaxAlpha;
        sprite.Diffuse = WhiteDiffuse;
    }

    // OFFSET 0x020152E0
    private static void SoftwareSpriteChars_Init(SoftwareSpriteChars chars)
    {
        chars.Key = Gfd.InvalidKey;
        chars.ImageProxy.Init();
    }

    // OFFSET 0x020152EC
    private static void SoftwareSpritePalette_Init(SoftwareSpritePalette palette)
    {
        palette.Key = Gfd.InvalidKey;
        palette.PaletteProxy.Init();
    }

    // OFFSET 0x020152F8
    private static SoftwareSprite[] SoftwareSpriteManager_AllocSprites(int count, int heapID)
    {
        SoftwareSprite[] sprites = new SoftwareSprite[count];
        for (int i = 0; i < count; i++)
        {
            sprites[i] = new SoftwareSprite();
            SoftwareSprite_Init(sprites[i]);
        }
        return sprites;
    }

    // OFFSET 0x02015328
    private static SoftwareSpriteChars[] SoftwareSpriteManager_AllocChars(int count, int heapID)
    {
        SoftwareSpriteChars[] chars = new SoftwareSpriteChars[count];
        for (int i = 0; i < count; i++)
        {
            chars[i] = new SoftwareSpriteChars();
            SoftwareSpriteChars_Init(chars[i]);
        }
        return chars;
    }

    // OFFSET 0x02015358
    private static SoftwareSpritePalette[] SoftwareSpriteManager_AllocPalettes(int count, int heapID)
    {
        SoftwareSpritePalette[] palettes = new SoftwareSpritePalette[count];
        for (int i = 0; i < count; i++)
        {
            palettes[i] = new SoftwareSpritePalette();
            SoftwareSpritePalette_Init(palettes[i]);
        }
        return palettes;
    }

    // OFFSET 0x02015388
    private static SoftwareSprite? SoftwareSpriteManager_FindFreeSprite(SoftwareSpriteManager softSpriteMan)
    {
        for (int i = 0; i < softSpriteMan.NumSprites; i++)
        {
            if (softSpriteMan.Sprites![i].Active == 0)
            {
                return softSpriteMan.Sprites[i];
            }
        }
        return null;
    }

    // OFFSET 0x020153B4
    private static SoftwareSpriteChars? SoftwareSpriteManager_FindFreeChars(SoftwareSpriteManager softSpriteMan)
    {
        for (int i = 0; i < softSpriteMan.NumSprites; i++)
        {
            if (softSpriteMan.Chars![i].Key == Gfd.InvalidKey)
            {
                return softSpriteMan.Chars[i];
            }
        }
        return null;
    }

    // OFFSET 0x020153E0
    private static SoftwareSpritePalette? SoftwareSpriteManager_FindFreePalette(SoftwareSpriteManager softSpriteMan)
    {
        for (int i = 0; i < softSpriteMan.NumSprites; i++)
        {
            if (softSpriteMan.Palettes![i].Key == Gfd.InvalidKey)
            {
                return softSpriteMan.Palettes[i];
            }
        }
        return null;
    }

    // OFFSET 0x0201540C
    private static uint SoftwareSprite_AllocTexVram(CharacterData charsData)
    {
        return Gfd.AllocTexVram(charsData.SizeBytes, 0, 0);
    }

    // OFFSET 0x02015420
    private static uint SoftwareSprite_AllocPlttVram(uint paletteSlots)
    {
        return Gfd.AllocPlttVram(paletteSlots * PaletteSlotBytes, 0, 0);
    }

    // OFFSET 0x02015434
    private static void SoftwareSprite_UploadChars(CharacterData charsData, uint key, ImageProxy imageProxy)
    {
        G2d.LoadImageMapping(G2d.Layout2D, charsData, Gfd.GetTexKeyAddress(key), G2d.Vram3D, imageProxy);
    }

    // OFFSET 0x02015444
    private static void SoftwareSprite_UploadPalette(PaletteFile paletteData, uint key, ImagePaletteProxy paletteProxy)
    {
        uint byteSize = paletteData.ByteSize;
        paletteData.ByteSize = Gfd.GetPlttKeySize(key);
        G2d.LoadPalette(paletteData, null, Gfd.GetPlttKeyAddress(key), G2d.Vram3D, paletteProxy);
        paletteData.ByteSize = byteSize;
    }

    // OFFSET 0x02015468
    private static void SoftwareSprite_Draw(SoftwareSprite sprite)
    {
        Render3D.PushMatrix();
        Render3D.SetDiffuseAmbient(AmbientWhiteVertexColor | sprite.Diffuse);
        Render3D.SetSpecularEmission(SpecularGray);
        ImageProxy attributes = sprite.Attributes!;
        Render3D.SetTexImageParam((attributes.Format << 26) | (sprite.CharsVramAddress >> 3) | TexCoordModeTexCoord
            | (attributes.SizeS << 20) | (attributes.SizeT << 23) | (attributes.Color0Transparent << 29));
        int paletteShift = attributes.Format == Format4Color ? 1 : 0;
        Render3D.SetPaletteBase((sprite.PaletteVramAddress + sprite.PaletteSlot * PaletteSlotBytes) >> (4 - paletteShift));
        Render3D.SetPolygonAttr(PolygonAttrBothSides | ((uint)sprite.Alpha << 16));
        int u0;
        int u1;
        if (sprite.FlipH != 0)
        {
            u0 = sprite.Width;
            u1 = 0;
        }
        else
        {
            u1 = sprite.Width;
            u0 = 0;
        }
        int v0;
        int v1;
        if (sprite.FlipV != 0)
        {
            v0 = sprite.Height;
            v1 = 0;
        }
        else
        {
            v1 = sprite.Height;
            v0 = 0;
        }
        Render3D.Translate((sprite.X + sprite.CenterX) << Fx32.Shift, (sprite.Y + sprite.CenterY) << Fx32.Shift, sprite.Priority << Fx32.Shift);
        int angle = (int)sprite.Rotation >> 4;
        Render3D.RotateZ(Render3D.Sin(angle), Render3D.Cos(angle));
        Render3D.Scale(sprite.ScaleX, sprite.ScaleY, Fx32.One);
        Render3D.Translate(-sprite.CenterX << Fx32.Shift, -sprite.CenterY << Fx32.Shift, 0);
        Render3D.DrawSpriteFast(0, 0, 0, sprite.Width, sprite.Height, u0, v0, u1, v1);
        Render3D.PopMatrix(1);
    }

    // OFFSET 0x020155A8
    private static void SoftwareSprite_InitFromTemplate(SoftwareSprite sprite, SoftwareSpriteTemplate template)
    {
        ImageProxy imageProxy = template.Chars.ImageProxy;
        int width = 8;
        for (int i = 0; i < (int)imageProxy.SizeS; i++)
        {
            width *= 2;
        }
        int height = 8;
        for (int i = 0; i < (int)imageProxy.SizeT; i++)
        {
            height *= 2;
        }
        SoftwareSprite_SetPosition(sprite, template.XPos, template.YPos);
        SoftwareSprite_SetCenter(sprite, (short)(width / 2), (short)(height / 2));
        sprite.ScaleX = Fx32.One;
        sprite.ScaleY = Fx32.One;
        SoftwareSprite_SetDimensions(sprite, width, height);
        SoftwareSprite_SetPriority(sprite, template.Priority);
        SoftwareSprite_SetAlpha(sprite, (byte)template.Alpha);
        SoftwareSprite_SetAttributes(sprite, imageProxy);
        SoftwareSprite_SetVRAMAddress_Chars(sprite, (uint)G2d.GetImageLocation(imageProxy, G2d.Vram3D));
        SoftwareSprite_SetVRAMAddress_Palette(sprite, (uint)G2d.GetImagePaletteLocation(template.Palette.PaletteProxy, G2d.Vram3D));
        SoftwareSprite_SetPaletteSlot(sprite, template.PaletteSlot);
        SoftwareSprite_SetFlip(sprite, 0, 0);
        SoftwareSprite_SetFlip(sprite, 1, 0);
        SoftwareSprite_SetRotation(sprite, template.Rotation);
    }
}
