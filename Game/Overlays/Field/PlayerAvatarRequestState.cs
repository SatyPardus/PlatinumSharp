using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void PlayerAvatarRequestStateFunc(PlayerAvatar playerAvatar);

public static class PlayerAvatarRequestState
{
    private const int RequestStateCount = 10;
    private const uint RequestStateWalking = 1 << 0;
    private const uint RequestStateRefreshWalking = 1 << 6;
    private const int PlayerStateWalking = 0;
    private const int PlayerStateCycling = 1;
    private const int PlayerStateSurfing = 2;
    private const int SpriteStateDistortionWalking = 0x18;
    private const int SpriteStateDistortionSurfing = 0x19;
    private const int SpriteStateFeatureWalking = 0x1C;
    private const int DynamicMapFeaturesID = 2;
    private const int SoundEffectCycling = 0x618;
    private const int GraphicsIDForm1 = 0xC4;
    private const int GraphicsIDForm2 = 0xC5;
    private const int GraphicsIDForm3 = 0x10E;
    private const int GraphicsIDForm4 = 0x10F;
    private const int GraphicsIDForm5 = 0x112;
    private const int GraphicsIDForm6 = 0x113;

    private static readonly PlayerAvatarRequestStateFunc[] sRequestStateFuncs =
    {
        RequestState_Walking,
        RequestState_Cycling,
        RequestState_Surfing,
        RequestState_IgnoreTileBehavior,
        RequestState_Sprite11,
        RequestState_Sprite13,
        RequestState_Sprite14,
        RequestState_Sprite15,
        RequestState_Sprite16,
        RequestState_Sprite17,
    };

    // OFFSET 0x021DFB54
    public static void PlayerAvatar_RequestState(PlayerAvatar playerAvatar, uint flags)
    {
        PlayerAvatar.PlayerAvatar_TurnOnRequestStateBit(playerAvatar, flags);
    }

    // OFFSET 0x021DFB5C
    public static void PlayerAvatar_ProcessRequestedStates(PlayerAvatar playerAvatar)
    {
        uint flags = PlayerAvatar.PlayerAvatar_GetRequestStateFlag(playerAvatar);
        for (uint i = 0; i < RequestStateCount; i++)
        {
            if ((flags & 1) != 0)
            {
                sRequestStateFuncs[i](playerAvatar);
            }
            flags >>= 1;
        }
        PlayerAvatar.PlayerAvatar_SetRequestStateFlag(playerAvatar, 0);
    }

    // OFFSET 0x021DFB90
    internal static void RequestState_Walking(PlayerAvatar playerAvatar)
    {
        int spriteState = 0;
        if (PlayerAvatar.PlayerAvatar_MapDistortionState(playerAvatar) != 0)
        {
            spriteState = SpriteStateDistortionWalking;
        }
        else if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(MapObject.MapObject_FieldSystem(PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar))!, DynamicMapFeaturesID) == 1)
        {
            spriteState = SpriteStateFeatureWalking;
        }
        int graphicsID = PlayerAvatar.Player_GetSpriteFromStateAndGender(spriteState, PlayerAvatar.PlayerAvatar_GetGender(playerAvatar));
        PlayerAvatar_SetGraphics(playerAvatar, graphicsID);
        PlayerAvatar.PlayerAvatar_SetPlayerState(playerAvatar, PlayerStateWalking);
        PlayerAvatar.PlayerAvatar_ClearSpeed(playerAvatar);
        if (PlayerAvatar.PlayerAvatar_GetSurfMountAnimManager(playerAvatar) != null)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager((OverworldAnimManager?)PlayerAvatar.PlayerAvatar_GetSurfMountAnimManager(playerAvatar));
        }
        PlayerAvatar.PlayerAvatar_SetSurfMountAnimManager(playerAvatar, null);
    }

    // OFFSET 0x021DFBF4
    private static void RequestState_Cycling(PlayerAvatar playerAvatar)
    {
        int graphicsID = PlayerAvatar.Player_GetSpriteFromStateAndGender(PlayerStateCycling, PlayerAvatar.PlayerAvatar_GetGender(playerAvatar));
        PlayerAvatar_SetGraphics(playerAvatar, graphicsID);
        PlayerAvatar.PlayerAvatar_SetPlayerState(playerAvatar, PlayerStateCycling);
        PlayerAvatar.PlayerAvatar_ClearSpeed(playerAvatar);
        if (PlayerAvatar.PlayerAvatar_GetSurfMountAnimManager(playerAvatar) != null)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager((OverworldAnimManager?)PlayerAvatar.PlayerAvatar_GetSurfMountAnimManager(playerAvatar));
        }
        PlayerAvatar.PlayerAvatar_SetSurfMountAnimManager(playerAvatar, null);
        SoundPlayback.Sound_PlayEffect(SoundEffectCycling);
    }

    // OFFSET 0x021DFC3C
    private static void RequestState_Surfing(PlayerAvatar playerAvatar)
    {
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar);
        MapObject.MapObject_FieldSystem(mapObject);
        int distortionState = PlayerAvatar.PlayerAvatar_MapDistortionState(playerAvatar);
        if (PlayerAvatar.PlayerAvatar_GetSurfMountAnimManager(playerAvatar) != null)
        {
            FieldEffects.FieldEffectManager_FinishAnimManager((OverworldAnimManager?)PlayerAvatar.PlayerAvatar_GetSurfMountAnimManager(playerAvatar));
        }
        PlayerAvatar.PlayerAvatar_SetSurfMountAnimManager(playerAvatar, null);
        int direction;
        if (PlayerAvatar.PlayerAvatar_DistortionStateOnFloor(playerAvatar) == 1)
        {
            direction = PlayerAvatar.PlayerAvatar_GetFacingDir(playerAvatar);
        }
        else
        {
            direction = PlayerAvatar.PlayerAvatar_GetMovingDir(playerAvatar);
        }
        object? surfMount;
        int spriteState;
        if (distortionState == 0)
        {
            surfMount = SurfMount.SurfMountRenderer_HandleSurfBegin(mapObject, 0, 0, direction, 1);
            spriteState = PlayerStateSurfing;
        }
        else
        {
            surfMount = ov5_021F85BC(playerAvatar, 0, 0, 0, direction, 1, distortionState);
            spriteState = SpriteStateDistortionSurfing;
        }
        PlayerAvatar.PlayerAvatar_SetSurfMountAnimManager(playerAvatar, surfMount);
        int graphicsID = PlayerAvatar.Player_GetSpriteFromStateAndGender(spriteState, PlayerAvatar.PlayerAvatar_GetGender(playerAvatar));
        PlayerAvatar_SetGraphics(playerAvatar, graphicsID);
        PlayerAvatar.PlayerAvatar_SetPlayerState(playerAvatar, PlayerStateSurfing);
        PlayerAvatar.PlayerAvatar_ClearSpeed(playerAvatar);
    }

    // OFFSET 0x021DFCE4
    private static void RequestState_IgnoreTileBehavior(PlayerAvatar playerAvatar)
    {
        PlayerAvatar.PlayerAvatar_SetIgnoreTileBehavior(playerAvatar, 1);
    }

    // OFFSET 0x021DFCF0
    private static void RequestState_Sprite11(PlayerAvatar playerAvatar)
    {
        int graphicsID = PlayerAvatar.Player_GetSpriteFromStateAndGender(0x11, PlayerAvatar.PlayerAvatar_GetGender(playerAvatar));
        PlayerAvatar_SetGraphics(playerAvatar, graphicsID);
    }

    // OFFSET 0x021DFD0C
    private static void RequestState_Sprite13(PlayerAvatar playerAvatar)
    {
        int graphicsID = PlayerAvatar.Player_GetSpriteFromStateAndGender(0x13, PlayerAvatar.PlayerAvatar_GetGender(playerAvatar));
        PlayerAvatar_SetGraphics(playerAvatar, graphicsID);
    }

    // OFFSET 0x021DFD28
    private static void RequestState_Sprite14(PlayerAvatar playerAvatar)
    {
        int spriteState = 0x14;
        if (PlayerAvatar.PlayerAvatar_MapDistortionState(playerAvatar) != 0)
        {
            spriteState = 0x1B;
        }
        else if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(MapObject.MapObject_FieldSystem(PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar))!, DynamicMapFeaturesID) == 1)
        {
            spriteState = 0x1E;
        }
        int graphicsID = PlayerAvatar.Player_GetSpriteFromStateAndGender(spriteState, PlayerAvatar.PlayerAvatar_GetGender(playerAvatar));
        PlayerAvatar_SetGraphics(playerAvatar, graphicsID);
    }

    // OFFSET 0x021DFD68
    private static void RequestState_Sprite15(PlayerAvatar playerAvatar)
    {
        int spriteState = 0x15;
        if (PlayerAvatar.PlayerAvatar_MapDistortionState(playerAvatar) != 0)
        {
            spriteState = 0x1A;
        }
        else if (PersistedMapFeaturesInit.PersistedMapFeatures_IsCurrentDynamicMap(MapObject.MapObject_FieldSystem(PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar))!, DynamicMapFeaturesID) == 1)
        {
            spriteState = 0x1D;
        }
        int graphicsID = PlayerAvatar.Player_GetSpriteFromStateAndGender(spriteState, PlayerAvatar.PlayerAvatar_GetGender(playerAvatar));
        PlayerAvatar_SetGraphics(playerAvatar, graphicsID);
    }

    // OFFSET 0x021DFDA8
    private static void RequestState_Sprite16(PlayerAvatar playerAvatar)
    {
        int graphicsID = PlayerAvatar.Player_GetSpriteFromStateAndGender(0x16, PlayerAvatar.PlayerAvatar_GetGender(playerAvatar));
        PlayerAvatar_SetGraphics(playerAvatar, graphicsID);
    }

    // OFFSET 0x021DFDC4
    private static void RequestState_Sprite17(PlayerAvatar playerAvatar)
    {
        int graphicsID = PlayerAvatar.Player_GetSpriteFromStateAndGender(0x17, PlayerAvatar.PlayerAvatar_GetGender(playerAvatar));
        PlayerAvatar_SetGraphics(playerAvatar, graphicsID);
    }

    // OFFSET 0x021E0E94
    public static void PlayerAvatar_RefreshWalkingGraphics(PlayerAvatar playerAvatar)
    {
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar);
        if (MapObject.MapObject_IsDrawActive(mapObject) == 0)
        {
            return;
        }
        if (PlayerAvatar.PlayerAvatar_GetPlayerState(playerAvatar) != PlayerStateWalking)
        {
            return;
        }
        int graphicsID = MapObject.MapObject_GetGraphicsID(mapObject);
        if (graphicsID == GraphicsIDForm1 || graphicsID == GraphicsIDForm2 || (uint)(graphicsID - GraphicsIDForm3) <= 1)
        {
            return;
        }
        uint savedFlags = PlayerAvatar.PlayerAvatar_GetRequestStateFlag(playerAvatar);
        PlayerAvatar_RequestState(playerAvatar, RequestStateRefreshWalking);
        PlayerAvatar_ProcessRequestedStates(playerAvatar);
        PlayerAvatar_RequestState(playerAvatar, savedFlags);
    }

    // OFFSET 0x021E0EEC
    public static void PlayerAvatar_RestoreWalkingGraphics(PlayerAvatar playerAvatar)
    {
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar);
        if (MapObject.MapObject_IsDrawActive(mapObject) == 0)
        {
            return;
        }
        if (PlayerAvatar.PlayerAvatar_GetPlayerState(playerAvatar) != PlayerStateWalking)
        {
            return;
        }
        int graphicsID = MapObject.MapObject_GetGraphicsID(mapObject);
        if (graphicsID == GraphicsIDForm1 || graphicsID == GraphicsIDForm2 || graphicsID == GraphicsIDForm3
            || graphicsID == GraphicsIDForm4 || graphicsID == GraphicsIDForm5 || graphicsID == GraphicsIDForm6)
        {
            uint savedFlags = PlayerAvatar.PlayerAvatar_GetRequestStateFlag(playerAvatar);
            PlayerAvatar_RequestState(playerAvatar, RequestStateWalking);
            PlayerAvatar_ProcessRequestedStates(playerAvatar);
            PlayerAvatar_RequestState(playerAvatar, savedFlags);
        }
    }

    // OFFSET 0x021E106C
    public static void PlayerAvatar_SetGraphics(PlayerAvatar playerAvatar, int graphicsID)
    {
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar);
        if (PlayerAvatar.PlayerAvatar_MapDistortionState(playerAvatar) == 0)
        {
            MapObjectRenderResources.MapObject_ChangeGraphicsKeepingBillboard(mapObject, graphicsID, null, null);
            return;
        }
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(mapObject)!;
        object? savedState = ov9_0224A520(fieldSystem, mapObject);
        Billboard? billboard = MapObjectRenderResources.MapObject_ChangeGraphicsKeepingBillboard(mapObject, graphicsID, PlayerAvatar_DistortionBillboardCallback, playerAvatar);
        if (billboard != null)
        {
            ov9_0224A558(fieldSystem, billboard, savedState);
        }
    }

    // OFFSET 0x021E10C0
    private static void PlayerAvatar_DistortionBillboardCallback(object? param, Billboard billboard)
    {
        PlayerAvatar playerAvatar = (PlayerAvatar)param!;
        FieldSystem fieldSystem = MapObject.MapObject_FieldSystem(PlayerAvatar.PlayerAvatar_GetMapObject(playerAvatar))!;
        ov9_0224A564(fieldSystem, billboard);
    }
}
