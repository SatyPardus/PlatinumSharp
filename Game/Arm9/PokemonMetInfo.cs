namespace PlatinumSharp.Game;

public static class PokemonMetInfo
{
    public const int MethodCaught = 0;
    public const int MethodTraded = 1;
    public const int MethodPalPark = 2;
    public const int MethodEggReceived = 3;
    public const int MethodGift = 4;
    public const int MethodEggTraded = 5;
    public const int MethodEggHatched = 6;

    private const int LocationTypeRegular = 0;
    private const int LocationTypeSpecial = 1;
    private const int LocationTypeEvent = 2;
    private const int SpecialLocationTrade = 1;
    private const int SpecialLocationLinkTrade = 2;
    private const int EventLocationFaraway = 2;
    private const int RegularLocationPalPark = 0x37;
    private const int ParamOTID = 7;
    private const int ParamIsEgg = 76;
    private const int ParamFatefulEncounter = 110;
    private const int ParamOTName = 145;
    private const int ParamEggYear = 146;
    private const int ParamEggMonth = 147;
    private const int ParamEggDay = 148;
    private const int ParamMetYear = 149;
    private const int ParamMetMonth = 150;
    private const int ParamMetDay = 151;
    private const int ParamEggLocation = 152;
    private const int ParamMetLocation = 153;
    private const int ParamMetLevel = 156;
    private const int ParamOTGender = 157;
    private const int ParamLevel = 161;

    // OFFSET 0x0209304C
    public static void Pokemon_UpdateMetInfo(Pokemon mon, TrainerInfo trainerInfo, int method, int location, int heapID)
    {
        BoxPokemon_UpdateMetInfo(Pokemon.Pokemon_GetBoxPokemon(mon), trainerInfo, method, location, heapID);
    }

    // OFFSET 0x0209305C
    public static void BoxPokemon_UpdateMetInfo(BoxPokemon boxMon, TrainerInfo trainerInfo, int method, int location, int heapID)
    {
        switch (method)
        {
            case MethodCaught:
                if (location > SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 0))
                {
                    location = SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeEvent, EventLocationFaraway);
                }
                if (Pokemon.BoxPokemon_GetValue(boxMon, ParamIsEgg, null) == 0)
                {
                    BoxPokemon_ClearLocationAndDate(boxMon, 0);
                    BoxPokemon_SetLocationAndDate(boxMon, location, 1);
                    BoxPokemon_SetMetLevel(boxMon);
                }
                else
                {
                    BoxPokemon_SetLocationAndDate(boxMon, location, 0);
                    BoxPokemon_ClearLocationAndDate(boxMon, 1);
                }
                BoxPokemon_SetOriginalTrainer(boxMon, trainerInfo, heapID);
                break;
            case MethodTraded:
                if (Pokemon.BoxPokemon_GetValue(boxMon, ParamIsEgg, null) == 0)
                {
                    BoxPokemon_ClearLocationAndDate(boxMon, 0);
                    BoxPokemon_SetLocationAndDate(boxMon, SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, SpecialLocationTrade), 1);
                    BoxPokemon_SetMetLevel(boxMon);
                }
                else
                {
                    BoxPokemon_ClearLocationAndDate(boxMon, 0);
                    BoxPokemon_SetLocationAndDate(boxMon, SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, SpecialLocationTrade), 1);
                }
                break;
            case MethodPalPark:
                BoxPokemon_ClearLocationAndDate(boxMon, 0);
                BoxPokemon_SetLocationAndDate(boxMon, SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeRegular, RegularLocationPalPark), 1);
                BoxPokemon_SetMetLevel(boxMon);
                break;
            case MethodEggReceived:
                BoxPokemon_SetLocationAndDate(boxMon, location, 0);
                BoxPokemon_ClearLocationAndDate(boxMon, 1);
                BoxPokemon_SetOriginalTrainer(boxMon, trainerInfo, heapID);
                break;
            case MethodGift:
                if (Pokemon.BoxPokemon_BelongsToTrainer(boxMon, trainerInfo, heapID) == 1)
                {
                    if (Pokemon.BoxPokemon_GetValue(boxMon, ParamIsEgg, null) == 0)
                    {
                        BoxPokemon_ClearLocationAndDate(boxMon, 0);
                        BoxPokemon_SetLocationAndDate(boxMon, location, 1);
                        BoxPokemon_SetMetLevel(boxMon);
                    }
                    else
                    {
                        BoxPokemon_SetLocationAndDate(boxMon, location, 0);
                        BoxPokemon_ClearLocationAndDate(boxMon, 1);
                    }
                }
                else if (Pokemon.BoxPokemon_GetValue(boxMon, ParamIsEgg, null) == 0)
                {
                    BoxPokemon_ClearLocationAndDate(boxMon, 0);
                    BoxPokemon_SetLocationAndDate(boxMon, location, 1);
                    BoxPokemon_SetMetLevel(boxMon);
                }
                else
                {
                    BoxPokemon_ClearLocationAndDate(boxMon, 0);
                    BoxPokemon_SetLocationAndDate(boxMon, location, 1);
                }
                BoxPokemon_SetFatefulEncounter(boxMon);
                break;
            case MethodEggTraded:
                if (Pokemon.BoxPokemon_GetValue(boxMon, ParamIsEgg, null) != 0)
                {
                    BoxPokemon_SetLocationAndDate(boxMon, SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, SpecialLocationLinkTrade), 1);
                }
                break;
            case MethodEggHatched:
                if (location > SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeSpecial, 0))
                {
                    location = SpecialMetLoc.SpecialMetLoc_GetId(LocationTypeEvent, EventLocationFaraway);
                }
                if (Pokemon.BoxPokemon_BelongsToTrainer(boxMon, trainerInfo, heapID) == 0)
                {
                    Pokemon.BoxPokemon_SetValue(boxMon, ParamEggLocation, Pokemon.BoxPokemon_GetValue(boxMon, ParamMetLocation, null));
                    Pokemon.BoxPokemon_SetValue(boxMon, ParamEggYear, Pokemon.BoxPokemon_GetValue(boxMon, ParamMetYear, null));
                    Pokemon.BoxPokemon_SetValue(boxMon, ParamEggMonth, Pokemon.BoxPokemon_GetValue(boxMon, ParamMetMonth, null));
                    Pokemon.BoxPokemon_SetValue(boxMon, ParamEggDay, Pokemon.BoxPokemon_GetValue(boxMon, ParamMetDay, null));
                }
                BoxPokemon_SetLocationAndDate(boxMon, location, 1);
                BoxPokemon_SetOriginalTrainer(boxMon, trainerInfo, heapID);
                break;
        }
    }

    // OFFSET 0x020932A4
    private static void BoxPokemon_SetOriginalTrainer(BoxPokemon boxMon, TrainerInfo trainerInfo, int heapID)
    {
        uint trainerID = TrainerInfo.TrainerInfo_ID(trainerInfo);
        uint gender = TrainerInfo.TrainerInfo_Gender(trainerInfo);
        GameString name = TrainerInfo.TrainerInfo_NameNewString(trainerInfo, heapID);
        Pokemon.BoxPokemon_SetValue(boxMon, ParamOTID, trainerID);
        Pokemon.BoxPokemon_SetValue(boxMon, ParamOTGender, gender);
        Pokemon.BoxPokemon_SetValue(boxMon, ParamOTName, name);
        GameString.String_Free(name);
    }

    // OFFSET 0x020932F0
    private static void BoxPokemon_SetLocationAndDate(BoxPokemon boxMon, int location, int isMet)
    {
        Rtc.GetCurrentDate(out RtcDate date);
        if (isMet == 0)
        {
            Pokemon.BoxPokemon_SetValue(boxMon, ParamEggLocation, location);
            Pokemon.BoxPokemon_SetValue(boxMon, ParamEggYear, date.Year);
            Pokemon.BoxPokemon_SetValue(boxMon, ParamEggMonth, date.Month);
            Pokemon.BoxPokemon_SetValue(boxMon, ParamEggDay, date.Day);
        }
        else
        {
            Pokemon.BoxPokemon_SetValue(boxMon, ParamMetLocation, location);
            Pokemon.BoxPokemon_SetValue(boxMon, ParamMetYear, date.Year);
            Pokemon.BoxPokemon_SetValue(boxMon, ParamMetMonth, date.Month);
            Pokemon.BoxPokemon_SetValue(boxMon, ParamMetDay, date.Day);
        }
    }

    // OFFSET 0x02093368
    private static void BoxPokemon_ClearLocationAndDate(BoxPokemon boxMon, int isMet)
    {
        if (isMet == 0)
        {
            Pokemon.BoxPokemon_SetValue(boxMon, ParamEggLocation, 0);
            Pokemon.BoxPokemon_SetValue(boxMon, ParamEggYear, 0);
            Pokemon.BoxPokemon_SetValue(boxMon, ParamEggMonth, 0);
            Pokemon.BoxPokemon_SetValue(boxMon, ParamEggDay, 0);
        }
        else
        {
            Pokemon.BoxPokemon_SetValue(boxMon, ParamMetLocation, 0);
            Pokemon.BoxPokemon_SetValue(boxMon, ParamMetYear, 0);
            Pokemon.BoxPokemon_SetValue(boxMon, ParamMetMonth, 0);
            Pokemon.BoxPokemon_SetValue(boxMon, ParamMetDay, 0);
        }
    }

    // OFFSET 0x020933C8
    private static void BoxPokemon_SetMetLevel(BoxPokemon boxMon)
    {
        Pokemon.BoxPokemon_SetValue(boxMon, ParamMetLevel, Pokemon.BoxPokemon_GetValue(boxMon, ParamLevel, null));
    }

    // OFFSET 0x020933E8
    private static void BoxPokemon_SetFatefulEncounter(BoxPokemon boxMon)
    {
        Pokemon.BoxPokemon_SetValue(boxMon, ParamFatefulEncounter, 1);
    }
}
