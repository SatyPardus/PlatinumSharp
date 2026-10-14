namespace PlatinumSharp.Game;

public sealed class MainMenuSpriteManager
{
    public const int ResourceTypeCount = 6;
    public const int ScreenCount = 2;

    public SpriteList? SpriteList;
    public readonly G2dRenderer Renderer = new();
    public readonly SpriteResourceCollection?[] ResourceCollections = new SpriteResourceCollection?[ResourceTypeCount];
    public readonly SpriteResource?[,] Resources = new SpriteResource?[ScreenCount, ResourceTypeCount];
    public readonly SpriteResourcesHeader[] ResourceHeaders = { new(), new() };
}

public sealed class MainMenuWindow
{
    public int Scrollable;
    public int DrawFrame;
    public int ClearBeforePrinting;
    public int TextRightAligned;
    public Window? Window;
    public StringTemplate? StrTemplate;
    public int Width;
    public int Height;
    public int TextXOffset;
    public int TextYOffset;
    public int BaseTile;
    public int BgLayer;
    public int TextPltt;
    public int TextBank;
    public int FrameBaseTile;
    public int FramePltt;
    public int Font;
    public uint TextColor;
    public byte BackgroundColorIdx;
    public int TextEntryID;
    public int RenderDelay;
}

public delegate void MainMenuUtilVBlankFunc(MainMenuUtilManager utilMan);

public sealed class MainMenuUtilManager
{
    public object? BgConfig;
    public int Initialized;
    public int HeapID;
    public int ScreenFadeQueuedState;
    public int FadeToWhite;
    public int SavingStatus;
    public SaveData? SaveData;
    public readonly MainMenuSpriteManager SpriteManager = new();
    public Sprite? MysteryGiftSprite;
    public int GiftSpriteDelay;
    public int BottomScreenTopY;
    public MainMenuUtilVBlankFunc? OnVBlank;
}

public static class MainMenuUtil
{
    private const int Overlay = 97;
    private const uint BgTemplateAddress = 0x0223DAAC;
    private const uint CharTransferTemplateAddress = 0x0223DA9C;
    private const int MessageArchive = 0x1A;
    private const int NoEntry = -1;
    private const int NoRenderDelay = 0xFF;
    private const uint DefaultTextColor = 0x0001020F;
    private const ushort White = 0x7FFF;
    private const int ObjLayer = 0x10;
    private const int VramMain = 1;
    private const int VramSub = 2;
    private const int UncompressedArchive = 0x10;
    private const int GiftSpriteRestY = 0x180000;
    private const int GiftSpriteFallSpeed = 0x3000;

    private static MainMenuUtilManager sMainMenuUtilManager = new();

    // OFFSET 0x02237694
    public static void MainMenuUtil_Init(int heapID)
    {
        sMainMenuUtilManager = new MainMenuUtilManager();
        sMainMenuUtilManager.Initialized = 1;
        sMainMenuUtilManager.HeapID = heapID;
    }

    // OFFSET 0x022376FC
    public static void MainMenuUtil_InitBGLayer(object bgConfig, int bgLayer, byte screenSize, uint screenBase, uint charBase)
    {
        BgTemplate template = RomStructs.BgTemplate(RomData.Overlay(Overlay, BgTemplateAddress, RomStructs.BgTemplateSize), 0);
        template.ScreenSize = screenSize;
        switch (screenSize)
        {
            case 1:
                template.BufferSize = 0x800;
                break;
            case 2:
                template.BufferSize = 0x1000;
                break;
            case 3:
                template.BufferSize = 0x1000;
                break;
            case 4:
                template.BufferSize = 0x2000;
                break;
        }
        template.ScreenBase = (byte)(screenBase >> 11);
        template.CharBase = (byte)(charBase >> 14);
        Bg.Bg_InitFromTemplate(bgConfig, (byte)bgLayer, template, 0);
        Bg.Bg_ClearTilemap(bgConfig, (byte)bgLayer);
    }

    // OFFSET 0x02237784
    public static void MainMenuUtil_SetFadeToWhite(int enable)
    {
        sMainMenuUtilManager.FadeToWhite = enable;
    }

    // OFFSET 0x02237790
    public static void MainMenuUtil_StartScreenFadeToState(int fadeType, int destState, ref int state, int waitState)
    {
        if (sMainMenuUtilManager.FadeToWhite == 0)
        {
            ScreenFadeControl.StartScreenFade(0, fadeType, fadeType, 0, 6, 1, sMainMenuUtilManager.HeapID);
        }
        else
        {
            ScreenFadeControl.StartScreenFade(0, fadeType, fadeType, White, 6, 1, sMainMenuUtilManager.HeapID);
        }
        state = waitState;
        sMainMenuUtilManager.ScreenFadeQueuedState = destState;
    }

    // OFFSET 0x022377F0
    public static void MainMenuUtil_CheckScreenFadeDone(ref int state)
    {
        if (ScreenFadeControl.IsScreenFadeDone())
        {
            state = sMainMenuUtilManager.ScreenFadeQueuedState;
        }
    }

    // OFFSET 0x02237808
    public static void MainMenuUtil_InitWindow(MainMenuWindow mainMenuWindow, Window window, int textPalette, int textBank, int baseTile, int framePalette)
    {
        mainMenuWindow.TextRightAligned = 0;
        mainMenuWindow.Width = 0;
        mainMenuWindow.Height = 0;
        mainMenuWindow.BaseTile = 0;
        mainMenuWindow.DrawFrame = 1;
        mainMenuWindow.ClearBeforePrinting = 1;
        mainMenuWindow.TextEntryID = NoEntry;
        mainMenuWindow.Window = window;
        mainMenuWindow.TextPltt = textPalette;
        mainMenuWindow.TextBank = textBank;
        mainMenuWindow.FrameBaseTile = baseTile;
        mainMenuWindow.FramePltt = framePalette;
        mainMenuWindow.BgLayer = 0;
        mainMenuWindow.Font = 0;
        mainMenuWindow.TextColor = DefaultTextColor;
        mainMenuWindow.BackgroundColorIdx = 0xF;
        mainMenuWindow.Scrollable = 0;
        mainMenuWindow.TextYOffset = 0;
        mainMenuWindow.TextXOffset = 0;
        mainMenuWindow.StrTemplate = null;
        mainMenuWindow.RenderDelay = NoRenderDelay;
    }

    // OFFSET 0x02237858
    public static void MainMenuWindow_SetDimensionsAndBasetile(MainMenuWindow window, int width, int height, int baseTile)
    {
        window.Width = width;
        window.Height = height;
        window.BaseTile = baseTile;
    }

    // OFFSET 0x02237860
    public static void MainMenuWindow_SetScrollableAndFont(MainMenuWindow window, int scrollable, int font)
    {
        window.Scrollable = scrollable;
        window.Font = font;
    }

    // OFFSET 0x02237868
    public static void MainMenuWindow_SetTextOffsets(MainMenuWindow window, int xOffset, int yOffset)
    {
        window.TextXOffset = xOffset;
        window.TextYOffset = yOffset;
    }

    // OFFSET 0x02237870
    private static int MainMenuWindow_PrintText(MainMenuWindow window, int textEntryID)
    {
        int printerID = 0;
        if (textEntryID != NoEntry && window.TextEntryID != textEntryID)
        {
            window.TextEntryID = textEntryID;
            if (window.ClearBeforePrinting == 1)
            {
                Bg.Window_FillTilemap(window.Window!, window.BackgroundColorIdx);
            }
            if (window.TextEntryID != NoEntry)
            {
                int heapID = sMainMenuUtilManager.HeapID;
                MessageLoader loader = MessageLoader.MessageLoader_Init(1, MessageArchive, window.TextBank, heapID)!;
                StringTemplate template = window.StrTemplate ?? StringTemplate.StringTemplate_Default(heapID);
                GameString? text = MessageUtil.MessageUtil_ExpandedString(template, loader, (uint)window.TextEntryID, heapID);
                if (window.TextRightAligned == 0)
                {
                    printerID = Text.Text_AddPrinterWithParamsAndColor(window.Window!, window.Font, text, (uint)window.TextXOffset, (uint)window.TextYOffset, (uint)window.RenderDelay, window.TextColor, null);
                }
                else
                {
                    int width = Font.Font_CalcStringWidth(window.Font, text, Font.Font_GetAttribute((byte)window.Font, 2));
                    int x = Bg.Window_GetWidth(window.Window!) * 8 - width;
                    printerID = Text.Text_AddPrinterWithParamsAndColor(window.Window!, window.Font, text, (uint)x, (uint)window.TextYOffset, (uint)window.RenderDelay, window.TextColor, null);
                    window.TextRightAligned = 0;
                }
                GameString.String_Free(text);
                if (window.StrTemplate == null)
                {
                    StringTemplate.StringTemplate_Free(template);
                }
                MessageLoader.MessageLoader_Free(loader);
            }
        }
        window.RenderDelay = NoRenderDelay;
        return printerID;
    }

    // OFFSET 0x0223795C
    public static int MainMenuUtil_ShowWindowAtPos(object bgConfig, MainMenuWindow window, int xPos, int yPos, int textEntryID)
    {
        int printerID;
        if (window.Window!.BgConfig == null)
        {
            Bg.Window_Add(bgConfig, window.Window, (byte)window.BgLayer, (byte)xPos, (byte)yPos, (byte)window.Width, (byte)window.Height, (byte)window.TextPltt, (ushort)window.BaseTile);
            printerID = MainMenuWindow_PrintText(window, textEntryID);
        }
        else
        {
            if (xPos != -1)
            {
                Bg.Window_SetXPos(window.Window, (byte)xPos);
            }
            if (yPos != -1)
            {
                Bg.Window_SetYPos(window.Window, (byte)yPos);
            }
            printerID = MainMenuWindow_PrintText(window, textEntryID);
        }
        if (window.DrawFrame == 1)
        {
            if (window.Scrollable == 0)
            {
                RenderWindow.Window_DrawStandardFrame(window.Window, 0, (ushort)window.FrameBaseTile, (byte)window.FramePltt);
            }
            else
            {
                RenderWindow.Window_DrawMessageBoxWithScrollCursor(window.Window, 0, (ushort)window.FrameBaseTile, (byte)window.FramePltt);
            }
        }
        return printerID;
    }

    // OFFSET 0x02237A18
    public static int MainMenuWindow_GetSize(MainMenuWindow window)
    {
        return window.Width * window.Height;
    }

    // OFFSET 0x02237A20
    public static void MainMenuUtil_InitCharPlttTransferBuffers()
    {
        uint[] raw = RomData.OverlayU32(Overlay, CharTransferTemplateAddress, 4);
        CharTransferTemplate template = new()
        {
            MaxTasks = (int)raw[0],
            SizeMain = (int)raw[1],
            SizeSub = (int)raw[2],
            HeapID = sMainMenuUtilManager.HeapID,
        };
        CharTransfer.CharTransfer_InitWithVramModes(template, 0x10, 0x10);
        PlttTransfer.PlttTransfer_Init(0x1E, sMainMenuUtilManager.HeapID);
        CharTransfer.CharTransfer_ClearBuffers();
        PlttTransfer.PlttTransfer_Clear();
    }

    // OFFSET 0x02237A60
    public static bool MainMenuUtil_CheckSpriteLoaderInit()
    {
        if (sMainMenuUtilManager.SpriteManager.SpriteList != null)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02237A74
    public static void MainMenuUtil_InitSpriteLoader()
    {
        MainMenuUtilManager utilMan = sMainMenuUtilManager;
        MainMenuSpriteManager spriteMan = utilMan.SpriteManager;
        OamManager.InitModule();
        RenderOam.RenderOam_Init(0, 0x7E, 0, 0x20, 0, 0x7E, 0, 0x20, utilMan.HeapID);
        spriteMan.SpriteList = SpriteUtil.SpriteList_InitRendering(0x80, spriteMan.Renderer, utilMan.HeapID);
        SpriteUtil.SetSubScreenViewRect(spriteMan.Renderer, 0, 0x100000);
        utilMan.BottomScreenTopY = 0xC0000;
        for (int i = 0; i < MainMenuSpriteManager.ResourceTypeCount; i++)
        {
            spriteMan.ResourceCollections[i] = SpriteResourceCollection.SpriteResourceCollection_New(0x20, i, utilMan.HeapID);
        }
    }

    // OFFSET 0x02237AEC
    public static MainMenuSpriteManager MainMenuUtil_GetSpriteManager()
    {
        if (sMainMenuUtilManager.SpriteManager.SpriteList == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return sMainMenuUtilManager.SpriteManager;
    }

    // OFFSET 0x02237B0C
    public static void MainMenuUtil_LoadSprite(int narcID, int tilesID, int plttID, int cellID, int animID, int screen)
    {
        MainMenuUtilManager utilMan = sMainMenuUtilManager;
        MainMenuSpriteManager spriteMan = utilMan.SpriteManager;
        int vramType = screen == 0 ? VramMain : VramSub;
        int compressed = 1;
        if (narcID == UncompressedArchive)
        {
            compressed = 0;
        }
        if (tilesID != -1)
        {
            spriteMan.Resources[screen, 0] = SpriteResourceCollection.SpriteResourceCollection_AddTiles(spriteMan.ResourceCollections[0], narcID, tilesID, compressed, screen, vramType, utilMan.HeapID);
        }
        if (plttID != -1)
        {
            spriteMan.Resources[screen, 1] = SpriteResourceCollection.SpriteResourceCollection_AddPalette(spriteMan.ResourceCollections[1], narcID, plttID, 0, screen, vramType, 3, utilMan.HeapID);
        }
        if (cellID != -1)
        {
            spriteMan.Resources[screen, 2] = SpriteResourceCollection.SpriteResourceCollection_Add(spriteMan.ResourceCollections[2], narcID, cellID, compressed, screen, 2, utilMan.HeapID);
        }
        if (animID != -1)
        {
            spriteMan.Resources[screen, 3] = SpriteResourceCollection.SpriteResourceCollection_Add(spriteMan.ResourceCollections[3], narcID, animID, compressed, screen, 3, utilMan.HeapID);
        }
        SpriteTransfer.SpriteTransfer_RequestChar(spriteMan.Resources[screen, 0]);
        SpriteTransfer.SpriteTransfer_RequestPlttFreeSpace(spriteMan.Resources[screen, 1]);
        SpriteUtil.SpriteResourcesHeader_Init(
            spriteMan.ResourceHeaders[screen],
            screen,
            screen,
            screen,
            screen,
            -1,
            -1,
            0,
            0,
            spriteMan.ResourceCollections[0],
            spriteMan.ResourceCollections[1],
            spriteMan.ResourceCollections[2],
            spriteMan.ResourceCollections[3],
            null,
            null);
        if (screen == 0)
        {
            GXLayers.GXLayers_EngineAToggleLayers(ObjLayer, 1);
        }
        else
        {
            GXLayers.GXLayers_EngineBToggleLayers(ObjLayer, 1);
        }
        GameSystem.SetVBlankCallback(TransferGraphicsOnVBlank, null);
    }

    // OFFSET 0x02237C80
    public static void MainMenuUtil_SetSubScreenViewRect(int x, int y)
    {
        SpriteUtil.SetSubScreenViewRect(sMainMenuUtilManager.SpriteManager.Renderer, x, y);
        sMainMenuUtilManager.BottomScreenTopY = y;
    }

    // OFFSET 0x02237CA0
    public static void MainMenuUtil_UpdateSprites()
    {
        MainMenuUtil_UpdateSpritesForAnimStatus(0);
    }

    // OFFSET 0x02237CAC
    public static void MainMenuUtil_UpdateSpritesForAnimStatus(int animStatus)
    {
        MainMenuUtilManager utilMan = sMainMenuUtilManager;
        if (utilMan.MysteryGiftSprite != null)
        {
            if (utilMan.GiftSpriteDelay == 0)
            {
                if (Sprite.Sprite_GetDrawFlag(utilMan.MysteryGiftSprite) == 0 && animStatus == 0)
                {
                    Sprite.Sprite_SetDrawFlag(utilMan.MysteryGiftSprite, 1);
                }
                if (utilMan.MysteryGiftSprite.Position.Y < GiftSpriteRestY)
                {
                    utilMan.MysteryGiftSprite.Position.Y += GiftSpriteFallSpeed;
                }
            }
            else
            {
                utilMan.GiftSpriteDelay--;
            }
        }
        if (utilMan.SpriteManager.SpriteList != null)
        {
            Sprite.SpriteList_Update(utilMan.SpriteManager.SpriteList);
        }
    }

    // OFFSET 0x02237D14
    public static Sprite MainMenuUtil_InitSprite(int screen, Sprite? sprite, int x, int y, int animationIdx)
    {
        MainMenuUtilManager utilMan = sMainMenuUtilManager;
        if (sprite == null)
        {
            AffineSpriteListTemplate template = new()
            {
                List = utilMan.SpriteManager.SpriteList,
                ResourceData = utilMan.SpriteManager.ResourceHeaders[screen],
                Position = new VecFx32 { X = x << 12, Y = y << 12, Z = 0 },
                AffineScale = new VecFx32 { X = 0x1000, Y = 0x1000, Z = 0x1000 },
                AffineZRotation = 0,
                Priority = 0xA,
                VramType = screen == 0 ? VramMain : VramSub,
                HeapID = utilMan.HeapID,
            };
            if (template.VramType == VramSub)
            {
                template.Position.Y += utilMan.BottomScreenTopY;
            }
            sprite = Sprite.SpriteList_AddAffine(template);
        }
        Sprite.Sprite_SetAnimateFlag(sprite!, 1);
        Sprite.Sprite_SetExplicitPriority(sprite!, 0);
        Sprite.Sprite_SetAnim(sprite!, (uint)animationIdx);
        Sprite.Sprite_SetDrawFlag(sprite!, 1);
        return sprite!;
    }

    // OFFSET 0x02237DA0
    public static void MainMenuUtil_FreeSprites()
    {
        MainMenuUtilManager utilMan = sMainMenuUtilManager;
        MainMenuSpriteManager spriteMan = utilMan.SpriteManager;
        if (utilMan.MysteryGiftSprite != null)
        {
            Sprite.Sprite_Delete(utilMan.MysteryGiftSprite);
            utilMan.MysteryGiftSprite = null;
        }
        if (spriteMan.Resources[0, 0] != null)
        {
            SpriteTransfer.SpriteTransfer_ResetCharTransfer(spriteMan.Resources[0, 0]);
        }
        if (spriteMan.Resources[1, 0] != null)
        {
            SpriteTransfer.SpriteTransfer_ResetCharTransfer(spriteMan.Resources[1, 0]);
        }
        if (spriteMan.Resources[0, 1] != null)
        {
            SpriteTransfer.SpriteTransfer_ResetPlttTransfer(spriteMan.Resources[0, 1]);
        }
        if (spriteMan.Resources[1, 1] != null)
        {
            SpriteTransfer.SpriteTransfer_ResetPlttTransfer(spriteMan.Resources[1, 1]);
        }
        for (int i = 0; i < MainMenuSpriteManager.ResourceTypeCount; i++)
        {
            SpriteResourceCollection.SpriteResourceCollection_Delete(spriteMan.ResourceCollections[i]);
            spriteMan.ResourceCollections[i] = null;
        }
        Sprite.SpriteList_Delete(spriteMan.SpriteList);
        spriteMan.SpriteList = null;
        RenderOam.RenderOam_Free();
        CharTransfer.CharTransfer_Free();
        PlttTransfer.PlttTransfer_Free();
        GameSystem.SetVBlankCallback(null, null);
    }

    // OFFSET 0x02237E58
    private static void TransferGraphicsOnVBlank(object? unused)
    {
        MainMenuUtilManager utilMan = sMainMenuUtilManager;
        if (utilMan.OnVBlank != null)
        {
            utilMan.OnVBlank(utilMan);
            utilMan.OnVBlank = null;
        }
        VramTransfer.VramTransfer_Process();
        RenderOam.RenderOam_Transfer();
        if (utilMan.BgConfig != null)
        {
            Bg.Bg_RunScheduledUpdates(utilMan.BgConfig);
        }
    }

    // OFFSET 0x022383DC
    private static void TerminateOnGBACartRemoved()
    {
        if (!Platform.IsFolded && !Cartridge.IsAgbCartridge())
        {
            Unported.sub_020D12E4();
        }
    }

    // OFFSET 0x02238400
    public static void MainMenuUtil_ToggleTerminateOnGBACartRemoved(int enable)
    {
        if (enable == 1)
        {
            Interrupts.MasterEnable = false;
            Interrupts.SetHandler(Cartridge.Interrupt, TerminateOnGBACartRemoved);
            Interrupts.Enable(Cartridge.Interrupt);
            Interrupts.MasterEnable = true;
        }
        else
        {
            Interrupts.Disable(Cartridge.Interrupt);
        }
    }

    // OFFSET 0x0223843C
    private static void GBACartRemovedIrqStub()
    {
    }

    // OFFSET 0x02238440
    public static void MainMenuUtil_UnsetGBACartIRQFunc()
    {
        Interrupts.MasterEnable = false;
        Interrupts.SetHandler(Cartridge.Interrupt, GBACartRemovedIrqStub);
        Interrupts.Enable(Cartridge.Interrupt);
        Interrupts.MasterEnable = true;
    }
}
