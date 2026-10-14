using System.Text;

namespace PlatinumSharp.Game;

public static class TestWorld
{
    public const int KeyRight = 0x010;
    public const int KeyLeft = 0x020;
    public const int KeyUp = 0x040;
    public const int KeyDown = 0x080;

    private const int SearchRadius = 70;
    private const int HalfTile = 0x8000;
    private const uint StatusActive = 1;

    private static readonly (int X, int Z)[] neighbours = { (0, -1), (0, 1), (-1, 0), (1, 0) };

    public static readonly HashSet<(int FromX, int FromZ, int ToX, int ToZ)> BlockedEdges = new();

    public static readonly HashSet<(int X, int Z)> KnownTriggerTiles = new();

    public static IEnumerable<MapObject> Objects(FieldSystem field)
    {
        MapObject[]? objects = field.MapObjectManager?.MapObjects;
        if (objects == null)
        {
            yield break;
        }
        foreach (MapObject mapObject in objects)
        {
            if (mapObject != null && MapObject.MapObject_IsActive(mapObject) != 0)
            {
                yield return mapObject;
            }
        }
    }

    public static MapObject? ObjectByLocalID(FieldSystem field, int localID)
    {
        MapObject? player = field.PlayerAvatar?.MapObject;
        int playerX = player != null ? MapObject.MapObject_GetX(player) : 0;
        int playerZ = player != null ? MapObject.MapObject_GetZ(player) : 0;
        MapObject? best = null;
        int bestDistance = int.MaxValue;
        foreach (MapObject mapObject in Objects(field))
        {
            if (mapObject == player || MapObject.MapObject_GetLocalID(mapObject) != localID)
            {
                continue;
            }
            int distance = Math.Abs(MapObject.MapObject_GetX(mapObject) - playerX) + Math.Abs(MapObject.MapObject_GetZ(mapObject) - playerZ);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = mapObject;
            }
        }
        return best;
    }

    private const int NarcLandData = 0x43;
    private const uint LandDataAttributesOffset = 0x10;
    private const int LandDataAttributeCount = 0x400;
    private const int MapBlockSize = 32;
    private const int CollisionBit = 15;
    private const int InvalidBehavior = 0xFF;
    private const int NoLandData = 0xFFFF;
    private static readonly Dictionary<int, ushort[]> landAttributes = new();
    private static NARC? landDataNarc;

    private static ushort[]? LandAttributes(int landDataID)
    {
        if (landAttributes.TryGetValue(landDataID, out ushort[]? cached))
        {
            return cached;
        }
        try
        {
            landDataNarc ??= NARC.NARC_ctor(NarcLandData, 0);
            byte[] buffer = new byte[LandDataAttributeCount * 2];
            NARC.NARC_ReadFromMember(landDataNarc, (uint)landDataID, LandDataAttributesOffset, (uint)buffer.Length, buffer);
            ushort[] attributes = new ushort[LandDataAttributeCount];
            for (int i = 0; i < LandDataAttributeCount; i++)
            {
                attributes[i] = (ushort)(buffer[i * 2] | (buffer[i * 2 + 1] << 8));
            }
            landAttributes[landDataID] = attributes;
            return attributes;
        }
        catch (Exception)
        {
            landAttributes[landDataID] = null!;
            return null;
        }
    }

    public static bool TileAttributes(FieldSystem field, int x, int z, out ushort attributes)
    {
        attributes = 0;
        MapMatrix? matrix = field.MapMatrix;
        if (matrix == null || x < 0 || z < 0)
        {
            return false;
        }
        int width = MapMatrix.MapMatrix_GetWidth(matrix);
        int height = MapMatrix.MapMatrix_GetHeight(matrix);
        if (x / MapBlockSize >= width || z / MapBlockSize >= height)
        {
            return false;
        }
        int landDataID = MapMatrix.MapMatrix_GetLandDataIDByIndex2(x / MapBlockSize + z / MapBlockSize * width, matrix);
        if (landDataID == NoLandData)
        {
            return false;
        }
        ushort[]? block = LandAttributes(landDataID);
        if (block == null)
        {
            return false;
        }
        attributes = block[x % MapBlockSize + z % MapBlockSize * MapBlockSize];
        return true;
    }

    public static int TileBehaviorAt(FieldSystem field, int x, int z)
    {
        return TileAttributes(field, x, z, out ushort attributes) ? attributes & 0xFF : InvalidBehavior;
    }

    public static bool TerrainBlocked(FieldSystem field, int x, int z)
    {
        if (!TileAttributes(field, x, z, out ushort attributes))
        {
            return true;
        }
        return ((attributes >> CollisionBit) & 1) != 0;
    }

    private static bool IsWater(FieldSystem field, int x, int z)
    {
        int behavior = TileBehaviorAt(field, x, z);
        return TileBehavior.TileBehavior_IsSurfable(behavior) != 0 || TileBehavior.TileBehavior_IsWaterfall(behavior) != 0;
    }

    public static bool WaterBlocked(FieldSystem field, int x, int z)
    {
        if (field.PlayerAvatar?.MapObject == null || !IsWater(field, x, z))
        {
            return false;
        }
        return !IsWater(field, PlayerAvatar.PlayerAvatar_GetXPos(field.PlayerAvatar), PlayerAvatar.PlayerAvatar_GetZPos(field.PlayerAvatar));
    }

    public static bool Walkable(FieldSystem field, int x, int z)
    {
        if (x < 0 || z < 0 || TerrainBlocked(field, x, z) || WaterBlocked(field, x, z))
        {
            return false;
        }
        MapObject? player = field.PlayerAvatar?.MapObject;
        foreach (MapObject mapObject in Objects(field))
        {
            if (mapObject != player && MapObject.MapObject_GetX(mapObject) == x && MapObject.MapObject_GetZ(mapObject) == z)
            {
                return false;
            }
        }
        return true;
    }

    public static List<(int X, int Z)>? FindPath(FieldSystem field, int startX, int startZ, Func<int, int, bool> isGoal)
    {
        HashSet<(int, int)> triggers = ActiveTriggerTiles(field);
        List<(int X, int Z)>? direct = FindPath(field, startX, startZ, isGoal, null);
        List<(int X, int Z)>? avoiding = FindPath(field, startX, startZ, isGoal, triggers);
        if (avoiding == null || (direct != null && avoiding.Count > direct.Count * 2 + 8))
        {
            return direct;
        }
        return avoiding;
    }

    public static Dictionary<(int X, int Z), int> Distances(FieldSystem field, int startX, int startZ)
    {
        Dictionary<(int X, int Z), int> distance = new() { [(startX, startZ)] = 0 };
        Dictionary<(int, int), int> heights = new();
        Queue<(int X, int Z)> open = new();
        open.Enqueue((startX, startZ));
        heights[(startX, startZ)] = field.PlayerAvatar != null && field.PlayerAvatar.MapObject != null ? PlayerAvatar.PlayerAvatar_GetPos(field.PlayerAvatar).Y : 0;
        while (open.Count > 0)
        {
            (int X, int Z) current = open.Dequeue();
            foreach ((int X, int Z) step in neighbours)
            {
                if (TryJump(field, current, step, heights[current], out (int X, int Z) landing, out int landingHeight) && !distance.ContainsKey(landing) && !KnownTriggerTiles.Contains(landing))
                {
                    heights[landing] = landingHeight;
                    distance[landing] = distance[current] + 1;
                    open.Enqueue(landing);
                    continue;
                }
                (int X, int Z) next = (current.X + step.X, current.Z + step.Z);
                if (distance.ContainsKey(next) || Math.Abs(next.X - startX) > SearchRadius || Math.Abs(next.Z - startZ) > SearchRadius)
                {
                    continue;
                }
                if (BlockedEdges.Contains((current.X, current.Z, next.X, next.Z)) || !Walkable(field, next.X, next.Z) || KnownTriggerTiles.Contains(next))
                {
                    continue;
                }
                if (!CanStep(field, current.X, current.Z, heights[current], next.X, next.Z, out int nextHeight))
                {
                    continue;
                }
                heights[next] = nextHeight;
                distance[next] = distance[current] + 1;
                open.Enqueue(next);
            }
        }
        return distance;
    }

    public static int NearestAdjacent(Dictionary<(int X, int Z), int> distance, int x, int z)
    {
        int best = int.MaxValue;
        foreach ((int X, int Z) step in neighbours)
        {
            if (distance.TryGetValue((x + step.X, z + step.Z), out int d) && d < best)
            {
                best = d;
            }
        }
        return best;
    }

    private static HashSet<(int, int)> ActiveTriggerTiles(FieldSystem field)
    {
        HashSet<(int, int)> tiles = new();
        if (field.MapHeaderData is MapHeaderData header && header.CoordEvents != null)
        {
            foreach (CoordEvent coordEvent in header.CoordEvents)
            {
                if (FieldSystemVarsFlags.FieldSystem_TryGetVar(field, coordEvent.Var) != coordEvent.Value)
                {
                    continue;
                }
                for (int dz = 0; dz < Math.Max(1, (int)coordEvent.Length); dz++)
                {
                    for (int dx = 0; dx < Math.Max(1, (int)coordEvent.Width); dx++)
                    {
                        tiles.Add((coordEvent.X + dx, coordEvent.Z + dz));
                    }
                }
            }
        }
        return tiles;
    }

    private static List<(int X, int Z)>? FindPath(FieldSystem field, int startX, int startZ, Func<int, int, bool> isGoal, HashSet<(int, int)>? avoid)
    {
        Dictionary<(int, int), (int, int)> cameFrom = new();
        Dictionary<(int, int), int> heights = new();
        Queue<(int X, int Z)> open = new();
        open.Enqueue((startX, startZ));
        cameFrom[(startX, startZ)] = (startX, startZ);
        heights[(startX, startZ)] = field.PlayerAvatar != null && field.PlayerAvatar.MapObject != null ? PlayerAvatar.PlayerAvatar_GetPos(field.PlayerAvatar).Y : 0;
        while (open.Count > 0)
        {
            (int X, int Z) current = open.Dequeue();
            if (isGoal(current.X, current.Z))
            {
                List<(int X, int Z)> path = new();
                (int, int) node = current;
                while (node != (startX, startZ))
                {
                    path.Add(node);
                    node = cameFrom[node];
                }
                path.Add((startX, startZ));
                path.Reverse();
                return path;
            }
            foreach ((int X, int Z) step in neighbours)
            {
                if (TryJump(field, current, step, heights[current], out (int X, int Z) landing, out int landingHeight))
                {
                    bool landingAllowed = !cameFrom.ContainsKey(landing) && !BlockedEdges.Contains((current.X, current.Z, landing.X, landing.Z))
                        && (!KnownTriggerTiles.Contains(landing) || isGoal(landing.X, landing.Z)) && (avoid == null || !avoid.Contains(landing) || isGoal(landing.X, landing.Z));
                    if (landingAllowed)
                    {
                        heights[landing] = landingHeight;
                        cameFrom[landing] = current;
                        open.Enqueue(landing);
                    }
                    continue;
                }
                (int X, int Z) next = (current.X + step.X, current.Z + step.Z);
                if (cameFrom.ContainsKey(next) || Math.Abs(next.X - startX) > SearchRadius || Math.Abs(next.Z - startZ) > SearchRadius)
                {
                    continue;
                }
                if (BlockedEdges.Contains((current.X, current.Z, next.X, next.Z)) || !Walkable(field, next.X, next.Z))
                {
                    continue;
                }
                if (KnownTriggerTiles.Contains(next) && !isGoal(next.X, next.Z))
                {
                    continue;
                }
                if (avoid != null && avoid.Contains(next) && !isGoal(next.X, next.Z))
                {
                    continue;
                }
                if (!CanStep(field, current.X, current.Z, heights[current], next.X, next.Z, out int nextHeight))
                {
                    continue;
                }
                heights[next] = nextHeight;
                cameFrom[next] = current;
                open.Enqueue(next);
            }
        }
        return null;
    }

    private static bool TryJump(FieldSystem field, (int X, int Z) from, (int X, int Z) step, int fromHeight, out (int X, int Z) landing, out int landingHeight)
    {
        landing = from;
        landingHeight = fromHeight;
        int behavior;
        try
        {
            behavior = TileBehaviorAt(field, from.X + step.X, from.Z + step.Z);
        }
        catch (Exception)
        {
            return false;
        }
        int distance;
        if (step == (0, -1))
        {
            distance = TileBehavior.TileBehavior_IsJumpNorth(behavior) != 0 ? 2 : TileBehavior.TileBehavior_IsJumpNorthTwice(behavior) != 0 ? 3 : 0;
        }
        else if (step == (0, 1))
        {
            distance = TileBehavior.TileBehavior_IsJumpSouth(behavior) != 0 ? 2 : TileBehavior.TileBehavior_IsJumpSouthTwice(behavior) != 0 ? 3 : 0;
        }
        else if (step == (-1, 0))
        {
            distance = TileBehavior.TileBehavior_IsJumpWest(behavior) != 0 ? 2 : TileBehavior.TileBehavior_IsJumpWestTwice(behavior) != 0 ? 3 : 0;
        }
        else
        {
            distance = TileBehavior.TileBehavior_IsJumpEast(behavior) != 0 ? 2 : TileBehavior.TileBehavior_IsJumpEastTwice(behavior) != 0 ? 3 : 0;
        }
        if (distance == 0)
        {
            return false;
        }
        landing = (from.X + step.X * distance, from.Z + step.Z * distance);
        if (!Walkable(field, landing.X, landing.Z))
        {
            return false;
        }
        try
        {
            landingHeight = TerrainCollisionManager.TerrainCollisionManager_GetHeight(field, fromHeight, (landing.X << 16) + HalfTile, (landing.Z << 16) + HalfTile, out _);
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    private static bool CanStep(FieldSystem field, int fromX, int fromZ, int fromHeight, int toX, int toZ, out int toHeight)
    {
        toHeight = fromHeight;
        try
        {
            VecFx32 position = default;
            position.X = (fromX << 16) + HalfTile;
            position.Y = fromHeight;
            position.Z = (fromZ << 16) + HalfTile;
            if (TerrainCollisionManager.TerrainCollisionManager_WillPlayerCollide(field, ref position, toX, toZ, out _) != 0)
            {
                return false;
            }
            toHeight = TerrainCollisionManager.TerrainCollisionManager_GetHeight(field, fromHeight, (toX << 16) + HalfTile, (toZ << 16) + HalfTile, out _);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static int WarpPushKey(FieldSystem field, int warpX, int warpZ)
    {
        byte behavior;
        try
        {
            behavior = (byte)TileBehaviorAt(field, warpX, warpZ);
        }
        catch (Exception)
        {
            return 0;
        }
        if (TileBehavior.TileBehavior_IsWarpStairsEast(behavior) != 0 || TileBehavior.TileBehavior_IsWarpEntranceEast(behavior) != 0 || TileBehavior.TileBehavior_IsWarpEast(behavior) != 0)
        {
            return KeyRight;
        }
        if (TileBehavior.TileBehavior_IsWarpStairsWest(behavior) != 0 || TileBehavior.TileBehavior_IsWarpEntranceWest(behavior) != 0 || TileBehavior.TileBehavior_IsWarpWest(behavior) != 0)
        {
            return KeyLeft;
        }
        if (TileBehavior.TileBehavior_IsWarpEntranceSouth(behavior) != 0 || TileBehavior.TileBehavior_IsWarpSouth(behavior) != 0)
        {
            return KeyDown;
        }
        if (TileBehavior.TileBehavior_IsWarpEntranceNorth(behavior) != 0 || TileBehavior.TileBehavior_IsWarpNorth(behavior) != 0 || TileBehavior.TileBehavior_IsDoor(behavior) != 0)
        {
            return KeyUp;
        }
        return 0;
    }

    public static (int X, int Z) Offset(int key)
    {
        switch (key)
        {
            case KeyRight:
                return (1, 0);
            case KeyLeft:
                return (-1, 0);
            case KeyUp:
                return (0, -1);
            case KeyDown:
                return (0, 1);
        }
        return (0, 0);
    }

    public static string Decode(GameString? text)
    {
        if (text == null)
        {
            return "";
        }
        StringBuilder result = new();
        foreach (ushort c in text.Data)
        {
            if (c == GameString.EOS)
            {
                break;
            }
            if (c >= 0x0121 && c <= 0x012A)
            {
                result.Append((char)('0' + c - 0x0121));
            }
            else if (c >= 0x012B && c <= 0x0144)
            {
                result.Append((char)('A' + c - 0x012B));
            }
            else if (c >= 0x0145 && c <= 0x015E)
            {
                result.Append((char)('a' + c - 0x0145));
            }
            else
            {
                switch (c)
                {
                    case 0x01DE:
                        result.Append(' ');
                        break;
                    case 0xE000:
                    case 0x25BC:
                    case 0x25BD:
                        result.Append(" / ");
                        break;
                    case 0x01AB:
                        result.Append('!');
                        break;
                    case 0x01AC:
                        result.Append('?');
                        break;
                    case 0x01AD:
                        result.Append(',');
                        break;
                    case 0x01AE:
                        result.Append('.');
                        break;
                    case 0x01AF:
                        result.Append("...");
                        break;
                    case 0x01B2:
                    case 0x01B3:
                        result.Append('\'');
                        break;
                    case 0x01B4:
                    case 0x01B5:
                        result.Append('"');
                        break;
                    case 0x01BE:
                        result.Append('-');
                        break;
                    case 0x01C4:
                        result.Append(':');
                        break;
                    case 0x0188:
                        result.Append('e');
                        break;
                    default:
                        result.Append($"[{c:X4}]");
                        break;
                }
            }
        }
        return result.ToString();
    }

    public static int KeyToward(int fromX, int fromZ, int toX, int toZ)
    {
        if (toX > fromX)
        {
            return KeyRight;
        }
        if (toX < fromX)
        {
            return KeyLeft;
        }
        if (toZ > fromZ)
        {
            return KeyDown;
        }
        if (toZ < fromZ)
        {
            return KeyUp;
        }
        return 0;
    }

    public static string Describe(FieldSystem field)
    {
        StringBuilder text = new();
        MapObject? player = field.PlayerAvatar?.MapObject;
        foreach (MapObject mapObject in Objects(field))
        {
            if (mapObject == player)
            {
                continue;
            }
            text.AppendLine($"object {MapObject.MapObject_GetLocalID(mapObject)} at ({MapObject.MapObject_GetX(mapObject)},{MapObject.MapObject_GetZ(mapObject)}) gfx {MapObject.MapObject_GetGraphicsID(mapObject)} script {MapObject.MapObject_GetScript(mapObject)} move {MapObject.MapObject_GetMovementType(mapObject)}" + (MapObject.MapObject_GetTrainerType(mapObject) != 0 ? $" trainer {MapObject.MapObject_GetTrainerType(mapObject)}" : ""));
        }
        if (field.MapHeaderData is MapHeaderData header)
        {
            if (header.CoordEvents != null)
            {
                foreach (CoordEvent coordEvent in header.CoordEvents)
                {
                    ushort value = FieldSystemVarsFlags.FieldSystem_TryGetVar(field, coordEvent.Var);
                    string active = value == coordEvent.Value ? "ACTIVE" : $"off (var 0x{coordEvent.Var:X}={value}, needs {coordEvent.Value})";
                    text.AppendLine($"trigger script {coordEvent.Script} at ({coordEvent.X},{coordEvent.Z}) size {coordEvent.Width}x{coordEvent.Length} {active}");
                }
            }
            if (header.BgEvents != null)
            {
                foreach (BgEvent bgEvent in header.BgEvents)
                {
                    text.AppendLine($"sign/bg script {bgEvent.Script} at ({bgEvent.X},{bgEvent.Z}) type {bgEvent.Type}");
                }
            }
        }
        return text.ToString();
    }
}
