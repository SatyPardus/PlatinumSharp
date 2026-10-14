namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const uint ControllerCommandNone = 0x27;
    private const int WishTurns = 2;
    private const int MonDataMove1 = 0x36;
    private const int RangeUser = 4;
    private const int RangeUserSide = 8;
    private const uint BattleStatus2MagicCoat = 8;

    // OFFSET 0x02246004
    private static int BattleScript_TryHelpingHand(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) == 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        int partner = BattleScript_Battler(battleSys, battleCtx, SelectorAttackerPartner);
        if ((HealthBar.FlagIndex(partner) & battleCtx.BattlersSwitchingMask) == 0
            && battleCtx.BattlerActions[partner, 0] != ControllerCommandNone
            && battleCtx.BattleMons[partner].CurHP != 0
            && battleCtx.TurnFlags[battleCtx.Attacker].HelpingHand == 0
            && battleCtx.TurnFlags[partner].HelpingHand == 0)
        {
            battleCtx.MsgBattlerTemp = partner;
            battleCtx.TurnFlags[partner].HelpingHand = 1;
        }
        else
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x022460A8
    private static int BattleScript_TrySwapItems(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int jumpOnStickyHold = BattleScript_Read(battleCtx);
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int attackerSide = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
        int defenderSide = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Defender);
        if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker) != 0 && (battleType & BattleTypeStealAllowed) == 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else if ((HealthBar.FlagIndex(battleCtx.SelectedPartySlot[battleCtx.Attacker]) & battleCtx.SideConditions[attackerSide].KnockedOffItemsMask) != 0
            || (HealthBar.FlagIndex(battleCtx.SelectedPartySlot[battleCtx.Defender]) & battleCtx.SideConditions[defenderSide].KnockedOffItemsMask) != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else if ((battleCtx.BattleMons[battleCtx.Attacker].HeldItem == 0 && battleCtx.BattleMons[battleCtx.Defender].HeldItem == 0)
            || BattleContexts.Battler_HeldItemIsNotMail(battleCtx, battleCtx.Attacker) == 0
            || BattleContexts.Battler_HeldItemIsNotMail(battleCtx, battleCtx.Defender) == 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else if (BattleContexts.Battler_IgnorableAbility(battleCtx, battleCtx.Attacker, battleCtx.Defender, AbilityStickyHold) == 1)
        {
            BattleScript_Iter(battleCtx, jumpOnStickyHold);
        }
        return 0;
    }

    // OFFSET 0x022461B0
    private static int BattleScript_TryWish(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        if (battleCtx.FieldConditions.WishTurns[battleCtx.Attacker] != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else
        {
            battleCtx.FieldConditions.WishTurns[battleCtx.Attacker] = WishTurns;
            battleCtx.FieldConditions.WishTarget[battleCtx.Attacker] = battleCtx.SelectedPartySlot[battleCtx.Attacker];
        }
        return 0;
    }

    // OFFSET 0x022461F4
    private static int BattleScript_TryAssist(BattleSystem battleSys, BattleContext battleCtx)
    {
        ushort[] availableMoves = new ushort[24];
        int count = 0;
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int partyCount = BattleSystems.BattleSystem_GetPartyCount(battleSys, battleCtx.Attacker);
        for (int slot = 0; slot < partyCount; slot++)
        {
            if (slot == battleCtx.SelectedPartySlot[battleCtx.Attacker])
            {
                continue;
            }
            Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battleCtx.Attacker, slot);
            if (Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEggBeatUp, null) == 0 || Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEggBeatUp, null) == SpeciesEgg)
            {
                continue;
            }
            for (int i = 0; i < 4; i++)
            {
                ushort move = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + i, null);
                if (BattleContexts.Move_IsInvoker(move) == 0 && BattleContexts.Move_CanBeMetronomed(battleSys, battleCtx, battleCtx.Attacker, move) == 1)
                {
                    availableMoves[count] = move;
                    count++;
                }
            }
        }
        if (count != 0)
        {
            battleCtx.MsgMoveTemp = availableMoves[BattleSystems.BattleSystem_RandNext(battleSys) % count];
        }
        else
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x022462D8
    private static int BattleScript_TrySetMagicCoat(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        for (int battler = 0; battler < BattleSystems.BattleSystem_GetMaxBattlers(battleSys); battler++)
        {
        }
        if (battleCtx.WaitingBattlers == 1)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else
        {
            battleCtx.TurnFlags[battleCtx.Attacker].MagicCoat = 1;
        }
        return 0;
    }

    // OFFSET 0x02246334
    private static int BattleScript_MagicCoat(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
        int originalAttacker = battleCtx.Attacker;
        battleCtx.MagicCoatMon = originalAttacker;
        battleCtx.Attacker = battleCtx.Defender;
        SideConditions conditions = battleCtx.SideConditions[side];
        if (conditions.FollowMe != 0 && battleCtx.BattleMons[conditions.FollowMeUser].CurHP != 0)
        {
            battleCtx.Defender = conditions.FollowMeUser;
        }
        else
        {
            uint move = battleCtx.MoveCur;
            ushort range = battleCtx.AiContext.MoveTable[move].Range;
            if (range == RangeUser || range == RangeUserSide)
            {
                battleCtx.Defender = originalAttacker;
            }
            else
            {
                int defender = BattleContexts.BattleSystem_Defender(battleSys, battleCtx, battleCtx.Attacker, (ushort)move, 1, 0);
                if (battleCtx.SelfTurnFlags[defender].LightningRodActivated != 0 || battleCtx.SelfTurnFlags[defender].StormDrainActivated != 0)
                {
                    battleCtx.Defender = defender;
                }
                else
                {
                    battleCtx.Defender = originalAttacker;
                }
            }
        }
        battleCtx.BattleStatusMask2 |= BattleStatus2MagicCoat;
        return 0;
    }
}
