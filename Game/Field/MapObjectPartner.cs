namespace PlatinumSharp.Game;

public static class MapObjectPartner
{
    private const int TrainerTypeNone = 0;
    private const int TrainerTypeLast = 8;
    private const uint StatusActive = 1;

    // OFFSET 0x02069EB8
    public static MapObject? MapObject_GetTrainerPartner(MapObject mapObject)
    {
        int index = 0;
        int trainerType = MapObject.MapObject_GetTrainerType(mapObject);
        int mapHeaderID = MapObject.MapObject_GetMapHeaderID(mapObject);
        int trainerID = TrainerEncounter.MapObject_GetTrainerIDFromScript(mapObject);
        MapObjectManager mapObjectManager = MapObject.MapObject_MapObjectManager(mapObject);
        if (trainerType == TrainerTypeNone || (uint)trainerType > TrainerTypeLast)
        {
            return null;
        }
        while (MapObjectManager.MapObjectMan_FindObjectWithStatus(mapObjectManager, out MapObject? other, ref index, StatusActive) == 1)
        {
            if (mapObject != other
                && mapHeaderID == MapObject.MapObject_GetMapHeaderID(other!)
                && trainerID == TrainerEncounter.MapObject_GetTrainerIDFromScript(other!))
            {
                return other;
            }
        }
        return null;
    }
}
