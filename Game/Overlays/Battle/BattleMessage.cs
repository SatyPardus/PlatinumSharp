namespace PlatinumSharp.Game;

public static partial class BattleMessages
{
    public const int TagNone = 0;
    public const int TagNoneSide = 1;
    public const int TagNickname = 2;
    public const int TagMove = 3;
    public const int TagStat = 4;
    public const int TagItem = 5;
    public const int TagNumber = 6;
    public const int TagNumbers = 7;
    public const int TagTrainerName = 8;
    public const int TagCount = 0x3D;

    public const int TagSideLocal = 0x40;
    public const int TagGlobal = 0x80;
    public const int TagMask = 0x3F;

    private const int FontMessage = 1;
    private const uint BattleTypeTrainer = 1;
    private const uint BattleTypeLink = 4;
    private const int SeqFanfareLevelUp = 0x484;
    private const int SeqFanfareCapture = 0x483;
    private const int SeqEffectClick = 0x5E6;
    private const int TrainerNameLength = 8;

    private enum Arg
    {
        Nickname,
        Move,
        Item,
        Number,
        NumberDigits,
        Type,
        Ability,
        Stat,
        Status,
        Species,
        Nothing,
        Flavor,
        TrainerClass,
        TrainerName,
        PCBox,
    }

    private enum SideRule
    {
        None,
        First,
        FirstWithTrainer,
        Second,
        SecondWithTrainer,
        FirstAndSecond,
        FirstAndThird,
    }

    private static readonly Arg[]?[] TagArgs =
    {
        null,
        null,
        new[] { Arg.Nickname },
        new[] { Arg.Move },
        new[] { Arg.Stat },
        new[] { Arg.Item },
        new[] { Arg.Number },
        new[] { Arg.NumberDigits },
        new[] { Arg.TrainerName },
        new[] { Arg.Nickname, Arg.Nickname },
        new[] { Arg.Nickname, Arg.Move },
        new[] { Arg.Nickname, Arg.Ability },
        new[] { Arg.Nickname, Arg.Stat },
        new[] { Arg.Nickname, Arg.Type },
        new[] { Arg.Nickname, Arg.Species },
        new[] { Arg.Nickname, Arg.Item },
        new[] { Arg.Nickname, Arg.Nothing },
        new[] { Arg.Nickname, Arg.Number },
        new[] { Arg.Nickname, Arg.TrainerName },
        new[] { Arg.Nickname, Arg.PCBox },
        new[] { Arg.Move },
        new[] { Arg.Move, Arg.Nickname },
        new[] { Arg.Move, Arg.Move },
        new[] { Arg.Ability, Arg.Nickname },
        new[] { Arg.Item, Arg.Move },
        new[] { Arg.Number, Arg.Number },
        new[] { Arg.TrainerName, Arg.TrainerName },
        new[] { Arg.TrainerName, Arg.Nickname },
        new[] { Arg.TrainerName, Arg.Item },
        new[] { Arg.TrainerName, Arg.Number },
        new[] { Arg.TrainerClass, Arg.TrainerName },
        new[] { Arg.Nickname, Arg.Nickname, Arg.Move },
        new[] { Arg.Nickname, Arg.Nickname, Arg.Ability },
        new[] { Arg.Nickname, Arg.Nickname, Arg.Item },
        new[] { Arg.Nickname, Arg.Move, Arg.Move },
        new[] { Arg.Nickname, Arg.Move, Arg.Number },
        new[] { Arg.Nickname, Arg.Ability, Arg.Nickname },
        new[] { Arg.Nickname, Arg.Ability, Arg.Move },
        new[] { Arg.Nickname, Arg.Ability, Arg.Item },
        new[] { Arg.Nickname, Arg.Ability, Arg.Stat },
        new[] { Arg.Nickname, Arg.Ability, Arg.Type },
        new[] { Arg.Nickname, Arg.Ability, Arg.Status },
        new[] { Arg.Nickname, Arg.Ability, Arg.Number },
        new[] { Arg.Nickname, Arg.Item, Arg.Nickname },
        new[] { Arg.Nickname, Arg.Item, Arg.Move },
        new[] { Arg.Nickname, Arg.Item, Arg.Stat },
        new[] { Arg.Nickname, Arg.Item, Arg.Status },
        new[] { Arg.Nickname, Arg.PCBox, Arg.PCBox },
        new[] { Arg.Item, Arg.Nickname, Arg.Flavor },
        new[] { Arg.TrainerName, Arg.Nickname, Arg.Nickname },
        new[] { Arg.TrainerClass, Arg.TrainerName, Arg.Nickname },
        new[] { Arg.TrainerClass, Arg.TrainerName, Arg.Item },
        new[] { Arg.Nickname, Arg.Ability, Arg.Nickname, Arg.Move },
        new[] { Arg.Nickname, Arg.Ability, Arg.Nickname, Arg.Ability },
        new[] { Arg.Nickname, Arg.Ability, Arg.Nickname, Arg.Stat },
        new[] { Arg.Nickname, Arg.Item, Arg.Nickname, Arg.Item },
        new[] { Arg.TrainerName, Arg.Nickname, Arg.TrainerName, Arg.Nickname },
        new[] { Arg.TrainerClass, Arg.TrainerName, Arg.Nickname, Arg.Nickname },
        new[] { Arg.TrainerClass, Arg.TrainerName, Arg.Nickname, Arg.TrainerName },
        new[] { Arg.TrainerClass, Arg.TrainerName, Arg.TrainerClass, Arg.TrainerName },
        new[] { Arg.TrainerClass, Arg.TrainerName, Arg.Nickname, Arg.TrainerClass, Arg.TrainerName, Arg.Nickname },
    };

    private static SideRule TagSideRule(int tag)
    {
        if (tag == 1)
        {
            return SideRule.First;
        }
        if (tag == 2 || (tag >= 10 && tag <= 19) || tag == 34 || tag == 35 || (tag >= 37 && tag <= 42) || (tag >= 44 && tag <= 47))
        {
            return SideRule.FirstWithTrainer;
        }
        if (tag == 20)
        {
            return SideRule.Second;
        }
        if (tag == 21 || tag == 23 || tag == 48)
        {
            return SideRule.SecondWithTrainer;
        }
        if (tag == 9 || (tag >= 31 && tag <= 33))
        {
            return SideRule.FirstAndSecond;
        }
        if (tag == 36 || tag == 43 || (tag >= 52 && tag <= 55))
        {
            return SideRule.FirstAndThird;
        }
        return SideRule.None;
    }

    internal static Trainer TrainerFromData(byte[] data)
    {
        Trainer trainer = new();
        trainer.MonDataType = data[0];
        trainer.TrainerClass = data[1];
        for (int i = 0; i < TrainerNameLength; i++)
        {
            trainer.Name[i] = (ushort)Nitro.Bin.U16(data, TrainerData.NameOffset + i * 2);
        }
        return trainer;
    }

    // OFFSET 0x0223EDF0
    public static int BattleSystem_GetTextSpeed(BattleSystem battleSys)
    {
        if ((battleSys.BattleType & BattleTypeLink) != 0 && (battleSys.BattleStatusMask & BattleSystems.BattleStatusRecording) == 0)
        {
            return 1;
        }
        return Options.Options_TextFrameDelay(battleSys.Options!);
    }

    // OFFSET 0x0223FB24
    public static byte BattleMessage_Print(BattleSystem battleSys, MessageLoader msgLoader, BattleMessage battleMsg, int renderDelay)
    {
        Window window = BattleSystems.BattleSystem_GetWindow(battleSys, 0);
        BattleMessage_CheckSide(battleSys, battleMsg);
        BattleMessage_FillFormatBuffers(battleSys, battleMsg);
        BattleMessage_Format(battleSys, msgLoader, battleMsg);
        Bg.Window_FillTilemap(window, 0xFF);
        return (byte)Text.Text_AddPrinterWithParams(window, FontMessage, battleSys.MsgBuffer, 0, 0, renderDelay, BattleMessage_Callback);
    }

    // OFFSET 0x0223FB78
    public static byte BattleMessage_PrintToWindow(BattleSystem battleSys, Window window, MessageLoader msgLoader, BattleMessage battleMsg, int x, int y, int flags, int width, int renderDelay)
    {
        BattleMessage_CheckSide(battleSys, battleMsg);
        BattleMessage_FillFormatBuffers(battleSys, battleMsg);
        BattleMessage_Format(battleSys, msgLoader, battleMsg);
        if ((flags & 1) != 0)
        {
            Bg.Window_FillTilemap(window, 0xFF);
        }
        int offset;
        if ((flags & 2) != 0)
        {
            offset = width - Font.Font_CalcStringWidth(0, battleSys.MsgBuffer, 0);
        }
        else
        {
            offset = 0;
        }
        return (byte)Text.Text_AddPrinterWithParams(window, 0, battleSys.MsgBuffer, x + offset, y, renderDelay, BattleMessage_Callback);
    }

    // OFFSET 0x0223FBE8
    private static void BattleMessage_CheckSide(BattleSystem battleSys, BattleMessage battleMsg)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        if ((battleMsg.Tags & TagGlobal) != 0)
        {
            return;
        }
        if ((battleMsg.Tags & TagSideLocal) != 0)
        {
            if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleMsg.Battler) != 0)
            {
                battleMsg.Id++;
            }
            return;
        }
        int tag = battleMsg.Tags & TagMask;
        if (tag >= TagCount)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        switch (TagSideRule(tag))
        {
            case SideRule.First:
                if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, (byte)battleMsg.Params[0]) != 0)
                {
                    battleMsg.Id++;
                }
                break;
            case SideRule.FirstWithTrainer:
                if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, (byte)battleMsg.Params[0]) != 0)
                {
                    battleMsg.Id++;
                    if ((battleType & BattleTypeTrainer) != 0)
                    {
                        battleMsg.Id++;
                    }
                }
                break;
            case SideRule.Second:
                if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, (byte)battleMsg.Params[1]) != 0)
                {
                    battleMsg.Id++;
                }
                break;
            case SideRule.SecondWithTrainer:
                if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, (byte)battleMsg.Params[1]) != 0)
                {
                    battleMsg.Id++;
                    if ((battleType & BattleTypeTrainer) != 0)
                    {
                        battleMsg.Id++;
                    }
                }
                break;
            case SideRule.FirstAndSecond:
                BattleMessage_CheckTwoSides(battleSys, battleMsg, battleType, 1);
                break;
            case SideRule.FirstAndThird:
                BattleMessage_CheckTwoSides(battleSys, battleMsg, battleType, 2);
                break;
        }
    }

    private static void BattleMessage_CheckTwoSides(BattleSystem battleSys, BattleMessage battleMsg, uint battleType, int second)
    {
        if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, (byte)battleMsg.Params[0]) != 0)
        {
            battleMsg.Id += 3;
            if ((battleType & BattleTypeTrainer) != 0)
            {
                battleMsg.Id += 2;
            }
            if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, (byte)battleMsg.Params[second]) != 0)
            {
                battleMsg.Id++;
            }
        }
        else if (BattleSystems.BattleSystem_GetBattlerSide(battleSys, (byte)battleMsg.Params[second]) != 0)
        {
            battleMsg.Id++;
            if ((battleType & BattleTypeTrainer) != 0)
            {
                battleMsg.Id++;
            }
        }
    }

    // OFFSET 0x0223FDE4
    private static void BattleMessage_FillFormatBuffers(BattleSystem battleSys, BattleMessage battleMsg)
    {
        int tag = battleMsg.Tags & TagMask;
        if (tag >= TagCount)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        Arg[]? args = TagArgs[tag];
        if (args == null)
        {
            return;
        }
        for (int i = 0; i < args.Length; i++)
        {
            int param = battleMsg.Params[i];
            switch (args[i])
            {
                case Arg.Nickname:
                    BattleMessage_Nickname(battleSys, (uint)i, param);
                    break;
                case Arg.Move:
                    BattleMessage_MoveName(battleSys, (uint)i, param);
                    break;
                case Arg.Item:
                    BattleMessage_ItemName(battleSys, (uint)i, param);
                    break;
                case Arg.Number:
                    BattleMessage_Number(battleSys, (uint)i, param);
                    break;
                case Arg.NumberDigits:
                    BattleMessage_NumberDigits(battleSys, (uint)i, param, battleMsg.Digits);
                    break;
                case Arg.Type:
                    BattleMessage_TypeName(battleSys, (uint)i, param);
                    break;
                case Arg.Ability:
                    BattleMessage_AbilityName(battleSys, (uint)i, param);
                    break;
                case Arg.Stat:
                    BattleMessage_StatName(battleSys, (uint)i, param);
                    break;
                case Arg.Status:
                    BattleMessage_StatusName(battleSys, (uint)i, param);
                    break;
                case Arg.Species:
                    BattleMessage_SpeciesName(battleSys, (uint)i, param);
                    break;
                case Arg.Nothing:
                    BattleMessage_Nothing(battleSys, (uint)i, param);
                    break;
                case Arg.Flavor:
                    BattleMessage_FlavorName(battleSys, (uint)i, param);
                    break;
                case Arg.TrainerClass:
                    BattleMessage_TrainerClassName(battleSys, (uint)i, param);
                    break;
                case Arg.TrainerName:
                    BattleMessage_TrainerName(battleSys, (uint)i, param);
                    break;
                case Arg.PCBox:
                    BattleMessage_PCBoxName(battleSys, (uint)i, param);
                    break;
            }
        }
    }

    // OFFSET 0x02240460
    private static void BattleMessage_Nickname(BattleSystem battleSys, uint idx, int param)
    {
        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, param & 0xFF, (param & 0xFF00) >> 8);
        StringTemplate.StringTemplate_SetNickname(battleSys.StrFormatter!, idx, Pokemon.Pokemon_GetBoxPokemon(mon));
    }

    // OFFSET 0x02240484
    private static void BattleMessage_MoveName(BattleSystem battleSys, uint idx, int param)
    {
        StringTemplate.StringTemplate_SetMoveName(battleSys.StrFormatter!, idx, (uint)param);
    }

    // OFFSET 0x02240490
    private static void BattleMessage_ItemName(BattleSystem battleSys, uint idx, int param)
    {
        StringTemplate.StringTemplate_SetItemName(battleSys.StrFormatter!, idx, (uint)param);
    }

    // OFFSET 0x0224049C
    private static void BattleMessage_Number(BattleSystem battleSys, uint idx, int param)
    {
        StringTemplate.StringTemplate_SetNumber(battleSys.StrFormatter!, idx, param, 5, 0, 1);
    }

    // OFFSET 0x022404B4
    private static void BattleMessage_NumberDigits(BattleSystem battleSys, uint idx, int param, int digits)
    {
        if (digits != 0)
        {
            StringTemplate.StringTemplate_SetNumber(battleSys.StrFormatter!, idx, param, (uint)digits, 1, 1);
        }
        else
        {
            StringTemplate.StringTemplate_SetNumber(battleSys.StrFormatter!, idx, param, 5, 1, 1);
        }
    }

    // OFFSET 0x022404E0
    private static void BattleMessage_TypeName(BattleSystem battleSys, uint idx, int param)
    {
        StringTemplate.StringTemplate_SetPokemonTypeName(battleSys.StrFormatter!, idx, (uint)param);
    }

    // OFFSET 0x022404EC
    private static void BattleMessage_AbilityName(BattleSystem battleSys, uint idx, int param)
    {
        StringTemplate.StringTemplate_SetAbilityName(battleSys.StrFormatter!, idx, (uint)param);
    }

    // OFFSET 0x022404F8
    private static void BattleMessage_StatName(BattleSystem battleSys, uint idx, int param)
    {
        StringTemplate.StringTemplate_SetPokemonStatName(battleSys.StrFormatter!, idx, (uint)param);
    }

    // OFFSET 0x02240504
    private static void BattleMessage_StatusName(BattleSystem battleSys, uint idx, int param)
    {
        StringTemplate.StringTemplate_SetStatusConditionName(battleSys.StrFormatter!, idx, (uint)param);
    }

    // OFFSET 0x02240510
    private static void BattleMessage_SpeciesName(BattleSystem battleSys, uint idx, int param)
    {
        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, param & 0xFF, (param & 0xFF00) >> 8);
        StringTemplate.StringTemplate_SetSpeciesName(battleSys.StrFormatter!, idx, Pokemon.Pokemon_GetBoxPokemon(mon));
    }

    // OFFSET 0x02240534
    private static void BattleMessage_Nothing(BattleSystem battleSys, uint idx, int param)
    {
    }

    // OFFSET 0x02240538
    private static void BattleMessage_FlavorName(BattleSystem battleSys, uint idx, int param)
    {
        StringTemplate.StringTemplate_SetFlavorName(battleSys.StrFormatter!, idx, (uint)param);
    }

    // OFFSET 0x02240544
    private static void BattleMessage_TrainerClassName(BattleSystem battleSys, uint idx, int param)
    {
        Trainer trainer = TrainerFromData(BattleSystems.BattleSystem_GetTrainer(battleSys, param));
        StringTemplate.StringTemplate_SetTrainerClassNameBattle(battleSys.StrFormatter!, idx, trainer);
    }

    // OFFSET 0x0224055C
    private static void BattleMessage_TrainerName(BattleSystem battleSys, uint idx, int param)
    {
        Trainer trainer = TrainerFromData(BattleSystems.BattleSystem_GetTrainer(battleSys, param));
        StringTemplate.StringTemplate_SetTrainerNameBattle(battleSys.StrFormatter!, idx, trainer);
    }

    // OFFSET 0x02240574
    private static void BattleMessage_PCBoxName(BattleSystem battleSys, uint idx, int param)
    {
        StringTemplate.StringTemplate_SetPCBoxName(battleSys.StrFormatter!, idx, battleSys.PcBoxes!, (uint)param);
    }

    // OFFSET 0x02240584
    private static void BattleMessage_Format(BattleSystem battleSys, MessageLoader msgLoader, BattleMessage battleMsg)
    {
        GameString? format = MessageLoader.MessageLoader_GetNewString(msgLoader, battleMsg.Id);
        StringTemplate.StringTemplate_Format(battleSys.StrFormatter!, battleSys.MsgBuffer!, format!);
        GameString.String_Free(format);
    }

    // OFFSET 0x022405A4
    private static bool BattleMessage_Callback(TextPrinter printer, ushort argument)
    {
        bool result = false;
        switch (argument)
        {
            case 1:
                result = SoundPlayback.Sound_IsAnyEffectPlaying();
                break;
            case 2:
                result = SoundPlayback.Sound_IsBGMPausedByFanfare() != 0;
                break;
            case 3:
                SoundPlayback.Sound_PlayFanfare(SeqFanfareLevelUp);
                break;
            case 4:
                SoundPlayback.Sound_PlayEffect(SeqEffectClick);
                break;
            case 5:
                SoundPlayback.Sound_PlayFanfare(SeqFanfareCapture);
                break;
        }
        return result;
    }
}
