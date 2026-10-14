namespace PlatinumSharp.Game;

public sealed class TouchIndicator
{
    public ManagedSprite? Sprite;
    public SysTask? Task;
    public int X;
    public int Y;
    public int Angle;
    public int SubscreenOffset;
    public short Timer;
    public byte TimerExpired;
    public byte Tapped;
    public byte State;
    public byte TapPhase;
    public byte TapCounter;
}

public static partial class BattleSubscreens
{
    private const int NarcTouchIndicator = 0x52;
    private const int IndicatorPaletteMember = 0xB;
    private const int IndicatorCharMember = 0xA;
    private const int IndicatorCellMember = 0xC;
    private const int IndicatorAnimMember = 0xD;
    private const uint IndicatorTemplate = 0x02270AD8;
    private const int IndicatorDefaultOffset = 0xC0000;
    private const uint TaskPriorityIndicator = 999;
    private const int IndicatorAngleStep = 1000;
    private const int IndicatorAngleWrap = 18000;
    private const int IndicatorAmplitude = 0xE000;
    private const int IndicatorTapDepth = 8;
    private const int IndicatorReleaseDepth = 2;
    private const int ResourceIndicatorTiles = 0x4FBC;
    private const int ResourceIndicatorPalette = 0x4E44;
    private const int ResourceIndicatorCell = 0x4FB6;
    private const int ResourceIndicatorAnim = 0x4FAB;
    private const int IndicatorPriority = 0xA;
    private const short TutorialTapTimer = 60;
    private const int TutorialLowHPDelay = 60;
    private const int TutorialStepCount = 2;

    // OFFSET 0x0226CD18
    private static int BattleSubscreen_CatchTutorialInput(BattleSubscreen subscreen)
    {
        BattleSystems.BattleSystem_GetSpriteSystem(subscreen.BattleSys!);
        BattleSystems.BattleSystem_GetSpriteManager(subscreen.BattleSys!);
        BattleSystems.BattleSystem_GetPaletteData(subscreen.BattleSys!);
        int result;
        if (BattleSystems.BattleSystem_GetCatchingTutorialLowHP(subscreen.BattleSys!) == 0)
        {
            if (subscreen.CatchTutorialState.Step >= TutorialStepCount)
            {
                ErrorHandling.GF_AssertFail();
            }
            result = subscreen.CatchTutorialState.Step == 0
                ? BattleSubscreen_CatchTutorialTapFight(subscreen)
                : BattleSubscreen_CatchTutorialTapBag(subscreen);
        }
        else
        {
            result = BattleSubscreen_CatchTutorialTapBagLowHP(subscreen);
        }
        if (result != TouchScreen.ButtonNotPressed)
        {
            subscreen.CatchTutorialState.Phase = 0;
            subscreen.CatchTutorialState.Delay = 0;
            subscreen.CatchTutorialState.Step++;
        }
        return result;
    }

    // OFFSET 0x0226CD84
    private static int BattleSubscreen_CatchTutorialTapFight(BattleSubscreen subscreen)
    {
        BattleSubscreenCatchTutorialState state = subscreen.CatchTutorialState;
        if (state.Phase == 0)
        {
            TouchIndicator_ShowAt((TouchIndicator)state.Indicator!, 0x80, 0x3C, SubscreenYOffset);
            TouchIndicator_SetTimer((TouchIndicator)state.Indicator!, TutorialTapTimer);
            state.Phase++;
        }
        else if (TouchIndicator_IsTapped((TouchIndicator)state.Indicator!) == 1)
        {
            state.Phase++;
            return 0;
        }
        return TouchScreen.ButtonNotPressed;
    }

    // OFFSET 0x0226CDDC
    private static int BattleSubscreen_CatchTutorialTapBag(BattleSubscreen subscreen)
    {
        BattleSubscreenCatchTutorialState state = subscreen.CatchTutorialState;
        if (state.Phase == 0)
        {
            TouchIndicator_ShowAt((TouchIndicator)state.Indicator!, 0x40, 0x16, SubscreenYOffset);
            TouchIndicator_SetTimer((TouchIndicator)state.Indicator!, TutorialTapTimer);
            state.Phase++;
        }
        else if (TouchIndicator_IsTapped((TouchIndicator)state.Indicator!) == 1)
        {
            state.Phase++;
            return 1;
        }
        return TouchScreen.ButtonNotPressed;
    }

    // OFFSET 0x0226CE34
    private static int BattleSubscreen_CatchTutorialTapBagLowHP(BattleSubscreen subscreen)
    {
        BattleSubscreenCatchTutorialState state = subscreen.CatchTutorialState;
        switch (state.Phase)
        {
            case 0:
                state.Delay++;
                if (state.Delay > TutorialLowHPDelay)
                {
                    state.Delay = 0;
                    state.Phase++;
                }
                break;
            case 1:
                TouchIndicator_ShowAt((TouchIndicator)state.Indicator!, 0x28, 0x92, SubscreenYOffset);
                TouchIndicator_SetTimer((TouchIndicator)state.Indicator!, TutorialTapTimer);
                state.Phase++;
                break;
            default:
                if (TouchIndicator_IsTapped((TouchIndicator)state.Indicator!) == 1)
                {
                    state.Phase++;
                    return 1;
                }
                break;
        }
        return TouchScreen.ButtonNotPressed;
    }

    // OFFSET 0x0226DE44
    internal static void TouchIndicator_LoadResources(SpriteSystem spriteSys, SpriteManager spriteMan, int heapID, PaletteData paletteData, int charID, int paletteID, int cellID, int animID)
    {
        NARC narc = NARC.NARC_ctor(NarcTouchIndicator, heapID);
        SpriteSystem.SpriteSystem_LoadPaletteBufferFromOpenNarc(paletteData, PaletteBufferSubObj, spriteSys, spriteMan, narc, IndicatorPaletteMember, 0, 1, VRamSub, paletteID);
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSys, spriteMan, narc, IndicatorCharMember, 0, VRamSub, charID);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(spriteSys, spriteMan, narc, IndicatorCellMember, 0, cellID);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(spriteSys, spriteMan, narc, IndicatorAnimMember, 0, animID);
        NARC.NARC_dtor(narc);
    }

    // OFFSET 0x0226DEC4
    internal static void TouchIndicator_UnloadResources(SpriteManager spriteMan, int charID, int paletteID, int cellID, int animID)
    {
        SpriteSystem.SpriteManager_UnloadCharObjById(spriteMan, charID);
        SpriteSystem.SpriteManager_UnloadPlttObjById(spriteMan, paletteID);
        SpriteSystem.SpriteManager_UnloadCellObjById(spriteMan, cellID);
        SpriteSystem.SpriteManager_UnloadAnimObjById(spriteMan, animID);
    }

    // OFFSET 0x0226DEEC
    internal static TouchIndicator TouchIndicator_New(SpriteSystem spriteSys, SpriteManager spriteMan, int heapID, int charID, int paletteID, int cellID, int animID, int priority, int bgPriority)
    {
        SpriteTemplate template = RomSpriteTemplate(IndicatorTemplate);
        template.Resources[0] = charID;
        template.Resources[1] = paletteID;
        template.Resources[2] = cellID;
        template.Resources[3] = animID;
        template.Priority = priority;
        template.BgPriority = bgPriority;
        TouchIndicator indicator = new();
        indicator.Sprite = SpriteSystem.SpriteSystem_NewSprite(spriteSys, spriteMan, template);
        SpriteSystem.ManagedSprite_SetDrawFlag(indicator.Sprite!, 0);
        indicator.SubscreenOffset = IndicatorDefaultOffset;
        indicator.Task = SysTask.SysTask_Start(TouchIndicator_Task, indicator, TaskPriorityIndicator);
        return indicator;
    }

    // OFFSET 0x0226DF68
    internal static void TouchIndicator_Free(TouchIndicator indicator)
    {
        SpriteSystem.Sprite_DeleteAndFreeResources(indicator.Sprite!);
        SysTask.SysTask_Done(indicator.Task!);
    }

    // OFFSET 0x0226DF80
    internal static void TouchIndicator_ShowAt(TouchIndicator indicator, int x, int y, int subscreenOffset)
    {
        TouchIndicator_Reset(indicator);
        indicator.X = x;
        indicator.Y = y;
        indicator.SubscreenOffset = subscreenOffset;
        SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(indicator.Sprite!, (short)x, (short)y, subscreenOffset);
        SpriteSystem.ManagedSprite_SetDrawFlag(indicator.Sprite!, 1);
    }

    // OFFSET 0x0226DFB0
    internal static void TouchIndicator_ShowAtDefaultOffset(TouchIndicator indicator, int x, int y)
    {
        TouchIndicator_ShowAt(indicator, x, y, IndicatorDefaultOffset);
    }

    // OFFSET 0x0226DFBC
    internal static void TouchIndicator_Hide(TouchIndicator indicator)
    {
        SpriteSystem.ManagedSprite_SetDrawFlag(indicator.Sprite!, 0);
        TouchIndicator_Reset(indicator);
    }

    // OFFSET 0x0226DFD0
    internal static void TouchIndicator_SetTimer(TouchIndicator indicator, short timer)
    {
        indicator.Timer = timer;
    }

    // OFFSET 0x0226DFD4
    internal static byte TouchIndicator_IsTapped(TouchIndicator indicator)
    {
        return indicator.Tapped;
    }

    // OFFSET 0x0226DFD8
    internal static void TouchIndicator_Task(SysTask task, object? param)
    {
        TouchIndicator indicator = (TouchIndicator)param!;
        if (indicator.Tapped == 1)
        {
            indicator.Tapped = 0;
        }
        if (indicator.Timer > 0)
        {
            indicator.Timer--;
            if (indicator.Timer == 0)
            {
                indicator.TimerExpired = 1;
            }
        }
        if (SpriteSystem.ManagedSprite_GetDrawFlag(indicator.Sprite!) == 0)
        {
            return;
        }
        if (indicator.State == 0)
        {
            indicator.Angle += IndicatorAngleStep;
            if (indicator.Angle >= IndicatorAngleWrap)
            {
                indicator.Angle -= IndicatorAngleWrap;
                if (indicator.TimerExpired == 1)
                {
                    indicator.State = 1;
                    indicator.TimerExpired = 0;
                }
            }
        }
        if (indicator.State == 0)
        {
            int sine = MathUtil.CalcSineDegrees((ushort)(indicator.Angle / 100));
            int offset = (int)(((long)sine * IndicatorAmplitude + 0x800) >> 12);
            SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(indicator.Sprite!, (short)indicator.X, (short)(indicator.Y - offset / 4096), indicator.SubscreenOffset);
        }
        if (indicator.State == 1)
        {
            switch (indicator.TapPhase)
            {
                case 0:
                    indicator.TapCounter++;
                    if (indicator.TapCounter > 3)
                    {
                        indicator.TapCounter = 0;
                        indicator.TapPhase++;
                    }
                    break;
                case 1:
                    SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(indicator.Sprite!, (short)indicator.X, (short)(indicator.Y + IndicatorTapDepth), indicator.SubscreenOffset);
                    indicator.Tapped = 1;
                    indicator.TapPhase++;
                    break;
                case 2:
                    indicator.TapCounter++;
                    if (indicator.TapCounter > 2)
                    {
                        SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(indicator.Sprite!, (short)indicator.X, (short)(indicator.Y + IndicatorReleaseDepth), indicator.SubscreenOffset);
                        indicator.TapCounter = 0;
                        indicator.TapPhase++;
                    }
                    break;
                case 3:
                    indicator.TapCounter++;
                    if (indicator.TapCounter > 2)
                    {
                        TouchIndicator_Hide(indicator);
                        indicator.TapCounter = 0;
                        indicator.TapPhase++;
                    }
                    break;
            }
        }
        SpriteSystem.ManagedSprite_TickFrame(indicator.Sprite!);
    }

    // OFFSET 0x0226E13C
    internal static void TouchIndicator_Reset(TouchIndicator indicator)
    {
        indicator.Angle = 0;
        indicator.State = 0;
        indicator.TapPhase = 0;
        indicator.TapCounter = 0;
    }
}
