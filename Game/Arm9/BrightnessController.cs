using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class BrightnessController
{
    public const int ScreenMain = 1;
    public const int ScreenSub = 2;
    public const int ScreenBoth = 3;

    private const int DisplayMain = 0;
    private const int DisplaySub = 1;

    public int PlaneMask;
    public int ScreenSelect;
    public ushort StepCount;
    public short TargetBrightness;
    public sbyte Direction;
    public short Delta;
    public short StepSize;
    public ushort StepRemainder;
    public ushort Accumulator;
    public short CurrentBrightness;
    public int Active;

    private static readonly BrightnessController sMainController = new();
    private static readonly BrightnessController sSubController = new();

    private void Clear()
    {
        PlaneMask = 0;
        ScreenSelect = 0;
        StepCount = 0;
        TargetBrightness = 0;
        Direction = 0;
        Delta = 0;
        StepSize = 0;
        StepRemainder = 0;
        Accumulator = 0;
        CurrentBrightness = 0;
        Active = 0;
    }

    // OFFSET 0x0200A9DC
    private static void BrightnessController_StepTransition(BrightnessController controller)
    {
        bool done = false;
        int next = controller.CurrentBrightness + controller.Direction * controller.StepSize;
        if (controller.TargetBrightness == next || controller.CurrentBrightness == controller.TargetBrightness)
        {
            controller.CurrentBrightness = controller.TargetBrightness;
            done = true;
        }
        else
        {
            controller.CurrentBrightness = (short)next;
            controller.Accumulator += controller.StepRemainder;
            if (controller.Accumulator >= controller.StepCount)
            {
                controller.CurrentBrightness += controller.Direction;
                if (controller.CurrentBrightness != controller.TargetBrightness)
                {
                    controller.Accumulator -= controller.StepCount;
                }
                else
                {
                    done = true;
                }
            }
        }
        if ((controller.ScreenSelect & ScreenMain) != 0)
        {
            Video.SetBlendBrightness(DisplayMain, controller.PlaneMask, controller.CurrentBrightness);
        }
        else if ((controller.ScreenSelect & ScreenSub) != 0)
        {
            Video.SetBlendBrightness(DisplaySub, controller.PlaneMask, controller.CurrentBrightness);
        }
        if (done)
        {
            controller.Active = 0;
        }
    }

    // OFFSET 0x0200AA78
    private static void BrightnessController_TransitionSetup(BrightnessController controller, int stepCount, int target, int start, int planeMask, int screenSelect)
    {
        if (controller.Active != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        controller.Active = 1;
        controller.PlaneMask = (byte)planeMask;
        controller.ScreenSelect = (byte)screenSelect;
        controller.StepCount = (ushort)stepCount;
        controller.TargetBrightness = (short)target;
        controller.CurrentBrightness = (short)start;
        controller.Delta = (short)(start - target);
        if (controller.Delta > 0)
        {
            controller.Direction = -1;
        }
        else
        {
            controller.Direction = 1;
            controller.Delta = (short)-controller.Delta;
        }
        controller.StepSize = (short)(controller.Delta / stepCount);
        controller.StepRemainder = (ushort)(controller.Delta % stepCount);
        controller.Accumulator = 0;
    }

    // OFFSET 0x0200AAE0
    public static void BrightnessController_StartTransition(int stepCount, int target, int start, int planeMask, int screenSelect)
    {
        if (stepCount == 0)
        {
            return;
        }
        if ((screenSelect & ScreenMain) != 0)
        {
            Video.SetBlendBrightness(DisplayMain, planeMask, start);
            BrightnessController_TransitionSetup(sMainController, stepCount, target, start, planeMask, ScreenMain);
        }
        if ((screenSelect & ScreenSub) != 0)
        {
            Video.SetBlendBrightness(DisplaySub, planeMask, start);
            BrightnessController_TransitionSetup(sSubController, stepCount, target, start, planeMask, ScreenSub);
        }
    }

    // OFFSET 0x0200AB4C
    public static void BrightnessController_SetScreenBrightness(int brightness, int planeMask, int screenSelect)
    {
        if ((screenSelect & ScreenMain) != 0)
        {
            Video.SetBlendBrightness(DisplayMain, planeMask, brightness);
        }
        if ((screenSelect & ScreenSub) != 0)
        {
            Video.SetBlendBrightness(DisplaySub, planeMask, brightness);
        }
        BrightnessController_ResetScreenController(screenSelect);
    }

    // OFFSET 0x0200AB84
    public static void BrightnessController_ResetAllControllers()
    {
        sMainController.Clear();
        sSubController.Clear();
        sMainController.Active = 0;
        sSubController.Active = 0;
    }

    // OFFSET 0x0200ABB0
    public static void BrightnessController_ResetScreenController(int screenSelect)
    {
        if ((screenSelect & ScreenMain) != 0)
        {
            sMainController.Clear();
            sMainController.Active = 0;
        }
        if ((screenSelect & ScreenSub) != 0)
        {
            sSubController.Clear();
            sSubController.Active = 0;
        }
    }

    // OFFSET 0x0200ABF0
    public static void BrightnessController_Update()
    {
        if (sMainController.Active != 0)
        {
            BrightnessController_StepTransition(sMainController);
        }
        if (sSubController.Active != 0)
        {
            BrightnessController_StepTransition(sSubController);
        }
    }

    // OFFSET 0x0200AC1C
    public static bool BrightnessController_IsTransitionComplete(int screenSelect)
    {
        if (screenSelect == ScreenBoth)
        {
            if (sMainController.Active == 0 && sSubController.Active == 0)
            {
                return true;
            }
        }
        else if (screenSelect == ScreenMain)
        {
            if (sMainController.Active == 0)
            {
                return true;
            }
        }
        else if (screenSelect == ScreenSub)
        {
            if (sSubController.Active == 0)
            {
                return true;
            }
        }
        return false;
    }
}
