using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class PersistedMapFeaturesInit
{
    private const uint CanalaveGymPlatformStatesAddress = 0x020F04CC;
    private const int CanalaveGymPlatformCount = 24;
    private const int SunyshoreGymRoomCount = 3;
    private const int LocationZ = 0x0C;
    private const int DistortionWorldFeaturesSize = 0x20;

    private static PersistedMapFeatures Features(FieldSystem fieldSystem)
    {
        return MiscSaveBlock.SaveData_GetPersistedMapFeatures(FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!);
    }

    // OFFSET 0x02071B10
    public static void PersistedMapFeatures_InitForPastoriaGym(FieldSystem fieldSystem)
    {
        PersistedMapFeatures features = Features(fieldSystem);
        PersistedMapFeatures.PersistedMapFeatures_InitWithID(features, DynamicMapFeatures.PastoriaGym);
        byte[] buffer = PersistedMapFeatures.PersistedMapFeatures_GetBuffer(features, DynamicMapFeatures.PastoriaGym);
        buffer[0] = 1;
    }

    // OFFSET 0x02071B30
    public static void PersistedMapFeatures_InitForCanalaveGym(FieldSystem fieldSystem)
    {
        PersistedMapFeatures features = Features(fieldSystem);
        PersistedMapFeatures.PersistedMapFeatures_InitWithID(features, DynamicMapFeatures.CanalaveGym);
        byte[] buffer = PersistedMapFeatures.PersistedMapFeatures_GetBuffer(features, DynamicMapFeatures.CanalaveGym);
        byte[] initialStates = RomData.Arm9(CanalaveGymPlatformStatesAddress, CanalaveGymPlatformCount);
        for (int i = 0; i < CanalaveGymPlatformCount; i++)
        {
            SetCanalaveGymPlatformInitialState((byte)i, initialStates[i], buffer);
        }
    }

    // OFFSET 0x02071B6C
    public static void PersistedMapFeatures_InitForSunyshoreGym(FieldSystem fieldSystem, byte roomID)
    {
        int startZ = 0;
        if (roomID >= SunyshoreGymRoomCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        PersistedMapFeatures features = Features(fieldSystem);
        PersistedMapFeatures.PersistedMapFeatures_InitWithID(features, DynamicMapFeatures.SunyshoreGym);
        byte[] buffer = PersistedMapFeatures.PersistedMapFeatures_GetBuffer(features, DynamicMapFeatures.SunyshoreGym);
        Bin.W32(buffer, 4, roomID);
        switch (roomID)
        {
            case 0:
                Bin.W32(buffer, 0, 2);
                startZ = 14;
                break;
            case 1:
                Bin.W32(buffer, 0, 1);
                startZ = 21;
                break;
            case 2:
                Bin.W32(buffer, 0, 0);
                startZ = 25;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
        if ((int)fieldSystem.Location!.U32(LocationZ) == startZ)
        {
            Bin.W32(buffer, 0, 0);
        }
    }

    // OFFSET 0x02071BD0
    public static void PersistedMapFeatures_InitForEternaGym(FieldSystem fieldSystem)
    {
        PersistedMapFeatures features = Features(fieldSystem);
        PersistedMapFeatures.PersistedMapFeatures_InitWithID(features, DynamicMapFeatures.EternaGym);
        byte[] buffer = PersistedMapFeatures.PersistedMapFeatures_GetBuffer(features, DynamicMapFeatures.EternaGym);
        buffer[0] = 0;
        buffer[1] = 0;
        buffer[2] = 0;
        buffer[3] = 0;
    }

    // OFFSET 0x02071BF8
    public static void PersistedMapFeatures_InitForVeilstoneGym(FieldSystem fieldSystem)
    {
        PersistedMapFeatures features = Features(fieldSystem);
        PersistedMapFeatures.PersistedMapFeatures_InitWithID(features, DynamicMapFeatures.VeilstoneGym);
        byte[] buffer = PersistedMapFeatures.PersistedMapFeatures_GetBuffer(features, DynamicMapFeatures.VeilstoneGym);
        Bin.W32(buffer, 0, 0);
    }

    // OFFSET 0x02071C18
    public static void PersistedMapFeatures_InitForHearthomeGym(FieldSystem fieldSystem)
    {
        PersistedMapFeatures features = Features(fieldSystem);
        PersistedMapFeatures.PersistedMapFeatures_InitWithID(features, DynamicMapFeatures.HearthomeGym);
        PersistedMapFeatures.PersistedMapFeatures_GetBuffer(features, DynamicMapFeatures.HearthomeGym);
    }

    // OFFSET 0x02071C34
    public static void PersistedMapFeatures_InitForVilla(FieldSystem fieldSystem)
    {
        PersistedMapFeatures features = Features(fieldSystem);
        PersistedMapFeatures.PersistedMapFeatures_InitWithID(features, DynamicMapFeatures.Villa);
        byte[] buffer = PersistedMapFeatures.PersistedMapFeatures_GetBuffer(features, DynamicMapFeatures.Villa);
        buffer[0] = 0;
        buffer[1] = 0;
        buffer[2] = 0;
        buffer[3] = 0;
    }

    // OFFSET 0x02071C5C
    public static void PersistedMapFeatures_InitForDistortionWorld(FieldSystem fieldSystem)
    {
        PersistedMapFeatures features = Features(fieldSystem);
        PersistedMapFeatures.PersistedMapFeatures_InitWithID(features, DynamicMapFeatures.DistortionWorld);
        byte[] buffer = PersistedMapFeatures.PersistedMapFeatures_GetBuffer(features, DynamicMapFeatures.DistortionWorld);
        Array.Clear(buffer, 0, DistortionWorldFeaturesSize);
    }

    // OFFSET 0x02071C80
    private static void SetCanalaveGymPlatformInitialState(byte index, byte startInPositionB, byte[] platformStates)
    {
        if (startInPositionB > 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        uint states = Bin.U32(platformStates, 0);
        uint lowMask = index == 0 ? 0 : 0xFFFFFFFF >> (32 - index);
        uint highMask = 0xFFFFFFFF << (index + 1);
        uint low = states & lowMask;
        uint high = states & highMask;
        Bin.W32(platformStates, 0, ((uint)startInPositionB << index) | high | low);
    }

    // OFFSET 0x02071CB4
    public static int PersistedMapFeatures_IsCurrentDynamicMap(FieldSystem fieldSystem, int dynamicMapFeaturesID)
    {
        if (dynamicMapFeaturesID == PersistedMapFeatures.PersistedMapFeatures_GetID(Features(fieldSystem)))
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02071CD0
    public static void PersistedMapFeatures_InitForGreatMarsh(FieldSystem fieldSystem)
    {
        PersistedMapFeatures features = Features(fieldSystem);
        if (PersistedMapFeatures.PersistedMapFeatures_GetID(features) != DynamicMapFeatures.GreatMarshTram)
        {
            PersistedMapFeatures.PersistedMapFeatures_InitWithID(features, DynamicMapFeatures.GreatMarshTram);
            byte[] buffer = PersistedMapFeatures.PersistedMapFeatures_GetBuffer(features, DynamicMapFeatures.GreatMarshTram);
            Bin.W16(buffer, 0, 2);
        }
    }
}
