namespace PlatinumSharp.Game;

public static partial class BattleParties
{
    private const uint SpriteManagerCapacities = 0x022292EC;
    private const uint SpriteResourcesAndPriority = 0x02229514;
    private const int SpriteResourcesAndPrioritySize = 0x14;
    private const uint PartyScreenPokemonPositions = 0x02229404;
    private const uint PartyScreenStatusPositions = 0x02229434;
    private const uint LearnMoveConfirmTypePositions = 0x02229284;
    private const uint LearnMoveTypePositions = 0x02229294;
    private const uint MovesScreenTypePositions = 0x022292A4;
    private const uint MoveSummaryTypePositions = 0x022292B4;
    private const uint SummaryScreenTypePositions = 0x022292C4;
    private const uint MovesScreenMoveTypePositions = 0x02229324;
    private const uint LearnMoveMoveTypePositions = 0x022293DC;
    private const uint ScreenCursorPositions = 0x0222938C;
    private const int MaxSprites = 0x2B;
    private const int NarcPokemonIcons = 0x13;
    private const int NarcSummaryScreen = 0x27;
    private const int NarcHeldItems = 0x14;
    private const int NarcContestStats = 0x48;
    private const int PlttBufSubObj = 3;
    private const int VramTypeSub = 2;
    private const int ResPartyPokemon = 0xB007;
    private const int ResSummaryStatuses = 0xB008;
    private const int ResSummaryStatusesChar = 0xB00D;
    private const int ResTypes = 0xB009;
    private const int ResPokemonTypesChar = 0xB00E;
    private const int ResMoveTypesChar = 0xB010;
    private const int ResMoveToLearnTypeChar = 0xB014;
    private const int ResMoveClassChar = 0xB015;
    private const int ResHeldItems = 0xB00A;
    private const int ResHeldItemsChar = 0xB016;
    private const int ResContestStats = 0xB00B;
    private const int ResContestStatsChar = 0xB017;
    private const int ResCursorChar = 0xB018;
    private const int ResCursor = 0xB00C;
    private const int SpriteSummaryHeldItem = 6;
    private const int SpritePartyPokemon = 7;
    private const int SpriteSummaryStatus = 13;
    private const int SpritePokemonType1 = 19;
    private const int SpritePokemonType2 = 20;
    private const int SpriteMoveTypes = 21;
    private const int SpriteMoveToLearnType = 25;
    private const int SpriteMoveClass = 26;
    private const int SpriteContestCool = 27;
    private const int SpriteContestBeauty = 28;
    private const int SpriteContestCute = 29;
    private const int SpriteContestSmart = 30;
    private const int SpriteContestTough = 31;
    private const int SpriteHeldMail = 32;

    private static int[] ScreenCursorPositionCounts = { 7, 4, 4, 8, 5, 5, 7, 3, 7, 3 };

    private static SpriteSystem PartySpriteSystem(BattleParty battleParty)
    {
        return BattleSystems.BattleSystem_GetSpriteSystem(battleParty.Context!.BattleSys!)!;
    }

    private static ManagedSprite PartySprite(BattleParty battleParty, int index)
    {
        return battleParty.Sprites[index]!;
    }

    private static int Coord(uint table, int index)
    {
        return (int)BattleSubMenuData.U32((uint)(table + index * 4));
    }

    private static BattlePartyPokemon SelectedPartyPokemon(BattleParty battleParty)
    {
        return battleParty.PartyPokemon[battleParty.Context!.SelectedPartyIndex];
    }

    // OFFSET 0x02224500
    private static void BattlePartySprites_InitializeSprites(BattleParty battleParty)
    {
        InitializeSpriteManager(battleParty);
        LoadPartyPokemonSpriteData(battleParty);
        LoadSummaryStatusesSpriteData(battleParty);
        LoadTypesAndMovesSpriteData(battleParty);
        LoadHeldMailSpriteData(battleParty);
        LoadContestStatsSpriteData(battleParty);
        InitializeSprites(battleParty);
        InitializeCursor(battleParty);
        GXLayers.GXLayers_EngineBToggleLayers(0x10, 1);
    }

    // OFFSET 0x0222453C
    private static void InitializeSpriteManager(BattleParty battleParty)
    {
        int[] capacities = new int[6];
        for (int i = 0; i < capacities.Length; i++)
        {
            capacities[i] = (int)BattleSubMenuData.U32((uint)(SpriteManagerCapacities + i * 4));
        }
        SpriteSystem spriteSys = PartySpriteSystem(battleParty);
        battleParty.SpriteManager = SpriteSystem.SpriteManager_New(spriteSys);
        SpriteSystem.SpriteSystem_InitSprites(spriteSys, battleParty.SpriteManager, MaxSprites);
        SpriteSystem.SpriteSystem_InitManagerWithCapacities(spriteSys, battleParty.SpriteManager, capacities);
    }

    // OFFSET 0x02224588
    private static void LoadPartyPokemonSpriteData(BattleParty battleParty)
    {
        NARC narc = NARC.NARC_ctor(NarcPokemonIcons, battleParty.Context!.HeapID);
        SpriteSystem spriteSys = PartySpriteSystem(battleParty);
        SpriteManager spriteMan = battleParty.SpriteManager!;
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(battleParty.Palette!, PlttBufSubObj, spriteSys, spriteMan, narc, (int)PokemonIcon.PokeIconPalettesFileIndex(), 0, 3, VramTypeSub, ResPartyPokemon);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(spriteSys, spriteMan, narc, (int)PokemonIcon.PokeIcon32KCellsFileIndex(), 0, ResPartyPokemon);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(spriteSys, spriteMan, narc, (int)PokemonIcon.PokeIcon32KAnimationFileIndex(), 0, ResPartyPokemon);
        for (int i = 0; i < 6; i++)
        {
            if (battleParty.PartyPokemon[i].Species != 0)
            {
                SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSys, spriteMan, narc, (int)PokemonIcon.Pokemon_IconSpriteIndex(battleParty.PartyPokemon[i].Mon!), 0, VramTypeSub, ResPartyPokemon + i);
            }
            else
            {
                SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSys, spriteMan, narc, (int)PokemonIcon.PokeIconSpriteIndex(0, 0, 0), 0, VramTypeSub, ResPartyPokemon + i);
            }
        }
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x02224670
    private static void LoadSummaryStatusesSpriteData(BattleParty battleParty)
    {
        SpriteSystem spriteSys = PartySpriteSystem(battleParty);
        SpriteManager spriteMan = battleParty.SpriteManager!;
        NARC narc = NARC.NARC_ctor(NarcSummaryScreen, battleParty.Context!.HeapID);
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(battleParty.Palette!, PlttBufSubObj, spriteSys, spriteMan, narc, (int)PokemonSummaryScreenIcons.PokemonSummaryScreen_StatusIconPltt(), 0, 1, VramTypeSub, ResSummaryStatuses);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(spriteSys, spriteMan, narc, (int)PokemonSummaryScreenIcons.PokemonSummaryScreen_StatusIconCell(), 0, ResSummaryStatuses);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(spriteSys, spriteMan, narc, (int)PokemonSummaryScreenIcons.PokemonSummaryScreen_StatusIconAnim(), 0, ResSummaryStatuses);
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSys, spriteMan, narc, (int)PokemonSummaryScreenIcons.PokemonSummaryScreen_StatusIconChar(), 0, VramTypeSub, ResSummaryStatusesChar);
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x02224720
    private static void LoadTypesAndMovesSpriteData(BattleParty battleParty)
    {
        SpriteSystem spriteSys = PartySpriteSystem(battleParty);
        TypeIcon.TypeIcon_LoadPlttSrc(battleParty.Palette!, PlttBufSubObj, spriteSys, battleParty.SpriteManager!, VramTypeSub, ResTypes);
        TypeIcon.TypeIcon_LoadAnim(spriteSys, battleParty.SpriteManager!, ResTypes, ResTypes);
        for (int resourceID = ResPokemonTypesChar; resourceID <= ResMoveToLearnTypeChar; resourceID++)
        {
            TypeIcon.TypeIcon_LoadChar(spriteSys, battleParty.SpriteManager!, VramTypeSub, 0, resourceID);
        }
        CategoryIcon.CategoryIcon_LoadChar(spriteSys, battleParty.SpriteManager!, VramTypeSub, 0, ResMoveClassChar);
    }

    // OFFSET 0x02224798
    private static void LoadHeldMailSpriteData(BattleParty battleParty)
    {
        NARC narc = NARC.NARC_ctor(NarcHeldItems, battleParty.Context!.HeapID);
        SpriteSystem spriteSys = PartySpriteSystem(battleParty);
        SpriteManager spriteMan = battleParty.SpriteManager!;
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(battleParty.Palette!, PlttBufSubObj, spriteSys, spriteMan, narc, (int)PartyMenuIcons.PartyMenu_HeldItemIconPltt(), 0, 1, VramTypeSub, ResHeldItems);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(spriteSys, spriteMan, narc, (int)PartyMenuIcons.PartyMenu_HeldItemIconCell(), 0, ResHeldItems);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(spriteSys, spriteMan, narc, (int)PartyMenuIcons.PartyMenu_HeldItemIconAnim(), 0, ResHeldItems);
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSys, spriteMan, narc, (int)PartyMenuIcons.PartyMenu_HeldItemIconChar(), 0, VramTypeSub, ResHeldItemsChar);
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x02224848
    private static void LoadContestStatsSpriteData(BattleParty battleParty)
    {
        SpriteSystem spriteSys = PartySpriteSystem(battleParty);
        SpriteManager spriteMan = battleParty.SpriteManager!;
        SpriteSystem.SpriteSystem_LoadPaletteBuffer(battleParty.Palette!, PlttBufSubObj, spriteSys, spriteMan, NarcContestStats, 0x1B, 0, 1, VramTypeSub, ResContestStats);
        SpriteSystem.SpriteSystem_LoadCellResObj(spriteSys, spriteMan, NarcContestStats, 0x19, 0, ResContestStats);
        SpriteSystem.SpriteSystem_LoadAnimResObj(spriteSys, spriteMan, NarcContestStats, 0x18, 0, ResContestStats);
        SpriteSystem.SpriteSystem_LoadCharResObj(spriteSys, spriteMan, NarcContestStats, 0x1A, 0, VramTypeSub, ResContestStatsChar);
    }

    // OFFSET 0x022248D8
    private static ManagedSprite? CreateSprite(BattleParty battleParty, int spriteIndex)
    {
        SpriteSystem spriteSys = PartySpriteSystem(battleParty);
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
        return SpriteSystem.SpriteSystem_NewSprite(spriteSys, battleParty.SpriteManager!, template);
    }

    // OFFSET 0x02224948
    private static void InitializeSprites(BattleParty battleParty)
    {
        for (int i = 0; i < BattleParty.NumSprites; i++)
        {
            battleParty.Sprites[i] = CreateSprite(battleParty, i);
        }
        InitializePartyPokemonSprites(battleParty);
    }

    // OFFSET 0x02224970
    private static void BattlePartySprites_CleanupSprites(BattleParty battleParty)
    {
        SpriteSystem spriteSys = PartySpriteSystem(battleParty);
        for (int i = 0; i < BattleParty.NumSprites; i++)
        {
            SpriteSystem.Sprite_DeleteAndFreeResources(PartySprite(battleParty, i));
        }
        CleanupCursor(battleParty);
        SpriteSystem.SpriteSystem_FreeResourcesAndManager(spriteSys, battleParty.SpriteManager!);
    }

    // OFFSET 0x022249AC
    private static void DrawSprite(ManagedSprite sprite, int x, int y)
    {
        SpriteSystem.ManagedSprite_SetDrawFlag(sprite, 1);
        SpriteSystem.ManagedSprite_SetPositionXY(sprite, (short)x, (short)y);
    }

    // OFFSET 0x022249CC
    private static void InitializePartyPokemonSprites(BattleParty battleParty)
    {
        for (int i = 0; i < 6; i++)
        {
            BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[i];
            if (partyPokemon.Species != 0)
            {
                SpriteSystem.ManagedSprite_SetExplicitPalette(PartySprite(battleParty, SpritePartyPokemon + i), PokemonIcon.PokeIconPaletteIndex(partyPokemon.Species, partyPokemon.Form, partyPokemon.IsEgg));
            }
        }
    }

    // OFFSET 0x02224A0C
    private static void InitializeTypeIcon(BattleParty battleParty, ManagedSprite sprite, int resourceID, byte type)
    {
        SpriteSystem spriteSys = PartySpriteSystem(battleParty);
        int narcID = TypeIcon.TypeIcon_GetNARC();
        SpriteSystem.SpriteSystem_ReplaceCharResObj(spriteSys, battleParty.SpriteManager!, narcID, (int)TypeIcon.TypeIcon_GetChar(type), 1, resourceID);
        SpriteSystem.ManagedSprite_SetExplicitPalette(sprite, (uint)(TypeIcon.TypeIcon_GetPlttIndex(type) + 4));
    }

    // OFFSET 0x02224A5C
    private static void InitializeMoveClassIcon(BattleParty battleParty, ManagedSprite sprite, int moveClass)
    {
        SpriteSystem spriteSys = PartySpriteSystem(battleParty);
        int narcID = CategoryIcon.CategoryIcon_GetNARC();
        SpriteSystem.SpriteSystem_ReplaceCharResObj(spriteSys, battleParty.SpriteManager!, narcID, (int)CategoryIcon.CategoryIcon_GetChar(moveClass), 1, ResMoveClassChar);
        SpriteSystem.ManagedSprite_SetExplicitPalette(sprite, (uint)(CategoryIcon.CategoryIcon_GetPlttIndex(moveClass) + 4));
    }

    // OFFSET 0x02224AB0
    private static void DrawSummaryStatusIcon(int summaryStatus, ManagedSprite sprite, int x, int y)
    {
        if (summaryStatus == Pokemon.SummaryStatusNone)
        {
            return;
        }
        SpriteSystem.ManagedSprite_SetAnim(sprite, (uint)summaryStatus);
        DrawSprite(sprite, x, y);
    }

    // OFFSET 0x02224AD4
    private static void DrawSelectedPokemonTypeIcons(BattleParty battleParty, BattlePartyPokemon pokemon, uint coordinates)
    {
        InitializeTypeIcon(battleParty, PartySprite(battleParty, SpritePokemonType1), ResPokemonTypesChar, pokemon.Type1);
        DrawSprite(PartySprite(battleParty, SpritePokemonType1), Coord(coordinates, 0), Coord(coordinates, 1));
        if (pokemon.Type1 != pokemon.Type2)
        {
            InitializeTypeIcon(battleParty, PartySprite(battleParty, SpritePokemonType2), ResPokemonTypesChar + 1, pokemon.Type2);
            DrawSprite(PartySprite(battleParty, SpritePokemonType2), Coord(coordinates, 2), Coord(coordinates, 3));
        }
    }

    // OFFSET 0x02224B28
    private static void DrawHeldItem(ushort item, ManagedSprite sprite, int x, int y)
    {
        if (item == 0)
        {
            return;
        }
        if (Item.Item_IsMail(item) == 1)
        {
            SpriteSystem.ManagedSprite_SetAnim(sprite, 1);
        }
        else
        {
            SpriteSystem.ManagedSprite_SetAnim(sprite, 0);
        }
        DrawSprite(sprite, x, y);
    }

    // OFFSET 0x02224B5C
    private static void DrawHeldMail(byte mail, ManagedSprite sprite, int x, int y)
    {
        if (mail == 0)
        {
            return;
        }
        SpriteSystem.ManagedSprite_SetAnim(sprite, 2);
        DrawSprite(sprite, x, y);
    }

    // OFFSET 0x02224B7C
    private static void BattlePartySprites_SetupScreen(BattleParty battleParty, int screen)
    {
        for (int i = 0; i < BattleParty.NumSprites; i++)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(PartySprite(battleParty, i), 0);
        }
        switch (screen)
        {
            case ScreenPokemonParty:
                DrawPokemonPartyScreenSprites(battleParty);
                break;
            case ScreenSelectPokemon:
                DrawSelectPokemonScreenSprites(battleParty);
                break;
            case ScreenPokemonSummary:
                DrawPokemonSummaryScreenSprites(battleParty);
                break;
            case ScreenPokemonMoves:
                DrawPokemonMovesScreenSprites(battleParty);
                break;
            case ScreenMoveSummary:
                DrawMoveSummaryScreenSprites(battleParty);
                break;
            case ScreenRestoreMovePP:
                DrawRestoreMovePPScreenSprites(battleParty);
                break;
            case ScreenLearnMove:
                DrawLearnMoveScreenSprites(battleParty);
                break;
            case ScreenConfirmLearnMove:
                DrawConfirmLearnMoveScreenSprites(battleParty);
                break;
            case ScreenLearnMoveContestStats:
                DrawLearnMoveContestStatsScreenSprites(battleParty);
                break;
            case ScreenConfirmLearnMoveContestStats:
                DrawConfirmLearnMoveContestStatsScreenSprites(battleParty);
                break;
        }
    }

    // OFFSET 0x02224C14
    private static void DrawPokemonPartyScreenSprites(BattleParty battleParty)
    {
        for (int i = 0; i < 6; i++)
        {
            BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[i];
            if (partyPokemon.Species == 0)
            {
                continue;
            }
            int x = Coord(PartyScreenPokemonPositions, i * 2);
            int y = Coord(PartyScreenPokemonPositions, i * 2 + 1);
            DrawSprite(PartySprite(battleParty, SpritePartyPokemon + i), x, y);
            DrawSummaryStatusIcon(partyPokemon.SummaryStatus, PartySprite(battleParty, SpriteSummaryStatus + i), Coord(PartyScreenStatusPositions, i * 2), Coord(PartyScreenStatusPositions, i * 2 + 1));
            DrawHeldItem(partyPokemon.HeldItem, PartySprite(battleParty, i), x + 8, y + 8);
            DrawHeldMail(partyPokemon.Mail, PartySprite(battleParty, SpriteHeldMail + i), x + 0x10, y + 8);
        }
    }

    // OFFSET 0x02224CB0
    private static void DrawSelectPokemonScreenSprites(BattleParty battleParty)
    {
        int slot = battleParty.Context!.SelectedPartyIndex;
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[slot];
        DrawSprite(PartySprite(battleParty, SpritePartyPokemon + slot), 0x80, 0x48);
        DrawHeldItem(partyPokemon.HeldItem, PartySprite(battleParty, slot), 0x88, 0x50);
        DrawHeldMail(partyPokemon.Mail, PartySprite(battleParty, SpriteHeldMail + slot), 0x90, 0x50);
    }

    private static void DrawSelectedPokemonHeader(BattleParty battleParty, bool drawStatus, uint typeCoordinates)
    {
        int slot = battleParty.Context!.SelectedPartyIndex;
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[slot];
        DrawSprite(PartySprite(battleParty, SpritePartyPokemon + slot), 0x18, 0xC);
        if (drawStatus)
        {
            DrawSummaryStatusIcon(partyPokemon.SummaryStatus, PartySprite(battleParty, SpriteSummaryStatus + slot), 0xC6, 0x14);
        }
        if (typeCoordinates != 0)
        {
            DrawSelectedPokemonTypeIcons(battleParty, partyPokemon, typeCoordinates);
        }
    }

    private static void DrawSelectedPokemonHeldItem(BattleParty battleParty)
    {
        int slot = battleParty.Context!.SelectedPartyIndex;
        BattlePartyPokemon partyPokemon = battleParty.PartyPokemon[slot];
        DrawHeldItem(partyPokemon.HeldItem, PartySprite(battleParty, slot), 0x20, 0x14);
        DrawHeldMail(partyPokemon.Mail, PartySprite(battleParty, SpriteHeldMail + slot), 0x28, 0x14);
    }

    // OFFSET 0x02224D08
    private static void DrawPokemonSummaryScreenSprites(BattleParty battleParty)
    {
        DrawSelectedPokemonHeader(battleParty, true, SummaryScreenTypePositions);
        DrawSelectedPokemonHeldItem(battleParty);
        DrawHeldItem(SelectedPartyPokemon(battleParty).HeldItem, PartySprite(battleParty, SpriteSummaryHeldItem), 0x14, 0x84);
    }

    // OFFSET 0x02224DA0
    private static void DrawPokemonMovesScreenSprites(BattleParty battleParty)
    {
        DrawSelectedPokemonHeader(battleParty, true, MovesScreenTypePositions);
        DrawSelectedPokemonHeldItem(battleParty);
        BattlePartyPokemon partyPokemon = SelectedPartyPokemon(battleParty);
        for (int i = 0; i < 4; i++)
        {
            BattlePartyPokemonMove move = partyPokemon.Moves[i];
            if (move.Move == 0)
            {
                continue;
            }
            InitializeTypeIcon(battleParty, PartySprite(battleParty, SpriteMoveTypes + i), ResMoveTypesChar + i, move.Type);
            DrawSprite(PartySprite(battleParty, SpriteMoveTypes + i), Coord(MovesScreenMoveTypePositions, i * 2), Coord(MovesScreenMoveTypePositions, i * 2 + 1));
        }
    }

    // OFFSET 0x02224E78
    private static void DrawMoveSummaryScreenSprites(BattleParty battleParty)
    {
        BattlePartyContext context = battleParty.Context!;
        DrawSelectedPokemonHeader(battleParty, true, MoveSummaryTypePositions);
        DrawSprite(PartySprite(battleParty, SpriteMoveTypes + context.SelectedMoveSlot), 0x88, 0x30);
        DrawSelectedPokemonHeldItem(battleParty);
        InitializeMoveClassIcon(battleParty, PartySprite(battleParty, SpriteMoveClass), SelectedPartyPokemon(battleParty).Moves[context.SelectedMoveSlot].Class);
        DrawSprite(PartySprite(battleParty, SpriteMoveClass), 0x18, 0x58);
    }

    // OFFSET 0x02224F3C
    private static void DrawLearnMoveScreenSprites(BattleParty battleParty)
    {
        DrawSelectedPokemonHeader(battleParty, false, LearnMoveTypePositions);
        DrawSelectedPokemonHeldItem(battleParty);
        BattlePartySprites_DrawMovesSprites(battleParty);
    }

    // OFFSET 0x02224FA8
    private static void DrawConfirmLearnMoveScreenSprites(BattleParty battleParty)
    {
        BattlePartyContext context = battleParty.Context!;
        DrawSelectedPokemonHeader(battleParty, false, LearnMoveConfirmTypePositions);
        DrawSprite(PartySprite(battleParty, SpriteMoveTypes + context.SelectedMoveSlot), 0x88, 0x30);
        DrawSelectedPokemonHeldItem(battleParty);
        if (context.SelectedMoveSlot < 4)
        {
            InitializeMoveClassIcon(battleParty, PartySprite(battleParty, SpriteMoveClass), SelectedPartyPokemon(battleParty).Moves[context.SelectedMoveSlot].Class);
        }
        else
        {
            InitializeMoveClassIcon(battleParty, PartySprite(battleParty, SpriteMoveClass), (int)MoveTable.MoveTable_LoadParam(context.MoveToLearn, 1));
        }
        DrawSprite(PartySprite(battleParty, SpriteMoveClass), 0x18, 0x58);
    }

    // OFFSET 0x0222506C
    private static void DrawRestoreMovePPScreenSprites(BattleParty battleParty)
    {
        DrawSelectedPokemonHeader(battleParty, false, LearnMoveTypePositions);
        DrawSelectedPokemonHeldItem(battleParty);
        BattlePartySprites_DrawMovesSprites(battleParty);
    }

    // OFFSET 0x022250D8
    private static void DrawConfirmLearnMoveContestStatsScreenSprites(BattleParty battleParty)
    {
        DrawSelectedPokemonHeader(battleParty, false, 0);
        DrawSprite(PartySprite(battleParty, SpriteMoveTypes + battleParty.Context!.SelectedMoveSlot), 0x88, 0x48);
        DrawSelectedPokemonHeldItem(battleParty);
        DrawContestStatSprites(battleParty);
    }

    // OFFSET 0x02225150
    private static void DrawLearnMoveContestStatsScreenSprites(BattleParty battleParty)
    {
        DrawSelectedPokemonHeader(battleParty, false, 0);
        DrawSelectedPokemonHeldItem(battleParty);
        BattlePartySprites_DrawMovesSprites(battleParty);
        DrawContestStatSprites(battleParty);
    }

    // OFFSET 0x022251B4
    private static void DrawStandardMovesSprites(BattleParty battleParty)
    {
        BattlePartyPokemon partyPokemon = SelectedPartyPokemon(battleParty);
        for (ushort i = 0; i < 4; i++)
        {
            BattlePartyPokemonMove move = partyPokemon.Moves[i];
            if (move.Move == 0)
            {
                continue;
            }
            InitializeTypeIcon(battleParty, PartySprite(battleParty, SpriteMoveTypes + i), ResMoveTypesChar + i, move.Type);
            DrawSprite(PartySprite(battleParty, SpriteMoveTypes + i), Coord(LearnMoveMoveTypePositions, i * 2), Coord(LearnMoveMoveTypePositions, i * 2 + 1));
        }
        ushort moveToLearn = battleParty.Context!.MoveToLearn;
        if (moveToLearn != 0)
        {
            byte type = (byte)MoveTable.MoveTable_LoadParam(moveToLearn, 3);
            InitializeTypeIcon(battleParty, PartySprite(battleParty, SpriteMoveToLearnType), ResMoveToLearnTypeChar, type);
            DrawSprite(PartySprite(battleParty, SpriteMoveToLearnType), 0x58, 0xB0);
        }
    }

    // OFFSET 0x02225248
    private static void DrawContestStatMovesSprites(BattleParty battleParty)
    {
        BattlePartyPokemon partyPokemon = SelectedPartyPokemon(battleParty);
        for (ushort i = 0; i < 4; i++)
        {
            BattlePartyPokemonMove move = partyPokemon.Moves[i];
            if (move.Move == 0)
            {
                continue;
            }
            byte type = (byte)(MoveTable.MoveTable_LoadParam(move.Move, 0xB) + 0x12);
            InitializeTypeIcon(battleParty, PartySprite(battleParty, SpriteMoveTypes + i), ResMoveTypesChar + i, type);
            DrawSprite(PartySprite(battleParty, SpriteMoveTypes + i), Coord(LearnMoveMoveTypePositions, i * 2), Coord(LearnMoveMoveTypePositions, i * 2 + 1));
        }
        ushort moveToLearn = battleParty.Context!.MoveToLearn;
        if (moveToLearn != 0)
        {
            byte type = (byte)(MoveTable.MoveTable_LoadParam(moveToLearn, 0xB) + 0x12);
            InitializeTypeIcon(battleParty, PartySprite(battleParty, SpriteMoveToLearnType), ResMoveToLearnTypeChar, type);
            DrawSprite(PartySprite(battleParty, SpriteMoveToLearnType), 0x58, 0xB0);
        }
    }

    // OFFSET 0x022252E8
    private static void BattlePartySprites_DrawMovesSprites(BattleParty battleParty)
    {
        if (battleParty.DisplayingContestStats == 0)
        {
            DrawStandardMovesSprites(battleParty);
        }
        else
        {
            DrawContestStatMovesSprites(battleParty);
        }
    }

    // OFFSET 0x02225304
    private static void SetPartyPokemonSpriteAnimation(ManagedSprite sprite, byte animationID)
    {
        if (animationID == SpriteSystem.ManagedSprite_GetActiveAnim(sprite))
        {
            return;
        }
        SpriteSystem.ManagedSprite_SetAnimationFrame(sprite, 0);
        SpriteSystem.ManagedSprite_SetAnim(sprite, animationID);
    }

    // OFFSET 0x02225324
    private static byte GetPartyPokemonSpriteAnimation(BattlePartyPokemon partyPokemon)
    {
        if (partyPokemon.CurHP == 0)
        {
            return 0;
        }
        if (partyPokemon.SummaryStatus != Pokemon.SummaryStatusNone && partyPokemon.SummaryStatus != 6)
        {
            return 5;
        }
        switch (HealthBar.HealthBar_Color(partyPokemon.CurHP, partyPokemon.MaxHP, 0x30))
        {
            case 4:
                return 1;
            case 3:
                return 2;
            case 2:
                return 3;
            case 1:
                return 4;
        }
        return 0;
    }

    // OFFSET 0x0222537C
    private static void BattlePartySprites_TickPartyPokemonSpriteAnimations(BattleParty battleParty)
    {
        for (ushort i = 0; i < Party.Party_GetCurrentCount(battleParty.Context!.Party!); i++)
        {
            if (battleParty.PartyPokemon[i].Species == 0)
            {
                continue;
            }
            SetPartyPokemonSpriteAnimation(PartySprite(battleParty, SpritePartyPokemon + i), GetPartyPokemonSpriteAnimation(battleParty.PartyPokemon[i]));
            SpriteSystem.ManagedSprite_TickNFrames(PartySprite(battleParty, SpritePartyPokemon + i), 0x1000);
        }
    }

    // OFFSET 0x022253DC
    private static short CalculateContestStatSpriteCoord(uint statValue, short endCoord, short startCoord)
    {
        if (startCoord > endCoord)
        {
            uint scaled = (uint)(((statValue + 0x2C) * (uint)(startCoord - endCoord)) << 16);
            return (short)(startCoord - (short)((scaled / 300) >> 16));
        }
        uint delta = (uint)(((statValue + 0x2C) * (uint)(endCoord - startCoord)) << 16);
        return (short)(startCoord + (short)((delta / 300) >> 16));
    }

    // OFFSET 0x02225420
    private static void DrawContestStatSprites(BattleParty battleParty)
    {
        BattlePartyPokemon partyPokemon = SelectedPartyPokemon(battleParty);
        SetPartyPokemonSpriteAnimation(PartySprite(battleParty, SpriteContestCool), 0);
        SetPartyPokemonSpriteAnimation(PartySprite(battleParty, SpriteContestBeauty), 1);
        SetPartyPokemonSpriteAnimation(PartySprite(battleParty, SpriteContestCute), 3);
        SetPartyPokemonSpriteAnimation(PartySprite(battleParty, SpriteContestSmart), 4);
        SetPartyPokemonSpriteAnimation(PartySprite(battleParty, SpriteContestTough), 2);
        DrawSprite(PartySprite(battleParty, SpriteContestCool), CalculateContestStatSpriteCoord(partyPokemon.Cool, 0x90, 0x90), CalculateContestStatSpriteCoord(partyPokemon.Cool, 2, 0x18));
        DrawSprite(PartySprite(battleParty, SpriteContestBeauty), CalculateContestStatSpriteCoord(partyPokemon.Beauty, 0xA4, 0x90), CalculateContestStatSpriteCoord(partyPokemon.Beauty, 0x10, 0x18));
        DrawSprite(PartySprite(battleParty, SpriteContestCute), CalculateContestStatSpriteCoord(partyPokemon.Cute, 0x9C, 0x90), CalculateContestStatSpriteCoord(partyPokemon.Cute, 0x29, 0x18));
        DrawSprite(PartySprite(battleParty, SpriteContestSmart), CalculateContestStatSpriteCoord(partyPokemon.Smart, 0x83, 0x8F), CalculateContestStatSpriteCoord(partyPokemon.Smart, 0x29, 0x18));
        DrawSprite(PartySprite(battleParty, SpriteContestTough), CalculateContestStatSpriteCoord(partyPokemon.Tough, 0x7B, 0x8F), CalculateContestStatSpriteCoord(partyPokemon.Tough, 0x10, 0x18));
    }

    // OFFSET 0x0222554C
    private static void InitializeCursor(BattleParty battleParty)
    {
        SpriteSystem spriteSys = PartySpriteSystem(battleParty);
        BattleCursors.BattleCursor_LoadResources(spriteSys, battleParty.SpriteManager!, battleParty.Palette!, battleParty.Context!.HeapID, ResCursorChar, ResCursor, ResCursor, ResCursor);
        BattleCursor cursor = BattleCursors.BattleCursor_New(spriteSys, battleParty.SpriteManager!, battleParty.Context.HeapID, ResCursorChar, ResCursor, ResCursor, ResCursor, 0, 1);
        BattleSubMenuCursor.SetBattleSubMenuCursorSprites(battleParty.Cursor!, cursor);
    }

    // OFFSET 0x022255B8
    private static void CleanupCursor(BattleParty battleParty)
    {
        BattleCursors.BattleCursor_Free(BattleSubMenuCursor.GetBattleSubMenuCursorSprites(battleParty.Cursor!)!);
        BattleCursors.BattleCursor_FreeResources(battleParty.SpriteManager!, ResCursorChar, ResCursor, ResCursor, ResCursor);
    }

    // OFFSET 0x022255EC
    private static void SetupLearnMoveScreenCursor(BattleParty battleParty)
    {
        if (battleParty.HasVisitedContestHall == 0)
        {
            BattleSubMenuCursor.SetBattleSubMenuCursorEnabledPositionsMask(battleParty.Cursor!, 0x5F);
        }
        else
        {
            BattleSubMenuCursor.SetBattleSubMenuCursorEnabledPositionsMask(battleParty.Cursor!, 0x7F);
        }
    }

    // OFFSET 0x02225614
    private static void SetupConfirmLearnMoveScreenCursor(BattleParty battleParty)
    {
        if (battleParty.HasVisitedContestHall == 0)
        {
            BattleSubMenuCursor.SetBattleSubMenuCursorEnabledPositionsMask(battleParty.Cursor!, 5);
        }
        else
        {
            BattleSubMenuCursor.SetBattleSubMenuCursorEnabledPositionsMask(battleParty.Cursor!, 7);
        }
    }

    // OFFSET 0x0222563C
    private static void BattlePartySprites_SetupCursor(BattleParty battleParty, int screen)
    {
        uint positions = BattleSubMenuData.U32((uint)(ScreenCursorPositions + screen * 4));
        BattleSubMenuCursor.SetBattleSubMenuCursorPositions(battleParty.Cursor!, BattleSubMenuData.CursorPositions(positions, ScreenCursorPositionCounts[screen]));
        switch (screen)
        {
            case ScreenPokemonParty:
                BattleSubMenuCursor.SetBattleSubMenuCursorCurrentPosition(battleParty.Cursor!, battleParty.Context!.SelectedPartyIndex);
                battleParty.SelectPokemonPreviousScreenButton = 0;
                battleParty.Context.SelectedMoveSlot = 0;
                break;
            case ScreenSelectPokemon:
                BattleSubMenuCursor.SetBattleSubMenuCursorCurrentPosition(battleParty.Cursor!, battleParty.SelectPokemonPreviousScreenButton);
                battleParty.Context!.SelectedMoveSlot = 0;
                break;
            case ScreenPokemonMoves:
            case ScreenMoveSummary:
                BattleSubMenuCursor.SetBattleSubMenuCursorCurrentPosition(battleParty.Cursor!, battleParty.Context!.SelectedMoveSlot);
                break;
            case ScreenLearnMove:
            case ScreenLearnMoveContestStats:
                SetupLearnMoveScreenCursor(battleParty);
                BattleSubMenuCursor.SetBattleSubMenuCursorCurrentPosition(battleParty.Cursor!, battleParty.LearnMovePreviousScreenButton);
                break;
            case ScreenConfirmLearnMove:
            case ScreenConfirmLearnMoveContestStats:
                SetupConfirmLearnMoveScreenCursor(battleParty);
                BattleSubMenuCursor.SetBattleSubMenuCursorCurrentPosition(battleParty.Cursor!, battleParty.ConfirmLearnMovePreviousScreenButton);
                break;
        }
    }

    // OFFSET 0x022256E8
    private static void BattlePartySprites_DisableCursor(BattleParty battleParty)
    {
        BattleSubMenuCursor.SetBattleSubMenuCursorVisibility(battleParty.Cursor!, 0);
        BattleSubMenuCursor.ResetBattleSubMenuCursorCurrentPosition(battleParty.Cursor!);
        BattleCursors.BattleCursor_Hide(BattleSubMenuCursor.GetBattleSubMenuCursorSprites(battleParty.Cursor!)!);
    }
}

public static class PokemonSummaryScreenIcons
{
    // OFFSET 0x0208E9E0
    public static uint PokemonSummaryScreen_StatusIconChar()
    {
        return 0x40;
    }

    // OFFSET 0x0208E9E4
    public static uint PokemonSummaryScreen_StatusIconPltt()
    {
        return 0x41;
    }

    // OFFSET 0x0208E9E8
    public static uint PokemonSummaryScreen_StatusIconCell()
    {
        return 0x3F;
    }

    // OFFSET 0x0208E9EC
    public static uint PokemonSummaryScreen_StatusIconAnim()
    {
        return 0x3E;
    }
}

public static class PartyMenuIcons
{
    // OFFSET 0x02081930
    public static uint PartyMenu_HeldItemIconChar()
    {
        return 0x14;
    }

    // OFFSET 0x02081934
    public static uint PartyMenu_HeldItemIconPltt()
    {
        return 0x15;
    }

    // OFFSET 0x02081938
    public static uint PartyMenu_HeldItemIconCell()
    {
        return 0x13;
    }

    // OFFSET 0x0208193C
    public static uint PartyMenu_HeldItemIconAnim()
    {
        return 0x12;
    }
}
