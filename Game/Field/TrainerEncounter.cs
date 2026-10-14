using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class ApproachingTrainerTemplate
{
    public int Distance;
    public int Direction;
    public int Script;
    public int TrainerID;
    public int IsDoubleBattle;
    public MapObject? TrainerMapObject;
}

public sealed class ApproachingTrainerData
{
    public int State;
    public int Done;
    public int Direction;
    public int SightRange;
    public int Unused;
    public int ApproachType;
    public int ApproachNum;
    public int Delay;
    public OverworldAnimManager? AnimManager;
    public MapObject? MapObject;
    public PlayerAvatar? PlayerAvatar;
    public FieldSystem? FieldSystem;
}

public static class TrainerEncounter
{
    public const int DistanceInvalid = -1;
    public const int ApproachTypeSingles = 0;
    public const int ApproachTypeDoubles = 1;
    public const int ApproachTypeVs2 = 2;

    private const int TrainerTypeNormal = 1;
    private const int TrainerTypeViewAllDirections = 2;
    private const int TrainerTypeFaceSides = 4;
    private const int TrainerTypeSpinClockwise = 8;

    private const ushort ScriptTrainerSight = 0xF58;
    private const uint StatusActive = 1;
    private const uint StatusFaceLocked = 0x40;
    private const uint StatusPauseDirection = 0x80;
    private const uint CollisionIgnoredMask = 1;
    private const uint CollisionObject = 4;
    private const int DirCount = 4;
    private const int DirNone = -1;
    private const int DynamicMapFeaturesHearthomeGym = 2;

    private const int MovementTypeHiddenFirst = 0x33;
    private const int MovementTypeHiddenLast = 0x36;
    private const int MovementTypeNone = 0;
    private const int MovementActionFace = 0;
    private const int MovementActionWalkNormal = 0xC;
    private const int MovementActionRevealTrainer = 0x65;

    private const uint TaskPriority = 0xFF;
    private const int RevealDelayFrames = 30;
    private const int NextToPlayerDelayFrames = 8;

    private const int StateUnpauseMovement = 0;
    private const int StateTryFaceDirection = 1;
    private const int StateCheckRevealOrFaceDirection = 2;
    private const int StateFaceDirection = 3;
    private const int StateWaitFaceDirection = 4;
    private const int StateShowEmote = 5;
    private const int StateWaitEmote = 6;
    private const int StateRevealTrainer = 7;
    private const int StateWaitRevealTrainer = 8;
    private const int StateDelayCheckNextToPlayer = 9;
    private const int StateCheckNextToPlayer = 10;
    private const int StateStepTowardsPlayer = 11;
    private const int StateWaitStepTowardsPlayer = 12;
    private const int StateDelayNextToPlayer = 13;
    private const int StateTryPlayerFaceTrainer = 14;
    private const int StateWaitPlayerFaceTrainer = 15;
    private const int StateSwitchMovementTypeNone = 16;
    private const int StateDone = 17;

    private delegate int DistanceFunc(MapObject mapObject, int range, int playerX, int playerZ, int unused);

    private static readonly DistanceFunc[] sDistanceFuncs =
    {
        GetDistanceNorth,
        GetDistanceSouth,
        GetDistanceWest,
        GetDistanceEast,
    };

    private static readonly Func<ApproachingTrainerData, int>[] sApproachingTrainerStates =
    {
        ApproachingTrainerTask_UnpauseMovement,
        ApproachingTrainerTask_TryFaceDirection,
        ApproachingTrainerTask_CheckRevealOrFaceDirection,
        ApproachingTrainerTask_FaceDirection,
        ApproachingTrainerTask_WaitFaceDirection,
        ApproachingTrainerTask_ShowEmote,
        ApproachingTrainerTask_WaitEmote,
        ApproachingTrainerTask_RevealTrainer,
        ApproachingTrainerTask_WaitRevealTrainer,
        ApproachingTrainerTask_DelayCheckNextToPlayer,
        ApproachingTrainerTask_CheckNextToPlayer,
        ApproachingTrainerTask_StepTowardsPlayer,
        ApproachingTrainerTask_WaitStepTowardsPlayer,
        ApproachingTrainerTask_DelayNextToPlayer,
        ApproachingTrainerTask_TryPlayerFaceTrainer,
        ApproachingTrainerTask_WaitPlayerFaceTrainer,
        ApproachingTrainerTask_SwitchMovementTypeNone,
        ApproachingTrainerTask_Done,
    };

    // OFFSET 0x02067A84
    public static int FieldSystem_CheckForTrainersWantingBattle(FieldSystem fieldSystem, int hasTwoAliveMons)
    {
        ApproachingTrainerTemplate trainer = new();
        MapObjectManager mapObjectManager = fieldSystem.MapObjectManager!;
        PlayerAvatar playerAvatar = fieldSystem.PlayerAvatar!;
        if (!FindUndefeatedTrainerInSight(fieldSystem, mapObjectManager, playerAvatar, null, trainer))
        {
            return 0;
        }
        if (trainer.IsDoubleBattle == 0)
        {
            ScriptManager.ScriptManager_Set(fieldSystem, ScriptTrainerSight, trainer.TrainerMapObject);
            if (hasTwoAliveMons != 0)
            {
                ApproachingTrainerTemplate secondTrainer = new();
                if (FindUndefeatedTrainerInSight(fieldSystem, mapObjectManager, playerAvatar, trainer.TrainerMapObject, secondTrainer))
                {
                    ScriptManager.ScriptManager_SetApproachingTrainer(fieldSystem, trainer.TrainerMapObject, trainer.Distance, trainer.Direction, trainer.Script, trainer.TrainerID, ApproachTypeVs2, 0);
                    ScriptManager.ScriptManager_SetApproachingTrainer(fieldSystem, secondTrainer.TrainerMapObject, secondTrainer.Distance, secondTrainer.Direction, secondTrainer.Script, secondTrainer.TrainerID, ApproachTypeVs2, 1);
                    return 1;
                }
            }
            ScriptManager.ScriptManager_SetApproachingTrainer(fieldSystem, trainer.TrainerMapObject, trainer.Distance, trainer.Direction, trainer.Script, trainer.TrainerID, ApproachTypeSingles, 0);
            return 1;
        }
        if (trainer.IsDoubleBattle == 1)
        {
            if (hasTwoAliveMons == 0)
            {
                return 0;
            }
            ApproachingTrainerTemplate partner = new();
            MapObject? partnerMapObject = FindTrainerPartner(fieldSystem, mapObjectManager, trainer.TrainerMapObject!, trainer.TrainerID);
            ApproachingTrainer_Init(partner, partnerMapObject!, trainer.Distance, trainer.Direction);
            ScriptManager.ScriptManager_Set(fieldSystem, ScriptTrainerSight, trainer.TrainerMapObject);
            ScriptManager.ScriptManager_SetApproachingTrainer(fieldSystem, trainer.TrainerMapObject, trainer.Distance, trainer.Direction, trainer.Script, trainer.TrainerID, ApproachTypeDoubles, 0);
            ScriptManager.ScriptManager_SetApproachingTrainer(fieldSystem, partner.TrainerMapObject, partner.Distance, partner.Direction, partner.Script, partner.TrainerID, ApproachTypeDoubles, 1);
            return 1;
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x02067BA8
    private static bool FindUndefeatedTrainerInSight(FieldSystem fieldSystem, MapObjectManager mapObjectManager, PlayerAvatar playerAvatar, MapObject? knownTrainer, ApproachingTrainerTemplate template)
    {
        int index = 0;
        while (MapObjectManager.MapObjectMan_FindObjectWithStatus(mapObjectManager, out MapObject? trainerMapObject, ref index, StatusActive) != 0)
        {
            if (knownTrainer != null && knownTrainer == trainerMapObject)
            {
                continue;
            }
            int distance = GetTrainerDistToPlayer(trainerMapObject!, playerAvatar, out int direction);
            if (distance == DistanceInvalid)
            {
                continue;
            }
            if (FieldSystemVarsFlags.Script_IsTrainerDefeated(fieldSystem, (ushort)MapObject_GetTrainerID(trainerMapObject!)))
            {
                continue;
            }
            ApproachingTrainer_Init(template, trainerMapObject!, distance, direction);
            return true;
        }
        return false;
    }

    // OFFSET 0x02067C28
    private static void ApproachingTrainer_Init(ApproachingTrainerTemplate template, MapObject trainerMapObject, int distance, int direction)
    {
        template.Distance = distance;
        template.Direction = direction;
        template.Script = MapObject.MapObject_GetScript(trainerMapObject);
        template.TrainerID = FieldSystemVarsFlags.Script_GetTrainerID((ushort)template.Script);
        template.IsDoubleBattle = FieldSystemVarsFlags.Script_IsTrainerDoubleBattle((ushort)template.TrainerID) ? 1 : 0;
        template.TrainerMapObject = trainerMapObject;
    }

    // OFFSET 0x02067C54
    private static int GetTrainerType(MapObject trainerMapObject)
    {
        int trainerType = MapObject.MapObject_GetTrainerType(trainerMapObject);
        if (trainerType >= TrainerTypeFaceSides && trainerType <= TrainerTypeSpinClockwise)
        {
            trainerType = TrainerTypeNormal;
        }
        return trainerType;
    }

    // OFFSET 0x02067C80
    private static int GetTrainerDistToPlayer(MapObject trainerMapObject, PlayerAvatar playerAvatar, out int direction)
    {
        direction = 0;
        int trainerType = GetTrainerType(trainerMapObject);
        if (trainerType == TrainerTypeNormal)
        {
            int playerX = PlayerAvatar.PlayerAvatar_GetXPos(playerAvatar);
            int playerZ = PlayerAvatar.PlayerAvatar_GetZPos(playerAvatar);
            int facingDir = MapObject.MapObject_GetFacingDir(trainerMapObject);
            int sightRange = MapObject.MapObject_GetDataAt(trainerMapObject, 0);
            int distance = GetDistanceToPlayerFromDir(trainerMapObject, facingDir, sightRange, playerX, playerZ, 0);
            if (distance != DistanceInvalid && !IsPathInterrupted(trainerMapObject, facingDir, distance))
            {
                direction = facingDir;
                return distance;
            }
            return DistanceInvalid;
        }
        if (trainerType == TrainerTypeViewAllDirections)
        {
            int playerX = PlayerAvatar.PlayerAvatar_GetXPos(playerAvatar);
            int playerZ = PlayerAvatar.PlayerAvatar_GetZPos(playerAvatar);
            int sightRange = MapObject.MapObject_GetDataAt(trainerMapObject, 0);
            for (int dir = 0; dir < DirCount; dir++)
            {
                int distance = GetDistanceToPlayerFromDir(trainerMapObject, dir, sightRange, playerX, playerZ, 0);
                if (distance != DistanceInvalid && !IsPathInterrupted(trainerMapObject, dir, distance))
                {
                    direction = dir;
                    return distance;
                }
            }
            return DistanceInvalid;
        }
        return DistanceInvalid;
    }

    // OFFSET 0x02067D58
    public static int MapObject_GetDistanceToPlayer(MapObject mapObject, PlayerAvatar playerAvatar, int direction, int range)
    {
        int playerX = PlayerAvatar.PlayerAvatar_GetXPos(playerAvatar);
        int playerZ = PlayerAvatar.PlayerAvatar_GetZPos(playerAvatar);
        int distance = GetDistanceToPlayerFromDir(mapObject, direction, range, playerX, playerZ, 0);
        if (distance != DistanceInvalid && IsPathInterrupted(mapObject, direction, distance))
        {
            distance = DistanceInvalid;
        }
        return distance;
    }

    // OFFSET 0x02067DA8
    private static int GetDistanceToPlayerFromDir(MapObject mapObject, int direction, int range, int playerX, int playerZ, int unused)
    {
        return sDistanceFuncs[direction](mapObject, range, playerX, playerZ, unused);
    }

    // OFFSET 0x02067DC4
    private static int GetDistanceNorth(MapObject mapObject, int range, int playerX, int playerZ, int unused)
    {
        if (MapObject.MapObject_GetX(mapObject) == playerX)
        {
            int z = MapObject.MapObject_GetZ(mapObject);
            if (playerZ < z && playerZ >= z - range)
            {
                return z - playerZ;
            }
        }
        return DistanceInvalid;
    }

    // OFFSET 0x02067DF0
    private static int GetDistanceSouth(MapObject mapObject, int range, int playerX, int playerZ, int unused)
    {
        if (MapObject.MapObject_GetX(mapObject) == playerX)
        {
            int z = MapObject.MapObject_GetZ(mapObject);
            if (playerZ > z && playerZ <= z + range)
            {
                return playerZ - z;
            }
        }
        return DistanceInvalid;
    }

    // OFFSET 0x02067E1C
    private static int GetDistanceWest(MapObject mapObject, int range, int playerX, int playerZ, int unused)
    {
        if (MapObject.MapObject_GetZ(mapObject) == playerZ)
        {
            int x = MapObject.MapObject_GetX(mapObject);
            if (playerX < x && playerX >= x - range)
            {
                return x - playerX;
            }
        }
        return DistanceInvalid;
    }

    // OFFSET 0x02067E48
    private static int GetDistanceEast(MapObject mapObject, int range, int playerX, int playerZ, int unused)
    {
        if (MapObject.MapObject_GetZ(mapObject) == playerZ)
        {
            int x = MapObject.MapObject_GetX(mapObject);
            if (playerX > x && playerX <= x + range)
            {
                return playerX - x;
            }
        }
        return DistanceInvalid;
    }

    // OFFSET 0x02067E74
    private static bool IsPathInterrupted(MapObject mapObject, int direction, int distance)
    {
        if (distance == 0)
        {
            return true;
        }
        int x = MapObject.MapObject_GetX(mapObject);
        int z = MapObject.MapObject_GetZ(mapObject);
        int y = MapObject.MapObject_GetY(mapObject);
        x += MapObjectMove.MapObject_GetDxFromDir(direction);
        z += MapObjectMove.MapObject_GetDzFromDir(direction);
        for (int i = 0; i < distance - 1; i++)
        {
            uint flags = MapObjectMove.MapObject_CheckCollisionAtCoords(mapObject, x, y, z, direction);
            if ((flags & ~CollisionIgnoredMask) != 0)
            {
                return true;
            }
            x += MapObjectMove.MapObject_GetDxFromDir(direction);
            z += MapObjectMove.MapObject_GetDzFromDir(direction);
        }
        uint finalFlags = MapObjectMove.MapObject_CheckCollisionAtCoords(mapObject, x, y, z, direction) & ~CollisionIgnoredMask;
        if (finalFlags == CollisionObject)
        {
            return false;
        }
        return true;
    }

    // OFFSET 0x02067F14
    public static int MapObject_GetTrainerID(MapObject mapObject)
    {
        return FieldSystemVarsFlags.Script_GetTrainerID((ushort)MapObject.MapObject_GetScript(mapObject));
    }

    // OFFSET 0x02067F24
    public static int MapObject_GetTrainerIDFromScript(MapObject mapObject)
    {
        return MapObject_GetTrainerID(mapObject);
    }

    // OFFSET 0x02067F2C
    private static MapObject? FindTrainerPartner(FieldSystem fieldSystem, MapObjectManager mapObjectManager, MapObject trainerMapObject, int trainerID)
    {
        int index = 0;
        while (MapObjectManager.MapObjectMan_FindObjectWithStatus(mapObjectManager, out MapObject? mapObject, ref index, StatusActive) != 0)
        {
            if (mapObject == trainerMapObject)
            {
                continue;
            }
            int trainerType = GetTrainerType(mapObject!);
            if (trainerType != TrainerTypeNormal && trainerType != TrainerTypeViewAllDirections)
            {
                continue;
            }
            if (trainerID == MapObject_GetTrainerID(mapObject!))
            {
                return mapObject;
            }
        }
        ErrorHandling.GF_AssertFail();
        return null;
    }

    // OFFSET 0x02067F88
    public static int FieldSystem_IsUndefeatedTrainer(FieldSystem fieldSystem, MapObject mapObject)
    {
        int trainerType = GetTrainerType(mapObject);
        if (trainerType == TrainerTypeNormal || trainerType == TrainerTypeViewAllDirections)
        {
            if (!FieldSystemVarsFlags.Script_IsTrainerDefeated(fieldSystem, (ushort)MapObject_GetTrainerID(mapObject)))
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x02067FB8
    public static SysTask TrainerEncounter_StartApproachingTrainerTask(FieldSystem fieldSystem, MapObject mapObject, PlayerAvatar playerAvatar, int direction, int sightRange, int unused, int approachType, int approachNum)
    {
        return StartApproachingTrainerTask(fieldSystem, mapObject, playerAvatar, direction, sightRange, unused, approachType, approachNum);
    }

    // OFFSET 0x02067FD4
    public static int TrainerEncounter_IsApproachingTrainerTaskDone(SysTask? task)
    {
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return IsApproachingTrainerTaskDone(task!);
    }

    // OFFSET 0x02067FE8
    public static void TrainerEncounter_FreeApproachingTrainerTask(SysTask task)
    {
        FreeApproachingTrainerTask(task);
    }

    // OFFSET 0x02067FF0
    private static SysTask StartApproachingTrainerTask(FieldSystem fieldSystem, MapObject mapObject, PlayerAvatar playerAvatar, int direction, int sightRange, int unused, int approachType, int approachNum)
    {
        ApproachingTrainerData data = new();
        data.Direction = direction;
        data.SightRange = sightRange;
        data.Unused = unused;
        data.ApproachType = approachType;
        data.ApproachNum = approachNum;
        data.FieldSystem = fieldSystem;
        data.MapObject = mapObject;
        data.PlayerAvatar = playerAvatar;
        SysTask? task = SysTask.SysTask_Start(SysTask_ApproachingTrainer, data, TaskPriority);
        if (task == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        return task!;
    }

    // OFFSET 0x02068048
    private static int IsApproachingTrainerTaskDone(SysTask task)
    {
        ApproachingTrainerData data = (ApproachingTrainerData)SysTaskManager.SysTask_GetParam(task)!;
        return data.Done;
    }

    // OFFSET 0x02068054
    private static void FreeApproachingTrainerTask(SysTask task)
    {
        SysTask.SysTask_Done(task);
    }

    // OFFSET 0x0206806C
    private static void SysTask_ApproachingTrainer(SysTask task, object? param)
    {
        ApproachingTrainerData data = (ApproachingTrainerData)param!;
        while (sApproachingTrainerStates[data.State](data) == 1)
        {
        }
    }

    // OFFSET 0x02068088
    private static int ApproachingTrainerTask_UnpauseMovement(ApproachingTrainerData data)
    {
        MapObject mapObject = data.MapObject!;
        if (MapObject.MapObject_IsMoving(mapObject) == 1)
        {
            MapObject.MapObject_SetPauseMovementOff(mapObject);
        }
        data.State = StateTryFaceDirection;
        return 1;
    }

    // OFFSET 0x020680A4
    private static int ApproachingTrainerTask_TryFaceDirection(ApproachingTrainerData data)
    {
        MapObject mapObject = data.MapObject!;
        if (MapObject.MapObject_IsMoving(mapObject) == 1)
        {
            return 0;
        }
        MapObjectRender.MapObject_FaceAndDraw(data.MapObject!, data.Direction);
        MapObject.MapObject_SetStatusFlagOn(mapObject, StatusFaceLocked);
        data.State = StateCheckRevealOrFaceDirection;
        return 1;
    }

    // OFFSET 0x020680D0
    private static int ApproachingTrainerTask_CheckRevealOrFaceDirection(ApproachingTrainerData data)
    {
        MapObject playerObject = PlayerAvatar.PlayerAvatar_GetMapObject(data.PlayerAvatar!);
        if (MovementActions.LocalMapObj_CheckAnimationFinished(playerObject) == 0)
        {
            return 0;
        }
        int movementType = MapObject.MapObject_GetMovementType(data.MapObject!);
        if (movementType >= MovementTypeHiddenFirst && movementType <= MovementTypeHiddenLast)
        {
            data.State = StateRevealTrainer;
            return 1;
        }
        data.State = StateFaceDirection;
        return 1;
    }

    // OFFSET 0x02068118
    private static int ApproachingTrainerTask_FaceDirection(ApproachingTrainerData data)
    {
        if (MovementActions.LocalMapObj_IsAnimationSet(data.MapObject!) == 0)
        {
            return 0;
        }
        if (data.Direction == DirNone)
        {
            ErrorHandling.GF_AssertFail();
        }
        int movementAction = MovementActions.MovementAction_TurnActionTowardsDir(data.Direction, MovementActionFace);
        MovementActions.LocalMapObj_SetAnimationCode(data.MapObject!, movementAction);
        data.State = StateWaitFaceDirection;
        return 0;
    }

    // OFFSET 0x02068150
    private static int ApproachingTrainerTask_WaitFaceDirection(ApproachingTrainerData data)
    {
        if (MovementActions.LocalMapObj_CheckAnimationFinished(data.MapObject!) == 0)
        {
            return 0;
        }
        data.State = StateShowEmote;
        return 1;
    }

    // OFFSET 0x0206816C
    private static int ApproachingTrainerTask_ShowEmote(ApproachingTrainerData data)
    {
        data.AnimManager = EmoteIconEffect.EmoteIconEffect_New(data.MapObject!, 0, 0, 0);
        data.State = StateWaitEmote;
        return 0;
    }

    // OFFSET 0x02068188
    private static int ApproachingTrainerTask_WaitEmote(ApproachingTrainerData data)
    {
        if (EmoteIconEffect.EmoteIconEffect_IsFinished(data.AnimManager!) == 1)
        {
            OverworldAnimManagers.OverworldAnimManager_Finish(data.AnimManager);
            data.State = StateDelayCheckNextToPlayer;
        }
        return 0;
    }

    // OFFSET 0x020681A4
    private static int ApproachingTrainerTask_RevealTrainer(ApproachingTrainerData data)
    {
        MovementActions.LocalMapObj_SetAnimationCode(data.MapObject!, MovementActionRevealTrainer);
        data.State = StateWaitRevealTrainer;
        return 0;
    }

    // OFFSET 0x020681B8
    private static int ApproachingTrainerTask_WaitRevealTrainer(ApproachingTrainerData data)
    {
        if (MovementActions.LocalMapObj_CheckAnimationFinished(data.MapObject!) == 1)
        {
            data.State = StateDelayCheckNextToPlayer;
        }
        return 0;
    }

    // OFFSET 0x020681D0
    private static int ApproachingTrainerTask_DelayCheckNextToPlayer(ApproachingTrainerData data)
    {
        data.Delay++;
        if (data.Delay >= RevealDelayFrames)
        {
            data.Delay = 0;
            data.State = StateCheckNextToPlayer;
        }
        return 0;
    }

    // OFFSET 0x020681E8
    private static int ApproachingTrainerTask_CheckNextToPlayer(ApproachingTrainerData data)
    {
        if (data.SightRange <= 1)
        {
            data.State = StateDelayNextToPlayer;
            return 1;
        }
        data.State = StateStepTowardsPlayer;
        return 1;
    }

    // OFFSET 0x02068200
    private static int ApproachingTrainerTask_StepTowardsPlayer(ApproachingTrainerData data)
    {
        if (MovementActions.LocalMapObj_IsAnimationSet(data.MapObject!) == 1)
        {
            int movementAction = MovementActions.MovementAction_TurnActionTowardsDir(data.Direction, MovementActionWalkNormal);
            MovementActions.LocalMapObj_SetAnimationCode(data.MapObject!, movementAction);
            data.State = StateWaitStepTowardsPlayer;
        }
        return 0;
    }

    // OFFSET 0x02068228
    private static int ApproachingTrainerTask_WaitStepTowardsPlayer(ApproachingTrainerData data)
    {
        if (MovementActions.LocalMapObj_CheckAnimationFinished(data.MapObject!) == 0)
        {
            return 0;
        }
        data.SightRange--;
        data.State = StateCheckNextToPlayer;
        return 1;
    }

    // OFFSET 0x02068248
    private static int ApproachingTrainerTask_DelayNextToPlayer(ApproachingTrainerData data)
    {
        data.Delay++;
        if (data.Delay < NextToPlayerDelayFrames)
        {
            return 0;
        }
        data.Delay = 0;
        data.State = StateTryPlayerFaceTrainer;
        return 1;
    }

    // OFFSET 0x02068264
    private static int ApproachingTrainerTask_TryPlayerFaceTrainer(ApproachingTrainerData data)
    {
        MapObject playerObject = PlayerAvatar.PlayerAvatar_GetMapObject(data.PlayerAvatar!);
        int playerX = MapObject.MapObject_GetX(playerObject);
        int playerZ = MapObject.MapObject_GetZ(playerObject);
        int trainerX = MapObject.MapObject_GetX(data.MapObject!);
        int trainerZ = MapObject.MapObject_GetZ(data.MapObject!);
        int direction = MapObjectMove.GetDirectionBetweenPoints(playerX, playerZ, trainerX, trainerZ);
        if (direction != PlayerAvatar.PlayerAvatar_GetFacingDir(data.PlayerAvatar!)
            && (data.ApproachNum == 0 || data.ApproachType == ApproachTypeVs2))
        {
            if (MovementActions.LocalMapObj_IsAnimationSet(playerObject) == 1)
            {
                MapObject.MapObject_SetStatusFlagOff(playerObject, StatusPauseDirection);
                int movementAction = MovementActions.MovementAction_TurnActionTowardsDir(direction, MovementActionFace);
                MovementActions.LocalMapObj_SetAnimationCode(playerObject, movementAction);
                data.State = StateWaitPlayerFaceTrainer;
            }
        }
        else
        {
            data.State = StateSwitchMovementTypeNone;
        }
        return 0;
    }

    // OFFSET 0x020682E0
    private static int ApproachingTrainerTask_WaitPlayerFaceTrainer(ApproachingTrainerData data)
    {
        MapObject playerObject = PlayerAvatar.PlayerAvatar_GetMapObject(data.PlayerAvatar!);
        if (MovementActions.LocalMapObj_CheckAnimationFinished(playerObject) == 0)
        {
            return 0;
        }
        MovementActions.LocalMapObj_ClearFinishedAnimation(playerObject);
        data.State = StateSwitchMovementTypeNone;
        return 1;
    }

    // OFFSET 0x02068308
    private static int ApproachingTrainerTask_SwitchMovementTypeNone(ApproachingTrainerData data)
    {
        MovementActions.LocalMapObj_ClearFinishedAnimation(data.MapObject!);
        if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(data.FieldSystem!, DynamicMapFeaturesHearthomeGym) == 0
            || ov8_0224C5DC(data.FieldSystem!, data.MapObject!) == 0)
        {
            MapObject.MapObject_SwitchMovementType(data.MapObject!, MovementTypeNone);
        }
        data.State = StateDone;
        return 1;
    }

    // OFFSET 0x0206833C
    private static int ApproachingTrainerTask_Done(ApproachingTrainerData data)
    {
        data.Done = 1;
        return 0;
    }
}
