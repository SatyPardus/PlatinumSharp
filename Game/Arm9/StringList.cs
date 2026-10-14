using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class StringList
{
    // OFFSET 0x02013A04
    public static StringListEntry[] StringList_New(uint capacity, int heapID)
    {
        StringListEntry[] list = new StringListEntry[capacity + 1];
        uint i;
        for (i = 0; i < capacity; i++)
        {
            list[i] = new StringListEntry { Text = null, Index = 0 };
        }
        list[i] = new StringListEntry { Terminator = true, Index = (uint)heapID };
        return list;
    }

    // OFFSET 0x02013A3C
    public static void StringList_Free(StringListEntry[] list)
    {
        StringList_FreeStrings(list);
    }

    // OFFSET 0x02013A4C
    public static void StringList_AddFromMessageBank(StringListEntry[] list, MessageLoader loader, uint bankEntry, uint index)
    {
        StringListEntry? entry = StringList_FindFreeEntry(list, out int heapID);
        if (entry != null)
        {
            entry.Text = MessageLoader.MessageLoader_GetNewString(loader, bankEntry);
            entry.Index = index;
        }
    }

    // OFFSET 0x02013A6C
    public static void StringList_AddFromString(StringListEntry[] list, GameString? str, uint index)
    {
        StringListEntry? entry = StringList_FindFreeEntry(list, out int heapID);
        if (entry != null)
        {
            entry.Text = GameString.String_Clone(str, heapID);
            entry.Index = index;
        }
    }

    // OFFSET 0x02013A90
    public static void StringList_AddFromEntry(StringListEntry[] list, StringListEntry source)
    {
        StringListEntry? entry = StringList_FindFreeEntry(list, out int heapID);
        if (entry != null)
        {
            entry.Text = source.Text;
            entry.Callback = source.Callback;
            entry.Index = source.Index;
        }
    }

    // OFFSET 0x02013AAC
    private static StringListEntry? StringList_FindFreeEntry(StringListEntry[] list, out int heapID)
    {
        int i = 0;
        heapID = 0;
        while (list[i].Text != null || list[i].Callback != null || list[i].Terminator)
        {
            if (list[i].Terminator)
            {
                ErrorHandling.GF_AssertFail();
                return null;
            }
            i++;
        }
        StringListEntry entry = list[i];
        while (!list[i].Terminator)
        {
            i++;
        }
        heapID = (int)list[i].Index;
        return entry;
    }

    // OFFSET 0x02013AE8
    private static void StringList_FreeStrings(StringListEntry[] list)
    {
        for (int i = 0; !list[i].Terminator; i++)
        {
            if (list[i].Text == null)
            {
                break;
            }
            GameString.String_Free(list[i].Text);
            list[i].Text = null;
        }
    }
}
