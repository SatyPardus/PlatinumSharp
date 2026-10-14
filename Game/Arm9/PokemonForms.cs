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
    private const int MonDataAbilityParam = 0xA;
    private const int MonDataStatusParam = 0xA0;
    private const int MonDataCurrentHPParam = 0xA3;
    private const int MonDataFatefulEncounterParam = 0x6E;
    private const uint FirstStagePath = 0x02100BB0;
    private const int MonDataBallCapsuleIDParam = 0xA2;
    private const int MonDataBallCapsuleParam = 0xAB;
    private const int MonDataSpeciesOrEggParam = 0xAE;
    private const int PersonalTMFlags1 = 0x1D;
    private const int FirstStageSpeciesCount = 0x1EE;
    private const uint FrontierBanlist = 0x020F05BE;
    private const int FrontierBanlistCount = 0x12;
    private const int SpeciesChansey = 0x71;
    private const int SpeciesMrMime = 0x7A;
    private const int SpeciesSnorlax = 0x8F;
    private const int SpeciesMarill = 0xB7;
    private const int SpeciesSudowoodo = 0xB9;
    private const int SpeciesWobbuffet = 0xCA;
    private const int SpeciesMantine = 0xE2;
    private const int SpeciesRoselia = 0x13B;
    private const int SpeciesChimecho = 0x166;

    // OFFSET 0x02076F84
    public static ushort Species_GetFirstStage(int species)
    {
        byte[] value = new byte[2];
        if (species >= FirstStageSpeciesCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        FsFile file = new();
        FileSystem.InitFile(file);
        FileSystem.OpenFile(file, RomData.Arm9String(FirstStagePath));
        FileSystem.SeekFile(file, species * 2, FileSystem.SeekSet);
        FileSystem.ReadFile(file, value, 0, 2);
        FileSystem.CloseFile(file);
        return (ushort)(value[0] | value[1] << 8);
    }

    // OFFSET 0x02076FD4
    public static ushort Species_GetFirstStageWithoutIncense(int species)
    {
        switch (species)
        {
            case SpeciesMarill:
            case SpeciesSudowoodo:
            case SpeciesChansey:
            case SpeciesMrMime:
            case SpeciesSnorlax:
            case SpeciesWobbuffet:
            case SpeciesMantine:
            case SpeciesRoselia:
            case SpeciesChimecho:
                return (ushort)species;
        }
        return Species_GetFirstStage(species);
    }

    // OFFSET 0x02078804
    public static bool Pokemon_IsOnBattleFrontierBanlist(ushort species)
    {
        ushort[] banned = RomData.Arm9U16(FrontierBanlist, FrontierBanlistCount);
        for (int i = 0; i < FrontierBanlistCount; i++)
        {
            if (species == banned[i])
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x02078824
    public static ushort BattleFrontier_GetBannedSpecies(int index)
    {
        if (index >= FrontierBanlistCount)
        {
            index = 0;
        }
        return RomData.Arm9U16(FrontierBanlist, FrontierBanlistCount)[index];
    }

    // OFFSET 0x02078838
    public static bool Pokemon_IsOnBattleFrontierBanlist(Pokemon mon)
    {
        return Pokemon_IsOnBattleFrontierBanlist((ushort)Pokemon_GetValue(mon, MonDataSpeciesParam, null));
    }

    private static byte[]? flavorAffinities;

    // OFFSET 0x02075A70
    public static uint Pokemon_GetExpToNextLevel(Pokemon mon)
    {
        return BoxPokemon_GetExpToNextLevel(mon.Box);
    }

    // OFFSET 0x02075A78
    public static uint BoxPokemon_GetExpToNextLevel(BoxPokemon boxMon)
    {
        ushort species = (ushort)BoxPokemon_GetValue(boxMon, MonDataSpeciesParam, null);
        ushort nextLevel = (ushort)(BoxPokemon_GetLevel(boxMon) + 1);
        uint exp = BoxPokemon_GetValue(boxMon, 8, null);
        return Species_GetExpForLevel(species, nextLevel) - exp;
    }

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

    // OFFSET 0x02077A64
    public static void Pokemon_SetGiratinaOriginForm(Pokemon mon)
    {
        int form = 1;
        if (Pokemon_GetValue(mon, 5, null) == SpeciesGiratina)
        {
            BoxPokemon boxMon = Pokemon_GetBoxPokemon(mon);
            BoxPokemon_SetValue(boxMon, 0x70, form);
            BoxPokemon_CalcAbility(boxMon);
            Pokemon_CalcLevelAndStats(mon);
        }
    }

    // OFFSET 0x02077A9C
    public static void Party_SetGiratinaForm(Party party, int originForm)
    {
        int count = Party.Party_GetCurrentCount(party);
        for (int slot = 0; slot < count; slot++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, slot);
            if (originForm != 0)
            {
                Pokemon_SetGiratinaOriginForm(mon);
            }
            else
            {
                Pokemon_SetGiratinaFormByHeldItem(mon);
            }
        }
    }

    // OFFSET 0x02077928
    public static void Pokemon_SetArceusForm(Pokemon mon)
    {
        BoxPokemon_SetArceusForm(mon.Box);
    }

    // OFFSET 0x02077930
    public static void BoxPokemon_SetArceusForm(BoxPokemon boxMon)
    {
        uint species = BoxPokemon_GetValue(boxMon, MonDataSpeciesParam, null);
        uint ability = BoxPokemon_GetValue(boxMon, MonDataAbilityParam, null);
        uint item = BoxPokemon_GetValue(boxMon, MonDataHeldItemParam, null);
        if (species == SpeciesArceus && ability == AbilityMultitype)
        {
            int form = (int)Pokemon_GetArceusTypeOf((ushort)Item.Item_LoadParam((ushort)item, 1, 0));
            BoxPokemon_SetValue(boxMon, MonDataFormParam, form);
        }
    }

    // OFFSET 0x02077B14
    public static bool Pokemon_CanShayminSkyForm(Pokemon mon)
    {
        uint species = Pokemon_GetValue(mon, MonDataSpeciesParam, null);
        uint form = Pokemon_GetValue(mon, MonDataFormParam, null);
        uint status = Pokemon_GetValue(mon, MonDataStatusParam, null);
        uint currentHP = Pokemon_GetValue(mon, MonDataCurrentHPParam, null);
        uint fatefulEncounter = Pokemon_GetValue(mon, MonDataFatefulEncounterParam, null);
        Rtc.RTC_GetCurrentTime(out RtcTime time);
        if (species == SpeciesShaymin && form == 0 && currentHP != 0 && fatefulEncounter == 1 && (status & StatusFreeze) == 0 && time.Hour >= 4 && time.Hour < 20)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0207893C
    public static void Pokemon_RemoveBallCapsule(Pokemon mon)
    {
        byte capsuleID = 0;
        Pokemon_SetValue(mon, MonDataBallCapsuleIDParam, capsuleID);
        Pokemon_SetValue(mon, MonDataBallCapsuleParam, new BallCapsule());
    }

    // OFFSET 0x02077FB4
    public static bool Pokemon_CanLearnTM(Pokemon mon, byte tm)
    {
        return BoxPokemon_CanLearnTM(mon.Box, tm);
    }

    // OFFSET 0x02077FBC
    public static bool BoxPokemon_CanLearnTM(BoxPokemon boxMon, byte tm)
    {
        ushort species = (ushort)BoxPokemon_GetValue(boxMon, MonDataSpeciesOrEggParam, null);
        int form = (int)BoxPokemon_GetValue(boxMon, MonDataFormParam, null);
        return Species_CanLearnTM(species, form, tm);
    }

    // OFFSET 0x02077FE4
    public static bool Species_CanLearnTM(ushort species, int form, int tm)
    {
        if (species == SpeciesEgg)
        {
            return false;
        }
        uint mask;
        int param;
        if (tm < 0x20)
        {
            mask = 1u << tm;
            param = PersonalTMFlags1;
        }
        else if (tm < 0x40)
        {
            mask = 1u << (tm - 0x20);
            param = PersonalTMFlags1 + 1;
        }
        else if (tm < 0x60)
        {
            mask = 1u << (tm - 0x40);
            param = PersonalTMFlags1 + 2;
        }
        else
        {
            mask = 1u << (tm - 0x60);
            param = PersonalTMFlags1 + 3;
        }
        return (SpeciesData.SpeciesData_GetFormValue(species, form, param) & mask) != 0;
    }

    // OFFSET 0x02077E3C
    public static void Pokemon_PlayCry(Pokemon mon)
    {
        ushort species = (ushort)Pokemon_GetValue(mon, MonDataSpeciesParam, null);
        byte form = (byte)Pokemon_GetValue(mon, MonDataFormParam, null);
        SoundPlayback.Sound_PlayPokemonCry(species, form);
    }

    private const int MonDataPersonalityParam = 0;
    private const int MonDataPokerusParam = 0x9A;

    // OFFSET 0x0207762C
    public static sbyte Pokemon_GetFlavorAffinity(Pokemon mon, int flavor)
    {
        return BoxPokemon_GetFlavorAffinity(mon.Box, flavor);
    }

    // OFFSET 0x02077634
    public static sbyte BoxPokemon_GetFlavorAffinity(BoxPokemon boxMon, int flavor)
    {
        uint personality = BoxPokemon_GetValue(boxMon, MonDataPersonalityParam, null);
        return Pokemon_GetFlavorAffinityOf(personality, flavor);
    }

    // OFFSET 0x020778D8
    public static bool Pokemon_HasPokerus(Pokemon mon)
    {
        return BoxPokemon_HasPokerus(mon.Box);
    }

    // OFFSET 0x020778E0
    public static bool BoxPokemon_HasPokerus(BoxPokemon boxMon)
    {
        return (BoxPokemon_GetValue(boxMon, MonDataPokerusParam, null) & 0xF) != 0;
    }

    // OFFSET 0x020778F8
    public static bool Pokemon_IsPokerusCured(Pokemon mon)
    {
        return BoxPokemon_IsPokerusCured(mon.Box);
    }

    // OFFSET 0x02077900
    public static bool BoxPokemon_IsPokerusCured(BoxPokemon boxMon)
    {
        byte pokerus = (byte)BoxPokemon_GetValue(boxMon, MonDataPokerusParam, null);
        if ((pokerus & 0xF) != 0)
        {
            return false;
        }
        return (pokerus & 0xF0) != 0;
    }
}
