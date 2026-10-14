namespace PlatinumSharp.Game;

public sealed class ShowPokemonTask
{
    public BattleSystem? BattleSys;
    public BattlerData? Battler;
    public BallEffect? BallEffect;
    public BallRotation? BallRotation;
    public BattlerObjSprites? ScreenEffect;
    public readonly PokemonSpriteTemplate Template = new();
    public readonly byte[] MonInfo = new byte[0x58];
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
    public ushort DropBall;
    public byte Counter;
    public byte Form;
    public int HasSubstitute;
}

public sealed class ReturnPokemonTask
{
    public BattleSystem? BattleSys;
    public BattlerData? Battler;
    public PokemonSprite? Sprite;
    public BallRotation? BallRotation;
    public readonly byte[] MonInfo = new byte[0x58];
    public byte Command;
    public byte BattlerId;
    public byte BattlerType;
    public byte State;
    public byte YOffset;
    public ushort Ball;
    public int HasSubstitute;
}

public sealed class OpenCaptureBallTask
{
    public BattleSystem? BattleSys;
    public PokemonSprite? Sprite;
    public byte Command;
    public byte Battler;
    public byte BattlerType;
    public byte State;
    public byte YShift;
}

public static partial class Battlers
{
    private const int CommandDeleteMonSprite = 7;

    // OFFSET 0x0225C190
    private static void Battler_CommandShowPokemon(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_ShowPokemon(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C1A4
    private static void Battler_CommandReturnPokemon(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_ReturnPokemon(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C1B8
    private static void Battler_CommandOpenCaptureBall(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_OpenCaptureBall(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C1CC
    private static void Battler_CommandDeleteMonSprite(BattleSystem battleSys, BattlerData battler)
    {
        PokemonSprite.PokemonSprite_Delete(battler.MonSprite!);
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, CommandDeleteMonSprite);
        Battler_ClearCommand(battler);
    }
}

public static partial class BattleDisplay
{
    private const uint BallThrowTypeTable = 0x0226F174;
    private const uint BallReturnTypeTable = 0x0226F17C;
    private const int BallModeShow = 1;
    private const int BallModeReturn = 5;
    private const int SubstituteOutEffect = 0xF;
    private const int SubstituteInEffect = 0x10;
    private const int ScaleShrinkStep = -0x20;

    private static int BallThrowType(int battlerType)
    {
        return RomData.Overlay(16, (uint)(BallThrowTypeTable + battlerType), 1)[0];
    }

    private static int BallReturnType(int battlerType)
    {
        return (int)Nitro.Bin.U32(RomData.Overlay(16, (uint)(BallReturnTypeTable + battlerType * 4), 4), 0);
    }

    private static void CopyMonInfo(byte[] dest, byte[] data, int dataOffset)
    {
        for (int i = 0; i < 4; i++)
        {
            BattleIO.W16(dest, 0x18 + i * 2, BattleIO.R16(data, dataOffset + i * 2));
            dest[0x20 + i] = data[dataOffset + 8 + i];
            dest[0x24 + i] = data[dataOffset + 0xC + i];
            dest[0x28 + i] = data[dataOffset + 0x10 + i];
            BattleIO.W32(dest, 0x2C + i * 4, BattleIO.R32(data, dataOffset + 0x14 + i * 4));
        }
    }

    private static void BattleDisplay_StartSubstituteAnim(BattleSystem battleSys, BattlerData battler, BattleAnimSystem animSys, int battlerId, int effect)
    {
        byte[] message = new byte[0x58];
        BattleIO.BattleIO_SetMoveAnimation(battleSys, null, message, TransferDataBattleEffect, effect, battlerId, battlerId, 0);
        BattleDisplay_StartMoveAnimation(battleSys, battler, animSys, message);
    }

    // OFFSET 0x0225CF70
    public static void BattleDisplay_ShowPokemon(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        BattleSystems.BattleSystem_GetBattleType(battleSys);
        ShowPokemonTask task = new();
        task.State = 0;
        task.Face = (byte)((battler.BattlerType & 1) != 0 ? SideEnemyMode : 0);
        int header = data[1];
        int gender = header & 3;
        int shiny = (header >> 2) & 1;
        int form = (header >> 3) & 0x1F;
        ushort species = (ushort)BattleIO.R16(data, 2);
        uint personality = BattleIO.R32(data, 4);
        PokemonSpriteTemplates.BuildPokemonSpriteTemplate(task.Template, species, (byte)gender, task.Face, shiny, form, personality);
        task.YOffset = PokemonSpriteMetadata.Pokemon_LoadSpriteYOffset(species, (byte)gender, task.Face, (byte)form, personality);
        PokemonSpriteMetadata.PokemonSprite_LoadYOffset(battler.Narc!, out task.SpriteYOffset, species);
        PokemonSpriteMetadata.PokemonSprite_LoadXOffsetShadow(battler.Narc!, out task.ShadowXOffset, species);
        PokemonSpriteMetadata.PokemonSprite_LoadShadowSize(battler.Narc!, out task.ShadowSize, species);
        Battler_ClearSavedCursor(battler);
        task.BattleSys = battleSys;
        task.Battler = battler;
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
        task.DropBall = (ushort)BattleIO.R32(data, 0x14);
        task.Counter = 0;
        task.HasSubstitute = (int)BattleIO.R32(data, 0x4C);
        CopyMonInfo(task.MonInfo, data, 0x50);
        SysTask.SysTask_Start(BattleDisplay_ShowPokemonTask, task, 0);
    }

    private static void BattleDisplay_ShowPokemonCryAndAnim(ShowPokemonTask task)
    {
        BattlerData battler = task.Battler!;
        PokemonSprite sprite = battler.MonSprite!;
        int pan;
        PokemonSprite.PokemonSprite_InitAnim(sprite, 1);
        if (task.Face == SideEnemyMode)
        {
            PokemonSprite.PokemonSprite_SetAttribute(sprite, MonAttrShadowShouldFollowY, 0);
            pan = PanEnemy;
        }
        else
        {
            pan = PanPlayer;
        }
        PokemonSpriteMetadata.PokemonSprite_LoadCryDelay(battler.Narc!, out byte cryDelay, task.Species, task.BattlerType);
        object? chatotCry = BattleSystem_GetChatotCry(task.BattleSys!, task.BattlerId);
        PokemonSpriteMetadata.Pokemon_PlayDelayedCry(chatotCry, task.CryModulation, task.Species, task.Form, pan, CryVolume, 0, HeapIDBattle, cryDelay);
        PokemonSpriteMetadata.PokemonSprite_LoadAnim(battler.Narc!, BattleSystem_GetPokemonAnimManager(task.BattleSys!), sprite, task.Species, task.Face, 0, task.BattlerId);
    }

    // OFFSET 0x0225F0C0
    private static void BattleDisplay_ShowPokemonTask(SysTask sysTask, object? param)
    {
        ShowPokemonTask task = (ShowPokemonTask)param!;
        BattleAnimSystem animSys = BattleSystems.BattleSystem_GetBattleAnimSystem(task.BattleSys!)!;
        BattlerData battler = task.Battler!;
        switch (task.State)
        {
            case 0:
                {
                    BallThrow ballThrow = new();
                    ballThrow.Type = BallThrowType(task.BattlerType);
                    ballThrow.HeapID = HeapIDBattle;
                    ballThrow.Mode = task.DropBall == 1 ? BallModeShow : 0;
                    ballThrow.Target = task.BattlerId;
                    ballThrow.BallID = task.Ball;
                    ballThrow.SpriteSys = BattleSystems.BattleSystem_GetSpriteSystem(task.BattleSys!);
                    ballThrow.PaletteData = BattleSystems.BattleSystem_GetPaletteData(task.BattleSys!);
                    ballThrow.BgPrio = 1;
                    ballThrow.Surface = 0;
                    task.BallRotation = BallRotations.BallRotation_New(ballThrow);
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
                    task.ScreenEffect = null;
                    task.ScreenEffect = BattlerObjSpritesFuncs.BattlerObjSprites_NewAll(task.BattleSys!, HeapIDBattle);
                    task.State++;
                    break;
                }
            case 1:
                {
                    Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(task.BattleSys!, task.BattlerId, task.PartySlot);
                    task.BallEffect = BallEffects.BallEffect_New(HeapIDBattle, task.BattlerType, task.Ball, mon);
                    BallEffects.BallEffect_Load(task.BallEffect);
                    task.State++;
                    break;
                }
            case 2:
                if (BallRotations.BallRotation_GetMode(task.BallRotation) != 0 && task.DropBall == 0)
                {
                    break;
                }
                if (BallEffects.BallEffect_IsLoaded(task.BallEffect!) != 1 || BallRotations.BallRotation_IsFinished(task.BallRotation) != 1)
                {
                    break;
                }
                if (task.ScreenEffect != null)
                {
                    BattlerObjSpritesFuncs.BattlerObjSprites_Free(task.ScreenEffect);
                }
                PokemonSprite.PokemonSprite_StartFade(battler.MonSprite!, 0x10, 0x10, 0, BallColor(task.Ball));
                PokemonSprite.PokemonSprite_SetAttribute(battler.MonSprite!, MonAttrHide, 0);
                BallEffects.BallEffect_Start(task.BallEffect!);
                SoundPlayback.Sound_PlayPannedEffect(SeqBallOpen, task.Face == SideEnemyMode ? PanEnemy : PanPlayer);
                task.State++;
                break;
            case 3:
                if (BallRotations.BallRotation_IsActive(task.BallRotation) != 1)
                {
                    task.State++;
                }
                goto case 4;
            case 4:
                if (PokemonSprite.PokemonSprite_GetAttribute(battler.MonSprite!, MonAttrScaleX) == FullScale && BallEffects.BallEffect_IsActive(task.BallEffect!) == 0)
                {
                    BattleDisplay_ShowPokemonCryAndAnim(task);
                    PokemonSprite.PokemonSprite_StartFade(battler.MonSprite!, 0x10, 0, 0, BallColor(task.Ball));
                    task.State = 5;
                    break;
                }
                if (PokemonSprite.PokemonSprite_GetAttribute(battler.MonSprite!, MonAttrScaleX) >= FullScale)
                {
                    PokemonSprite.PokemonSprite_SetAttribute(battler.MonSprite!, MonAttrScaleX, FullScale);
                    PokemonSprite.PokemonSprite_SetAttribute(battler.MonSprite!, MonAttrScaleY, FullScale);
                    BattleDisplay_ShowPokemonCryAndAnim(task);
                    PokemonSprite.PokemonSprite_StartFade(battler.MonSprite!, 0x10, 0, 1, BallColor(task.Ball));
                    task.State = 5;
                    break;
                }
                PokemonSprite.PokemonSprite_AddAttribute(battler.MonSprite!, MonAttrScaleX, ScaleStep);
                PokemonSprite.PokemonSprite_AddAttribute(battler.MonSprite!, MonAttrScaleY, ScaleStep);
                PokemonSprite.PokemonSprite_CalcScaledYOffset(battler.MonSprite!, task.SpriteYOffset);
                break;
            case 5:
                if (BallEffects.BallEffect_IsActive(task.BallEffect!) == 0)
                {
                    task.State = 6;
                }
                break;
            case 6:
                if (PokemonAnimManager.PokemonAnimManager_HasAnimCompleted(BattleSystem_GetPokemonAnimManager(task.BattleSys!), task.BattlerId) != 1 || PokemonSprite.PokemonSprite_IsAnimActive(battler.MonSprite!))
                {
                    break;
                }
                BallRotations.BallRotation_Free(task.BallRotation);
                BallEffects.BallEffect_Free(task.BallEffect!);
                if (task.IsShiny != 0)
                {
                    BattleDisplay_StartSubstituteAnim(task.BattleSys!, battler, animSys, task.BattlerId, ShinyEffectAnim);
                    task.State = 7;
                }
                else
                {
                    task.State = 8;
                }
                break;
            case 7:
            case 9:
            case 11:
                BattleAnimSystems.BattleAnimSystem_ExecuteScript(animSys);
                if (BattleAnimSystems.BattleAnimSystem_IsMoveActive(animSys) == 0)
                {
                    BattleAnimSystems.BattleAnimSystem_FreeScriptData(animSys);
                    task.State++;
                }
                break;
            case 8:
                if (task.HasSubstitute != 0)
                {
                    BattleDisplay_StartSubstituteAnim(task.BattleSys!, battler, animSys, task.BattlerId, SubstituteOutEffect);
                    task.State++;
                }
                else
                {
                    task.State = StateDone;
                }
                break;
            case 10:
                {
                    BattlerSpriteContext ctx = new();
                    BattleDisplay_InitBattlerSpriteContext(task.BattleSys!, task.MonInfo, ctx, task.BattlerId);
                    BattlerSlide.BattlerSlide_StartPartnerSlideIn(ctx, HeapIDBattle);
                    BattleDisplay_StartSubstituteAnim(task.BattleSys!, battler, animSys, task.BattlerId, SubstituteInEffect);
                    battler.IsAlternateSprite = 1;
                    task.State++;
                    break;
                }
            default:
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.BattlerId, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }

    // OFFSET 0x0225D118
    public static void BattleDisplay_ReturnPokemon(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        ReturnPokemonTask task = new();
        task.BattleSys = battleSys;
        task.Battler = battler;
        task.Sprite = battler.MonSprite;
        task.Command = data[0];
        task.BattlerId = battler.Battler;
        task.BattlerType = battler.BattlerType;
        task.State = 0;
        task.YOffset = data[1];
        task.Ball = (ushort)BattleIO.R16(data, 2);
        task.HasSubstitute = (int)BattleIO.R32(data, 4);
        CopyMonInfo(task.MonInfo, data, 8);
        SysTask.SysTask_Start(BattleDisplay_ReturnPokemonTask, task, 0);
    }

    // OFFSET 0x0225F764
    private static void BattleDisplay_ReturnPokemonTask(SysTask sysTask, object? param)
    {
        ReturnPokemonTask task = (ReturnPokemonTask)param!;
        BattleAnimSystem animSys = BattleSystems.BattleSystem_GetBattleAnimSystem(task.BattleSys!)!;
        switch (task.State)
        {
            case 0:
                if (task.HasSubstitute != 0)
                {
                    BattleDisplay_StartSubstituteAnim(task.BattleSys!, task.Battler!, animSys, task.BattlerId, SubstituteOutEffect);
                    task.State++;
                }
                else
                {
                    task.State = 4;
                }
                break;
            case 1:
            case 3:
                BattleAnimSystems.BattleAnimSystem_ExecuteScript(animSys);
                if (BattleAnimSystems.BattleAnimSystem_IsMoveActive(animSys) == 0)
                {
                    BattleAnimSystems.BattleAnimSystem_FreeScriptData(animSys);
                    task.State++;
                }
                break;
            case 2:
                {
                    BattlerSpriteContext ctx = new();
                    BattleDisplay_InitBattlerSpriteContext(task.BattleSys!, task.MonInfo, ctx, task.BattlerId);
                    BattlerSlide.BattlerSlide_StartSlideOut(ctx, HeapIDBattle);
                    BattleDisplay_StartSubstituteAnim(task.BattleSys!, task.Battler!, animSys, task.BattlerId, SubstituteInEffect);
                    task.Battler!.IsAlternateSprite = 0;
                    task.State++;
                    break;
                }
            case 4:
                {
                    int battlerType = task.BattlerType;
                    BallThrow ballThrow = new();
                    ballThrow.Type = (battlerType & 1) != 0 ? BallThrowType(battlerType) : BallReturnType(battlerType);
                    ballThrow.HeapID = HeapIDBattle;
                    ballThrow.Mode = BallModeReturn;
                    ballThrow.Target = task.BattlerId;
                    ballThrow.BallID = task.Ball;
                    ballThrow.SpriteSys = BattleSystems.BattleSystem_GetSpriteSystem(task.BattleSys!);
                    ballThrow.PaletteData = BattleSystems.BattleSystem_GetPaletteData(task.BattleSys!);
                    ballThrow.BgPrio = 1;
                    ballThrow.Surface = 0;
                    task.BallRotation = BallRotations.BallRotation_New(ballThrow);
                    task.State = 5;
                    PokemonSprite.PokemonSprite_StartFade(task.Sprite!, 0, 0x10, 0, BallColor(task.Ball));
                    switch (battlerType)
                    {
                        case 0:
                        case 2:
                        case 4:
                            SoundPlayback.Sound_PlayPannedEffect(SeqBallOpen, PanPlayer);
                            break;
                        case 1:
                        case 3:
                        case 5:
                            SoundPlayback.Sound_PlayPannedEffect(SeqBallOpen, PanEnemy);
                            break;
                    }
                    break;
                }
            case 5:
                if (!PokemonSprite.PokemonSprite_IsFadeActive(task.Sprite!))
                {
                    task.State++;
                }
                break;
            case 6:
                PokemonSprite.PokemonSprite_AddAttribute(task.Sprite!, MonAttrScaleX, ScaleShrinkStep);
                PokemonSprite.PokemonSprite_AddAttribute(task.Sprite!, MonAttrScaleY, ScaleShrinkStep);
                PokemonSprite.PokemonSprite_CalcScaledYOffset(task.Sprite!, task.YOffset);
                if (PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonAttrScaleX) <= 0)
                {
                    PokemonSprite.PokemonSprite_Delete(task.Sprite!);
                    task.State = 7;
                }
                break;
            case 7:
                if (BallRotations.BallRotation_IsActive(task.BallRotation) == 0)
                {
                    BallRotations.BallRotation_Free(task.BallRotation);
                    task.State = 8;
                }
                break;
            case 8:
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.BattlerId, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }

    // OFFSET 0x0225D1C4
    public static void BattleDisplay_OpenCaptureBall(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        OpenCaptureBallTask task = new();
        task.BattleSys = battleSys;
        task.Sprite = battler.MonSprite;
        task.Command = data[0];
        task.Battler = battler.Battler;
        task.BattlerType = battler.BattlerType;
        task.State = 0;
        task.YShift = data[1];
        PokemonSprite.PokemonSprite_StartFade(task.Sprite!, 0, 0x10, 0, BallColor(BattleIO.R16(data, 2)));
        PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonAttrShadowShouldFollowY, 1);
        SysTask.SysTask_Start(BattleDisplay_OpenCaptureBallTask, task, 0);
    }

    // OFFSET 0x0225FA00
    private static void BattleDisplay_OpenCaptureBallTask(SysTask sysTask, object? param)
    {
        OpenCaptureBallTask task = (OpenCaptureBallTask)param!;
        switch (task.State)
        {
            case 0:
                PokemonSprite.PokemonSprite_AddAttribute(task.Sprite!, MonAttrScaleX, ScaleShrinkStep);
                PokemonSprite.PokemonSprite_AddAttribute(task.Sprite!, MonAttrScaleY, ScaleShrinkStep);
                if (task.YShift != 0)
                {
                    task.YShift--;
                    PokemonSprite.PokemonSprite_AddAttribute(task.Sprite!, MonAttrY, -1);
                }
                if (PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonAttrScaleX) <= 0)
                {
                    PokemonSprite.PokemonSprite_Delete(task.Sprite!);
                    task.State++;
                }
                break;
            case 1:
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }
}
