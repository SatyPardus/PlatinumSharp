namespace PlatinumSharp.Game;

public sealed partial class Poketch
{
    private const int HistorySpeciesOffset = 0;
    private const int HistoryIconOffset = 2;
    private const int HistoryFormOffset = 4;
    private const int MonParamSpecies = 5;
    private const int MonParamForm = 112;

    // OFFSET 0x02056A48
    public static void Poketch_PokemonHistoryEnqueue(Poketch poketch, BoxPokemon boxMon)
    {
        int index = Poketch_PokemonHistorySize(poketch);
        if (index >= HistoryCount)
        {
            for (int i = 0; i < HistoryCount - 1; i++)
            {
                poketch.W32(HistoryOffset + i * HistoryEntrySize, poketch.U32(HistoryOffset + (i + 1) * HistoryEntrySize));
                poketch.W32(HistoryOffset + i * HistoryEntrySize + HistoryFormOffset, poketch.U32(HistoryOffset + (i + 1) * HistoryEntrySize + HistoryFormOffset));
            }
            index = HistoryCount - 1;
        }
        int entry = HistoryOffset + index * HistoryEntrySize;
        poketch.W16(entry + HistorySpeciesOffset, (ushort)Pokemon.BoxPokemon_GetValue(boxMon, MonParamSpecies, null));
        poketch.W16(entry + HistoryIconOffset, (ushort)PokemonIcon.BoxPokemon_IconFormOffset(boxMon));
        poketch.W32(entry + HistoryFormOffset, Pokemon.BoxPokemon_GetValue(boxMon, MonParamForm, null));
    }

    // OFFSET 0x02056AAC
    public static int Poketch_PokemonHistorySize(Poketch poketch)
    {
        int count;
        for (count = 0; count < HistoryCount; count++)
        {
            if (poketch.U16(HistoryOffset + count * HistoryEntrySize) == 0)
            {
                return count;
            }
        }
        return count;
    }

    // OFFSET 0x02056AC8
    public static void Poketch_PokemonHistorySpeciesAndIcon(Poketch poketch, int index, out int species, out int icon)
    {
        if (index >= HistoryCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (poketch.U16(HistoryOffset + index * HistoryEntrySize) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        species = poketch.U16(HistoryOffset + index * HistoryEntrySize + HistorySpeciesOffset);
        icon = poketch.U16(HistoryOffset + index * HistoryEntrySize + HistoryIconOffset);
    }

    // OFFSET 0x02056AFC
    public static uint Poketch_PokemonHistoryForm(Poketch poketch, int index)
    {
        if (index >= HistoryCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (poketch.U16(HistoryOffset + index * HistoryEntrySize) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        return poketch.U32(HistoryOffset + index * HistoryEntrySize + HistoryFormOffset);
    }
}
