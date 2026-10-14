namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int BattlerDataEnemyFlag = 1;

    // OFFSET 0x02248040
    private static int BattleScript_TryEscape(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int jumpOnEscape = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        if (BattleContexts.Battler_CanEscape(battleSys, battleCtx, battler) != 0)
        {
            BattleScript_Iter(battleCtx, jumpOnEscape);
        }
        return 0;
    }

    // OFFSET 0x02248164
    private static int BattleScript_IncrementGameRecord(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int tags = BattleScript_Read(battleCtx);
        int record = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        BattleIO.BattleIO_PrintAlertMessage(battleSys, battler, tags, record);
        return 0;
    }

    // OFFSET 0x022481A4
    private static int BattleScript_RestoreSprite(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_RestoreSprite(battleSys, battleCtx, battler);
        return 0;
    }

    // OFFSET 0x022481D0
    private static int BattleScript_TriggerAbilityOnHit(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpIfNone = BattleScript_Read(battleCtx);
        if (BattleContexts.BattleSystem_TriggerAbilityOnHit(battleSys, battleCtx, ref battleCtx.ScriptTemp) == 0)
        {
            BattleScript_Iter(battleCtx, jumpIfNone);
        }
        return 0;
    }

    private static void BattleScript_ForEachSide(BattleSystem battleSys, BattleContext battleCtx, int selector, int maxBattlers, System.Action<BattleSystem, int> action)
    {
        if (selector == SelectorPlayerSide)
        {
            for (int battler = 0; battler < maxBattlers; battler++)
            {
                if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!.BattlerType & BattlerDataEnemyFlag) == 0)
                {
                    action(battleSys, battler);
                }
            }
        }
        else if (selector == SelectorEnemySide)
        {
            for (int battler = 0; battler < maxBattlers; battler++)
            {
                if ((BattleSystems.BattleSystem_GetBattlerData(battleSys, battler)!.BattlerType & BattlerDataEnemyFlag) != 0)
                {
                    action(battleSys, battler);
                }
            }
        }
        else
        {
            action(battleSys, BattleScript_Battler(battleSys, battleCtx, selector));
        }
    }

    // OFFSET 0x02248204
    private static int BattleScript_SpriteToOAM(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        BattleScript_ForEachSide(battleSys, battleCtx, selector, maxBattlers, BattleIO.BattleIO_SpriteToOAM);
        return 0;
    }

    // OFFSET 0x02248294
    private static int BattleScript_OAMToSprite(BattleSystem battleSys, BattleContext battleCtx)
    {
        int maxBattlers = BattleSystems.BattleSystem_GetMaxBattlers(battleSys);
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        BattleScript_ForEachSide(battleSys, battleCtx, selector, maxBattlers, BattleIO.BattleIO_OAMToSprite);
        return 0;
    }

    private static int BattleScript_PartyHP(Party party)
    {
        int total = 0;
        for (int slot = 0; slot < Party.Party_GetCurrentCount(party); slot++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, slot);
            if (Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEggBeatUp, null) != 0 && Pokemon.Pokemon_GetValue(mon, MonDataSpeciesEggBeatUp, null) != SpeciesEgg)
            {
                total += (int)Pokemon.Pokemon_GetValue(mon, MonDataCurHP, null);
            }
        }
        return total;
    }

    // OFFSET 0x02248324
    private static int BattleScript_CheckBlackOut(BattleSystem battleSys, BattleContext battleCtx)
    {
        int total = 0;
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int jumpIfBlackOut = BattleScript_Read(battleCtx);
        uint battleType = BattleSystems.BattleSystem_GetBattleType(battleSys);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        if ((battleType & BattleTypeMulti) != 0 || ((battleType & BattleTypeTag) != 0 && BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) != 0))
        {
            Party party = BattleSystems.BattleSystem_GetParty(battleSys, battler);
            Party partnerParty = BattleSystems.BattleSystem_GetParty(battleSys, BattleSystems.BattleSystem_GetPartner(battleSys, battler));
            BattleSystems.BattleSystem_GetBattlerData(battleSys, battler);
            total += BattleScript_PartyHP(party);
            if (!((uint)(battleType - 0x4A) <= 1 && BattleSystems.BattleSystem_GetBattlerSide(battleSys, battler) == 0 && BattleSystems.BattleSystem_BattlerSlot(battleSys, battler) == 2))
            {
                total += BattleScript_PartyHP(partnerParty);
            }
        }
        else
        {
            Party party = BattleSystems.BattleSystem_GetParty(battleSys, battler);
            BattleSystems.BattleSystem_GetBattlerData(battleSys, battler);
            total += BattleScript_PartyHP(party);
        }
        if (total == 0)
        {
            BattleScript_Iter(battleCtx, jumpIfBlackOut);
        }
        return 0;
    }
}

public static partial class BattleIO
{
    private const int CommandRestoreSprite = 0x38;
    private const int CommandSpriteToOAM = 0x39;
    private const int CommandOAMToSprite = 0x3A;
    private const int RestoreSpriteSize = 0x58;

    // OFFSET 0x0226673C
    public static void BattleIO_RestoreSprite(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        byte[] message = new byte[RestoreSpriteSize];
        W8(message, 0, CommandRestoreSprite);
        for (int i = 0; i < 4; i++)
        {
            BattleMon mon = battleCtx.BattleMons[i];
            W16(message, 0x18 + i * 2, mon.Species);
            W8(message, 0x24 + i, mon.IsShiny & 1);
            W8(message, 0x28 + i, mon.FormNum & 0x1F);
            if ((mon.StatusVolatile & 0x200000) != 0)
            {
                W8(message, 0x20 + i, mon.MoveEffectsData.TransformedGender);
                W32(message, 0x2C + i * 4, mon.MoveEffectsData.TransformedPID);
            }
            else
            {
                W8(message, 0x20 + i, mon.Gender & 0xF);
                W32(message, 0x2C + i * 4, mon.Personality);
            }
        }
        BattleIO_Send(battleSys, RecipientBattler, battler, message, RestoreSpriteSize);
    }

    // OFFSET 0x022667E8
    public static void BattleIO_SpriteToOAM(BattleSystem battleSys, int battler)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandSpriteToOAM);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x02266804
    public static void BattleIO_OAMToSprite(BattleSystem battleSys, int battler)
    {
        byte[] message = new byte[4];
        W32(message, 0, CommandOAMToSprite);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }
}
