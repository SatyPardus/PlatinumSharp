namespace PlatinumSharp.Game;

public sealed class HealthboxLevelUpTask
{
    public HealthBox? Healthbox;
    public LevelUpTask? Owner;
    public byte State;
    public byte PaletteIndex;
    public sbyte Fraction;
}

public static partial class Healthboxes
{
    private const int LevelUpTaskPriority = 1000;
    private const int ResourceHealthboxLevelUpPalette = 0x4E27;
    private const int ResourceHealthboxPalette = 0x4E26;
    private const int LevelUpFlashMax = 10;
    private const int LevelUpFlashStep = 2;
    private const ushort LevelUpFlashColor = 0x73A5;
    private const int VramTypeMain = 1;
    private const int PaletteBufferMainObjects = 2;

    // OFFSET 0x0226834C
    public static void Healthbox_PlayLevelUpAnimation(HealthBox healthbox, LevelUpTask owner)
    {
        owner.Done = 0;
        HealthboxLevelUpTask task = new();
        task.Healthbox = healthbox;
        task.Owner = owner;
        SysTask.SysTask_Start(Healthbox_LevelUpTask, task, LevelUpTaskPriority);
    }

    // OFFSET 0x02268380
    private static void Healthbox_LevelUpTask(SysTask sysTask, object? param)
    {
        HealthboxLevelUpTask task = (HealthboxLevelUpTask)param!;
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(task.Healthbox!.BattleSys!)!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(task.Healthbox.BattleSys!)!;
        switch (task.State)
        {
            case 0:
                {
                    int index = SpriteSystem.SpriteManager_FindPlttResourceOffset(spriteMan, ResourceHealthboxLevelUpPalette, VramTypeMain);
                    SpriteSystem.ManagedSprite_SetExplicitPalette(task.Healthbox.MainSprite!, (uint)index);
                    task.PaletteIndex = (byte)index;
                    task.State++;
                    goto case 1;
                }
            case 1:
                task.Fraction += LevelUpFlashStep;
                if (task.Fraction >= LevelUpFlashMax)
                {
                    task.Fraction = LevelUpFlashMax;
                    task.State++;
                }
                PaletteData.PaletteData_Blend(paletteData, PaletteBufferMainObjects, (ushort)(task.PaletteIndex << 4), 0x10, (byte)task.Fraction, LevelUpFlashColor);
                break;
            case 2:
                task.Fraction -= LevelUpFlashStep;
                if (task.Fraction <= 0)
                {
                    task.Fraction = 0;
                    task.State++;
                }
                PaletteData.PaletteData_Blend(paletteData, PaletteBufferMainObjects, (ushort)(task.PaletteIndex << 4), 0x10, (byte)task.Fraction, LevelUpFlashColor);
                break;
            default:
                {
                    int index = SpriteSystem.SpriteManager_FindPlttResourceOffset(spriteMan, ResourceHealthboxPalette, VramTypeMain);
                    SpriteSystem.ManagedSprite_SetExplicitPalette(task.Healthbox.MainSprite!, (uint)index);
                    task.Owner!.Done = 1;
                    SysTask.SysTask_Done(sysTask);
                    break;
                }
        }
    }
}
