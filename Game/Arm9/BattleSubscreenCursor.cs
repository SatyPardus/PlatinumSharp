namespace PlatinumSharp.Game;

public static class BattleSubscreenCursor
{
    public const int Size = 4;

    // OFFSET 0x0209C370
    public static byte[] BattleSubscreenCursor_New(int heapID)
    {
        byte[] cursorOn = Heap.Heap_Alloc(heapID, Size)!;
        Array.Clear(cursorOn, 0, Size);
        return cursorOn;
    }

    // OFFSET 0x0209C388
    public static void BattleSubscreenCursor_Free(byte[] cursorOn)
    {
        Heap.Heap_Free(cursorOn);
    }
}
