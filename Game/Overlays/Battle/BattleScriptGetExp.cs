namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const uint GameLanguageAddress = 0x020E4C44;
    private const int ExpStateEnd = 0x26;
    private const int ExpStateNextMon = 0x25;
    private const int ExpStateGauge = 3;
    private const int ExpStateWaitGauge = 4;
    private const int ExpStateCheckLevelUp = 5;
    private const int ExpStateLevelUp = 6;
    private const int ExpStateWaitLevelMessage = 7;
    private const int ExpStateShowIcon = 8;
    private const int ExpStateOpenWindow = 9;
    private const int ExpStateShowGains = 0xA;
    private const int ExpStateWaitGains = 0xB;
    private const int ExpStateShowTotals = 0xC;
    private const int ExpStateWaitTotals = 0xD;
    private const int ExpStateCloseWindow = 0xE;
    private const int ExpStateLearnMove = 0xF;
    private const int ExpStateTryLearnMove = 0x10;
    private const int ExpStateForgetMoveMessage = 0x16;
    private const int ExpStateForgetSlotChosen = 0x18;
    private const int ExpStateStopLearning = 0x1F;
    private const int ExpStateGaveUpLearning = 0x23;
    private const int ExpStateWaitLearned = 0x24;
    private const int HoldEffectLuckyEgg = 0x42;
    private const int HoldEffectMachoBrace = 0x32;
    private const int HoldEffectPowerHP = 0x7A;
    private const int HoldEffectPowerAttack = 0x75;
    private const int HoldEffectPowerDefense = 0x76;
    private const int HoldEffectPowerSpeed = 0x79;
    private const int HoldEffectPowerSpAttack = 0x77;
    private const int HoldEffectPowerSpDefense = 0x78;
    private const int ItemParamHoldEffectParam = 2;
    private const int MonDataExp = 8;
    private const int MonDataLanguage = 0xC;
    private const int MonDataHPEV = 0xD;
    private const int MonDataCurrentHP = 0xA3;
    private const int MonDataMove1PPUps = 0x3E;
    private const int MonDataShowGender = 0xB0;
    private const int MonDataGender = 0x6F;
    private const int SpeciesDataBaseHPEV = 0xA;
    private const int MaxLevelValue = 100;
    private const int MaxTotalEVs = 0x1FE;
    private const int MaxStatEV = 0xFF;
    private const uint BattleTypeTrainerFlag = 1;
    private const uint BattleTypeDoublesFlag = 2;
    private const uint BattleTypeAIPartnerFlag = 0x40;
    private const ushort SeqVictoryWild = 0x467;
    private const int SeqConfirm = 0x5DC;
    private const int RedHPSoundExp = 2;
    private const int StatusEffectLevelUp = 8;
    private const int ExpMessageTimer = 7;
    private const int MsgExpGained = 1;
    private const int MsgExpGainedBoosted = 2;
    private const int MsgLevelUp = 3;
    private const int MsgLearnedMove = 4;
    private const int MsgStatName = 0x3B3;
    private const int MsgStatGain = 0x3B4;
    private const int MsgStatTotal = 0x3B5;
    private const int MsgWantsToLearn = 0x49A;
    private const int MsgCannotLearnMore = 0x49B;
    private const int MsgDeleteMovePrompt = 0x49C;
    private const int MsgWhichMove = 0x49F;
    private const int MsgStopLearning = 0x4A0;
    private const int MsgStopLearningPrompt = 0x4A1;
    private const int MsgDidNotLearn = 0x4A4;
    private const int MsgOneTwoPoof = 0x4A5;
    private const int MsgForgotMove = 0x4A6;
    private const int MsgAnd = 0x4A7;
    private const int MsgLearnedInstead = 0x4A8;
    private const int MsgLevelMale = 0x3B0;
    private const int MsgLevelFemale = 0x3B1;
    private const int MsgLevelNone = 0x3B2;
    private const int TagNone = 0;
    private const int TagStat = 4;
    private const int TagNumber = 7;
    private const int TagNicknameMove = 0xA;
    private const int TagNicknameNumber = 0x11;
    private const ushort LevelUpMoveNoSlot = 0xFFFF;
    private const ushort LevelUpMoveKnown = 0xFFFE;
    private const int YesNoDeleteMove = 1;
    private const int YesNoStopLearning = 2;
    private const byte MenuCancelled = 0xFF;
    private const int ConfirmKeys = 0xC03;
    private const int StatWindowBgLayer = 2;
    private const int ResourceLevelUpChar = 0x4E35;
    private const int ResourceLevelUpPltt = 0x4E30;
    private const int ResourceLevelUpCell = 0x4E2D;
    private const int ResourceIconChar = 0x4E36;
    private const int ResourceIconPltt = 0x4E31;
    private const int ResourceIconCell = 0x4E2E;
    private const int NarcBattleObj = 0x1B;
    private const int NarcPokemonIcons = 0x13;
    private const uint LevelUpFrameTemplate = 0x0226E6C4;
    private const uint MonIconTemplate = 0x0226E6F8;
    private const int VramMain = 1;
    private const int PaletteBufferMainObj = 2;
    private const int GetExpHeapID = 5;
    private static readonly int[] StatParams = { 0xA4, 0xA5, 0xA6, 0xA8, 0xA9, 0xA7 };
    private static readonly int[] StatNames = { 8, 1, 2, 4, 5, 3 };

    private static BattleMessage BattleScript_ExpMessage(int id, int tags)
    {
        BattleMessage msg = new();
        msg.Id = (ushort)id;
        msg.Tags = (byte)tags;
        return msg;
    }

    private static void BattleScript_ExpPrint(GetExpTask task, MessageLoader msgLoader, BattleMessage msg)
    {
        task.PrinterID = BattleMessages.BattleMessage_Print(task.BattleSys!, msgLoader, msg, BattleMessages.BattleSystem_GetTextSpeed(task.BattleSys!));
    }

    // OFFSET 0x02248E74
    private static void BattleScript_GetExpTask(SysTask sysTask, object? param)
    {
        GetExpTask task = (GetExpTask)param!;
        BattleSystem battleSys = task.BattleSys!;
        BattleContext battleCtx = task.BattleCtx!;
        MessageLoader msgLoader = BattleSystems.BattleSystem_GetMessageLoader(battleSys)!;
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int battler = 0;
        int expSide = (battleCtx.FaintedMon >> 1) & 1;
        int slot = task.ExpPartySlot;
        Pokemon mon = null!;
        BattleMessage msg;
        for (; slot < BattleSystems.BattleSystem_GetPartyCount(battleSys, 0); slot++)
        {
            mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, 0, slot);
            if (Item.Item_LoadParam((ushort)Pokemon.Pokemon_GetValue(mon, MonDataHeldItem, null), ItemParamHoldEffect, GetExpHeapID) == HoldEffectExpShare)
            {
                break;
            }
            if ((HealthBar.FlagIndex(slot) & battleCtx.SideGetExpMask[expSide]) != 0)
            {
                break;
            }
        }
        if (slot == BattleSystems.BattleSystem_GetPartyCount(battleSys, 0))
        {
            task.State = ExpStateEnd;
        }
        else if ((battleType & BattleTypeDoublesFlag) != 0 && (battleType & BattleTypeAIPartnerFlag) == 0 && battleCtx.SelectedPartySlot[2] == slot)
        {
            battler = 2;
        }
        switch (task.State)
        {
            case 0:
                {
                    int itemEffect = Item.Item_LoadParam((ushort)Pokemon.Pokemon_GetValue(mon, MonDataHeldItem, null), ItemParamHoldEffect, GetExpHeapID);
                    uint trainer = battleType & BattleTypeTrainerFlag;
                    if (trainer == 0
                        && battleCtx.BattleMons[1].CurHP + battleCtx.BattleMons[3].CurHP == 0
                        && Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null) != 0
                        && battleCtx.ExpJinglePlayed == 0)
                    {
                        SoundPlayback.Sound_PlayBGM(SeqVictoryWild);
                        battleCtx.ExpJinglePlayed = 1;
                        Battle.BattleSystem_SetRedHPSoundFlag(battleSys, RedHPSoundExp);
                    }
                    int exp = 0;
                    msg = BattleScript_ExpMessage(MsgExpGained, 0);
                    if (Pokemon.Pokemon_GetValue(mon, MonDataCurrentHP, null) != 0 && Pokemon.Pokemon_GetValue(mon, MonDataLevel, null) != MaxLevelValue)
                    {
                        if ((HealthBar.FlagIndex(slot) & battleCtx.SideGetExpMask[expSide]) != 0)
                        {
                            exp = battleCtx.GainedExp;
                        }
                        if (itemEffect == HoldEffectExpShare)
                        {
                            exp += battleCtx.SharedExp;
                        }
                        if (itemEffect == HoldEffectLuckyEgg)
                        {
                            exp = exp * 150 / 100;
                        }
                        if (trainer != 0)
                        {
                            exp = exp * 150 / 100;
                        }
                        if (BattleContexts.BattleSystem_PokemonIsOT(battleSys, mon) == 0)
                        {
                            if (RomData.Arm9(GameLanguageAddress, 1)[0] != Pokemon.Pokemon_GetValue(mon, MonDataLanguage, null))
                            {
                                exp = exp * 170 / 100;
                            }
                            else
                            {
                                exp = exp * 150 / 100;
                            }
                            msg.Id = MsgExpGainedBoosted;
                        }
                        uint curExp = Pokemon.Pokemon_GetValue(mon, MonDataExp, null);
                        task.ExpDelta = (int)(curExp - Pokemon.Pokemon_GetCurrentLevelBaseExp(mon));
                        curExp += (uint)exp;
                        if (slot == battleCtx.SelectedPartySlot[battler])
                        {
                            battleCtx.BattleMons[battler].Exp = curExp;
                        }
                        Pokemon.Pokemon_SetValue(mon, MonDataExp, curExp);
                        BattleMon fainted = battleCtx.BattleMons[battleCtx.FaintedMon];
                        BattleScript_GainEVs(BattleSystems.BattleSystem_GetParty(battleSys, battler), slot, fainted.Species, fainted.FormNum);
                    }
                    if (exp != 0)
                    {
                        msg.Tags = TagNicknameNumber;
                        msg.Params[0] = (slot << 8) | battler;
                        msg.Params[1] = exp;
                        BattleScript_ExpPrint(task, msgLoader, msg);
                        task.Timer = ExpMessageTimer;
                        task.State++;
                    }
                    else
                    {
                        task.State = ExpStateNextMon;
                    }
                    break;
                }
            case 1:
                if (Text.Text_IsPrinterActive((byte)task.PrinterID) == 0)
                {
                    task.State++;
                }
                break;
            case 2:
                if (--task.Timer == 0)
                {
                    task.State++;
                }
                break;
            case ExpStateGauge:
                if (slot == battleCtx.SelectedPartySlot[battler])
                {
                    BattleIO.BattleIO_UpdateExpGauge(battleSys, battleCtx, battler, task.ExpDelta);
                    task.ExpDelta = 0;
                    task.State++;
                }
                else
                {
                    task.State = ExpStateCheckLevelUp;
                }
                break;
            case ExpStateWaitGauge:
                if (BattleIO.BattleIO_QueueIsEmpty(battleCtx) != 0)
                {
                    task.State++;
                }
                break;
            case ExpStateCheckLevelUp:
                if (Pokemon.Pokemon_ShouldLevelUp(mon) != 0)
                {
                    if (slot == battleCtx.SelectedPartySlot[battler])
                    {
                        BattleIO.BattleIO_PlayStatusEffect(battleSys, battleCtx, battler, StatusEffectLevelUp);
                        BattleIO.BattleIO_PlayLevelUpAnimation(battleSys, battler);
                    }
                    task.State = ExpStateLevelUp;
                }
                else
                {
                    task.State = ExpStateNextMon;
                }
                break;
            case ExpStateLevelUp:
                {
                    if (BattleIO.BattleIO_QueueIsEmpty(battleCtx) == 0)
                    {
                        break;
                    }
                    int level = (int)Pokemon.Pokemon_GetValue(mon, MonDataLevel, null);
                    int[] oldStats = new int[6];
                    battleCtx.TmpData = oldStats;
                    for (int i = 0; i < 6; i++)
                    {
                        oldStats[i] = (int)Pokemon.Pokemon_GetValue(mon, StatParams[i], null);
                    }
                    Pokemon.Pokemon_UpdateFriendship(mon, 0, (ushort)BattleSystems.BattleSystem_GetMapLabelTextID(battleSys));
                    Pokemon.Pokemon_CalcStats(mon);
                    if (slot == battleCtx.SelectedPartySlot[battler])
                    {
                        BattleControllers.BattleSystem_ReloadMonData(battleSys, battleCtx, battler, slot);
                    }
                    battleCtx.LevelUpMons |= (byte)HealthBar.FlagIndex(slot);
                    BattleIO.BattleIO_RefreshHPGauge(battleSys, battleCtx, battler);
                    msg = BattleScript_ExpMessage(MsgLevelUp, TagNicknameNumber);
                    msg.Params[0] = (slot << 8) | battler;
                    msg.Params[1] = level;
                    BattleScript_ExpPrint(task, msgLoader, msg);
                    task.State = ExpStateWaitLevelMessage;
                    break;
                }
            case ExpStateWaitLevelMessage:
                if (Text.Text_IsPrinterActive((byte)task.PrinterID) == 0)
                {
                    task.State = ExpStateShowIcon;
                    task.LearnsetIndex = 0;
                }
                break;
            case ExpStateShowIcon:
                if (slot != battleCtx.SelectedPartySlot[battler])
                {
                    BattleScript_ShowLevelUpIcon(battleSys, task, mon);
                }
                task.State = ExpStateOpenWindow;
                break;
            case ExpStateOpenWindow:
                {
                    BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(battleSys)!;
                    Window window = BattleSystems.BattleSystem_GetWindow(battleSys, 1);
                    PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(battleSys)!;
                    Video.SetBgControl(0, (ushort)((Video.GetBgControl(0) & ~3) | 2));
                    Bg.Bg_SetPriority(1, 1);
                    Bg.Bg_SetPriority(2, 0);
                    BattleSystems.BattleSystem_SetHealthboxPriority(battleSys, 2);
                    RenderWindow.LoadStandardWindowTiles(bgConfig, StatWindowBgLayer, 1, 0, GetExpHeapID);
                    PaletteData.PaletteData_LoadBufferFromFileStart(paletteData, 0x26, (int)RenderWindow.GetStandardWindowPaletteNARCMember(), GetExpHeapID, 0, 0x20, 0x80);
                    Bg.Window_Add(bgConfig, window, StatWindowBgLayer, 0x11, 7, 0xE, 0xC, 0xB, 0xA);
                    Bg.Window_FillTilemap(window, 0xFF);
                    RenderWindow.Window_DrawStandardFrame(window, 0, 1, 8);
                    task.State = ExpStateShowGains;
                    break;
                }
            case ExpStateShowGains:
                {
                    Window window = BattleSystems.BattleSystem_GetWindow(battleSys, 1);
                    int[] oldStats = (int[])battleCtx.TmpData!;
                    for (int i = 0; i < 6; i++)
                    {
                        msg = BattleScript_ExpMessage(MsgStatName, TagStat);
                        msg.Params[0] = StatNames[i];
                        BattleMessages.BattleMessage_PrintToWindow(battleSys, window, msgLoader, msg, 0, i * 16, 0, 0, 0);
                        msg.Id = MsgStatGain;
                        msg.Tags = TagNumber;
                        msg.Params[0] = (int)Pokemon.Pokemon_GetValue(mon, StatParams[i], null) - oldStats[i];
                        msg.Digits = 2;
                        BattleMessages.BattleMessage_PrintToWindow(battleSys, window, msgLoader, msg, 0x50, i * 16, 2, 0x1C, 0);
                    }
                    task.State = ExpStateWaitGains;
                    break;
                }
            case ExpStateWaitGains:
            case ExpStateWaitTotals:
                if ((Globals.gSystem.PressedKeys & ConfirmKeys) != 0 || TouchScreen.TouchScreen_Tapped())
                {
                    SoundPlayback.Sound_PlayEffect(SeqConfirm);
                    task.State++;
                }
                break;
            case ExpStateShowTotals:
                {
                    Window window = BattleSystems.BattleSystem_GetWindow(battleSys, 1);
                    Bg.Window_FillRectWithColor(window, 0xF, 0x50, 0, 0x24, 0x60);
                    for (int i = 0; i < 6; i++)
                    {
                        msg = BattleScript_ExpMessage(MsgStatTotal, TagNumber);
                        msg.Params[0] = (int)Pokemon.Pokemon_GetValue(mon, StatParams[i], null);
                        msg.Digits = 3;
                        BattleMessages.BattleMessage_PrintToWindow(battleSys, window, msgLoader, msg, 0x48, i * 16, 2, 0x24, 0);
                    }
                    task.State = ExpStateWaitTotals;
                    break;
                }
            case ExpStateCloseWindow:
                {
                    Window window = BattleSystems.BattleSystem_GetWindow(battleSys, 1);
                    RenderWindow.Window_EraseStandardFrame(window, 0);
                    Bg.Window_Remove(window);
                    Video.SetBgControl(0, (ushort)((Video.GetBgControl(0) & ~3) | 1));
                    Bg.Bg_SetPriority(1, 0);
                    Bg.Bg_SetPriority(2, 1);
                    BattleSystems.BattleSystem_SetHealthboxPriority(battleSys, 0);
                    if (slot != battleCtx.SelectedPartySlot[battler])
                    {
                        BattleScript_HideLevelUpIcon(battleSys, task);
                    }
                    battleCtx.TmpData = null;
                    task.State = ExpStateLearnMove;
                    break;
                }
            case ExpStateLearnMove:
                {
                    BattleSystems.BattleSystem_GetBgConfig(battleSys);
                    ushort move = 0;
                    ushort result = Pokemon.Pokemon_LevelUpMove(mon, ref task.LearnsetIndex, ref move);
                    if (result == 0)
                    {
                        task.State = ExpStateGauge;
                    }
                    else if (result == LevelUpMoveKnown)
                    {
                    }
                    else if (result == LevelUpMoveNoSlot)
                    {
                        task.Move = move;
                        task.State = ExpStateTryLearnMove;
                    }
                    else
                    {
                        if (slot == battleCtx.SelectedPartySlot[battler])
                        {
                            BattleControllers.BattleSystem_ReloadMonData(battleSys, battleCtx, battler, slot);
                        }
                        msg = BattleScript_ExpMessage(MsgLearnedMove, TagNicknameMove);
                        msg.Params[0] = (slot << 8) | battler;
                        msg.Params[1] = move;
                        BattleScript_ExpPrint(task, msgLoader, msg);
                        task.State = ExpStateWaitLearned;
                    }
                    break;
                }
            case ExpStateTryLearnMove:
                msg = BattleScript_ExpMessage(MsgWantsToLearn, TagNicknameMove);
                msg.Params[0] = (slot << 8) | battler;
                msg.Params[1] = task.Move;
                BattleScript_ExpPrint(task, msgLoader, msg);
                task.State++;
                break;
            case 17:
            case 19:
            case 25:
            case 27:
            case 29:
            case 32:
                if (Text.Text_IsPrinterActive((byte)task.PrinterID) == 0)
                {
                    task.State++;
                }
                break;
            case 18:
                msg = BattleScript_ExpMessage(MsgCannotLearnMore, TagNickname);
                msg.Params[0] = (slot << 8) | battler;
                BattleScript_ExpPrint(task, msgLoader, msg);
                task.State++;
                break;
            case 20:
                BattleIO.BattleIO_ShowYesNoMenu(battleSys, battleCtx, battler, MsgDeleteMovePrompt, YesNoDeleteMove, 0, 0);
                task.State++;
                break;
            case 21:
                {
                    byte answer = BattleContexts.BattleContext_IOBufferVal(battleCtx, battler);
                    if (answer == 0)
                    {
                        break;
                    }
                    if (answer == MenuCancelled)
                    {
                        task.State = ExpStateStopLearning;
                        break;
                    }
                    msg = BattleScript_ExpMessage(MsgWhichMove, TagNone);
                    BattleScript_ExpPrint(task, msgLoader, msg);
                    task.State = ExpStateForgetMoveMessage;
                    break;
                }
            case ExpStateForgetMoveMessage:
                if (Text.Text_IsPrinterActive((byte)task.PrinterID) == 0)
                {
                    BattleIO.BattleIO_ForgetMove(battleSys, battler, task.Move, slot);
                    task.State++;
                }
                break;
            case 23:
                {
                    byte answer = BattleContexts.BattleContext_IOBufferVal(battleCtx, battler);
                    if (answer == MenuCancelled)
                    {
                        task.State = ExpStateStopLearning;
                        break;
                    }
                    if (BattleContexts.BattleContext_IOBufferVal(battleCtx, battler) != 0)
                    {
                        task.ForgetSlot = battleCtx.IoBuffer[battler, 0] - 1;
                        task.State = ExpStateForgetSlotChosen;
                    }
                    break;
                }
            case ExpStateForgetSlotChosen:
                msg = BattleScript_ExpMessage(MsgOneTwoPoof, TagNone);
                BattleScript_ExpPrint(task, msgLoader, msg);
                task.State++;
                break;
            case 26:
                msg = BattleScript_ExpMessage(MsgForgotMove, TagNicknameMove);
                msg.Params[0] = (slot << 8) | battler;
                msg.Params[1] = (int)Pokemon.Pokemon_GetValue(mon, MonDataMove1 + task.ForgetSlot, null);
                BattleScript_ExpPrint(task, msgLoader, msg);
                task.State++;
                break;
            case 28:
                msg = BattleScript_ExpMessage(MsgAnd, TagNone);
                BattleScript_ExpPrint(task, msgLoader, msg);
                task.State++;
                break;
            case 30:
                msg = BattleScript_ExpMessage(MsgLearnedInstead, TagNicknameMove);
                msg.Params[0] = (slot << 8) | battler;
                msg.Params[1] = task.Move;
                BattleScript_ExpPrint(task, msgLoader, msg);
                Pokemon.Pokemon_SetValue(mon, MonDataMove1PPUps + task.ForgetSlot, 0);
                Pokemon.Pokemon_SetMoveSlot(mon, (ushort)task.Move, (byte)task.ForgetSlot);
                if (slot == battleCtx.SelectedPartySlot[battler])
                {
                    BattleControllers.BattleSystem_ReloadMonData(battleSys, battleCtx, battler, slot);
                }
                task.State = ExpStateWaitLearned;
                break;
            case ExpStateStopLearning:
                msg = BattleScript_ExpMessage(MsgStopLearning, TagNone);
                BattleScript_ExpPrint(task, msgLoader, msg);
                task.State++;
                break;
            case 33:
                BattleIO.BattleIO_ShowYesNoMenu(battleSys, battleCtx, battler, MsgStopLearningPrompt, YesNoStopLearning, task.Move, 0);
                task.State++;
                break;
            case 34:
                {
                    byte answer = BattleContexts.BattleContext_IOBufferVal(battleCtx, battler);
                    if (answer == 0)
                    {
                        break;
                    }
                    if (answer == MenuCancelled)
                    {
                        task.State = ExpStateTryLearnMove;
                        break;
                    }
                    msg = BattleScript_ExpMessage(MsgDidNotLearn, TagNicknameMove);
                    msg.Params[0] = (slot << 8) | battler;
                    msg.Params[1] = task.Move;
                    BattleScript_ExpPrint(task, msgLoader, msg);
                    task.State = ExpStateGaveUpLearning;
                    break;
                }
            case ExpStateGaveUpLearning:
            case ExpStateWaitLearned:
                if (Text.Text_IsPrinterActive((byte)task.PrinterID) == 0)
                {
                    task.State = ExpStateLearnMove;
                }
                break;
            case ExpStateNextMon:
                battleCtx.SideGetExpMask[expSide] &= ~HealthBar.FlagIndex(slot);
                task.ExpPartySlot = slot + 1;
                task.State = 0;
                break;
            case ExpStateEnd:
                battleCtx.TaskData = null;
                SysTask.SysTask_Done(sysTask);
                break;
        }
    }

    // OFFSET 0x022499C0
    private static void BattleScript_GainEVs(Party party, int slot, int species, int form)
    {
        byte[] speciesData = SpeciesData.SpeciesData_FromMonForm(species, form, GetExpHeapID);
        Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, slot);
        ushort item = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataHeldItem, null);
        int itemEffect = Item.Item_LoadParam(item, ItemParamHoldEffect, GetExpHeapID);
        int itemPower = Item.Item_LoadParam(item, ItemParamHoldEffectParam, GetExpHeapID);
        byte[] evs = new byte[6];
        int totalEVs = 0;
        for (int stat = 0; stat < 6; stat++)
        {
            evs[stat] = (byte)Pokemon.Pokemon_GetValue(mon, MonDataHPEV + stat, null);
            totalEVs = (ushort)(totalEVs + evs[stat]);
        }
        int[] powerItems = { HoldEffectPowerHP, HoldEffectPowerAttack, HoldEffectPowerDefense, HoldEffectPowerSpeed, HoldEffectPowerSpAttack, HoldEffectPowerSpDefense };
        for (int stat = 0; stat < 6 && totalEVs < MaxTotalEVs; stat++)
        {
            short gain = (short)SpeciesData.SpeciesData_GetValue(speciesData, SpeciesDataBaseHPEV + stat);
            if (itemEffect == powerItems[stat])
            {
                gain = (short)(gain + itemPower);
            }
            if (Pokemon.Party_MaskMonsWithPokerus(party, (byte)HealthBar.FlagIndex(slot)) != 0)
            {
                gain = (short)(gain * 2);
            }
            if (itemEffect == HoldEffectMachoBrace)
            {
                gain = (short)(gain * 2);
            }
            if (totalEVs + gain > MaxTotalEVs)
            {
                gain = (short)(gain - (totalEVs + gain - MaxTotalEVs));
            }
            if (evs[stat] + gain > MaxStatEV)
            {
                gain = (short)(gain - (evs[stat] + gain - MaxStatEV));
            }
            evs[stat] = (byte)(evs[stat] + gain);
            totalEVs = (ushort)(totalEVs + gain);
            Pokemon.Pokemon_SetValue(mon, MonDataHPEV + stat, evs[stat]);
        }
        SpeciesData.SpeciesData_Free(speciesData);
    }

    // OFFSET 0x0224B520
    private static void BattleScript_ShowLevelUpIcon(BattleSystem battleSys, GetExpTask task, Pokemon mon)
    {
        MessageLoader msgLoader = BattleSystems.BattleSystem_GetMessageLoader(battleSys)!;
        GameString msgBuffer = BattleSystems.BattleSystem_GetMessageBuffer(battleSys)!;
        StringTemplate strTemplate = BattleSystems.BattleSystem_GetStringTemplate(battleSys)!;
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(battleSys)!;
        SpriteSystem spriteSys = BattleSystems.BattleSystem_GetSpriteSystem(battleSys)!;
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(battleSys)!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(battleSys)!;
        SpriteSystem.SpriteSystem_LoadCharResObj(spriteSys, spriteMan, NarcBattleObj, 0x100, 1, VramMain, ResourceLevelUpChar);
        SpriteSystem.SpriteSystem_LoadPaletteBuffer(paletteData, PaletteBufferMainObj, spriteSys, spriteMan, NarcBattleObj, 0x52, 0, 2, VramMain, ResourceLevelUpPltt);
        SpriteSystem.SpriteSystem_LoadCellResObj(spriteSys, spriteMan, NarcBattleObj, 0x101, 1, ResourceLevelUpCell);
        SpriteSystem.SpriteSystem_LoadAnimResObj(spriteSys, spriteMan, NarcBattleObj, 0x102, 1, ResourceLevelUpCell);
        task.LevelUpFrame = SpriteSystem.SpriteSystem_NewSprite(spriteSys, spriteMan, BattleSubscreens.RomSpriteTemplate(LevelUpFrameTemplate));
        SpriteSystem.ManagedSprite_TickFrame(task.LevelUpFrame!);
        SpriteSystem.SpriteSystem_LoadCharResObjAtEndWithHardwareMappingType(spriteSys, spriteMan, NarcPokemonIcons, (int)PokemonIcon.Pokemon_IconSpriteIndex(mon), 0, VramMain, ResourceIconChar);
        SpriteSystem.SpriteSystem_LoadPaletteBuffer(paletteData, PaletteBufferMainObj, spriteSys, spriteMan, NarcPokemonIcons, (int)PokemonIcon.PokeIconPalettesFileIndex(), 0, 3, VramMain, ResourceIconPltt);
        SpriteSystem.SpriteSystem_LoadCellResObj(spriteSys, spriteMan, NarcPokemonIcons, (int)PokemonIcon.PokeIcon64KCellsFileIndex(), 0, ResourceIconCell);
        SpriteSystem.SpriteSystem_LoadAnimResObj(spriteSys, spriteMan, NarcPokemonIcons, (int)PokemonIcon.PokeIcon64KAnimationFileIndex(), 0, ResourceIconCell);
        task.MonIcon = SpriteSystem.SpriteSystem_NewSprite(spriteSys, spriteMan, BattleSubscreens.RomSpriteTemplate(MonIconTemplate));
        Sprite.Sprite_SetExplicitPaletteOffsetAutoAdjust(task.MonIcon!.Sprite!, PokemonIcon.Pokemon_IconPaletteIndex(mon));
        SpriteSystem.ManagedSprite_TickFrame(task.MonIcon);
        task.FontOAMMan = FontOAMSystem.FontOAMManager_New(1, GetExpHeapID);
        int gender = Pokemon.Pokemon_GetValue(mon, MonDataShowGender, null) == 0 ? 2 : (int)Pokemon.Pokemon_GetValue(mon, MonDataGender, null);
        GameString fmt;
        if (gender == 0)
        {
            fmt = MessageLoader.MessageLoader_GetNewString(msgLoader, MsgLevelMale)!;
        }
        else if (gender == 1)
        {
            fmt = MessageLoader.MessageLoader_GetNewString(msgLoader, MsgLevelFemale)!;
        }
        else
        {
            fmt = MessageLoader.MessageLoader_GetNewString(msgLoader, MsgLevelNone)!;
        }
        StringTemplate.StringTemplate_SetNickname(strTemplate, 0, Pokemon.Pokemon_GetBoxPokemon(mon));
        StringTemplate.StringTemplate_SetNumber(strTemplate, 1, (int)Pokemon.Pokemon_GetValue(mon, MonDataLevel, null), 3, 0, 1);
        StringTemplate.StringTemplate_Format(strTemplate, msgBuffer, fmt);
        GameString.String_Free(fmt);
        Window window = new();
        Bg.Window_Init(window);
        Bg.Window_AddToTopLeftCorner(bgConfig, window, 0xC, 4, 0, 0);
        Text.Text_AddPrinterWithParamsAndColor(window, 0, msgBuffer, 0, 0, 0xFF, 0x10200, null);
        int size = FontOAMSystem.FontOAM_CalcCharSize(window, VramMain, GetExpHeapID);
        CharTransferAllocation allocation = new();
        CharTransfer.CharTransfer_AllocRange(size, true, VramMain, allocation);
        FontOAMInitData init = new();
        init.Manager = task.FontOAMMan;
        init.Window = window;
        init.SpriteList = SpriteSystem.SpriteManager_GetSpriteList(spriteMan);
        init.PaletteProxy = SpriteSystem.SpriteManager_FindPlttResourceProxy(spriteMan, ResourceLevelUpPltt);
        init.Parent = null;
        init.CharOffset = (uint)allocation.Offset;
        init.X = 0xB0;
        init.Y = 8;
        init.BgPriority = 0;
        init.Priority = 0x64;
        init.VramType = VramMain;
        init.HeapID = GetExpHeapID;
        task.LevelText = FontOAMSystem.FontOAM_Create(init);
        task.LevelTextChars.Size = allocation.Size;
        task.LevelTextChars.Offset = allocation.Offset;
        task.LevelTextChars.VramType = allocation.VramType;
        task.LevelTextChars.AtEnd = allocation.AtEnd;
        FontOAMSystem.FontOAM_SetExplicitPaletteOffsetAutoAdjust(task.LevelText, 1);
        Bg.Window_Remove(window);
    }

    // OFFSET 0x0224B7CC
    private static void BattleScript_HideLevelUpIcon(BattleSystem battleSys, GetExpTask task)
    {
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(battleSys)!;
        SpriteSystem.Sprite_DeleteAndFreeResources(task.LevelUpFrame!);
        SpriteSystem.Sprite_DeleteAndFreeResources(task.MonIcon!);
        FontOAMSystem.FontOAM_Delete(task.LevelText);
        CharTransfer.CharTransfer_ClearRange(task.LevelTextChars);
        SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, ResourceLevelUpChar);
        SpriteSystem.SpriteManager_UnloadPlttObjById(spriteMan, ResourceLevelUpPltt);
        SpriteSystem.SpriteManager_UnloadCellObjById(spriteMan, ResourceLevelUpCell);
        SpriteSystem.SpriteManager_UnloadAnimObjById(spriteMan, ResourceLevelUpCell);
        SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, ResourceIconChar);
        SpriteSystem.SpriteManager_UnloadPlttObjById(spriteMan, ResourceIconPltt);
        SpriteSystem.SpriteManager_UnloadCellObjById(spriteMan, ResourceIconCell);
        SpriteSystem.SpriteManager_UnloadAnimObjById(spriteMan, ResourceIconCell);
        FontOAMSystem.FontOAMManager_Free(task.FontOAMMan);
    }
}

public static partial class BattleSystems
{
    // OFFSET 0x0223F8DC
    public static void BattleSystem_SetHealthboxPriority(BattleSystem battleSys, int priority)
    {
        for (int i = 0; i < battleSys.MaxBattlers; i++)
        {
            Healthboxes.Healthbox_SetExplicitPriority(Battlers.Battler_GetHealthbox(battleSys.Battlers[i]!), priority);
        }
    }
}

public static partial class BattleIO
{
    private const int CommandUpdateExpGauge = 0x19;
    private const int CommandPlayLevelUpAnimation = 0x24;
    private const int CommandForgetMove = 0x2B;
    private const int MonDataSpeciesValue = 5;
    private const int MonDataLevelNumber = 0xA1;

    // OFFSET 0x02265D14
    public static void BattleIO_UpdateExpGauge(BattleSystem battleSys, BattleContext battleCtx, int battler, int expDelta)
    {
        byte[] message = new byte[0x10];
        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, battleCtx.SelectedPartySlot[battler]);
        int species = (int)Pokemon.Pokemon_GetValue(mon, MonDataSpeciesValue, null);
        int level = (int)Pokemon.Pokemon_GetValue(mon, MonDataLevelNumber, null);
        W8(message, 0, CommandUpdateExpGauge);
        W32(message, 4, (uint)expDelta);
        W32(message, 8, battleCtx.BattleMons[battler].Exp - Pokemon.Species_GetExpForLevel(species, level));
        uint nextLevel = Pokemon.Species_GetExpForLevel(species, level + 1);
        W32(message, 0xC, nextLevel - Pokemon.Species_GetExpForLevel(species, level));
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 0x10);
    }

    // OFFSET 0x0226614C
    public static void BattleIO_PlayLevelUpAnimation(BattleSystem battleSys, int battler)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandPlayLevelUpAnimation);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x022664C4
    public static void BattleIO_ForgetMove(BattleSystem battleSys, int battler, int move, int partySlot)
    {
        BattleIO_ClearBuffer(BattleSystems.BattleSystem_GetBattleContext(battleSys)!, battler);
        byte[] message = new byte[4];
        W8(message, 0, CommandForgetMove);
        W16(message, 2, move);
        W8(message, 1, partySlot);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }
}
