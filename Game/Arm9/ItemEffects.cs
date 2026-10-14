namespace PlatinumSharp.Game;

public static class ItemEffects
{
    private const int ItemParamPartyUse = 0xE;
    private const int ItemParamHealSleep = 0xF;
    private const int ItemParamHealPoison = 0x10;
    private const int ItemParamHealBurn = 0x11;
    private const int ItemParamHealFreeze = 0x12;
    private const int ItemParamHealParalysis = 0x13;
    private const int ItemParamRevive = 0x17;
    private const int ItemParamReviveAll = 0x18;
    private const int ItemParamLevelUp = 0x19;
    private const int ItemParamEvolve = 0x1A;
    private const int ItemParamPPUp = 0x22;
    private const int ItemParamPPMax = 0x23;
    private const int ItemParamPPRestore = 0x24;
    private const int ItemParamPPRestoreAll = 0x25;
    private const int ItemParamHPRestore = 0x26;
    private const int ItemParamGiveHPEVs = 0x27;
    private const int ItemParamFriendshipLow = 0x2D;
    private const int ItemParamFriendshipMid = 0x2E;
    private const int ItemParamFriendshipHigh = 0x2F;
    private const int ItemParamHPEVs = 0x30;
    private const int ItemParamHPRestored = 0x36;
    private const int ItemParamPPRestored = 0x37;
    private const int ItemParamFriendshipLowValue = 0x38;
    private const int ItemParamFriendshipMidValue = 0x39;
    private const int ItemParamFriendshipHighValue = 0x3A;
    private const int ItemHoldEffectParam = 1;
    private const int HoldEffectSootheBell = 0x35;
    private const int BallLuxury = 0xB;
    private const int MonDataSpecies = 5;
    private const int MonDataHeldItem = 6;
    private const int MonDataExp = 8;
    private const int MonDataFriendship = 9;
    private const int MonDataHPEV = 0xD;
    private const int MonDataMove1 = 0x36;
    private const int MonDataMove1PP = 0x3A;
    private const int MonDataMove1PPUps = 0x3E;
    private const int MonDataMetLocation = 0x98;
    private const int MonDataPokeball = 0x9B;
    private const int MonDataStatus = 0xA0;
    private const int MonDataLevel = 0xA1;
    private const int MonDataCurrentHP = 0xA3;
    private const int MonDataMaxHP = 0xA4;
    private const int SpeciesShedinja = 0x124;
    private const int MaxLevel = 100;
    private const int MaxStatEV = 100;
    private const int MaxTotalEV = 0x1FE;
    private const int EvoClassByItem = 3;
    private const uint StatusSleep = 7;
    private const uint StatusPoison = 0x88;
    private const uint StatusPoisonCureMask = 0xFFFFF077;
    private const uint StatusBurn = 0x10;
    private const uint StatusFreeze = 0x20;
    private const uint StatusParalysis = 0x40;

    private static bool CanChangeEV(Pokemon mon, byte[] itemData, int stat, int[] evs)
    {
        if (Item.Item_Get(itemData, 0, ItemParamGiveHPEVs + stat) == 0)
        {
            return false;
        }
        int amount = Item.Item_Get(itemData, 0, ItemParamHPEVs + stat);
        if (amount > 0)
        {
            int total = evs[0] + evs[1] + evs[2] + evs[3] + evs[4] + evs[5];
            return evs[stat] < MaxStatEV && total < MaxTotalEV;
        }
        if (amount < 0)
        {
            if (evs[stat] > 0)
            {
                return true;
            }
            return Pokemon_CanItemIncreaseFriendship(mon, itemData);
        }
        return false;
    }

    // OFFSET 0x02096420
    public static bool Pokemon_CanUseItem(Pokemon mon, ushort item, int moveSlot, int heapID)
    {
        byte[] itemData = Item.Item_Load(item, 0, heapID)!;
        if (Item.Item_Get(itemData, 0, ItemParamPartyUse) != 1)
        {
            return false;
        }
        uint status = Pokemon.Pokemon_GetValue(mon, MonDataStatus, null);
        if (Item.Item_Get(itemData, 0, ItemParamHealSleep) != 0 && (status & StatusSleep) != 0)
        {
            return true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamHealPoison) != 0 && (status & StatusPoison) != 0)
        {
            return true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamHealBurn) != 0 && (status & StatusBurn) != 0)
        {
            return true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamHealFreeze) != 0 && (status & StatusFreeze) != 0)
        {
            return true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamHealParalysis) != 0 && (status & StatusParalysis) != 0)
        {
            return true;
        }
        uint hp = Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null);
        if ((Item.Item_Get(itemData, 0, ItemParamRevive) != 0 || Item.Item_Get(itemData, 0, ItemParamReviveAll) != 0) && Item.Item_Get(itemData, 0, ItemParamLevelUp) == 0)
        {
            if (hp == 0)
            {
                return true;
            }
        }
        else if (Item.Item_Get(itemData, 0, ItemParamHPRestore) != 0 && hp != 0 && hp < Pokemon.Pokemon_GetValue(mon, MonDataMaxHP, null))
        {
            return true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamLevelUp) != 0 && Pokemon.Pokemon_GetValue(mon, MonDataLevel, null) < MaxLevel)
        {
            return true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamEvolve) != 0 && Pokemon.Pokemon_GetEvolutionTargetSpecies(null, mon, EvoClassByItem, item, out _) != 0)
        {
            return true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamPPUp) != 0 || Item.Item_Get(itemData, 0, ItemParamPPMax) != 0)
        {
            if (Pokemon.Pokemon_GetValue(mon, MonDataMove1PPUps + moveSlot, null) < 3 && MoveTable.MoveTable_CalcMaxPP((ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + moveSlot, null), 0) >= 5)
            {
                return true;
            }
        }
        if (Item.Item_Get(itemData, 0, ItemParamPPRestore) != 0 && PartyHeal.Pokemon_CanRestorePP(mon, moveSlot) == 1)
        {
            return true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamPPRestoreAll) != 0)
        {
            for (int i = 0; i < 4; i++)
            {
                if (PartyHeal.Pokemon_CanRestorePP(mon, i) == 1)
                {
                    return true;
                }
            }
        }
        int[] evs = new int[6];
        for (int i = 0; i < 6; i++)
        {
            evs[i] = (int)Pokemon.Pokemon_GetValue(mon, MonDataHPEV + i, null);
        }
        if (Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null) != SpeciesShedinja && CanChangeEV(mon, itemData, 0, evs))
        {
            return true;
        }
        for (int stat = 1; stat < 6; stat++)
        {
            if (CanChangeEV(mon, itemData, stat, evs))
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0209693C
    public static bool Party_CanUseItemOnPokemonAt(Party party, ushort item, int slot, int moveSlot, int heapID)
    {
        return Pokemon_CanUseItem(Party.Party_GetPokemonBySlotIndex(party, slot), item, moveSlot, heapID);
    }

    // OFFSET 0x02096954
    public static bool Pokemon_ApplyItemEffects(Pokemon mon, ushort item, int moveSlot, ushort location, int heapID)
    {
        byte[] itemData = Item.Item_Load(item, 0, heapID)!;
        if (Item.Item_Get(itemData, 0, ItemParamPartyUse) != 1)
        {
            return false;
        }
        bool applied = false;
        bool attempted = false;
        uint oldStatus = Pokemon.Pokemon_GetValue(mon, MonDataStatus, null);
        uint status = oldStatus;
        if (Item.Item_Get(itemData, 0, ItemParamHealSleep) != 0)
        {
            status &= ~StatusSleep;
            attempted = true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamHealPoison) != 0)
        {
            status &= StatusPoisonCureMask;
            attempted = true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamHealBurn) != 0)
        {
            status &= ~StatusBurn;
            attempted = true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamHealFreeze) != 0)
        {
            status &= ~StatusFreeze;
            attempted = true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamHealParalysis) != 0)
        {
            status &= ~StatusParalysis;
            attempted = true;
        }
        if (oldStatus != status)
        {
            Pokemon.Pokemon_SetValue(mon, MonDataStatus, status);
            applied = true;
        }
        uint hp = Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null);
        uint maxHP = Pokemon.Pokemon_GetValue(mon, MonDataMaxHP, null);
        if ((Item.Item_Get(itemData, 0, ItemParamRevive) != 0 || Item.Item_Get(itemData, 0, ItemParamReviveAll) != 0) && Item.Item_Get(itemData, 0, ItemParamLevelUp) != 0)
        {
            if (hp == 0)
            {
                Pokemon_RestoreHP(mon, hp, maxHP, (uint)Item.Item_Get(itemData, 0, ItemParamHPRestored));
                applied = true;
            }
            attempted = true;
        }
        else if (Item.Item_Get(itemData, 0, ItemParamHPRestore) != 0)
        {
            if ((int)hp < (int)maxHP)
            {
                Pokemon_RestoreHP(mon, hp, maxHP, (uint)Item.Item_Get(itemData, 0, ItemParamHPRestored));
                applied = true;
            }
            attempted = true;
        }
        uint level = Pokemon.Pokemon_GetValue(mon, MonDataLevel, null);
        if (Item.Item_Get(itemData, 0, ItemParamLevelUp) != 0)
        {
            if ((int)level < MaxLevel)
            {
                Pokemon.Pokemon_IncreaseValue(mon, MonDataExp, (int)Pokemon.Pokemon_GetExpToNextLevel(mon));
                Pokemon.Pokemon_CalcLevelAndStats(mon);
                if (hp == 0)
                {
                    uint newMaxHP = Pokemon.Pokemon_GetValue(mon, MonDataMaxHP, null);
                    Pokemon_RestoreHP(mon, hp, newMaxHP, newMaxHP - maxHP);
                }
                applied = true;
            }
            attempted = true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamEvolve) != 0)
        {
            attempted = true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamPPUp) != 0)
        {
            if (Pokemon_IncreasePPUps(mon, moveSlot, 1))
            {
                applied = true;
            }
            attempted = true;
        }
        else if (Item.Item_Get(itemData, 0, ItemParamPPMax) != 0)
        {
            if (Pokemon_IncreasePPUps(mon, moveSlot, 3))
            {
                applied = true;
            }
            attempted = true;
        }
        if (Item.Item_Get(itemData, 0, ItemParamPPRestore) != 0)
        {
            if (PartyHeal.Pokemon_RestorePP(mon, moveSlot, Item.Item_Get(itemData, 0, ItemParamPPRestored)) == 1)
            {
                applied = true;
            }
            attempted = true;
        }
        else if (Item.Item_Get(itemData, 0, ItemParamPPRestoreAll) != 0)
        {
            for (int i = 0; i < 4; i++)
            {
                if (PartyHeal.Pokemon_RestorePP(mon, i, Item.Item_Get(itemData, 0, ItemParamPPRestored)) == 1)
                {
                    applied = true;
                }
            }
            attempted = true;
        }
        int[] evs = new int[6];
        for (int i = 0; i < 6; i++)
        {
            evs[i] = (int)Pokemon.Pokemon_GetValue(mon, MonDataHPEV + i, null);
        }
        bool isShedinja = Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null) == SpeciesShedinja;
        for (int stat = 0; stat < 6; stat++)
        {
            if ((stat == 0 && isShedinja) || Item.Item_Get(itemData, 0, ItemParamGiveHPEVs + stat) == 0)
            {
                continue;
            }
            int amount = Item.Item_Get(itemData, 0, ItemParamHPEVs + stat);
            int others = 0;
            for (int i = 0; i < 6; i++)
            {
                if (i != stat)
                {
                    others += evs[i];
                }
            }
            int ev = Pokemon_CalcItemEV(evs[stat], others, amount);
            if (ev != -1)
            {
                evs[stat] = ev;
                Pokemon.Pokemon_SetValue(mon, MonDataHPEV + stat, ev);
                Pokemon.Pokemon_CalcLevelAndStats(mon);
                applied = true;
            }
            if (amount > 0)
            {
                attempted = true;
            }
        }
        if (!applied && attempted)
        {
            return false;
        }
        int friendship = (int)Pokemon.Pokemon_GetValue(mon, MonDataFriendship, null);
        if (friendship < 100)
        {
            if (Item.Item_Get(itemData, 0, ItemParamFriendshipLow) != 0)
            {
                Pokemon_ChangeFriendshipFromItem(mon, friendship, Item.Item_Get(itemData, 0, ItemParamFriendshipLowValue), location, heapID);
            }
        }
        else if (friendship < 200)
        {
            if (Item.Item_Get(itemData, 0, ItemParamFriendshipMid) != 0)
            {
                Pokemon_ChangeFriendshipFromItem(mon, friendship, Item.Item_Get(itemData, 0, ItemParamFriendshipMidValue), location, heapID);
            }
        }
        else if (friendship <= 255)
        {
            if (Item.Item_Get(itemData, 0, ItemParamFriendshipHigh) != 0)
            {
                Pokemon_ChangeFriendshipFromItem(mon, friendship, Item.Item_Get(itemData, 0, ItemParamFriendshipHighValue), location, heapID);
            }
        }
        return applied;
    }

    // OFFSET 0x02096F14
    public static bool Party_ApplyItemEffectsToMember(Party party, ushort item, int slot, byte moveSlot, ushort location, int heapID)
    {
        return Pokemon_ApplyItemEffects(Party.Party_GetPokemonBySlotIndex(party, slot), item, moveSlot, location, heapID);
    }

    // OFFSET 0x02097004
    private static bool Pokemon_IncreasePPUps(Pokemon mon, int moveSlot, int amount)
    {
        byte ppUps = (byte)Pokemon.Pokemon_GetValue(mon, MonDataMove1PPUps + moveSlot, null);
        if (ppUps == 3)
        {
            return false;
        }
        ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + moveSlot, null);
        if (MoveTable.MoveTable_CalcMaxPP(move, 0) < 5)
        {
            return false;
        }
        byte pp = (byte)Pokemon.Pokemon_GetValue(mon, MonDataMove1PP + moveSlot, null);
        uint oldMaxPP = MoveTable.MoveTable_CalcMaxPP(move, ppUps);
        ppUps = (byte)(ppUps + amount > 3 ? 3 : ppUps + amount);
        pp = (byte)(pp + MoveTable.MoveTable_CalcMaxPP(move, ppUps) - oldMaxPP);
        Pokemon.Pokemon_SetValue(mon, MonDataMove1PPUps + moveSlot, ppUps);
        Pokemon.Pokemon_SetValue(mon, MonDataMove1PP + moveSlot, pp);
        return true;
    }

    // OFFSET 0x020970AC
    private static void Pokemon_RestoreHP(Pokemon mon, uint hp, uint maxHP, uint amount)
    {
        if (maxHP == 1)
        {
            amount = 1;
        }
        else if (amount == 0xFF)
        {
            amount = maxHP;
        }
        else if (amount == 0xFE)
        {
            amount = maxHP >> 1;
        }
        else if (amount == 0xFD)
        {
            amount = maxHP >> 2;
        }
        hp = hp + amount > maxHP ? maxHP : hp + amount;
        Pokemon.Pokemon_SetValue(mon, MonDataCurrentHP, hp);
    }

    // OFFSET 0x020970EC
    private static int Pokemon_CalcItemEV(int ev, int otherEVs, int amount)
    {
        if (ev == 0 && amount < 0)
        {
            return -1;
        }
        if (ev >= MaxStatEV && amount > 0)
        {
            return -1;
        }
        if (ev + otherEVs >= MaxTotalEV && amount > 0)
        {
            return -1;
        }
        ev += amount;
        if (ev > MaxStatEV)
        {
            ev = MaxStatEV;
        }
        else if (ev < 0)
        {
            ev = 0;
        }
        if (ev + otherEVs > MaxTotalEV)
        {
            ev = MaxTotalEV - otherEVs;
        }
        return ev;
    }

    // OFFSET 0x02097144
    private static bool Pokemon_CanItemIncreaseFriendship(Pokemon mon, byte[] itemData)
    {
        int friendship = (int)Pokemon.Pokemon_GetValue(mon, MonDataFriendship, null);
        if (friendship >= 255)
        {
            return false;
        }
        if (friendship < 100)
        {
            return Item.Item_Get(itemData, 0, ItemParamFriendshipLow) != 0 && Item.Item_Get(itemData, 0, ItemParamFriendshipLowValue) > 0;
        }
        if (friendship < 200)
        {
            return Item.Item_Get(itemData, 0, ItemParamFriendshipMid) != 0 && Item.Item_Get(itemData, 0, ItemParamFriendshipMidValue) > 0;
        }
        return Item.Item_Get(itemData, 0, ItemParamFriendshipHigh) != 0 && Item.Item_Get(itemData, 0, ItemParamFriendshipHighValue) > 0;
    }

    // OFFSET 0x020971D0
    private static bool Pokemon_ChangeFriendshipFromItem(Pokemon mon, int friendship, int amount, ushort location, int heapID)
    {
        if (friendship == 255 && amount > 0)
        {
            return false;
        }
        if (friendship == 0 && amount < 0)
        {
            return false;
        }
        if (amount > 0)
        {
            ushort heldItem = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataHeldItem, null);
            if (Item.Item_LoadParam(heldItem, ItemHoldEffectParam, heapID) == HoldEffectSootheBell)
            {
                amount = amount * 150 / 100;
            }
            if (Pokemon.Pokemon_GetValue(mon, MonDataPokeball, null) == BallLuxury)
            {
                amount++;
            }
            if (location == Pokemon.Pokemon_GetValue(mon, MonDataMetLocation, null))
            {
                amount++;
            }
        }
        amount += friendship;
        if (amount > 255)
        {
            amount = 255;
        }
        if (amount < 0)
        {
            amount = 0;
        }
        Pokemon.Pokemon_SetValue(mon, MonDataFriendship, amount);
        return true;
    }
}
