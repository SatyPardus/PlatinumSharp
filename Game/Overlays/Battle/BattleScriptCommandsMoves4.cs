namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int SpitePPMax = 4;
    private const int MoveHealBell = 0xD7;
    private const int AbilitySoundproof = 0x2B;
    private const int AbilityStickyHold = 0x3C;
    private const uint VolatileNightmare = 0x8000000;
    private const int SelectorAttackerPartner = 0x10;
    private const int RefreshAttackerBlocked = 5;
    private const int RefreshPartnerBlocked = 0xA;
    private const int RefreshNoPartner = 8;
    private const uint BattleTypeStealAllowed = 0x84;
    private const int ItemGriseousOrb = 0x70;
    private const int MoveProtect = 0xB6;
    private const int MoveDetect = 0xC5;
    private const int MoveEndure = 0xCB;
    private const uint ProtectChanceTableAddress = 0x0226E57C;
    private const int EffectProtect = 0x6F;
    private const int EffectEndure = 0x74;
    private const int MsgProtectedItself = 0x11A;
    private const int MsgBracedItself = 0x1BA;
    private const int TagNicknameOnly = 2;
    private const int ProtectSuccessMax = 3;

    // OFFSET 0x022446F4
    private static int BattleScript_TrySpite(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        if (battleCtx.MovePrevByBattler[battleCtx.Defender] == 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        int moveSlot = BattleContexts.Battler_SlotForMove(battleCtx.BattleMons[battleCtx.Defender], battleCtx.MovePrevByBattler[battleCtx.Defender]);
        int pp;
        if (moveSlot == 4 || (pp = battleCtx.BattleMons[battleCtx.Defender].PpCur[moveSlot]) == 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        int ppLoss = SpitePPMax;
        if (pp < SpitePPMax)
        {
            ppLoss = pp;
        }
        battleCtx.MsgMoveTemp = battleCtx.MovePrevByBattler[battleCtx.Defender];
        battleCtx.MsgTemp = ppLoss;
        battleCtx.BattleMons[battleCtx.Defender].PpCur[moveSlot] = (byte)(battleCtx.BattleMons[battleCtx.Defender].PpCur[moveSlot] - ppLoss);
        BattleContexts.BattleMon_CopyToParty(battleSys, battleCtx, battleCtx.Defender);
        return 0;
    }

    private static void BattleScript_CureStatus(BattleContext battleCtx, int battler)
    {
        battleCtx.BattleMons[battler].Status = 0;
        battleCtx.BattleMons[battler].StatusVolatile &= ~VolatileNightmare;
    }

    // OFFSET 0x02244798
    private static int BattleScript_TryPartyStatusRefresh(BattleSystem battleSys, BattleContext battleCtx)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        BattleScript_Iter(battleCtx, 1);
        battleCtx.CalcTemp = 0;
        if (battleCtx.MoveCur == MoveHealBell)
        {
            battleCtx.MsgMoveTemp = (int)battleCtx.MoveCur;
            if (BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) != AbilitySoundproof)
            {
                BattleScript_CureStatus(battleCtx, battleCtx.Attacker);
            }
            else
            {
                battleCtx.CalcTemp |= RefreshAttackerBlocked;
            }
            if ((battleType & BattleTypeDoubles) != 0)
            {
                int partner = BattleScript_Battler(battleSys, battleCtx, SelectorAttackerPartner);
                if ((HealthBar.FlagIndex(partner) & battleCtx.BattlersSwitchingMask) == 0)
                {
                    if (BattleContexts.Battler_IgnorableAbility(battleCtx, battleCtx.Attacker, partner, AbilitySoundproof) == 0)
                    {
                        BattleScript_CureStatus(battleCtx, partner);
                    }
                    else
                    {
                        battleCtx.MsgBattlerTemp = partner;
                        battleCtx.CalcTemp |= RefreshPartnerBlocked;
                    }
                }
            }
            else
            {
                battleCtx.CalcTemp |= RefreshNoPartner;
            }
        }
        else
        {
            BattleScript_CureStatus(battleCtx, battleCtx.Attacker);
            if ((battleType & BattleTypeDoubles) != 0)
            {
                int partner = BattleScript_Battler(battleSys, battleCtx, SelectorAttackerPartner);
                if ((HealthBar.FlagIndex(partner) & battleCtx.BattlersSwitchingMask) == 0)
                {
                    BattleScript_CureStatus(battleCtx, partner);
                }
            }
            else
            {
                battleCtx.CalcTemp |= RefreshNoPartner;
            }
        }
        BattleIO.BattleIO_RefreshPartyStatus(battleSys, battleCtx, battleCtx.Attacker, (int)battleCtx.MoveCur);
        return 0;
    }

    // OFFSET 0x022448E8
    private static int BattleScript_TryStealItem(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int jumpOnStickyHold = BattleScript_Read(battleCtx);
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int attackerSide = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
        if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker) != 0 && (battleType & BattleTypeStealAllowed) == 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else if ((HealthBar.FlagIndex(battleCtx.SelectedPartySlot[battleCtx.Attacker]) & battleCtx.SideConditions[attackerSide].KnockedOffItemsMask) != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else if (BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) == AbilityMultitype || BattleContexts.Battler_Ability(battleCtx, battleCtx.Defender) == AbilityMultitype)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else if (battleCtx.BattleMons[battleCtx.Defender].HeldItem == ItemGriseousOrb)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else if (battleCtx.BattleMons[battleCtx.Defender].MoveEffectsData.CustapBerry != 0 || battleCtx.BattleMons[battleCtx.Defender].MoveEffectsData.QuickClaw != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else if (battleCtx.BattleMons[battleCtx.Defender].HeldItem != 0 && BattleContexts.Battler_IgnorableAbility(battleCtx, battleCtx.Attacker, battleCtx.Defender, AbilityStickyHold) == 1)
        {
            BattleScript_Iter(battleCtx, jumpOnStickyHold);
        }
        else if (battleCtx.BattleMons[battleCtx.Attacker].HeldItem != 0 || BattleContexts.Battler_CanLoseItem(battleSys, battleCtx, battleCtx.Defender) == 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x02244A0C
    private static int BattleScript_TryProtection(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        ushort protectMove = battleCtx.MoveProtect[battleCtx.Attacker];
        if (protectMove != MoveProtect && protectMove != MoveDetect && protectMove != MoveEndure)
        {
            battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.ProtectSuccessTurns = 0;
        }
        bool notLastBattler = battleCtx.WaitingBattlers != 1;
        MoveEffectsData effects = battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData;
        int rand = BattleSystems.BattleSystem_RandNext(battleSys);
        ushort chance = RomData.OverlayU16(BattleOverlay, (uint)(ProtectChanceTableAddress + effects.ProtectSuccessTurns * 2), 1)[0];
        if (chance >= rand && notLastBattler)
        {
            if (battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Effect == EffectProtect)
            {
                battleCtx.TurnFlags[battleCtx.Attacker].Protecting = 1;
                battleCtx.MsgBuffer.Id = MsgProtectedItself;
            }
            if (battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Effect == EffectEndure)
            {
                battleCtx.TurnFlags[battleCtx.Attacker].Enduring = 1;
                battleCtx.MsgBuffer.Id = MsgBracedItself;
            }
            battleCtx.MsgBuffer.Tags = TagNicknameOnly;
            battleCtx.MsgBuffer.Params[0] = BattleContexts.BattleSystem_NicknameTag(battleCtx, battleCtx.Attacker);
            MoveEffectsData attackerEffects = battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData;
            if (attackerEffects.ProtectSuccessTurns < ProtectSuccessMax)
            {
                attackerEffects.ProtectSuccessTurns = (attackerEffects.ProtectSuccessTurns + 1) & 3;
            }
        }
        else
        {
            effects.ProtectSuccessTurns = 0;
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }
}

public static partial class BattleIO
{
    private const int CommandRefreshPartyStatus = 0x2A;

    // OFFSET 0x02266498
    public static void BattleIO_RefreshPartyStatus(BattleSystem battleSys, BattleContext battleCtx, int battler, int move)
    {
        byte[] message = new byte[4];
        W8(message, 0, CommandRefreshPartyStatus);
        W16(message, 2, move);
        W8(message, 1, battleCtx.BattleMons[battler].Ability);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }
}
