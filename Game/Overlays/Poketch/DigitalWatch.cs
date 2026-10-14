namespace PlatinumSharp.Game;

public sealed class DigitalWatchApp
{
    public byte State;
    public byte SubState;
    public byte ShouldExit;
    public byte Unused3;
    public byte Hour;
    public byte Minute;
    public byte UpdateBacklight;
    public PoketchButtonManager? ButtonManager;
    public RtcTime Time;
    public int Backlight;
    public DigitalWatchGraphics? Graphics;
    public PoketchSystem? PoketchSys;
}

public sealed class DigitalWatchGraphics
{
    public const int DigitTilemapWidth = 40;
    public const int DigitTilemapHeight = 9;

    public DigitalWatchApp? App;
    public BgConfig? BgConfig;
    public readonly PoketchActiveTaskList ActiveTasks = new();
    public readonly byte[] DigitTiles = new byte[DigitTilemapWidth * DigitTilemapHeight * 2];
}

public static class DigitalWatch
{
    private const int Overlay = 26;
    private const int HeapIDPoketch = 8;
    private const uint MainTaskPriority = 1;
    private const uint GraphicsTaskPriority = 2;
    private const int ActiveTaskSlots = 8;
    private const uint ButtonHitTable = 0x02256718;
    private const uint BgTemplate = 0x02256728;
    private const int BgLayerSub2 = 6;
    private const int NarcPoketchGraphics = 0xC;
    private const byte StateLoad = 0;
    private const byte StateUpdate = 1;
    private const byte StateUnload = 2;
    private const uint TaskSetup = 0;
    private const uint TaskDrawTime = 1;
    private const uint TaskBacklight = 2;
    private const uint TaskFreeBackground = 3;

    private static readonly Func<DigitalWatchApp, bool>[] sStateFuncs =
    {
        DigitalWatch_StateLoad,
        DigitalWatch_StateUpdate,
        DigitalWatch_StateUnload,
    };

    private static readonly PoketchTask[] sGraphicsTasks =
    {
        new(TaskSetup, DigitalWatchGraphics_TaskSetup, 0),
        new(TaskDrawTime, DigitalWatchGraphics_TaskDrawTime, 0),
        new(TaskBacklight, DigitalWatchGraphics_TaskBacklight, 0),
        new(TaskFreeBackground, DigitalWatchGraphics_TaskFreeBackground, 0),
        new(PoketchActiveTaskList.NoTask, null, 0),
    };

    // OFFSET 0x022561C0
    public static void DigitalWatch_RegisterAppFunctions()
    {
        PoketchSystems.PoketchSystem_SetAppFunctions(DigitalWatch_Init, DigitalWatch_Shutdown);
    }

    // OFFSET 0x022561D4
    private static bool DigitalWatch_Init(out object? appPtr, PoketchSystem poketchSys, BgConfig bgConfig, int appID)
    {
        appPtr = null;
        DigitalWatchApp app = new();
        if (!DigitalWatch_Setup(app, poketchSys, bgConfig, appID))
        {
            return false;
        }
        if (SysTask.SysTask_Start(DigitalWatch_Main, app, MainTaskPriority) == null)
        {
            return false;
        }
        appPtr = app;
        return true;
    }

    // OFFSET 0x0225621C
    private static bool DigitalWatch_Setup(DigitalWatchApp app, PoketchSystem poketchSys, BgConfig bgConfig, int appID)
    {
        if (!DigitalWatchGraphics_New(out app.Graphics, app))
        {
            return false;
        }
        app.State = StateLoad;
        app.SubState = 0;
        app.ShouldExit = 0;
        app.Unused3 = 1;
        app.UpdateBacklight = 0;
        app.Backlight = 0;
        Rtc.RTC_GetCurrentTime(out app.Time);
        if (app.Time.Hour >= 24)
        {
            app.Time.Hour = (int)((uint)app.Time.Hour % 24);
        }
        if (app.Time.Minute >= 60)
        {
            app.Time.Hour = (int)((uint)app.Time.Hour % 60);
            app.Time.Minute = (int)((uint)app.Time.Minute % 60);
        }
        app.Minute = (byte)app.Time.Minute;
        app.Hour = (byte)app.Time.Hour;
        TouchScreenHitTable[] hitTable = PoketchButtons.HitTableFromRom(Overlay, ButtonHitTable, 1);
        app.ButtonManager = PoketchButtons.PoketchButtonManager_New(hitTable, 1, DigitalWatch_OnButtonEvent, app, HeapIDPoketch);
        if (app.ButtonManager == null)
        {
            return false;
        }
        app.PoketchSys = poketchSys;
        return true;
    }

    // OFFSET 0x022562A4
    private static void DigitalWatch_Free(DigitalWatchApp app)
    {
        DigitalWatchGraphics_Free(app.Graphics);
        PoketchButtons.PoketchButtonManager_Free(app.ButtonManager);
    }

    // OFFSET 0x022562BC
    private static void DigitalWatch_Main(SysTask task, object? param)
    {
        DigitalWatchApp app = (DigitalWatchApp)param!;
        if (app.State >= 3)
        {
            return;
        }
        PoketchSystems.PoketchSystem_UpdateButtonManager(app.PoketchSys!, app.ButtonManager!);
        if (sStateFuncs[app.State](app))
        {
            DigitalWatch_Free(app);
            SysTask.SysTask_Done(task);
            PoketchSystems.PoketchSystem_NotifyAppUnloaded(app.PoketchSys!);
        }
    }

    // OFFSET 0x022562F8
    private static void DigitalWatch_Shutdown(object? appData)
    {
        ((DigitalWatchApp)appData!).ShouldExit = 1;
    }

    // OFFSET 0x02256300
    private static void DigitalWatch_OnButtonEvent(uint buttonIdx, uint buttonState, uint touchState, object? data)
    {
        DigitalWatchApp app = (DigitalWatchApp)data!;
        if (touchState == 0)
        {
            app.Backlight = 0;
            app.UpdateBacklight = 1;
        }
        else if (touchState == 1)
        {
            app.Backlight = 1;
            app.UpdateBacklight = 1;
        }
    }

    // OFFSET 0x0225631C
    private static void DigitalWatch_SetState(DigitalWatchApp app, byte state)
    {
        if (app.ShouldExit != 0)
        {
            state = StateUnload;
        }
        app.State = state;
        app.SubState = 0;
    }

    // OFFSET 0x02256330
    private static bool DigitalWatch_StateLoad(DigitalWatchApp app)
    {
        switch (app.SubState)
        {
            case 0:
                DigitalWatchGraphics_StartTask(app.Graphics!, TaskSetup);
                app.SubState++;
                break;
            case 1:
                if (DigitalWatchGraphics_TaskIsNotActive(app.Graphics!, TaskSetup))
                {
                    PoketchSystems.PoketchSystem_NotifyAppLoaded(app.PoketchSys!);
                    DigitalWatch_SetState(app, StateUpdate);
                }
                break;
        }
        return false;
    }

    // OFFSET 0x02256370
    private static bool DigitalWatch_StateUpdate(DigitalWatchApp app)
    {
        if (app.ShouldExit != 0)
        {
            DigitalWatch_SetState(app, StateUnload);
            return false;
        }
        if (app.UpdateBacklight != 0)
        {
            app.UpdateBacklight = 0;
            DigitalWatchGraphics_StartTask(app.Graphics!, TaskBacklight);
        }
        if (DigitalWatchGraphics_TaskIsNotActive(app.Graphics!, TaskDrawTime))
        {
            app.Minute = (byte)app.Time.Minute;
            app.Hour = (byte)app.Time.Hour;
            Rtc.RTC_GetCurrentTime(out app.Time);
            if (app.Minute != app.Time.Minute || app.Hour != app.Time.Hour)
            {
                DigitalWatchGraphics_StartTask(app.Graphics!, TaskDrawTime);
            }
        }
        return false;
    }

    // OFFSET 0x022563D0
    private static bool DigitalWatch_StateUnload(DigitalWatchApp app)
    {
        switch (app.SubState)
        {
            case 0:
                DigitalWatchGraphics_StartTask(app.Graphics!, TaskFreeBackground);
                app.SubState++;
                break;
            case 1:
                if (DigitalWatchGraphics_NoActiveTasks(app.Graphics!))
                {
                    return true;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x02256404
    private static bool DigitalWatchGraphics_New(out DigitalWatchGraphics? graphicsPtr, DigitalWatchApp app)
    {
        DigitalWatchGraphics graphics = new();
        PoketchTasks.PoketchTask_InitActiveTaskList(graphics.ActiveTasks, ActiveTaskSlots);
        graphics.App = app;
        graphics.BgConfig = PoketchGraphics.PoketchGraphics_GetBgConfig();
        byte[]? file = Graphics.Graphics_GetScrnData(NarcPoketchGraphics, 0x19, 1, out ScreenData? screenData, HeapIDPoketch);
        if (file == null)
        {
            graphicsPtr = null;
            return false;
        }
        DigitalWatchGraphics_CopyDigitTiles(screenData!.RawData, graphics.DigitTiles);
        graphicsPtr = graphics;
        return true;
    }

    // OFFSET 0x0225646C
    private static void DigitalWatchGraphics_CopyDigitTiles(byte[] source, byte[] destination)
    {
        int lowerOffset = 0x240;
        int sourceOffset = 0;
        int destinationOffset = 0;
        for (int row = 0; row < DigitalWatchGraphics.DigitTilemapHeight; row++)
        {
            Array.Copy(source, sourceOffset, destination, destinationOffset, 0x40);
            Array.Copy(source, sourceOffset + lowerOffset, destination, destinationOffset + 0x40, 0x10);
            sourceOffset += 0x40;
            destinationOffset += 0x50;
            lowerOffset -= 0x30;
        }
    }

    // OFFSET 0x0225649C
    private static void DigitalWatchGraphics_Free(DigitalWatchGraphics? graphics)
    {
    }

    // OFFSET 0x022564A8
    private static void DigitalWatchGraphics_StartTask(DigitalWatchGraphics graphics, uint taskID)
    {
        PoketchTasks.PoketchTask_Start(sGraphicsTasks, taskID, graphics, graphics.App, graphics.ActiveTasks, GraphicsTaskPriority, HeapIDPoketch);
    }

    // OFFSET 0x022564CC
    private static bool DigitalWatchGraphics_TaskIsNotActive(DigitalWatchGraphics graphics, uint taskID)
    {
        return PoketchTasks.PoketchTask_TaskIsNotActive(graphics.ActiveTasks, taskID);
    }

    // OFFSET 0x022564D8
    private static bool DigitalWatchGraphics_NoActiveTasks(DigitalWatchGraphics graphics)
    {
        return PoketchTasks.PoketchTask_NoActiveTasks(graphics.ActiveTasks);
    }

    // OFFSET 0x022564E4
    private static void DigitalWatchGraphics_EndTask(PoketchTaskManager taskMan)
    {
        DigitalWatchGraphics graphics = (DigitalWatchGraphics)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        PoketchTasks.PoketchTask_EndTask(graphics.ActiveTasks, taskMan);
    }

    // OFFSET 0x022564F8
    private static void DigitalWatchGraphics_TaskSetup(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        DigitalWatchGraphics graphics = (DigitalWatchGraphics)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        Bg.Bg_InitFromTemplate(graphics.BgConfig!, BgLayerSub2, RomStructs.BgTemplate(RomData.Overlay(Overlay, BgTemplate, RomStructs.BgTemplateSize), 0), 0);
        Graphics.Graphics_LoadTilesToBgLayer(NarcPoketchGraphics, 0x17, graphics.BgConfig!, BgLayerSub2, 0, 0, 1, HeapIDPoketch);
        Graphics.Graphics_LoadTilemapToBgLayer(NarcPoketchGraphics, 0x18, graphics.BgConfig!, BgLayerSub2, 0, 0, 1, HeapIDPoketch);
        PoketchGraphics.PoketchGraphics_LoadActivePalette(0, 0);
        DigitalWatchGraphics_DrawTime(graphics);
        Bg.Bg_CopyTilemapBufferToVRAM(graphics.BgConfig!, BgLayerSub2);
        uint control = Video.GetDisplayControl(1);
        Video.SetDisplayControl(1, (control & 0xFFFFE0FF) | ((((control >> 8) & 0x1F) | 4) << 8));
        DigitalWatchGraphics_EndTask(taskMan);
    }

    // OFFSET 0x0225658C
    private static void DigitalWatchGraphics_TaskDrawTime(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        DigitalWatchGraphics graphics = (DigitalWatchGraphics)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        DigitalWatchGraphics_DrawTime(graphics);
        Bg.Bg_CopyTilemapBufferToVRAM(graphics.BgConfig!, BgLayerSub2);
        DigitalWatchGraphics_EndTask(taskMan);
    }

    // OFFSET 0x022565AC
    private static void DigitalWatchGraphics_TaskBacklight(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        DigitalWatchGraphics graphics = (DigitalWatchGraphics)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        if (graphics.App!.Backlight != 0)
        {
            PoketchGraphics.PoketchGraphics_LoadActiveBacklightPalette(0, 0);
        }
        else
        {
            PoketchGraphics.PoketchGraphics_LoadActivePalette(0, 0);
        }
        DigitalWatchGraphics_EndTask(taskMan);
    }

    // OFFSET 0x022565D8
    private static void DigitalWatchGraphics_TaskFreeBackground(SysTask task, object? param)
    {
        PoketchTaskManager taskMan = (PoketchTaskManager)param!;
        DigitalWatchGraphics graphics = (DigitalWatchGraphics)PoketchTasks.PoketchTask_GetTaskData(taskMan)!;
        Bg.Bg_FreeTilemapBuffer(graphics.BgConfig!, BgLayerSub2);
        DigitalWatchGraphics_EndTask(taskMan);
    }

    private static void DigitalWatchGraphics_DrawDigit(DigitalWatchGraphics graphics, int destX, uint digit)
    {
        Bg.Bg_CopyToTilemapRect(graphics.BgConfig!, BgLayerSub2, destX, 7, 4, 9, graphics.DigitTiles, (int)((digit & 0x3F) << 2), 0, DigitalWatchGraphics.DigitTilemapWidth, DigitalWatchGraphics.DigitTilemapHeight);
    }

    // OFFSET 0x022565F4
    private static void DigitalWatchGraphics_DrawTime(DigitalWatchGraphics graphics)
    {
        uint hour = (uint)graphics.App!.Time.Hour;
        DigitalWatchGraphics_DrawDigit(graphics, 3, hour / 10);
        DigitalWatchGraphics_DrawDigit(graphics, 8, hour % 10);
        uint minute = (uint)graphics.App.Time.Minute;
        DigitalWatchGraphics_DrawDigit(graphics, 0xF, minute / 10);
        DigitalWatchGraphics_DrawDigit(graphics, 0x14, minute % 10);
    }
}
