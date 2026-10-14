using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class FieldOverworldSave
{
    // OFFSET 0x0203A7A8
    public static void FieldSystem_SaveObjects(FieldSystem fieldSystem)
    {
        FieldOverworldState savedObjects = FieldOverworldState.SaveData_GetFieldOverworldState(fieldSystem.SaveData!);
        MapObjectManager.MapObjectMan_SaveAll(fieldSystem, fieldSystem.MapObjectManager, savedObjects, FieldOverworldState.SavedMapObjectCount);
    }

    // OFFSET 0x0203A7C0
    public static void FieldSystem_LoadObjects(FieldSystem fieldSystem)
    {
        FieldOverworldState savedObjects = FieldOverworldState.SaveData_GetFieldOverworldState(fieldSystem.SaveData!);
        MapObjectManager.MapObjectMan_LoadAllObjects(fieldSystem.MapObjectManager, savedObjects, FieldOverworldState.SavedMapObjectCount);
    }
}
