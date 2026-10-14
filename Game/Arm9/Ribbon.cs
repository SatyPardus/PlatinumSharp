namespace PlatinumSharp.Game;

public static class Ribbon
{
    private const uint RibbonData = 0x020F51AC;
    private const int RibbonDataSize = 0xC;

    public const int ParamMonDataParam = 0;
    public const int ParamGfxID = 1;
    public const int ParamPaletteNum = 2;
    public const int ParamNameID = 3;
    public const int ParamDescriptionID = 4;

    // OFFSET 0x020923C0
    public static uint Ribbon_GetData(int ribbonID, int param)
    {
        uint entry = (uint)(RibbonData + ribbonID * RibbonDataSize);
        switch (param)
        {
            case ParamMonDataParam:
                return RomData.Arm9U32(entry, 1)[0];
            case ParamGfxID:
                return RomData.Arm9U16(entry + 4, 1)[0];
            case ParamPaletteNum:
                return RomData.Arm9U16(entry + 6, 1)[0];
            case ParamNameID:
                return RomData.Arm9U16(entry + 8, 1)[0];
            case ParamDescriptionID:
                return RomData.Arm9U16(entry + 0xA, 1)[0];
        }
        return 0;
    }

    private const ushort SpecialDescriptionFlag = 0x8000;
    private const int SpecialDescriptionBase = 0x92;
    private const int NumRibbons = 0x50;

    // OFFSET 0x02092424
    public static uint Ribbon_GetDescriptionID(byte[] specialRibbons, int ribbonID)
    {
        ushort description = RomData.Arm9U16((uint)(RibbonData + ribbonID * RibbonDataSize + 0xA), 1)[0];
        if ((description & SpecialDescriptionFlag) != 0)
        {
            return (uint)(specialRibbons[description & 0xFF] + SpecialDescriptionBase);
        }
        return description;
    }

    // OFFSET 0x02092444
    public static byte Ribbon_TryGetSpecialDescriptionID(int ribbonID)
    {
        ushort description = RomData.Arm9U16((uint)(RibbonData + ribbonID * RibbonDataSize + 0xA), 1)[0];
        if ((description & SpecialDescriptionFlag) != 0)
        {
            return (byte)description;
        }
        return 0xFF;
    }

    // OFFSET 0x02092464
    public static ushort Ribbon_MonDataParamToNameID(uint monDataParam)
    {
        for (int i = 0; i < NumRibbons; i++)
        {
            uint entry = (uint)(RibbonData + i * RibbonDataSize);
            if (RomData.Arm9U32(entry, 1)[0] == monDataParam)
            {
                return RomData.Arm9U16(entry + 8, 1)[0];
            }
        }
        return 0xFFFF;
    }
}
