using static PlatinumSharp.Game.MapObject;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate int TileBehaviorFunc(int tileBehavior);

public static class MapObjectMove
{
    public const int DirNorth = 0;
    public const int DirSouth = 1;
    public const int DirWest = 2;
    public const int DirEast = 3;
    public const int DirNone = -1;

    public const uint CollisionOutOfRange = 1 << 0;
    public const uint CollisionTerrain = 1 << 1;
    public const uint CollisionObject = 1 << 2;
    public const uint CollisionHeight = 1 << 3;

    private const uint DeepestSnowOffsetAddress = 0x020EE720;
    private const uint DeeperSnowOffsetAddress = 0x020EE72C;
    private const uint DeepMudOffsetAddress = 0x020EE738;
    private const uint MudOffsetAddress = 0x020EE744;
    private const uint DeepSnowOffsetAddress = 0x020EE750;
    private const uint DirDxAddress = 0x020EE75C;
    private const uint DirDzAddress = 0x020EE78C;
    private const uint DirOppositeAddress = 0x020EE79C;
    private const int DirCount = 4;

    private const int FootprintTypeShift = 6;
    private const uint FootprintTypeMask = 0xF;
    private const int ShadowSizeShift = 4;
    private const uint ShadowSizeMask = 0x3;
    private const int ReflectionShift = 10;
    private const uint ReflectionMask = 0x3;
    private const int FootprintTypeHuman = 1;
    private const int FootprintTypeBike = 2;
    private const int ReflectionTypePuddle = 0;
    private const int ReflectionTypeWater = 1;
    private const int ReflectionTypeMirror = 2;
    private const int MovementTypeScriptedFirst = 0x33;
    private const int MovementTypeScriptedRange = 3;
    private const int ObjectCollisionHeightRange = 2;
    private const int HeightSourceNone = 0;
    private const int HeightSourceDynamic = 2;
    private const uint ManagerStatusStopMovement = 1 << 1;

    private static readonly TileBehaviorFunc[] sCurrentTileBlocksMovement =
    {
        TileBehavior.TileBehavior_BlocksMovementNorthward,
        TileBehavior.TileBehavior_BlocksMovementSouthward,
        TileBehavior.TileBehavior_BlocksMovementWestward,
        TileBehavior.TileBehavior_BlocksMovementEastward,
    };

    private static readonly TileBehaviorFunc[] sNextTileBlocksMovement =
    {
        TileBehavior.TileBehavior_BlocksMovementSouthward,
        TileBehavior.TileBehavior_BlocksMovementNorthward,
        TileBehavior.TileBehavior_BlocksMovementEastward,
        TileBehavior.TileBehavior_BlocksMovementWestward,
    };

    private static int[]? sDirDx;
    private static int[]? sDirDz;
    private static int[]? sDirOpposite;
    private static readonly Dictionary<uint, VecFx32> sTerrainOffsets = new();

    // OFFSET 0x02063400
    public static void MapObject_InitMove(MapObject mapObject)
    {
        MapObject_CallMovementInit(mapObject);
        MapObjectTrainerType.MapObject_InitTrainerType(mapObject);
    }

    // OFFSET 0x02063410
    public static void MapObject_Move(MapObject mapObject)
    {
        if (MapObject_CheckManagerStatus(mapObject, ManagerStatusStopMovement) != 0)
        {
            return;
        }
        MapObject_UpdateHeightIfPending(mapObject);
        MapObject_UpdateTileBehaviorsIfPending(mapObject);
        MapObject_StartMove(mapObject);
        if (MapObject_CheckStatus(mapObject, StatusMovementAction) != 0)
        {
            MovementActions.MapObject_DoMovementAction(mapObject);
        }
        else if (MapObject_IsMovementPaused(mapObject) == 0)
        {
            if (MapObject_CanMove(mapObject) == 1)
            {
                if (MapObjectTrainerType.MapObject_UpdateTrainerType(mapObject) == 0)
                {
                    MapObject_CallMovementMove(mapObject);
                }
            }
        }
        MapObject_StartStep(mapObject);
        MapObject_EndMove(mapObject);
    }

    // OFFSET 0x02063478
    private static int MapObject_CanMove(MapObject mapObject)
    {
        if (MapObject_IsMoving(mapObject) == 1)
        {
            return 1;
        }
        if (MapObject_CheckStatus(mapObject, Status11 | Status12) == 0)
        {
            return 1;
        }
        if (MapObject_GetMovementType(mapObject) == MovementTypeFollowPartnerTrainer)
        {
            return 1;
        }
        uint status = MapObject_GetStatus(mapObject);
        if ((status & Status12) != 0 && (status & StatusHeightCalculationDisabled) == 0)
        {
            return 0;
        }
        if ((status & Status11) != 0)
        {
            if (MapObject_IsStatus2Bit2On(mapObject) == 0)
            {
                return 0;
            }
        }
        return 1;
    }

    // OFFSET 0x020634DC
    private static void MapObject_UpdateHeightIfPending(MapObject mapObject)
    {
        if (MapObject_CheckStatus(mapObject, Status12) != 0)
        {
            MapObject_RecalculateObjectHeight(mapObject);
        }
    }

    // OFFSET 0x020634F4
    private static void MapObject_UpdateTileBehaviorsIfPending(MapObject mapObject)
    {
        if (MapObject_CheckStatus(mapObject, Status11) != 0)
        {
            if (MapObject_SetTileBehaviors(mapObject) == 1)
            {
                MapObject_SetStartMovement(mapObject);
            }
        }
    }

    // OFFSET 0x02063518
    private static void MapObject_StartMove(MapObject mapObject)
    {
        if (MapObject_CheckStatus(mapObject, StatusStartMovement) != 0)
        {
            MapObject_OnStartMove(mapObject);
        }
        MapObject_SetStatusFlagOff(mapObject, StatusStartMovement | StatusStartJump);
    }

    // OFFSET 0x0206353C
    private static void MapObject_StartStep(MapObject mapObject)
    {
        if (MapObject_CheckStatus(mapObject, StatusStartJump) != 0)
        {
            MapObject_OnStartJump(mapObject);
        }
        else if (MapObject_CheckStatus(mapObject, StatusStartMovement) != 0)
        {
            MapObject_OnStartStep(mapObject);
        }
        MapObject_SetStatusFlagOff(mapObject, StatusStartMovement | StatusStartJump);
    }

    // OFFSET 0x02063574
    private static void MapObject_EndMove(MapObject mapObject)
    {
        if (MapObject_CheckStatus(mapObject, StatusEndJump) != 0)
        {
            MapObject_OnEndJump(mapObject);
        }
        else if (MapObject_CheckStatus(mapObject, StatusEndMovement) != 0)
        {
            MapObject_OnEndStep(mapObject);
        }
        MapObject_SetStatusFlagOff(mapObject, StatusEndMovement | StatusEndJump);
    }

    // OFFSET 0x020635AC
    private static void MapObject_OnStartMove(MapObject mapObject)
    {
        MapObject_SetTileBehaviors(mapObject);
        if (MapObject_IsDrawActive(mapObject) == 1)
        {
            int currTileBehavior = (byte)MapObject_GetCurrTileBehavior(mapObject);
            int prevTileBehavior = (byte)MapObject_GetPrevTileBehavior(mapObject);
            MapObjectRenderDetails renderDetails = MapObjectRender.MapObject_GetRenderDetails(mapObject)!;
            MapObject_DetermineElevatedBridgeStatus(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_ShakeTallGrassOnStart(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_UpdateShallowWaterRipple(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_AddShadowOnStart(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_SinkIntoTerrain(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_ShakeVeryTallGrassOnStart(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_ShakeMudGrassOnStart(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_AddReflection(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
        }
    }

    // OFFSET 0x0206363C
    private static void MapObject_OnStartStep(MapObject mapObject)
    {
        MapObject_SetTileBehaviors(mapObject);
        if (MapObject_IsDrawActive(mapObject) == 1)
        {
            int currTileBehavior = (byte)MapObject_GetCurrTileBehavior(mapObject);
            int prevTileBehavior = (byte)MapObject_GetPrevTileBehavior(mapObject);
            MapObjectRenderDetails renderDetails = MapObjectRender.MapObject_GetRenderDetails(mapObject)!;
            MapObject_DetermineElevatedBridgeStatus(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_ShakeTallGrassOnStep(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_AddFootprints(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_UpdateShallowWaterRipple(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_AddShadow(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_ShakeVeryTallGrassOnStep(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_ShakeMudGrassOnStep(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_SplashPuddleOnLeave(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_SplashMudOnLeave(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_AddReflection(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_EmptyFunction(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
        }
    }

    // OFFSET 0x020636F0
    private static void MapObject_OnStartJump(MapObject mapObject)
    {
        MapObject_SetTileBehaviors(mapObject);
        if (MapObject_IsDrawActive(mapObject) == 1)
        {
            int currTileBehavior = (byte)MapObject_GetCurrTileBehavior(mapObject);
            int prevTileBehavior = (byte)MapObject_GetPrevTileBehavior(mapObject);
            MapObjectRenderDetails renderDetails = MapObjectRender.MapObject_GetRenderDetails(mapObject)!;
            MapObject_DetermineElevatedBridgeStatus(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_AddShadow(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_AddReflection(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_ClearShallowWaterRipple(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_EmptyFunction(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
        }
    }

    // OFFSET 0x0206375C
    private static void MapObject_OnEndStep(MapObject mapObject)
    {
        MapObject_SetTileBehaviors(mapObject);
        if (MapObject_IsDrawActive(mapObject) == 1)
        {
            int currTileBehavior = (byte)MapObject_GetCurrTileBehavior(mapObject);
            int prevTileBehavior = (byte)MapObject_GetPrevTileBehavior(mapObject);
            MapObjectRenderDetails renderDetails = MapObjectRender.MapObject_GetRenderDetails(mapObject)!;
            MapObject_SinkIntoTerrain(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_SplashPuddleOnEnter(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_SplashMudOnEnter(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_UpdateShallowWaterRipple(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_RemoveReflection(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_UpdateShadowVisibility(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
        }
    }

    // OFFSET 0x020637D4
    private static void MapObject_OnEndJump(MapObject mapObject)
    {
        MapObject_SetTileBehaviors(mapObject);
        if (MapObject_IsDrawActive(mapObject) == 1)
        {
            int currTileBehavior = (byte)MapObject_GetCurrTileBehavior(mapObject);
            int prevTileBehavior = (byte)MapObject_GetPrevTileBehavior(mapObject);
            MapObjectRenderDetails renderDetails = MapObjectRender.MapObject_GetRenderDetails(mapObject)!;
            MapObject_SinkIntoTerrain(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_SplashPuddleOnEnter(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_SplashMudOnEnter(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_UpdateShallowWaterRipple(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_RemoveReflection(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_UpdateShadowVisibility(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_ShakeTallGrassOnStep(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
            MapObject_AddLandingDust(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
        }
    }

    // OFFSET 0x02063864
    private static void MapObject_SinkIntoTerrain(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (MapObject_CheckFlagDoNotSinkIntoTerrain(mapObject) == 0)
        {
            if (TileBehavior.TileBehavior_IsDeepMud(currTileBehavior) == 1
                || TileBehavior.TileBehavior_IsDeepMudWithGrass(currTileBehavior) == 1)
            {
                VecFx32 offset = TerrainOffset(DeepMudOffsetAddress);
                MapObject_SetSpriteTerrainOffset(mapObject, offset);
                return;
            }
            if (TileBehavior.TileBehavior_IsMud(currTileBehavior) == 1
                || TileBehavior.TileBehavior_IsMudWithGrass(currTileBehavior) == 1)
            {
                VecFx32 offset = TerrainOffset(MudOffsetAddress);
                MapObject_SetSpriteTerrainOffset(mapObject, offset);
                return;
            }
            if (TileBehavior.TileBehavior_IsDeepestSnow(currTileBehavior) == 1)
            {
                VecFx32 offset = TerrainOffset(DeepestSnowOffsetAddress);
                MapObject_SetSpriteTerrainOffset(mapObject, offset);
                return;
            }
            if (TileBehavior.TileBehavior_IsDeeperSnow(currTileBehavior) == 1)
            {
                VecFx32 offset = TerrainOffset(DeeperSnowOffsetAddress);
                MapObject_SetSpriteTerrainOffset(mapObject, offset);
                return;
            }
            if (TileBehavior.TileBehavior_IsDeepSnow(currTileBehavior) == 1)
            {
                VecFx32 offset = TerrainOffset(DeepSnowOffsetAddress);
                MapObject_SetSpriteTerrainOffset(mapObject, offset);
                return;
            }
        }
        VecFx32 zero = default;
        MapObject_SetSpriteTerrainOffset(mapObject, zero);
    }

    // OFFSET 0x02063964
    private static void MapObject_ShakeTallGrassOnStart(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (TileBehavior.TileBehavior_IsTallGrass(currTileBehavior) == 1)
        {
            GrassEffect.GrassEffect_New(mapObject, 0);
        }
    }

    // OFFSET 0x0206397C
    private static void MapObject_ShakeTallGrassOnStep(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (TileBehavior.TileBehavior_IsTallGrass(currTileBehavior) == 1)
        {
            GrassEffect.GrassEffect_New(mapObject, 1);
        }
    }

    // OFFSET 0x02063994
    private static void MapObject_AddFootprints(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (FootprintType(renderDetails) == 0)
        {
            return;
        }
        if (TileBehavior.TileBehavior_IsSnowWithShadows(prevTileBehavior) == 1)
        {
            int footprintType = FootprintType(renderDetails);
            if (footprintType == FootprintTypeHuman)
            {
                FootprintEffect.FootprintEffect_NewAlt(mapObject);
            }
            else if (footprintType == FootprintTypeBike)
            {
                FootprintEffect.FootprintEffect_NewNone(mapObject);
            }
        }
        if (MapObject_IsOnSand(mapObject, prevTileBehavior) == 1)
        {
            int footprintType = FootprintType(renderDetails);
            if (footprintType == FootprintTypeHuman)
            {
                FootprintEffect.FootprintEffect_NewStraight(mapObject);
            }
            else if (footprintType == FootprintTypeBike)
            {
                FootprintEffect.FootprintEffect_NewTurn(mapObject);
            }
            return;
        }
        if (TileBehavior.TileBehavior_IsDeeperSnow(prevTileBehavior) == 1
            || TileBehavior.TileBehavior_IsDeepestSnow(prevTileBehavior) == 1
            || TileBehavior.TileBehavior_IsDeepSnow(prevTileBehavior) != 0)
        {
            FootprintEffect.FootprintEffect_NewAltTurn(mapObject);
            return;
        }
        if (MapObject_IsOnShallowSnow(mapObject, prevTileBehavior) == 1)
        {
            FootprintEffect.FootprintEffect_NewAltStraight(mapObject);
        }
    }

    // OFFSET 0x02063A30
    private static void MapObject_UpdateShallowWaterRipple(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (TileBehavior.TileBehavior_IsShallowWater(currTileBehavior) == 1)
        {
            if (MapObject_IsStatus26On(mapObject) == 0)
            {
                FollowingBillboardEffect.FollowingBillboardEffect_New(mapObject, 1);
                MapObject_SetStatus26(mapObject, 1);
            }
        }
        else
        {
            MapObject_SetStatus26(mapObject, 0);
        }
    }

    // OFFSET 0x02063A64
    private static void MapObject_ClearShallowWaterRipple(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        MapObject_SetStatus26(mapObject, 0);
    }

    // OFFSET 0x02063A70
    private static void MapObject_AddShadowOnStart(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        MapObject_AddShadow(mapObject, currTileBehavior, prevTileBehavior, renderDetails);
    }

    // OFFSET 0x02063A78
    private static void MapObject_AddShadow(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (MapObjectManager.MapObjectMan_AreShadowsEnabled(MapObject_MapObjectManager(mapObject)!) == 0)
        {
            return;
        }
        if (ShadowSize(renderDetails) == 0)
        {
            return;
        }
        if (TileBehavior.TileBehavior_IsTallGrass(currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsVeryTallGrass(currTileBehavior) == 1
            || MapObject_IsOnWater(mapObject, currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsPuddle(currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsShallowWater(currTileBehavior) == 1
            || MapObject_IsOnSnow(mapObject, currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsMud(currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsMudWithGrass(currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsReflective(currTileBehavior) != 0)
        {
            MapObject_SetStatusFlagOn(mapObject, StatusHideShadow);
            return;
        }
        if (MapObject_CheckStatus(mapObject, StatusShowShadow) == 0)
        {
            ShadowEffect.ShadowEffect_New(mapObject);
            MapObject_SetStatusFlagOn(mapObject, StatusShowShadow);
        }
    }

    // OFFSET 0x02063B20
    private static void MapObject_UpdateShadowVisibility(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (MapObjectManager.MapObjectMan_AreShadowsEnabled(MapObject_MapObjectManager(mapObject)!) == 0)
        {
            return;
        }
        if (ShadowSize(renderDetails) == 0)
        {
            return;
        }
        if (TileBehavior.TileBehavior_IsTallGrass(currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsVeryTallGrass(currTileBehavior) == 1
            || MapObject_IsOnWater(mapObject, currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsPuddle(currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsShallowWater(currTileBehavior) == 1
            || MapObject_IsOnSnow(mapObject, currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsMud(currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsMudWithGrass(currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsReflective(currTileBehavior) != 0)
        {
            MapObject_SetStatusFlagOn(mapObject, StatusHideShadow);
            return;
        }
        MapObject_SetStatusFlagOff(mapObject, StatusHideShadow);
    }

    // OFFSET 0x02063BB4
    private static void MapObject_AddLandingDust(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (MapObject_IsOnWater(mapObject, currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsShallowWater(currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsIce(currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsMud(currTileBehavior) == 1
            || TileBehavior.TileBehavior_IsMudWithGrass(currTileBehavior) == 1
            || MapObject_IsOnSnow(mapObject, currTileBehavior) == 1)
        {
            return;
        }
        JumpDustEffect.JumpDustEffect_New(mapObject);
    }

    // OFFSET 0x02063C00
    private static void MapObject_ShakeVeryTallGrassOnStart(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (TileBehavior.TileBehavior_IsVeryTallGrass(currTileBehavior) == 1)
        {
            LongGrassEffect.LongGrassEffect_New(mapObject, 0);
        }
    }

    // OFFSET 0x02063C18
    private static void MapObject_ShakeVeryTallGrassOnStep(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (TileBehavior.TileBehavior_IsVeryTallGrass(currTileBehavior) == 1)
        {
            LongGrassEffect.LongGrassEffect_New(mapObject, 1);
        }
    }

    // OFFSET 0x02063C30
    private static void MapObject_ShakeMudGrassOnStart(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (TileBehavior.TileBehavior_IsMudWithGrass(currTileBehavior) == 1)
        {
            ShallowCoverEffect.ShallowCoverEffect_New(mapObject, 0);
        }
    }

    // OFFSET 0x02063C48
    private static void MapObject_ShakeMudGrassOnStep(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (TileBehavior.TileBehavior_IsMudWithGrass(currTileBehavior) == 1)
        {
            ShallowCoverEffect.ShallowCoverEffect_New(mapObject, 1);
        }
    }

    // OFFSET 0x02063C60
    private static void MapObject_SplashPuddleOnLeave(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (TileBehavior.TileBehavior_IsPuddle(prevTileBehavior) == 1)
        {
            int x = MapObject_GetXPrev(mapObject);
            int y = MapObject_GetYPrev(mapObject);
            int z = MapObject_GetZPrev(mapObject);
            RippleEffect.RippleEffect_New(mapObject, x, y, z);
        }
    }

    // OFFSET 0x02063C94
    private static void MapObject_SplashPuddleOnEnter(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (TileBehavior.TileBehavior_IsPuddle(currTileBehavior) == 1)
        {
            int x = MapObject_GetX(mapObject);
            int y = MapObject_GetY(mapObject);
            int z = MapObject_GetZ(mapObject);
            RippleEffect.RippleEffect_New(mapObject, x, y, z);
        }
    }

    // OFFSET 0x02063CC8
    private static void MapObject_SplashMudOnLeave(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (TileBehavior.TileBehavior_IsMud(prevTileBehavior) == 1)
        {
            int x = MapObject_GetXPrev(mapObject);
            int y = MapObject_GetYPrev(mapObject);
            int z = MapObject_GetZPrev(mapObject);
            SplashEffect.SplashEffect_New(mapObject, x, y, z);
        }
    }

    // OFFSET 0x02063CFC
    private static void MapObject_SplashMudOnEnter(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (TileBehavior.TileBehavior_IsMud(currTileBehavior) == 1)
        {
            int x = MapObject_GetX(mapObject);
            int y = MapObject_GetY(mapObject);
            int z = MapObject_GetZ(mapObject);
            SplashEffect.SplashEffect_New(mapObject, x, y, z);
        }
    }

    // OFFSET 0x02063D30
    private static void MapObject_AddReflection(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (ReflectionEnabled(renderDetails) == 0)
        {
            return;
        }
        if (MapObject_IsStatus24On(mapObject) != 0)
        {
            return;
        }
        int reflectiveTileBehavior = TileBehavior.TileBehavior_GetNull();
        if (TileBehavior.TileBehavior_HasReflectiveSurface(currTileBehavior) == 1)
        {
            reflectiveTileBehavior = currTileBehavior;
        }
        else
        {
            int southTileBehavior = (byte)MapObject_GetTileBehaviorFromDir(mapObject, DirSouth);
            if (TileBehavior.TileBehavior_HasReflectiveSurface(southTileBehavior) == 1)
            {
                reflectiveTileBehavior = southTileBehavior;
            }
        }
        if (reflectiveTileBehavior == TileBehavior.TileBehavior_GetNull())
        {
            return;
        }
        MapObject_SetStatus24(mapObject, 1);
        int reflectionType;
        if (TileBehavior.TileBehavior_IsReflective(reflectiveTileBehavior) == 1)
        {
            reflectionType = ReflectionTypeMirror;
        }
        else if (TileBehavior.TileBehavior_IsPuddle(reflectiveTileBehavior) == 1)
        {
            reflectionType = ReflectionTypePuddle;
        }
        else
        {
            reflectionType = ReflectionTypeWater;
        }
        ReflectionEffect.ReflectionEffect_New(mapObject, reflectionType);
    }

    // OFFSET 0x02063DA8
    private static void MapObject_RemoveReflection(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (ReflectionEnabled(renderDetails) == 0)
        {
            return;
        }
        if (MapObject_IsStatus24On(mapObject) == 0)
        {
            return;
        }
        int southTileBehavior = (byte)MapObject_GetTileBehaviorFromDir(mapObject, DirSouth);
        if (TileBehavior.TileBehavior_HasReflectiveSurface(southTileBehavior) == 0)
        {
            MapObject_SetStatus24(mapObject, 0);
        }
    }

    // OFFSET 0x02063DDC
    private static void MapObject_DetermineElevatedBridgeStatus(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
        if (TileBehavior.TileBehavior_IsBridgeStart(currTileBehavior) == 1)
        {
            MapObject_SetElevatedBridgeStatus(mapObject, 1);
            return;
        }
        if (MapObject_IsStatusOnElevatedBridge(mapObject) == 1)
        {
            if (TileBehavior.TileBehavior_IsBridge(currTileBehavior) == 0)
            {
                MapObject_SetElevatedBridgeStatus(mapObject, 0);
            }
        }
    }

    // OFFSET 0x02063E14
    private static void MapObject_EmptyFunction(MapObject mapObject, int currTileBehavior, int prevTileBehavior, MapObjectRenderDetails renderDetails)
    {
    }

    // OFFSET 0x02063E18
    public static uint MapObject_CheckCollisionAt(MapObject mapObject, ref VecFx32 position, int x, int y, int z, int dir)
    {
        uint result = 0;
        if (MapObject_IsOutOfRange(mapObject, x, y, z) == 1)
        {
            result |= CollisionOutOfRange;
        }
        FieldSystem fieldSystem = MapObject_FieldSystem(mapObject)!;
        if (TerrainCollisionManager.TerrainCollisionManager_WillMapObjectCollide(fieldSystem, ref position, x, z, out sbyte verticalDirection) == 1)
        {
            result |= CollisionTerrain;
            if (verticalDirection != 0)
            {
                result |= CollisionHeight;
            }
        }
        if (MapObject_IsMovementBlockedByTiles(mapObject, x, z, dir) == 1)
        {
            result |= CollisionTerrain;
        }
        if (MapObject_IsObjectAt(mapObject, x, y, z) == 1)
        {
            result |= CollisionObject;
        }
        return result;
    }

    // OFFSET 0x02063E94
    public static uint MapObject_CheckCollisionAtCoords(MapObject mapObject, int x, int y, int z, int dir)
    {
        MapObject_CopyPos(mapObject, out VecFx32 position);
        return MapObject_CheckCollisionAt(mapObject, ref position, x, y, z, dir);
    }

    // OFFSET 0x02063EBC
    public static uint MapObject_CheckCollisionInDir(MapObject mapObject, int dir)
    {
        int x = MapObject_GetX(mapObject) + MapObject_GetDxFromDir(dir);
        int y = MapObject_GetY(mapObject);
        int z = MapObject_GetZ(mapObject) + MapObject_GetDzFromDir(dir);
        return MapObject_CheckCollisionAtCoords(mapObject, x, y, z, dir);
    }

    // OFFSET 0x02063F00
    public static int MapObject_IsObjectAt(MapObject? mapObject, int x, int y, int z)
    {
        MapObjectManager mapObjectManager = MapObject_MapObjectManager(mapObject!)!;
        MapObject? other = MapObjectManager.MapObjectMan_GetMapObjectsConst(mapObjectManager)![0];
        int remaining = MapObjectManager.MapObjectMan_GetMaxObjects(mapObjectManager);
        do
        {
            if (other != mapObject
                && MapObject_CheckStatus(other!, StatusActive) != 0
                && MapObject_CheckStatus(other!, Status18) == 0)
            {
                int otherX = MapObject_GetX(other!);
                int otherZ = MapObject_GetZ(other!);
                if (otherX == x && otherZ == z)
                {
                    int dy = MapObject_GetY(other!) - y;
                    if (dy < 0)
                    {
                        dy = -dy;
                    }
                    if (dy < ObjectCollisionHeightRange)
                    {
                        return 1;
                    }
                }
                otherX = MapObject_GetXPrev(other!);
                otherZ = MapObject_GetZPrev(other!);
                if (otherX == x && otherZ == z)
                {
                    int dy = MapObject_GetY(other!) - y;
                    if (dy < 0)
                    {
                        dy = -dy;
                    }
                    if (dy < ObjectCollisionHeightRange)
                    {
                        return 1;
                    }
                }
            }
            MapObjectManager.MapObjectMan_NextMapObject(ref other);
            remaining--;
        }
        while (remaining != 0);
        return 0;
    }

    // OFFSET 0x02063FAC
    public static int MapObject_IsOutOfRange(MapObject mapObject, int x, int y, int z)
    {
        int initial = MapObject_GetXInitial(mapObject);
        int range = MapObject_GetMovementRangeX(mapObject);
        if (range != -1)
        {
            if (initial - range > x || initial + range < x)
            {
                return 1;
            }
        }
        initial = MapObject_GetZInitial(mapObject);
        range = MapObject_GetMovementRangeZ(mapObject);
        if (range != -1)
        {
            if (initial - range > z || initial + range < z)
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x02064004
    public static int MapObject_IsMovementBlockedByTiles(MapObject mapObject, int x, int z, int dir)
    {
        if (MapObject_IsStatus2Bit2On(mapObject) == 0)
        {
            FieldSystem fieldSystem = MapObject_FieldSystem(mapObject)!;
            int currTileBehavior = (byte)MapObject_GetCurrTileBehavior(mapObject);
            int nextTileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
            if (nextTileBehavior == TileBehavior.TileBehavior_GetNull())
            {
                return 1;
            }
            if (sCurrentTileBlocksMovement[dir](currTileBehavior) == 1
                || sNextTileBlocksMovement[dir](nextTileBehavior) == 1)
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0206406C
    public static int MapObject_IsOnWater(MapObject mapObject, int tileBehavior)
    {
        if (TileBehavior.TileBehavior_IsBridgeOverWater((byte)tileBehavior) != 0)
        {
            if (MapObject_IsStatusOnElevatedBridge(mapObject) == 0)
            {
                return 1;
            }
        }
        else if (TileBehavior.TileBehavior_IsSurfable((byte)tileBehavior) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020640A0
    public static int MapObject_IsOnSand(MapObject mapObject, int tileBehavior)
    {
        if (TileBehavior.TileBehavior_IsBridgeOverSand((byte)tileBehavior) != 0)
        {
            if (MapObject_IsStatusOnElevatedBridge(mapObject) == 0)
            {
                return 1;
            }
        }
        else if (TileBehavior.TileBehavior_IsSand((byte)tileBehavior) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x020640D4
    public static int MapObject_IsOnSnow(MapObject mapObject, int tileBehavior)
    {
        if (TileBehavior.TileBehavior_IsBridgeOverSnow((byte)tileBehavior) != 0)
        {
            if (MapObject_IsStatusOnElevatedBridge(mapObject) == 0)
            {
                return 1;
            }
        }
        else if (TileBehavior.TileBehavior_IsSnow((byte)tileBehavior) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x02064108
    public static int MapObject_IsOnShallowSnow(MapObject mapObject, int tileBehavior)
    {
        if (TileBehavior.TileBehavior_IsBridgeOverSnow((byte)tileBehavior) != 0)
        {
            if (MapObject_IsStatusOnElevatedBridge(mapObject) == 0)
            {
                return 1;
            }
        }
        else if (TileBehavior.TileBehavior_IsShallowSnow((byte)tileBehavior) != 0)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0206413C
    public static int MapObject_IsOnElevatedBridge(MapObject mapObject, int tileBehavior)
    {
        if (MapObject_IsStatusOnElevatedBridge(mapObject) == 1)
        {
            if (TileBehavior.TileBehavior_IsBridge((byte)tileBehavior) == 1)
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0206415C
    public static int MapObject_IsOnBikeBridgeNorthSouth(MapObject mapObject, int tileBehavior)
    {
        if (MapObject_IsStatusOnElevatedBridge(mapObject) == 1)
        {
            if (TileBehavior.TileBehavior_IsBikeBridgeNorthSouth((byte)tileBehavior) == 1)
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0206417C
    public static int MapObject_IsOnBikeBridgeEastWest(MapObject mapObject, int tileBehavior)
    {
        if (MapObject_IsStatusOnElevatedBridge(mapObject) == 1)
        {
            if (TileBehavior.TileBehavior_IsBikeBridgeEastWest((byte)tileBehavior) == 1)
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0206419C
    public static int MapObject_GetDxFromDir(int dir)
    {
        sDirDx ??= DirTable(DirDxAddress);
        return sDirDx[dir];
    }

    // OFFSET 0x020641A8
    public static int MapObject_GetDzFromDir(int dir)
    {
        sDirDz ??= DirTable(DirDzAddress);
        return sDirDz[dir];
    }

    // OFFSET 0x020641B4
    public static void MapObject_StepDir(MapObject mapObject, int dir)
    {
        MapObject_SetXPrev(mapObject, MapObject_GetX(mapObject));
        MapObject_SetYPrev(mapObject, MapObject_GetY(mapObject));
        MapObject_SetZPrev(mapObject, MapObject_GetZ(mapObject));
        MapObject_AddX(mapObject, MapObject_GetDxFromDir(dir));
        MapObject_AddY(mapObject, 0);
        MapObject_AddZ(mapObject, MapObject_GetDzFromDir(dir));
    }

    // OFFSET 0x02064208
    public static void MapObject_UpdateCoords(MapObject mapObject)
    {
        MapObject_SetXPrev(mapObject, MapObject_GetX(mapObject));
        MapObject_SetYPrev(mapObject, MapObject_GetY(mapObject));
        MapObject_SetZPrev(mapObject, MapObject_GetZ(mapObject));
    }

    // OFFSET 0x02064238
    public static int MapObject_GetTileBehaviorFromDir(MapObject mapObject, int dir)
    {
        int x = MapObject_GetX(mapObject) + MapObject_GetDxFromDir(dir);
        int z = MapObject_GetZ(mapObject) + MapObject_GetDzFromDir(dir);
        return TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(MapObject_FieldSystem(mapObject)!, x, z);
    }

    // OFFSET 0x02064270
    public static void MapObject_AddVecToPos(MapObject mapObject, in VecFx32 vec)
    {
        MapObject_CopyPos(mapObject, out VecFx32 position);
        position.X += vec.X;
        position.Y += vec.Y;
        position.Z += vec.Z;
        MapObject_SetPos(mapObject, position);
    }

    // OFFSET 0x020642A4
    public static void MapObject_MovePosInDir(MapObject mapObject, int dir, int distance)
    {
        MapObject_CopyPos(mapObject, out VecFx32 position);
        switch (dir)
        {
            case DirNorth:
                position.Z -= distance;
                break;
            case DirSouth:
                position.Z += distance;
                break;
            case DirWest:
                position.X -= distance;
                break;
            case DirEast:
                position.X += distance;
                break;
        }
        MapObject_SetPos(mapObject, position);
    }

    // OFFSET 0x020642F8
    public static int MapObject_RecalculateObjectHeight(MapObject mapObject)
    {
        MapObject_CopyPos(mapObject, out VecFx32 position);
        VecFx32 newPosition = position;
        if (MapObject_IsHeightCalculationDisabled(mapObject) == 1)
        {
            MapObject_SetStatusFlagOff(mapObject, Status12);
            return 0;
        }
        int dynamicHeight = MapObject_IsDynamicHeightCalculationEnabled(mapObject);
        int result = MapObject_RecalculatePositionHeightEx(MapObject_FieldSystem(mapObject)!, ref newPosition, dynamicHeight);
        if (result == 1)
        {
            position.Y = newPosition.Y;
            MapObject_SetPos(mapObject, position);
            MapObject_SetYPrev(mapObject, MapObject_GetY(mapObject));
            MapObject_SetY(mapObject, (position.Y >> 3) / 0x1000);
            MapObject_SetStatusFlagOff(mapObject, Status12);
        }
        else
        {
            MapObject_SetStatusFlagOn(mapObject, Status12);
        }
        return result;
    }

    // OFFSET 0x02064390
    public static int MapObject_SetTileBehaviors(MapObject mapObject)
    {
        int prevTileBehavior = TileBehavior.TileBehavior_GetNull();
        int currTileBehavior = prevTileBehavior;
        if (MapObject_IsStatus2Bit2On(mapObject) == 0)
        {
            int x = MapObject_GetXPrev(mapObject);
            int z = MapObject_GetZPrev(mapObject);
            FieldSystem fieldSystem = MapObject_FieldSystem(mapObject)!;
            prevTileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
            x = MapObject_GetX(mapObject);
            z = MapObject_GetZ(mapObject);
            currTileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
        }
        MapObject_SetPrevTileBehavior(mapObject, (uint)prevTileBehavior);
        MapObject_SetCurrTileBehavior(mapObject, (uint)currTileBehavior);
        if (TileBehavior.TileBehavior_IsNull(currTileBehavior) == 1)
        {
            MapObject_SetStatusFlagOn(mapObject, Status11);
            return 0;
        }
        MapObject_SetStatusFlagOff(mapObject, Status11);
        return 1;
    }

    // OFFSET 0x02064418
    public static void VecFx32_StepDirection(int dir, ref VecFx32 vec, int value)
    {
        switch (dir)
        {
            case DirNorth:
                vec.Z -= value;
                break;
            case DirSouth:
                vec.Z += value;
                break;
            case DirWest:
                vec.X -= value;
                break;
            case DirEast:
                vec.X += value;
                break;
        }
    }

    // OFFSET 0x02064450
    public static void VecFx32_SetPosFromMapCoords(int x, int z, ref VecFx32 vec)
    {
        vec.X = (x << 16) + 0x8000;
        vec.Z = (z << 16) + 0x8000;
    }

    // OFFSET 0x02064464
    public static void MapObject_MoveIfScripted(MapObject mapObject)
    {
        if ((uint)(MapObject_GetMovementType(mapObject) - MovementTypeScriptedFirst) <= MovementTypeScriptedRange)
        {
            MapObject_CallMovementMove(mapObject);
        }
    }

    // OFFSET 0x0206447C
    public static int Direction_GetOpposite(int dir)
    {
        sDirOpposite ??= DirTable(DirOppositeAddress);
        return sDirOpposite[dir];
    }

    // OFFSET 0x02064488
    public static int GetDirectionBetweenPoints(int xSrc, int zSrc, int xDst, int zDst)
    {
        if (xSrc > xDst)
        {
            return DirWest;
        }
        if (xSrc < xDst)
        {
            return DirEast;
        }
        if (zSrc > zDst)
        {
            return DirNorth;
        }
        return DirSouth;
    }

    // OFFSET 0x020644A4
    public static int MapObject_RecalculatePositionHeight(FieldSystem fieldSystem, ref VecFx32 position)
    {
        int height = TerrainCollisionManager.TerrainCollisionManager_GetHeight(fieldSystem, position.Y, position.X, position.Z, out byte heightSource);
        if (heightSource == HeightSourceNone)
        {
            return 0;
        }
        position.Y = height;
        return 1;
    }

    // OFFSET 0x020644D0
    public static int MapObject_RecalculatePositionHeightEx(FieldSystem fieldSystem, ref VecFx32 position, int dynamicHeightCalculationEnabled)
    {
        int height = TerrainCollisionManager.TerrainCollisionManager_GetHeight(fieldSystem, position.Y, position.X, position.Z, out byte heightSource);
        if (heightSource == HeightSourceNone)
        {
            return 0;
        }
        if (heightSource == HeightSourceDynamic && dynamicHeightCalculationEnabled == 0)
        {
            return 0;
        }
        position.Y = height;
        return 1;
    }

    private static int FootprintType(MapObjectRenderDetails renderDetails)
    {
        return (int)((renderDetails.Flags >> FootprintTypeShift) & FootprintTypeMask);
    }

    private static int ShadowSize(MapObjectRenderDetails renderDetails)
    {
        return (int)((renderDetails.Flags >> ShadowSizeShift) & ShadowSizeMask);
    }

    private static int ReflectionEnabled(MapObjectRenderDetails renderDetails)
    {
        return (int)((renderDetails.Flags >> ReflectionShift) & ReflectionMask);
    }

    private static int[] DirTable(uint address)
    {
        uint[] words = RomData.Arm9U32(address, DirCount);
        int[] table = new int[DirCount];
        for (int i = 0; i < DirCount; i++)
        {
            table[i] = (int)words[i];
        }
        return table;
    }

    private static VecFx32 TerrainOffset(uint address)
    {
        if (!sTerrainOffsets.TryGetValue(address, out VecFx32 offset))
        {
            uint[] words = RomData.Arm9U32(address, 3);
            offset.X = (int)words[0];
            offset.Y = (int)words[1];
            offset.Z = (int)words[2];
            sTerrainOffsets[address] = offset;
        }
        return offset;
    }
}
