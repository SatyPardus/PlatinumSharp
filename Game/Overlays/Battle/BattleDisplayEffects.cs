namespace PlatinumSharp.Game;

public sealed class MoveAnimationTask
{
    public BattleSystem? BattleSys;
    public BattlerData? Battler;
    public PokemonSprite? Sprite;
    public BattleAnimSystem? AnimSys;
    public readonly byte[] MoveAnim = new byte[0x58];
    public byte Command;
    public byte BattlerID;
    public byte State;
    public byte HideHealthboxes;
    public byte HideShadows;
}

public sealed class FlickerTask
{
    public BattleSystem? BattleSys;
    public PokemonSprite? Sprite;
    public byte Battler;
    public byte Count;
    public byte Timer;
}

public sealed class FaintAnimationTask
{
    public BattleSystem? BattleSys;
    public BattlerData? Battler;
    public PokemonSprite? Sprite;
    public readonly byte[] MonInfo = new byte[0x58];
    public byte Command;
    public byte BattlerID;
    public byte State;
    public byte Face;
    public ushort Species;
    public byte Gender;
    public byte Form;
    public uint Personality;
    public ushort IsSubstitute;
    public ushort IsTransformed;
}

public sealed class FadeOutTask
{
    public BattleSystem? BattleSys;
    public byte Command;
    public byte Battler;
    public byte State;
}

public sealed class ToggleVanishTask
{
    public BattleSystem? BattleSys;
    public BattlerData? Battler;
    public readonly byte[] MonInfo = new byte[0x58];
    public byte Command;
    public byte BattlerID;
    public byte State;
    public byte Toggle;
    public int IsSubstitute;
}

public sealed class LevelUpTask
{
    public BattleSystem? BattleSys;
    public HealthBox? Healthbox;
    public byte Command;
    public byte Battler;
    public byte State;
    public byte Done;
}

public sealed class AlertMessageTask
{
    public BattleSystem? BattleSys;
    public byte Command;
    public byte Battler;
    public byte PrinterID;
    public byte State;
    public byte Timer;
}

public static partial class Battlers
{
    private const int CommandFlickerBattlerID = 0x17;
    private const int MonSpriteHideAttr = 6;

    // OFFSET 0x0225C2EC
    private static void Battler_CommandPrintAttackMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PrintAttackMessage(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C300
    private static void Battler_CommandPrintMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PrintMessage(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C314
    private static void Battler_CommandSetMoveAnimation(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_SetMoveAnimation(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C328
    private static void Battler_CommandFlickerBattler(BattleSystem battleSys, BattlerData battler)
    {
        if (PokemonSprite.PokemonSprite_GetAttribute(battler.MonSprite!, MonSpriteHideAttr) == 1)
        {
            BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, CommandFlickerBattlerID);
        }
        else
        {
            BattleDisplay.BattleDisplay_FlickerBattler(battleSys, battler);
        }
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C35C
    private static void Battler_CommandUpdateHPGauge(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_UpdateHPGauge(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C370
    private static void Battler_CommandUpdateExpGauge(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_UpdateExpGauge(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C384
    private static void Battler_CommandPlayFaintingSequence(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PlayFaintingSequence(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C398
    private static void Battler_CommandPlaySound(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PlaySound(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C3AC
    private static void Battler_CommandFadeOut(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_FadeOut(battleSys, battler);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C3BC
    private static void Battler_CommandToggleVanish(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_ToggleVanish(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C3D0
    private static void Battler_CommandSetStatusIcon(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_SetStatusIcon(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C3E4
    private static void Battler_CommandPrintTrainerMessage(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PrintTrainerMessage(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C444
    private static void Battler_CommandPlayLevelUpAnimation(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_PlayLevelUpAnimation(battleSys, battler);
        Battler_ClearCommand(battler);
    }
}

public static partial class BattleDisplay
{
    private const int FaceFrontDisplay = 0;
    private const int FaceBackDisplay = 2;
    private const int MonSpriteYCenterAttr = 1;
    private const int MonSpriteHide = 6;
    private const int MonSpritePartialDrawHeight = 0x12;
    private const int MonSpriteShadowHeightAttr = 0x29;
    private const int MoveAnimTaskPriority = 0;
    private const int GaugeTaskPriority = 1000;
    private const int TagNicknameDisplay = 2;
    private const int SeExpGain = 0x70B;
    private const int SeLevelUp = 0x70C;
    private const int SeFaint = 0x703;
    private const int PanPlayer = -0x75;
    private const int CryModFaint = 5;
    private const int CryHeap = 5;
    private const int HPGaugeNoDamage = 0x7FFF;
    private const int TransferDataBattleEffect = 1;
    private const int TransferSubstituteOut = 0xF;
    private const int TransferSubstituteIn = 0x10;
    private const int TransferAlternateSpriteOn = 0x19;
    private const int TransferAlternateSpriteOff = 0x1A;
    private const int MoveFlagHideHealthbox = 0x40;
    private const int MoveFlagHideShadows = 0x80;
    private const int MoveParamFlags = 9;
    private const int MoveArcIDMoves = 8;
    private const int MoveArcIDBattleEffects = 0x3F;

    // OFFSET 0x0225D8F0
    public static void BattleDisplay_PrintAttackMessage(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        MessageLoader msgLoader = BattleSystems.BattleSystem_GetAttackMessageLoader(battleSys)!;
        MessageWaitTask task = new();
        task.BattleSys = battleSys;
        task.Command = data[0];
        task.Battler = battler.Battler;
        BattleMessage message = new();
        message.Id = (ushort)(BattleIO.R16(data, 2) * 3);
        message.Tags = TagNicknameDisplay;
        message.Params[0] = (data[1] << 8) | battler.Battler;
        task.PrinterID = BattleMessages.BattleMessage_Print(battleSys, msgLoader, message, BattleMessages.BattleSystem_GetTextSpeed(battleSys));
        SysTask.SysTask_Start(BattleDisplay_MessageWaitTask, task, 0);
    }

    // OFFSET 0x0225D958
    public static void BattleDisplay_PrintMessage(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        MessageLoader msgLoader = BattleSystems.BattleSystem_GetMessageLoader(battleSys)!;
        MessageWaitTask task = new();
        task.BattleSys = battleSys;
        task.Command = data[0];
        task.Battler = battler.Battler;
        BattleMessage message = new();
        BattleIO.ReadBattleMessage(data, 0, message);
        task.PrinterID = BattleMessages.BattleMessage_Print(battleSys, msgLoader, message, BattleMessages.BattleSystem_GetTextSpeed(battleSys));
        SysTask.SysTask_Start(BattleDisplay_MessageWaitTask, task, 0);
    }

    // OFFSET 0x0225D9A8
    public static void BattleDisplay_SetMoveAnimation(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        MoveAnimationTask task = new();
        task.State = 0;
        task.BattleSys = battleSys;
        task.Battler = battler;
        task.Command = data[0];
        task.BattlerID = battler.Battler;
        task.AnimSys = BattleSystems.BattleSystem_GetBattleAnimSystem(battleSys);
        Array.Copy(data, task.MoveAnim, task.MoveAnim.Length);
        task.Sprite = battler.MonSprite;
        int transferData = (int)BattleIO.R32(data, 0x4C);
        int transferValue = (int)BattleIO.R32(data, 0x50);
        if (transferData == TransferDataBattleEffect && transferValue == TransferAlternateSpriteOn)
        {
            battler.IsAlternateSprite = 1;
        }
        if (transferData == TransferDataBattleEffect && transferValue == TransferAlternateSpriteOff)
        {
            battler.IsAlternateSprite = 0;
        }
        BattleDisplay_GetMoveHideFlags(out task.HideHealthboxes, out task.HideShadows, transferData, transferValue, (ushort)BattleIO.R16(data, 2));
        SysTask.SysTask_Start(BattleDisplay_MoveAnimationTask, task, MoveAnimTaskPriority);
    }

    // OFFSET 0x0225DA44
    public static void BattleDisplay_FlickerBattler(BattleSystem battleSys, BattlerData battler)
    {
        FlickerTask task = new();
        task.Count = 0;
        task.BattleSys = battleSys;
        task.Sprite = battler.MonSprite;
        task.Battler = battler.Battler;
        task.Timer = 0;
        SysTask.SysTask_Start(BattleDisplay_FlickerTask, task, 0);
    }

    // OFFSET 0x0225DA74
    public static void BattleDisplay_UpdateHPGauge(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        HealthBox healthbox = battler.Healthbox;
        if (healthbox.MainSprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        healthbox.State = 0;
        healthbox.BattleSys = battleSys;
        healthbox.Command = data[0];
        healthbox.Battler = battler.Battler;
        healthbox.Type = Healthboxes.Healthbox_Type(battler.BattlerType, BattleSystems.BattleSystem_GetBattleType(battleSys));
        healthbox.CurHP = (short)BattleIO.R16(data, 2);
        healthbox.MaxHP = (ushort)BattleIO.R16(data, 4);
        healthbox.Damage = (int)BattleIO.R32(data, 8);
        healthbox.Level = data[1];
        if (BattleIO.R32(data, 8) == HPGaugeNoDamage)
        {
            healthbox.CurHP = 0;
            healthbox.Damage = 0;
        }
        healthbox.Task10 = SysTask.SysTask_Start(BattleDisplay_UpdateHPGaugeTask, healthbox, GaugeTaskPriority);
    }

    // OFFSET 0x0225DB00
    public static void BattleDisplay_UpdateExpGauge(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        HealthBox healthbox = battler.Healthbox;
        if (healthbox.MainSprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        healthbox.State = 0;
        healthbox.BattleSys = battleSys;
        healthbox.Command = data[0];
        healthbox.Battler = battler.Battler;
        healthbox.CurExp = (int)BattleIO.R32(data, 4);
        healthbox.MaxExp = (int)BattleIO.R32(data, 0xC);
        healthbox.ExpReward = (int)BattleIO.R32(data, 8) - healthbox.CurExp;
        if (battler.BattlerType == 0)
        {
            healthbox.Task10 = SysTask.SysTask_Start(BattleDisplay_UpdateExpGaugeTask, healthbox, GaugeTaskPriority);
        }
        else
        {
            BattleIO.BattleIO_ClearMessage(healthbox.BattleSys!, healthbox.Battler, healthbox.Command);
        }
    }

    // OFFSET 0x0225DB74
    public static void BattleDisplay_PlayFaintingSequence(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        FaintAnimationTask task = new();
        task.Face = (byte)((battler.BattlerType & 1) != 0 ? FaceBackDisplay : FaceFrontDisplay);
        task.State = 0;
        task.BattleSys = battleSys;
        task.Battler = battler;
        task.Command = data[0];
        task.BattlerID = battler.Battler;
        task.Sprite = battler.MonSprite;
        task.Species = (ushort)BattleIO.R16(data, 2);
        task.Gender = data[1];
        task.Form = data[8];
        task.Personality = BattleIO.R32(data, 4);
        task.IsSubstitute = data[9];
        task.IsTransformed = data[0xA];
        for (int i = 0; i < 4; i++)
        {
            BattleIO.W16(task.MonInfo, 0x18 + i * 2, BattleIO.R16(data, 0xC + i * 2));
            task.MonInfo[0x20 + i] = data[0x14 + i];
            task.MonInfo[0x24 + i] = data[0x18 + i];
            task.MonInfo[0x28 + i] = data[0x1C + i];
            BattleIO.W32(task.MonInfo, 0x2C + i * 4, BattleIO.R32(data, 0x20 + i * 4));
        }
        SysTask.SysTask_Start(BattleDisplay_FaintingSequenceTask, task, 0);
    }

    // OFFSET 0x0225DC4C
    public static void BattleDisplay_PlaySound(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        int pan = (battler.BattlerType & 1) != 0 ? PanEnemy : PanPlayer;
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, data[0]);
        SoundPlayback.Sound_PlayPannedEffect((ushort)BattleIO.R16(data, 2), pan);
    }

    // OFFSET 0x0225DC7C
    public static void BattleDisplay_FadeOut(BattleSystem battleSys, BattlerData battler)
    {
        FadeOutTask task = new();
        task.State = 0;
        task.BattleSys = battleSys;
        task.Command = battler.Data[0];
        task.Battler = battler.Battler;
        SysTask.SysTask_Start(BattleDisplay_FadeOutTask, task, 0);
    }

    // OFFSET 0x0225DCB0
    public static void BattleDisplay_ToggleVanish(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        ToggleVanishTask task = new();
        task.BattleSys = battleSys;
        task.Battler = battler;
        task.Command = data[0];
        task.BattlerID = battler.Battler;
        task.State = 0;
        task.Toggle = data[1];
        task.IsSubstitute = data[2];
        for (int i = 0; i < 4; i++)
        {
            BattleIO.W16(task.MonInfo, 0x18 + i * 2, BattleIO.R16(data, 4 + i * 2));
            task.MonInfo[0x20 + i] = data[0xC + i];
            task.MonInfo[0x24 + i] = data[0x10 + i];
            task.MonInfo[0x28 + i] = data[0x14 + i];
            BattleIO.W32(task.MonInfo, 0x2C + i * 4, BattleIO.R32(data, 0x18 + i * 4));
        }
        SysTask.SysTask_Start(BattleDisplay_ToggleVanishTask, task, 0);
    }

    // OFFSET 0x0225DD44
    public static void BattleDisplay_SetStatusIcon(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        HealthBox healthbox = battler.Healthbox;
        if (healthbox.MainSprite == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        healthbox.Status = data[1];
        Healthboxes.Healthbox_DrawInfo(healthbox, healthbox.CurHP, 0x100);
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, data[0]);
    }

    // OFFSET 0x0225DD7C
    public static void BattleDisplay_PrintTrainerMessage(BattleSystem battleSys, BattlerData battler, byte[] data)
    {
        int trainerID = BattleSystems.BattleSystem_GetTrainerID(battleSys, battler.Battler);
        MessageWaitTask task = new();
        task.BattleSys = battleSys;
        task.Command = data[0];
        task.Battler = battler.Battler;
        task.PrinterID = BattleMessages.BattleMessage_PrintTrainerMessage(battleSys, trainerID, battler.Battler, data[1], BattleMessages.BattleSystem_GetTextSpeed(battleSys));
        SysTask.SysTask_Start(BattleDisplay_MessageWaitTask, task, 0);
    }

    // OFFSET 0x0225DF34
    public static void BattleDisplay_PlayLevelUpAnimation(BattleSystem battleSys, BattlerData battler)
    {
        LevelUpTask task = new();
        task.BattleSys = battleSys;
        task.Command = battler.Data[0];
        task.Battler = battler.Battler;
        task.State = 0;
        task.Healthbox = battler.Healthbox;
        SysTask.SysTask_Start(BattleDisplay_LevelUpTask, task, 0);
    }

    // OFFSET 0x022626C0
    private static void BattleDisplay_MoveAnimationTask(SysTask sysTask, object? param)
    {
        MoveAnimationTask task = (MoveAnimationTask)param!;
        byte[] message = new byte[0x58];
        switch (task.State)
        {
            case 0:
                if ((BattleIO.R16(task.MoveAnim, 0xE) & 1) != 0 && BattleIO.R32(task.MoveAnim, 0x4C) == 0 && PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonSpriteHide) == 0)
                {
                    BattleIO.BattleIO_SetMoveAnimation(task.BattleSys!, null, message, TransferDataBattleEffect, TransferSubstituteOut, task.BattlerID, task.BattlerID, 0);
                    BattleDisplay_StartMoveAnimation(task.BattleSys!, task.Battler!, task.AnimSys!, message);
                    task.State = 1;
                }
                else
                {
                    task.State = 4;
                }
                break;
            case 1:
            case 3:
            case 5:
            case 7:
            case 9:
                BattleAnimSystems.BattleAnimSystem_ExecuteScript(task.AnimSys!);
                if (BattleAnimSystems.BattleAnimSystem_IsMoveActive(task.AnimSys!) == 0)
                {
                    BattleAnimSystems.BattleAnimSystem_FreeScriptData(task.AnimSys!);
                    task.State++;
                }
                break;
            case 2:
            {
                BattlerSpriteContext ctx = new();
                BattleDisplay_InitBattlerSpriteContext(task.BattleSys!, task.MoveAnim, ctx, task.BattlerID);
                BattlerSlide.BattlerSlide_StartSlideOut(ctx, 5);
                BattleIO.BattleIO_SetMoveAnimation(task.BattleSys!, null, message, TransferDataBattleEffect, TransferSubstituteIn, task.BattlerID, task.BattlerID, 0);
                BattleDisplay_StartMoveAnimation(task.BattleSys!, task.Battler!, task.AnimSys!, message);
                task.Battler!.IsAlternateSprite = 0;
                task.State++;
                break;
            }
            case 4:
                Battle.BattleSystem_SetRedHPSoundFlag(task.BattleSys!, 2);
                if (task.HideHealthboxes != 0)
                {
                    Battle.BattleSystem_HideHealthboxes(task.BattleSys!);
                }
                if (task.HideShadows != 0)
                {
                    PokemonSpriteManager.PokemonSpriteManager_SetHideShadows(BattleSystems.BattleSystem_GetPokemonSpriteManager(task.BattleSys!)!, 1);
                }
                BattleDisplay_StartMoveAnimation(task.BattleSys!, task.Battler!, task.AnimSys!, task.MoveAnim);
                task.State++;
                break;
            case 6:
                Battle.BattleSystem_SetRedHPSoundFlag(task.BattleSys!, 0);
                if (task.HideHealthboxes != 0)
                {
                    Battle.BattleSystem_ShowActiveHealthboxes(task.BattleSys!);
                }
                if (task.HideShadows != 0)
                {
                    PokemonSpriteManager.PokemonSpriteManager_ClearHideShadows(BattleSystems.BattleSystem_GetPokemonSpriteManager(task.BattleSys!)!, 1);
                }
                if ((BattleIO.R16(task.MoveAnim, 0xE) & 1) != 0 && BattleIO.R32(task.MoveAnim, 0x4C) == 0 && PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonSpriteHide) == 0)
                {
                    BattleIO.BattleIO_SetMoveAnimation(task.BattleSys!, null, message, TransferDataBattleEffect, TransferSubstituteOut, task.BattlerID, task.BattlerID, 0);
                    BattleDisplay_StartMoveAnimation(task.BattleSys!, task.Battler!, task.AnimSys!, message);
                    task.State = 7;
                }
                else
                {
                    task.State = 10;
                }
                break;
            case 8:
            {
                BattlerSpriteContext ctx = new();
                BattleDisplay_InitBattlerSpriteContext(task.BattleSys!, task.MoveAnim, ctx, task.BattlerID);
                BattlerSlide.BattlerSlide_StartPartnerSlideIn(ctx, 5);
                BattleIO.BattleIO_SetMoveAnimation(task.BattleSys!, null, message, TransferDataBattleEffect, TransferSubstituteIn, task.BattlerID, task.BattlerID, 0);
                BattleDisplay_StartMoveAnimation(task.BattleSys!, task.Battler!, task.AnimSys!, message);
                task.Battler!.IsAlternateSprite = 1;
                task.State++;
                break;
            }
            case 10:
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.BattlerID, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }

    // OFFSET 0x0226292C
    private static void BattleDisplay_FlickerTask(SysTask sysTask, object? param)
    {
        FlickerTask task = (FlickerTask)param!;
        if (task.Count < 6)
        {
            if (task.Timer == 0)
            {
                task.Timer = 2;
                task.Count++;
                int hidden = PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonSpriteHide);
                PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonSpriteHide, hidden ^ 1);
            }
            else
            {
                task.Timer--;
            }
            return;
        }
        PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonSpriteHide, 0);
        BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, 0x17);
        SysTask.SysTask_Done(sysTask);
    }

    // OFFSET 0x02262988
    private static void BattleDisplay_UpdateHPGaugeTask(SysTask sysTask, object? param)
    {
        HealthBox healthbox = (HealthBox)param!;
        switch (healthbox.State)
        {
            case 0:
                Healthboxes.Healthbox_CalcHP(healthbox, healthbox.Damage);
                healthbox.State++;
                goto case 1;
            case 1:
                if (Healthboxes.Healthbox_UpdateHP(healthbox) == -1)
                {
                    healthbox.State++;
                }
                break;
            default:
                BattleIO.BattleIO_ClearMessage(healthbox.BattleSys!, healthbox.Battler, healthbox.Command);
                healthbox.Task10 = null;
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }

    // OFFSET 0x022629DC
    private static void BattleDisplay_UpdateExpGaugeTask(SysTask sysTask, object? param)
    {
        HealthBox healthbox = (HealthBox)param!;
        switch (healthbox.State)
        {
            case 0:
                healthbox.ExpSoundTimer = 0;
                SoundPlayback.Sound_PlayEffect(SeExpGain);
                Healthboxes.Healthbox_CalcExp(healthbox, healthbox.ExpReward);
                healthbox.State++;
                goto case 1;
            case 1:
                if (healthbox.ExpSoundTimer < 8)
                {
                    healthbox.ExpSoundTimer++;
                }
                if (Healthboxes.Healthbox_UpdateExp(healthbox) == -1)
                {
                    if (healthbox.ExpSoundTimer >= 8)
                    {
                        SoundPlayback.Sound_StopEffect(SeExpGain, 0);
                        healthbox.State = 100;
                    }
                    else
                    {
                        healthbox.State++;
                    }
                }
                break;
            case 2:
                healthbox.ExpSoundTimer++;
                if (healthbox.ExpSoundTimer >= 8)
                {
                    SoundPlayback.Sound_StopEffect(SeExpGain, 0);
                    healthbox.State = 100;
                }
                break;
            default:
                BattleIO.BattleIO_ClearMessage(healthbox.BattleSys!, healthbox.Battler, healthbox.Command);
                healthbox.Task10 = null;
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }

    // OFFSET 0x02262A9C
    private static void BattleDisplay_FaintingSequenceTask(SysTask sysTask, object? param)
    {
        FaintAnimationTask task = (FaintAnimationTask)param!;
        BattleAnimSystem animSys = BattleSystems.BattleSystem_GetBattleAnimSystem(task.BattleSys!)!;
        byte[] message = new byte[0x58];
        switch (task.State)
        {
            case 0:
                if (task.IsSubstitute != 0)
                {
                    BattleIO.BattleIO_SetMoveAnimation(task.BattleSys!, null, message, TransferDataBattleEffect, TransferSubstituteOut, task.BattlerID, task.BattlerID, 0);
                    BattleDisplay_StartMoveAnimation(task.BattleSys!, task.Battler!, animSys, message);
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
                BattleDisplay_InitBattlerSpriteContext(task.BattleSys!, task.MonInfo, ctx, task.BattlerID);
                BattlerSlide.BattlerSlide_StartSlideOut(ctx, 5);
                BattleIO.BattleIO_SetMoveAnimation(task.BattleSys!, null, message, TransferDataBattleEffect, TransferSubstituteIn, task.BattlerID, task.BattlerID, 0);
                BattleDisplay_StartMoveAnimation(task.BattleSys!, task.Battler!, animSys, message);
                task.Battler!.IsAlternateSprite = 0;
                task.State++;
                break;
            }
            case 4:
            {
                int pan = task.Face == FaceBackDisplay ? PanEnemy : PanPlayer;
                PokemonSpriteMetadata.Pokemon_PlayDelayedCry(BattleDisplay_GetChatotCry(task.BattleSys!, task.BattlerID), CryModFaint, task.Species, task.Form, pan, CryVolume, task.IsTransformed, CryHeap, 0);
                task.State++;
                goto case 5;
            }
            case 5:
                if (SoundPlayback.Sound_IsPokemonCryPlaying() != 0)
                {
                    break;
                }
                task.State++;
                break;
            case 6:
                SoundPlayback.Sound_PlayPannedEffect(SeFaint, task.Face == FaceBackDisplay ? PanEnemy : PanPlayer);
                task.State = (byte)(PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonSpriteShadowHeightAttr) > 0 ? 7 : 8);
                break;
            case 7:
            {
                int height = PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonSpriteShadowHeightAttr) - 8;
                if (height < 0)
                {
                    height = 0;
                }
                PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonSpriteShadowHeightAttr, height);
                if (height != 0)
                {
                    break;
                }
                task.State++;
                goto case 8;
            }
            case 8:
            {
                int yOffset = PokemonSpriteMetadata.Pokemon_LoadSpriteYOffset(task.Species, task.Gender, task.Face, task.Form, task.Personality);
                PokemonSprite.PokemonSprite_SetPartialDraw(task.Sprite!, 0, 0, 0x50, 0x50 - yOffset);
                task.State++;
                break;
            }
            case 9:
            {
                int y = PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonSpriteYCenterAttr);
                PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonSpriteYCenterAttr, y + 8);
                int height = PokemonSprite.PokemonSprite_GetAttribute(task.Sprite!, MonSpritePartialDrawHeight) - 8;
                if (height < 0)
                {
                    height = 0;
                }
                PokemonSprite.PokemonSprite_SetAttribute(task.Sprite!, MonSpritePartialDrawHeight, height);
                if (height == 0)
                {
                    PokemonSprite.PokemonSprite_Delete(task.Sprite!);
                    task.State++;
                }
                break;
            }
            case 10:
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.BattlerID, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }

    private static object? BattleDisplay_GetChatotCry(BattleSystem battleSys, int battler)
    {
        return BattleSystem_GetChatotCry(battleSys, battler);
    }

    // OFFSET 0x02262D28
    private static void BattleDisplay_FadeOutTask(SysTask sysTask, object? param)
    {
        FadeOutTask task = (FadeOutTask)param!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(task.BattleSys!)!;
        PokemonSpriteManager monSpriteMan = BattleSystems.BattleSystem_GetPokemonSpriteManager(task.BattleSys!)!;
        switch (task.State)
        {
            case 0:
                PaletteAnimator.PaletteAnimator_SetMode(BattleSystems.BattleSystem_GetPaletteAnimator(task.BattleSys!)!, 1);
                PaletteData.PaletteData_StartFade(paletteData, 0xF, 0xFFFF, 1, 0, 0x10, 0);
                PokemonSpriteManager.PokemonSpriteManager_StartFadeAll(monSpriteMan, 0, 0x10, 0, 0);
                SoundPlayback.Sound_FadeOutBGM(0, 0x10);
                task.State++;
                goto case 1;
            case 1:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
                {
                    break;
                }
                task.State++;
                break;
            case 2:
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }

    // OFFSET 0x02262DC4
    private static void BattleDisplay_ToggleVanishTask(SysTask sysTask, object? param)
    {
        ToggleVanishTask task = (ToggleVanishTask)param!;
        BattleAnimSystem animSys = BattleSystems.BattleSystem_GetBattleAnimSystem(task.BattleSys!)!;
        byte[] message = new byte[0x58];
        switch (task.State)
        {
            case 0:
                if (task.Battler!.MonSprite != null)
                {
                    PokemonSprite.PokemonSprite_SetAttribute(task.Battler.MonSprite, MonSpriteHide, task.Toggle);
                    if (task.Toggle == 1)
                    {
                        task.State = 0xFF;
                    }
                    else
                    {
                        task.State++;
                    }
                }
                else
                {
                    task.State = 0xFF;
                }
                break;
            case 1:
                if (task.IsSubstitute != 0 && task.Battler!.IsAlternateSprite == 0)
                {
                    BattleIO.BattleIO_SetMoveAnimation(task.BattleSys!, null, message, TransferDataBattleEffect, TransferSubstituteOut, task.BattlerID, task.BattlerID, 0);
                    BattleDisplay_StartMoveAnimation(task.BattleSys!, task.Battler, animSys, message);
                    task.State++;
                }
                else
                {
                    task.State = 0xFF;
                }
                break;
            case 2:
            case 4:
                BattleAnimSystems.BattleAnimSystem_ExecuteScript(animSys);
                if (BattleAnimSystems.BattleAnimSystem_IsMoveActive(animSys) == 0)
                {
                    BattleAnimSystems.BattleAnimSystem_FreeScriptData(animSys);
                    task.State++;
                }
                break;
            case 3:
            {
                BattlerSpriteContext ctx = new();
                BattleDisplay_InitBattlerSpriteContext(task.BattleSys!, task.MonInfo, ctx, task.BattlerID);
                BattlerSlide.BattlerSlide_StartPartnerSlideIn(ctx, 5);
                BattleIO.BattleIO_SetMoveAnimation(task.BattleSys!, null, message, TransferDataBattleEffect, TransferSubstituteIn, task.BattlerID, task.BattlerID, 0);
                BattleDisplay_StartMoveAnimation(task.BattleSys!, task.Battler!, animSys, message);
                task.Battler!.IsAlternateSprite = 1;
                task.State++;
                break;
            }
            default:
                BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.BattlerID, task.Command);
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }

    // OFFSET 0x02262F30
    private static void BattleDisplay_LevelUpTask(SysTask sysTask, object? param)
    {
        LevelUpTask task = (LevelUpTask)param!;
        switch (task.State)
        {
            case 0:
                Healthboxes.Healthbox_PlayLevelUpAnimation(task.Healthbox!, task);
                SoundPlayback.Sound_PlayEffect(SeLevelUp);
                task.State++;
                break;
            case 1:
                if (task.Done == 1)
                {
                    BattleIO.BattleIO_ClearMessage(task.BattleSys!, task.Battler, task.Command);
                    SysTask.SysTask_Done(sysTask);
                }
                break;
        }
    }

    // OFFSET 0x02264408
    private static void BattleDisplay_StartMoveAnimation(BattleSystem battleSys, BattlerData battler, BattleAnimSystem animSys, byte[] moveAnim)
    {
        BattleAnimBattlerContext battlerContext = new();
        ushort move;
        if (BattleIO.R32(moveAnim, 0x4C) == 0)
        {
            battlerContext.MoveArcID = MoveArcIDMoves;
            move = (ushort)BattleIO.R16(moveAnim, 2);
        }
        else
        {
            battlerContext.MoveArcID = MoveArcIDBattleEffects;
            move = (ushort)BattleIO.R32(moveAnim, 0x50);
        }
        battlerContext.BgConfig = BattleSystems.BattleSystem_GetBgConfig(battleSys);
        battlerContext.PaletteData = BattleSystems.BattleSystem_GetPaletteData(battleSys);
        battlerContext.SpriteSystem = BattleSystems.BattleSystem_GetSpriteSystem(battleSys);
        for (int i = 0; i < 4; i++)
        {
            battlerContext.PokemonSpriteData[i] = BattleSystems.BattleSystem_GetBattlerSpriteData(battleSys, i);
            battlerContext.Species[i] = (ushort)BattleIO.R16(moveAnim, 0x18 + i * 2);
            battlerContext.Genders[i] = moveAnim[0x20 + i];
            battlerContext.ShinyFlags[i] = moveAnim[0x24 + i];
            battlerContext.Forms[i] = moveAnim[0x28 + i];
            battlerContext.Personalities[i] = BattleIO.R32(moveAnim, 0x2C + i * 4);
            battlerContext.MoveEffects[i] = BattleIO.R32(moveAnim, 0x3C + i * 4);
        }
        BattleSystems.BattleSystem_GetBattlerTypes(battleSys, battlerContext.BattlerTypes);
        BattleSystems.BattleSystem_GetBattlerMonSprites(battleSys, battlerContext.BattlerSprites);
        battlerContext.BattleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        battlerContext.ChatotCry = BattleSystem_GetChatotCry(battleSys, battler.Battler);
        battlerContext.BgTiles = BattleSystems.BattleSystem_GetBgTiles(battleSys);
        battlerContext.BgPaletteBuffer = BattleSystems.BattleSystem_GetBgPaletteBuffer(battleSys);
        battlerContext.BattleBgRefs.NarcID = 7;
        battlerContext.BattleBgRefs.TilesNarcMemberIdx = BattleSystems.BattleSystem_GetBackground(battleSys) + 3;
        int timeOffset = BattleSystems.BattleSystem_GetBackgroundTimeOffset(battleSys);
        battlerContext.BattleBgRefs.PaletteNarcMemberIdx = BattleSystems.BattleSystem_GetBackground(battleSys) * 3 + 0xAC + timeOffset;
        battlerContext.BattleBgRefs.TilemapNarcMemberIdx = 2;
        battlerContext.BattleBgRefs.PaletteDestStart = 0;
        battlerContext.BattleBgRefs.PaletteSrcSize = 8;
        BattleAnimSystems.BattleAnimSystem_StartMove(animSys, moveAnim, move, battlerContext);
    }

    // OFFSET 0x02264530
    private static void BattleDisplay_InitBattlerSpriteContext(BattleSystem battleSys, byte[] monInfo, BattlerSpriteContext ctx, int battler)
    {
        ctx.TargetBattler = battler;
        ctx.SourceBattler = battler;
        for (int i = 0; i < 4; i++)
        {
            ctx.PokemonSpriteData[i] = BattleSystems.BattleSystem_GetBattlerSpriteData(battleSys, i);
            ctx.Species[i] = (ushort)BattleIO.R16(monInfo, 0x18 + i * 2);
            ctx.Genders[i] = monInfo[0x20 + i];
            ctx.ShinyFlags[i] = monInfo[0x24 + i];
            ctx.Forms[i] = monInfo[0x28 + i];
            ctx.Personalities[i] = BattleIO.R32(monInfo, 0x2C + i * 4);
        }
        BattleSystems.BattleSystem_GetBattlerTypes(battleSys, ctx.Types);
        BattleSystems.BattleSystem_GetBattlerMonSprites(battleSys, ctx.Sprites);
    }

    // OFFSET 0x022645B8
    private static void BattleDisplay_GetMoveHideFlags(out byte hideHealthboxes, out byte hideShadows, int transferData, int transferValue, ushort move)
    {
        if (transferData == 0)
        {
            uint flags = MoveTable.MoveTable_LoadParam(move, MoveParamFlags);
            hideHealthboxes = (byte)((flags & MoveFlagHideHealthbox) == 0 ? 1 : 0);
            hideShadows = (byte)((flags & MoveFlagHideShadows) != 0 ? 1 : 0);
            return;
        }
        switch (transferValue - 0x12)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 13:
            case 14:
            case 16:
            case 17:
            case 19:
            case 21:
                hideHealthboxes = 1;
                hideShadows = 0;
                break;
            case 18:
            case 20:
                hideHealthboxes = 1;
                hideShadows = 1;
                break;
            default:
                hideHealthboxes = 0;
                hideShadows = 0;
                break;
        }
    }
}

public static partial class BattleSystems
{
    // OFFSET 0x0223F1E8
    public static byte[]? BattleSystem_GetBgTiles(BattleSystem battleSys)
    {
        return battleSys.BgTileSnapshot;
    }

    // OFFSET 0x0223F1F0
    public static ushort[]? BattleSystem_GetBgPaletteBuffer(BattleSystem battleSys)
    {
        return battleSys.BgPaletteSnapshot;
    }
}

public static partial class BattleMessages
{
    private const uint BattleTypeFrontierLink = 0x80;
    private const int LinkTrainerID = 10000;
    private const int TrainerMessageWin = 100;
    private const int TrainerWinMsgOffset = 0x24;
    private const int TrainerLoseMsgOffset = 0x2C;
    private const int FrontierMessageArchive = 0x1A;
    private const int FrontierMessagesAllRemote = 0x266;
    private const int FrontierMessages = 0x265;
    private const int TrainerMessageHeapID = 5;

    private static EasyChatSentence BattleMessage_TrainerSentence(byte[] trainer, int offset)
    {
        EasyChatSentence sentence = new();
        sentence.Type = (ushort)Nitro.Bin.U16(trainer, offset);
        sentence.ID = (ushort)Nitro.Bin.U16(trainer, offset + 2);
        sentence.Words[0] = (ushort)Nitro.Bin.U16(trainer, offset + 4);
        sentence.Words[1] = (ushort)Nitro.Bin.U16(trainer, offset + 6);
        return sentence;
    }

    // OFFSET 0x0223F9FC
    public static byte BattleMessage_PrintTrainerMessage(BattleSystem battleSys, int trainerID, int battler, int messageType, int renderDelay)
    {
        Window window = BattleSystems.BattleSystem_GetWindow(battleSys, 0);
        int printerID;
        if ((battleSys.BattleType & BattleTypeFrontierLink) != 0)
        {
            if (trainerID == LinkTrainerID)
            {
                int offset = messageType == TrainerMessageWin ? TrainerWinMsgOffset : TrainerLoseMsgOffset;
                GameString str = EasyChatSentence.EasyChatSentence_ToString(BattleMessage_TrainerSentence(battleSys.Trainers[battler], offset), TrainerMessageHeapID)!;
                Bg.Window_FillTilemap(window, 0xFF);
                printerID = Text.Text_AddPrinterWithParams(window, FontMessage, str, 0, 0, renderDelay, BattleMessage_Callback);
                GameString.String_Free(str);
            }
            else
            {
                int entryID = messageType == TrainerMessageWin ? trainerID * 3 + 1 : trainerID * 3 + 2;
                int i;
                for (i = 0; i < 4; i++)
                {
                    if (TrainerInfo.TrainerInfo_GameCode(battleSys.TrainerInfos[i]!) == 0)
                    {
                        break;
                    }
                }
                int bankID = i == 4 ? FrontierMessagesAllRemote : FrontierMessages;
                MessageLoader loader = MessageLoader.MessageLoader_Init(0, FrontierMessageArchive, bankID, TrainerMessageHeapID)!;
                GameString str = MessageLoader.MessageLoader_GetNewString(loader, (uint)entryID)!;
                Bg.Window_FillTilemap(window, 0xFF);
                printerID = Text.Text_AddPrinterWithParams(window, FontMessage, str, 0, 0, renderDelay, BattleMessage_Callback);
                GameString.String_Free(str);
                MessageLoader.MessageLoader_Free(loader);
            }
        }
        else
        {
            TrainerData.Trainer_LoadMessage(trainerID, messageType, battleSys.MsgBuffer!, TrainerMessageHeapID);
            Bg.Window_FillTilemap(window, 0xFF);
            printerID = Text.Text_AddPrinterWithParams(window, FontMessage, battleSys.MsgBuffer, 0, 0, renderDelay, BattleMessage_Callback);
        }
        return (byte)printerID;
    }
}
