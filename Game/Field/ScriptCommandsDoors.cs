namespace PlatinumSharp.Game;

public static partial class ScriptCommands
{
    private const int MapTilesPerBlock = 32;

    private static void RegisterDoors(Dictionary<uint, ScrCmdFunc> ported)
    {
        ported[0x02043F6C] = ScrCmd_LoadDoorAnimation;
        ported[0x02043FC4] = ScrCmd_WaitForAnimation;
        ported[0x02043FDC] = ScrCmd_UnloadAnimation;
        ported[0x02043FF4] = ScrCmd_PlayDoorOpenAnimation;
        ported[0x0204400C] = ScrCmd_PlayDoorCloseAnimation;
    }

    // OFFSET 0x02043F6C
    private static bool ScrCmd_LoadDoorAnimation(ScriptContext ctx)
    {
        ushort blockX = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort blockZ = ScriptContext.ScriptContext_ReadHalfWord(ctx);
        ushort tileX = FieldSystemVarsFlags.FieldSystem_TryGetVar(ctx.FieldSystem, ScriptContext.ScriptContext_ReadHalfWord(ctx));
        ushort tileZ = FieldSystemVarsFlags.FieldSystem_TryGetVar(ctx.FieldSystem, ScriptContext.ScriptContext_ReadHalfWord(ctx));
        byte tag = ScriptContext.ScriptContext_ReadByte(ctx);
        DoorAnimations.FieldSystem_LoadDoorAnimation(ctx.FieldSystem, tileX + blockX * MapTilesPerBlock, tileZ + blockZ * MapTilesPerBlock, tag);
        return false;
    }

    // OFFSET 0x02043FC4
    private static bool ScrCmd_WaitForAnimation(ScriptContext ctx)
    {
        byte tag = ScriptContext.ScriptContext_ReadByte(ctx);
        DoorAnimations.FieldSystem_WaitForDoorAnimation(ctx.FieldSystem, tag);
        return true;
    }

    // OFFSET 0x02043FDC
    private static bool ScrCmd_UnloadAnimation(ScriptContext ctx)
    {
        byte tag = ScriptContext.ScriptContext_ReadByte(ctx);
        DoorAnimations.FieldSystem_UnloadDoorAnimation(ctx.FieldSystem, tag);
        return false;
    }

    // OFFSET 0x02043FF4
    private static bool ScrCmd_PlayDoorOpenAnimation(ScriptContext ctx)
    {
        byte tag = ScriptContext.ScriptContext_ReadByte(ctx);
        DoorAnimations.FieldSystem_PlayDoorOpenAnimation(ctx.FieldSystem, tag);
        return false;
    }

    // OFFSET 0x0204400C
    private static bool ScrCmd_PlayDoorCloseAnimation(ScriptContext ctx)
    {
        byte tag = ScriptContext.ScriptContext_ReadByte(ctx);
        DoorAnimations.FieldSystem_PlayDoorCloseAnimation(ctx.FieldSystem, tag);
        return false;
    }
}
