using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public static partial class PokemonSummaryScreenApp
{
    private const uint SubscreenTouchRects = 0x02100D78;
    private const uint SubscreenButtons = 0x02100D84;
    private const int SubscreenTypeNone = 0xF;
    private const int SubscreenBgLayer = 4;
    private const int ButtonEntrySize = 4;
    private const int ButtonTerminator = 0xFF;
    private const int SeqButtonPress = 0x5E4;
    private const ushort SubscreenTransparentPixel = 0xFFFE;

    private static byte[] SubscreenButtonTable(PokemonSummaryScreen summaryScreen)
    {
        uint address = RomData.Arm9U32((uint)(SubscreenButtons + summaryScreen.SubscreenType * 4), 1)[0];
        int count = 0;
        while (RomData.Arm9((uint)(address + count * ButtonEntrySize), 1)[0] != ButtonTerminator)
        {
            count++;
        }
        return RomData.Arm9(address, (count + 1) * ButtonEntrySize);
    }

    private static TouchScreenHitTable[] SubscreenHitTable(PokemonSummaryScreen summaryScreen)
    {
        uint address = RomData.Arm9U32((uint)(SubscreenTouchRects + summaryScreen.SubscreenType * 4), 1)[0];
        int count = 0;
        while (RomData.Arm9((uint)(address + count * 4), 1)[0] != TouchScreen.TableTerminator)
        {
            count++;
        }
        byte[] raw = RomData.Arm9(address, (count + 1) * 4);
        TouchScreenHitTable[] rects = new TouchScreenHitTable[count + 1];
        for (int i = 0; i <= count; i++)
        {
            rects[i].Top = raw[i * 4];
            rects[i].Bottom = raw[i * 4 + 1];
            rects[i].Left = raw[i * 4 + 2];
            rects[i].Right = raw[i * 4 + 3];
        }
        return rects;
    }

    // OFFSET 0x020920C0
    private static void PokemonSummaryScreen_SetSubscreenType(PokemonSummaryScreen summaryScreen)
    {
        PokemonSummary data = summaryScreen.Data!;
        switch (data.Mode)
        {
            case ModeNormal:
            case ModeLock:
                if (data.ShowContest != 0)
                {
                    summaryScreen.SubscreenType = 0;
                }
                else
                {
                    summaryScreen.SubscreenType = 1;
                    Bg.Bg_ScheduleScroll(summaryScreen.BgConfig!, SubscreenBgLayer, 0, 4);
                }
                break;
            default:
                summaryScreen.SubscreenType = SubscreenTypeNone;
                break;
        }
        PokemonSummaryScreen_UpdateSubscreenButtonGfx(summaryScreen);
    }

    // OFFSET 0x0209212C
    private static void UpdateSubscreenButtonTilemap(PokemonSummaryScreen summaryScreen, byte[] buttons, int entry, byte state)
    {
        byte[] tilemap = Bg.Bg_GetTilemapBuffer(summaryScreen.BgConfig!, SubscreenBgLayer)!;
        int offset = entry * ButtonEntrySize;
        byte page = buttons[offset];
        ushort baseTile = (ushort)(state * 5 + (page & 1) * 15 + (page >> 1) * 150 + 0x1E);
        for (byte y = 0; y < 5; y++)
        {
            for (byte x = 0; x < 5; x++)
            {
                int tile = (buttons[offset + 1] << 12) + x + baseTile + y * 0x1E;
                int index = (y + buttons[offset + 3]) * 32 + x + buttons[offset + 2];
                tilemap[index * 2] = (byte)tile;
                tilemap[index * 2 + 1] = (byte)(tile >> 8);
            }
        }
    }

    // OFFSET 0x0209219C
    public static void PokemonSummaryScreen_UpdateSubscreenButtonGfx(PokemonSummaryScreen summaryScreen)
    {
        if (summaryScreen.SubscreenType == SubscreenTypeNone)
        {
            return;
        }
        byte[] buttons = SubscreenButtonTable(summaryScreen);
        for (byte i = 0; buttons[i * ButtonEntrySize] != ButtonTerminator; i++)
        {
            UpdateSubscreenButtonTilemap(summaryScreen, buttons, i, 0);
        }
        Bg.Bg_ScheduleTilemapTransfer(summaryScreen.BgConfig!, SubscreenBgLayer);
    }

    // OFFSET 0x020921E4
    private static byte PokemonSummaryScreen_CheckSubscreenButtonPress(PokemonSummaryScreen summaryScreen)
    {
        int button = CheckSubscreenRectanglePress(summaryScreen);
        if (button == -1)
        {
            return 0xFF;
        }
        return (byte)button;
    }

    // OFFSET 0x020921FC
    private static int CheckSubscreenRectanglePress(PokemonSummaryScreen summaryScreen)
    {
        if (summaryScreen.SubscreenType == SubscreenTypeNone)
        {
            return -1;
        }
        return TouchScreen.TouchScreen_CheckRectanglePressed(SubscreenHitTable(summaryScreen));
    }

    // OFFSET 0x02092224
    private static int CheckSubscreenRectangleHeld(PokemonSummaryScreen summaryScreen)
    {
        if (summaryScreen.SubscreenType == SubscreenTypeNone)
        {
            return -1;
        }
        int button = TouchScreen.TouchScreen_CheckRectangleHeld(SubscreenHitTable(summaryScreen));
        if (button != -1)
        {
            ushort[] transparent = { SubscreenTransparentPixel };
            if (Bg.Bg_DoesPixelAtXYMatchVal(summaryScreen.BgConfig!, SubscreenBgLayer, gSystem.TouchX, gSystem.TouchY, transparent) == 0)
            {
                return -1;
            }
        }
        return button;
    }

    // OFFSET 0x0209228C
    private static int PokemonSummaryScreen_DrawSubscreenButtonAnim(PokemonSummaryScreen summaryScreen)
    {
        byte[] buttons = SubscreenButtonTable(summaryScreen);
        switch (summaryScreen.ButtonState)
        {
            case 0:
                SoundPlayback.Sound_PlayEffect(SeqButtonPress);
                PokemonSummaryScreen_UpdateSubscreenButtonGfx(summaryScreen);
                UpdateSubscreenButtonTilemap(summaryScreen, buttons, summaryScreen.ButtonCurrent, 2);
                Bg.Bg_ScheduleTilemapTransfer(summaryScreen.BgConfig!, SubscreenBgLayer);
                PokemonSummaryScreen_InitButtonTapAnim(summaryScreen);
                summaryScreen.ButtonAnimFrame = 0;
                summaryScreen.ButtonState = 1;
                break;
            case 1:
                summaryScreen.ButtonState = 2;
                break;
            case 2:
                if (summaryScreen.ButtonAnimFrame != 3)
                {
                    summaryScreen.ButtonAnimFrame++;
                    break;
                }
                PokemonSummaryScreen_HideButtonTapCircle(summaryScreen);
                byte button = summaryScreen.ButtonCurrent;
                if (button == CheckSubscreenRectangleHeld(summaryScreen))
                {
                    break;
                }
                byte page = buttons[button * ButtonEntrySize];
                if (summaryScreen.MonData.IsEgg != 0 && page != PageMemo && page != PageExit)
                {
                    UpdateSubscreenButtonTilemap(summaryScreen, buttons, button, 0);
                }
                else
                {
                    UpdateSubscreenButtonTilemap(summaryScreen, buttons, button, 1);
                }
                Bg.Bg_ScheduleTilemapTransfer(summaryScreen.BgConfig!, SubscreenBgLayer);
                return 1;
        }
        return 0;
    }

    // OFFSET 0x02092368
    private static void PokemonSummaryScreen_CalcSubscreenButtonTapAnimPos(PokemonSummaryScreen summaryScreen, out short x, out short y)
    {
        byte[] buttons = SubscreenButtonTable(summaryScreen);
        int offset = summaryScreen.ButtonCurrent * ButtonEntrySize;
        x = (short)(buttons[offset + 2] * 8 + 0x14);
        y = (short)(buttons[offset + 3] * 8 + 0x14);
    }

    // OFFSET 0x020923A4
    private static byte PokemonSummaryScreen_GetSubscreenButtonPage(PokemonSummaryScreen summaryScreen, byte button)
    {
        return SubscreenButtonTable(summaryScreen)[button * ButtonEntrySize];
    }
}
