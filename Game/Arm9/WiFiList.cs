using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class WiFiFriend
{
    public const int Size = 0x38;
    public const int NameLength = 8;
    public const byte GenderNone = 2;

    private const int FriendNameOffset = 0x10;
    private const ushort EndOfString = 0xFFFF;

    public readonly ushort[] GroupName = new ushort[NameLength];
    public readonly ushort[] FriendName = new ushort[NameLength];
    public uint TrainerID;
    public ushort BattleWins;
    public ushort BattleLosses;
    public ushort Trades;
    public ushort Year;
    public byte Month;
    public byte Day;
    public byte Gender;
    public byte TrainerType;
    public ushort ExtraCount0;
    public ushort ExtraCount1;
    public ushort ExtraCount2;
    public ushort ExtraCount3;

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < NameLength; i++)
        {
            Bin.W16(body, offset + i * 2, GroupName[i]);
            Bin.W16(body, offset + FriendNameOffset + i * 2, FriendName[i]);
        }
        Bin.W32(body, offset + 0x20, TrainerID);
        Bin.W16(body, offset + 0x24, BattleWins);
        Bin.W16(body, offset + 0x26, BattleLosses);
        Bin.W16(body, offset + 0x28, Trades);
        Bin.W16(body, offset + 0x2A, Year);
        body[offset + 0x2C] = Month;
        body[offset + 0x2D] = Day;
        body[offset + 0x2E] = Gender;
        body[offset + 0x2F] = TrainerType;
        Bin.W16(body, offset + 0x30, ExtraCount0);
        Bin.W16(body, offset + 0x32, ExtraCount1);
        Bin.W16(body, offset + 0x34, ExtraCount2);
        Bin.W16(body, offset + 0x36, ExtraCount3);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < NameLength; i++)
        {
            GroupName[i] = (ushort)Bin.U16(body, offset + i * 2);
            FriendName[i] = (ushort)Bin.U16(body, offset + FriendNameOffset + i * 2);
        }
        TrainerID = Bin.U32(body, offset + 0x20);
        BattleWins = (ushort)Bin.U16(body, offset + 0x24);
        BattleLosses = (ushort)Bin.U16(body, offset + 0x26);
        Trades = (ushort)Bin.U16(body, offset + 0x28);
        Year = (ushort)Bin.U16(body, offset + 0x2A);
        Month = body[offset + 0x2C];
        Day = body[offset + 0x2D];
        Gender = body[offset + 0x2E];
        TrainerType = body[offset + 0x2F];
        ExtraCount0 = (ushort)Bin.U16(body, offset + 0x30);
        ExtraCount1 = (ushort)Bin.U16(body, offset + 0x32);
        ExtraCount2 = (ushort)Bin.U16(body, offset + 0x34);
        ExtraCount3 = (ushort)Bin.U16(body, offset + 0x36);
    }

    public void CopyFrom(WiFiFriend source)
    {
        byte[] buffer = new byte[Size];
        source.WriteTo(buffer, 0);
        ReadFrom(buffer, 0);
    }

    public void Reset()
    {
        ReadFrom(new byte[Size], 0);
        FriendName[0] = EndOfString;
        GroupName[0] = EndOfString;
        Gender = GenderNone;
    }
}

public sealed class WiFiList : ISaveBlock
{
    public const int Size = 0x8C0;
    public const int MaxFriends = 32;
    public const int UserDataSize = 0x40;
    public const int FriendKeySize = 0xC;
    public const int FriendDataTrainerID = 0;
    public const int FriendDataBattleWins = 1;
    public const int FriendDataBattleLosses = 2;
    public const int FriendDataTrades = 3;
    public const int FriendDataYear = 4;
    public const int FriendDataMonth = 5;
    public const int FriendDataDay = 6;
    public const int FriendDataTrainerType = 7;
    public const int FriendDataGender = 8;
    public const int FriendDataExtraCount0 = 9;
    public const int FriendDataExtraCount1 = 10;
    public const int FriendDataExtraCount2 = 11;
    public const int FriendDataExtraCount3 = 12;

    private const int FriendKeysOffset = 0x40;
    private const int FriendsOffset = 0x1C0;
    private const int NameBufferSize = 0x10;
    private const int MaxCount = 9999;
    private const int BaseYear = 2000;
    private const int NoIndex = -1;

    public readonly byte[] UserData = new byte[UserDataSize];
    public readonly byte[][] FriendKeys = new byte[MaxFriends][];
    public readonly WiFiFriend[] Friends = new WiFiFriend[MaxFriends];

    public WiFiList()
    {
        for (int i = 0; i < MaxFriends; i++)
        {
            FriendKeys[i] = new byte[FriendKeySize];
            Friends[i] = new WiFiFriend();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        Array.Copy(UserData, 0, body, offset, UserDataSize);
        for (int i = 0; i < MaxFriends; i++)
        {
            Array.Copy(FriendKeys[i], 0, body, offset + FriendKeysOffset + i * FriendKeySize, FriendKeySize);
            Friends[i].WriteTo(body, offset + FriendsOffset + i * WiFiFriend.Size);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        Array.Copy(body, offset, UserData, 0, UserDataSize);
        for (int i = 0; i < MaxFriends; i++)
        {
            Array.Copy(body, offset + FriendKeysOffset + i * FriendKeySize, FriendKeys[i], 0, FriendKeySize);
            Friends[i].ReadFrom(body, offset + FriendsOffset + i * WiFiFriend.Size);
        }
    }

    private static ushort AddClamped(ushort value, int amount)
    {
        ushort total = (ushort)(value + amount);
        if (total > MaxCount)
        {
            total = MaxCount;
        }
        return total;
    }

    // OFFSET 0x0202ACE8
    public static void WiFiList_Init(WiFiList wiFiList)
    {
        Array.Clear(wiFiList.UserData);
        for (int i = 0; i < MaxFriends; i++)
        {
            Array.Clear(wiFiList.FriendKeys[i]);
            wiFiList.Friends[i].Reset();
        }
        WiFiUserData.WiFiList_InitUserData(wiFiList);
    }

    // OFFSET 0x0202AD28
    public static byte[] WiFiList_GetUserData(WiFiList wiFiList)
    {
        return wiFiList.UserData;
    }

    // OFFSET 0x0202AD2C
    public static uint WiFiList_GetFriendData(WiFiList wiFiList, int index, int field)
    {
        uint value = 0;
        if (index >= MaxFriends)
        {
            ErrorHandling.GF_AssertFail();
        }
        WiFiFriend friend = wiFiList.Friends[index];
        switch (field)
        {
            case FriendDataTrainerID:
                value = friend.TrainerID;
                break;
            case FriendDataBattleWins:
                value = friend.BattleWins;
                break;
            case FriendDataBattleLosses:
                value = friend.BattleLosses;
                break;
            case FriendDataTrades:
                value = friend.Trades;
                break;
            case FriendDataYear:
                value = friend.Year;
                break;
            case FriendDataMonth:
                value = friend.Month;
                break;
            case FriendDataDay:
                value = friend.Day;
                break;
            case FriendDataTrainerType:
                value = friend.TrainerType;
                break;
            case FriendDataGender:
                value = friend.Gender;
                break;
            case FriendDataExtraCount0:
                value = friend.ExtraCount0;
                break;
            case FriendDataExtraCount1:
                value = friend.ExtraCount1;
                break;
            case FriendDataExtraCount2:
                value = friend.ExtraCount2;
                break;
            case FriendDataExtraCount3:
                value = friend.ExtraCount3;
                break;
        }
        return value;
    }

    // OFFSET 0x0202AE2C
    public static void WiFiList_SetFriendData(WiFiList wiFiList, int index, int field, uint value)
    {
        if (index >= MaxFriends)
        {
            ErrorHandling.GF_AssertFail();
        }
        WiFiFriend friend = wiFiList.Friends[index];
        switch (field)
        {
            case FriendDataTrainerID:
                friend.TrainerID = value;
                break;
            case FriendDataBattleWins:
            case FriendDataBattleLosses:
            case FriendDataTrades:
            case FriendDataExtraCount0:
                ErrorHandling.GF_AssertFail();
                break;
            case FriendDataYear:
                friend.Year = (ushort)value;
                break;
            case FriendDataMonth:
                friend.Month = (byte)value;
                break;
            case FriendDataDay:
                friend.Day = (byte)value;
                break;
            case FriendDataTrainerType:
                friend.TrainerType = (byte)value;
                break;
            case FriendDataGender:
                friend.Gender = (byte)value;
                break;
        }
    }

    // OFFSET 0x0202AED8
    public static byte[] WiFiList_GetFriendKey(WiFiList wiFiList, int index)
    {
        if (index >= MaxFriends)
        {
            ErrorHandling.GF_AssertFail();
        }
        return wiFiList.FriendKeys[index];
    }

    // OFFSET 0x0202AEF0
    public static ushort[] WiFiList_GetFriendName(WiFiList wiFiList, int index)
    {
        if (index >= MaxFriends)
        {
            ErrorHandling.GF_AssertFail();
        }
        return wiFiList.Friends[index].FriendName;
    }

    // OFFSET 0x0202AF0C
    public static void WiFiList_SetFriendName(WiFiList wiFiList, int index, GameString name)
    {
        if (index >= MaxFriends)
        {
            ErrorHandling.GF_AssertFail();
        }
        GameString.String_ToChars(name, wiFiList.Friends[index].FriendName, NameBufferSize);
    }

    // OFFSET 0x0202AF34
    public static ushort[] WiFiList_GetGroupName(WiFiList wiFiList, int index)
    {
        if (index >= MaxFriends)
        {
            ErrorHandling.GF_AssertFail();
        }
        return wiFiList.Friends[index].GroupName;
    }

    // OFFSET 0x0202AF50
    public static void WiFiList_SetGroupName(WiFiList wiFiList, int index, GameString name)
    {
        if (index >= MaxFriends)
        {
            ErrorHandling.GF_AssertFail();
        }
        GameString.String_ToChars(name, wiFiList.Friends[index].GroupName, NameBufferSize);
    }

    // OFFSET 0x0202AF78
    public static bool WiFiList_IsValidFriendData(WiFiList wiFiList, int index)
    {
        if (index >= MaxFriends)
        {
            ErrorHandling.GF_AssertFail();
        }
        return Dwc.IsValidFriendData(wiFiList.FriendKeys[index]);
    }

    // OFFSET 0x0202AF94
    public static int WiFiList_GetValidFriendsCount(WiFiList wiFiList)
    {
        int count = 0;
        for (int i = 0; i < MaxFriends; i++)
        {
            if (WiFiList_IsValidFriendData(wiFiList, i))
            {
                count++;
            }
        }
        return count;
    }

    // OFFSET 0x0202AFB4
    public static int WiFiList_GetLastValidFriendEnd(WiFiList wiFiList)
    {
        int end = 0;
        for (int i = 0; i < MaxFriends; i++)
        {
            if (WiFiList_IsValidFriendData(wiFiList, i))
            {
                end = i + 1;
            }
        }
        return end;
    }

    // OFFSET 0x0202AFD4
    public static void WiFiList_DeleteFriend(WiFiList wiFiList, int index)
    {
        if (index >= MaxFriends)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = index; i < MaxFriends - 1; i++)
        {
            wiFiList.Friends[i].CopyFrom(wiFiList.Friends[i + 1]);
            Array.Copy(wiFiList.FriendKeys[i + 1], wiFiList.FriendKeys[i], FriendKeySize);
        }
        Array.Clear(wiFiList.FriendKeys[MaxFriends - 1]);
        wiFiList.Friends[MaxFriends - 1].Reset();
    }

    // OFFSET 0x0202B088
    private static void WiFiList_MoveFriend(WiFiList wiFiList, int destination, int source)
    {
        wiFiList.Friends[destination].CopyFrom(wiFiList.Friends[source]);
        Array.Copy(wiFiList.FriendKeys[source], wiFiList.FriendKeys[destination], FriendKeySize);
        Array.Clear(wiFiList.FriendKeys[source]);
        wiFiList.Friends[source].Reset();
    }

    // OFFSET 0x0202B0F8
    public static void WiFiList_Compact(WiFiList wiFiList)
    {
        int empty = NoIndex;
        for (int i = 0; i < MaxFriends; i++)
        {
            if (WiFiList_IsValidFriendData(wiFiList, i))
            {
                if (empty != NoIndex)
                {
                    WiFiList_MoveFriend(wiFiList, empty, i);
                    i = NoIndex;
                    empty = NoIndex;
                }
            }
            else if (empty == NoIndex)
            {
                empty = i;
            }
        }
    }

    // OFFSET 0x0202B13C
    public static void WiFiList_SetHostFriendCurrentDate(WiFiList wiFiList, int index)
    {
        Rtc.GetCurrentDate(out RtcDate date);
        WiFiFriend friend = wiFiList.Friends[index];
        friend.Year = (ushort)(date.Year + BaseYear);
        friend.Month = (byte)date.Month;
        friend.Day = (byte)date.Day;
    }

    // OFFSET 0x0202B174
    public static void WiFiList_AddBattleAndTradeCounts(WiFiList wiFiList, int index, int wins, int losses, int trades)
    {
        WiFiFriend friend = wiFiList.Friends[index];
        friend.BattleWins = AddClamped(friend.BattleWins, wins);
        friend.BattleLosses = AddClamped(friend.BattleLosses, losses);
        friend.Trades = AddClamped(friend.Trades, trades);
        WiFiList_SetHostFriendCurrentDate(wiFiList, index);
    }

    // OFFSET 0x0202B1D0
    public static void WiFiList_AddExtraCount0(WiFiList wiFiList, int index, int amount)
    {
        WiFiFriend friend = wiFiList.Friends[index];
        friend.ExtraCount0 = AddClamped(friend.ExtraCount0, amount);
        WiFiList_SetHostFriendCurrentDate(wiFiList, index);
    }

    // OFFSET 0x0202B1F8
    public static void WiFiList_AddExtraCount1(WiFiList wiFiList, int index, int amount)
    {
        WiFiFriend friend = wiFiList.Friends[index];
        friend.ExtraCount1 = AddClamped(friend.ExtraCount1, amount);
        WiFiList_SetHostFriendCurrentDate(wiFiList, index);
    }

    // OFFSET 0x0202B220
    public static void WiFiList_AddExtraCount2(WiFiList wiFiList, int index, int amount)
    {
        WiFiFriend friend = wiFiList.Friends[index];
        friend.ExtraCount2 = AddClamped(friend.ExtraCount2, amount);
        WiFiList_SetHostFriendCurrentDate(wiFiList, index);
    }

    // OFFSET 0x0202B248
    public static void WiFiList_AddExtraCount3(WiFiList wiFiList, int index, int amount)
    {
        WiFiFriend friend = wiFiList.Friends[index];
        friend.ExtraCount3 = AddClamped(friend.ExtraCount3, amount);
        WiFiList_SetHostFriendCurrentDate(wiFiList, index);
    }

    // OFFSET 0x0202B270
    public static void WiFiList_MergeFriend(WiFiList wiFiList, int source, int destination)
    {
        WiFiFriend from = wiFiList.Friends[source];
        WiFiFriend to = wiFiList.Friends[destination];
        to.BattleWins = AddClamped(to.BattleWins, from.BattleWins);
        to.BattleLosses = AddClamped(to.BattleLosses, from.BattleLosses);
        to.Trades = AddClamped(to.Trades, from.Trades);
        to.ExtraCount0 = AddClamped(to.ExtraCount0, from.ExtraCount0);
        to.ExtraCount1 = AddClamped(to.ExtraCount1, from.ExtraCount1);
        to.ExtraCount2 = AddClamped(to.ExtraCount2, from.ExtraCount2);
        to.ExtraCount3 = AddClamped(to.ExtraCount3, from.ExtraCount3);
        Array.Copy(from.GroupName, to.GroupName, WiFiFriend.NameLength);
        from.Reset();
    }

    // OFFSET 0x0202B370
    public static WiFiList SaveData_GetWiFiList(SaveData saveData)
    {
        return (WiFiList)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryWiFiList);
    }
}
