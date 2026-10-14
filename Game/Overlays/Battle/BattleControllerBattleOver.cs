namespace PlatinumSharp.Game;

public static partial class BattleControllers
{
    private const uint BattleTypeDoublesMask = 2;
    private const uint BattleTypeMultiMask = 0x18;
    private const uint BattleTypeTagMask = 0x10;
    private const uint BattleTypeMultiPartner = 8;
    private const uint BattleTypeNoReplacePrompt = 0x86;
    private const uint BattleTypeTrainerMask = 1;
    private const uint BattleTypeLinkMask = 4;
    private const uint BattleTypeFrontierMask = 0x80;
    private const uint BattlerStatusSwitchingFlag = 1;
    private const int SpeciesEggValue = 0x1EE;
    private const int MonDataSpeciesEggValue = 0xAE;
    private const int MonDataHPValue = 0xA3;
    private const int NoSelectedSlot = 6;
    private const int SubseqReplacePrompt = 0xE7;
    private const int SubseqReplaceFainted = 0xA;
    private const int ResultWinFlag = 1;
    private const int ResultLoseFlag = 2;
    private const int TrainerClassFirst = 0x3E;
    private const int TrainerClassRange = 0x28;
    private const ushort BgmVictoryTrainer = 0x468;
    private const ushort BgmVictoryGymLeader = 0x469;
    private const ushort BgmVictoryGalactic = 0x46A;
    private const ushort BgmVictoryEliteFour = 0x46B;
    private const ushort BgmVictoryChampion = 0x46C;
    private const ushort BgmVictoryFrontierBrain = 0x46D;
    private const ushort BgmVictoryCynthia = 0x4B3;
    private const int RedHPSoundOff = 2;

    private static int BattleController_PartyHPTotal(Party party, int excludedSlot)
    {
        int total = 0;
        for (int slot = 0; slot < Party.Party_GetCurrentCount(party); slot++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, slot);
            if (Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEggValue, null) != 0 && Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEggValue, null) != SpeciesEggValue)
            {
                uint hp = Pokemon.Pokemon_GetValue(mon, MonDataHPValue, null);
                if (hp != 0 && slot != excludedSlot)
                {
                    total += (int)hp;
                }
            }
        }
        return total;
    }

    // OFFSET 0x0225079C
    private static int BattleController_ReplaceFainted(BattleSystem battleSys, BattleContext battleCtx)
    {
        int result = 0;
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int command = battleCtx.Command;
        for (int battler = 0; battler < maxBattlers; battler++)
        {
            battleCtx.BattlerStatusFlags[battler] &= ~BattlerStatusSwitchingFlag;
            bool doubles = ((battleType & BattleTypeDoublesMask) != 0 && (battleType & BattleTypeMultiMask) == 0)
                || ((battleType & BattleTypeTagMask) != 0 && BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) == 0);
            int hp = battleCtx.BattleMons[battler].CurHP;
            int total;
            if (doubles)
            {
                if ((hp == 0 && battleCtx.BattleMons[battler ^ 2].CurHP == 0 && (battler & 2) != 0) || hp != 0)
                {
                    continue;
                }
                Party party = BattleSystems.BattleSystem_GetParty(battleSys, battler);
                BattleSystems.BattleSystem_GetBattlerData(battleSys, battler);
                total = BattleController_PartyHPTotal(party, battleCtx.SelectedPartySlot[battler ^ 2]);
            }
            else
            {
                if (hp != 0)
                {
                    continue;
                }
                Party party = BattleSystems.BattleSystem_GetParty(battleSys, battler);
                BattleSystems.BattleSystem_GetBattlerData(battleSys, battler);
                total = BattleController_PartyHPTotal(party, -1);
            }
            if (total == 0)
            {
                battleCtx.BattlersSwitchingMask |= (byte)HealthBar.FlagIndex(battler);
                battleCtx.SelectedPartySlot[battler] = NoSelectedSlot;
            }
            else
            {
                battleCtx.CommandNext = command;
                battleCtx.Command = ControllerCommandRunScript;
                battleCtx.BattlerStatusFlags[battler] |= BattlerStatusSwitchingFlag;
            }
        }
        if (battleCtx.Command == ControllerCommandRunScript)
        {
            if ((battleType & BattleTypeNoReplacePrompt) == 0
                && BattleSystems.BattleSystem_BattleStyle(battleSys) == 0
                && !((battleCtx.BattlerStatusFlags[0] & BattlerStatusSwitchingFlag) != 0 && (battleCtx.BattlerStatusFlags[1] & BattlerStatusSwitchingFlag) != 0)
                && BattleContexts.BattleSystem_AnyReplacementMons(battleSys, battleCtx, 0) != 0)
            {
                battleCtx.ScriptTemp = (battleCtx.BattlerStatusFlags[0] & BattlerStatusSwitchingFlag) != 0 ? 0 : 1;
                BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqReplacePrompt);
            }
            else
            {
                BattleContexts.BattleSystem_LoadScript(battleCtx, ScriptNarcSubSeq, SubseqReplaceFainted);
            }
            result = 1;
        }
        return result;
    }

    // OFFSET 0x02250A48
    private static int BattleController_CheckBattleOver(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        byte result = 0;
        for (int battler = 0; battler < maxBattlers; battler++)
        {
            BattleMon mon = battleCtx.BattleMons[battler];
            if ((uint)(battleType - 0x4A) <= 1 && BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) == 0)
            {
                if (BattleSystems.BattleSystem_BattlerSlot(battleSys, battler) == 2 && mon.CurHP == 0)
                {
                    Party party = BattleSystems.BattleSystem_GetParty(battleSys, battler);
                    BattleSystems.BattleSystem_GetBattlerData(battleSys, battler);
                    if (BattleController_PartyHPTotal(party, -1) == 0)
                    {
                        result |= ResultLoseFlag;
                    }
                }
                continue;
            }
            if ((battleType & BattleTypeMultiPartner) != 0 || ((battleType & BattleTypeTagMask) != 0 && BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) != 0))
            {
                if (mon.CurHP != 0)
                {
                    continue;
                }
                Party party = BattleSystems.BattleSystem_GetParty(battleSys, battler);
                Party partnerParty = BattleSystems.BattleSystem_GetParty(battleSys, BattleSystems.BattleSystem_GetPartner(battleSys, battler));
                BattlerData data = BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!;
                int total = BattleController_PartyHPTotal(party, -1) + BattleController_PartyHPTotal(partnerParty, -1);
                if (total == 0)
                {
                    result |= (byte)((Battlers.Battler_GetType(data) & 1) != 0 ? ResultWinFlag : ResultLoseFlag);
                }
            }
            else
            {
                if (mon.CurHP != 0)
                {
                    continue;
                }
                Party party = BattleSystems.BattleSystem_GetParty(battleSys, battler);
                BattlerData data = BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!;
                if (BattleController_PartyHPTotal(party, -1) == 0)
                {
                    result |= (byte)((Battlers.Battler_GetType(data) & 1) != 0 ? ResultWinFlag : ResultLoseFlag);
                }
            }
        }
        if ((result == ResultWinFlag && (battleType & BattleTypeTrainerMask) != 0 && (battleType & BattleTypeLinkMask) == 0)
            || (result == ResultWinFlag && (battleType & BattleTypeFrontierMask) != 0 && (battleType & BattleTypeLinkMask) == 0))
        {
            int trainerClass = BattleSystems.BattleSystem_GetTrainer(battleSys, 1)[1] - TrainerClassFirst;
            ushort bgm;
            switch (trainerClass)
            {
                case 0:
                case 2:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                    bgm = BgmVictoryGymLeader;
                    break;
                case 3:
                case 4:
                case 5:
                case 6:
                    bgm = BgmVictoryFrontierBrain;
                    break;
                case 7:
                    bgm = BgmVictoryGalactic;
                    break;
                case 10:
                case 11:
                case 25:
                case 26:
                case 27:
                    bgm = BgmVictoryEliteFour;
                    break;
                case 24:
                    bgm = BgmVictoryChampion;
                    break;
                case 35:
                case 37:
                case 38:
                case 39:
                case 40:
                    bgm = BgmVictoryCynthia;
                    break;
                default:
                    bgm = BgmVictoryTrainer;
                    break;
            }
            SoundPlayback.Sound_PlayBGM(bgm);
            Battle.BattleSystem_SetRedHPSoundFlag(battleSys, RedHPSoundOff);
        }
        if (result != 0)
        {
            Battle.BattleSystem_SetResultMask(battleSys, result);
        }
        return result != 0 ? 1 : 0;
    }
}

public static partial class BattleSystems
{
    // OFFSET 0x0223EE18
    public static int BattleSystem_BattleStyle(BattleSystem battleSys)
    {
        return Options.Options_BattleStyle(battleSys.Options!);
    }
}
