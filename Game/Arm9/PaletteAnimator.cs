using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void PaletteAnimatorApplyFunc(PaletteAnimator animator, ushort[] colors);

public sealed class PaletteAnimator
{
    public const int ModeRestart = 0;
    public const int ModePause = 1;
    public const int ModeFinish = 2;

    private const int PaletteColors = 16;
    private const int PaletteBytes = 0x20;
    private const int AnimatedColorMask = 0x1E << 10;
    private const int StateRestart = 0;
    private const int StateRunning = 1;
    private const int StatePaused = 2;
    private const int StateFinish = 3;
    private const int CycleLength = 0x20;
    private const int HighlightEndFrame = 0x18;
    private const int MainBgLayer = 0;
    private const int SubBgLayer = 4;
    private const int MainBgBuffer = 0;
    private const int SubBgBuffer = 1;

    public SysTask? Task;
    public PaletteAnimatorApplyFunc? Apply;
    public PaletteData? PaletteData;
    public readonly ushort[] Original = new ushort[PaletteColors];
    public readonly ushort[] Working = new ushort[PaletteColors];
    public byte IsSubScreen;
    public byte PaletteIndex;
    public byte State;
    public byte Counter;

    // OFFSET 0x0201567C
    public static PaletteAnimator PaletteAnimator_New(PaletteData? paletteData, ushort isSubScreen, ushort paletteIndex, int heapID)
    {
        SysTask task = SysTaskExtensions.SysTask_StartAndAllocateParam<PaletteAnimator>(PaletteAnimator_Task, 0x50, 0, heapID)!;
        PaletteAnimator animator = (PaletteAnimator)SysTaskManager.SysTask_GetParam(task)!;
        ushort[] source;
        if (paletteData != null)
        {
            if (isSubScreen == 0)
            {
                source = PaletteData.PaletteData_GetUnfadedBuffer(paletteData, MainBgBuffer)!;
                animator.Apply = PaletteAnimator_ApplyToMainBuffer;
            }
            else
            {
                source = PaletteData.PaletteData_GetUnfadedBuffer(paletteData, SubBgBuffer)!;
                animator.Apply = PaletteAnimator_ApplyToSubBuffer;
            }
        }
        else
        {
            if (isSubScreen == 0)
            {
                source = HardwarePalette.GetHardwareMainBgPaletteAddress();
                animator.Apply = PaletteAnimator_ApplyToMainHardware;
            }
            else
            {
                source = HardwarePalette.GetHardwareSubBgPaletteAddress();
                animator.Apply = PaletteAnimator_ApplyToSubHardware;
            }
        }
        Array.Copy(source, paletteIndex * PaletteColors, animator.Original, 0, PaletteColors);
        Array.Copy(source, paletteIndex * PaletteColors, animator.Working, 0, PaletteColors);
        animator.Task = task;
        animator.PaletteData = paletteData;
        animator.IsSubScreen = (byte)isSubScreen;
        animator.PaletteIndex = (byte)paletteIndex;
        animator.State = StateRunning;
        animator.Counter = 0;
        return animator;
    }

    // OFFSET 0x02015738
    public static void PaletteAnimator_SetMode(PaletteAnimator animator, byte mode)
    {
        switch (mode)
        {
            case ModeRestart:
                animator.State = StateRestart;
                break;
            case ModePause:
                animator.State = StatePaused;
                break;
            case ModeFinish:
                animator.State = StateFinish;
                break;
        }
    }

    // OFFSET 0x02015760
    public static void PaletteAnimator_Free(PaletteAnimator animator)
    {
        SysTaskExtensions.SysTask_FinishAndFreeParam(animator.Task!);
    }

    // OFFSET 0x0201576C
    private static void PaletteAnimator_Task(SysTask task, object? param)
    {
        PaletteAnimator animator = (PaletteAnimator)param!;
        switch (animator.State)
        {
            case StateRestart:
                animator.Counter = 0;
                animator.State = StateRunning;
                break;
            case StateRunning:
                if (PaletteAnimator_Step(animator))
                {
                    animator.Apply!(animator, animator.Working);
                }
                animator.Counter++;
                if (animator.Counter == CycleLength)
                {
                    animator.Counter = 0;
                }
                break;
            case StatePaused:
                break;
            case StateFinish:
                animator.Apply!(animator, animator.Original);
                SysTaskExtensions.SysTask_FinishAndFreeParam(task);
                break;
        }
    }

    // OFFSET 0x020157E4
    private static bool PaletteAnimator_Step(PaletteAnimator animator)
    {
        if (animator.Counter == 0)
        {
            for (int i = 0; i < PaletteColors; i++)
            {
                if (((1 << i) & AnimatedColorMask) != 0)
                {
                    animator.Working[i] = animator.Original[i];
                }
            }
            return true;
        }
        if (animator.Counter == HighlightEndFrame)
        {
            for (int i = 0; i < PaletteColors; i++)
            {
                if (((1 << i) & AnimatedColorMask) != 0)
                {
                    animator.Working[i] = animator.Original[PaletteColors - 1];
                }
            }
            return true;
        }
        return false;
    }

    // OFFSET 0x02015840
    private static void PaletteAnimator_ApplyToMainHardware(PaletteAnimator animator, ushort[] colors)
    {
        Bg.Bg_LoadPalette(MainBgLayer, colors, 0, PaletteBytes, (ushort)(animator.PaletteIndex * PaletteBytes));
    }

    // OFFSET 0x02015858
    private static void PaletteAnimator_ApplyToSubHardware(PaletteAnimator animator, ushort[] colors)
    {
        Bg.Bg_LoadPalette(SubBgLayer, colors, 0, PaletteBytes, (ushort)(animator.PaletteIndex * PaletteBytes));
    }

    // OFFSET 0x02015870
    private static void PaletteAnimator_ApplyToMainBuffer(PaletteAnimator animator, ushort[] colors)
    {
        PaletteData.PaletteData_LoadBuffer(animator.PaletteData!, colors, 0, MainBgBuffer, (ushort)(animator.PaletteIndex * PaletteColors), PaletteBytes);
    }

    // OFFSET 0x0201588C
    private static void PaletteAnimator_ApplyToSubBuffer(PaletteAnimator animator, ushort[] colors)
    {
        PaletteData.PaletteData_LoadBuffer(animator.PaletteData!, colors, 0, SubBgBuffer, (ushort)(animator.PaletteIndex * PaletteColors), PaletteBytes);
    }
}
