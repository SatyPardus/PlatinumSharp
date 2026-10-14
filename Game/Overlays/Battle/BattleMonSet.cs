namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const int StatBoostMax = 12;
    private const int FriendshipMax = 0xFF;

    // OFFSET 0x022523E8
    public static void BattleMon_Set(BattleContext battleCtx, int battler, int param, int value, ushort[]? buffer = null)
    {
        BattleMon mon = battleCtx.BattleMons[battler];
        MoveEffectsData effects = mon.MoveEffectsData;
        switch (param)
        {
            case 0:
                mon.Species = (ushort)value;
                return;
            case 1:
                mon.Attack = (ushort)value;
                return;
            case 2:
                mon.Defense = (ushort)value;
                return;
            case 3:
                mon.Speed = (ushort)value;
                return;
            case 4:
                mon.SpAttack = (ushort)value;
                return;
            case 5:
                mon.SpDefense = (ushort)value;
                return;
            case 6:
            case 7:
            case 8:
            case 9:
                mon.Moves[param - 6] = (ushort)value;
                return;
            case 10:
                mon.HpIV = value & 0x1F;
                return;
            case 11:
                mon.AttackIV = value & 0x1F;
                return;
            case 12:
                mon.DefenseIV = value & 0x1F;
                return;
            case 13:
                mon.SpeedIV = value & 0x1F;
                return;
            case 14:
                mon.SpAttackIV = value & 0x1F;
                return;
            case 15:
                mon.SpDefenseIV = value & 0x1F;
                return;
            case 16:
                mon.IsEgg = value & 1;
                return;
            case 17:
                mon.HasNickname = value & 1;
                return;
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
            case 24:
            case 25:
                mon.StatBoosts[param - 18] = (sbyte)value;
                return;
            case 26:
                mon.Ability = (byte)value;
                return;
            case 27:
                mon.Type1 = (byte)value;
                return;
            case 28:
                mon.Type2 = (byte)value;
                return;
            case 29:
                mon.Gender = value & 0xF;
                return;
            case 30:
                mon.IsShiny = value & 1;
                return;
            case 31:
            case 32:
            case 33:
            case 34:
                mon.PpCur[param - 31] = (byte)value;
                return;
            case 35:
            case 36:
            case 37:
            case 38:
                mon.PpUps[param - 35] = (byte)value;
                return;
            case 39:
            case 40:
            case 41:
            case 42:
                ErrorHandling.GF_AssertFail();
                return;
            case 43:
                mon.Level = (byte)value;
                return;
            case 44:
                mon.Friendship = (byte)value;
                return;
            case 45:
                for (int i = 0; i < NicknameLength; i++)
                {
                    mon.Nickname[i] = buffer![i];
                }
                return;
            case 47:
                mon.CurHP = (short)value;
                return;
            case 48:
                mon.MaxHP = (ushort)value;
                return;
            case 49:
                for (int i = 0; i < NicknameLength && i < mon.OTName.Length; i++)
                {
                    mon.OTName[i] = buffer![i];
                }
                return;
            case 50:
                mon.Exp = (uint)value;
                return;
            case 51:
                mon.Personality = (uint)value;
                return;
            case 52:
                mon.Status = (uint)value;
                return;
            case 53:
                mon.StatusVolatile = (uint)value;
                return;
            case 54:
                mon.OTId = (uint)value;
                return;
            case 55:
                mon.HeldItem = (ushort)value;
                return;
            case 56:
                mon.TimesDamaged = (byte)value;
                return;
            case 57:
                mon.TrainerMessageFlags = (byte)value;
                return;
            case 58:
                mon.OTGender = value & 0xF;
                return;
            case 59:
                mon.MoveEffectsMask = (uint)value;
                return;
            case 60:
                mon.MoveEffectsTemp = (uint)value;
                return;
            case 61:
                effects.DisabledTurns = value & 7;
                return;
            case 62:
                effects.EncoredTurns = value & 7;
                return;
            case 63:
                effects.ChargedTurns = value & 3;
                return;
            case 64:
                effects.TauntedTurns = value & 7;
                return;
            case 65:
                effects.ProtectSuccessTurns = value & 3;
                return;
            case 66:
                effects.PerishSongTurns = value & 3;
                return;
            case 67:
                effects.RolloutCount = value & 7;
                return;
            case 68:
                effects.FuryCutterCount = value & 7;
                return;
            case 69:
                effects.StockpileCount = value & 7;
                return;
            case 70:
                effects.StockpileDefBoosts = value & 7;
                return;
            case 71:
                effects.StockpileSpDefBoosts = value & 7;
                return;
            case 72:
                effects.Truant = value & 1;
                return;
            case 73:
                effects.FlashFire = value & 1;
                return;
            case 74:
                effects.LockOnTarget = value & 3;
                return;
            case 75:
                effects.MimickedMoveSlot = value & 0xF;
                return;
            case 76:
                effects.BindTarget = value & 3;
                return;
            case 77:
                effects.MeanLookTarget = value & 3;
                return;
            case 78:
                effects.LastResortCount = value & 7;
                return;
            case 79:
                effects.MagnetRiseTurns = value & 7;
                return;
            case 80:
                effects.HealBlockTurns = value & 7;
                return;
            case 81:
                effects.EmbargoTurns = value & 7;
                return;
            case 82:
                effects.CanUnburden = value & 1;
                return;
            case 83:
                effects.MetronomeTurns = value & 0xF;
                return;
            case 84:
                effects.MicleBerry = value & 1;
                return;
            case 85:
                effects.CustapBerry = value & 1;
                return;
            case 86:
                effects.QuickClaw = value & 1;
                return;
            case 87:
                effects.RechargeTurnNumber = value;
                return;
            case 88:
                effects.FakeOutTurnNumber = value;
                return;
            case 89:
                effects.SlowStartTurnNumber = value;
                return;
            case 90:
                effects.SubstituteHP = value;
                return;
            case 91:
                effects.TransformedPID = (uint)value;
                return;
            case 92:
                effects.DisabledMove = (ushort)value;
                return;
            case 93:
                effects.EncoredMove = (ushort)value;
                return;
            case 94:
                effects.BindingMove = (ushort)value;
                return;
            case 95:
                effects.ItemHPRecovery = value;
                return;
            case 96:
                mon.SlowStartAnnounced = value & 1;
                return;
            case 97:
                mon.SlowStartFinished = value & 1;
                return;
            case 98:
                mon.FormNum = value & 0x1F;
                return;
            case 100:
                BattleMon_Set(battleCtx, battler, battleCtx.ScriptTemp, value, buffer);
                return;
        }
        ErrorHandling.GF_AssertFail();
    }

    // OFFSET 0x02252A14
    public static void Battler_AddVal(BattleContext battleCtx, int battler, int param, int value)
    {
        BattleMon_AddVal(battleCtx.BattleMons[battler], param, value);
    }

    // OFFSET 0x02252A2C
    public static void BattleMon_AddVal(BattleMon mon, int param, int value)
    {
        MoveEffectsData effects = mon.MoveEffectsData;
        switch (param)
        {
            case 1:
                mon.Attack = (ushort)(mon.Attack + value);
                return;
            case 2:
                mon.Defense = (ushort)(mon.Defense + value);
                return;
            case 3:
                mon.Speed = (ushort)(mon.Speed + value);
                return;
            case 4:
                mon.SpAttack = (ushort)(mon.SpAttack + value);
                return;
            case 5:
                mon.SpDefense = (ushort)(mon.SpDefense + value);
                return;
            case 10:
                mon.HpIV = (mon.HpIV + value) & 0x1F;
                return;
            case 11:
                mon.AttackIV = (mon.AttackIV + value) & 0x1F;
                return;
            case 12:
                mon.DefenseIV = (mon.DefenseIV + value) & 0x1F;
                return;
            case 13:
                mon.SpeedIV = (mon.SpeedIV + value) & 0x1F;
                return;
            case 14:
                mon.SpAttackIV = (mon.SpAttackIV + value) & 0x1F;
                return;
            case 15:
                mon.SpDefenseIV = (mon.SpDefenseIV + value) & 0x1F;
                return;
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
            case 24:
            case 25:
            {
                int boost = mon.StatBoosts[param - 18] + value;
                if (boost < 0)
                {
                    boost = 0;
                }
                else if (boost > StatBoostMax)
                {
                    boost = StatBoostMax;
                }
                mon.StatBoosts[param - 18] = (sbyte)boost;
                return;
            }
            case 31:
            case 32:
            case 33:
            case 34:
            {
                int slot = param - 31;
                int maxPP = (int)MoveTable.MoveTable_CalcMaxPP(mon.Moves[slot], mon.PpUps[slot]);
                int pp = mon.PpCur[slot] + value;
                mon.PpCur[slot] = (byte)(pp > maxPP ? maxPP : pp);
                return;
            }
            case 35:
            case 36:
            case 37:
            case 38:
                mon.PpUps[param - 35] = (byte)(mon.PpUps[param - 35] + value);
                return;
            case 43:
                mon.Level = (byte)(mon.Level + value);
                return;
            case 44:
            {
                int friendship = mon.Friendship + value;
                if (friendship > FriendshipMax)
                {
                    friendship = FriendshipMax;
                }
                else if (friendship < 0)
                {
                    friendship = 0;
                }
                mon.Friendship = (byte)friendship;
                return;
            }
            case 47:
            {
                uint hp = (uint)(mon.CurHP + value);
                mon.CurHP = hp > mon.MaxHP ? (int)mon.MaxHP : (int)hp;
                return;
            }
            case 48:
                mon.MaxHP = (uint)(mon.MaxHP + value);
                return;
            case 50:
                mon.Exp = (uint)(mon.Exp + value);
                return;
            case 51:
                mon.Personality = (uint)(mon.Personality + value);
                return;
            case 61:
                effects.DisabledTurns = (effects.DisabledTurns + value) & 7;
                return;
            case 62:
                effects.EncoredTurns = (effects.EncoredTurns + value) & 7;
                return;
            case 63:
                effects.ChargedTurns = (effects.ChargedTurns + value) & 3;
                return;
            case 64:
                effects.TauntedTurns = (effects.TauntedTurns + value) & 7;
                return;
            case 65:
                effects.ProtectSuccessTurns = (effects.ProtectSuccessTurns + value) & 3;
                return;
            case 66:
                effects.PerishSongTurns = (effects.PerishSongTurns + value) & 3;
                return;
            case 67:
                effects.RolloutCount = (effects.RolloutCount + value) & 7;
                return;
            case 68:
                effects.FuryCutterCount = (effects.FuryCutterCount + value) & 7;
                return;
            case 69:
                effects.StockpileCount = (effects.StockpileCount + value) & 7;
                return;
            case 70:
                effects.StockpileDefBoosts = (effects.StockpileDefBoosts + value) & 7;
                return;
            case 71:
                effects.StockpileSpDefBoosts = (effects.StockpileSpDefBoosts + value) & 7;
                return;
            case 78:
                effects.LastResortCount = (effects.LastResortCount + value) & 7;
                return;
            case 79:
                effects.MagnetRiseTurns = (effects.MagnetRiseTurns + value) & 7;
                return;
            case 80:
                effects.HealBlockTurns = (effects.HealBlockTurns + value) & 7;
                return;
            case 87:
                effects.RechargeTurnNumber += value;
                return;
            case 88:
                effects.FakeOutTurnNumber += value;
                return;
            case 89:
                effects.SlowStartTurnNumber += value;
                return;
            case 90:
                effects.SubstituteHP += value;
                return;
            case 95:
                effects.ItemHPRecovery += value;
                return;
            case 96:
                mon.SlowStartAnnounced = (mon.SlowStartAnnounced + value) & 1;
                return;
            case 97:
                mon.SlowStartFinished = (mon.SlowStartFinished + value) & 1;
                return;
            case 98:
                mon.FormNum = (mon.FormNum + value) & 0x1F;
                return;
        }
        ErrorHandling.GF_AssertFail();
    }
}
