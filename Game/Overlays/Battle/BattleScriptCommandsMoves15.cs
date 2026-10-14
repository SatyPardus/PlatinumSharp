namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int ToxicSpikesMaxLayers = 2;
    private const int SideEffectToxicSpikes = 6;
    private const int TypePoison = 3;
    private const int IgnorableCheckHave = 0;
    private const int SelectorAllBySpeed = 0;
    private const int MonDataHeldItemValue = 6;
    private const int MonDataAbilityValue = 0xA;
    private const int AbilityPickup = 0x35;
    private const int AbilityHoneyGather = 0x76;
    private const int ItemHoney = 0x5E;
    private const uint PickupChanceTableAddress = 0x0226EAA8;
    private const uint PickupItemTableAddress = 0x0226E66C;
    private const uint PickupRareItemTableAddress = 0x0226E590;
    private const uint HoneyGatherChanceTableAddress = 0x0226EAB4;
    private const int PickupCommonSlots = 9;
    private const int PickupMaxLevelBracket = 9;

    // OFFSET 0x022473F4
    private static int BattleScript_TryLastResort(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int moveCount = BattleContexts.Battler_CountMoves(battleSys, battleCtx, battleCtx.Attacker);
        int used = battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.LastResortCount;
        if ((uint)used < (uint)(moveCount - 1) || moveCount < 2)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x02247440
    private static int BattleScript_TryToxicSpikes(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker) ^ 1;
        if (battleCtx.SideConditions[side].ToxicSpikesLayers == ToxicSpikesMaxLayers)
        {
            battleCtx.SelfTurnFlags[battleCtx.Attacker].SkipPressureCheck = 1;
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else
        {
            battleCtx.SideConditionsMask[side] |= SideConditionToxicSpikes;
            battleCtx.SideConditions[side].ToxicSpikesLayers = (battleCtx.SideConditions[side].ToxicSpikesLayers + 1) & 3;
        }
        return 0;
    }

    // OFFSET 0x022474C4
    private static int BattleScript_CheckToxicSpikes(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int jumpIfNone = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler);
        int layers = battleCtx.SideConditions[side].ToxicSpikesLayers;
        if (layers == 0)
        {
            BattleScript_Iter(battleCtx, jumpIfNone);
            return 0;
        }
        battleCtx.CalcTemp = layers;
        battleCtx.SideEffectType = SideEffectToxicSpikes;
        battleCtx.SideEffectMon = battler;
        if (BattleContexts.BattleMon_Get(battleCtx, battleCtx.SwitchedMon, BattleMonType1, null) == TypePoison || BattleContexts.BattleMon_Get(battleCtx, battleCtx.SwitchedMon, BattleMonType2, null) == TypePoison)
        {
            battleCtx.SideConditionsMask[side] &= ~SideConditionToxicSpikes;
            battleCtx.SideConditions[side].ToxicSpikesLayers = 0;
            battleCtx.CalcTemp = 0;
        }
        return 0;
    }

    private static bool BattleScript_IgnorableAbilityMatches(BattleContext battleCtx, int mode, int battler, int ability)
    {
        if (mode == IgnorableCheckHave)
        {
            return BattleContexts.Battler_IgnorableAbility(battleCtx, battleCtx.Attacker, battler, ability) == 1 && battleCtx.BattleMons[battler].CurHP != 0;
        }
        return BattleContexts.Battler_IgnorableAbility(battleCtx, battleCtx.Attacker, battler, ability) == 0 || battleCtx.BattleMons[battler].CurHP == 0;
    }

    // OFFSET 0x02247570
    private static int BattleScript_CheckIgnorableAbility(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int mode = BattleScript_Read(battleCtx);
        int selector = BattleScript_Read(battleCtx);
        int ability = BattleScript_Read(battleCtx);
        int jump = BattleScript_Read(battleCtx);
        if (selector == SelectorAllBySpeed)
        {
            int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
            for (int i = 0; i < maxBattlers; i++)
            {
                int battler = battleCtx.MonSpeedOrder[i];
                if (BattleScript_IgnorableAbilityMatches(battleCtx, mode, battler, ability))
                {
                    BattleScript_Iter(battleCtx, jump);
                    battleCtx.AbilityMon = battler;
                    break;
                }
            }
        }
        else
        {
            int battler = BattleScript_Battler(battleSys, battleCtx, selector);
            if (BattleScript_IgnorableAbilityMatches(battleCtx, mode, battler, ability))
            {
                BattleScript_Iter(battleCtx, jump);
                battleCtx.AbilityMon = battler;
            }
        }
        return 0;
    }

    // OFFSET 0x02247698
    private static int BattleScript_IfSameSide(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector1 = BattleScript_Read(battleCtx);
        int selector2 = BattleScript_Read(battleCtx);
        int jump = BattleScript_Read(battleCtx);
        int battler1 = BattleScript_Battler(battleSys, battleCtx, selector1);
        int battler2 = BattleScript_Battler(battleSys, battleCtx, selector2);
        if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler1) == BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler2))
        {
            BattleScript_Iter(battleCtx, jump);
        }
        return 0;
    }

    // OFFSET 0x022476F8
    private static int BattleScript_GenerateEndOfBattleItem(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        byte[] pickupChances = RomData.Overlay(BattleOverlay, PickupChanceTableAddress, PickupCommonSlots);
        for (int slot = 0; slot < BattleSystems.BattleSystem_GetPartyCount(battleSys, 0); slot++)
        {
            Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, 0, slot);
            ushort species = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEggBeatUp, null);
            ushort item = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataHeldItemValue, null);
            byte ability = (byte)Pokemon.Pokemon_GetValue(mon, MonDataAbilityValue, null);
            if (ability == AbilityPickup && species != 0 && species != SpeciesEgg && item == 0 && BattleSystems.BattleSystem_RandNext(battleSys) % 10 == 0)
            {
                int rand = BattleSystems.BattleSystem_RandNext(battleSys) % 100;
                byte bracket = (byte)((Pokemon.Pokemon_GetValue(mon, MonDataLevel, null) - 1) / 10);
                if (bracket >= 10)
                {
                    bracket = PickupMaxLevelBracket;
                }
                for (int i = 0; i < PickupCommonSlots; i++)
                {
                    if (pickupChances[i] > rand)
                    {
                        ushort newItem = RomData.OverlayU16(BattleOverlay, (uint)(PickupItemTableAddress + (bracket + i) * 2), 1)[0];
                        Pokemon.Pokemon_SetValue(mon, MonDataHeldItemValue, newItem);
                        break;
                    }
                    if (rand >= 0x62 && rand <= 0x63)
                    {
                        ushort newItem = RomData.OverlayU16(BattleOverlay, (uint)(PickupRareItemTableAddress + (bracket + (0x63 - rand)) * 2), 1)[0];
                        Pokemon.Pokemon_SetValue(mon, MonDataHeldItemValue, newItem);
                        break;
                    }
                }
            }
            if (ability == AbilityHoneyGather && species != 0 && species != SpeciesEgg && item == 0)
            {
                int bracket = 0;
                int threshold = 10;
                byte level = (byte)Pokemon.Pokemon_GetValue(mon, MonDataLevel, null);
                while (threshold < level)
                {
                    threshold += 10;
                    bracket++;
                }
                byte chance = RomData.Overlay(BattleOverlay, (uint)(HoneyGatherChanceTableAddress + bracket), 1)[0];
                if (BattleSystems.BattleSystem_RandNext(battleSys) % 100 < chance)
                {
                    Pokemon.Pokemon_SetValue(mon, MonDataHeldItemValue, (ushort)ItemHoney);
                }
            }
        }
        return 0;
    }

    // OFFSET 0x0224787C
    private static int BattleScript_TrickRoom(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        battleCtx.SelfTurnFlags[battleCtx.Attacker].TrickRoomActivated = 1;
        return 0;
    }

    // OFFSET 0x022478A4
    private static int BattleScript_IfMovedThisTurn(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int jump = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        if (BattleContexts.Battler_MovedThisTurn(battleCtx, battler) == 1)
        {
            BattleScript_Iter(battleCtx, jump);
        }
        return 0;
    }
}
