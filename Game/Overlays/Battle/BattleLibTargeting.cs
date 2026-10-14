namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    public const int BattlerNone = 0xFF;
    public const int CommandRunScriptState = 0x15;
    public const uint MoveStatusNoEffectMask = 0x801FDA49;
    public const int SideEffectTypeDirect = 1;
    public const int SideEffectTypeIndirect = 2;
    public const int SideEffectTypeAbility = 3;

    private const uint SideEffectAlways = 0x20000000;
    private const uint SideEffectCheckHP = 0x10000000;
    private const uint SideEffectCheckSubstitute = 0x01000000;
    private const uint SideEffectCheckHPAndSubstitute = 0x02000000;
    private const uint SideEffectMoveEffect = 0x04000000;
    private const uint SideEffectOnHitEvenMissed = 0x00800000;
    private const uint SideEffectTargetAttacker = 0x40000000;
    private const uint SideEffectTargetDefender = 0x80000000;
    private const uint SideEffectParamMask = 0x007FFFFF;
    private const uint SideEffectFlagsMask = 0xFF800000;
    private const uint MoveStatusMissedOrFailed = 0x40008;
    private const uint MoveStatusMissed = 0x10001;
    private const uint BattleStatusSecondaryEffect = 0x400000;
    private const uint BattleStatusNoRedirect = 0x20;
    private const uint BattleStatusIgnoreTypeEffectiveness = 0x800;
    private const uint BattleStatusIgnoreTypeImmunity = 0x8000;
    private const uint BattleStatusChargeTurn = 0x200;
    private const uint SideEffectSubscriptTable = 0x0226EE24;
    private const int SideEffectSubscriptCount = 0x91;
    private const int AbilityLightningRod = 0x1F;
    private const int AbilitySereneGrace = 0x20;
    private const int AbilityNormalize = 0x60;
    private const int AbilityMoldBreaker = 0x68;
    private const int AbilityStormDrain = 0x72;
    private const int TypeWater = 0xB;
    private const int TypeElectric = 0xD;
    private const int CountAbilityNotAttacker = 9;
    private const int SubscriptRedirectAbility = 0xB4;
    private const int RangeSingleTarget = 0;
    private const int RangeRandomOpponent = 2;
    private const int RangeAdjacentOpponents = 4;
    private const int RangeAllAdjacent = 8;
    private const int RangeUser = 0x10;
    private const int RangeUserSide = 0x20;
    private const int RangeField = 0x40;
    private const int RangeOpponentSide = 0x80;
    private const int RangeAlly = 0x100;
    private const int RangeUserOrAlly = 0x200;
    private const int RangeMeFirst = 0x400;
    private const uint BattleTypeDoubles = 0x2;
    private const uint BattleTypePalPark = 0x200;
    private const uint VolatileRecharging = 0x400000;
    private const uint VolatileRampage = 0xC00;
    private const uint VolatileUproar = 0x70;
    private const uint VolatileMoveLocked = 0x1000;
    private const uint VolatileBide = 0x300;
    private const uint MoveEffectUnlockMask = 0xDFFBFF3F;
    private const uint SelfTurnSubstituteHit = 0x8;
    private const int MonDataOTId = 7;
    private const int MonDataBall = 0x9B;
    private const int OTNameCompareLength = 7;
    private const int HeapIDBattleLib = 5;

    // OFFSET 0x0225366C
    public static int BattleSystem_CheckDirectSideEffect(BattleSystem battleSys, BattleContext battleCtx, out int script)
    {
        int result = 0;
        script = 0;
        uint flags = battleCtx.SideEffectDirectFlags;
        if ((flags & SideEffectAlways) != 0)
        {
            script = BattleSystem_MapSideEffectToSubscript(battleCtx, SideEffectTypeDirect, flags);
            battleCtx.SideEffectDirectFlags = 0;
            if ((battleCtx.MoveStatusFlags & MoveStatusNoEffectMask) == 0)
            {
                result = 1;
            }
        }
        else if (flags != 0)
        {
            script = BattleSystem_MapSideEffectToSubscript(battleCtx, SideEffectTypeDirect, flags);
            if (battleCtx.BattleMons[battleCtx.SideEffectMon].CurHP != 0)
            {
                uint status = battleCtx.MoveStatusFlags;
                if ((status & MoveStatusNoEffectMask) == 0)
                {
                    result = 1;
                }
                else if ((battleCtx.SideEffectDirectFlags & SideEffectOnHitEvenMissed) != 0 && (status & MoveStatusMissedOrFailed) != 0)
                {
                    result = 1;
                }
                else if ((battleCtx.SideEffectDirectFlags & SideEffectCheckHP) != 0 && (status & MoveStatusMissed) != 0)
                {
                    result = 1;
                }
            }
            battleCtx.SideEffectDirectFlags = 0;
        }
        return result;
    }

    private static int BattleContext_MoveEffectChance(BattleContext battleCtx)
    {
        int chance;
        if (Battler_Ability(battleCtx, battleCtx.Attacker) == AbilitySereneGrace)
        {
            chance = (ushort)(battleCtx.AiContext.MoveTable[battleCtx.MoveCur].EffectChance * 2);
        }
        else
        {
            chance = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].EffectChance;
        }
        if (chance == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        return chance;
    }

    // OFFSET 0x02253710
    public static int BattleSystem_CheckIndirectSideEffect(BattleSystem battleSys, BattleContext battleCtx, out int script)
    {
        int result = 0;
        script = 0;
        uint flags = battleCtx.SideEffectIndirectFlags;
        if ((flags & SideEffectAlways) != 0)
        {
            script = BattleSystem_MapSideEffectToSubscript(battleCtx, SideEffectTypeIndirect, flags);
            battleCtx.SideEffectIndirectFlags = 0;
            if ((battleCtx.MoveStatusFlags & MoveStatusNoEffectMask) == 0)
            {
                result = 1;
            }
        }
        else if ((flags & SideEffectCheckSubstitute) != 0)
        {
            script = BattleSystem_MapSideEffectToSubscript(battleCtx, SideEffectTypeIndirect, flags);
            battleCtx.SideEffectIndirectFlags = 0;
            if (Battler_SubstituteWasHit(battleCtx, battleCtx.SideEffectMon) == 0 && (battleCtx.MoveStatusFlags & MoveStatusNoEffectMask) == 0)
            {
                result = 1;
            }
        }
        else if ((flags & SideEffectCheckHPAndSubstitute) != 0)
        {
            script = BattleSystem_MapSideEffectToSubscript(battleCtx, SideEffectTypeIndirect, flags);
            battleCtx.SideEffectIndirectFlags = 0;
            if (battleCtx.BattleMons[battleCtx.SideEffectMon].CurHP != 0
                && Battler_SubstituteWasHit(battleCtx, battleCtx.SideEffectMon) == 0
                && (battleCtx.MoveStatusFlags & MoveStatusNoEffectMask) == 0)
            {
                result = 1;
            }
        }
        else if ((flags & SideEffectCheckHP) != 0)
        {
            script = BattleSystem_MapSideEffectToSubscript(battleCtx, SideEffectTypeIndirect, flags);
            battleCtx.SideEffectIndirectFlags = 0;
            if (battleCtx.BattleMons[battleCtx.SideEffectMon].CurHP != 0)
            {
                result = 1;
            }
        }
        else if ((flags & SideEffectMoveEffect) != 0)
        {
            int chance = BattleContext_MoveEffectChance(battleCtx);
            if (BattleSystems.BattleSystem_RandNext(battleSys) % 100 < chance)
            {
                battleCtx.BattleStatusMask |= BattleStatusSecondaryEffect;
            }
            script = BattleSystem_MapSideEffectToSubscript(battleCtx, SideEffectTypeIndirect, battleCtx.SideEffectIndirectFlags);
            battleCtx.SideEffectIndirectFlags = 0;
            if (battleCtx.BattleMons[battleCtx.SideEffectMon].CurHP == 0)
            {
                battleCtx.BattleStatusMask &= ~BattleStatusSecondaryEffect;
            }
            result = 1;
        }
        else if (flags != 0)
        {
            int chance = BattleContext_MoveEffectChance(battleCtx);
            if (BattleSystems.BattleSystem_RandNext(battleSys) % 100 < chance)
            {
                script = BattleSystem_MapSideEffectToSubscript(battleCtx, SideEffectTypeIndirect, battleCtx.SideEffectIndirectFlags);
                battleCtx.SideEffectIndirectFlags = 0;
                if (battleCtx.BattleMons[battleCtx.SideEffectMon].CurHP != 0
                    && Battler_SubstituteWasHit(battleCtx, battleCtx.SideEffectMon) == 0
                    && (battleCtx.MoveStatusFlags & MoveStatusNoEffectMask) == 0)
                {
                    result = 1;
                }
            }
        }
        else if (battleCtx.SideEffectAbilityFlags != 0)
        {
            script = BattleSystem_MapSideEffectToSubscript(battleCtx, SideEffectTypeAbility, battleCtx.SideEffectAbilityFlags);
            battleCtx.SideEffectAbilityFlags = 0;
            if (battleCtx.BattleMons[battleCtx.SideEffectMon].CurHP != 0)
            {
                result = 1;
            }
        }
        return result;
    }

    private static bool Side_FollowMeTarget(BattleContext battleCtx, int side, out int target)
    {
        SideConditions conditions = battleCtx.SideConditions[side];
        target = conditions.FollowMeUser;
        return conditions.FollowMe != 0 && battleCtx.BattleMons[target].CurHP != 0;
    }

    // OFFSET 0x02253954
    public static int BattleSystem_Defender(BattleSystem battleSys, BattleContext battleCtx, int attacker, ushort move, int randomize, int inRange)
    {
        int defender = BattlerNone;
        int range = move != 0 ? battleCtx.AiContext.MoveTable[move].Range : inRange;
        if (range == RangeAdjacentOpponents)
        {
            int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
            int side = Battlers.Battler_GetType(BattleSystems.BattleSystem_GetBattlerData(battleSys, attacker)!) & 1;
            for (battleCtx.BattlerCounter = 0; battleCtx.BattlerCounter < maxBattlers; battleCtx.BattlerCounter++)
            {
                int battler = battleCtx.MonSpeedOrder[battleCtx.BattlerCounter];
                if (battleCtx.BattleMons[battler].CurHP != 0)
                {
                    BattlerData data = BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!;
                    if ((side != 0 && (Battlers.Battler_GetType(data) & 1) == 0) || (side == 0 && (Battlers.Battler_GetType(data) & 1) != 0))
                    {
                        defender = battler;
                        break;
                    }
                }
            }
            if (battleCtx.BattlerCounter != maxBattlers)
            {
                battleCtx.BattlerCounter++;
            }
        }
        else if (range == RangeAllAdjacent)
        {
            int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
            for (battleCtx.BattlerCounter = 0; battleCtx.BattlerCounter < maxBattlers; battleCtx.BattlerCounter++)
            {
                int battler = battleCtx.MonSpeedOrder[battleCtx.BattlerCounter];
                if (battleCtx.BattleMons[battler].CurHP != 0 && battler != attacker)
                {
                    defender = battler;
                    break;
                }
            }
            if (battleCtx.BattlerCounter != maxBattlers)
            {
                battleCtx.BattlerCounter++;
            }
        }
        else if (range == RangeUserOrAlly && randomize == 1)
        {
            if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) != 0 && BattleSystems.BattleSystem_RandNext(battleSys) % 2 == 0)
            {
                defender = BattleSystems.BattleSystem_GetPartner(battleSys, attacker);
                if (battleCtx.BattleMons[defender].CurHP == 0)
                {
                    defender = attacker;
                }
            }
            else
            {
                defender = attacker;
            }
        }
        else if (range == RangeMeFirst && randomize == 1)
        {
            defender = BattleSystem_RandomOpponent(battleSys, battleCtx, attacker);
        }
        else if (range == RangeOpponentSide)
        {
            defender = BattleSystem_RandomOpponent(battleSys, battleCtx, attacker);
        }
        else if (range == RangeUser || range == RangeUserSide || range == 1 || range == RangeField)
        {
            defender = attacker;
        }
        else if (range == RangeAlly)
        {
            defender = (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) != 0 ? BattleSystems.BattleSystem_GetPartner(battleSys, attacker) : attacker;
        }
        else if (range == RangeUserOrAlly)
        {
            if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) != 0)
            {
                defender = (int)battleCtx.BattlerActions[attacker, 1];
                if (battleCtx.BattleMons[defender].CurHP == 0)
                {
                    defender = attacker;
                }
            }
            else
            {
                defender = attacker;
            }
        }
        else if (range == RangeRandomOpponent || randomize == 1)
        {
            uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
            int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, attacker) ^ 1;
            int[] enemies =
            {
                BattleSystems.BattleSystem_GetEnemyInSlot(battleSys, attacker, 0),
                BattleSystems.BattleSystem_GetEnemyInSlot(battleSys, attacker, 2),
            };
            if ((battleType & BattleTypeDoubles) != 0)
            {
                if (Side_FollowMeTarget(battleCtx, side, out int followMe))
                {
                    defender = followMe;
                }
                else if (battleCtx.BattleMons[enemies[0]].CurHP != 0 && battleCtx.BattleMons[enemies[1]].CurHP != 0)
                {
                    defender = enemies[BattleSystems.BattleSystem_RandNext(battleSys) & 1];
                }
                else if (battleCtx.BattleMons[enemies[0]].CurHP != 0)
                {
                    defender = enemies[0];
                }
                else if (battleCtx.BattleMons[enemies[1]].CurHP != 0)
                {
                    defender = enemies[1];
                }
            }
            else if (battleCtx.BattleMons[attacker ^ 1].CurHP != 0)
            {
                defender = attacker ^ 1;
            }
        }
        else
        {
            int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, attacker) ^ 1;
            int target = (int)battleCtx.BattlerActions[attacker, 1];
            BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
            if (Side_FollowMeTarget(battleCtx, side, out int followMe))
            {
                defender = followMe;
            }
            else if (battleCtx.BattleMons[target].CurHP != 0)
            {
                defender = target;
            }
            else
            {
                target = BattleSystem_RandomOpponent(battleSys, battleCtx, attacker);
                if (battleCtx.BattleMons[target].CurHP != 0)
                {
                    defender = target;
                }
            }
        }
        return defender;
    }

    private static bool BattleSystem_RedirectToAbility(BattleSystem battleSys, BattleContext battleCtx, int attacker, int ability, out int target)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        target = 0;
        for (int i = 0; i < maxBattlers; i++)
        {
            target = battleCtx.MonSpeedOrder[i];
            if (Battler_Ability(battleCtx, target) == ability && battleCtx.BattleMons[target].CurHP != 0 && attacker != target)
            {
                break;
            }
        }
        return target != battleCtx.Defender;
    }

    // OFFSET 0x02253C98
    public static void BattleSystem_RedirectTarget(BattleSystem battleSys, BattleContext battleCtx, int attacker, ushort move)
    {
        if (battleCtx.Defender == BattlerNone)
        {
            return;
        }
        if (Battler_Ability(battleCtx, attacker) == AbilityNormalize || Battler_Ability(battleCtx, attacker) == AbilityMoldBreaker)
        {
            return;
        }
        if (Side_FollowMeTarget(battleCtx, BattleSystems.BattleSystem_GetBattlerSide(battleSys, attacker) ^ 1, out _))
        {
            return;
        }
        int type = BattleSystem_MoveTypeOverride(battleSys, battleCtx, attacker, move);
        if (type == 0)
        {
            type = battleCtx.AiContext.MoveTable[move].Type;
        }
        int range = battleCtx.AiContext.MoveTable[move].Range;
        bool singleTarget = range == RangeSingleTarget || range == RangeRandomOpponent;
        if (type == TypeElectric && singleTarget && (battleCtx.BattleStatusMask & BattleStatusNoRedirect) == 0
            && BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityNotAttacker, attacker, AbilityLightningRod) != 0)
        {
            if (BattleSystem_RedirectToAbility(battleSys, battleCtx, attacker, AbilityLightningRod, out int target))
            {
                battleCtx.SelfTurnFlags[target].LightningRodActivated = 1;
                battleCtx.Defender = target;
            }
        }
        else if (type == TypeWater && singleTarget && (battleCtx.BattleStatusMask & BattleStatusNoRedirect) == 0
            && BattleSystem_CountAbility(battleSys, battleCtx, CountAbilityNotAttacker, attacker, AbilityStormDrain) != 0)
        {
            if (BattleSystem_RedirectToAbility(battleSys, battleCtx, attacker, AbilityStormDrain, out int target))
            {
                battleCtx.SelfTurnFlags[target].StormDrainActivated = 1;
                battleCtx.Defender = target;
            }
        }
    }

    // OFFSET 0x02253E3C
    public static int BattleSystem_TriggerRedirectionAbilities(BattleSystem battleSys, BattleContext battleCtx)
    {
        int result = 0;
        if ((battleCtx.MoveStatusFlags & MoveStatusNoEffectMask) == 0 && battleCtx.SelfTurnFlags[battleCtx.Defender].LightningRodActivated != 0)
        {
            battleCtx.SelfTurnFlags[battleCtx.Defender].LightningRodActivated = 0;
            BattleSystem_LoadScript(battleCtx, 1, SubscriptRedirectAbility);
            battleCtx.CommandNext = battleCtx.Command;
            battleCtx.Command = CommandRunScriptState;
            result = 1;
        }
        if ((battleCtx.MoveStatusFlags & MoveStatusNoEffectMask) == 0 && battleCtx.SelfTurnFlags[battleCtx.Defender].StormDrainActivated != 0)
        {
            battleCtx.SelfTurnFlags[battleCtx.Defender].StormDrainActivated = 0;
            BattleSystem_LoadScript(battleCtx, 1, SubscriptRedirectAbility);
            battleCtx.CommandNext = battleCtx.Command;
            battleCtx.Command = CommandRunScriptState;
            result = 1;
        }
        return result;
    }

    // OFFSET 0x02253EC0
    public static void BattleMon_CopyToParty(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        if (battleCtx.BattleMons[battler].HeldItem == 0)
        {
            AI_ClearKnownItem(battleCtx, (byte)battler);
        }
        BattleIO.BattleIO_UpdatePartyMon(battleSys, battleCtx, battler);
    }

    // OFFSET 0x02253EF0
    public static void Battler_LockMoveChoice(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        battleCtx.BattleMons[battler].StatusVolatile |= VolatileMoveLocked;
        battleCtx.MoveLockedInto[battler] = battleCtx.MoveCur;
    }

    // OFFSET 0x02253F20
    public static void Battler_UnlockMoveChoice(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        mon.StatusVolatile &= ~VolatileMoveLocked;
        mon.StatusVolatile &= ~VolatileBide;
        mon.MoveEffectsMask &= MoveEffectUnlockMask;
        mon.MoveEffectsData.RolloutCount = 0;
        mon.MoveEffectsData.FuryCutterCount = 0;
    }

    // OFFSET 0x02257028
    public static int BattleSystem_RandomOpponent(BattleSystem battleSys, BattleContext battleCtx, int attacker)
    {
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoubles) == 0)
        {
            return attacker ^ 1;
        }
        int[] enemies =
        {
            BattleSystems.BattleSystem_GetEnemyInSlot(battleSys, attacker, 0),
            BattleSystems.BattleSystem_GetEnemyInSlot(battleSys, attacker, 2),
        };
        int pick = BattleSystems.BattleSystem_RandNext(battleSys) & 1;
        int defender = enemies[pick];
        if (battleCtx.BattleMons[defender].CurHP == 0)
        {
            defender = enemies[pick ^ 1];
        }
        return defender;
    }

    // OFFSET 0x02259A28
    public static int Battler_CanPickCommand(BattleContext battleCtx, int battler)
    {
        uint status = battleCtx.BattleMons[battler].StatusVolatile;
        if ((status & VolatileRecharging) != 0 || (status & VolatileRampage) != 0 || (status & VolatileUproar) != 0 || (status & VolatileMoveLocked) != 0)
        {
            return 0;
        }
        return 1;
    }

    // OFFSET 0x02259A5C
    public static void BattleSystem_SetPokemonCatchData(BattleSystem battleSys, BattleContext battleCtx, Pokemon mon)
    {
        TrainerInfo trainerInfo = BattleSystems.BattleSystem_GetTrainerInfo(battleSys, 0);
        int mapLabel = BattleSystems.BattleSystem_GetMapLabelTextID(battleSys);
        int terrain = BattleSystems.BattleSystem_GetTerrain(battleSys);
        int ball;
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypePalPark) != 0)
        {
            ball = (int)Pokemon.Pokemon_GetValue(mon, MonDataBall, null);
        }
        else
        {
            ball = battleCtx.MsgItemTemp;
        }
        Pokemon.Pokemon_SetCatchData(mon, trainerInfo, ball, mapLabel, terrain, HeapIDBattleLib);
    }

    // OFFSET 0x02259AB4
    public static byte BattleContext_IOBufferVal(BattleContext battleCtx, int battler)
    {
        return battleCtx.IoBuffer[battler, 0];
    }

    // OFFSET 0x02259AC0
    public static int Battler_SubstituteWasHit(BattleContext battleCtx, int battler)
    {
        return (battleCtx.SelfTurnFlags[battler].StatusFlags & SelfTurnSubstituteHit) != 0 ? 1 : 0;
    }

    // OFFSET 0x02259ADC
    public static int BattleSystem_TrainerIsOT(BattleSystem battleSys, BattleContext battleCtx)
    {
        TrainerInfo trainerInfo = BattleSystems.BattleSystem_GetTrainerInfo(battleSys, 0);
        uint trainerID = TrainerInfo.TrainerInfo_ID(trainerInfo);
        uint gender = TrainerInfo.TrainerInfo_Gender(trainerInfo);
        ushort[] name = TrainerInfo.TrainerInfo_Name(trainerInfo);
        BattleMon mon = battleCtx.BattleMons[battleCtx.Attacker];
        if (trainerID == mon.OTId && gender == (uint)mon.OTGender && CharCode.CharCode_CompareNumChars(name, 0, mon.OTName, 0, OTNameCompareLength) == 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02259B38
    public static int BattleSystem_PokemonIsOT(BattleSystem battleSys, Pokemon mon)
    {
        TrainerInfo trainerInfo = BattleSystems.BattleSystem_GetTrainerInfo(battleSys, 0);
        uint trainerID = TrainerInfo.TrainerInfo_ID(trainerInfo);
        uint gender = TrainerInfo.TrainerInfo_Gender(trainerInfo);
        ushort[] name = TrainerInfo.TrainerInfo_Name(trainerInfo);
        ushort[] otName = new ushort[8];
        Pokemon.Pokemon_GetValue(mon, MonDataOTName, otName);
        if (trainerID == Pokemon.Pokemon_GetValue(mon, MonDataOTId, null)
            && gender == Pokemon.Pokemon_GetValue(mon, MonDataOTGender, null)
            && CharCode.CharCode_CompareNumChars(name, 0, otName, 0, OTNameCompareLength) == 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0225B5D0
    public static int BattleSystem_MapSideEffectToSubscript(BattleContext battleCtx, int type, uint flags)
    {
        battleCtx.SideEffectType = type;
        battleCtx.SideEffectParam = (int)(flags & SideEffectParamMask);
        battleCtx.SideEffectFlags = (int)(flags & SideEffectFlagsMask);
        if ((flags & SideEffectTargetAttacker) != 0)
        {
            battleCtx.SideEffectMon = battleCtx.Attacker;
        }
        else if ((flags & SideEffectTargetDefender) != 0)
        {
            battleCtx.SideEffectMon = battleCtx.Defender;
        }
        else if ((flags & SideEffectAlways) != 0 || (flags & SideEffectCheckHP) != 0)
        {
            battleCtx.SideEffectMon = 0;
        }
        if (battleCtx.SideEffectParam >= SideEffectSubscriptCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return (int)Nitro.Bin.U32(RomData.Overlay(16, (uint)(SideEffectSubscriptTable + battleCtx.SideEffectParam * 4), 4), 0);
    }

    // OFFSET 0x0225B63C
    public static int BattleSystem_ApplyTypeMultiplier(BattleContext battleCtx, int attacker, int multiplier, int damage, int update, ref uint moveStatusMask)
    {
        if ((battleCtx.BattleStatusMask & BattleStatusIgnoreTypeEffectiveness) == 0 && (battleCtx.BattleStatusMask & BattleStatusIgnoreTypeImmunity) == 0 && damage != 0)
        {
            damage = BattleSystem_Divide(damage * multiplier, 10);
        }
        switch (multiplier)
        {
            case 0:
                moveStatusMask |= 8;
                moveStatusMask &= ~4u;
                moveStatusMask &= ~2u;
                break;
            case 5:
                if (update != 0)
                {
                    if ((moveStatusMask & 2) != 0)
                    {
                        moveStatusMask &= ~2u;
                    }
                    else
                    {
                        moveStatusMask |= 4;
                    }
                }
                break;
            case 20:
                if (update != 0)
                {
                    if ((moveStatusMask & 4) != 0)
                    {
                        moveStatusMask &= ~4u;
                    }
                    else
                    {
                        moveStatusMask |= 2;
                    }
                }
                break;
        }
        return damage;
    }

    // OFFSET 0x0225B6C8
    public static int BattleContext_MoveExecutesThisTurn(BattleContext battleCtx, int move)
    {
        switch (battleCtx.AiContext.MoveTable[move].Effect)
        {
            case 0x1A:
            case 0x27:
            case 0x4B:
            case 0x91:
            case 0x97:
            case 0x9B:
            case 0xFF:
            case 0x100:
            case 0x107:
            case 0x111:
                return (int)(battleCtx.BattleStatusMask & BattleStatusChargeTurn);
        }
        return 1;
    }
}
