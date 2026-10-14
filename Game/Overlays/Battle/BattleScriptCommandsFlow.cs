namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    public const int SelectorAll = 0;
    private const int SkipWaitKeys = 0xC03;
    public const int SelectorPlayerSide = 3;
    public const int SelectorEnemySide = 4;
    public const int BattlerTypePlayerSlot2 = 4;

    public const int CompareEqual = 0;
    public const int CompareNotEqual = 1;
    public const int CompareGreater = 2;
    public const int CompareLessOrEqual = 3;
    public const int CompareFlagSet = 4;
    public const int CompareFlagNotSet = 5;
    public const int CompareAllFlagsSet = 6;

    private static bool BattleScript_Compare(int op, int data, int value)
    {
        switch (op)
        {
            case CompareEqual:
                return data == value;
            case CompareNotEqual:
                return data != value;
            case CompareGreater:
                return data > value;
            case CompareLessOrEqual:
                return data <= value;
            case CompareFlagSet:
                return (data & value) != 0;
            case CompareFlagNotSet:
                return (data & value) == 0;
            case CompareAllFlagsSet:
                return (data & value) == value;
        }
        return true;
    }

    // OFFSET 0x0224064C
    private static int BattleScript_SetupBattleUI(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleIO.BattleIO_SetupBattleUI(battleSys, 0);
        return 0;
    }

    private static void BattleScript_SendOutOne(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!.BattlerType & 1) == 0)
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
        BattleIO.BattleIO_PokemonSendOut(battleSys, battler);
    }

    // OFFSET 0x022406E0
    private static int BattleScript_PokemonSendOut(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        switch (selector)
        {
            default:
                for (int i = 0; i < maxBattlers; i++)
                {
                    BattleIO.BattleIO_PokemonSendOut(battleSys, i);
                    BattleSystems.BattleSystem_RegisterSeenPokemon(battleSys, i);
                }
                break;
            case SelectorPlayerSide:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, i)!.BattlerType & 1) == 0)
                    {
                        BattleIO.BattleIO_PokemonSendOut(battleSys, i);
                        BattleSystems.BattleSystem_RegisterSeenPokemon(battleSys, i);
                    }
                }
                BattleContexts.BattleSystem_UpdateSideExpMask(battleSys, battleCtx, 1);
                BattleContexts.BattleSystem_UpdateSideExpMask(battleSys, battleCtx, 3);
                break;
            case SelectorEnemySide:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, i)!.BattlerType & 1) != 0)
                    {
                        BattleContexts.BattleSystem_ClearSideExpMask(battleCtx, i);
                        BattleContexts.BattleSystem_UpdateSideExpMask(battleSys, battleCtx, i);
                        BattleIO.BattleIO_PokemonSendOut(battleSys, i);
                        BattleSystems.BattleSystem_RegisterSeenPokemon(battleSys, i);
                    }
                }
                break;
            case 1:
                BattleScript_SendOutOne(battleSys, battleCtx, battleCtx.Attacker);
                break;
            case 2:
                BattleScript_SendOutOne(battleSys, battleCtx, battleCtx.Defender);
                break;
            case 6:
                BattleScript_SendOutOne(battleSys, battleCtx, battleCtx.SwitchedMon);
                break;
        }
        return 0;
    }

    // OFFSET 0x02240B68
    private static int BattleScript_TrainerEncounter(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        switch (selector)
        {
            default:
                if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeTag) != 0)
                {
                    for (int i = 0; i < maxBattlers; i++)
                    {
                        if (BattleSystems.BattleSystem_GetBattlerData(battleSys, i)!.BattlerType != BattlerTypePlayerSlot2)
                        {
                            BattleIO.BattleIO_TrainerEncounter(battleSys, i);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < maxBattlers; i++)
                    {
                        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeMulti) == 0 && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeDoubles) != 0 && i > 1)
                        {
                            break;
                        }
                        BattleIO.BattleIO_TrainerEncounter(battleSys, i);
                    }
                }
                break;
            case SelectorPlayerSide:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, i)!.BattlerType & 1) == 0)
                    {
                        BattleIO.BattleIO_TrainerEncounter(battleSys, i);
                        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeMulti) == 0 && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeDoubles) != 0)
                        {
                            break;
                        }
                    }
                }
                break;
            case SelectorEnemySide:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, i)!.BattlerType & 1) != 0)
                    {
                        BattleIO.BattleIO_TrainerEncounter(battleSys, i);
                        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeMulti) == 0 && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeTag) == 0 && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeDoubles) != 0)
                        {
                            break;
                        }
                    }
                }
                break;
        }
        return 0;
    }

    // OFFSET 0x02243504
    private static int BattleScript_PrintEncounterMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_EncounterMessage(battleSys, battleCtx, battler);
        return 0;
    }

    // OFFSET 0x02243530
    private static int BattleScript_PrintSendOutMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_SendOutMessage(battleSys, battleCtx, battler);
        return 0;
    }

    // OFFSET 0x02248084
    private static int BattleScript_ShowPartyGaugeAtStart(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_ShowPartyGaugeAtStart(battleSys, battler);
        return 0;
    }

    // OFFSET 0x022480B0
    private static int BattleScript_HidePartyGaugeAtStart(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_HidePartyGaugeAtStart(battleSys, battler);
        return 0;
    }

    // OFFSET 0x022480DC
    private static int BattleScript_ShowPartyGauge(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_ShowPartyGauge(battleSys, battler);
        return 0;
    }

    // OFFSET 0x02248108
    private static int BattleScript_HidePartyGauge(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_HidePartyGauge(battleSys, battler);
        return 0;
    }

    // OFFSET 0x0224A984
    public static int BattleScript_Battler(BattleSystem battleSys, BattleContext battleCtx, int selector)
    {
        int battler;
        int maxBattlers;
        switch (selector)
        {
            default:
            case 0:
            case 1:
            case 13:
            case 18:
                battler = battleCtx.Attacker;
                break;
            case 2:
                battler = battleCtx.Defender;
                break;
            case 3:
                maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
                for (battler = 0; battler < maxBattlers; battler++)
                {
                    if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!.BattlerType & 1) == 0)
                    {
                        break;
                    }
                }
                break;
            case 4:
                maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
                for (battler = 0; battler < maxBattlers; battler++)
                {
                    if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!.BattlerType & 1) != 0)
                    {
                        break;
                    }
                }
                break;
            case 5:
                battler = battleCtx.FaintedMon;
                break;
            case 6:
            case 22:
                battler = battleCtx.SwitchedMon;
                break;
            case 7:
                battler = battleCtx.SideEffectMon;
                break;
            case 8:
                battler = battleCtx.AbilityMon;
                break;
            case 9:
                maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
                for (battler = 0; battler < maxBattlers; battler++)
                {
                    int type = BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!.BattlerType;
                    if (type == 2 || type == 0)
                    {
                        break;
                    }
                }
                break;
            case 10:
                maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
                for (battler = 0; battler < maxBattlers; battler++)
                {
                    int type = BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!.BattlerType;
                    if (type == 3 || type == 1)
                    {
                        break;
                    }
                }
                break;
            case 11:
            {
                int target = (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeDoubles) != 0 ? 4 : 0;
                maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
                for (battler = 0; battler < maxBattlers; battler++)
                {
                    if (BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!.BattlerType == target)
                    {
                        break;
                    }
                }
                break;
            }
            case 12:
            {
                int target = (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeDoubles) != 0 ? 5 : 1;
                maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
                for (battler = 0; battler < maxBattlers; battler++)
                {
                    if (BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!.BattlerType == target)
                    {
                        break;
                    }
                }
                break;
            }
            case 14:
                battler = battleCtx.MsgAttacker;
                break;
            case 15:
                battler = battleCtx.MsgDefender;
                break;
            case 16:
                maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
                for (battler = 0; battler < maxBattlers; battler++)
                {
                    if (battler != battleCtx.Attacker && BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) == BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker))
                    {
                        break;
                    }
                }
                if (battler == maxBattlers)
                {
                    battler = 0;
                }
                break;
            case 17:
                maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
                for (battler = 0; battler < maxBattlers; battler++)
                {
                    if (battler != battleCtx.Defender && BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) == BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Defender))
                    {
                        break;
                    }
                }
                if (battler == maxBattlers)
                {
                    battler = 0;
                }
                break;
            case 19:
            {
                maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
                int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
                for (battler = 0; battler < maxBattlers; battler++)
                {
                    if (side != BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler))
                    {
                        break;
                    }
                }
                break;
            }
            case 20:
            {
                maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
                int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Defender);
                for (battler = 0; battler < maxBattlers; battler++)
                {
                    if (side != BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler))
                    {
                        break;
                    }
                }
                break;
            }
            case 21:
            case 0xFF:
                battler = battleCtx.MsgBattlerTemp;
                break;
        }
        if (battler == 0xFF)
        {
            ErrorHandling.GF_AssertFail();
        }
        return battler;
    }

    // OFFSET 0x02248134
    private static int BattleScript_LoadPartyGaugeGraphics(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleIO.BattleIO_LoadPartyGaugeGraphics(battleSys);
        return 0;
    }

    // OFFSET 0x0224814C
    private static int BattleScript_FreePartyGaugeGraphics(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        BattleIO.BattleIO_FreePartyGaugeGraphics(battleSys);
        return 0;
    }

    // OFFSET 0x02248988
    private static int BattleScript_WaitFrames(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int frames = BattleScript_Read(battleCtx);
        int step;
        if ((battleSys.BattleType & BattleSystems.BattleTypeLink) != 0 && (battleSys.BattleStatusMask & BattleSystems.BattleStatusRecording) == 0)
        {
            step = 2;
        }
        else
        {
            step = 1;
        }
        if (frames > battleCtx.WaitCounter)
        {
            BattleScript_Iter(battleCtx, -2);
            battleCtx.WaitCounter += step;
        }
        else
        {
            battleCtx.WaitCounter = 0;
        }
        battleCtx.BattleProgressFlag = 1;
        return 0;
    }

    // OFFSET 0x02248AB4
    private static int BattleScript_End(BattleSystem battleSys, BattleContext battleCtx)
    {
        battleCtx.BattleProgressFlag = 1;
        return BattleContexts.BattleSystem_PopScript(battleCtx);
    }

    // OFFSET 0x02240C84
    private static int BattleScript_ThrowTrainerBall(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int throwType = BattleScript_Read(battleCtx);
        switch (selector)
        {
            default:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeMulti) == 0 && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeDoubles) != 0 && i > 1)
                    {
                        break;
                    }
                    BattleIO.BattleIO_ThrowTrainerBall(battleSys, i, throwType);
                }
                break;
            case SelectorPlayerSide:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, i)!.BattlerType & 1) == 0)
                    {
                        BattleIO.BattleIO_ThrowTrainerBall(battleSys, i, throwType);
                        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeMulti) == 0 && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeDoubles) != 0)
                        {
                            break;
                        }
                    }
                }
                break;
            case SelectorEnemySide:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, i)!.BattlerType & 1) != 0)
                    {
                        BattleIO.BattleIO_ThrowTrainerBall(battleSys, i, throwType);
                        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeMulti) == 0 && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeTag) == 0 && (BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeDoubles) != 0)
                        {
                            break;
                        }
                    }
                }
                break;
        }
        battleCtx.BattleProgressFlag = 1;
        return 0;
    }

    // OFFSET 0x02241340
    private static int BattleScript_WaitMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        if (BattleIO.BattleIO_QueueIsEmpty(battleCtx) != 0)
        {
            BattleScript_Iter(battleCtx, 1);
        }
        else
        {
            BattleIO.BattleIO_UpdateTimeout(battleCtx);
        }
        battleCtx.BattleProgressFlag = 1;
        return 0;
    }

    // OFFSET 0x02241984
    private static int BattleScript_WaitButtonABTime(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int frames = BattleScript_Read(battleCtx);
        if ((BattleSystems.BattleSystem_GetBattleType(battleSys) & BattleSystems.BattleTypeLink) == 0)
        {
            if ((Globals.gSystem.PressedKeys & SkipWaitKeys) != 0 || TouchScreen.TouchScreen_Tapped())
            {
                battleCtx.WaitCounter = frames;
            }
        }
        int step;
        if ((battleSys.BattleType & BattleSystems.BattleTypeLink) != 0 && (battleSys.BattleStatusMask & BattleSystems.BattleStatusRecording) == 0)
        {
            step = 2;
        }
        else
        {
            step = 1;
        }
        if (frames > battleCtx.WaitCounter)
        {
            BattleScript_Iter(battleCtx, -2);
            battleCtx.WaitCounter += step;
        }
        else
        {
            battleCtx.WaitCounter = 0;
        }
        battleCtx.BattleProgressFlag = 1;
        return 0;
    }

    // OFFSET 0x02241A58
    private static int BattleScript_CompareVarToValue(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int op = BattleScript_Read(battleCtx);
        int var = BattleScript_Read(battleCtx);
        int value = BattleScript_Read(battleCtx);
        int jump = BattleScript_Read(battleCtx);
        int data = BattleScript_VarAddress(battleSys, battleCtx, var);
        if (!BattleScript_Compare(op, data, value))
        {
            jump = 0;
        }
        if (jump != 0)
        {
            BattleScript_Iter(battleCtx, jump);
        }
        return 0;
    }
}
