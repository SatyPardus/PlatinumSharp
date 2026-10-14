namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private static void RegisterMoney(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x0204C938] = ScrCmd_RemoveMoney2;
        ported[0x0204C968] = ScrCmd_CheckMoney;
        ported[0x0204C9AC] = ScrCmd_CheckMoney2;
        ported[0x0204C9FC] = ScrCmd_ShowMoney;
        ported[0x0204CA4C] = ScrCmd_HideMoney;
        ported[0x0204CA64] = ScrCmd_UpdateMoneyDisplay;
    }

    // OFFSET 0x0204C938
    private static bool ScrCmd_RemoveMoney2(ScriptContext ctx)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(FieldSystemCore.FieldSystem_GetSaveData(ctx.FieldSystem)!);
        TrainerInfo.TrainerInfo_TakeMoney(trainerInfo, VarValue(ctx));
        return false;
    }

    // OFFSET 0x0204C968
    private static bool ScrCmd_CheckMoney(ScriptContext ctx)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(FieldSystemCore.FieldSystem_GetSaveData(ctx.FieldSystem)!);
        BufferView destination = VarPointer(ctx);
        uint amount = ScriptContext.ScriptContext_ReadWord(ctx);
        destination.W16(0, TrainerInfo.TrainerInfo_Money(trainerInfo) >= amount ? 1 : 0);
        return false;
    }

    // OFFSET 0x0204C9AC
    private static bool ScrCmd_CheckMoney2(ScriptContext ctx)
    {
        TrainerInfo trainerInfo = PlayerSave.SaveData_GetTrainerInfo(FieldSystemCore.FieldSystem_GetSaveData(ctx.FieldSystem)!);
        BufferView destination = VarPointer(ctx);
        uint amount = VarValue(ctx);
        destination.W16(0, TrainerInfo.TrainerInfo_Money(trainerInfo) >= amount ? 1 : 0);
        return false;
    }

    // OFFSET 0x0204C9FC
    private static bool ScrCmd_ShowMoney(ScriptContext ctx)
    {
        ushort tilemapLeft = VarValue(ctx);
        ushort tilemapTop = VarValue(ctx);
        Manager(ctx).MoneyWindow = FieldMenus.FieldMenu_CreateMoneyWindow(ctx.FieldSystem, (byte)tilemapLeft, (byte)tilemapTop);
        return false;
    }

    // OFFSET 0x0204CA4C
    private static bool ScrCmd_HideMoney(ScriptContext ctx)
    {
        FieldMenus.FieldMenu_DeleteMoneyWindow(Manager(ctx).MoneyWindow!);
        return false;
    }

    // OFFSET 0x0204CA64
    private static bool ScrCmd_UpdateMoneyDisplay(ScriptContext ctx)
    {
        FieldMenus.FieldMenu_PrintMoneyToWindow(ctx.FieldSystem, Manager(ctx).MoneyWindow!);
        return false;
    }
}
