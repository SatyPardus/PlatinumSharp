namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const int MonDataPersonality = 0;
    private const int MonDataSpecies = 5;
    private const int MonDataHeldItem = 6;
    private const int MonDataOTID = 7;
    private const int MonDataExp = 8;
    private const int MonDataFriendship = 9;
    private const int MonDataAbility = 0xA;
    private const int MonDataMove1 = 0x36;
    private const int MonDataMove1CurPP = 0x3A;
    private const int MonDataMove1PPUps = 0x3E;
    private const int MonDataHpIV = 0x46;
    private const int MonDataAtkIV = 0x47;
    private const int MonDataDefIV = 0x48;
    private const int MonDataSpeedIV = 0x49;
    private const int MonDataSpAtkIV = 0x4A;
    private const int MonDataSpDefIV = 0x4B;
    private const int MonDataIsEgg = 0x4C;
    private const int MonDataHasNickname = 0x4D;
    private const int MonDataForm = 0x70;
    private const int MonDataNickname = 0x75;
    private const int MonDataOTName = 0x90;
    private const int MonDataPokeball = 0x9B;
    private const int MonDataOTGender = 0x9D;
    private const int MonDataStatus = 0xA0;
    private const int MonDataLevel = 0xA1;
    private const int MonDataCurHP = 0xA3;
    private const int MonDataMaxHP = 0xA4;
    private const int MonDataAttack = 0xA5;
    private const int MonDataDefense = 0xA6;
    private const int MonDataSpeed = 0xA7;
    private const int MonDataSpAttack = 0xA8;
    private const int MonDataSpDefense = 0xA9;
    private const int MonDataType1 = 0xB1;
    private const int MonDataType2 = 0xB2;
    private const uint BattleStatusBatonPass = 0x100;
    private const uint BattleTypeNoAbilities = 0x220;
    private const int BattleStatStages = 8;
    private const sbyte DefaultStatStage = 6;
    private const int LearnedMoves = 4;

    // OFFSET 0x0225177C
    public static void BattleSystem_InitBattleMon(BattleSystem battleSys, BattleContext battleCtx, int battler, int partySlot)
    {
        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, partySlot);
        BattleMon battleMon = battleCtx.BattleMons[battler];
        battleMon.Species = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
        battleMon.Attack = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataAttack, null);
        battleMon.Defense = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataDefense, null);
        battleMon.Speed = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpeed, null);
        battleMon.SpAttack = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpAttack, null);
        battleMon.SpDefense = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpDefense, null);
        for (int i = 0; i < LearnedMoves; i++)
        {
            battleMon.Moves[i] = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + i, null);
            battleMon.PpCur[i] = (byte)Pokemon.Pokemon_GetValue(mon, MonDataMove1CurPP + i, null);
            battleMon.PpUps[i] = (byte)Pokemon.Pokemon_GetValue(mon, MonDataMove1PPUps + i, null);
        }
        battleMon.HpIV = (int)(Pokemon.Pokemon_GetValue(mon, MonDataHpIV, null) & 0x1F);
        battleMon.AttackIV = (int)(Pokemon.Pokemon_GetValue(mon, MonDataAtkIV, null) & 0x1F);
        battleMon.DefenseIV = (int)(Pokemon.Pokemon_GetValue(mon, MonDataDefIV, null) & 0x1F);
        battleMon.SpeedIV = (int)(Pokemon.Pokemon_GetValue(mon, MonDataSpeedIV, null) & 0x1F);
        battleMon.SpAttackIV = (int)(Pokemon.Pokemon_GetValue(mon, MonDataSpAtkIV, null) & 0x1F);
        battleMon.SpDefenseIV = (int)(Pokemon.Pokemon_GetValue(mon, MonDataSpDefIV, null) & 0x1F);
        battleMon.IsEgg = (int)(Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) & 1);
        battleMon.HasNickname = (int)(Pokemon.Pokemon_GetValue(mon, MonDataHasNickname, null) & 1);
        if ((battleCtx.BattleStatusMask & BattleStatusBatonPass) == 0)
        {
            for (int i = 0; i < BattleStatStages; i++)
            {
                battleMon.StatBoosts[i] = DefaultStatStage;
            }
        }
        battleMon.WeatherAbilityAnnounced = 0;
        battleMon.IntimidateAnnounced = 0;
        battleMon.TraceAnnounced = 0;
        battleMon.DownloadAnnounced = 0;
        battleMon.AnticipationAnnounced = 0;
        battleMon.ForewarnAnnounced = 0;
        battleMon.SlowStartAnnounced = 0;
        battleMon.SlowStartFinished = 0;
        battleMon.FriskAnnounced = 0;
        battleMon.MoldBreakerAnnounced = 0;
        battleMon.PressureAnnounced = 0;
        battleMon.Type1 = (byte)Pokemon.Pokemon_GetValue(mon, MonDataType1, null);
        battleMon.Type2 = (byte)Pokemon.Pokemon_GetValue(mon, MonDataType2, null);
        battleMon.Gender = Pokemon.Pokemon_GetGender(mon) & 0xF;
        battleMon.IsShiny = Pokemon.Pokemon_IsShiny(mon) & 1;
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeNoAbilities) != 0)
        {
            battleMon.Ability = 0;
            battleMon.Status = 0;
            battleMon.HeldItem = 0;
        }
        else
        {
            battleMon.Ability = (byte)Pokemon.Pokemon_GetValue(mon, MonDataAbility, null);
            battleMon.Status = Pokemon.Pokemon_GetValue(mon, MonDataStatus, null);
            battleMon.HeldItem = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataHeldItem, null);
        }
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeNoAbilities) != 0 && BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) == 0)
        {
            battleMon.FormNum = 0;
        }
        else
        {
            battleMon.FormNum = (int)(Pokemon.Pokemon_GetValue(mon, MonDataForm, null) & 0x1F);
        }
        battleMon.Level = (byte)Pokemon.Pokemon_GetValue(mon, MonDataLevel, null);
        battleMon.Friendship = (byte)Pokemon.Pokemon_GetValue(mon, MonDataFriendship, null);
        battleMon.CurHP = (int)Pokemon.Pokemon_GetValue(mon, MonDataCurHP, null);
        battleMon.MaxHP = Pokemon.Pokemon_GetValue(mon, MonDataMaxHP, null);
        battleMon.Exp = Pokemon.Pokemon_GetValue(mon, MonDataExp, null);
        battleMon.Personality = Pokemon.Pokemon_GetValue(mon, MonDataPersonality, null);
        battleMon.OTId = Pokemon.Pokemon_GetValue(mon, MonDataOTID, null);
        battleMon.OTGender = (int)(Pokemon.Pokemon_GetValue(mon, MonDataOTGender, null) & 0xF);
        battleMon.CapturedBall = (byte)Pokemon.Pokemon_GetValue(mon, MonDataPokeball, null);
        HeightWeight.HeightWeight_SetFormNarcs(battleMon.FormNum);
        HeightWeightData heightWeight = HeightWeight.HeightWeightData_New(HeapIDBattle);
        HeightWeight.HeightWeightData_Load(heightWeight, 0, HeapIDBattle);
        battleMon.Weight = HeightWeight.HeightWeightData_Weight(heightWeight, battleMon.Species);
        HeightWeight.HeightWeightData_Release(heightWeight);
        HeightWeight.HeightWeightData_Free(heightWeight);
        Pokemon.Pokemon_GetValue(mon, MonDataNickname, battleMon.Nickname);
        Pokemon.Pokemon_GetValue(mon, MonDataOTName, battleMon.OTName);
        battleMon.TimesDamaged = 0;
        battleMon.TrainerMessageFlags = 0;
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler);
        if ((HealthBar.FlagIndex(battleCtx.SelectedPartySlot[battler]) & (uint)battleCtx.SideConditions[side].KnockedOffItemsMask) != 0)
        {
            battleMon.HeldItem = 0;
            battleMon.MoveEffectsData.CanUnburden = 0;
            return;
        }
        if (battleMon.HeldItem != 0)
        {
            battleMon.MoveEffectsData.CanUnburden = 1;
        }
    }
}
