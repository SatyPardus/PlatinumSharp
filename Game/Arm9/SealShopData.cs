namespace PlatinumSharp.Game;

public static class SealShopData
{
    private const uint SealShopTable = 0x020F64D0;
    private const int SealShopEntrySize = 0xA;
    private const int SealShopEntryCount = 0x32A;

    private static uint Entry(int index)
    {
        if (index >= SealShopEntryCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return (uint)(SealShopTable + index * SealShopEntrySize);
    }

    // OFFSET 0x02098164
    public static int SealShopEntry_GetSealID(int index)
    {
        return RomData.Arm9(Entry(index) + 2, 1)[0];
    }

    // OFFSET 0x020981D0
    public static int SealShopEntry_GetPrice(int index)
    {
        return RomData.Arm9U16(Entry(index) + 6, 1)[0];
    }

    // OFFSET 0x020981F4
    public static int SealShopEntry_GetDescriptionID(int index)
    {
        return RomData.Arm9(Entry(index) + 8, 1)[0];
    }
}
