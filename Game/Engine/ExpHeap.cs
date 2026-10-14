using System.Runtime.CompilerServices;

namespace PlatinumSharp.Game;

public sealed class HeapBlock
{
    public ExpHeap? Owner;
    public uint Size;
    public byte GroupID;
    public byte[] Memory = Array.Empty<byte>();
}

public sealed class ExpHeap
{
    private const uint BlockHeaderSize = 0x10;
    private const uint HeapHeaderSize = 0x50;

    private static readonly ConditionalWeakTable<byte[], HeapBlock> blocks = new();

    private readonly HashSet<HeapBlock> allocated = new();

    public uint Size { get; private set; }

    public uint Used { get; private set; }

    public bool Destroyed { get; private set; }

    public int BlockCount => allocated.Count;

    public static ExpHeap? Create(uint size)
    {
        if (size < HeapHeaderSize + BlockHeaderSize)
        {
            return null;
        }
        return new ExpHeap { Size = size - HeapHeaderSize };
    }

    public void Destroy()
    {
        Destroyed = true;
        allocated.Clear();
        Used = 0;
    }

    private static uint Align(uint size, int alignment)
    {
        uint unit = (uint)Math.Abs(alignment);
        if (unit == 0)
        {
            unit = 4;
        }
        return (size + unit - 1) / unit * unit;
    }

    public HeapBlock? Alloc(uint size, int alignment, uint payloadSize)
    {
        if (size == 0)
        {
            size = 1;
        }
        uint total = Align(size, alignment) + BlockHeaderSize;
        if (Destroyed || Used + total > Size)
        {
            return null;
        }
        HeapBlock block = new() { Owner = this, Size = Align(size, alignment), Memory = new byte[payloadSize] };
        allocated.Add(block);
        blocks.Add(block.Memory, block);
        Used += total;
        return block;
    }

    public void Free(HeapBlock block)
    {
        if (allocated.Remove(block))
        {
            Used -= block.Size + BlockHeaderSize;
            blocks.Remove(block.Memory);
            block.Owner = null;
        }
    }

    public uint GetTotalFreeSize()
    {
        return Size - Used;
    }

    public uint Resize(HeapBlock block, uint size)
    {
        uint aligned = Align(size, 4);
        if (aligned > block.Size && Used + (aligned - block.Size) > Size)
        {
            return 0;
        }
        Used = Used - block.Size + aligned;
        block.Size = aligned;
        return aligned;
    }

    public static HeapBlock? FindBlock(byte[] memory)
    {
        return blocks.TryGetValue(memory, out HeapBlock? block) ? block : null;
    }
}
