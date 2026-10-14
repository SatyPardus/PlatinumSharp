namespace PlatinumSharp.Game;

public static partial class BattleSubscreens
{
    private const int SubscreenTilesMember = 0x1C;
    private const int SubscreenTilesMemberFrontier = 0xA9;
    private const uint SubscreenTilesSize = 0x6000;
    private const int VRamSub = 2;
    private const int PaletteBufferSubObj = 3;
    private const int ResourceTypeIconAnim = 0x4E31;
    private const int ResourceTypeIconPalette = 0x4E34;
    private const int ResourceTypeIconTiles = 0x4E39;

    // OFFSET 0x02268A88
    public static void BattleSubscreen_LoadGraphics(BattleSubscreen subscreen)
    {
        SpriteSystem spriteSys = BattleSystems.BattleSystem_GetSpriteSystem(subscreen.BattleSys!)!;
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(subscreen.BattleSys!)!;
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!)!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(subscreen.BattleSys!)!;
        int member = (BattleSystems.BattleSystem_GetBattleType(subscreen.BattleSys!) & BattleSystems.BattleTypeFrontier) != 0 ? SubscreenTilesMemberFrontier : SubscreenTilesMember;
        Graphics.Graphics_LoadTilesToBgLayer(NarcBattleGraphics, (uint)member, bgConfig, SubscreenFirstLayer, 0, SubscreenTilesSize, 1, HeapIDBattle);
        TypeIcon.TypeIcon_LoadAnim(spriteSys, spriteMan, ResourceTypeIconAnim, ResourceTypeIconAnim);
        TypeIcon.TypeIcon_LoadPlttSrc(BattleSystems.BattleSystem_GetPaletteData(subscreen.BattleSys!)!, PaletteBufferSubObj, spriteSys, spriteMan, VRamSub, ResourceTypeIconPalette);
        for (int i = 0; i < MoveSlots; i++)
        {
            TypeIcon.TypeIcon_LoadChar(spriteSys, spriteMan, VRamSub, 0, ResourceTypeIconTiles + i);
        }
        if ((BattleSystems.BattleSystem_GetBattleType(subscreen.BattleSys!) & BattleSystems.BattleTypeCatchTutorial) != 0)
        {
            TouchIndicator_LoadResources(spriteSys, spriteMan, HeapIDBattle, paletteData, ResourceIndicatorTiles, ResourceIndicatorPalette, ResourceIndicatorCell, ResourceIndicatorAnim);
            subscreen.CatchTutorialState.Indicator = TouchIndicator_New(spriteSys, spriteMan, HeapIDBattle, ResourceIndicatorTiles, ResourceIndicatorPalette, ResourceIndicatorCell, ResourceIndicatorAnim, IndicatorPriority, 0);
        }
    }
}
