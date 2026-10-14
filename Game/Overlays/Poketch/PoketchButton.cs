namespace PlatinumSharp.Game;

public delegate void PoketchButtonCallback(uint buttonIdx, uint buttonState, uint touchState, object? data);

public sealed class PoketchButton
{
    public byte State;
    public byte Touched;
    public byte PrevTouched;
    public ushort Timer;
    public ushort DoubleTapTime;
    public readonly ushort[] Timers = new ushort[2];
    public ushort RepeatTime;
}

public sealed class PoketchButtonManager
{
    public TouchScreenHitTable[] HitTable = Array.Empty<TouchScreenHitTable>();
    public PoketchButtonCallback? Callback;
    public int HeapID;
    public uint NumButtons;
    public object? CallbackData;
    public PoketchButton[] Buttons = Array.Empty<PoketchButton>();
}

public static class PoketchButtons
{
    public const uint ButtonStateNone = 0;
    public const uint ButtonStatePressed = 1;
    public const uint ButtonStateReleased = 2;
    public const uint ButtonStateTapped = 3;
    public const uint ButtonStateDoubleTapped = 4;
    public const uint ButtonStateTimer0 = 5;
    public const uint ButtonStateTimer1 = 6;
    public const uint ButtonStateRepeat = 7;
    public const uint TouchStateUnchanged = 2;

    private const byte StateIdle = 0;
    private const byte StatePressed = 1;
    private const byte StateWaitDoubleTap = 2;
    private const byte StateHeldAfterDoubleTap = 3;
    private const byte StateWaitRelease = 4;

    private delegate uint ButtonStateFunc(PoketchButton button, bool touchedAny, bool tapped);

    private static readonly ButtonStateFunc[] sStateFuncs =
    {
        PoketchButton_StateIdle,
        PoketchButton_StatePressed,
        PoketchButton_StateWaitDoubleTap,
        PoketchButton_StateHeldAfterDoubleTap,
        PoketchButton_StateWaitRelease,
    };

    public static TouchScreenHitTable[] HitTableFromRom(int overlay, uint address, int count)
    {
        byte[] raw = RomData.Overlay(overlay, address, count * 4);
        TouchScreenHitTable[] table = new TouchScreenHitTable[count];
        for (int i = 0; i < count; i++)
        {
            table[i] = new TouchScreenHitTable { Top = raw[i * 4], Bottom = raw[i * 4 + 1], Left = raw[i * 4 + 2], Right = raw[i * 4 + 3] };
        }
        return table;
    }

    // OFFSET 0x02255ACC
    public static PoketchButtonManager? PoketchButtonManager_New(TouchScreenHitTable[] hitTable, uint numButtons, PoketchButtonCallback callback, object? callbackData, int heapID)
    {
        if (hitTable == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        PoketchButtonManager buttonManager = new();
        buttonManager.HeapID = heapID;
        buttonManager.Callback = callback;
        buttonManager.HitTable = hitTable!;
        buttonManager.NumButtons = numButtons;
        buttonManager.CallbackData = callbackData;
        buttonManager.Buttons = new PoketchButton[numButtons];
        for (int i = 0; i < numButtons; i++)
        {
            buttonManager.Buttons[i] = new PoketchButton();
            PoketchButton_Init(buttonManager.Buttons[i]);
        }
        return buttonManager;
    }

    // OFFSET 0x02255B34
    public static void PoketchButtonManager_Free(PoketchButtonManager? buttonManager)
    {
        if (buttonManager == null)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02255B50
    public static void PoketchButtonManager_Update(PoketchButtonManager buttonManager)
    {
        bool touchedAny = TouchScreen.TouchScreen_Touched();
        bool tapped;
        if (touchedAny)
        {
            tapped = TouchScreen.TouchScreen_Tapped();
            for (int i = 0; i < buttonManager.NumButtons; i++)
            {
                PoketchButton button = buttonManager.Buttons[i];
                button.PrevTouched = button.Touched;
                bool touched = button.Touched != 0 ? TouchScreen.TouchScreen_LocationHeld(buttonManager.HitTable[i]) : TouchScreen.TouchScreen_LocationPressed(buttonManager.HitTable[i]);
                button.Touched = (byte)(touched ? 1 : 0);
            }
        }
        else
        {
            tapped = false;
            for (int i = 0; i < buttonManager.NumButtons; i++)
            {
                PoketchButton button = buttonManager.Buttons[i];
                button.PrevTouched = button.Touched;
                button.Touched = 0;
            }
        }
        uint index = 0;
        for (; index < buttonManager.NumButtons; index++)
        {
            PoketchButton button = buttonManager.Buttons[index];
            uint buttonState = sStateFuncs[button.State](button, touchedAny, tapped);
            uint touchState = button.Touched;
            if (button.Touched == button.PrevTouched)
            {
                touchState = TouchStateUnchanged;
            }
            if (buttonState != ButtonStateNone || touchState != TouchStateUnchanged)
            {
                buttonManager.Callback!(index, buttonState, touchState, buttonManager.CallbackData);
                break;
            }
        }
        for (index++; index < buttonManager.NumButtons; index++)
        {
            PoketchButton button = buttonManager.Buttons[index];
            sStateFuncs[button.State](button, touchedAny, tapped);
        }
    }

    // OFFSET 0x02255C48
    public static void PoketchButtonManager_SetButtonTimer(PoketchButtonManager buttonManager, uint buttonIndex, uint timerIndex, ushort time)
    {
        buttonManager.Buttons[buttonIndex].Timers[timerIndex] = time;
    }

    // OFFSET 0x02255C5C
    public static void PoketchButtonManager_SetRepeatTime(PoketchButtonManager buttonManager, uint index, ushort repeatTime)
    {
        buttonManager.Buttons[index].RepeatTime = repeatTime;
    }

    // OFFSET 0x02255C68
    public static void PoketchButtonManager_ResetButtonState(PoketchButtonManager buttonManager, uint index)
    {
        buttonManager.Buttons[index].Timer = 0;
        buttonManager.Buttons[index].State = StateWaitRelease;
    }

    // OFFSET 0x02255C80
    private static void PoketchButton_Init(PoketchButton button)
    {
        button.State = StateIdle;
        button.Touched = 0;
        button.PrevTouched = 0;
        button.Timer = 0;
        button.DoubleTapTime = 0;
        button.Timers[0] = 0;
        button.Timers[1] = 0;
        button.RepeatTime = 0;
    }

    // OFFSET 0x02255CA0
    private static void PoketchButton_SetState(PoketchButton button, byte state)
    {
        button.State = state;
        button.Timer = 0;
    }

    // OFFSET 0x02255CA8
    private static void PoketchButton_SetStateKeepTimer(PoketchButton button, byte state)
    {
        button.State = state;
    }

    // OFFSET 0x02255CAC
    private static void PoketchButton_IncrementTimer(PoketchButton button)
    {
        if (button.Timer < 0xFFFF)
        {
            button.Timer++;
        }
    }

    // OFFSET 0x02255CC0
    private static uint PoketchButton_CheckTimers(PoketchButton button)
    {
        for (uint i = 0; i < 2; i++)
        {
            if (button.Timer == button.Timers[i])
            {
                return ButtonStateTimer0 + i;
            }
        }
        return ButtonStateNone;
    }

    // OFFSET 0x02255CDC
    private static uint PoketchButton_StateIdle(PoketchButton button, bool touchedAny, bool tapped)
    {
        if (button.Touched != 0 && tapped)
        {
            PoketchButton_SetState(button, StatePressed);
            return ButtonStatePressed;
        }
        return ButtonStateNone;
    }

    // OFFSET 0x02255CF8
    private static uint PoketchButton_StatePressed(PoketchButton button, bool touchedAny, bool tapped)
    {
        PoketchButton_IncrementTimer(button);
        if (button.Touched != 0)
        {
            uint timerState = PoketchButton_CheckTimers(button);
            if (timerState != ButtonStateNone)
            {
                return timerState;
            }
            if (button.RepeatTime != 0 && button.Timer >= button.RepeatTime)
            {
                button.Timer = 0;
                return ButtonStateRepeat;
            }
            return ButtonStateNone;
        }
        if (touchedAny)
        {
            PoketchButton_SetState(button, StateIdle);
            return ButtonStateReleased;
        }
        if (button.DoubleTapTime != 0)
        {
            PoketchButton_SetStateKeepTimer(button, StateWaitDoubleTap);
            return ButtonStateNone;
        }
        PoketchButton_SetState(button, StateIdle);
        return ButtonStateTapped;
    }

    // OFFSET 0x02255D58
    private static uint PoketchButton_StateWaitDoubleTap(PoketchButton button, bool touchedAny, bool tapped)
    {
        PoketchButton_IncrementTimer(button);
        if (button.Timer > button.DoubleTapTime)
        {
            PoketchButton_SetState(button, StateIdle);
            return ButtonStateTapped;
        }
        if (button.Touched != 0 && tapped)
        {
            PoketchButton_SetState(button, StateHeldAfterDoubleTap);
            return ButtonStateDoubleTapped;
        }
        return ButtonStateNone;
    }

    // OFFSET 0x02255D90
    private static uint PoketchButton_StateHeldAfterDoubleTap(PoketchButton button, bool touchedAny, bool tapped)
    {
        if (button.Touched != 0)
        {
            PoketchButton_IncrementTimer(button);
        }
        else
        {
            PoketchButton_SetState(button, StateIdle);
        }
        return ButtonStateNone;
    }

    // OFFSET 0x02255DA8
    private static uint PoketchButton_StateWaitRelease(PoketchButton button, bool touchedAny, bool tapped)
    {
        if (button.Touched == 0)
        {
            PoketchButton_SetState(button, StateIdle);
        }
        return ButtonStateNone;
    }
}
