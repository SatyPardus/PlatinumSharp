namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    public const int BattleMonSpecies = 0;
    public const int BattleMonAttack = 1;
    public const int BattleMonDefense = 2;
    public const int BattleMonSpeed = 3;
    public const int BattleMonSpAttack = 4;
    public const int BattleMonSpDefense = 5;
    public const int BattleMonMove1 = 6;
    public const int BattleMonHpIV = 10;
    public const int BattleMonIsEgg = 16;
    public const int BattleMonHasNickname = 17;
    public const int BattleMonStatBoost = 18;
    public const int BattleMonAbility = 26;
    public const int BattleMonType1 = 27;
    public const int BattleMonType2 = 28;
    public const int BattleMonGender = 29;
    public const int BattleMonIsShiny = 30;
    public const int BattleMonCurPP1 = 31;
    public const int BattleMonPPUps1 = 35;
    public const int BattleMonMaxPP1 = 39;
    public const int BattleMonLevel = 43;
    public const int BattleMonFriendship = 44;
    public const int BattleMonNickname = 45;
    public const int BattleMonNicknameString = 46;
    public const int BattleMonCurHP = 47;
    public const int BattleMonMaxHP = 48;
    public const int BattleMonOTName = 49;
    public const int BattleMonExp = 50;
    public const int BattleMonPersonality = 51;
    public const int BattleMonStatus = 52;
    public const int BattleMonStatusVolatile = 53;
    public const int BattleMonOTId = 54;
    public const int BattleMonHeldItem = 55;
    public const int BattleMonTimesDamaged = 56;
    public const int BattleMonTrainerMessageFlags = 57;
    public const int BattleMonOTGender = 58;
    public const int BattleMonMoveEffectsMask = 59;
    public const int BattleMonMoveEffectsTemp = 60;
    public const int BattleMonFormNum = 98;
    public const int BattleMonScriptTempParam = 100;

    private const int SpeciesArceus = 0x1ED;
    private const int AbilityMultitype = 0x79;
    private const int NicknameLength = 11;

    // OFFSET 0x02252060
    public static int BattleMon_Get(BattleContext battleCtx, int battler, int param, object? buffer)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        MoveEffectsData effects = mon.MoveEffectsData;
        switch (param)
        {
            case 0:
                return mon.Species;
            case 1:
                return mon.Attack;
            case 2:
                return mon.Defense;
            case 3:
                return mon.Speed;
            case 4:
                return mon.SpAttack;
            case 5:
                return mon.SpDefense;
            case 6:
            case 7:
            case 8:
            case 9:
                return mon.Moves[param - 6];
            case 10:
                return mon.HpIV;
            case 11:
                return mon.AttackIV;
            case 12:
                return mon.DefenseIV;
            case 13:
                return mon.SpeedIV;
            case 14:
                return mon.SpAttackIV;
            case 15:
                return mon.SpDefenseIV;
            case 16:
                return mon.IsEgg;
            case 17:
                return mon.HasNickname;
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
            case 24:
            case 25:
                return mon.StatBoosts[param - 18];
            case 26:
                return mon.Ability;
            case 27:
            case 28:
                return BattleMon_GetType(battleCtx, battler, param);
            case 29:
                return mon.Gender;
            case 30:
                return mon.IsShiny;
            case 31:
            case 32:
            case 33:
            case 34:
                return mon.PpCur[param - 31];
            case 35:
            case 36:
            case 37:
            case 38:
                return mon.PpUps[param - 35];
            case 39:
            case 40:
            case 41:
            case 42:
                return (int)MoveTable.MoveTable_CalcMaxPP(mon.Moves[param - 39], mon.PpUps[param - 39]);
            case 43:
                return mon.Level;
            case 44:
                return mon.Friendship;
            case 45:
            {
                ushort[] destination = (ushort[])buffer!;
                for (int i = 0; i < NicknameLength; i++)
                {
                    destination[i] = mon.Nickname[i];
                }
                return 0;
            }
            case 46:
                GameString.String_CopyChars((GameString)buffer!, mon.Nickname);
                return 0;
            case 47:
                return mon.CurHP;
            case 48:
                return (int)mon.MaxHP;
            case 49:
            {
                ushort[] destination = (ushort[])buffer!;
                for (int i = 0; i < mon.OTName.Length && i < destination.Length; i++)
                {
                    destination[i] = mon.OTName[i];
                }
                return 0;
            }
            case 50:
                return (int)mon.Exp;
            case 51:
                return (int)mon.Personality;
            case 52:
                return (int)mon.Status;
            case 53:
                return (int)mon.StatusVolatile;
            case 54:
                return (int)mon.OTId;
            case 55:
                return mon.HeldItem;
            case 56:
                return mon.TimesDamaged;
            case 57:
                return mon.TrainerMessageFlags;
            case 58:
                return mon.OTGender;
            case 59:
                return (int)mon.MoveEffectsMask;
            case 60:
                return (int)mon.MoveEffectsTemp;
            case 61:
                return effects.DisabledTurns;
            case 62:
                return effects.EncoredTurns;
            case 63:
                return effects.ChargedTurns;
            case 64:
                return effects.TauntedTurns;
            case 65:
                return effects.ProtectSuccessTurns;
            case 66:
                return effects.PerishSongTurns;
            case 67:
                return effects.RolloutCount;
            case 68:
                return effects.FuryCutterCount;
            case 69:
                return effects.StockpileCount;
            case 70:
                return effects.StockpileDefBoosts;
            case 71:
                return effects.StockpileSpDefBoosts;
            case 72:
                return effects.Truant;
            case 73:
                return effects.FlashFire;
            case 74:
                return effects.LockOnTarget;
            case 75:
                return effects.MimickedMoveSlot;
            case 76:
                return effects.BindTarget;
            case 77:
                return effects.MeanLookTarget;
            case 78:
                return effects.LastResortCount;
            case 79:
                return effects.MagnetRiseTurns;
            case 80:
                return effects.HealBlockTurns;
            case 81:
                return effects.EmbargoTurns;
            case 82:
                return effects.CanUnburden;
            case 83:
                return effects.MetronomeTurns;
            case 84:
                return effects.MicleBerry;
            case 85:
                return effects.CustapBerry;
            case 86:
                return effects.QuickClaw;
            case 87:
                return effects.RechargeTurnNumber;
            case 88:
                return effects.FakeOutTurnNumber;
            case 89:
                return effects.SlowStartTurnNumber;
            case 90:
                return effects.SubstituteHP;
            case 91:
                return (int)effects.TransformedPID;
            case 92:
                return effects.DisabledMove;
            case 93:
                return effects.EncoredMove;
            case 94:
                return effects.BindingMove;
            case 95:
                return effects.ItemHPRecovery;
            case 96:
                return mon.SlowStartAnnounced;
            case 97:
                return mon.SlowStartFinished;
            case 98:
                return mon.FormNum;
            case 100:
                return BattleMon_Get(battleCtx, battler, battleCtx.ScriptTemp, buffer);
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x0225B734
    public static int BattleMon_GetType(BattleContext battleCtx, int battler, int param)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        int type = 0;
        if (param == BattleMonType1)
        {
            type = mon.Type1;
        }
        else if (param == BattleMonType2)
        {
            type = mon.Type2;
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        if (mon.Species == SpeciesArceus && mon.Ability == AbilityMultitype)
        {
            throw Unported.Partial(0x0225B734);
        }
        return type;
    }

    // OFFSET 0x022599D0
    public static int BattleSystem_GetCryModulation(BattleContext battleCtx, int battler, int slot, int isSendOut)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        int modulation = 0;
        int color = HealthBar.HealthBar_Color((ushort)mon.CurHP, (ushort)mon.MaxHP, 0x30);
        if ((mon.Status & 0xFF) != 0 || (color != HealthBar.ColorFull && color != HealthBar.ColorGreen))
        {
            modulation = 11;
        }
        return modulation;
    }

    // OFFSET 0x022535E0
    public static void BattleSystem_ClearSideExpMask(BattleContext battleCtx, int battler)
    {
        battleCtx.SideGetExpMask[(battler >> 1) & 1] = 0;
    }

    // OFFSET 0x022535F0
    public static void BattleSystem_UpdateSideExpMask(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        int i = 0;
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int side = (battler >> 1) & 1;
        do
        {
            if ((HealthBar.FlagIndex(i) & battleCtx.BattlersSwitchingMask) == 0
                && (HealthBar.FlagIndex(battler) & battleCtx.BattlersSwitchingMask) == 0
                && battleCtx.BattleMons[battler].CurHP != 0)
            {
                battleCtx.SideGetExpMask[side] |= HealthBar.FlagIndex(battleCtx.SelectedPartySlot[i]);
            }
            i += 2;
            if (battleType == 0x4A || battleType == 0x4B)
            {
                break;
            }
        }
        while (i <= 2);
    }
}

public static partial class BattleSystems
{
    private const int MonDataSpeciesEggValue = 0xAE;
    private const uint SpeciesBurmyFamily = 0x19C;

    // OFFSET 0x0223EF2C
    public static void BattleSystem_SetParticipant(BattleSystem battleSys, int battler, int slot)
    {
        battleSys.BattleParticipantMask[battler] |= (byte)HealthBar.FlagIndex(slot);
    }

    // OFFSET 0x0223F938
    public static void BattleSystem_RegisterSeenPokemon(BattleSystem battleSys, int battler)
    {
        int type = Battlers.Battler_GetType(battleSys.Battlers[battler]!);
        int slot = BattleContexts.BattleContext_Get(battleSys, battleSys.BattleCtx!, 2, battler);
        Pokemon mon = BattleSystem_GetPartyPokemon(battleSys, battler, slot);
        if ((battleSys.BattleType & 0x84) == 0)
        {
            if ((type & 1) != 0 || battleSys.BattleType == 0x4A || battleSys.BattleType == 0x4B)
            {
                Pokedex.Pokedex_Encounter(battleSys.Pokedex!, mon);
            }
        }
        if ((type & 1) == 0)
        {
            if (Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEggValue, null) == SpeciesBurmyFamily)
            {
                Pokedex.Pokedex_Capture(battleSys.Pokedex!, mon);
            }
        }
    }
}
