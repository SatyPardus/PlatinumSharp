namespace PlatinumSharp.Game;

public static class BattleTerrainSprites
{
    public const int SidePlayer = 0;
    public const int SideEnemy = 1;
    public const int TerrainCount = 24;

    private const int NarcBattleSubscreen = 0x1B;
    private const int HeapIDBattle = 5;
    private const uint PlayerTilesTable = 0x0227009C;
    private const uint EnemyTilesTable = 0x0227006C;
    private const uint PaletteTable = 0x02270134;
    private const int PaletteVariants = 3;
    private const uint SpriteTemplates = 0x022700CC;
    private const int SpriteTemplateSize = 0x34;
    private const int ResourcePlayerTiles = 0x4E2D;
    private const int ResourceEnemyTiles = 0x4E2E;
    private const int ResourcePalette = 0x4E29;
    private const int CellResourceOffset = 8;
    private const int MemberPlayerCell = 0x80;
    private const int MemberPlayerAnim = 0x81;
    private const int MemberEnemyCell = 0x83;
    private const int MemberEnemyAnim = 0x84;
    private const int PaletteBufferMainBg = 0;
    private const int PaletteBufferMainObj = 2;
    private const int VRamMain = 1;
    private const uint BgPaletteSize = 0x20;
    private const ushort BgPaletteStart = 0x70;

    private static int TableU16(uint address, int index)
    {
        return Nitro.Bin.U16(RomData.Overlay(16, (uint)(address + index * 2), 2), 0);
    }

    // OFFSET 0x02268520
    private static void TerrainSprite_LoadResources(Terrain terrain)
    {
        NARC narc = NARC.NARC_ctor(NarcBattleSubscreen, HeapIDBattle);
        SpriteSystem spriteSys = BattleSystems.BattleSystem_GetSpriteSystem(terrain.BattleSys!)!;
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(terrain.BattleSys!)!;
        int timeOffset = BattleSystems.BattleSystem_GetBackgroundTimeOffset(terrain.BattleSys!);
        int tilesMember;
        int tilesResource;
        int cellMember;
        int animMember;
        if (terrain.Side == SidePlayer)
        {
            tilesMember = TableU16(PlayerTilesTable, terrain.TerrainType);
            tilesResource = ResourcePlayerTiles;
            cellMember = MemberPlayerCell;
            animMember = MemberPlayerAnim;
        }
        else
        {
            tilesMember = TableU16(EnemyTilesTable, terrain.TerrainType);
            tilesResource = ResourceEnemyTiles;
            cellMember = MemberEnemyCell;
            animMember = MemberEnemyAnim;
        }
        int cellResource = tilesResource - CellResourceOffset;
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSys, spriteMan, narc, tilesMember, 1, VRamMain, tilesResource);
        int paletteMember = TableU16(PaletteTable, terrain.TerrainType * PaletteVariants + timeOffset);
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(BattleSystems.BattleSystem_GetPaletteData(terrain.BattleSys!)!, PaletteBufferMainObj, spriteSys, spriteMan, narc, paletteMember, 0, 1, VRamMain, ResourcePalette);
        PaletteData.PaletteData_LoadBufferFromFileStart(BattleSystems.BattleSystem_GetPaletteData(terrain.BattleSys!)!, NarcBattleSubscreen, paletteMember, HeapIDBattle, PaletteBufferMainBg, BgPaletteSize, BgPaletteStart);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(spriteSys, spriteMan, narc, cellMember, 1, cellResource);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(spriteSys, spriteMan, narc, animMember, 1, cellResource);
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x0226862C
    private static void TerrainSprite_CreateSprite(Terrain terrain)
    {
        SpriteSystem spriteSys = BattleSystems.BattleSystem_GetSpriteSystem(terrain.BattleSys!)!;
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(terrain.BattleSys!)!;
        SpriteTemplate template = BattleSubscreens.RomSpriteTemplate((uint)(SpriteTemplates + terrain.Side * SpriteTemplateSize));
        terrain.ManagedSprite = SpriteSystem.SpriteSystem_NewSprite(spriteSys, spriteMan, template);
        SpriteSystem.Sprite_TickFrame(terrain.ManagedSprite!.Sprite!);
    }

    // OFFSET 0x02268660
    private static void TerrainSprite_DeleteSprite(Terrain terrain)
    {
        if (terrain.ManagedSprite != null)
        {
            SpriteSystem.Sprite_DeleteAndFreeResources(terrain.ManagedSprite);
            terrain.ManagedSprite = null;
        }
    }

    // OFFSET 0x02268674
    private static void TerrainSprite_UnloadResources(Terrain terrain)
    {
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(terrain.BattleSys!)!;
        int tilesResource = terrain.Side == SidePlayer ? ResourcePlayerTiles : ResourceEnemyTiles;
        int cellResource = tilesResource - CellResourceOffset;
        SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, tilesResource);
        SpriteSystem.SpriteManager_UnloadPlttObjById(spriteMan, ResourcePalette);
        SpriteSystem.SpriteManager_UnloadCellObjById(spriteMan, cellResource);
        SpriteSystem.SpriteManager_UnloadAnimObjById(spriteMan, cellResource);
    }

    // OFFSET 0x022686BC
    public static void TerrainSprite_SetDrawFlag(Terrain terrain, int draw)
    {
        if (terrain.ManagedSprite != null)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(terrain.ManagedSprite, draw);
        }
    }

    // OFFSET 0x022686CC
    public static void TerrainSprite_Init(Terrain terrain, BattleSystem battleSys, int side, int terrainType)
    {
        terrain.ManagedSprite = null;
        terrain.Unused0A = 0;
        terrain.Unused0C = 0;
        terrain.BattleSys = battleSys;
        terrain.Side = (byte)side;
        terrain.TerrainType = (byte)terrainType;
        if (terrainType >= TerrainCount)
        {
            ErrorHandling.GF_AssertFail();
            terrain.TerrainType = 0;
        }
        TerrainSprite_LoadResources(terrain);
        TerrainSprite_CreateSprite(terrain);
    }

    // OFFSET 0x02268700
    public static void TerrainSprite_Free(Terrain terrain)
    {
        TerrainSprite_DeleteSprite(terrain);
        TerrainSprite_UnloadResources(terrain);
        terrain.ManagedSprite = null;
        terrain.BattleSys = null;
        terrain.Side = 0;
        terrain.TerrainType = 0;
        terrain.Unused0A = 0;
        terrain.Unused0C = 0;
    }
}
