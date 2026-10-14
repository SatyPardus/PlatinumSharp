using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class FieldSystemVarsFlags
{
    private const int VarFirst = 0x4000;
    private const int SpecialVarFirst = 0x8000;
    private const int SpecialVarMemberBias = 0x7FD7;
    private const int VarObjectGraphicsBase = 0x4020;
    private const int ObjectGraphicsVarCount = 0x10;
    private const int LocalFlagFirst = 1;
    private const int LocalFlagBytes = 8;
    private const int LocalVarBytes = 0x40;
    private const int DailyFlagFirst = 0xAA0;
    private const int DailyFlagBytes = 0x18;
    private const int TrainerScriptFirst = 3000;
    private const int TrainerScriptDoubleFirst = 5000;
    private const int FlagTrainerBase = 0x550;
    private const int TrainerParamDoubleBattle = 9;
    private const int HiddenItemScriptFirst = 8000;
    private const int HiddenItemFlagBias = 0x1C66;
    private const int FlagHiddenItemBase = 0x2DA;
    private const int LocationMapIdOffset = 0x00;
    private const uint DailyHiddenItemsByMapAddress = 0x02100AE0;
    private const int DailyHiddenItemsByMapCount = 4;
    private const uint DailyHiddenItemsAddress = 0x02100AD4;
    private const int DailyHiddenItemsCount = 6;
    private const int MapWithoutDailyHiddenItems = 0x100;

    // OFFSET 0x0203F118
    public static BufferView? FieldSystem_GetVarPointer(FieldSystem fieldSystem, ushort varID)
    {
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        if (varID < VarFirst)
        {
            return null;
        }
        if (varID < SpecialVarFirst)
        {
            return new BufferView(varsFlags.Buffer, varsFlags.Offset + VarsFlags.VarsFlags_GetVarOffset(varsFlags, varID));
        }
        return ScriptManager.FieldSystem_GetScriptMemberPtr(fieldSystem, varID - SpecialVarMemberBias);
    }

    // OFFSET 0x0203F150
    public static ushort FieldSystem_TryGetVar(FieldSystem fieldSystem, ushort varID)
    {
        BufferView? var = FieldSystem_GetVarPointer(fieldSystem, varID);
        if (var == null)
        {
            return varID;
        }
        return var.U16(0);
    }

    // OFFSET 0x0203F164
    public static ushort FieldSystem_GetGraphicsID(FieldSystem fieldSystem, ushort graphicsVarID)
    {
        if (graphicsVarID >= ObjectGraphicsVarCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return FieldSystem_TryGetVar(fieldSystem, (ushort)(VarObjectGraphicsBase + graphicsVarID));
    }

    // OFFSET 0x0203F188
    public static bool FieldSystem_CheckFlag(FieldSystem fieldSystem, ushort flagID)
    {
        return VarsFlags.VarsFlags_CheckFlag(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), flagID);
    }

    // OFFSET 0x0203F19C
    public static void FieldSystem_SetFlag(FieldSystem fieldSystem, ushort flagID)
    {
        VarsFlags.VarsFlags_SetFlag(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), flagID);
    }

    // OFFSET 0x0203F1B0
    public static void FieldSystem_ClearFlag(FieldSystem fieldSystem, ushort flagID)
    {
        VarsFlags.VarsFlags_ClearFlag(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), flagID);
    }

    // OFFSET 0x0203F1C4
    public static void FieldSystem_ClearLocalFlags(FieldSystem fieldSystem)
    {
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        VarsFlags.VarsFlags_GetFlagChunk(varsFlags, LocalFlagFirst, out byte[] chunk, out int index);
        for (int i = 0; i < LocalFlagBytes; i++)
        {
            chunk[index + i] = 0;
        }
        varsFlags.Clear(VarsFlags.VarsFlags_GetVarOffset(varsFlags, VarFirst), LocalVarBytes);
    }

    // OFFSET 0x0203F1FC
    public static void FieldSystem_ClearDailyFlags(FieldSystem fieldSystem)
    {
        VarsFlags varsFlags = VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!);
        VarsFlags.VarsFlags_GetFlagChunk(varsFlags, DailyFlagFirst, out byte[] chunk, out int index);
        for (int i = 0; i < DailyFlagBytes; i++)
        {
            chunk[index + i] = 0;
        }
    }

    // OFFSET 0x0203F254
    public static ushort Script_GetTrainerID(ushort scriptID)
    {
        if (scriptID < TrainerScriptDoubleFirst)
        {
            return (ushort)(scriptID - TrainerScriptFirst + 1);
        }
        return (ushort)(scriptID - TrainerScriptDoubleFirst + 1);
    }

    // OFFSET 0x0203F278
    public static int Script_GetTrainerBattlerIndex(ushort scriptID)
    {
        if (scriptID >= TrainerScriptDoubleFirst)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0203F28C
    public static bool Script_IsTrainerDoubleBattle(ushort trainerID)
    {
        if (TrainerData.Trainer_LoadParam(trainerID, TrainerParamDoubleBattle) != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0203F2A0
    public static bool Script_IsTrainerDefeated(FieldSystem fieldSystem, ushort trainerID)
    {
        return VarsFlags.VarsFlags_CheckFlag(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), (ushort)(FlagTrainerBase + trainerID));
    }

    // OFFSET 0x0203F2BC
    public static void Script_SetTrainerDefeated(FieldSystem fieldSystem, ushort trainerID)
    {
        VarsFlags.VarsFlags_SetFlag(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), (ushort)(FlagTrainerBase + trainerID));
    }

    // OFFSET 0x0203F2D8
    public static void Script_ClearTrainerDefeated(FieldSystem fieldSystem, ushort trainerID)
    {
        VarsFlags.VarsFlags_ClearFlag(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!), (ushort)(FlagTrainerBase + trainerID));
    }

    // OFFSET 0x0203F2F4
    public static ushort Script_GetHiddenItemFlag(ushort scriptID)
    {
        return (ushort)(scriptID - HiddenItemFlagBias);
    }

    // OFFSET 0x0203F304
    public static ushort Script_GetHiddenItemScript(ushort scriptID)
    {
        return (ushort)(scriptID - HiddenItemScriptFirst);
    }

    // OFFSET 0x0203F310
    public static void FieldSystem_ClearDailyHiddenItemFlags(FieldSystem fieldSystem)
    {
        ushort[] byMap = RomData.Arm9U16(DailyHiddenItemsByMapAddress, DailyHiddenItemsByMapCount * 2);
        ushort[] anywhere = RomData.Arm9U16(DailyHiddenItemsAddress, DailyHiddenItemsCount);
        for (int i = 0; i < 2; i++)
        {
            int entry = (byte)(MathUtil.LCRNG_Next() & 3);
            if (fieldSystem.Location!.U32(LocationMapIdOffset) != byMap[entry * 2])
            {
                FieldSystem_ClearFlag(fieldSystem, (ushort)(byMap[entry * 2 + 1] + FlagHiddenItemBase));
            }
        }
        if (fieldSystem.Location!.U32(LocationMapIdOffset) != MapWithoutDailyHiddenItems)
        {
            for (int i = 0; i < 2; i++)
            {
                int entry = (byte)(MathUtil.LCRNG_Next() % DailyHiddenItemsCount);
                FieldSystem_ClearFlag(fieldSystem, (ushort)(anywhere[entry] + FlagHiddenItemBase));
            }
        }
    }
}
