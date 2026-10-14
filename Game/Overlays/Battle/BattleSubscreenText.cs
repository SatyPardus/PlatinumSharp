namespace PlatinumSharp.Game;

public static partial class BattleSubscreens
{
    private const int NarcPokemonIcons = 0x13;
    private const int PokeIconPaletteCount = 3;
    private const uint PokeIconTemplate = 0x022704B0;
    private const uint PokeIconPositions = 0x02270304;
    private const uint MoveTypeIconTemplate = 0x0227047C;
    private const uint MoveTypeIconPositions = 0x022702B4;
    private const int HealthBarPixels = 0x30;
    private const int BallStatusFainted = 2;
    private const int BallStatusNotPresent = 3;
    private const int FontOAMYOffset = 0x108;
    private const int FontOAMPriority = 0x64;
    private const int TextWindowHeight = 2;
    private const uint ColorPPEmpty = 0x70800;
    private const uint ColorPPNormal = 0x10200;
    private const uint ColorPPLow = 0x50600;
    private const uint ColorPPHalf = 0x30400;
    private const int MessagePPLabel = 0x3AA;
    private const int MessagePPCount = 0x3A9;
    private const int PPStringSize = 0x10;
    private const int MoveTypePaletteSlot = 8;
    private const int MoveTypePaletteBuffer = 5;
    private const int DisabledButtonPalette = 0xE0;
    private const int PaletteColorsPerSlot = 16;
    private const int PaletteSlotBytes = 0x20;
    private const uint DisabledMoveKeyframes = 0x0227022C;
    private const uint DisabledMoveRects = 0x02270274;
    private const uint DisabledTargetKeyframes = 0x022701FC;
    private const uint DisabledTargetRects = 0x022702C4;
    private const uint DisabledTargetPalettes = 0x022701D4;
    private const ushort TargetConnectorTile = 0x2A4;

    private static Window CopyWindow(Window src)
    {
        Window window = new();
        window.BgConfig = src.BgConfig;
        window.BgLayer = src.BgLayer;
        window.TilemapLeft = src.TilemapLeft;
        window.TilemapTop = src.TilemapTop;
        window.Width = src.Width;
        window.Height = src.Height;
        window.Palette = src.Palette;
        window.BaseTile = src.BaseTile;
        window.ColorMode = src.ColorMode;
        window.Pixels = src.Pixels;
        return window;
    }

    // OFFSET 0x0226A698
    private static void BattleSubscreen_LoadPokeIconResources(BattleSubscreen subscreen)
    {
        SpriteSystem spriteSys = BattleSystems.BattleSystem_GetSpriteSystem(subscreen.BattleSys!)!;
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(subscreen.BattleSys!)!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(subscreen.BattleSys!)!;
        SpriteSystem.SpriteSystem_LoadPaletteBuffer(paletteData, PaletteBufferSubObj, spriteSys, spriteMan, NarcPokemonIcons, (int)PokemonIcon.PokeIconPalettesFileIndex(), 0, PokeIconPaletteCount, VRamSub, ResourcePokeIconPalette);
        SpriteSystem.SpriteSystem_LoadCellResObj(spriteSys, spriteMan, NarcPokemonIcons, (int)PokemonIcon.PokeIcon32KCellsFileIndex(), 0, ResourcePokeIconAnim);
        SpriteSystem.SpriteSystem_LoadAnimResObj(spriteSys, spriteMan, NarcPokemonIcons, (int)PokemonIcon.PokeIcon32KAnimationFileIndex(), 0, ResourcePokeIconAnim);
    }

    // OFFSET 0x0226A7A4
    private static ManagedSprite? BattleSubscreen_CreatePokeIcon(BattleSubscreen subscreen, Pokemon mon, int battlerType, int curHP, int maxHP, int status)
    {
        int index = battlerType >= 2 ? battlerType - 2 : battlerType;
        if (subscreen.TargetSelectPokeIconSprites[index] != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (subscreen.PokeIconAnimTasks[index] != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        int resourceID;
        switch (battlerType)
        {
            case 0:
            case 2:
            case BattlerTypePlayerSlot2:
                resourceID = ResourcePokeIconTiles;
                break;
            case BattlerTypeEnemySlot1:
                resourceID = ResourcePokeIconTiles + 1;
                break;
            case BattlerTypeEnemySlot2:
                resourceID = ResourcePokeIconTiles + 2;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                return null;
        }
        SpriteSystem spriteSys = BattleSystems.BattleSystem_GetSpriteSystem(subscreen.BattleSys!)!;
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(subscreen.BattleSys!)!;
        SpriteSystem.SpriteSystem_LoadCharResObj(spriteSys, spriteMan, NarcPokemonIcons, (int)PokemonIcon.Pokemon_IconSpriteIndex(mon), 0, VRamSub, resourceID);
        SpriteTemplate template = RomSpriteTemplate(PokeIconTemplate);
        template.Resources[0] = resourceID;
        (short x, short y) = RomPosition(PokeIconPositions, index);
        template.X = x;
        template.Y = y;
        ManagedSprite sprite = SpriteSystem.SpriteSystem_NewSprite(spriteSys, spriteMan, template)!;
        SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(sprite, template.X, template.Y, SubscreenYOffset);
        Sprite.Sprite_SetExplicitPaletteOffsetAutoAdjust(sprite.Sprite!, PokemonIcon.Pokemon_IconPaletteIndex(mon));
        if (curHP != 0 && status != BallStatusFainted && status != BallStatusNotPresent)
        {
            uint anim = 0;
            switch (HealthBar.HealthBar_Color((ushort)curHP, (ushort)maxHP, HealthBarPixels))
            {
                case 4:
                    anim = 1;
                    break;
                case 3:
                    anim = 2;
                    break;
                case 2:
                    anim = 3;
                    break;
                case 1:
                    anim = 4;
                    break;
            }
            SpriteSystem.ManagedSprite_SetAnim(sprite, anim);
        }
        SpriteSystem.ManagedSprite_TickFrame(sprite);
        subscreen.TargetSelectPokeIconSprites[index] = sprite;
        subscreen.PokeIconAnimTasks[index] = SysTask.SysTask_Start(BattleSubscreen_PokeIconAnimTask, sprite, TaskPriorityButtonAnim);
        return sprite;
    }

    // OFFSET 0x0226A928
    private static void BattleSubscreen_PokeIconAnimTask(SysTask task, object? param)
    {
        SpriteSystem.ManagedSprite_TickFrame((ManagedSprite)param!);
    }

    // OFFSET 0x0226A95C
    private static void BattleSubscreen_CalcTextWidth(GameString? text, int font, out int pixelWidth, out int tileWidth)
    {
        pixelWidth = Font.Font_CalcStringWidth(font, text, 0);
        tileWidth = pixelWidth / 8;
        if (pixelWidth % 8 != 0)
        {
            tileWidth++;
        }
    }

    // OFFSET 0x0226A98C
    private static void BattleSubscreen_CreateFontOAM(BattleSubscreen subscreen, FontOAMEntry entry, GameString? text, int font, uint color, uint paletteOffset, int paletteResourceID, int x, int y, int centered, TextWindowLayout? layout)
    {
        if (entry.FontOAM != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(subscreen.BattleSys!)!;
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!)!;
        int pixelWidth;
        int tileWidth;
        if (layout == null)
        {
            BattleSubscreen_CalcTextWidth(text, font, out pixelWidth, out tileWidth);
        }
        else
        {
            pixelWidth = layout.PixelWidth;
            tileWidth = layout.TileWidth;
        }
        Window window;
        if (layout == null)
        {
            window = new Window();
            Bg.Window_Init(window);
            Bg.Window_AddToTopLeftCorner(bgConfig, window, (byte)tileWidth, TextWindowHeight, 0, 0);
            Text.Text_AddPrinterWithParamsColorAndSpacing(window, font, text, 0, 0, 0xFF, color, 0, 0, null);
        }
        else
        {
            window = CopyWindow(layout.Window);
        }
        int size = FontOAMSystem.FontOAM_CalcCharSize(window, VRamSub, HeapIDBattle);
        CharTransferAllocation allocation = new();
        CharTransfer.CharTransfer_AllocRange(size, true, VRamSub, allocation);
        if (centered == 1)
        {
            x -= pixelWidth / 2;
        }
        FontOAMInitData init = new();
        init.Manager = subscreen.FontOAMManager!;
        init.Window = window;
        init.SpriteList = SpriteSystem.SpriteManager_GetSpriteList(spriteMan);
        init.PaletteProxy = SpriteSystem.SpriteManager_FindPlttResourceProxy(spriteMan, paletteResourceID);
        init.Parent = null;
        init.CharOffset = (uint)allocation.Offset;
        init.X = x;
        init.Y = y + FontOAMYOffset;
        init.BgPriority = 0;
        init.Priority = FontOAMPriority;
        init.VramType = VRamSub;
        init.HeapID = HeapIDBattle;
        FontOAM fontOAM = FontOAMSystem.FontOAM_Create(init);
        FontOAMSystem.FontOAM_SetExplicitPaletteOffsetAutoAdjust(fontOAM, paletteOffset);
        FontOAMSystem.FontOAM_SetXY(fontOAM, x, y + FontOAMYOffset);
        if (layout == null)
        {
            Bg.Window_Remove(window);
        }
        entry.FontOAM = fontOAM;
        entry.Allocation.Size = allocation.Size;
        entry.Allocation.Offset = allocation.Offset;
        entry.Allocation.VramType = allocation.VramType;
        entry.Allocation.AtEnd = allocation.AtEnd;
        entry.TextWidth = (ushort)pixelWidth;
    }

    // OFFSET 0x0226AAF8
    private static void BattleSubscreen_DrawTargetConnectors(BattleSubscreen subscreen)
    {
        byte[] tilemap = Bg.Bg_GetTilemapBuffer(BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!)!, SubscreenFirstLayer + 1)!;
        byte[] buttons = new byte[MoveSlots];
        byte[] highlighted = new byte[MoveSlots];
        BattleSubscreen_GetTargetButtons(subscreen, buttons, 0);
        if (buttons[0] == 1)
        {
            BattleSubscreen_GetHighlightedTargets(subscreen, 1, highlighted, 0);
            if (highlighted[2] == 1)
            {
                FillTilemapEntries(tilemap, 0xF, 0x10, 0xC, 0x10);
            }
            if (highlighted[3] == 1)
            {
                FillTilemapEntries(tilemap, 2, 0xC, 10, 10);
            }
        }
        if (buttons[1] == 1)
        {
            BattleSubscreen_GetHighlightedTargets(subscreen, 2, highlighted, 0);
            if (highlighted[2] == 1)
            {
                FillTilemapEntries(tilemap, 0x13, 0x1D, 10, 10);
            }
            if (highlighted[3] == 1)
            {
                FillTilemapEntries(tilemap, 0xF, 0x10, 3, 7);
            }
        }
    }

    private static void FillTilemapEntries(byte[] tilemap, int left, int right, int top, int bottom)
    {
        for (int y = top; y <= bottom; y++)
        {
            for (int x = left; x <= right; x++)
            {
                int index = (y * SubscreenTilemapWidth + x) * 2;
                tilemap[index] = TargetConnectorTile & 0xFF;
                tilemap[index + 1] = TargetConnectorTile >> 8;
            }
        }
    }

    // OFFSET 0x0226ABD4
    private static MoveDisplayData BattleSubscreen_GetMoveDisplayData(BattleSubscreen subscreen, int battlerType)
    {
        if (battlerType >= 2)
        {
            battlerType -= 2;
        }
        return subscreen.MoveDisplayData[battlerType];
    }

    // OFFSET 0x0226ABE8
    private static void BattleSubscreen_FreeMoveDisplayData(BattleSubscreen subscreen)
    {
        for (int i = 0; i < MoveSlots; i++)
        {
            MoveDisplayData display = subscreen.MoveDisplayData[i];
            for (int slot = 0; slot < MoveSlots; slot++)
            {
                display.MoveIcons[slot] = null;
                if (display.MoveNameWindows[slot].Window.Pixels != null)
                {
                    Bg.Window_Remove(display.MoveNameWindows[slot].Window);
                }
                if (display.PpCountWindows[slot].Window.Pixels != null)
                {
                    Bg.Window_Remove(display.PpCountWindows[slot].Window);
                }
                if (display.PpLabelWindows[slot].Window.Pixels != null)
                {
                    Bg.Window_Remove(display.PpLabelWindows[slot].Window);
                }
            }
        }
    }

    // OFFSET 0x0226AC98
    public static void BattleSubscreen_SetMoveDisplayInfo(BattleSubscreen subscreen, int battlerType, MoveDisplayInfo newInfo)
    {
        MessageLoader loader = BattleSystems.BattleSystem_GetMessageLoader(subscreen.BattleSys!)!;
        MoveDisplayData display = BattleSubscreen_GetMoveDisplayData(subscreen, battlerType);
        MoveDisplayInfo oldInfo = display.MoveDisplayInfo;
        int iconSize = SpriteSize_GetTileBytes(MoveIconSpriteSize);
        GameString ppLabel = MessageLoader.MessageLoader_GetNewString(loader, MessagePPLabel)!;
        StringTemplate template = BattleSystems.BattleSystem_GetStringTemplate(subscreen.BattleSys!)!;
        GameString ppCount = GameString.String_Init(PPStringSize, HeapIDBattle);
        GameString ppFormat = MessageLoader.MessageLoader_GetNewString(loader, MessagePPCount)!;
        for (int i = 0; i < MoveSlots; i++)
        {
            if (newInfo.Move[i] != oldInfo.Move[i] && newInfo.Move[i] != 0)
            {
                int type = (int)MoveTable.MoveTable_LoadParam(newInfo.Move[i], MoveTypeParam);
                int narc = TypeIcon.TypeIcon_GetNARC();
                byte[]? file = Graphics.Graphics_GetCharData(narc, TypeIcon.TypeIcon_GetChar(type), 1, out CharacterData? charData, HeapIDBattle);
                Array.Copy(charData!.RawData, 0, display.MoveIcons[i]!, 0, iconSize);
                Heap.Heap_Free(file!);
            }
            if (display.MoveNameWindows[i].Window.Pixels == null || (newInfo.Move[i] != oldInfo.Move[i] && newInfo.Move[i] != 0))
            {
                GameString name = MessageUtil.MessageUtil_MoveName(newInfo.Move[i], HeapIDBattle)!;
                BattleSubscreen_PrintToLayout(subscreen, name, FontSubscreen, display.MoveNameWindows[i], ColorButton2);
                GameString.String_Free(name);
            }
            if (display.PpCountWindows[i].Window.Pixels != null && display.PpLabelWindows[i].Window.Pixels != null
                && newInfo.Move[i] == 0 && newInfo.Move[i] == oldInfo.Move[i]
                && newInfo.CurPP[i] == oldInfo.CurPP[i] && newInfo.MaxPP[i] == oldInfo.MaxPP[i])
            {
                continue;
            }
            StringTemplate.StringTemplate_SetNumber(template, 0, newInfo.CurPP[i], 2, 1, 0);
            StringTemplate.StringTemplate_SetNumber(template, 1, newInfo.MaxPP[i], 2, 1, 0);
            StringTemplate.StringTemplate_Format(template, ppCount, ppFormat);
            uint color = BattleSubscreen_PPColor(newInfo.CurPP[i], newInfo.MaxPP[i]);
            if (display.PpCountWindows[i].Window.Pixels == null || newInfo.Move[i] != oldInfo.Move[i] || newInfo.CurPP[i] != oldInfo.CurPP[i])
            {
                BattleSubscreen_PrintToLayout(subscreen, ppCount, FontMessage, display.PpCountWindows[i], color);
            }
            if (display.PpLabelWindows[i].Window.Pixels == null || newInfo.Move[i] != oldInfo.Move[i] || newInfo.CurPP[i] != oldInfo.CurPP[i])
            {
                BattleSubscreen_PrintToLayout(subscreen, ppLabel, FontMessage, display.PpLabelWindows[i], color);
            }
        }
        GameString.String_Free(ppLabel);
        GameString.String_Free(ppFormat);
        GameString.String_Free(ppCount);
        for (int i = 0; i < MoveSlots; i++)
        {
            oldInfo.Move[i] = newInfo.Move[i];
            oldInfo.CurPP[i] = newInfo.CurPP[i];
            oldInfo.MaxPP[i] = newInfo.MaxPP[i];
        }
    }

    // OFFSET 0x0226AEA0
    private static void BattleSubscreen_PrintToLayout(BattleSubscreen subscreen, GameString text, int font, TextWindowLayout layout, uint color)
    {
        BattleSubscreen_CalcTextWidth(text, font, out int pixelWidth, out int tileWidth);
        layout.PixelWidth = (ushort)pixelWidth;
        layout.TileWidth = (ushort)tileWidth;
        if (layout.Window.Pixels != null)
        {
            Bg.Window_Remove(layout.Window);
        }
        Bg.Window_Init(layout.Window);
        Bg.Window_AddToTopLeftCorner(BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!)!, layout.Window, (byte)tileWidth, TextWindowHeight, 0, 0);
        Text.Text_AddPrinterWithParamsColorAndSpacing(layout.Window, font, text, 0, 0, 0xFF, color, 0, 0, null);
    }

    // OFFSET 0x0226AF0C
    private static void BattleSubscreen_CreateMoveTypeIcons(BattleSubscreen subscreen)
    {
        MoveSelectData moves = subscreen.ActiveMenuData.MoveData;
        SpriteSystem spriteSys = BattleSystems.BattleSystem_GetSpriteSystem(subscreen.BattleSys!)!;
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(subscreen.BattleSys!)!;
        MoveDisplayData display = BattleSubscreen_GetMoveDisplayData(subscreen, subscreen.BattlerType);
        SpriteTemplate template = RomSpriteTemplate(MoveTypeIconTemplate);
        for (int i = 0; i < MoveSlots; i++)
        {
            if (subscreen.MoveSelectSprites[i] != null)
            {
                ErrorHandling.GF_AssertFail();
            }
            if (moves.MoveIDs[i] == 0)
            {
                continue;
            }
            int type = (int)MoveTable.MoveTable_LoadParam(moves.MoveIDs[i], MoveTypeParam);
            template.Resources[0] = ResourceTypeIconTiles + i;
            (short x, short y) = RomPosition(MoveTypeIconPositions, i);
            template.X = x;
            template.Y = y;
            subscreen.MoveSelectSprites[i] = TypeIconSprites.TypeIcon_NewSprite(spriteSys, spriteMan, type, template);
            SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(subscreen.MoveSelectSprites[i]!, template.X, template.Y, SubscreenYOffset);
            ImageProxy proxy = Sprite.Sprite_GetImageProxy(subscreen.MoveSelectSprites[i]!.Sprite!);
            int size = SpriteSize_GetTileBytes(MoveIconSpriteSize);
            Video.LoadObjectTiles(DisplaySub, display.MoveIcons[i]!, (uint)proxy.Locations[VRamSub], (uint)size);
        }
    }

    // OFFSET 0x0226B06C
    private static void BattleSubscreen_LoadMoveTypePalette(BattleSubscreen subscreen, int moveType, int slot)
    {
        BattleParticleUtil.BattleParticleUtil_LoadPalette(BattleSystems.BattleSystem_GetPaletteData(subscreen.BattleSys!)!, moveType, MoveTypePaletteBuffer, PaletteBufferSubBg, slot + MoveTypePaletteSlot);
    }

    // OFFSET 0x0226B088
    private static void BattleSubscreen_DisableMoveButtons(BattleSubscreen subscreen, int slot)
    {
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(subscreen.BattleSys!)!;
        for (; slot < MoveSlots; slot++)
        {
            BattleSubscreen_ApplyButtonKeyframe(subscreen, RomShorts(DisabledMoveKeyframes + (uint)slot * 2, 1), RomBytes(DisabledMoveRects + (uint)slot * 4, 4), MoveTilemapBuffer, KeyframeDefault);
            PaletteData.PaletteData_LoadBuffer(paletteData, subscreen.SubscreenPaletteBuf!, DisabledButtonPalette, PaletteBufferSubBg, (ushort)((slot + MoveTypePaletteSlot) * PaletteColorsPerSlot), PaletteSlotBytes);
        }
    }

    // OFFSET 0x0226B0DC
    private static void BattleSubscreen_DisableTargetButton(BattleSubscreen subscreen, int target)
    {
        byte[] palettes = RomBytes(DisabledTargetPalettes, MoveSlots);
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!)!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(subscreen.BattleSys!)!;
        byte[] rect = RomBytes(DisabledTargetRects + (uint)target * 4, 4);
        BattleSubscreen_ApplyButtonKeyframe(subscreen, RomShorts(DisabledTargetKeyframes + (uint)target * 2, 1), rect, TargetTilemapBuffer, KeyframeDefault);
        PaletteData.PaletteData_LoadBuffer(paletteData, subscreen.SubscreenPaletteBuf!, DisabledButtonPalette, PaletteBufferSubBg, (ushort)(palettes[target] * PaletteColorsPerSlot), PaletteSlotBytes);
        Bg.Bg_ScheduleTilemapTransfer(bgConfig, SubscreenFirstLayer);
        Bg.Bg_FillTilemapRect(bgConfig, SubscreenFirstLayer + 1, BlankTile, rect[2], rect[0], rect[3] - rect[2] + 1, rect[1] - rect[0] + 1, BlankPalette);
        Bg.Bg_ScheduleTilemapTransfer(bgConfig, SubscreenFirstLayer + 1);
    }

    // OFFSET 0x0226B924
    private static uint BattleSubscreen_PPColor(int curPP, int maxPP)
    {
        if (curPP == 0)
        {
            return ColorPPEmpty;
        }
        if (maxPP == curPP)
        {
            return ColorPPNormal;
        }
        if (maxPP <= 2)
        {
            if (curPP == 1)
            {
                return ColorPPLow;
            }
        }
        else if (maxPP <= 7)
        {
            switch (curPP)
            {
                case 1:
                    return ColorPPLow;
                case 2:
                    return ColorPPHalf;
            }
        }
        else
        {
            if (curPP <= maxPP / 4)
            {
                return ColorPPLow;
            }
            if (curPP <= maxPP / 2)
            {
                return ColorPPHalf;
            }
        }
        return ColorPPNormal;
    }
}
