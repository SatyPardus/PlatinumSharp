namespace PlatinumSharp.Game;

public sealed class BattleFactorySave : BufferView
{
    public const int Size = 0x58;
    public const int FieldIsOpenLevel = 0;
    public const int FieldChallengeType = 1;
    public const int FieldCurrentBattleNum = 2;
    public const int FieldTrainerIDs = 3;
    public const int FieldPlayerPartySets = 4;
    public const int FieldPlayerIVs = 5;
    public const int FieldPlayerPersonality = 6;
    public const int FieldOpponentPartySets = 7;
    public const int FieldOpponentIVs = 8;
    public const int FieldOpponentPersonality = 9;

    private const int FrontierOffset = 0x8E0;
    private const int CurrentBattleOffset = 1;
    private const int TrainerIDsOffset = 4;
    private const int PlayerPartySetsOffset = 0x20;
    private const int PlayerIVsOffset = 0x28;
    private const int PlayerPersonalityOffset = 0x2C;
    private const int OpponentPartySetsOffset = 0x3C;
    private const int OpponentIVsOffset = 0x44;
    private const int OpponentPersonalityOffset = 0x48;
    private const int DidSaveShift = 4;

    public BattleFactorySave(byte[] buffer, int offset)
        : base(buffer, offset)
    {
    }

    // OFFSET 0x0202FF4C
    public static void BattleFactorySave_Init(BattleFactorySave save)
    {
        save.Clear(0, Size);
    }

    // OFFSET 0x0202FF58
    public static BattleFactorySave BattleFactorySave_Get(SaveData saveData)
    {
        BattleFrontierSave frontier = (BattleFrontierSave)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryFrontier);
        return new BattleFactorySave(frontier.Raw, FrontierOffset);
    }

    // OFFSET 0x0202FF68
    public static int BattleFactorySave_HasSaved(BattleFactorySave save)
    {
        return save.GetBits(0, DidSaveShift, 1);
    }

    // OFFSET 0x0202FF70
    public static void BattleFactorySave_RecordSave(BattleFactorySave save, int didSave)
    {
        save.SetBits(0, DidSaveShift, 1, didSave);
    }

    // OFFSET 0x0202FF84
    public static void BattleFactorySave_SetMember(BattleFactorySave save, byte field, byte index, uint value)
    {
        switch (field)
        {
            case FieldIsOpenLevel:
                save.SetBits(0, 0, 1, (byte)value);
                break;
            case FieldChallengeType:
                save.SetBits(0, 1, 7, (byte)value);
                break;
            case FieldCurrentBattleNum:
                save.W8(CurrentBattleOffset, (byte)value);
                break;
            case FieldTrainerIDs:
                save.W16(TrainerIDsOffset + index * 2, (ushort)value);
                break;
            case FieldPlayerPartySets:
                save.W16(PlayerPartySetsOffset + index * 2, (ushort)value);
                break;
            case FieldPlayerIVs:
                save.W8(PlayerIVsOffset + index, (byte)value);
                break;
            case FieldPlayerPersonality:
                save.W32(PlayerPersonalityOffset + index * 4, value);
                break;
            case FieldOpponentPartySets:
                save.W16(OpponentPartySetsOffset + index * 2, (ushort)value);
                break;
            case FieldOpponentIVs:
                save.W8(OpponentIVsOffset + index, (byte)value);
                break;
            case FieldOpponentPersonality:
                save.W32(OpponentPersonalityOffset + index * 4, value);
                break;
        }
    }

    // OFFSET 0x02030030
    public static uint BattleFactorySave_GetMember(BattleFactorySave save, byte field, byte index)
    {
        switch (field)
        {
            case FieldIsOpenLevel:
                return (uint)save.GetBits(0, 0, 1);
            case FieldChallengeType:
                return (uint)save.GetBits(0, 1, 7);
            case FieldCurrentBattleNum:
                return save.U8(CurrentBattleOffset);
            case FieldTrainerIDs:
                return save.U16(TrainerIDsOffset + index * 2);
            case FieldPlayerPartySets:
                return save.U16(PlayerPartySetsOffset + index * 2);
            case FieldPlayerIVs:
                return save.U8(PlayerIVsOffset + index);
            case FieldPlayerPersonality:
                return save.U32(PlayerPersonalityOffset + index * 4);
            case FieldOpponentPartySets:
                return save.U16(OpponentPartySetsOffset + index * 2);
            case FieldOpponentIVs:
                return save.U8(OpponentIVsOffset + index);
            case FieldOpponentPersonality:
                return save.U32(OpponentPersonalityOffset + index * 4);
        }
        return 0;
    }
}

public sealed class BattleFactoryStreakFlags : BufferView
{
    public const int Size = 4;
    public const int FieldStreakActive = 10;

    private const int FrontierOffset = 0x1618;

    public BattleFactoryStreakFlags(byte[] buffer, int offset)
        : base(buffer, offset)
    {
    }

    // OFFSET 0x020300A4
    public static void BattleFactoryStreakFlags_Init(BattleFactoryStreakFlags flags)
    {
        flags.Clear(0, Size);
    }

    // OFFSET 0x020300B0
    public static void BattleFactoryStreakFlags_SetFlag(BattleFactoryStreakFlags flags, byte field, byte challengeType, byte value)
    {
        if (field != FieldStreakActive)
        {
            return;
        }
        if (value >= 1)
        {
            flags.W8(0, flags.U8(0) | (1 << challengeType));
        }
        else
        {
            flags.W8(0, flags.U8(0) & (0xFF ^ (1 << challengeType)));
        }
    }

    // OFFSET 0x020300E0
    public static uint BattleFactoryStreakFlags_GetFlag(BattleFactoryStreakFlags flags, byte field, byte challengeType)
    {
        if (field == FieldStreakActive)
        {
            return (uint)((flags.U8(0) >> challengeType) & 1);
        }
        return 0;
    }

    // OFFSET 0x020300F4
    public static BattleFactoryStreakFlags BattleFactoryStreakFlags_Get(SaveData saveData)
    {
        BattleFrontierSave frontier = (BattleFrontierSave)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryFrontier);
        return new BattleFactoryStreakFlags(frontier.Raw, FrontierOffset);
    }
}

public sealed class BattleHallSave : BufferView
{
    public const int Size = 0x54;
    public const int FieldChallengeType = 0;
    public const int FieldCurrentBattleNum = 1;
    public const int FieldTrainerIDs = 2;
    public const int FieldPartySlots = 3;
    public const int FieldMonIndices = 4;
    public const int TypeCount = 18;

    private const int FrontierOffset = 0x8E0;
    private const int CurrentBattleOffset = 1;
    private const int PartySlotsOffset = 2;
    private const int TrainerIDsOffset = 4;
    private const int MonIndicesOffset = 0x2C;
    private const int DidSaveShift = 3;

    public BattleHallSave(byte[] buffer, int offset)
        : base(buffer, offset)
    {
    }

    // OFFSET 0x02030108
    public static void BattleHallSave_Init(BattleHallSave save)
    {
        save.Clear(0, Size);
    }

    // OFFSET 0x02030114
    public static BattleHallSave BattleHallSave_Get(SaveData saveData)
    {
        BattleFrontierSave frontier = (BattleFrontierSave)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryFrontier);
        return new BattleHallSave(frontier.Raw, FrontierOffset);
    }

    // OFFSET 0x02030124
    public static int BattleHallSave_HasSaved(BattleHallSave save)
    {
        return save.GetBits(0, DidSaveShift, 1);
    }

    // OFFSET 0x0203012C
    public static void BattleHallSave_RecordSave(BattleHallSave save, int didSave)
    {
        save.SetBits(0, DidSaveShift, 1, didSave);
    }

    // OFFSET 0x02030140
    public static void BattleHallSave_SetMember(BattleHallSave save, byte field, byte index, byte unused, uint value)
    {
        switch (field)
        {
            case FieldChallengeType:
                save.SetBits(0, 0, 7, (byte)value);
                break;
            case FieldCurrentBattleNum:
                save.W8(CurrentBattleOffset, (byte)value);
                break;
            case FieldTrainerIDs:
                save.W16(TrainerIDsOffset + index * 2, (ushort)value);
                break;
            case FieldPartySlots:
                save.W8(PartySlotsOffset + index, (byte)value);
                break;
            case FieldMonIndices:
                save.W16(MonIndicesOffset + index * 2, (ushort)value);
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
    }

    // OFFSET 0x02030198
    public static uint BattleHallSave_GetMember(BattleHallSave save, byte field, byte index, byte unused)
    {
        switch (field)
        {
            case FieldChallengeType:
                return (uint)save.GetBits(0, 0, 7);
            case FieldCurrentBattleNum:
                return save.U8(CurrentBattleOffset);
            case FieldTrainerIDs:
                return save.U16(TrainerIDsOffset + index * 2);
            case FieldPartySlots:
                return save.U8(PartySlotsOffset + index);
            case FieldMonIndices:
                return save.U16(MonIndicesOffset + index * 2);
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }

    // OFFSET 0x020301E0
    public static byte BattleHall_GetRankOfType(byte index, byte[] typeRanks)
    {
        return (byte)((typeRanks[index / 2] >> ((index % 2) * 4)) & 0xF);
    }

    // OFFSET 0x02030204
    public static void BattleHall_SetRankOfType(byte pokemonType, byte[] typeRanks, byte value)
    {
        int slot = pokemonType / 2;
        int half = pokemonType % 2;
        byte mask;
        if (half == 0)
        {
            mask = 0xF0;
        }
        else
        {
            mask = 0x0F;
        }
        typeRanks[slot] = (byte)(typeRanks[slot] & mask);
        typeRanks[slot] |= (byte)(value << (half * 4));
    }

    // OFFSET 0x02030244
    public static void BattleHall_ResetRankOfAllTypes(byte[] typeRanks)
    {
        for (int i = 0; i < TypeCount; i++)
        {
            BattleHall_SetRankOfType((byte)i, typeRanks, 0);
        }
    }
}

public sealed class BattleHallStreakFlags : BufferView
{
    public const int Size = 4;
    public const int FieldStreakActive = 5;

    private const int FrontierOffset = 0x161C;

    public BattleHallStreakFlags(byte[] buffer, int offset)
        : base(buffer, offset)
    {
    }

    // OFFSET 0x02030260
    public static void BattleHallStreakFlags_Init(BattleHallStreakFlags flags)
    {
        flags.Clear(0, Size);
    }

    // OFFSET 0x0203026C
    public static BattleHallStreakFlags BattleHallStreakFlags_Get(SaveData saveData)
    {
        BattleFrontierSave frontier = (BattleFrontierSave)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryFrontier);
        return new BattleHallStreakFlags(frontier.Raw, FrontierOffset);
    }

    // OFFSET 0x02030280
    public static void BattleHallStreakFlags_SetFlag(BattleHallStreakFlags flags, byte field, byte challengeType, byte unused, byte value)
    {
        if (field != FieldStreakActive)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        if (value >= 1)
        {
            flags.W8(0, flags.U8(0) | (1 << challengeType));
        }
        else
        {
            flags.W8(0, flags.U8(0) & (0xFF ^ (1 << challengeType)));
        }
    }

    // OFFSET 0x020302B4
    public static uint BattleHallStreakFlags_GetFlag(BattleHallStreakFlags flags, byte field, byte challengeType, byte unused)
    {
        if (field == FieldStreakActive)
        {
            return (uint)((flags.U8(0) >> challengeType) & 1);
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }
}

public sealed class BattleCastleSave : BufferView
{
    public const int Size = 0x74;
    public const int FieldChallengeType = 0;
    public const int FieldCurrentBattleNum = 1;
    public const int FieldMonHP = 2;
    public const int FieldMonPP = 3;
    public const int FieldMonStatus = 4;
    public const int FieldHeldItems = 5;
    public const int FieldTrainerIDs = 6;
    public const int FieldPartySlots = 7;
    public const int FieldMonSetIDs = 8;

    private const int FrontierOffset = 0x8E0;
    private const int CurrentBattleOffset = 1;
    private const int PartySlotsOffset = 3;
    private const int TrainerIDsOffset = 6;
    private const int MonHPOffset = 0x22;
    private const int MonPPOffset = 0x2A;
    private const int MonStatusOffset = 0x3C;
    private const int HeldItemsOffset = 0x4C;
    private const int MonSetIDsOffset = 0x54;
    private const int DidSaveShift = 3;

    public BattleCastleSave(byte[] buffer, int offset)
        : base(buffer, offset)
    {
    }

    // OFFSET 0x020302D0
    public static void BattleCastleSave_Init(BattleCastleSave save)
    {
        save.Clear(0, Size);
    }

    // OFFSET 0x020302DC
    public static BattleCastleSave BattleCastleSave_Get(SaveData saveData)
    {
        BattleFrontierSave frontier = (BattleFrontierSave)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryFrontier);
        return new BattleCastleSave(frontier.Raw, FrontierOffset);
    }

    // OFFSET 0x020302EC
    public static int BattleCastleSave_HasSaved(BattleCastleSave save)
    {
        return save.GetBits(0, DidSaveShift, 1);
    }

    // OFFSET 0x020302F4
    public static void BattleCastleSave_RecordSave(BattleCastleSave save, int didSave)
    {
        save.SetBits(0, DidSaveShift, 1, didSave);
    }

    // OFFSET 0x02030308
    public static void BattleCastleSave_SetMember(BattleCastleSave save, byte field, byte index, byte index2, uint value)
    {
        switch (field)
        {
            case FieldChallengeType:
                save.SetBits(0, 0, 7, (byte)value);
                break;
            case FieldCurrentBattleNum:
                save.W8(CurrentBattleOffset, (byte)value);
                break;
            case FieldMonHP:
                save.W16(MonHPOffset + index * 2, (ushort)value);
                break;
            case FieldMonPP:
                save.W8(MonPPOffset + index * 4 + index2, (byte)value);
                break;
            case FieldMonStatus:
                save.W32(MonStatusOffset + index * 4, value);
                break;
            case FieldHeldItems:
                save.W16(HeldItemsOffset + index * 2, (ushort)value);
                break;
            case FieldTrainerIDs:
                save.W16(TrainerIDsOffset + index * 2, (ushort)value);
                break;
            case FieldPartySlots:
                save.W8(PartySlotsOffset + index, (byte)value);
                break;
            case FieldMonSetIDs:
                save.W16(MonSetIDsOffset + index * 2, (ushort)value);
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
    }

    // OFFSET 0x02030398
    public static uint BattleCastleSave_GetMember(BattleCastleSave save, byte field, byte index, byte index2)
    {
        switch (field)
        {
            case FieldChallengeType:
                return (uint)save.GetBits(0, 0, 7);
            case FieldCurrentBattleNum:
                return save.U8(CurrentBattleOffset);
            case FieldMonHP:
                return save.U16(MonHPOffset + index * 2);
            case FieldMonPP:
                return save.U8(MonPPOffset + index * 4 + index2);
            case FieldMonStatus:
                return save.U32(MonStatusOffset + index * 4);
            case FieldHeldItems:
                return save.U16(HeldItemsOffset + index * 2);
            case FieldTrainerIDs:
                return save.U16(TrainerIDsOffset + index * 2);
            case FieldPartySlots:
                return save.U8(PartySlotsOffset + index);
            case FieldMonSetIDs:
                return save.U16(MonSetIDsOffset + index * 2);
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }
}

public sealed class BattleCastlePersistentSave : BufferView
{
    public const int Size = 4;
    public const int FieldStreakActive = 9;
    public const int FieldHeardIntro = 10;

    private const int FrontierOffset = 0x1620;
    private const int HeardIntroOffset = 1;

    public BattleCastlePersistentSave(byte[] buffer, int offset)
        : base(buffer, offset)
    {
    }

    // OFFSET 0x02030410
    public static void BattleCastlePersistentSave_Init(BattleCastlePersistentSave save)
    {
        save.Clear(0, Size);
    }

    // OFFSET 0x0203041C
    public static BattleCastlePersistentSave BattleCastlePersistentSave_Get(SaveData saveData)
    {
        BattleFrontierSave frontier = (BattleFrontierSave)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryFrontier);
        return new BattleCastlePersistentSave(frontier.Raw, FrontierOffset);
    }

    // OFFSET 0x02030430
    public static void BattleCastlePersistentSave_SetFlag(BattleCastlePersistentSave save, byte field, byte challengeType, byte unused, byte value)
    {
        switch (field)
        {
            case FieldStreakActive:
                if (value >= 1)
                {
                    save.W8(0, save.U8(0) | (1 << challengeType));
                }
                else
                {
                    save.W8(0, save.U8(0) & (0xFF ^ (1 << challengeType)));
                }
                break;
            case FieldHeardIntro:
                save.W8(HeardIntroOffset, 1);
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
    }

    // OFFSET 0x02030470
    public static uint BattleCastlePersistentSave_GetFlag(BattleCastlePersistentSave save, byte field, byte challengeType, byte unused)
    {
        switch (field)
        {
            case FieldStreakActive:
                return (uint)((save.U8(0) >> challengeType) & 1);
            case FieldHeardIntro:
                return save.U8(HeardIntroOffset);
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }
}

public sealed class BattleArcadeSave : BufferView
{
    public const int Size = 0x48;
    public const int FieldChallengeType = 0;
    public const int FieldCursorRandomized = 1;
    public const int FieldCurrentBattle = 2;
    public const int FieldRouletteSpeed = 3;
    public const int FieldHeldItems = 4;
    public const int FieldTrainerIDs = 5;
    public const int FieldPartySlots = 6;
    public const int FieldMonSetIDs = 7;

    private const int FrontierOffset = 0x8E0;
    private const int CurrentBattleOffset = 1;
    private const int RouletteSpeedOffset = 2;
    private const int PartySlotsOffset = 3;
    private const int HeldItemsOffset = 6;
    private const int TrainerIDsOffset = 0xE;
    private const int MonSetIDsOffset = 0x2A;
    private const int DidSaveShift = 3;
    private const int CursorRandomizedShift = 4;

    public BattleArcadeSave(byte[] buffer, int offset)
        : base(buffer, offset)
    {
    }

    // OFFSET 0x02030494
    public static void BattleArcadeSave_Init(BattleArcadeSave save)
    {
        save.Clear(0, Size);
    }

    // OFFSET 0x020304A0
    public static BattleArcadeSave BattleArcadeSave_Get(SaveData saveData)
    {
        BattleFrontierSave frontier = (BattleFrontierSave)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryFrontier);
        return new BattleArcadeSave(frontier.Raw, FrontierOffset);
    }

    // OFFSET 0x020304B0
    public static int BattleArcadeSave_HasSaved(BattleArcadeSave save)
    {
        return save.GetBits(0, DidSaveShift, 1);
    }

    // OFFSET 0x020304B8
    public static void BattleArcadeSave_RecordSave(BattleArcadeSave save, int didSave)
    {
        save.SetBits(0, DidSaveShift, 1, didSave);
    }

    // OFFSET 0x020304CC
    public static void BattleArcadeSave_SetMember(BattleArcadeSave save, byte field, byte index, byte unused, uint value)
    {
        switch (field)
        {
            case FieldChallengeType:
                save.SetBits(0, 0, 7, (byte)value);
                break;
            case FieldCursorRandomized:
                save.SetBits(0, CursorRandomizedShift, 1, (byte)value);
                break;
            case FieldCurrentBattle:
                save.W8(CurrentBattleOffset, (byte)value);
                break;
            case FieldRouletteSpeed:
                save.W8(RouletteSpeedOffset, (byte)value);
                break;
            case FieldHeldItems:
                save.W16(HeldItemsOffset + index * 2, (ushort)value);
                break;
            case FieldTrainerIDs:
                save.W16(TrainerIDsOffset + index * 2, (ushort)value);
                break;
            case FieldPartySlots:
                save.W8(PartySlotsOffset + index, (byte)value);
                break;
            case FieldMonSetIDs:
                save.W16(MonSetIDsOffset + index * 2, (ushort)value);
                break;
            default:
                ErrorHandling.GF_AssertFail();
                break;
        }
    }

    // OFFSET 0x0203054C
    public static uint BattleArcadeSave_GetMember(BattleArcadeSave save, byte field, byte index, byte unused)
    {
        switch (field)
        {
            case FieldChallengeType:
                return (uint)save.GetBits(0, 0, 7);
            case FieldCursorRandomized:
                return (uint)save.GetBits(0, CursorRandomizedShift, 1);
            case FieldCurrentBattle:
                return save.U8(CurrentBattleOffset);
            case FieldRouletteSpeed:
                return save.U8(RouletteSpeedOffset);
            case FieldHeldItems:
                return save.U16(HeldItemsOffset + index * 2);
            case FieldTrainerIDs:
                return save.U16(TrainerIDsOffset + index * 2);
            case FieldPartySlots:
                return save.U8(PartySlotsOffset + index);
            case FieldMonSetIDs:
                return save.U16(MonSetIDsOffset + index * 2);
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }
}

public sealed class BattleArcadeStreakFlags : BufferView
{
    public const int Size = 4;
    public const int FieldStreakActive = 8;

    private const int FrontierOffset = 0x1624;

    public BattleArcadeStreakFlags(byte[] buffer, int offset)
        : base(buffer, offset)
    {
    }

    // OFFSET 0x020305AC
    public static void BattleArcadeStreakFlags_Init(BattleArcadeStreakFlags flags)
    {
        flags.Clear(0, Size);
    }

    // OFFSET 0x020305B8
    public static BattleArcadeStreakFlags BattleArcadeStreakFlags_Get(SaveData saveData)
    {
        BattleFrontierSave frontier = (BattleFrontierSave)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryFrontier);
        return new BattleArcadeStreakFlags(frontier.Raw, FrontierOffset);
    }

    // OFFSET 0x020305CC
    public static void BattleArcadeStreakFlags_SetFlag(BattleArcadeStreakFlags flags, byte field, byte challengeType, byte unused, byte value)
    {
        if (field != FieldStreakActive)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        if (value >= 1)
        {
            flags.W8(0, flags.U8(0) | (1 << challengeType));
        }
        else
        {
            flags.W8(0, flags.U8(0) & (0xFF ^ (1 << challengeType)));
        }
    }

    // OFFSET 0x02030600
    public static uint BattleArcadeStreakFlags_GetFlag(BattleArcadeStreakFlags flags, byte field, byte challengeType, byte unused)
    {
        if (field == FieldStreakActive)
        {
            return (uint)((flags.U8(0) >> challengeType) & 1);
        }
        ErrorHandling.GF_AssertFail();
        return 0;
    }
}
