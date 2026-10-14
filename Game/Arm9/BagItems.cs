using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class BagItems
{
    public const int SlotSize = 4;
    public const int PocketItems = 0;
    public const int PocketMedicine = 1;
    public const int PocketBalls = 2;
    public const int PocketTMHMs = 3;
    public const int PocketBerries = 4;
    public const int PocketMail = 5;
    public const int PocketBattleItems = 6;
    public const int PocketKeyItems = 7;
    public const int PocketCount = 8;
    public const int PocketListEnd = 0xFF;

    private const int SlotItemOffset = 0;
    private const int SlotQuantityOffset = 2;
    private const int ItemParamFieldPocket = 5;
    private const int MaxQuantity = 999;
    private const int MaxQuantityTMHM = 99;
    private const int RegisteredItemOffset = 0x770;
    private const int ItemsOffset = 0x000;
    private const int ItemsSize = 0xA5;
    private const int KeyItemsOffset = 0x294;
    private const int KeyItemsSize = 0x32;
    private const int TMHMsOffset = 0x35C;
    private const int TMHMsSize = 0x64;
    private const int MailOffset = 0x4EC;
    private const int MailSize = 0x0C;
    private const int MedicineOffset = 0x51C;
    private const int MedicineSize = 0x28;
    private const int BerriesOffset = 0x5BC;
    private const int BerriesSize = 0x40;
    private const int BallsOffset = 0x6BC;
    private const int BallsSize = 0x0F;
    private const int BattleItemsOffset = 0x6F8;
    private const int BattleItemsSize = 0x1E;

    private static bool PocketRange(int pocket, out int offset, out int size)
    {
        switch (pocket)
        {
            case PocketItems:
                offset = ItemsOffset;
                size = ItemsSize;
                return true;
            case PocketMedicine:
                offset = MedicineOffset;
                size = MedicineSize;
                return true;
            case PocketBalls:
                offset = BallsOffset;
                size = BallsSize;
                return true;
            case PocketTMHMs:
                offset = TMHMsOffset;
                size = TMHMsSize;
                return true;
            case PocketBerries:
                offset = BerriesOffset;
                size = BerriesSize;
                return true;
            case PocketMail:
                offset = MailOffset;
                size = MailSize;
                return true;
            case PocketBattleItems:
                offset = BattleItemsOffset;
                size = BattleItemsSize;
                return true;
            case PocketKeyItems:
                offset = KeyItemsOffset;
                size = KeyItemsSize;
                return true;
            default:
                offset = 0;
                size = 0;
                return false;
        }
    }

    private static BufferView Slot(BufferView slots, uint index)
    {
        return new BufferView(slots.Buffer, slots.Offset + (int)index * SlotSize);
    }

    // OFFSET 0x0207D3C0
    public static Bag Bag_New(int heapID)
    {
        Bag bag = new();
        Bag.Bag_Init(bag);
        return bag;
    }

    // OFFSET 0x0207D3EC
    public static void Bag_Copy(Bag source, Bag destination)
    {
        destination.CopyFrom(0, source.Buffer, source.Offset, Bag.Size);
    }

    // OFFSET 0x0207D3FC
    public static uint Bag_GetRegisteredItem(Bag bag)
    {
        return bag.U32(RegisteredItemOffset);
    }

    // OFFSET 0x0207D404
    public static void Bag_SetRegisteredItem(Bag bag, uint item)
    {
        bag.W32(RegisteredItemOffset, item);
    }

    // OFFSET 0x0207D40C
    private static uint Bag_GetItemPocket(Bag bag, ushort item, ref BufferView? slots, ref uint size, int heapID)
    {
        uint pocket = (uint)Item.Item_LoadParam(item, ItemParamFieldPocket, heapID);
        if (PocketRange((int)pocket, out int offset, out int pocketSize))
        {
            slots = new BufferView(bag.Buffer, bag.Offset + offset);
            size = (uint)pocketSize;
        }
        return pocket;
    }

    // OFFSET 0x0207D4B4
    private static BufferView? Pocket_FindSlotToAddItem(BufferView slots, uint size, ushort item, ushort count, ushort maxQuantity)
    {
        int emptySlot = -1;
        for (uint i = 0; i < size; i++)
        {
            BufferView slot = Slot(slots, i);
            if (slot.U16(SlotItemOffset) == item)
            {
                if (count + slot.U16(SlotQuantityOffset) > maxQuantity)
                {
                    return null;
                }
                return slot;
            }
            if (emptySlot == -1 && slot.U16(SlotItemOffset) == 0 && slot.U16(SlotQuantityOffset) == 0)
            {
                emptySlot = (int)i;
            }
        }
        if (emptySlot == -1)
        {
            return null;
        }
        return Slot(slots, (uint)emptySlot);
    }

    // OFFSET 0x0207D518
    private static BufferView? Bag_FindSlotToAddItem(Bag bag, ushort item, ushort count, int heapID)
    {
        BufferView? slots = null;
        uint size = 0;
        uint pocket = Bag_GetItemPocket(bag, item, ref slots, ref size, heapID);
        if (pocket == PocketTMHMs)
        {
            return Pocket_FindSlotToAddItem(slots!, size, item, count, MaxQuantityTMHM);
        }
        return Pocket_FindSlotToAddItem(slots!, size, item, count, MaxQuantity);
    }

    // OFFSET 0x0207D55C
    public static bool Bag_CanFitItem(Bag bag, ushort item, ushort count, int heapID)
    {
        return Bag_FindSlotToAddItem(bag, item, count, heapID) != null;
    }

    // OFFSET 0x0207D570
    public static bool Bag_TryAddItem(Bag bag, ushort item, ushort count, int heapID)
    {
        BufferView? slot = Bag_FindSlotToAddItem(bag, item, count, heapID);
        if (slot == null)
        {
            return false;
        }
        slot.W16(SlotItemOffset, item);
        slot.W16(SlotQuantityOffset, slot.U16(SlotQuantityOffset) + count);
        BufferView? slots = slot;
        uint size = 0;
        uint pocket = Bag_GetItemPocket(bag, item, ref slots, ref size, heapID);
        if (pocket - PocketTMHMs <= 1)
        {
            Pocket_Sort(slots!, size);
        }
        return true;
    }

    // OFFSET 0x0207D5B8
    private static BufferView? Pocket_FindSlotWithQuantity(BufferView slots, uint size, ushort item, ushort count)
    {
        for (uint i = 0; i < size; i++)
        {
            BufferView slot = Slot(slots, i);
            if (slot.U16(SlotItemOffset) == item)
            {
                if (slot.U16(SlotQuantityOffset) >= count)
                {
                    return slot;
                }
                return null;
            }
        }
        return null;
    }

    // OFFSET 0x0207D5E8
    private static BufferView? Bag_FindSlotWithQuantity(Bag bag, ushort item, ushort count, int heapID)
    {
        BufferView? slots = null;
        uint size = 0;
        Bag_GetItemPocket(bag, item, ref slots, ref size, heapID);
        return Pocket_FindSlotWithQuantity(slots!, size, item, count);
    }

    // OFFSET 0x0207D60C
    public static bool Bag_TryRemoveItem(Bag bag, ushort item, ushort count, int heapID)
    {
        BufferView? slot = Bag_FindSlotWithQuantity(bag, item, count, heapID);
        if (slot == null)
        {
            return false;
        }
        slot.W16(SlotQuantityOffset, slot.U16(SlotQuantityOffset) - count);
        if (slot.U16(SlotQuantityOffset) == 0)
        {
            slot.W16(SlotItemOffset, 0);
        }
        BufferView? slots = slot;
        uint size = 0;
        Bag_GetItemPocket(bag, item, ref slots, ref size, heapID);
        Pocket_Compact(slots!, size);
        return true;
    }

    // OFFSET 0x0207D658
    public static bool Pocket_TryRemoveItem(BufferView slots, uint size, ushort item, ushort count)
    {
        BufferView? slot = Pocket_FindSlotWithQuantity(slots, size, item, count);
        if (slot == null)
        {
            return false;
        }
        slot.W16(SlotQuantityOffset, slot.U16(SlotQuantityOffset) - count);
        if (slot.U16(SlotQuantityOffset) == 0)
        {
            slot.W16(SlotItemOffset, 0);
        }
        Pocket_Compact(slots, size);
        return true;
    }

    // OFFSET 0x0207D688
    public static bool Bag_CanRemoveItem(Bag bag, ushort item, ushort count, int heapID)
    {
        return Bag_FindSlotWithQuantity(bag, item, count, heapID) != null;
    }

    // OFFSET 0x0207D69C
    public static bool Bag_PocketHasItems(Bag bag, int pocket)
    {
        if (!PocketRange(pocket, out int offset, out int size))
        {
            return false;
        }
        for (uint i = 0; i < (uint)size; i++)
        {
            if (bag.U16(offset + (int)i * SlotSize + SlotItemOffset) != 0)
            {
                return true;
            }
        }
        return false;
    }

    // OFFSET 0x0207D730
    public static ushort Bag_GetItemQuantity(Bag bag, ushort item, int heapID)
    {
        BufferView? slot = Bag_FindSlotWithQuantity(bag, item, 1, heapID);
        if (slot == null)
        {
            return 0;
        }
        return slot.U16(SlotQuantityOffset);
    }

    // OFFSET 0x0207D748
    public static ushort Pocket_GetItemQuantity(BufferView slots, uint size, ushort item)
    {
        BufferView? slot = Pocket_FindSlotWithQuantity(slots, size, item, 1);
        if (slot == null)
        {
            return 0;
        }
        return slot.U16(SlotQuantityOffset);
    }

    // OFFSET 0x0207D75C
    private static void Pocket_SwapSlots(BufferView first, BufferView second)
    {
        ushort item = first.U16(SlotItemOffset);
        ushort quantity = first.U16(SlotQuantityOffset);
        first.W16(SlotItemOffset, second.U16(SlotItemOffset));
        first.W16(SlotQuantityOffset, second.U16(SlotQuantityOffset));
        second.W16(SlotItemOffset, item);
        second.W16(SlotQuantityOffset, quantity);
    }

    // OFFSET 0x0207D780
    private static void Pocket_Compact(BufferView slots, uint size)
    {
        uint last = size - 1;
        if (last == 0)
        {
            return;
        }
        uint i = 0;
        do
        {
            BufferView slot = Slot(slots, i);
            for (uint j = i + 1; j < size; j++)
            {
                if (slot.U16(SlotQuantityOffset) == 0)
                {
                    Pocket_SwapSlots(slot, Slot(slots, j));
                }
            }
            i++;
        }
        while (i < last);
    }

    // OFFSET 0x0207D7CC
    private static void Pocket_Sort(BufferView slots, uint size)
    {
        uint last = size - 1;
        if (last == 0)
        {
            return;
        }
        uint i = 0;
        do
        {
            BufferView slot = Slot(slots, i);
            for (uint j = i + 1; j < size; j++)
            {
                BufferView other = Slot(slots, j);
                if (slot.U16(SlotQuantityOffset) == 0
                    || (other.U16(SlotQuantityOffset) != 0 && slot.U16(SlotItemOffset) > other.U16(SlotItemOffset)))
                {
                    Pocket_SwapSlots(slot, other);
                }
            }
            i++;
        }
        while (i < last);
    }

    // OFFSET 0x0207D824
    public static object? Bag_CreateContextWithPockets(Bag bag, byte[] pockets, int heapID)
    {
        object? context = sub_0207CB08((byte)heapID);
        for (int i = 0; pockets[i] != PocketListEnd; i++)
        {
            if (PocketRange(pockets[i], out int offset, out _))
            {
                sub_0207CB48(context, new BufferView(bag.Buffer, bag.Offset + offset), pockets[i], (byte)i);
            }
        }
        return context;
    }

    // OFFSET 0x0207D910
    public static BufferView? Bag_GetItemSlot(Bag bag, int pocket, int slot)
    {
        if (!PocketRange(pocket, out int offset, out int size))
        {
            return null;
        }
        if ((uint)slot >= (uint)size)
        {
            return null;
        }
        return new BufferView(bag.Buffer, bag.Offset + offset + slot * SlotSize);
    }

    // OFFSET 0x0207D990
    public static Bag SaveData_GetBag(SaveData saveData)
    {
        return (Bag)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryBag);
    }
}
