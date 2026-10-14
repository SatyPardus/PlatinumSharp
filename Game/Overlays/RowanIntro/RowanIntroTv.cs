using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public sealed class RowanIntroTvData
{
    public int HeapID;
    public BgConfig? BgConfig;
    public MessageLoader? MsgLoader;
    public int State;
    public readonly Window Window = new();
    public int CrtScroll;
    public int Delay;
}

public static class RowanIntroTv
{
    private const int Overlay = 73;
    private const int HeapID = 0x53;
    private const uint HeapSize = 0x40000;
    private const uint BanksAddress = 0x021D3AC0;
    private const uint GraphicsModesAddress = 0x021D3A40;
    private const uint Bg0TemplateAddress = 0x021D3A88;
    private const uint Bg1TemplateAddress = 0x021D3A50;
    private const uint Bg2TemplateAddress = 0x021D3A6C;
    private const uint Bg3TemplateAddress = 0x021D3AA4;
    private const uint WindowTemplateAddress = 0x021D3A38;
    private const uint AllLayersMask = 0xFFFFE0FF;
    private const int TvArchive = 0x88;
    private const int MessageArchive = 0x1A;
    private const int MessageBank = 0x25F;
    private const int TvBgm = 0x406;
    private const int KeyA = 1;
    private const int KeyB = 2;
    private const int DisplayMain = 0;

    private static BgTemplate BgTemplateAt(uint address)
    {
        return RomStructs.BgTemplate(RomData.Overlay(Overlay, address, RomStructs.BgTemplateSize), 0);
    }

    // OFFSET 0x021D3250
    public static int RowanIntroTv_Init(ApplicationManager appMan, ref int state)
    {
        Heap.Heap_Create(3, HeapID, HeapSize);
        RowanIntroTvData data = new();
        appMan.Data = data;
        data.HeapID = HeapID;
        data.Delay = 0;
        return 1;
    }

    // OFFSET 0x021D3280
    public static int RowanIntroTv_Main(ApplicationManager appMan, ref int state)
    {
        RowanIntroTvData data = (RowanIntroTvData)ApplicationManager.ApplicationManager_Data(appMan)!;
        int finished = 0;
        switch (state)
        {
            case 0:
            {
                ScreenFadeControl.SetScreenColorBrightness(0, 0);
                ScreenFadeControl.SetScreenColorBrightness(1, 0);
                GameSystem.SetVBlankCallback(null, null);
                GameSystem.SetHBlankCallback(null, null);
                GXLayers.GXLayers_DisableEngineALayers();
                GXLayers.GXLayers_DisableEngineBLayers();
                Video.SetDisplayControl(0, Video.GetDisplayControl(0) & AllLayersMask);
                Video.SetDisplayControl(1, Video.GetDisplayControl(1) & AllLayersMask);
                GameSystem.SetAutorepeat(4, 8);
                RowanIntroTv_InitGraphics(data);
                RowanIntroTv_InitMessageStructs(data);
                GameSystem.SetVBlankCallback(RowanIntroTv_VBlankCallback, data);
                GXLayers.GXLayers_TurnBothDispOn();
                ushort[] palette = HardwarePalette.GetHardwareMainBgPaletteAddress();
                PaletteData.BlendPalettes(palette, palette, 0xFFFC, 7, 0);
                Video.LoadPalette(Video.MainBackground, palette, 0x200);
                data.Delay = 0x3C;
                state = 1;
                break;
            }
            case 1:
                if (data.Delay != 0)
                {
                    data.Delay--;
                }
                else
                {
                    Sound.Sound_SetFieldBGM(TvBgm);
                    Sound.Sound_SetSceneAndPlayBGM(4, TvBgm, 1);
                    data.Delay = 0x5A;
                    state = 2;
                }
                break;
            case 2:
                if (data.Delay != 0)
                {
                    data.Delay--;
                }
                else
                {
                    data.Delay = 0;
                    ScreenFadeControl.StartScreenFade(0, 1, 1, 0, 6, 1, data.HeapID);
                    state = 3;
                }
                break;
            case 3:
                RowanIntroTv_ShiftCrtOverlay(data);
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    state = 4;
                }
                break;
            case 4:
                RowanIntroTv_ShiftCrtOverlay(data);
                if (RowanIntroTv_Run(data, 0, 0x28, 0x30))
                {
                    ScreenFadeControl.StartScreenFade(0, 0, 0, 0, 6, 1, data.HeapID);
                    state = 5;
                }
                break;
            case 5:
                RowanIntroTv_ShiftCrtOverlay(data);
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    RowanIntroTv_FreeMessageStructs(data);
                    RowanIntroTv_FreeGraphics(data);
                    GameSystem.SetVBlankCallback(null, null);
                    finished = 1;
                }
                break;
        }
        return finished;
    }

    // OFFSET 0x021D3404
    public static int RowanIntroTv_Exit(ApplicationManager appMan, ref int state)
    {
        RowanIntroTvData data = (RowanIntroTvData)ApplicationManager.ApplicationManager_Data(appMan)!;
        int heapID = data.HeapID;
        ApplicationManager.ApplicationManager_FreeData(appMan);
        Heap.Heap_Destroy(heapID);
        return 1;
    }

    // OFFSET 0x021D3420
    private static void RowanIntroTv_VBlankCallback(object? param)
    {
        RowanIntroTvData data = (RowanIntroTvData)param!;
        Bg.Bg_RunScheduledUpdates(data.BgConfig!);
    }

    // OFFSET 0x021D342C
    private static void RowanIntroTv_InitGraphics(RowanIntroTvData data)
    {
        GXLayers.GXLayers_SetBanks(RomStructs.Banks(RomData.Overlay(Overlay, BanksAddress, RomStructs.BanksSize), 0));
        data.BgConfig = Bg.BgConfig_New(data.HeapID);
        Bg.SetAllGraphicsModes(RomStructs.GraphicsModes(RomData.Overlay(Overlay, GraphicsModesAddress, RomStructs.GraphicsModesSize), 0));
        Bg.Bg_InitFromTemplate(data.BgConfig, 2, BgTemplateAt(Bg2TemplateAddress), 0);
        Bg.Bg_ClearTilesRange(2, 0x20, 0, data.HeapID);
        Bg.Bg_ClearTilemap(data.BgConfig, 2);
        Bg.Bg_InitFromTemplate(data.BgConfig, 0, BgTemplateAt(Bg0TemplateAddress), 0);
        Graphics.Graphics_LoadTilesToBgLayer(TvArchive, 1, data.BgConfig, 0, 0, 0, 0, data.HeapID);
        Graphics.Graphics_LoadTilemapToBgLayer(TvArchive, 4, data.BgConfig, 0, 0, 0, 0, data.HeapID);
        Bg.Bg_InitFromTemplate(data.BgConfig, 1, BgTemplateAt(Bg1TemplateAddress), 0);
        Graphics.Graphics_LoadTilesToBgLayer(TvArchive, 2, data.BgConfig, 1, 0, 0, 0, data.HeapID);
        Graphics.Graphics_LoadTilemapToBgLayer(TvArchive, 5, data.BgConfig, 1, 0, 0, 0, data.HeapID);
        Bg.Bg_InitFromTemplate(data.BgConfig, 3, BgTemplateAt(Bg3TemplateAddress), 0);
        Graphics.Graphics_LoadTilesToBgLayer(TvArchive, 8, data.BgConfig, 3, 0, 0, 0, data.HeapID);
        Graphics.Graphics_LoadTilemapToBgLayer(TvArchive, 7, data.BgConfig, 3, 0, 0, 0, data.HeapID);
        Graphics.Graphics_LoadPalette(TvArchive, 6, 0, 0, 0, data.HeapID);
        Graphics.Graphics_LoadPaletteWithSrcOffset(TvArchive, 9, 0, 0x40, 0x40, 0x1C0, data.HeapID);
        Font.Font_LoadTextPalette(0, 0x20, data.HeapID);
        Bg.Bg_MaskPalette(0, 0);
        Bg.Bg_MaskPalette(4, 0);
        Video.SetBlendAlpha(DisplayMain, 2, 0xC, 4, 0xC);
    }

    // OFFSET 0x021D35F4
    private static void RowanIntroTv_FreeGraphics(RowanIntroTvData data)
    {
        Bg.Bg_ToggleLayer(0, 0);
        Bg.Bg_ToggleLayer(1, 0);
        Bg.Bg_ToggleLayer(2, 0);
        Bg.Bg_ToggleLayer(3, 0);
        Bg.Bg_ToggleLayer(4, 0);
        Bg.Bg_ToggleLayer(5, 0);
        Bg.Bg_ToggleLayer(6, 0);
        Bg.Bg_ToggleLayer(7, 0);
        Video.SetBlendControl(DisplayMain, 0);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 3);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 1);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 0);
        Bg.Bg_FreeTilemapBuffer(data.BgConfig!, 2);
        data.BgConfig = null;
    }

    // OFFSET 0x021D366C
    private static void RowanIntroTv_InitMessageStructs(RowanIntroTvData data)
    {
        data.MsgLoader = MessageLoader.MessageLoader_Init(1, MessageArchive, MessageBank, data.HeapID);
        Text.Text_ResetAllPrinters();
        data.State = 0;
    }

    // OFFSET 0x021D368C
    private static void RowanIntroTv_FreeMessageStructs(RowanIntroTvData data)
    {
        MessageLoader.MessageLoader_Free(data.MsgLoader);
    }

    // OFFSET 0x021D3698
    private static bool RowanIntroTv_Run(RowanIntroTvData data, int messageID, int unused2, int unused3)
    {
        bool finished = false;
        switch (data.State)
        {
            case 0:
            {
                Bg.Bg_ToggleLayer(2, 0);
                GameString text = GameString.String_Init(0x400, data.HeapID);
                MessageLoader.MessageLoader_GetString(data.MsgLoader!, (uint)messageID, text);
                Bg.Window_AddFromTemplate(data.BgConfig!, data.Window, RomStructs.WindowTemplate(RomData.Overlay(Overlay, WindowTemplateAddress, RomStructs.WindowTemplateSize), 0));
                Bg.Window_FillRectWithColor(data.Window, 0, 0, 0, 0x100, 0xC0);
                uint x = (uint)(0x100 - Font.Font_CalcMaxLineWidth(0, text, 0)) >> 1;
                uint y = (0xC0 - (GameString.String_NumLines(text) << 4)) >> 1;
                Text.Text_AddPrinterWithParamsAndColor(data.Window, 0, text, x, y, 0, 0x000F0200, null);
                GameString.String_Free(text);
                Bg.Window_CopyToVRAM(data.Window);
                Bg.Bg_ToggleLayer(2, 1);
                data.Delay = 0xF0;
                data.State = 1;
                break;
            }
            case 1:
                if (data.Delay != 0)
                {
                    data.Delay--;
                }
                else
                {
                    data.State = 2;
                }
                break;
            case 2:
                if ((gSystem.PressedKeys & KeyA) == KeyA || (gSystem.PressedKeys & KeyB) == KeyB)
                {
                    data.State = 3;
                }
                break;
            case 3:
                Bg.Window_Remove(data.Window);
                Bg.Bg_ClearTilemap(data.BgConfig!, 2);
                data.State = 0;
                finished = true;
                break;
        }
        return finished;
    }

    // OFFSET 0x021D37AC
    private static void RowanIntroTv_ShiftCrtOverlay(RowanIntroTvData data)
    {
        data.CrtScroll += 4;
        Bg.Bg_SetOffset(data.BgConfig!, 1, Bg.OffsetSetY, data.CrtScroll >> 4);
    }
}
