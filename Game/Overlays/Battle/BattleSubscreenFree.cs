namespace PlatinumSharp.Game;

public static partial class BattleSubscreens
{
    // OFFSET 0x02268A14
    public static void BattleSubscreen_Free(BattleSubscreen subscreen)
    {
        if (subscreen.SuppressActivationSfxPtr != null)
        {
            subscreen.SuppressActivationSfxPtr[0] = subscreen.SuppressActivationSfx;
        }
        BattleSubscreen_DeleteMenuSprites(subscreen);
        BattleSubscreen_FreePartyBalls(subscreen);
        BattleSubscreen_FreeGraphics(subscreen);
        FontOAMSystem.FontOAMManager_Free(subscreen.FontOAMManager);
        SysTask.SysTask_Done(subscreen.PulseCursorTask!);
        for (int i = 0; i < TilemapBufferCount; i++)
        {
            subscreen.TilemapBuffers[i] = null;
        }
        subscreen.SubscreenPaletteBuf = null;
        BattleSubscreen_FreeMoveDisplayData(subscreen);
        subscreen.NormalMovePalBuf = null;
        subscreen.SpeedUpMovePalBuf = null;
        SysTask.SysTask_Done(subscreen.SpeedUpPaletteTask!);
    }

    // OFFSET 0x02268B8C
    private static void BattleSubscreen_FreeGraphics(BattleSubscreen subscreen)
    {
        BattleSystems.BattleSystem_GetSpriteSystem(subscreen.BattleSys!);
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(subscreen.BattleSys!)!;
        for (int i = 0; i < MoveSlots; i++)
        {
            TypeIcon.TypeIcon_UnloadChar(spriteMan, ResourceTypeIconTiles + i);
        }
        TypeIcon.TypeIcon_UnloadPltt(spriteMan, ResourceTypeIconPalette);
        TypeIcon.TypeIcon_UnloadAnim(spriteMan, ResourceTypeIconAnim, ResourceTypeIconAnim);
        if ((BattleSystems.BattleSystem_GetBattleType(subscreen.BattleSys!) & BattleSystems.BattleTypeCatchTutorial) != 0)
        {
            TouchIndicator_Free((TouchIndicator)subscreen.CatchTutorialState.Indicator!);
            TouchIndicator_UnloadResources(spriteMan, ResourceIndicatorTiles, ResourceIndicatorPalette, ResourceIndicatorCell, ResourceIndicatorAnim);
        }
    }

    // OFFSET 0x02268F00
    private static void BattleSubscreen_FreePartyBalls(BattleSubscreen subscreen)
    {
        if (subscreen.PlayerPartyBallSprites[0] == null || subscreen.OpponentPartyBallSprites[0] == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        BattleSystems.BattleSystem_GetSpriteSystem(subscreen.BattleSys!);
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(subscreen.BattleSys!)!;
        SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, ResourcePlayerBallTiles);
        SpriteSystem.SpriteManager_UnloadCellObjById(spriteMan, ResourcePlayerBallAnim);
        SpriteSystem.SpriteManager_UnloadAnimObjById(spriteMan, ResourcePlayerBallAnim);
        SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, ResourceOpponentBallTiles);
        SpriteSystem.SpriteManager_UnloadCellObjById(spriteMan, ResourceOpponentBallAnim);
        SpriteSystem.SpriteManager_UnloadAnimObjById(spriteMan, ResourceOpponentBallAnim);
        SpriteSystem.SpriteManager_UnloadPlttObjById(spriteMan, ResourceSubscreenPalette);
        for (int i = 0; i < PartySize; i++)
        {
            SpriteSystem.Sprite_DeleteAndFreeResources(subscreen.PlayerPartyBallSprites[i]!);
            subscreen.PlayerPartyBallSprites[i] = null;
            SpriteSystem.Sprite_DeleteAndFreeResources(subscreen.OpponentPartyBallSprites[i]!);
            subscreen.OpponentPartyBallSprites[i] = null;
        }
        SysTask.SysTask_Done(subscreen.AnimatePartyBallsTask!);
        subscreen.AnimatePartyBallsTask = null;
        BattleCursors.BattleCursor_FreeResources(spriteMan, ResourceCursorTiles, ResourceCursorPalette, ResourceCursorCell, ResourceCursorAnim);
        BattleCursors.BattleCursor_Free(subscreen.CursorRenderer!);
    }
}
