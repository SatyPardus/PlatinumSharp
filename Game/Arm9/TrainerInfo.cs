using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class Coins
{
    public const uint MaxValue = 50000;

    public ushort Value;

    // OFFSET 0x0202B594
    public static void Coins_Init(Coins coins)
    {
        coins.Value = 0;
    }

    // OFFSET 0x0202B59C
    public static void Coins_SetValue(Coins coins, uint value)
    {
        if (value > MaxValue)
        {
            ErrorHandling.GF_AssertFail();
        }
        coins.Value = (ushort)value;
    }

    // OFFSET 0x0202B5B4
    public static ushort Coins_GetValue(Coins coins)
    {
        return coins.Value;
    }

    // OFFSET 0x0202B5B8
    public static bool Coins_Add(Coins coins, ushort amount)
    {
        if (coins.Value >= MaxValue)
        {
            return false;
        }
        coins.Value += amount;
        if (coins.Value > MaxValue)
        {
            coins.Value = (ushort)MaxValue;
        }
        return true;
    }

    // OFFSET 0x0202B5D8
    public static bool Coins_CanAdd(Coins coins, ushort amount)
    {
        return (uint)amount + coins.Value <= MaxValue;
    }

    // OFFSET 0x0202B5F0
    public static bool Coins_Subtract(Coins coins, ushort amount)
    {
        if (coins.Value < amount)
        {
            return false;
        }
        coins.Value -= amount;
        return true;
    }
}

public sealed partial class PlayTime
{
    public const ushort MaxHours = 999;
    public const byte MaxMinutes = 59;
    public const byte MaxSeconds = 59;

    public ushort Hours;
    public byte Minutes;
    public byte Seconds;

    // OFFSET 0x0202CBE4
    public static void PlayTime_Init(PlayTime playTime)
    {
        playTime.Hours = 0;
        playTime.Minutes = 0;
        playTime.Seconds = 0;
    }

    // OFFSET 0x0202CBF0
    public static void PlayTime_Increment(PlayTime playTime, uint seconds)
    {
        uint hours = playTime.Hours;
        if (hours == MaxHours && playTime.Minutes == MaxMinutes && playTime.Seconds == MaxSeconds)
        {
            return;
        }
        uint minutes = playTime.Minutes;
        uint total = playTime.Seconds + seconds;
        if (total > MaxSeconds)
        {
            minutes += total / 60;
            total %= 60;
            if (minutes > MaxMinutes)
            {
                hours += minutes / 60;
                minutes %= 60;
                if (hours >= MaxHours)
                {
                    minutes = MaxMinutes;
                    hours = MaxHours;
                    total = minutes;
                }
            }
        }
        playTime.Hours = (ushort)hours;
        playTime.Minutes = (byte)minutes;
        playTime.Seconds = (byte)total;
    }

    // OFFSET 0x0202CC58
    public static ushort PlayTime_GetHours(PlayTime playTime)
    {
        return playTime.Hours;
    }

    // OFFSET 0x0202CC5C
    public static byte PlayTime_GetMinutes(PlayTime playTime)
    {
        return playTime.Minutes;
    }

    // OFFSET 0x0202CC60
    public static byte PlayTime_GetSeconds(PlayTime playTime)
    {
        return playTime.Seconds;
    }
}

public sealed partial class Options
{
    private const ushort TextSpeedMask = 0xF;
    private const ushort TextSpeedNormal = 1;
    private const ushort SoundModeMask = 0x30;
    private const ushort BattleStyleMask = 0x40;
    private const ushort BattleSceneMask = 0x80;
    private const ushort ButtonModeMask = 0x300;
    private const ushort FrameMask = 0x7C00;

    // OFFSET 0x02027A1C
    public static void Options_Init(Options options)
    {
        options.Flags = 0;
        options.Flags = (ushort)((options.Flags & ~TextSpeedMask) | TextSpeedNormal);
        options.Flags &= unchecked((ushort)~SoundModeMask);
        options.Flags &= unchecked((ushort)~BattleStyleMask);
        options.Flags &= unchecked((ushort)~BattleSceneMask);
        options.Flags &= unchecked((ushort)~ButtonModeMask);
        options.Flags &= unchecked((ushort)~FrameMask);
    }
}

public sealed class TrainerInfo
{
    public const int NameLength = 8;
    public const int Size = 0x20;
    public const uint MaxMoney = 999999;
    public const byte LanguageEnglish = 2;
    public const byte GameCodePlatinum = 0xC;

    private const byte MainStoryClearedFlag = 1;
    private const byte NationalDexFlag = 2;
    private const int ComparedNameLength = 7;

    public readonly ushort[] Name = new ushort[NameLength];
    public uint ID;
    public uint Money;
    public byte Gender;
    public byte Language;
    public byte BadgeMask;
    public byte Appearance;
    public byte GameCode;
    public byte Flags;

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < NameLength; i++)
        {
            Bin.W16(body, offset + i * 2, Name[i]);
        }
        Bin.W32(body, offset + 0x10, ID);
        Bin.W32(body, offset + 0x14, Money);
        body[offset + 0x18] = Gender;
        body[offset + 0x19] = Language;
        body[offset + 0x1A] = BadgeMask;
        body[offset + 0x1B] = Appearance;
        body[offset + 0x1C] = GameCode;
        body[offset + 0x1D] = Flags;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < NameLength; i++)
        {
            Name[i] = (ushort)Bin.U16(body, offset + i * 2);
        }
        ID = Bin.U32(body, offset + 0x10);
        Money = Bin.U32(body, offset + 0x14);
        Gender = body[offset + 0x18];
        Language = body[offset + 0x19];
        BadgeMask = body[offset + 0x1A];
        Appearance = body[offset + 0x1B];
        GameCode = body[offset + 0x1C];
        Flags = body[offset + 0x1D];
    }

    // OFFSET 0x02025E68
    public static int TrainerInfo_Size()
    {
        return Size;
    }

    // OFFSET 0x02025E6C
    public static TrainerInfo TrainerInfo_New(int heapID)
    {
        TrainerInfo info = new();
        TrainerInfo_Init(info);
        return info;
    }

    // OFFSET 0x02025E80
    public static void TrainerInfo_Copy(TrainerInfo source, TrainerInfo destination)
    {
        Array.Copy(source.Name, destination.Name, NameLength);
        destination.ID = source.ID;
        destination.Money = source.Money;
        destination.Gender = source.Gender;
        destination.Language = source.Language;
        destination.BadgeMask = source.BadgeMask;
        destination.Appearance = source.Appearance;
        destination.GameCode = source.GameCode;
        destination.Flags = source.Flags;
    }

    // OFFSET 0x02025E8C
    public static void TrainerInfo_Init(TrainerInfo info)
    {
        Array.Clear(info.Name);
        info.ID = 0;
        info.Money = 0;
        info.Gender = 0;
        info.BadgeMask = 0;
        info.Appearance = 0;
        info.GameCode = 0;
        info.Flags = 0;
        info.Language = LanguageEnglish;
        TrainerInfo_SetGameCode(info, GameCodePlatinum);
    }

    // OFFSET 0x02025EA8
    public static bool TrainerInfo_HasNoName(TrainerInfo info)
    {
        for (int i = 0; i < NameLength; i++)
        {
            if (info.Name[i] != 0)
            {
                return false;
            }
        }
        return true;
    }

    // OFFSET 0x02025EC0
    public static void TrainerInfo_SetName(TrainerInfo info, ushort[] name)
    {
        if (CharCode.CharCode_Length(name, 0) >= NameLength)
        {
            ErrorHandling.GF_AssertFail();
        }
        CharCode.CharCode_Copy(info.Name, 0, name, 0);
    }

    // OFFSET 0x02025EE0
    public static void TrainerInfo_SetNameFromString(TrainerInfo info, GameString name)
    {
        GameString.String_ToChars(name, info.Name, NameLength);
    }

    // OFFSET 0x02025EF0
    public static ushort[] TrainerInfo_Name(TrainerInfo info)
    {
        return info.Name;
    }

    // OFFSET 0x02025EF4
    public static void TrainerInfo_NameString(TrainerInfo info, GameString name)
    {
        GameString.String_CopyChars(name, info.Name);
    }

    // OFFSET 0x02025F04
    public static GameString TrainerInfo_NameNewString(TrainerInfo info, int heapID)
    {
        GameString name = GameString.String_Init(NameLength, heapID);
        TrainerInfo_NameString(info, name);
        return name;
    }

    // OFFSET 0x02025F1C
    public static void TrainerInfo_SetID(TrainerInfo info, uint id)
    {
        info.ID = id;
    }

    // OFFSET 0x02025F20
    public static uint TrainerInfo_ID(TrainerInfo info)
    {
        return info.ID;
    }

    // OFFSET 0x02025F24
    public static ushort TrainerInfo_ID_LowHalf(TrainerInfo info)
    {
        return (ushort)info.ID;
    }

    // OFFSET 0x02025F2C
    public static void TrainerInfo_SetGender(TrainerInfo info, int gender)
    {
        info.Gender = (byte)gender;
    }

    // OFFSET 0x02025F30
    public static uint TrainerInfo_Gender(TrainerInfo info)
    {
        return info.Gender;
    }

    // OFFSET 0x02025F34
    public static bool TrainerInfo_HasBadge(TrainerInfo info, int badgeNum)
    {
        return (info.BadgeMask & (1 << badgeNum)) != 0;
    }

    // OFFSET 0x02025F48
    public static void TrainerInfo_SetBadge(TrainerInfo info, int badgeNum)
    {
        info.BadgeMask |= (byte)(1 << badgeNum);
    }

    // OFFSET 0x02025F58
    public static int TrainerInfo_BadgeCount(TrainerInfo info)
    {
        int count = 0;
        for (uint mask = info.BadgeMask; mask != 0; mask >>= 1)
        {
            if ((mask & 1) != 0)
            {
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x02025F74
    public static uint TrainerInfo_Money(TrainerInfo info)
    {
        return info.Money;
    }

    // OFFSET 0x02025F78
    public static uint TrainerInfo_SetMoney(TrainerInfo info, uint money)
    {
        if (money > MaxMoney)
        {
            money = MaxMoney;
        }
        info.Money = money;
        return money;
    }

    // OFFSET 0x02025F8C
    public static byte TrainerInfo_Appearance(TrainerInfo info)
    {
        return info.Appearance;
    }

    // OFFSET 0x02025F90
    public static void TrainerInfo_SetAppearance(TrainerInfo info, byte appearance)
    {
        info.Appearance = appearance;
    }

    // OFFSET 0x02025F94
    public static uint TrainerInfo_GiveMoney(TrainerInfo info, uint money)
    {
        if (money > MaxMoney)
        {
            info.Money = MaxMoney;
        }
        else
        {
            info.Money += money;
        }
        if (info.Money > MaxMoney)
        {
            info.Money = MaxMoney;
        }
        return info.Money;
    }

    // OFFSET 0x02025FB8
    public static uint TrainerInfo_TakeMoney(TrainerInfo info, uint money)
    {
        if (info.Money < money)
        {
            info.Money = 0;
        }
        else
        {
            info.Money -= money;
        }
        return info.Money;
    }

    // OFFSET 0x02025FCC
    public static byte TrainerInfo_GameCode(TrainerInfo info)
    {
        return info.GameCode;
    }

    // OFFSET 0x02025FD0
    public static void TrainerInfo_SetGameCode(TrainerInfo info, byte gameCode)
    {
        info.GameCode = gameCode;
    }

    // OFFSET 0x02025FD4
    public static byte TrainerInfo_DPGameCode()
    {
        return 0;
    }

    // OFFSET 0x02025FD8
    public static byte TrainerInfo_Language(TrainerInfo info)
    {
        return info.Language;
    }

    // OFFSET 0x02025FDC
    public static void TrainerInfo_SetLanguage(TrainerInfo info, byte language)
    {
        info.Language = language;
    }

    // OFFSET 0x02025FE0
    public static void TrainerInfo_SetMainStoryCleared(TrainerInfo info)
    {
        info.Flags = (byte)((info.Flags & ~MainStoryClearedFlag) | MainStoryClearedFlag);
    }

    // OFFSET 0x02025FF0
    public static bool TrainerInfo_IsMainStoryCleared(TrainerInfo info)
    {
        return (info.Flags & MainStoryClearedFlag) != 0;
    }

    // OFFSET 0x02025FF8
    public static void TrainerInfo_GiveNationalDex(TrainerInfo info)
    {
        info.Flags |= NationalDexFlag;
    }

    // OFFSET 0x02026004
    public static bool TrainerInfo_HasNationalDex(TrainerInfo info)
    {
        return (info.Flags & NationalDexFlag) != 0;
    }

    // OFFSET 0x0202600C
    public static bool TrainerInfo_Equals(TrainerInfo first, TrainerInfo second)
    {
        if (CharCode.CharCode_CompareNumChars(first.Name, 0, second.Name, 0, ComparedNameLength) == 0)
        {
            if (first.ID == second.ID)
            {
                return true;
            }
        }
        return false;
    }
}

public sealed class PlayerSave : ISaveBlock
{
    private const int TrainerInfoOffset = 4;
    private const int CoinsOffset = 0x24;
    private const int PlayTimeOffset = 0x26;

    public readonly Options Options = new();
    public readonly TrainerInfo Info = new();
    public readonly Coins Coins = new();
    public readonly PlayTime PlayTime = new();

    public void WriteTo(byte[] body, int offset)
    {
        Bin.W16(body, offset, Options.Flags);
        Info.WriteTo(body, offset + TrainerInfoOffset);
        Bin.W16(body, offset + CoinsOffset, Coins.Value);
        Bin.W16(body, offset + PlayTimeOffset, PlayTime.Hours);
        body[offset + PlayTimeOffset + 2] = PlayTime.Minutes;
        body[offset + PlayTimeOffset + 3] = PlayTime.Seconds;
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Options.Flags = (ushort)Bin.U16(body, offset);
        Info.ReadFrom(body, offset + TrainerInfoOffset);
        Coins.Value = (ushort)Bin.U16(body, offset + CoinsOffset);
        PlayTime.Hours = (ushort)Bin.U16(body, offset + PlayTimeOffset);
        PlayTime.Minutes = body[offset + PlayTimeOffset + 2];
        PlayTime.Seconds = body[offset + PlayTimeOffset + 3];
    }

    // OFFSET 0x02025E0C
    public static void Player_Init(PlayerSave player)
    {
        Options.Options_Init(player.Options);
        TrainerInfo.TrainerInfo_Init(player.Info);
        Coins.Coins_Init(player.Coins);
        PlayTime.PlayTime_Init(player.PlayTime);
    }

    // OFFSET 0x02025E38
    public static TrainerInfo SaveData_GetTrainerInfo(SaveData saveData)
    {
        PlayerSave player = (PlayerSave)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryPlayer);
        return player.Info;
    }

    // OFFSET 0x02025E44
    public static Options SaveData_GetOptions(SaveData saveData)
    {
        PlayerSave player = (PlayerSave)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryPlayer);
        return player.Options;
    }

    // OFFSET 0x02025E50
    public static Coins SaveData_GetCoins(SaveData saveData)
    {
        PlayerSave player = (PlayerSave)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryPlayer);
        return player.Coins;
    }

    // OFFSET 0x02025E5C
    public static PlayTime SaveData_GetPlayTime(SaveData saveData)
    {
        PlayerSave player = (PlayerSave)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryPlayer);
        return player.PlayTime;
    }
}
