using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class GameTime
{
    public const int MaxPenaltyMinutes = 24 * 60;

    public int Canary;
    public RtcDate Date;
    public RtcTime Time;
    public uint Day;
    public long StartTimestamp;
    public long FirstCompletionTimestamp;
    public uint PenaltyInMinutes;
}

public sealed class SystemData : ISaveBlock
{
    public const int MacAddressLength = 6;

    private const int GameTimeOffset = 0x10;

    public long RtcOffset;
    public readonly byte[] MacAddress = new byte[MacAddressLength];
    public byte OwnerBirthMonth;
    public byte OwnerBirthDayOfMonth;
    public readonly GameTime GameTime = new();
    public byte IsMysteryGiftUnlocked;
    public int DwcProfileId;
    public readonly uint[] Unused50 = new uint[3];

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W32(body, offset, (uint)RtcOffset);
        Bin.W32(body, offset + 4, (uint)(RtcOffset >> 32));
        Array.Copy(MacAddress, 0, body, offset + 8, MacAddressLength);
        body[offset + 0xE] = OwnerBirthMonth;
        body[offset + 0xF] = OwnerBirthDayOfMonth;
        int time = offset + GameTimeOffset;
        Bin.W32(body, time, (uint)GameTime.Canary);
        Bin.W32(body, time + 4, (uint)GameTime.Date.Year);
        Bin.W32(body, time + 8, (uint)GameTime.Date.Month);
        Bin.W32(body, time + 0xC, (uint)GameTime.Date.Day);
        Bin.W32(body, time + 0x10, (uint)GameTime.Date.Week);
        Bin.W32(body, time + 0x14, (uint)GameTime.Time.Hour);
        Bin.W32(body, time + 0x18, (uint)GameTime.Time.Minute);
        Bin.W32(body, time + 0x1C, (uint)GameTime.Time.Second);
        Bin.W32(body, time + 0x20, GameTime.Day);
        Bin.W32(body, time + 0x24, (uint)GameTime.StartTimestamp);
        Bin.W32(body, time + 0x28, (uint)(GameTime.StartTimestamp >> 32));
        Bin.W32(body, time + 0x2C, (uint)GameTime.FirstCompletionTimestamp);
        Bin.W32(body, time + 0x30, (uint)(GameTime.FirstCompletionTimestamp >> 32));
        Bin.W32(body, time + 0x34, GameTime.PenaltyInMinutes);
        body[offset + 0x48] = IsMysteryGiftUnlocked;
        Bin.W32(body, offset + 0x4C, (uint)DwcProfileId);
        for (int i = 0; i < Unused50.Length; i++)
        {
            Bin.W32(body, offset + 0x50 + i * 4, Unused50[i]);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        RtcOffset = (long)(Bin.U32(body, offset) | ((ulong)Bin.U32(body, offset + 4) << 32));
        Array.Copy(body, offset + 8, MacAddress, 0, MacAddressLength);
        OwnerBirthMonth = body[offset + 0xE];
        OwnerBirthDayOfMonth = body[offset + 0xF];
        int time = offset + GameTimeOffset;
        GameTime.Canary = (int)Bin.U32(body, time);
        GameTime.Date.Year = (int)Bin.U32(body, time + 4);
        GameTime.Date.Month = (int)Bin.U32(body, time + 8);
        GameTime.Date.Day = (int)Bin.U32(body, time + 0xC);
        GameTime.Date.Week = (int)Bin.U32(body, time + 0x10);
        GameTime.Time.Hour = (int)Bin.U32(body, time + 0x14);
        GameTime.Time.Minute = (int)Bin.U32(body, time + 0x18);
        GameTime.Time.Second = (int)Bin.U32(body, time + 0x1C);
        GameTime.Day = Bin.U32(body, time + 0x20);
        GameTime.StartTimestamp = (long)(Bin.U32(body, time + 0x24) | ((ulong)Bin.U32(body, time + 0x28) << 32));
        GameTime.FirstCompletionTimestamp = (long)(Bin.U32(body, time + 0x2C) | ((ulong)Bin.U32(body, time + 0x30) << 32));
        GameTime.PenaltyInMinutes = Bin.U32(body, time + 0x34);
        IsMysteryGiftUnlocked = body[offset + 0x48];
        DwcProfileId = (int)Bin.U32(body, offset + 0x4C);
        for (int i = 0; i < Unused50.Length; i++)
        {
            Unused50[i] = Bin.U32(body, offset + 0x50 + i * 4);
        }
    }

    // OFFSET 0x02025CB4
    public static void SystemData_Clear(SystemData systemData)
    {
        systemData.RtcOffset = 0;
        Array.Clear(systemData.MacAddress);
        systemData.OwnerBirthMonth = 0;
        systemData.OwnerBirthDayOfMonth = 0;
        systemData.IsMysteryGiftUnlocked = 0;
        systemData.DwcProfileId = 0;
        Array.Clear(systemData.Unused50);
        GameTime_Clear(systemData.GameTime);
    }

    // OFFSET 0x02025CCC
    public static SystemData SaveData_GetSystemData(SaveData saveData)
    {
        return (SystemData)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntrySystem);
    }

    // OFFSET 0x02025CD8
    public static GameTime SaveData_GetGameTime(SaveData saveData)
    {
        SystemData systemData = SaveData_GetSystemData(saveData);
        return systemData.GameTime;
    }

    // OFFSET 0x02025CE4
    public static void SystemData_Init(SystemData systemData)
    {
        systemData.RtcOffset = Platform.OwnerRtcOffset;
        Array.Copy(Platform.MacAddress, systemData.MacAddress, MacAddressLength);
        systemData.OwnerBirthMonth = Platform.OwnerBirthMonth;
        systemData.OwnerBirthDayOfMonth = Platform.OwnerBirthDay;
    }

    // OFFSET 0x02025D10
    public static bool SystemData_MatchesCurrentSystem(SystemData systemData)
    {
        for (int i = 0; i < MacAddressLength; i++)
        {
            if (Platform.MacAddress[i] != systemData.MacAddress[i])
            {
                return false;
            }
        }
        return true;
    }

    // OFFSET 0x02025D40
    public static bool SystemData_MatchesCurrentRTCOffset(SystemData systemData)
    {
        return Platform.OwnerRtcOffset == systemData.RtcOffset;
    }

    // OFFSET 0x02025D5C
    public static byte SystemData_GetOwnerBirthMonth(SystemData systemData)
    {
        return systemData.OwnerBirthMonth;
    }

    // OFFSET 0x02025D60
    public static byte SystemData_GetOwnerBirthDayOfMonth(SystemData systemData)
    {
        return systemData.OwnerBirthDayOfMonth;
    }

    // OFFSET 0x02025D64
    public static byte SystemData_IsMysteryGiftUnlocked(SystemData systemData)
    {
        return systemData.IsMysteryGiftUnlocked;
    }

    // OFFSET 0x02025D6C
    public static void SystemData_SetMysteryGiftUnlocked(SystemData systemData, byte isUnlocked)
    {
        systemData.IsMysteryGiftUnlocked = isUnlocked;
    }

    // OFFSET 0x02025D74
    public static int SystemData_GetDWCProfileId(SystemData systemData)
    {
        return systemData.DwcProfileId;
    }

    // OFFSET 0x02025D78
    public static void SystemData_SetDWCProfileId(SystemData systemData, int profileId)
    {
        if (systemData.DwcProfileId == 0)
        {
            systemData.DwcProfileId = profileId;
        }
    }

    // OFFSET 0x02025D84
    public static void GameTime_Clear(GameTime gameTime)
    {
        gameTime.Canary = 1;
        Rtc.GetCurrentDateTime(out gameTime.Date, out gameTime.Time);
        gameTime.Day = (uint)RealTimeClock.ConvertDateToDay(gameTime.Date);
        gameTime.StartTimestamp = RealTimeClock.ConvertDateTimeToSecond(gameTime.Date, gameTime.Time);
        gameTime.FirstCompletionTimestamp = 0;
        gameTime.PenaltyInMinutes = 0;
    }

    // OFFSET 0x02025DB8
    public static bool GameTime_HasPenalty(GameTime gameTime)
    {
        return gameTime.PenaltyInMinutes != 0;
    }

    // OFFSET 0x02025DC8
    public static void GameTime_DecrementPenalty(GameTime gameTime, uint minutes)
    {
        if (gameTime.PenaltyInMinutes > GameTime.MaxPenaltyMinutes)
        {
            gameTime.PenaltyInMinutes = GameTime.MaxPenaltyMinutes;
        }
        if (gameTime.PenaltyInMinutes < minutes)
        {
            gameTime.PenaltyInMinutes = 0;
        }
        else
        {
            gameTime.PenaltyInMinutes -= minutes;
        }
    }

    // OFFSET 0x02025DE8
    public static void GameTime_StartPenalty(GameTime gameTime)
    {
        gameTime.PenaltyInMinutes = GameTime.MaxPenaltyMinutes;
        Rtc.GetCurrentDateTime(out gameTime.Date, out gameTime.Time);
        gameTime.Day = (uint)RealTimeClock.ConvertDateToDay(gameTime.Date);
    }
}
