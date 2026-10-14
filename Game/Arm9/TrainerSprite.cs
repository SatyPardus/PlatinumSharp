namespace PlatinumSharp.Game;

public sealed class TrainerSpriteResources
{
    public int NarcID;
    public int Tiles;
    public int Palette;
    public int Cell;
    public int Anim;
    public int Unused;
}

public static class TrainerSprites
{
    public const int FaceBack = 0;
    public const int FaceFront = 2;

    private const int NarcTrainerFront = 0x3C;
    private const int NarcTrainerBack = 6;
    private const int MembersPerClass = 5;
    private const int ResourceTiles = 0x4E2F;
    private const int ResourcePalette = 0x4E2A;
    private const int ResourceCell = 0x4E27;
    private const int PaletteBufferMainObj = 2;
    private const int VRamMain = 1;
    private const int ClassTwoPalettes = 0x66;
    private const uint PriorityTable = 0x020F0588;
    private const int ClassBackRival = 0x3F;
    private const int ClassBackPartnerFirst = 0x5A;
    private const int ClassBackFrontierFirst = 0x5B;
    private const int ClassBackLinkFirst = 0x67;

    // OFFSET 0x020788D0
    public static int TrainerClass_GetBackSpriteIndex(int trainerClass)
    {
        if (trainerClass == ClassBackRival)
        {
            return 2;
        }
        if (trainerClass == 0 || trainerClass == 1)
        {
            return trainerClass;
        }
        if (trainerClass >= ClassBackPartnerFirst && trainerClass <= ClassBackPartnerFirst + 4)
        {
            return trainerClass - 0x57;
        }
        if (trainerClass == ClassBackLinkFirst || trainerClass == ClassBackLinkFirst + 1)
        {
            return trainerClass - 0x5F;
        }
        if (TrainerData.TrainerClass_Gender(trainerClass) == 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02076994
    public static ManagedSprite TrainerSprite_New(SpriteSystem spriteSys, SpriteManager spriteMan, PaletteData paletteData, int x, int y, int trainerClass, int face, int resourceIndex, int heapID)
    {
        TrainerSpriteResources resources = new();
        int paletteCount = 1;
        TrainerSprite_GetResources(trainerClass, face, resources);
        if (trainerClass == ClassTwoPalettes)
        {
            paletteCount = 2;
        }
        NARC narc = NARC.NARC_ctor(resources.NarcID, heapID);
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSys, spriteMan, narc, resources.Tiles, 0, VRamMain, resourceIndex + ResourceTiles);
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(paletteData, PaletteBufferMainObj, spriteSys, spriteMan, narc, resources.Palette, 0, paletteCount, VRamMain, resourceIndex + ResourcePalette);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(spriteSys, spriteMan, narc, resources.Cell, 0, resourceIndex + ResourceCell);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(spriteSys, spriteMan, narc, resources.Anim, 0, resourceIndex + ResourceCell);
        NARC.NARC_dtor(narc);
        SpriteTemplate template = new();
        template.X = 0;
        template.Y = 0;
        template.Z = 0;
        template.AnimIdx = 0;
        template.PlttIdx = 0;
        template.VramType = VRamMain;
        template.Resources[0] = resourceIndex + ResourceTiles;
        template.Resources[1] = resourceIndex + ResourcePalette;
        template.Resources[2] = resourceIndex + ResourceCell;
        template.Resources[3] = resourceIndex + ResourceCell;
        template.Resources[4] = -1;
        template.Resources[5] = -1;
        template.BgPriority = 2;
        template.VramTransfer = 1;
        template.Priority = (int)RomData.Arm9U32((uint)(PriorityTable + resourceIndex * 4), 1)[0];
        ManagedSprite sprite = SpriteSystem.SpriteSystem_NewSprite(spriteSys, spriteMan, template)!;
        Sprite.Sprite_SetExplicitPaletteOffsetAutoAdjust(sprite.Sprite!, 0);
        SpriteSystem.ManagedSprite_SetPositionXY(sprite, (short)x, (short)y);
        SpriteSystem.ManagedSprite_TickFrame(sprite);
        SpriteSystem.ManagedSprite_SetAnimateFlag(sprite, 1);
        return sprite;
    }

    // OFFSET 0x02076AAC
    public static void TrainerSprite_GetResources(int trainerClass, int face, TrainerSpriteResources resources)
    {
        int first;
        if (face == FaceFront)
        {
            resources.NarcID = NarcTrainerFront;
            first = trainerClass * MembersPerClass;
        }
        else
        {
            resources.NarcID = NarcTrainerBack;
            first = TrainerClass_GetBackSpriteIndex(trainerClass) * MembersPerClass;
        }
        resources.Tiles = first;
        resources.Palette = first + 1;
        resources.Cell = first + 2;
        resources.Anim = first + 3;
        resources.Unused = first + 4;
    }
}
