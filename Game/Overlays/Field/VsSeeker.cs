namespace PlatinumSharp.Game;

public sealed class VsSeekerSystem
{
    public const int MaxNPCs = 0x40;

    public int State;
    public FieldSystem? FieldSystem;
    public VarsFlags? VarsFlags;
    public readonly MapObject?[] NPCs = new MapObject?[MaxNPCs];
    public ushort NumNPCs;
    public ushort NumActiveAnimations;
    public ushort[]? Result;
    public StringTemplate? Template;
    public SysTask? PlayerStateTask;
}

public sealed class VsSeekerAnimationTask
{
    public SysTask? Task;
    public SysTask? AnimationTask;
    public VsSeekerSystem? VsSeeker;
}

public static class VsSeeker
{
    private const ushort ItemVsSeeker = 0x1BB;
    private const int HeapIDField = 4;
    private const int MaxBattery = 100;
    private const int MaxActiveSteps = 100;
    private const int MovementTypeVsSeekerSpin = 0x31;
    private const int MovementTypeLookAround = 2;
    private const int MovementTypeHiddenFirst = 0x33;
    private const int MovementTypeHiddenCount = 4;
    private const int FieldOverlayID = 5;
    private const int ScanRadiusX = 7;
    private const int ScanRadiusUp = 7;
    private const int ScanRadiusDown = 6;
    private const ushort SeqVsSeekerBeep = 0x620;
    private const uint AnimTrainerNotice = 0x021F8E34;
    private const int AnimTrainerNoticeBytes = 8;
    private const uint AnimTrainerRematch = 0x021F8E3C;
    private const int AnimTrainerRematchBytes = 0xC;
    private const uint RematchTable = 0x021F8E48;
    private const int RematchEntrySize = 0xC;
    private const int RematchTableSearchCount = 0xF0;
    private const int RematchLevels = 6;
    private const ushort RematchIndexNone = 0xFF;
    private const ushort RematchTrainerSkip = 0xFFFF;
    private const int RematchChance = 0x32;

    private const int StateWaitForNPCs = 0;
    private const int StateCheckUsability = 1;
    private const int StateStart = 2;
    private const int StateWaitForPlayerAnim = 3;
    private const int StatePickRematchTrainers = 4;
    private const int StateWaitForRematchAnims = 5;
    private const int StateWaitForVsSeekerSfx = 6;
    private const int StateNoBattery = 7;
    private const int StateNoTrainers = 8;
    private const int StateDone = 9;

    private const int UsabilityNoBattery = 0;
    private const int UsabilityNoTrainers = 1;
    private const int UsabilityOK = 2;

    private const ushort ResultOK = 0;
    private const ushort ResultNoBattery = 1;
    private const ushort ResultNoTrainers = 2;
    private const ushort ResultNoTrainersPicked = 3;

    private static readonly int[] FacingRematchMoveCodes = { 0xE, 0xF, 0x10, 0x11 };

    // OFFSET 0x021DB888
    public static void VsSeeker_Start(FieldTask taskMan, StringTemplate template, ushort[] outResult)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(taskMan);
        VsSeekerSystem vsSeeker = new();
        vsSeeker.FieldSystem = fieldSystem;
        vsSeeker.VarsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        vsSeeker.Result = outResult;
        vsSeeker.Template = template;
        FieldTask.FieldTask_InitCall(taskMan, VsSeeker_ExecuteTask, vsSeeker);
    }

    // OFFSET 0x021DB8D8
    private static bool VsSeeker_ExecuteTask(FieldTask taskMan)
    {
        VsSeekerSystem vsSeeker = (VsSeekerSystem)FieldTask.FieldTask_GetEnv(taskMan)!;
        switch (vsSeeker.State)
        {
            case StateWaitForNPCs:
                if (VsSeeker_WaitForNPCsToPause(vsSeeker.FieldSystem!) == 1)
                {
                    VsSeekerSystem_SetState(vsSeeker, StateCheckUsability);
                }
                break;
            case StateCheckUsability:
            {
                VsSeekerSystem_CollectViableNPCs(vsSeeker);
                int usability = VsSeekerSystem_CheckUsability(vsSeeker);
                if (usability == UsabilityOK)
                {
                    vsSeeker.Result![0] = ResultOK;
                    VsSeekerSystem_SetState(vsSeeker, StateStart);
                }
                else if (usability == UsabilityNoBattery)
                {
                    vsSeeker.Result![0] = ResultNoBattery;
                    VsSeekerSystem_SetState(vsSeeker, StateNoBattery);
                }
                else
                {
                    vsSeeker.Result![0] = ResultNoTrainers;
                    VsSeekerSystem_SetState(vsSeeker, StateNoTrainers);
                }
                break;
            }
            case StateStart:
                vsSeeker.PlayerStateTask = PlayerFieldMoves.FieldSystem_StartSprite17Task(vsSeeker.FieldSystem!);
                SoundPlayback.Sound_PlayEffect(SeqVsSeekerBeep);
                SystemVars.SystemVars_SetVsSeekerBattery(vsSeeker.VarsFlags!, 0);
                VsSeekerSystem_SetState(vsSeeker, StateWaitForPlayerAnim);
                break;
            case StateWaitForPlayerAnim:
                if (VsSeekerSystem_GetNumActiveAnimations(vsSeeker) == 0)
                {
                    VsSeekerSystem_SetState(vsSeeker, StatePickRematchTrainers);
                }
                break;
            case StatePickRematchTrainers:
                if (VsSeekerSystem_PickRematchTrainers(vsSeeker) == 0)
                {
                    vsSeeker.Result![0] = ResultNoTrainersPicked;
                }
                VsSeekerSystem_SetState(vsSeeker, StateWaitForRematchAnims);
                break;
            case StateWaitForRematchAnims:
                if (VsSeekerSystem_GetNumActiveAnimations(vsSeeker) == 0)
                {
                    VsSeekerSystem_SetState(vsSeeker, StateWaitForVsSeekerSfx);
                }
                break;
            case StateWaitForVsSeekerSfx:
                if (SoundPlayback.Sound_IsEffectPlaying(SeqVsSeekerBeep) == 0)
                {
                    PlayerFieldMoves.FieldSystem_EndSprite17Task(vsSeeker.PlayerStateTask);
                    VsSeekerSystem_SetState(vsSeeker, StateDone);
                }
                break;
            case StateNoBattery:
            {
                int missingBattery = MaxBattery - SystemVars.SystemVars_GetVsSeekerBattery(vsSeeker.VarsFlags!);
                int numDigits;
                if (missingBattery / 10 == 0)
                {
                    numDigits = 1;
                }
                else if (missingBattery / 100 == 0)
                {
                    numDigits = 2;
                }
                else
                {
                    numDigits = 3;
                }
                StringTemplate.StringTemplate_SetNumber(vsSeeker.Template!, 0, missingBattery, (uint)numDigits, 1, 1);
                VsSeekerSystem_SetState(vsSeeker, StateDone);
                break;
            }
            case StateNoTrainers:
                VsSeekerSystem_SetState(vsSeeker, StateDone);
                break;
            case StateDone:
                return true;
        }
        return false;
    }

    // OFFSET 0x021DBA54
    private static void VsSeekerSystem_SetState(VsSeekerSystem vsSeeker, int state)
    {
        vsSeeker.State = state;
    }

    // OFFSET 0x021DBA58
    private static int VsSeekerSystem_CheckUsability(VsSeekerSystem vsSeeker)
    {
        if (SystemVars.SystemVars_GetVsSeekerBattery(vsSeeker.VarsFlags!) != MaxBattery)
        {
            return UsabilityNoBattery;
        }
        if (vsSeeker.NumNPCs == 0)
        {
            return UsabilityNoTrainers;
        }
        return UsabilityOK;
    }

    private static bool VsSeeker_IsTrainerTypeViable(int trainerType)
    {
        switch (trainerType)
        {
            case 1:
            case 2:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
                return true;
        }
        return false;
    }

    // OFFSET 0x021DBA7C
    private static void VsSeekerSystem_CollectViableNPCs(VsSeekerSystem vsSeeker)
    {
        FieldSystem fieldSystem = vsSeeker.FieldSystem!;
        uint numObjectEvents = MapHeaderData.MapHeaderData_GetNumObjectEvents(fieldSystem);
        int numViable = 0;
        for (uint i = 0; i < numObjectEvents; i++)
        {
            vsSeeker.NPCs[i] = null;
        }
        int playerX = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        int playerZ = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        int minX = playerX - ScanRadiusX;
        int maxX = playerX + ScanRadiusX;
        int minZ = playerZ - ScanRadiusUp;
        int maxZ = playerZ + ScanRadiusDown;
        if (minX < 0)
        {
            minX = 0;
        }
        if (minZ < 0)
        {
            minZ = 0;
        }
        for (uint i = 0; i < numObjectEvents; i++)
        {
            MapObject? mapObj = MapObjectManager.MapObjMan_LocalMapObjByIndex(fieldSystem.MapObjectManager, (int)i);
            if (mapObj == null || !VsSeeker_IsTrainerTypeViable(MapObject.MapObject_GetTrainerType(mapObj)))
            {
                continue;
            }
            int x = MapObject.MapObject_GetX(mapObj);
            int z = MapObject.MapObject_GetZ(mapObj);
            if (x < minX || x > maxX || z < minZ || z > maxZ)
            {
                continue;
            }
            if (VsSeeker_IsMoveCodeHidden(MapObject.MapObject_GetMovementType(mapObj)) == 0)
            {
                vsSeeker.NPCs[numViable] = mapObj;
                numViable++;
            }
        }
        vsSeeker.NumNPCs = (ushort)numViable;
    }

    // OFFSET 0x021DBB70
    private static int VsSeeker_IsMoveCodeHidden(int moveCode)
    {
        if ((uint)(moveCode - MovementTypeHiddenFirst) < MovementTypeHiddenCount)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021DBB94
    public static int VsSeeker_UpdateStepCount(FieldSystem fieldSystem)
    {
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        ushort battery = SystemVars.SystemVars_GetVsSeekerBattery(varsFlags);
        ushort activeSteps = SystemVars.SystemVars_GetVsSeekerStepCount(varsFlags);
        if (BagItems.Bag_CanRemoveItem(BagItems.SaveData_GetBag(fieldSystem.SaveData!), ItemVsSeeker, 1, HeapIDField) && battery < MaxBattery)
        {
            battery++;
            SystemVars.SystemVars_SetVsSeekerBattery(varsFlags, battery);
        }
        if (SystemFlags.SystemFlag_CheckVsSeekerUsed(varsFlags))
        {
            if (activeSteps < MaxActiveSteps)
            {
                activeSteps++;
                SystemVars.SystemVars_SetVsSeekerStepCount(varsFlags, activeSteps);
            }
            if (activeSteps == MaxActiveSteps)
            {
                SystemVars.SystemVars_ResetVsSeeker(varsFlags);
                VsSeeker_ClearRematchMoveCodes(fieldSystem);
            }
        }
        return 0;
    }

    // OFFSET 0x021DBC08
    private static void VsSeeker_ClearRematchMoveCodes(FieldSystem fieldSystem)
    {
        uint numObjectEvents = MapHeaderData.MapHeaderData_GetNumObjectEvents(fieldSystem);
        for (uint i = 0; i < numObjectEvents; i++)
        {
            MapObject? trainer = MapObjectManager.MapObjMan_LocalMapObjByIndex(fieldSystem.MapObjectManager, (int)i);
            if (trainer == null)
            {
                continue;
            }
            if (MapObject.MapObject_GetMovementType(trainer) == MovementTypeVsSeekerSpin)
            {
                VsSeeker_SetMoveCode(trainer, MovementTypeLookAround);
            }
        }
    }

    // OFFSET 0x021DBC40
    private static void VsSeekerSystem_StartAnimation(VsSeekerSystem vsSeeker, MapObject mapObj, uint animCmdList, int animCmdBytes)
    {
        SysTask animTask = MovementActions.MapObject_StartAnimation(mapObj, new BufferView(RomData.Overlay(FieldOverlayID, animCmdList, animCmdBytes), 0));
        vsSeeker.NumActiveAnimations++;
        VsSeekerSystem_StartAnimationTask(vsSeeker, animTask);
    }

    // OFFSET 0x021DBC64
    private static void VsSeekerSystem_StartAnimationTask(VsSeekerSystem vsSeeker, SysTask animTask)
    {
        VsSeekerAnimationTask data = new();
        data.VsSeeker = vsSeeker;
        data.AnimationTask = animTask;
        data.Task = SysTask.SysTask_Start(VsSeeker_TrackAnimation, data, 0);
    }

    // OFFSET 0x021DBC94
    private static void VsSeeker_TrackAnimation(SysTask task, object? param)
    {
        VsSeekerAnimationTask data = (VsSeekerAnimationTask)param!;
        if (MovementActions.MapObject_HasAnimationEnded(data.AnimationTask!) != 1)
        {
            return;
        }
        if (data.VsSeeker!.NumActiveAnimations == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        data.VsSeeker.NumActiveAnimations--;
        MovementActions.MapObject_FinishAnimation(data.AnimationTask!);
        SysTask.SysTask_Done(data.Task!);
    }

    // OFFSET 0x021DBCD4
    private static int VsSeekerSystem_GetNumActiveAnimations(VsSeekerSystem vsSeeker)
    {
        return vsSeeker.NumActiveAnimations;
    }

    // OFFSET 0x021DBCE0
    private static int VsSeekerSystem_PickRematchTrainers(VsSeekerSystem vsSeeker)
    {
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(vsSeeker.FieldSystem!.SaveData!);
        int picked = 0;
        for (int i = 0; i < vsSeeker.NumNPCs; i++)
        {
            MapObject trainer = vsSeeker.NPCs[i]!;
            ushort trainerID = VsSeeker_GetTrainerIDFromMapObject(trainer);
            if (!FieldSystemVarsFlags.Script_IsTrainerDefeated(vsSeeker.FieldSystem, trainerID))
            {
                VsSeekerSystem_StartAnimation(vsSeeker, trainer, AnimTrainerNotice, AnimTrainerNoticeBytes);
                picked = 1;
                continue;
            }
            if (MathUtil.LCRNG_Next() % 100 >= RematchChance || VsSeeker_IsTrainerDoingRematchAnimation(trainer) != 0)
            {
                continue;
            }
            VsSeeker_SetMoveCode(trainer, MovementTypeVsSeekerSpin);
            VsSeekerSystem_StartAnimation(vsSeeker, trainer, AnimTrainerRematch, AnimTrainerRematchBytes);
            MapObject? secondTrainer = VsSeeker_GetSecondDoubleBattleTrainer(vsSeeker.FieldSystem, trainer, 0);
            if (secondTrainer != null)
            {
                VsSeeker_SetMoveCode(secondTrainer, MovementTypeVsSeekerSpin);
                VsSeekerSystem_StartAnimation(vsSeeker, secondTrainer, AnimTrainerRematch, AnimTrainerRematchBytes);
            }
            picked = 1;
            SystemFlags.SystemFlag_SetVsSeekerUsed(varsFlags);
        }
        return picked;
    }

    // OFFSET 0x021DBD88
    private static ushort VsSeeker_GetTrainerIDFromMapObject(MapObject trainer)
    {
        return FieldSystemVarsFlags.Script_GetTrainerID((ushort)MapObject.MapObject_GetScript(trainer));
    }

    // OFFSET 0x021DBD98
    public static ushort VsSeeker_GetRematchTrainerID(FieldSystem fieldSystem, MapObject trainer, ushort trainerID)
    {
        if (VsSeeker_IsTrainerDoingRematchAnimation(trainer) == 0)
        {
            return 0;
        }
        ushort rematchDataIndex = VsSeeker_GetRematchDataIndexForTrainer(fieldSystem, trainerID);
        if (rematchDataIndex == RematchIndexNone)
        {
            return 0;
        }
        ushort level = VsSeeker_GetCurrentLevelForRematchData(fieldSystem, rematchDataIndex);
        level = VsSeeker_AdjustRematchLevel(fieldSystem, rematchDataIndex, level);
        return VsSeeker_GetTrainerIDForRematchLevel(rematchDataIndex, level);
    }

    private static ushort RematchTrainerID(int rematchDataIndex, int level)
    {
        return RomData.OverlayU16(FieldOverlayID, (uint)(RematchTable + rematchDataIndex * RematchEntrySize + level * 2), 1)[0];
    }

    // OFFSET 0x021DBDDC
    private static ushort VsSeeker_GetRematchDataIndexForTrainer(FieldSystem fieldSystem, ushort trainerID)
    {
        for (ushort i = 0; i < RematchTableSearchCount; i++)
        {
            if (trainerID == RematchTrainerID(i, 0))
            {
                return i;
            }
        }
        return RematchIndexNone;
    }

    // OFFSET 0x021DBDFC
    private static ushort VsSeeker_GetCurrentLevelForRematchData(FieldSystem fieldSystem, ushort rematchDataIndex)
    {
        int level;
        for (level = 1; level < RematchLevels; level++)
        {
            ushort trainerID = RematchTrainerID(rematchDataIndex, level);
            if (trainerID == 0)
            {
                return (ushort)(level - 1);
            }
            if (trainerID != RematchTrainerSkip && !FieldSystemVarsFlags.Script_IsTrainerDefeated(fieldSystem, trainerID))
            {
                return (ushort)level;
            }
        }
        return (ushort)(level - 1);
    }

    // OFFSET 0x021DBE48
    private static ushort VsSeeker_AdjustRematchLevel(FieldSystem fieldSystem, ushort rematchDataIndex, ushort level)
    {
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        if (level != 0 && !SystemFlags.SystemFlag_CheckUnlockedVsSeekerLevel(varsFlags, level))
        {
            level = VsSeeker_GetNextLowerRematchLevel(rematchDataIndex, level);
        }
        return level;
    }

    // OFFSET 0x021DBE70
    private static ushort VsSeeker_GetNextLowerRematchLevel(ushort rematchDataIndex, ushort level)
    {
        for (level = (ushort)(level - 1); level != 0; level--)
        {
            if (RematchTrainerID(rematchDataIndex, level) != RematchTrainerSkip)
            {
                return level;
            }
        }
        return 0;
    }

    // OFFSET 0x021DBEA4
    private static ushort VsSeeker_GetTrainerIDForRematchLevel(ushort rematchDataIndex, ushort level)
    {
        return RematchTrainerID(rematchDataIndex, level);
    }

    // OFFSET 0x021DBEB8
    private static int VsSeeker_IsTrainerDoingRematchAnimation(MapObject trainer)
    {
        if (MapObject.MapObject_GetMovementType(trainer) == MovementTypeVsSeekerSpin)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021DBECC
    private static void VsSeeker_SetMoveCode(MapObject trainer, int movementType)
    {
        MapObject.MapObject_SwitchMovementType(trainer, movementType);
    }

    // OFFSET 0x021DBED4
    public static void VsSeeker_SetMoveCodeForFacingDirection(FieldSystem fieldSystem, MapObject? trainer)
    {
        if (trainer == null)
        {
            return;
        }
        int facingDir = MapObject.MapObject_GetFacingDir(trainer);
        int moveCode = facingDir >= 0 && facingDir < 3 ? FacingRematchMoveCodes[facingDir] : FacingRematchMoveCodes[3];
        MapObject? secondTrainer = VsSeeker_GetSecondDoubleBattleTrainer(fieldSystem, trainer, 1);
        if (secondTrainer != null)
        {
            VsSeeker_SetMoveCode(secondTrainer, (ushort)moveCode);
        }
        VsSeeker_SetMoveCode(trainer, (ushort)moveCode);
    }

    // OFFSET 0x021DBF20
    private static int VsSeeker_WaitForNPCsToPause(FieldSystem fieldSystem)
    {
        uint numObjectEvents = MapHeaderData.MapHeaderData_GetNumObjectEvents(fieldSystem);
        int anyMoving = 0;
        for (uint i = 0; i < numObjectEvents; i++)
        {
            MapObject? mapObj = MapObjectManager.MapObjMan_LocalMapObjByIndex(fieldSystem.MapObjectManager, (int)i);
            if (mapObj == null)
            {
                continue;
            }
            if (MapObject.MapObject_IsMoving(mapObj) == 1)
            {
                MapObject.MapObject_SetPauseMovementOff(mapObj);
                anyMoving = 1;
            }
            else
            {
                MapObject.MapObject_SetPauseMovementOn(mapObj);
            }
        }
        if (anyMoving == 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x021DBF70
    private static MapObject? VsSeeker_GetSecondDoubleBattleTrainer(FieldSystem fieldSystem, MapObject trainer, int mode)
    {
        uint numObjectEvents = MapHeaderData.MapHeaderData_GetNumObjectEvents(fieldSystem);
        ushort scriptID = (ushort)MapObject.MapObject_GetScript(trainer);
        ushort trainerID = FieldSystemVarsFlags.Script_GetTrainerID(scriptID);
        if (!FieldSystemVarsFlags.Script_IsTrainerDoubleBattle(trainerID))
        {
            return null;
        }
        for (uint i = 0; i < numObjectEvents; i++)
        {
            MapObject? mapObj = MapObjectManager.MapObjMan_LocalMapObjByIndex(fieldSystem.MapObjectManager, (int)i);
            if (mapObj == null)
            {
                continue;
            }
            if (mode == 0 && MapObject.MapObject_GetMovementType(mapObj) == MovementTypeVsSeekerSpin)
            {
                continue;
            }
            if (!VsSeeker_IsTrainerTypeViable(MapObject.MapObject_GetTrainerType(mapObj)))
            {
                continue;
            }
            int otherScript = MapObject.MapObject_GetScript(mapObj);
            ushort otherTrainerID = FieldSystemVarsFlags.Script_GetTrainerID((ushort)otherScript);
            if (scriptID != otherScript && trainerID == otherTrainerID)
            {
                return mapObj;
            }
        }
        return null;
    }
}
