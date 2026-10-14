namespace PlatinumSharp.Game;

public sealed class SlideTrainerOutTask
{
    public BattleSystem? BattleSys;
    public BattlerData? Battler;
    public byte Command;
    public byte BattlerId;
    public byte State;
    public byte Side;
}

public sealed class SlideTrainerInTask
{
    public BattleSystem? BattleSys;
    public ManagedSprite? Sprite;
    public byte Command;
    public byte Battler;
    public byte State;
    public byte Side;
    public short TargetX;
}

public static partial class BattleDisplay
{
    private const uint TrainerSlideInPositionTable = 0x0223B0DC;
    private const int TrainerSlideSpeed = 5;
    private const int TrainerSlideOutRightX = 0x128;
    private const int TrainerSlideOutLeftX = -0x28;
    private const int TrainerCharResourceBase = 0x4E2F;
    private const int TrainerPlttResourceBase = 0x4E2A;
    private const int TrainerCellResourceBase = 0x4E27;
    private const int TrainerAnimResourceBase = 0x4E27;

    // OFFSET 0x0225D3CC
    public static void BattleDisplay_SlideTrainerOut(BattleSystem battleSys, BattlerData battler)
    {
        SlideTrainerOutTask task = new();
        task.State = 0;
        task.BattleSys = battleSys;
        task.Command = battler.Data[0];
        task.BattlerId = battler.Battler;
        task.Battler = battler;
        task.Side = (byte)((battler.BattlerType & 1) != 0 ? SideEnemyMode : 0);
        SysTask.SysTask_Start(BattleDisplay_SlideTrainerOutTask, task, 0);
    }

    // OFFSET 0x0225D414
    public static void BattleDisplay_SlideTrainerIn(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        BattleSystems.BattleSystem_GetPokemonSpriteManager(battleSys);
        SlideTrainerInTask task = new();
        task.State = 0;
        task.Side = (byte)((battler.BattlerType & 1) != 0 ? SideEnemyMode : 0);
        int side = battler.BattlerType & 1;
        ManagedSprite sprite = BattleDisplay_NewTrainerSprite(battleSys, side, BattleIO.R16(data, 2), battler.BattlerType, TrainerTable(TrainerSlideInPositionTable, side * TrainerPositionSize), TrainerTable(TrainerSlideInPositionTable, side * TrainerPositionSize + 2));
        battler.ManagedSprite = sprite;
        task.Sprite = sprite;
        task.TargetX = TrainerTable(TrainerTargetTable, side * 4 + (int)BattleIO.R32(data, 4) * 2);
        task.BattleSys = battleSys;
        task.Command = data[0];
        task.Battler = battler.Battler;
        SysTask.SysTask_Start(BattleDisplay_SlideTrainerInTask, task, 0);
    }

    // OFFSET 0x02260284
    private static void BattleDisplay_SlideTrainerOutTask(SysTask sysTask, object? param)
    {
        SlideTrainerOutTask task = (SlideTrainerOutTask)param!;
        short x;
        short y;
        switch (task.State)
        {
            case 0:
                if (task.Side == SideEnemyMode)
                {
                    SpriteSystem.ManagedSprite_OffsetPositionXY(task.Battler!.ManagedSprite!, TrainerSlideSpeed, 0);
                    SpriteSystem.ManagedSprite_GetPositionXY(task.Battler.ManagedSprite!, out x, out y);
                    if (x >= TrainerSlideOutRightX)
                    {
                        task.State++;
                    }
                }
                else
                {
                    SpriteSystem.ManagedSprite_OffsetPositionXY(task.Battler!.ManagedSprite!, -TrainerSlideSpeed, 0);
                    SpriteSystem.ManagedSprite_GetPositionXY(task.Battler.ManagedSprite!, out x, out y);
                    if (x <= TrainerSlideOutLeftX)
                    {
                        task.State++;
                    }
                }
                break;
            case 1:
                SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(task.BattleSys!)!;
                SpriteSystem.Sprite_DeleteAndFreeResources(task.Battler!.ManagedSprite!);
                task.Battler.ManagedSprite = null;
                SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, task.Battler.BattlerType + TrainerCharResourceBase);
                SpriteSystem.SpriteManager_UnloadPlttObjById(spriteMan, task.Battler.BattlerType + TrainerPlttResourceBase);
                SpriteSystem.SpriteManager_UnloadCellObjById(spriteMan, task.Battler.BattlerType + TrainerCellResourceBase);
                SpriteSystem.SpriteManager_UnloadAnimObjById(spriteMan, task.Battler.BattlerType + TrainerAnimResourceBase);
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.BattlerId, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }

    // OFFSET 0x02260384
    private static void BattleDisplay_SlideTrainerInTask(SysTask sysTask, object? param)
    {
        SlideTrainerInTask task = (SlideTrainerInTask)param!;
        switch (task.State)
        {
            case 0:
                SpriteSystem.ManagedSprite_GetPositionXY(task.Sprite!, out short x, out short y);
                if (task.Side == 0)
                {
                    x += TrainerSlideSpeed;
                    if (x >= task.TargetX)
                    {
                        x = task.TargetX;
                        task.State++;
                    }
                }
                else
                {
                    x -= TrainerSlideSpeed;
                    if (x <= task.TargetX)
                    {
                        x = task.TargetX;
                        task.State++;
                    }
                }
                SpriteSystem.ManagedSprite_SetPositionXY(task.Sprite!, x, y);
                break;
            case 1:
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }
}

public static partial class Battlers
{
    // OFFSET 0x0225C218
    private static void Battler_CommandSlideTrainerOut(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_SlideTrainerOut(battleSys, battler);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C228
    private static void Battler_CommandSlideTrainerIn(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_SlideTrainerIn(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }
}
