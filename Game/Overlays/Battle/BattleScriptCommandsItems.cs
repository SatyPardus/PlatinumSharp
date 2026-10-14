namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int ItemDataHoldEffect = 1;
    private const int ItemDataHoldEffectParam = 2;
    private const int TerrainMax = 12;
    private const uint CamouflageTypeTableAddress = 0x0226EAC0;
    private const uint NaturePowerMoveTableAddress = 0x0226E652;
    private const uint SecretPowerEffectTableAddress = 0x0226E690;

    // OFFSET 0x022478E4
    private static int BattleScript_CheckItemHoldEffect(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int mode = BattleScript_Read(battleCtx);
        int selector = BattleScript_Read(battleCtx);
        int effect = BattleScript_Read(battleCtx);
        int jump = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        if (mode == CheckHave)
        {
            if (effect == BattleContexts.Battler_HeldItemEffect(battleCtx, battler))
            {
                BattleScript_Iter(battleCtx, jump);
            }
        }
        else if (effect != BattleContexts.Battler_HeldItemEffect(battleCtx, battler))
        {
            BattleScript_Iter(battleCtx, jump);
        }
        return 0;
    }

    // OFFSET 0x02247950
    private static int BattleScript_GetItemHoldEffect(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int var = BattleScript_Read(battleCtx);
        ref int data = ref BattleScript_VarAddress(battleSys, battleCtx, var);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        data = BattleContexts.BattleSystem_GetItemData(battleCtx, BattleContexts.Battler_HeldItem(battleCtx, battler), ItemDataHoldEffect);
        return 0;
    }

    // OFFSET 0x0224799C
    private static int BattleScript_GetItemEffectParam(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int var = BattleScript_Read(battleCtx);
        ref int data = ref BattleScript_VarAddress(battleSys, battleCtx, var);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        data = BattleContexts.BattleSystem_GetItemData(battleCtx, BattleContexts.Battler_HeldItem(battleCtx, battler), ItemDataHoldEffectParam);
        return 0;
    }

    private static int BattleScript_CappedTerrain(BattleSystem battleSys)
    {
        int terrain = BattleSystems.BattleSystem_GetTerrain(battleSys);
        if (terrain > TerrainMax)
        {
            terrain = TerrainMax;
        }
        return terrain;
    }

    // OFFSET 0x022479E8
    private static int BattleScript_TryCamouflage(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        if (BattleContexts.Battler_Ability(battleCtx, battleCtx.Attacker) == AbilityMultitype)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
            return 0;
        }
        int terrain = BattleScript_CappedTerrain(battleSys);
        byte type = RomData.Overlay(BattleOverlay, (uint)(CamouflageTypeTableAddress + terrain), 1)[0];
        if (type == BattleContexts.BattleMon_Get(battleCtx, battleCtx.Attacker, BattleMonType1, null) || type == BattleContexts.BattleMon_Get(battleCtx, battleCtx.Attacker, BattleMonType2, null))
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        else
        {
            battleCtx.BattleMons[battleCtx.Attacker].Type1 = type;
            battleCtx.BattleMons[battleCtx.Attacker].Type2 = type;
            battleCtx.MsgTemp = type;
        }
        return 0;
    }

    // OFFSET 0x02247A80
    private static int BattleScript_GetTerrainMove(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int terrain = BattleScript_CappedTerrain(battleSys);
        battleCtx.MsgMoveTemp = RomData.OverlayU16(BattleOverlay, (uint)(NaturePowerMoveTableAddress + terrain * 2), 1)[0];
        return 0;
    }

    // OFFSET 0x02247AB0
    private static int BattleScript_GetTerrainSecondaryEffect(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int terrain = BattleScript_CappedTerrain(battleSys);
        battleCtx.SideEffectIndirectFlags = RomData.OverlayU32(BattleOverlay, (uint)(SecretPowerEffectTableAddress + terrain * 4), 1)[0];
        return 0;
    }

    // OFFSET 0x02247AE0
    private static int BattleScript_CalcNaturalGiftParams(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        int power = BattleContexts.Battler_NaturalGiftPower(battleCtx, battleCtx.Attacker);
        if (power != 0)
        {
            battleCtx.MovePower = power;
            battleCtx.MoveType = BattleContexts.Battler_NaturalGiftType(battleCtx, battleCtx.Attacker);
        }
        else
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x02247B28
    private static int BattleScript_TryPluck(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnStickyHold = BattleScript_Read(battleCtx);
        int jumpOnFail = BattleScript_Read(battleCtx);
        BattleMon defender = battleCtx.BattleMons[battleCtx.Defender];
        if (defender.HeldItem != 0 && BattleContexts.Battler_IgnorableAbility(battleCtx, battleCtx.Attacker, battleCtx.Defender, AbilityStickyHold) == 1)
        {
            BattleScript_Iter(battleCtx, jumpOnStickyHold);
            return 0;
        }
        if ((defender.HeldItem == 0 || defender.MoveEffectsData.CustapBerry == 0) && BattleContexts.BattleSystem_PluckBerry(battleSys, battleCtx, battleCtx.Defender) == 1)
        {
            return 0;
        }
        BattleScript_Iter(battleCtx, jumpOnFail);
        return 0;
    }

    // OFFSET 0x02247BA4
    private static int BattleScript_TryFling(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpOnFail = BattleScript_Read(battleCtx);
        if (BattleContexts.BattleSystem_FlingItem(battleSys, battleCtx, battleCtx.Attacker) != 1)
        {
            BattleScript_Iter(battleCtx, jumpOnFail);
        }
        return 0;
    }

    // OFFSET 0x02247BD4
    private static int BattleScript_YesNoMenu(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int type = BattleScript_Read(battleCtx);
        BattleIO.BattleIO_ShowYesNoMenu(battleSys, battleCtx, 0, 0, type, 0, 0);
        return 0;
    }
}
