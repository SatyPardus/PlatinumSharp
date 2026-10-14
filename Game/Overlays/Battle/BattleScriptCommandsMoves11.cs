namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int RevengeBoosted = 20;
    private const int RevengeNormal = 10;
    private const uint MoveEffectYawnAny = 0x1800;
    private const uint MoveEffectYawn = 0x1000;
    private const int MsgStickyHoldPrevents = 0x2CA;
    private const int TagNicknameAbilityMove = 0x25;
    private const int MsgKnockedOffItem = 0x228;
    private const int TagNicknameNicknameItem = 0x21;
    private const uint MoveEffectImprisonedBy = 0x40000000;
    private const uint MoveEffectImprison = 0x2000;

    // OFFSET 0x022463E8
    private static int BattleScript_CalcRevengePowerMul(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        TurnFlags flags = battleCtx.TurnFlags[battleCtx.Attacker];
        int defender = battleCtx.Defender;
        if ((flags.PhysicalDamageTakenFrom[defender] != 0 && (HealthBar.FlagIndex(defender) & (uint)flags.PhysicalDamageAttackerMask) != 0)
            || (flags.SpecialDamageTakenFrom[battleCtx.Defender] != 0 && (HealthBar.FlagIndex(battleCtx.Defender) & (uint)battleCtx.TurnFlags[battleCtx.Attacker].SpecialDamageAttackerMask) != 0))
        {
            battleCtx.PowerMul = RevengeBoosted;
        }
        else
        {
            battleCtx.PowerMul = RevengeNormal;
        }
        return 0;
    }

    // OFFSET 0x02246458
    private static int BattleScript_TryBreakScreens(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Defender);
        if ((battleCtx.SideConditionsMask[side] & SideConditionReflect) != 0 || (battleCtx.SideConditionsMask[side] & SideConditionLightScreen) != 0)
        {
            battleCtx.SideConditionsMask[side] &= ~SideConditionReflect;
            battleCtx.SideConditionsMask[side] &= ~SideConditionLightScreen;
            battleCtx.SideConditions[side].ReflectTurns = 0;
            battleCtx.SideConditions[side].LightScreenTurns = 0;
        }
        else
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x022464CC
    private static int BattleScript_TryYawn(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        BattleMon defender = battleCtx.BattleMons[battleCtx.Defender];
        if ((defender.MoveEffectsMask & MoveEffectYawnAny) != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else
        {
            defender.MoveEffectsMask |= MoveEffectYawn;
        }
        return 0;
    }

    // OFFSET 0x0224650C
    private static int BattleScript_TryKnockOff(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Defender);
        if (battleCtx.BattleMons[battleCtx.Defender].HeldItem != 0 && BattleContexts.Battler_IgnorableAbility(battleCtx, battleCtx.Attacker, battleCtx.Defender, AbilityStickyHold) == 1)
        {
            battleCtx.MsgBuffer.Id = MsgStickyHoldPrevents;
            battleCtx.MsgBuffer.Tags = TagNicknameAbilityMove;
            battleCtx.MsgBuffer.Params[0] = BattleContexts.BattleSystem_NicknameTag(battleCtx, battleCtx.Defender);
            battleCtx.MsgBuffer.Params[1] = battleCtx.BattleMons[battleCtx.Defender].Ability;
            battleCtx.MsgBuffer.Params[2] = (int)battleCtx.MoveCur;
        }
        else if (battleCtx.BattleMons[battleCtx.Defender].HeldItem != 0)
        {
            battleCtx.MsgBuffer.Id = MsgKnockedOffItem;
            battleCtx.MsgBuffer.Tags = TagNicknameNicknameItem;
            battleCtx.MsgBuffer.Params[0] = BattleContexts.BattleSystem_NicknameTag(battleCtx, battleCtx.Attacker);
            battleCtx.MsgBuffer.Params[1] = BattleContexts.BattleSystem_NicknameTag(battleCtx, battleCtx.Defender);
            battleCtx.MsgBuffer.Params[2] = battleCtx.BattleMons[battleCtx.Defender].HeldItem;
            battleCtx.BattleMons[battleCtx.Defender].HeldItem = 0;
            SideConditions conditions = battleCtx.SideConditions[side];
            conditions.KnockedOffItemsMask = (int)((HealthBar.FlagIndex(battleCtx.SelectedPartySlot[battleCtx.Defender]) | (uint)conditions.KnockedOffItemsMask) & 0x3F);
        }
        else
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x02246630
    private static int BattleScript_CalcHPFalloffPower(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        if (battleCtx.MovePower == 0)
        {
            BattleMon attacker = battleCtx.BattleMons[battleCtx.Attacker];
            battleCtx.MovePower = (int)((uint)(battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Power * attacker.CurHP) / attacker.MaxHP);
            if (battleCtx.MovePower == 0)
            {
                battleCtx.MovePower = 1;
            }
        }
        return 0;
    }

    // OFFSET 0x02246688
    private static int BattleScript_TryImprison(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int enemy1 = BattleSystems.BattleSystem_GetEnemyInSlot(battleSys, battleCtx.Attacker, 0);
        int enemy2 = BattleSystems.BattleSystem_GetEnemyInSlot(battleSys, battleCtx.Attacker, 2);
        battleCtx.BattleMons[enemy1].MoveEffectsMask |= MoveEffectImprisonedBy;
        battleCtx.BattleMons[enemy2].MoveEffectsMask |= MoveEffectImprisonedBy;
        if ((battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsMask & MoveEffectImprison) != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        int attackerSide = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        int battler;
        for (battler = 0; battler < maxBattlers; battler++)
        {
            if (attackerSide == BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler))
            {
                continue;
            }
            int j = 4;
            for (int i = 0; i < 4; i++)
            {
                ushort attackerMove = battleCtx.BattleMons[battleCtx.Attacker].Moves[i];
                for (j = 0; j < 4; j++)
                {
                    ushort targetMove = battleCtx.BattleMons[battler].Moves[j];
                    if (attackerMove == targetMove && attackerMove != 0 && targetMove != 0)
                    {
                        break;
                    }
                }
                if (j != 4)
                {
                    break;
                }
            }
            if (j != 4)
            {
                break;
            }
        }
        if (battler == maxBattlers)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else
        {
            battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsMask |= MoveEffectImprison;
        }
        return 0;
    }
}
