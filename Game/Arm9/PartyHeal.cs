namespace PlatinumSharp.Game;

public static class PartyHeal
{
    public const int RestoreAllPP = 0x7F;

    private const int MoveSlots = 4;
    private const int ParamMove1 = 54;
    private const int ParamMove1PP = 58;
    private const int ParamMove1PPUps = 62;
    private const int ParamStatus = 160;
    private const int ParamHP = 163;
    private const int ParamMaxHP = 164;
    private const int ParamSpeciesExists = 172;

    // OFFSET 0x02096F34
    public static byte Pokemon_CanRestorePP(Pokemon mon, int moveSlot)
    {
        ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, ParamMove1 + moveSlot, null);
        if (move == 0)
        {
            return 0;
        }
        byte pp = (byte)Pokemon.Pokemon_GetValue(mon, ParamMove1PP + moveSlot, null);
        byte ppUps = (byte)Pokemon.Pokemon_GetValue(mon, ParamMove1PPUps + moveSlot, null);
        if (pp < MoveTable.MoveTable_CalcMaxPP(move, ppUps))
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02096F84
    public static byte Pokemon_RestorePP(Pokemon mon, int moveSlot, int amount)
    {
        ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, ParamMove1 + moveSlot, null);
        if (move == 0)
        {
            return 0;
        }
        int ppParam = ParamMove1PP + moveSlot;
        byte pp = (byte)Pokemon.Pokemon_GetValue(mon, ppParam, null);
        byte ppUps = (byte)Pokemon.Pokemon_GetValue(mon, ParamMove1PPUps + moveSlot, null);
        byte maxPP = (byte)MoveTable.MoveTable_CalcMaxPP(move, ppUps);
        if (pp < maxPP)
        {
            if (amount == RestoreAllPP)
            {
                pp = maxPP;
            }
            else
            {
                pp = (byte)(pp + amount);
                if (pp > maxPP)
                {
                    pp = maxPP;
                }
            }
            Pokemon.Pokemon_SetValue(mon, ppParam, pp);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02097284
    public static void Party_HealAllMembers(Party party)
    {
        int count = Party.Party_GetCurrentCount(party);
        for (int i = 0; i < count; i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, i);
            if (Pokemon.Pokemon_GetValue(mon, ParamSpeciesExists, null) == 0)
            {
                continue;
            }
            Pokemon.Pokemon_SetValue(mon, ParamHP, Pokemon.Pokemon_GetValue(mon, ParamMaxHP, null));
            Pokemon.Pokemon_SetValue(mon, ParamStatus, 0);
            for (int slot = 0; slot < MoveSlots; slot++)
            {
                if (Pokemon_CanRestorePP(mon, slot) == 1)
                {
                    Pokemon_RestorePP(mon, slot, RestoreAllPP);
                }
            }
        }
    }
}
