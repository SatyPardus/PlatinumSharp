namespace PlatinumSharp.Game;

public static class FieldEvents
{
    public const int ScriptNone = 0xFFFF;

    private const int DirectionNorth = 0;
    private const int DirectionSouth = 1;
    private const int DirectionWest = 2;
    private const int DirectionEast = 3;
    private const int BgEventTypeWallSign = 1;
    private const int BgEventTypeHiddenItem = 2;
    private const int BgEventDirNorth = 0;
    private const int BgEventDirEast = 1;
    private const int BgEventDirWest = 2;
    private const int BgEventDirSouth = 3;
    private const int BgEventDirAny = 4;
    private const int BgEventDirHorizontal = 5;
    private const int BgEventDirVertical = 6;
    private const int GraphicsIDFirstSign = 0x5B;
    private const int SignGraphicsCount = 6;

    // OFFSET 0x0203C954
    public static int BgEvent_GetPlayerFacingPosition(FieldSystem fieldSystem, out int x, out int z)
    {
        int direction = PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!);
        x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        switch (direction)
        {
            case DirectionNorth:
                z--;
                break;
            case DirectionSouth:
                z++;
                break;
            case DirectionWest:
                x--;
                break;
            case DirectionEast:
                x++;
                break;
        }
        return direction;
    }

    // OFFSET 0x0203C9B0
    private static bool PlayerAvatar_IsAtSameHeightAs(PlayerAvatar playerAvatar, MapObject mapObject)
    {
        int playerY = MapObject.MapObject_GetPosYAsTile(PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar));
        if (playerY == MapObject.MapObject_GetPosYAsTile(mapObject))
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0203C9D4
    public static void FieldEvent_GetFacingObject(FieldSystem fieldSystem, out MapObject? mapObject)
    {
        int direction = BgEvent_GetPlayerFacingPosition(fieldSystem, out int x, out int z);
        if (TileBehavior.TileBehavior_IsTable(TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z)) == 1)
        {
            switch (direction)
            {
                case DirectionNorth:
                    z--;
                    break;
                case DirectionSouth:
                    z++;
                    break;
                case DirectionWest:
                    x--;
                    break;
                case DirectionEast:
                    x++;
                    break;
            }
        }
        mapObject = MapObjectManager.MapObjectMan_FindObjectAtTile(fieldSystem.MapObjectManager!, x, z, 0);
    }

    // OFFSET 0x0203CA40
    public static int FieldEvent_GetInteractedObject(FieldSystem fieldSystem, out MapObject? mapObject)
    {
        FieldEvent_GetFacingObject(fieldSystem, out mapObject);
        if (mapObject != null
            && MapObject.MapObject_IsStatus19Off(mapObject) == 1
            && PlayerAvatar_IsAtSameHeightAs(fieldSystem.PlayerAvatar!, mapObject) == true)
        {
            return 1;
        }
        return 0;
    }

    // OFFSET 0x0203CA6C
    public static int FieldEvent_GetInteractedBgEventScript(FieldSystem fieldSystem, BgEvent[]? bgEvents, int bgEventCount)
    {
        BgEvent_GetPlayerFacingPosition(fieldSystem, out int x, out int z);
        for (int i = 0; i < bgEventCount; i++)
        {
            BgEvent bgEvent = bgEvents![i];
            if (x != bgEvent.X || z != bgEvent.Z)
            {
                continue;
            }
            if (bgEvent.Type == BgEventTypeHiddenItem)
            {
                if (BgEvent_IsHiddenItemAvailable(fieldSystem, bgEvent) == true)
                {
                    return bgEvents[i].Script;
                }
            }
            else if (BgEvent_CheckPlayerFacingDirection(fieldSystem, bgEvent) == true)
            {
                return bgEvents[i].Script;
            }
        }
        return ScriptNone;
    }

    // OFFSET 0x0203CAEC
    private static bool BgEvent_IsHiddenItemAvailable(FieldSystem fieldSystem, BgEvent bgEvent)
    {
        if (bgEvent.Type != BgEventTypeHiddenItem)
        {
            return false;
        }
        ushort flag = FieldSystemVarsFlags.Script_GetHiddenItemFlag(bgEvent.Script);
        if (FieldSystemVarsFlags.FieldSystem_CheckFlag(fieldSystem, flag) == true)
        {
            return false;
        }
        return true;
    }

    // OFFSET 0x0203CB18
    private static bool BgEvent_CheckPlayerFacingDirection(FieldSystem fieldSystem, BgEvent bgEvent)
    {
        if (bgEvent.PlayerFacingDir == BgEventDirAny)
        {
            return true;
        }
        switch (PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!))
        {
            case DirectionNorth:
                if (bgEvent.PlayerFacingDir == BgEventDirNorth || bgEvent.PlayerFacingDir == BgEventDirVertical)
                {
                    return true;
                }
                break;
            case DirectionSouth:
                if (bgEvent.PlayerFacingDir == BgEventDirSouth || bgEvent.PlayerFacingDir == BgEventDirVertical)
                {
                    return true;
                }
                break;
            case DirectionWest:
                if (bgEvent.PlayerFacingDir == BgEventDirWest || bgEvent.PlayerFacingDir == BgEventDirHorizontal)
                {
                    return true;
                }
                break;
            case DirectionEast:
                if (bgEvent.PlayerFacingDir == BgEventDirEast || bgEvent.PlayerFacingDir == BgEventDirHorizontal)
                {
                    return true;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x0203CB80
    public static int FieldEvent_GetInteractedWallSignScript(FieldSystem fieldSystem, BgEvent[]? bgEvents, int bgEventCount)
    {
        if (PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!) != DirectionNorth)
        {
            return ScriptNone;
        }
        BgEvent_GetPlayerFacingPosition(fieldSystem, out int x, out int z);
        for (int i = 0; i < bgEventCount; i++)
        {
            BgEvent bgEvent = bgEvents![i];
            if (x == bgEvent.X && z == bgEvent.Z && bgEvent.Type == BgEventTypeWallSign)
            {
                return bgEvents[i].Script;
            }
        }
        return ScriptNone;
    }

    // OFFSET 0x0203CBE0
    public static int FieldEvent_GetInteractedSignObject(FieldSystem fieldSystem, out MapObject? mapObject)
    {
        mapObject = null;
        if (PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!) != DirectionNorth)
        {
            return 0;
        }
        if (FieldEvent_GetInteractedObject(fieldSystem, out mapObject) == 1)
        {
            int graphicsID = MapObject.MapObject_GetGraphicsID(mapObject!);
            if ((uint)(graphicsID - GraphicsIDFirstSign) < SignGraphicsCount)
            {
                return 1;
            }
        }
        return 0;
    }

    // OFFSET 0x0203CC14
    public static int FieldEvent_GetCoordEventScript(FieldSystem fieldSystem, CoordEvent[]? coordEvents, int coordEventCount)
    {
        int x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        int z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        for (int i = 0; i < coordEventCount; i++)
        {
            CoordEvent coordEvent = coordEvents![i];
            if (x >= coordEvent.X
                && x < coordEvent.X + coordEvent.Width
                && z >= coordEvent.Z
                && z < coordEvent.Z + coordEvent.Length
                && coordEvent.Value == FieldSystemVarsFlags.FieldSystem_TryGetVar(fieldSystem, coordEvent.Var))
            {
                return coordEvents[i].Script;
            }
        }
        return ScriptNone;
    }
}
