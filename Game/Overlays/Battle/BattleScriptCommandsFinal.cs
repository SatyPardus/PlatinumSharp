namespace PlatinumSharp.Game;

public static partial class BattleScripts
{
    private const int PartyFull = 6;
    private const int NoEmptyBox = 0x12;

    // OFFSET 0x02248800
    private static int BattleScript_CheckIgnoreWeather(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpIfIgnored = BattleScript_Read(battleCtx);
        if (BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAllBattlers, 0, AbilityCloudNine) != 0 || BattleContexts.BattleSystem_CountAbility(battleSys, battleCtx, CountAllBattlers, 0, AbilityAirLock) != 0)
        {
            BattleScript_Iter(battleCtx, jumpIfIgnored);
        }
        return 0;
    }

    // OFFSET 0x02248850
    private static int BattleScript_SetRandomTarget(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        battleCtx.Defender = BattleContexts.BattleSystem_RandomOpponent(battleSys, battleCtx, battler);
        return 0;
    }

    // OFFSET 0x02248880
    private static int BattleScript_TriggerHeldItemOnPivotMove(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpIfNone = BattleScript_Read(battleCtx);
        if (BattleContexts.BattleSystem_TriggerHeldItemAfterMove(battleSys, battleCtx, ref battleCtx.ScriptTemp) == 0)
        {
            BattleScript_Iter(battleCtx, jumpIfNone);
        }
        return 0;
    }

    // OFFSET 0x022488B4
    private static int BattleScript_RefreshSprite(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_RefreshSprite(battleSys, battleCtx, battler);
        return 0;
    }

    // OFFSET 0x022488E0
    private static int BattleScript_PlayMoveHitSound(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleIO.BattleIO_PlayMoveHitSound(battleSys, battleCtx, battler);
        return 0;
    }

    // OFFSET 0x0224890C
    private static int BattleScript_PlayBGM(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int selector = BattleScript_Read(battleCtx);
        int bgm = BattleScript_Read(battleCtx);
        int battler = BattleScript_Battler(battleSys, battleCtx, selector);
        BattleIO.BattleIO_PlayBGM(battleSys, battler, bgm);
        return 0;
    }

    // OFFSET 0x02248944
    private static int BattleScript_CheckSafariGameDone(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int jumpIfContinue = BattleScript_Read(battleCtx);
        if (BattleSystems.BattleSystem_GetPartyCount(battleSys, 0) == PartyFull && PCBoxes.PCBoxes_FirstEmptyBox(battleSys.PcBoxes!) == NoEmptyBox)
        {
            return 0;
        }
        if (battleSys.SafariBalls != 0)
        {
            BattleScript_Iter(battleCtx, jumpIfContinue);
        }
        return 0;
    }

    // OFFSET 0x022489F4
    private static int BattleScript_CheckCurMoveIsType(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int type = BattleScript_Read(battleCtx);
        int jump = BattleScript_Read(battleCtx);
        if (type == battleCtx.AiContext.MoveTable[battleCtx.MoveCur].Type)
        {
            BattleScript_Iter(battleCtx, jump);
        }
        return 0;
    }

    // OFFSET 0x02248A34
    private static int BattleScript_LoadArchivedMonData(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int species = BattleScript_Read(battleCtx);
        int formVar = BattleScript_Read(battleCtx);
        int param = BattleScript_Read(battleCtx);
        int form = BattleScript_VarAddress(battleSys, battleCtx, formVar);
        battleCtx.CalcTemp = (int)SpeciesData.SpeciesData_GetFormValue(species, form, param);
        return 0;
    }

    // OFFSET 0x02248A7C
    private static int BattleScript_RefreshMonData(BattleSystem battleSys, BattleContext battleCtx)
    {
        BattleScript_Iter(battleCtx, 1);
        int battler = BattleScript_Battler(battleSys, battleCtx, BattleScript_Read(battleCtx));
        BattleControllers.BattleSystem_ReloadMonData(battleSys, battleCtx, battler, battleCtx.SelectedPartySlot[battler]);
        return 0;
    }
}

public static partial class BattleControllers
{
    private const int MonDataAttackValue = 0xA5;
    private const int MonDataDefenseValue = 0xA6;
    private const int MonDataSpeedValue = 0xA7;
    private const int MonDataSpAttackValue = 0xA8;
    private const int MonDataSpDefenseValue = 0xA9;
    private const int MonDataLevelValue = 0xA1;
    private const int MonDataFriendshipValue = 9;
    private const int MonDataMaxHPValue = 0xA4;
    private const int MonDataExpValue = 8;
    private const int MonDataMove1Value = 0x36;
    private const int MonDataMove1PPValue = 0x3A;
    private const int MonDataMove1PPUpsValue = 0x3E;
    private const uint VolatileTransformedMon = 0x200000;

    // OFFSET 0x02251C94
    public static void BattleSystem_ReloadMonData(BattleSystem battleSys, BattleContext battleCtx, int battler, int partySlot)
    {
        Pokemon mon = BattleSystems.BattleSystem_GetPartyPokemon(battleSys, battler, partySlot);
        BattleMon battleMon = battleCtx.BattleMons[battler];
        battleMon.Attack = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataAttackValue, null);
        battleMon.Defense = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataDefenseValue, null);
        battleMon.Speed = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpeedValue, null);
        battleMon.SpAttack = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpAttackValue, null);
        battleMon.SpDefense = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataSpDefenseValue, null);
        battleMon.Level = (byte)Pokemon.Pokemon_GetValue(mon, MonDataLevelValue, null);
        battleMon.Friendship = (byte)Pokemon.Pokemon_GetValue(mon, MonDataFriendshipValue, null);
        battleMon.CurHP = (int)Pokemon.Pokemon_GetValue(mon, MonDataHPValue, null);
        battleMon.MaxHP = Pokemon.Pokemon_GetValue(mon, MonDataMaxHPValue, null);
        if ((battleMon.StatusVolatile & VolatileTransformedMon) != 0)
        {
            return;
        }
        for (int slot = 0; slot < 4; slot++)
        {
            if ((HealthBar.FlagIndex(slot) & (uint)battleMon.MoveEffectsData.MimickedMoveSlot) != 0)
            {
                continue;
            }
            battleMon.Moves[slot] = (ushort)Pokemon.Pokemon_GetValue(mon, MonDataMove1Value + slot, null);
            battleMon.PpCur[slot] = (byte)Pokemon.Pokemon_GetValue(mon, MonDataMove1PPValue + slot, null);
            battleMon.PpUps[slot] = (byte)Pokemon.Pokemon_GetValue(mon, MonDataMove1PPUpsValue + slot, null);
        }
        battleMon.Exp = Pokemon.Pokemon_GetValue(mon, MonDataExpValue, null);
    }
}

public static partial class BattleIO
{
    private const int CommandRefreshSprite = 0x3E;
    private const int CommandPlayMoveHitSound = 0x3F;
    private const int CommandPlayBGM = 0x40;
    private const uint MoveStatusSuperEffective = 2;
    private const uint MoveStatusNotVeryEffective = 4;

    // OFFSET 0x0226692C
    public static void BattleIO_RefreshSprite(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        byte[] message = new byte[RestoreSpriteSize];
        W8(message, 0, CommandRefreshSprite);
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

    // OFFSET 0x022669D8
    public static void BattleIO_PlayMoveHitSound(BattleSystem battleSys, BattleContext battleCtx, int battler)
    {
        byte[] message = new byte[4];
        W8(message, 0, CommandPlayMoveHitSound);
        uint flags = battleCtx.MoveStatusFlags;
        if ((flags & MoveStatusSuperEffective) != 0)
        {
            W8(message, 1, 2);
        }
        else if ((flags & MoveStatusNotVeryEffective) != 0)
        {
            W8(message, 1, 1);
        }
        else
        {
            W8(message, 1, 0);
        }
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }

    // OFFSET 0x02266A18
    public static void BattleIO_PlayBGM(BattleSystem battleSys, int battler, int bgm)
    {
        byte[] message = new byte[4];
        W8(message, 0, CommandPlayBGM);
        W16(message, 2, bgm);
        BattleIO_Send(battleSys, RecipientBattler, battler, message, 4);
    }
}

public sealed partial class PCBoxes
{
    private const int BoxMonSpeciesExists = 0xAC;

    // OFFSET 0x020799A0
    public static int PCBoxes_FirstEmptyBox(PCBoxes boxes)
    {
        int box = (int)boxes.U32(0);
        do
        {
            for (int slot = 0; slot < MonsPerBox; slot++)
            {
                BoxPokemon boxMon = new(boxes.Buffer, MonsOffset + box * BoxSize + slot * BoxPokemon.Size);
                if (Pokemon.BoxPokemon_GetValue(boxMon, BoxMonSpeciesExists, null) == 0)
                {
                    return box;
                }
            }
            box++;
            if (box >= BoxCount)
            {
                box = 0;
            }
        }
        while (box != boxes.U32(0));
        return BoxCount;
    }
}
