namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const uint MoveStatusNoEffect = 0x1FD849;
    private const int BeatUpFirstHits = 2;
    private const uint BeatUpAccuracyCheck = 0xFD;
    private const int MonDataSpeciesEggBeatUp = 0xAE;
    private const int MonDataStatusCondition = 0xA0;
    private const int MonDataForm = 0x70;
    private const int MonDataLevel = 0xA1;
    private const int SpeciesEgg = 0x1EE;
    private const int SpeciesDataBaseAttack = 1;
    private const int SpeciesDataBaseDefense = 2;
    private const int MsgBeatUpAttack = 0x1E1;

    // OFFSET 0x0224B9F4
    private static void BattleScript_CheckMoveHitAndProtection(BattleSystem battleSys, BattleContext battleCtx, int attacker, int defender, int move)
    {
        BattleControllers.BattleController_CheckMoveHit(battleSys, battleCtx, attacker, defender, move);
        BattleControllers.BattleController_CheckProtectAndSemiInvulnerable(battleSys, battleCtx, attacker, defender, move);
    }

    // OFFSET 0x02245CB4
    private static int BattleScript_CheckMoveHit(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int attackerSelector = BattleScript_Read(battleCtx);
        int defenderSelector = BattleScript_Read(battleCtx);
        int moveSelector = BattleScript_Read(battleCtx);
        int jumpOnMiss = BattleScript_Read(battleCtx);
        int attacker = BattleScript_Battler(battleSys, battleCtx, attackerSelector);
        int defender = BattleScript_Battler(battleSys, battleCtx, defenderSelector);
        int move = BattleMessage_MoveTag(battleCtx, moveSelector);
        BattleScript_CheckMoveHitAndProtection(battleSys, battleCtx, attacker, defender, move);
        if ((battleCtx.MoveStatusFlags & MoveStatusNoEffect) != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnMiss);
        }
        return 0;
    }

    // OFFSET 0x02245D34
    private static int BattleScript_TryTeleport(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        if (BattleContexts.Battler_IsTrapped(battleSys, battleCtx, battleCtx.Attacker, null) != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    private static bool BattleScript_BeatUpSkips(Pokemon mon)
    {
        if (Pokemon.Pokemon_GetValue(mon, MonDataCurHP, null) == 0)
        {
            return true;
        }
        if (Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEggBeatUp, null) == 0)
        {
            return true;
        }
        if (Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEggBeatUp, null) == SpeciesEgg)
        {
            return true;
        }
        return Pokemon.Pokemon_GetValue(mon, MonDataStatusCondition, null) != 0;
    }

    // OFFSET 0x02245D68
    private static int BattleScript_BeatUp(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int partyCount = BattleSystems.BattleSystem_GetPartyCount(battleSys, battleCtx.Attacker);
        Pokemon mon;
        if (battleCtx.MultiHitNumHits == 0)
        {
            battleCtx.MultiHitNumHits = BeatUpFirstHits;
            battleCtx.MultiHitAccuracyCheck = BeatUpAccuracyCheck;
            battleCtx.BeatUpCounter = 0;
            while (true)
            {
                mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battleCtx.Attacker, battleCtx.BeatUpCounter);
                if (battleCtx.BeatUpCounter == battleCtx.SelectedPartySlot[battleCtx.Attacker] || !BattleScript_BeatUpSkips(mon))
                {
                    break;
                }
                battleCtx.BeatUpCounter++;
            }
        }
        mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battleCtx.Attacker, battleCtx.BeatUpCounter);
        int species = (int)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
        int form = (int)Pokemon.Pokemon_GetValue(mon, MonDataForm, null);
        int level = (int)Pokemon.Pokemon_GetValue(mon, MonDataLevel, null);
        battleCtx.Damage = (int)SpeciesData.SpeciesData_GetFormValue(species, form, SpeciesDataBaseAttack);
        battleCtx.Damage = battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Power * battleCtx.Damage;
        battleCtx.Damage = (level * 2 / 5 + 2) * battleCtx.Damage;
        BattleMon defender = battleCtx.BattleMons[battleCtx.Defender];
        battleCtx.Damage = (int)((uint)battleCtx.Damage / SpeciesData.SpeciesData_GetFormValue(defender.Species, defender.FormNum & 0x1F, SpeciesDataBaseDefense));
        battleCtx.Damage = battleCtx.Damage / 50;
        battleCtx.Damage = battleCtx.Damage + 2;
        battleCtx.Damage = battleCtx.CriticalMul * battleCtx.Damage;
        if (battleCtx.TurnFlags[battleCtx.Attacker].HelpingHand != 0)
        {
            battleCtx.Damage = battleCtx.Damage * 15 / 10;
        }
        battleCtx.Damage = BattleContexts.BattleSystem_CalcDamageVariance(battleSys, battleCtx, battleCtx.Damage);
        battleCtx.Damage = battleCtx.Damage * -1;
        battleCtx.MsgBuffer.Id = MsgBeatUpAttack;
        battleCtx.MsgBuffer.Tags = TagNicknameOnly;
        battleCtx.MsgBuffer.Params[0] = battleCtx.Attacker | (battleCtx.BeatUpCounter << 8);
        battleCtx.BeatUpCounter++;
        battleCtx.MultiHitCounter = 2;
        if (battleCtx.BeatUpCounter < partyCount)
        {
            while (true)
            {
                mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battleCtx.Attacker, battleCtx.BeatUpCounter);
                if (battleCtx.BeatUpCounter == battleCtx.SelectedPartySlot[battleCtx.Attacker] || !BattleScript_BeatUpSkips(mon))
                {
                    break;
                }
                battleCtx.BeatUpCounter++;
                if (battleCtx.BeatUpCounter >= partyCount)
                {
                    battleCtx.MultiHitCounter = 1;
                    break;
                }
            }
        }
        else
        {
            battleCtx.MultiHitCounter = 1;
        }
        return 0;
    }

    // OFFSET 0x02245FC4
    private static int BattleScript_FollowMe(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
        battleCtx.SideConditions[side].FollowMe = 1;
        battleCtx.SideConditions[side].FollowMeUser = battleCtx.Attacker & 3;
        return 0;
    }
}
