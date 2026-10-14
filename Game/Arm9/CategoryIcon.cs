namespace PlatinumSharp.Game;

public static class TypeIconSprites
{
    // OFFSET 0x0207CA58
    public static ManagedSprite? TypeIcon_NewSprite(SpriteSystem spriteSys, SpriteManager spriteMan, int moveType, SpriteTemplate template)
    {
        SpriteTemplate copy = new();
        copy.X = template.X;
        copy.Y = template.Y;
        copy.Z = template.Z;
        copy.AnimIdx = template.AnimIdx;
        copy.Priority = template.Priority;
        copy.VramType = template.VramType;
        Array.Copy(template.Resources, copy.Resources, copy.Resources.Length);
        copy.BgPriority = template.BgPriority;
        copy.VramTransfer = template.VramTransfer;
        copy.PlttIdx = TypeIcon.TypeIcon_GetPlttIndex(moveType);
        return SpriteSystem.SpriteSystem_NewSprite(spriteSys, spriteMan, copy);
    }

    // OFFSET 0x0207CA88
    public static void TypeIcon_DeleteSprite(ManagedSprite sprite)
    {
        SpriteSystem.Sprite_DeleteAndFreeResources(sprite);
    }
}

public static class CategoryIcon
{
    public const int ClassCount = 3;

    private const uint TileMemberTable = 0x020F0AE4;
    private const uint PaletteIndexTable = 0x020F0AE0;
    private const int NarcBattleSubscreen = 0x1B;

    private static uint[]? tileMembers;
    private static byte[]? paletteIndices;

    // OFFSET 0x0207CA90
    public static uint CategoryIcon_GetChar(int moveClass)
    {
        if (moveClass >= ClassCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        tileMembers ??= RomData.Arm9U32(TileMemberTable, ClassCount);
        return tileMembers[moveClass];
    }

    // OFFSET 0x0207CAA8
    public static byte CategoryIcon_GetPlttIndex(int moveClass)
    {
        if (moveClass >= ClassCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        paletteIndices ??= RomData.Arm9(PaletteIndexTable, ClassCount);
        return paletteIndices[moveClass];
    }

    // OFFSET 0x0207CAC0
    public static int CategoryIcon_GetNARC()
    {
        return NarcBattleSubscreen;
    }

    // OFFSET 0x0207CAC4
    public static void CategoryIcon_LoadChar(SpriteSystem spriteSys, SpriteManager spriteMan, int vramType, int moveClass, int resourceID)
    {
        SpriteSystem.SpriteSystem_LoadCharResObj(spriteSys, spriteMan, CategoryIcon_GetNARC(), (int)CategoryIcon_GetChar(moveClass), 1, vramType, resourceID);
    }

    // OFFSET 0x0207CAF8
    public static void CategoryIcon_UnloadChar(SpriteManager spriteMan, int resourceID)
    {
        SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, resourceID);
    }

    // OFFSET 0x0207CB00
    public static void CategoryIcon_DeleteSprite(ManagedSprite sprite)
    {
        SpriteSystem.Sprite_DeleteAndFreeResources(sprite);
    }
}
