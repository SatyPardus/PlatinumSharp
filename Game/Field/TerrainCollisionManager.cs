using System.Runtime.CompilerServices;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate int TerrainGetHeightFunc(FieldSystem fieldSystem, int objectHeight, int objectX, int objectZ, out byte newObjectHeightSource);

public delegate bool TerrainGetTileAttributesFunc(FieldSystem fieldSystem, int tileX, int tileZ, out ushort attributes);

public struct TerrainCollisionHitbox
{
    public int StartX;
    public int StartZ;
    public int EndX;
    public int EndZ;
}

public sealed class TerrainCollisionManager
{
    public const int GetHeightOffset = 0x00;
    public const int GetTileAttributesOffset = 0x04;
    public const int Size = 0x08;

    public const int CalculatedHeightSourceNone = 0;
    public const int CalculatedHeightSourceBdhc = 1;
    public const int CalculatedHeightSourceDynamic = 2;

    private const int Fx32One = 0x10000;
    private const int Fx32Half = 0x8000;
    private const int MapTilesPerSide = 32;
    private const int LoadedMapCount = 4;
    private const int LoadedMapPropCount = 32;
    private const int InvalidTileAttributes = 0xFF;
    private const int InvalidTileBehavior = 0xFF;
    private const int CollisionBitShift = 15;
    private const int MaxHeightDifference = 0x14000;
    private const int MapOriginOffset = 0x100000;
    private const int MapSizeShift = 21;

    public readonly TerrainGetHeightFunc GetHeightFunc;
    public readonly TerrainGetTileAttributesFunc GetTileAttributesFunc;

    private static readonly TerrainCollisionManager sTerrainCollisionManager = new(GetHeight, GetTileAttributes);
    private static readonly TerrainCollisionManager sSimpleTerrainCollisionManager = new(GetSimpleHeight, GetSimpleTileAttributes);

    private TerrainCollisionManager(TerrainGetHeightFunc getHeight, TerrainGetTileAttributesFunc getTileAttributes)
    {
        GetHeightFunc = getHeight;
        GetTileAttributesFunc = getTileAttributes;
    }

    // OFFSET 0x02054D00
    private static int CalculateDistance(int lhs, int rhs)
    {
        if (lhs >= rhs)
        {
            return lhs - rhs;
        }
        return rhs - lhs;
    }

    // OFFSET 0x02054D0C
    private static int GetHeight(FieldSystem fieldSystem, int objectHeight, int objectX, int objectZ, out byte newObjectHeightSource)
    {
        LandDataManager landDataManager = fieldSystem.LandDataManager!;
        LandDataManager.LandDataManager_GetOffset(landDataManager, out VecFx32 offset);
        int relativeHeight = objectHeight - offset.Y;
        VecFx32 relativePosition;
        relativePosition.X = objectX - offset.X;
        relativePosition.Y = 0;
        relativePosition.Z = objectZ - offset.Z;
        int height = 0;
        int mapMatrixWidth = MapMatrix.MapMatrix_GetWidth(fieldSystem.MapMatrix);
        int mapMatrixWidthInTiles = mapMatrixWidth * MapTilesPerSide;
        int tileX = (objectX - offset.X) / Fx32One;
        int tileZ = (objectZ - offset.Z) / Fx32One;
        bool hasDynamicHeight = DynamicTerrainHeightManager.DynamicTerrainHeightManager_GetPlateIndexOfTile(tileX, tileZ, fieldSystem.DynamicTerrainHeightManager, out byte plateIndex) != 0;
        int mapX = (int)((uint)tileX >> 5);
        int mapZ = (int)((uint)tileZ >> 5);
        relativePosition.X = objectX - ((mapX * MapTilesPerSide + MapTilesPerSide / 2) << 16) - offset.X;
        relativePosition.Z = objectZ - ((mapZ * MapTilesPerSide + MapTilesPerSide / 2) << 16) - offset.Z;
        int quadrant = LandDataManager.LandDataManager_CalculateMapQuadrantOfTile((uint)(tileX + mapMatrixWidthInTiles * tileZ), mapMatrixWidthInTiles);
        int loadedMapIndex = LandDataManager.LandDataManager_GetRelativeLoadedMapsQuadrant(mapX + mapMatrixWidth * mapZ, (byte)quadrant, landDataManager);
        bool hasBdhcHeight;
        if ((uint)loadedMapIndex > LoadedMapCount - 1)
        {
            hasBdhcHeight = false;
        }
        else
        {
            BDHC? bdhc = LandDataManager.LandDataManager_GetLoadedMapBDHC(landDataManager, (byte)loadedMapIndex);
            hasBdhcHeight = BDHC.CalculateObjectHeight(relativeHeight, relativePosition.X, relativePosition.Z, bdhc!, out int bdhcHeight);
            if (hasBdhcHeight)
            {
                relativePosition.Y = bdhcHeight;
            }
        }
        byte source;
        if (hasDynamicHeight)
        {
            int dynamicHeight = DynamicTerrainHeightManager.DynamicTerrainHeightManager_GetHeight(plateIndex, fieldSystem.DynamicTerrainHeightManager);
            if (hasBdhcHeight)
            {
                height = relativePosition.Y;
                if (dynamicHeight <= height)
                {
                    source = CalculatedHeightSourceBdhc;
                }
                else
                {
                    int bdhcDistance = CalculateDistance(height, relativeHeight);
                    int dynamicDistance = CalculateDistance(dynamicHeight, relativeHeight);
                    if (bdhcDistance <= dynamicDistance)
                    {
                        height = relativePosition.Y;
                        source = CalculatedHeightSourceBdhc;
                    }
                    else
                    {
                        height = dynamicHeight;
                        source = CalculatedHeightSourceDynamic;
                    }
                }
            }
            else
            {
                source = CalculatedHeightSourceDynamic;
                height = dynamicHeight;
            }
        }
        else if (hasBdhcHeight)
        {
            source = CalculatedHeightSourceBdhc;
            height = relativePosition.Y;
        }
        else
        {
            source = CalculatedHeightSourceNone;
        }
        newObjectHeightSource = source;
        return height + offset.Y;
    }

    // OFFSET 0x02054E50
    private static int GetSimpleHeight(FieldSystem fieldSystem, int objectHeight, int objectX, int objectZ, out byte newObjectHeightSource)
    {
        bool isLoaded = LandDataManager.LandDataManager_GetRelativeLoadedMapsQuadrantOfTile(fieldSystem.LandDataManager, objectX / Fx32One, objectZ / Fx32One, out _);
        if (isLoaded)
        {
            newObjectHeightSource = CalculatedHeightSourceBdhc;
        }
        else
        {
            newObjectHeightSource = CalculatedHeightSourceNone;
        }
        return 0;
    }

    // OFFSET 0x02054E84
    private static bool GetTileAttributes(FieldSystem fieldSystem, int tileX, int tileZ, out ushort attributes)
    {
        LandDataManager landDataManager = fieldSystem.LandDataManager!;
        int relativeTileX = tileX - LandDataManager.LandDataManager_GetOffsetTileX(landDataManager);
        int relativeTileZ = tileZ - LandDataManager.LandDataManager_GetOffsetTileZ(landDataManager);
        if (!LandDataManager.LandDataManager_GetRelativeLoadedMapsQuadrantOfTile(landDataManager, tileX, tileZ, out byte loadedMapIndex))
        {
            attributes = InvalidTileAttributes;
            return false;
        }
        ushort[] terrainAttributes = LandDataManager.LandDataManager_GetLoadedMapTerrainAttributes(landDataManager, loadedMapIndex);
        attributes = terrainAttributes[relativeTileX % MapTilesPerSide + relativeTileZ % MapTilesPerSide * MapTilesPerSide];
        return true;
    }

    // OFFSET 0x02054EF4
    private static bool GetSimpleTileAttributes(FieldSystem fieldSystem, int tileX, int tileZ, out ushort attributes)
    {
        int mapMatrixWidth = MapMatrix.MapMatrix_GetWidth(fieldSystem.MapMatrix);
        int mapMatrixIndex = tileX / MapTilesPerSide + tileZ / MapTilesPerSide * mapMatrixWidth;
        Span<ushort> terrainAttributes = TerrainAttributes.TerrainAttributes_Get((uint)mapMatrixIndex, fieldSystem.TerrainAttributes!);
        attributes = terrainAttributes[tileX % MapTilesPerSide + tileZ % MapTilesPerSide * MapTilesPerSide];
        return true;
    }

    // OFFSET 0x02054F44
    public static void TerrainCollisionManager_Init(out object? terrainCollisionManager, int useSimpleTerrainCollisions)
    {
        Unsafe.SkipInit(out terrainCollisionManager);
        if (useSimpleTerrainCollisions == 0)
        {
            terrainCollisionManager = sTerrainCollisionManager;
        }
        else if (useSimpleTerrainCollisions == 1)
        {
            terrainCollisionManager = sSimpleTerrainCollisionManager;
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02054F68
    public static int TerrainCollisionManager_CheckCollision(FieldSystem fieldSystem, int tileX, int tileZ)
    {
        TerrainCollisionManager manager = (TerrainCollisionManager)fieldSystem.TerrainCollisionManager!;
        if (manager.GetTileAttributesFunc(fieldSystem, tileX, tileZ, out ushort attributes))
        {
            if (((attributes >> CollisionBitShift) & 1) == 1)
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x02054F94
    public static byte TerrainCollisionManager_GetTileBehavior(FieldSystem fieldSystem, int tileX, int tileZ)
    {
        TerrainCollisionManager manager = (TerrainCollisionManager)fieldSystem.TerrainCollisionManager!;
        if (manager.GetTileAttributesFunc(fieldSystem, tileX, tileZ, out ushort attributes))
        {
            return (byte)attributes;
        }
        return InvalidTileBehavior;
    }

    // OFFSET 0x02054FBC
    public static int TerrainCollisionManager_GetHeight(FieldSystem fieldSystem, int objectHeight, int objectX, int objectZ, out byte newObjectHeightSource)
    {
        TerrainCollisionManager manager = (TerrainCollisionManager)fieldSystem.TerrainCollisionManager!;
        return manager.GetHeightFunc(fieldSystem, objectHeight, objectX, objectZ, out newObjectHeightSource);
    }

    // OFFSET 0x02054FD0
    private static int GetVerticalDirection(FieldSystem fieldSystem, ref VecFx32 objectPosition, int tileX, int tileZ, out byte newObjectHeightSource)
    {
        int newHeight = TerrainCollisionManager_GetHeight(fieldSystem, objectPosition.Y, (tileX << 16) + Fx32Half, (tileZ << 16) + Fx32Half, out newObjectHeightSource);
        int direction;
        int upper;
        int lower;
        if (newHeight < objectPosition.Y)
        {
            direction = -1;
            upper = objectPosition.Y;
            lower = newHeight;
        }
        else if (newHeight > objectPosition.Y)
        {
            upper = newHeight;
            lower = objectPosition.Y;
            direction = 1;
        }
        else
        {
            return 0;
        }
        if (upper - lower >= MaxHeightDifference)
        {
            if (direction == 0)
            {
                ErrorHandling.GF_AssertFail();
            }
        }
        else
        {
            direction = 0;
        }
        return direction;
    }

    // OFFSET 0x02055024
    public static int TerrainCollisionManager_WillMapObjectCollide(FieldSystem fieldSystem, ref VecFx32 objectPosition, int tileX, int tileZ, out sbyte verticalDirection)
    {
        int direction = GetVerticalDirection(fieldSystem, ref objectPosition, tileX, tileZ, out byte newObjectHeightSource);
        verticalDirection = (sbyte)direction;
        if (direction != 0)
        {
            return 1;
        }
        int collides = TerrainCollisionManager_CheckCollision(fieldSystem, tileX, tileZ);
        if (collides == 0 && newObjectHeightSource == CalculatedHeightSourceDynamic)
        {
            if (TileBehavior.TileBehavior_HasDynamicHeightCollision(TerrainCollisionManager_GetTileBehavior(fieldSystem, tileX, tileZ)) != 0)
            {
                return 1;
            }
        }
        return collides;
    }

    // OFFSET 0x0205507C
    public static int TerrainCollisionManager_WillPlayerCollide(FieldSystem fieldSystem, ref VecFx32 objectPosition, int tileX, int tileZ, out sbyte verticalDirection)
    {
        int direction = GetVerticalDirection(fieldSystem, ref objectPosition, tileX, tileZ, out byte newObjectHeightSource);
        verticalDirection = (sbyte)direction;
        if (direction != 0)
        {
            return 1;
        }
        int collides = 0;
        if (DynamicMapFeatures.DynamicMapFeatures_CheckCollision(fieldSystem, tileX, tileZ, objectPosition.Y, ref collides) != 0)
        {
            return collides;
        }
        collides = TerrainCollisionManager_CheckCollision(fieldSystem, tileX, tileZ);
        if (collides == 0 && newObjectHeightSource == CalculatedHeightSourceDynamic)
        {
            if (TileBehavior.TileBehavior_HasDynamicHeightCollision(TerrainCollisionManager_GetTileBehavior(fieldSystem, tileX, tileZ)) != 0)
            {
                return 1;
            }
        }
        return collides;
    }

    // OFFSET 0x020550F4
    public static void TerrainCollisionHitbox_Init(int tileX, int tileZ, int offsetTileX, int offsetTileZ, uint sizeX, uint sizeZ, out TerrainCollisionHitbox hitbox)
    {
        int startX = tileX + offsetTileX;
        int startZ = tileZ + offsetTileZ;
        int endX = startX + (int)sizeX;
        int endZ = startZ + (int)sizeZ;
        if (startX < 0 || startZ < 0 || endX < 0 || endZ < 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        hitbox.StartX = startX << 16;
        hitbox.StartZ = startZ << 16;
        hitbox.EndX = endX << 16;
        hitbox.EndZ = endZ << 16;
    }

    // OFFSET 0x02055130
    public static bool TerrainCollisionHitbox_CollidesWithMapProp(object? mapProp, ref TerrainCollisionHitbox hitbox, ref VecFx32 mapAbsoluteOrigin)
    {
        VecFx32 position = MapProps.MapProp_GetPosition((MapProp)mapProp!);
        position.X += mapAbsoluteOrigin.X;
        position.Z += mapAbsoluteOrigin.Z;
        if (hitbox.StartX <= position.X && position.X <= hitbox.EndX && hitbox.StartZ <= position.Z && position.Z <= hitbox.EndZ)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02055178
    public static bool FieldSystem_FindCollidingLoadedMapPropByModelID(FieldSystem fieldSystem, int targetMapPropModelID, ref TerrainCollisionHitbox hitbox, ref object? mapPropOut)
    {
        for (byte i = 0; i < LoadedMapCount; i++)
        {
            LandDataManager.LandDataManager_GetLoadedMapPropManager(i, fieldSystem.LandDataManager!, out MapPropManager? mapPropManager);
            if (mapPropManager == null)
            {
                continue;
            }
            int mapMatrixIndex = LandDataManager.LandDataManager_GetLoadedMapMatrixIndex(fieldSystem.LandDataManager, i);
            VecFx32 mapAbsoluteOrigin = default;
            TerrainCollisionManager_GetMapAbsoluteOrigin(mapMatrixIndex, MapMatrix.MapMatrix_GetWidth(fieldSystem.MapMatrix), ref mapAbsoluteOrigin);
            for (byte j = 0; j < LoadedMapPropCount; j++)
            {
                MapProp mapProp = MapProps.MapPropManager_GetLoadedProp(mapPropManager!, j);
                if (TerrainCollisionHitbox_CollidesWithMapProp(mapProp, ref hitbox, ref mapAbsoluteOrigin) && MapProps.MapProp_GetModelID(mapProp) == targetMapPropModelID)
                {
                    mapPropOut = mapProp;
                    return true;
                }
            }
        }
        return false;
    }

    // OFFSET 0x02055208
    public static bool FieldSystem_FindCollidingLoadedMapPropByModelIDs(FieldSystem fieldSystem, int[] mapPropModelIDs, byte mapPropModelIDsCount, ref TerrainCollisionHitbox hitbox, ref object? mapPropOut, ref int mapPropModelIDOut)
    {
        for (byte i = 0; i < LoadedMapCount; i++)
        {
            LandDataManager.LandDataManager_GetLoadedMapPropManager(i, fieldSystem.LandDataManager!, out MapPropManager? mapPropManager);
            if (mapPropManager == null)
            {
                continue;
            }
            int mapMatrixIndex = LandDataManager.LandDataManager_GetLoadedMapMatrixIndex(fieldSystem.LandDataManager, i);
            VecFx32 mapAbsoluteOrigin = default;
            TerrainCollisionManager_GetMapAbsoluteOrigin(mapMatrixIndex, MapMatrix.MapMatrix_GetWidth(fieldSystem.MapMatrix), ref mapAbsoluteOrigin);
            for (byte j = 0; j < LoadedMapPropCount; j++)
            {
                MapProp mapProp = MapProps.MapPropManager_GetLoadedProp(mapPropManager!, j);
                if (TerrainCollisionHitbox_CollidesWithMapProp(mapProp, ref hitbox, ref mapAbsoluteOrigin))
                {
                    int modelID = MapProps.MapProp_GetModelID(mapProp);
                    for (byte k = 0; k < mapPropModelIDsCount; k++)
                    {
                        if (modelID == mapPropModelIDs[k])
                        {
                            mapPropOut = mapProp;
                            mapPropModelIDOut = modelID;
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    // OFFSET 0x020552B4
    public static bool FieldSystem_FindLoadedMapPropByModelID(FieldSystem fieldSystem, int targetMapPropModelID, ref object? mapPropOut, ref int mapMatrixIndexOut)
    {
        for (byte i = 0; i < LoadedMapCount; i++)
        {
            LandDataManager.LandDataManager_GetLoadedMapPropManager(i, fieldSystem.LandDataManager!, out MapPropManager? mapPropManager);
            if (mapPropManager == null)
            {
                continue;
            }
            for (byte j = 0; j < LoadedMapPropCount; j++)
            {
                MapProp mapProp = MapProps.MapPropManager_GetLoadedProp(mapPropManager!, j);
                if (MapProps.MapProp_GetModelID(mapProp) == targetMapPropModelID)
                {
                    mapPropOut = mapProp;
                    mapMatrixIndexOut = LandDataManager.LandDataManager_GetLoadedMapMatrixIndex(fieldSystem.LandDataManager, i);
                    return true;
                }
            }
        }
        return false;
    }

    // OFFSET 0x02055324
    public static bool FieldSystem_FindLoadedMapPropByModelIDs(FieldSystem fieldSystem, int[] mapPropModelIDs, byte mapPropModelIDsCount, ref object? mapPropOut, ref int mapPropModelIDOut)
    {
        for (byte i = 0; i < LoadedMapCount; i++)
        {
            LandDataManager.LandDataManager_GetLoadedMapPropManager(i, fieldSystem.LandDataManager!, out MapPropManager? mapPropManager);
            if (mapPropManager == null)
            {
                continue;
            }
            for (byte j = 0; j < LoadedMapPropCount; j++)
            {
                MapProp mapProp = MapProps.MapPropManager_GetLoadedProp(mapPropManager!, j);
                int modelID = MapProps.MapProp_GetModelID(mapProp);
                for (byte k = 0; k < mapPropModelIDsCount; k++)
                {
                    if (modelID == mapPropModelIDs[k])
                    {
                        mapPropOut = mapProp;
                        mapPropModelIDOut = modelID;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // OFFSET 0x020553A4
    public static void TerrainCollisionManager_GetMapAbsoluteOrigin(int mapMatrixIndex, int mapMatrixWidth, ref VecFx32 mapAbsoluteOrigin)
    {
        mapAbsoluteOrigin.X = MapOriginOffset;
        mapAbsoluteOrigin.Z = MapOriginOffset;
        mapAbsoluteOrigin.X += (ushort)(mapMatrixIndex % mapMatrixWidth) << MapSizeShift;
        mapAbsoluteOrigin.Z += (ushort)(mapMatrixIndex / mapMatrixWidth) << MapSizeShift;
    }
}
