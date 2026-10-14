namespace PlatinumSharp.Game;

public static partial class BattleSubscreens
{
    private const int BattlerTypePlayerSlot2 = 4;
    private const uint BattleTypeSafari = 0x20;
    private const uint BattleTypePalPark = 0x200;
    private const uint BattleTypeNoPokeIcon = BattleTypeSafari | BattleTypePalPark;
    private const int FontMessage = 0;
    private const int FontSubscreen = 2;
    private const uint ColorButton0 = 0x10203;
    private const uint ColorButton1 = 0x40506;
    private const uint ColorButton2 = 0x70809;
    private const uint ColorButton3 = 0xA0B0C;
    private const int TextPaletteOffset = 2;
    private const int MoveNamePaletteOffset = 3;
    private const int MovePPPaletteOffset = 4;
    private const int TargetNamePaletteOffset = 6;
    private const int ButtonFontOAMFight = 0;
    private const int ButtonFontOAMBag = 1;
    private const int ButtonFontOAMPokemon = 2;
    private const int ButtonFontOAMRun = 3;
    private const int ButtonFontOAMCancel = 4;
    private const int ButtonFontOAMPPLabel = 5;
    private const int ButtonFontOAMPPCount = 9;
    private const int MessageFight = 0x39C;
    private const int MessageBag = 0x39D;
    private const int MessagePokemon = 0x39E;
    private const int MessageRun = 0x39F;
    private const int MessageCancelPartner = 0x3A0;
    private const int MessageMoveCancel = 0x3A1;
    private const int MessageTargetCancel = 0x3A2;
    private const int MessageSafariBall = 0x3A3;
    private const int MessageSafariBait = 0x3A4;
    private const int MessageSafariRock = 0x3A5;
    private const int MessagePalParkBall = 0x4C7;
    private const int MessageRunYes = 0x3AC;
    private const int MessageRunNo = 0x3AD;
    private const int MessageSwitchYes = 0x49D;
    private const int MessageSwitchNo = 0x49E;
    private const int MessageForgetMoveYes = 0x4A2;
    private const int MessageForgetMoveNo = 0x4A3;
    private const int MessageGiveUpMoveYes = 0x4C1;
    private const int MessageGiveUpMoveNo = 0x4C2;
    private const int MessageLearnMoveYes = 0x4BF;
    private const int MessageLearnMoveNo = 0x4C0;
    private const int MessageStopPlayback = 0x4EC;
    private const int MessageTargetMale = 0x3C2;
    private const int MessageTargetFemale = 0x3C3;
    private const int MessageTargetGenderless = 0x3C4;
    private const int ButtonCenterX = 0x80;
    private const int FightButtonY = 0x54;
    private const int BagButtonX = 0x28;
    private const int PokemonButtonX = 0xD8;
    private const int SideButtonY = 0xAA;
    private const int BottomButtonY = 0xB2;
    private const int YesButtonY = 0x44;
    private const int NoButtonY = 0x8C;
    private const int PokeIconFightGap = 0x10;
    private const int SubscreenPixelOffset = 0x110;
    private const ushort BlankTile = 0x2FF;
    private const int BlankPalette = 0x11;
    private const int PromptStringSize = 0x64;
    private const int TargetNameSize = 0x18;
    private const int MoveTypeParam = 3;
    private const uint MoveNameLeftPositions = 0x022702D4;
    private const uint MovePPLabelPositions = 0x02270284;
    private const uint TargetNamePositions = 0x02270328;
    private const uint TargetCancelPosition = 0x02270338;
    private const int NarcBattleSubscreenTilemaps = 7;
    private const int StopPlaybackTilemapMember = 0x2E;
    private const int StopPlaybackBuffer = 6;
    private const int ClearedBuffer = 4;

    private static readonly int[] TargetMessageByGender = { MessageTargetMale, MessageTargetFemale };

    private static (short X, short Y) RomPosition(uint table, int index)
    {
        ushort[] pos = RomData.OverlayU16(16, table + (uint)index * 4, 2);
        return ((short)pos[0], (short)pos[1]);
    }

    // OFFSET 0x02269550
    private static void BattleSubscreen_StartOpeningCurtain(BattleSubscreen subscreen, int battlerType)
    {
        subscreen.IsPanelSliding = 1;
        Video.SetWindowOutside(DisplaySub, 0x2F);
        Video.SetWindowInside(DisplaySub, 0, 0x2C);
        Video.SetWindowInside(DisplaySub, 1, 0x2C);
        Video.SetWindowHorizontal(DisplaySub, 0, 0, 0xFF);
        Video.SetWindowVertical(DisplaySub, 0, 0, 0x90);
        Video.SetWindowHorizontal(DisplaySub, 1, 0, 0xFF);
        Video.SetWindowVertical(DisplaySub, 1, 0x90, 0xC0);
        Video.SetVisibleWindows(DisplaySub, 3);
        CurtainTaskData data = new();
        data.Subscreen = subscreen;
        data.BattlerType = (byte)battlerType;
        data.SlideX = CurtainSlideXStart;
        data.SlideY = CurtainSlideYStart;
        data.ScrollX = (short)((CurtainSlideXInit - data.SlideX) / 100);
        data.ScrollY = (short)((CurtainSlideYInit - data.SlideY) / 100);
        SoundPlayback.Sound_PlayEffect(CurtainSfx);
        SysTask.SysTask_Start(BattleSubscreen_CurtainTask, data, TaskPriorityCurtain);
        if (!GameSystem.SetHBlankCallback(BattleSubscreen_CurtainHBlank, data))
        {
            ErrorHandling.GF_AssertFail();
        }
        data.VBlankTask = SysTask.SysTask_ExecuteOnVBlank(BattleSubscreen_CurtainVBlankTask, data, TaskPriorityMenuTransfer);
    }

    // OFFSET 0x02269654
    private static void BattleSubscreen_DrawActionMenuWithCurtain(BattleSubscreen subscreen)
    {
        int battlerType = subscreen.ActiveMenuData.ActionData.BattlerType;
        BattleSubscreen_DrawActionMenu(subscreen);
        BattleSubscreen_StartOpeningCurtain(subscreen, battlerType);
    }

    // OFFSET 0x02269668
    private static void BattleSubscreen_DrawActionMenu(BattleSubscreen subscreen)
    {
        ActionSelectData data = subscreen.ActiveMenuData.ActionData;
        int battlerType = data.BattlerType;
        subscreen.IsWaitingForPartner = data.IsWaitingForPartner;
        MessageLoader loader = BattleSystems.BattleSystem_GetMessageLoader(subscreen.BattleSys!)!;
        subscreen.BattlerType = (byte)battlerType;
        GameString fight;
        GameString bag;
        GameString pokemon;
        if ((BattleSystems.BattleSystem_GetBattleType(subscreen.BattleSys!) & BattleTypeSafari) != 0)
        {
            fight = MessageLoader.MessageLoader_GetNewString(loader, MessageSafariBall)!;
            bag = MessageLoader.MessageLoader_GetNewString(loader, MessageSafariBait)!;
            pokemon = MessageLoader.MessageLoader_GetNewString(loader, MessageSafariRock)!;
        }
        else if ((BattleSystems.BattleSystem_GetBattleType(subscreen.BattleSys!) & BattleTypePalPark) != 0)
        {
            fight = MessageLoader.MessageLoader_GetNewString(loader, MessagePalParkBall)!;
            bag = MessageLoader.MessageLoader_GetNewString(loader, MessageSafariBait)!;
            pokemon = MessageLoader.MessageLoader_GetNewString(loader, MessageSafariRock)!;
        }
        else
        {
            fight = MessageLoader.MessageLoader_GetNewString(loader, MessageFight)!;
            bag = MessageLoader.MessageLoader_GetNewString(loader, MessageBag)!;
            pokemon = MessageLoader.MessageLoader_GetNewString(loader, MessagePokemon)!;
        }
        GameString run;
        if (subscreen.IsWaitingForPartner == 1)
        {
            run = MessageLoader.MessageLoader_GetNewString(loader, MessageCancelPartner)!;
            subscreen.HasCancelButton = 1;
        }
        else
        {
            run = MessageLoader.MessageLoader_GetNewString(loader, MessageRun)!;
            subscreen.HasCancelButton = 0;
        }
        BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[ButtonFontOAMFight], fight, FontSubscreen, ColorButton0, TextPaletteOffset, ResourceSubscreenPalette, ButtonCenterX, FightButtonY, 1, null);
        BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[ButtonFontOAMBag], bag, FontSubscreen, ColorButton1, TextPaletteOffset, ResourceSubscreenPalette, BagButtonX, SideButtonY, 1, null);
        BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[ButtonFontOAMPokemon], pokemon, FontSubscreen, ColorButton2, TextPaletteOffset, ResourceSubscreenPalette, PokemonButtonX, SideButtonY, 1, null);
        BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[ButtonFontOAMRun], run, FontSubscreen, ColorButton3, TextPaletteOffset, ResourceSubscreenPalette, ButtonCenterX, BottomButtonY, 1, null);
        GameString.String_Free(fight);
        GameString.String_Free(bag);
        GameString.String_Free(pokemon);
        GameString.String_Free(run);
        if ((BattleSystems.BattleSystem_GetBattleType(subscreen.BattleSys!) & BattleTypeNoPokeIcon) == 0)
        {
            Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(subscreen.BattleSys!, data.Battler, data.PartySlot);
            BattleSubscreen_LoadPokeIconResources(subscreen);
            ManagedSprite icon = BattleSubscreen_CreatePokeIcon(subscreen, mon, battlerType, data.CurHp, data.MaxHp, data.BallStatusBattler)!;
            FontOAMSystem.FontOAM_GetXY(subscreen.FontOAMEntry[ButtonFontOAMFight].FontOAM, out int x, out int y);
            if (battlerType == BattlerTypePlayerSlot2)
            {
                x += subscreen.FontOAMEntry[ButtonFontOAMFight].TextWidth + PokeIconFightGap;
            }
            else
            {
                x -= PokeIconFightGap;
            }
            SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(icon, (short)x, (short)(y - SubscreenPixelOffset), SubscreenYOffset);
        }
    }

    // OFFSET 0x022698A8
    private static void BattleSubscreen_DrawFightOnlyMenuWithCurtain(BattleSubscreen subscreen)
    {
        int battlerType = subscreen.ActiveMenuData.ActionData.BattlerType;
        BattleSubscreen_DrawFightOnlyMenu(subscreen);
        BattleSubscreen_StartOpeningCurtain(subscreen, battlerType);
    }

    // OFFSET 0x022698BC
    private static void BattleSubscreen_DrawFightOnlyMenu(BattleSubscreen subscreen)
    {
        BattleSubscreen_DrawActionMenu(subscreen);
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!)!;
        Bg.Bg_FillTilemapRect(bgConfig, SubscreenFirstLayer, BlankTile, 0, 0x10, 0x20, 8, BlankPalette);
        Bg.Bg_ScheduleTilemapTransfer(bgConfig, SubscreenFirstLayer + 1);
        FontOAMSystem.FontOAM_SetDrawFlag(subscreen.FontOAMEntry[ButtonFontOAMBag].FontOAM, 0);
        FontOAMSystem.FontOAM_SetDrawFlag(subscreen.FontOAMEntry[ButtonFontOAMPokemon].FontOAM, 0);
        FontOAMSystem.FontOAM_SetDrawFlag(subscreen.FontOAMEntry[ButtonFontOAMRun].FontOAM, 0);
    }

    // OFFSET 0x02269924
    private static void BattleSubscreen_DrawPalParkMenuWithCurtain(BattleSubscreen subscreen)
    {
        int battlerType = subscreen.ActiveMenuData.ActionData.BattlerType;
        BattleSubscreen_DrawPalParkMenu(subscreen);
        BattleSubscreen_StartOpeningCurtain(subscreen, battlerType);
    }

    // OFFSET 0x02269938
    private static void BattleSubscreen_DrawPalParkMenu(BattleSubscreen subscreen)
    {
        BattleSubscreen_DrawActionMenu(subscreen);
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!)!;
        Bg.Bg_FillTilemapRect(bgConfig, SubscreenFirstLayer, BlankTile, 0, 0x10, 0xA, 8, BlankPalette);
        Bg.Bg_FillTilemapRect(bgConfig, SubscreenFirstLayer, BlankTile, 0x16, 0x10, 0xA, 8, BlankPalette);
        Bg.Bg_ScheduleTilemapTransfer(bgConfig, SubscreenFirstLayer + 1);
        FontOAMSystem.FontOAM_SetDrawFlag(subscreen.FontOAMEntry[ButtonFontOAMBag].FontOAM, 0);
        FontOAMSystem.FontOAM_SetDrawFlag(subscreen.FontOAMEntry[ButtonFontOAMPokemon].FontOAM, 0);
    }

    // OFFSET 0x022699AC
    private static void BattleSubscreen_DrawMoveSelectMenu(BattleSubscreen subscreen)
    {
        MoveSelectData moves = subscreen.ActiveMenuData.MoveData;
        subscreen.BattlerType = moves.BattlerType;
        MessageLoader loader = BattleSystems.BattleSystem_GetMessageLoader(subscreen.BattleSys!)!;
        MoveDisplayData display = BattleSubscreen_GetMoveDisplayData(subscreen, moves.BattlerType);
        BattleSubscreen_CreateMoveTypeIcons(subscreen);
        BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[0], null, FontSubscreen, ColorButton2, MoveNamePaletteOffset, ResourceSubscreenPalette, 0x40, 0x2E, 1, display.MoveNameWindows[0]);
        BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[1], null, FontSubscreen, ColorButton2, MoveNamePaletteOffset, ResourceSubscreenPalette, 0xC0, 0x2E, 1, display.MoveNameWindows[1]);
        BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[2], null, FontSubscreen, ColorButton2, MoveNamePaletteOffset, ResourceSubscreenPalette, 0x40, 0x6E, 1, display.MoveNameWindows[2]);
        BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[3], null, FontSubscreen, ColorButton2, MoveNamePaletteOffset, ResourceSubscreenPalette, 0xC0, 0x6E, 1, display.MoveNameWindows[3]);
        for (int i = 0; i < MoveSlots; i++)
        {
            uint color = BattleSubscreen_PPColor(moves.CurrPP[i], moves.MaxPP[i]);
            (short x, short y) = RomPosition(MoveNameLeftPositions, i);
            BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[ButtonFontOAMPPCount + i], null, FontMessage, color, MovePPPaletteOffset, ResourceSubscreenPalette, x, y, 0, display.PpCountWindows[i]);
            (x, y) = RomPosition(MovePPLabelPositions, i);
            BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[ButtonFontOAMPPLabel + i], null, FontMessage, color, MovePPPaletteOffset, ResourceSubscreenPalette, x, y, 0, display.PpLabelWindows[i]);
        }
        GameString cancel = MessageLoader.MessageLoader_GetNewString(loader, MessageMoveCancel)!;
        BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[ButtonFontOAMCancel], cancel, FontSubscreen, ColorButton3, TextPaletteOffset, ResourceSubscreenPalette, ButtonCenterX, BottomButtonY, 1, null);
        GameString.String_Free(cancel);
        for (int i = 0; i < MoveSlots; i++)
        {
            if (moves.MoveIDs[i] != 0)
            {
                BattleSubscreen_LoadMoveTypePalette(subscreen, (int)MoveTable.MoveTable_LoadParam(moves.MoveIDs[i], MoveTypeParam), i);
            }
            else
            {
                BattleSubscreen_DisableMoveButtons(subscreen, i);
                break;
            }
        }
        BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!);
        for (int i = 0; i < MoveSlots; i++)
        {
            if (moves.MoveIDs[i] == 0)
            {
                FontOAMSystem.FontOAM_SetDrawFlag(subscreen.FontOAMEntry[i].FontOAM, 0);
                FontOAMSystem.FontOAM_SetDrawFlag(subscreen.FontOAMEntry[ButtonFontOAMPPLabel + i].FontOAM, 0);
                FontOAMSystem.FontOAM_SetDrawFlag(subscreen.FontOAMEntry[ButtonFontOAMPPCount + i].FontOAM, 0);
            }
        }
    }

    private static void BattleSubscreen_DrawYesNoMenu(BattleSubscreen subscreen, GameString yes, GameString no)
    {
        BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[0], yes, FontSubscreen, ColorButton0, TextPaletteOffset, ResourceSubscreenPalette, ButtonCenterX, YesButtonY, 1, null);
        BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[1], no, FontSubscreen, ColorButton3, TextPaletteOffset, ResourceSubscreenPalette, ButtonCenterX, NoButtonY, 1, null);
    }

    // OFFSET 0x02269C7C
    private static void BattleSubscreen_DrawRunConfirmMenu(BattleSubscreen subscreen)
    {
        MessageLoader loader = BattleSystems.BattleSystem_GetMessageLoader(subscreen.BattleSys!)!;
        GameString yes = MessageLoader.MessageLoader_GetNewString(loader, MessageRunYes)!;
        GameString no = MessageLoader.MessageLoader_GetNewString(loader, MessageRunNo)!;
        BattleSubscreen_DrawYesNoMenu(subscreen, yes, no);
        GameString.String_Free(yes);
        GameString.String_Free(no);
    }

    // OFFSET 0x02269D14
    private static void BattleSubscreen_DrawSwitchConfirmMenu(BattleSubscreen subscreen)
    {
        MessageLoader loader = BattleSystems.BattleSystem_GetMessageLoader(subscreen.BattleSys!)!;
        GameString yes = MessageLoader.MessageLoader_GetNewString(loader, MessageSwitchYes)!;
        GameString no = MessageLoader.MessageLoader_GetNewString(loader, MessageSwitchNo)!;
        BattleSubscreen_DrawYesNoMenu(subscreen, yes, no);
        GameString.String_Free(yes);
        GameString.String_Free(no);
    }

    private static void BattleSubscreen_DrawMoveYesNoMenu(BattleSubscreen subscreen, uint yesID, uint noID)
    {
        MessageLoader loader = BattleSystems.BattleSystem_GetMessageLoader(subscreen.BattleSys!)!;
        StringTemplate template = BattleSystems.BattleSystem_GetStringTemplate(subscreen.BattleSys!)!;
        GameString yesFormat = MessageLoader.MessageLoader_GetNewString(loader, yesID)!;
        GameString noFormat = MessageLoader.MessageLoader_GetNewString(loader, noID)!;
        GameString yes = GameString.String_Init(PromptStringSize, HeapIDBattle);
        GameString no = GameString.String_Init(PromptStringSize, HeapIDBattle);
        StringTemplate.StringTemplate_SetMoveName(template, 0, subscreen.ActiveMenuData.GiveUpMoveData.Move);
        StringTemplate.StringTemplate_Format(template, yes, yesFormat);
        StringTemplate.StringTemplate_Format(template, no, noFormat);
        BattleSubscreen_DrawYesNoMenu(subscreen, yes, no);
        GameString.String_Free(yesFormat);
        GameString.String_Free(noFormat);
        GameString.String_Free(yes);
        GameString.String_Free(no);
    }

    // OFFSET 0x02269DB0
    private static void BattleSubscreen_DrawForgetMoveMenu(BattleSubscreen subscreen)
    {
        BattleSubscreen_DrawMoveYesNoMenu(subscreen, MessageForgetMoveYes, MessageForgetMoveNo);
    }

    // OFFSET 0x02269E94
    private static void BattleSubscreen_DrawStopPlaybackMenu(BattleSubscreen subscreen)
    {
        Video.SetBlendAlpha(DisplaySub, 2, 0xF, 0x1F, 0);
        GameString message = MessageLoader.MessageLoader_GetNewString(BattleSystems.BattleSystem_GetMessageLoader(subscreen.BattleSys!)!, MessageStopPlayback)!;
        byte[]? file = Graphics.Graphics_GetScrnData(NarcBattleSubscreenTilemaps, StopPlaybackTilemapMember, 1, out ScreenData? screenData, HeapIDBattle);
        Array.Copy(screenData!.RawData, 0, subscreen.TilemapBuffers[StopPlaybackBuffer]!, 0, TilemapBufferSize);
        Heap.Heap_Free(file!);
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!)!;
        Bg.Bg_LoadTilemapBuffer(bgConfig, SubscreenFirstLayer, subscreen.TilemapBuffers[StopPlaybackBuffer]!, TilemapBufferSize);
        Bg.Bg_ScheduleTilemapTransfer(bgConfig, SubscreenFirstLayer);
        bgConfig = BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!)!;
        Array.Clear(subscreen.TilemapBuffers[ClearedBuffer]!, 0, TilemapBufferSize);
        Bg.Bg_LoadTilemapBuffer(bgConfig, SubscreenFirstLayer + 1, subscreen.TilemapBuffers[ClearedBuffer]!, TilemapBufferSize);
        Bg.Bg_ScheduleTilemapTransfer(bgConfig, SubscreenFirstLayer + 1);
        BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[0], message, FontSubscreen, ColorButton0, TextPaletteOffset, ResourceSubscreenPalette, ButtonCenterX, BottomButtonY, 1, null);
        GameString.String_Free(message);
    }

    // OFFSET 0x02269F68
    private static void BattleSubscreen_DrawGiveUpMoveMenu(BattleSubscreen subscreen)
    {
        BattleSubscreen_DrawMoveYesNoMenu(subscreen, MessageGiveUpMoveYes, MessageGiveUpMoveNo);
    }

    // OFFSET 0x0226A04C
    private static void BattleSubscreen_DrawLearnMoveMenu(BattleSubscreen subscreen)
    {
        BattleSubscreen_DrawMoveYesNoMenu(subscreen, MessageLearnMoveYes, MessageLearnMoveNo);
    }

    // OFFSET 0x0226A12C
    private static void BattleSubscreen_DrawTargetSelectMenu(BattleSubscreen subscreen)
    {
        TargetSelectData data = subscreen.ActiveMenuData.TargetSelectData;
        subscreen.TargetingLayout = data.TargetingLayout;
        subscreen.BattlerType = data.BattlerType;
        MessageLoader loader = BattleSystems.BattleSystem_GetMessageLoader(subscreen.BattleSys!)!;
        StringTemplate template = BattleSystems.BattleSystem_GetStringTemplate(subscreen.BattleSys!)!;
        byte[] selectable = new byte[MoveSlots];
        BattleSubscreen_GetTargetButtons(subscreen, selectable, 0);
        BattleSubscreen_LoadPokeIconResources(subscreen);
        BattleSubscreen_DrawTargetConnectors(subscreen);
        byte[] types = new byte[BattlerObjSprites.MaxBattlers];
        BattleSystems.BattleSystem_GetBattlerTypes(subscreen.BattleSys!, types);
        byte[] battlersByType = new byte[BattlerTypeCount];
        BattleSystems.BattleSystem_GetBattlersByType(subscreen.BattleSys!, battlersByType);
        GameString name = GameString.String_Init(TargetNameSize, HeapIDBattle);
        for (int i = 0; i < MoveSlots; i++)
        {
            int battler = battlersByType[i + 2];
            TargetMonData target = data.TargetMonData[battler];
            if (target.IsPresent == 1 && selectable[i] == 1)
            {
                GameString format = target.Gender < TargetMessageByGender.Length
                    ? MessageLoader.MessageLoader_GetNewString(loader, (uint)TargetMessageByGender[target.Gender])!
                    : MessageLoader.MessageLoader_GetNewString(loader, MessageTargetGenderless)!;
                Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(subscreen.BattleSys!, battler, target.PartySlot);
                StringTemplate.StringTemplate_SetNickname(template, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
                StringTemplate.StringTemplate_Format(template, name, format);
                uint color = (i & 1) != 0 ? ColorButton0 : ColorButton1;
                (short x, short y) = RomPosition(TargetNamePositions, i);
                BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[i], name, FontSubscreen, color, TargetNamePaletteOffset, ResourceSubscreenPalette, x, y, 1, null);
                GameString.String_Free(format);
                if (i == 1)
                {
                    BattleSubscreen_CreatePokeIcon(subscreen, mon, BattlerTypeEnemySlot1, target.CurHP, target.MaxHP, target.StockStatus);
                }
                else if (i == 3)
                {
                    BattleSubscreen_CreatePokeIcon(subscreen, mon, BattlerTypeEnemySlot2, target.CurHP, target.MaxHP, target.StockStatus);
                }
            }
            else if (selectable[i] == 0)
            {
                BattleSubscreen_DisableTargetButton(subscreen, i);
            }
        }
        GameString.String_Free(name);
        GameString cancel = MessageLoader.MessageLoader_GetNewString(loader, MessageTargetCancel)!;
        (short cancelX, short cancelY) = RomPosition(TargetCancelPosition, 0);
        BattleSubscreen_CreateFontOAM(subscreen, subscreen.FontOAMEntry[ButtonFontOAMCancel], cancel, FontSubscreen, ColorButton3, TextPaletteOffset, ResourceSubscreenPalette, cancelX, cancelY, 1, null);
        GameString.String_Free(cancel);
    }
}

public sealed class CurtainTaskData
{
    public BattleSubscreen? Subscreen;
    public SysTask? VBlankTask;
    public short SlideX;
    public short SlideY;
    public short ScrollX;
    public short ScrollY;
    public short HBlankScrollY;
    public byte BattlerType;
    public byte State;
}
