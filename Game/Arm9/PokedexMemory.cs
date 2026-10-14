namespace PlatinumSharp.Game;

public sealed class PokedexMemory
{
    public const int Size = 8;
    public const uint BootModeDefault = 2;

    public uint CurrentSpecies;
    public uint BootMode;

    // OFFSET 0x0209ACBC
    public static PokedexMemory PokedexMemory_New(int heapID)
    {
        PokedexMemory pokedexMemory = new();
        pokedexMemory.CurrentSpecies = 0;
        pokedexMemory.BootMode = 0;
        pokedexMemory.BootMode = BootModeDefault;
        return pokedexMemory;
    }

    // OFFSET 0x0209ACDC
    public static void PokedexMemory_Free(PokedexMemory pokedexMemory)
    {
    }

    // OFFSET 0x0209ACE4
    public static uint PokedexMemory_GetCurrentSpecies(PokedexMemory pokedexMemory)
    {
        return pokedexMemory.CurrentSpecies;
    }

    // OFFSET 0x0209ACE8
    public static void PokedexMemory_SetCurrentSpecies(PokedexMemory pokedexMemory, uint species)
    {
        pokedexMemory.CurrentSpecies = species;
    }

    // OFFSET 0x0209ACEC
    public static uint PokedexMemory_GetBootMode(PokedexMemory pokedexMemory)
    {
        return pokedexMemory.BootMode;
    }

    // OFFSET 0x0209ACF0
    public static void PokedexMemory_SetBootMode(PokedexMemory pokedexMemory, uint bootMode)
    {
        pokedexMemory.BootMode = bootMode;
    }
}
