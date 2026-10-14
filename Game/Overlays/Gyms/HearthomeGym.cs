namespace PlatinumSharp.Game;

public static class HearthomeGym
{
    private const int MovementTypeGymTrainer = 3;

    // OFFSET 0x0224C5DC
    public static int HearthomeGym_ResetTrainerToPost(FieldSystem fieldSystem, MapObject mapObject)
    {
        PersistedMapFeatures features = MiscSaveBlock.SaveData_GetPersistedMapFeatures(FieldSystemCore.FieldSystem_GetSaveData(fieldSystem)!);
        if (PersistedMapFeatures.PersistedMapFeatures_GetID(features) != DynamicMapFeatures.HearthomeGym)
        {
            return 0;
        }
        MapObject.MapObject_SetXInitial(mapObject, MapObject.MapObject_GetX(mapObject));
        MapObject.MapObject_SetZInitial(mapObject, MapObject.MapObject_GetZ(mapObject));
        MapObject.MapObject_SetMovementRangeX(mapObject, 2);
        MapObject.MapObject_SetMovementRangeZ(mapObject, 2);
        MapObject.MapObject_SwitchMovementType(mapObject, MovementTypeGymTrainer);
        return 1;
    }
}
