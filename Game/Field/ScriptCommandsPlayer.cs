namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const int BadgeCount = 8;

    private static void RegisterPlayer(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x0204E440] = ScrCmd_GiveRunningShoes;
        ported[0x0204E45C] = ScrCmd_CheckBadge;
    }

    // OFFSET 0x0204E440
    private static bool ScrCmd_GiveRunningShoes(ScriptContext ctx)
    {
        PlayerAvatar.PlayerData_SetRunningShoes(FieldPlayerState.FieldPlayerState_GetPlayerData(FieldPlayerState.SaveData_GetFieldPlayerState(ctx.FieldSystem.SaveData!)), 1);
        return false;
    }

    // OFFSET 0x0204E45C
    private static bool ScrCmd_CheckBadge(ScriptContext ctx)
    {
        ushort badgeID = VarValue(ctx);
        BufferView destVar = VarPointer(ctx);
        if (badgeID >= BadgeCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        destVar.W16(0, TrainerInfo.TrainerInfo_HasBadge(PlayerSave.SaveData_GetTrainerInfo(ctx.FieldSystem.SaveData!), badgeID) ? 1 : 0);
        return false;
    }
}
