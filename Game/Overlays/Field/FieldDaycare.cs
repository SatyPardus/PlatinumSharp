using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class FieldDaycare
{
    private const int MonDataIsBadEgg = 3;
    private const int MonDataFriendship = 9;
    private const int MonDataAbility = 0xA;
    private const int MonDataIsEgg = 0x4C;
    private const int MonDataSpeciesExists = 0xAC;
    private const int MonDataSpeciesEgg = 0xAD;
    private const int AbilityMagmaArmor = 0x28;
    private const int AbilityFlameBody = 0x31;
    private const int EggCycleLengthNormal = 0xFF;
    private const int EggCycleLengthSpecial = 0xE6;
    private const uint SpecialEggCycleDatesAddress = 0x021F9F54;
    private const int SpecialEggCycleDateCount = 12;
    private const int OverlayID = 5;
    private const uint DaycareStepsFull = 0xFF;

    // OFFSET 0x021E622C
    public static BoxPokemon Daycare_GetBoxMon(Daycare daycare, int slot)
    {
        return Daycare.DaycareMon_GetBoxMon(Daycare.Daycare_GetDaycareMon(daycare, slot));
    }

    // OFFSET 0x021E6668
    public static void Daycare_GetBoxMons(Daycare daycare, BoxPokemon[] boxMons)
    {
        boxMons[0] = Daycare_GetBoxMon(daycare, 0);
        boxMons[1] = Daycare_GetBoxMon(daycare, 1);
    }

    // OFFSET 0x021E6F6C
    public static int Party_GetEggCyclesToSubtract(Party party)
    {
        int count = Party.Party_GetCurrentCount(party);
        for (byte i = 0; i < count; i++)
        {
            if (Pokemon.Pokemon_GetValue(Party.Party_GetPokemonBySlotIndex(party, i), MonDataSpeciesEgg, null) == 0)
            {
                byte ability = (byte)Pokemon.Pokemon_GetValue(Party.Party_GetPokemonBySlotIndex(party, i), MonDataAbility, null);
                if (ability == AbilityMagmaArmor || ability == AbilityFlameBody)
                {
                    return 2;
                }
            }
        }
        return 1;
    }

    // OFFSET 0x021E7110
    public static int FieldSystem_GetEggCycleLength(FieldSystem fieldSystem)
    {
        int date = FieldTime.FieldSystem_GetDayOfMonth(fieldSystem);
        date += FieldTime.FieldSystem_GetMonth(fieldSystem) * 100;
        if (FieldTime.FieldSystem_HasTimePenalty(fieldSystem) != 0)
        {
            return EggCycleLengthNormal;
        }
        ushort[] dates = RomData.OverlayU16(OverlayID, SpecialEggCycleDatesAddress, SpecialEggCycleDateCount);
        for (uint i = 0; i < SpecialEggCycleDateCount; i++)
        {
            if (date == dates[i])
            {
                return EggCycleLengthSpecial;
            }
        }
        return EggCycleLengthNormal;
    }

    // OFFSET 0x021E7154
    public static int Daycare_Update(Daycare daycare, Party party, FieldSystem fieldSystem)
    {
        BoxPokemon[] boxMons = new BoxPokemon[Daycare.SlotCount];
        Daycare_GetBoxMons(daycare, boxMons);
        uint monCount = 0;
        for (uint i = 0; i < Daycare.SlotCount; i++)
        {
            if (Pokemon.BoxPokemon_GetValue(boxMons[i], MonDataSpeciesExists, null) != 0)
            {
                Daycare.DaycareMon_AddSteps(Daycare.Daycare_GetDaycareMon(daycare, (int)i), 1);
                monCount++;
            }
        }
        if (!Daycare.Daycare_HasEgg(daycare) && monCount == Daycare.SlotCount && (byte)Daycare.DaycareMon_GetSteps(Daycare.Daycare_GetDaycareMon(daycare, 1)) == DaycareStepsFull)
        {
            uint compatibility = (uint)ov5_021E70FC(daycare);
            uint roll = (uint)(MathUtil.LCRNG_Next() * 100) / 0xFFFF;
            if (compatibility > roll)
            {
                ov5_021E6720(daycare);
            }
        }
        int stepCounter = Daycare.Daycare_GetStepCounter(daycare) + 1;
        Daycare.Daycare_SetStepCounter(daycare, stepCounter);
        if (stepCounter == FieldSystem_GetEggCycleLength(fieldSystem))
        {
            Daycare.Daycare_SetStepCounter(daycare, 0);
            uint cycles = (uint)Party_GetEggCyclesToSubtract(party);
            for (uint i = 0; i < (uint)Party.Party_GetCurrentCount(party); i++)
            {
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, (int)i);
                if (Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) == 0)
                {
                    continue;
                }
                if (Pokemon.Pokemon_GetValue(mon, MonDataIsBadEgg, null) != 0)
                {
                    continue;
                }
                uint friendship = Pokemon.Pokemon_GetValue(mon, MonDataFriendship, null);
                if (friendship == 0)
                {
                    return 1;
                }
                if (friendship >= cycles)
                {
                    friendship -= cycles;
                }
                else
                {
                    friendship--;
                }
                Pokemon.Pokemon_SetValue(mon, MonDataFriendship, friendship);
            }
        }
        return 0;
    }
}
