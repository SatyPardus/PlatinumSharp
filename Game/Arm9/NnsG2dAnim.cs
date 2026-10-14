using PlatinumSharp.Nitro;
using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public delegate void AnimCallback(object? param, int value);

public sealed class AnimSequence
{
    public byte[] File = Array.Empty<byte>();
    public int Offset;
    public int FrameBase;
    public int ContentBase;

    public int NumFrames => Bin.U16(File, Offset);

    public int LoopStartFrameIdx => Bin.U16(File, Offset + 2);

    public int ElementType => Bin.U16(File, Offset + 4) & 0xFF;

    public int PlayMode => (int)Bin.U32(File, Offset + 8);

    public int Frames => FrameBase + (int)Bin.U32(File, Offset + 0xC);
}

public sealed class AnimController
{
    public int Current;
    public int ActiveCurrent;
    public int Reverse;
    public int Active;
    public int CurrentTime;
    public int Speed;
    public int OverriddenPlayMode;
    public AnimSequence? Sequence;
    public int CallbackType;
    public object? CallbackParam;
    public AnimCallback? CallbackFunc;
    public ushort CallbackFrameIdx;
}

public sealed class SrtControl
{
    public int Type;
    public int Sx;
    public int Sy;
    public short Px;
    public short Py;
    public ushort RotZ;
    public ushort EnableFlag;
}

public sealed class G2dNode
{
    public CellAnimation? Content;
    public int Type;
    public int Visible;
    public SrtControl Srt = new();
}

public sealed class CellAnimation
{
    public AnimController AnimCtrl = new();
    public int CurrentCell;
    public CellDataBank? CellBank;
    public uint TransferHandle;
    public SrtControl Srt = new();
    public int Inited;
}

public sealed class MultiCellAnimation
{
    public AnimController AnimCtrl = new();
    public ushort TotalVideoFrame;
    public int CurrentMultiCell;
    public AnimBankData? AnimDataBank;
    public int McType;
    public G2dNode[]? Nodes;
    public CellAnimation[]? SharedCellAnims;
    public MultiCellDataBank? MultiCellBank;
    public SrtControl Srt = new();
}

public static partial class NnsG2d
{
    public const int PlayModeForward = 1;
    public const int PlayModeForwardLoop = 2;
    public const int PlayModeReverse = 3;
    public const int PlayModeReverseLoop = 4;
    public const int CallbackNone = 0;
    public const int CallbackLastFrame = 1;
    public const int CallbackSpecificFrame = 2;
    public const int CallbackEveryFrame = 3;
    public const int ElementIndex = 0;
    public const int ElementIndexSrt = 1;
    public const int ElementIndexT = 2;
    public const int SrtTypeNormal = 1;
    public const int SrtScale = 2;
    public const int SrtRotZ = 4;
    public const int SrtTrans = 8;
    public const int McTypeNodes = 0;
    public const int McTypeShareCellAnim = 1;
    public const uint NoTransferHandle = 0xFFFFFFFF;

    private const int BlockHeaderSize = 8;
    private const int FrameSize = 8;
    private const int SequenceSize = 16;
    private const int CellSize = 8;
    private const int CellWithBoundsSize = 16;
    private const int MultiCellSize = 8;
    private const int HierarchySize = 8;
    private const int NodePlayModeContinue = 1;

    private static int FrameDuration(AnimController controller, int frame)
    {
        return Bin.U16(controller.Sequence!.File, frame + 4);
    }

    // OFFSET 0x020A6EF0
    public static AnimSequence? GetAnimSequenceByIdx(AnimBankData? bank, ushort index)
    {
        byte[] file = bank!.File;
        int data = bank.SectionOffset + BlockHeaderSize;
        if (Bin.U16(file, data) <= index)
        {
            return null;
        }
        AnimSequence sequence = new();
        sequence.File = file;
        sequence.Offset = data + (int)Bin.U32(file, data + 4) + index * SequenceSize;
        sequence.FrameBase = data + (int)Bin.U32(file, data + 8);
        sequence.ContentBase = data + (int)Bin.U32(file, data + 0xC);
        return sequence;
    }

    public static AnimSequence? GetAnimSequenceByIdx(MultiCellAnimBankData? bank, ushort index)
    {
        return GetAnimSequenceByIdx(new AnimBankData { File = bank!.File, SectionOffset = bank.SectionOffset }, index);
    }

    // OFFSET 0x020A6F74
    public static int GetCellDataByIdx(CellDataBank bank, ushort index)
    {
        byte[] file = bank.File;
        int data = bank.SectionOffset + BlockHeaderSize;
        if (index >= Bin.U16(file, data))
        {
            return 0;
        }
        int head = data + (int)Bin.U32(file, data + 4);
        if ((Bin.U16(file, data + 2) & 1) != 0)
        {
            return head + index * CellWithBoundsSize;
        }
        return head + index * CellSize;
    }

    // OFFSET 0x020A7100
    public static int GetMultiCellDataByIdx(MultiCellDataBank bank, ushort index)
    {
        byte[] file = bank.File;
        int data = bank.SectionOffset + BlockHeaderSize;
        if (index < Bin.U16(file, data))
        {
            return data + (int)Bin.U32(file, data + 4) + index * MultiCellSize;
        }
        return 0;
    }

    private static int GetPlayMode(AnimController controller)
    {
        if (controller.OverriddenPlayMode != 0)
        {
            return controller.OverriddenPlayMode;
        }
        return controller.Sequence!.PlayMode;
    }

    private static void FinishSequence(AnimController controller)
    {
        if (controller.CallbackType == CallbackLastFrame)
        {
            controller.CallbackFunc!(controller.CallbackParam, controller.CurrentTime);
        }
        int playMode = GetPlayMode(controller);
        if (playMode == PlayModeForwardLoop || playMode == PlayModeReverseLoop)
        {
            ResetAnimCtrlState(controller);
        }
        else
        {
            controller.Active = 0;
        }
    }

    // OFFSET 0x020A74F0
    private static void AnimCtrl_HandleSequenceEdge(AnimController controller)
    {
        AnimSequence sequence = controller.Sequence!;
        int playMode = GetPlayMode(controller);
        if (playMode == PlayModeReverse || playMode == PlayModeReverseLoop)
        {
            controller.Reverse ^= 1;
            if (controller.Current <= sequence.Frames + sequence.LoopStartFrameIdx * FrameSize - FrameSize)
            {
                FinishSequence(controller);
            }
        }
        else
        {
            FinishSequence(controller);
        }
        int last = sequence.Frames + sequence.NumFrames * FrameSize - FrameSize;
        if (controller.Current > last)
        {
            controller.Current = last;
            return;
        }
        if (controller.Current < sequence.Frames)
        {
            controller.Current = sequence.Frames;
        }
    }

    // OFFSET 0x020A7604
    private static bool AnimCtrl_SetCurrentFrameImpl(AnimController controller, ushort index)
    {
        AnimSequence sequence = controller.Sequence!;
        if (index < sequence.NumFrames)
        {
            controller.Current = sequence.Frames + index * FrameSize;
            if (FrameDuration(controller, controller.Current) != 0)
            {
                controller.ActiveCurrent = controller.Current;
            }
            return true;
        }
        return false;
    }

    // OFFSET 0x020A763C
    public static int GetAnimCtrlCurrentElement(AnimController controller)
    {
        AnimSequence sequence = controller.Sequence!;
        return sequence.ContentBase + (int)Bin.U32(sequence.File, controller.ActiveCurrent);
    }

    private static bool IsMovingForward(AnimController controller)
    {
        int forward = controller.Speed > 0 ? 1 : 0;
        return forward != controller.Reverse;
    }

    // OFFSET 0x020A7648
    public static bool TickAnimCtrl(AnimController controller, int frames)
    {
        bool changed = false;
        if (controller.Active != 1)
        {
            return false;
        }
        controller.CurrentTime += Math.Abs(Fx32.Mul(controller.Speed, frames));
        while (controller.Active != 0 && controller.CurrentTime >= FrameDuration(controller, controller.Current) << Fx32.Shift)
        {
            AnimSequence sequence = controller.Sequence!;
            changed = true;
            controller.CurrentTime -= FrameDuration(controller, controller.Current) << Fx32.Shift;
            if (IsMovingForward(controller))
            {
                controller.Current += FrameSize;
            }
            else
            {
                controller.Current -= FrameSize;
            }
            bool atEdge;
            if (IsMovingForward(controller))
            {
                atEdge = controller.Current >= sequence.Frames + sequence.NumFrames * FrameSize;
            }
            else
            {
                atEdge = controller.Current <= sequence.Frames + sequence.LoopStartFrameIdx * FrameSize - FrameSize;
            }
            if (atEdge)
            {
                AnimCtrl_HandleSequenceEdge(controller);
            }
            if (FrameDuration(controller, controller.Current) != 0)
            {
                controller.ActiveCurrent = controller.Current;
            }
            if (controller.CallbackType != CallbackNone)
            {
                ushort frameIdx = (ushort)((controller.Current - controller.Sequence!.Frames) / FrameSize);
                switch (controller.CallbackType)
                {
                    case CallbackSpecificFrame:
                        if (frameIdx == controller.CallbackFrameIdx)
                        {
                            controller.CallbackFunc!(controller.CallbackParam, frameIdx);
                        }
                        break;
                    case CallbackEveryFrame:
                        controller.CallbackFunc!(controller.CallbackParam, frameIdx);
                        break;
                }
            }
        }
        return changed;
    }

    // OFFSET 0x020A77FC
    public static bool SetAnimCtrlCurrentFrame(AnimController controller, ushort index)
    {
        bool result = AnimCtrl_SetCurrentFrameImpl(controller, index);
        if (result)
        {
            controller.CurrentTime = 0;
        }
        return result;
    }

    // OFFSET 0x020A7818
    public static ushort GetAnimCtrlCurrentFrame(AnimController controller)
    {
        return (ushort)((controller.Current - controller.Sequence!.Frames) / FrameSize);
    }

    // OFFSET 0x020A7834
    public static void InitAnimCtrl(AnimController controller)
    {
        InitAnimCallback(controller);
        controller.Current = 0;
        controller.ActiveCurrent = 0;
        controller.Reverse = 0;
        controller.Active = 1;
        controller.CurrentTime = 0;
        controller.Speed = Fx32.One;
        controller.OverriddenPlayMode = 0;
        controller.Sequence = null;
    }

    // OFFSET 0x020A7874
    private static void InitAnimCallback(AnimController controller)
    {
        controller.CallbackType = CallbackNone;
        controller.CallbackParam = null;
        controller.CallbackFunc = null;
        controller.CallbackFrameIdx = 0;
    }

    // OFFSET 0x020A788C
    public static void ResetAnimCtrlState(AnimController controller)
    {
        AnimSequence sequence = controller.Sequence!;
        if (IsMovingForward(controller))
        {
            controller.Current = sequence.Frames + sequence.LoopStartFrameIdx * FrameSize;
        }
        else
        {
            controller.Current = sequence.Frames + sequence.NumFrames * FrameSize - FrameSize;
        }
        controller.ActiveCurrent = controller.Current;
        controller.CurrentTime = 0;
        TickAnimCtrl(controller, 0);
    }

    // OFFSET 0x020A78EC
    public static void BindAnimCtrl(AnimController controller, AnimSequence? sequence)
    {
        controller.Sequence = sequence;
        ResetAnimCtrlState(controller);
    }

    // OFFSET 0x020A78FC
    public static bool IsAnimCtrlLoopAnim(AnimController controller)
    {
        int playMode = GetPlayMode(controller);
        return playMode == PlayModeForwardLoop || playMode == PlayModeReverseLoop;
    }

    // OFFSET 0x020A8AD8
    public static void SrtControl_SetTrans(SrtControl control, short x, short y)
    {
        if (control.Type != SrtTypeNormal)
        {
            return;
        }
        control.EnableFlag |= SrtTrans;
        control.Px = x;
        control.Py = y;
    }

    // OFFSET 0x020A8AFC
    public static void SrtControl_SetRotZ(SrtControl control, ushort rotZ)
    {
        if (control.Type != SrtTypeNormal)
        {
            return;
        }
        control.EnableFlag |= SrtRotZ;
        control.RotZ = rotZ;
    }

    // OFFSET 0x020A8B1C
    public static void SrtControl_SetScale(SrtControl control, int sx, int sy)
    {
        if (control.Type != SrtTypeNormal)
        {
            return;
        }
        control.EnableFlag |= SrtScale;
        control.Sx = sx;
        control.Sy = sy;
    }

    // OFFSET 0x020A8B3C
    public static void SrtControl_Init(SrtControl control, int type)
    {
        control.Type = type;
        SrtControl_InitData(control);
    }

    // OFFSET 0x020A8B4C
    private static void SrtControl_InitData(SrtControl control)
    {
        control.Px = 0;
        control.Py = 0;
        control.RotZ = 0;
        control.EnableFlag = 0;
        control.Sx = Fx32.One;
        control.Sy = Fx32.One;
    }

    // OFFSET 0x020A8B74
    public static void G2dNode_Init(G2dNode node, int type)
    {
        node.Content = null;
        node.Type = type;
        node.Visible = 1;
        SrtControl_Init(node.Srt, SrtTypeNormal);
    }

    private static void ApplyElementSrt(AnimSequence sequence, int element, SrtControl control)
    {
        byte[] file = sequence.File;
        int elementType = sequence.ElementType;
        SrtControl_Init(control, SrtTypeNormal);
        if (elementType == ElementIndex)
        {
            return;
        }
        if (elementType == ElementIndexT)
        {
            SrtControl_SetTrans(control, (short)Bin.U16(file, element + 4), (short)Bin.U16(file, element + 6));
            return;
        }
        SrtControl_SetScale(control, (int)Bin.U32(file, element + 4), (int)Bin.U32(file, element + 8));
        SrtControl_SetRotZ(control, (ushort)Bin.U16(file, element + 2));
        SrtControl_SetTrans(control, (short)Bin.U16(file, element + 0xC), (short)Bin.U16(file, element + 0xE));
    }

    // OFFSET 0x020A8B98
    private static void CellAnimation_ApplyCurrentAnimResult(CellAnimation anim)
    {
        AnimController controller = anim.AnimCtrl;
        if (FrameDuration(controller, controller.ActiveCurrent) == 0)
        {
            return;
        }
        AnimSequence sequence = controller.Sequence!;
        int element = GetAnimCtrlCurrentElement(controller);
        ushort cellIndex = (ushort)Bin.U16(sequence.File, element);
        CellDataBank cellBank = anim.CellBank!;
        anim.CurrentCell = GetCellDataByIdx(cellBank, cellIndex);
        ApplyElementSrt(sequence, element, anim.Srt);
        int bankData = cellBank.SectionOffset + BlockHeaderSize;
        int transferOffset = (int)Bin.U32(cellBank.File, bankData + 0xC);
        if (transferOffset == 0)
        {
            return;
        }
        if (anim.TransferHandle == NoTransferHandle)
        {
            return;
        }
        int transferData = bankData + transferOffset;
        int entry = transferData + (int)Bin.U32(cellBank.File, transferData + 4) + cellIndex * 8;
        SetCellTransferStateRequested(anim.TransferHandle, Bin.U32(cellBank.File, entry), Bin.U32(cellBank.File, entry + 4));
    }

    // OFFSET 0x020A8C88
    public static void InitCellAnimation(CellAnimation anim, AnimSequence? sequence, CellDataBank? cellBank)
    {
        anim.CellBank = cellBank;
        anim.TransferHandle = NoTransferHandle;
        SrtControl_Init(anim.Srt, SrtTypeNormal);
        InitAnimCtrl(anim.AnimCtrl);
        SetCellAnimationSequence(anim, sequence);
    }

    // OFFSET 0x020A8CC4
    public static void InitCellAnimationVramTransfered(CellAnimation anim, AnimSequence? sequence, CellDataBank? cellBank, uint handle, uint dest3D, uint destMain, uint destSub, byte[] source2D, byte[]? source3D, uint size)
    {
        int bankData = cellBank!.SectionOffset + BlockHeaderSize;
        int transferData = bankData + (int)Bin.U32(cellBank.File, bankData + 0xC);
        InitCellTransferState(handle, dest3D, destMain, destSub, Bin.U32(cellBank.File, transferData), source2D, source3D, size);
        anim.TransferHandle = handle;
        anim.CellBank = cellBank;
        anim.TransferHandle = handle;
        SrtControl_Init(anim.Srt, SrtTypeNormal);
        InitAnimCtrl(anim.AnimCtrl);
        SetCellAnimationSequence(anim, sequence);
    }

    // OFFSET 0x020A8D48
    public static void SetCellAnimationSequence(CellAnimation anim, AnimSequence? sequence)
    {
        BindAnimCtrl(anim.AnimCtrl, sequence);
        CellAnimation_ApplyCurrentAnimResult(anim);
    }

    // OFFSET 0x020A8D60
    public static void TickCellAnimation(CellAnimation anim, int frames)
    {
        if (TickAnimCtrl(anim.AnimCtrl, frames))
        {
            CellAnimation_ApplyCurrentAnimResult(anim);
        }
    }

    // OFFSET 0x020A8D80
    public static void SetCellAnimationCurrentFrame(CellAnimation anim, ushort frame)
    {
        if (SetAnimCtrlCurrentFrame(anim.AnimCtrl, frame))
        {
            CellAnimation_ApplyCurrentAnimResult(anim);
        }
    }

    private static uint SequenceTotalDuration(AnimSequence sequence)
    {
        uint total = 0;
        int frames = sequence.Frames;
        int count = sequence.NumFrames;
        for (int i = 0; i < count; i++)
        {
            total += (uint)Bin.U16(sequence.File, frames + i * FrameSize + 4);
        }
        return total;
    }

    private static void StartNodeAnimation(MultiCellAnimation anim, CellAnimation cellAnim, int nodeInfo, uint frames)
    {
        byte[] file = anim.MultiCellBank!.File;
        AnimSequence? sequence = GetAnimSequenceByIdx(anim.AnimDataBank, (ushort)Bin.U16(file, nodeInfo));
        SetCellAnimationSequence(cellAnim, sequence);
        cellAnim.AnimCtrl.Active = 1;
        if ((Bin.U16(file, nodeInfo + 6) & 0xF) != NodePlayModeContinue)
        {
            return;
        }
        uint total = SequenceTotalDuration(sequence!);
        if (IsAnimCtrlLoopAnim(cellAnim.AnimCtrl))
        {
            TickCellAnimation(cellAnim, (int)((frames % total) << Fx32.Shift));
        }
        else
        {
            if (frames < total)
            {
                total = frames;
            }
            TickCellAnimation(cellAnim, (int)(total << Fx32.Shift));
        }
    }

    // OFFSET 0x020A9104
    private static void MultiCellAnimation_SetCurrentMultiCell(MultiCellAnimation anim, int multiCell, uint frames)
    {
        MultiCellDataBank bank = anim.MultiCellBank!;
        byte[] file = bank.File;
        int bankData = bank.SectionOffset + BlockHeaderSize;
        int hierarchyBase = bankData + (int)Bin.U32(file, bankData + 8);
        anim.CurrentMultiCell = multiCell;
        int numNodes = Bin.U16(file, multiCell);
        int hierarchy = hierarchyBase + (int)Bin.U32(file, multiCell + 4);
        if (anim.McType == McTypeShareCellAnim)
        {
            CellAnimation[] cellAnims = anim.SharedCellAnims!;
            int numCellAnims = Bin.U16(file, multiCell + 2);
            for (int i = 0; i < numCellAnims; i++)
            {
                cellAnims[i].Inited = 0;
            }
            for (int i = 0; i < numNodes; i++)
            {
                int nodeInfo = hierarchy + i * HierarchySize;
                int cellAnimIdx = (Bin.U16(file, nodeInfo + 6) & 0xFF00) >> 8;
                CellAnimation cellAnim = cellAnims[cellAnimIdx];
                if (cellAnim.Inited == 0)
                {
                    StartNodeAnimation(anim, cellAnim, nodeInfo, frames);
                    cellAnim.Inited = 1;
                }
            }
            return;
        }
        G2dNode[] nodes = anim.Nodes!;
        for (int i = 0; i < numNodes; i++)
        {
            int nodeInfo = hierarchy + i * HierarchySize;
            CellAnimation cellAnim = nodes[i].Content!;
            StartNodeAnimation(anim, cellAnim, nodeInfo, frames);
            nodes[i].Visible = 1;
            SrtControl_SetTrans(nodes[i].Srt, (short)Bin.U16(file, nodeInfo + 2), (short)Bin.U16(file, nodeInfo + 4));
        }
    }

    private static void MultiCellAnimation_ApplyCurrentAnimResult(MultiCellAnimation anim)
    {
        AnimSequence sequence = anim.AnimCtrl.Sequence!;
        int element = GetAnimCtrlCurrentElement(anim.AnimCtrl);
        int multiCell = GetMultiCellDataByIdx(anim.MultiCellBank!, (ushort)Bin.U16(sequence.File, element));
        ApplyElementSrt(sequence, element, anim.Srt);
        MultiCellAnimation_SetCurrentMultiCell(anim, multiCell, anim.TotalVideoFrame);
    }

    // OFFSET 0x020A9380
    public static void SetAnimSequenceToMCAnimation(MultiCellAnimation anim, AnimSequence? sequence)
    {
        BindAnimCtrl(anim.AnimCtrl, sequence);
        anim.TotalVideoFrame = 0;
        if (FrameDuration(anim.AnimCtrl, anim.AnimCtrl.ActiveCurrent) == 0)
        {
            return;
        }
        MultiCellAnimation_ApplyCurrentAnimResult(anim);
    }

    // OFFSET 0x020A9438
    public static ushort GetMCBankNumNodesRequired(MultiCellDataBank? bank)
    {
        byte[] file = bank!.File;
        int data = bank.SectionOffset + BlockHeaderSize;
        ushort required = 0;
        int count = Bin.U16(file, data);
        for (ushort i = 0; i < count; i++)
        {
            ushort numNodes = (ushort)Bin.U16(file, GetMultiCellDataByIdx(bank, i));
            if (numNodes > required)
            {
                required = numNodes;
            }
        }
        return required;
    }

    // OFFSET 0x020A948C
    private static void MultiCellAnimation_TickCellAnimations(MultiCellAnimation anim, int frames)
    {
        byte[] file = anim.MultiCellBank!.File;
        if (anim.McType == McTypeShareCellAnim)
        {
            int numCellAnims = Bin.U16(file, anim.CurrentMultiCell + 2);
            for (int i = 0; i < numCellAnims; i++)
            {
                TickCellAnimation(anim.SharedCellAnims![i], frames);
            }
            return;
        }
        int numNodes = Bin.U16(file, anim.CurrentMultiCell);
        for (int i = 0; i < numNodes; i++)
        {
            TickCellAnimation(anim.Nodes![i].Content!, frames);
        }
    }

    // OFFSET 0x020A952C
    public static void TickMCAnimation(MultiCellAnimation anim, int frames)
    {
        AnimController controller = anim.AnimCtrl;
        int duration = FrameDuration(controller, controller.Current);
        if (TickAnimCtrl(controller, frames))
        {
            anim.TotalVideoFrame += (ushort)duration;
            if (FrameDuration(controller, controller.ActiveCurrent) == 0)
            {
                return;
            }
            MultiCellAnimation_ApplyCurrentAnimResult(anim);
            return;
        }
        MultiCellAnimation_TickCellAnimations(anim, frames);
    }

    // OFFSET 0x020A960C
    public static void SetMCAnimationCurrentFrame(MultiCellAnimation anim, ushort frame)
    {
        if (!SetAnimCtrlCurrentFrame(anim.AnimCtrl, frame))
        {
            return;
        }
        if (FrameDuration(anim.AnimCtrl, anim.AnimCtrl.ActiveCurrent) == 0)
        {
            return;
        }
        MultiCellAnimation_ApplyCurrentAnimResult(anim);
    }

    // OFFSET 0x020A96C0
    public static void InitMCAnimation(MultiCellAnimation anim, G2dNode[] nodes, CellAnimation[] cellAnims, ushort numNodes, AnimBankData? cellAnimBank, CellDataBank? cellBank, MultiCellDataBank? multiCellBank)
    {
        anim.McType = McTypeNodes;
        anim.CurrentMultiCell = 0;
        anim.AnimDataBank = cellAnimBank;
        anim.Nodes = nodes;
        for (ushort i = 0; i < numNodes; i++)
        {
            G2dNode_Init(nodes[i], 1);
            nodes[i].Content = cellAnims[i];
            InitCellAnimation(cellAnims[i], GetAnimSequenceByIdx(cellAnimBank, 0), cellBank);
        }
        InitAnimCtrl(anim.AnimCtrl);
        anim.MultiCellBank = multiCellBank;
        SrtControl_Init(anim.Srt, SrtTypeNormal);
        anim.TotalVideoFrame = 0;
    }
}
