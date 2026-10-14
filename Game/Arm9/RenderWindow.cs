using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class WaitDial
{
    public const int FrameCount = 8;
    public const int FrameBytes = 0x80;

    public Window? Window;
    public readonly byte[] Frames = new byte[FrameCount * FrameBytes];
    public readonly byte[] SavedTiles = new byte[FrameBytes];
    public ushort Tile;
    public byte Counter;
    public byte Frame;
    public byte State;
}

public sealed class Overlay5SpriteManager
{
    public SpriteList? SpriteList;
    public readonly G2dRenderer Renderer = new();
    public readonly SpriteResourceCollection?[] Collections = new SpriteResourceCollection?[SpriteManager.ResourceTypes];
    public readonly SpriteResourceList?[] Lists = new SpriteResourceList?[SpriteManager.ResourceTypes];
    public ushort NumTypes;
    public ushort HeapID;
    public SpriteResourcesHeaderList? Headers;
}

public sealed class PokemonPreview
{
    public readonly Overlay5SpriteManager Sprites = new();
    public ManagedSprite? Sprite;
    public object BgConfig = null!;
    public byte BgLayer;
    public byte X;
    public byte Y;
    public byte State;
}

public static class RenderWindow
{
    private const int WindowGraphicsArchive = 0x26;
    private const int SignpostGraphicsArchive = 0x24;
    private const int PaletteLocationMainBg = 0;
    private const int PaletteLocationSubBg = 4;
    private const int FirstSubBgLayer = 4;
    private const int KeepPalette = 16;
    private const int TileBytes = 0x20;
    private const int PreviewResourceID = 0x15CD5;
    private const int PreviewSize = 10;
    private const int PreviewFrameBytes = 0xC80;
    private const int DisplayMain = 0;
    private const int VramMain = 1;

    private const int WaitDialDrawFrame = 0;
    private const int WaitDialDrawInit = 1;
    private const int WaitDialErase = 2;

    private const int PreviewStateIdle = 0;
    private const int PreviewStateErase = 1;
    private const int PreviewStateStartAnim = 2;
    private const int PreviewStateAnimating = 3;

    // OFFSET 0x0200DA60
    public static void LoadStandardWindowTiles(object bgConfig, uint bgLayer, uint tileStart, int alternate, int heapID)
    {
        if (alternate == 0)
        {
            Graphics.Graphics_LoadTilesToBgLayer(WindowGraphicsArchive, 0, bgConfig, bgLayer, tileStart, 0, 0, heapID);
        }
        else
        {
            Graphics.Graphics_LoadTilesToBgLayer(WindowGraphicsArchive, 1, bgConfig, bgLayer, tileStart, 0, 0, heapID);
        }
    }

    // OFFSET 0x0200DAA0
    public static uint GetStandardWindowPaletteNARCMember()
    {
        return 0x18;
    }

    // OFFSET 0x0200DAA4
    public static void LoadStandardWindowGraphics(object bgConfig, uint bgLayer, uint tileStart, uint palette, byte frameType, int heapID)
    {
        uint tilesMember = frameType != 0 ? 1u : 0u;
        Graphics.Graphics_LoadTilesToBgLayer(WindowGraphicsArchive, tilesMember, bgConfig, bgLayer, tileStart, 0, 0, heapID);
        uint paletteMember = frameType == 2 ? 0x2Du : 0x18u;
        if (bgLayer < FirstSubBgLayer)
        {
            Graphics.Graphics_LoadPalette(WindowGraphicsArchive, paletteMember, PaletteLocationMainBg, palette * 0x20, 0x20, heapID);
        }
        else
        {
            Graphics.Graphics_LoadPalette(WindowGraphicsArchive, paletteMember, PaletteLocationSubBg, palette * 0x20, 0x20, heapID);
        }
    }

    // OFFSET 0x0200DB10
    private static void DrawStandardWindowFrame(object bgConfig, int bgLayer, int x, int y, byte width, byte height, int palette, ushort tile)
    {
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, tile, (byte)(x - 1), (byte)(y - 1), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 1), x, (byte)(y - 1), width, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 2), (byte)(x + width), (byte)(y - 1), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 3), (byte)(x - 1), y, 1, height, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 5), (byte)(x + width), y, 1, height, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 6), (byte)(x - 1), (byte)(y + height), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 7), x, (byte)(y + height), width, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 8), (byte)(x + width), (byte)(y + height), 1, 1, palette);
    }

    // OFFSET 0x0200DC48
    public static void Window_DrawStandardFrame(Window window, int skipTransfer, ushort tile, int palette)
    {
        int bgLayer = Bg.Window_GetBgLayer(window);
        int x = Bg.Window_GetXPos(window);
        int y = Bg.Window_GetYPos(window);
        int width = Bg.Window_GetWidth(window);
        int height = Bg.Window_GetHeight(window);
        DrawStandardWindowFrame(window.BgConfig, bgLayer, x, y, (byte)width, (byte)height, palette, tile);
        if (skipTransfer == 0)
        {
            Bg.Window_CopyToVRAM(window);
        }
    }

    // OFFSET 0x0200DC9C
    public static void Window_EraseStandardFrame(Window window, int skipTransfer)
    {
        int bgLayer = Bg.Window_GetBgLayer(window);
        int x = Bg.Window_GetXPos(window);
        int y = Bg.Window_GetYPos(window);
        int width = Bg.Window_GetWidth(window);
        int height = Bg.Window_GetHeight(window);
        Bg.Bg_FillTilemapRect(window.BgConfig, bgLayer, 0, (byte)(x - 1), (byte)(y - 1), (byte)(width + 2), (byte)(height + 2), 0);
        if (skipTransfer == 0)
        {
            Bg.Window_ClearAndCopyToVRAM(window);
        }
    }

    // OFFSET 0x0200DD04
    public static uint GetMessageBoxTilesNARCMember(uint frame)
    {
        return frame + 2;
    }

    // OFFSET 0x0200DD08
    public static uint GetMessageBoxPaletteNARCMember(uint frame)
    {
        return frame + 0x19;
    }

    // OFFSET 0x0200DD0C
    public static void LoadMessageBoxGraphics(object bgConfig, uint bgLayer, uint tileStart, uint palette, byte frame, int heapID)
    {
        Graphics.Graphics_LoadTilesToBgLayer(WindowGraphicsArchive, GetMessageBoxTilesNARCMember(frame), bgConfig, bgLayer, tileStart, 0, 0, heapID);
        if (bgLayer < FirstSubBgLayer)
        {
            Graphics.Graphics_LoadPalette(WindowGraphicsArchive, GetMessageBoxPaletteNARCMember(frame), PaletteLocationMainBg, palette * 0x20, 0x20, heapID);
        }
        else
        {
            Graphics.Graphics_LoadPalette(WindowGraphicsArchive, GetMessageBoxPaletteNARCMember(frame), PaletteLocationSubBg, palette * 0x20, 0x20, heapID);
        }
    }

    // OFFSET 0x0200DD7C
    private static void DrawMessageBoxFrame(object bgConfig, int bgLayer, int x, int y, byte width, int height, int palette, ushort tile)
    {
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, tile, (byte)(x - 2), (byte)(y - 1), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 1), (byte)(x - 1), (byte)(y - 1), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 2), x, (byte)(y - 1), width, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 3), (byte)(x + width), (byte)(y - 1), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 4), (byte)(x + width + 1), (byte)(y - 1), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 5), (byte)(x + width + 2), (byte)(y - 1), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 6), (byte)(x - 2), y, 1, height, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 7), (byte)(x - 1), y, 1, height, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 9), (byte)(x + width), y, 1, height, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 10), (byte)(x + width + 1), y, 1, height, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 11), (byte)(x + width + 2), y, 1, height, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 12), (byte)(x - 2), (byte)(y + height), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 13), (byte)(x - 1), (byte)(y + height), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 14), x, (byte)(y + height), width, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 15), (byte)(x + width), (byte)(y + height), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 16), (byte)(x + width + 1), (byte)(y + height), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 17), (byte)(x + width + 2), (byte)(y + height), 1, 1, palette);
    }

    // OFFSET 0x0200E010
    public static void Window_DrawMessageBox(Window window, ushort tile, byte palette)
    {
        int bgLayer = Bg.Window_GetBgLayer(window);
        int x = Bg.Window_GetXPos(window);
        int y = Bg.Window_GetYPos(window);
        int width = Bg.Window_GetWidth(window);
        int height = Bg.Window_GetHeight(window);
        DrawMessageBoxFrame(window.BgConfig, bgLayer, x, y, (byte)width, height, palette, tile);
    }

    // OFFSET 0x0200E060
    public static void Window_DrawMessageBoxWithScrollCursor(Window window, int skipTransfer, ushort tile, byte palette)
    {
        Window_DrawMessageBox(window, tile, palette);
        if (skipTransfer == 0)
        {
            Bg.Window_CopyToVRAM(window);
        }
        DrawMessageBoxScrollCursor(window, tile);
    }

    // OFFSET 0x0200E084
    public static void Window_EraseMessageBox(Window window, int skipTransfer)
    {
        int bgLayer = Bg.Window_GetBgLayer(window);
        int x = Bg.Window_GetXPos(window);
        int y = Bg.Window_GetYPos(window);
        int width = Bg.Window_GetWidth(window);
        int height = Bg.Window_GetHeight(window);
        Bg.Bg_FillTilemapRect(window.BgConfig, bgLayer, 0, (byte)(x - 2), (byte)(y - 1), (byte)(width + 5), (byte)(height + 2), 0);
        if (skipTransfer == 0)
        {
            Bg.Window_ClearAndCopyToVRAM(window);
        }
    }

    // OFFSET 0x0200E0EC
    private static void BlitRectToBitmap(byte[] srcPixels, ushort srcX, ushort srcY, ushort srcWidth, ushort srcHeight, byte[] destPixels, ushort destWidth, ushort destHeight, ushort destX, ushort destY, ushort width, ushort height)
    {
        Bg.Bitmap_BlitRect4bpp(new Bitmap { Pixels = srcPixels, Width = srcWidth, Height = srcHeight }, new Bitmap { Pixels = destPixels, Width = destWidth, Height = destHeight }, srcX, srcY, destX, destY, width, height, 0);
    }

    // OFFSET 0x0200E130
    private static void DrawMessageBoxScrollCursor(Window window, ushort tile)
    {
        int heapID = Bg.BgConfig_GetHeapID(window.BgConfig);
        int bgLayer = Bg.Window_GetBgLayer(window);
        byte[] buffer = new byte[0x180];
        byte[] bgTiles = Bg.Bg_GetCharPtr(bgLayer);
        Graphics.Graphics_GetCharData(WindowGraphicsArchive, 0x16, 0, out CharacterData? charData, heapID);
        byte[] rawData = charData!.RawData;
        for (byte i = 0; i < 3; i++)
        {
            Array.Copy(bgTiles, (tile + 10) * TileBytes, buffer, i * 0x80, TileBytes);
            Array.Copy(bgTiles, (tile + 11) * TileBytes, buffer, i * 0x80 + 0x20, TileBytes);
            Array.Copy(bgTiles, (tile + 10) * TileBytes, buffer, i * 0x80 + 0x40, TileBytes);
            Array.Copy(bgTiles, (tile + 11) * TileBytes, buffer, i * 0x80 + 0x60, TileBytes);
        }
        BlitRectToBitmap(rawData, 4, 0, 12, 48, buffer, 12, 48, 1, 0, 12, 48);
        Bg.Bg_LoadTiles(window.BgConfig, bgLayer, buffer, 0x180, (uint)(tile + 18));
        RenderText.TextPrinter_SetScrollArrowBaseTile(tile);
    }

    // OFFSET 0x0200E218
    public static void ReplaceTransparentTiles(object bgConfig, int bgLayer, uint tileStart, byte color, byte frame, int heapID)
    {
        const int Size = 0x240;
        Graphics.Graphics_GetCharData(WindowGraphicsArchive, GetMessageBoxTilesNARCMember(frame), 0, out CharacterData? charData, heapID);
        byte[] buffer = new byte[Size];
        Array.Copy(charData!.RawData, buffer, Size);
        for (int i = 0; i < Size; i++)
        {
            byte high = (byte)(buffer[i] >> 4);
            byte low = (byte)(buffer[i] & 0xF);
            if (high == 0)
            {
                high = color;
            }
            if (low == 0)
            {
                low = color;
            }
            buffer[i] = (byte)((high << 4) | low);
        }
        Bg.Bg_LoadTiles(bgConfig, bgLayer, buffer, Size, tileStart);
    }

    // OFFSET 0x0200E2A4
    public static void LoadSignpostContentGraphics(object bgConfig, uint bgLayer, ushort tileStart, uint palette, byte signpostType, ushort contentID, int heapID)
    {
        Graphics.Graphics_LoadTilesToBgLayer(SignpostGraphicsArchive, 0, bgConfig, bgLayer, tileStart, 0x3C0, 0, heapID);
        byte[] file = NARC.NARC_AllocAndReadWholeMemberByIndexPair(SignpostGraphicsArchive, 1, heapID);
        G2d.GetUnpackedPaletteData(file, out PaletteFile? paletteData);
        Bg.Bg_LoadPalette((int)bgLayer, paletteData!.Colors, signpostType * 16, 0x20, (ushort)(palette << 5));
        if (signpostType <= 1)
        {
            LoadSignpostContentTiles(bgConfig, bgLayer, (ushort)(tileStart + 30), signpostType, contentID, heapID);
        }
    }

    // OFFSET 0x0200E31C
    private static void LoadSignpostContentTiles(object bgConfig, uint bgLayer, ushort tileStart, byte signpostType, ushort contentID, int heapID)
    {
        ushort member;
        if (signpostType == 0)
        {
            member = (ushort)(contentID + 33);
        }
        else
        {
            member = (ushort)(contentID + 2);
        }
        Graphics.Graphics_LoadTilesToBgLayer(SignpostGraphicsArchive, member, bgConfig, bgLayer, tileStart, 0x300, 0, heapID);
    }

    // OFFSET 0x0200E354
    private static void DrawSignpostFrame(object bgConfig, int bgLayer, int x, int y, byte width, int height, int palette, ushort tile)
    {
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, tile, (byte)(x - 9), (byte)(y - 1), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 1), (byte)(x - 8), (byte)(y - 1), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 2), (byte)(x - 7), (byte)(y - 1), (byte)(width + 7), 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 3), (byte)(x + width), (byte)(y - 1), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 4), (byte)(x + width + 1), (byte)(y - 1), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 5), (byte)(x + width + 2), (byte)(y - 1), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 6), (byte)(x - 9), y, 1, height, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 7), (byte)(x - 8), y, 1, height, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 8), (byte)(x - 1), y, 1, height, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 9), (byte)(x + width), y, 1, height, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 10), (byte)(x + width + 1), y, 1, height, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 11), (byte)(x + width + 2), y, 1, height, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 12), (byte)(x - 9), (byte)(y + height), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 13), (byte)(x - 8), (byte)(y + height), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 14), (byte)(x - 7), (byte)(y + height), (byte)(width + 7), 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 15), (byte)(x + width), (byte)(y + height), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 16), (byte)(x + width + 1), (byte)(y + height), 1, 1, palette);
        Bg.Bg_FillTilemapRect(bgConfig, bgLayer, (ushort)(tile + 17), (byte)(x + width + 2), (byte)(y + height), 1, 1, palette);
    }

    // OFFSET 0x0200E61C
    private static void DrawSignpostTiles(Window window, ushort tile, int palette)
    {
        int bgLayer = Bg.Window_GetBgLayer(window);
        ushort x = (ushort)(Bg.Window_GetXPos(window) - 7);
        int y = Bg.Window_GetYPos(window);
        for (ushort row = 0; row < 4; row++)
        {
            for (ushort column = 0; column < 6; column++)
            {
                Bg.Bg_FillTilemapRect(window.BgConfig, bgLayer, (ushort)(tile + row * 6 + column), (byte)(x + column), (byte)(y + row), 1, 1, palette);
            }
        }
    }

    // OFFSET 0x0200E69C
    public static void Window_DrawSignpost(Window window, int skipTransfer, ushort tile, int palette, byte signpostType)
    {
        int bgLayer = Bg.Window_GetBgLayer(window);
        if (signpostType <= 1)
        {
            int x = Bg.Window_GetXPos(window);
            int y = Bg.Window_GetYPos(window);
            int width = Bg.Window_GetWidth(window);
            int height = Bg.Window_GetHeight(window);
            DrawSignpostFrame(window.BgConfig, bgLayer, x, y, (byte)width, height, palette, tile);
            DrawSignpostTiles(window, (ushort)(tile + 30), palette);
        }
        else
        {
            int x = Bg.Window_GetXPos(window);
            int y = Bg.Window_GetYPos(window);
            int width = Bg.Window_GetWidth(window);
            int height = Bg.Window_GetHeight(window);
            DrawMessageBoxFrame(window.BgConfig, bgLayer, x, y, (byte)width, height, palette, tile);
        }
        if (skipTransfer == 0)
        {
            Bg.Window_CopyToVRAM(window);
        }
        DrawMessageBoxScrollCursor(window, tile);
    }

    // OFFSET 0x0200E744
    public static void Window_EraseSignpost(Window window, byte signpostType, int skipTransfer)
    {
        int bgLayer = Bg.Window_GetBgLayer(window);
        if (signpostType <= 1)
        {
            int x = Bg.Window_GetXPos(window);
            int y = Bg.Window_GetYPos(window);
            int width = Bg.Window_GetWidth(window);
            int height = Bg.Window_GetHeight(window);
            Bg.Bg_FillTilemapRect(window.BgConfig, bgLayer, 0, (byte)(x - 9), (byte)(y - 1), (byte)(width + 11), (byte)(height + 2), 0);
        }
        else
        {
            int x = Bg.Window_GetXPos(window);
            int y = Bg.Window_GetYPos(window);
            int width = Bg.Window_GetWidth(window);
            int height = Bg.Window_GetHeight(window);
            Bg.Bg_FillTilemapRect(window.BgConfig, bgLayer, 0, (byte)(x - 2), (byte)(y - 1), (byte)(width + 5), (byte)(height + 2), 0);
        }
        if (skipTransfer == 0)
        {
            Bg.Window_ClearAndCopyToVRAM(window);
        }
    }

    // OFFSET 0x0200E7FC
    public static WaitDial Window_AddWaitDial(Window window, ushort tile)
    {
        int heapID = Bg.BgConfig_GetHeapID(window.BgConfig);
        int bgLayer = Bg.Window_GetBgLayer(window);
        byte[] bgTiles = Bg.Bg_GetCharPtr(bgLayer);
        WaitDial dial = new();
        Array.Copy(bgTiles, (tile + 18) * TileBytes, dial.SavedTiles, 0, WaitDial.FrameBytes);
        byte[] background = new byte[WaitDial.FrameBytes];
        Array.Copy(bgTiles, (tile + 10) * TileBytes, background, 0, TileBytes);
        Array.Copy(bgTiles, (tile + 11) * TileBytes, background, 0x20, TileBytes);
        Array.Copy(bgTiles, (tile + 10) * TileBytes, background, 0x40, TileBytes);
        Array.Copy(bgTiles, (tile + 11) * TileBytes, background, 0x60, TileBytes);
        for (byte i = 0; i < WaitDial.FrameCount; i++)
        {
            Array.Copy(background, 0, dial.Frames, i * WaitDial.FrameBytes, WaitDial.FrameBytes);
        }
        Graphics.Graphics_GetCharData(WindowGraphicsArchive, 0x17, 0, out CharacterData? charData, heapID);
        BlitRectToBitmap(charData!.RawData, 0, 0, 16, 128, dial.Frames, 16, 128, 0, 0, 16, 128);
        dial.Window = window;
        dial.Tile = tile;
        dial.Counter = 0;
        dial.Frame = 0;
        dial.State = 0;
        SysTask.SysTask_ExecuteOnVBlank(SysTask_TickWaitDial, dial, 0);
        DrawWaitDial(dial, WaitDialDrawInit);
        return dial;
    }

    // OFFSET 0x0200E924
    private static void DrawWaitDial(WaitDial dial, uint mode)
    {
        Window window = dial.Window!;
        int bgLayer = Bg.Window_GetBgLayer(window);
        int x = Bg.Window_GetXPos(window);
        int y = Bg.Window_GetYPos(window);
        int width = Bg.Window_GetWidth(window);
        if (mode == WaitDialErase)
        {
            Bg.Bg_LoadTiles(window.BgConfig, bgLayer, dial.SavedTiles, WaitDial.FrameBytes, (uint)(dial.Tile + 18));
            Bg.Bg_FillTilemapRect(window.BgConfig, bgLayer, (ushort)(dial.Tile + 10), (byte)(x + width + 1), (byte)(y + 2), 1, 1, KeepPalette);
            Bg.Bg_FillTilemapRect(window.BgConfig, bgLayer, (ushort)(dial.Tile + 11), (byte)(x + width + 2), (byte)(y + 2), 1, 1, KeepPalette);
            Bg.Bg_FillTilemapRect(window.BgConfig, bgLayer, (ushort)(dial.Tile + 10), (byte)(x + width + 1), (byte)(y + 3), 1, 1, KeepPalette);
            Bg.Bg_FillTilemapRect(window.BgConfig, bgLayer, (ushort)(dial.Tile + 11), (byte)(x + width + 2), (byte)(y + 3), 1, 1, KeepPalette);
            Bg.Bg_CopyTilemapBufferToVRAM(window.BgConfig, bgLayer);
            return;
        }
        byte[] frame = new byte[WaitDial.FrameBytes];
        Array.Copy(dial.Frames, dial.Frame * WaitDial.FrameBytes, frame, 0, WaitDial.FrameBytes);
        Bg.Bg_LoadTiles(window.BgConfig, bgLayer, frame, WaitDial.FrameBytes, (uint)(dial.Tile + 18));
        if (mode != WaitDialDrawFrame)
        {
            Bg.Bg_FillTilemapRect(window.BgConfig, bgLayer, (ushort)(dial.Tile + 18), (byte)(x + width + 1), (byte)(y + 2), 1, 1, KeepPalette);
            Bg.Bg_FillTilemapRect(window.BgConfig, bgLayer, (ushort)(dial.Tile + 19), (byte)(x + width + 2), (byte)(y + 2), 1, 1, KeepPalette);
            Bg.Bg_FillTilemapRect(window.BgConfig, bgLayer, (ushort)(dial.Tile + 20), (byte)(x + width + 1), (byte)(y + 3), 1, 1, KeepPalette);
            Bg.Bg_FillTilemapRect(window.BgConfig, bgLayer, (ushort)(dial.Tile + 21), (byte)(x + width + 2), (byte)(y + 3), 1, 1, KeepPalette);
            Bg.Bg_CopyTilemapBufferToVRAM(window.BgConfig, bgLayer);
        }
    }

    // OFFSET 0x0200EB20
    private static void SysTask_TickWaitDial(SysTask task, object? param)
    {
        WaitDial dial = (WaitDial)param!;
        if (dial.State != 0)
        {
            if (dial.State == 1)
            {
                DrawWaitDial(dial, WaitDialErase);
            }
            SysTask.SysTask_Done(task);
            return;
        }
        dial.Counter++;
        if (dial.Counter == 16)
        {
            dial.Counter = 0;
            dial.Frame = (byte)((dial.Frame + 1) & 7);
            DrawWaitDial(dial, WaitDialDrawFrame);
        }
    }

    // OFFSET 0x0200EB8C
    private static void SysTask_CleanupWaitDial(SysTask task, object? param)
    {
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x0200EBA0
    public static void DestroyWaitDial(WaitDial dial)
    {
        SysTask.SysTask_ExecuteAfterVBlank(SysTask_CleanupWaitDial, dial, 0);
        dial.State = 1;
    }

    // OFFSET 0x0200EBC8
    public static void DestroyWaitDialTaskOnly(WaitDial dial)
    {
        SysTask.SysTask_ExecuteAfterVBlank(SysTask_CleanupWaitDial, dial, 0);
        dial.State = 2;
    }

    // OFFSET 0x0200EBF0
    public static PokemonPreview DrawPokemonPreview(object bgConfig, byte bgLayer, byte x, byte y, byte palette, ushort tile, ushort species, byte gender, int heapID)
    {
        PokemonPreview preview = CreatePokemonPreviewTask(bgConfig, bgLayer, x, y, heapID);
        InitPokemonPreviewSprites(preview, heapID);
        LoadPokemonPreviewResources(preview);
        CreatePokemonPreviewSprite(preview, x, y);
        LoadAndDrawPokemonPreviewSprite(preview, species, gender);
        DrawPokemonPreviewWindow(preview, palette, tile);
        Bg.Bg_CopyTilemapBufferToVRAM(bgConfig, bgLayer);
        return preview;
    }

    // OFFSET 0x0200EC48
    public static PokemonPreview DrawPokemonPreviewFromStruct(object bgConfig, byte bgLayer, byte x, byte y, byte palette, ushort tile, object mon, int heapID)
    {
        PokemonPreview preview = CreatePokemonPreviewTask(bgConfig, bgLayer, x, y, heapID);
        InitPokemonPreviewSprites(preview, heapID);
        LoadPokemonPreviewResources(preview);
        CreatePokemonPreviewSprite(preview, x, y);
        LoadAndDrawPokemonPreviewSpriteFromStruct(preview, mon);
        DrawPokemonPreviewWindow(preview, palette, tile);
        Bg.Bg_CopyTilemapBufferToVRAM(bgConfig, bgLayer);
        return preview;
    }

    // OFFSET 0x0200EC9C
    private static void SysTask_HandlePokemonPreview(SysTask task, object? param)
    {
        PokemonPreview preview = (PokemonPreview)param!;
        switch (preview.State)
        {
            case PreviewStateErase:
                ErasePokemonPreviewWindow(preview);
                SpriteSystem.Sprite_DeleteAndFreeResources(preview.Sprite!);
                SpriteResourceManager.SpriteResourceManager_Cleanup(preview.Sprites);
                SysTaskExtensions.SysTask_FinishAndFreeParam(task);
                return;
            case PreviewStateStartAnim:
                preview.State = PreviewStateAnimating;
                Sprite.Sprite_SetAnim(preview.Sprite!.Sprite!, 1);
                break;
            case PreviewStateAnimating:
                if (Sprite.Sprite_GetAnimFrame(preview.Sprite!.Sprite!) == 6)
                {
                    preview.State = PreviewStateIdle;
                }
                break;
        }
        Sprite.Sprite_UpdateAnim(preview.Sprite!.Sprite!, 0x1000);
        Sprite.SpriteList_Update(preview.Sprites.SpriteList!);
    }

    // OFFSET 0x0200ED14
    private static PokemonPreview CreatePokemonPreviewTask(object bgConfig, byte bgLayer, byte x, byte y, int heapID)
    {
        SysTask task = SysTaskExtensions.SysTask_StartAndAllocateParam<PokemonPreview>(SysTask_HandlePokemonPreview, 0x170, 0, heapID)!;
        PokemonPreview preview = (PokemonPreview)SysTaskManager.SysTask_GetParam(task)!;
        preview.State = PreviewStateIdle;
        preview.BgConfig = bgConfig;
        preview.BgLayer = bgLayer;
        preview.X = x;
        preview.Y = y;
        return preview;
    }

    // OFFSET 0x0200ED50
    private static void InitPokemonPreviewSprites(PokemonPreview preview, int heapID)
    {
        uint[] capacities = RomData.Arm9U32(0x020E5028, 6);
        SpriteResourceManager.SpriteResourceManager_SetCapacities(preview.Sprites, capacities, 1, heapID);
    }

    // OFFSET 0x0200ED7C
    private static void LoadPokemonPreviewResources(PokemonPreview preview)
    {
        SpriteResourceManager.SpriteResourceManager_AddPalette(preview.Sprites, WindowGraphicsArchive, 49, 0, 1, VramMain, PreviewResourceID);
        SpriteResourceManager.SpriteResourceManager_AddCell(preview.Sprites, WindowGraphicsArchive, 47, 0, PreviewResourceID);
        SpriteResourceManager.SpriteResourceManager_AddAnimation(preview.Sprites, WindowGraphicsArchive, 46, 0, PreviewResourceID);
        SpriteResourceManager.SpriteResourceManager_AddTiles(preview.Sprites, WindowGraphicsArchive, 48, 0, VramMain, PreviewResourceID);
    }

    private static SpriteTemplate LoadSpriteTemplate(uint address)
    {
        byte[] raw = RomData.Arm9(address, 0x34);
        SpriteTemplate template = new()
        {
            X = (short)Bin.U16(raw, 0),
            Y = (short)Bin.U16(raw, 2),
            Z = (short)Bin.U16(raw, 4),
            AnimIdx = (ushort)Bin.U16(raw, 6),
            Priority = Bin.I32(raw, 8),
            PlttIdx = Bin.I32(raw, 0xC),
            VramType = Bin.I32(raw, 0x10),
            BgPriority = Bin.I32(raw, 0x2C),
            VramTransfer = Bin.I32(raw, 0x30),
        };
        for (int i = 0; i < SpriteManager.ResourceTypes; i++)
        {
            template.Resources[i] = Bin.I32(raw, 0x14 + i * 4);
        }
        return template;
    }

    // OFFSET 0x0200EDD4
    private static void CreatePokemonPreviewSprite(PokemonPreview preview, byte x, byte y)
    {
        SpriteTemplate template = LoadSpriteTemplate(0x020E5040);
        template.X = (short)((x + 5) * 8);
        template.Y = (short)((y + 5) * 8);
        preview.Sprite = SpriteResourceManager.SpriteResourceManager_CreateManagedSprite(preview.Sprites, template);
        Sprite.SpriteList_Update(preview.Sprites.SpriteList!);
        GXLayers.GXLayers_EngineBToggleLayers(0x10, 1);
    }

    // OFFSET 0x0200EE24
    private static void LoadAndDrawPokemonPreviewSprite(PokemonPreview preview, ushort species, byte gender)
    {
        PokemonSpriteManager monSpriteMan = PokemonSpriteManager.PokemonSpriteManager_New(preview.Sprites.HeapID);
        PokemonSpriteTemplate template = new();
        PokemonSpriteTemplates.BuildPokemonSpriteTemplate(template, species, gender, 2, 0, 0, 0);
        DrawPokemonPreviewSprite(preview, template);
        PokemonSpriteManager.PokemonSpriteManager_Free(monSpriteMan);
    }

    // OFFSET 0x0200EE64
    private static void LoadAndDrawPokemonPreviewSpriteFromStruct(PokemonPreview preview, object mon)
    {
        PokemonSpriteManager monSpriteMan = PokemonSpriteManager.PokemonSpriteManager_New(preview.Sprites.HeapID);
        PokemonSpriteTemplate template = new();
        PokemonSpriteTemplates.Pokemon_BuildSpriteTemplate(template, (Pokemon)mon, 2);
        DrawPokemonPreviewSprite(preview, template);
        PokemonSpriteManager.PokemonSpriteManager_Free(monSpriteMan);
    }

    // OFFSET 0x0200EE98
    private static void DrawPokemonPreviewSprite(PokemonPreview preview, PokemonSpriteTemplate template)
    {
        int heapID = preview.Sprites.HeapID;
        int narcID = (int)(template.Word0 & 0xFFFF);
        int character = (int)(template.Word0 >> 16);
        int palette = (int)(template.Word1 & 0xFFFF);
        byte[] buffer = new byte[PreviewFrameBytes * 2];
        CharacterSprite.CharacterSprite_LoadSpriteRegion(narcID, character, heapID, RomData.Arm9U32(0x020E5018, 4), buffer, 0);
        CharacterSprite.CharacterSprite_LoadSpriteRegion(narcID, character, heapID, RomData.Arm9U32(0x020E5008, 4), buffer, PreviewFrameBytes);
        SpriteResource? charResource = SpriteResourceCollection.SpriteResourceCollection_Find(preview.Sprites.Collections[SpriteResource.TypeChar], PreviewResourceID);
        ImageProxy imageProxy = SpriteTransfer.SpriteTransfer_GetImageProxy(charResource)!;
        int tileLocation = G2d.GetImageLocation(imageProxy, VramMain);
        Video.LoadObjectTiles(DisplayMain, buffer, (uint)tileLocation, (uint)buffer.Length);
        ushort[] colors = CharacterSprite.CharacterSprite_LoadPalette(narcID, palette, heapID);
        SpriteResource? plttResource = SpriteResourceCollection.SpriteResourceCollection_Find(preview.Sprites.Collections[SpriteResource.TypePltt], PreviewResourceID);
        ImagePaletteProxy paletteProxy = SpriteTransfer.SpriteTransfer_GetPaletteProxy(plttResource, imageProxy)!;
        int paletteLocation = G2d.GetImagePaletteLocation(paletteProxy, VramMain);
        Video.LoadPaletteRange(Video.MainObject, colors, 0, (uint)paletteLocation, 0x20);
    }

    // OFFSET 0x0200EF7C
    private static void DrawPokemonPreviewWindow(PokemonPreview preview, byte palette, ushort tile)
    {
        Bg.Bg_FillTilemapRect(preview.BgConfig, preview.BgLayer, tile, (byte)(preview.X - 1), (byte)(preview.Y - 1), 1, 1, palette);
        Bg.Bg_FillTilemapRect(preview.BgConfig, preview.BgLayer, (ushort)(tile + 1), preview.X, (byte)(preview.Y - 1), PreviewSize, 1, palette);
        Bg.Bg_FillTilemapRect(preview.BgConfig, preview.BgLayer, (ushort)(tile + 2), (byte)(preview.X + PreviewSize), (byte)(preview.Y - 1), 1, 1, palette);
        Bg.Bg_FillTilemapRect(preview.BgConfig, preview.BgLayer, (ushort)(tile + 4), preview.X, preview.Y, PreviewSize, PreviewSize, palette);
        Bg.Bg_FillTilemapRect(preview.BgConfig, preview.BgLayer, (ushort)(tile + 3), (byte)(preview.X - 1), preview.Y, 1, PreviewSize, palette);
        Bg.Bg_FillTilemapRect(preview.BgConfig, preview.BgLayer, (ushort)(tile + 5), (byte)(preview.X + PreviewSize), preview.Y, 1, PreviewSize, palette);
        Bg.Bg_FillTilemapRect(preview.BgConfig, preview.BgLayer, (ushort)(tile + 6), (byte)(preview.X - 1), (byte)(preview.Y + PreviewSize), 1, 1, palette);
        Bg.Bg_FillTilemapRect(preview.BgConfig, preview.BgLayer, (ushort)(tile + 7), preview.X, (byte)(preview.Y + PreviewSize), PreviewSize, 1, palette);
        Bg.Bg_FillTilemapRect(preview.BgConfig, preview.BgLayer, (ushort)(tile + 8), (byte)(preview.X + PreviewSize), (byte)(preview.Y + PreviewSize), 1, 1, palette);
        Bg.Bg_ScheduleTilemapTransfer(preview.BgConfig, preview.BgLayer);
    }

    // OFFSET 0x0200F12C
    private static void ErasePokemonPreviewWindow(PokemonPreview preview)
    {
        Bg.Bg_FillTilemapRect(preview.BgConfig, preview.BgLayer, 0, (byte)(preview.X - 1), (byte)(preview.Y - 1), PreviewSize + 2, PreviewSize + 2, 0);
        Bg.Bg_ScheduleTilemapTransfer(preview.BgConfig, preview.BgLayer);
    }
}
