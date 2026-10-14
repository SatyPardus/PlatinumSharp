namespace PlatinumSharp.Game;

public static partial class TrainerCaseApplication
{
    private const uint CollectionCapacities = 0x0223D6C8;
    private const uint CharTransferTemplateAddress = 0x0223D6CC;
    private const uint BadgePaletteMembers = 0x0223D6DC;
    private const uint SparklePositions = 0x0223D6FC;
    private const uint BadgePositions = 0x0223D73C;
    private const int ResourceID = 1;
    private const int VramTypeMain = 1;

    // OFFSET 0x0223CF0C
    private static void TrainerCaseApp_InitSpriteData(TrainerCaseSpriteData spriteData, NARC narc)
    {
        TrainerCase_InitCharPlttTransferBuffers();
        OamManager.InitModule();
        RenderOam.RenderOam_Init(0, 0x80, 0, 0x20, 0, 0x80, 0, 0x20, HeapID);
        spriteData.SpriteList = SpriteUtil.SpriteList_InitRendering(TrainerCaseSpriteData.NumSprites, spriteData.G2dRenderer, HeapID);
        byte[] capacities = RomData.Overlay(Overlay, CollectionCapacities, 4);
        for (int i = 0; i < 4; i++)
        {
            spriteData.SpriteResourceCollections[i] = SpriteResourceCollection.SpriteResourceCollection_New(capacities[i], i, HeapID);
        }
        spriteData.SpriteResources[0] = SpriteResourceCollection.SpriteResourceCollection_AddTilesFrom(spriteData.SpriteResourceCollections[0], narc, 0x21, 0, ResourceID, VramTypeMain, HeapID);
        spriteData.SpriteResources[1] = SpriteResourceCollection.SpriteResourceCollection_AddPaletteFrom(spriteData.SpriteResourceCollections[1], narc, 0xE, 0, ResourceID, VramTypeMain, 9, HeapID);
        spriteData.SpriteResources[2] = SpriteResourceCollection.SpriteResourceCollection_AddFrom(spriteData.SpriteResourceCollections[2], narc, 0x2C, 0, ResourceID, 2, HeapID);
        spriteData.SpriteResources[3] = SpriteResourceCollection.SpriteResourceCollection_AddFrom(spriteData.SpriteResourceCollections[3], narc, 0x2E, 0, ResourceID, 3, HeapID);
        SpriteTransfer.SpriteTransfer_RequestChar(spriteData.SpriteResources[0]);
        SpriteTransfer.SpriteTransfer_RequestPlttWholeRange(spriteData.SpriteResources[1]);
        uint[] members = RomData.OverlayU32(Overlay, BadgePaletteMembers, TrainerCase.MaxBadges);
        for (int i = 0; i < TrainerCase.MaxBadges; i++)
        {
            spriteData.BadgePaletteBuffers[i] = NARC.NARC_AllocAndReadWholeMember(narc, members[i], HeapID);
            if (spriteData.BadgePaletteBuffers[i] != null)
            {
                if (!G2d.GetUnpackedPaletteData(spriteData.BadgePaletteBuffers[i]!, out spriteData.BadgePalettes[i]))
                {
                    Heap.Heap_Free(spriteData.BadgePaletteBuffers[i]!);
                    ErrorHandling.GF_AssertFail();
                }
            }
            else
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        GXLayers.GXLayers_EngineAToggleLayers(0x10, 1);
    }

    private static Sprite AddBadgeCaseSprite(AffineSpriteListTemplate template, uint positionTable, int index, int priority)
    {
        uint[] position = RomData.OverlayU32(Overlay, (uint)(positionTable + index * 8), 2);
        template.Position = new VecFx32 { X = (int)position[0] << 12, Y = (int)position[1] << 12, Z = 0 };
        template.Priority = priority;
        return Sprite.SpriteList_AddAffine(template)!;
    }

    // OFFSET 0x0223D070
    private static void TrainerCase_DrawBadgeCaseSprites(TrainerCaseSpriteData spriteData, byte[] obtainedBadges)
    {
        SpriteResourcesHeader header = new();
        SpriteUtil.SpriteResourcesHeader_Init(header, ResourceID, ResourceID, ResourceID, ResourceID, -1, -1, 0, 1,
            spriteData.SpriteResourceCollections[0], spriteData.SpriteResourceCollections[1], spriteData.SpriteResourceCollections[2], spriteData.SpriteResourceCollections[3], null, null);
        AffineSpriteListTemplate template = new()
        {
            List = spriteData.SpriteList,
            ResourceData = header,
            AffineScale = new VecFx32 { X = 0x1000, Y = 0x1000, Z = 0x1000 },
            AffineZRotation = 0,
            Priority = 2,
            VramType = VramTypeMain,
            HeapID = HeapID,
        };
        for (int i = 0; i < TrainerCase.MaxBadges; i++)
        {
            Sprite sprite = AddBadgeCaseSprite(template, BadgePositions, i, 2);
            spriteData.Sprites[i] = sprite;
            Sprite.Sprite_SetAnimateFlag(sprite, 0);
            Sprite.Sprite_SetAnim(sprite, (uint)i);
            if (obtainedBadges[i] == 0)
            {
                Sprite.Sprite_SetDrawFlag(sprite, 0);
            }
        }
        for (int i = 0; i < TrainerCase.MaxBadges; i++)
        {
            Sprite sprite = AddBadgeCaseSprite(template, SparklePositions, i, 1);
            spriteData.Sprites[SpriteSparklesTwo + i] = sprite;
            Sprite.Sprite_SetAnimateFlag(sprite, 1);
            Sprite.Sprite_SetAnim(sprite, 8);
            Sprite.Sprite_SetDrawFlag(sprite, 0);
        }
        for (int i = 0; i < TrainerCase.MaxBadges; i++)
        {
            Sprite sprite = AddBadgeCaseSprite(template, SparklePositions, i, 1);
            spriteData.Sprites[SpriteSparklesFour + i] = sprite;
            Sprite.Sprite_SetAnimateFlag(sprite, 1);
            Sprite.Sprite_SetAnim(sprite, 9);
            Sprite.Sprite_SetDrawFlag(sprite, 0);
        }
        template.Position = new VecFx32 { X = 0x60000, Y = 0x88000, Z = 0 };
        template.Priority = 0;
        Sprite buttonEffect = Sprite.SpriteList_AddAffine(template)!;
        spriteData.Sprites[SpriteButtonEffect] = buttonEffect;
        Sprite.Sprite_SetAnimateFlag(buttonEffect, 0);
        Sprite.Sprite_SetAnim(buttonEffect, 0xA);
        Sprite.Sprite_SetDrawFlag(buttonEffect, 0);
    }

    // OFFSET 0x0223D238
    private static void TrainerCase_FreeSprites(TrainerCaseSpriteData spriteData)
    {
        for (int i = 0; i < TrainerCase.MaxBadges; i++)
        {
            Heap.Heap_Free(spriteData.BadgePaletteBuffers[i]!);
        }
        SpriteTransfer.SpriteTransfer_ResetCharTransfer(spriteData.SpriteResources[0]);
        SpriteTransfer.SpriteTransfer_ResetPlttTransfer(spriteData.SpriteResources[1]);
        for (int i = 0; i < 4; i++)
        {
            SpriteResourceCollection.SpriteResourceCollection_Delete(spriteData.SpriteResourceCollections[i]);
        }
        Sprite.SpriteList_Delete(spriteData.SpriteList);
        RenderOam.RenderOam_Free();
        CharTransfer.CharTransfer_Free();
        PlttTransfer.PlttTransfer_Free();
    }

    // OFFSET 0x0223D298
    private static void TrainerCase_DrawBadgeDirt(TrainerCaseSpriteData spriteData, int badge, byte dirtLevel)
    {
        if (badge >= TrainerCase.MaxBadges)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (dirtLevel > 3)
        {
            ErrorHandling.GF_AssertFail();
        }
        ImagePaletteProxy proxy = Sprite.Sprite_GetPaletteProxy(spriteData.Sprites[badge]!);
        int location = G2d.GetImagePaletteLocation(proxy, VramTypeMain);
        PaletteFile palette = spriteData.BadgePalettes[badge]!;
        Video.LoadPaletteRange(Video.MainObject, palette.Colors, dirtLevel * 0x10, (uint)(location + (badge << 5)), 0x20);
    }

    // OFFSET 0x0223D2F4
    private static void TrainerCase_InitCharPlttTransferBuffers()
    {
        uint[] raw = RomData.OverlayU32(Overlay, CharTransferTemplateAddress, 4);
        CharTransferTemplate template = new()
        {
            MaxTasks = (int)raw[0],
            SizeMain = (int)raw[1],
            SizeSub = (int)raw[2],
            HeapID = (int)raw[3],
        };
        CharTransfer.CharTransfer_Init(template);
        PlttTransfer.PlttTransfer_Init(2, HeapID);
        CharTransfer.CharTransfer_ClearBuffers();
        PlttTransfer.PlttTransfer_Clear();
    }
}
