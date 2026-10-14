namespace PlatinumSharp.Game;

public static class TileBehavior
{
    private const uint FlagsAddress = 0x020ED8C4;
    private const int FlagsCount = 0x100;
    private const int FlagSurfable = 1 << 0;
    private const int FlagEncounter = 1 << 1;
    private const int NullTileBehavior = 0xFF;

    private static byte[]? flags;

    // OFFSET 0x0205DAC8
    public static int TileBehavior_IsTallGrass(int tileBehavior)
    {
        if (tileBehavior == 0x02)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DAD4
    public static int TileBehavior_IsVeryTallGrass(int tileBehavior)
    {
        if (tileBehavior == 0x03)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DAE0
    public static int TileBehavior_IsTable(int tileBehavior)
    {
        if (tileBehavior == 0x80)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DAEC
    public static int TileBehavior_IsDoor(int tileBehavior)
    {
        if (tileBehavior == 0x69)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DAF8
    public static int TileBehavior_IsWarpEntranceEast(int tileBehavior)
    {
        if (tileBehavior == 0x62)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DB04
    public static int TileBehavior_IsWarpEntranceWest(int tileBehavior)
    {
        if (tileBehavior == 0x63)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DB10
    public static int TileBehavior_IsWarpEntranceNorth(int tileBehavior)
    {
        if (tileBehavior == 0x64)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DB1C
    public static int TileBehavior_IsWarpEntranceSouth(int tileBehavior)
    {
        if (tileBehavior == 0x65)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DB28
    public static int TileBehavior_IsWarpEast(int tileBehavior)
    {
        if (tileBehavior == 0x6C)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DB34
    public static int TileBehavior_IsWarpWest(int tileBehavior)
    {
        if (tileBehavior == 0x6D)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DB40
    public static int TileBehavior_IsWarpNorth(int tileBehavior)
    {
        if (tileBehavior == 0x6E)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DB4C
    public static int TileBehavior_IsWarpSouth(int tileBehavior)
    {
        if (tileBehavior == 0x6F)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DB58
    public static int TileBehavior_IsSurfable(int tileBehavior)
    {
        if ((Flags()[tileBehavior] & FlagSurfable) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DB6C
    public static int TileBehavior_IsSand(int tileBehavior)
    {
        if (tileBehavior == 0x21)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DB78
    public static int TileBehavior_IsShallowWater(int tileBehavior)
    {
        if (tileBehavior == 0x17)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DB84
    public static int TileBehavior_IsJumpNorth(int tileBehavior)
    {
        if (tileBehavior == 0x3A)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DB90
    public static int TileBehavior_IsJumpSouth(int tileBehavior)
    {
        if (tileBehavior == 0x3B)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DB9C
    public static int TileBehavior_IsJumpWest(int tileBehavior)
    {
        if (tileBehavior == 0x39)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DBA8
    public static int TileBehavior_IsJumpEast(int tileBehavior)
    {
        if (tileBehavior == 0x38)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DBB4
    public static int TileBehavior_IsJumpNorthTwice(int tileBehavior)
    {
        if (tileBehavior == 0x5A)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DBC0
    public static int TileBehavior_IsJumpSouthTwice(int tileBehavior)
    {
        if (tileBehavior == 0x5B)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DBCC
    public static int TileBehavior_IsJumpWestTwice(int tileBehavior)
    {
        if (tileBehavior == 0x5C)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DBD8
    public static int TileBehavior_IsJumpEastTwice(int tileBehavior)
    {
        if (tileBehavior == 0x5D)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DBE4
    public static int TileBehavior_IsPC(int tileBehavior)
    {
        if (tileBehavior == 0x83)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DBF0
    public static int TileBehavior_IsTownMap(int tileBehavior)
    {
        if (tileBehavior == 0x85)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DBFC
    public static int TileBehavior_IsPastoriaGymHighGround(int tileBehavior)
    {
        if (tileBehavior == 0x56)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DC08
    public static int TileBehavior_IsPastoriaGymMiddleGround(int tileBehavior)
    {
        if (tileBehavior == 0x57)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DC14
    public static int TileBehavior_IsPastoriaGymLowGround(int tileBehavior)
    {
        if (tileBehavior == 0x58)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DC20
    public static int TileBehavior_HasDynamicHeightCollision(int tileBehavior)
    {
        if (tileBehavior == 0x59)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DC2C
    public static int TileBehavior_IsEscalatorFlipFace(int tileBehavior)
    {
        if (tileBehavior == 0x6A)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DC38
    public static int TileBehavior_IsEscalator(int tileBehavior)
    {
        if (tileBehavior == 0x6B)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DC44
    public static int TileBehavior_IsWarpStairsEast(int tileBehavior)
    {
        if (tileBehavior == 0x5E)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DC50
    public static int TileBehavior_IsWarpStairsWest(int tileBehavior)
    {
        if (tileBehavior == 0x5F)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DC5C
    public static int TileBehavior_IsIce(int tileBehavior)
    {
        if (tileBehavior == 0x20)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DC68
    public static int TileBehavior_IsRockClimbNorthSouth(int tileBehavior)
    {
        if (tileBehavior == 0x4B)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DC74
    public static int TileBehavior_IsRockClimbEastWest(int tileBehavior)
    {
        if (tileBehavior == 0x4C)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DC80
    public static int TileBehavior_IsSmallBookshelf1(int tileBehavior)
    {
        if (tileBehavior == 0xE0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DC8C
    public static int TileBehavior_IsSmallBookshelf2(int tileBehavior)
    {
        if (tileBehavior == 0xEA)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DC98
    public static int TileBehavior_IsBookshelf1(int tileBehavior)
    {
        if (tileBehavior == 0xE1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DCA4
    public static int TileBehavior_IsBookshelf2(int tileBehavior)
    {
        if (tileBehavior == 0xE2)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DCB0
    public static int TileBehavior_IsTrashCan(int tileBehavior)
    {
        if (tileBehavior == 0xE4)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DCBC
    public static int TileBehavior_IsMartShelf1(int tileBehavior)
    {
        if (tileBehavior == 0xE5)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DCC8
    public static int TileBehavior_IsMartShelf2(int tileBehavior)
    {
        if (tileBehavior == 0xEB)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DCD4
    public static int TileBehavior_IsMartShelf3(int tileBehavior)
    {
        if (tileBehavior == 0xEC)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DCE0
    public static int TileBehavior_IsMud(int tileBehavior)
    {
        if (tileBehavior == 0xA4 || tileBehavior == 0xA5)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DCF0
    public static int TileBehavior_IsDeepMud(int tileBehavior)
    {
        if (tileBehavior == 0xA5)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DCFC
    public static int TileBehavior_IsMudWithGrass(int tileBehavior)
    {
        if (tileBehavior == 0xA6 || tileBehavior == 0xA7)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DD0C
    public static int TileBehavior_IsDeepMudWithGrass(int tileBehavior)
    {
        if (tileBehavior == 0xA7)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DD18
    public static int TileBehavior_IsSnow(int tileBehavior)
    {
        switch ((byte)tileBehavior)
        {
            case 0xA1:
            case 0xA2:
            case 0xA3:
            case 0xA8:
                return 1;
            default:
                return 0;
        }
    }

    // OFFSET 0x0205DD38
    public static int TileBehavior_IsShallowSnow(int tileBehavior)
    {
        if (tileBehavior == 0xA8)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DD44
    public static int TileBehavior_IsDeepSnow(int tileBehavior)
    {
        if (tileBehavior == 0xA1)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DD50
    public static int TileBehavior_IsDeeperSnow(int tileBehavior)
    {
        if (tileBehavior == 0xA2)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DD5C
    public static int TileBehavior_IsDeepestSnow(int tileBehavior)
    {
        if (tileBehavior == 0xA3)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DD68
    public static int TileBehavior_IsBikeSlope(int tileBehavior)
    {
        if (tileBehavior == 0xD9 || tileBehavior == 0xDA)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DD78
    public static int TileBehavior_IsBikeSlopeTop(int tileBehavior)
    {
        if (tileBehavior == 0xD9)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DD84
    public static int TileBehavior_IsBikeSlopeBottom(int tileBehavior)
    {
        if (tileBehavior == 0xDA)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DD90
    public static int TileBehavior_IsBikeRampEastward(int tileBehavior)
    {
        if (tileBehavior == 0xD7)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DD9C
    public static int TileBehavior_IsBikeRampWestward(int tileBehavior)
    {
        if (tileBehavior == 0xD8)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DDA8
    public static int TileBehavior_IsCaveFloor(int tileBehavior)
    {
        if (tileBehavior == 0x08)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DDB4
    public static int TileBehavior_IsWaterfall(int tileBehavior)
    {
        if (tileBehavior == 0x13)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DDC0
    public static int TileBehavior_IsBikeParking(int tileBehavior)
    {
        if (tileBehavior == 0xDB)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DDCC
    public static int TileBehavior_BlocksMovementNorthward(int tileBehavior)
    {
        switch ((byte)tileBehavior)
        {
            case 0x32:
            case 0x34:
            case 0x35:
            case 0x49:
                return 1;
            default:
                return 0;
        }
    }

    // OFFSET 0x0205DDF0
    public static int TileBehavior_BlocksMovementSouthward(int tileBehavior)
    {
        switch ((byte)tileBehavior)
        {
            case 0x33:
            case 0x36:
            case 0x37:
            case 0x49:
                return 1;
            default:
                return 0;
        }
    }

    // OFFSET 0x0205DE14
    public static int TileBehavior_BlocksMovementWestward(int tileBehavior)
    {
        switch ((byte)tileBehavior)
        {
            case 0x31:
            case 0x35:
            case 0x37:
            case 0x4A:
                return 1;
            default:
                return 0;
        }
    }

    // OFFSET 0x0205DE38
    public static int TileBehavior_BlocksMovementEastward(int tileBehavior)
    {
        switch ((byte)tileBehavior)
        {
            case 0x30:
            case 0x34:
            case 0x36:
            case 0x4A:
                return 1;
            default:
                return 0;
        }
    }

    // OFFSET 0x0205DE5C
    public static int TileBehavior_IsPuddle(int tileBehavior)
    {
        if (tileBehavior == 0x16 || tileBehavior == 0x1D)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DE6C
    public static int TileBehavior_HasEncounters(int tileBehavior)
    {
        if ((Flags()[tileBehavior] & FlagEncounter) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DE84
    public static int TileBehavior_IsTV(int tileBehavior)
    {
        if (tileBehavior == 0x86)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DE90
    public static int TileBehavior_HasReflectiveSurface(int tileBehavior)
    {
        switch ((byte)tileBehavior)
        {
            case 0x10:
            case 0x16:
            case 0x1D:
            case 0x2C:
                return 1;
            default:
                return 0;
        }
    }

    // OFFSET 0x0205DEB4
    public static int TileBehavior_IsSlideEastward(int tileBehavior)
    {
        if (tileBehavior == 0x40)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DEC0
    public static int TileBehavior_IsSlideWestward(int tileBehavior)
    {
        if (tileBehavior == 0x41)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DECC
    public static int TileBehavior_IsSlideNorthward(int tileBehavior)
    {
        if (tileBehavior == 0x42)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DED8
    public static int TileBehavior_IsSlideSouthward(int tileBehavior)
    {
        if (tileBehavior == 0x43)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DEE4
    public static int TileBehavior_IsWarpPanel(int tileBehavior)
    {
        if (tileBehavior == 0x67)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DEF0
    public static int TileBehavior_IsBridgeStart(int tileBehavior)
    {
        if (tileBehavior == 0x70)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DEFC
    public static int TileBehavior_IsBridge(int tileBehavior)
    {
        if ((byte)(tileBehavior - 0x71) <= 0xC)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DF10
    public static int TileBehavior_IsBridgeOverWater(int tileBehavior)
    {
        switch ((byte)tileBehavior)
        {
            case 0x73:
            case 0x78:
            case 0x7C:
                return 1;
            default:
                return 0;
        }
    }

    // OFFSET 0x0205DF34
    public static int TileBehavior_IsBridgeOverSand(int tileBehavior)
    {
        switch ((byte)tileBehavior)
        {
            case 0x74:
            case 0x79:
            case 0x7D:
                return 1;
            default:
                return 0;
        }
    }

    // OFFSET 0x0205DF58
    public static int TileBehavior_IsBridgeOverSnow(int tileBehavior)
    {
        if (tileBehavior == 0x75)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DF64
    public static int TileBehavior_IsBikeBridgeNorthSouth(int tileBehavior)
    {
        if ((byte)(tileBehavior - 0x76) <= 0x3)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DF78
    public static int TileBehavior_IsBikeBridgeEastWest(int tileBehavior)
    {
        if ((byte)(tileBehavior - 0x7A) <= 0x3)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DF8C
    public static int TileBehavior_IsNull(int tileBehavior)
    {
        if (tileBehavior == 0xFF)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DF98
    public static int TileBehavior_GetNull()
    {
        return NullTileBehavior;
    }

    // OFFSET 0x0205DF9C
    public static int TileBehavior_IsReflective(int tileBehavior)
    {
        if (tileBehavior == 0x1D || tileBehavior == 0x2C)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DFAC
    public static int TileBehavior_IsSnowWithShadows(int tileBehavior)
    {
        if (tileBehavior == 0xA9)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0205DFB8
    public static int TileBehavior_ForbidsExplorationKit(int tileBehavior)
    {
        if (tileBehavior == 0x2D)
        {
            return 1;
        }
        return 0;
    }

    private static byte[] Flags()
    {
        flags ??= RomData.Arm9(FlagsAddress, FlagsCount);
        return flags;
    }
}
