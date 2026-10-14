using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class JournalLocationEvent
{
    public byte Type;
    public ushort Location;
    public ushort Trainer;
    public ushort Item;
}

public sealed class JournalMonEvent
{
    public byte Type;
    public byte Stats;
    public ushort Species;
}

public sealed class JournalTrainerEvent
{
    public ushort Packed;
    public ushort Location;
}

public sealed class JournalOnlineEvent
{
    public const int NameLength = 8;
    public const int TitleLength = 12;

    public byte Type;
    public byte Stats;
    public readonly ushort[] PlayerName1 = new ushort[NameLength];
    public readonly ushort[] PlayerName2 = new ushort[NameLength];
    public readonly ushort[] Title = new ushort[TitleLength];

    public void Clear()
    {
        Type = 0;
        Stats = 0;
        Array.Clear(PlayerName1);
        Array.Clear(PlayerName2);
        Array.Clear(Title);
    }
}

public sealed class JournalOnlineSlot
{
    public const int Size = 0x2A;
    public const int NameLength = 8;
    public const int ExtraLength = 12;

    private const int NameOffset = 2;
    private const int ExtraOffset = 0x12;

    public byte Type;
    public byte Stats;
    public readonly ushort[] Name = new ushort[NameLength];
    public readonly ushort[] Extra = new ushort[ExtraLength];

    public void WriteTo(byte[] body, int offset)
    {
        body[offset] = Type;
        body[offset + 1] = Stats;
        for (int i = 0; i < NameLength; i++)
        {
            Bin.W16(body, offset + NameOffset + i * 2, Name[i]);
        }
        for (int i = 0; i < ExtraLength; i++)
        {
            Bin.W16(body, offset + ExtraOffset + i * 2, Extra[i]);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Type = body[offset];
        Stats = body[offset + 1];
        for (int i = 0; i < NameLength; i++)
        {
            Name[i] = (ushort)Bin.U16(body, offset + NameOffset + i * 2);
        }
        for (int i = 0; i < ExtraLength; i++)
        {
            Extra[i] = (ushort)Bin.U16(body, offset + ExtraOffset + i * 2);
        }
    }

    public void CopyFrom(JournalOnlineSlot source)
    {
        Type = source.Type;
        Stats = source.Stats;
        Array.Copy(source.Name, Name, NameLength);
        Array.Copy(source.Extra, Extra, ExtraLength);
    }

    public void Clear()
    {
        Type = 0;
        Stats = 0;
        Array.Clear(Name);
        Array.Clear(Extra);
    }
}

public sealed class JournalEntry
{
    public const int Size = 0x70;
    public const int LocationEventCount = 4;
    public const int OnlineEventCount = 2;

    private const int LocationsOffset = 4;
    private const int MonOffset = 0x14;
    private const int TrainerOffset = 0x18;
    private const int OnlineOffset = 0x1C;

    public uint Date;
    public readonly uint[] Locations = new uint[LocationEventCount];
    public readonly ushort[] Mon = new ushort[2];
    public readonly ushort[] Trainer = new ushort[2];
    public readonly JournalOnlineSlot[] Online = { new JournalOnlineSlot(), new JournalOnlineSlot() };

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, Date);
        for (int i = 0; i < LocationEventCount; i++)
        {
            Bin.W32(body, offset + LocationsOffset + i * 4, Locations[i]);
        }
        Bin.W16(body, offset + MonOffset, Mon[0]);
        Bin.W16(body, offset + MonOffset + 2, Mon[1]);
        Bin.W16(body, offset + TrainerOffset, Trainer[0]);
        Bin.W16(body, offset + TrainerOffset + 2, Trainer[1]);
        for (int i = 0; i < OnlineEventCount; i++)
        {
            Online[i].WriteTo(body, offset + OnlineOffset + i * JournalOnlineSlot.Size);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Date = Bin.U32(body, offset);
        for (int i = 0; i < LocationEventCount; i++)
        {
            Locations[i] = Bin.U32(body, offset + LocationsOffset + i * 4);
        }
        Mon[0] = (ushort)Bin.U16(body, offset + MonOffset);
        Mon[1] = (ushort)Bin.U16(body, offset + MonOffset + 2);
        Trainer[0] = (ushort)Bin.U16(body, offset + TrainerOffset);
        Trainer[1] = (ushort)Bin.U16(body, offset + TrainerOffset + 2);
        for (int i = 0; i < OnlineEventCount; i++)
        {
            Online[i].ReadFrom(body, offset + OnlineOffset + i * JournalOnlineSlot.Size);
        }
    }

    public void CopyFrom(JournalEntry source)
    {
        byte[] buffer = new byte[Size];
        source.WriteTo(buffer, 0);
        ReadFrom(buffer, 0);
    }
}

public sealed class Journal : ISaveBlock
{
    public const int EntryCount = 10;
    public const int DataDate = 0;
    public const int DataLocation = 1;
    public const int DataMon = 2;
    public const int DataTrainer = 3;
    public const int DataOnline = 4;
    public const int LocationNone = 0;
    public const int LocationRestedAtHome = 1;
    public const int LocationLeftResearchLab = 2;
    public const int LocationUsedPC = 3;
    public const int LocationShopped = 4;
    public const int LocationLotsOfShopping = 5;
    public const int LocationSoldALittle = 6;
    public const int LocationSoldALot = 7;
    public const int LocationBusinessAtMart = 8;
    public const int LocationGymWasTooTough = 9;
    public const int LocationBeatGymLeader = 10;
    public const int LocationBeatEliteFour = 11;
    public const int LocationBeatChampion = 12;
    public const int LocationArrivedInLocation = 13;
    public const int LocationLeftCave = 14;
    public const int LocationLeftBuilding = 15;
    public const int LocationGameCorner = 16;
    public const int LocationSafariGame = 17;
    public const int LocationItemWasObtained = 18;
    public const int LocationFirstFieldMove = 19;
    public const int LocationLastFieldMove = 30;
    public const int LocationUnused31 = 31;
    public const int LocationDugUnderground = 34;
    public const int LocationBuiltSecretBase = 35;
    public const int OnlineNone = 0;
    public const int MonCaught = 1;
    public const int MonDefeated = 2;
    public const int TrainerUnknown = 10;
    public const int TrainerEliteFour = 8;
    public const int TrainerChampion = 9;
    public const int NoGym = 0xFF;

    private const uint EventTypeMask = 0x3F;
    private const int EventParamShift = 16;
    private const int EventTrainerShift = 6;
    private const uint EventTrainerMask = 0x3FF;
    private const int LastLocationEvent = 40;
    private const int LastOnlineEvent = 29;
    private const uint DateYearMask = 0x7F;
    private const int DateMonthShift = 7;
    private const uint DateMonthMask = 0xF;
    private const int DateWeekShift = 11;
    private const uint DateWeekMask = 7;
    private const int DateDayShift = 14;
    private const uint DateDayMask = 0x1F;
    private const int DateLocationShift = 19;
    private const uint DateFieldsMask = 0x7FFFF;
    private const ushort EndOfString = 0xFFFF;
    private const int GymCount = 8;
    private const int GymEntrySize = 8;
    private const uint GymTable = 0x020E5B50;
    private const int BuildingCount = 27;
    private const int BuildingEntrySize = 8;
    private const uint BuildingTable = 0x020E5B90;
    private const int MapPlayerHouse = 0x19E;
    private const int MapResearchLab = 0x1A6;
    private const int FirstEliteFourTrainer = 0x105;
    private const int FirstEliteFourRematchTrainer = 0x362;
    private const int EliteFourCount = 4;
    private const int ChampionTrainer = 0x10B;
    private const int ChampionRematchTrainer = 0x366;
    private const int MinutesPerSlice = 10;

    public readonly JournalEntry[] Entries = new JournalEntry[EntryCount];

    private static byte[]? sGyms;
    private static byte[]? sBuildings;

    public Journal()
    {
        for (int i = 0; i < EntryCount; i++)
        {
            Entries[i] = new JournalEntry();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < EntryCount; i++)
        {
            Entries[i].WriteTo(body, offset + i * JournalEntry.Size);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < EntryCount; i++)
        {
            Entries[i].ReadFrom(body, offset + i * JournalEntry.Size);
        }
    }

    private static byte[] Gyms => sGyms ??= RomData.Arm9(GymTable, GymCount * GymEntrySize);

    private static byte[] Buildings => sBuildings ??= RomData.Arm9(BuildingTable, BuildingCount * BuildingEntrySize);

    private static int DateYear(uint date)
    {
        return (int)(date & DateYearMask);
    }

    private static int DateMonth(uint date)
    {
        return (int)((date >> DateMonthShift) & DateMonthMask);
    }

    private static int DateDay(uint date)
    {
        return (int)((date >> DateDayShift) & DateDayMask);
    }

    private static int DateWeek(uint date)
    {
        return (int)((date >> DateWeekShift) & DateWeekMask);
    }

    private static int CountLocationEvents(uint[] events)
    {
        int count;
        for (count = 0; count < JournalEntry.LocationEventCount; count++)
        {
            if (events[count] == 0)
            {
                break;
            }
        }
        return count;
    }

    private static uint PackLocationEvent(JournalLocationEvent locationEvent)
    {
        return (locationEvent.Type & EventTypeMask) | ((uint)locationEvent.Location << EventParamShift) | ((locationEvent.Trainer & EventTrainerMask) << EventTrainerShift);
    }

    private static void RemoveLocationEvent(uint[] events, int index)
    {
        events[index] = 0;
        for (int i = index; i < JournalEntry.LocationEventCount - 1; i++)
        {
            events[i] = events[i + 1];
            events[i + 1] = 0;
        }
    }

    // OFFSET 0x0202B60C
    public static void JournalEntry_Init(JournalEntry entry)
    {
        entry.ReadFrom(new byte[JournalEntry.Size], 0);
    }

    // OFFSET 0x0202B618
    public static void Journal_Init(Journal journal)
    {
        for (int i = 0; i < EntryCount; i++)
        {
            JournalEntry_Init(journal.Entries[i]);
        }
    }

    // OFFSET 0x0202B628
    public static Journal SaveData_GetJournal(SaveData saveData)
    {
        return (Journal)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryJournal);
    }

    // OFFSET 0x0202B634
    public static Journal? Journal_GetSavedPage(Journal journal, int journalAcquired)
    {
        if (journalAcquired != 1)
        {
            return null;
        }
        Rtc.GetCurrentDate(out RtcDate date);
        uint stored = journal.Entries[0].Date;
        if (DateMonth(stored) != 0)
        {
            if (DateYear(stored) != date.Year || DateMonth(stored) != date.Month || DateDay(stored) != date.Day || DateWeek(stored) != date.Week)
            {
                for (int i = EntryCount - 1; i >= 1; i--)
                {
                    journal.Entries[i].CopyFrom(journal.Entries[i - 1]);
                }
                JournalEntry_Init(journal.Entries[0]);
            }
        }
        return journal;
    }

    // OFFSET 0x0202B6A4
    public static bool Journal_CheckOpenOnContinue(Journal journal, int journalAcquired)
    {
        if (journalAcquired != 1)
        {
            return false;
        }
        Rtc.GetCurrentDate(out RtcDate now);
        uint stored = journal.Entries[0].Date;
        RtcDate last = new()
        {
            Year = DateYear(stored),
            Month = DateMonth(stored),
            Day = DateDay(stored),
            Week = DateWeek(stored),
        };
        int difference = Rtc.DayNumberForDate(now) - Rtc.DayNumberForDate(last);
        if ((now.Month == 12 && now.Day == 31 && last.Month == 1 && last.Day == 1)
            || (now.Month == 1 && now.Day == 1 && last.Month == 12 && last.Day == 31))
        {
            int years = now.Year - last.Year;
            if (years >= 2 || years <= -2)
            {
                return true;
            }
            return false;
        }
        if (difference <= -2 || difference >= 2)
        {
            return true;
        }
        if (now.Year != last.Year)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0202B758
    public static void JournalEntry_SaveData(Journal? journal, object data, int dataType)
    {
        if (journal == null)
        {
            return;
        }
        JournalEntry entry = journal.Entries[0];
        switch (dataType)
        {
            case DataDate:
                JournalEntry_SaveDate(entry, (uint)data);
                break;
            case DataLocation:
                JournalEntry_SaveLocationEvent(entry, (JournalLocationEvent)data);
                break;
            case DataMon:
                JournalEntry_SaveMonEvent(entry, (JournalMonEvent)data);
                break;
            case DataTrainer:
                JournalEntry_SaveTrainerEvent(entry, (JournalTrainerEvent)data);
                break;
            case DataOnline:
                JournalEntry_SaveOnlineEvent(entry, (JournalOnlineEvent)data);
                break;
        }
    }

    // OFFSET 0x0202B7A0
    private static void JournalEntry_SaveDate(JournalEntry entry, uint date)
    {
        if (DateYear(entry.Date) != DateYear(date) || DateMonth(entry.Date) != DateMonth(date) || DateDay(entry.Date) != DateDay(date) || DateWeek(entry.Date) != DateWeek(date))
        {
            entry.Date = date;
        }
    }

    // OFFSET 0x0202B7E0
    private static void JournalEntry_SaveLocationEvent(JournalEntry entry, JournalLocationEvent locationEvent)
    {
        int type = locationEvent.Type;
        if (type > LastLocationEvent || type == LocationNone || type == LocationUnused31)
        {
            return;
        }
        if (type == LocationGymWasTooTough || type == LocationBeatGymLeader)
        {
            JournalEntry_SaveGymEvent(entry.Locations, locationEvent);
        }
        else if (type == LocationBeatEliteFour)
        {
            JournalEntry_SaveEliteFourEvent(entry.Locations, locationEvent);
        }
        else if (type == LocationBeatChampion)
        {
            JournalEntry_SaveChampionEvent(entry.Locations, locationEvent);
        }
        else if (type >= LocationArrivedInLocation && type <= LocationLeftBuilding)
        {
            JournalEntry_SaveArrivalEvent(entry.Locations, locationEvent);
        }
        else if (type == LocationItemWasObtained)
        {
            JournalEntry_SaveItemEvent(entry.Locations, locationEvent);
        }
        else if (type >= LocationFirstFieldMove && type < LocationDugUnderground)
        {
            JournalEntry_SaveFieldMoveEvent(entry.Locations, locationEvent);
        }
        else
        {
            JournalEntry_SaveSimpleEvent(entry.Locations, locationEvent);
        }
    }

    // OFFSET 0x0202B880
    private static void JournalEntry_SaveMonEvent(JournalEntry entry, JournalMonEvent monEvent)
    {
        entry.Mon[0] = (ushort)(monEvent.Type | (monEvent.Stats << 8));
        entry.Mon[1] = monEvent.Species;
    }

    // OFFSET 0x0202B88C
    private static void JournalEntry_SaveTrainerEvent(JournalEntry entry, JournalTrainerEvent trainerEvent)
    {
        entry.Trainer[0] = trainerEvent.Packed;
        entry.Trainer[1] = trainerEvent.Location;
    }

    // OFFSET 0x0202B898
    private static void JournalEntry_SaveOnlineEvent(JournalEntry entry, JournalOnlineEvent onlineEvent)
    {
        JournalOnlineSlot slot = JournalEntry_GetOpenOnlineSlot(entry);
        switch (onlineEvent.Type)
        {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 11:
                JournalEntry_SaveOnlineBattle(slot, onlineEvent);
                break;
            case 6:
            case 22:
            case 23:
                JournalEntry_SaveOnlinePlayer(slot, onlineEvent);
                break;
            case 7:
            case 9:
            case 15:
                JournalEntry_SaveOnlineTrade(slot, onlineEvent);
                break;
            case 13:
            case 24:
                JournalEntry_SaveOnlineResult(slot, onlineEvent);
                break;
            case 8:
            case 10:
            case 12:
            case 14:
            case 16:
            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 25:
            case 26:
            case 27:
            case 28:
            case 29:
                JournalEntry_SaveOnlineSimple(entry, onlineEvent);
                break;
        }
    }

    // OFFSET 0x0202B91C
    private static int JournalEntry_GetOpenLocationSlot(uint[] events)
    {
        for (int i = 0; i < JournalEntry.LocationEventCount; i++)
        {
            if ((events[i] & EventTypeMask) == 0)
            {
                return i;
            }
        }
        int last = JournalEntry.LocationEventCount - 1;
        for (int i = 0; i < last; i++)
        {
            events[i] = events[i + 1];
        }
        events[last] = 0;
        return last;
    }

    // OFFSET 0x0202B954
    private static JournalOnlineSlot JournalEntry_GetOpenOnlineSlot(JournalEntry entry)
    {
        for (int i = 0; i < JournalEntry.OnlineEventCount; i++)
        {
            if (entry.Online[i].Type == 0)
            {
                return entry.Online[i];
            }
        }
        entry.Online[0].CopyFrom(entry.Online[1]);
        entry.Online[1].Clear();
        return entry.Online[1];
    }

    // OFFSET 0x0202B994
    private static void JournalEntry_SaveSimpleEvent(uint[] events, JournalLocationEvent locationEvent)
    {
        int count = CountLocationEvents(events);
        if (count != 0 && locationEvent.Type == (events[count - 1] & EventTypeMask))
        {
            return;
        }
        events[JournalEntry_GetOpenLocationSlot(events)] = locationEvent.Type & EventTypeMask;
    }

    // OFFSET 0x0202B9D0
    private static void JournalEntry_SaveChampionEvent(uint[] events, JournalLocationEvent locationEvent)
    {
        events[JournalEntry_GetOpenLocationSlot(events)] = PackLocationEvent(locationEvent);
    }

    // OFFSET 0x0202B9F0
    private static void JournalEntry_SaveEliteFourEvent(uint[] events, JournalLocationEvent locationEvent)
    {
        for (int i = 0; i < JournalEntry.LocationEventCount; i++)
        {
            if ((events[i] & EventTypeMask) == LocationBeatEliteFour)
            {
                RemoveLocationEvent(events, i);
                break;
            }
        }
        events[JournalEntry_GetOpenLocationSlot(events)] = PackLocationEvent(locationEvent);
    }

    // OFFSET 0x0202BA48
    private static void JournalEntry_SaveGymEvent(uint[] events, JournalLocationEvent locationEvent)
    {
        for (int i = 0; i < JournalEntry.LocationEventCount; i++)
        {
            if ((events[i] & EventTypeMask) == LocationGymWasTooTough && (events[i] >> EventParamShift) == locationEvent.Location)
            {
                RemoveLocationEvent(events, i);
                break;
            }
        }
        events[JournalEntry_GetOpenLocationSlot(events)] = PackLocationEvent(locationEvent);
    }

    // OFFSET 0x0202BAB0
    private static void JournalEntry_SaveArrivalEvent(uint[] events, JournalLocationEvent locationEvent)
    {
        int count = CountLocationEvents(events);
        if (count != 0 && locationEvent.Type == (events[count - 1] & EventTypeMask) && locationEvent.Location == (events[count - 1] >> EventParamShift))
        {
            return;
        }
        events[JournalEntry_GetOpenLocationSlot(events)] = (locationEvent.Type & EventTypeMask) | ((uint)locationEvent.Location << EventParamShift);
    }

    // OFFSET 0x0202BAF8
    private static void JournalEntry_SaveItemEvent(uint[] events, JournalLocationEvent locationEvent)
    {
        int count = CountLocationEvents(events);
        if (count != 0 && locationEvent.Type == (events[count - 1] & EventTypeMask) && locationEvent.Item == (events[count - 1] >> EventParamShift))
        {
            return;
        }
        events[JournalEntry_GetOpenLocationSlot(events)] = (locationEvent.Type & EventTypeMask) | ((uint)locationEvent.Item << EventParamShift);
    }

    // OFFSET 0x0202BB40
    private static void JournalEntry_SaveFieldMoveEvent(uint[] events, JournalLocationEvent locationEvent)
    {
        int count = CountLocationEvents(events);
        if (count != 0 && locationEvent.Type == (events[count - 1] & EventTypeMask) && locationEvent.Location == (events[count - 1] >> EventParamShift))
        {
            return;
        }
        events[JournalEntry_GetOpenLocationSlot(events)] = (locationEvent.Type & EventTypeMask) | ((uint)locationEvent.Location << EventParamShift);
    }

    // OFFSET 0x0202BB88
    private static void JournalEntry_SaveOnlineBattle(JournalOnlineSlot slot, JournalOnlineEvent onlineEvent)
    {
        slot.Type = onlineEvent.Type;
        slot.Stats = (byte)(((onlineEvent.Stats & 0xF) << 2) | (((onlineEvent.Stats >> 4) & 1) << 1) | ((onlineEvent.Stats >> 5) & 1));
        Journal_CopyName(onlineEvent.PlayerName1, 0, slot.Name, 0, JournalOnlineEvent.NameLength);
        Journal_CopyName(onlineEvent.PlayerName2, 0, slot.Extra, 0, JournalOnlineEvent.NameLength);
    }

    // OFFSET 0x0202BBC4
    private static void JournalEntry_SaveOnlinePlayer(JournalOnlineSlot slot, JournalOnlineEvent onlineEvent)
    {
        slot.Type = onlineEvent.Type;
        slot.Stats = (byte)((onlineEvent.Stats >> 4) & 1);
        Journal_CopyName(onlineEvent.PlayerName1, 0, slot.Name, 0, JournalOnlineEvent.NameLength);
    }

    // OFFSET 0x0202BBE0
    private static void JournalEntry_SaveOnlineTrade(JournalOnlineSlot slot, JournalOnlineEvent onlineEvent)
    {
        slot.Type = onlineEvent.Type;
        slot.Stats = (byte)((((onlineEvent.Stats >> 4) & 1) << 2) | ((onlineEvent.Stats >> 6) & 3));
        Journal_CopyName(onlineEvent.PlayerName1, 0, slot.Name, 0, JournalOnlineEvent.NameLength);
        Journal_CopyName(onlineEvent.Title, 0, slot.Extra, 0, JournalOnlineEvent.TitleLength);
    }

    // OFFSET 0x0202BC14
    private static void JournalEntry_SaveOnlineSimple(JournalEntry entry, JournalOnlineEvent onlineEvent)
    {
        int count;
        for (count = 0; count < JournalEntry.OnlineEventCount; count++)
        {
            if (entry.Online[count].Type == 0)
            {
                break;
            }
        }
        if (count != 0 && onlineEvent.Type == entry.Online[count - 1].Type)
        {
            return;
        }
        JournalEntry_GetOpenOnlineSlot(entry).Type = onlineEvent.Type;
    }

    // OFFSET 0x0202BC48
    private static void JournalEntry_SaveOnlineResult(JournalOnlineSlot slot, JournalOnlineEvent onlineEvent)
    {
        slot.Type = onlineEvent.Type;
        slot.Stats = (byte)(onlineEvent.Stats & 0xF);
    }

    // OFFSET 0x0202BC58
    public static uint JournalEntry_CreateTitle(uint mapLabel, int heapID)
    {
        Rtc.GetCurrentDate(out RtcDate date);
        uint title = (uint)date.Year & DateYearMask;
        title |= ((uint)date.Month & DateMonthMask) << DateMonthShift;
        title |= ((uint)date.Day & DateDayMask) << DateDayShift;
        title |= ((uint)date.Week & DateWeekMask) << DateWeekShift;
        title &= DateFieldsMask;
        title |= mapLabel << DateLocationShift;
        return title;
    }

    // OFFSET 0x0202BCC8
    private static JournalLocationEvent JournalEntry_AllocLocationEvent(int heapID)
    {
        return new JournalLocationEvent();
    }

    private static JournalLocationEvent CreateLocationEvent(int heapID, int type)
    {
        JournalLocationEvent locationEvent = JournalEntry_AllocLocationEvent(heapID);
        locationEvent.Type = (byte)type;
        return locationEvent;
    }

    // OFFSET 0x0202BCE4
    public static JournalLocationEvent JournalEntry_CreateEventRestedAtHome(int heapID)
    {
        return CreateLocationEvent(heapID, LocationRestedAtHome);
    }

    // OFFSET 0x0202BCF0
    public static JournalLocationEvent JournalEntry_CreateEventLeftResearchLab(int heapID)
    {
        return CreateLocationEvent(heapID, LocationLeftResearchLab);
    }

    // OFFSET 0x0202BCFC
    public static JournalLocationEvent JournalEntry_CreateEventUsedPCBox(int heapID)
    {
        return CreateLocationEvent(heapID, LocationUsedPC);
    }

    // OFFSET 0x0202BD08
    public static JournalLocationEvent JournalEntry_CreateEventShoppedAtMart(int heapID)
    {
        return CreateLocationEvent(heapID, LocationShopped);
    }

    // OFFSET 0x0202BD14
    public static JournalLocationEvent JournalEntry_CreateEventLotsOfShopping(int heapID)
    {
        return CreateLocationEvent(heapID, LocationLotsOfShopping);
    }

    // OFFSET 0x0202BD20
    public static JournalLocationEvent JournalEntry_CreateEventSoldALittle(int heapID)
    {
        return CreateLocationEvent(heapID, LocationSoldALittle);
    }

    // OFFSET 0x0202BD2C
    public static JournalLocationEvent JournalEntry_CreateEventSoldALot(int heapID)
    {
        return CreateLocationEvent(heapID, LocationSoldALot);
    }

    // OFFSET 0x0202BD38
    public static JournalLocationEvent JournalEntry_CreateEventBusinessAtMart(int heapID)
    {
        return CreateLocationEvent(heapID, LocationBusinessAtMart);
    }

    // OFFSET 0x0202BD44
    public static JournalLocationEvent JournalEntry_CreateEventGymTooTough(ushort gym, int heapID)
    {
        JournalLocationEvent locationEvent = CreateLocationEvent(heapID, LocationGymWasTooTough);
        locationEvent.Location = gym;
        return locationEvent;
    }

    // OFFSET 0x0202BD58
    public static JournalLocationEvent JournalEntry_CreateEventGymLeader(ushort gym, ushort trainer, int heapID)
    {
        JournalLocationEvent locationEvent = CreateLocationEvent(heapID, LocationBeatGymLeader);
        locationEvent.Location = gym;
        locationEvent.Trainer = trainer;
        return locationEvent;
    }

    // OFFSET 0x0202BD70
    public static JournalLocationEvent JournalEntry_CreateEventEliteFour(ushort trainer, int heapID)
    {
        JournalLocationEvent locationEvent = CreateLocationEvent(heapID, LocationBeatEliteFour);
        locationEvent.Trainer = trainer;
        return locationEvent;
    }

    // OFFSET 0x0202BD84
    public static JournalLocationEvent JournalEntry_CreateEventChampion(ushort trainer, int heapID)
    {
        JournalLocationEvent locationEvent = CreateLocationEvent(heapID, LocationBeatChampion);
        locationEvent.Trainer = trainer;
        return locationEvent;
    }

    // OFFSET 0x0202BD98
    public static JournalLocationEvent JournalEntry_CreateEventArrivedInLocation(ushort location, int heapID)
    {
        JournalLocationEvent locationEvent = CreateLocationEvent(heapID, LocationArrivedInLocation);
        locationEvent.Location = location;
        return locationEvent;
    }

    // OFFSET 0x0202BDAC
    public static JournalLocationEvent JournalEntry_CreateEventLeftCave(ushort location, int heapID)
    {
        JournalLocationEvent locationEvent = CreateLocationEvent(heapID, LocationLeftCave);
        locationEvent.Location = location;
        return locationEvent;
    }

    // OFFSET 0x0202BDC0
    public static JournalLocationEvent JournalEntry_CreateEventLeftBuilding(ushort location, int heapID)
    {
        JournalLocationEvent locationEvent = CreateLocationEvent(heapID, LocationLeftBuilding);
        locationEvent.Location = location;
        return locationEvent;
    }

    // OFFSET 0x0202BDD4
    public static JournalLocationEvent JournalEntry_CreateEventGameCorner(int heapID)
    {
        return CreateLocationEvent(heapID, LocationGameCorner);
    }

    // OFFSET 0x0202BDE0
    public static JournalLocationEvent JournalEntry_CreateEventSafariGame(int heapID)
    {
        return CreateLocationEvent(heapID, LocationSafariGame);
    }

    // OFFSET 0x0202BDEC
    public static JournalLocationEvent JournalEntry_CreateEventObtainedItem(ushort item, int heapID)
    {
        JournalLocationEvent locationEvent = CreateLocationEvent(heapID, LocationItemWasObtained);
        locationEvent.Item = item;
        return locationEvent;
    }

    // OFFSET 0x0202BE00
    public static JournalLocationEvent JournalEntry_CreateEventUsedMove(byte moveIndex, ushort location, int heapID)
    {
        JournalLocationEvent locationEvent = CreateLocationEvent(heapID, (byte)(moveIndex + LocationFirstFieldMove));
        locationEvent.Location = location;
        return locationEvent;
    }

    // OFFSET 0x0202BE14
    public static JournalLocationEvent JournalEntry_CreateEventDugUnderground(int heapID)
    {
        return CreateLocationEvent(heapID, LocationDugUnderground);
    }

    // OFFSET 0x0202BE20
    public static JournalLocationEvent JournalEntry_CreateEventBuiltSecretBase(int heapID)
    {
        return CreateLocationEvent(heapID, LocationBuiltSecretBase);
    }

    // OFFSET 0x0202BE2C
    public static JournalLocationEvent JournalEntry_CreateEventBattleFacility(int heapID, int type)
    {
        return CreateLocationEvent(heapID, type);
    }

    // OFFSET 0x0202BE38
    private static JournalMonEvent JournalEntry_AllocMonEvent(int heapID)
    {
        return new JournalMonEvent();
    }

    private static void SetMonEventDetails(JournalMonEvent monEvent, ushort species, byte gender, byte timeOfDay)
    {
        monEvent.Species = species;
        monEvent.Stats = (byte)((monEvent.Stats & ~0xC0) | ((gender & 3) << 6));
        monEvent.Stats = (byte)((monEvent.Stats & ~0x3C) | ((timeOfDay & 0xF) << 2));
    }

    // OFFSET 0x0202BE4C
    public static JournalMonEvent JournalEntry_CreateEventMonCaught(PlayTime playTime, ushort species, byte gender, byte timeOfDay, int heapID)
    {
        JournalMonEvent monEvent = JournalEntry_AllocMonEvent(heapID);
        monEvent.Type = MonCaught;
        SetMonEventDetails(monEvent, species, gender, timeOfDay);
        byte slice = (byte)(PlayTime.PlayTime_GetMinutes(playTime) / MinutesPerSlice);
        if (slice == 0 || slice == 2 || slice == 4)
        {
            monEvent.Stats = (byte)(monEvent.Stats & ~3);
        }
        else if (slice == 1 || slice == 3)
        {
            monEvent.Stats = (byte)((monEvent.Stats & ~3) | 1);
        }
        else
        {
            monEvent.Stats = (byte)((monEvent.Stats & ~3) | 2);
        }
        return monEvent;
    }

    // OFFSET 0x0202BECC
    public static JournalMonEvent JournalEntry_CreateEventMonDefeated(PlayTime playTime, ushort species, byte gender, byte timeOfDay, int heapID)
    {
        JournalMonEvent monEvent = JournalEntry_AllocMonEvent(heapID);
        monEvent.Type = MonDefeated;
        SetMonEventDetails(monEvent, species, gender, timeOfDay);
        byte slice = (byte)(PlayTime.PlayTime_GetMinutes(playTime) / MinutesPerSlice);
        if (slice == 1 || slice == 3 || slice == 5)
        {
            monEvent.Stats = (byte)(monEvent.Stats & ~3);
        }
        else if (slice == 2 || slice == 4)
        {
            monEvent.Stats = (byte)((monEvent.Stats & ~3) | 1);
        }
        else
        {
            monEvent.Stats = (byte)((monEvent.Stats & ~3) | 2);
        }
        return monEvent;
    }

    // OFFSET 0x0202BF4C
    public static JournalTrainerEvent JournalEntry_CreateEventStandardTrainer(ushort location, ushort trainer, int heapID)
    {
        JournalTrainerEvent trainerEvent = new();
        trainerEvent.Packed = 1;
        trainerEvent.Location = location;
        trainerEvent.Packed = (ushort)((trainerEvent.Packed & 1) | (trainer << 1));
        return trainerEvent;
    }

    // OFFSET 0x0202BF7C
    private static JournalOnlineEvent JournalEntry_AllocOnlineEvent(int heapID)
    {
        return new JournalOnlineEvent();
    }

    // OFFSET 0x0202BF94
    private static void Journal_CopyName(ushort[] source, int sourcePos, ushort[] destination, int destinationPos, uint length)
    {
        for (uint i = 0; i < length; i++)
        {
            destination[destinationPos + i] = EndOfString;
        }
        for (uint i = 0; i < length; i++)
        {
            if (source[sourcePos + i] == EndOfString)
            {
                break;
            }
            destination[destinationPos + i] = source[sourcePos + i];
        }
    }

    private static JournalOnlineEvent CreateOnlineBattleEvent(int type, ushort[] opponentName, byte opponentGender, byte result, int heapID)
    {
        JournalOnlineEvent onlineEvent = JournalEntry_AllocOnlineEvent(heapID);
        onlineEvent.Type = (byte)type;
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0xF) | (result & 0xF));
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0x10) | ((opponentGender & 1) << 4));
        Journal_CopyName(opponentName, 0, onlineEvent.PlayerName1, 0, JournalOnlineEvent.NameLength);
        return onlineEvent;
    }

    private static JournalOnlineEvent CreateOnlineTradeEvent(int type, ushort[] playerName, byte playerGender, ushort[] title, byte detail, int heapID)
    {
        JournalOnlineEvent onlineEvent = JournalEntry_AllocOnlineEvent(heapID);
        onlineEvent.Type = (byte)type;
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0x10) | ((playerGender & 1) << 4));
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0xC0) | ((detail & 3) << 6));
        Journal_CopyName(playerName, 0, onlineEvent.PlayerName1, 0, JournalOnlineEvent.NameLength);
        Journal_CopyName(title, 0, onlineEvent.Title, 0, JournalOnlineEvent.TitleLength);
        return onlineEvent;
    }

    // OFFSET 0x0202BFCC
    public static JournalOnlineEvent JournalEntry_CreateEventSingleBattle(ushort[] opponentName, byte opponentGender, byte result, int heapID)
    {
        return CreateOnlineBattleEvent(1, opponentName, opponentGender, result, heapID);
    }

    // OFFSET 0x0202C00C
    public static JournalOnlineEvent JournalEntry_CreateEventDoubleBattle(ushort[] opponentName, byte opponentGender, byte result, int heapID)
    {
        return CreateOnlineBattleEvent(2, opponentName, opponentGender, result, heapID);
    }

    // OFFSET 0x0202C04C
    public static JournalOnlineEvent JournalEntry_CreateEventMultiBattle(ushort[] opponentName1, ushort[] opponentName2, byte opponentGender1, byte opponentGender2, byte result, int heapID)
    {
        JournalOnlineEvent onlineEvent = JournalEntry_AllocOnlineEvent(heapID);
        onlineEvent.Type = 3;
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0xF) | (result & 0xF));
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0x10) | ((opponentGender1 & 1) << 4));
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0x20) | ((opponentGender2 & 1) << 5));
        Journal_CopyName(opponentName1, 0, onlineEvent.PlayerName1, 0, JournalOnlineEvent.NameLength);
        Journal_CopyName(opponentName2, 0, onlineEvent.PlayerName2, 0, JournalOnlineEvent.NameLength);
        return onlineEvent;
    }

    // OFFSET 0x0202C0AC
    public static JournalOnlineEvent JournalEntry_CreateEventMixSingleBattle(ushort[] opponentName, byte opponentGender, byte result, int heapID)
    {
        return CreateOnlineBattleEvent(4, opponentName, opponentGender, result, heapID);
    }

    // OFFSET 0x0202C0EC
    public static JournalOnlineEvent JournalEntry_CreateEventGreetedInUnionRoom(ushort[] playerName, byte playerGender, int heapID)
    {
        JournalOnlineEvent onlineEvent = JournalEntry_AllocOnlineEvent(heapID);
        onlineEvent.Type = 6;
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0x10) | ((playerGender & 1) << 4));
        Journal_CopyName(playerName, 0, onlineEvent.PlayerName1, 0, JournalOnlineEvent.NameLength);
        return onlineEvent;
    }

    // OFFSET 0x0202C11C
    public static JournalOnlineEvent JournalEntry_CreateEventGotPokemonFromTrade(ushort[] playerName, byte playerGender, ushort[] pokemonName, byte pokemonGender, int heapID)
    {
        return CreateOnlineTradeEvent(7, playerName, playerGender, pokemonName, pokemonGender, heapID);
    }

    // OFFSET 0x0202C168
    public static JournalOnlineEvent JournalEntry_CreateEventDrewPictures(int heapID)
    {
        JournalOnlineEvent onlineEvent = JournalEntry_AllocOnlineEvent(heapID);
        onlineEvent.Type = 8;
        return onlineEvent;
    }

    // OFFSET 0x0202C174
    public static JournalOnlineEvent JournalEntry_CreateEventUnionBattle(ushort[] opponentName, byte opponentGender, byte result, int heapID)
    {
        return CreateOnlineBattleEvent(11, opponentName, opponentGender, result, heapID);
    }

    // OFFSET 0x0202C1B4
    public static JournalOnlineEvent JournalEntry_CreateEventMixedRecords(int heapID)
    {
        JournalOnlineEvent onlineEvent = JournalEntry_AllocOnlineEvent(heapID);
        onlineEvent.Type = 12;
        return onlineEvent;
    }

    // OFFSET 0x0202C1C0
    public static JournalOnlineEvent JournalEntry_CreateEventPlacedInContest(byte place, int heapID)
    {
        JournalOnlineEvent onlineEvent = JournalEntry_AllocOnlineEvent(heapID);
        onlineEvent.Type = 13;
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0xF) | (place & 0xF));
        return onlineEvent;
    }

    // OFFSET 0x0202C1E0
    public static JournalOnlineEvent JournalEntry_CreateEventMadePoffins(int heapID)
    {
        JournalOnlineEvent onlineEvent = JournalEntry_AllocOnlineEvent(heapID);
        onlineEvent.Type = 14;
        return onlineEvent;
    }

    // OFFSET 0x0202C1EC
    public static JournalOnlineEvent JournalEntry_CreateEventGotPokemonGTS(ushort[] playerName, byte playerGender, ushort[] pokemonName, byte pokemonGender, int heapID)
    {
        return CreateOnlineTradeEvent(15, playerName, playerGender, pokemonName, pokemonGender, heapID);
    }

    // OFFSET 0x0202C238
    public static JournalOnlineEvent JournalEntry_CreateEventBattleRoom(int heapID)
    {
        JournalOnlineEvent onlineEvent = JournalEntry_AllocOnlineEvent(heapID);
        onlineEvent.Type = 16;
        return onlineEvent;
    }

    // OFFSET 0x0202C244
    public static JournalOnlineEvent JournalEntry_CreateEventMisc(int heapID, byte type)
    {
        JournalOnlineEvent onlineEvent = JournalEntry_AllocOnlineEvent(heapID);
        onlineEvent.Type = type;
        return onlineEvent;
    }

    // OFFSET 0x0202C250
    public static JournalOnlineEvent JournalEntry_CreateEventInteraction(ushort[] playerName, byte playerGender, int heapID, byte type)
    {
        JournalOnlineEvent onlineEvent = JournalEntry_AllocOnlineEvent(heapID);
        onlineEvent.Type = type;
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0x10) | ((playerGender & 1) << 4));
        Journal_CopyName(playerName, 0, onlineEvent.PlayerName1, 0, JournalOnlineEvent.NameLength);
        return onlineEvent;
    }

    // OFFSET 0x0202C280
    public static JournalOnlineEvent JournalEntry_CreateEventPlazaMinigame(byte minigame, int heapID, byte type)
    {
        JournalOnlineEvent onlineEvent = JournalEntry_AllocOnlineEvent(heapID);
        onlineEvent.Type = type;
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0xF) | (minigame & 0xF));
        return onlineEvent;
    }

    // OFFSET 0x0202C2A4
    public static void JournalEntry_GetData(Journal journal, object destination, int dataType, int page)
    {
        JournalEntry entry = journal.Entries[page];
        switch (dataType)
        {
            case DataDate:
                JournalEntry_GetDate(entry, (uint[])destination);
                break;
            case DataLocation:
                JournalEntry_GetLocationEvents(entry, (JournalLocationEvent[])destination);
                break;
            case DataMon:
                JournalEntry_GetMonEvent(entry, (JournalMonEvent)destination);
                break;
            case DataTrainer:
                JournalEntry_GetTrainerEvent(entry, (JournalTrainerEvent)destination);
                break;
            case DataOnline:
                JournalEntry_GetOnlineEvents(entry, (JournalOnlineEvent[])destination);
                break;
        }
    }

    // OFFSET 0x0202C2FC
    private static void JournalEntry_GetDate(JournalEntry entry, uint[] destination)
    {
        destination[0] = entry.Date;
    }

    // OFFSET 0x0202C304
    private static void JournalEntry_GetLocationEvents(JournalEntry entry, JournalLocationEvent[] destination)
    {
        for (int i = 0; i < JournalEntry.LocationEventCount; i++)
        {
            JournalLocationEvent locationEvent = destination[i];
            locationEvent.Type = 0;
            locationEvent.Location = 0;
            locationEvent.Trainer = 0;
            locationEvent.Item = 0;
            uint packed = entry.Locations[i];
            int type = (int)(packed & EventTypeMask);
            if (type > LastLocationEvent || type == LocationNone || type == LocationUnused31)
            {
                continue;
            }
            if (type >= LocationGymWasTooTough && type <= LocationBeatChampion)
            {
                JournalEntry_UnpackLeagueEvent(packed, locationEvent);
            }
            else if (type >= LocationArrivedInLocation && type <= LocationLeftBuilding)
            {
                JournalEntry_UnpackArrivalEvent(packed, locationEvent);
            }
            else if (type == LocationItemWasObtained)
            {
                JournalEntry_UnpackItemEvent(packed, locationEvent);
            }
            else if (type >= LocationFirstFieldMove && type < LocationDugUnderground)
            {
                JournalEntry_UnpackFieldMoveEvent(packed, locationEvent);
            }
            else
            {
                JournalEntry_UnpackSimpleEvent(packed, locationEvent);
            }
        }
    }

    // OFFSET 0x0202C3B8
    private static void JournalEntry_GetMonEvent(JournalEntry entry, JournalMonEvent destination)
    {
        destination.Type = (byte)entry.Mon[0];
        destination.Stats = (byte)(entry.Mon[0] >> 8);
        destination.Species = entry.Mon[1];
    }

    // OFFSET 0x0202C3C4
    private static void JournalEntry_GetTrainerEvent(JournalEntry entry, JournalTrainerEvent destination)
    {
        destination.Packed = entry.Trainer[0];
        destination.Location = entry.Trainer[1];
    }

    // OFFSET 0x0202C3D0
    private static void JournalEntry_GetOnlineEvents(JournalEntry entry, JournalOnlineEvent[] destination)
    {
        for (int i = 0; i < JournalEntry.OnlineEventCount; i++)
        {
            JournalOnlineSlot slot = entry.Online[i];
            JournalOnlineEvent onlineEvent = destination[i];
            onlineEvent.Clear();
            switch (slot.Type)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 11:
                    JournalEntry_UnpackOnlineBattle(slot, onlineEvent);
                    break;
                case 6:
                case 22:
                case 23:
                    JournalEntry_UnpackOnlinePlayer(slot, onlineEvent);
                    break;
                case 7:
                case 9:
                case 15:
                    JournalEntry_UnpackOnlineTrade(slot, onlineEvent);
                    break;
                case 13:
                case 24:
                    JournalEntry_UnpackOnlineResult(slot, onlineEvent);
                    break;
                case 8:
                case 10:
                case 12:
                case 14:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                    JournalEntry_UnpackOnlineSimple(slot, onlineEvent);
                    break;
            }
        }
    }

    // OFFSET 0x0202C474
    private static void JournalEntry_UnpackSimpleEvent(uint packed, JournalLocationEvent locationEvent)
    {
        locationEvent.Type = (byte)(packed & EventTypeMask);
    }

    // OFFSET 0x0202C47C
    private static void JournalEntry_UnpackLeagueEvent(uint packed, JournalLocationEvent locationEvent)
    {
        locationEvent.Type = (byte)(packed & EventTypeMask);
        locationEvent.Trainer = (ushort)((packed >> EventTrainerShift) & EventTrainerMask);
        locationEvent.Location = (ushort)(packed >> EventParamShift);
    }

    // OFFSET 0x0202C494
    private static void JournalEntry_UnpackArrivalEvent(uint packed, JournalLocationEvent locationEvent)
    {
        locationEvent.Type = (byte)(packed & EventTypeMask);
        locationEvent.Location = (ushort)(packed >> EventParamShift);
    }

    // OFFSET 0x0202C4A0
    private static void JournalEntry_UnpackItemEvent(uint packed, JournalLocationEvent locationEvent)
    {
        locationEvent.Type = (byte)(packed & EventTypeMask);
        locationEvent.Item = (ushort)(packed >> EventParamShift);
    }

    // OFFSET 0x0202C4AC
    private static void JournalEntry_UnpackFieldMoveEvent(uint packed, JournalLocationEvent locationEvent)
    {
        locationEvent.Type = (byte)(packed & EventTypeMask);
        locationEvent.Location = (ushort)(packed >> EventParamShift);
    }

    // OFFSET 0x0202C4B8
    private static void JournalEntry_UnpackOnlineBattle(JournalOnlineSlot slot, JournalOnlineEvent onlineEvent)
    {
        onlineEvent.Type = slot.Type;
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0xF) | ((slot.Stats >> 2) & 0xF));
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0x10) | (((slot.Stats >> 1) & 1) << 4));
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0x20) | ((slot.Stats & 1) << 5));
        Journal_CopyName(slot.Name, 0, onlineEvent.PlayerName1, 0, JournalOnlineEvent.NameLength);
        Journal_CopyName(slot.Extra, 0, onlineEvent.PlayerName2, 0, JournalOnlineEvent.NameLength);
    }

    // OFFSET 0x0202C528
    private static void JournalEntry_UnpackOnlinePlayer(JournalOnlineSlot slot, JournalOnlineEvent onlineEvent)
    {
        onlineEvent.Type = slot.Type;
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0x10) | ((slot.Stats & 1) << 4));
        Journal_CopyName(slot.Name, 0, onlineEvent.PlayerName1, 0, JournalOnlineEvent.NameLength);
    }

    // OFFSET 0x0202C54C
    private static void JournalEntry_UnpackOnlineTrade(JournalOnlineSlot slot, JournalOnlineEvent onlineEvent)
    {
        onlineEvent.Type = slot.Type;
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0x10) | (((slot.Stats >> 2) & 1) << 4));
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0xC0) | ((slot.Stats & 3) << 6));
        Journal_CopyName(slot.Name, 0, onlineEvent.PlayerName1, 0, JournalOnlineEvent.NameLength);
        Journal_CopyName(slot.Extra, 0, onlineEvent.Title, 0, JournalOnlineEvent.TitleLength);
    }

    // OFFSET 0x0202C5A4
    private static void JournalEntry_UnpackOnlineSimple(JournalOnlineSlot slot, JournalOnlineEvent onlineEvent)
    {
        onlineEvent.Type = slot.Type;
    }

    // OFFSET 0x0202C5AC
    private static void JournalEntry_UnpackOnlineResult(JournalOnlineSlot slot, JournalOnlineEvent onlineEvent)
    {
        onlineEvent.Type = slot.Type;
        onlineEvent.Stats = (byte)((onlineEvent.Stats & ~0xF) | (slot.Stats & 0xF));
    }

    // OFFSET 0x0202C5C4
    public static void JournalEntry_CreateAndSaveEventMapTransition(TrainerInfo trainerInfo, Journal? journal, int currentMap, int previousMap, int heapID)
    {
        JournalLocationEvent? locationEvent = null;
        if (MapHeader.MapHeader_IsCave(previousMap))
        {
            if (MapHeader.MapHeader_IsOutdoors(currentMap))
            {
                locationEvent = JournalEntry_CreateEventLeftCave((ushort)MapHeader.MapHeader_GetMapLabelTextID(previousMap), heapID);
            }
        }
        else if (MapHeader.MapHeader_IsBuilding(previousMap))
        {
            if (MapHeader.MapHeader_IsOutdoors(currentMap))
            {
                if (previousMap == MapPlayerHouse)
                {
                    locationEvent = JournalEntry_CreateEventRestedAtHome(heapID);
                }
                else if (previousMap == MapResearchLab)
                {
                    locationEvent = JournalEntry_CreateEventLeftResearchLab(heapID);
                }
                else
                {
                    uint mapLabel = (uint)MapHeader.MapHeader_GetMapLabelTextID(previousMap);
                    for (uint i = 0; i < BuildingCount; i++)
                    {
                        if (mapLabel == Bin.U32(Buildings, (int)i * BuildingEntrySize))
                        {
                            locationEvent = JournalEntry_CreateEventLeftBuilding((ushort)mapLabel, heapID);
                        }
                    }
                }
            }
        }
        else if (MapHeader.MapHeader_IsOutdoors(previousMap))
        {
            if (MapHeader.MapHeader_IsBuilding(currentMap))
            {
                uint gym = Journal_GetGymTooTough(trainerInfo, (uint)currentMap);
                if (gym != NoGym)
                {
                    locationEvent = JournalEntry_CreateEventGymTooTough((ushort)gym, heapID);
                }
            }
        }
        if (locationEvent != null)
        {
            JournalEntry_SaveData(journal, locationEvent, DataLocation);
        }
    }

    // OFFSET 0x0202C6A4
    public static uint Journal_DoesBuildingUseExitedMessage(uint mapLabel)
    {
        for (uint i = 0; i < BuildingCount; i++)
        {
            if (mapLabel == Bin.U32(Buildings, (int)i * BuildingEntrySize))
            {
                return Bin.U32(Buildings, (int)i * BuildingEntrySize + 4);
            }
        }
        return 0;
    }

    // OFFSET 0x0202C6CC
    private static uint Journal_GetGymTooTough(TrainerInfo trainerInfo, uint mapID)
    {
        for (byte i = 0; i < GymCount; i++)
        {
            if (mapID == (uint)Bin.U16(Gyms, i * GymEntrySize + 4))
            {
                if (!TrainerInfo.TrainerInfo_HasBadge(trainerInfo, Bin.U16(Gyms, i * GymEntrySize + 6)))
                {
                    return i;
                }
                break;
            }
        }
        return NoGym;
    }

    // OFFSET 0x0202C704
    public static void JournalEntry_CreateAndSaveEventArrivedInLocation(Journal? journal, uint mapLabel, int heapID)
    {
        JournalLocationEvent locationEvent = JournalEntry_CreateEventArrivedInLocation((ushort)mapLabel, heapID);
        JournalEntry_SaveData(journal, locationEvent, DataLocation);
    }

    // OFFSET 0x0202C720
    public static void JournalEntry_CreateAndSaveEventTrainer(Journal? journal, uint mapLabel, uint trainerID, int heapID)
    {
        byte kind = Journal_GetTrainerKind(trainerID);
        if (kind < GymCount)
        {
            JournalEntry_SaveData(journal, JournalEntry_CreateEventGymLeader(kind, (ushort)trainerID, heapID), DataLocation);
        }
        else if (kind == TrainerEliteFour)
        {
            JournalEntry_SaveData(journal, JournalEntry_CreateEventEliteFour((ushort)trainerID, heapID), DataLocation);
        }
        else if (kind == TrainerChampion)
        {
            JournalEntry_SaveData(journal, JournalEntry_CreateEventChampion((ushort)trainerID, heapID), DataLocation);
        }
        else
        {
            JournalEntry_SaveData(journal, JournalEntry_CreateEventStandardTrainer((ushort)mapLabel, (ushort)trainerID, heapID), DataTrainer);
        }
    }

    // OFFSET 0x0202C78C
    private static byte Journal_GetTrainerKind(uint trainerID)
    {
        for (byte i = 0; i < GymCount; i++)
        {
            if (trainerID == Bin.U32(Gyms, i * GymEntrySize))
            {
                return i;
            }
        }
        for (int i = 0; i < EliteFourCount; i++)
        {
            if (trainerID == FirstEliteFourTrainer + i || trainerID == FirstEliteFourRematchTrainer + i)
            {
                return TrainerEliteFour;
            }
        }
        if (trainerID == ChampionTrainer || trainerID == ChampionRematchTrainer)
        {
            return TrainerChampion;
        }
        return TrainerUnknown;
    }
}
