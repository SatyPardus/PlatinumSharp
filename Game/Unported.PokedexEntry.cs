namespace PlatinumSharp.Game;

public sealed class PokedexEntryScreenParams
{
    public BgConfig? BgConfig;
    public PaletteData? PaletteData;
    public PokemonSpriteManager? MonSpriteMan;
    public int HeapID;
    public Pokemon? Mon;
    public int DexMode;
}

public static partial class Unported
{
    public static object? PokedexEntryScreen_New(PokedexEntryScreenParams screenParams) => throw Missing(0x021E8D48);
    public static void PokedexEntryScreen_Free(object? screen) => throw Missing(0x021E8DD0);
    public static bool PokedexEntryScreen_IsReady(object? screen) => throw Missing(0x021E8DEC);
    public static PokemonSprite PokedexEntryScreen_GetSprite(object? screen) => throw Missing(0x021E8E00);
    public static void PokedexEntryScreen_StartExit(object? screen, bool exit) => throw Missing(0x021E8E04);
}
