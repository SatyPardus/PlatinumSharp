using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed class Shop
{
    public const int WindowCount = 6;
    public const int SpriteCount = 4;

    public object BgConfig = null!;
    public BagContext? BagContext;
    public readonly Window[] Windows = Enumerable.Range(0, WindowCount).Select(_ => new Window()).ToArray();
    public ListMenu? ItemListMenu;
    public StringListEntry[]? ItemList;
    public Menu? Menu;
    public StringListEntry[]? ContextList;
    public MessageLoader? Messages;
    public StringTemplate? Template;
    public Camera? Camera;
    public readonly Overlay5SpriteManager SpriteManager = new();
    public readonly Sprite?[] Sprites = new Sprite?[SpriteCount];
    public ushort UpArrowDrawFlag;
    public ushort DownArrowDrawFlag;
    public TrainerInfo? TrainerInfo;
    public object? Destination;
    public Options? Options;
    public Journal? Journal;
    public GameRecords? Records;
    public SaveData? SaveData;
    public VarsFlags? VarsFlags;
    public int IsDepartmentStore;
    public ushort[] Items = Array.Empty<ushort>();
    public byte ItemCount;
    public byte State;
    public GameString? Text;
    public readonly byte[] BgPriorities = new byte[4];
    public int SavedLayers;
    public byte PrinterID;
    public byte CameraMoveCount;
    public byte CameraMoveTarget;
    public byte ItemsBought;
    public byte ItemsSold;
    public byte MartType;
    public ushort SelectedItem;
    public short Quantity;
    public ushort MaxQuantity;
    public uint Price;
    public FontSpecialCharsContext? FontSpecialChars;
}

public static class ShopMenu
{
    public const int MartTypeNormal = 0;
    public const int MartTypeDecor = 1;
    public const int MartTypeSeal = 2;
    public const int MartTypeFrontier = 3;

    private const int OverlayID = 7;
    private const int HeapID = 0xB;
    private const int MaxItems = 0x100;
    private const ushort ItemListEnd = 0xFFFF;
    private const int NarcMessages = 0x1A;
    private const int MessageBankShop = 0x21F;
    private const int MessageBankItemNames = 0x188;
    private const int MessageBankMoveNames = 0x287;
    private const int MessageBankGoodsNames = 0x272;
    private const int MessageBankSealNames = 0xC;
    private const int MessageBankSealDescriptions = 0x21E;
    private const int GoodsDescriptionOffset = 0x8A;
    private const int NarcShopGraphics = 0x3E;
    private const int NarcItemIcons = 0x10;
    private const int BgLayerMain1 = 1;
    private const int BgLayerMain3 = 3;
    private const ushort ContextWindowBaseTile = 0x1E9;
    private const ushort ContextFrameTile = 0x3D9;
    private const int StandardFramePalette = 0xB;
    private const ushort StandardFrameTile = 0x1F;
    private const int MessageBoxPalette = 0xA;
    private const ushort FirstTMHM = 0x148;
    private const ushort LastTMHM = 0x1A4;
    private const ushort FirstTMMinusOne = 0x147;
    private const int TMTextXOffset = 0x23;
    private const int MaxQuantityCap = 99;
    private const ushort ItemPokeBall = 4;
    private const ushort ItemPremierBall = 0xC;
    private const int PremierBallThreshold = 10;
    private const int GoodsPCCapacity = 200;
    private const int RecordMoneySpent = 0x23;
    private const int RecordBattlePointsSpent = 0x45;
    private const int RecordPremierBalls = 0x32;
    private const int JournalDataLocation = 1;
    private const int MapLabelVeilstoneStore = 0x65;
    private const int MapVeilstoneStore = 0x51;
    private const int MapFrontierShop = 0x1BE;
    private const uint ColorItemDescription = 0xF0E00;
    private const uint ColorItemList = 0x10200;
    private const int SeqSeCursor = 0x638;
    private const int SeqSeSelect = 0x5DC;
    private const int SeqSePurchase = 0x644;
    private const uint PadButtonA = 1;
    private const uint PadButtonB = 2;
    private const int DirNorth = 0;
    private const int DirSouth = 1;
    private const int DirEast = 3;
    private const byte CameraMovesFacing = 10;
    private const byte CameraMovesOther = 8;
    private const uint WindowTemplateDecorDescription = 0x0224F2BC;
    private const uint WindowTemplateFrontierMoney = 0x0224F2C4;
    private const uint WindowTemplateYesNo = 0x0224F2CC;
    private const uint CameraMoveForward = 0x0224F2D4;
    private const uint CameraMoveBack = 0x0224F2E0;
    private const uint SpriteResourcePaths = 0x0224F2EC;
    private const int SpriteResourcePathCount = 7;
    private const uint ItemListTemplate = 0x0224F308;
    private const uint WindowTemplates = 0x0224F328;
    private const uint SpriteTemplates = 0x0224F358;
    private const int SpriteTemplateSize = 0x28;
    private const uint FrontierPrices = 0x0224F3F8;
    private const int FrontierPriceCount = 0x29;
    private const uint SellPockets = 0x0224F49C;
    private const int SellPocketCount = 8;
    private const int MaxSprites = 4;
    private const int SpriteUpArrow = 0;
    private const int SpriteDownArrow = 1;
    private const int SpriteCursor = 2;
    private const int SpriteItemIcon = 3;
    private const int WindowItemList = 0;
    private const int WindowItemDescription = 1;
    private const int WindowCurrentMoney = 2;
    private const int WindowQuantityTotal = 3;
    private const int WindowItemsInBag = 4;
    private const int WindowMessage = 5;
    private const uint ChoiceBuy = 2;
    private const uint ChoiceSell = 0xE;

    private const byte StateInit = 0;
    private const byte StateSelectContextMenu = 1;
    private const byte StateStartBuy = 2;
    private const byte StateMoveCamera = 3;
    private const byte StateSelectBuyMenu = 4;
    private const byte StateShowPurchaseMenu = 5;
    private const byte StateSelectPurchaseMenu = 6;
    private const byte StateShowConfirmPurchase = 7;
    private const byte StateSelectConfirmPurchase = 8;
    private const byte StateConfirmItemPurchase = 9;
    private const byte StateFinishPurchase = 0xA;
    private const byte StateFinishFreePremierBall = 0xB;
    private const byte StateMoveCameraBack = 0xC;
    private const byte StateReinitContextMenu = 0xD;
    private const byte StateStartSell = 0xE;
    private const byte StateFinishScreenTransition = 0xF;
    private const byte StateSelling = 0x10;
    private const byte StateReinitMerchantMessage = 0x12;
    private const byte StatePrintExit = 0x13;
    private const byte StateExit = 0x14;

    private static WindowTemplate WindowTemplateAt(uint address)
    {
        return RomStructs.WindowTemplate(RomData.Overlay(OverlayID, address, 8), 0);
    }

    private static VecFx32 VectorAt(uint address)
    {
        uint[] words = RomData.OverlayU32(OverlayID, address, 3);
        return new VecFx32 { X = (int)words[0], Y = (int)words[1], Z = (int)words[2] };
    }

    private static string? OverlayString(uint address)
    {
        if (address == 0)
        {
            return null;
        }
        List<byte> bytes = new();
        for (uint offset = 0; ; offset++)
        {
            byte value = RomData.Overlay(OverlayID, address + offset, 1)[0];
            if (value == 0)
            {
                break;
            }
            bytes.Add(value);
        }
        return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
    }

    private static void PrintMessage(Shop shop, int messageID, Window window)
    {
        GameString format = MessageLoader.MessageLoader_GetNewString(shop.Messages!, (uint)messageID)!;
        StringTemplate.StringTemplate_Format(shop.Template!, shop.Text!, format);
        GameString.String_Free(format);
        shop.PrinterID = FieldMessage.FieldMessage_Print(window, shop.Text!, shop.Options!, 1);
    }

    private static void RestoreBuyMenu(Shop shop)
    {
        RenderWindow.Window_EraseMessageBox(shop.Windows[WindowMessage], 0);
        Shop_SetScrollSpritesPositionXY(shop, 0);
        Sprite.Sprite_SetDrawFlag(shop.Sprites[SpriteUpArrow]!, shop.UpArrowDrawFlag);
        Sprite.Sprite_SetDrawFlag(shop.Sprites[SpriteDownArrow]!, shop.DownArrowDrawFlag);
        Shop_SetCursorSpritePalette(shop, 0);
        Bg.Window_ScheduleCopyToVRAM(shop.Windows[WindowItemDescription]);
    }

    private static bool UsesBag(Shop shop)
    {
        return shop.MartType == MartTypeNormal || shop.MartType == MartTypeFrontier;
    }

    // OFFSET 0x0224CD28
    private static void Shop_SetItemsForSale(Shop shop, ushort[] items)
    {
        ushort count = 0;
        while (count < MaxItems && items[count] != ItemListEnd)
        {
            count++;
        }
        shop.ItemCount = (byte)count;
        shop.Items = new ushort[shop.ItemCount];
        for (ushort i = 0; i < shop.ItemCount; i++)
        {
            shop.Items[i] = items[i];
        }
    }

    // OFFSET 0x0224CD88
    private static Shop Shop_Alloc()
    {
        return new Shop();
    }

    // OFFSET 0x0224CDA4
    public static void Shop_Start(FieldTask task, FieldSystem fieldSystem, ushort[] items, int martType, int isDepartmentStore)
    {
        Shop shop = Shop_Alloc();
        shop.BgConfig = fieldSystem.BgConfig!;
        shop.Text = GameString.String_Init(0x60, HeapID);
        shop.TrainerInfo = PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!);
        shop.Options = PlayerSave.SaveData_GetOptions(fieldSystem.SaveData!);
        shop.Records = GameRecords.SaveData_GetGameRecords(fieldSystem.SaveData!);
        shop.VarsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        shop.IsDepartmentStore = isDepartmentStore;
        shop.CameraMoveTarget = Shop_GetCameraPosDest(fieldSystem);
        shop.Journal = fieldSystem.JournalEntry;
        shop.MartType = (byte)martType;
        shop.SaveData = fieldSystem.SaveData;
        shop.FontSpecialChars = FontSpecialCharsContext.FontSpecialChars_Init(1, 2, 0, HeapID);
        if (shop.MartType == MartTypeNormal)
        {
            shop.Destination = BagItems.SaveData_GetBag(fieldSystem.SaveData!);
        }
        else if (shop.MartType == MartTypeFrontier)
        {
            shop.Destination = BagItems.SaveData_GetBag(fieldSystem.SaveData!);
        }
        else if (shop.MartType == MartTypeDecor)
        {
            shop.Destination = Underground.SaveData_GetUnderground(fieldSystem.SaveData!);
        }
        else
        {
            shop.Destination = SealCase.SaveData_GetSealCase(fieldSystem.SaveData!);
        }
        Shop_SetItemsForSale(shop, items);
        FieldTask.FieldTask_InitCall(task, FieldTask_InitShop, shop);
    }

    // OFFSET 0x0224CE90
    private static byte Shop_GetCameraPosDest(FieldSystem fieldSystem)
    {
        int dir = PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!);
        if (dir == DirNorth || dir == DirSouth || dir == DirEast)
        {
            return CameraMovesFacing;
        }
        return CameraMovesOther;
    }

    // OFFSET 0x0224CEAC
    private static bool FieldTask_InitShop(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        Shop shop = (Shop)FieldTask.FieldTask_GetEnv(task)!;
        switch (shop.State)
        {
            case StateInit:
                Shop_ShowContextMenu(shop);
                shop.State = StateSelectContextMenu;
                break;
            case StateSelectContextMenu:
                shop.State = Shop_SelectContextMenu(shop);
                break;
            case StateStartBuy:
                Shop_InitCamera(fieldSystem, shop);
                shop.State = StateMoveCamera;
                break;
            case StateMoveCamera:
                shop.State = Shop_MoveCamera(shop);
                break;
            case StateSelectBuyMenu:
                shop.State = Shop_SelectBuyMenu(shop);
                break;
            case StateShowPurchaseMenu:
                shop.State = Shop_ShowPurchaseMenu(shop);
                break;
            case StateSelectPurchaseMenu:
                shop.State = Shop_SelectPurchaseMenu(shop);
                break;
            case StateShowConfirmPurchase:
                shop.State = Shop_ShowConfirmPurchase(shop);
                break;
            case StateSelectConfirmPurchase:
                shop.State = Shop_SelectConfirmPurchase(shop);
                break;
            case StateConfirmItemPurchase:
                shop.State = Shop_ConfirmItemPurchase(shop);
                break;
            case StateFinishPurchase:
                shop.State = Shop_FinishPurchase(shop);
                break;
            case StateFinishFreePremierBall:
                shop.State = Shop_FinishFreePremierBall(shop);
                break;
            case StateMoveCameraBack:
                shop.State = Shop_MoveCameraBack(fieldSystem, shop);
                break;
            case StateReinitContextMenu:
                shop.State = Shop_ReinitContextMenu(shop);
                break;
            case StateStartSell:
                Shop_StartScreenFade(fieldSystem, shop);
                break;
            case StateFinishScreenTransition:
                Shop_FinishScreenTransition(task);
                break;
            case StateReinitMerchantMessage:
                shop.State = Shop_ReinitMerchantMessage(fieldSystem, shop);
                break;
            case StatePrintExit:
                Shop_PrintExit(fieldSystem, shop);
                shop.State = StateExit;
                break;
            case StateExit:
                return Shop_Exit(fieldSystem, shop);
        }
        if (shop.State >= StateSelectBuyMenu && shop.State <= StateFinishFreePremierBall)
        {
            Sprite.Sprite_UpdateAnim(shop.Sprites[SpriteUpArrow]!, 0x1000);
            Sprite.Sprite_UpdateAnim(shop.Sprites[SpriteDownArrow]!, 0x1000);
            Sprite.SpriteList_Update(shop.SpriteManager.SpriteList);
        }
        return false;
    }

    // OFFSET 0x0224D008
    private static void Shop_ShowContextMenu(Shop shop)
    {
        Shop_InitStringUtil(shop);
        Shop_InitContextMenu(shop);
    }

    // OFFSET 0x0224D018
    private static void Shop_InitStringUtil(Shop shop)
    {
        shop.Messages = MessageLoader.MessageLoader_Init(0, NarcMessages, MessageBankShop, HeapID);
        shop.Template = StringTemplate.StringTemplate_Default(HeapID);
    }

    // OFFSET 0x0224D040
    private static void Shop_InitContextMenu(Shop shop)
    {
        int count;
        if (shop.MartType == MartTypeNormal)
        {
            count = 3;
            shop.ContextList = StringList.StringList_New((uint)count, HeapID);
            StringList.StringList_AddFromMessageBank(shop.ContextList, shop.Messages!, 0xF, ChoiceBuy);
            StringList.StringList_AddFromMessageBank(shop.ContextList, shop.Messages!, 0x10, ChoiceSell);
            StringList.StringList_AddFromMessageBank(shop.ContextList, shop.Messages!, 0x11, Menu.Cancel);
            Bg.Window_Add(shop.BgConfig, shop.Windows[WindowItemList], (byte)count, 1, 1, 0xD, 6, 0xD, ContextWindowBaseTile);
        }
        else if (shop.MartType == MartTypeFrontier)
        {
            count = 2;
            shop.ContextList = StringList.StringList_New((uint)count, HeapID);
            StringList.StringList_AddFromMessageBank(shop.ContextList, shop.Messages!, 0x1D, ChoiceBuy);
            StringList.StringList_AddFromMessageBank(shop.ContextList, shop.Messages!, 0x1E, Menu.Cancel);
            Bg.Window_Add(shop.BgConfig, shop.Windows[WindowItemList], BgLayerMain3, 0x17, 0xD, 7, 4, 0xD, ContextWindowBaseTile);
        }
        else
        {
            count = 2;
            shop.ContextList = StringList.StringList_New((uint)count, HeapID);
            StringList.StringList_AddFromMessageBank(shop.ContextList, shop.Messages!, 0xF, ChoiceBuy);
            StringList.StringList_AddFromMessageBank(shop.ContextList, shop.Messages!, 0x11, Menu.Cancel);
            Bg.Window_Add(shop.BgConfig, shop.Windows[WindowItemList], BgLayerMain3, 1, 1, 0xD, 4, 0xD, ContextWindowBaseTile);
        }
        MenuTemplate template = new MenuTemplate
        {
            Choices = shop.ContextList,
            Window = shop.Windows[WindowItemList],
            FontID = 0,
            XSize = 1,
            YSize = (byte)count,
            LineSpacing = 0,
            SuppressCursor = 0,
            LoopAround = 0,
        };
        RenderWindow.LoadStandardWindowGraphics(shop.BgConfig, BgLayerMain3, ContextFrameTile, StandardFramePalette, 0, HeapID);
        RenderWindow.Window_DrawStandardFrame(shop.Windows[WindowItemList], 1, ContextFrameTile, StandardFramePalette);
        shop.Menu = Menu.Menu_NewAndCopyToVRAM(template, 8, 0, 0, HeapID, PadButtonB);
    }

    // OFFSET 0x0224D1EC
    private static byte Shop_SelectContextMenu(Shop shop)
    {
        uint input = Menu.Menu_ProcessInput(shop.Menu!);
        if (input == Menu.Cancel)
        {
            Shop_CloseContextMenu(shop);
            return StatePrintExit;
        }
        if (input == Menu.NoChoice)
        {
            return StateSelectContextMenu;
        }
        return (byte)input;
    }

    // OFFSET 0x0224D21C
    private static void Shop_CloseContextMenu(Shop shop)
    {
        RenderWindow.Window_EraseStandardFrame(shop.Windows[WindowItemList], 1);
        Bg.Window_ClearAndCopyToVRAM(shop.Windows[WindowItemList]);
        Menu.Menu_Free(shop.Menu!, out _);
        StringList.StringList_Free(shop.ContextList!);
        Bg.Window_Remove(shop.Windows[WindowItemList]);
    }

    // OFFSET 0x0224D250
    private static bool Shop_Exit(FieldSystem fieldSystem, Shop shop)
    {
        if (FieldMessage.FieldMessage_FinishedPrinting(shop.PrinterID) == 0)
        {
            return false;
        }
        if ((gSystem.PressedKeys & (PadButtonA | PadButtonB)) == 0)
        {
            return false;
        }
        RenderWindow.Window_EraseMessageBox(shop.Windows[WindowItemDescription], 0);
        Bg.Window_Remove(shop.Windows[WindowItemDescription]);
        MessageLoader.MessageLoader_Free(shop.Messages);
        StringTemplate.StringTemplate_Free(shop.Template!);
        GameString.String_Free(shop.Text);
        int map = (int)fieldSystem.Location!.U32(0);
        if (shop.MartType == MartTypeNormal && MapHeader.MapHeader_GetMapLabelTextID(map) != MapLabelVeilstoneStore && map != MapVeilstoneStore && map != MapFrontierShop)
        {
            JournalLocationEvent? journalEvent = null;
            if (shop.ItemsBought != 0 && shop.ItemsSold != 0)
            {
                journalEvent = Journal.JournalEntry_CreateEventBusinessAtMart(HeapID);
            }
            else if (shop.ItemsBought > 1)
            {
                journalEvent = Journal.JournalEntry_CreateEventLotsOfShopping(HeapID);
            }
            else if (shop.ItemsSold > 1)
            {
                journalEvent = Journal.JournalEntry_CreateEventSoldALot(HeapID);
            }
            else if (shop.ItemsBought != 0)
            {
                journalEvent = Journal.JournalEntry_CreateEventShoppedAtMart(HeapID);
            }
            else if (shop.ItemsSold != 0)
            {
                journalEvent = Journal.JournalEntry_CreateEventSoldALittle(HeapID);
            }
            if (journalEvent != null)
            {
                Journal.JournalEntry_SaveData(shop.Journal, journalEvent, JournalDataLocation);
            }
        }
        FontSpecialCharsContext.FontSpecialChars_Free(shop.FontSpecialChars);
        return true;
    }

    // OFFSET 0x0224D388
    private static void Shop_InitCamera(FieldSystem fieldSystem, Shop shop)
    {
        Bg.Bg_FillTilemapRect(shop.BgConfig, BgLayerMain3, 0, 0, 0x12, 0x20, 6, 0);
        Shop_CloseContextMenu(shop);
        Shop_LoadWindows(shop);
        shop.Camera = Camera.Camera_Alloc(HeapID);
        Camera.Camera_Copy(fieldSystem.Camera!, shop.Camera);
        Camera.Camera_SetAsActive(shop.Camera);
        shop.CameraMoveCount = 0;
    }

    // OFFSET 0x0224D3E8
    private static void Shop_LoadWindows(Shop shop)
    {
        for (int i = 0; i < Shop.WindowCount; i++)
        {
            if (shop.MartType != MartTypeNormal && shop.MartType != MartTypeFrontier && i == WindowItemDescription)
            {
                Bg.Window_AddFromTemplate(shop.BgConfig, shop.Windows[i], WindowTemplateAt(WindowTemplateDecorDescription));
            }
            else if (shop.MartType == MartTypeFrontier && i == WindowCurrentMoney)
            {
                Bg.Window_AddFromTemplate(shop.BgConfig, shop.Windows[i], WindowTemplateAt(WindowTemplateFrontierMoney));
            }
            else
            {
                Bg.Window_AddFromTemplate(shop.BgConfig, shop.Windows[i], WindowTemplateAt((uint)(WindowTemplates + i * 8)));
            }
        }
    }

    // OFFSET 0x0224D44C
    private static void Shop_DestroyStaticWindows(Shop shop)
    {
        RenderWindow.Window_EraseStandardFrame(shop.Windows[WindowCurrentMoney], 1);
        for (int i = 0; i < Shop.WindowCount; i++)
        {
            Bg.Window_ClearAndCopyToVRAM(shop.Windows[i]);
            Bg.Window_Remove(shop.Windows[i]);
        }
    }

    // OFFSET 0x0224D474
    private static void Shop_LoadGraphics(Shop shop)
    {
        NARC narc = NARC.NARC_ctor(NarcShopGraphics, HeapID);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, 0, shop.BgConfig, BgLayerMain1, 0, 0, 0, HeapID);
        if (UsesBag(shop))
        {
            Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 2, shop.BgConfig, BgLayerMain1, 0, 0, 0, HeapID);
        }
        else
        {
            Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, 3, shop.BgConfig, BgLayerMain1, 0, 0, 0, HeapID);
        }
        if (shop.MartType == MartTypeFrontier)
        {
            Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 0, 0, 0, 0x20, HeapID);
        }
        else
        {
            Graphics.Graphics_LoadPaletteFromOpenNARC(narc, 1, 0, 0, 0x20, HeapID);
        }
        RenderWindow.LoadStandardWindowGraphics(shop.BgConfig, BgLayerMain3, StandardFrameTile, StandardFramePalette, 0, HeapID);
        RenderWindow.LoadMessageBoxGraphics(shop.BgConfig, BgLayerMain3, 1, MessageBoxPalette, (byte)Options.Options_Frame(shop.Options!), HeapID);
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x0224D548
    private static void Shop_SetBgPriorities(Shop shop)
    {
        shop.SavedLayers = GXLayers.GXLayers_EngineAGetLayers();
        for (int layer = 0; layer < 4; layer++)
        {
            shop.BgPriorities[layer] = Bg.Bg_GetPriority(shop.BgConfig, layer);
        }
        Bg.Bg_SetPriority(0, 3);
        Bg.Bg_SetPriority(1, 2);
        Bg.Bg_SetPriority(2, 1);
        Bg.Bg_SetPriority(3, 0);
        GXLayers.GXLayers_EngineAToggleLayers(1, 1);
        GXLayers.GXLayers_EngineAToggleLayers(2, 1);
        GXLayers.GXLayers_EngineAToggleLayers(4, 1);
        GXLayers.GXLayers_EngineAToggleLayers(8, 1);
    }

    // OFFSET 0x0224D5D8
    private static void Shop_RestoreBgPriorities(Shop shop)
    {
        for (byte layer = 0; layer < 4; layer++)
        {
            Bg.Bg_SetPriority(layer, shop.BgPriorities[layer]);
        }
        GXLayers.GXLayers_EngineASetLayers(shop.SavedLayers);
    }

    // OFFSET 0x0224D620
    private static byte Shop_MoveCamera(Shop shop)
    {
        if (shop.CameraMoveCount != shop.CameraMoveTarget)
        {
            Camera.Camera_Move(VectorAt(CameraMoveForward), shop.Camera!);
            shop.CameraMoveCount++;
            return StateMoveCamera;
        }
        shop.CameraMoveCount = 0;
        Video.ClearBgVram(0);
        Shop_DrawSprites(shop);
        Shop_LoadGraphics(shop);
        Shop_SetBgPriorities(shop);
        Shop_InitItemsList(shop);
        Shop_PrintCurrentMoney(shop, 0);
        return StateSelectBuyMenu;
    }

    // OFFSET 0x0224D698
    private static ushort Shop_GetItemId(Shop shop, ushort item)
    {
        if (shop.MartType == MartTypeDecor)
        {
            return item;
        }
        if (shop.MartType == MartTypeSeal)
        {
            return (ushort)SealShopData.SealShopEntry_GetSealID((byte)item);
        }
        return item;
    }

    private static bool IsTMHM(int item)
    {
        return item >= FirstTMHM && item <= LastTMHM;
    }

    // OFFSET 0x0224D6BC
    private static void Shop_InitItemsList(Shop shop)
    {
        bool hasTM = false;
        MessageLoader names;
        MessageLoader? moveNames = null;
        if (UsesBag(shop))
        {
            names = MessageLoader.MessageLoader_Init(0, NarcMessages, MessageBankItemNames, HeapID)!;
            moveNames = MessageLoader.MessageLoader_Init(0, NarcMessages, MessageBankMoveNames, HeapID)!;
        }
        else if (shop.MartType == MartTypeDecor)
        {
            names = MessageLoader.MessageLoader_Init(0, NarcMessages, MessageBankGoodsNames, HeapID)!;
        }
        else
        {
            names = MessageLoader.MessageLoader_Init(0, NarcMessages, MessageBankSealNames, HeapID)!;
        }
        shop.ItemList = StringList.StringList_New((uint)(shop.ItemCount + 1), HeapID);
        ushort itemID = 0;
        for (int i = 0; i < shop.ItemCount; i++)
        {
            itemID = Shop_GetItemId(shop, shop.Items[i]);
            GameString name;
            if (IsTMHM(itemID))
            {
                name = MessageLoader.MessageLoader_GetNewString(moveNames!, Item.Item_MoveForTMHM(itemID))!;
                hasTM = true;
            }
            else
            {
                name = MessageLoader.MessageLoader_GetNewString(names, itemID)!;
            }
            StringList.StringList_AddFromString(shop.ItemList, name, shop.Items[i]);
            GameString.String_Free(name);
        }
        if (hasTM)
        {
            StringList.StringList_AddFromMessageBank(shop.ItemList, shop.Messages!, 0x1A, ListMenu.Cancel);
        }
        else
        {
            StringList.StringList_AddFromMessageBank(shop.ItemList, shop.Messages!, 8, ListMenu.Cancel);
        }
        MessageLoader.MessageLoader_Free(names);
        if (UsesBag(shop))
        {
            MessageLoader.MessageLoader_Free(moveNames);
        }
        ListMenuTemplate template = ItemListTemplateFromRom();
        template.TextXOffset = (byte)(IsTMHM(itemID) ? TMTextXOffset : 0);
        template.Choices = shop.ItemList;
        template.Window = shop.Windows[WindowItemList];
        template.Count = (ushort)(shop.ItemCount + 1);
        template.Parent = shop;
        shop.ItemListMenu = ListMenu.ListMenu_New(template, 0, 0, HeapID);
    }

    private static ListMenuTemplate ItemListTemplateFromRom()
    {
        ListMenuTemplate template = ListMenuTemplate.FromBytes(RomData.Overlay(OverlayID, ItemListTemplate, 0x20));
        template.CursorCallback = Shop_MenuCursorCallback;
        template.PrintCallback = Shop_MenuPrintCallback;
        return template;
    }

    // OFFSET 0x0224D85C
    private static void Shop_MenuCursorCallback(ListMenu menu, uint index, byte onInit)
    {
        Shop shop = (Shop)ListMenu.ListMenu_GetAttribute(menu, 0x13)!;
        if (onInit != 1)
        {
            SoundPlayback.Sound_PlayEffect(SeqSeCursor);
        }
        Bg.Window_FillTilemap(shop.Windows[WindowItemDescription], 0);
        if (index != ListMenu.Cancel)
        {
            GameString description;
            if (UsesBag(shop))
            {
                description = GameString.String_Init(0x82, HeapID);
                Item.Item_LoadDescription(description, (ushort)index, HeapID);
            }
            else if (shop.MartType == MartTypeDecor)
            {
                MessageLoader loader = MessageLoader.MessageLoader_Init(0, NarcMessages, MessageBankGoodsNames, HeapID)!;
                description = MessageLoader.MessageLoader_GetNewString(loader, index + GoodsDescriptionOffset)!;
                MessageLoader.MessageLoader_Free(loader);
            }
            else
            {
                MessageLoader loader = MessageLoader.MessageLoader_Init(0, NarcMessages, MessageBankSealDescriptions, HeapID)!;
                description = MessageLoader.MessageLoader_GetNewString(loader, (uint)SealShopData.SealShopEntry_GetDescriptionID((byte)index))!;
                MessageLoader.MessageLoader_Free(loader);
            }
            Text.Text_AddPrinterWithParamsAndColor(shop.Windows[WindowItemDescription], 0, description, 0, 0, 0xFF, ColorItemDescription, null);
            GameString.String_Free(description);
            Shop_ChangeItemIconGfx(shop, (ushort)index);
        }
        else
        {
            Shop_ChangeItemIconGfx(shop, (ushort)(index >> 16));
        }
        ListMenu.ListMenu_GetListAndCursorPos(menu, out ushort listPos, out _);
        Sprite.Sprite_SetDrawFlag(shop.Sprites[SpriteUpArrow]!, listPos == 0 ? 0 : 1);
        uint count = (uint)ListMenu.ListMenu_GetAttribute(menu, 2)!;
        Sprite.Sprite_SetDrawFlag(shop.Sprites[SpriteDownArrow]!, count > 7 && count > (uint)(listPos + 7) ? 1 : 0);
        Bg.Window_ScheduleCopyToVRAM(shop.Windows[WindowItemDescription]);
    }

    // OFFSET 0x0224D9B8
    private static void Shop_MenuPrintCallback(ListMenu menu, uint index, byte y)
    {
        Shop shop = (Shop)ListMenu.ListMenu_GetAttribute(menu, 0x13)!;
        if (index != ListMenu.Cancel)
        {
            ushort item = (ushort)index;
            if (IsTMHM(item))
            {
                FontSpecialCharsContext.FontSpecialChars_DrawPartyScreenText(shop.FontSpecialChars!, 2, item - FirstTMMinusOne, 2, 2, shop.Windows[WindowItemList], 0, (uint)(y + 4));
            }
            uint price = Shop_GetItemPrice(shop, item);
            GameString text = GameString.String_Init(0xC, HeapID);
            GameString format = MessageLoader.MessageLoader_GetNewString(shop.Messages!, shop.MartType == MartTypeFrontier ? 0x20u : 9u)!;
            StringTemplate.StringTemplate_SetNumber(shop.Template!, 0, (int)price, 4, 1, 1);
            StringTemplate.StringTemplate_Format(shop.Template!, text, format);
            int width = Font.Font_CalcStringWidth(0, text, 0);
            Text.Text_AddPrinterWithParamsAndColor(shop.Windows[WindowItemList], 0, text, (uint)(0x98 - width), y, 0xFF, ColorItemList, null);
            GameString.String_Free(format);
            GameString.String_Free(text);
        }
        if (index == ListMenu.Cancel)
        {
            GameString text = MessageLoader.MessageLoader_GetNewString(shop.Messages!, 8)!;
            Bg.Window_FillRectWithColor(shop.Windows[WindowItemList], 0xF, 0, y, 0x98, 0x10);
            Text.Text_AddPrinterWithParamsAndColor(shop.Windows[WindowItemList], 0, text, 0, y, 0xFF, ColorItemList, null);
            GameString.String_Free(text);
        }
    }

    // OFFSET 0x0224DAF8
    private static void Shop_PrintCurrentMoney(Shop shop, int update)
    {
        Window window = shop.Windows[WindowCurrentMoney];
        GameString text;
        GameString format;
        if (shop.MartType == MartTypeFrontier)
        {
            if (update == 0)
            {
                Bg.Window_FillTilemap(window, 0xF);
                RenderWindow.Window_DrawStandardFrame(window, 1, StandardFrameTile, StandardFramePalette);
            }
            else
            {
                Bg.Window_FillRectWithColor(window, 0xF, 0, 0, 0x48, 0x10);
            }
            text = GameString.String_Init(0x10, HeapID);
            format = MessageLoader.MessageLoader_GetNewString(shop.Messages!, 0x26)!;
            StringTemplate.StringTemplate_SetNumber(shop.Template!, 0, (int)Shop_GetCurrentMoney(shop), 6, 1, 1);
            StringTemplate.StringTemplate_Format(shop.Template!, text, format);
            int width = Font.Font_CalcStringWidth(0, text, 0);
            Text.Text_AddPrinterWithParams(window, 0, text, 0x40 - width, 0, 0xFF, null);
        }
        else
        {
            if (update == 0)
            {
                Bg.Window_FillTilemap(window, 0xF);
                RenderWindow.Window_DrawStandardFrame(window, 1, StandardFrameTile, StandardFramePalette);
                GameString label = MessageLoader.MessageLoader_GetNewString(shop.Messages!, 0x12)!;
                Text.Text_AddPrinterWithParams(window, 0, label, 0, 0, 0xFF, null);
                GameString.String_Free(label);
            }
            else
            {
                Bg.Window_FillRectWithColor(window, 0xF, 0, 0x10, 0x48, 0x10);
            }
            text = GameString.String_Init(0x10, HeapID);
            format = MessageLoader.MessageLoader_GetNewString(shop.Messages!, 0x13)!;
            StringTemplate.StringTemplate_SetNumber(shop.Template!, 0, (int)Shop_GetCurrentMoney(shop), 6, 1, 1);
            StringTemplate.StringTemplate_Format(shop.Template!, text, format);
            int width = Font.Font_CalcStringWidth(0, text, 0);
            Text.Text_AddPrinterWithParams(window, 0, text, 0x48 - width, 0x10, 0xFF, null);
        }
        GameString.String_Free(format);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x0224DC84
    private static byte Shop_SelectBuyMenu(Shop shop)
    {
        ListMenu.ListMenu_GetListAndCursorPos(shop.ItemListMenu!, out _, out ushort previousCursor);
        uint input = ListMenu.ListMenu_ProcessInput(shop.ItemListMenu!);
        ListMenu.ListMenu_GetListAndCursorPos(shop.ItemListMenu!, out _, out ushort cursor);
        if (previousCursor != cursor)
        {
            SpriteSystem.Sprite_SetPositionXY(shop.Sprites[SpriteCursor]!, 0xAC, (short)(cursor * 16 + 0x18));
        }
        if (input == ListMenu.Cancel)
        {
            ListMenu.ListMenu_Free(shop.ItemListMenu!, out _, out _);
            StringList.StringList_Free(shop.ItemList!);
            Shop_DestroyStaticWindows(shop);
            Shop_RestoreBgPriorities(shop);
            Bg.Bg_ClearTilemap(shop.BgConfig, BgLayerMain1);
            SoundPlayback.Sound_PlayEffect(SeqSeSelect);
            return StateMoveCameraBack;
        }
        if (input == ListMenu.NoChoice)
        {
            return StateSelectBuyMenu;
        }
        shop.UpArrowDrawFlag = (ushort)Sprite.Sprite_GetDrawFlag(shop.Sprites[SpriteUpArrow]!);
        shop.DownArrowDrawFlag = (ushort)Sprite.Sprite_GetDrawFlag(shop.Sprites[SpriteDownArrow]!);
        Sprite.Sprite_SetDrawFlag(shop.Sprites[SpriteUpArrow]!, 0);
        Sprite.Sprite_SetDrawFlag(shop.Sprites[SpriteDownArrow]!, 0);
        Shop_SetCursorSpritePalette(shop, 1);
        Bg.Window_FillTilemap(shop.Windows[WindowMessage], 0xF);
        Bg.Window_ClearAndCopyToVRAM(shop.Windows[WindowItemDescription]);
        RenderWindow.Window_DrawMessageBoxWithScrollCursor(shop.Windows[WindowMessage], 0, 1, MessageBoxPalette);
        shop.SelectedItem = (ushort)input;
        shop.Quantity = 1;
        shop.Price = Shop_GetItemPrice(shop, shop.SelectedItem);
        uint money = Shop_GetCurrentMoney(shop);
        if (money < shop.Price)
        {
            PrintMessage(shop, shop.MartType == MartTypeFrontier ? 0x25 : 3, shop.Windows[WindowMessage]);
            return StateFinishPurchase;
        }
        if (shop.MartType == MartTypeDecor)
        {
            SoundPlayback.Sound_PlayEffect(SeqSeSelect);
            return Shop_ShowPurchaseMessage(shop);
        }
        shop.MaxQuantity = (ushort)(money / shop.Price);
        if (shop.MaxQuantity > MaxQuantityCap)
        {
            shop.MaxQuantity = MaxQuantityCap;
        }
        Shop_SetItemNameToIndex(shop, shop.SelectedItem, 0);
        PrintMessage(shop, shop.MartType == MartTypeFrontier ? 0x21 : 4, shop.Windows[WindowMessage]);
        Shop_SetScrollSpritesPositionXY(shop, 1);
        SoundPlayback.Sound_PlayEffect(SeqSeSelect);
        return StateShowPurchaseMenu;
    }

    // OFFSET 0x0224DE94
    private static byte Shop_ShowPurchaseMenu(Shop shop)
    {
        if (FieldMessage.FieldMessage_FinishedPrinting(shop.PrinterID) == 0)
        {
            return StateShowPurchaseMenu;
        }
        Shop_ShowQtyWithinInventory(shop);
        Shop_ShowQtyTotalItemPurchase(shop, 0);
        Sprite.Sprite_SetDrawFlag(shop.Sprites[SpriteUpArrow]!, 1);
        Sprite.Sprite_SetDrawFlag(shop.Sprites[SpriteDownArrow]!, 1);
        return StateSelectPurchaseMenu;
    }

    // OFFSET 0x0224DED4
    private static void Shop_ShowQtyWithinInventory(Shop shop)
    {
        int count;
        if (UsesBag(shop))
        {
            count = BagItems.Bag_GetItemQuantity((Bag)shop.Destination!, shop.SelectedItem, HeapID);
        }
        else if (shop.MartType == MartTypeSeal)
        {
            count = (ushort)SealCase.SealCase_CountSealOccurrenceAnywhere((SealCase)shop.Destination!, shop.SelectedItem);
        }
        else
        {
            count = 0;
        }
        Window window = shop.Windows[WindowItemsInBag];
        Bg.Window_FillTilemap(window, 0xF);
        RenderWindow.Window_DrawStandardFrame(window, 1, StandardFrameTile, StandardFramePalette);
        GameString text = GameString.String_Init(0x18, HeapID);
        GameString format = MessageLoader.MessageLoader_GetNewString(shop.Messages!, 0x14)!;
        StringTemplate.StringTemplate_SetNumber(shop.Template!, 0, count, 3, 1, 1);
        StringTemplate.StringTemplate_Format(shop.Template!, text, format);
        Text.Text_AddPrinterWithParams(window, 0, text, 0, 0, 0xFF, null);
        GameString.String_Free(format);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x0224DFB0
    private static byte Shop_SelectPurchaseMenu(Shop shop)
    {
        if (HealthBar.App_AdjustItemCountWithInput(ref shop.Quantity, shop.MaxQuantity) != 0)
        {
            SoundPlayback.Sound_PlayEffect(SeqSeCursor);
            Shop_ShowQtyTotalItemPurchase(shop, 1);
            return StateSelectPurchaseMenu;
        }
        uint pressed = (uint)gSystem.PressedKeys;
        if ((pressed & PadButtonA) != 0)
        {
            Sprite.Sprite_SetDrawFlag(shop.Sprites[SpriteUpArrow]!, 0);
            Sprite.Sprite_SetDrawFlag(shop.Sprites[SpriteDownArrow]!, 0);
            RenderWindow.Window_EraseStandardFrame(shop.Windows[WindowItemsInBag], 1);
            RenderWindow.Window_EraseStandardFrame(shop.Windows[WindowQuantityTotal], 1);
            Bg.Window_FillTilemap(shop.Windows[WindowMessage], 0xF);
            SoundPlayback.Sound_PlayEffect(SeqSeSelect);
            return Shop_ShowPurchaseMessage(shop);
        }
        if ((pressed & PadButtonB) != 0)
        {
            RenderWindow.Window_EraseStandardFrame(shop.Windows[WindowItemsInBag], 1);
            RenderWindow.Window_EraseStandardFrame(shop.Windows[WindowQuantityTotal], 1);
            RestoreBuyMenu(shop);
            SoundPlayback.Sound_PlayEffect(SeqSeSelect);
            return StateSelectBuyMenu;
        }
        return StateSelectPurchaseMenu;
    }

    // OFFSET 0x0224E098
    private static byte Shop_ShowPurchaseMessage(Shop shop)
    {
        bool canFit;
        if (UsesBag(shop))
        {
            canFit = BagItems.Bag_CanFitItem((Bag)shop.Destination!, shop.SelectedItem, (ushort)shop.Quantity, HeapID);
        }
        else if (shop.MartType == MartTypeDecor)
        {
            canFit = Underground.Underground_GetGoodsCountPC((Underground)shop.Destination!) != GoodsPCCapacity;
        }
        else
        {
            canFit = SealCase.SealCase_CheckSealCount((SealCase)shop.Destination!, shop.SelectedItem, shop.Quantity);
        }
        if (!canFit)
        {
            shop.Quantity = 0;
            int message = UsesBag(shop) ? 7 : shop.MartType == MartTypeDecor ? 0xC : 0xE;
            PrintMessage(shop, message, shop.Windows[WindowMessage]);
            return StateFinishPurchase;
        }
        Shop_SetItemNameToIndex(shop, shop.SelectedItem, 0);
        StringTemplate.StringTemplate_SetNumber(shop.Template!, 1, shop.Quantity, 2, 0, 1);
        StringTemplate.StringTemplate_SetNumber(shop.Template!, 2, (int)(shop.Quantity * shop.Price), 6, 0, 1);
        int purchaseMessage;
        if (IsTMHM(shop.SelectedItem))
        {
            StringTemplate.StringTemplate_SetMoveName(shop.Template!, 3, Item.Item_MoveForTMHM(shop.SelectedItem));
            purchaseMessage = shop.MartType == MartTypeFrontier ? 0x24 : 0x1B;
        }
        else
        {
            purchaseMessage = shop.MartType == MartTypeFrontier ? 0x23 : 5;
        }
        PrintMessage(shop, purchaseMessage, shop.Windows[WindowMessage]);
        return StateShowConfirmPurchase;
    }

    // OFFSET 0x0224E28C
    private static void Shop_ShowQtyTotalItemPurchase(Shop shop, int update)
    {
        Window window = shop.Windows[WindowQuantityTotal];
        Bg.Window_FillTilemap(window, 0xF);
        if (update == 0)
        {
            RenderWindow.Window_DrawStandardFrame(window, 1, StandardFrameTile, StandardFramePalette);
        }
        GameString text = GameString.String_Init(0x18, HeapID);
        GameString format = MessageLoader.MessageLoader_GetNewString(shop.Messages!, 0x15)!;
        StringTemplate.StringTemplate_SetNumber(shop.Template!, 0, shop.Quantity, 2, 2, 1);
        StringTemplate.StringTemplate_Format(shop.Template!, text, format);
        Text.Text_AddPrinterWithParams(window, 0, text, 0, 8, 0xFF, null);
        GameString.String_Free(format);
        format = MessageLoader.MessageLoader_GetNewString(shop.Messages!, shop.MartType == MartTypeFrontier ? 0x22u : 0x16u)!;
        StringTemplate.StringTemplate_SetNumber(shop.Template!, 0, (int)(shop.Quantity * shop.Price), 6, 1, 1);
        StringTemplate.StringTemplate_Format(shop.Template!, text, format);
        int width = Font.Font_CalcStringWidth(0, text, 0);
        Text.Text_AddPrinterWithParams(window, 0, text, 0x60 - width, 8, 0xFF, null);
        GameString.String_Free(format);
        GameString.String_Free(text);
        Bg.Window_ScheduleCopyToVRAM(window);
    }

    // OFFSET 0x0224E3A0
    private static byte Shop_ShowConfirmPurchase(Shop shop)
    {
        if (FieldMessage.FieldMessage_FinishedPrinting(shop.PrinterID) == 0)
        {
            return StateShowConfirmPurchase;
        }
        shop.Menu = Menu.Menu_MakeYesNoChoice(shop.BgConfig, WindowTemplateAt(WindowTemplateYesNo), StandardFrameTile, StandardFramePalette, HeapID);
        return StateSelectConfirmPurchase;
    }

    // OFFSET 0x0224E3D8
    private static byte Shop_SelectConfirmPurchase(Shop shop)
    {
        uint input = Menu.Menu_ProcessInputAndHandleExit(shop.Menu!, HeapID);
        if (input == 0)
        {
            int message;
            if (UsesBag(shop))
            {
                if (shop.Quantity == 1)
                {
                    StringTemplate.StringTemplate_SetItemName(shop.Template!, 0, shop.SelectedItem);
                }
                else
                {
                    StringTemplate.StringTemplate_SetItemNamePlural(shop.Template!, 0, shop.SelectedItem);
                }
                message = 6;
                StringTemplate.StringTemplate_SetBagPocketName(shop.Template!, 1, (uint)Item.Item_LoadParam(shop.SelectedItem, 5, HeapID));
            }
            else if (shop.MartType == MartTypeDecor)
            {
                Shop_SetItemNameToIndex(shop, shop.SelectedItem, 0);
                message = 0xB;
            }
            else
            {
                if (shop.Quantity == 1)
                {
                    StringTemplate.StringTemplate_SetBallSealName(shop.Template!, 0, (uint)SealShopData.SealShopEntry_GetSealID((byte)shop.SelectedItem));
                }
                else
                {
                    StringTemplate.StringTemplate_SetBallSealNamePlural(shop.Template!, 0, (uint)SealShopData.SealShopEntry_GetSealID((byte)shop.SelectedItem));
                }
                message = 0xD;
            }
            GameString format = MessageLoader.MessageLoader_GetNewString(shop.Messages!, (uint)message)!;
            StringTemplate.StringTemplate_Format(shop.Template!, shop.Text!, format);
            GameString.String_Free(format);
            Bg.Window_FillTilemap(shop.Windows[WindowMessage], 0xF);
            shop.PrinterID = FieldMessage.FieldMessage_Print(shop.Windows[WindowMessage], shop.Text!, shop.Options!, 1);
            SoundPlayback.Sound_PlayEffect(SeqSePurchase);
            return StateConfirmItemPurchase;
        }
        if (input == Menu.Cancel)
        {
            RestoreBuyMenu(shop);
            return StateSelectBuyMenu;
        }
        return StateSelectConfirmPurchase;
    }

    // OFFSET 0x0224E5B0
    private static byte Shop_ConfirmItemPurchase(Shop shop)
    {
        if (FieldMessage.FieldMessage_FinishedPrinting(shop.PrinterID) == 0)
        {
            return StateConfirmItemPurchase;
        }
        if (shop.MartType == MartTypeNormal)
        {
            BagItems.Bag_TryAddItem((Bag)shop.Destination!, shop.SelectedItem, (ushort)shop.Quantity, HeapID);
            TVSegments.TVSegment_SaveBoughtItems(shop.SaveData!, shop.SelectedItem, (byte)shop.Quantity);
        }
        else if (shop.MartType == MartTypeFrontier)
        {
            BagItems.Bag_TryAddItem((Bag)shop.Destination!, shop.SelectedItem, (ushort)shop.Quantity, HeapID);
        }
        else if (shop.MartType == MartTypeDecor)
        {
            Underground.Underground_TryAddGoodPC((Underground)shop.Destination!, shop.SelectedItem);
        }
        else
        {
            SealCase.GiveOrTakeSeal((SealCase)shop.Destination!, shop.SelectedItem, shop.Quantity);
        }
        Shop_TakeMoney(shop, (uint)(shop.Quantity * shop.Price));
        GameRecords.GameRecords_AddToRecordValue(shop.Records!, shop.MartType == MartTypeFrontier ? RecordBattlePointsSpent : RecordMoneySpent, (uint)(shop.Quantity * shop.Price));
        Shop_PrintCurrentMoney(shop, 1);
        if (shop.Quantity > 1)
        {
            shop.ItemsBought = 2;
        }
        else if (shop.ItemsBought != 0xFF)
        {
            shop.ItemsBought++;
        }
        return StateFinishPurchase;
    }

    // OFFSET 0x0224E6B8
    private static byte Shop_FinishPurchase(Shop shop)
    {
        if (FieldMessage.FieldMessage_FinishedPrinting(shop.PrinterID) == 0)
        {
            return StateFinishPurchase;
        }
        if ((gSystem.PressedKeys & (PadButtonA | PadButtonB)) == 0)
        {
            return StateFinishPurchase;
        }
        if (shop.IsDepartmentStore == 1)
        {
            SystemVars.SystemVars_IncrementDepartmentStoreBuyCount(shop.VarsFlags!);
        }
        if (UsesBag(shop) && shop.SelectedItem == ItemPokeBall && shop.Quantity >= PremierBallThreshold
            && BagItems.Bag_TryAddItem((Bag)shop.Destination!, ItemPremierBall, 1, HeapID))
        {
            GameString format = MessageLoader.MessageLoader_GetNewString(shop.Messages!, 0xA)!;
            StringTemplate.StringTemplate_Format(shop.Template!, shop.Text!, format);
            GameString.String_Free(format);
            Bg.Window_FillTilemap(shop.Windows[WindowMessage], 0xF);
            shop.PrinterID = FieldMessage.FieldMessage_Print(shop.Windows[WindowMessage], shop.Text!, shop.Options!, 1);
            GameRecords.GameRecords_IncrementRecordValue(GameRecords.SaveData_GetGameRecords(shop.SaveData!), RecordPremierBalls);
            return StateFinishFreePremierBall;
        }
        RestoreBuyMenu(shop);
        return StateSelectBuyMenu;
    }

    // OFFSET 0x0224E7C8
    private static byte Shop_FinishFreePremierBall(Shop shop)
    {
        if (FieldMessage.FieldMessage_FinishedPrinting(shop.PrinterID) == 0)
        {
            return StateFinishFreePremierBall;
        }
        if ((gSystem.PressedKeys & (PadButtonA | PadButtonB)) == 0)
        {
            return StateFinishFreePremierBall;
        }
        RestoreBuyMenu(shop);
        return StateSelectBuyMenu;
    }

    // OFFSET 0x0224E834
    private static void Shop_SetItemNameToIndex(Shop shop, ushort item, uint idx)
    {
        if (UsesBag(shop))
        {
            StringTemplate.StringTemplate_SetItemName(shop.Template!, idx, item);
        }
        else if (shop.MartType == MartTypeDecor)
        {
            StringTemplate.StringTemplate_SetUndergroundGoodsName(shop.Template!, idx, item);
        }
        else
        {
            StringTemplate.StringTemplate_SetBallSealName(shop.Template!, idx, (uint)SealShopData.SealShopEntry_GetSealID((byte)item));
        }
    }

    // OFFSET 0x0224E890
    private static uint Shop_GetItemPrice(Shop shop, ushort item)
    {
        if (shop.MartType == MartTypeNormal)
        {
            return (uint)Item.Item_LoadParam(item, 0, HeapID);
        }
        if (shop.MartType == MartTypeFrontier)
        {
            return Shop_GetItemBPPrice(item);
        }
        if (shop.MartType == MartTypeDecor)
        {
            return (uint)UndergroundGoods.UndergroundGoods_GetPrice(item);
        }
        return (uint)SealShopData.SealShopEntry_GetPrice((byte)item);
    }

    // OFFSET 0x0224E8CC
    private static uint Shop_GetItemBPPrice(ushort item)
    {
        ushort[] prices = RomData.OverlayU16(OverlayID, FrontierPrices, FrontierPriceCount * 2);
        for (int i = 0; i < FrontierPriceCount; i++)
        {
            if (item == prices[i * 2])
            {
                return prices[i * 2 + 1];
            }
        }
        return 0;
    }

    // OFFSET 0x0224E8F4
    private static uint Shop_GetCurrentMoney(Shop shop)
    {
        if (shop.MartType == MartTypeFrontier)
        {
            return BattleTowerRecords.BattlePoints_ApplyFuncAndGet(BattleTowerRecords.SaveData_GetBattleTowerRecords(shop.SaveData!), 0, 0);
        }
        return TrainerInfo.TrainerInfo_Money(shop.TrainerInfo!);
    }

    // OFFSET 0x0224E920
    private static void Shop_TakeMoney(Shop shop, uint amount)
    {
        if (shop.MartType == MartTypeFrontier)
        {
            BattleTowerRecords.BattlePoints_ApplyFuncAndGet(BattleTowerRecords.SaveData_GetBattleTowerRecords(shop.SaveData!), (ushort)amount, 6);
            return;
        }
        TrainerInfo.TrainerInfo_TakeMoney(shop.TrainerInfo!, amount);
    }

    // OFFSET 0x0224E950
    private static byte Shop_MoveCameraBack(FieldSystem fieldSystem, Shop shop)
    {
        if (shop.CameraMoveCount != shop.CameraMoveTarget)
        {
            Camera.Camera_Move(VectorAt(CameraMoveBack), shop.Camera!);
            shop.CameraMoveCount++;
            return StateMoveCameraBack;
        }
        shop.CameraMoveCount = 0;
        Camera.Camera_Copy(shop.Camera!, fieldSystem.Camera!);
        Camera.Camera_Delete(shop.Camera!);
        Camera.Camera_SetAsActive(fieldSystem.Camera!);
        Shop_DestroySprites(shop);
        if (shop.MartType == MartTypeFrontier)
        {
            return StatePrintExit;
        }
        FieldMessage.FieldMessage_AddWindow(fieldSystem.BgConfig!, shop.Windows[WindowItemDescription], BgLayerMain3);
        FieldMessage.FieldMessage_DrawWindow(shop.Windows[WindowItemDescription], shop.Options!);
        PrintMessage(shop, 2, shop.Windows[WindowItemDescription]);
        return StateReinitContextMenu;
    }

    // OFFSET 0x0224EA2C
    private static byte Shop_ReinitContextMenu(Shop shop)
    {
        if (FieldMessage.FieldMessage_FinishedPrinting(shop.PrinterID) == 0)
        {
            return StateReinitContextMenu;
        }
        Bg.Window_Remove(shop.Windows[WindowItemDescription]);
        Shop_InitContextMenu(shop);
        return StateSelectContextMenu;
    }

    // OFFSET 0x0224EA54
    private static void Shop_PrintExit(FieldSystem fieldSystem, Shop shop)
    {
        GameString format = MessageLoader.MessageLoader_GetNewString(shop.Messages!, shop.MartType == MartTypeFrontier ? 0x1Fu : 1u)!;
        StringTemplate.StringTemplate_Format(shop.Template!, shop.Text!, format);
        GameString.String_Free(format);
        FieldMessage.FieldMessage_AddWindow(fieldSystem.BgConfig!, shop.Windows[WindowItemDescription], BgLayerMain3);
        FieldMessage.FieldMessage_DrawWindow(shop.Windows[WindowItemDescription], shop.Options!);
        shop.PrinterID = FieldMessage.FieldMessage_Print(shop.Windows[WindowItemDescription], shop.Text!, shop.Options!, 1);
    }

    // OFFSET 0x0224EAD0
    private static void Shop_DrawSprites(Shop shop)
    {
        uint[] pathPointers = RomData.OverlayU32(OverlayID, SpriteResourcePaths, SpriteResourcePathCount);
        string?[] paths = pathPointers.Select(OverlayString).ToArray();
        SpriteResourceManager.SpriteResourceManager_Init(shop.SpriteManager, paths, MaxSprites, HeapID);
        for (int i = 0; i < Shop.SpriteCount; i++)
        {
            byte[] raw = RomData.Overlay(OverlayID, (uint)(SpriteTemplates + i * SpriteTemplateSize), SpriteTemplateSize);
            SpriteTemplateFromResourceHeader template = new SpriteTemplateFromResourceHeader
            {
                ResourceHeaderID = (int)Bin.U32(raw, 0),
                X = (short)Bin.U16(raw, 4),
                Y = (short)Bin.U16(raw, 6),
                Z = (short)Bin.U16(raw, 8),
                AnimIdx = (ushort)Bin.U16(raw, 0xA),
                Priority = (int)Bin.U32(raw, 0xC),
                PlttIdx = (int)Bin.U32(raw, 0x10),
                VramType = (int)Bin.U32(raw, 0x14),
            };
            shop.Sprites[i] = SpriteResourceManager.SpriteResourceManager_CreateSprite(shop.SpriteManager, template);
        }
        GXLayers.GXLayers_EngineAToggleLayers(0x10, 1);
    }

    // OFFSET 0x0224EB14
    private static void Shop_DestroySprites(Shop shop)
    {
        for (int i = 0; i < Shop.SpriteCount; i++)
        {
            Sprite.Sprite_Delete(shop.Sprites[i]!);
        }
        SpriteResourceManager.SpriteResourceManager_Free(shop.SpriteManager);
    }

    // OFFSET 0x0224EB38
    private static void Shop_SetScrollSpritesPositionXY(Shop shop, int quantityMode)
    {
        if (quantityMode == 0)
        {
            SpriteSystem.Sprite_SetPositionXY(shop.Sprites[SpriteUpArrow]!, 0xB1, 8);
            SpriteSystem.Sprite_SetPositionXY(shop.Sprites[SpriteDownArrow]!, 0xB1, 0x84);
            return;
        }
        SpriteSystem.Sprite_SetPositionXY(shop.Sprites[SpriteUpArrow]!, 0xA2, 0x6C);
        SpriteSystem.Sprite_SetPositionXY(shop.Sprites[SpriteDownArrow]!, 0xA2, 0x84);
    }

    // OFFSET 0x0224EB7C
    private static void Shop_ChangeItemIconGfx(Shop shop, ushort item)
    {
        if (!UsesBag(shop))
        {
            Sprite.Sprite_SetDrawFlag(shop.Sprites[SpriteItemIcon]!, 0);
            return;
        }
        SpriteResourceCollection? tiles = shop.SpriteManager.Collections[SpriteResource.TypeChar];
        SpriteResource? tileResource = SpriteResourceCollection.SpriteResourceCollection_Find(tiles, 2);
        SpriteResourceCollection.SpriteResourceCollection_ModifyTiles(tiles, tileResource, NarcItemIcons, Item.Item_FileID(item, 1), 0, HeapID);
        SpriteTransfer.SpriteTransfer_RetransferCharData(tileResource);
        SpriteResourceCollection? palettes = shop.SpriteManager.Collections[SpriteResource.TypePltt];
        SpriteResource? paletteResource = SpriteResourceCollection.SpriteResourceCollection_Find(palettes, 1);
        SpriteResourceCollection.SpriteResourceCollection_ModifyPalette(palettes, paletteResource, NarcItemIcons, Item.Item_FileID(item, 2), 0, HeapID);
        SpriteTransfer.SpriteTransfer_ReplacePlttData(paletteResource);
    }

    // OFFSET 0x0224EC10
    private static void Shop_SetCursorSpritePalette(Shop shop, uint palette)
    {
        SpriteSystem.Sprite_SetExplicitPalette2(shop.Sprites[SpriteCursor]!, palette);
    }

    // OFFSET 0x0224EC20
    private static void Shop_StartScreenFade(FieldSystem fieldSystem, Shop shop)
    {
        FieldMap.FieldMap_FadeScreen(0);
        shop.State = StateFinishScreenTransition;
    }

    // OFFSET 0x0224EC38
    private static void Shop_FinishScreenTransition(FieldTask task)
    {
        if (!ScreenFadeControl.IsScreenFadeDone())
        {
            return;
        }
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        Shop shop = (Shop)FieldTask.FieldTask_GetEnv(task)!;
        Shop_CloseContextMenu(shop);
        shop.BagContext = BagItems.Bag_CreateContextWithPockets(BagItems.SaveData_GetBag(fieldSystem.SaveData!), RomData.Overlay(OverlayID, SellPockets, SellPocketCount), HeapID);
        BagContext.BagContext_Init(shop.BagContext, fieldSystem.SaveData!, 2, fieldSystem.BagCursor);
        FieldSystemChildProcesses.FieldSystem_StartBagApplication(fieldSystem, shop.BagContext);
        FieldTask.FieldTask_InitJump(task, _ => throw Unported.Missing(0x0209AC14), shop);
        shop.State = StateSelling;
    }

    // OFFSET 0x0224EC9C
    private static byte Shop_ReinitMerchantMessage(FieldSystem fieldSystem, Shop shop)
    {
        if (!ScreenFadeControl.IsScreenFadeDone())
        {
            return StateReinitMerchantMessage;
        }
        FieldMessage.FieldMessage_AddWindow(fieldSystem.BgConfig!, shop.Windows[WindowItemDescription], BgLayerMain3);
        FieldMessage.FieldMessage_DrawWindow(shop.Windows[WindowItemDescription], shop.Options!);
        PrintMessage(shop, 2, shop.Windows[WindowItemDescription]);
        return StateReinitContextMenu;
    }
}
