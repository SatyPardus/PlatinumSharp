namespace PlatinumSharp.Game;

public sealed class FieldPoisonEffect
{
    public const int ScreenLines = 0xC0;
    public const int Size = 0x328;

    private const int StateStart = 0;
    private const int StateShakeIn = 1;
    private const int StateShakeOut = 2;
    private const int StateEnd = 3;
    private const int ShakeFrames = 3;
    private const int StripeHeight = 10;
    private const uint TaskPriority = 0x400;
    private const int SeqSEPoison = 0x610;

    public int Active;
    public int Timer;
    public int State;
    public readonly ushort[] Buffer1 = new ushort[ScreenLines];
    public readonly ushort[] Buffer2 = new ushort[ScreenLines];
    public BufferManager? BufferManager;
    public HBlankSystem? HBlankSystem;
    public HBlankTask? HBlankTask;
    public int BuffersReady;
    public SysTask? MainTask;
    public SysTask? VBlankResetTask;
    public SysTask? VBlankSwapTask;

    // OFFSET 0x021EF4BC
    public static FieldPoisonEffect FieldPoisonEffect_New(int heapID, HBlankSystem? hBlankSystem)
    {
        FieldPoisonEffect effect = new();
        effect.Active = 0;
        effect.BufferManager = BufferManager.BufferManager_New(heapID, effect.Buffer1, effect.Buffer2);
        effect.HBlankSystem = hBlankSystem;
        return effect;
    }

    // OFFSET 0x021EF4F8
    public static void FieldPoisonEffect_Free(FieldPoisonEffect effect)
    {
        if (effect.Active == 1)
        {
            FieldPoisonEffect_Stop(effect);
        }
        BufferManager.BufferManager_Delete(effect.BufferManager);
    }

    // OFFSET 0x021EF518
    public static void Field_DoPoisonEffect(FieldPoisonEffect effect)
    {
        if (effect.Active != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        effect.HBlankTask = HBlankSystem.HBlankSystem_StartTask(effect.HBlankSystem!, FieldPoisonEffect_HBlankCallback, effect);
        effect.MainTask = SysTask.SysTask_Start(FieldPoisonEffect_MainTask, effect, TaskPriority);
        effect.VBlankResetTask = SysTask.SysTask_ExecuteOnVBlank(FieldPoisonEffect_VBlankResetTask, effect, TaskPriority);
        effect.VBlankSwapTask = SysTask.SysTask_ExecuteOnVBlank(FieldPoisonEffect_VBlankSwapTask, effect, TaskPriority);
        Array.Clear(effect.Buffer1);
        Array.Clear(effect.Buffer2);
        effect.Active = 1;
        effect.State = 0;
        SoundPlayback.Sound_PlayEffect(SeqSEPoison);
    }

    // OFFSET 0x021EF5A8
    private static void FieldPoisonEffect_MainTask(SysTask task, object? param)
    {
        FieldPoisonEffect effect = (FieldPoisonEffect)param!;
        switch (effect.State)
        {
            case StateStart:
                effect.Timer = ShakeFrames;
                effect.State++;
                break;
            case StateShakeIn:
                effect.Timer--;
                FieldPoisonEffect_FillOffsets((ushort[])BufferManager.BufferManager_GetWriteBuffer(effect.BufferManager)!, ShakeFrames - effect.Timer, ShakeFrames);
                if (effect.Timer <= 0)
                {
                    effect.Timer = ShakeFrames;
                    effect.State++;
                }
                break;
            case StateShakeOut:
                effect.Timer--;
                FieldPoisonEffect_FillOffsets((ushort[])BufferManager.BufferManager_GetWriteBuffer(effect.BufferManager)!, effect.Timer, ShakeFrames);
                if (effect.Timer <= 0)
                {
                    effect.State++;
                }
                break;
            case StateEnd:
                FieldPoisonEffect_Stop(effect);
                Video.SetBgOffset(0, 0);
                break;
        }
    }

    // OFFSET 0x021EF634
    private static void FieldPoisonEffect_ApplyLineOffset(FieldPoisonEffect effect)
    {
        int line = Video.VCount;
        ushort[] offsets = (ushort[])BufferManager.BufferManager_GetReadBuffer(effect.BufferManager)!;
        if (line < ScreenLines)
        {
            int nextLine = line + 1;
            if (nextLine >= ScreenLines)
            {
                nextLine -= ScreenLines;
            }
            if (Video.InHBlank)
            {
                Video.SetBgOffset(0, offsets[nextLine]);
            }
        }
    }

    // OFFSET 0x021EF66C
    private static void FieldPoisonEffect_FillOffsets(ushort[] offsets, int step, int steps)
    {
        int stripe = 1;
        int amount = step * 3 / steps;
        ushort negative = (ushort)-amount;
        ushort positive = (ushort)amount;
        for (int i = 0; i < ScreenLines; i++)
        {
            if (i % StripeHeight == 0)
            {
                stripe ^= 1;
            }
            if (stripe != 0)
            {
                offsets[i] = positive;
            }
            else
            {
                offsets[i] = negative;
            }
        }
    }

    // OFFSET 0x021EF6B0
    private static void FieldPoisonEffect_VBlankResetTask(SysTask task, object? param)
    {
        FieldPoisonEffect effect = (FieldPoisonEffect)param!;
        effect.BuffersReady = 0;
        Video.SetBgOffset(0, 0);
    }

    // OFFSET 0x021EF6C0
    private static void FieldPoisonEffect_HBlankCallback(HBlankTask task, object? param)
    {
        FieldPoisonEffect_HBlank((FieldPoisonEffect)param!);
    }

    // OFFSET 0x021EF6CC
    private static void FieldPoisonEffect_HBlank(FieldPoisonEffect effect)
    {
        if (effect.Active == 1 && effect.BuffersReady == 1)
        {
            FieldPoisonEffect_ApplyLineOffset(effect);
        }
    }

    // OFFSET 0x021EF6E4
    private static void FieldPoisonEffect_VBlankSwapTask(SysTask task, object? param)
    {
        FieldPoisonEffect_SwapBuffers((FieldPoisonEffect)param!);
    }

    // OFFSET 0x021EF6F0
    private static void FieldPoisonEffect_SwapBuffers(FieldPoisonEffect effect)
    {
        if (effect.Active == 1)
        {
            BufferManager.BufferManager_SwapBuffers(effect.BufferManager);
            effect.BuffersReady = 1;
        }
    }

    // OFFSET 0x021EF710
    private static void FieldPoisonEffect_Stop(FieldPoisonEffect effect)
    {
        HBlankSystem.HBlankTask_Delete(effect.HBlankTask!);
        effect.HBlankTask = null;
        SysTask.SysTask_Done(effect.MainTask!);
        effect.MainTask = null;
        SysTask.SysTask_Done(effect.VBlankResetTask!);
        effect.VBlankResetTask = null;
        SysTask.SysTask_Done(effect.VBlankSwapTask!);
        effect.VBlankSwapTask = null;
        effect.Active = 0;
    }
}
