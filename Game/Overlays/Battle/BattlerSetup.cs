namespace PlatinumSharp.Game;

public static partial class BattleSystems
{
    // OFFSET 0x0223EBEC
    public static uint BattleSystem_GetBattleStatusMask(BattleSystem battleSys)
    {
        return battleSys.BattleStatusMask;
    }
}

public static partial class Battlers
{
    public const int BootStatePlayer = 0;
    public const int BootStateAI = 1;
    public const int BootStateLink = 2;

    private const int HeapIDBattle = 5;
    private const int NarcBattlerGraphics = 0x93;
    private const uint BattleTypePokeDude = 0x40;
    private const uint BattleTypeLinkFrontier = 0x88;
    private const int BattlerTypeEnemy = 1;
    private static readonly int[] playerFuncs = { 0x022604C8, 0x02260C00, 0x02260F14, 0x022611DC, 0x02261E8C, 0x022623F0 };
    private static readonly int[] aiFuncs = { 0x02260AB4, 0x02260DB0, 0x022610A8, 0x02261D50, 0x02262258, 0x0226262C };
    private static readonly int[] recordedFuncs = { 0x02260BAC, 0x02260E98, 0x0226110C, 0x02261DE0, 0x0226232C, 0x0226266C };
    private static readonly int[] frontierEnemyFuncs = { 0x02260B84, 0, 0, 0, 0, 0 };
    private static readonly int[] safariEnemyFuncs = { 0x02260B04, 0, 0, 0, 0, 0 };
    private static readonly int[] frontierPlayerFuncs = { 0x022604C8, 0, 0, 0, 0, 0 };
    private static readonly int[] safariPlayerFuncs = { 0x022604C8, 0, 0, 0, 0, 0x022623F0 };

    // OFFSET 0x0225BFFC
    public static BattlerData Battler_New(BattleSystem battleSys, int battlerID, int battlerType)
    {
        BattlerData battler = new();
        battler.Battler = (byte)battlerID;
        battler.BattlerType = (byte)battlerType;
        battler.Narc = NARC.NARC_ctor(NarcBattlerGraphics, HeapIDBattle);
        return battler;
    }

    private static void Battler_SetTaskFuncs(BattlerData battler, int[] funcs, int bootState)
    {
        battler.TaskFuncs.SetCommandSelection = funcs[0];
        battler.TaskFuncs.ShowMoveSelectMenu = funcs[1];
        battler.TaskFuncs.ShowTargetSelectMenu = funcs[2];
        battler.TaskFuncs.ShowBagMenu = funcs[3];
        battler.TaskFuncs.ShowPartyMenu = funcs[4];
        battler.TaskFuncs.ShowYesNoMenu = funcs[5];
        battler.BootState = (byte)bootState;
    }

    // OFFSET 0x02263730
    public static void Battler_InitTaskFuncs(BattleSystem battleSys, BattlerData battler)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        uint battleStatus = BattleSystems.BattleSystem_GetBattleStatusMask(battleSys);
        bool recording = (battleStatus & BattleSystems.BattleStatusRecording) != 0;
        bool enemy = (battler.BattlerType & BattlerTypeEnemy) != 0;
        if ((battleType & BattleTypePokeDude) != 0)
        {
            if (battler.Battler != 0)
            {
                Battler_SetTaskFuncs(battler, aiFuncs, BootStateAI);
            }
            else if (recording)
            {
                Battler_SetTaskFuncs(battler, recordedFuncs, BootStatePlayer);
            }
            else
            {
                Battler_SetTaskFuncs(battler, playerFuncs, BootStatePlayer);
            }
        }
        else if ((battleType & BattleTypeLinkFrontier) == BattleTypeLinkFrontier || (battleType & BattleSystems.BattleTypeMulti) != 0 || (battleType & BattleSystems.BattleTypeLink) != 0)
        {
            throw Unported.Partial(0x02263730);
        }
        else if ((battleType & BattleSystems.BattleTypeFrontier) != 0)
        {
            if (enemy)
            {
                Battler_SetTaskFuncs(battler, frontierEnemyFuncs, BootStateAI);
            }
            else
            {
                Battler_SetTaskFuncs(battler, frontierPlayerFuncs, BootStatePlayer);
            }
        }
        else if ((battleType & BattleSystems.BattleTypeSafari) != 0)
        {
            if (enemy)
            {
                Battler_SetTaskFuncs(battler, safariEnemyFuncs, BootStateAI);
            }
            else
            {
                Battler_SetTaskFuncs(battler, safariPlayerFuncs, BootStatePlayer);
            }
        }
        else if (enemy)
        {
            Battler_SetTaskFuncs(battler, aiFuncs, BootStateAI);
        }
        else if (recording)
        {
            Battler_SetTaskFuncs(battler, recordedFuncs, BootStatePlayer);
        }
        else
        {
            Battler_SetTaskFuncs(battler, playerFuncs, BootStatePlayer);
        }
    }
}

public static partial class BattleContexts
{
    public const int ParamSideConditionsMask = 0;
    public const int ParamSideMistTurns = 1;
    public const int ParamSelectedPartySlot = 2;
    public const int ParamTotalTurns = 3;
    public const int ParamLevelUpMons = 4;
    public const int ParamSafariEscapeCount = 5;
    public const int ParamTotalFainted = 6;
    public const int ParamTotalDamage = 7;
    public const int ParamActionTemp = 8;
    public const int ParamAIDefender = 9;
    public const int ParamBattlersSwitchingMask = 10;
    public const int ParamAISelectedTarget = 11;
    public const int ParamActionTemp2 = 12;
    public const int ParamCommand = 13;
    public const int ParamCommandNext = 14;

    private const int PartySize = 6;
    private const int BattlerTypePlayerSide2 = 4;
    private const int BattlerTypeEnemySide2 = 5;
    private const uint BattleTypeMultiOrTag = 0x18;

    // OFFSET 0x0225A1B0
    public static void BattleContext_InitPartyOrder(BattleSystem battleSys, BattleContext battleCtx)
    {
        for (int battler = 0; battler < BattleSystems.BattleSystem_GetMaxBattlers(battleSys); battler++)
        {
            for (int i = 0; i < PartySize; i++)
            {
                battleCtx.PartyOrder[battler, i] = (byte)i;
            }
            BattleContext_SwitchPartyOrder(battleSys, battleCtx, battler, battleCtx.SelectedPartySlot[battler]);
        }
    }

    // OFFSET 0x0225A200
    public static void BattleContext_SwitchPartyOrder(BattleSystem battleSys, BattleContext battleCtx, int battler, int partySlot)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int orderSlot;
        if (((battleType & BattleSystems.BattleTypeDoubles) != 0 && (battleType & BattleTypeMultiOrTag) == 0) || ((battleType & BattleSystems.BattleTypeTag) != 0 && (BattleSystems.BattleSystem_BattlerSlot(battleSys, battler) & 1) == 0))
        {
            int type = BattleSystems.BattleSystem_BattlerSlot(battleSys, battler);
            orderSlot = type == BattlerTypePlayerSide2 || type == BattlerTypeEnemySide2 ? 1 : 0;
            battler &= 1;
        }
        else
        {
            orderSlot = 0;
        }
        int found;
        for (found = 0; found < PartySize; found++)
        {
            if (partySlot == battleCtx.PartyOrder[battler, found])
            {
                break;
            }
        }
        if (found >= PartySize)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        byte previous = battleCtx.PartyOrder[battler, orderSlot];
        battleCtx.PartyOrder[battler, orderSlot] = battleCtx.PartyOrder[battler, found];
        battleCtx.PartyOrder[battler, found] = previous;
    }

    // OFFSET 0x0225B45C
    public static int BattleContext_Get(BattleSystem battleSys, BattleContext battleCtx, int param, int battler)
    {
        switch (param)
        {
            case ParamSideConditionsMask:
                return (int)battleCtx.SideConditionsMask[BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler)];
            case ParamSideMistTurns:
                return battleCtx.SideConditions[BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler)].MistTurns;
            case ParamSelectedPartySlot:
                return battleCtx.SelectedPartySlot[battler];
            case ParamTotalTurns:
                return battleCtx.TotalTurns;
            case ParamLevelUpMons:
                return battleCtx.LevelUpMons;
            case ParamSafariEscapeCount:
                return battleCtx.SafariEscapeCount;
            case ParamTotalFainted:
                return battleCtx.TotalFainted[battler];
            case ParamTotalDamage:
                return battleCtx.TotalDamage[battler];
            case ParamActionTemp:
                return (int)battleCtx.BattlerActions[battler, 0];
            case ParamAIDefender:
                return battleCtx.AiContext.Defender;
            case ParamBattlersSwitchingMask:
                return battleCtx.BattlersSwitchingMask;
            case ParamAISelectedTarget:
                return battleCtx.AiContext.SelectedTarget[battler];
            case ParamActionTemp2:
                return (int)battleCtx.BattlerActions[battler, 2];
            case ParamCommand:
                return battleCtx.Command;
            case ParamCommandNext:
                return battleCtx.CommandNext;
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x0225B540
    public static void BattleContext_Set(BattleSystem battleSys, BattleContext battleCtx, int param, int battler, int value)
    {
        switch (param)
        {
            case ParamSideConditionsMask:
                battleCtx.SideConditionsMask[BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler)] = (uint)value;
                break;
            case ParamSideMistTurns:
                battleCtx.SideConditions[BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler)].MistTurns = value & 7;
                break;
            case ParamSelectedPartySlot:
                battleCtx.SelectedPartySlot[battler] = (byte)value;
                break;
            case ParamTotalTurns:
                battleCtx.TotalTurns = value;
                break;
            case ParamAIDefender:
                battleCtx.AiContext.Defender = (byte)value;
                break;
            case ParamAISelectedTarget:
                battleCtx.AiContext.SelectedTarget[battler] = (byte)value;
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
    }
}
