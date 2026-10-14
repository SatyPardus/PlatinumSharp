namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const int TrainerBattleHeapID = 0xB;
    private const int GroupActionIsEntryValid = 0;
    private const int GroupActionIsEntryActive = 1;
    private const int GroupActionBufferGroupName = 2;
    private const int GroupActionBufferLeaderName = 3;
    private const int GroupActionNameGroup = 4;
    private const int GroupActionJoinGroup = 5;
    private const int GroupActionCreateGroup = 6;
    private const int NamingScreenTypeGroup = 5;
    private const int GroupNameMaxChars = 7;
    private const int GroupLeaderNameSize = 0x40;
    private const int GroupHeapID = 0x20;

    private static void RegisterTrainers(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x02041C40] = ScrCmd_GetRematchTrainerID;
        ported[0x02041C8C] = ScrCmd_GetOverworldWeather;
        ported[0x02042028] = ScrCmd_StartVsSeeker;
        ported[0x0204205C] = ScrCmd_SetMoveCodeForFacingDirection;
        ported[0x02042E64] = ScrCmd_PrintTrainerDialogue;
        ported[0x02048614] = ScrCmd_StartApproachingTrainerTask;
        ported[0x020486E0] = ScrCmd_CheckIsApproachingTrainerTaskDone;
        ported[0x02048750] = ScrCmd_GetApproachingTrainerType;
        ported[0x02048778] = ScrCmd_GetApproachingTrainerID;
        ported[0x020487CC] = ScrCmd_GetTrainerID;
        ported[0x020487FC] = ScrCmd_StartTrainerBattle;
        ported[0x0204887C] = ScrCmd_StartTagBattle;
        ported[0x020488E0] = ScrCmd_GetTrainerMessageTypes;
        ported[0x02048968] = ScrCmd_GetTrainerRematchMessageTypes;
        ported[0x020489F0] = ScrCmd_CheckIsTrainerDoubleBattle;
        ported[0x02048A24] = ScrCmd_PlayTrainerEncounterBGM;
        ported[0x02048A44] = ScrCmd_BlackOutFromBattle;
        ported[0x02048AE0] = ScrCmd_CheckHasTwoAliveMons;
        ported[0x02048B0C] = ScrCmd_StartDummyTrainerBattle;
        ported[0x02048B34] = ScrCmd_SetTargetTrainerDefeated;
        ported[0x02048B60] = ScrCmd_GoToIfTargetTrainerDefeated;
        ported[0x02048BA8] = ScrCmd_GetBattleResult;
        ported[0x02048BD0] = ScrCmd_DoGroupConnectionAction;
    }

    // OFFSET 0x02041C40
    private static bool ScrCmd_GetRematchTrainerID(ScriptContext ctx)
    {
        MapObject? targetObject = Manager(ctx).TargetObject;
        ushort trainerID = VarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, VsSeeker.VsSeeker_GetRematchTrainerID(ctx.FieldSystem, targetObject!, trainerID));
        return false;
    }

    // OFFSET 0x02041C8C
    private static bool ScrCmd_GetOverworldWeather(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, FieldPlayerState.FieldPlayerState_GetWeather(FieldPlayerState.SaveData_GetFieldPlayerState(ctx.FieldSystem.SaveData!)));
        return false;
    }

    // OFFSET 0x02042028
    private static bool ScrCmd_StartVsSeeker(ScriptContext ctx)
    {
        ushort varID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        StringTemplate template = Manager(ctx).StringTemplate!;
        BufferView result = FieldSystemVarsFlags.FieldSystem_GetVarPointer(ctx.FieldSystem, varID)!;
        VsSeeker.VsSeeker_Start(ctx.Task!, template, result);
        return true;
    }

    // OFFSET 0x0204205C
    private static bool ScrCmd_SetMoveCodeForFacingDirection(ScriptContext ctx)
    {
        MapObject? targetObject = Manager(ctx).TargetObject;
        if (targetObject == null)
        {
            return false;
        }
        if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(ctx.FieldSystem, DynamicMapFeatures.HearthomeGym) == 0
            || HearthomeGym.HearthomeGym_ResetTrainerToPost(ctx.FieldSystem, targetObject) == 0)
        {
            VsSeeker.VsSeeker_SetMoveCodeForFacingDirection(ctx.FieldSystem, targetObject);
        }
        return false;
    }

    // OFFSET 0x02042E64
    private static bool ScrCmd_PrintTrainerDialogue(ScriptContext ctx)
    {
        ScriptManager manager = Manager(ctx);
        ushort trainerID = VarValue(ctx);
        ushort messageType = VarValue(ctx);
        TrainerData.Trainer_LoadMessage(trainerID, messageType, manager.MessageBuffer!, TrainerBattleHeapID);
        Bg.Window_FillTilemap(manager.Window, 0xF);
        manager.MessageID = FieldMessage.FieldMessage_Print(manager.Window, manager.MessageBuffer!, PlayerSave.SaveData_GetOptions(ctx.FieldSystem.SaveData!), 1);
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForMessage);
        return true;
    }

    // OFFSET 0x02048614
    private static bool ScrCmd_StartApproachingTrainerTask(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort approachNum = VarValue(ctx);
        ApproachingTrainer trainer = Manager(ctx).Trainers[approachNum == 0 ? 0 : 1];
        trainer.Task = TrainerEncounter.TrainerEncounter_StartApproachingTrainerTask(fieldSystem, trainer.Object!, fieldSystem.PlayerAvatar!, trainer.Direction, trainer.SightRange, 0, trainer.ApproachType, approachNum);
        return false;
    }

    // OFFSET 0x020486E0
    private static bool ScrCmd_CheckIsApproachingTrainerTaskDone(ScriptContext ctx)
    {
        ushort approachNum = VarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, 0);
        ApproachingTrainer trainer = Manager(ctx).Trainers[approachNum == 0 ? 0 : 1];
        if (trainer.Task == null)
        {
            destination.W16(0, 1);
            return true;
        }
        if (TrainerEncounter.TrainerEncounter_IsApproachingTrainerTaskDone(trainer.Task) == 1)
        {
            TrainerEncounter.TrainerEncounter_FreeApproachingTrainerTask(trainer.Task);
            trainer.Task = null;
            destination.W16(0, 1);
        }
        return true;
    }

    // OFFSET 0x02048750
    private static bool ScrCmd_GetApproachingTrainerType(ScriptContext ctx)
    {
        int approachType = Manager(ctx).Trainers[0].ApproachType;
        VarPointer(ctx).W16(0, approachType);
        return false;
    }

    // OFFSET 0x02048778
    private static bool ScrCmd_GetApproachingTrainerID(ScriptContext ctx)
    {
        ScriptManager manager = Manager(ctx);
        ushort approachNum = VarValue(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, approachNum == 0 ? manager.Trainers[0].TrainerID : manager.Trainers[1].TrainerID);
        return false;
    }

    // OFFSET 0x020487CC
    private static bool ScrCmd_GetTrainerID(ScriptContext ctx)
    {
        ushort scriptID = Manager(ctx).ScriptID;
        VarPointer(ctx).W16(0, FieldSystemVarsFlags.Script_GetTrainerID(scriptID));
        return false;
    }

    // OFFSET 0x020487FC
    private static bool ScrCmd_StartTrainerBattle(ScriptContext ctx)
    {
        ScriptManager manager = Manager(ctx);
        ushort enemyTrainer1 = VarValue(ctx);
        ushort enemyTrainer2 = VarValue(ctx);
        int partnerTrainer = 0;
        if (SystemFlags.SystemFlag_CheckHasPartner(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!)))
        {
            partnerTrainer = SystemVars.SystemVars_GetPartnerTrainerID(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        }
        Encounters.Encounter_NewVsTrainer(ctx.Task!, enemyTrainer1, enemyTrainer2, partnerTrainer, TrainerBattleHeapID, result => manager.BattleResult = result);
        return true;
    }

    // OFFSET 0x0204887C
    private static bool ScrCmd_StartTagBattle(ScriptContext ctx)
    {
        ScriptManager manager = Manager(ctx);
        ushort partnerTrainer = VarValue(ctx);
        ushort enemyTrainer1 = VarValue(ctx);
        ushort enemyTrainer2 = VarValue(ctx);
        Encounters.Encounter_NewVsTrainer(ctx.Task!, enemyTrainer1, enemyTrainer2, partnerTrainer, TrainerBattleHeapID, result => manager.BattleResult = result);
        return true;
    }

    private static void ScrCmd_WriteTrainerMessageTypes(ScriptContext ctx, int singleSee, int singleBeat, int singleAfter, int firstSee, int firstBeat, int firstAfter, int secondSee, int secondBeat, int secondAfter)
    {
        ushort scriptID = Manager(ctx).ScriptID;
        BufferView seeType = VarPointer(ctx);
        BufferView beatType = VarPointer(ctx);
        BufferView afterType = VarPointer(ctx);
        int see;
        int beat;
        int after;
        if (!FieldSystemVarsFlags.Script_IsTrainerDoubleBattle(FieldSystemVarsFlags.Script_GetTrainerID(scriptID)))
        {
            see = singleSee;
            beat = singleBeat;
            after = singleAfter;
        }
        else if (FieldSystemVarsFlags.Script_GetTrainerBattlerIndex(scriptID) == 0)
        {
            see = firstSee;
            beat = firstBeat;
            after = firstAfter;
        }
        else
        {
            see = secondSee;
            beat = secondBeat;
            after = secondAfter;
        }
        seeType.W16(0, see);
        beatType.W16(0, beat);
        afterType.W16(0, after);
    }

    // OFFSET 0x020488E0
    private static bool ScrCmd_GetTrainerMessageTypes(ScriptContext ctx)
    {
        ScrCmd_WriteTrainerMessageTypes(ctx, 0, 2, 0, 3, 5, 6, 7, 9, 10);
        return false;
    }

    // OFFSET 0x02048968
    private static bool ScrCmd_GetTrainerRematchMessageTypes(ScriptContext ctx)
    {
        ScrCmd_WriteTrainerMessageTypes(ctx, 0x11, 0, 0, 0x12, 0, 6, 0x13, 0, 10);
        return false;
    }

    // OFFSET 0x020489F0
    private static bool ScrCmd_CheckIsTrainerDoubleBattle(ScriptContext ctx)
    {
        ushort scriptID = Manager(ctx).ScriptID;
        BufferView destination = VarPointer(ctx);
        destination.W16(0, FieldSystemVarsFlags.Script_IsTrainerDoubleBattle(FieldSystemVarsFlags.Script_GetTrainerID(scriptID)) ? 1 : 0);
        return false;
    }

    // OFFSET 0x02048A24
    private static bool ScrCmd_PlayTrainerEncounterBGM(ScriptContext ctx)
    {
        Sound.Sound_SwapBGM(FieldBgm.FieldBGM_GetEyesMeetForTrainer(VarValue(ctx)));
        return true;
    }

    // OFFSET 0x02048A44
    private static bool ScrCmd_BlackOutFromBattle(ScriptContext ctx)
    {
        BlackOut.FieldTask_StartBlackOut(ctx.Task!);
        return true;
    }

    // OFFSET 0x02048AE0
    private static bool ScrCmd_CheckHasTwoAliveMons(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, FieldParty.Party_HasTwoAliveMons(Party.SaveData_GetParty(ctx.FieldSystem.SaveData!)));
        return false;
    }

    // OFFSET 0x02048B0C
    private static bool ScrCmd_StartDummyTrainerBattle(ScriptContext ctx)
    {
        ScriptManager manager = Manager(ctx);
        Encounters.Encounter_NewVsTrainer(ctx.Task!, 1, 0, 0, TrainerBattleHeapID, result => manager.BattleResult = result);
        return true;
    }

    // OFFSET 0x02048B34
    private static bool ScrCmd_SetTargetTrainerDefeated(ScriptContext ctx)
    {
        MapObject targetObject = Manager(ctx).TargetObject!;
        FieldSystemVarsFlags.Script_SetTrainerDefeated(MapObject.MapObject_FieldSystem(targetObject), (ushort)MapObject.MapObject_GetLocalID(targetObject));
        return false;
    }

    // OFFSET 0x02048B60
    private static bool ScrCmd_GoToIfTargetTrainerDefeated(ScriptContext ctx)
    {
        MapObject targetObject = Manager(ctx).TargetObject!;
        int offset = (int)ScriptContext.ScriptContext_ReadWord(ctx);
        if (FieldSystemVarsFlags.Script_IsTrainerDefeated(MapObject.MapObject_FieldSystem(targetObject), (ushort)MapObject.MapObject_GetLocalID(targetObject)))
        {
            ScriptContext.ScriptContext_Jump(ctx, ctx.ScriptPtr + offset);
            return true;
        }
        return false;
    }

    // OFFSET 0x02048BA8
    private static bool ScrCmd_GetBattleResult(ScriptContext ctx)
    {
        int result = Manager(ctx).BattleResult;
        VarPointer(ctx).W16(0, result);
        return true;
    }

    // OFFSET 0x02048BD0
    private static bool ScrCmd_DoGroupConnectionAction(ScriptContext ctx)
    {
        StringTemplate template = Manager(ctx).StringTemplate!;
        RecordMixedRNG rng = RecordMixedRNG.SaveData_GetRecordMixedRNG(ctx.FieldSystem.SaveData!);
        SaveData saveData = ctx.FieldSystem.SaveData!;
        switch (ScriptContext.ScriptContext_ReadHalfWord(ctx))
        {
            case GroupActionIsEntryValid:
            {
                ushort entry = VarValue(ctx);
                VarPointer(ctx).W16(0, RecordMixedRNG.RecordMixedRNG_IsEntryValid(rng, entry) ? 1 : 0);
                return false;
            }
            case GroupActionIsEntryActive:
            {
                ushort entry = VarValue(ctx);
                VarPointer(ctx).W16(0, RecordMixedRNG.RecordMixedRNG_IsEntryEqualToActive(rng, entry) ? 1 : 0);
                return false;
            }
            case GroupActionBufferGroupName:
            {
                ushort groupID = VarValue(ctx);
                ushort templateArg = VarValue(ctx);
                StringTemplate.StringTemplate_SetUnionGroupName(template, saveData, groupID, templateArg, 0);
                break;
            }
            case GroupActionBufferLeaderName:
            {
                ushort groupID = VarValue(ctx);
                ushort templateArg = VarValue(ctx);
                StringTemplate.StringTemplate_SetUnionGroupName(template, saveData, groupID, templateArg, 1);
                break;
            }
            case GroupActionNameGroup:
            {
                ushort[] name = RecordMixedRNG.RecordMixedRNG_GetEntryName(rng, 0, 0);
                BufferView destination = VarPointer(ctx);
                FieldSystemChildProcesses.FieldTask_OpenNamingScreen(ctx.Task!, NamingScreenTypeGroup, 0, GroupNameMaxChars, 0, name, destination);
                return true;
            }
            case GroupActionJoinGroup:
            {
                ushort entry = VarValue(ctx);
                bool hadGroup = RecordMixedRNG.RecordMixedRNG_IsEntryValid(rng, 1);
                RecordMixedRNG.RecordMixedRNG_CopyEntry(rng, entry, 1);
                if (hadGroup)
                {
                    FieldTimeEvents.FieldSystem_SaveTVSegment_GroupJoined(ctx.FieldSystem);
                }
                return false;
            }
            case GroupActionCreateGroup:
            {
                GameString name = GameString.String_Init(GroupLeaderNameSize, GroupHeapID);
                TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(saveData);
                TrainerInfo.TrainerInfo_NameString(trainerInfo, name);
                RecordMixedRNG.RecordMixedRNG_SetEntryName(rng, 0, 1, name);
                RecordMixedRNG.RecordMixedRNG_SetEntryGender(rng, 0, (int)TrainerInfo.TrainerInfo_Gender(trainerInfo));
                RecordMixedRNG.RecordMixedRNG_SetEntryLanguage(rng, 0, GameLanguageEnglish);
                RecordMixedRNG.RecordMixedRNG_SetEntrySeed(rng, 0, MathUtil.MTRNG_Next());
                GameString.String_Free(name);
                RecordMixedRNG.RecordMixedRNG_CopyEntry(rng, 0, 1);
                FieldTimeEvents.FieldSystem_SaveTVSegment_GroupCreated(ctx.FieldSystem);
                break;
            }
        }
        return false;
    }
}
