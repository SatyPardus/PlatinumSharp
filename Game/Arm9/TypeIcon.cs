namespace PlatinumSharp.Game;

public static class TypeIcon
{
    public const int TypeCount = 23;

    private const uint TileMemberTable = 0x020F0AF0;
    private const uint PaletteIndexTable = 0x020F0B4C;
    private const int PaletteMember = 0x4A;
    private const int CellMember = 0xF2;
    private const int AnimMember = 0xF3;
    private const int NarcBattleSubscreen = 0x1B;
    private const int PaletteCount = 3;

    private static uint[]? tileMembers;
    private static byte[]? paletteIndices;

    // OFFSET 0x0207C908
    public static uint TypeIcon_GetChar(int moveType)
    {
        if (moveType >= TypeCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        tileMembers ??= RomData.Arm9U32(TileMemberTable, TypeCount);
        return tileMembers[moveType];
    }

    // OFFSET 0x0207C920
    public static int TypeIcon_GetPltt()
    {
        return PaletteMember;
    }

    // OFFSET 0x0207C924
    public static int TypeIcon_GetCell()
    {
        return CellMember;
    }

    // OFFSET 0x0207C928
    public static int TypeIcon_GetAnim()
    {
        return AnimMember;
    }

    // OFFSET 0x0207C92C
    public static byte TypeIcon_GetPlttIndex(int moveType)
    {
        if (moveType >= TypeCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        paletteIndices ??= RomData.Arm9(PaletteIndexTable, TypeCount);
        return paletteIndices[moveType];
    }

    // OFFSET 0x0207C944
    public static int TypeIcon_GetNARC()
    {
        return NarcBattleSubscreen;
    }

    // OFFSET 0x0207C948
    public static void TypeIcon_LoadChar(SpriteSystem spriteSys, SpriteManager spriteMan, int vramType, int moveType, int resourceID)
    {
        SpriteSystem.SpriteSystem_LoadCharResObj(spriteSys, spriteMan, TypeIcon_GetNARC(), (int)TypeIcon_GetChar(moveType), 1, vramType, resourceID);
    }

    // OFFSET 0x0207C97C
    public static void TypeIcon_LoadPltt(SpriteSystem spriteSys, SpriteManager spriteMan, int vramType, int resourceID)
    {
        SpriteSystem.SpriteSystem_LoadPlttResObj(spriteSys, spriteMan, TypeIcon_GetNARC(), TypeIcon_GetPltt(), 0, PaletteCount, vramType, resourceID);
    }

    // OFFSET 0x0207C9B0
    public static void TypeIcon_LoadPlttSrc(PaletteData paletteData, int bufferID, SpriteSystem spriteSys, SpriteManager spriteMan, int vramType, int resourceID)
    {
        SpriteSystem.SpriteSystem_LoadPaletteBuffer(paletteData, bufferID, spriteSys, spriteMan, TypeIcon_GetNARC(), TypeIcon_GetPltt(), 0, PaletteCount, vramType, resourceID);
    }

    // OFFSET 0x0207C9EC
    public static void TypeIcon_LoadAnim(SpriteSystem spriteSys, SpriteManager spriteMan, int cellResourceID, int animResourceID)
    {
        SpriteSystem.SpriteSystem_LoadCellResObj(spriteSys, spriteMan, TypeIcon_GetNARC(), TypeIcon_GetCell(), 1, cellResourceID);
        SpriteSystem.SpriteSystem_LoadAnimResObj(spriteSys, spriteMan, TypeIcon_GetNARC(), TypeIcon_GetAnim(), 1, animResourceID);
    }

    // OFFSET 0x0207CA34
    public static void TypeIcon_UnloadChar(SpriteManager spriteMan, int resourceID)
    {
        SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, resourceID);
    }

    // OFFSET 0x0207CA3C
    public static void TypeIcon_UnloadPltt(SpriteManager spriteMan, int resourceID)
    {
        SpriteSystem.SpriteManager_UnloadPlttObjById(spriteMan, resourceID);
    }

    // OFFSET 0x0207CA44
    public static void TypeIcon_UnloadAnim(SpriteManager spriteMan, int cellResourceID, int animResourceID)
    {
        SpriteSystem.SpriteManager_UnloadCellObjById(spriteMan, cellResourceID);
        SpriteSystem.SpriteManager_UnloadAnimObjById(spriteMan, animResourceID);
    }
}
