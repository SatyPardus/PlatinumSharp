namespace PlatinumSharp.Game;

public delegate bool ScrCmdFunc(ScriptContext ctx);

public delegate bool ShouldResumeScriptFunc(ScriptContext ctx);

public sealed class ScriptContext
{
    public const int StackPointerOffset = 0x00;
    public const int StateOffset = 0x01;
    public const int ComparisonResultOffset = 0x02;
    public const int ShouldResumeOffset = 0x04;
    public const int ScriptPtrOffset = 0x08;
    public const int StackOffset = 0x0C;
    public const int CmdTableOffset = 0x5C;
    public const int CmdTableSizeOffset = 0x60;
    public const int DataOffset = 0x64;
    public const int TaskOffset = 0x74;
    public const int LoaderOffset = 0x78;
    public const int ScriptsOffset = 0x7C;
    public const int FieldSystemOffset = 0x80;
    public const int Size = 0x84;

    public const int StackDepth = 20;
    public const int DataCount = 4;
    public const int NullPtr = -1;

    public const int StateStopped = 0;
    public const int StateRunning = 1;
    public const int StateWaiting = 2;

    public byte StackPointer;
    public byte State;
    public byte ComparisonResult;
    public ShouldResumeScriptFunc? ShouldResume;
    public int ScriptPtr = NullPtr;
    public readonly int[] Stack = new int[StackDepth];
    public ScrCmdFunc[]? CmdTable;
    public uint CmdTableSize;
    public readonly uint[] Data = new uint[DataCount];
    public FieldTask? Task;
    public MessageLoader? Loader;
    public byte[]? Scripts;
    public int ScriptFile = ScriptPlayback.NoFile;
    public FieldSystem FieldSystem = null!;

    // OFFSET 0x0203E724
    public static void ScriptContext_Init(ScriptContext ctx, ScrCmdFunc[] cmdTable, uint cmdTableSize)
    {
        ctx.State = StateStopped;
        ctx.ScriptPtr = NullPtr;
        ctx.StackPointer = 0;
        ctx.ShouldResume = null;
        ctx.CmdTable = cmdTable;
        ctx.CmdTableSize = cmdTableSize;
        for (int i = 0; i < DataCount; i++)
        {
            ctx.Data[i] = 0;
        }
        for (int i = 0; i < StackDepth; i++)
        {
            ctx.Stack[i] = NullPtr;
        }
        ctx.Task = null;
    }

    // OFFSET 0x0203E758
    public static bool ScriptContext_Start(ScriptContext ctx, int ptr)
    {
        ctx.ScriptPtr = ptr;
        ctx.State = StateRunning;
        return true;
    }

    // OFFSET 0x0203E764
    public static void ScriptContext_Pause(ScriptContext ctx, ShouldResumeScriptFunc? shouldResume)
    {
        ctx.State = StateWaiting;
        ctx.ShouldResume = shouldResume;
    }

    // OFFSET 0x0203E76C
    public static void ScriptContext_Stop(ScriptContext ctx)
    {
        ctx.State = StateStopped;
        ctx.ScriptPtr = NullPtr;
    }

    // OFFSET 0x0203E774
    public static void ScriptContext_SetTask(ScriptContext ctx, FieldTask? task)
    {
        ctx.Task = task;
    }

    // OFFSET 0x0203E778
    public static bool ScriptContext_Run(ScriptContext ctx)
    {
        if (ctx.State == StateStopped)
        {
            return false;
        }
        switch (ctx.State)
        {
            case StateStopped:
                return false;
            case StateWaiting:
                if (ctx.ShouldResume != null)
                {
                    if (ctx.ShouldResume(ctx))
                    {
                        ctx.State = StateRunning;
                    }
                    return true;
                }
                ctx.State = StateRunning;
                goto case StateRunning;
            case StateRunning:
                while (true)
                {
                    if (ctx.ScriptPtr == NullPtr)
                    {
                        ctx.State = StateStopped;
                        return false;
                    }
                    if (ScriptPlayback.Hold(ctx))
                    {
                        break;
                    }
                    ushort command = ScriptContext_ReadHalfWord(ctx);
                    if (command >= ctx.CmdTableSize)
                    {
                        ErrorHandling.GF_AssertFail();
                        ctx.State = StateStopped;
                        return false;
                    }
                    if (ctx.CmdTable![command](ctx))
                    {
                        break;
                    }
                }
                break;
        }
        return true;
    }

    // OFFSET 0x0203E7E4
    private static bool ScriptContext_Push(ScriptContext ctx, int ptr)
    {
        if (ctx.StackPointer + 1 >= StackDepth)
        {
            return true;
        }
        ctx.Stack[ctx.StackPointer] = ptr;
        ctx.StackPointer++;
        return false;
    }

    // OFFSET 0x0203E800
    private static int ScriptContext_Pop(ScriptContext ctx)
    {
        if (ctx.StackPointer == 0)
        {
            return NullPtr;
        }
        ctx.StackPointer--;
        return ctx.Stack[ctx.StackPointer];
    }

    // OFFSET 0x0203E818
    public static void ScriptContext_Jump(ScriptContext ctx, int ptr)
    {
        ctx.ScriptPtr = ptr;
    }

    // OFFSET 0x0203E81C
    public static void ScriptContext_Call(ScriptContext ctx, int ptr)
    {
        ScriptContext_Push(ctx, ctx.ScriptPtr);
        ctx.ScriptPtr = ptr;
    }

    // OFFSET 0x0203E82C
    public static void ScriptContext_Return(ScriptContext ctx)
    {
        ctx.ScriptPtr = ScriptContext_Pop(ctx);
    }

    // OFFSET 0x0203E838
    public static ushort ScriptContext_ReadHalfWord(ScriptContext ctx)
    {
        byte low = ctx.Scripts![ctx.ScriptPtr++];
        byte high = ctx.Scripts[ctx.ScriptPtr++];
        return (ushort)(low + (high << 8));
    }

    // OFFSET 0x0203E850
    public static uint ScriptContext_ReadWord(ScriptContext ctx)
    {
        uint b0 = ctx.Scripts![ctx.ScriptPtr++];
        uint b1 = ctx.Scripts[ctx.ScriptPtr++];
        uint b2 = ctx.Scripts[ctx.ScriptPtr++];
        uint b3 = ctx.Scripts[ctx.ScriptPtr++];
        return (((((b3 << 8) + b2) << 8) + b1) << 8) + b0;
    }

    public static byte ScriptContext_ReadByte(ScriptContext ctx)
    {
        return ctx.Scripts![ctx.ScriptPtr++];
    }
}
