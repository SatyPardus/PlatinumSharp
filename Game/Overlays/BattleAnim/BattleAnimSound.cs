namespace PlatinumSharp.Game;

public sealed class BattleAnimSoundContext
{
    public byte Type;
    public byte Unused01;
    public byte Unused02;
    public byte ApplyInterval;
    public byte TickCount;
    public int StartPan;
    public int EndPan;
    public int PanStep;
    public int CurPan;
    public byte RepeatCount;
    public ushort EffectID;
    public BattleAnimSystem? System;
}

public static partial class BattleAnimSystems
{
    private const int SoundTaskNone = 0;
    private const int SoundTaskPan = 1;
    private const int SoundTaskPan2 = 2;
    private const int SoundTaskPan3 = 3;
    private const int SoundTaskRepeat = 4;
    private const int SoundTaskDelay = 5;
    private const ushort SoundTrackAll = 0xFFFF;

    private static readonly Func<BattleAnimSoundContext, int>[] SoundFuncs =
    {
        BattleAnimSoundFunc_None,
        BattleAnimSoundFunc_Pan,
        BattleAnimSoundFunc_Pan2,
        BattleAnimSoundFunc_Pan3,
        BattleAnimSoundFunc_Repeat,
        BattleAnimSoundFunc_Delay,
    };

    // OFFSET 0x02220314
    private static BattleAnimSoundContext BattleAnimSystem_CreateSoundContext(BattleAnimSystem system)
    {
        BattleAnimSoundContext ctx = new();
        system.ActiveSoundTasks++;
        return ctx;
    }

    // OFFSET 0x02220344
    private static void BattleAnimSystem_StartSoundTaskInternal(BattleAnimSystem system, BattleAnimSoundContext ctx)
    {
        ctx.System = system;
        SysTask.SysTask_Start(BattleAnimSound_Task, ctx, AnimTaskPriority);
    }

    // OFFSET 0x0222035C
    private static void BattleAnimSound_Task(SysTask task, object? param)
    {
        BattleAnimSoundContext ctx = (BattleAnimSoundContext)param!;
        if (SoundFuncs[ctx.Type](ctx) == 0)
        {
            BattleAnimSystem system = ctx.System!;
            if (system.ActiveSoundTasks != 0)
            {
                system.ActiveSoundTasks--;
            }
            SysTask.SysTask_Done(task);
        }
    }

    // OFFSET 0x0222039C
    private static int BattleAnimSoundFunc_None(BattleAnimSoundContext ctx)
    {
        return 0;
    }

    // OFFSET 0x022203A0
    private static int BattleAnimSoundFunc_Pan(BattleAnimSoundContext ctx)
    {
        byte tick = ctx.TickCount;
        ctx.TickCount++;
        if (tick < ctx.ApplyInterval)
        {
            return 1;
        }
        ctx.TickCount = 0;
        ctx.CurPan += ctx.PanStep;
        int result = 1;
        if (ctx.PanStep == 0)
        {
            result = 0;
        }
        else if (ctx.StartPan < ctx.EndPan)
        {
            if (ctx.CurPan >= ctx.EndPan)
            {
                result = 0;
            }
        }
        else if (ctx.CurPan <= ctx.EndPan)
        {
            result = 0;
        }
        SoundPlayback.Sound_PanAllEffects(ctx.CurPan);
        if (SoundPlayback.Sound_IsEffectPlaying(ctx.EffectID) == 0)
        {
            result = 0;
        }
        return result;
    }

    // OFFSET 0x022203FC
    private static int BattleAnimSoundFunc_Pan2(BattleAnimSoundContext ctx)
    {
        return BattleAnimSoundFunc_Pan(ctx);
    }

    // OFFSET 0x02220404
    private static int BattleAnimSoundFunc_Pan3(BattleAnimSoundContext ctx)
    {
        return BattleAnimSoundFunc_Pan(ctx);
    }

    // OFFSET 0x0222040C
    private static int BattleAnimSoundFunc_Repeat(BattleAnimSoundContext ctx)
    {
        byte tick = ctx.TickCount;
        ctx.TickCount++;
        if (tick < ctx.ApplyInterval)
        {
            return 1;
        }
        ctx.TickCount = 0;
        int result = 1;
        ctx.RepeatCount--;
        SoundPlayback.Sound_PlayEffect(ctx.EffectID);
        SoundPlayback.Sound_PanEffect(ctx.EffectID, SoundTrackAll, ctx.CurPan);
        if (ctx.RepeatCount == 0)
        {
            result = 0;
        }
        return result;
    }

    // OFFSET 0x0222044C
    private static int BattleAnimSoundFunc_Delay(BattleAnimSoundContext ctx)
    {
        byte delay = ctx.ApplyInterval;
        ctx.ApplyInterval = (byte)(delay - 1);
        if (delay == 0)
        {
            SoundPlayback.Sound_PlayEffect(ctx.EffectID);
            SoundPlayback.Sound_PanEffect(ctx.EffectID, SoundTrackAll, ctx.CurPan);
            return 0;
        }
        return 1;
    }

    // OFFSET 0x02222950
    private static void BattleAnimScriptCmd_PlaySoundEffect(BattleAnimSystem system)
    {
        Next(system);
        ushort effectID = (ushort)ReadWord(system);
        Next(system);
        SoundPlayback.Sound_PlayEffect(effectID);
    }

    // OFFSET 0x02222968
    private static void BattleAnimScriptCmd_StopSoundEffect(BattleAnimSystem system)
    {
        Next(system);
        ushort effectID = (ushort)ReadWord(system);
        Next(system);
        SoundPlayback.Sound_StopEffect(effectID, 0);
    }

    // OFFSET 0x02222984
    private static void BattleAnimScriptCmd_PlayPannedSoundEffect(BattleAnimSystem system)
    {
        Next(system);
        ushort effectID = (ushort)ReadWord(system);
        Next(system);
        int pan = (int)ReadWord(system);
        Next(system);
        pan = BattleAnimSound_CorrectPanDirection(system, (sbyte)pan);
        SoundPlayback.Sound_PlayEffect(effectID);
        SoundPlayback.Sound_PanEffect(effectID, SoundTrackAll, pan);
    }

    // OFFSET 0x022229BC
    private static void BattleAnimScriptCmd_PanSoundEffects(BattleAnimSystem system)
    {
        Next(system);
        int pan = (int)ReadWord(system);
        Next(system);
        SoundPlayback.Sound_PanAllEffects(BattleAnimSound_CorrectPanDirection(system, (sbyte)pan));
    }

    // OFFSET 0x022229D8
    private static void BattleAnimScriptCmd_PlayMovingSoundEffectAtkDef(BattleAnimSystem system)
    {
        BattleAnimSoundContext ctx = BattleAnimSystem_CreateSoundContext(system);
        ctx.Type = SoundTaskPan;
        Next(system);
        ctx.EffectID = (ushort)ReadWord(system);
        Next(system);
        ctx.StartPan = (int)ReadWord(system);
        Next(system);
        ctx.EndPan = (int)ReadWord(system);
        Next(system);
        ctx.PanStep = (int)ReadWord(system);
        Next(system);
        ctx.ApplyInterval = (byte)ReadWord(system);
        Next(system);
        ctx.StartPan = BattleAnimSound_CorrectPanDirection(system, (sbyte)ctx.StartPan);
        ctx.EndPan = BattleAnimSound_CorrectPanDirection(system, (sbyte)ctx.EndPan);
        ctx.PanStep = BattleAnimSound_CorrectStepDirection((sbyte)ctx.StartPan, (sbyte)ctx.EndPan, (sbyte)ctx.PanStep);
        SoundPlayback.Sound_PlayEffect(ctx.EffectID);
        SoundPlayback.Sound_PanEffect(ctx.EffectID, SoundTrackAll, ctx.StartPan);
        BattleAnimSystem_StartSoundTaskInternal(system, ctx);
    }

    // OFFSET 0x02222A78
    private static void BattleAnimScriptCmd_PlayMovingSoundEffectNoCorrection(BattleAnimSystem system)
    {
        BattleAnimSoundContext ctx = BattleAnimSystem_CreateSoundContext(system);
        ctx.Type = SoundTaskPan2;
        Next(system);
        ctx.EffectID = (ushort)ReadWord(system);
        Next(system);
        ctx.StartPan = (sbyte)ReadWord(system);
        Next(system);
        ctx.EndPan = (sbyte)ReadWord(system);
        Next(system);
        ctx.PanStep = (sbyte)ReadWord(system);
        Next(system);
        ctx.ApplyInterval = (byte)ReadWord(system);
        Next(system);
        SoundPlayback.Sound_PlayEffect(ctx.EffectID);
        SoundPlayback.Sound_PanEffect(ctx.EffectID, SoundTrackAll, ctx.StartPan);
        BattleAnimSystem_StartSoundTaskInternal(system, ctx);
    }

    // OFFSET 0x02222AF0
    private static void BattleAnimScriptCmd_PlayMovingSoundEffectAtkDef2(BattleAnimSystem system)
    {
        BattleAnimSoundContext ctx = BattleAnimSystem_CreateSoundContext(system);
        ctx.Type = SoundTaskPan;
        Next(system);
        ctx.EffectID = (ushort)ReadWord(system);
        Next(system);
        ctx.StartPan = (sbyte)ReadWord(system);
        Next(system);
        ctx.EndPan = (sbyte)ReadWord(system);
        Next(system);
        ctx.PanStep = (sbyte)ReadWord(system);
        Next(system);
        ctx.ApplyInterval = (byte)ReadWord(system);
        Next(system);
        ctx.StartPan = BattleAnimSound_CorrectPanDirection(system, (sbyte)ctx.StartPan);
        ctx.EndPan = BattleAnimSound_CorrectPanDirection(system, (sbyte)ctx.EndPan);
        ctx.PanStep = BattleAnimSound_CorrectPanDirection(system, (sbyte)ctx.PanStep);
        SoundPlayback.Sound_PlayEffect(ctx.EffectID);
        SoundPlayback.Sound_PanEffect(ctx.EffectID, SoundTrackAll, ctx.StartPan);
        BattleAnimSystem_StartSoundTaskInternal(system, ctx);
    }

    // OFFSET 0x02222B94
    private static void BattleAnimScriptCmd_PlayLoopedSoundEffect(BattleAnimSystem system)
    {
        BattleAnimSoundContext ctx = BattleAnimSystem_CreateSoundContext(system);
        ctx.Type = SoundTaskRepeat;
        Next(system);
        ctx.EffectID = (ushort)ReadWord(system);
        Next(system);
        ctx.CurPan = (sbyte)ReadWord(system);
        Next(system);
        ctx.ApplyInterval = (byte)ReadWord(system);
        Next(system);
        ctx.RepeatCount = (byte)ReadWord(system);
        Next(system);
        ctx.TickCount = ctx.ApplyInterval;
        ctx.CurPan = BattleAnimSound_CorrectPanDirection(system, (sbyte)ctx.CurPan);
        BattleAnimSystem_StartSoundTaskInternal(system, ctx);
    }

    // OFFSET 0x02222BF8
    private static void BattleAnimScriptCmd_PlayDelayedSoundEffect(BattleAnimSystem system)
    {
        BattleAnimSoundContext ctx = BattleAnimSystem_CreateSoundContext(system);
        ctx.Type = SoundTaskDelay;
        Next(system);
        ctx.EffectID = (ushort)ReadWord(system);
        Next(system);
        ctx.CurPan = (sbyte)ReadWord(system);
        Next(system);
        ctx.ApplyInterval = (byte)ReadWord(system);
        Next(system);
        ctx.CurPan = BattleAnimSound_CorrectPanDirection(system, (sbyte)ctx.CurPan);
        BattleAnimSystem_StartSoundTaskInternal(system, ctx);
    }

    // OFFSET 0x02222C50
    private static void BattleAnimScriptCmd_Nop3(BattleAnimSystem system)
    {
    }

    // OFFSET 0x02222C54
    private static void BattleAnimScriptCmd_WaitForSoundEffects(BattleAnimSystem system)
    {
        if (system.ActiveSoundTasks != 0)
        {
            system.ScriptDelay = 1;
            system.SoundEffectWaitTimer = 0;
            return;
        }
        if (SoundPlayback.Sound_IsAnyEffectPlaying())
        {
            system.SoundEffectWaitTimer++;
            if (system.SoundEffectWaitTimer > MaxSfxWaitFrames)
            {
                system.SoundEffectWaitTimer = 0;
            }
            else
            {
                system.ScriptDelay = 1;
            }
        }
        else
        {
            system.ScriptDelay = 0;
            system.SoundEffectWaitTimer = 0;
            Next(system);
        }
    }

    // OFFSET 0x0222317C
    public static sbyte BattleAnimSound_CorrectPanDirection(BattleAnimSystem system, sbyte pan)
    {
        int adjustedPan = pan;
        int attacker = BattleAnimSystem_GetAttacker(system);
        int defender = BattleAnimSystem_GetDefender(system);
        BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, attacker);
        BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, defender);
        int attackerSide = BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, attacker);
        if (attackerSide == BattlerSidePlayer && BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, defender) == BattlerSideEnemy)
        {
        }
        else if (BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, attacker) == BattlerSideEnemy && BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, defender) == BattlerSidePlayer)
        {
            adjustedPan *= -1;
        }
        else if (BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, attacker) == BattlerSidePlayer && BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, defender) == BattlerSidePlayer)
        {
            if (adjustedPan > 0)
            {
                adjustedPan *= -1;
            }
        }
        else if (BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, attacker) == BattlerSideEnemy && BattleAnimUtil.BattleAnimUtil_GetBattlerSide(system, defender) == BattlerSideEnemy)
        {
            if (adjustedPan < 0)
            {
                adjustedPan *= -1;
            }
        }
        if (BattleAnimSystem_IsContest(system) == 1)
        {
            adjustedPan *= -1;
        }
        return (sbyte)adjustedPan;
    }

    // OFFSET 0x02223234
    public static sbyte BattleAnimSound_CorrectStepDirection(sbyte start, sbyte end, sbyte step)
    {
        if (start < end)
        {
            return (sbyte)Math.Abs((int)step);
        }
        if (start > end)
        {
            return (sbyte)-Math.Abs((int)step);
        }
        return 0;
    }
}
