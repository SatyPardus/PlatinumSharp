namespace PlatinumSharp.Game;

public static class SystemVars
{
    private const int VarPlayerStarter = 0x4030;
    private const int VarVsSeekerBattery = 0x4033;
    private const int VarVsSeekerStepCount = 0x4034;
    private const int VarSizeContestRecord = 0x4035;
    private const int VarNewsPressDeadline = 0x403B;
    private const int VarSpiritombCounter = 0x403E;
    private const int VarPartnerTrainerID = 0x403F;
    private const int VarConsecutiveBonusRoundWins = 0x4040;
    private const int VarDailyRandomLevel = 0x4041;
    private const int VarDepartmentStoreBuyCount = 0x4042;
    private const int VarUndergroundFossilsUnearthed = 0x4047;
    private const int VarUndergroundTrapsSet = 0x4048;
    private const int VarUndergroundTalkCounter = 0x4049;
    private const int VarFriendshipStepCount = 0x404A;
    private const int VarEternaGymFlowerClockState = 0x404B;
    private const int VarBattleTowerPrintState = 0x404F;
    private const int VarBattleFactoryPrintState = 0x4050;
    private const int VarBattleHallPrintState = 0x4051;
    private const int VarBattleCastlePrintState = 0x4052;
    private const int VarBattleArcadePrintState = 0x4053;
    private const int VarUndergroundItemsGivenAway = 0x4054;
    private const int VarDistortionWorldProgress = 0x4055;
    private const int VarArceusEventState = 0x4056;
    private const int VarShayminEventState = 0x4057;
    private const int VarRoamingCresseliaState = 0x4058;
    private const int VarRoamingMespritState = 0x4059;
    private const int VarDistortionWorldCyrusApperanceState = 0x405B;
    private const int VarRoamingMoltresState = 0x405E;
    private const int VarRoamingZapdosState = 0x405F;
    private const int VarRoamingArticunoState = 0x4060;
    private const int VarWiFiFrontierCleared = 0x4064;
    private const int VarAmitySquareStepCount = 0x403A;
    private const int VarJubilifeLotteryTrainerID = 0x403C;
    private const int VarJubilifeLotteryTrainerIDHigh = 0x403D;
    private const int VarHiddenLocationBase = 0x4036;
    private const int VarDistributionEventBase = 0x4043;
    private const int VarTotalTurnsForLastBattle = 0x404D;
    private const int VarVillaVisitor = 0x404E;
    private const int VarVillaVisitorVariant = 0x405A;
    private const int VarFirst = 0x4000;
    private const int VarLast = 0x8000;
    private const int SpeciesTurtwig = 0x183;
    private const int SpeciesChimchar = 0x186;
    private const int SpeciesPiplup = 0x189;
    private const int SpeciesArticuno = 0x90;
    private const int SpeciesZapdos = 0x91;
    private const int SpeciesMoltres = 0x92;
    private const int SpeciesMesprit = 0x1E1;
    private const int SpeciesCresselia = 0x1E8;
    private const int StepCountMax = 10000;
    private const int DistributionEventCount = 4;
    private const int HiddenLocationCount = 4;
    private const int VillaFurnitureCount = 0x14;
    private const int VillaVisitorNone = 0xFF;
    private const int VillaVisitorVariantCount = 5;
    private const int DailyRandomLevelRange = 0x62;
    private const int DailyRandomLevelMin = 2;
    private const int MapVillaInterior = 0x1C9;
    private const int MapVillaExterior = 0x1D0;
    private const uint LotteryMultiplier = 0x41C64E6D;
    private const uint LotteryIncrement = 0x3039;
    private const uint DistributionEventMagicAddress = 0x020EFA7C;
    private const uint HiddenLocationMagicAddress = 0x020EFA74;
    private const uint VillaVisitorChanceAddress = 0x020EFA70;
    private const uint VillaVisitorCountAddress = 0x020EFA6C;
    private const int VillaVisitorTierCount = 3;

    // OFFSET 0x0206AFE0
    private static bool TrySetVarToValue(VarsFlags varsFlags, ushort varID, ushort value)
    {
        int offset = VarsFlags.VarsFlags_GetVarOffset(varsFlags, varID);
        if (varID < VarFirst || varID > VarLast)
        {
            ErrorHandling.GF_AssertFail();
            return false;
        }
        varsFlags.W16(offset, value);
        return true;
    }

    // OFFSET 0x0206B010
    private static ushort TryGetVarValue(VarsFlags varsFlags, ushort varID)
    {
        int offset = VarsFlags.VarsFlags_GetVarOffset(varsFlags, varID);
        return varsFlags.U16(offset);
    }

    // OFFSET 0x0206B024
    public static bool SystemVars_SetPartnerTrainerID(VarsFlags varsFlags, ushort trainerID)
    {
        return TrySetVarToValue(varsFlags, VarPartnerTrainerID, trainerID);
    }

    // OFFSET 0x0206B034
    public static ushort SystemVars_GetPartnerTrainerID(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarPartnerTrainerID);
    }

    // OFFSET 0x0206B044
    public static bool SystemVars_SetPlayerStarter(VarsFlags varsFlags, ushort species)
    {
        return TrySetVarToValue(varsFlags, VarPlayerStarter, species);
    }

    // OFFSET 0x0206B054
    public static ushort SystemVars_GetPlayerStarter(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarPlayerStarter);
    }

    // OFFSET 0x0206B064
    public static ushort SystemVars_GetRivalStarter(VarsFlags varsFlags)
    {
        ushort playerStarter = TryGetVarValue(varsFlags, VarPlayerStarter);
        ushort rivalStarter = SpeciesTurtwig;
        if (playerStarter == SpeciesTurtwig)
        {
            rivalStarter = SpeciesChimchar;
        }
        else if (playerStarter == SpeciesChimchar)
        {
            rivalStarter = SpeciesPiplup;
        }
        return rivalStarter;
    }

    // OFFSET 0x0206B08C
    public static ushort SystemVars_GetPlayerCounterpartStarter(VarsFlags varsFlags)
    {
        ushort playerStarter = TryGetVarValue(varsFlags, VarPlayerStarter);
        ushort counterpartStarter = SpeciesTurtwig;
        if (playerStarter == SpeciesTurtwig)
        {
            counterpartStarter = SpeciesPiplup;
        }
        else if (playerStarter != SpeciesChimchar)
        {
            counterpartStarter = SpeciesChimchar;
        }
        return counterpartStarter;
    }

    // OFFSET 0x0206B0B4
    public static ushort SystemVars_GetSizeContestRecord(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarSizeContestRecord);
    }

    // OFFSET 0x0206B0C4
    public static bool SystemVars_SetSizeContestRecord(VarsFlags varsFlags, ushort value)
    {
        return TrySetVarToValue(varsFlags, VarSizeContestRecord, value);
    }

    // OFFSET 0x0206B0D4
    public static void SystemVars_ResetVsSeeker(VarsFlags varsFlags)
    {
        SystemFlags.SystemFlag_ClearVsSeekerUsed(varsFlags);
        SystemVars_SetVsSeekerStepCount(varsFlags, 0);
    }

    // OFFSET 0x0206B0E8
    public static ushort SystemVars_GetVsSeekerBattery(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarVsSeekerBattery);
    }

    // OFFSET 0x0206B0F8
    public static bool SystemVars_SetVsSeekerBattery(VarsFlags varsFlags, ushort battery)
    {
        return TrySetVarToValue(varsFlags, VarVsSeekerBattery, battery);
    }

    // OFFSET 0x0206B108
    public static ushort SystemVars_GetVsSeekerStepCount(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarVsSeekerStepCount);
    }

    // OFFSET 0x0206B118
    public static bool SystemVars_SetVsSeekerStepCount(VarsFlags varsFlags, ushort stepCount)
    {
        return TrySetVarToValue(varsFlags, VarVsSeekerStepCount, stepCount);
    }

    // OFFSET 0x0206B128
    private static int GetDistributionEventMagicNumber(int eventID)
    {
        if (eventID < 0 || eventID >= DistributionEventCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return RomData.Arm9U16(DistributionEventMagicAddress, DistributionEventCount)[eventID];
    }

    // OFFSET 0x0206B144
    public static void SystemVars_SetDistributionEventMagic(VarsFlags varsFlags, int eventID)
    {
        TrySetVarToValue(varsFlags, (ushort)(VarDistributionEventBase + eventID), (ushort)GetDistributionEventMagicNumber(eventID));
    }

    // OFFSET 0x0206B16C
    public static bool SystemVars_CheckDistributionEvent(VarsFlags varsFlags, int eventID)
    {
        int value = TryGetVarValue(varsFlags, (ushort)(VarDistributionEventBase + eventID));
        return value == GetDistributionEventMagicNumber(eventID);
    }

    // OFFSET 0x0206B194
    private static int GetHiddenLocationMagicNumber(int hiddenLocation)
    {
        if (hiddenLocation < 0 || hiddenLocation >= HiddenLocationCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return RomData.Arm9U16(HiddenLocationMagicAddress, HiddenLocationCount)[hiddenLocation];
    }

    // OFFSET 0x0206B1B0
    public static void SystemVars_SetHiddenLocationMagic(VarsFlags varsFlags, int hiddenLocation)
    {
        TrySetVarToValue(varsFlags, (ushort)(VarHiddenLocationBase + hiddenLocation), (ushort)GetHiddenLocationMagicNumber(hiddenLocation));
    }

    // OFFSET 0x0206B1D8
    public static void SystemVars_ClearHiddenLocation(VarsFlags varsFlags, int hiddenLocation)
    {
        TrySetVarToValue(varsFlags, (ushort)(VarHiddenLocationBase + hiddenLocation), 0);
    }

    // OFFSET 0x0206B1F0
    public static bool SystemVars_CheckHiddenLocation(VarsFlags varsFlags, int hiddenLocation)
    {
        int value = TryGetVarValue(varsFlags, (ushort)(VarHiddenLocationBase + hiddenLocation));
        return value == GetHiddenLocationMagicNumber(hiddenLocation);
    }

    // OFFSET 0x0206B218
    public static bool SystemVars_ClearAmitySquareStepCount(VarsFlags varsFlags)
    {
        return TrySetVarToValue(varsFlags, VarAmitySquareStepCount, 0);
    }

    // OFFSET 0x0206B228
    public static ushort SystemVars_GetAmitySquareStepCount(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarAmitySquareStepCount);
    }

    // OFFSET 0x0206B238
    public static bool SystemVars_IncrementAmitySquareStepCount(VarsFlags varsFlags)
    {
        ushort stepCount = TryGetVarValue(varsFlags, VarAmitySquareStepCount);
        ushort newStepCount = StepCountMax;
        if (stepCount < StepCountMax)
        {
            newStepCount = (ushort)(stepCount + 1);
        }
        return TrySetVarToValue(varsFlags, VarAmitySquareStepCount, newStepCount);
    }

    // OFFSET 0x0206B260
    public static ushort SystemVars_GetNewsPressDeadline(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarNewsPressDeadline);
    }

    // OFFSET 0x0206B270
    public static bool SystemVars_SetNewsPressDeadline(VarsFlags varsFlags, ushort deadlineInDays)
    {
        return TrySetVarToValue(varsFlags, VarNewsPressDeadline, deadlineInDays);
    }

    // OFFSET 0x0206B280
    private static void SetJubilifeLotteryTrainerID(VarsFlags varsFlags, uint trainerID)
    {
        TrySetVarToValue(varsFlags, VarJubilifeLotteryTrainerID, (ushort)trainerID);
        TrySetVarToValue(varsFlags, VarJubilifeLotteryTrainerID, (ushort)(trainerID >> 16));
    }

    // OFFSET 0x0206B2A4
    public static uint SystemVars_GetJubilifeLotteryTrainerID(VarsFlags varsFlags)
    {
        uint low = TryGetVarValue(varsFlags, VarJubilifeLotteryTrainerID);
        uint high = TryGetVarValue(varsFlags, VarJubilifeLotteryTrainerIDHigh);
        return (high << 16) | low;
    }

    // OFFSET 0x0206B2C8
    public static void SystemVars_RandomizeJubilifeLotteryTrainerID(VarsFlags varsFlags)
    {
        uint low = MathUtil.LCRNG_Next();
        uint high = MathUtil.LCRNG_Next();
        SetJubilifeLotteryTrainerID(varsFlags, (high << 16) | low);
    }

    // OFFSET 0x0206B2E4
    public static void SystemVars_SynchronizeJubilifeLotteryTrainerID(SaveData saveData, ushort unused)
    {
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(saveData);
        uint rand = RecordMixedRNG.RecordMixedRNG_GetRand(RecordMixedRNG.SaveData_GetRecordMixedRNG(saveData));
        SetJubilifeLotteryTrainerID(varsFlags, rand * LotteryMultiplier + LotteryIncrement);
    }

    // OFFSET 0x0206B314
    public static ushort SystemVars_GetDailyRandomLevel(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarDailyRandomLevel);
    }

    // OFFSET 0x0206B324
    private static bool SetDailyRandomLevel(VarsFlags varsFlags, ushort level)
    {
        return TrySetVarToValue(varsFlags, VarDailyRandomLevel, level);
    }

    // OFFSET 0x0206B334
    public static void SystemVars_InitDailyRandomLevel(SaveData saveData)
    {
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(saveData);
        int level = MathUtil.LCRNG_Next() % DailyRandomLevelRange + DailyRandomLevelMin;
        SetDailyRandomLevel(varsFlags, (ushort)level);
    }

    // OFFSET 0x0206B354
    public static ushort SystemVars_GetSpiritombCounter(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarSpiritombCounter);
    }

    // OFFSET 0x0206B364
    public static bool SystemVars_SetSpiritombCounter(VarsFlags varsFlags, ushort value)
    {
        return TrySetVarToValue(varsFlags, VarSpiritombCounter, value);
    }

    // OFFSET 0x0206B374
    public static ushort SystemVars_GetUndergroundTalkCounter(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarUndergroundTalkCounter);
    }

    // OFFSET 0x0206B384
    public static bool SystemVars_SetUndergroundTalkCounter(VarsFlags varsFlags, ushort value)
    {
        return TrySetVarToValue(varsFlags, VarUndergroundTalkCounter, value);
    }

    // OFFSET 0x0206B394
    public static ushort SystemVars_GetConsecutiveBonusRoundWins(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarConsecutiveBonusRoundWins);
    }

    // OFFSET 0x0206B3A4
    public static bool SystemVars_SetConsecutiveBonusRoundWins(VarsFlags varsFlags, ushort wins)
    {
        return TrySetVarToValue(varsFlags, VarConsecutiveBonusRoundWins, wins);
    }

    // OFFSET 0x0206B3B4
    public static bool SystemVars_IncrementDepartmentStoreBuyCount(VarsFlags varsFlags)
    {
        ushort buyCount = TryGetVarValue(varsFlags, VarDepartmentStoreBuyCount);
        ushort newBuyCount = StepCountMax;
        if (buyCount < StepCountMax)
        {
            newBuyCount = (ushort)(buyCount + 1);
        }
        return TrySetVarToValue(varsFlags, VarDepartmentStoreBuyCount, newBuyCount);
    }

    // OFFSET 0x0206B3DC
    public static ushort SystemVars_GetDepartmentStoreBuyCount(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarDepartmentStoreBuyCount);
    }

    // OFFSET 0x0206B3EC
    public static ushort SystemVars_GetUndergroundItemsGivenAway(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarUndergroundItemsGivenAway);
    }

    // OFFSET 0x0206B3FC
    public static bool SystemVars_SetUndergroundItemsGivenAway(VarsFlags varsFlags, ushort value)
    {
        return TrySetVarToValue(varsFlags, VarUndergroundItemsGivenAway, value);
    }

    // OFFSET 0x0206B40C
    public static ushort SystemVars_GetUndergroundFossilsUnearthed(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarUndergroundFossilsUnearthed);
    }

    // OFFSET 0x0206B41C
    public static bool SystemVars_SetUndergroundFossilsUnearthed(VarsFlags varsFlags, ushort value)
    {
        return TrySetVarToValue(varsFlags, VarUndergroundFossilsUnearthed, value);
    }

    // OFFSET 0x0206B42C
    public static ushort SystemVars_GetUndergroundTrapsSet(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarUndergroundTrapsSet);
    }

    // OFFSET 0x0206B43C
    public static bool SystemVars_SetUndergroundTrapsSet(VarsFlags varsFlags, ushort value)
    {
        return TrySetVarToValue(varsFlags, VarUndergroundTrapsSet, value);
    }

    // OFFSET 0x0206B44C
    public static ushort SystemVars_GetFriendshipStepCount(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarFriendshipStepCount);
    }

    // OFFSET 0x0206B45C
    public static bool SystemVars_SetFriendshipStepCount(VarsFlags varsFlags, ushort value)
    {
        return TrySetVarToValue(varsFlags, VarFriendshipStepCount, value);
    }

    // OFFSET 0x0206B46C
    public static bool SystemVars_SetEternaGymFlowerClockState(VarsFlags varsFlags, ushort state)
    {
        return TrySetVarToValue(varsFlags, VarEternaGymFlowerClockState, state);
    }

    // OFFSET 0x0206B47C
    public static ushort SystemVars_GetEternaGymFlowerClockState(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarEternaGymFlowerClockState);
    }

    // OFFSET 0x0206B48C
    public static void SystemVars_SetTotalTurnsForLastBattle(VarsFlags varsFlags, ushort turns)
    {
        TrySetVarToValue(varsFlags, VarTotalTurnsForLastBattle, (ushort)(turns + 1));
    }

    // OFFSET 0x0206B4A4
    private static byte CalcVillaVisitorIndex(VarsFlags varsFlags)
    {
        ushort ownedCount = 0;
        for (int furniture = 0; furniture < VillaFurnitureCount; furniture++)
        {
            if (SystemFlags.SystemFlag_HandleOwnsVillaFurniture(varsFlags, SystemFlags.HandleFlagCheck, furniture))
            {
                ownedCount++;
            }
        }
        int tier;
        if (ownedCount >= 12)
        {
            tier = 2;
        }
        else if (ownedCount >= 8)
        {
            tier = 1;
        }
        else
        {
            tier = 0;
        }
        if (MathUtil.LCRNG_Next() % 100 > RomData.Arm9(VillaVisitorChanceAddress, VillaVisitorTierCount)[tier])
        {
            return VillaVisitorNone;
        }
        return (byte)(MathUtil.LCRNG_Next() % RomData.Arm9(VillaVisitorCountAddress, VillaVisitorTierCount)[tier]);
    }

    // OFFSET 0x0206B514
    public static void SystemVars_UpdateVillaVisitor(SaveData saveData)
    {
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(saveData);
        BufferView location = FieldPlayerState.FieldPlayerState_GetPlayerLocation(FieldPlayerState.SaveData_GetFieldPlayerState(saveData));
        uint mapID = location.U32(0);
        if (mapID == MapVillaInterior || mapID == MapVillaExterior)
        {
            return;
        }
        SystemFlags.SystemFlag_ClearVillaVisitorInside(varsFlags);
        SystemFlags.SystemFlag_ClearVillaVisitorOutside(varsFlags);
        InitVillaVisitor(varsFlags, CalcVillaVisitorIndex(varsFlags));
    }

    // OFFSET 0x0206B558
    private static void InitVillaVisitor(VarsFlags varsFlags, ushort visitor)
    {
        TrySetVarToValue(varsFlags, VarVillaVisitor, visitor);
        TrySetVarToValue(varsFlags, VarVillaVisitorVariant, (ushort)(MathUtil.LCRNG_Next() % VillaVisitorVariantCount));
    }

    // OFFSET 0x0206B588
    public static ushort SystemVars_GetBattleFactoryPrintState(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarBattleFactoryPrintState);
    }

    // OFFSET 0x0206B598
    public static ushort SystemVars_GetBattleHallPrintState(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarBattleHallPrintState);
    }

    // OFFSET 0x0206B5A8
    public static ushort SystemVars_GetBattleCastlePrintState(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarBattleCastlePrintState);
    }

    // OFFSET 0x0206B5B8
    public static ushort SystemVars_GetBattleArcadePrintState(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarBattleArcadePrintState);
    }

    // OFFSET 0x0206B5C8
    public static ushort SystemVars_GetBattleTowerPrintState(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarBattleTowerPrintState);
    }

    // OFFSET 0x0206B5D8
    public static ushort SystemVars_GetDistortionWorldProgress(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarDistortionWorldProgress);
    }

    // OFFSET 0x0206B5E8
    public static bool SystemVars_SetDistortionWorldProgress(VarsFlags varsFlags, ushort state)
    {
        return TrySetVarToValue(varsFlags, VarDistortionWorldProgress, state);
    }

    // OFFSET 0x0206B5F8
    public static ushort SystemVars_GetArceusEventState(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarArceusEventState);
    }

    // OFFSET 0x0206B608
    public static bool SystemVars_SetArceusEventState(VarsFlags varsFlags, ushort state)
    {
        return TrySetVarToValue(varsFlags, VarArceusEventState, state);
    }

    // OFFSET 0x0206B618
    public static ushort SystemVars_GetShayminEventState(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarShayminEventState);
    }

    // OFFSET 0x0206B628
    public static bool SystemVars_SetShayminEventState(VarsFlags varsFlags, ushort state)
    {
        return TrySetVarToValue(varsFlags, VarShayminEventState, state);
    }

    // OFFSET 0x0206B638
    private static bool SetRoamingCresseliaState(VarsFlags varsFlags, ushort state)
    {
        return TrySetVarToValue(varsFlags, VarRoamingCresseliaState, state);
    }

    // OFFSET 0x0206B648
    private static bool SetRoamingMespritState(VarsFlags varsFlags, ushort state)
    {
        return TrySetVarToValue(varsFlags, VarRoamingMespritState, state);
    }

    // OFFSET 0x0206B658
    private static bool SetRoamingMoltresState(VarsFlags varsFlags, ushort state)
    {
        return TrySetVarToValue(varsFlags, VarRoamingMoltresState, state);
    }

    // OFFSET 0x0206B668
    private static bool SetRoamingZapdosState(VarsFlags varsFlags, ushort state)
    {
        return TrySetVarToValue(varsFlags, VarRoamingZapdosState, state);
    }

    // OFFSET 0x0206B678
    private static bool SetRoamingArticunoState(VarsFlags varsFlags, ushort state)
    {
        return TrySetVarToValue(varsFlags, VarRoamingArticunoState, state);
    }

    // OFFSET 0x0206B688
    public static void SystemVars_SetRoamingSpeciesState(VarsFlags varsFlags, ushort species, ushort state)
    {
        switch (species)
        {
            case SpeciesMesprit:
                SetRoamingMespritState(varsFlags, state);
                break;
            case SpeciesCresselia:
                SetRoamingCresseliaState(varsFlags, state);
                break;
            case SpeciesMoltres:
                SetRoamingMoltresState(varsFlags, state);
                break;
            case SpeciesZapdos:
                SetRoamingZapdosState(varsFlags, state);
                break;
            case SpeciesArticuno:
                SetRoamingArticunoState(varsFlags, state);
                break;
        }
    }

    // OFFSET 0x0206B6DC
    public static ushort SystemVars_GetDistortionWorldCyrusApperanceState(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarDistortionWorldCyrusApperanceState);
    }

    // OFFSET 0x0206B6EC
    public static bool SystemVars_SetDistortionWorldCyrusApperanceState(VarsFlags varsFlags, ushort state)
    {
        return TrySetVarToValue(varsFlags, VarDistortionWorldCyrusApperanceState, state);
    }

    // OFFSET 0x0206B6FC
    public static ushort SystemVars_GetWiFiFrontierCleared(VarsFlags varsFlags)
    {
        return TryGetVarValue(varsFlags, VarWiFiFrontierCleared);
    }
}
