using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed class OpeningSprites
{
    public const int Size = 0x2A0;
    public const int SpriteCount = 17;
    public const int ResourceSetCount = 11;
    public const int ResourceTypeCount = 4;
    public const int HeaderCount = 3;
    public const int SpriteLogoTop = 0;
    public const int SpriteLogoBottom = 1;
    public const int SpriteLogoShine = 2;
    public const int SpriteBirdLeft = 3;
    public const int SpriteBirdRight = 10;
    public const int SpriteRingTop = 11;
    public const int SpriteRingBottom = 12;
    public const int SpriteCloudsTop = 13;
    public const int SpriteCloudsBottom = 15;

    private const int HeapID = 0x4C;
    private const int Overlay = 77;
    private const uint CharTransferTemplateAddress = 0x021D78BC;
    private const uint ResourceCapacitiesAddress = 0x021D7914;
    private const uint FlockIndexAddress = 0x021D78FC;
    private const uint FlockXAddress = 0x021D78CC;
    private const uint FlockYAddress = 0x021D78E4;
    private const uint FlockGroupsAddress = 0x021D789C;
    private const uint FlockFirstGroupAddress = 0x021D78B0;
    private const uint FlockLoneSpeedAddress = 0x021D79A0;
    private const uint FlockPairSpeedAddress = 0x021D79A4;
    private const uint FlockFirstSpeedAddress = 0x021D79AC;
    private const int GraphicsArchive = 0x80;
    private const int ResourceTiles = 0;
    private const int ResourcePalette = 1;
    private const int ResourceCells = 2;
    private const int ResourceAnimations = 3;
    private const uint ObjMappingMask = 0x00300010;
    private const uint ObjMapping1D32K = 0x10;
    private const int LayerObj = 0x10;
    private const int PaletteTransferCapacity = 0xB;
    private const int VramTransferCapacity = 6;
    private const int CellTransferCapacity = 3;
    private const int SpriteListCapacity = 0x21;
    private const int FlockCount = 6;
    private const int ParticleSpriteCount = 0x10;
    private const int LogoScrollSpeed = 0x200;
    private const int BirdSpeed = 0x12000;
    private const int BirdLeftStartFrame = 0x582;
    private const int BirdRightStartFrame = 0x5DC;
    private const int BirdLeaveFrame = 0x627;
    private const int BirdLeftStop = 0xB0000;
    private const int BirdLeftExit = 0x140000;
    private const int BirdRightStop = 0x50000;
    private const int BirdRightExit = -0x40000;
    private const int CloudWrapRight = 0x180000;
    private const int CloudWrapLeft = -0x80000;
    private const int CloudWidth = 0x100000;
    private const int CloudStartX = 0x80000;
    private const int RingSpinFrame = 0x744;
    private const int RingSpinSpeed = 0x1C00;
    private const int RingDriftSpeed = 0x1000;
    private const int FlockFirstFrame = 0x5A5;
    private const int FlockLoneFrame = 0x5D2;
    private const int FlockPairFrame = 0x5E6;
    private const int FlockFirstRise = 0x4000;
    private const int FlockRise = 0x3000;

    private static readonly int[] tilesMembers = { 0x2E, 0x2E, 0x32, 0x57, 0x16, 0x17, 0x5B, 0x53, 0x53, 0x36, 0x36 };
    private static readonly int[] tilesVramTypes = { 1, 2, 1, 1, 1, 2, 1, 1, 2, 1, 2 };
    private static readonly int[] paletteMembers = { 0x31, 0x31, 0x35, 0x5A, 0x14, 0x15, 0x5E, 0x56, 0x56, 0x39, 0x39 };
    private static readonly int[] paletteCounts = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3 };
    private static readonly int[] cellsMembers = { 0x30, 0x30, 0x33, 0x59, 0x18, 0x19, 0x5D, 0x55, 0x55, 0x37, 0x37 };
    private static readonly int[] animationsMembers = { 0x2F, 0x2F, 0x34, 0x58, 0x1A, 0x1B, 0x5C, 0x54, 0x54, 0x38, 0x38 };

    public SpriteList? List;
    public readonly G2dRenderer Renderer = new();
    public readonly SpriteResourceCollection?[] Collections = new SpriteResourceCollection?[ResourceTypeCount];
    public readonly SpriteResource?[,] Resources = new SpriteResource?[ResourceSetCount, ResourceTypeCount];
    public readonly Sprite?[] Sprites = new Sprite?[SpriteCount];
    public readonly SpriteResourcesHeader[] Headers = { new(), new(), new() };
    public CellTransferState[]? CellTransferStates;

    // OFFSET 0x021D54B0
    private static void OpeningSprites_InitTransfers()
    {
        byte[] raw = RomData.Overlay(Overlay, CharTransferTemplateAddress, 0x10);
        CharTransferTemplate template = new()
        {
            MaxTasks = (int)Bin.U32(raw, 0),
            SizeMain = (int)Bin.U32(raw, 4),
            SizeSub = (int)Bin.U32(raw, 8),
            HeapID = (int)Bin.U32(raw, 0xC),
        };
        CharTransfer.CharTransfer_Init(template);
        PlttTransfer.PlttTransfer_Init(PaletteTransferCapacity, HeapID);
        CharTransfer.CharTransfer_ClearBuffers();
        PlttTransfer.PlttTransfer_Clear();
        VramTransfer.VramTransfer_New(VramTransferCapacity, HeapID);
    }

    // OFFSET 0x021D54E8
    private static void OpeningSprites_BuildTemplate(int resourceID, OpeningSprites sprites, int bgPriority, AffineSpriteListTemplate template, SpriteResourcesHeader header, int vramTransfer, int vramType)
    {
        SpriteUtil.SpriteResourcesHeader_Init(
            header,
            resourceID,
            resourceID,
            resourceID,
            resourceID,
            -1,
            -1,
            vramTransfer,
            bgPriority,
            sprites.Collections[ResourceTiles],
            sprites.Collections[ResourcePalette],
            sprites.Collections[ResourceCells],
            sprites.Collections[ResourceAnimations],
            null,
            null);
        template.List = sprites.List;
        template.ResourceData = header;
        template.Position = new VecFx32 { X = 0, Y = 0, Z = 0 };
        template.AffineScale = new VecFx32 { X = Fx32.One, Y = Fx32.One, Z = Fx32.One };
        template.AffineZRotation = 0;
        template.Priority = 1;
        template.VramType = vramType;
        template.HeapID = HeapID;
    }

    // OFFSET 0x021D555C
    public static int OpeningSprites_Size()
    {
        return Size;
    }

    // OFFSET 0x021D5564
    public static void OpeningSprites_InitRendering(OpeningSprites sprites)
    {
        Video.SetDisplayControl(0, (Video.GetDisplayControl(0) & ~ObjMappingMask) | ObjMapping1D32K);
        Video.SetDisplayControl(1, (Video.GetDisplayControl(1) & ~ObjMappingMask) | ObjMapping1D32K);
        OpeningSprites_InitTransfers();
        OamManager.InitModule();
        RenderOam.RenderOam_Init(0, 0x80, 0, 0x20, 0, 0x80, 0, 0x20, HeapID);
        sprites.List = SpriteUtil.SpriteList_InitRendering(SpriteListCapacity, sprites.Renderer, HeapID);
        sprites.CellTransferStates = CellTransfer.CellTransfer_New(CellTransferCapacity, HeapID);
        byte[] capacities = RomData.Overlay(Overlay, ResourceCapacitiesAddress, ResourceTypeCount);
        for (int i = 0; i < ResourceTypeCount; i++)
        {
            sprites.Collections[i] = SpriteResourceCollection.SpriteResourceCollection_New(capacities[i], i, HeapID);
        }
        for (int i = 0; i < ResourceSetCount; i++)
        {
            sprites.Resources[i, ResourceTiles] = SpriteResourceCollection.SpriteResourceCollection_AddTiles(sprites.Collections[ResourceTiles], GraphicsArchive, tilesMembers[i], 0, i, tilesVramTypes[i], HeapID);
        }
        for (int i = 0; i < ResourceSetCount; i++)
        {
            sprites.Resources[i, ResourcePalette] = SpriteResourceCollection.SpriteResourceCollection_AddPalette(sprites.Collections[ResourcePalette], GraphicsArchive, paletteMembers[i], 0, i, tilesVramTypes[i], paletteCounts[i], HeapID);
        }
        for (int i = 0; i < ResourceSetCount; i++)
        {
            sprites.Resources[i, ResourceCells] = SpriteResourceCollection.SpriteResourceCollection_Add(sprites.Collections[ResourceCells], GraphicsArchive, cellsMembers[i], 0, i, ResourceCells, HeapID);
        }
        for (int i = 0; i < ResourceSetCount; i++)
        {
            sprites.Resources[i, ResourceAnimations] = SpriteResourceCollection.SpriteResourceCollection_Add(sprites.Collections[ResourceAnimations], GraphicsArchive, animationsMembers[i], 0, i, ResourceAnimations, HeapID);
        }
        for (int i = 0; i < ResourceSetCount; i++)
        {
            SpriteTransfer.SpriteTransfer_RequestCharAtEnd(sprites.Resources[i, ResourceTiles]);
            SpriteTransfer.SpriteTransfer_RequestPlttFreeSpace(sprites.Resources[i, ResourcePalette]);
        }
        GXLayers.GXLayers_EngineAToggleLayers(LayerObj, 1);
    }

    // OFFSET 0x021D5BAC
    public static void OpeningSprites_FreeRendering(OpeningSprites sprites)
    {
        for (byte i = 0; i < ResourceSetCount; i++)
        {
            SpriteTransfer.SpriteTransfer_ResetCharTransfer(sprites.Resources[i, ResourceTiles]);
            SpriteTransfer.SpriteTransfer_ResetPlttTransfer(sprites.Resources[i, ResourcePalette]);
        }
        for (byte i = 0; i < ResourceTypeCount; i++)
        {
            SpriteResourceCollection.SpriteResourceCollection_Delete(sprites.Collections[i]);
        }
        Sprite.SpriteList_Delete(sprites.List);
        for (byte i = 0; i < HeaderCount; i++)
        {
            SpriteUtil.SpriteResourcesHeader_Clear(sprites.Headers[i]);
        }
        if (sprites.CellTransferStates != null)
        {
            CellTransfer.CellTransfer_Free(sprites.CellTransferStates);
            sprites.CellTransferStates = null;
        }
        RenderOam.RenderOam_Free();
        CharTransfer.CharTransfer_Free();
        PlttTransfer.PlttTransfer_Free();
        VramTransfer.VramTransfer_Free();
    }

    private static Sprite AddSprite(OpeningSprites sprites, int index, int resourceID, int bgPriority, SpriteResourcesHeader header, int vramTransfer, int vramType, int x, int y, int animate)
    {
        AffineSpriteListTemplate template = new();
        OpeningSprites_BuildTemplate(resourceID, sprites, bgPriority, template, header, vramTransfer, vramType);
        template.Position.X = x;
        template.Position.Y = y;
        Sprite sprite = Sprite.SpriteList_AddAffine(template)!;
        sprites.Sprites[index] = sprite;
        Sprite.Sprite_SetAnimateFlag(sprite, animate);
        Sprite.Sprite_SetDrawFlag(sprite, 0);
        return sprite;
    }

    // OFFSET 0x021D5C3C
    public static void OpeningSprites_CreateSprites(OpeningSprites sprites)
    {
        uint[] flockIndex = RomData.OverlayU32(Overlay, FlockIndexAddress, FlockCount);
        uint[] flockX = RomData.OverlayU32(Overlay, FlockXAddress, FlockCount);
        uint[] flockY = RomData.OverlayU32(Overlay, FlockYAddress, FlockCount);
        SpriteResourcesHeader header = new();
        AddSprite(sprites, SpriteLogoTop, 0, 0, sprites.Headers[0], 1, G2d.VramMain, 0x50000, 0x70000, 0);
        AddSprite(sprites, SpriteLogoBottom, 1, 0, sprites.Headers[2], 1, G2d.VramSub, 0x50000, 0x130000, 0);
        AddSprite(sprites, SpriteLogoShine, 2, 0, sprites.Headers[1], 1, G2d.VramMain, 0xB0000, 0x70000, 0);
        AddSprite(sprites, SpriteBirdLeft, 3, 0, header, 0, G2d.VramMain, -0x40000, 0x70000, 1);
        AddSprite(sprites, SpriteBirdRight, 6, 0, header, 0, G2d.VramMain, 0x140000, 0x70000, 1);
        Sprite ringTop = AddSprite(sprites, SpriteRingTop, 7, 1, header, 0, G2d.VramMain, 0x80000, 0x60000, 1);
        Sprite.Sprite_SetAffineOverwriteMode(ringTop, 1);
        Sprite ringBottom = AddSprite(sprites, SpriteRingBottom, 8, 1, header, 0, G2d.VramSub, 0x80000, 0x120000, 1);
        Sprite.Sprite_SetAffineOverwriteMode(ringBottom, 1);
        AddSprite(sprites, SpriteCloudsTop, 9, 3, header, 0, G2d.VramMain, 0x80000, 0x68000, 1);
        AddSprite(sprites, SpriteCloudsTop + 1, 9, 3, header, 0, G2d.VramMain, 0x80000, 0x68000, 1);
        AddSprite(sprites, SpriteCloudsBottom, 10, 3, header, 0, G2d.VramSub, 0x80000, 0x128000, 1);
        AddSprite(sprites, SpriteCloudsBottom + 1, 10, 3, header, 0, G2d.VramSub, 0x80000, 0x128000, 1);
        for (int i = 0; i < FlockCount; i++)
        {
            AddSprite(sprites, (int)flockIndex[i], 4, 0, header, 0, G2d.VramMain, (int)flockX[i] << 12, (int)flockY[i] << 12, 1);
        }
    }

    // OFFSET 0x021D5FB4
    public static void OpeningSprites_CreateParticleSprites(OpeningSprites sprites, OpeningParticleSet set)
    {
        AffineSpriteListTemplate template = new();
        SpriteResourcesHeader header = new();
        OpeningSprites_BuildTemplate(5, sprites, 0, template, header, 0, G2d.VramSub);
        for (int i = 0; i < ParticleSpriteCount; i++)
        {
            OpeningParticle particle = OpeningEffects.OpeningParticleSet_GetParticle(set, i);
            particle.Sprite = Sprite.SpriteList_AddAffine(template);
            Sprite.Sprite_SetAnimateFlag(particle.Sprite!, 0);
            Sprite.Sprite_SetDrawFlag(particle.Sprite!, 0);
        }
    }

    // OFFSET 0x021D6000
    public static void OpeningSprites_HideParticleSprites(OpeningSprites sprites, OpeningParticleSet set)
    {
        for (int i = 0; i < ParticleSpriteCount; i++)
        {
            OpeningParticle particle = OpeningEffects.OpeningParticleSet_GetParticle(set, i);
            Sprite.Sprite_SetDrawFlag(particle.Sprite!, 0);
        }
    }

    // OFFSET 0x021D6020
    public static void OpeningSprites_Draw(OpeningSprites sprites)
    {
        Sprite.SpriteList_Update(sprites.List);
        if (sprites.CellTransferStates != null)
        {
            CellTransfer.CellTransfer_Update();
        }
    }

    // OFFSET 0x021D603C
    public static void OpeningSprites_SetDrawFlag(OpeningSprites sprites, int index, int draw)
    {
        Sprite.Sprite_SetDrawFlag(sprites.Sprites[index]!, draw);
    }

    // OFFSET 0x021D6050
    public static void OpeningSprites_SetLogoAnimate(OpeningSprites sprites, int animate)
    {
        Sprite.Sprite_SetAnimateFlag(sprites.Sprites[SpriteLogoTop]!, animate);
        Sprite.Sprite_SetAnimateFlag(sprites.Sprites[SpriteLogoShine]!, animate);
        Sprite.Sprite_SetAnimateFlag(sprites.Sprites[SpriteLogoBottom]!, animate);
    }

    // OFFSET 0x021D607C
    public static void OpeningSprites_ScrollLogo(OpeningSprites sprites)
    {
        VecFx32 position = Sprite.Sprite_GetPosition(sprites.Sprites[SpriteLogoBottom]!);
        position.X -= LogoScrollSpeed;
        Sprite.Sprite_SetPosition(sprites.Sprites[SpriteLogoBottom]!, position);
        position = Sprite.Sprite_GetPosition(sprites.Sprites[SpriteLogoShine]!);
        position.X += LogoScrollSpeed;
        Sprite.Sprite_SetPosition(sprites.Sprites[SpriteLogoShine]!, position);
    }

    // OFFSET 0x021D60E0
    public static bool OpeningSprites_UpdateBirdLeft(OpeningSprites sprites, int frame)
    {
        if (frame < BirdLeftStartFrame)
        {
            return false;
        }
        if (frame == BirdLeftStartFrame)
        {
            OpeningSprites_SetDrawFlag(sprites, SpriteBirdLeft, 1);
        }
        Sprite sprite = sprites.Sprites[SpriteBirdLeft]!;
        VecFx32 position = Sprite.Sprite_GetPosition(sprite);
        position.X += BirdSpeed;
        if (frame < BirdLeaveFrame)
        {
            if (position.X >= BirdLeftStop)
            {
                position.X = BirdLeftStop;
                Sprite.Sprite_SetPosition(sprite, position);
                return true;
            }
            Sprite.Sprite_SetPosition(sprite, position);
            return false;
        }
        if (position.X >= BirdLeftExit)
        {
            position.X = BirdLeftExit;
            Sprite.Sprite_SetPosition(sprite, position);
            OpeningSprites_SetDrawFlag(sprites, SpriteBirdLeft, 0);
            return true;
        }
        Sprite.Sprite_SetPosition(sprite, position);
        return false;
    }

    // OFFSET 0x021D61B8
    public static bool OpeningSprites_UpdateBirdRight(OpeningSprites sprites, int frame)
    {
        if (frame < BirdRightStartFrame)
        {
            return false;
        }
        if (frame == BirdRightStartFrame)
        {
            OpeningSprites_SetDrawFlag(sprites, SpriteBirdRight, 1);
        }
        Sprite sprite = sprites.Sprites[SpriteBirdRight]!;
        VecFx32 position = Sprite.Sprite_GetPosition(sprite);
        position.X -= BirdSpeed;
        if (frame < BirdLeaveFrame)
        {
            if (position.X <= BirdRightStop)
            {
                position.X = BirdRightStop;
                Sprite.Sprite_SetPosition(sprite, position);
                return true;
            }
            Sprite.Sprite_SetPosition(sprite, position);
            return false;
        }
        if (position.X <= BirdRightExit)
        {
            position.X = BirdRightExit;
            Sprite.Sprite_SetPosition(sprite, position);
            OpeningSprites_SetDrawFlag(sprites, SpriteBirdRight, 0);
            return true;
        }
        Sprite.Sprite_SetPosition(sprite, position);
        return false;
    }

    // OFFSET 0x021D6290
    public static void OpeningSprites_ScrollClouds(OpeningSprites sprites, int speed)
    {
        int forward;
        int backward;
        if (gSystem.WhichScreenIs3D == 0)
        {
            forward = SpriteCloudsTop;
            backward = SpriteCloudsBottom;
        }
        else
        {
            forward = SpriteCloudsBottom;
            backward = SpriteCloudsTop;
        }
        VecFx32 position = Sprite.Sprite_GetPosition(sprites.Sprites[forward]!);
        position.X += speed;
        if (position.X >= CloudWrapRight)
        {
            position.X -= CloudWidth;
        }
        Sprite.Sprite_SetPosition(sprites.Sprites[forward]!, position);
        position.X -= CloudWidth;
        Sprite.Sprite_SetPosition(sprites.Sprites[forward + 1]!, position);
        position = Sprite.Sprite_GetPosition(sprites.Sprites[backward]!);
        position.X -= speed;
        if (position.X <= CloudWrapLeft)
        {
            position.X += CloudWidth;
        }
        Sprite.Sprite_SetPosition(sprites.Sprites[backward]!, position);
        position.X += CloudWidth;
        Sprite.Sprite_SetPosition(sprites.Sprites[backward + 1]!, position);
    }

    // OFFSET 0x021D636C
    public static void OpeningSprites_ResetClouds(OpeningSprites sprites, int palette)
    {
        uint top;
        uint bottom;
        if (palette == 0)
        {
            if (gSystem.WhichScreenIs3D == 0)
            {
                top = 0;
                bottom = 1;
            }
            else
            {
                top = 1;
                bottom = 0;
            }
        }
        else
        {
            top = 2;
            bottom = 2;
        }
        Sprite.Sprite_SetExplicitPaletteOffsetAutoAdjust(sprites.Sprites[SpriteCloudsTop]!, top);
        Sprite.Sprite_SetExplicitPaletteOffsetAutoAdjust(sprites.Sprites[SpriteCloudsTop + 1]!, top);
        Sprite.Sprite_SetExplicitPaletteOffsetAutoAdjust(sprites.Sprites[SpriteCloudsBottom]!, bottom);
        Sprite.Sprite_SetExplicitPaletteOffsetAutoAdjust(sprites.Sprites[SpriteCloudsBottom + 1]!, bottom);
        VecFx32 position = Sprite.Sprite_GetPosition(sprites.Sprites[SpriteCloudsTop]!);
        position.X = CloudStartX;
        Sprite.Sprite_SetPosition(sprites.Sprites[SpriteCloudsTop]!, position);
        position = Sprite.Sprite_GetPosition(sprites.Sprites[SpriteCloudsBottom]!);
        position.X = CloudStartX;
        Sprite.Sprite_SetPosition(sprites.Sprites[SpriteCloudsBottom]!, position);
    }

    // OFFSET 0x021D6470
    public static void OpeningSprites_SpinRings(OpeningSprites sprites, int frame)
    {
        int extra = frame - RingSpinFrame;
        if (extra < 0)
        {
            extra = 0;
        }
        Sprite top = sprites.Sprites[SpriteRingTop]!;
        Sprite bottom = sprites.Sprites[SpriteRingBottom]!;
        Sprite.Sprite_SetAffineZRotation(top, (ushort)(Sprite.Sprite_GetAffineZRotation(top) + RingSpinSpeed + extra));
        Sprite.Sprite_SetAffineZRotation(bottom, (ushort)(Sprite.Sprite_GetAffineZRotation(bottom) - RingSpinSpeed - extra));
        if (frame >= RingSpinFrame)
        {
            VecFx32 position = Sprite.Sprite_GetPosition(top);
            position.X += RingDriftSpeed;
            Sprite.Sprite_SetPosition(top, position);
            position = Sprite.Sprite_GetPosition(bottom);
            position.X -= RingDriftSpeed;
            Sprite.Sprite_SetPosition(bottom, position);
        }
    }

    // OFFSET 0x021D6530
    public static void OpeningSprites_UpdateFlock(OpeningSprites sprites, int frame)
    {
        uint[] first = RomData.OverlayU32(Overlay, FlockFirstGroupAddress, 3);
        uint[] groups = RomData.OverlayU32(Overlay, FlockGroupsAddress, 5);
        if (frame >= FlockFirstFrame)
        {
            uint[] speeds = RomData.OverlayU32(Overlay, FlockFirstSpeedAddress, 3);
            for (byte i = 0; i < 3; i++)
            {
                Sprite sprite = sprites.Sprites[first[i]]!;
                VecFx32 position = Sprite.Sprite_GetPosition(sprite);
                position.X -= (int)speeds[i];
                position.Y -= FlockFirstRise;
                Sprite.Sprite_SetPosition(sprite, position);
            }
        }
        if (frame >= FlockLoneFrame)
        {
            Sprite sprite = sprites.Sprites[groups[2]]!;
            VecFx32 position = Sprite.Sprite_GetPosition(sprite);
            position.X -= (int)RomData.OverlayU32(Overlay, FlockLoneSpeedAddress, 1)[0];
            position.Y -= FlockRise;
            Sprite.Sprite_SetPosition(sprite, position);
        }
        if (frame >= FlockPairFrame)
        {
            uint[] speeds = RomData.OverlayU32(Overlay, FlockPairSpeedAddress, 2);
            for (byte i = 0; i < 2; i++)
            {
                Sprite sprite = sprites.Sprites[groups[3 + i]]!;
                VecFx32 position = Sprite.Sprite_GetPosition(sprite);
                position.X -= (int)speeds[i];
                position.Y -= FlockRise;
                Sprite.Sprite_SetPosition(sprite, position);
            }
        }
    }
}
