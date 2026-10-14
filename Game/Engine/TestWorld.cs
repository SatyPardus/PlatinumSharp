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

    public static bool TerrainBlocked(FieldSystem field, int x, int z)
    {
        try
        {
            return TerrainCollisionManager.TerrainCollisionManager_CheckCollision(field, x, z) != 0;
        }
        catch (Exception)
        {
            return true;
        }
    }

    public static bool Walkable(FieldSystem field, int x, int z)
    {
        if (x < 0 || z < 0 || TerrainBlocked(field, x, z))
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
        return FindPath(field, startX, startZ, isGoal, triggers) ?? FindPath(field, startX, startZ, isGoal, null);
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
                (int X, int Z) next = (current.X + step.X, current.Z + step.Z);
                if (cameFrom.ContainsKey(next) || Math.Abs(next.X - startX) > SearchRadius || Math.Abs(next.Z - startZ) > SearchRadius)
                {
                    continue;
                }
                if (BlockedEdges.Contains((current.X, current.Z, next.X, next.Z)) || !Walkable(field, next.X, next.Z))
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
            behavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(field, warpX, warpZ);
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
