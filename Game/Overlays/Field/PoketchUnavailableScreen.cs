namespace PlatinumSharp.Game;

public static class PoketchUnavailableScreen
{
    private const int OverlayID = 24;
    private const uint BgTemplateAddress = 0x02253DB8;
    private const int HeapIDParent = 3;
    private const int HeapIDPoketch = 8;
    private const uint HeapSize = 0x18000;
    private const int DisplaySub = 1;
    private const uint VramSubBg = 0x80;
    private const uint VramSubObj = 0x100;
    private const int BankSubBg = 2;
    private const int BankSubObj = 6;
    private const int BankCount = 10;
    private const uint ObjMappingMask = 0xFFCFFFEF;
    private const uint ObjMapping1D32K = 0x10;
    private const uint VisibleWindowMask = 0xFFFF1FFF;
    private const uint VisiblePlaneMask = 0xFFFFE0FF;
    private const uint VisiblePlaneBg0 = 0x100;
    private const int BgLayerSub0 = 4;
    private const int NarcPoketchGraphics = 0xC;
    private const uint NarcMemberTiles = 10;
    private const uint NarcMemberTilemap = 11;
    private const uint NarcMemberPalette = 12;
    private const int PaletteLoadSubBg = 4;
    private const uint PaletteSize = 0x20;

    // OFFSET 0x02253CE0
    public static void PoketchUnavailableScreen_Init(object bgConfig)
    {
        Heap.Heap_Create(HeapIDParent, HeapIDPoketch, HeapSize);
        Video.SetSubGraphicsMode(0);
        uint[] banks = new uint[BankCount];
        for (int i = 0; i < BankCount; i++)
        {
            banks[i] = Video.Banks[i];
        }
        banks[BankSubBg] = VramSubBg;
        banks[BankSubObj] = VramSubObj;
        Video.SetBanks(banks);
        Video.SetDisplayControl(DisplaySub, (Video.GetDisplayControl(DisplaySub) & ObjMappingMask) | ObjMapping1D32K);
        BgTemplate template = RomStructs.BgTemplate(RomData.Overlay(OverlayID, BgTemplateAddress, RomStructs.BgTemplateSize), 0);
        Bg.Bg_InitFromTemplate(bgConfig, BgLayerSub0, template, 0);
        NARC narc = NARC.NARC_ctor(NarcPoketchGraphics, HeapIDPoketch);
        Graphics.Graphics_LoadTilesToBgLayerFromOpenNARC(narc, NarcMemberTiles, bgConfig, BgLayerSub0, 0, 0, 1, HeapIDPoketch);
        Graphics.Graphics_LoadTilemapToBgLayerFromOpenNARC(narc, NarcMemberTilemap, bgConfig, BgLayerSub0, 0, 0, 1, HeapIDPoketch);
        Graphics.Graphics_LoadPaletteFromOpenNARC(narc, NarcMemberPalette, PaletteLoadSubBg, 0, PaletteSize, HeapIDPoketch);
        NARC.NARC_dtor(narc);
        Video.SetDisplayControl(DisplaySub, Video.GetDisplayControl(DisplaySub) & VisibleWindowMask);
        Video.SetDisplayControl(DisplaySub, (Video.GetDisplayControl(DisplaySub) & VisiblePlaneMask) | VisiblePlaneBg0);
    }

    // OFFSET 0x02253DA4
    public static void PoketchUnavailableScreen_Exit(object bgConfig)
    {
        Bg.Bg_FreeTilemapBuffer(bgConfig, BgLayerSub0);
        Heap.Heap_Destroy(HeapIDPoketch);
    }

    // OFFSET 0x02253DB4
    public static int PoketchUnavailableScreen_IsDone(object bgConfig)
    {
        return 1;
    }
}
