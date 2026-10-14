namespace PlatinumSharp.Game;

public static partial class BattleIO
{
    public const int CommandSlideHealthboxIn = 0xC;
    public const int CommandSlideHealthboxOut = 0xD;
    public const int CommandStartHealthboxArrow = 0x28;
    public const int CommandStopHealthboxArrow = 0x29;
    private const int SlideHealthboxInSize = 0x18;
    private const int HealthboxMonDataSpecies = 5;
    private const int HealthboxMonDataLevel = 0xA1;
    private const int SpeciesNidoranF = 0x1D;
    private const int SpeciesNidoranM = 0x20;
    private const int GenderNone = 2;

    // OFFSET 0x022651DC
    public static void BattleIO_SlideHealthboxIn(BattleSystem battleSys, BattleContext battleCtx, int battler, int delay)
    {
        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, battleCtx.SelectedPartySlot[battler]);
        int species = (int)Pokemon.Pokemon_GetValue(mon, HealthboxMonDataSpecies, null);
        int level = (int)Pokemon.Pokemon_GetValue(mon, HealthboxMonDataLevel, null);
        BattleMon battleMon = battleCtx.BattleMons[battler];
        byte[] message = new byte[SlideHealthboxInSize];
        W8(message, 0, CommandSlideHealthboxIn);
        W8(message, 1, battleMon.Level);
        W16(message, 2, battleMon.CurHP);
        W16(message, 4, (int)battleMon.MaxHP);
        W8(message, 6, battleCtx.SelectedPartySlot[battler]);
        int info = BattleContexts.Battler_StatusIcon(battleCtx, battler) & 0x1F;
        if ((battleMon.Species == SpeciesNidoranF || battleMon.Species == SpeciesNidoranM) && battleMon.HasNickname == 0)
        {
            info |= GenderNone << 5;
        }
        else
        {
            info |= (battleMon.Gender & 3) << 5;
        }
        W32(message, 8, battleMon.Exp - Pokemon.Species_GetExpForLevel(species, level));
        W32(message, 0xC, Pokemon.Species_GetExpForLevel(species, level + 1) - Pokemon.Species_GetExpForLevel(species, level));
        info |= (BattleSystems.BattleSystem_CaughtSpecies(battleSys, battleMon.Species) & 1) << 7;
        W8(message, 7, info);
        W32(message, 0x10, (uint)BattleSystems.BattleSystem_GetSafariBalls(battleSys));
        W8(message, 0x14, delay);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, SlideHealthboxInSize);
    }

    // OFFSET 0x02265314
    public static void BattleIO_SlideHealthboxOut(BattleSystem battleSys, int battler)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandSlideHealthboxOut);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x02266460
    public static void BattleIO_StartHealthboxArrow(BattleSystem battleSys, int battler)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandStartHealthboxArrow);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x0226647C
    public static void BattleIO_StopHealthboxArrow(BattleSystem battleSys, int battler)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandStopHealthboxArrow);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }
}
