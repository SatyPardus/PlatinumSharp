namespace PlatinumSharp.Game;

public sealed partial class Pokemon
{
    private const uint FlavorAffinityTable = 0x020F0695;
    private const int FlavorCount = 5;
    private const int SpeciesGiratina = 0x1E7;
    private const int ItemGriseousOrb = 0x70;
    private const int MonDataSpeciesParam = 5;
    private const int MonDataHeldItemParam = 6;
    private const int MonDataFormParam = 0x70;

    private static byte[]? flavorAffinities;

    // OFFSET 0x020775EC
    public static void Pokemon_Copy(Pokemon src, Pokemon dst)
    {
        dst.CopyFrom(0, src.Buffer, src.Offset, Size);
    }

    // OFFSET 0x02077648
    public static sbyte Pokemon_GetFlavorAffinityOf(uint personality, int flavor)
    {
        flavorAffinities ??= RomData.Arm9(FlavorAffinityTable, NatureCount * FlavorCount);
        return (sbyte)flavorAffinities[Personality_GetNature(personality) * FlavorCount + flavor];
    }

    // OFFSET 0x02077A00
    public static int Pokemon_SetGiratinaFormByHeldItem(Pokemon mon)
    {
        int form = BoxPokemon_SetGiratinaFormByHeldItem(mon.Box);
        if (form != -1)
        {
            Pokemon_CalcLevelAndStats(mon);
        }
        return form;
    }

    // OFFSET 0x02077A1C
    public static int BoxPokemon_SetGiratinaFormByHeldItem(BoxPokemon boxMon)
    {
        uint species = BoxPokemon_GetValue(boxMon, MonDataSpeciesParam, null);
        uint item = BoxPokemon_GetValue(boxMon, MonDataHeldItemParam, null);
        if (species == SpeciesGiratina)
        {
            int form = item == ItemGriseousOrb ? 1 : 0;
            BoxPokemon_SetValue(boxMon, MonDataFormParam, form);
            BoxPokemon_CalcAbility(boxMon);
            return form;
        }
        return -1;
    }
}
