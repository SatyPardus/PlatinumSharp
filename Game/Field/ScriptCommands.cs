namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const uint TableAddress = 0x020EAC58;
    private const uint TableSizeAddress = 0x020EAB80;
    private const uint ConditionTableAddress = 0x020EABA6;
    private const int ConditionTableSize = 18;
    private const int ComparisonLess = 0;
    private const int ComparisonEqual = 1;
    private const int ComparisonGreater = 2;

    private static ScrCmdFunc[]? sTable;
    private static uint[]? sTableAddresses;
    private static Dictionary<uint, ScrCmdFunc>? sPorted;
    private static byte[]? sConditionTable;

    public static ScrCmdFunc[] Table => sTable ??= BuildTable();

    public static uint TableSize => (uint)Table.Length;

    public static uint[] TableAddresses
    {
        get
        {
            if (sTableAddresses == null)
            {
                uint count = RomData.Arm9U32(TableSizeAddress, 1)[0];
                uint[] addresses = RomData.Arm9U32(TableAddress, (int)count);
                for (int i = 0; i < addresses.Length; i++)
                {
                    addresses[i] &= ~1u;
                }
                sTableAddresses = addresses;
            }
            return sTableAddresses;
        }
    }

    public static bool IsPorted(int command)
    {
        return Ported.ContainsKey(TableAddresses[command]);
    }

    private static Dictionary<uint, ScrCmdFunc> Ported
    {
        get
        {
            if (sPorted == null)
            {
                Dictionary<uint, ScrCmdFunc> ported = new();
                RegisterCore(ported);
                RegisterMessage(ported);
                RegisterObjects(ported);
                RegisterSound(ported);
                RegisterStrings(ported);
                RegisterStrings2(ported);
                RegisterDoors(ported);
                RegisterExtra(ported);
                RegisterStory(ported);
                RegisterPlayer(ported);
                RegisterSave(ported);
                RegisterTrainers(ported);
                RegisterParty(ported);
                RegisterMenus(ported);
                RegisterTV(ported);
                sPorted = ported;
            }
            return sPorted;
        }
    }

    private static ScrCmdFunc[] BuildTable()
    {
        uint[] addresses = TableAddresses;
        ScrCmdFunc[] table = new ScrCmdFunc[addresses.Length];
        for (int i = 0; i < table.Length; i++)
        {
            uint address = addresses[i];
            if (Ported.TryGetValue(address, out ScrCmdFunc? function))
            {
                table[i] = function;
            }
            else
            {
                table[i] = _ => Unported.ScriptCommand(address);
            }
        }
        return table;
    }

    private static void RegisterCore(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x0203F6C4] = ScrCmd_Noop;
        ported[0x0203F6C8] = ScrCmd_Dummy;
        ported[0x0203F6CC] = ScrCmd_End;
        ported[0x0203F6D8] = ScrCmd_WaitTime;
        ported[0x0203F734] = ScrCmd_ReadVar;
        ported[0x0203F74C] = ScrCmd_SetDataFromValue;
        ported[0x0203F764] = ScrCmd_SetDataFromWord;
        ported[0x0203F7D0] = ScrCmd_CopyData;
        ported[0x0203F81C] = ScrCmd_CompareDataToData;
        ported[0x0203F84C] = ScrCmd_CompareDataToValue;
        ported[0x0203F900] = ScrCmd_CompareVarToValue;
        ported[0x0203F92C] = ScrCmd_CompareVarToVar;
        ported[0x0203F964] = ScrCmd_StartSubContext;
        ported[0x0203F99C] = ScrCmd_CallCommonScript;
        ported[0x0203FA08] = ScrCmd_ReturnCommonScript;
        ported[0x0203FA1C] = ScrCmd_GoTo;
        ported[0x0203FA34] = ScrCmd_GoToIfTargetObjectIs;
        ported[0x0203FA6C] = ScrCmd_GoToIfTaskValueIs;
        ported[0x0203FA9C] = ScrCmd_GoToIfPlayerDirIs;
        ported[0x0203FAD0] = ScrCmd_Call;
        ported[0x0203FAE8] = ScrCmd_Return;
        ported[0x0203FAF4] = ScrCmd_GoToIf;
        ported[0x0203FB28] = ScrCmd_CallIf;
        ported[0x0203FB5C] = ScrCmd_SetFlag;
        ported[0x0203FB74] = ScrCmd_ClearFlag;
        ported[0x0203FB8C] = ScrCmd_CheckFlag;
        ported[0x0203FBA8] = ScrCmd_CheckFlagFromVar;
        ported[0x0203FBE4] = ScrCmd_SetFlagFromVar;
        ported[0x0203FC0C] = ScrCmd_SetTrainerFlag;
        ported[0x0203FC30] = ScrCmd_ClearTrainerFlag;
        ported[0x0203FC54] = ScrCmd_CheckTrainerFlag;
        ported[0x0203FC7C] = ScrCmd_AddVar;
        ported[0x0203FCAC] = ScrCmd_SubVar;
        ported[0x0203FCDC] = ScrCmd_SetVarFromValue;
        ported[0x0203FD00] = ScrCmd_SetVarFromVar;
        ported[0x0203FD30] = ScrCmd_SetVar;
        ported[0x02044510] = ScrCmd_InitSizeContestRecord;
        ported[0x02048254] = ScrCmd_RandomizeJubilifeLottery;
        ported[0x02045850] = ScrCmd_InitDailyRandomLevel;
    }

    private static ScriptManager Manager(ScriptContext ctx)
    {
        return ScriptManager.FieldSystem_GetScriptManager(ctx.FieldSystem);
    }

    private static BufferView VarPointer(ScriptContext ctx)
    {
        ushort varID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        return FieldSystemVarsFlags.FieldSystem_GetVarPointer(ctx.FieldSystem, varID)!;
    }

    private static ushort VarValue(ScriptContext ctx)
    {
        ushort varID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        return FieldSystemVarsFlags.FieldSystem_TryGetVar(ctx.FieldSystem, varID);
    }

    private static byte Condition(int condition, int comparisonResult)
    {
        sConditionTable ??= RomData.Arm9(ConditionTableAddress, ConditionTableSize);
        return sConditionTable[condition * 3 + comparisonResult];
    }

    // OFFSET 0x0203F6C4
    private static bool ScrCmd_Noop(ScriptContext ctx)
    {
        return false;
    }

    // OFFSET 0x0203F6C8
    private static bool ScrCmd_Dummy(ScriptContext ctx)
    {
        return false;
    }

    // OFFSET 0x0203F6CC
    private static bool ScrCmd_End(ScriptContext ctx)
    {
        ScriptContext.ScriptContext_Stop(ctx);
        return false;
    }

    // OFFSET 0x0203F6D8
    private static bool ScrCmd_WaitTime(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort frames = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort countdownVarID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        FieldSystemVarsFlags.FieldSystem_GetVarPointer(fieldSystem, countdownVarID)!.W16(0, frames);
        ctx.Data[0] = countdownVarID;
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_DecrementTimer);
        return true;
    }

    // OFFSET 0x0203F70C
    private static bool ScriptContext_DecrementTimer(ScriptContext ctx)
    {
        BufferView countdown = FieldSystemVarsFlags.FieldSystem_GetVarPointer(ctx.FieldSystem, (ushort)ctx.Data[0])!;
        countdown.W16(0, countdown.U16(0) - 1);
        if (countdown.U16(0) == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0203F734
    private static bool ScrCmd_ReadVar(ScriptContext ctx)
    {
        VarValue(ctx);
        return false;
    }

    // OFFSET 0x0203F74C
    private static bool ScrCmd_SetDataFromValue(ScriptContext ctx)
    {
        byte index = ScriptContext.ScriptContext_ReadByte(ctx);
        byte value = ScriptContext.ScriptContext_ReadByte(ctx);
        ctx.Data[index] = value;
        return false;
    }

    // OFFSET 0x0203F764
    private static bool ScrCmd_SetDataFromWord(ScriptContext ctx)
    {
        byte index = ScriptContext.ScriptContext_ReadByte(ctx);
        ctx.Data[index] = ScriptContext.ScriptContext_ReadWord(ctx);
        return false;
    }

    // OFFSET 0x0203F7D0
    private static bool ScrCmd_CopyData(ScriptContext ctx)
    {
        byte destination = ScriptContext.ScriptContext_ReadByte(ctx);
        byte source = ScriptContext.ScriptContext_ReadByte(ctx);
        ctx.Data[destination] = ctx.Data[source];
        return false;
    }

    // OFFSET 0x0203F808
    private static byte ScriptContext_Compare(ushort value1, ushort value2)
    {
        if (value1 < value2)
        {
            return ComparisonLess;
        }
        if (value1 == value2)
        {
            return ComparisonEqual;
        }
        return ComparisonGreater;
    }

    // OFFSET 0x0203F81C
    private static bool ScrCmd_CompareDataToData(ScriptContext ctx)
    {
        byte value1 = (byte)ctx.Data[ScriptContext.ScriptContext_ReadByte(ctx)];
        byte value2 = (byte)ctx.Data[ScriptContext.ScriptContext_ReadByte(ctx)];
        ctx.ComparisonResult = ScriptContext_Compare(value1, value2);
        return false;
    }

    // OFFSET 0x0203F84C
    private static bool ScrCmd_CompareDataToValue(ScriptContext ctx)
    {
        byte value1 = (byte)ctx.Data[ScriptContext.ScriptContext_ReadByte(ctx)];
        byte value2 = ScriptContext.ScriptContext_ReadByte(ctx);
        ctx.ComparisonResult = ScriptContext_Compare(value1, value2);
        return false;
    }

    // OFFSET 0x0203F900
    private static bool ScrCmd_CompareVarToValue(ScriptContext ctx)
    {
        ushort value1 = VarPointer(ctx).U16(0);
        ushort value2 = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ctx.ComparisonResult = ScriptContext_Compare(value1, value2);
        return false;
    }

    // OFFSET 0x0203F92C
    private static bool ScrCmd_CompareVarToVar(ScriptContext ctx)
    {
        BufferView var1 = VarPointer(ctx);
        BufferView var2 = VarPointer(ctx);
        ctx.ComparisonResult = ScriptContext_Compare(var1.U16(0), var2.U16(0));
        return false;
    }

    // OFFSET 0x0203F964
    private static bool ScrCmd_StartSubContext(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ScriptManager scriptManager = Manager(ctx);
        ushort scriptID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        scriptManager.Contexts[ScriptManager.ContextSub] = ScriptManager.ScriptContext_CreateAndStart(fieldSystem, scriptID);
        scriptManager.NumActiveContexts++;
        return true;
    }

    // OFFSET 0x0203F99C
    private static bool ScrCmd_CallCommonScript(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ScriptManager scriptManager = Manager(ctx);
        ushort scriptID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        scriptManager.SubContextActive = 1;
        scriptManager.Contexts[ScriptManager.ContextSub] = ScriptManager.ScriptContext_CreateAndStart(fieldSystem, scriptID);
        scriptManager.NumActiveContexts++;
        ScriptContext.ScriptContext_Pause(ctx, ScriptContext_WaitForSubContext);
        return true;
    }

    // OFFSET 0x0203F9EC
    private static bool ScriptContext_WaitForSubContext(ScriptContext ctx)
    {
        if (Manager(ctx).SubContextActive == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0203FA08
    private static bool ScrCmd_ReturnCommonScript(ScriptContext ctx)
    {
        Manager(ctx).SubContextActive = 0;
        return false;
    }

    // OFFSET 0x0203FA1C
    private static bool ScrCmd_GoTo(ScriptContext ctx)
    {
        int offset = (int)ScriptContext.ScriptContext_ReadWord(ctx);
        ScriptContext.ScriptContext_Jump(ctx, ctx.ScriptPtr + offset);
        return false;
    }

    // OFFSET 0x0203FA34
    private static bool ScrCmd_GoToIfTargetObjectIs(ScriptContext ctx)
    {
        ScriptManager scriptManager = Manager(ctx);
        byte localID = ScriptContext.ScriptContext_ReadByte(ctx);
        int offset = (int)ScriptContext.ScriptContext_ReadWord(ctx);
        if (MapObject.MapObject_GetLocalID(scriptManager.TargetObject!) == localID)
        {
            ScriptContext.ScriptContext_Jump(ctx, ctx.ScriptPtr + offset);
        }
        return false;
    }

    // OFFSET 0x0203FA6C
    private static bool ScrCmd_GoToIfTaskValueIs(ScriptContext ctx)
    {
        uint taskValue = FieldTask.FieldTask_GetDummy1CValue(ctx.Task!);
        byte value = ScriptContext.ScriptContext_ReadByte(ctx);
        int offset = (int)ScriptContext.ScriptContext_ReadWord(ctx);
        if (taskValue == value)
        {
            ScriptContext.ScriptContext_Jump(ctx, ctx.ScriptPtr + offset);
        }
        return false;
    }

    // OFFSET 0x0203FA9C
    private static bool ScrCmd_GoToIfPlayerDirIs(ScriptContext ctx)
    {
        ScriptManager scriptManager = Manager(ctx);
        byte direction = ScriptContext.ScriptContext_ReadByte(ctx);
        int offset = (int)ScriptContext.ScriptContext_ReadWord(ctx);
        if (scriptManager.PlayerDir == direction)
        {
            ScriptContext.ScriptContext_Jump(ctx, ctx.ScriptPtr + offset);
        }
        return false;
    }

    // OFFSET 0x0203FAD0
    private static bool ScrCmd_Call(ScriptContext ctx)
    {
        int offset = (int)ScriptContext.ScriptContext_ReadWord(ctx);
        ScriptContext.ScriptContext_Call(ctx, ctx.ScriptPtr + offset);
        return false;
    }

    // OFFSET 0x0203FAE8
    private static bool ScrCmd_Return(ScriptContext ctx)
    {
        ScriptContext.ScriptContext_Return(ctx);
        return false;
    }

    // OFFSET 0x0203FAF4
    private static bool ScrCmd_GoToIf(ScriptContext ctx)
    {
        byte condition = ScriptContext.ScriptContext_ReadByte(ctx);
        int offset = (int)ScriptContext.ScriptContext_ReadWord(ctx);
        if (Condition(condition, ctx.ComparisonResult) == 1)
        {
            ScriptContext.ScriptContext_Jump(ctx, ctx.ScriptPtr + offset);
        }
        return false;
    }

    // OFFSET 0x0203FB28
    private static bool ScrCmd_CallIf(ScriptContext ctx)
    {
        byte condition = ScriptContext.ScriptContext_ReadByte(ctx);
        int offset = (int)ScriptContext.ScriptContext_ReadWord(ctx);
        if (Condition(condition, ctx.ComparisonResult) == 1)
        {
            ScriptContext.ScriptContext_Call(ctx, ctx.ScriptPtr + offset);
        }
        return false;
    }

    // OFFSET 0x0203FB5C
    private static bool ScrCmd_SetFlag(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort flagID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        FieldSystemVarsFlags.FieldSystem_SetFlag(fieldSystem, flagID);
        return false;
    }

    // OFFSET 0x0203FB74
    private static bool ScrCmd_ClearFlag(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort flagID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        FieldSystemVarsFlags.FieldSystem_ClearFlag(fieldSystem, flagID);
        return false;
    }

    // OFFSET 0x0203FB8C
    private static bool ScrCmd_CheckFlag(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort flagID = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ctx.ComparisonResult = (byte)(FieldSystemVarsFlags.FieldSystem_CheckFlag(fieldSystem, flagID) ? 1 : 0);
        return false;
    }

    // OFFSET 0x0203FBA8
    private static bool ScrCmd_CheckFlagFromVar(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        BufferView flagVar = VarPointer(ctx);
        BufferView destination = VarPointer(ctx);
        destination.W16(0, FieldSystemVarsFlags.FieldSystem_CheckFlag(fieldSystem, flagVar.U16(0)) ? 1 : 0);
        return false;
    }

    // OFFSET 0x0203FBE4
    private static bool ScrCmd_SetFlagFromVar(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        BufferView flagVar = VarPointer(ctx);
        FieldSystemVarsFlags.FieldSystem_SetFlag(fieldSystem, flagVar.U16(0));
        return false;
    }

    // OFFSET 0x0203FC0C
    private static bool ScrCmd_SetTrainerFlag(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort trainerID = VarValue(ctx);
        FieldSystemVarsFlags.Script_SetTrainerDefeated(fieldSystem, trainerID);
        return false;
    }

    // OFFSET 0x0203FC30
    private static bool ScrCmd_ClearTrainerFlag(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort trainerID = VarValue(ctx);
        FieldSystemVarsFlags.Script_ClearTrainerDefeated(fieldSystem, trainerID);
        return false;
    }

    // OFFSET 0x0203FC54
    private static bool ScrCmd_CheckTrainerFlag(ScriptContext ctx)
    {
        FieldSystem fieldSystem = ctx.FieldSystem;
        ushort trainerID = VarValue(ctx);
        ctx.ComparisonResult = (byte)(FieldSystemVarsFlags.Script_IsTrainerDefeated(fieldSystem, trainerID) ? 1 : 0);
        return false;
    }

    // OFFSET 0x0203FC7C
    private static bool ScrCmd_AddVar(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        ushort value = VarValue(ctx);
        destination.W16(0, destination.U16(0) + value);
        return false;
    }

    // OFFSET 0x0203FCAC
    private static bool ScrCmd_SubVar(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        ushort value = VarValue(ctx);
        destination.W16(0, destination.U16(0) - value);
        return false;
    }

    // OFFSET 0x0203FCDC
    private static bool ScrCmd_SetVarFromValue(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, ScriptContext.ScriptContext_ReadHalfWord(ctx));
        return false;
    }

    // OFFSET 0x0203FD00
    private static bool ScrCmd_SetVarFromVar(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        BufferView source = VarPointer(ctx);
        destination.W16(0, source.U16(0));
        return false;
    }

    // OFFSET 0x0203FD30
    private static bool ScrCmd_SetVar(ScriptContext ctx)
    {
        BufferView destination = VarPointer(ctx);
        destination.W16(0, VarValue(ctx));
        return false;
    }

    // OFFSET 0x02044510
    private static bool ScrCmd_InitSizeContestRecord(ScriptContext ctx)
    {
        SystemVars.SystemVars_SetSizeContestRecord(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!), 0x8200);
        return false;
    }

    // OFFSET 0x02048254
    private static bool ScrCmd_RandomizeJubilifeLottery(ScriptContext ctx)
    {
        SystemVars.SystemVars_RandomizeJubilifeLotteryTrainerID(VarsFlags.SaveData_GetVarsFlags(ctx.FieldSystem.SaveData!));
        return false;
    }

    // OFFSET 0x02045850
    private static bool ScrCmd_InitDailyRandomLevel(ScriptContext ctx)
    {
        SystemVars.SystemVars_InitDailyRandomLevel(ctx.FieldSystem.SaveData!);
        return false;
    }
}
