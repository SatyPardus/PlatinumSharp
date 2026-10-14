using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed partial class StartMenu
{
    private const int BallCountWindowLeft = 1;
    private const int BallCountWindowTop = 1;
    private const int BallCountWindowWidth = 0x0C;
    private const int BallCountWindowHeight = 4;
    private const int BallCountWindowPalette = 0x0D;
    private const int BallCountWindowBaseTile = 0x237;
    private const int BallCountFillColor = 0x0F;
    private const int BallCountTypeSafari = 0;
    private const int BallCountTypePalPark = 1;
    private const uint MessageSafariBalls = 9;
    private const uint MessageParkBalls = 10;
    private const uint MessageBallCount = 11;
    private const int BallCountStringLength = 0x20;
    private const int BallCountLineHeight = 0x10;
    private const int TextSpeedNoTransfer = 0xFF;
    private const ushort SoundEffectMenuSelect = 0x5E0;
    private const uint MenuNothingChosen = 0xFFFFFFFF;
    private const uint MenuCanceled = 0xFFFFFFFE;
    private const uint ActionNone = 0xFFFFFFFF;
    private const uint ActionExit = 0xFFFFFFFE;
    private const uint SpriteCapacitiesAddress = 0x020EA044;
    private const uint CursorTemplateAddress = 0x020EA0A4;
    private const uint IconTemplateAddress = 0x020EA0D8;
    private const uint IconScaleAddress = 0x020EA038;
    private const int SpriteTemplateSize = 0x34;
    private const int MenuIconArchive = 0x42;
    private const int CursorResourceID = 0x34D8;
    private const int IconResourceID = 0x34D9;
    private const int VramTypeMain = 1;
    private const int IconSpacing = 0x18;
    private const int CursorTopOffset = 0x14;
    private const int IconAnimsPerOption = 3;
    private const int IconAnimIdle = 0;
    private const int IconAnimSelected = 1;
    private const int IconAnimWiggle = 2;
    private const int IconAnimBagFemale = 0x1B;
    private const int GenderFemale = 1;
    private const int AffineModeNormal = 1;
    private const int ObjLayer = 0x10;
    private const int FxOne = 0x1000;

    private static uint ActionFunction(uint option)
    {
        sActions ??= RomData.Arm9U32(ActionTableAddress, OptionCount * ActionEntryWords);
        return sActions[option * ActionEntryWords + 1];
    }

    private static FieldTaskFunc ActionHandler(uint address)
    {
        switch (address)
        {
            case 0x0203B681:
                return StartMenu_SelectPokedex;
            case 0x0203B769:
                return StartMenu_SelectPokemon;
            case 0x0203BBF5:
                return StartMenu_SelectBag;
            case 0x0203BE8D:
                return StartMenu_SelectTrainerCase;
            case 0x0203BF39:
                return StartMenu_SelectSave;
            case 0x0203C001:
                return StartMenu_SelectOptions;
            case 0x0203C07D:
                return StartMenu_SelectChat;
            case 0x0203C165:
                return StartMenu_SelectRetire;
        }
        throw new InvalidOperationException($"Unknown start menu action 0x{address:X8}");
    }

    private static SpriteTemplate LoadSpriteTemplate(uint address)
    {
        byte[] raw = RomData.Arm9(address, SpriteTemplateSize);
        SpriteTemplate template = new()
        {
            X = (short)Bin.U16(raw, 0),
            Y = (short)Bin.U16(raw, 2),
            Z = (short)Bin.U16(raw, 4),
            AnimIdx = (ushort)Bin.U16(raw, 6),
            Priority = Bin.I32(raw, 8),
            PlttIdx = Bin.I32(raw, 0xC),
            VramType = Bin.I32(raw, 0x10),
            BgPriority = Bin.I32(raw, 0x2C),
            VramTransfer = Bin.I32(raw, 0x30),
        };
        for (int i = 0; i < Game.SpriteManager.ResourceTypes; i++)
        {
            template.Resources[i] = Bin.I32(raw, 0x14 + i * 4);
        }
        return template;
    }

    // OFFSET 0x0203B078
    private static void StartMenu_Close(StartMenu menu)
    {
        StartMenu_FreeSprites(menu);
        Menu.Menu_Free(menu.Menu!, out _);
        StringList.StringList_Free(menu.MenuOptions!);
        menu.Menu = null;
    }

    // OFFSET 0x0203B094
    private static void StartMenu_PrintBallCount(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        int type;
        if (SystemFlags.SystemFlag_CheckSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)) == true)
        {
            type = BallCountTypeSafari;
        }
        else if (SystemFlags.SystemFlag_CheckInPalPark(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)) == true)
        {
            type = BallCountTypePalPark;
        }
        else
        {
            return;
        }
        Bg.Window_Add(fieldSystem.BgConfig!, menu.SecondaryWindow, BgLayerMenu, BallCountWindowLeft, BallCountWindowTop, BallCountWindowWidth, BallCountWindowHeight, BallCountWindowPalette, BallCountWindowBaseTile);
        RenderWindow.LoadStandardWindowGraphics(fieldSystem.BgConfig!, BgLayerMenu, WindowFrameTile, WindowFramePalette, WindowFrameType, HeapIDFieldTask);
        RenderWindow.Window_DrawStandardFrame(menu.SecondaryWindow, 1, WindowFrameTile, WindowFramePalette);
        Bg.Window_FillTilemap(menu.SecondaryWindow, BallCountFillColor);
        MessageLoader loader = MessageLoader.MessageLoader_Init(0, MessageArchive, MessageBankStartMenu, HeapIDFieldTask)!;
        GameString? label;
        if (type == BallCountTypeSafari)
        {
            label = MessageLoader.MessageLoader_GetNewString(loader, MessageSafariBalls);
        }
        else
        {
            label = MessageLoader.MessageLoader_GetNewString(loader, MessageParkBalls);
        }
        Text.Text_AddPrinterWithParams(menu.SecondaryWindow, 0, label, 0, 0, TextSpeedNoTransfer, null);
        GameString.String_Free(label);
        StringTemplate template = StringTemplate.StringTemplate_Default(HeapIDFieldTask);
        GameString count = GameString.String_Init(BallCountStringLength, HeapIDFieldTask);
        GameString? format = MessageLoader.MessageLoader_GetNewString(loader, MessageBallCount);
        if (type == BallCountTypeSafari)
        {
            BufferView balls = FieldPlayerState.FieldPlayerState_GetSafariBallCount(FieldPlayerState.SaveData_GetFieldPlayerState(fieldSystem.SaveData!));
            StringTemplate.StringTemplate_SetNumber(template, 0, balls.U16(0), 2, 0, 1);
        }
        else
        {
            int balls = sub_020563BC(fieldSystem);
            StringTemplate.StringTemplate_SetNumber(template, 0, balls, 2, 0, 1);
        }
        StringTemplate.StringTemplate_Format(template, count, format!);
        Text.Text_AddPrinterWithParams(menu.SecondaryWindow, 0, count, 0, BallCountLineHeight, TextSpeedNoTransfer, null);
        GameString.String_Free(count);
        GameString.String_Free(format);
        StringTemplate.StringTemplate_Free(template);
        MessageLoader.MessageLoader_Free(loader);
        Bg.Window_ScheduleCopyToVRAM(menu.SecondaryWindow);
    }

    // OFFSET 0x0203B200
    private static void StartMenu_EraseBallCount(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        if (SystemFlags.SystemFlag_CheckSafariGameActive(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!))
            || SystemFlags.SystemFlag_CheckInPalPark(VarsFlags.SaveData_GetVarsFlags(fieldSystem.SaveData!)))
        {
            RenderWindow.Window_EraseStandardFrame(menu.SecondaryWindow, 1);
            Bg.Window_Remove(menu.SecondaryWindow);
        }
    }

    // OFFSET 0x0203B244
    private static bool StartMenu_Select(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        StartMenu menu = (StartMenu)FieldTask.FieldTask_GetEnv(task)!;
        ushort previous = Menu.Menu_GetCursorPos(menu.Menu!);
        menu.Input = Menu.Menu_ProcessInputWithSound(menu.Menu!, SoundEffectMenuSelect);
        menu.CursorPosition = Menu.Menu_GetCursorPos(menu.Menu!);
        if (previous != menu.CursorPosition)
        {
            StartMenu_SetCursorPos(menu.Sprites[0]!.Sprite!, menu.CursorPosition);
            StartMenu_ChangeActiveMenuIcon(menu, previous, menu.CursorPosition);
            fieldSystem.MenuCursorPosition = menu.Options[menu.CursorPosition];
        }
        StartMenu_TryTransitionIconAnimationToWiggle(menu.Sprites[menu.CursorPosition + 1]!.Sprite!);
        switch (menu.Input)
        {
            case MenuCanceled:
                menu.State = StateEnd;
                break;
            case MenuNothingChosen:
                break;
            default:
                uint action = ActionFunction(menu.Input);
                if (action == ActionExit)
                {
                    menu.State = StateEnd;
                }
                else if (action != ActionNone)
                {
                    return ActionHandler(action)(task);
                }
                break;
        }
        return true;
    }

    // OFFSET 0x0203B2EC
    private static void StartMenu_RestoreUnionRoomComm(StartMenu menu, FieldSystem fieldSystem)
    {
        if (WirelessManager.WirelessMan_IsInitialized() && menu.InUnionRoom != 0)
        {
            sub_0205C2B0(fieldSystem.Unknown80);
            sub_02036AC4();
            sub_0205BEA8(0);
        }
    }

    // OFFSET 0x0203B318
    private static void StartMenu_InitSprites(StartMenu menu, byte[] options, uint optionCount, byte gender)
    {
        uint[] capacities = RomData.Arm9U32(SpriteCapacitiesAddress, Game.SpriteManager.ResourceTypes);
        SpriteResourceManager.SpriteResourceManager_SetCapacities(menu.SpriteManager, capacities, MaxOptions, HeapIDFieldTask);
        NARC narc = NARC.NARC_ctor(MenuIconArchive, HeapIDFieldTask);
        SpriteResourceManager.SpriteResourceManager_LoadPalette(menu.SpriteManager, narc, 5, 0, 2, VramTypeMain, CursorResourceID);
        SpriteResourceManager.SpriteResourceManager_LoadCell(menu.SpriteManager, narc, 1, 0, CursorResourceID);
        SpriteResourceManager.SpriteResourceManager_LoadAnimation(menu.SpriteManager, narc, 0, 0, CursorResourceID);
        SpriteResourceManager.SpriteResourceManager_LoadTiles(menu.SpriteManager, narc, 2, 0, VramTypeMain, CursorResourceID);
        menu.Sprites[0] = SpriteResourceManager.SpriteResourceManager_CreateManagedSprite(menu.SpriteManager, LoadSpriteTemplate(CursorTemplateAddress));
        StartMenu_SetCursorPos(menu.Sprites[0]!.Sprite!, menu.CursorPosition);
        SpriteResourceManager.SpriteResourceManager_LoadCell(menu.SpriteManager, narc, 4, 0, IconResourceID);
        SpriteResourceManager.SpriteResourceManager_LoadAnimation(menu.SpriteManager, narc, 3, 0, IconResourceID);
        SpriteResourceManager.SpriteResourceManager_LoadTiles(menu.SpriteManager, narc, 6, 0, VramTypeMain, IconResourceID);
        for (uint i = 0; i < optionCount; i++)
        {
            SpriteTemplate template = LoadSpriteTemplate(IconTemplateAddress);
            uint[] scaleWords = RomData.Arm9U32(IconScaleAddress, 3);
            VecFx32 scale;
            scale.X = (int)scaleWords[0];
            scale.Y = (int)scaleWords[1];
            scale.Z = (int)scaleWords[2];
            template.Y = (short)(template.Y + IconSpacing * i);
            if (options[i] == OptionBag && gender == GenderFemale)
            {
                template.AnimIdx = IconAnimBagFemale;
            }
            else
            {
                template.AnimIdx = (ushort)(options[i] * IconAnimsPerOption);
            }
            menu.Sprites[i + 1] = SpriteResourceManager.SpriteResourceManager_CreateManagedSprite(menu.SpriteManager, template);
            Sprite.Sprite_SetAffineScaleEx(menu.Sprites[i + 1]!.Sprite!, scale, AffineModeNormal);
        }
        StartMenu_SetIconAnimationAndPalette(menu.Sprites[menu.CursorPosition + 1]!.Sprite!, IconAnimWiggle, 1);
        menu.SpriteCount = optionCount + 1;
        GXLayers.GXLayers_EngineAToggleLayers(ObjLayer, 1);
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x0203B4E8
    private static void StartMenu_FreeSprites(StartMenu menu)
    {
        for (ushort i = 0; i < menu.SpriteCount; i++)
        {
            SpriteSystem.Sprite_DeleteAndFreeResources(menu.Sprites[i]!);
        }
        SpriteResourceManager.SpriteResourceManager_Cleanup(menu.SpriteManager);
    }

    // OFFSET 0x0203B520
    private static void StartMenu_AnimateSprites(StartMenu menu)
    {
        for (ushort i = 0; i < menu.SpriteCount; i++)
        {
            Sprite.Sprite_UpdateAnim(menu.Sprites[i]!.Sprite!, FxOne);
        }
    }

    // OFFSET 0x0203B558
    private static void StartMenu_SetCursorPos(Sprite sprite, uint position)
    {
        VecFx32 vec = Sprite.Sprite_GetPosition(sprite);
        vec.Y = (int)((position * IconSpacing + CursorTopOffset) << 12);
        Sprite.Sprite_SetPosition(sprite, vec);
    }

    // OFFSET 0x0203B588
    private static void StartMenu_SetIconAnimationAndPalette(Sprite sprite, uint animOffset, uint palette)
    {
        uint option = Sprite.Sprite_GetActiveAnim(sprite) / IconAnimsPerOption;
        Sprite.Sprite_SetAnim(sprite, option * IconAnimsPerOption + animOffset);
        Sprite.Sprite_SetExplicitPaletteWithOffset(sprite, palette);
    }

    // OFFSET 0x0203B5B4
    private static void StartMenu_ChangeActiveMenuIcon(StartMenu menu, ushort previous, ushort current)
    {
        StartMenu_SetIconAnimationAndPalette(menu.Sprites[previous + 1]!.Sprite!, IconAnimIdle, 0);
        StartMenu_SetIconAnimationAndPalette(menu.Sprites[current + 1]!.Sprite!, IconAnimSelected, 1);
    }

    // OFFSET 0x0203B5E8
    private static void StartMenu_TryTransitionIconAnimationToWiggle(Sprite sprite)
    {
        if (Sprite.Sprite_GetActiveAnim(sprite) % IconAnimsPerOption != IconAnimSelected)
        {
            return;
        }
        if (Sprite.Sprite_IsAnimated(sprite) == false)
        {
            StartMenu_SetIconAnimationAndPalette(sprite, IconAnimWiggle, 1);
        }
    }
}
