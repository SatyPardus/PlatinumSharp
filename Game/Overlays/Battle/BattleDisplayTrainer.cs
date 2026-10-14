namespace PlatinumSharp.Game;

public sealed class TrainerEncounterTask
{
    public BattleSystem? BattleSys;
    public Terrain? TerrainSprite;
    public ManagedSprite? Sprite;
    public byte Command;
    public byte Battler;
    public byte State;
    public byte Side;
    public short TargetX;
    public ushort Flags;
    public int BattlerType;
    public int Timer;
    public int ScrollCount;
}

public sealed class TrainerFlashTask
{
    public int State;
    public int Unused;
}

public static partial class BattleDisplay
{
    private const uint TrainerPositionTable = 0x0223B0B8;
    private const int TrainerPositionSize = 6;
    private const uint TrainerTargetTable = 0x0223B0A0;
    private const uint BattleTypeLink = 4;
    private const uint BattleTypeMulti = 8;
    private const uint BattleTypeTag = 0x10;
    private const int LinkTrainerClassOffset = 0x67;
    private const int SideEnemyMode = 2;
    private const int EnemyTerrainX = 0xC0;
    private const int EnemyTerrainY = 0x58;
    private const int PlayerTerrainX = 0x40;
    private const int PlayerTerrainY = 0x88;
    private const int TerrainSlideSpeed = 8;
    private const int BgLayerMain3 = 3;
    private const byte BgOffsetAddX = 1;
    private const byte BgOffsetSetX = 2;
    private const int TrainerSlideDelay = 0x1C;
    private const int MaxBgScrollSteps = 0x21;
    private const uint AnimAttrFlash = 1;
    private const uint AnimAttrEnd = 0xFFF;
    private const uint AnimAttrCommandMask = 0xF00;
    private const uint AnimAttrSetAnim = 0x100;
    private const int FlashSteps = 4;
    private const int FlashPlanes = 0x3D;

    private static short TrainerTable(uint table, int offset)
    {
        return (short)Nitro.Bin.U16(RomData.Overlay(12, (uint)(table + offset), 2), 0);
    }

    // OFFSET 0x0225D228
    public static void BattleDisplay_TrainerEncounter(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        TrainerEncounterTask task = new();
        task.State = 0;
        int side = battler.BattlerType & 1;
        if (side != 0)
        {
            task.Side = SideEnemyMode;
            task.TerrainSprite = BattleSystems.BattleSystem_GetTerrainSprite(battleSys, 1);
            SpriteSystem.ManagedSprite_SetPositionXY(task.TerrainSprite.ManagedSprite!, TrainerTable(TrainerPositionTable, side * TrainerPositionSize), EnemyTerrainY);
        }
        else
        {
            task.Side = 0;
            task.TerrainSprite = BattleSystems.BattleSystem_GetTerrainSprite(battleSys, 0);
            SpriteSystem.ManagedSprite_SetPositionXY(task.TerrainSprite.ManagedSprite!, TrainerTable(TrainerPositionTable, side * TrainerPositionSize), PlayerTerrainY);
        }
        int index;
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeMulti) != 0 || ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeTag) != 0 && (battler.BattlerType & 1) != 0))
        {
            index = battler.BattlerType;
        }
        else
        {
            index = battler.BattlerType & 1;
        }
        int trainerClass = BattleDisplay_GetTrainerClass(battleSys, battler.Battler, (byte)BattleIO.R16(data, 2));
        BattleIO.W16(data, 2, trainerClass);
        ManagedSprite sprite = BattleDisplay_NewTrainerSprite(battleSys, index, trainerClass, battler.BattlerType, TrainerTable(TrainerPositionTable, index * TrainerPositionSize), TrainerTable(TrainerPositionTable, index * TrainerPositionSize + 2));
        battler.ManagedSprite = sprite;
        task.Sprite = sprite;
        task.TargetX = TrainerTable(TrainerTargetTable, index * 4);
        task.BattleSys = battleSys;
        task.Command = data[0];
        task.Battler = battler.Battler;
        task.BattlerType = battler.BattlerType;
        task.ScrollCount = 0;
        if (task.BattlerType == 0 || task.BattlerType == 2)
        {
            Bg.Bg_SetOffset(BattleSystems.BattleSystem_GetBgConfig(battleSys)!, BgLayerMain3, BgOffsetSetX, 0x84);
        }
        SysTask.SysTask_Start(BattleDisplay_TrainerEncounterTask, task, 0);
    }

    // OFFSET 0x0225FA70
    private static void BattleDisplay_TrainerEncounterTask(SysTask sysTask, object? param)
    {
        TrainerEncounterTask task = (TrainerEncounterTask)param!;
        uint battleType = BattleSystems.BattleSystem_GetBattleType(task.BattleSys!);
        if (task.ScrollCount < MaxBgScrollSteps && task.State >= 2 && (task.BattlerType == 0 || task.BattlerType == 2))
        {
            Bg.Bg_SetOffset(BattleSystems.BattleSystem_GetBgConfig(task.BattleSys!)!, BgLayerMain3, BgOffsetAddX, 4);
            task.ScrollCount++;
        }
        ManagedSprite terrain = task.TerrainSprite!.ManagedSprite!;
        short terrainX;
        short terrainY;
        short spriteX;
        short spriteY;
        switch (task.State)
        {
            case 0:
                if (SpriteSystem.ManagedSprite_GetNumFrames(task.Sprite!) > 2)
                {
                    SpriteSystem.ManagedSprite_SetAnimationFrame(task.Sprite!, 0);
                    SpriteSystem.ManagedSprite_SetAnim(task.Sprite!, 2);
                }
                task.Timer = TrainerSlideDelay;
                task.State = 1;
                goto case 1;
            case 1:
                task.Timer--;
                if (task.Timer != 0)
                {
                    break;
                }
                task.State = 2;
                goto case 2;
            case 2:
                SpriteSystem.ManagedSprite_GetPositionXY(terrain, out terrainX, out terrainY);
                SpriteSystem.ManagedSprite_GetPositionXY(task.Sprite!, out spriteX, out spriteY);
                if (task.Side == SideEnemyMode)
                {
                    if (task.BattlerType == 1 || task.BattlerType == 3)
                    {
                        if (terrainX < EnemyTerrainX)
                        {
                            SpriteSystem.ManagedSprite_OffsetPositionXY(terrain, TerrainSlideSpeed, 0);
                        }
                        else
                        {
                            SpriteSystem.ManagedSprite_SetPositionXY(terrain, EnemyTerrainX, EnemyTerrainY);
                        }
                    }
                    SpriteSystem.ManagedSprite_GetPositionXY(terrain, out terrainX, out terrainY);
                    if (task.BattlerType == 1 || (task.BattlerType == 3 && (battleType == 3 || battleType == 0x83 || battleType == 7)))
                    {
                        SpriteSystem.ManagedSprite_SetPositionXY(task.Sprite!, terrainX, spriteY);
                    }
                    else if (task.BattlerType == 3)
                    {
                        terrainX = (short)(spriteX - terrainX);
                        terrainX -= 0x18;
                        SpriteSystem.ManagedSprite_OffsetPositionXY(task.Sprite!, (short)-terrainX, 0);
                    }
                    else if (task.BattlerType == 5)
                    {
                        terrainX = (short)(terrainX - spriteX);
                        terrainX -= 0x10;
                        SpriteSystem.ManagedSprite_OffsetPositionXY(task.Sprite!, terrainX, 0);
                    }
                    SpriteSystem.ManagedSprite_GetPositionXY(task.Sprite!, out spriteX, out spriteY);
                    if (spriteX < task.TargetX)
                    {
                        break;
                    }
                    SpriteSystem.ManagedSprite_SetPositionXY(task.Sprite!, task.TargetX, spriteY);
                    if (task.BattlerType == 1 || task.BattlerType == 3)
                    {
                        SpriteSystem.ManagedSprite_SetPositionXY(terrain, EnemyTerrainX, EnemyTerrainY);
                    }
                    if (SpriteSystem.ManagedSprite_GetNumFrames(task.Sprite!) > 1)
                    {
                        SpriteSystem.ManagedSprite_SetAnimationFrame(task.Sprite!, 0);
                        SpriteSystem.ManagedSprite_SetAnim(task.Sprite!, 1);
                        task.Flags = (ushort)(task.Flags & ~1);
                        task.State = 3;
                    }
                    else
                    {
                        task.State = 4;
                    }
                    break;
                }
                if (task.BattlerType == 0 || task.BattlerType == 2)
                {
                    if (terrainX > PlayerTerrainX)
                    {
                        SpriteSystem.ManagedSprite_OffsetPositionXY(terrain, -TerrainSlideSpeed, 0);
                    }
                    else
                    {
                        SpriteSystem.ManagedSprite_SetPositionXY(terrain, PlayerTerrainX, PlayerTerrainY);
                    }
                }
                SpriteSystem.ManagedSprite_GetPositionXY(terrain, out terrainX, out terrainY);
                if (task.BattlerType == 0 || (task.BattlerType == 2 && (battleType == 3 || battleType == 0x83 || battleType == 0x13 || battleType == 7)))
                {
                    SpriteSystem.ManagedSprite_SetPositionXY(task.Sprite!, terrainX, spriteY);
                }
                else if (task.BattlerType == 2)
                {
                    terrainX = (short)(terrainX - spriteX);
                    terrainX -= 0x18;
                    SpriteSystem.ManagedSprite_OffsetPositionXY(task.Sprite!, terrainX, 0);
                }
                else if (task.BattlerType == 4)
                {
                    terrainX = (short)(spriteX - terrainX);
                    terrainX -= 0x10;
                    SpriteSystem.ManagedSprite_OffsetPositionXY(task.Sprite!, (short)-terrainX, 0);
                }
                SpriteSystem.ManagedSprite_GetPositionXY(task.Sprite!, out spriteX, out spriteY);
                if (spriteX > task.TargetX)
                {
                    break;
                }
                SpriteSystem.ManagedSprite_SetPositionXY(task.Sprite!, task.TargetX, spriteY);
                if (task.BattlerType == 0 || task.BattlerType == 2)
                {
                    SpriteSystem.ManagedSprite_SetPositionXY(terrain, PlayerTerrainX, PlayerTerrainY);
                }
                task.State = 4;
                break;
            case 3:
                if (SpriteSystem.ManagedSprite_IsAnimated(task.Sprite!))
                {
                    if (BattleDisplay_UpdateTrainerAnim(task, task.Sprite!) == 0)
                    {
                        break;
                    }
                }
                task.State = 4;
                goto case 4;
            case 4:
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }

    // OFFSET 0x022643B8
    public static ManagedSprite BattleDisplay_NewTrainerSprite(BattleSystem battleSys, int index, int trainerClass, int resourceIndex, short x, short y)
    {
        SpriteSystem spriteSys = BattleSystems.BattleSystem_GetSpriteSystem(battleSys)!;
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(battleSys)!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(battleSys)!;
        int face = (index & 1) != 0 ? TrainerSprites.FaceFront : TrainerSprites.FaceBack;
        return TrainerSprites.TrainerSprite_New(spriteSys, spriteMan, paletteData, x, y, trainerClass, face, resourceIndex, HeapIDBattle);
    }

    // OFFSET 0x02264650
    private static int BattleDisplay_UpdateTrainerAnim(TrainerEncounterTask task, ManagedSprite sprite)
    {
        int result = 0;
        uint attr = SpriteSystem.ManagedSprite_GetUserAttrForCurrentAnimFrame(task.Sprite!);
        if (attr == AnimAttrFlash)
        {
            if ((task.Flags & 1) == 0)
            {
                task.Flags = (ushort)((task.Flags & ~1) | 1);
                TrainerFlashTask flash = new();
                flash.State = 0;
                flash.Unused = 0;
                SysTask.SysTask_Start(BattleDisplay_TrainerFlashTask, flash, 0);
            }
        }
        else if (attr == AnimAttrEnd)
        {
            result = 1;
        }
        else if ((attr & AnimAttrCommandMask) == AnimAttrSetAnim)
        {
            uint anim = attr & 0xFF;
            if (anim != 0)
            {
                SpriteSystem.ManagedSprite_SetAnimationFrame(task.Sprite!, 0);
                SpriteSystem.ManagedSprite_SetAnim(task.Sprite!, anim - 1);
                result = 1;
            }
        }
        return result;
    }

    // OFFSET 0x022646C8
    private static void BattleDisplay_TrainerFlashTask(SysTask sysTask, object? param)
    {
        TrainerFlashTask flash = (TrainerFlashTask)param!;
        switch (flash.State)
        {
            case 0:
                BrightnessController.BrightnessController_StartTransition(FlashSteps, 0x10, 0, FlashPlanes, 1);
                flash.State++;
                break;
            case 1:
                if (BrightnessController.BrightnessController_IsTransitionComplete(1))
                {
                    BrightnessController.BrightnessController_StartTransition(FlashSteps, 0, 0x10, FlashPlanes, 1);
                    flash.State++;
                }
                break;
            case 2:
                if (BrightnessController.BrightnessController_IsTransitionComplete(1))
                {
                    SysTask.SysTask_Done(sysTask);
                }
                break;
        }
    }

    // OFFSET 0x02264768
    public static int BattleDisplay_GetTrainerClass(BattleSystem battleSys, int battler, byte trainerClass)
    {
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeLink) != 0)
        {
            if (TrainerInfo.TrainerInfo_GameCode(BattleSystems.BattleSystem_GetTrainerInfo(battleSys, battler)) == 0 && trainerClass <= 1)
            {
                trainerClass = (byte)(trainerClass + LinkTrainerClassOffset);
            }
        }
        return trainerClass;
    }
}
