using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class PlayerAppearance
{
    private const uint VariantTable = 0x020ED770;
    private const uint AppearanceTable = 0x020ED7F0;
    private const int VariantsPerID = 4;
    private const int AppearancesPerGender = 8;
    private const int AppearanceEntryWords = 3;

    // OFFSET 0x0205C9BC
    public static int PlayerAppearance_GetByTrainerID(uint trainerID, uint gender, int index)
    {
        uint variant = RomData.Arm9U32(VariantTable + (trainerID & 7) * (VariantsPerID * 4) + (uint)index * 4, 1)[0];
        uint row = gender * AppearancesPerGender + variant;
        return (int)RomData.Arm9U32(AppearanceTable + row * (AppearanceEntryWords * 4), 1)[0];
    }

    // OFFSET 0x0205C9E0
    public static int PlayerAppearance_FindIndex(uint gender, uint appearance)
    {
        uint first = gender * AppearancesPerGender;
        for (int i = 0; i < AppearancesPerGender; i++)
        {
            if (RomData.Arm9U32(AppearanceTable + (first + (uint)i) * (AppearanceEntryWords * 4), 1)[0] == appearance)
            {
                return i + (int)first;
            }
        }
        return 0;
    }
}

public static class BattleTowerSeed
{
    private const uint Multiplier = 0x5D588B65;
    private const int TowerRecordOffset = 0x954;
    private const int SeedOffset = 4;

    // OFFSET 0x0206BFFC
    public static uint BattleTowerSeed_Next(uint seed)
    {
        return seed * Multiplier + 1;
    }

    // OFFSET 0x0206C008
    public static uint BattleTowerSeed_Reset(SaveData saveData)
    {
        RecordMixedRNG rng = RecordMixedRNG.SaveData_GetRecordMixedRNG(saveData);
        uint seed = BattleTowerSeed_Next(RecordMixedRNG.RecordMixedRNG_GetRand(rng));
        BattleFrontierSave frontier = BattleFrontierSave.SaveData_GetBattleFrontier(saveData);
        WifiBattleTowerRecord_SetSeed(new BufferView(frontier.Raw, TowerRecordOffset), seed);
        return seed;
    }

    // OFFSET 0x0202D470
    public static void WifiBattleTowerRecord_SetSeed(BufferView record, uint seed)
    {
        record.W32(SeedOffset, seed);
    }

    // OFFSET 0x0202D474
    public static uint WifiBattleTowerRecord_GetSeed(BufferView record)
    {
        return record.U32(SeedOffset);
    }
}

public static class PartyForms
{
    private const int ParamSpecies = 5;
    private const int ParamForm = 0x70;
    private const int SpeciesShaymin = 492;

    // OFFSET 0x02077B8C
    public static void Party_ResetShayminForms(Party party)
    {
        int count = Party.Party_GetCurrentCount(party);
        for (int i = 0; i < count; i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, i);
            uint species = Pokemon.Pokemon_GetValue(mon, ParamSpecies, null);
            uint form = Pokemon.Pokemon_GetValue(mon, ParamForm, null);
            if (species == SpeciesShaymin && form == 1)
            {
                Pokemon.Pokemon_SetShayminForm(mon, 0);
            }
        }
    }
}
