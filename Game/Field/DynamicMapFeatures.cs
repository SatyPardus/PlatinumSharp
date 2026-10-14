using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class DynamicMapFeatures
{
    public const int None = 0;
    public const int PastoriaGym = 1;
    public const int HearthomeGym = 2;
    public const int CanalaveGym = 3;
    public const int VeilstoneGym = 4;
    public const int SunyshoreGym = 5;
    public const int GreatMarshTram = 6;
    public const int PlatformLift = 7;
    public const int EternaGym = 8;
    public const int DistortionWorld = 9;
    public const int Villa = 10;

    private const int LocationMapHeaderID = 0x00;
    private const int MapHeaderEternaGym = 0x43;

    private static int CurrentID(FieldSystem fieldSystem)
    {
        SaveData saveData = FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!;
        return PersistedMapFeatures.PersistedMapFeatures_GetID(MiscSaveBlock.SaveData_GetPersistedMapFeatures(saveData));
    }

    // OFFSET 0x02068344
    public static void DynamicMapFeatures_Init(FieldSystem fieldSystem)
    {
        switch (CurrentID(fieldSystem))
        {
            case None:
                break;
            case PastoriaGym:
                ov8_02249DBC(fieldSystem);
                break;
            case HearthomeGym:
                ov8_0224C198(fieldSystem);
                break;
            case CanalaveGym:
                ov8_0224A8C8(fieldSystem);
                break;
            case VeilstoneGym:
                ov8_0224B764(fieldSystem);
                break;
            case SunyshoreGym:
                ov8_0224ABD4(fieldSystem);
                break;
            case GreatMarshTram:
                ov6_02242AF0(fieldSystem);
                break;
            case PlatformLift:
                sub_0207160C(fieldSystem);
                break;
            case EternaGym:
                ov8_0224B020(fieldSystem);
                break;
            case DistortionWorld:
                ov9_02249960(fieldSystem);
                break;
            case Villa:
                VillaMap.Villa_DynamicMapFeaturesInit(fieldSystem);
                break;
        }
    }

    // OFFSET 0x02068368
    public static void DynamicMapFeatures_Free(FieldSystem fieldSystem)
    {
        switch (CurrentID(fieldSystem))
        {
            case HearthomeGym:
                ov8_0224C388(fieldSystem);
                break;
            case CanalaveGym:
                ov8_0224AAE8(fieldSystem);
                break;
            case VeilstoneGym:
                ov8_0224B7C0(fieldSystem);
                break;
            case SunyshoreGym:
                ov8_0224ACAC(fieldSystem);
                break;
            case EternaGym:
                ov8_0224B0FC(fieldSystem);
                break;
            case DistortionWorld:
                ov9_02249A60(fieldSystem);
                break;
            case Villa:
                VillaMap.Villa_DynamicMapFeaturesFree(fieldSystem);
                break;
        }
    }

    // OFFSET 0x02068390
    public static int DynamicMapFeatures_CheckCollision(FieldSystem fieldSystem, int tileX, int tileZ, int height, ref int collides)
    {
        switch (CurrentID(fieldSystem))
        {
            case PastoriaGym:
                return ov8_02249A40(fieldSystem, tileX, tileZ, height, ref collides);
            case CanalaveGym:
                return ov8_0224AAFC(fieldSystem, tileX, tileZ, height, ref collides);
            case VeilstoneGym:
                return ov8_0224B7D4(fieldSystem, tileX, tileZ, height, ref collides);
            case SunyshoreGym:
                return ov8_0224ACC0(fieldSystem, tileX, tileZ, height, ref collides);
            case EternaGym:
                return ov8_0224B110(fieldSystem, tileX, tileZ, height, ref collides);
            case DistortionWorld:
                return ov9_02249E50(fieldSystem, tileX, tileZ, height, ref collides);
            case Villa:
                return VillaMap.Villa_DynamicMapFeaturesCheckCollision(fieldSystem, tileX, tileZ, height, ref collides);
        }
        return 0;
    }

    // OFFSET 0x020683D8
    public static int DynamicMapFeatures_WillPlayerJumpEternaGymClock(FieldSystem fieldSystem, int tileX, int tileZ, int unused, int direction)
    {
        if (fieldSystem.Location!.U32(LocationMapHeaderID) == MapHeaderEternaGym
            && ov8_0224B714(fieldSystem, tileX, tileZ, direction) != 0)
        {
            return 1;
        }
        return 0;
    }
}
