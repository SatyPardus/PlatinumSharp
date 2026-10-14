namespace PlatinumSharp.Game;

public static class SplLists
{
    // OFFSET 0x020A2238
    public static T SPLList_Erase<T>(SplList<T> list, T node)
        where T : SplNode<T>
    {
        T? next = node.Next;
        if (next == null)
        {
            if (list.First == node)
            {
                list.First = null;
                list.Last = null;
            }
            else
            {
                node.Prev!.Next = null;
                list.Last = list.Last!.Prev;
            }
        }
        else if (list.First == node)
        {
            list.First = next;
            list.First.Prev = null;
        }
        else
        {
            next.Prev = node.Prev;
            node.Prev!.Next = node.Next;
        }
        list.Count--;
        return node;
    }

    // OFFSET 0x020A22B8
    public static T? SPLList_PopFront<T>(SplList<T> list)
        where T : SplNode<T>
    {
        T? node = list.First;
        if (node != null)
        {
            list.First = node.Next;
            if (list.First != null)
            {
                node.Next!.Prev = null;
            }
            else
            {
                list.First = null;
                list.Last = null;
            }
            list.Count--;
        }
        return node;
    }

    // OFFSET 0x020A2304
    public static void SPLList_PushFront<T>(SplList<T> list, T node)
        where T : SplNode<T>
    {
        if (list.First == null)
        {
            list.First = node;
            list.Last = node;
            node.Next = null;
            node.Prev = null;
        }
        else
        {
            node.Next = list.First;
            node.Prev = null;
            list.First.Prev = node;
            list.First = node;
        }
        list.Count++;
    }
}
