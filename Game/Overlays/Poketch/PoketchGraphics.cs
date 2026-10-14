namespace PlatinumSharp.Game;

public sealed class PoketchConstTaskData
{
    public int AppID;
}

public sealed class PoketchCursor
{
    public int Loaded;
    public PoketchAnimationManager? AnimMan;
    public readonly PoketchSpriteData SpriteData = new();
    public PoketchAnimatedSprite? Tens;
    public PoketchAnimatedSprite? Ones;
}

public sealed class PoketchGraphicsTaskData
{
    public const int ButtonTileCount = 6;
    public const int ButtonTileBytes = 0x40;

    public PoketchConstTaskData? ConstTaskData;
    public readonly PoketchActiveTaskList ActiveTasks = new();
    public readonly byte[][] ButtonTiles = Enumerable.Range(0, ButtonTileCount).Select(_ => new byte[ButtonTileBytes]).ToArray();
    public OamManager? OamMan;
    public PoketchAnimationManager? AnimMan;
    public SysTask? VBlankTask;
    public readonly PoketchCursor Cursor = new();
    public BgConfig? BgConfig;
    public PoketchSystem? PoketchSys;
    public readonly ushort[] Palettes = new ushort[0x100];
    public readonly ushort[] TempPalette = new ushort[0x10];
}

public static class PoketchGraphics
{
    public const uint TaskSetup = 0;
    public const uint TaskOpen = 1;
    public const uint TaskOpenAlt = 2;
    public const uint TaskSwitchOut = 3;
    public const uint TaskClose = 4;
    public const uint TaskSwitchIn = 5;
    public const uint TaskButtonTopPressed = 6;
    public const uint TaskButtonTopHeld = 7;
    public const uint TaskButtonTopReleased = 8;
    public const uint TaskButtonBottomPressed = 9;
    public const uint TaskButtonBottomHeld = 0xA;
    public const uint TaskButtonBottomReleased = 0xB;
    public const uint TaskLoadCursor = 0xC;
    public const uint TaskUpdateCursor = 0xD;
    public const uint TaskFreeCursor = 0xE;
    public const uint TaskDimScreen = 0xF;
    public const uint TaskUndimScreen = 0x10;
    public const uint TaskFreeBackgrounds = 0x11;

    private const int PoketchOverlay = 25;
    private const int HeapIDPoketchApp = 7;
    private const int NarcPoketchGraphics = 0xC;
    private const int ActiveTaskSlots = 8;
    private const uint TaskPriority = 2;
    private const int BgLayerSub0 = 4;
    private const int BgLayerSub1 = 5;
    private const uint BgTemplateSub0 = 0x02255F54;
    private const uint BgTemplateSub1 = 0x02255F70;
    private const uint CursorAnimData = 0x02255F44;
    private const int DisplaySub = 1;
    private const uint VramSubBg = 0x80;
    private const uint VramSubObj = 0x100;
    private const int BankSubBg = 2;
    private const int BankSubObj = 6;
    private const int BankCount = 10;
    private const uint ObjMappingMask = 0xFFCFFFEF;
    private const uint ObjMapping1D32K = 0x10;
    private const uint VisiblePlaneMask = 0xFFFFE0FF;
    private const uint VisiblePlanesPoketch = 0x1300;
    private const uint VisibleWindowMask = 0xFFFF1FFF;
    private const ushort SeqPoketchButton = 0x671;
    private const ushort SeqPoketchButtonHeld = 0x66F;
    private const ushort TileScreenFill = 0xE4;
    private const ushort TileBlank = 0x40;
    private const int ScreenPalette = 0xF;

    private static readonly PoketchTask[] sTasks =
    {
        new(TaskSetup, PoketchGraphics_TaskSetup, 0),
        new(TaskOpen, PoketchGraphics_TaskOpen, 4),
        new(TaskOpenAlt, PoketchGraphics_TaskOpen, 4),
        new(TaskSwitchOut, PoketchGraphics_TaskSwitchOut, 4),
        new(TaskClose, PoketchGraphics_TaskClose, 4),
        new(TaskSwitchIn, PoketchGraphics_TaskSwitchIn, 4),
        new(TaskButtonTopPressed, PoketchGraphics_TaskButtonTopPressed, 0),
        new(TaskButtonTopReleased, PoketchGraphics_TaskButtonTopReleased, 0),
        new(TaskButtonTopHeld, PoketchGraphics_TaskButtonTopHeld, 0),
        new(TaskButtonBottomPressed, PoketchGraphics_TaskButtonBottomPressed, 0),
        new(TaskButtonBottomReleased, PoketchGraphics_TaskButtonBottomReleased, 0),
        new(TaskButtonBottomHeld, PoketchGraphics_TaskButtonBottomHeld, 0),
        new(TaskLoadCursor, PoketchGraphics_TaskLoadCursor, 0),
        new(TaskUpdateCursor, PoketchGraphics_TaskUpdateCursor, 0),
        new(TaskFreeCursor, PoketchGraphics_TaskFreeCursor, 0),
        new(TaskDimScreen, PoketchGraphics_TaskDimScreen, 0),
        new(TaskUndimScreen, PoketchGraphics_TaskUndimScreen, 0),
        new(TaskFreeBackgrounds, PoketchGraphics_TaskFreeBackgrounds, 0),
        new(PoketchActiveTaskList.NoTask, null, 0),
    };

    // OFFSET 0x02254560
    public static bool PoketchGraphics_Main(out PoketchGraphicsTaskData? taskDataPtr, PoketchConstTaskData constTaskData, OamManager oamMan, PoketchSystem poketchSys)
    {
        PoketchGraphicsTaskData taskData = new();
        taskDataPtr = taskData;
        taskData.OamMan = oamMan;
        oamMan.ApplyAndReset();
        taskData.AnimMan = PoketchAnimation.PoketchAnimation_SetupAnimationManager(taskData.OamMan, HeapIDPoketchApp);
        if (taskData.AnimMan == null)
        {
            taskDataPtr = null;
            return false;
        }
        taskData.ConstTaskData = constTaskData;
        taskData.BgConfig = Bg.BgConfig_New(HeapIDPoketchApp);
        taskData.PoketchSys = poketchSys;
        PoketchGraphics_LoadPalettes(taskData);
        PoketchGraphics_InitCursor(taskData.Cursor, taskData.AnimMan);
        PoketchTasks.PoketchTask_InitActiveTaskList(taskData.ActiveTasks, ActiveTaskSlots);
        PoketchGraphics_SetupButtonTiles(taskData.ButtonTiles[0], 0x48, 4, 6);
        PoketchGraphics_SetupButtonTiles(taskData.ButtonTiles[1], 0x4C, 4, 6);
        PoketchGraphics_SetupButtonTiles(taskData.ButtonTiles[2], 0x50, 4, 6);
        PoketchGraphics_SetupButtonTiles(taskData.ButtonTiles[3], 0x54, 2, 4);
        PoketchGraphics_SetupButtonTiles(taskData.ButtonTiles[4], 0x58, 2, 4);
        PoketchGraphics_SetupButtonTiles(taskData.ButtonTiles[5], 0x5C, 2, 4);
        taskData.VBlankTask = SysTask.SysTask_ExecuteAfterVBlank(PoketchGraphics_UpdateAnimationsTask, taskData, 0);
        return true;
    }

    // OFFSET 0x02254654
    private static void PoketchGraphics_UpdateAnimationsTask(SysTask task, object? param)
    {
        PoketchAnimation.PoketchAnimation_UpdateAnimations(((PoketchGraphicsTaskData)param!).AnimMan!);
    }

    // OFFSET 0x02254664
    public static PoketchAnimationManager PoketchGraphics_GetAnimationManager()
    {
        return PoketchSystems.PoketchSystem_GetTaskData().AnimMan!;
    }

    // OFFSET 0x02254674
    public static BgConfig PoketchGraphics_GetBgConfig()
    {
        return PoketchSystems.PoketchSystem_GetTaskData().BgConfig!;
    }

    // OFFSET 0x02254684
    private static void PoketchGraphics_LoadPalettes(PoketchGraphicsTaskData taskData)
    {
        byte[]? file = Graphics.Graphics_GetPlttData(NarcPoketchGraphics, 0, out PaletteFile? palette, HeapIDPoketchApp);
        if (file != null)
        {
            Array.Copy(palette!.Colors, taskData.Palettes, taskData.Palettes.Length);
        }
    }

    private static int PoketchGraphics_ActiveColor(PoketchGraphicsTaskData taskData)
    {
        return Poketch.Poketch_GetScreenColor(PoketchSystems.PoketchSystem_GetPoketchData(taskData.PoketchSys!));
    }

    // OFFSET 0x022546B8
    public static void PoketchGraphics_LoadActivePalette(uint bgOffset, uint objOffset)
    {
        PoketchGraphicsTaskData taskData = PoketchSystems.PoketchSystem_GetTaskData();
        int color = PoketchGraphics_ActiveColor(taskData);
        Video.LoadPaletteRange(Video.SubBackground, taskData.Palettes, color * 0x20, bgOffset * 0x20, 0x20);
        Video.LoadPaletteRange(Video.SubObject, taskData.Palettes, color * 0x20, objOffset * 0x20, 0x20);
    }

    // OFFSET 0x022546F0
    public static void PoketchGraphics_LoadActiveBacklightPalette(uint bgOffset, uint objOffset)
    {
        PoketchGraphicsTaskData taskData = PoketchSystems.PoketchSystem_GetTaskData();
        int color = PoketchGraphics_ActiveColor(taskData);
        Video.LoadPaletteRange(Video.SubBackground, taskData.Palettes, color * 0x20 + 0x10, 0, 0x20);
        Video.LoadPaletteRange(Video.SubObject, taskData.Palettes, color * 0x20 + 0x10, 0, 0x20);
    }

    // OFFSET 0x02254728
    public static void PoketchGraphics_CopyActivePalette(ushort[] dest)
    {
        PoketchGraphicsTaskData taskData = PoketchSystems.PoketchSystem_GetTaskData();
        int color = PoketchGraphics_ActiveColor(taskData);
        Array.Copy(taskData.Palettes, color * 0x20, dest, 0, 0x10);
    }

    // OFFSET 0x02254754
    public static void PoketchGraphics_Close(PoketchGraphicsTaskData? taskData)
    {
        if (taskData == null)
        {
            return;
        }
        PoketchGraphics_FreeCursor(taskData.Cursor);
        if (taskData.AnimMan != null)
        {
            PoketchAnimation.PoketchAnimation_FreeAnimationManager(taskData.AnimMan);
        }
        if (taskData.VBlankTask != null)
        {
            SysTask.SysTask_Done(taskData.VBlankTask);
        }
    }

    // OFFSET 0x02254798
    private static void PoketchGraphics_SetupButtonTiles(byte[] tiles, int tileBase, int repeatStart, int repeatEnd)
    {
        int rowOffset = 0;
        int index = 0;
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                ushort tile = (ushort)((tileBase + rowOffset + col) | 0xF000);
                tiles[index * 2] = (byte)tile;
                tiles[index * 2 + 1] = (byte)(tile >> 8);
                index++;
            }
            if (row < repeatStart || row >= repeatEnd)
            {
                rowOffset += 0x20;
            }
        }
    }

    // OFFSET 0x022547D0
    public static void PoketchGraphics_StartTask(PoketchGraphicsTaskData taskData, uint taskID)
    {
        PoketchTasks.PoketchTask_Start(sTasks, taskID, taskData, taskData.ConstTaskData, taskData.ActiveTasks, TaskPriority, HeapIDPoketchApp);
    }

    // OFFSET 0x022547F4
    public static bool PoketchGraphics_TaskIsNotActive(PoketchGraphicsTaskData taskData, uint taskID)
    {
        return PoketchTasks.PoketchTask_TaskIsNotActive(taskData.ActiveTasks, taskID);
    }

    // OFFSET 0x02254800
    public static bool PoketchGraphics_NoActiveTasks(PoketchGraphicsTaskData taskData)
    {
        return PoketchTasks.PoketchTask_NoActiveTasks(taskData.ActiveTasks);
    }

    // OFFSET 0x0225480C
    private static void PoketchGraphics_EndTask(PoketchTaskManager taskMan)
    {
        PoketchGraphicsTaskData taskData = (PoketchGraphicsTaskData)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        PoketchTasks.PoketchTask_EndTask(taskData.ActiveTasks, taskMan);
    }

    // OFFSET 0x02254820
    private static void PoketchGraphics_TaskSetup(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        PoketchGraphicsTaskData taskData = (PoketchGraphicsTaskData)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        Video.SetSubGraphicsMode(0);
        uint[] banks = new uint[BankCount];
        for (int i = 0; i < BankCount; i++)
        {
            banks[i] = Video.Banks[i];
        }
        banks[BankSubBg] = VramSubBg;
        banks[BankSubObj] = VramSubObj;
        Video.SetBanks(banks);
        Video.SetDisplayControl(DisplaySub, (Video.GetDisplayControl(DisplaySub) & ObjMappingMask) | ObjMapping1D32K);
        GXLayers.GXLayers_DisableEngineBLayers();
        GXLayers.GXLayers_EngineBToggleLayers(0x10, 1);
        Bg.Bg_InitFromTemplate(taskData.BgConfig!, BgLayerSub0, RomStructs.BgTemplate(RomData.Overlay(PoketchOverlay, BgTemplateSub0, RomStructs.BgTemplateSize), 0), 0);
        Bg.Bg_InitFromTemplate(taskData.BgConfig!, BgLayerSub1, RomStructs.BgTemplate(RomData.Overlay(PoketchOverlay, BgTemplateSub1, RomStructs.BgTemplateSize), 0), 0);
        Graphics.Graphics_LoadTilesToBgLayer(NarcPoketchGraphics, 0xE, taskData.BgConfig!, BgLayerSub0, 0x40, 0, 1, HeapIDPoketchApp);
        Graphics.Graphics_LoadTilemapToBgLayer(NarcPoketchGraphics, 0xF, taskData.BgConfig!, BgLayerSub0, 0, 0, 1, HeapIDPoketchApp);
        int borderColor = PoketchSystems.PoketchSystem_GetBorderColor(taskData.PoketchSys!);
        Graphics.Graphics_LoadPaletteWithSrcOffset(NarcPoketchGraphics, 0xD, 4, (uint)(borderColor * 0x20), 0x1E0, 0x20, HeapIDPoketchApp);
        Bg.Bg_FillTilemapRect(taskData.BgConfig!, BgLayerSub1, TileScreenFill, 0, 0, 0x20, 0x18, ScreenPalette);
        Bg.Bg_CopyTilemapBufferToVRAM(taskData.BgConfig!, BgLayerSub1);
        Video.SetDisplayControl(DisplaySub, (Video.GetDisplayControl(DisplaySub) & VisiblePlaneMask) | VisiblePlanesPoketch);
        Video.SetDisplayControl(DisplaySub, Video.GetDisplayControl(DisplaySub) & VisibleWindowMask);
        PoketchGraphics_EndTask(taskMan);
    }

    // OFFSET 0x02254944
    private static void PoketchGraphics_TaskOpen(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        BufferView extra = PoketchTasks.PoketchTask_GetExtraData(taskMan)!;
        PoketchGraphicsTaskData taskData = (PoketchGraphicsTaskData)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        switch (PoketchTasks.PoketchTask_GetState(taskMan))
        {
            case 0:
                extra.W16(0, 0);
                extra.W16(2, 0);
                PoketchTasks.PoketchTask_IncrementState(taskMan);
                goto case 1;
            case 1:
            {
                extra.W16(0, extra.U16(0) + 1);
                if (extra.U16(0) == 0)
                {
                    return;
                }
                extra.W16(0, 0);
                extra.W16(2, extra.U16(2) + 0xC);
                if (extra.U16(2) > 0x28)
                {
                    extra.W16(2, 0x28);
                }
                int progress = extra.U16(2);
                int rows = progress >> 2;
                int partial = progress & 3;
                int top = 0xC - rows;
                Bg.Bg_FillTilemapRect(taskData.BgConfig!, BgLayerSub1, TileBlank, 2, (byte)top, 0x18, (byte)(rows * 2), ScreenPalette);
                if (partial != 0)
                {
                    Bg.Bg_FillTilemapRect(taskData.BgConfig!, BgLayerSub1, (ushort)(TileScreenFill + partial), 2, (byte)(top - 1), 0x18, 1, ScreenPalette);
                    Bg.Bg_FillTilemapRect(taskData.BgConfig!, BgLayerSub1, (ushort)(TileScreenFill - partial), 2, (byte)(top + rows * 2), 0x18, 1, ScreenPalette);
                }
                Bg.Bg_CopyTilemapBufferToVRAM(taskData.BgConfig!, BgLayerSub1);
                if (extra.U16(2) == 0x28)
                {
                    PoketchTasks.PoketchTask_IncrementState(taskMan);
                }
                break;
            }
            case 2:
                PoketchGraphics_EndTask(taskMan);
                break;
        }
    }

    // OFFSET 0x02254A5C
    private static void PoketchGraphics_TaskClose(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        BufferView extra = PoketchTasks.PoketchTask_GetExtraData(taskMan)!;
        PoketchGraphicsTaskData taskData = (PoketchGraphicsTaskData)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        switch (PoketchTasks.PoketchTask_GetState(taskMan))
        {
            case 0:
                extra.W16(0, 0);
                extra.W16(2, 0);
                PoketchTasks.PoketchTask_IncrementState(taskMan);
                goto case 1;
            case 1:
            {
                extra.W16(0, extra.U16(0) + 1);
                if (extra.U16(0) == 0)
                {
                    return;
                }
                extra.W16(0, 0);
                extra.W16(2, extra.U16(2) + 8);
                if (extra.U16(2) > 0x28)
                {
                    extra.W16(2, 0x28);
                }
                int progress = extra.U16(2);
                int rows = progress >> 2;
                int partial = progress & 3;
                int bottom = 0x16 - rows;
                Bg.Bg_FillTilemapRect(taskData.BgConfig!, BgLayerSub1, TileScreenFill, 2, 2, 0x18, (byte)rows, ScreenPalette);
                Bg.Bg_FillTilemapRect(taskData.BgConfig!, BgLayerSub1, TileScreenFill, 2, (byte)bottom, 0x18, (byte)rows, ScreenPalette);
                if (partial != 0)
                {
                    Bg.Bg_FillTilemapRect(taskData.BgConfig!, BgLayerSub1, (ushort)(0xE7 - (partial - 1)), 2, 0, 0x18, 1, ScreenPalette);
                    Bg.Bg_FillTilemapRect(taskData.BgConfig!, BgLayerSub1, (ushort)(partial + 0xE0), 2, (byte)(bottom - 1), 0x18, 1, ScreenPalette);
                }
                Bg.Bg_CopyTilemapBufferToVRAM(taskData.BgConfig!, BgLayerSub1);
                if (extra.U16(2) == 0x28)
                {
                    PoketchTasks.PoketchTask_IncrementState(taskMan);
                }
                break;
            }
            case 2:
                PoketchGraphics_EndTask(taskMan);
                break;
        }
    }

    private static void PoketchGraphics_FadeStep(PoketchTaskManager taskMan, ushort baseTile)
    {
        PoketchGraphicsTaskData taskData = (PoketchGraphicsTaskData)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        uint state = PoketchTasks.PoketchTask_GetState(taskMan);
        if (state < 4)
        {
            Bg.Bg_FillTilemapRect(taskData.BgConfig!, BgLayerSub1, (ushort)(baseTile - state), 2, 2, 0x18, 0x14, ScreenPalette);
            Bg.Bg_CopyTilemapBufferToVRAM(taskData.BgConfig!, BgLayerSub1);
            PoketchTasks.PoketchTask_IncrementState(taskMan);
            return;
        }
        PoketchGraphics_EndTask(taskMan);
    }

    // OFFSET 0x02254B8C
    private static void PoketchGraphics_TaskSwitchOut(SysTask task, object? param)
    {
        PoketchGraphics_FadeStep((PoketchTaskManager)param!, 0xE3);
    }

    // OFFSET 0x02254BF0
    private static void PoketchGraphics_TaskSwitchIn(SysTask task, object? param)
    {
        PoketchGraphics_FadeStep((PoketchTaskManager)param!, 0xE7);
    }

    // OFFSET 0x02254C54
    private static void PoketchGraphics_LoadButtonTiles(PoketchTaskManager taskMan, byte[] tiles, int bottom)
    {
        PoketchGraphicsTaskData taskData = (PoketchGraphicsTaskData)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        byte destY = (byte)(bottom == 0 ? 4 : 0xC);
        Bg.Bg_LoadToTilemapRect(taskData.BgConfig!, BgLayerSub0, tiles, 0x1C, destY, 4, 8);
        Bg.Bg_CopyTilemapBufferToVRAM(taskData.BgConfig!, BgLayerSub0);
        PoketchGraphics_EndTask(taskMan);
    }

    private static byte[] ButtonTiles(PoketchTaskManager taskMan, int index)
    {
        return ((PoketchGraphicsTaskData)PoketchTasks.PoketchTask_GetTaskData(taskMan)!).ButtonTiles[index];
    }

    // OFFSET 0x02254CA8
    private static void PoketchGraphics_TaskButtonTopPressed(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        PoketchGraphics_LoadButtonTiles(taskMan, ButtonTiles(taskMan, 1), 0);
        SoundPlayback.Sound_PlayEffect(SeqPoketchButton);
    }

    // OFFSET 0x02254CCC
    private static void PoketchGraphics_TaskButtonBottomPressed(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        PoketchGraphics_LoadButtonTiles(taskMan, ButtonTiles(taskMan, 4), 1);
        SoundPlayback.Sound_PlayEffect(SeqPoketchButton);
    }

    // OFFSET 0x02254CF4
    private static void PoketchGraphics_TaskButtonTopReleased(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        PoketchGraphics_LoadButtonTiles(taskMan, ButtonTiles(taskMan, 0), 0);
    }

    // OFFSET 0x02254D0C
    private static void PoketchGraphics_TaskButtonBottomReleased(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        PoketchGraphics_LoadButtonTiles(taskMan, ButtonTiles(taskMan, 3), 1);
    }

    // OFFSET 0x02254D24
    private static void PoketchGraphics_TaskButtonTopHeld(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        PoketchGraphics_LoadButtonTiles(taskMan, ButtonTiles(taskMan, 2), 0);
        SoundPlayback.Sound_PlayEffect(SeqPoketchButtonHeld);
    }

    // OFFSET 0x02254D48
    private static void PoketchGraphics_TaskButtonBottomHeld(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        PoketchGraphics_LoadButtonTiles(taskMan, ButtonTiles(taskMan, 5), 1);
        SoundPlayback.Sound_PlayEffect(SeqPoketchButtonHeld);
    }

    // OFFSET 0x02254D70
    private static void PoketchGraphics_TaskLoadCursor(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        PoketchGraphicsTaskData taskData = (PoketchGraphicsTaskData)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        PoketchGraphics_LoadCursor(taskData, taskData.Cursor);
        PoketchGraphics_UpdateCursor(taskData.Cursor, taskData.ConstTaskData!);
        PoketchGraphics_EndTask(taskMan);
    }

    // OFFSET 0x02254D9C
    private static void PoketchGraphics_TaskUpdateCursor(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        PoketchGraphicsTaskData taskData = (PoketchGraphicsTaskData)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        PoketchGraphics_UpdateCursor(taskData.Cursor, taskData.ConstTaskData!);
        PoketchGraphics_EndTask(taskMan);
    }

    // OFFSET 0x02254DBC
    private static void PoketchGraphics_TaskFreeCursor(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        PoketchGraphicsTaskData taskData = (PoketchGraphicsTaskData)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        PoketchGraphics_FreeCursor(taskData.Cursor);
        PoketchGraphics_EndTask(taskMan);
    }

    // OFFSET 0x02254DD8
    private static void PoketchGraphics_InitCursor(PoketchCursor cursor, PoketchAnimationManager animMan)
    {
        cursor.Loaded = 0;
        cursor.AnimMan = animMan;
    }

    // OFFSET 0x02254DE0
    private static void PoketchGraphics_LoadCursor(PoketchGraphicsTaskData taskData, PoketchCursor cursor)
    {
        if (!PoketchAnimation.PoketchAnimation_LoadSpriteFromNARC(cursor.SpriteData, NarcPoketchGraphics, 3, 4, HeapIDPoketchApp))
        {
            return;
        }
        Graphics.Graphics_LoadObjectTiles(NarcPoketchGraphics, 2, DisplaySub, 0, 0, 1, HeapIDPoketchApp);
        Graphics.Graphics_LoadPalette(NarcPoketchGraphics, 0, 5, 0, 0x60, HeapIDPoketchApp);
        PoketchGraphics_LoadCursorPalette(taskData, 0xF);
        PoketchAnimationData animData = PoketchAnimationData.FromRom(PoketchOverlay, CursorAnimData);
        cursor.Tens = PoketchAnimation.PoketchAnimation_SetupNewAnimatedSprite(cursor.AnimMan!, animData, cursor.SpriteData);
        if (cursor.Tens == null)
        {
            return;
        }
        cursor.Ones = PoketchAnimation.PoketchAnimation_SetupNewAnimatedSprite(cursor.AnimMan!, animData, cursor.SpriteData);
        if (cursor.Ones == null)
        {
            PoketchAnimation.PoketchAnimation_RemoveAnimatedSprite(cursor.AnimMan!, cursor.Tens);
            return;
        }
        PoketchAnimation.PoketchAnimation_TranslateSprite(cursor.Ones, 0x10000, 0);
        PoketchAnimation.PoketchAnimation_SetCParam(cursor.Tens, 0xF);
        PoketchAnimation.PoketchAnimation_SetCParam(cursor.Ones, 0xF);
        cursor.Loaded = 1;
    }

    // OFFSET 0x02254E84
    private static void PoketchGraphics_LoadCursorPalette(PoketchGraphicsTaskData taskData, uint objOffset)
    {
        PoketchGraphics_CopyActivePalette(taskData.TempPalette);
        (taskData.TempPalette[1], taskData.TempPalette[4]) = (taskData.TempPalette[4], taskData.TempPalette[1]);
        (taskData.TempPalette[8], taskData.TempPalette[0xF]) = (taskData.TempPalette[0xF], taskData.TempPalette[8]);
        Video.LoadPaletteRange(Video.SubObject, taskData.TempPalette, 0, objOffset * 0x20, 0x20);
    }

    // OFFSET 0x02254EE8
    private static void PoketchGraphics_UpdateCursor(PoketchCursor cursor, PoketchConstTaskData constTaskData)
    {
        if (cursor.Loaded == 0)
        {
            return;
        }
        uint number = (uint)(constTaskData.AppID + 1);
        PoketchAnimation.PoketchAnimation_UpdateAnimationIdx(cursor.Tens!, number / 10);
        PoketchAnimation.PoketchAnimation_UpdateAnimationIdx(cursor.Ones!, number % 10);
    }

    // OFFSET 0x02254F40
    private static void PoketchGraphics_FreeCursor(PoketchCursor cursor)
    {
        if (cursor.Loaded == 0)
        {
            return;
        }
        PoketchAnimation.PoketchAnimation_RemoveAnimatedSprite(cursor.AnimMan!, cursor.Tens!);
        PoketchAnimation.PoketchAnimation_RemoveAnimatedSprite(cursor.AnimMan!, cursor.Ones!);
        PoketchAnimation.PoketchAnimation_FreeSpriteData(cursor.SpriteData);
        cursor.Loaded = 0;
    }

    // OFFSET 0x02254F68
    private static void PoketchGraphics_TaskDimScreen(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        PoketchGraphicsTaskData taskData = (PoketchGraphicsTaskData)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        uint state = PoketchTasks.PoketchTask_GetState(taskMan);
        if (state > 3)
        {
            return;
        }
        if (state == 0)
        {
            Video.SetBlendAlpha(DisplaySub, 2, 0x1C, 0x1A, 6);
            Bg.Bg_CopyTilemapBufferToVRAM(taskData.BgConfig!, BgLayerSub0);
        }
        Bg.Bg_FillTilemapRect(taskData.BgConfig!, BgLayerSub1, (ushort)(0xE7 - state), 2, 2, 0x18, 0x14, ScreenPalette);
        Bg.Bg_CopyTilemapBufferToVRAM(taskData.BgConfig!, BgLayerSub1);
        if (state == 3)
        {
            PoketchGraphics_EndTask(taskMan);
            return;
        }
        PoketchTasks.PoketchTask_IncrementState(taskMan);
    }

    // OFFSET 0x02255004
    private static void PoketchGraphics_TaskUndimScreen(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        PoketchGraphicsTaskData taskData = (PoketchGraphicsTaskData)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        Bg.Bg_FillTilemapRect(taskData.BgConfig!, BgLayerSub1, TileBlank, 2, 2, 0x18, 0x14, ScreenPalette);
        Bg.Bg_CopyTilemapBufferToVRAM(taskData.BgConfig!, BgLayerSub0);
        Bg.Bg_CopyTilemapBufferToVRAM(taskData.BgConfig!, BgLayerSub1);
        Video.SetBlendAlpha(DisplaySub, 0, 0, 0x1F, 0);
        PoketchGraphics_EndTask(taskMan);
    }

    // OFFSET 0x02255064
    private static void PoketchGraphics_TaskFreeBackgrounds(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        PoketchGraphicsTaskData taskData = (PoketchGraphicsTaskData)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        Bg.Bg_FreeTilemapBuffer(taskData.BgConfig!, BgLayerSub0);
        Bg.Bg_FreeTilemapBuffer(taskData.BgConfig!, BgLayerSub1);
        PoketchGraphics_EndTask(taskMan);
    }
}
