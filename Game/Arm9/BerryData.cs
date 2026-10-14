using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public static class BerryData
{
    private const int BerryArchive = 0x44;
    private const int AttributeCount = 11;
    private const int NarcMessageBanks = 0x1A;
    private const int MessageBankBerryNames = 0x1A8;
    private const int MessageBankBerryDescriptions = 0x1A7;

    // OFFSET 0x0209739C
    public static object BerryData_OpenArchive(int heapID)
    {
        return NARC.NARC_ctor(BerryArchive, heapID);
    }

    // OFFSET 0x020973A8
    public static object BerryData_LoadFromOpenArchive(object berryArchive, int index, int heapID)
    {
        return NARC.NARC_AllocAndReadWholeMember((NARC)berryArchive, (uint)index, heapID);
    }

    // OFFSET 0x020973B0
    public static void BerryData_CloseArchive(object berryArchive)
    {
        NARC.NARC_dtor((NARC)berryArchive);
    }

    // OFFSET 0x020973B8
    public static object BerryData_Load(int index, int heapID)
    {
        return NARC.NARC_AllocAndReadWholeMemberByIndexPair(BerryArchive, index, heapID);
    }

    // OFFSET 0x020973D4
    public static int BerryData_GetAttribute(object berryData, int attribute)
    {
        byte[] data = (byte[])berryData;
        if ((uint)attribute >= AttributeCount)
        {
            return 0;
        }
        if (attribute == 0)
        {
            return Bin.U16(data, 0);
        }
        return data[attribute + 1];
    }

    // OFFSET 0x0209742C
    public static GameString? BerryData_AllocAndGetName(ushort berryIndex, int heapID)
    {
        MessageLoader? loader = MessageLoader.MessageLoader_Init(1, NarcMessageBanks, MessageBankBerryNames, heapID);
        GameString? name = MessageLoader.MessageLoader_GetNewString(loader!, berryIndex);
        MessageLoader.MessageLoader_Free(loader);
        return name;
    }

    // OFFSET 0x02097454
    public static GameString? BerryData_AllocAndGetDescription(ushort berryIndex, int heapID)
    {
        MessageLoader? loader = MessageLoader.MessageLoader_Init(1, NarcMessageBanks, MessageBankBerryDescriptions, heapID);
        GameString? description = MessageLoader.MessageLoader_GetNewString(loader!, berryIndex);
        MessageLoader.MessageLoader_Free(loader);
        return description;
    }
}
