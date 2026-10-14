namespace PlatinumSharp.Game;

public static partial class BattleSubscreens
{
    private const int ResourcePokeIconTiles = 0x4E44;
    private const int ResourcePokeIconAnim = 0x4E35;
    private const int ResourcePokeIconPalette = 0x4E36;
    private const int ResourceCategoryIconTiles = 0x4E3D;
    private const int TargetIconCount = 4;
    private const int PokeIconTileSets = 3;

    // OFFSET 0x0226A718
    private static void BattleSubscreen_UnloadPokeIconResources(BattleSubscreen subscreen)
    {
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(subscreen.BattleSys!)!;
        for (int i = 0; i < PokeIconTileSets; i++)
        {
            SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, ResourcePokeIconTiles + i);
        }
        SpriteSystem.SpriteManager_UnloadCellObjById(spriteMan, ResourcePokeIconAnim);
        SpriteSystem.SpriteManager_UnloadAnimObjById(spriteMan, ResourcePokeIconAnim);
        SpriteSystem.SpriteManager_UnloadPlttObjById(spriteMan, ResourcePokeIconPalette);
    }

    // OFFSET 0x0226A768
    private static void BattleSubscreen_DeleteTargetPokeIcons(BattleSubscreen subscreen)
    {
        for (int i = 0; i < TargetIconCount; i++)
        {
            if (subscreen.TargetSelectPokeIconSprites[i] != null)
            {
                SpriteSystem.Sprite_DeleteAndFreeResources(subscreen.TargetSelectPokeIconSprites[i]!);
                subscreen.TargetSelectPokeIconSprites[i] = null;
            }
            if (subscreen.PokeIconAnimTasks[i] != null)
            {
                SysTask.SysTask_Done(subscreen.PokeIconAnimTasks[i]!);
                subscreen.PokeIconAnimTasks[i] = null;
            }
        }
    }

    // OFFSET 0x0226AAC0
    private static void BattleSubscreen_DeleteFontOAMs(BattleSubscreen subscreen)
    {
        for (int i = 0; i < FontOAMCount; i++)
        {
            FontOAMEntry entry = subscreen.FontOAMEntry[i];
            if (entry.FontOAM != null)
            {
                FontOAMSystem.FontOAM_Delete(entry.FontOAM);
                CharTransfer.CharTransfer_ClearRange(entry.Allocation);
                entry.FontOAM = null;
            }
        }
    }

    // OFFSET 0x0226AFF4
    private static void BattleSubscreen_DeleteMoveTypeIcons(BattleSubscreen subscreen)
    {
        BattleSystems.BattleSystem_GetSpriteSystem(subscreen.BattleSys!);
        BattleSystems.BattleSystem_GetSpriteManager(subscreen.BattleSys!);
        for (int i = 0; i < MoveSlots; i++)
        {
            if (subscreen.MoveSelectSprites[i] != null)
            {
                TypeIconSprites.TypeIcon_DeleteSprite(subscreen.MoveSelectSprites[i]!);
                subscreen.MoveSelectSprites[i] = null;
            }
        }
    }

    // OFFSET 0x0226B028
    private static void BattleSubscreen_DeleteCategoryIcons(BattleSubscreen subscreen)
    {
        BattleSystems.BattleSystem_GetSpriteSystem(subscreen.BattleSys!);
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(subscreen.BattleSys!)!;
        for (int i = 0; i < MoveSlots; i++)
        {
            if (subscreen.CategoryIconSprites[i] != null)
            {
                CategoryIcon.CategoryIcon_DeleteSprite(subscreen.CategoryIconSprites[i]!);
                CategoryIcon.CategoryIcon_UnloadChar(spriteMan, ResourceCategoryIconTiles + i);
                subscreen.CategoryIconSprites[i] = null;
            }
        }
    }

    // OFFSET 0x0226B1E8
    public static void BattleSubscreen_DeleteMenuSprites(BattleSubscreen subscreen)
    {
        BattleSubscreen_DeleteMoveTypeIcons(subscreen);
        BattleSubscreen_DeleteCategoryIcons(subscreen);
        BattleSubscreen_DeleteFontOAMs(subscreen);
        BattleSubscreen_UnloadPokeIconResources(subscreen);
        BattleSubscreen_DeleteTargetPokeIcons(subscreen);
    }
}
