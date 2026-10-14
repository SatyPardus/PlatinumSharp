namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int AbilityMultitype = 0x79;
    private const int MoveConversion = 0xA0;
    private const int TypeMystery = 9;
    private const int TypeGhost = 7;
    private const int TypeNormal = 0;
    private const int BattleMonType1 = 0x1B;
    private const int BattleMonType2 = 0x1C;
    private const uint SideConditionReflect = 1;
    private const uint SideConditionLightScreen = 2;
    private const uint SideConditionMist = 0x40;
    private const uint MoveStatusFailed = 0x40;
    private const int HoldEffectExtendScreens = 0x61;
    private const int TagMoveNicknameDummy = 0x14;
    private const int MsgLightScreenSingle = 0xBE;
    private const int MsgLightScreenDouble = 0xC0;
    private const int MsgReflectSingle = 0xC2;
    private const int MsgReflectDouble = 0xC4;
    private const int ScreenTurns = 5;
    private const int MistTurns = 5;
    private const int PayDayMax = 0xFFFF;
    private const int CountAliveOnSide = 1;

    private static int BattleScript_ConversionType(BattleContext battleCtx, ushort move)
    {
        int type = battleCtx.AiContext.MoveTable[move].Type;
        if (type == TypeMystery)
        {
            if (BattleContexts.BattleMon_Get(battleCtx, battleCtx.Attacker, BattleMonType1, null) == TypeGhost || BattleContexts.BattleMon_Get(battleCtx, battleCtx.Attacker, BattleMonType2, null) == TypeGhost)
            {
                type = TypeGhost;
            }
            else
            {
                type = TypeNormal;
            }
        }
        return type;
    }

    private static bool BattleScript_TypeMatchesAttacker(BattleContext battleCtx, int type)
    {
        return type == BattleContexts.BattleMon_Get(battleCtx, battleCtx.Attacker, BattleMonType1, null) || type == BattleContexts.BattleMon_Get(battleCtx, battleCtx.Attacker, BattleMonType2, null);
    }

    // OFFSET 0x0224358C
    private static int BattleScript_TryConversion(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        if (BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) == AbilityMultitype)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        int moveCount;
        for (moveCount = 0; moveCount < 4; moveCount++)
        {
            if (battleCtx.BattleMons[battleCtx.Attacker].Moves[moveCount] == 0)
            {
                break;
            }
        }
        int i;
        for (i = 0; i < moveCount; i++)
        {
            ushort move = battleCtx.BattleMons[battleCtx.Attacker].Moves[i];
            if (move == MoveConversion)
            {
                continue;
            }
            int type = BattleScript_ConversionType(battleCtx, move);
            if (!BattleScript_TypeMatchesAttacker(battleCtx, type))
            {
                break;
            }
        }
        if (i == moveCount)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        int newType;
        while (true)
        {
            int index = BattleSystems.BattleSystem_RandNext(battleSys) % moveCount;
            ushort move = battleCtx.BattleMons[battleCtx.Attacker].Moves[index];
            if (move == MoveConversion)
            {
                continue;
            }
            newType = BattleScript_ConversionType(battleCtx, move);
            if (!BattleScript_TypeMatchesAttacker(battleCtx, newType))
            {
                break;
            }
        }
        battleCtx.BattleMons[battleCtx.Attacker].Type1 = (byte)newType;
        battleCtx.BattleMons[battleCtx.Attacker].Type2 = (byte)newType;
        battleCtx.MsgTemp = newType;
        return 0;
    }

    private static bool BattleScript_CompareUnsigned(int op, int data, int value)
    {
        uint a = (uint)data;
        uint b = (uint)value;
        switch (op)
        {
            case CompareEqual:
                return a == b;
            case CompareNotEqual:
                return a != b;
            case CompareGreater:
                return a > b;
            case CompareLessOrEqual:
                return a <= b;
            case CompareFlagSet:
                return (a & b) != 0;
            case CompareFlagNotSet:
                return (a & b) == 0;
            case CompareAllFlagsSet:
                return (b & a) == b;
        }
        return true;
    }

    // OFFSET 0x02243708
    private static int BattleScript_CompareVarToVar(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int op = BattleScript_Read(battleCtx);
        int var1 = BattleScript_Read(battleCtx);
        int var2 = BattleScript_Read(battleCtx);
        int jump = BattleScript_Read(battleCtx);
        int value1 = BattleScript_VarAddress(battleSys, battleCtx, var1);
        int value2 = BattleScript_VarAddress(battleSys, battleCtx, var2);
        if (!BattleScript_CompareUnsigned(op, value1, value2))
        {
            jump = 0;
        }
        if (jump != 0)
        {
            BattleScript_Iter(battleCtx, jump);
        }
        return 0;
    }

    // OFFSET 0x022437D4
    private static int BattleScript_CompareMonDataToVar(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int op = BattleScript_Read(battleCtx);
        int selector = BattleScript_Read(battleCtx);
        int param = BattleScript_Read(battleCtx);
        int var = BattleScript_Read(battleCtx);
        int jump = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        int data = BattleContexts.BattleMon_Get(battleCtx, battler, param, null);
        int value = BattleScript_VarAddress(battleSys, battleCtx, var);
        if (!BattleScript_CompareUnsigned(op, data, value))
        {
            jump = 0;
        }
        if (jump != 0)
        {
            BattleScript_Iter(battleCtx, jump);
        }
        return 0;
    }

    // OFFSET 0x022438A8
    private static int BattleScript_AddPayDayMoney(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        battleCtx.MsgTemp = (int)(battleCtx.PayDayCount * battleCtx.PrizeMoneyMul);
        if (battleCtx.MsgTemp > PayDayMax)
        {
            battleCtx.MsgTemp = PayDayMax;
        }
        TrainerInfo.TrainerInfo_GiveMoney(BattleSystems.BattleSystem_GetTrainerInfo(battleSys, 0), (uint)battleCtx.MsgTemp);
        return 0;
    }

    private static void BattleScript_SetScreenMessage(BattleSystem battleSys, BattleContext battleCtx, int singleMsg, int doubleMsg)
    {
        battleCtx.MsgBuffer.Tags = TagMoveNicknameDummy;
        battleCtx.MsgBuffer.Params[0] = (int)battleCtx.MoveCur;
        battleCtx.MsgBuffer.Params[1] = battleCtx.Attacker;
        if (BattleContexts.BattleSystem_NumBattlersAlive(battleSys, battleCtx, CountAliveOnSide, battleCtx.Attacker) == 2)
        {
            battleCtx.MsgBuffer.Id = (ushort)doubleMsg;
        }
        else
        {
            battleCtx.MsgBuffer.Id = (ushort)singleMsg;
        }
    }

    // OFFSET 0x022438F8
    private static int BattleScript_TryLightScreen(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
        if ((battleCtx.SideConditionsMask[side] & SideConditionLightScreen) != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            battleCtx.MoveStatusFlags |= MoveStatusFailed;
            return 0;
        }
        battleCtx.SideConditionsMask[side] |= SideConditionLightScreen;
        battleCtx.SideConditions[side].LightScreenTurns = ScreenTurns;
        battleCtx.SideConditions[side].LightScreenUser = battleCtx.Attacker & 3;
        if (BattleContexts.Battler_HeldItemEffect(battleCtx, battleCtx.Attacker) == HoldEffectExtendScreens)
        {
            int power = BattleContexts.Battler_HeldItemPower(battleCtx, battleCtx.Attacker, 0);
            battleCtx.SideConditions[side].LightScreenTurns = (battleCtx.SideConditions[side].LightScreenTurns + power) & 7;
        }
        BattleScript_SetScreenMessage(battleSys, battleCtx, MsgLightScreenSingle, MsgLightScreenDouble);
        return 0;
    }

    // OFFSET 0x022439D8
    private static int BattleScript_TryReflect(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
        if ((battleCtx.SideConditionsMask[side] & SideConditionReflect) != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            battleCtx.MoveStatusFlags |= MoveStatusFailed;
            return 0;
        }
        battleCtx.SideConditionsMask[side] |= SideConditionReflect;
        battleCtx.SideConditions[side].ReflectTurns = ScreenTurns;
        battleCtx.SideConditions[side].ReflectUser = battleCtx.Attacker & 3;
        if (BattleContexts.Battler_HeldItemEffect(battleCtx, battleCtx.Attacker) == HoldEffectExtendScreens)
        {
            int power = BattleContexts.Battler_HeldItemPower(battleCtx, battleCtx.Attacker, 0);
            battleCtx.SideConditions[side].ReflectTurns = (battleCtx.SideConditions[side].ReflectTurns + power) & 7;
        }
        BattleScript_SetScreenMessage(battleSys, battleCtx, MsgReflectSingle, MsgReflectDouble);
        return 0;
    }

    // OFFSET 0x02243AB8
    private static int BattleScript_TryMist(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int side = BattleSystems.BattleSystem_GetBattlerSide(battleSys, battleCtx.Attacker);
        if ((battleCtx.SideConditionsMask[side] & SideConditionMist) != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            battleCtx.MoveStatusFlags |= MoveStatusFailed;
            return 0;
        }
        battleCtx.SideConditionsMask[side] |= SideConditionMist;
        battleCtx.SideConditions[side].MistTurns = MistTurns;
        battleCtx.SideConditions[side].MistUser = battleCtx.Attacker & 3;
        return 0;
    }
}
