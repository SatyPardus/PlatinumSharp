namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const byte DailyMonSwarm = 2;
    private const int BadgeCount = 8;

    private const ushort SeqCyclingRoad = 0x4A5;

    private static void RegisterPlayer(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x02042D70] = ScrCmd_SetCyclingBGM;
        ported[0x02046144] = ScrCmd_GetTrainerCardLevel;
        ported[0x02042A8C] = ScrCmd_GetCurrentMapID;
        ported[0x02042D84] = ScrCmd_ForceBicycling;
        ported[0x02042D9C] = ScrCmd_GetPlayerState;
        ported[0x02042DC4] = ScrCmd_SetPlayerState;
        ported[0x02042DDC] = ScrCmd_ChangePlayerState;
        ported[0x02042DEC] = ScrCmd_GetSwarmMapAndSpecies;
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

    // OFFSET 0x02042D70
    private static bool ScrCmd_SetCyclingBGM(ScriptContext ctx)
    {
        FieldBgm.FieldBGM_SetOverride(ctx.FieldSystem, SeqCyclingRoad);
        return false;
    }

    // OFFSET 0x02042D84
    private static bool ScrCmd_ForceBicycling(ScriptContext ctx)
    {
        byte enabled = ScriptContext.ScriptContext_ReadByte(ctx);
        PlayerAvatar.PlayerAvatar_SetOnCyclingRoad(ctx.FieldSystem.PlayerAvatar!, enabled);
        return false;
    }

    // OFFSET 0x02042D9C
    private static bool ScrCmd_GetPlayerState(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, PlayerAvatar.PlayerAvatar_GetPlayerState(ctx.FieldSystem.PlayerAvatar!));
        return false;
    }

    // OFFSET 0x02042DC4
    private static bool ScrCmd_SetPlayerState(ScriptContext ctx)
    {
        ushort state = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        PlayerAvatar.PlayerAvatar_TurnOnRequestStateBit(ctx.FieldSystem.PlayerAvatar!, state);
        return true;
    }

    // OFFSET 0x02042DDC
    private static bool ScrCmd_ChangePlayerState(ScriptContext ctx)
    {
        PlayerAvatarRequestState.PlayerAvatar_ProcessRequestedStates(ctx.FieldSystem.PlayerAvatar!);
        return false;
    }

    // OFFSET 0x02042DEC
    private static bool ScrCmd_GetSwarmMapAndSpecies(ScriptContext ctx)
    {
        SpecialEncounter specialEncounter = SpecialEncounter.SaveData_GetSpecialEncounters(ctx.FieldSystem.SaveData!);
        BufferView mapDest = VarPointer(ctx);
        BufferView speciesDest = VarPointer(ctx);
        Swarms.Swarm_GetMapIdAndSpecies(SpecialEncounter.SpecialEncounter_GetDailyMon(specialEncounter, DailyMonSwarm), mapDest, speciesDest);
        return false;
    }

    // OFFSET 0x02046144
    private static bool ScrCmd_GetTrainerCardLevel(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        BufferView destination = VarPointer(ctx);
        destination.W16(0, TrainerCase.TrainerCase_CalculateTrainerCardLevel(fieldSystem));
        return false;
    }

    // OFFSET 0x02042A8C
    private static bool ScrCmd_GetCurrentMapID(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, (int)ctx.FieldSystem.Location!.U32(0));
        return false;
    }
}
