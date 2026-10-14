using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void ItemMenuUseFunc(ItemMenuUseContext menuContext, ItemUseContext useContext);

public delegate void FieldMoveTaskFunc(FieldMoveTaskContext taskContext, FieldMoveContext moveContext);

public sealed class ItemUseContext
{
    public const int Size = 0x1C;

    public int MapHeaderID;
    public int HasPartner;
    public int PlayerState;
    public ushort FacingTileBehavior;
    public ushort CurrentTileBehavior;
    public ushort BerryPatchFlags;
    public PlayerAvatar? PlayerAvatar;
    public FieldSystem? FieldSystem;

    private const int DirectionNorth = 0;
    private const int DirectionSouth = 1;
    private const int DirectionWest = 2;
    private const int DirectionEast = 3;

    // OFFSET 0x02068B50
    public static bool ItemUseContext_CanPlantBerry(ItemUseContext context)
    {
        return (context.BerryPatchFlags & 1) != 0;
    }

    // OFFSET 0x0206842C
    public static void ItemUseContext_Init(FieldSystem fieldSystem, ItemUseContext context)
    {
        if (PlayerAvatar.PlayerAvatar_DistortionGravityChanged(fieldSystem.PlayerAvatar!) == 1)
        {
            ItemUseContext_InitDistortion(fieldSystem, context);
            return;
        }
        context.FieldSystem = fieldSystem;
        context.MapHeaderID = (int)fieldSystem.Location!.U32(0);
        context.HasPartner = SystemFlags.SystemFlag_CheckHasPartner(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)) ? 1 : 0;
        context.PlayerState = PlayerAvatar.PlayerAvatar_GetPlayerState(fieldSystem.PlayerAvatar!);
        int x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        int z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        context.CurrentTileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
        switch (PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!))
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
        context.FacingTileBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
        FieldEvents.FieldEvent_GetFacingObject(fieldSystem, out MapObject? mapObject);
        context.BerryPatchFlags = (ushort)BerryPatchManager.BerryPatches_GetPatchFlags(fieldSystem, mapObject);
        context.PlayerAvatar = fieldSystem.PlayerAvatar;
    }

    // OFFSET 0x020684D0
    private static void ItemUseContext_InitDistortion(FieldSystem fieldSystem, ItemUseContext context)
    {
        context.FieldSystem = fieldSystem;
        context.MapHeaderID = (int)fieldSystem.Location!.U32(0);
        context.HasPartner = SystemFlags.SystemFlag_CheckHasPartner(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)) ? 1 : 0;
        context.PlayerState = PlayerAvatar.PlayerAvatar_GetPlayerState(fieldSystem.PlayerAvatar!);
        context.CurrentTileBehavior = (ushort)PlayerMove.PlayerAvatar_GetDistortionCurrTileBehaviour(fieldSystem.PlayerAvatar!);
        int direction = PlayerAvatar.PlayerAvatar_GetDistortionDir(fieldSystem.PlayerAvatar!);
        context.FacingTileBehavior = (ushort)PlayerMove.PlayerAvatar_GetDistortionFacingTileBehaviour(fieldSystem.PlayerAvatar!, direction);
        context.BerryPatchFlags = (ushort)BerryPatchManager.BerryPatches_GetPatchFlags(fieldSystem, null);
        context.PlayerAvatar = fieldSystem.PlayerAvatar;
    }
}

public sealed class ItemMenuUseContext
{
    public FieldTask? Task;
    public ushort Item;
    public byte SelectedMonSlot;
}

public sealed class FieldMoveContext
{
    public const int Size = 0x10;
    public const ushort FlagCut = 0x001;
    public const ushort FlagSurf = 0x004;
    public const ushort FlagStrength = 0x008;
    public const ushort FlagDefog = 0x010;
    public const ushort FlagRockSmash = 0x020;
    public const ushort FlagWaterfall = 0x040;
    public const ushort FlagRockClimb = 0x080;
    public const ushort FlagFlash = 0x100;

    public int MapHeaderID;
    public FieldSystem? FieldSystem;
    public MapObject? MapObject;
    public ushort Flags;

    private const int GraphicsIDStrengthBoulder = 0x54;
    private const int GraphicsIDRockSmash = 0x55;
    private const int GraphicsIDCutTree = 0x56;
    private const int WeatherFog = 0x0E;
    private const int WeatherDark = 0x10;

    // OFFSET 0x02070728
    public static void FieldMoveContext_Init(FieldSystem fieldSystem, FieldMoveContext context)
    {
        context.FieldSystem = fieldSystem;
        context.MapHeaderID = (int)fieldSystem.Location!.U32(0);
        context.Flags = 0;
        if (PlayerAvatar.PlayerAvatar_DistortionGravityChanged(fieldSystem.PlayerAvatar!) == 1)
        {
            FieldMoveContext_InitDistortion(fieldSystem, context);
            return;
        }
        FieldEvents.FieldEvent_GetFacingObject(fieldSystem, out MapObject? mapObject);
        context.MapObject = mapObject;
        if (context.MapObject != null)
        {
            switch (MapObject.MapObject_GetGraphicsID(context.MapObject))
            {
                case GraphicsIDStrengthBoulder:
                    context.Flags |= FlagStrength;
                    break;
                case GraphicsIDRockSmash:
                    context.Flags |= FlagRockSmash;
                    break;
                case GraphicsIDCutTree:
                    context.Flags |= FlagCut;
                    break;
            }
        }
        int x = PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar!);
        int z = PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar!);
        int currentBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
        PlayerMove.PlayerAvatar_GetFacingTileCoords(fieldSystem.PlayerAvatar!, out x, out z);
        int facingBehavior = TerrainCollisionManager.TerrainCollisionManager_GetTileBehavior(fieldSystem, x, z);
        if (PlayerFieldMoves.PlayerAvatar_CanUseSurf(fieldSystem.PlayerAvatar!, currentBehavior, facingBehavior) != 0)
        {
            context.Flags |= FlagSurf;
        }
        if (PlayerFieldMoves.PlayerAvatar_CanUseRockClimb(facingBehavior, PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar!)) != 0)
        {
            context.Flags |= FlagRockClimb;
        }
        if (TileBehavior.TileBehavior_IsWaterfall((byte)facingBehavior) != 0)
        {
            context.Flags |= FlagWaterfall;
        }
        int weather = FieldPlayerState.FieldPlayerState_GetWeather(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
        if (weather == WeatherFog)
        {
            context.Flags |= FlagDefog;
        }
        else if (weather == WeatherDark)
        {
            context.Flags |= FlagFlash;
        }
    }

    // OFFSET 0x02070838
    private static void FieldMoveContext_InitDistortion(FieldSystem fieldSystem, FieldMoveContext context)
    {
        int direction = PlayerAvatar.PlayerAvatar_GetDistortionDir(fieldSystem.PlayerAvatar!);
        int facingBehavior = (int)PlayerMove.PlayerAvatar_GetDistortionFacingTileBehaviour(fieldSystem.PlayerAvatar!, direction);
        int currentBehavior = (int)PlayerMove.PlayerAvatar_GetDistortionCurrTileBehaviour(fieldSystem.PlayerAvatar!);
        if (PlayerFieldMoves.PlayerAvatar_CanUseSurf(fieldSystem.PlayerAvatar!, currentBehavior, facingBehavior) == 1)
        {
            context.Flags |= FlagSurf;
        }
    }
}

public sealed class FieldMoveTaskContext
{
    public FieldTask? Task;
    public ushort SelectedMonSlot;
    public ushort FieldMove;
}

public sealed class PokedexOverlayArgs
{
    public Pokedex? Pokedex;
    public TrainerInfo? TrainerInfo;
    public int TimeOfDay;
    public int FullmoonIslandVisible;
    public int NewmoonIslandVisible;
    public int SpringPathVisible;
    public int SeabreakPathVisible;
    public PokedexMemory? PokedexMemory;
}

public sealed class PartyMenu
{
    public const int Size = 0x40;
    public const int MaxPartySize = 6;

    public Party? Party;
    public Bag? Bag;
    public Mailbox? Mailbox;
    public Options? Options;
    public object? Broadcast;
    public object? BattleRegulation;
    public FieldMoveContext? FieldMoveContext;
    public FieldSystem? FieldSystem;
    public byte Mode;
    public byte Type;
    public byte SelectedMonSlot;
    public byte MenuSelectionResult;
    public ushort UsedItemID;
    public ushort LearnedMove;
    public byte SelectedMoveSlot;
    public byte Unknown29;
    public byte ContestType;
    public byte ContestRank;
    public readonly byte[] SelectionOrder = new byte[MaxPartySize];
    public byte SelectionSlots;
    public byte RequiredLevel;
    public int LevelUpMoveIndex;
    public ushort EvoTargetSpecies;
    public int EvoType;
}

public sealed class PokemonSummary
{
    public const int Size = 0x30;

    public object? MonData;
    public Options? Options;
    public ushort[]? OTName;
    public uint OTID;
    public byte OTGender;
    public byte DataType;
    public byte Mode;
    public byte MonMax;
    public byte MonIndex;
    public byte PageFlags;
    public byte SelectedMoveSlot;
    public byte ReturnMode;
    public ushort Move;
    public uint DexMode;
    public object? SpecialRibbons;
    public object? Poffin;
    public object? ChatotCry;
    public int ShowContest;
}

public sealed class TownMapContext
{
    public int PlayerX;
    public int PlayerZ;
    public int TrainerGender;
    public int FlyLocationSelected;
    public int FlyLocationX;
    public int FlyLocationZ;
    public int FlyLocationMapHeader;
}

public sealed class StartMenuSummaryData
{
    public ushort Item;
    public ushort LevelUpMoveIndex;
}

public sealed class StartMenuMailData
{
    public ushort Item;
    public byte PartySlot;
    public byte UsageType;
}

public sealed class StartMenuEvolutionData
{
    public byte PartySlot;
    public byte EvolutionSource;
    public ushort Method;
    public ushort TargetSpecies;
    public int EvoType;
}

public sealed class StartMenuPartySlot
{
    public int Slot;
}
