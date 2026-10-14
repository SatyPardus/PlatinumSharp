namespace PlatinumSharp.Game;

public static partial class TVSegments
{
    // OFFSET 0x0206D90C
    public static void FieldSystem_SaveTVSegment_HerbalMedicineTrainerSightingDummy(object? broadcast, Pokemon mon, ushort item)
    {
    }

    private const int ProgramTypeSightings = 2;
    private const int SegmentBoughtItems = 7;
    private const int BoughtItemsMinimum = 10;
    private const int BoughtItemsMaximum = 999;
    private const byte BoughtItemsClamp = 0xE7;

    // OFFSET 0x0206D504
    public static void TVSegment_SaveBoughtItems(SaveData saveData, ushort item, byte count)
    {
        byte[] data = new byte[TVSegmentInstance.DataSize];
        if (count < BoughtItemsMinimum)
        {
            return;
        }
        Nitro.Bin.W16(data, 0, item);
        data[2] = count;
        if (data[2] > BoughtItemsMaximum)
        {
            data[2] = BoughtItemsClamp;
        }
        TVBroadcast.TVBroadcast_SaveSegmentData((TVBroadcast)SaveDataTable.SaveData_GetTVBroadcast(saveData), ProgramTypeSightings, SegmentBoughtItems, data);
    }
}
