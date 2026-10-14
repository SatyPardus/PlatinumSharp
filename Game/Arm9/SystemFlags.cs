namespace PlatinumSharp.Game;

public static class SystemFlags
{
    public const int HandleFlagClear = 0;
    public const int HandleFlagSet = 1;
    public const int HandleFlagCheck = 2;

    private const int FlagBagAcquired = 0x960;
    private const int FlagHasPartner = 0x961;
    private const int FlagStrengthActive = 0x962;
    private const int FlagJournalAcquired = 0x963;
    private const int FlagGameCompleted = 0x964;
    private const int FlagStep = 0x965;
    private const int FlagCommunicationClubAccessible = 0x966;
    private const int FlagSafariGameActive = 0x967;
    private const int FlagVsSeekerUsed = 0x96D;
    private const int FlagEnteredUnderground = 0x96E;
    private const int FlagDiggingForFossils = 0x96F;
    private const int FlagSphereAcquired = 0x970;
    private const int FlagCreatedSecretBase = 0x971;
    private const int FlagDecoratedSecretBase = 0x972;
    private const int FlagDeliveredStolenFlag = 0x973;
    private const int FlagContestHallVisited = 0x978;
    private const int FlagInPalPark = 0x979;
    private const int FlagFlashActive = 0x97A;
    private const int FlagDefogActive = 0x97B;
    private const int FlagPoketchHidden = 0x97C;
    private const int FlagFreedGalacticHQPokemon = 0x97D;
    private const int FlagMetBebe = 0x97E;
    private const int FlagConnectedToWiFi = 0x98C;
    private const int FlagHasSeenUndergroundRoarkIntro = 0x992;
    private const int FlagOnCyclingRoad = 0x994;
    private const int FlagForceBikingInGate = 0x996;
    private const int FlagVillaVisitorInside = 0x9AB;
    private const int FlagVillaVisitorOutside = 0x9AC;
    private const int FlagDistortionWorldPuzzleFinished = 0x9AD;
    private const int FlagContestMasterCool = 0x968;
    private const int FlagContestMasterBeauty = 0x969;
    private const int FlagContestMasterCute = 0x96A;
    private const int FlagContestMasterSmart = 0x96B;
    private const int FlagContestMasterTough = 0x96C;
    private const int FlagVsSeekerLevel1 = 0x97F;
    private const int FlagVsSeekerLevel2 = 0x980;
    private const int FlagVsSeekerLevel3 = 0x981;
    private const int FlagVsSeekerLevel4 = 0x982;
    private const int FlagVsSeekerLevel5 = 0x983;
    private const int FlagAltMusicLakeValor = 0x984;
    private const int FlagAltMusicGalacticHQ = 0x985;
    private const int FlagAltMusicGalacticHQ1F = 0x986;
    private const int FlagAltMusicGalacticEternaBuilding = 0x987;
    private const int FlagAltMusicValleyWindworksBuilding = 0x988;
    private const int FlagAltMusicFloaromaMeadow = 0x989;
    private const int FlagAltMusicRoute224 = 0x98A;
    private const int FlagAltMusicChampionRoom = 0x98B;
    private const int FlagAltMusicLakeVerity = 0x98E;
    private const int FlagAltMusicLakeAcuity = 0x98F;
    private const int FlagAltMusicRowansLab = 0x993;
    private const int FlagAltMusicPalPark = 0x995;
    private const int FlagVillaFurnitureBase = 0x997;
    private const int FlagGiratinaAnimationBase = 0x9AE;
    private const int FlagFirstArrivalBase = 0x9B0;
    private const int FirstArrivalCount = 0x45;
    private const int FlagDistortionWorldSteppingStones = 0x977;
    private const int VillaFurnitureCount = 0x14;

    // OFFSET 0x0206A8DC
    private static void SetFlag(VarsFlags varsFlags, ushort flagID)
    {
        VarsFlags.VarsFlags_SetFlag(varsFlags, flagID);
    }

    // OFFSET 0x0206A8E4
    private static void ClearFlag(VarsFlags varsFlags, ushort flagID)
    {
        VarsFlags.VarsFlags_ClearFlag(varsFlags, flagID);
    }

    // OFFSET 0x0206A8EC
    private static bool CheckFlag(VarsFlags varsFlags, ushort flagID)
    {
        return VarsFlags.VarsFlags_CheckFlag(varsFlags, flagID);
    }

    // OFFSET 0x0206A8F4
    private static bool HandleFlag(VarsFlags varsFlags, int op, uint flagID)
    {
        switch (op)
        {
            case HandleFlagSet:
                SetFlag(varsFlags, (ushort)flagID);
                break;
            case HandleFlagClear:
                ClearFlag(varsFlags, (ushort)flagID);
                break;
            case HandleFlagCheck:
                return CheckFlag(varsFlags, (ushort)flagID);
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        return false;
    }

    // OFFSET 0x0206A92C
    public static void SystemFlag_SetBagAcquired(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagBagAcquired);
    }

    // OFFSET 0x0206A938
    public static bool SystemFlag_CheckBagAcquired(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagBagAcquired);
    }

    // OFFSET 0x0206A944
    public static void SystemFlag_SetGameCompleted(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagGameCompleted);
    }

    // OFFSET 0x0206A954
    public static bool SystemFlag_CheckGameCompleted(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagGameCompleted);
    }

    // OFFSET 0x0206A964
    public static void SystemFlag_SetHasPartner(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagHasPartner);
    }

    // OFFSET 0x0206A974
    public static void SystemFlag_ClearHasPartner(VarsFlags varsFlags)
    {
        ClearFlag(varsFlags, FlagHasPartner);
    }

    // OFFSET 0x0206A984
    public static bool SystemFlag_CheckHasPartner(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagHasPartner);
    }

    // OFFSET 0x0206A994
    public static void SystemFlag_SetStep(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagStep);
    }

    // OFFSET 0x0206A9A4
    public static void SystemFlag_ClearStep(VarsFlags varsFlags)
    {
        ClearFlag(varsFlags, FlagStep);
    }

    // OFFSET 0x0206A9B4
    public static bool SystemFlag_CheckStep(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagStep);
    }

    // OFFSET 0x0206A9C4
    public static void SystemFlag_SetVsSeekerUsed(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagVsSeekerUsed);
    }

    // OFFSET 0x0206A9D4
    public static void SystemFlag_ClearVsSeekerUsed(VarsFlags varsFlags)
    {
        ClearFlag(varsFlags, FlagVsSeekerUsed);
    }

    // OFFSET 0x0206A9E4
    public static bool SystemFlag_CheckVsSeekerUsed(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagVsSeekerUsed);
    }

    // OFFSET 0x0206A9F4
    public static void SystemFlag_SetEnteredUnderground(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagEnteredUnderground);
    }

    // OFFSET 0x0206AA04
    public static void SystemFlag_SetDiggingForFossils(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagDiggingForFossils);
    }

    // OFFSET 0x0206AA14
    public static void SystemFlag_SetSphereAcquired(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagSphereAcquired);
    }

    // OFFSET 0x0206AA20
    public static void SystemFlag_SetCreatedSecretBase(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagCreatedSecretBase);
    }

    // OFFSET 0x0206AA30
    public static void SystemFlag_SetDecoratedSecretBase(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagDecoratedSecretBase);
    }

    // OFFSET 0x0206AA40
    public static void SystemFlag_SetDeliveredStolenFlag(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagDeliveredStolenFlag);
    }

    // OFFSET 0x0206AA50
    public static void SystemFlag_SetContestMaster(VarsFlags varsFlags, int contestType)
    {
        switch (contestType)
        {
            case 0:
                SetFlag(varsFlags, FlagContestMasterCool);
                break;
            case 1:
                SetFlag(varsFlags, FlagContestMasterBeauty);
                break;
            case 2:
                SetFlag(varsFlags, FlagContestMasterCute);
                break;
            case 3:
                SetFlag(varsFlags, FlagContestMasterSmart);
                break;
            case 4:
                SetFlag(varsFlags, FlagContestMasterTough);
                break;
        }
    }

    // OFFSET 0x0206AAA8
    public static bool SystemFlag_CheckContestMaster(VarsFlags varsFlags, int contestType)
    {
        switch (contestType)
        {
            case 0:
                return CheckFlag(varsFlags, FlagContestMasterCool);
            case 1:
                return CheckFlag(varsFlags, FlagContestMasterBeauty);
            case 2:
                return CheckFlag(varsFlags, FlagContestMasterCute);
            case 3:
                return CheckFlag(varsFlags, FlagContestMasterSmart);
            case 4:
                return CheckFlag(varsFlags, FlagContestMasterTough);
        }
        return true;
    }

    // OFFSET 0x0206AB00
    public static bool SystemFlag_CheckUnlockedVsSeekerLevel(VarsFlags varsFlags, int level)
    {
        bool unlocked = false;
        switch (level)
        {
            case 1:
                unlocked = CheckFlag(varsFlags, FlagVsSeekerLevel1);
                break;
            case 2:
                unlocked = CheckFlag(varsFlags, FlagVsSeekerLevel2);
                break;
            case 3:
                unlocked = CheckFlag(varsFlags, FlagVsSeekerLevel3);
                break;
            case 4:
                unlocked = CheckFlag(varsFlags, FlagVsSeekerLevel4);
                break;
            case 5:
                unlocked = CheckFlag(varsFlags, FlagVsSeekerLevel5);
                break;
        }
        return unlocked;
    }

    // OFFSET 0x0206AB68
    public static ushort SystemFlag_GetAltMusicForHeader(VarsFlags varsFlags, int mapHeaderID)
    {
        ushort music = 0;
        switch (mapHeaderID)
        {
            case 0x13A:
                if (CheckFlag(varsFlags, FlagAltMusicLakeValor))
                {
                    music = 0x42E;
                }
                break;
            case 0x13C:
                if (CheckFlag(varsFlags, FlagAltMusicLakeValor))
                {
                    music = 0x429;
                }
                break;
            case 0x138:
                if (CheckFlag(varsFlags, FlagAltMusicLakeVerity))
                {
                    music = 0x42E;
                }
                break;
            case 0x13E:
                if (CheckFlag(varsFlags, FlagAltMusicLakeAcuity))
                {
                    music = 0x42E;
                }
                break;
            case 0xFB:
                if (CheckFlag(varsFlags, FlagAltMusicPalPark))
                {
                    music = 0x42D;
                }
                break;
            case 0x1A6:
                if (CheckFlag(varsFlags, FlagAltMusicRowansLab))
                {
                    music = 0x44A;
                }
                break;
            case 0x131:
            case 0x132:
            case 0x133:
            case 0x134:
            case 0x135:
            case 0x136:
            case 0x1EE:
            case 0x1F1:
                if (CheckFlag(varsFlags, FlagAltMusicGalacticHQ))
                {
                    music = (ushort)(!Rtc.IsNight() ? 0x3F8 : 0x415);
                }
                else if (mapHeaderID == 0x131)
                {
                    if (CheckFlag(varsFlags, FlagAltMusicGalacticHQ1F))
                    {
                        music = 0x42B;
                    }
                }
                break;
            case 0x48:
            case 0x49:
            case 0x4A:
            case 0x4B:
                if (CheckFlag(varsFlags, FlagAltMusicGalacticEternaBuilding))
                {
                    music = (ushort)(!Rtc.IsNight() ? 0x3F5 : 0x412);
                }
                break;
            case 0xC9:
                if (CheckFlag(varsFlags, FlagAltMusicValleyWindworksBuilding))
                {
                    music = (ushort)(!Rtc.IsNight() ? 0x3FF : 0x41C);
                }
                break;
            case 0x100:
                if (CheckFlag(varsFlags, FlagAltMusicFloaromaMeadow))
                {
                    music = (ushort)(!Rtc.IsNight() ? 0x3EE : 0x40B);
                }
                break;
            case 0x18F:
                if (CheckFlag(varsFlags, FlagAltMusicRoute224))
                {
                    music = (ushort)(!Rtc.IsNight() ? 0x3EE : 0x40B);
                }
                break;
            case 0xB9:
                if (CheckFlag(varsFlags, FlagAltMusicChampionRoom))
                {
                    music = 0x3E9;
                }
                break;
        }
        return music;
    }

    // OFFSET 0x0206AD9C
    public static void SystemFlag_SetCommunicationClubAccessible(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagCommunicationClubAccessible);
    }

    // OFFSET 0x0206ADAC
    public static void SystemFlag_ClearCommunicationClubAccessible(VarsFlags varsFlags)
    {
        ClearFlag(varsFlags, FlagCommunicationClubAccessible);
    }

    // OFFSET 0x0206ADBC
    public static bool SystemFlag_CheckCommunicationClubAccessible(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagCommunicationClubAccessible);
    }

    // OFFSET 0x0206ADCC
    public static bool SystemFlag_CheckContestHallVisited(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagContestHallVisited);
    }

    // OFFSET 0x0206ADDC
    public static bool SystemFlag_CheckHasSeenUndergroundRoarkIntro(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagHasSeenUndergroundRoarkIntro);
    }

    // OFFSET 0x0206ADEC
    public static bool SystemFlag_CheckFreedGalacticHQPokemon(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagFreedGalacticHQPokemon);
    }

    // OFFSET 0x0206ADFC
    public static bool SystemFlag_CheckMetBebe(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagMetBebe);
    }

    // OFFSET 0x0206AE0C
    public static void SystemFlag_SetPoketchHidden(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagPoketchHidden);
    }

    // OFFSET 0x0206AE1C
    public static void SystemFlag_ClearPoketchHidden(VarsFlags varsFlags)
    {
        ClearFlag(varsFlags, FlagPoketchHidden);
    }

    // OFFSET 0x0206AE2C
    public static bool SystemFlag_CheckPoketchHidden(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagPoketchHidden);
    }

    // OFFSET 0x0206AE3C
    public static void SystemFlag_SetSafariGameActive(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagSafariGameActive);
    }

    // OFFSET 0x0206AE4C
    public static void SystemFlag_ClearSafariGameActive(VarsFlags varsFlags)
    {
        ClearFlag(varsFlags, FlagSafariGameActive);
    }

    // OFFSET 0x0206AE5C
    public static bool SystemFlag_CheckSafariGameActive(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagSafariGameActive);
    }

    // OFFSET 0x0206AE6C
    public static void SystemFlag_SetInPalPark(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagInPalPark);
    }

    // OFFSET 0x0206AE7C
    public static void SystemFlag_ClearInPalPark(VarsFlags varsFlags)
    {
        ClearFlag(varsFlags, FlagInPalPark);
    }

    // OFFSET 0x0206AE8C
    public static bool SystemFlag_CheckInPalPark(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagInPalPark);
    }

    // OFFSET 0x0206AE9C
    public static void SystemFlag_ClearOnCyclingRoad(VarsFlags varsFlags)
    {
        ClearFlag(varsFlags, FlagOnCyclingRoad);
    }

    // OFFSET 0x0206AEAC
    public static bool SystemFlag_HandleStrengthActive(VarsFlags varsFlags, int op)
    {
        return HandleFlag(varsFlags, op, FlagStrengthActive);
    }

    // OFFSET 0x0206AEBC
    public static void SystemFlag_SetFlashActive(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagFlashActive);
    }

    // OFFSET 0x0206AECC
    public static void SystemFlag_ClearFlashActive(VarsFlags varsFlags)
    {
        ClearFlag(varsFlags, FlagFlashActive);
    }

    // OFFSET 0x0206AEDC
    public static bool SystemFlag_CheckFlashActive(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagFlashActive);
    }

    // OFFSET 0x0206AEEC
    public static void SystemFlag_SetDefogActive(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagDefogActive);
    }

    // OFFSET 0x0206AEFC
    public static void SystemFlag_ClearDefogActive(VarsFlags varsFlags)
    {
        ClearFlag(varsFlags, FlagDefogActive);
    }

    // OFFSET 0x0206AF0C
    public static bool SystemFlag_CheckDefogActive(VarsFlags varsFlags)
    {
        return CheckFlag(varsFlags, FlagDefogActive);
    }

    // OFFSET 0x0206AF1C
    public static bool SystemFlag_HandleForceBikingInGate(VarsFlags varsFlags, int op)
    {
        return HandleFlag(varsFlags, op, FlagForceBikingInGate);
    }

    // OFFSET 0x0206AF2C
    public static bool SystemFlag_HandleJournalAcquired(VarsFlags varsFlags, int op)
    {
        return HandleFlag(varsFlags, op, FlagJournalAcquired);
    }

    // OFFSET 0x0206AF3C
    public static bool SystemFlag_HandleFirstArrivalToZone(VarsFlags varsFlags, int op, uint firstArrival)
    {
        if (firstArrival >= FirstArrivalCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return HandleFlag(varsFlags, op, FlagFirstArrivalBase + firstArrival);
    }

    // OFFSET 0x0206AF5C
    public static void SystemFlag_SetConnectedToWiFi(VarsFlags varsFlags)
    {
        SetFlag(varsFlags, FlagConnectedToWiFi);
    }

    // OFFSET 0x0206AF6C
    public static bool SystemFlag_HandleOwnsVillaFurniture(VarsFlags varsFlags, int op, int furniture)
    {
        if (furniture >= VillaFurnitureCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return HandleFlag(varsFlags, op, (uint)(FlagVillaFurnitureBase + furniture));
    }

    // OFFSET 0x0206AF90
    public static void SystemFlag_ClearVillaVisitorInside(VarsFlags varsFlags)
    {
        ClearFlag(varsFlags, FlagVillaVisitorInside);
    }

    // OFFSET 0x0206AFA0
    public static void SystemFlag_ClearVillaVisitorOutside(VarsFlags varsFlags)
    {
        ClearFlag(varsFlags, FlagVillaVisitorOutside);
    }

    // OFFSET 0x0206AFB0
    public static bool SystemFlag_HandleGiratinaAnimation(VarsFlags varsFlags, int op, int animation)
    {
        return HandleFlag(varsFlags, op, (uint)(FlagGiratinaAnimationBase + animation));
    }

    // OFFSET 0x0206AFC0
    public static bool SystemFlag_HandleDistortionWorldPuzzleFinished(VarsFlags varsFlags, int op)
    {
        return HandleFlag(varsFlags, op, FlagDistortionWorldPuzzleFinished);
    }

    // OFFSET 0x0206AFD0
    public static bool SystemFlag_CheckDistortionWorldSteppingStones(VarsFlags varsFlags)
    {
        return HandleFlag(varsFlags, HandleFlagCheck, FlagDistortionWorldSteppingStones);
    }
}
