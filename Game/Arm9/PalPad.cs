using PlatinumSharp.Nitro;

namespace PlatinumSharp.Game;

public sealed class PalPadEntry
{
    public const int Size = 0x88;
    public const int NameLength = 8;
    public const int AssociatedCount = 16;

    private const int TrainerIDOffset = 0x10;
    private const int InfoOffset = 0x14;
    private const int InfoSize = 4;
    private const int AssociatedOffset = 0x18;
    private const int TailOffset = 0x58;
    private const int TailSize = 0x30;

    public readonly ushort[] Name = new ushort[NameLength];
    public uint TrainerID;
    public readonly byte[] Info = new byte[InfoSize];
    public readonly uint[] AssociatedTrainerIDs = new uint[AssociatedCount];
    public readonly byte[] Tail = new byte[TailSize];

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < NameLength; i++)
        {
            Bin.W16(body, offset + i * 2, Name[i]);
        }
        Bin.W32(body, offset + TrainerIDOffset, TrainerID);
        Array.Copy(Info, 0, body, offset + InfoOffset, InfoSize);
        for (int i = 0; i < AssociatedCount; i++)
        {
            Bin.W32(body, offset + AssociatedOffset + i * 4, AssociatedTrainerIDs[i]);
        }
        Array.Copy(Tail, 0, body, offset + TailOffset, TailSize);
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < NameLength; i++)
        {
            Name[i] = (ushort)Bin.U16(body, offset + i * 2);
        }
        TrainerID = Bin.U32(body, offset + TrainerIDOffset);
        Array.Copy(body, offset + InfoOffset, Info, 0, InfoSize);
        for (int i = 0; i < AssociatedCount; i++)
        {
            AssociatedTrainerIDs[i] = Bin.U32(body, offset + AssociatedOffset + i * 4);
        }
        Array.Copy(body, offset + TailOffset, Tail, 0, TailSize);
    }

    public void CopyFrom(PalPadEntry source)
    {
        Array.Copy(source.Name, Name, NameLength);
        TrainerID = source.TrainerID;
        Array.Copy(source.Info, Info, InfoSize);
        Array.Copy(source.AssociatedTrainerIDs, AssociatedTrainerIDs, AssociatedCount);
        Array.Copy(source.Tail, Tail, TailSize);
    }

    public void Clear()
    {
        Array.Clear(Name);
        TrainerID = 0;
        Array.Clear(Info);
        Array.Clear(AssociatedTrainerIDs);
        Array.Clear(Tail);
    }
}

public sealed class PalPad : ISaveBlock
{
    public const int EntryCount = 16;
    public const int NotFriend = 0;
    public const int DirectFriend = 1;
    public const int FriendOfFriendBase = 2;

    private const int RegionOffset = 0x68;
    private const ushort EndOfString = 0xFFFF;

    public readonly PalPadEntry[] Entries = new PalPadEntry[EntryCount];

    public PalPad()
    {
        for (int i = 0; i < EntryCount; i++)
        {
            Entries[i] = new PalPadEntry();
        }
    }

    public void WriteTo(byte[] body, int offset)
    {
        for (int i = 0; i < EntryCount; i++)
        {
            Entries[i].WriteTo(body, offset + i * PalPadEntry.Size);
        }
    }

    public void ReadFrom(byte[] body, int offset)
    {
        for (int i = 0; i < EntryCount; i++)
        {
            Entries[i].ReadFrom(body, offset + i * PalPadEntry.Size);
        }
    }

    // OFFSET 0x02027F8C
    public static PalPad SaveData_GetPalPad(SaveData saveData)
    {
        return (PalPad)SaveData.SaveData_SaveTable(saveData, SaveDataTable.EntryPalPad);
    }

    // OFFSET 0x02027F98
    public static void PalPad_Init(PalPad palPad)
    {
        for (int i = 0; i < EntryCount; i++)
        {
            palPad.Entries[i].Clear();
            palPad.Entries[i].Name[0] = EndOfString;
        }
    }

    // OFFSET 0x02027FBC
    public static PalPadEntry PalPad_GetEntry(PalPad palPad, int index)
    {
        return palPad.Entries[index];
    }

    // OFFSET 0x02027FC4
    public static int PalPad_GetRegionByte(PalPad palPad, int index)
    {
        int position = RegionOffset + index;
        byte[] buffer = new byte[PalPadEntry.Size];
        palPad.Entries[position / PalPadEntry.Size].WriteTo(buffer, 0);
        return buffer[position % PalPadEntry.Size];
    }

    // OFFSET 0x02027FCC
    private static bool PalPad_AreEntriesEqual(PalPadEntry first, PalPadEntry second)
    {
        if (CharCode.CharCode_Compare(first.Name, 0, second.Name, 0) == 0 && first.TrainerID == second.TrainerID)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02027FEC
    public static void PalPad_PushEntries(PalPad destination, PalPadEntry[] newEntries, int count, int heapID)
    {
        PalPad merged = new();
        PalPad_Init(merged);
        int[] existing = new int[count];
        for (int i = 0; i < count; i++)
        {
            existing[i] = -1;
            for (int j = 0; j < EntryCount; j++)
            {
                if (PalPad_AreEntriesEqual(destination.Entries[j], newEntries[i]))
                {
                    existing[i] = j;
                }
            }
        }
        int used = 0;
        for (int i = 0; i < count; i++)
        {
            merged.Entries[used].CopyFrom(newEntries[i]);
            if (existing[i] >= 0)
            {
                destination.Entries[existing[i]].Name[0] = EndOfString;
            }
            used++;
        }
        for (int i = 0; i < EntryCount; i++)
        {
            if (destination.Entries[i].Name[0] == EndOfString)
            {
                continue;
            }
            merged.Entries[used].CopyFrom(destination.Entries[i]);
            used++;
            if (used >= EntryCount)
            {
                break;
            }
        }
        for (int i = 0; i < EntryCount; i++)
        {
            destination.Entries[i].CopyFrom(merged.Entries[i]);
        }
    }

    // OFFSET 0x020280E0
    public static int PalPad_TrainerIsFriend(PalPad palPad, uint trainerID)
    {
        for (int i = 0; i < EntryCount; i++)
        {
            if (trainerID == palPad.Entries[i].TrainerID)
            {
                return DirectFriend;
            }
        }
        for (int i = 0; i < EntryCount; i++)
        {
            for (int j = 0; j < PalPadEntry.AssociatedCount; j++)
            {
                if (trainerID == palPad.Entries[i].AssociatedTrainerIDs[j])
                {
                    return i + FriendOfFriendBase;
                }
            }
        }
        return NotFriend;
    }
}
