namespace PlatinumSharp.Game;

public static partial class MovementActions
{
    // OFFSET 0x02065950
    private static int MovementAction_FaceNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitFace(mapObject, 0);
        return 1;
    }

    // OFFSET 0x0206595C
    private static int MovementAction_FaceSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitFace(mapObject, 1);
        return 1;
    }

    // OFFSET 0x02065968
    private static int MovementAction_FaceWest_Step0(MapObject mapObject)
    {
        MovementAction_InitFace(mapObject, 2);
        return 1;
    }

    // OFFSET 0x02065974
    private static int MovementAction_FaceEast_Step0(MapObject mapObject)
    {
        MovementAction_InitFace(mapObject, 3);
        return 1;
    }

    // OFFSET 0x02065A1C
    private static int MovementAction_WalkSlowerNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 0, 0x800, 0x20, 1);
        return 1;
    }

    // OFFSET 0x02065A30
    private static int MovementAction_WalkSlowerSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 1, 0x800, 0x20, 1);
        return 1;
    }

    // OFFSET 0x02065A44
    private static int MovementAction_WalkSlowerWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 2, 0x800, 0x20, 1);
        return 1;
    }

    // OFFSET 0x02065A58
    private static int MovementAction_WalkSlowerEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 3, 0x800, 0x20, 1);
        return 1;
    }

    // OFFSET 0x02065A6C
    private static int MovementAction_WalkSlowNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 0, 0x1000, 0x10, 2);
        return 1;
    }

    // OFFSET 0x02065A80
    private static int MovementAction_WalkSlowSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 1, 0x1000, 0x10, 2);
        return 1;
    }

    // OFFSET 0x02065A94
    private static int MovementAction_WalkSlowWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 2, 0x1000, 0x10, 2);
        return 1;
    }

    // OFFSET 0x02065AA8
    private static int MovementAction_WalkSlowEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 3, 0x1000, 0x10, 2);
        return 1;
    }

    // OFFSET 0x02065ABC
    private static int MovementAction_WalkNormalNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 0, 0x2000, 8, 3);
        return 1;
    }

    // OFFSET 0x02065AD4
    private static int MovementAction_WalkNormalSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 1, 0x2000, 8, 3);
        return 1;
    }

    // OFFSET 0x02065AE8
    private static int MovementAction_WalkNormalWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 2, 0x2000, 8, 3);
        return 1;
    }

    // OFFSET 0x02065AFC
    private static int MovementAction_WalkNormalEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 3, 0x2000, 8, 3);
        return 1;
    }

    // OFFSET 0x02065B10
    private static int MovementAction_WalkFastNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 0, 0x4000, 4, 4);
        return 1;
    }

    // OFFSET 0x02065B24
    private static int MovementAction_WalkFastSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 1, 0x4000, 4, 4);
        return 1;
    }

    // OFFSET 0x02065B38
    private static int MovementAction_WalkFastWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 2, 0x4000, 4, 4);
        return 1;
    }

    // OFFSET 0x02065B4C
    private static int MovementAction_WalkFastEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 3, 0x4000, 4, 4);
        return 1;
    }

    // OFFSET 0x02065B60
    private static int MovementAction_WalkFasterNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 0, 0x8000, 2, 5);
        return 1;
    }

    // OFFSET 0x02065B78
    private static int MovementAction_WalkFasterSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 1, 0x8000, 2, 5);
        return 1;
    }

    // OFFSET 0x02065B8C
    private static int MovementAction_WalkFasterWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 2, 0x8000, 2, 5);
        return 1;
    }

    // OFFSET 0x02065BA0
    private static int MovementAction_WalkFasterEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 3, 0x8000, 2, 5);
        return 1;
    }

    // OFFSET 0x02065BB8
    private static int MovementAction_WalkFastestNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 0, 0x10000, 1, 0);
        return 1;
    }

    // OFFSET 0x02065BCC
    private static int MovementAction_WalkFastestSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 1, 0x10000, 1, 0);
        return 1;
    }

    // OFFSET 0x02065BE0
    private static int MovementAction_WalkFastestWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 2, 0x10000, 1, 0);
        return 1;
    }

    // OFFSET 0x02065BF4
    private static int MovementAction_WalkFastestEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 3, 0x10000, 1, 0);
        return 1;
    }

    // OFFSET 0x02065C0C
    private static int MovementAction_RunNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 0, 0x4000, 4, 9);
        return 1;
    }

    // OFFSET 0x02065C24
    private static int MovementAction_RunSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 1, 0x4000, 4, 9);
        return 1;
    }

    // OFFSET 0x02065C38
    private static int MovementAction_RunWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 2, 0x4000, 4, 9);
        return 1;
    }

    // OFFSET 0x02065C4C
    private static int MovementAction_RunEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalk(mapObject, 3, 0x4000, 4, 9);
        return 1;
    }

    // OFFSET 0x02065CCC
    private static int MovementAction_WalkOnSpotSlowerNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 0, 0x20, 1);
        return 1;
    }

    // OFFSET 0x02065CDC
    private static int MovementAction_WalkOnSpotSlowerSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 1, 0x20, 1);
        return 1;
    }

    // OFFSET 0x02065CEC
    private static int MovementAction_WalkOnSpotSlowerWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 2, 0x20, 1);
        return 1;
    }

    // OFFSET 0x02065CFC
    private static int MovementAction_WalkOnSpotSlowerEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 3, 0x20, 1);
        return 1;
    }

    // OFFSET 0x02065D0C
    private static int MovementAction_WalkOnSpotSlowNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 0, 0x10, 2);
        return 1;
    }

    // OFFSET 0x02065D1C
    private static int MovementAction_WalkOnSpotSlowSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 1, 0x10, 2);
        return 1;
    }

    // OFFSET 0x02065D2C
    private static int MovementAction_WalkOnSpotSlowWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 2, 0x10, 2);
        return 1;
    }

    // OFFSET 0x02065D3C
    private static int MovementAction_WalkOnSpotSlowEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 3, 0x10, 2);
        return 1;
    }

    // OFFSET 0x02065D4C
    private static int MovementAction_WalkOnSpotNormalNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 0, 8, 3);
        return 1;
    }

    // OFFSET 0x02065D5C
    private static int MovementAction_WalkOnSpotNormalSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 1, 8, 3);
        return 1;
    }

    // OFFSET 0x02065D6C
    private static int MovementAction_WalkOnSpotNormalWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 2, 8, 3);
        return 1;
    }

    // OFFSET 0x02065D7C
    private static int MovementAction_WalkOnSpotNormalEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 3, 8, 3);
        return 1;
    }

    // OFFSET 0x02065D8C
    private static int MovementAction_WalkOnSpotFastNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 0, 4, 4);
        return 1;
    }

    // OFFSET 0x02065D9C
    private static int MovementAction_WalkOnSpotFastSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 1, 4, 4);
        return 1;
    }

    // OFFSET 0x02065DAC
    private static int MovementAction_WalkOnSpotFastWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 2, 4, 4);
        return 1;
    }

    // OFFSET 0x02065DBC
    private static int MovementAction_WalkOnSpotFastEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 3, 4, 4);
        return 1;
    }

    // OFFSET 0x02065DCC
    private static int MovementAction_WalkOnSpotFasterNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 0, 2, 5);
        return 1;
    }

    // OFFSET 0x02065DDC
    private static int MovementAction_WalkOnSpotFasterSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 1, 2, 5);
        return 1;
    }

    // OFFSET 0x02065DEC
    private static int MovementAction_WalkOnSpotFasterWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 2, 2, 5);
        return 1;
    }

    // OFFSET 0x02065DFC
    private static int MovementAction_WalkOnSpotFasterEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkOnSpot(mapObject, 3, 2, 5);
        return 1;
    }

    // OFFSET 0x02065F88
    private static int MovementAction_JumpOnSpotSlowNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 0, 0, 0x10, 2, 0, 0x100);
        return 1;
    }

    // OFFSET 0x02065FA8
    private static int MovementAction_JumpOnSpotSlowSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 1, 0, 0x10, 2, 0, 0x100);
        return 1;
    }

    // OFFSET 0x02065FC8
    private static int MovementAction_JumpOnSpotSlowWest_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 2, 0, 0x10, 2, 0, 0x100);
        return 1;
    }

    // OFFSET 0x02065FE8
    private static int MovementAction_JumpOnSpotSlowEast_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 3, 0, 0x10, 2, 0, 0x100);
        return 1;
    }

    // OFFSET 0x02066008
    private static int MovementAction_JumpOnSpotFastNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 0, 0, 8, 3, 0, 0x200);
        return 1;
    }

    // OFFSET 0x02066028
    private static int MovementAction_JumpOnSpotFastSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 1, 0, 8, 3, 0, 0x200);
        return 1;
    }

    // OFFSET 0x02066048
    private static int MovementAction_JumpOnSpotFastWest_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 2, 0, 8, 3, 0, 0x200);
        return 1;
    }

    // OFFSET 0x02066068
    private static int MovementAction_JumpOnSpotFastEast_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 3, 0, 8, 3, 0, 0x200);
        return 1;
    }

    // OFFSET 0x02066088
    private static int MovementAction_JumpNearFastNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 0, 0x2000, 8, 3, 0, 0x200);
        return 1;
    }

    // OFFSET 0x020660A8
    private static int MovementAction_JumpNearFastSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 1, 0x2000, 8, 3, 0, 0x200);
        return 1;
    }

    // OFFSET 0x020660CC
    private static int MovementAction_JumpNearFastWest_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 2, 0x2000, 8, 3, 0, 0x200);
        return 1;
    }

    // OFFSET 0x020660F0
    private static int MovementAction_JumpNearFastEast_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 3, 0x2000, 8, 3, 0, 0x200);
        return 1;
    }

    // OFFSET 0x02066110
    private static int MovementAction_JumpFarNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 0, 0x2000, 0x10, 3, 0, 0x100);
        return 1;
    }

    // OFFSET 0x02066130
    private static int MovementAction_JumpFarSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 1, 0x2000, 0x10, 3, 0, 0x100);
        return 1;
    }

    // OFFSET 0x02066150
    private static int MovementAction_JumpFarWest_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 2, 0x2000, 0x10, 3, 0, 0x100);
        return 1;
    }

    // OFFSET 0x02066170
    private static int MovementAction_JumpFarEast_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 3, 0x2000, 0x10, 3, 0, 0x100);
        return 1;
    }

    // OFFSET 0x02066194
    private static int MovementAction_JumpNearSlowWest_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 2, 0x1000, 0x10, 9, 0, 0xF0);
        return 1;
    }

    // OFFSET 0x020661B4
    private static int MovementAction_JumpNearSlowEast_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 3, 0x1000, 0x10, 9, 0, 0xF0);
        return 1;
    }

    // OFFSET 0x020661D8
    private static int MovementAction_JumpFartherWest_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 2, 0x4000, 12, 10, 0, 0x140);
        return 1;
    }

    // OFFSET 0x020661F8
    private static int MovementAction_JumpFartherEast_Step0(MapObject mapObject)
    {
        MovementAction_InitJump(mapObject, 3, 0x4000, 12, 10, 0, 0x140);
        return 1;
    }

    // OFFSET 0x0206621C
    private static int MovementAction_JumpDistortionWorldNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitJumpCustomSound(mapObject, 0, 0x2000, 0x18, 3, 0, 0xA0, 0);
        return 1;
    }

    // OFFSET 0x02066240
    private static int MovementAction_JumpDistortionWorldSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitJumpCustomSound(mapObject, 1, 0x2000, 0x18, 3, 0, 0xA0, 0);
        return 1;
    }

    // OFFSET 0x02066264
    private static int MovementAction_JumpDistortionWorldWest_Step0(MapObject mapObject)
    {
        MovementAction_InitJumpCustomSound(mapObject, 2, 0x2000, 0x18, 3, 0, 0xA0, 0);
        return 1;
    }

    // OFFSET 0x02066288
    private static int MovementAction_JumpDistortionWorldEast_Step0(MapObject mapObject)
    {
        MovementAction_InitJumpCustomSound(mapObject, 3, 0x2000, 0x18, 3, 0, 0xA0, 0);
        return 1;
    }

    // OFFSET 0x020662E4
    private static int MovementAction_Delay1_Step0(MapObject mapObject)
    {
        MovementAction_InitDelay(mapObject, 1);
        return 1;
    }

    // OFFSET 0x020662F0
    private static int MovementAction_Delay2_Step0(MapObject mapObject)
    {
        MovementAction_InitDelay(mapObject, 2);
        return 1;
    }

    // OFFSET 0x020662FC
    private static int MovementAction_Delay4_Step0(MapObject mapObject)
    {
        MovementAction_InitDelay(mapObject, 4);
        return 1;
    }

    // OFFSET 0x02066308
    private static int MovementAction_Delay8_Step0(MapObject mapObject)
    {
        MovementAction_InitDelay(mapObject, 8);
        return 1;
    }

    // OFFSET 0x02066314
    private static int MovementAction_Delay15_Step0(MapObject mapObject)
    {
        MovementAction_InitDelay(mapObject, 15);
        return 1;
    }

    // OFFSET 0x02066320
    private static int MovementAction_Delay16_Step0(MapObject mapObject)
    {
        MovementAction_InitDelay(mapObject, 0x10);
        return 1;
    }

    // OFFSET 0x0206632C
    private static int MovementAction_Delay32_Step0(MapObject mapObject)
    {
        MovementAction_InitDelay(mapObject, 0x20);
        return 1;
    }

    // OFFSET 0x020664F0
    private static int MovementAction_EmoteExclamationMark_Step0(MapObject mapObject)
    {
        MovementAction_InitEmote(mapObject, 0, 0);
        return 0;
    }

    // OFFSET 0x02066500
    private static int MovementAction_EmoteDoubleExclamationMark_Step0(MapObject mapObject)
    {
        MovementAction_InitEmote(mapObject, 1, 0);
        return 0;
    }

    // OFFSET 0x02066510
    private static int MovementAction_Emote153_Step0(MapObject mapObject)
    {
        MovementAction_InitEmote(mapObject, 0, 1);
        return 0;
    }

    // OFFSET 0x020665C0
    private static int MovementAction_WalkSlightlyFastNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkUneven(mapObject, 0, 6, 6);
        return 1;
    }

    // OFFSET 0x020665D0
    private static int MovementAction_WalkSlightlyFastSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkUneven(mapObject, 1, 6, 6);
        return 1;
    }

    // OFFSET 0x020665E0
    private static int MovementAction_WalkSlightlyFastWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkUneven(mapObject, 2, 6, 6);
        return 1;
    }

    // OFFSET 0x020665F0
    private static int MovementAction_WalkSlightlyFastEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkUneven(mapObject, 3, 6, 6);
        return 1;
    }

    // OFFSET 0x02066618
    private static int MovementAction_WalkSlightlyFasterNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkUneven(mapObject, 0, 3, 7);
        return 1;
    }

    // OFFSET 0x02066628
    private static int MovementAction_WalkSlightlyFasterSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkUneven(mapObject, 1, 3, 7);
        return 1;
    }

    // OFFSET 0x02066638
    private static int MovementAction_WalkSlightlyFasterWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkUneven(mapObject, 2, 3, 7);
        return 1;
    }

    // OFFSET 0x02066648
    private static int MovementAction_WalkSlightlyFasterEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkUneven(mapObject, 3, 3, 7);
        return 1;
    }

    // OFFSET 0x02066670
    private static int MovementAction_WalkEverSoSlightlyFastNorth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkUneven(mapObject, 0, 7, 8);
        return 1;
    }

    // OFFSET 0x02066680
    private static int MovementAction_WalkEverSoSlightlyFastSouth_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkUneven(mapObject, 1, 7, 8);
        return 1;
    }

    // OFFSET 0x02066690
    private static int MovementAction_WalkEverSoSlightlyFastWest_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkUneven(mapObject, 2, 7, 8);
        return 1;
    }

    // OFFSET 0x020666A0
    private static int MovementAction_WalkEverSoSlightlyFastEast_Step0(MapObject mapObject)
    {
        MovementAction_InitWalkUneven(mapObject, 3, 7, 8);
        return 1;
    }

    // OFFSET 0x02066934
    private static int MovementAction_VectorWalk105_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EECC8), 2, 0, 8, 3);
        return 1;
    }

    // OFFSET 0x02066968
    private static int MovementAction_VectorWalk106_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EECD4), 3, 1, 8, 3);
        return 1;
    }

    // OFFSET 0x02066998
    private static int MovementAction_VectorWalk107_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EED4C), 1, 2, 8, 3);
        return 1;
    }

    // OFFSET 0x020669CC
    private static int MovementAction_VectorWalk108_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EECE0), 0, 3, 8, 3);
        return 1;
    }

    // OFFSET 0x020669FC
    private static int MovementAction_VectorWalk109_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEE48), 3, 0, 8, 3);
        return 1;
    }

    // OFFSET 0x02066A2C
    private static int MovementAction_VectorWalk110_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEE3C), 2, 1, 8, 3);
        return 1;
    }

    // OFFSET 0x02066A60
    private static int MovementAction_VectorWalk111_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEE30), 0, 2, 8, 3);
        return 1;
    }

    // OFFSET 0x02066A94
    private static int MovementAction_VectorWalk112_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEE24), 1, 2, 8, 3);
        return 1;
    }

    // OFFSET 0x02066AC8
    private static int MovementAction_VectorWalk113_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEE18), 1, 0, 8, 3);
        return 1;
    }

    // OFFSET 0x02066AFC
    private static int MovementAction_VectorWalk114_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEE0C), 0, 1, 8, 3);
        return 1;
    }

    // OFFSET 0x02066B30
    private static int MovementAction_VectorWalk115_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEE00), 3, 2, 8, 3);
        return 1;
    }

    // OFFSET 0x02066B60
    private static int MovementAction_VectorWalk116_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEDF4), 2, 3, 8, 3);
        return 1;
    }

    // OFFSET 0x02066B90
    private static int MovementAction_VectorWalk145_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEDE8), 1, 0, 4, 4);
        return 1;
    }

    // OFFSET 0x02066BC0
    private static int MovementAction_VectorWalk146_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEDDC), 0, 1, 4, 4);
        return 1;
    }

    // OFFSET 0x02066BF0
    private static int MovementAction_VectorWalk147_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEDD0), 3, 2, 4, 4);
        return 1;
    }

    // OFFSET 0x02066C20
    private static int MovementAction_VectorWalk148_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEDC4), 2, 3, 4, 4);
        return 1;
    }

    // OFFSET 0x02066C50
    private static int MovementAction_VectorWalk149_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEDB8), 1, 0, 2, 5);
        return 1;
    }

    // OFFSET 0x02066C84
    private static int MovementAction_VectorWalk150_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEDAC), 0, 1, 2, 5);
        return 1;
    }

    // OFFSET 0x02066CB8
    private static int MovementAction_VectorWalk151_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EEDA0), 3, 2, 2, 5);
        return 1;
    }

    // OFFSET 0x02066CE8
    private static int MovementAction_VectorWalk152_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EED94), 2, 3, 2, 5);
        return 1;
    }

    // OFFSET 0x02066D18
    private static int MovementAction_VectorWalk121_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EED88), 2, 0, 4, 9);
        return 1;
    }

    // OFFSET 0x02066D4C
    private static int MovementAction_VectorWalk122_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EED7C), 3, 1, 4, 9);
        return 1;
    }

    // OFFSET 0x02066D80
    private static int MovementAction_VectorWalk123_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EED70), 1, 2, 4, 9);
        return 1;
    }

    // OFFSET 0x02066DB4
    private static int MovementAction_VectorWalk124_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EED64), 0, 3, 4, 9);
        return 1;
    }

    // OFFSET 0x02066DE8
    private static int MovementAction_VectorWalk125_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EED58), 3, 0, 4, 9);
        return 1;
    }

    // OFFSET 0x02066E1C
    private static int MovementAction_VectorWalk126_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EED1C), 2, 1, 4, 9);
        return 1;
    }

    // OFFSET 0x02066E50
    private static int MovementAction_VectorWalk127_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EED40), 0, 2, 4, 9);
        return 1;
    }

    // OFFSET 0x02066E84
    private static int MovementAction_VectorWalk128_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EED10), 1, 2, 4, 9);
        return 1;
    }

    // OFFSET 0x02066EB8
    private static int MovementAction_VectorWalk129_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EED34), 1, 0, 4, 9);
        return 1;
    }

    // OFFSET 0x02066EEC
    private static int MovementAction_VectorWalk130_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EED04), 0, 1, 4, 9);
        return 1;
    }

    // OFFSET 0x02066F20
    private static int MovementAction_VectorWalk131_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EED28), 3, 2, 4, 9);
        return 1;
    }

    // OFFSET 0x02066F54
    private static int MovementAction_VectorWalk132_Step0(MapObject mapObject)
    {
        MovementAction_InitVectorWalk(mapObject, RomVec(0x020EECF8), 2, 3, 4, 9);
        return 1;
    }

    // OFFSET 0x020671F0
    private static int MovementAction_AxisJump133_Step0(MapObject mapObject)
    {
        MovementAction_InitAxisJump(mapObject, 0x2000, 2, 0, 8, 3, 1, 1, 0);
        return 1;
    }

    // OFFSET 0x02067214
    private static int MovementAction_AxisJump134_Step0(MapObject mapObject)
    {
        MovementAction_InitAxisJump(mapObject, -0x2000, 3, 1, 8, 3, 1, 1, 0);
        return 1;
    }

    // OFFSET 0x0206723C
    private static int MovementAction_AxisJump135_Step0(MapObject mapObject)
    {
        MovementAction_InitAxisJump(mapObject, 0x2000, 1, 2, 8, 3, 2, 1, 0);
        return 1;
    }

    // OFFSET 0x02067260
    private static int MovementAction_AxisJump136_Step0(MapObject mapObject)
    {
        MovementAction_InitAxisJump(mapObject, -0x2000, 0, 3, 8, 3, 2, 1, 0);
        return 1;
    }

    // OFFSET 0x02067288
    private static int MovementAction_AxisJump137_Step0(MapObject mapObject)
    {
        MovementAction_InitAxisJump(mapObject, 0x2000, 3, 0, 8, 3, 1, 1, 0);
        return 1;
    }

    // OFFSET 0x020672AC
    private static int MovementAction_AxisJump138_Step0(MapObject mapObject)
    {
        MovementAction_InitAxisJump(mapObject, -0x2000, 2, 1, 8, 3, 1, 1, 0);
        return 1;
    }

    // OFFSET 0x020672D4
    private static int MovementAction_AxisJump139_Step0(MapObject mapObject)
    {
        MovementAction_InitAxisJump(mapObject, -0x2000, 0, 2, 8, 3, 2, 1, 0);
        return 1;
    }

    // OFFSET 0x020672FC
    private static int MovementAction_AxisJump140_Step0(MapObject mapObject)
    {
        MovementAction_InitAxisJump(mapObject, 0x2000, 1, 2, 8, 3, 2, 1, 0);
        return 1;
    }

    // OFFSET 0x02067320
    private static int MovementAction_AxisJump141_Step0(MapObject mapObject)
    {
        MovementAction_InitAxisJump(mapObject, 0x2000, 1, 0, 8, 3, 2, 1, 0);
        return 1;
    }

    // OFFSET 0x02067344
    private static int MovementAction_AxisJump142_Step0(MapObject mapObject)
    {
        MovementAction_InitAxisJump(mapObject, -0x2000, 0, 1, 8, 3, 2, 1, 0);
        return 1;
    }

    // OFFSET 0x0206736C
    private static int MovementAction_AxisJump143_Step0(MapObject mapObject)
    {
        MovementAction_InitAxisJump(mapObject, -0x2000, 3, 2, 8, 3, 0, 1, 0);
        return 1;
    }

    // OFFSET 0x02067394
    private static int MovementAction_AxisJump144_Step0(MapObject mapObject)
    {
        MovementAction_InitAxisJump(mapObject, 0x2000, 2, 3, 8, 3, 0, 1, 0);
        return 1;
    }

    private static MovementActionStepFunc? ResolveStep(uint address)
    {
        switch (address & ~1u)
        {
            case 0x02065924:
                return MovementAction_End;
            case 0x02065950:
                return MovementAction_FaceNorth_Step0;
            case 0x0206595C:
                return MovementAction_FaceSouth_Step0;
            case 0x02065968:
                return MovementAction_FaceWest_Step0;
            case 0x02065974:
                return MovementAction_FaceEast_Step0;
            case 0x020659C8:
                return MovementAction_Walk_Step1;
            case 0x02065A1C:
                return MovementAction_WalkSlowerNorth_Step0;
            case 0x02065A30:
                return MovementAction_WalkSlowerSouth_Step0;
            case 0x02065A44:
                return MovementAction_WalkSlowerWest_Step0;
            case 0x02065A58:
                return MovementAction_WalkSlowerEast_Step0;
            case 0x02065A6C:
                return MovementAction_WalkSlowNorth_Step0;
            case 0x02065A80:
                return MovementAction_WalkSlowSouth_Step0;
            case 0x02065A94:
                return MovementAction_WalkSlowWest_Step0;
            case 0x02065AA8:
                return MovementAction_WalkSlowEast_Step0;
            case 0x02065ABC:
                return MovementAction_WalkNormalNorth_Step0;
            case 0x02065AD4:
                return MovementAction_WalkNormalSouth_Step0;
            case 0x02065AE8:
                return MovementAction_WalkNormalWest_Step0;
            case 0x02065AFC:
                return MovementAction_WalkNormalEast_Step0;
            case 0x02065B10:
                return MovementAction_WalkFastNorth_Step0;
            case 0x02065B24:
                return MovementAction_WalkFastSouth_Step0;
            case 0x02065B38:
                return MovementAction_WalkFastWest_Step0;
            case 0x02065B4C:
                return MovementAction_WalkFastEast_Step0;
            case 0x02065B60:
                return MovementAction_WalkFasterNorth_Step0;
            case 0x02065B78:
                return MovementAction_WalkFasterSouth_Step0;
            case 0x02065B8C:
                return MovementAction_WalkFasterWest_Step0;
            case 0x02065BA0:
                return MovementAction_WalkFasterEast_Step0;
            case 0x02065BB8:
                return MovementAction_WalkFastestNorth_Step0;
            case 0x02065BCC:
                return MovementAction_WalkFastestSouth_Step0;
            case 0x02065BE0:
                return MovementAction_WalkFastestWest_Step0;
            case 0x02065BF4:
                return MovementAction_WalkFastestEast_Step0;
            case 0x02065C0C:
                return MovementAction_RunNorth_Step0;
            case 0x02065C24:
                return MovementAction_RunSouth_Step0;
            case 0x02065C38:
                return MovementAction_RunWest_Step0;
            case 0x02065C4C:
                return MovementAction_RunEast_Step0;
            case 0x02065C98:
                return MovementAction_WalkOnSpot_Step1;
            case 0x02065CCC:
                return MovementAction_WalkOnSpotSlowerNorth_Step0;
            case 0x02065CDC:
                return MovementAction_WalkOnSpotSlowerSouth_Step0;
            case 0x02065CEC:
                return MovementAction_WalkOnSpotSlowerWest_Step0;
            case 0x02065CFC:
                return MovementAction_WalkOnSpotSlowerEast_Step0;
            case 0x02065D0C:
                return MovementAction_WalkOnSpotSlowNorth_Step0;
            case 0x02065D1C:
                return MovementAction_WalkOnSpotSlowSouth_Step0;
            case 0x02065D2C:
                return MovementAction_WalkOnSpotSlowWest_Step0;
            case 0x02065D3C:
                return MovementAction_WalkOnSpotSlowEast_Step0;
            case 0x02065D4C:
                return MovementAction_WalkOnSpotNormalNorth_Step0;
            case 0x02065D5C:
                return MovementAction_WalkOnSpotNormalSouth_Step0;
            case 0x02065D6C:
                return MovementAction_WalkOnSpotNormalWest_Step0;
            case 0x02065D7C:
                return MovementAction_WalkOnSpotNormalEast_Step0;
            case 0x02065D8C:
                return MovementAction_WalkOnSpotFastNorth_Step0;
            case 0x02065D9C:
                return MovementAction_WalkOnSpotFastSouth_Step0;
            case 0x02065DAC:
                return MovementAction_WalkOnSpotFastWest_Step0;
            case 0x02065DBC:
                return MovementAction_WalkOnSpotFastEast_Step0;
            case 0x02065DCC:
                return MovementAction_WalkOnSpotFasterNorth_Step0;
            case 0x02065DDC:
                return MovementAction_WalkOnSpotFasterSouth_Step0;
            case 0x02065DEC:
                return MovementAction_WalkOnSpotFasterWest_Step0;
            case 0x02065DFC:
                return MovementAction_WalkOnSpotFasterEast_Step0;
            case 0x02065EA0:
                return MovementAction_Jump_Step1;
            case 0x02065F88:
                return MovementAction_JumpOnSpotSlowNorth_Step0;
            case 0x02065FA8:
                return MovementAction_JumpOnSpotSlowSouth_Step0;
            case 0x02065FC8:
                return MovementAction_JumpOnSpotSlowWest_Step0;
            case 0x02065FE8:
                return MovementAction_JumpOnSpotSlowEast_Step0;
            case 0x02066008:
                return MovementAction_JumpOnSpotFastNorth_Step0;
            case 0x02066028:
                return MovementAction_JumpOnSpotFastSouth_Step0;
            case 0x02066048:
                return MovementAction_JumpOnSpotFastWest_Step0;
            case 0x02066068:
                return MovementAction_JumpOnSpotFastEast_Step0;
            case 0x02066088:
                return MovementAction_JumpNearFastNorth_Step0;
            case 0x020660A8:
                return MovementAction_JumpNearFastSouth_Step0;
            case 0x020660CC:
                return MovementAction_JumpNearFastWest_Step0;
            case 0x020660F0:
                return MovementAction_JumpNearFastEast_Step0;
            case 0x02066110:
                return MovementAction_JumpFarNorth_Step0;
            case 0x02066130:
                return MovementAction_JumpFarSouth_Step0;
            case 0x02066150:
                return MovementAction_JumpFarWest_Step0;
            case 0x02066170:
                return MovementAction_JumpFarEast_Step0;
            case 0x02066194:
                return MovementAction_JumpNearSlowWest_Step0;
            case 0x020661B4:
                return MovementAction_JumpNearSlowEast_Step0;
            case 0x020661D8:
                return MovementAction_JumpFartherWest_Step0;
            case 0x020661F8:
                return MovementAction_JumpFartherEast_Step0;
            case 0x0206621C:
                return MovementAction_JumpDistortionWorldNorth_Step0;
            case 0x02066240:
                return MovementAction_JumpDistortionWorldSouth_Step0;
            case 0x02066264:
                return MovementAction_JumpDistortionWorldWest_Step0;
            case 0x02066288:
                return MovementAction_JumpDistortionWorldEast_Step0;
            case 0x020662C4:
                return MovementAction_Delay_Step1;
            case 0x020662E4:
                return MovementAction_Delay1_Step0;
            case 0x020662F0:
                return MovementAction_Delay2_Step0;
            case 0x020662FC:
                return MovementAction_Delay4_Step0;
            case 0x02066308:
                return MovementAction_Delay8_Step0;
            case 0x02066314:
                return MovementAction_Delay15_Step0;
            case 0x02066320:
                return MovementAction_Delay16_Step0;
            case 0x0206632C:
                return MovementAction_Delay32_Step0;
            case 0x02066338:
                return MovementAction_WarpOut_Step0;
            case 0x0206635C:
                return MovementAction_WarpOut_Step1;
            case 0x020663A4:
                return MovementAction_WarpIn_Step0;
            case 0x020663D0:
                return MovementAction_WarpIn_Step1;
            case 0x02066418:
                return MovementAction_SetInvisible_Step0;
            case 0x02066430:
                return MovementAction_SetVisible_Step0;
            case 0x02066448:
                return MovementAction_LockDir_Step0;
            case 0x0206645C:
                return MovementAction_UnlockDir_Step0;
            case 0x02066470:
                return MovementAction_PauseAnimation_Step0;
            case 0x02066488:
                return MovementAction_ResumeAnimation_Step0;
            case 0x020664C8:
                return MovementAction_Emote_Step1;
            case 0x020664F0:
                return MovementAction_EmoteExclamationMark_Step0;
            case 0x02066500:
                return MovementAction_EmoteDoubleExclamationMark_Step0;
            case 0x02066510:
                return MovementAction_Emote153_Step0;
            case 0x020665C0:
                return MovementAction_WalkSlightlyFastNorth_Step0;
            case 0x020665D0:
                return MovementAction_WalkSlightlyFastSouth_Step0;
            case 0x020665E0:
                return MovementAction_WalkSlightlyFastWest_Step0;
            case 0x020665F0:
                return MovementAction_WalkSlightlyFastEast_Step0;
            case 0x02066600:
                return MovementAction_WalkSlightlyFast_Step1;
            case 0x02066618:
                return MovementAction_WalkSlightlyFasterNorth_Step0;
            case 0x02066628:
                return MovementAction_WalkSlightlyFasterSouth_Step0;
            case 0x02066638:
                return MovementAction_WalkSlightlyFasterWest_Step0;
            case 0x02066648:
                return MovementAction_WalkSlightlyFasterEast_Step0;
            case 0x02066658:
                return MovementAction_WalkSlightlyFaster_Step1;
            case 0x02066670:
                return MovementAction_WalkEverSoSlightlyFastNorth_Step0;
            case 0x02066680:
                return MovementAction_WalkEverSoSlightlyFastSouth_Step0;
            case 0x02066690:
                return MovementAction_WalkEverSoSlightlyFastWest_Step0;
            case 0x020666A0:
                return MovementAction_WalkEverSoSlightlyFastEast_Step0;
            case 0x020666B0:
                return MovementAction_WalkEverSoSlightlyFast_Step1;
            case 0x020666C8:
                return MovementAction_PokecenterNurseBow_Step0;
            case 0x020666E4:
                return MovementAction_PokecenterNurseBow_Step1;
            case 0x02066710:
                return MovementAction_RevealTrainer_Step0;
            case 0x02066764:
                return MovementAction_RevealTrainer_Step1;
            case 0x020667CC:
                return MovementAction_PlayerGive_Step0;
            case 0x020667E8:
                return MovementAction_PlayerReceive_Step0;
            case 0x02066804:
                return MovementAction_PlayerGiveReceive_Step1;
            case 0x020668EC:
                return MovementAction_VectorWalk_Step1;
            case 0x02066934:
                return MovementAction_VectorWalk105_Step0;
            case 0x02066968:
                return MovementAction_VectorWalk106_Step0;
            case 0x02066998:
                return MovementAction_VectorWalk107_Step0;
            case 0x020669CC:
                return MovementAction_VectorWalk108_Step0;
            case 0x020669FC:
                return MovementAction_VectorWalk109_Step0;
            case 0x02066A2C:
                return MovementAction_VectorWalk110_Step0;
            case 0x02066A60:
                return MovementAction_VectorWalk111_Step0;
            case 0x02066A94:
                return MovementAction_VectorWalk112_Step0;
            case 0x02066AC8:
                return MovementAction_VectorWalk113_Step0;
            case 0x02066AFC:
                return MovementAction_VectorWalk114_Step0;
            case 0x02066B30:
                return MovementAction_VectorWalk115_Step0;
            case 0x02066B60:
                return MovementAction_VectorWalk116_Step0;
            case 0x02066B90:
                return MovementAction_VectorWalk145_Step0;
            case 0x02066BC0:
                return MovementAction_VectorWalk146_Step0;
            case 0x02066BF0:
                return MovementAction_VectorWalk147_Step0;
            case 0x02066C20:
                return MovementAction_VectorWalk148_Step0;
            case 0x02066C50:
                return MovementAction_VectorWalk149_Step0;
            case 0x02066C84:
                return MovementAction_VectorWalk150_Step0;
            case 0x02066CB8:
                return MovementAction_VectorWalk151_Step0;
            case 0x02066CE8:
                return MovementAction_VectorWalk152_Step0;
            case 0x02066D18:
                return MovementAction_VectorWalk121_Step0;
            case 0x02066D4C:
                return MovementAction_VectorWalk122_Step0;
            case 0x02066D80:
                return MovementAction_VectorWalk123_Step0;
            case 0x02066DB4:
                return MovementAction_VectorWalk124_Step0;
            case 0x02066DE8:
                return MovementAction_VectorWalk125_Step0;
            case 0x02066E1C:
                return MovementAction_VectorWalk126_Step0;
            case 0x02066E50:
                return MovementAction_VectorWalk127_Step0;
            case 0x02066E84:
                return MovementAction_VectorWalk128_Step0;
            case 0x02066EB8:
                return MovementAction_VectorWalk129_Step0;
            case 0x02066EEC:
                return MovementAction_VectorWalk130_Step0;
            case 0x02066F20:
                return MovementAction_VectorWalk131_Step0;
            case 0x02066F54:
                return MovementAction_VectorWalk132_Step0;
            case 0x02067068:
                return MovementAction_AxisJump_Step1;
            case 0x020671F0:
                return MovementAction_AxisJump133_Step0;
            case 0x02067214:
                return MovementAction_AxisJump134_Step0;
            case 0x0206723C:
                return MovementAction_AxisJump135_Step0;
            case 0x02067260:
                return MovementAction_AxisJump136_Step0;
            case 0x02067288:
                return MovementAction_AxisJump137_Step0;
            case 0x020672AC:
                return MovementAction_AxisJump138_Step0;
            case 0x020672D4:
                return MovementAction_AxisJump139_Step0;
            case 0x020672FC:
                return MovementAction_AxisJump140_Step0;
            case 0x02067320:
                return MovementAction_AxisJump141_Step0;
            case 0x02067344:
                return MovementAction_AxisJump142_Step0;
            case 0x0206736C:
                return MovementAction_AxisJump143_Step0;
            case 0x02067394:
                return MovementAction_AxisJump144_Step0;
            default:
                return null;
        }
    }
}
