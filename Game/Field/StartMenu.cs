using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed partial class StartMenu
{
    public const int Size = 0x200;
    public const int PrimaryWindowOffset = 0x00;
    public const int SecondaryWindowOffset = 0x10;
    public const int MenuOffset = 0x20;
    public const int MenuOptionsOffset = 0x24;
    public const int CursorPositionOffset = 0x28;
    public const int StateOffset = 0x2A;
    public const int InputOffset = 0x2C;
    public const int OptionsOffset = 0x30;
    public const int SpriteManagerOffset = 0x38;
    public const int SpritesOffset = 0x19C;
    public const int SpriteCountOffset = 0x1BC;
    public const int HideOptionFlagsOffset = 0x1C0;
    public const int InUnionRoomOffset = 0x1C4;
    public const int CallbackOffset = 0x1C8;
    public const int ItemUseContextOffset = 0x1CC;
    public const int FieldMoveContextOffset = 0x1E8;
    public const int TaskDataOffset = 0x1F8;
    public const int AdditionalTaskDataOffset = 0x1FC;
    public const int MaxOptions = 8;
    public const int MaxSprites = MaxOptions + 1;

    public const int OptionPokedex = 0;
    public const int OptionPokemon = 1;
    public const int OptionBag = 2;
    public const int OptionTrainerCase = 3;
    public const int OptionSave = 4;
    public const int OptionOptions = 5;
    public const int OptionExit = 6;
    public const int OptionChat = 7;
    public const int OptionRetire = 8;
    public const int OptionCount = 9;

    public const int StateInit = 0;
    public const int StateSelect = 1;
    public const int StateApplicationStart = 2;
    public const int StateApplicationRun = 3;
    public const int StateSave = 4;
    public const int StateSaveWait = 5;
    public const int StateEvolveInit = 6;
    public const int StateEvolve = 7;
    public const int StateFadeAndEnd = 8;
    public const int StateEndAfterFade = 9;
    public const int StateNewTask = 10;
    public const int StateExitWithNewTask = 11;
    public const int StateReinit = 12;
    public const int StateEnd = 13;
    public const int StateReinitWaitForFade = 14;
    public const int StateSaved = 15;

    public readonly Window PrimaryWindow = new();
    public readonly Window SecondaryWindow = new();
    public Menu? Menu;
    public StringListEntry[]? MenuOptions;
    public ushort CursorPosition;
    public ushort State;
    public uint Input;
    public readonly byte[] Options = new byte[MaxOptions];
    public readonly Overlay5SpriteManager SpriteManager = new();
    public readonly ManagedSprite?[] Sprites = new ManagedSprite?[MaxSprites];
    public uint SpriteCount;
    public uint HideOptionFlags;
    public int InUnionRoom;
    public FieldTaskFunc? Callback;
    public readonly ItemUseContext ItemUseContext = new();
    public readonly FieldMoveContext FieldMoveContext = new();
    public object? TaskData;
    public object? AdditionalTaskData;

    private const uint ActionTableAddress = 0x020EA05C;
    private const int ActionEntryWords = 2;
    private const int HeapIDFieldTask = 11;
    private const int MessageArchive = 0x1A;
    private const int MessageBankStartMenu = 0x16F;
    private const int SoundEffectOpenMenu = 0x5FD;
    private const int MapLoadTypeUnion = 2;
    private const int MapLoadTypeColosseum = 3;
    private const int FadeTypeIn = 1;
    private const int BgLayerMenu = 3;
    private const int WindowLeft = 0x14;
    private const int WindowTop = 1;
    private const int WindowWidth = 0x0B;
    private const int WindowRowsPerOption = 3;
    private const int WindowPalette = 0x0C;
    private const int WindowBaseTile = 0x145;
    private const int WindowFrameTile = 0x3D9;
    private const int WindowFramePalette = 0x0B;
    private const int WindowFrameType = 1;
    private const int MenuXOffset = 0x1C;
    private const int MenuYOffset = 4;
    private const int MenuLineSpacing = 8;
    private const int MenuLoopMinimumOptions = 4;
    private const uint MenuCancelKeys = 0x402;
    private const int PlayerNameLength = 8;
    private const uint HidePokedex = 0x001;
    private const uint HidePokemon = 0x002;
    private const uint HideBag = 0x004;
    private const uint HideTrainerCase = 0x008;
    private const uint HideSave = 0x010;
    private const uint HideOptions = 0x020;
    private const uint HideExit = 0x040;
    private const uint HideChat = 0x080;
    private const uint HideRetire = 0x100;

    private static uint[]? sActions;

    private static uint ActionBankEntry(int option)
    {
        sActions ??= RomData.Arm9U32(ActionTableAddress, OptionCount * ActionEntryWords);
        return sActions[option * ActionEntryWords];
    }

    // OFFSET 0x0203A9C8
    public static bool FieldSystem_IsInValidLocation(FieldSystem fieldSystem)
    {
        if (MapHeader.MapHeader_GetMapLabelTextID((int)fieldSystem.Location!.U32(0)) != 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x0203A9E0
    public static void StartMenu_Open(FieldSystem fieldSystem)
    {
        StartMenu menu = StartMenu_New();
        if (SystemFlags.SystemFlag_CheckSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)) == true)
        {
            menu.HideOptionFlags = StartMenu_GetSafariHiddenOptions(fieldSystem);
        }
        else if (SystemFlags.SystemFlag_CheckInPalPark(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)) == true)
        {
            menu.HideOptionFlags = StartMenu_GetPalParkHiddenOptions(fieldSystem);
        }
        else if (BattleTowerSalon.FieldSystem_IsInBattleTowerSalon(fieldSystem) == true)
        {
            menu.HideOptionFlags = StartMenu_GetBattleTowerSalonHiddenOptions(fieldSystem);
        }
        else
        {
            menu.HideOptionFlags = StartMenu_GetNormalHiddenOptions(fieldSystem);
        }
        menu.InUnionRoom = 0;
        if (PlayerMove.PlayerAvatar_CheckForceStopMovement(fieldSystem.PlayerAvatar) == 1)
        {
            PlayerMove.PlayerAvatar_ForceStopMovement(fieldSystem.PlayerAvatar, PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar));
        }
        FieldTask.FieldSystem_CreateTask(fieldSystem, StartMenu_Main, menu);
    }

    // OFFSET 0x0203AA78
    public static void StartMenu_OpenUnionRoom(FieldSystem fieldSystem)
    {
        StartMenu menu = StartMenu_New();
        menu.HideOptionFlags = StartMenu_GetUnionRoomHiddenOptions(fieldSystem);
        menu.InUnionRoom = 1;
        if (PlayerMove.PlayerAvatar_CheckForceStopMovement(fieldSystem.PlayerAvatar) == 1)
        {
            PlayerMove.PlayerAvatar_ForceStopMovement(fieldSystem.PlayerAvatar, PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar));
        }
        FieldTask.FieldSystem_CreateTask(fieldSystem, StartMenu_Main, menu);
    }

    // OFFSET 0x0203AABC
    public static void StartMenu_OpenColosseum(FieldSystem fieldSystem)
    {
        StartMenu menu = StartMenu_New();
        menu.HideOptionFlags = StartMenu_GetColosseumHiddenOptions(fieldSystem);
        menu.InUnionRoom = 0;
        if (PlayerMove.PlayerAvatar_CheckForceStopMovement(fieldSystem.PlayerAvatar) == 1)
        {
            PlayerMove.PlayerAvatar_ForceStopMovement(fieldSystem.PlayerAvatar, PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar));
        }
        FieldTask.FieldSystem_CreateTask(fieldSystem, StartMenu_Main, menu);
    }

    // OFFSET 0x0203AB00
    public static void StartMenu_OpenFromScript(FieldSystem fieldSystem)
    {
        SoundPlayback.Sound_PlayEffect(SoundEffectOpenMenu);
        StartMenu menu = StartMenu_New();
        menu.InUnionRoom = 0;
        if (SystemFlags.SystemFlag_CheckSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)) == true)
        {
            menu.HideOptionFlags = StartMenu_GetSafariHiddenOptions(fieldSystem);
        }
        else if (SystemFlags.SystemFlag_CheckInPalPark(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)) == true)
        {
            menu.HideOptionFlags = StartMenu_GetPalParkHiddenOptions(fieldSystem);
        }
        else if (BattleTowerSalon.FieldSystem_IsInBattleTowerSalon(fieldSystem) == true)
        {
            menu.HideOptionFlags = StartMenu_GetBattleTowerSalonHiddenOptions(fieldSystem);
        }
        else if (fieldSystem.MapLoadType == MapLoadTypeColosseum)
        {
            menu.HideOptionFlags = StartMenu_GetColosseumHiddenOptions(fieldSystem);
        }
        else if (fieldSystem.MapLoadType == MapLoadTypeUnion)
        {
            menu.HideOptionFlags = StartMenu_GetUnionRoomHiddenOptions(fieldSystem);
            menu.InUnionRoom = 1;
        }
        else
        {
            menu.HideOptionFlags = StartMenu_GetNormalHiddenOptions(fieldSystem);
        }
        FieldTask.FieldTask_InitJump(fieldSystem.Task!, StartMenu_Main, menu);
    }

    // OFFSET 0x0203ABB4
    private static StartMenu StartMenu_New()
    {
        StartMenu menu = new();
        menu.State = StateInit;
        menu.CursorPosition = 0;
        menu.TaskData = null;
        return menu;
    }

    // OFFSET 0x0203ABD0
    private static uint StartMenu_GetNormalHiddenOptions(FieldSystem fieldSystem)
    {
        uint flags = 0;
        if (!Pokedex.Pokedex_IsObtained(Pokedex.SaveData_GetPokedex(fieldSystem.SaveData!)))
        {
            flags |= HidePokedex;
        }
        if (SystemVars.SystemVars_GetPlayerStarter(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)) == 0)
        {
            flags |= HidePokemon;
        }
        if (!SystemFlags.SystemFlag_CheckBagAcquired(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)))
        {
            flags |= HideBag;
        }
        if (MapHeader.MapHeader_IsAmitySquare((int)fieldSystem.Location!.U32(0)) == true)
        {
            flags |= HidePokemon | HideBag;
        }
        return flags | HideChat | HideRetire;
    }

    // OFFSET 0x0203AC24
    private static uint StartMenu_GetSafariHiddenOptions(FieldSystem fieldSystem)
    {
        return HideChat | HideSave;
    }

    // OFFSET 0x0203AC28
    private static uint StartMenu_GetPalParkHiddenOptions(FieldSystem fieldSystem)
    {
        return HideChat | HideSave | HideBag;
    }

    // OFFSET 0x0203AC2C
    private static uint StartMenu_GetBattleTowerSalonHiddenOptions(FieldSystem fieldSystem)
    {
        return HideRetire | HideChat | HideSave | HideBag | HidePokedex;
    }

    // OFFSET 0x0203AC34
    private static uint StartMenu_GetUnionRoomHiddenOptions(FieldSystem fieldSystem)
    {
        return HideRetire | HideSave;
    }

    // OFFSET 0x0203AC3C
    private static uint StartMenu_GetColosseumHiddenOptions(FieldSystem fieldSystem)
    {
        return HideRetire | HideChat | HideSave | HidePokedex;
    }

    // OFFSET 0x0203AC44
    private static bool StartMenu_Main(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        switch (menu.State)
        {
            case StateInit:
                MapObjectManager.MapObjectMan_PauseAllMovement(fieldSystem.MapObjectManager);
                ItemUseContext.ItemUseContext_Init(fieldSystem, menu.ItemUseContext);
                FieldMoveContext.FieldMoveContext_Init(fieldSystem, menu.FieldMoveContext);
                StartMenu_InitMenu(task);
                StartMenu_PrintBallCount(task);
                menu.State = StateSelect;
                break;
            case StateSelect:
                if (StartMenu_Select(task) == false)
                {
                    return false;
                }
                break;
            case StateApplicationStart:
                StartMenu_ApplicationStart(task);
                break;
            case StateApplicationRun:
                StartMenu_ApplicationRun(task);
                break;
            case StateSave:
                StartMenu_Save(task);
                break;
            case StateSaveWait:
                StartMenu_SaveWait(task);
                break;
            case StateEvolveInit:
                StartMenu_EvolveInit(task);
                break;
            case StateEvolve:
                StartMenu_Evolve(task);
                break;
            case StateFadeAndEnd:
                if (FieldTask.FieldSystem_IsRunningFieldMap(fieldSystem))
                {
                    FieldMap.FieldMap_FadeScreen(FadeTypeIn);
                    menu.State = StateEndAfterFade;
                }
                break;
            case StateEndAfterFade:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    StartMenu_RestoreUnionRoomComm(menu, fieldSystem);
                    MapObjectManager.MapObjectMan_UnpauseAllMovement(fieldSystem.MapObjectManager);
                    return true;
                }
                break;
            case StateNewTask:
                if (FieldTask.FieldSystem_IsRunningFieldMap(fieldSystem))
                {
                    MapObjectManager.MapObjectMan_PauseAllMovement(fieldSystem.MapObjectManager);
                    FieldMap.FieldMap_FadeScreen(FadeTypeIn);
                    menu.State = StateExitWithNewTask;
                }
                break;
            case StateExitWithNewTask:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    FieldTask.FieldTask_InitJump(task, menu.Callback!, menu.TaskData);
                }
                break;
            case StateReinit:
                if (FieldTask.FieldSystem_IsRunningFieldMap(fieldSystem))
                {
                    MapObjectManager.MapObjectMan_PauseAllMovement(fieldSystem.MapObjectManager);
                    StartMenu_InitMenu(task);
                    StartMenu_PrintBallCount(task);
                    FieldMap.FieldMap_FadeScreen(FadeTypeIn);
                    menu.State = StateReinitWaitForFade;
                }
                break;
            case StateEnd:
                StartMenu_RestoreUnionRoomComm(menu, fieldSystem);
                StartMenu_Close(menu);
                RenderWindow.Window_EraseStandardFrame(menu.PrimaryWindow, 1);
                Bg.Window_Remove(menu.PrimaryWindow);
                StartMenu_EraseBallCount(task);
                Bg.Bg_ScheduleTilemapTransfer(fieldSystem.BgConfig!, BgLayerMenu);
                MapObjectManager.MapObjectMan_UnpauseAllMovement(fieldSystem.MapObjectManager);
                return true;
            case StateReinitWaitForFade:
                if (ScreenFadeControl.IsScreenFadeDone())
                {
                    menu.State = StateSelect;
                }
                break;
            case StateSaved:
                MapObjectManager.MapObjectMan_UnpauseAllMovement(fieldSystem.MapObjectManager);
                return true;
        }
        if (menu.Menu != null)
        {
            StartMenu_AnimateSprites(menu);
            Sprite.SpriteList_Update(menu.SpriteManager.SpriteList!);
        }
        return false;
    }

    // OFFSET 0x0203ADFC
    private static void StartMenu_InitMenu(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        uint optionCount = StartMenu_MakeOptionList(menu, menu.Options);
        Bg.Window_Add(fieldSystem.BgConfig!, menu.PrimaryWindow, BgLayerMenu, WindowLeft, WindowTop, WindowWidth, (byte)(optionCount * WindowRowsPerOption), WindowPalette, WindowBaseTile);
        RenderWindow.LoadStandardWindowGraphics(fieldSystem.BgConfig!, BgLayerMenu, WindowFrameTile, WindowFramePalette, WindowFrameType, HeapIDFieldTask);
        RenderWindow.Window_DrawStandardFrame(menu.PrimaryWindow, 1, WindowFrameTile, WindowFramePalette);
        MessageLoader loader = MessageLoader.MessageLoader_Init(0, MessageArchive, MessageBankStartMenu, HeapIDFieldTask)!;
        menu.MenuOptions = StringList.StringList_New(optionCount, HeapIDFieldTask);
        menu.CursorPosition = 0;
        for (uint i = 0; i < optionCount; i++)
        {
            if (menu.Options[i] == OptionTrainerCase)
            {
                StringTemplate template = StringTemplate.StringTemplate_Default(HeapIDFieldTask);
                GameString name = GameString.String_Init(PlayerNameLength, HeapIDFieldTask);
                GameString? format = MessageLoader.MessageLoader_GetNewString(loader, ActionBankEntry(menu.Options[i]));
                StringTemplate.StringTemplate_SetPlayerName(template, 0, PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!));
                StringTemplate.StringTemplate_Format(template, name, format!);
                StringList.StringList_AddFromString(menu.MenuOptions, name, menu.Options[i]);
                GameString.String_Free(format);
                GameString.String_Free(name);
                StringTemplate.StringTemplate_Free(template);
            }
            else
            {
                StringList.StringList_AddFromMessageBank(menu.MenuOptions, loader, ActionBankEntry(menu.Options[i]), menu.Options[i]);
            }
            if (fieldSystem.MenuCursorPosition == menu.Options[i])
            {
                menu.CursorPosition = (ushort)i;
            }
        }
        fieldSystem.MenuCursorPosition = menu.Options[menu.CursorPosition];
        MessageLoader.MessageLoader_Free(loader);
        MenuTemplate template2 = new();
        template2.Choices = menu.MenuOptions;
        template2.Window = menu.PrimaryWindow;
        template2.FontID = 1;
        template2.XSize = 1;
        template2.YSize = (byte)optionCount;
        template2.LineSpacing = MenuLineSpacing;
        template2.SuppressCursor = 1;
        if (optionCount >= MenuLoopMinimumOptions)
        {
            template2.LoopAround = 1;
        }
        else
        {
            template2.LoopAround = 0;
        }
        menu.Menu = Menu.Menu_New(template2, MenuXOffset, MenuYOffset, (byte)menu.CursorPosition, HeapIDFieldTask, MenuCancelKeys);
        Bg.Window_ScheduleCopyToVRAM(menu.PrimaryWindow);
        uint gender = TrainerInfo.TrainerInfo_Gender(PlayerSave.SaveData_GetTrainerInfo(fieldSystem.SaveData!));
        StartMenu_InitSprites(menu, menu.Options, optionCount, (byte)gender);
    }

    // OFFSET 0x0203AFCC
    private static uint StartMenu_MakeOptionList(StartMenu menu, byte[] options)
    {
        uint count = 0;
        if ((menu.HideOptionFlags & HideRetire) == 0)
        {
            options[count] = OptionRetire;
            count++;
        }
        if ((menu.HideOptionFlags & HideChat) == 0)
        {
            options[count] = OptionChat;
            count++;
        }
        if ((menu.HideOptionFlags & HidePokedex) == 0)
        {
            options[count] = OptionPokedex;
            count++;
        }
        if ((menu.HideOptionFlags & HidePokemon) == 0)
        {
            options[count] = OptionPokemon;
            count++;
        }
        if ((menu.HideOptionFlags & HideBag) == 0)
        {
            options[count] = OptionBag;
            count++;
        }
        if ((menu.HideOptionFlags & HideTrainerCase) == 0)
        {
            options[count] = OptionTrainerCase;
            count++;
        }
        if ((menu.HideOptionFlags & HideSave) == 0)
        {
            options[count] = OptionSave;
            count++;
        }
        if ((menu.HideOptionFlags & HideOptions) == 0)
        {
            options[count] = OptionOptions;
            count++;
        }
        if ((menu.HideOptionFlags & HideExit) == 0)
        {
            options[count] = OptionExit;
            count++;
        }
        return count;
    }
}
