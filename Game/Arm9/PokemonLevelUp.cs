using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed partial class Pokemon
{
    private const int LevelUpMovesSize = 0x2C;
    private const ushort LevelUpMovesEnd = 0xFFFF;
    private const int LevelUpMoveLevelMask = 0xFE00;
    private const int LevelUpMoveIDMask = 0x1FF;

    // OFFSET 0x02075AAC
    public static uint Pokemon_GetCurrentLevelBaseExp(Pokemon mon)
    {
        int species = (int)Pokemon_GetValue(mon, ParamSpecies, null);
        int level = (int)Pokemon_GetValue(mon, ParamLevel, null);
        return Species_GetExpForLevel(species, level);
    }

    // OFFSET 0x02076B14
    public static int Pokemon_ShouldLevelUp(Pokemon mon)
    {
        ushort species = (ushort)Pokemon_GetValue(mon, ParamSpecies, null);
        byte[] level = { (byte)(Pokemon_GetValue(mon, ParamLevel, null) + 1) };
        uint exp = Pokemon_GetValue(mon, ParamExp, null);
        int growthRate = (int)SpeciesData.SpeciesData_GetSpeciesValue(species, PersonalGrowthRate);
        uint maxExp = GrowthRate_GetExpForLevel(growthRate, MaxLevel);
        if (exp > maxExp)
        {
            exp = maxExp;
            Pokemon_SetValue(mon, ParamExp, exp);
        }
        if (level[0] > MaxLevel)
        {
            return 0;
        }
        if (exp >= GrowthRate_GetExpForLevel(growthRate, level[0]))
        {
            Pokemon_SetValue(mon, ParamLevel, level[0]);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0207727C
    public static ushort Pokemon_LevelUpMove(Pokemon mon, ref int index, ref ushort move)
    {
        ushort result = 0;
        byte[] levelUpMoves = Heap.Heap_Alloc(0, LevelUpMovesSize)!;
        ushort species = (ushort)Pokemon_GetValue(mon, ParamSpecies, null);
        int form = (int)Pokemon_GetValue(mon, ParamForm, null);
        int level = (byte)Pokemon_GetValue(mon, ParamLevel, null);
        Species_LoadLevelUpMoves(species, form, levelUpMoves);
        int entry = Bin.U16(levelUpMoves, index * 2);
        if (entry == LevelUpMovesEnd)
        {
            Heap.Heap_Free(levelUpMoves);
            return 0;
        }
        while ((entry & LevelUpMoveLevelMask) != level << 9)
        {
            index++;
            entry = Bin.U16(levelUpMoves, index * 2);
            if (entry == LevelUpMovesEnd)
            {
                Heap.Heap_Free(levelUpMoves);
                return 0;
            }
        }
        move = (ushort)(entry & LevelUpMoveIDMask);
        index++;
        result = Pokemon_AddMove(mon, move);
        Heap.Heap_Free(levelUpMoves);
        return result;
    }
}
