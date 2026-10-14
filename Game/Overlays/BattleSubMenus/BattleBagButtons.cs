namespace PlatinumSharp.Game;

public static partial class BattleBags
{
    private const uint ButtonRects = 0x02229D7C;
    private const uint ButtonWindowLists = 0x02229DC0;
    private const int ButtonBgLayer = 6;
    private const int ButtonStateDefault = 0;
    private const int ButtonStatePressed = 1;
    private const int ButtonStateReleased = 2;
    private const int ButtonStateDisabled = 3;
    private const int ButtonLastUsedItem = 4;
    private const int ButtonMenuCancel = 5;
    private const int ButtonPocketItem1 = 6;
    private const int ButtonPrevPage = 0xC;
    private const int ButtonNextPage = 0xD;
    private const int ButtonPocketCancel = 0xE;
    private const int ButtonUseItem = 0xF;
    private const int ButtonUseItemCancel = 0x10;
    private const int AltWindowListOffset = 0xB;
    private const int WindowListEnd = 0xFF;
    private const int MaxButtonWindows = 8;
    private const ushort TilePaletteMask = 0xFFF;

    private static byte ButtonRect(int button, int field)
    {
        return BattleSubMenuData.Bytes((uint)(ButtonRects + button * 4 + field), 1)[0];
    }

    // OFFSET 0x02228128
    private static void BattleBagButtons_InitializeButtonData(BattleBag battleBag, byte[] rawData)
    {
        for (int i = 0; i < 3; i++)
        {
            LoadButtonData(battleBag.MenuPocketButtonData[i], rawData, 0, i * 9, 0x10, 9);
        }
        LoadButtonData(battleBag.UseItemButtonData[0], rawData, 0, 0x1B, 0x1A, 5);
        LoadButtonData(battleBag.UseItemButtonData[1], rawData, 0, 0x20, 0x1A, 5);
        LoadButtonData(battleBag.UseItemButtonData[2], rawData, 0, 0x25, 0x1A, 5);
        LoadButtonData(battleBag.UseItemButtonData[3], rawData, 0, 0x25, 0x1A, 5);
        for (int i = 0; i < 3; i++)
        {
            LoadButtonData(battleBag.CancelButtonData[i], rawData, i * 5, 0x39, 5, 5);
        }
        for (int i = 0; i < 4; i++)
        {
            LoadButtonData(battleBag.PocketItemButtonData[i], rawData, 0x10, i * 6, 0x10, 6);
        }
        for (int i = 0; i < 4; i++)
        {
            LoadButtonData(battleBag.PocketPrevPageButtonData[i], rawData, i * 5, 0x2F, 5, 5);
        }
        for (int i = 0; i < 4; i++)
        {
            LoadButtonData(battleBag.PocketNextPageButtonData[i], rawData, i * 5, 0x34, 5, 5);
        }
        for (int i = 0; i < 3; i++)
        {
            LoadButtonData(battleBag.MenuScreenRecoverHPPocketIconData[i], rawData, 0x14 + i * 4, 0x2F, 4, 4);
        }
        for (int i = 0; i < 3; i++)
        {
            LoadButtonData(battleBag.MenuScreenRecoverStatusPocketIconData[i], rawData, 0x14 + i * 4, 0x33, 4, 4);
        }
        for (int i = 0; i < 3; i++)
        {
            LoadButtonData(battleBag.MenuScreenPokeBallsPocketIconData[i], rawData, 0x14 + i * 4, 0x37, 4, 4);
        }
        for (int i = 0; i < 3; i++)
        {
            LoadButtonData(battleBag.MenuScreenBattleItemsIconData[i], rawData, 0x14 + i * 4, 0x3B, 4, 4);
        }
    }

    // OFFSET 0x02228460
    private static void LoadButtonData(ushort[] dest, byte[] source, int x, int y, byte width, byte height)
    {
        for (ushort row = 0; row < height; row++)
        {
            for (ushort col = 0; col < width; col++)
            {
                int src = ((y + row) * 0x20 + x + col) * 2;
                dest[row * width + col] = (ushort)(source[src] | (source[src + 1] << 8));
            }
        }
    }

    // OFFSET 0x022284B0
    private static ushort[]? RetrieveRawButtonData(BattleBag battleBag, int button, int state)
    {
        switch (button)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                return battleBag.MenuPocketButtonData[state];
            case ButtonLastUsedItem:
            case ButtonUseItem:
                return battleBag.UseItemButtonData[state];
            case ButtonMenuCancel:
            case ButtonPocketCancel:
            case ButtonUseItemCancel:
                return battleBag.CancelButtonData[state];
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
                return battleBag.PocketItemButtonData[state];
            case ButtonPrevPage:
                return battleBag.PocketPrevPageButtonData[state];
            case ButtonNextPage:
                return battleBag.PocketNextPageButtonData[state];
        }
        return null;
    }

    // OFFSET 0x02228558
    private static ushort GetButtonColor(BattleBag battleBag, int button, int state, int screen)
    {
        if (state == ButtonStateDisabled)
        {
            return 5;
        }
        switch (button)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                return 0;
            case ButtonLastUsedItem:
                return 3;
            case ButtonMenuCancel:
            case ButtonPrevPage:
            case ButtonNextPage:
            case ButtonPocketCancel:
            case ButtonUseItemCancel:
                return 2;
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
                return (ushort)(battleBag.CurrentBattlePocket + 8);
            case ButtonUseItem:
                if (screen == ScreenUseItem)
                {
                    return (ushort)(battleBag.CurrentBattlePocket + 8);
                }
                return 1;
        }
        return 0;
    }

    // OFFSET 0x022285C8
    private static void AddIconDataToButtonData(BattleBag battleBag, ushort[] dest, int button, int state)
    {
        ushort[] icon;
        switch (button)
        {
            case PocketRecoverHP:
                icon = battleBag.MenuScreenRecoverHPPocketIconData[state];
                break;
            case PocketRecoverStatus:
                icon = battleBag.MenuScreenRecoverStatusPocketIconData[state];
                break;
            case PocketPokeBalls:
                icon = battleBag.MenuScreenPokeBallsPocketIconData[state];
                break;
            case PocketBattleItems:
                icon = battleBag.MenuScreenBattleItemsIconData[state];
                break;
            default:
                return;
        }
        for (ushort row = 0; row < 4; row++)
        {
            for (ushort col = 0; col < 4; col++)
            {
                dest[row * 0x10 + col + 6] = icon[row * 4 + col];
            }
        }
    }

    // OFFSET 0x0222863C
    private static void RetrieveButtonData(BattleBag battleBag, ushort[] dest, int button, int state, int screen)
    {
        ushort[] source = RetrieveRawButtonData(battleBag, button, state)!;
        ushort color = (ushort)(GetButtonColor(battleBag, button, state, screen) << 12);
        int size = ButtonRect(button, 2) * ButtonRect(button, 3);
        for (ushort i = 0; i < size; i++)
        {
            dest[i] = (ushort)((source[i] & TilePaletteMask) | color);
        }
        AddIconDataToButtonData(battleBag, dest, button, state);
    }

    // OFFSET 0x022286B8
    private static void DrawButton(BattleBag battleBag, int button, int state, int screen)
    {
        byte height = ButtonRect(button, 3);
        byte width = ButtonRect(button, 2);
        ushort[] tiles = new ushort[width * height];
        RetrieveButtonData(battleBag, tiles, button, state, screen);
        byte[] buffer = new byte[tiles.Length * 2];
        for (int i = 0; i < tiles.Length; i++)
        {
            buffer[i * 2] = (byte)tiles[i];
            buffer[i * 2 + 1] = (byte)(tiles[i] >> 8);
        }
        Bg.Bg_LoadToTilemapRect(battleBag.Background!, ButtonBgLayer, buffer, ButtonRect(button, 0), ButtonRect(button, 1), width, height);
        Bg.Bg_ScheduleTilemapTransfer(battleBag.Background!, ButtonBgLayer);
    }

    // OFFSET 0x0222872C
    private static void UpdateWindowScroll(BattleBag battleBag, int button, int state)
    {
        uint windowList;
        if (button >= ButtonPocketItem1 && button <= ButtonPocketItem1 + 5 && battleBag.UseAltPocketMenuWindows == 0)
        {
            windowList = BattleSubMenuData.U32((uint)(ButtonWindowLists + (button + AltWindowListOffset) * 4));
        }
        else
        {
            windowList = BattleSubMenuData.U32((uint)(ButtonWindowLists + button * 4));
        }
        if (windowList == 0)
        {
            return;
        }
        int direction = 0;
        int distance = 0;
        if (state == ButtonStateDefault || state == ButtonStateReleased)
        {
            direction = 1;
            distance = 2;
        }
        else if (state == ButtonStatePressed)
        {
            direction = 0;
            distance = 4;
        }
        byte[] windows = BattleSubMenuData.Bytes(windowList, MaxButtonWindows);
        for (ushort i = 0; i < MaxButtonWindows; i++)
        {
            if (windows[i] == WindowListEnd)
            {
                break;
            }
            Bg.Window_Scroll(BagWindow(battleBag, windows[i]), direction, distance, 0);
            Bg.Window_ScheduleCopyToVRAM(BagWindow(battleBag, windows[i]));
        }
    }

    // OFFSET 0x022287A4
    private static void UpdateSpritePositions(BattleBag battleBag, int button, int state)
    {
        ManagedSprite sprite;
        if (button >= ButtonPocketItem1 && button <= ButtonPocketItem1 + 5)
        {
            sprite = BagSprite(battleBag, button - ButtonPocketItem1);
        }
        else if (button == ButtonLastUsedItem)
        {
            sprite = BagSprite(battleBag, 0);
            for (byte i = 0; i < BattleBag.NumSprites; i++)
            {
                sprite = BagSprite(battleBag, i);
                if (SpriteSystem.ManagedSprite_GetDrawFlag(sprite) != 0)
                {
                    break;
                }
            }
        }
        else
        {
            return;
        }
        if (state == ButtonStateDefault || state == ButtonStateReleased)
        {
            SpriteSystem.ManagedSprite_OffsetPositionXY(sprite, 0, 2);
        }
        else if (state == ButtonStatePressed)
        {
            SpriteSystem.ManagedSprite_OffsetPositionXY(sprite, 0, -4);
        }
    }

    // OFFSET 0x0222880C
    private static void BattleBagButtons_PressButton(BattleBag battleBag, int button, int screen)
    {
        battleBag.PressedButtonState = 0;
        battleBag.Unused2 = 0;
        battleBag.PressedButton = (byte)button;
        battleBag.PressedButtonScreen = (byte)(screen & 0xF);
        battleBag.IsAButtonPressed = 1;
    }

    // OFFSET 0x02228848
    private static void BattleBagButtons_Tick(BattleBag battleBag)
    {
        if (battleBag.IsAButtonPressed == 0)
        {
            return;
        }
        switch (battleBag.PressedButtonState)
        {
            case 0:
                DrawButton(battleBag, battleBag.PressedButton, ButtonStatePressed, battleBag.CurrentScreen);
                UpdateWindowScroll(battleBag, battleBag.PressedButton, ButtonStatePressed);
                UpdateSpritePositions(battleBag, battleBag.PressedButton, ButtonStatePressed);
                battleBag.Unused2 = 0;
                battleBag.PressedButtonState = 1;
                break;
            case 1:
                DrawButton(battleBag, battleBag.PressedButton, ButtonStateReleased, battleBag.CurrentScreen);
                UpdateWindowScroll(battleBag, battleBag.PressedButton, ButtonStateReleased);
                UpdateSpritePositions(battleBag, battleBag.PressedButton, ButtonStateReleased);
                battleBag.Unused2 = 0;
                battleBag.PressedButtonState = 2;
                break;
            case 2:
                DrawButton(battleBag, battleBag.PressedButton, ButtonStateDefault, battleBag.CurrentScreen);
                UpdateWindowScroll(battleBag, battleBag.PressedButton, ButtonStateDefault);
                UpdateSpritePositions(battleBag, battleBag.PressedButton, ButtonStateDefault);
                battleBag.Unused2 = 0;
                battleBag.PressedButtonState = 0;
                battleBag.IsAButtonPressed = 0;
                break;
        }
    }

    // OFFSET 0x02228924
    private static void BattleBagButtons_InitializeButtons(BattleBag battleBag, int screen)
    {
        switch (screen)
        {
            case ScreenMenu:
                for (int i = 0; i < 4; i++)
                {
                    DrawButton(battleBag, i, ButtonStateDefault, screen);
                }
                if (battleBag.Context!.LastUsedItem == 0)
                {
                    DrawButton(battleBag, ButtonLastUsedItem, ButtonStateDisabled, screen);
                }
                else
                {
                    DrawButton(battleBag, ButtonLastUsedItem, ButtonStateDefault, screen);
                }
                DrawButton(battleBag, ButtonMenuCancel, ButtonStateDefault, screen);
                break;
            case ScreenPocketMenu:
                for (int i = 0; i < NumSpritesPerPage; i++)
                {
                    if (BattleBag_GetItem(battleBag, i) == 0)
                    {
                        DrawButton(battleBag, ButtonPocketItem1 + i, ButtonStateDisabled, screen);
                    }
                    else
                    {
                        DrawButton(battleBag, ButtonPocketItem1 + i, ButtonStateDefault, screen);
                    }
                }
                if (battleBag.NumBattlePocketPages[battleBag.CurrentBattlePocket] == 0)
                {
                    DrawButton(battleBag, ButtonPrevPage, ButtonStateDisabled, screen);
                    DrawButton(battleBag, ButtonNextPage, ButtonStateDisabled, screen);
                }
                else
                {
                    DrawButton(battleBag, ButtonPrevPage, ButtonStateDefault, screen);
                    DrawButton(battleBag, ButtonNextPage, ButtonStateDefault, screen);
                }
                DrawButton(battleBag, ButtonPocketCancel, ButtonStateDefault, screen);
                break;
            case ScreenUseItem:
                DrawButton(battleBag, ButtonUseItem, ButtonStateDefault, screen);
                DrawButton(battleBag, ButtonUseItemCancel, ButtonStateDefault, screen);
                break;
        }
    }
}
