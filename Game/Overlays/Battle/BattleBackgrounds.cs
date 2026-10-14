using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class Battle
{
    private const int OverlayBattle = 16;
    private const uint BattleBanksAddress = 0x0226E32C;
    private const uint BattleBanksRestoreAddress = 0x0226E354;
    private const uint SubMenuBanksAddress = 0x0226E37C;
    private const uint BattleBgTemplates = 0x0226E3F8;
    private const int DisplayMain = 0;
    private const int DisplaySub = 1;
    private const int MessageBoxTileStart = 1;
    private const int MessageBoxPalette = 0xA;
    private const int BackgroundTilesMemberBase = 3;
    private const int BackgroundPaletteMemberBase = 0xAC;
    private const int BackgroundTilemapMember = 2;
    private const int NarcMessageBoxFrames = 0x26;
    private const int NarcFontPalettes = 0xE;
    private const int FontPaletteMember = 7;
    private const uint ParticleTextureVramSize = 0x8000;
    private const uint ParticlePaletteVramSize = 0xC0;

    // OFFSET 0x0223C004
    private static void BattleSystem_InitBackgrounds(BattleSystem battleSys, BgConfig bgConfig)
    {
        GXLayers.GXLayers_DisableEngineALayers();
        ScreenFadeControl.ResetScreenMasterBrightness(DisplayMain);
        ScreenFadeControl.ResetScreenMasterBrightness(DisplaySub);
        GXLayers.GXLayers_SetBanks(RomStructs.Banks(RomData.Overlay(OverlayBattle, BattleBanksAddress, RomStructs.BanksSize), 0));
        Video.ClearBgVram(DisplayMain);
        Video.ClearBgVram(DisplaySub);
        Video.ClearObjectTiles(DisplayMain);
        Video.ClearObjectTiles(DisplaySub);
        GraphicsModes modes = new() { DisplayMode = 1, MainBgMode = 0, SubBgMode = 0, Bg0As2DOr3D = 1 };
        Bg.SetAllGraphicsModes(modes);
        battleSys.PendingBlendReset = 1;
        byte[] templates = RomData.Overlay(OverlayBattle, BattleBgTemplates, RomStructs.BgTemplateSize * 3);
        for (int layer = 1; layer <= 3; layer++)
        {
            Bg.Bg_InitFromTemplate(bgConfig, layer, RomStructs.BgTemplate(templates, (layer - 1) * RomStructs.BgTemplateSize), 0);
            Bg.Bg_ClearTilemap(bgConfig, layer);
        }
        Video.SetBgControl(0, (ushort)((Video.GetBgControl(0) & ~3) | 1));
        GXLayers.GXLayers_EngineAToggleLayers(1, 1);
        BattleSubscreens.BattleSubscreen_InitBackgrounds(bgConfig);
        int frame = BattleSystems.BattleSystem_GetOptionsFrame(battleSys);
        RenderWindow.ReplaceTransparentTiles(bgConfig, 1, MessageBoxTileStart, MessageBoxPalette, (byte)frame, HeapIDBattle);
        Graphics.Graphics_LoadTilesToBgLayer(NarcBattleGraphics, (uint)(battleSys.Background + BackgroundTilesMemberBase), bgConfig, 3, 0, 0, 1, HeapIDBattle);
        int timeOffset = BattleSystems.BattleSystem_GetBackgroundTimeOffset(battleSys);
        PaletteData.PaletteData_LoadBufferFromFileStart(battleSys.PaletteData!, NarcBattleGraphics, BackgroundPaletteMemberBase + battleSys.Background * 3 + timeOffset, HeapIDBattle, PaletteBufferMainBg, 0, 0);
        PaletteData.PaletteData_LoadBufferFromFileStart(battleSys.PaletteData!, NarcMessageBoxFrames, (int)RenderWindow.GetMessageBoxPaletteNARCMember((uint)frame), HeapIDBattle, PaletteBufferMainBg, 0x20, 0xA0);
        PaletteData.PaletteData_LoadBufferFromFileStart(battleSys.PaletteData!, NarcFontPalettes, FontPaletteMember, HeapIDBattle, PaletteBufferMainBg, 0x20, 0xB0);
        Graphics.Graphics_LoadTilemapToBgLayer(NarcBattleGraphics, BackgroundTilemapMember, bgConfig, 3, 0, 0, 1, HeapIDBattle);
        Video.SetVisibleWindows(DisplayMain, 0);
        Video.SetVisibleWindows(DisplaySub, 0);
        Video.SetWindowInside(DisplayMain, 0, 0);
        Video.SetWindowOutside(DisplayMain, 0);
        GXLayers.GXLayers_TurnBothDispOn();
        GXLayers.GXLayers_EngineAToggleLayers(0x10, 1);
        GXLayers.GXLayers_EngineBToggleLayers(0x10, 1);
        GameSystem.SetVBlankCallback(BattleSystem_VBlankCallback, battleSys);
    }

    // OFFSET 0x0223CE28
    private static void BattleSystem_InitParticleVram()
    {
        uint textureKey = Gfd.AllocTexVram(ParticleTextureVramSize, 0, 0);
        uint paletteKey = Gfd.AllocPlttVram(ParticlePaletteVramSize, 0, 0);
        if (textureKey == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (paletteKey == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        ParticleSystem.ParticleSystem_ZeroAll();
    }

    // OFFSET 0x0223CE68
    private static void BattleSystem_VBlankCallback(object? param)
    {
        BattleSystem battleSys = (BattleSystem)param!;
        if (battleSys.PendingBlendReset != 0)
        {
            battleSys.PendingBlendReset = 0;
            Video.SetBlendControl(DisplayMain, 0);
            Video.SetBlendControl(DisplaySub, 0);
        }
        if (battleSys.PendingBattleVRAMSetup != 0)
        {
            battleSys.PendingBattleVRAMSetup = 0;
            GXLayers.GXLayers_SetBanks(RomStructs.Banks(RomData.Overlay(OverlayBattle, BattleBanksRestoreAddress, RomStructs.BanksSize), 0));
        }
        if (battleSys.PendingSubMenuVRAMSetup != 0)
        {
            battleSys.PendingSubMenuVRAMSetup = 0;
            GXLayers.GXLayers_SetBanks(RomStructs.Banks(RomData.Overlay(OverlayBattle, SubMenuBanksAddress, RomStructs.BanksSize), 0));
        }
        PokemonSpriteManager.PokemonSpriteManager_UpdateCharAndPltt(battleSys.MonSpriteMan!);
        VramTransfer.VramTransfer_Process();
        SpriteSystem.SpriteSystem_TransferOam();
        PaletteData.PaletteData_CommitFadedBuffers(battleSys.PaletteData!);
        Bg.Bg_RunScheduledUpdates(battleSys.BgConfig!);
    }
}

public static partial class BattleSystems
{
    private const int LastOutdoorBackground = 5;
    private const int TimeOfDayEvening = 2;
    private const int TimeOfDayNight = 3;
    private const int TimeOfDayLateNight = 4;

    // OFFSET 0x0223EC04
    public static int BattleSystem_GetBackgroundTimeOffset(BattleSystem battleSys)
    {
        if (battleSys.Background > LastOutdoorBackground || battleSys.Background < 0)
        {
            return 0;
        }
        switch (battleSys.Time)
        {
            case TimeOfDayEvening:
                return 1;
            case TimeOfDayNight:
            case TimeOfDayLateNight:
                return 2;
        }
        return 0;
    }

    // OFFSET 0x0223EDE0
    public static int BattleSystem_GetOptionsFrame(BattleSystem battleSys)
    {
        return Options.Options_Frame(battleSys.Options!);
    }
}

public static partial class BattleSubscreens
{
    private const uint SubscreenBgTemplates = 0x02270540;
    private const int SubscreenFirstLayer = 4;
    private const int SubscreenLayerCount = 4;
    private const ushort SubscreenEmptyTile = 0x2FF;
    private const int BgOffsetSetX = 0;
    private const int BgOffsetSetY = 3;

    // OFFSET 0x02268744
    public static void BattleSubscreen_InitBackgrounds(BgConfig bgConfig)
    {
        byte[] templates = RomData.Overlay(16, SubscreenBgTemplates, RomStructs.BgTemplateSize * SubscreenLayerCount);
        for (int i = 0; i < SubscreenLayerCount; i++)
        {
            int layer = SubscreenFirstLayer + i;
            Bg.Bg_InitFromTemplate(bgConfig, layer, RomStructs.BgTemplate(templates, i * RomStructs.BgTemplateSize), 0);
            Bg.Bg_FillTilemap(bgConfig, layer, SubscreenEmptyTile);
            Bg.Bg_SetOffset(bgConfig, layer, BgOffsetSetX, 0);
            Bg.Bg_SetOffset(bgConfig, layer, BgOffsetSetY, 0);
        }
    }

    // OFFSET 0x022687A0
    public static void BattleSubscreen_FreeBackgrounds(BgConfig bgConfig)
    {
        for (int i = 0; i < SubscreenLayerCount; i++)
        {
            Bg.Bg_ToggleLayer((byte)(SubscreenFirstLayer + i), 0);
            Bg.Bg_FreeTilemapBuffer(bgConfig, SubscreenFirstLayer + i);
        }
    }
}
