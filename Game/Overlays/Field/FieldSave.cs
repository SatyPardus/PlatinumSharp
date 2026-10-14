namespace PlatinumSharp.Game;

public static class FieldSave
{
    private const int PoketchEventSave = 4;
    private const int LocationWarpIdOffset = 0x04;
    private const int LocationXOffset = 0x08;
    private const int LocationZOffset = 0x0C;
    private const int LocationFaceDirectionOffset = 0x10;
    private const int SaveResultOk = 2;

    // OFFSET 0x021E200C
    public static bool FieldSystem_Save(FieldSystem fieldSystem)
    {
        FieldSystem_SaveObjectsAndLocation(fieldSystem);
        if (SaveData.SaveData_Save(fieldSystem.SaveData!) == SaveResultOk)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x021E2028
    private static void FieldSystem_SaveObjectsAndLocation(FieldSystem fieldSystem)
    {
        FieldOverworldSave.FieldSystem_SaveObjects(fieldSystem);
        FieldPoketch.FieldSystem_SendPoketchEvent(fieldSystem, PoketchEventSave, 0);
        fieldSystem.Location!.W32(LocationXOffset, (uint)PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!));
        fieldSystem.Location.W32(LocationZOffset, (uint)PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!));
        fieldSystem.Location.W32(LocationWarpIdOffset, 0xFFFFFFFF);
        fieldSystem.Location.W32(LocationFaceDirectionOffset, (uint)PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!));
    }
}
