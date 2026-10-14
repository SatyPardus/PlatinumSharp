using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class FieldParty
{
    public const int NoPartySlot = 0xFF;
    public const int PoisonResultNone = 0;
    public const int PoisonResultDamaged = 1;
    public const int PoisonResultSurvived = 2;

    private const int ParamHeldItem = 6;
    private const int ParamChampionRibbon = 0x19;
    private const int ParamMove1 = 0x36;
    private const int ParamMove2 = 0x37;
    private const int ParamMove3 = 0x38;
    private const int ParamMove4 = 0x39;
    private const int ParamIsEgg = 0x4C;
    private const int ParamStatus = 0xA0;
    private const int ParamHP = 0xA3;
    private const uint StatusPoisonMask = 0x88;
    private const int RandomIVs = 0x20;
    private const int PokeballPoke = 4;
    private const int HeapIDEgg = 0x20;
    private const int FriendshipEventSurvivePoison = 7;

    // OFFSET 0x02054884
    public static bool Pokemon_CanBattle(Pokemon mon)
    {
        if (Pokemon.Pokemon_GetValue(mon, ParamHP, null) == 0)
        {
            return false;
        }
        if (Pokemon.Pokemon_GetValue(mon, ParamIsEgg, null) == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020548B0
    public static bool Pokemon_GiveMonFromScript(int heapID, SaveData saveData, ushort species, byte level, ushort heldItem, int metLocation, int metTerrain)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(saveData);
        Party party = Party.SaveData_GetParty(saveData);
        Pokemon mon = Pokemon.Pokemon_New(heapID);
        Pokemon.Pokemon_Init(mon);
        Pokemon.Pokemon_InitWith(mon, species, level, RandomIVs, 0, 0, 0, 0);
        Pokemon.Pokemon_SetCatchData(mon, trainerInfo, PokeballPoke, metLocation, metTerrain, heapID);
        uint item = heldItem;
        Pokemon.Pokemon_SetValue(mon, ParamHeldItem, item);
        bool result = Party.Party_AddPokemon(party, mon);
        if (result)
        {
            CatchRecords.SaveData_UpdateCatchRecords(saveData, mon);
        }
        return result;
    }

    // OFFSET 0x02054930
    public static bool Pokemon_GiveEggFromScript(int unused, SaveData saveData, ushort species, byte isFromDaycare, int metLocationType, int metLocationIndex)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(saveData);
        Party party = Party.SaveData_GetParty(saveData);
        Pokemon mon = Pokemon.Pokemon_New(HeapIDEgg);
        Pokemon.Pokemon_Init(mon);
        int metLocation = SpecialMetLoc.SpecialMetLoc_GetId(metLocationType, metLocationIndex);
        ov5_021E6CF0(mon, species, isFromDaycare, trainerInfo, PokeballPoke, metLocation);
        return Party.Party_AddPokemon(party, mon);
    }

    // OFFSET 0x02054988
    public static void Party_ResetMonMoveSlot(Party party, int partySlot, int moveSlot, ushort moveID)
    {
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, partySlot);
        Pokemon.Pokemon_ResetMoveSlot(mon, moveID, (byte)moveSlot);
    }

    // OFFSET 0x020549A0
    public static int Party_HasMonWithMove(Party party, int moveID)
    {
        int count = Party.Party_GetCurrentCount(party);
        for (int i = 0; i < count; i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, i);
            if (Pokemon.Pokemon_GetValue(mon, ParamIsEgg, null) != 0)
            {
                continue;
            }
            if (moveID == Pokemon.Pokemon_GetValue(mon, ParamMove1, null)
                || moveID == Pokemon.Pokemon_GetValue(mon, ParamMove2, null)
                || moveID == Pokemon.Pokemon_GetValue(mon, ParamMove3, null)
                || moveID == Pokemon.Pokemon_GetValue(mon, ParamMove4, null))
            {
                return i;
            }
        }
        return NoPartySlot;
    }

    // OFFSET 0x02054A10
    public static int Party_AliveMonsCount(Party party)
    {
        int count = Party.Party_GetCurrentCount(party);
        int alive = 0;
        for (int i = 0; i < count; i++)
        {
            if (Pokemon_CanBattle(Party.Party_GetPokemonBySlotIndex(party, i)))
            {
                alive++;
            }
        }
        return alive;
    }

    // OFFSET 0x02054A40
    public static Pokemon? Party_FindFirstEligibleBattler(Party party)
    {
        int count = Party.Party_GetCurrentCount(party);
        for (int i = 0; i < count; i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, i);
            if (Pokemon_CanBattle(mon))
            {
                return mon;
            }
        }
        ErrorHandling.GF_AssertFail();
        return null;
    }

    // OFFSET 0x02054A74
    public static Pokemon? Party_FindFirstHatchedMon(Party party)
    {
        ushort count = (ushort)Party.Party_GetCurrentCount(party);
        for (ushort i = 0; i < count; i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, i);
            if (Pokemon.Pokemon_GetValue(mon, ParamIsEgg, null) == 0)
            {
                return mon;
            }
        }
        return null;
    }

    // OFFSET 0x02054AB0
    public static int Party_HasTwoAliveMons(Party party)
    {
        if (Party_AliveMonsCount(party) >= 2)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02054AC4
    public static void Party_GiveChampionRibbons(Party party)
    {
        byte hasRibbon = 1;
        int count = Party.Party_GetCurrentCount(party);
        for (int i = 0; i < count; i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, i);
            if (Pokemon.Pokemon_GetValue(mon, ParamIsEgg, null) == 0)
            {
                Pokemon.Pokemon_SetValue(mon, ParamChampionRibbon, hasRibbon);
            }
        }
    }

    // OFFSET 0x02054B04
    public static int Pokemon_DoPoisonDamage(Party party, ushort mapLabelTextID)
    {
        int poisoned = 0;
        int survived = 0;
        int count = Party.Party_GetCurrentCount(party);
        for (int i = 0; i < count; i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, i);
            if (!Pokemon_CanBattle(mon))
            {
                continue;
            }
            if ((Pokemon.Pokemon_GetValue(mon, ParamStatus, null) & StatusPoisonMask) == 0)
            {
                continue;
            }
            uint hp = Pokemon.Pokemon_GetValue(mon, ParamHP, null);
            if (hp > 1)
            {
                hp--;
            }
            Pokemon.Pokemon_SetValue(mon, ParamHP, hp);
            if (hp == 1)
            {
                survived++;
                Pokemon.Pokemon_UpdateFriendship(mon, FriendshipEventSurvivePoison, mapLabelTextID);
            }
            poisoned++;
        }
        if (survived != 0)
        {
            return PoisonResultSurvived;
        }
        if (poisoned != 0)
        {
            return PoisonResultDamaged;
        }
        return PoisonResultNone;
    }

    // OFFSET 0x02054B94
    public static bool Pokemon_TrySurvivePoison(Pokemon mon)
    {
        if ((Pokemon.Pokemon_GetValue(mon, ParamStatus, null) & StatusPoisonMask) != 0
            && Pokemon.Pokemon_GetValue(mon, ParamHP, null) == 1)
        {
            uint status = 0;
            Pokemon.Pokemon_SetValue(mon, ParamStatus, status);
            return true;
        }
        return false;
    }
}
