using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class SpecialMetLoc
{
    private const int TypeCount = 3;
    private const int LastRegularLocation = 0x6F;
    private const int FirstSpecialLocation = 2000;
    private const int LastSpecialLocation = 2010;
    private const int FirstEventLocation = 3000;
    private const int LastEventLocation = 3076;

    private static ushort[]? sTypeBases;

    private static ushort[] TypeBases => sTypeBases ??= RomData.Arm9U16(0x020E565C, TypeCount);

    // OFFSET 0x02017038
    public static int SpecialMetLoc_GetType(uint id)
    {
        int type;
        for (type = 0; type < TypeCount - 1; type++)
        {
            if (id < TypeBases[type + 1])
            {
                return type;
            }
        }
        return type;
    }

    // OFFSET 0x02017058
    public static int SpecialMetLoc_GetIndex(uint id)
    {
        int type = SpecialMetLoc_GetType(id);
        return (int)(id - TypeBases[type]);
    }

    // OFFSET 0x02017070
    public static int SpecialMetLoc_GetId(int type, int index)
    {
        if (type >= TypeCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return index + TypeBases[type];
    }

    // OFFSET 0x0201708C
    public static bool SpecialMetLoc_IsValid(ushort id)
    {
        if (id >= 1 && id <= LastRegularLocation)
        {
            return true;
        }
        if (id >= FirstSpecialLocation && id <= LastSpecialLocation)
        {
            return true;
        }
        if (id >= FirstEventLocation && id <= LastEventLocation)
        {
            return true;
        }
        return false;
    }
}
