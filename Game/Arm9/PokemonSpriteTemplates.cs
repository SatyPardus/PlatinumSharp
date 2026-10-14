namespace PlatinumSharp.Game;

public static class PokemonSpriteTemplates
{
    private const int ArchivePokemonSprites = 4;
    private const int ArchiveOtherPokemonSprites = 0x75;
    private const int SpeciesUnown = 201;
    private const int SpeciesSpinda = 327;
    private const int SpeciesCastform = 351;
    private const int SpeciesDeoxys = 386;
    private const int SpeciesBurmy = 412;
    private const int SpeciesWormadam = 413;
    private const int SpeciesCherrim = 421;
    private const int SpeciesShellos = 422;
    private const int SpeciesGastrodon = 423;
    private const int SpeciesRotom = 479;
    private const int SpeciesGiratina = 487;
    private const int SpeciesShaymin = 492;
    private const int SpeciesArceus = 493;
    private const int SpeciesEgg = 494;
    private const int SpeciesBadEgg = 495;
    private const int GenderFemale = 1;
    private const int FaceFront = 2;

    private static void Set(PokemonSpriteTemplate template, int archive, int character, int palette)
    {
        template.Word0 = (uint)(archive & 0xFFFF) | ((uint)(character & 0xFFFF) << 16);
        template.Word1 = (template.Word1 & 0xFFFF0000) | (uint)(palette & 0xFFFF);
    }

    private const int ParamPersonality = 0;
    private const int ParamSpecies = 5;
    private const int ParamForm = 0x70;
    private const int ParamSpeciesOrEgg = 0xAE;
    private const int SpeciesManaphy = 490;

    // OFFSET 0x02075EF4
    public static void Pokemon_BuildSpriteTemplate(PokemonSpriteTemplate template, Pokemon mon, int face)
    {
        BoxPokemon_BuildSpriteTemplate(template, Pokemon.Pokemon_GetBoxPokemon(mon), face, 0);
    }

    // OFFSET 0x02075F00
    public static void Pokemon_BuildSpriteTemplateDP(PokemonSpriteTemplate template, Pokemon mon, int face)
    {
        BoxPokemon_BuildSpriteTemplate(template, Pokemon.Pokemon_GetBoxPokemon(mon), face, 1);
    }

    // OFFSET 0x02075F0C
    public static void BoxPokemon_BuildSpriteTemplate(PokemonSpriteTemplate template, BoxPokemon boxMon, int face, int preferDP)
    {
        int decryptionContext = Pokemon.BoxPokemon_EnterDecryptionContext(boxMon);
        ushort species = (ushort)Pokemon.BoxPokemon_GetValue(boxMon, ParamSpeciesOrEgg, null);
        byte gender = Pokemon.BoxPokemon_GetGender(boxMon);
        byte shiny = Pokemon.BoxPokemon_IsShiny(boxMon);
        uint personality = Pokemon.BoxPokemon_GetValue(boxMon, ParamPersonality, null);
        byte form;
        if (species == SpeciesEgg)
        {
            form = (byte)(Pokemon.BoxPokemon_GetValue(boxMon, ParamSpecies, null) == SpeciesManaphy ? 1 : 0);
        }
        else
        {
            form = (byte)Pokemon.BoxPokemon_GetValue(boxMon, ParamForm, null);
        }
        if (preferDP == 1)
        {
            Unported.sub_02076300(template, species, gender, face, shiny, form, personality);
        }
        else
        {
            BuildPokemonSpriteTemplate(template, species, gender, face, shiny, form, personality);
        }
        Pokemon.BoxPokemon_ExitDecryptionContext(boxMon, decryptionContext);
    }

    // OFFSET 0x02075FB4
    public static void BuildPokemonSpriteTemplate(PokemonSpriteTemplate template, ushort species, byte gender, int face, int shiny, int form, uint personality)
    {
        template.Word1 &= 0x0000FFFF;
        template.Word2 &= 0xFFFFFF00;
        template.Word3 = 0;
        form = Pokemon_SanitizeFormId(species, form);
        switch (species)
        {
            case SpeciesBurmy:
                Set(template, ArchiveOtherPokemonSprites, (face >> 1) + 0x48 + form * 2, shiny + 0xA6 + form * 2);
                break;
            case SpeciesWormadam:
                Set(template, ArchiveOtherPokemonSprites, (face >> 1) + 0x4E + form * 2, shiny + 0xAC + form * 2);
                break;
            case SpeciesShellos:
                Set(template, ArchiveOtherPokemonSprites, face + 0x54 + form, shiny + 0xB2 + form * 2);
                break;
            case SpeciesGastrodon:
                Set(template, ArchiveOtherPokemonSprites, face + 0x58 + form, shiny + 0xB6 + form * 2);
                break;
            case SpeciesCherrim:
                Set(template, ArchiveOtherPokemonSprites, face + 0x5C + form, shiny * 2 + 0xBA + form);
                break;
            case SpeciesArceus:
                Set(template, ArchiveOtherPokemonSprites, (face >> 1) + 0x60 + form * 2, shiny + 0xBE + form * 2);
                break;
            case SpeciesCastform:
                Set(template, ArchiveOtherPokemonSprites, face * 2 + 0x40 + form, shiny * 4 + 0x9E + form);
                break;
            case SpeciesDeoxys:
                Set(template, ArchiveOtherPokemonSprites, (face >> 1) + form * 2, shiny + 0x9A);
                break;
            case SpeciesUnown:
                Set(template, ArchiveOtherPokemonSprites, (face >> 1) + 8 + form * 2, shiny + 0x9C);
                break;
            case SpeciesEgg:
                Set(template, ArchiveOtherPokemonSprites, form + 0x84, form + 0xE2);
                break;
            case SpeciesBadEgg:
                Set(template, ArchiveOtherPokemonSprites, 0x84, 0xE2);
                break;
            case SpeciesShaymin:
                Set(template, ArchiveOtherPokemonSprites, (face >> 1) + 0x86 + form * 2, shiny + 0xE4 + form * 2);
                break;
            case SpeciesRotom:
                Set(template, ArchiveOtherPokemonSprites, (face >> 1) + 0x8A + form * 2, shiny + 0xE8 + form * 2);
                break;
            case SpeciesGiratina:
                Set(template, ArchiveOtherPokemonSprites, (face >> 1) + 0x96 + form * 2, shiny + 0xF4 + form * 2);
                break;
            default:
                Set(template, ArchivePokemonSprites, face + species * 6 + (gender != GenderFemale ? 1 : 0), shiny + species * 6 + 4);
                if (species == SpeciesSpinda && face == FaceFront)
                {
                    template.Word1 = (template.Word1 & 0x0000FFFF) | ((uint)SpeciesSpinda << 16);
                    template.Word2 &= 0xFFFFFF00;
                    template.Word3 = personality;
                }
                break;
        }
    }

    // OFFSET 0x020761E8
    public static int Pokemon_SanitizeFormId(ushort species, int form)
    {
        switch (species)
        {
            case SpeciesBurmy:
            case SpeciesWormadam:
                if ((uint)form > 2)
                {
                    form = 0;
                }
                break;
            case SpeciesShellos:
            case SpeciesGastrodon:
            case SpeciesCherrim:
            case SpeciesEgg:
            case SpeciesShaymin:
            case SpeciesGiratina:
                if ((uint)form > 1)
                {
                    form = 0;
                }
                break;
            case SpeciesArceus:
                if ((uint)form > 0x11)
                {
                    form = 0;
                }
                break;
            case SpeciesCastform:
            case SpeciesDeoxys:
                if ((uint)form > 3)
                {
                    form = 0;
                }
                break;
            case SpeciesUnown:
                if ((uint)form >= 0x1C)
                {
                    form = 0;
                }
                break;
            case SpeciesRotom:
                if ((uint)form > 5)
                {
                    form = 0;
                }
                break;
        }
        return form;
    }
}
