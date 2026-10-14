namespace PlatinumSharp.Game;

public static partial class BattleSystems
{
    private const int SubMenuOverlayBattleAnim = 0xC;
    private const int SubMenuOverlayBattleSubMenus = 0xD;
    private const int SubMenuOverlayTrainerAI = 0xE;
    private const int SubMenuOverlayLoadAsync = 2;
    private const int SubMenuFontSubscreen = 2;
    private const int SubMenuHeapIDBattle = 5;
    private const int SubMenuNarcBattleGraphics = 7;
    private const int SubMenuNarcBattleSubscreen = 0x1B;
    private const int SubMenuRenderModeNormal = 0;
    private const int SubMenuRenderModeSubMenu = 3;

    // OFFSET 0x0223B384
    public static void BattleSystem_EnterSubMenu(BattleSystem battleSys)
    {
        BattleSubscreens.BattleSubscreen_Free(battleSys.BtlSubscreen!);
        BattleSubscreens.BattleSubscreen_FreeBackgrounds(battleSys.BgConfig!);
        battleSys.PendingSubMenuVRAMSetup = 1;
        Font.Font_Free(SubMenuFontSubscreen);
        BattleSystem_SetRenderMode(battleSys, SubMenuRenderModeSubMenu);
        if (battleSys.OverlayFlags == 0)
        {
            GameOverlay.Overlay_UnloadByID(SubMenuOverlayBattleAnim);
        }
        else
        {
            GameOverlay.Overlay_UnloadByID(SubMenuOverlayTrainerAI);
        }
        GameOverlay.Overlay_LoadByID(SubMenuOverlayBattleSubMenus, SubMenuOverlayLoadAsync);
    }

    // OFFSET 0x0223B430
    public static void BattleSystem_ExitSubMenu(BattleSystem battleSys)
    {
        GameOverlay.Overlay_UnloadByID(SubMenuOverlayBattleSubMenus);
        if (battleSys.OverlayFlags == 0)
        {
            GameOverlay.Overlay_LoadByID(SubMenuOverlayBattleAnim, SubMenuOverlayLoadAsync);
        }
        else
        {
            GameOverlay.Overlay_LoadByID(SubMenuOverlayTrainerAI, SubMenuOverlayLoadAsync);
        }
        BattleSystem_SetRenderMode(battleSys, SubMenuRenderModeNormal);
        Array.Clear(HardwarePalette.GetHardwareSubBgPaletteAddress(), 0, (int)HardwarePalette.GetHardwareSubBgPaletteSize() / 2);
        NARC battleNarc = NARC.NARC_ctor(SubMenuNarcBattleGraphics, SubMenuHeapIDBattle);
        NARC subscreenNarc = NARC.NARC_ctor(SubMenuNarcBattleSubscreen, SubMenuHeapIDBattle);
        battleSys.BtlSubscreen = BattleSubscreens.BattleSubscreen_New(battleNarc, subscreenNarc, battleSys, BattleSystem_GetTrainerGender(battleSys, BattleSystem_GetNetworkID(battleSys)), battleSys.SubscreenCursorOn);
        Font.Font_InitManager(SubMenuFontSubscreen, SubMenuHeapIDBattle);
        battleSys.PendingBattleVRAMSetup = 1;
        BattleSubscreens.BattleSubscreen_InitBackgrounds(battleSys.BgConfig!);
        GXLayers.GXLayers_EngineBToggleLayers(0x10, 1);
        BattleSubscreens.BattleSubscreen_LoadGraphics(battleSys.BtlSubscreen);
        BattleSubscreens.BattleSubscreen_LoadMenu(battleNarc, subscreenNarc, battleSys.BtlSubscreen, 0, 1, null);
        BattleSubscreens.BattleSubscreen_CreatePartyBalls(subscreenNarc, battleSys.BtlSubscreen);
        NARC.NARC_dtor(battleNarc);
        NARC.NARC_dtor(subscreenNarc);
        RenderText.TextPrinter_SetScrollArrowBaseTile(1);
        BattleSystem_SetTextRenderFlags(battleSys);
        SpriteUtil.SetSubScreenViewRect(SpriteSystem.SpriteSystem_GetRenderer(battleSys.SpriteSys), 0, 0x110000);
    }

    // OFFSET 0x0223F314
    public static void BattleSystem_SetRenderMode(BattleSystem battleSys, int renderMode)
    {
        battleSys.RenderMode = (byte)renderMode;
    }
}
