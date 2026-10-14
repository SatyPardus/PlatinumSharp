namespace PlatinumSharp.Game;

public static partial class Battle
{
    private const int RedHPSoundDelayLimit = 0xF;

    // OFFSET 0x0223F36C
    public static void BattleSystem_LoadHealthboxes(BattleSystem battleSys)
    {
        for (int i = 0; i < battleSys.MaxBattlers; i++)
        {
            HealthBox healthbox = Battlers.Battler_GetHealthbox(battleSys.Battlers[i]!);
            healthbox.BattleSys = battleSys;
            healthbox.Type = Healthboxes.Healthbox_Type(Battlers.Battler_GetType(battleSys.Battlers[i]!), BattleSystems.BattleSystem_GetBattleType(battleSys));
            Healthboxes.Healthbox_LoadResources(healthbox);
            Healthboxes.Healthbox_Enable(healthbox, 0);
        }
    }

    // OFFSET 0x0223F3BC
    public static void BattleSystem_ShowActiveHealthboxes(BattleSystem battleSys)
    {
        for (int i = 0; i < battleSys.MaxBattlers; i++)
        {
            HealthBox healthbox = Battlers.Battler_GetHealthbox(battleSys.Battlers[i]!);
            if (healthbox.CurHP != 0)
            {
                Healthboxes.Healthbox_Enable(healthbox, 1);
            }
        }
    }

    // OFFSET 0x0223F3EC
    public static void BattleSystem_HideHealthboxes(BattleSystem battleSys)
    {
        for (int i = 0; i < battleSys.MaxBattlers; i++)
        {
            Healthboxes.Healthbox_Enable(Battlers.Battler_GetHealthbox(battleSys.Battlers[i]!), 0);
        }
    }

    // OFFSET 0x0223F414
    public static void BattleSystem_FreeHealthboxes(BattleSystem battleSys)
    {
        for (int i = 0; i < battleSys.MaxBattlers; i++)
        {
            Healthboxes.Healthbox_Free(Battlers.Battler_GetHealthbox(battleSys.Battlers[i]!));
        }
    }

    // OFFSET 0x0223F438
    public static byte BattleSystem_GetResultMask(BattleSystem battleSys)
    {
        return battleSys.ResultMask;
    }

    // OFFSET 0x0223F444
    public static void BattleSystem_SetResultMask(BattleSystem battleSys, int resultMask)
    {
        battleSys.ResultMask = (byte)resultMask;
    }

    // OFFSET 0x0223F450
    public static int BattleSystem_GetRedHPSoundFlag(BattleSystem battleSys)
    {
        return battleSys.RedHPSoundFlag;
    }

    // OFFSET 0x0223F460
    public static void BattleSystem_SetRedHPSoundFlag(BattleSystem battleSys, int flag)
    {
        battleSys.RedHPSoundFlag = flag & 3;
    }

    // OFFSET 0x0223F47C
    public static int BattleSystem_GetRedHPSoundEffectDelay(BattleSystem battleSys)
    {
        return battleSys.RedHPSoundEffectDelay;
    }

    // OFFSET 0x0223F48C
    public static void BattleSystem_SetRedHPSoundEffectDelay(BattleSystem battleSys, int delay)
    {
        if (delay >= RedHPSoundDelayLimit)
        {
            ErrorHandling.GF_AssertFail();
        }
        battleSys.RedHPSoundEffectDelay = delay & 7;
    }

    // OFFSET 0x0223F4B0
    public static void BattleSystem_SetCaughtBattlerIdx(BattleSystem battleSys, int battler)
    {
        battleSys.CaughtBattlerIdx = battler;
    }
}
