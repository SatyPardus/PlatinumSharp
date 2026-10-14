using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class PokemonSendOutTask
{
    public BattleSystem? BattleSys;
    public BattlerData? Battler;
    public BallEffect? BallEffect;
    public BattlerObjSprites? ScreenEffect;
    public readonly PokemonSpriteTemplate Template = new();
    public BattleAnimSystem? AnimSys;
    public byte Command;
    public byte BattlerId;
    public byte BattlerType;
    public byte State;
    public byte Face;
    public byte YOffset;
    public ushort Species;
    public int CryModulation;
    public byte PartySlot;
    public byte Nature;
    public ushort Ball;
    public sbyte SpriteYOffset;
    public sbyte ShadowXOffset;
    public byte IsShiny;
    public byte ShadowSize;
    public ushort Unused94;
    public byte Counter;
    public byte Form;
}

public static partial class BattleDisplay
{
    private const uint MonPositionYTable = 0x0223B0BA;
    private const uint MonPositionZTable = 0x0223B0BC;
    private const uint BallColorTable = 0x0226F1AE;
    private const int SeqBallOpen = 0x706;
    private const int PanEnemy = 0x75;
    private const int CryVolume = 0x7F;
    private const uint BattleStatusNoScreenEffect = 0x20;
    private const int MonAttrScaleX = 0xC;
    private const int MonAttrScaleY = 0xD;
    private const int MonAttrHide = 6;
    private const int MonAttrShadowY = 0x14;
    private const int MonAttrShadowXOffset = 0x15;
    private const int MonAttrShadowHeight = 0x16;
    private const int MonAttrShadowYOffset = 0x29;
    private const int MonAttrShadowPlttSlot = 0x2A;
    private const int MonAttrShadowIsAffine = 0x2C;
    private const int MonAttrShadowShouldFollowY = 0x2D;
    private const int MonAttrShadowSize = 0x2E;
    private const int FullScale = 0x100;
    private const int ScaleStep = 0x20;
    private const int StateDone = 0xFF;

    // OFFSET 0x0223EE28
    public static PokemonAnimManager BattleSystem_GetPokemonAnimManager(BattleSystem battleSys)
    {
        return battleSys.MonAnimMan!;
    }

    // OFFSET 0x0223EE30
    public static object? BattleSystem_GetChatotCry(BattleSystem battleSys, int battler)
    {
        if ((battleSys.BattleType & BattleTypeMulti) != 0 || ((battleSys.BattleType & BattleTypeTag) != 0 && (BattleSystems.BattleSystem_BattlerSlot(battleSys, battler) & 1) != 0))
        {
            return battleSys.ChatotCries[battler];
        }
        if ((battleSys.BattleType & BattleTypeDoubles) != 0)
        {
            return battleSys.ChatotCries[battler & 1];
        }
        return battleSys.ChatotCries[battler];
    }

    // OFFSET 0x0223F2B8
    public static byte[]? PokemonSpriteData_GetTiles(PokemonSpriteData[] spriteData, int battler)
    {
        if (battler >= 4)
        {
            ErrorHandling.GF_AssertFail();
        }
        return spriteData[battler].Tiles;
    }

    // OFFSET 0x0223F2CC
    public static void PokemonSpriteData_SetNarcID(PokemonSpriteData[] spriteData, int battler, int narcID)
    {
        if (battler >= 4)
        {
            ErrorHandling.GF_AssertFail();
        }
        spriteData[battler].NarcID = narcID;
    }

    // OFFSET 0x0223F2E4
    public static void PokemonSpriteData_SetPalette(PokemonSpriteData[] spriteData, int battler, int palette)
    {
        if (battler >= 4)
        {
            ErrorHandling.GF_AssertFail();
        }
        spriteData[battler].Palette = palette;
    }

    // OFFSET 0x0223F2FC
    public static void PokemonSpriteData_SetYOffset(PokemonSpriteData[] spriteData, int battler, int yOffset)
    {
        if (battler >= 4)
        {
            ErrorHandling.GF_AssertFail();
        }
        spriteData[battler].YOffset = yOffset;
    }

    // OFFSET 0x02263B10
    public static void Battler_ClearSavedCursor(BattlerData battler)
    {
        battler.SavedCursorPos.Clear();
    }

    // OFFSET 0x02263B20
    public static void Battler_SetTrainerSpriteDrawFlag(BattlerData battler, int draw)
    {
        if (battler.ManagedSprite != null)
        {
            SpriteSystem.ManagedSprite_SetDrawFlag(battler.ManagedSprite, draw);
        }
    }

    // OFFSET 0x02263B30
    public static PokemonSprite BattleDisplay_NewMonSprite(BattleSystem battleSys, PokemonSpriteManager monSpriteMan, PokemonSpriteTemplate template, int x, int y, int z, int yOffset, int shadowYOffset, int shadowXOffset, int shadowSize, int battler, SpriteAnimFrame[]? animFrames, PokemonSpriteCallback? callback)
    {
        PokemonSpriteData[] spriteData = BattleSystems.BattleSystem_GetPokemonSpriteData(battleSys);
        byte[] tiles = PokemonSpriteData_GetTiles(spriteData, battler)!;
        int slot = BattleSystems.BattleSystem_BattlerSlot(battleSys, battler);
        int isEnemy = slot & 1;
        int face = isEnemy != 0 ? PokemonSpriteMetadata.FaceFront : PokemonSpriteMetadata.FaceBack;
        int narcID = (int)(template.Word0 & 0xFFFF);
        int character = (int)(template.Word0 >> 16);
        int palette = (int)(template.Word1 & 0xFFFF);
        int species = (int)(template.Word1 >> 16);
        CharacterSprite.CharacterSprite_LoadPokemonSprite(narcID, character, HeapIDBattle, tiles, template.Word3, false, face, species);
        PokemonSpriteData_SetNarcID(BattleSystems.BattleSystem_GetPokemonSpriteData(battleSys), battler, narcID);
        PokemonSpriteData_SetPalette(BattleSystems.BattleSystem_GetPokemonSpriteData(battleSys), battler, palette);
        PokemonSpriteData_SetYOffset(BattleSystems.BattleSystem_GetPokemonSpriteData(battleSys), battler, yOffset);
        PokemonSprite sprite = PokemonSpriteManager.PokemonSpriteManager_CreateSpriteAtIndex(monSpriteMan, template, x, y + yOffset, z, battler, battler, animFrames, callback);
        if (isEnemy != 0)
        {
            if (slot > 1)
            {
                slot >>= 1;
            }
            PokemonSprite.PokemonSprite_SetAttribute(sprite, MonAttrShadowPlttSlot, slot);
            PokemonSprite.PokemonSprite_SetAttribute(sprite, MonAttrShadowSize, shadowSize);
            PokemonSprite.PokemonSprite_SetAttribute(sprite, MonAttrShadowY, y + 0x24);
            PokemonSprite.PokemonSprite_SetAttribute(sprite, MonAttrShadowXOffset, shadowXOffset);
            PokemonSprite.PokemonSprite_SetAttribute(sprite, MonAttrShadowHeight, 0x24 - yOffset);
            PokemonSprite.PokemonSprite_SetAttribute(sprite, MonAttrShadowYOffset, shadowYOffset);
        }
        return sprite;
    }

    // OFFSET 0x0225CE1C
    public static void BattleDisplay_PokemonSendOut(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        BattleSystems.BattleSystem_GetBattleType(battleSys);
        PokemonSendOutTask task = new();
        int header = data[1];
        int gender = header & 3;
        int shiny = (header >> 2) & 1;
        int form = (header >> 3) & 0x1F;
        ushort species = (ushort)BattleIO.R16(data, 2);
        uint personality = BattleIO.R32(data, 4);
        task.Face = (byte)((battler.BattlerType & 1) != 0 ? PokemonSpriteMetadata.FaceFront : PokemonSpriteMetadata.FaceBack);
        PokemonSpriteTemplates.BuildPokemonSpriteTemplate(task.Template, species, (byte)gender, task.Face, shiny, form, personality);
        task.YOffset = PokemonSpriteMetadata.Pokemon_LoadSpriteYOffset(species, (byte)gender, task.Face, (byte)form, personality);
        PokemonSpriteMetadata.PokemonSprite_LoadYOffset(battler.Narc!, out task.SpriteYOffset, species);
        PokemonSpriteMetadata.PokemonSprite_LoadXOffsetShadow(battler.Narc!, out task.ShadowXOffset, species);
        PokemonSpriteMetadata.PokemonSprite_LoadShadowSize(battler.Narc!, out task.ShadowSize, species);
        Battler_ClearSavedCursor(battler);
        task.BattleSys = battleSys;
        task.Battler = battler;
        task.State = 0;
        task.Counter = 0;
        task.Command = data[0];
        task.BattlerId = battler.Battler;
        task.Species = species;
        task.Form = (byte)form;
        task.BattlerType = battler.BattlerType;
        task.CryModulation = (int)BattleIO.R32(data, 8);
        task.PartySlot = (byte)BattleIO.R32(data, 0xC);
        task.Nature = Pokemon.Personality_GetNature(personality);
        task.Ball = (ushort)BattleIO.R32(data, 0x10);
        task.IsShiny = (byte)shiny;
        task.Unused94 = 0;
        Sound.Sound_Set2PokemonCriesAllowed(1);
        SysTask.SysTask_Start(BattleDisplay_PokemonSendOutTask, task, 0);
    }

    private static int BallColor(int ball)
    {
        return Nitro.Bin.U16(RomData.Overlay(16, (uint)(BallColorTable + ball * 2), 2), 0);
    }

    private static void BattleDisplay_StartSendOutCryAndAnim(PokemonSendOutTask task)
    {
        BattlerData battler = task.Battler!;
        PokemonSprite sprite = battler.MonSprite!;
        PokemonSprite.PokemonSprite_InitAnim(sprite, 1);
        if (task.Face == PokemonSpriteMetadata.FaceFront)
        {
            PokemonSprite.PokemonSprite_SetAttribute(sprite, MonAttrShadowShouldFollowY, 0);
        }
        PokemonSpriteMetadata.PokemonSprite_LoadCryDelay(battler.Narc!, out byte cryDelay, task.Species, task.BattlerType);
        object? chatotCry = BattleSystem_GetChatotCry(task.BattleSys!, task.BattlerId);
        int pan = task.Face == PokemonSpriteMetadata.FaceFront ? PanEnemy : -PanEnemy;
        PokemonSpriteMetadata.Pokemon_PlayDelayedCry(chatotCry, task.CryModulation, task.Species, task.Form, pan, CryVolume, 0, HeapIDBattle, cryDelay);
        PokemonAnimManager animMan = BattleSystem_GetPokemonAnimManager(task.BattleSys!);
        PokemonSpriteMetadata.PokemonSprite_LoadAnim(battler.Narc!, animMan, sprite, task.Species, task.Face, 0, task.BattlerId);
    }

    // OFFSET 0x0225EA80
    private static void BattleDisplay_PokemonSendOutTask(SysTask sysTask, object? param)
    {
        PokemonSendOutTask task = (PokemonSendOutTask)param!;
        BattlerData battler = task.Battler!;
        switch (task.State)
        {
            case 0:
                task.Counter = 0;
                task.ScreenEffect = null;
                if ((BattleSystems.BattleSystem_GetBattleType(task.BattleSys!) & BattleTypeMulti) != 0)
                {
                    if ((BattleSystems.BattleSystem_GetBattleStatusMask(task.BattleSys!) & BattleStatusNoScreenEffect) == 0 && task.BattlerType == 2)
                    {
                        task.ScreenEffect = BattlerObjSpritesFuncs.BattlerObjSprites_NewAll(task.BattleSys!, HeapIDBattle);
                    }
                }
                else if ((BattleSystems.BattleSystem_GetBattleStatusMask(task.BattleSys!) & BattleStatusNoScreenEffect) == 0)
                {
                    if (BattleIO.BattleSystem_IsInitialized(task.BattleSys!) == 1 && task.BattlerType == 2)
                    {
                        task.ScreenEffect = BattlerObjSpritesFuncs.BattlerObjSprites_NewAll(task.BattleSys!, HeapIDBattle);
                    }
                    else if (task.BattlerType == 0)
                    {
                        task.ScreenEffect = BattlerObjSpritesFuncs.BattlerObjSprites_NewAll(task.BattleSys!, HeapIDBattle);
                    }
                }
                task.State++;
                break;
            case 1:
                task.BallEffect = BallEffects.BallEffect_New(HeapIDBattle, task.BattlerType, 0, BattleSystems.BattleSystem_GetPartyPokemon(task.BattleSys!, task.BattlerId, task.PartySlot));
                BallEffects.BallEffect_Load(task.BallEffect);
                task.State++;
                break;
            case 2:
            {
                if (BallRotations.BallRotation_GetMode(battler.BallRotation) != 0)
                {
                    break;
                }
                if (BallEffects.BallEffect_IsLoaded(task.BallEffect!) != 1)
                {
                    break;
                }
                if (BallRotations.BallRotation_IsFinished(battler.BallRotation) != 1)
                {
                    break;
                }
                if (task.BattlerType == 4)
                {
                    task.Counter++;
                    if (task.Counter < 12)
                    {
                        break;
                    }
                    task.Counter = 0;
                }
                PokemonSpriteManager monSpriteMan = BattleSystems.BattleSystem_GetPokemonSpriteManager(task.BattleSys!)!;
                SpriteAnimFrame[] frames = new SpriteAnimFrame[PokemonSpriteMetadata.AnimFrameCount];
                PokemonSpriteMetadata.PokemonSprite_LoadAnimFrames(battler.Narc!, frames, task.Species, task.BattlerType);
                int x = TrainerTable(TrainerTargetTable, task.BattlerType * 4);
                int y = TrainerTable(MonPositionYTable, task.BattlerType * TrainerPositionSize);
                int z = TrainerTable(MonPositionZTable, task.BattlerType * TrainerPositionSize);
                battler.MonSprite = BattleDisplay_NewMonSprite(task.BattleSys!, monSpriteMan, task.Template, x, y, z, task.YOffset, task.SpriteYOffset, task.ShadowXOffset, task.ShadowSize, task.BattlerId, frames, null);
                PokemonSprite.PokemonSprite_SetAttribute(battler.MonSprite, MonAttrScaleX, 0);
                PokemonSprite.PokemonSprite_SetAttribute(battler.MonSprite, MonAttrScaleY, 0);
                PokemonSprite.PokemonSprite_SetAttribute(battler.MonSprite, MonAttrShadowIsAffine, 0);
                PokemonSprite.PokemonSprite_SetAttribute(battler.MonSprite, MonAttrHide, 1);
                PokemonSprite.PokemonSprite_StartFade(battler.MonSprite, 0x10, 0x10, 0, BallColor(task.Ball));
                PokemonSprite.PokemonSprite_SetAttribute(battler.MonSprite, MonAttrHide, 0);
                BallEffects.BallEffect_Start(task.BallEffect!);
                SoundPlayback.Sound_PlayPannedEffect(SeqBallOpen, task.Face == PokemonSpriteMetadata.FaceFront ? PanEnemy : -PanEnemy);
                if (task.ScreenEffect != null)
                {
                    BattlerObjSpritesFuncs.BattlerObjSprites_Free(task.ScreenEffect);
                    task.ScreenEffect = null;
                }
                task.State++;
                break;
            }
            case 3:
                if (BallRotations.BallRotation_IsActive(battler.BallRotation) != 1)
                {
                    task.State++;
                }
                goto case 4;
            case 4:
                if (PokemonSprite.PokemonSprite_GetAttribute(battler.MonSprite!, MonAttrScaleX) == FullScale && BallEffects.BallEffect_IsActive(task.BallEffect!) == 0)
                {
                    BattleDisplay_StartSendOutCryAndAnim(task);
                    PokemonSprite.PokemonSprite_StartFade(battler.MonSprite!, 0x10, 0, 0, BallColor(task.Ball));
                    task.State = 5;
                    break;
                }
                if (PokemonSprite.PokemonSprite_GetAttribute(battler.MonSprite!, MonAttrScaleX) >= FullScale)
                {
                    PokemonSprite.PokemonSprite_SetAttribute(battler.MonSprite!, MonAttrScaleX, FullScale);
                    PokemonSprite.PokemonSprite_SetAttribute(battler.MonSprite!, MonAttrScaleY, FullScale);
                    BattleDisplay_StartSendOutCryAndAnim(task);
                    PokemonSprite.PokemonSprite_StartFade(battler.MonSprite!, 0x10, 0, 1, BallColor(task.Ball));
                    task.State = 5;
                    break;
                }
                PokemonSprite.PokemonSprite_AddAttribute(battler.MonSprite!, MonAttrScaleX, ScaleStep);
                PokemonSprite.PokemonSprite_AddAttribute(battler.MonSprite!, MonAttrScaleY, ScaleStep);
                PokemonSprite.PokemonSprite_CalcScaledYOffset(battler.MonSprite!, task.SpriteYOffset);
                break;
            case 5:
                if (BallEffects.BallEffect_IsActive(task.BallEffect!) != 0)
                {
                    break;
                }
                task.State = 6;
                break;
            case 6:
                if (PokemonAnimManager.PokemonAnimManager_HasAnimCompleted(BattleSystem_GetPokemonAnimManager(task.BattleSys!), task.BattlerId) != 1)
                {
                    break;
                }
                if (PokemonSprite.PokemonSprite_IsAnimActive(battler.MonSprite!))
                {
                    break;
                }
                BallRotations.BallRotation_Free(battler.BallRotation);
                battler.BallRotation = null;
                BallEffects.BallEffect_Free(task.BallEffect!);
                if (task.IsShiny != 0)
                {
                    task.AnimSys = BattleAnimSystems.BattleAnimSystem_New(HeapIDBattle);
                    byte[] message = new byte[0x58];
                    BattleIO.BattleIO_SetMoveAnimation(task.BattleSys!, null, message, TransferDataBattleEffect, ShinyEffectAnim, task.BattlerId, task.BattlerId, 0);
                    BattleDisplay_StartMoveAnimation(task.BattleSys!, battler, task.AnimSys, message);
                    task.State = 7;
                    break;
                }
                task.State = StateDone;
                break;
            case 7:
                BattleAnimSystems.BattleAnimSystem_ExecuteScript(task.AnimSys!);
                if (BattleAnimSystems.BattleAnimSystem_IsMoveActive(task.AnimSys!) == 0)
                {
                    BattleAnimSystems.BattleAnimSystem_FreeScriptData(task.AnimSys!);
                    BattleAnimSystems.BattleAnimSystem_Delete(task.AnimSys);
                    task.State = StateDone;
                }
                break;
            default:
                Sound.Sound_Set2PokemonCriesAllowed(0);
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.BattlerId, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }
}
