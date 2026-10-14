namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int HealthboxStaggerDelay = 4;

    // OFFSET 0x022410DC
    private static int BattleScript_StartHealthboxArrows(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        for (int i = 0; i < maxBattlers; i++)
        {
            BattleIO.BattleIO_StartHealthboxArrow(battleSys, i);
        }
        return 0;
    }

    // OFFSET 0x02241108
    private static int BattleScript_SlideHealthboxIn(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        switch (selector)
        {
            case SelectorAll:
                for (int i = 0; i < maxBattlers; i++)
                {
                    BattleIO.BattleIO_SlideHealthboxIn(battleSys, battleCtx, i, 0);
                }
                break;
            case SelectorPlayerSide:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, i)!.BattlerType & 1) == 0)
                    {
                        BattleIO.BattleIO_SlideHealthboxIn(battleSys, battleCtx, i, 0);
                    }
                }
                break;
            case SelectorEnemySide:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, i)!.BattlerType & 1) != 0)
                    {
                        BattleIO.BattleIO_SlideHealthboxIn(battleSys, battleCtx, i, 0);
                    }
                }
                break;
            default:
                BattleIO.BattleIO_SlideHealthboxIn(battleSys, battleCtx, BattleScript_Battler(battleSys, battleCtx, selector), 0);
                break;
        }
        return 0;
    }

    // OFFSET 0x022411C0
    private static int BattleScript_SlideHealthboxInWait(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        byte delay = 0;
        switch (selector)
        {
            case SelectorAll:
                for (int i = 0; i < maxBattlers; i++)
                {
                    BattleIO.BattleIO_SlideHealthboxIn(battleSys, battleCtx, i, 0);
                }
                break;
            case SelectorPlayerSide:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, i)!.BattlerType & 1) == 0)
                    {
                        BattleIO.BattleIO_SlideHealthboxIn(battleSys, battleCtx, i, delay);
                        delay += HealthboxStaggerDelay;
                    }
                }
                break;
            case SelectorEnemySide:
                for (int i = 0; i < maxBattlers; i++)
                {
                    if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, i)!.BattlerType & 1) != 0)
                    {
                        BattleIO.BattleIO_SlideHealthboxIn(battleSys, battleCtx, i, delay);
                        delay += HealthboxStaggerDelay;
                    }
                }
                break;
            default:
                BattleIO.BattleIO_SlideHealthboxIn(battleSys, battleCtx, BattleScript_Battler(battleSys, battleCtx, selector), 0);
                break;
        }
        return 0;
    }
}
