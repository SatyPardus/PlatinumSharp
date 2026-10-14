using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate int FieldMoveCheckFunc(FieldMoveContext context);

public sealed class FieldMoveScriptData
{
    public const uint Magic = 0x19740205;

    public uint MagicNumber;
    public MapObject? MapObject;
    public FieldTask? Task;
    public ushort PartySlot;
    public ushort FieldMove;
}

public sealed class FieldMovePokemon
{
    public Pokemon? Mon;
    public int Unused04;
}

public static class FieldMoveTasks
{
    private const int HeapIDField = 0x20;
    private const int HeapIDFieldTask = 0xB;
    private const int MapLoadTypeUnion = 2;
    private const int PlayerStateSurfing = 2;
    private const int BadgeCoal = 0;
    private const int BadgeForest = 1;
    private const int BadgeCobble = 2;
    private const int BadgeFen = 3;
    private const int BadgeRelic = 4;
    private const int BadgeMine = 5;
    private const int BadgeIcicle = 6;
    private const int BadgeBeacon = 7;
    private const ushort ScriptCut = 0x2718;
    private const ushort ScriptRockSmash = 0x2719;
    private const ushort ScriptStrength = 0x271A;
    private const ushort ScriptRockClimb = 0x271B;
    private const ushort ScriptSurf = 0x271C;
    private const ushort ScriptWaterfall = 0x271D;
    private const ushort ScriptDefog = 0x271E;
    private const ushort ScriptFlash = 0x271F;
    private const ushort ScriptChatter = 0x22C4;
    private const int DynamicMapFeaturesDistortionWorld = 9;
    private const int JournalMoveDig = 0xA;
    private const int JournalMoveSweetScent = 0xB;
    private const int TownMapContextSize = 0x140;
    private const int TownMapModeFly = 1;

    // OFFSET 0x0207070C
    public static FieldMoveTaskFunc FieldMove_GetTaskFunc(ushort fieldMove)
    {
        return fieldMove switch
        {
            0 => FieldMove_UseCut,
            1 => FieldMove_UseFly,
            2 => FieldMove_UseSurf,
            3 => FieldMove_UseStrength,
            4 => FieldMove_UseDefog,
            5 => FieldMove_UseRockSmash,
            6 => FieldMove_UseWaterfall,
            7 => FieldMove_UseRockClimb,
            8 => FieldMove_UseFlash,
            9 => FieldMove_UseTeleport,
            10 => FieldMove_UseDig,
            11 => FieldMove_UseSweetScent,
            _ => FieldMove_UseChatter,
        };
    }

    public static FieldMoveCheckFunc FieldMove_GetCheckFunc(ushort fieldMove)
    {
        return fieldMove switch
        {
            0 => FieldMove_CheckCut,
            1 => FieldMove_CheckFly,
            2 => FieldMove_CheckSurf,
            3 => FieldMove_CheckStrength,
            4 => FieldMove_CheckDefog,
            5 => FieldMove_CheckRockSmash,
            6 => FieldMove_CheckWaterfall,
            7 => FieldMove_CheckRockClimb,
            8 => FieldMove_CheckFlash,
            9 => FieldMove_CheckTeleport,
            10 => FieldMove_CheckDig,
            11 => FieldMove_CheckSweetScent,
            _ => FieldMove_CheckChatter,
        };
    }

    // OFFSET 0x0207086C
    private static FieldMoveScriptData FieldMoveScriptData_New(FieldMoveTaskContext taskContext, FieldMoveContext moveContext)
    {
        FieldMoveScriptData data = new();
        data.MagicNumber = FieldMoveScriptData.Magic;
        data.MapObject = moveContext.MapObject;
        data.Task = taskContext.Task;
        data.PartySlot = taskContext.SelectedMonSlot;
        data.FieldMove = taskContext.FieldMove;
        return data;
    }

    // OFFSET 0x02070890
    private static void FieldMoveScriptData_Free(FieldMoveScriptData data)
    {
        if (data.MagicNumber != FieldMoveScriptData.Magic)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    private static bool InLinkRoom(FieldMoveContext context)
    {
        return (uint)(context.FieldSystem!.MapLoadType - MapLoadTypeUnion) <= 1;
    }

    private static bool HasBadge(FieldMoveContext context, int badge)
    {
        return TrainerInfo.TrainerInfo_HasBadge(PlayerSave.SaveData_GetTrainerInfo(context.FieldSystem!.SaveData!), badge);
    }

    private static VarsFlags Flags(FieldMoveContext context)
    {
        return VarsFlags.SaveData_GetVarsFlags(context.FieldSystem!.SaveData!);
    }

    private static void StartScriptTask(FieldMoveTaskContext taskContext, FieldMoveContext moveContext, FieldTaskFunc callback)
    {
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(taskContext.Task!)!;
        FieldMoveScriptData data = FieldMoveScriptData_New(taskContext, moveContext);
        FieldSystemCore.FieldSystem_StartFieldMap(moveContext.FieldSystem!);
        menu.Callback = callback;
        menu.TaskData = data;
        menu.State = StartMenu.StateNewTask;
    }

    private static bool RunFieldMoveScript(FieldTask task, ushort scriptID, bool withObject)
    {
        FieldMoveScriptData data = (FieldMoveScriptData)FieldTask.FieldTask_GetEnv(task)!;
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        ScriptManager.ScriptManager_Change(task, scriptID, withObject ? data.MapObject : null);
        ScriptManager.FieldSystem_SetScriptParameters(fieldSystem, data.PartySlot, 0, 0, 0);
        FieldMoveScriptData_Free(data);
        return false;
    }

    // OFFSET 0x020708AC
    private static int FieldMove_CheckCut(FieldMoveContext context)
    {
        if (InLinkRoom(context))
        {
            return 1;
        }
        if (!HasBadge(context, BadgeForest))
        {
            return 2;
        }
        return (context.Flags & FieldMoveContext.FlagCut) != 0 ? 0 : 1;
    }

    // OFFSET 0x020708E0
    private static void FieldMove_UseCut(FieldMoveTaskContext taskContext, FieldMoveContext moveContext)
    {
        StartScriptTask(taskContext, moveContext, FieldMove_CutTask);
    }

    // OFFSET 0x02070914
    private static bool FieldMove_CutTask(FieldTask task)
    {
        return RunFieldMoveScript(task, ScriptCut, true);
    }

    // OFFSET 0x02070950
    private static int FieldMove_CheckFly(FieldMoveContext context)
    {
        if (InLinkRoom(context))
        {
            return 1;
        }
        if (!HasBadge(context, BadgeCobble))
        {
            return 2;
        }
        if (!MapHeader.MapHeader_IsFlyAllowed(context.MapHeaderID))
        {
            return 1;
        }
        if (SystemFlags.SystemFlag_CheckHasPartner(Flags(context)))
        {
            return 3;
        }
        if (SystemFlags.SystemFlag_CheckSafariGameActive(Flags(context)) || SystemFlags.SystemFlag_CheckInPalPark(Flags(context)))
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020709CC
    private static void FieldMove_UseFly(FieldMoveTaskContext taskContext, FieldMoveContext moveContext)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(taskContext.Task!);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(taskContext.Task!)!;
        menu.AdditionalTaskData = (int)taskContext.SelectedMonSlot;
        menu.TaskData = new byte[TownMapContextSize];
        sub_0206B70C(fieldSystem, menu.TaskData, TownMapModeFly);
        sub_0203D884(fieldSystem, menu.TaskData);
        StartMenu.StartMenu_SetCallback(menu, StartMenu.StartMenu_FlyDestinationSelected);
    }

    // OFFSET 0x02070A24
    private static int FieldMove_CheckSurf(FieldMoveContext context)
    {
        if (InLinkRoom(context))
        {
            return 1;
        }
        if (!HasBadge(context, BadgeFen))
        {
            return 2;
        }
        if (PlayerAvatar.PlayerAvatar_GetPlayerState(context.FieldSystem!.PlayerAvatar!) == PlayerStateSurfing)
        {
            return 4;
        }
        if ((context.Flags & FieldMoveContext.FlagSurf) == 0)
        {
            return 1;
        }
        if (SystemFlags.SystemFlag_CheckHasPartner(Flags(context)))
        {
            return 3;
        }
        return 0;
    }

    // OFFSET 0x02070A80
    private static void FieldMove_UseSurf(FieldMoveTaskContext taskContext, FieldMoveContext moveContext)
    {
        StartScriptTask(taskContext, moveContext, FieldMove_SurfTask);
    }

    // OFFSET 0x02070AB4
    private static bool FieldMove_SurfTask(FieldTask task)
    {
        return RunFieldMoveScript(task, ScriptSurf, false);
    }

    // OFFSET 0x02070AF0
    private static int FieldMove_CheckStrength(FieldMoveContext context)
    {
        if (InLinkRoom(context))
        {
            return 1;
        }
        if (!HasBadge(context, BadgeMine))
        {
            return 2;
        }
        return (context.Flags & FieldMoveContext.FlagStrength) != 0 ? 0 : 1;
    }

    // OFFSET 0x02070B28
    private static void FieldMove_UseStrength(FieldMoveTaskContext taskContext, FieldMoveContext moveContext)
    {
        StartScriptTask(taskContext, moveContext, FieldMove_StrengthTask);
    }

    // OFFSET 0x02070B5C
    private static bool FieldMove_StrengthTask(FieldTask task)
    {
        return RunFieldMoveScript(task, ScriptStrength, true);
    }

    // OFFSET 0x02070B98
    private static int FieldMove_CheckDefog(FieldMoveContext context)
    {
        if (InLinkRoom(context))
        {
            return 1;
        }
        if (!HasBadge(context, BadgeRelic))
        {
            return 2;
        }
        return (context.Flags & FieldMoveContext.FlagDefog) != 0 ? 0 : 1;
    }

    // OFFSET 0x02070BD0
    private static void FieldMove_UseDefog(FieldMoveTaskContext taskContext, FieldMoveContext moveContext)
    {
        StartScriptTask(taskContext, moveContext, FieldMove_DefogTask);
    }

    // OFFSET 0x02070C04
    private static bool FieldMove_DefogTask(FieldTask task)
    {
        return RunFieldMoveScript(task, ScriptDefog, false);
    }

    // OFFSET 0x02070C40
    private static int FieldMove_CheckRockSmash(FieldMoveContext context)
    {
        if (InLinkRoom(context))
        {
            return 1;
        }
        if (!HasBadge(context, BadgeCoal))
        {
            return 2;
        }
        if (PlayerAvatar.PlayerAvatar_GetPlayerState(context.FieldSystem!.PlayerAvatar!) == PlayerStateSurfing)
        {
            return 1;
        }
        return (context.Flags & FieldMoveContext.FlagRockSmash) != 0 ? 0 : 1;
    }

    // OFFSET 0x02070C88
    private static void FieldMove_UseRockSmash(FieldMoveTaskContext taskContext, FieldMoveContext moveContext)
    {
        StartScriptTask(taskContext, moveContext, FieldMove_RockSmashTask);
    }

    // OFFSET 0x02070CBC
    private static bool FieldMove_RockSmashTask(FieldTask task)
    {
        return RunFieldMoveScript(task, ScriptRockSmash, true);
    }

    // OFFSET 0x02070CF8
    private static int FieldMove_CheckWaterfall(FieldMoveContext context)
    {
        if (InLinkRoom(context))
        {
            return 1;
        }
        if (!HasBadge(context, BadgeBeacon))
        {
            return 2;
        }
        return (context.Flags & FieldMoveContext.FlagWaterfall) != 0 ? 0 : 1;
    }

    // OFFSET 0x02070D30
    private static void FieldMove_UseWaterfall(FieldMoveTaskContext taskContext, FieldMoveContext moveContext)
    {
        StartScriptTask(taskContext, moveContext, FieldMove_WaterfallTask);
    }

    // OFFSET 0x02070D64
    private static bool FieldMove_WaterfallTask(FieldTask task)
    {
        return RunFieldMoveScript(task, ScriptWaterfall, false);
    }

    // OFFSET 0x02070DA0
    private static int FieldMove_CheckRockClimb(FieldMoveContext context)
    {
        if (InLinkRoom(context))
        {
            return 1;
        }
        if (!HasBadge(context, BadgeIcicle))
        {
            return 2;
        }
        if ((context.Flags & FieldMoveContext.FlagRockClimb) == 0)
        {
            return 1;
        }
        if (SystemFlags.SystemFlag_CheckHasPartner(Flags(context)))
        {
            return 3;
        }
        return 0;
    }

    // OFFSET 0x02070DEC
    private static void FieldMove_UseRockClimb(FieldMoveTaskContext taskContext, FieldMoveContext moveContext)
    {
        StartScriptTask(taskContext, moveContext, FieldMove_RockClimbTask);
    }

    // OFFSET 0x02070E20
    private static bool FieldMove_RockClimbTask(FieldTask task)
    {
        return RunFieldMoveScript(task, ScriptRockClimb, false);
    }

    // OFFSET 0x02070E5C
    private static int FieldMove_CheckFlash(FieldMoveContext context)
    {
        if (InLinkRoom(context))
        {
            return 1;
        }
        return (context.Flags & FieldMoveContext.FlagFlash) != 0 ? 0 : 1;
    }

    // OFFSET 0x02070E7C
    private static void FieldMove_UseFlash(FieldMoveTaskContext taskContext, FieldMoveContext moveContext)
    {
        StartScriptTask(taskContext, moveContext, FieldMove_FlashTask);
    }

    // OFFSET 0x02070EB0
    private static bool FieldMove_FlashTask(FieldTask task)
    {
        return RunFieldMoveScript(task, ScriptFlash, false);
    }

    // OFFSET 0x02070EEC
    private static int FieldMove_CheckTeleport(FieldMoveContext context)
    {
        if (InLinkRoom(context))
        {
            return 1;
        }
        if (!MapHeader.MapHeader_IsTeleportAllowed(context.MapHeaderID))
        {
            return 1;
        }
        if (SystemFlags.SystemFlag_CheckHasPartner(Flags(context)))
        {
            return 3;
        }
        if (SystemFlags.SystemFlag_CheckSafariGameActive(Flags(context)) || SystemFlags.SystemFlag_CheckInPalPark(Flags(context)))
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02070F54
    private static void FieldMove_UseTeleport(FieldMoveTaskContext taskContext, FieldMoveContext moveContext)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(taskContext.Task!);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(taskContext.Task!)!;
        FieldSystemCore.FieldSystem_StartFieldMap(fieldSystem);
        FieldMovePokemon data = FieldMovePokemon_New(HeapIDFieldTask, (byte)taskContext.SelectedMonSlot, fieldSystem.SaveData!);
        menu.Callback = FieldMove_TeleportTask;
        menu.TaskData = data;
        menu.State = StartMenu.StateNewTask;
    }

    // OFFSET 0x02070F94
    private static bool FieldMove_TeleportTask(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        FieldMovePokemon data = (FieldMovePokemon)FieldTask.FieldTask_GetEnv(task)!;
        object? effect = ov6_02247530(fieldSystem, data.Mon!, 4);
        FieldTask.FieldTask_InitJump(task, ov6_02247554, effect);
        return false;
    }

    // OFFSET 0x02070FCC
    private static int FieldMove_CheckDig(FieldMoveContext context)
    {
        if (InLinkRoom(context))
        {
            return 1;
        }
        if (!MapHeader.MapHeader_IsCave(context.MapHeaderID) || !MapHeader.MapHeader_IsEscapeRopeAllowed(context.MapHeaderID))
        {
            return 1;
        }
        if (SystemFlags.SystemFlag_CheckHasPartner(Flags(context)))
        {
            return 3;
        }
        return 0;
    }

    // OFFSET 0x02071010
    private static void FieldMove_UseDig(FieldMoveTaskContext taskContext, FieldMoveContext moveContext)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(taskContext.Task!);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(taskContext.Task!)!;
        FieldSystemCore.FieldSystem_StartFieldMap(fieldSystem);
        FieldMovePokemon data = FieldMovePokemon_New(HeapIDFieldTask, (byte)taskContext.SelectedMonSlot, fieldSystem.SaveData!);
        menu.Callback = FieldMove_DigTask;
        menu.TaskData = data;
        menu.State = StartMenu.StateNewTask;
    }

    // OFFSET 0x02071050
    private static bool FieldMove_DigTask(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        FieldMovePokemon data = (FieldMovePokemon)FieldTask.FieldTask_GetEnv(task)!;
        object? effect = ov6_02247488(fieldSystem, data.Mon!, HeapIDFieldTask);
        JournalLocationEvent entry = Journal.JournalEntry_CreateEventUsedMove(JournalMoveDig, (ushort)fieldSystem.Location!.U32(0), 4);
        Journal.JournalEntry_SaveData(fieldSystem.JournalEntry, entry, 1);
        FieldTask.FieldTask_InitJump(task, ov6_022474AC, effect);
        return false;
    }

    // OFFSET 0x020710A4
    private static int FieldMove_CheckSweetScent(FieldMoveContext context)
    {
        if (InLinkRoom(context))
        {
            return 1;
        }
        if (SystemFlags.SystemFlag_CheckInPalPark(Flags(context)))
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020710D4
    private static void FieldMove_UseSweetScent(FieldMoveTaskContext taskContext, FieldMoveContext moveContext)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(taskContext.Task!);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(taskContext.Task!)!;
        FieldMovePokemon data = FieldMovePokemon_New(HeapIDFieldTask, (byte)taskContext.SelectedMonSlot, fieldSystem.SaveData!);
        FieldSystemCore.FieldSystem_StartFieldMap(fieldSystem);
        menu.Callback = ov5_021F101C;
        menu.TaskData = data;
        menu.State = StartMenu.StateNewTask;
        JournalLocationEvent entry = Journal.JournalEntry_CreateEventUsedMove(JournalMoveSweetScent, (ushort)fieldSystem.Location!.U32(0), HeapIDFieldTask);
        Journal.JournalEntry_SaveData(fieldSystem.JournalEntry, entry, 1);
    }

    // OFFSET 0x02071130
    private static int FieldMove_CheckChatter(FieldMoveContext context)
    {
        if (InLinkRoom(context))
        {
            return 1;
        }
        if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(context.FieldSystem!, DynamicMapFeaturesDistortionWorld) == 1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02071150
    private static void FieldMove_UseChatter(FieldMoveTaskContext taskContext, FieldMoveContext moveContext)
    {
        StartScriptTask(taskContext, moveContext, FieldMove_ChatterTask);
    }

    // OFFSET 0x0207118C
    private static bool FieldMove_ChatterTask(FieldTask task)
    {
        return RunFieldMoveScript(task, ScriptChatter, false);
    }

    // OFFSET 0x020711C8
    private static FieldMovePokemon FieldMovePokemon_New(int heapID, byte partySlot, SaveData saveData)
    {
        FieldMovePokemon data = new();
        data.Mon = Party.Party_GetPokemonBySlotIndex(Party.SaveData_GetParty(saveData), partySlot);
        data.Unused04 = 0;
        return data;
    }
}
