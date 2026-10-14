using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class Dwc
{
    public const int UserDataSize = 0x40;

    private const int SizeOffset = 0;
    private const int PseudoIDOffset = 4;
    private const int FlagOffset = 0x20;
    private const int GameCodeOffset = 0x24;
    private const int ChecksumOffset = 0x3C;
    private const uint DirtyFlag = 1;
    private const uint Polynomial = 0xEDB88320;

    private static uint Crc32(byte[] data, int length)
    {
        uint crc = 0xFFFFFFFF;
        for (int i = 0; i < length; i++)
        {
            crc ^= data[i];
            for (int bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ Polynomial : crc >> 1;
            }
        }
        return ~crc;
    }

    public static int InitResult { get; set; }

    public static int Init()
    {
        return InitResult;
    }

    public static bool CheckUserData(byte[] userData)
    {
        return Crc32(userData, ChecksumOffset) == Bin.U32(userData, ChecksumOffset);
    }

    public static void CreateUserData(byte[] userData, uint gameCode)
    {
        Array.Clear(userData, 0, UserDataSize);
        Bin.W32(userData, SizeOffset, UserDataSize);
        byte[] entropy = Platform.GetLowEntropyData();
        Array.Copy(entropy, 0, userData, PseudoIDOffset, 12);
        Bin.W32(userData, FlagOffset, DirtyFlag);
        Bin.W32(userData, GameCodeOffset, gameCode);
        Bin.W32(userData, ChecksumOffset, Crc32(userData, ChecksumOffset));
    }

    public static void ClearDirtyFlag(byte[] userData)
    {
        Bin.W32(userData, FlagOffset, Bin.U32(userData, FlagOffset) & ~DirtyFlag);
        Bin.W32(userData, ChecksumOffset, Crc32(userData, ChecksumOffset));
    }

    public static bool IsValidFriendData(byte[] friendData)
    {
        return (Bin.U32(friendData, 0) & 3) != 0;
    }
}
