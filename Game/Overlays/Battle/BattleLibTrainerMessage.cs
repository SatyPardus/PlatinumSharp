namespace PlatinumSharp.Game;

public static partial class BattleContexts
{
    private const uint BattleTypeNoTrainerMessages = 0x84;
    private const uint BattleTypeTrainerBattle = 0x1;
    private const uint BattleStatus2FirstDamageMessage = 0x20;
    private const int TrainerMessageFirstDamage = 0xD;
    private const int TrainerMessageHalfHP = 0xE;
    private const int TrainerMessageLastMon = 0xF;
    private const int TrainerMessageLastMonHalfHP = 0x10;
    private const byte TrainerMessageFlagHalfHP = 2;
    private const byte TrainerMessageFlagLastMon = 3;
    private const byte TrainerMessageFlagLastMonHalfHP = 4;
    private const int TrainerMessageStateCount = 4;
    private const int EnemyBattler = 1;

    private static int BattleSystem_CountAliveEnemyMons(BattleSystem battleSys)
    {
        Party party = BattleSystems.BattleSystem_GetParty(battleSys, EnemyBattler);
        int alive = 0;
        for (int i = 0; i < Party.Party_GetCurrentCount(party); i++)
        {
            if (Pokemon.Pokemon_GetValue(Party.Party_GetPokemonBySlotIndex(party, i), MonDataCurrentHP, null) != 0)
            {
                alive++;
            }
        }
        return alive;
    }

    // OFFSET 0x02253FCC
    public static int BattleSystem_CheckTrainerMessage(BattleSystem battleSys, BattleContext battleCtx)
    {
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        if ((battleType & BattleTypeNoTrainerMessages) != 0)
        {
            return 0;
        }
        if ((battleType & BattleTypeTrainerBattle) == 0)
        {
            return 0;
        }
        if ((battleType & BattleTypeDoubles) != 0)
        {
            return 0;
        }
        int trainerID = BattleSystems.BattleSystem_GetTrainerID(battleSys, EnemyBattler);
        BattleMon mon = battleCtx.BattleMons[EnemyBattler];
        int state = 0;
        do
        {
            switch (state)
            {
                case 0:
                    if (mon.TimesDamaged == 1 && (battleCtx.BattleStatusMask2 & BattleStatus2FirstDamageMessage) == 0
                        && TrainerData.Trainer_HasMessageType(trainerID, TrainerMessageFirstDamage, HeapIDBattleLib) != 0)
                    {
                        battleCtx.BattleStatusMask2 |= BattleStatus2FirstDamageMessage;
                        battleCtx.MsgTemp = TrainerMessageFirstDamage;
                        return 1;
                    }
                    state++;
                    break;
                case 1:
                    if ((mon.TrainerMessageFlags & TrainerMessageFlagHalfHP) == 0 && (uint)mon.CurHP <= mon.MaxHP / 2
                        && TrainerData.Trainer_HasMessageType(trainerID, TrainerMessageHalfHP, HeapIDBattleLib) != 0)
                    {
                        mon.TrainerMessageFlags |= TrainerMessageFlagHalfHP;
                        battleCtx.MsgTemp = TrainerMessageHalfHP;
                        return 1;
                    }
                    state++;
                    break;
                case 2:
                    if ((mon.TrainerMessageFlags & TrainerMessageFlagLastMon) == 0)
                    {
                        if (BattleSystem_CountAliveEnemyMons(battleSys) == 1
                            && TrainerData.Trainer_HasMessageType(trainerID, TrainerMessageLastMon, HeapIDBattleLib) != 0)
                        {
                            mon.TrainerMessageFlags |= TrainerMessageFlagLastMon;
                            battleCtx.MsgTemp = TrainerMessageLastMon;
                            return 1;
                        }
                    }
                    state++;
                    break;
                case 3:
                    if ((mon.TrainerMessageFlags & TrainerMessageFlagLastMonHalfHP) == 0)
                    {
                        if (BattleSystem_CountAliveEnemyMons(battleSys) == 1 && (uint)mon.CurHP <= mon.MaxHP / 2
                            && TrainerData.Trainer_HasMessageType(trainerID, TrainerMessageLastMonHalfHP, HeapIDBattleLib) != 0)
                        {
                            mon.TrainerMessageFlags |= TrainerMessageFlagLastMonHalfHP;
                            battleCtx.MsgTemp = TrainerMessageLastMonHalfHP;
                            return 1;
                        }
                    }
                    state++;
                    break;
            }
        } while (state != TrainerMessageStateCount);
        return 0;
    }
}
