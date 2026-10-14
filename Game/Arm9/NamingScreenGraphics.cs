using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class NamingScreen
{
    // OFFSET 0x0208769C
    private static void NamingScreen_LoadGraphicsFromNarc(NamingScreenData data, NARC narc)
    {
        BgConfig bgConfig = data.BgConfig!;
        Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 0, 0, 0, 0x60, HeapID);
        Graphics.Graphics_LoadPalette(FontArchive, 0xC, 4, 0, 0x20, HeapID);
        Bg.Bg_MaskPalette(4, 0);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 2, bgConfig, 2, 0, 0x2000, 1, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 4, bgConfig, 2, 0, 0x600, 1, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 2, bgConfig, 1, 0, 0x2000, 1, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 6, bgConfig, 1, 0, 0x380, 1, HeapID);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 7, bgConfig, 0, 0, 0x380, 1, HeapID);
        Font.Font_LoadScreenIndicatorsPalette(0, 0x180, HeapID);
        RenderWindow.LoadMessageBoxGraphics(bgConfig, 4, MessageBoxTile, MessageBoxPalette, (byte)Options.Options_Frame(data.Options!), HeapID);
        Font.Font_LoadScreenIndicatorsPalette(4, 0x180, HeapID);
        data.CharDataAlloc = Graphics.Graphics_GetCharDataFromOpenNARC(narc, 0x10, 1, out data.CharData, HeapID);
    }

    // OFFSET 0x020877C4
    public static void NamingScreen_TransferChars()
    {
        uint[] raw = RomData.Arm9U32(CharTransferTemplateAddress, 4);
        CharTransferTemplate template = new()
        {
            MaxTasks = (int)raw[0],
            SizeMain = (int)raw[1],
            SizeSub = (int)raw[2],
            HeapID = (int)raw[3],
        };
        CharTransfer.CharTransfer_Init(template);
        PlttTransfer.PlttTransfer_Init(0x14, HeapID);
        CharTransfer.CharTransfer_ClearBuffers();
        PlttTransfer.PlttTransfer_Clear();
    }

    // OFFSET 0x020877F4
    private static void NamingScreen_LoadObjectGfx(NamingScreenData data, NARC narc)
    {
        OamManager.InitModule();
        RenderOam.RenderOam_Init(0, 0x80, 0, 0x20, 0, 0x80, 0, 0x20, HeapID);
        data.SpriteList = SpriteUtil.SpriteList_InitRendering(0x2C, data.G2dRenderer, HeapID);
        SpriteUtil.SetSubScreenViewRect(data.G2dRenderer, 0, 0x100000);
        for (int i = 0; i < 4; i++)
        {
            data.SpriteResourceCollections[i] = SpriteResourceCollection.SpriteResourceCollection_New(2, i, HeapID);
        }
        data.SpriteResources[0] = SpriteResourceCollection.SpriteResourceCollection_AddTilesFrom(data.SpriteResourceCollections[0], narc, 0xA, 1, 0, 1, HeapID);
        data.SpriteResources[1] = SpriteResourceCollection.SpriteResourceCollection_AddPaletteFrom(data.SpriteResourceCollections[1], narc, 1, 0, 0, 1, 9, HeapID);
        data.SpriteResources[2] = SpriteResourceCollection.SpriteResourceCollection_AddFrom(data.SpriteResourceCollections[2], narc, 0xC, 1, 0, 2, HeapID);
        data.SpriteResources[3] = SpriteResourceCollection.SpriteResourceCollection_AddFrom(data.SpriteResourceCollections[3], narc, 0xE, 1, 0, 3, HeapID);
        if (data.Type == TypePokemon)
        {
            uint iconIndex = PokemonIcon.PokeIconSpriteIndex((uint)data.PlayerGenderOrMonSpecies, 0, (uint)data.MonForm);
            data.MonIconRawCharData = Graphics.Graphics_GetCharData(PokemonIconArchive, iconIndex, 0, out data.MonIconCharData, HeapID);
            data.MonIconRawPlttData = Graphics.Graphics_GetPlttData(PokemonIconArchive, PokemonIcon.PokeIconPalettesFileIndex(), out data.PlttData, HeapID);
        }
        data.SpriteResources[4] = SpriteResourceCollection.SpriteResourceCollection_AddTilesFrom(data.SpriteResourceCollections[0], narc, 0xB, 1, 1, 2, HeapID);
        data.SpriteResources[5] = SpriteResourceCollection.SpriteResourceCollection_AddPaletteFrom(data.SpriteResourceCollections[1], narc, 1, 0, 1, 2, 3, HeapID);
        data.SpriteResources[6] = SpriteResourceCollection.SpriteResourceCollection_AddFrom(data.SpriteResourceCollections[2], narc, 0xD, 1, 1, 2, HeapID);
        data.SpriteResources[7] = SpriteResourceCollection.SpriteResourceCollection_AddFrom(data.SpriteResourceCollections[3], narc, 0xF, 1, 1, 3, HeapID);
        SpriteTransfer.SpriteTransfer_RequestChar(data.SpriteResources[0]);
        SpriteTransfer.SpriteTransfer_RequestChar(data.SpriteResources[4]);
        SpriteTransfer.SpriteTransfer_RequestPlttWholeRange(data.SpriteResources[1]);
        SpriteTransfer.SpriteTransfer_RequestPlttWholeRange(data.SpriteResources[5]);
    }

    // OFFSET 0x020879DC
    private static void NamingScreen_SubspritePosControllerTask(SysTask task, object? param)
    {
        NamingScreenSubspritePosParams posParams = (NamingScreenSubspritePosParams)param!;
        VecFx32 parentPosition = Sprite.Sprite_GetPosition(posParams.Parent!);
        VecFx32 position = new()
        {
            X = parentPosition.X + posParams.Dx,
            Y = SpriteAnimY(posParams.Index) << 12,
            Z = 0,
        };
        Sprite.Sprite_SetPosition(posParams.Child!, position);
    }

    // OFFSET 0x02087A10
    private static void NamingScreen_InitSprites(NamingScreenData data)
    {
        SpriteUtil.SpriteResourcesHeader_Init(
            data.MainSpriteResourceHeader,
            0,
            0,
            0,
            0,
            -1,
            -1,
            0,
            1,
            data.SpriteResourceCollections[0],
            data.SpriteResourceCollections[1],
            data.SpriteResourceCollections[2],
            data.SpriteResourceCollections[3],
            null,
            null);
        SpriteUtil.SpriteResourcesHeader_Init(
            data.SubSpriteResourceHeader,
            1,
            1,
            1,
            1,
            -1,
            -1,
            0,
            0,
            data.SpriteResourceCollections[0],
            data.SpriteResourceCollections[1],
            data.SpriteResourceCollections[2],
            data.SpriteResourceCollections[3],
            null,
            null);
        AffineSpriteListTemplate template = new()
        {
            List = data.SpriteList,
            ResourceData = data.MainSpriteResourceHeader,
            Position = new VecFx32 { X = 0x20000, Y = 0x60000, Z = 0 },
            AffineScale = new VecFx32 { X = 0x1000, Y = 0x1000, Z = 0x1000 },
            AffineZRotation = 0,
            Priority = 1,
            VramType = 1,
            HeapID = HeapID,
        };
        for (int i = 0; i < SpriteCount; i++)
        {
            template.Position.X = SpriteAnimX(i) << 12;
            template.Position.Y = SpriteAnimY(i) << 12;
            data.UiSprites[i] = Sprite.SpriteList_AddAffine(template);
            Sprite.Sprite_SetAnimateFlag(data.UiSprites[i]!, 1);
            Sprite.Sprite_SetAnim(data.UiSprites[i]!, (uint)SpriteAnimID(i));
            Sprite.Sprite_SetPriority(data.UiSprites[i]!, (uint)SpriteAnimPriority(i));
        }
        Sprite.Sprite_SetDrawFlag(data.UiSprites[4]!, 0);
        Sprite.Sprite_SetDrawFlag(data.UiSprites[SpriteCursor]!, 0);
        for (int i = 0; i < NamingScreenData.FollowTaskCount; i++)
        {
            data.SpriteFollowTasks[i] = SysTaskExtensions.SysTask_StartAndAllocateParam<NamingScreenSubspritePosParams>(NamingScreen_SubspritePosControllerTask, 0x10, 5, HeapID);
            NamingScreenSubspritePosParams posParams = (NamingScreenSubspritePosParams)SysTaskManager.SysTask_GetParam(data.SpriteFollowTasks[i]!)!;
            posParams.Parent = data.UiSprites[SpriteOverlay];
            posParams.Child = data.UiSprites[i];
            posParams.Dx = SpriteAnimX(i) << 12;
            posParams.Index = i;
        }
        for (int i = 0; i < data.MaxChars; i++)
        {
            template.Position.X = (0x50 + i * 0xC) << 12;
            template.Position.Y = 0x27000;
            data.TextCursorSprites[i] = Sprite.SpriteList_AddAffine(template);
            Sprite.Sprite_SetAnimateFlag(data.TextCursorSprites[i]!, 1);
            Sprite.Sprite_SetAnim(data.TextCursorSprites[i]!, 0x2B);
        }
        NamingScreen_UpdateTextCursors(data.TextCursorSprites, data.TextCursorPos, data.MaxChars);
        NamingScreen_InitIconSprite(data, template);
        GXLayers.GXLayers_EngineAToggleLayers(0x10, 1);
        GXLayers.GXLayers_EngineBToggleLayers(0x10, 1);
    }

    // OFFSET 0x02087BE4
    private static void NamingScreen_InitIconSprite(NamingScreenData data, AffineSpriteListTemplate template)
    {
        template.Position.X = 0x18000;
        template.Position.Y = 0x8000;
        data.EntitySprites[0] = Sprite.SpriteList_AddAffine(template);
        Sprite.Sprite_SetAnimateFlag(data.EntitySprites[0]!, 1);
        switch (data.Type)
        {
            case 0:
                if (data.PlayerGenderOrMonSpecies == 0)
                {
                    Sprite.Sprite_SetAnim(data.EntitySprites[0]!, 0x30);
                }
                else
                {
                    Sprite.Sprite_SetAnim(data.EntitySprites[0]!, 0x31);
                }
                break;
            case 3:
                Sprite.Sprite_SetAnim(data.EntitySprites[0]!, 0x33);
                break;
            case 6:
                Sprite.Sprite_SetAnim(data.EntitySprites[0]!, 0x37);
                break;
            case 5:
                Sprite.Sprite_SetAnim(data.EntitySprites[0]!, 0x36);
                break;
            case 4:
            case 7:
                Sprite.Sprite_SetAnim(data.EntitySprites[0]!, 0x35);
                break;
            case 2:
                Sprite.Sprite_SetAnim(data.EntitySprites[0]!, 0x2F);
                break;
            case 1:
                Sprite.Sprite_SetAnim(data.EntitySprites[0]!, 0x32);
                if (data.MonGender != 2)
                {
                    template.Position.X = (data.MaxChars * 0xD + 0x50) << 12;
                    template.Position.Y = 0x1B000;
                    data.EntitySprites[1] = Sprite.SpriteList_AddAffine(template);
                    if (data.MonGender == 0)
                    {
                        Sprite.Sprite_SetAnim(data.EntitySprites[1]!, 0x2D);
                    }
                    else
                    {
                        Sprite.Sprite_SetAnim(data.EntitySprites[1]!, 0x2E);
                    }
                }
                break;
        }
    }

    // OFFSET 0x02087CDC
    private static void NamingScreen_WiggleOverlayTask(SysTask task, object? param)
    {
        NamingScreenOverlayWiggleParams wiggle = (NamingScreenOverlayWiggleParams)param!;
        VecFx32 position = new()
        {
            Y = wiggle.OverlayYPosition,
            Z = 0,
        };
        switch (wiggle.State)
        {
            case 0:
                position.X = wiggle.OverlayXPosition + 0x4000;
                Sprite.Sprite_SetPosition(wiggle.OverlaySprite!, position);
                break;
            case 2:
                position.X = wiggle.OverlayXPosition - 0x3000;
                Sprite.Sprite_SetPosition(wiggle.OverlaySprite!, position);
                break;
            case 4:
                position.X = wiggle.OverlayXPosition + 0x2000;
                Sprite.Sprite_SetPosition(wiggle.OverlaySprite!, position);
                break;
            case 6:
                position.X = wiggle.OverlayXPosition;
                Sprite.Sprite_SetPosition(wiggle.OverlaySprite!, position);
                SysTaskExtensions.SysTask_FinishAndFreeParam(task);
                break;
        }
        wiggle.State++;
    }

    // OFFSET 0x02087D64
    private static void NamingScreen_AnimateChangeChars(
        BgConfig bgConfig,
        Window[] windows,
        ref int state,
        int charsIdx,
        ref int freeCharsBgLayer,
        VecFx32[] charsPosition,
        Sprite?[] uiSprites,
        byte[] pixels)
    {
        int layer = freeCharsBgLayer;
        int other = layer ^ 1;
        switch (state)
        {
            case 0:
            {
                Graphics.Graphics_LoadTilemapToBgLayer(NamingScreenArchive, (uint)(charsIdx + 6), bgConfig, (uint)layer, 0, 0x380, 1, HeapID);
                NamingScreen_InitializeCharsPosition(charsPosition, layer);
                int color = RomData.Arm9(CharsBgColorAddress + (uint)charsIdx, 1)[0];
                NamingScreen_InitializeCharsGraphics(windows[layer], (ushort)((color << 4) | color), charsIdx, KeyboardTextColor, pixels);
                state++;
                break;
            }
            case 1:
                Bg.Bg_SetOffset(bgConfig, (byte)layer, 0, 0xEE);
                Bg.Bg_SetOffset(bgConfig, (byte)layer, 3, -0x50);
                state++;
                break;
            case 2:
                charsPosition[layer].X -= 0x18;
                if (charsPosition[layer].X < -1)
                {
                    SysTask task = SysTaskExtensions.SysTask_StartAndAllocateParam<NamingScreenOverlayWiggleParams>(NamingScreen_WiggleOverlayTask, 0x10, 0, HeapID)!;
                    NamingScreenOverlayWiggleParams wiggle = (NamingScreenOverlayWiggleParams)SysTaskManager.SysTask_GetParam(task)!;
                    wiggle.OverlaySprite = uiSprites[SpriteOverlay];
                    wiggle.State = 0;
                    wiggle.OverlayXPosition = Sprite.Sprite_GetPosition(uiSprites[SpriteOverlay]!).X;
                    wiggle.OverlayYPosition = Sprite.Sprite_GetPosition(uiSprites[SpriteOverlay]!).Y;
                    charsPosition[layer].X = -0xB;
                    state++;
                }
                charsPosition[other].Y -= 0xA;
                if (charsPosition[other].Y < -0xC4)
                {
                    charsPosition[other].Y = -0xC4;
                }
                Bg.Bg_SetOffset(bgConfig, (byte)layer, 0, charsPosition[layer].X);
                Bg.Bg_SetOffset(bgConfig, (byte)other, 3, charsPosition[other].Y);
                break;
            case 3:
                charsPosition[other].Y -= 0xA;
                if (charsPosition[other].Y < -0xC4)
                {
                    charsPosition[other].Y = -0xC4;
                }
                Bg.Bg_SetOffset(bgConfig, (byte)layer, 0, charsPosition[layer].X);
                Bg.Bg_SetOffset(bgConfig, (byte)other, 3, charsPosition[other].Y);
                if (charsPosition[layer].X == -0xB && charsPosition[other].Y == -0xC4)
                {
                    state++;
                    freeCharsBgLayer ^= 1;
                    NamingScreen_UpdateCharsPriorities(bgConfig, freeCharsBgLayer, charsPosition);
                    SoundPlayback.Sound_PlayEffect(SeqCharsSlideDone);
                }
                break;
        }
    }

    // OFFSET 0x02087F48
    private static void NamingScreen_PrintMessageOnWindowLeftAlign(Window window, int unused, GameString? str)
    {
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(window, 0, MessageBoxTile, MessageBoxPalette);
        Text.Text_AddPrinterWithParams(window, 1, str, 0, 0, 0, null);
        Bg.Window_CopyToVRAM(window);
    }

    // OFFSET 0x02087F78
    private static void NamingScreen_PrintMessageOnWindow(Window window, int unused, GameString? str)
    {
        uint x = 0x10;
        if (Font.Font_CalcStringWidth(0, str, 0) > 0x82)
        {
            x = 0;
        }
        Bg.Window_FillTilemap(window, 1);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, str, x, 0, 0, EntryTextColor, null);
        Bg.Window_CopyToVRAM(window);
    }

    // OFFSET 0x02087FC0
    private static void NamingScreen_InitWindows(NamingScreenData data, ApplicationManager appMan, NARC narc)
    {
        BgConfig bgConfig = data.BgConfig!;
        Bg.Window_Add(bgConfig, data.Windows[0], 0, 2, 1, 0x1A, 0xC, 1, 0x100);
        Bg.Window_Add(bgConfig, data.Windows[1], 1, 2, 1, 0x1A, 0xC, 1, 0x238);
        if (data.Type == TypeFriendCode)
        {
            Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 9, bgConfig, 1, 0, 0x380, 1, HeapID);
            data.CurrentCharsIdx = 4;
            NamingScreen_LoadKeyboardLayout(data.KeyboardChars, 4);
            NamingScreen_InitializeCharsGraphics(data.Windows[1], 0xA0A, 4, KeyboardTextColor, data.CharData!.RawData);
        }
        else
        {
            data.CurrentCharsIdx = 0;
            NamingScreen_LoadKeyboardLayout(data.KeyboardChars, 0);
            NamingScreen_InitializeCharsGraphics(data.Windows[1], 0x404, 0, KeyboardTextColor, data.CharData!.RawData);
        }
        Bg.Window_Add(bgConfig, data.Windows[2], 2, 7, 2, 0x16, 2, 0, 0x370);
        int entryWidth = data.MaxChars * 0xC / 8 + 1;
        Bg.Window_Add(bgConfig, data.Windows[WindowEnteredText], 2, 0xA, 3, (byte)entryWidth, 2, 0, 0x39C);
        Bg.Window_FillTilemap(data.Windows[WindowEnteredText], 1);
        Bg.Window_Add(bgConfig, data.Windows[WindowGroupText], 2, (byte)(entryWidth + 9), 3, 7, 2, 0, 0x3C0);
        Bg.Window_FillTilemap(data.Windows[WindowGroupText], 1);
        if (data.Type == TypeGroup)
        {
            NamingScreen_PrintMessageOnWindow(data.Windows[WindowGroupText], data.Type, data.GroupTextString);
            Bg.Window_CopyToVRAM(data.Windows[WindowGroupText]);
        }
        Bg.Window_Add(bgConfig, data.Windows[WindowPrompt], 4, 2, 0x13, 0x1B, 4, 0xC, 0x84);
        Bg.Window_FillTilemap(data.Windows[WindowPrompt], 0xF);
        NamingScreen_PrintMessageOnWindowLeftAlign(data.Windows[WindowPrompt], data.Type, data.PromptString);
        ApplicationManager.ApplicationManager_Args(appMan);
        if (data.EntryBufBak[0] != CharEOS)
        {
            CharCode.CharCode_Copy(data.EntryBuf, 0, data.EntryBufBak, 0);
            NamingScreen_PrintChars(data.Windows[WindowEnteredText], data.EntryBuf, 0, 0, 0xC, 0, EntryTextColor, null);
        }
        for (int i = 0; i < 3; i++)
        {
            Bg.Window_Add(bgConfig, data.Windows[WindowLastChar + i], 2, 0, 0, 2, 2, 0, 0x78);
            Bg.Window_FillTilemap(data.Windows[WindowLastChar + i], 0);
        }
        Bg.Window_Add(bgConfig, data.Windows[WindowLastChar + 3], 2, 0, 0, 0x10, 2, 0, 0x84);
        Bg.Window_FillTilemap(data.Windows[WindowLastChar + 3], 0);
    }

    // OFFSET 0x02088240
    private static void NamingScreen_UpdateCharsPriorities(BgConfig unused, int bgLayer, VecFx32[] unusedPositions)
    {
        Bg.Bg_SetPriority((byte)bgLayer, 1);
        Bg.Bg_SetPriority((byte)(bgLayer ^ 1), 2);
    }

    // OFFSET 0x02088260
    private static void NamingScreen_InitializeCharsPosition(VecFx32[] charsPosition, int freeBgLayer)
    {
        charsPosition[freeBgLayer].X = 0xEE;
        charsPosition[freeBgLayer].Y = -0x50;
        charsPosition[freeBgLayer ^ 1].X = -0xB;
        charsPosition[freeBgLayer ^ 1].Y = -0x50;
    }

    // OFFSET 0x02088514
    private static void NamingScreen_PaletteGlowEffect(ref ushort angle)
    {
        angle += 0x14;
        if (angle > 0x168)
        {
            angle = 0;
        }
        int sine = MathUtil.CalcSineDegrees_Wraparound(angle);
        int level = sine * 0xA / 0x1000 + 0xF;
        ushort color = (ushort)((level << 5) | 0x1D);
        Video.LoadPaletteRange(Video.MainObject, new[] { color }, 0, 0x3A, 2);
    }

    // OFFSET 0x02088554
    private static void NamingScreen_PrintChars(Window window, ushort[] chars, int x, int y, int spacing, int renderDelay, uint color, byte[]? pixels)
    {
        int i = 0;
        GameString str = GameString.String_Init(2, HeapID);
        ushort[] single = new ushort[2];
        while (chars[i] != CharEOS)
        {
            ushort c = chars[i];
            if ((ushort)(c - ControlDakuten) <= 2)
            {
                int sourceOffset = (ushort)(c - ControlDakuten) * 0x100 / 2;
                Bg.Window_BlitBitmapRect(window, SliceFrom(pixels!, sourceOffset), 0, 0, 0xC, 0xC, (ushort)(x + i * spacing), (ushort)(y + 2), 0xC, 0xC);
            }
            else if (c != ControlSkip)
            {
                single[0] = c;
                single[1] = CharEOS;
                int width = Font.Font_CalcCharArrayWidth(0, single, 0);
                GameString.String_CopyChars(str, single);
                Text.Text_AddPrinterWithParamsAndColor(window, 0, str, (uint)(x + i * spacing + (spacing - width) / 2), (uint)y, (uint)renderDelay, color, null);
            }
            i++;
        }
        GameString.String_Free(str);
    }

    private static byte[] SliceFrom(byte[] source, int offset)
    {
        byte[] slice = new byte[source.Length - offset];
        Array.Copy(source, offset, slice, 0, slice.Length);
        return slice;
    }

    // OFFSET 0x02088654
    private static byte[] NamingScreen_PrintStringOnWindowAndGetPixelBuffer(Window window, GameString str, int font, uint color)
    {
        Text.Text_AddPrinterWithParamsAndColor(window, font, str, 0, 0, 0xFF, color, null);
        return window.Pixels!;
    }

    // OFFSET 0x02088678
    private static void NamingScreen_PrintCharOnWindowAndOBJ(Window[] windows, int windowBase, ushort[] chars, byte[] pixelBuf, GameString str)
    {
        Window wide = windows[windowBase + 3];
        Bg.Window_FillTilemap(wide, 0);
        NamingScreen_PrintStringOnWindowAndGetPixelBuffer(wide, str, 2, ObjTextColor);
        byte[] wideOffsets = RomData.Arm9(ObjOffsetsWideAddress, 4);
        for (ushort i = 0; i < 4; i++)
        {
            FontOAMSystem.FontOAM_CopyWindowTiles(wide, 4, 2, i * 4, 0, pixelBuf, 0);
            Video.LoadObjectTiles(DisplaySub, pixelBuf, (uint)(wideOffsets[i] * 0x20), 0x100);
        }
        GameString charStr = GameString.String_Init(0x15, HeapID);
        uint[] charOffsets = RomData.Arm9U32(ObjOffsetsCharAddress, 3);
        ushort[] single = new ushort[2];
        for (ushort i = 0; i < 3; i++)
        {
            single[0] = chars[i];
            single[1] = CharEOS;
            Bg.Window_FillTilemap(windows[windowBase + i], 0);
            GameString.String_CopyChars(charStr, single);
            byte[] pixels = NamingScreen_PrintStringOnWindowAndGetPixelBuffer(windows[windowBase + i], charStr, 2, ObjTextColor);
            Video.LoadObjectTiles(DisplaySub, pixels, charOffsets[i] * 0x20, 0x80);
        }
        GameString.String_Free(charStr);
    }

    // OFFSET 0x02088754
    private static void NamingScreen_PrintLastCharOfEntryBuf(Window[] windows, int windowBase, ushort[] entryBuf, int textCursorPos, ushort[] tmpBuf, byte[] pixelBuf, GameString str)
    {
        ushort c;
        if (textCursorPos == 0)
        {
            c = ControlSpace;
        }
        else
        {
            c = entryBuf[textCursorPos - 1];
        }
        if ((c >= ControlDakuten && c <= ControlSkip) || (c >= ButtonPageUpper && c <= ButtonOk))
        {
            c = CharSpace;
        }
        for (int i = 0; i < 3; i++)
        {
            tmpBuf[i] = CharSpace;
        }
        tmpBuf[0] = c;
        if (c != CharSpace)
        {
            ushort[] table = ConversionTable;
            for (int row = 0; row < ConversionTableRows; row++)
            {
                if (c == table[row * 3] || c == table[row * 3 + 2])
                {
                    for (int i = 0; i < 3; i++)
                    {
                        tmpBuf[i] = table[row * 3 + i];
                    }
                    break;
                }
            }
        }
        NamingScreen_PrintCharOnWindowAndOBJ(windows, windowBase, tmpBuf, pixelBuf, str);
    }

    // OFFSET 0x02088844
    private static void NamingScreen_LoadKeyboardLayout(ushort[] keyboardChars, int charsIdx)
    {
        uint homeRow = RomData.Arm9U32(HomeRowLayoutsAddress + (uint)(charsIdx * 4), 1)[0];
        ushort[] homeRowChars = RomData.Arm9U16(homeRow, NamingScreenData.Columns);
        for (int i = 0; i < NamingScreenData.Columns; i++)
        {
            keyboardChars[i] = homeRowChars[i];
        }
        for (int row = 0; row < KeyboardPageRows; row++)
        {
            uint rowAddress = RomData.Arm9U32(CharCodesAddress + (uint)((charsIdx * KeyboardPageRows + row) * 4), 1)[0];
            ushort[] rowChars = RomData.Arm9U16(rowAddress, NamingScreenData.Columns);
            for (int i = 0; i < NamingScreenData.Columns; i++)
            {
                keyboardChars[(row + 1) * NamingScreenData.Columns + i] = rowChars[i];
            }
        }
    }

    // OFFSET 0x02088E1C
    private static void NamingScreen_UpdateTextCursors(Sprite?[] textCursorSprites, int textCursorPos, int maxChars)
    {
        for (int i = 0; i < maxChars; i++)
        {
            Sprite.Sprite_SetAnim(textCursorSprites[i]!, 0x2B);
        }
        if (textCursorPos != maxChars)
        {
            Sprite.Sprite_SetAnim(textCursorSprites[textCursorPos]!, 0x2C);
        }
    }

    // OFFSET 0x02088E58
    private static void NamingScreen_InitializeCharsGraphics(Window window, ushort bgColor, int charsIdx, uint textColor, byte[] pixels)
    {
        Bg.Window_FillTilemap(window, (byte)bgColor);
        int altColor = RomData.Arm9(CharsAltBgColorAddress + (uint)charsIdx, 1)[0];
        int x = 0x10;
        for (int i = 0; i < 6; i++)
        {
            Bg.Window_FillRectWithColor(window, altColor, (ushort)x, 0, 0x10, 0x13);
            Bg.Window_FillRectWithColor(window, altColor, (ushort)x, 0x26, 0x10, 0x13);
            Bg.Window_FillRectWithColor(window, altColor, (ushort)x, 0x4C, 0x10, 0x13);
            x += 0x20;
        }
        x = 0;
        for (int i = 0; i < 7; i++)
        {
            Bg.Window_FillRectWithColor(window, altColor, (ushort)x, 0x13, 0x10, 0x13);
            Bg.Window_FillRectWithColor(window, altColor, (ushort)x, 0x39, 0x10, 0x13);
            x += 0x20;
        }
        int y = 4;
        for (int row = 0; row < KeyboardPageRows; row++)
        {
            NamingScreen_PrintChars(window, KeyboardRow(charsIdx, row), 0, y, 0x10, 0xFF, textColor, pixels);
            y += 0x13;
        }
        Bg.Window_CopyToVRAM(window);
    }

    // OFFSET 0x02088F40
    private static void NamingScreen_UpdateSpriteAnimations(int[] spritesToUpdate, Sprite?[] sprites, int unused)
    {
        for (int i = 0; i < 3; i++)
        {
            if (spritesToUpdate[i] != 0)
            {
                for (int j = 0; j < 3; j++)
                {
                    Sprite.Sprite_SetAnim(sprites[j]!, (uint)SpriteAnimID(j));
                }
                Sprite.Sprite_SetAnim(sprites[i]!, (uint)(SpriteAnimID(i) - 3));
                break;
            }
        }
        for (int i = SpriteBackButton; i < SpriteOverlay; i++)
        {
            if (spritesToUpdate[i] != 0)
            {
                Sprite.Sprite_SetAnim(sprites[i]!, (uint)(SpriteAnimID(i) + 1));
            }
        }
        for (int i = 0; i < NamingScreenData.FollowTaskCount; i++)
        {
            spritesToUpdate[i] = 0;
        }
    }
}
