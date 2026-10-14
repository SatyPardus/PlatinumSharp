namespace PlatinumSharp.Game;

public static partial class BattleSystems
{
    // OFFSET 0x0223ED8C
    public static int BattleSystem_GetSafariBalls(BattleSystem battleSys)
    {
        return battleSys.SafariBalls;
    }

    // OFFSET 0x0223ED98
    public static void BattleSystem_SetSafariBalls(BattleSystem battleSys, int count)
    {
        battleSys.SafariBalls = count;
    }

    // OFFSET 0x0223EBF8
    public static int BattleSystem_GetTime(BattleSystem battleSys)
    {
        return battleSys.Time;
    }

    // OFFSET 0x0223EDA4
    public static Options? BattleSystem_GetOptions(BattleSystem battleSys)
    {
        return battleSys.Options;
    }

    // OFFSET 0x0223EF48
    public static void BattleSystem_AddToPoketchHistory(BattleSystem battleSys, Pokemon mon)
    {
        if (battleSys.Poketch != null)
        {
            Poketch.Poketch_PokemonHistoryEnqueue(battleSys.Poketch, Pokemon.Pokemon_GetBoxPokemon(mon));
        }
    }

    // OFFSET 0x0223F9A0
    public static void BattleSystem_RegisterCaughtPokemon(BattleSystem battleSys, int battler)
    {
        int type = Battlers.Battler_GetType(battleSys.Battlers[battler]!);
        if ((battleSys.BattleType & 0x84) == 0 && (type & 1) != 0)
        {
            int slot = BattleContexts.BattleContext_Get(battleSys, battleSys.BattleCtx!, 2, battler);
            Pokedex.Pokedex_Capture(battleSys.Pokedex!, BattleSystem_GetPartyPokemon(battleSys, battler, slot));
        }
    }

    // OFFSET 0x0223F9E0
    public static int BattleSystem_CaughtSpecies(BattleSystem battleSys, int species)
    {
        return Pokedex.Pokedex_HasCaughtSpecies(battleSys.Pokedex!, (ushort)species) ? 1 : 0;
    }
}

public static partial class BattleContexts
{
    private const uint ConditionSleep = 0x7;
    private const uint ConditionPoison = 0x8;
    private const uint ConditionBurn = 0x10;
    private const uint ConditionFreeze = 0x20;
    private const uint ConditionParalysis = 0x40;
    private const uint ConditionToxic = 0x80;

    // OFFSET 0x02253F7C
    public static int Battler_StatusIcon(BattleContext battleCtx, int battler)
    {
        uint status = battleCtx.BattleMons[battler].Status;
        if ((status & ConditionSleep) != 0)
        {
            return 1;
        }
        if ((status & ConditionPoison) != 0)
        {
            return 2;
        }
        if ((status & ConditionBurn) != 0)
        {
            return 3;
        }
        if ((status & ConditionFreeze) != 0)
        {
            return 4;
        }
        if ((status & ConditionParalysis) != 0)
        {
            return 5;
        }
        if ((status & ConditionToxic) != 0)
        {
            return 2;
        }
        return 0;
    }
}
