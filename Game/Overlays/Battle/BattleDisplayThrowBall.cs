namespace PlatinumSharp.Game;

public sealed class TrainerThrowBallTask
{
    public BattleSystem? BattleSys;
    public BattlerData? Battler;
    public byte Command;
    public byte BattlerId;
    public byte State;
    public byte Side;
    public int BackSprite;
    public int ThrowType;
    public int Timer;
    public int BallMode;
}

public static partial class BattleDisplay
{
    private const uint ThrowOffsetTable = 0x0226F22C;
    private const int ThrowOffsetRowSize = 0x18;
    private const short ThrowOffsetNone = 0x7FFF;
    private const int ResourceTrainerTiles = 0x4E2F;
    private const int ResourceTrainerPalette = 0x4E2A;
    private const int ResourceTrainerCell = 0x4E27;
    private const int SeqBallThrow = 0x70A;
    private const int BallModeThrow = 0;
    private const int BallModeDrop = 3;
    private const int BallTypeSafariThrow = 0xF;
    private const int BallTypeBait = 0xC;
    private const int BallTypeRock = 0xD;
    private const int BallIDSafari = 5;
    private const int BallIDBait = 0x111;
    private const int BallIDRock = 0x112;
    private const int BallIDPark = 0x110;

    private static short ThrowOffset(int backSprite, int frame, int axis)
    {
        return (short)Nitro.Bin.U16(RomData.Overlay(16, (uint)(ThrowOffsetTable + backSprite * ThrowOffsetRowSize + frame * 4 + axis * 2), 2), 0);
    }

    // OFFSET 0x0225D360
    public static void BattleDisplay_ThrowTrainerBall(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        TrainerThrowBallTask task = new();
        task.State = 0;
        task.BattleSys = battleSys;
        task.Command = data[0];
        task.ThrowType = data[1];
        task.BattlerId = battler.Battler;
        task.Battler = battler;
        if ((battler.BattlerType & 1) != 0)
        {
            task.Side = SideEnemyMode;
            task.BackSprite = 0;
        }
        else
        {
            byte[] trainer = BattleSystems.BattleSystem_GetTrainer(battleSys, battler.Battler);
            task.Side = 0;
            task.BackSprite = TrainerSprites.TrainerClass_GetBackSpriteIndex(BattleDisplay_GetTrainerClass(battleSys, battler.Battler, trainer[1]));
        }
        SysTask.SysTask_Start(BattleDisplay_ThrowTrainerBallTask, task, 0);
    }

    private static void BattleDisplay_DeleteTrainerSprite(TrainerThrowBallTask task)
    {
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(task.BattleSys!)!;
        SpriteSystem.Sprite_DeleteAndFreeResources(task.Battler!.ManagedSprite!);
        task.Battler.ManagedSprite = null;
        SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, task.Battler.BattlerType + ResourceTrainerTiles);
        SpriteSystem.SpriteManager_UnloadPlttObjById(spriteMan, task.Battler.BattlerType + ResourceTrainerPalette);
        SpriteSystem.SpriteManager_UnloadCellObjById(spriteMan, task.Battler.BattlerType + ResourceTrainerCell);
        SpriteSystem.SpriteManager_UnloadAnimObjById(spriteMan, task.Battler.BattlerType + ResourceTrainerCell);
    }

    // OFFSET 0x0225FD5C
    private static void BattleDisplay_ThrowTrainerBallTask(SysTask sysTask, object? param)
    {
        TrainerThrowBallTask task = (TrainerThrowBallTask)param!;
        uint battleType = BattleSystems.BattleSystem_GetBattleType(task.BattleSys!);
        BattlerData battler = task.Battler!;
        short x;
        short y;
        switch (task.State)
        {
            case 0:
            {
                if (task.ThrowType == 0)
                {
                    task.State = 1;
                    break;
                }
                BallThrow template = new();
                switch (task.ThrowType)
                {
                    case 2:
                        template.Type = BallTypeBait;
                        task.BallMode = BallModeThrow;
                        template.BallID = BallIDBait;
                        break;
                    case 3:
                        template.Type = BallTypeRock;
                        task.BallMode = BallModeThrow;
                        template.BallID = BallIDRock;
                        break;
                    case 4:
                        template.Type = BallTypeSafariThrow;
                        task.BallMode = BallModeDrop;
                        template.BallID = BallIDPark;
                        break;
                    default:
                        template.Type = BallTypeSafariThrow;
                        task.BallMode = BallModeDrop;
                        template.BallID = BallIDSafari;
                        break;
                }
                template.HeapID = HeapIDBattle;
                template.Mode = BallRotations.ModeHold;
                template.Target = task.BattlerId;
                template.SpriteSys = BattleSystems.BattleSystem_GetSpriteSystem(task.BattleSys!);
                template.PaletteData = BattleSystems.BattleSystem_GetPaletteData(task.BattleSys!);
                template.Surface = 0;
                template.BattleSys = task.BattleSys;
                battler.BallRotation = BallRotations.BallRotation_New(template);
                BallRotations.BallRotation_SetPriority(battler.BallRotation, 100);
                BallRotations.BallRotation_SetExplicitPriority(battler.BallRotation, 2);
                BallRotations.BallRotation_SetTickAnimation(battler.BallRotation, 0);
                SpriteSystem.ManagedSprite_SetAnimationFrame(battler.ManagedSprite!, 0);
                SpriteSystem.ManagedSprite_SetAnim(battler.ManagedSprite!, 1);
                SpriteSystem.ManagedSprite_SetAnimateFlag(battler.ManagedSprite!, 1);
                task.State = 3;
                break;
            }
            case 1:
                if (task.Side == SideEnemyMode)
                {
                    SpriteSystem.ManagedSprite_OffsetPositionXY(battler.ManagedSprite!, 5, 0);
                    SpriteSystem.ManagedSprite_GetPositionXY(battler.ManagedSprite!, out x, out y);
                    if (x >= 0xA0 && BallRotations.BallRotation_GetMode(battler.BallRotation) != 0)
                    {
                        BallRotations.BallRotation_SetDrawFlag(battler.BallRotation!, 1);
                        BallRotations.BallRotation_SetMode(battler.BallRotation!, BallModeThrow);
                        if ((battleType & BattleTypeDoubles) != 0 && (battleType & BattleTypeMulti) == 0)
                        {
                            BattlerData partner = BattleSystems.BattleSystem_GetBattlerData(task.BattleSys!, BattleSystems.BattleSystem_GetPartner(task.BattleSys!, task.BattlerId))!;
                            BallRotations.BallRotation_SetDrawFlag(partner.BallRotation!, 1);
                            BallRotations.BallRotation_SetMode(partner.BallRotation!, BallModeThrow);
                            BallRotations.BallRotation_SetDelay(partner.BallRotation!, 12);
                        }
                        break;
                    }
                    if (x >= 0x128)
                    {
                        BattleDisplay_DeleteTrainerSprite(task);
                        task.State = 6;
                    }
                }
                else
                {
                    SpriteSystem.ManagedSprite_SetAnimationFrame(battler.ManagedSprite!, 0);
                    SpriteSystem.ManagedSprite_SetAnim(battler.ManagedSprite!, 1);
                    SpriteSystem.ManagedSprite_SetAnimateFlag(battler.ManagedSprite!, 1);
                    task.State = 2;
                }
                break;
            case 2:
                SpriteSystem.ManagedSprite_OffsetPositionXY(battler.ManagedSprite!, -5, 0);
                SpriteSystem.ManagedSprite_GetPositionXY(battler.ManagedSprite!, out x, out y);
                if (battler.BallRotation != null)
                {
                    int frame = SpriteSystem.ManagedSprite_GetAnimationFrame(battler.ManagedSprite!);
                    short offsetX = ThrowOffset(task.BackSprite, frame, 0);
                    if (offsetX != ThrowOffsetNone)
                    {
                        short offsetY = ThrowOffset(task.BackSprite, frame, 1);
                        BallRotations.BallRotation_SetDrawFlag(battler.BallRotation, 1);
                        BallRotations.BallRotation_SetPositionXY(battler.BallRotation, (short)(x + offsetX), (short)(y + offsetY));
                        if (frame == 3 && BallRotations.BallRotation_GetMode(battler.BallRotation) != 0)
                        {
                            BallRotations.BallRotation_SetMode(battler.BallRotation, BallModeThrow);
                            BallRotations.BallRotation_SetTickAnimation(battler.BallRotation, 1);
                            if ((battleType & BattleTypeDoubles) != 0 && (battleType & BattleTypeMulti) == 0)
                            {
                                BattlerData partner = BattleSystems.BattleSystem_GetBattlerData(task.BattleSys!, BattleSystems.BattleSystem_GetPartner(task.BattleSys!, task.BattlerId))!;
                                BallRotations.BallRotation_SetPositionXY(partner.BallRotation!, (short)(x + offsetX), (short)(y + offsetY));
                                BallRotations.BallRotation_SetDrawFlag(partner.BallRotation!, 1);
                                BallRotations.BallRotation_SetMode(partner.BallRotation!, BallModeThrow);
                                BallRotations.BallRotation_SetTickAnimation(partner.BallRotation!, 1);
                            }
                        }
                    }
                }
                if (x <= -40)
                {
                    BattleDisplay_DeleteTrainerSprite(task);
                    task.State = 6;
                }
                break;
            case 3:
            {
                SpriteSystem.ManagedSprite_GetPositionXY(battler.ManagedSprite!, out x, out y);
                int frame = SpriteSystem.ManagedSprite_GetAnimationFrame(battler.ManagedSprite!);
                if (frame == 4)
                {
                    task.Timer = 8;
                    task.State = 4;
                    break;
                }
                if (battler.BallRotation == null)
                {
                    break;
                }
                short offsetX = ThrowOffset(task.BackSprite, frame, 0);
                if (offsetX == ThrowOffsetNone)
                {
                    break;
                }
                BallRotations.BallRotation_SetPositionXY(battler.BallRotation, (short)(x + offsetX), (short)(y + ThrowOffset(task.BackSprite, frame, 1)));
                if (frame != 3)
                {
                    break;
                }
                if (task.BallMode == BallRotations.BallRotation_GetMode(battler.BallRotation))
                {
                    break;
                }
                BallRotations.BallRotation_SetMode(battler.BallRotation, task.BallMode);
                BallRotations.BallRotation_SetExplicitPriority(battler.BallRotation, 1);
                BallRotations.BallRotation_SetTickAnimation(battler.BallRotation, 1);
                if (task.BallMode != BallModeDrop)
                {
                    SoundPlayback.Sound_PlayEffect(SeqBallThrow);
                }
                break;
            }
            case 4:
                task.Timer--;
                if (task.Timer == 0)
                {
                    task.State = (byte)(task.BallMode == BallModeDrop ? 6 : 5);
                }
                break;
            case 5:
                if (BallRotations.BallRotation_IsActive(battler.BallRotation) != 1)
                {
                    BallRotations.BallRotation_Free(battler.BallRotation);
                    battler.BallRotation = null;
                    task.State = 6;
                }
                break;
            case 6:
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.BattlerId, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }
}
