namespace PlatinumSharp.Game;

public sealed class PokemonEncounterTask
{
    public BattleSystem? BattleSys;
    public BattlerData? Battler;
    public PokemonSprite? Sprite;
    public Terrain? TerrainSprite;
    public byte Command;
    public byte BattlerId;
    public byte State;
    public byte Face;
    public short TargetPos;
    public ushort Species;
    public int CryModulation;
    public int BattlerType;
    public int Timer;
    public int Nature;
    public int IsShiny;
    public byte Form;
}

public static partial class Battlers
{
    // OFFSET 0x0225C168
    private static void Battler_CommandPokemonEncounter(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PokemonEncounter(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }
}

public static partial class BattleDisplay
{
    private const int EncounterSlideDelay = 0x1C;
    private const int EnemyTerrainYPos = 0x58;
    private const int PlayerTerrainYPos = 0x88;
    private const int EncounterSlideSpeed = 8;
    private const int EncounterDropSpeed = 4;
    private const int EncounterDropOffset = 0x88;
    private const uint BattleStatusNoEncounterSlide = 0x40;
    private const int MonAttrX = 0;
    private const int MonAttrY = 1;
    private const int ShinyEffectAnim = 0xB;

    // OFFSET 0x0225CBDC
    public static void BattleDisplay_PokemonEncounter(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        PokemonSpriteManager monSpriteMan = BattleSystems.BattleSystem_GetPokemonSpriteManager(battleSys)!;
        BattleSystems.BattleSystem_GetBattleType(battleSys);
        PokemonEncounterTask task = new();
        task.State = 0;
        if ((battler.BattlerType & 1) != 0)
        {
            task.Face = SideEnemyMode;
            task.TerrainSprite = BattleSystems.BattleSystem_GetTerrainSprite(battleSys, 1);
            SpriteSystem.ManagedSprite_SetPositionXY(task.TerrainSprite.ManagedSprite!, TrainerTable(TrainerPositionTable, (battler.BattlerType & 1) * TrainerPositionSize), EnemyTerrainYPos);
        }
        else
        {
            task.Face = 0;
            task.TerrainSprite = BattleSystems.BattleSystem_GetTerrainSprite(battleSys, 0);
            SpriteSystem.ManagedSprite_SetPositionXY(task.TerrainSprite.ManagedSprite!, TrainerTable(TrainerPositionTable, (battler.BattlerType & 1) * TrainerPositionSize), PlayerTerrainYPos);
        }
        int header = data[1];
        int gender = header & 3;
        int shiny = (header >> 2) & 1;
        int form = (header >> 3) & 0x1F;
        ushort species = (ushort)BattleIO.R16(data, 2);
        uint personality = BattleIO.R32(data, 4);
        PokemonSpriteTemplate template = new();
        PokemonSpriteTemplates.BuildPokemonSpriteTemplate(template, species, (byte)gender, task.Face, shiny, form, personality);
        int yOffset = PokemonSpriteMetadata.Pokemon_LoadSpriteYOffset(species, (byte)gender, task.Face, (byte)form, personality);
        PokemonSpriteMetadata.PokemonSprite_LoadYOffset(battler.Narc!, out sbyte spriteYOffset, species);
        PokemonSpriteMetadata.PokemonSprite_LoadXOffsetShadow(battler.Narc!, out sbyte shadowXOffset, species);
        PokemonSpriteMetadata.PokemonSprite_LoadShadowSize(battler.Narc!, out byte shadowSize, species);
        SpriteAnimFrame[] frames = new SpriteAnimFrame[PokemonSpriteMetadata.AnimFrameCount];
        PokemonSpriteMetadata.PokemonSprite_LoadAnimFrames(battler.Narc!, frames, species, battler.BattlerType);
        int x = TrainerTable(TrainerPositionTable, battler.BattlerType * TrainerPositionSize);
        int y = TrainerTable(MonPositionYTable, battler.BattlerType * TrainerPositionSize);
        int z = TrainerTable(MonPositionZTable, battler.BattlerType * TrainerPositionSize);
        battler.MonSprite = BattleDisplay_NewMonSprite(battleSys, monSpriteMan, template, x, y, z, yOffset, spriteYOffset, shadowXOffset, shadowSize, battler.Battler, frames, null);
        task.Sprite = battler.MonSprite;
        if (task.Face == SideEnemyMode)
        {
            PokemonSprite.PokemonSprite_StartFade(task.Sprite, 8, 8, 0, 0);
        }
        if (task.Face == SideEnemyMode && (BattleSystems.BattleSystem_GetBattleStatusMask(battleSys) & BattleStatusNoEncounterSlide) != 0)
        {
            int targetY = PokemonSprite.PokemonSprite_GetAttribute(task.Sprite, MonAttrY);
            PokemonSprite.PokemonSprite_SetAttribute(task.Sprite, MonAttrShadowSize, 0);
            PokemonSprite.PokemonSprite_SetAttribute(task.Sprite, MonAttrX, EnemyTerrainX);
            PokemonSprite.PokemonSprite_SetAttribute(task.Sprite, MonAttrY, targetY - EncounterDropOffset);
            task.TargetPos = (short)targetY;
        }
        else
        {
            task.TargetPos = TrainerTable(TrainerTargetTable, battler.BattlerType * 4);
        }
        task.BattleSys = battleSys;
        task.Battler = battler;
        task.Command = data[0];
        task.BattlerId = battler.Battler;
        task.Species = species;
        task.Form = (byte)form;
        task.CryModulation = (int)BattleIO.R32(data, 8);
        task.BattlerType = battler.BattlerType;
        task.Nature = Pokemon.Personality_GetNature(personality);
        task.IsShiny = shiny;
        if (task.Face == SideEnemyMode && (BattleSystems.BattleSystem_GetBattleStatusMask(battleSys) & BattleStatusNoEncounterSlide) != 0)
        {
            SysTask.SysTask_Start(BattleDisplay_PokemonDropInTask, task, 0);
        }
        else
        {
            SysTask.SysTask_Start(BattleDisplay_PokemonEncounterTask, task, 0);
        }
        Sound.Sound_Set2PokemonCriesAllowed(1);
    }

    private static void BattleDisplay_EncounterCry(PokemonEncounterTask task, int pan)
    {
        PokemonAnimManager animMan = BattleSystem_GetPokemonAnimManager(task.BattleSys!);
        PokemonSpriteMetadata.PokemonSprite_LoadAnim(task.Battler!.Narc!, animMan, task.Sprite!, task.Species, task.Face, 0, task.BattlerId);
        PokemonSpriteMetadata.PokemonSprite_LoadCryDelay(task.Battler.Narc!, out byte cryDelay, task.Species, (ushort)task.BattlerType);
        object? chatotCry = BattleSystem_GetChatotCry(task.BattleSys!, task.BattlerId);
        PokemonSpriteMetadata.Pokemon_PlayDelayedCry(chatotCry, task.CryModulation, task.Species, task.Form, pan, CryVolume, 0, HeapIDBattle, cryDelay);
    }

    private static void BattleDisplay_EncounterShinyCheck(PokemonEncounterTask task, BattleAnimSystem animSys)
    {
        if (PokemonAnimManager.PokemonAnimManager_HasAnimCompleted(BattleSystem_GetPokemonAnimManager(task.BattleSys!), task.BattlerId) != 1 || PokemonSprite.PokemonSprite_IsAnimActive(task.Sprite!))
        {
            return;
        }
        if (task.IsShiny != 0)
        {
            byte[] message = new byte[0x58];
            BattleIO.BattleIO_SetMoveAnimation(task.BattleSys!, null, message, TransferDataBattleEffect, ShinyEffectAnim, task.BattlerId, task.BattlerId, 0);
            BattleDisplay_StartMoveAnimation(task.BattleSys!, task.Battler!, animSys, message);
            task.State = 4;
        }
        else
        {
            task.State = StateDone;
        }
    }

    private static void BattleDisplay_EncounterAnimWait(PokemonEncounterTask task, BattleAnimSystem animSys)
    {
        BattleAnimSystems.BattleAnimSystem_ExecuteScript(animSys);
        if (BattleAnimSystems.BattleAnimSystem_IsMoveActive(animSys) == 0)
        {
            BattleAnimSystems.BattleAnimSystem_FreeScriptData(animSys);
            task.State = StateDone;
        }
    }

    private static void BattleDisplay_EncounterFinish(SysTask sysTask, PokemonEncounterTask task)
    {
        Sound.Sound_Set2PokemonCriesAllowed(0);
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.BattlerId, task.Command);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x0225E4E8
    private static void BattleDisplay_PokemonEncounterTask(SysTask sysTask, object? param)
    {
        PokemonEncounterTask task = (PokemonEncounterTask)param!;
        BattleAnimSystem animSys = BattleSystems.BattleSystem_GetBattleAnimSystem(task.BattleSys!)!;
        ManagedSprite terrain = task.TerrainSprite!.ManagedSprite!;
        short terrainX;
        short terrainY;
        switch (task.State)
        {
            case 0:
                task.Timer = EncounterSlideDelay;
                task.State++;
                goto case 1;
            case 1:
                if (--task.Timer != 0)
                {
                    break;
                }
                task.State++;
                goto case 2;
            case 2:
                SpriteSystem.ManagedSprite_GetPositionXY(terrain, out terrainX, out terrainY);
                if (task.Face == SideEnemyMode)
                {
                    if (task.BattlerType == 1 || task.BattlerType == 3)
                    {
                        if (terrainX < EnemyTerrainX)
                        {
                            SpriteSystem.ManagedSprite_OffsetPositionXY(terrain, EncounterSlideSpeed, 0);
                        }
                        else
                        {
                            SpriteSystem.ManagedSprite_SetPositionXY(terrain, EnemyTerrainX, EnemyTerrainYPos);
                        }
                    }
                    SpriteSystem.ManagedSprite_GetPositionXY(terrain, out terrainX, out terrainY);
                    if (task.BattlerType == 1)
                    {
                        PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonAttrX, terrainX);
                    }
                    else if (task.BattlerType == 3)
                    {
                        terrainX = (short)(PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonAttrX) - terrainX);
                        terrainX -= 0x18;
                        PokemonSprite.PokemonSprite_AddAttribute(task.Sprite!, MonAttrX, -terrainX);
                    }
                    else if (task.BattlerType == 5)
                    {
                        terrainX = (short)(terrainX - PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonAttrX));
                        terrainX -= 0x10;
                        PokemonSprite.PokemonSprite_AddAttribute(task.Sprite!, MonAttrX, terrainX);
                    }
                    if (PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonAttrX) < task.TargetPos)
                    {
                        break;
                    }
                    PokemonSprite.PokemonSprite_InitAnim(task.Sprite!, 1);
                    PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonAttrShadowIsAffine, 0);
                    PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonAttrShadowShouldFollowY, 0);
                    PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonAttrX, task.TargetPos);
                    BattleDisplay_EncounterCry(task, PanEnemy);
                    if (task.BattlerType == 1 || task.BattlerType == 3)
                    {
                        SpriteSystem.ManagedSprite_SetPositionXY(terrain, EnemyTerrainX, EnemyTerrainYPos);
                    }
                    PokemonSprite.PokemonSprite_StartFade(task.Sprite!, 8, 0, 0, 0);
                    task.State++;
                    break;
                }
                if (task.BattlerType == 0 || task.BattlerType == 2)
                {
                    if (terrainX > PlayerTerrainX)
                    {
                        SpriteSystem.ManagedSprite_OffsetPositionXY(terrain, -EncounterSlideSpeed, 0);
                    }
                    else
                    {
                        SpriteSystem.ManagedSprite_SetPositionXY(terrain, PlayerTerrainX, PlayerTerrainYPos);
                    }
                }
                SpriteSystem.ManagedSprite_GetPositionXY(terrain, out terrainX, out terrainY);
                if (task.BattlerType == 0)
                {
                    PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonAttrX, terrainX);
                }
                else if (task.BattlerType == 2)
                {
                    terrainX = (short)(terrainX - PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonAttrX));
                    terrainX -= 0x18;
                    PokemonSprite.PokemonSprite_AddAttribute(task.Sprite!, MonAttrX, terrainX);
                }
                else if (task.BattlerType == 4)
                {
                    terrainX = (short)(PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonAttrX) - terrainX);
                    terrainX -= 0x10;
                    PokemonSprite.PokemonSprite_AddAttribute(task.Sprite!, MonAttrX, -terrainX);
                }
                if (PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonAttrX) > task.TargetPos)
                {
                    break;
                }
                PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonAttrX, task.TargetPos);
                BattleDisplay_EncounterCry(task, PanPlayer);
                if (task.BattlerType == 0 || task.BattlerType == 2)
                {
                    SpriteSystem.ManagedSprite_SetPositionXY(terrain, PlayerTerrainX, PlayerTerrainYPos);
                }
                task.State++;
                break;
            case 3:
                BattleDisplay_EncounterShinyCheck(task, animSys);
                break;
            case 4:
                BattleDisplay_EncounterAnimWait(task, animSys);
                break;
            default:
                BattleDisplay_EncounterFinish(sysTask, task);
                break;
        }
    }

    // OFFSET 0x0225E894
    private static void BattleDisplay_PokemonDropInTask(SysTask sysTask, object? param)
    {
        PokemonEncounterTask task = (PokemonEncounterTask)param!;
        BattleAnimSystem animSys = BattleSystems.BattleSystem_GetBattleAnimSystem(task.BattleSys!)!;
        ManagedSprite terrain = task.TerrainSprite!.ManagedSprite!;
        short terrainX;
        short terrainY;
        switch (task.State)
        {
            case 0:
                task.Timer = EncounterSlideDelay;
                task.State++;
                goto case 1;
            case 1:
                if (--task.Timer != 0)
                {
                    break;
                }
                task.State++;
                goto case 2;
            case 2:
                SpriteSystem.ManagedSprite_GetPositionXY(terrain, out terrainX, out terrainY);
                if (task.BattlerType == 1 || task.BattlerType == 3)
                {
                    if (terrainX < EnemyTerrainX)
                    {
                        SpriteSystem.ManagedSprite_OffsetPositionXY(terrain, EncounterSlideSpeed, 0);
                    }
                    else
                    {
                        SpriteSystem.ManagedSprite_SetPositionXY(terrain, EnemyTerrainX, EnemyTerrainYPos);
                    }
                }
                SpriteSystem.ManagedSprite_GetPositionXY(terrain, out terrainX, out terrainY);
                PokemonSprite.PokemonSprite_AddAttribute(task.Sprite!, MonAttrY, EncounterDropSpeed);
                if (PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonAttrY) < task.TargetPos)
                {
                    break;
                }
                PokemonSprite.PokemonSprite_InitAnim(task.Sprite!, 1);
                PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonAttrShadowIsAffine, 0);
                PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonAttrShadowShouldFollowY, 0);
                PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonAttrY, task.TargetPos);
                BattleDisplay_EncounterCry(task, PanEnemy);
                SpriteSystem.ManagedSprite_SetPositionXY(terrain, EnemyTerrainX, EnemyTerrainYPos);
                PokemonSprite.PokemonSprite_StartFade(task.Sprite!, 8, 0, 0, 0);
                task.State++;
                break;
            case 3:
                BattleDisplay_EncounterShinyCheck(task, animSys);
                break;
            case 4:
                BattleDisplay_EncounterAnimWait(task, animSys);
                break;
            default:
                BattleDisplay_EncounterFinish(sysTask, task);
                break;
        }
    }
}
