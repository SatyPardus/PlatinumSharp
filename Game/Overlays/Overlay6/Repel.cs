namespace PlatinumSharp.Game;

public static class Repel
{
    private const ushort ScriptRepelWoreOff = 0x7F0;

    // OFFSET 0x02246BF4
    public static int Repel_UpdateSteps(SaveData saveData, FieldSystem fieldSystem)
    {
        ref byte repelSteps = ref SpecialEncounter.SpecialEncounter_GetRepelSteps(SpecialEncounter.SaveData_GetSpecialEncounters(saveData));
        if (repelSteps != 0)
        {
            repelSteps--;
            if (repelSteps == 0)
            {
                ScriptManager.ScriptManager_Set(fieldSystem, ScriptRepelWoreOff, null);
                return 1;
            }
        }
        return 0;
    }
}
