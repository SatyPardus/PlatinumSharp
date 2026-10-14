namespace PlatinumSharp.Game;

public static partial class BattleSystems
{
    private const int UseItemHeap = 5;
    private const int CtxParamSideConditionsMask = 0;
    private const int CtxParamMistTurns = 1;
    private const int CtxParamSelectedPartySlot = 2;
    private const int UseItemBattleTypeDoublesTrainer = 3;
    private const uint UseItemBattleTypeTag = 0x10;
    private const int ItemParamHealSleep = 0xF;
    private const int ItemParamHealPoison = 0x10;
    private const int ItemParamHealBurn = 0x11;
    private const int ItemParamHealFreeze = 0x12;
    private const int ItemParamHealParalysis = 0x13;
    private const int ItemParamHealConfusion = 0x14;
    private const int ItemParamHealAttract = 0x15;
    private const int ItemParamGuardSpec = 0x16;
    private const int ItemParamRevive = 0x17;
    private const int ItemParamAttackUp = 0x1B;
    private const int ItemParamDefenseUp = 0x1C;
    private const int ItemParamSpAttackUp = 0x1D;
    private const int ItemParamSpDefenseUp = 0x1E;
    private const int ItemParamSpeedUp = 0x1F;
    private const int ItemParamAccuracyUp = 0x20;
    private const int ItemParamCriticalUp = 0x21;
    private const int ItemParamPPRestore = 0x24;
    private const int ItemParamPPRestoreAll = 0x25;
    private const int ItemParamHPRestore = 0x26;
    private const int ItemParamFriendshipLow = 0x2D;
    private const int ItemParamFriendshipMid = 0x2E;
    private const int ItemParamFriendshipHigh = 0x2F;
    private const int ItemParamHPRestoreAmount = 0x36;
    private const int ItemParamPPRestoreAmount = 0x37;
    private const int ItemParamFriendshipLowAmount = 0x38;
    private const int ItemParamFriendshipMidAmount = 0x39;
    private const int ItemParamFriendshipHighAmount = 0x3A;
    private const int ItemParamHoldEffect = 1;
    private const int MonParamStatus = 0xA0;
    private const int MonParamCurHP = 0xA3;
    private const int MonParamMaxHP = 0xA4;
    private const int MonParamFriendship = 9;
    private const int MonParamMove1PP = 0x3A;
    private const int MonParamMove1MaxPP = 0x42;
    private const int MonParamPokeball = 0x9B;
    private const int MonParamMetLocation = 0x98;
    private const int MonParamHeldItem = 6;
    private const int BattleMonParamStatus = 0x34;
    private const int BattleMonParamStatusVolatile = 0x35;
    private const int BattleMonParamAttackStage = 0x13;
    private const int BattleMonParamDefenseStage = 0x14;
    private const int BattleMonParamSpeedStage = 0x15;
    private const int BattleMonParamSpAttackStage = 0x16;
    private const int BattleMonParamSpDefenseStage = 0x17;
    private const int BattleMonParamAccuracyStage = 0x18;
    private const int BattleMonParamMove1PP = 0x1F;
    private const int BattleMonParamFriendship = 0x2C;
    private const int BattleMonParamCurHP = 0x2F;
    private const int BattleMonParamMimickedSlots = 0x4B;
    private const int BattleMonParamEnemyHPRestore = 0x5F;
    private const uint UseStatusSleep = 7;
    private const uint UseStatusPoison = 0xF88;
    private const uint UseStatusPoisonClear = 0xFFFFF077;
    private const uint UseStatusBurn = 0x10;
    private const uint UseStatusFreeze = 0x20;
    private const uint UseStatusParalysis = 0x40;
    private const uint UseVolatileConfusion = 7;
    private const uint UseVolatileNightmareClear = 0xF7FFFFFF;
    private const uint UseVolatileAttraction = 0xF0000;
    private const uint UseVolatileFocusEnergy = 0x100000;
    private const uint UseVolatileTransformed = 0x200000;
    private const int UseSideConditionMist = 0x40;
    private const int UseMistTurns = 5;
    private const int StageMax = 12;
    private const int HPRestoreFull = 0xFF;
    private const int HPRestoreHalf = 0xFE;
    private const int HPRestoreQuarter = 0xFD;
    private const int FriendshipMid = 100;
    private const int FriendshipHigh = 200;
    private const int BallLuxury = 0xB;
    private const int HoldEffectSootheBell = 0x35;

    private static bool UseItem_TargetsActive(int selectedSlot, int partnerSelectedSlot, int partySlot)
    {
        return selectedSlot == partySlot || partnerSelectedSlot == partySlot;
    }

    private static bool UseItem_ClearStatus(BattleContext battleCtx, Pokemon mon, int battler, int selectedSlot, int partnerSelectedSlot, int partySlot, uint statusMask, uint clearMask, bool clearNightmare)
    {
        uint status = Pokemon.Pokemon_GetValue(mon, MonParamStatus, null);
        if ((status & statusMask) == 0)
        {
            return false;
        }
        status &= clearMask;
        Pokemon.Pokemon_SetValue(mon, MonParamStatus, status);
        if (UseItem_TargetsActive(selectedSlot, partnerSelectedSlot, partySlot))
        {
            int battleStatus = BattleContexts.BattleMon_Get(battleCtx, battler, BattleMonParamStatus, null);
            battleStatus &= (int)clearMask;
            BattleContexts.BattleMon_Set(battleCtx, battler, BattleMonParamStatus, battleStatus);
            if (clearNightmare)
            {
                int volatileStatus = BattleContexts.BattleMon_Get(battleCtx, battler, BattleMonParamStatusVolatile, null);
                volatileStatus &= unchecked((int)UseVolatileNightmareClear);
                BattleContexts.BattleMon_Set(battleCtx, battler, BattleMonParamStatusVolatile, volatileStatus);
            }
        }
        return true;
    }

    private static bool UseItem_RaiseStage(BattleContext battleCtx, int battler, int selectedSlot, int partnerSelectedSlot, int partySlot, int param)
    {
        if (UseItem_TargetsActive(selectedSlot, partnerSelectedSlot, partySlot) && BattleContexts.BattleMon_Get(battleCtx, battler, param, null) < StageMax)
        {
            BattleContexts.Battler_AddVal(battleCtx, battler, param, 1);
            return true;
        }
        return false;
    }

    private static bool UseItem_RestorePP(BattleContext battleCtx, Pokemon mon, int battler, int selectedSlot, int partnerSelectedSlot, int partySlot, int moveSlot, int amount)
    {
        if (Pokemon.Pokemon_GetValue(mon, MonParamMove1PP + moveSlot, null) == Pokemon.Pokemon_GetValue(mon, MonParamMove1MaxPP + moveSlot, null))
        {
            return false;
        }
        Pokemon.Pokemon_IncreaseValue(mon, MonParamMove1PP + moveSlot, amount);
        if (UseItem_TargetsActive(selectedSlot, partnerSelectedSlot, partySlot)
            && (BattleContexts.BattleMon_Get(battleCtx, battler, BattleMonParamStatusVolatile, null) & UseVolatileTransformed) == 0)
        {
            int mimicked = BattleContexts.BattleMon_Get(battleCtx, battler, BattleMonParamMimickedSlots, null);
            if ((HealthBar.FlagIndex(moveSlot) & mimicked) == 0)
            {
                BattleContexts.Battler_AddVal(battleCtx, battler, BattleMonParamMove1PP + moveSlot, amount);
            }
        }
        return true;
    }

    // OFFSET 0x0223E30C
    public static int BattleSystem_UseBagItem(BattleSystem battleSys, int battler, int partySlot, int moveSlot, int item)
    {
        BattleContext battleCtx = battleSys.BattleCtx!;
        int used = 0;
        ushort itemID = (ushort)item;
        int selectedSlot = BattleContexts.BattleContext_Get(battleSys, battleCtx, CtxParamSelectedPartySlot, battler);
        int partnerSelectedSlot;
        if (BattleSystem_GetBattleType(battleSys) == UseItemBattleTypeDoublesTrainer
            || ((BattleSystem_GetBattleType(battleSys) & UseItemBattleTypeTag) != 0 && (BattleSystem_BattlerSlot(battleSys, battler) & 1) == 0))
        {
            int partner = BattleSystem_GetPartner(battleSys, battler);
            partnerSelectedSlot = BattleContexts.BattleContext_Get(battleSys, battleCtx, CtxParamSelectedPartySlot, partner);
            if (partnerSelectedSlot == partySlot)
            {
                battler = BattleSystem_GetPartner(battleSys, battler);
            }
        }
        else
        {
            partnerSelectedSlot = selectedSlot;
        }
        Pokemon mon = BattleSystem_GetPartyPokemon(battleSys, battler, partySlot);
        int friendshipDelta = 0;
        if (Item.Item_LoadParam(itemID, ItemParamHealSleep, UseItemHeap) != 0 && UseItem_ClearStatus(battleCtx, mon, battler, selectedSlot, partnerSelectedSlot, partySlot, UseStatusSleep, ~UseStatusSleep, true))
        {
            used = 1;
        }
        if (Item.Item_LoadParam(itemID, ItemParamHealPoison, UseItemHeap) != 0 && UseItem_ClearStatus(battleCtx, mon, battler, selectedSlot, partnerSelectedSlot, partySlot, UseStatusPoison, UseStatusPoisonClear, false))
        {
            used = 1;
        }
        if (Item.Item_LoadParam(itemID, ItemParamHealBurn, UseItemHeap) != 0 && UseItem_ClearStatus(battleCtx, mon, battler, selectedSlot, partnerSelectedSlot, partySlot, UseStatusBurn, ~UseStatusBurn, false))
        {
            used = 1;
        }
        if (Item.Item_LoadParam(itemID, ItemParamHealFreeze, UseItemHeap) != 0 && UseItem_ClearStatus(battleCtx, mon, battler, selectedSlot, partnerSelectedSlot, partySlot, UseStatusFreeze, ~UseStatusFreeze, false))
        {
            used = 1;
        }
        if (Item.Item_LoadParam(itemID, ItemParamHealParalysis, UseItemHeap) != 0 && UseItem_ClearStatus(battleCtx, mon, battler, selectedSlot, partnerSelectedSlot, partySlot, UseStatusParalysis, ~UseStatusParalysis, false))
        {
            used = 1;
        }
        if (Item.Item_LoadParam(itemID, ItemParamHealConfusion, UseItemHeap) != 0 && UseItem_TargetsActive(selectedSlot, partnerSelectedSlot, partySlot))
        {
            int volatileStatus = BattleContexts.BattleMon_Get(battleCtx, battler, BattleMonParamStatusVolatile, null);
            if ((volatileStatus & UseVolatileConfusion) != 0)
            {
                volatileStatus &= ~(int)UseVolatileConfusion;
                BattleContexts.BattleMon_Set(battleCtx, battler, BattleMonParamStatusVolatile, volatileStatus);
                used = 1;
            }
        }
        if (Item.Item_LoadParam(itemID, ItemParamHealAttract, UseItemHeap) != 0 && UseItem_TargetsActive(selectedSlot, partnerSelectedSlot, partySlot))
        {
            int volatileStatus = BattleContexts.BattleMon_Get(battleCtx, battler, BattleMonParamStatusVolatile, null);
            if ((volatileStatus & UseVolatileAttraction) != 0)
            {
                volatileStatus &= ~(int)UseVolatileAttraction;
                BattleContexts.BattleMon_Set(battleCtx, battler, BattleMonParamStatusVolatile, volatileStatus);
                used = 1;
            }
        }
        if (Item.Item_LoadParam(itemID, ItemParamGuardSpec, UseItemHeap) != 0 && BattleContexts.BattleContext_Get(battleSys, battleCtx, CtxParamMistTurns, battler) == 0)
        {
            int mask = BattleContexts.BattleContext_Get(battleSys, battleCtx, CtxParamSideConditionsMask, battler) | UseSideConditionMist;
            BattleContexts.BattleContext_Set(battleSys, battleCtx, CtxParamSideConditionsMask, battler, mask);
            BattleContexts.BattleContext_Set(battleSys, battleCtx, CtxParamMistTurns, battler, UseMistTurns);
            used = 1;
        }
        if (Item.Item_LoadParam(itemID, ItemParamAttackUp, UseItemHeap) != 0 && UseItem_RaiseStage(battleCtx, battler, selectedSlot, partnerSelectedSlot, partySlot, BattleMonParamAttackStage))
        {
            used = 1;
        }
        if (Item.Item_LoadParam(itemID, ItemParamDefenseUp, UseItemHeap) != 0 && UseItem_RaiseStage(battleCtx, battler, selectedSlot, partnerSelectedSlot, partySlot, BattleMonParamDefenseStage))
        {
            used = 1;
        }
        if (Item.Item_LoadParam(itemID, ItemParamSpAttackUp, UseItemHeap) != 0 && UseItem_RaiseStage(battleCtx, battler, selectedSlot, partnerSelectedSlot, partySlot, BattleMonParamSpAttackStage))
        {
            used = 1;
        }
        if (Item.Item_LoadParam(itemID, ItemParamSpDefenseUp, UseItemHeap) != 0 && UseItem_RaiseStage(battleCtx, battler, selectedSlot, partnerSelectedSlot, partySlot, BattleMonParamSpDefenseStage))
        {
            used = 1;
        }
        if (Item.Item_LoadParam(itemID, ItemParamSpeedUp, UseItemHeap) != 0 && UseItem_RaiseStage(battleCtx, battler, selectedSlot, partnerSelectedSlot, partySlot, BattleMonParamSpeedStage))
        {
            used = 1;
        }
        if (Item.Item_LoadParam(itemID, ItemParamAccuracyUp, UseItemHeap) != 0 && UseItem_RaiseStage(battleCtx, battler, selectedSlot, partnerSelectedSlot, partySlot, BattleMonParamAccuracyStage))
        {
            used = 1;
        }
        if (Item.Item_LoadParam(itemID, ItemParamCriticalUp, UseItemHeap) != 0 && UseItem_TargetsActive(selectedSlot, partnerSelectedSlot, partySlot))
        {
            int volatileStatus = BattleContexts.BattleMon_Get(battleCtx, battler, BattleMonParamStatusVolatile, null);
            if ((volatileStatus & UseVolatileFocusEnergy) == 0)
            {
                volatileStatus |= (int)UseVolatileFocusEnergy;
                BattleContexts.BattleMon_Set(battleCtx, battler, BattleMonParamStatusVolatile, volatileStatus);
                used = 1;
            }
        }
        if (Item.Item_LoadParam(itemID, ItemParamPPRestore, UseItemHeap) != 0)
        {
            int amount = Item.Item_LoadParam(itemID, ItemParamPPRestoreAmount, UseItemHeap);
            if (UseItem_RestorePP(battleCtx, mon, battler, selectedSlot, partnerSelectedSlot, partySlot, moveSlot, amount))
            {
                used = 1;
            }
        }
        if (Item.Item_LoadParam(itemID, ItemParamPPRestoreAll, UseItemHeap) != 0)
        {
            int amount = Item.Item_LoadParam(itemID, ItemParamPPRestoreAmount, UseItemHeap);
            for (int slot = 0; slot < 4; slot++)
            {
                if (UseItem_RestorePP(battleCtx, mon, battler, selectedSlot, partnerSelectedSlot, partySlot, slot, amount))
                {
                    used = 1;
                }
            }
        }
        if (Item.Item_LoadParam(itemID, ItemParamHPRestore, UseItemHeap) != 0)
        {
            int canRestore = 0;
            if (Item.Item_LoadParam(itemID, ItemParamRevive, UseItemHeap) != 0)
            {
                if (Pokemon.Pokemon_GetValue(mon, MonParamCurHP, null) == 0)
                {
                    canRestore = 1;
                }
            }
            else
            {
                canRestore = (int)Pokemon.Pokemon_GetValue(mon, MonParamCurHP, null);
            }
            if (canRestore != 0 && Pokemon.Pokemon_GetValue(mon, MonParamCurHP, null) != Pokemon.Pokemon_GetValue(mon, MonParamMaxHP, null))
            {
                int amount = Item.Item_LoadParam(itemID, ItemParamHPRestoreAmount, UseItemHeap);
                switch (amount)
                {
                    case HPRestoreFull:
                        amount = (int)Pokemon.Pokemon_GetValue(mon, MonParamMaxHP, null);
                        break;
                    case HPRestoreHalf:
                        amount = (int)(Pokemon.Pokemon_GetValue(mon, MonParamMaxHP, null) >> 1);
                        if (amount == 0)
                        {
                            amount = 1;
                        }
                        break;
                    case HPRestoreQuarter:
                        amount = (int)(Pokemon.Pokemon_GetValue(mon, MonParamMaxHP, null) * 25 / 100);
                        if (amount == 0)
                        {
                            amount = 1;
                        }
                        break;
                }
                Pokemon.Pokemon_IncreaseValue(mon, MonParamCurHP, amount);
                if (Item.Item_LoadParam(itemID, ItemParamRevive, UseItemHeap) == 0)
                {
                    if (BattleSystem_GetBattlerSide(battleSys, battler) != 0)
                    {
                        BattleContexts.BattleMon_Set(battleCtx, battler, BattleMonParamEnemyHPRestore, amount);
                    }
                    else if (UseItem_TargetsActive(selectedSlot, partnerSelectedSlot, partySlot))
                    {
                        BattleContexts.Battler_AddVal(battleCtx, battler, BattleMonParamCurHP, amount);
                    }
                }
                used = 1;
            }
        }
        if (Item.Item_LoadParam(itemID, ItemParamFriendshipLow, UseItemHeap) != 0 && Pokemon.Pokemon_GetValue(mon, MonParamFriendship, null) < FriendshipMid && used == 1)
        {
            friendshipDelta = Item.Item_LoadParam(itemID, ItemParamFriendshipLowAmount, UseItemHeap);
        }
        if (Item.Item_LoadParam(itemID, ItemParamFriendshipMid, UseItemHeap) != 0
            && Pokemon.Pokemon_GetValue(mon, MonParamFriendship, null) >= FriendshipMid
            && Pokemon.Pokemon_GetValue(mon, MonParamFriendship, null) < FriendshipHigh
            && used == 1)
        {
            friendshipDelta = Item.Item_LoadParam(itemID, ItemParamFriendshipMidAmount, UseItemHeap);
        }
        if (Item.Item_LoadParam(itemID, ItemParamFriendshipHigh, UseItemHeap) != 0 && Pokemon.Pokemon_GetValue(mon, MonParamFriendship, null) >= FriendshipHigh && used == 1)
        {
            friendshipDelta = Item.Item_LoadParam(itemID, ItemParamFriendshipHighAmount, UseItemHeap);
        }
        if (friendshipDelta != 0)
        {
            if (friendshipDelta > 0)
            {
                if (Pokemon.Pokemon_GetValue(mon, MonParamPokeball, null) == BallLuxury)
                {
                    friendshipDelta++;
                }
                if (Pokemon.Pokemon_GetValue(mon, MonParamMetLocation, null) == BattleSystem_GetMapLabelTextID(battleSys))
                {
                    friendshipDelta++;
                }
                ushort heldItem = (ushort)Pokemon.Pokemon_GetValue(mon, MonParamHeldItem, null);
                if (Item.Item_LoadParam(heldItem, ItemParamHoldEffect, UseItemHeap) == HoldEffectSootheBell)
                {
                    friendshipDelta = friendshipDelta * 150 / 100;
                }
            }
            Pokemon.Pokemon_IncreaseValue(mon, MonParamFriendship, friendshipDelta);
            if (UseItem_TargetsActive(selectedSlot, partnerSelectedSlot, partySlot))
            {
                BattleContexts.Battler_AddVal(battleCtx, battler, BattleMonParamFriendship, friendshipDelta);
            }
        }
        return used;
    }
}
