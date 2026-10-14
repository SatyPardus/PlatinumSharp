using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class FieldTextureSlot
{
    public const int Size = 0x18;

    public uint TextureAddress;
    public uint TextureSize;
    public G3dTexture? AnimationTexture;
    public byte[]? AnimationFile;
    public int AnimationOffset;
    public bool HasAnimation;
    public ushort FrameIndex;
    public ushort FrameTimer;
}

public sealed class FieldTextureManager
{
    public const int SlotCount = 16;
    public const int Size = 0x188;

    public byte[]? AnimationList;
    public readonly FieldTextureSlot[] Slots = new FieldTextureSlot[SlotCount];

    public FieldTextureManager()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            Slots[i] = new FieldTextureSlot();
        }
    }
}

public static class FieldTextureResources
{
    private const int HeapIDField = 4;
    private const int NarcFieldAnimations = 0x9B;
    private const int AnimationListCount = 0x00;
    private const int AnimationListEntries = 0x04;
    private const int AnimationEntrySize = 0x34;
    private const int AnimationEntryFrames = 0x10;
    private const int FrameTextureIndex = 0;
    private const int FrameDuration = 1;
    private const int FrameSize = 2;
    private const byte FrameListEnd = 0xFF;
    private const int TransferTexture = 0;

    // OFFSET 0x021D5CB0
    public static FieldTextureManager FieldTextureManager_Create()
    {
        FieldTextureManager manager = new();
        for (int i = 0; i < FieldTextureManager.SlotCount; i++)
        {
            FieldTextureSlot slot = manager.Slots[i];
            slot.FrameIndex = 0;
            slot.FrameTimer = 0;
            slot.TextureAddress = 0;
            slot.AnimationTexture = null;
            slot.HasAnimation = false;
            slot.AnimationOffset = 0;
            slot.AnimationFile = null;
        }
        return manager;
    }

    // OFFSET 0x021D5CE4
    public static int FieldTextureManager_LoadTexture(FieldTextureManager manager, G3dTexture? texture)
    {
        int loaded = 0;
        NARC narc = NARC.NARC_ctor(NarcFieldAnimations, HeapIDField);
        manager.AnimationList = NARC.NARC_AllocAndReadWholeMember(narc, 0, HeapIDField);
        for (int i = 0; (uint)i < Bin.U32(manager.AnimationList, AnimationListCount); i++)
        {
            if (FieldTextureManager_LoadSlot(manager, texture, i, narc) >= 0)
            {
                loaded++;
            }
        }
        NARC.NARC_dtor(narc);
        return loaded;
    }

    // OFFSET 0x021D5D30
    private static int FieldTextureManager_LoadSlot(FieldTextureManager? manager, G3dTexture? texture, int animationIndex, NARC narc)
    {
        if (manager == null)
        {
            return -1;
        }
        int slotIndex = 0;
        while (slotIndex < FieldTextureManager.SlotCount)
        {
            if (manager.Slots[slotIndex].AnimationFile == null)
            {
                break;
            }
            slotIndex++;
        }
        if (slotIndex == FieldTextureManager.SlotCount || texture == null)
        {
            return -1;
        }
        FieldTextureSlot slot = manager.Slots[slotIndex];
        int entry = AnimationListEntries + animationIndex * AnimationEntrySize;
        string name = Bin.CString(manager.AnimationList!, entry);
        slot.TextureAddress = BillboardVramTransfer.GetTextureDataBufferFromResourceName(texture, name);
        if (slot.TextureAddress == 0)
        {
            return -1;
        }
        slot.TextureSize = BillboardVramTransfer.CalcTextureDataSizeFromResourceName(texture, name);
        slot.AnimationOffset = entry;
        slot.HasAnimation = true;
        slot.AnimationFile = NARC.NARC_AllocAndReadWholeMember(narc, (uint)(animationIndex + 1), HeapIDField);
        slot.AnimationTexture = G3d.GetTexture(slot.AnimationFile);
        return slotIndex;
    }

    // OFFSET 0x021D5DEC
    public static void FieldTextureManager_Update(FieldTextureManager? manager)
    {
        if (manager == null)
        {
            return;
        }
        for (int i = 0; i < FieldTextureManager.SlotCount; i++)
        {
            FieldTextureSlot slot = manager.Slots[i];
            if (slot.AnimationFile == null || !slot.HasAnimation)
            {
                continue;
            }
            byte[] list = manager.AnimationList!;
            int frames = slot.AnimationOffset + AnimationEntryFrames;
            if (list[frames + slot.FrameIndex * FrameSize + FrameDuration] > slot.FrameTimer)
            {
                slot.FrameTimer++;
                continue;
            }
            slot.FrameTimer = 0;
            slot.FrameIndex++;
            if (list[frames + slot.FrameIndex * FrameSize + FrameTextureIndex] == FrameListEnd)
            {
                slot.FrameIndex = 0;
            }
            int source = BillboardVramTransfer.GetTextureDataVRAMBuffer(slot.AnimationTexture, list[frames + slot.FrameIndex * FrameSize + FrameTextureIndex]);
            VramTransfer.VramTransfer_Request(TransferTexture, slot.TextureAddress, Slice(slot.AnimationTexture, source, slot.TextureSize), slot.TextureSize);
        }
    }

    private static byte[] Slice(G3dTexture? texture, int offset, uint size)
    {
        if (texture == null || offset < 0)
        {
            return Array.Empty<byte>();
        }
        int length = (int)Math.Min(size, texture.File.Length - offset);
        return texture.File.AsSpan(offset, Math.Max(0, length)).ToArray();
    }

    // OFFSET 0x021D5E5C
    public static void FieldTextureManager_FreeSlot(FieldTextureManager? manager, int slotIndex)
    {
        if (manager == null)
        {
            return;
        }
        FieldTextureSlot slot = manager.Slots[slotIndex];
        if (slot.HasAnimation)
        {
            slot.HasAnimation = false;
            slot.AnimationOffset = 0;
        }
        slot.FrameIndex = 0;
        slot.FrameTimer = 0;
    }

    // OFFSET 0x021D5E8C
    public static void FieldTextureManager_FreeAllSlots(FieldTextureManager? manager)
    {
        if (manager == null)
        {
            return;
        }
        for (int i = 0; i < FieldTextureManager.SlotCount; i++)
        {
            FieldTextureManager_FreeSlot(manager, i);
        }
        manager.AnimationList = null;
    }

    // OFFSET 0x021D5EAC
    public static void FieldTextureManager_Destroy(FieldTextureManager? manager)
    {
    }
}
