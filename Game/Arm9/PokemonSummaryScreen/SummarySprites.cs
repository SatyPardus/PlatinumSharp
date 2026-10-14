namespace PlatinumSharp.Game;

public static partial class PokemonSummaryScreenApp
{
    private const uint RenderOamTemplateAddress = 0x020F4160;
    private const uint CharTransferTemplateAddress = 0x020F4130;
    private const uint ResourcePathsAddress = 0x020F4144;
    private const int ResourcePathCount = 7;
    private const uint SpriteTemplatesAddress = 0x020F41A8;
    private const uint BallPaletteTable = 0x020F411C;
    private const uint ConditionFlashPosCool = 0x020F4180;
    private const uint ConditionFlashPosBeauty = 0x020F4188;
    private const uint ConditionFlashPosCute = 0x020F4190;
    private const uint ConditionFlashPosSmart = 0x020F4198;
    private const uint ConditionFlashPosTough = 0x020F41A0;
    private const int PokemonIconArchive = 0x13;
    private const int SheenUnfed = 0xFF;
    private const int SheenSpriteMax = 12;
    private const int NumMarkings = 6;
    private const int NumContestStats = 5;
    private const int StateFeedPoffinFailed = 0x11;
    private const int StateFeedPoffin = 0x12;
    private const int AnimStep = 0x1000;
    private const int MoveParamClass = 1;
    private const int MoveParamType = 3;
    private const int MoveParamContestType = 0xB;
    private const int ContestTypeIconOffset = 0x12;
    private const int FormParamFlip = 0x1C;
    private const int ResourceCaughtBall = 0;
    private const int ResourceMonType1 = 3;
    private const int ResourceMonType2 = 4;
    private const int ResourceMoveType1 = 5;
    private const int ResourceLearnMoveType = 9;
    private const int ResourceMoveCategory = 0xA;
    private const int ResourceMonIcon = 0xB;
    private const int ResourceRibbon1 = 0x1A;
    private const int PaletteResourceMonIcon = 5;
    private const int PaletteResourceCaughtBall = 6;
    private const int PaletteResourceRibbons = 5;
    private const int RibbonPaletteMember = 0x88;
    private const int SpriteMoveTypeIcon5 = 17;

    // OFFSET 0x0208E904
    public static byte PokemonSummaryScreen_RibbonIDAt(PokemonSummaryScreen summaryScreen, byte col)
    {
        byte target = (byte)(col + summaryScreen.RibbonRow * RibbonsPerRow);
        byte count = 0;
        for (ushort i = 0; i < NumRibbons; i++)
        {
            if ((summaryScreen.MonData.Ribbons[i >> 5] & (1u << (i & 0x1F))) == 0)
            {
                continue;
            }
            if (target == count)
            {
                return (byte)i;
            }
            count++;
        }
        return 0;
    }

    // OFFSET 0x0208E958
    private static int TryFeedPoffin(PokemonSummaryScreen summaryScreen)
    {
        if (summaryScreen.MonData.Sheen == SheenUnfed)
        {
            Font.Font_LoadScreenIndicatorsPalette(0, 0x1C0, HeapIDSummary);
            RenderWindow.LoadMessageBoxGraphics(summaryScreen.BgConfig!, 1, 0x3E2, 0xD, (byte)Options.Options_Frame(summaryScreen.Data!.Options!), HeapIDSummary);
            PokemonSummaryScreen_PrintPoffinFeedMsg(summaryScreen, 0xFF);
            summaryScreen.Data.ReturnMode = 1;
            return StateFeedPoffinFailed;
        }
        summaryScreen.Data!.ReturnMode = 0;
        return StateFeedPoffin;
    }

    // OFFSET 0x0208E9C0
    public static void PokemonSummaryScreen_SetPlayerProfile(PokemonSummary summary, TrainerInfo trainerInfo)
    {
        summary.OTName = TrainerInfo.TrainerInfo_Name(trainerInfo);
        summary.OTID = TrainerInfo.TrainerInfo_ID(trainerInfo);
        summary.OTGender = (byte)TrainerInfo.TrainerInfo_Gender(trainerInfo);
    }

    // OFFSET 0x0208EA44
    private static void PokemonSummaryScreen_InitSpriteResources(PokemonSummaryScreen summaryScreen)
    {
        GXLayers.GXLayers_EngineAToggleLayers(0x10, 1);
        GXLayers.GXLayers_EngineBToggleLayers(0x10, 1);
        VramTransfer.VramTransfer_New(0x20, HeapIDSummary);
        summaryScreen.SpriteSys = SpriteSystem.SpriteSystem_Alloc(HeapIDSummary);
        summaryScreen.SpriteMan = SpriteSystem.SpriteManager_New(summaryScreen.SpriteSys);
        SpriteSystem.SpriteSystem_Init(summaryScreen.SpriteSys, RomStructs.Arm9RenderOamTemplate(RenderOamTemplateAddress), RomStructs.Arm9CharTransferTemplate(CharTransferTemplateAddress), 0x20);
        SpriteSystem.SpriteSystem_InitSprites(summaryScreen.SpriteSys, summaryScreen.SpriteMan, PokemonSummaryScreen.SpriteMax);
        SpriteSystem.SpriteSystem_LoadResourceDataFromFilepaths(summaryScreen.SpriteSys, summaryScreen.SpriteMan, RomStructs.Arm9StringTable(ResourcePathsAddress, ResourcePathCount));
    }

    // OFFSET 0x0208EAF4
    private static void PokemonSummaryScreen_FreeSpriteResources(PokemonSummaryScreen summaryScreen)
    {
        SpriteSystem.SpriteSystem_DestroySpriteManager(summaryScreen.SpriteSys!, summaryScreen.SpriteMan!);
        SpriteSystem.SpriteSystem_Free(summaryScreen.SpriteSys!);
    }

    // OFFSET 0x0208EB14
    private static void PokemonSummaryScreen_UpdateArrowAndTapAnims(PokemonSummaryScreen summaryScreen)
    {
        Sprite.Sprite_UpdateAnim(summaryScreen.Sprites[SpritePageArrowLeft]!, AnimStep);
        Sprite.Sprite_UpdateAnim(summaryScreen.Sprites[SpritePageArrowRight]!, AnimStep);
        Sprite.Sprite_UpdateAnim(summaryScreen.Sprites[SpriteButtonTapCircle]!, AnimStep);
        Sprite.Sprite_UpdateAnim(summaryScreen.Sprites[SpriteRibbonArrowUp]!, AnimStep);
        Sprite.Sprite_UpdateAnim(summaryScreen.Sprites[SpriteRibbonArrowDown]!, AnimStep);
    }

    // OFFSET 0x0208EB64
    private static void PokemonSummaryScreen_SetDefaultSpriteStates(PokemonSummaryScreen summaryScreen)
    {
        for (ushort i = 0; i < PokemonSummaryScreen.SpriteMax; i++)
        {
            SpriteTemplateFromResourceHeader template = RomStructs.Arm9SpriteTemplate((uint)(SpriteTemplatesAddress + i * RomStructs.SpriteTemplateSize));
            summaryScreen.Sprites[i] = SpriteSystem.SpriteSystem_NewSpriteFromResourceHeader(summaryScreen.SpriteSys!, summaryScreen.SpriteMan!, template);
        }
        SetMonAndTypeIcons(summaryScreen);
        SetSpriteDraw(summaryScreen, SpriteMoveSelector1, 0);
        SetSpriteDraw(summaryScreen, SpriteMoveSelector2, 0);
        SetSpriteDraw(summaryScreen, SpriteButtonTapCircle, 0);
        for (int i = 0; i < NumContestStats; i++)
        {
            SetSpriteDraw(summaryScreen, SpriteConditionArrowCool + i, 0);
        }
        SetSpriteDraw(summaryScreen, SpriteAButton, 0);
        for (int i = 0; i < NumContestStats; i++)
        {
            SetSpriteDraw(summaryScreen, SpriteContestStatDotCool + i, 0);
        }
        for (int i = 0; i < NumContestStats; i++)
        {
            SetSpriteDraw(summaryScreen, SpriteConditionFlashCool + i, 0);
        }
        SetSpriteDraw(summaryScreen, SpritePokerusIcon, 0);
    }

    // OFFSET 0x0208ECB8
    private static byte CalcPageTabsBaseXPos(PokemonSummaryScreen summaryScreen)
    {
        byte count = 0;
        for (byte i = 0; i < PageMax; i++)
        {
            if (PokemonSummaryScreen_PageIsVisble(summaryScreen, i) != 0)
            {
                count++;
            }
        }
        return (byte)(0xBC - ((count - 1) * 16 + 0x18) / 2);
    }

    // OFFSET 0x0208ECF4
    public static void PokemonSummaryScreen_UpdatePageTabSprites(PokemonSummaryScreen summaryScreen)
    {
        byte baseX = CalcPageTabsBaseXPos(summaryScreen);
        int afterSelectedX = baseX + 0x18;
        byte visibleIndex = 0;
        for (byte i = 0; i < PageMax; i++)
        {
            Sprite sprite = summaryScreen.Sprites[SpriteTabInfo + i]!;
            if (PokemonSummaryScreen_PageIsVisble(summaryScreen, i) == 0)
            {
                Sprite.Sprite_SetDrawFlag(sprite, 0);
                continue;
            }
            ushort anim = (ushort)Sprite.Sprite_GetActiveAnim(sprite);
            if (summaryScreen.Page == i)
            {
                if (anim < 8)
                {
                    Sprite.Sprite_SetAnim(sprite, (uint)(anim + 8));
                }
            }
            else if (anim >= 8)
            {
                Sprite.Sprite_SetAnim(sprite, (uint)(anim - 8));
            }
            SpriteSystem.Sprite_GetPositionXY(sprite, out short x, out short y);
            if (summaryScreen.Page >= i)
            {
                x = (short)(baseX + visibleIndex * 16);
            }
            else
            {
                x = (short)(afterSelectedX + (visibleIndex - 1) * 16);
            }
            SpriteSystem.Sprite_SetPositionXY(sprite, x, y);
            Sprite.Sprite_SetDrawFlag(sprite, 1);
            visibleIndex++;
        }
    }

    // OFFSET 0x0208EDC4
    public static void PokemonSummaryScreen_SetPageArrowsPos(PokemonSummaryScreen summaryScreen)
    {
        if (PokemonSummaryScreen_CountVisiblePages(summaryScreen) <= 1)
        {
            PokemonSummaryScreen_UpdatePageArrows(summaryScreen, 0);
        }
        byte baseX = CalcPageTabsBaseXPos(summaryScreen);
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpritePageArrowLeft]!, (short)(baseX - 0xC), 0x18);
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpritePageArrowRight]!, (short)(0x174 - baseX), 0x18);
    }

    // OFFSET 0x0208EE10
    public static void PokemonSummaryScreen_UpdatePageArrows(PokemonSummaryScreen summaryScreen, int draw)
    {
        if (PokemonSummaryScreen_CountVisiblePages(summaryScreen) <= 1)
        {
            draw = 0;
        }
        SetSpriteDraw(summaryScreen, SpritePageArrowLeft, draw);
        SetSpriteDraw(summaryScreen, SpritePageArrowRight, draw);
    }

    // OFFSET 0x0208EE3C
    public static void PokemonSummaryScreen_SetCaughtBallGfx(PokemonSummaryScreen summaryScreen)
    {
        byte ball = summaryScreen.MonData.CaughtBall;
        int member = ball == 0 ? 0x15 : ball + 0x14;
        SpriteSystem.SpriteSystem_ReplaceCharResObj(summaryScreen.SpriteSys!, summaryScreen.SpriteMan!, SummaryArchive, member, 0, ResourceCaughtBall);
        int palette = RomData.Arm9((uint)(BallPaletteTable + summaryScreen.MonData.CaughtBall), 1)[0] + 0x25;
        SpriteSystem.SpriteSystem_ReplacePlttResObj(summaryScreen.SpriteSys!, summaryScreen.SpriteMan!, SummaryArchive, palette, 0, PaletteResourceCaughtBall);
    }

    // OFFSET 0x0208EE9C
    public static void PokemonSummaryScreen_UpdateStatusIcon(PokemonSummaryScreen summaryScreen)
    {
        uint status = summaryScreen.MonData.Status;
        if (status == Pokemon.SummaryStatusNone)
        {
            SetSpriteDraw(summaryScreen, SpriteStatusIcon, 0);
            SetSpriteDraw(summaryScreen, SpritePokerusIcon, 0);
            return;
        }
        if (status != 0)
        {
            SetSpriteDraw(summaryScreen, SpritePokerusIcon, 0);
            SetSpriteDraw(summaryScreen, SpriteStatusIcon, 1);
            Sprite.Sprite_SetAnim(summaryScreen.Sprites[SpriteStatusIcon]!, summaryScreen.MonData.Status);
            return;
        }
        SetSpriteDraw(summaryScreen, SpritePokerusIcon, 1);
        SetSpriteDraw(summaryScreen, SpriteStatusIcon, 0);
    }

    // OFFSET 0x0208EF0C
    private static void SetMonAndTypeIcons(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummaryScreen_SetMonTypeIcons(summaryScreen);
        for (ushort i = 0; i < PokemonSummaryMonData.LearnedMovesMax; i++)
        {
            ushort move = summaryScreen.MonData.Moves[i];
            if (move == 0)
            {
                continue;
            }
            SetTypeIcon(summaryScreen, (byte)(SpriteMoveTypeIcon1 + i), (byte)(ResourceMoveType1 + i), (byte)MoveTable.MoveTable_LoadParam(move, MoveParamType));
        }
        PokemonSummaryScreen_SetMonIcon(summaryScreen);
    }

    // OFFSET 0x0208EF58
    public static void PokemonSummaryScreen_UpdateTypeIcons(PokemonSummaryScreen summaryScreen)
    {
        for (ushort i = SpriteMonTypeIcon1; i <= SpriteMonIcon; i++)
        {
            SetSpriteDraw2(summaryScreen, i, 0);
        }
        switch (summaryScreen.Page)
        {
            case PageInfo:
                SetSpriteDraw2(summaryScreen, SpriteMonTypeIcon1, 1);
                if (summaryScreen.MonData.Type1 == summaryScreen.MonData.Type2)
                {
                    SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteMonTypeIcon1]!, 0xD8, 0x50);
                    return;
                }
                SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteMonTypeIcon1]!, 0xC7, 0x50);
                SetSpriteDraw2(summaryScreen, SpriteMonTypeIcon2, 1);
                SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteMonTypeIcon2]!, 0xE9, 0x50);
                return;
            case PageBattleMoves:
                ShowMoveTypeIcons(summaryScreen, MoveParamType, 0);
                return;
            case PageContestMoves:
                ShowMoveTypeIcons(summaryScreen, MoveParamContestType, ContestTypeIconOffset);
                return;
        }
    }

    private static void ShowMoveTypeIcons(PokemonSummaryScreen summaryScreen, int param, int typeOffset)
    {
        for (ushort i = 0; i < PokemonSummaryMonData.LearnedMovesMax; i++)
        {
            ushort move = summaryScreen.MonData.Moves[i];
            if (move == 0)
            {
                continue;
            }
            uint type = MoveTable.MoveTable_LoadParam(move, param);
            SetTypeIcon(summaryScreen, (byte)(SpriteMoveTypeIcon1 + i), (byte)(ResourceMoveType1 + i), (byte)(type + typeOffset));
            SetSpriteDraw2(summaryScreen, SpriteMoveTypeIcon1 + i, 1);
            SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteMoveTypeIcon1 + i]!, 0x97, (short)(i * 32 + 0x2A));
        }
        ushort learnMove = summaryScreen.Data!.Move;
        if (learnMove != 0)
        {
            uint type = MoveTable.MoveTable_LoadParam(learnMove, param);
            SetTypeIcon(summaryScreen, SpriteMoveTypeIcon5, ResourceLearnMoveType, (byte)(type + typeOffset));
            SetSpriteDraw2(summaryScreen, SpriteMoveTypeIcon5, 1);
            SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteMoveTypeIcon5]!, 0x97, 0xAA);
        }
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteMonTypeIcon1]!, 0x3F, 0x34);
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteMonTypeIcon2]!, 0x61, 0x34);
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteMoveCategory]!, 0x6C, 0x48);
    }

    // OFFSET 0x0208F16C
    public static void PokemonSummaryScreen_SetMonTypeIcons(PokemonSummaryScreen summaryScreen)
    {
        SetTypeIcon(summaryScreen, SpriteMonTypeIcon1, ResourceMonType1, summaryScreen.MonData.Type1);
        SetTypeIcon(summaryScreen, SpriteMonTypeIcon2, ResourceMonType2, summaryScreen.MonData.Type2);
    }

    // OFFSET 0x0208F194
    private static void SetTypeIcon(PokemonSummaryScreen summaryScreen, byte sprite, byte resourceID, byte type)
    {
        int narc = TypeIcon.TypeIcon_GetNARC();
        SpriteSystem.SpriteSystem_ReplaceCharResObj(summaryScreen.SpriteSys!, summaryScreen.SpriteMan!, narc, (int)TypeIcon.TypeIcon_GetChar(type), 1, resourceID);
        SpriteSystem.Sprite_SetExplicitPalette2(summaryScreen.Sprites[sprite]!, (uint)(TypeIcon.TypeIcon_GetPlttIndex(type) + 3));
    }

    // OFFSET 0x0208F1E4
    private static void FindMoveTypeIconForSlot(PokemonSummaryScreen summaryScreen, ref byte slot, out short x, out short y)
    {
        short slotY = (short)(slot * 32 + 0x2A);
        x = 0;
        y = 0;
        for (short i = 0; i < PokemonSummaryMonData.LearnedMovesMax; i++)
        {
            SpriteSystem.Sprite_GetPositionXY(summaryScreen.Sprites[SpriteMoveTypeIcon1 + i]!, out x, out y);
            if (slotY == y)
            {
                slot = (byte)i;
                return;
            }
        }
    }

    // OFFSET 0x0208F22C
    private static void PokemonSummaryScreen_SwapMoveTypeIcons(PokemonSummaryScreen summaryScreen, byte slotA, byte slotB)
    {
        FindMoveTypeIconForSlot(summaryScreen, ref slotA, out short xA, out short yA);
        FindMoveTypeIconForSlot(summaryScreen, ref slotB, out short xB, out short yB);
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteMoveTypeIcon1 + slotA]!, xB, yB);
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteMoveTypeIcon1 + slotB]!, xA, yA);
    }

    // OFFSET 0x0208F294
    private static void PokemonSummaryScreen_UpdateMoveCategoryIcon(PokemonSummaryScreen summaryScreen, uint move)
    {
        int moveClass = (int)MoveTable.MoveTable_LoadParam((int)move, MoveParamClass);
        int narc = CategoryIcon.CategoryIcon_GetNARC();
        SpriteSystem.SpriteSystem_ReplaceCharResObj(summaryScreen.SpriteSys!, summaryScreen.SpriteMan!, narc, (int)CategoryIcon.CategoryIcon_GetChar(moveClass), 1, ResourceMoveCategory);
        SpriteSystem.Sprite_SetExplicitPalette2(summaryScreen.Sprites[SpriteMoveCategory]!, (uint)(CategoryIcon.CategoryIcon_GetPlttIndex(moveClass) + 3));
    }

    // OFFSET 0x0208F2E8
    private static void PokemonSummaryScreen_UpdateMoveSelectorPos(PokemonSummaryScreen summaryScreen)
    {
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteMoveSelector1]!, 0xC2, (short)(summaryScreen.Cursor * 32 + 0x30));
    }

    // OFFSET 0x0208F310
    private static void PokemonSummaryScreen_SetMoveSelector2Pos(PokemonSummaryScreen summaryScreen)
    {
        SpriteSystem.Sprite_GetPositionXY(summaryScreen.Sprites[SpriteMoveSelector1]!, out short x, out short y);
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteMoveSelector2]!, x, y);
        SetSpriteDraw(summaryScreen, SpriteMoveSelector2, 1);
    }

    // OFFSET 0x0208F34C
    public static void PokemonSummaryScreen_InitSheenSprites(PokemonSummaryScreen summaryScreen)
    {
        summaryScreen.SheenState = 0;
        summaryScreen.SheenCount = 0;
        summaryScreen.SheenPos = 0;
        byte sheen = summaryScreen.MonData.Sheen;
        if (sheen == 0)
        {
            summaryScreen.SheenMax = 0;
        }
        else if (sheen == SheenUnfed)
        {
            summaryScreen.SheenMax = SheenSpriteMax;
        }
        else
        {
            summaryScreen.SheenMax = (byte)((sheen * SheenSpriteMax) >> 8);
        }
        for (int i = SpriteSheen1; i < SpriteSheen1 + SheenSpriteMax; i++)
        {
            Sprite.Sprite_SetDrawFlag(summaryScreen.Sprites[i]!, 0);
            Sprite.Sprite_SetAnimFrame(summaryScreen.Sprites[i]!, 0);
        }
    }

    // OFFSET 0x0208F3B0
    private static void PokemonSummaryScreen_DrawSheenSprites(PokemonSummaryScreen summaryScreen)
    {
        if (summaryScreen.Page != PageCondition || summaryScreen.SheenMax == 0)
        {
            return;
        }
        switch (summaryScreen.SheenState)
        {
            case 0:
                summaryScreen.SheenCount++;
                if (summaryScreen.SheenCount == 8)
                {
                    summaryScreen.SheenState = 1;
                    summaryScreen.SheenCount = 0;
                    summaryScreen.SheenPos = 0;
                }
                break;
            case 1:
                if (summaryScreen.SheenCount == 0)
                {
                    Sprite sprite = summaryScreen.Sprites[SpriteSheen1 + summaryScreen.SheenPos]!;
                    Sprite.Sprite_SetDrawFlag(sprite, 1);
                    Sprite.Sprite_SetAnimFrame(summaryScreen.Sprites[SpriteSheen1 + summaryScreen.SheenPos]!, 0);
                    Sprite.Sprite_SetAnim(summaryScreen.Sprites[SpriteSheen1 + summaryScreen.SheenPos]!, 0);
                    summaryScreen.SheenPos++;
                    if (summaryScreen.SheenPos == summaryScreen.SheenMax)
                    {
                        summaryScreen.SheenState = 2;
                        break;
                    }
                }
                summaryScreen.SheenCount = (byte)((summaryScreen.SheenCount + 1) % 10);
                break;
            case 2:
                if (Sprite.Sprite_GetDrawFlag(summaryScreen.Sprites[SpriteSheen1 - 1 + summaryScreen.SheenMax]!) == 0)
                {
                    summaryScreen.SheenCount = 0;
                    summaryScreen.SheenState = 3;
                }
                break;
            case 3:
                summaryScreen.SheenCount++;
                if (summaryScreen.SheenCount == 0x20)
                {
                    for (int i = SpriteSheen1; i < summaryScreen.SheenMax + SpriteSheen1; i++)
                    {
                        Sprite.Sprite_SetDrawFlag(summaryScreen.Sprites[i]!, 1);
                        Sprite.Sprite_SetAnimFrame(summaryScreen.Sprites[i]!, 0);
                        Sprite.Sprite_SetAnim(summaryScreen.Sprites[i]!, 0);
                    }
                    summaryScreen.SheenCount = 0;
                    summaryScreen.SheenState = 4;
                }
                break;
            case 4:
                summaryScreen.SheenCount++;
                if (summaryScreen.SheenCount == 0x20)
                {
                    summaryScreen.SheenState = 0;
                    summaryScreen.SheenCount = 0;
                    summaryScreen.SheenPos = 0;
                }
                break;
        }
        for (int i = SpriteSheen1; i < summaryScreen.SheenMax + SpriteSheen1; i++)
        {
            Sprite sprite = summaryScreen.Sprites[i]!;
            if (Sprite.Sprite_GetDrawFlag(sprite) == 0)
            {
                continue;
            }
            Sprite.Sprite_UpdateAnim(sprite, AnimStep);
            if (Sprite.Sprite_GetAnimFrame(sprite) == 6)
            {
                Sprite.Sprite_SetDrawFlag(sprite, 0);
            }
        }
    }

    // OFFSET 0x0208F574
    public static void PokemonSummaryScreen_UpdateMiscMonDataSprites(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummaryMonData monData = summaryScreen.MonData;
        for (int i = 0; i < NumMarkings; i++)
        {
            Sprite sprite = summaryScreen.Sprites[SpriteMarkingCircle + i]!;
            if ((monData.Markings & (1 << i)) != 0)
            {
                Sprite.Sprite_SetAnim(sprite, 1);
            }
            else
            {
                Sprite.Sprite_SetAnim(sprite, 0);
            }
        }
        if (monData.IsShiny == 1 && monData.IsEgg == 0)
        {
            SetSpriteDraw2(summaryScreen, SpriteShinyIcon, 1);
        }
        else
        {
            SetSpriteDraw2(summaryScreen, SpriteShinyIcon, 0);
        }
        if (monData.Pokerus == 2)
        {
            SetSpriteDraw2(summaryScreen, SpritePokerusCuredIcon, 1);
            return;
        }
        SetSpriteDraw2(summaryScreen, SpritePokerusCuredIcon, 0);
    }

    // OFFSET 0x0208F600
    public static void PokemonSummaryScreen_InitButtonTapAnim(PokemonSummaryScreen summaryScreen)
    {
        Sprite sprite = summaryScreen.Sprites[SpriteButtonTapCircle]!;
        Sprite.Sprite_SetDrawFlag(sprite, 1);
        Sprite.Sprite_SetAnimFrame(sprite, 0);
        Sprite.Sprite_SetAnim(sprite, 0);
        PokemonSummaryScreen_CalcSubscreenButtonTapAnimPos(summaryScreen, out short x, out short y);
        if (summaryScreen.SubscreenType == 0)
        {
            SpriteSystem.Sprite_SetPositionXY(sprite, x, (short)(y + 0xC0));
            return;
        }
        SpriteSystem.Sprite_SetPositionXY(sprite, (short)(x - 4), (short)(y + 0xC0));
    }

    // OFFSET 0x0208F684
    public static void PokemonSummaryScreen_HideButtonTapCircle(PokemonSummaryScreen summaryScreen)
    {
        if (Sprite.Sprite_GetAnimFrame(summaryScreen.Sprites[SpriteButtonTapCircle]!) == 2)
        {
            SetSpriteDraw(summaryScreen, SpriteButtonTapCircle, 0);
        }
    }

    // OFFSET 0x0208F6A4
    private static void PokemonSummaryScreen_UpdateConditionArrowSprites(PokemonSummaryScreen summaryScreen)
    {
        for (byte i = 0; i < NumContestStats; i++)
        {
            if ((summaryScreen.PageState & (1 << i)) != 0)
            {
                SetSpriteDraw(summaryScreen, SpriteConditionArrowCool + i, 1);
            }
        }
    }

    // OFFSET 0x0208F6DC
    public static void PokemonSummaryScreen_UpdateAButtonSprite(PokemonSummaryScreen summaryScreen, Window? window)
    {
        if (window == null)
        {
            SetSpriteDraw(summaryScreen, SpriteAButton, 0);
            return;
        }
        int x = Bg.Window_GetXPos(window);
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteAButton]!, (short)(x * 8 - 0xA), 8);
        SetSpriteDraw(summaryScreen, SpriteAButton, 1);
    }

    // OFFSET 0x0208F71C
    public static void PokemonSummaryScreen_SetMonIcon(PokemonSummaryScreen summaryScreen)
    {
        object mon = PokemonSummaryScreen_MonData(summaryScreen);
        uint iconIndex;
        if (summaryScreen.Data!.DataType == DataTypeBoxMon)
        {
            iconIndex = PokemonIcon.BoxPokemon_IconSpriteIndex((BoxPokemon)mon);
        }
        else
        {
            iconIndex = PokemonIcon.Pokemon_IconSpriteIndex((Pokemon)mon);
        }
        SpriteSystem.SpriteSystem_ReplaceCharResObj(summaryScreen.SpriteSys!, summaryScreen.SpriteMan!, PokemonIconArchive, (int)iconIndex, 0, ResourceMonIcon);
        PokemonSummaryMonData monData = summaryScreen.MonData;
        uint palette = PokemonIcon.PokeIconPaletteIndex(monData.Species, monData.Form, monData.IsEgg);
        SpriteSystem.Sprite_SetExplicitPalette2(summaryScreen.Sprites[SpriteMonIcon]!, palette + 7);
        uint flip = SpeciesData.SpeciesData_GetFormValue(monData.Species, monData.Form, FormParamFlip);
        Sprite.Sprite_SetFlipMode(summaryScreen.Sprites[SpriteMonIcon]!, (int)(flip ^ 1));
    }

    // OFFSET 0x0208F7A4
    public static void PokemonSummaryScreen_ShowMonIcon(PokemonSummaryScreen summaryScreen)
    {
        SpriteSystem.SpriteSystem_ReplacePlttResObj(summaryScreen.SpriteSys!, summaryScreen.SpriteMan!, PokemonIconArchive, (int)PokemonIcon.PokeIconPalettesFileIndex(), 0, PaletteResourceMonIcon);
        if (summaryScreen.Page == PageBattleMoves)
        {
            SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteMonIcon]!, 0x18, 0x30);
        }
        else
        {
            SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteMonIcon]!, 0x20, 0x44);
        }
        SetSpriteDraw2(summaryScreen, SpriteMonIcon, 1);
    }

    // OFFSET 0x0208F800
    private static short CalcContestStatDotPos(byte stat, short maxPos, short minPos)
    {
        if (minPos > maxPos)
        {
            uint scaled = (uint)((stat + 0x2C) * (minPos - maxPos)) << 16;
            return (short)(minPos - (short)((scaled / 300) >> 16));
        }
        uint offset = (uint)((stat + 0x2C) * (maxPos - minPos)) << 16;
        return (short)(minPos + (short)((offset / 300) >> 16));
    }

    // OFFSET 0x0208F844
    public static void PokemonSummaryScreen_DrawContestStatDots(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummaryMonData monData = summaryScreen.MonData;
        short x = CalcContestStatDotPos(monData.Cool, 0x58, 0x58);
        short y = CalcContestStatDotPos(monData.Cool, 0x31, 0x49);
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteContestStatDotCool]!, x, y);
        x = CalcContestStatDotPos(monData.Beauty, 0x6E, 0x58);
        y = CalcContestStatDotPos(monData.Beauty, 0x41, 0x49);
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteContestStatDotCool + 1]!, x, y);
        x = CalcContestStatDotPos(monData.Cute, 0x67, 0x58);
        y = CalcContestStatDotPos(monData.Cute, 0x5C, 0x49);
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteContestStatDotCool + 2]!, x, y);
        x = CalcContestStatDotPos(monData.Smart, 0x48, 0x57);
        y = CalcContestStatDotPos(monData.Smart, 0x5C, 0x49);
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteContestStatDotCool + 3]!, x, y);
        x = CalcContestStatDotPos(monData.Tough, 0x41, 0x57);
        y = CalcContestStatDotPos(monData.Tough, 0x41, 0x49);
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteContestStatDotCool + 4]!, x, y);
        for (int i = 0; i < NumContestStats; i++)
        {
            SetSpriteDraw(summaryScreen, SpriteContestStatDotCool + i, 1);
        }
    }

    // OFFSET 0x0208F964
    public static void PokemonSummaryScreen_HideContestStatDots(PokemonSummaryScreen summaryScreen)
    {
        for (int i = 0; i < NumContestStats; i++)
        {
            SetSpriteDraw(summaryScreen, SpriteContestStatDotCool + i, 0);
        }
    }

    // OFFSET 0x0208F9B0
    private static void SetRibbonSpriteGfx(PokemonSummaryScreen summaryScreen, byte ribbonID, byte slot)
    {
        uint gfx = Ribbon.Ribbon_GetData(ribbonID, Ribbon.ParamGfxID);
        SpriteSystem.SpriteSystem_ReplaceCharResObj(summaryScreen.SpriteSys!, summaryScreen.SpriteMan!, SummaryArchive, (int)gfx, 0, ResourceRibbon1 + slot);
        uint palette = Ribbon.Ribbon_GetData(ribbonID, Ribbon.ParamPaletteNum);
        SpriteSystem.Sprite_SetExplicitPalette2(summaryScreen.Sprites[SpriteRibbon1 + slot]!, palette + 7);
    }

    // OFFSET 0x0208FA04
    public static void PokemonSummaryScreen_UpdateRibbonSprites(PokemonSummaryScreen summaryScreen)
    {
        for (ushort i = SpriteRibbon1; i <= SpriteRibbonFlash; i++)
        {
            SetSpriteDraw(summaryScreen, i, 0);
        }
        if (summaryScreen.Page != PageRibbons)
        {
            return;
        }
        SpriteSystem.SpriteSystem_ReplacePlttResObj(summaryScreen.SpriteSys!, summaryScreen.SpriteMan!, SummaryArchive, RibbonPaletteMember, 0, PaletteResourceRibbons);
        for (ushort i = 0; i < 12; i++)
        {
            if (i >= summaryScreen.RibbonMax)
            {
                continue;
            }
            SetSpriteDraw(summaryScreen, SpriteRibbon1 + i, 1);
            SetRibbonSpriteGfx(summaryScreen, PokemonSummaryScreen_RibbonIDAt(summaryScreen, (byte)i), (byte)i);
        }
        Sprite.Sprite_SetAnim(summaryScreen.Sprites[SpriteRibbonFlash]!, 0);
    }

    // OFFSET 0x0208FAA4
    private static void PokemonSummaryScreen_RefreshRibbonSprites(PokemonSummaryScreen summaryScreen)
    {
        for (ushort i = 0; i < 8; i++)
        {
            if (summaryScreen.RibbonRow * RibbonsPerRow + i < summaryScreen.RibbonMax)
            {
                SetSpriteDraw(summaryScreen, SpriteRibbon1 + i, 1);
                SetRibbonSpriteGfx(summaryScreen, PokemonSummaryScreen_RibbonIDAt(summaryScreen, (byte)i), (byte)i);
            }
            else
            {
                SetSpriteDraw(summaryScreen, SpriteRibbon1 + i, 0);
            }
        }
    }

    // OFFSET 0x0208FB00
    private static void PokemonSummaryScreen_UpdateRibbonCursorPos(PokemonSummaryScreen summaryScreen)
    {
        byte col = summaryScreen.RibbonCol;
        SpriteSystem.Sprite_SetPositionXY(summaryScreen.Sprites[SpriteRibbonCursor]!, (short)((col & 3) * 32 + 0x84), (short)((col >> 2) * 0x28 + 0x38));
    }

    // OFFSET 0x0208FB30
    private static void PokemonSummaryScreen_UpdateRibbonFlashAnim(PokemonSummaryScreen summaryScreen)
    {
        if (Sprite.Sprite_GetDrawFlag(summaryScreen.Sprites[SpriteRibbonFlash]!) == 1)
        {
            Sprite.Sprite_UpdateAnim(summaryScreen.Sprites[SpriteRibbonFlash]!, AnimStep);
        }
    }

    // OFFSET 0x0208FB54
    public static void PokemonSummaryScreen_UpdateConditionFlashSprites(PokemonSummaryScreen summaryScreen, int show)
    {
        if (show == 0)
        {
            for (ushort i = 0; i < NumContestStats; i++)
            {
                SetSpriteDraw(summaryScreen, SpriteConditionFlashCool + i, 0);
            }
            return;
        }
        PokemonSummaryMonData monData = summaryScreen.MonData;
        byte max = monData.Cool;
        if (max < monData.Beauty)
        {
            max = monData.Beauty;
        }
        if (max < monData.Cute)
        {
            max = monData.Cute;
        }
        if (max < monData.Smart)
        {
            max = monData.Smart;
        }
        if (max < monData.Tough)
        {
            max = monData.Tough;
        }
        DrawConditionFlash(summaryScreen.Sprites[SpriteConditionFlashCool]!, monData.Cool, max, ConditionFlashPosCool);
        DrawConditionFlash(summaryScreen.Sprites[SpriteConditionFlashCool + 1]!, monData.Beauty, max, ConditionFlashPosBeauty);
        DrawConditionFlash(summaryScreen.Sprites[SpriteConditionFlashCool + 2]!, monData.Cute, max, ConditionFlashPosCute);
        DrawConditionFlash(summaryScreen.Sprites[SpriteConditionFlashCool + 3]!, monData.Smart, max, ConditionFlashPosSmart);
        DrawConditionFlash(summaryScreen.Sprites[SpriteConditionFlashCool + 4]!, monData.Tough, max, ConditionFlashPosTough);
    }

    private static int ScaleConditionOffset(int value, int distance)
    {
        return (int)((uint)(value * distance) >> 8);
    }

    // OFFSET 0x0208FC30
    private static void DrawConditionFlash(Sprite sprite, byte value, byte max, uint positions)
    {
        if (value == 0 || value != max)
        {
            Sprite.Sprite_SetDrawFlag(sprite, 0);
            return;
        }
        Sprite.Sprite_SetDrawFlag(sprite, 1);
        ushort[] pos = RomData.Arm9U16(positions, 4);
        short maxX = (short)pos[0];
        short maxY = (short)pos[1];
        short minX = (short)pos[2];
        short minY = (short)pos[3];
        int x;
        if (maxX >= minX)
        {
            x = minX + ScaleConditionOffset(value, maxX - minX);
        }
        else
        {
            x = minX - ScaleConditionOffset(value, minX - maxX);
        }
        int y;
        if (maxY >= minY)
        {
            y = minY + ScaleConditionOffset(value, maxY - minY);
        }
        else
        {
            y = minY - ScaleConditionOffset(value, minY - maxY);
        }
        SpriteSystem.Sprite_SetPositionXY(sprite, (short)x, (short)y);
        Sprite.Sprite_SetAnimFrame(sprite, 0);
        Sprite.Sprite_SetAnim(sprite, 0);
    }

    // OFFSET 0x0208FCD4
    public static void PokemonSummaryScreen_UpdateConditionFlashAnims(PokemonSummaryScreen summaryScreen)
    {
        for (int i = 0; i < NumContestStats; i++)
        {
            Sprite.Sprite_UpdateAnim(summaryScreen.Sprites[SpriteConditionFlashCool + i]!, AnimStep);
        }
    }
}
