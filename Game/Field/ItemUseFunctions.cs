namespace PlatinumSharp.Game;

public delegate int ItemCheckUseFunc(ItemUseContext context);

public static class ItemUseFunctions
{
    private const uint ItemUseFuncs = 0x020EF79C;
    private const int ItemUseFuncSize = 0xC;
    private const int CheckResultOk = 0;
    private const int CheckResultFail = -1;
    private const int CheckResultCycling = 1;
    private const int CheckResultHasPartner = 2;
    private const int CheckResultUnderground = 3;
    private const int CheckResultNoMulchSpot = -3;
    private const int CheckResultNoSprayDuckSpot = -5;
    private const int PlayerStateCycling = 1;
    private const int PlayerStateSurfing = 2;
    private const int BerryPatchCanMulch = 2;
    private const int BerryPatchCanWater = 4;
    private const int MapUndergroundFirst = 0x23D;
    private const int MapUndergroundCount = 10;
    private const int DistributionEventArceus = 2;
    private const uint CanUseExplorerKitAddress = 0x0206877D;
    private const uint CanUseBicycleAddress = 0x02068949;
    private const uint CanUseBerryAddress = 0x02068B19;
    private const uint CanUsePokeRadarAddress = 0x02068C5D;
    private const uint CanUseSprayDuckAddress = 0x02068CB5;
    private const uint CanUseMulchAddress = 0x02068CE1;
    private const uint CanUseVsSeekerAddress = 0x02068D69;
    private const uint CanUseFishingRodAddress = 0x02068E95;
    private const uint CanUseEscapeRopeAddress = 0x020690C5;
    private const uint CanUseAzureFluteAddress = 0x02069131;

    private static VarsFlags ContextVarsFlags(ItemUseContext context)
    {
        return VarsFlags.SaveData_GetVarsFlags(context.FieldSystem!.SaveData!);
    }

    // OFFSET 0x020683F4
    public static Delegate? GetItemUseFunction(int funcType, ushort functionIdx)
    {
        uint address = RomData.Arm9U32((uint)(ItemUseFuncs + functionIdx * ItemUseFuncSize + funcType * 4), 1)[0];
        if (address == 0)
        {
            return null;
        }
        if (funcType == 2)
        {
            return CheckFunction(address);
        }
        if (funcType == 0)
        {
            return MenuUseFunction(address);
        }
        throw Unported.Missing(address);
    }

    private const uint UseHealingItemFromMenuAddress = 0x02068631;
    private const uint UseTMHMFromMenuAddress = 0x02068A35;
    private const uint UseEvoStoneFromMenuAddress = 0x02068FED;
    private const int HeapIDFieldTask = 0xB;

    private static ItemMenuUseFunc MenuUseFunction(uint address)
    {
        switch (address)
        {
            case UseHealingItemFromMenuAddress:
                return UseHealingItemFromMenu;
            case UseTMHMFromMenuAddress:
                return UseTMHMFromMenu;
            case UseEvoStoneFromMenuAddress:
                return UseEvoStoneFromMenu;
        }
        throw Unported.Missing(address);
    }

    private static PartyMenu NewItemPartyMenu(FieldSystem fieldSystem, StartMenu menu, ItemMenuUseContext context, int mode)
    {
        Heap.Heap_Alloc(HeapIDFieldTask, PartyMenu.Size);
        PartyMenu partyMenu = new();
        partyMenu.Party = Party.SaveData_GetParty(fieldSystem.SaveData!);
        partyMenu.Bag = BagItems.SaveData_GetBag(fieldSystem.SaveData!);
        partyMenu.Mailbox = Mailbox.SaveData_GetMailbox(fieldSystem.SaveData!);
        partyMenu.Options = PlayerSave.SaveData_GetOptions(fieldSystem.SaveData!);
        partyMenu.FieldMoveContext = menu.FieldMoveContext;
        partyMenu.Type = 0;
        partyMenu.Mode = (byte)mode;
        partyMenu.FieldSystem = fieldSystem;
        partyMenu.UsedItemID = context.Item;
        partyMenu.SelectedMonSlot = context.SelectedMonSlot;
        return partyMenu;
    }

    private static void StartItemPartyMenu(FieldSystem fieldSystem, StartMenu menu, PartyMenu partyMenu)
    {
        FieldSystemCore.FieldSystem_StartChildProcess(fieldSystem, ApplicationTemplate.At(PartyMenus.PartyMenuTemplate), partyMenu);
        menu.TaskData = partyMenu;
        StartMenu.StartMenu_SetCallback(menu, StartMenu.StartMenu_ExitPartyMenu);
    }

    // OFFSET 0x02068630
    private static void UseHealingItemFromMenu(ItemMenuUseContext context, ItemUseContext useContext)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(context.Task!);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(context.Task!)!;
        PartyMenu partyMenu = NewItemPartyMenu(fieldSystem, menu, context, PartyMenus.ModeUseItem);
        partyMenu.Broadcast = SaveDataTable.SaveData_GetTVBroadcast(fieldSystem.SaveData!);
        StartItemPartyMenu(fieldSystem, menu, partyMenu);
    }

    // OFFSET 0x02068A34
    private static void UseTMHMFromMenu(ItemMenuUseContext context, ItemUseContext useContext)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(context.Task!);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(context.Task!)!;
        PartyMenu partyMenu = NewItemPartyMenu(fieldSystem, menu, context, PartyMenus.ModeTeachMove);
        partyMenu.LearnedMove = Item.Item_MoveForTMHM(context.Item);
        StartItemPartyMenu(fieldSystem, menu, partyMenu);
    }

    // OFFSET 0x02068FEC
    private static void UseEvoStoneFromMenu(ItemMenuUseContext context, ItemUseContext useContext)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(context.Task!);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(context.Task!)!;
        PartyMenu partyMenu = NewItemPartyMenu(fieldSystem, menu, context, PartyMenus.ModeUseEvoItem);
        partyMenu.Broadcast = SaveDataTable.SaveData_GetTVBroadcast(fieldSystem.SaveData!);
        StartItemPartyMenu(fieldSystem, menu, partyMenu);
    }

    private static ItemCheckUseFunc CheckFunction(uint address)
    {
        switch (address)
        {
            case CanUseExplorerKitAddress:
                return CanUseExplorerKit;
            case CanUseBicycleAddress:
                return CanUseBicycle;
            case CanUseBerryAddress:
                return CanUseBerry;
            case CanUsePokeRadarAddress:
                return CanUsePokeRadar;
            case CanUseSprayDuckAddress:
                return CanUseSprayDuck;
            case CanUseMulchAddress:
                return CanUseMulch;
            case CanUseVsSeekerAddress:
                return CanUseVsSeeker;
            case CanUseFishingRodAddress:
                return CanUseFishingRod;
            case CanUseEscapeRopeAddress:
                return CanUseEscapeRope;
            case CanUseAzureFluteAddress:
                return CanUseAzureFlute;
        }
        throw Unported.Missing(address);
    }

    // OFFSET 0x0206877C
    private static int CanUseExplorerKit(ItemUseContext context)
    {
        if (MapHeader.MapHeader_GetMapLabelTextID(context.MapHeaderID) == 0)
        {
            return CheckResultFail;
        }
        if (!MapHeader.MapHeader_IsOnMainMatrix(context.MapHeaderID))
        {
            return CheckResultFail;
        }
        if (PlayerAvatar.PlayerAvatar_IsOnCyclingRoad(context.PlayerAvatar!) == 1)
        {
            return CheckResultFail;
        }
        if (SystemFlags.SystemFlag_CheckSafariGameActive(ContextVarsFlags(context)) || SystemFlags.SystemFlag_CheckInPalPark(ContextVarsFlags(context)))
        {
            return CheckResultFail;
        }
        if (PlayerAvatar.PlayerAvatar_GetPlayerState(context.PlayerAvatar!) == PlayerStateSurfing)
        {
            return CheckResultFail;
        }
        if (TileBehavior.TileBehavior_IsBridge((byte)context.CurrentTileBehavior) == 1)
        {
            return CheckResultFail;
        }
        if (TileBehavior.TileBehavior_ForbidsExplorationKit((byte)context.CurrentTileBehavior) == 1)
        {
            return CheckResultFail;
        }
        ushort x = (ushort)PlayerAvatar.PlayerAvatar_GetXPos(context.FieldSystem!.PlayerAvatar!);
        ushort z = (ushort)PlayerAvatar.PlayerAvatar_GetZPos(context.FieldSystem.PlayerAvatar!);
        if (!MapHeaderData.MapHeaderData_IsPosFreeOfObjectEvents(context.FieldSystem, x, z))
        {
            return CheckResultFail;
        }
        return CheckResultOk;
    }

    // OFFSET 0x02068948
    private static int CanUseBicycle(ItemUseContext context)
    {
        VarsFlags varsFlags = ContextVarsFlags(context);
        if (context.HasPartner == 1)
        {
            return CheckResultHasPartner;
        }
        if (SystemFlags.SystemFlag_HandleForceBikingInGate(varsFlags, 2))
        {
            return CheckResultCycling;
        }
        if (PlayerAvatar.PlayerAvatar_IsOnCyclingRoad(context.PlayerAvatar!) == 1)
        {
            return CheckResultCycling;
        }
        MapObject mapObject = PlayerAvatar.PlayerAvatar_GetMapObject(context.PlayerAvatar!);
        if (MapObjectMove.MapObject_IsOnBikeBridgeNorthSouth(mapObject, context.CurrentTileBehavior) == 1 || MapObjectMove.MapObject_IsOnBikeBridgeEastWest(mapObject, context.CurrentTileBehavior) == 1)
        {
            return CheckResultCycling;
        }
        byte tile = (byte)context.CurrentTileBehavior;
        if (TileBehavior.TileBehavior_IsVeryTallGrass(tile) == 1 || TileBehavior.TileBehavior_IsMud(tile) == 1 || TileBehavior.TileBehavior_IsMudWithGrass(tile) == 1)
        {
            return CheckResultFail;
        }
        if (!MapHeader.MapHeader_IsBikeAllowed(context.MapHeaderID))
        {
            return CheckResultFail;
        }
        if (context.PlayerState == PlayerStateSurfing)
        {
            return CheckResultFail;
        }
        return CheckResultOk;
    }

    // OFFSET 0x02068B18
    private static int CanUseBerry(ItemUseContext context)
    {
        return CheckResultOk;
    }

    // OFFSET 0x02068C5C
    private static int CanUsePokeRadar(ItemUseContext context)
    {
        if (context.HasPartner == 1)
        {
            return CheckResultHasPartner;
        }
        if (PlayerAvatar.PlayerAvatar_GetPlayerState(context.FieldSystem!.PlayerAvatar!) == PlayerStateCycling)
        {
            return CheckResultFail;
        }
        if (TileBehavior.TileBehavior_IsTallGrass((byte)context.CurrentTileBehavior) != 0)
        {
            return CheckResultOk;
        }
        return CheckResultFail;
    }

    // OFFSET 0x02068CB4
    private static int CanUseSprayDuck(ItemUseContext context)
    {
        if (context.HasPartner == 1)
        {
            return CheckResultHasPartner;
        }
        if ((context.BerryPatchFlags & BerryPatchCanWater) != 0)
        {
            return CheckResultOk;
        }
        return CheckResultNoSprayDuckSpot;
    }

    // OFFSET 0x02068CE0
    private static int CanUseMulch(ItemUseContext context)
    {
        if ((context.BerryPatchFlags & BerryPatchCanMulch) != 0)
        {
            return CheckResultOk;
        }
        return CheckResultNoMulchSpot;
    }

    // OFFSET 0x02068D68
    private static int CanUseVsSeeker(ItemUseContext context)
    {
        if (MapHeader.MapHeader_IsOnMainMatrix(context.MapHeaderID))
        {
            return CheckResultOk;
        }
        return CheckResultFail;
    }

    // OFFSET 0x02068E94
    private static int CanUseFishingRod(ItemUseContext context)
    {
        if (context.HasPartner == 1)
        {
            return CheckResultHasPartner;
        }
        if ((uint)(context.MapHeaderID - MapUndergroundFirst) <= MapUndergroundCount)
        {
            return CheckResultUnderground;
        }
        if (TileBehavior.TileBehavior_IsSurfable((byte)context.FacingTileBehavior) != 1)
        {
            return CheckResultFail;
        }
        byte tile = (byte)context.CurrentTileBehavior;
        if ((TileBehavior.TileBehavior_IsBridge(tile) == 1 || TileBehavior.TileBehavior_IsBridgeStart(tile) == 1) && MapObject.MapObject_IsStatusOnElevatedBridge(PlayerAvatar.PlayerAvatar_GetMapObject(context.PlayerAvatar!)) == 1)
        {
            return CheckResultFail;
        }
        return CheckResultOk;
    }

    // OFFSET 0x020690C4
    private static int CanUseEscapeRope(ItemUseContext context)
    {
        if (context.HasPartner == 1)
        {
            return CheckResultHasPartner;
        }
        if (MapHeader.MapHeader_IsCave(context.MapHeaderID) && MapHeader.MapHeader_IsEscapeRopeAllowed(context.MapHeaderID))
        {
            return CheckResultOk;
        }
        return CheckResultFail;
    }

    // OFFSET 0x02069130
    private static int CanUseAzureFlute(ItemUseContext context)
    {
        VarsFlags varsFlags = ContextVarsFlags(context);
        if (!SystemFlags.SystemFlag_CheckGameCompleted(varsFlags))
        {
            return CheckResultFail;
        }
        if (!SystemVars.SystemVars_CheckDistributionEvent(varsFlags, DistributionEventArceus))
        {
            return CheckResultFail;
        }
        if (!Pokedex.Pokedex_IsNationalDexObtained(Pokedex.SaveData_GetPokedex(context.FieldSystem!.SaveData!)))
        {
            return CheckResultFail;
        }
        if (MapHeader.MapHeader_IsAzureFluteAllowed(context.MapHeaderID))
        {
            return CheckResultOk;
        }
        return CheckResultFail;
    }
}
