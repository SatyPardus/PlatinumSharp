namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const uint VolatileBindTurns = 0xE000;
    private const uint BattleTypeTrainer = 1;
    private const uint BattleTypeMulti = 8;
    private const int MonDataIsEgg = 0x4C;
    private const int TransformPPMax = 5;

    // OFFSET 0x02244B48
    private static int BattleScript_TrySubstitute(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int substituteHP = BattleContexts.BattleSystem_Divide((int)battleCtx.BattleMons[battleCtx.Attacker].MaxHP, 4);
        if (battleCtx.BattleMons[battleCtx.Attacker].CurHP <= substituteHP)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else
        {
            battleCtx.HpCalcTemp = -substituteHP;
            battleCtx.BattleMons[battleCtx.Attacker].MoveEffectsData.SubstituteHP = substituteHP;
            battleCtx.BattleMons[battleCtx.Attacker].StatusVolatile &= ~VolatileBindTurns;
        }
        return 0;
    }

    private static bool BattleScript_IsValidSwitchTarget(Pokemon mon)
    {
        return Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null) != 0 && Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) == 0 && Pokemon.Pokemon_GetValue(mon, MonDataCurHP, null) != 0;
    }

    // OFFSET 0x02244BC4
    private static int BattleScript_TryWhirlwind(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        if ((battleType & BattleTypeTrainer) == 0)
        {
            if (BattleContexts.BattleSystem_ForceSwitchSucceeds(battleSys, battleCtx) == 0)
            {
                BattleScript_Iter(battleCtx, jumpOnFail);
            }
            return 0;
        }
        int count = 0;
        Party party = BattleSystems.BattleSystem_GetParty(battleSys, battleCtx.Defender);
        int partyCount = BattleSystems.BattleSystem_GetPartyCount(battleSys, battleCtx.Defender);
        int start;
        int minMons;
        int slot1;
        int slot2;
        if ((battleType & BattleTypeMulti) != 0 || ((battleType & BattleTypeTag) != 0 && BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Defender) != 0))
        {
            start = 0;
            minMons = 1;
            slot1 = battleCtx.SelectedPartySlot[battleCtx.Defender];
            slot2 = slot1;
        }
        else if ((battleType & BattleTypeDoubles) != 0)
        {
            minMons = 2;
            start = 0;
            slot1 = battleCtx.SelectedPartySlot[battleCtx.Defender];
            slot2 = battleCtx.SelectedPartySlot[BattleSystems.BattleSystem_GetPartner(battleSys, battleCtx.Defender)];
        }
        else
        {
            start = 0;
            minMons = 1;
            slot1 = battleCtx.SelectedPartySlot[battleCtx.Defender];
            slot2 = slot1;
        }
        for (int i = start; i < partyCount; i++)
        {
            if (BattleScript_IsValidSwitchTarget(Party.Party_GetPokemonBySlotIndex(party, i)))
            {
                count++;
            }
        }
        if (count <= minMons)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else if (BattleContexts.BattleSystem_ForceSwitchSucceeds(battleSys, battleCtx) != 0)
        {
            int slot;
            while (true)
            {
                slot = BattleSystems.BattleSystem_RandNext(battleSys) % partyCount + start;
                if (slot == slot1 || slot == slot2)
                {
                    continue;
                }
                Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, slot);
                if (Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null) == 0 || Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) == 1 || Pokemon.Pokemon_GetValue(mon, MonDataCurHP, null) == 0)
                {
                    continue;
                }
                break;
            }
            battleCtx.SwitchedPartySlot[battleCtx.Defender] = (byte)slot;
        }
        else
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x02244D60
    private static int BattleScript_Transform(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleMon attacker = battleCtx.BattleMons[battleCtx.Attacker];
        BattleMon defender = battleCtx.BattleMons[battleCtx.Defender];
        attacker.StatusVolatile |= VolatileTransformed;
        attacker.MoveEffectsData.DisabledMove = 0;
        attacker.MoveEffectsData.DisabledTurns = 0;
        attacker.MoveEffectsData.TransformedPID = defender.Personality;
        attacker.MoveEffectsData.TransformedGender = (ushort)defender.Gender;
        attacker.MoveEffectsData.MimickedMoveSlot = 0;
        attacker.MoveEffectsData.LastResortCount = 0;
        attacker.Species = defender.Species;
        attacker.Attack = defender.Attack;
        attacker.Defense = defender.Defense;
        attacker.Speed = defender.Speed;
        attacker.SpAttack = defender.SpAttack;
        attacker.SpDefense = defender.SpDefense;
        for (int i = 0; i < 4; i++)
        {
            attacker.Moves[i] = defender.Moves[i];
        }
        attacker.HpIV = defender.HpIV;
        attacker.AttackIV = defender.AttackIV;
        attacker.DefenseIV = defender.DefenseIV;
        attacker.SpeedIV = defender.SpeedIV;
        attacker.SpAttackIV = defender.SpAttackIV;
        attacker.SpDefenseIV = defender.SpDefenseIV;
        attacker.IsEgg = defender.IsEgg;
        attacker.HasNickname = defender.HasNickname;
        for (int i = 0; i < 8; i++)
        {
            attacker.StatBoosts[i] = defender.StatBoosts[i];
        }
        attacker.Weight = defender.Weight;
        attacker.Type1 = defender.Type1;
        attacker.Type2 = defender.Type2;
        attacker.FormNum = defender.FormNum;
        attacker.IsShiny = defender.IsShiny;
        attacker.Padding002606 = defender.Padding002606;
        attacker.Ability = defender.Ability;
        attacker.WeatherAbilityAnnounced = 0;
        attacker.IntimidateAnnounced = 0;
        attacker.TraceAnnounced = 0;
        attacker.DownloadAnnounced = 0;
        attacker.AnticipationAnnounced = 0;
        attacker.ForewarnAnnounced = 0;
        attacker.FriskAnnounced = 0;
        attacker.MoldBreakerAnnounced = 0;
        attacker.PressureAnnounced = 0;
        attacker.MoveEffectsData.Truant = battleCtx.TotalTurns & 1;
        attacker.MoveEffectsData.SlowStartTurnNumber = battleCtx.TotalTurns + 1;
        attacker.SlowStartAnnounced = 0;
        attacker.SlowStartFinished = 0;
        for (int i = 0; i < 4; i++)
        {
            byte pp = battleCtx.AiContext.MoveTable[attacker.Moves[i]].Pp;
            attacker.PpCur[i] = pp < TransformPPMax ? pp : (byte)TransformPPMax;
        }
        return 0;
    }
}
