namespace PlatinumSharp.Game;

public static partial class BattleBags
{
    private const uint SpriteManagerCapacities = 0x02229BE4;
    private const uint SpriteResourcesAndPriority = 0x02229CBC;
    private const int SpriteResourcesAndPrioritySize = 0x14;
    private const uint PocketItemSpritePositions = 0x02229C44;
    private const uint CatchTutorialCursorPositions = 0x02229BFC;
    private const uint ScreenCursorPositions = 0x02229BC8;
    private const int MaxSprites = 0xC;
    private const int NarcItemIcons = 0x10;
    private const int PlttBufSubObj = 3;
    private const int VramTypeSub = 2;
    private const int ItemFileTypeIcon = 1;
    private const int ItemFileTypePalette = 2;
    private const int ResItemIcon = 0xB4B7;
    private const int ResCursorTiles = 0xB4BE;
    private const int ResCursor = 0xB4B9;
    private const int ResCatchTutorialCursorTiles = 0xB4BD;
    private const int ResCatchTutorialCursor = 0xB4B8;

    private static readonly int[] ScreenCursorPositionCounts = { 6, 9, 2 };

    private static SpriteSystem BagSpriteSystem(BattleBag battleBag)
    {
        return BattleSystems.BattleSystem_GetSpriteSystem(battleBag.Context!.BattleSys!)!;
    }

    private static ManagedSprite BagSprite(BattleBag battleBag, int index)
    {
        return battleBag.Sprites[index]!;
    }

    // OFFSET 0x02227BDC
    private static void BattleBagSprites_InitializeSprites(BattleBag battleBag)
    {
        InitializeSpriteManager(battleBag);
        LoadSpriteData(battleBag);
        InitializeSprites(battleBag);
        InitializeCursor(battleBag);
        InitializeCatchTutorialCursor(battleBag);
        GXLayers.GXLayers_EngineBToggleLayers(0x10, 1);
    }

    // OFFSET 0x02227C08
    private static void InitializeSpriteManager(BattleBag battleBag)
    {
        int[] capacities = new int[6];
        for (int i = 0; i < capacities.Length; i++)
        {
            capacities[i] = (int)BattleSubMenuData.U32((uint)(SpriteManagerCapacities + i * 4));
        }
        SpriteSystem spriteSys = BagSpriteSystem(battleBag);
        battleBag.SpriteManager = SpriteSystem.SpriteManager_New(spriteSys);
        SpriteSystem.SpriteSystem_InitSprites(spriteSys, battleBag.SpriteManager, MaxSprites);
        SpriteSystem.SpriteSystem_InitManagerWithCapacities(spriteSys, battleBag.SpriteManager, capacities);
    }

    // OFFSET 0x02227C54
    private static void LoadSpriteData(BattleBag battleBag)
    {
        NARC narc = NARC.NARC_ctor(NarcItemIcons, battleBag.Context!.HeapID);
        SpriteSystem spriteSys = BagSpriteSystem(battleBag);
        for (int i = 0; i < NumSpritesPerPage; i++)
        {
            SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSys, battleBag.SpriteManager!, narc, Item.Item_FileID(1, ItemFileTypeIcon), 0, VramTypeSub, ResItemIcon + i);
            SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(battleBag.Palette!, PlttBufSubObj, spriteSys, battleBag.SpriteManager!, narc, Item.Item_FileID(1, ItemFileTypePalette), 0, 1, VramTypeSub, ResItemIcon + i);
        }
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(spriteSys, battleBag.SpriteManager!, narc, Item.Item_IconCellFile(), 0, ResItemIcon);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(spriteSys, battleBag.SpriteManager!, narc, Item.Item_IconAnimFile(), 0, ResItemIcon);
        NARC.NARC_dtor(narc);
    }

    private const int NumSpritesPerPage = BattleBag.NumSprites;

    // OFFSET 0x02227D10
    private static void LoadItemIcon(BattleBag battleBag, ushort item, int resourceID)
    {
        SpriteSystem spriteSys = BagSpriteSystem(battleBag);
        SpriteSystem.SpriteSystem_ReplaceCharResObj(spriteSys, battleBag.SpriteManager!, NarcItemIcons, Item.Item_FileID(item, ItemFileTypeIcon), 0, resourceID);
    }

    // OFFSET 0x02227D48
    private static void LoadItemPaletteData(BattleBag battleBag, ushort item, ushort slot, int resourceID)
    {
        PaletteData.PaletteData_LoadBufferFromFileStart(battleBag.Palette!, NarcItemIcons, Item.Item_FileID(item, ItemFileTypePalette), battleBag.Context!.HeapID, PlttBufSubObj, 0x20, (ushort)(slot << 4));
    }

    // OFFSET 0x02227D78
    private static ManagedSprite? CreateSprite(BattleBag battleBag, int spriteIndex)
    {
        SpriteSystem spriteSys = BagSpriteSystem(battleBag);
        uint entry = (uint)(SpriteResourcesAndPriority + spriteIndex * SpriteResourcesAndPrioritySize);
        SpriteTemplate template = new();
        template.X = 0;
        template.Y = 0;
        template.Z = 0;
        template.AnimIdx = 0;
        template.Priority = (int)BattleSubMenuData.U32(entry + 0x10);
        template.PlttIdx = 0;
        template.VramType = VramTypeSub;
        template.Resources[0] = (int)BattleSubMenuData.U32(entry);
        template.Resources[1] = (int)BattleSubMenuData.U32(entry + 4);
        template.Resources[2] = (int)BattleSubMenuData.U32(entry + 8);
        template.Resources[3] = (int)BattleSubMenuData.U32(entry + 0xC);
        template.Resources[4] = -1;
        template.Resources[5] = -1;
        template.BgPriority = 1;
        template.VramTransfer = 0;
        return SpriteSystem.SpriteSystem_NewSprite(spriteSys, battleBag.SpriteManager!, template);
    }

    // OFFSET 0x02227DE8
    private static void InitializeSprites(BattleBag battleBag)
    {
        for (int i = 0; i < BattleBag.NumSprites; i++)
        {
            battleBag.Sprites[i] = CreateSprite(battleBag, i);
        }
    }

    // OFFSET 0x02227E08
    private static void BattleBagSprites_CleanupSprites(BattleBag battleBag)
    {
        SpriteSystem spriteSys = BagSpriteSystem(battleBag);
        for (int i = 0; i < BattleBag.NumSprites; i++)
        {
            SpriteSystem.Sprite_DeleteAndFreeResources(BagSprite(battleBag, i));
        }
        CleanupCursor(battleBag);
        CleanupCatchTutorialCursor(battleBag);
        SpriteSystem.SpriteSystem_FreeResourcesAndManager(spriteSys, battleBag.SpriteManager!);
    }

    // OFFSET 0x02227E48
    private static void DrawSprite(ManagedSprite sprite, int x, int y)
    {
        SpriteSystem.ManagedSprite_SetDrawFlag(sprite, 1);
        SpriteSystem.ManagedSprite_SetPositionXY(sprite, (short)x, (short)y);
    }

    // OFFSET 0x02227E68
    private static void BattleBagSprites_SetupScreen(BattleBag battleBag, int screen)
    {
        for (int i = 0; i < BattleBag.NumSprites; i++)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(BagSprite(battleBag, i), 0);
        }
        switch (screen)
        {
            case ScreenMenu:
                DrawLastUsedItem(battleBag);
                break;
            case ScreenPocketMenu:
                DrawPocketItems(battleBag);
                break;
            case ScreenUseItem:
                DrawSelectedItem(battleBag);
                break;
        }
    }

    // OFFSET 0x02227EAC
    private static void DrawLastUsedItem(BattleBag battleBag)
    {
        ushort item = battleBag.Context!.LastUsedItem;
        if (item == 0)
        {
            return;
        }
        LoadItemIcon(battleBag, item, ResItemIcon);
        LoadItemPaletteData(battleBag, battleBag.Context.LastUsedItem, 0, ResItemIcon);
        DrawSprite(BagSprite(battleBag, 0), 0x18, 0xB2);
    }

    // OFFSET 0x02227EE0
    private static void DrawPocketItems(BattleBag battleBag)
    {
        for (int i = 0; i < NumSpritesPerPage; i++)
        {
            ushort item = BattleBag_GetItem(battleBag, i);
            if (item == 0)
            {
                continue;
            }
            LoadItemIcon(battleBag, item, ResItemIcon + i);
            LoadItemPaletteData(battleBag, item, (ushort)i, ResItemIcon + i);
            DrawSprite(BagSprite(battleBag, i), (int)BattleSubMenuData.U32((uint)(PocketItemSpritePositions + i * 8)), (int)BattleSubMenuData.U32((uint)(PocketItemSpritePositions + i * 8 + 4)));
        }
    }

    // OFFSET 0x02227F38
    private static void DrawSelectedItem(BattleBag battleBag)
    {
        ushort item = BattleBag_GetItem(battleBag, battleBag.Context!.PocketCurrentPagePositions[battleBag.CurrentBattlePocket]);
        LoadItemIcon(battleBag, item, ResItemIcon);
        LoadItemPaletteData(battleBag, item, 0, ResItemIcon);
        DrawSprite(BagSprite(battleBag, 0), 0x28, 0x2C);
    }

    // OFFSET 0x02227F7C
    private static void InitializeCursor(BattleBag battleBag)
    {
        SpriteSystem spriteSys = BagSpriteSystem(battleBag);
        BattleCursors.BattleCursor_LoadResources(spriteSys, battleBag.SpriteManager!, battleBag.Palette!, battleBag.Context!.HeapID, ResCursorTiles, ResCursorTiles, ResCursor, ResCursor);
        BattleCursor cursor = BattleCursors.BattleCursor_New(spriteSys, battleBag.SpriteManager!, battleBag.Context.HeapID, ResCursorTiles, ResCursorTiles, ResCursor, ResCursor, 0, 1);
        BattleSubMenuCursor.SetBattleSubMenuCursorSprites(battleBag.Cursor!, cursor);
    }

    // OFFSET 0x02227FDC
    private static void CleanupCursor(BattleBag battleBag)
    {
        BattleCursors.BattleCursor_Free(BattleSubMenuCursor.GetBattleSubMenuCursorSprites(battleBag.Cursor!)!);
        BattleCursors.BattleCursor_FreeResources(battleBag.SpriteManager!, ResCursorTiles, ResCursorTiles, ResCursor, ResCursor);
    }

    // OFFSET 0x02228008
    private static void BattleBagSprites_SetupCursor(BattleBag battleBag, int screen)
    {
        uint positions = BattleSubMenuData.U32((uint)(ScreenCursorPositions + screen * 4));
        BattleSubMenuCursor.SetBattleSubMenuCursorPositions(battleBag.Cursor!, BattleSubMenuData.CursorPositions(positions, ScreenCursorPositionCounts[screen]));
        if (screen == ScreenMenu)
        {
            BattleSubMenuCursor.SetBattleSubMenuCursorCurrentPosition(battleBag.Cursor!, battleBag.CurrentBattlePocket);
        }
        else if (screen == ScreenPocketMenu)
        {
            BattleSubMenuCursor.SetBattleSubMenuCursorCurrentPosition(battleBag.Cursor!, battleBag.Context!.PocketCurrentPagePositions[battleBag.CurrentBattlePocket]);
        }
    }

    // OFFSET 0x02228050
    private static void BattleBagSprites_DisableCursor(BattleBag battleBag)
    {
        BattleSubMenuCursor.SetBattleSubMenuCursorVisibility(battleBag.Cursor!, 0);
        BattleSubMenuCursor.ResetBattleSubMenuCursorCurrentPosition(battleBag.Cursor!);
        BattleCursors.BattleCursor_Hide(BattleSubMenuCursor.GetBattleSubMenuCursorSprites(battleBag.Cursor!)!);
    }

    // OFFSET 0x02228070
    private static void InitializeCatchTutorialCursor(BattleBag battleBag)
    {
        SpriteSystem spriteSys = BagSpriteSystem(battleBag);
        BattleSubscreens.TouchIndicator_LoadResources(spriteSys, battleBag.SpriteManager!, battleBag.Context!.HeapID, battleBag.Palette!, ResCatchTutorialCursorTiles, ResCatchTutorialCursorTiles, ResCatchTutorialCursor, ResCatchTutorialCursor);
        battleBag.CatchTutorialCursor = BattleSubscreens.TouchIndicator_New(spriteSys, battleBag.SpriteManager!, battleBag.Context.HeapID, ResCatchTutorialCursorTiles, ResCatchTutorialCursorTiles, ResCatchTutorialCursor, ResCatchTutorialCursor, 0, 0);
    }

    // OFFSET 0x022280C8
    private static void CleanupCatchTutorialCursor(BattleBag battleBag)
    {
        BattleSubscreens.TouchIndicator_Free(battleBag.CatchTutorialCursor!);
        BattleSubscreens.TouchIndicator_UnloadResources(battleBag.SpriteManager!, ResCatchTutorialCursorTiles, ResCatchTutorialCursorTiles, ResCatchTutorialCursor, ResCatchTutorialCursor);
    }

    // OFFSET 0x022280F0
    private static void BattleBagSprites_SetupCatchTutorialCursor(BattleBag battleBag, int screen)
    {
        if (battleBag.Context!.IsInCatchTutorial == 1)
        {
            int x = (int)BattleSubMenuData.U32((uint)(CatchTutorialCursorPositions + screen * 8));
            int y = (int)BattleSubMenuData.U32((uint)(CatchTutorialCursorPositions + screen * 8 + 4));
            BattleSubscreens.TouchIndicator_ShowAtDefaultOffset(battleBag.CatchTutorialCursor!, x, y);
            BattleSubscreens.TouchIndicator_SetTimer(battleBag.CatchTutorialCursor!, 0x3C);
        }
        else
        {
            BattleSubscreens.TouchIndicator_Hide(battleBag.CatchTutorialCursor!);
        }
    }
}
