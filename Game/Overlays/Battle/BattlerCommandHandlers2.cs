namespace PlatinumSharp.Game;

public static partial class Battlers
{
    private const uint VolatileTransformed = 0x200000;
    private const int MonDataMove1 = 0x36;
    private const int MonDataMove1PP = 0x3A;
    private const int MonDataHeldItem = 6;
    private const int MonDataCurHP = 0xA3;
    private const int MonDataStatus = 0xA0;
    private const int MonDataForm = 0x70;
    private const int MonDataAbility = 0xA;
    private const int PanelSlideSpeed = -0xD00;
    private const int BattlerTypePlayerSlot2 = 4;
    private const byte AbilityOverrideNone = 0x68;
    private const ushort MoveHealBell = 0xD7;
    private const int AbilitySoundproof = 0x2B;

    // OFFSET 0x0225C47C
    private static void Battler_CommandUpdatePartyMon(BattleSystem battleSys, BattlerData battler)
    {
        byte[] data = battler.Data;
        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler.Battler, data[1] & 0xF);
        if ((BattleIO.R32(data, 0x1C) & VolatileTransformed) == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                if ((HealthBar.FlagIndex(i) & ((data[1] >> 4) & 0xF)) == 0)
                {
                    Pokemon.Pokemon_SetValue(mon, MonDataMove1 + i, BattleIO.R16(data, 0xE + i * 2));
                    Pokemon.Pokemon_SetValue(mon, MonDataMove1PP + i, (int)data[0x16 + i]);
                }
            }
        }
        if ((HealthBar.FlagIndex(data[1] & 0xF) & BattleIO.R32(data, 8)) == 0)
        {
            Pokemon.Pokemon_SetValue(mon, MonDataHeldItem, BattleIO.R16(data, 0xC));
        }
        Pokemon.Pokemon_SetValue(mon, MonDataCurHP, BattleIO.R16(data, 2));
        Pokemon.Pokemon_SetValue(mon, MonDataStatus, (int)BattleIO.R32(data, 4));
        if (BattleIO.R16(data, 0x2A) != 0)
        {
            Pokemon.Pokemon_SetValue(mon, MonDataForm, BattleIO.R16(data, 0x20));
        }
        if (BattleIO.R16(data, 0x28) != 0)
        {
            Pokemon.Pokemon_SetValue(mon, MonDataAbility, (int)BattleIO.R32(data, 0x24));
            Pokemon.Pokemon_CalcLevelAndStats(mon);
        }
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, data[0]);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C558
    private static void Battler_CommandSlidePartyGauge(BattleSystem battleSys, BattlerData battler)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        BattleSubscreen subscreen = BattleSystems.BattleSystem_GetBattleSubscreen(battleSys)!;
        if (battler.BootState == 0 && ((battleType & BattleSystems.BattleTypeMulti) != 0 || battler.BattlerType != BattlerTypePlayerSlot2))
        {
            BattleSubscreens.BattleSubscreen_StartPanelSlide(subscreen, PanelSlideSpeed, 0);
        }
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, battler.Data[0]);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C5B0
    private static void Battler_CommandStopCommandIdle(BattleSystem battleSys, BattlerData battler)
    {
        Healthboxes.Healthbox_StopArrowAnim(battler.Healthbox);
        Battler_StopIdleBob(battler);
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, battler.Data[0]);
        Battler_ClearCommand(battler);
    }

    // OFFSET 0x0225C5E0
    private static void Battler_CommandCurePartyStatus(BattleSystem battleSys, BattlerData battler)
    {
        byte[] data = battler.Data;
        int count = BattleSystems.BattleSystem_GetPartyCount(battleSys, battler.Battler);
        for (int i = 0; i < count; i++)
        {
            Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler.Battler, i);
            int ability = data[1] == AbilityOverrideNone ? 0 : (int)Pokemon.Pokemon_GetValue(mon, MonDataAbility, null);
            if (BattleIO.R16(data, 2) != MoveHealBell || ability != AbilitySoundproof)
            {
                Pokemon.Pokemon_SetValue(mon, MonDataStatus, 0);
            }
        }
        BattleIO.BattleIO_ClearMessage(battleSys, battler.Battler, data[0]);
        Battler_ClearCommand(battler);
    }
}
