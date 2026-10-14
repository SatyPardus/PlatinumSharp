namespace PlatinumSharp.Game;

public static class PokemonIcon
{
    private const int SpeciesUnown = 201;
    private const int SpeciesDeoxys = 386;
    private const int SpeciesBurmy = 412;
    private const int SpeciesWormadam = 413;
    private const int SpeciesShellos = 422;
    private const int SpeciesGastrodon = 423;
    private const int SpeciesRotom = 479;
    private const int SpeciesGiratina = 487;
    private const int SpeciesManaphy = 490;
    private const int SpeciesShaymin = 492;
    private const int SpeciesLast = 493;
    private const int IconFirstSpecies = 7;
    private const int IconEgg = 0x1F5;
    private const int IconManaphyEgg = 0x1F6;
    private const int IconDeoxysForms = 0x1F6;
    private const int IconUnownForms = 0x1FA;
    private const int IconBurmyForms = 0x215;
    private const int IconWormadamForms = 0x217;
    private const int IconShellosForms = 0x219;
    private const int IconGastrodonForms = 0x21A;
    private const int IconGiratinaForms = 0x21B;
    private const int IconShayminForms = 0x21C;
    private const int IconRotomForms = 0x21D;
    private const int PaletteEgg = 0x1EE;
    private const int PaletteManaphyEgg = 0x1EF;
    private const int PaletteDeoxysForms = 0x1EF;
    private const int PaletteUnownForms = 0x1F2;
    private const int PaletteBurmyForms = 0x20E;
    private const int PaletteWormadamForms = 0x210;
    private const int PaletteShellosForms = 0x212;
    private const int PaletteGastrodonForms = 0x213;
    private const int PaletteGiratinaForms = 0x214;
    private const int PaletteShayminForms = 0x215;
    private const int PaletteRotomForms = 0x216;
    private const int PaletteTableSize = 0x21C;
    private const uint PaletteTableAddress = 0x020F0780;
    private const int ParamSpecies = 5;
    private const int ParamIsEgg = 76;
    private const int ParamForm = 112;
    private const int ParamSpeciesEgg = 174;

    private static byte[]? paletteIndices;

    // OFFSET 0x02079D40
    public static uint BoxPokemon_IconSpriteIndex(BoxPokemon boxMon)
    {
        int entered = Pokemon.BoxPokemon_EnterDecryptionContext(boxMon);
        uint species = Pokemon.BoxPokemon_GetValue(boxMon, ParamSpecies, null);
        uint isEgg = Pokemon.BoxPokemon_GetValue(boxMon, ParamIsEgg, null);
        uint form = BoxPokemon_IconFormOffset(boxMon);
        uint index = PokeIconSpriteIndex(species, isEgg, form);
        Pokemon.BoxPokemon_ExitDecryptionContext(boxMon, entered);
        return index;
    }

    // OFFSET 0x02079D80
    public static uint Pokemon_IconSpriteIndex(Pokemon mon)
    {
        return BoxPokemon_IconSpriteIndex(Pokemon.Pokemon_GetBoxPokemon(mon));
    }

    // OFFSET 0x02079D8C
    public static uint PokeIconSpriteIndex(uint species, uint isEgg, uint form)
    {
        if (isEgg == 1)
        {
            if (species == SpeciesManaphy)
            {
                return IconManaphyEgg;
            }
            return IconEgg;
        }
        form = (uint)PokemonSpriteTemplates.Pokemon_SanitizeFormId((ushort)species, (byte)form);
        if (form != 0)
        {
            switch (species)
            {
                case SpeciesDeoxys:
                    return form + IconDeoxysForms;
                case SpeciesUnown:
                    return form + IconUnownForms;
                case SpeciesBurmy:
                    return form + IconBurmyForms;
                case SpeciesWormadam:
                    return form + IconWormadamForms;
                case SpeciesShellos:
                    return form + IconShellosForms;
                case SpeciesGastrodon:
                    return form + IconGastrodonForms;
                case SpeciesGiratina:
                    return form + IconGiratinaForms;
                case SpeciesShaymin:
                    return form + IconShayminForms;
                case SpeciesRotom:
                    return form + IconRotomForms;
            }
        }
        if (species > SpeciesLast)
        {
            species = 0;
        }
        return species + IconFirstSpecies;
    }

    // OFFSET 0x02079E44
    public static uint BoxPokemon_IconFormOffset(BoxPokemon boxMon)
    {
        switch (Pokemon.BoxPokemon_GetValue(boxMon, ParamSpeciesEgg, null))
        {
            case SpeciesUnown:
                return Pokemon.BoxPokemon_GetForm(boxMon);
            case SpeciesDeoxys:
            case SpeciesBurmy:
            case SpeciesWormadam:
            case SpeciesShellos:
            case SpeciesGastrodon:
            case SpeciesRotom:
            case SpeciesGiratina:
            case SpeciesShaymin:
                return (ushort)Pokemon.BoxPokemon_GetValue(boxMon, ParamForm, null);
        }
        return 0;
    }

    // OFFSET 0x02079EDC
    public static uint PokeIconPaletteIndex(uint species, uint form, uint isEgg)
    {
        if (isEgg == 1)
        {
            if (species == SpeciesManaphy)
            {
                species = PaletteManaphyEgg;
            }
            else
            {
                species = PaletteEgg;
            }
        }
        else if (species > SpeciesLast)
        {
            species = 0;
        }
        else if (form != 0)
        {
            switch (species)
            {
                case SpeciesDeoxys:
                    species = form + PaletteDeoxysForms;
                    break;
                case SpeciesUnown:
                    species = form + PaletteUnownForms;
                    break;
                case SpeciesBurmy:
                    species = form + PaletteBurmyForms;
                    break;
                case SpeciesWormadam:
                    species = form + PaletteWormadamForms;
                    break;
                case SpeciesShellos:
                    species = form + PaletteShellosForms;
                    break;
                case SpeciesGastrodon:
                    species = form + PaletteGastrodonForms;
                    break;
                case SpeciesGiratina:
                    species = form + PaletteGiratinaForms;
                    break;
                case SpeciesShaymin:
                    species = form + PaletteShayminForms;
                    break;
                case SpeciesRotom:
                    species = form + PaletteRotomForms;
                    break;
            }
        }
        paletteIndices ??= RomData.Arm9(PaletteTableAddress, PaletteTableSize);
        return paletteIndices[species];
    }

    // OFFSET 0x02079F84
    public static uint BoxPokemon_IconPaletteIndex(BoxPokemon boxMon)
    {
        int entered = Pokemon.BoxPokemon_EnterDecryptionContext(boxMon);
        uint form = BoxPokemon_IconFormOffset(boxMon);
        uint species = Pokemon.BoxPokemon_GetValue(boxMon, ParamSpecies, null);
        uint isEgg = Pokemon.BoxPokemon_GetValue(boxMon, ParamIsEgg, null);
        Pokemon.BoxPokemon_ExitDecryptionContext(boxMon, entered);
        return PokeIconPaletteIndex(species, form, isEgg);
    }

    // OFFSET 0x02079FC4
    public static uint Pokemon_IconPaletteIndex(Pokemon mon)
    {
        return BoxPokemon_IconPaletteIndex(Pokemon.Pokemon_GetBoxPokemon(mon));
    }

    // OFFSET 0x02079FD0
    public static uint PokeIconPalettesFileIndex()
    {
        return 0;
    }

    // OFFSET 0x02079FD4
    public static uint PokeIconCellsFileIndex()
    {
        return 2;
    }

    // OFFSET 0x02079FD8
    public static uint PokeIcon32KCellsFileIndex()
    {
        return 4;
    }

    // OFFSET 0x02079FDC
    public static uint PokeIcon64KCellsFileIndex()
    {
        return 6;
    }

    // OFFSET 0x02079FE0
    public static uint PokeIconAnimationFileIndex()
    {
        return 1;
    }

    // OFFSET 0x02079FE4
    public static uint PokeIcon32KAnimationFileIndex()
    {
        return 3;
    }

    // OFFSET 0x02079FE8
    public static uint PokeIcon64KAnimationFileIndex()
    {
        return 5;
    }
}
