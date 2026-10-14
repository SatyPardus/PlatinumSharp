namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int SelectorAllBattlers0 = 0;
    private const int SelectorPlayerSideOnly = 3;
    private const int SelectorEnemySideOnly = 4;
    private const int SelectorSwitchedMonOnly = 6;
    private const int SelectorAttackerOnly = 1;
    private const int SelectorDefenderOnly = 2;
    private const uint BattleTypeMultiSlide = 8;
    private const uint BattleTypeDoublesSlide = 2;
    private const uint BattleTypeTagSlide = 0x10;

    private static bool BattleScript_IsEnemy(BattleSystem battleSys, int battler)
    {
        return (BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!.BattlerType & 1) != 0;
    }

    // OFFSET 0x02240664
    private static int BattleScript_SetPokemonEncounter(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        if (selector == SelectorPlayerSideOnly)
        {
            return 0;
        }
        for (int battler = 0; battler < maxBattlers; battler++)
        {
            if (selector == SelectorEnemySideOnly && !BattleScript_IsEnemy(battleSys, battler))
            {
                continue;
            }
            BattleIO.BattleIO_PokemonEncounter(battleSys, battler);
            BattleSystems.BattleSystem_RegisterSeenPokemon(battleSys, battler);
        }
        return 0;
    }

    private static void BattleScript_SendOutSingle(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        if (!BattleScript_IsEnemy(battleSys, battler))
        {
            BattleContexts.BattleSystem_UpdateSideExpMask(battleSys, battleCtx, 1);
            BattleContexts.BattleSystem_UpdateSideExpMask(battleSys, battleCtx, 3);
        }
        else
        {
            BattleContexts.BattleSystem_ClearSideExpMask(battleCtx, battler);
            BattleContexts.BattleSystem_UpdateSideExpMask(battleSys, battleCtx, battler);
        }
        BattleSystems.BattleSystem_RegisterSeenPokemon(battleSys, battler);
        BattleIO.BattleIO_ShowPokemon(battleSys, battler, 0, 0);
    }

    // OFFSET 0x022408A0
    private static int BattleScript_ShowPokemon(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        switch (selector)
        {
            case SelectorPlayerSideOnly:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    if (!BattleScript_IsEnemy(battleSys, battler))
                    {
                        BattleIO.BattleIO_ShowPokemon(battleSys, battler, 0, 0);
                        BattleSystems.BattleSystem_RegisterSeenPokemon(battleSys, battler);
                    }
                }
                BattleContexts.BattleSystem_UpdateSideExpMask(battleSys, battleCtx, 1);
                BattleContexts.BattleSystem_UpdateSideExpMask(battleSys, battleCtx, 3);
                break;
            case SelectorEnemySideOnly:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    if (BattleScript_IsEnemy(battleSys, battler))
                    {
                        BattleContexts.BattleSystem_ClearSideExpMask(battleCtx, battler);
                        BattleContexts.BattleSystem_UpdateSideExpMask(battleSys, battleCtx, battler);
                        BattleIO.BattleIO_ShowPokemon(battleSys, battler, 0, 0);
                        BattleSystems.BattleSystem_RegisterSeenPokemon(battleSys, battler);
                    }
                }
                break;
            case SelectorAttackerOnly:
                BattleScript_SendOutSingle(battleSys, battleCtx, battleCtx.Attacker);
                break;
            case SelectorDefenderOnly:
                BattleScript_SendOutSingle(battleSys, battleCtx, battleCtx.Defender);
                break;
            case SelectorSwitchedMonOnly:
                BattleScript_SendOutSingle(battleSys, battleCtx, battleCtx.SwitchedMon);
                break;
            default:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    BattleIO.BattleIO_ShowPokemon(battleSys, battler, 0, 0);
                    BattleSystems.BattleSystem_RegisterSeenPokemon(battleSys, battler);
                }
                break;
        }
        return 0;
    }

    // OFFSET 0x02240A7C
    private static int BattleScript_RecallPokemon(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        switch (selector)
        {
            case SelectorAllBattlers0:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    BattleIO.BattleIO_ReturnPokemon(battleSys, battleCtx, battler);
                }
                break;
            case SelectorPlayerSideOnly:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    if (!BattleScript_IsEnemy(battleSys, battler))
                    {
                        BattleIO.BattleIO_ReturnPokemon(battleSys, battleCtx, battler);
                    }
                }
                break;
            case SelectorEnemySideOnly:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    if (BattleScript_IsEnemy(battleSys, battler) && (HealthBar.FlagIndex(battler) & battleCtx.BattlersSwitchingMask) == 0)
                    {
                        BattleIO.BattleIO_ReturnPokemon(battleSys, battleCtx, battler);
                    }
                }
                break;
            default:
                BattleIO.BattleIO_ReturnPokemon(battleSys, battleCtx, BattleScript_Battler(battleSys, battleCtx, selector));
                break;
        }
        return 0;
    }

    // OFFSET 0x02240B3C
    private static int BattleScript_DeletePokemon(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_DeleteTrainerSprite(battleSys, battler);
        return 0;
    }

    // OFFSET 0x02240D94
    private static int BattleScript_TrainerSlideOut(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        switch (selector)
        {
            case 3:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    if (!BattleScript_IsEnemy(battleSys, battler))
                    {
                        BattleIO.BattleIO_SlideTrainerOut(battleSys, battler);
                        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeMultiSlide) == 0 && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoublesSlide) != 0)
                        {
                            break;
                        }
                    }
                }
                break;
            case 4:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    if (BattleScript_IsEnemy(battleSys, battler))
                    {
                        BattleIO.BattleIO_SlideTrainerOut(battleSys, battler);
                        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeMultiSlide) == 0
                            && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeTagSlide) == 0
                            && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoublesSlide) != 0)
                        {
                            break;
                        }
                    }
                }
                break;
            case 9:
            case 10:
            case 11:
            case 12:
            {
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    int type = BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!.BattlerType;
                    bool match = selector switch
                    {
                        9 => type == 0 || type == 2,
                        10 => type == 1 || type == 3,
                        11 => type == 4,
                        _ => type == 5,
                    };
                    if (match)
                    {
                        BattleIO.BattleIO_SlideTrainerOut(battleSys, battler);
                        break;
                    }
                }
                break;
            }
            default:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeMultiSlide) == 0 && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoublesSlide) != 0 && battler > 1)
                    {
                        break;
                    }
                    BattleIO.BattleIO_SlideTrainerOut(battleSys, battler);
                }
                break;
        }
        return 0;
    }
}

public static partial class BattleIO
{
    private const int CommandPokemonEncounter = 2;
    private const int PokemonEncounterSize = 0x3C;
    private const int CommandShowPokemon = 4;
    private const int ShowPokemonSize = 0x74;
    private const int CommandReturnPokemon = 5;
    private const int ReturnPokemonSize = 0x2C;

    // OFFSET 0x02264AB4
    public static void BattleIO_PokemonEncounter(BattleSystem battleSys, int battler)
    {
        BattleContext battleCtx = battleSys.BattleCtx!;
        byte[] message = new byte[PokemonEncounterSize];
        W8(message, 0, CommandPokemonEncounter);
        BattleIO_WriteMonHeader(battleSys, battler, message);
        W32(message, 8, (uint)BattleContexts.BattleSystem_GetCryModulation(battleCtx, battler, BattleSystems.BattleSystem_BattlerSlot(battleSys, battler), 1));
        BattleIO_WriteMonMoves(battleSys, battler, message, 0xC);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, PokemonEncounterSize);
    }

    private static void BattleIO_WriteAllMonSprites(BattleContext battleCtx, byte[] message, int speciesOffset, int genderOffset, int shinyOffset, int formOffset, int personalityOffset)
    {
        for (int i = 0; i < 4; i++)
        {
            BattleMon mon = battleCtx.BattleMons[i];
            W16(message, speciesOffset + i * 2, mon.Species);
            W8(message, shinyOffset + i, mon.IsShiny & 1);
            W8(message, formOffset + i, mon.FormNum & 0x1F);
            if ((mon.StatusVolatile & 0x200000) != 0)
            {
                W8(message, genderOffset + i, mon.MoveEffectsData.TransformedGender);
                W32(message, personalityOffset + i * 4, mon.MoveEffectsData.TransformedPID);
            }
            else
            {
                W8(message, genderOffset + i, mon.Gender & 0xF);
                W32(message, personalityOffset + i * 4, mon.Personality);
            }
        }
    }

    // OFFSET 0x02264CE8
    public static void BattleIO_ShowPokemon(BattleSystem battleSys, int battler, int ball, int skipSlideIn)
    {
        BattleContext battleCtx = battleSys.BattleCtx!;
        byte[] message = new byte[ShowPokemonSize];
        BattleMon mon = battleCtx.BattleMons[battler];
        W8(message, 0, CommandShowPokemon);
        if ((mon.StatusVolatile & 0x200000) != 0)
        {
            W8(message, 1, (message[1] & ~3) | ((byte)mon.MoveEffectsData.TransformedGender & 3));
            W32(message, 4, mon.MoveEffectsData.TransformedPID);
        }
        else
        {
            W8(message, 1, (message[1] & ~3) | (mon.Gender & 0xF & 3));
            W32(message, 4, mon.Personality);
        }
        W8(message, 1, (message[1] & ~4) | ((mon.IsShiny & 1) << 2));
        W16(message, 2, mon.Species);
        W32(message, 8, (uint)BattleContexts.BattleSystem_GetCryModulation(battleCtx, battler, BattleSystems.BattleSystem_BattlerSlot(battleSys, battler), 0));
        int slot = battleCtx.SelectedPartySlot[battler];
        W32(message, 0xC, (uint)slot);
        W8(message, 1, (message[1] & ~0xF8) | ((mon.FormNum & 0x1F) << 3));
        W32(message, 0x10, ball != 0 ? (uint)ball : mon.CapturedBall);
        W32(message, 0x14, (uint)skipSlideIn);
        W32(message, 0x4C, (mon.StatusVolatile & 0x1000000) != 0 ? 1u : 0u);
        BattleSystems.BattleSystem_SetParticipant(battleSys, battler, slot);
        BattleIO_WriteMonMoves(battleSys, battler, message, 0x18);
        BattleIO_WriteAllMonSprites(battleCtx, message, 0x50, 0x58, 0x5C, 0x60, 0x64);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, ShowPokemonSize);
    }

    // OFFSET 0x02264EF8
    public static void BattleIO_ReturnPokemon(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        byte face = (byte)((battleSys.Battlers[battler]!.BattlerType & 1) != 0 ? 2 : 0);
        BattleMon mon = battleCtx.BattleMons[battler];
        byte form = (byte)(mon.FormNum & 0x1F);
        byte[] message = new byte[ReturnPokemonSize];
        W8(message, 0, CommandReturnPokemon);
        byte yOffset;
        if ((mon.StatusVolatile & 0x200000) != 0)
        {
            yOffset = PokemonSpriteMetadata.Pokemon_LoadSpriteYOffset(mon.Species, (byte)mon.MoveEffectsData.TransformedGender, face, form, mon.MoveEffectsData.TransformedPID);
        }
        else
        {
            yOffset = PokemonSpriteMetadata.Pokemon_LoadSpriteYOffset(mon.Species, (byte)(mon.Gender & 0xF), face, form, mon.Personality);
        }
        W8(message, 1, yOffset);
        W16(message, 2, mon.CapturedBall);
        W32(message, 4, (mon.StatusVolatile & 0x1000000) != 0 ? 1u : 0u);
        BattleIO_WriteAllMonSprites(battleCtx, message, 8, 0x10, 0x14, 0x18, 0x1C);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, ReturnPokemonSize);
    }
}

public static partial class BattleScripts
{
    // OFFSET 0x02240F44
    private static int BattleScript_TrainerSlideIn(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int position = BattleScript_Read(battleCtx);
        switch (selector)
        {
            case 3:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    if (!BattleScript_IsEnemy(battleSys, battler))
                    {
                        BattleIO.BattleIO_SlideTrainerIn(battleSys, battler, position);
                        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoublesSlide) != 0)
                        {
                            break;
                        }
                    }
                }
                break;
            case 4:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    if (BattleScript_IsEnemy(battleSys, battler))
                    {
                        BattleIO.BattleIO_SlideTrainerIn(battleSys, battler, position);
                        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoublesSlide) != 0)
                        {
                            break;
                        }
                    }
                }
                break;
            case 9:
            case 10:
            case 11:
            case 12:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    int type = BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!.BattlerType;
                    bool match = selector switch
                    {
                        9 => type == 0 || type == 2,
                        10 => type == 1 || type == 3,
                        11 => type == 4,
                        _ => type == 5,
                    };
                    if (match)
                    {
                        BattleIO.BattleIO_SlideTrainerIn(battleSys, battler, position);
                        break;
                    }
                }
                break;
            default:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleTypeDoublesSlide) != 0 && battler > 1)
                    {
                        break;
                    }
                    BattleIO.BattleIO_SlideTrainerIn(battleSys, battler, position);
                }
                break;
        }
        return 0;
    }

    // OFFSET 0x02241288
    private static int BattleScript_HealthBoxSlideOut(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        switch (selector)
        {
            case SelectorAllBattlers0:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    BattleIO.BattleIO_SlideHealthboxOut(battleSys, battler);
                }
                break;
            case SelectorPlayerSideOnly:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    if (!BattleScript_IsEnemy(battleSys, battler) && (HealthBar.FlagIndex(battler) & battleCtx.BattlersSwitchingMask) == 0)
                    {
                        BattleIO.BattleIO_SlideHealthboxOut(battleSys, battler);
                    }
                }
                break;
            case SelectorEnemySideOnly:
                for (int battler = 0; battler < maxBattlers; battler++)
                {
                    if (BattleScript_IsEnemy(battleSys, battler))
                    {
                        BattleIO.BattleIO_SlideHealthboxOut(battleSys, battler);
                    }
                }
                break;
            default:
                BattleIO.BattleIO_SlideHealthboxOut(battleSys, BattleScript_Battler(battleSys, battleCtx, selector));
                break;
        }
        return 0;
    }
}
