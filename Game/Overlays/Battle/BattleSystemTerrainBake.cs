namespace PlatinumSharp.Game;

public static partial class BattleSystems
{
    private const int BgSnapshotOffset = 0x10000;
    private const int BgSnapshotSize = 0x10000;
    private const int PaletteSnapshotColors = 0x100;
    private const int BakeVRamMain = 1;
    private const int BakeDisplayMain = 0;
    private const int BakeBgLayer = 3;
    private const int BakePaletteBase = 0x70;
    private const int BgTileBytes = 0x40;
    private const int BgRowBytes = 0x800;
    private const int ObjTileBytes = 0x20;
    private const int EnemyTerrainTop = 0x14;
    private const int EnemyTerrainBottom = 0x1C;
    private const int EnemyTerrainLeft = 0x10;
    private const int EnemyTerrainRight = 0x20;
    private const int EnemyTerrainHalfWidth = 8;
    private const int EnemyTerrainSecondHalf = 0x700;
    private const int PlayerTerrainStripDest = 0x9800;
    private const int PlayerTerrainStripPixels = 0x800;
    private const int PlayerTerrainTop = 0x1C;
    private const int PlayerTerrainBottom = 0x20;
    private const int PlayerTerrainWidth = 0x18;
    private const int PlayerTerrainBlockBytes = 0x400;

    private static int BakePixel(byte[] objTiles, int address, int pixel)
    {
        byte value = objTiles[address + pixel / 2];
        return (pixel & 1) != 0 ? (value & 0xF0) >> 4 : value & 0xF;
    }

    // OFFSET 0x0223EF8C
    public static void BattleSystem_BakeTerrainSprites(BattleSystem battleSys)
    {
        battleSys.BgTileSnapshot = new byte[BgSnapshotSize];
        battleSys.BgPaletteSnapshot = new ushort[PaletteSnapshotColors];
        Array.Copy(Video.GetBgVram(BakeDisplayMain), BgSnapshotOffset, battleSys.BgTileSnapshot, 0, BgSnapshotSize);
        Array.Copy(PaletteData.PaletteData_GetUnfadedBuffer(battleSys.PaletteData!, 0)!, battleSys.BgPaletteSnapshot, PaletteSnapshotColors);
        byte[] snapshot = battleSys.BgTileSnapshot;
        byte[] objTiles = Video.GetObjectTiles(BakeDisplayMain);
        int source = G2d.GetImageLocation(Sprite.Sprite_GetImageProxy(battleSys.Terrains[1].ManagedSprite!.Sprite!), BakeVRamMain);
        for (int row = EnemyTerrainTop; row < EnemyTerrainBottom; row++)
        {
            int rowSource = source + (row - EnemyTerrainTop) * 0x100;
            for (int column = EnemyTerrainLeft; column < EnemyTerrainRight; column++)
            {
                int tile = column - EnemyTerrainLeft;
                int tileSource = rowSource + tile * ObjTileBytes;
                int dest = row * BgRowBytes + column * BgTileBytes;
                for (int i = 0; i < BgTileBytes; i++)
                {
                    int pixel = tile < EnemyTerrainHalfWidth ? BakePixel(objTiles, tileSource, i) : BakePixel(objTiles, tileSource + EnemyTerrainSecondHalf, i);
                    if (pixel != 0)
                    {
                        snapshot[dest + i] = (byte)(pixel + BakePaletteBase);
                    }
                }
            }
        }
        source = G2d.GetImageLocation(Sprite.Sprite_GetImageProxy(battleSys.Terrains[0].ManagedSprite!.Sprite!), BakeVRamMain);
        for (int i = 0; i < PlayerTerrainStripPixels; i++)
        {
            int pixel = BakePixel(objTiles, source, i);
            if (pixel != 0)
            {
                snapshot[PlayerTerrainStripDest + i] = (byte)(pixel + BakePaletteBase);
            }
        }
        for (int row = PlayerTerrainTop; row < PlayerTerrainBottom; row++)
        {
            int rowSource = source + (row - PlayerTerrainTop) * 0x100;
            for (int column = 0; column < PlayerTerrainWidth; column++)
            {
                int tileSource = rowSource + column / 8 * PlayerTerrainBlockBytes + column % 8 * ObjTileBytes;
                int dest = row * BgRowBytes + column * BgTileBytes;
                for (int i = 0; i < BgTileBytes; i++)
                {
                    int pixel = BakePixel(objTiles, tileSource + PlayerTerrainBlockBytes, i);
                    if (pixel != 0)
                    {
                        snapshot[dest + i] = (byte)(pixel + BakePaletteBase);
                    }
                }
            }
        }
        Bg.Bg_LoadTiles(battleSys.BgConfig!, BakeBgLayer, snapshot, BgSnapshotSize, 0);
        BattleTerrainSprites.TerrainSprite_Free(battleSys.Terrains[0]);
        BattleTerrainSprites.TerrainSprite_Free(battleSys.Terrains[1]);
    }
}

public static partial class BattleIO
{
    public const int CommandBakeTerrain = 0x2E;

    // OFFSET 0x022665AC
    public static void BattleIO_BakeTerrain(BattleSystem battleSys, int battler)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandBakeTerrain);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }
}

public static partial class Battlers
{
    // OFFSET 0x0225C77C
    private static void Battler_CommandBakeTerrain(BattleSystem battleSys, BattlerData battler)
    {
        BattleSystems.BattleSystem_BakeTerrainSprites(battleSys);
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, BattleIO.CommandBakeTerrain);
        Battler_ClearCommand(battler);
    }
}

public static partial class BattleScripts
{
    // OFFSET 0x02247FE8
    private static int BattleScript_BakeTerrain(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleIO.BattleIO_BakeTerrain(battleSys, 0);
        return 0;
    }
}
