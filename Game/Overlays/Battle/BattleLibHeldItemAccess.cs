namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    public const int HeldItemPowerNormal = 0;
    public const int HeldItemPowerNoEmbargoCheck = 1;
    public const int HeldItemPowerEmbargoOnly = 2;

    private const sbyte StatBoostNeutralStage = 6;
    private const uint VolatileFocusEnergy = 0x100000;
    private const int AbilityKlutz = 0x67;
    private const int HoldEffectLeftovers = 0x45;
    private const int HoldEffectBlackSludge = 0x6D;
    private const int HoldEffectToxicOrb = 0x64;
    private const int HoldEffectFlameOrb = 0x65;
    private const int HoldEffectStickyBarb = 0x74;
    private const int HoldEffectEnigmaBerry = 0x2B;
    private const int HoldEffectJabocaBerry = 0x2E;
    private const int HoldEffectRowapBerry = 0x2F;
    private const int TypePoison = 3;
    private const int MoveKnockOff = 0x11A;
    private const int SubscriptItemRecoverHP = 0xD5;
    private const int SubscriptItemDamage = 0xD7;
    private const int SubscriptStickyBarbTransfer = 0xD8;
    private const int SubscriptItemRecoilDamage = 0x10A;
    private const int SideEffectBadlyPoison = 0x2F;
    private const int ItemParamHoldPower = 2;
    private const int ItemParamPluckEffect = 8;
    private const int ItemParamFlingEffect = 9;
    private const int ItemParamNaturalGiftPower = 0xB;
    private const int ItemParamNaturalGiftType = 0xC;

    private static void BattleContext_LoadHeldItemScript(BattleContext battleCtx, int battler, int script)
    {
        battleCtx.MsgBattlerTemp = battler;
        battleCtx.MsgItemTemp = Battler_HeldItem(battleCtx, battler);
        BattleSystem_LoadScript(battleCtx, 1, script);
        battleCtx.CommandNext = battleCtx.Command;
        battleCtx.Command = CommandRunScriptState;
    }

    // OFFSET 0x02258008
    public static int BattleSystem_TriggerLeftovers(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        int result = 0;
        int script = 0;
        int itemEffect = Battler_HeldItemEffect(battleCtx, battler);
        Battler_HeldItemPower(battleCtx, battler, 0);
        BattleMon mon = battleCtx.BattleMons[battler];
        if (mon.CurHP == 0)
        {
            return result;
        }
        switch (itemEffect)
        {
            case HoldEffectLeftovers:
                if ((uint)mon.CurHP < mon.MaxHP)
                {
                    battleCtx.HpCalcTemp = BattleSystem_Divide((int)mon.MaxHP, 16);
                    script = SubscriptItemRecoverHP;
                    result = 1;
                }
                break;
            case HoldEffectBlackSludge:
                if (BattleMon_Get(battleCtx, battler, BattleMonType1, null) == TypePoison || BattleMon_Get(battleCtx, battler, BattleMonType2, null) == TypePoison)
                {
                    if ((uint)mon.CurHP < mon.MaxHP)
                    {
                        battleCtx.HpCalcTemp = BattleSystem_Divide((int)mon.MaxHP, 16);
                        script = SubscriptItemRecoverHP;
                        result = 1;
                    }
                }
                else if (Battler_Ability(battleCtx, battler) != AbilityMagicGuard)
                {
                    battleCtx.HpCalcTemp = BattleSystem_Divide((int)mon.MaxHP * -1, 8);
                    script = SubscriptItemDamage;
                    result = 1;
                }
                break;
        }
        if (result == 1)
        {
            BattleContext_LoadHeldItemScript(battleCtx, battler, script);
        }
        return result;
    }

    // OFFSET 0x022587A4
    public static int BattleSystem_TriggerDetrimentalHeldItem(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        int result = 0;
        int script = 0;
        int itemEffect = Battler_HeldItemEffect(battleCtx, battler);
        int itemPower = Battler_HeldItemPower(battleCtx, battler, 0);
        BattleMon mon = battleCtx.BattleMons[battler];
        if (mon.CurHP == 0)
        {
            return result;
        }
        switch (itemEffect)
        {
            case HoldEffectToxicOrb:
                battleCtx.SideEffectMon = battler;
                battleCtx.SideEffectType = SideEffectTypeHeldItem;
                script = SideEffectBadlyPoison;
                result = 1;
                break;
            case HoldEffectFlameOrb:
                battleCtx.SideEffectMon = battler;
                battleCtx.SideEffectType = SideEffectTypeHeldItem;
                script = SideEffectBurn;
                result = 1;
                break;
            case HoldEffectStickyBarb:
                if (Battler_Ability(battleCtx, battler) != AbilityMagicGuard)
                {
                    battleCtx.HpCalcTemp = BattleSystem_Divide((int)mon.MaxHP * -1, itemPower);
                    script = SubscriptItemDamage;
                    result = 1;
                }
                break;
        }
        if (result == 1)
        {
            BattleContext_LoadHeldItemScript(battleCtx, battler, script);
        }
        return result;
    }

    // OFFSET 0x02258874
    public static ushort Battler_HeldItem(BattleContext battleCtx, int battler)
    {
        if (Battler_Ability(battleCtx, battler) == AbilityKlutz)
        {
            return 0;
        }
        if (battleCtx.BattleMons[battler].MoveEffectsData.EmbargoTurns != 0)
        {
            return 0;
        }
        return battleCtx.BattleMons[battler].HeldItem;
    }

    // OFFSET 0x022588A4
    public static int Battler_MovedThisTurn(BattleContext battleCtx, int battler)
    {
        return battleCtx.BattlerActions[battler, 0] == BattlerActionSwitchedIn ? 1 : 0;
    }

    // OFFSET 0x022588BC
    public static int BattleSystem_TriggerHeldItemOnHit(BattleSystem battleSys, BattleContext battleCtx, ref int script)
    {
        int result = 0;
        if (battleCtx.Defender == BattlerNone)
        {
            return result;
        }
        if (Battler_SubstituteWasHit(battleCtx, battleCtx.Defender) == 1)
        {
            return result;
        }
        int itemEffect = Battler_HeldItemEffect(battleCtx, battleCtx.Defender);
        int itemPower = Battler_HeldItemPower(battleCtx, battleCtx.Defender, 0);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
        BattleMon attacker = battleCtx.BattleMons[battleCtx.Attacker];
        BattleMon defender = battleCtx.BattleMons[battleCtx.Defender];
        SelfTurnFlags flags = battleCtx.SelfTurnFlags[battleCtx.Defender];
        switch (itemEffect)
        {
            case HoldEffectStickyBarb:
                if (attacker.CurHP != 0 && attacker.HeldItem == 0
                    && (HealthBar.FlagIndex(battleCtx.SelectedPartySlot[battleCtx.Attacker]) & (uint)battleCtx.SideConditions[side].KnockedOffItemsMask) == 0
                    && battleCtx.MoveCur != MoveKnockOff
                    && (flags.PhysicalDamageTaken != 0 || flags.SpecialDamageTaken != 0)
                    && (battleCtx.BattleStatusMask2 & BattleStatus2UTurn) == 0
                    && (battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Flags & MoveFlagContact) != 0)
                {
                    script = SubscriptStickyBarbTransfer;
                    result = 1;
                }
                break;
            case HoldEffectJabocaBerry:
                if (attacker.CurHP != 0 && Battler_Ability(battleCtx, battleCtx.Attacker) != AbilityMagicGuard
                    && (battleCtx.BattleStatusMask2 & BattleStatus2UTurn) == 0 && flags.PhysicalDamageTaken != 0)
                {
                    battleCtx.HpCalcTemp = BattleSystem_Divide((int)attacker.MaxHP * -1, itemPower);
                    script = SubscriptItemRecoilDamage;
                    result = 1;
                }
                break;
            case HoldEffectRowapBerry:
                if (attacker.CurHP != 0 && Battler_Ability(battleCtx, battleCtx.Attacker) != AbilityMagicGuard && flags.SpecialDamageTaken != 0)
                {
                    battleCtx.HpCalcTemp = BattleSystem_Divide((int)attacker.MaxHP * -1, itemPower);
                    script = SubscriptItemRecoilDamage;
                    result = 1;
                }
                break;
            case HoldEffectEnigmaBerry:
                if (defender.CurHP != 0 && (battleCtx.MoveStatusFlags & MoveStatusSuperEffective) != 0)
                {
                    battleCtx.HpCalcTemp = BattleSystem_Divide((int)defender.MaxHP, itemPower);
                    script = SubscriptItemRestoreHP;
                    battleCtx.MsgBattlerTemp = battleCtx.Defender;
                    battleCtx.MsgItemTemp = defender.HeldItem;
                    result = 1;
                }
                break;
        }
        return result;
    }

    // OFFSET 0x02258AB8
    public static int Battler_HeldItemEffect(BattleContext battleCtx, int battler)
    {
        return BattleSystem_GetItemData(battleCtx, Battler_HeldItem(battleCtx, battler), ItemParamHoldEffect);
    }

    // OFFSET 0x02258ACC
    public static int Battler_HeldItemPower(BattleContext battleCtx, int battler, int mode)
    {
        ushort item;
        switch (mode)
        {
            case HeldItemPowerNormal:
                item = Battler_HeldItem(battleCtx, battler);
                break;
            case HeldItemPowerEmbargoOnly:
                if (battleCtx.BattleMons[battler].MoveEffectsData.EmbargoTurns != 0)
                {
                    return 0;
                }
                item = battleCtx.BattleMons[battler].HeldItem;
                break;
            case HeldItemPowerNoEmbargoCheck:
                item = battleCtx.BattleMons[battler].HeldItem;
                break;
            default:
                item = (ushort)battler;
                break;
        }
        return BattleSystem_GetItemData(battleCtx, item, ItemParamHoldPower);
    }

    // OFFSET 0x02258B18
    public static int Battler_NaturalGiftPower(BattleContext battleCtx, int battler)
    {
        return BattleSystem_GetItemData(battleCtx, Battler_HeldItem(battleCtx, battler), ItemParamNaturalGiftPower);
    }

    // OFFSET 0x02258B2C
    public static int Battler_NaturalGiftType(BattleContext battleCtx, int battler)
    {
        return BattleSystem_GetItemData(battleCtx, Battler_HeldItem(battleCtx, battler), ItemParamNaturalGiftType);
    }

    // OFFSET 0x02258B40
    public static int Battler_ItemPluckEffect(BattleContext battleCtx, int battler)
    {
        return BattleSystem_GetItemData(battleCtx, battleCtx.BattleMons[battler].HeldItem, ItemParamPluckEffect);
    }

    // OFFSET 0x02258B58
    public static int Battler_ItemFlingEffect(BattleContext battleCtx, int battler)
    {
        if (battleCtx.BattleMons[battler].MoveEffectsData.EmbargoTurns != 0)
        {
            return 0;
        }
        return BattleSystem_GetItemData(battleCtx, battleCtx.BattleMons[battler].HeldItem, ItemParamFlingEffect);
    }
}
