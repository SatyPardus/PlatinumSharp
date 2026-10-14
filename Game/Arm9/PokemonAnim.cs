using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void TransformFunc(TransformData transform, PokemonAnim anim);

public delegate void PokemonAnimCommand(PokemonAnim anim);

public sealed class PokemonAnimTemplate
{
    public ushort Animation;
    public ushort StartDelay;
    public byte FlipSprite;
}

public sealed class TransformData
{
    public const int MaxVars = 8;
    public const int MemberOffsetX = 0;
    public const int MemberOffsetY = 1;
    public const int MemberScaleX = 2;
    public const int MemberScaleY = 3;
    public const int MemberRotationZ = 4;
    public const int MemberCount = 5;

    public int Active;
    public readonly int[] Vars = new int[MaxVars];
    public int TargetMember;
    public byte CalcType;
    public byte StartDelay;
    public int OriginalValue;
    public int Unused34;
    public int Unused38;
    public readonly int[] Members = new int[MemberCount];
    public TransformFunc? Func;

    public void Clear()
    {
        Active = 0;
        Array.Clear(Vars);
        TargetMember = 0;
        CalcType = 0;
        StartDelay = 0;
        OriginalValue = 0;
        Unused34 = 0;
        Unused38 = 0;
        Array.Clear(Members);
        Func = null;
    }
}

public sealed class PokemonAnim
{
    public const int MaxVars = 8;
    public const int MaxTransforms = 4;
    public const int MemberTranslateX = 0;
    public const int MemberTranslateY = 1;
    public const int MemberOffsetX = 2;
    public const int MemberOffsetY = 3;
    public const int MemberScaleX = 4;
    public const int MemberScaleY = 5;
    public const int MemberRotationZ = 6;
    public const int MemberCount = 7;

    public PokemonSprite? Sprite;
    public SysTask? Task;
    public uint[]? ScriptData;
    public int ScriptPtr;
    public int Active;
    public int AnimNum;
    public int WaitFrame;
    public int EndAnim;
    public int Completed;
    public readonly int[] Vars = new int[MaxVars];
    public int CommandCount;
    public int LoopMax;
    public int LoopCounter;
    public int LoopStart;
    public int StartDelay;
    public int OriginalX;
    public int OriginalY;
    public readonly int[] Members = new int[MemberCount];
    public readonly TransformData[] Transforms = { new(), new(), new(), new() };
    public byte FlipSprite;
    public byte WaitForTransform;
    public byte YNormalization;
    public byte FadeActive;

    public void Clear()
    {
        Sprite = null;
        Task = null;
        ScriptData = null;
        ScriptPtr = 0;
        Active = 0;
        AnimNum = 0;
        WaitFrame = 0;
        EndAnim = 0;
        Completed = 0;
        Array.Clear(Vars);
        CommandCount = 0;
        LoopMax = 0;
        LoopCounter = 0;
        LoopStart = 0;
        StartDelay = 0;
        OriginalX = 0;
        OriginalY = 0;
        Array.Clear(Members);
        for (int i = 0; i < MaxTransforms; i++)
        {
            Transforms[i].Clear();
        }
        FlipSprite = 0;
        WaitForTransform = 0;
        YNormalization = 0;
        FadeActive = 0;
    }
}

public sealed class PokemonAnimManager
{
    private const int AnimScriptNarcID = 0x5E;
    private const int AnimScriptCount = 0x8F;
    private const int CommandCount = 0x22;
    private const int MaxCommandsPerFrame = 0x100;
    private const int TransformTypeCount = 5;

    private const int AttributeX = 0;
    private const int AttributeY = 1;
    private const int AttributeRotationZ = 9;
    private const int AttributeUnknown0A = 0xA;
    private const int AttributeScaleX = 0xC;
    private const int AttributeScaleY = 0xD;
    private const int DefaultScale = 0x100;

    private const int CodeTranslateX = 8;
    private const int CodeTranslateY = 9;
    private const int CodeOffsetX = 0xA;
    private const int CodeOffsetY = 0xB;
    private const int CodeScaleX = 0xC;
    private const int CodeScaleY = 0xD;
    private const int CodeRotationZ = 0xE;
    private const int CompareLess = 0xF;
    private const int CompareGreater = 0x10;
    private const int CompareEqual = 0x11;
    private const int OperandVarAndValue = 0x12;
    private const int OperandVarAndVar = 0x13;
    private const int ValueLiteral = 0x14;
    private const int ValueVar = 0x15;
    private const int ModeSet = 0x16;
    private const int ModeAdd = 0x17;
    private const int CalcSet = 0x18;
    private const int CalcAddToOriginal = 0x19;
    private const int CalcAdd = 0x1A;
    private const int YNormalizeNegativeScale = 0x1B;
    private const int YNormalizeNone = 0x1C;
    private const int YNormalizeAnyScale = 0x1D;
    private const int CurveSin = 0x1E;
    private const int CurveCos = 0x1F;
    private const int CurveNegativeSin = 0x20;
    private const int CurveNegativeCos = 0x21;
    private const int TargetOffsetX = 0x23;

    private static readonly PokemonAnimCommand[] sCommands =
    {
        PokemonAnimCmd_End,
        PokemonAnimCmd_WaitFrame,
        PokemonAnimCmd_ResetSprite,
        PokemonAnimCmd_SetVarIf,
        PokemonAnimCmd_SetVar,
        PokemonAnimCmd_CopyVar,
        PokemonAnimCmd_Add,
        PokemonAnimCmd_Multiply,
        PokemonAnimCmd_Subtract,
        PokemonAnimCmd_Divide,
        PokemonAnimCmd_Modulo,
        PokemonAnimCmd_LoopStart,
        PokemonAnimCmd_LoopEnd,
        PokemonAnimCmd_SetSpriteAttribute,
        PokemonAnimCmd_AddSpriteAttribute,
        PokemonAnimCmd_UpdateSpriteAttribute,
        PokemonAnimCmd_Sin,
        PokemonAnimCmd_Cos,
        PokemonAnimCmd_SetTranslation,
        PokemonAnimCmd_AddTranslation,
        PokemonAnimCmd_UpdateMember,
        PokemonAnim_ApplyTranslation,
        PokemonAnim_ApplyScaleAndRotation,
        PokemonAnimCmd_SetOffset,
        PokemonAnimCmd_WaitTransform,
        PokemonAnimCmd_SetYNormalization,
        PokemonAnimCmd_StartCurveTransform,
        PokemonAnimCmd_StartTimedCurveTransform,
        PokemonAnimCmd_StartAcceleratedTransform,
        PokemonAnimCmd_StartLinearTransform,
        PokemonAnimCmd_StartBoundedTransform,
        PokemonAnimCmd_SetStartDelay,
        PokemonAnimCmd_Fade,
        PokemonAnimCmd_WaitFade,
    };

    private static readonly TransformFunc[] sTransformFuncs =
    {
        Transform_Curve,
        Transform_TimedCurve,
        Transform_Accelerated,
        Transform_Linear,
        Transform_Bounded,
    };

    private static uint[]? sTransformTable;

    private static uint[] TransformTable => sTransformTable ??= RomData.Arm9U32(0x020E5598, TransformTypeCount * 3);

    public PokemonAnim[] Anims = Array.Empty<PokemonAnim>();
    public int HeapID;
    public byte FlipSprite;
    public byte AnimCount;

    // OFFSET 0x02015F84
    public static PokemonAnimManager PokemonAnimManager_New(int heapID, int animCount, byte flipSprite)
    {
        PokemonAnimManager monAnimMan = new();
        monAnimMan.FlipSprite = flipSprite;
        monAnimMan.AnimCount = (byte)animCount;
        monAnimMan.HeapID = heapID;
        monAnimMan.Anims = new PokemonAnim[animCount];
        for (int i = 0; i < animCount; i++)
        {
            monAnimMan.Anims[i] = new PokemonAnim();
        }
        return monAnimMan;
    }

    // OFFSET 0x02015FB8
    public static void PokemonAnimManager_Free(PokemonAnimManager monAnimMan)
    {
        monAnimMan.Anims = Array.Empty<PokemonAnim>();
    }

    // OFFSET 0x02015FCC
    public static void PokemonAnimManager_InitAnim(PokemonAnimManager monAnimMan, PokemonSprite monSprite, PokemonAnimTemplate animTemplate, byte index)
    {
        int animation = animTemplate.Animation;
        int startDelay = animTemplate.StartDelay;
        if (index >= monAnimMan.AnimCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        PokemonAnim anim = monAnimMan.Anims[index];
        if (anim.Active != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        anim.Clear();
        anim.Active = 1;
        anim.Sprite = monSprite;
        if (animation >= AnimScriptCount)
        {
            animation = 0;
            startDelay = 0;
        }
        anim.AnimNum = animation;
        if (monAnimMan.FlipSprite != 0)
        {
            anim.FlipSprite = animTemplate.FlipSprite;
        }
        else
        {
            anim.FlipSprite = 0;
        }
        byte[] script = NARC.NARC_AllocAtEndAndReadWholeMemberByIndexPair(AnimScriptNarcID, anim.AnimNum, monAnimMan.HeapID);
        anim.ScriptData = new uint[script.Length / 4];
        for (int i = 0; i < anim.ScriptData.Length; i++)
        {
            anim.ScriptData[i] = Bin.U32(script, i * 4);
        }
        anim.ScriptPtr = 0;
        anim.EndAnim = 0;
        anim.Completed = 0;
        anim.WaitForTransform = 0;
        anim.YNormalization = YNormalizeNone;
        anim.FadeActive = 0;
        anim.Task = SysTask.SysTask_Start(PokemonAnim_Task, anim, 0);
        anim.StartDelay = startDelay;
        anim.OriginalX = PokemonSprite.PokemonSprite_GetAttribute(monSprite, AttributeX);
        anim.OriginalY = PokemonSprite.PokemonSprite_GetAttribute(monSprite, AttributeY);
        anim.Members[PokemonAnim.MemberTranslateX] = 0;
        anim.Members[PokemonAnim.MemberTranslateY] = 0;
        anim.Members[PokemonAnim.MemberOffsetX] = 0;
        anim.Members[PokemonAnim.MemberOffsetY] = 0;
        anim.Members[PokemonAnim.MemberScaleX] = 0;
        anim.Members[PokemonAnim.MemberScaleY] = 0;
        anim.Members[PokemonAnim.MemberRotationZ] = 0;
    }

    // OFFSET 0x020160F4
    public static int PokemonAnimManager_HasAnimCompleted(PokemonAnimManager monAnimMan, byte index)
    {
        if (index >= monAnimMan.AnimCount)
        {
            ErrorHandling.GF_AssertFail();
        }
        return monAnimMan.Anims[index].Completed;
    }

    // OFFSET 0x02016114
    public static void PokemonAnimManager_DeleteAnim(PokemonAnimManager monAnimMan, byte index)
    {
        PokemonAnim anim = monAnimMan.Anims[index];
        if (anim.Task != null)
        {
            SysTask.SysTask_Done(anim.Task);
            anim.Task = null;
            anim.Completed = 1;
            anim.Active = 0;
            anim.ScriptData = null;
        }
    }

    // OFFSET 0x02016150
    private static void PokemonAnim_Task(SysTask task, object? param)
    {
        PokemonAnim anim = (PokemonAnim)param!;
        if (anim.StartDelay == 0)
        {
            PokemonAnim_RunScript(anim);
        }
        else
        {
            anim.StartDelay--;
        }
        if (anim.EndAnim != 0)
        {
            anim.Completed = 1;
            anim.Active = 0;
            SysTask.SysTask_Done(task);
            anim.Task = null;
            anim.ScriptData = null;
        }
    }

    // OFFSET 0x02016188
    private static void PokemonAnim_RunScript(PokemonAnim anim)
    {
        byte idleTransforms = 0;
        anim.WaitFrame = 0;
        anim.CommandCount = 0;
        for (byte i = 0; i < PokemonAnim.MaxTransforms; i++)
        {
            TransformData transform = anim.Transforms[i];
            if (transform.Active != 0)
            {
                if (transform.StartDelay == 0)
                {
                    transform.Func!(transform, anim);
                }
                else
                {
                    transform.StartDelay--;
                }
            }
            else
            {
                idleTransforms++;
            }
        }
        if (idleTransforms == PokemonAnim.MaxTransforms)
        {
            anim.WaitForTransform = 0;
        }
        if (anim.WaitForTransform != 0)
        {
            PokemonAnim_ApplyTranslation(anim);
            PokemonAnim_ApplyScaleAndRotation(anim);
            return;
        }
        if (anim.FadeActive != 0)
        {
            if (PokemonSprite.PokemonSprite_IsFadeActive(anim.Sprite!))
            {
                return;
            }
            anim.FadeActive = 0;
        }
        do
        {
            anim.CommandCount++;
            if (anim.ScriptData![anim.ScriptPtr] >= CommandCount)
            {
                ErrorHandling.GF_AssertFail();
            }
            sCommands[anim.ScriptData[anim.ScriptPtr]](anim);
            if (anim.EndAnim != 0)
            {
                return;
            }
            anim.ScriptPtr++;
            if (anim.WaitFrame != 0)
            {
                return;
            }
            if (anim.WaitForTransform != 0)
            {
                PokemonAnim_ApplyTranslation(anim);
                PokemonAnim_ApplyScaleAndRotation(anim);
                return;
            }
        } while (anim.CommandCount < MaxCommandsPerFrame);
        ErrorHandling.GF_AssertFail();
        anim.EndAnim = 1;
    }

    // OFFSET 0x02016280
    private static int PokemonAnim_ReadScriptWord(uint[] script, int scriptPtr, int index, int wordSize)
    {
        int value = (int)script[scriptPtr + index];
        if (wordSize != 1)
        {
            ErrorHandling.GF_AssertFail();
        }
        return value;
    }

    // OFFSET 0x02016294
    private static int PokemonAnim_ReadScriptWordAt(uint[] script, int scriptPtr, int wordSize)
    {
        return PokemonAnim_ReadScriptWord(script, scriptPtr, 0, wordSize);
    }

    // OFFSET 0x020162A0
    private static int PokemonAnim_ReadScriptValue(uint[] script, int scriptPtr)
    {
        return PokemonAnim_ReadScriptWordAt(script, scriptPtr, 1);
    }

    // OFFSET 0x020162AC
    private static TransformData? PokemonAnim_AllocTransform(PokemonAnim anim, byte transformType)
    {
        for (byte i = 0; i < PokemonAnim.MaxTransforms; i++)
        {
            TransformData transform = anim.Transforms[i];
            if (transform.Active == 0)
            {
                transform.Clear();
                transform.Active = 1;
                transform.Func = sTransformFuncs[transformType];
                return transform;
            }
        }
        ErrorHandling.GF_AssertFail();
        return null;
    }

    // OFFSET 0x020162F8
    private static void PokemonAnim_ReadInt(PokemonAnim anim, out int value)
    {
        anim.ScriptPtr++;
        value = PokemonAnim_ReadScriptValue(anim.ScriptData!, anim.ScriptPtr);
    }

    // OFFSET 0x0201630C
    private static void PokemonAnim_ReadByte(PokemonAnim anim, out byte value)
    {
        anim.ScriptPtr++;
        value = (byte)PokemonAnim_ReadScriptValue(anim.ScriptData!, anim.ScriptPtr);
    }

    // OFFSET 0x02016320
    private static void PokemonAnim_ReadVarIndex(PokemonAnim anim, out byte index)
    {
        anim.ScriptPtr++;
        index = (byte)PokemonAnim_ReadScriptValue(anim.ScriptData!, anim.ScriptPtr);
        if (index >= PokemonAnim.MaxVars)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02016340
    private static void PokemonAnim_ReadTwoVarIndices(PokemonAnim anim, out byte first, out byte second)
    {
        PokemonAnim_ReadVarIndex(anim, out first);
        PokemonAnim_ReadVarIndex(anim, out second);
    }

    // OFFSET 0x02016354
    private static void PokemonAnim_ReadVarFirstOperands(PokemonAnim anim, out byte destination, out int left, out int right)
    {
        left = 0;
        right = 0;
        PokemonAnim_ReadVarIndex(anim, out destination);
        PokemonAnim_ReadByte(anim, out byte operandType);
        if (operandType == OperandVarAndValue)
        {
            PokemonAnim_ReadVarIndex(anim, out byte leftVar);
            left = anim.Vars[leftVar];
            PokemonAnim_ReadInt(anim, out right);
        }
        else if (operandType == OperandVarAndVar)
        {
            PokemonAnim_ReadTwoVarIndices(anim, out byte leftVar, out byte rightVar);
            left = anim.Vars[leftVar];
            right = anim.Vars[rightVar];
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x020163C8
    private static void PokemonAnim_ReadTypedOperands(PokemonAnim anim, out byte destination, out int left, out int right)
    {
        left = 0;
        right = 0;
        PokemonAnim_ReadVarIndex(anim, out destination);
        PokemonAnim_ReadByte(anim, out byte leftType);
        PokemonAnim_ReadByte(anim, out byte rightType);
        if (leftType == OperandVarAndValue)
        {
            PokemonAnim_ReadInt(anim, out left);
        }
        else if (leftType == OperandVarAndVar)
        {
            PokemonAnim_ReadVarIndex(anim, out byte leftVar);
            left = anim.Vars[leftVar];
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        if (rightType == OperandVarAndValue)
        {
            PokemonAnim_ReadInt(anim, out right);
        }
        else if (rightType == OperandVarAndVar)
        {
            PokemonAnim_ReadVarIndex(anim, out byte rightVar);
            right = anim.Vars[rightVar];
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02016454
    private static void PokemonAnim_ReadTrigOperands(PokemonAnim anim, out byte destination, out int angle, out int amplitude)
    {
        amplitude = 0;
        int angleOffset = 0;
        PokemonAnim_ReadTwoVarIndices(anim, out destination, out byte angleVar);
        int baseAngle = anim.Vars[angleVar];
        PokemonAnim_ReadByte(anim, out byte valueType);
        if (valueType == ValueLiteral)
        {
            PokemonAnim_ReadInt(anim, out amplitude);
        }
        else if (valueType == ValueVar)
        {
            PokemonAnim_ReadVarIndex(anim, out byte amplitudeVar);
            amplitude = anim.Vars[amplitudeVar];
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        PokemonAnim_ReadByte(anim, out valueType);
        if (valueType == ValueLiteral)
        {
            PokemonAnim_ReadInt(anim, out angleOffset);
        }
        else if (valueType == ValueVar)
        {
            PokemonAnim_ReadVarIndex(anim, out byte offsetVar);
            angleOffset = anim.Vars[offsetVar];
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        angle = (baseAngle + angleOffset) % 0x10000;
    }

    // OFFSET 0x020164FC
    private static byte PokemonAnim_Compare(int left, int right)
    {
        int difference = left - right;
        if (difference < 0)
        {
            return CompareLess;
        }
        if (difference > 0)
        {
            return CompareGreater;
        }
        return CompareEqual;
    }

    // OFFSET 0x02016514
    private static void PokemonAnim_NormalizeY(PokemonAnim anim)
    {
        PokemonSprite.PokemonSprite_AddAttribute(anim.Sprite!, AttributeY, -anim.Members[PokemonAnim.MemberScaleY] / 8);
    }

    // OFFSET 0x02016530
    private static void PokemonAnimCmd_End(PokemonAnim anim)
    {
        PokemonAnimCmd_ResetSprite(anim);
        anim.WaitFrame = 1;
        anim.EndAnim = 1;
    }

    // OFFSET 0x02016540
    private static void PokemonAnimCmd_WaitFrame(PokemonAnim anim)
    {
        anim.WaitFrame = 1;
    }

    // OFFSET 0x02016548
    private static void PokemonAnimCmd_ResetSprite(PokemonAnim anim)
    {
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, AttributeX, anim.OriginalX);
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, AttributeY, anim.OriginalY);
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, AttributeRotationZ, 0);
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, AttributeUnknown0A, 0);
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, AttributeScaleX, DefaultScale);
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, AttributeScaleY, DefaultScale);
    }

    // OFFSET 0x02016590
    private static void PokemonAnimCmd_CopyVar(PokemonAnim anim)
    {
        PokemonAnim_ReadTwoVarIndices(anim, out byte destination, out byte source);
        anim.Vars[destination] = anim.Vars[source];
    }

    // OFFSET 0x020165B8
    private static void PokemonAnimCmd_Add(PokemonAnim anim)
    {
        PokemonAnim_ReadVarFirstOperands(anim, out byte destination, out int left, out int right);
        anim.Vars[destination] = left + right;
    }

    // OFFSET 0x020165DC
    private static void PokemonAnimCmd_Multiply(PokemonAnim anim)
    {
        PokemonAnim_ReadVarFirstOperands(anim, out byte destination, out int left, out int right);
        anim.Vars[destination] = left * right;
    }

    // OFFSET 0x02016604
    private static void PokemonAnimCmd_Subtract(PokemonAnim anim)
    {
        PokemonAnim_ReadTypedOperands(anim, out byte destination, out int left, out int right);
        anim.Vars[destination] = left - right;
    }

    // OFFSET 0x02016628
    private static void PokemonAnimCmd_Divide(PokemonAnim anim)
    {
        PokemonAnim_ReadTypedOperands(anim, out byte destination, out int left, out int right);
        anim.Vars[destination] = left / right;
    }

    // OFFSET 0x02016650
    private static void PokemonAnimCmd_Modulo(PokemonAnim anim)
    {
        PokemonAnim_ReadTypedOperands(anim, out byte destination, out int left, out int right);
        anim.Vars[destination] = left % right;
    }

    // OFFSET 0x02016678
    private static void PokemonAnimCmd_SetVarIf(PokemonAnim anim)
    {
        byte destination = 0;
        int left = 0;
        int right = 0;
        int value = 0;
        PokemonAnim_ReadByte(anim, out byte valueType);
        if (valueType == ValueLiteral)
        {
            PokemonAnim_ReadVarIndex(anim, out destination);
            left = anim.Vars[destination];
            PokemonAnim_ReadInt(anim, out right);
        }
        else if (valueType == ValueVar)
        {
            PokemonAnim_ReadTwoVarIndices(anim, out destination, out byte rightVar);
            left = anim.Vars[destination];
            right = anim.Vars[rightVar];
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        PokemonAnim_ReadByte(anim, out byte condition);
        if (condition > CompareEqual)
        {
            ErrorHandling.GF_AssertFail();
        }
        byte result = PokemonAnim_Compare(left, right);
        PokemonAnim_ReadByte(anim, out valueType);
        if (valueType == ValueLiteral)
        {
            PokemonAnim_ReadVarIndex(anim, out destination);
            PokemonAnim_ReadInt(anim, out value);
        }
        else if (valueType == ValueVar)
        {
            PokemonAnim_ReadTwoVarIndices(anim, out destination, out byte valueVar);
            value = anim.Vars[valueVar];
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        if (condition == result)
        {
            anim.Vars[destination] = value;
        }
    }

    // OFFSET 0x02016758
    private static void PokemonAnimCmd_SetVar(PokemonAnim anim)
    {
        PokemonAnim_ReadVarIndex(anim, out byte destination);
        anim.ScriptPtr++;
        anim.Vars[destination] = PokemonAnim_ReadScriptValue(anim.ScriptData!, anim.ScriptPtr);
    }

    // OFFSET 0x0201677C
    private static void PokemonAnimCmd_LoopStart(PokemonAnim anim)
    {
        if (anim.LoopStart != 0)
        {
            ErrorHandling.GF_AssertFail();
        }
        anim.ScriptPtr++;
        anim.LoopStart = anim.ScriptPtr;
        anim.LoopMax = PokemonAnim_ReadScriptValue(anim.ScriptData!, anim.ScriptPtr);
        anim.LoopCounter = 0;
    }

    // OFFSET 0x020167A0
    private static void PokemonAnimCmd_LoopEnd(PokemonAnim anim)
    {
        anim.LoopCounter++;
        if (anim.LoopCounter >= anim.LoopMax)
        {
            anim.LoopStart = 0;
            anim.LoopCounter = 0;
            anim.LoopMax = 0;
            return;
        }
        anim.ScriptPtr = anim.LoopStart;
    }

    // OFFSET 0x020167BC
    private static void PokemonAnimCmd_SetSpriteAttribute(PokemonAnim anim)
    {
        PokemonAnim_ReadInt(anim, out int attribute);
        PokemonAnim_ReadVarIndex(anim, out byte valueVar);
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, attribute, anim.Vars[valueVar]);
    }

    // OFFSET 0x020167E8
    private static void PokemonAnimCmd_AddSpriteAttribute(PokemonAnim anim)
    {
        PokemonAnim_ReadInt(anim, out int attribute);
        PokemonAnim_ReadVarIndex(anim, out byte valueVar);
        PokemonSprite.PokemonSprite_AddAttribute(anim.Sprite!, attribute, anim.Vars[valueVar]);
    }

    // OFFSET 0x02016814
    private static void PokemonAnimCmd_UpdateSpriteAttribute(PokemonAnim anim)
    {
        int value = 0;
        PokemonAnim_ReadInt(anim, out int attribute);
        PokemonAnim_ReadByte(anim, out byte valueType);
        if (valueType == ValueLiteral)
        {
            PokemonAnim_ReadInt(anim, out value);
        }
        else if (valueType == ValueVar)
        {
            PokemonAnim_ReadVarIndex(anim, out byte valueVar);
            value = anim.Vars[valueVar];
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        PokemonAnim_ReadByte(anim, out byte mode);
        if (mode == ModeSet)
        {
            PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, attribute, value);
        }
        else if (mode == ModeAdd)
        {
            PokemonSprite.PokemonSprite_AddAttribute(anim.Sprite!, attribute, value);
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02016894
    private static void PokemonAnimCmd_Sin(PokemonAnim anim)
    {
        PokemonAnim_ReadTrigOperands(anim, out byte destination, out int angle, out int amplitude);
        anim.Vars[destination] = (Render3D.Sin(angle >> 4) * amplitude) >> Fx32.Shift;
    }

    // OFFSET 0x020168C8
    private static void PokemonAnimCmd_Cos(PokemonAnim anim)
    {
        PokemonAnim_ReadTrigOperands(anim, out byte destination, out int angle, out int amplitude);
        anim.Vars[destination] = (Render3D.Cos(angle >> 4) * amplitude) >> Fx32.Shift;
    }

    // OFFSET 0x02016900
    private static void PokemonAnimCmd_SetTranslation(PokemonAnim anim)
    {
        PokemonAnim_ReadVarIndex(anim, out byte valueVar);
        PokemonAnim_ReadByte(anim, out byte member);
        if (member == CodeTranslateX)
        {
            anim.Members[PokemonAnim.MemberTranslateX] = anim.Vars[valueVar];
        }
        else if (member == CodeTranslateY)
        {
            anim.Members[PokemonAnim.MemberTranslateY] = anim.Vars[valueVar];
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02016948
    private static void PokemonAnimCmd_AddTranslation(PokemonAnim anim)
    {
        PokemonAnim_ReadVarIndex(anim, out byte valueVar);
        PokemonAnim_ReadByte(anim, out byte member);
        if (member == CodeTranslateX)
        {
            anim.Members[PokemonAnim.MemberTranslateX] += anim.Vars[valueVar];
        }
        else if (member == CodeTranslateY)
        {
            anim.Members[PokemonAnim.MemberTranslateY] += anim.Vars[valueVar];
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02016998
    private static void PokemonAnimCmd_UpdateMember(PokemonAnim anim)
    {
        int member = 0;
        int value = 0;
        PokemonAnim_ReadByte(anim, out byte memberCode);
        if (memberCode == CodeTranslateX)
        {
            member = PokemonAnim.MemberTranslateX;
        }
        else if (memberCode == CodeTranslateY)
        {
            member = PokemonAnim.MemberTranslateY;
        }
        else if (memberCode == CodeOffsetX)
        {
            member = PokemonAnim.MemberOffsetX;
        }
        else if (memberCode == CodeOffsetY)
        {
            member = PokemonAnim.MemberOffsetY;
        }
        else if (memberCode == CodeScaleX)
        {
            member = PokemonAnim.MemberScaleX;
        }
        else if (memberCode == CodeScaleY)
        {
            member = PokemonAnim.MemberScaleY;
        }
        else if (memberCode == CodeRotationZ)
        {
            member = PokemonAnim.MemberRotationZ;
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        PokemonAnim_ReadByte(anim, out byte valueType);
        if (valueType == ValueLiteral)
        {
            PokemonAnim_ReadInt(anim, out value);
        }
        else if (valueType == ValueVar)
        {
            PokemonAnim_ReadVarIndex(anim, out byte valueVar);
            value = anim.Vars[valueVar];
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        PokemonAnim_ReadByte(anim, out byte mode);
        if (mode == ModeSet)
        {
            anim.Members[member] = value;
        }
        else if (mode == ModeAdd)
        {
            anim.Members[member] += value;
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02016A60
    private static void PokemonAnim_ApplyTranslation(PokemonAnim anim)
    {
        if (anim.FlipSprite != 0)
        {
            PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, AttributeX,
                anim.OriginalX - (anim.Members[PokemonAnim.MemberTranslateX] + anim.Members[PokemonAnim.MemberOffsetX]));
        }
        else
        {
            PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, AttributeX,
                anim.Members[PokemonAnim.MemberOffsetX] + (anim.OriginalX + anim.Members[PokemonAnim.MemberTranslateX]));
        }
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, AttributeY,
            anim.Members[PokemonAnim.MemberOffsetY] + (anim.OriginalY + anim.Members[PokemonAnim.MemberTranslateY]));
    }

    // OFFSET 0x02016AA8
    private static void PokemonAnim_ApplyScaleAndRotation(PokemonAnim anim)
    {
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, AttributeScaleX, anim.Members[PokemonAnim.MemberScaleX] + DefaultScale);
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, AttributeScaleY, anim.Members[PokemonAnim.MemberScaleY] + DefaultScale);
        PokemonSprite.PokemonSprite_SetAttribute(anim.Sprite!, AttributeRotationZ, (ushort)anim.Members[PokemonAnim.MemberRotationZ]);
        if (anim.YNormalization == YNormalizeNegativeScale)
        {
            if (anim.Members[PokemonAnim.MemberScaleY] < 0)
            {
                PokemonAnim_NormalizeY(anim);
            }
        }
        else if (anim.YNormalization == YNormalizeAnyScale)
        {
            if (anim.Members[PokemonAnim.MemberScaleY] != 0)
            {
                PokemonAnim_NormalizeY(anim);
            }
        }
        else if (anim.YNormalization != YNormalizeNone)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02016B10
    private static void PokemonAnimCmd_SetOffset(PokemonAnim anim)
    {
        PokemonAnim_ReadVarIndex(anim, out byte valueVar);
        anim.ScriptPtr++;
        byte member = (byte)PokemonAnim_ReadScriptValue(anim.ScriptData!, anim.ScriptPtr);
        if (member == CodeTranslateX || member == CodeOffsetX)
        {
            anim.Members[PokemonAnim.MemberOffsetX] = anim.Vars[valueVar];
        }
        else if (member == CodeTranslateY || member == CodeOffsetY)
        {
            anim.Members[PokemonAnim.MemberOffsetY] = anim.Vars[valueVar];
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02016B64
    private static void PokemonAnimCmd_SetStartDelay(PokemonAnim anim)
    {
        PokemonAnim_ReadInt(anim, out anim.StartDelay);
        anim.WaitFrame = 1;
    }

    // OFFSET 0x02016B78
    private static void PokemonAnimCmd_Fade(PokemonAnim anim)
    {
        PokemonAnim_ReadByte(anim, out byte initAlpha);
        PokemonAnim_ReadByte(anim, out byte targetAlpha);
        PokemonAnim_ReadByte(anim, out byte delay);
        PokemonAnim_ReadInt(anim, out int color);
        PokemonSprite.PokemonSprite_StartFade(anim.Sprite!, initAlpha, targetAlpha, delay, color);
    }

    // OFFSET 0x02016BB8
    private static void PokemonAnimCmd_WaitFade(PokemonAnim anim)
    {
        if (PokemonSprite.PokemonSprite_IsFadeActive(anim.Sprite!))
        {
            anim.FadeActive = 1;
            anim.WaitFrame = 1;
        }
    }

    // OFFSET 0x02016BD4
    private static void PokemonAnimCmd_WaitTransform(PokemonAnim anim)
    {
        anim.WaitForTransform = 1;
    }

    // OFFSET 0x02016BE0
    private static void PokemonAnimCmd_SetYNormalization(PokemonAnim anim)
    {
        PokemonAnim_ReadByte(anim, out anim.YNormalization);
        bool valid = true;
        bool scaled = true;
        if (anim.YNormalization != YNormalizeNegativeScale && anim.YNormalization != YNormalizeAnyScale)
        {
            scaled = false;
        }
        if (!scaled && anim.YNormalization != YNormalizeNone)
        {
            valid = false;
        }
        if (!valid)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02016C18
    private static void PokemonAnimCmd_StartCurveTransform(PokemonAnim anim)
    {
        PokemonAnim_StartTransform(anim, 0);
    }

    // OFFSET 0x02016C24
    private static void PokemonAnimCmd_StartTimedCurveTransform(PokemonAnim anim)
    {
        PokemonAnim_StartTransform(anim, 1);
    }

    // OFFSET 0x02016C30
    private static void PokemonAnimCmd_StartAcceleratedTransform(PokemonAnim anim)
    {
        PokemonAnim_StartTransform(anim, 2);
    }

    // OFFSET 0x02016C3C
    private static void PokemonAnimCmd_StartLinearTransform(PokemonAnim anim)
    {
        PokemonAnim_StartTransform(anim, 3);
    }

    // OFFSET 0x02016C48
    private static void PokemonAnimCmd_StartBoundedTransform(PokemonAnim anim)
    {
        PokemonAnim_StartTransform(anim, 4);
    }

    // OFFSET 0x02016C54
    private static void Transform_Apply(TransformData transform, PokemonAnim anim)
    {
        int animMember = transform.TargetMember + PokemonAnim.MemberOffsetX;
        int value = transform.Members[transform.TargetMember];
        if (transform.CalcType == CalcSet)
        {
            anim.Members[animMember] = value;
        }
        else if (transform.CalcType == CalcAddToOriginal)
        {
            anim.Members[animMember] = transform.OriginalValue + value;
        }
        else if (transform.CalcType == CalcAdd)
        {
            anim.Members[animMember] += value;
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    // OFFSET 0x02016C84
    private static void Transform_SetTarget(byte targetCode, TransformData transform, PokemonAnim anim)
    {
        int member = targetCode - TargetOffsetX;
        if ((uint)member >= TransformData.MemberCount)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        transform.TargetMember = member;
        transform.OriginalValue = anim.Members[member + PokemonAnim.MemberOffsetX];
    }

    // OFFSET 0x02016D04
    private static void PokemonAnim_StartTransform(PokemonAnim anim, int transformType)
    {
        TransformData transform = PokemonAnim_AllocTransform(anim, (byte)transformType)!;
        PokemonAnim_ReadByte(anim, out transform.CalcType);
        PokemonAnim_ReadByte(anim, out transform.StartDelay);
        uint[] table = TransformTable;
        for (byte i = 0; i < (int)table[transformType * 3 + 1]; i++)
        {
            PokemonAnim_ReadInt(anim, out transform.Vars[i]);
        }
        Transform_SetTarget((byte)transform.Vars[table[transformType * 3 + 2]], transform, anim);
        if (transform.StartDelay == 0)
        {
            transform.Func!(transform, anim);
            return;
        }
        transform.StartDelay--;
    }

    private static int Transform_CalcCurve(TransformData transform, ushort angle)
    {
        switch (transform.Vars[0])
        {
            case CurveSin:
                return (Render3D.Sin(angle >> 4) * transform.Vars[2]) >> Fx32.Shift;
            case CurveCos:
                return (Render3D.Cos(angle >> 4) * transform.Vars[2]) >> Fx32.Shift;
            case CurveNegativeSin:
                return -((Render3D.Sin(angle >> 4) * transform.Vars[2]) >> Fx32.Shift);
            case CurveNegativeCos:
                return -((Render3D.Cos(angle >> 4) * transform.Vars[2]) >> Fx32.Shift);
            default:
                ErrorHandling.GF_AssertFail();
                return transform.Members[transform.TargetMember];
        }
    }

    // OFFSET 0x02016DAC
    private static void Transform_Curve(TransformData transform, PokemonAnim anim)
    {
        ushort angle = (ushort)(transform.Vars[4] + transform.Vars[3] * (transform.Vars[6] + 1));
        transform.Members[transform.TargetMember] = Transform_CalcCurve(transform, angle);
        Transform_Apply(transform, anim);
        transform.Vars[6]++;
        if (transform.Vars[6] >= transform.Vars[5])
        {
            transform.Active = 0;
        }
    }

    // OFFSET 0x02016E64
    private static void Transform_TimedCurve(TransformData transform, PokemonAnim anim)
    {
        ushort angle = (ushort)(transform.Vars[4] + transform.Vars[3] * (transform.Vars[6] + 1) / transform.Vars[5]);
        transform.Members[transform.TargetMember] = Transform_CalcCurve(transform, angle);
        Transform_Apply(transform, anim);
        transform.Vars[6]++;
        if (transform.Vars[6] >= transform.Vars[5])
        {
            transform.Active = 0;
        }
    }

    // OFFSET 0x02016F24
    private static void Transform_Accelerated(TransformData transform, PokemonAnim anim)
    {
        transform.Members[transform.TargetMember] += transform.Vars[1] + transform.Vars[2] * transform.Vars[4];
        Transform_Apply(transform, anim);
        transform.Vars[4]++;
        if (transform.Vars[4] >= transform.Vars[3])
        {
            transform.Active = 0;
        }
    }

    // OFFSET 0x02016F60
    private static void Transform_Linear(TransformData transform, PokemonAnim anim)
    {
        transform.Members[transform.TargetMember] = (transform.Vars[3] + 1) * transform.Vars[1] / transform.Vars[2];
        Transform_Apply(transform, anim);
        transform.Vars[3]++;
        if (transform.Vars[3] >= transform.Vars[2])
        {
            transform.Active = 0;
        }
    }

    // OFFSET 0x02016F9C
    private static void Transform_Bounded(TransformData transform, PokemonAnim anim)
    {
        int delta = transform.Vars[1] + transform.Vars[2] * transform.Vars[4];
        int target = transform.Vars[3];
        transform.Members[transform.TargetMember] += delta;
        if (transform.CalcType == CalcSet || transform.CalcType == CalcAdd)
        {
            if (delta < 0)
            {
                if (transform.Members[transform.TargetMember] <= target)
                {
                    transform.Members[transform.TargetMember] = target;
                    transform.Active = 0;
                }
            }
            else
            {
                if (transform.Members[transform.TargetMember] >= target)
                {
                    transform.Members[transform.TargetMember] = target;
                    transform.Active = 0;
                }
            }
        }
        else if (transform.CalcType == CalcAddToOriginal)
        {
            int current = transform.Members[transform.TargetMember];
            int absolute = transform.OriginalValue + current;
            if (delta < 0)
            {
                if (absolute <= target)
                {
                    transform.Members[transform.TargetMember] = current + (target - absolute);
                    transform.Active = 0;
                }
            }
            else
            {
                if (absolute >= target)
                {
                    transform.Members[transform.TargetMember] = current - (absolute - target);
                    transform.Active = 0;
                }
            }
        }
        else
        {
            ErrorHandling.GF_AssertFail();
        }
        Transform_Apply(transform, anim);
        transform.Vars[4]++;
    }
}
