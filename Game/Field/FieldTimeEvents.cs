using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class FieldTimeEvents
{
    private const int GroupSegmentDataSize = 0x28;
    private const int GroupNameLength = 0xB;
    private const int TVProgramTypeSightings = 2;
    private const int TVSegmentGroupCreated = 0x14;
    private const int TVSegmentGroupJoined = 4;
    private const int TVSegmentFoundHiddenItem = 6;
    private const int BadgeCount = 8;
    private const int BadgePolishMax = 200;
    private const int BadgePolishDecayPerDay = 10;

    private const int ParamSpecies = 5;
    private const int ParamPokerus = 0x9A;
    private const int PokerusDaysMask = 0xF;
    private const int PokerusStrainMask = 0xF0;
    private const int PokerusCured = 0x10;
    private const int PokerusMaxDays = 4;

    private const int ShayminNightStartHour = 20;
    private const int ShayminNightEndHour = 4;
    private const int HoursPerDay = 24;
    private const int MinutesPerHour = 60;

    private const int SaveTableTVBroadcast = 0x1B;
    private const int TVProgramTypeRecords = 3;
    private const int TVSegmentCaptureCount = 10;
    private const int TVSegmentDailyCount = 11;
    private const int TVSegmentDataSize = 0x28;
    private const int TVProgramTypeCatchThatPokemon = 2;
    private const int TVSegmentCaught = 1;
    private const int TVSegmentFailed = 2;
    private const int CatchShowBallsOffset = 4;
    private const ushort CatchShowMaxBalls = 999;
    private const int BattleResultWin = 1;
    private const int BattleResultCaught = 4;
    private const int BattleResultPlayerFled = 5;
    private const int BattleResultEnemyFled = 6;
    private const int CaptureCountThreshold = 30;
    private const int DailyCountThreshold = 10;

    // OFFSET 0x0206C2D0
    public static void BadgePolish_DecayByDays(SaveData saveData, int daysPassed)
    {
        if (daysPassed <= 0)
        {
            return;
        }
        uint[] badges = TrainerCaseSaveData.TrainerCaseSaveData_GetBadges(TrainerCaseSaveData.SaveData_GetTrainerCaseSaveData(saveData));
        TrainerInfo info = PlayerSave.SaveData_GetTrainerInfo(saveData);
        int decay = daysPassed * BadgePolishDecayPerDay;
        for (byte i = 0; i < BadgeCount; i++)
        {
            if (!TrainerInfo.TrainerInfo_HasBadge(info, i))
            {
                continue;
            }
            int polish = (int)TrainerCaseSaveData.TrainerCaseSaveDataBadge_GetPolish(i, badges);
            if (polish > 0 && polish < BadgePolishMax)
            {
                polish -= decay;
            }
            else
            {
                if (polish >= BadgePolishMax)
                {
                    ErrorHandling.GF_AssertFail();
                }
                polish = 0;
            }
            if (polish < 0)
            {
                polish = 0;
            }
            TrainerCaseSaveData.TrainerCaseSaveDataBadge_SetPolish(i, (uint)polish, badges);
        }
    }

    // OFFSET 0x020777B4
    public static void Party_UpdatePokerusStatus(Party party, int daysPassed)
    {
        int count = Party.Party_GetCurrentCount(party);
        for (int i = 0; i < count; i++)
        {
            Pokemon mon = Party.Party_GetPokemonBySlotIndex(party, i);
            if (Pokemon.Pokemon_GetValue(mon, ParamSpecies, null) == 0)
            {
                continue;
            }
            byte pokerus = (byte)Pokemon.Pokemon_GetValue(mon, ParamPokerus, null);
            if ((pokerus & PokerusDaysMask) == 0)
            {
                continue;
            }
            if ((pokerus & PokerusDaysMask) < daysPassed || daysPassed > PokerusMaxDays)
            {
                pokerus = (byte)(pokerus & PokerusStrainMask);
            }
            else
            {
                pokerus = (byte)(pokerus - daysPassed);
            }
            if (pokerus == 0)
            {
                pokerus = PokerusCured;
            }
            Pokemon.Pokemon_SetValue(mon, ParamPokerus, pokerus);
        }
    }

    // OFFSET 0x02077BD8
    public static int Party_UpdateShayminFormForTime(Party party, int minutesPassed, ref RtcTime currentTime)
    {
        uint hour = (uint)currentTime.Hour;
        if (hour >= ShayminNightStartHour || hour < ShayminNightEndHour)
        {
            int nightHour = currentTime.Hour;
            if (nightHour < ShayminNightEndHour)
            {
                nightHour += HoursPerDay;
            }
            nightHour -= ShayminNightStartHour;
            if (currentTime.Minute + nightHour * MinutesPerHour < minutesPassed + 1)
            {
                PartyForms.Party_ResetShayminForms(party);
                return 1;
            }
            return 0;
        }
        int dayHour = currentTime.Hour - ShayminNightEndHour;
        if (currentTime.Minute + dayHour * MinutesPerHour < minutesPassed)
        {
            PartyForms.Party_ResetShayminForms(party);
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0206CD70
    private static void FieldSystem_SaveTVSegment(FieldSystem fieldSystem, int programType, int segmentID, byte[] data)
    {
        TVSegment_Save(fieldSystem.SaveData!, programType, segmentID, data);
    }

    // OFFSET 0x0206D3E4
    private static void FieldSystem_SaveTVSegment_GroupEvent(FieldSystem fieldSystem, int segmentID)
    {
        RecordMixedRNG rng = RecordMixedRNG.SaveData_GetRecordMixedRNG(fieldSystem.SaveData!);
        byte[] segment = new byte[GroupSegmentDataSize];
        ushort[] groupName = new ushort[GroupNameLength];
        ushort[] entryNames = new ushort[RecordMixedRNGEntry.NameLength * 2];
        Array.Copy(RecordMixedRNG.RecordMixedRNG_GetEntryName(rng, 1, RecordMixedRNG.NameGroup), entryNames, RecordMixedRNGEntry.NameLength);
        Array.Copy(RecordMixedRNG.RecordMixedRNG_GetEntryName(rng, 1, RecordMixedRNG.NamePlayer), 0, entryNames, RecordMixedRNGEntry.NameLength, RecordMixedRNGEntry.NameLength);
        CharCode.CharCode_CopyNumChars(groupName, 0, entryNames, 0, GroupNameLength);
        for (int i = 0; i < GroupNameLength; i++)
        {
            Nitro.Bin.W16(segment, i * 2, groupName[i]);
        }
        FieldSystem_SaveTVSegment(fieldSystem, TVProgramTypeSightings, segmentID, segment);
    }

    // OFFSET 0x0206D424
    public static void FieldSystem_SaveTVSegment_GroupCreated(FieldSystem fieldSystem)
    {
        FieldSystem_SaveTVSegment_GroupEvent(fieldSystem, TVSegmentGroupCreated);
    }

    // OFFSET 0x0206D430
    public static void FieldSystem_SaveTVSegment_GroupJoined(FieldSystem fieldSystem)
    {
        FieldSystem_SaveTVSegment_GroupEvent(fieldSystem, TVSegmentGroupJoined);
    }

    // OFFSET 0x0206D4AC
    public static void FieldSystem_SaveTVSegment_FoundHiddenItem(FieldSystem fieldSystem, ushort item)
    {
        byte[] segment = new byte[TVSegmentDataSize];
        Nitro.Bin.W16(segment, 0, item);
        Nitro.Bin.W16(segment, 2, (ushort)MapHeader.MapHeader_GetMapLabelTextID((int)fieldSystem.Location!.U32(0)));
        FieldSystem_SaveTVSegment(fieldSystem, TVProgramTypeSightings, TVSegmentFoundHiddenItem, segment);
    }

    // OFFSET 0x0206CD7C
    private static void TVSegment_Save(SaveData saveData, int programType, int segmentID, byte[] data)
    {
        TVBroadcast broadcast = (TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(saveData);
        TVBroadcast.TVBroadcast_SaveSegmentData(broadcast, programType, segmentID, data);
    }

    // OFFSET 0x0206D1B8
    public static void FieldSystem_SaveTVSegment_CatchThatPokemonShow(FieldSystem fieldSystem, CaptureAttempt? captureAttempt, int resultMask)
    {
        byte[] segment = new byte[TVSegmentDataSize];
        Array.Copy(captureAttempt!.Data, segment, CaptureAttempt.Size);
        Bin.W32(segment, 0, (uint)resultMask);
        ushort ballsThrown = (ushort)Bin.U16(segment, CatchShowBallsOffset);
        if (ballsThrown == 0)
        {
            return;
        }
        if (ballsThrown > CatchShowMaxBalls)
        {
            Bin.W16(segment, CatchShowBallsOffset, CatchShowMaxBalls);
            ballsThrown = CatchShowMaxBalls;
        }
        switch (resultMask)
        {
            case BattleResultCaught:
                FieldSystem_SaveTVSegment(fieldSystem, TVProgramTypeCatchThatPokemon, TVSegmentCaught, segment);
                break;
            case BattleResultWin:
            case BattleResultPlayerFled:
            case BattleResultEnemyFled:
                if (ballsThrown > 2)
                {
                    FieldSystem_SaveTVSegment(fieldSystem, TVProgramTypeCatchThatPokemon, TVSegmentFailed, segment);
                }
                break;
        }
    }

    // OFFSET 0x0206D0F0
    public static void TVBroadcast_ResetCaptureCount(TVBroadcast broadcast)
    {
        byte[] section = TVBroadcast.TVBroadcast_GetSection0BBC(broadcast);
        Bin.W32(section, 4, 0);
        SaveData.SaveData_SetChecksum(SaveTableTVBroadcast);
    }

    // OFFSET 0x0206D12C
    public static void TVBroadcast_ResetDailyCount(TVBroadcast broadcast)
    {
        byte[] section = TVBroadcast.TVBroadcast_GetSection0BC4(broadcast);
        Bin.W16(section, 2, 0);
        SaveData.SaveData_SetChecksum(SaveTableTVBroadcast);
    }

    // OFFSET 0x0206DFE0
    public static void TVSegment_SaveCaptureCountSegment(SaveData saveData)
    {
        byte[] data = new byte[TVSegmentDataSize];
        byte[] section = TVBroadcast.TVBroadcast_GetSection0BBC((TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(saveData));
        uint count = Bin.U32(section, 4);
        if (count >= CaptureCountThreshold)
        {
            Bin.W32(data, 0, Bin.U32(section, 0));
            Bin.W32(data, 4, count);
            section[0] = 0;
            SaveData.SaveData_SetChecksum(SaveTableTVBroadcast);
            TVSegment_Save(saveData, TVProgramTypeRecords, TVSegmentCaptureCount, data);
        }
    }

    // OFFSET 0x0206E060
    public static void TVSegment_SaveDailyCountSegment(SaveData saveData)
    {
        byte[] data = new byte[TVSegmentDataSize];
        byte[] section = TVBroadcast.TVBroadcast_GetSection0BC4((TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(saveData));
        if (Bin.U16(section, 2) >= DailyCountThreshold)
        {
            Bin.W16(data, 0, (ushort)Bin.U16(section, 0));
            Bin.W16(data, 2, (ushort)Bin.U16(section, 2));
            section[0] = 0;
            SaveData.SaveData_SetChecksum(SaveTableTVBroadcast);
            TVSegment_Save(saveData, TVProgramTypeRecords, TVSegmentDailyCount, data);
        }
    }

    // OFFSET 0x0206F2F0
    public static void TVBroadcast_HandleDailyEvents(SaveData saveData)
    {
        TVBroadcast broadcast = (TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(saveData);
        TVSegment_SaveCaptureCountSegment(saveData);
        TVSegment_SaveDailyCountSegment(saveData);
        TVBroadcast_ResetCaptureCount(broadcast);
        TVBroadcast_ResetDailyCount(broadcast);
    }
}
