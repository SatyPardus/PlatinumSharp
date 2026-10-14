using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed partial class Pokemon
{
    private const int NarcEvolutions = 0x22;
    private const int EvolutionDataSize = 0x2A;
    private const int EvolutionEntrySize = 6;
    private const int MaxEvolutions = 7;
    private const int EvoParamPersonality = 0;
    private const int EvoParamHeldItem = 6;
    private const int EvoParamFriendship = 9;
    private const int EvoParamBeauty = 0x14;
    private const int EvoParamAttack = 0xA5;
    private const int EvoParamDefense = 0xA6;
    private const int EvoParamGender = 0x6F;
    private const int EvoParamMove1 = 0x36;
    private const int SpeciesKadabra = 0x40;
    private const int HoldEffectEverstone = 0x40;
    private const int EvolutionClassLevelUp = 0;
    private const int EvolutionClassTrade = 1;
    private const int EvolutionClassItemCheck = 2;
    private const int EvolutionClassItemUse = 3;
    private const int FriendshipEvolutionThreshold = 220;

    // OFFSET 0x0207821C
    public static void Species_LoadEvolutions(int species, byte[] evolutions)
    {
        NARC.NARC_ReadWholeMemberByIndexPair(evolutions, NarcEvolutions, species);
    }

    // OFFSET 0x0207749C
    public static int Pokemon_HasMove(Pokemon mon, int move)
    {
        int slot;
        for (slot = 0; slot < 4; slot++)
        {
            if (move == Pokemon_GetValue(mon, EvoParamMove1 + slot, null))
            {
                break;
            }
        }
        return slot != 4 ? 1 : 0;
    }

    private static void Evolution_Set(ref int target, ref int evolutionType, int species, int type)
    {
        target = species;
        evolutionType = type;
    }

    // OFFSET 0x02076B94
    public static int Pokemon_GetEvolutionTargetSpecies(Party? party, Pokemon mon, int evolutionClass, ushort evolutionParam, out int evolutionType)
    {
        int target = 0;
        evolutionType = 0;
        ushort species = (ushort)Pokemon_GetValue(mon, ParamSpecies, null);
        ushort item = (ushort)Pokemon_GetValue(mon, EvoParamHeldItem, null);
        uint personality = Pokemon_GetValue(mon, EvoParamPersonality, null);
        byte beauty = (byte)Pokemon_GetValue(mon, EvoParamBeauty, null);
        ushort personalityHigh = (ushort)((personality & 0xFFFF0000) >> 16);
        byte holdEffect = (byte)Item.Item_LoadParam(item, 1, 0);
        if (species != SpeciesKadabra && holdEffect == HoldEffectEverstone && evolutionClass != EvolutionClassItemUse)
        {
            return 0;
        }
        byte[] evolutions = Heap.Heap_Alloc(0, (EvolutionDataSize + 3) & ~3)!;
        Species_LoadEvolutions(species, evolutions);
        switch (evolutionClass)
        {
            case EvolutionClassLevelUp:
                {
                    byte level = (byte)Pokemon_GetValue(mon, ParamLevel, null);
                    ushort friendship = (ushort)Pokemon_GetValue(mon, EvoParamFriendship, null);
                    int personalityMod = personalityHigh % 10;
                    for (int i = 0; i < MaxEvolutions; i++)
                    {
                        int offset = i * EvolutionEntrySize;
                        ushort method = (ushort)Bin.U16(evolutions, offset);
                        ushort param = (ushort)Bin.U16(evolutions, offset + 2);
                        ushort targetSpecies = (ushort)Bin.U16(evolutions, offset + 4);
                        switch (method)
                        {
                            case 1:
                                if (friendship >= FriendshipEvolutionThreshold)
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 1);
                                }
                                break;
                            case 2:
                                if (!Rtc.IsNight() && friendship >= FriendshipEvolutionThreshold)
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 2);
                                }
                                break;
                            case 3:
                                if (Rtc.IsNight() && friendship >= FriendshipEvolutionThreshold)
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 3);
                                }
                                break;
                            case 4:
                                if (param <= level)
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 4);
                                }
                                break;
                            case 8:
                                if (param <= level && Pokemon_GetValue(mon, EvoParamAttack, null) > Pokemon_GetValue(mon, EvoParamDefense, null))
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 8);
                                }
                                break;
                            case 9:
                                if (param <= level && Pokemon_GetValue(mon, EvoParamAttack, null) == Pokemon_GetValue(mon, EvoParamDefense, null))
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 9);
                                }
                                break;
                            case 10:
                                if (param <= level && Pokemon_GetValue(mon, EvoParamAttack, null) < Pokemon_GetValue(mon, EvoParamDefense, null))
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 10);
                                }
                                break;
                            case 11:
                                if (param <= level && personalityMod < 5)
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 11);
                                }
                                break;
                            case 12:
                                if (param <= level && personalityMod >= 5)
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 12);
                                }
                                break;
                            case 13:
                                if (param <= level)
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 13);
                                }
                                break;
                            case 14:
                                evolutionType = 14;
                                break;
                            case 15:
                                if (param <= beauty)
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 15);
                                }
                                break;
                            case 18:
                                if (!Rtc.IsNight() && item == param)
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 18);
                                }
                                break;
                            case 19:
                                if (Rtc.IsNight() && item == param)
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 19);
                                }
                                break;
                            case 20:
                                if (Pokemon_HasMove(mon, param) == 1)
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 20);
                                }
                                break;
                            case 21:
                                if (party != null && Party.Party_HasSpecies(party, param))
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 21);
                                }
                                break;
                            case 22:
                                if (Pokemon_GetValue(mon, EvoParamGender, null) == 0 && param <= level)
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 22);
                                }
                                break;
                            case 23:
                                if (Pokemon_GetValue(mon, EvoParamGender, null) == 1 && param <= level)
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, 23);
                                }
                                break;
                            case 24:
                            case 25:
                            case 26:
                                if (evolutionParam == method)
                                {
                                    Evolution_Set(ref target, ref evolutionType, targetSpecies, method);
                                }
                                break;
                        }
                        if (target != 0)
                        {
                            break;
                        }
                    }
                    break;
                }
            case EvolutionClassTrade:
                for (int i = 0; i < MaxEvolutions; i++)
                {
                    int offset = i * EvolutionEntrySize;
                    ushort method = (ushort)Bin.U16(evolutions, offset);
                    if (method == 5)
                    {
                        Evolution_Set(ref target, ref evolutionType, Bin.U16(evolutions, offset + 4), 5);
                    }
                    else if (method == 6 && item == Bin.U16(evolutions, offset + 2))
                    {
                        Evolution_Set(ref target, ref evolutionType, Bin.U16(evolutions, offset + 4), 6);
                    }
                    if (target != 0)
                    {
                        break;
                    }
                }
                break;
            case EvolutionClassItemCheck:
            case EvolutionClassItemUse:
                for (int i = 0; i < MaxEvolutions; i++)
                {
                    int offset = i * EvolutionEntrySize;
                    ushort method = (ushort)Bin.U16(evolutions, offset);
                    ushort param = (ushort)Bin.U16(evolutions, offset + 2);
                    if (method == 7 && evolutionParam == param)
                    {
                        Evolution_Set(ref target, ref evolutionType, Bin.U16(evolutions, offset + 4), 0);
                        break;
                    }
                    if (method == 0x10 && Pokemon_GetValue(mon, EvoParamGender, null) == 0 && evolutionParam == param)
                    {
                        Evolution_Set(ref target, ref evolutionType, Bin.U16(evolutions, offset + 4), 0);
                        break;
                    }
                    if (method == 0x11 && Pokemon_GetValue(mon, EvoParamGender, null) == 1 && evolutionParam == param)
                    {
                        Evolution_Set(ref target, ref evolutionType, Bin.U16(evolutions, offset + 4), 0);
                        break;
                    }
                }
                break;
        }
        Heap.Heap_Free(evolutions);
        return target;
    }
}
