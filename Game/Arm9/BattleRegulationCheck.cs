namespace PlatinumSharp.Game;

public static class BattleRegulationCheck
{
    private const int MonDataSpecies = 5;
    private const int MonDataHeldItem = 6;
    private const int MonDataIsEgg = 0x4C;
    private const int MonDataLevel = 0xA1;
    private const int RuleTeamSize = 1;
    private const int RuleMaxLevel = 2;
    private const int RuleLevelSum = 3;
    private const int RuleEvolvedAllowed = 4;
    private const int RuleHeight = 5;
    private const int RuleWeight = 7;
    private const int RuleDuplicateItems = 9;
    private const int RuleDuplicateSpecies = 0xA;
    private const int RuleBannedSpeciesAllowed = 0xB;

    // OFFSET 0x0207A2D0
    public static bool BattleRegulation_CheckPokemon(BattleRegulation? regulation, Pokemon mon, HeightWeightData? heightWeight)
    {
        ushort species = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
        if (regulation == null)
        {
            return true;
        }
        int maxLevel = BattleRegulation.BattleRegulation_GetRuleValue(regulation, RuleMaxLevel);
        if (Pokemon.Pokemon_GetValue(mon, MonDataLevel, null) > (uint)maxLevel)
        {
            return false;
        }
        if (Pokemon.Pokemon_GetValue(mon, MonDataIsEgg, null) != 0)
        {
            return false;
        }
        if (BattleRegulation.BattleRegulation_GetRuleValue(regulation, RuleEvolvedAllowed) == 0 && species != Pokemon.Species_GetFirstStageWithoutIncense(species))
        {
            return false;
        }
        int height = BattleRegulation.BattleRegulation_GetRuleValue(regulation, RuleHeight);
        if (height != 0)
        {
            int monHeight = HeightWeight.HeightWeightData_Height(heightWeight, species);
            if (height > 0)
            {
                if (monHeight < height)
                {
                    return false;
                }
            }
            else if (monHeight > -height)
            {
                return false;
            }
        }
        int weight = BattleRegulation.BattleRegulation_GetRuleValue(regulation, RuleWeight) * 10;
        if (weight != 0)
        {
            int monWeight = HeightWeight.HeightWeightData_Weight(heightWeight, species);
            if (weight > 0)
            {
                if (monWeight < weight)
                {
                    return false;
                }
            }
            else if (monWeight > -weight)
            {
                return false;
            }
        }
        if (BattleRegulation.BattleRegulation_GetRuleValue(regulation, RuleBannedSpeciesAllowed) == 0 && Pokemon.Pokemon_IsOnBattleFrontierBanlist(mon))
        {
            return false;
        }
        return true;
    }

    // OFFSET 0x0207A3AC
    public static int BattleRegulation_CheckParty(BattleRegulation? regulation, Party party, byte[] selectionOrder, HeightWeightData? heightWeight)
    {
        int count = 0;
        int levelSum = 0;
        if (regulation == null)
        {
            return count;
        }
        ushort[] species = new ushort[6];
        ushort[] items = new ushort[6];
        for (int i = 0; i < 6; i++)
        {
            if (selectionOrder[i] != 0)
            {
                count++;
            }
        }
        if (count != BattleRegulation.BattleRegulation_GetRuleValue(regulation, RuleTeamSize))
        {
            return 4;
        }
        for (int i = 0; i < 6; i++)
        {
            if (selectionOrder[i] == 0)
            {
                continue;
            }
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, selectionOrder[i] - 1);
            if (!BattleRegulation_CheckPokemon(regulation, mon, heightWeight))
            {
                return 5;
            }
            species[i] = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpecies, null);
            items[i] = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataHeldItem, null);
            levelSum += (int)Pokemon.Pokemon_GetValue(mon, MonDataLevel, null);
        }
        int maxLevelSum = BattleRegulation.BattleRegulation_GetRuleValue(regulation, RuleLevelSum);
        if (levelSum > maxLevelSum && maxLevelSum != 0)
        {
            return 1;
        }
        if (BattleRegulation.BattleRegulation_GetRuleValue(regulation, RuleDuplicateSpecies) == 0 && count > 1)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = i + 1; j < 6; j++)
                {
                    if (species[i] == species[j] && species[i] != 0)
                    {
                        return 2;
                    }
                }
            }
        }
        if (BattleRegulation.BattleRegulation_GetRuleValue(regulation, RuleDuplicateItems) == 0 && count > 1)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = i + 1; j < 6; j++)
                {
                    if (items[i] == items[j] && species[i] != 0 && items[i] != 0)
                    {
                        return 3;
                    }
                }
            }
        }
        return 0;
    }
}
