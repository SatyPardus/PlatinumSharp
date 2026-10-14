namespace PlatinumSharp.Game;

public static partial class BattleSubscreens
{
    private const int SubscreenPanelLayer = 6;
    private const int SubscreenOverlayLayer = 7;
    private const uint TaskPriorityPanelSlide = 0x4B0;
    private const uint TaskPriorityCurtain = 0x4BA;
    private const int CurtainSfx = 0x70E;
    private const short CurtainSlideXStart = -25500;
    private const short CurtainSlideYStart = 4000;
    private const int CurtainSlideXInit = 0x639C;
    private const int CurtainSlideYInit = 0xFA0;
    private const int CurtainSlideXStep = 0x18E7;
    private const int CurtainSlideYStep = 1000;
    private const short CurtainSlideYEnd = 0x90;
    private const int CurtainSplitLine = 0x90;
    private const int ScreenHeight = 0xC0;
    private const int CurtainFixedScale = 100;

    // OFFSET 0x0226940C
    private static void BattleSubscreen_UpdatePanelWindow(BattleSubscreen subscreen)
    {
        Video.SetVisibleWindows(DisplaySub, 1);
        Video.SetWindowOutside(DisplaySub, 0x3F);
        Video.SetWindowInside(DisplaySub, 0, 0x3B);
        int left = 0xFF - subscreen.PanelScrollPos / 256;
        if (left < 0)
        {
            left = 0;
        }
        Video.SetWindowHorizontal(DisplaySub, 0, (byte)left, 0xFF);
        Video.SetWindowVertical(DisplaySub, 0, 0, ScreenHeight);
    }

    // OFFSET 0x0226947C
    public static void BattleSubscreen_StartPanelSlide(BattleSubscreen subscreen, int speed, int target)
    {
        target <<= 8;
        if (subscreen.PanelScrollPos != target)
        {
            subscreen.SlideSpeed = speed;
            subscreen.TargetOffset = target;
            SysTask.SysTask_Start(BattleSubscreen_PanelSlideTask, subscreen, TaskPriorityPanelSlide);
        }
    }

    // OFFSET 0x022694A8
    private static void BattleSubscreen_PanelSlideTask(SysTask task, object? param)
    {
        BattleSubscreen subscreen = (BattleSubscreen)param!;
        int done = 0;
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(subscreen.BattleSys!)!;
        subscreen.PanelScrollPos += subscreen.SlideSpeed;
        if ((subscreen.SlideSpeed <= 0 && subscreen.PanelScrollPos <= subscreen.TargetOffset)
            || (subscreen.SlideSpeed > 0 && subscreen.PanelScrollPos >= subscreen.TargetOffset))
        {
            done = 1;
            subscreen.PanelScrollPos = subscreen.TargetOffset;
        }
        Bg.Bg_SetOffset(bgConfig, SubscreenPanelLayer, BgOffsetSetX, subscreen.PanelScrollPos / 256);
        BattleSubscreen_UpdatePanelWindow(subscreen);
        if (done == 1)
        {
            Bg.Bg_FillTilemap(bgConfig, SubscreenOverlayLayer, BlankTile);
            Bg.Bg_ToggleLayer(SubscreenOverlayLayer, 0);
            Bg.Bg_SetPriority(SubscreenOverlayLayer, 0);
            Video.SetVisibleWindows(DisplaySub, 0);
            SysTask.SysTask_Done(task);
        }
    }

    // OFFSET 0x0226B988
    private static void BattleSubscreen_CurtainTask(SysTask task, object? param)
    {
        CurtainTaskData data = (CurtainTaskData)param!;
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(data.Subscreen!.BattleSys!)!;
        switch (data.State)
        {
            case 0:
                data.SlideX = (short)(data.SlideX + CurtainSlideXStep);
                data.SlideY = (short)(data.SlideY - CurtainSlideYStep);
                data.ScrollX = (short)((CurtainSlideXInit - data.SlideX) / CurtainFixedScale);
                data.ScrollY = (short)(-data.SlideY / CurtainFixedScale);
                if (data.SlideX < 0 && data.SlideY > 0)
                {
                    return;
                }
                data.SlideX = 0;
                data.SlideY = CurtainSlideYEnd;
                data.ScrollX = 0;
                data.ScrollY = 0;
                data.State++;
                break;
            case 1:
                data.Subscreen.IsPanelSliding = 0;
                SysTask.SysTask_Done(data.VBlankTask!);
                GameSystem.DisableHBlank();
                Video.SetVisibleWindows(DisplaySub, 0);
                BattleSubscreen_SetCommandMenuActive(data.Subscreen, 1);
                data.State++;
                break;
            default:
                if (BattleSubscreen_NoPendingTask(data.Subscreen) != 0)
                {
                    Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer, BgOffsetSetX, 0);
                    Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer, BgOffsetSetY, 0);
                    Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer + 1, BgOffsetSetX, 0);
                    Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer + 1, BgOffsetSetY, 0);
                    BattleSystems.BattleSystem_SetCommandSelectionFlags(data.Subscreen.BattleSys!, 1);
                    SysTask.SysTask_Done(task);
                }
                break;
        }
    }

    // OFFSET 0x0226BA88
    private static void BattleSubscreen_CurtainVBlankTask(SysTask task, object? param)
    {
        CurtainTaskData data = (CurtainTaskData)param!;
        BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(data.Subscreen!.BattleSys!)!;
        int edge;
        if (data.BattlerType == BattlerTypePlayerSlot2)
        {
            int x = 0xFF - data.ScrollX;
            if (x > 0)
            {
                x = 0;
            }
            Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer, BgOffsetSetX, x);
            Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer + 1, BgOffsetSetX, 0xFF - data.ScrollX);
            edge = -data.SlideX / CurtainFixedScale;
        }
        else
        {
            int x = data.ScrollX;
            if (x < 0)
            {
                x = 0;
            }
            Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer, BgOffsetSetX, x);
            Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer + 1, BgOffsetSetX, data.ScrollX);
            edge = data.SlideX / CurtainFixedScale + 0xFF;
        }
        Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer, BgOffsetSetY, 0);
        Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer + 1, BgOffsetSetY, 0);
        int bottom = data.SlideY / CurtainFixedScale + CurtainSplitLine;
        if (edge < 0)
        {
            edge = 0;
        }
        else if (edge > 0xFF)
        {
            edge = 0xFF;
        }
        if (bottom > ScreenHeight)
        {
            bottom = ScreenHeight;
        }
        else if (bottom < 0)
        {
            bottom = 0;
        }
        if (data.BattlerType == BattlerTypePlayerSlot2)
        {
            Video.SetWindowHorizontal(DisplaySub, 0, 0, (byte)edge);
            Video.SetWindowVertical(DisplaySub, 0, 0, CurtainSplitLine);
        }
        else
        {
            if (edge == 0)
            {
                edge = 1;
            }
            Video.SetWindowHorizontal(DisplaySub, 0, (byte)edge, 0);
            Video.SetWindowVertical(DisplaySub, 0, 0, CurtainSplitLine);
        }
        Video.SetWindowHorizontal(DisplaySub, 1, 0, 0xFF);
        Video.SetWindowVertical(DisplaySub, 1, CurtainSplitLine, (byte)bottom);
        data.HBlankScrollY = data.ScrollY;
    }

    // OFFSET 0x0226BB94
    private static void BattleSubscreen_CurtainHBlank(object? param)
    {
        CurtainTaskData data = (CurtainTaskData)param!;
        int line = Video.VCount;
        if (line == CurtainSplitLine)
        {
            BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(data.Subscreen!.BattleSys!)!;
            Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer, BgOffsetSetX, 0);
            Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer, BgOffsetSetY, data.HBlankScrollY);
            Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer + 1, BgOffsetSetX, 0);
            Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer + 1, BgOffsetSetY, data.HBlankScrollY);
        }
        else if (line > ScreenHeight)
        {
            BgConfig bgConfig = BattleSystems.BattleSystem_GetBgConfig(data.Subscreen!.BattleSys!)!;
            if (data.BattlerType == BattlerTypePlayerSlot2)
            {
                Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer, BgOffsetSetX, 0xFF - data.ScrollX);
                Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer + 1, BgOffsetSetX, 0xFF - data.ScrollX);
            }
            else
            {
                Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer, BgOffsetSetX, data.ScrollX);
                Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer + 1, BgOffsetSetX, data.ScrollX);
            }
            Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer, BgOffsetSetY, 0);
            Bg.Bg_SetOffset(bgConfig, SubscreenFirstLayer + 1, BgOffsetSetY, 0);
        }
    }

    // OFFSET 0x0226B198
    private static void BattleSubscreen_RestoreMenuLayers(BattleSubscreen subscreen)
    {
        for (int i = 0; i < BattleMenuConfig.LayerCount; i++)
        {
            int layer = SubscreenFirstLayer + i;
            if (layer == SubscreenPanelLayer)
            {
                Bg.Bg_ToggleLayer((byte)layer, 1);
            }
            else
            {
                Bg.Bg_ToggleLayer((byte)layer, 0);
            }
        }
    }

    // OFFSET 0x0226B1C4
    private static void BattleSubscreen_FinishMenu(BattleSubscreen subscreen, int deleteSprites, int hideLayers)
    {
        if (deleteSprites == 1)
        {
            BattleSubscreen_DeleteMenuSprites(subscreen);
        }
        if (hideLayers == 1)
        {
            SysTask.SysTask_ExecuteAfterVBlank(BattleSubscreen_RestoreMenuLayersTask, subscreen, TaskPriorityMenuTransfer);
        }
    }

    // OFFSET 0x0226BCBC
    private static void BattleSubscreen_RestoreMenuLayersTask(SysTask task, object? param)
    {
        BattleSubscreen_RestoreMenuLayers((BattleSubscreen)param!);
        SysTask.SysTask_Done(task);
    }
}
