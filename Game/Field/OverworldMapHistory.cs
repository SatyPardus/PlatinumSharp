namespace PlatinumSharp.Game;

public static class OverworldMapHistory
{
    public const int HistoryPointerOffset = 0x00;
    public const int CurrentMapXOffset = 0x04;
    public const int CurrentMapZOffset = 0x05;
    public const int ItemsOffset = 0x06;
    public const int ItemMapXOffset = 0x00;
    public const int ItemMapZOffset = 0x01;
    public const int ItemFaceDirectionOffset = 0x02;
    public const int ItemIsSetOffset = 0x03;
    public const int ItemSize = 0x04;
    public const int Length = 6;
    public const int Size = 0x20;

    private const int MapTilesPerSide = 32;
    private const int FaceDirectionNone = 0xFF;

    // OFFSET 0x020556E8
    public static void OverworldMapHistory_PushViaWarp(object? mapHistory, int x, int z)
    {
        BufferView history = (BufferView)mapHistory!;
        byte mapX = (byte)(x / MapTilesPerSide);
        byte mapZ = (byte)(z / MapTilesPerSide);
        if (mapX == history.U8(CurrentMapXOffset) && mapZ == history.U8(CurrentMapZOffset))
        {
            return;
        }
        int historyPointer = (int)history.U32(HistoryPointerOffset);
        int item = ItemsOffset + historyPointer * ItemSize;
        history.W8(item + ItemMapXOffset, mapX);
        history.W8(item + ItemMapZOffset, mapZ);
        history.W8(item + ItemFaceDirectionOffset, FaceDirectionNone);
        history.W8(item + ItemIsSetOffset, 1);
        history.W32(HistoryPointerOffset, (uint)((historyPointer + 1) % Length));
    }

    // OFFSET 0x02055740
    public static void OverworldMapHistory_Push(object? mapHistory, int x, int z, int faceDirection)
    {
        BufferView history = (BufferView)mapHistory!;
        byte mapX = (byte)x;
        byte mapZ = (byte)z;
        if (mapX == history.U8(CurrentMapXOffset) && mapZ == history.U8(CurrentMapZOffset))
        {
            return;
        }
        int historyPointer = (int)history.U32(HistoryPointerOffset);
        int item = ItemsOffset + historyPointer * ItemSize;
        history.W8(item + ItemMapXOffset, mapX);
        history.W8(item + ItemMapZOffset, mapZ);
        history.W8(item + ItemFaceDirectionOffset, faceDirection);
        history.W8(item + ItemIsSetOffset, 1);
        history.W8(CurrentMapXOffset, mapX);
        history.W8(CurrentMapZOffset, mapZ);
        history.W32(HistoryPointerOffset, (uint)((historyPointer + 1) % Length));
    }
}
